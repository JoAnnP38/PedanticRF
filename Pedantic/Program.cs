// <copyright file="Program.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic
{
    using System.CommandLine;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using Pedantic.Chess;
    using Pedantic.Chess.DataGen;
    using Pedantic.Chess.NNUE;
    using Pedantic.Tablebase;

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

            var uciSpsaOption = new Option<bool>
            (
                name: "--spsa",
                description: "SPSA optimization currently being run.",
                getDefaultValue: () => false
            );

            var doDataGen = new Option<bool>
            (
                name: "--datagen",
                description: "Configure engine for generating data.",
                getDefaultValue: () => false
            );

            var outputPDatOption = new Option<string?>("--output", () => null, "Specifies the name of the Pedantic binary file.");
            outputPDatOption.AddAlias("-o");

            var concurrencyOption = new Option<int>("--concurrency", () => 2, "Number of threads to use for data generation.");
            concurrencyOption.AddAlias("-c");

            var positionCountOption = new Option<int>("--pos_count", () => 1000, "The number of evaluated/filtered positions to generate.");
            positionCountOption.AddAlias("-n");

            var uciCmd = new Command("uci", "Start the pedantic application in UCI mode (default).")
            {
                uciSpsaOption,
                doDataGen
            };

            var wfCmd = new Command("wf", "Create configuration files for weather-factory");

            var dataGenCmd = new Command("generate", "Generate training data.")
            {
                outputPDatOption,
                concurrencyOption,
                positionCountOption
            };

            var rootCmd = new RootCommand("The pedantic chess engine.")
            {
                uciCmd,
                wfCmd,
                dataGenCmd
            };

            uciCmd.SetHandler(RunUci, uciSpsaOption, doDataGen);
            wfCmd.SetHandler(RunWf);
            dataGenCmd.SetHandler(RunGenerate, outputPDatOption, positionCountOption, concurrencyOption);
            rootCmd.SetHandler(() => RunUci(false, false));
            await rootCmd.InvokeAsync(args);
        }

        public static void InitializeStaticData()
        {
            Board.Initialize();
            Network.Initialize();
            Engine.Initialize();
            BasicSearch.Initialize();
            Engine.SetupPosition(FEN_START_POS);
        }

        private static async Task RunUci(bool spsa, bool datagen)
        {
            try
            {
                UciOptions.Optimizing = spsa;
                UciOptions.IsDataGen = datagen;
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

                case "eval":
                    Engine.Eval();
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
            TryParse(tokens, "minnodes", out long minNodes, long.MaxValue);
            TryParse(tokens, "movestogo", out int movesToGo, -1);  
            bool ponder = tokens.IndexOfToken("ponder") >= 0;

            int blackTime;
            if (Engine.Color == Color.White && TryParse(tokens, "wtime", out int whiteTime))
            {
                TryParse(tokens, "winc", out int whiteIncrement);
                TryParse(tokens, "btime", out blackTime, whiteTime);
                Engine.Go(whiteTime, blackTime, whiteIncrement, movesToGo, maxDepth, maxNodes, minNodes, ponder);
            }
            else if (Engine.Color == Color.Black && TryParse(tokens, "btime", out blackTime))
            {
                TryParse(tokens, "binc", out int blackIncrement);
                TryParse(tokens, "wtime", out whiteTime, blackTime);
                Engine.Go(blackTime, whiteTime, blackIncrement, movesToGo, maxDepth, maxNodes, minNodes, ponder);
            }
            else
            {
                Engine.Go(maxDepth, maxTime, maxNodes, minNodes, ponder);
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

                    case "syzygypath":
                        string path = UciOptions.SyzygyPath;
                        if (path != string.Empty && string.Compare(path, "<empty>", true) != 0)
                        {
                            if (!Path.Exists(path))
                            {
                                Uci.Default.Log($"Ignoring specified SyzygyPath: '{path}'. Path doesn't exist.");
                            }
                            else
                            {
                                bool result = Syzygy.Initialize(path);
                                if (!result)
                                {
                                    Uci.Default.Log($"Could not locate valid Syzygy tablebase files at '{path}'.");
                                }
                            }
                        }
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
                if (firstMove != -1 && moves.Length > 6)
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

        private static void RunWf()
        {
            UciOptions.WriteOptions();
        }

        private static void RunGenerate(string? outputFile, int positionCount, int concurrency)
        {
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                Console.Error.WriteLine("Output file must be specified");
            }

            using DataGenerator dataGen = new(outputFile!, concurrency);

            ConsoleCancelEventHandler ctrlCHandler = (sender, e) =>
            {
                Console.WriteLine("\n*** Shutting down data generation.");
                dataGen.Stop();
            };

            Console.CancelKeyPress += ctrlCHandler;

            UciOptions.HashTableSize = 16;
            Engine.ResizeHashTable();
            UciOptions.IsDataGen = true;
            UciOptions.RandomSearch = true;

            try
            {
                dataGen.Start();
                DateTime startTime = DateTime.UtcNow;

                int positionsWritten = 0;
                double pps = 0;

                while (positionsWritten < positionCount)
                {
                    Thread.Sleep(2000);
                    positionsWritten = dataGen.PositionCount;
                    pps = positionsWritten / (DateTime.UtcNow - startTime).TotalSeconds;
                    Console.Write($"Generating {positionsWritten:N0} positions at {pps:N1} pps...\r");
                }

                dataGen.Stop();
                positionsWritten = dataGen.PositionCount;
                pps = positionsWritten / (DateTime.UtcNow - startTime).TotalSeconds;
                Console.WriteLine($"Generating {positionsWritten:N0} positions at {pps:N1} pps...");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                Console.CancelKeyPress -= ctrlCHandler;
            }
        }
    }
}
