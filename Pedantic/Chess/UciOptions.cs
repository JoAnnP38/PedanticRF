using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Pedantic.Chess
{
    public static class UciOptions
    {
        // Prefix UCI_T_ is for tunable paramters
        internal const string OPT_CLEAR_HASH = "Clear Hash";
        internal const string OPT_CONTEMPT = "Contempt";
        internal const string OPT_EVAL_FILE = "EvalFile";
        internal const string OPT_HASH_TABLE_SIZE = "Hash";
        internal const string OPT_MOVE_OVERHEAD = "Move Overhead";
        internal const string OPT_OWN_BOOK = "OwnBook";
        internal const string OPT_PONDER = "Ponder";
        internal const string OPT_RANDOM_SEARCH = "RandomSearch";
        internal const string OPT_SYZYGY_PATH = "SyzygyPath";
        internal const string OPT_SYZYGY_PROBE_ROOT = "SyzygyProbeRoot";
        internal const string OPT_SYZYGY_PROBE_DEPTH = "SyzygyProbeDepth";
        internal const string OPT_THREADS = "Threads";
        internal const string OPT_ANALYSE_MODE = "UCI_AnalyseMode";
        internal const string OPT_ENGINE_ABOUT = "UCI_EngineAbout";
        internal const string OPT_OPPONENT = "UCI_Opponent";
        internal const string OPT_TM_BRANCH_FACTOR = "UCI_T_TM_BranchFactor";
        internal const string OPT_TM_DEF_MOVES_TO_GO = "UCI_T_TM_DefaultMovesToGo";
        internal const string OPT_TM_DEF_MOVES_TO_GO_PONDER = "UCI_T_TM_DefaultMovesToGo_Ponder";
        internal const string OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH = "UCI_T_TM_DefaultMovesToGo_SuddenDeath";
        internal const string OPT_TM_ABSOLUTE_LIMIT = "UCI_T_TM_AbsoluteLimit";
        internal const string OPT_TM_DIFFICULTY_MIN = "UCI_T_TM_DifficultyMin";
        internal const string OPT_TM_DIFFICULTY_MAX = "UCI_T_TM_DifficultyMax";
        internal const string OPT_ASP_MIN_DEPTH = "UCI_T_ASP_MinDepth";
        internal const string OPT_ONE_MOVE_MAX_DEPTH = "UCI_T_OneMoveMaxDepth";
        internal const string OPT_QS_RECAPTURE_DEPTH = "UCI_T_QS_RecaptureDepth";
        internal const string OPT_QS_PROMOTION_DEPTH = "UCI_T_QS_PromotionDepth";
        internal const string OPT_NMP_MIN_DEPTH = "UCI_T_NMP_MinDepth";
        internal const string OPT_NMP_BASE_DEDUCTION = "UCI_T_NMP_BaseDeduction";
        internal const string OPT_NMP_INC_DIVISOR = "UCI_T_NMP_IncDivisor";
        internal const string OPT_LMR_DEPTH_FACTOR = "UCI_T_LMR_DepthFactor";
        internal const string OPT_LMR_MOVE_FACTOR = "UCI_T_LMR_MoveFactor";
        internal const string OPT_LMR_SCALE_FACTOR = "UCI_T_LMR_ScaleFactor";
        internal const string OPT_RFP_MAX_DEPTH = "UCI_T_RFP_MaxDepth";
        internal const string OPT_RFP_MARGIN = "UCI_T_RFP_Margin";

        static UciOptions()
        {
            options = new(initialOptions.Length, StringComparer.InvariantCultureIgnoreCase);
            foreach (var opt in initialOptions)
            {
                options.Add(opt.Name, opt);
            }
        }

        public static int Contempt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_CONTEMPT];
                return opt.CurrentValue;
            }
        }

        public static string EvalFile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionString opt = (UciOptionString)options[OPT_EVAL_FILE];
                return opt.CurrentValue;
            }
        }

        public static int HashTableSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_HASH_TABLE_SIZE];
                return opt.CurrentValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_HASH_TABLE_SIZE];
                opt.CurrentValue = value;
            }
        }

        public static int MoveOverhead
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_MOVE_OVERHEAD];
                return opt.CurrentValue;
            }
        }

        public static bool OwnBook
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionCheck opt = (UciOptionCheck)options[OPT_OWN_BOOK];
                return opt.CurrentValue;
            }
        }

        public static bool Ponder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionCheck opt = (UciOptionCheck)options[OPT_PONDER];
                return opt.CurrentValue;
            }
        }

        public static bool RandomSearch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionCheck opt = (UciOptionCheck)options[OPT_RANDOM_SEARCH];
                return opt.CurrentValue;
            }
        }

        public static string SyzygyPath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionString opt = (UciOptionString)options[OPT_SYZYGY_PATH];
                return opt.CurrentValue;
            }
        }

        public static bool SyzygyProbeRoot
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionCheck opt = (UciOptionCheck)options[OPT_SYZYGY_PROBE_ROOT];
                return opt.CurrentValue;
            }
        }

        public static int SyzygyProbeDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_SYZYGY_PROBE_DEPTH];
                return opt.CurrentValue;
            }
        }

        public static int Threads
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_THREADS];
                return opt.CurrentValue;
            }
        }

        public static bool AnalyseMode
        {
            get
            {
                UciOptionCheck opt = (UciOptionCheck)options[OPT_ANALYSE_MODE];
                return opt.CurrentValue;
            }
        }

        public static int TmBranchFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_BRANCH_FACTOR];
                return opt.CurrentValue;
            }
        }

        public static int TmDefMovesToGo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DEF_MOVES_TO_GO];
                return opt.CurrentValue;
            }
        }

        public static int TmDefMovesToGoPonder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DEF_MOVES_TO_GO_PONDER];
                return opt.CurrentValue;
            }
        }

        public static int TmDefMovesToGoSuddenDeath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH];
                return opt.CurrentValue;
            }
        }

        public static int TmAbsoluteLimit
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_ABSOLUTE_LIMIT];
                return opt.CurrentValue;
            }
        }

        public static int TmDifficultyMin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DIFFICULTY_MIN];
                return opt.CurrentValue;
            }
        }

        public static int TmDifficultyMax
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DIFFICULTY_MAX];
                return opt.CurrentValue;
            }
        }

        public static int AspMinDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_ASP_MIN_DEPTH];
                return opt.CurrentValue;
            }
        }

        public static int OneMoveMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_ONE_MOVE_MAX_DEPTH];
                return opt.CurrentValue;
            }
        }

        public static int RecaptureDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_QS_RECAPTURE_DEPTH];
                return opt.CurrentValue;
            }
        }

        public static int PromotionDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_QS_PROMOTION_DEPTH];
                return opt.CurrentValue;
            }
        }

        public static int NmpMinDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_NMP_MIN_DEPTH];
                return opt.CurrentValue;
            }
        }

        public static int NmpBaseDeduction
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_NMP_BASE_DEDUCTION];
                return opt.CurrentValue;
            }
        }

        public static int NmpIncDivisor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_NMP_INC_DIVISOR];
                return opt.CurrentValue;
            }
        }

        public static int LmrDepthFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_LMR_DEPTH_FACTOR];
                return opt.CurrentValue;
            }
        }

        public static int LmrMoveFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_LMR_MOVE_FACTOR];
                return opt.CurrentValue;
            }
        }

        public static int LmrScaleFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_LMR_SCALE_FACTOR];
                return opt.CurrentValue;
            }
        }

        public static int RfpMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_RFP_MAX_DEPTH];
                return opt.CurrentValue;
            }
        }

        public static int RfpMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_RFP_MARGIN];
                return opt.CurrentValue;
            }
        }

        public static void WriteLine()
        {
            foreach (var kvp in options)
            {
                UciOptionBase opt = kvp.Value;
                if (!opt.Name.StartsWith("UCI_T_", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine(opt);
                }
            }
        }

        public static bool SetOption(ReadOnlySpan<char> input, out string optionName)
        {
            optionName = string.Empty;
            Span<Range> tokenRanges = stackalloc Range[2];
            int splitCount = input.SplitAny(tokenRanges, CMD_SEP, TOKEN_OPTIONS);
            if (splitCount < 2 || !input[tokenRanges[0]].Equals("name", StringComparison.InvariantCulture))
            {
                return false;
            }

            int index = input[tokenRanges[1]].IndexOfToken("value");
            ReadOnlySpan<char> tail = input[tokenRanges[1]];
            optionName = index == -1 ? tail.ToString() : tail.Slice(0, index - 1).ToString();
            UciOptionBase? option = null;
            if (options.ContainsKey(optionName))
            {
                option = options[optionName];
            }

            if (option == null)
            {
                return false;
            }

            string value = tail.Slice(index + 6).ToString();
            switch (option.Type)
            {
                case UciOptionType.Check:
                    UciOptionCheck check = (UciOptionCheck)option;
                    if (bool.TryParse(value, out bool boolResult))
                    {
                        check.CurrentValue = boolResult;
                    }
                    break;

                case UciOptionType.Combo:
                    UciOptionCombo combo = (UciOptionCombo)option;
                    combo.CurrentValue = value;
                    break;

                case UciOptionType.Spin:
                    UciOptionSpin spin = (UciOptionSpin)option;
                    if (int.TryParse(value, out int intResult))
                    {
                        spin.CurrentValue = intResult;
                    }
                    break;

                case UciOptionType.String:
                    UciOptionString str = (UciOptionString)option;
                    str.CurrentValue = value;
                    break;

                default:
                    break;
            }

            return true;
        }

        public static IList<UciOptionBase> Options => options.Values;

        private static readonly SortedList<string, UciOptionBase> options;

        // UCI Options
        private static readonly UciOptionBase[] initialOptions =
        [
            new UciOptionButton(OPT_CLEAR_HASH),
            new UciOptionSpin(OPT_CONTEMPT, 0, -50, 50),
            new UciOptionString(OPT_EVAL_FILE, "./Pedantic.hce"),
            new UciOptionSpin(OPT_HASH_TABLE_SIZE, 64, 16, 2048),
            new UciOptionSpin(OPT_MOVE_OVERHEAD, 25, 0, 1000),
            new UciOptionCheck(OPT_OWN_BOOK, false),
            new UciOptionCheck(OPT_PONDER, false),
            new UciOptionCheck(OPT_RANDOM_SEARCH, false),
            new UciOptionString(OPT_SYZYGY_PATH, string.Empty),
            new UciOptionCheck(OPT_SYZYGY_PROBE_ROOT, true),
            new UciOptionSpin(OPT_SYZYGY_PROBE_DEPTH, 2, 1, MAX_PLY - 1),
            new UciOptionSpin(OPT_THREADS, 1, 1, Environment.ProcessorCount),
            new UciOptionCheck(OPT_ANALYSE_MODE, false),
            new UciOptionString(OPT_ENGINE_ABOUT, $"{APP_NAME_VER} by {APP_AUTHOR}, see {PROGRAM_URL}"),
            new UciOptionString(OPT_OPPONENT, "none none computer generic engine"),
            new UciOptionSpin(OPT_TM_BRANCH_FACTOR, 30, 20, 40),
            new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO, 30, 20, 40),
            new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_PONDER, 35, 20, 40),
            new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH, 40, 20, 40),
            new UciOptionSpin(OPT_TM_ABSOLUTE_LIMIT, 5, 2, 8),
            new UciOptionSpin(OPT_TM_DIFFICULTY_MIN, 60, 0, 60),
            new UciOptionSpin(OPT_TM_DIFFICULTY_MAX, 200, 100, 300),
            new UciOptionSpin(OPT_ASP_MIN_DEPTH, 6, 1, 10),
            new UciOptionSpin(OPT_ONE_MOVE_MAX_DEPTH, 10, 1, 20),
            new UciOptionSpin(OPT_QS_RECAPTURE_DEPTH, 6, 4, 8),
            new UciOptionSpin(OPT_QS_PROMOTION_DEPTH, 2, 0, 8),
            new UciOptionSpin(OPT_NMP_MIN_DEPTH, 3, 3, 6),
            new UciOptionSpin(OPT_NMP_BASE_DEDUCTION, 3, 1, 8),
            new UciOptionSpin(OPT_NMP_INC_DIVISOR, 4, 2, 8),
            new UciOptionSpin(OPT_LMR_DEPTH_FACTOR, 18, 10, 30),
            new UciOptionSpin(OPT_LMR_MOVE_FACTOR, 10, 5, 20),
            new UciOptionSpin(OPT_LMR_SCALE_FACTOR, 20, 10, 30),
            new UciOptionSpin(OPT_RFP_MAX_DEPTH, 6, 4, 12),
            new UciOptionSpin(OPT_RFP_MARGIN, 85, 25, 250)
        ];
    }
}
