// <copyright file="EvalCache.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Intrinsics.X86;
    using Pedantic.Utilities;

    public unsafe sealed class EvalCache : IDisposable
    {
        public const int MB_SIZE = 1024 * 1024;
        public const int DEFAULT_CACHE_SIZE = 64;
        public const int EVAL_CACHE_ITEM_SIZE = 8;
        public const int MEM_ALIGNMENT = 64;

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
            Util.Assert(EvalCacheItem.Size == EVAL_CACHE_ITEM_SIZE);
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

        public void Resize(int sizeMb)
        {
            CalcCacheSize(sizeMb, out evalSize);
            evalByteCount = (nuint)(evalSize * EvalCacheItem.Size);
            pEvalCache = (EvalCacheItem*)NativeMemory.AlignedRealloc(pEvalCache, evalByteCount, MEM_ALIGNMENT);
            Clear();
        }

        public void Clear()
        {
            NativeMemory.Clear(pEvalCache, evalByteCount);
        }

        public static void CalcCacheSize(int sizeMb, out int evalSize)
        {
            sizeMb /= 2;
            sizeMb = Math.Clamp(sizeMb, 8, 1024);
            evalSize = sizeMb * MB_SIZE / EvalCacheItem.Size;
        }

        public void PrefetchEvalCache(ulong hash)
        {
            if (Sse.IsSupported)
            {
                int index = (int)(hash % (uint)evalSize);
                Sse.Prefetch0(&pEvalCache[index]);
            }
        }

        public void Dispose()
        {
            if (pEvalCache != null)
            {
                NativeMemory.AlignedFree(pEvalCache);
                pEvalCache = null;
            }
            GC.SuppressFinalize(this);
        }

        public int EvalCacheSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => evalSize;
        }

        private int evalSize;
        private EvalCacheItem* pEvalCache;
        private nuint evalByteCount;
    }
}
