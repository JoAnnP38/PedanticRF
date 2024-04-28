// <copyright file="MoveGen.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Collections;
    using System.Runtime.CompilerServices;

    public struct MoveGen : IEnumerable<GenMove>, IEnumerator<GenMove>
    {
        public const int MAX_BAD_CAPTURES = Board.MAX_BAD_CAPTURES;

        internal enum State
        {
            HashMove,
            GenerateCaptures,
            GoodCapture,
            GeneratePromotions,
            Promotion,
            GenerateQuiets,
            Killer1,
            Killer2,
            Counter,
            BadCapture,
            Quiet,
            End
        }

        [InlineArray(MAX_BAD_CAPTURES)]
        internal struct BadCaptureArray
        {
            public const int CAPACITY = MAX_BAD_CAPTURES;
            private Move _element0;
        }

        private MoveGenType type;
        private Board board;
        private int ply;
        private int qsPly;
        private History history;
        private SearchStack ss;
        private MoveList list;
        private Move ttMove;
        private GenMove current;
        private State state;
        private SquareIndex kingIndex;
        private Board.EvasionInfo info;
        private Board.GenMoveHelper? helper;
        private int moveIndex;
        private int bcIndex;
        private int bcCount;
        private BadCaptureArray badCaptures;

        private static readonly GenMove startEndMove = new GenMove(Move.NullMove, MoveGenPhase.End);

        public MoveGen(MoveGenType type, Board board, int ply, History history, SearchStack ss, MoveList list, 
            Move ttMove, int qsPly = 0)
        {
            this.type = type;
            this.board = board;
            this.ply = ply;
            this.qsPly = qsPly;
            this.history = history;
            this.ss = ss;
            this.list = list;
            this.ttMove = ttMove;
            state = State.HashMove;
            current = startEndMove;
            helper = null;
        }

        public GenMove Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => current;
        }

        object IEnumerator.Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        public void Dispose()
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<GenMove> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            if (state != State.End)
            {
                return type switch 
                {
                    MoveGenType.Normal => NormalMoveNextImpl(),
                    MoveGenType.QSearch => QSearchMoveNextImpl(),
                    MoveGenType.Evasion => EvasionMoveNextImpl(),
                    _ => false
                };
            }
            current = startEndMove;
            return false;
        }

        public void Reset()
        {
            state = State.HashMove;
            current = startEndMove;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool NormalMoveNextImpl()
        {
            bool moveAvailable = false;
            do
            {
                switch (state)
                {
                    case State.HashMove:
                        moveAvailable = HashMove();
                        break;

                    case State.GenerateCaptures:
                        moveIndex = 0;
                        bcIndex = 0;
                        bcCount = 0;
                        list.Clear();
                        kingIndex = board.KingIndex[board.SideToMove];
                        info = new();
                        history.SetContext(ply);
                        board.GenerateCaptures(kingIndex, list, in info);
                        list.Remove(ttMove);
                        state = list.Count > 0 ? State.GoodCapture : State.GeneratePromotions;
                        break;

                    case State.GoodCapture:
                        moveAvailable = GoodCapture();

                        if (!moveAvailable)
                        {
                            state = State.GeneratePromotions;
                        }
                        break;

                    case State.GeneratePromotions:
                        moveIndex = 0;
                        list.Clear();
                        history.SetContext(ply);
                        board.GeneratePromotions(list, in info);
                        list.Remove(ttMove);
                        state = list.Count > 0 ? State.Promotion : State.GenerateQuiets;
                        break;

                    case State.Promotion:
                        moveAvailable = Promotion();

                        if (!moveAvailable)
                        {
                            state = State.GenerateQuiets;
                        }
                        break;

                    case State.GenerateQuiets:
                        moveIndex = 0;
                        bcIndex = 0;
                        list.Clear();
                        history.SetContext(ply);
                        board.GenerateQuiets(kingIndex, list, in info);
                        list.Remove(ttMove);
                        state = list.Count > 0 ? State.Killer1 : (bcCount > 0 ? State.BadCapture : State.End);
                        break;

                    case State.Killer1:
                        moveAvailable = Killer1();
                        break;

                    case State.Killer2:
                        moveAvailable = Killer2();
                        break;

                    case State.Counter:
                        moveAvailable = Counter();
                        break;

                    case State.BadCapture:
                        moveAvailable = BadCapture();

                        if (!moveAvailable)
                        {
                            state = list.Count > 0 ? State.Quiet : State.End;
                        }
                        break;

                    case State.Quiet:
                        moveAvailable = Quiet();
                        break;

                    default:
                        state = State.End;
                        break;
                }
            } while (state != State.End && !moveAvailable);

            return moveAvailable;
        }

        private bool QSearchMoveNextImpl()
        {
            bool moveAvailable = false;
            do
            {
                switch (state)
                {
                    case State.HashMove:
                        moveAvailable = HashMove();
                        break;

                    case State.GenerateCaptures:
                        moveIndex = 0;
                        bcIndex = 0;
                        bcCount = 0;
                        list.Clear();
                        kingIndex = board.KingIndex[board.SideToMove];
                        info = new();
                        history.SetContext(ply);

                        if (qsPly >= UciOptions.RecaptureDepth)
                        {
                            info = new Board.EvasionInfo(0, Bitboard.None, new Bitboard(ss[ply - 1].Move.To), Bitboard.None);
                        }

                        board.GenerateCaptures(kingIndex, list, in info);
                        list.Remove(ttMove);

                        if (list.Count > 0)
                        {
                            state = State.GoodCapture;
                        }
                        else if (qsPly < UciOptions.PromotionDepth)
                        {
                            state = State.GeneratePromotions;
                        }
                        else
                        {
                            state = State.End;
                        }
                        break;

                    case State.GoodCapture:
                        moveAvailable = GoodCapture();

                        if (!moveAvailable)
                        {
                            state = qsPly < UciOptions.PromotionDepth ? 
                                State.GeneratePromotions : (bcCount > 0 ? State.BadCapture : State.End);
                        }
                        break;

                    case State.GeneratePromotions:
                        moveIndex = 0;
                        list.Clear();
                        history.SetContext(ply);
                        board.GeneratePromotions(list, in info);
                        list.Remove(ttMove);
                        state = list.Count > 0 ? State.Promotion : (bcCount > 0 ? State.BadCapture : State.End);
                        break;

                    case State.Promotion:
                        moveAvailable = Promotion();

                        if (!moveAvailable)
                        {
                            if (bcCount > 0)
                            {
                                state = State.BadCapture;
                            }
                            else
                            {
                                state = State.End;
                            }
                        }
                        break;

                    case State.BadCapture:
                        moveAvailable = BadCapture();

                        if (!moveAvailable)
                        {
                            state = State.End;
                        }
                        break;

                    default:
                        state = State.End;
                        break;
                }
            } while (state != State.End && !moveAvailable);

            return moveAvailable;
        }

        private bool EvasionMoveNextImpl()
        {
            bool moveAvailable = false;
            do
            {
                switch (state)
                {
                    case State.HashMove:
                        moveAvailable = HashMove();
                        break;

                    case State.GenerateCaptures:
                        moveIndex = 0;
                        bcIndex = 0;
                        bcCount = 0;
                        list.Clear();
                        kingIndex = board.KingIndex[board.SideToMove];
                        board.GetEvasionInfo(kingIndex, out info, out helper);
                        history.SetContext(ply);
                        board.GenerateKingCaptures(kingIndex, list, in info);
                        if (info.CheckerCount <= 1)
                        {
                            board.GeneratePawnCaptures(list, in info, helper);
                            board.GeneratePieceCaptures(list, in info);
                        }
                        list.Remove(ttMove);
                        state = list.Count > 0 ? State.GoodCapture : State.GeneratePromotions;
                        break;

                    case State.GoodCapture:
                        moveAvailable = GoodCapture();

                        if (!moveAvailable)
                        {
                            state = State.GeneratePromotions;
                        }
                        break;

                    case State.GeneratePromotions:
                        moveIndex = 0;
                        list.Clear();
                        history.SetContext(ply);
                        if (info.CheckerCount <= 1)
                        {
                            board.GeneratePromotions(list, in info);
                            list.Remove(ttMove);
                        }
                        state = list.Count > 0 ? State.Promotion : State.GenerateQuiets;
                        break;

                    case State.Promotion:
                        moveAvailable = Promotion();

                        if (!moveAvailable)
                        {
                            state = State.GenerateQuiets;
                        }
                        break;

                    case State.GenerateQuiets:
                        moveIndex = 0;
                        bcIndex = 0;
                        list.Clear();
                        history.SetContext(ply);
                        board.GenerateKingQuiets(kingIndex, list, in info);
                        if (info.CheckerCount <= 1)
                        {
                            board.GeneratePawnQuiets(list, in info, helper);
                            board.GeneratePieceQuiets(list, in info);
                        }
                        list.Remove(ttMove);
                        state = list.Count > 0 ? State.Killer1 : (bcCount > 0 ? State.BadCapture : State.End);
                        break;

                    case State.Killer1:
                        moveAvailable = Killer1();
                        break;

                    case State.Killer2:
                        moveAvailable = Killer2();
                        break;

                    case State.Counter:
                        moveAvailable = Counter();
                        break;

                    case State.BadCapture:
                        moveAvailable = BadCapture();

                        if (!moveAvailable)
                        {
                            state = list.Count > 0 ? State.Quiet : State.End;
                        }
                        break;

                    case State.Quiet:
                        moveAvailable = Quiet();
                        break;

                    default:
                        state = State.End;
                        break;
                }
            } while (state != State.End && !moveAvailable);

            return moveAvailable;
        }

        private bool HashMove()
        {
            state = State.GenerateCaptures;
            if (ttMove != Move.NullMove)
            {
                current = new GenMove(ttMove, MoveGenPhase.HashMove);
                return true;
            }
            return false;
        }

        private bool GoodCapture()
        {
            bool moveAvailable = false;
            while (!moveAvailable && moveIndex < list.Count)
            {
                Move move = list.Sort(moveIndex++);
                if (bcCount < MAX_BAD_CAPTURES && move.Piece.Value() > move.Capture.Value() && board.See0(move) < 0)
                {
                    badCaptures[bcCount++] = move;
                    continue;
                }
                current = new GenMove(move, MoveGenPhase.GoodCapture);
                moveAvailable = true;
            }

            return moveAvailable;
        }

        private bool Promotion()
        {
            bool moveAvailable = false;
            if (moveIndex < list.Count)
            {
                Move move = list.Sort(moveIndex++);
                current = new GenMove(move, MoveGenPhase.Promotion);
                moveAvailable = true;
            }
            return moveAvailable;
        }

        private bool Killer1()
        {
            bool moveAvailable = false;
            Move killer = ss[ply].Killers.Move1;
            if (list.Remove(killer))
            {
                current = new GenMove(killer, MoveGenPhase.Killers);
                moveAvailable = true;
            }
            state = State.Killer2;
            return moveAvailable;
        }

        private bool Killer2()
        {
            bool moveAvailable = false;
            Move killer = ss[ply].Killers.Move2;
            if (list.Remove(killer))
            {
                current = new GenMove(killer, MoveGenPhase.Killers);
                moveAvailable = true;
            }
            state = State.Counter;
            return moveAvailable;
        }

        private bool Counter()
        {
            bool moveAvailable = false;
            Move counter = history.CounterMove(ss[ply - 1].Move);
            if (list.Remove(counter))
            {
                current = new GenMove(counter, MoveGenPhase.Counter);
                moveAvailable = true;
            }
            state = bcCount > 0 ? State.BadCapture : (list.Count > 0 ? State.Quiet : State.End);
            return moveAvailable;
        }

        private bool BadCapture()
        {
            bool moveAvailable = false;
            if (bcIndex < bcCount)
            {
                current = new GenMove(badCaptures[bcIndex++], MoveGenPhase.BadCapture);
                moveAvailable = true;
            }
            return moveAvailable;
        }

        private bool Quiet()
        {
            bool moveAvailable = false;
            if (moveIndex < list.Count)
            {
                current = new GenMove(list.Sort(moveIndex++), MoveGenPhase.Quiet);
                moveAvailable = true;
            }

            if (!moveAvailable)
            {
                state = State.End;
            }

            return moveAvailable;
        }
    }
}
