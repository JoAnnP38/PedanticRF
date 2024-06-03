// <copyright file="Program.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic
{
    using System.CommandLine;
    using System.Globalization;
    using System.Text;
    using Pedantic.Chess;
    using Pedantic.Chess.HCE;
    using Pedantic.Tablebase;
    using Pedantic.Tuning;
    using static Pedantic.Chess.HCE.Weights;

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
                getDefaultValue: () => true
            );
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

            var uciCmd = new Command("uci", "Start the pedantic application in UCI mode (default).")
            {
                uciSpsaOption
            };

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

            var wfCmd = new Command("wf", "Create configuration files for weather-factory");

            var rootCmd = new RootCommand("The pedantic chess engine.")
            {
                uciCmd,
                learnCmd,
                wfCmd
            };

            uciCmd.SetHandler(RunUci, uciSpsaOption);
            learnCmd.SetHandler(RunLearn, learnDataOption, learnSampleOption, learnMaxEpochOption, learnMaxTimeOption,
                learnSaveOption, learnResetOption, learnEvalPctOption);
            wfCmd.SetHandler(RunWf);
            rootCmd.SetHandler(() => RunUci(false));
            await rootCmd.InvokeAsync(args);
        }

        public static void InitializeStaticData()
        {
            Board.Initialize();
            Weights.Initialize();
            HceEval.Initialize();
            Engine.Initialize();
            BasicSearch.Initialize();
            Engine.SetupPosition(FEN_START_POS);
        }

        private static async Task RunUci(bool spsa)
        {
            try
            {
                UciOptions.Optimizing = spsa;
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
            WriteLine("[");
            indentLevel++;
            PrintSolutionSection(weights);
            indentLevel--;            
            WriteLine("];");
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
            WriteLine("/* friendly king relative piece square values */");
            WriteLine("#region friendly king relative piece square values");
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
            WriteLine("/* enemy king relative piece square values */");
            WriteLine("#region enemy king relative piece square values");
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
            WriteLine();
            WriteLine("/* piece mobility */");
            WriteLine("#region piece mobility");
            WriteLine();
            WriteLine("/* knight mobility */");
            WriteWts2D(wts, KNIGHT_MOBILITY, 8, 9);
            WriteLine();
            WriteLine("/* bishop mobility */");
            WriteWts2D(wts, BISHOP_MOBILITY, 8, 14);
            WriteLine();
            WriteLine("/* rook mobility */");
            WriteWts2D(wts, ROOK_MOBILITY, 8, 15);
            WriteLine();
            WriteLine("/* queen mobility */");
            WriteWts2D(wts, QUEEN_MOBILITY, 8, 28);
            WriteLine();
            WriteLine("#endregion");
            WriteLine();
            WriteLine("/* pawn structure */");
            WriteLine("#region pawn structure");
            WriteLine();
            WriteLine("/* passed pawn */");
            WriteWts2D(wts, PASSED_PAWN, 8, MAX_SQUARES);
            WriteLine();
            WriteLine("/* adjacent/phalanx pawn */");
            WriteWts2D(wts, PHALANX_PAWN, 8, MAX_SQUARES);
            WriteLine();
            WriteLine("/* chained pawn */");
            WriteWts2D(wts, CHAINED_PAWN, 8, MAX_SQUARES);
            WriteLine();
            WriteLine("/* pawn ram */");
            WriteWts2D(wts, PAWN_RAM, 8, MAX_SQUARES);
            WriteLine();
            WriteLine("/* isolated pawn */");
            WriteWts2D(wts, ISOLATED_PAWN, 8, MAX_SQUARES);
            WriteLine();
            WriteLine("/* backward pawn */");
            WriteWtLine(wts.BackwardPawn);
            WriteLine();
            WriteLine("#endregion");
            WriteLine();
            WriteLine("/* king safety */");
            WriteLine("#region king safety");
            WriteLine();
            WriteLine("/* squares attacked near enemy king */");
            WriteIndent(); WriteWt(wts.KingAttack1(0));
            Console.WriteLine("\t// attacks to squares 1 from king");
            WriteWts2D(wts, KING_ATTACK_1 + 1, 8, 8);
            WriteLine();
            WriteIndent(); WriteWt(wts.KingAttack2(0));
            Console.WriteLine("\t// attacks to squares 2 from king");
            WriteWts2D(wts, KING_ATTACK_2 + 1, 8, 8);
            WriteLine();
            WriteLine("/* castling available */");
            WriteIndent(); WriteWt(wts.CanCastleKS);
            Console.WriteLine("\t// king-side castling available");
            WriteIndent(); WriteWt(wts.CanCastleQS);
            Console.WriteLine("\t// queen-side castling available");
            WriteLine();
            WriteLine("/* king mobility penalties (open line attacks) */");
            WriteLine("/* diagonal lines */");
            WriteWts2D(wts, KS_DIAG_MOBILITY, 8, 13);
            WriteLine();
            WriteLine("/* orthogonal lines */");
            WriteWts2D(wts, KS_ORTH_MOBILITY, 8, 14);
            WriteLine();
            WriteLine("/* pawnless flank */");
            WriteWtLine(wts.PawnlessFlank);
            WriteLine();
            WriteLine("#endregion");
            WriteLine();
            WriteLine("/* passed pawns */");
            WriteLine("#region passed pawns");
            WriteLine();
            WriteLine("/* enemy king outside passed pawn square */");
            WriteWtLine(wts.KingOutsidePasserSquare);
            WriteLine();
            WriteLine("/* passed pawn can advance */");
            WriteWts2D(wts, PP_CAN_ADVANCE, 8, 4);
            WriteLine();
            WriteLine("/* blocked passed pawn */");
            for (Piece pc = Piece.Knight; pc <= Piece.King; pc++)
            {
                int index = BLOCKED_PASSED_PAWN + ((int)pc - 1) * MAX_COORDS;
                WriteIndent();
                for (int rank = 0; rank < MAX_COORDS; rank++)
                {
                    WriteWt(wts[index + rank]);
                }
                Console.WriteLine($"\t// blocked by {pc}");
            }
            WriteLine();
            WriteLine("/* rook behind passed pawn */");
            WriteWtLine(wts.RookBehindPassedPawn);
            WriteLine();
            WriteLine("#endregion");
            WriteLine();
            WriteLine("/* piece evaluations */");
            WriteLine("#region piece evaluations");
            WriteLine();
            WriteLine("/* bishop pair */");
            WriteWtLine(wts.BishopPair);
            WriteLine();
            WriteLine("/* bad bishop pawn */");
            WriteWtLine(wts.BadBishopPawn);
            WriteLine();
            WriteLine("/* rook on open file */");
            WriteWtLine(wts.RookOnOpenFile);
            WriteLine();
            WriteLine("/* rook on half-open file */");
            WriteWtLine(wts.RookOnHalfOpenFile);
            WriteLine();
            WriteLine("/* pawn shields minor piece */");
            WriteWtLine(wts.PawnShieldsMinor);
            WriteLine();
            WriteLine("/* bishop on long diagonal */");
            WriteWtLine(wts.BishopLongDiagonal);
            WriteLine();
            WriteLine("#endregion");
            WriteLine();
            WriteLine("/* threats */");
            WriteLine("#region threats");
            WriteLine();
            WriteLine("/* pushed pawn threats */");
            WriteWts2D(wts, PAWN_PUSH_THREAT, 8, MAX_PIECES);
            WriteLine();
            WriteLine("/* pawn threats */");
            WriteWts2D(wts, PAWN_THREAT, 8, MAX_PIECES);
            WriteLine();
            WriteLine("/* minor piece threats */");
            WriteWts2D(wts, MINOR_THREAT, 8, MAX_PIECES);
            WriteLine();
            WriteLine("/* rook threats */");
            WriteWts2D(wts, ROOK_THREAT, 8, MAX_PIECES);
            WriteLine();
            WriteLine("/* check threats */");
            WriteWts2D(wts, CHECK_THREAT, 8, MAX_PIECES);
            WriteLine();
            WriteLine("#endregion");
            WriteLine();
            WriteLine("/* tempo bonus for side to move */");
            WriteWtLine(wts[TEMPO]);
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

        private static void RunWf()
        {
            UciOptions.WriteOptions();
        }

        private static int indentLevel = 0;
    }
}
