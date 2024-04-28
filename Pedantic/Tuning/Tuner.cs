// <copyright file="Tuner.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Tuning
{
    using System.Runtime.CompilerServices;

    public abstract class Tuner
    {
        public const double GOLDEN_RATIO = 1.618033988749894;
        public const double DEFAULT_K = 0.00385;
        public const double TOLERENCE = 1.0e-7;

        protected Tuner(List<PosRecord> positions)
        {
            this.positions = positions;

#if DEBUG
            rand = new Random(1);
#else
            rand = new Random();
#endif
        }

        public abstract (double Error, double Accuracy, Chess.HCE.Weights Weights, double K) Train(int maxEpoch, 
            TimeSpan? maxTime, double minError, double precision = TOLERENCE);

        public abstract double SolveK(double a = 0.0, double b = 1.0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sigmoid(double k, double eval)
        {
            return 1.0 / (1.0 + Math.Exp(-k * eval));
        }

        protected double k;
        protected readonly List<PosRecord> positions;
        protected readonly Random rand;
    }
}

