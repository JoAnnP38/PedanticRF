using System.Diagnostics;
using Pedantic.Utilities;

using static Pedantic.Chess.Constants;

namespace Pedantic.Chess
{
    public sealed class Perft
    {
        public Perft(Board board, int maxDepth = 1, bool details = false, bool divide = false)
        {
            this.board = board;
            this.maxDepth = maxDepth;
            this.details = details;
            this.divide = divide;
            listPool = new(() => new MoveList(), MAX_PLY, 10); 
            Uci = Uci.Default;
        }

        public Uci Uci { get; set; }

        public void Execute()
        {
            bool inCheck = board.IsChecked();
            clock.Restart();
            ulong nodes;
            if (divide)
            {
                nodes = Divide(maxDepth, inCheck);
            }
            else
            {
                nodes = PerftImpl(maxDepth, inCheck);
            }
            clock.Stop();

            double Mnps = nodes / (clock.Elapsed.TotalSeconds * 1000000.0d);
            string results = $"Depth = {maxDepth}, Nodes = {nodes}, Elapsed = {clock.Elapsed}, Mnps: {Mnps,6:N2}";
            Uci.Log(results);
        }

        private ulong Divide(int depth, bool inCheck)
        {
            ulong nodes = 0;
            MoveList list = listPool.Rent();
            board.PushBoardState();

            if (inCheck)
            {
                board.GenerateEvasions(list);
            }
            else
            {
                board.GenerateMoves(list);
            }

            for (int n = 0; n < list.Count; n++)
            {
                Move move = list[n];
                if (!board.MakeMove(move))
                {
                    continue;
                }

                ulong moveNodes = PerftImpl(depth - 1, board.IsChecked());
                nodes += moveNodes;
                Uci.Log($"{move} : {moveNodes} : {board.ToFenString()}");

                board.UnmakeMove();
            }

            board.PopBoardState();
            listPool.Return(list);
            return nodes;
        }

        private ulong PerftImpl(int depth, bool inCheck)
        {
            if (depth == 0)
            {
                return 1;
            }

            ulong nodes = 0;
            MoveList list = listPool.Rent();
            board.PushBoardState();

            if (inCheck)
            {
                board.GenerateEvasions(list);
            }
            else
            {
                board.GenerateMoves(list);
            }

            for (int n = 0; n < list.Count; n++)
            {
                if (!board.MakeMove(list[n]))
                {
                    continue;
                }

                nodes += depth == 1 ? 1 : PerftImpl(depth - 1, board.IsChecked());
                board.UnmakeMove();
            }

            board.PopBoardState();
            listPool.Return(list);
            return nodes;
        }


        private readonly Board board;
        private readonly ObjectPool<MoveList> listPool;
        private readonly int maxDepth;
        private readonly bool details;
        private readonly bool divide;
        private readonly Stopwatch clock = new();
    }
}
