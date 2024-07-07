// <copyright file="SearchThreads.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;
    using Pedantic.Utilities;

    public sealed class SearchThreads
    {
        static SearchThreads()
        {
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out _);
        }

        public SearchThreads()
        {
            done = new CountdownEvent(0);
            threads = [new SearchThread(true)];
        }

        public bool IsRunning
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !done.IsSet;
        }

        public int ThreadCount
        {
            get => threads.Length;
            set
            {
                Util.Assert(value > 0 && value <= maxWorkerThreads);
                Array.Resize(ref threads, value);
                for (int n = 1; n < threads.Length; n++)
                {
                    if (threads[n] == null)
                    {
                        threads[n] = new SearchThread();
                    }
                }
            }
        }

        public void ClearEvalCache()
        {
            foreach (var thread in threads)
            {
                thread.EvalCache.Clear();
                thread.History.Clear();
            }
        }

        public void ResizeEvalCache()
        {
            int sizeMb = UciOptions.HashTableSize;
            foreach (var thread in threads)
            {
                thread.EvalCache.Resize(sizeMb);
            }
        }

        public void Wait()
        {
            // wait (i.e. block) for search to be complete
            if (!done.IsSet)
            {
                done.Wait();
            }
        }

        public void Search(GameClock clock, Board board, int maxDepth, long maxNodes, long minNodes)
        {
            if (done.IsSet)
            {
                done.Reset(threads.Length);
                for (int n = 1; n < threads.Length; n++)
                {
                    threads[n].Search(clock.Clone(), board.Clone(), maxDepth, maxNodes, minNodes, done);
                }
                threads[0].Search(clock, board, maxDepth, maxNodes, minNodes, done);
            }
        }

        public void Stop()
        {
            if (!done.IsSet)
            {
                foreach(var thread in threads)
                {
                    thread.Stop();
                }
            }
        }

        public long TotalNodes
        {
            get
            {
                if (!done.IsSet)
                {
                    return 0;
                }
                return threads[0].TotalNodes;
            }
        }

        public double TotalTime
        {
            get
            {
                if (!done.IsSet)
                {
                    return 0.0;
                }
                return threads[0].TotalTime;
            }
        }

        public static int MaxWorkerThreads
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => maxWorkerThreads;
        }

        private readonly CountdownEvent done;
        private SearchThread[] threads;
        private static readonly int maxWorkerThreads;    
    }
}
