using Pedantic.Chess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pedantic.Tuning
{
    public readonly struct PosRecord
    {

        public const float WDL_WIN = 1.0f;
        public const float WDL_DRAW  = 0.5f;
        public const float WDL_LOSS = 0.0f;

        private readonly int progress;
        public readonly short Eval;
        public readonly EvalFeatures Features;
        public readonly byte result;

        public PosRecord(int ply, int gamePly, string fen, short eval, float fResult)
        {
            progress = ply * 1000 / gamePly;
            Eval = eval;
            Features = new EvalFeatures(new Board(fen), Engine.Weights);
            result = fResult switch
            {
                WDL_WIN => 2,
                WDL_DRAW => 1,
                WDL_LOSS => 0,
                _ => 1
            };
        }

        public PosRecord(int ply, int gamePly, ReadOnlySpan<char> fen, short eval, float fResult)
        {
            progress = (ply * 1000) / gamePly;
            Eval = eval;
            Features = new EvalFeatures(new Board(fen), Engine.Weights);
            result = fResult switch
            {
                WDL_WIN => 2,
                WDL_DRAW => 1,
                WDL_LOSS => 0,
                _ => 1
            };
        }

        public double CombinedResult(double k)
        {
            double ratio = EvalRatio;
            return ratio * Tuner.Sigmoid(k, Eval) + (1.0 - ratio) * Result;
        }

        public double Progress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => progress / 1000.0;
        }
        public double EvalRatio
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (1.0 - Progress * Progress) * (EvalPct / 100.0);
        }

        public float Result
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return result switch
                {
                    2 => WDL_WIN,
                    1 => WDL_DRAW,
                    0 => WDL_LOSS,
                    _ => WDL_DRAW
                };
            }
        }

        public static int EvalPct { get; set; } = 0;

    }
}
