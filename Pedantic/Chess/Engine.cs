using System.Runtime.CompilerServices;
using System.Text;
using Pedantic.Collections;
using Pedantic.Utilities;

using static Pedantic.Chess.Constants;

namespace Pedantic.Chess
{
    public static class Engine
    {
        public static bool IsRunning { get; private set; } = false;
        public static bool IsDebug { get; set; } = false;
        public static Board Board { get; } = new();

        public static void Start()
        {
            IsRunning = true;
        }

        public static void Stop()
        {
        }

        public static void Quit()
        {
            Stop();
            IsRunning = false;
        }

        public static bool SetupPosition(string fen)
        {
            try
            {
                Stop();
                bool loaded = Board.LoadFen(fen);
                if (loaded)
                {
                    Uci.Default.Debug($"New position: {Board.ToFenString()}");
                }
                else
                {
                    Uci.Default.Log($"Engine failed to load position: '{fen}'");
                }
                return loaded;
            }
            catch (Exception ex)
            {
                Uci.Default.Log($"Engine faulted: {ex.Message}");
                Uci.Default.Log(ex.ToString());
                return false;
            }
        }

        public static void MakeMoves(IEnumerable<string> moves)
        {
            foreach (string mv in moves)
            {
                if (Move.TryParse(Board, mv, out Move move))
                {
                    if (!Board.MakeMove(move))
                    {
                        throw new InvalidOperationException($"Invalid move passed to engine: '{mv}'.");
                    }
                }
                else
                {
                    throw new ArgumentException($"Long algebraic move expected. Bad format '{mv}'.");
                }
            }

            Uci.Default.Debug($"New position: {Board.ToFenString()}");
        }

        public static void Perft(int depth, bool divide, bool details)
        {
            Perft perft = new(Board, depth, details, divide);
            perft.Execute();
        }
    }
}
