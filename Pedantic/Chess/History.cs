using System.Runtime.CompilerServices;

using Pedantic.Collections;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public unsafe sealed class History : IHistory
    {
        internal const int HISTORY_LEN = MAX_COLORS * MAX_PIECES * MAX_SQUARES;

        private short[] history;
        private Move[] counterMoves;
        private short[][] contHist;
        private SearchStack ss;
        private int ply;
        private readonly short[] nullMoveCont;

        public History(SearchStack searchStack)
        {
            history = new short[HISTORY_LEN];
            counterMoves = new Move[HISTORY_LEN];
            contHist = Mem.Allocate2D<short>(HISTORY_LEN, HISTORY_LEN);
            ss = searchStack;
            ply = 0;
            Clear();
            nullMoveCont = GetContinuation(Color.White, Piece.Pawn, SquareIndex.A1);
        }

        public short this[Color stm, Piece piece, SquareIndex sq]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int index = GetIndex(stm, piece, sq);
                int value = history[index] 
                          + CH(in ss[ply - 1], index)
                          + CH(in ss[ply - 2], index)
                          + CH(in ss[ply - 4], index);
                return (short)Math.Clamp(value, short.MinValue, short.MaxValue);
            }
        }

        public short this[Move move]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int index = GetIndex(move);
                int value = history[GetIndex(move)] 
                          + CH(in ss[ply - 1], index)
                          + CH(in ss[ply - 2], index)
                          + CH(in ss[ply - 4], index);
                return (short)Math.Clamp(value, short.MinValue, short.MaxValue);
            }
        }

        public short GetHistory(int ply, Move move)
        {
            SetContext(ply);
            return this[move];
        }

	    public short[]? NullMoveContinuation
	    {
		    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		    get
		    {
			    return nullMoveCont;
		    }
	    }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move CounterMove(Move lastMove)
        {
            Util.Assert(lastMove.IsValid);
            if (lastMove == Move.NullMove)
            {
                return Move.NullMove;
            }
            return counterMoves[GetIndex(lastMove)];
        }

        public void Clear()
        {
            Array.Clear(history);
            Array.Clear(counterMoves);
            Mem.Clear(contHist);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetContext(int ply)
        {
            this.ply = ply;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short[] GetContinuation(Color stm, Piece piece, SquareIndex to)
        {
            return contHist[GetIndex(stm, piece, to)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short[]? GetContinuation(Move move)
        {
            if (move == Move.NullMove)
            {
                return nullMoveCont;
            }
            return contHist[GetIndex(move)];
        }

        public void UpdateCutoff(Move move, int ply, ref StackList<Move> quiets, int depth)
        {
            SetContext(ply);
            short bonus = (short)Math.Min(UciOptions.HisMaxBonus, (short)(UciOptions.HisBonusCoefficient * (depth - 1)));
            int index = GetIndex(move);
            UpdateHistory(ref history[index], bonus);
            UpdateHistory(ref ss[ply - 1].Continuation![index], bonus);
            UpdateHistory(ref ss[ply - 2].Continuation![index], bonus);
            UpdateHistory(ref ss[ply - 4].Continuation![index], bonus);

            short malus = (short)-bonus;
            for (int n = 0; n < quiets.Count; n++)
            {
                index = GetIndex(quiets[n]);
                UpdateHistory(ref history[index], malus);
                UpdateHistory(ref ss[ply - 1].Continuation![index], malus);
                UpdateHistory(ref ss[ply - 2].Continuation![index], malus);
                UpdateHistory(ref ss[ply - 4].Continuation![index], malus);
            }

            Move lastMove = ss[ply - 1].Move;
            if (lastMove != Move.NullMove)
            {
                counterMoves[GetIndex(lastMove)] = move;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIndex(Color stm, Piece piece, SquareIndex to)
        {
            return ((int)stm * MAX_PIECES + (int)piece) * MAX_SQUARES + (int) to;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIndex(Move move)
        {
            return GetIndex(move.Stm, move.Piece, move.To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateHistory(ref short hist, short bonus)
        {
            hist += (short)(bonus - hist * Math.Abs(bonus) / HISTORY_SCORE_MAX);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short CH(in SearchItem ssItem, int index)
        {
		    if (ssItem.Continuation != null && ssItem.Continuation != nullMoveCont)
		    {
			    return ssItem.Continuation[index];
		    }
		    return 0;        
        }
    }
}
