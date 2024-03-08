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
        internal const string OPT_NMP_BASE_REDUCTION = "UCI_T_NMP_BaseReduction";
        internal const string OPT_NMP_INC_DIVISOR = "UCI_T_NMP_IncDivisor";
        internal const string OPT_LMR_DEPTH_FACTOR = "UCI_T_LMR_DepthFactor";
        internal const string OPT_LMR_MOVE_FACTOR = "UCI_T_LMR_MoveFactor";
        internal const string OPT_LMR_SCALE_FACTOR = "UCI_T_LMR_ScaleFactor";
        internal const string OPT_RFP_MAX_DEPTH = "UCI_T_RFP_MaxDepth";
        internal const string OPT_RFP_MARGIN = "UCI_T_RFP_Margin";
        internal const string OPT_LMP_MAX_DEPTH = "UCI_T_LMP_MaxDepth";
        internal const string OPT_LMP_DEPTH_INCREMENT = "UCI_T_LMP_DepthIncrement";
        internal const string OPT_FUT_MAX_DEPTH = "UCI_T_FUT_MaxDepth";
        internal const string OPT_FUT_MARGIN = "UCI_T_FUT_Margin";
        internal const string OPT_IIR_MIN_DEPTH = "UCI_T_IIR_MinDepth";
        internal const string OPT_SEE_MAX_DEPTH = "UCI_T_SEE_MaxDepth";
        internal const string OPT_SEE_CAPTURE_MARGIN = "UCI_T_SEE_CaptureMargin";
        internal const string OPT_SEE_QUIET_MARGIN = "UCI_T_SEE_QuietMargin";

        static UciOptions()
        {
            options = new(StringComparer.InvariantCultureIgnoreCase)
            {
                { clearHash.Name, clearHash },
                { engineAbout.Name, engineAbout },
                { opponent.Name, opponent },
                { contempt.Name, contempt },
                { evalFile.Name, evalFile },
                { hashTableSize.Name, hashTableSize },
                { moveOverhead.Name, moveOverhead },
                { ownBook.Name, ownBook },
                { ponder.Name, ponder },
                { randomSearch.Name, randomSearch },
                { syzygyPath.Name, syzygyPath },
                { syzygyProbeRoot.Name, syzygyProbeRoot },
                { syzygyProbeDepth.Name, syzygyProbeDepth },
                { threads.Name, threads },
                { analyseMode.Name, analyseMode },
                { tmBranchFactor.Name, tmBranchFactor },
                { tmDefMovesToGo.Name, tmDefMovesToGo },
                { tmDefMovesToGoPonder.Name, tmDefMovesToGoPonder },
                { tmDefMovesToGoSuddenDeath.Name, tmDefMovesToGoPonder },
                { tmAbsoluteLimit.Name, tmAbsoluteLimit },
                { tmDifficultyMin.Name, tmDifficultyMin },
                { tmDifficultyMax.Name, tmDifficultyMax },
                { aspMinDepth.Name, aspMinDepth },
                { oneMoveMaxDepth.Name, oneMoveMaxDepth },
                { recaptureDepth.Name, recaptureDepth },
                { promotionDepth.Name, promotionDepth },
                { nmpMinDepth.Name, nmpMinDepth },
                { nmpBaseReduction.Name, nmpBaseReduction },
                { nmpIncDivisor.Name, nmpIncDivisor },
                { lmrDepthFactor.Name, lmrDepthFactor },
                { lmrMoveFactor.Name, lmrMoveFactor },
                { lmrScaleFactor.Name, lmrScaleFactor },
                { rfpMaxDepth.Name, rfpMaxDepth },
                { rfpMargin.Name, rfpMargin },
                { lmpMaxDepth.Name, lmpMaxDepth },
                { lmpDepthIncrement.Name, lmpDepthIncrement },
                { futMaxDepth.Name, futMaxDepth },
                { futMargin.Name, futMargin },
                { iirMinDepth.Name, iirMinDepth },
                { seeMaxDepth.Name, seeMaxDepth },
                { seeCaptureMargin.Name, seeCaptureMargin },
                { seeQuietMargin.Name, seeQuietMargin }
            };
        }

        public static int Contempt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return contempt.CurrentValue;
            }
        }

        public static string EvalFile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return evalFile.CurrentValue;
            }
        }

        public static int HashTableSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return hashTableSize.CurrentValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                hashTableSize.CurrentValue = value;
            }
        }

        public static int MoveOverhead
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return moveOverhead.CurrentValue;
            }
        }

        public static bool OwnBook
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ownBook.CurrentValue;
            }
        }

        public static bool Ponder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ponder.CurrentValue;
            }
        }

        public static bool RandomSearch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return randomSearch.CurrentValue;
            }
        }

        public static string SyzygyPath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return syzygyPath.CurrentValue;
            }
        }

        public static bool SyzygyProbeRoot
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return syzygyProbeRoot.CurrentValue;
            }
        }

        public static int SyzygyProbeDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return syzygyProbeDepth.CurrentValue;
            }
        }

        public static int Threads
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return threads.CurrentValue;
            }
        }

        public static bool AnalyseMode
        {
            get
            {
                return analyseMode.CurrentValue;
            }
        }

        public static int TmBranchFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return tmBranchFactor.CurrentValue;
            }
        }

        public static int TmDefMovesToGo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return tmDefMovesToGo.CurrentValue;
            }
        }

        public static int TmDefMovesToGoPonder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return tmDefMovesToGoPonder.CurrentValue;
            }
        }

        public static int TmDefMovesToGoSuddenDeath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return tmDefMovesToGoSuddenDeath.CurrentValue;
            }
        }

        public static int TmAbsoluteLimit
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return tmAbsoluteLimit.CurrentValue;
            }
        }

        public static int TmDifficultyMin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return tmDifficultyMin.CurrentValue;
            }
        }

        public static int TmDifficultyMax
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return tmDifficultyMax.CurrentValue;
            }
        }

        public static int AspMinDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return aspMinDepth.CurrentValue;
            }
        }

        public static int OneMoveMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return oneMoveMaxDepth.CurrentValue;
            }
        }

        public static int RecaptureDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return recaptureDepth.CurrentValue;
            }
        }

        public static int PromotionDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return promotionDepth.CurrentValue;
            }
        }

        public static int NmpMinDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return nmpMinDepth.CurrentValue;
            }
        }

        public static int NmpBaseReduction
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return nmpBaseReduction.CurrentValue;
            }
        }

        public static int NmpIncDivisor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return nmpIncDivisor.CurrentValue;
            }
        }

        public static int LmrDepthFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lmrDepthFactor.CurrentValue;
            }
        }

        public static int LmrMoveFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lmrMoveFactor.CurrentValue;
            }
        }

        public static int LmrScaleFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lmrScaleFactor.CurrentValue;
            }
        }

        public static int RfpMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return rfpMaxDepth.CurrentValue;
            }
        }

        public static int RfpMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return rfpMargin.CurrentValue;
            }
        }

        public static int LmpMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lmpMaxDepth.CurrentValue;
            }
        }

        public static int LmpDepthIncrement
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lmpDepthIncrement.CurrentValue;
            }
        }

        public static int FutMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return futMaxDepth.CurrentValue;
            }
        }

        public static int FutMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return futMargin.CurrentValue;
            }
        }

        public static int IirMinDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return iirMinDepth.CurrentValue;
            }
        }

        public static int SeeMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return seeMaxDepth.CurrentValue;
            }
        }

        public static int SeeCaptureMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return seeCaptureMargin.CurrentValue;
            }
        }

        public static int SeeQuietMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return seeQuietMargin.CurrentValue;
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
        private static UciOptionButton clearHash = new UciOptionButton(OPT_CLEAR_HASH);
        private static UciOptionString engineAbout = new UciOptionString(OPT_ENGINE_ABOUT, $"{APP_NAME_VER} by {APP_AUTHOR}, see {PROGRAM_URL}");
        private static UciOptionString opponent = new UciOptionString(OPT_OPPONENT, "none none computer generic engine");
        private static UciOptionSpin contempt = new UciOptionSpin(OPT_CONTEMPT, 0, -50, 50);
        private static UciOptionString evalFile = new UciOptionString(OPT_EVAL_FILE, "./Pedantic.hce");
        private static UciOptionSpin hashTableSize = new UciOptionSpin(OPT_HASH_TABLE_SIZE, 64, 16, 2048);
        private static UciOptionSpin moveOverhead = new UciOptionSpin(OPT_MOVE_OVERHEAD, 25, 0, 1000);
        private static UciOptionCheck ownBook = new UciOptionCheck(OPT_OWN_BOOK, false);
        private static UciOptionCheck ponder = new UciOptionCheck(OPT_PONDER, false);
        private static UciOptionCheck randomSearch = new UciOptionCheck(OPT_RANDOM_SEARCH, false);
        private static UciOptionString syzygyPath = new UciOptionString(OPT_SYZYGY_PATH, string.Empty);
        private static UciOptionCheck syzygyProbeRoot = new UciOptionCheck(OPT_SYZYGY_PROBE_ROOT, true);
        private static UciOptionSpin syzygyProbeDepth = new UciOptionSpin(OPT_SYZYGY_PROBE_DEPTH, 2, 1, MAX_PLY - 1);
        private static UciOptionSpin threads = new UciOptionSpin(OPT_THREADS, 1, 1, Environment.ProcessorCount);
        private static UciOptionCheck analyseMode = new UciOptionCheck(OPT_ANALYSE_MODE, false);
        private static UciOptionSpin tmBranchFactor = new UciOptionSpin(OPT_TM_BRANCH_FACTOR, 30, 20, 40);
        private static UciOptionSpin tmDefMovesToGo = new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO, 30, 20, 40);
        private static UciOptionSpin tmDefMovesToGoPonder = new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_PONDER, 35, 20, 40);
        private static UciOptionSpin tmDefMovesToGoSuddenDeath = new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH, 40, 20, 40);
        private static UciOptionSpin tmAbsoluteLimit = new UciOptionSpin(OPT_TM_ABSOLUTE_LIMIT, 5, 2, 8);
        private static UciOptionSpin tmDifficultyMin = new UciOptionSpin(OPT_TM_DIFFICULTY_MIN, 60, 0, 60);
        private static UciOptionSpin tmDifficultyMax = new UciOptionSpin(OPT_TM_DIFFICULTY_MAX, 200, 100, 300);
        private static UciOptionSpin aspMinDepth = new UciOptionSpin(OPT_ASP_MIN_DEPTH, 6, 1, 10);
        private static UciOptionSpin oneMoveMaxDepth = new UciOptionSpin(OPT_ONE_MOVE_MAX_DEPTH, 10, 1, 20);
        private static UciOptionSpin recaptureDepth = new UciOptionSpin(OPT_QS_RECAPTURE_DEPTH, 6, 4, 8);
        private static UciOptionSpin promotionDepth = new UciOptionSpin(OPT_QS_PROMOTION_DEPTH, 2, 0, 8);
        private static UciOptionSpin nmpMinDepth = new UciOptionSpin(OPT_NMP_MIN_DEPTH, 3, 3, 6);
        private static UciOptionSpin nmpBaseReduction = new UciOptionSpin(OPT_NMP_BASE_REDUCTION, 3, 1, 8);
        private static UciOptionSpin nmpIncDivisor = new UciOptionSpin(OPT_NMP_INC_DIVISOR, 4, 2, 8);
        private static UciOptionSpin lmrDepthFactor = new UciOptionSpin(OPT_LMR_DEPTH_FACTOR, 18, 10, 30);
        private static UciOptionSpin lmrMoveFactor = new UciOptionSpin(OPT_LMR_MOVE_FACTOR, 10, 5, 20);
        private static UciOptionSpin lmrScaleFactor = new UciOptionSpin(OPT_LMR_SCALE_FACTOR, 20, 10, 30);
        private static UciOptionSpin rfpMaxDepth = new UciOptionSpin(OPT_RFP_MAX_DEPTH, 6, 4, 12);
        private static UciOptionSpin rfpMargin = new UciOptionSpin(OPT_RFP_MARGIN, 85, 25, 250);
        private static UciOptionSpin lmpMaxDepth = new UciOptionSpin(OPT_LMP_MAX_DEPTH, 6, 1, 10);
        private static UciOptionSpin lmpDepthIncrement = new UciOptionSpin(OPT_LMP_DEPTH_INCREMENT, 1, 0, 5);
        private static UciOptionSpin futMaxDepth = new UciOptionSpin(OPT_FUT_MAX_DEPTH, 7, 1, 10);
        private static UciOptionSpin futMargin = new UciOptionSpin(OPT_FUT_MARGIN, 70, 35, 200);
        private static UciOptionSpin iirMinDepth = new UciOptionSpin(OPT_IIR_MIN_DEPTH, 5, 1, 10);
        private static UciOptionSpin seeMaxDepth = new UciOptionSpin(OPT_SEE_MAX_DEPTH, 7, 1, 12);
        private static UciOptionSpin seeCaptureMargin = new UciOptionSpin(OPT_SEE_CAPTURE_MARGIN, 100, 25, 200);
        private static UciOptionSpin seeQuietMargin = new UciOptionSpin(OPT_SEE_QUIET_MARGIN, 60, 25, 200);
    }
}
