using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public unsafe sealed class TtCache : IDisposable
    {
        public const int ITEM_SIZE = 16;
        public const int MB_SIZE = 1024 * 1024;
        public const int CAPACITY_MULTIPLIER = MB_SIZE / ITEM_SIZE;
        public const int MEM_ALIGNMENT = 64;

        public readonly struct TtItem
        {
            private readonly ulong hash;
            private readonly ulong data;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TtItem(ulong hash, short score, Bound bound, byte depth, byte age)
                : this(hash, score, bound, depth, age, Move.NullMove)
            { }

            public TtItem(ulong hash, short score, Bound bound, byte depth, ushort age, Move bestMove)
            {
                data = ((ulong)bestMove & 0x01ffffffful)
                     | (((ulong)score & 0x0fffful) << 29)
                     | (((ulong)bound & 0x03ul) << 45)
                     | (((ulong)depth & 0x0fful) << 47)
                     | (((ulong)age & 0x01fful) << 55)
                     ;
                this.hash = hash ^ data;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsValid(ulong hash) => Hash == hash;

            public ulong Hash => hash ^ data;
            public ulong Data => data;
            public Move BestMove => (Move)(uint)BitOps.BitFieldExtract(data, 0, 29);
            public short Score => (short)BitOps.BitFieldExtract(data, 29, 16);
            public Bound Bound => (Bound)BitOps.BitFieldExtract(data, 45, 2);
            public byte Depth => (byte)BitOps.BitFieldExtract(data, 47, 8);
            public ushort Age => (ushort)BitOps.BitFieldExtract(data, 55, 9);
        }

        public TtCache()
        {
            Util.Assert(sizeof(TtItem) == ITEM_SIZE);
            Resize();
        }

        ~TtCache()
        {
            if (pTable != null)
            {
                NativeMemory.AlignedFree(pTable);
                pTable = null;
            }
        }

        public int Usage => (int)((used * 1000L) / capacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            NativeMemory.Clear(pTable, byteCount);
        }

        public void Resize()
        {
            int sizeMb = Math.Clamp(UciOptions.HashTableSize, 16, 2048);
            if (!BitOps.IsPow2(sizeMb))
            {
                sizeMb = BitOps.GreatestPowerOfTwoLessThan(sizeMb);
                UciOptions.HashTableSize = sizeMb;
            }

            capacity = sizeMb * CAPACITY_MULTIPLIER;
            byteCount = (nuint)(sizeMb * MB_SIZE);
            pTable = (TtItem*)NativeMemory.AlignedRealloc(pTable, byteCount, MEM_ALIGNMENT);
            mask = (uint)(capacity - 1);
            generation = 1;
            Clear();
        }

        public bool Probe(ulong hash, int depth, int ply, int alpha, int beta, out int ttScore, out TtItem ttItem)
        {
            ttScore = NO_SCORE;
            if (!TryGetItem(hash, out ttItem) || ttItem.Depth < depth)
            {
                return false;
            }

            ttScore = ttItem.Score;
            if (ttScore >= TABLEBASE_WIN)
            {
                ttScore -= ply;
            }
            else if (ttScore <= TABLEBASE_LOSS)
            {
                ttScore += ply;
            }

            Bound bound = ttItem.Bound;
            return bound == Bound.Exact || 
                (bound == Bound.Upper && ttScore <= alpha) || 
                (bound == Bound.Lower && ttScore >= beta);
        }

        public void Store(ulong hash, int depth, int ply, int alpha, int beta, int score, Move move)
        {
            int index = GetStoreIndex(hash, out TtItem ttItem);
            Move bestMove = move;

            if (ttItem.IsValid(hash) && bestMove == Move.NullMove)
            {
                bestMove = ttItem.BestMove;
            }

            if (ttItem.Age != generation)
            {
                used++;
            }

            if (score >= TABLEBASE_WIN)
            {
                score += ply;
            }
            else if (score <= TABLEBASE_LOSS)
            {
                score -= ply;
            }

            Bound bound = Bound.Exact;
            if (score <= alpha)
            {
                bound = Bound.Upper;
            }
            else if (score >= beta)
            {
                bound = Bound.Lower;
            }

            pTable[index] = new TtItem(hash, (short)score, bound, (byte)depth, generation, bestMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementGeneration()
        {
            generation++;
            used = 0;
        }

        public void Dispose()
        {
            if (pTable != null)
            {
                NativeMemory.AlignedFree(pTable);
                pTable = null;
            }
            GC.SuppressFinalize(this);
        }

        private bool TryGetItem(ulong hash, out TtItem item)
        {
            int index = (int)(hash & mask);
            item = pTable[index];
            if (!item.IsValid(hash))
            {
                index ^= 1;
                item = pTable[index];

                if (!item.IsValid(hash))
                {
                    return false;
                }
            }
            return true;
        }

        private int GetStoreIndex(ulong hash, out TtItem item)
        {
            int index0 = (int)(hash & mask);
            TtItem item0 = pTable[index0];

            if (item0.IsValid(hash))
            {
                item = item0;
                return index0;
            }

            int index1 = index0 ^ 1;
            TtItem item1 = pTable[index1];

            if (item1.IsValid(hash))
            {
                item = item1;
                return index1;
            }

            if (item0.Age < item1.Age)
            {
                item = item0;
                return index0;
            }
            else if (item0.Age > item1.Age)
            {
                item = item1;
                return index1;
            }
            else if (item0.Depth <= item1.Depth)
            {
                item = item0;
                return index0;
            }

            item = item1;
            return index1;
        }

        public static readonly TtCache Default = new ();

        private TtItem* pTable;
        private nuint byteCount;
        private int capacity;
        private int used;
        private uint mask;
        private ushort generation;
    }
}
