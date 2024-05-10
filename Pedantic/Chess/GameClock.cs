// <copyright file="GameClock.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;

    public sealed class GameClock : ICloneable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameClock()
        {
            Reset();
        }

        private GameClock(GameClock other)
        {
            t0 = other.t0;
            tN = other.tN;
            timeBudget = other.timeBudget;
            adjustedBudget = other.adjustedBudget;
            timeLimit = other.timeLimit;
            absoluteLimit = other.absoluteLimit;
            difficulty = other.difficulty;
            remaining = other.remaining;
        }

        public bool Infinite 
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get; 

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set; 
        }

        public static long Now
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Stopwatch.GetTimestamp();
        }

        public int Elapsed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Milliseconds(Now - t0);
        }

        public int ElapsedInterval
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Milliseconds(Now - tN);
        }

        public int TimeLimit
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => timeLimit;
        }

        public Uci Uci
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => uci;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => uci = value;
        }

        public void Reset()
        {
            t0 = Now;
            tN = t0;
            timeBudget = 0;
            timeLimit = 0;
            absoluteLimit = 0;
            difficulty = 100;
            Infinite = false;
            LoadParameters();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            Infinite = false;
            timeLimit = 0;
            absoluteLimit = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartInterval() => tN = Now;

        public void Go(int timePerMove, bool ponder = false)
        {
            Reset();
            timeBudget = 0;
            remaining = Math.Min(timePerMove, max_time_remaining);
            timeLimit = remaining - moveOverhead;
            absoluteLimit = remaining - moveOverhead;
            Infinite = ponder;
        }

        public void Go(int time, int opponentTime, int increment = 0, int movesToGo = -1, int movesOutOfBook = 10, bool ponder = false)
        {
            Reset();

            if (movesToGo <= 0)
            {
                if (increment <= 0)
                {
                    movesToGo = defMovesToGoSuddenDeath;
                }
                else
                {
                    movesToGo = ponder ? defMovesToGoPonder : defMovesToGo;
                }
            }

            remaining = time;
            timeBudget = (time + movesToGo * increment) / movesToGo;

            int timeDiff = (opponentTime - time) * 10 / time;
            int sign = Math.Sign(timeDiff);

            // if opponent is using significantly less/more time than we are then reduce/increase time budget
            int timeImbalance = Math.Abs(timeDiff) switch
            {
                >= 9 => timeBudget * 8 / 10,
                >= 7 and < 9 => timeBudget * 6 / 10,
                >= 5 and < 7 => timeBudget * 4 / 10,
                >= 3 and < 5 => timeBudget * 2 / 10,
                _ => 0
            };

            timeImbalance *= sign;

            // give a bonus to move time for the first few moves following the conclusion of book moves
            int factor = 10;
            if (movesOutOfBook < 10)
            {
                // increase time budget by 0 - 100%
                factor = 20 - Math.Min(movesOutOfBook, 10);
            }

            // final adjusted time budget
            adjustedBudget = timeBudget - timeImbalance;
            adjustedBudget = (adjustedBudget * factor) / 10;

            // set the final move time limits
            timeLimit = Math.Max(adjustedBudget - moveOverhead, moveOverhead); 
            absoluteLimit = Math.Max(Math.Min(adjustedBudget * absoluteLimitFactor, remaining / 2) - moveOverhead, moveOverhead);
            Infinite = ponder;
            Uci.Debug($"Starting TimeLimit: {timeLimit}, AbsoluteLimit: {absoluteLimit}");
        }

        public void AdjustTime(bool oneLegalMove, bool bestMoveChanged, int changes)
        {
            if (timeBudget == 0)
            {
                // don't adjust the budget set for analysis
                return;
            }

            if (oneLegalMove)
            {
                difficulty = 10;
            }
            else if (bestMoveChanged)
            {
                if (difficulty < 100)
                {
                    difficulty = 100 + changes * 20;

                }
                else
                {
                    difficulty = (difficulty * 80) / 100 + changes * 20;
                }

                difficulty = Math.Min(difficulty, difficultyMax);
            }
            else
            {
                difficulty = (difficulty * 9) / 10;
                difficulty = Math.Max(difficulty, difficultyMin);
            }

            int budget = (adjustedBudget * difficulty) / 100;

            // update time limits
            timeLimit = Math.Max(budget - moveOverhead, moveOverhead);
            absoluteLimit = Math.Max(Math.Min(budget * absoluteLimitFactor, remaining / 2) - moveOverhead, moveOverhead);
            Uci.Debug($"Difficulty: {difficulty}, Adjusted TimeLimit: {timeLimit}, AbsoluteLimit: {absoluteLimit}");
        }

        public bool CanSearchDeeper()
        {
            int elapsed = Elapsed;
            if (Infinite)
            {
                return true;
            }

            if (timeBudget == 0 && elapsed < absoluteLimit)
            {
                return true;
            }

            int estimate = (ElapsedInterval * branchFactor) / branch_factor_divisor;

            Uci.Debug($"CanSearchDeeper Elapsed: {elapsed}, Estimate: {estimate}, TimeLimit: {timeLimit}");
            if (elapsed + estimate <= timeLimit)
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckTimeBudget()
        {
            if (!Infinite)
            {
                return Elapsed > absoluteLimit;
            }

            return false;
        }

        private void LoadParameters()
        { 
            moveOverhead = UciOptions.MoveOverhead;
            branchFactor = UciOptions.TmBranchFactor;
            defMovesToGo = UciOptions.TmDefMovesToGo;
            defMovesToGoPonder = UciOptions.TmDefMovesToGoPonder;
            defMovesToGoSuddenDeath = UciOptions.TmDefMovesToGoSuddenDeath;
            absoluteLimitFactor = UciOptions.TmAbsoluteLimit;
            difficultyMin = UciOptions.TmDifficultyMin;
            difficultyMax = UciOptions.TmDifficultyMax;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Milliseconds(long ticks) => (int)((ticks * 1000L) / Stopwatch.Frequency);

        public GameClock Clone()
        {
            return new(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"GameClock.t0 = {t0}");
            sb.AppendLine($"GameClock.tN = {tN}");
            sb.AppendLine($"GameClock.timeBudget = {timeBudget}");
            sb.AppendLine($"GameClock.adjustedBudget = {adjustedBudget}");
            sb.AppendLine($"GameClock.timeLimit = {timeLimit}");
            sb.AppendLine($"GameClock.absoluteLimit = {absoluteLimit}");
            sb.AppendLine($"GameClock.difficulty = {difficulty}");
            sb.AppendLine($"GameClock.remaining = {remaining}");
            sb.AppendLine($"GameClock.elapsed = {Elapsed}");
            return sb.ToString();
        }

        private const int branch_factor_divisor = 16;
        private const int max_time_remaining = int.MaxValue / 3;

        // parameters
        private int moveOverhead = 25;
        private int branchFactor = 30; /* A: 28, B: 30, C: 32 */
        private int defMovesToGo = 30;
        private int defMovesToGoPonder = 35;
        private int defMovesToGoSuddenDeath = 40;
        private int absoluteLimitFactor = 5;
        private int difficultyMin = 60;
        private int difficultyMax = 200;

        private long t0;
        private long tN;
        private int timeBudget;         // time per move budget
        private int adjustedBudget;     // adjusted time budget
        private int timeLimit;          // time limit that governs ID
        private int absoluteLimit;      // time limit that represents the absolute limit on search time
        private int difficulty;         // a quantity that reflects difficulty of position
        private int remaining;
        private Uci uci = Uci.Default;
    }

}
