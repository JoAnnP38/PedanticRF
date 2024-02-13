using System.Globalization;
using System.Text;
using Pedantic.Chess;

namespace Pedantic
{
    public class Program
    {
        const int BUFFER_SIZE = 8192;

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);            
            Stream iStream = Console.OpenStandardInput();
            Console.SetIn(new StreamReader(iStream, encoding, false, BUFFER_SIZE));
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
            Board.Initialize();
        }

        private static async Task RunUci()
        {
            try
            {
                Engine.Start();

                while (Engine.IsRunning)
                {
                    string? input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        ParseUciCommand(input);
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
            ReadOnlySpan<char> cmdSepSpan = CMD_SEP;
            ReadOnlySpan<char> inputSpan = input.AsSpan().Trim();
            Span<Range> tokenRange = stackalloc Range[2];
            tokenRange.Clear();
            int splitCount = inputSpan.SplitAny(tokenRange, CMD_SEP, TOKEN_OPTIONS);

            switch (inputSpan[tokenRange[0]])
            {
                case "bench":
                    // custom: bench depth <x>
                    break;

                case "debug":
                    Debug(inputSpan[tokenRange[1]]);
                    break;

                case "go":
                    Go(inputSpan[tokenRange[1]]);
                    break;

                case "isready":
                    Console.WriteLine("readyok");
                    break;

                case "perft":
                    // custom: perft depth <x> [divide | details]
                    Perft(inputSpan[tokenRange[1]]);
                    break;

                case "ponderhit":
                    Engine.PonderHit();
                    break;

                case "position":
                    SetupPosition(inputSpan[tokenRange[1]]);
                    break;

                case "quit":
                    Engine.Quit();
                    break;

                case "setoption":
                    SetOption(inputSpan[tokenRange[1]]);
                    break;

                case "stop":
                    Engine.Stop();
                    break;

                case "uci":
                    Console.WriteLine($"id name {APP_NAME_VER}");
                    Console.WriteLine($"id author {APP_AUTHOR}");
                    UciOptions.WriteLine();
                    Console.WriteLine("uciok");
                    break;

                case "ucinewgame":
                    Engine.SetupNewGame();
                    break;

                case "wait":
                    Engine.Wait();
                    break;

                default:
                    return;
            }
        }

        static void Go(ReadOnlySpan<char> tokens)
        {
            TryParse(tokens, "depth", out int maxDepth, MAX_PLY - 1);
            TryParse(tokens, "movetime", out int maxTime, int.MaxValue);
            TryParse(tokens, "nodes", out long maxNodes, long.MaxValue);
            TryParse(tokens, "movestogo", out int movesToGo, -1);  
            bool ponder = tokens.IndexOfToken("ponder") >= 0;

            int blackTime;
            if (Engine.Color == Color.White && TryParse(tokens, "wtime", out int whiteTime))
            {
                TryParse(tokens, "winc", out int whiteIncrement);
                TryParse(tokens, "btime", out blackTime, whiteTime);
                Engine.Go(whiteTime, blackTime, whiteIncrement, movesToGo, maxDepth, maxNodes, ponder);
            }
            else if (Engine.Color == Color.Black && TryParse(tokens, "btime", out blackTime))
            {
                TryParse(tokens, "binc", out int blackIncrement);
                TryParse(tokens, "wtime", out whiteTime, blackTime);
                Engine.Go(blackTime, whiteTime, blackIncrement, movesToGo, maxDepth, maxNodes, ponder);
            }
            else
            {
                Engine.Go(maxDepth, maxTime, maxNodes, ponder);
            }
        }

        static void SetOption(ReadOnlySpan<char> tokens)
        {
            if (UciOptions.SetOption(tokens, out string optionName))
            {
                switch (optionName.ToLower())
                {

                    case "clear hash":
                        Engine.ClearHashTable();
                        break;

                    case "hash":
                        Engine.ResizeHashTable();
                        break;

                    case "threads":
                        Engine.SearchThreads = UciOptions.Threads;
                        break;

                    default:
                        break;
                }
            }
        }

        static void SetupPosition(ReadOnlySpan<char> tokens)
        {
            Span<Range> tokenRanges = stackalloc Range[2];
            int splitCount = tokens.SplitAny(tokenRanges, CMD_SEP, TOKEN_OPTIONS);
            if (splitCount <= 0)
            {
                Uci.Default.Log($"position is currently set to fen {Engine.Board.ToFenString()}");
            }
            if (tokens[tokenRanges[0]].Equals("startpos".AsSpan(), StringComparison.InvariantCulture))
            {
                Engine.SetupPosition(FEN_START_POS);
            }
            else if (tokens[tokenRanges[0]].Equals("fen".AsSpan(), StringComparison.InvariantCulture) &&
                splitCount > 1)
            {
                ReadOnlySpan<char> fen = tokens[tokenRanges[1]];
                int fenEnd = fen.IndexOfNthAny(6, CMD_SEP);
                if (fenEnd >= 6)
                {
                    Engine.SetupPosition(fen[..fenEnd]);
                }
            }
            else
            {
                Uci.Default.Log("'position' parameters missing or not understood. Assuming 'startpos'.");
                Engine.SetupPosition(FEN_START_POS);            
            }

            if (splitCount > 1)
            {
                ReadOnlySpan<char> moves = tokens[tokenRanges[1]];
                int firstMove = moves.IndexOf("moves") + 6;
                moves = moves[firstMove..];
                Engine.MakeMoves(moves);
            }
        }

        static bool TryParse(ReadOnlySpan<char> tokens, string tokenName, out int value, int defaultValue = 0)
        {
            Span<Range> tokenRanges = stackalloc Range[2];
            value = defaultValue;
            int index = tokens.IndexOfToken(tokenName);

            if (index >= 0)
            {
                ReadOnlySpan<char> parsedToken = tokens.Slice(index + tokenName.Length + 1);
                int splitCount = parsedToken.SplitAny(tokenRanges, CMD_SEP, TOKEN_OPTIONS);
                if (splitCount >= 1)
                {
                    if (!int.TryParse(parsedToken[tokenRanges[0]], out value))
                    {
                        value = defaultValue;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool TryParse(ReadOnlySpan<char> tokens, string tokenName, out long value, long defaultValue = 0L)
        {
            Span<Range> tokenRanges = stackalloc Range[2];
            value = defaultValue;
            int index = tokens.IndexOfToken(tokenName);

            if (index >= 0)
            {
                ReadOnlySpan<char> parsedToken = tokens.Slice(index + tokenName.Length + 1);
                int splitCount = parsedToken.SplitAny(tokenRanges, CMD_SEP, TOKEN_OPTIONS);
                if (splitCount >= 1)
                {
                    if (!long.TryParse(parsedToken[tokenRanges[0]], out value))
                    {
                        value = defaultValue;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static void Perft(ReadOnlySpan<char> tokens)
        {
            Span<Range> splits = stackalloc Range[4];
            int depth = 1;
            bool divide = false;
            bool details = false;

            int splitCount = tokens.SplitAny(splits, CMD_SEP);

            for (int n = 0; n < splitCount; n++)
            {
                if (tokens[splits[n]].Equals("depth".AsSpan(), StringComparison.InvariantCulture))
                {
                    if (n + 1 < splitCount && !int.TryParse(tokens[splits[n + 1]], out depth))
                    {
                        Uci.Default.Log($"'depth' parameter missing or not understood ('{tokens[n + 1]}'). Defaulting to depth 1.");
                        depth = 1;
                    }
                    else
                    {
                        n++;
                    }
                }
                else if (tokens[splits[n]].Equals("divide".AsSpan(), StringComparison.InvariantCulture))
                {
                    divide = true;
                }
                else if (tokens[splits[n]].Equals("details".AsSpan(), StringComparison.InvariantCulture))
                {
                    details = true;
                }
                else
                {
                    Uci.Default.Log($"Unexpected perft parameter '{tokens[splits[n]]}'.");
                }
            }

            Engine.Perft(depth, divide, details);
        }

        static void Debug(ReadOnlySpan<char> tokens)
        {
            Span<Range> tokenRanges = stackalloc Range[2];
            int splitCount = tokens.SplitAny(tokenRanges, CMD_SEP, TOKEN_OPTIONS);
            if (splitCount <= 0)
            {
                Uci.Default.Log($"debug is currently set to {Engine.Debug.ToString().ToLower()}");
            }
            else
            {
                Engine.Debug = tokens[tokenRanges[0]].Equals("on".AsSpan(), StringComparison.InvariantCulture);
            }
        }
    }
}
