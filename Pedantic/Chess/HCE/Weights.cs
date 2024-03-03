using System;
using System.Runtime.CompilerServices;
using System.Text;
using Pedantic.Utilities;

namespace Pedantic.Chess.HCE
{
    // Coefficients or weights for the HCE evaluation function
    public sealed class Weights : IInitialize
    {
        #region Feature Identifiers/Constants

        public static readonly Guid HCE_WEIGHTS_VERSION = new("e3afd1d2-468e-4531-a73b-01f904fc06b7");

        // 6 (piece weights) + (6x64x16x2) 6 Piece Types X 64 Squares X 16 King Buckets X 2 Both Kings
        public const int MAX_WEIGHTS = 12757;
        public const int PIECE_VALUES = 0;          // start of piece weights
        public const int FRIENDLY_KB_PST = 6;       // start of friend king bucket piece square tables
        public const int ENEMY_KB_PST = 6150;       // start of enemy king bucket piece square tables
        public const int KNIGHT_MOBILITY = 12294;   // start of knight mobility values
        public const int BISHOP_MOBILITY = 12303;   // start of bishop mobility values
        public const int ROOK_MOBILITY = 12317;     // start of rook mobility values
        public const int QUEEN_MOBILITY = 12332;    // start of queen mobility values
        public const int PASSED_PAWN = 12360;       // passed pawn PST
        public const int PHALANX_PAWN = 12424;      // adjacent/phalanx pawn PST
        public const int CHAINED_PAWN = 12488;      // pawn chain/connected pawn PST
        public const int PAWN_RAM = 12552;          // pawn ram PST
        public const int ISOLATED_PAWN = 12616;     // isolated pawn 
        public const int KING_ATTACK = 12680;       // attacks near king
        public const int CAN_CASTLE_KS = 12682;     // can castle king-side
        public const int CAN_CASTLE_QS = 12683;     // can castle queen-side
        public const int KS_DIAG_MOBILITY = 12684;  // open diagonal line attacks against king
        public const int KS_ORTH_MOBILITY = 12697;  // open orthogonal line attacks against king
        public const int KING_OUTSIDE_PP_SQUARE = 12711;    // king cannot stop promotion
        public const int PP_CAN_ADVANCE = 12712;    // passed pawn can safely advance
        public const int BLOCKED_PASSED_PAWN = 12716;       // blocked passed pawn
        public const int TEMPO = 12756;             // tempo bonus for side moving

        #endregion

        #region Constructors

        public Weights()
        {
            for (Piece piece = Piece.Pawn; piece <= Piece.Queen; piece++)
            {
                weights[PIECE_VALUES + (int)piece] = S(piece.Value(), piece.Value());
            }
        }

        public Weights(Score[] wts)
        {
            Util.Assert(wts.Length == MAX_WEIGHTS);
            Array.Copy(wts, weights, MAX_WEIGHTS);
        }

        #endregion

        #region Accessors

        public ref Score this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref weights[i];
        }

