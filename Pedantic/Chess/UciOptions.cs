// <copyright file="UciOptions.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Runtime.CompilerServices;

    public static class UciOptions
    {
        // Prefix UCI_T_ is for tunable paramters
        internal const string OPT_CLEAR_HASH = "Clear Hash";
        internal const string OPT_CONTEMPT = "Contempt";
        //internal const string OPT_EVAL_FILE = "EvalFile";
        internal const string OPT_HASH_TABLE_SIZE = "Hash";
        internal const string OPT_MOVE_OVERHEAD = "Move Overhead";
        internal const string OPT_PONDER = "Ponder";
        internal const string OPT_RANDOM_SEARCH = "RandomSearch";
        internal const string OPT_SYZYGY_PATH = "SyzygyPath";
        internal const string OPT_SYZYGY_PROBE_ROOT = "SyzygyProbeRoot";
        internal const string OPT_SYZYGY_PROBE_DEPTH = "SyzygyProbeDepth";
        internal const string OPT_THREADS = "Threads";
        internal const string OPT_ANALYSE_MODE = "UCI_AnalyseMode";
        internal const string OPT_ENGINE_ABOUT = "UCI_EngineAbout";
        internal const string OPT_TM_BRANCH_FACTOR = "T_TM_BranchFactor";
        internal const string OPT_TM_DEF_MOVES_TO_GO = "T_TM_DefaultMovesToGo";
        internal const string OPT_TM_DEF_MOVES_TO_GO_PONDER = "T_TM_DefaultMovesToGo_Ponder";
        internal const string OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH = "T_TM_DefaultMovesToGo_SuddenDeath";
        internal const string OPT_TM_ABSOLUTE_LIMIT = "T_TM_AbsoluteLimit";
        internal const string OPT_TM_DIFFICULTY_MIN = "T_TM_DifficultyMin";
        internal const string OPT_TM_DIFFICULTY_MAX = "T_TM_DifficultyMax";
        internal const string OPT_ASP_MIN_DEPTH = "T_ASP_MinDepth";
        internal const string OPT_ONE_MOVE_MAX_DEPTH = "T_OneMoveMaxDepth";
        internal const string OPT_QS_RECAPTURE_DEPTH = "T_QS_RecaptureDepth";
        internal const string OPT_QS_PROMOTION_DEPTH = "T_QS_PromotionDepth";
        internal const string OPT_NMP_MIN_DEPTH = "T_NMP_MinDepth";
        internal const string OPT_NMP_BASE_REDUCTION = "T_NMP_BaseReduction";
        internal const string OPT_NMP_INC_DIVISOR = "T_NMP_IncDivisor";
        internal const string OPT_LMR_DEPTH_FACTOR = "T_LMR_DepthFactor";
        internal const string OPT_LMR_MOVE_FACTOR = "T_LMR_MoveFactor";
        internal const string OPT_LMR_SCALE_FACTOR = "T_LMR_ScaleFactor";
        internal const string OPT_LMR_HISTORY_DIV = "T_LMR_HistoryDiv";
        internal const string OPT_RFP_MAX_DEPTH = "T_RFP_MaxDepth";
        internal const string OPT_RFP_MARGIN = "T_RFP_Margin";
        internal const string OPT_LMP_MAX_DEPTH = "T_LMP_MaxDepth";
        internal const string OPT_LMP_DEPTH_INCREMENT = "T_LMP_DepthIncrement";
        internal const string OPT_FUT_MAX_DEPTH = "T_FUT_MaxDepth";
        internal const string OPT_FUT_MARGIN = "T_FUT_Margin";
        internal const string OPT_IIR_MIN_DEPTH = "T_IIR_MinDepth";
        internal const string OPT_SEE_MAX_DEPTH = "T_SEE_MaxDepth";
        internal const string OPT_SEE_CAPTURE_MARGIN = "T_SEE_CaptureMargin";
        internal const string OPT_SEE_QUIET_MARGIN = "T_SEE_QuietMargin";
        internal const string OPT_LZY_EVAL_MARGIN = "T_LZY_EvalMargin";
        internal const string OPT_RZR_MAX_DEPTH = "T_RZR_MaxDepth";
        internal const string OPT_RZR_MARGIN = "T_RZR_Margin";
        internal const string OPT_HIS_MAX_BONUS = "T_HIS_MaxBonus";
        internal const string OPT_HIS_BONUS_COEFF = "T_HIS_BonusCoefficient";
        internal const string OPT_SEX_DEPTH_MULT = "T_SEX_DepthMult";
        internal const string OPT_SEX_DEPTH_OFFSET = "T_SEX_DepthOffset";
        internal const string OPT_SEX_MIN_DEPTH = "T_SEX_MinDepth";

        static UciOptions()
        {
            Optimizing = true;
            IsDataGen = false;
            options = new(StringComparer.InvariantCultureIgnoreCase)
            {
                { clearHash.Name, clearHash },
                { engineAbout.Name, engineAbout },
                { contempt.Name, contempt },
                //{ evalFile.Name, evalFile },
                { hashTableSize.Name, hashTableSize },
                { moveOverhead.Name, moveOverhead },
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
                { tmDefMovesToGoSuddenDeath.Name, tmDefMovesToGoSuddenDeath },
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
                { lmrHistoryDiv.Name, lmrHistoryDiv },
                { rfpMaxDepth.Name, rfpMaxDepth },
                { rfpMargin.Name, rfpMargin },
                { lmpMaxDepth.Name, lmpMaxDepth },
                { lmpDepthIncrement.Name, lmpDepthIncrement },
                { futMaxDepth.Name, futMaxDepth },
                { futMargin.Name, futMargin },
                { iirMinDepth.Name, iirMinDepth },
                { seeMaxDepth.Name, seeMaxDepth },
                { seeCaptureMargin.Name, seeCaptureMargin },
                { seeQuietMargin.Name, seeQuietMargin },
                { lzyEvalMargin.Name, lzyEvalMargin },
                { rzrMaxDepth.Name, rzrMaxDepth },
                { rzrMargin.Name, rzrMargin },
                { hisMaxBonus.Name, hisMaxBonus },
                { hisBonusCoefficient.Name, hisBonusCoefficient },
                { sexDepthMult.Name, sexDepthMult },
                { sexDepthOffset.Name, sexDepthOffset },
                { sexMinDepth.Name, sexMinDepth },
            };
        }

        public static void WriteOptions()
        {
            using StreamWriter sw = System.IO.File.CreateText("./config.json");
            sw.WriteLine("{");

            foreach (var kvp in options)
            {
                UciOptionBase opt = kvp.Value;
                if (opt.Name.StartsWith("T_", StringComparison.InvariantCultureIgnoreCase))
                {
                    UciOptionSpin spin = (UciOptionSpin)opt;
                    sw.WriteLine($"    \"{spin.Name}\": {{");
                    sw.WriteLine($"        \"value\": {spin.CurrentValue},");
                    sw.WriteLine($"        \"min_value\": {spin.MinValue},");
                    sw.WriteLine($"        \"max_value\": {spin.MaxValue},");
                    int step = Math.Max((spin.MaxValue - spin.MinValue) / 8, 1);
                    sw.WriteLine($"        \"step\": {step}");
                    sw.WriteLine("    },");
                }
            }

            sw.WriteLine("}");
            sw.Close();
        }

        public static int Contempt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return contempt.CurrentValue;
            }
        }

        //public static string EvalFile
        //{
        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    get
        //    {
        //        return evalFile.CurrentValue;
        //    }
        //}

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

        public static int LmrHistoryDiv
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lmrHistoryDiv.CurrentValue;
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

        public static int LzyEvalMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lzyEvalMargin.CurrentValue;
            }
        }

        public static int RzrMaxDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return rzrMaxDepth.CurrentValue;
            }
        }

        public static int RzrMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return rzrMargin.CurrentValue;
            }
        }

        public static int HisMaxBonus
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return hisMaxBonus.CurrentValue;
            }
        }

        public static int HisBonusCoefficient
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return hisBonusCoefficient.CurrentValue;
            }
        }

        public static int SexDepthMult
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return sexDepthMult.CurrentValue;
            }
        }

        public static int SexDepthOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return sexDepthOffset.CurrentValue;
            }
        }

        public static int SexMinDepth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return sexMinDepth.CurrentValue;
            }
        }

        public static bool Optimizing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        public static bool IsDataGen
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        public static void WriteLine()
        {
            foreach (var kvp in options)
            {
                UciOptionBase opt = kvp.Value;
                if (Optimizing || !opt.Name.StartsWith("T_", StringComparison.InvariantCultureIgnoreCase))
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
        private static UciOptionSpin contempt = new UciOptionSpin(OPT_CONTEMPT, 0, -50, 50);
        //private static UciOptionString evalFile = new UciOptionString(OPT_EVAL_FILE, "./Pedantic.hce");
        private static UciOptionSpin hashTableSize = new UciOptionSpin(OPT_HASH_TABLE_SIZE, 64, 16, 1024);
        private static UciOptionSpin moveOverhead = new UciOptionSpin(OPT_MOVE_OVERHEAD, 50, 0, 1000);
        private static UciOptionCheck ponder = new UciOptionCheck(OPT_PONDER, false);
        private static UciOptionCheck randomSearch = new UciOptionCheck(OPT_RANDOM_SEARCH, false);
        private static UciOptionString syzygyPath = new UciOptionString(OPT_SYZYGY_PATH, string.Empty);
        private static UciOptionCheck syzygyProbeRoot = new UciOptionCheck(OPT_SYZYGY_PROBE_ROOT, true);
        private static UciOptionSpin syzygyProbeDepth = new UciOptionSpin(OPT_SYZYGY_PROBE_DEPTH, 2, 1, MAX_PLY - 1);
        private static UciOptionSpin threads = new UciOptionSpin(OPT_THREADS, 1, 1, Environment.ProcessorCount);
        private static UciOptionCheck analyseMode = new UciOptionCheck(OPT_ANALYSE_MODE, false);
        private static UciOptionSpin tmBranchFactor = new UciOptionSpin(OPT_TM_BRANCH_FACTOR, 29, 20, 40);
        private static UciOptionSpin tmDefMovesToGo = new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO, 24, 20, 40);
        private static UciOptionSpin tmDefMovesToGoPonder = new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_PONDER, 33, 20, 40);
        private static UciOptionSpin tmDefMovesToGoSuddenDeath = new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH, 38, 20, 40);
        private static UciOptionSpin tmAbsoluteLimit = new UciOptionSpin(OPT_TM_ABSOLUTE_LIMIT, 5, 2, 8);
        private static UciOptionSpin tmDifficultyMin = new UciOptionSpin(OPT_TM_DIFFICULTY_MIN, 79, 0, 100);
        private static UciOptionSpin tmDifficultyMax = new UciOptionSpin(OPT_TM_DIFFICULTY_MAX, 204, 100, 300);
        private static UciOptionSpin aspMinDepth = new UciOptionSpin(OPT_ASP_MIN_DEPTH, 7, 1, 10);
        private static UciOptionSpin oneMoveMaxDepth = new UciOptionSpin(OPT_ONE_MOVE_MAX_DEPTH, 13, 1, 20);
        private static UciOptionSpin recaptureDepth = new UciOptionSpin(OPT_QS_RECAPTURE_DEPTH, 8, 4, 10);
        private static UciOptionSpin promotionDepth = new UciOptionSpin(OPT_QS_PROMOTION_DEPTH, 2, 0, 8);
        private static UciOptionSpin nmpMinDepth = new UciOptionSpin(OPT_NMP_MIN_DEPTH, 3, 3, 6);
        private static UciOptionSpin nmpBaseReduction = new UciOptionSpin(OPT_NMP_BASE_REDUCTION, 4, 1, 8);
        private static UciOptionSpin nmpIncDivisor = new UciOptionSpin(OPT_NMP_INC_DIVISOR, 5, 2, 8);
        private static UciOptionSpin lmrDepthFactor = new UciOptionSpin(OPT_LMR_DEPTH_FACTOR, 21, 10, 30);
        private static UciOptionSpin lmrMoveFactor = new UciOptionSpin(OPT_LMR_MOVE_FACTOR, 11, 5, 20);
        private static UciOptionSpin lmrScaleFactor = new UciOptionSpin(OPT_LMR_SCALE_FACTOR, 21, 10, 30);
        private static UciOptionSpin rfpMaxDepth = new UciOptionSpin(OPT_RFP_MAX_DEPTH, 8, 4, 12);
        private static UciOptionSpin rfpMargin = new UciOptionSpin(OPT_RFP_MARGIN, 109, 25, 250);
        private static UciOptionSpin lmpMaxDepth = new UciOptionSpin(OPT_LMP_MAX_DEPTH, 5, 1, 10);
        private static UciOptionSpin lmpDepthIncrement = new UciOptionSpin(OPT_LMP_DEPTH_INCREMENT, 4, 0, 10);
        private static UciOptionSpin futMaxDepth = new UciOptionSpin(OPT_FUT_MAX_DEPTH, 9, 1, 10);
        private static UciOptionSpin futMargin = new UciOptionSpin(OPT_FUT_MARGIN, 120, 35, 200);
        private static UciOptionSpin iirMinDepth = new UciOptionSpin(OPT_IIR_MIN_DEPTH, 4, 1, 10);
        private static UciOptionSpin seeMaxDepth = new UciOptionSpin(OPT_SEE_MAX_DEPTH, 8, 1, 12);
        private static UciOptionSpin seeCaptureMargin = new UciOptionSpin(OPT_SEE_CAPTURE_MARGIN, 102, 25, 200);
        private static UciOptionSpin seeQuietMargin = new UciOptionSpin(OPT_SEE_QUIET_MARGIN, 26, 25, 200);
        private static UciOptionSpin lzyEvalMargin = new UciOptionSpin(OPT_LZY_EVAL_MARGIN, 599, 0, 1200);
        private static UciOptionSpin rzrMaxDepth = new UciOptionSpin(OPT_RZR_MAX_DEPTH, 4, 0, 5);
        private static UciOptionSpin rzrMargin = new UciOptionSpin(OPT_RZR_MARGIN, 87, 50, 400);
        private static UciOptionSpin hisMaxBonus = new UciOptionSpin(OPT_HIS_MAX_BONUS, 1772, 500, 2500);
        private static UciOptionSpin hisBonusCoefficient = new UciOptionSpin(OPT_HIS_BONUS_COEFF, 159, 50, 250);
        private static UciOptionSpin lmrHistoryDiv = new UciOptionSpin(OPT_LMR_HISTORY_DIV, 5331, 2048, 16384);
        private static UciOptionSpin sexDepthMult = new UciOptionSpin(OPT_SEX_DEPTH_MULT, 21, 4, 32);
        private static UciOptionSpin sexDepthOffset = new UciOptionSpin(OPT_SEX_DEPTH_OFFSET, 0, -2, 2);
        private static UciOptionSpin sexMinDepth = new UciOptionSpin(OPT_SEX_MIN_DEPTH, 10, 4, 12);
    }
}
