// <copyright file="GdTuner.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Tuning
{
    using System.Collections.Concurrent;
    using Pedantic.Chess;
    using Pedantic.Utilities;
    using static Pedantic.Chess.HCE.Weights;

    public class GdTuner : Tuner
    {
        public const double initial_learning_rate = 1.0;
        public const double lr_decay = 0.5;
        public const double lr_step = 1000;

        public struct WeightPair
        {
            public WeightPair(Score s)
            {
                MG = s.MgScore;
                EG = s.EgScore;
            }

            public static WeightPair operator +(WeightPair lhs, WeightPair rhs)
            {
                WeightPair result;
                result.MG = lhs.MG + rhs.MG;
                result.EG = lhs.EG + rhs.EG;
                return result;
            }

            public override readonly string ToString()
            {
                return $"({MG:F6}, {EG:F6})";
            }

            public static explicit operator Score(WeightPair pair) => new((short)Math.Round(pair.MG), (short)Math.Round(pair.EG));

            public double MG;
            public double EG;
        }

        public GdTuner(Chess.HCE.Weights weights, List<PosRecord> positions)
            : base(positions)
        {
            this.weights = new WeightPair[MAX_WEIGHTS];
            CopyWeights(weights, this.weights);
            gradient = new WeightPair[MAX_WEIGHTS];
            k = SolveK();
            if ((k > -TOLERENCE && k < TOLERENCE) || (k > 1.0 - TOLERENCE && k < 1.0 + TOLERENCE))
            {
                k = DEFAULT_K;
            }
            lRate = initial_learning_rate;
        }

        public GdTuner(List<PosRecord> positions)
            : base(positions)
        {
            weights = ZeroWeights();
            gradient = new WeightPair[MAX_WEIGHTS];
            k = DEFAULT_K;
            lRate = initial_learning_rate;
        }

        public override (double Error, double Accuracy, Chess.HCE.Weights Weights, double K) Train(int maxEpoch, TimeSpan? maxTime, 
            double minError = 0.0, double precision = TOLERENCE)
        {
            WeightPair[] momentum = new WeightPair[weights.Length];
            WeightPair[] velocity = new WeightPair[weights.Length];

            const double beta1 = 0.9;
            const double beta2 = 0.999;
            DateTime start = DateTime.Now;

            Console.WriteLine($"Data size: {positions.Count}, K: {k:F6}, Start time: {start:h\\:mm\\:ss}");
            double currError = MeanSquaredError(k);
            double bestError = currError + TOLERENCE * 2;
            double accuracy = Accuracy();
            int epoch = 0;

            Console.WriteLine($"Epoch {epoch,5} - \u03B5: {currError:F6}, Accuracy {accuracy:F4}");

            while (epoch < maxEpoch && currError > minError && (bestError - currError) >= TOLERENCE && (maxTime == null || DateTime.Now - start < maxTime))
            {
                ComputeGradient();
                
                for (int n = 0; n < weights.Length; n++)
                {
                    double grad = -k * gradient[n].MG / positions.Count;
                    momentum[n].MG = beta1 * momentum[n].MG + (1.0 - beta1) * grad;
                    velocity[n].MG = beta2 * velocity[n].MG + (1.0 - beta2) * grad * grad;
                    weights[n].MG -= lRate * momentum[n].MG / (1e-8 + Math.Sqrt(velocity[n].MG));

                    grad = -k * gradient[n].EG / positions.Count;
                    momentum[n].EG = beta1 * momentum[n].EG + (1.0 - beta1) * grad;
                    velocity[n].EG = beta2 * velocity[n].EG + (1.0 - beta2) * grad * grad;
                    weights[n].EG -= lRate * momentum[n].EG / (1e-8 + Math.Sqrt(velocity[n].EG));
                }

                if (++epoch % 100 == 0)
                {
                    bestError = currError;
                    currError = MeanSquaredError(k);
                    accuracy = Accuracy();
                    TimeSpan elapsed = DateTime.Now - start;
                    double epochsPerSec = epoch / elapsed.TotalSeconds;
                    Console.WriteLine($"Epoch {epoch, 5} - \u03B5: {currError:F6}, Accuracy {accuracy:F4}, Epoch/sec {epochsPerSec:F3}, elapsed: {elapsed:d\\.hh\\:mm\\:ss}");

                    //lRate = initial_learning_rate * Math.Pow(lr_decay, Math.Floor((1.0 + epoch) / lr_step));
                }
            }

            currError = MeanSquaredError(k);
            accuracy = Accuracy();
            k = SolveK(k);
            Chess.HCE.Weights nWeights = new();
            CopyWeights(weights, nWeights);
            return (currError, accuracy, nWeights, k);
        }

        public override double SolveK(double a = 0.0, double b = 1.0)
        {
            double k1 = b - (b - a) / GOLDEN_RATIO;
            double k2 = a + (b - a) / GOLDEN_RATIO;

            while (Math.Abs(b - a) > TOLERENCE)
            {
                double f1 = MeanSquaredError(k1);
                double f2 = MeanSquaredError(k2);

                if (f1 < f2)
                {
                    b = k2;
                }
                else
                {
                    a = k1;
                }
                k1 = b - (b - a) / GOLDEN_RATIO;
                k2 = a + (b - a) / GOLDEN_RATIO;
            }

            return (b + a) / 2.0;
        }

        private void ComputeGradient()
        {
            ConcurrentBag<WeightPair[]> gradients = new();
            Array.Clear(gradient);

            Parallel.For(0, positions.Count, () => new WeightPair[MAX_WEIGHTS],
                (j, loop, grad) =>
                {
                    UpdateSingleGradient(grad, positions[j]);
                    return grad;
                },
                    gradients.Add
            );

            foreach (var grad in gradients)
            {
                for (int n = 0; n < MAX_WEIGHTS; n++)
                {
                    gradient[n] += grad[n];
                }
            }
        }

        public void UpdateSingleGradient(WeightPair[] grad, PosRecord pos)
        {
            double sig = Sigmoid(k, ComputeEval(pos));
            double res = (pos.CombinedResult(k) - sig) * sig * (1.0 - sig);
            double mgBase = res * pos.Features.Phase / MAX_PHASE;
            double egBase = res - mgBase;
            double egScale = pos.Features.DrawRatio.Scale / pos.Features.DrawRatio.Divisor;

            foreach (var kvp in pos.Features.Coefficients)
            {
                grad[kvp.Key].MG += mgBase * kvp.Value;
                grad[kvp.Key].EG += egBase * kvp.Value * egScale;
            }
        }

        private double MeanSquaredError(double k)
        {
            ConcurrentBag<double> subtotals = new();

            Parallel.For(0, positions.Count, () => 0.0, (j, loop, subtotal) =>
            {
                double result = positions[j].CombinedResult(k) - Sigmoid(k, ComputeEval(positions[j]));
                subtotal += result * result;
                return subtotal;
            },
                subtotals.Add
            );

            return subtotals.Sum() / positions.Count;
        }

        private double Accuracy()
        {
            ConcurrentBag<(int correct, int wrong)> subtotals = new();

            Parallel.For(0, positions.Count, () => (correct: 0, wrong: 0), (j, loop, subtotal) =>
            {
                double normalized = Sigmoid(k, ComputeEval(positions[j]));
                float predicted = normalized switch
                {
                    >= 0.00 and <= 0.33 => 0.0f,    // white loses
                    > 0.33 and < 0.67 => 0.5f,      // draw
                    >= 0.67 and <= 1.00 => 1.0f,    // white wins
                    _ => -1.0f
                };
                if (predicted == positions[j].Result)
                {
                    subtotal = (subtotal.correct + 1, subtotal.wrong);
                }
                else
                {
                    subtotal = (subtotal.correct, subtotal.wrong + 1);
                }
                return subtotal;
            },
                subtotals.Add
            );

            (int correct, int wrong) total = (0, 0);
            foreach (var (correct, wrong) in subtotals)
            {
                total = (total.correct + correct, total.wrong + wrong);
            }
            return (double)total.correct / (total.correct + total.wrong);
        }

        private static void CopyWeights(Chess.HCE.Weights src, WeightPair[] dst)
        {
            for (int n = 0; n < src.Length; n++)
            {
                dst[n] = new WeightPair(src[n]);
            }
        }

        private static void CopyWeights(WeightPair[] src, Chess.HCE.Weights dst)
        {
            Util.Assert(src.Length == dst.Length);
            for (int n = 0; n < src.Length; n++)
            { 
                dst[n] = (Score)src[n];
            }
        }

        private static WeightPair[] ZeroWeights()
        {
            Chess.HCE.Weights weights = new();
            WeightPair[] wts = new WeightPair[MAX_WEIGHTS];
            CopyWeights(weights, wts);
            return wts;
        }

        private double ComputeEval(PosRecord p)
        {
            double opening = 0.0, endgame = 0.0;

            foreach (var kvp in p.Features.Coefficients)
            {
                opening += kvp.Value * weights[kvp.Key].MG;
                endgame += kvp.Value * weights[kvp.Key].EG;
            }

            double phase = p.Features.Phase;
            double egScaled = endgame * p.Features.DrawRatio.Scale / p.Features.DrawRatio.Divisor;
            double eval = (opening * phase + egScaled * (MAX_PHASE - phase)) / MAX_PHASE;
            return eval;
        }

        private readonly WeightPair[] weights;
        private readonly WeightPair[] gradient;
        private double lRate = 1.0;
    }
}

