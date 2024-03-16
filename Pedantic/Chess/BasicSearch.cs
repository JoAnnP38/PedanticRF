using System.Runtime.CompilerServices;

using Pedantic.Collections;
using Pedantic.Utilities;
using Pedantic.Chess.HCE;
using Pedantic.Tablebase;

namespace Pedantic.Chess
{
    public sealed class BasicSearch : IInitialize
    {
        internal const int CHECK_TC_NODES_MASK = 1023;
        internal const int WAIT_TIME = 50;
        internal const int LMR_MAX_MOVES = 64;
        internal const double LMR_MIN = 0.0;
        internal const double LMR_MAX = 15.0;
        internal const int LMP_MAX_DEPTH_CUTOFF = 11;

        #region Constructors

        public BasicSearch(SearchStack searchStack, Board board, GameClock clock, HceEval eval, History history,
            ObjectPool<MoveList> listPool, TtCache ttCache, int maxDepth, long maxNodes = long.MaxValue - 100)
        {
            ss = searchStack;
            this.board = board;
            this.clock = clock;
            this.eval = eval;
            this.history = history;
            this.listPool = listPool;
            this.ttCache = ttCache;
            this.maxDepth = maxDepth;
            this.maxNodes = maxNodes;
        }

        #endregion

        #region Accessors

        public bool CanPonder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        public int Depth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public int Elapsed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public bool MustAbort
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => NodesVisited >= maxNodes ||
                   ((NodesVisited & CHECK_TC_NODES_MASK) == 0 && clock.CheckTimeBudget());
        }

