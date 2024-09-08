namespace Pedantic.Chess.DataGen
{
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    using Pedantic.Chess.NNUE;
    using Pedantic.Collections;
    using Pedantic.Utilities;

    public class DataGenThread
    {
#if DEBUG
        const int VERIF_NODE_LIMIT = 10000;
        const int HARD_NODE_LIMIT = 10000;
        const int SOFT_NODE_LIMIT = 4000;
#else
        const int VERIF_NODE_LIMIT = 20000;
        const int HARD_NODE_LIMIT = 2500000;
        const int SOFT_NODE_LIMIT = 5000;
#endif

        const int OPENING_MAX_SCORE = 1000;
        const int MAX_EVAL_FILTER = 2500;
        const int WIN_LOSS_ADJ_PLIES = 6;
        const int DRAW_ADJ_PLIES = 10;
        const int MAX_DRAW_FILTER = 10;
        const int MIN_OPENING_PLY = 16;
        public DataGenThread(BlockingCollection<PedanticFormat> dataQ, CancellationTokenSource cancelSource)
        {
            this.dataQ = dataQ;
            cancelToken = cancelSource.Token;
            board = new();
            bd = new();
            clock = new();
            cache = new(UciOptions.HashTableSize);
            eval = new(cache);
            ttCache = new();
            stack = new();
            uci = new(false, false);
            clock.Uci = uci;
            history = new(stack);
            listPool = new(() => new MoveList(history), (o) => { o.History = history; o.Clear(); }, MAX_PLY, 32);
            search = new(stack, eval, history, listPool, ttCache);
        }

        public int PositionCount
        {
            get => positionCount;
        }

        public void Generate()
        {
            clock.Go(int.MaxValue, false);
            clock.Infinite = true;
            thread = new Thread(new ThreadStart(GenerateProc));
            thread.Start();
        }

        public void Join()
        {
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }
        }

        private void GenerateProc()
        {
            int gameCount = 0;
            PedanticFormat pdata = default;
            ValueList<PedanticFormat> pdataList = new ValueList<PedanticFormat>(512);

            while (!cancelToken.IsCancellationRequested)
            {
                ClearCaches();

                ushort ply = (ushort)(8 + (gameCount & 0x03));
                board.RandomStart(ply);
                bool firstPlySearch = true;
                short eval = 0;
                int winPlies = 0, lossPlies = 0, drawPlies = 0;
                Wdl wdl;

                if (board.Hash != board.CalculateHash())
                {
                    throw new Exception("Zobrist key corruption.");
                }

                while (!board.IsGameOver(out wdl))
                {
                    stack.Initialize(board, history);

                    ulong hash = board.Hash;
                    string fen = board.ToFenString();

                    if (firstPlySearch)
                    {
                        // do a reasonably sufficient search to insure that the opening 
                        // is probably not busted from the get-go
                        search.Initialize(board, clock, uci, false, MAX_PLY, HARD_NODE_LIMIT, VERIF_NODE_LIMIT);
                    }
                    else
                    {
                        search.Initialize(board, clock, uci, false, MAX_PLY, HARD_NODE_LIMIT, SOFT_NODE_LIMIT);
                    }
                        
                    search.Search();
                    eval = (short)search.Score;

                    if (hash != board.Hash)
                    {
                        Console.Error.WriteLine($"Board was not restored on search completion: Old Hash {hash} != New Hash {board.Hash}.");
                        Console.Error.WriteLine($"Old FEN: {fen}");
                        Console.Error.WriteLine($"New FEN: {board.ToFenString()}");
                        Console.Error.WriteLine($"Best move: {search.PV[0]}");

                        wdl = Wdl.Incomplete;
                        break;
                    }

                    if (board.SideToMove == Color.Black)
                    {
                        // eval is always from white's perspective
                        eval = (short)-eval;
                    }

                    Move bestMove = search.PV[0];

                    if (firstPlySearch && Math.Abs(eval) > OPENING_MAX_SCORE)
                    {
                        // if the first search after opening is out of range just abandon 
                        // the game
                        wdl = Wdl.Incomplete;
                        break;
                    }
                    else
                    {
                        firstPlySearch = false;
                    }

                    if (eval > MAX_EVAL_FILTER)
                    {
                        winPlies++;
                        lossPlies = 0;
                        drawPlies = 0;
                    }
                    else if (eval < -MAX_EVAL_FILTER)
                    {
                        lossPlies++;
                        winPlies = 0;
                        drawPlies = 0;
                    }
                    else if (ply > 80 && Math.Abs(eval) < MAX_DRAW_FILTER)
                    {
                        drawPlies++;
                        winPlies = 0;
                        lossPlies = 0;
                    }
                    else
                    {
                        winPlies = 0;
                        lossPlies = 0;
                        drawPlies = 0;
                    }

                    if (eval >= MIN_TABLEBASE_WIN || (eval > MAX_EVAL_FILTER && winPlies >= WIN_LOSS_ADJ_PLIES))
                    {
                        wdl = Wdl.Win;
                        break;
                    }
                    else if (eval <= MAX_TABLEBASE_LOSS || (eval < -MAX_EVAL_FILTER && lossPlies >= WIN_LOSS_ADJ_PLIES))
                    {
                        wdl = Wdl.Loss;
                        break;
                    }
                    else if (ply > 80 && Math.Abs(eval) < MAX_DRAW_FILTER && drawPlies >= DRAW_ADJ_PLIES)
                    {
                        wdl = Wdl.Draw;
                        break;
                    }

                    if (bestMove.IsQuiet && !board.IsChecked() && ply > MIN_OPENING_PLY && Math.Abs(eval) < MAX_EVAL_FILTER && board.All.PopCount > 4)
                    {
                        // use dummy values for max ply and result. they will be replaced with correct 
                        // values at the conclusion of the game
                        pdata = board.ToBinary(512, eval, Result.Draw);
                        bd.LoadBinary(ref pdata);
                        if (bd.Hash != board.Hash)
                        {
                            Console.Error.WriteLine($"Corrupted zobrist hash detected in output: {board.ToFenString()}");
                            hash = board.CalculateHash();
                            if (board.Hash != hash)
                            {
                                Console.Error.WriteLine("Corruption found in source board.");
                            }

                            if (bd.Hash != hash)
                            {
                                Console.Error.WriteLine("Corruption found in output board.");
                            }

                            Console.Error.WriteLine($"Last moves: {board.PrevLastMove}, {board.LastMove}");
                            Console.Error.WriteLine("Abandoning game.");
                            wdl = Wdl.Incomplete;
                            break;
                        }
                        pdataList.Add(ref pdata);
                    }

                    board.MakeMove(bestMove);
                    ply++;
                }

                // only save position from completed games
                if (wdl != Wdl.Incomplete)
                {
                    Result result = wdl switch
                    {
                        Wdl.Win => Result.Win,
                        Wdl.Loss => Result.Loss,
                        _ => Result.Draw
                    };

                    for (int n = 0; n < pdataList.Count; n++)
                    {
                        pdataList[n].MaxPly = ply;
                        pdataList[n].Wdl = result;
                        pdataList[n].Filter = FilterPositionByEval(pdataList[n].Eval, result);
                    }

                    int newPositions = Write(pdataList.Where(pd => !pd.Filter));
                    Interlocked.Add(ref positionCount, newPositions);
                }

                pdataList.Clear();
                gameCount++;
            }
        }

        private int Write(IEnumerable<PedanticFormat> pdata)
        {
            int written = 0;

            lock (dataQ)
            {
                foreach (var pd in pdata)
                {
                    dataQ.Add(pd);
                    written++;
                }
            }

            return written;
        }

        private void ClearCaches()
        {
            ttCache.Clear();
            cache.Clear();
            history.Clear();
        }

        private static bool FilterPositionByEval(short eval, Result result)
        {
            return (eval < MAX_DRAW_FILTER && result == Result.Win) ||
                   (eval > -MAX_DRAW_FILTER && result == Result.Loss) ||
                   (Math.Abs(eval) > MAX_DRAW_FILTER && result == Result.Draw);
        }

        private readonly Board board;
        private readonly Board bd;
        private readonly BasicSearch search;
        private readonly GameClock clock;
        private readonly EvalCache cache;
        private readonly NnueEval eval;
        private readonly SearchStack stack;
        private readonly History history;
        private readonly ObjectPool<MoveList> listPool;
        private readonly TtCache ttCache;
        private readonly Uci uci;
        private readonly BlockingCollection<PedanticFormat> dataQ;
        private readonly CancellationToken cancelToken;
        private Thread? thread = null;
        private volatile int positionCount;
    }
}
