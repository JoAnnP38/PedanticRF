using static Pedantic.Chess.Constants;

namespace Pedantic.Chess
{
    public static class UciOptions
    {
        // Prefix UCI_T_ is for tunable paramters
        internal const string OPT_HASH_TABLE_SIZE = "Hash";
        internal const string OPT_MOVE_OVERHEAD = "Move Overhead";
        internal const string OPT_ENGINE_ABOUT = "UCI_EngineAbout";
        internal const string OPT_TM_BRANCH_FACTOR = "UCI_T_TM_BranchFactor";
        internal const string OPT_TM_DEF_MOVES_TO_GO = "UCI_T_TM_DefaultMovesToGo";
        internal const string OPT_TM_DEF_MOVES_TO_GO_PONDER = "UCI_T_TM_DefaultMovesToGo_Ponder";
        internal const string OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH = "UCI_T_TM_DefaultMovesToGo_SuddenDeath";
        internal const string OPT_TM_ABSOLUTE_LIMIT = "UCI_T_TM_AbsoluteLimit";
        internal const string OPT_TM_DIFFICULTY_MIN = "UCI_T_TM_DifficultyMin";
        internal const string OPT_TM_DIFFICULTY_MAX = "UCI_T_TM_DifficultyMax";

        static UciOptions()
        {
            options = new(initialOptions.Length);
            foreach (var opt in initialOptions)
            {
                options.Add(opt.Name, opt);
            }
        }

        public static int HashTableSize
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_HASH_TABLE_SIZE];
                return opt.DefaultValue;
            }
        }

        public static int MoveOverhead
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_MOVE_OVERHEAD];
                return opt.DefaultValue;
            }
        }

        public static int TmBranchFactor
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_BRANCH_FACTOR];
                return opt.DefaultValue;
            }
        }

        public static int TmDefMovesToGo
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DEF_MOVES_TO_GO];
                return opt.DefaultValue;
            }
        }

        public static int TmDefMovesToGoPonder
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DEF_MOVES_TO_GO_PONDER];
                return opt.DefaultValue;
            }
        }

        public static int TmDefMovesToGoSuddenDeath
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH];
                return opt.DefaultValue;
            }
        }

        public static int TmAbsoluteLimit
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_ABSOLUTE_LIMIT];
                return opt.DefaultValue;
            }
        }

        public static int TmDifficultyMin
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DIFFICULTY_MIN];
                return opt.DefaultValue;
            }
        }

        public static int TmDifficultyMax
        {
            get
            {
                UciOptionSpin opt = (UciOptionSpin)options[OPT_TM_DIFFICULTY_MAX];
                return opt.DefaultValue;
            }
        }

        public static IList<UciOptionBase> Options => options.Values;

        private static readonly SortedList<string, UciOptionBase> options;

        // UCI Options
        private static readonly UciOptionBase[] initialOptions =
        [
            new UciOptionSpin(OPT_HASH_TABLE_SIZE, 64, 16, 2048),
            new UciOptionSpin(OPT_MOVE_OVERHEAD, 25, 0, 1000),
            new UciOptionString(OPT_ENGINE_ABOUT, $"{APP_NAME_VER} by {APP_AUTHOR}, see {PROGRAM_URL}"),
            new UciOptionSpin(OPT_TM_BRANCH_FACTOR, 30, 20, 40),
            new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO, 30, 20, 40),
            new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_PONDER, 35, 20, 40),
            new UciOptionSpin(OPT_TM_DEF_MOVES_TO_GO_SUDDEN_DEATH, 40, 20, 40),
            new UciOptionSpin(OPT_TM_ABSOLUTE_LIMIT, 5, 2, 8),
            new UciOptionSpin(OPT_TM_DIFFICULTY_MIN, 60, 0, 60),
            new UciOptionSpin(OPT_TM_DIFFICULTY_MAX, 200, 100, 300)
        ];
    }
}
