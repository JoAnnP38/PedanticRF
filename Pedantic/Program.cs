using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Text;
using Pedantic.Chess;
using Pedantic.Tuning;

using static Pedantic.Chess.HCE.Weights;

namespace Pedantic
{
    public class Program
    {
        const int BUFFER_SIZE = 8192;

        static async Task Main(string[] args)
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
            Console.WriteLine();

            var learnDataOption = new Option<string?>
            (
                name: "--data",
                description: "The file name/path of the labeled training data file.",
                getDefaultValue: () => null
            );
            var learnSampleOption = new Option<int>
            (
                name: "--sample",
                description: "Specify the sample size to extract from training data.",
                getDefaultValue: () => -1
            );
            var learnMaxEpochOption = new Option<int>
            (
                name: "--epochs",
                description: "The maximum number of training iterations before a solution is declared.",
                getDefaultValue: () => 1000
            );
            var learnMaxTimeOption = new Option<TimeSpan?>
            (
                name: "--time",
                description: "The maximum duration of training session before a solution is declared.",
                getDefaultValue: () => new TimeSpan(0, 30, 0)
            );
            var learnSaveOption = new Option<bool>
            (
                name: "--save",
                description: "If specified the extracted sample will be saved in file so it can be reused.",
                getDefaultValue: () => false
            );
            var learnResetOption = new Option<bool>
            (
                name: "--reset",
                description: "Reset all evaluation weights (excluding piece weights) to zero before training begins.",
                getDefaultValue: () => false
            );
            var learnEvalPctOption = new Option<int>
            (
                name: "--eval",
                description: "The proportion of eval score to use in linear interpolation between eval and WDL.",
                getDefaultValue: () => 25
            );

            var uciCmd = new Command("uci", "Start the pedantic application in UCI mode (default).");

            var learnCmd = new Command("learn", "Optimize evaluation function using training data.")
            {
                learnDataOption,
                learnSampleOption,
                learnMaxEpochOption,
                learnMaxTimeOption,
                learnSaveOption,
                learnResetOption,
                learnEvalPctOption
            };
            var rootCmd = new RootCommand("The pedantic chess engine.")
            {
                uciCmd,
                learnCmd
            };

            uciCmd.SetHandler(RunUci);
            learnCmd.SetHandler(RunLearn, learnDataOption, learnSampleOption, learnMaxEpochOption, learnMaxTimeOption,
                learnSaveOption, learnResetOption, learnEvalPctOption);
            rootCmd.SetHandler(RunUci);
            await rootCmd.InvokeAsync(args);
        }

        static void InitializeStaticData()
        {
            Chess.HCE.Weights.Initialize();
            Board.Initialize();
            BasicSearch.Initialize();
        }

        private static async Task RunUci()
        {
            try
            {
                Engine.Start();

                while (Engine.IsRunning)
                {
                    string? input = await Task.Run(Console.ReadLine);
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        ParseUciCommand(input);
                    }
                }
            }
            catch (Exception ex)
            {
                Uci.Default.Log(@$"Fatal error occurred in Pedantic: '{ex.Message}'.");
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
                    Bench(inputSpan[tokenRange[1]]);
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
                    // custom: perft depth <x> [divide]
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
                int fenEnd = fen.IndexOf("moves");
                if (fenEnd == -1)
                {
                    fenEnd = fen.Length;
                }
                else
                {
                    fenEnd--;
                }
                Engine.SetupPosition(fen[..fenEnd]);
            }
            else
            {
                Uci.Default.Log("'position' parameters missing or not understood. Assuming 'startpos'.");
                Engine.SetupPosition(FEN_START_POS);            
            }

