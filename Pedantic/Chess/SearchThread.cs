// <copyright file="SearchThread.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;
    using Pedantic.Chess.HCE;
    using Pedantic.Utilities;

    public sealed class SearchThread
    {
        public SearchThread(bool isPrimary = false)
        {
            this.isPrimary = isPrimary;
            search = null;
            clock = null;
            eval = new(cache);
            history = new(stack);
            listPool = new(() => new MoveList(history), (o) => {o.History = history; o.Clear();}, MAX_PLY, 32);
        }

        public void Search(GameClock clock, Board board, int maxDepth, long maxNodes, CountdownEvent done)
        {
            Uci uci = new(isPrimary, false);
            clock.Uci = uci;
            this.clock = clock;

            search = new(stack, board, clock, eval, history, listPool, TtCache.Default, maxDepth, maxNodes)
            {
                CanPonder = UciOptions.Ponder,
                Uci = uci
            };

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
            search?.Search();
            done.Signal();
        }

        public long TotalNodes => search?.NodesVisited ?? 0;
        public double TotalTime => (search?.Elapsed ?? 0) / 1000.0;
        public bool IsPrimary => isPrimary;
        public SearchStack Stack => stack;
        public EvalCache EvalCache => cache;
        public History History => history;
        public ObjectPool<MoveList> MoveListPool => listPool;

        private readonly bool isPrimary;
        private BasicSearch? search;
        private GameClock? clock;
        private readonly EvalCache cache = new();
        private readonly HceEval eval;
        private readonly SearchStack stack = new();
        private readonly History history;
        private readonly ObjectPool<MoveList> listPool;
    }
}
