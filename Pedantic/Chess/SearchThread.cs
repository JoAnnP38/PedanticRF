using System.Runtime.CompilerServices;
using Pedantic.Chess.HCE;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public sealed class SearchThread
    {
        public SearchThread(bool isPrimary = false)
        {
            this.isPrimary = isPrimary;
            search = null;
            clock = null;
            listPool = new(() => new MoveList(), (o) => o.Clear(), MAX_PLY, 32);
        }

        public void Search(GameClock clock, Board board, int maxDepth, long maxNodes, CountdownEvent done)
        {
            Uci uci = new(isPrimary, false);
            clock.Uci = uci;
            this.clock = clock;
            eval.Update(board);

            search = new(stack, board, clock, eval, listPool, TtCache.Default, maxDepth, maxNodes)
            {
                CanPonder = Engine.IsPondering,
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
            stack.Initialize(board);
            search?.Search();
            done.Signal();
        }

        public long TotalNodes => search?.NodesVisited ?? 0;
        public double TotalTime => (search?.Elapsed ?? 0) / 1000.0;
        public bool IsPrimary => isPrimary;
        public SearchStack Stack => stack;
        public ObjectPool<MoveList> MoveListPool => listPool;

        private readonly bool isPrimary;
        private BasicSearch? search;
        private GameClock? clock;
        private readonly HceEval eval = new();
        private readonly SearchStack stack = new();
        private readonly ObjectPool<MoveList> listPool;
    }
}
