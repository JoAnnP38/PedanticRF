using System.Reflection;
using Pedantic.Chess;
using static Pedantic.Chess.Constants;

namespace Pedantic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{APP_NAME_VER} by {APP_AUTHOR}");
            Console.Write("Fast PEXT available: ");
            InitializeStaticData();
            if (Board.IsPextSupported)
            {
                Console.WriteLine("Yes");
            }
            else
            {
                Console.WriteLine("No");
            } 
            Task.WaitAll(RunUci());
        }

        static void InitializeStaticData()
        {
            var classesWithStaticData = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IInitialize)));

            foreach(var initClass in classesWithStaticData)
            {
                initClass.GetMethod("Initialize")?.Invoke(null, null);
            }
        }

        private static async Task RunUci()
        {
            try
            {
                Engine.Start();

                while (Engine.IsRunning)
                {
                    string? input = await Console.In.ReadLineAsync();

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        ParseUciCommand(input!);
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteAsync(Environment.NewLine);
                await Console.Error.WriteLineAsync($@"[{DateTime.Now}]");
                await Console.Error.WriteLineAsync(ex.ToString());            
            }
        }

        public static void ParseUciCommand(string input)
        {
            input = input.Trim();
            StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
            string[] tokens = input.Split(commandSeparator, options);

            switch (tokens[0])
            {
                case "bench":
                    // custom: bench depth <x>
                    break;

                case "debug":
                    break;

                case "isready":
                    Console.WriteLine("readyok");
                    break;

                case "perft":
                    // custom: perft depth <x> [divide | details]
                    Perft(tokens);
                    break;

                case "position":
                    SetupPosition(tokens);
                    break;

                case "quit":
                    Engine.Quit();
                    break;

                case "stop":
                    Engine.Stop();
                    break;

                case "uci":
                    Console.WriteLine($"id name {APP_NAME_VER}");
                    Console.WriteLine($"id author {APP_AUTHOR}");
                    Console.WriteLine("uciok");
                    break;

                default:
                    Uci.Default.Log($"Unexpected input: '{input}'");
                    return;
            }
        }

        static void SetupPosition(string[] tokens)
        {
            if (tokens[1] == "startpos")
            {
                Engine.SetupPosition(FEN_START_POS);
            }
            else if (tokens[1] == "fen")
            {
                string fen = string.Join(' ', tokens[2..8]);
                Engine.SetupPosition(fen);
            }
            else
            {
                Uci.Default.Log("'position' parameters missing or not understood. Assuming 'startpos'.");
                Engine.SetupPosition(FEN_START_POS);            
            }

            int firstMove = Array.IndexOf(tokens, "moves") + 1;
            if (firstMove == 0)
            {
                return;
            }

            Engine.MakeMoves(tokens[firstMove..]);
        }

        static void Perft(string[] tokens)
        {
            int depth = 1;
            bool divide = false;
            bool details = false;

            for (int n = 1; n < tokens.Length; n++)
            {
                if (tokens[n] == "depth")
                {
                    if (n + 1 < tokens.Length && !int.TryParse(tokens[n + 1], out depth))
                    {
                        Uci.Default.Log($"'depth' parameter missing or not understood ('{tokens[n + 1]}'). Defaulting to depth 1.");
                        depth = 1;
                    }
                    else
                    {
                        n++;
                    }
                }
                else if (tokens[n] == "divide")
                {
                    divide = true;
                }
                else if (tokens[n] == "details")
                {
                    details = true;
                }
                else
                {
                    Uci.Default.Log($"Unexpected perft parameter '{tokens[n]}'.");
                }
            }

            Engine.Perft(depth, divide, details);
        }

        static readonly char[] commandSeparator = [' ', '\t'];
    }
}
