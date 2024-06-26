﻿// <copyright file="Uci.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;
    using System.Text;
    using Pedantic.Utilities;

    public class Uci
    {
        static Uci()
        {
            defaultUci = new Uci(true, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Uci(bool enable = true, bool debug = false)
        {
            this.enable = enable;
            this.debug = debug;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(string message)
        {
            if (!enable)
            {
                return;
            }
            Console.Out.WriteLineAsync($"info string {message}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug(string message)
        {
            if (!enable)
            {
                return;
            }
            if (debug)
            {
                Console.Out.WriteLineAsync($"info string {message}");
            }
        }

        public void Info(int depth, int seldepth, int score, long nodes, long timeMs, IList<Move> pv,
            int hashfull = 0, long tbHits = 0L, Bound bound = Bound.Exact)
        {
            if (!enable)
            {
                return;
            }

            ValueStringBuilder sb = new(stackalloc char[256]);
            int nps = (int)(nodes * 1000 / Math.Max(1, timeMs));
            sb.Append($"info depth {depth} seldepth {seldepth} score cp {score}");
            if (bound == Bound.Upper)
            {
                sb.Append(" upperbound");
            }
            else if (bound == Bound.Lower) 
            {
                sb.Append(" lowerbound");
            }

            sb.Append($" nodes {nodes} nps {nps} time {timeMs}");

            if (hashfull > 0)
            {
                sb.Append($" hashfull {hashfull}");
            }

            if (tbHits > 0)
            {
                sb.Append($" tbhits {tbHits}");
            }

            if (bound == Bound.Exact && pv.Count > 0)
            {
                sb.Append(" pv");
                for (int n = 0; n < pv.Count; n++)
                {
                    sb.Append($" {pv[n]}");
                }
            }

            string output = sb.ToString();
            Console.Out.WriteLineAsync(output);
            Util.WriteLine(output);
        }

        public void InfoMate(int depth, int seldepth, int mateIn, long nodes, long timeMs,
            IList<Move> pv, int hashfull = 0, long tbHits = 0L)
        {
            if (!enable)
            {
                return;
            }
            ValueStringBuilder sb = new(stackalloc char[256]);
            int nps = (int)(nodes * 1000 / Math.Max(1, timeMs));
            sb.Append($"info depth {depth} seldepth {seldepth} score mate {mateIn} nodes {nodes} nps {nps} time {timeMs}");

            if (hashfull > 0)
            {
                sb.Append($" hashfull {hashfull}");
            }

            if (tbHits > 0)
            {
                sb.Append($" tbhits {tbHits}");
            }

            sb.Append(" pv");
            for (int n = 0; n < pv.Count; n++)
            {
                sb.Append($" {pv[n]}");
            }

            string output = sb.ToString();
            Console.Out.WriteLineAsync(output);
            Util.WriteLine(output);
        }

        public void CurrentMove(int depth, Move move, int moveNumber, long nodes)
        {
            if (!enable)
            {
                return;
            }
            string output = $"info depth {depth} currmove {move} currmovenumber {moveNumber} nodes {nodes}";
            Console.Out.WriteLineAsync(output);
            Util.WriteLine(output);
        }

        public void BestMove(Move bestmove, Move suggestedPonder)
        {
            if (!enable)
            {
                return;
            }

            string? ponder = suggestedPonder != Move.NullMove ? suggestedPonder.ToString() : null;
            BestMove(bestmove.ToString(), ponder);
        }

        public void BestMove(string bestmove, string? suggestedPonder = null)
        {
            if (!enable)
            {
                return;
            }
            StringBuilder sb = new();
            sb.Append($"bestmove {bestmove}");
            if (suggestedPonder != null)
            {
                sb.Append($" ponder {suggestedPonder}");
            }
            string output = sb.ToString();
            Console.Out.WriteLine(output);
            Util.WriteLine(output);
        }

        public void Usage(int cpuload)
        {
            if (!enable)
            {
                return;
            }
            Console.Out.WriteLineAsync($"info cpuload {cpuload}");
        }

        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => enable;
        }

        public static Uci Default => defaultUci;

        private readonly bool enable;
        private readonly bool debug;
        private readonly static Uci defaultUci;
    }
}
