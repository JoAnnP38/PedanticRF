using System.Runtime.CompilerServices;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public sealed class PvTable
    {
        internal const int TABLE_LEN = MAX_PLY * MAX_PLY;
        private readonly Move[] pvTable = new Move[TABLE_LEN];
        private readonly int[] pvLength = new int[MAX_PLY];

        public Move this[int ply, int n]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pvTable[GetIndex(ply, n)];
    
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => pvTable[GetIndex(ply, n)] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitPly(int ply)
        {
            Util.Assert(ply >= 0 && ply < MAX_PLY);
            pvLength[ply] = 0;
        }

        public void MergeMove(int ply, Move move)
        {
            pvLength[ply] = pvLength[ply + 1] + 1;
            this[ply, 0] = move;
            Array.Copy(pvTable, GetIndex(ply + 1, 0), pvTable, GetIndex(ply, 1), pvLength[ply + 1]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<Move> GetPv()
        {
            return new ReadOnlySpan<Move>(pvTable, 0, pvLength[0]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(int ply, int n)
        {
            Util.Assert(ply >= 0 && ply < MAX_PLY);
            Util.Assert(n >= 0 && n < MAX_PLY);
            return ply * MAX_PLY + n;
        }
    }
}
