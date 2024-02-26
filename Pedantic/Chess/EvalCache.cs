using Pedantic.Utilities;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Pedantic.Chess
{
    public unsafe sealed class EvalCache : IDisposable
    {
        public const int MB_SIZE = 1024 * 1024;
        public const int DEFAULT_CACHE_SIZE = 4;
        public const int PAWN_CACHE_ITEM_SIZE = 20;
        public const int MEM_ALIGNMENT = 64;

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public struct PawnCacheItem
        {
            public PawnCacheItem(ulong hash, Bitboard passedPawns, Score eval)
            {
                this.hash = hash;
                this.passedPawns = passedPawns;
                this.eval = eval;
            }

            public readonly ulong Hash => hash;
            public readonly Bitboard PassedPawns => passedPawns;
            public readonly Score Eval => eval;

            public static void SetValue(ref PawnCacheItem item, ulong hash, Bitboard passedPawns, Score eval)
            {
                item.hash = hash;
                item.passedPawns = passedPawns;
                item.eval = eval;
            }

            public unsafe static int Size
            {
                get
                {
                    return sizeof(PawnCacheItem);
                }
            }

            private ulong hash;
            private Bitboard passedPawns;
            private Score eval;
        }

        public EvalCache(int sizeMb = DEFAULT_CACHE_SIZE)
        {
            Util.Assert(sizeof(PawnCacheItem) == PAWN_CACHE_ITEM_SIZE);
            pPawnCache = null;
            Resize(sizeMb);
        }

        ~EvalCache()
        {
            if (pPawnCache != null)
            {
                NativeMemory.AlignedFree(pPawnCache);
                pPawnCache = null;
            }
        }

        public bool ProbePawnCache(ulong hash, out PawnCacheItem item)
        {
            int index = (int)(hash % (uint)pawnSize);
            item = pPawnCache[index];
            return item.Hash == hash;
        }

        public void SavePawnEval(ulong hash, Bitboard passedPawns, Score eval)
        {
            int index = (int)(hash % (uint)pawnSize);
            ref PawnCacheItem item = ref pPawnCache[index];
            PawnCacheItem.SetValue(ref item, hash, passedPawns, eval);
        }

        public void Resize(int sizeMb)
        {
            CalcCacheSizes(sizeMb, out pawnSize);
            byteCount = (nuint)(sizeMb * MB_SIZE);
            pPawnCache = (PawnCacheItem*)NativeMemory.AlignedRealloc(pPawnCache, byteCount, MEM_ALIGNMENT);
            Clear();
        }

        public void Clear()
        {
            NativeMemory.Clear(pPawnCache, byteCount);
        }

        public static void CalcCacheSizes(int sizeMb, out int pawnSize)
        {
            sizeMb = Math.Clamp(sizeMb, 1, 128);
            pawnSize = sizeMb * MB_SIZE / PawnCacheItem.Size;
        }

        public void PrefetchPawnCache(ulong hash)
        {
            if (Sse.IsSupported)
            {
                int index = (int)(hash % (uint)pawnSize);
                Sse.Prefetch0(pPawnCache + index);
            }
        }

        public void Dispose()
        {
            if (pPawnCache != null)
            {
                NativeMemory.AlignedFree(pPawnCache);
                pPawnCache = null;
            }
            GC.SuppressFinalize(this);
        }

        public int PawnCacheSize => pawnSize;

        private int pawnSize;
        private PawnCacheItem* pPawnCache;
        private nuint byteCount;
    }
}
