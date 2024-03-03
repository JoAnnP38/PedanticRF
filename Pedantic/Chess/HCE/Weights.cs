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

        // Solution sample size: 6000013, generated on Sun, 03 Mar 2024 15:12:31 GMT
        // Solution K: 0.003850, error: 0.086061, accuracy: 0.4951
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 96, 173),   S(425, 524),   S(409, 623),   S(567, 956),   S(1292, 1713), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 99, -112),  S(148, -92),   S( 30, -30),   S(-48,  35),   S(-27,  18),   S(-24,  -2),   S(-46, -10),   S(-41, -20),
            S(107, -110),  S( 89, -93),   S( -1, -57),   S(-21, -47),   S(-20, -24),   S(-26, -34),   S(-39, -31),   S(-40, -35),
            S( 93, -78),   S( 71, -55),   S( 17, -56),   S(  8, -59),   S( -6, -60),   S( -1, -49),   S(-18, -54),   S(-27, -42),
            S( 67, -26),   S( 56, -50),   S( 15, -42),   S( 23, -79),   S( -4, -52),   S(-28, -46),   S(-27, -46),   S(-38, -14),
            S( 77,  46),   S( 46,   8),   S( 30,  -3),   S( 46, -59),   S( 20, -37),   S(-28, -42),   S(-39,  -9),   S(-57,  58),
            S( 81,  41),   S( 61,  36),   S(  4, -16),   S( 25, -41),   S(-30, -11),   S(-29,  18),   S(-40,   0),   S(-11,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 31, -30),   S( 33, -44),   S( 43, -26),   S( -2,   9),   S(-11, -18),   S( 11, -19),   S(-34,  -9),   S(-37,  14),
            S( 21, -37),   S(  5, -41),   S(  4, -43),   S( -3, -45),   S(-21, -22),   S( -1, -34),   S(-37, -19),   S(-45, -11),
            S( 14, -27),   S( 13, -26),   S( 23, -48),   S(  9, -48),   S(-19, -24),   S( 19, -41),   S(-21, -24),   S(-25, -10),
            S( 20,  10),   S(  6, -32),   S( 28, -46),   S(  3, -43),   S(  2, -26),   S(  1, -32),   S(-44, -14),   S(-35,  18),
            S( -2,  64),   S(-25,   2),   S(  1, -14),   S( -1, -27),   S( 27,  -4),   S(-37,   3),   S(-56,  21),   S(-30,  57),
            S( 70,  54),   S( 38,  -6),   S(-43,  -4),   S(-40,  31),   S(-34, -20),   S(-42,   1),   S(-40,  23),   S(-83,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31,   2),   S(-32,   2),   S(-21,   3),   S(-16,  11),   S(  7, -14),   S( 32, -19),   S(  6, -44),   S(-17, -14),
            S(-34, -12),   S(-54,  -5),   S(-34, -33),   S(-24, -35),   S( -1, -28),   S(  2, -25),   S(-14, -34),   S(-30, -20),
            S(-43,   0),   S(-34, -15),   S(-15, -44),   S( -1, -56),   S( -1, -20),   S( 14, -25),   S( -6, -30),   S(-18, -10),
            S(-56,  22),   S(-32, -25),   S(-30, -28),   S(-11, -38),   S( 10, -39),   S(-11,  -9),   S(-20,  -8),   S(-27,  19),
            S(-34,  53),   S(-61,  22),   S(-58, -19),   S(-56, -10),   S(  1,   5),   S(-36,  22),   S(-41,  14),   S(-49,  84),
            S(-78,  87),   S(-86,  74),   S(-122,  22),  S(-31, -48),   S(-33,  21),   S(-11,   9),   S( -6,   8),   S(-39,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33,  -6),   S(-39,  -2),   S(-34,  -5),   S( -8, -36),   S(-10,  -8),   S( 27,  -8),   S( 82, -58),   S( 56, -73),
            S(-40, -28),   S(-55, -17),   S(-35, -43),   S(-25, -30),   S(-24, -23),   S( -2, -32),   S( 53, -59),   S( 52, -71),
            S(-42, -22),   S(-27, -42),   S( -9, -58),   S( -3, -65),   S(-10, -45),   S(  6, -35),   S( 27, -47),   S( 51, -49),
            S(-48,   2),   S(-23, -51),   S(-19, -54),   S( -1, -65),   S( 15, -62),   S( -4, -39),   S(  7, -25),   S( 43, -12),
            S(-10,  26),   S(-23, -14),   S(-11, -54),   S( -2, -58),   S( 64, -47),   S( 43, -37),   S( 44,  10),   S( 40,  68),
            S(-65, 103),   S(-49,  36),   S( -5, -54),   S(-44, -30),   S( 22, -50),   S( 68, -41),   S( 43,  13),   S( 40,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  12),   S( 15, -25),   S( -7,   2),   S( -7,  24),   S(-50, -22),   S(-26,   3),   S(-99,  16),   S(-63,  -1),
            S(  9,  -7),   S( 28,   0),   S( 50, -43),   S( 15, -21),   S(-16, -28),   S(-39, -31),   S(-16, -37),   S(  4, -31),
            S( 36,  -9),   S( 56,  -4),   S( 33, -10),   S(-10, -18),   S(-38, -25),   S(-11, -44),   S(-43, -33),   S(-42,  -5),
            S( 38,  25),   S( 37,  26),   S( 49,   8),   S(  4,   8),   S(  8, -19),   S(-59, -14),   S(  2, -36),   S( 52, -32),
            S(  3,  77),   S(  2,  94),   S( 12,  15),   S(-26,  -4),   S( 14,   5),   S(  5, -19),   S(-31, -39),   S( 37,  13),
            S( 91,  88),   S( 91,  85),   S(  4,  -3),   S( 17, -37),   S( 23, -10),   S( 15, -13),   S( 20, -43),   S(-43,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-84,  46),   S(-48,  24),   S(-13,  13),   S( 14, -10),   S(-14,  25),   S(-12,   7),   S(-22,  -2),   S(-31,  19),
            S(-69,  25),   S(-64,  15),   S( 37, -15),   S(-15,  12),   S(  8, -11),   S(-15, -16),   S(-19, -13),   S(-40,  10),
            S(-58,  44),   S(-46,  25),   S( 60, -27),   S( 17, -23),   S( 40, -15),   S(-36,  -3),   S(-14,  -5),   S(-29,  13),
            S(-25,  52),   S(-17,  25),   S( 14,  11),   S( 54,  13),   S(  0,   7),   S(-28,   8),   S( 15,  -7),   S( 16,  15),
            S( 63,  45),   S( 88,  25),   S( 62,  32),   S( 73,  10),   S(  4,  50),   S( 85,  -6),   S( 17,  -7),   S( -1,  45),
            S(100,  38),   S( 59,  31),   S( 68,  -7),   S( 44, -14),   S( 54,   2),   S( 21, -20),   S( -2,  18),   S( 78,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-51,  26),   S(-15,   0),   S(  9,   0),   S(-20,  11),   S( 28, -20),   S(-16,   3),   S(-46,   1),   S(-53,  13),
            S(-53,   7),   S( -1, -23),   S(  4, -42),   S( 25, -20),   S( 62, -23),   S( 30, -19),   S(-26,  -9),   S(-66,  10),
            S(-56,  21),   S(  5, -14),   S(-19, -15),   S(-18,  -8),   S( 31, -10),   S( 78, -32),   S(  9, -17),   S(-40,  20),
            S(-28,  28),   S(-52,  13),   S( 14, -12),   S( -2,   3),   S( 44,   1),   S( 57,  -3),   S( 38,  -6),   S( 35,  10),
            S(-30,  48),   S(-36,  14),   S(-25,   0),   S(  9,  -5),   S( 78,  27),   S( 93,  29),   S( 28,  12),   S( 73,  31),
            S( 57,  37),   S( 18,   4),   S( 13, -29),   S( 35, -36),   S( 28, -12),   S( 52,  -7),   S( 30,   0),   S( 58,  41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-64, -12),   S(-58,  -9),   S(  3, -32),   S(-57,   1),   S(-29, -24),   S( 35, -28),   S( -4, -39),   S(-47, -15),
            S(-75, -23),   S(-46, -39),   S(-35, -47),   S( -8, -47),   S( -8, -33),   S( 41, -38),   S( 41, -36),   S(-25, -21),
            S(-75, -22),   S(-63, -29),   S(-36, -46),   S(-11, -48),   S( -8, -22),   S( 32, -23),   S( 37, -36),   S( 31, -27),
            S(-61, -10),   S(-57, -35),   S(-78, -31),   S(-24, -26),   S( -2, -22),   S(  6,   4),   S( 28,  -3),   S( 63,  -9),
            S(-66,  -1),   S(  1, -36),   S(-20, -37),   S(  7, -53),   S( 16,   6),   S( 36,   0),   S( 87,  37),   S( 93,  40),
            S(  8, -12),   S(  7, -41),   S( 25, -73),   S( 33, -58),   S( 34, -50),   S( 11, -30),   S( 54,  36),   S( 89,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  38),   S(-59,  41),   S(-11,  15),   S(-12, -24),   S(  2,   3),   S(-61,  13),   S(-35,   1),   S(-64,   5),
            S(-56,  43),   S(-11,  14),   S(-20,  13),   S(-51,  -6),   S(-33, -18),   S(-21, -27),   S(-10, -28),   S(-16, -17),
            S(  2,  59),   S( 22,  72),   S( -3,  39),   S(-24,  39),   S( -5,   4),   S(-77, -22),   S(-14, -42),   S( -1, -26),
            S( 40, 102),   S( 35, 110),   S( 69,  58),   S(-15,  54),   S(-22, -11),   S(-14, -26),   S(-11, -28),   S(  0, -27),
            S( 47, 112),   S( 53, 111),   S( 50, 114),   S( 62, 108),   S( 19,   6),   S( -1,  -5),   S(  3, -21),   S(  5,   5),
            S(112, 131),   S( 73, 152),   S(102, 162),   S( 50,  83),   S(  9,  28),   S( -8, -31),   S( -1, -28),   S( 10, -16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-74,  51),   S(-96,  41),   S(-10,  12),   S( 22,  44),   S(-19,  -8),   S(-103,  34),  S(-93,  11),   S(-95,  34),
            S(-59,  13),   S(-18,   4),   S(-50,  18),   S(  0,  30),   S(-70,  10),   S(-67,   3),   S(-113,   2),  S(-46,   5),
            S(-62,  51),   S(-27,  63),   S(-32,  65),   S(-35,  52),   S(-53,  53),   S(-84,  12),   S(-25, -20),   S(-63,   8),
            S( 30,  64),   S( 48,  71),   S( 34, 113),   S( 76, 118),   S(-70,  65),   S(-13,  21),   S( 27,   2),   S( 61, -19),
            S(100,  67),   S( 80, 108),   S( 93, 114),   S( 95, 118),   S( 77,  93),   S( 37,  32),   S( 36, -19),   S( 34, -19),
            S( 16,  47),   S( 55,  80),   S(106,  90),   S(106, 131),   S( 31,  79),   S( -6, -33),   S( 15, -17),   S( -3, -27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-104,  13),  S(-110,  14),  S(  9,  -1),   S( -5,  15),   S(-27, -16),   S(-74,  42),   S(-120,  28),  S(-82,  31),
            S(-81,  -4),   S(-37, -16),   S(-51,  -7),   S(-60,   2),   S(-20,  19),   S(-44,  20),   S(-135,  32),  S(-117,  28),
            S(-24,  -2),   S(-38,  -3),   S(-12,   7),   S(-52,  59),   S(-80,  85),   S(-23,  50),   S(-74,  30),   S(-74,  43),
            S(  8,  -6),   S( 25,   8),   S( 31,  19),   S(  7,  69),   S( 38,  96),   S( -6,  91),   S( 16,  44),   S( 33,  23),
            S( 50,   7),   S( 23, -19),   S( 19,  32),   S( 47, 106),   S( 66, 108),   S( 63,  69),   S( 26,  70),   S( 76,  34),
            S( 35,   2),   S( 14, -18),   S( 25,  -5),   S( 84, 105),   S( 57,  75),   S( 48,  69),   S( 41,  53),   S( 45,  74),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-121,  12),  S(-113,  14),  S(  9, -13),   S(-12,  11),   S( 10,  42),   S(-82,  37),   S(-54,  28),   S(-56,  33),
            S(-89, -18),   S(-52, -36),   S(-27, -38),   S(-36,   7),   S(-46,  -3),   S( -2,   9),   S(-127,  62),  S(-98,  46),
            S(-47, -35),   S(-39, -38),   S(-53,   7),   S(-51,  13),   S(-19,  14),   S( 22,  45),   S(-54,  66),   S(-31,  59),
            S( -8, -18),   S(-60, -30),   S(-36,  -4),   S( -8,  11),   S( -1,  49),   S( 35,  54),   S( 80,  94),   S( 94,  60),
            S(  5,   0),   S(-38,  -1),   S( -9,   8),   S( -1,  -6),   S( 65,  66),   S( 60,  66),   S(116,  94),   S(181,  54),
            S( 12, -11),   S( -6, -43),   S(  9, -28),   S( 11, -25),   S( 35,  50),   S( 45, 117),   S( 75, 141),   S(116, 112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  -3),   S(-28, -28),   S(-27, -23),   S( -3, -10),   S(  1,   5),   S( -9,   3),   S(-39, -28),   S(-16,  22),
            S(-34, -20),   S(-13, -15),   S( -8,  16),   S( 11,  27),   S(-21,  22),   S(-17,  13),   S(-26, -24),   S(  0, -53),
            S(-11,  41),   S( 19,  61),   S( 22,  48),   S(  8,  -6),   S(  5,  18),   S(-12, -29),   S(-35, -55),   S(-29, -48),
            S( -6,  72),   S( 38,  82),   S( 52, 126),   S( 51,  78),   S(-12, -37),   S(-18, -37),   S(  9,  -5),   S(-46, -66),
            S( 66,  77),   S( 90, 204),   S( 98, 131),   S( 27,  63),   S(  1, -14),   S( -8, -35),   S(  0, -38),   S(  1, -60),
            S( 58, 128),   S( 57, 261),   S(103, 233),   S( 41,  82),   S(  2,   1),   S(-15, -73),   S( -5, -51),   S( -9, -61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -38),   S(-22,  13),   S( -5,   0),   S( -1,   3),   S(-13, -19),   S(-30, -19),   S(-38, -30),   S(-14, -19),
            S(-18, -40),   S(-27,  -9),   S(-14, -10),   S( 20,  37),   S(-37,  21),   S( -6,   1),   S(-46, -34),   S(-23, -17),
            S( 22,  15),   S( 12,   2),   S( 16,  28),   S( 32,  74),   S(  1,  25),   S(-31, -30),   S(-22, -44),   S( -6, -50),
            S( 21,  32),   S( 61,  55),   S( 43,  71),   S( 19,  90),   S( 35,  61),   S( 12, -12),   S( 11,  -5),   S( 40, -56),
            S( 28,  48),   S( 70, 126),   S( 97, 178),   S(104, 200),   S( 58, 100),   S(  5,  29),   S(  8, -49),   S( 12, -67),
            S( 37,  80),   S( 77, 158),   S( 98, 236),   S(111, 249),   S( 52,  92),   S( 15,  29),   S(  7, -21),   S( 11, -33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -51),   S(-19, -38),   S( -2, -44),   S( -2,  -3),   S(-12, -23),   S(-55,   5),   S(-37, -19),   S( -5,  21),
            S(-27, -14),   S(-17, -23),   S(-27, -49),   S( -7,  -2),   S(-11,  48),   S(-11,   8),   S(-14,  11),   S(-38,  14),
            S(-10, -42),   S( -9, -38),   S(-16, -26),   S( 11,  33),   S(-22,  41),   S(-18,  28),   S(  2,   6),   S(  6,  20),
            S( 16, -46),   S( 20, -12),   S( 27, -14),   S( 20,  54),   S( 12, 111),   S( 21,  66),   S(  3,  16),   S( 41,  17),
            S( 13, -41),   S( 28,  16),   S( 33,   5),   S( 37, 111),   S( 73, 187),   S( 93, 153),   S( 30,  31),   S( 44,  66),
            S(  6, -49),   S( 22, -14),   S( 56, 118),   S( 44, 140),   S( 77, 217),   S( 54, 184),   S( 37, 100),   S( 18,  83),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60, -64),   S(-25, -33),   S( -8, -47),   S( -2,  -4),   S(  4,  23),   S( -1,  60),   S(-27,  -9),   S( 16,  15),
            S( -9, -68),   S(-18, -27),   S(-28, -26),   S(  8,  44),   S(-26,   8),   S( 11,  54),   S( -3,  55),   S(-18,  17),
            S(  3, -56),   S(-14, -61),   S( -8, -47),   S(-16,  17),   S( 10,  26),   S( 14,  33),   S( 21,  76),   S(  9,  63),
            S( 14, -45),   S(-26, -39),   S( -3, -37),   S(  8,  -3),   S(  6,  44),   S( 43,  38),   S( 30, 111),   S(  4,  62),
            S( -6, -68),   S(-26, -118),  S(  4,   8),   S(  8, -12),   S( 37, 102),   S( 88, 117),   S( 67, 232),   S( 62, 104),
            S( 12,  -5),   S( -1, -12),   S(  3, -12),   S(  2,  21),   S( 28,  74),   S( 77, 235),   S( 25, 212),   S( 47, 112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-37,   0),   S( 19,  -7),   S(-32,  18),   S(-35,  -5),   S(-48, -11),   S( 24, -54),   S(-40, -61),   S(-31, -24),
            S(-47,  69),   S( 46, -69),   S(-31, -11),   S(  1, -14),   S( -7, -32),   S(-16, -30),   S(-33, -23),   S(-47, -26),
            S( -4,  87),   S( -8, -26),   S(  3, -18),   S(-11,  25),   S( 24,   9),   S(-29,  12),   S( -2, -39),   S(-38, -62),
            S(  8, -33),   S( 45, -22),   S( 17,  -1),   S( 26,  27),   S(  7, -11),   S(  6, -20),   S(-13, -24),   S(  1,  -7),
            S( 33, -45),   S( 45, -23),   S( 24,  14),   S( 62, -17),   S( 38,   8),   S( 43,   4),   S( 31, -25),   S(-75, -26),
            S( -5, -39),   S(  1, -34),   S( 26,  35),   S( 73, -25),   S( 50, -61),   S( 41, -26),   S(  3, -39),   S( -4, -63),
            S( 13, -20),   S( 12, -66),   S( 18, -28),   S( 40, -51),   S( 30, -27),   S( 15, -64),   S(  3, -67),   S(-18, -51),
            S(-61, -49),   S(-10,   8),   S(-15, -40),   S(-16, -72),   S(-30,  -9),   S( 26,   3),   S( -5, -10),   S( 27,  15),

            /* knights: bucket 1 */
            S(-35,  11),   S(-48,  73),   S( 17,  34),   S(-33,  65),   S( -5,  36),   S(-42,  49),   S(-37,  49),   S(-28, -14),
            S( 44,   7),   S( -6,  26),   S(-10,   1),   S(-10,  22),   S( -3,  16),   S( 12,   2),   S( 15, -28),   S(-19, -20),
            S(-34,  17),   S(  2,   5),   S( -2,   8),   S( 23,  18),   S( 15,  13),   S(-19,  15),   S(-10, -12),   S(-38,  16),
            S(-12,  34),   S( 43,  23),   S( 18,  47),   S( 17,  35),   S(  2,  32),   S(  6,   9),   S( 20, -16),   S( 22,   9),
            S(  3,  37),   S(  7,  27),   S( 27,  37),   S( 20,  25),   S( 30,  29),   S( 37,  24),   S( 11,  16),   S( 19,   2),
            S( 22,   5),   S( 18,  -3),   S( 28,  43),   S( 33,  18),   S( 42,  25),   S( 48,  25),   S( 69, -15),   S(-15,  22),
            S( 20, -10),   S( -7,  24),   S(-12,  -9),   S(  4,   3),   S(-25,   7),   S( 49,  -7),   S( -9, -24),   S(  7, -19),
            S(-96, -59),   S(-10,  -8),   S( -4,  -4),   S(-24, -14),   S(-19,  -9),   S(-18, -36),   S(-10,  -9),   S(-25, -54),

            /* knights: bucket 2 */
            S(-45,  13),   S( -6,  26),   S(-33,  56),   S(-22,  39),   S(-20,  45),   S(-29,  52),   S(-20,  43),   S(-36,   5),
            S(-14,  -7),   S( -7,   2),   S(-19,  19),   S(-15,  30),   S( -8,  15),   S( -6,  30),   S(-29,  50),   S(-52,  80),
            S(-26,  20),   S( -7,  18),   S(-22,  42),   S( 13,  21),   S(  4,  26),   S( -7,  15),   S( -7,  27),   S(-38,  13),
            S(-16,  35),   S(-31,  35),   S(  5,  41),   S(  1,  49),   S( -9,  57),   S(-12,  40),   S( 13,  32),   S( -4,  49),
            S( 11,  21),   S(-32,  40),   S(-21,  56),   S(-37,  67),   S( -6,  53),   S(-17,  59),   S( -9,  33),   S(-16,  18),
            S(-28,  29),   S( 13,  30),   S(-20,  54),   S(-15,  40),   S( -9,  44),   S( -7,  47),   S(-38,   5),   S( 31, -28),
            S( -6,  17),   S(-54,  24),   S(-15,   7),   S(-22,  31),   S( 16,  -8),   S( 26,   3),   S(-51,  52),   S(-37,  22),
            S(-158,  32),  S( 25,   7),   S(-70,  39),   S(-31, -20),   S( 34,  -7),   S(-28,  34),   S( -9, -32),   S(-212, -55),

            /* knights: bucket 3 */
            S(-51,   8),   S( -6, -19),   S(-51,  31),   S(-19,  30),   S(-16,  32),   S(-18,  14),   S(  2,  23),   S(-54, -15),
            S(-33,  17),   S(-47,  27),   S(-31,   9),   S( -2,  23),   S(  1,  14),   S(-26,  15),   S(-25,  16),   S(-38,  77),
            S(-20,  -8),   S(-12,  19),   S(-18,  22),   S(  9,  23),   S( 13,  26),   S(  3,  18),   S( -6,   4),   S(-17,  51),
            S(-21,  21),   S( -5,  26),   S( -2,  57),   S(  6,  53),   S(  5,  58),   S(  5,  49),   S(  6,  44),   S(  9,  36),
            S(  3,  26),   S(-14,  29),   S( 15,  35),   S(  4,  57),   S(  4,  66),   S(  2,  79),   S( 19,  51),   S(-10,  32),
            S(  1,  -5),   S( 13,  14),   S( 36,  15),   S( 48,  21),   S( 57,  -3),   S( 75,   1),   S(  2,  18),   S( -2,  67),
            S( -2,   2),   S(  4,   6),   S( 59, -13),   S( 53,  -1),   S( 33,   3),   S( 66, -33),   S( 22, -44),   S( 58, -23),
            S(-147,  21),  S(-34,   4),   S(-38,  11),   S(  9, -13),   S( 38, -23),   S( 17, -31),   S( 18,   8),   S(-82, -65),

            /* knights: bucket 4 */
            S(  7,   1),   S(-80,  -6),   S( 21,  -4),   S(  0,  -8),   S(  2, -21),   S(-34, -12),   S(-14, -60),   S(-21, -59),
            S( 38,  48),   S(-17,  23),   S( 43, -52),   S( 27, -38),   S(  5, -40),   S( -9, -52),   S( 21, -14),   S(-18, -48),
            S(  0,  18),   S( 15,  42),   S( 16,   1),   S( 33,  16),   S( 17, -19),   S( -6,  -1),   S(-49, -49),   S( -5, -62),
            S( 30,  36),   S( 26, -14),   S( 83, -19),   S( 29,  11),   S(  9,   5),   S(120, -35),   S(  9, -55),   S(-14, -35),
            S( 57,  45),   S(-16,  37),   S( 82,  12),   S( 90,   2),   S( 39,  39),   S( -7,   6),   S(  2, -53),   S(-27, -43),
            S( -8,   4),   S(  1, -15),   S(111,  -6),   S( 15, -15),   S( 23, -11),   S(  9,  11),   S(  9,  39),   S( -8, -14),
            S(  4,  26),   S(-26,  -4),   S( 12,  36),   S( 17,  28),   S(  5, -22),   S( 14, -34),   S(  3, -47),   S( -9,   1),
            S(-12,  -3),   S( -5, -15),   S( 17,  18),   S(  1,   1),   S( -7, -19),   S( 13,  12),   S(  0,  18),   S(  2, -13),

            /* knights: bucket 5 */
            S( 28,  -6),   S(-10,  36),   S( 47,  19),   S( 29,  37),   S( 47,   6),   S( -4, -12),   S(-74,  -3),   S(-11,  -7),
            S(  6,  -8),   S( 57,  17),   S( 27,  10),   S(  4,  23),   S( 70,  -2),   S( 29,  19),   S( -2,   1),   S(-30, -11),
            S( 27,  16),   S(-28,  36),   S( 73,   5),   S( 90,   5),   S( -6,  25),   S( -1,  28),   S( 13,  -1),   S( 18, -13),
            S( 54,  26),   S( 20,  20),   S( 61,  17),   S(  7,  33),   S( 31,  23),   S( 34,  30),   S( 53,  23),   S(-11,  32),
            S( -4,  45),   S( 50,  18),   S( 38,  48),   S( 84,  36),   S(102,  38),   S( 30,  36),   S( 24,  29),   S( 20,  30),
            S( 26,  33),   S( 50,  42),   S( 12,  40),   S( 51,  52),   S( 76,  47),   S( 28,  59),   S( 36,  -3),   S(  2,  22),
            S( 10,  42),   S(-15,  45),   S( 23,  43),   S( 24,  72),   S( -1,  22),   S( 15,  48),   S( 10,  38),   S( 15,   0),
            S(  9,  20),   S( -3,   3),   S(  6,  29),   S(  2,   1),   S( 24,  48),   S(  2,  34),   S( 12,  27),   S(-26, -23),

            /* knights: bucket 6 */
            S( -3, -49),   S( 20, -12),   S( 42,  28),   S(  1,  16),   S(-28,  20),   S( 16,  41),   S(  4,   9),   S( -4,  27),
            S( -7, -41),   S( 66, -10),   S( 65,  -5),   S( 10,  12),   S(-11,  40),   S( 75,  13),   S( 28,  21),   S(-23,  -4),
            S(-28, -20),   S(  1,  -4),   S( 52,  -5),   S( 40,  15),   S( -8,  35),   S(-22,  41),   S( -2,  43),   S(  3,  52),
            S( 34,   1),   S( 62,   7),   S( 84,  16),   S( 90,   6),   S( 75,   5),   S( 62,  24),   S( 42,  32),   S( 35,  53),
            S(  8,  32),   S( 97, -17),   S( 68,  24),   S(103,  21),   S(141,  14),   S(132,  25),   S( 53,  26),   S( 34,  38),
            S( 14,  28),   S( 23,   0),   S( 75,  13),   S( 60,  41),   S( 50,  65),   S( 54,  40),   S( 54,  38),   S( 38,  29),
            S( 13,  15),   S( 30,  15),   S(-36,  26),   S( 40,  22),   S( 25,  36),   S( 38,  55),   S( 32,  92),   S(-23,  -2),
            S(-48, -10),   S( 10,  23),   S( 42,  12),   S( 28,  33),   S( 14,  13),   S(  2,  11),   S( 14,  51),   S( 15,  30),

            /* knights: bucket 7 */
            S(-39, -49),   S(-193, -77),  S(-87, -64),   S(-27, -35),   S(-23,   0),   S(-21, -29),   S(  6, -20),   S( -3,  -3),
            S(-36, -93),   S(-48, -35),   S(-41, -35),   S( -9, -30),   S(-41,  10),   S( 48, -43),   S( 22,  40),   S( 18,  43),
            S(-106, -98),  S(-39, -32),   S(-30, -10),   S( 47, -27),   S( 24, -14),   S( 29, -12),   S(  7,  36),   S( 53,  34),
            S(-41, -21),   S( 48, -29),   S( 19, -12),   S( 72, -11),   S( 79, -15),   S( -2,   7),   S( 34,  -2),   S( -7,  36),
            S(-64, -42),   S(  0, -47),   S( 68, -26),   S(108, -27),   S(116,   1),   S( 73,  29),   S(103,  -6),   S( 69,  17),
            S( -1, -31),   S(  4, -23),   S(  4,  -5),   S( 74, -13),   S( 93,  -1),   S(132,  -4),   S( 75, -23),   S(  6,   5),
            S(-32, -42),   S(-41, -22),   S( 13, -22),   S( 33,  22),   S( 61,   6),   S( 66,   5),   S( -2,  28),   S( 44,  17),
            S(-39, -26),   S( -3,   9),   S( -8, -27),   S( 17,   4),   S( 16, -32),   S(  8,  -1),   S(-16, -48),   S( -4, -10),

            /* knights: bucket 8 */
            S( -4,  -6),   S( -3,   3),   S( -4,  17),   S(-18, -41),   S(-20, -61),   S(-13, -60),   S( 13,  14),   S( -8, -28),
            S(  1,  -6),   S( -9, -11),   S( -2, -13),   S(-27, -31),   S(-25, -40),   S(-10, -67),   S(-23, -75),   S(-19, -59),
            S( -3,  -4),   S(-28, -50),   S( 40,  17),   S( -6, -12),   S( -6, -20),   S( -6,   2),   S( -5, -19),   S( -7, -44),
            S(-27,   3),   S(  2,  23),   S( 17,  30),   S(  7,  42),   S( 30,  19),   S( 21,  13),   S( -7, -43),   S( -7, -29),
            S( 28,  84),   S( -7,  10),   S( 32,  29),   S( 34,  45),   S(  9,  30),   S(  1,  -9),   S(  2,  -1),   S( -7, -11),
            S(  8,  49),   S( -9, -15),   S( 23,  21),   S( 44,  30),   S( -9,   8),   S(-15, -21),   S(-14, -40),   S( -7, -10),
            S( 13,  35),   S( -3,  -5),   S(  4,   4),   S(  3, -10),   S( -5,   3),   S(  5,   6),   S(  1,   0),   S(  0, -10),
            S(  3,   1),   S( 13,  37),   S(  2,  17),   S(  6,  21),   S(  9,  26),   S( -5, -10),   S(  2,  17),   S( -4, -17),

            /* knights: bucket 9 */
            S(-10, -48),   S(-30, -48),   S(-21, -51),   S( -8, -15),   S(-28, -76),   S(-17, -72),   S( -1, -10),   S(  0,  -7),
            S(-15, -62),   S(-14,  -8),   S(-13, -72),   S(-23, -43),   S( 16,  31),   S(-12, -21),   S( -1,   9),   S(-13, -40),
            S(  4, -20),   S(  2,  -3),   S( 16,  -3),   S(  2,   0),   S(-12,  -3),   S(-46, -17),   S( -2,  16),   S( -9, -14),
            S( -9, -11),   S(-11, -16),   S( -4,  11),   S(  8,   8),   S( 33,  28),   S( 10,  28),   S(-21, -67),   S(  0, -18),
            S( -7,  22),   S(  3,  -1),   S( 21,  28),   S( 14,  20),   S( 10,   7),   S( 10,  -9),   S( -1, -25),   S(  7,   0),
            S(  6,  28),   S( 15,  33),   S( 10,  15),   S( -9,   2),   S( 37,  38),   S( 16,  42),   S( 10,  37),   S(-14, -34),
            S(  6,  23),   S( -3,  12),   S( 12,  51),   S(  1,  30),   S( 14,  60),   S(-12, -40),   S(  4,   7),   S(-12, -17),
            S( -2, -11),   S( -4,  -9),   S(  9,  23),   S(  8,  38),   S(  7,   0),   S(  4,  28),   S(  6,  20),   S(  0,  -4),

            /* knights: bucket 10 */
            S(-23, -69),   S(-20, -65),   S( -7, -49),   S(-25, -38),   S(-16, -54),   S( -7, -19),   S( -2, -15),   S( -1,  -2),
            S(-14, -78),   S(-13, -47),   S(  3,  10),   S(-16, -23),   S(-20, -24),   S(-11, -65),   S(  2,  34),   S( -6, -15),
            S(-20, -65),   S(-21, -61),   S(-10,  -8),   S(-25, -21),   S( 20, -16),   S(-10,  -2),   S(-10, -17),   S(-13, -16),
            S(-11, -28),   S( -2, -37),   S(  6, -30),   S( 30,  -1),   S(  7,  12),   S( 33,  11),   S( -2,   9),   S(  7,  11),
            S(-10, -49),   S(-24, -37),   S(-12,  14),   S( 51,   3),   S( 20,  11),   S( 16,  14),   S( 10,  15),   S( 20,  54),
            S(-12, -28),   S(  1, -16),   S( -3,  -4),   S( 13,  23),   S( 31,  36),   S( 25,  37),   S( 35,  92),   S( 13,  65),
            S(  6,  26),   S( -5,  -3),   S( -8, -20),   S( 18,  50),   S( 17,  28),   S( 22,  55),   S( 10,  26),   S(  9,  18),
            S( -2, -14),   S( -1,  -5),   S( -6,   0),   S(  3,  18),   S( 18,  57),   S( -1,  14),   S(  4,   2),   S( -2,  -7),

            /* knights: bucket 11 */
            S(  1,   2),   S(-13, -10),   S( -9, -57),   S(-15, -36),   S(-23, -64),   S(-10, -19),   S( -9, -25),   S( -3,   2),
            S(-14, -18),   S( -9, -24),   S(-18, -58),   S(  2, -13),   S(-11, -23),   S(-31, -28),   S( -8, -26),   S(-10, -13),
            S(-22, -86),   S(-35, -76),   S(-30, -33),   S(  5,   7),   S(-35,  -8),   S(-28,  13),   S(  5,   1),   S( -8,  -9),
            S(-10,  -7),   S(-11, -33),   S(-20,  21),   S( 13,  24),   S( 32,  28),   S( 33,  30),   S(-12,  29),   S( 10,  21),
            S(-14, -52),   S(-34, -76),   S(  2, -22),   S( 16,  -8),   S( 14,  22),   S( 59,  51),   S(  3,  -8),   S( 16,  53),
            S( -8, -23),   S( -5, -19),   S( -5,   9),   S( 34,  36),   S( 10,   2),   S( 48,  37),   S(  1,   4),   S( -1,  47),
            S(  4,  24),   S(  0,  -1),   S(  1, -18),   S( 12,  10),   S( 24,  56),   S(-10,  11),   S(  8,  33),   S( 11,  64),
            S( -3,  -4),   S( -4, -28),   S( 18,  37),   S(  2,   7),   S( -1,  17),   S(  7,  20),   S(  8,  23),   S(  5,  42),

            /* knights: bucket 12 */
            S( -3, -13),   S(  0,  -3),   S( -1, -13),   S(  1,  13),   S( -2,  -8),   S( -1,  -5),   S(  3,  -1),   S( -1, -11),
            S( -3, -10),   S( -2,  -9),   S(  6,  19),   S(  0,  -8),   S(-12, -12),   S( -3, -19),   S(  1, -10),   S(  2,  -4),
            S(  3,  13),   S(  2,   5),   S( -1,  -8),   S( -5,  -2),   S(  5,  -2),   S(-10, -39),   S(  1,  -2),   S( -9, -22),
            S( -2,   5),   S(  0,   7),   S(  2,  11),   S(  7,  61),   S(  4,   4),   S(  2,   9),   S( -5, -21),   S(  3,  -5),
            S( 11,  33),   S(  6,  21),   S( -1,   9),   S(  8,  18),   S(-10, -42),   S(  3,  13),   S(  5,  -2),   S(  2,   8),
            S(  6,  34),   S( -4,  -9),   S(  1,  21),   S(  7,  17),   S(  8,  -2),   S( -3, -18),   S( -2, -14),   S( -3,  -3),
            S(  4,  16),   S( -7, -12),   S(-10, -13),   S( -9, -10),   S( -2,  -7),   S( -6, -28),   S( -2,  -9),   S( -2,  -6),
            S(  3,   9),   S(  4,  26),   S( -4, -13),   S(  0,  -1),   S( -3,  -5),   S( -3, -13),   S( -5, -18),   S(  1,   3),

            /* knights: bucket 13 */
            S( -4, -20),   S( -2,  -3),   S( -5, -13),   S( -5, -11),   S(-14, -34),   S( -1, -11),   S( -3, -14),   S( -1,  -3),
            S( -1,  -7),   S(  4,  22),   S( -1, -13),   S( -3,   3),   S( -5, -33),   S(  1, -16),   S( -1,  -8),   S(  2,  -1),
            S( -1,  -4),   S( -9, -23),   S( 12,  50),   S( -3, -19),   S( -1,   9),   S( -5, -12),   S( -1,  -6),   S( -6, -26),
            S(-10, -24),   S(  8,  16),   S(  4,  30),   S(-13, -34),   S(  3,  24),   S( 16,  23),   S(  0, -12),   S( -8, -13),
            S(  5,  13),   S( -4,  33),   S( 15,   5),   S(  6,  27),   S(  4,   1),   S(  3,   4),   S( -6, -15),   S(  3,   9),
            S(  2,  24),   S( 13,   2),   S(  1,  61),   S(-10,   9),   S( -2,  15),   S(-10, -51),   S(  0,   4),   S( -6, -14),
            S(  0, -10),   S( -1,  19),   S(  0,  -6),   S( -1,  12),   S( 12,  34),   S( -2,  -8),   S( -3,  10),   S( -8, -17),
            S(  0,   3),   S( 11,  67),   S( -4,   8),   S(  6,  37),   S(  1,  21),   S(  2,  13),   S(  0,  -3),   S(  0,  -3),

            /* knights: bucket 14 */
            S( -3,  -9),   S( -2, -18),   S( -3,   1),   S(  3,  22),   S(-13, -40),   S( -2, -19),   S( -3, -12),   S(  1,  -1),
            S( -2,  -9),   S( -3, -16),   S(-15, -61),   S( -1, -18),   S(  5,   2),   S( -1,  -4),   S(  2,   8),   S( -1,  -2),
            S( -8, -24),   S(  3,   0),   S(-17, -51),   S(  5,  -3),   S(  5,  22),   S(  1,   2),   S(  0,   5),   S(  1,   0),
            S(  3,  13),   S( -5, -17),   S( -3, -27),   S( -2,  -3),   S( -2, -29),   S( 13,  19),   S(  6,  13),   S(-11, -21),
            S( -2,  -2),   S( -4, -23),   S(  8,  13),   S(  4,  -9),   S( -7, -15),   S(  0, -13),   S(  4,  17),   S( -2, -23),
            S(  1,   1),   S(  6,  15),   S( -3,  13),   S(  6,   7),   S(  7,  16),   S( -5,   8),   S(  3,  14),   S(  2,  19),
            S(  1,   5),   S( -5, -12),   S(  8,   3),   S( -3, -13),   S( -7,  18),   S(  1,  42),   S(-10,   0),   S(  2,  24),
            S(  0,  -4),   S(  3,  10),   S( -2,   1),   S( 12,  48),   S(  7,  18),   S( 12,  61),   S(  6,  32),   S(  1,   1),

            /* knights: bucket 15 */
            S( -3,  -6),   S( -1, -28),   S( -4, -26),   S( -7, -22),   S( -4,  -5),   S( -5, -26),   S(  2,   1),   S(  1,   4),
            S( -2, -13),   S(  1,  -3),   S(-11, -42),   S( -9, -42),   S(  2,  -2),   S(  4,  13),   S( -4, -21),   S(  0,   0),
            S( -4, -11),   S( -4, -10),   S( -8, -33),   S(-12, -52),   S( -5, -24),   S( -6, -21),   S( -2,  -4),   S( -3, -15),
            S( -7, -18),   S( -4, -23),   S( -9, -41),   S( -3,  15),   S( -8, -42),   S(  9,  28),   S( -5, -16),   S( -7, -21),
            S(  1,   6),   S( -5, -11),   S(  1,  -7),   S( -1,  -4),   S(  6,   9),   S(  9,  16),   S( -1,  -6),   S( -3,  -6),
            S(  0,   5),   S( -3, -16),   S( -3, -20),   S( -2,   6),   S( -4, -11),   S( -1,  13),   S( -2,  -1),   S(  0,  27),
            S( -2,  -9),   S( -4,  -8),   S( -3, -15),   S( -3,  -1),   S( -7,  -5),   S(  4,  33),   S( -5, -14),   S(  2,  19),
            S(  0,   0),   S(  0,   2),   S(  0,   1),   S( -5, -12),   S( -2,  -5),   S( -2,  -9),   S(  9,  32),   S(  1,   5),

            /* bishops: bucket 0 */
            S( 12,  40),   S(  4,  -4),   S( 65,  -1),   S( 12,  15),   S(  5,   0),   S( 12, -19),   S( 24, -49),   S( -9,   1),
            S( 25, -23),   S( 84,   6),   S( 30,  16),   S( 20,   2),   S( -4,  27),   S(  7, -15),   S(-21,  -7),   S( -7, -20),
            S( 24,  34),   S( 34,  26),   S( 29,  42),   S(  2,  53),   S( 21,  31),   S(-23,  58),   S(  3,   8),   S( 20, -51),
            S( 42,  -6),   S( 64,   7),   S( 33,  23),   S( 32,  41),   S( -9,  55),   S( 34,  11),   S( -3,  26),   S(  7,  -3),
            S( -4,  34),   S( 22,  31),   S(-13,  50),   S( 51,  23),   S( 17,  39),   S(-15,  28),   S( 23,  18),   S(-62,  31),
            S(-38,  83),   S(-27,  61),   S( 29,  19),   S( 51,  47),   S(-12,  77),   S(-13,  24),   S(-38,  35),   S(-24,  49),
            S(-63,  74),   S(-10,  45),   S(  7,  54),   S(-11,  72),   S(-68,  48),   S(  0,  32),   S( 30,  21),   S(-40,   6),
            S(-25, -16),   S( -2,  52),   S(-24,  26),   S( -3,  23),   S(  9,  43),   S( 43,  32),   S(-38,  35),   S( -5,  50),

            /* bishops: bucket 1 */
            S( 49,  49),   S( -6,  31),   S(  4,  42),   S(  1,  37),   S( 26,   0),   S(  3,  36),   S( -2,  21),   S(-42,   3),
            S( 24,  14),   S( 45,  18),   S( 48,  25),   S( 36,  32),   S(  3,  20),   S( 26,  16),   S( -6,  24),   S( 24, -16),
            S( 45,  -4),   S( 14,  31),   S( 47,  30),   S( 11,  39),   S( 14,  42),   S( -8,  36),   S( 25,   5),   S( 18,  -7),
            S( 48,  14),   S( 46,  11),   S(  7,  44),   S( 16,  65),   S(-21,  77),   S( 29,  24),   S(-34,  53),   S( 25,   4),
            S( 38,  52),   S(-12,  61),   S( 22,  20),   S( -4,  49),   S( 13,  44),   S(-17,  61),   S( 34,   3),   S( -1,  22),
            S(-45,  71),   S( 16,  43),   S( -8,  76),   S( 55,  25),   S(-24,  66),   S(-27,  63),   S( -5,  53),   S( 27,   2),
            S(-12,  71),   S( 25,  45),   S( 15,  41),   S( 25,  60),   S( 33,  52),   S(-24,  70),   S( 16,  55),   S(-29,  60),
            S( 33,  74),   S(-45,  37),   S(-11,  34),   S(-48,  49),   S( 11,  29),   S(-36,  56),   S(-23,  63),   S(-22, 102),

            /* bishops: bucket 2 */
            S( 20,  27),   S(  7,  26),   S(  4,  36),   S(-21,  51),   S(-10,  32),   S(-13,  36),   S(-24,  -9),   S(-38,  63),
            S(-12,  40),   S( 17,  24),   S( 23,  24),   S(  5,  41),   S(  4,  38),   S(  3,  26),   S( 12,  11),   S(  2, -23),
            S(  5,  19),   S(  2,  40),   S( 11,  58),   S( -4,  62),   S(  0,  50),   S( 12,  48),   S( -4,  48),   S( -7,  26),
            S( 21,  29),   S(-22,  63),   S(-15,  58),   S( -2,  62),   S( 10,  59),   S(  1,  55),   S( 10,  41),   S( -2,  15),
            S( -8,  49),   S( -6,  45),   S(-29,  51),   S(-19,  60),   S(-23,  70),   S( -4,  60),   S(  5,  39),   S(-32,  37),
            S( -9,  54),   S( -9,  48),   S(  1,  69),   S(-18,  59),   S( 29,  34),   S(-12,  73),   S( 25,  60),   S(-12,  54),
            S( 27,  31),   S( -8,  65),   S(-37,  72),   S( -7,  49),   S(-41,  73),   S(-29,  51),   S(-95,  76),   S(-38,  65),
            S(-125, 115),  S(-54, 100),   S(-94,  76),   S(-86,  73),   S(-69,  58),   S(-80,  69),   S(  6,  49),   S(-65,  61),

            /* bishops: bucket 3 */
            S(-22,  35),   S( 12,  14),   S( 13,  26),   S(  6,  32),   S(  6,  12),   S( 39,   7),   S( 27, -11),   S( 43, -11),
            S(  2,  37),   S(  8,  28),   S( 18,  20),   S(  5,  47),   S( 13,  34),   S( -1,  43),   S( 47,  36),   S( 19,   7),
            S( 24,   7),   S(  2,  47),   S(  9,  58),   S( 13,  43),   S(  5,  65),   S( 15,  62),   S( 12,  44),   S( 38,  15),
            S( 35,  18),   S( -7,  57),   S(  0,  70),   S(  8,  69),   S( 18,  69),   S( 16,  44),   S( 12,  47),   S(  1,  17),
            S( -8,  40),   S( 16,  40),   S(  6,  40),   S( 17,  66),   S( 10,  67),   S( 20,  57),   S(  2,  52),   S(  3,  60),
            S( 14,  42),   S( 10,  47),   S( 19,  64),   S( 25,  40),   S( 18,  50),   S( 47,  53),   S( 19,  51),   S( -7,  70),
            S(-12,  44),   S( -8,  69),   S( 33,  36),   S(  5,  58),   S( -8,  43),   S(-15,  47),   S(-23,  70),   S(  2,  76),
            S(-42, 116),   S(-24,  61),   S( 22,  60),   S(-20,  70),   S(-49,  65),   S(-56,  84),   S( -6,  63),   S( 56,   8),

            /* bishops: bucket 4 */
            S( -5, -13),   S(-25,  -2),   S(-60,   8),   S(-38,  28),   S(-34,  10),   S(-25,  26),   S( -2,   8),   S( -2,  -8),
            S(-30,  25),   S( 25,   0),   S(  1,  20),   S(  3,  14),   S(-31,  21),   S( 88,   3),   S( 11,  -3),   S( 46,  14),
            S(-14,  -3),   S(-36,  43),   S(  7,  27),   S(-22,  37),   S( 48,   8),   S( 60,   7),   S(-16, -11),   S(-59,  16),
            S(-44,  40),   S(-17,  27),   S( 27,  21),   S( 34,  35),   S(  8,  32),   S( -5,   8),   S( 32,  54),   S(-18, -17),
            S( 18,  -5),   S(-13,  50),   S(-56,  63),   S( 18,  33),   S(  7,  21),   S(-26,  26),   S(-58,  11),   S( -8,  37),
            S(-18,   2),   S( 16,  25),   S( 13,  30),   S( 21,  15),   S( 18,  40),   S(  1,  33),   S(-10,  -1),   S(  0,   2),
            S(-38,  49),   S( 39,  47),   S(-14,  30),   S(  9,  54),   S( -1,  23),   S( -1,  24),   S( 14,   3),   S(  1,   1),
            S(  5,  44),   S(-13, -13),   S( 12,  27),   S(-15,   9),   S( -8,  42),   S( -4,   7),   S(  6,  17),   S( -4,  14),

            /* bishops: bucket 5 */
            S(  9,   9),   S( -9,  23),   S( -1,  25),   S(-35,  38),   S(-47,  31),   S(-21,  22),   S( -9,  40),   S( 14,  37),
            S( -4,  17),   S(-27,  51),   S(-26,  64),   S( 30,  19),   S( -9,  32),   S(  5,  20),   S(-14,  26),   S(-16,  11),
            S(-19,  43),   S(-60,  65),   S( 24,  41),   S( 14,  29),   S(  3,  44),   S(-31,  42),   S(-22,  33),   S( 24,  -2),
            S( 29,  24),   S( 22,  36),   S(-27,  58),   S( 44,  34),   S( 49,  41),   S( 25,  40),   S( 28,  30),   S( 11,  45),
            S( 17,  58),   S(-24,  42),   S( 52,  17),   S(101,  31),   S( 57,  24),   S( 33,  27),   S( 11,  42),   S(-21,  26),
            S( 34,  36),   S( 34,  41),   S( 63,  41),   S(  1,  49),   S(-11,  59),   S( 24,  24),   S(-14,  54),   S( 22,  42),
            S( -8,  62),   S(-40,  54),   S(-19,  45),   S( 14,  52),   S(-12,  47),   S( 11,  66),   S(  0,  45),   S( -8,  13),
            S(-20,  40),   S( 19,  33),   S(  9,  13),   S( -6,  47),   S( 33,  49),   S( -2,  49),   S( 13,  73),   S(  0,   7),

            /* bishops: bucket 6 */
            S(-19,  49),   S(-15,  53),   S(-11,  40),   S(-40,  35),   S(-57,  40),   S(-28,  35),   S( 14,  42),   S(-16,  46),
            S( 19,  22),   S( 14,  20),   S(-27,  49),   S(-29,  57),   S(  6,  31),   S( -3,  38),   S(-130,  81),  S( 18,  47),
            S(-12,  28),   S( -2,  34),   S( 19,  44),   S( 46,  24),   S( 67,  13),   S( 54,  30),   S( 24,  43),   S(-46,  49),
            S( 23,  38),   S(-53,  66),   S( 29,  43),   S( 52,  35),   S( 28,  48),   S( 27,  40),   S(  9,  54),   S(-17,  23),
            S(-28,  50),   S( 32,  37),   S( 85,  18),   S( 48,  33),   S(114,  34),   S( 83,  32),   S( 52,  37),   S( 17,  39),
            S( -9,  32),   S(-14,  41),   S( 27,  43),   S( 31,  48),   S( 21,  52),   S( 51,  50),   S( 49,  34),   S(-11,  70),
            S(-14,  38),   S( -2,  39),   S(  6,  42),   S(  1,  54),   S( 28,  45),   S(  6,  47),   S(  9,  59),   S( -7,  51),
            S(  5,  88),   S( -7,  66),   S( 14,  36),   S( -8,  60),   S( 12,  43),   S(  2,  36),   S( 10,  40),   S(-11,  54),

            /* bishops: bucket 7 */
            S(  2,   4),   S(-30,  26),   S(-40,  -4),   S(-29,  17),   S(-57,   9),   S(-77,  12),   S(-84, -35),   S(-63,  -7),
            S(-30,  -4),   S(-85,  19),   S(-24,  21),   S(  6,   7),   S(-30,  24),   S(-88,  39),   S(-33,   4),   S(-26, -36),
            S(-65,  20),   S( 22, -17),   S( 18,  12),   S( 24,  21),   S(-40,  35),   S( -7,  18),   S( -1,  34),   S(-29,  19),
            S(-61,  30),   S( 32,  22),   S( 82,  -1),   S( 39,  22),   S( 98,   9),   S( -8,  29),   S( 24,  35),   S(  0,   2),
            S(-39,   8),   S(-29,   9),   S( 33,  -2),   S( 59,  11),   S( 70,  26),   S( 72,  19),   S( 28,  32),   S( 58, -13),
            S(-13,  -6),   S(-22,  19),   S( 29,   8),   S( 55,  -1),   S( 21,  11),   S( 68,  38),   S( 32,  37),   S(  2,  33),
            S( 10,   6),   S(-33,  29),   S( -8,  22),   S(-28,  22),   S(-27,   8),   S( 14,  19),   S( 38,  45),   S( 37,  40),
            S( -3,   8),   S(-29,  34),   S(-25,  26),   S(  8,  16),   S(-14,   8),   S( -6,  16),   S( 19,  25),   S( 15,   0),

            /* bishops: bucket 8 */
            S( -3, -41),   S( -3, -34),   S(-47, -36),   S( -4, -10),   S(  7,  -5),   S( -7, -26),   S( 13,  22),   S(-19, -50),
            S( -5, -32),   S(-27, -83),   S( -9, -26),   S(-10,  -3),   S( 29,  -4),   S(-19, -64),   S(-23, -42),   S(  2, -24),
            S( 14,  23),   S(-19,   3),   S( -9,  -1),   S(  8, -38),   S( 14, -23),   S(  5, -33),   S( -1, -71),   S(-40, -46),
            S(  8,  37),   S( 17,   2),   S(  5,  24),   S( 24, -16),   S(  2,   0),   S(  4, -35),   S(  5,  -3),   S(  3, -13),
            S( 15,  41),   S( 27,  46),   S(  5,   0),   S( 68,  -9),   S( 24, -26),   S( 12,  -6),   S(  8, -29),   S( -7, -44),
            S( -7,   9),   S( 16,  -6),   S( 11,  -1),   S( -7, -13),   S( 23,  -7),   S( -1, -30),   S( -9, -40),   S(-20, -56),
            S( -7,   7),   S( 33,  13),   S( 11,  11),   S(-12, -27),   S(  1, -26),   S(  6, -14),   S(  0, -38),   S(-15, -42),
            S( -7, -20),   S( 11, -28),   S(  0, -13),   S(  0, -17),   S( -6, -30),   S( -4, -33),   S( -5, -41),   S( -9, -40),

            /* bishops: bucket 9 */
            S(-16, -52),   S( 10, -56),   S(-14,   2),   S(  5, -32),   S(-10, -56),   S(-14, -54),   S( -9, -21),   S(  9,  26),
            S(-11, -37),   S( -7, -25),   S( -4,  -8),   S( 12, -13),   S(-10, -23),   S( -4, -30),   S( 10, -30),   S( -6,  -6),
            S( -1, -31),   S( 11, -16),   S(  8, -36),   S( 12, -16),   S( 38, -24),   S( 34, -19),   S( -5,  -9),   S( 12,   1),
            S(-10,  -4),   S( 24,  13),   S( 28,  -9),   S( 52, -28),   S( 43, -15),   S( 17,  -9),   S( -3, -52),   S(-14, -41),
            S(  2,  -3),   S( 27, -13),   S( 48,  11),   S( 46,   9),   S( 25, -32),   S( 39,  -9),   S( 14,   0),   S( -5, -14),
            S(  4, -26),   S( 55,  26),   S( 11,  15),   S( 13,  -2),   S( 39, -10),   S( 21, -33),   S(  9, -51),   S(-15, -33),
            S(  7,  16),   S( 31,  16),   S(  0, -32),   S( 13, -11),   S( 20, -10),   S( 14, -35),   S(  2, -11),   S( -4, -37),
            S(  0,  -9),   S( -1,  -6),   S(  1, -38),   S( -9, -53),   S( -3, -41),   S( 13,  14),   S( -2, -22),   S( -5, -45),

            /* bishops: bucket 10 */
            S(-18, -37),   S( 18, -31),   S(-27, -46),   S(-11, -51),   S(-10, -22),   S(-22, -46),   S(-10, -53),   S(-15, -77),
            S(  7, -19),   S( -9, -45),   S( 18, -54),   S(-10, -40),   S( -5, -39),   S( 12, -39),   S( -7, -48),   S( -7, -20),
            S( 11, -38),   S( 27, -38),   S( -4, -48),   S( 38, -35),   S( 49, -42),   S( 15,  -1),   S(-23,   6),   S( 13,   5),
            S( -5, -40),   S( 26, -27),   S( 21, -23),   S( 33, -26),   S( 73, -29),   S(  2,  -5),   S( -8,  -3),   S( 10,  16),
            S(-17, -41),   S( 37, -32),   S( 46, -33),   S( 68, -18),   S( 39, -11),   S( 31, -25),   S( -5,  12),   S(  6, -30),
            S( -1, -45),   S( 22, -47),   S( 32, -37),   S( 35, -34),   S( 41, -13),   S( 45,  -2),   S( 18,  -3),   S( -1, -46),
            S(-12, -76),   S( -5, -68),   S( 16, -40),   S( 26, -24),   S(  5, -34),   S( 22,  20),   S( 17,  18),   S( 16,  31),
            S( -7, -58),   S(  3, -38),   S( 12,  14),   S(  4, -14),   S(-12, -39),   S( -7, -48),   S( -4, -22),   S(  3,  14),

            /* bishops: bucket 11 */
            S(-14, -24),   S(-29, -20),   S(-47, -65),   S(-14, -31),   S(-16,   6),   S(-61, -55),   S(-19, -62),   S(-14, -53),
            S( -7, -75),   S( 29, -34),   S( -8, -19),   S(-18, -24),   S(-30, -35),   S(-13, -68),   S(  1, -33),   S(-29, -71),
            S(-20, -96),   S( 13, -49),   S( 17, -41),   S( 38, -52),   S(-10, -21),   S(  8, -41),   S(-14, -38),   S( -9, -21),
            S(-18, -49),   S(  4, -40),   S( 29, -35),   S( 37, -49),   S( 48, -45),   S( 39,  -6),   S( 21, -16),   S(  9,  31),
            S(-14, -35),   S(  0, -48),   S( 28, -38),   S( 61, -34),   S( 37, -14),   S( 37,  -6),   S( 12,  33),   S( 26,   6),
            S(-12, -51),   S( -1, -68),   S(  4, -39),   S( 17, -16),   S( 16, -13),   S( 21,  -2),   S( 32,  30),   S( -9, -12),
            S( -3, -30),   S( -3, -63),   S(-20, -44),   S( -6, -83),   S(  9, -10),   S( 23, -37),   S(  3, -21),   S( -3, -13),
            S(-15, -74),   S(-20, -52),   S( -2, -33),   S( 10, -22),   S( 20, -24),   S( -1, -33),   S(-13, -67),   S( 10,  22),

            /* bishops: bucket 12 */
            S(  4,   7),   S( -6, -25),   S(-15, -50),   S(-12, -44),   S(  4, -20),   S( -4, -29),   S(  2,   5),   S( -1,  12),
            S( -3, -17),   S(-16, -60),   S(  0,  -6),   S( -9, -17),   S(-14, -41),   S(  0,  -5),   S( -1, -11),   S( -3,  -8),
            S( -2, -22),   S(-14, -38),   S( -4,   0),   S( -6, -25),   S( -3,   8),   S( -9, -67),   S(-11, -33),   S( -9, -32),
            S( -1, -17),   S(  6, -15),   S(-15, -72),   S(  3,  -9),   S(  3, -23),   S(  3,  -3),   S( -2, -20),   S( -3,  -6),
            S(-11, -31),   S( -9, -28),   S( 12, -34),   S( -3,  -3),   S( -5, -57),   S(  1, -10),   S( -1, -34),   S( -2,   1),
            S(-22, -39),   S(-14,  12),   S( -4,   0),   S(-14, -42),   S( -1, -34),   S(  0, -19),   S(  4, -21),   S( -1,  -6),
            S( -5, -15),   S( -2, -11),   S( -8,  -8),   S(-13, -39),   S( -9, -26),   S(  6,   1),   S( -7, -25),   S( -4,  -6),
            S( -1,  -7),   S( -5,  -5),   S( -3, -44),   S(  0,  -8),   S(  3,  -4),   S(  2,   2),   S(  1, -14),   S(  0,   5),

            /* bishops: bucket 13 */
            S(-11, -65),   S( -5, -56),   S( -4, -28),   S( -7, -65),   S( -4, -30),   S( -4, -18),   S( -7, -33),   S( -7, -40),
            S( -4, -12),   S( -4, -38),   S( -4, -54),   S(-11, -48),   S( -1, -29),   S(  4,  -6),   S( -2, -24),   S(  2, -32),
            S(-11, -43),   S( -5, -16),   S( -4, -26),   S( 11, -52),   S(  2, -70),   S( 10, -50),   S(  0, -18),   S(  5,  11),
            S(  1,  -1),   S( -2, -34),   S(  1, -16),   S( -6, -55),   S( 17, -40),   S(  6, -23),   S(  1,  -2),   S( -7, -43),
            S(  1,   5),   S( -2,  -2),   S(-11, -69),   S( 11, -29),   S(  8,  -5),   S( 11, -29),   S( -6, -39),   S( -3, -33),
            S( -4,  -5),   S(  0, -14),   S(-11, -14),   S( 11,   0),   S( -3, -21),   S( 16, -22),   S( 11, -27),   S( -5, -35),
            S( -8, -44),   S(-13, -52),   S( -6, -21),   S( -2,  -7),   S( -6, -41),   S(  7,  11),   S( -4, -30),   S(  5,   6),
            S( -6, -43),   S( -7, -19),   S( -4, -26),   S(  9,   5),   S(  2,  -3),   S( -8, -55),   S(  2,   4),   S( -6, -36),

            /* bishops: bucket 14 */
            S( -5, -45),   S(-19, -57),   S(-15, -47),   S(-15, -64),   S( -1, -29),   S( -7, -48),   S(-18, -102),  S( -5, -26),
            S(-10, -21),   S(  3,   5),   S(  2, -27),   S(-23, -103),  S( -1, -35),   S( -5, -44),   S(-11, -39),   S( -5, -44),
            S( -1, -16),   S(-10, -42),   S(-12, -68),   S(  3, -52),   S( -6, -77),   S(-15, -86),   S( -6, -35),   S( -1,  -4),
            S( -2, -24),   S( -5, -39),   S(-14, -69),   S(  1, -45),   S(  8, -65),   S(  2, -51),   S(-18, -72),   S(-11, -36),
            S( -9, -36),   S(  7, -22),   S( 12, -16),   S(  5, -34),   S( -4, -70),   S( -4, -13),   S( -2, -45),   S( -3, -19),
            S( -4, -46),   S( -7, -51),   S( -9, -30),   S( -2, -26),   S( -7,  -3),   S( -5, -15),   S(  3, -28),   S( -3, -27),
            S( -6, -41),   S(  5, -22),   S( -5, -29),   S( 12,  -9),   S( -6, -36),   S(  1, -16),   S( -3,  -6),   S( -4, -11),
            S( -5, -28),   S( -3, -17),   S( -8, -42),   S(-13, -51),   S( -2, -14),   S( -2,  -6),   S(  3,  30),   S( -2, -15),

            /* bishops: bucket 15 */
            S( 11,  48),   S(  1,  23),   S(-23, -73),   S(  2,  18),   S(-14, -35),   S( -9, -24),   S( -4, -22),   S( -4, -14),
            S(  4,  12),   S(  1,   1),   S(  3, -19),   S( -5, -28),   S(-11, -40),   S(  3,   7),   S( -3, -11),   S( -1,  -1),
            S( -9, -32),   S( -7, -16),   S(-12, -41),   S( -6, -16),   S( -8, -52),   S( -4, -10),   S( -7, -50),   S(  8,  19),
            S( -4, -21),   S(-17, -59),   S( -1, -25),   S(-18, -56),   S( -3, -44),   S( -6, -38),   S(  5,   6),   S( -7, -15),
            S(  6,  -3),   S(-15, -35),   S( -3, -45),   S(-18, -46),   S( -3, -12),   S( -4, -28),   S( 11, -12),   S( -8, -38),
            S(-10, -38),   S( -2, -58),   S(-10, -28),   S(-12, -56),   S(  6, -30),   S( -1, -27),   S( 19,  45),   S( -2,  -7),
            S( -4, -30),   S(  2,  -1),   S( -3, -16),   S( -1, -16),   S( -7, -25),   S( -2, -11),   S(-10, -29),   S(  2,   5),
            S( -8, -13),   S( -4,  -9),   S( -1, -31),   S( -4, -45),   S( -6, -41),   S(-18, -75),   S(-11, -21),   S( -1,  -9),

            /* rooks: bucket 0 */
            S(-32,  17),   S(  6, -13),   S( 12, -23),   S( 16,  -4),   S( 21,   3),   S( 18, -13),   S(  7,   1),   S( 16,   2),
            S( 13, -63),   S( 44, -27),   S( 20, -15),   S( 20, -13),   S( 43, -19),   S( 30, -30),   S( -5,  -9),   S(-49,  25),
            S(-12,  -2),   S(  6,  11),   S( 34,   2),   S( 15,  15),   S( 11,   4),   S(  4,  13),   S(-23,   9),   S(-43,   2),
            S( 47, -47),   S( 41,  17),   S( 75,  15),   S( 63,  -6),   S( 43,   1),   S(-23,  20),   S(  0,   3),   S(-27,  29),
            S( 55, -24),   S( 99, -15),   S( 74,   4),   S( 52,  14),   S( 65,   2),   S( 41,  -2),   S( 44,  -7),   S(-17,  20),
            S( 66, -65),   S( 86, -27),   S( 77,   7),   S( 44,  18),   S(102, -17),   S( -9,  29),   S( 44,  10),   S(-35,  24),
            S( 53, -12),   S( 89, -16),   S( 18,  15),   S( 32,  20),   S( 37,  20),   S( 55, -16),   S(  4,  34),   S(  2,  24),
            S( 38,  20),   S( 30,  18),   S( 51,   1),   S( 44,   9),   S( -1,  31),   S( 39,  -4),   S(  6,  19),   S( 18,  11),

            /* rooks: bucket 1 */
            S(-76,  36),   S(-36,   9),   S(-34,  -5),   S(-31,  -4),   S( -1, -25),   S(-12,  -7),   S(-19, -11),   S(-31,  15),
            S(-33,  11),   S(-51,  31),   S(-35,  16),   S(-17, -21),   S(-29,   5),   S(-44,   7),   S(-40, -18),   S(-50,  11),
            S( -6,  12),   S(-19,  26),   S(-23,  29),   S(-53,  39),   S(-26,  16),   S(  0,   4),   S(-25,   6),   S(-53,  22),
            S(-56,  51),   S(-53,  45),   S(-28,  60),   S(-13,  37),   S(-62,  64),   S(-43,  55),   S(-20,  43),   S(-38,  23),
            S( 44,  18),   S( 33,  50),   S(  6,  12),   S(-23,  44),   S(-19,  42),   S( 39,  13),   S(-16,  35),   S(-23,   7),
            S( 58,  11),   S( 35,  32),   S( 38,  19),   S( -4,  33),   S( 25,  31),   S(-28,  42),   S( -1,  36),   S(-52,  52),
            S(-28,  47),   S( 18,  51),   S( 10,  46),   S(-52,  79),   S( 17,  34),   S( 13,  38),   S(-41,  31),   S(  2,  25),
            S( 35,  43),   S( 16,  41),   S( 14,  36),   S(-62,  68),   S(  8,  22),   S( 52,   7),   S(-30,  37),   S( 32,   8),

            /* rooks: bucket 2 */
            S(-62,  38),   S(-42,  30),   S(-34,  24),   S(-45,  27),   S(-53,  23),   S(-58,  34),   S(-44,  -5),   S(-56,  35),
            S(-88,  54),   S(-64,  47),   S(-48,  43),   S(-53,  28),   S(-29,   7),   S(-55,   4),   S(-73,  22),   S(-60,  30),
            S(-67,  62),   S(-51,  62),   S(-49,  62),   S(-37,  35),   S(-59,  48),   S(-47,  52),   S(-13,   7),   S(-29,   9),
            S(-77,  73),   S(-55,  70),   S(-35,  80),   S(-28,  66),   S(-18,  43),   S( 10,  35),   S(-29,  51),   S(-19,  26),
            S(-26,  60),   S(-48,  82),   S(-52,  74),   S(  0,  47),   S( 29,  33),   S( 14,  55),   S(-33,  52),   S(-38,  53),
            S(  9,  42),   S(-31,  67),   S(  1,  42),   S( 14,  37),   S( 20,  49),   S( 28,  40),   S( 18,  32),   S( -5,  37),
            S(-59,  68),   S(-48,  82),   S(-14,  72),   S( 11,  57),   S( 38,  29),   S( 41,  22),   S(-44,  71),   S(-25,  65),
            S(-38,  78),   S( 17,  52),   S(-60,  76),   S(-10,  55),   S(-28,  53),   S( -5,  62),   S(-22,  59),   S( 24,  34),

            /* rooks: bucket 3 */
            S(  1,  70),   S(  0,  74),   S( 18,  61),   S( 25,  57),   S( 14,  60),   S( -7,  66),   S( -1,  67),   S(-13,  46),
            S(-42,  86),   S( -2,  62),   S( -7,  76),   S( 17,  56),   S( 17,  61),   S(  9,  58),   S( 31,  16),   S( 26, -33),
            S(-45,  88),   S(-10,  85),   S(  6,  81),   S( 11,  60),   S(  9,  76),   S( 17,  72),   S( 22,  57),   S(  0,  36),
            S(-23,  90),   S(-38,  97),   S( 24,  85),   S( 41,  70),   S( 24,  74),   S(  2, 108),   S( 55,  61),   S(-11,  74),
            S( -2,  97),   S( 25,  82),   S( 11,  76),   S( 55,  65),   S( 48,  70),   S( 51,  76),   S( 87,  45),   S( 52,  46),
            S(  4,  90),   S( 12,  87),   S( 20,  80),   S( 39,  74),   S( 39,  63),   S( 70,  47),   S(105,  29),   S( 78,  26),
            S(-10, 101),   S( -5, 106),   S( 11,  99),   S( 48,  85),   S( 46,  76),   S( 81,  56),   S( 58,  62),   S(139,  14),
            S(-58, 148),   S( 12, 109),   S( 17,  86),   S( 67,  72),   S( 80,  44),   S( 88,  54),   S(153,  34),   S( 91,  50),

            /* rooks: bucket 4 */
            S(-86,  36),   S( -1, -28),   S(-62,   7),   S(-29,  21),   S(-20, -26),   S(  9, -51),   S( -3, -35),   S(  6, -47),
            S(-18, -22),   S(-39,  18),   S(-54,   9),   S(-10,   7),   S(-19,  -7),   S(-33, -36),   S(  7, -46),   S(-59, -13),
            S( -8,  17),   S(-23, -24),   S(-11,   6),   S( 15, -32),   S(-25,  -2),   S( 10, -37),   S( 25, -25),   S(-86, -10),
            S(-45, -16),   S(  3,  -3),   S(-20,  12),   S( 37,  -9),   S( 18,   1),   S( -7, -10),   S(-12,  13),   S(-12,   1),
            S(-49,  -4),   S(-18,  40),   S( 17,  14),   S( 20,  19),   S(  9,  14),   S(-12,  -4),   S( 26,   7),   S( 28,  -6),
            S( 30,   9),   S( 31, -10),   S( 38,  14),   S( 52,  18),   S( 32,   2),   S( 16,  19),   S( 46,  10),   S( 23,  22),
            S(-15,  -4),   S(  7,  29),   S( 57,  12),   S(  7,  35),   S( 24, -14),   S( 12,  -5),   S( 34,  10),   S( 23,  25),
            S(-24, -57),   S( 39,  17),   S( 27,   6),   S( 13,   0),   S( 12,   0),   S( 27,   6),   S(  7, -11),   S( 30,   4),

            /* rooks: bucket 5 */
            S(-53,  45),   S(-18,  29),   S(-42,  43),   S(-23,   6),   S(-22,  12),   S( -7,  17),   S(  2,  11),   S(-22,  23),
            S(-23,  17),   S(-60,  45),   S(-98,  65),   S(-52,  40),   S(-63,  47),   S(-40,  23),   S( 23, -10),   S(-12,  -5),
            S( -4,  35),   S(-70,  54),   S(-70,  60),   S(-111,  56),  S(-28,  17),   S( -3,  23),   S( 21,  18),   S(-27,  27),
            S(-48,  77),   S(  3,  35),   S(-25,  69),   S(-20,  43),   S(-41,  60),   S( 10,  56),   S( -1,  53),   S(  9,  16),
            S(-31,  70),   S(  2,  58),   S( 48,  40),   S( -2,  77),   S( 33,  49),   S( 35,  74),   S( 62,  40),   S( -2,  42),
            S( 35,  73),   S( 59,  64),   S( 72,  58),   S( 47,  52),   S( 75,  43),   S( 86,  50),   S( 49,  54),   S( 60,  44),
            S( 34,  46),   S( 13,  69),   S( 51,  50),   S( 42,  57),   S( 63,  43),   S( 76,  41),   S( 87,  45),   S( 55,  45),
            S( 84,  37),   S( 77,  27),   S( 46,  45),   S( 37,  38),   S( 72,  25),   S( 53,  47),   S( 52,  32),   S( 28,  50),

            /* rooks: bucket 6 */
            S(-65,  33),   S(-32,  31),   S(-17,  27),   S( -7,  14),   S(-17,   5),   S(-40,  36),   S(-66,  63),   S(-13,  33),
            S(-26,  16),   S(-30,  37),   S(-15,  33),   S(-77,  48),   S(-53,  44),   S(-57,  52),   S(-35,  34),   S(  4,  17),
            S(-60,  52),   S(-49,  54),   S(-20,  48),   S(-74,  56),   S(-39,  40),   S(-57,  73),   S(-48,  62),   S( 40,   6),
            S(-50,  71),   S(  8,  50),   S( -7,  66),   S(-36,  67),   S( -4,  42),   S(  2,  63),   S(-81,  85),   S(-26,  49),
            S( 23,  57),   S(  8,  70),   S( 47,  45),   S( 20,  42),   S( 23,  62),   S( 20,  68),   S( 23,  50),   S( -2,  61),
            S(  9,  67),   S( 68,  50),   S( 98,  44),   S( 75,  30),   S( 21,  51),   S( 29,  61),   S( 40,  68),   S( 76,  44),
            S( 69,  53),   S( 57,  52),   S( 95,  31),   S( 82,  32),   S( 91,  38),   S( 57,  54),   S( 74,  42),   S( 50,  54),
            S( 61,  60),   S( 59,  51),   S( 58,  42),   S( 28,  58),   S( 64,  32),   S( 58,  47),   S( 92,  48),   S( 34,  46),

            /* rooks: bucket 7 */
            S(-78,   4),   S(-44,  -5),   S(-23, -12),   S( -5,  -3),   S( 12, -28),   S(-28,  17),   S(-39,  12),   S( 21, -22),
            S(-77,  29),   S(-10,   0),   S(-21, -11),   S( -9,  -6),   S( -4,  10),   S( 27,  -7),   S(-23,   6),   S(-24, -17),
            S(-60,  39),   S(-40,  17),   S(  1,  15),   S(-11,   0),   S( -8,  18),   S(-32,  16),   S( 19, -15),   S(  4,  12),
            S(-33,  33),   S( 27,  16),   S( 17,  19),   S( 32,  15),   S( 23,  12),   S( 62,   3),   S( -2,  11),   S( 15, -14),
            S(  1,  29),   S( 15,  20),   S( 74, -16),   S( 47,  -1),   S( 89,   1),   S( 99,   0),   S( 29,  23),   S( 70, -11),
            S( 18,  32),   S(  7,  27),   S( 95, -15),   S( 86,  -9),   S( 68,   5),   S( 92,  10),   S( 56,  31),   S( 75,  -6),
            S( 22,  29),   S( 72,  13),   S( 74,   5),   S( 99,  -4),   S(104,  -9),   S( 92,   7),   S( 66,  16),   S( 40, -14),
            S( -1,  54),   S(-26,  50),   S( 62,  15),   S(126, -20),   S( 40,   4),   S( 10,  21),   S( 53,  -3),   S( 64,   5),

            /* rooks: bucket 8 */
            S(-39, -45),   S(-27,  -4),   S(  0,  14),   S(-34,  -8),   S(-31, -49),   S(-39, -71),   S(-30, -45),   S(-29, -11),
            S( -6,  13),   S( -8, -27),   S(  6,   5),   S( -2, -19),   S(-25, -18),   S(-17, -51),   S(-20, -60),   S(-15, -55),
            S( -2,   5),   S( -7,  -6),   S( -6, -24),   S(  1,  -5),   S(-30, -42),   S(-28, -50),   S( -2,  35),   S(-17,   7),
            S( -8,   1),   S(-12,   6),   S( -8,   9),   S(  9,  -1),   S( -1,  13),   S(-39, -25),   S( 13, -27),   S(-10,   1),
            S(-18, -36),   S(  4,  12),   S(-14,  20),   S(  7,  19),   S( -1,   9),   S( 13,   5),   S( -7, -22),   S( -6, -34),
            S( -3,  28),   S(-17,  22),   S(  5,  36),   S( 26,   9),   S( -7,   6),   S( -6, -19),   S( -5, -18),   S( -6,  17),
            S(-17,  -4),   S(  0,  -4),   S( 24,  35),   S( 12, -20),   S( 29,  20),   S( -2, -15),   S( 10,  -9),   S( 10,  -7),
            S(  1, -109),  S(  0, -28),   S( 10,  -1),   S(  3,  -5),   S( -8,  -1),   S(  0, -31),   S( 13,  19),   S(  4,   9),

            /* rooks: bucket 9 */
            S(-82, -22),   S(-13, -37),   S(-39, -54),   S(-58, -29),   S(-47,  -1),   S(-30, -27),   S( -2, -77),   S(-49, -47),
            S( 18, -26),   S(-30, -59),   S(-34, -49),   S(-34, -36),   S(-37, -29),   S(  8,  -1),   S(  2, -57),   S(-38, -44),
            S(-19, -42),   S( 13, -23),   S(  3,  -9),   S(-34, -19),   S(-41, -33),   S( 10, -24),   S( 24, -15),   S(-13, -19),
            S( -4,  -8),   S( -7,  -2),   S(-11,   3),   S(-22,  -5),   S(  5, -11),   S(-14, -12),   S(  9,  23),   S(  0,   2),
            S(-18, -12),   S(-35, -12),   S(-22,  -9),   S( 20,  47),   S(  8,  21),   S( 11,  -2),   S(  9,  19),   S( -7, -39),
            S( 14,  23),   S( -1, -13),   S( -5,  -4),   S(-17,   4),   S(  5, -17),   S( 14,  -7),   S(  9,  -5),   S( -6, -36),
            S( 30,   6),   S( 32,   4),   S( 44, -13),   S( 33,  18),   S(  6,  -7),   S( 34,   0),   S( 44, -23),   S( 40,  -4),
            S( 48, -86),   S( 33, -50),   S( 16,  -2),   S(  8,  19),   S( 10,   9),   S( -1, -17),   S( 11, -26),   S( 30,   0),

            /* rooks: bucket 10 */
            S(-74, -93),   S(-33, -69),   S(-49, -67),   S(-47, -37),   S(-43, -50),   S(-69, -56),   S(  0, -57),   S(-35, -38),
            S(-14, -47),   S(-19, -53),   S( -3, -46),   S(-26, -34),   S(-13, -46),   S(-12, -27),   S(  4, -12),   S(  5, -41),
            S(-28, -47),   S(-22, -34),   S(-18, -31),   S(-16, -18),   S(-31, -25),   S( -1, -41),   S( 31,   5),   S(-12, -19),
            S(-27, -24),   S(  6, -28),   S(-28, -19),   S( -3, -27),   S( -1,  -2),   S(-11, -28),   S(  2,  20),   S( -7, -58),
            S(  4, -20),   S( 13, -29),   S(-17, -13),   S( 13, -51),   S(-11, -19),   S( -2,  -2),   S( 26,   2),   S( -1,  14),
            S( 35, -11),   S( 15,   3),   S(  7,  -6),   S(  0, -23),   S(-31, -15),   S(  4, -21),   S( 19,  -7),   S(  0,  -3),
            S( 73,   2),   S( 77, -28),   S( 61, -12),   S( 23, -37),   S(  8, -21),   S( 15, -11),   S( 19, -29),   S( 24, -30),
            S( 35, -11),   S( 12, -14),   S( 38, -12),   S( 18, -33),   S( 23,  -7),   S( 18,   3),   S( 25, -30),   S(  4, -40),

            /* rooks: bucket 11 */
            S(-64, -33),   S(-48, -33),   S(-41, -32),   S(-37, -78),   S(-44,  -8),   S(-14,  -2),   S(-28, -20),   S(-62,  11),
            S(-28, -23),   S(-16, -52),   S(-32, -35),   S(-35, -16),   S(-29, -11),   S(-18, -10),   S(-21, -10),   S(-18,   4),
            S(-10, -20),   S(  8, -34),   S( -5,   2),   S( 12, -28),   S(  0, -25),   S(-33,   3),   S(-47, -29),   S(-21, -64),
            S(-22,  18),   S(  2, -18),   S(-11,  -4),   S( -8,   5),   S( -4,  -8),   S(-17,  33),   S( 19,   0),   S( -4, -31),
            S( -7,  -8),   S(  2, -33),   S( 18,  10),   S( 30, -13),   S( 12, -21),   S(  6,  -5),   S( 15, -14),   S(-16, -26),
            S(  3,  28),   S( 24,  11),   S( 19, -13),   S( 67,   6),   S( 28,  11),   S( 23,   0),   S(-27,   3),   S( 18,  13),
            S( 58,  27),   S( 46,   6),   S( 36, -22),   S( 59,   8),   S( 23,  19),   S( 25,  27),   S( 22,  17),   S( 40,   2),
            S( 36,  31),   S(  5,   6),   S( 26,   2),   S( 11, -23),   S(  1, -18),   S( 19,   5),   S( 14,  -9),   S( 20, -12),

            /* rooks: bucket 12 */
            S(  8, -68),   S(  0, -45),   S(-28, -18),   S(-26, -44),   S( -6, -17),   S( -8, -45),   S(-22, -63),   S( -9,  -8),
            S( 11,  18),   S( 11,  29),   S(-14,   9),   S( -7,  -6),   S( -5,  -6),   S( -8, -12),   S(-13, -46),   S(  4, -17),
            S( 18,  25),   S(-12, -15),   S(-16, -23),   S(-21, -20),   S( -1, -15),   S(  0, -13),   S(  1,  -2),   S( 10,  14),
            S(-31, -41),   S( -7, -27),   S(-12, -17),   S(  8,   1),   S(-13, -38),   S(-10, -26),   S( -5, -15),   S( -8, -36),
            S(-18, -24),   S(-15,  -7),   S( 17,  17),   S(  5,  15),   S( -8, -14),   S( -3, -31),   S(  1,  -1),   S( -4, -16),
            S( -4,  -4),   S(-10, -16),   S( 16, -13),   S(  2, -36),   S( -8, -36),   S( -4, -24),   S(  1, -23),   S(  1,  -2),
            S( -6, -16),   S( -4, -11),   S(  1, -36),   S(  2, -30),   S(  7, -30),   S( -5, -54),   S(-16, -53),   S(  2,   3),
            S( -3, -37),   S(  6,   2),   S( -1, -26),   S(  1, -26),   S(  3,  -3),   S( -3, -16),   S(-14, -58),   S( 10,  13),

            /* rooks: bucket 13 */
            S(  3, -15),   S(  3,   0),   S( -7, -26),   S( -1,   3),   S( -6,  -9),   S(-25, -64),   S(  2, -21),   S(-18, -35),
            S(  6,  -2),   S( -5, -15),   S( -9,   6),   S( -4,  16),   S(-23, -38),   S( -1,   7),   S( -4, -11),   S( -1, -12),
            S( -8, -36),   S( -5, -12),   S(  0,  -6),   S(  3,  -9),   S(  2,  12),   S(  7, -19),   S( -8, -27),   S( -9, -47),
            S( -9, -22),   S( -5,  -7),   S(-19, -29),   S( -8, -12),   S( 10,   5),   S(-18, -56),   S( -9, -40),   S( -4, -20),
            S(  7,   7),   S(  7,   9),   S(  7,  16),   S( -9, -18),   S(-18, -34),   S( -5, -35),   S( -5, -43),   S( -2,   0),
            S(-23, -40),   S( -1, -13),   S(-22, -53),   S(  1, -29),   S( 11,  -4),   S( 20,  42),   S(  2, -18),   S( -2,  -6),
            S( -7, -10),   S(  7,  -1),   S(  5,  -8),   S(  1,   0),   S( 12,  -2),   S( 10,  18),   S(  7, -10),   S( -3, -15),
            S(-28, -123),  S(-19, -74),   S(  3,   4),   S( -6, -13),   S(  8,  19),   S(-15, -58),   S( -9, -44),   S(  2,  -2),

            /* rooks: bucket 14 */
            S(-15, -65),   S(-15, -13),   S( -4, -10),   S( -7, -14),   S(-19, -20),   S(-13, -33),   S(  1, -24),   S(-24, -32),
            S(-15, -39),   S(-27, -88),   S( -6,   3),   S(-22, -47),   S( -7,  -9),   S(  3, -10),   S(  9,  18),   S(  3, -20),
            S(-14, -25),   S( -3, -16),   S( -7, -17),   S(  2,  -9),   S(-10, -15),   S( -9, -15),   S( 15,  45),   S(  4,  10),
            S(  5,  29),   S(-10, -29),   S( -4,  -8),   S( -7, -18),   S(  1, -20),   S( -9, -15),   S(-17, -52),   S( -3,  11),
            S( 13,  -3),   S( -7, -53),   S(  6, -37),   S( -5, -52),   S( -3, -40),   S(  0, -49),   S( -5, -52),   S(  4,  11),
            S( -6, -43),   S( -7, -30),   S(  1, -48),   S(  3, -73),   S( -2, -86),   S( -7, -69),   S(  9, -32),   S( -4, -42),
            S( 13,  -5),   S(  0, -57),   S(  6, -35),   S(  2, -61),   S( -2, -60),   S( 15,  -6),   S( 16,   6),   S(  5,  -1),
            S(-11, -34),   S( -7, -25),   S(-14, -39),   S(  3,  -9),   S(-12, -22),   S( -9,  -9),   S(  5,  -1),   S( -8, -11),

            /* rooks: bucket 15 */
            S( -8, -31),   S(-25, -72),   S( -5, -31),   S(-15, -60),   S( -3, -28),   S(-10,   4),   S( -8,  -8),   S( -8,  -4),
            S(-12,  -2),   S(-18, -28),   S( -6, -15),   S(-20, -47),   S(-10, -36),   S(  1,   4),   S( -9, -28),   S( 11,  16),
            S( -9, -21),   S(-13, -38),   S(-10, -47),   S(  3, -18),   S(  6, -17),   S(-11, -34),   S( -4,  14),   S(  0, -10),
            S( -5, -38),   S(  4,  -5),   S( -8, -13),   S(  0, -18),   S(-15, -55),   S( -8, -52),   S( -2, -29),   S( -7,   7),
            S(  0,  -6),   S( -8, -47),   S( 15,   1),   S( -5, -44),   S( -2, -23),   S(  4, -39),   S(  3, -16),   S( -2,   3),
            S(  0, -22),   S( -3,  -6),   S( -1, -42),   S(  8, -41),   S( -4, -42),   S(  3, -25),   S(  9, -21),   S(  0,   7),
            S( 11,  18),   S(  5, -34),   S( 16, -27),   S( -6, -63),   S( -3, -44),   S( 10,  10),   S(  7,  15),   S(  0,  -3),
            S(  2,   3),   S(-11, -42),   S( -3, -33),   S( -2, -46),   S( -1, -35),   S( 10,  14),   S( -4, -25),   S( -8, -44),

            /* queens: bucket 0 */
            S(-22, -17),   S(-15, -67),   S( 28, -97),   S( 55, -54),   S( 30, -40),   S( 33, -18),   S( 63,   3),   S( 35,   4),
            S(-22, -14),   S( 25, -68),   S( 40, -35),   S( 39, -14),   S( 32,   8),   S( 17,  12),   S( 30,   4),   S( 37,  31),
            S( 17,   8),   S( 51, -43),   S( 18,   5),   S( 21,  33),   S( 21,  11),   S(  8,  38),   S(  8,   5),   S( 24,  35),
            S( 10,  34),   S( 17,  26),   S( -4,  43),   S( 17,  36),   S( 37,  22),   S( -4,  53),   S(  2,  32),   S(  1,  38),
            S( 33,  43),   S(  7,  45),   S( 24,  11),   S( 44,  38),   S( -3,  28),   S( -3,   7),   S( 27,  24),   S( 26,   4),
            S( -2,  84),   S( 12,  62),   S(  2,  46),   S( 48,   6),   S( 43,  15),   S( 30,  17),   S( 20,  33),   S( 24,  -5),
            S( 59,  29),   S( 20,  25),   S(  9,  47),   S( 31,  41),   S(  9,  35),   S(-15,  -8),   S( 30,  21),   S( 12,  12),
            S( 27,  28),   S( 34,  24),   S( 37,  39),   S( 51,  53),   S( 38,  40),   S(-45,  -6),   S( 68,  20),   S( 50,  33),

            /* queens: bucket 1 */
            S( 14, -25),   S(-60, -21),   S(-68,  -8),   S(  6, -116),  S(  0, -24),   S(-14, -40),   S( 44, -63),   S( 25,  42),
            S(-12, -64),   S(-15, -61),   S( 10, -28),   S(  4,  33),   S(  3,  15),   S( 15,   3),   S( 28, -37),   S(  9,   1),
            S(-24,  19),   S(  3,  -9),   S(  7,  18),   S( 15,  -9),   S(  4,  35),   S( -9,  19),   S(  7,  -2),   S( 17,  21),
            S( 12, -10),   S(-27,  40),   S(  3,  45),   S( 20,  46),   S(  2,  79),   S(  3,  15),   S( 11,  -2),   S( 14,  18),
            S( 18,  -6),   S(-16,  30),   S(-40,  72),   S(-43, 103),   S(-12,  60),   S( 24,  16),   S(-13,  11),   S(  5,  58),
            S(  1,  23),   S( 18,  68),   S( 14,  52),   S(  1,  60),   S( -4,  64),   S(  9,  54),   S( 45,  22),   S( 14,  40),
            S(-11,  46),   S( -4, 107),   S(-21,  19),   S( -6,  93),   S(-55,  71),   S(-21,  28),   S( -7,  49),   S( -8,  49),
            S(-30,  14),   S(  3,  33),   S( 22,  48),   S(-44,  10),   S( -7,  39),   S(-15,  10),   S( 26,  18),   S( 16,  27),

            /* queens: bucket 2 */
            S( 16, -16),   S( 33, -61),   S( 11, -37),   S(  6, -28),   S(-25,   0),   S(-25, -31),   S(-28, -44),   S(  0,  11),
            S( 15,  -1),   S( 21,  10),   S( 23, -12),   S( 30, -28),   S( 24, -23),   S( 19, -43),   S( 25, -16),   S( 42, -43),
            S( 11,  15),   S( 19,  -4),   S(  3,  41),   S( 14,  31),   S( 11,  67),   S( 17,  50),   S( 13,  -1),   S( 30,   3),
            S( 12,  -1),   S(-21,  57),   S(  8,  38),   S( 17,  41),   S( -6,  93),   S(  2,  87),   S(  9,  28),   S( 13,  45),
            S( 12,  32),   S(-11,  54),   S(-26,  79),   S(-40, 137),   S(-37, 114),   S(-23, 124),   S(-36, 119),   S( -9, 123),
            S( -2,  66),   S(  8,  67),   S(-22,  99),   S(  8,  51),   S(-18, 120),   S(-15,  88),   S(-24, 112),   S(  9,  58),
            S(-16,  61),   S(-33, 100),   S(-45,  83),   S( 13,  47),   S(-24, 101),   S( 23,  53),   S(-51,  61),   S(-42,  96),
            S(-77,  88),   S(-32,  62),   S( 40,  41),   S( 47,  36),   S( 35,  59),   S( 43,  43),   S( 39,  45),   S(-30,  45),

            /* queens: bucket 3 */
            S( 63, 102),   S( 53,  87),   S( 47,  84),   S( 40,  85),   S( 66,  15),   S( 39,  16),   S( 10,  31),   S( 36,  49),
            S( 57, 108),   S( 51, 109),   S( 38, 116),   S( 49,  77),   S( 46,  76),   S( 59,  50),   S( 60,  13),   S( 21,  35),
            S( 53,  87),   S( 39, 109),   S( 46,  77),   S( 45,  75),   S( 51,  82),   S( 45,  90),   S( 47,  92),   S( 39,  53),
            S( 32, 128),   S( 39,  82),   S( 43,  97),   S( 35, 100),   S( 40, 100),   S( 24, 140),   S( 36, 100),   S( 21, 136),
            S( 51,  94),   S( 39, 113),   S( 16, 119),   S( 31, 115),   S( 16, 140),   S( 10, 129),   S(  8, 169),   S( 28, 151),
            S( 45, 119),   S( 40, 117),   S( 53,  97),   S( 17, 125),   S( 17, 146),   S( 62, 124),   S( 50, 153),   S(  6, 203),
            S( 40, 126),   S( 55, 106),   S( 60,  91),   S( 49, 108),   S( 36, 131),   S( 54, 117),   S( 49, 134),   S(127,  78),
            S( 65,  95),   S( 63, 118),   S( 50, 113),   S( 65, 108),   S( 31, 104),   S( 74,  91),   S(120,  57),   S(121,  52),

            /* queens: bucket 4 */
            S(-11, -21),   S(-18, -33),   S(-48, -10),   S( -5, -34),   S( 24, -15),   S( 31,  18),   S(-48, -36),   S(-30,  -4),
            S(-24, -29),   S(-17, -14),   S( 38,   5),   S(-36,  18),   S(-16, -36),   S( -7, -16),   S( -4, -24),   S(-70, -24),
            S(-14, -17),   S( 46,  29),   S(  3,  42),   S( 11,  35),   S(-13, -29),   S( -3, -17),   S(  9,   0),   S( -9,  -6),
            S(-14, -21),   S( 17,  31),   S(-14,  45),   S(-20,  22),   S( 21,  37),   S( 16,  24),   S(-10, -33),   S(  1,  -4),
            S( 17,  27),   S( 37,  28),   S(  5,  33),   S( 38,  52),   S( 11,  14),   S( 16, -15),   S(-11,  -6),   S( 15,   1),
            S( 11,  10),   S( 20,  -8),   S( 19,  31),   S( 39,  42),   S( 24, -13),   S( 17,  13),   S( -9, -14),   S(-17,  -7),
            S(-23, -32),   S(-22,  18),   S(  7,  35),   S( 25,  42),   S( 24,  17),   S( -7,  -8),   S( -9, -31),   S(-22, -19),
            S( -4, -28),   S(-26,  -2),   S( 28,  21),   S( 12,  20),   S( -8,  16),   S(  8,  18),   S(-13, -31),   S(-25, -47),

            /* queens: bucket 5 */
            S(-31, -23),   S(-36, -19),   S(-24, -15),   S(-29,  -9),   S(-28, -22),   S( 29,  -2),   S(-18,  -9),   S( -6, -11),
            S(-47, -30),   S(-38, -34),   S(-41, -30),   S(-81,  -3),   S(  6, -13),   S(-34, -11),   S(-43,  -8),   S(-39, -24),
            S(-20,   8),   S(-41,  -7),   S(-69, -22),   S(-45,  18),   S( 63,  53),   S(  1,  45),   S( 13,   2),   S(  8,  28),
            S(-68, -11),   S(-65, -50),   S(-12,  29),   S( 18,  69),   S( 19,  25),   S( -2,  28),   S(  9, -24),   S( -8,  28),
            S(-58, -30),   S(-28,  29),   S( 32,  76),   S( 11,  77),   S( 40,  64),   S(  2,  25),   S(  4,   8),   S(-27, -11),
            S(-28,  -1),   S( 28,  78),   S(-28,  65),   S( 39,  44),   S( 40,  62),   S( 15,  28),   S(  0,  -2),   S(  9, -11),
            S(-25,   3),   S( -3,  17),   S(  9,  67),   S(  5,  58),   S( 13,  40),   S( 19,  60),   S( 16,  16),   S(-22, -24),
            S(  8,   5),   S( 23,  34),   S(-16,  22),   S(  8,  56),   S( 30,  60),   S(  4,  17),   S(  3, -10),   S(-11,  -9),

            /* queens: bucket 6 */
            S(-39, -38),   S(-57, -17),   S(-39, -31),   S(-59, -70),   S(-80, -31),   S(-60, -49),   S(-80, -61),   S(-25,   5),
            S(-49,   8),   S(-22,  15),   S(-10, -10),   S(-54,  27),   S(-76,  48),   S(-88,  13),   S(-74,  -6),   S(  3,  16),
            S(-41,  -7),   S( -9,  22),   S(-28,  39),   S(-65,  80),   S(  3,  65),   S(-20,  -6),   S(-22,  13),   S(  0,  29),
            S(  0,  46),   S(  4,  33),   S(-23,  72),   S(-40,  75),   S( 23,  97),   S( 37,  35),   S( -4,  58),   S( 49,  21),
            S(-58,  37),   S( -1,  41),   S( -7,  49),   S( 37,  61),   S( 35,  86),   S( 85,  74),   S( 41,  34),   S( -2,  12),
            S(-37,  52),   S( -1,  48),   S( 45,   7),   S( 17,  78),   S( 14,  78),   S( 52,  91),   S(-25,  31),   S( -1,  18),
            S(  3,  18),   S(-15,  30),   S( 22,  49),   S(  8,  42),   S( 29,  58),   S( 30,  85),   S( -5,  48),   S(-46,   2),
            S( 22,  37),   S( 39,  64),   S( 15,  45),   S( 17,  41),   S( 58,  62),   S( 38,  60),   S(-19,  -6),   S( -5,  -6),

            /* queens: bucket 7 */
            S( 13, -20),   S(-25,   4),   S(-22,   7),   S(-24,   8),   S(-16, -32),   S(-40,  -9),   S(-57,  11),   S(-27, -25),
            S(-28,  12),   S(-65,  21),   S(-30,  15),   S( 19,   6),   S(-20,  45),   S(-36,  35),   S(-51,  56),   S(-15, -47),
            S(-60,   5),   S(-58,  39),   S(-16,  23),   S(-13,  42),   S( 24,  50),   S(-12,  56),   S(-49,  16),   S(-31,  -3),
            S(-71,   9),   S( 20,   8),   S( -4,  42),   S(  2,  48),   S( 45,  44),   S( 46,  35),   S( 19,  63),   S( 20,  31),
            S(-58,  35),   S(-46,  46),   S( -8,  56),   S( 48,  25),   S( 39,  26),   S( 85,  13),   S( 27,  40),   S( 53,   9),
            S( -5,  25),   S(  2,  27),   S( 18,  20),   S( -2,  30),   S( 51,  62),   S( 94,  39),   S( 78,  15),   S( 33,  25),
            S( 21, -12),   S( 24,  47),   S( 14,  17),   S( 54,  20),   S( 39,  33),   S( 37,  55),   S( 58,  22),   S( 55,  36),
            S( 46,  24),   S(  7,   0),   S( 36,  37),   S( 48,  39),   S( 51,  30),   S( 18,  35),   S(  5, -10),   S( 53,  49),

            /* queens: bucket 8 */
            S( -6, -14),   S(  2,  -6),   S( -7,  -3),   S(-12,  -3),   S(-14, -27),   S(-21, -27),   S(-28, -46),   S( -1,   7),
            S( -8,   5),   S( -8,  -6),   S( -3,   6),   S( -4,  12),   S(-16, -32),   S(-34, -39),   S(-29, -61),   S( -2,  -5),
            S(  2,  -2),   S(-29, -21),   S(-15, -26),   S(-19, -29),   S(-18, -17),   S( -8,  -8),   S( -9, -29),   S(-20, -36),
            S(-12, -10),   S(  4,  10),   S(-15, -14),   S( -4,  22),   S( -2,  16),   S( -7,   6),   S( -2,  -4),   S( -8, -23),
            S( 10,  27),   S( 14,  39),   S( 18,  50),   S(  1,   9),   S(  3,  16),   S(  0,   7),   S( -9, -10),   S(-14, -16),
            S( 10,  21),   S( 15,  23),   S(-14,  32),   S( 16,  49),   S(-15, -15),   S(-12, -18),   S(-14, -20),   S(  9,  19),
            S( -5, -18),   S(-30, -24),   S( 19,  36),   S( 11,  15),   S( -8,  -2),   S(  1,  14),   S( -6,  -8),   S( -2, -10),
            S(-22, -46),   S(  2,   9),   S(-18, -40),   S(-17, -30),   S( -3, -19),   S( -7, -11),   S(  5,  -8),   S( -5,  -4),

            /* queens: bucket 9 */
            S( -5, -14),   S(-20, -45),   S( -1,  -6),   S(-42, -49),   S(-29, -51),   S(-22, -34),   S(-13, -33),   S(-17, -37),
            S(-10, -31),   S(-13, -25),   S(-17, -15),   S(-16, -18),   S(-29, -38),   S(-10, -19),   S(  2, -10),   S( -1,  -2),
            S(  8,  11),   S(-16,  -6),   S(  5,  36),   S(-30, -52),   S(-28, -25),   S(-19, -22),   S(-10, -13),   S(-20, -32),
            S(-14, -12),   S(-13, -12),   S(  8,  40),   S( -5,  15),   S( 28,  36),   S( 12,  20),   S(-21, -37),   S( -8, -18),
            S( -3,  -5),   S(-14,  -8),   S( 16,  45),   S( 18,  50),   S(-18,   0),   S(  6,  17),   S(  2,   5),   S(-11, -16),
            S(-16, -22),   S(-23,  -6),   S(  2,  29),   S( 11,  40),   S(-12,  -2),   S( -9, -11),   S(-23, -25),   S(-12, -11),
            S(-15, -18),   S( -1,   2),   S(  2,  35),   S(  9,  20),   S(  7,  11),   S( 15,   8),   S(  1,  -1),   S(-33, -59),
            S( -1, -16),   S(-27, -42),   S(  5,  16),   S( 27,  49),   S(  8,   8),   S( -9, -14),   S(  0,  -3),   S( -4,  -9),

            /* queens: bucket 10 */
            S( -3,  -5),   S( -7, -11),   S(-17, -31),   S(-27, -32),   S(-17, -13),   S(-14,  -8),   S( -7, -18),   S(-10, -23),
            S(-12, -23),   S(-23, -35),   S( -4, -14),   S(-18, -11),   S(-12, -15),   S(-21, -25),   S(-11, -26),   S( -9, -10),
            S(  2,   1),   S(-18, -22),   S(-11, -14),   S( -4,  10),   S(-17,  -3),   S(-18,  26),   S( -2,   6),   S( -7, -18),
            S( -8, -18),   S( -1,   6),   S(  1,  10),   S( -7,   7),   S(  0,  22),   S(-22,   9),   S(-13, -25),   S(-27, -41),
            S(-10, -16),   S(-10, -23),   S( -7,  -5),   S( 13,  49),   S(  7,  24),   S( -3,   1),   S( 11,  17),   S( -2,   3),
            S( -3,   0),   S(-32, -40),   S(-21,  -7),   S( -8,  35),   S(  2,  36),   S(  1,  16),   S( 12,   6),   S(-22, -27),
            S( -7, -16),   S(-27, -29),   S(  9,  25),   S(-27, -29),   S( -8,  -8),   S( -2,   4),   S(-13, -26),   S(-14, -36),
            S( -5,  -4),   S( -8, -16),   S(-19, -26),   S( -4,  -5),   S( -5,  -4),   S( -5,  -7),   S(  6,  12),   S( -2, -25),

            /* queens: bucket 11 */
            S(-19, -38),   S(-25, -43),   S(-25, -33),   S(-12, -30),   S(-19, -20),   S(-22, -17),   S( -5,  -2),   S(-16, -35),
            S(-12, -31),   S(-13, -22),   S(-35, -38),   S(-11, -26),   S(-10,  -6),   S(-14, -19),   S( -7,  -3),   S(-15,  -6),
            S(-27, -42),   S(-28, -44),   S(  7,  -8),   S(-11, -13),   S(-16,  -1),   S( 17,  15),   S(  7,  23),   S(-19, -18),
            S(-14, -33),   S(-26, -44),   S( -2,  -4),   S( 17,  43),   S(-18, -16),   S(-25, -17),   S( 28,  34),   S( -1, -10),
            S( -5,   1),   S( -3, -26),   S(-15, -18),   S( 26,  23),   S( 17,  30),   S( 35,  44),   S( 27,  56),   S(-18, -15),
            S( -5, -18),   S( -3,   3),   S( -6,   4),   S(  3,  15),   S( 14,  33),   S( 49,  64),   S( 25,  26),   S( -8, -10),
            S( -7, -10),   S(-30, -37),   S(  6,   3),   S( -2,   3),   S(-10,   0),   S( 21,  33),   S( 21,  23),   S(  3,  -9),
            S(-13, -20),   S(-33, -69),   S(  2,  -9),   S( 14,  17),   S( -6, -10),   S(  5, -17),   S( 30,  25),   S(-18, -43),

            /* queens: bucket 12 */
            S(  1,  -2),   S( -3,  -5),   S( -1,   5),   S( -4,  -4),   S( -8, -14),   S( -2,  -3),   S( -3,  -5),   S( -2,  -4),
            S( -8, -14),   S( -4, -10),   S( -5,  -1),   S( -7, -14),   S( -3,  -2),   S(-13, -25),   S( -8, -19),   S(-12, -26),
            S( -3,  -8),   S( -8, -15),   S(  7,   6),   S(  0, -16),   S(-10, -16),   S(-14, -31),   S(-14, -33),   S( -2,   0),
            S(  6,  13),   S(  5,   8),   S(  9,  28),   S(  2,  12),   S( -4,  -5),   S(  0,   5),   S( -4,  -2),   S(  7,   0),
            S( -1,  -9),   S( -1,  -1),   S( 34,  63),   S( -2,   7),   S(-12, -15),   S( 13,  31),   S(-16, -29),   S(  4,  10),
            S(  7,  14),   S(  8,   9),   S( 30,  51),   S(  2,   4),   S( -2,   1),   S( 11,  16),   S( 11,  17),   S( -7, -16),
            S(  1,   3),   S( 13,  28),   S( 27,  53),   S(  5,   7),   S(  7,   6),   S(  2,   1),   S(  9,  21),   S(  1,   2),
            S(-15, -36),   S(-24, -52),   S(-26, -22),   S(-17, -38),   S( -5,   1),   S( -2,  -1),   S( -1,  -2),   S(-10, -12),

            /* queens: bucket 13 */
            S(  1,  -2),   S(-15, -39),   S(-11, -27),   S(  1,   0),   S( -3, -10),   S(  4,  -3),   S( -9, -24),   S( -3,  -6),
            S(  1,   4),   S(  8,  21),   S(  6,  13),   S(  1,  -4),   S( -7, -17),   S(  4,  10),   S( -4, -13),   S( -5, -12),
            S(  0,  -1),   S( -9, -18),   S( -4,  -2),   S(-11, -32),   S(-15, -27),   S( -6, -20),   S( -9, -18),   S(-13, -16),
            S( -5,  -9),   S( -9, -29),   S(  8,  11),   S( 10,  16),   S( 21,  35),   S(-15, -38),   S(  2,  -2),   S(-10, -22),
            S( -6,  -5),   S(  6,  14),   S( 23,  29),   S( 19,  43),   S( 26,  58),   S(-13, -25),   S(-11, -33),   S(-14, -30),
            S( -6, -14),   S(  6,  18),   S( 46,  87),   S( 29,  57),   S( -3,  13),   S(-12, -29),   S(  5,  11),   S( -8, -19),
            S( -4,  -7),   S(  4,  14),   S(  6,  26),   S( 14,  41),   S( 18,  27),   S( -1,  -7),   S(  0,  -2),   S( 15,  33),
            S(-33, -69),   S(-19, -33),   S(-16, -26),   S(  0,   9),   S(  4,   6),   S(  3,   5),   S( -8, -16),   S( -9, -16),

            /* queens: bucket 14 */
            S(  1,   3),   S(-15, -34),   S( -5, -12),   S(-10, -22),   S( -1,  -6),   S( -3,  -7),   S(  0,  -5),   S(-13, -37),
            S( -8, -18),   S( 13,  30),   S( -9, -26),   S( -7, -30),   S(-17, -26),   S(-14, -35),   S( -3, -10),   S(-15, -36),
            S(  0,  -4),   S(-11, -25),   S( -9, -10),   S( -1,   4),   S( -7, -22),   S( -6, -24),   S(  5,  11),   S( -6,  -5),
            S(-13, -27),   S( 12,  18),   S( -5,  -7),   S( 26,  61),   S( -2,  -9),   S(  4,  10),   S( 12,  20),   S( -1,  -8),
            S(  4,  14),   S(  1,   5),   S( -9, -12),   S( 18,  20),   S( 15,  42),   S(  8,  15),   S( 11,  18),   S( -8, -27),
            S(  1,   1),   S( -3,  -1),   S( 14,  38),   S(  8,  14),   S(  9,  22),   S( 14,  35),   S(  8,  13),   S( -2,  -8),
            S(  1,   9),   S( -4, -11),   S( 10,  26),   S( 15,  21),   S( 11,   6),   S( 14,  34),   S( 21,  34),   S(  8,  17),
            S(-11, -21),   S( -6, -16),   S(-21, -38),   S( -2,  -7),   S(-11, -30),   S( -2,  -4),   S( -2,   3),   S(-11, -26),

            /* queens: bucket 15 */
            S( -2,  -9),   S( -4, -11),   S( -7, -17),   S(  0,  -5),   S( -9, -15),   S( -7, -16),   S(-10, -27),   S( -6, -15),
            S( -1,   2),   S( -3,  -9),   S(-12, -16),   S( -3,  -5),   S(  5,  11),   S( -7, -11),   S( 11,  16),   S( -4, -10),
            S( -2,  -7),   S( -5, -15),   S( -4, -10),   S( -2,  -5),   S( -4, -12),   S( 15,  25),   S( -8, -14),   S(  2,   4),
            S( -4, -14),   S( -3,  -7),   S(-12, -15),   S(  5,  -1),   S(  1,   1),   S( -6, -15),   S( 14,  21),   S( -2,   1),
            S( -3,   0),   S(  4,   0),   S( -5, -10),   S( -8, -19),   S( 13,  21),   S(  6,  10),   S( -5, -13),   S(  0,  -3),
            S(  0,  -5),   S( -4,  -9),   S(  8,  16),   S( -7, -11),   S( -4, -20),   S( 12,  24),   S(  1,  10),   S( -3, -13),
            S(-12, -17),   S( -3, -20),   S( 13,  27),   S( 15,  28),   S(  3,   9),   S( 18,  35),   S( -7, -24),   S(  7,  12),
            S( -8, -17),   S( -5,  -8),   S( -1,  -4),   S( 14,  27),   S(  8,   7),   S(  0, -15),   S( -2,  -3),   S(-11, -15),

            /* kings: bucket 0 */
            S( 11,  58),   S(  8,  72),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 12,  61),   S( 86,  69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-19,  24),   S(-69,  30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 17,  39),   S(  2,  41),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-46,  40),   S(-42,  30),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  46),   S( 23,  40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 21,  57),   S( -1,  43),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 54,  84),   S( 10,  62),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -47),   S( 13, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58,  -8),   S( -3,  22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  8, -49),   S(-34, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 15, -17),   S( -8, -18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-12, -20),   S(-41, -14),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  6,  -5),   S(-13,  -1),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 31,  12),   S(-10, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 30,  38),   S(-37,  15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-85, -44),   S( -9, -10),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-59, -45),   S( 17, -12),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -4, -46),   S(-23, -53),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 83, -63),   S( 23, -45),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-28, -49),   S(-45, -39),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 75, -69),   S( 71, -48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  3, -28),   S(-86, -54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 96, -41),   S( -8, -56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30, -40),   S( 70, -30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -82),   S(-15,  -9),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 41, -36),   S( 53, -38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 23, -61),   S( 23, -66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60, -44),   S( 26, -35),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 14, -63),   S(  1, -62),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 53, -42),   S(-59, -59),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -3, -56),   S(-14, -125),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -12),   S(-36,   7),   S(  4, -10),   S( -6,  -5),   S(  2,   9),   S( 20,  18),   S( 28,  -2),   S( 46,  -5),
            S( -8, -33),   S(-30, -12),   S(-13, -14),   S( -7, -10),   S(  7,   7),   S( -8,  12),   S( 31, -10),   S( 20,  10),
            S(  9, -16),   S(  6, -25),   S( 29, -28),   S(  5, -17),   S( 14,  -4),   S( 13,  27),   S( 12,  31),   S( 49,  12),
            S( 30, -25),   S( 51,  -5),   S( 60, -21),   S( 45,  -3),   S( 17,  48),   S(-16,  91),   S( 20,  74),   S( 68,  53),
            S(100, -59),   S(119,  -3),   S(129, -31),   S( 33,  23),   S( 42, 130),   S( 24,  83),   S( -1, 150),   S( 85,  93),
            S(-151, -105), S(-157, -117), S( 48, -142),  S( 64,  19),   S(116, 148),   S( 58, 190),   S(108,  94),   S( 59, 175),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  16),   S(-55,  12),   S(-33,   8),   S(-49,  48),   S(-32,   7),   S( -4,   8),   S( -1,  -4),   S( -1,  15),
            S(-53,   2),   S(-44,   0),   S(-38,  -6),   S(-24,   6),   S( -8,   4),   S(-28,   6),   S( -6, -10),   S(-21,  10),
            S(-32,  12),   S(-10,  11),   S(-19,  -2),   S(  9, -19),   S( -2,  15),   S( -9,   7),   S(-15,  11),   S(  4,  14),
            S(-24,  31),   S( 28,  10),   S( -3,  18),   S( 25,  23),   S( 10,  20),   S(-17,  26),   S( 22,  15),   S( 45,  36),
            S( 16,  17),   S( 84, -17),   S(105, -29),   S(121, -34),   S( 53,   4),   S(  7,  24),   S(-38,  67),   S( 46,  73),
            S(113, -27),   S( -8, -22),   S( 58, -139),  S( 36, -123),  S(-36, -70),   S( -2,  54),   S( 72, 130),   S( 74, 169),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  34),   S(-46,  17),   S(-36,   4),   S(-25,  11),   S(-52,  42),   S(-38,  20),   S(-24,  -3),   S(-29,  23),
            S(-56,  16),   S(-41,  12),   S(-44,   1),   S(-48,  27),   S(-42,  21),   S(-50,   9),   S(-27,  -8),   S(-56,  12),
            S(-36,  38),   S(-30,  39),   S(-21,  20),   S(-27,  22),   S(-29,  25),   S(-27,   8),   S(-27,   6),   S(-26,  11),
            S(-12,  67),   S(-21,  54),   S(-12,  41),   S(  4,  32),   S( -3,  31),   S(-14,  13),   S( 11,  16),   S( 26,   5),
            S(-34, 117),   S(-39,  91),   S(-20,  35),   S( 36, -31),   S(116, -31),   S(103,   0),   S( 86, -38),   S( 56,  -9),
            S( 32, 194),   S( 76, 108),   S(-11,  68),   S(-16, -73),   S(-26, -160),  S(-60, -157),  S( 26, -61),   S(102, -64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8,  17),   S(-16,  13),   S( -8,  16),   S(-14,  36),   S(-25,  55),   S(  8,  37),   S( -7,  12),   S(-16,  -1),
            S( -1,  10),   S(  1,  12),   S(-16,  12),   S(-19,  23),   S( -2,  24),   S(  0,  11),   S( -5,   1),   S(-40,   4),
            S( 14,  35),   S( -3,  49),   S(  2,  25),   S( -8,  12),   S( 16,   2),   S( 16,   0),   S(  1,  -8),   S(-11,   0),
            S( 22,  78),   S(  0,  87),   S( 10,  69),   S( 17,  28),   S( 31,   5),   S( 39, -22),   S( 27,  13),   S( 41, -13),
            S(  4, 144),   S(-30, 173),   S(-40, 174),   S( -4, 118),   S( 42,  54),   S( 89,  -9),   S( 97, -24),   S( 96, -27),
            S( 87, 139),   S( 46, 215),   S(  1, 245),   S(  3, 187),   S(-12,  85),   S( 27, -151),  S(-98, -186),  S(-166, -168),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 66, -16),   S( 47,  -8),   S(-24,  -7),   S( 14,  -8),   S( 15, -14),   S( -8,  -8),   S(-40,   7),   S(-44,  19),
            S( 59, -25),   S( 41,  -7),   S( 42, -33),   S( 16, -12),   S(-46,  -7),   S(-13, -22),   S(-45, -15),   S(-29, -13),
            S( 94, -27),   S(108, -30),   S( 50, -34),   S(-16, -13),   S(-67,  12),   S(-34,  16),   S(-68,  17),   S(-58,  21),
            S(-40, -86),   S( 24, -111),  S( 40, -25),   S(-53,  23),   S(-36,  17),   S(-26,  46),   S(-22,  46),   S( 24,  28),
            S( 23, -87),   S(-18, -118),  S(  7, -93),   S( 21,  15),   S( 80,  86),   S( 53,  56),   S(  5,  52),   S( 12,  91),
            S(  7, -31),   S( -6, -70),   S(  1, -76),   S( 37,  21),   S( 49,  60),   S( 60, 130),   S( 30, 127),   S( 34, 121),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71,  34),   S(-16,  16),   S( 12,  -3),   S( 44, -12),   S( 61, -10),   S( 29, -12),   S(-23,  -1),   S(-59,  37),
            S(-67,  12),   S( -5,   4),   S( 10, -15),   S( 18,   8),   S(  1,   2),   S( 12, -17),   S(-57,   0),   S(-80,  21),
            S(-18,  14),   S( 41,  10),   S( 49,   7),   S( 38,  13),   S( -9,  26),   S(  0,   3),   S(-17,   5),   S(-45,  31),
            S( 22,  13),   S( 38, -20),   S(-22, -39),   S(-13, -25),   S(-15, -11),   S(-19,  12),   S( 59,   1),   S( 12,  24),
            S( 76, -16),   S( 29, -55),   S( 23, -86),   S( -6, -55),   S( 56, -52),   S( 30,   7),   S( 10,  28),   S( 17,  70),
            S( 64,  30),   S( 40, -27),   S( -8, -74),   S( 16, -72),   S(-14, -70),   S( 51,  31),   S( 71,  97),   S( 81,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-95,  27),   S(-38,  -6),   S(-11, -13),   S(-15,  18),   S( 37,   3),   S( 28,   2),   S( 24,  -6),   S( -1,  18),
            S(-69,   1),   S(-50,  -7),   S(-38,  -9),   S( 44,  -7),   S( -5,  15),   S( 13, -10),   S( 13, -11),   S( -2,  -5),
            S(-54,  27),   S(-45,  17),   S(  1,   3),   S(  9,   9),   S( 35,  19),   S( 58,  -7),   S( 64, -17),   S( 12,   3),
            S(-44,  51),   S( -1,  21),   S( 35,   6),   S( 14,  -6),   S(-22, -36),   S( -9, -42),   S( 47, -25),   S( 94, -32),
            S( 17,  68),   S( 20,  24),   S( 48,   3),   S( 20, -53),   S(-40, -74),   S(-10, -68),   S( 58, -74),   S(115, -28),
            S(117,  41),   S(115,  41),   S( 71,   2),   S( 32, -66),   S(  9, -106),  S( -4, -85),   S(-21, -38),   S( 87, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61,  -4),   S(-59, -15),   S( -9, -27),   S(-59,  47),   S( 27,  -5),   S( 65, -14),   S( 46, -19),   S( 71, -11),
            S(-53, -13),   S(-49, -19),   S(-34, -27),   S(-20,  -2),   S( 16, -13),   S( 44, -28),   S( 37, -20),   S( 50, -20),
            S(-41,  17),   S(-65,  24),   S(-24,  -2),   S(-34,  -7),   S(  3,  -3),   S( 32, -21),   S( 81, -23),   S( 77, -25),
            S( -1,  29),   S(-57,  53),   S(-34,  48),   S( -4,  22),   S( 15, -17),   S( 62, -63),   S( 12, -65),   S( 28, -97),
            S( 52,  28),   S( -4,  99),   S( 34,  83),   S( -7,  66),   S(  2,   3),   S(-10, -57),   S(-49, -110),  S(  5, -90),
            S(123,  72),   S(105, 102),   S(108,  98),   S( 88,  85),   S( 41, -28),   S(  9, -94),   S(-26, -81),   S(  1, -162),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  -7),   S( -3, -16),   S( 66, -14),   S(-23, -44),   S(-30, -37),   S( 37, -47),   S( 70, -76),   S( 35, -46),
            S(-35, -31),   S(-22,  -9),   S(-58, -68),   S(-21, -34),   S(-47, -42),   S( 25, -41),   S(-36, -37),   S(-36, -36),
            S(-18, -67),   S( 15, -54),   S( 11, -63),   S(-44, -34),   S(-20,  -6),   S(-14, -26),   S(-42, -16),   S(-44,   2),
            S(  8, -13),   S(-37, -30),   S( 24, -16),   S( 13,  -8),   S(  3,   1),   S(  3,  32),   S(-25,  32),   S(-12,  29),
            S( 10,  22),   S( 11, -32),   S( 26,  43),   S( 48,  82),   S( 69, 107),   S( 54, 105),   S( 21,  74),   S(-12,  92),
            S(  7,  32),   S(  7,  27),   S( 28,  80),   S( 49,  92),   S( 42, 101),   S( 59, 166),   S( 39,  94),   S( -1,  59),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 58, -16),   S( 86, -17),   S( 42,  -4),   S( -4, -26),   S( 11, -17),   S( 46, -33),   S( 87, -68),   S(  9, -33),
            S(  1, -48),   S(-34, -50),   S( -7, -50),   S(-30, -25),   S(-17, -42),   S(-17, -39),   S(  4, -46),   S(  5, -43),
            S(-89, -19),   S(-51, -47),   S(-20, -71),   S(-42, -52),   S(  0, -37),   S(-27, -39),   S(-24, -39),   S(-38, -12),
            S(-48, -12),   S(-48, -49),   S( -1, -64),   S( 16, -37),   S(  4, -29),   S( 26, -26),   S(-21,  -2),   S(  8,  -6),
            S(-23,   7),   S( 15, -38),   S( 18, -18),   S( 24,   2),   S( 21,  41),   S( 15,  35),   S( -2,  41),   S(  3,  38),
            S(-10,  52),   S( 26,  57),   S(  5,  -5),   S( 24,  26),   S( 32,  69),   S( 11,  59),   S( 16,  67),   S( 11,  70),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26, -52),   S(-26, -45),   S( 11, -20),   S(  1, -15),   S( 33, -20),   S(156, -35),   S( 91, -28),   S( 47, -33),
            S(-74, -49),   S(-66, -51),   S( 29, -71),   S( 16, -40),   S(-10, -36),   S( 21, -34),   S( 20, -47),   S( 11, -42),
            S(-59, -25),   S(-54, -40),   S(-19, -45),   S( 42, -48),   S(-34, -46),   S(-27, -57),   S(  0, -75),   S( 51, -57),
            S(-11, -18),   S( 18, -23),   S(-12, -21),   S(-43, -45),   S(-21, -43),   S(-61, -35),   S( 17, -51),   S(  6, -33),
            S( -9,   7),   S( 26,   9),   S( 36,  12),   S( -3, -24),   S( 32,  -9),   S( 12,   8),   S(-17, -32),   S( -9,  -4),
            S(-13,  20),   S(  2,  42),   S( 33,  38),   S( 21,  46),   S( 26,  46),   S(-10,   5),   S(  3,  -3),   S( 13,  16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57, -36),   S(-18, -38),   S(  9, -42),   S(  9, -13),   S( -6, -35),   S( 92,  -6),   S(140, -50),   S( 93, -25),
            S(-26, -55),   S(-22, -63),   S(-42, -55),   S(-27, -38),   S( -4, -31),   S(-40, -34),   S( 23, -33),   S( 21, -67),
            S(-43, -23),   S(-35, -30),   S(-22, -16),   S( 46, -50),   S(-37, -38),   S( 11, -32),   S(-25, -84),   S(-16, -58),
            S( 16,   8),   S(-31,  25),   S( 44,  25),   S( 58,   2),   S( -4, -14),   S(-26, -33),   S(-43, -58),   S(  1, -40),
            S( -3,  15),   S(  4,  53),   S(  9,  91),   S( 52,  46),   S( 37,  64),   S( -7, -18),   S( 28,  -3),   S(  3,   7),
            S( 10,  66),   S( 40,  74),   S( 26,  64),   S( 24,  57),   S( 30,  72),   S( 46,  99),   S( 10,  35),   S( 43,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -72),   S( -5, -11),   S(-29, -40),   S(  5,  23),   S( -1, -17),   S(-26, -43),   S( 38, -27),   S( 22, -14),
            S(-25, -47),   S(-20,  -9),   S(-18, -61),   S(-36, -44),   S(-48, -62),   S(  0, -32),   S(-34, -43),   S( -1, -55),
            S(-38,  -8),   S( -1, -46),   S(-19, -94),   S(-23, -89),   S( -2, -17),   S(-25,   2),   S(-51,  -8),   S(-36, -33),
            S(-16,  12),   S( 13, -14),   S(-21, -35),   S( -9,   8),   S(  6,  49),   S( -9,  65),   S(  2,  77),   S(-29,  33),
            S(  7,  43),   S(  4,  -7),   S(  0,   7),   S( 20,  71),   S( 25,  82),   S( 27, 104),   S( -2, 104),   S( -2, 124),
            S(  8,   7),   S(  5,  16),   S( 21,  67),   S( 16,  56),   S( 28,  97),   S( 24,  93),   S(-23,  25),   S(-16,  34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -51),   S(-27, -52),   S(  0, -11),   S( -9, -26),   S(  0, -22),   S(-12,  -9),   S(  7, -65),   S(-12, -42),
            S(-71, -64),   S(-37, -68),   S(-47, -74),   S(  5, -14),   S(-37, -58),   S(-30, -31),   S(-21, -53),   S(-11, -53),
            S(-42, -43),   S(-26, -61),   S(-10, -56),   S( -4, -25),   S(-16, -41),   S( -9, -48),   S(-47, -42),   S(-43, -14),
            S(-20,  22),   S( -8, -29),   S( -3, -41),   S( -7,  13),   S(  2,  34),   S(-36,  21),   S(-10,   4),   S(-29,  15),
            S( -6,  15),   S( 12,  45),   S( -2,  -4),   S( 30,  42),   S( 22,  79),   S( 30, 115),   S( 21, 118),   S(-17,  80),
            S(  6, 109),   S( 32,  74),   S( -2,  -4),   S(  4,  12),   S( 16,  78),   S( 18,  61),   S(  2,  55),   S( -4,  82),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -93),   S(  4, -74),   S( -4, -25),   S(  0,  10),   S(-11, -54),   S( -5,  -7),   S( 17, -40),   S( -7, -41),
            S(-46, -68),   S(-92, -83),   S(-28, -63),   S(-28, -82),   S(-36, -57),   S(  1, -30),   S(-27, -28),   S(  8, -49),
            S(-39, -56),   S(-31, -66),   S(-16, -53),   S( -9, -30),   S(  1, -51),   S(-24, -72),   S(-16, -52),   S(-38, -18),
            S(-23,   1),   S(-17, -50),   S(-10,  14),   S( -9,  10),   S(  8, -36),   S(  6, -25),   S( -1, -18),   S(-14,  29),
            S(  1, -25),   S( 17,  30),   S(  6,   7),   S( -7,   8),   S( 31,  78),   S( -1,  10),   S( 26,  74),   S(  5,  58),
            S( -8,  37),   S( 15,  52),   S( 27,  98),   S( 26,  84),   S( 24,  73),   S( -3,  42),   S( 24,  90),   S( 24, 102),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -39),   S( -5, -59),   S(-19, -32),   S(  5,   1),   S(-12, -25),   S(-30, -26),   S(-18, -11),   S( -5, -59),
            S(-27, -44),   S(-43, -59),   S(-17, -76),   S(-15, -47),   S( -8, -34),   S(-49, -31),   S(-25,   1),   S(-27, -57),
            S(-36,  -9),   S(-40, -38),   S(-41, -39),   S( 38, -41),   S(-40, -36),   S( 11, -38),   S(  3, -27),   S(-16,   4),
            S(-26,   3),   S(-10,   8),   S( 12,  31),   S( -7,  17),   S( 18,  35),   S( 18,  49),   S(  4,  13),   S(  0,  15),
            S( 18,  94),   S(  9,  81),   S( 13,  66),   S( 22,  47),   S( 26, 118),   S( 11,  71),   S( 13,  48),   S( 15,  50),
            S(-28,  23),   S(-10,  35),   S( 13,  94),   S( 25,  80),   S( 29, 104),   S( 17,  56),   S( 17,  63),   S( 21,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-77, -67),   S(-34, -18),   S(-16,  -9),   S(  9,  54),   S( -8, -21),   S(-24,  18),   S( -1, -28),   S(-97, -31),
            S( 17,   8),   S( -4,  -2),   S(-23, -16),   S( -3,   4),   S( -8,  14),   S(-19,   3),   S(-26, -35),   S(-24,  11),
            S(-11, -10),   S( 22, -25),   S( -7,  28),   S( 43,  19),   S(-15,  33),   S( -5,  21),   S(-30,  33),   S(-33, -26),
            S(  2,  44),   S( 20,  66),   S( 10,  50),   S( 37,  30),   S(  7,  61),   S(  6,  44),   S( 48,  -6),   S(-14,  -7),
            S( 10,  72),   S(-24,  83),   S( 21, 102),   S( 44,  57),   S( 83,  37),   S( 12,  42),   S( 24,   1),   S( -3,  24),
            S(115, -29),   S(-50,  75),   S(134,  42),   S( 87,  18),   S( 49,  52),   S(-19,  84),   S( 47,  -6),   S(-11,   5),
            S( 22,   2),   S(-30, -46),   S( 17,   4),   S( 70,  93),   S( 38,  34),   S( 47,  12),   S(-30,  14),   S(-27,  23),
            S(-99, -150),  S( -1,   2),   S( 27,  27),   S( 35,  61),   S( 25,  56),   S( 42,  33),   S(-22,  12),   S( 15,  30),

            /* knights: bucket 1 */
            S( 42,  21),   S(-59,  28),   S(-42,  25),   S(-41,  39),   S(-30,  45),   S(-16, -35),   S(-32,  -7),   S(  9,  -5),
            S(-60,  20),   S(-53,  52),   S(-31,  41),   S(-15,  28),   S(-21,  24),   S(-12,  27),   S(-16,  29),   S(-26, -83),
            S(-48,  40),   S( -5,  12),   S(-17,  21),   S(-26,  67),   S(-20,  55),   S(-19,  23),   S(-47,  29),   S(-19,  31),
            S(-20,  85),   S( 18,  48),   S( -9,  56),   S(-20,  64),   S(-12,  65),   S(-17,  76),   S(-13,  23),   S(-30,  53),
            S( 37,  17),   S( -7,  21),   S(  3,  83),   S( 11,  68),   S( 12,  55),   S( 19,  46),   S(-19,  39),   S(-22,  65),
            S(-14,  61),   S( 64, -12),   S( 79,  38),   S(101,  63),   S( 85,  54),   S( -7,  78),   S( 16,  64),   S( 13,  38),
            S( 32,  -1),   S( 52, -11),   S( 37, -16),   S( 25,  37),   S( 19,  40),   S( 55,   3),   S( 17,  70),   S(-15,  24),
            S(-189, -21),  S( 15,   6),   S(-34, -82),   S( 10,  24),   S( 12,  15),   S( 36,  19),   S( 19,  53),   S(-88,   8),

            /* knights: bucket 2 */
            S(-80,  24),   S(-41,  32),   S(-39,  20),   S(-22,  30),   S(-29,  35),   S(-59,  25),   S(-36,  12),   S(-27, -21),
            S(-37,  33),   S( -3,  33),   S(-35,  39),   S(-19,  40),   S(-31,  44),   S(-24,  23),   S( -9,  29),   S(-33,  24),
            S(-51,  69),   S(-32,  44),   S(-30,  38),   S(-32,  77),   S(-35,  71),   S(-35,  39),   S(-35,  37),   S(-12,  13),
            S(-17,  78),   S(-25,  62),   S(-38, 102),   S(-41,  99),   S(-36,  86),   S(-16,  64),   S(  4,  45),   S(-13,  39),
            S(-37,  87),   S(-22,  78),   S(-16,  83),   S(  6,  78),   S(-43,  90),   S(  0,  83),   S(-47,  84),   S( -3,  29),
            S(-47,  81),   S(-10,  73),   S(-46, 109),   S( 39,  37),   S( 25,  73),   S(135,  10),   S( 85,  30),   S(-11,   2),
            S( 24,  56),   S(-43,  66),   S( 48,  13),   S( 19,  -3),   S(-25,  51),   S( -6,  14),   S( 48,  39),   S(  0,  23),
            S(-55,  41),   S(  6,  84),   S( -5,  69),   S(-40,  10),   S(-40,  18),   S(-21, -34),   S(  4, -31),   S(-140, -39),

            /* knights: bucket 3 */
            S(-84,  34),   S(-26, -14),   S(-15,  18),   S(-13,  22),   S(-10,  16),   S(-23,  31),   S(-30,   2),   S(-31, -13),
            S(-24,  -2),   S(-14,  39),   S( -4,  33),   S(-12,  37),   S(-14,  42),   S( 10,  24),   S( 13,   5),   S(  8, -17),
            S(-27,  23),   S(-23,  41),   S(-10,  53),   S(-13,  85),   S( -6,  81),   S(-18,  62),   S( -6,  55),   S(  5,   9),
            S( -6,  41),   S( -3,  74),   S( -2,  88),   S(-16, 103),   S(  3, 109),   S(  5, 103),   S( 16,  82),   S(-10,  75),
            S(-15,  72),   S( -3,  85),   S(  2,  99),   S( 23, 122),   S( -5,  97),   S(  3, 121),   S(-43, 127),   S( 29, 118),
            S(-32,  90),   S( 15,  72),   S( 18,  93),   S( 10, 105),   S( 64,  91),   S( 96, 121),   S( 41, 117),   S(-22, 141),
            S( -9,  68),   S(-20,  97),   S(-51, 102),   S( 43,  69),   S( 23,  92),   S(100,  51),   S( -1,   1),   S( 95,  11),
            S(-168,  75),  S( 10, 104),   S(-36, 112),   S( 58,  72),   S( 70,  75),   S(-44,  90),   S( -4, -33),   S(-56, -105),

            /* knights: bucket 4 */
            S(  7,  19),   S(  0,  -5),   S(-38,   4),   S(-39, -29),   S(-36, -11),   S(-18,   0),   S(-17, -52),   S(-23, -36),
            S( 24,  10),   S(  2, -11),   S(  1,   9),   S(-32,  20),   S( -2,  -2),   S( 29, -51),   S( -2,  14),   S(-40, -17),
            S(  2,   0),   S(  3,  12),   S( 49,  -5),   S( 62,   1),   S( 34,  35),   S( 72, -41),   S(-14, -32),   S(-31, -37),
            S(-40, -46),   S( 25, -13),   S( 77,  -2),   S( 81,   8),   S(-17,  32),   S( 30,  37),   S(-19,  -7),   S( 37, -19),
            S(-27, -63),   S( 13, -25),   S( 87,  28),   S( 27,  50),   S( 89, -14),   S(  2,  33),   S( 52,  -7),   S(-30,  36),
            S(-13, -41),   S( -4, -14),   S( 27, -22),   S( 85,  -7),   S( 11,  29),   S( 19,  23),   S(-10, -15),   S( 30,   9),
            S(-17, -28),   S(-23, -32),   S(  0,  -8),   S( 33,  49),   S( 10,  16),   S( -7,  20),   S(  4,  34),   S(-30, -28),
            S(  1,  -2),   S(-17, -57),   S( -3,  -5),   S(  9,   9),   S(  8,   4),   S(-15,   4),   S(  0,  15),   S(-18, -15),

            /* knights: bucket 5 */
            S( 19,  25),   S( 16,  16),   S(-23,  29),   S( -2,  26),   S( 17,  16),   S( 12,   4),   S(  4,  22),   S( 17,  30),
            S( 16,  14),   S( 69,  17),   S( 33,  -7),   S(-12,  12),   S( 49, -14),   S(-11,  -2),   S(-31,  42),   S(-61,   6),
            S(-40,  14),   S(  3,   6),   S( 13,  17),   S( 32,  19),   S( 46,  15),   S(  8,  10),   S( 23,   7),   S(-21,  20),
            S( 55,   9),   S( 34, -28),   S( 81,  10),   S( 58,  -1),   S( 72,  20),   S( 96,   1),   S(-11,  17),   S( 24,  23),
            S( 61,   8),   S( 43, -13),   S(120, -17),   S(100,   8),   S( 92,  -4),   S( 57,  20),   S( 45, -15),   S( 43,  33),
            S( 13, -25),   S( 45, -15),   S( 11, -30),   S(-16,   9),   S( 25,  -1),   S( 52,   2),   S(-11,  24),   S( 36,  21),
            S(  1, -17),   S(-28, -42),   S( -1, -30),   S(-11, -11),   S( -6, -46),   S( -8,   9),   S( 16,  25),   S( 16,  26),
            S(-13, -24),   S(-24, -26),   S(  9, -12),   S(-18, -30),   S( 22, -11),   S( -4,  13),   S(  4,  36),   S( -6,  23),

            /* knights: bucket 6 */
            S(  5,  10),   S(-25,  16),   S(-18,  -9),   S(-32,  42),   S(-32,  27),   S( 39,  21),   S(-41,  51),   S(-28,   1),
            S( 19, -14),   S(  8,  28),   S(  2, -15),   S( 50,  -4),   S( 22,  17),   S( 14,  19),   S( 26,  36),   S(-30,  32),
            S( -4,  10),   S( 30,  11),   S( 43,  12),   S( 41,  30),   S( 33,  40),   S(-32,  33),   S(  4,  36),   S(  4,  21),
            S( 55,  35),   S( 83,   0),   S( 65,  15),   S( 74,  13),   S( 68,  11),   S(108,  10),   S( 45,  -3),   S( -4,  27),
            S( 16,  36),   S( 78,   2),   S(129,  11),   S(151,   5),   S(123, -13),   S( 67,  23),   S(163, -14),   S( 26,  19),
            S( 18,  24),   S( 53,  10),   S( 57,  22),   S( 22,  29),   S( 42, -20),   S( 42,  -3),   S( 39, -13),   S( 36,  12),
            S( 12,  28),   S( -4,  43),   S( 32,  43),   S( -3,  -9),   S( 50,   3),   S( 16, -19),   S(-18, -23),   S( 27,  26),
            S( 10,  24),   S( 20,  32),   S( 10,  12),   S(  9,  18),   S( 25,  -8),   S( -7, -11),   S(  4,  20),   S(-29, -48),

            /* knights: bucket 7 */
            S( 15, -19),   S( -2, -14),   S( 32, -23),   S(-12,  12),   S( 22, -11),   S(-13,  11),   S( 15, -16),   S(-18,  29),
            S(-20, -47),   S( 26, -22),   S( 12,  -9),   S(  8,  -2),   S( 40,  -7),   S( 47,   4),   S(-12,  19),   S(-69,  38),
            S( 11, -55),   S(-50,  -3),   S( 29,  -9),   S( 53,  12),   S( 72,   4),   S( 46,  16),   S( 68,  -3),   S(  3,  23),
            S(-25,  10),   S( 54,   2),   S( 66,  -3),   S(114,  -4),   S(139, -21),   S( 93,  24),   S( 61,  12),   S( 57,  14),
            S( 40, -18),   S(  8,  16),   S( 10,  27),   S(111,   0),   S(133,   8),   S(166, -20),   S(196,  -2),   S( 22,   9),
            S( -3,  13),   S( 44,   8),   S(  5,  35),   S( 72,  16),   S(126, -10),   S(125,   3),   S( 42, -14),   S( -7, -64),
            S(-17,  23),   S( -4,  12),   S( 24,  13),   S( 34,  40),   S( 57,  35),   S( 61,  52),   S(-17, -42),   S( -5, -17),
            S(-28, -27),   S(-13, -19),   S(-12,  17),   S(-29,  -6),   S( -9,   9),   S( 49,  27),   S(  5, -15),   S(  2,  -5),

            /* knights: bucket 8 */
            S( -3, -21),   S(  7, -20),   S(  3,  24),   S(-27, -67),   S( -5,  -7),   S(-10, -55),   S( 10,  19),   S( -3, -30),
            S(-15, -69),   S(-14, -61),   S( -6, -80),   S( -9, -13),   S(-25, -10),   S( 14,   4),   S(  5, -15),   S( -7, -23),
            S(-12, -74),   S(-11, -29),   S(  6, -45),   S( 14, -20),   S( -6, -62),   S( 15, -32),   S( -7, -59),   S( -1, -17),
            S(-16, -70),   S(-17, -55),   S( 18,   6),   S( 20, -28),   S( 19, -48),   S( 14, -23),   S(-29, -57),   S(-17, -35),
            S( -6, -23),   S( -2, -42),   S(  9,  -4),   S( 28, -44),   S( 18, -16),   S( 16, -17),   S( 13,  -5),   S( -4, -50),
            S(  2,   7),   S( 16, -28),   S( 10,   4),   S(  2, -20),   S(  0,  -9),   S( -6, -26),   S(-10, -28),   S( -4,  -4),
            S(  1,   8),   S(  0, -45),   S(-21, -50),   S(  8,   2),   S(  7,  -4),   S( 13,  -4),   S( -6, -11),   S( -8, -28),
            S(  2,   2),   S(  2,  10),   S( -8, -14),   S(  6,  -2),   S(  0,  10),   S( -4,   0),   S( -3,   1),   S( -1,  -4),

            /* knights: bucket 9 */
            S(-16, -78),   S( -1, -36),   S( -4, -71),   S( -7, -76),   S(-29, -54),   S( -8, -15),   S(  6,   6),   S( -2, -16),
            S( -5, -14),   S(-23, -82),   S(-12, -91),   S( -4, -73),   S(-11, -58),   S(-14, -90),   S(  1, -21),   S(-18, -30),
            S( -7, -40),   S( -9, -34),   S(  5, -43),   S( 19, -67),   S(  6, -24),   S(  9, -31),   S(  1,  -2),   S( -1, -24),
            S(-20, -59),   S(  1, -37),   S(  1, -50),   S(  0, -53),   S( -1, -43),   S(  8,  -5),   S(-16, -84),   S( -1, -43),
            S( 21,  54),   S(-14, -36),   S( 17, -19),   S(  8, -21),   S( -1, -70),   S(  0, -34),   S(  9, -48),   S( -2, -21),
            S(-11, -48),   S(-15, -61),   S( -7, -38),   S( 18, -12),   S( 18,   0),   S(  1, -17),   S( -7,  -8),   S( -4,   4),
            S(-10, -12),   S(  0,  -3),   S( -8,   0),   S(-22, -29),   S(  1, -38),   S(  7,  15),   S( -1,  13),   S( -7, -22),
            S(  0,   0),   S(  4,   7),   S( -2,  10),   S( -5, -21),   S( -5, -24),   S( -3,  10),   S(  5,   3),   S( -2,  -3),

            /* knights: bucket 10 */
            S( -6, -28),   S(-10, -41),   S(-13, -21),   S(  1,   7),   S(-19, -75),   S( 16, -44),   S( -1,  12),   S(  0, -31),
            S( -3, -48),   S( 13, -10),   S( -6, -68),   S(  5, -100),  S( -1, -51),   S(-26, -91),   S(-19, -40),   S(  4,  51),
            S( -4, -25),   S( -9, -46),   S( -3, -28),   S( 20, -62),   S(  5, -60),   S( 14, -56),   S(  4, -35),   S( -1, -36),
            S( -4, -60),   S(-13, -65),   S( 27, -34),   S( 17, -25),   S(  4, -42),   S(  5, -23),   S(-14, -88),   S(  0, -53),
            S(-25, -62),   S(  2, -42),   S( 17, -25),   S(  2, -46),   S(  4, -26),   S( 20, -52),   S(-12, -28),   S(  0, -45),
            S(  0,  10),   S(-13, -28),   S(  4, -24),   S( -5, -23),   S( -3, -35),   S(-10, -33),   S(-12, -29),   S(-12, -43),
            S( 11,   3),   S(-12, -44),   S(  8, -32),   S(  6, -45),   S( -7, -38),   S(-10, -55),   S( -8,  -8),   S( -7,  -5),
            S(  0,  -6),   S( -9, -18),   S(  4,  12),   S( -6, -42),   S(  3, -36),   S(  1, -19),   S( 11,  17),   S(  2,  -1),

            /* knights: bucket 11 */
            S( -5, -46),   S( -7, -22),   S( -6, -32),   S(  6, -11),   S(-36, -74),   S(-19, -45),   S( -9, -34),   S(  8,  43),
            S( -2, -21),   S(-27, -64),   S(-13, -80),   S( 40, -30),   S( 31, -10),   S( 11, -32),   S(  1, -36),   S(-19, -25),
            S(-22, -76),   S(-27, -77),   S(-13, -44),   S( 31, -10),   S(  5, -16),   S( 22,  -9),   S( -1, -44),   S(-18, -49),
            S(-27, -33),   S( 23,  -2),   S( 27, -37),   S( 29,   1),   S( 37, -31),   S( 14, -42),   S( 27,   4),   S( -8, -34),
            S(-13, -14),   S(  4, -70),   S(  8, -11),   S( 19, -12),   S( 52,  14),   S( 15,  -7),   S(  3, -68),   S( -8, -14),
            S( -3, -25),   S( -1, -77),   S( 15,   0),   S( 38,   0),   S( 19,  19),   S(-21, -51),   S( -4, -45),   S( -3,  19),
            S(  5,  23),   S( -1,  29),   S(-10, -22),   S(  7,  -3),   S( 15,  -6),   S( 26, -29),   S(  0, -34),   S( -2,  -5),
            S(  0, -13),   S( -2,  -6),   S( -6, -27),   S(-15, -18),   S( -3,  -4),   S(  0, -13),   S(  9,  33),   S(  1,   5),

            /* knights: bucket 12 */
            S(-16, -61),   S( -7, -53),   S(  4,  -1),   S( -1,  12),   S( -8,  -3),   S( -5, -21),   S( -2,  -5),   S( -3, -11),
            S( -6, -40),   S(  2,   7),   S( -7,   0),   S( -2, -20),   S(  3, -48),   S(  4,  -4),   S(  1,  -5),   S(  2,  -2),
            S( -8, -30),   S( -8, -41),   S(  2,  18),   S( -8, -90),   S( 14,   7),   S(  5,   2),   S( -5, -17),   S( -7, -25),
            S(  6,  23),   S( -3, -61),   S(  1,  -9),   S( 16,  -1),   S( 23, -29),   S(  4,   1),   S(  5,  13),   S(  1,   4),
            S( -4, -12),   S(-11, -46),   S( -5, -19),   S(  0, -30),   S(  9,  27),   S( -3,  -1),   S( -6, -17),   S(-10, -26),
            S( -2,  -5),   S( -6, -24),   S( -5, -26),   S(  7,  -6),   S( -5, -18),   S( -3, -41),   S(  4,  -3),   S( -1,  -6),
            S( -3,  -8),   S( -7, -15),   S( -3, -15),   S( -6, -24),   S( -1,   7),   S(-12, -34),   S( -2, -10),   S( -2,   4),
            S(  0,   0),   S(  3,  12),   S(  0,   3),   S( -2, -13),   S( -2,  -9),   S(  6,  19),   S( -1,  -6),   S( -2,  -6),

            /* knights: bucket 13 */
            S( -4, -16),   S( -7, -20),   S( -8, -53),   S( -7, -28),   S(  1,  -5),   S( -8, -15),   S( -8, -25),   S(  2,  11),
            S( -3, -23),   S( -5, -22),   S(  3,   2),   S(-10, -46),   S( -5, -38),   S(  4, -18),   S( -6, -11),   S(  2,   3),
            S(  0, -17),   S( -2,   2),   S( -7, -27),   S( -3, -30),   S(  2,   4),   S(-11, -23),   S(  3,  -9),   S( -4, -10),
            S( -6, -29),   S(  1,   4),   S( -7, -39),   S( 14,  -1),   S(  6, -31),   S( 13,  19),   S(  0, -25),   S( 18,  36),
            S(  5,  24),   S(-10, -52),   S( -6, -76),   S( -6, -53),   S( -2, -20),   S(  8,  -1),   S( -5, -40),   S( -7, -21),
            S( -2,  -6),   S(  1,  -4),   S( -7, -27),   S( -2, -50),   S( -6, -20),   S( -7, -40),   S(  5,  -2),   S( -1,  -1),
            S(  0,  -6),   S(  3,   7),   S( -4, -14),   S( -7, -21),   S( -1,   0),   S(  1,   5),   S(  3,   2),   S( -2,  -9),
            S(  3,  10),   S(  0,   5),   S( -2, -12),   S(  1,   8),   S( -2,  -3),   S(  0,  -4),   S( -1,  -4),   S(  0,   1),

            /* knights: bucket 14 */
            S(  0,  -5),   S( -1,  -6),   S(  8,  27),   S( -2,  -9),   S(-12, -51),   S( -4, -14),   S(  5,   5),   S( -3, -19),
            S( -2, -13),   S(-10, -35),   S( -1, -29),   S(  0, -34),   S(  0, -22),   S( -1, -10),   S( -2, -19),   S(  4,  41),
            S( -1, -17),   S( -6, -45),   S(  3,  -3),   S( -9, -62),   S( -8, -48),   S(  6,  -7),   S( -4, -23),   S(  2,  20),
            S(  1,   4),   S( -9, -37),   S(-13, -49),   S(  5,  16),   S(  2,  -4),   S(-14, -58),   S(  0, -26),   S(  5,  16),
            S(  2,  -6),   S(-15, -53),   S( -6, -39),   S(-18, -41),   S(  0,   5),   S(  2,  -6),   S( -1, -10),   S( -6, -37),
            S( -3,  -5),   S(  6,  16),   S(  3,  37),   S( -3,   7),   S( -2, -10),   S( -2,  -6),   S( -2,   8),   S(-12, -30),
            S(  1,   0),   S( -6, -19),   S(  1,  -2),   S(  4,  26),   S(  7,  29),   S( -4, -18),   S(  5,   2),   S( -3, -10),
            S(  1,   1),   S( -1, -11),   S(  0,  -4),   S(  2,   1),   S( -2, -11),   S( -3, -11),   S(  2,   7),   S(  1,   4),

            /* knights: bucket 15 */
            S( -7, -33),   S( -3, -10),   S(  4,  16),   S( -2,   1),   S( -8, -33),   S(-10, -51),   S(  3, -26),   S( -4, -38),
            S( -1,  -6),   S(  7,  16),   S( -6, -28),   S( 17,  27),   S( 17,  -2),   S( -7, -53),   S( -3, -11),   S( -1,  -8),
            S( -2,  -9),   S(  1, -11),   S( -4, -45),   S( 13, -19),   S(-28, -113),  S(  2, -24),   S( -1, -13),   S( -4, -20),
            S(  0, -13),   S( -5,  -1),   S( -4, -22),   S( -7, -16),   S(  1, -52),   S( -4, -29),   S(  6,  18),   S(  1,   4),
            S( -3, -13),   S( 18,  36),   S(-11, -49),   S( -6, -16),   S( 16,  13),   S(  9,  22),   S( -3, -25),   S(  0,   5),
            S(  0,   3),   S( -9, -23),   S(  2,  20),   S( -3,  -7),   S( -7, -41),   S(  6,  40),   S(  0,  19),   S(  6,  23),
            S( -2,  -7),   S( -1,  -2),   S(  8,  28),   S(  4,   7),   S(  2,  14),   S(  4,  14),   S(  1,   9),   S(  3,   6),
            S(  2,   6),   S( -1,  -4),   S( -2,  -2),   S(  0,   2),   S(  1,   5),   S(  1,   7),   S(  0,   3),   S(  0,   3),

            /* bishops: bucket 0 */
            S( 54, -32),   S(-10,  18),   S( -7,  -2),   S(-17, -26),   S(  6,  -4),   S( 18,  -9),   S( 86, -76),   S( 34, -14),
            S(-23, -11),   S( 18, -32),   S(  2,  21),   S( 13, -10),   S( 23,  -2),   S( 65, -31),   S( 47,  28),   S( 58, -21),
            S( 34,   3),   S( 16,  18),   S( 24,  -2),   S( 25,  -3),   S( 50, -26),   S( 51,  22),   S( 58, -38),   S( 13, -13),
            S( 10, -16),   S( 80, -71),   S( 28,   7),   S( 77, -31),   S( 53,  33),   S( 49,  22),   S( 23, -17),   S(  0,  30),
            S( 29,  -2),   S( 45,  -6),   S( 70, -27),   S(114, -17),   S(124, -27),   S( 27,  20),   S( 55,   0),   S( 32,  -8),
            S( 28,   9),   S( 94,   0),   S( 47,  22),   S( 49,   4),   S( 23,  17),   S( 30,  15),   S( 40,   9),   S( 13,  -1),
            S(-48, -87),   S( 72,  20),   S( 80,  38),   S(-18,  13),   S(  4, -34),   S( 16,  21),   S(-18,  29),   S( -8,  50),
            S( -3, -24),   S( -4, -13),   S(  2, -36),   S(-15, -16),   S(-19, -11),   S(-20,   8),   S(-28,  -9),   S(-44, -28),

            /* bishops: bucket 1 */
            S(-14, -31),   S( 43, -35),   S(-18,  31),   S( 39, -12),   S( -8,  10),   S( 23,   5),   S( 36, -13),   S( 47, -14),
            S( 28, -21),   S(  4, -14),   S( 14,  -9),   S( -5,   2),   S( 47, -13),   S( 13, -10),   S( 71, -30),   S( 13, -15),
            S(-17,  10),   S( 45, -15),   S(  7, -13),   S( 41, -13),   S( 10, -11),   S( 60, -14),   S( 11,  -1),   S( 81, -19),
            S( 43, -21),   S( 47, -32),   S( 40,  -3),   S( 19,  -6),   S( 69, -13),   S( 13,  -6),   S( 80, -28),   S(  9,  -6),
            S( 13, -35),   S( 69,  -9),   S( 23,   3),   S(110, -19),   S( 76, -21),   S(102, -29),   S( 20,  -7),   S( 45,   3),
            S( 65, -36),   S( 44, -13),   S( 96,  -9),   S( 68, -26),   S(136, -60),   S(-13,  -1),   S( 10,  13),   S(-14, -11),
            S(-27, -58),   S(  2,   1),   S(-27, -42),   S( 14,   0),   S( 47,   4),   S( 15,  12),   S( 37, -14),   S(-47,  31),
            S( -6, -38),   S(  7,  -2),   S(-10, -50),   S(-66,   3),   S(  3, -20),   S(  7, -16),   S( 46, -17),   S( -7, -25),

            /* bishops: bucket 2 */
            S( 23, -16),   S(  2,  -8),   S(  6,  13),   S(  5,   7),   S( 30,  21),   S( -6,  19),   S( 37, -23),   S( 10,  29),
            S( 29, -17),   S( 20,  -5),   S(  7,  -3),   S( 19,   1),   S(  4,  16),   S( 24,  -9),   S( 15,  -7),   S( 25, -25),
            S( 32,   2),   S( 23,   3),   S(  9,  18),   S(  9,   5),   S( 10,  22),   S(  4,  -6),   S(  8, -14),   S( -6,   3),
            S(  1,  -1),   S( 32,  -5),   S( 10,  12),   S( 37,  19),   S(  6,  17),   S( -9,  30),   S(-15,  12),   S( 10,  -3),
            S(  9,  15),   S( 16,  16),   S( 85, -14),   S( 38,  15),   S( 14,  35),   S( 10,  24),   S(  7,  30),   S( 50,  -4),
            S(-26,  32),   S( 11,  19),   S(  2,   4),   S( 95, -17),   S( 85, -12),   S( 87,  13),   S( 73,  -4),   S( 16, -33),
            S( -2,  31),   S(  9,   2),   S( -9,  18),   S( -1,  26),   S(-93, -22),   S(-69,  10),   S(-12,  28),   S(-16, -23),
            S(-66,   3),   S(-39,  30),   S(  4,   4),   S(-34,  23),   S(-13, -26),   S(-23,   2),   S(-61, -10),   S(-29, -18),

            /* bishops: bucket 3 */
            S( 48,  11),   S( 53, -19),   S( 13, -14),   S(  7,   7),   S(  6,  36),   S(  4,  39),   S( -5,  60),   S(  1,  13),
            S( 40,   6),   S( 28,   1),   S( 23,   9),   S( 18,   8),   S( 27,  18),   S( 26,  17),   S( 16,  12),   S( 38, -22),
            S( -5,  15),   S( 28,  39),   S( 20,  36),   S( 26,  30),   S( 22,  37),   S( 20,  20),   S( 26,   0),   S( 11,  28),
            S(-18,  12),   S( 25,  20),   S( 29,  45),   S( 47,  35),   S( 43,  23),   S( 20,  17),   S( 34, -13),   S( 40, -31),
            S( 19,   7),   S( 16,  47),   S( 13,  51),   S( 56,  55),   S( 50,  39),   S( 46,  25),   S( 16,  29),   S( -8,  42),
            S( 16,  25),   S( 39,  39),   S( 20,  32),   S( 22,  35),   S( 52,  35),   S( 41,  71),   S( 46,  49),   S( 26,  92),
            S(-20,  58),   S( 29,  31),   S( 22,  24),   S( 26,  23),   S(  7,  47),   S( 68,  43),   S( -6,   7),   S( 18, -25),
            S(-57,  55),   S(-62,  53),   S(-58,  40),   S(-54,  64),   S( 12,  28),   S(-94,  68),   S(  1, -19),   S( 28,   8),

            /* bishops: bucket 4 */
            S(-39,   3),   S(  8,   5),   S(-31,   7),   S(-73,   5),   S(-35, -12),   S( 23, -31),   S(-25, -26),   S(-32, -70),
            S(-25,  15),   S(  2,  13),   S( 31, -26),   S( -4, -12),   S(-55,  10),   S( -2, -39),   S(-15, -14),   S(-33, -39),
            S(  5,  32),   S(-38, -23),   S( 44, -20),   S(  3, -15),   S( 14, -13),   S(-35,   6),   S(-19, -25),   S(-22,  -1),
            S( 47, -26),   S( 45,  -5),   S( 18, -27),   S(  8, -18),   S( 14,  -1),   S( 43, -12),   S(-40,  -1),   S(-22, -13),
            S(  7,   1),   S(-20, -73),   S( 29, -52),   S( 37,  -6),   S( 45,  -1),   S( 29,   9),   S(-14,  43),   S(-52,   6),
            S(-70, -82),   S(-33, -53),   S( 49, -27),   S( 50, -26),   S(-13,  -1),   S( 13,  13),   S(-21,  11),   S(-44,  20),
            S(  7,   7),   S(  4, -23),   S( -4, -64),   S( -8, -40),   S(-20,  10),   S( 20,  12),   S(-20,  22),   S( 25,  25),
            S( 12,   2),   S(  3, -20),   S( -6, -42),   S(  4, -45),   S(-11, -37),   S( -1,   7),   S(-23,  33),   S(  7,  19),

            /* bishops: bucket 5 */
            S(-43,  13),   S( -9,  -1),   S(-39,   5),   S(-40,   9),   S(-10,  -7),   S(-21, -14),   S( -8,   4),   S(-43,   9),
            S(-17,   4),   S(-27,   7),   S( -8,  -8),   S( 21, -20),   S(-43,  15),   S( -3,  -2),   S(-21, -12),   S(  6, -46),
            S( -3,   7),   S(-29, -14),   S( 31, -13),   S(-15,   6),   S( 10,   0),   S(-58,   9),   S( 27,  -3),   S( -4,   4),
            S( 24, -24),   S(-13,  -5),   S( 34, -11),   S( 68, -13),   S(-22,   3),   S( 40, -19),   S(-66,   7),   S(-35,  15),
            S(  1,  -9),   S( 13, -13),   S( -7, -22),   S( 21, -52),   S( 28, -28),   S( 28,  -8),   S( 32,  -5),   S(-49,   0),
            S( 20, -16),   S(-30,   0),   S( 31, -41),   S(-34, -36),   S(-56, -16),   S( 32, -19),   S( -3,   5),   S(  9,  17),
            S(-30, -31),   S( -1, -29),   S( -4,  -8),   S(-18,  -1),   S( -5, -15),   S( 13,   5),   S(  9,  28),   S( -6,  12),
            S(-25, -19),   S(-26, -36),   S( -5, -26),   S( -4, -18),   S(-35,  15),   S(-24,   1),   S(-29, -12),   S( -5,  36),

            /* bishops: bucket 6 */
            S(-40,  -8),   S(-27,   9),   S(-26,   7),   S(  4,   3),   S(-12,   8),   S(-21,  20),   S(-56,  33),   S(-47,  32),
            S(-15,  -5),   S(-20,  -9),   S(-35,   7),   S(-32,  16),   S(-44,  27),   S(-42,  14),   S(-57,  38),   S(-32,  12),
            S(  1,   2),   S(-38,   8),   S( -4,  -6),   S(-32,  22),   S(-11,  15),   S( -3,  -1),   S(-51,  13),   S(-27,  13),
            S(-34,  14),   S(-58,  13),   S(  9,   2),   S( 59,   0),   S( 47,   7),   S( 18,   2),   S( -5, -11),   S( 28,  -6),
            S(-45,  12),   S(-12,  17),   S( 34,  -4),   S( 82, -18),   S(-20, -27),   S(-30, -20),   S( 20,  -7),   S(-11, -15),
            S(-49,  27),   S(-26,  16),   S(-33,   9),   S( 31, -17),   S(-20,   1),   S(  4, -15),   S(  4,  11),   S(-15,  -4),
            S(-11,  18),   S(-27,  25),   S( 14,  -6),   S(-21,  -7),   S(-34,   7),   S(-19,  13),   S(-11,  -9),   S(-38,  -3),
            S(-41,  17),   S(-19,  -9),   S(-23,  19),   S(-20,   1),   S(-13,  17),   S( 27, -27),   S(-15,   9),   S( -9, -13),

            /* bishops: bucket 7 */
            S(-23, -25),   S(-103, -11),  S(-34, -36),   S(-31,  -4),   S(-38,   0),   S(-61,  16),   S(-55, -33),   S(-51,   2),
            S(-37, -38),   S( 25, -47),   S( 25, -39),   S(-27, -20),   S(-41,   9),   S(-40,  -7),   S(-51,  -4),   S(-34,  32),
            S(-25, -37),   S(-67,  15),   S(-18,   2),   S( 20, -22),   S( 12, -20),   S(  1, -21),   S(-71,  14),   S(-71,  34),
            S( 10, -48),   S(-71,  16),   S(-16,  10),   S(  2,  -4),   S( 96, -10),   S(-14, -21),   S( 32, -16),   S(-27,  -8),
            S(-77,   8),   S(-12,   9),   S( 15, -10),   S( 15,   4),   S( 47, -16),   S( 57, -21),   S(-32, -24),   S(-26, -22),
            S(-87,  13),   S( -2,  25),   S( 30,  -5),   S(-41,  -1),   S(-43,  18),   S( 64, -24),   S( 12,   2),   S(-83, -76),
            S(-18,  10),   S(-34,  26),   S(-74,  28),   S( 45,  -4),   S( 18, -25),   S(  3, -32),   S( 11, -18),   S( -6, -24),
            S(-51, -34),   S(-32,   2),   S(-29,  20),   S( -2,  -9),   S(-28,  -2),   S( -9,  -6),   S( 47, -21),   S(-23, -24),

            /* bishops: bucket 8 */
            S(  6,  88),   S(-25, -38),   S( 11,  14),   S( -8,  14),   S(  2,  19),   S( -8, -22),   S(-19, -45),   S( -5, -29),
            S( -4,   8),   S(  8,  49),   S(  8,  16),   S( -7,  17),   S( -8, -22),   S( 11,  19),   S(-17,  -8),   S( -5, -23),
            S(-12, -41),   S( -6, -56),   S( 16,  32),   S(  9, -24),   S(  7,  10),   S( 30,  10),   S( -6, -20),   S(-24, -53),
            S(  4,   8),   S( 14,  51),   S( 15,  33),   S(  8,  53),   S( 20,  13),   S( 19,  24),   S( -9,   2),   S( -2, -24),
            S(  7,  64),   S( 16,  91),   S(-11,  20),   S(  5,   1),   S(-15,  41),   S(-22,  25),   S( 20, -18),   S( -4,  12),
            S(-18, -24),   S( 10,  46),   S( 11,  50),   S( 12,  44),   S(  8,  20),   S( 12,  49),   S( 11,  37),   S( -2,  10),
            S(-11,  -7),   S( -8, -39),   S( 13,  64),   S( -7,  53),   S( -5,  17),   S(  3,  34),   S( 10,  79),   S(-24,   1),
            S(  0,   9),   S(  3,   8),   S( -1, -13),   S(  2,  32),   S( 12,  54),   S( 10,  45),   S( 12,  67),   S(  9,  64),

            /* bishops: bucket 9 */
            S(  4,  44),   S(-24,  17),   S(-31,  14),   S(-31, -22),   S(-13, -12),   S(-15, -31),   S(-14, -18),   S(-19, -29),
            S(  1,  21),   S( -5,  10),   S(-14,  13),   S(  4, -12),   S(-11, -19),   S( -3, -33),   S(-23, -16),   S(-19, -71),
            S( -7, -44),   S(  3,  11),   S( -9, -11),   S( -4,  -5),   S( -7,  18),   S(  3,  -1),   S(  0,   0),   S( -4, -28),
            S(-18,  23),   S(-15,  10),   S( -8,  15),   S(-10,  27),   S(-23,  27),   S(  4,   8),   S(-10,  12),   S(  5,  16),
            S(  1,  25),   S(-17,  17),   S( 11,  11),   S(-29,  -1),   S( -2,  29),   S( -5,  22),   S( 11,  14),   S(-24, -21),
            S( -5,  37),   S(-22,   0),   S(-29,  19),   S(  8,  19),   S(-26,  17),   S(-19,  26),   S(-15,  41),   S( -8,   8),
            S(-11,  31),   S(-11,  23),   S( -5,  16),   S(-12,   9),   S(-14,  13),   S( -1,  34),   S( -4,  40),   S(  5,  59),
            S( 10,  65),   S( -1,  23),   S(-10, -12),   S(  1,   8),   S(  5,  29),   S( -8,  21),   S( -7,  35),   S( 10,  76),

            /* bishops: bucket 10 */
            S(  2,  10),   S( 10,  37),   S(-15, -13),   S(-20,  -5),   S(-65, -17),   S(-53, -55),   S(-19,   9),   S(-10,  -5),
            S(-11, -22),   S(-11, -23),   S( -7,  -5),   S(-36, -32),   S(-28,  -2),   S(-36, -24),   S(-24, -18),   S( -7,   4),
            S( -6, -28),   S( -4, -33),   S(-15, -26),   S(-18,  16),   S(-15, -26),   S(  5, -12),   S(-25,   2),   S(  4, -39),
            S(-24, -42),   S(-34, -22),   S(-40, -18),   S( -7,  -2),   S(-17, -10),   S(  2, -10),   S(  8,  39),   S(-24, -11),
            S(  1,  17),   S(-66,  23),   S(-16,   0),   S(-10,   6),   S( -9,  -1),   S( -3,  18),   S(-33,   4),   S(  8,  32),
            S( -5,  41),   S(  2,  27),   S(-31,  21),   S(-22,  10),   S(-23,  -5),   S( -4,  24),   S( -7,  39),   S( 15,  58),
            S(  2,  16),   S(-25,  25),   S(-28,   6),   S( -4,  37),   S(-17,   3),   S(-29, -29),   S(-14,   3),   S(  8,  38),
            S(-10,  37),   S( -6,  36),   S(  3,  54),   S(-18,  14),   S(-13,  27),   S(-13,   8),   S( -3,   9),   S( -2,   6),

            /* bishops: bucket 11 */
            S( -2, -27),   S(-18, -24),   S(  2,   5),   S(-29,   3),   S(-32, -28),   S(  4, -10),   S(-32, -17),   S(-38,  28),
            S( -3,  15),   S(  6, -13),   S(-14,  -8),   S(  2, -30),   S(-15, -34),   S(-51, -14),   S(-27,   7),   S(  1,  10),
            S(-15, -63),   S(-10, -15),   S( 21,  -8),   S(-10, -13),   S( -8, -17),   S(  6,   0),   S( -2, -22),   S(  0,   6),
            S(  1,  17),   S( 11, -34),   S(  7,   0),   S(-31, -15),   S( 18,  14),   S(  5,  62),   S( 15,  64),   S(-16, -56),
            S(-21,   4),   S( -5,   1),   S(-26,  15),   S(-46,  37),   S( -8,  -1),   S(  2,   9),   S( -6,  35),   S(  2,  15),
            S( -6,  15),   S(-27,  -4),   S(-19,  47),   S(-14,  25),   S(-14,  41),   S(-14,  33),   S(  6,  25),   S(-15, -15),
            S( -7,  28),   S(-10,  88),   S( -9,  38),   S(  5,  25),   S(  8,  23),   S(-20,  -2),   S(-10, -24),   S( -8,   5),
            S(  1,  84),   S(-17,  23),   S( 13,  62),   S(  8,  25),   S( 16,  36),   S( -6,  18),   S( -6,   5),   S(  6,  23),

            /* bishops: bucket 12 */
            S( -4, -16),   S( -3, -28),   S(-10, -39),   S( -6,  10),   S(-15, -16),   S(-11,  -6),   S( -1,   0),   S(  2,  -1),
            S( -1, -14),   S( 10,  26),   S(  9,  31),   S( -5, -18),   S( -5, -13),   S( 18,  39),   S(-10, -14),   S(  0,  -2),
            S(  2,  19),   S( 13,  73),   S( 22,  48),   S(  0, -22),   S( -6,   3),   S( -9, -56),   S(  3,   0),   S(  0,   8),
            S( 11,  84),   S( 11,  61),   S(  7,  43),   S( 21,  44),   S( 15,  -8),   S(  1, -18),   S( -1,  16),   S(  6,  17),
            S( 13,  26),   S(  2,  19),   S(  7,  41),   S(  9,  24),   S(  9,  50),   S(  5,  36),   S(  1,  -5),   S( -1,  -4),
            S(  1,  -3),   S(-15, -36),   S( -4,  -3),   S( -4,  -8),   S(  9,  41),   S(  4,  19),   S(-19, -29),   S( -2, -21),
            S( -1,   8),   S(  3,  10),   S(  0,  -6),   S(  7,  12),   S(  7,  41),   S( 13,  32),   S(  6,  34),   S(  1,  29),
            S(  0,   7),   S( -2,   2),   S( -1,  -4),   S( -5, -20),   S(  2,   9),   S(  0,   7),   S(  3,  44),   S(  8,  37),

            /* bishops: bucket 13 */
            S(-11, -31),   S(-10, -18),   S( -7, -30),   S(  1,  17),   S(  5,   9),   S(-22, -37),   S(-17, -32),   S(  0, -23),
            S( -1,  -2),   S(-12, -19),   S(  1,  15),   S( 11,  64),   S(-12,   1),   S( -3,   3),   S(  7, -15),   S( -1, -12),
            S(  0,  15),   S( 15,  75),   S(  2,  16),   S( 16,  37),   S( -4,  -6),   S(  6,  10),   S(-13, -23),   S( -5,  10),
            S( 15,  74),   S( 14,  87),   S( -1,  27),   S(-20,  -4),   S( 20,  63),   S( -4,  -3),   S(  5,   9),   S( -5,   0),
            S(  7,  52),   S( -8,   5),   S(  3,   6),   S( -6,  11),   S( -7,  17),   S( -1,  37),   S( -5,  -5),   S(  1,  23),
            S( -5,  32),   S(  3,  27),   S(  1,  22),   S(  8,  29),   S( -2,  63),   S( -9, -41),   S(-10,   2),   S(  6,  30),
            S(  8,  27),   S( -7, -15),   S( -6, -17),   S(  2,  30),   S( -1,   0),   S( 11,  61),   S(  8,  51),   S(  6,  60),
            S(  2,   0),   S( -2,   4),   S(  4,  14),   S(  8,  42),   S(  3,  28),   S( -4, -12),   S( 10,  63),   S( -1,  11),

            /* bishops: bucket 14 */
            S( -9,  -3),   S(  5,  21),   S(  9,   5),   S( -2,  36),   S(-12, -19),   S(  0,   4),   S(-15, -44),   S( -6, -28),
            S( -9, -36),   S( -4, -17),   S(  8,  50),   S( -1,  18),   S(  1,  19),   S(  3,  19),   S(  7,  39),   S(  0,   6),
            S( -4,  -8),   S(  3,   3),   S(-12,   5),   S( 10,  31),   S( 14,  56),   S(  9,  61),   S( -4,  43),   S(  1,  21),
            S(  1,  14),   S(  9,  25),   S(-21, -13),   S( -2,  34),   S( -5,  13),   S(  2,  29),   S(  6,  64),   S( -1,  38),
            S(  4,  26),   S(  0,  22),   S(-10,  18),   S( 12,  68),   S(-11,  -6),   S(  2,  46),   S( 10,  36),   S( -7,  18),
            S(  1,  11),   S(  9,  20),   S(  1,  43),   S( 10,  39),   S(  6,  53),   S( -7,  16),   S(  3,  16),   S(  4,   9),
            S( 19,  82),   S(  8,  35),   S(  6,  40),   S(  2,  -8),   S( -1,   4),   S( 14,  58),   S( -3, -31),   S(  3,  26),
            S( 11,  68),   S(  7,  46),   S(  4,  -2),   S(  7,  26),   S( -5, -20),   S( -2,  -7),   S( 12,  36),   S(  0,   0),

            /* bishops: bucket 15 */
            S(  2,  17),   S(  0,   3),   S( -7, -23),   S( -3, -21),   S(-23, -45),   S( -4, -52),   S( -6, -49),   S(  1,   0),
            S(  5,  27),   S( -4, -11),   S( -3, -18),   S( 10,  30),   S( 15,   2),   S(  1,  -1),   S( -3, -12),   S( -4,  -8),
            S(  7,   3),   S( -6, -26),   S(  2,  27),   S( 19,  35),   S( 15,   5),   S(  9,  17),   S(  8,  48),   S(  0,  25),
            S(  1,   0),   S(  5,   1),   S( -5,   9),   S(-10,   2),   S(  5,  14),   S(  0,  17),   S( 15,  56),   S(  2,  33),
            S(-10, -14),   S(  1,  12),   S( -8,  -2),   S( 12,  49),   S(  1,  42),   S(  1,   7),   S( -4,  18),   S( -3,  -2),
            S(  2,   2),   S(  0,   4),   S( 19,  53),   S(  3,  31),   S( 14,  61),   S(  2,  30),   S(  1,   7),   S( -3,   0),
            S(  5,  37),   S(  3,  28),   S( -4,  42),   S( 11,  43),   S( 12,  64),   S( -5,   6),   S( -2,  18),   S( -4,  -6),
            S(  0,   6),   S(  8,  62),   S( 10,  61),   S(  8,  19),   S(  2,  17),   S( -2,   5),   S(  1,   3),   S(  4,  24),

            /* rooks: bucket 0 */
            S(-14,  26),   S(  8,  21),   S(  1,  18),   S(  2,  17),   S(-15,  64),   S( -7,  43),   S(-40,  87),   S(-50,  55),
            S( -7,  20),   S(-15,  34),   S(-14,  24),   S( -1,  33),   S( -7,  72),   S( -3,  30),   S(-15,  31),   S(-32,  76),
            S(  9, -10),   S(-10,  33),   S(-30,  50),   S(  4,  35),   S(-22,  61),   S(-20,  50),   S(-17,  69),   S( 13,  22),
            S(  4,  10),   S( 32,  20),   S(-46,  48),   S( 11,  24),   S(  5,  66),   S(-20,  49),   S(-24,  84),   S(-26,  76),
            S( 59, -41),   S( 10,  34),   S( -7,  53),   S( -2,  51),   S( 28,  38),   S( 43,  54),   S( 45,  64),   S(  3,  72),
            S( 41,   0),   S( 68,  60),   S( 65,  49),   S(112,  18),   S( 20,  79),   S( 43,  62),   S( 29,  79),   S( 11,  66),
            S( 56,  22),   S( 55,  93),   S(112,  71),   S( 47,  61),   S( 94,  47),   S( 29,  74),   S( -1,  93),   S(-20,  92),
            S( 17, -35),   S( 28,  25),   S( 43,  62),   S( 57,   5),   S( 97,  45),   S( 94,  44),   S( 53,  23),   S( 43,  10),

            /* rooks: bucket 1 */
            S(-58,  54),   S(-28,  21),   S(-17,  41),   S(-40,  47),   S(-33,  50),   S(-42,  53),   S(-52,  77),   S(-84,  84),
            S(-53,  47),   S(-31,  10),   S(-53,  59),   S(-32,  51),   S(-31,  28),   S(-57,  52),   S(-20,  18),   S(-38,  33),
            S(-53,  55),   S(-25,  19),   S(-25,  18),   S(-33,  45),   S(-52,  43),   S(-76,  56),   S(-62,  73),   S(-22,  60),
            S(-61,  62),   S(-34,  30),   S(-43,  53),   S(-16,  25),   S(-59,  61),   S(-62,  67),   S(-29,  58),   S(-56,  75),
            S(-51,  70),   S(-31,  18),   S( 22,  51),   S( 48,  21),   S(-14,  42),   S( -5,  66),   S(-39,  83),   S(-23,  93),
            S( 66,  36),   S( 64,  16),   S( 75,  13),   S( 32,  44),   S(-13,  56),   S( 12,  60),   S( 23,  47),   S( 28,  79),
            S( 26,  83),   S( 37,  35),   S( 49,  45),   S( 25,  45),   S( 48,  46),   S( 20,  69),   S( 44,  94),   S( 52,  95),
            S( 55,   4),   S( 53, -12),   S(  7,  -3),   S(  7,  10),   S( 45,   4),   S( 33,  29),   S( 92,  22),   S( 93,  37),

            /* rooks: bucket 2 */
            S(-65,  84),   S(-48,  76),   S(-42,  77),   S(-36,  44),   S(-25,  51),   S(-39,  46),   S(-31,  47),   S(-74,  74),
            S(-55,  88),   S(-60,  77),   S(-38,  70),   S(-47,  66),   S(-57,  70),   S(-54,  50),   S(-15,  18),   S(-60,  76),
            S(-57,  87),   S(-49,  78),   S(-54,  64),   S(-56,  77),   S(-44,  65),   S(-39,  49),   S(-18,  37),   S(-17,  55),
            S(-35,  93),   S(-20,  78),   S(-60,  85),   S(-80,  78),   S(-36,  69),   S(-45,  65),   S(-33,  56),   S(-22,  58),
            S(-43, 111),   S(-29,  99),   S( -4,  94),   S(-38,  81),   S(-39,  85),   S(  0,  73),   S(-18,  67),   S(-25,  90),
            S( 28,  94),   S( 25,  93),   S( 36,  79),   S(  3,  68),   S( 75,  31),   S( 25,  77),   S( 94,  39),   S( 67,  78),
            S( 62,  96),   S( 19, 102),   S( 54,  78),   S( 51,  61),   S( 24,  37),   S( 64,  91),   S(-58, 134),   S( 11, 118),
            S( 17,  79),   S( 30,  74),   S( 68,  51),   S( 15,  36),   S( -8,  27),   S( 58,  58),   S( 21,  52),   S( 57,  43),

            /* rooks: bucket 3 */
            S(-21, 104),   S(-12, 105),   S(-16, 135),   S(-11, 123),   S( -1,  88),   S(  3,  77),   S( 19,  67),   S(-15,  44),
            S(  0,  97),   S(-24, 118),   S( -3, 123),   S( -9, 129),   S( -4,  89),   S(  2,  61),   S( 51,  28),   S(  5,  56),
            S( 12,  99),   S(-16, 120),   S(-23, 123),   S(-10, 132),   S(  6,  90),   S( -9,  81),   S( 23,  72),   S(  9,  79),
            S(-11, 128),   S( -8, 148),   S(-29, 146),   S(-22, 137),   S(-12, 110),   S(-15, 101),   S( 13,  95),   S( -9,  83),
            S( -2, 138),   S(-23, 155),   S( 19, 148),   S(  2, 153),   S(  8, 113),   S( -1, 115),   S( 33, 107),   S( 22,  97),
            S( -2, 158),   S( 22, 140),   S( 35, 143),   S( 42, 133),   S( 94,  92),   S( 98,  97),   S( 71, 102),   S( 30,  99),
            S(  8, 158),   S( -1, 161),   S( 21, 164),   S( 14, 157),   S( 15, 150),   S( 99, 105),   S( 81, 134),   S(213, 104),
            S(117,  41),   S( 68,  93),   S( 57, 132),   S( 39, 122),   S( 29, 122),   S( 55, 102),   S( 46,  73),   S(134,  54),

            /* rooks: bucket 4 */
            S(-33, -16),   S( 10, -15),   S( -7, -14),   S(-17, -12),   S(-50,   9),   S(-29,  36),   S(-38,   5),   S(-56,  21),
            S(-61, -15),   S(-55,   7),   S(  0, -19),   S(  7, -44),   S(-28,  27),   S(-15,   9),   S(-43,   9),   S( 14,   0),
            S(-18, -26),   S(  0, -28),   S(-32, -11),   S(  1, -42),   S(-50,   3),   S(-27,  -1),   S(-27, -23),   S(-69,   8),
            S(-49, -29),   S( 26, -17),   S( 44, -44),   S( 19, -45),   S( 33, -14),   S( 16,  10),   S(-20, -15),   S(-13,  13),
            S(  3, -28),   S( 35, -18),   S( 46,  13),   S( 33,   9),   S( 54,   8),   S( 48,  17),   S(  7,  13),   S( 25,  26),
            S(-11, -32),   S(  8,   0),   S( 22,  -5),   S(  7,   9),   S( 39,  20),   S( 25,  21),   S( 42,  17),   S(  7,  21),
            S( -9, -21),   S( 56,  15),   S( 50, -10),   S( 72, -15),   S( 39,  22),   S( 13,  18),   S( 24, -10),   S( 15,   2),
            S( 21,  -9),   S( 30, -12),   S( 39, -26),   S(  9, -30),   S( 52,   2),   S( 18,   3),   S( 24,  -3),   S( 26,  14),

            /* rooks: bucket 5 */
            S(-47,  28),   S(  7, -17),   S( -3,   1),   S( 23,   2),   S(-25,  27),   S(-11,  15),   S(-35,  42),   S(-41,  31),
            S(-27, -12),   S(-22, -14),   S(-21, -15),   S( -8,  -2),   S( -3, -12),   S(-19,  -4),   S(-69,  22),   S(-37,  27),
            S(-65,  20),   S(-17,  -7),   S(  0, -25),   S(-25,   3),   S(-56,   8),   S( 13, -25),   S(-39,  10),   S(-43,  29),
            S(-92,  49),   S(-20,   9),   S(  5,  -6),   S( 13,  17),   S( 15,   1),   S(-19,  32),   S(-25,  13),   S( -3,  28),
            S( 28,  10),   S( 13,  12),   S(  8,  24),   S( 10,   2),   S( -1,  23),   S( 88, -17),   S( 20,  21),   S( 60,  20),
            S( 29,   9),   S(-17,   4),   S( 18,  10),   S(  2,  20),   S( 27,  22),   S( 18,  34),   S( 73,   9),   S( 53,  38),
            S( 72, -16),   S( 49, -18),   S(  3,  11),   S( 33,  13),   S( 70, -22),   S( 65, -25),   S( 83, -16),   S( 34,  10),
            S(-27,  32),   S( 23,   5),   S( 31,  -7),   S(  7,  29),   S( 47,  13),   S( 18,  22),   S( 21,  35),   S( -6,  46),

            /* rooks: bucket 6 */
            S(-61,  42),   S(-46,  27),   S(-29,  24),   S(-23,  18),   S(  3,  12),   S( -6,   5),   S( 32, -11),   S(-49,  19),
            S(-71,  34),   S(-12,  11),   S(-33,  20),   S(-29,  17),   S(-10,   3),   S(-30,   7),   S(-43,   2),   S(-53,  32),
            S(-74,  43),   S(-30,  24),   S(-14,   9),   S( -2,   4),   S(-22,  20),   S( 30, -14),   S( 16, -16),   S(-21,   8),
            S(-50,  41),   S(-35,  46),   S( 11,   6),   S( 45,  -9),   S( 22,  11),   S(  5,  11),   S(-13,   3),   S(-24,  41),
            S( -2,  44),   S( 39,  17),   S( 83,  14),   S( 94,  -9),   S( 41,  -1),   S(  5,  36),   S( 62,   3),   S( 93,   2),
            S(108,   7),   S( 94,  -6),   S( 94,   4),   S( 72, -14),   S(  1,  -2),   S( 25,  57),   S( 51,  -2),   S( 57,  14),
            S( 33,  12),   S( 86, -12),   S(137, -40),   S( 87, -35),   S( 68,   3),   S( 27,  -1),   S( 58,  -8),   S( 44, -16),
            S( 38,   6),   S(  9,  32),   S(-13,  31),   S( 67,  -9),   S( 46,  16),   S( 30,  26),   S( 59,  14),   S( 36,  15),

            /* rooks: bucket 7 */
            S(-106,  41),  S(-89,  48),   S(-66,  39),   S(-57,  29),   S(-43,  14),   S(-30,  -8),   S(-60,  32),   S(-81,   9),
            S(-91,  41),   S(-48,  23),   S(-72,  31),   S(-63,  33),   S(-47,   2),   S(-12, -14),   S( 13,   6),   S( 17, -53),
            S(-76,  34),   S(-84,  35),   S(-29,  15),   S(-80,  43),   S(-55,  25),   S(-47,  20),   S( 45, -21),   S(-41,   4),
            S(-89,  46),   S(-20,   9),   S(-15,   9),   S( 67, -22),   S(-16,  12),   S( 23,   1),   S( 48,  17),   S( 63, -26),
            S(-28,  40),   S( 32,  21),   S( 58,  14),   S(104,   2),   S(127, -30),   S(133, -42),   S(121, -29),   S(-77, -13),
            S(  8,  31),   S( 11,  15),   S(113,  -3),   S(109, -10),   S( 89, -11),   S( 60,   4),   S( 31,  43),   S( -7, -27),
            S( -9,  16),   S( 48, -11),   S( 38,  12),   S( 78, -16),   S(117, -31),   S(130, -26),   S( 86,  -2),   S( 54, -33),
            S(-47,  13),   S(-22,  25),   S( 35,  14),   S( 14,  14),   S(  3,  23),   S( 25,   7),   S( 52,  15),   S( 36,  -4),

            /* rooks: bucket 8 */
            S( -3, -74),   S( 15, -46),   S( 36, -44),   S( 50, -15),   S( -2, -62),   S(-27, -54),   S( 11, -69),   S(  9, -23),
            S( -8, -75),   S( -6, -53),   S(-22, -32),   S(-38, -83),   S(  6, -58),   S( -4, -26),   S( -2, -36),   S(-30, -39),
            S( 12,  -2),   S(  6,   4),   S( 12,  -6),   S(-25, -15),   S( -7,  17),   S( 27,  20),   S( 14,  32),   S(-16, -23),
            S( -9, -35),   S(  1,  -8),   S(  6, -25),   S( 15,  -2),   S(  4,   9),   S( 27,  11),   S( -7, -10),   S(-15, -43),
            S(-11, -58),   S( 19,  20),   S( 18,  -4),   S( 14,   8),   S( 26,   9),   S(  2, -17),   S(  9,  19),   S(  2, -11),
            S(-15, -15),   S( 23, -10),   S(-23, -47),   S(-14, -13),   S( -1,   8),   S( -4,  -8),   S( -4,  17),   S(  3,  32),
            S( 25,  16),   S( 15, -12),   S( 16,  -2),   S( 30,  20),   S(  4,   2),   S(  7,   5),   S( 22,  20),   S( -2,  33),
            S( -9,  18),   S( 27,   1),   S( -4, -15),   S( 33,  25),   S( -5,  10),   S( 18,  32),   S(-13,  -3),   S(  3,  -8),

            /* rooks: bucket 9 */
            S( -1, -93),   S( 19, -94),   S( -8, -116),  S( 26, -97),   S( 15, -73),   S( 18, -72),   S( -8, -60),   S( 12, -66),
            S(-42, -96),   S(  0, -84),   S(-19, -65),   S( -7, -76),   S(-14, -81),   S(-22, -40),   S(-29, -68),   S(-11, -59),
            S( -7, -21),   S(  1, -34),   S(  4,   4),   S(-11, -55),   S( 10, -72),   S(-19, -23),   S(  8,  15),   S( -1,  -2),
            S( 22, -30),   S( 21,  12),   S(  3, -15),   S(  3,  -2),   S( -6, -56),   S( 16, -48),   S( -1, -15),   S(-11, -40),
            S( 28, -30),   S( -6, -39),   S( -7, -52),   S( -4, -34),   S( -1, -61),   S( -9, -21),   S(  2, -49),   S( -5, -48),
            S(  7, -44),   S(-16, -41),   S(  0, -42),   S( 28, -27),   S( 20, -27),   S(  1, -29),   S(-23, -31),   S(-15, -37),
            S(  3, -29),   S( 38, -40),   S(  5, -42),   S(-19,  -2),   S( 14, -48),   S( 11,  -5),   S( 10, -19),   S( -5, -57),
            S(-15, -28),   S(-17, -25),   S( -6, -14),   S( 12,  -5),   S( 20, -31),   S(-11,  -4),   S(-10,  11),   S(-11, -13),

            /* rooks: bucket 10 */
            S( 23, -88),   S(-52, -72),   S( 16, -106),  S( 23, -112),  S( 43, -107),  S( 21, -99),   S(  6, -105),  S( -4, -81),
            S(-23, -71),   S(-16, -76),   S(-48, -59),   S(-25, -81),   S(-15, -90),   S( -1, -69),   S( -4, -46),   S(-28, -79),
            S(-18, -44),   S(-45, -26),   S(-14, -54),   S(-47, -60),   S(-23, -45),   S(  0, -35),   S( -2, -64),   S(-15, -56),
            S( -7, -36),   S(-37, -66),   S(  6, -42),   S(-14, -33),   S(  4,  -9),   S(  4,   3),   S( -9, -85),   S(  4, -50),
            S(  4, -48),   S( 12, -55),   S(  0, -66),   S( -5, -57),   S(  7, -28),   S(  0, -14),   S( 10, -69),   S(-17, -94),
            S(-25, -52),   S(-12, -33),   S( -1, -55),   S( 17, -82),   S(  1, -39),   S( -3, -60),   S(-40, -61),   S( -5, -43),
            S(-32, -43),   S(-22, -43),   S( -9, -57),   S(  9, -57),   S( 29, -31),   S( 14, -47),   S(-18, -71),   S( 16, -49),
            S(-19, -36),   S(-15, -24),   S( -5, -23),   S(-18, -43),   S( -7,   5),   S(-12, -25),   S( -3, -59),   S(-21, -29),

            /* rooks: bucket 11 */
            S(  4, -65),   S( -7, -40),   S(-12, -50),   S(  5, -35),   S(-29, -53),   S( 60, -70),   S( -5, -72),   S( -9, -85),
            S( -7, -15),   S(-13, -24),   S(-52, -25),   S(-38, -29),   S(-30, -22),   S(  3, -22),   S(-15, -76),   S(-22, -78),
            S(-34,  -6),   S(-17, -13),   S( -6,  -4),   S(-27, -14),   S( 17, -30),   S(  5, -14),   S( 19, -19),   S(-14, -23),
            S( -7, -33),   S(  2, -32),   S(  1, -25),   S( 34,  20),   S( 28, -33),   S( -1, -29),   S(  9,  21),   S(-10, -34),
            S(-21, -32),   S( 17, -25),   S( -8, -13),   S( -4,  -5),   S( 39, -30),   S( 20, -50),   S( 30,   1),   S(-17, -59),
            S( -6, -33),   S(-18, -29),   S(  0, -43),   S( 17,  -6),   S(-19, -32),   S( 19, -25),   S( 14, -15),   S( 12, -25),
            S(-15, -18),   S(-18, -33),   S(-12, -15),   S( -8, -34),   S( -5, -12),   S( 30, -24),   S( 30, -44),   S(-10, -44),
            S(-17, -11),   S( 31,  22),   S( -2,   4),   S( 16,  11),   S(-26,  10),   S( 28,  -5),   S( 33, -30),   S(-14, -14),

            /* rooks: bucket 12 */
            S(-25, -109),  S(  5,  -3),   S( 18, -32),   S(-16, -58),   S(-10, -55),   S( 11, -36),   S(-20, -61),   S(-19, -60),
            S( 12,  12),   S(  1,  -6),   S(  9,  13),   S( 10,  -3),   S( 12, -10),   S( 21,  -1),   S(  1, -24),   S(-11, -39),
            S(-10, -19),   S(  7,  15),   S(  5,  -7),   S( 12,  -5),   S(  8, -28),   S( 13, -20),   S( 13,  34),   S( -4, -32),
            S(-10, -19),   S(  4,  -4),   S(  5,   6),   S(  6,  -2),   S(  6,   5),   S(  1,  -2),   S(  9,  11),   S( -6, -11),
            S(  8, -13),   S( 10,  16),   S( -3, -21),   S( -8, -48),   S(  5,  12),   S(  0, -11),   S( -1, -18),   S(  5,   2),
            S( -2, -26),   S( -5, -33),   S(  3, -32),   S( -5, -29),   S(  9,   3),   S( -9, -36),   S( 13,  28),   S( -5, -24),
            S(-23, -36),   S( -4, -15),   S(  8,  -8),   S(  4,  10),   S( -5, -40),   S(  7,  -7),   S(  8,  25),   S( -3, -11),
            S( -6, -19),   S(  0, -15),   S(  7, -27),   S( 15,  18),   S(  1,  11),   S(  3,  -3),   S( -9, -29),   S(  4,  31),

            /* rooks: bucket 13 */
            S(-28, -68),   S(-17, -83),   S(-10, -32),   S( -6, -29),   S(-21, -86),   S( 13, -44),   S(-18, -48),   S(-30, -63),
            S( -6, -27),   S( -9, -35),   S( -2, -10),   S(  2, -16),   S( 13,   3),   S( 10,  -5),   S(  4, -51),   S( -2, -35),
            S( -7, -39),   S( -7, -43),   S(  8,   1),   S(  5, -30),   S( 14, -15),   S( 16, -27),   S(  5, -13),   S( -8, -63),
            S(  9, -19),   S(-12, -45),   S( 15,   6),   S( 13,  -2),   S( -4, -28),   S( -6, -16),   S(  1,   8),   S( -5,  -5),
            S(  5, -23),   S( -3, -70),   S(-13, -78),   S(  5, -22),   S(  5, -41),   S( -8, -33),   S( -4, -16),   S( -2, -11),
            S( -4, -25),   S(  0, -37),   S(  0, -38),   S( -8, -62),   S(-13, -116),  S(  8, -17),   S( -8, -21),   S( -8, -30),
            S(  5,  -8),   S( -1, -45),   S(-11, -58),   S(  4, -27),   S( -5, -46),   S( -3, -34),   S( -9, -46),   S( -8, -33),
            S(  6,   4),   S(  0, -21),   S( -4, -33),   S( 13,  -7),   S( -6, -46),   S(  0, -16),   S( -3, -20),   S( 10,   4),

            /* rooks: bucket 14 */
            S(  4, -38),   S(-35, -73),   S(-17, -87),   S(-10, -76),   S(-14, -63),   S(  3, -43),   S(-38, -130),  S(-21, -55),
            S(  1, -23),   S(  3, -37),   S(  5, -43),   S( -5, -57),   S( -5, -34),   S( -5, -20),   S( -6, -55),   S(-10, -84),
            S(  5,  -3),   S( -1, -39),   S(  6, -38),   S(  9, -16),   S(  7, -21),   S( -8, -32),   S(  6, -48),   S(-22, -95),
            S(-10, -21),   S(  5,  17),   S( 11,  33),   S(  3, -25),   S(-11, -44),   S(-11, -42),   S(  1, -26),   S(-16, -55),
            S(  4,   3),   S( 11,  27),   S(  0, -10),   S(  8, -40),   S(  1, -28),   S( 15,  -7),   S( -9, -54),   S( -8, -38),
            S(  5, -13),   S(  1,   3),   S( -4, -37),   S(  3, -80),   S(  6, -50),   S( -5, -60),   S( -8, -89),   S(-18, -65),
            S(-13, -15),   S(-13, -39),   S(-19, -85),   S(-18, -87),   S(  7, -29),   S(  1, -22),   S(-17, -70),   S( -5, -28),
            S( -5, -17),   S(-14, -36),   S(  3, -18),   S(  0, -60),   S(-16, -69),   S(-12, -87),   S(  1, -33),   S( -7,  -8),

            /* rooks: bucket 15 */
            S( -9, -58),   S( -5, -32),   S(-29, -43),   S(-13, -64),   S(-13, -61),   S(-16, -52),   S(  3, -16),   S(-19, -56),
            S( 18,  24),   S( -5, -49),   S(  1,  -4),   S(-11, -53),   S(-10, -35),   S(  1, -24),   S( 10,   7),   S(  6,   9),
            S( -1, -14),   S( -4, -32),   S( 15,  19),   S( -4, -34),   S(  3, -46),   S( -4, -44),   S(  2,   4),   S(  3,  -6),
            S(  8,  31),   S( -7, -17),   S( 22,  58),   S( -3,  -1),   S( 11,  13),   S( -9, -45),   S(  6, -25),   S( -6, -43),
            S(  6,   4),   S( -1, -32),   S(  9,   8),   S(  9,   5),   S(  4,   8),   S( -3, -49),   S(  2, -46),   S( -2, -21),
            S(  1,   8),   S(  4,  12),   S(  1, -10),   S(  3, -13),   S( -2, -24),   S(  2, -50),   S( 10, -28),   S(  3, -28),
            S( -5, -32),   S( -1, -10),   S(  5,  -6),   S( -5, -41),   S(-10, -40),   S( -1, -40),   S(  4, -36),   S(-11, -63),
            S( -3, -19),   S( -3, -10),   S( 11,  30),   S(  0,   2),   S(  0,  -9),   S( -3, -46),   S(  7, -17),   S(-21, -58),

            /* queens: bucket 0 */
            S( -3, -39),   S(-35, -78),   S(-16, -80),   S(  7, -127),  S( -1, -89),   S(  9, -84),   S(-72, -54),   S(-15, -42),
            S(  1, -85),   S( 14, -89),   S(  9, -55),   S(-10, -44),   S(  0, -42),   S( -5, -54),   S(-20, -47),   S(-25,   0),
            S( -2,  -1),   S( 15, -68),   S( 28, -57),   S(-14, -27),   S(-21,  -1),   S(-13,  -2),   S(-45,  17),   S(-67, -74),
            S(-38,  58),   S( 13, -20),   S(-22,  44),   S( -6,  39),   S(-14,  48),   S(-15,  19),   S(-36,   6),   S(-10, -59),
            S(-34,  -2),   S(-24,  90),   S(-16,  75),   S(-16,  74),   S( -6,  62),   S(-33,  56),   S(-56,  44),   S(-50,   9),
            S(-51,  62),   S( 10,  95),   S( 33,  70),   S(-13,  97),   S(-52,  62),   S(-43,  53),   S(-83,  34),   S(-58, -12),
            S(  0,   0),   S(  0,   0),   S( 17,  17),   S(-28,  43),   S(-50,  51),   S(-82,  87),   S(-95,  64),   S(-102,  21),
            S(  0,   0),   S(  0,   0),   S( -3,  -3),   S(-12,  21),   S(-44,  56),   S(-56,  11),   S(-70, -11),   S(-72,  -9),

            /* queens: bucket 1 */
            S(-16, -47),   S( -1, -27),   S( 13, -100),  S( 30, -128),  S( 28, -77),   S( -7, -58),   S( -7, -31),   S(-18, -10),
            S(-37, -52),   S(  3,  24),   S( 34, -75),   S( 18, -33),   S( 29, -41),   S(  1, -43),   S(-48,  33),   S(-54, -39),
            S( 32, -24),   S( 21, -22),   S( 18, -22),   S(  9,  10),   S(-37,  60),   S( 13,  43),   S(-11, -27),   S( -2,  18),
            S( 23, -17),   S( 37,  29),   S( -8,  55),   S( 25,  65),   S( -9,  74),   S(-11,  53),   S( -2,  40),   S(-18,  61),
            S( 26,  18),   S(  1,  66),   S( 15, 111),   S( -4,  77),   S( 20, 112),   S( 76,   5),   S(-43,  90),   S( -9,  68),
            S( 38,  31),   S( 77,  47),   S(100,  59),   S(133,  94),   S( 64,  61),   S(  1,  74),   S(  7,  58),   S( -3,  29),
            S( 83,  -1),   S( 31,  25),   S(  0,   0),   S(  0,   0),   S( 12,  74),   S(-13,  52),   S(  1,  91),   S(-51,  38),
            S( 92,   4),   S( 39,   9),   S(  0,   0),   S(  0,   0),   S( 32,  25),   S( 23,  44),   S( 89,  28),   S(-13,  48),

            /* queens: bucket 2 */
            S( 20, -27),   S( 18, -44),   S( 29,   9),   S( 41, -32),   S( 49, -65),   S( 13, -30),   S(-27, -49),   S(  9,  19),
            S( 17,  16),   S(  9,   8),   S( 34,  -4),   S( 36,  -6),   S( 46, -21),   S( 15,  -7),   S( 15,   1),   S( 17,  10),
            S( 18,  26),   S( 23,  27),   S( 12,  71),   S( 12,  45),   S( 20,  42),   S( 17,  50),   S( 26,  22),   S( 30,  58),
            S( 25,   3),   S(  7, 102),   S( -5,  99),   S( -2, 123),   S( 17,  98),   S(  4,  76),   S( 20,  85),   S( 19,  94),
            S(-12,  95),   S( 13,  47),   S(-16, 134),   S( 17, 121),   S( 27, 149),   S( 74,  74),   S( 47, 108),   S( 55,  69),
            S(-27,  75),   S(-11,  85),   S(-15, 116),   S( 58, 106),   S( 48, 109),   S(107,  84),   S(132,  84),   S( 18, 149),
            S( -3,  72),   S(-25,  99),   S(-16, 138),   S( 71,  70),   S(  0,   0),   S(  0,   0),   S( 23, 122),   S( 32, 110),
            S(  8,  42),   S( 28,  39),   S( 48,  44),   S( 14,  95),   S(  0,   0),   S(  0,   0),   S( 80,  78),   S( 30, 106),

            /* queens: bucket 3 */
            S(-31,  30),   S(-19,  19),   S(-11,  34),   S(  1,  42),   S(-13,  22),   S( -7, -21),   S(-14, -40),   S(-37,  12),
            S(-40,  43),   S(-27,  49),   S(-10,  45),   S(-12,  70),   S( -8,  59),   S( -6,  22),   S( 21, -11),   S( 30, -43),
            S(-40,  60),   S(-27,  81),   S(-23, 119),   S(-33, 134),   S(-26, 101),   S(-17, 101),   S( -6,  70),   S( -1,  47),
            S(-34,  67),   S(-44, 125),   S(-44, 147),   S(-22, 157),   S(-35, 151),   S(-23, 115),   S( -1, 100),   S(-19,  95),
            S(-43, 110),   S(-43, 141),   S(-29, 144),   S(-40, 183),   S(-22, 170),   S(-25, 201),   S(-20, 157),   S(-30, 135),
            S(-67, 120),   S(-49, 141),   S(-73, 189),   S(-59, 193),   S(-41, 203),   S( 12, 168),   S(-24, 200),   S(-20, 173),
            S(-89, 130),   S(-102, 163),  S(-70, 186),   S(-65, 173),   S(-101, 221),  S(-28, 155),   S(  0,   0),   S(  0,   0),
            S(-156, 189),  S(-120, 175),  S(-76, 138),   S(-76, 150),   S(-48, 156),   S( -6, 108),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-33,  14),   S(-55, -56),   S(-11,  29),   S(-22, -34),   S(  4,  -2),   S(-25,  -8),   S(-41, -25),   S(  6,   7),
            S( -7,  22),   S( -3,  24),   S( -1, -13),   S( -6, -10),   S(-39,   9),   S(-38,  13),   S(-39, -37),   S( -1, -16),
            S(-12,   9),   S( 18,  -5),   S( 21,  -8),   S(  4, -12),   S( 53,  -8),   S( -9,  -4),   S(  0, -13),   S(  5,  -2),
            S(  9,   3),   S( 29,  -6),   S(  7,   8),   S( 17,  42),   S( 48,  37),   S( -7,  41),   S(-34, -25),   S(-13,  -7),
            S(  0,   0),   S(  0,   0),   S( 42,  19),   S( 71,  68),   S( 12,  68),   S(  1,  24),   S( -7,  -8),   S( 31,  16),
            S(  0,   0),   S(  0,   0),   S( 24,  34),   S( 41,  39),   S( -2,  43),   S( 34,  48),   S(  1,  31),   S(-12,  13),
            S( 25,  13),   S( 21,  34),   S( 70,  37),   S( 34,  30),   S( 58,  15),   S(-24,  21),   S(  1,  -1),   S(-53,   6),
            S( 60,  31),   S(-20,  -5),   S( 40,  24),   S( 55,  52),   S( -5, -23),   S( -3,  14),   S(-10, -29),   S(  6, -24),

            /* queens: bucket 5 */
            S( 19, -10),   S(  5,   9),   S(-44,  -6),   S( -2,  -5),   S( 34, -36),   S( 50,  57),   S( 18,  12),   S(  1,  -2),
            S(  5,   7),   S(-15,  -7),   S( -3,  -3),   S(  8,  17),   S(  8,   5),   S(-47, -32),   S( 40,  -3),   S( 15,   3),
            S( 44,  31),   S( 50,  -7),   S( 12,  21),   S(  1,  35),   S(  5,  22),   S( 29,  16),   S(-20,  40),   S( -5,  32),
            S( -9, -18),   S( 22,  -5),   S( 30,  10),   S( 51,  39),   S( 86,  35),   S( 20,  35),   S( 22,  57),   S(  6,  26),
            S( 53,  36),   S( 35, -12),   S(  0,   0),   S(  0,   0),   S(  6,  15),   S( 52,  15),   S( 59,  40),   S(  4,  -3),
            S( 55,  27),   S( 48,  68),   S(  0,   0),   S(  0,   0),   S( 29,  27),   S( 82,  59),   S( 37,  -2),   S( 25,  29),
            S( 74,   8),   S( 76,  34),   S( 77,  91),   S( 35,  48),   S( 55,  50),   S(130,  81),   S( 36,  78),   S(  4,   2),
            S( 41,  20),   S( 45,  38),   S( 74,  52),   S( 62,  53),   S( 68,  59),   S( 55,  60),   S( 26,  21),   S( 35,  20),

            /* queens: bucket 6 */
            S( 68,  34),   S(-11, -19),   S(  3, -19),   S(  0,  39),   S( -5,   4),   S(-27,   0),   S(-21, -21),   S( 36,  28),
            S( -1,   6),   S( 30,  13),   S( 51,  18),   S( 30,  27),   S( 23,  14),   S(  7,  -1),   S(-22,  11),   S(  2,  29),
            S(-24,  35),   S(  5,  46),   S( -2,  61),   S( 46,  -5),   S( -3,  24),   S( 41,   6),   S( 58,  22),   S( 74,  44),
            S(-14,  36),   S(-39,  30),   S( 54,  12),   S( 63,  60),   S( 43,   4),   S( 38,  37),   S( 91,  33),   S(104,  41),
            S(  1,  19),   S(-21,  47),   S( 22,  90),   S( 76,  61),   S(  0,   0),   S(  0,   0),   S( 53,  39),   S(106,  91),
            S( 27,  18),   S( 51,  38),   S( 76,  54),   S( 66,  53),   S(  0,   0),   S(  0,   0),   S(110,  85),   S(130,  61),
            S( 14,   4),   S(  2,  34),   S( 54,  37),   S( 66,  44),   S( 41,  47),   S( 78,  72),   S(122,  64),   S(131,  15),
            S( 14,  12),   S( 72,  20),   S( 57,  29),   S(102,  63),   S(110,  49),   S(115,  66),   S(132,  41),   S( 98,  19),

            /* queens: bucket 7 */
            S(-24, -23),   S(-30, -18),   S(-13,   7),   S(-38,  18),   S(-17,   3),   S(-31,  12),   S(-18, -18),   S(-18, -19),
            S(-61,   2),   S(-46,  -3),   S(-34,  49),   S(-18,  36),   S(-46,  30),   S(-30,  37),   S( 13, -29),   S(-24,  -9),
            S(-56,  -5),   S(-46,   7),   S(-24,  50),   S(  6,  42),   S( 74, -23),   S( 18,  27),   S( 32, -27),   S( 33,   0),
            S(-47,   5),   S( -2,  18),   S(  5,  20),   S( 30,  50),   S( 89,  43),   S( 65,  24),   S( 72, -21),   S( 48,  -4),
            S(-46, -25),   S(-10,  13),   S( 29,  60),   S( 43,  51),   S( 86,  33),   S( 62,  29),   S(  0,   0),   S(  0,   0),
            S( -7,  -9),   S(-32,  45),   S( -2,  69),   S( -8,  58),   S( 58,  16),   S(131,  88),   S(  0,   0),   S(  0,   0),
            S(-29,  36),   S(-16,  14),   S(-11,  31),   S( 53,  37),   S( 64,  36),   S( 91,  39),   S( 49,  17),   S( 96,  54),
            S(-12, -12),   S(  3,  -4),   S( 18,  43),   S( 74, -18),   S( 65,  34),   S( 25,  28),   S( 22,  16),   S( 91,   7),

            /* queens: bucket 8 */
            S(  2,  -8),   S( 29,  13),   S(-15, -26),   S(  5,   0),   S(-11, -21),   S( 17,  10),   S(  5,  -4),   S(  1,   3),
            S(-19, -39),   S( -7,  -7),   S( 25,  10),   S( -5,   2),   S( 26,  17),   S(  5,  -1),   S(  6,  21),   S(-10, -13),
            S(  0,   0),   S(  0,   0),   S( 13,   1),   S(-11, -21),   S( 23,  13),   S( 13,  13),   S( -1,   0),   S( -1,   4),
            S(  0,   0),   S(  0,   0),   S(  5,   9),   S( 23,  25),   S( 15,  14),   S(  6, -14),   S( -1,  12),   S(  0,  -3),
            S(  6,  -1),   S(  0,   3),   S( 22,  29),   S( 33,  14),   S(-11, -30),   S( -6, -12),   S( 11,  -2),   S(-14, -19),
            S( 11, -11),   S(  8,   1),   S( 30,  43),   S( -4, -11),   S( 12,  12),   S( 11,   5),   S( 12,  -3),   S(-15, -37),
            S( -5, -22),   S( -7, -27),   S( 16,   8),   S( 31,  47),   S(  7,  -5),   S( -8,  -8),   S( -1, -23),   S( -5, -16),
            S( 19,  12),   S( 11,   5),   S( 23,  35),   S(  3,  -5),   S( 20,  10),   S(-14, -22),   S( -8,  -8),   S(-27, -62),

            /* queens: bucket 9 */
            S( 16,   0),   S(  1, -23),   S( -3, -23),   S( 13, -14),   S(  9,  -3),   S(-10, -37),   S(-12, -21),   S( -6, -17),
            S( 21,  15),   S( 12,   4),   S( 11,   6),   S( 25,  28),   S( -2, -19),   S(  8, -11),   S( -6, -18),   S( -7, -24),
            S(  8, -20),   S(-17, -40),   S(  0,   0),   S(  0,   0),   S( 13,  10),   S( 24,  21),   S( -4, -10),   S(  0,   1),
            S( 23,  16),   S(-12, -28),   S(  0,   0),   S(  0,   0),   S(-10, -18),   S( 29,  32),   S( -4, -16),   S(-13, -11),
            S( 10,  -6),   S( 24,  19),   S( 15,  16),   S(-17, -27),   S(-13, -25),   S( 27,  38),   S( 16,  -5),   S( -2, -21),
            S(  6,  14),   S( 11,  -4),   S(  0,  -6),   S(  2, -29),   S( 16,  21),   S( -3,  -3),   S( -9, -18),   S( -3, -27),
            S( -2,   3),   S( 32,   9),   S( -6,  -8),   S( -7,  -5),   S( 13,   0),   S( 34,  39),   S(  0,   3),   S( 12,   0),
            S(  0, -33),   S(  3, -34),   S(-21, -44),   S( 11,  -3),   S( 25,  21),   S(  3,   0),   S(  9,   0),   S(-10, -35),

            /* queens: bucket 10 */
            S( 20,  25),   S(  8, -14),   S(-12, -21),   S(  5,  -4),   S( -3, -30),   S( 24,  30),   S( -9, -26),   S(  5,  -9),
            S( 12,   8),   S(-23, -40),   S( 24,  15),   S(  2,  -8),   S(  3,   1),   S( 31,  23),   S(  1, -11),   S( -6, -16),
            S(  1,  -2),   S(  4,   1),   S( 11,   1),   S( -3, -21),   S(  0,   0),   S(  0,   0),   S(  1,   0),   S(-32, -41),
            S(  3,  -9),   S(  6,   3),   S(  1, -10),   S(  6,  12),   S(  0,   0),   S(  0,   0),   S( -2, -12),   S( 14,   9),
            S( 16,  29),   S( 22,   9),   S( -2, -22),   S( 56,  61),   S( 21,  33),   S(  0,  -8),   S( 10,  -4),   S( 25,   4),
            S(-10, -17),   S(  9,   9),   S( 35,  36),   S( 30,  36),   S( 32,  36),   S( 18,  15),   S( 18, -14),   S(  2, -23),
            S( 10,   2),   S( 19,  34),   S( 18,  40),   S( 39,  52),   S( 17,  25),   S( 20,  15),   S( 17,  11),   S( 11,  -9),
            S( -1, -20),   S(-10, -20),   S( 14,  -4),   S(-10, -39),   S(  6,   2),   S(-16, -33),   S(  0, -16),   S( -7, -28),

            /* queens: bucket 11 */
            S(-16, -10),   S(-20, -27),   S(  1, -14),   S(-25, -38),   S(-15, -12),   S( -5, -18),   S( -8, -32),   S(  1, -11),
            S( -7,   2),   S( -3,   3),   S(-21, -16),   S(  4,   0),   S( 25,   6),   S( 15,  10),   S(  5, -21),   S( 13,   5),
            S( -8,  -9),   S(  7,   5),   S(-46, -39),   S(  2, -23),   S( 12,   8),   S(-19, -38),   S(  0,   0),   S(  0,   0),
            S(-13, -24),   S(-15, -11),   S( -7,  -2),   S(  4,   3),   S(  2,  -4),   S( -5,   5),   S(  0,   0),   S(  0,   0),
            S( -7, -15),   S(  8,   5),   S( 14,  35),   S(  0,  12),   S( 39,  59),   S( 32,  39),   S( 21,  26),   S( -6,  -9),
            S(  3,   5),   S( -2, -12),   S(-19, -22),   S( 11,  16),   S( 26,  43),   S( 26,  13),   S( -1,   0),   S( 17,  -9),
            S(  1,   5),   S(-10,  -9),   S( 21,  14),   S(  1,  11),   S( 30,  47),   S(  3,  -8),   S(-12, -17),   S( 16,   5),
            S(-30, -89),   S( -5, -23),   S(-18, -37),   S( 10,  16),   S( 11,   0),   S( 10,   5),   S( -8,  -6),   S( 16,   8),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 18,  28),   S( -3,  -9),   S(  3,  17),   S( -5, -12),   S( 10,   3),   S( -5,  -8),
            S(  0,   0),   S(  0,   0),   S(  5,   1),   S( -1, -15),   S( 10,  13),   S( -3, -17),   S( -8, -16),   S( -7, -18),
            S( -1,  -2),   S( 10,  10),   S(  3,  -2),   S( 18,  11),   S( 10,  24),   S( -1,   2),   S(  0,   3),   S(  6,   6),
            S(  2, -14),   S( 24,  28),   S( 16,  25),   S(  7,  14),   S(  0,   1),   S(  4, -13),   S( -8, -19),   S( -2,  -7),
            S(-13, -20),   S(  2,  -5),   S( -5, -26),   S( -2,  -7),   S( -3,   6),   S(-11, -43),   S( -9, -24),   S( -8, -19),
            S(  6,   6),   S( -2,  -2),   S(  6,  16),   S(-12, -26),   S( -7, -23),   S(-22, -24),   S(-18, -33),   S(  1,  -2),
            S(-12, -28),   S(  7,  13),   S(-11, -16),   S(  3,   6),   S(-11, -29),   S(-19, -39),   S( -3,  -9),   S( -9, -17),
            S( -8, -18),   S( -7, -13),   S(  4,   9),   S(  3,   5),   S(-15, -31),   S( -8, -21),   S( -7, -12),   S( -7,  -9),

            /* queens: bucket 13 */
            S( -8, -34),   S( -2, -10),   S(  0,   0),   S(  0,   0),   S( -6, -12),   S(-13, -21),   S( 12,  -4),   S( -3,  -7),
            S( -5, -28),   S(  0,  -5),   S(  0,   0),   S(  0,   0),   S( -7, -10),   S(-19, -42),   S( -6, -13),   S( -6, -16),
            S(  0,   1),   S(  1,  -2),   S( -1,  -3),   S(  7,  10),   S(-13, -36),   S( -5, -17),   S( -1,  -2),   S( -3,  -2),
            S(-10, -23),   S(-22, -58),   S(  8,   4),   S(-18, -43),   S( 26,  48),   S( 20,  41),   S(-10, -25),   S( -7, -10),
            S(  6,  -3),   S(-12, -30),   S( -5, -22),   S( 12,  21),   S(-15, -29),   S(  1,  -3),   S(-20, -44),   S( -5, -15),
            S( 20,  35),   S(-21, -51),   S( 15,  17),   S( 11,  20),   S( -8, -15),   S(-12, -18),   S(-12, -26),   S(-16, -44),
            S(  1,  -2),   S( -8, -21),   S( 11,  19),   S(  2,  -1),   S( -9, -27),   S(  4,  -1),   S(-19, -42),   S( -6, -21),
            S(-23, -49),   S(-14, -36),   S(-10, -18),   S(  3,   0),   S(  0,   4),   S( -2, -18),   S(  3,   0),   S( -8, -19),

            /* queens: bucket 14 */
            S( -7, -20),   S( 14,   5),   S(-12, -29),   S( 12,  15),   S(  0,   0),   S(  0,   0),   S(  5,  -5),   S( -6, -19),
            S(-14, -30),   S(-11, -37),   S(  3,   0),   S( -2, -14),   S(  0,   0),   S(  0,   0),   S(  6,  11),   S( -5, -12),
            S( -9, -20),   S( -7, -32),   S(  7,   5),   S(-14, -33),   S(  4,   7),   S(  8,  18),   S(-11, -29),   S(-18, -46),
            S(  2,   1),   S( -7, -19),   S( 13,  19),   S(-14, -32),   S(  0,   0),   S(-23, -54),   S(  6,  -1),   S( -6,  -8),
            S( -3,  -4),   S(  8,   4),   S(-12, -33),   S( 14,  26),   S(  8,  11),   S( -3,  -4),   S( -8, -18),   S( -1, -20),
            S(  1,   4),   S(  7,   5),   S(-10, -18),   S(-14, -41),   S(  5,   7),   S( -3,   3),   S(  2,  -3),   S(-17, -32),
            S(-23, -43),   S(-15, -25),   S( -5, -10),   S(  3,   4),   S(  2,   8),   S(  0, -10),   S(-12, -31),   S(  0,   0),
            S(-12, -34),   S(  3,  -2),   S(-17, -25),   S( -8, -15),   S(  6,  10),   S( -3,  -6),   S( -6, -15),   S( -7, -15),

            /* queens: bucket 15 */
            S( -5, -15),   S(-12, -29),   S(  8,   6),   S(-15, -27),   S(  9,   2),   S( -6, -17),   S(  0,   0),   S(  0,   0),
            S(  4,   6),   S(  0,  -7),   S( -2,  -3),   S(-10, -29),   S(  4,   4),   S(  3,   6),   S(  0,   0),   S(  0,   0),
            S( -2, -11),   S( -5, -13),   S(-10, -12),   S(  0,  -2),   S( -8, -25),   S(  1, -11),   S( -2,  -6),   S(  0,  -3),
            S( -1,   0),   S(-13, -28),   S(  5,   4),   S(  4,   3),   S( 11,  19),   S(  9,  21),   S(  5,   8),   S( -2, -10),
            S(  1,   2),   S( -2,  -9),   S( -1,  -2),   S(  5,  12),   S(  7,  31),   S( 11,  21),   S(  9,  18),   S( -5, -18),
            S(  5,   3),   S( -8, -17),   S(-18, -36),   S(-10, -20),   S(-10, -26),   S(-17, -38),   S(-10, -22),   S(-12, -25),
            S( -5,  -8),   S( -6, -11),   S(  2,  14),   S( -7, -20),   S( -6, -13),   S(  4,   1),   S(  1,  10),   S( -4, -13),
            S(-17, -31),   S(-20, -44),   S( -8, -26),   S( -2,   0),   S(-11, -18),   S(-13, -33),   S(  3,   7),   S( -9, -16),

            /* kings: bucket 0 */
            S( 22, -52),   S( 21,   3),   S( -2,   1),   S(-33,  11),   S(-18,  -2),   S( 28, -36),   S(  8,   8),   S( 15, -53),
            S(-27,  33),   S(-17,  17),   S( -4,   9),   S(-36,  11),   S(-42,  41),   S(-19,  26),   S( -6,  39),   S(-17,  32),
            S( 29,   3),   S( 51, -15),   S(  2,   3),   S(-23,  -4),   S(-16,   0),   S( -6,   0),   S(-52,  32),   S( -5, -21),
            S(-13, -33),   S( -3, -19),   S(-25, -10),   S(-25,  15),   S(-58,  27),   S(-29,   0),   S(-36,  19),   S(-12,  16),
            S(-41, -94),   S( 51, -32),   S( -1, -44),   S( 13,  -6),   S(-40,  -2),   S(-29,  15),   S( -3,   9),   S( 10, -19),
            S( -5, -93),   S( 24, -31),   S( 38, -63),   S( -2, -15),   S( 23, -28),   S( 11, -38),   S( 37,   2),   S(-20, -42),
            S(  0,   0),   S(  0,   0),   S( 10, -21),   S( 21, -34),   S( 19,  -6),   S(  8,  -7),   S( 12,  -3),   S(-20,  -8),
            S(  0,   0),   S(  0,   0),   S( -1, -66),   S(  9, -42),   S( 13, -15),   S(  4, -11),   S( 13,  10),   S(  1, -10),

            /* kings: bucket 1 */
            S(  9, -27),   S( 40, -19),   S(  5, -14),   S( 12,  -4),   S(-10,   1),   S( 29, -11),   S( 12,  13),   S( 24, -21),
            S(  8,  -3),   S( 15,  16),   S( 21,  -6),   S(-40,  25),   S(-31,  22),   S( -8,  14),   S(  3,  23),   S( -9,  22),
            S(-20,  -7),   S( 12,  -5),   S( 14, -22),   S( 25, -24),   S(-35,   3),   S( -3, -14),   S(  3,   3),   S( 53, -18),
            S( 11,  -6),   S( 16, -23),   S( 21,  -3),   S( -2,  11),   S( -9,  22),   S(-31,  10),   S(  1,   7),   S(-27,  26),
            S( -5, -29),   S( 12, -30),   S( 10, -39),   S( 27, -23),   S( 20, -12),   S(-26, -14),   S( 18,   0),   S(-19,  19),
            S(  6, -17),   S( 37, -32),   S( 26,  -2),   S( 30,  -6),   S( -1, -22),   S( 18,  -2),   S( 16,   9),   S(-14,  10),
            S(-21, -40),   S( 16,   9),   S(  0,   0),   S(  0,   0),   S(-14,   7),   S(  8,  33),   S(  1,  46),   S(-24, -46),
            S(-16, -123),  S(-14,  -8),   S(  0,   0),   S(  0,   0),   S(  0, -39),   S(  8, -24),   S( -4,  35),   S( -7, -56),

            /* kings: bucket 2 */
            S( 28, -65),   S( 18,   1),   S( 11, -18),   S( 23, -18),   S(-14,  12),   S( 34, -25),   S(  7,  23),   S( 28, -26),
            S( 13, -12),   S( -5,  33),   S(-17,   8),   S( -2,   5),   S(-21,  12),   S( -8,   6),   S( 17,   4),   S( -5,  10),
            S(-50,  -1),   S(-11,   1),   S( 14, -18),   S(-13, -18),   S( -6,  -3),   S(  5, -23),   S( 37, -20),   S( 17, -17),
            S( -1,  17),   S(-17,   7),   S(-22,  14),   S( -4,   5),   S( -2,   6),   S(-19,  -5),   S( 32, -19),   S( 30, -10),
            S(  5,  -8),   S( 21,  -6),   S(  1, -20),   S( 11,  -9),   S( 19, -38),   S(-11, -30),   S( 46, -37),   S(-10, -25),
            S(  5,   3),   S(  2, -14),   S( 27, -30),   S( 50, -28),   S( 65, -12),   S( 36,   0),   S( 64, -36),   S( 26, -22),
            S( -7,   7),   S(  0,  27),   S( -5,  -9),   S( 31,   2),   S(  0,   0),   S(  0,   0),   S( 27,  36),   S(-31, -37),
            S(-10, -30),   S(-13, -30),   S(-11, -36),   S( -3,   4),   S(  0,   0),   S(  0,   0),   S( -5, -25),   S(-14, -124),

            /* kings: bucket 3 */
            S(  9, -71),   S( 13,  -4),   S( 21, -38),   S(-17,  -5),   S(-14, -20),   S( 32, -28),   S(  3,  17),   S(  5, -30),
            S(-13,  25),   S(-20,  34),   S( -9,  -4),   S(-35,  14),   S(-54,  27),   S( -2,  -5),   S( -2,  15),   S(-16,  17),
            S( 20, -42),   S(  5,  -3),   S(-18,  -8),   S(-14, -20),   S(  7,  -7),   S( 37, -35),   S( 46, -23),   S( 53, -24),
            S(-22,  14),   S(-89,  26),   S(-51,  13),   S(-94,  25),   S(-73,  17),   S(-86,   4),   S(-79,   8),   S(-42, -15),
            S(-46,  14),   S(-27,   2),   S(-52, -10),   S(-73,  11),   S(-32, -28),   S( 16, -48),   S(-10, -44),   S(  4, -73),
            S(-23, -16),   S( 12, -20),   S(  7, -27),   S(-28, -15),   S( 30, -34),   S( 68, -63),   S(123, -32),   S( 38, -116),
            S(-29,   6),   S( 32, -11),   S(  3, -44),   S( -4, -26),   S( 26, -23),   S( 48, -39),   S(  0,   0),   S(  0,   0),
            S(-32, -55),   S( 18, -17),   S(  8,   7),   S(  5,   5),   S(-26, -103),  S( 11,   2),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-31,  -2),   S( 17,  18),   S( 15,  10),   S( 22,  13),   S( 28, -25),   S(-12,  19),   S( 11,  22),   S( 21, -11),
            S(-62,  21),   S( 10,  27),   S(-22,  10),   S(-33,  10),   S( 23,  -7),   S( 40, -10),   S( 25,   7),   S( 29, -13),
            S(  7,  17),   S( -9,  -3),   S(-11,   9),   S( -9,   1),   S( -4,   0),   S(  7, -20),   S(-14, -16),   S( -6,   9),
            S( -7, -12),   S(  9,  -2),   S( 14,  -3),   S( -7,  11),   S( 27,  -1),   S( -6,  10),   S( -8,  13),   S( -4,  21),
            S(  0,   0),   S(  0,   0),   S( 15, -26),   S(  6,   6),   S(-28,   6),   S(-25,   0),   S(-26,  -4),   S(-10,  -3),
            S(  0,   0),   S(  0,   0),   S(  6,   1),   S( 18,  13),   S( 12,  24),   S(  1, -45),   S( 13, -16),   S( 10,  12),
            S( -5, -13),   S(  0, -12),   S(  3, -44),   S( 14,   6),   S(  7,  22),   S(-16,  -3),   S( 15,   6),   S( -4, -15),
            S(  0,  54),   S(  1,   2),   S( -2,  -7),   S(  2,  18),   S(  6,   5),   S( -6, -13),   S( -2,  -7),   S( -7, -15),

            /* kings: bucket 5 */
            S( 34, -23),   S(  1,  12),   S(-61,  26),   S(-46,  22),   S(-38,  26),   S(-13,  12),   S( 50,  -2),   S( 23,  -1),
            S(-20,  11),   S( 53,  -5),   S( 23,  -3),   S( 19,  -3),   S( 33,  -4),   S( 39, -11),   S( 28,  10),   S( 76, -19),
            S(  6,  -6),   S(-37,   9),   S(-13,  -2),   S(-39,  -2),   S(-15,   4),   S(-65,   2),   S( -3,  -2),   S( 34, -14),
            S( -7,  -6),   S( 50, -16),   S( 34, -15),   S( 19,  16),   S( 44,   7),   S(  4,   2),   S(-15,  22),   S( 14,   0),
            S(-27, -14),   S(-20, -17),   S(  0,   0),   S(  0,   0),   S(-12,  10),   S(-14, -13),   S(  3,   4),   S(  1,  -9),
            S(-28,  -5),   S(-21,  -5),   S(  0,   0),   S(  0,   0),   S(  3,  12),   S(-34,   5),   S(-54,   9),   S(-12,  -2),
            S(-12,   1),   S( 12,  30),   S(  9,  52),   S(  0,  -6),   S( -8,  13),   S(-12,  -6),   S(  8,  19),   S( 10,  15),
            S(-14, -44),   S(  2,  12),   S(  1,  33),   S(  2,   6),   S(  6,  33),   S(  0,  30),   S( 10,  28),   S( -6,   8),

            /* kings: bucket 6 */
            S( 19, -32),   S(  6,  11),   S(  2,  -2),   S( 18,   2),   S( 11,   6),   S(-10,  16),   S( 22,  20),   S( 43,  -2),
            S( 52, -25),   S( 32,  15),   S( 12,  -5),   S( 43, -16),   S( 21,   0),   S( -2,   9),   S( 25,   7),   S( 12,   7),
            S(  2,  -7),   S(-17,   8),   S(  0,  -7),   S(  1, -12),   S(  5,  -8),   S(-51,   0),   S(-10,   4),   S(-29,  12),
            S(-14,  11),   S(  5,  -1),   S(  5,   0),   S( 31,   3),   S( 70,   1),   S(-22,   5),   S( 92, -21),   S( 14,  -8),
            S(-13, -15),   S(-21, -13),   S(  7, -15),   S( 10,  -5),   S(  0,   0),   S(  0,   0),   S(-20,  -9),   S(-59, -12),
            S(-19,  10),   S(-13,  22),   S(-28,  -4),   S(-23,   1),   S(  0,   0),   S(  0,   0),   S(-29,  25),   S(-40, -12),
            S(  2, -19),   S(  7,  15),   S(  2,   5),   S(-13,  18),   S(  4,  21),   S( -4, -10),   S(-26,  -5),   S(-18, -19),
            S(  0,  31),   S( -7,  13),   S(  0,   2),   S(  0,   2),   S(  5,  12),   S( -7,   1),   S(  3,  33),   S(  3,  39),

            /* kings: bucket 7 */
            S( 11, -18),   S( -4,   4),   S( -9, -19),   S(-16,  10),   S(-46,  18),   S(-53,  36),   S(-12,  35),   S(-15,  14),
            S( 64, -20),   S( 36, -16),   S(  7,  -6),   S(-13,  -2),   S(  4,   2),   S(-22,  14),   S( 13,   2),   S(  3,  16),
            S( 42, -16),   S(-22,  -4),   S(-17,  -3),   S(-39,  -4),   S(-53,   9),   S(-63,  15),   S( -7,   7),   S(-53,  14),
            S( -5,   0),   S( 17,  -7),   S( 18,  -3),   S(  9,   4),   S(-16,  10),   S( 69, -24),   S( 34,  -7),   S( 30, -16),
            S( -1,  -3),   S( -4,  -2),   S( -4, -19),   S(-37,   7),   S( -4, -18),   S( 23, -27),   S(  0,   0),   S(  0,   0),
            S(  0, -39),   S( 14,  -5),   S( 25,  -9),   S( -6,   3),   S(  0,  -6),   S( 11,   5),   S(  0,   0),   S(  0,   0),
            S( 16,  34),   S( 27, -10),   S( 16,  -4),   S(-13, -17),   S( 19, -28),   S(-20,   2),   S(  9,   0),   S( -4,  -7),
            S(  7,  21),   S(-14, -66),   S( 27,  11),   S(  6, -14),   S( 15,  20),   S( -7, -25),   S(  2,  17),   S( -3,  -7),

            /* kings: bucket 8 */
            S(-27,  91),   S(-41,  72),   S(-26,  58),   S(-11,  -6),   S(-22,  -3),   S( 10,   8),   S( 30, -11),   S( 22,  -3),
            S( 17,  81),   S( 21,   6),   S(-15,  49),   S(  2,   5),   S(  7,  12),   S(-29,   8),   S( -6,  13),   S( 37,  21),
            S(  0,   0),   S(  0,   0),   S( 21,  31),   S( 37,   9),   S( 24,  -6),   S( 15,  -5),   S( -8,  22),   S( -9,   6),
            S(  0,   0),   S(  0,   0),   S(  7,  30),   S( 29, -29),   S(  6,  12),   S( 21,  -4),   S( 18,   0),   S(-12,  21),
            S( -3, -24),   S(  2,   6),   S(  8,   0),   S( 14, -20),   S(  6, -28),   S( 12, -15),   S(  6,  20),   S(-19, -22),
            S(  5,  14),   S( -4,   4),   S(  3,   5),   S(  0, -26),   S( -6,  -9),   S(-11,   4),   S( -8,  -8),   S(  7,  -5),
            S( -4, -13),   S(  3, -13),   S( 15,  25),   S(  1, -24),   S( -8, -61),   S(  2,   1),   S(  8, -13),   S( 16, -68),
            S( -1,  -5),   S(-13, -54),   S( -4, -29),   S( -3, -10),   S(  9,  42),   S( -3,  -5),   S(  8,   0),   S( -1, -17),

            /* kings: bucket 9 */
            S(-34,  55),   S(-42,  28),   S(-56,  45),   S(-65,  34),   S(-83,  41),   S(-49,  22),   S(101,   2),   S( 30,  19),
            S(-14,  31),   S( 43,  17),   S( -5,  -5),   S( 31,   6),   S( 36,  16),   S(  7,   7),   S(  7,  18),   S(  9,   0),
            S(-24,  10),   S(  4,  15),   S(  0,   0),   S(  0,   0),   S( 11,  23),   S(-17,  -3),   S( 28,  -6),   S( -7,   0),
            S(-17, -15),   S( -7,  -7),   S(  0,   0),   S(  0,   0),   S( -3,  12),   S( 44,  -5),   S( 12,  12),   S(-11,  13),
            S(  4,  -1),   S(-15,   2),   S( -3,  28),   S( -3, -36),   S(  3,  -5),   S( -6,   7),   S( 11,  18),   S(-12, -22),
            S( -3,  30),   S(-10,  11),   S(  4,  16),   S(  2,  -7),   S(-14,  -4),   S(  2,  21),   S(-12, -22),   S( -7,  43),
            S(  5,  -2),   S( -4, -15),   S(  5,   9),   S( -3,   2),   S( -4,  24),   S( 20,  16),   S(  1, -15),   S(  8,  25),
            S( 12,  49),   S( -5,  23),   S(  9,   4),   S(  4, -22),   S(-13, -48),   S(  2,   9),   S( -8, -29),   S(  6,  28),

            /* kings: bucket 10 */
            S( -7,  37),   S(-12,  13),   S( -1,   1),   S(-40,  39),   S(-81,  25),   S(-114,  52),  S(  3,  38),   S(-68,  87),
            S( 27,  -4),   S( 27,   7),   S( 11,  -5),   S( 20,  13),   S( 66,   9),   S( 30,   5),   S( 19,  17),   S(-34,  39),
            S( 18,  11),   S(  8,   1),   S( 15,  -7),   S(  2,   4),   S(  0,   0),   S(  0,   0),   S( 28,  11),   S(-58,  23),
            S( 11,  11),   S(  2,  -8),   S( 18,  -1),   S( 24,  -8),   S(  0,   0),   S(  0,   0),   S(  6,  15),   S( -4,  -3),
            S( -6,   0),   S( 19,  13),   S( 10,   0),   S( 12, -29),   S( 11, -15),   S(  3,  19),   S( -2,   8),   S(-26,  24),
            S( -6,  27),   S( -8,   6),   S(-10,   9),   S(  2,   1),   S(-10,   1),   S( -4,  -6),   S(-15,  17),   S( -1,   7),
            S( -4, -47),   S( 10,  -5),   S( 13, -13),   S( 16,  34),   S( 10,   7),   S(  2,  -1),   S( 13, -12),   S(  3,  26),
            S(  7,  15),   S(  8,  12),   S(  2,  -9),   S( -8,  -2),   S(  5,  -2),   S( -6, -48),   S(  0, -14),   S( -1,   5),

            /* kings: bucket 11 */
            S(-27,  47),   S( -2,   7),   S(  2, -23),   S(-23,   0),   S(-18,  -3),   S(-160,  73),  S(-76,  77),   S(-144, 153),
            S( 15, -22),   S(-11,  18),   S(  0, -29),   S( 19,  13),   S( 40,   5),   S( 20,  43),   S( 56,  -4),   S( 17,  32),
            S( -9,  -8),   S( 23,   2),   S( -4,  -2),   S( 20,  -3),   S( 54,  -1),   S( 31,  21),   S(  0,   0),   S(  0,   0),
            S( -2,   4),   S(  0,  14),   S( 18,   5),   S( 29,  -3),   S( 44, -19),   S( 18,   7),   S(  0,   0),   S(  0,   0),
            S(  8,   4),   S( -6,   0),   S(  6,  -5),   S( 13,  -2),   S( 32, -16),   S(  4,  -9),   S(  0, -13),   S(  8,  10),
            S(  4,  12),   S(  8,   0),   S(  3,  -9),   S(  0,  -1),   S( 12, -34),   S( -5,   1),   S(-11,   2),   S(  3,   1),
            S( 13, -19),   S(  6,   4),   S( -8,  20),   S( -1, -35),   S( 13,   0),   S(  0,  -8),   S(  3, -17),   S(-13, -13),
            S(  3,   0),   S(  7,  -8),   S(-11, -18),   S(  8,   8),   S(-10, -20),   S(  1, -11),   S(  0, -25),   S(  8,  28),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  6,  81),   S( -5,   1),   S( -8,   7),   S(  6,  -2),   S( 13, -29),   S(  9,  57),
            S(  0,   0),   S(  0,   0),   S( 26,  94),   S(  8, -11),   S( 10,  12),   S( 22,   7),   S( 12,  23),   S(-24,  25),
            S( -4, -12),   S( -2, -51),   S( 16,  12),   S(  3,  26),   S(  7, -16),   S( -4,   5),   S(-15,  -6),   S(-18,   6),
            S( -2,   7),   S( 12,  25),   S( -3, -16),   S( -3, -22),   S( -5, -20),   S(  0, -11),   S( -4,  16),   S( -1,   7),
            S(  7,  32),   S(  5,  33),   S(  7,  11),   S(  2, -19),   S( -4,  22),   S( -6,  -3),   S(-19, -10),   S(  5,  -4),
            S(  1,  11),   S(  2,   8),   S( -6, -14),   S(-10, -35),   S( -9, -16),   S( -3,  25),   S( -8,  14),   S( -6,  22),
            S(  4,  14),   S( -6, -23),   S( -1,  -2),   S(  1,   4),   S( -2, -14),   S(  0, -10),   S(  7,  18),   S( -1, -22),
            S( -1,   4),   S(  2,   5),   S(  2, -20),   S( -1, -11),   S( -1, -16),   S(  6,   9),   S( -3, -20),   S( -6, -28),

            /* kings: bucket 13 */
            S( -5,  78),   S(-14,  59),   S(  0,   0),   S(  0,   0),   S(  0,  80),   S(-29,  -5),   S( 17,  -7),   S( 13,  63),
            S(-18,  24),   S( -2,  -1),   S(  0,   0),   S(  0,   0),   S( 25,  -6),   S( -9, -27),   S(-25,  -1),   S(-15,  10),
            S( -6,  -4),   S(  5,  48),   S( -6, -38),   S( -4,  24),   S(  2,  21),   S(-11,  -5),   S(-15,  18),   S(  5,  19),
            S(-12, -13),   S( -4,  -6),   S(  1, -28),   S( -2, -43),   S(  1, -35),   S( 19,  -9),   S( -6,  -4),   S(-21,  -9),
            S(  1,  18),   S( -3,  -5),   S(  7,  24),   S( -9, -10),   S(-14, -11),   S( -3,  -1),   S( -3, -11),   S(  0,  13),
            S(  1,   6),   S( -8,  17),   S(  0,  15),   S(-13, -23),   S( -4,  -6),   S( -3,   8),   S(  8, -13),   S(  7,  35),
            S(  4,   9),   S(-11, -19),   S(-11, -25),   S(  1,   3),   S(  6,   6),   S( -4, -21),   S( -8, -50),   S(  6,  26),
            S( -1, -13),   S(  7,  52),   S(  6,  21),   S(  4,   0),   S(  2,   9),   S(-12, -25),   S( -5, -24),   S( 11,  41),

            /* kings: bucket 14 */
            S(  2,  53),   S( -1,  12),   S(-15,  -4),   S( -8,   7),   S(  0,   0),   S(  0,   0),   S( -9,  81),   S(-51,  67),
            S(-19,   0),   S(-25, -25),   S(  6,   8),   S( 14,   7),   S(  0,   0),   S(  0,   0),   S( 19,   8),   S(-20,  10),
            S( -7,   5),   S(  6, -16),   S(  9, -15),   S( 10,   5),   S(  2, -37),   S(  5,  48),   S(  6,  41),   S(-15,   3),
            S(  2, -23),   S(  9,  22),   S( -6, -29),   S(  2, -30),   S(-15, -46),   S( 10,  12),   S( -2, -23),   S(  4,  10),
            S( 18,  48),   S( -3,  14),   S( -6,   6),   S( -8, -24),   S( -5,  20),   S( -1,  25),   S(  0,  38),   S( 14,  36),
            S( -9, -41),   S( -5,   3),   S(  6,  -2),   S(  3,  20),   S( -6,  12),   S(  0, -14),   S(-13, -34),   S( -5,  -4),
            S(  4,  13),   S(-12, -51),   S(  3,   5),   S(  4,   7),   S( -4, -18),   S( -2, -20),   S( -6, -68),   S( 11,  67),
            S(  2,  21),   S(  3,  48),   S(  2,  17),   S( -1,   2),   S(  3,  38),   S(  4,  -1),   S(-14, -52),   S(  0, -14),

            /* kings: bucket 15 */
            S( 13,  59),   S(-12, -30),   S( 13,  10),   S(  3, -21),   S(-22,   9),   S(-17,  59),   S(  0,   0),   S(  0,   0),
            S( -7, -28),   S(  0,  10),   S( -2, -21),   S( 20,  48),   S( 26, -10),   S( 35,  94),   S(  0,   0),   S(  0,   0),
            S(-20, -19),   S(  4, -17),   S( -6, -21),   S(-10,  -7),   S(  2, -30),   S( 20,  21),   S(  8,  -2),   S(-15, -39),
            S( 12,  34),   S(  2,  32),   S( -1,  -2),   S( -5, -40),   S( 10, -47),   S(  6,  10),   S(  6,  55),   S( -3, -18),
            S(  2,  16),   S(-27,  21),   S( -8,  -1),   S(-10, -26),   S( -1, -17),   S(-10,  13),   S( -4,  -4),   S(  2,  12),
            S(  6,  44),   S(-12,  19),   S(  4,  -7),   S(  7,  25),   S( -6, -11),   S(  3,  10),   S( -3,   6),   S(  6,  13),
            S(  3,  24),   S(  0,  27),   S( -5,  -4),   S(  6,  15),   S(  3, -12),   S( -1,  18),   S( -4,  -9),   S( -1,   8),
            S( -2,  -7),   S(  3,   8),   S( -3,  17),   S( -1, -18),   S(  0,  21),   S(  7,  41),   S(  2,  10),   S(  3,  -8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-14, -189),  S(  8, -34),   S( 17,  41),   S( 26,  77),   S( 33,  98),   S( 37, 118),   S( 45, 116),   S( 55, 102),
            S( 83,  66),

            /* bishop mobility */
            S(  3, -65),   S(  7,   9),   S( 16,  51),   S( 23,  90),   S( 28, 115),   S( 29, 136),   S( 31, 146),   S( 32, 146),
            S( 33, 151),   S( 41, 143),   S( 44, 137),   S( 69, 126),   S(106, 115),   S(112, 101),

            /* rook mobility */
            S(-23,  -6),   S(-14,  53),   S(-14,  84),   S( -9, 100),   S(-10, 124),   S( -3, 131),   S(  1, 143),   S( 10, 141),
            S( 14, 149),   S( 21, 152),   S( 30, 155),   S( 41, 152),   S( 55, 146),   S( 65, 140),   S(155,  92),

            /* queen mobility */
            S( 49, -74),   S( 48, -37),   S( 44,  65),   S( 40, 122),   S( 40, 169),   S( 40, 209),   S( 39, 246),   S( 39, 269),
            S( 39, 293),   S( 39, 310),   S( 38, 328),   S( 32, 350),   S( 33, 360),   S( 30, 372),   S( 32, 379),   S( 29, 387),
            S( 25, 389),   S( 43, 372),   S( 56, 365),   S( 71, 344),   S( 86, 332),   S(139, 294),   S(151, 279),   S(204, 242),
            S(189, 246),   S(178, 250),   S(126, 227),   S( 97, 191),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,  32),   S(-20,  60),   S(-27,  55),   S(-17,  55),   S( 16,  28),   S(-21,  42),   S(-14,  80),   S( 34,  33),
            S( 15,  39),   S(  0,  63),   S(-13,  50),   S(-10,  49),   S( -1,  46),   S(-23,  58),   S(-34,  81),   S( 39,  35),
            S( 38,  66),   S( 23,  72),   S( 12,  55),   S( 34,  45),   S(  6,  53),   S(-15,  64),   S(-38, 108),   S( 13,  69),
            S( 53,  89),   S( 60, 102),   S( 47,  61),   S( 24,  52),   S( 18,  53),   S( 26,  78),   S(-16, 107),   S(-46, 122),
            S( 76, 124),   S(104, 144),   S(100,  98),   S( 53,  98),   S(-20,  86),   S( 67,  82),   S( 12, 149),   S(-34, 136),
            S(153, 176),   S(143, 209),   S(175, 193),   S(190, 183),   S(195, 195),   S(186, 203),   S(170, 200),   S(196, 203),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  17),   S(  3,   1),   S(  7,   6),   S(  7,  36),   S( 21,  43),   S( -2, -27),   S(-19,  37),   S(  9, -32),
            S( -8,  37),   S( 30,   3),   S( -3,  54),   S( 24,  31),   S( 35,  10),   S(  3,  28),   S( 16,   5),   S( 11,   4),
            S( -7,  22),   S( 18,   9),   S( -6,  58),   S( 21,  67),   S( 24,  34),   S( 30,  24),   S( 26,   8),   S(  3,  18),
            S(  7,  31),   S( 31,  52),   S( 33,  79),   S( 20,  94),   S( 66,  67),   S( 78,  52),   S( 29,  44),   S( 18,  34),
            S( 65,  81),   S( 94, 108),   S( 80, 134),   S(123, 138),   S(160, 131),   S(154, 140),   S(151,  85),   S( 92,  42),
            S( 72, 143),   S(106, 228),   S(111, 232),   S(106, 206),   S( 55, 140),   S( 51, 144),   S( 45, 163),   S( 18,  96),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  49),   S( 27,  35),   S( 40,  58),   S( 40,  38),   S( 30,  40),   S( 30,  38),   S(  7,  22),   S( 45,  14),
            S(  0,  24),   S( 16,  45),   S( 14,  39),   S( 12,  50),   S( 24,  18),   S( 10,  27),   S( 27,  24),   S( -1,  14),
            S(  0,   8),   S( 23,  30),   S( 55,  38),   S( 41,  31),   S( 40,  44),   S( 61,   7),   S( 22,  19),   S( 18,   4),
            S( 63,  42),   S(120,   2),   S(140,  71),   S(168,  81),   S(163,  75),   S( 93,  77),   S( 82,  16),   S( 93, -15),
            S( 93,  51),   S(161,  30),   S(180, 167),   S(167, 152),   S(148, 171),   S(128, 157),   S(182, 120),   S(-29, 129),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 27,  23),   S( 15,  15),   S( 15,  30),   S( -1,  55),   S( 62,  37),   S( 24,   5),   S( -3,  -4),   S( 35,   8),
            S(  4,  11),   S(  8,   6),   S( 20,  17),   S( 17,  28),   S( 16,  14),   S(  1,   8),   S(  4,   6),   S( 30,  -7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -4, -11),   S( -8,  -6),   S(-20, -17),   S(-17, -28),   S(-16, -14),   S( -1,  -8),   S( -4,  -6),   S(-30,   7),
            S(-27, -23),   S(-15, -15),   S(-15, -30),   S(  1, -55),   S(-62, -37),   S(-24,  -5),   S(  3,   4),   S(-35,  -8),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-14, -23),   S( -8, -14),   S(  0, -29),   S(-56,  -2),   S(-13, -16),   S(-18, -24),   S( -4, -23),   S(-15, -36),
            S(-18,   2),   S(-10,  -2),   S(-26,  15),   S(  7, -22),   S(-31, -11),   S(-23,  -5),   S(-44,   4),   S(-10, -15),
            S(-14, -19),   S( -5, -19),   S(-25,  -1),   S(-27, -11),   S(-25, -30),   S(-23, -10),   S(-23,   0),   S(-39, -20),
            S(  1, -34),   S( 22, -40),   S( 19, -23),   S(  9, -29),   S(  3, -15),   S( 61, -36),   S( 45, -36),   S( -9, -45),
            S( 24, -56),   S( 57, -75),   S( 58, -41),   S( 68, -41),   S( 55, -26),   S( 77, -22),   S(109, -63),   S( 28, -63),
            S(136, -116),  S(184, -134),  S(141, -79),   S(123, -54),   S( 87, -26),   S(157, -60),   S(151, -68),   S( 67, -85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 16,   2),        // attacks to squares 1 from king
            S( 18,   1),        // attacks to squares 2 from king

            /* castling available */
            S( 69, -69),        // king-side castling available
            S( 17,  45),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S(  8,  63),   S( 12,  70),   S(  1,  69),   S(-12,  87),   S(-22,  96),   S(-38, 105),   S(-72, 123),   S(-81, 128),
            S(-94, 130),   S(-80, 137),   S(-69, 139),   S( 34, 122),   S( 10, 128),

            /* orthogonal lines */
            S(  4, -58),   S(-53, -19),   S(-73, -11),   S(-86, -10),   S(-94, -14),   S(-95, -20),   S(-94, -25),   S(-92, -28),
            S(-112, -20),  S(-130, -18),  S(-149, -25),  S(-144, -34),  S(-180, -19),  S(-370,  45),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S(  6, 219),

            /* passed pawn can advance */
            S(-17,  39),   S(-18,  72),   S(  3, 106),   S( 30, 197),

            /* blocked passed pawn */
            S(  0,   0),   S( 61, -12),   S( 54, -11),   S( 54,  24),   S( 62,  38),   S( 74,  -6),   S(171, -22),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 59, -23),   S( 29,  49),   S( 18,  44),   S( 21,  67),   S( 55,  71),   S(116, 112),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-48,  14),   S(-32,  -4),   S( 15, -31),   S(  3, -37),   S( 29, -49),   S(203, -82),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S(  2, -22),   S( 42, -29),   S( 31, -22),   S( 31, -49),   S( 36, -146),  S(-26, -244),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 14,  44),   S(  8,  39),   S(127,  38),   S( 20,  30),   S(229,  75),   S(150,  44),   S(  0,   0),       // blocked by King

            #endregion

            /* tempo bonus for side to move */
            S(  8,  11),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
