﻿// <copyright file="BasicSearch.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;
    using Pedantic.Chess.NNUE;
    using Pedantic.Collections;
    using Pedantic.Tablebase;
    using Pedantic.Utilities;

    public sealed class BasicSearch : IInitialize
    {
        internal const int CHECK_TC_NODES_MASK = 1023;
        internal const int WAIT_TIME = 50;
        internal const int LMR_MAX_MOVES = 64;
        internal const double LMR_MIN = 0.0;
        internal const double LMR_MAX = 15.0;
        internal const int LMP_MAX_DEPTH_CUTOFF = 11;

        #region Constructors

        public BasicSearch(SearchStack searchStack, NnueEval eval, History history, ObjectPool<MoveList> listPool)
        {
            ss = searchStack;
            this.eval = eval;
            this.history = history;
            this.listPool = listPool;
            ttCache = TtCache.Default;
        }

        public BasicSearch(SearchStack searchStack, NnueEval eval, History history, ObjectPool<MoveList> listPool, TtCache tt)
        {
            ss = searchStack;
            this.eval = eval;
            this.history = history;
            this.listPool = listPool;
            ttCache = tt;
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
                   ((NodesVisited & CHECK_TC_NODES_MASK) == 0 && clock!.CheckTimeBudget());
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

        #region Initializer

        public void Initialize(Board board, GameClock clock, Uci uci, bool canPonder, int maxDepth, long maxNodes, long minNodes)
        {
            this.board = board;
            this.clock = clock;
            this.uci = uci;
            CanPonder = canPonder;
            Depth = 0;
            Elapsed = 0;
            NodesVisited = 0;
            Score = 0;
            SelDepth = 0;
            this.maxDepth = maxDepth;
            this.maxNodes = maxNodes;
            this.minNodes = minNodes;
            pv.Clear();
            cpuStats.Reset();
            oneLegalMove = false;
            wasAborted = false;
            startReporting = false;
            startDateTime = DateTime.MinValue;
            rootChanges = 0;
            tbHits = 0;
            board.AttachEfficientlyUpdatable(eval);
        }

        #endregion

        #region Search Methods

        public void Search()
        {
            Util.Assert(eval.StateCount == 0);
            string position = board!.ToFenString();
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

                while (++Depth <= maxDepth && NodesVisited < minNodes && clock!.CanSearchDeeper())
                {
                    Util.Assert(eval.StateCount == 0);
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
                    while (clock!.Infinite && !wasAborted)
                    {
                        waiting = true;
                        Thread.Sleep(WAIT_TIME);
                    }

                    if (waiting)
                    {
                        ReportSearchResults(ref bestMove, ref ponderMove);
                    }
                }

                if (bestMove.IsNull)
                {
                    // somehow we arrived here without a best move to report. log some information
                    // to help us diagnose the problem and set the bestMove to the firstMove in the move
                    // list.
                    TextWriter err = Console.Error;
                    err.WriteLine($"[{DateTime.Now}]\nIllegal null move result on position: {board.ToFenString()}");
                    err.WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}");
                    err.WriteLine($"Search.Depth = {Depth}");
                    err.WriteLine(clock!.ToString());

                    if (list.Count > 0)
                    {
                        // since search was aborted before search count find a best move
                        // just return the first legal move in the move list
                        for (int n = 0; n < list.Count; n++)
                        {
                            Move move = list.Sort(n);
                            if (!board.MakeMove(move))
                            {
                                continue;
                            }
                            board.UnmakeMove();
                            bestMove = move;
                            break;
                        }
                        ponderMove = Move.NullMove;
                    }
                    else
                    {
                        throw new Exception("Engine could not generate a legal move.");
                    }
                }
                Uci.Usage(cpuStats.CpuLoad);
                Uci.BestMove(bestMove, CanPonder ? ponderMove : Move.NullMove);
            }
            catch (Exception ex)
            {
                string msg =
                    $"[{DateTime.Now}]\nSearch: Unexpected exception occurred on position '{position}' : {ex.Message}";
                Console.Error.WriteLine(msg);
                Console.Error.WriteLine(ex.ToString());
                Uci.Log(msg);
                Util.TraceError(ex.ToString());
                throw;            
            }
            finally
            {
                listPool.Return(list);
                board.DetachEfficientlyUpdatable();
            }
        }

        private int SearchRoot(MoveList list, int alpha, int beta, int depth)
        {
            Util.Assert(eval.StateCount == 0);

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
            int expandedNodes = 0, bestScore = -INFINITE_WINDOW;
            board!.PushBoardState();

            for (int n = 0; n < list.Count; n++)
            {
                Move move = list[n];
                if (!board.MakeMoveNs(move))
                {
                    list.SetScore(n, int.MinValue);
                    continue;
                }

                ttCache.Prefetch(board.Hash); // do prefetch before we need the ttItem
                eval.Prefetch(board);
                
                expandedNodes++;
                NodesVisited++;

                if (startReporting || (DateTime.Now - startDateTime).TotalMilliseconds >= 1000)
                {
                    startReporting = true;
                    Uci.CurrentMove(depth, move, expandedNodes, NodesVisited);
                }

                bool checkingMove = board.IsChecked();
                bool isQuiet = move.IsQuiet;
                bool interesting = inCheck || !isQuiet;

                ssItem.IsCheckingMove = checkingMove;
                ssItem.Move = move;
                ssItem.Continuation = history.GetContinuation(move);
                
                int R = 0;
                if (!interesting)
                {
                    R = LMR[Math.Min(depth, MAX_PLY - 1), Math.Min(expandedNodes - 1, LMR_MAX_MOVES - 1)];

                    if (checkingMove && R > 0)
                    {
                        R--;
                    }
                }

                int newDepth = depth - 1;

                if (expandedNodes == 1)
                {
                    score = -Search(-beta, -alpha, newDepth, 1, false);
                }
                else
                {
                    score = -Search(-alpha - 1, -alpha, Math.Max(newDepth - R, 0), 1, true);

                    if (score > alpha && R > 0)
                    {
                        score = -Search(-alpha - 1, -alpha, newDepth, 1, true);
                    }

                    if (score > alpha)
                    {
                        score = -Search(-beta, -alpha, newDepth, 1, false);
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
            Util.Assert(eval.StateCount == 0);

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

        private int Search(int alpha, int beta, int depth, int ply, bool cutNode, bool canNull = true)
        {
            Util.Assert(eval.StateCount == ply);
            pvTable.InitPly(ply);
            SelDepth = Math.Max(SelDepth, ply);
            bool inCheck = ss[ply - 1].IsCheckingMove;

            if (wasAborted || MustAbort)
            {
                wasAborted = true;
                return 0;
            }

            if (ply >= MAX_PLY - 1)
            {
                return ComputeStaticEval();
            }

            if (depth <= 0)
            {
                return Quiesce(alpha, beta, ply);
            }

            var rep = board!.PositionRepeated();
            if (rep.Repeated || rep.OverFiftyMoves)
            {
                return DrawScore;
            }

            int score;
            int originalAlpha = alpha;
            bool isPv = beta - alpha > 1;
            bool allNode = !isPv && !cutNode;
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

            if (ttCache.Probe(board.Hash, depth, ply, alpha, beta, out bool ttHit, out int ttScore, out TtCache.TtItem ttItem) && !isPv)
            {
                return ttScore;
            }

            Move ttMove;
            if (ssItem.Exclude.IsNull && ttItem.BestMove != Move.NullMove && board.IsPseudoLegal(ttItem.BestMove))
            {
                ttMove = ttItem.BestMove;
            }
            else
            {
                ttMove = Move.NullMove;
            }

            if (ProbeTb(depth, ply, alpha, beta, out score))
            {
                return score;
            }

            if (!inCheck)
            {
                if (!ssItem.Exclude.IsNull)
                {
                    evaluation = ssItem.Eval;
                }
                else
                {
                    evaluation = ssItem.Eval = ComputeStaticEval();
                }

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
                if (depth <= UciOptions.RfpMaxDepth && 
                    evaluation >= beta + (depth / (improving ? 2 : 1)) * UciOptions.RfpMargin)
                {
                    return evaluation;
                }

                // Null Move Pruning - Prune if current evaluation looks so good that we can see what happens
                // if we just skip our move.
                if (canNull && depth >= UciOptions.NmpMinDepth && evaluation >= beta && board.PieceCount(board.SideToMove) > 1)
                {
                    int R = NMP[depth] + (improving ? 1 : 0);

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
                            // don't trust mate scores
                            score = Math.Abs(score) > TABLEBASE_WIN ? beta : score;
                            return score;
                        }
                    }
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

            // IIR 
            if (ttMove.IsNull && depth >= UciOptions.IirMinDepth)
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
                if (genMove.Move == ssItem.Exclude || !board.MakeMoveNs(genMove.Move))
                {
                    continue;
                }

                expandedNodes++;
                bool checkingMove = board.IsChecked();
                bool isQuiet = genMove.Move.IsQuiet;
                int hist = history.GetHistory(ply, genMove.Move);
                bool interesting = inCheck || (genMove.MovePhase < MoveGenPhase.Killers) || expandedNodes == 1;

                if (!interesting && !UciOptions.IsDataGen)
                {
                    if (canPrune && genMove.MovePhase >= MoveGenPhase.BadCapture && bestScore > MAX_TABLEBASE_LOSS)
                    { 
                        // futility pruning
                        if (depth <= UciOptions.FutMaxDepth && !genMove.Move.IsPawnMove && 
                            evaluation + depth * UciOptions.FutMargin < alpha)
                        {
                            board.UnmakeMoveNs();
                            continue;
                        }

                        // LMP pruning
                        if (depth <= UciOptions.LmpMaxDepth && expandedNodes > LMP[depth])
                        {
                            board.UnmakeMoveNs();
                            continue;                    
                        }

                        // SEE-based pruning
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
                }

                int X = 0, R = 0;

                // singular extension
                if (ply < Depth * 2)
                {
                    if (inCheck)
                    {
                        X = 1;
                    }
                    else if (ssItem.Exclude.IsNull && 
                        !allNode && 
                        genMove.Move == ttMove && 
                        Math.Abs(ttScore) < MIN_TABLEBASE_WIN &&
                        depth >= UciOptions.SexMinDepth &&
                        ttItem.Depth >= depth - 3 &&
                        ttItem.Bound != Bound.Upper)
                    {
                        board.UnmakeMoveNs();
                        board.PopBoardState();

                        int sexBeta = ttScore - depth * UciOptions.SexDepthMult / 16;
                        ssItem.Exclude = genMove.Move;
                        score = Search(sexBeta - 1, sexBeta, (depth + UciOptions.SexDepthOffset) / 2, ply, cutNode, false);
                        ssItem.Exclude = Move.NullMove;

                        if (score < sexBeta)
                        {
                            X = 1;
                        }

                        board.PushBoardState();
                        board.MakeMoveNs(genMove.Move);

                    }
                }

                ttCache.Prefetch(board.Hash); // do prefetch before we need the ttItem
                eval.Prefetch(board);

                ssItem.Move = genMove.Move;
                ssItem.IsCheckingMove = checkingMove;
                ssItem.Continuation = history.GetContinuation(genMove.Move);

                NodesVisited++;

                if (!interesting && depth >= 3)
                {
                    R = LMR[Math.Min(depth, MAX_PLY - 1), Math.Min(expandedNodes - 1, LMR_MAX_MOVES - 1)];
                    R += !improving ? 1 : 0;
                    R += board.PieceCount(board.SideToMove.Flip()) <= 2 ? 1 : 0;
                    R -= checkingMove ? 1 : 0;
                    R -= isPv ? 1 : 0;
                    R -= hist / UciOptions.LmrHistoryDiv;
                    R -= genMove.MovePhase == MoveGenPhase.Killers || genMove.MovePhase == MoveGenPhase.Counter ? 1 : 0;
                    R = Math.Clamp(R, 0, depth - 1);
                }
                
                if (expandedNodes == 1)
                {
                    score = -Search(-beta, -alpha, depth + X - 1, ply + 1, false);
                }
                else
                {
                    score = -Search(-alpha - 1, -alpha, Math.Max(depth + X - R - 1, 0), ply + 1, true);

                    if (score > alpha && R > 0)
                    {
                        score = -Search(-alpha - 1, -alpha, depth + X - 1, ply + 1, !cutNode);
                    }

                    if (score > alpha)
                    {
                        score = -Search(-beta, -alpha, depth + X - 1, ply + 1, false);
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
            Util.Assert(eval.StateCount == ply);

            if (wasAborted)
            {
                return 0;
            }

            if (expandedNodes == 0)
            {
                return inCheck ? -CHECKMATE_SCORE + ply : DrawScore;
            }

            if (ssItem.Exclude.IsNull)
            {
                ttCache.Store(board.Hash, depth, ply, originalAlpha, beta, bestScore, bestMove);
            }
            return bestScore;
        }

        private int Quiesce(int alpha, int beta, int ply, int qsPly = 0)
        {
            Util.Assert(eval.StateCount == ply);

            if (wasAborted || MustAbort)
            {
                wasAborted = true;
                return 0;
            }

            SelDepth = Math.Max(SelDepth, ply);
            if (ply >= MAX_PLY - 1)
            {
                return ComputeStaticEval();
            }

            var rep = board!.PositionRepeated();
            if (rep.Repeated || rep.OverFiftyMoves)
            {
                return DrawScore;
            }

            int originalAlpha = alpha;
            bool inCheck = ss[ply - 1].IsCheckingMove;
            ref SearchItem ssItem = ref ss[ply];

            Move ttMove = Move.NullMove;
            if (ttCache.Probe(board!.Hash, -qsPly, ply, alpha, beta, out _, out int ttScore, out TtCache.TtItem ttItem))
            {
                return ttScore;
            }

            if (ttItem.BestMove != Move.NullMove && board.IsPseudoLegal(ttItem.BestMove))
            {
                ttMove = ttItem.BestMove;
            }

            int standPatScore = ComputeStaticEval();
            if (!inCheck)
            {
                if (standPatScore >= beta)
                {
                    return standPatScore;
                }
                alpha = Math.Max(alpha, standPatScore);
            }

            int score;
            int bestScore = inCheck ? -INFINITE_WINDOW : alpha;
            Move bestMove = Move.NullMove;
            board.PushBoardState();
            MoveList list = listPool.Rent();
            int expandedNodes = 0;

            var moves = new MoveGen(inCheck ? MoveGenType.Evasion : MoveGenType.QSearch, board, ply, history, ss, list, ttMove, qsPly);

            while (moves.MoveNext())
            {
                GenMove genMove = moves.Current;
                if (!board.MakeMoveNs(genMove.Move))
                {
                    continue;
                }

                expandedNodes++;

                if (!inCheck && bestScore > MAX_TABLEBASE_LOSS && genMove.MovePhase >= MoveGenPhase.BadCapture)
                {
                    board.UnmakeMoveNs();
                    continue;
                }

                ttCache.Prefetch(board.Hash); // do prefetch before we need the ttItem
                eval.Prefetch(board);
                NodesVisited++;
                ssItem.Move = genMove.Move;
                ssItem.IsCheckingMove = board.IsChecked();
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
            Util.Assert(eval.StateCount == ply);

            if (wasAborted)
            {
                return 0;
            }

            if (expandedNodes == 0 && inCheck)
            {
                return -CHECKMATE_SCORE + ply;
            }

            ttCache.Store(board.Hash, -qsPly, ply, originalAlpha, beta, bestScore, bestMove);
            return bestScore;
        }

        private bool ProbeTb(int depth, int ply, int alpha, int beta, out int score)
        {
            score = 0;
            if (Syzygy.IsInitialized && depth >= UciOptions.SyzygyProbeDepth && 
                BitOps.PopCount(board!.All) <= Syzygy.TbLargest)
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
                Uci.Info(Depth, SelDepth, ScaleCpScore(score), NodesVisited, clock!.Elapsed, PV, 0, tbHits, bound);
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
            Elapsed = clock!.Elapsed;
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
                if (board!.IsLegalMove(bestMove))
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
                Uci.Info(Depth, SelDepth, ScaleCpScore(Score), NodesVisited, Elapsed, pv, ttCache.Usage, tbHits);
            }
        }

        private short ComputeStaticEval()
        {
            if (board != null)
            {
                short score = eval.Compute(board);

#if DEBUG
                short scoreDebug = evalDebug.ComputeUncached(board);
                Util.Assert(score == scoreDebug);
#endif
                return score;
            }
            return 0;
        }

        public int ScaleCpScore(int score)
        {
            if (!UciOptions.IsDataGen && Math.Abs(score) < MIN_TABLEBASE_WIN)
            {
                return score / 2;
            }
            return score;
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
            return (sbyte)(depth < UciOptions.NmpMinDepth ? 0 : 
                UciOptions.NmpBaseReduction + Math.Max(depth - 3, 0) / UciOptions.NmpIncDivisor);
        }

        #endregion

        #region Fields

        private readonly SearchStack ss;
        private readonly NnueEval eval;
        private readonly History history;
        private readonly ObjectPool<MoveList> listPool;
        private readonly TtCache ttCache;
        private readonly PvTable pvTable = new();
        private readonly List<Move> pv = new(MAX_PLY);
        private readonly CpuStats cpuStats = new();

        private Board? board = null;
        private GameClock? clock = null;
        private Uci uci = Uci.Default;
        private int maxDepth;
        private long maxNodes;
        private long minNodes;
        private bool oneLegalMove = false;
        private bool wasAborted = false;
        private bool startReporting = false;
        private DateTime startDateTime = DateTime.MinValue;
        private int rootChanges = 0;
        private long tbHits = 0;

#if DEBUG
        private readonly NnueEval evalDebug = new();
#endif

        internal static readonly int[] AspWindow = [33, 100, 300, 900, 2700, INFINITE_WINDOW];
        internal static readonly sbyte[] NMP = new sbyte[MAX_PLY];
        internal static readonly sbyte[] LMP = new sbyte[LMP_MAX_DEPTH_CUTOFF];
        internal static readonly FixedArray2D<sbyte> LMR = new(MAX_PLY, LMR_MAX_MOVES, true);

#endregion
    }
}