        public long NodesVisited
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public IList<Move> PV
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return pv;
            }
        }


        public int Score
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public int SelDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public Uci Uci
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => uci;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => uci = value;
        }

        public int DrawScore
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)(8 - (NodesVisited & 0x7) + UciOptions.Contempt);
        }

        #endregion

        #region Search Methods

        public void Search()
        {
            string position = board.ToFenString();
            MoveList list = listPool.Rent();

            try
            {
                int alpha, beta, result;
                long startNodes = 0;
                Move ponderMove = Move.NullMove;
                history.SetContext(0);
                oneLegalMove = board.OneLegalMove(list, out Move bestMove);
                if (!oneLegalMove)
                {
                    bestMove = Move.NullMove;
                }
                startDateTime = DateTime.Now;

                while (++Depth <= maxDepth && clock.CanSearchDeeper())
                {
                    int iAlpha = 0, iBeta = 0;
                    clock.StartInterval();
                    SelDepth = 0;
                    list.SortAll();

                    do
                    {
                        SetAspWindow(Depth, iAlpha, iBeta, out alpha, out beta);
                        result = SearchRoot(list, alpha, beta, Depth);

                        if (wasAborted)
                        {
                            break;
                        }

                        if (result <= alpha)
                        {
                            ++iAlpha;
                            ReportOOBSearchResults(result, Bound.Upper, ref bestMove, ref ponderMove);
                        }
                        else if (result >= beta)
                        {
                            ++iBeta;
                            ReportOOBSearchResults(result, Bound.Lower, ref bestMove, ref ponderMove);
                        }                    
                    } while (result <= alpha || result >= beta);

                    if (wasAborted)
                    {
                        break;
                    }

                    startNodes = NodesVisited;
                    Score = result;
                    ReportSearchResults(ref bestMove, ref ponderMove);

                    if (oneLegalMove && Depth >= UciOptions.OneMoveMaxDepth && !UciOptions.AnalyseMode)
                    {
                        break;
                    }
                }

                // If program was pondering next move and the search loop was exited for 
                // reasons not due to the client telling us to stop, then sleep until 
                // we get a stop from the client (i.e. Engine will change the Infinite
                // property to false resulting in CanSearchDeeper returning false.)
                if (CanPonder)
                {
                    bool waiting = false;
                    while (clock.Infinite && !wasAborted)
                    {
                        waiting = true;
                        Thread.Sleep(WAIT_TIME);
                    }

                    if (waiting)
                    {
                        ReportSearchResults(ref bestMove, ref ponderMove);
                    }
                }

                Uci.Usage(cpuStats.CpuLoad);
                Uci.Debug("Incrementing hash table version.");
                ttCache.IncrementGeneration();
                Uci.BestMove(bestMove, CanPonder ? ponderMove : Move.NullMove);
            }
            catch (Exception ex)
            {
                string msg =
                    $"Search: Unexpected exception occurred on position '{position}'.";
                Console.Error.WriteLine(msg);
                Console.Error.WriteLine(ex.ToString());
                Uci.Log(msg);
                Util.TraceError(ex.ToString());
                throw;            
            }
            finally
            {
                listPool.Return(list);
            }
        }

        private int SearchRoot(MoveList list, int alpha, int beta, int depth)
        {
            int originalAlpha = alpha;
            bool inCheck = ss[-1].IsCheckingMove;
            ref SearchItem ssItem = ref ss[0];
            depth = Math.Min(depth, MAX_PLY - 1);
            pvTable.InitPly(0);

            if (wasAborted || MustAbort)
            {
                wasAborted = true;
                return 0;
            }

            int score;
            Move bestMove = Move.NullMove;
            int bestMoveIndex = -1;
            bool alphaRaised = false;
            int expandedNodes = 0, bestScore = -INFINITE_WINDOW;
            board.PushBoardState();

            for (int n = 0; n < list.Count; n++)
            {
                Move move = list[n];
                if (!board.MakeMoveNs(move))
                {
                    continue;
                }

                ttCache.Prefetch(board.Hash); // do prefetch before we need the ttItem
                expandedNodes++;
                NodesVisited++;

                if (startReporting || (DateTime.Now - startDateTime).TotalMilliseconds >= 1000)
                {
                    startReporting = true;
                    Uci.CurrentMove(depth, move, expandedNodes, NodesVisited, ttCache.Usage);
                }

                bool checkingMove = board.IsChecked();
                bool isQuiet = move.IsQuiet;
                bool interesting = inCheck || checkingMove || !isQuiet || !alphaRaised;

                ssItem.IsCheckingMove = checkingMove;
                ssItem.Move = move;
                ssItem.Continuation = history.GetContinuation(move);
                
                int R = 0;
                if (!interesting)
                {
                    R = LMR[Math.Min(depth, MAX_PLY - 1), Math.Min(expandedNodes - 1, LMR_MAX_MOVES - 1)];
                }

                if (!alphaRaised)
                {
                    score = -Search(-beta, -alpha, depth - 1, 1);
                }
                else
                {
                    score = -Search(-alpha - 1, -alpha, Math.Max(depth - R - 1, 0), 1);

                    if (score > alpha && R > 0)
                    {
                        score = -Search(-alpha - 1, -alpha, depth - 1, 1);
                    }

                    if (score > alpha)
                    {
                        score = -Search(-beta, -alpha, depth - 1, 1);
                    }
                }

                list.SetScore(n, score);
                board.UnmakeMoveNs();

                if (wasAborted)
                {
                    break;
                }

                if (score > bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                    bestMoveIndex = n;

                    if (score > alpha)
                    {
                        alphaRaised = true;
                        alpha = score;

                        if (score >= beta)
                        {
                            break;
                        }

                        pvTable.MergeMove(0, move);
                    }
                }
            }

            if (bestMoveIndex >= 0)
            {
                list.SetScore(bestMoveIndex, PV_BONUS);
            }

            board.PopBoardState();

            if (wasAborted)
            {
                return 0;
            }

            if (expandedNodes == 0)
            {
                return inCheck ? -CHECKMATE_SCORE : 0;
            }

            ttCache.Store(board.Hash, depth, 0, originalAlpha, beta, bestScore, bestMove);
            return bestScore;
        }

        private int Search(int alpha, int beta, int depth, int ply, bool canNull = true)
        {
            pvTable.InitPly(ply);
            SelDepth = Math.Max(SelDepth, ply);

            if (wasAborted || MustAbort)
            {
                wasAborted = true;
                return 0;
            }

            if (ply >= MAX_PLY - 1)
            {
                return eval.Compute(board, alpha, beta);
            }
            if (depth <= 0)
            {
                return Quiesce(alpha, beta, ply);
            }

            var rep = board.PositionRepeated();
            if (rep.Repeated || rep.OverFiftyMoves)
            {
                return DrawScore;
            }

            int score;
            int originalAlpha = alpha;
            bool isPv = beta - alpha > 1;
            bool inCheck = ss[ply - 1].IsCheckingMove;
            bool canPrune = false;
            ref SearchItem ssItem = ref ss[ply];
            int evaluation = ssItem.Eval = NO_SCORE;
            bool improving = false;

            // mate distance pruning
            alpha = Math.Max(alpha, -CHECKMATE_SCORE + ply);
            beta = Math.Min(beta, CHECKMATE_SCORE - ply - 1);

            if (alpha >= beta)
            {
                return alpha;
            }

            Move ttMove = Move.NullMove;
            if (ttCache.Probe(board.Hash, depth, ply, alpha, beta, out int ttScore, out TtCache.TtItem ttItem) && !isPv)
            {
                return ttScore;
            }

            if (ttItem.BestMove != Move.NullMove && board.IsPseudoLegal(ttItem.BestMove))
            {
                ttMove = ttItem.BestMove;
            }

            if (ProbeTb(depth, ply, alpha, beta, out score))
            {
                return score;
            }

            if (!inCheck)
            {
                evaluation = ssItem.Eval = eval.Compute(board, alpha, beta);
                if (ply >= 4 && ss[ply - 4].Eval != NO_SCORE)
                {
                    improving = evaluation > ss[ply - 4].Eval;
                }
                else if (ply >= 2 && ss[ply - 2].Eval != NO_SCORE)
                {
                    improving = evaluation > ss[ply - 2].Eval;
                }
            }

            if (!inCheck && !isPv)
            {
                // static null move pruning (reverse futility pruning)
                if (depth <= UciOptions.RfpMaxDepth && evaluation >= beta + depth * UciOptions.RfpMargin)
                {
                    return evaluation;
                }

                // Null Move Pruning - Prune if current evaluation looks so good that we can see what happens
                // if we just skip our move.
                if (canNull && depth >= UciOptions.NmpMinDepth && evaluation >= beta && board.PieceCount(board.SideToMove) > 1)
                {
                    int R = NMP[depth];
                    if (improving)
                    {
                        R++;
                    }
                    if (board.MakeMove(Move.NullMove))
                    {
                        ssItem.Move = Move.NullMove;
                        ssItem.IsCheckingMove = false;
                        ssItem.Eval = NO_SCORE;
                        ssItem.Continuation = history.NullMoveContinuation;

                        score = -Search(-beta, -beta + 1, Math.Max(depth - R - 1, 0), ply + 1, false);
                        board.UnmakeMove();
                        if (wasAborted)
                        {
                            return 0;
                        }

                        if (score >= beta)
                        {
                            ttCache.Store(board.Hash, depth, ply, alpha, beta, score, Move.NullMove);
                            return score;
                        }
                    }
                    ssItem.Eval = (short)evaluation;
                }

                if (canNull)
                {
                    if (depth <= UciOptions.RzrMaxDepth)
                    {
                        int threshold = alpha - depth * UciOptions.RzrMargin;
                        if (evaluation <= threshold)
                        {
                            score = Quiesce(alpha, beta, ply);
                            if (score <= alpha)
                            {
                                return score;
                            }
                        }
                    }
                    canPrune = true;
                }
            }

            if (!inCheck && depth >= UciOptions.IirMinDepth && ttMove == Move.NullMove)
            {
                depth--;
            }

            Move bestMove = Move.NullMove;
            int expandedNodes = 0, bestScore = -INFINITE_WINDOW;
            history.SetContext(ply);
            StackList<Move> quiets = new(stackalloc Move[64]);
            board.PushBoardState();
            MoveList list = listPool.Rent();

            MoveGenType type = inCheck ? MoveGenType.Evasion : MoveGenType.Normal;
            var moves = new MoveGen(type, board, ply, history, ss, list, ttMove);

            while (moves.MoveNext())
            {
                GenMove genMove = moves.Current;
                if (!board.MakeMoveNs(genMove.Move))
                {
                    continue;
                }

                expandedNodes++;
                NodesVisited++;
                bool checkingMove = board.IsChecked();
                bool isQuiet = genMove.Move.IsQuiet;
                ssItem.Move = genMove.Move;
                ssItem.IsCheckingMove = checkingMove;
                ssItem.Continuation = history.GetContinuation(genMove.Move);
                
                bool interesting = inCheck || checkingMove || (genMove.MovePhase < MoveGenPhase.BadCapture) || expandedNodes == 1;

                int R = 0;
                if (!interesting)
                {
                    if (canPrune)
                    { 
                        if (depth <= UciOptions.FutMaxDepth && !genMove.Move.IsPawnMove && 
                            evaluation + depth * UciOptions.FutMargin < alpha)
                        {
                            board.UnmakeMoveNs();
                            continue;
                        }

                        if (depth <= UciOptions.LmpMaxDepth && expandedNodes > LMP[depth])
                        {
                            board.UnmakeMoveNs();
                            continue;                    
                        }

                        if (depth <= UciOptions.SeeMaxDepth)
                        {
                            int captureValue = genMove.Move.Capture.Value();
                            if (genMove.MovePhase == MoveGenPhase.BadCapture &&
                                (depth <= 1 || board.See1(genMove.Move) - captureValue > (depth - 1) * UciOptions.SeeCaptureMargin))
                            {
                                board.UnmakeMoveNs();
                                continue;
                            }
                            else if (genMove.MovePhase == MoveGenPhase.Quiet &&
                                board.See1(genMove.Move) > depth * UciOptions.SeeQuietMargin)
                            {
                                board.UnmakeMoveNs();
                                continue;
                            }
                        }
                    }
                    R = LMR[Math.Min(depth, MAX_PLY - 1), Math.Min(expandedNodes - 1, LMR_MAX_MOVES - 1)];
                }

                ttCache.Prefetch(board.Hash); // do prefetch before we need the ttItem
                
                if (expandedNodes == 1)
                {
                    score = -Search(-beta, -alpha, depth - 1, ply + 1);
                }
                else
                {
                    score = -Search(-alpha - 1, -alpha, Math.Max(depth - R - 1, 0), ply + 1);

                    if (score > alpha && R > 0)
                    {
                        score = -Search(-alpha - 1, -alpha, depth - 1, ply + 1);
                    }

                    if (score > alpha)
                    {
                        score = -Search(-beta, -alpha, depth - 1, ply + 1);
                    }
                }

                board.UnmakeMoveNs();
                
                if (wasAborted)
                {
                    break;
                }

                if (score > bestScore)
                {
                    bestMove = genMove.Move;
                    bestScore = score;

                    if (score > alpha)
                    {
                        alpha = score;

                        if (score >= beta)
                        {
                            if (isQuiet)
                            {
                                ssItem.Killers.Add(genMove.Move);
                                history.UpdateCutoff(genMove.Move, ply, ref quiets, depth);
                            }
                            break;
                        }

                        pvTable.MergeMove(ply, genMove.Move);
                    }
                }

                if (isQuiet)
                {
                    quiets.Add(genMove.Move);
                }
            }

            listPool.Return(list);
            board.PopBoardState();

            if (wasAborted)
            {
                return 0;
            }

            if (expandedNodes == 0)
            {
                return inCheck ? -CHECKMATE_SCORE + ply : DrawScore;
            }
            ttCache.Store(board.Hash, depth, ply, originalAlpha, beta, bestScore, bestMove);
            return bestScore;
        }

        private int Quiesce(int alpha, int beta, int ply, int qsPly = 0)
        {
            if (wasAborted || MustAbort)
            {
                wasAborted = true;
                return 0;
            }

            SelDepth = Math.Max(SelDepth, ply);
            if (ply >= MAX_PLY - 1)
            {
                return eval.Compute(board, alpha, beta);
            }

            var rep = board.PositionRepeated();
            if (rep.Repeated || rep.OverFiftyMoves)
            {
                return DrawScore;
            }

            int originalAlpha = alpha;
            bool inCheck = ss[ply - 1].IsCheckingMove;
            ref SearchItem ssItem = ref ss[ply];

            Move ttMove = Move.NullMove;
            if (ttCache.Probe(board.Hash, -qsPly, ply, alpha, beta, out int ttScore, out TtCache.TtItem ttItem))
            {
                return ttScore;
            }

            if (ttItem.BestMove != Move.NullMove && board.IsPseudoLegal(ttItem.BestMove))
            {
                ttMove = ttItem.BestMove;
            }

            int standPatScore = eval.Compute(board, alpha, beta);
            if (!inCheck)
            {
                if (standPatScore >= beta)
                {
                    return standPatScore;
                }
                alpha = Math.Max(alpha, standPatScore);
            }

            int score;
            int bestScore = standPatScore;
            Move bestMove = Move.NullMove;
            board.PushBoardState();
            MoveList list = listPool.Rent();
            int expandedNodes = 0;

            MoveGenType type = inCheck ? MoveGenType.Evasion : MoveGenType.QSearch;
            var moves = new MoveGen(type, board, ply, history, ss, list, ttMove, qsPly);

            while (moves.MoveNext())
            {
                GenMove genMove = moves.Current;
                if (!board.MakeMoveNs(genMove.Move))
                {
                    continue;
                }

                expandedNodes++;
                NodesVisited++;
                bool isCheckingMove = board.IsChecked();

                if (!inCheck && !isCheckingMove && genMove.MovePhase == MoveGenPhase.BadCapture)
                {
                    board.UnmakeMoveNs();
                    continue;
                }
                ttCache.Prefetch(board.Hash); // do prefetch before we need the ttItem
                ssItem.Move = genMove.Move;
                ssItem.IsCheckingMove = isCheckingMove;
                ssItem.Continuation = history.GetContinuation(genMove.Move);

                score = -Quiesce(-beta, -alpha, ply + 1, qsPly + 1);
                board.UnmakeMoveNs();

                if (wasAborted)
                {
                    break;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = genMove.Move;

                    if (score > alpha)
                    {
                        alpha = score;

                        if (score >= beta)
                        {
                            break;
                        }
                    }
                }
            }

            listPool.Return(list);
            board.PopBoardState();

            if (wasAborted)
            {
                return 0;
            }

            ttCache.Store(board.Hash, -qsPly, ply, originalAlpha, beta, bestScore, bestMove);
            return bestScore;
        }

        private bool ProbeTb(int depth, int ply, int alpha, int beta, out int score)
        {
            score = 0;
            if (Syzygy.IsInitialized && depth >= UciOptions.SyzygyProbeDepth && 
                /*board.HalfMoveClock == 0 && board.Castling == CastlingRights.None &&*/
                BitOps.PopCount(board.All) <= Syzygy.TbLargest)
            {
                TbResult result = Syzygy.ProbeWdl(board.WhitePieces, board.BlackPieces, 
                    board.Kings, board.Queens, board.Rooks, board.Bishops, board.Knights, board.Pawns,
                    board.HalfMoveClock, (uint)board.Castling, (uint)(board.EnPassantValidated != SquareIndex.None ? board.EnPassantValidated : 0), 
                    board.SideToMove == Color.White);

                if (result == TbResult.TbFailure)
                {
                    return false;
                }

                tbHits++;
                Bound bound = Bound.Exact;
                if (result.Wdl == TbGameResult.Win)
                {
                    score = TABLEBASE_WIN - ply;
                    bound = Bound.Lower;
                }
                else if (result.Wdl == TbGameResult.Loss)
                {
                    score = TABLEBASE_LOSS + ply;
                    bound = Bound.Upper;
                }
                else
                {
                    score = (int)result.Wdl;
                }

                if (bound == Bound.Exact || 
                    (bound == Bound.Upper && score <= alpha) || (bound == Bound.Lower && score >= beta))
                {
                    ttCache.Store(board.Hash, MAX_PLY, ply, alpha, beta, score, Move.NullMove);
                    return true;
                }
            }
            return false;
        }

        private void SetAspWindow(int depth, int iAlpha, int iBeta, out int alpha, out int beta)
        {
            alpha = -INFINITE_WINDOW;
            beta = INFINITE_WINDOW;

            if (depth >= UciOptions.AspMinDepth)
            {
                alpha = AspWindow[iAlpha] == INFINITE_WINDOW ? -INFINITE_WINDOW : Score - AspWindow[iAlpha];
                beta = AspWindow[iBeta] == INFINITE_WINDOW ? INFINITE_WINDOW : Score + AspWindow[iBeta];
            }
        }

        public void ReportOOBSearchResults(int score, Bound bound, ref Move bestMove, ref Move ponderMove)
        {
            if (Depth >= UciOptions.AspMinDepth)
            {
                Uci.Info(Depth, SelDepth, score, NodesVisited, clock.Elapsed, PV, ttCache.Usage, tbHits, bound);
            }
            if (bound == Bound.Lower)
            {
                // when an iteration fails high, go ahead a preserve the best move. if time runs out
                // we can still use this as our best move.
                ReadOnlySpan<Move> moves = pvTable.GetPv();
                if (moves.Length > 0)
                {
                    bestMove = moves[0];
                }
                ponderMove = moves.Length > 1 ? moves[1] : Move.NullMove;
            }
        }

        private void ReportSearchResults(ref Move bestMove, ref Move ponderMove)
        {
            Elapsed = clock.Elapsed;
            bool bestMoveChanged = false;
            Move oldBestMove = bestMove;
            pv.Clear();
            pv.AddRange(pvTable.GetPv());
            //PV = ExtractPv(PV);

            if (pv.Count > 0)
            {
                bestMove = pv[0];
                if (bestMove != oldBestMove)
                {
                    bestMoveChanged = true;
                }

                if (pv.Count > 1)
                {
                    ponderMove = PV[1];
                }
                else
                {
                    ponderMove = Move.NullMove;
                }
            }
            else if (bestMove != Move.NullMove)
            {
                if (board.IsLegalMove(bestMove))
                {
                    board.MakeMove(bestMove);
                    pv.Add(bestMove);

                    if (ponderMove != Move.NullMove && board.IsLegalMove(ponderMove))
                    {
                        pv.Add(ponderMove);
                    }

                    board.UnmakeMove();
                }
            }

            if (bestMoveChanged)
            {
                ++rootChanges;
            }

            if (Depth > 4)
            {
                clock.AdjustTime(oneLegalMove && !UciOptions.AnalyseMode, bestMoveChanged, rootChanges);
            }

            if (IsCheckmate(Score, out int mateIn))
            {
                Uci.InfoMate(Depth, SelDepth, mateIn, NodesVisited, Elapsed, pv, ttCache.Usage, tbHits);
            }
            else
            {
                Uci.Info(Depth, SelDepth, Score, NodesVisited, Elapsed, pv, ttCache.Usage, tbHits);
            }
        }

        #endregion

        #region Static Methods

        public static void Initialize() 
        {
            Array.Clear(NMP);
            Array.Clear(LMP);
            for (int ply = 0; ply < MAX_PLY; ply++)
            {
                NMP[ply] = NmpReduction(ply);
            }

            LMR.Clear();
            double depthFactor = UciOptions.LmrDepthFactor / 10.0;
            double moveFactor = UciOptions.LmrMoveFactor / 10.0;
            double scaleFactor = UciOptions.LmrScaleFactor / 10.0;

            double reduction;
            for (int depth = 1; depth < MAX_PLY; depth++)
            {
                if (depth >= 3)
                {
                    for (int move = 0; move < LMR_MAX_MOVES; move++)
                    {
                        reduction = Math.Log(depth * depthFactor) * Math.Log(move * moveFactor) / scaleFactor;
                        reduction = (sbyte)Math.Clamp(reduction, LMR_MIN, LMR_MAX);
                        LMR[depth, move] = (sbyte)Math.Min(reduction, Math.Max(depth - 2, 0));
                    }
                }
                if (depth < LMP_MAX_DEPTH_CUTOFF)
                {
                    LMP[depth] = (sbyte)Math.Clamp(depth * depth + UciOptions.LmpDepthIncrement, 1, 127);
                }
            }
        }

        private static bool IsCheckmate(int score, out int mateIn)
        {
            mateIn = 0;
            int absScore = Math.Abs(score);
            bool checkMate = absScore is >= CHECKMATE_SCORE - MAX_PLY * 2 and <= CHECKMATE_SCORE;
            if (checkMate)
            {
                mateIn = ((CHECKMATE_SCORE - absScore + 1) / 2) * Math.Sign(score);
            }

            return checkMate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte NmpReduction(int depth)
        {
            return (sbyte)(depth < 3 ? 0 : UciOptions.NmpBaseReduction + Math.Max(depth - 3, 0) / UciOptions.NmpIncDivisor);
        }

        #endregion

        #region Fields

        private readonly SearchStack ss;
        private readonly Board board;
        private readonly GameClock clock;
        private readonly HceEval eval;
        private readonly History history;
        private readonly ObjectPool<MoveList> listPool;
        private readonly TtCache ttCache;
        private readonly int maxDepth;
        private readonly long maxNodes;
        private readonly PvTable pvTable = new();
        private readonly List<Move> pv = new(MAX_PLY);
        private readonly CpuStats cpuStats = new();
        private Uci uci = Uci.Default;
        private bool oneLegalMove = false;
        private bool wasAborted = false;
        private bool startReporting = false;
        private DateTime startDateTime = DateTime.MinValue;
        private int rootChanges = 0;
        private long tbHits = 0;
        internal static readonly int[] AspWindow = [33, 100, 300, 900, 2700, INFINITE_WINDOW];
        internal static readonly sbyte[] NMP = new sbyte[MAX_PLY];
        internal static readonly sbyte[] LMP = new sbyte[LMP_MAX_DEPTH_CUTOFF];
        internal static readonly FixedArray2D<sbyte> LMR = new(MAX_PLY, LMR_MAX_MOVES, true);

        #endregion
    }
}
