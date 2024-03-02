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
        public const int MAX_WEIGHTS = 12717;
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
        public const int TEMPO = 12716;             // tempo bonus for side moving

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

        // Solution sample size: 6000001, generated on Sat, 02 Mar 2024 01:27:04 GMT
        // Solution K: 0.003850, error: 0.086215, accuracy: 0.4943
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 91, 174),   S(418, 528),   S(403, 627),   S(561, 956),   S(1284, 1709), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 91, -109),  S(144, -92),   S( 29, -34),   S(-62,  49),   S(-38,  28),   S(-18,  -9),   S(-51,   0),   S(-47, -16),
            S( 99, -113),  S( 94, -96),   S( -2, -56),   S(-24, -47),   S(-24, -19),   S(-17, -39),   S(-40, -25),   S(-39, -36),
            S( 82, -75),   S( 70, -56),   S( 19, -56),   S(  3, -55),   S( -3, -59),   S(  3, -50),   S(-18, -46),   S(-27, -36),
            S( 59, -26),   S( 56, -47),   S( 22, -57),   S( 10, -57),   S( -6, -40),   S(-29, -41),   S(-24, -37),   S(-40, -15),
            S( 84,  11),   S( -4,  29),   S( 33, -22),   S( 49, -85),   S( 20, -42),   S( -6, -58),   S(-50,   1),   S(-48,  57),
            S( 33,  27),   S( 52,  33),   S(-23, -12),   S(  6, -82),   S(-46, -21),   S(-19, -15),   S(  5, -38),   S(-72,  44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 29, -30),   S( 31, -42),   S( 45, -31),   S( -6,   4),   S( -9, -12),   S(  9, -16),   S(-38, -13),   S(-38,  19),
            S( 20, -35),   S( 14, -45),   S( 10, -45),   S( -4, -48),   S(-12, -25),   S(-10, -32),   S(-34, -21),   S(-45, -14),
            S( 14, -23),   S( 17, -27),   S( 23, -44),   S( 14, -49),   S(-16, -21),   S( 10, -38),   S(-13, -28),   S(-19, -13),
            S( 14,   5),   S( 23, -37),   S( 25, -38),   S( 11, -53),   S( -4, -19),   S(  0, -30),   S(-28, -18),   S(-32,  16),
            S( 20,  46),   S(-17, -19),   S(-26,  -8),   S( -6, -27),   S( 26, -14),   S(-21,  -3),   S(-14,  -6),   S(-21,  62),
            S( 32,  51),   S(-39,   3),   S(-41, -42),   S(-37,  12),   S(-64,  -3),   S(-103,  13),  S(-63,   4),   S(-81,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23,   1),   S(-34,   4),   S(-15,  -3),   S(-11,   6),   S( 10, -13),   S( 32, -20),   S(  4, -42),   S(-19, -12),
            S(-24, -16),   S(-49,  -6),   S(-30, -36),   S(-19, -32),   S(  7, -30),   S(  1, -25),   S( -9, -34),   S(-34, -19),
            S(-37,  -2),   S(-27, -17),   S(-10, -44),   S( -3, -50),   S(  0, -23),   S( 14, -22),   S( -1, -28),   S(-19,  -9),
            S(-48,  17),   S(-24, -24),   S(-28, -28),   S( -7, -44),   S( 15, -40),   S(-12, -10),   S( -8,  -8),   S(-28,  17),
            S(-46,  50),   S(-67,   7),   S(-58, -29),   S(-56, -18),   S( 15,  -4),   S( -3,  15),   S(-30,   8),   S(-36,  89),
            S(-94,  76),   S(-132,  67),  S(-140,  16),  S(-88, -28),   S(-56,   3),   S(-15, -11),   S(-66,  14),   S(-83,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30,  -6),   S(-40,   1),   S(-29,  -9),   S( -5, -37),   S(-13,   2),   S( 32, -11),   S( 81, -62),   S( 60, -77),
            S(-38, -28),   S(-50, -18),   S(-34, -42),   S(-19, -32),   S(-22, -17),   S(  0, -31),   S( 55, -61),   S( 52, -73),
            S(-43, -20),   S(-18, -41),   S( -7, -58),   S( -4, -62),   S(-11, -41),   S(  8, -33),   S( 32, -46),   S( 58, -54),
            S(-47,  -3),   S(-16, -49),   S(-16, -54),   S(  4, -68),   S( 18, -63),   S( -4, -39),   S( 21, -31),   S( 49, -15),
            S(-27,  37),   S(-23, -20),   S(-21, -52),   S(-12, -48),   S( 79, -45),   S( 52, -39),   S( 49,  19),   S( 51,  71),
            S(-100, 120),  S(-69,  35),   S(-43, -47),   S(-68, -41),   S(-22, -37),   S( 26, -15),   S( 72,   4),   S(  2,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-70,   8),   S( 24, -28),   S(-28,   3),   S(  0,  14),   S(-36, -21),   S(-36,   3),   S(-68,   3),   S(-70,   3),
            S(-15,  -3),   S( 17,  -6),   S( 37, -35),   S( 19, -21),   S(  7, -31),   S(-42, -21),   S(-15, -37),   S(-15, -25),
            S( 28, -11),   S( 87, -19),   S(  9,   2),   S( 11, -20),   S(-25, -31),   S(-11, -39),   S(-49, -20),   S(-28, -10),
            S( 13,  35),   S( 12,  34),   S( 34,  14),   S( 18,  -2),   S( 15, -32),   S(-46, -10),   S( -5, -38),   S( 27, -22),
            S(-18,  82),   S( -9,  63),   S( 34,  -5),   S(  2,  24),   S( 25, -31),   S(-26,  -3),   S( -7, -57),   S( 72,  18),
            S( 77,  71),   S( 57,  71),   S( 60,   5),   S( -2, -14),   S( -2, -46),   S( -5,   0),   S(  5,   2),   S(-39,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-73,  42),   S(-67,  31),   S(-17,  17),   S(  4,  -6),   S(-32,  29),   S(-28,  16),   S(-34,   1),   S(-36,  22),
            S(-56,  18),   S(-57,  16),   S( 28, -21),   S(  7,   9),   S( 16, -11),   S(-10, -18),   S(-14, -15),   S(-60,  17),
            S(-46,  38),   S(-27,  21),   S( 76, -35),   S( 20, -26),   S( 44, -15),   S(-48,   3),   S( -3,  -5),   S(-26,   7),
            S(-21,  50),   S(-25,  34),   S( 21,   8),   S( 65,   6),   S(  0,   8),   S(-56,  13),   S( 14,  -8),   S(-21,  28),
            S( 54,  57),   S( 55,  35),   S( 77,  20),   S( 52,  19),   S(-18,  51),   S( 35,   7),   S(  8,   8),   S(  0,  56),
            S(102,  33),   S( 67,  21),   S( 39,   2),   S( 52,  -5),   S( 41,  -9),   S(  3,  -7),   S( 16,   9),   S( 55,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39,  25),   S(-29,   5),   S(  0,   0),   S( -6,  -1),   S( 52, -27),   S(-26,   6),   S(-57,   3),   S(-67,  21),
            S(-37,  -3),   S( -4, -22),   S(-10, -37),   S(  7,  -9),   S( 46, -18),   S( 42, -27),   S(-32,  -8),   S(-77,  13),
            S(-40,  14),   S(  5,  -9),   S( -2, -20),   S(-26,  -5),   S( 28,  -9),   S( 76, -33),   S( -1, -10),   S(-46,  21),
            S(-26,  22),   S(-46,  14),   S(  5, -10),   S( 19, -13),   S( 39,   4),   S( 53,  -6),   S( 32,  -1),   S( 27,  14),
            S(-45,  51),   S( -2,  -2),   S(-15,  -8),   S( -8,  -1),   S( 43,  31),   S( 68,  28),   S( 61,  -2),   S( 29,  42),
            S( 59,  33),   S( 16,   7),   S( -2, -30),   S( 52, -70),   S( 61, -22),   S( 27, -32),   S( 15,  -1),   S( 84,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -20),   S(-57, -10),   S( -3, -24),   S(-31, -15),   S(-15, -16),   S( 33, -27),   S( -1, -43),   S(-50, -18),
            S(-51, -31),   S(-52, -32),   S(-31, -46),   S( -7, -48),   S(  4, -31),   S( 55, -47),   S( 50, -41),   S(-28, -27),
            S(-64, -23),   S(-53, -31),   S(-20, -50),   S( -2, -48),   S( -2, -25),   S( 29, -23),   S( 50, -40),   S( 32, -32),
            S(-52, -13),   S(-60, -29),   S(-64, -37),   S(-25, -23),   S( 20, -30),   S( 15,  -5),   S( 34,   0),   S( 83, -20),
            S(-30, -10),   S(-13, -44),   S(-33, -36),   S(-16, -34),   S( 67, -10),   S( 11,   4),   S( 83,  35),   S( 98,  50),
            S(-13,  -3),   S(-10, -34),   S( 24, -70),   S( 22, -73),   S( 12, -51),   S( 33, -29),   S( 21,  57),   S(105,  44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-81,  41),   S(-31,  17),   S(-17,  26),   S(-15, -18),   S( 11,  23),   S(-30,  17),   S(-61,   6),   S(-37,   4),
            S(-47,  23),   S(-59,  34),   S(-43,  18),   S(-26,   2),   S(-46,   1),   S(-35, -18),   S(-38, -21),   S( -3,  -7),
            S( 18,  60),   S(  2,  61),   S( -6,  44),   S(-42,  42),   S(-25, -18),   S(-36, -33),   S(-29, -48),   S(-10, -25),
            S( 37,  91),   S( 44, 118),   S( 83,  63),   S( 17,  50),   S( 11,  -2),   S(-54,  -9),   S(  6, -16),   S(-17, -15),
            S( 50, 114),   S( 68, 104),   S( 35, 112),   S( 64, 108),   S( 18, -17),   S( 12, -11),   S(  0, -30),   S( 10,  -3),
            S( 88, 123),   S(101, 128),   S( 90, 182),   S( 50,  83),   S(  4,  11),   S( -7, -15),   S(-14, -58),   S( 12, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94,  52),   S(-44,  38),   S(-20,   4),   S( 21,  33),   S(-15, -10),   S(-88,  27),   S(-78,  13),   S(-88,  31),
            S(-52,  16),   S(-51,  18),   S(-53,  23),   S(  7,  18),   S(-89,  18),   S(-52,  -7),   S(-127,   4),  S(-45,  12),
            S(-64,  49),   S(-38,  70),   S( -4,  62),   S(-55,  58),   S(-58,  54),   S(-76,   9),   S(-40, -19),   S(-46,   1),
            S(  0,  79),   S( 42,  88),   S( 58, 120),   S( 78, 119),   S(-49,  67),   S(-49,  24),   S( 46,  -3),   S( 45, -12),
            S( 84,  72),   S( 61, 101),   S( 66, 140),   S( 96, 133),   S( 81, 102),   S( 50,  41),   S( 50, -12),   S( 38, -16),
            S( 40,  60),   S( 68,  81),   S(113,  88),   S(101, 115),   S( 52,  64),   S(  5, -28),   S( 13, -11),   S(  5, -11),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-103,  17),  S(-91,  -6),   S(  1, -11),   S(  4,  22),   S(-41,  -2),   S(-109,  45),  S(-129,  28),  S(-82,  39),
            S(-102,   0),  S(-37, -21),   S(-37, -17),   S(-53,   9),   S(-49,  39),   S(-24,  19),   S(-126,  31),  S(-72,  18),
            S(-20,  -4),   S(-62,   4),   S(-57,  14),   S(-70,  60),   S(-70,  90),   S(  7,  41),   S(-80,  35),   S(-70,  49),
            S( -1,   0),   S( 10,  10),   S( 27,  14),   S(  8,  69),   S( 36,  92),   S(  4,  85),   S( 11,  42),   S( 37,  24),
            S( 63,  -4),   S( 31,  -6),   S( 31,  34),   S( 84, 115),   S( 83, 114),   S(100,  67),   S( 28,  64),   S( 79,  39),
            S( 83,  10),   S( 20,   0),   S( 36,  30),   S( 74,  90),   S( 46,  69),   S( 37,  41),   S( 38,  43),   S( 58,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-117,   8),  S(-79,  12),   S(  2,  -1),   S(-19, -14),   S( -9,  15),   S(-83,  48),   S(-35,  18),   S(-82,  46),
            S(-95, -23),   S(-62, -33),   S(-59, -36),   S(-88,  16),   S(-72,   9),   S(-13,  10),   S(-116,  58),  S(-95,  44),
            S(-17, -26),   S(-27, -50),   S(-54,   4),   S(-29,   6),   S(-61,  43),   S(  0,  54),   S(-44,  66),   S(-41,  64),
            S( 30, -15),   S(-46, -33),   S(-17,  -1),   S(-26,  15),   S( -2,  46),   S( 60,  50),   S( 34, 108),   S(125,  57),
            S( 16,  11),   S(-27, -23),   S( 13,  -1),   S(-12,   1),   S( 56,  92),   S( 61,  78),   S( 99, 108),   S(160,  77),
            S( 10, -10),   S(  4, -51),   S(  9, -34),   S(  7, -18),   S( 43,  35),   S( 62, 121),   S( 59, 129),   S(113,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6,   2),   S(-26, -24),   S(-12,   0),   S(  9,  24),   S( -8, -11),   S(-15,  -3),   S(-21,   5),   S(-42,  -2),
            S(-30,   4),   S( 13,  11),   S( 11,  36),   S( -2, -20),   S( -8,  46),   S(-21,   4),   S(-36, -30),   S( -2, -57),
            S(-25,  67),   S( -7,  57),   S( 25,  56),   S( 13,  28),   S(-13,   3),   S(-48, -39),   S(-46, -57),   S(-29, -63),
            S(-14,  79),   S( 10,  89),   S( 48,  85),   S( 40,  65),   S(-33, -52),   S(-15, -47),   S( 13, -14),   S(-44, -79),
            S( 83,  79),   S( 83, 202),   S( 68, 123),   S( 25,  61),   S( -2, -12),   S( -4, -19),   S(  5, -12),   S(-10, -73),
            S( 56, 123),   S( 54, 279),   S(116, 254),   S( 50,  97),   S( -1,   0),   S( -7, -57),   S( -8, -50),   S(-11, -77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24, -14),   S( -8,   1),   S(  7,  23),   S(  0,   2),   S( -6,  -6),   S(-16, -27),   S(-32, -41),   S(-21, -22),
            S( -8, -41),   S(-23, -20),   S(-24, -48),   S(  8,   6),   S(-44,   7),   S( -4, -11),   S(-51, -45),   S(-21, -30),
            S( 32,   6),   S( 16, -21),   S( -4,  31),   S( 21,  72),   S(  0,  37),   S(-29, -30),   S(-50, -34),   S(-13, -56),
            S( 17,  31),   S( 51,  38),   S( 45,  87),   S( 38, 105),   S( 43,  51),   S( 17, -25),   S(  2, -15),   S( 12, -56),
            S( 32,  48),   S( 74, 136),   S( 97, 169),   S( 96, 182),   S( 54, 109),   S(-19, -21),   S( 15, -42),   S( 10, -68),
            S( 39,  73),   S( 97, 153),   S( 82, 214),   S(113, 271),   S( 56, 133),   S( 16,  31),   S(  7, -15),   S( 20, -24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-51, -52),   S(-28, -14),   S( -4, -19),   S(  2,  14),   S( -6, -15),   S(-45,  26),   S(-43,  -4),   S( -5,  37),
            S(-38,  -9),   S(-32, -30),   S(-38, -31),   S(  2,  20),   S(  4,  42),   S( -8,  -5),   S(-25,   9),   S(-22,  -4),
            S(  2, -40),   S(-29, -19),   S(-30, -30),   S( -3,  30),   S( -4,  30),   S(  9,  12),   S( 10,  -6),   S(  7,  23),
            S( 18, -43),   S( 18, -12),   S( 13,  -8),   S( 19,  56),   S( -1, 139),   S( 15,  67),   S(  3,  22),   S( 47,  23),
            S( 13, -30),   S( 17,  -3),   S( 36,  35),   S( 39, 123),   S( 81, 210),   S( 89, 146),   S( 56,  61),   S( 33,  41),
            S( 32, -13),   S( 38,  17),   S( 44,  82),   S( 60, 174),   S( 85, 260),   S( 54, 153),   S( 36, 118),   S( 38,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -63),   S(-26, -35),   S(-24, -50),   S( -2,  -6),   S(  9,  26),   S(  4,  33),   S(-20, -21),   S( 19,  52),
            S(  8, -65),   S(-35, -25),   S(-24, -42),   S(  1,   1),   S(-33,  10),   S(  6,  48),   S( 22,  35),   S( -3,  25),
            S(-14, -73),   S( -9, -87),   S(-14, -17),   S( 17,  23),   S( 26,  30),   S( 34,  20),   S( 27,  49),   S(  8,  74),
            S( 19, -39),   S(-35, -44),   S(-20, -49),   S(  8,   9),   S(  4,  61),   S( 54,  63),   S( 20, 119),   S( 14,  69),
            S(-23, -95),   S(-11, -83),   S( -7, -19),   S(  5, -30),   S( 31, 101),   S( 92, 113),   S( 48, 268),   S(104,  92),
            S( 10, -24),   S(  4,   6),   S(  2, -11),   S( 10,  16),   S( 36,  83),   S( 64, 257),   S( 35, 225),   S( 29,  96),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-15, -11),   S( -2,  -7),   S(-48,  27),   S(-50,  15),   S(-42, -14),   S(-16, -31),   S(-49, -44),   S(-11, -23),
            S(-33,  66),   S( 20, -24),   S(-51,  17),   S(  0, -13),   S( -4, -42),   S(-20, -17),   S(-41, -36),   S(-81, -21),
            S(  9,  34),   S(-14, -29),   S(  7, -22),   S(-17,  19),   S(  7,   4),   S(-34,   3),   S(  2, -57),   S(-37, -52),
            S( 32, -41),   S( 30,  -3),   S( 23,  19),   S( 14,  37),   S( 12,  -2),   S( -7,  18),   S( 12, -42),   S(-16,  -4),
            S(  8, -17),   S( 40, -11),   S( 25, -30),   S( 58, -10),   S( 37, -12),   S( 23,  24),   S( 12,  -6),   S(-70, -10),
            S( -2, -27),   S(  5, -21),   S( 81, -21),   S( 50,  -5),   S( 60, -79),   S( 18,  -5),   S( 10, -30),   S( -4, -39),
            S(-19, -25),   S( 31, -82),   S( 14, -34),   S( 25, -56),   S( 49, -61),   S( 17, -72),   S(-20, -36),   S( -5, -16),
            S(-44, -49),   S(-21, -27),   S( -2,  -9),   S( -1, -39),   S(-12, -27),   S( -5,  -5),   S(  3,  17),   S( 18,   9),

            /* knights: bucket 1 */
            S(-43,  40),   S(-56,  76),   S( 20,  41),   S(-14,  41),   S( -4,  28),   S(-29,  22),   S(-37,  54),   S(-62,  15),
            S( 47,   3),   S( 31,  14),   S( -5,  14),   S(-11,  29),   S( -2,  12),   S( -7,  12),   S( 10, -26),   S(-33,  10),
            S(-39,  35),   S( -4,  11),   S(  1,  13),   S( -2,  32),   S( 10,  19),   S(-26,  17),   S(-18,   5),   S(-21,  11),
            S( -3,  45),   S( 62,  17),   S( 21,  45),   S( 19,  20),   S(  9,  25),   S(-15,  32),   S( 48, -16),   S( -7,  39),
            S(-10,  35),   S( 15,   0),   S( 32,  17),   S( 39,  20),   S( 27,  33),   S( 20,  27),   S( 13,   5),   S(-10,  36),
            S( 10,   9),   S( 44, -21),   S( 54,  24),   S( 44,  10),   S( 28,  27),   S( 71,  37),   S( 36, -14),   S( 25, -13),
            S( 61,  15),   S(-17,  16),   S(  2, -38),   S( 18,  27),   S( -3,   7),   S( 50, -10),   S(-19,  -4),   S(-10, -15),
            S(-83, -56),   S(-23, -51),   S( -2,  -2),   S(-25, -36),   S( -2,  18),   S( -3,  10),   S(-19, -34),   S(-52, -68),

            /* knights: bucket 2 */
            S(-51,  14),   S( -7,  28),   S(-22,  34),   S(-37,  61),   S(-17,  47),   S(-39,  63),   S(-14,  35),   S(-56,   6),
            S(-17, -16),   S(-23,  11),   S( -9,  15),   S(-16,  35),   S( -8,  12),   S( -6,  44),   S(-42,  57),   S(-45,  65),
            S(-33,  23),   S( -2,  16),   S(-17,  33),   S(  9,  21),   S(  1,  23),   S( -5,  16),   S(-13,  37),   S(-20,  28),
            S(-13,  44),   S(-21,  42),   S(  2,  51),   S( -3,  47),   S( -5,  59),   S( -7,  39),   S(-12,  46),   S( -4,  48),
            S( 20,  28),   S(-25,  42),   S(-23,  57),   S(-23,  59),   S( -9,  51),   S(-13,  47),   S(-13,  38),   S(-29,  14),
            S( 26,   5),   S( 10,  22),   S(-24,  48),   S(-12,  39),   S(-28,  61),   S( -1,  46),   S(-46,  11),   S( 43, -23),
            S(  6,   5),   S(-55,  27),   S(-40,  24),   S(-12,  28),   S( -3,  10),   S( 24, -17),   S(-67,  47),   S(-38, -17),
            S(-178,   2),  S( -1,   6),   S(-78,  26),   S( -7,   2),   S(  9, -13),   S(-91,  -5),   S( 18,   5),   S(-190, -54),

            /* knights: bucket 3 */
            S(-79,   6),   S( -8, -10),   S(-53,  27),   S(-21,  17),   S(-14,  24),   S(-22,  25),   S(  2,   8),   S(-25, -14),
            S(-32,  36),   S(-36,  15),   S(-23,   7),   S( -1,  26),   S(  3,  13),   S(-19,   8),   S(-19,   8),   S(-38,  75),
            S(-20, -22),   S( -7,  10),   S(-13,  18),   S( 10,  18),   S(  8,  33),   S(  5,  16),   S(-12,  15),   S( -3,  48),
            S(-16,  16),   S(  3,  28),   S( -4,  58),   S(  5,  52),   S( 14,  54),   S( 11,  43),   S( 11,  46),   S(  7,  37),
            S( 15,  18),   S(-11,  38),   S(  6,  34),   S( 10,  60),   S(  1,  64),   S( 10,  68),   S( 17,  52),   S(-12,  33),
            S(  5,   4),   S( 22,   1),   S( 34,  17),   S( 48,  18),   S( 65,   1),   S( 65,   3),   S(  1,  19),   S(-18,  61),
            S(  0,  -4),   S(  8,   7),   S( 65, -41),   S( 46,   8),   S( 61, -18),   S( 45, -26),   S( 24, -50),   S( 21,  -3),
            S(-153,  24),  S(-30,  15),   S( -7,   8),   S(-24,  26),   S( 33, -24),   S(  0, -19),   S( 22,  -3),   S(-80, -23),

            /* knights: bucket 4 */
            S(  9,  10),   S(-40, -32),   S( 34,  20),   S( -1, -19),   S(-11, -22),   S(-16, -27),   S( -1, -48),   S(-26, -55),
            S( 28,  38),   S(-19,  15),   S( 16, -24),   S( 32, -33),   S(-12, -26),   S(-16, -52),   S( 26, -26),   S( 15, -43),
            S(-11,  24),   S( 25,  16),   S( 30,  -9),   S( 49,  -5),   S( 37, -13),   S(-35,  13),   S(-33, -57),   S(  3, -62),
            S(  7,  39),   S( 32, -24),   S( 98,  -9),   S( 50,   5),   S( 11,  18),   S(108, -33),   S( 14, -30),   S(  5, -37),
            S( 98,  31),   S(-25,  47),   S( 42,  39),   S( 35,   7),   S( 41,  24),   S(-26, -10),   S( -9, -65),   S( 23, -16),
            S( -7,  19),   S(-32, -20),   S(109,  14),   S( 35,  -7),   S( 42,   8),   S( 28,   7),   S( 12,  23),   S(-23, -33),
            S(-13,  10),   S(-32, -10),   S( -6,  -9),   S( 12,   3),   S(  8, -13),   S( 36, -10),   S( 11, -29),   S(-15,  -3),
            S(-10,  13),   S(  3, -10),   S(  7,  15),   S(  1,  -8),   S(  2,  -5),   S( 15,  31),   S(  1,  22),   S(  2, -11),

            /* knights: bucket 5 */
            S( 20,  12),   S(-34,  23),   S( 43,  22),   S(  4,  38),   S( 37,   4),   S( 24,   5),   S(-40,  10),   S(-15, -20),
            S( -1,   4),   S( 13,  27),   S( 40,   3),   S( 24,  15),   S( 40,   7),   S(  4,  29),   S( 31,  -6),   S(-36, -38),
            S( 25,  30),   S(-43,  52),   S( 56,   8),   S( 79,   5),   S( -3,  31),   S( 24,  14),   S(-32,   8),   S(  8,   8),
            S( 67,  25),   S( 22,  21),   S( 60,  21),   S(  0,  37),   S( 65,  17),   S( 65,  22),   S( 43,  26),   S( 22,  36),
            S( 18,  39),   S( 53,  15),   S( 66,  37),   S( 55,  35),   S(113,  21),   S( 34,  35),   S( 40,  32),   S( 28,  16),
            S(-11,  25),   S( -5,  52),   S( 18,  30),   S(  9,  59),   S( 50,  48),   S( 31,  42),   S(  5,   9),   S(  1,  30),
            S( 26,  59),   S(-19,  29),   S( 68,  32),   S( 23,  72),   S(-10,  32),   S( 26,  42),   S( 20,  41),   S( 12,   5),
            S(  0,   3),   S( -6,  -5),   S(  2,  23),   S(  4,  -8),   S(  5,  19),   S(-17,  11),   S( 17,  34),   S(-40, -54),

            /* knights: bucket 6 */
            S( -2, -32),   S( 34, -11),   S( 62,  14),   S(-29,  24),   S(-32,  24),   S( 27,  32),   S(-32,   9),   S(-17,  32),
            S(  9, -53),   S( 77, -17),   S( 47,   2),   S( -6,  23),   S(-33,  53),   S( 97,  17),   S( 26,  37),   S( 20,  35),
            S( 26, -15),   S( 29,  -6),   S( 21,  15),   S( 37,  17),   S( 44,  12),   S(-41,  50),   S( 34,  35),   S(-10,  49),
            S( 27,   2),   S( 63,   5),   S( 90,  13),   S( 97,   3),   S( 36,  27),   S( 67,  23),   S( 29,  44),   S(  6,  53),
            S( -3,  28),   S( 59,  -3),   S( 82,  12),   S(116,  17),   S(142,  16),   S(126,  22),   S( 30,  53),   S( 37,  26),
            S( 29,   5),   S( 29,  13),   S( 33,  34),   S( 75,  23),   S( 75,  40),   S( 68,  15),   S( 40,  19),   S( 44,  42),
            S( 15,   3),   S( 18,   7),   S(-14,  22),   S( -9,  50),   S( 10,  43),   S( 46,  32),   S(  4,  56),   S( -7,  12),
            S(-47, -41),   S( 27,  55),   S( 17,  11),   S(  5,  47),   S( 18,  39),   S( 15,  33),   S( 23,  38),   S(  5,   7),

            /* knights: bucket 7 */
            S(-21, -45),   S(-188,  -3),  S(-86, -47),   S(-61, -14),   S(-30,  -3),   S(-13, -12),   S(-32,   0),   S(-24, -22),
            S(-50, -113),  S(-29, -55),   S(-40, -40),   S(  3, -34),   S(-28, -11),   S( 24, -10),   S( 42,  12),   S( 50,  29),
            S(-85, -45),   S(-65, -26),   S(-17,  -5),   S( 59, -34),   S( 36, -21),   S( 38, -15),   S( 12,  27),   S( 22,  42),
            S(-61, -17),   S( 63, -41),   S( 19,  -6),   S( 83, -15),   S( 83,  -7),   S( 12,   5),   S( 17,  20),   S(-26,  41),
            S(-74, -22),   S( 11, -43),   S(105, -47),   S(103, -16),   S(137, -12),   S(103,  11),   S(110,   2),   S( 81,   4),
            S( 11, -46),   S( 40, -37),   S( 30, -17),   S( 60, -11),   S( 76,  -4),   S(119,  -5),   S( 73, -24),   S(-20,  46),
            S(-15, -43),   S(-43, -23),   S(  9, -25),   S( 60,   9),   S( 36,   3),   S( 58,   3),   S( -9,  11),   S(  9, -18),
            S(-37, -12),   S(  7,  26),   S(-33, -35),   S( 27,  12),   S( -2,   7),   S( 26,   1),   S( -5, -31),   S(-12, -21),

            /* knights: bucket 8 */
            S( -2,   5),   S( -2,   5),   S( -1,  13),   S( -5, -23),   S(-10, -21),   S(-10, -44),   S(-11, -10),   S( -2, -25),
            S(  7,  19),   S( -7,   1),   S(-15, -42),   S(-22, -24),   S(-23,  13),   S(-14, -67),   S(-15, -61),   S(-16, -44),
            S(  2,  25),   S( -8, -22),   S( 18,  25),   S(-20, -18),   S( -4, -12),   S(-12,  34),   S(  0,  -3),   S( -2, -45),
            S(-12,  -3),   S(  4,  -5),   S(-14,   5),   S( 12,  31),   S( 28,   5),   S( 12, -10),   S(-12, -48),   S(-10, -20),
            S( 37,  85),   S(  0,   6),   S( 21,  53),   S( 20,  34),   S(  4,  36),   S(  2,  21),   S( 13,  -4),   S( -9,  -6),
            S( 16,  60),   S( 14,  18),   S( 39,  15),   S( 40,   4),   S(-16, -13),   S(-10,  -5),   S(-13, -61),   S(-12, -18),
            S(  7,  29),   S( 10,  42),   S( 14,   0),   S( 15,  18),   S(  7, -20),   S(  8,  22),   S( -2,   4),   S( -8, -13),
            S(  1,  -3),   S( 11,  27),   S( -4, -10),   S(  4,  26),   S(  7,  23),   S( -4, -20),   S( -2,  -7),   S(  1,   8),

            /* knights: bucket 9 */
            S( -7, -38),   S(-29, -52),   S(-20, -59),   S(  1,  -4),   S(-19, -55),   S(-28, -74),   S( -9, -27),   S(  0,  -2),
            S(-12, -53),   S(  0,  11),   S( -7, -58),   S(-27, -15),   S(  3, -15),   S( -2, -31),   S( -5, -28),   S(-14, -45),
            S( 24,  53),   S(  0,   0),   S( -3, -14),   S( 17,   4),   S( 11,  10),   S(-19,   8),   S(-12,   1),   S(-11, -18),
            S( -9,  -2),   S( -9, -18),   S(  3,   6),   S( 25,  -6),   S( 42,   1),   S( 16,  28),   S(-16, -59),   S( -3, -44),
            S(  3,  -1),   S( 13,  -5),   S( 23,  10),   S(  1,  15),   S( 27, -11),   S( -4,  -3),   S( 19,  -5),   S(  5,   7),
            S(  4,   9),   S( 19,  31),   S( 15,  23),   S( 17, -21),   S( 22,  10),   S(  1,   5),   S(  1,  18),   S(-20, -51),
            S(  2,  11),   S( -6,   2),   S( 11,  34),   S( 10,  40),   S( 15,  59),   S( -8, -26),   S(  3,  36),   S(  5,  21),
            S(  2,   6),   S(  7,  44),   S( 13,  37),   S( 14,  57),   S( 10,  15),   S(  5,  34),   S( -6,  -2),   S(  0,  -8),

            /* knights: bucket 10 */
            S(-17, -51),   S(-25, -88),   S(-12, -62),   S(-24, -28),   S( -7,  -4),   S(-22, -70),   S( -8,  -8),   S(  8,  17),
            S( -7, -27),   S( -3, -42),   S(-14, -33),   S(-12, -38),   S(-12, -79),   S( -6, -33),   S( -3,   1),   S(  0, -10),
            S(-16, -59),   S(-18, -74),   S(-17,   9),   S(-23, -33),   S( 11,  -5),   S(-23, -23),   S( -5, -14),   S(-11,  -8),
            S(-12, -28),   S( -8, -49),   S(-11, -31),   S( 21,  -3),   S(  9,   2),   S( 20,   2),   S(  3,   5),   S( 17,  45),
            S( -6, -33),   S( -7,  -8),   S( 16,  17),   S( 38,   2),   S( 19,   8),   S( 11,   0),   S( 18,  24),   S( 11,  37),
            S( -5, -13),   S(  1,  -8),   S(-22, -16),   S( 28,  12),   S( 18,  38),   S( 17,  41),   S( 30,  73),   S( 16,  77),
            S( 11,  23),   S(-13, -21),   S(  1,  -4),   S( -3,  20),   S(  9,  28),   S(  7,  24),   S(  3,   7),   S(  9,  23),
            S( -6, -26),   S(  3,   5),   S( -8, -10),   S(  0,   9),   S(  6,  30),   S(  5,  17),   S(  8,  12),   S(  1,   7),

            /* knights: bucket 11 */
            S( -1,  -1),   S(-12, -32),   S(-19, -54),   S(-19, -32),   S(-32, -55),   S( -7, -25),   S(  5,   9),   S( -5,  -7),
            S(-17, -28),   S(-22, -59),   S(-21, -73),   S(-37, -41),   S(-18, -20),   S(-25, -28),   S(-18, -29),   S( -6,  -7),
            S(-17, -60),   S(-20, -45),   S(-22, -22),   S(  2,  -5),   S(-31,  15),   S(-31,  16),   S( 19,   3),   S(-18, -28),
            S(-25, -47),   S( -2, -15),   S(-10,   4),   S( 20,  34),   S( 34,  28),   S(  5,  35),   S(-16,  36),   S( -3,  17),
            S(-15, -46),   S(-37, -78),   S(  8,  -3),   S( 17,  13),   S( 27,  24),   S( 47,  42),   S(-16, -33),   S( 16,  53),
            S( -9, -19),   S(-18, -41),   S(  2,  18),   S( 37,  29),   S( 16,  31),   S( 55,  58),   S( 32,  65),   S( 10,  51),
            S(  6,  47),   S( -6,  -6),   S( -6, -10),   S( -1,   6),   S( 10,  24),   S(  0,  24),   S( 13,  54),   S(  9,  63),
            S( -4,  -4),   S( -8, -36),   S(  9,  13),   S(  8,  22),   S( -8,   4),   S(  3,   3),   S(  6,  17),   S(  3,  22),

            /* knights: bucket 12 */
            S( -1,  -3),   S( -3,  -1),   S( -3, -28),   S( -1,   8),   S( -8, -16),   S( -2,   4),   S(  2,  -6),   S( -1, -11),
            S( -1,  -2),   S(  2,   7),   S(  8,  36),   S( -7, -24),   S( -4, -10),   S( -7, -33),   S( -4, -20),   S(  0,  -8),
            S( -4, -18),   S(  4,   8),   S( -8,  -7),   S(  2,   6),   S(  0,  -9),   S( -5, -23),   S( -3, -14),   S( -9, -28),
            S(-10, -27),   S(-10, -20),   S(-10, -35),   S( 11,  63),   S( -3,  -9),   S( -2,  -5),   S( -2,  10),   S( -1, -18),
            S( 17,  49),   S( -7, -35),   S( -3, -13),   S(  8,  25),   S( -8, -29),   S(  4,   4),   S( -1, -21),   S(  5,  17),
            S( -6,  -3),   S( -3, -20),   S(  1,  37),   S(  2,   2),   S(  4, -12),   S( -4, -19),   S(  0,   3),   S(  1,   3),
            S(  5,  17),   S(-10,  -5),   S( -5,  -9),   S(-15, -24),   S( -3,  -8),   S( -5, -21),   S( -1,  -2),   S( -1,   0),
            S(  3,  14),   S(  5,  28),   S(  5,  11),   S(  6,  13),   S( -6, -21),   S( -4, -19),   S( -4, -16),   S(  1,   5),

            /* knights: bucket 13 */
            S( -5, -23),   S( -2, -11),   S(  0,   0),   S( -3, -11),   S(-14, -33),   S( -1,  -8),   S(  0,  -2),   S(  0,   2),
            S( -1, -10),   S(  1,   9),   S(  2, -16),   S( -4, -18),   S( -5, -34),   S(  2, -12),   S(  2,   4),   S(  1, -15),
            S( -6, -30),   S( -6, -11),   S(  8,  34),   S(  1,  -3),   S( -4,  12),   S( -4,  -5),   S( -3,   3),   S( -5, -21),
            S(-12, -17),   S( 11,  28),   S( -1,  20),   S(-11, -34),   S(  3,  -1),   S(  3,  -7),   S(  1,  -4),   S( -5, -17),
            S(  4,   7),   S(  4,  36),   S( 23,  37),   S( 11,  27),   S(  4,  17),   S( -4,   4),   S( -3,  -3),   S(  1,  -1),
            S(  5,  34),   S( 10,  24),   S( 12,  87),   S(-10,  13),   S( -6,   9),   S(-12, -48),   S(  3,   1),   S( -7, -23),
            S(  3,  10),   S(  0,  17),   S(  8,  11),   S(  5,  30),   S( 13,  55),   S(  0,  22),   S( -5,  -2),   S( -2,  -1),
            S(  0,   0),   S(  2,  39),   S(  7,  26),   S(  2,  15),   S(  1,  26),   S(  4,  16),   S( -1,  -5),   S( -1,  -3),

            /* knights: bucket 14 */
            S( -2, -14),   S( -5, -34),   S(  1,   4),   S( -3,  -3),   S(-14, -56),   S( -5, -33),   S( -3,  -9),   S(  0,  -4),
            S(  0,  -6),   S( -7, -34),   S(-15, -65),   S( -1, -22),   S(  7,   4),   S( -1, -13),   S(  2,   8),   S(  1,   2),
            S( -2,  -5),   S( -4, -16),   S(-23, -74),   S( 10,  28),   S(  2, -11),   S(  4,   8),   S( -1,  -7),   S(  1,   2),
            S(  4,  21),   S( -1, -13),   S( -7, -30),   S( -3,  -9),   S(  2, -15),   S(  0, -26),   S(  1,  11),   S( -5,  -8),
            S( -4,  -9),   S( -3, -31),   S(  7,  32),   S(  3,  -1),   S(-12,   5),   S(  0, -25),   S( -4,  -8),   S( -1,  10),
            S( -2, -12),   S(  5,   8),   S( -4,   2),   S( -2, -13),   S( 12,  26),   S(-10,  11),   S(  0,   5),   S(  3,  18),
            S(  0,  -5),   S( -1,  -7),   S(  8,  20),   S(  6,  20),   S( -2,  23),   S( -1,  26),   S( -7,  -3),   S(  3,  18),
            S(  0,   0),   S(  2,  10),   S(  1,   8),   S(  3,  14),   S(  2,   9),   S(  2,  22),   S(  4,  31),   S(  0,   4),

            /* knights: bucket 15 */
            S( -4, -11),   S(  2,  -5),   S( -1, -22),   S( -6, -18),   S(  0,   6),   S(  1,  -5),   S(  1,  -7),   S(  0,  -2),
            S( -1, -12),   S( -1,  -8),   S( -8, -18),   S( -5, -25),   S(  1,   1),   S(  1,   0),   S(  0,  -3),   S(  2,   6),
            S( -7, -22),   S( -5, -22),   S( -4, -37),   S(-13, -48),   S(  4,  -9),   S( -3, -11),   S(  2,   4),   S( -4, -14),
            S( -6, -23),   S( -6, -33),   S( -6, -22),   S( -7,  -2),   S(-10, -30),   S( 12,  46),   S( -2, -16),   S(  1,  -4),
            S( -4, -13),   S( -7,  -9),   S(  3, -19),   S( -9, -42),   S(  6,   4),   S( 22,  23),   S( -7, -10),   S( -3,  -5),
            S(  0,   5),   S( -6, -12),   S(  0,  -2),   S( -1,   6),   S( -3,  -7),   S(  5,  25),   S(  1,   8),   S(  1,  36),
            S( -7, -20),   S( -3,  -6),   S( -1,  -9),   S( -7, -16),   S( -3, -11),   S( -3,   2),   S( -3, -30),   S(  6,  29),
            S( -1,  -8),   S(  2,  11),   S( -3,  -6),   S( -5, -15),   S( -1,  -8),   S( -2,  -7),   S(  2,  12),   S( -1,  -1),

            /* bishops: bucket 0 */
            S( 59,  66),   S( -1, -17),   S( 42,  27),   S(-16,  31),   S( -9,  14),   S(  5, -12),   S( -1, -25),   S( 16, -34),
            S( 36, -21),   S( 88,  21),   S( 29,  12),   S( 18, -13),   S(  1,  29),   S( -1,   1),   S(-28,  10),   S(-10,  -8),
            S( 24,  23),   S( 31,  14),   S( 31,  37),   S( -3,  57),   S( 12,  29),   S(-21,  61),   S( -8,   3),   S( 13, -51),
            S( 27,  -5),   S( 46,  35),   S( 34,  26),   S( 38,  34),   S(  0,  48),   S( 39,  10),   S( -5,  14),   S( -9,  22),
            S(  2,  18),   S( 47, -13),   S(-10,  55),   S( 50,  45),   S( 45,  21),   S(-15,  39),   S(  8,  17),   S(-75,  45),
            S(-48,  74),   S(-22,  63),   S( 41,  51),   S( 35,  46),   S( 32,  59),   S(-34,  22),   S(-50,  37),   S(-31,  71),
            S(-52,  63),   S(-12,  34),   S( 52,  32),   S(-24,  81),   S(-64,  26),   S( 23,  12),   S(  5,  17),   S(-38,   7),
            S(-13,   4),   S( -9,  59),   S(-10,  64),   S(-17,  41),   S( 16,  43),   S(  2,  29),   S(-30,  40),   S(-13,  28),

            /* bishops: bucket 1 */
            S( 31,  97),   S( -7,  52),   S( 14,  38),   S( 15,  23),   S(  8,   9),   S(  0,  33),   S(-18,  42),   S(-60,  47),
            S( 10,  17),   S( 43,  22),   S( 49,  16),   S( 30,  32),   S(  6,  18),   S( 27,  10),   S(-19,  26),   S( 28,  16),
            S( 49,  26),   S(  5,  34),   S( 49,  42),   S(  8,  38),   S( 17,  45),   S( -5,  45),   S( 27,  20),   S( -4,  -9),
            S( 41,  12),   S( 34,  31),   S( 14,  28),   S( 53,  28),   S(  1,  50),   S( 24,  30),   S( -1,  44),   S( 24,   1),
            S( 11,  51),   S( 16,  35),   S( 30,  35),   S( 14,  40),   S( 15,  32),   S(-16,  48),   S( 31,   4),   S(-15,  21),
            S(-56,  57),   S( 55,  39),   S( 40,  63),   S( 54,  29),   S(-25,  59),   S( 10,  49),   S(-11,  58),   S( 39,  25),
            S( 24,  62),   S( 23,  32),   S( 27,  34),   S( 10,  73),   S( 67,  32),   S(-40,  48),   S(  9,  54),   S(-43,  69),
            S( 17,  72),   S(-29,  49),   S(-41,  22),   S(-29,  48),   S(  0,  39),   S( -3,  53),   S(-12,  61),   S(-28, 100),

            /* bishops: bucket 2 */
            S( 23,  32),   S(  3,  51),   S(  2,  40),   S(-27,  63),   S(-11,  38),   S(-14,  38),   S(-16, -10),   S(-58,  70),
            S( -3,  46),   S( 14,  36),   S( 27,  28),   S(  2,  37),   S(  6,  41),   S( 14,  18),   S( 11,  13),   S( 15, -15),
            S(  0,  18),   S( -4,  52),   S(  9,  52),   S( -3,  65),   S(  1,  52),   S( 10,  55),   S( -7,  53),   S(-25,  18),
            S(  4,  48),   S(-28,  55),   S( -5,  63),   S(  2,  63),   S(  9,  55),   S(  1,  45),   S( 27,  41),   S(  1,   4),
            S( -4,  47),   S( -3,  49),   S(-28,  56),   S(-21,  61),   S(-21,  68),   S(  0,  61),   S( -3,  47),   S(-13,  27),
            S(  3,  35),   S(-18,  50),   S(  3,  61),   S(-14,  58),   S( 41,  33),   S( -9,  76),   S( 20,  68),   S(  2,  47),
            S(-24,  47),   S(-13,  54),   S(-43,  85),   S( -1,  39),   S(-29,  70),   S(-19,  48),   S(-44,  47),   S(-27,  62),
            S(-92, 106),   S(-94,  99),   S(-81,  81),   S(-89,  68),   S(-58,  67),   S(-80,  78),   S(-18,  55),   S(-74,  59),

            /* bishops: bucket 3 */
            S( -1,  27),   S(  9,  30),   S( 12,  28),   S( -4,  36),   S(  4,  21),   S( 37,   3),   S( 22,  -5),   S( 36, -28),
            S(  5,  22),   S(  6,  35),   S( 25,  22),   S(  2,  46),   S( 15,  33),   S(  4,  33),   S( 49,  28),   S( 25,   9),
            S( 20,  14),   S( -3,  53),   S(  2,  62),   S( 12,  52),   S(  2,  74),   S( 16,  63),   S( 16,  37),   S( 35,   0),
            S( 10,  27),   S( -4,  50),   S(  4,  68),   S( 12,  68),   S( 19,  63),   S( 15,  33),   S( 10,  54),   S(  6,  14),
            S( -4,  35),   S( 20,  37),   S( 11,  43),   S( 17,  69),   S(  6,  63),   S( 33,  47),   S( -2,  43),   S( 18,  38),
            S(  4,  45),   S( 14,  44),   S( 25,  57),   S( 25,  32),   S( 16,  58),   S( 34,  58),   S( 32,  38),   S(  1,  60),
            S( -1,  39),   S( -9,  61),   S( 29,  41),   S( -3,  46),   S( -4,  47),   S(-19,  51),   S(  3,  62),   S(  4,  70),
            S(-63, 116),   S(-31,  49),   S( 35,  42),   S( -7,  58),   S(-31,  52),   S(-53,  83),   S(  1,  42),   S( 51,  21),

            /* bishops: bucket 4 */
            S(-28,  -2),   S(-27,  10),   S(-49,  -8),   S(-32,   1),   S(-41,  14),   S(-26,   9),   S(-10,  -4),   S(  8,  27),
            S(-14,  -7),   S( 19,  -1),   S(-32,  26),   S(-30,  10),   S(-16,  16),   S( 47, -16),   S(-12,   3),   S( 14, -18),
            S(-32,  21),   S(-34,  19),   S(-15,  17),   S( -2,  15),   S( 27,   2),   S( 34,  19),   S(-16, -18),   S(-57,  10),
            S( -6,  37),   S(-23,  27),   S( 36,  19),   S( 60,  13),   S(-20,  31),   S( 32,   2),   S(  8,  50),   S(  7,   7),
            S( 37,  30),   S(-39,  51),   S(-27,  55),   S(-11,  38),   S( -9,  16),   S(-12,   9),   S(-27,  14),   S(  9,  37),
            S(  0,  25),   S( 53,   5),   S(-30,  37),   S( -1,  12),   S(  5,  36),   S(  3,  27),   S(-19,  -6),   S( -1,   3),
            S(-39,  31),   S( 28,  37),   S(  0,  41),   S( -2,  37),   S(-10,   6),   S(-23,  18),   S( -7,  -4),   S( -5,  -4),
            S(  4,  34),   S( -9, -11),   S( -8,   7),   S(-24,   3),   S(-15,  15),   S( -1,   8),   S(-17, -19),   S(-18,   0),

            /* bishops: bucket 5 */
            S(-15,  55),   S(-36,  49),   S(-52,  48),   S( 11,  20),   S(-49,  39),   S( 11,  14),   S( -1,  36),   S(-23,  46),
            S(-17,  52),   S(-40,  58),   S(-65,  73),   S( 30,  16),   S( -8,  41),   S(-21,  36),   S(-39,  31),   S(-36,  12),
            S(-34,  43),   S(-29,  63),   S(-11,  52),   S(  7,  32),   S(  5,  38),   S(-18,  40),   S(-15,  36),   S(-29,  13),
            S( 37,  28),   S( 45,  31),   S(-32,  62),   S( 46,  21),   S( 41,  39),   S( 11,  26),   S(-39,  31),   S( -5,  33),
            S( 51,  32),   S( 55,  23),   S( 70,  29),   S( 85,  34),   S( 44,  19),   S( 55,  19),   S( 50,  21),   S(-22,  23),
            S( 48,  41),   S( 46,  45),   S( 66,  42),   S( 33,  35),   S(-15,  59),   S( 31,  15),   S(  6,  34),   S( 11,  44),
            S( -5,  59),   S(-44,  44),   S(-14,  26),   S( 25,  48),   S( 12,  45),   S( 27,  46),   S( 19,  36),   S( -1,  12),
            S(-12,  55),   S(  6,  36),   S(  4,  30),   S( -4,  38),   S( 10,  39),   S(  3,  43),   S( 14, 104),   S( -6,  34),

            /* bishops: bucket 6 */
            S(-55,  76),   S(  3,  51),   S(-11,  44),   S(-65,  46),   S(-24,  35),   S(-33,  34),   S(-10,  59),   S( -9,  46),
            S( 25,  56),   S(  5,  35),   S( 16,  37),   S( 26,  37),   S( -9,  45),   S( -2,  30),   S(-97,  69),   S( 13,  54),
            S( 20,  35),   S(-34,  47),   S( 32,  44),   S( 34,  32),   S( 83,   9),   S( 29,  38),   S(-33,  60),   S(-57,  45),
            S( 12,  42),   S(  5,  47),   S( 13,  43),   S( 50,  37),   S( 35,  37),   S( 49,  20),   S(-18,  70),   S(-27,  20),
            S(-42,  67),   S( -2,  44),   S( 75,  21),   S( 42,  35),   S(105,  37),   S( 24,  38),   S( 52,  34),   S(  0,  40),
            S(-11,  31),   S( 24,  28),   S( 26,  40),   S( 42,  52),   S( 15,  50),   S( 60,  48),   S( 61,  39),   S(-32,  65),
            S(-33,  17),   S(  5,  43),   S( 17,  32),   S(-35,  44),   S( 19,  46),   S(  7,  45),   S( -8,  62),   S(-29,  60),
            S(  7,  88),   S( 26,  66),   S( -4,  47),   S(  5,  53),   S(-19,  36),   S( 12,  42),   S(-13,  30),   S( -1,  65),

            /* bishops: bucket 7 */
            S(-40,  11),   S(-16,  20),   S(-51,   4),   S(-49,  -1),   S(-39,   4),   S(-87,  33),   S(-83, -31),   S(-46,   4),
            S(-61,  14),   S(-52,   6),   S(-39,  32),   S(  8,   5),   S(-17,  17),   S(-38,  23),   S(-33,  -3),   S(-28,  -4),
            S(-40,  11),   S(  5,  -5),   S( 25,  12),   S( 31,  12),   S(-13,  25),   S(  3,   6),   S(-28,  23),   S( 14, -10),
            S(-28,  18),   S( 15,  27),   S( 64,  -2),   S( 39,  28),   S( 65,  23),   S(-27,  28),   S( 15,  35),   S( -6,  25),
            S(-30,  16),   S(-32,  10),   S( 51,  -3),   S(105, -15),   S( 47,  22),   S( 87,  20),   S( 18,  26),   S( 32,  18),
            S(-23,   2),   S(-28,  19),   S( 19,   3),   S(  6,  20),   S( 43,   3),   S( 95,  10),   S( 52,  11),   S(-16,  32),
            S(-24,   3),   S( -7,  18),   S(-26,  22),   S(-16,  12),   S(-12,   9),   S( 22,   3),   S( 26,  24),   S( -2,  24),
            S( -7,  18),   S(-15,  19),   S(-37,  17),   S(-17,  17),   S( 10,  -3),   S( -3,  14),   S( 23,  30),   S( 10,  38),

            /* bishops: bucket 8 */
            S( -3, -24),   S(-14, -77),   S(-54, -40),   S(  1, -22),   S(  4, -13),   S(-14, -21),   S( 19,  24),   S( -4, -10),
            S(  0,   2),   S(-31, -76),   S(-11, -44),   S(  8, -29),   S( 13, -21),   S( -2, -19),   S(-12, -59),   S(  8,  -2),
            S(  4,   2),   S( -5,  -2),   S(-11,  -8),   S(  2, -49),   S( 13, -37),   S( 15, -30),   S(  1, -68),   S(-22, -40),
            S( 10,  25),   S( -1, -17),   S( 16,  14),   S(  9,  -6),   S(  4, -21),   S( -5, -45),   S(-10, -33),   S( -1, -44),
            S( -2,  -1),   S( 12,  38),   S( 11,  -5),   S( 23, -14),   S( 32, -20),   S( 17, -11),   S( 11, -30),   S( -8, -40),
            S( -2,   7),   S(-10, -13),   S( 18,  28),   S( -7, -41),   S( 37,   8),   S( -3, -32),   S(-11, -61),   S(-22, -54),
            S(-19, -21),   S( 12,  11),   S( 12,   0),   S(-12, -46),   S(  7, -22),   S(  4,  -5),   S( -5, -33),   S(-19, -49),
            S( -2,  -7),   S(  0, -49),   S( -5, -26),   S( -2, -25),   S(-10, -41),   S( -3, -54),   S( -5, -33),   S( -3, -26),

            /* bishops: bucket 9 */
            S( -6, -40),   S(  4, -40),   S(-24,  -1),   S(-10, -39),   S(-25, -55),   S( -3, -38),   S( -7, -27),   S( 17,  16),
            S( -8, -28),   S(-20, -58),   S( -2, -37),   S(  1, -13),   S(-11, -30),   S( 10, -33),   S( -1,   5),   S(  7,  20),
            S(  6, -13),   S(  6,   8),   S( 18, -26),   S( 45, -23),   S( 33, -17),   S( 33, -30),   S(-16, -44),   S( -1,   5),
            S( -8, -24),   S( 21,  32),   S(  7, -20),   S( 36, -35),   S( 44,  -7),   S( 15,   6),   S(  7, -52),   S( -9, -47),
            S( 12, -11),   S( 17,  -5),   S( 27,   1),   S( 32,   5),   S( 29, -57),   S( 30, -26),   S( -2,  -4),   S(-10,  -8),
            S(-12, -41),   S( 43,  14),   S(  3,  13),   S( 10,  22),   S( 26, -18),   S( 27, -39),   S(  5, -63),   S(-15, -23),
            S( -5, -21),   S( 25,  11),   S(  0,  -7),   S( 23, -12),   S( 24,  -9),   S( 18, -18),   S( -2, -37),   S( -7, -45),
            S( -2,  -9),   S(  2,   2),   S( -4, -63),   S(-17, -74),   S(-10, -26),   S(  8,   5),   S( -2, -34),   S( -9, -57),

            /* bishops: bucket 10 */
            S( -9, -10),   S(  8,   5),   S(-22, -34),   S( -3, -17),   S( -6, -17),   S(-39, -57),   S( -1, -54),   S( -8, -36),
            S(  8, -30),   S( -5, -20),   S(  0, -29),   S(-17, -37),   S(-14, -45),   S(  4, -51),   S( -8, -55),   S( -5, -35),
            S(  6, -32),   S( 21, -48),   S( 12, -52),   S( 55, -42),   S( 43, -37),   S(-11, -23),   S( -7,  -8),   S(  3,  19),
            S( -4, -63),   S( 27, -18),   S( -1, -34),   S( 55, -38),   S( 65, -47),   S( 18, -20),   S( -2,   0),   S( 18,  21),
            S(-17, -43),   S( 30, -43),   S( 52, -52),   S( 60, -41),   S( 59, -29),   S( 42, -16),   S( 25,   3),   S( -1, -37),
            S(-15, -57),   S(  5, -51),   S( 18, -49),   S( 12, -47),   S( 49, -27),   S( 37,  11),   S( 15, -29),   S(  4, -38),
            S( -7, -53),   S(  2, -53),   S(  6, -61),   S( 16, -55),   S(  6, -49),   S( 13, -10),   S( 22,  54),   S( 11,  11),
            S( -5, -64),   S(  0, -32),   S( 13,  20),   S( -3, -31),   S(  2,   1),   S( -7, -54),   S( 10,   9),   S(  5,   5),

            /* bishops: bucket 11 */
            S( -4,  33),   S(-33,  27),   S(-38, -52),   S( -9, -12),   S(-25, -38),   S(-29, -66),   S( -6, -27),   S( -5, -31),
            S(  7, -19),   S(  1, -32),   S(-30, -16),   S(-23, -19),   S( 13, -36),   S( -7, -60),   S(-18, -60),   S(-29, -37),
            S(-17, -59),   S(  0, -56),   S(  0, -40),   S( 30, -47),   S( 13, -32),   S( 13, -21),   S(-10, -26),   S(  5, -15),
            S(-10, -19),   S(  1, -60),   S( 32, -47),   S( 25, -45),   S( 61, -38),   S( 38,  14),   S( 48,  -1),   S( 25,  68),
            S(  3, -40),   S(  6, -59),   S( 17, -51),   S( 68, -23),   S( 57, -24),   S( 61,   5),   S( -3,  12),   S( 10, -15),
            S(-20, -65),   S( 11, -53),   S( 12, -39),   S( 30, -13),   S( 31, -19),   S( 21,  -3),   S(  3,  34),   S(-14, -18),
            S( -5, -39),   S(  5, -56),   S( -2, -57),   S( 12, -44),   S(  5,  -8),   S( 35, -23),   S( -7, -31),   S(  6,  -2),
            S(-24, -104),  S(-17, -55),   S(  1, -42),   S( 15,   1),   S( 17, -23),   S(-15, -55),   S( 10, -11),   S( 10,  25),

            /* bishops: bucket 12 */
            S( -1, -11),   S(-10, -33),   S(-19, -69),   S( -8, -42),   S( -3,  -4),   S( -2, -10),   S(  3,  17),   S( -8, -25),
            S( -3, -15),   S( -6, -41),   S( -7, -38),   S( -2,  -5),   S( -4, -31),   S( 16,  44),   S(  7,  10),   S(  6,   7),
            S( -2, -11),   S( -7, -34),   S(-10, -30),   S(  1,   0),   S(  7,  18),   S( -2, -37),   S(-15, -48),   S(  0,  -5),
            S( -4, -16),   S( -8, -35),   S( -3, -41),   S( 10,   1),   S( -3, -31),   S( 10,  11),   S( -3, -35),   S( -4, -13),
            S( -6,  -8),   S( 11,   1),   S(  5, -16),   S(-12, -13),   S(  5, -19),   S(  0, -11),   S( 10,  -3),   S(-10, -21),
            S( -7,  -3),   S(-13, -26),   S(  7,  19),   S(-11, -33),   S( -5, -26),   S( -4, -28),   S(  0, -26),   S(  6,  11),
            S( -6,  -6),   S( -8,  -6),   S( 16,  43),   S(-14, -38),   S(  0, -21),   S(  8,  16),   S( -9, -20),   S( -4, -11),
            S(  0,  -5),   S( -5, -10),   S(-10, -55),   S(  7,   7),   S(  9,  11),   S( -1, -14),   S( -9, -37),   S(  1,  19),

            /* bishops: bucket 13 */
            S(-10, -45),   S( -9, -49),   S( -9, -43),   S(  3, -24),   S( -9, -43),   S(  1,  -1),   S( -4, -25),   S(-10, -35),
            S(  1, -10),   S( -1, -33),   S(  0, -53),   S( -7, -35),   S(  6, -27),   S( -5, -20),   S(  3,  -5),   S(  3, -31),
            S( -6, -31),   S( -3, -15),   S(  7, -20),   S(  3, -74),   S(  9, -76),   S(  5, -39),   S(  4,  -9),   S(  6,  26),
            S( -6,  -7),   S(  3,  -4),   S( -5, -49),   S( -5, -59),   S( 24, -30),   S(  7, -17),   S(  1,   9),   S( -9, -48),
            S(  0,   6),   S(-16, -17),   S( -4, -67),   S( 20,  -2),   S( 10, -24),   S( 10, -31),   S( -1, -34),   S(  0, -14),
            S( -8, -32),   S( -2, -11),   S(-12, -13),   S( 20,   0),   S(  1, -26),   S(  9, -32),   S( 16,  -6),   S( -6, -38),
            S( -3, -22),   S( -7, -29),   S(-12, -20),   S( -3,  23),   S( -6, -34),   S(  8,  -1),   S( -6, -36),   S(  4,   2),
            S(-13, -62),   S( -5, -21),   S(  0, -17),   S(  5, -23),   S( -7, -24),   S( -3, -27),   S( -1,   0),   S( -3, -36),

            /* bishops: bucket 14 */
            S( -1, -42),   S(-12, -50),   S(-15, -69),   S(-11, -58),   S( -6, -51),   S( -3, -41),   S(-10, -79),   S(-11, -42),
            S( -1,   3),   S(  6,  17),   S( -7, -34),   S(-21, -81),   S( -5, -40),   S(-13, -85),   S(-14, -56),   S(  1, -29),
            S( -4, -20),   S( -4, -27),   S( -2, -31),   S( -7, -69),   S(  0, -45),   S(-17, -87),   S( -8, -36),   S(  0, -10),
            S( -1,  -7),   S( -5, -33),   S( -1, -22),   S( -1, -41),   S( 24, -50),   S( -3, -39),   S(-14, -53),   S( -8, -26),
            S( -1, -12),   S(  3, -19),   S(  1, -49),   S( 10, -29),   S(  7, -62),   S(  7, -27),   S(  4, -28),   S( -4, -35),
            S(  8,   4),   S( -6, -46),   S(-10, -50),   S( -1, -42),   S(  1,  -2),   S(  2,  21),   S(  6, -50),   S( -7, -38),
            S( -8, -59),   S(  0, -30),   S( -3, -24),   S(  6,  -2),   S(-17, -35),   S(  0,  -3),   S( -1, -22),   S( -7, -27),
            S( -5, -38),   S( -3, -24),   S( -2,  -7),   S( -9, -40),   S( -5, -18),   S(  2,   7),   S(  4,  25),   S( -2, -15),

            /* bishops: bucket 15 */
            S(  6,  25),   S( -2,  14),   S( -9, -29),   S( -2, -11),   S( -6, -19),   S(-16, -32),   S( -1,  -4),   S( -5, -23),
            S( 10,  39),   S(  4,  23),   S(  8,   9),   S( -3, -34),   S( -8, -36),   S( -3, -15),   S( -1,  -2),   S( -3, -11),
            S( -5, -33),   S( -2, -22),   S( -8, -50),   S( -8,  -3),   S(-10, -71),   S( -7, -30),   S( -9, -37),   S( -3, -13),
            S( -1, -14),   S(-15, -54),   S(  3, -22),   S(-13, -55),   S(  0, -25),   S( -7, -26),   S( -1,  -2),   S( -1,  -7),
            S( -3, -34),   S( -3, -21),   S(  2, -31),   S(-16, -47),   S( -4, -46),   S(-13, -48),   S( 14,  13),   S( -7, -23),
            S( -3, -23),   S( -3, -48),   S(-16, -63),   S(-29, -94),   S( -8, -51),   S(-11,   5),   S( 28,  45),   S(  3,  16),
            S( -4, -37),   S( -2, -13),   S( -8, -40),   S(  0, -14),   S(-18, -57),   S(  7,   3),   S( -7, -20),   S(  2,   7),
            S( -5, -18),   S(  0,  -3),   S( -8, -44),   S( -6, -47),   S( -2, -41),   S(-21, -75),   S(-12, -16),   S(  0,  -2),

            /* rooks: bucket 0 */
            S(-29,   3),   S( 16, -15),   S(  6, -22),   S( 14, -12),   S( 28,  -6),   S( 19, -15),   S( 15,  -3),   S( 18,   0),
            S( 15, -79),   S(  9, -24),   S( 24,   2),   S( 23, -14),   S( 22,  -1),   S( 25, -16),   S( -4,  -3),   S(-49,  22),
            S( 18, -17),   S( 14,  19),   S( 29,  -2),   S(  5,  27),   S( 21,   0),   S( 19,  -5),   S(-13,  10),   S(-34,   2),
            S( 44, -41),   S( 73, -15),   S( 62,   7),   S( 54,   0),   S( 40,   0),   S( -4,  30),   S(-16,  21),   S(-20,  12),
            S( 94, -37),   S( 62,   6),   S( 94, -28),   S( 39,   9),   S( 67, -28),   S( 49, -14),   S(  2,  19),   S(-24,  39),
            S( 68, -59),   S( 95, -37),   S( 56,  16),   S( 42,   1),   S( 69,   3),   S(-29,  28),   S( 89, -11),   S(-54,  29),
            S( 77, -13),   S( 79,  -6),   S(  6,  20),   S( 34,  35),   S( -2,  25),   S(  5,  31),   S( 19,   9),   S(  3,  40),
            S( 24,  18),   S(  9,  43),   S( 24,  39),   S( 49,   7),   S( 78, -12),   S( 42,   6),   S( 62,  -6),   S( 42,  -4),

            /* rooks: bucket 1 */
            S(-82,  41),   S(-43,   5),   S(-30,  -9),   S(-35,  -9),   S( -4, -22),   S( -5, -21),   S(-25,  -2),   S(-38,  27),
            S(-40,   1),   S(-57,  15),   S(-21,  11),   S(-11, -21),   S(-17,  -9),   S(-42,  10),   S(-44, -10),   S(-54,  -1),
            S(  3,   2),   S(-37,  59),   S(-26,  27),   S(-50,  45),   S(-29,  22),   S(-12,  14),   S( -9,  -7),   S(-56,  31),
            S(-65,  59),   S(-43,  37),   S(-11,  36),   S(-21,  39),   S(-48,  61),   S(-49,  48),   S(-28,  38),   S(-51,  35),
            S( 60,  14),   S( 39,  32),   S(  0,  46),   S(-19,  47),   S(-22,  48),   S( 56,  13),   S(-13,  30),   S(-42,  23),
            S(103,  -3),   S(  9,  51),   S( 31,  28),   S(-16,  35),   S( 17,  24),   S(  5,  42),   S(  1,  24),   S(-32,  41),
            S(-27,  59),   S( 11,  47),   S( 29,  35),   S(-83,  96),   S(-26,  43),   S( 27,  49),   S(-49,  55),   S(-19,  35),
            S( 62,  18),   S(  2,  43),   S( 22,  25),   S(-26,  75),   S(-11,  44),   S( 32,  20),   S( -6,  39),   S( 67,  -8),

            /* rooks: bucket 2 */
            S(-71,  48),   S(-40,  26),   S(-35,  27),   S(-51,  25),   S(-47,  17),   S(-46,  10),   S(-41,   1),   S(-52,  28),
            S(-71,  46),   S(-65,  45),   S(-32,  38),   S(-47,  16),   S(-41,  17),   S(-74,  24),   S(-68,  28),   S(-66,  28),
            S(-74,  72),   S(-70,  66),   S(-46,  62),   S(-43,  33),   S(-58,  47),   S(-52,  44),   S(-14,  16),   S(-31,  28),
            S(-69,  66),   S(-46,  68),   S(-38,  78),   S(-33,  65),   S(-28,  52),   S(  1,  36),   S(-32,  62),   S(-19,  37),
            S( -9,  55),   S(-44,  70),   S(-42,  67),   S(-28,  61),   S( 25,  35),   S( -1,  69),   S(-33,  61),   S(-21,  46),
            S(-22,  53),   S(-23,  60),   S( -6,  48),   S( 12,  44),   S( 34,  33),   S( 41,  35),   S(  4,  34),   S(-35,  47),
            S(-57,  73),   S(-44,  81),   S(-40,  72),   S( 21,  63),   S( 45,  40),   S( 54,  35),   S(-62,  85),   S(  0,  42),
            S(-28,  73),   S( 10,  46),   S(-51,  71),   S( -2,  50),   S(-16,  55),   S(  2,  52),   S(-36,  72),   S( 13,  43),

            /* rooks: bucket 3 */
            S( -2,  73),   S(  2,  68),   S( 15,  65),   S( 18,  58),   S( 14,  60),   S(-11,  66),   S(  2,  66),   S(-20,  56),
            S(-31,  92),   S( -7,  64),   S( 11,  64),   S( 17,  59),   S( 15,  66),   S( 14,  54),   S( 33,  16),   S(  5, -19),
            S(-38,  91),   S(-22,  85),   S(  8,  83),   S( 15,  57),   S( 15,  76),   S( 13,  68),   S( 23,  58),   S( -2,  55),
            S(-26,  89),   S(-33, 100),   S( 13,  84),   S( 22,  84),   S( 30,  76),   S( -1, 105),   S( 53,  67),   S(  8,  71),
            S(-13, 105),   S( 16,  90),   S( 20,  76),   S( 42,  76),   S( 45,  72),   S( 33,  74),   S( 82,  49),   S( 52,  47),
            S(-10,  96),   S( 18,  80),   S( 25,  80),   S( 42,  70),   S( 54,  54),   S( 63,  46),   S( 90,  33),   S( 87,  25),
            S(-13, 106),   S(-12, 108),   S( 11,  97),   S( 56,  84),   S( 18,  86),   S( 86,  49),   S( 83,  58),   S(113,  31),
            S(-52, 138),   S( 46,  82),   S( 30,  82),   S( 60,  69),   S( 62,  57),   S(125,  27),   S(136,  31),   S(127,  34),

            /* rooks: bucket 4 */
            S(-82,   6),   S(-22, -24),   S(-68,  26),   S(-19,  10),   S(-44, -10),   S(-30, -26),   S( 21, -32),   S( -4, -49),
            S(-46,   5),   S(-56,   0),   S(-35,  11),   S(-23,  11),   S(-31, -12),   S( -4, -15),   S(  2, -42),   S(-34, -28),
            S( -2,  12),   S(-13, -21),   S(-48,  10),   S( -5, -12),   S(-21,   5),   S( 31, -33),   S( 28,   2),   S(-34, -21),
            S(-37, -22),   S( 18,  -5),   S(-40,  24),   S( 58,  -8),   S( 46,  -8),   S(  6, -11),   S(-23,  11),   S(  1,  12),
            S(-63,   5),   S( -9,  25),   S(  8,  12),   S( 60,  19),   S( 30,  -7),   S(-18,  21),   S( 42,  14),   S( 34, -20),
            S( 29,  -6),   S( 23,   3),   S( 62,  12),   S( 48,  27),   S( 70,   3),   S( 24,  48),   S( -1,  20),   S(  1,  26),
            S(-17,   6),   S( 34,  33),   S( 19,  22),   S( 56,  14),   S( 57,  -4),   S( 13,  -4),   S( 24,   5),   S( 35,  17),
            S( 10, -48),   S( 68,  48),   S( 43, -14),   S( 16,  18),   S( 19,   5),   S( 13,  17),   S( 11,  -7),   S(-21,   6),

            /* rooks: bucket 5 */
            S(-35,  32),   S(-58,  50),   S(-20,  29),   S(-28,  -3),   S(-18,  13),   S( -1,  23),   S( -3,  26),   S(-48,  50),
            S( -3,  22),   S(-35,  40),   S(-65,  59),   S(-45,  33),   S(-62,  51),   S(-12,  11),   S( 20,  10),   S(-35,   6),
            S(-43,  52),   S(-53,  65),   S(-62,  54),   S(-75,  40),   S(-44,  31),   S(-23,  38),   S(-19,  34),   S(-29,  22),
            S(-37,  68),   S(  5,  38),   S(-16,  58),   S(  2,  46),   S(-19,  50),   S(-22,  69),   S( 20,  46),   S(-10,  24),
            S( 10,  67),   S( 26,  54),   S(  4,  62),   S(  8,  76),   S( 30,  42),   S( 21,  59),   S( 69,  50),   S( 45,  25),
            S( 44,  69),   S( 59,  50),   S( 54,  53),   S( 44,  60),   S( 74,  42),   S( 89,  46),   S( 59,  37),   S( 54,  32),
            S( 35,  49),   S( 27,  63),   S( 92,  33),   S( 28,  75),   S( 67,  44),   S( 38,  53),   S( 79,  38),   S( 86,  46),
            S( 78,  25),   S( 70,  22),   S( 44,  48),   S(  2,  47),   S( 52,  52),   S( 63,  40),   S( 62,  30),   S( 30,  49),

            /* rooks: bucket 6 */
            S(-55,  39),   S(-27,  32),   S( -3,  25),   S( -1,   9),   S(-35,  21),   S(-69,  51),   S(-55,  56),   S(-16,  37),
            S( -9,  23),   S(-21,  31),   S(  5,  24),   S(-58,  40),   S(-20,  35),   S(-102,  72),  S(-58,  44),   S( 28,  12),
            S(-56,  51),   S(-55,  53),   S(-13,  40),   S(-49,  43),   S(-27,  37),   S(-56,  65),   S(-42,  68),   S( 26,   2),
            S(-57,  67),   S( 19,  51),   S(-16,  65),   S(  1,  52),   S( -2,  44),   S(-13,  72),   S(-73,  85),   S(  9,  37),
            S( 23,  65),   S( 45,  57),   S( 65,  47),   S( 43,  47),   S( -3,  77),   S( 31,  69),   S( 22,  52),   S( 11,  47),
            S( 13,  64),   S( 68,  51),   S(101,  33),   S( 63,  35),   S( 22,  54),   S( 49,  68),   S( 73,  56),   S( 85,  41),
            S( 80,  50),   S( 68,  50),   S(109,  29),   S( 79,  41),   S(103,  33),   S( 52,  60),   S( 76,  40),   S( 64,  44),
            S( 93,  52),   S( 72,  40),   S( 50,  41),   S( 58,  43),   S( 48,  44),   S( 70,  40),   S( 91,  37),   S( 42,  46),

            /* rooks: bucket 7 */
            S(-83,   9),   S(-46,  -2),   S(-20, -15),   S(-37,   8),   S(-11,   1),   S(-34,  24),   S(-39,   8),   S(-10, -11),
            S(-70,  24),   S(-16,  -4),   S(-21,   0),   S(  0,  -5),   S( -6,   8),   S( 17,   2),   S(-27,  26),   S(-28,  -9),
            S(-89,  49),   S(-30,  12),   S(-21,  30),   S(  9, -16),   S( -7,   6),   S(-22,   7),   S( -6,  13),   S( 43,  13),
            S(-26,  29),   S( 10,  16),   S( 19,  20),   S(  5,  30),   S( 19,   9),   S( 22,   5),   S( 24,  13),   S( -7,  14),
            S( 16,  22),   S(-28,  34),   S( 49,  -4),   S( 48,   2),   S( 77,  -1),   S(103,   2),   S( 58,   5),   S( 47,  -6),
            S(  0,  31),   S( 17,  18),   S( 85,  -8),   S( 97, -14),   S( 56,   6),   S( 93,   7),   S( 46,  36),   S( 59, -12),
            S( 12,  35),   S( 26,  23),   S( 87,  -1),   S( 85,   6),   S(108,  -4),   S( 94,  12),   S( 67,  25),   S( 60,  -5),
            S( 10,  51),   S(-44,  48),   S( 56,  11),   S( 93, -13),   S( 43,  14),   S(  1,  14),   S( 48,   8),   S( 47,  -6),

            /* rooks: bucket 8 */
            S(-39, -40),   S(-30, -22),   S( -2,  20),   S(-56,  -6),   S(-38, -42),   S(-48, -43),   S(-47, -69),   S( -8, -20),
            S( -9, -14),   S( -1, -13),   S(-18,  18),   S( -4, -16),   S(-28,  -2),   S(-13, -18),   S( -8, -42),   S( -9, -72),
            S( -6,  -6),   S( -3, -33),   S( -1,  -6),   S(-15,  11),   S(-26, -28),   S(-27, -32),   S(  8,  40),   S(-24, -64),
            S(  2, -11),   S( -8,  -5),   S(-15, -10),   S( 11,  -6),   S( 11,  18),   S(-17, -14),   S( -2, -33),   S(-21,  -8),
            S(-19, -27),   S( -2, -11),   S(-10,  26),   S(-18,  10),   S( -1,   3),   S( 12,  -6),   S( -9, -23),   S( -4, -21),
            S(  2,  24),   S(-15,  -1),   S( 19,  37),   S( 17,  10),   S(-11,  19),   S( -7, -13),   S(  4,   5),   S( -5,   4),
            S(-11,   3),   S( -8, -10),   S( 36,  37),   S(  9,  -5),   S( 34,   6),   S( -9, -52),   S( 20,  -5),   S(  3,  24),
            S( -7, -100),  S( -1, -20),   S( 25,  24),   S( -8, -16),   S( -8, -32),   S( -6, -55),   S( 13,  10),   S(  1,  20),

            /* rooks: bucket 9 */
            S(-47, -41),   S( -4, -49),   S(-29, -44),   S(-55, -20),   S(-42, -17),   S(-10, -16),   S(-28, -38),   S(-65, -44),
            S( 15, -50),   S(-25, -47),   S(-21, -30),   S(-29, -14),   S(-26, -47),   S(  1, -18),   S( -1, -43),   S(-28, -37),
            S(  1, -44),   S( 13, -35),   S( 12, -21),   S(-20, -13),   S(-47, -32),   S(  5, -35),   S(  1,  -8),   S(-18, -41),
            S(  3, -11),   S(-17,  -8),   S( -9,   8),   S(-42, -11),   S(-21,  -5),   S(-13, -23),   S(  5,   3),   S(  2, -18),
            S( -5,  -7),   S(-15,   7),   S(-11,   6),   S( -2,  27),   S( 14,  38),   S(  6, -25),   S( -4,   7),   S(  0, -37),
            S(  8,  17),   S(-22,  -4),   S(-13, -14),   S(-19,   7),   S( -4, -31),   S( 17,  -2),   S( 19,   8),   S(  9, -28),
            S( 21,  20),   S( 37, -20),   S( 44,   6),   S( 31, -18),   S( 25,  -1),   S( 33, -19),   S( 27,  -3),   S( 47, -13),
            S( 41, -86),   S( 18, -57),   S( 25,  -1),   S( 21,  24),   S(  9,   6),   S( 13,   1),   S(  8, -29),   S( 17,  -5),

            /* rooks: bucket 10 */
            S(-58, -108),  S(-38, -59),   S(-39, -57),   S(-38, -34),   S(-59, -50),   S(-31, -47),   S( 10, -38),   S(-16, -52),
            S(-17, -48),   S(  2, -36),   S(-20, -58),   S(-48, -17),   S(  1, -22),   S(-28, -38),   S( 17, -25),   S(  5, -34),
            S(-29, -28),   S(-19, -48),   S(-40, -48),   S(-14, -19),   S(-38, -27),   S(-16, -34),   S( 20,  -3),   S(-14, -21),
            S( -6,  -9),   S(-11, -35),   S(-34, -23),   S(-11, -22),   S( 12,   1),   S(  2, -20),   S(  9,  22),   S(-15, -39),
            S(-12, -35),   S( 18, -22),   S(-12, -26),   S(  9, -34),   S(-15, -10),   S( -3,  -8),   S( 18,   1),   S(-12, -18),
            S( 27, -12),   S( 12,   6),   S( 12, -24),   S(-15, -17),   S( -9, -21),   S(  7, -23),   S(  9, -22),   S(  2,   0),
            S( 68, -18),   S( 76, -32),   S( 52, -32),   S( 38, -25),   S( 32, -25),   S( 16, -17),   S( 21, -26),   S( 19, -41),
            S( 41,   1),   S( 12, -13),   S( 31, -18),   S( 12, -32),   S( 17, -21),   S( 16, -14),   S( 15, -41),   S(  0, -41),

            /* rooks: bucket 11 */
            S(-60, -33),   S(-23, -35),   S(-34, -33),   S(-21, -83),   S(-47,  -9),   S(-21,  -4),   S(-26, -32),   S(-70, -27),
            S(-48, -33),   S(-30, -53),   S(-30, -29),   S(-20, -20),   S(-39, -40),   S(-39,  -9),   S(-28, -21),   S(-39,  -3),
            S(-31, -30),   S( 20, -38),   S( -5,   4),   S(  7,  -2),   S(  2, -11),   S( -8,  -5),   S(-26, -10),   S(-31, -41),
            S(-33,   5),   S( -3, -36),   S(-13,  15),   S( 21,  27),   S(-13, -24),   S(-27,  13),   S(  5,  -4),   S(  1, -12),
            S(-22,  14),   S( -6, -14),   S( 16,  -8),   S( 27, -27),   S( 14,  -1),   S( 14, -15),   S( 11,   0),   S( -8,  -3),
            S(  4,  23),   S( 16,   5),   S(  1, -14),   S( 49,   0),   S( 29,   2),   S( 18, -13),   S(-27,   7),   S( 19,  21),
            S( 46,  23),   S( 16,   4),   S( 61, -12),   S( 54, -10),   S( 26,  -2),   S( 19,  31),   S( 15,  15),   S( 60,   4),
            S( 23,  19),   S(  7,  12),   S( 13, -13),   S( 29, -18),   S(-12, -15),   S( 19,  -1),   S(  7,   0),   S( 15,   2),

            /* rooks: bucket 12 */
            S( -3, -68),   S( -1, -17),   S(-28, -35),   S(  8,  19),   S( -8, -44),   S( -9, -36),   S(-21, -73),   S(-23, -34),
            S( 10,   4),   S(  3,   0),   S(-10, -11),   S(-17, -29),   S(-15, -16),   S( -5,  -3),   S( -3, -34),   S( -2, -21),
            S(  8,  -7),   S( -8, -11),   S(-19, -22),   S( -8,  -6),   S(-13, -50),   S(  6,   5),   S(  4,   0),   S(  5,   9),
            S(-17, -17),   S( -7, -22),   S(  1,  11),   S(  5, -12),   S( -8, -26),   S( -7, -26),   S(-13, -34),   S( -4, -33),
            S(-14,  -9),   S(-18, -38),   S( 10,   2),   S(  5, -19),   S(-14, -34),   S( 10,   5),   S( -4, -18),   S(  5,   7),
            S(  4,  -5),   S(-10, -32),   S( 14,   6),   S(  8, -34),   S( -1,  -6),   S(-10, -40),   S(  0, -16),   S(  1,  -4),
            S(-13, -35),   S(  1, -17),   S( -5, -25),   S( -4, -40),   S( 11, -12),   S( -9, -60),   S(  0, -20),   S(  1, -14),
            S( -2, -39),   S( -1, -11),   S(  4, -24),   S( -1, -24),   S( 11,  10),   S(-17, -47),   S(-16, -72),   S(  4,  -1),

            /* rooks: bucket 13 */
            S( -6, -27),   S(  0, -29),   S(  1,  12),   S(  1,  36),   S(  3,  -3),   S(-20, -59),   S( -3, -38),   S( -4,  -8),
            S( -6, -20),   S(  0,   7),   S( -5,  27),   S( -8,  27),   S( -7, -32),   S( -5,  -9),   S( -1, -11),   S( -1, -24),
            S( -6, -42),   S(-11, -31),   S( -6, -17),   S( -1, -22),   S(  7,  40),   S( -3, -40),   S( -8, -14),   S( -6, -30),
            S( -7, -47),   S(  2,   9),   S(-24, -41),   S( -6, -24),   S(  0, -12),   S(-22, -26),   S( -9, -41),   S( -9, -18),
            S(  2, -28),   S(  3,  11),   S(  6,   7),   S( -9, -28),   S( -6, -39),   S(-12, -42),   S( -2, -23),   S( -1,   2),
            S(-22, -39),   S(  6, -16),   S( -6, -34),   S(  8, -39),   S( 14, -19),   S(  7,  -7),   S(  0, -19),   S(  7,   7),
            S( -4,  -7),   S( 14,  21),   S(  8,  18),   S( -7, -14),   S(  3,  -4),   S( 18,  31),   S(  8, -17),   S(-14, -35),
            S(-30, -136),  S(-19, -73),   S( -7, -30),   S( -4, -20),   S(  4,  24),   S( -6, -48),   S(-14, -61),   S(-13, -22),

            /* rooks: bucket 14 */
            S(-27, -39),   S(-20, -45),   S( -3, -24),   S(-13, -42),   S(-15,  19),   S(-17, -38),   S(  1, -26),   S(-22, -33),
            S(-19, -30),   S( -7, -27),   S(-14, -34),   S(-30, -46),   S( -7,  -9),   S( -6, -28),   S(  4,  12),   S(  1,  -8),
            S(-14, -41),   S(-12, -31),   S(-13, -31),   S(-15, -30),   S(-12, -15),   S(-12, -20),   S( 12,  38),   S(  3, -16),
            S(  2,  12),   S( -4, -24),   S( -7, -30),   S( -2,  -4),   S(  3,  -5),   S(  3,  -2),   S( -7, -44),   S( -1,  25),
            S( -6, -45),   S( -1, -56),   S( -8, -40),   S(-17, -76),   S(  5, -37),   S( -2, -41),   S(  1, -42),   S( -1, -24),
            S( -6, -39),   S( -9, -30),   S( -2, -36),   S(  9, -58),   S( -9, -66),   S( -6, -63),   S( -2, -35),   S(  1,  -9),
            S( 24,   9),   S( -2, -40),   S(  1, -34),   S(-10, -68),   S(  1, -68),   S(  4, -36),   S(  3,  -6),   S(  6,  -3),
            S(-10, -36),   S(  1, -19),   S(-15, -67),   S( -4, -31),   S(-12, -39),   S( -3,  20),   S( 10,  23),   S(-13, -13),

            /* rooks: bucket 15 */
            S(-18, -36),   S(-15, -43),   S(  8, -13),   S(-10, -24),   S(-17, -39),   S(  3,   5),   S(-16, -52),   S(  3,   3),
            S(-17, -26),   S(-26, -65),   S( -9,  -8),   S(-19, -32),   S( -7, -16),   S(  3,  18),   S(-13, -46),   S(  4,   7),
            S( -1,  -7),   S(-14, -50),   S( -7, -30),   S( -5, -46),   S(  7,  -1),   S(-12, -31),   S(  7,  19),   S(-10, -29),
            S(-10, -53),   S(  1, -19),   S( -3,  -4),   S( -1, -19),   S(-12, -35),   S( -3, -34),   S( -1,  -3),   S( -6,   7),
            S( -7, -10),   S(  5, -19),   S(  7, -17),   S(  0, -29),   S(  5,  -5),   S(  6,  -9),   S(  4, -22),   S( -1,  12),
            S(  1, -15),   S( -7, -37),   S(  3, -46),   S( -2, -57),   S(-11, -92),   S(  6,  -8),   S( 11, -21),   S(  0,  -3),
            S( -1, -18),   S( 14,  -7),   S( 12,   9),   S(  2, -48),   S( -2, -42),   S(  9,  10),   S(  0, -13),   S( -4,  -6),
            S( -2, -18),   S( -9, -24),   S(  2, -28),   S( -3, -51),   S(-11, -60),   S(  3, -27),   S(  0, -34),   S( -6, -16),

            /* queens: bucket 0 */
            S(-38, -23),   S(-24, -65),   S( 51, -97),   S( 58, -57),   S( 42, -77),   S( 26, -26),   S( 67,  -3),   S( 11, -15),
            S(  8, -15),   S( 29, -88),   S( 38, -28),   S( 37, -17),   S( 35,   2),   S( 17,  39),   S( 27,  19),   S( 36,   6),
            S( 15,  13),   S( 41, -17),   S( 19,  32),   S(  8,  56),   S( 28, -13),   S(  6,  28),   S(  7,  39),   S( 19,  57),
            S(  5,  31),   S( -4,  48),   S(  0,  59),   S( 21,  38),   S( 41,  34),   S(  0,  30),   S( 10,  41),   S( 10,  40),
            S( 26,  39),   S( 30,  42),   S( 21,  39),   S( 24,  64),   S(-30,  47),   S( 14, -21),   S( 20,   4),   S( 61,  -5),
            S( 30,  63),   S( 40,  45),   S( 33,  41),   S( 43,  10),   S( 40,  31),   S( 22,  40),   S(  0,   4),   S( 15, -23),
            S( 54,  13),   S( 34,  12),   S(  6,  75),   S( 80,   9),   S(  5,  10),   S(  3,  50),   S( 34,  33),   S( 28,  24),
            S( 47,  26),   S(  6,  29),   S( 47,  36),   S( 38,  49),   S( 38,  35),   S(-13,  20),   S( 79,  13),   S( 40,  25),

            /* queens: bucket 1 */
            S( 12, -71),   S(-73, -31),   S(-42, -21),   S(-14, -68),   S( -1, -36),   S( -4, -85),   S(  7, -58),   S( 38,  -8),
            S(-23, -49),   S(-26, -36),   S( 10, -35),   S( 11,  16),   S(  6,  16),   S( 20, -16),   S( 43, -74),   S(-15,  10),
            S(-30,  39),   S( -3, -19),   S( 12,   1),   S( 13, -14),   S(  4,  41),   S( -5,  30),   S( 19,  -1),   S(  4,  38),
            S( 15, -36),   S(-17,  51),   S(-28, 102),   S( 21,  60),   S( -6,  71),   S(  7,  26),   S(  2,   9),   S(  9,   9),
            S( 25,   1),   S( -2,  18),   S(  2,  87),   S(-11,  70),   S(-18,  57),   S( 40,  -1),   S(-11,   9),   S(  0,  21),
            S( -2,  55),   S(  4,  69),   S( 37,  59),   S(-21,  57),   S(-28,  23),   S(-57,  61),   S( 33,  38),   S( 41,  15),
            S(-32,  28),   S(-13, 101),   S(-22,  64),   S(-14,  80),   S(-11,  65),   S( 35,  23),   S(-29,  35),   S(-16,  45),
            S(-13,  14),   S( -3,  28),   S( 45,  29),   S(-39,   6),   S( -7,  32),   S(-17,  15),   S( 22,  33),   S( -4,  45),

            /* queens: bucket 2 */
            S( 40, -19),   S( 24, -113),  S( 16, -45),   S(  1, -22),   S(-22,  13),   S(-32, -59),   S(-44, -33),   S( 20,  -9),
            S( 12,  32),   S( 21,  30),   S( 12, -15),   S( 25, -25),   S( 17, -13),   S(  6, -48),   S(  5,   8),   S( 34, -53),
            S(  3,  26),   S(  8,   1),   S(  4,  46),   S(  5,  45),   S( -8,  84),   S(  4,  54),   S(  9,  24),   S( 19,   2),
            S(  1,   6),   S( -9,  44),   S( -4,  32),   S(  8,  57),   S(-14, 104),   S( -4,  79),   S( 12,  30),   S(-12,  64),
            S(  3,  17),   S(-19,  74),   S(-32,  98),   S(-53, 138),   S(-42, 136),   S(-41, 130),   S(-30, 125),   S( -8, 103),
            S( 10,  15),   S(  7,  36),   S(-35, 105),   S(-20,  82),   S(-22, 104),   S(-44, 119),   S(-30, 110),   S(  5, 107),
            S(-44, 102),   S(-47, 101),   S(-27,  86),   S( 19,  59),   S(-22, 104),   S( 47,  56),   S(-50,  46),   S(-34, 118),
            S(-56,  93),   S(  8,  69),   S( 33,  51),   S( 47,  48),   S( 46,  61),   S( 48,  25),   S( -4,  48),   S(-10,  47),

            /* queens: bucket 3 */
            S( 78,  73),   S( 47,  85),   S( 46, 102),   S( 41,  74),   S( 65,  22),   S( 33,  24),   S( 26,  -5),   S( 14,  44),
            S( 52,  97),   S( 53, 105),   S( 34, 100),   S( 48,  71),   S( 44,  68),   S( 53,  54),   S( 48,  22),   S( 24,  22),
            S( 45,  90),   S( 39,  94),   S( 46,  80),   S( 47,  64),   S( 41,  96),   S( 43,  94),   S( 44, 109),   S( 45,  54),
            S( 31, 128),   S( 43,  82),   S( 38,  84),   S( 35,  92),   S( 34, 102),   S( 20, 140),   S( 49,  88),   S( 18, 141),
            S( 27, 116),   S( 41, 112),   S( 38, 105),   S( 25, 108),   S( 26, 128),   S( 11, 140),   S( 17, 169),   S( 23, 169),
            S( 31, 131),   S( 43, 127),   S( 30, 112),   S( 26, 120),   S( 23, 153),   S( 57, 114),   S( 39, 179),   S( 21, 181),
            S( 46, 128),   S( 30, 119),   S( 65,  83),   S( 48, 106),   S( 19, 129),   S( 45, 127),   S( 58, 145),   S(148,  50),
            S( 52, 112),   S( 88,  80),   S( 68,  98),   S( 68, 104),   S( 20, 107),   S( 66,  90),   S(116,  50),   S(127,  50),

            /* queens: bucket 4 */
            S(-26, -28),   S( 28,   0),   S(-24,  -6),   S(-14, -24),   S( 29, -32),   S( 12, -25),   S(-25,  -8),   S(-62, -28),
            S(-22, -33),   S( -4,   0),   S( 50,   6),   S(-48,  40),   S(-17, -15),   S( -7, -50),   S(  5,  -2),   S(-27, -28),
            S(-11,  -6),   S(  2, -15),   S( -5,  20),   S( 23,  51),   S( 18,   1),   S( 60,   3),   S( -7, -24),   S( -3, -24),
            S( -2,  -2),   S( 10,  39),   S(  8,  32),   S(-24,  39),   S( 32,  14),   S( 11,  33),   S( -4, -27),   S(-28,  -3),
            S(-20,   4),   S( 12,  20),   S( 23,  24),   S( 36,  52),   S( 35,  39),   S( 16, -26),   S( -2,  -6),   S( -7, -19),
            S(-18,  10),   S( 29,   4),   S( 32,  43),   S(  3,  40),   S( 42,  38),   S( -1,  10),   S(-28, -29),   S(-21,   2),
            S(-50, -35),   S( 13,  31),   S( 20,  21),   S( 30,  44),   S( 17,  10),   S(-10,   8),   S(-11, -41),   S(-21, -36),
            S(  9,   1),   S( -8, -24),   S( 55,  55),   S( 30,  42),   S(-19,  -4),   S(-13,  -1),   S(-38, -79),   S(-20, -34),

            /* queens: bucket 5 */
            S(-52, -33),   S(-46, -44),   S(  5,  -5),   S( -1, -35),   S(-25,  -8),   S(  5, -18),   S(-31,  -6),   S( -6,  -5),
            S(-33, -19),   S(-39, -20),   S(-42,  -8),   S(-76,  -5),   S(-31, -20),   S(-39,  -9),   S(-46, -15),   S(-57, -30),
            S( -5,  16),   S(-70, -21),   S(-69, -18),   S(-51,  32),   S( 38,  52),   S(-11,  19),   S(  9, -20),   S( 21,  12),
            S(-38, -15),   S(-62, -18),   S( 11,  29),   S( 32,  53),   S( 50,  37),   S( 18,  33),   S( 17,  -3),   S(-28,  -5),
            S(-31,  -5),   S(  2,  27),   S(-17,  49),   S( 35,  73),   S( 18,  60),   S( 10,  31),   S( -6,   4),   S(-30, -14),
            S(-31,   5),   S( 37,  60),   S(-12,  49),   S( 19,  42),   S( 51,  61),   S( 26,  28),   S( -4,  13),   S(-24,   2),
            S(-12,   5),   S(  7,  12),   S( 12,  58),   S(-22,  34),   S( 20,  64),   S( 37,  68),   S( 39,  25),   S( -1,  -4),
            S(-11, -15),   S( 14,  16),   S(-11,   1),   S( 22,  65),   S( 27,  46),   S( 12,  17),   S( 11,   2),   S(-11, -20),

            /* queens: bucket 6 */
            S( -2,  -6),   S(-30, -26),   S(-51, -43),   S(-88, -68),   S(-86, -43),   S(-59, -49),   S(-45, -47),   S(-21,   4),
            S(-58, -21),   S(-21,  11),   S(-16, -28),   S(-58,  35),   S(-81,  34),   S(-104, -13),  S(-84,   0),   S(  9,  19),
            S(-36, -13),   S(-21,  -6),   S( -1,  26),   S(-89,  84),   S(  1,  77),   S( -5,  17),   S(-60, -23),   S(  1,  17),
            S(-12,   8),   S(-20,  21),   S(-39,  66),   S(-39,  87),   S( 15,  77),   S( 36,  65),   S( -6,  53),   S( 34,   9),
            S(-37,  49),   S(-21,  45),   S(-19,  61),   S( 31,  72),   S( 35,  99),   S( 65,  75),   S( 42,  48),   S(  9,  19),
            S(-36,  47),   S( -2,  33),   S( 29,  37),   S( 58,  65),   S( 18,  64),   S( 70,  89),   S(-15,   9),   S( -2,   7),
            S( -5,  36),   S(-10,  10),   S(  9,  49),   S( -7,  44),   S( 65,  79),   S( 18,  76),   S(-24,  44),   S(-46,  -3),
            S(  5,  40),   S( -8,  32),   S( 26,  30),   S(  9,  33),   S( 59,  68),   S( 26,  40),   S( -5,  16),   S( 14,  36),

            /* queens: bucket 7 */
            S(-11, -13),   S(-55,  -5),   S(-56,   8),   S(-33,  14),   S(-20, -14),   S(-34, -12),   S(-21,  10),   S(-24, -11),
            S(-41,  -4),   S(-43,  -3),   S(-42,  52),   S(-10,  15),   S(-15,  34),   S(-32,  17),   S(-56,  59),   S(  6, -14),
            S(-28,   2),   S(-51,  31),   S(-13,  48),   S( -1,  41),   S( 12,  52),   S(-20,  65),   S( 40,   4),   S( -3,  -2),
            S(-71,  -3),   S( -8,  19),   S(-32,  35),   S(-15,  45),   S( 49,  34),   S( 67,  35),   S( -7,  63),   S( 13,  -5),
            S(-58,  32),   S(-49,  35),   S( 14,  32),   S( 35,   9),   S( 63,  23),   S( 79,  26),   S( 12,  62),   S( 53,   5),
            S(-27,  24),   S(-21,  38),   S( -4,  15),   S( 28,   9),   S( 21,  66),   S(121,  29),   S( 81,   4),   S( 46,  35),
            S( 10,   3),   S( 18,   5),   S( 48, -10),   S( 13,  31),   S( 52,  53),   S( 53,  48),   S( 44, -17),   S( 42,  31),
            S(  6,  29),   S( 32,  25),   S( 49,  20),   S( 40,  57),   S( 51,  48),   S( 56,  50),   S( 23,  29),   S( 58,  60),

            /* queens: bucket 8 */
            S( -6, -21),   S( -2, -17),   S(-14, -11),   S(  0,  12),   S( -5,  -5),   S( -7, -26),   S(-32, -49),   S(  0,   0),
            S( -8,  -4),   S(-12, -14),   S( -5,  -5),   S(-26, -10),   S( 11,   8),   S(-11, -16),   S(-36, -78),   S( -5, -11),
            S( -6,  -8),   S(-14,   1),   S(-28, -43),   S(  3,   6),   S(-28, -38),   S( -8, -10),   S(-28, -50),   S(-14, -32),
            S( -4,  -2),   S( 10,  23),   S(  6,   3),   S( 16,  28),   S(  0,  10),   S(-15, -21),   S(  2,  -3),   S( -7, -25),
            S(  8,  17),   S( 22,  56),   S(  8,  45),   S(  6,  30),   S( 28,  52),   S(-10,  -6),   S(-20, -38),   S(-28, -42),
            S( 11,  21),   S(  3,  16),   S(-17,  12),   S(  7,  32),   S( -9, -23),   S( -8,  -8),   S( -6,  -1),   S(  1,  -2),
            S(  0,  -2),   S( -8,   7),   S( -8,  18),   S( 21,  28),   S( 19,  35),   S( -9, -10),   S( -7, -12),   S( -1,  -4),
            S(-23, -46),   S(  4,  13),   S(-13, -33),   S( -3, -10),   S( -3, -25),   S(-12, -26),   S( -8, -31),   S( -1,  -5),

            /* queens: bucket 9 */
            S( -2,  -5),   S(-17, -49),   S( -8, -19),   S(-34, -35),   S(-24, -39),   S(-16, -32),   S(-18, -38),   S(-18, -40),
            S( -5, -12),   S(-10, -25),   S(-21, -10),   S(  9,  17),   S(-33, -43),   S(-27, -47),   S(-18, -43),   S(-15, -33),
            S(  4,   8),   S( 11,  16),   S(-16,  19),   S(-28, -36),   S( -3,  -4),   S( -6,   1),   S( -4,   1),   S(-10, -32),
            S(-21, -23),   S(-25, -29),   S( 12,  53),   S(  8,  26),   S( 19,  34),   S( 18,  19),   S(-20, -26),   S(  0, -16),
            S(  8,  24),   S(-16,  -1),   S( -6,  13),   S(  8,  46),   S( -1,  23),   S( 12,  31),   S( -5, -17),   S(-30, -30),
            S(-16, -19),   S(-12,   1),   S( -6,  10),   S( 18,  49),   S( -4,  -5),   S( -3,  21),   S(-16, -22),   S( -6,  -3),
            S(-18, -35),   S(-10, -15),   S( 10,  44),   S( 11,  18),   S(  7,  20),   S( -1, -26),   S( -2,  -4),   S(-15, -34),
            S(-21, -38),   S(-14, -35),   S(  2,  15),   S(  1,   6),   S(  5,  11),   S( -8, -20),   S(  2,   4),   S(-11, -18),

            /* queens: bucket 10 */
            S(-14, -31),   S( -8, -18),   S(-21, -47),   S(-32, -42),   S( -7, -14),   S( -3,  -5),   S(-12, -30),   S(-11, -34),
            S( -7, -19),   S( -7, -13),   S(-13, -23),   S(-15, -10),   S(-10,  -3),   S(-15, -23),   S(  2, -12),   S(-16, -26),
            S(-14, -31),   S(  0, -11),   S(-29, -29),   S(-14,  -6),   S(-15,   4),   S(-41, -28),   S(  1,   8),   S(-14, -27),
            S( -3,  -7),   S( -2,  -5),   S(  5,  14),   S(  6,  17),   S(  6,  26),   S(-32,  -2),   S(  7,  12),   S(-10, -18),
            S(  6,  10),   S( -7, -14),   S(-26, -20),   S(  8,  43),   S(-13,   0),   S( -1,  12),   S( 13,  16),   S(-15, -13),
            S(-10, -12),   S(-32, -47),   S( -4,  13),   S( -6,  24),   S( 24,  74),   S( 19,  40),   S(  9,  24),   S(-12, -20),
            S( -5,   0),   S(-13,  -5),   S( -2,   5),   S(-15, -10),   S( -5,  -1),   S(  6,  15),   S( -4,  -7),   S(-19, -42),
            S( -1,  -3),   S( -5,   0),   S( -8, -11),   S(  3,   9),   S( 18,  31),   S(-11, -22),   S( 10,  21),   S( -2, -18),

            /* queens: bucket 11 */
            S(-22, -34),   S(-22, -53),   S(-18, -26),   S(-24, -59),   S( -7, -11),   S(-13,  -5),   S( -3,  -1),   S(-23, -39),
            S( -2, -16),   S(-11, -11),   S(-51, -48),   S(-17,  -5),   S(-10,  11),   S(-13,  -3),   S( -1,  -1),   S(-10, -14),
            S(-21, -21),   S(-17, -31),   S( -9, -27),   S(-25, -44),   S(-19, -18),   S(  1,  10),   S( 18,  38),   S(-21, -23),
            S(-14, -40),   S(-27, -36),   S(-15, -14),   S( 14,  19),   S( -7, -22),   S( -4,  26),   S(  3,  19),   S(  5,  -2),
            S(-18, -17),   S(-14, -38),   S(-22, -19),   S( 23,  32),   S( 17,   9),   S( 40,  69),   S( 27,  48),   S(-15,  -3),
            S(-19, -46),   S(  4,  18),   S(-18, -27),   S(  8,   7),   S( 20,  28),   S( 61,  62),   S( 19,  16),   S( -8,  -1),
            S(-15, -23),   S(-19, -19),   S( 21,  44),   S(-15, -12),   S(-17,  -7),   S( 11,  13),   S( 45,  70),   S(-19, -33),
            S(  3,  -3),   S(-23, -36),   S(-18, -30),   S( -8, -26),   S( 11,  15),   S(-18, -34),   S( 15,   6),   S(-15, -42),

            /* queens: bucket 12 */
            S(  4,   4),   S(  7,   9),   S( -9, -15),   S( -9, -21),   S(-12, -21),   S( -3, -10),   S( -3,  -5),   S( -5, -11),
            S(  1,   2),   S( -6,  -9),   S(-10, -21),   S(-10, -19),   S( -5,  -7),   S( -5,  -6),   S( -6, -17),   S( -8, -16),
            S(  0,   0),   S(  1,   2),   S( 16,  19),   S(-16, -23),   S( -9, -16),   S(-16, -22),   S(-15, -30),   S(-12, -24),
            S(  7,  11),   S( -2,   0),   S( -1,   4),   S(  6,  17),   S( 11,  28),   S(  1,   2),   S( -2,  -2),   S( -7, -15),
            S( -8, -16),   S( 12,  24),   S( 34,  69),   S(  2,  20),   S( -8,  -2),   S(  9,  26),   S(-18, -36),   S( -2,  -1),
            S( 10,  23),   S(  2,   3),   S( 30,  57),   S( -9,  -6),   S(  3,  11),   S(  0,   1),   S( 16,  26),   S(-11, -27),
            S(  1,   4),   S(  9,  22),   S( 11,  20),   S( 17,  38),   S( 10,  13),   S(  2,  -1),   S(  4,   8),   S( -6, -13),
            S(-21, -49),   S(-15, -36),   S(-35, -49),   S(-17, -49),   S( -3,  10),   S( -2,   1),   S(  4,  13),   S(-11,  -6),

            /* queens: bucket 13 */
            S( -3,  -9),   S( -5, -13),   S( -1,  -1),   S(-11, -20),   S( -8, -22),   S( -4, -13),   S(-19, -44),   S(-14, -25),
            S(  4,   8),   S(  4,   8),   S(  6,  13),   S( -7, -21),   S( -9, -21),   S( -5,  -9),   S( -7, -15),   S(-10, -20),
            S( -2,  -5),   S(  2,   1),   S(  1,  -5),   S(  9,   3),   S( -9, -20),   S(-12, -22),   S(-12, -27),   S( -8, -18),
            S(-11, -20),   S( -6, -16),   S( 20,  22),   S( 20,  22),   S( 13,  26),   S( -6, -21),   S(-11, -29),   S( -3, -14),
            S( -2,  -5),   S(  5,  22),   S( 10,   7),   S( 12,  30),   S( 30,  55),   S( -7,  -9),   S( -9, -30),   S(-13, -27),
            S(  6,   5),   S(  1,   2),   S( 36,  56),   S( 24,  57),   S(  3,  23),   S(-10, -16),   S(  5,   9),   S( -7, -19),
            S( -6, -14),   S( 11,  22),   S(  5,  22),   S( 17,  48),   S( -2,   1),   S( -6,  -8),   S( -8, -16),   S( 15,  32),
            S(-33, -65),   S( -2,   1),   S( -4, -15),   S(-23, -37),   S( -2, -11),   S(  5,   5),   S( -9, -17),   S(  0,   1),

            /* queens: bucket 14 */
            S( -3, -10),   S( -8, -22),   S(  1,  -2),   S(-11, -16),   S( -1,   0),   S( -6, -15),   S(-11, -23),   S(-10, -18),
            S(-13, -30),   S(  8,  15),   S( -5, -12),   S( -5, -13),   S(-12, -19),   S( -2, -15),   S( -4, -10),   S( -9, -19),
            S( -1,  -5),   S(-17, -36),   S(-24, -31),   S( -2,  -1),   S( -8, -17),   S(-15, -36),   S( -4,  -9),   S( -4,  -4),
            S(-14, -26),   S( 12,  17),   S(  5,   4),   S( 22,  38),   S( 14,  25),   S( 11,  34),   S(  5,  -1),   S(  0, -10),
            S(  6,  20),   S( -7, -13),   S(-17, -15),   S( 11,  13),   S(  9,  21),   S( 10,  10),   S( -1,  -5),   S( -5, -20),
            S(-10, -22),   S(  7,  15),   S(  9,  26),   S(  1,   5),   S( 18,  55),   S( 15,  35),   S(  2,  -1),   S(  1,  -7),
            S(  9,  23),   S( -6, -13),   S( 10,  21),   S( 16,  40),   S( 23,  43),   S( 17,  33),   S( 14,  24),   S(  7,  11),
            S( -2,  -4),   S(  3,   6),   S(-11, -21),   S(  8,   9),   S(  0,  14),   S( -5,  -9),   S(  1,   7),   S(-22, -47),

            /* queens: bucket 15 */
            S( -7, -18),   S( -2, -21),   S( -8, -13),   S(  2,  -3),   S( -3,  -6),   S(-11, -28),   S( -5, -13),   S(  3,   0),
            S( -1,  -5),   S(  5,   3),   S(-10, -23),   S( -6, -11),   S(  0,  -3),   S(  2,  12),   S( 11,  24),   S(  1,   2),
            S(  0,  -4),   S(  2,  -2),   S(  3,   6),   S(  5,   9),   S( -2,  -8),   S(  4,   9),   S( -5, -15),   S(  5,   7),
            S( -2,  -4),   S(  0,  -2),   S(-10, -11),   S( 13,  22),   S( -3,  -7),   S(  2,  11),   S( 14,  18),   S(  2,   8),
            S( -6,  -9),   S( -3,  -9),   S( -6, -19),   S( -2,  -5),   S(  8,  13),   S(  6,  15),   S( -1,  -8),   S(  1,   0),
            S(-10, -24),   S( -1,  -5),   S( -1,  -5),   S(-10, -17),   S(-13, -32),   S( 15,  22),   S( 22,  36),   S(  1,   0),
            S( -5,  -6),   S(  2,   0),   S(  9,  18),   S( 17,  32),   S( -3,  -2),   S( 13,  19),   S(  6,   1),   S( -2,  -4),
            S( -1,  -6),   S( -6, -19),   S(  0,   1),   S(  7,  11),   S(  5,   5),   S(  8,   0),   S( -1,  -9),   S(-15, -23),

            /* kings: bucket 0 */
            S( 23,  44),   S( 19,  56),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  66),   S( 96,  62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-23,  14),   S(-76,  28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11,  37),   S(  6,  40),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-46,  30),   S(-45,  26),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  46),   S( 20,  38),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19,  57),   S( -5,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 55,  83),   S(  8,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -39),   S( 20, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-75,   3),   S( 26,   7),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 10, -49),   S(-30, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  8, -13),   S(  6, -18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -9, -17),   S(-35, -11),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 22,  -8),   S(-26,   4),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 29,  17),   S(-21, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 48,  39),   S(-54,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-122, -38),  S(  3, -13),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-72, -29),   S( 45, -23),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -2, -47),   S(-37, -46),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 76, -59),   S( 39, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  4, -56),   S(-51, -35),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60, -63),   S( 87, -55),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -7, -26),   S(-110, -52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 91, -47),   S( 28, -61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -41),   S( 37, -16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55, -89),   S(  1, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -4, -24),   S( 77, -38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 33, -66),   S( 10, -59),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 46, -36),   S( 50, -45),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18, -68),   S(-10, -52),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 43, -30),   S(-55, -48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18, -56),   S( 10, -88),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -12),   S(-35,   7),   S(  1,  -5),   S(-19,  11),   S( -2,   8),   S( 27,  14),   S( 31,  -5),   S( 46, -10),
            S(-15, -27),   S(-30, -13),   S( -9, -18),   S( -6, -10),   S(  8,   7),   S(  3,   7),   S( 23,  -2),   S( 25,   7),
            S( 12, -21),   S(  2, -22),   S( 28, -22),   S(  7, -19),   S( 15,  -5),   S( 19,  24),   S(  4,  36),   S( 46,   7),
            S( 22, -18),   S( 41,   0),   S( 65, -31),   S( 39,   1),   S( 24,  45),   S( -2,  77),   S( 15,  78),   S( 78,  41),
            S( 86, -93),   S( 98, -60),   S( 93,  -9),   S( 21,  48),   S( 81,  97),   S( 11,  97),   S( 21, 165),   S( 99, 106),
            S(-242, -117), S(-217, -162), S( 46, -156),  S( 41,  35),   S( 67, 172),   S(111, 199),   S(107,  99),   S( 56, 175),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  18),   S(-49,  11),   S(-29,   5),   S(-46,  36),   S(-29,  -5),   S( -4,   4),   S(  3,  -9),   S(  4,  16),
            S(-48,  -3),   S(-38,  -7),   S(-41,  -4),   S(-21,  -5),   S( -7,   5),   S(-24,   5),   S( -9,  -8),   S(-16,   3),
            S(-30,  13),   S(-11,   9),   S(-26,   5),   S(  9, -15),   S( -2,  19),   S( -7,   6),   S(-17,  15),   S(  2,   9),
            S(-13,  28),   S( 30,   4),   S( -6,  15),   S( 17,  21),   S( 13,  19),   S(-16,  30),   S( 11,  24),   S( 47,  33),
            S( 21,  24),   S( 86, -25),   S( 84, -92),   S( 48, -89),   S( 82,   1),   S(  6,  35),   S(-23,  52),   S( 54,  79),
            S(146, -65),   S(-21, -31),   S(-29, -148),  S(-18, -145),  S(-10, -69),   S(-48,  68),   S( 64, 132),   S( 82, 151),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  35),   S(-42,  14),   S(-34,  10),   S(-23,  27),   S(-53,  44),   S(-35,  18),   S(-15, -10),   S(-23,  22),
            S(-53,  14),   S(-42,  13),   S(-44,   6),   S(-45,  19),   S(-43,  24),   S(-41,   6),   S(-24, -11),   S(-46,  10),
            S(-32,  38),   S(-32,  38),   S(-20,  21),   S(-21,  19),   S(-27,  29),   S(-20,  10),   S(-25,   6),   S(-23,   9),
            S(-14,  70),   S(-29,  56),   S( -8,  32),   S(  7,  30),   S( -4,  30),   S(-19,  18),   S( 10,  16),   S( 29,   7),
            S(-31, 121),   S(-49,  93),   S(-29,  38),   S( 34, -29),   S( 87, -80),   S( 68, -53),   S( 91, -26),   S( 49,   9),
            S(-24, 206),   S( 72, 106),   S(-22,  45),   S(-38, -58),   S(-48, -192),  S(-87, -198),  S( 14, -63),   S( 93, -53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  17),   S( -9,  12),   S( -8,  20),   S(-12,  36),   S(-21,  52),   S(  7,  38),   S( -2,  10),   S(-14,   2),
            S( -1,   8),   S(  2,  14),   S(-13,  12),   S(-19,  22),   S( -1,  26),   S(  5,  13),   S( -4,   1),   S(-32,   1),
            S( 15,  35),   S( -5,  51),   S(  3,  26),   S( -3,  11),   S( 21,  -1),   S( 19,  -1),   S(  1,  -7),   S(-12,   0),
            S( 20,  80),   S( -3,  92),   S(  9,  68),   S( 20,  30),   S( 32,   6),   S( 43, -19),   S( 19,  18),   S( 37,  -7),
            S( 15, 144),   S(-33, 169),   S(-28, 161),   S( -4, 115),   S( 51,  46),   S( 78,  -8),   S( 82, -50),   S( 78, -48),
            S( 31, 159),   S( 13, 242),   S(-22, 269),   S(-24, 190),   S(-48,  95),   S(-41, -127),  S(-193, -214), S(-238, -195),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 59,  -2),   S( 39, -16),   S(-18,  -8),   S(-12, -28),   S(-24, -15),   S(-11,  -3),   S(-27,  -3),   S(-48,  29),
            S( 60, -24),   S( 19,  -5),   S( 26, -29),   S(  2,  -9),   S(-35, -13),   S( -7, -20),   S(-42, -18),   S(-20, -16),
            S( 95, -33),   S(128, -49),   S( 44, -26),   S( -9,  -6),   S(-89,  16),   S(-25,  15),   S(-59,  15),   S(-35,  16),
            S(-62, -103),  S( 57, -137),  S( 30, -23),   S(-36,  16),   S(-41,  30),   S(-63,  70),   S(-17,  44),   S( 19,  29),
            S( 34, -102),  S(-22, -103),  S( 19, -96),   S( 21,   3),   S( 57,  93),   S( 66,  53),   S( 36,  80),   S(  7, 100),
            S( 11, -65),   S( -5, -76),   S( 16, -86),   S( 28, -10),   S( 57,  60),   S( 54, 150),   S( 65, 156),   S( 40, 121),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38,  20),   S(-20,  19),   S( 18,  -5),   S( 87, -11),   S( 59, -18),   S( 13,  -4),   S(-26,   1),   S(-78,  44),
            S(-58,  12),   S(-12,   6),   S( 18, -12),   S( 30,  -1),   S(  0,   7),   S(  4, -13),   S(-52,  -6),   S(-110,  29),
            S( -4,   7),   S( 58,   5),   S( 88, -12),   S( 12,  19),   S(  4,  24),   S( -1,   4),   S(-37,  13),   S(-52,  32),
            S( 35,   7),   S( 57, -26),   S( 10, -73),   S(-36, -28),   S( 14, -23),   S(  2,  13),   S( 31,   9),   S(  8,  29),
            S( 36,  11),   S( 67, -58),   S( 35, -82),   S( -9, -52),   S( 83, -50),   S( 42,   9),   S( 44,  27),   S(  7,  85),
            S( 61,  14),   S( 41, -24),   S(-12, -91),   S( 18, -89),   S( -5, -52),   S( 72,  19),   S( 85, 113),   S(119,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-80,  24),   S(-48,  -1),   S(  6, -13),   S(  0, -15),   S( 18,   8),   S( 20,   4),   S( 18,  -5),   S( -9,  23),
            S(-66,   0),   S(-38, -10),   S(-39,  -7),   S( 55, -14),   S(-14,  21),   S( 16, -11),   S(  6,  -7),   S( -1,  -2),
            S(-43,  22),   S(-49,  20),   S( -7,   7),   S( -3,  18),   S( 28,  18),   S( 64, -12),   S( 52,  -6),   S( 25,  -1),
            S(-27,  44),   S(-12,  27),   S( 22,   2),   S( 37,  -8),   S(-34, -43),   S( -2, -55),   S( 65, -28),   S( 93, -26),
            S( 31,  66),   S( 21,  24),   S( 29,  -2),   S( 16, -38),   S(-11, -60),   S(-25, -58),   S( 74, -53),   S(110, -10),
            S(136,  40),   S(129,  55),   S( 66,   6),   S( 91, -81),   S(-28, -106),  S(-11, -117),  S( -5, -30),   S( 64,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58,  -2),   S(-49, -17),   S( -3, -28),   S(-49,  21),   S( 38,  -3),   S( 69, -14),   S( 62, -29),   S( 51,   1),
            S(-34, -21),   S(-51, -14),   S(-47, -22),   S(-28,   0),   S( 21, -16),   S( 65, -37),   S( 40, -21),   S( 65, -24),
            S(-42,  14),   S(-51,  20),   S(-32,   5),   S(-29,  -5),   S( 10,  -2),   S( 50, -28),   S( 89, -27),   S( 71, -24),
            S( -9,  33),   S(-58,  54),   S(-26,  47),   S( -9,  23),   S( 22, -13),   S( 75, -65),   S( 10, -74),   S( 15, -112),
            S( 39,  43),   S(-30, 100),   S( 26,  94),   S( 39,  51),   S(-36,  46),   S(-15, -59),   S(-20, -115),  S( 36, -83),
            S(135,  75),   S( 82, 100),   S(117,  95),   S( 43,  84),   S( 60,  -5),   S(-40, -93),   S( -6, -76),   S( 29, -161),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 42, -10),   S( 13, -21),   S( 33,  -7),   S(-15, -38),   S( -9, -47),   S( 31, -40),   S( 27, -54),   S( 20, -33),
            S(-17, -62),   S(-40,  -8),   S(-55, -63),   S(-16, -35),   S(-36, -43),   S( 42, -51),   S(  3, -47),   S(-40, -37),
            S(-39, -50),   S( 29, -65),   S(-24, -52),   S(-28, -43),   S(-52,  -4),   S(-41,  -6),   S(-45, -11),   S(-28, -11),
            S(-10,  -2),   S(-27, -37),   S( 32, -17),   S( 42,  -8),   S(  7,   4),   S(-19,  28),   S( 11,  29),   S(-50,  59),
            S( 11,  42),   S( -8, -29),   S( 23,  26),   S( 47,  92),   S( 69, 140),   S( 58, 113),   S( 28,  70),   S( -7,  98),
            S( 27,  64),   S( 14,  68),   S( 24,  61),   S( 50,  96),   S( 29,  81),   S( 52, 152),   S( 45,  90),   S(-13,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,  -5),   S( 79, -20),   S( 59,  -7),   S( -1,   1),   S( -5, -31),   S( 71, -35),   S(104, -70),   S(-15, -34),
            S(  8, -47),   S(-45, -39),   S(-26, -65),   S(-29, -39),   S(-55, -31),   S(-27, -42),   S( -9, -45),   S( -6, -42),
            S(-75, -25),   S(-45, -47),   S(-14, -76),   S(-60, -41),   S(  9, -32),   S( -1, -47),   S(-16, -42),   S(-39, -13),
            S(-35,   5),   S(-84, -41),   S(-13, -50),   S(  8, -40),   S( 17, -36),   S(-24, -16),   S(  8,   2),   S( 30,   4),
            S(-12,   8),   S(  8, -39),   S( 10, -44),   S( 41,  31),   S( 22,  47),   S( 45,  44),   S(-13,  64),   S( 26,  44),
            S(-10,  48),   S( 26,  53),   S(  7,   5),   S( 31,  36),   S( 39,  96),   S(  8,  65),   S(  3,  65),   S( 10,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -61),   S(-21, -47),   S(  2, -25),   S( -5, -26),   S( 33, -46),   S(162, -30),   S( 48, -20),   S( 58, -32),
            S(-36, -55),   S(-52, -55),   S( 30, -71),   S( 33, -45),   S( 15, -49),   S(-11, -29),   S( 39, -51),   S( 12, -37),
            S(-68, -27),   S(-46, -51),   S(-13, -45),   S( 19, -35),   S(-52, -34),   S(-10, -66),   S( -6, -70),   S( 37, -53),
            S(  1, -23),   S(  8, -27),   S(  2, -28),   S(-16, -43),   S(-15, -32),   S(-56, -42),   S( 20, -58),   S(  9, -28),
            S(  4,  12),   S( 25,  17),   S( 25,   4),   S( -9, -23),   S( 27, -11),   S( 41,   2),   S(-30, -21),   S( 32,   8),
            S( -8,  26),   S(  7,  31),   S( 27,  39),   S( 24,  32),   S( 21,  25),   S(  4,   1),   S( -9,  -7),   S(  8,   2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -45),   S(-34, -32),   S(-21, -27),   S( 14,  16),   S(  1, -10),   S( 94,  -7),   S( 97, -34),   S( 64, -12),
            S(-76, -47),   S(-63, -56),   S(-39, -56),   S(  1, -47),   S(-28, -18),   S(  4, -41),   S(  3, -37),   S(  2, -64),
            S(-36, -14),   S(-38, -28),   S(-19,  -6),   S( 31, -47),   S(-16, -46),   S( -2, -32),   S(-34, -71),   S(  9, -65),
            S(-24,  14),   S(  0,  14),   S( 30,  22),   S( 57, -15),   S( 22, -21),   S(-32, -42),   S(-42, -40),   S(-16, -41),
            S(  1,   6),   S( 37,  50),   S( -2,  69),   S( 43,  34),   S( 47,  62),   S( 10,  -7),   S( 14,  -2),   S(-14,  21),
            S( 43,  50),   S( 51,  59),   S( 43,  93),   S( 46, 105),   S( 17,  39),   S( 37, 109),   S( 19,  41),   S( 38,  73),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -71),   S(-18, -10),   S(-14,  -4),   S(  2,  20),   S( 10,  16),   S( -5, -69),   S( 15, -17),   S( 14, -22),
            S(-23, -43),   S(-24,   9),   S(-51, -79),   S(-28, -39),   S(-39, -44),   S( -3, -45),   S(-20, -56),   S(  6, -59),
            S(-36,  -5),   S( -2, -56),   S(-18, -104),  S(-19, -58),   S(  6, -27),   S(-40, -13),   S(-46, -27),   S(-34, -24),
            S(-15,  -8),   S( 10,  31),   S(-11, -23),   S(  9,  22),   S(  3,  52),   S( 13,  88),   S(  4,  52),   S(-29,  23),
            S(  9,   6),   S( -3,   1),   S(  7,   8),   S( 18,  79),   S( 44, 118),   S( 27, 111),   S(  2, 103),   S( 11,  93),
            S( 10,   3),   S(  5, -10),   S( 19,  59),   S( 15,  44),   S( 34, 123),   S( 29, 102),   S(-19,  34),   S(-21,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -55),   S(-32, -67),   S(  2,   7),   S( -2,  -6),   S(  5,  13),   S(-14, -30),   S(-13, -61),   S(-12, -38),
            S(-78, -62),   S(-23, -62),   S(-40, -53),   S( 14,   5),   S(-38, -32),   S(-63, -50),   S(-31, -63),   S( -7, -69),
            S(-30, -48),   S(-32, -73),   S(  5, -67),   S(-15, -13),   S(  4, -26),   S(  1, -31),   S(-16, -47),   S(-39, -26),
            S(-21,  28),   S( -9, -24),   S( -4, -45),   S(  7,  13),   S(  4,  34),   S(-31,  23),   S( -8,  10),   S(-36,   9),
            S(-23,  -4),   S(  8,  30),   S(  4,  -3),   S( 21,  61),   S( 21,  83),   S( 24,  97),   S( 18, 132),   S(-13,  79),
            S( 23, 113),   S( 48, 120),   S( 11,  30),   S( 19,  42),   S( 24,  87),   S(  8,  69),   S( -3,  45),   S( 15, 112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-18, -88),   S(  5, -74),   S(-22, -60),   S(  4,  24),   S(-10, -28),   S(-10, -12),   S(  8, -54),   S(  5, -19),
            S(-19, -60),   S(-82, -90),   S(-11, -73),   S(-43, -105),  S(-22, -59),   S(-18, -43),   S(-23, -23),   S(-27, -30),
            S(-15, -51),   S(-17, -54),   S(-20, -35),   S(  1, -63),   S(-20, -56),   S(-20, -70),   S(-25, -65),   S(-54,   7),
            S(-16,  -3),   S(-31, -25),   S( 10,  16),   S( -6,   4),   S( -1, -24),   S(  6, -16),   S(  6,  -8),   S(  1,  34),
            S( -9,  17),   S(  3,  27),   S( 14,  52),   S(  7,  40),   S( 23,  53),   S(  5,  23),   S( 20,  67),   S( 18,  64),
            S(  0,  60),   S( 20,  55),   S( 22,  90),   S( 29,  82),   S( 24,  86),   S(  4,  52),   S( 32,  95),   S( 24,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,  -6),   S( -6, -39),   S( -9, -31),   S( -6, -24),   S( -5, -17),   S(-18,  -8),   S(-14,   6),   S(-27, -95),
            S(-30, -42),   S(-29, -59),   S(-32, -89),   S(-20, -52),   S(  7, -29),   S(-48, -19),   S(-41,  -1),   S(-26, -60),
            S(-44,  -6),   S(-34, -34),   S(-32, -36),   S( 22, -33),   S(-51, -47),   S(-13, -55),   S(  0, -28),   S( -8,   3),
            S(-24,  21),   S(-10,  23),   S(  7,  71),   S(  0,  11),   S( 24,  54),   S(  0,  19),   S(  1,   9),   S( -4,  22),
            S( 12,  74),   S(  8,  84),   S( 12, 112),   S( 21,  70),   S( 29, 124),   S( 13,  65),   S( 24,  83),   S(  8,  25),
            S(-27,  39),   S( -4,  36),   S( 12,  89),   S( 19,  50),   S( 25,  87),   S( 14,  51),   S( 12,  47),   S( 21,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-71, -48),   S(-25, -21),   S(-11,  21),   S( -6,  38),   S(-36, -13),   S(-34,  32),   S(-30,   3),   S(-51, -41),
            S( 23, -41),   S(-12,  -8),   S(-47,  -3),   S(-13,   6),   S(-18,  27),   S(-35,  27),   S(-34, -43),   S(-18, -24),
            S(-34,  -7),   S( 11, -13),   S( -5,  26),   S( 11,  64),   S( -1,  18),   S(-11,  33),   S(-39,  16),   S(-39, -24),
            S( 12,  17),   S( 26,  60),   S(  9,  64),   S( 36,  37),   S(  3,  60),   S(  7,  42),   S( 29, -23),   S(-20,   8),
            S( 18,  68),   S( -8,  67),   S( 33,  86),   S( 38,  43),   S( 56,  59),   S( 22,  31),   S( 10,  13),   S( -5,  30),
            S(103, -25),   S(-35,  68),   S(141,  46),   S( 71,  30),   S( 55,  53),   S( -8,  65),   S( 43,  20),   S(-27,  19),
            S( 68,  -3),   S(-16, -41),   S( 50,  38),   S( 71,  64),   S( 80,  14),   S( 42,  27),   S(-22,  -5),   S(-17,  19),
            S(-105, -153), S(  8,   2),   S( 31,  31),   S( 48,  59),   S( 23,  29),   S( 21,  36),   S(-34,   4),   S(-20,  10),

            /* knights: bucket 1 */
            S( 18,  22),   S(-63,  27),   S(-29,  28),   S(-59,  56),   S(-33,  54),   S(-12, -28),   S(-55,  -4),   S(-75,  -4),
            S(-35,  14),   S(-34,  44),   S(-45,  43),   S(-16,  23),   S(-24,  32),   S( -2,  21),   S(-23,  -7),   S(-19, -62),
            S(-27,  20),   S(-17,  19),   S(-31,  26),   S(-20,  63),   S(-16,  55),   S(-28,  28),   S(-49,  40),   S(-26,   9),
            S(-22,  76),   S( 31,  31),   S(-12,  55),   S(-22,  71),   S(-15,  63),   S(-30,  81),   S(-25,  38),   S(-37,  64),
            S( 32, -12),   S(-25,  24),   S(  0,  86),   S(-27,  71),   S( 37,  41),   S(  9,  55),   S(-23,  40),   S(-49,  91),
            S( 22,  46),   S( 99, -10),   S(107,  28),   S(102,  60),   S( 68,  34),   S(-38,  90),   S( 32,  26),   S( 23,  22),
            S( 32, -12),   S( 10,  12),   S( 37, -33),   S( 35,  75),   S( -4,  22),   S( 55,  11),   S( 13,  60),   S(-26,  88),
            S(-182, -38),  S( 20,   4),   S(-25, -72),   S( -5,  23),   S(-17,  -6),   S( 57,  48),   S( 40,  42),   S(-67,  49),

            /* knights: bucket 2 */
            S(-85,   9),   S(-37,  34),   S(-43,  17),   S(-25,  48),   S(-35,  28),   S(-57,  27),   S(-40,  21),   S( -8, -32),
            S(-44,  46),   S( -4,  37),   S(-37,  39),   S(-16,  35),   S(-32,  46),   S(-28,  30),   S( -2,  24),   S(-42,  21),
            S(-42,  51),   S(-34,  47),   S(-32,  40),   S(-32,  80),   S(-24,  69),   S(-35,  35),   S(-29,  31),   S(-30,  20),
            S(-17,  78),   S(-24,  62),   S(-31,  92),   S(-34, 101),   S(-36,  78),   S(-15,  66),   S(  1,  41),   S(-15,  50),
            S(-21,  72),   S(-29,  78),   S(-12,  85),   S( -4,  73),   S(-38,  93),   S(  1,  83),   S(-39,  80),   S( 19,  30),
            S(-32,  88),   S(-15,  65),   S(-39, 112),   S( 48,  41),   S( 21,  66),   S(125,  26),   S( 55,  42),   S(-10,  10),
            S( 14,  74),   S(-31,  68),   S( 58,  20),   S(  0,  22),   S( -7,  34),   S( 15,   4),   S( 28,  42),   S( 16,  20),
            S(-50,  47),   S( 28,  73),   S(-21,  70),   S(-38, -11),   S(-17,  -7),   S(-53, -16),   S( 23,  -1),   S(-147, -68),

            /* knights: bucket 3 */
            S(-62,  49),   S(-22,  -6),   S(-14,  20),   S(-14,  36),   S(-11,  23),   S(-18,  21),   S(-32,   8),   S(-41, -10),
            S(-26,   8),   S(-13,  42),   S(-11,  32),   S(-10,  33),   S(-14,  42),   S(  5,  17),   S(  7,  15),   S( 16, -24),
            S(-23,  42),   S(-26,  47),   S(-12,  56),   S( -6,  90),   S(  2,  75),   S(-18,  61),   S(  2,  42),   S(-15,  28),
            S( -8,  28),   S( -3,  62),   S(  3,  82),   S(-11, 104),   S( -1, 108),   S(  2, 103),   S( 10,  80),   S( -5,  72),
            S(-29,  75),   S( -3,  74),   S( 13,  96),   S( 16, 116),   S(  0,  99),   S(  1, 127),   S(-39, 124),   S( 42, 111),
            S(-24,  73),   S( 12,  77),   S( 25,  88),   S(  7, 109),   S( 57,  98),   S(107, 126),   S( 46, 117),   S( -5, 136),
            S(-18,  75),   S(-25,  81),   S(-39, 110),   S( 29,  76),   S( 29,  88),   S( 89,  40),   S(-22,  38),   S( 82,  19),
            S(-146,  74),  S(-30, 110),   S(-51, 102),   S(  3,  93),   S( 51, 109),   S(-62,  94),   S(-26, -40),   S(-68, -117),

            /* knights: bucket 4 */
            S( 12,  12),   S(-16, -10),   S(-61,  -6),   S(-37, -14),   S(-20,  21),   S(-13, -19),   S( 26, -17),   S(-28, -25),
            S( 15,  -7),   S(  6,  -6),   S( 22,   0),   S(-29,   5),   S(-18, -16),   S( 38, -54),   S( 29,  22),   S(-40,  12),
            S(  6,   0),   S( 32,  -4),   S( 25,  -3),   S( 59,  -1),   S( 40,  13),   S( 38, -31),   S( -7, -23),   S( 26, -17),
            S(-11, -56),   S( 44,  -5),   S( 69,  -7),   S( 46,  -1),   S( 42,   6),   S( 28,   6),   S(-37,  36),   S( 11,   3),
            S( 23, -36),   S(  4, -16),   S( 86,  22),   S( 62,  43),   S( 70, -13),   S( 30,  45),   S( 52,   5),   S(-38,  35),
            S(-18, -54),   S(-12, -10),   S( 48, -15),   S( 83,   2),   S(  4,  30),   S(  4,  43),   S(-21,  -1),   S( 40,   6),
            S( -8, -28),   S(-37, -42),   S(  3, -10),   S( 16,  -6),   S( 15,  20),   S( -2,   5),   S(  3,  43),   S(-34, -30),
            S(  2,  17),   S( -9, -31),   S(-16, -44),   S( 23,  11),   S( 16,  17),   S(-14,   8),   S(  2,  11),   S(-20, -13),

            /* knights: bucket 5 */
            S( 28,  43),   S(  0,  23),   S(-23,  17),   S(-29,  11),   S( 26,  21),   S( 25,  13),   S(-30,  -1),   S(  8,  -5),
            S( 35,  40),   S( 60,  18),   S( 46,  -7),   S( 31,   1),   S( 40,  -3),   S( -1,  -6),   S(-16,  54),   S(-67,  28),
            S(-11,   8),   S( -4,  -2),   S( 47,  -3),   S( 46,  15),   S( 21,  31),   S( 16,  13),   S( 13,  -3),   S(-38, -10),
            S( 24,  27),   S( 41, -25),   S( 70,  10),   S( 85,  -1),   S(103,  -2),   S( 76,  -1),   S( 57,  -2),   S( 17,  10),
            S( 80,  -6),   S( 57,  -2),   S(113, -11),   S(119, -11),   S( 80,  -9),   S( 52,  28),   S( 24,  -5),   S( 45,  27),
            S( 10, -31),   S( 33, -17),   S( -2, -24),   S( -9,   7),   S( 25,  -1),   S( 84,  -6),   S(-27,  20),   S( 43,  39),
            S( 10,  11),   S(-29, -48),   S(-11, -39),   S(-14, -25),   S(-22, -68),   S(  9,  14),   S( -9,  44),   S(-13,  26),
            S(-50, -44),   S(-21, -46),   S( 15, -15),   S(-17, -42),   S( 15,   8),   S( -8,  22),   S( 18,   6),   S(-10,  12),

            /* knights: bucket 6 */
            S( -2,  -7),   S(-30,   1),   S(  1,   4),   S(-21,  28),   S(-22,  23),   S( 11,  22),   S(-15,  43),   S(-32,  31),
            S( 16, -25),   S(  3,  44),   S( 25,  -8),   S( 42,   5),   S( 46,  10),   S( 23,   7),   S( 40,  30),   S(-50,  48),
            S( -8,   6),   S( 53,   0),   S( 69,   5),   S( 60,  26),   S( 31,  36),   S(-26,  33),   S( 11,  20),   S(  5,  11),
            S(  3,  41),   S( 93, -12),   S( 66,  16),   S( 84,  20),   S( 87,   9),   S( 92,  10),   S( 55,   1),   S(-28,  42),
            S( 12,  36),   S( 48,  14),   S(118,   3),   S(131,  11),   S(161, -24),   S( 85,  23),   S(178, -21),   S( 20,  28),
            S( 36,  15),   S( 27,   5),   S( 62,  12),   S( 31,  35),   S( 51,   4),   S( 51,   0),   S( 29,  -3),   S( 44,  -3),
            S(  2,  19),   S( 24,  26),   S( 25,  54),   S( 12,  -7),   S( 18,  28),   S( 18, -33),   S(  7,  -5),   S(  6,  41),
            S(  7,  11),   S( -6,  11),   S(  8,  42),   S( -5,  24),   S( 13, -36),   S(  1,  -3),   S( 17,  45),   S(-23, -66),

            /* knights: bucket 7 */
            S(-25, -34),   S(-19, -15),   S( 44, -26),   S(-38,  26),   S( 17, -20),   S(-32,   3),   S(  6,  10),   S(  2,  47),
            S(-37, -43),   S( 32, -41),   S(-14,  -8),   S(-20,   3),   S( 22,   4),   S( 29,   9),   S( -1,  20),   S(-17,  26),
            S( 12, -37),   S(-15, -14),   S( 12, -11),   S( 77,   9),   S( 92,  -5),   S( 66,   0),   S( 32,   7),   S( 34,   3),
            S(  8,  15),   S( 37,   0),   S( 58,  -7),   S(109,   1),   S(113, -14),   S( 97,  22),   S( 40,  18),   S( 85,  12),
            S( 13,  24),   S( 26,  18),   S( 45,  27),   S(107,  -9),   S(135,  -4),   S(154, -22),   S(196, -23),   S( 29,   9),
            S(-17,  15),   S( 41,   2),   S( 24,  19),   S( 72,  13),   S(119,  -3),   S(120,   0),   S( 40,  -2),   S(-20, -53),
            S(  0,  26),   S(-10,  -3),   S( 67,  13),   S( 19,  32),   S( 63,  23),   S( 36,  20),   S(-20, -31),   S( -6, -16),
            S(-34, -37),   S(-16, -19),   S( -9,   3),   S(  7,  10),   S( -8,  19),   S( 11,  12),   S( -1, -31),   S( -2, -13),

            /* knights: bucket 8 */
            S(  3,  -8),   S( 13, -18),   S( 11,  10),   S(-15, -33),   S(  0,  14),   S(-21, -59),   S(  4,  -6),   S( -5, -27),
            S( -7, -28),   S( -5, -37),   S(-16, -81),   S(-11, -30),   S(  0,   1),   S( -3, -11),   S(  8,  -5),   S( -7, -29),
            S(-13, -63),   S(-12, -31),   S( 24, -31),   S( 10, -29),   S( 10, -34),   S( 32,  13),   S(  4,  -4),   S(  1,  -3),
            S(-18, -84),   S( -3, -27),   S( 15,  -3),   S( 17,  -2),   S( 16, -45),   S(  8,  -8),   S(-15, -31),   S(-15, -31),
            S( -4, -21),   S( -5, -66),   S( -1, -39),   S( 17, -16),   S( 27, -17),   S(-16, -28),   S( -3,  -9),   S(  0, -22),
            S(  6,  23),   S(  9, -31),   S(  1, -30),   S(  5, -39),   S(  0,  10),   S(  7, -21),   S( -8, -29),   S(-15, -36),
            S(  2,  11),   S( -4, -44),   S(-20, -67),   S(  0,   8),   S(  1,  -4),   S( 25,  16),   S( -8, -20),   S(  1,  15),
            S(  0,  -3),   S( -3,   0),   S( -8,  -8),   S(  4,  -4),   S(  5,  31),   S( -3, -23),   S(  0,  -7),   S( -5, -11),

            /* knights: bucket 9 */
            S(-12, -57),   S( -3,  -7),   S(-13, -69),   S(-18, -58),   S(-22, -26),   S(-14, -44),   S(  4,  -6),   S( -2, -24),
            S( -8, -27),   S(-22, -68),   S(-25, -116),  S(-10, -80),   S( -8, -70),   S(-23, -67),   S(-10, -52),   S(-10, -17),
            S(-11, -42),   S(-14, -21),   S(-18, -56),   S(  6, -69),   S( -7,  -5),   S( 25, -21),   S( -1, -27),   S( -1,  -3),
            S(-20, -53),   S( -9, -62),   S(-13, -63),   S(  3, -60),   S( -7, -58),   S( 27, -31),   S(-11, -59),   S( 13, -16),
            S(  5,   0),   S(  1, -30),   S(  3, -29),   S( 17, -11),   S(  1, -73),   S(  1, -16),   S( -2, -37),   S(  4, -24),
            S(-14, -56),   S(-15, -78),   S(-11, -34),   S( 24, -36),   S( 15, -12),   S( -4, -27),   S(-13, -18),   S(-14,  10),
            S(-14, -41),   S( -2,  -4),   S( -4, -10),   S(-30, -55),   S(  4, -33),   S( 10,  16),   S(-13,  -6),   S( -9, -37),
            S( -1,  -2),   S(  1, -11),   S(  3,   7),   S( -5, -27),   S( -9, -32),   S( -4, -23),   S(  9,   9),   S(  0,  13),

            /* knights: bucket 10 */
            S( -1, -30),   S(-14, -40),   S( -6, -19),   S(-14, -23),   S(-12, -67),   S( 15, -51),   S( -1, -20),   S( -8, -19),
            S( -8, -43),   S( 11, -29),   S( -3, -72),   S(-14, -80),   S( -5, -71),   S(-11, -67),   S(-15, -18),   S(  9,  38),
            S( 14,   7),   S(  3, -36),   S(  8, -22),   S( 10, -60),   S(-11, -61),   S( 10, -30),   S(  0, -36),   S(-10, -11),
            S(  8, -37),   S(-10, -43),   S( 26, -40),   S( 33, -27),   S( -3, -51),   S( -3, -34),   S(-17, -65),   S(  0, -37),
            S( -4, -27),   S( -2, -63),   S( 18, -48),   S(  0, -42),   S(  2, -22),   S(  2, -78),   S( -7, -23),   S( -2, -30),
            S(  3,   0),   S(  7, -23),   S( -2, -36),   S( -5, -25),   S(-16, -41),   S(-20, -42),   S(-11, -43),   S(-19, -31),
            S(  7,  21),   S( -4,  -8),   S( -7, -61),   S( 11, -45),   S( -6, -16),   S(-20, -63),   S(-16, -21),   S(-11, -35),
            S(  2,  -5),   S(  2,   2),   S( -5,   3),   S( -3, -23),   S( -4, -36),   S(-10, -60),   S( 14,  27),   S(  5,  14),

            /* knights: bucket 11 */
            S( -6, -26),   S(-19, -42),   S( -7, -44),   S( -4, -33),   S(-41, -83),   S(  8,  -7),   S( -8, -10),   S( 10,  41),
            S(  4,  -3),   S(-31, -64),   S(  0, -55),   S( 33, -52),   S( 16, -33),   S( 12, -46),   S(  2, -35),   S( -6, -20),
            S(-10, -40),   S(-17, -43),   S( 12, -26),   S( 23, -20),   S( 17, -20),   S( 29,  -6),   S(  3, -40),   S( -4, -36),
            S(-15, -31),   S(  9, -22),   S( 22, -41),   S( 59, -18),   S( 40, -39),   S(  9, -33),   S( 16, -15),   S(-11, -37),
            S(-11, -13),   S(  9, -66),   S( 13, -10),   S( 19, -16),   S( 45,   5),   S( 12, -27),   S(-10, -82),   S( -9, -35),
            S( -8, -38),   S(-22, -74),   S( 25,  14),   S( 33, -35),   S( 23,   5),   S( -8, -39),   S(  5, -37),   S(  2,  22),
            S(  2,   5),   S( -7,   1),   S( -8, -39),   S(  2,   5),   S( 16,  -2),   S( 24, -26),   S(  2, -17),   S( -9, -25),
            S(  2,  -4),   S(  5,  19),   S( -4, -26),   S( -2,  19),   S( -4, -10),   S(  3, -11),   S(  7,  24),   S(  2,   8),

            /* knights: bucket 12 */
            S(-15, -51),   S(  0, -28),   S( -2, -23),   S( -3,   9),   S( -7, -13),   S( -5, -29),   S( -1,   1),   S( -1, -11),
            S( -4, -36),   S(  1,  -1),   S( -2,  -9),   S( -4, -15),   S(  6, -49),   S( -4,   0),   S(  0,  -8),   S(  3,   5),
            S( -3, -11),   S(-15, -64),   S( -2, -22),   S( -9, -83),   S( -4, -32),   S(  4,   4),   S( -1, -22),   S(-15, -54),
            S(  1,   6),   S( -2, -65),   S( -4, -18),   S(  2, -24),   S( 21,  -7),   S( 13,   7),   S(  8,  25),   S(  4,  21),
            S( -3, -14),   S( -9, -45),   S(  7, -10),   S( -2, -42),   S( 23,  34),   S(  4,  -9),   S( -4, -16),   S(-10, -25),
            S( -7, -16),   S( -2, -24),   S(  2,  -5),   S( -3, -26),   S( -5, -40),   S( -9, -26),   S(  0,  -4),   S(  2,  13),
            S( -6, -21),   S( -2,  -1),   S( -8, -11),   S( -5,  -4),   S(  3,  21),   S( -6, -19),   S( -5, -18),   S( -6, -11),
            S(  0,   2),   S(  2,  21),   S(  1,  -2),   S(  2,  -5),   S(  1,   5),   S(  4,   9),   S( -1,  -2),   S(  0,   2),

            /* knights: bucket 13 */
            S( -1,  -3),   S( -8, -32),   S( -5, -47),   S( -4, -36),   S( -2, -12),   S( -3,  -4),   S( -3, -19),   S(  2,   5),
            S( -1, -11),   S( -7, -31),   S(  2,  -3),   S( -4,  -7),   S(-10, -47),   S( -4, -15),   S(-10, -40),   S( -4, -15),
            S(  6,   9),   S(  1,   9),   S( -7, -34),   S( -3, -21),   S( 10,  27),   S(-10, -31),   S( 11,   7),   S( -1, -11),
            S(  3, -27),   S(  5,   0),   S( -3, -30),   S(  6,  -4),   S(  0, -38),   S( 17,   5),   S(  3,  -1),   S(  7,  -4),
            S(  3,  27),   S( -1, -46),   S(  2, -45),   S( -9, -63),   S(-10, -42),   S(  6, -24),   S(-12, -41),   S( -6, -33),
            S( -2,  -5),   S(  2,  10),   S( -8, -15),   S( -1, -26),   S( -7, -30),   S( -7, -55),   S(  6,  15),   S( -2,  -8),
            S(  0,  -5),   S(  5,  18),   S( -7, -11),   S(  6,   7),   S(  2,   3),   S(  1,   4),   S( -1,  -5),   S( -2,  -5),
            S(  0,   1),   S(  1,   8),   S( -1,  -7),   S(  1,  -1),   S( -1,   2),   S( -1,  -6),   S( -2,   1),   S(  0,   0),

            /* knights: bucket 14 */
            S( -1, -15),   S( -5, -22),   S(  7,  28),   S( -3,  -8),   S( -9, -52),   S( -2,   7),   S(  5,   4),   S( -1, -12),
            S( -3, -13),   S( -7, -41),   S( -1, -14),   S(  4, -21),   S(  1, -21),   S(  0,   5),   S( -6, -35),   S(  5,  51),
            S( -3, -19),   S( -7, -42),   S(  8,   2),   S( -3, -42),   S( -3, -46),   S( -5, -29),   S( -2, -16),   S(  5,  23),
            S( -1, -13),   S( -1, -47),   S(-11, -41),   S( -5, -15),   S( 15,  15),   S( -6, -63),   S(  6,  -3),   S(  0,   5),
            S(  4,   2),   S(-13, -49),   S(-13, -45),   S( -9,  -8),   S( -8,   6),   S( -2, -17),   S( -4,  -3),   S( -4, -19),
            S(  0,  -2),   S(  7,  11),   S(  5,  69),   S(  7,  -6),   S(  2, -21),   S( -9, -11),   S( -7,  -7),   S( -6, -11),
            S( -1,   0),   S( -6, -23),   S(  4,   0),   S( 12,  59),   S(  4,   2),   S( -6, -23),   S(  3,   0),   S(  5,   8),
            S(  0,   0),   S(  1,   2),   S(  0,  -1),   S(  2,  12),   S( -3, -16),   S(  0,   3),   S(  2,   1),   S(  0,   2),

            /* knights: bucket 15 */
            S( -1,  -2),   S(  1,   1),   S(  6,  30),   S(  2,   7),   S( -9, -24),   S( -7, -43),   S( -1, -32),   S( -4, -29),
            S(  0,   5),   S(  4,   2),   S( -7, -18),   S(  7,  24),   S(  4, -19),   S( -3, -33),   S( -5, -27),   S(  1,  -3),
            S(  1, -10),   S( -4, -28),   S( -8, -32),   S( -7, -67),   S(-18, -74),   S(  0, -16),   S( -3, -23),   S(  1,  -4),
            S( -1, -13),   S(  1,   6),   S( -4, -20),   S(-11, -45),   S( 18, -23),   S( -7, -30),   S( -4, -24),   S( -1,   4),
            S(  1,  -7),   S( 17,  41),   S(-10, -32),   S( -7,  -6),   S( 14,   2),   S( 11,  16),   S(  6, -20),   S(  3,  13),
            S(  1,  12),   S( -3,  -1),   S(  0,  -5),   S(-14, -47),   S( -5, -27),   S(  1,  26),   S( -1,  11),   S(  3,  19),
            S( -2,  -7),   S( -5, -14),   S( -2,  -3),   S( 10,  10),   S( -1,  11),   S(  4,  19),   S(  0,  -3),   S(  1,   4),
            S(  3,   8),   S( -2, -17),   S(  3,  14),   S( -3,  -4),   S(  1,   2),   S(  3,   9),   S(  0,  -1),   S(  1,   5),

            /* bishops: bucket 0 */
            S( 60, -57),   S(  8,   2),   S( -6,  -6),   S(-13, -18),   S(  2, -17),   S( 17, -14),   S( 84, -84),   S( 29,  35),
            S(-42,  -6),   S( 16, -21),   S(-23,  27),   S( 17, -13),   S( 14,  -2),   S( 76, -57),   S( 44,  21),   S( 64,   0),
            S( 32,  -7),   S(  9,  21),   S( 29,   2),   S( 27,  -5),   S( 52, -26),   S( 44,  21),   S( 46,  -6),   S( 30,   0),
            S( -7,  -9),   S( 51, -56),   S( 18,  -1),   S( 84, -34),   S( 63,  21),   S( 52,  16),   S(  5,   2),   S(  6,  15),
            S( 40, -20),   S( 42, -25),   S(104, -31),   S( 94,   6),   S(139, -57),   S( 26,  16),   S( 62, -15),   S(  8,   3),
            S( 32,  42),   S(135, -29),   S( 70,  -5),   S( 39,  -4),   S( -4,  22),   S( 30,  21),   S( 47,   3),   S( -7,   8),
            S(-65, -117),  S( 57,  -8),   S(110,  19),   S(-15,  22),   S( 42, -28),   S( 48,  -6),   S( 19,  24),   S(-29,  23),
            S(-24, -42),   S(  2,   0),   S(  6, -58),   S(-32, -22),   S(-38, -27),   S(-21,   1),   S(-16,   9),   S(-56, -47),

            /* bishops: bucket 1 */
            S(-13,  -7),   S(  8,  -9),   S(-24,  30),   S( 27,  -9),   S(-12,   7),   S( 25,  -1),   S( 12,   1),   S( 33, -20),
            S( 22, -15),   S(-10,  -7),   S(  1,  -5),   S(-15,   8),   S( 39, -19),   S(  5,  -2),   S( 63, -13),   S(-10,   4),
            S(-29,  20),   S( 34, -11),   S(  3, -16),   S( 37,  -8),   S( 13, -15),   S( 50, -16),   S( 13, -16),   S(103, -43),
            S( 42, -20),   S( 51, -20),   S( 27,  -2),   S( 22,  -5),   S( 69, -11),   S(  8,  -4),   S( 70, -27),   S( 13, -11),
            S( 26, -34),   S( 60, -23),   S( 12,  -2),   S( 93, -17),   S( 55, -16),   S(117, -45),   S( 21,  -9),   S( 43,  -3),
            S( 60, -28),   S( 48, -26),   S( 73,  -5),   S( 83, -41),   S(132, -68),   S(-10,   1),   S( 18,  11),   S(-23, -13),
            S( 20, -75),   S( 25, -31),   S( -9, -46),   S(-18, -39),   S( 57,  -5),   S( 18,  15),   S(-11,   5),   S(-25,  16),
            S(  9, -31),   S( -8,  -3),   S(  2, -50),   S(-54,  -6),   S(-14, -30),   S( -7, -21),   S( 14, -13),   S(-39, -32),

            /* bishops: bucket 2 */
            S( -2,  10),   S(  1, -18),   S(  9,  10),   S( -7,  14),   S( 17,  25),   S( -3,  15),   S( 34, -10),   S( 18,  12),
            S( 17,  -3),   S( 21, -14),   S( -1,  -3),   S( 21,  10),   S(  1,  18),   S( 19,   4),   S( 17,  -2),   S( 22, -46),
            S( 36,   5),   S( 31, -10),   S(  9,  16),   S(  9,   1),   S( 12,  17),   S(  8, -12),   S(  7, -13),   S(  1,   4),
            S( 22, -14),   S( 48, -11),   S(  4,   7),   S( 41,  16),   S( 13,   8),   S( -6,  27),   S(-13,   0),   S( 13,   4),
            S( 18,  11),   S( 12,  19),   S( 66, -16),   S( 35,  10),   S( 19,  33),   S( 18,  15),   S( 14,  28),   S( 16,   0),
            S(-25,  31),   S(  8,  21),   S( -1,  -4),   S(106, -39),   S( 83, -25),   S(104,  -2),   S( 64,  -5),   S( 19, -32),
            S(-25,  44),   S( 31,  -8),   S(  5,  12),   S( 49,  -2),   S(-48, -41),   S(-58,  -8),   S(  1,  19),   S(-18, -34),
            S(-67,  -4),   S(-14,  24),   S( -8,  10),   S(-36,  33),   S( -4,  -1),   S(-15,   5),   S(-38, -20),   S(-45, -25),

            /* bishops: bucket 3 */
            S( 25,  35),   S( 56, -26),   S( 14, -12),   S( 15,  12),   S( 10,  30),   S(  7,  38),   S( -4,  58),   S(  5,  -3),
            S( 33,  13),   S( 30,   0),   S( 20,   4),   S( 23,  11),   S( 25,  12),   S( 24,  14),   S( 14,  16),   S( 31, -20),
            S(  2,  13),   S( 33,  38),   S( 27,  31),   S( 28,  24),   S( 25,  28),   S( 19,  23),   S( 24,   5),   S( 19,  33),
            S(  4,  11),   S( 24,  20),   S( 26,  44),   S( 43,  34),   S( 42,  29),   S( 20,  28),   S( 37, -15),   S( 39, -30),
            S( 17,  11),   S( 14,  47),   S( 15,  38),   S( 56,  52),   S( 52,  43),   S( 38,  34),   S( 22,  28),   S(-12,  43),
            S( 23,  16),   S( 38,  32),   S( 21,  24),   S( 20,  36),   S( 51,  24),   S( 53,  59),   S( 40,  69),   S( 17,  93),
            S(-15,  51),   S( 21,  49),   S( 15,  22),   S( 18,  39),   S( 14,  47),   S( 43,  61),   S(-37, -29),   S( 22, -24),
            S(-52,  48),   S(-56,  62),   S(-87,  56),   S(-62,  74),   S( 13,  25),   S(-112,  57),  S(  9,  18),   S(  5,  27),

            /* bishops: bucket 4 */
            S(-32,  31),   S(-23,  22),   S(-68,  26),   S(-62,  33),   S(-48, -36),   S(-19, -29),   S(-18, -19),   S(-36, -69),
            S(-11,  58),   S( -5,   2),   S( 43,  -5),   S(-12, -18),   S(-18, -15),   S( 12, -54),   S( 16, -17),   S(-20, -22),
            S(-24,  25),   S(-71,  -3),   S( 17, -26),   S( 17, -18),   S( 26, -19),   S(-58,   6),   S(-37, -36),   S(-63,  -9),
            S( 40, -23),   S( 16,  -5),   S( 41, -36),   S( 18,   2),   S( 16,  -6),   S( 59, -15),   S(-55, -10),   S(  0,  -4),
            S( 33,   8),   S(-16, -82),   S( 38, -53),   S( 64, -39),   S( 29,  -9),   S( 29,   0),   S(-20,  28),   S(-36, -25),
            S(-57, -73),   S(-28, -44),   S( 19, -35),   S( 29, -41),   S(-21,  -7),   S( 30,   7),   S(-11,   6),   S( -5,  33),
            S(  2, -10),   S( -2, -33),   S( 10, -44),   S( -3, -44),   S(-29, -15),   S( 10,  19),   S( -5,  24),   S( 19,  59),
            S( -8,  -5),   S(  0, -27),   S( -5, -45),   S(-12, -57),   S(  9, -42),   S(-17, -12),   S(  9,  48),   S( 11,  23),

            /* bishops: bucket 5 */
            S(-41,  34),   S(-15,  -7),   S(-50,  19),   S(-26,   4),   S(-13,  -1),   S(-57,   0),   S(-24,   1),   S(-15, -12),
            S(-31,   5),   S(-26,   6),   S( 14, -14),   S( 14,  -5),   S(-51,  14),   S( -4,   9),   S(-32,   5),   S( 17,   1),
            S( 27,   8),   S(-38, -10),   S( 36,  -8),   S( -4,   3),   S(  1,   7),   S(-65,   7),   S( 30,  -5),   S(-31,  13),
            S( -8,   5),   S(-14,  -7),   S( 60, -19),   S( 80, -18),   S(-47,  17),   S( 44, -15),   S(-88,  22),   S(-14,  33),
            S( 18, -25),   S( 22, -19),   S(  0, -32),   S( -9, -38),   S( 31, -33),   S(  7,  -7),   S( 14,  11),   S(-30,   7),
            S(-33, -16),   S(-15, -10),   S( 57, -39),   S(-46, -20),   S(  3, -13),   S( 13, -15),   S(-21,   3),   S(-22,  10),
            S(-27, -18),   S(-28, -27),   S( -8, -12),   S( -3,  -6),   S(  6, -33),   S(  5,  -1),   S(  0,  15),   S(-24,  12),
            S(-22, -21),   S(-24, -28),   S(  1, -11),   S( -2, -32),   S(-33,   6),   S( -3,   6),   S(-33,  -6),   S( -5,  31),

            /* bishops: bucket 6 */
            S(-61,  25),   S(-48,  20),   S(-17,  -4),   S(-30,  17),   S(-35,  13),   S( -8,  12),   S(-74,  36),   S(-78,  35),
            S( -5, -17),   S(-15, -20),   S(-40,  16),   S(  4,   3),   S(-22,  13),   S(-26,  11),   S(-57,  28),   S(-49,  12),
            S( -7,  12),   S(-55,  21),   S(  1,  -4),   S(-21,  18),   S(-17,  19),   S(-11,  -3),   S(-66,  28),   S(-30,  30),
            S( -6,   8),   S(-27,   2),   S(  5,  -1),   S( 78, -16),   S( 43,  10),   S( 12,  12),   S( -2, -14),   S(-31,  12),
            S(-64,  22),   S(  1,   4),   S( 39, -19),   S( 66, -19),   S(-22, -25),   S(-14, -13),   S( 11,  -6),   S(  1, -16),
            S(-57,  33),   S(-24,  11),   S(-32,  11),   S( 24,  -7),   S( 15,  -8),   S(-13, -15),   S( 33,  -2),   S(-27,  -9),
            S(-43,  26),   S(-30,  21),   S(-11,   2),   S(-29,  -2),   S(-44,  11),   S(  1,  -5),   S(  9,  -6),   S(-53,   6),
            S(-31,  13),   S(-16,  -9),   S(-45,  13),   S( -5,   5),   S(-34,   6),   S( 20, -38),   S(-27,   1),   S(  2,  -5),

            /* bishops: bucket 7 */
            S(-28,  -4),   S(-72,  -1),   S(-63, -22),   S(-21,   1),   S(-34,  -2),   S(-39,  10),   S(-55, -12),   S(-60,  17),
            S( 18, -61),   S( 13, -37),   S(  7, -22),   S(-32, -14),   S(-26,   9),   S(-37,  -3),   S(-51,  -1),   S(-21,  17),
            S(-48, -21),   S(-45,   8),   S( -8,  -2),   S(  2,  -1),   S(  2, -16),   S(-18, -13),   S(-107,  25),  S(-94,  61),
            S(-14, -25),   S(-61,  10),   S( -7, -11),   S( 14, -10),   S(110, -16),   S(  2, -13),   S( 32, -21),   S(-17,  -4),
            S(-56,  -7),   S( 13,  -5),   S( 13, -23),   S(  9,  -3),   S( 56, -19),   S( 76, -35),   S(-17, -34),   S(-50,  -6),
            S(-56,  13),   S( 24,   6),   S( 11,  -2),   S(-69,   7),   S(-21,   8),   S( 69, -36),   S(  8, -10),   S(-55, -55),
            S(-26,   3),   S(-48,  24),   S(-63,  19),   S(-17,  11),   S(  0, -26),   S( 10, -44),   S(  1, -12),   S(-29, -47),
            S(-34, -12),   S(-43,  19),   S(-35,  26),   S( 14, -12),   S( -8, -17),   S( -2, -25),   S(  8, -19),   S(-10, -21),

            /* bishops: bucket 8 */
            S( 33, 126),   S(-30,  15),   S(-18, -11),   S(-20,  17),   S(  2,  19),   S(-16, -29),   S(-16, -44),   S( -5, -28),
            S( -7,  35),   S(  8,  63),   S( -1,  10),   S( 11,  -1),   S( 26,  -9),   S(  2,   4),   S( -9, -27),   S( -9, -35),
            S(-16, -54),   S( -4, -35),   S( 12,  67),   S( 10,  -1),   S( 10,  -5),   S( 41,   9),   S(-13, -30),   S(-30, -69),
            S( -4,   2),   S( 28,  79),   S( 11,   5),   S( 17,  43),   S( 20,   6),   S(  2,  14),   S(-13,  11),   S( -4, -15),
            S(  2,  52),   S( 23,  93),   S( -9,  21),   S( -2,  -4),   S( -4,  36),   S(-15,   7),   S(  4, -19),   S(  1,  18),
            S(-14, -31),   S( -3,  14),   S( 13,  32),   S(  1,  23),   S(  8,  15),   S( 16,  64),   S( -9,  15),   S(  9,  27),
            S( -3,  20),   S(-14, -39),   S( 17,  40),   S( -4,  48),   S( -2,  33),   S( 12,  50),   S(  5,  63),   S(-24,   8),
            S(  1,  20),   S(  7,  20),   S( -7, -16),   S(  2,  34),   S( 16,  62),   S( 14,  36),   S( -1,  36),   S( 12,  89),

            /* bishops: bucket 9 */
            S(  5,  75),   S(-14,  42),   S(-26,  33),   S(-34, -18),   S(-26, -28),   S(-11, -16),   S( -1,   5),   S(-13, -25),
            S( -4,  29),   S(-24,  19),   S( -2,  15),   S(  1,   4),   S(-23, -34),   S( -2, -13),   S(-15,   3),   S(-21, -60),
            S(  2, -18),   S( 11,  12),   S( -5,  -5),   S(-13,   7),   S(  0,   9),   S( -1,  -4),   S( -3, -16),   S(-23, -26),
            S(  1,  41),   S(-10,  23),   S( -6,  26),   S(-12,  16),   S(-19,  43),   S( -6,   1),   S( -4,  12),   S(  6, -22),
            S( 10,  45),   S(-20,  16),   S(  7,  16),   S(-20,  -7),   S(-29,  12),   S(-10,   4),   S(  3,  10),   S(-20, -21),
            S(  3,  29),   S(-26,   2),   S( -5,  24),   S( -4,  22),   S(-20,  -7),   S(-19,  22),   S(  8,  11),   S(-10,  -1),
            S(-10,   8),   S(-21,  26),   S(  4,  13),   S(-12,  21),   S(  4,  19),   S(  9,  33),   S( 13,  39),   S(  0,  50),
            S(  5,  38),   S( -5, -14),   S( -4,  15),   S(  1,  13),   S(  2,  21),   S(-12,  34),   S( -2,  42),   S( 23,  98),

            /* bishops: bucket 10 */
            S( -5,  23),   S( 10,  58),   S(-11, -22),   S(-37, -29),   S(-55, -18),   S(-53, -41),   S( -8,  30),   S(-10,  15),
            S( -6,  15),   S(-15, -18),   S(-24, -31),   S(-38, -58),   S(-21, -13),   S(-39, -29),   S(-30, -33),   S( -3,  45),
            S( -7, -28),   S(-26, -40),   S(-16, -22),   S( -5,   5),   S(-10, -54),   S(  5, -25),   S(-13,  15),   S(  2, -50),
            S(-13, -22),   S(-27, -27),   S(-59, -16),   S( -2,   8),   S(-16,  -3),   S(  7,   0),   S( -3,  22),   S(-22,  17),
            S( -8,  15),   S(-33,  12),   S( -3,  -7),   S( -4,  -4),   S( 10,  -8),   S(  8,  27),   S(-25,  11),   S( -5,  21),
            S(-11,  10),   S(-14,  16),   S(-12,   9),   S(-20,  13),   S(-24, -15),   S(-20,   8),   S(-15,  29),   S(  6,  39),
            S( -1,  35),   S(-10,  30),   S(-21,  14),   S( -7,  25),   S(-16,   5),   S(-32, -29),   S( -7,  16),   S( 13,  54),
            S(  0,  56),   S( -6,  34),   S( 10,  52),   S( -9,  11),   S(-20,  20),   S( -7,   4),   S(  2,   6),   S(  3,  16),

            /* bishops: bucket 11 */
            S( 17,  10),   S(-25,  -6),   S(-16,  19),   S(-22,   2),   S(-29, -43),   S( 10,  -2),   S(-28, -16),   S(-16,  99),
            S( -3,  18),   S( 10,  -5),   S(-18,   2),   S(-13,  -7),   S( -1,   2),   S(-40,   6),   S(-41,  15),   S(  8,  26),
            S(  4, -34),   S( -7, -25),   S( -6, -17),   S(-28, -25),   S(-19,   1),   S( 26,  43),   S(-10, -37),   S(  5,   1),
            S(  2, -23),   S(-11, -46),   S(  0,   4),   S(-23, -28),   S( 10,   9),   S( 26,  23),   S( 12,  31),   S(-11, -45),
            S(-43, -11),   S( -5,   5),   S(-30,  24),   S(-36,  41),   S(  7,  -9),   S(  1,  15),   S( -8,  16),   S(  1,  27),
            S( -4,  18),   S(-13,  14),   S(-16,  46),   S( -9,  18),   S(-12,  64),   S( -9,  30),   S(  1,  18),   S(-16, -26),
            S(-29,   1),   S( 10,  98),   S( -1,  39),   S( -1,  34),   S( -4,   9),   S(-14,  20),   S(-15, -57),   S(-12, -16),
            S(  2,  89),   S(-21,  11),   S( 12,  56),   S( -1,  33),   S(  7,  43),   S( -6,  -4),   S( -6,   0),   S(  6,  15),

            /* bishops: bucket 12 */
            S( -6, -25),   S( -9, -30),   S( -6,  -3),   S(  5,  17),   S( -9,  -4),   S(-11, -10),   S(  1,   5),   S( -3, -12),
            S(  2,   7),   S(  9,  40),   S(  5,  13),   S(-15, -26),   S(  7,  -1),   S(  3,  -8),   S(-15, -23),   S( -2, -17),
            S( 10,  41),   S( 14,  72),   S( 12,  52),   S( 18,  21),   S( 10,  23),   S(  5, -45),   S(  6, -17),   S( -3,   2),
            S(  8,  65),   S(  9,  69),   S(  5,  37),   S( 14,  38),   S( 15, -22),   S( -1,  -8),   S(  1,   7),   S(  1,   3),
            S( 11,  17),   S(  3,  28),   S(  6,  41),   S( 17,  48),   S( 20,  48),   S( 12,  25),   S(  8,  14),   S(  7,  20),
            S( -5, -11),   S(-11, -20),   S(  0,  20),   S(  1,   6),   S(  4,  68),   S(  4,  20),   S( -7, -22),   S(  4,   6),
            S( -4,  -4),   S(  3,  17),   S(  3,   4),   S(  6,  17),   S( -3,  30),   S( 14,  66),   S( 24,  64),   S( -2,  19),
            S(  1,   6),   S(  0,  13),   S(  3,   8),   S( -2,  -8),   S(  3,  17),   S( 10,  40),   S(  8,  61),   S(  5,  31),

            /* bishops: bucket 13 */
            S( -6,  -7),   S(  0,   7),   S(-10, -40),   S(-11,  -3),   S(  3,  30),   S(-21, -54),   S(-11, -27),   S( -4, -21),
            S( -7,   1),   S( -4,   1),   S(  0,  15),   S(  3,  58),   S(-14,  -3),   S( 11,   1),   S(  9,  -8),   S(  1,  -3),
            S( -1,  14),   S( 16,  63),   S(  7,  28),   S( 22,  32),   S(  3,  15),   S( 15,  31),   S(-14, -42),   S(-17, -39),
            S(  9,  64),   S(  5,  49),   S( -7,  47),   S(-19, -23),   S( 10,  36),   S(-13, -34),   S( 16,  56),   S(  0,   9),
            S( -1,  53),   S(  3,  27),   S(  6,  15),   S(  9,  24),   S(  5,   1),   S( -1,  39),   S(  3,  -7),   S( -6,  -7),
            S( -9,  11),   S( -3,   9),   S( -4,  22),   S( 13,  38),   S( -5,  65),   S( -3, -19),   S(-12, -24),   S(  4,  23),
            S(  6,  41),   S( -8, -18),   S( -8, -21),   S(  0,  20),   S(  1,  27),   S(  7,  74),   S( -1,  14),   S(  5,  59),
            S(  1,   3),   S( -6,  -3),   S(  1,  -4),   S(  4,  19),   S(  2,  25),   S( -4,  -5),   S(  6,  57),   S(  5,  30),

            /* bishops: bucket 14 */
            S(-11, -13),   S(  6,  16),   S( 15,  22),   S( -2,  24),   S( -8,   1),   S(  4,   9),   S( -6, -18),   S( -5, -16),
            S(  1,  20),   S(  4,  -2),   S( -2,   9),   S( -6,  -4),   S(  9,  50),   S(  3,  32),   S( -4,  11),   S(  3,  35),
            S( -3,   1),   S(  4,  -1),   S(-13,  -6),   S(  4,  11),   S(  8,  49),   S( -9,  24),   S(  7,  59),   S( -1,  44),
            S( -2,  22),   S(  7,  29),   S(  1,  27),   S(  1,  46),   S( -6,  -7),   S( 10,  47),   S(  4,  64),   S(  3,  29),
            S(  4,  31),   S(  2,  24),   S(-12,  15),   S(  3,  41),   S(-12, -19),   S(  0,  17),   S( 20,  54),   S( -8,  11),
            S(  1,  41),   S( 13,  34),   S(  4,  39),   S(  7,  11),   S(  6,  48),   S( -3,  17),   S(  1,   4),   S(  7,  31),
            S( 14,  77),   S(  1,  25),   S(  5,  46),   S(  0,  10),   S( -3,  -6),   S(  0,  -5),   S( -5, -25),   S(  2,  14),
            S(  5,  42),   S(  4,  58),   S( -1, -12),   S(  0,   2),   S(  2,   0),   S(  1,  -7),   S(  7,  25),   S(  1,   2),

            /* bishops: bucket 15 */
            S( -2,  -2),   S( -8, -18),   S(-14, -29),   S(  0, -13),   S(-11, -25),   S(  3, -23),   S( -7, -23),   S( -4, -17),
            S( 10,  33),   S(  6,  22),   S(  0,  -5),   S( 11,  15),   S(  5,  -7),   S( -7,  -2),   S( -3,  -2),   S( -5,  -4),
            S( 12,  20),   S( -2,  -4),   S( -3,  13),   S(  2,  -6),   S( 17,  37),   S( 13,  37),   S( 11,  40),   S(  3,  27),
            S( -1,   1),   S(  9,  18),   S(  2,  17),   S(-11, -16),   S(  3,  32),   S(  0,  18),   S( 14,  53),   S(  5,  53),
            S( -4,  -5),   S( -3, -12),   S( 11,  47),   S( 16,  76),   S(  0,  36),   S(  8,  31),   S(  6,  39),   S( -2, -13),
            S(  7,  18),   S(  2,  34),   S( 13,  81),   S(  3,  24),   S( 16,  41),   S(  4,  53),   S(  1,  15),   S(  3,  11),
            S( -2,  -6),   S(  7,  28),   S( -1,  44),   S(  9,  44),   S(  6,  22),   S( -6, -17),   S(  0,  10),   S( -2,  -5),
            S(  0,   3),   S(  1,  27),   S(  2,  40),   S(  4,   9),   S(  3,  21),   S(  2,  18),   S(  4,  14),   S(  4,  26),

            /* rooks: bucket 0 */
            S(-13,  15),   S( 12,  15),   S(  1,  16),   S(  0,  27),   S(-29,  83),   S( -7,  45),   S(-41,  66),   S(-55,  61),
            S(-15,  18),   S(  2,  34),   S(-27,  28),   S( -6,  37),   S(  9,  49),   S(  4,   9),   S(-10,  25),   S(-30,  80),
            S( 20, -27),   S(  1,  20),   S( -5,  30),   S( -7,  18),   S(-24,  61),   S(-24,  41),   S(-22,  49),   S( 12,  15),
            S(-13,   9),   S( 24,  22),   S(-54,  54),   S(  8,  24),   S( -4,  64),   S(-20,  49),   S(-25,  57),   S( -8,  35),
            S( 39, -47),   S( 40,  27),   S(  4,  31),   S( -8,  75),   S( 37,  40),   S( 20,  75),   S( 42,  64),   S( 15,  52),
            S( 18,  -4),   S( 78,  44),   S( 80,  30),   S(124,  30),   S( 31,  67),   S( 42,  45),   S( 21,  65),   S(-40,  82),
            S( 45,  52),   S( 57,  85),   S(131,  49),   S( 33,  50),   S(110,  43),   S( 31,  70),   S(-21,  91),   S(  5,  82),
            S( 10, -17),   S( 26,  27),   S( 24,  48),   S( 60,   4),   S( 74,  52),   S( 73,  27),   S( 65,  36),   S( 30,   2),

            /* rooks: bucket 1 */
            S(-56,  48),   S(-21,  15),   S(-11,  26),   S(-33,  44),   S(-35,  49),   S(-37,  48),   S(-48,  66),   S(-81,  75),
            S(-70,  31),   S(-22,   7),   S(-38,  38),   S(-24,  22),   S(-51,  41),   S(-71,  63),   S(-51,  56),   S(-37,  45),
            S(-34,  30),   S(-39,  19),   S(-35,  21),   S(-26,  31),   S(-53,  45),   S(-42,  35),   S(-73,  70),   S(-21,  50),
            S(-43,  49),   S(-14,  26),   S(-52,  62),   S(-25,  26),   S(-64,  60),   S(-80,  81),   S(-31,  45),   S(-84,  78),
            S(-35,  53),   S(-17,  22),   S( -4,  41),   S( 23,  34),   S( -5,  35),   S( -9,  72),   S(-32,  81),   S( -4,  68),
            S( 68,  27),   S( 52,  10),   S( 65,  11),   S( 21,  53),   S(-12,  44),   S( 28,  57),   S( 14,  55),   S(  0,  87),
            S( 30,  80),   S( 33,  19),   S( 54,  59),   S( 55,  55),   S( 24,  52),   S( -3,  64),   S( 42,  81),   S( 33, 101),
            S( 83,  -1),   S( 73, -17),   S( -5,  -5),   S(-13,  -2),   S( 54,   7),   S( 57,  23),   S( 73,  35),   S( 79,  29),

            /* rooks: bucket 2 */
            S(-65,  78),   S(-49,  74),   S(-44,  78),   S(-34,  50),   S(-34,  56),   S(-42,  47),   S(-34,  42),   S(-74,  69),
            S(-62,  71),   S(-65,  74),   S(-44,  69),   S(-59,  68),   S(-51,  57),   S(-69,  49),   S(-30,  29),   S(-44,  50),
            S(-53,  82),   S(-36,  70),   S(-44,  53),   S(-41,  59),   S(-39,  50),   S(-37,  41),   S(-34,  33),   S(-20,  63),
            S(-56,  96),   S(-35,  80),   S(-35,  74),   S(-73,  77),   S(-65,  69),   S(-48,  61),   S(-41,  54),   S(-20,  41),
            S(-27,  94),   S(-53, 109),   S(-21,  90),   S(-26,  71),   S(-37,  81),   S( 12,  52),   S(-20,  67),   S(-34,  92),
            S( 30,  95),   S( 15,  93),   S( 12,  90),   S(  0,  69),   S( 77,  37),   S( 25,  92),   S(108,  30),   S( 95,  55),
            S( 71,  80),   S(  6, 104),   S( 58,  75),   S( 69,  40),   S( 58,  35),   S( 45,  85),   S(  4,  92),   S( 15, 101),
            S( 18,  73),   S( 61,  68),   S( 62,  51),   S( 21,  36),   S(-17,  56),   S( 46,  41),   S( 31,  49),   S( 36,  53),

            /* rooks: bucket 3 */
            S(-21, 105),   S(-16, 112),   S(-17, 137),   S( -7, 125),   S( -4,  92),   S(  4,  80),   S( 16,  60),   S(-14,  42),
            S(-13,  91),   S(-25, 123),   S(-21, 132),   S(-14, 132),   S( -4,  89),   S( -2,  66),   S( 48,  23),   S( 16,  50),
            S(  2, 101),   S( -2, 111),   S(-22, 125),   S(-16, 135),   S( -5,  95),   S( -2,  79),   S( 21,  76),   S(  9,  62),
            S( -7, 132),   S(-18, 148),   S(-17, 146),   S( -5, 128),   S(-12, 108),   S(-14, 100),   S( 11,  90),   S(-15,  75),
            S(-10, 143),   S(-24, 153),   S(  8, 151),   S( 17, 143),   S(  9, 115),   S( 22, 107),   S( 41,  98),   S(  8, 105),
            S(  7, 158),   S( 17, 140),   S( 27, 146),   S( 36, 136),   S( 89,  95),   S(103, 100),   S( 78, 106),   S( 26, 105),
            S( 16, 151),   S(  2, 161),   S( 21, 168),   S( -4, 168),   S( 34, 139),   S( 89, 103),   S( 71, 139),   S(189, 116),
            S(106,  43),   S( 38, 109),   S( 43, 142),   S( 39, 122),   S( 27, 122),   S( 79, 104),   S( 47,  63),   S(128,  42),

            /* rooks: bucket 4 */
            S(-34, -13),   S( 27, -30),   S( 26, -29),   S(-39,   4),   S(-54,  13),   S(-28,  44),   S(-25, -15),   S(-88,  21),
            S(-27, -26),   S(-50,  -8),   S( 19, -31),   S( -5, -18),   S(-33,  33),   S( -9,  19),   S( -4,  10),   S(  3,  11),
            S(-32, -15),   S(-26, -23),   S(-32, -15),   S(-32, -36),   S(-65,  -5),   S(-51,  -6),   S(-44,  -2),   S(-78,  18),
            S(-35, -45),   S( 43,  -2),   S( -2, -19),   S( 14, -34),   S( 54, -12),   S(-11,  21),   S(-15,   0),   S(  2,  25),
            S(-27, -27),   S( 36, -20),   S( 34,  17),   S( 46, -16),   S( 70,   8),   S( 41,  14),   S(  5,  12),   S( 10,  22),
            S(-10, -23),   S(  4,   3),   S( 15, -15),   S( 16,   9),   S( 36,  37),   S( 18,   7),   S( 32,  34),   S( 29,  23),
            S(-16, -47),   S( 41,  18),   S( 44,   6),   S( 63, -35),   S( 12,   7),   S(  9, -15),   S(  3,  -1),   S( 19,   8),
            S( 26, -24),   S( 30,   6),   S( 45, -27),   S( 11,  -7),   S( 23,   7),   S( 19,  -5),   S( 11,  -3),   S( 15,  16),

            /* rooks: bucket 5 */
            S(-54,  33),   S( -6, -13),   S( -9,   7),   S( -4,  25),   S( 12,  -4),   S( -6,  21),   S(-39,  38),   S(-72,  52),
            S(-46,  23),   S(-28, -28),   S( -7, -19),   S( 24, -20),   S(-12,   5),   S(-37,   9),   S(-75,  25),   S(-20,  19),
            S(-73,  28),   S(-21,  -5),   S(-21, -10),   S(-27,   5),   S(-20,  -2),   S( 13, -13),   S(-59,  18),   S(-17,  14),
            S(-62,  28),   S( -4,  -1),   S( 55, -15),   S( 32,  -2),   S( 47, -20),   S(-43,  30),   S(-37,  32),   S( -8,  34),
            S( 57,  -5),   S( 23,  11),   S( 31,  28),   S( 19,  -2),   S(-21,  28),   S( 48,   7),   S( 15,  36),   S( 52,  18),
            S( 37,  15),   S(-19,  12),   S( 11,  24),   S(  6,  17),   S(  2,   9),   S( 29,  30),   S( 87,  12),   S( 20,  45),
            S( 46,  -4),   S( 44,  -5),   S( -4,  11),   S( 53,   4),   S( 46,   1),   S( 53, -26),   S( 60,  -8),   S( 65,  -5),
            S(  8,  26),   S( -1,   4),   S( 78, -11),   S( 16,  34),   S( 57,   3),   S( 19,  21),   S( 22,  25),   S( 30,  39),

            /* rooks: bucket 6 */
            S(-73,  48),   S(-27,  14),   S(-49,  36),   S(-14,  12),   S( -7,   9),   S(-16,   7),   S(-11,   0),   S(-46,  16),
            S(-69,  35),   S( -8,  18),   S(-23,  10),   S( -4,   0),   S(-13,   1),   S(-40,  15),   S(-48,  10),   S(-41,  25),
            S(-107,  51),  S(-50,  27),   S( -2,   1),   S( -1,   6),   S(-23,  11),   S( -5,   7),   S( 14, -29),   S(-20,  -9),
            S(-48,  45),   S( -8,  26),   S(  1,   8),   S( 26,   1),   S( 15,   8),   S(  4,  12),   S(  4, -11),   S(-10,  18),
            S(-11,  48),   S( 31,  17),   S( 77,  12),   S( 54,   6),   S( 28,   7),   S( 22,  12),   S( 40,  -3),   S( 73,   1),
            S(123,  -3),   S( 71,  15),   S( 95,   1),   S( 68, -11),   S(  4, -21),   S( 26,  51),   S( 26, -18),   S( 68,  14),
            S( 18,  16),   S( 85,  -8),   S(108, -21),   S( 95, -34),   S( 59,  -4),   S( 33,  -3),   S( 70, -27),   S( 53, -16),
            S( 44,   1),   S(  9,  41),   S(  9,  25),   S( 92, -19),   S( 66,  -3),   S( 41,  35),   S( 54,  11),   S( 57,  12),

            /* rooks: bucket 7 */
            S(-119,  48),  S(-88,  49),   S(-76,  42),   S(-60,  38),   S(-44,  17),   S(-30,  -3),   S(-49,  23),   S(-70,   3),
            S(-71,  25),   S(-59,  27),   S(-60,  27),   S(-76,  36),   S(-55,   6),   S(-33,  -1),   S( 23, -11),   S(-40, -27),
            S(-67,  29),   S(-66,  24),   S(-64,  28),   S(-70,  47),   S(-66,  20),   S(-42,  28),   S( 52, -21),   S( -6, -33),
            S(-52,  30),   S(  6,  11),   S(-21,  13),   S( 37,  -7),   S(  0,  -4),   S( 37,  -6),   S( 54,  10),   S( 10,  -7),
            S(  5,  27),   S( 48,  10),   S( 69,   3),   S( 74,   4),   S(137, -31),   S(128, -41),   S(117, -15),   S(-29, -10),
            S(  2,  33),   S( 49,   5),   S(130,  -9),   S( 93,   2),   S(111, -10),   S( 76,   7),   S( 23,  29),   S(  0,   1),
            S( -8,   8),   S( 29,  -1),   S( 51,  -1),   S( 66, -15),   S(124, -36),   S(106, -23),   S( 73,  -2),   S( 70, -36),
            S(-32,  10),   S(-15,  30),   S( 45,   5),   S( 13,  23),   S( 32,   2),   S( 58,   6),   S( 62,   9),   S(  9,  -6),

            /* rooks: bucket 8 */
            S(-14, -81),   S( 17, -75),   S( 38, -45),   S( 45, -18),   S(  7, -46),   S(  0, -23),   S( -4, -58),   S(-14, -56),
            S(-12, -91),   S(-11, -44),   S( 27, -16),   S(-19, -64),   S( 18, -44),   S( 14, -33),   S( -3, -30),   S(-31, -40),
            S(  7,   8),   S(  3, -12),   S(  9, -10),   S( -9,   4),   S( -8,   7),   S( 15,  18),   S(  5,  59),   S(-24, -48),
            S( -1, -16),   S( -4, -19),   S( -3,  -5),   S( 25,  12),   S( 20,  29),   S( 29,  40),   S(  1,  30),   S( -7, -21),
            S( -7, -54),   S( 11,  -2),   S( 33,  -9),   S( 15,  28),   S(  2,   5),   S(  9, -20),   S(  9,  26),   S( 13,  28),
            S(-10, -36),   S( 24, -32),   S( 12, -16),   S( -5, -36),   S( -1,  13),   S(-23,  23),   S( -7,  -2),   S(  1,   8),
            S( 20,  14),   S( 35,  -1),   S( 25, -10),   S(  8,  -2),   S(  6,  -6),   S(  8,  28),   S( 19,  41),   S( 14,  34),
            S(-22,  -9),   S( 25,  -7),   S(  1, -16),   S( 23,  24),   S(-12,  -1),   S(  8,  21),   S( 16,  30),   S(  3,  -6),

            /* rooks: bucket 9 */
            S(-32, -91),   S( 15, -103),  S( 27, -103),  S( -3, -84),   S(-12, -86),   S( 43, -64),   S( -5, -56),   S(  8, -66),
            S(-29, -92),   S(  3, -60),   S(  1, -67),   S(-17, -68),   S( -7, -77),   S(-19, -29),   S(-26, -75),   S(-17, -68),
            S(-17, -50),   S( -1, -45),   S( 10,   1),   S(-11, -55),   S( 30, -51),   S(  8, -10),   S(  6,  -1),   S(  0,  14),
            S(  8, -38),   S( 12, -20),   S( -5, -11),   S(  4,   6),   S(-11, -62),   S( 29, -12),   S(-13, -34),   S(  0, -16),
            S( 19, -59),   S(  2, -42),   S(  3, -53),   S( 10,  -7),   S( -6, -56),   S(-21, -26),   S( -7, -42),   S( -3, -42),
            S( -2, -36),   S(-32, -58),   S( -7, -52),   S( 22, -32),   S( 31, -24),   S( -7, -32),   S(-13, -21),   S(  6, -30),
            S(  4, -13),   S( 12, -34),   S( 19, -64),   S(-10,  -9),   S(  1, -27),   S(  0, -24),   S(  7, -13),   S(-14, -49),
            S(  7, -22),   S(  0, -37),   S(  3, -28),   S( 12, -20),   S(  5, -11),   S(  6,  21),   S( -9, -27),   S(  5,  -2),

            /* rooks: bucket 10 */
            S(  7, -84),   S(-49, -62),   S( 30, -87),   S( 17, -104),  S( 18, -100),  S(  1, -132),  S( 26, -116),  S(-17, -69),
            S( -4, -64),   S(-30, -72),   S(-18, -58),   S(-37, -79),   S( -8, -81),   S(-10, -76),   S(-23, -75),   S(-38, -80),
            S(-14, -40),   S(-34, -28),   S(-41, -49),   S(-42, -79),   S( -7, -32),   S(  9, -37),   S( -2, -64),   S(-21, -57),
            S(-14, -35),   S(-33, -60),   S( -3, -71),   S( -4, -26),   S( -2, -30),   S(  6,  -2),   S(-16, -85),   S(  9, -45),
            S( 16, -55),   S(  0, -40),   S(  2, -42),   S( -3, -69),   S( 11, -32),   S(  9,  -6),   S(  8, -64),   S(  1, -83),
            S(-21, -37),   S( -2, -21),   S(-30, -51),   S( -5, -76),   S( 12, -40),   S( 17, -46),   S(-24, -62),   S(  7, -38),
            S(-31, -26),   S( -8, -46),   S(-11, -48),   S( 10, -60),   S( 10, -26),   S(-10, -50),   S(-23, -64),   S( 11, -56),
            S(-19, -28),   S(-11, -14),   S(  1,  -3),   S(-22, -40),   S(-19, -12),   S(-18, -18),   S(  2, -42),   S(-12, -26),

            /* rooks: bucket 11 */
            S( -1, -76),   S(-27, -26),   S(  9, -57),   S( 10, -33),   S(-20, -41),   S( 12, -68),   S( 27, -61),   S(  5, -92),
            S( 11, -41),   S(-20, -36),   S(-38, -27),   S(-25, -31),   S(-32, -32),   S(  5, -20),   S(-23, -60),   S(-25, -81),
            S(-27,  -7),   S(-22,   1),   S( -2,  10),   S(  0, -23),   S( -3, -34),   S( -2, -45),   S( 44,  25),   S(-15, -18),
            S(-13, -52),   S( -8, -46),   S( -4, -24),   S( 16,  -1),   S( 19, -22),   S(-20, -69),   S(  1,   9),   S( -8, -29),
            S(  0, -45),   S(  1, -37),   S(-26, -15),   S(-10, -14),   S( 35,   0),   S( 12, -46),   S( 23,  -1),   S( -4, -48),
            S(-19, -38),   S(-30, -35),   S( 19, -39),   S( 18, -19),   S( -6, -20),   S( 20, -43),   S( 29,   1),   S( -3, -49),
            S(-10, -19),   S(-16, -36),   S( -8,  -6),   S( -7, -16),   S(  8, -36),   S( 43, -30),   S( 27, -49),   S(  9, -34),
            S( -1, -23),   S( 30,  17),   S( -9,  16),   S( 15,  11),   S( -7,  -5),   S( 15, -19),   S( 34, -31),   S( -7, -16),

            /* rooks: bucket 12 */
            S(-10, -87),   S(-10, -16),   S(  9, -34),   S(-10, -40),   S( -4, -35),   S( 18,  -8),   S(-19, -60),   S(-15, -32),
            S(  0, -17),   S(  1,  -4),   S( 13,  14),   S( 14,   8),   S( 15,   2),   S( 25,   5),   S( 11,  11),   S(-13, -38),
            S(  3,  -6),   S( -1,   6),   S( 11,   9),   S( 19,  14),   S(  5, -18),   S( 12, -22),   S( 11,  26),   S(  5,   3),
            S( -8, -12),   S(-17, -37),   S(  5, -13),   S(  5,  -1),   S(  8,  10),   S(  6,   3),   S( 11,  24),   S( -3,  -8),
            S( 12,  15),   S( 16,  18),   S( -6, -41),   S(  0, -38),   S(  7,  14),   S( -1,  -4),   S( -5, -33),   S( -2, -12),
            S(-15, -51),   S( -3, -45),   S(-12, -53),   S(  1,   3),   S(-10, -26),   S(-11, -49),   S(  8,  15),   S( -5, -32),
            S(-16, -34),   S(-10, -32),   S(  1, -17),   S(  9,  23),   S( -3, -20),   S( 11,  -4),   S( 12,  41),   S( -9, -23),
            S(  0,  -2),   S(  1,   0),   S( -2, -37),   S( 11,  17),   S(  5,   8),   S( -8, -30),   S( -7, -28),   S( -2,  10),

            /* rooks: bucket 13 */
            S(-15, -58),   S(-20, -89),   S(-20, -42),   S(-14, -60),   S(-22, -93),   S(  5, -18),   S(-39, -73),   S(-17, -51),
            S(-12, -49),   S( -9, -45),   S(  1,  -3),   S(  2, -11),   S( 25,  17),   S(  4, -21),   S( 10, -41),   S( -7, -53),
            S(  4, -22),   S( -8, -30),   S( -5, -27),   S(  1, -32),   S( 17,  10),   S(  2, -52),   S( 19,  -4),   S( -8, -88),
            S(  1, -18),   S( -7, -37),   S( 13,  14),   S( 14,  -3),   S( 20,   4),   S( -6, -34),   S(  8,  11),   S(  1,   7),
            S(  4,   1),   S(-12, -80),   S( -1, -45),   S( -2, -47),   S(  5, -44),   S( -4, -24),   S(  0,  -4),   S( -3, -12),
            S( -7, -35),   S(  4, -41),   S( -8, -60),   S(-12, -68),   S( -3, -103),  S(  0, -18),   S(-14, -23),   S( -1, -39),
            S( 11,  -6),   S(  0, -44),   S(-10, -60),   S( -4, -27),   S( -9, -52),   S( 10,  -3),   S(  6,   2),   S( -6, -19),
            S(  4,  -8),   S(-19, -49),   S(-11, -50),   S(  9, -14),   S( -4, -32),   S( -5, -31),   S(  1,  -9),   S(  7,   9),

            /* rooks: bucket 14 */
            S(  1, -51),   S(-24, -18),   S( -5, -30),   S(-20, -85),   S(-10, -65),   S(  7, -45),   S(-30, -94),   S( -9, -42),
            S( 13,  -8),   S(  8, -21),   S( 10, -34),   S(-12, -71),   S( -6, -26),   S( -1,  -7),   S( -2, -49),   S(  1, -60),
            S(  2, -13),   S(  0, -16),   S( -7, -35),   S(  0, -27),   S( 13,  -5),   S( -6, -22),   S( 17, -17),   S(-16, -69),
            S( -6, -22),   S( 11,  11),   S( -9, -25),   S( 14,  -2),   S(  3, -13),   S(  7,  -1),   S( -4, -38),   S( -3, -38),
            S(  8,  21),   S( 10,  40),   S(  3, -11),   S( -3, -68),   S( -7, -63),   S( 12, -20),   S( -5, -59),   S( -2, -45),
            S(  5, -11),   S(  3,   0),   S(  7,  -9),   S(  5, -81),   S( -1, -72),   S( -8, -82),   S( -7, -79),   S(-16, -48),
            S(-10, -23),   S( 12,  15),   S( -7, -59),   S(-23, -85),   S( -4, -23),   S( 13,  -7),   S(-13, -76),   S( -8, -42),
            S(-20, -59),   S( -3, -21),   S( -7, -25),   S(  2, -61),   S(-13, -70),   S(-12, -92),   S( -8, -81),   S( -2,   6),

            /* rooks: bucket 15 */
            S(-11, -69),   S( -9, -50),   S(-28, -52),   S(-22, -50),   S(-11, -55),   S(-16, -62),   S(  6,  -2),   S(-16, -67),
            S( 20,  42),   S( -6, -49),   S( -3, -18),   S(-11, -34),   S(-12, -61),   S(  7,   0),   S( 17,  17),   S( -4, -20),
            S( -4, -26),   S( -7, -51),   S(  5,   9),   S( 12,  -9),   S( -1, -35),   S( -1, -24),   S( 19,  38),   S( -3, -13),
            S(  2,  12),   S(  5,  19),   S( 23,  66),   S(  3,  -3),   S(  6,   4),   S( -7, -28),   S( 13,  11),   S( 14, -10),
            S(  5,  -4),   S(  3,  -8),   S(  3,  -6),   S(  2, -13),   S( 13,  24),   S( 10, -21),   S( 14, -22),   S( -9, -40),
            S(  6,  28),   S( -3, -22),   S(  7,  12),   S( -8, -47),   S( -4, -28),   S( 11, -33),   S(  4, -29),   S(  3, -14),
            S(  1, -22),   S(-10, -38),   S( 13,  15),   S(  6,  -1),   S(  1,  -2),   S(  7, -21),   S( -7, -42),   S(-11, -65),
            S( -3, -33),   S( -4, -20),   S(  2,  10),   S(  3,  -9),   S(  2,  -4),   S( -6, -57),   S( 13, -33),   S(-23, -73),

            /* queens: bucket 0 */
            S(-15, -43),   S(-25, -75),   S(-31, -53),   S(  1, -93),   S(-22, -34),   S(  8, -64),   S(-96, -19),   S(-14, -34),
            S(-13, -50),   S(  2, -71),   S( 12, -79),   S(-21, -32),   S(  6, -57),   S(-21, -47),   S(-35, -55),   S(-24, -29),
            S(-26,  42),   S(  1, -44),   S( 26, -57),   S( -3, -25),   S(-20,  13),   S( -6, -10),   S(-21, -25),   S(-75, -61),
            S(-36,  18),   S(  4, -16),   S(-14,  39),   S(-23,  39),   S( -2,  26),   S(-35,  46),   S(-18, -50),   S( -6, -52),
            S(-41,   4),   S(-18,  86),   S(-12,  41),   S(-19,  79),   S(-23,  80),   S(-44,  81),   S(-57,  22),   S(-54,   3),
            S(-31,  62),   S( 30,  71),   S( 39,  73),   S(-30,  87),   S(-72, 104),   S(-46,  53),   S(-103,  39),  S(-65, -11),
            S(  0,   0),   S(  0,   0),   S( 12,  39),   S(-32,  73),   S(-59,  33),   S(-83,  73),   S(-86,  45),   S(-109,  -3),
            S(  0,   0),   S(  0,   0),   S( 15,  20),   S(  2,  17),   S(-55,  24),   S(-54,  23),   S(-66,  -1),   S(-34,   8),

            /* queens: bucket 1 */
            S( 25, -54),   S( 18, -56),   S( 15, -90),   S( 28, -118),  S( 32, -108),  S( 12, -91),   S(  7, -41),   S(-25,   3),
            S(-33,  23),   S( 25, -26),   S( 35, -51),   S( 21, -27),   S( 22,  -6),   S( -3, -33),   S(-50,  16),   S(-19, -20),
            S( 32,  -3),   S( 16, -11),   S(  6,   3),   S( 13,  27),   S(-18,  58),   S( 20,  28),   S(-21,  24),   S( 18, -21),
            S( 32, -29),   S( -9,  44),   S( -1,  59),   S(  8,  67),   S( -9,  85),   S(  1,  36),   S(  0,  52),   S(-20,  76),
            S(-20,  64),   S( 10,  47),   S( 38,  55),   S(  6,  67),   S( 18, 107),   S( 41,  31),   S(-14,  60),   S(  7,  50),
            S( 43,  -2),   S( 88,  75),   S( 70,  55),   S(112, 113),   S( 84, 102),   S(  5, 106),   S( 38,  64),   S(  2,   6),
            S( 78,  21),   S( 48,  25),   S(  0,   0),   S(  0,   0),   S( 23,  96),   S(-37,  92),   S(-24,  93),   S(-66,  61),
            S( 83,  29),   S( 54,  16),   S(  0,   0),   S(  0,   0),   S( 45,  41),   S( 49,  66),   S( 95,  24),   S(-14,  39),

            /* queens: bucket 2 */
            S( 19, -36),   S( 36, -55),   S( 20,  -6),   S( 40, -26),   S( 42, -69),   S( 26, -71),   S(-11, -64),   S( 18,  -1),
            S( 21,  -1),   S(  7,   7),   S( 40,   0),   S( 33,  -3),   S( 45, -13),   S( 15, -10),   S( 13,  10),   S( 31,  45),
            S( 47,  -1),   S( 29,  42),   S( 11,  84),   S(  8,  58),   S( 23,  47),   S( 22,  54),   S( 23,  19),   S( 37,  44),
            S( 32,  16),   S(  8, 100),   S( 10, 106),   S(  4, 120),   S( 30,  86),   S(  3, 100),   S( 14,  93),   S( 33,  94),
            S( -8,  96),   S(  7,  38),   S(-18, 132),   S( 17, 124),   S( 25, 132),   S( 89,  61),   S( 48, 104),   S( 60,  43),
            S(-36, 109),   S(-34, 107),   S(  6, 135),   S(106,  83),   S( 47, 115),   S( 94, 125),   S(128,  87),   S( 21, 149),
            S(-21,  76),   S(-27, 117),   S(-12, 104),   S( 88,  82),   S(  0,   0),   S(  0,   0),   S( 13, 129),   S( 40,  98),
            S( -2,  54),   S( 56,  23),   S( 91,  15),   S( 45,  78),   S(  0,   0),   S(  0,   0),   S( 71,  98),   S( 14,  89),

            /* queens: bucket 3 */
            S(-40,  32),   S(-16,  30),   S(-13,  21),   S(  0,  48),   S(-13,  15),   S( -5, -14),   S(  3, -84),   S(-23,  19),
            S(-36,  39),   S(-26,  39),   S( -7,  58),   S(-11,  76),   S( -6,  64),   S( -4,  25),   S( 25,  -8),   S( 45, -65),
            S(-37,  62),   S(-23,  90),   S(-23, 113),   S(-34, 139),   S(-23, 101),   S(-20, 107),   S( -7,  60),   S( -6,  53),
            S(-29,  55),   S(-49, 128),   S(-36, 161),   S(-21, 171),   S(-31, 159),   S(-21, 117),   S(-16, 126),   S(-12,  91),
            S(-34, 106),   S(-46, 146),   S(-52, 159),   S(-33, 188),   S(-24, 165),   S(-32, 204),   S(-34, 160),   S(-30, 125),
            S(-52, 111),   S(-52, 143),   S(-53, 184),   S(-65, 204),   S(-50, 216),   S( 20, 173),   S(-27, 214),   S(-27, 181),
            S(-91, 129),   S(-87, 160),   S(-81, 203),   S(-73, 191),   S(-73, 206),   S(  8, 128),   S(  0,   0),   S(  0,   0),
            S(-159, 193),  S(-104, 152),  S(-69, 133),   S(-87, 176),   S(-41, 166),   S( 15, 109),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-56, -14),   S(-73, -70),   S(  0, -30),   S(-21, -19),   S( 32,   5),   S( -8, -13),   S(-53, -44),   S( -7,  -4),
            S(-13, -10),   S( -7,  45),   S(-10,  13),   S(-34, -18),   S(-43,  31),   S(  2,  23),   S(-56, -23),   S(-19, -22),
            S(  7,  27),   S( 10, -28),   S( 12,  -6),   S( 12,   2),   S( 10,  15),   S( -3,  25),   S(-18, -11),   S( 17,   7),
            S(  5, -17),   S( 28,   9),   S( 13,   9),   S( 21,  53),   S( 47,  29),   S( -2,  32),   S(-12, -11),   S(-48, -30),
            S(  0,   0),   S(  0,   0),   S( 34,  29),   S( 63,  70),   S( 10,  69),   S( 21,  37),   S( -6,  -1),   S(-11,  14),
            S(  0,   0),   S(  0,   0),   S( 31,  40),   S( 53,  43),   S( 34,  26),   S( 59,  35),   S(-21,   9),   S(-22,   7),
            S( 27,  22),   S( 26,  20),   S( 76,  51),   S( 57,  44),   S( 41,  18),   S( 20,  35),   S(-26,   2),   S( -9,  12),
            S( 45,  20),   S( -7,   8),   S( 26,   4),   S( 84,  71),   S(  8,  25),   S( -8,  24),   S(-50, -24),   S(  2, -16),

            /* queens: bucket 5 */
            S( 15, -26),   S( 25,  10),   S(-27, -11),   S(-58, -16),   S(-28, -41),   S( 25,  31),   S( 23,   2),   S( -6, -24),
            S( 15,  15),   S( -6,  18),   S(-20, -21),   S( -4,  17),   S( -4,  25),   S(-39,  -6),   S( 11,  -5),   S( -1, -14),
            S( 29,  23),   S( 32, -20),   S( -2,  -5),   S(-18,  26),   S(  4,  14),   S( 32,  21),   S( 11,  27),   S(-11,   3),
            S(-33, -33),   S( 14,  -3),   S( 37,  31),   S( 36,  27),   S( 82,  43),   S( 53,  45),   S( 28,  49),   S(-13,  45),
            S( 54,  18),   S( 46,   1),   S(  0,   0),   S(  0,   0),   S( 42,  35),   S( 32,  25),   S( 48,  53),   S(-35,  -7),
            S( 50,  18),   S( 63,  67),   S(  0,   0),   S(  0,   0),   S( 44,  42),   S( 63,  55),   S( 38,  18),   S( 54,  34),
            S( 78,  26),   S( 73,  32),   S( 61,  60),   S( 30,  50),   S( 61,  51),   S(114,  71),   S( 68,  60),   S( 29,  28),
            S( 28,  28),   S( 51,  34),   S( 91,  59),   S( 45,  16),   S( 74,  67),   S( 58,  46),   S( 20,   4),   S( 32,  19),

            /* queens: bucket 6 */
            S( 68,  36),   S( -6, -10),   S( 28,  14),   S(-13, -16),   S( 35,  -3),   S(-67, -11),   S( -4, -11),   S( 12,  12),
            S(-21,  -1),   S( 31,  23),   S(  3,  53),   S( 35,  25),   S( 25,   5),   S(-10,   6),   S(-18,  -1),   S(-15,  16),
            S( -3,  43),   S( 12,  52),   S(  6,  18),   S( 25,  12),   S(  3,   3),   S( 27,   5),   S( 65,  36),   S( 55,  44),
            S( -2,  48),   S(-16,  24),   S( 29,  37),   S(107,  62),   S( 61,   3),   S( 25,   9),   S( 74,  40),   S(106,  22),
            S( 15,  28),   S( -6,  33),   S( 45,  68),   S( 60,  68),   S(  0,   0),   S(  0,   0),   S( 56,  29),   S(116,  67),
            S( -8,  38),   S( 52,  54),   S( 60,  65),   S( 32,  32),   S(  0,   0),   S(  0,   0),   S(104,  84),   S(140,  55),
            S( 29,  28),   S( 10,  29),   S( 91,  39),   S( 60,  55),   S( 57,  70),   S(100,  62),   S( 99,  36),   S(141,   6),
            S( 39,  13),   S( 31,   0),   S( 52,  32),   S( 83,  42),   S(109,  37),   S( 94,  42),   S(135,  41),   S( 78,  12),

            /* queens: bucket 7 */
            S(-30, -40),   S(-46, -16),   S(-16, -23),   S(-24,   3),   S(  6,  -9),   S(-16,  -2),   S(-10,  16),   S(-24, -42),
            S(-18,  -9),   S(-51, -29),   S(-25,  40),   S(-19,  62),   S(-37,  45),   S(  9,  10),   S( -7, -23),   S(-21,  -7),
            S(-55,  -3),   S(-31,  26),   S(-20,  59),   S( -4,  66),   S( 30,  14),   S( 28,   7),   S( 48, -26),   S( 51,  14),
            S(-46,  19),   S(-29,   9),   S(-16,  60),   S( 42,  57),   S( 62,  34),   S( 64,  22),   S( 78,  -9),   S( -1,  28),
            S(-14,  18),   S(-28,  26),   S(  7,  79),   S( 32,  71),   S( 72,  30),   S(100,  23),   S(  0,   0),   S(  0,   0),
            S( -8,  -6),   S(  2,  29),   S( -6,  50),   S(  1,  64),   S( 70,  19),   S(100,  60),   S(  0,   0),   S(  0,   0),
            S(-82,  53),   S(-18,   6),   S(  6,  42),   S( 56,  29),   S( 81,  39),   S( 91,  25),   S( 71,  36),   S( 91,  51),
            S( 14, -31),   S( 15,   6),   S( 29,   6),   S( 46,  -3),   S( 64,  36),   S( 42,  11),   S( 13,  16),   S( 84,  29),

            /* queens: bucket 8 */
            S(-13, -33),   S(-15, -43),   S( -6, -19),   S(  2,   5),   S(  3, -16),   S(  7,   5),   S( -9, -18),   S(  2,  -1),
            S(-25, -47),   S(  8,  24),   S(  4, -17),   S( -7,  12),   S( 18,  12),   S(  9,  14),   S( -3, -11),   S( -6, -11),
            S(  0,   0),   S(  0,   0),   S( 16,   6),   S( -6, -27),   S( 38,  22),   S(-13,   1),   S( -7, -11),   S( -2,   4),
            S(  0,   0),   S(  0,   0),   S( 14,  28),   S( 10,   7),   S(  8,   8),   S( -7, -17),   S( -5,   5),   S(  3,  -4),
            S(  5,   3),   S( 11,  15),   S(  9,  15),   S( 27,  14),   S(-20, -33),   S( 17,  12),   S( 13,   2),   S( 10,  10),
            S( -1, -18),   S(  5, -25),   S(  5,  15),   S( -9, -25),   S(  5,   3),   S( 25,  23),   S( -6, -16),   S(  1, -11),
            S(  2, -13),   S( -6, -22),   S( 28,  26),   S( 22,  21),   S( 14,   4),   S(  5,  13),   S(  6,  -9),   S(  5,   5),
            S(  0,  -2),   S( 14,  26),   S( 14,  13),   S(  4,   1),   S( 16,  16),   S( -6,  -4),   S(-17, -32),   S(-26, -52),

            /* queens: bucket 9 */
            S( 41,  26),   S( -9, -50),   S( -1,   0),   S( 22,   7),   S(  3, -14),   S( -9, -32),   S(-10, -19),   S( -4, -20),
            S( 33,  24),   S( 12,  -4),   S(-11, -18),   S( 21,  28),   S(-13, -33),   S(  1, -13),   S( 16,  15),   S(-20, -42),
            S(-13, -31),   S(-20, -42),   S(  0,   0),   S(  0,   0),   S( 16,  19),   S( 10,   1),   S(-31, -41),   S(  1,  -7),
            S( 18,  16),   S(-13, -23),   S(  0,   0),   S(  0,   0),   S( 12,  10),   S( 34,  26),   S( 13,  14),   S(-10, -13),
            S( 13,   0),   S( 18,   8),   S(  7,  20),   S(-22, -30),   S( -1, -17),   S( 13,  14),   S(  8, -13),   S(  6,  -5),
            S( 22,  20),   S(  7,  13),   S( 30,  34),   S(  9,  -1),   S(  0, -28),   S(-10, -19),   S( -7, -17),   S(  1, -11),
            S(-20, -21),   S( 10,   4),   S(-12, -30),   S( -1,   2),   S( 16,  20),   S( 37,  39),   S( 10,  22),   S( 13, -10),
            S( -5, -41),   S( 20,  -4),   S( -5, -18),   S( 13,   2),   S( 15,  17),   S(-27, -39),   S(  7, -11),   S( 11,  -7),

            /* queens: bucket 10 */
            S(  3,  -1),   S( 10,  -9),   S(  3,   0),   S(-10, -36),   S(  7,  -8),   S( 20,  24),   S(  1, -14),   S( -7, -21),
            S(  5,  -5),   S(-10, -39),   S( 11,  -2),   S(-18, -40),   S(  3,  -3),   S( -3, -17),   S(  5,  -8),   S(  1,  -1),
            S( 12,  18),   S(  5,  10),   S( 11,  -2),   S( 18,  13),   S(  0,   0),   S(  0,   0),   S(  8,  12),   S(-12, -24),
            S(  1,  -7),   S( 12,   5),   S(  1, -23),   S(  6,  12),   S(  0,   0),   S(  0,   0),   S(-13, -30),   S(  4, -13),
            S(  2,   9),   S( 14,  29),   S(  5, -14),   S( 44,  47),   S(  8,  11),   S(  8,  17),   S(  3,  -9),   S( 40,  19),
            S( -2, -11),   S(  7,   1),   S( 20,  21),   S( -2,  -2),   S( 12,  11),   S(  4,  16),   S( 22,  -5),   S(-21, -62),
            S( 16,  11),   S(  8,  18),   S( 17,  41),   S( 22,  12),   S( 23,  30),   S( 32,  27),   S( 23,  10),   S(-12, -33),
            S(-13, -21),   S( -9, -29),   S( 25,   9),   S(-20, -58),   S( 27,  27),   S( -8, -13),   S(  1,  -8),   S(  6,  -4),

            /* queens: bucket 11 */
            S(-13, -15),   S(  5,   1),   S(  1,  -5),   S(  4,  -6),   S( -5, -14),   S(-37, -71),   S(-12, -42),   S( -2, -24),
            S( 18,  17),   S( -1,  -1),   S( -5,  -5),   S( 23,  18),   S( 21,  -6),   S( -1,   0),   S( 11,  -5),   S(  0,  -5),
            S( -6,   5),   S( -2, -15),   S(-27, -20),   S( -1, -23),   S( 18,  -4),   S(-17, -44),   S(  0,   0),   S(  0,   0),
            S( -7, -13),   S(-11,   0),   S(-19, -12),   S( 20,   9),   S( 12,  19),   S( 21,  42),   S(  0,   0),   S(  0,   0),
            S( -8,  -9),   S( -6,  -6),   S( 18,  35),   S(  3,  12),   S( 41,  66),   S( 28,  20),   S(  2,  10),   S(-14, -32),
            S(-18, -23),   S( -6, -19),   S(-14, -22),   S(  5,  -8),   S( 29,  36),   S(  7,   9),   S( -5,  -4),   S( 26,   5),
            S( -7,  -9),   S(  1,   8),   S(  6,   2),   S(-13, -18),   S( 27,  47),   S(  3,   9),   S( -5,   2),   S( 24,  18),
            S(-31, -82),   S( -1, -10),   S(-10, -20),   S(-14, -20),   S( 12,   6),   S( -2, -24),   S( 10,   2),   S( 24,  22),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  8,   9),   S( -7, -22),   S( 17,  36),   S( -8, -18),   S( 10,   6),   S( -2,  -1),
            S(  0,   0),   S(  0,   0),   S( 11,  20),   S(-10, -33),   S( 11,   9),   S(-26, -57),   S( -9, -20),   S(  1,  -6),
            S( -5, -10),   S(  0,  -2),   S(  4,  -3),   S(  9, -13),   S( 17,  25),   S( -7,  -5),   S(  4,   3),   S(  4,   3),
            S( 10,   5),   S( 10,   0),   S(  8,  19),   S( 10,  20),   S( -2, -10),   S(  5,   0),   S( -4, -10),   S(-10, -22),
            S(-12, -25),   S(  2,  -4),   S( 11,  12),   S( -9, -33),   S(  4,   6),   S(-13, -45),   S(-11, -21),   S( -1,  -6),
            S(-12, -15),   S( -2,  -7),   S( 11,   3),   S(-12, -20),   S(-13, -36),   S(-10,   2),   S( -8, -10),   S(  2,   1),
            S(-14, -31),   S(  2,   9),   S( -8, -19),   S(  7,  17),   S( -2,  -7),   S(-19, -42),   S(  0,   2),   S(-16, -34),
            S( 14,  22),   S( -4, -11),   S(  4,   1),   S(  7,   8),   S( -5, -13),   S(-15, -44),   S( -4,  -6),   S(-14, -35),

            /* queens: bucket 13 */
            S(-10, -37),   S(-13, -26),   S(  0,   0),   S(  0,   0),   S( -5,  -9),   S( -1,  -2),   S( 10,  13),   S(-13, -28),
            S(-11, -41),   S(  3,   5),   S(  0,   0),   S(  0,   0),   S(-12, -20),   S(-14, -27),   S( -3,  -7),   S(  0,  -7),
            S(-11, -31),   S(-16, -38),   S( -3,  -7),   S(  9,  15),   S(-20, -36),   S( -3,  -6),   S(-10, -20),   S( -1,   7),
            S(-16, -30),   S(  1, -13),   S( -2, -19),   S(  7,  -1),   S( 31,  55),   S( 27,  44),   S(-14, -37),   S( -9, -19),
            S(  4,  -5),   S(  2, -12),   S( -8, -13),   S( 25,  37),   S(  9,   9),   S( -4, -13),   S(-13, -29),   S( -7, -16),
            S(  8,   7),   S(-27, -62),   S(-12, -17),   S( 10,   9),   S( 16,  18),   S( -7,  -2),   S(-21, -45),   S(-18, -48),
            S( -9, -18),   S( -2, -11),   S( 12,  23),   S(  4,   5),   S( -5, -14),   S( -3, -15),   S(-18, -38),   S(-13, -28),
            S(-10, -26),   S( -4,  -4),   S( -6, -15),   S(  0,   2),   S(  1,   5),   S( -6, -21),   S(  2,   4),   S(-21, -35),

            /* queens: bucket 14 */
            S( -1, -10),   S(  9,   3),   S(  0, -16),   S( -3, -24),   S(  0,   0),   S(  0,   0),   S( -2, -12),   S( -6, -29),
            S(-10, -23),   S(-15, -41),   S( -8, -30),   S(  0, -10),   S(  0,   0),   S(  0,   0),   S(  4,   3),   S(-13, -30),
            S( -7, -15),   S( -3, -34),   S( -5, -19),   S( -5, -14),   S(  9,  19),   S(  7,  16),   S( -6, -20),   S(-24, -56),
            S( -7, -18),   S( -5, -12),   S( 12,  11),   S(-14, -39),   S(  5,   1),   S( -9, -22),   S(  6,  -1),   S(  4,  -4),
            S(-12, -17),   S(  3,  -5),   S(-18, -45),   S( 10,   9),   S( 22,  35),   S(  4,  -7),   S(  0, -12),   S( -9, -29),
            S(-12, -25),   S( -6, -25),   S(-21, -45),   S(-20, -44),   S(  2,   7),   S( -2,   0),   S( -4, -13),   S(-22, -46),
            S( -9, -16),   S(  0,   1),   S( -3, -10),   S(  0,   3),   S( -1,   6),   S( -6, -18),   S(-15, -33),   S(-12, -23),
            S(-22, -54),   S(  2,  -7),   S(-12, -22),   S( -7, -18),   S(  2,   3),   S( -3, -10),   S( -4, -10),   S(  3,  -5),

            /* queens: bucket 15 */
            S( -7, -23),   S( -2,  -8),   S( 10,   6),   S( -4, -18),   S(  2,  -8),   S( -9, -22),   S(  0,   0),   S(  0,   0),
            S( -5, -10),   S(  5,   6),   S(-10, -17),   S( -7, -24),   S( -2,  -8),   S(  9,  20),   S(  0,   0),   S(  0,   0),
            S( -9, -22),   S(  3,   0),   S(-20, -36),   S(-15, -27),   S( -6, -21),   S(  4,   0),   S(  2,   4),   S( -1,  -2),
            S( -3,  -5),   S(-14, -28),   S( -9, -21),   S( -5, -16),   S(  7,  12),   S( 11,  25),   S( -1,  -5),   S( -5, -24),
            S( -4, -14),   S( -1,  -3),   S(-16, -39),   S( 10,  16),   S(  2,  28),   S(  4,   9),   S(  2,   2),   S(-26, -58),
            S(  7,  12),   S(-10, -16),   S(-14, -34),   S(-11, -18),   S( -6,  -3),   S(-10, -30),   S(  1,   0),   S( -4,  -8),
            S(-12, -22),   S( -4,  -9),   S( -2,   0),   S(  4,  -5),   S( -7, -13),   S( -5,  -6),   S( -5, -12),   S( -6, -14),
            S(-11, -26),   S(-22, -53),   S(-12, -38),   S( -7, -12),   S(-23, -31),   S( -8, -19),   S(  3,   5),   S( -6, -11),

            /* kings: bucket 0 */
            S( -2, -25),   S( 19,   2),   S( 16,   4),   S(-16,   1),   S(-20,  -2),   S( 29, -27),   S(  9,   9),   S( 15, -45),
            S(-18,  21),   S(  1,   3),   S(-19,  18),   S(-46,  12),   S(-30,  43),   S(-10,  11),   S( -9,  45),   S(  7,  19),
            S( -2,   2),   S( 58, -16),   S(-40,  21),   S( 11, -17),   S(-24,  -2),   S(-16,  -3),   S(-29,  13),   S(  8,  -5),
            S( -5, -27),   S( -4, -13),   S(-25,  -5),   S(-32,  28),   S(-68,  34),   S(-14,   4),   S(-43,  20),   S(-22,  24),
            S(-52, -97),   S( 19, -20),   S( 20, -37),   S(  9, -12),   S(-19,  -8),   S(-47,   2),   S(-10,  18),   S( -5,  -3),
            S(  7, -77),   S( 23, -45),   S( 27, -59),   S(-11, -30),   S(  8, -18),   S( 22, -27),   S( 23,   4),   S(-14, -26),
            S(  0,   0),   S(  0,   0),   S(  3, -39),   S( 23, -29),   S(  9, -20),   S(  5, -15),   S( 17,  15),   S(-31, -39),
            S(  0,   0),   S(  0,   0),   S( -5, -46),   S(  6, -54),   S( 19,  11),   S( -3, -28),   S(  7,  -6),   S(  1,  14),

            /* kings: bucket 1 */
            S( 16, -31),   S( 37, -27),   S( 18, -11),   S( 16,   0),   S( -9,   3),   S( 32, -12),   S(  8,  21),   S( 31, -24),
            S(  4,   1),   S(  5,  22),   S( 15, -14),   S(-52,  25),   S(-31,  16),   S( -1,  10),   S(  3,  27),   S(  1,  19),
            S(-14, -16),   S( 20, -21),   S( 17, -14),   S( 17, -17),   S(-22,   2),   S(  3, -21),   S( 14,  -2),   S( 59, -21),
            S( -9,   1),   S( 33, -17),   S( 11,   2),   S(-20,  15),   S(  2,  22),   S(-50,  19),   S( 29,   2),   S(-51,  40),
            S(-35, -17),   S( 33, -21),   S( 23, -27),   S( 18, -17),   S(  4, -10),   S(-23,  -6),   S( -3,  -5),   S(-16,  11),
            S( -1,   2),   S( 36, -47),   S( 23, -16),   S( 37, -36),   S( -5, -10),   S( 26, -12),   S( 17,   6),   S(-19,  -6),
            S( -7, -51),   S( 11,  33),   S(  0,   0),   S(  0,   0),   S( -4,  13),   S(  8,  -1),   S( -4,  42),   S(-12, -34),
            S(-18, -120),  S( -9,  -9),   S(  0,   0),   S(  0,   0),   S( -3, -36),   S( 16,  26),   S(  3,  23),   S(  4, -36),

            /* kings: bucket 2 */
            S( 27, -71),   S(  1,  13),   S( 14, -21),   S( 11,  -3),   S(-18,  14),   S( 38, -25),   S(  5,  28),   S( 29, -20),
            S( 26, -19),   S( -8,  35),   S(-11,   6),   S( -9,   4),   S(-21,  13),   S( -7,   3),   S( 14,  12),   S( -5,   8),
            S(-29, -18),   S(-21,   5),   S( 21, -26),   S(-30,  -7),   S( -4,  -6),   S( -3, -22),   S( 34, -15),   S( 26, -18),
            S(  1,  24),   S(-15,   8),   S(  4,  10),   S( -7,  12),   S( 31,   0),   S(-29,   3),   S(  4,  -9),   S( 38, -19),
            S(-16,  -1),   S( 25,   6),   S( -4, -19),   S( -4, -14),   S( 22, -32),   S(  1, -32),   S( 77, -45),   S(  3, -23),
            S( -5, -17),   S( 27,  -1),   S( 20, -20),   S( 55, -22),   S( 64, -20),   S( 13,  11),   S( 67, -40),   S( 30, -31),
            S(-12, -28),   S( 11,  30),   S(-18,  -9),   S( 22,   0),   S(  0,   0),   S(  0,   0),   S( 26,  25),   S( -3, -21),
            S(-11, -25),   S( -9, -22),   S(  0, -29),   S(  8,   6),   S(  0,   0),   S(  0,   0),   S(  3,   5),   S(-24, -158),

            /* kings: bucket 3 */
            S(  5, -68),   S(  7,  -6),   S( 20, -37),   S( -8, -13),   S(-14, -17),   S( 37, -35),   S(  3,  17),   S(  7, -31),
            S(-19,  22),   S(-21,  33),   S(-17,   1),   S(-42,   7),   S(-54,  20),   S(  2,  -8),   S( -6,  19),   S(-15,  12),
            S( 26, -35),   S( 13,  -5),   S(-10,  -4),   S(-13, -22),   S( -7,  -3),   S( 33, -34),   S( 45, -26),   S( 59, -29),
            S(-42,  26),   S(-101,  27),  S(-84,  17),   S(-52,  11),   S(-79,  20),   S(-59,  -8),   S(-71,   4),   S(-54,  -6),
            S(-54,  27),   S(-19, -10),   S(-46, -18),   S(-72,  10),   S( -1, -35),   S(  6, -43),   S(  1, -49),   S(-24, -61),
            S(-28,  -5),   S( 21, -20),   S( 20, -33),   S(-37, -15),   S( 13, -26),   S( 84, -60),   S(125, -49),   S( 54, -102),
            S(-38,  -5),   S( 19,  12),   S( 10, -52),   S( 25, -34),   S( 21, -29),   S( 41, -48),   S(  0,   0),   S(  0,   0),
            S(-11, -11),   S(  6, -18),   S(  7,   9),   S(  0,  -4),   S( -5, -83),   S(  7,  -5),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-23, -21),   S(  2,  17),   S( -9,  41),   S( -7,   9),   S( -1,  -8),   S(-11,  16),   S(  0,  26),   S( 44, -37),
            S(-53,  36),   S( 24,  21),   S(-12,   9),   S( -3,  -5),   S( 58, -24),   S( 18,  -3),   S( 54, -10),   S(  5,   7),
            S( -8,  13),   S( -4, -12),   S(  2,   4),   S( -7, -11),   S(-29,  12),   S( 27, -29),   S(-76,  24),   S(-11,  13),
            S( -2,   0),   S( 14,   0),   S( 25,  16),   S( 17,   4),   S( 20,  -7),   S(-26,  11),   S( 24, -11),   S(-11,  15),
            S(  0,   0),   S(  0,   0),   S(  8, -20),   S(  5,  17),   S( -9,   9),   S(-29, -12),   S(-24,   4),   S(-10,   6),
            S(  0,   0),   S(  0,   0),   S(  3,  -3),   S( 14,  -2),   S(  6,  24),   S( -4, -18),   S(  7, -23),   S( 10,   5),
            S( -1,  -7),   S(  6,  11),   S( -1, -26),   S(  5,  -3),   S( 24,  37),   S(-10,  -8),   S( -2,   2),   S(  9,   2),
            S( -2,  26),   S( -5, -11),   S( -3,   6),   S( -6,  -8),   S(  0, -14),   S(  4,  -1),   S( -2, -27),   S(  3,   4),

            /* kings: bucket 5 */
            S( 16, -30),   S(  5,   9),   S(-69,  24),   S(-17,   5),   S(-21,  14),   S(-16,  14),   S( 53,   0),   S( 21,   7),
            S( 16,  -2),   S( 49,  -5),   S( 34,  -8),   S( 21,  -3),   S( 14,  -6),   S( 37, -10),   S( 40,   4),   S( 42, -10),
            S(  9,  -5),   S(-62,  12),   S( -2, -13),   S(-25,  -2),   S(-17,   9),   S(-58,  -5),   S(  3,  -2),   S( -1,   6),
            S(-31,   0),   S( 50, -21),   S( 41, -13),   S(-12,  25),   S( 24,  15),   S( 12,   2),   S(-15,  16),   S( 15,  -7),
            S(-13,  -1),   S( -5, -23),   S(  0,   0),   S(  0,   0),   S( -6,  -1),   S( -8, -10),   S(-25,  -4),   S(  5,   1),
            S(-29,  -4),   S(-21,  -3),   S(  0,   0),   S(  0,   0),   S( 15,  21),   S(-19,  -5),   S(-25,  16),   S(-19,  -7),
            S( -9,  11),   S(  4,   3),   S(  4,  38),   S( -3,   7),   S(-23, -16),   S( -6,  22),   S(  6,  26),   S(  7,   9),
            S(-14, -25),   S(  6,  35),   S( -3,   7),   S(  9,  35),   S( -2,  25),   S( -2,  34),   S(  5,   5),   S(  4,  31),

            /* kings: bucket 6 */
            S( 31, -21),   S( 24,  -9),   S( 11, -11),   S(-24,  13),   S( -9,  15),   S(-23,  18),   S( 24,  19),   S( 25,  10),
            S( 54, -21),   S( 14,  16),   S( 35, -10),   S( 41, -13),   S( 29,  -6),   S(  3,   6),   S( 30,   5),   S( 18,   3),
            S( -2,  -7),   S(-20,  12),   S( 15, -19),   S(-21,  -4),   S(-22,  -1),   S(-58,  -3),   S( -4,   1),   S(-47,  19),
            S( 12,   2),   S( 19,   6),   S( 22,  -7),   S( 30,   7),   S( 88,   0),   S( -8,   3),   S( 87, -18),   S( -1,  -3),
            S(  6, -12),   S(-16,  -1),   S(  1, -25),   S(-10,  -3),   S(  0,   0),   S(  0,   0),   S(-10, -17),   S(-65, -10),
            S(-22,  -2),   S( 14,  23),   S(-18,   8),   S(-23,   7),   S(  0,   0),   S(  0,   0),   S(-47,  27),   S(-51, -16),
            S( -8, -25),   S(  3,  11),   S( -3,   6),   S(-12,   2),   S(  0,  -6),   S(  1,   3),   S(-14, -26),   S(-31, -18),
            S(  0,  27),   S( -4,   1),   S( 16,  27),   S( -6,   9),   S(  3,   2),   S( -5,  22),   S(  8,  63),   S(-13,   3),

            /* kings: bucket 7 */
            S( 33, -21),   S( -5,  -1),   S( -4, -19),   S(-32,  10),   S(-13,   4),   S(-45,  31),   S(-12,  36),   S(-13,  18),
            S( 22,  -5),   S( 39, -26),   S( -8,   0),   S( -9,  -7),   S( -2,   1),   S(-41,  24),   S(  6,   7),   S(  4,  13),
            S( 46, -29),   S(  8,  -8),   S(-26,  -1),   S( -8,  -3),   S(-16,  -3),   S(-60,  12),   S( -2,   7),   S(-47,  11),
            S(-23,  -6),   S( 42,   2),   S( 11,  -9),   S( 21,   0),   S(-11,  13),   S( 55, -24),   S( 23, -10),   S( 30, -11),
            S( -2,   8),   S(-12,   5),   S(-14, -12),   S(-10,   2),   S( -4, -10),   S( 13, -28),   S(  0,   0),   S(  0,   0),
            S(-17, -49),   S(  8,   3),   S(  8, -13),   S( 24,  -7),   S(  1,   2),   S(  5,  -1),   S(  0,   0),   S(  0,   0),
            S( 14,  12),   S(  9, -16),   S(  0,  22),   S(  2, -18),   S( 11,   4),   S( -6, -28),   S( 13,  15),   S( -6, -33),
            S(  1,  -2),   S(-11, -41),   S( 16,   8),   S(  9,   2),   S( 11,  -2),   S(-11, -28),   S(  9,  -1),   S(-11,  -5),

            /* kings: bucket 8 */
            S( -9,  97),   S(-34,  41),   S(-34,  36),   S(-12, -18),   S(-43, -32),   S( -8,  -1),   S( 58, -18),   S(-15,  -8),
            S( 29, 101),   S( 19,  -3),   S(  3,  61),   S(  9,   4),   S( 10,  24),   S( 10,  17),   S( 17,  -7),   S( 17,  43),
            S(  0,   0),   S(  0,   0),   S( 11,  45),   S( 23,   9),   S( 18, -16),   S( -5,   1),   S(-11,   9),   S( -2,   1),
            S(  0,   0),   S(  0,   0),   S(  4,  14),   S( 30, -35),   S(  4,  13),   S(  1,  -7),   S( 32,  -9),   S(  3,  32),
            S(  4, -10),   S(  1,   5),   S(  9, -17),   S(  6, -10),   S( 10, -26),   S(  7,  -2),   S( 10,  10),   S(-16, -37),
            S(  3,  25),   S( -7, -20),   S(  0,  -9),   S( -7, -17),   S(-13, -20),   S( -2,  -2),   S(-12, -18),   S( 13,  15),
            S( -2, -17),   S(-10,  -4),   S(  8, -15),   S( -2,  10),   S(  0, -20),   S( -3, -36),   S(  4,   4),   S(  4, -48),
            S(  1,  -1),   S(-13, -46),   S(  9,  13),   S( -5,  -8),   S(  5,  18),   S(  6,   5),   S( 15,   3),   S( 14,  22),

            /* kings: bucket 9 */
            S(-32,  51),   S(-19,  38),   S(-67,  49),   S(-65,  16),   S(-71,  36),   S(-45,  22),   S( 64,   7),   S( 39,  32),
            S(-11,  16),   S( 15,  33),   S( -6,  14),   S( 38,  17),   S( 21,  21),   S( 29,  -3),   S( 20,  14),   S( 16,   1),
            S(-17,  17),   S( -2,  14),   S(  0,   0),   S(  0,   0),   S(  9,  21),   S( -5,  -7),   S( 31,  -4),   S(-30,   7),
            S( -8, -21),   S( -3,  -4),   S(  0,   0),   S(  0,   0),   S(  1,   2),   S( 46,   4),   S( -7,   9),   S( -4,  19),
            S( -1, -26),   S( -1,  11),   S( -3,   9),   S(  4, -15),   S(  2,  -7),   S(  8,  -9),   S( -9,  11),   S(-14,   6),
            S(  2,  34),   S( -8,  33),   S(  3,  24),   S( -4, -18),   S(  3,  -2),   S( -7,  18),   S(-17, -12),   S( -3,  41),
            S( -5, -15),   S(  1, -20),   S(  2,  13),   S(-10, -15),   S(  7,  30),   S( 20,  22),   S( -3, -27),   S( -2,  15),
            S(  6,  45),   S( -2,  -7),   S( 15,  22),   S( -2, -12),   S(-16, -63),   S(  1,  -6),   S(  4, -22),   S(  3,   5),

            /* kings: bucket 10 */
            S(  0,  20),   S(-10,   7),   S(  6,   9),   S(-28,  27),   S(-61,  24),   S(-106,  36),  S(  1,  31),   S(-81,  76),
            S(  4,   6),   S( 44,  17),   S(  2, -10),   S( 19,  12),   S( 66,  11),   S(  7,  14),   S( 13,  26),   S(-35,  38),
            S(  4,   8),   S( 18,   7),   S( 15,   2),   S(-24,   6),   S(  0,   0),   S(  0,   0),   S(  9,  14),   S(-42,  12),
            S( 17,  -5),   S( -4, -13),   S(  3,  -7),   S( 25,  -6),   S(  0,   0),   S(  0,   0),   S( 15,  24),   S(  5, -11),
            S( -8,  -5),   S( 15,  20),   S(  0,  11),   S(  8, -43),   S(  2, -20),   S(  4,  19),   S(  3,  -4),   S( -3,  14),
            S( -8,   4),   S( -3,  29),   S(-12,  23),   S( -3,  10),   S( -2,   4),   S( -4,  -3),   S( -7,  19),   S( -3,  30),
            S( 10,  10),   S(  3, -18),   S(  1,  -1),   S( 13,  21),   S( 16,  41),   S(-10, -28),   S( 21,  -5),   S( -4,  24),
            S( -1,  -6),   S( 11,   0),   S( -3, -27),   S(  0,  -7),   S(  3,  -4),   S( -3, -26),   S( -8, -20),   S(  4,  29),

            /* kings: bucket 11 */
            S(-36,  33),   S( 28,   4),   S(  7,   7),   S(-12,   6),   S(-11,   4),   S(-175,  69),  S(-59,  56),   S(-138, 140),
            S(  6, -35),   S(  6,   5),   S(  1, -27),   S( 24,   9),   S( 19,   9),   S(  0,  54),   S( 25,   9),   S( 25,  53),
            S( -3,   5),   S( 16,  -6),   S( -8,  -1),   S( 29,  -2),   S( 66,  -3),   S( 39,  26),   S(  0,   0),   S(  0,   0),
            S(  5,  -3),   S( -2,  12),   S( 17,   0),   S( 26,  -4),   S( 43, -11),   S( 24,   7),   S(  0,   0),   S(  0,   0),
            S( 12,  20),   S(-17,  18),   S( 11,  -3),   S( 14, -10),   S( 15, -15),   S( -4,  -8),   S(  1,  11),   S( -4, -24),
            S( 11,  -4),   S(  0,  -2),   S( 28, -21),   S(-10, -13),   S(  1, -31),   S( -1,  -4),   S(  1,  18),   S(  2,  -4),
            S( 13,   8),   S(  6, -10),   S( 16,  40),   S(  7, -13),   S(  6,  -7),   S(  2,  22),   S( -1, -15),   S(-13, -17),
            S(  2,  13),   S(  7, -18),   S(-17, -74),   S(  2,  -2),   S(-11,  -7),   S(-14, -44),   S(  6,   5),   S( 12,  31),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( -6,  57),   S(-12,  -3),   S( -5,  -3),   S( -1,  12),   S( 14, -29),   S( -2,  41),
            S(  0,   0),   S(  0,   0),   S( 24,  88),   S( 17,   9),   S( 17,  35),   S(  9,  -3),   S( 16,   2),   S( -6,   6),
            S( -2,  -3),   S( -1, -37),   S( 14,  28),   S(  2,  18),   S(  8, -19),   S( -9,  27),   S( -9, -23),   S( -9,  17),
            S( -3,   0),   S(  8,  22),   S( -5, -28),   S(  2, -42),   S(  4,  -8),   S(  6, -14),   S( 10,  44),   S( -6,   8),
            S( 14,  42),   S(  1,   6),   S(  3,   8),   S(  2,  -7),   S(-10,  13),   S(-10,  -2),   S(-10,   9),   S(-10, -12),
            S(  2,  -1),   S(  6,  22),   S( -6,   7),   S(  1, -12),   S(  2, -18),   S( -6,  20),   S(-16,  -8),   S(  8,  18),
            S(  4,  11),   S( -1,  -4),   S( -3,  -1),   S(  4,  -9),   S( -9, -25),   S(  4,  17),   S(  6,  28),   S(  0,  -9),
            S( -6, -13),   S(  4,   6),   S( -1, -28),   S( -4, -22),   S(  2,   3),   S(  7,  13),   S(-16, -55),   S( -1,  -8),

            /* kings: bucket 13 */
            S( -5,  84),   S( -6,  64),   S(  0,   0),   S(  0,   0),   S(  0,  73),   S(-29,  -9),   S(  8,  -6),   S(-17,  63),
            S(-10,  36),   S( -7, -18),   S(  0,   0),   S(  0,   0),   S( 23,  -8),   S( -4, -23),   S(-23,   0),   S(-10,  30),
            S( -9, -16),   S(  9,  43),   S( -7, -62),   S(  1,  19),   S( 14,  23),   S( -1,  -1),   S(  1,  10),   S( -6,   8),
            S( -2, -15),   S(  7,  18),   S(  4,  11),   S( -5, -52),   S( -4, -50),   S( -4, -16),   S( -8, -14),   S(-13, -19),
            S(  2,   3),   S( -4,  -7),   S(  5,   2),   S( -6, -14),   S( -4,   5),   S(  0,  22),   S(-18,  32),   S(  0,  11),
            S(  4,  19),   S(-12, -16),   S( -1,  11),   S( -3,  -6),   S( -7, -24),   S(  0,  10),   S(  0,   5),   S( 10,   2),
            S(  6,  25),   S( -9,  -2),   S(-13, -30),   S(  3,   7),   S( -4, -40),   S( 14,  15),   S( -9, -60),   S( 10,  48),
            S(  4,   6),   S(  2,  30),   S(  3,  14),   S(  7,  10),   S(  5,   4),   S(-14, -30),   S( -1,  -1),   S(  6,   8),

            /* kings: bucket 14 */
            S(  2,  60),   S( -6,   8),   S(-14, -30),   S( 10,  47),   S(  0,   0),   S(  0,   0),   S( -1,  81),   S(-40,  84),
            S(-23,  10),   S( -5, -26),   S( -9, -17),   S( -3,  -1),   S(  0,   0),   S(  0,   0),   S(  5,   5),   S(-29,  -4),
            S( -8,   8),   S( -6,  -9),   S( 21, -12),   S( 11,  16),   S(  3, -29),   S(  0,  28),   S(  9,  62),   S(-20, -16),
            S( -4,   1),   S( -2, -15),   S( -6, -40),   S(  8, -29),   S( -9, -37),   S(  7,  16),   S(  3,  -1),   S(  3,   8),
            S(  9,  50),   S( -1,  -6),   S(-10,  16),   S( -8, -19),   S( -9,   8),   S(  5,  31),   S(  3,  25),   S( 13,  17),
            S( -6, -10),   S( -9,  -3),   S(  4,  -5),   S(  2,  26),   S( -2,   4),   S(  1, -27),   S( -7, -18),   S(  8,  13),
            S(  2,  -4),   S( -8, -56),   S(  0,  34),   S(  4,  10),   S( -6, -12),   S( -1, -21),   S( -9, -69),   S( 17,  66),
            S( -2,   2),   S(  5,  51),   S(  2,  17),   S( -5, -10),   S(  7,  46),   S(  0, -14),   S(-12, -41),   S(  2, -21),

            /* kings: bucket 15 */
            S( 17,  44),   S( -8,  -5),   S( -1,  -8),   S( -9, -19),   S(-36,  -2),   S(-23,  76),   S(  0,   0),   S(  0,   0),
            S( -6, -45),   S(  1,  23),   S( -9, -29),   S( 27,  55),   S( 30, -10),   S( 27,  94),   S(  0,   0),   S(  0,   0),
            S(-16,  14),   S(  2, -22),   S( -4, -32),   S( -5, -23),   S( 16, -31),   S( 30,  61),   S( 13,   9),   S(-17, -17),
            S( 10,  29),   S( -3,  -2),   S( -4,   1),   S(-10, -19),   S( -5, -68),   S( 10,  16),   S( -3,  45),   S(  2, -15),
            S( -8, -32),   S(-16,  -7),   S( -6,  16),   S( -9, -28),   S(  7, -27),   S( -7,  12),   S( -5, -15),   S(  4,  27),
            S(  9,  49),   S(-22,   8),   S( -1,  -7),   S( 13,  48),   S(-11, -14),   S( -2,   0),   S( -7, -12),   S(  5,  -4),
            S( 10,  29),   S( -3,   2),   S( -8, -12),   S(  6,  16),   S(  3,  -2),   S( -2,  20),   S( -4,  -6),   S( -2,   0),
            S( -6,  -7),   S( 10,  21),   S( -2,   6),   S( -2,  -4),   S(  2,  29),   S(  0,  20),   S( -1,   5),   S(  6,   2),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-14, -187),  S(  7, -32),   S( 16,  40),   S( 25,  79),   S( 32, 102),   S( 37, 122),   S( 43, 121),   S( 57, 107),
            S( 80,  73),

            /* bishop mobility */
            S(  2, -72),   S(  7,   6),   S( 16,  51),   S( 22,  90),   S( 27, 119),   S( 29, 138),   S( 31, 148),   S( 30, 151),
            S( 34, 153),   S( 37, 150),   S( 47, 141),   S( 63, 134),   S( 77, 138),   S(105, 112),

            /* rook mobility */
            S(-23, -11),   S(-14,  50),   S(-14,  81),   S( -9,  98),   S(-11, 122),   S( -4, 131),   S(  1, 140),   S(  9, 141),
            S( 13, 149),   S( 19, 154),   S( 29, 154),   S( 40, 151),   S( 50, 146),   S( 72, 133),   S(152,  91),

            /* queen mobility */
            S( 50, -56),   S( 49, -60),   S( 45,  43),   S( 40, 125),   S( 39, 175),   S( 41, 200),   S( 41, 237),   S( 40, 261),
            S( 41, 277),   S( 41, 298),   S( 38, 320),   S( 33, 345),   S( 31, 360),   S( 30, 366),   S( 30, 380),   S( 21, 395),
            S( 29, 384),   S( 44, 373),   S( 55, 362),   S( 69, 347),   S( 86, 327),   S(127, 301),   S(149, 277),   S(165, 262),
            S(169, 255),   S(167, 243),   S(122, 203),   S(107, 226),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,  25),   S(-20,  59),   S(-33,  57),   S(-25,  85),   S( -4,  34),   S(-18,  40),   S(-11,  75),   S( 35,  32),
            S( 15,  39),   S(  6,  55),   S(-20,  55),   S(-16,  47),   S( -4,  43),   S(-31,  61),   S(-37,  81),   S( 37,  35),
            S( 43,  64),   S( 39,  62),   S( 13,  51),   S( 34,  37),   S( 11,  45),   S(-20,  65),   S(-20,  96),   S( 24,  63),
            S( 54,  84),   S( 54,  97),   S( 28,  64),   S( 14,  47),   S(  3,  54),   S( 13,  71),   S(-19, 104),   S(-44, 116),
            S( 59, 124),   S( 98, 142),   S( 91,  98),   S( 38,  77),   S(-52,  75),   S( 52,  71),   S(-71, 144),   S(-117, 138),
            S(106, 176),   S( 94, 214),   S(127, 191),   S(145, 177),   S(129, 201),   S(109, 202),   S(118, 198),   S(144, 204),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  11),   S(  1,   6),   S(  8,   3),   S(  6,  29),   S( 23,  34),   S( -2, -18),   S(-15,  33),   S(  6, -28),
            S( -4,  22),   S( 23,  17),   S(  3,  41),   S( 19,  43),   S( 40,   3),   S( -4,  35),   S( 18,  -2),   S(  5,  11),
            S(-11,  14),   S( 17,  15),   S( -3,  47),   S( 21,  71),   S( 27,  26),   S( 26,  31),   S( 28,   1),   S( -2,  27),
            S( 10,  25),   S( 31,  60),   S( 40,  73),   S( 15, 106),   S( 80,  54),   S( 79,  45),   S( 23,  51),   S( 24,  15),
            S( 54,  84),   S(115, 107),   S(100, 146),   S(131, 169),   S(158, 179),   S(139, 151),   S(183,  95),   S(117,  86),
            S( 80, 188),   S(111, 264),   S( 85, 214),   S(107, 215),   S( 77, 166),   S( 52, 143),   S( 43, 159),   S( 10,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  47),   S( 27,  34),   S( 40,  57),   S( 39,  42),   S( 29,  36),   S( 27,  41),   S(  8,  22),   S( 40,  18),
            S(  0,  21),   S( 14,  44),   S( 14,  39),   S( 12,  47),   S( 26,  16),   S(  8,  24),   S( 25,  23),   S( -1,  15),
            S(  1,  10),   S( 22,  24),   S( 54,  38),   S( 39,  28),   S( 40,  40),   S( 61,   9),   S( 19,  20),   S( 20,   1),
            S( 70,  32),   S(132,   4),   S(140,  69),   S(182,  89),   S(150,  81),   S( 91,  88),   S(114,  -2),   S( 89, -19),
            S( 98,  43),   S(176,  41),   S(203, 138),   S(140, 162),   S(170, 195),   S(150, 187),   S(172, 119),   S(-52,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 24,  22),   S(  8,  11),   S( 11,  24),   S( -7,  54),   S( 68,  33),   S( 18,   5),   S(  0,  -9),   S( 30,  13),
            S(  4,  13),   S(  7,   6),   S( 19,  15),   S( 17,  27),   S( 14,  15),   S( -1,  10),   S(  5,   7),   S( 27,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -4, -13),   S( -7,  -6),   S(-19, -15),   S(-17, -27),   S(-14, -15),   S(  1, -10),   S( -5,  -7),   S(-27,   4),
            S(-24, -22),   S( -8, -11),   S(-11, -24),   S(  7, -54),   S(-68, -33),   S(-18,  -5),   S(  0,   9),   S(-30, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -21),   S( -9, -13),   S( -1, -31),   S(-46,  -7),   S(-18, -13),   S(-13, -24),   S(  2, -22),   S(-13, -40),
            S(-18,   5),   S(-13,  -4),   S(-23,  10),   S(  4, -14),   S(-30, -14),   S(-29,  -2),   S(-41,   5),   S(-11, -12),
            S(-12, -19),   S(-12, -19),   S(-26,  -3),   S(-28, -13),   S(-26, -30),   S(-21, -13),   S(-29,  -3),   S(-39, -18),
            S(  5, -31),   S( 18, -42),   S( 20, -25),   S(  5, -27),   S(  4, -19),   S( 54, -32),   S( 39, -36),   S( -6, -44),
            S( 30, -59),   S( 61, -74),   S( 69, -40),   S( 76, -32),   S( 70, -25),   S( 61, -15),   S(137, -77),   S( 32, -72),
            S(147, -107),  S(164, -132),  S(141, -68),   S(118, -45),   S(118, -36),   S(157, -42),   S(116, -49),   S( 87, -96),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 16,   2),        // attacks to squares 1 from king
            S( 17,   2),        // attacks to squares 2 from king

            /* castling available */
            S( 68, -73),        // king-side castling available
            S( 26,  33),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 13,  63),   S( 15,  69),   S(  5,  68),   S( -7,  82),   S(-17,  92),   S(-34, 102),   S(-67, 119),   S(-75, 124),
            S(-95, 131),   S(-81, 135),   S(-76, 141),   S( 20, 126),   S( -5, 132),

            /* orthogonal lines */
            S(-11, -53),   S(-52, -21),   S(-76, -12),   S(-90,  -9),   S(-97, -16),   S(-99, -19),   S(-98, -27),   S(-96, -28),
            S(-115, -21),  S(-131, -22),  S(-158, -27),  S(-162, -32),  S(-182, -21),  S(-365,  38),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 16, 236),

            /* passed pawn can advance */
            S(-30,  37),   S(-19,  60),   S(-14,  91),   S( 21, 121),

            #endregion

            /* tempo bonus for side to move */
            S(  7,  10),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
