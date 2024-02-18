using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Pedantic.Collections;

namespace Pedantic.Chess
{
    public unsafe sealed class History : IHistory
    {
        internal const nuint MEM_ALIGNMENT = 64;
        internal const nuint HISTORY_LEN = MAX_COLORS * MAX_PIECES * MAX_SQUARES;
        internal const short BONUS_MAX = 920;
        internal const short BONUS_COEFF = 96;

        private short[] history;
        private SearchStack ss;
        private int ply;

        public History(SearchStack searchStack)
        {
            history = new short[HISTORY_LEN];
            ss = searchStack;
            ply = 0;
        }

        public short this[Color stm, Piece piece, SquareIndex sq]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return history[GetIndex(stm, piece, sq)];
            }
        }

        public short this[Move move]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return history[GetIndex(move)];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Span<short> h = history;
            h.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetContext(int ply)
        {
            this.ply = ply;
        }

        public void UpdateCutoff(Move move, int ply, ref StackList<Move> quiets, int depth)
        {
            SetContext(ply);
            short bonus = Math.Min(BONUS_MAX, (short)(BONUS_COEFF * (depth - 1)));
            int index = GetIndex(move);
            UpdateHistory(ref history[index], bonus);

            short malus = (short)-bonus;
            for (int n = 0; n < quiets.Count; n++)
            {
                index = GetIndex(quiets[n]);
                UpdateHistory(ref history[index], malus);
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
    }
}
