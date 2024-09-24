// <copyright file="NnueEval.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess.NNUE
{
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using Pedantic.Collections;

    public unsafe class NnueEval : EfficientlyUpdatable, IDisposable
    {
        public const short SCALE = 400;
        public const short QA = 255;
        public const short QB = 64;
        public const short QAB = QA * QB;

        public static readonly Vector<short> CReLU_Max = new Vector<short>(QA);
        public static readonly Vector<short> CReLU_Min = Vector<short>.Zero;

        public struct AccumState
        {
            private fixed short whiteAccum[Network.HIDDEN_SIZE];
            private fixed short blackAccum[Network.HIDDEN_SIZE];

            public void Save(NnueEval nnue)
            {
                fixed (short* wp = &whiteAccum[0])
                {
                    Span<short> ws = new(wp, Network.HIDDEN_SIZE);
                    nnue.WhiteAccum.CopyTo(ws);
                }

                fixed (short* bp = &blackAccum[0])
                {
                    Span<short> bs = new(bp, Network.HIDDEN_SIZE);
                    nnue.BlackAccum.CopyTo(bs);
                }
            }

            public void Restore(NnueEval nnue)
            {
                fixed (short* wp = &whiteAccum[0])
                {
                    ReadOnlySpan<short> ws = new(wp, Network.HIDDEN_SIZE);
                    ws.CopyTo(nnue.WhiteAccum);
                }

                fixed (short* bp = &blackAccum[0])
                {
                    ReadOnlySpan<short> bs = new(bp, Network.HIDDEN_SIZE);
                    bs.CopyTo(nnue.BlackAccum);
                }
            }
        }

        private short* whiteAccum;  // = new short[Network.HIDDEN_SIZE];
        private short* blackAccum;  // = new short[Network.HIDDEN_SIZE];
        private readonly ValueStack<AccumState> accumStack = new(MAX_GAME_LENGTH);
        private short score;
        private bool disposedValue;
        private readonly EvalCache cache;

        private static readonly sbyte[] inputBuckets =
        [
            0, 0, 0, 1, 1, 2, 2, 2,
            0, 0, 0, 1, 1, 2, 2, 2,
            3, 3, 3, 3, 4, 4, 4, 4,
            3, 3, 3, 3, 4, 4, 4, 4,
            3, 3, 3, 3, 4, 4, 4, 4,
            3, 3, 3, 3, 4, 4, 4, 4,
            3, 3, 3, 3, 4, 4, 4, 4,
            3, 3, 3, 3, 4, 4, 4, 4
        ];

        public NnueEval()
        {
            whiteAccum = Network.AlignedAlloc<short>(Network.HIDDEN_SIZE);
            blackAccum = Network.AlignedAlloc<short>(Network.HIDDEN_SIZE);
            cache = new EvalCache();
        }

        public NnueEval(EvalCache cache)
        {
            whiteAccum = Network.AlignedAlloc<short>(Network.HIDDEN_SIZE);
            blackAccum = Network.AlignedAlloc<short>(Network.HIDDEN_SIZE);
            this.cache = cache;
        }

        public Span<short> WhiteAccum
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Span<short>(whiteAccum, Network.HIDDEN_SIZE);
        }

        public Span<short> BlackAccum
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Span<short>(blackAccum, Network.HIDDEN_SIZE);
        }

        public int StateCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => accumStack.Count;
        }

        public short Score
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => score;
        }

        public override void Clear()
        {
            // reset accumulators back to their initial bias
            Network.Default.FeatureBiases.CopyTo(WhiteAccum);
            Network.Default.FeatureBiases.CopyTo(BlackAccum);
            accumStack.Clear();
        }

        public override void Copy(IEfficientlyUpdatable other)
        {
            NnueEval nnue = (NnueEval)other;
            nnue.WhiteAccum.CopyTo(WhiteAccum);
            nnue.BlackAccum.CopyTo(BlackAccum);
        }

        public override void AddPiece(Color color, Piece piece, SquareIndex sq)
        {
            int white = InputIndex(color, piece, sq);
            int black = InputIndex(color.Flip(), piece, sq.Flip());

            AddAccums(WhiteAccum, Network.Default.FeatureWeights, white * Network.HIDDEN_SIZE);
            AddAccums(BlackAccum, Network.Default.FeatureWeights, black * Network.HIDDEN_SIZE);
        }

        public override void RemovePiece(Color color, Piece piece, SquareIndex sq)
        {
            int white = InputIndex(color, piece, sq);
            int black = InputIndex(color.Flip(), piece, sq.Flip());

            SubAccums(WhiteAccum, Network.Default.FeatureWeights, white * Network.HIDDEN_SIZE);
            SubAccums(BlackAccum, Network.Default.FeatureWeights, black * Network.HIDDEN_SIZE);
        }

        public override void Update(Board board)
        {
            Clear();

            foreach (SquareIndex sq in board.All)
            {
                Square square = board.PieceBoard(sq);
                AddPiece(square.Color, square.Piece, sq);
            }
        }

        public override void UpdatePiece(Color color, Piece piece, SquareIndex from, SquareIndex to)
        {
            int white = InputIndex(color, piece, from);
            int black = InputIndex(color.Flip(), piece, from.Flip());

            SubAccums(WhiteAccum, Network.Default.FeatureWeights, white * Network.HIDDEN_SIZE);
            SubAccums(BlackAccum, Network.Default.FeatureWeights, black * Network.HIDDEN_SIZE);

            white = InputIndex(color, piece, to);
            black = InputIndex(color.Flip(), piece, to.Flip());

            AddAccums(WhiteAccum, Network.Default.FeatureWeights, white * Network.HIDDEN_SIZE);
            AddAccums(BlackAccum, Network.Default.FeatureWeights, black * Network.HIDDEN_SIZE);
        }

        public override void PushState()
        {
            accumStack.Push().Save(this);
        }

        public override void RestoreState()
        {
            accumStack.Peek().Restore(this);
        }

        public override void PopState()
        {
            accumStack.Pop();
        }

        public short ComputeUncached(Board board)
        {
            Update(board);

            int eval = board.SideToMove == Color.White ?
                Activation(WhiteAccum, BlackAccum, Network.Default.OutputWeights) :
                Activation(BlackAccum, WhiteAccum, Network.Default.OutputWeights);

            score = (short)((eval / QA + Network.Default.OutputBias) * SCALE / QAB);
            return score;
        }

        public short Compute(Board board)
        {
            if (cache.ProbeEvalCache(board.Hash, board.SideToMove, out EvalCache.EvalCacheItem item))
            {
                return item.EvalScore;
            }

            int eval = board.SideToMove == Color.White ?
                Activation(WhiteAccum, BlackAccum, Network.Default.OutputWeights) :
                Activation(BlackAccum, WhiteAccum, Network.Default.OutputWeights);

            score = (short)((eval / QA + Network.Default.OutputBias) * SCALE / QAB);

            if (UciOptions.RandomSearch)
            {
                score += (short)Random.Shared.Next(-8, 9);
            }

            cache.SaveEval(board.Hash, score, board.SideToMove);
            return score;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prefetch(Board board)
        {
            cache.PrefetchEvalCache(board.Hash);
        }

        private static void AddAccums(Span<short> accumulators, ReadOnlySpan<short> hiddenWeights, int index)
        {
            // See https://stackoverflow.com/questions/64729099/system-numerics-vectort-initialization-performance-on-net-framework
            Span<Vector<short>> acc = MemoryMarshal.Cast<short, Vector<short>>(accumulators);
            ReadOnlySpan<Vector<short>> wts = MemoryMarshal.Cast<short, Vector<short>>(hiddenWeights.Slice(index, Network.HIDDEN_SIZE));

            for (int n = 0; n < acc.Length; n++)
            {
                acc[n] += wts[n];
            }
        }

        private static void SubAccums(Span<short> accumulators, ReadOnlySpan<short> hiddenWeights, int index)
        {
            // See https://stackoverflow.com/questions/64729099/system-numerics-vectort-initialization-performance-on-net-framework
            Span<Vector<short>> acc = MemoryMarshal.Cast<short, Vector<short>>(accumulators);
            ReadOnlySpan<Vector<short>> wts = MemoryMarshal.Cast<short, Vector<short>>(hiddenWeights.Slice(index, Network.HIDDEN_SIZE));

            for (int n = 0; n < acc.Length; n++)
            {
                acc[n] -= wts[n];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int InputIndex(Color color, Piece piece, SquareIndex sq)
        {
            return (((int)color * MAX_PIECES) + (int)piece) * MAX_SQUARES + (int)sq;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Activation(ReadOnlySpan<short> us, ReadOnlySpan<short> them, ReadOnlySpan<short> weights)
        {
            return ActivationSCReLU(us, weights.Slice(0, Network.HIDDEN_SIZE)) + 
                   ActivationSCReLU(them, weights.Slice(Network.HIDDEN_SIZE, Network.HIDDEN_SIZE));
        }

        private static int ActivationSCReLU(ReadOnlySpan<short> accum, ReadOnlySpan<short> weights)
        {
            // SCReLU starts off like CReLU

            Vector<int> sum = Vector<int>.Zero;

            ReadOnlySpan<Vector<short>> acc = MemoryMarshal.Cast<short, Vector<short>>(accum);
            ReadOnlySpan<Vector<short>> wts = MemoryMarshal.Cast<short, Vector<short>>(weights);

            for (int n = 0; n < acc.Length; n++)
            {
                // widen to int32 to prevent overflows which reduces ops by 1/2 :`(
                Vector.Widen(CReLU(acc[n]), out Vector<int> accLow, out Vector<int> accHigh);
                Vector.Widen(wts[n], out Vector<int> wtsLow, out Vector<int> wtsHigh);

                // special sauce: perform the squared part of SCReLU
                sum += accLow * accLow * wtsLow;
                sum += accHigh * accHigh * wtsHigh;
            }

            return Vector.Sum(sum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<short> CReLU(Vector<short> vec)
        {
            return Vector.Max(Vector.Min(vec, CReLU_Max), CReLU_Min);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                if (disposing)
                {
                    cache.Dispose();
                }

                Network.AlignedFree(whiteAccum);
                Network.AlignedFree(blackAccum);
                whiteAccum = null;
                blackAccum = null;
            }
        }

        ~NnueEval()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
