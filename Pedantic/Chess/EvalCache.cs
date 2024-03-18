using Pedantic.Utilities;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Pedantic.Chess
{
    public unsafe sealed class EvalCache : IDisposable
    {
        public const int MB_SIZE = 1024 * 1024;
        public const int DEFAULT_CACHE_SIZE = 64;
        public const int PAWN_CACHE_ITEM_SIZE = 16;
        public const int EVAL_CACHE_ITEM_SIZE = 8;
        public const int MEM_ALIGNMENT = 64;

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public struct PawnCacheItem
        {
            public PawnCacheItem(ulong hash, Bitboard passedPawns, Score eval)
            {
                this.hash = (uint)(hash >> 32);
                this.passedPawns = passedPawns;
                this.eval = eval;
            }

            public uint Hash 
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => hash;
            }

            public Bitboard PassedPawns 
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => passedPawns;
            }

            public Score Eval 
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => eval;
            }

            public static void SetValue(ref PawnCacheItem item, ulong hash, Bitboard passedPawns, Score eval)
            {
                item.hash = (uint)(hash >> 32);
                item.passedPawns = passedPawns;
                item.eval = eval;
            }

            public unsafe static int Size
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return sizeof(PawnCacheItem);
                }
            }

            private Bitboard passedPawns;
            private uint hash;
            private Score eval;
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        public struct EvalCacheItem
        {
            public EvalCacheItem(ulong hash, short evalScore, Color stm)
            {
                this.hash = (uint)(hash >> 32);
                this.evalScore = evalScore;
                this.stm = stm;
            }

            public uint Hash 
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => hash;
            }

            public short EvalScore 
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => evalScore;
            }

            public Color SideToMove
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => stm;
            }

            public static void SetValue(ref EvalCacheItem item, ulong hash, short evalScore, Color stm)
            {
                item.hash = (uint)(hash >> 32);
                item.evalScore = evalScore;
                item.stm = stm;
            }

            public unsafe static int Size
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return sizeof(EvalCacheItem);
                }
            }

            private uint hash;
            private short evalScore;
            private Color stm;
        }

        public EvalCache(int sizeMb = DEFAULT_CACHE_SIZE)
        {
            Util.Assert(PawnCacheItem.Size == PAWN_CACHE_ITEM_SIZE);
            Util.Assert(EvalCacheItem.Size == EVAL_CACHE_ITEM_SIZE);
            pPawnCache = null;
            pEvalCache = null;
            Resize(sizeMb);
        }

        ~EvalCache()
        {
            if (pEvalCache != null)
            {
                NativeMemory.AlignedFree(pEvalCache);
                pEvalCache = null;
            }

            if (pPawnCache != null)
            {
                NativeMemory.AlignedFree(pPawnCache);
                pPawnCache = null;
            }
        }

        public bool ProbeEvalCache(ulong hash, Color stm, out EvalCacheItem item)
        {
            int index = (int)(hash % (uint)evalSize);
            item = pEvalCache[index];
            return item.Hash == (uint)(hash >> 32) && item.SideToMove == stm;
        }

        public void SaveEval(ulong hash, short score, Color stm)
        {
            int index = (int)(hash % (uint)evalSize);
            ref EvalCacheItem item = ref pEvalCache[index];
            EvalCacheItem.SetValue(ref item, hash, score, stm);
        }

        public bool ProbePawnCache(ulong hash, out PawnCacheItem item)
        {
            int index = (int)(hash % (uint)pawnSize);
            item = pPawnCache[index];
            return item.Hash == (uint)(hash >> 32);
        }

        public void SavePawnEval(ulong hash, Bitboard passedPawns, Score eval)
        {
            int index = (int)(hash % (uint)pawnSize);
            ref PawnCacheItem item = ref pPawnCache[index];
            PawnCacheItem.SetValue(ref item, hash, passedPawns, eval);
        }

        public void Resize(int sizeMb)
        {
            CalcCacheSizes(sizeMb, out evalSize, out pawnSize);
            evalByteCount = (nuint)(evalSize * EvalCacheItem.Size);
            pawnByteCount = (nuint)(pawnSize * PawnCacheItem.Size);
            pEvalCache = (EvalCacheItem*)NativeMemory.AlignedRealloc(pEvalCache, evalByteCount, MEM_ALIGNMENT);
            pPawnCache = (PawnCacheItem*)NativeMemory.AlignedRealloc(pPawnCache, pawnByteCount, MEM_ALIGNMENT);
            Clear();
        }

        public void Clear()
        {
            NativeMemory.Clear(pEvalCache, evalByteCount);
            NativeMemory.Clear(pPawnCache, pawnByteCount);
        }

        public static void CalcCacheSizes(int sizeMb, out int evalSize, out int pawnSize)
        {
            sizeMb /= 4;
            sizeMb = Math.Clamp(sizeMb, 4, 512);
            evalSize = sizeMb * MB_SIZE / EvalCacheItem.Size;
            sizeMb /= 4;
            sizeMb = Math.Clamp(sizeMb, 1, 128);
            pawnSize = sizeMb * MB_SIZE / PawnCacheItem.Size;
        }

        public void PrefetchEvalCache(ulong hash, ulong pawnHash)
        {
            if (Sse.IsSupported)
            {
                int index = (int)(hash % (uint)evalSize);
                Sse.Prefetch0(&pEvalCache[index]);
                index = (int)(pawnHash % (uint)pawnSize);
                Sse.Prefetch0(&pPawnCache[index]);
            }
        }

        public void Dispose()
        {
            if (pPawnCache != null)
            {
                NativeMemory.AlignedFree(pPawnCache);
                pPawnCache = null;
            }

            if (pEvalCache != null)
            {
                NativeMemory.AlignedFree(pEvalCache);
                pEvalCache = null;
            }
            GC.SuppressFinalize(this);
        }

        public int PawnCacheSize 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pawnSize;
        }

        public int EvalCacheSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => evalSize;
        }

        private int pawnSize;
        private PawnCacheItem* pPawnCache;
        private nuint pawnByteCount;

        private int evalSize;
        private EvalCacheItem* pEvalCache;
        private nuint evalByteCount;
    }
}
