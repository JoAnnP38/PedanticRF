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

    public class NnueEval : EfficientlyUpdatable
    {
        public const short SCALE = 400;
        public const short QA = 255;
        public const short QB = 64;
        public const short QAB = QA * QB;

        public unsafe struct AccumState
        {
            private fixed short whiteAccum[Network.HIDDEN_SIZE];
            private fixed short blackAccum[Network.HIDDEN_SIZE];

            public void Save(NnueEval nnue)
            {
                fixed (short* wp = &whiteAccum[0])
                {
                    Span<short> ws = new(wp, Network.HIDDEN_SIZE);
                    nnue.whiteAccum.CopyTo(ws);
                }

                fixed (short* bp = &blackAccum[0])
                {
                    Span<short> bs = new(bp, Network.HIDDEN_SIZE);
                    nnue.blackAccum.CopyTo(bs);
                }
            }

            public void Restore(NnueEval nnue)
            {
                fixed (short* wp = &whiteAccum[0])
                {
                    ReadOnlySpan<short> ws = new(wp, Network.HIDDEN_SIZE);
                    ws.CopyTo(nnue.whiteAccum);
                }

                fixed (short* bp = &blackAccum[0])
                {
                    ReadOnlySpan<short> bs = new(bp, Network.HIDDEN_SIZE);
                    bs.CopyTo(nnue.blackAccum);
                }
            }
        }

        private readonly short[] whiteAccum = new short[Network.HIDDEN_SIZE];
        private readonly short[] blackAccum = new short[Network.HIDDEN_SIZE];
        private readonly ValueStack<AccumState> accumStack = new(MAX_GAME_LENGTH);
        private short score;
        private readonly EvalCache cache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NnueEval()
        {
            cache = new EvalCache();
        }

        public NnueEval(EvalCache cache)
        {
            this.cache = cache;
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
            Network.Default.HiddenBiases.CopyTo(whiteAccum);
            Network.Default.HiddenBiases.CopyTo(blackAccum);
            accumStack.Clear();
        }

        public override void Copy(IEfficientlyUpdatable other)
        {
            NnueEval nnue = (NnueEval)other;
            Array.Copy(nnue.whiteAccum, whiteAccum, Network.HIDDEN_SIZE);
            Array.Copy(nnue.blackAccum, blackAccum, Network.HIDDEN_SIZE);
        }

        public override void AddPiece(Color color, Piece piece, SquareIndex sq)
        {
            int white = InputIndex(color, piece, sq);
            int black = InputIndex(color.Flip(), piece, sq.Flip());

            AddAccums(whiteAccum, Network.Default.HiddenWeights, white * Network.HIDDEN_SIZE);
            AddAccums(blackAccum, Network.Default.HiddenWeights, black * Network.HIDDEN_SIZE);
        }

        public override void RemovePiece(Color color, Piece piece, SquareIndex sq)
        {
            int white = InputIndex(color, piece, sq);
            int black = InputIndex(color.Flip(), piece, sq.Flip());

            SubAccums(whiteAccum, Network.Default.HiddenWeights, white * Network.HIDDEN_SIZE);
            SubAccums(blackAccum, Network.Default.HiddenWeights, black * Network.HIDDEN_SIZE);
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

            SubAccums(whiteAccum, Network.Default.HiddenWeights, white * Network.HIDDEN_SIZE);
            SubAccums(blackAccum, Network.Default.HiddenWeights, black * Network.HIDDEN_SIZE);

            white = InputIndex(color, piece, to);
            black = InputIndex(color.Flip(), piece, to.Flip());

            AddAccums(whiteAccum, Network.Default.HiddenWeights, white * Network.HIDDEN_SIZE);
            AddAccums(blackAccum, Network.Default.HiddenWeights, black * Network.HIDDEN_SIZE);
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
                Activation(whiteAccum, blackAccum, Network.Default.OutputWeights) :
                Activation(blackAccum, whiteAccum, Network.Default.OutputWeights);

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
                Activation(whiteAccum, blackAccum, Network.Default.OutputWeights) :
                Activation(blackAccum, whiteAccum, Network.Default.OutputWeights);

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

        private static int ActivationCReLU(ReadOnlySpan<short> accum, ReadOnlySpan<short> weights)
        {
            Vector<short> CReLU_Max = new Vector<short>(QA);
            Vector<short> CReLU_Min = Vector<short>.Zero; 
            
            Vector<int> sum = Vector<int>.Zero;

            ReadOnlySpan<Vector<short>> acc = MemoryMarshal.Cast<short, Vector<short>>(accum);
            ReadOnlySpan<Vector<short>> wts = MemoryMarshal.Cast<short, Vector<short>>(weights);

            for (int n = 0; n < acc.Length; n++)
            {
                Vector<short> a = Vector.Max(Vector.Min(acc[n], CReLU_Max), CReLU_Min);
                Vector<short> w = wts[n];

                Vector.Widen(a, out Vector<int> aLow, out Vector<int> aHigh);
                Vector.Widen(w, out Vector<int> wLow, out Vector<int> wHigh);

                sum += aLow * wLow;
                sum += aHigh * wHigh;
            }

            return Vector.Sum(sum);
        }


        private static int ActivationSCReLU(ReadOnlySpan<short> accum, ReadOnlySpan<short> weights)
        {
            // SCReLU starts off like CReLU
            Vector<short> CReLU_Max = new Vector<short>(QA);
            Vector<short> CReLU_Min = Vector<short>.Zero;

            Vector<int> sum = Vector<int>.Zero;

            ReadOnlySpan<Vector<short>> acc = MemoryMarshal.Cast<short, Vector<short>>(accum);
            ReadOnlySpan<Vector<short>> wts = MemoryMarshal.Cast<short, Vector<short>>(weights);

            for (int n = 0; n < acc.Length; n++)
            {
                Vector<short> a = Vector.Max(Vector.Min(acc[n], CReLU_Max), CReLU_Min);
                Vector<short> w = wts[n];

                // widen to int32 to prevent overflows which reduces ops by 1/2 :`(
                Vector.Widen(a, out Vector<int> accLow, out Vector<int> accHigh);
                Vector.Widen(w, out Vector<int> wtsLow, out Vector<int> wtsHigh);

                // special sauce: perform the squared part of SCReLU
                sum += accLow * accLow * wtsLow;
                sum += accHigh * accHigh * wtsHigh;
            }

            return Vector.Sum(sum);
        }
    }
}
