// <copyright file="SearchThread.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;
    using Pedantic.Chess.NNUE;
    using Pedantic.Utilities;

    public sealed class SearchThread : IDisposable
    {
        public SearchThread(bool isPrimary = false)
        {
            this.isPrimary = isPrimary;
            clock = null;
            eval = new(cache);
            history = new(stack);
            listPool = new(() => new MoveList(history), (o) => {o.History = history; o.Clear();}, MAX_PLY, 32);
            search = new BasicSearch(stack, eval, history, listPool);
        }

        public void Search(GameClock clock, Board board, int maxDepth, long maxNodes, long minNodes, CountdownEvent done)
        {
            Uci uci = new(isPrimary, false);
            clock.Uci = uci;
            this.clock = clock;

            search.Initialize(board, clock, uci, UciOptions.Ponder, maxDepth, maxNodes, minNodes);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                SearchProc(board, done);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            clock?.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SearchProc(Board board, CountdownEvent done)
        {
            stack.Initialize(board, history);
            search.Search();
            done.Signal();

            if (IsPrimary)
            {
                Engine.StopSearch();
            }
        }

        public long TotalNodes => search?.NodesVisited ?? 0;
        public double TotalTime => (search?.Elapsed ?? 0) / 1000.0;
        public bool IsPrimary => isPrimary;
        public SearchStack Stack => stack;
        public EvalCache EvalCache => cache;
        public History History => history;
        public ObjectPool<MoveList> MoveListPool => listPool;

        private readonly bool isPrimary;
        private BasicSearch search;
        private GameClock? clock;
        private bool disposedValue;
        private readonly EvalCache cache = new();
        private readonly NnueEval eval;
        private readonly SearchStack stack = new();
        private readonly History history;
        private readonly ObjectPool<MoveList> listPool;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    eval.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