        public int Length => MAX_WEIGHTS;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PieceValue(Piece piece)
        {
            Util.Assert(piece != Piece.None);
            return weights[PIECE_VALUES + (int)piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score FriendlyPieceSquareValue(Piece piece, KingBuckets kb, SquareIndex square)
        {
            Util.Assert(piece != Piece.None);
            Util.Assert(square != SquareIndex.None);

            int offset = ((int)piece * MAX_KING_BUCKETS + kb.Friendly) * MAX_SQUARES + (int)square;
            return weights[FRIENDLY_KB_PST + offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score EnemyPieceSquareValue(Piece piece, KingBuckets kb, SquareIndex square)
        {
            Util.Assert(piece != Piece.None);
            Util.Assert(square != SquareIndex.None);

            int offset = ((int)piece * MAX_KING_BUCKETS + kb.Enemy) * MAX_SQUARES + (int)square;
            return weights[ENEMY_KB_PST + offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score KnightMobility(int mobility)
        {
            return weights[KNIGHT_MOBILITY + mobility];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score BishopMobility(int mobility)
        {
            return weights[BISHOP_MOBILITY + mobility];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score RookMobility(int mobility)
        {
            return weights[ROOK_MOBILITY + mobility];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score QueenMobility(int mobility)
        {
            return weights[QUEEN_MOBILITY + mobility];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PieceMobility(Piece piece, int mobility)
        {
            return piece switch
            {
                Piece.Knight => KnightMobility(mobility),
                Piece.Bishop => BishopMobility(mobility),
                Piece.Rook => RookMobility(mobility),
                Piece.Queen => QueenMobility(mobility),
                _ => Score.Zero
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PassedPawn(SquareIndex sq)
        {
            return weights[PASSED_PAWN + (int)sq];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PhalanxPawn(SquareIndex sq)
        {
            return weights[PHALANX_PAWN + (int)sq];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score ChainedPawn(SquareIndex sq)
        {
            return weights[CHAINED_PAWN + (int)sq];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PawnRam(SquareIndex sq)
        {
            return weights[PAWN_RAM + (int)sq];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score IsolatedPawn(SquareIndex sq)
        {
            return weights[ISOLATED_PAWN + (int)sq];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score KingAttack(int dist)
        {
            return weights[KING_ATTACK + dist];
        }

        public Score CanCastleKS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[CAN_CASTLE_KS];
        }

        public Score CanCastleQS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[CAN_CASTLE_QS];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score KsDiagonalMobility(int count)
        {
            if (count <= 0)
            {
                return Score.Zero;
            }
            return weights[KS_DIAG_MOBILITY + count - 1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score KsOrthogonalMobility(int count)
        {
            if (count <= 0)
            {
                return Score.Zero;
            }
            return weights[KS_ORTH_MOBILITY + count - 1];
        }

        public Score KingOutsidePasserSquare
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[KING_OUTSIDE_PP_SQUARE];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PassedPawnCanAdvance(Rank rank)
        {
            Util.Assert(rank >= Rank.Rank4);
            int index = rank - Rank.Rank4;
            return weights[PP_CAN_ADVANCE + index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score BlockedPassedPawn(Piece blocker, Rank rank)
        {
            Util.Assert(rank >= Rank.Rank2 && rank <= Rank.Rank7);
            int index = ((int)blocker - 1) * MAX_COORDS + (int)rank;
            return weights[BLOCKED_PASSED_PAWN + index];
        }

        public Score TempoBonus
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[TEMPO];
        }

        public static Weights Default => defWts;

        #endregion

        #region I/O

        public void Save(string path)
        {
            try
            {
                using Stream output = System.IO.File.OpenWrite(path);
                using BinaryWriter writer = new(output, Encoding.UTF8);
                writer.Write(HCE_WEIGHTS_VERSION.ToByteArray());
                writer.Write(MAX_WEIGHTS);
                for (int n = 0; n < MAX_WEIGHTS; n++)
                {
                    writer.Write(weights[n]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occurred while saving Pedantic weights.", ex);
            }
        }

        public static Weights Load(string path)
        {
            try
            {
                using Stream input = System.IO.File.OpenRead(path);
                using BinaryReader reader = new(input, Encoding.UTF8);
                byte[] guidBytes = reader.ReadBytes(16);
                Guid fileGuid = new(guidBytes);
                if (fileGuid != HCE_WEIGHTS_VERSION)
                {
                    throw new Exception("Incorrect file version.");
                }
                int length = reader.ReadInt32();
                if (length != MAX_WEIGHTS)
                {
                    throw new Exception("Incorrect file length.");
                }

                Score[] wts = new Score[MAX_WEIGHTS];
                for (int n = 0; n < length; n++)
                {
                    wts[n] = (Score)reader.ReadInt32();
                }            

                return new Weights(wts);
            }
            catch (Exception ex)
            {
                throw new FileLoadException("Could not load the Pedantic weights file.", ex);
            }
        }

        #endregion

        public static void Initialize() {}

        private static Score S(short mgValue, short egValue) => new Score(mgValue, egValue);
        private readonly Score[] weights = new Score[MAX_WEIGHTS];

        // Solution sample size: 6000010, generated on Sat, 02 Mar 2024 13:34:49 GMT
        // Solution K: 0.003850, error: 0.086124, accuracy: 0.4948
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 94, 175),   S(418, 530),   S(405, 628),   S(566, 957),   S(1287, 1711), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 96, -117),  S(153, -98),   S( 28, -28),   S(-38,  27),   S(-39,  16),   S(-21, -11),   S(-43, -13),   S(-41, -22),
            S(105, -111),  S(101, -100),  S( -4, -59),   S(-29, -42),   S(-24, -21),   S(-21, -32),   S(-42, -26),   S(-41, -40),
            S( 88, -81),   S( 74, -66),   S(  9, -44),   S(  0, -64),   S(-12, -55),   S( -2, -50),   S(-23, -44),   S(-29, -37),
            S( 50, -23),   S( 49, -52),   S( 16, -47),   S( 15, -66),   S( -1, -58),   S(-25, -52),   S(-28, -41),   S(-42, -13),
            S( 71,  27),   S( 37, -17),   S( 47, -22),   S( 67, -81),   S( 42, -54),   S(-18, -37),   S(-36, -19),   S(-39,  69),
            S( 84,  35),   S( 29,  58),   S( 14, -23),   S( 22, -41),   S(-32, -18),   S(  3, -19),   S(  2, -13),   S( -3,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 32, -35),   S( 27, -37),   S( 43, -22),   S(-11,  17),   S(-14, -11),   S(  1, -17),   S(-36, -12),   S(-45,  19),
            S( 23, -39),   S(  8, -38),   S(  7, -48),   S( -9, -38),   S(-16, -25),   S(-14, -30),   S(-42, -17),   S(-52,  -9),
            S( 14, -27),   S( 15, -24),   S( 20, -47),   S( 10, -49),   S(-18, -27),   S(  4, -36),   S(-20, -27),   S(-27,  -8),
            S( 17,   6),   S( 15, -37),   S( 16, -34),   S(  7, -48),   S(  2, -32),   S( -1, -38),   S(-29, -20),   S(-45,  25),
            S(  4,  60),   S(-24,  -4),   S( -9, -15),   S( -6, -20),   S( 28,  -9),   S(-15,  -5),   S( -8,   3),   S(-50,  76),
            S( 32,  53),   S(  8,   5),   S(-65, -20),   S(-34,   2),   S(-40,  -3),   S(-86,  -1),   S(-71,  30),   S(-76,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28,   0),   S(-36,   2),   S(-15,  -2),   S(-14,   7),   S(  8, -14),   S( 32, -20),   S(  8, -45),   S(-18, -16),
            S(-28, -14),   S(-50,  -7),   S(-30, -31),   S(-23, -28),   S(  1, -26),   S(  3, -26),   S(-12, -35),   S(-34, -20),
            S(-43,   0),   S(-29, -21),   S(-14, -43),   S( -4, -58),   S(  1, -27),   S( 14, -29),   S( -7, -30),   S(-21, -13),
            S(-54,  21),   S(-32, -29),   S(-29, -29),   S( -4, -48),   S( 16, -40),   S( -6, -23),   S(-12,  -8),   S(-29,  12),
            S(-37,  66),   S(-69,  15),   S(-49, -29),   S(-56, -12),   S( 34,  -8),   S( -7,   7),   S(-31,  22),   S(-37,  93),
            S(-102,  88),  S(-119,  86),  S(-117,   7),  S(-56, -35),   S(-27,   1),   S(-41,  -6),   S(-46,   5),   S(-37,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32,  -8),   S(-39,  -2),   S(-29,  -3),   S( -7, -38),   S(-13,  -7),   S( 29, -10),   S( 84, -63),   S( 57, -75),
            S(-39, -28),   S(-53, -18),   S(-35, -44),   S(-25, -29),   S(-22, -22),   S(  1, -31),   S( 53, -59),   S( 47, -68),
            S(-43, -23),   S(-20, -46),   S( -7, -59),   S( -5, -66),   S( -9, -45),   S(  6, -37),   S( 28, -47),   S( 52, -52),
            S(-48,   0),   S(-22, -51),   S(-16, -58),   S(  3, -70),   S( 18, -65),   S(  4, -46),   S( 19, -32),   S( 43, -10),
            S(-20,  37),   S(-37, -11),   S( -5, -50),   S(  4, -55),   S( 80, -50),   S( 64, -43),   S( 44,  21),   S( 39,  73),
            S(-82, 120),   S(-70,  35),   S(-32, -46),   S(-48, -51),   S(-10, -30),   S( 39, -32),   S( 57,  21),   S( 15,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-85,  19),   S( 19, -32),   S(  1,  -3),   S( -6,  24),   S(-44,  -4),   S(-21,   2),   S(-72,   5),   S(-58,   3),
            S(-39,  12),   S( 40, -16),   S( 17, -31),   S( 44, -44),   S(-13, -19),   S(-42, -28),   S(-32, -33),   S(-31, -19),
            S( 34, -12),   S( 48,  -5),   S(-15,   9),   S(  8, -31),   S(-40, -23),   S( -5, -40),   S(-59, -33),   S(-19, -14),
            S(  1,  36),   S( 13,  27),   S( 25,   6),   S( 23, -10),   S( 16, -28),   S(-29, -28),   S(  0, -37),   S( 43, -31),
            S( 13,  79),   S(-15,  75),   S(  7,  11),   S(-25, -12),   S( 14,  -9),   S(-25,  -3),   S(-40, -38),   S( 59,  12),
            S( 68,  91),   S( 37,  56),   S( 31,   3),   S( 11, -16),   S(  4, -50),   S( 11, -14),   S( -1, -32),   S(-37,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-105,  54),  S(-60,  32),   S(-11,  19),   S( 34,   1),   S(-14,  15),   S(-17,  13),   S(-47,   5),   S(-29,  21),
            S(-77,  27),   S(-64,  20),   S( 23,  -9),   S( -2,  15),   S(  6,  -7),   S(-17, -14),   S(-25, -15),   S(-32,   6),
            S(-71,  52),   S(-25,  17),   S( 73, -28),   S( -8, -18),   S( 36, -12),   S(-28,  -7),   S(-23,  -2),   S(-27,   6),
            S(-39,  64),   S(-30,  29),   S( 27,   8),   S( 60,  11),   S(  9,   6),   S(-55,  10),   S(  9, -10),   S( 20,  12),
            S( 62,  60),   S( 99,  19),   S( 62,  40),   S( 45,  27),   S(-50,  69),   S( 36,   2),   S( 14,  20),   S( 19,  46),
            S( 58,  38),   S( 66,  18),   S( 26,  -3),   S( 35,  -5),   S( 45, -18),   S(  9, -16),   S(-12, -15),   S( 49,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43,  22),   S(-16,   1),   S(  7,  -2),   S(-19,  11),   S( 30, -20),   S(-23,   4),   S(-46,  -2),   S(-52,  14),
            S(-42,   4),   S(-11, -19),   S(-10, -33),   S( 36, -17),   S( 42, -17),   S( 25, -17),   S(-35,  -9),   S(-81,  14),
            S(-44,  14),   S( 18, -20),   S( -2, -18),   S( -9, -16),   S( 37, -13),   S( 62, -30),   S(  1, -13),   S(-49,  22),
            S(-16,  24),   S(-45,   7),   S( 13, -17),   S( 14,  -5),   S( 46,  -1),   S( 54,  -8),   S( 32,  -6),   S( 48,   5),
            S(-24,  42),   S(-21,  12),   S(-11,  -6),   S( 27, -13),   S( 38,  34),   S( 77,  32),   S(100,  -6),   S( 88,  33),
            S( 83,  35),   S( 34,   7),   S( 15, -14),   S( 10, -30),   S( 46, -17),   S( 54, -15),   S( 24,   4),   S( 67,  28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57, -17),   S(-47, -15),   S( -2, -20),   S(-37, -19),   S(-17, -28),   S( 20, -19),   S(-10, -41),   S(-65, -11),
            S(-56, -32),   S(-48, -39),   S(-39, -44),   S(-18, -42),   S(-14, -29),   S( 40, -40),   S( 44, -38),   S(-29, -22),
            S(-66, -26),   S(-69, -30),   S(-25, -47),   S(-11, -46),   S( -2, -22),   S( 15, -17),   S( 45, -42),   S( 31, -33),
            S(-49, -13),   S(-52, -42),   S(-44, -47),   S(-36, -15),   S(  1, -23),   S( 14,  -7),   S( 37,  -7),   S( 70, -18),
            S(-55,   8),   S( 12, -40),   S(-28, -36),   S(  4, -60),   S( 20,  14),   S( 34,   4),   S( 90,  41),   S(107,  50),
            S( 35,  -8),   S( -5, -37),   S( 59, -68),   S( 12, -57),   S( 27, -38),   S( 45, -36),   S( 34,  51),   S( 84,  62),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30,  42),   S(-47,  34),   S(-19,   6),   S( -8, -16),   S( 13,  14),   S(-46,  12),   S(-104,  11),  S(-58,  24),
            S(-63,  19),   S(-17,  18),   S(-41,  23),   S(-28,  -3),   S(-36, -13),   S(-26, -32),   S(-12, -31),   S(-29,  -2),
            S( -5,  63),   S( 21,  70),   S(-18,  44),   S(-11,  29),   S( 13, -14),   S(-70, -28),   S(-36, -34),   S(-15, -25),
            S( 40, 104),   S( 49, 114),   S( 82,  65),   S( 22,  61),   S(-11,   0),   S(-47, -18),   S( 41, -35),   S( 22, -28),
            S( 54, 117),   S( 51, 127),   S( 31, 141),   S( 70,  99),   S( 12,  -5),   S( 12,   2),   S( 15, -23),   S( 17,   0),
            S(105, 130),   S(110, 153),   S( 96, 188),   S( 32,  36),   S(  8,  17),   S( -6, -43),   S(-10, -35),   S( 13,   7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-77,  60),   S(-62,  36),   S(-23,  32),   S( 15,  36),   S( -6,  14),   S(-81,  20),   S(-82,   6),   S(-94,  29),
            S(-42,  24),   S(-28,  10),   S(-72,  22),   S(-15,  29),   S(-76,   4),   S(-75,   7),   S(-103,  -4),  S(-42,  13),
            S(-72,  59),   S(-13,  47),   S(  2,  64),   S(-46,  66),   S(-73,  54),   S(-93,  12),   S(-21, -22),   S(-25,   0),
            S( 17,  78),   S( 68,  80),   S( 54, 103),   S( 71, 120),   S(-50,  63),   S( -6,  14),   S( 15,  -1),   S( 35,  -6),
            S( 79,  64),   S( 56, 114),   S( 79, 124),   S( 94, 132),   S( 65, 110),   S( 46,  34),   S( 20,  -1),   S( 27,   0),
            S( 52,  71),   S( 67,  70),   S(119, 107),   S(110, 162),   S( 36,  71),   S(  1, -24),   S( 11, -33),   S(  2, -30),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-99,  10),   S(-91,   1),   S(-35,  -3),   S(  1,  25),   S(-29, -11),   S(-109,  40),  S(-124,  24),  S(-69,  31),
            S(-79,  -4),   S(-27, -24),   S(-21, -21),   S(-80,  24),   S(-22,  20),   S( -6,  16),   S(-139,  30),  S(-89,  18),
            S(-34,  -5),   S(-36,  -4),   S(-12,   6),   S(-99,  57),   S(-75,  94),   S( 22,  38),   S(-93,  36),   S(-92,  49),
            S( 16,  -8),   S(-35,   9),   S(  2,  23),   S(-19,  69),   S( 14,  94),   S(-21,  96),   S( 15,  38),   S( 64,  24),
            S( 64,  -9),   S( 25,  -3),   S( 39,  30),   S( 47, 103),   S( 74, 113),   S( 63,  76),   S( 37,  77),   S( 87,  38),
            S( 52, -16),   S( 21, -11),   S( 30,  30),   S( 81, 118),   S( 52,  87),   S( 54,  59),   S( 50,  59),   S( 30,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-130,   6),  S(-78,   4),   S( -9,   8),   S(  3,  16),   S(  2,  -4),   S(-81,  42),   S(-58,  21),   S(-66,  38),
            S(-70, -26),   S(-61, -41),   S(-50, -34),   S(-76,   4),   S(-48,  10),   S(-28,  22),   S(-103,  47),  S(-100,  41),
            S(-53, -22),   S(-24, -50),   S(-42,  -6),   S(-19,   0),   S(-41,  24),   S(  9,  48),   S(-38,  59),   S(-41,  61),
            S( 32, -17),   S(-51, -20),   S(-18, -27),   S(-45,  19),   S( -8,  47),   S( 62,  47),   S( 26,  95),   S( 88,  58),
            S(  9, -10),   S(-26, -18),   S(-33, -18),   S(-11,   7),   S( 60,  81),   S( 60,  75),   S(136,  86),   S(144,  70),
            S(  1, -37),   S(-10, -47),   S( -1, -30),   S( 13, -10),   S( 31,  31),   S( 54, 124),   S( 96, 149),   S(115, 112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1,  31),   S(-25, -29),   S(-19,   6),   S( -1,   1),   S( -8,  -7),   S( -5,  -1),   S(-30,  -1),   S(-35,   8),
            S(-26, -18),   S( 30,   8),   S(  3,  34),   S( -5, -18),   S(-14,  29),   S(-13, -15),   S(-40, -30),   S(-18, -61),
            S( 19,  45),   S(  4,  62),   S( 14,  30),   S( 15,  25),   S( 15,  22),   S(-12, -23),   S(-22, -40),   S(-20, -50),
            S(-13,  63),   S( 27,  93),   S( 61, 123),   S( 56,  70),   S( -6, -25),   S( -1, -45),   S(  1, -26),   S(-40, -83),
            S( 62,  76),   S( 83, 192),   S( 77, 140),   S( 14,  53),   S(  8,  -6),   S( -1, -21),   S( 10, -25),   S(-20, -79),
            S( 50, 136),   S( 57, 268),   S(119, 253),   S( 50,  94),   S( -5,  -7),   S( -2, -28),   S(-11, -67),   S(-11, -85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39, -12),   S(-25,  -5),   S( -5,  -6),   S(  3,  10),   S(-10, -16),   S(-31, -16),   S(-32, -26),   S(-33,   5),
            S(-11, -44),   S(-42,  -9),   S(-13,  -2),   S(  6,   9),   S(-37,   6),   S( -5,   1),   S(-60, -47),   S(-30,   0),
            S( 20,  13),   S( 21,   3),   S(-11,  34),   S( 12,  56),   S(-22,  35),   S(-19, -13),   S(-34, -31),   S( -9, -53),
            S( 10,  33),   S( 47,  52),   S( 41,  92),   S(  8,  80),   S(  4,  38),   S( 29,  -6),   S( 11, -20),   S( 24, -72),
            S( 37,  52),   S( 77, 136),   S(106, 171),   S(109, 195),   S( 63,  98),   S( 10,  -1),   S( 14, -46),   S(  6, -80),
            S( 35,  67),   S( 89, 169),   S( 90, 216),   S(115, 249),   S( 66, 100),   S(  6,  -3),   S(  2, -33),   S( 16, -24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52, -61),   S(-37, -38),   S( -9, -36),   S( -5, -19),   S( -1,  -8),   S(-43,  13),   S(-44,   4),   S( -6,  22),
            S(-16, -13),   S(-22, -48),   S(-28, -48),   S( -3,  28),   S(-13,  22),   S( -4,  10),   S(-13,  18),   S(-40,   4),
            S(-11, -37),   S(-13, -41),   S(-19, -18),   S(  4,  15),   S( -8,  37),   S( -8,  32),   S(  1,  -7),   S(  0,  18),
            S(  9, -42),   S( 21,   8),   S(  8,  -7),   S( 18,  77),   S(  4, 130),   S( 15,  63),   S( 20,  16),   S( 34,  33),
            S( 14, -55),   S( 27,  -1),   S( 39,  40),   S( 57, 144),   S( 64, 200),   S( 86, 156),   S( 65,  71),   S( 52,  74),
            S( 33,  15),   S( 23, -14),   S( 38,  83),   S( 53, 143),   S( 87, 242),   S( 51, 161),   S( 53, 129),   S( 30,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -63),   S(-32, -42),   S(-16, -37),   S( -7, -22),   S( 20,  37),   S( 18,  38),   S(-23,   1),   S( 26,  54),
            S( 10, -44),   S(-41, -44),   S(-13, -69),   S( 14,   6),   S(-10,  47),   S( -7,  24),   S( 15,  35),   S(-10,   8),
            S( 25, -37),   S( -7, -75),   S(-22, -33),   S(  5,   5),   S( 14,  17),   S( 32,  26),   S( 24,  64),   S( 13,  74),
            S( 13, -48),   S(-29, -38),   S( -5, -25),   S( 13,  37),   S( 20,  42),   S( 46,  49),   S( 28, 107),   S( 15,  89),
            S( -9, -56),   S(-15, -78),   S(-12, -53),   S( 15,   4),   S( 46, 100),   S( 95, 109),   S( 56, 261),   S( 71,  98),
            S(  2, -49),   S(  0, -17),   S( -4, -33),   S(  7,  24),   S( 32, 101),   S( 81, 262),   S( 27, 212),   S( 24,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-59,  -1),   S( 31,   4),   S(-34,   0),   S(-35, -11),   S(-45, -31),   S(-32, -41),   S(-48, -65),   S(-60, -31),
            S(-39,  32),   S( 39, -42),   S(-32,  -9),   S(  1, -23),   S( -6, -44),   S(  0, -43),   S(-12, -43),   S(-68, -23),
            S( 20,  45),   S(-12, -28),   S(  1, -20),   S(-20,  20),   S( 14,  21),   S(-30, -12),   S( 16, -59),   S(-28, -73),
            S( 15, -22),   S( 30,   8),   S( 23,  -4),   S( 22,  22),   S( 13,   3),   S(  8, -16),   S( 13, -26),   S( -6, -15),
            S( 25, -50),   S( 43, -32),   S( 26,  -2),   S( 76, -20),   S( 41, -13),   S( 26,  13),   S(  0,  12),   S(-54, -50),
            S( 34, -23),   S( -3,  16),   S( 33,  44),   S( 63, -18),   S( 35, -72),   S( 32,  -4),   S( 15, -35),   S(-52, -16),
            S(  1,  -5),   S( 45, -65),   S( 54, -38),   S(101, -41),   S( 31, -55),   S(-22, -50),   S(  0, -41),   S(  2, -67),
            S(-70, -62),   S(-23,  -9),   S( -1, -25),   S(-26, -72),   S(-26, -40),   S( 14,  18),   S(  8,   6),   S( 18,  -9),

            /* knights: bucket 1 */
            S(-26,  36),   S(-49,  64),   S( 16,  36),   S(-10,  54),   S(-33,  54),   S(-23,  26),   S(-45,  49),   S(-22,   8),
            S(-12,  41),   S(-12,  26),   S(-22,  14),   S(-14,  31),   S(-11,  18),   S( -2,  -4),   S(  9, -33),   S(-41,  10),
            S(-28,   4),   S(-10,  15),   S( -2,   9),   S( 16,  16),   S(  8,  22),   S(-33,  29),   S( -9,  -9),   S(-25,  21),
            S(-11,  33),   S( 42,  26),   S( 28,  31),   S( 16,  29),   S(  5,  28),   S( -4,  21),   S(  5,   5),   S(  5,  21),
            S(-15,  46),   S( -6,  28),   S( 16,  36),   S( 42,  16),   S( 12,  42),   S( 34,  20),   S(  3,   4),   S( -7,  33),
            S(-11,   9),   S( 41,   0),   S( 37,  20),   S( 42,  17),   S( 30,  20),   S( 43,  35),   S( 57, -15),   S(-17,  31),
            S( 54, -16),   S(  6,   8),   S(-35, -16),   S(  9,  39),   S( 30, -29),   S( 73, -29),   S(-43,   3),   S( 27, -24),
            S(-67, -58),   S(-24, -26),   S(  5,  29),   S( -7, -15),   S(-16, -15),   S(-23, -27),   S(-16, -39),   S(-58, -79),

            /* knights: bucket 2 */
            S(-55,   3),   S( -6,  25),   S(-40,  49),   S(-17,  46),   S(-18,  41),   S(-28,  47),   S(-22,  47),   S(-19,   6),
            S(-28,   5),   S(-25,  12),   S( -7,  15),   S(-12,  30),   S( -6,  13),   S( -3,  40),   S(-28,  45),   S(-44,  86),
            S(-25,  23),   S( -9,  29),   S(-17,  41),   S( 17,  26),   S(  1,  29),   S( -5,  10),   S( -5,  40),   S(-28,  36),
            S(-13,  32),   S(-18,  28),   S( -1,  47),   S(  3,  41),   S(  1,  58),   S(-10,  44),   S(-18,  56),   S( -7,  42),
            S( 21,   6),   S(-30,  43),   S(-20,  55),   S(-29,  71),   S(-13,  52),   S(-15,  57),   S(-16,  47),   S(-21,  16),
            S( -6,  19),   S( 16,  27),   S(-40,  71),   S(-12,  30),   S(  0,  51),   S( -8,  46),   S(-28,  16),   S( 46,  -9),
            S(-13,  27),   S(-37,  11),   S( -8,  10),   S(-31,  22),   S(-19,   5),   S( 33,   3),   S(-34,  39),   S(-38,  17),
            S(-137,  13),  S( 15,   6),   S(-100,  33),  S(-10, -17),   S(  6,   1),   S(-54,  -3),   S(  7, -13),   S(-166, -48),

            /* knights: bucket 3 */
            S(-62,   3),   S( -7, -19),   S(-45,  11),   S(-16,  27),   S(-17,  19),   S(-14,  22),   S(  5,   7),   S(-33, -21),
            S(-22,  18),   S(-48,  25),   S(-23,   0),   S(  1,  24),   S(  3,  16),   S(-17,   8),   S(-26,  20),   S(-30,  61),
            S(-21, -18),   S(-12,  19),   S(-13,  22),   S( 11,  23),   S( 11,  29),   S(  4,  17),   S( -4,  11),   S(  2,  40),
            S(-20,  21),   S( -2,  28),   S( -2,  57),   S(  8,  52),   S( 17,  53),   S( 10,  45),   S( 10,  36),   S(  3,  50),
            S(  8,  19),   S(-14,  32),   S( 10,  44),   S(  8,  66),   S(  2,  63),   S(  9,  68),   S( 16,  54),   S(-10,  30),
            S( -9,   3),   S( 19,   2),   S( 43,  14),   S( 48,  22),   S( 67,  -2),   S( 77,   8),   S(  8,  26),   S(-17,  51),
            S(  5,  -4),   S( 24,  10),   S( 67, -35),   S( 26,   0),   S( 41, -18),   S( 84, -41),   S( 37, -52),   S( 51, -26),
            S(-145,  44),  S(  4,   4),   S( -7,  -4),   S(-27,  18),   S( 27,  14),   S(-36,  17),   S( 29, -10),   S(-62, -48),

            /* knights: bucket 4 */
            S( 11,   5),   S(-43,   1),   S( 27,   1),   S(  3, -47),   S(-30, -61),   S(-44, -42),   S(-19, -56),   S(-35, -48),
            S( 51,  27),   S(-30,  -3),   S( 13, -45),   S(-12,  -8),   S( 61, -33),   S(-27, -20),   S( 25, -28),   S(  1, -52),
            S(  2,  38),   S( 12,  40),   S( 19, -11),   S( 46,  -7),   S( 22, -13),   S(-47,  28),   S(-48, -39),   S(-19, -50),
            S(-31,  43),   S( 42, -34),   S( 66,  -4),   S( 55,  -5),   S( 30,  -5),   S( 74,  -6),   S( 33, -49),   S(-34, -53),
            S( 61,  38),   S(-50,  53),   S( 67,  -7),   S( 69,  10),   S( 66,  19),   S(-22,  14),   S( 31, -47),   S(  7, -19),
            S(  9,   0),   S(-13, -31),   S(104,   9),   S( 32, -19),   S( 15,   7),   S( 37,  14),   S( 17,  25),   S(  4,  -6),
            S(  0,  10),   S( -9,  25),   S(  9,  -7),   S( 12,  23),   S( 42,  33),   S(  7, -20),   S(  5, -31),   S(-22, -19),
            S(-29,  -9),   S(  0, -16),   S( 19,  42),   S(  0,   5),   S( -6,  -9),   S( 18,  13),   S(  0,  13),   S( -4, -15),

            /* knights: bucket 5 */
            S( 12,  -1),   S(  4,   8),   S( 19,   9),   S( 43,  35),   S( 25,  22),   S( 13, -28),   S(-27,   1),   S(-21, -31),
            S( 18, -14),   S( 53,  39),   S( 21,  -1),   S( 25,  19),   S( 74,  13),   S( 16,  23),   S( 13,  32),   S(-18, -40),
            S( -9,  16),   S( 16,  36),   S( 41,  21),   S( 79,   8),   S( -8,  27),   S( 20,  21),   S( -7,   1),   S(  5, -11),
            S( 44,  52),   S( 29,  35),   S( 69,  12),   S(  2,  34),   S( 17,  33),   S( 44,  33),   S( 43,  36),   S( 40,  16),
            S( 52,  36),   S( 79,   0),   S( 75,  40),   S( 70,  33),   S( 97,  25),   S( 56,  27),   S( 26,  38),   S( 31,  26),
            S( -5,  36),   S(-15,  43),   S( 43,  14),   S( 26,  57),   S( 37,  50),   S( 28,  64),   S( 48,  -7),   S( -7,   3),
            S( 27,  71),   S(-15,  41),   S( 60,  39),   S( 25,  56),   S(  5,  50),   S( 12,  44),   S( 17,  55),   S(-14,   0),
            S(-26, -18),   S(  2,   9),   S(  6,  54),   S(  0,  -2),   S(  9,  36),   S(  0,  19),   S( 11,  30),   S(-27, -37),

            /* knights: bucket 6 */
            S(-21, -44),   S( 28, -38),   S( 24,  36),   S(-53,  53),   S( -1,  20),   S(-24,  39),   S(  4,  17),   S(-28,  -5),
            S(  3, -49),   S( 76,   6),   S( 41,  -9),   S(-24,  24),   S(-41,  52),   S( 68,  18),   S( 17,  19),   S(  5, -14),
            S(-23, -17),   S( 19,  -8),   S( 33,  15),   S( 60,  11),   S( 12,  35),   S(-30,  43),   S( 30,  35),   S(-23,  50),
            S( 33,  -3),   S( 92,  -2),   S( 82,  14),   S( 90,  12),   S( 63,  21),   S( 49,  32),   S( 45,  46),   S( 15,  48),
            S(-20,  37),   S( 72,  -1),   S( 83,  21),   S( 97,  22),   S(114,  19),   S(125,  24),   S( 34,  36),   S( 42,  36),
            S( 22,  13),   S( 70,  -5),   S( 73,  18),   S( 77,  25),   S( 78,  49),   S( 52,  35),   S( 55,  14),   S( 71,  57),
            S(-28,  10),   S(  6,  10),   S(-13,  25),   S( 18,  12),   S( 10,  20),   S( 24,  51),   S( 31,  65),   S(-11,  34),
            S(-50,  -1),   S( 10,  31),   S( 16,  23),   S( 12,  25),   S( 21,  27),   S( 28,  63),   S( 22,  42),   S( 17,  18),

            /* knights: bucket 7 */
            S(-42, -73),   S(-208, -26),  S(-56, -50),   S(-68, -28),   S(-24, -16),   S(-60, -10),   S(  9,   9),   S(-17, -17),
            S(-57, -72),   S(-16, -69),   S(-17, -36),   S(-17, -26),   S(-45,   4),   S( 35, -22),   S( 36,  11),   S(-19,  27),
            S(-70, -81),   S(-59, -22),   S(-37,  -8),   S( 36, -21),   S( 20, -12),   S( 41, -18),   S( -5,  49),   S( 44,  29),
            S( -8, -33),   S( 12, -25),   S(  6,   3),   S( 72, -10),   S( 92, -14),   S(  2,  17),   S( 32,   3),   S(-14,  36),
            S(-80, -23),   S(-10, -44),   S( 71, -26),   S( 94, -17),   S(120,  -7),   S( 93,  17),   S(107,  -8),   S( 69,  20),
            S(  1, -46),   S(-17, -35),   S(-19,   1),   S( 41,   4),   S( 62,  13),   S(104,   8),   S( 97, -22),   S(  7,  26),
            S(-20, -35),   S(-73, -26),   S( 13, -17),   S( 46,   0),   S( 36,  -1),   S( 60, -10),   S(-19,  14),   S( 21, -11),
            S(-41, -17),   S(-15, -18),   S(-19, -36),   S( 28,  -7),   S( 22,  26),   S( 23,  -8),   S( -4, -37),   S(-18, -11),

            /* knights: bucket 8 */
            S( -1,   2),   S( -8,  -6),   S( -1,  10),   S(  2, -10),   S( -2, -13),   S( -6, -22),   S(  0,   5),   S( -4, -13),
            S(  0,   3),   S(  0, -12),   S(  1,   4),   S(-16, -49),   S(-23, -24),   S(-17, -71),   S(-15, -54),   S(-18, -43),
            S( 17,  36),   S(-11, -35),   S( 13, -21),   S(-12, -16),   S( -1, -14),   S(-12,   5),   S(-22, -53),   S(-12, -55),
            S(-13,  -9),   S( -6,   2),   S( -8,  23),   S(-11,  37),   S( 14,  20),   S( 13,  -9),   S( -8, -29),   S( -4, -18),
            S( 33,  94),   S(  4,   5),   S( 30,  41),   S( 45,  56),   S( 17,  69),   S( -2, -14),   S(  1, -32),   S( -5,  -7),
            S( 12,  60),   S(  4,   5),   S( 32,  16),   S( 45,  23),   S( -6,   1),   S(  2,  23),   S( -7, -44),   S( -7, -11),
            S( 12,  12),   S( 11,  20),   S( -2, -15),   S( 16,  10),   S(  4,  -5),   S( -1,  -7),   S( -4,  -7),   S( -8, -11),
            S(  7,  11),   S( 15,  46),   S(  0,   5),   S(  3,  23),   S( 14,  37),   S(  0,   4),   S(  3,  18),   S(  1,  11),

            /* knights: bucket 9 */
            S( -4, -10),   S(-37, -53),   S(-16, -63),   S(  5,   0),   S(-29, -76),   S(-16, -22),   S( -2,   8),   S( -1,  -8),
            S(-14, -36),   S( -8,   9),   S(  0, -35),   S(-16, -19),   S( -6, -17),   S(-10, -40),   S(  3,  10),   S(-12, -36),
            S(  2,  15),   S( -3, -16),   S(  8, -16),   S(  3,   7),   S(  0,  18),   S(-18,   1),   S(-17,   3),   S( -9, -13),
            S( -3, -14),   S( -7,  -2),   S(  9,  13),   S( 14,  18),   S( 21,   7),   S(  1,  13),   S(-17, -55),   S(  3,  -8),
            S(  9,  28),   S(-14,  -3),   S(  5,   2),   S( 10,   5),   S( 34,   4),   S(  8,   0),   S(  0, -18),   S(  4,   1),
            S(  4,  15),   S( 18,  21),   S( 11,  29),   S(  9,  -8),   S( 44,  30),   S(  7,  29),   S(  8,  16),   S(-16, -43),
            S(  1,  -6),   S(-17,  11),   S( 13,  50),   S(  0,  18),   S( 12,  44),   S( -9, -23),   S(  2,  -1),   S( -5,  -8),
            S(  1,   4),   S(  6,  23),   S( 11,  37),   S( 12,  55),   S( 15,  27),   S( -2,  12),   S(  0,   5),   S(  1,  -2),

            /* knights: bucket 10 */
            S(-10, -37),   S(-13, -43),   S(-14, -51),   S(-23, -44),   S(-11, -22),   S(-16, -55),   S(  3,  16),   S( 10,  29),
            S(-19, -69),   S( -3, -23),   S( -4, -14),   S(-28, -45),   S(-15, -23),   S(-17, -63),   S( -1,   0),   S(-11, -41),
            S(-10, -53),   S(-32, -81),   S( -3, -12),   S( -8, -14),   S( 13,  -1),   S(-16,   5),   S( -4, -25),   S( -4,  -8),
            S(-19, -53),   S(-20, -78),   S(  3, -30),   S( 13,   1),   S( 22,  -2),   S( 22,   1),   S( -5,  -4),   S( 14,  37),
            S(-11, -73),   S(-15, -28),   S( 16,  13),   S( 47,  10),   S( 31,   8),   S(  9,  -5),   S( 26,  22),   S( 20,  55),
            S( -5, -16),   S( -2,  -9),   S(-13, -12),   S(  6,   7),   S(  8,   9),   S( 25,  37),   S( 17,  54),   S( 23,  58),
            S(  3,  10),   S(-13, -37),   S( -5, -17),   S(  9,  26),   S(  3,  35),   S(  0,  15),   S(-12, -33),   S( -3,   0),
            S(  0, -15),   S(  2,   7),   S( -2, -13),   S(  1, -10),   S( -3,  11),   S(  1,  19),   S(  3,  22),   S(  0,   3),

            /* knights: bucket 11 */
            S(  1,   7),   S(-15,  -9),   S( -7, -51),   S(-12, -44),   S(-14, -28),   S( -9, -45),   S(  6,  -2),   S( -3,   9),
            S(-16, -26),   S(-11, -33),   S( -6, -46),   S(-14, -37),   S(-26, -20),   S(-31, -39),   S(-15, -27),   S(-14, -22),
            S(-18, -55),   S(-44, -60),   S(-41, -34),   S( 21,   4),   S(-21,   8),   S(-15,  30),   S( -6, -20),   S(  1,   6),
            S(-29, -56),   S( -7, -29),   S(-12,  -4),   S(  8,  37),   S( 38,  13),   S(  5,  22),   S( -4,   4),   S(  3,  38),
            S( -7, -41),   S(-20, -49),   S( 11, -25),   S( 21,  -1),   S( 31,   2),   S( 58,  26),   S( 12,  -4),   S( 37, 107),
            S(-12,   7),   S(-13, -25),   S(-14,   2),   S( 27,  28),   S( -5,  -4),   S( 45,  29),   S( 19,  43),   S( 14,  51),
            S(  9,  45),   S( -1,  -2),   S(  3, -19),   S(  4,  24),   S(  8,  22),   S( -2,  23),   S( 14,  32),   S( 14,  63),
            S( -4,  -6),   S( -7, -30),   S(  3,   8),   S(  7,  13),   S(  1,  -9),   S(-14, -23),   S(  3,   1),   S(  7,  36),

            /* knights: bucket 12 */
            S( -2,  -9),   S( -1,  -5),   S( -3, -15),   S( -1,   8),   S( -2,  -6),   S( -4,  -7),   S( -3, -19),   S( -1, -11),
            S(  2,   7),   S( -3, -11),   S(  0,   5),   S( -1, -10),   S(  2,  15),   S(  3,  -4),   S( -1, -16),   S(  0,  -5),
            S(  0,  -3),   S(  3,  -3),   S(  0,   8),   S(  2,  18),   S( -8, -13),   S( -5, -17),   S(  0, -13),   S( -8, -33),
            S(-10, -30),   S( -4,  -7),   S( -9, -34),   S(  8,  41),   S( -6, -12),   S( 14,  31),   S( -2,   6),   S(  1,  -8),
            S( 10,  26),   S( -1, -10),   S(-11, -22),   S(  9,  22),   S( -2, -25),   S(  9,  20),   S( -1, -17),   S( -2,   2),
            S(  4,  37),   S(-18, -36),   S( -7,  35),   S(  2,  -8),   S( 13,  11),   S( -4, -19),   S(  2,   0),   S(  1,  15),
            S( -2,   2),   S(-20, -30),   S( -4,   4),   S( -1, -11),   S( -5, -16),   S(  1,  -1),   S(  0,   4),   S( -1,  -3),
            S(  7,  24),   S(  0,  12),   S( -3,  -1),   S(  6,  31),   S( -2,  -8),   S( -4, -16),   S( -1,  -6),   S(  1,   6),

            /* knights: bucket 13 */
            S( -6, -24),   S(  0,   3),   S( -4, -12),   S( -1,  -6),   S( -8, -16),   S( -2, -10),   S(-11, -43),   S( -1,   4),
            S( -1,  -1),   S(  1,   2),   S(  5, -12),   S( -4, -29),   S( -8, -46),   S(  0, -18),   S( -2, -15),   S(  0, -13),
            S( -2,  -3),   S( -8, -22),   S( -4,   8),   S( -4, -32),   S( -9,  -5),   S( -7, -11),   S( -4, -14),   S( -7, -25),
            S(-15, -13),   S(  3,  16),   S(  8,  31),   S( -9, -24),   S(  0,   2),   S(  7,   9),   S( -1, -21),   S( -3, -10),
            S(  6,  18),   S(  4,  34),   S(  3,  -9),   S( 12,  31),   S(  0,  16),   S( -2,  12),   S(  3,   9),   S(  2,   3),
            S(  1,  15),   S( 21,  41),   S( -3,  61),   S(-12,   6),   S(  0,  38),   S(-15, -52),   S(  5,  16),   S( -3, -13),
            S(  7,  18),   S(  3,  27),   S(  4,   7),   S(  5,  36),   S( 14,  52),   S( -2,   4),   S( -2,   3),   S( -2,  -2),
            S(  0,   0),   S(  3,  46),   S(  3,  16),   S( 10,  34),   S( -4,   6),   S(  2,   9),   S(  1,   4),   S(  0,  -1),

            /* knights: bucket 14 */
            S(  0,  -9),   S( -8, -37),   S(  3,  19),   S( -5, -23),   S(-15, -58),   S(  3,   3),   S( -3,  -6),   S(  0,  -6),
            S( -3, -18),   S( -3, -17),   S(-12, -44),   S( -5, -19),   S( -2,  -8),   S(  0,  -6),   S(  2,  11),   S(  2,   7),
            S( -8, -25),   S( -3, -13),   S(-10, -22),   S(  8,  21),   S( -1,  -6),   S( -4,   3),   S(  2,  -1),   S(  2,  11),
            S(  1,   1),   S( -6, -35),   S(-11, -48),   S( -7, -21),   S( -1, -14),   S(  9,  -4),   S( -2,   6),   S( -8, -13),
            S(  1,   3),   S(  0,  -4),   S(  0,  25),   S(  0, -27),   S(-13, -46),   S( 10, -16),   S( 14,  26),   S(  1, -13),
            S( -1,  -3),   S(  7,   6),   S(  0,   8),   S(  6,   0),   S(  2,  10),   S( 15,  20),   S(  2,  23),   S(  2,  11),
            S(  0,  -1),   S( -3, -14),   S( 11,   5),   S( -1,  -1),   S( -7,  16),   S( -9,  19),   S(  2,  26),   S(  1,  13),
            S(  0,   0),   S(  1,  14),   S( -3,  -6),   S(  4,  38),   S( -1,   8),   S(  3,  20),   S(  1,  21),   S(  1,   8),

            /* knights: bucket 15 */
            S( -5, -22),   S( -2, -26),   S( -1, -28),   S(-14, -34),   S( -6,  -9),   S(  0,  -4),   S(  0,   0),   S( -2,  -7),
            S( -3, -23),   S( -1,  -8),   S( -3,  -6),   S(  1,  10),   S(  7,  25),   S( -1,   3),   S( -1,  -3),   S(  2,   7),
            S(  0, -17),   S(  0,  -6),   S( -4, -23),   S(-17, -49),   S(  1,   9),   S(  4,  -8),   S( -4,  -6),   S( -4,  -6),
            S(  0,  -4),   S( -6, -33),   S( -2, -15),   S(  4,  15),   S(  7,  18),   S(  6,  27),   S(  5,   8),   S( -3, -11),
            S( -4, -13),   S( -2,   0),   S(  5,  -9),   S(  0, -25),   S( 14,  17),   S( 10,  -1),   S( -8, -20),   S( -2, -27),
            S( -3, -10),   S( -2,   2),   S( -7, -17),   S(  7,  50),   S(-12, -33),   S( -4,  18),   S( -5,  -8),   S( -2,  13),
            S( -1,  -6),   S( -2,  -2),   S(  2,  12),   S( -9, -27),   S( -8, -19),   S(  1,   5),   S( -2, -23),   S(  4,  21),
            S( -1,  -9),   S( -1,  -1),   S( -3,  -8),   S(  4,  -1),   S( -3,  -8),   S(  0, -15),   S(  8,  37),   S( -2,  -4),

            /* bishops: bucket 0 */
            S( 45,  42),   S(-11,  15),   S( 44,  23),   S( 15,  17),   S( -3,  22),   S(  7,  -6),   S(  3, -12),   S(-14, -31),
            S( 46, -34),   S( 87,   5),   S( 26,   9),   S( 15,  10),   S(  0,  25),   S( -4,  20),   S(-31,   0),   S(  1, -33),
            S( 48,  -3),   S( 41,  26),   S( 15,  40),   S( -7,  63),   S( 13,  29),   S(-29,  59),   S(  0,  19),   S( 10, -36),
            S( 23,  -2),   S( 64,   5),   S( 21,  27),   S( 42,  19),   S(  7,  34),   S( 27,  21),   S( -9,  21),   S(  1,  16),
            S( 12,   8),   S( 49, -12),   S(  6,  52),   S( 39,  27),   S( 32,  38),   S(-24,  32),   S( 15,  26),   S(-39,  31),
            S(-32,  57),   S(-14,  47),   S( 52,  29),   S( 37,  39),   S( 20,  68),   S(-10,   9),   S(-50,  53),   S(  1,  52),
            S(-71,  68),   S(-13,  37),   S( 16,  53),   S(-23,  65),   S(-73,  41),   S( 21,  23),   S( 35,   4),   S(-46,   9),
            S(-43, -19),   S( 14,  50),   S(-17,  45),   S(-11,  30),   S(  4,  28),   S( 26,  35),   S( -9,  61),   S(-37,  57),

            /* bishops: bucket 1 */
            S( 52,  55),   S(-26,  34),   S( 18,  23),   S( -4,  31),   S( 15,  16),   S(  2,  40),   S(  8,  44),   S( -6, -10),
            S( 20,  16),   S( 46,  23),   S( 49,  12),   S( 32,  24),   S(  2,  26),   S( 23,   4),   S(-21,  26),   S( 10,  29),
            S( 18,  27),   S(  5,  47),   S( 39,  43),   S(  6,  48),   S( 20,  37),   S( -8,  41),   S( 27,  17),   S(  4, -24),
            S( 67,  -3),   S( 47,  38),   S(  2,  40),   S( 30,  44),   S(-20,  57),   S( 21,  25),   S(-22,  59),   S( 16,  15),
            S( 31,  48),   S(  8,  49),   S(  6,  36),   S( -2,  44),   S( 14,  39),   S(-23,  49),   S( 24,  11),   S(-22,  36),
            S( 11,  28),   S( 32,  45),   S( 12,  55),   S( 65,  27),   S( 10,  58),   S( -3,  59),   S( -5,  49),   S( 24,  28),
            S(-14,  68),   S( 22,  43),   S( 40,  34),   S( 38,  56),   S(-14,  53),   S( -2,  59),   S(-19,  65),   S(-26,  51),
            S( 34,  86),   S(-28,  48),   S(-25,  29),   S(-24,  36),   S( -4,  49),   S(  1,  30),   S(-20,  64),   S(-31, 112),

            /* bishops: bucket 2 */
            S( 32,  28),   S( -8,  50),   S( -2,  41),   S(-21,  55),   S(-10,  44),   S(-15,  45),   S(  5, -11),   S(-56,  84),
            S( -6,  35),   S( 19,  29),   S( 11,  37),   S(  0,  36),   S(  1,  44),   S(  4,  20),   S(  9,  14),   S( -1, -11),
            S(  3,  29),   S( -1,  46),   S( 12,  55),   S( -9,  68),   S(  3,  49),   S(  2,  53),   S(  3,  44),   S(-12,  12),
            S( 18,  45),   S(-13,  61),   S(-16,  68),   S(  1,  62),   S(  7,  55),   S(  0,  47),   S(  1,  47),   S( -5,  22),
            S(-23,  59),   S(-15,  55),   S(-31,  60),   S(-15,  61),   S(-27,  70),   S( -4,  76),   S( -4,  50),   S(-16,  27),
            S(-27,  58),   S(-22,  41),   S( -4,  69),   S(-18,  65),   S( 20,  45),   S( -9,  67),   S( -7,  70),   S(  0,  49),
            S(-38,  47),   S(-29,  71),   S(-31,  69),   S( -8,  46),   S(-32,  72),   S(-26,  54),   S(-59,  66),   S(-65,  62),
            S(-94, 122),   S(-123, 103),  S(-97,  77),   S(-88,  70),   S(-69,  56),   S(-62,  69),   S( 34,  40),   S(-64,  45),

            /* bishops: bucket 3 */
            S( -6,  34),   S(  6,  32),   S( 10,  34),   S( -3,  40),   S( -3,  32),   S( 39,   3),   S( 17,  -8),   S( 34, -16),
            S(  6,  34),   S(  9,  40),   S( 16,  19),   S(  2,  46),   S( 15,  32),   S(  1,  37),   S( 49,  28),   S( 21, -10),
            S( 21,  10),   S(  1,  45),   S(  5,  63),   S( 12,  46),   S(  8,  65),   S( 11,  59),   S( 23,  42),   S( 30,   4),
            S( 25,  22),   S(  6,  57),   S( -3,  73),   S(  9,  69),   S( 20,  60),   S( 13,  41),   S(  4,  60),   S(  7,  10),
            S( -9,  39),   S( 14,  41),   S(  2,  49),   S( 26,  64),   S( 10,  56),   S( 25,  48),   S( -4,  51),   S( 13,  51),
            S(  5,  52),   S( 13,  41),   S( 11,  61),   S( 21,  42),   S(  7,  57),   S( 35,  60),   S( 21,  46),   S(  0,  63),
            S( -1,  32),   S( -5,  47),   S( 28,  33),   S( 10,  49),   S( -5,  45),   S( -7,  48),   S( -9,  50),   S(  4,  60),
            S(-63, 114),   S(-24,  58),   S( 32,  48),   S(-22,  77),   S(-34,  67),   S(-64,  81),   S(-22,  38),   S( 67,  19),

            /* bishops: bucket 4 */
            S(-14, -13),   S(-13,  20),   S(-62,  15),   S(-43,  25),   S(-37,  29),   S(-73,  13),   S(-17,   6),   S(  0, -13),
            S(-16,  29),   S( 49, -13),   S( 20,  16),   S(  3, -15),   S(-22,  15),   S( 85, -23),   S(-13,  25),   S( 34,  21),
            S(-27,  -9),   S(-60,  28),   S(  6,   2),   S(-35,  28),   S(  6,  11),   S( 52, -12),   S(  6, -38),   S(-54,   0),
            S(-15,  14),   S(-20,  32),   S( 59,  15),   S( 31,  28),   S(-37,  53),   S( 37,  -1),   S( 19,  45),   S(  1,  -3),
            S(  1,  -6),   S( 14,  29),   S(-32,  50),   S( 30,  29),   S( -6,  35),   S(-42,  30),   S(-29,  14),   S(-11,  25),
            S(  8,  27),   S( 29,  19),   S(  1,  32),   S( 22,  20),   S(  7,  23),   S( 12,  21),   S(  1, -18),   S(-15,   7),
            S(-32,  34),   S( 42,  33),   S( -7,  25),   S( 31,  67),   S( -5,  14),   S(  2,  23),   S(  0,  12),   S(  0, -30),
            S( 19,  26),   S(-35, -10),   S(  5,  27),   S( -6,  21),   S(  1,  46),   S( -7, -22),   S( -6,  -8),   S(-10,  24),

            /* bishops: bucket 5 */
            S(-36,  43),   S(-51,  57),   S(-36,  39),   S(-31,  42),   S(-23,  26),   S( -6,  14),   S(-23,  47),   S( -9,  45),
            S(  3,  23),   S(-44,  55),   S(-30,  58),   S( 12,  18),   S(  3,  37),   S(-13,  30),   S(-21,  21),   S(-38,  35),
            S(-59,  54),   S(-37,  65),   S( 15,  36),   S( 22,  28),   S( 12,  32),   S(-14,  38),   S(-38,  52),   S( 15,   0),
            S( 20,  45),   S( 32,  48),   S( -5,  48),   S( 23,  30),   S( 39,  34),   S( 46,  26),   S(  8,  27),   S( -1,  44),
            S( 53,  44),   S( 31,  18),   S( 71,  27),   S( 80,  35),   S( 28,  27),   S( 35,  26),   S( 81,  23),   S(-10,  29),
            S( -4,  42),   S( 19,  48),   S(106,  31),   S( 35,  53),   S(-29,  56),   S( -1,  11),   S(  9,  32),   S( 15,  45),
            S( 12,  49),   S(-51,  50),   S(  1,  32),   S( 23,  36),   S( -4,  50),   S(-16,  63),   S(  5,  36),   S(  5,  28),
            S( -3,  36),   S( 22,  33),   S(-17,  25),   S( -4,  51),   S(  1,  48),   S( 11,  59),   S( 11,  87),   S( -3,  14),

            /* bishops: bucket 6 */
            S(-61,  75),   S( 26,  37),   S(-14,  48),   S(-45,  44),   S(-44,  32),   S(-47,  45),   S( 15,  50),   S(-16,  15),
            S(  9,  26),   S(-20,  33),   S( -8,  47),   S(-10,  53),   S(  6,  35),   S(-13,  24),   S(-115,  71),  S( 29,  30),
            S(  8,  18),   S(-26,  47),   S( 41,  43),   S( 36,  35),   S( 77,   7),   S( 79,  13),   S( -2,  53),   S(-50,  41),
            S( 15,  49),   S(-37,  60),   S( 14,  49),   S( 40,  39),   S( 20,  51),   S( 37,  32),   S(  6,  57),   S(-17,  19),
            S(-16,  54),   S( 37,  35),   S( 55,  23),   S( 30,  43),   S(127,  30),   S( 92,  29),   S( 56,  30),   S(-23,  43),
            S( -2,  33),   S(-21,  43),   S( 13,  46),   S( 42,  45),   S( 21,  51),   S( 75,  39),   S( 31,  49),   S(-15,  70),
            S(-18,  17),   S(-22,  47),   S( 18,  26),   S( -6,  45),   S( 55,  43),   S( 10,  60),   S( 13,  62),   S(-24,  70),
            S( -1,  74),   S(  4,  63),   S(  9,  49),   S( -6,  52),   S( -1,  42),   S(  3,  41),   S( -6,  49),   S( 16,  46),

            /* bishops: bucket 7 */
            S(-20,  11),   S(-38,  33),   S(-47,   3),   S(-41,  15),   S(-73,  28),   S(-76,  18),   S(-83, -21),   S(-40,  19),
            S(-48,   0),   S(-75,  29),   S(-21,  14),   S( 10,   9),   S(-14,  10),   S(-61,  37),   S(-42,  -8),   S(-25, -25),
            S(-35,  21),   S( 20, -16),   S(  6,  30),   S( 46,  -1),   S(-44,  37),   S(-13,  16),   S(-27,  31),   S(-30,   5),
            S(-86,  44),   S( 23,  31),   S( 97, -14),   S( 79,   7),   S( 85,  16),   S(-13,  38),   S( 33,  35),   S(  1,  22),
            S(-28,  12),   S(-43,   7),   S( 52,  -6),   S(102,  -9),   S( 61,  21),   S( 70,  24),   S( 14,  27),   S( 31,   8),
            S( 13, -13),   S(-21,  16),   S( 46,  -7),   S( 34,  14),   S( 41,   8),   S( 79,  17),   S( 56,  22),   S(  0,  25),
            S(  3,   3),   S( -9,   7),   S(-12,  18),   S(-24,  13),   S( 17,   2),   S( 25,  15),   S( 38,  22),   S( 14,  49),
            S( -9,  31),   S(-30,  27),   S(-39,  27),   S( -6,  16),   S(  4,  13),   S( 17,  11),   S( 24,  30),   S( 12,  33),

            /* bishops: bucket 8 */
            S( -1, -26),   S(-15, -78),   S(-63, -65),   S(  4, -19),   S(  5, -17),   S(-29, -34),   S(  5, -14),   S( -5,  -1),
            S(  3, -16),   S(-34, -89),   S(-11, -44),   S(  2, -11),   S( 11, -19),   S(-14, -43),   S( -2, -41),   S(  2, -36),
            S(-10, -31),   S(-12,  -5),   S(  2,   8),   S( 31, -28),   S(  3, -18),   S(  1, -31),   S(  0, -25),   S(-42, -34),
            S( 11,  55),   S( 11, -13),   S(  9,   7),   S( 11,  -6),   S( 30,  -4),   S(  0, -16),   S(  0, -31),   S( -1, -45),
            S( -8,  18),   S( -1,  18),   S( 19, -15),   S( 32,   0),   S( 16, -41),   S( 13,  -5),   S(  4, -26),   S( -1, -33),
            S( -3,  -9),   S( 12,  -2),   S( 10,  11),   S(-20, -40),   S(  1, -31),   S(  8, -26),   S( -1, -27),   S(-17, -46),
            S( -1,   0),   S( 16,   8),   S( 19,  10),   S( -1, -33),   S( -7, -15),   S(  0, -35),   S( -7, -46),   S(-17, -49),
            S( -5, -23),   S( -1, -57),   S(  4, -13),   S( -2, -19),   S(-13, -39),   S( -3, -31),   S( -6, -33),   S(  1, -12),

            /* bishops: bucket 9 */
            S(-18, -38),   S( 20, -26),   S( -5,   5),   S(-17, -43),   S(-30, -60),   S(-20, -46),   S( -8, -29),   S(  6,  18),
            S( -5, -22),   S(-13, -33),   S(-15, -11),   S( 19, -12),   S( -3, -12),   S(-12, -37),   S( -9, -15),   S(  6,  23),
            S( 11, -13),   S( 11, -13),   S( 18, -17),   S( 33, -31),   S( 24, -14),   S( 47, -22),   S(-11, -47),   S( -1, -35),
            S(-17, -28),   S( 20,  21),   S(  1,  -9),   S( 41, -10),   S( 47, -12),   S( 28, -16),   S( 26, -30),   S(-18, -50),
            S(  0,  -5),   S( 31, -11),   S( 28,  -9),   S( 40,  -6),   S( 43, -29),   S( 35, -16),   S( 25,  -8),   S(-13,  -5),
            S( -3, -42),   S( 53,  19),   S( -4,  28),   S(  9,  -1),   S( 37, -29),   S( 30, -39),   S(  3, -64),   S(-15, -46),
            S( 16,  31),   S( 18, -13),   S( -5, -27),   S( 16,  -4),   S(  7, -19),   S(  8, -37),   S(  2, -32),   S( -1, -29),
            S( -8, -36),   S(  1,  11),   S( -1, -45),   S( -8, -40),   S( -4, -38),   S(  4, -16),   S(  1, -12),   S( -6, -25),

            /* bishops: bucket 10 */
            S(-16, -38),   S( 15, -35),   S(-24, -27),   S(-13, -27),   S(-11, -32),   S(-27, -44),   S( -6, -47),   S(-17, -63),
            S( 16,  -9),   S(-23, -40),   S( 18, -19),   S(-11, -52),   S(  2, -21),   S(  8, -35),   S(-20, -74),   S(  0,   2),
            S(-15, -42),   S( 24, -38),   S(  0, -50),   S( 25, -35),   S( 42, -46),   S( -7, -13),   S(-16,  -1),   S( 14,   9),
            S(-10, -61),   S( 22, -33),   S( 25, -29),   S( 57, -25),   S( 69, -35),   S( 19, -10),   S( -9,  -6),   S( 21,  20),
            S( -3, -36),   S( 28, -39),   S( 50, -40),   S( 82, -21),   S( 57,  -9),   S( 43, -26),   S( 10,  11),   S(  9, -35),
            S(  2, -34),   S( 17, -49),   S( 18, -46),   S( 23, -54),   S( 44, -33),   S( 44,  23),   S( 15, -18),   S( -2, -33),
            S(-18, -73),   S(  4, -56),   S(  3, -53),   S( 24, -24),   S(-11, -55),   S(  9, -26),   S( 21,  41),   S( 15,  21),
            S( -1, -39),   S( -1, -22),   S(  7,  -6),   S(  0, -47),   S( -6, -28),   S( -6, -47),   S(  2, -25),   S(  8,  27),

            /* bishops: bucket 11 */
            S( -7,  20),   S(-36, -10),   S(-39, -47),   S(-22, -10),   S(-15, -13),   S(-44, -70),   S( -5, -26),   S(-27, -72),
            S( -2, -49),   S(  1, -28),   S(-15, -14),   S(-27, -24),   S(-24, -45),   S(  0, -44),   S( -2, -20),   S(-29, -69),
            S(  1, -54),   S(  2, -60),   S( 15, -22),   S( 51, -58),   S(-17,  -8),   S( 22, -23),   S( -6, -11),   S( -3,  -2),
            S(  0, -20),   S( -6, -37),   S( 25, -42),   S( 26, -42),   S( 71, -36),   S( 33,  -9),   S( 31, -15),   S( 13,  71),
            S( -2, -20),   S(-18, -63),   S( 16, -28),   S( 65, -29),   S( 55, -13),   S( 34,  -2),   S( 30,  43),   S(  7,   3),
            S(-19, -79),   S( -2, -68),   S(  9, -64),   S( 38, -20),   S( 32, -25),   S( 34,  11),   S(  2,  14),   S(-27, -20),
            S( -3, -26),   S(  5, -36),   S( -4, -36),   S(  2, -40),   S(  5, -10),   S( 23, -40),   S(  8, -27),   S(  9,   4),
            S(-15, -69),   S(-11, -40),   S(  0, -50),   S( 23, -14),   S( 21, -22),   S(-14, -66),   S( -5, -25),   S(  3,   4),

            /* bishops: bucket 12 */
            S(  3,  -1),   S(-13, -41),   S(-10, -52),   S(-13, -59),   S( -6, -15),   S( -1, -17),   S( -2,  -4),   S(-12, -16),
            S( -4, -29),   S( -6, -33),   S(  3, -13),   S(-12, -33),   S( -6, -30),   S(  0,   2),   S(  1,  -2),   S(  0, -17),
            S( -3, -19),   S(-18, -51),   S( -7, -23),   S(  4, -26),   S( 11,   4),   S( -5, -37),   S(-11, -51),   S(  8,  15),
            S( -7, -25),   S(  4, -16),   S(-15, -53),   S(  2, -12),   S(  6,  11),   S(  8,  10),   S( -3, -31),   S( -6, -16),
            S(-15, -20),   S(  0,  -5),   S( -3, -53),   S( -8,  -2),   S(  9, -18),   S(  0,  -3),   S(  2, -30),   S( -3,  -1),
            S(-24, -49),   S( 10,  46),   S( -4,   8),   S( -2, -17),   S(  7, -12),   S( -6, -30),   S( -1, -38),   S(  1,  -6),
            S( -4, -16),   S(  5,  18),   S( -7,   3),   S(-10, -39),   S( -6, -20),   S( 14,  26),   S( -7, -21),   S( -6, -22),
            S(  1,  -3),   S( -2,   2),   S(  2, -13),   S( -4, -28),   S(  4, -14),   S(  3,   2),   S(-10, -27),   S( -2,  -6),

            /* bishops: bucket 13 */
            S( -6, -40),   S( -8, -69),   S( -9, -41),   S( -8, -46),   S(-16, -72),   S(-14, -38),   S(-15, -37),   S( -2, -10),
            S(  1,  -9),   S(  3, -26),   S( -9, -84),   S(-11, -42),   S( -6, -34),   S(  2, -25),   S( -2, -22),   S( -1, -28),
            S(-10, -52),   S( -9, -26),   S( -1, -44),   S( -1, -66),   S( -2, -67),   S( 10, -48),   S(  1, -12),   S(  9,  33),
            S( -4, -13),   S( -9, -42),   S(  6, -28),   S( -5, -48),   S( 20, -45),   S(  2,  -7),   S( -2, -11),   S( -7, -45),
            S( -1, -14),   S( -1,  10),   S(  5, -53),   S( 21, -19),   S(  0,  -9),   S( 11,  -7),   S( -1, -45),   S(  0, -19),
            S( -5, -13),   S(  0,  -4),   S( -8,   0),   S( 11,  -1),   S( 10,  -8),   S(  9, -36),   S( 14, -22),   S( -7, -34),
            S( -6, -29),   S(-10, -40),   S(  5,  12),   S( -6,  22),   S( -7, -36),   S(  1,  -2),   S( -4, -48),   S(  2,  -8),
            S( -7, -34),   S(  1,  -7),   S( -3, -24),   S(  8,   8),   S(  3,  -5),   S(-12, -64),   S( -8, -26),   S( -5, -36),

            /* bishops: bucket 14 */
            S( -5, -46),   S(-13, -78),   S(-16, -66),   S(-13, -62),   S(-11, -73),   S( -8, -41),   S(-12, -79),   S(-10, -40),
            S( -6, -21),   S( -8, -37),   S( -1, -34),   S(-18, -57),   S( -4, -40),   S( -3, -56),   S(-12, -50),   S( -1, -23),
            S( -8, -25),   S( -2, -14),   S(-14, -59),   S( 11, -32),   S(-15, -84),   S(-15, -88),   S( -6, -39),   S(  4,   0),
            S( -8, -32),   S(-12, -42),   S( -9, -39),   S(  6, -36),   S( 17, -50),   S(  2, -49),   S( -6, -57),   S( -5,  -5),
            S( -2, -27),   S(  6, -14),   S(  0, -48),   S(  3, -33),   S(  0, -59),   S( -2, -46),   S(  4, -21),   S(  0,  -3),
            S(  2, -18),   S( -8, -61),   S( -7, -47),   S( -3, -25),   S(  8,   1),   S(  7,  38),   S( 14,   1),   S( -8, -27),
            S( -7, -55),   S(  9, -35),   S( -5, -33),   S(  0, -13),   S( -8, -31),   S( -5, -28),   S( -4, -13),   S( -2, -20),
            S( -3, -27),   S(  3,   1),   S( -7, -36),   S(-13, -48),   S( -7, -33),   S(  0,  -3),   S(  6,  22),   S(  1,  -9),

            /* bishops: bucket 15 */
            S(  5,  14),   S(  5,  24),   S(-16, -72),   S( -8, -23),   S( -7, -27),   S(-10, -27),   S( -4, -18),   S( -2, -18),
            S(  3,   2),   S(  7,  28),   S( 16,  26),   S(  2, -37),   S(-13, -41),   S( -9, -43),   S( -1,  -3),   S( -1,  -2),
            S( -7, -42),   S(  4, -10),   S( -1, -28),   S( -1,   4),   S( -3, -41),   S( -3, -30),   S(-13, -38),   S(  6,   1),
            S( -7, -33),   S(-14, -60),   S(  2, -33),   S(-15, -77),   S( -6, -13),   S( -6, -38),   S(  5,   2),   S( -6, -34),
            S( -2, -22),   S( -8, -23),   S( -1, -37),   S(-14, -23),   S(  0, -11),   S( -7, -17),   S( 10, -30),   S(-14, -31),
            S( -5, -20),   S( -7, -48),   S(-16, -62),   S(-11, -81),   S(  0, -37),   S( -4,   9),   S( 12, -20),   S(  2,  19),
            S(  0, -18),   S( -4, -18),   S(  1,  -8),   S( -3, -38),   S(-10, -46),   S( -7, -17),   S( -1,   4),   S(  3,   3),
            S( -1, -13),   S( -2,  -9),   S( -2, -24),   S( -7, -54),   S( -8, -45),   S(-13, -44),   S(-14, -39),   S(  2,   7),

            /* rooks: bucket 0 */
            S(-24,  -6),   S(  4,  -8),   S( 12, -32),   S( 13,  -8),   S( 21,  -8),   S( 21, -15),   S( 10,   2),   S( 19,  -6),
            S( 46, -75),   S( 28, -23),   S( 17,   1),   S( 14,  -2),   S( 32,  -7),   S( 29, -26),   S( -5,   5),   S(-43,  27),
            S(-20,  17),   S( 18,   7),   S( 31,   5),   S( 16,   0),   S(  7,  22),   S( 24, -14),   S(-15,  14),   S(-39,   5),
            S( 41, -22),   S( 69, -12),   S( 45,  24),   S( 52,   6),   S( 26,  -2),   S(-12,  12),   S(  8,  16),   S( -4,  -4),
            S( 39, -23),   S( 95, -15),   S( 57,  -1),   S( 77, -14),   S( 56,   2),   S( 33,  22),   S( 29,  15),   S(-26,  43),
            S( 71, -47),   S(106, -49),   S( 78,  -2),   S( 34,  10),   S( 58,  21),   S(-11,  25),   S( 73, -14),   S(-16,  31),
            S( 68, -22),   S( 72,  -1),   S( 14,  19),   S( 42,  34),   S( 13,  20),   S( 27,  14),   S( 35,   6),   S( 30,   7),
            S( 59,  11),   S( 20,  41),   S( 12,  32),   S( 40,  20),   S( 27,  -2),   S( 32, -11),   S(  8,  22),   S( 43,   3),

            /* rooks: bucket 1 */
            S(-84,  29),   S(-43,  16),   S(-44,  -4),   S(-41,  -1),   S( -9, -27),   S(-10, -20),   S(-25, -13),   S(-29,  13),
            S(-47,  14),   S(-53,  14),   S(-57,  32),   S(-26, -25),   S(-19,  -6),   S(-44,  -5),   S(-34, -27),   S(-47,  -4),
            S( 10,   0),   S( -9,  22),   S(-37,  23),   S(-46,  32),   S(-25,   8),   S(-17,   6),   S(-16,   4),   S(-35,  13),
            S( -9,  41),   S(-53,  49),   S( 12,  21),   S( -6,  21),   S(-35,  48),   S(-28,  41),   S(-19,  30),   S(-27,  21),
            S( 57,  14),   S( 18,  40),   S(  5,  39),   S(-41,  57),   S( 15,  21),   S( 41,   1),   S( 10,  26),   S(-27,  11),
            S( 65,  22),   S(-32,  61),   S( 33,  30),   S( 13,   5),   S( 50,  14),   S(-26,  45),   S(  6,  25),   S(-40,  37),
            S(-54,  62),   S(  4,  53),   S( 12,  36),   S(-75, 104),   S(-26,  40),   S( 21,  34),   S(-78,  58),   S(-22,  26),
            S( 28,  32),   S( 21,  33),   S(  4,  32),   S(-56,  68),   S( -1,  28),   S( 52,   6),   S( 21,  35),   S( 33,   3),

            /* rooks: bucket 2 */
            S(-67,  42),   S(-40,  28),   S(-34,  23),   S(-56,  32),   S(-50,   6),   S(-54,  24),   S(-42,  -4),   S(-53,  32),
            S(-82,  51),   S(-63,  40),   S(-48,  38),   S(-49,  23),   S(-41,  17),   S(-75,  37),   S(-77,  31),   S(-59,   8),
            S(-64,  60),   S(-61,  62),   S(-49,  64),   S(-50,  39),   S(-48,  47),   S(-41,  42),   S(-18,   9),   S(-38,  25),
            S(-71,  76),   S(-32,  61),   S(-31,  72),   S(-29,  62),   S(-37,  64),   S( -3,  47),   S(-38,  59),   S(-12,  33),
            S(-29,  62),   S(-47,  77),   S(-35,  67),   S(-17,  54),   S( 30,  37),   S(-31,  69),   S(-16,  50),   S(-41,  59),
            S(-14,  52),   S(-40,  69),   S(  4,  48),   S( 20,  39),   S( 24,  46),   S( 80,  33),   S( 10,  37),   S(-35,  53),
            S(-30,  55),   S(-66,  85),   S( -4,  67),   S(  9,  65),   S( 50,  35),   S( 58,  17),   S(-40,  75),   S(-24,  58),
            S(-23,  69),   S(  1,  51),   S(-18,  48),   S(-24,  60),   S(  0,  57),   S( 16,  38),   S(-17,  61),   S(  3,  39),

            /* rooks: bucket 3 */
            S( -3,  76),   S(  3,  74),   S( 16,  66),   S( 17,  57),   S( 11,  54),   S(-11,  64),   S( -5,  69),   S(-14,  54),
            S(-37,  88),   S(-14,  66),   S( -1,  69),   S( 12,  61),   S( 22,  58),   S(  4,  63),   S( 37,   8),   S( 16, -21),
            S(-37,  82),   S(-14,  87),   S(  2,  81),   S(  6,  66),   S( 16,  76),   S( 14,  65),   S( 21,  61),   S( -6,  63),
            S(-19,  88),   S(-27,  96),   S( 21,  85),   S( 38,  64),   S( 27,  80),   S( -3, 100),   S( 52,  66),   S(  8,  78),
            S(-13, 102),   S( 19,  90),   S( 19,  81),   S( 48,  68),   S( 43,  78),   S( 46,  65),   S( 89,  50),   S( 52,  52),
            S(  4,  96),   S( 18,  80),   S( 40,  73),   S( 39,  71),   S( 43,  56),   S( 59,  51),   S( 85,  32),   S( 77,  32),
            S(-15, 103),   S( -8, 109),   S( 17,  95),   S( 51,  83),   S( 26,  82),   S( 64,  71),   S( 60,  73),   S(135,  15),
            S(-55, 146),   S( 32,  87),   S( 14,  90),   S( 66,  62),   S( 71,  59),   S( 86,  48),   S(131,  32),   S(132,  21),

            /* rooks: bucket 4 */
            S(-108,  28),  S(-47,  15),   S(-54,  -5),   S(  9,  -4),   S(-26,  -8),   S( -5, -53),   S(  1, -23),   S( -2, -47),
            S(-30, -23),   S(-39,   3),   S(-31,  -2),   S(-34,  18),   S(-11, -30),   S( -4, -28),   S(-14, -29),   S(-65, -12),
            S(-25,  25),   S(-27, -26),   S(-16,   7),   S(-12, -25),   S(-21,   3),   S( 14, -32),   S( 13, -23),   S(-89,   8),
            S(-59, -14),   S( 26, -15),   S(-30,  32),   S( 25, -13),   S( 43,  -3),   S(  7, -14),   S(-13,   6),   S(-12,   9),
            S(-42,   9),   S(-30,  47),   S(  8, -10),   S( 51,  26),   S( 43,  -7),   S(  1,   6),   S( 37,  18),   S( 23,   5),
            S( 13, -19),   S(  7,   1),   S( 42,  24),   S( 40,  16),   S( 35,   1),   S( 22,  22),   S( -7,  15),   S( 17,  24),
            S( -5,  -1),   S( 32,  25),   S( 20,  13),   S( 25,  17),   S( 62,  -9),   S( 23,  -4),   S( 46,  14),   S( 29,  18),
            S( 11, -59),   S( 36,  31),   S( 36,  -1),   S(  9, -10),   S( 22,  -2),   S( 21,  10),   S( 16,   0),   S(  8,   3),

            /* rooks: bucket 5 */
            S(-44,  34),   S(-35,  50),   S(-63,  53),   S(-27,   2),   S(-17,   9),   S(-20,  22),   S( 21,   6),   S(-32,  39),
            S(-11,  11),   S(-27,  25),   S(-61,  63),   S(-59,  30),   S(-52,  38),   S(  5,  12),   S(  4,  12),   S(-27,  11),
            S(-22,  39),   S(-46,  55),   S(-76,  69),   S(-82,  62),   S(-34,  23),   S(-32,  39),   S(-26,  39),   S( -9,  15),
            S(-67,  75),   S(  3,  34),   S( -5,  53),   S(-28,  58),   S(-37,  66),   S(-10,  61),   S( -9,  38),   S( -2,  20),
            S( 30,  56),   S(-16,  68),   S( 45,  45),   S( 34,  70),   S( 47,  42),   S( 23,  63),   S( 59,  45),   S( 40,  28),
            S( 76,  58),   S( 47,  62),   S( 92,  51),   S( 59,  57),   S( 67,  43),   S( 90,  39),   S( 48,  55),   S( 54,  41),
            S( 26,  56),   S( 35,  50),   S( 45,  42),   S( 43,  52),   S( 34,  45),   S( 70,  56),   S( 88,  36),   S( 72,  36),
            S( 78,  29),   S( 47,  26),   S( 51,  50),   S( 40,  33),   S( 35,  40),   S( 67,  28),   S( 66,  26),   S( 25,  54),

            /* rooks: bucket 6 */
            S(-68,  37),   S(-23,  30),   S( -4,  21),   S(-10,  24),   S(-42,  20),   S(-76,  50),   S(-54,  59),   S(-28,  57),
            S(-47,  37),   S(-30,  33),   S( -6,  28),   S(-57,  40),   S(-54,  51),   S(-44,  48),   S(-75,  58),   S( 30,  17),
            S(-72,  65),   S(-66,  56),   S(  6,  39),   S(-55,  53),   S(-25,  42),   S(-81,  69),   S(-50,  68),   S( 43,  -2),
            S(-62,  76),   S(-15,  59),   S(-14,  68),   S(-33,  65),   S(-23,  59),   S(  3,  58),   S(-90,  84),   S(-31,  58),
            S( -7,  73),   S( 19,  69),   S( 68,  44),   S( 27,  54),   S(-13,  73),   S( 39,  58),   S(  4,  61),   S( 37,  41),
            S( 27,  60),   S( 81,  50),   S(106,  38),   S( 14,  57),   S( 54,  55),   S( 49,  63),   S( 71,  49),   S( 91,  39),
            S( 64,  55),   S( 89,  40),   S(121,  32),   S( 75,  42),   S(111,  30),   S( 55,  59),   S( 80,  37),   S( 39,  62),
            S( 87,  52),   S( 51,  50),   S( 31,  55),   S( 71,  38),   S( 66,  53),   S( 63,  45),   S( 83,  36),   S( 34,  46),

            /* rooks: bucket 7 */
            S(-70,   5),   S(-38,   6),   S(-17,  -9),   S( -5, -10),   S( -6,  -9),   S(-16,  10),   S(-29,  18),   S(  3, -12),
            S(-71,  24),   S( -6,  -3),   S(-15,  -6),   S( -8,   4),   S( 10,   5),   S(  9,   7),   S(-27,   8),   S(-39,   7),
            S(-86,  48),   S(-54,  25),   S(  1,  22),   S( -5,  -5),   S(  0,  17),   S(-36,  13),   S( 11, -24),   S(  9,   5),
            S(-65,  42),   S(-30,  32),   S(  6,  24),   S( 77,  -7),   S( 40,   6),   S( 11,  16),   S( 42,   5),   S(-36,   9),
            S(-20,  41),   S( 41,  11),   S( 60,  -1),   S( 40,   1),   S( 88,   6),   S(113,   0),   S( 33,  39),   S( 45,  -7),
            S( -6,  41),   S( 28,  16),   S(100, -15),   S(121, -12),   S( 68,   1),   S( 63,  21),   S( 78,  13),   S( 46,  -7),
            S( 28,  30),   S( 41,  18),   S( 77,   7),   S( 90,   6),   S(120,   2),   S(112,  14),   S( 34,  32),   S( 52,  -7),
            S( 21,  54),   S(  2,  36),   S( 66,   6),   S(114, -25),   S( 44,  14),   S(  3,  21),   S( 60,   5),   S( 65, -10),

            /* rooks: bucket 8 */
            S(-35, -34),   S( -8,  -4),   S(  1,   6),   S(-40,  -8),   S(-46, -49),   S(-27, -67),   S(-10, -18),   S(-24, -14),
            S(-13, -10),   S(-19, -24),   S(-16,  -7),   S(-10, -29),   S(-28, -20),   S( -8, -51),   S(-18, -39),   S(-22, -75),
            S(  2, -10),   S(  5,   2),   S( -4, -25),   S(-27,  -8),   S(-31, -25),   S(-21, -40),   S(-10,  13),   S(-14, -19),
            S( -6, -18),   S( -1,  24),   S(  0,  21),   S(  6,   9),   S( -8,   0),   S(-19, -10),   S( -5, -19),   S(-22, -27),
            S(-23, -48),   S(-19,   5),   S(-13,  41),   S( -4,   3),   S(  3,   7),   S( 25,  31),   S( -5, -22),   S(-12, -36),
            S(  6,  20),   S(-14,   2),   S( 25,  52),   S( 24,   0),   S(  2,  20),   S(  3, -11),   S( -3,   3),   S(  6,  16),
            S( -3,   5),   S(-18,  -9),   S( 24,  41),   S( 19,  10),   S( 31,   1),   S( 12, -21),   S( 24,  -2),   S(  2,  -7),
            S(  3, -106),  S(  4,  -2),   S( 22,   7),   S( -3, -15),   S(-16,  -7),   S(  1, -21),   S( -2, -10),   S(  2,   8),

            /* rooks: bucket 9 */
            S(-79, -33),   S( -8, -43),   S(-27, -31),   S(-72, -25),   S(-41,  26),   S(-38,  -7),   S(  8, -55),   S(-62, -39),
            S( 27, -35),   S(-23, -42),   S( -9, -30),   S(-31, -39),   S(-24, -33),   S(  4, -22),   S(-18, -65),   S(-17, -52),
            S( -3, -42),   S( 12, -36),   S(  0,  -2),   S( -8,  -5),   S(-51, -38),   S( 16, -12),   S(  7, -17),   S(-17, -23),
            S(  7, -12),   S( -7,  -4),   S(-14,  -6),   S(-37, -12),   S(-13,  -8),   S( -5, -14),   S( -6, -15),   S(-11, -12),
            S(  8, -11),   S(  0,  23),   S( -4,  -5),   S(  3,  18),   S( 15,  19),   S(  4,   1),   S( 15,   8),   S(  1, -49),
            S(  9,  13),   S(-12, -20),   S( -3,  -6),   S(-23,   7),   S( 16, -21),   S( 10,  14),   S(  8, -10),   S(  2, -26),
            S( 48,  -4),   S( 45,  -8),   S( 40,   2),   S( 27, -10),   S( 21, -31),   S( 31,  -4),   S( 26, -40),   S( 49,  -4),
            S( 44, -81),   S( 21, -61),   S( 23, -18),   S( 11,  13),   S( 19,  19),   S( 13, -31),   S(  8, -31),   S( 23,   4),

            /* rooks: bucket 10 */
            S(-37, -86),   S(-40, -69),   S(-30, -36),   S(-34, -43),   S(-45, -54),   S(-40, -54),   S( 22, -25),   S(-39, -45),
            S(-12, -33),   S(-14, -40),   S( -9, -39),   S(-48, -24),   S(-21, -37),   S(-12, -23),   S( 10, -24),   S(  5, -32),
            S(-24, -39),   S(-35, -43),   S(-19, -42),   S(-10, -25),   S(-46, -15),   S(  1, -29),   S( 26,  -1),   S(  6,  -3),
            S(-18, -29),   S( -3, -30),   S(-36, -18),   S( -6,   0),   S( 20, -28),   S( -3, -35),   S( 19,   9),   S(-13, -32),
            S(-10, -21),   S( -7,  -5),   S( 10, -19),   S( -3, -38),   S(-24, -15),   S( -2, -15),   S( 24,  15),   S(  4,  13),
            S( 32,   0),   S(  9,  20),   S(-12, -10),   S(  1,   4),   S(-23, -27),   S(  8, -20),   S( 13, -12),   S(  6,   3),
            S( 70, -19),   S( 68, -32),   S( 64, -16),   S( 35, -21),   S( 51, -12),   S( 32, -26),   S( 43, -15),   S( 19, -30),
            S( 38,  -1),   S(  9, -27),   S( 28, -17),   S( 16, -23),   S( 31,   0),   S( 17, -10),   S( 12, -29),   S( 20, -31),

            /* rooks: bucket 11 */
            S(-63, -36),   S(-42, -26),   S(-37, -26),   S(-19, -81),   S(-14, -20),   S(-35,   1),   S(-35, -47),   S(-35,  -2),
            S(-46, -34),   S( -2, -31),   S(-22, -26),   S(-54, -13),   S(-54, -29),   S(-44, -19),   S( 17,  14),   S(-32, -14),
            S(-17, -40),   S( -3, -31),   S( -6, -28),   S( -2, -32),   S(  0,  -5),   S( -9,  13),   S(-30, -36),   S(-22, -50),
            S(-29,  21),   S(-20, -33),   S( -7,  -4),   S( -3,   5),   S(-17, -12),   S(-19,  16),   S( -1, -23),   S(  0, -25),
            S(-12,   4),   S( 24,  -9),   S( 21,  -8),   S( 11, -30),   S( 29, -11),   S( 16,  -8),   S(  8, -10),   S( -1,   0),
            S(-18,  23),   S( 39,  12),   S(  8,  -8),   S( 41, -12),   S( 18,   5),   S( 37,   1),   S(-33, -11),   S( 17,   4),
            S( 39,  36),   S( 24,   1),   S( 38, -12),   S( 53,  -7),   S( 42,  15),   S( 28,   6),   S( 13,  40),   S( 44,  11),
            S( 22,  23),   S( 12,  13),   S( 22, -14),   S( 15,  12),   S( -8, -31),   S(  9, -12),   S(  8,  11),   S( 17, -18),

            /* rooks: bucket 12 */
            S(  0, -66),   S(-16, -60),   S(-10,  -7),   S( 14,  17),   S(-10, -39),   S(  1, -30),   S(-23, -57),   S(-15, -27),
            S( 15,  18),   S( -3,   5),   S(-15,  -7),   S( -1, -20),   S(  5,   4),   S(  1,  10),   S( -4, -24),   S( -6, -38),
            S(  1, -13),   S(-15, -18),   S(-19, -34),   S(-12, -20),   S(-11, -19),   S(  3,  -5),   S( -1,   3),   S( 13,  30),
            S( -4,  -7),   S( -7, -11),   S(  5,  12),   S( 12,  -3),   S( -7, -28),   S( -6, -25),   S(-15, -53),   S(  1, -13),
            S(-27, -33),   S(-16, -43),   S( 13,  17),   S(  6,   3),   S(-12, -29),   S(  7,  -9),   S( -3, -30),   S(-10, -31),
            S( -5, -19),   S(  2, -32),   S( 25,  -7),   S(  5, -25),   S( -1, -15),   S( -7, -29),   S(  0, -18),   S( -2,  -7),
            S( -7,  -9),   S( -6, -18),   S(  0, -42),   S( -5, -25),   S(  4, -33),   S( -4, -45),   S( -6, -28),   S(  4,  -9),
            S(  1, -36),   S(  7,   4),   S(  3, -26),   S(  1, -32),   S( -3, -28),   S( -6, -31),   S(-23, -72),   S(  5,  13),

            /* rooks: bucket 13 */
            S(  0, -36),   S( -7, -54),   S(-10, -10),   S( -3,  19),   S( 16,  40),   S(-21, -51),   S(-13, -57),   S(-24, -22),
            S( -8, -27),   S(  3,  12),   S( -9,  14),   S(-12,   8),   S(-24, -39),   S(  1,  -3),   S(  7,  26),   S(  9,  14),
            S( -2, -27),   S(-17, -57),   S(-11, -19),   S( -8, -20),   S(  3,  30),   S(  6,  -6),   S( -6, -15),   S( -4, -31),
            S(-19, -56),   S(  0,  16),   S(-26, -45),   S( -2,  -7),   S(  7, -14),   S(-21, -44),   S(  7, -14),   S(-11, -34),
            S(  1, -21),   S( -9, -38),   S( 14,  17),   S( -9, -28),   S(-14, -38),   S( -3, -36),   S(-14, -57),   S(  8,  27),
            S(-18, -34),   S(-13, -19),   S(-25, -45),   S(  4,  -8),   S( -3, -21),   S(  9,   2),   S(  4, -19),   S(  7,  14),
            S(-14, -20),   S(  6,   7),   S( -1,  22),   S( -7,  -1),   S( 12, -11),   S(  0,  -7),   S(  4, -31),   S(  7,  17),
            S(-19, -110),  S(-17, -60),   S( -3,  -9),   S( -5,   3),   S( -5, -12),   S(-11, -55),   S( -9, -44),   S(  2,  21),

            /* rooks: bucket 14 */
            S(-20, -33),   S(-22, -55),   S(  2, -10),   S(-13, -21),   S(-10,  10),   S(-14, -10),   S( 10,  -3),   S(-17, -31),
            S(-31, -77),   S(-27, -64),   S(-13, -18),   S( -6, -13),   S( -3,  -4),   S( -6, -29),   S(  2,  12),   S( 10,   2),
            S( -5, -11),   S(-14, -40),   S( -6, -19),   S(  1,  20),   S( -6, -11),   S(-14, -43),   S( 10,  33),   S(  2,  -7),
            S( -1,  11),   S( -8, -29),   S( -1, -17),   S( -8, -20),   S( -1, -17),   S( -5,  12),   S( -1, -30),   S(-11,  10),
            S(  3, -34),   S( -7, -52),   S(-12, -77),   S(-13, -62),   S( -5, -49),   S(-11, -69),   S(  4, -46),   S( -8, -32),
            S( -8, -44),   S( -5, -33),   S( -7, -71),   S( -6, -72),   S(  8, -48),   S(  1, -39),   S( -1, -48),   S(-12, -53),
            S( 12,  10),   S(  6, -30),   S( -5, -52),   S(  5, -56),   S(  6, -53),   S( -1, -33),   S( 16,   0),   S(  2, -16),
            S( -5, -17),   S( -5, -15),   S(-20, -72),   S( -2, -23),   S(-20, -56),   S(  0,  16),   S(  7,  22),   S(-15, -43),

            /* rooks: bucket 15 */
            S(-14, -30),   S(-24, -65),   S( -4, -38),   S(-18, -35),   S( 11,  -3),   S( -8,  -8),   S( -7,  -4),   S(-13, -40),
            S(-16, -13),   S( -7, -21),   S( -9, -24),   S( -6, -11),   S(-14, -33),   S(  8,  19),   S(-17, -57),   S( -2,  12),
            S( -8, -27),   S( -2, -20),   S(-16, -42),   S(  6,   0),   S(  4, -19),   S(-12, -35),   S(-16, -37),   S(-18, -40),
            S( -5, -38),   S( -4, -37),   S(-12, -20),   S( -8, -25),   S( -5, -26),   S( -3, -49),   S( -4, -33),   S(-13,  -1),
            S(  2, -16),   S( -7, -47),   S( 11, -17),   S( -1, -28),   S(  5, -13),   S(  7, -20),   S( -1, -28),   S(  2,  27),
            S( -3, -16),   S(  1, -19),   S( 10, -21),   S(  8, -32),   S(-12, -60),   S(  7, -40),   S( 16, -31),   S(-15, -29),
            S(  9,  19),   S( 15,  -7),   S(  7, -57),   S( -7, -66),   S(  1, -29),   S( 24,  18),   S( 12,   3),   S(  2,  -3),
            S(-11, -29),   S( -4, -15),   S( -1, -33),   S( -8, -44),   S( -1, -38),   S(  1, -12),   S( -4, -41),   S(-15, -56),

            /* queens: bucket 0 */
            S(-42, -36),   S( -6, -67),   S( 38, -73),   S( 68, -82),   S( 31, -26),   S( 39, -37),   S( 57,   3),   S( 11,   9),
            S(-17, -23),   S( 52, -83),   S( 36, -28),   S( 26,   2),   S( 33,  19),   S( 31,   2),   S( 26,  46),   S( 39,  32),
            S( 37,   9),   S( 32,  16),   S( 11,  31),   S( 24,  19),   S( 13,  11),   S( 12,  12),   S(  0,  30),   S( 22,  26),
            S(  6,  40),   S( 28,  24),   S(-13,  69),   S( 18,  33),   S( 12,  49),   S(  7,  39),   S( 15,  23),   S(  7,  50),
            S( 30,  14),   S( 11,  82),   S(  5,  58),   S( 27,  70),   S(  4,  35),   S(  1,  -4),   S( 10,  29),   S( 32,   5),
            S( 30,  48),   S( -1,  59),   S( 19,  24),   S( 34,  28),   S( 62,   6),   S( 29,  33),   S( 25,  14),   S( 14,  -6),
            S( 50,  50),   S( 74,  31),   S( 12,  39),   S( 55,  14),   S( -4,  26),   S(-26, -11),   S( 15,  34),   S( 25,  34),
            S( 28,  35),   S( 13,  18),   S( 43,  39),   S( 17,  48),   S( 44,  32),   S(-35,  -5),   S( 70,  18),   S( 50,  11),

            /* queens: bucket 1 */
            S( -1,   5),   S(-50, -41),   S(-53, -43),   S(  1, -100),  S( 10, -60),   S(-19, -36),   S(  2, -20),   S( 30,   0),
            S(-20, -30),   S( -6, -33),   S(  9, -22),   S(  6,  19),   S(  9,  -4),   S( 13,   8),   S( 37, -53),   S( 28,  11),
            S(-29,  37),   S( 13, -37),   S( 18,  19),   S(  9,   2),   S( -6,  51),   S( -5,  15),   S( 16,  -3),   S( 11,  39),
            S( 13, -18),   S( -7,  28),   S( -1,  50),   S( 24,  51),   S( 13,  45),   S( 10,  24),   S( -4,  25),   S( 21,  -5),
            S( 39, -63),   S(  8,  44),   S(-16,  93),   S(-19,  87),   S(-20,  67),   S(  8,  23),   S( -7,  21),   S( -7,  69),
            S(  6,  39),   S( 29,  55),   S( 34,  72),   S(-28,  69),   S(  7,  46),   S(-33,  68),   S( 31,  30),   S(  8,  57),
            S(-22,  60),   S( -8,  74),   S( -6,   4),   S(-25,  95),   S(-25,  70),   S( 11,  47),   S(  0,  43),   S(-30,  46),
            S( -8,  32),   S( 13,  40),   S( 50,  27),   S(  3,  40),   S( 14,  25),   S( -6,  15),   S( 11,   2),   S(-18,  24),

            /* queens: bucket 2 */
            S( 10,   9),   S( 27, -68),   S(  7, -39),   S(  5, -31),   S(-16, -11),   S(-24, -37),   S(-29,   6),   S(-19,  29),
            S(  4,  10),   S( 18,  11),   S( 21, -27),   S( 26, -31),   S( 20, -14),   S( 16, -64),   S( 28, -14),   S( 34,  -4),
            S( 11,   9),   S( 18,   4),   S(  9,  43),   S( 11,  33),   S(  9,  59),   S(  9,  59),   S(  8,   2),   S( 31,  10),
            S( 15,  -5),   S(-16,  51),   S(  1,  43),   S(  8,  59),   S( -3,  91),   S(-10,  98),   S( 11,  33),   S(  0,  64),
            S(  7,   7),   S(  5,  51),   S(-31,  74),   S(-51, 143),   S(-37, 102),   S(-25, 130),   S(-48, 128),   S(-11, 116),
            S( 16,  21),   S(  4,  59),   S(-30, 105),   S( -2,  59),   S(-17, 109),   S(-23, 120),   S(  2, 104),   S( -4,  93),
            S(-17,  52),   S(-28,  88),   S(-33,  81),   S( 22,  73),   S(-19,  95),   S( 44,  65),   S(-34,  50),   S(-41,  73),
            S(-61, 104),   S(  7,  55),   S( 41,  51),   S( 45,  50),   S( 22,  66),   S( 31,  37),   S( 39,  29),   S(-15,  53),

            /* queens: bucket 3 */
            S( 85,  71),   S( 42,  88),   S( 37,  97),   S( 40,  95),   S( 63,  21),   S( 47,  10),   S( 11,  10),   S( 30,  52),
            S( 57, 117),   S( 48, 101),   S( 37, 109),   S( 44,  73),   S( 45,  79),   S( 51,  56),   S( 53,   0),   S( 30,  34),
            S( 41,  86),   S( 42,  97),   S( 48,  85),   S( 48,  69),   S( 41,  88),   S( 40,  96),   S( 41, 110),   S( 51,  60),
            S( 36, 130),   S( 41,  94),   S( 36,  98),   S( 33, 102),   S( 41,  95),   S( 25, 135),   S( 37, 113),   S( 22, 140),
            S( 37, 110),   S( 46,  93),   S( 24, 107),   S( 23, 119),   S( 18, 138),   S(  5, 157),   S(  5, 168),   S( 33, 147),
            S( 43, 120),   S( 31, 119),   S( 46, 101),   S( 20, 125),   S( 25, 150),   S( 59, 124),   S( 52, 154),   S( 28, 181),
            S( 45, 130),   S( 31, 133),   S( 55, 103),   S( 59, 101),   S( 23, 132),   S( 42, 121),   S( 57, 148),   S(160,  41),
            S( 70,  92),   S( 89,  82),   S( 66, 104),   S( 66,  93),   S( 30,  97),   S( 87,  73),   S(120,  74),   S(108,  65),

            /* queens: bucket 4 */
            S(  5, -17),   S(-40, -39),   S(  0,  12),   S(-24, -45),   S( 29, -37),   S( -5, -13),   S(-42, -57),   S(-34, -10),
            S(-13, -29),   S(-21,  -7),   S( 15,   2),   S(-48,  45),   S( -6,  -9),   S( 20, -11),   S(-19,   1),   S(-45, -12),
            S( -9,  -6),   S( 22,  16),   S(  7,  24),   S( 20,  46),   S( 13,  -9),   S(  9,   3),   S( 22, -11),   S(-15, -13),
            S( -9,  -5),   S(-10, -10),   S( 23,  40),   S( -2,  49),   S( 35,  28),   S( 20,  38),   S(-20, -58),   S( -6,  14),
            S(-18,   9),   S( 40,  13),   S( 14,  23),   S( 43,  57),   S( 34,  45),   S( 31, -23),   S(-25,  -2),   S( 15, -18),
            S(  4,  19),   S( 16, -15),   S( 35,  66),   S( 35,  35),   S( 12,  21),   S( -5,   9),   S(-24, -25),   S( -4,  19),
            S(-24, -24),   S(-11,  29),   S( 15,  20),   S( 44,  72),   S(  5,  -7),   S(-20, -13),   S(  5, -18),   S(-42, -43),
            S(-21, -39),   S( 13,  32),   S( 49,  56),   S( 25,  27),   S(-10,   1),   S(-11, -12),   S(-19, -32),   S(-26, -32),

            /* queens: bucket 5 */
            S(-54, -18),   S(-11, -19),   S( -2, -22),   S(-57,   2),   S(-17,   2),   S( 31, -11),   S(-14, -12),   S(-13, -31),
            S(-29, -16),   S(-26,  17),   S(-48, -40),   S(-77, -37),   S( -8, -26),   S(-48, -16),   S(-66, -17),   S(-30,   5),
            S(-32,  -5),   S(-38,  -5),   S(-57, -13),   S(-47,  29),   S( 54,  42),   S(-16,  37),   S( 15,   9),   S(  9,  26),
            S(-59, -36),   S(-49,   6),   S( 28,  48),   S( 16,  66),   S( 43,  35),   S( 10,  16),   S( 34,  -7),   S(-22,  17),
            S(-63,  -4),   S(-43, -16),   S(-10,  71),   S( 11,  79),   S( 53,  57),   S(  4,  13),   S(-12,   1),   S(-39,   3),
            S(-34,   1),   S(  8,  50),   S(-19,  53),   S( 25,  62),   S( 32,  52),   S( 27,  13),   S(  5,  15),   S(-24, -28),
            S(-19,   4),   S(  1,  18),   S( 13,  58),   S(  3,  51),   S( 45,  51),   S( 11,  32),   S( 12,  14),   S(-28, -31),
            S( 11,  24),   S( 23,   9),   S(  8,  32),   S( 25,  53),   S(  5,  28),   S(  3,  30),   S(  3,  -2),   S(-29, -35),

            /* queens: bucket 6 */
            S(-42, -14),   S(-34, -21),   S(-41, -25),   S(-61, -54),   S(-103, -24),  S(-64, -44),   S(-46, -66),   S( -4,  -6),
            S(-79, -37),   S(-40,  -5),   S(-28,  -4),   S(-21,  -9),   S(-58,  28),   S(-106, -24),  S(-78, -12),   S(-26,  11),
            S(-34,  -1),   S(-41,   8),   S(-60,  42),   S(-82,  90),   S(-53,  68),   S(-32,  10),   S(-17,  16),   S( 19,  13),
            S(-47,  18),   S( 39,  11),   S(-26,  75),   S(-50,  77),   S( -3,  65),   S(  1,  42),   S(-30,   8),   S(  9,  -3),
            S(-31,  46),   S( -4,  32),   S(-26,  73),   S( 10,  59),   S( 49,  82),   S( 57,  56),   S( 66,  43),   S( -6,  14),
            S(-15,  46),   S(  8,  27),   S( 34,  30),   S( 32,  70),   S( 23,  77),   S( 80,  67),   S(-25,   3),   S( -5,  34),
            S(  5,  23),   S(-13,  10),   S( -1,  53),   S( -1,  35),   S( 65,  57),   S( 58,  75),   S( 10,  59),   S(-36,  -1),
            S(  7,  55),   S(  8,  25),   S( 27,  56),   S( 23,  50),   S( 63,  85),   S( 16,  48),   S( -3,  30),   S( 16,  44),

            /* queens: bucket 7 */
            S(  2, -16),   S(-19,   1),   S(-58,   2),   S(-12,   2),   S(-35,   6),   S(-54, -21),   S(-25,   2),   S(  5,   2),
            S(-53,  -4),   S(-48,  -4),   S(-36,  35),   S(-11,  35),   S(-20,  35),   S(-66,  55),   S(-29,  50),   S(-48, -30),
            S(-24,   0),   S(-55,  46),   S(  0,  17),   S(-15,  53),   S(  8,  45),   S(  3,  46),   S( -8,  -3),   S(-29,  -2),
            S(-86,   5),   S( -3,   5),   S(-27,  43),   S( 19,  39),   S( 48,  31),   S( 41,  33),   S(-14,  83),   S( 39,  24),
            S(-35,  39),   S(-69,  43),   S( 11,  30),   S( 47,  27),   S( 49,  32),   S( 92,  28),   S( 41,  49),   S( 22,  23),
            S(-13,  25),   S(-16,  29),   S( -1,   9),   S( 10,  19),   S( 63,  53),   S( 79,  14),   S( 81,  32),   S( 62,  33),
            S( 11,  -5),   S( -7,  29),   S( -1,  42),   S( 28,  42),   S( 61,  42),   S( 75,  51),   S( 36, -12),   S( 70,  46),
            S(  7,  26),   S( 15,   3),   S( 55,  18),   S( 30,  27),   S( 24,  45),   S( 26,  25),   S( 27,  32),   S( 40,  51),

            /* queens: bucket 8 */
            S(-15, -24),   S(  6,   0),   S(-19, -20),   S(-23, -33),   S(-14, -15),   S( -4, -11),   S(-22, -33),   S(-10, -19),
            S(-14,  -9),   S( -9,  -8),   S(-16,  -7),   S(-13,   9),   S( -8, -22),   S(-27, -43),   S(-37, -76),   S( -8, -14),
            S( -5,  -4),   S(-15, -10),   S( 15,  27),   S( 10,  11),   S( -8,  -8),   S(-21, -32),   S(-16, -40),   S(-21, -35),
            S( -9, -12),   S( 22,  33),   S( -4,   7),   S(-18,  14),   S( 12,  20),   S( -3,  -6),   S( 12,  11),   S(-22, -50),
            S( 26,  47),   S( 14,  47),   S( 22,  31),   S(  7,  21),   S( 31,  61),   S( -5,   0),   S(-11, -14),   S(-17, -30),
            S(  8,  17),   S( 22,  38),   S( -3,  33),   S( 21,  46),   S(-30, -28),   S( -9, -20),   S(  4,  26),   S( -3,  -8),
            S(  2,   2),   S(-22, -16),   S( 25,  46),   S(  8,  18),   S(  3,   1),   S( -1,   7),   S( -6, -10),   S( -5,  -9),
            S(-25, -46),   S( 23,  49),   S(-25, -48),   S(-24, -56),   S(-16, -50),   S(-10, -23),   S(-13, -41),   S( -4, -13),

            /* queens: bucket 9 */
            S(  2,   8),   S(-22, -55),   S(  5,   6),   S(-40, -52),   S(-22, -42),   S(-30, -41),   S(-15, -26),   S(-19, -41),
            S(  2,  -3),   S(-15, -28),   S(-15, -23),   S(-29, -41),   S(-39, -49),   S(-17, -37),   S( -5, -24),   S( -5, -15),
            S(  6,   8),   S( 12,  25),   S(-24,   7),   S(-23, -33),   S(-18, -13),   S(-15,  -9),   S(-19, -29),   S( -7, -16),
            S(-24, -32),   S(-21, -24),   S(  3,  33),   S( -8,  -7),   S(  9,  15),   S(  1,  -5),   S( -3,  -9),   S(  5, -13),
            S(  8,  19),   S( -7,   9),   S( 27,  57),   S( 30,  66),   S( -4,  28),   S( -1,  12),   S( -8,  -9),   S(-14, -11),
            S(-25, -24),   S(-22, -15),   S( -4,  17),   S( -8,  18),   S(-17,  -8),   S(-20, -19),   S(-22, -43),   S( -9,  -6),
            S(-19, -26),   S(-23, -42),   S( -2,  19),   S(  5,  21),   S(  5,   2),   S(  5,  -2),   S( 11,  24),   S(-14, -27),
            S(-11, -14),   S(-17, -33),   S(-15, -21),   S(  2,  -1),   S(  1,   3),   S(  7,  12),   S( -3, -18),   S(-15, -23),

            /* queens: bucket 10 */
            S(-18, -30),   S(-20, -32),   S(-22, -46),   S(-37, -58),   S(-21, -35),   S(-14,  -4),   S(-15, -37),   S( -5, -19),
            S(-14, -34),   S(-16, -33),   S(-20, -34),   S(-19,  -6),   S(-31, -26),   S(-22, -21),   S( -5, -20),   S(-27, -34),
            S(-13, -28),   S(-22, -28),   S(-14, -18),   S( -6,   9),   S( -2,  19),   S(-24,  -3),   S(-11, -13),   S( -1,   2),
            S(-11,  -9),   S(  0,   4),   S( -8, -11),   S(  9,  12),   S( -2,  24),   S(-15,  13),   S( -2,  -3),   S(-15, -23),
            S(-13, -17),   S(-11, -29),   S(-27, -23),   S( -5,   2),   S(-12, -10),   S(  2,  21),   S( 10,   8),   S(-10, -10),
            S( -9, -16),   S(-22, -38),   S(  9,  21),   S( 13,  51),   S(  0,  15),   S(  4,  24),   S(  2,   2),   S(  2,  -4),
            S(  4,   9),   S(-29, -25),   S( -7,   1),   S(-24, -13),   S( -1,   4),   S(  3,  20),   S( -2,  -4),   S( -8, -17),
            S(  1,  -4),   S(-10, -10),   S(  3,   2),   S(  0,   0),   S(  9,  12),   S(  6,  11),   S( -6,  -6),   S(-17, -28),

            /* queens: bucket 11 */
            S(  0, -10),   S(-16, -36),   S(-14, -18),   S( -9, -51),   S(-26, -40),   S(-31, -38),   S(-19, -36),   S(-21, -44),
            S(-12, -29),   S(-22, -30),   S(-55, -60),   S( 20,  13),   S(-15, -10),   S(-17, -22),   S( -7, -11),   S(  3,  16),
            S(-25, -32),   S(-23, -38),   S(  0, -29),   S(-38, -46),   S(-17, -10),   S( 25,  26),   S(  2,  22),   S(-26, -31),
            S( -8, -29),   S(-21, -31),   S(-16, -30),   S( 13,  39),   S(  4,  12),   S( -5,  15),   S( 37,  50),   S(-30, -32),
            S(-13, -15),   S(-10,  -9),   S(-16, -17),   S( 16,   4),   S( 19,   8),   S( 28,  66),   S( 26,  48),   S(  1,  11),
            S(-18, -38),   S( 11,  30),   S(-23,  -8),   S(  7,  15),   S( 14,  24),   S( 43,  65),   S( 11,  -6),   S(  6,  11),
            S(-14, -26),   S(-26, -32),   S( 18,  36),   S(-11,  -3),   S( -4,  -1),   S(  9,  16),   S( 35,  35),   S(-23, -34),
            S(-23, -32),   S(-16, -29),   S(-13, -26),   S( -8, -19),   S( -3, -11),   S(-25, -54),   S( 10,   7),   S( -8, -26),

            /* queens: bucket 12 */
            S(  4,   9),   S(  7,  14),   S(  2,   9),   S( -6,  -7),   S( -4,  -4),   S( -2,  -5),   S( -6, -12),   S(-10, -24),
            S( -8, -18),   S( -8, -19),   S( -4,  -2),   S( -1,  -3),   S(-10, -20),   S( -9, -14),   S( -5, -13),   S( -9, -20),
            S(  0,   0),   S(-10, -16),   S( 18,  19),   S(  1, -15),   S(-13, -16),   S(-13, -21),   S(-11, -24),   S( -2,  -3),
            S( -2,  -5),   S( -8, -10),   S(  8,  13),   S( 16,  37),   S(  6,  10),   S(-10, -20),   S( -2,   2),   S(-12, -29),
            S(  1,   4),   S( 20,  38),   S( 28,  60),   S( -1,  14),   S(  7,  16),   S(  1,  -1),   S(-17, -28),   S(-13, -31),
            S(  8,  17),   S(  8,  20),   S( 32,  61),   S(  8,  -4),   S(  4,   5),   S( -3, -10),   S( 10,  22),   S( -7, -19),
            S( -5,  -9),   S( 22,  47),   S( 24,  48),   S( -1,  -6),   S(  3,  -1),   S(  6,   9),   S(  2,   1),   S(  0,   1),
            S(-14, -40),   S(-15, -22),   S(-27, -41),   S(-13, -27),   S( -1,   0),   S(  1,   1),   S(  1,  -2),   S( -7, -11),

            /* queens: bucket 13 */
            S( -7, -22),   S(-10, -27),   S(  2,   0),   S(-13, -20),   S( -5, -14),   S( -7, -21),   S(-10, -28),   S( -8, -15),
            S(  3,   7),   S( -1,  -2),   S( 13,  23),   S( -2,  -9),   S(-12, -17),   S( -9, -20),   S( -2,  -6),   S(-13, -28),
            S(  1,   5),   S( -7, -13),   S( -8, -16),   S( -3, -18),   S(  0,  -2),   S( -6, -16),   S( -3,  -9),   S(-17, -24),
            S(  4,   5),   S(  2,   2),   S( 10,   8),   S( 18,  27),   S(  7,  20),   S( -9, -22),   S( -8, -16),   S( -9, -21),
            S( -2,  -5),   S( 19,  38),   S( 15,  24),   S( -3,  12),   S( 34,  68),   S(-21, -43),   S( -6, -19),   S(-17, -44),
            S( -9, -20),   S(  0,   5),   S( 33,  69),   S( 23,  64),   S( -4,  21),   S(  4,   2),   S(  7,  12),   S( -9, -21),
            S(-11, -17),   S( 11,  25),   S( -2,  16),   S( 19,  44),   S(  7,  12),   S(  6,  10),   S( -2,  -2),   S(  4,   6),
            S(-15, -34),   S(  0,   2),   S( -5,  -7),   S( -3,  -3),   S( 13,  21),   S(  3,   2),   S(-16, -31),   S(-11, -24),

            /* queens: bucket 14 */
            S(  1,   7),   S( -7, -18),   S( -4, -11),   S(-13, -23),   S(  2,   4),   S(  0,  -3),   S( -4, -12),   S( -5, -15),
            S( -5, -12),   S(  6,  11),   S( -8, -23),   S( -3, -16),   S( -8, -21),   S( -1,  -3),   S( -8, -20),   S( -7, -19),
            S(  4,   1),   S( -9, -16),   S( -4,  -2),   S( -9, -14),   S(  6,   7),   S( -4, -17),   S(  0,   1),   S(-12, -14),
            S(  1,   2),   S( 11,  13),   S(  5,  11),   S( 22,  51),   S( 11,  24),   S( -6, -12),   S(  9,  19),   S( -1, -11),
            S( -1,   0),   S(  2,   6),   S( -3,  -3),   S( 15,  28),   S( 21,  45),   S(  8,  13),   S( 11,  16),   S( -2, -17),
            S( -7, -15),   S( -4, -10),   S(  0,  10),   S(  2,  -2),   S(  5,  26),   S( 16,  39),   S( 13,  14),   S( -5, -12),
            S(  2,   5),   S(  5,   6),   S( 20,  40),   S(  3,   3),   S( 14,  24),   S(  7,  21),   S( 11,  16),   S(  5,   7),
            S( -3, -10),   S(-10, -14),   S( -3,  -2),   S( 10,  12),   S( -6, -12),   S(  5,   2),   S( -6,  -9),   S(-21, -46),

            /* queens: bucket 15 */
            S( -6, -17),   S( -5, -19),   S( -7, -18),   S(  6,   8),   S(  0,  -1),   S( -2,  -8),   S(-12, -29),   S( -2, -10),
            S( -4, -15),   S( -4, -16),   S(-18, -34),   S( -2,  -6),   S( -9, -16),   S(-10, -19),   S( 10,  20),   S( -2,  -5),
            S(  3,  -2),   S( -5, -15),   S(  0,   3),   S( -5, -11),   S(  3,  -2),   S(  9,  14),   S( -6, -12),   S( -1,  -6),
            S(  0,  -2),   S(  0,  -5),   S( -6,  -7),   S(  7,  12),   S(  3,   3),   S( -3,   7),   S( 15,  20),   S(  1,   8),
            S( -2,   0),   S( -3, -11),   S( -5, -16),   S(  8,  11),   S( -1,  -3),   S( 12,  15),   S( -6, -12),   S(  1,  -1),
            S(  2,   2),   S(  3,  10),   S(  9,  16),   S(-10, -22),   S(  5,   5),   S( 25,  51),   S(  6,   6),   S( -5, -12),
            S(  0,   2),   S(  2,  -5),   S( 12,  22),   S(  7,  12),   S(  9,  15),   S( 20,  34),   S(  6,   2),   S(  6,  10),
            S(  2,   4),   S(  0,  -2),   S( -4, -12),   S( 10,  19),   S( -4, -19),   S( -2, -14),   S(-12, -28),   S( -8, -14),

            /* kings: bucket 0 */
            S( 15,  49),   S( 12,  69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 17,  62),   S( 96,  70),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-20,  17),   S(-69,  26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 24,  30),   S(  3,  39),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-47,  36),   S(-42,  29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  45),   S( 25,  40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 16,  59),   S( -4,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 62,  83),   S( 10,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -35),   S( 28, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  -2),   S( 10,  16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -47),   S(-30, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11, -16),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-22, -14),   S(-36, -13),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  4,  -3),   S(-19,   5),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 31,  12),   S(-19, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 41,  37),   S(-38,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-95, -55),   S(  9, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56, -42),   S( 30, -17),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-17, -43),   S(-17, -57),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 85, -60),   S( -9, -39),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-23, -46),   S(-59, -35),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 75, -62),   S( 76, -47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 13, -26),   S(-108, -46),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 64, -36),   S( -2, -55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -56),   S( 44, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -95),   S(-15, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 13, -26),   S( 67, -35),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 27, -64),   S( 27, -58),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 45, -45),   S( 49, -45),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 40, -69),   S( -3, -54),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 56, -44),   S(-30, -66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -5, -64),   S( -8, -110),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26,  -9),   S(-38,  10),   S( -7,  -5),   S(-15,   4),   S(  4,  -2),   S( 27,  10),   S( 29,  -9),   S( 40,   2),
            S(-10, -31),   S(-34,  -7),   S(-18,  -8),   S( -9,  -2),   S(  1,  10),   S( -2,  11),   S( 26,  -8),   S( 18,  14),
            S(  8, -19),   S(  0, -18),   S( 24, -21),   S( 14, -27),   S( 12,  -3),   S( 16,  24),   S( 11,  29),   S( 49,   9),
            S( 29, -24),   S( 51,  -4),   S( 66, -21),   S( 39,  12),   S( 23,  44),   S( -3,  71),   S( 27,  57),   S( 72,  47),
            S( 86, -76),   S( 98, -32),   S( 89, -23),   S( 63,   7),   S( 41, 144),   S( 20, 104),   S( 33, 157),   S( 98,  99),
            S(-231, -114), S(-221, -143), S( 34, -181),  S( 32,  46),   S( 81, 141),   S( 56, 152),   S(122,  92),   S( 74, 169),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  16),   S(-47,   6),   S(-35,   2),   S(-45,  29),   S(-29,  -2),   S( -3,   4),   S(  4,  -7),   S(  0,  21),
            S(-55,   0),   S(-40,  -6),   S(-43,  -4),   S(-23,  -2),   S( -8,   0),   S(-29,   6),   S( -9,  -8),   S(-24,  10),
            S(-41,  19),   S(-14,   9),   S(-22,   0),   S(  9, -19),   S( -6,  17),   S(-10,  11),   S(-20,  16),   S(  9,   9),
            S(-24,  31),   S( 36,   5),   S( -2,  14),   S( 20,  21),   S( 14,  16),   S(-19,  26),   S(  5,  23),   S( 53,  33),
            S( 11,  17),   S( 66, -24),   S( 97, -67),   S( 81, -53),   S( 61,   1),   S( 27,  30),   S( -9,  61),   S( 62,  79),
            S(151, -55),   S(-22, -32),   S(  6, -144),  S(-18, -128),  S(-19, -77),   S(-23,  45),   S( 65, 144),   S(122, 137),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  38),   S(-39,   8),   S(-37,   9),   S(-25,  18),   S(-50,  39),   S(-37,  17),   S(-19,  -8),   S(-24,  22),
            S(-54,  17),   S(-39,   9),   S(-48,   4),   S(-48,  24),   S(-44,  20),   S(-47,   7),   S(-26, -13),   S(-49,   7),
            S(-36,  39),   S(-30,  35),   S(-18,  16),   S(-22,  16),   S(-29,  29),   S(-24,  10),   S(-24,   5),   S(-21,  10),
            S(-14,  68),   S(-19,  50),   S( -6,  35),   S(  3,  34),   S( -4,  28),   S(-13,  12),   S( 10,  13),   S( 31,   8),
            S(-38, 117),   S(-39,  94),   S(-20,  32),   S( 39, -23),   S( 92, -54),   S( 87, -29),   S( 87, -19),   S( 51,   7),
            S(  0, 202),   S( 81,  90),   S( 12,  54),   S( 13, -71),   S(-66, -177),  S(-82, -170),  S( 46, -74),   S(101, -41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9,  19),   S(-11,   9),   S(-11,  17),   S(-14,  54),   S(-21,  53),   S( 10,  35),   S( -3,  13),   S(-14,   0),
            S(  0,   8),   S(  3,   8),   S(-16,  12),   S(-18,  19),   S( -3,  23),   S(  1,   9),   S( -5,   0),   S(-34,   1),
            S( 14,  35),   S( -5,  49),   S(  2,  23),   S( -4,   9),   S( 17,   2),   S( 18,   0),   S(  3,  -9),   S(-10,   0),
            S( 19,  79),   S(  0,  90),   S( 11,  68),   S( 16,  33),   S( 32,   3),   S( 36, -18),   S( 26,  10),   S( 43, -15),
            S( 10, 146),   S(-22, 167),   S(-25, 159),   S(  0, 117),   S( 43,  51),   S( 87,  -6),   S(105, -33),   S( 95, -36),
            S( 53, 145),   S( 43, 225),   S(-30, 275),   S( -9, 201),   S(-30,  82),   S( 15, -137),  S(-192, -191), S(-268, -163),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 74,  -5),   S( 21,  -7),   S(-29,  -5),   S(  5, -22),   S(-14,  -8),   S(-10,  -9),   S(-23,  -6),   S(-62,  32),
            S( 46, -21),   S( 25,  -5),   S( 27, -21),   S(-32,   0),   S(-30, -16),   S(-26, -20),   S(-44, -13),   S(-56,  -3),
            S( 90, -26),   S(117, -40),   S( 53, -33),   S(-20, -12),   S(-76,  14),   S(-37,  20),   S(-63,  17),   S(-52,  28),
            S(-56, -80),   S( 15, -113),  S( 30, -33),   S(-40,  11),   S(-15,  17),   S(-49,  42),   S(  1,  39),   S(  6,  43),
            S( 35, -100),  S( -3, -111),  S(  1, -80),   S( 67,   5),   S( 61,  93),   S( 40,  59),   S( 28,  78),   S( 19,  98),
            S( 11, -68),   S(-17, -71),   S( 16, -69),   S( 24,  44),   S( 56, 101),   S( 56, 148),   S( 46, 123),   S( 44, 121),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-67,  37),   S(-19,  13),   S( 10,  -2),   S( 65,  -7),   S( 38,  -5),   S( 15,  -5),   S(-28,   1),   S(-49,  36),
            S(-53,  15),   S(  7,   1),   S( 26, -16),   S( -7,  16),   S( -7,   5),   S( -1, -11),   S(-63,  -3),   S(-81,  21),
            S(-25,  20),   S( 38,  11),   S( 85,  -5),   S( 29,  12),   S(-11,  29),   S(-12,   4),   S(-41,  13),   S(-48,  30),
            S( 18,  12),   S( 53, -23),   S(-14, -56),   S(-46, -12),   S(  5, -21),   S( 13,   3),   S( 36,   5),   S(  8,  34),
            S( 56,  -2),   S( 40, -47),   S( 24, -92),   S(-11, -48),   S( 55, -49),   S( 65,  11),   S( 52,  35),   S( -6,  93),
            S( 43,  18),   S( 37, -28),   S(-20, -90),   S( 38, -86),   S(-17, -66),   S( 64,  38),   S( 76, 105),   S( 91,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-75,  23),   S(-46,  -4),   S(-12,  -8),   S( -1,   8),   S( 17,   7),   S( 28,   1),   S( 22,  -4),   S( -8,  23),
            S(-66,   4),   S(-42, -10),   S(-14, -23),   S( 47, -11),   S( -3,  11),   S(  4,  -6),   S(  4,  -7),   S( -7,  -1),
            S(-46,  27),   S(-40,  16),   S( -3,   7),   S(  6,   8),   S( 34,  19),   S( 54,  -8),   S( 56,  -7),   S( 11,   7),
            S(-22,  48),   S( -8,  26),   S( 18,  10),   S( 47, -12),   S(-28, -42),   S(  8, -42),   S( 79, -41),   S( 84, -25),
            S( 11,  72),   S( 44,  21),   S( 42,  -8),   S( 31, -40),   S(  5, -75),   S( -6, -65),   S( 93, -51),   S(139, -20),
            S(141,  37),   S(103,  53),   S( 98,  12),   S( 50, -66),   S( 16, -126),  S(  3, -120),  S( -2, -46),   S( 68,  -1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50,  -5),   S(-46, -28),   S( -8, -26),   S(-18,  22),   S( 52, -12),   S( 70, -17),   S( 63, -29),   S( 60,  -6),
            S(-51, -10),   S(-50, -18),   S(-44, -20),   S(-35,   4),   S( -2,  -4),   S( 45, -27),   S( 35, -23),   S( 44, -18),
            S(-24,  10),   S(-56,  16),   S(-30,   6),   S(-35,  -5),   S(  6,  -3),   S( 30, -18),   S( 79, -25),   S( 69, -21),
            S(  8,  25),   S(-55,  54),   S(-18,  44),   S( -5,  20),   S( 13, -14),   S( 57, -63),   S( 26, -75),   S( 20, -102),
            S( 41,  45),   S( -9, 101),   S( 48,  84),   S( 30,  74),   S(-20,  34),   S(  9, -68),   S(-35, -112),  S( -5, -88),
            S(112,  80),   S(115,  83),   S(126,  92),   S( 75,  79),   S( 71,  -8),   S( -1, -80),   S( -5, -105),  S( 24, -167),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -18),   S(-16, -25),   S( 58,   0),   S(-14, -31),   S(-18, -57),   S( 35, -34),   S( 37, -54),   S( 44, -27),
            S(-37, -44),   S(-20, -11),   S(-58, -54),   S( -3, -37),   S(-50, -38),   S( 32, -50),   S(-18, -49),   S(-61, -26),
            S(-26, -67),   S( 15, -77),   S(  4, -57),   S(-36, -53),   S(-54,  -7),   S(-20, -15),   S(-32, -13),   S(-60,  -5),
            S(  2, -23),   S(-35, -29),   S( 20, -18),   S(  5, -14),   S( 10,   4),   S(  1,  21),   S(  1,  40),   S(  8,  28),
            S( 23,  23),   S(  3, -45),   S( 11,  29),   S( 23,  62),   S( 55,  98),   S( 34, 128),   S( 26,  72),   S(-12, 107),
            S( 25,  66),   S( 18,  82),   S( 33,  76),   S( 36,  87),   S( 45, 103),   S( 56, 179),   S( 41, 117),   S(  8,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,   3),   S( 42, -19),   S( 59,   0),   S( -7, -13),   S(  7,  -6),   S( 56, -37),   S( 68, -62),   S( -1, -24),
            S(  3, -44),   S(-42, -44),   S(-23, -46),   S(-21, -25),   S(-20, -39),   S(-17, -37),   S( 40, -64),   S( 11, -46),
            S(-62, -25),   S( 12, -65),   S(-23, -83),   S(-54, -40),   S( 18, -43),   S(-13, -43),   S(-61, -31),   S(-16, -16),
            S(-42,  -1),   S(-62, -37),   S(-12, -42),   S( -4, -33),   S(  0, -43),   S(  3, -28),   S(-18,  -4),   S( 27, -10),
            S( -1,   9),   S( -4, -35),   S( 19,  -6),   S( 19,  14),   S( 17,  58),   S( 24,  28),   S(  0,  63),   S(-15,  54),
            S(  0,  49),   S( 44,  60),   S(  1,  -2),   S( 28,  11),   S( 35,  68),   S( 25,  59),   S(  1,  49),   S( 22,  78),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36, -54),   S(-30, -44),   S( -4, -12),   S( -5, -31),   S( 38, -30),   S(146, -31),   S( 75, -20),   S( 40, -25),
            S(-27, -60),   S(-66, -57),   S(  8, -60),   S( -1, -49),   S( 19, -38),   S( 24, -41),   S(  9, -47),   S(-12, -37),
            S(-72, -22),   S(-13, -50),   S(-21, -48),   S(  6, -50),   S(  3, -42),   S(-43, -68),   S( 11, -73),   S( 20, -46),
            S(-16, -13),   S(  4, -22),   S( 11, -34),   S(  0, -57),   S(-20, -43),   S(-23, -42),   S(-15, -59),   S( 10, -36),
            S(  6,  -5),   S( 17,  19),   S( 31,  11),   S( -9, -22),   S( 46, -10),   S( 22,  18),   S(  6, -13),   S(  3,   4),
            S(-10,  42),   S(-10,  28),   S( 30,  58),   S( 14,  31),   S( 26,  43),   S( -5, -17),   S(-17,  -4),   S( 32,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -53),   S(-22, -41),   S( -7, -38),   S( 29,  43),   S(  8, -19),   S(128, -10),   S(119, -36),   S( 51, -13),
            S(-32, -56),   S(-48, -63),   S(-35, -70),   S(  0, -56),   S( -3, -39),   S(-13, -44),   S(-13, -27),   S( 20, -59),
            S(-79,  -2),   S(-13, -34),   S(-53,  -7),   S( 37, -37),   S(-45, -45),   S( 30, -47),   S(-38, -79),   S(  8, -63),
            S(-26,   9),   S(-30,  17),   S( 32,  19),   S( 53,  -7),   S( -5,  -6),   S( 10, -49),   S(-23, -49),   S(-28, -34),
            S( 13,  12),   S(  6,  46),   S( 23,  72),   S( 30,  74),   S( 38,  58),   S( -5,   3),   S(  6,  -3),   S( 20, -14),
            S( 52,  61),   S( 22,  92),   S( 36,  86),   S( 22,  62),   S( 21,  37),   S( 49, 102),   S( 17,  31),   S( 27,  41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31, -71),   S(-13,  -1),   S(-18,   1),   S(  0,   5),   S( -6, -20),   S(-36, -42),   S( 31, -22),   S( 10, -27),
            S(  6, -35),   S(-18, -11),   S(-36, -75),   S(-35, -30),   S(-42, -63),   S(-27, -32),   S(-32, -49),   S(-13, -60),
            S(-40, -10),   S( -4, -67),   S(-14, -80),   S(-29, -66),   S( -6, -24),   S( -6, -16),   S(-61, -23),   S(-42, -35),
            S(-23,  -7),   S(  0,  17),   S( -5, -35),   S(  4,  10),   S(  9,  54),   S( -4,  62),   S( -1,  47),   S(-43,  28),
            S( 15,  39),   S(  7,  17),   S( 10,  25),   S( 13,  66),   S( 34, 101),   S( 22,  89),   S(  7, 107),   S(  4, 101),
            S( 17,  46),   S(  1,  -5),   S( 21,  74),   S( 12,  53),   S( 38, 128),   S( 33, 138),   S(-35,  -4),   S(-15,  45),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -72),   S(-20, -57),   S( -2, -29),   S( -7, -26),   S(  7,  -2),   S(-22, -23),   S( 23, -55),   S(-14, -57),
            S(-73, -66),   S(-26, -65),   S(-46, -56),   S(  5, -26),   S(-36, -50),   S(-22, -50),   S(-19, -55),   S(-13, -50),
            S(-25, -33),   S(-24, -59),   S(-15, -72),   S(-19, -52),   S( -4, -39),   S(  0, -36),   S(-40, -36),   S(-36, -36),
            S(-14,  30),   S( -8, -15),   S(  9, -35),   S( 10,  35),   S(  6,  50),   S(-27,  52),   S(-16, -13),   S(-17,  25),
            S( -9,  43),   S( 13,  50),   S( -4,  -3),   S( 28,  38),   S( 27,  76),   S( 37, 114),   S( 20, 121),   S( -3, 104),
            S( 10,  78),   S( 29,  99),   S(  1,  18),   S( 12,  31),   S( 19, 100),   S(  8,  68),   S( 14,  91),   S(-15,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -90),   S( 10, -72),   S( 29, -10),   S(  0,   5),   S( -5, -21),   S(-16,  -6),   S(  4, -52),   S( -4, -42),
            S(-23, -83),   S(-62, -74),   S(-22, -66),   S(-26, -87),   S(-14, -23),   S(-12, -44),   S(-11, -23),   S(-12, -52),
            S(-35, -65),   S(-35, -60),   S(-21, -46),   S(-19, -64),   S( -8, -50),   S(-17, -64),   S(-11, -65),   S(-46, -11),
            S(-28,  -7),   S(-45, -40),   S(-11,   8),   S( 10,  18),   S(  5, -28),   S(  7,  15),   S(  3,  15),   S( -6,  22),
            S( -1,   2),   S( 14,  54),   S( -7,  60),   S(  3,  18),   S( 37,  84),   S(  6,  27),   S( 14,  84),   S(  8,  69),
            S( -6,  64),   S( 21,  99),   S( 32, 102),   S( 21,  69),   S( 24,  91),   S(  3,  15),   S( 33, 128),   S( 29, 125),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -25),   S(  5, -54),   S(-16, -31),   S(-12, -32),   S( -6, -21),   S(-46, -21),   S(-20, -21),   S(-12, -66),
            S(-49, -36),   S(-35, -66),   S(-36, -91),   S( -5, -49),   S(-12,  -5),   S(-41,  -4),   S(-38, -11),   S(-28, -61),
            S(-30,   1),   S(-53, -54),   S(-35, -10),   S(  7, -45),   S(-30, -36),   S(-14, -37),   S( 13, -25),   S(-20, -12),
            S(-21,  46),   S(-23, -10),   S(  3,  58),   S(-12,   5),   S( -3,  15),   S(-16, -26),   S( -3,  -9),   S( -2,  18),
            S( -1,  73),   S(  3,  61),   S( 14,  83),   S( 22,  60),   S( 27, 117),   S(  4,  44),   S( 15,  40),   S( 10,  30),
            S(-11,  55),   S( -4,   6),   S( -3,  73),   S( 32,  88),   S( 23,  80),   S( 19,  52),   S( 21,  56),   S( 17,  61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-82, -48),   S(-36, -12),   S(-19,   7),   S( 18,  32),   S(-18, -21),   S(-32,  34),   S(-12, -26),   S(-103, -47),
            S( -7, -57),   S(-19,  17),   S(-30, -23),   S( -3,   2),   S(-10,   6),   S(-25,  12),   S(-27, -57),   S(-33, -19),
            S( -3, -48),   S( 11, -13),   S( -5,  30),   S( 31,  42),   S(-24,  41),   S( -6,  21),   S(-29,  14),   S(-24, -38),
            S(  7,  39),   S( 31,  72),   S( 24,  59),   S( 43,  30),   S(  3,  50),   S( 16,  38),   S( 41, -20),   S(-10,  -8),
            S( 23,  69),   S(-20,  83),   S( 33,  79),   S( 50,  50),   S( 86,  30),   S( 17,  34),   S( 15,  16),   S(  1,  24),
            S(110, -24),   S( -4,  43),   S( 88,  55),   S( 56,  41),   S( 55,  52),   S(-36,  77),   S( 66,  -9),   S(-24,  44),
            S( 86,  22),   S(-17, -36),   S( 34,   9),   S( 71,  63),   S( 58,  52),   S( 48,  15),   S(-22,   5),   S( -1,  18),
            S(-97, -131),  S( -3,  -8),   S( 16,  20),   S( 28,  42),   S(  8,  54),   S( 36,  43),   S(-51, -16),   S(-12,  -2),

            /* knights: bucket 1 */
            S( 25, -11),   S(-64,  17),   S(-52,  40),   S(-60,  37),   S(-29,  40),   S(-18, -13),   S(-43,   5),   S(-17, -22),
            S(-47,  47),   S(-47,  71),   S(-33,  39),   S(-18,  31),   S(-24,  39),   S(-19,  41),   S(-18,  23),   S(-27, -42),
            S(-53,  21),   S(-12,  12),   S(-26,  24),   S(-17,  60),   S(-20,  51),   S(-31,  28),   S(-60,  50),   S(-16,  15),
            S(-20,  67),   S(  3,  47),   S( -9,  53),   S(-31,  72),   S(-25,  69),   S(-32,  80),   S(-12,  25),   S(-28,  49),
            S( 49,  -7),   S( -5,  17),   S( 16,  58),   S(-13,  68),   S( 32,  47),   S( -2,  68),   S( -8,  52),   S( -2,  60),
            S( 18,  60),   S( 41,   7),   S( 96,  31),   S(117,  45),   S( 46,  56),   S(-10,  64),   S( 44,  31),   S( 44,  46),
            S( 11,   9),   S( 58, -18),   S( 51,   6),   S( 31,  48),   S(  5,   2),   S( 12,  19),   S( 15,  49),   S(-13,  21),
            S(-186, -32),  S( 28,   9),   S(-20, -54),   S(-36,   7),   S( -6, -32),   S( 26,  38),   S( 22,  40),   S(-67,  11),

            /* knights: bucket 2 */
            S(-69,  44),   S(-38,  41),   S(-37,   9),   S(-26,  35),   S(-22,  24),   S(-60,  33),   S(-35,  10),   S(-64, -21),
            S(-39,  36),   S( 12,  29),   S(-34,  42),   S(-19,  38),   S(-33,  49),   S(-32,  23),   S( -2,  18),   S(-34,  18),
            S(-44,  60),   S(-28,  40),   S(-33,  41),   S(-33,  77),   S(-17,  57),   S(-33,  36),   S(-38,  39),   S(-27,  20),
            S( -9,  73),   S(-27,  69),   S(-29,  97),   S(-40, 104),   S(-42,  83),   S(-13,  70),   S(-17,  52),   S(-10,  52),
            S(-25,  83),   S(-19,  83),   S( -8,  79),   S(  4,  62),   S(-35,  94),   S( -1,  91),   S(-34,  75),   S( 16,  28),
            S(-23,  70),   S(  0,  78),   S(-42, 114),   S( 48,  42),   S( 52,  56),   S(117,  26),   S( 82,  30),   S( -5,  15),
            S( 15,  56),   S(-82,  92),   S( 78,  12),   S( -6,  27),   S(-14,  52),   S( 10,   2),   S( 30,  40),   S( 27,  -7),
            S(-36, -11),   S( 35,  73),   S(-20,  95),   S( -5, -10),   S(-10,  -8),   S(-68, -61),   S( 49,  15),   S(-137, -68),

            /* knights: bucket 3 */
            S(-71,  56),   S(-20, -15),   S(-12,  28),   S(-20,  31),   S( -6,  23),   S(-21,  17),   S(-25,   6),   S(-26, -40),
            S(-33,   3),   S( -9,  39),   S( -9,  38),   S(-11,  36),   S(-12,  41),   S(  9,  14),   S( 18,   1),   S(  8, -10),
            S(-22,  37),   S(-19,  37),   S(-11,  53),   S( -7,  80),   S(  3,  75),   S(-15,  61),   S( -3,  43),   S( -9,  26),
            S( -2,  32),   S( -4,  70),   S(  2,  85),   S(-15, 106),   S( -1, 105),   S(  5, 105),   S( 19,  87),   S(  0,  66),
            S(-27,  81),   S(  1,  81),   S(  8,  97),   S( 21, 113),   S(  2, 101),   S(  5, 123),   S(-35, 119),   S( 35, 120),
            S(-22,  79),   S( 10,  84),   S( 22,  87),   S(  9, 114),   S( 55, 102),   S( 93, 133),   S( 44,  96),   S( 10, 133),
            S(-15,  68),   S(-40,  88),   S(-29,  99),   S( 45,  74),   S( 42,  94),   S( 69,  64),   S( 16,  11),   S( 68,  -2),
            S(-184,  74),  S(-46, 101),   S(-37, 109),   S( 26,  84),   S( 22, 122),   S(-58,  85),   S(  9, -26),   S(-55, -106),

            /* knights: bucket 4 */
            S(  4,  27),   S( 10,  13),   S(-28,   7),   S(-10,  15),   S(-58,  -1),   S(-30, -39),   S( 51, -17),   S( -7,  -5),
            S( 13,  39),   S( -1, -40),   S( -2,  27),   S( -9,  -9),   S( -4, -21),   S( 56, -62),   S( -5,  27),   S(-44, -11),
            S( 15, -12),   S( 11,  -6),   S( 55,  -5),   S(110, -30),   S( 50,   8),   S( 47, -35),   S(-18, -16),   S( -3, -42),
            S(-39, -63),   S(  2,  -2),   S( 47, -17),   S( 71,  14),   S( 33,  19),   S( -6,  22),   S(-27,  46),   S(-26,  -7),
            S(-10, -55),   S(-12,  28),   S( 68,  -7),   S( 53,  49),   S( 82,  -7),   S(  4,  37),   S(-23,  -4),   S(-46,  31),
            S( -3, -10),   S(-20, -34),   S( 49,  10),   S( 91,  40),   S( -3,  41),   S( 14,  32),   S(-24,   5),   S( 33,  32),
            S(-15, -18),   S(-22,  -9),   S(  1,   5),   S( 31,  31),   S( 25,  33),   S(  9,  23),   S(-11,  -6),   S(-36, -34),
            S(  7,  21),   S(-17, -40),   S(-11, -35),   S( 12,  21),   S(  6,   4),   S(-12,  10),   S(  6,  21),   S(  7,  10),

            /* knights: bucket 5 */
            S( 21,  40),   S( 19,  21),   S(-22,  32),   S(-30,  21),   S(  2,  10),   S(  1,  21),   S(-44, -21),   S( 19,  10),
            S(  6,  15),   S( 51,  22),   S( 55,  -2),   S(  8,   6),   S( 49, -10),   S(  6,   8),   S(  8,  42),   S(-70,  21),
            S(-52,  28),   S( -1,  -9),   S( 16,  15),   S( 13,  17),   S(  9,  31),   S(  0,  20),   S( 18,   3),   S(-16,   8),
            S( 29,  13),   S( 45, -26),   S( 65,   7),   S( 81,  -3),   S(104,   9),   S( 87,  -2),   S(-30,  21),   S( 22,  16),
            S( 89,   4),   S( 21,   5),   S(102,  -4),   S(124,  -5),   S( 94,  -9),   S( 82,  17),   S( 44,   4),   S( 55,  10),
            S( -7, -24),   S( 39, -20),   S( 15, -23),   S(-19,   2),   S( 39,  -6),   S( 60,  -1),   S( -5,  22),   S( 19,  22),
            S( -3,   3),   S(-34, -25),   S( -1, -33),   S(-18, -37),   S(-17, -45),   S( 11,  -8),   S( 14,  40),   S(-12,  -8),
            S(-33, -16),   S(-29, -52),   S( -4, -25),   S(-27, -16),   S( 13,  -5),   S(-11,  26),   S( 14,  22),   S(  5,  27),

            /* knights: bucket 6 */
            S(-14, -16),   S(-32,  32),   S(-14,   7),   S(-17,  23),   S(-37,  28),   S( 10,  32),   S( -7,  64),   S(-25,  -1),
            S( 26, -17),   S( 31,  17),   S( 26, -17),   S( 34,   3),   S( 55,   7),   S(-18,  26),   S(  9,  36),   S(-34,  38),
            S( 40,   6),   S( 47,   6),   S( 31,  18),   S( 60,  29),   S( 13,  38),   S(-47,  37),   S( 65,   3),   S(-31,  29),
            S( 13,  49),   S( 69,   2),   S( 81,   8),   S( 72,  21),   S( 92,   0),   S( 83,   9),   S( 55,  -1),   S(-12,  37),
            S(-12,  40),   S( 79,  -1),   S(136,  -3),   S(141,   2),   S(105,   2),   S( 89,  19),   S(180,  -9),   S( 31,  39),
            S(  3,  35),   S( 20,  13),   S( 36,  19),   S( 72,  10),   S( 41, -13),   S( 61,   1),   S( 37,   7),   S(  2, -25),
            S( 13,  46),   S( 53,  27),   S( 53,  42),   S( -6, -10),   S( 17,  38),   S( 46, -23),   S( -6, -13),   S(  6,  31),
            S( 28,  26),   S( 24,  22),   S(  3,  18),   S(  9,  22),   S( 34, -26),   S( 10, -19),   S( -7,   6),   S(-30, -49),

            /* knights: bucket 7 */
            S(-20, -47),   S(  0, -27),   S( 41, -20),   S( -7,  13),   S( 10,  -6),   S(-24,  18),   S( -3,  12),   S(  0,  28),
            S(-24, -33),   S(  2, -23),   S(-19,  12),   S( -6,  -5),   S( 12,   4),   S( 46,   9),   S( -2,  14),   S(-47,  26),
            S(-21, -32),   S(-67,   2),   S( 25,  -9),   S( 28,  12),   S( 69,   6),   S( 66,   1),   S( 28,   8),   S( 33,   3),
            S(-25,   6),   S( 66,  -5),   S( 54,   0),   S(125,  -1),   S(116,  -5),   S( 80,  34),   S( 85,   5),   S( 70,   4),
            S( 22, -21),   S(  8,   4),   S( -1,  33),   S( 84,   9),   S(129,  15),   S(195, -27),   S(206,  -6),   S( 12,  23),
            S(-26,   4),   S( 55,  19),   S( 19,  26),   S( 53,  20),   S(111,   0),   S( 90,  -8),   S( 40, -12),   S( -9, -38),
            S(-19,  13),   S( 26,  -3),   S( 44,  16),   S( 58,  39),   S( 86,   3),   S( 33,   6),   S(-16, -47),   S(-13, -23),
            S(-27, -30),   S( -9, -24),   S( 10,  27),   S(  9,  14),   S(-11,  22),   S( 43,  23),   S( 10,  -9),   S( -5, -18),

            /* knights: bucket 8 */
            S( -4,   8),   S(  9,   3),   S(  8,  -3),   S(-17, -72),   S( -5, -31),   S(-14, -50),   S( 17,  14),   S( -4, -28),
            S(-11, -45),   S( -9, -38),   S( -1, -65),   S(-28, -16),   S(  5,  -3),   S( -4, -39),   S(  4, -16),   S( -7, -11),
            S(-19, -77),   S(-13, -31),   S(  2, -57),   S( 18, -31),   S( 16, -34),   S( 37, -10),   S( -8, -55),   S( 13,   7),
            S(-21, -79),   S(-13, -64),   S( 10, -28),   S( 32,  -7),   S( 26, -51),   S( -5, -32),   S(-16, -61),   S( -6, -20),
            S( -7, -25),   S( -1, -51),   S(  4, -18),   S( 15, -43),   S( -5, -27),   S(-13, -52),   S( 13,  18),   S( -6, -28),
            S(  3,   2),   S( 16, -18),   S(  5, -16),   S( -9, -34),   S( -9, -15),   S(  8, -19),   S( -3, -36),   S(-11,  -5),
            S(  0,   1),   S(-12, -80),   S(-14, -48),   S(  4,   8),   S( -4, -62),   S( 15,  -9),   S( -4,  -1),   S(  3,  16),
            S(  3,   3),   S( -5, -10),   S( -6, -17),   S(  8,   7),   S( -5,   3),   S(  5,  15),   S(  5,  12),   S( -3,  -2),

            /* knights: bucket 9 */
            S(-17, -67),   S( -3, -49),   S(-12, -73),   S( -5, -49),   S( -3, -23),   S(-18, -43),   S( -9, -46),   S( -2, -14),
            S( -2, -17),   S(-19, -73),   S( -6, -89),   S(  0, -69),   S(  1, -58),   S(-22, -88),   S(  2, -30),   S(-13, -27),
            S(-11, -58),   S(-17, -33),   S(-12, -25),   S( 21, -53),   S( -5, -23),   S( 20, -31),   S(  0,  -7),   S( -4, -36),
            S(-19, -59),   S(-10, -57),   S(  2, -36),   S(  0, -60),   S( 12, -27),   S( 19, -27),   S(-12, -64),   S( 16,  11),
            S( 15,  25),   S(-12, -47),   S(  5, -52),   S( 18, -54),   S(  6, -50),   S( 16, -18),   S( -2, -16),   S( -7, -17),
            S(-13, -62),   S(-25, -88),   S( -4, -20),   S( 18, -45),   S(  6, -13),   S(  7, -30),   S( -3, -33),   S( -4,  -8),
            S(-15, -34),   S( -5, -22),   S( -7, -21),   S( -9, -37),   S(  5, -16),   S( 17,  43),   S(-13,   1),   S( -6,   7),
            S(  5,  15),   S(  1, -15),   S( -8,   4),   S(  3, -17),   S( -7, -33),   S(  3,   6),   S(  5,   8),   S( -1,   9),

            /* knights: bucket 10 */
            S( -5, -42),   S(-10, -39),   S(-12, -41),   S( -6, -26),   S(-25, -75),   S(  3, -75),   S(-13,  12),   S( -4, -20),
            S( -3, -42),   S( 18, -14),   S( -8, -60),   S(-20, -92),   S( -1, -41),   S(-14, -95),   S( -9, -12),   S(  0,  22),
            S(  4,   0),   S(-12, -27),   S(  2, -48),   S( 30, -35),   S(-14, -51),   S( 18, -47),   S( -6, -54),   S( -7,  -4),
            S( -5, -58),   S(  1, -67),   S( 18, -32),   S(  3, -44),   S(  7, -25),   S( -2, -53),   S( -4, -77),   S( -9, -55),
            S(-20, -59),   S(  5, -34),   S( 24, -30),   S( -3, -10),   S( 20, -15),   S( -9, -67),   S(-13, -16),   S(  5, -24),
            S(  1,   6),   S(-14, -42),   S(  8, -18),   S(  7,   3),   S( -9, -28),   S( -7, -46),   S(  4, -17),   S(-17, -27),
            S(  7,  -9),   S( -1,  -9),   S( -7, -66),   S( 12, -48),   S(-15, -35),   S(-15, -54),   S( -3,  -1),   S( -8, -10),
            S( -2,  -2),   S(  2,  -6),   S(  2,  19),   S(-10, -45),   S(  2, -21),   S(-15, -57),   S(  2, -16),   S(  0,  10),

            /* knights: bucket 11 */
            S( -7, -45),   S(-23, -61),   S( -6, -35),   S( -2, -32),   S(-45, -60),   S(-19, -16),   S( -7, -31),   S(  4,  41),
            S(  3, -19),   S(-25, -67),   S( -3, -85),   S( 54, -16),   S( 11,  -5),   S( -6, -39),   S( -9, -60),   S( -4, -39),
            S(  1, -29),   S( -2, -42),   S( -4, -43),   S( 21,  -6),   S(  2, -13),   S( 26,  -6),   S( -3, -45),   S(-10, -41),
            S(-11, -45),   S( 24,  -8),   S( 17, -46),   S( 19, -15),   S( 67, -22),   S(  4,  -1),   S( 14, -29),   S(-10, -41),
            S( -6,   0),   S(-11, -70),   S( -7, -23),   S( 24, -21),   S( 36,   9),   S( 10, -15),   S(-10, -98),   S(-12, -34),
            S( -5, -20),   S( -2, -38),   S( 17,  -7),   S( 11, -29),   S(  9,   2),   S(  4, -12),   S(  5, -18),   S(  1,  18),
            S( -4, -21),   S( -7,   9),   S(-23, -30),   S(  8, -24),   S(  8, -20),   S( 23, -44),   S( -8, -25),   S( -6, -24),
            S( -4, -15),   S(  3,  11),   S( -5, -20),   S( -1,   6),   S(  5,   1),   S( -2, -15),   S(  4,  13),   S(  1,   1),

            /* knights: bucket 12 */
            S(-15, -50),   S( -5, -38),   S( -5, -34),   S( -3,   4),   S( -2,   5),   S(  3,   3),   S( -2,  -1),   S( -1,  -3),
            S( -5, -31),   S(  2,   5),   S( -6, -10),   S( -5,  -8),   S(-10, -50),   S(  7,  -4),   S(  4,   6),   S( -3, -15),
            S(  1,  -4),   S( -7, -47),   S( -8, -47),   S(-11, -87),   S(  1,  -9),   S(  5,  15),   S( -4, -26),   S( -5, -15),
            S(  3,   8),   S( -2, -69),   S(-11, -33),   S(  7, -18),   S(  8, -23),   S(  7,  15),   S( 10,  17),   S( -1,  14),
            S( -8, -23),   S( -9, -47),   S(  6,  -5),   S(  1, -27),   S( -5, -10),   S(  1, -15),   S(  0, -16),   S( -7, -20),
            S( -8, -16),   S( -3, -12),   S( -4, -16),   S(-10, -51),   S( -8, -34),   S( -8, -18),   S(  9,  13),   S( -2, -11),
            S( -3,  -8),   S( -3,  -4),   S( -5, -16),   S( -2,  -3),   S( -1,   6),   S(-17, -47),   S( -6, -17),   S( -3, -11),
            S(  0,   2),   S(  5,  33),   S(  1,   0),   S( -1,  -7),   S(  1,   5),   S(  3,   8),   S(  0,  -1),   S( -2,  -6),

            /* knights: bucket 13 */
            S( -3,  -6),   S( -6, -25),   S( -8, -44),   S( -7, -44),   S( -9, -30),   S( -5, -10),   S( -4, -18),   S(  4,  18),
            S( -7, -33),   S( -6, -32),   S(  4,  -3),   S(-10, -34),   S( -6, -39),   S( -1, -22),   S( -4,  -2),   S( -6, -16),
            S(  2,   0),   S(  4,  20),   S(  2, -13),   S( -6, -46),   S(  5,  27),   S( -2,  -7),   S(  4, -17),   S( -1,  -6),
            S( -4, -11),   S(  0,  -3),   S( -3, -23),   S(  0, -21),   S(  6, -32),   S( 17,  -6),   S(  5,  -8),   S( 16,  22),
            S(  1,  20),   S(-10, -59),   S( -1, -58),   S( 10, -46),   S(-11, -78),   S(  2, -31),   S( -4, -36),   S( -1, -16),
            S( -6, -20),   S(  2,  -4),   S(-15, -32),   S( 11,   5),   S( -5, -27),   S(  0, -18),   S( -4, -14),   S(  2,   7),
            S(  3,   7),   S(  2,   4),   S( -6, -19),   S( -7, -19),   S(  0,  11),   S( -5, -10),   S(  1,   5),   S(  0,  -3),
            S(  1,   4),   S( -1,   3),   S(  1,   1),   S(  0,  -3),   S(  2,  -2),   S(  3,   9),   S(  0,   2),   S(  1,   3),

            /* knights: bucket 14 */
            S( -2,  -8),   S(  0, -12),   S( -4, -16),   S( -5, -12),   S( -1, -32),   S( -5,  -1),   S(  8,  27),   S( -3, -19),
            S( -4, -20),   S( -5, -30),   S(  1, -21),   S(  1, -21),   S( -1, -21),   S(  5,  12),   S(-10, -29),   S(  4,  11),
            S( -1, -13),   S( -8, -47),   S(  9,   7),   S( -6, -32),   S( -1, -21),   S( -1, -23),   S( -4, -21),   S(  0,   2),
            S(  0, -11),   S( -8, -57),   S(-15, -42),   S(  1,  -5),   S(  7,  -4),   S(-10, -33),   S( -1,   0),   S(  1,  13),
            S(  3,   1),   S(-11, -43),   S(  0, -15),   S(  1, -17),   S( 10,  24),   S(  3,  -5),   S( -1,   3),   S(  4, -10),
            S(  1,   7),   S(  6,  12),   S(  0,  32),   S(  0,  15),   S( -3, -12),   S(  0,  -1),   S(  0,   3),   S( -2, -15),
            S( -1,   3),   S( -4, -15),   S(  3,  -5),   S(  2,  37),   S(  0,   9),   S( -6, -16),   S(  3,   5),   S(  1,   0),
            S(  1,  -1),   S(  2,  10),   S( -1,  -7),   S(  1,  10),   S(  1,  -1),   S( -2,  -4),   S(  4,  15),   S(  0,   4),

            /* knights: bucket 15 */
            S( -4, -20),   S( -2, -11),   S( 11,  38),   S(  0,  11),   S( -5, -23),   S( -9, -42),   S(  0, -13),   S( -2, -14),
            S( -1,  -5),   S(  7,  15),   S( -4, -15),   S(  3,  22),   S(  8,   3),   S( -1, -28),   S( -3, -18),   S(  1,  -1),
            S(  1,  -8),   S( -8, -36),   S( -8, -18),   S( 12, -27),   S(-17, -82),   S(  7,   0),   S( -5, -31),   S(  0,  -4),
            S( -3, -21),   S( -2, -12),   S( -6, -32),   S( -4, -18),   S(  3, -48),   S( -2, -35),   S(  7,  19),   S( -1,   4),
            S(  2,   6),   S( 14,  41),   S( -4, -36),   S(  1,  25),   S( 25,  16),   S(  3,   1),   S( -5, -42),   S(  4,   1),
            S(  5,  20),   S( -2,  -2),   S( -2,   6),   S(-21, -66),   S(-17, -44),   S( 14,  52),   S(  3,  19),   S(  6,  18),
            S( -5, -12),   S( -7, -24),   S(  8,  27),   S(  8,  20),   S( -2,  -8),   S(  4,  15),   S(  1,   8),   S(  3,   3),
            S(  0,   3),   S( -2, -16),   S(  2,  10),   S( -1,  -3),   S(  1,   8),   S( -3,  -4),   S(  1,   7),   S(  1,  10),

            /* bishops: bucket 0 */
            S( 27, -16),   S(-34,  49),   S( -2, -11),   S(-22,  -7),   S( -6,   6),   S( 16, -21),   S( 64, -82),   S( 20,  18),
            S(-30,  -5),   S( 10, -22),   S( -8,  21),   S( 11,   4),   S( 17, -10),   S( 75, -30),   S( 41,  22),   S( 54,   1),
            S(  4,   5),   S(-10,  45),   S( 16,   2),   S( 19,  -4),   S( 43, -12),   S( 49,  13),   S( 53,  -8),   S( 30,  -6),
            S( -8, -18),   S( 39, -50),   S( 27,   4),   S( 82, -40),   S( 82,  16),   S( 55,  14),   S( 19, -20),   S( 39,  -6),
            S( 45, -17),   S( 48, -39),   S( 81, -20),   S(107,  -6),   S(125, -15),   S( 24,   3),   S( 54,  16),   S( 17, -13),
            S( 39,  32),   S( 75, -14),   S( 80,  -4),   S( 49,  13),   S( 11,  18),   S( 40,  35),   S( 33,   7),   S( 11,  13),
            S(-58, -80),   S( 46,   7),   S(111,  50),   S( 13, -11),   S(  4, -31),   S( 21,  16),   S( 15,  29),   S(-36,  60),
            S(-11, -43),   S(  6,  37),   S( 14, -23),   S(  1,  -4),   S(-48, -30),   S(-12,  -2),   S(-20,  37),   S(-33, -25),

            /* bishops: bucket 1 */
            S(-38,   9),   S( 12, -11),   S(-20,  17),   S( 29, -13),   S(-34,  19),   S( 18,   6),   S( 22,   4),   S( 50, -36),
            S(-10, -26),   S( -5, -11),   S( 16, -10),   S(-12,   3),   S( 44, -24),   S( -1,  -2),   S( 69, -15),   S(  3,   0),
            S(-19, -14),   S( 38, -16),   S(  2, -15),   S( 36,  -7),   S(  2, -12),   S( 55,  -9),   S(  1, -11),   S( 99, -37),
            S( 42, -20),   S( 45, -27),   S( 42, -11),   S( 14,   1),   S( 75, -11),   S( 15, -10),   S( 64, -29),   S( -5,  11),
            S( 10, -11),   S( 74, -12),   S(  3,   8),   S(115, -22),   S( 49,  -1),   S( 95, -43),   S( 11,   9),   S( 41, -27),
            S( 87, -34),   S( 71, -21),   S( 77,  -3),   S( 60, -44),   S(121, -44),   S( -1,  -6),   S(  4,  25),   S(  3, -14),
            S(-12, -63),   S( 11, -19),   S(-12, -36),   S( 25,   4),   S( 60,   3),   S( 20,   9),   S(-10,  16),   S( -8,   6),
            S(-29, -28),   S(-38,  -4),   S(-17, -63),   S(-34,   7),   S( 20, -10),   S( 25, -20),   S( 43, -37),   S(-36, -33),

            /* bishops: bucket 2 */
            S( 36, -22),   S(  5,  -5),   S(  9,   7),   S(-14,  16),   S(  4,  18),   S(-11,  18),   S( 34, -21),   S( 22, -12),
            S( 25, -12),   S( 15,  -8),   S(  9,  -7),   S( 18,  12),   S(  1,  15),   S( 14,   7),   S( 15,  -6),   S( 16, -30),
            S( 32,  -3),   S( 23,   0),   S(  6,  15),   S(  8,   3),   S(  6,  23),   S(  3,  -2),   S(  2, -16),   S( -1,   4),
            S( 16, -16),   S( 30,  -5),   S( 13,   1),   S( 45,  20),   S(  6,  19),   S( -6,  26),   S(-13,   2),   S( 19,  -3),
            S( 20,   1),   S( 14,  19),   S( 73, -11),   S( 29,  20),   S( 37,  24),   S(  7,  21),   S( 13,  23),   S( 25,  10),
            S( -8,  16),   S( 15,  23),   S( 18,   0),   S(107, -34),   S( 54,   4),   S(125,   4),   S( 73,   9),   S( 18, -37),
            S( 19,  22),   S( 19,  -5),   S(  4,  18),   S( 30,  17),   S(-83, -16),   S(-24,  -6),   S( -5,  28),   S( -5, -24),
            S(-76,   0),   S(-42,  25),   S(  0,   7),   S(-42,  26),   S( -2,  -1),   S(-31,  -6),   S( -8, -39),   S(-32, -19),

            /* bishops: bucket 3 */
            S( 26,  18),   S( 57, -31),   S( 16, -17),   S( 14,   3),   S( 12,  23),   S(  4,  41),   S(  7,  35),   S(  3,   4),
            S( 39,  -2),   S( 26,  -9),   S( 26,  12),   S( 22,  12),   S( 25,  19),   S( 27,  17),   S( 14,  21),   S( 37,  -8),
            S(  2,   9),   S( 29,  38),   S( 27,  28),   S( 28,  27),   S( 20,  34),   S( 22,  23),   S( 15,   3),   S( 22,  31),
            S(-10,   9),   S( 11,  16),   S( 33,  40),   S( 47,  34),   S( 42,  28),   S( 22,  19),   S( 38, -14),   S( 37, -33),
            S( 24,   6),   S( 20,  46),   S( 17,  43),   S( 49,  57),   S( 51,  45),   S( 50,  20),   S( 20,  33),   S(-17,  39),
            S( 18,  15),   S( 40,  42),   S( 35,  17),   S( 25,  29),   S( 55,  35),   S( 63,  57),   S( 57,  64),   S( 16,  95),
            S(-10,  59),   S( 21,  53),   S( 36,  13),   S( 10,  49),   S( 13,  43),   S( 15,  83),   S(-30,   3),   S( 23, -63),
            S(-51,  66),   S(-60,  51),   S(-80,  59),   S(-33,  60),   S(  0,  37),   S(-100,  57),  S( 15,   0),   S(  5,  26),

            /* bishops: bucket 4 */
            S(  8,  49),   S(-37, -35),   S(-86,  30),   S(-68,   4),   S(-30, -27),   S(-24, -30),   S( -9,   7),   S(-31, -51),
            S( -5,  12),   S(-22, -10),   S( 29, -20),   S(-45, -13),   S(-69,  12),   S(-13, -54),   S(-21, -24),   S(-34, -66),
            S( 14,  12),   S(-18,  -3),   S( 29, -24),   S( 13, -20),   S( 24, -27),   S(-28,  -7),   S(-14, -22),   S(-87, -37),
            S( 49, -42),   S( 53, -27),   S( 53, -39),   S( 29, -11),   S(-14,  -2),   S( 31,  -3),   S(-29,  -2),   S(-11, -22),
            S( 14,  -1),   S( -9, -83),   S( 45, -32),   S( 59, -41),   S( 15, -18),   S(  9, -13),   S(-13,  10),   S(-31, -13),
            S(-33, -56),   S(-38, -62),   S( 10, -36),   S( 52,   1),   S( -5,  12),   S( 24,  17),   S(-34,  -3),   S( -5,  13),
            S(  2,  15),   S( -2, -22),   S(  0, -48),   S(-14, -60),   S(-21, -15),   S( 10,  -1),   S(-11,   6),   S(  6,  32),
            S(  0, -18),   S( 16, -24),   S( -2, -12),   S( -6, -56),   S(-11, -49),   S( 15,   3),   S(-13,  27),   S(  6,  19),

            /* bishops: bucket 5 */
            S(-62,   6),   S(-14,  -8),   S(-65,  24),   S(-41,  15),   S( -5,  -5),   S(-32,  -1),   S(-24,   3),   S(-29,  -3),
            S(-12,   7),   S(-18,   7),   S( 24,  -7),   S( 16,  -8),   S(-19,  -4),   S(  6,  -1),   S(-14,  -5),   S(  7, -18),
            S( 14,   7),   S(-43,  -4),   S( 26, -12),   S(-25,   3),   S(  6,   5),   S(-17, -12),   S(  6,  -8),   S(-14,  16),
            S( 13, -11),   S(-19,  -3),   S( 25, -15),   S( 43,  -2),   S( -2,  -5),   S( 60, -17),   S(-59,  15),   S(-54,   1),
            S( 15, -38),   S( 11,   0),   S(  2, -33),   S(-13, -50),   S( 43, -21),   S( 27, -20),   S( 61,  -7),   S(-47,   7),
            S(-31,  -6),   S( -3,  -5),   S( 37, -39),   S(-23,  -6),   S( 40, -29),   S( 42, -22),   S(-39,  18),   S(-33,  32),
            S(-26, -30),   S(-28, -15),   S(-42, -22),   S( -1,   3),   S( -8, -27),   S( 15,  -4),   S( 35,   9),   S(-31,   6),
            S( -7, -16),   S(-21, -30),   S(-12, -14),   S(  1, -20),   S(-30,  12),   S(  3,   3),   S(-32,  -8),   S(-18,  23),

            /* bishops: bucket 6 */
            S(-66,  -1),   S(-54,  15),   S(-36,   3),   S( -4,   3),   S(-21,  14),   S(-27,  18),   S(-58,  34),   S(-57,  33),
            S(  2,   2),   S(-15, -20),   S(-53,  20),   S( -6,   8),   S(-60,  25),   S(-29,  12),   S(-63,  41),   S(-27,  11),
            S(  3,  -1),   S(-57,  15),   S( 12, -15),   S(-50,  29),   S(-13,  27),   S(-27,   9),   S(-22,  -4),   S(-26,  34),
            S(-15,  -2),   S(-40,   2),   S(-22,  13),   S( 85, -15),   S( 50,   3),   S(  8,   1),   S(-10, -12),   S(-23,  18),
            S(-57,  19),   S(-25,  19),   S( 23,  -8),   S(100, -25),   S(  2, -22),   S( -8, -19),   S( 24, -15),   S(-16,   2),
            S(-67,  45),   S(-26,  17),   S( -6,   6),   S( 22, -13),   S(-21, -15),   S(-41,  -8),   S(-10,  18),   S(-14,   0),
            S(-36,  29),   S(-26,  25),   S( -2,   0),   S( -6, -12),   S(-36,   1),   S(  9, -14),   S( -1,  -1),   S(-50,  19),
            S(-12,  29),   S(-31,  -1),   S(-29,  14),   S(-15,   6),   S( -5,  11),   S( 16, -33),   S(-38,   6),   S( -7, -25),

            /* bishops: bucket 7 */
            S( -5, -32),   S(-67, -16),   S(-52, -30),   S(-54,  -3),   S(-47,  -9),   S(-49,   9),   S(-71, -13),   S(-67, -10),
            S( 29, -49),   S( -5, -32),   S( 13, -29),   S(-33, -22),   S(-26,   5),   S(-41,   8),   S(-45,   3),   S(-24,  32),
            S(-43, -30),   S(-40,   1),   S(-17,   4),   S( 14, -12),   S( 15, -19),   S( -4, -14),   S(-82,  16),   S(-65,  53),
            S(-35, -22),   S(-48,  -2),   S(-45,  16),   S( 19, -14),   S( 78,  -6),   S( 14, -15),   S( 34, -22),   S(-14, -28),
            S(-62,   2),   S( 26,  -1),   S( 14, -23),   S(  2,   4),   S( 53, -18),   S( 47,  -9),   S(-24, -44),   S(-57, -10),
            S(-53,  -2),   S(-19,  25),   S( 35, -15),   S(-42,  -2),   S(-17,  -2),   S( 76, -34),   S( 41, -24),   S(-66, -82),
            S(-35,   1),   S(-54,  37),   S(-28,   7),   S( 19,  -5),   S( -4, -25),   S( -8, -40),   S( 20,  -2),   S(-12, -15),
            S(-29,  -5),   S(-39,   7),   S(-23,  17),   S(-23, -11),   S(-20, -17),   S(-16, -17),   S(  5, -37),   S(  7,  -6),

            /* bishops: bucket 8 */
            S( 15,  93),   S(-16,  14),   S( -9,   1),   S(-14,  20),   S(  7,  25),   S(-25, -28),   S(-25, -53),   S( -7, -16),
            S( -4,  33),   S( 14,  31),   S( -4,  10),   S(  5, -38),   S(  8, -19),   S(  3, -11),   S(-34, -50),   S( -1,  -3),
            S(-18, -48),   S(-17, -77),   S( 25,  54),   S( 19,   2),   S( 31,  16),   S( 11,  14),   S(-26, -52),   S(-35, -49),
            S( -4,  -8),   S( 21,  80),   S( -3,   7),   S(-16,  39),   S( 24,  14),   S( 26,  30),   S(-15,  -1),   S(  8,  14),
            S( 11,  83),   S( 30,  97),   S( -5,  25),   S(  3,  -1),   S(  0,  46),   S(-27,  19),   S( -8, -50),   S(  7,  29),
            S(-11, -40),   S( -1,  38),   S( -5,  38),   S( 13,  44),   S( 19,  25),   S(-10,  40),   S(  4,  26),   S(  3,  31),
            S( -8,  19),   S(-15, -52),   S(  9,  15),   S( -8,  32),   S(-13,  17),   S( 10,  59),   S(  8,  76),   S(-23,  -4),
            S( -2,  -9),   S(  2,  -5),   S( -7, -28),   S( -5,  22),   S(  8,  51),   S(  5,  15),   S( -3,  51),   S( 14,  98),

            /* bishops: bucket 9 */
            S(  4,  51),   S(-17,  25),   S(-23,   8),   S(-38,  -3),   S(-33,  -9),   S(-18, -45),   S( -6, -22),   S(-15, -23),
            S(-17, -19),   S(-17,  30),   S(  2,  19),   S( -8, -19),   S(-12, -34),   S(-17, -28),   S(-26,   1),   S(-13, -47),
            S(-10, -21),   S( 13,  15),   S( -4,   4),   S( 11,  15),   S(-14,   5),   S( -1, -21),   S( 13,   8),   S(-23, -40),
            S(-17,  24),   S( -2,   6),   S(  5,  17),   S( -5,   8),   S(-31,  30),   S( -7, -25),   S( -6,  22),   S(  3,  11),
            S(  4,  35),   S( -6,  40),   S( 17,   4),   S( -4,   4),   S(-13,  20),   S(  1,  24),   S( 19,  29),   S(-24, -29),
            S(  2,  22),   S(-19,   4),   S( -5,  36),   S(  7,  11),   S(-33,  11),   S(-20,  27),   S( -1,  10),   S(-18,   8),
            S( -3,  37),   S( -6,  19),   S( -7,  32),   S(-11,  24),   S( -7,  17),   S(-11,  19),   S(  0,  24),   S(  2,  56),
            S(  1,  37),   S( -2,   8),   S( -3,   5),   S( -6,  14),   S( -4,  36),   S(-14,  25),   S( -8,  37),   S( 20,  87),

            /* bishops: bucket 10 */
            S( -2,  17),   S( -2,  25),   S(-30, -30),   S(-30, -34),   S(-69, -16),   S(-54, -52),   S( -9,  28),   S( -1,  37),
            S( -3,  -1),   S(  4,   0),   S(-11, -24),   S(-30, -45),   S(-23, -31),   S(-28, -29),   S(-22, -45),   S( -6,  29),
            S(-21, -33),   S(-16, -28),   S(-29, -10),   S(-14,   5),   S( -3, -28),   S(  2, -15),   S(-36, -14),   S( -9, -43),
            S(-10, -13),   S( -7, -14),   S(-25, -20),   S(-11,  -5),   S( -3,   6),   S( -5,  20),   S(  6,  18),   S(-25, -17),
            S( -9,   3),   S(-55,  17),   S(-13,  -6),   S(  1,   7),   S( -4,  10),   S( -5,  27),   S(-15,  13),   S( 11,  26),
            S(-16,  16),   S( -1,  18),   S(-16,   7),   S(-19,  11),   S( -9, -13),   S(-13,  28),   S(-13,  15),   S(  5,  36),
            S( -7,  31),   S(-12,   7),   S(-18,  20),   S( -9,  40),   S( -7,   4),   S(-22, -32),   S( -6, -10),   S(  3,  55),
            S(-10,  50),   S( -3,  50),   S(  2,  56),   S(-14,   4),   S(-17,  18),   S(  5,   4),   S( 10,  18),   S( -5,  -2),

            /* bishops: bucket 11 */
            S( 18,  25),   S(  2,  -2),   S(-12,  35),   S(-33, -19),   S(-38, -39),   S(-15, -19),   S(-24,  17),   S(-31,  40),
            S( -5,  21),   S(  9, -17),   S(  2,  15),   S( 13, -36),   S( -9, -28),   S(-39, -11),   S(-37,  17),   S(  1,  25),
            S(-13, -36),   S( -9, -29),   S( 20,  -2),   S(  8, -26),   S(-21, -19),   S( 19,  18),   S( 11, -13),   S(  6, -21),
            S( -5,   3),   S(-14, -42),   S(  5,  17),   S( -5, -25),   S(  7,  14),   S( 18,  46),   S( 10,  46),   S(-15, -49),
            S(-31, -20),   S(-18, -21),   S(-30,  30),   S(-35,  45),   S(-10,  -2),   S(  1,  20),   S( -7,  30),   S(  9,  48),
            S(-16, -12),   S(-13,  27),   S(-27,  30),   S( -9,  16),   S(-21,  46),   S(-25,  31),   S(  5,   8),   S(-11,   1),
            S(-19,  10),   S(  1,  64),   S(  2,  39),   S( -5,  34),   S(  2,  15),   S( -6,  -5),   S(-15, -36),   S( -9,   9),
            S( 14, 108),   S(-15,  26),   S( -2,  38),   S( -8,  17),   S(  2,  47),   S(-13,  -2),   S(  1,  11),   S(  6,  20),

            /* bishops: bucket 12 */
            S( -5, -16),   S( -9, -37),   S( -6,   1),   S(  2,  29),   S( -2,  -8),   S(-10,  -2),   S(  0,   7),   S(  1,   4),
            S(  1,  -5),   S(  9,  43),   S(  1,  -1),   S( 11,  27),   S(  8,  20),   S(  8,   6),   S( -8, -14),   S( -1,  -9),
            S(  7,  44),   S(  9,  48),   S( 12,  30),   S( 26,  28),   S( -2,  15),   S( -7, -47),   S(  8,   8),   S(  1,  12),
            S(  6,  79),   S( 11,  54),   S(  1,  31),   S( 12,  23),   S( 15,  11),   S( -3, -19),   S( -3,   4),   S(  8,  25),
            S( 10,  11),   S( 10,  34),   S(  1,  21),   S( 17,  52),   S( 10,  25),   S(  7,  52),   S(  0, -10),   S(  9,  26),
            S(  3,   6),   S(-12, -18),   S( -8,  -3),   S( -5,  -6),   S(  9,  47),   S(  9,  34),   S( -4, -21),   S( -5, -23),
            S( -2,   3),   S(  5,  11),   S(  1,   0),   S(  6,  34),   S( -1,  20),   S( 10,  37),   S( 10,  23),   S( -1,  18),
            S(  0,   6),   S(  3,  17),   S( -1,   3),   S( -3,  -8),   S( -3, -17),   S(  5,  16),   S(  8,  62),   S(  9,  50),

            /* bishops: bucket 13 */
            S( -7, -17),   S( -4, -16),   S( -8, -20),   S(-12, -13),   S(  2,  17),   S(-10, -31),   S(-12, -34),   S( -6, -27),
            S(-10,  -4),   S(  0,  17),   S( -4,   1),   S( 12,  61),   S( -6,   9),   S(  9,  12),   S(  6, -10),   S( -1,  -7),
            S( -3,   8),   S( 27,  94),   S(  5,  38),   S( 16,  34),   S(  0,   9),   S( 12,  31),   S( -3,  -3),   S( -5, -24),
            S( 17,  93),   S( 17, 103),   S(  4,  37),   S(-25,   7),   S( 10,  42),   S( -1,   1),   S(  8,  52),   S( -1,  17),
            S(  5,  68),   S(  1,  15),   S(  9,   3),   S(  1,  24),   S( -5,  18),   S(  0,  54),   S(  9,  39),   S( -8,   5),
            S(  0,  18),   S(-11, -15),   S( -2,  34),   S( 15,  41),   S(-11,  41),   S( -4,  -2),   S( -8,  12),   S(  9,  33),
            S(  7,  31),   S( -4, -13),   S( -4,  -5),   S(  2,  30),   S( -1,   9),   S( 11,  67),   S(  6,  47),   S(  2,  50),
            S(  0, -11),   S( -1,   6),   S(  0,   7),   S(  3,  17),   S(  6,  30),   S(  1,   3),   S(  1,  30),   S( 16,  62),

            /* bishops: bucket 14 */
            S(-13, -25),   S(  8,  30),   S(  9,  12),   S(  0,  51),   S(-17, -32),   S( -4,   1),   S( -6, -23),   S( -8, -40),
            S(  1,   2),   S(  0, -12),   S(  2,  28),   S( -7,   5),   S( 14,  60),   S( -1,   5),   S(  1,  10),   S( -7, -12),
            S( -7, -12),   S(  2,  24),   S( -4, -10),   S(  1,   3),   S(  8,  40),   S(  8,  32),   S(  0,  58),   S(  5,  39),
            S( -3,  12),   S( 10,  18),   S(-19, -12),   S(-11,  59),   S( -3,  31),   S( 11,  35),   S( 12,  81),   S(  4,  55),
            S(  3,  24),   S( -5, -20),   S(-15,   1),   S( -5,  15),   S(-11, -18),   S(  1,  51),   S( 22,  59),   S(  0,  35),
            S( -5,  -2),   S( 20,  50),   S( -2,  38),   S( 10,  27),   S(  3,  31),   S( -3,  13),   S( -9, -28),   S(  5,   9),
            S(  4,  32),   S( 13,  52),   S(  1,  39),   S(  5,   7),   S(  1,  24),   S(  1,   4),   S( -7, -42),   S(  2,  15),
            S( 11,  69),   S(  3,  58),   S(  2,   2),   S(  4,  29),   S( -6, -10),   S( -3, -20),   S( 12,  31),   S(  0,   2),

            /* bishops: bucket 15 */
            S( -1,  -6),   S( -6, -10),   S( -8, -18),   S( -4, -12),   S(-12, -29),   S( -2, -25),   S( -5, -41),   S( -7, -26),
            S(  8,  31),   S( -1,  -1),   S(  3,  12),   S(  1,  15),   S( 16,   8),   S(-12, -18),   S(  2,  19),   S( -5, -13),
            S( -2, -10),   S(-11, -46),   S(  3,  33),   S( 14,  27),   S( -1,  -6),   S(  5,   8),   S( 13,  72),   S(  5,  51),
            S( -2,  -7),   S(  7,  14),   S(  9,  54),   S( -9,   3),   S(  3,  29),   S(  5,  27),   S( 11,  55),   S(  7,  60),
            S(-14, -23),   S( -7, -19),   S( -8,  13),   S( 28,  91),   S( 10,  66),   S(  9,  28),   S(  3,  11),   S( -3,  -6),
            S(  2,   4),   S( -4,  20),   S(  7,  58),   S( -9,  -7),   S( 11,  41),   S(  1,  24),   S( -4,  -9),   S(  2,  11),
            S(  2,  27),   S(  3,  14),   S(  0,  37),   S(  7,  29),   S(  6,  50),   S( -3, -11),   S(  2,  16),   S( -5, -11),
            S( -1,  11),   S(  2,  38),   S(  9,  51),   S(  7,  15),   S(  2,  20),   S(  3,  17),   S(  2,  17),   S(  4,  25),

            /* rooks: bucket 0 */
            S(-11,  20),   S( 15,   8),   S(  6,  12),   S( 16,   8),   S(-15,  63),   S( -5,  40),   S(-32,  66),   S(-52,  56),
            S( -1,  19),   S( -6,  52),   S(-28,  41),   S(  6,  29),   S(-12,  69),   S( -7,  44),   S( -7,  16),   S(-39,  86),
            S( 14, -16),   S( -1,  16),   S(-12,  43),   S( 13,  38),   S(-26,  59),   S(-24,  45),   S(-27,  52),   S( -1,  32),
            S(-12,  17),   S( 21,  27),   S(-46,  53),   S( 20,   7),   S(  3,  75),   S( -7,  54),   S(-18,  49),   S(-21,  57),
            S( 31, -36),   S( 24,  25),   S( 12,  21),   S(  7,  49),   S( 24,  55),   S( 32,  65),   S( 44,  72),   S( 13,  68),
            S( 48,   0),   S( 64,  28),   S( 80,  35),   S(110,  34),   S( 26,  66),   S( 32,  62),   S( -4,  85),   S(-15,  86),
            S( 50,  50),   S( 30,  57),   S(102,  65),   S( 64,  48),   S( 90,  50),   S( 10,  75),   S(  1,  85),   S(-32, 111),
            S( 29,  -5),   S( 28,  34),   S( 21,  47),   S( 56,  19),   S( 85,  51),   S( 69,  32),   S( 32,  38),   S( 68,  -3),

            /* rooks: bucket 1 */
            S(-63,  49),   S(-37,  22),   S(-20,  34),   S(-40,  41),   S(-35,  53),   S(-41,  54),   S(-35,  65),   S(-85,  78),
            S(-50,  34),   S(-25,   8),   S(-34,  33),   S(-27,  31),   S(-37,  20),   S(-44,  35),   S(-47,  26),   S(-27,  33),
            S(-48,  38),   S(-36,  18),   S(-36,  27),   S(-11,  17),   S(-52,  31),   S(-60,  39),   S(-68,  53),   S(-41,  69),
            S(-85,  62),   S(-30,  33),   S(-13,  39),   S(-33,  35),   S(-49,  50),   S(-57,  74),   S(-32,  40),   S(-85,  84),
            S(-55,  54),   S(-22,  14),   S( 16,  32),   S( 46,   9),   S(-21,  31),   S(-24,  79),   S(-24,  65),   S(  4,  73),
            S( 42,  31),   S( 41,  16),   S( 63,  21),   S( 19,  58),   S(  7,  39),   S( -2,  55),   S( 24,  55),   S( 33,  64),
            S( 54,  64),   S( 70,  17),   S( 22,  22),   S( 23,  60),   S( 49,  46),   S( 12,  71),   S( 38,  78),   S( 51,  87),
            S( 43,  -1),   S( 16,   3),   S( -6, -15),   S(-26, -25),   S( 56,   0),   S( 55,  23),   S( 60,  35),   S( 98,  21),

            /* rooks: bucket 2 */
            S(-65,  81),   S(-50,  74),   S(-44,  79),   S(-31,  46),   S(-28,  56),   S(-42,  49),   S(-29,  40),   S(-80,  80),
            S(-61,  71),   S(-62,  87),   S(-46,  72),   S(-45,  60),   S(-52,  63),   S(-57,  44),   S(-32,  38),   S(-47,  58),
            S(-56,  80),   S(-46,  81),   S(-58,  62),   S(-32,  58),   S(-49,  61),   S(-35,  41),   S(-30,  42),   S(-19,  46),
            S(-54,  99),   S(-53,  92),   S(-62,  84),   S(-82,  80),   S(-47,  64),   S(-55,  65),   S(-44,  69),   S(-29,  61),
            S(-36, 103),   S(-30,  94),   S(-11,  89),   S(-29,  70),   S(-20,  68),   S(  4,  59),   S(-45,  75),   S(-13,  82),
            S(  7,  96),   S( 17,  91),   S( 43,  73),   S( -3,  68),   S( 59,  38),   S( 72,  45),   S( 91,  32),   S( 83,  65),
            S( 70,  87),   S( 13, 104),   S( 67,  66),   S( 44,  56),   S( 23,  26),   S( 86, 101),   S(-36, 116),   S( 16, 108),
            S( 40,  66),   S( 55,  67),   S( 60,  47),   S( 11,  47),   S(-18,  46),   S( 55,  59),   S(-12,  64),   S( 49,  47),

            /* rooks: bucket 3 */
            S(-21, 104),   S(-17, 108),   S(-18, 136),   S( -7, 126),   S( -1,  95),   S(  1,  83),   S( 21,  60),   S(-16,  44),
            S( -9, 101),   S(-17, 115),   S(-11, 129),   S( -8, 130),   S( -9,  93),   S(  4,  65),   S( 39,  40),   S(  7,  63),
            S(  3, 102),   S(-15, 118),   S(-16, 120),   S( -8, 128),   S( -3,  93),   S( -5,  89),   S( 23,  70),   S( 15,  54),
            S(-11, 130),   S(-15, 145),   S(-25, 144),   S(-18, 140),   S(-18, 112),   S( -8,  96),   S( 11,  86),   S(-23,  83),
            S( -9, 143),   S(-22, 151),   S(  4, 154),   S(  0, 147),   S(  7, 114),   S( -3, 120),   S( 28, 101),   S( 20,  92),
            S(  1, 151),   S( 20, 138),   S( 22, 149),   S( 38, 137),   S( 87,  98),   S(101,  96),   S( 73, 117),   S( 39,  96),
            S( 11, 158),   S(  2, 161),   S( 10, 168),   S(  3, 166),   S( 33, 135),   S( 94,  95),   S( 94, 141),   S(190,  89),
            S(102,  45),   S( 58,  97),   S( 47, 131),   S( 39, 125),   S( 47,  95),   S( 60, 106),   S( 54,  69),   S(135,  50),

            /* rooks: bucket 4 */
            S(-25, -24),   S(  0, -21),   S(  9, -28),   S(-27,   8),   S(-52,  23),   S(-22,  40),   S(-21,  10),   S(-74,  36),
            S(-34, -36),   S(-28, -16),   S(-21, -18),   S(-28, -30),   S( -4,   4),   S( -7,  14),   S(-21,   4),   S(-22,   7),
            S(  3, -16),   S(-27, -24),   S(-48,  -1),   S(-26, -19),   S(-30,   0),   S(-45,  21),   S(-34,   2),   S(-55,  -8),
            S(-29, -37),   S( 47,  -4),   S( 33, -35),   S( 50, -37),   S( 31,   0),   S( -1,  -1),   S(-14, -14),   S(-28,   6),
            S( -4, -10),   S(  4, -28),   S( 48,  16),   S( 69, -18),   S( 84,  -3),   S( 49,  20),   S( 30,  -5),   S( 21,  20),
            S( -7, -29),   S(  6,  -1),   S( 34, -13),   S( 32,  29),   S( 43,  38),   S( 35,  20),   S( 34,   9),   S(  8,  37),
            S(  4,  -8),   S( 50,  40),   S( 30, -18),   S( 66, -18),   S( 69,  -3),   S( 49, -16),   S(  7, -25),   S( 31,  10),
            S(  2, -13),   S( 35,   2),   S( 44, -30),   S( 12, -18),   S( 58,  10),   S( 20,  -8),   S(  2,  10),   S( -9,   1),

            /* rooks: bucket 5 */
            S(-52,  30),   S(-12,  -7),   S(-16,  17),   S(  5,  19),   S(-39,  33),   S(-29,  33),   S(-33,  45),   S(-50,  32),
            S(-18,   2),   S(-14, -19),   S( -1, -21),   S(-21,   6),   S(-14,  -2),   S( -4, -15),   S(-50,  16),   S(-19,  46),
            S(-93,  41),   S(-24,  -9),   S(-11, -26),   S(-74,  22),   S(-66,  17),   S( 31, -30),   S(-46,  15),   S(-54,  21),
            S(-52,  21),   S(  0,  -4),   S( 24,  -6),   S( 29,  14),   S( 24,  -5),   S(-39,  36),   S(-26,  21),   S(-13,  30),
            S( 38,  11),   S(-22,  15),   S( 26,  45),   S( 18,  -9),   S( -7,  11),   S( 89,  -6),   S( 34,  10),   S( 43,  32),
            S( 25,   6),   S(  7,   0),   S(  0,  11),   S(-13, -21),   S( 25,  17),   S( 20,  24),   S( 93,   0),   S(  8,  54),
            S( 41,  -6),   S( 56, -15),   S(  6,   0),   S( 48,  19),   S( 51,  -8),   S( 72, -24),   S( 55, -16),   S( 70,   1),
            S( 26,  15),   S( 18,   6),   S( 55, -16),   S(  8,  21),   S( 60,   1),   S( 28,  22),   S( 39,  12),   S( 21,  38),

            /* rooks: bucket 6 */
            S(-85,  57),   S(-51,  27),   S(-37,  30),   S(-18,  15),   S( 13,   7),   S( 34, -12),   S( -1,   5),   S(-21,  -7),
            S(-95,  36),   S(  0,   7),   S(-35,  25),   S(-19,  11),   S(  9,  -4),   S(-55,  12),   S(-54,  15),   S(-46,  22),
            S(-92,  44),   S(-25,  20),   S(-15,   8),   S(-11,   5),   S(-18,   9),   S( 20,  -4),   S(-20,  -4),   S(-12, -10),
            S(-47,  35),   S(-28,  35),   S( 19,   2),   S( 32,   3),   S( 20,  11),   S(-18,  27),   S(-26,   7),   S(  1,  24),
            S(-11,  45),   S( 58,  13),   S( 77,   8),   S( 64,   7),   S( 61,  -8),   S( 36,  13),   S( 67, -16),   S( 64,   3),
            S( 92,   9),   S( 71,   9),   S(129,  -3),   S( 47, -10),   S( 10, -12),   S( 32,  52),   S( 40,  -8),   S( 98,  11),
            S( 28,  14),   S(117, -17),   S(126, -33),   S( 76, -29),   S( 69,  -5),   S( 27,   8),   S( 56, -14),   S( 78, -20),
            S( 54,   0),   S( -6,  41),   S(-16,  31),   S( 42,   1),   S( 44,   7),   S( 45,  30),   S( 91,   5),   S( 46,  16),

            /* rooks: bucket 7 */
            S(-103,  44),  S(-72,  37),   S(-61,  42),   S(-56,  30),   S(-38,  17),   S(-17, -11),   S(-49,  28),   S(-66,  -2),
            S(-96,  34),   S(-57,  34),   S(-45,  22),   S(-74,  41),   S(-42,   2),   S( -2,  -8),   S(-14,  33),   S( 12, -49),
            S(-57,  30),   S(-97,  41),   S(-20,   4),   S(-76,  43),   S(-66,  20),   S(-72,  47),   S( 63, -16),   S(-20, -19),
            S(-97,  48),   S(-12,  18),   S(-12,  14),   S( 50,  -5),   S(-15,  -1),   S( 51, -10),   S( 75,  -6),   S( 56, -25),
            S( -1,  31),   S( 17,  27),   S( 58,   6),   S( 56,  10),   S(135, -35),   S(123, -25),   S(103, -14),   S(-42, -13),
            S( 22,  27),   S( 45,  16),   S(111,  -6),   S(121, -13),   S( 94, -10),   S( 57,  18),   S( 19,  17),   S(-21, -27),
            S(-11,  15),   S(  6,  10),   S( 61,  -2),   S( 88, -23),   S(110, -27),   S(111, -30),   S( 92,   9),   S( 49, -31),
            S(-18,   2),   S( -4,  23),   S( 24,  18),   S( 58,  13),   S(-19,  16),   S( 39,   9),   S( 94,   2),   S( 27,  -7),

            /* rooks: bucket 8 */
            S(-13, -80),   S( 17, -66),   S( 33, -49),   S( 29, -33),   S(-10, -48),   S(-17, -27),   S( 18, -14),   S( -7, -40),
            S(-18, -101),  S(-21, -50),   S( 15,  -4),   S(-19, -66),   S(-17, -79),   S( -5, -31),   S(-23, -49),   S(-28, -31),
            S( -3, -11),   S( -4, -38),   S( 13, -13),   S(  0,  13),   S(  4,  15),   S( 23,  -3),   S(  3,  21),   S(-15, -20),
            S(  8,  -3),   S(  0,  -6),   S( 13,   7),   S( 11,  -9),   S(  7,  16),   S( 18,  -6),   S(  9,  21),   S( -9, -21),
            S(-15, -78),   S( 18,  23),   S( 25, -17),   S( 22,  20),   S( 12,   9),   S(  2, -27),   S( 12,  33),   S(  7,  19),
            S( 13,  -8),   S( 17, -13),   S( -4, -24),   S(  3, -16),   S(  9,  47),   S(-17,  -6),   S( -5, -11),   S( -6,  12),
            S(  7,  -4),   S( 20, -25),   S( 27,  -5),   S( 16,   7),   S(  9,  23),   S( 29,  43),   S( 13,   9),   S( -9,  20),
            S( -7,   3),   S( 20, -15),   S( 21, -12),   S( 32,  26),   S( -6,   5),   S(  2,   2),   S(  6,   8),   S( 11,  -1),

            /* rooks: bucket 9 */
            S(  7, -84),   S( -8, -99),   S( 10, -100),  S(  5, -70),   S( 26, -76),   S(  5, -62),   S( -3, -44),   S( -3, -67),
            S(-37, -89),   S(-18, -83),   S(-15, -70),   S(-20, -53),   S( -5, -79),   S(-11, -11),   S(-25, -61),   S(-20, -62),
            S(-10, -30),   S(  1, -37),   S( 10,   2),   S(-11, -38),   S(  5, -67),   S(-19, -22),   S(-14, -19),   S( -6,  15),
            S( 16, -41),   S(  0, -35),   S(  6, -13),   S(  2,  -5),   S(  0, -65),   S( 22, -24),   S(-11, -16),   S( -9, -62),
            S( 21, -45),   S( -4, -34),   S( -4, -68),   S( -8, -33),   S(-13, -67),   S(-17, -14),   S( -3, -30),   S( -2, -56),
            S( 13, -40),   S(-38, -61),   S( -3, -34),   S( 19, -20),   S( 13, -19),   S( -5, -12),   S(-21, -31),   S(-16, -23),
            S(  4, -14),   S(  8, -33),   S(  9, -63),   S(-13,  -6),   S(  4, -50),   S( -1, -47),   S( 19, -18),   S( -8, -57),
            S( -6, -20),   S( -7, -21),   S( 12, -28),   S( 12, -18),   S( -8,  -6),   S( 14,   5),   S(-21, -14),   S(  1,  -4),

            /* rooks: bucket 10 */
            S( -1, -86),   S(-33, -52),   S( 29, -88),   S( -6, -84),   S( 27, -106),  S(-11, -99),   S( 22, -100),  S(-19, -57),
            S(-36, -77),   S(-26, -60),   S(-26, -50),   S(-40, -84),   S(-26, -89),   S(  3, -80),   S( -5, -77),   S(-34, -85),
            S(-13, -39),   S(-27, -37),   S(-12, -50),   S(-40, -68),   S( -2, -31),   S( -9, -59),   S( -4, -64),   S(-10, -42),
            S(-18, -35),   S(-20, -48),   S(-24, -56),   S(-10, -14),   S(  4, -23),   S( -6,  -6),   S(-10, -89),   S( -6, -64),
            S( -1, -37),   S( 11, -31),   S( 10, -51),   S(  0, -71),   S( 18, -11),   S( 10, -17),   S( 11, -78),   S(-27, -72),
            S( -4, -33),   S(-19, -18),   S( 12, -55),   S(  8, -78),   S( 17, -37),   S( 10, -36),   S( -8, -49),   S( -3, -49),
            S(-36, -32),   S(-24, -46),   S( -7, -51),   S(  5, -53),   S( 28, -28),   S( -5, -25),   S(-12, -53),   S(  2, -43),
            S(-16, -24),   S( -9,  -7),   S( -8,   7),   S( -8, -38),   S( -5,  -4),   S(-19, -33),   S(  6, -36),   S(-13, -16),

            /* rooks: bucket 11 */
            S(-13, -56),   S(  2, -42),   S(  0, -52),   S( -1, -15),   S(-24, -51),   S( 44, -75),   S( 27, -58),   S( 14, -88),
            S(  2, -39),   S(-30, -34),   S(-43, -34),   S(-45, -40),   S(-42, -20),   S( -8, -22),   S(-14, -58),   S(-14, -81),
            S(-32,  -1),   S(-14,  -4),   S(-11,  12),   S(-30, -12),   S( 10, -24),   S(  5, -17),   S(  8, -29),   S(-14,  -1),
            S( -7, -30),   S( -3, -31),   S(  9,  -5),   S( 26,  -3),   S( 13, -35),   S(-26, -82),   S(  5,   4),   S(-10, -38),
            S(-11, -39),   S( 29, -31),   S(  2, -22),   S( -5,  -4),   S( 24, -22),   S( 12, -49),   S( 19,   9),   S(  0, -32),
            S(-14, -27),   S(-18, -16),   S( 35, -31),   S(  7, -18),   S(-10, -12),   S( 33, -22),   S( 34,  -1),   S( 12, -46),
            S( -3, -19),   S(-26, -41),   S(  9, -11),   S( -9, -23),   S( 15, -20),   S( 50, -25),   S( 26, -23),   S(  8, -40),
            S(  5,  -9),   S( 42,  36),   S( -6,  -5),   S( 22,  22),   S(-14, -16),   S( 32, -13),   S( 37, -34),   S(-32, -14),

            /* rooks: bucket 12 */
            S(-34, -121),  S(  4,   3),   S( 17, -33),   S(-30, -67),   S( -5, -31),   S( 23, -16),   S(-17, -55),   S(-22, -56),
            S(  2,   0),   S(  2,  -6),   S(  0,  -3),   S( 13,  13),   S( 20,  -4),   S( 13, -14),   S( -8, -20),   S(-10, -23),
            S(  0,  -3),   S( 11,  35),   S( 12,  13),   S(  3, -21),   S(  9, -28),   S( 14, -25),   S(  9,  29),   S(  9,  11),
            S( -7, -21),   S( -3,  -2),   S( -4, -10),   S( 18,   4),   S( 14,  23),   S(  6,  -4),   S(  8,  18),   S(  0,  10),
            S( 14,  22),   S(  5, -13),   S(  7, -12),   S(  4, -17),   S( 10,  17),   S( -5, -12),   S(  1,   0),   S(  3,   9),
            S(  2, -29),   S( -2, -37),   S( -5, -45),   S( -3,  -8),   S( -5, -27),   S( -2, -13),   S(  8,  15),   S(-11, -40),
            S( -7,  -4),   S(-22, -36),   S(  9,  -1),   S( 11,  25),   S( -1, -21),   S(  1, -13),   S(  7,  29),   S(-13, -28),
            S( -1,  -6),   S( -5, -25),   S(  8, -18),   S( 14,   7),   S( -3, -15),   S( -2,  -7),   S( -8, -19),   S(  0,  15),

            /* rooks: bucket 13 */
            S(-17, -44),   S(-14, -75),   S( -9, -31),   S(  2, -36),   S(-16, -81),   S( -9, -67),   S(-30, -63),   S(-11, -42),
            S(-12, -49),   S( -7, -44),   S(  0,  -1),   S( -8, -47),   S( 18,   0),   S( 11, -11),   S( 20, -34),   S(-12, -61),
            S( -8, -47),   S(-14, -61),   S(  5,   0),   S( 12, -16),   S( 12, -15),   S( 18, -22),   S( 12, -38),   S( -4, -80),
            S( 13,   8),   S( -6, -24),   S( -2, -35),   S(  6, -28),   S( 16, -15),   S(  6,   6),   S( -4, -22),   S( -3, -14),
            S( -9, -43),   S(  6, -60),   S( 13, -24),   S(  8, -14),   S( -2, -53),   S(  5,  -5),   S(  5,   9),   S( -2, -21),
            S( -4, -30),   S( -7, -46),   S(-11, -66),   S( -2, -32),   S(  3, -83),   S(  9,  -5),   S(-14, -47),   S( -9, -47),
            S( -5, -28),   S(  3, -32),   S(-10, -28),   S(  0, -35),   S( -3, -59),   S(  7, -21),   S(  4, -12),   S( -2, -20),
            S(  8,   4),   S(-14, -56),   S(-11, -43),   S( 12, -11),   S( -4, -47),   S(  5, -13),   S( -5, -25),   S(  2, -10),

            /* rooks: bucket 14 */
            S(  4, -37),   S(-35, -59),   S(-11, -45),   S( -5, -68),   S(-23, -79),   S( -1, -40),   S(-33, -111),  S(-15, -51),
            S( 16,  -5),   S( 15, -25),   S(  6, -35),   S( -2, -18),   S(  0, -17),   S( -2,  -5),   S(  9, -13),   S( -8, -69),
            S( -7, -25),   S( -4, -44),   S(-12, -77),   S(  3, -19),   S( 14,  13),   S(  0, -17),   S( 16, -39),   S( -8, -64),
            S( -7, -38),   S( -2, -19),   S(  9,  36),   S( -1, -38),   S( -8, -37),   S(  2, -12),   S(  3, -27),   S( -7, -43),
            S( 10, -12),   S(  7,  22),   S(  1, -17),   S( -4, -63),   S( 11, -14),   S( 20,  -4),   S(  4, -58),   S( -2, -46),
            S(  4,  14),   S(  6,  12),   S(  2, -30),   S( -1, -81),   S(  1, -67),   S( -4, -74),   S( -9, -100),  S(-11, -43),
            S(-22, -52),   S(  8,   8),   S(-16, -77),   S(-16, -81),   S(  0, -44),   S(  3, -34),   S(-16, -60),   S(-10, -33),
            S(-14, -51),   S( -2, -19),   S(  0, -29),   S(  0, -62),   S(-14, -55),   S(-29, -121),  S( -7, -61),   S( -2, -12),

            /* rooks: bucket 15 */
            S(-16, -61),   S(-16, -55),   S(-21, -51),   S(-20, -74),   S(-11, -53),   S(-16, -56),   S( -6, -30),   S(-14, -70),
            S( 23,  36),   S( -2, -32),   S( -2, -31),   S( -7, -35),   S(  3, -26),   S( -2,  -4),   S( 12,  19),   S(  0, -13),
            S( -6, -10),   S( -3, -29),   S(  0,  -7),   S(  5, -22),   S( 18,   1),   S(-11, -44),   S( 12,  16),   S( -1, -11),
            S(  0,   5),   S( -7,  -7),   S( 22,  46),   S( -4,   2),   S( 11,  12),   S( -2, -25),   S(  7, -19),   S(  7,  -3),
            S(  4,   3),   S(  4,  -2),   S( -5, -26),   S(  4,   1),   S( 10,  10),   S( -9, -65),   S( -1, -53),   S(  2, -33),
            S(-12, -32),   S(  3,  -2),   S( 10,  23),   S( -3, -42),   S(-13, -49),   S(  3, -58),   S(  2, -42),   S( -4, -20),
            S(  2, -19),   S(  0, -12),   S(  7,   1),   S( -4, -20),   S(  1,  13),   S( -3, -33),   S(  1, -30),   S(-10, -40),
            S(-12, -27),   S(  0,   0),   S(  6,   4),   S(  2,  -3),   S(  0,   2),   S( -9, -55),   S(  6, -25),   S(-22, -66),

            /* queens: bucket 0 */
            S( -9, -41),   S( -8, -67),   S(-34, -66),   S( -2, -102),  S( -8, -67),   S( -3, -79),   S(-69, -19),   S(  2, -18),
            S(-17, -40),   S( 13, -85),   S(  3, -65),   S(-10, -45),   S( -7, -35),   S(  0, -71),   S(-22, -77),   S(-40, -41),
            S( -4,  25),   S(  4, -46),   S( 11, -43),   S(-17, -19),   S( -1, -10),   S( -4, -22),   S(-33,  -4),   S(-75, -81),
            S(-36,  29),   S(  4, -12),   S(-14,  22),   S( -8,  37),   S(-15,  37),   S(-24,  35),   S(-49,  26),   S(-14, -67),
            S(-48,   3),   S(-23,  74),   S( -8,  70),   S(-22,  68),   S(-15,  68),   S(-26,  51),   S(-43,  55),   S(-32,  -8),
            S(-36,  24),   S( -8,  68),   S( 35,  63),   S(-22, 102),   S(-67,  91),   S(-42,  39),   S(-90,  12),   S(-63,  -6),
            S(  0,   0),   S(  0,   0),   S( 20,  26),   S( -3,  65),   S(-37,  33),   S(-64,  54),   S(-97,  70),   S(-97,   4),
            S(  0,   0),   S(  0,   0),   S( 22,  31),   S( -1,   8),   S(-48,  61),   S(-57,  23),   S(-55,  23),   S(-42,   1),

            /* queens: bucket 1 */
            S(  9, -34),   S(-13, -27),   S( 24, -97),   S( 23, -116),  S( 28, -103),  S( -7, -51),   S( -2, -31),   S(-16,  15),
            S(-33,  -6),   S( 22, -27),   S( 33, -61),   S( 20, -22),   S( 23,  -3),   S(  0, -31),   S(-38,  19),   S(-41,  -3),
            S( 46, -62),   S( 17,   1),   S( -1,   0),   S( 26,   8),   S( -8,  58),   S( 14,  45),   S(  0,  -8),   S(  9, -10),
            S( 23, -35),   S( 27,  39),   S(  2,  50),   S(  6,  88),   S(-11, 104),   S( -8,  50),   S(  1,  52),   S(-21,  31),
            S( 26,  15),   S( 10,  45),   S( 10,  93),   S(-18,  97),   S( 42,  79),   S( 27,  50),   S(-29,  98),   S(  3,  46),
            S( 50,  27),   S( 44,  64),   S( 96,  83),   S(103,  92),   S( 44,  83),   S(-13, 110),   S( 31,  68),   S(  5,  31),
            S( 79,  13),   S( 48,  31),   S(  0,   0),   S(  0,   0),   S(-10,  55),   S(-35,  83),   S(-20,  93),   S(-39,  51),
            S( 93,  29),   S( 92,  31),   S(  0,   0),   S(  0,   0),   S( 25,  47),   S( 28,  40),   S( 94,  -1),   S(-16,  57),

            /* queens: bucket 2 */
            S( 22, -36),   S( 37, -34),   S( 33,   3),   S( 43, -42),   S( 48, -57),   S( 29, -93),   S(  0, -52),   S( 40, -18),
            S(  3,   6),   S( 13,  34),   S( 42, -20),   S( 36,  21),   S( 48, -22),   S( 15, -12),   S( 14, -11),   S(  9,  63),
            S( 33,  33),   S( 22,  27),   S( 13,  86),   S( 11,  55),   S( 23,  54),   S( 23,  51),   S( 28,  34),   S( 20,  61),
            S( 20,  31),   S( 20,  75),   S(  8, 102),   S(  3, 117),   S( 23,  82),   S( 11,  79),   S( 22,  72),   S( 20, 103),
            S(  6,  75),   S( -3,  60),   S(-18, 144),   S( 11, 126),   S( 24, 153),   S( 75,  68),   S( 38, 119),   S( 53,  98),
            S(-34,  87),   S(-28, 113),   S(-11, 126),   S( 61,  84),   S( 86,  83),   S( 97, 124),   S(133,  60),   S( 23, 140),
            S(-11,  93),   S(-29, 104),   S(-11, 123),   S( 97,  69),   S(  0,   0),   S(  0,   0),   S( 21, 140),   S( 49,  99),
            S(-15,  64),   S( 19,  55),   S( 63,  24),   S( 30,  67),   S(  0,   0),   S(  0,   0),   S( 89,  74),   S( -9, 117),

            /* queens: bucket 3 */
            S(-52,  51),   S(-13,  21),   S( -9,  37),   S( -1,  39),   S(-15,  30),   S(-19,  -4),   S(  2, -55),   S(-32,  26),
            S(-45,  51),   S(-25,  60),   S(-13,  60),   S(-11,  88),   S( -9,  61),   S(-10,  38),   S( 23,  -9),   S( 25, -58),
            S(-36,  58),   S(-29,  96),   S(-26, 120),   S(-38, 143),   S(-22, 111),   S(-19, 109),   S( -1,  62),   S( -7,  26),
            S(-39,  69),   S(-49, 115),   S(-43, 163),   S(-23, 167),   S(-36, 163),   S(-29, 124),   S( -4, 106),   S(-19,  96),
            S(-43,  99),   S(-54, 162),   S(-36, 152),   S(-36, 196),   S(-26, 175),   S(-20, 191),   S(-24, 172),   S(-36, 146),
            S(-52,  99),   S(-32, 123),   S(-66, 190),   S(-60, 200),   S(-47, 202),   S( 20, 169),   S(-12, 187),   S(-35, 205),
            S(-100, 139),  S(-83, 151),   S(-66, 188),   S(-89, 199),   S(-80, 211),   S(-12, 153),   S(  0,   0),   S(  0,   0),
            S(-142, 193),  S(-102, 159),  S(-76, 145),   S(-72, 158),   S(-41, 158),   S( 16, 114),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-35,   9),   S(-56, -59),   S(  2,  13),   S(-46, -43),   S(-34, -19),   S( -7,   1),   S(-39, -40),   S( -8,   4),
            S( -7,  10),   S( 11,   7),   S(-14, -17),   S(-31, -28),   S(-36,  15),   S(-43,  12),   S(-45, -22),   S(-15, -36),
            S(-15,  32),   S( 30, -28),   S( -8,  17),   S( -2,   5),   S( 46,  24),   S( 10,  15),   S( -1, -20),   S( 16,  30),
            S(  3,  -8),   S( 20,   4),   S( 15,  -6),   S( 12,  39),   S( 63,  45),   S( 34,  28),   S(-47,  -8),   S( -8,  29),
            S(  0,   0),   S(  0,   0),   S( 54,  38),   S( 57,  61),   S( 20,  59),   S(-13,  11),   S( -7, -20),   S( -4,  -8),
            S(  0,   0),   S(  0,   0),   S( 50,  47),   S( 61,  52),   S( 28,  52),   S( 28,  25),   S(-14,  10),   S(  2, -10),
            S( 18,   9),   S( 41,  52),   S( 61,  25),   S( 50,  27),   S( 54,  30),   S(-12,  10),   S(  8,  32),   S(-73, -26),
            S( 47,   9),   S(  4,  -1),   S( 46,  24),   S( 66,  52),   S( -1,  -2),   S(-34, -14),   S(-48, -23),   S( -6,   2),

            /* queens: bucket 5 */
            S( 23,  -2),   S( 22,  15),   S( -7,  -5),   S(-50, -14),   S( 26, -36),   S( 32,  37),   S( -4,   0),   S(-11, -24),
            S( 14,  10),   S( 17,   6),   S(-14, -15),   S( -7,  20),   S( 13,   9),   S(-56, -31),   S( 41,  31),   S( 21,  27),
            S( 16,  23),   S( 39,  11),   S( 42,  11),   S( 18,  37),   S(-15,  17),   S( 32,   6),   S( 22,  18),   S(-10,  -1),
            S( -4, -36),   S( 61,  27),   S( 43,  27),   S( 40,  44),   S( 79,  24),   S( 15,  35),   S( 25,  42),   S(-65,  -4),
            S( 48,  28),   S( 61,  10),   S(  0,   0),   S(  0,   0),   S( 32,  38),   S( 36,  35),   S( 32,  49),   S(-13,  27),
            S( 48,  26),   S( 55,  59),   S(  0,   0),   S(  0,   0),   S( 42,  45),   S( 75,  49),   S( 39,   3),   S( 49,  35),
            S( 58,  39),   S( 70,  26),   S( 59,  68),   S(  6,  18),   S( 70,  74),   S(110,  73),   S( 51,  45),   S( 10,  27),
            S( 27,  53),   S( 65,  40),   S( 99,  58),   S( 35,  24),   S( 68,  33),   S( 56,  47),   S( 31,  22),   S( 28,   4),

            /* queens: bucket 6 */
            S( 31,  31),   S(-35, -33),   S(  8,   9),   S( 11,  -9),   S(  9,   0),   S(-36,   6),   S(-23,   5),   S( -8,   1),
            S(-11,   8),   S( 36,  -4),   S( 33,  54),   S( 45,  15),   S( 30,  -6),   S(  5, -10),   S(-41,   9),   S( 19,  39),
            S(-39,  35),   S( 17,  30),   S( 11,  26),   S( 25,  19),   S( 11,  50),   S( -1,  12),   S( 51,  41),   S( 65,  57),
            S(  2,  31),   S(-30,  24),   S( 25,  16),   S( 54,  36),   S( 69,   8),   S( -5,  11),   S(102,  48),   S(127,  50),
            S( -5,  23),   S(  1,   9),   S( 37,  87),   S( 57,  74),   S(  0,   0),   S(  0,   0),   S( 67,  35),   S(119,  83),
            S( 26,  19),   S( 63,  50),   S( 64,  63),   S( 45,  35),   S(  0,   0),   S(  0,   0),   S( 83,  73),   S(120,  20),
            S( 57,  -7),   S(  0,  17),   S( 76,  46),   S( 63,  48),   S( 53,  60),   S( 97,  81),   S(139,  66),   S(172,  38),
            S(  6,   9),   S( 43,   9),   S( 68,  14),   S(102,  53),   S(108,  45),   S(101,  53),   S(124,  45),   S( 98,  11),

            /* queens: bucket 7 */
            S(-14, -20),   S(-16, -35),   S(-44,   8),   S(-16,  -9),   S(-15, -14),   S(-18,   2),   S(-24,  -1),   S(-13, -36),
            S(-44, -35),   S(-76,  20),   S(-26,  46),   S(-15,  42),   S(-31,  40),   S(-17,  31),   S(  9,  -7),   S(-59,  -6),
            S(-19, -18),   S(-53,  24),   S( -4,  36),   S( 25,  29),   S( 17,  23),   S( 14,  18),   S( 15,  14),   S( 47,   4),
            S(-68,  10),   S( -4,   1),   S(-13,  46),   S( 52,  40),   S( 84,  26),   S( 68,  15),   S( 67, -12),   S( 45,  17),
            S( 14,  -2),   S(-10,  20),   S( -6,  70),   S( 59,  49),   S( 92,  10),   S( 72,   4),   S(  0,   0),   S(  0,   0),
            S( -4,  -1),   S(-11,  42),   S( -5,  54),   S( -7,  54),   S( 78,  20),   S(111,  51),   S(  0,   0),   S(  0,   0),
            S(-34,  17),   S(-56,  11),   S(-20,  38),   S( 34,  36),   S(116,  15),   S(106,   8),   S( 75,  39),   S( 72,  44),
            S( 16, -12),   S( 31, -16),   S( 41,  13),   S( 65,  -7),   S( 79,  38),   S( 43,  15),   S(  2,   5),   S( 71,  17),

            /* queens: bucket 8 */
            S(  3,  -2),   S( -6, -14),   S(-12, -26),   S( 19,   5),   S(  1, -14),   S( 26,  13),   S( -3, -18),   S(  3,   3),
            S(-16, -30),   S(-12,  -9),   S(  9,  -3),   S( 15,  30),   S(  2, -11),   S( -5, -20),   S( -4,  12),   S( -4, -12),
            S(  0,   0),   S(  0,   0),   S(  8,  -4),   S(-12, -18),   S(  5, -10),   S(  7,  15),   S( -5,  -1),   S(-14, -11),
            S(  0,   0),   S(  0,   0),   S(  8,  13),   S(  8,   8),   S( 27,  38),   S( -2,  -8),   S(  1,  20),   S(-10, -21),
            S(  3,  -4),   S(  7,  14),   S( 21,  39),   S( 30,  17),   S(  2, -10),   S(  4,   8),   S( 11,   8),   S(-14, -27),
            S( -1, -26),   S(  2,  -8),   S( 16,  22),   S(  5,   5),   S(  3,  -2),   S( 25,  22),   S(  0, -22),   S(-12, -26),
            S( -6, -13),   S( 11,   5),   S(  8,   9),   S( 17,  20),   S( 23,  17),   S( -2,  11),   S(  3,  -6),   S( -1,  -9),
            S(  8,  11),   S( 13,  28),   S( 23,  23),   S(  3,  10),   S(  2,  -6),   S(-17, -30),   S(  2,  -6),   S(-16, -39),

            /* queens: bucket 9 */
            S( 22,  16),   S(-15, -54),   S(  7,  -5),   S( 29,  -2),   S( 13,   8),   S(  5, -13),   S(-12, -21),   S( -1, -21),
            S( 15,   2),   S(  7,   0),   S(-12, -27),   S( 11,   2),   S(  0, -21),   S(-13, -33),   S( 11,   7),   S(  6,   0),
            S(  7,  -4),   S(-11, -25),   S(  0,   0),   S(  0,   0),   S(  7,  11),   S( 11,  17),   S( -4,  -8),   S(  9,   9),
            S( 10,  10),   S( -4, -12),   S(  0,   0),   S(  0,   0),   S(  3, -10),   S( 29,  29),   S( 23,  22),   S( -7,   2),
            S( 16,   7),   S( 27,  30),   S( -2,  -3),   S(-15, -26),   S( 10,  -1),   S( 31,  35),   S(  1, -24),   S(-12, -42),
            S( 14,   6),   S( 12,  -5),   S( 20,  24),   S(  8, -19),   S( 18,   1),   S( -8,  -2),   S(-11, -18),   S(  8,   0),
            S( 14,  19),   S( 27,   7),   S( -5,   2),   S(  3,   5),   S( -3, -11),   S( 17,   2),   S( 13,  23),   S( 15,  -9),
            S( -2, -23),   S( 16, -18),   S( -7, -18),   S( 24,  17),   S(  9,  13),   S( -5,  -6),   S(-14, -53),   S( -1, -18),

            /* queens: bucket 10 */
            S( -1,  -6),   S( 18,  -4),   S(  1,  -5),   S(-15, -21),   S( 10, -12),   S(  2,  -1),   S(  2, -17),   S(  0,  -7),
            S( 12,   8),   S(-25, -51),   S( -3, -30),   S( -7, -21),   S( -1, -11),   S( 24,  25),   S( -8, -30),   S(  7,  -3),
            S(  3,  -4),   S(  2,   5),   S( 22,  25),   S( -3, -27),   S(  0,   0),   S(  0,   0),   S(  3,  -2),   S( -6, -16),
            S( -8, -10),   S( 12,  -1),   S( 12,   2),   S( 25,  35),   S(  0,   0),   S(  0,   0),   S( -7, -20),   S( 10,   0),
            S(  4,  15),   S( 15,  -4),   S(  4,  -7),   S( 49,  57),   S( 11,  13),   S( 10,  15),   S(  8,  -9),   S( 15,   2),
            S(  2,  -5),   S( 18,  11),   S( 23,  23),   S( 32,  43),   S(  6,  -7),   S( 24,  38),   S( 37,  46),   S( -9, -41),
            S( 14,  21),   S( 11,  21),   S( 36,  54),   S( 30,  33),   S(  2,  -1),   S( 20,  19),   S(  4, -20),   S( -5, -23),
            S(-17, -56),   S( -8, -20),   S( 18,  -9),   S(-13, -37),   S( 33,  24),   S(-15, -36),   S( -4, -27),   S(-14, -48),

            /* queens: bucket 11 */
            S(-15, -10),   S( -3, -23),   S( -9, -17),   S(-13, -19),   S(  7,  12),   S(-19, -39),   S(  2, -23),   S(  0, -16),
            S( -5,  -5),   S( -6,  -3),   S(-19,  -8),   S( -3, -21),   S( 22,  -5),   S(  3,  -6),   S(  2, -18),   S(  9,   3),
            S( -6,  -1),   S( -7, -19),   S(-13, -11),   S(  1,   0),   S(  4,  12),   S(-12, -41),   S(  0,   0),   S(  0,   0),
            S(  8,   9),   S(  6,   0),   S(  1,  10),   S( 25,  26),   S(  1,   1),   S(  0,   3),   S(  0,   0),   S(  0,   0),
            S(-12, -10),   S( 12,  10),   S(  2,   6),   S( -5,   0),   S( 56,  84),   S( 31,  46),   S( 18,  19),   S(-11, -27),
            S( -8,  -6),   S(  3,   2),   S(-18, -25),   S( -1,   0),   S( 26,  34),   S(  4, -19),   S( 24,  19),   S( 20,  -6),
            S( -7,  -3),   S(-11, -19),   S(  7,   2),   S(  4,  18),   S( 28,  41),   S( 23,   8),   S(-14, -25),   S( 24,  -6),
            S(-27, -64),   S(-11, -25),   S(-19, -24),   S(-13, -20),   S( 12,  11),   S( 10,  14),   S(  1,  12),   S(  2, -14),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  8,  12),   S(-13, -26),   S( -4,  14),   S(  2,  -6),   S( -5, -17),   S( -9, -20),
            S(  0,   0),   S(  0,   0),   S( 14,  31),   S( -3, -26),   S(  1,  -4),   S( -9, -29),   S( -6, -12),   S( -3,  -7),
            S(  4,   9),   S(  2,   1),   S(  0,  -8),   S( -4, -27),   S( 23,  44),   S(  3,   5),   S( -9, -16),   S( 12,  16),
            S( -1, -16),   S( 15,  17),   S( 12,  18),   S(  4,   0),   S(-12, -35),   S( -5, -12),   S( -2,  -2),   S( -2,   1),
            S(-17, -31),   S(  7,   9),   S(  5,  -7),   S(  6,   4),   S(-10, -14),   S(-27, -69),   S(-13, -34),   S(  2,   3),
            S( 12,  21),   S( -2,   2),   S(  1,   4),   S( -1,  -2),   S(-16, -37),   S(-17, -17),   S(-24, -41),   S(  4,   6),
            S(-17, -27),   S( 12,  19),   S(-13, -30),   S(  9,  16),   S(  6,   4),   S(-21, -47),   S(  5,  11),   S( -8, -27),
            S(  7,  14),   S(  1,  -1),   S(  3,   9),   S(  1,  -2),   S(-25, -57),   S(-23, -47),   S( -6,  -4),   S(-10, -20),

            /* queens: bucket 13 */
            S(-12, -35),   S( -9, -18),   S(  0,   0),   S(  0,   0),   S( -8, -15),   S(-20, -44),   S(  7,  -6),   S( -4,  -7),
            S(-14, -41),   S( -4,  -5),   S(  0,   0),   S(  0,   0),   S(-15, -35),   S(-12, -29),   S(-10, -24),   S(  3,  -2),
            S( -8, -25),   S( -2, -21),   S( -1,  -7),   S(  3,   2),   S(-11, -34),   S(-12, -31),   S(-12, -22),   S(  2,  11),
            S( -3,  -5),   S(-17, -54),   S( 10,   7),   S( 13,   2),   S( 23,  44),   S( 19,  31),   S(-10, -33),   S(-19, -43),
            S(  7,  -5),   S( -2, -10),   S(  0, -18),   S(  5,   8),   S(  7,  14),   S( -4, -11),   S(-13, -31),   S( -1,  -9),
            S(  2, -13),   S(-23, -61),   S(  4,   0),   S( 14,  27),   S(  7,  -3),   S(-13, -18),   S(-23, -48),   S(-14, -40),
            S( -7, -14),   S( -5, -16),   S( 16,  31),   S( -7, -12),   S( -8, -19),   S( 10,   2),   S(-19, -44),   S(-13, -33),
            S(-13, -32),   S( -4, -13),   S(-16, -32),   S(  4,   9),   S(-13, -22),   S(  3, -10),   S( -1,  -1),   S(-16, -33),

            /* queens: bucket 14 */
            S(  3,  -1),   S( 20,  22),   S( -6, -18),   S(  7,   1),   S(  0,   0),   S(  0,   0),   S(-13, -38),   S( -7, -22),
            S(-13, -38),   S(-23, -56),   S(-14, -29),   S(  1,  -6),   S(  0,   0),   S(  0,   0),   S( -5, -11),   S(-12, -30),
            S( -7, -18),   S(  2, -18),   S(  1,  -9),   S( -9, -23),   S(  6,  11),   S(  5,   9),   S(-10, -25),   S(-23, -53),
            S( -5, -13),   S( -6, -13),   S( 11,   9),   S(-15, -32),   S( -6, -13),   S( -2, -12),   S(  6,   0),   S( -7, -22),
            S(-10, -19),   S( -1, -12),   S(-15, -39),   S(  8,  15),   S( 11,  12),   S(  5,   5),   S(-10, -19),   S( -3, -16),
            S( -7, -16),   S(-10, -37),   S(-18, -37),   S(-19, -54),   S( -2,  -6),   S(  8,  16),   S(  2,  -8),   S(-13, -13),
            S(-16, -31),   S(-12, -24),   S( -2,  -2),   S( -5, -10),   S(  1,   4),   S( 12,  17),   S(-14, -36),   S(-17, -27),
            S(-17, -37),   S(  8,   4),   S(-13, -23),   S(-13, -24),   S(  0,   1),   S( -4,  -8),   S(-10, -28),   S( -3, -10),

            /* queens: bucket 15 */
            S( -5, -14),   S( -4, -10),   S(  5,   1),   S(-14, -24),   S(  6,   2),   S( -8, -27),   S(  0,   0),   S(  0,   0),
            S( -4, -14),   S( -2, -12),   S(-12, -26),   S( -9, -34),   S( -7, -15),   S(  5,  10),   S(  0,   0),   S(  0,   0),
            S( -1,  -7),   S(  4,  -2),   S( -7, -13),   S(  3,   0),   S(-12, -33),   S(  2,   1),   S( -4, -11),   S(  0,  -3),
            S(  0,   0),   S( -9, -23),   S( -6, -17),   S( -2,  -8),   S(  9,  11),   S(  1,   7),   S( -5, -16),   S( -8, -30),
            S(  0,   1),   S(  1,   2),   S( -7, -18),   S( -1,  -6),   S( 11,  46),   S(  2,   3),   S(  1,   3),   S(-19, -44),
            S(  6,   7),   S( -2,  -4),   S(  0,  -3),   S(-18, -36),   S( -8, -11),   S(-20, -38),   S(  4,  12),   S(  2,   4),
            S( -9, -18),   S( -8, -16),   S( -1,  10),   S( -1,   1),   S(-10, -18),   S(  1,   4),   S(  0,  -1),   S( -5, -13),
            S( -7, -17),   S(-17, -35),   S(  4,  -6),   S( -6,  -8),   S(-22, -33),   S( -3, -11),   S( -2, -10),   S(-17, -33),

            /* kings: bucket 0 */
            S(  1, -19),   S( 28, -11),   S( 17,  -7),   S(-10,  -3),   S(-16,   7),   S( 28, -33),   S( 16,   7),   S( 23, -56),
            S(-26,  26),   S( -5,   6),   S( -9,   9),   S(-45,  13),   S(-44,  31),   S(-20,  21),   S(-18,  44),   S(-11,  28),
            S( 25,  -7),   S( 50, -29),   S(-16,   9),   S(  8, -16),   S(-23,   8),   S(-18, -12),   S(-49,  26),   S(  7, -24),
            S(-12, -29),   S( -5,  -2),   S(-11,  -9),   S(-37,  11),   S(-74,  30),   S(-27,  12),   S(-48,   8),   S( -7,  27),
            S(-62, -90),   S( 27, -23),   S( 13, -28),   S( 16,  -9),   S(-24, -13),   S(-44,  20),   S(-16,   7),   S(-33,  -3),
            S( -3, -75),   S( 22, -50),   S( 45, -49),   S(  5, -41),   S( 20, -10),   S(  7, -29),   S( 20,  -8),   S(  4,  20),
            S(  0,   0),   S(  0,   0),   S(  8, -44),   S( 26, -25),   S(  9, -26),   S(  8,  -6),   S(  7, -11),   S(-20, -29),
            S(  0,   0),   S(  0,   0),   S(-20, -94),   S( 16, -30),   S( 14, -16),   S(  9,  -6),   S( 19,  23),   S(  6,  15),

            /* kings: bucket 1 */
            S( 22, -43),   S( 33, -17),   S( 10, -12),   S( 26,  -6),   S(-15,   4),   S( 27, -17),   S( 11,  16),   S( 22, -14),
            S( -6,  16),   S( -5,  31),   S(  4,  -4),   S(-42,  22),   S(-33,  25),   S( -1,  13),   S(  2,  23),   S(-10,  20),
            S( -6, -10),   S(  7, -10),   S( 17, -26),   S( -5, -13),   S(-39,   6),   S(  3, -13),   S( 21, -12),   S( 34, -17),
            S( -7, -10),   S( 62, -22),   S( 11,  -3),   S( -2,   7),   S( -1,  22),   S(-49,  13),   S( 31,   3),   S(-39,  38),
            S(-19, -25),   S( 20, -25),   S( 26, -35),   S( 24, -22),   S(-15,  -2),   S(-16, -17),   S( 10,  -6),   S(-12,  -7),
            S(  8, -14),   S( 23, -43),   S(  9, -23),   S( 48,  -5),   S(  8, -20),   S( 22,   3),   S( 38,  15),   S(-22,  13),
            S(  2, -21),   S( 17,  17),   S(  0,   0),   S(  0,   0),   S(-19,   3),   S(  2,  15),   S( -6,  46),   S( -4, -26),
            S(-17, -125),  S(-16, -20),   S(  0,   0),   S(  0,   0),   S(  1, -35),   S(  3,  -1),   S(  6,  48),   S(-10, -54),

            /* kings: bucket 2 */
            S( 33, -65),   S( 14,  -2),   S( 13, -20),   S( 27, -17),   S( -9,   9),   S( 36, -23),   S(  9,  25),   S( 28, -18),
            S( 33, -13),   S(-23,  40),   S( -6,   4),   S(-14,  10),   S(-27,  17),   S( -7,   1),   S(  9,   8),   S( -7,  11),
            S(-24, -19),   S(-16,   1),   S(  1, -14),   S(-18, -15),   S( -1,  -4),   S(  7, -24),   S( 29, -13),   S( 25, -17),
            S(-16,  29),   S(-18,  17),   S( -8,  10),   S(-14,  14),   S( -3,   9),   S(-26,   1),   S( -4,  -8),   S( 54, -25),
            S(-11, -17),   S( 10,  -1),   S(  1, -22),   S(-12,  -7),   S( 48, -38),   S( 10, -30),   S( 32, -36),   S(  8, -25),
            S( 17, -13),   S( 31, -10),   S( 24, -23),   S( 39, -34),   S( 51, -28),   S( 39,   0),   S( 69, -39),   S(  9, -26),
            S(-12, -20),   S( -5,  15),   S(-36, -15),   S( 22,   8),   S(  0,   0),   S(  0,   0),   S( 25,   7),   S( -2, -31),
            S( -8, -16),   S( -3, -21),   S( -2, -42),   S( 12,  22),   S(  0,   0),   S(  0,   0),   S(  0,  -6),   S(-15, -126),

            /* kings: bucket 3 */
            S(  7, -71),   S( 10,  -6),   S( 23, -40),   S( -4, -15),   S(-13, -21),   S( 31, -32),   S(  7,  14),   S(  8, -28),
            S(-18,  25),   S(-26,  41),   S(-21,   6),   S(-35,   8),   S(-52,  23),   S( -3,  -7),   S(-11,  17),   S(-16,  12),
            S( 17, -23),   S(  3,  -7),   S(  0, -17),   S(-24, -10),   S( -3,   0),   S( 32, -35),   S( 43, -19),   S( 56, -30),
            S(-32,   7),   S(-125,  45),  S(-76,  17),   S(-85,  22),   S(-40,   7),   S(-84,   3),   S(-68,   1),   S(-68,  -6),
            S(-25,  19),   S(-14, -12),   S(-57, -12),   S(-38,   1),   S(-13, -36),   S(  0, -41),   S( 22, -49),   S( -5, -63),
            S(-23, -18),   S(  9, -27),   S(  3, -32),   S(-52,  -7),   S( 28, -32),   S( 98, -67),   S(111, -41),   S( 33, -116),
            S(-30,  -8),   S( 24,  -4),   S(  7, -25),   S( 11, -21),   S( 19, -23),   S( 65, -49),   S(  0,   0),   S(  0,   0),
            S( -1,  -6),   S(  7, -23),   S(  7,   1),   S(-13,  -8),   S(  5, -77),   S(  0, -30),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-41,   5),   S( -1,  21),   S(-32,  25),   S( 16,   0),   S(-10, -13),   S(  9,  -2),   S( 24,  12),   S( 31, -18),
            S(-43,  27),   S( 25,  14),   S(-40,  31),   S( -1,  -3),   S( 56, -12),   S( 18,  -1),   S( 56, -16),   S( 29,   0),
            S(  0,  18),   S(-37,   8),   S(-15,   0),   S( -4,  -9),   S(-23,  10),   S( 16, -21),   S(-39,   2),   S(  7, -10),
            S( 12, -20),   S( 25,  -5),   S( 26,  11),   S(  0,   8),   S( -5,  -2),   S(-14,  15),   S( -7,   8),   S(  4,  16),
            S(  0,   0),   S(  0,   0),   S(  4, -28),   S(  5,   1),   S(-15,  -4),   S(-26,   7),   S(-16,   7),   S(-13, -12),
            S(  0,   0),   S(  0,   0),   S(  3,  18),   S( 11,   4),   S(  3,   9),   S(  1, -19),   S(  4, -24),   S( -3,  -4),
            S(  2,  20),   S(  0,  -4),   S(  6, -38),   S( 18,  -5),   S( 25,  27),   S(-25,  -5),   S(  0, -14),   S(  3,  15),
            S( -8,  13),   S(  0,  13),   S( -1,  -4),   S( 13,  40),   S(  8, -11),   S( -5,  -2),   S( -5, -34),   S(  1,   0),

            /* kings: bucket 5 */
            S( 30, -22),   S( -6,  17),   S(-35,  16),   S(-37,  23),   S(-30,  15),   S( -7,   9),   S( 51,   4),   S( 51, -19),
            S(-11,  17),   S( 43,  -8),   S( 41,  -9),   S(  7,   6),   S( 24,  -4),   S( 49, -12),   S( 40,   4),   S( 43,  -5),
            S(-10,  -9),   S(-29,   9),   S( 14, -15),   S(-33,  -2),   S( -4,   3),   S(-40,  -5),   S(-20,   8),   S(  6,  -2),
            S(-21,   6),   S( 54, -16),   S( 39, -12),   S(-19,  27),   S(  1,  23),   S( 33,  -7),   S( 33,   4),   S(-17,   1),
            S(  3,  -9),   S( -7, -24),   S(  0,   0),   S(  0,   0),   S( -2,  -1),   S(-17, -10),   S(  7,   1),   S(-18,  -1),
            S(-32,  -7),   S(-25,  -2),   S(  0,   0),   S(  0,   0),   S(  1,  27),   S(-41,   8),   S(-47,  22),   S( -9,  -2),
            S(-17, -24),   S(  5,  17),   S(  2,  33),   S( -1,   7),   S(-16,  34),   S(-16,  -6),   S(  3,  39),   S( 12,   7),
            S(-18, -42),   S(  4,  55),   S(  1,  35),   S(  7,   3),   S( -3,  27),   S(  4,  23),   S(-13,  -3),   S( -9, -17),

            /* kings: bucket 6 */
            S( 25, -24),   S( 33,  -8),   S(  3,  -8),   S( -5,   4),   S(-14,  17),   S(-17,  13),   S( 12,  22),   S( 28,   4),
            S( 72, -23),   S( 12,  16),   S( 38, -10),   S( 44, -13),   S( 25,   0),   S( 21,  -2),   S( 25,   7),   S( 19,   3),
            S(  5, -11),   S( 19,  -6),   S(-17, -10),   S( -6, -10),   S(  6,  -7),   S(-78,   8),   S(  0,   2),   S(-58,  22),
            S(-20,  14),   S( 20,   3),   S( 20,  -2),   S( 16,   6),   S( 72,   1),   S(  7,  -5),   S( 88, -23),   S(  2,   4),
            S(-11,  -5),   S(-34,  -4),   S(  1, -14),   S(-14,   2),   S(  0,   0),   S(  0,   0),   S( -1, -16),   S(-62,  -3),
            S(-20,   7),   S(  8,   7),   S(-23,   0),   S(-36,   0),   S(  0,   0),   S(  0,   0),   S(-50,  25),   S(-30, -14),
            S( 10, -35),   S(  8,  10),   S(  6,   0),   S( -6,  23),   S( -3,  -6),   S(  1,  17),   S(-14, -16),   S(-16,  -6),
            S(  0,  13),   S(  9,  31),   S(  2,   5),   S( -7, -16),   S(  5,  16),   S( -8,   1),   S(  7,  52),   S( -5,  12),

            /* kings: bucket 7 */
            S( 56, -29),   S( -5,   7),   S(  4, -13),   S(-60,  30),   S(-41,  10),   S(-32,  31),   S(-17,  36),   S(-12,  23),
            S(  8,  15),   S( 40, -32),   S( 26, -25),   S(-18,  -3),   S( -3,   2),   S(-28,  18),   S( 17,   2),   S( 16,   6),
            S( 17, -23),   S(  4,  -5),   S(-12,   2),   S(-22, -10),   S(-28,  -2),   S(-46,  11),   S(  3,   5),   S(-74,  21),
            S( 11,  15),   S(  4,  -2),   S(  2,  -3),   S( 31,  -1),   S( 27,  -1),   S( 19, -15),   S( 34, -12),   S( 42, -17),
            S( -1,  12),   S(  5,   9),   S(-15, -11),   S(-31,   8),   S(-40,  -8),   S(  9, -15),   S(  0,   0),   S(  0,   0),
            S(-15, -41),   S(  2, -24),   S( 28, -19),   S(  3,   0),   S( 12, -10),   S( 14,  -4),   S(  0,   0),   S(  0,   0),
            S(  9,   4),   S( 18,  -6),   S( 11,   9),   S(-10,   1),   S( 10, -29),   S( -8, -18),   S( 14,   7),   S( -8, -27),
            S( 14,   9),   S(-10, -36),   S( 24,   6),   S(  6, -12),   S( 16,  21),   S(-14, -28),   S(  6,  24),   S(-14, -34),

            /* kings: bucket 8 */
            S(-11,  83),   S(-18,  66),   S(-22,  46),   S(-22,   1),   S( -8,  -4),   S(-16,   7),   S( 23,  -6),   S( -2,  13),
            S( 22,  82),   S( 26,   6),   S(  8,  58),   S(  3,   3),   S(-13,  23),   S(-11,  13),   S( 27,  -3),   S(  3,  36),
            S(  0,   0),   S(  0,   0),   S(  5,  27),   S( 12,  17),   S( 19, -15),   S(  9, -17),   S(  4,  18),   S( 10,  16),
            S(  0,   0),   S(  0,   0),   S( 14,  32),   S( 29, -33),   S(  9,  26),   S( 22,   5),   S(  2,   6),   S( -6,   3),
            S( -1, -12),   S(  4,   4),   S(  8, -13),   S( 12,   5),   S(  3, -17),   S(  8,   6),   S(  3,   5),   S(-13, -29),
            S(  3,  13),   S( -6,  -5),   S( -6,   1),   S( -8, -39),   S(-11, -15),   S(-19, -18),   S(-11,   7),   S(  6,   4),
            S( -6, -22),   S( -2, -14),   S( 13,  -5),   S( -8, -22),   S(  4, -24),   S( 12,   2),   S( 20,  -6),   S(  6, -49),
            S( -7, -14),   S(-11, -42),   S(  7,  -5),   S( -6, -20),   S(  7,  39),   S( -5, -33),   S(  7,   1),   S(  3,  -5),

            /* kings: bucket 9 */
            S(-60,  52),   S(-32,  33),   S(-65,  46),   S(-60,  34),   S(-81,  37),   S(-41,  27),   S( 50,  15),   S( 37,  22),
            S(-20,  22),   S( 33,  21),   S(  4,  -8),   S( 14,  18),   S( 29,  22),   S( 41,   0),   S( 34,  12),   S( 25,   0),
            S(-17,  11),   S(  0,  30),   S(  0,   0),   S(  0,   0),   S(  7,  12),   S(  0,  -2),   S( 25,   1),   S( -2,   8),
            S(  0, -20),   S( -6, -17),   S(  0,   0),   S(  0,   0),   S(  9,  13),   S( 55,  -9),   S(  1,   1),   S( -9,  11),
            S( -9,  -4),   S(  5,   5),   S(  1,  10),   S(  1, -15),   S( -1, -12),   S(  9,   8),   S( -8,   9),   S(  2,   2),
            S( -4,  10),   S(  1,  36),   S(  5,  20),   S( -6, -15),   S( -2,  10),   S( -4,  22),   S(-28,  -5),   S(  0,  37),
            S(  3,  -9),   S( -5,  14),   S(  5,  -4),   S(  1,  15),   S( 14,  35),   S( 22,  18),   S(-13, -16),   S( -1,   9),
            S( 12,  47),   S(-15, -38),   S(  9,  13),   S( -7, -30),   S( -8, -55),   S( -2, -14),   S( -4, -34),   S(  8,  44),

            /* kings: bucket 10 */
            S(-13,  28),   S(-27,   1),   S( -8,   1),   S(-32,  23),   S(-64,  15),   S(-140,  54),  S(-27,  38),   S(-82,  91),
            S( 31, -15),   S( 50,  17),   S(  6, -11),   S( 35,  21),   S( 48,  18),   S( 15,  16),   S( 30,  22),   S(-32,  34),
            S(  1,  22),   S( 15,  -1),   S( 25,  -3),   S( 19,  -9),   S(  0,   0),   S(  0,   0),   S( 11,   8),   S(-36,  13),
            S(  7,  -3),   S( 14, -11),   S(  3,   0),   S( 11,  -8),   S(  0,   0),   S(  0,   0),   S( 19,  11),   S( 14,   5),
            S( 12,   0),   S( 20,  21),   S(  1,   0),   S( 14, -36),   S(  3,  -5),   S(  3,  -5),   S(  2,  -3),   S( -8,  14),
            S(-10,   0),   S( -4,  23),   S( -9,   5),   S( -7,   4),   S(  5,   6),   S(-10,   4),   S( -3,  17),   S( -4,   6),
            S(  0,  -9),   S( -2, -25),   S(  3,  -7),   S( 22,   9),   S(  9,  17),   S( -4,   3),   S( 25, -17),   S(  3,  36),
            S(  0,  -1),   S( 11,   7),   S( -3, -26),   S(  4,   7),   S(  0,  -5),   S(  2,  -7),   S( -8, -40),   S( 11,  39),

            /* kings: bucket 11 */
            S(-16,   8),   S(  3,  -4),   S(-12,  12),   S(-17,  -5),   S(-17,   2),   S(-152,  65),  S(-69,  61),   S(-162, 145),
            S( 11, -38),   S( 27,  18),   S(-24, -21),   S( 37,  -2),   S( 64,   1),   S( -6,  56),   S( 49,   9),   S( 29,  50),
            S(-11,   0),   S( 24,  -5),   S( -8,   1),   S( 20,  -6),   S( 53,   7),   S( 23,  25),   S(  0,   0),   S(  0,   0),
            S( 16,  29),   S( -1,   9),   S(  1,  -2),   S( 54, -15),   S( 31, -23),   S( 37,  -2),   S(  0,   0),   S(  0,   0),
            S( -7, -41),   S(  8,   6),   S( -1,  -9),   S( 18, -27),   S( 16,  -7),   S( -3, -21),   S(  0,  10),   S(  3,  -6),
            S( 12,  22),   S(  1,  28),   S( 17, -14),   S( -1,   7),   S(  2,  -8),   S( -1,   0),   S( -4,   3),   S(  1,  -8),
            S(  9, -14),   S( 14,   4),   S( 10,  38),   S(  1, -20),   S( 11,  -1),   S(  1,  -3),   S(  4, -16),   S(-12, -17),
            S(  5,   8),   S( -8, -37),   S( -8, -25),   S(  5, -12),   S(-12, -27),   S(  1,  -9),   S(  4,   4),   S(  7,  23),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( -7,  69),   S( -9,  17),   S( -4,   0),   S(  8,  32),   S(  1, -29),   S(  5,  44),
            S(  0,   0),   S(  0,   0),   S( 24,  79),   S(  6, -12),   S( 16,  14),   S(  7,  -5),   S(  4,   6),   S(-11,  21),
            S( -1,  -4),   S(  1, -25),   S( 14,  36),   S( 13,  34),   S( -5,  -6),   S(-14,   5),   S( -2,  -7),   S( -8,   4),
            S( -1,  -9),   S(  7,  -3),   S( -5, -44),   S(  4, -33),   S(-11, -22),   S(  4,   3),   S(-19,  25),   S( -2,   8),
            S( 18,  43),   S( -2,   7),   S(  2,   8),   S(  5, -21),   S(  7,  22),   S(-12, -11),   S( -4,  13),   S( -4,   3),
            S(  8,  33),   S(  2,   8),   S(  5,   6),   S( -7,  -8),   S( -2, -10),   S( -3,  27),   S(-12,   8),   S(  0,  15),
            S(  3,  13),   S( -1,  -3),   S( -2,   1),   S(  1, -10),   S( -3, -19),   S( -8, -14),   S( 16,  60),   S(  2, -14),
            S( -1,   2),   S( -2, -12),   S( -3, -22),   S( -1, -14),   S(  1, -11),   S(  3,   0),   S(-14, -35),   S( -7, -27),

            /* kings: bucket 13 */
            S(-10,  83),   S( -1, 102),   S(  0,   0),   S(  0,   0),   S(  3,  59),   S(-20,  -2),   S( -2, -12),   S(  6,  32),
            S(-12,  19),   S( -6, -14),   S(  0,   0),   S(  0,   0),   S( 27, -14),   S( -4, -13),   S(-24,  25),   S(  8,   9),
            S(-18, -14),   S(  3,  43),   S( -6, -30),   S( -3,   6),   S(  3,   7),   S( -7,  14),   S(-12,  -2),   S( -8,  11),
            S(-13, -29),   S( -7,   6),   S( -3, -23),   S(  2, -31),   S(  6, -13),   S(  2, -21),   S(  3,   7),   S(-14, -22),
            S( -2,   2),   S( -5,   3),   S( 11,  41),   S( -4, -11),   S( -9,  16),   S( -6,   1),   S(-20, -11),   S(  9,  39),
            S(  6,  21),   S( -4,  26),   S( -7,  -2),   S( -9, -22),   S( -5,  -2),   S( -6,   3),   S(  1,  -3),   S(  2,  -8),
            S(  6,  15),   S( -3,   3),   S( -8, -16),   S(  1, -11),   S( -1, -43),   S(  6,   0),   S( -3, -58),   S(  5,  18),
            S(  1,   1),   S(  0,  25),   S(  5,  33),   S(  3,  -7),   S( -4,  -7),   S(  1,   6),   S( -4, -20),   S(  2,   7),

            /* kings: bucket 14 */
            S( -4,  74),   S(-13,   9),   S(  0, -10),   S(  2,  26),   S(  0,   0),   S(  0,   0),   S( -5,  99),   S(-69,  63),
            S(-13, -11),   S( -9, -18),   S( -2,  -5),   S( 12,   6),   S(  0,   0),   S(  0,   0),   S( 23,  13),   S(-37,   2),
            S( -7, -12),   S(-10, -22),   S( 13,  -5),   S(  9,   3),   S(  4, -14),   S(  0,  18),   S( 10,  37),   S(-21,   0),
            S(  8,  18),   S(  5,   8),   S( -2, -32),   S( 11, -43),   S(-13, -55),   S( 13,  25),   S(  2,  14),   S(  5,   5),
            S(  7,  35),   S( -9, -19),   S(-18,   8),   S( -7, -14),   S( -2,   5),   S( -1,  19),   S(  0,  27),   S(  7,  25),
            S( -6, -15),   S( -9,  14),   S(  7,   1),   S( -8,  22),   S( -6, -16),   S(  2, -17),   S(-14, -37),   S(  7,  26),
            S(  1,   1),   S( -9, -45),   S(  1,  51),   S( -3, -10),   S( -4, -15),   S( -5, -31),   S( -7, -63),   S(  0,  31),
            S(  4,  20),   S( 11,  66),   S(  5,  12),   S( -6,  -9),   S( 10,  47),   S( -2,  -3),   S( -9, -48),   S(  6,   1),

            /* kings: bucket 15 */
            S(  5,  54),   S(-14, -26),   S(  3,  -7),   S( -7, -33),   S(-36,  24),   S(-30,  65),   S(  0,   0),   S(  0,   0),
            S( -4, -34),   S(  1,   9),   S( -2, -22),   S( 20,  41),   S( 29, -22),   S( 30,  93),   S(  0,   0),   S(  0,   0),
            S( -7,  27),   S(  8,  -8),   S( -2, -32),   S( -6,   1),   S(  1, -42),   S( 27,  59),   S( 10,   4),   S( -9, -12),
            S(  6,   2),   S( -6,  12),   S( 11,  16),   S(  3, -14),   S(  1, -56),   S( -2,  -2),   S(  0,  48),   S(  0, -12),
            S( -2,  16),   S(-17,  19),   S( -3,   0),   S(-12, -54),   S(  2, -16),   S(-10,  17),   S(  0,  14),   S( -4,   1),
            S( -2,  24),   S(-16,   1),   S(  8,  -4),   S( 17,  57),   S(-13, -19),   S(-11,  -1),   S( -2,  -5),   S(  3,  -4),
            S( 14,  41),   S(-11, -22),   S(-10, -13),   S(  2,   9),   S( -3,   9),   S(  4,  11),   S( -9, -12),   S(  1,   9),
            S( -7,  -6),   S(  7,   1),   S(  6,  41),   S(  2,  18),   S(  0,  21),   S(  9,  41),   S(  1,  -5),   S(  7,   7),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-20, -179),  S(  6, -30),   S( 16,  41),   S( 24,  79),   S( 31,  99),   S( 37, 120),   S( 43, 117),   S( 56, 105),
            S( 80,  69),

            /* bishop mobility */
            S(  3, -72),   S(  7,   7),   S( 16,  52),   S( 22,  90),   S( 27, 116),   S( 30, 136),   S( 30, 148),   S( 31, 148),
            S( 31, 154),   S( 38, 148),   S( 42, 143),   S( 66, 132),   S(108, 123),   S(100, 103),

            /* rook mobility */
            S(-22, -10),   S(-15,  54),   S(-14,  82),   S(-10, 101),   S(-11, 123),   S( -4, 133),   S(  1, 142),   S( 10, 140),
            S( 13, 150),   S( 21, 153),   S( 30, 153),   S( 41, 150),   S( 55, 144),   S( 73, 134),   S(159,  92),

            /* queen mobility */
            S( 44, -59),   S( 49, -53),   S( 43,  51),   S( 39, 128),   S( 41, 160),   S( 40, 205),   S( 40, 246),   S( 39, 265),
            S( 41, 284),   S( 39, 306),   S( 36, 327),   S( 33, 347),   S( 35, 356),   S( 30, 373),   S( 31, 380),   S( 26, 390),
            S( 27, 388),   S( 40, 381),   S( 56, 365),   S( 71, 347),   S( 84, 336),   S(114, 307),   S(148, 289),   S(206, 242),
            S(201, 244),   S(176, 236),   S(113, 192),   S( 88, 196),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  29),   S(-15,  57),   S(-32,  57),   S(-26,  68),   S( 11,  27),   S(-14,  37),   S( -8,  77),   S( 35,  36),
            S( 12,  42),   S(  4,  60),   S(-18,  56),   S(-16,  46),   S( -4,  42),   S(-37,  60),   S(-35,  81),   S( 37,  39),
            S( 51,  64),   S( 44,  70),   S( 22,  54),   S( 37,  46),   S(  8,  52),   S( -7,  60),   S(-13,  99),   S( 19,  72),
            S( 57,  91),   S( 69, 102),   S( 38,  69),   S( 28,  47),   S( 18,  58),   S( 14,  84),   S(-12, 110),   S(-41, 124),
            S( 67, 128),   S(113, 140),   S(100, 100),   S( 51,  85),   S(-33,  83),   S( 45,  79),   S(-41, 134),   S(-82, 144),
            S(131, 183),   S(100, 216),   S(157, 201),   S(159, 194),   S(171, 201),   S(171, 197),   S(143, 204),   S(167, 206),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  18),   S(  3,   2),   S(  7,   2),   S(  8,  33),   S( 22,  42),   S( -4, -23),   S(-19,  38),   S(  7, -33),
            S( -8,  27),   S( 30,   7),   S(  0,  48),   S( 22,  41),   S( 40,  -2),   S( -2,  36),   S( 21,  -4),   S(  7,  11),
            S( -3,  18),   S( 15,  16),   S(  1,  48),   S( 17,  72),   S( 28,  32),   S( 26,  27),   S( 31,   5),   S( -6,  19),
            S( 26,  25),   S( 18,  46),   S( 42,  82),   S(  9,  97),   S( 83,  63),   S( 63,  54),   S( 37,  46),   S( 11,  30),
            S( 63,  64),   S(117, 110),   S( 95, 147),   S(122, 163),   S(150, 140),   S(127, 141),   S(141,  96),   S( 79,  36),
            S( 73, 203),   S(116, 289),   S(112, 252),   S(113, 243),   S( 72, 172),   S( 51, 134),   S( 45, 168),   S( 20, 118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  46),   S( 26,  34),   S( 41,  55),   S( 42,  37),   S( 29,  39),   S( 28,  40),   S(  7,  22),   S( 45,  14),
            S(  3,  21),   S( 14,  43),   S( 14,  38),   S( 12,  51),   S( 26,  15),   S( 11,  27),   S( 29,  22),   S(  0,  16),
            S(  1,   7),   S( 24,  29),   S( 51,  42),   S( 41,  34),   S( 40,  46),   S( 58,  13),   S( 15,  26),   S( 19,   4),
            S( 79,  31),   S(116,   8),   S(116,  85),   S(169,  80),   S(155,  77),   S( 80,  95),   S( 80,  14),   S( 78, -19),
            S( 71,  32),   S(136,  60),   S(146, 167),   S(166, 158),   S(162, 174),   S(122, 143),   S(196, 130),   S(-10, 119),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26,  23),   S(  9,  15),   S( 19,  22),   S( -1,  52),   S( 71,  26),   S( 29,   7),   S(  3,   1),   S( 34,  11),
            S(  3,  12),   S(  7,   9),   S( 21,  17),   S( 18,  28),   S( 16,  14),   S(  0,   6),   S(  5,   6),   S( 31,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -3, -12),   S( -7,  -9),   S(-21, -17),   S(-18, -28),   S(-16, -14),   S(  0,  -6),   S( -5,  -6),   S(-31,   6),
            S(-26, -23),   S( -9, -15),   S(-19, -22),   S(  1, -52),   S(-71, -26),   S(-29,  -7),   S( -3,  -1),   S(-34, -11),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -26),   S(-13,  -7),   S( -2, -32),   S(-52,  -7),   S(-21, -12),   S(-19, -24),   S( -1, -23),   S(-13, -38),
            S(-17,  -1),   S(-13,  -1),   S(-25,  12),   S( 10, -21),   S(-29, -13),   S(-28,  -3),   S(-42,   5),   S( -7, -18),
            S( -9, -23),   S(-11, -15),   S(-22,  -5),   S(-28,  -7),   S(-25, -30),   S(-21,  -9),   S(-23,  -2),   S(-37, -22),
            S(  5, -36),   S( 16, -35),   S( 16, -20),   S(  6, -24),   S(  5, -15),   S( 62, -32),   S( 40, -31),   S( -5, -46),
            S( 31, -64),   S( 53, -74),   S( 58, -40),   S( 70, -39),   S( 71, -34),   S( 75, -20),   S(119, -70),   S( 36, -80),
            S(132, -109),  S(169, -120),  S(113, -77),   S(111, -54),   S(115, -34),   S(175, -55),   S(152, -63),   S( 80, -84),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 16,   2),        // attacks to squares 1 from king
            S( 17,   1),        // attacks to squares 2 from king

            /* castling available */
            S( 72, -81),        // king-side castling available
            S( 16,  52),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 13,  60),   S( 16,  68),   S(  5,  67),   S( -7,  83),   S(-16,  91),   S(-34, 102),   S(-68, 121),   S(-69, 122),
            S(-91, 129),   S(-70, 133),   S(-62, 137),   S( 30, 124),   S( 33, 128),

            /* orthogonal lines */
            S(  0, -55),   S(-54, -23),   S(-74, -13),   S(-87, -12),   S(-96, -16),   S(-97, -22),   S(-97, -28),   S(-94, -30),
            S(-114, -22),  S(-132, -22),  S(-159, -27),  S(-157, -34),  S(-172, -26),  S(-356,  34),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S(  6, 226),

            /* passed pawn can advance */
            S(-35,  34),   S(-28,  55),   S(-30,  90),   S( -6, 116),

            /* blocked passed pawn */
            S(  0,   0),   S( 65,   0),   S( 58,  13),   S( 87,  34),   S( 80,  58),   S( 81,  21),   S( 88,  13),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 63, -13),   S( 52,  54),   S( 62,  65),   S( 37,  84),   S( 57,  97),   S( 85, 146),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(  2,  21),   S(  4,  10),   S( 50, -34),   S(  7,  -7),   S( 56, -32),   S(167, -47),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( -6,   7),   S( 66, -13),   S( 69,  -7),   S( 55, -33),   S( 40, -98),   S(-57, -133),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 14,  84),   S( 40,  58),   S(124,  65),   S( 29,  49),   S(186,  85),   S( 25,  88),   S(  0,   0),       // blocked by King

            #endregion

            /* tempo bonus for side to move */
            S(  8,  10),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