            if (splitCount > 1)
            {
                ReadOnlySpan<char> moves = tokens[tokenRanges[1]];
                int firstMove = moves.IndexOf("moves");
                if (firstMove != -1)
                {
                    firstMove += 6;
                    moves = moves[firstMove..];
                    Engine.MakeMoves(moves);
                }
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

        static void Bench(ReadOnlySpan<char> tokens)
        {
            TryParse(tokens, "depth", out int maxDepth, MAX_PLY - 1);
            if (maxDepth > 0)
            {
                Engine.Bench(maxDepth, false);
            }
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

        static void RunLearn(string? dataFile, int samples, int maxEpoch, TimeSpan? maxTime, bool save, bool reset, int evalPct)
        {
            try
            {
                PosRecord.EvalPct = Math.Clamp(evalPct, 0, 100);
                if (dataFile == null)
                {
                    throw new ArgumentNullException(nameof(dataFile));
                }

                using var data = new TrainingDataFile(dataFile, Encoding.UTF8);
                List<PosRecord> positions = samples <= 0 ? data.LoadFile() : data.LoadSample(samples, save);

                var tuner = reset ? new GdTuner(positions) : new GdTuner(Engine.Weights, positions);
                var (Error, Accuracy, Weights, K) = tuner.Train(maxEpoch, maxTime);
                PrintSolution(positions.Count, Error, Accuracy, Weights, K);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static void PrintSolution(int sampleSize, double error, double accuracy, Chess.HCE.Weights weights, double K)
        {
            indentLevel = 2;
            WriteLine($"// Solution sample size: {sampleSize}, generated on {DateTime.Now:R}");
            WriteLine($"// Solution K: {K:F6}, error: {error:F6}, accuracy: {accuracy:F4}");
            WriteLine("private static readonly Score[] defaultWeights =");
            WriteLine("{");
            indentLevel++;
            PrintSolutionSection(weights);
            indentLevel--;            
            WriteLine("};");
        }

        private static void PrintSolutionSection(Chess.HCE.Weights wts)
        {
            void WriteWt(Score s)
            {
                string score = $"S({s.MgScore,3}, {s.EgScore,3}), ";
                Console.Write($"{score,-15}");
            }

            void WriteWtLine(Score s)
            {
                WriteIndent();
                WriteWt(s);
                Console.WriteLine();
            }

            void WriteWtsLine(Chess.HCE.Weights wts, int start, int length)
            {
                WriteIndent();
                for (int n = start; n < start + length; n++)
                {
                    WriteWt(wts[n]);
                }
                Console.WriteLine();
            }

            void WriteWts2D(Chess.HCE.Weights wts, int start, int width, int length)
            {
                for (int n = 0; n < length; n++)
                {
                    if (n % width == 0)
                    {
                        if (n != 0)
                        {
                            Console.WriteLine();
                        }
                        WriteIndent();
                    }
                    WriteWt(wts[start + n]);
                }
                Console.WriteLine();
            }

            string[] pieceNames = { "pawns", "knights", "bishops", "rooks", "queens", "kings" };
            string[] upperNames = { "Pawn", "Knight", "Bishop", "Rook", "Queen", "King" };
            WriteLine("/* piece values */");
            WriteWtsLine(wts, PIECE_VALUES, MAX_PIECES);
            WriteLine();
            WriteLine("/* friendly king piece square values */");
            WriteLine("#region friendly king piece square values");
            WriteLine();
            for (int pc = 0; pc < MAX_PIECES; pc++)
            {
                for (int kp = 0; kp < MAX_KING_BUCKETS; kp++)
                {
                    int index = (pc * MAX_KING_BUCKETS + kp) * MAX_SQUARES;
                    WriteLine($"/* {pieceNames[pc]}: bucket {kp} */");
                    WriteWts2D(wts, FRIENDLY_KB_PST + index, 8, MAX_SQUARES);
                    WriteLine();
                }
            }
            WriteLine("#endregion");
            WriteLine();
            WriteLine("/* enemy king piece square values */");
            WriteLine("#region enemy king piece square values");
            WriteLine();
            for (int pc = 0; pc < MAX_PIECES; pc++)
            {
                for (int kp = 0; kp < MAX_KING_BUCKETS; kp++)
                {
                    int index = (pc * MAX_KING_BUCKETS + kp) * MAX_SQUARES;
                    WriteLine($"/* {pieceNames[pc]}: bucket {kp} */");
                    WriteWts2D(wts, ENEMY_KB_PST + index, 8, MAX_SQUARES);
                    WriteLine();
                }
            }
            WriteLine("#endregion");
        }

        private static void WriteIndent()
        {
            string indent = new(' ', indentLevel * 4);
            Console.Write(indent);
        }
        private static void WriteLine(string text = "")
        {
            WriteIndent();
            Console.WriteLine(text);
        }

        private static int indentLevel = 0;
    }
}
