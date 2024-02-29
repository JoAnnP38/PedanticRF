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
        public const int MAX_WEIGHTS = 12712;
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
        public const int TEMPO = 12711;             // tempo bonus for side moving

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

        // Solution sample size: 6000000, generated on Wed, 28 Feb 2024 12:15:59 GMT
        // Solution K: 0.003850, error: 0.086362, accuracy: 0.4938
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 90, 180),   S(417, 534),   S(402, 631),   S(562, 962),   S(1291, 1711), S(  0,   0),

            /* friendly king piece square values */
            #region friendly king piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(107, -136),  S(153, -104),  S( 29, -31),   S(-30,  25),   S(-47,  32),   S(-17, -10),   S(-54,  -2),   S(-45, -23),
            S(109, -121),  S(100, -103),  S(  5, -63),   S( -4, -67),   S(-28, -16),   S(-16, -40),   S(-41, -26),   S(-35, -45),
            S( 92, -91),   S( 71, -56),   S( 17, -46),   S(  4, -62),   S( -6, -61),   S(  2, -57),   S(-27, -39),   S(-21, -38),
            S( 61, -35),   S( 49, -43),   S( 21, -48),   S( 10, -63),   S( -7, -54),   S(-29, -41),   S(-32, -46),   S(-48, -10),
            S( 77,  31),   S( 31,  -6),   S( 72, -26),   S( 30, -66),   S( 20, -45),   S( -3, -42),   S(-54,   3),   S(-53,  63),
            S( 53,  42),   S( 49,  63),   S(  8,  -6),   S(-29, -29),   S(-71, -39),   S( -9,   6),   S( 25, -32),   S(-31,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 28, -33),   S( 27, -42),   S( 45, -27),   S( -6,  10),   S( -7, -27),   S(  5, -18),   S(-39, -12),   S(-43,  15),
            S( 20, -40),   S(  4, -43),   S(  6, -47),   S(-10, -41),   S(-11, -24),   S( -7, -33),   S(-41, -16),   S(-52, -10),
            S(  8, -27),   S(  7, -21),   S( 22, -45),   S( 13, -51),   S(-12, -30),   S(  7, -34),   S(-20, -23),   S(-29,  -9),
            S( 19,  -3),   S( 13, -36),   S( 23, -36),   S(  2, -45),   S(  5, -29),   S( -2, -30),   S(-36, -17),   S(-41,  18),
            S(  0,  63),   S(-31,   8),   S( -8,  -7),   S( -8, -13),   S( 21,   4),   S(-31,   3),   S(-21,  14),   S(-38,  67),
            S( 52,  54),   S( -3,  25),   S(-87,  -7),   S(-26,  13),   S(-30, -33),   S(-71,   4),   S(-108,  46),  S(-93,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24,  -5),   S(-28,  -6),   S(-18,   0),   S(-17,  12),   S(  6, -16),   S( 31, -23),   S(  4, -47),   S(-14, -19),
            S(-26, -18),   S(-44, -12),   S(-31, -36),   S(-21, -40),   S(  1, -31),   S(  1, -28),   S(-12, -36),   S(-28, -20),
            S(-41,  -5),   S(-30, -21),   S(-14, -45),   S(  1, -61),   S(  7, -29),   S( 10, -25),   S( -5, -31),   S(-11, -16),
            S(-51,  15),   S(-30, -30),   S(-25, -33),   S( -9, -48),   S(  8, -36),   S(-11, -12),   S(-18, -10),   S(-27,  14),
            S(-38,  60),   S(-74,  14),   S(-41, -31),   S(-55, -11),   S( 29,  -7),   S(-14,  16),   S(-17,  19),   S(-41,  95),
            S(-95,  88),   S(-128,  52),  S(-122,   9),  S(-87, -35),   S(-49,  16),   S(-76,  10),   S(-67,  33),   S(-60,  86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -10),   S(-36,  -4),   S(-31,  -6),   S( -6, -35),   S(-15,  -5),   S( 32, -14),   S( 83, -68),   S( 64, -86),
            S(-36, -30),   S(-48, -22),   S(-34, -44),   S(-20, -39),   S(-24, -19),   S(  1, -33),   S( 56, -65),   S( 56, -76),
            S(-45, -21),   S(-23, -40),   S( -8, -61),   S( -2, -68),   S( -5, -49),   S(  5, -36),   S( 33, -53),   S( 62, -59),
            S(-47,  -6),   S(-20, -51),   S(-14, -56),   S(  0, -70),   S( 13, -60),   S( -2, -44),   S( 18, -34),   S( 47, -14),
            S(-22,  42),   S(-42,  -3),   S( -3, -53),   S(-13, -46),   S( 78, -44),   S( 55, -37),   S( 39,  35),   S( 32,  90),
            S(-102, 122),  S(-103,  51),  S(-38, -50),   S(-65, -42),   S(  2, -39),   S( 27, -41),   S( 38,  20),   S(  5,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-74,  -3),   S( 30, -48),   S(-20,  -6),   S(-20,  17),   S(  2, -37),   S(-40,   3),   S(-94,  14),   S(-70,   9),
            S(-11, -10),   S( 45, -28),   S( 36, -42),   S( 12, -18),   S(-18, -15),   S(-34, -29),   S(-34, -28),   S(-14, -26),
            S( 39, -24),   S( 43, -15),   S(  1,   1),   S(-16, -19),   S(-44, -18),   S( 21, -55),   S(-66, -27),   S(-33, -12),
            S( 26,  22),   S( -9,  36),   S( 32,   9),   S( 14,  -8),   S( 12, -35),   S(-64, -16),   S( 18, -49),   S( 53, -28),
            S(-18,  98),   S( 33,  84),   S(  3,  31),   S(  4, -13),   S( 32,   7),   S(-30,  -1),   S(-49, -44),   S( 25,  27),
            S( 74,  84),   S( 62,  70),   S( 31,  17),   S( 11,  11),   S(  9, -42),   S(-11, -14),   S(  6, -25),   S(-36, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-92,  50),   S(-52,  18),   S( -4,   6),   S( 38,  -3),   S(-14,  11),   S(-15,   8),   S(-41,   3),   S(-24,  16),
            S(-71,  20),   S(-40,   2),   S( 28, -21),   S(-14,   8),   S( 14, -14),   S(-26, -15),   S(-24, -13),   S(-49,  14),
            S(-75,  47),   S(-24,  18),   S( 79, -36),   S( 22, -35),   S( 36, -12),   S(-44,  -1),   S(-18,  -3),   S(-21,   5),
            S(-39,  57),   S( -4,  22),   S( -2,  16),   S( 70,  -4),   S(  6,   4),   S(-52,  12),   S( -5,  -6),   S( 12,  11),
            S( 44,  65),   S( 49,  37),   S( 75,  36),   S( 24,  37),   S(-42,  53),   S( 85,  -9),   S( 19,  13),   S(-23,  68),
            S( 99,  35),   S( 62,  14),   S( 71,  -8),   S( 39,  -1),   S( 29,  25),   S( 18,  -7),   S(-12,   9),   S( 25,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39,  18),   S(-26,   1),   S(  0,   1),   S( -8, -15),   S(  8, -20),   S(-20,  -3),   S(-51,  -5),   S(-77,  22),
            S(-40,  -1),   S(-21, -20),   S(-16, -31),   S( 27, -27),   S( 60, -30),   S( 38, -31),   S(-18, -16),   S(-85,  15),
            S(-44,  12),   S( -6,  -8),   S( -4, -19),   S(-11, -16),   S( 26, -12),   S( 67, -32),   S(  0, -14),   S(-52,  22),
            S(-26,  20),   S(-39,   2),   S(  5,  -8),   S(  6,  -8),   S( 18,  12),   S( 58,  -5),   S( 21,  -3),   S( 13,  18),
            S(-36,  54),   S(-46,  21),   S(-36,   3),   S( 14, -11),   S( 67,  31),   S( 74,  24),   S( 55,   9),   S( 67,  47),
            S( 37,  57),   S(  6,  -2),   S(  0, -31),   S( 15, -46),   S( 20,   6),   S( 46,   1),   S( 14,  29),   S( 48,  30),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -18),   S(-53, -18),   S(  5, -18),   S(-40, -26),   S(-27, -26),   S( 38, -34),   S( -3, -50),   S(-48, -28),
            S(-54, -32),   S(-46, -44),   S(-33, -46),   S(-25, -40),   S( -8, -30),   S( 54, -50),   S( 53, -49),   S(-20, -35),
            S(-70, -20),   S(-40, -37),   S(-22, -47),   S( -3, -51),   S( -2, -26),   S( 23, -25),   S( 61, -53),   S( 45, -39),
            S(-52, -19),   S(-58, -35),   S(-57, -40),   S(-34, -21),   S( -5, -16),   S( 21,  -6),   S( 43, -14),   S( 66, -16),
            S(-27,  -7),   S(-14, -46),   S(-28, -28),   S( -7, -39),   S( 40,   5),   S( 32,  -3),   S( 58,  55),   S( 86,  53),
            S(-14,  15),   S( 19, -43),   S( 41, -54),   S( 25, -59),   S( 13, -31),   S( 51, -38),   S( 45,  24),   S( 63,  89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36,  33),   S(-32,  25),   S(-13,  20),   S(-16, -21),   S(  0,  -4),   S(-49,  13),   S(-34,  -9),   S(-55,  13),
            S(-44,  22),   S( -6,   5),   S(-31,   2),   S(-32,  -2),   S(-37, -13),   S(-47, -14),   S(-44, -18),   S(-21, -11),
            S( -6,  66),   S( 17,  70),   S( 20,  42),   S(-29,  21),   S( -7,  -6),   S(-20, -38),   S(-51, -29),   S( 10, -20),
            S( 26,  93),   S( 51, 110),   S( 54,  68),   S( 26,  37),   S(  2,  -6),   S(-22, -20),   S( -3, -28),   S( 11, -32),
            S( 70, 134),   S( 69, 131),   S( 61, 120),   S( 63, 101),   S( 20,   3),   S(  8,  11),   S(-11, -28),   S(  4,   9),
            S( 98, 153),   S( 94, 163),   S(113, 197),   S( 41,  67),   S(  8,   4),   S( 11,  -1),   S( -3, -47),   S( 11, -16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-99,  60),   S(-73,  33),   S( -2,  23),   S( 21,  30),   S(  1,   7),   S(-68,  22),   S(-78,  19),   S(-90,  34),
            S(-55,  21),   S(-51,   4),   S(-48,  17),   S( 10,  35),   S(-81,  18),   S(-56,  -3),   S(-114,   4),  S(-46,   5),
            S(-60,  61),   S(-17,  66),   S( -4,  64),   S(-75,  64),   S(-68,  54),   S(-101,  21),  S(-27, -17),   S(-30,   2),
            S(-26,  82),   S( 46,  80),   S( 74, 106),   S( 73, 113),   S(-11,  62),   S(-11,  15),   S( 26,  -8),   S( 73, -18),
            S( 95,  74),   S( 78, 122),   S( 76, 132),   S(106, 144),   S( 67, 111),   S( 31,  34),   S( 32,   0),   S( 48, -12),
            S( 37,  76),   S( 76,  97),   S(114, 117),   S(107, 136),   S( 40,  86),   S(  5,  24),   S( 10, -20),   S( 10,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-122,  18),  S(-133,  14),  S( -6,   9),   S(  3,  22),   S(-24,  11),   S(-103,  34),  S(-142,  34),  S(-85,  40),
            S(-134,   3),  S(-54, -20),   S(-44, -11),   S(-60,   2),   S(-54,  35),   S(-37,  12),   S(-143,  34),  S(-147,  31),
            S(  0,  -5),   S(-59,  -6),   S( -4,   2),   S(-83,  64),   S(-93,  86),   S( 30,  30),   S(-56,  29),   S(-75,  38),
            S( 28, -20),   S( 13, -12),   S( 18,  23),   S(-42,  83),   S( 65,  89),   S( -4,  86),   S( 23,  40),   S( 58,  16),
            S( 73,   3),   S(  9,  -2),   S( 18,  49),   S( 81,  94),   S( 90, 122),   S( 46,  83),   S( 75,  86),   S( 63,  46),
            S( 62,  -3),   S( 22, -25),   S( 19,   5),   S( 90, 129),   S( 31,  71),   S( 51,  69),   S( 52,  52),   S( 35,  85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-115,   8),  S(-75,  10),   S( 21, -13),   S(-20,  -5),   S( -9,  28),   S(-106,  43),  S(-43,  22),   S(-68,  34),
            S(-100, -19),  S(-62, -32),   S(-44, -40),   S(-84,   6),   S(-35,  -3),   S( -8,   3),   S(-98,  45),   S(-104,  34),
            S(-46, -30),   S(-34, -46),   S(-44,  -1),   S(-31,   2),   S(-37,  22),   S( 15,  45),   S(-42,  52),   S(-40,  52),
            S( 50, -23),   S(-54, -30),   S(-26, -13),   S(-34,  14),   S( -4,  49),   S( 46,  61),   S( 49,  96),   S(105,  47),
            S(  4,   5),   S(-32, -10),   S(  7,  -1),   S( -5,  13),   S( 64,  98),   S( 47,  85),   S(147, 111),   S(162,  79),
            S( 19, -18),   S( -1, -29),   S(  4, -27),   S( 12, -12),   S( 50,  42),   S( 29, 126),   S( 75, 159),   S(123, 126),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,  16),   S( -5,   3),   S(-25, -18),   S(  1,   4),   S( -4,   6),   S( -6,   4),   S(-16, -11),   S(-55,   5),
            S(-26, -29),   S(  4,  -5),   S( -2,   8),   S(  3, -11),   S( 19,  59),   S(-18,  10),   S(-43, -34),   S(-14, -63),
            S( -6,  55),   S( 19,  50),   S( 22,  13),   S( 27,  32),   S(  2,  21),   S(-14,  -9),   S(-22, -61),   S(-33, -47),
            S(-14,  73),   S( 12,  82),   S( 78, 104),   S( 56,  85),   S(-20, -64),   S(-20, -43),   S( 10, -30),   S(-44, -66),
            S( 65,  81),   S( 66, 183),   S( 67, 154),   S( 28,  69),   S(  7,   7),   S(-10, -41),   S(  2, -32),   S(-15, -94),
            S( 54, 128),   S( 52, 279),   S(117, 269),   S( 50,  95),   S(  4,  18),   S( -3, -54),   S( -7, -43),   S( -4, -51),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62, -35),   S(-15,  11),   S( -7,  -3),   S(  1,  11),   S( -5,  -5),   S(-25, -24),   S(-49, -27),   S(-37,  -2),
            S(-15, -43),   S(-35, -17),   S(  1,  -6),   S(  3,  28),   S(-47,   1),   S(-16, -13),   S(-56, -54),   S( -8, -18),
            S( 40,   6),   S( 10,   4),   S(-14,   6),   S( 15,  62),   S(-33,  32),   S(-22, -31),   S(-19, -43),   S(-11, -73),
            S( 14,  47),   S( 59,  57),   S( 49,  99),   S( 37,  79),   S( 15,  66),   S( 14, -16),   S( 25, -31),   S(  8, -64),
            S( 33,  96),   S( 79, 161),   S( 93, 161),   S(108, 183),   S( 62, 128),   S( 11,  11),   S(  8, -52),   S(  5, -84),
            S( 46,  97),   S( 94, 187),   S( 72, 202),   S(120, 239),   S( 65, 145),   S( 10,   6),   S( 13,  -6),   S( 19, -32),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -36),   S(-28, -66),   S( -3, -28),   S( -3,   3),   S(-10, -10),   S(-31,  16),   S(-33,  -5),   S(-20,  40),
            S(-29,  -5),   S(-22, -32),   S(-12, -19),   S(  1,  16),   S(-23,  35),   S( 11,  26),   S(-29,  14),   S(-42,  -7),
            S(-19, -27),   S(-15, -24),   S(-22, -26),   S( 10,   9),   S(  1,  26),   S(-19,  13),   S(  2,   3),   S(  8,  26),
            S( 16, -51),   S( 24, -30),   S( 23,   0),   S( 19,  61),   S( 24, 149),   S( 18,  73),   S(  6,  24),   S( 25,  26),
            S( 14, -29),   S( 29,   3),   S( 42,  44),   S( 50, 140),   S( 64, 185),   S( 77, 141),   S( 43,  61),   S( 43,  47),
            S( 32,   2),   S( 27,  23),   S( 48, 106),   S( 71, 180),   S( 88, 244),   S( 41, 152),   S( 52, 133),   S( 36,  82),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47, -55),   S(-29, -46),   S(-35, -85),   S(  1,   2),   S( 27,  59),   S( 11,  48),   S(-19, -11),   S( 12,  44),
            S( -9, -75),   S(-25, -25),   S(-38, -63),   S( 23,  50),   S(-28,  13),   S(  9,  32),   S(  4,  41),   S(-14,  29),
            S(  6, -68),   S( -2, -82),   S(-27, -45),   S(  1,   3),   S(  8,  41),   S( 19,  20),   S( 12,  67),   S( 19,  56),
            S( -2, -52),   S(-35, -38),   S(-12, -30),   S( 16,  32),   S(  9,  35),   S( 49,  66),   S( 29, 126),   S( 12,  83),
            S(-16, -74),   S( -5, -60),   S( -2,  -3),   S(  5, -19),   S( 46, 128),   S( 95, 131),   S( 58, 259),   S( 63,  84),
            S( 19,  -2),   S(  7,   8),   S(  7,  13),   S(  8,  20),   S( 29,  92),   S( 93, 265),   S( 22, 206),   S( 52,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-36, -14),   S( 24,  10),   S(-42,   7),   S(-22, -28),   S(-42, -33),   S(-13, -42),   S(-47, -58),   S(-50,  -7),
            S(-42,  34),   S( 34, -39),   S(-40, -12),   S( 10, -40),   S( -3, -47),   S(-19,   0),   S(-17, -44),   S(-68, -14),
            S(-13,  78),   S(-14, -15),   S(  7, -30),   S(-12,  22),   S( 13,  14),   S(-28,  -4),   S(-13, -28),   S(-42, -57),
            S( 18, -25),   S( 29,  -4),   S(  1,  21),   S( 18,  17),   S(  2,   7),   S( 13,  -8),   S( -7, -28),   S(  7, -14),
            S( -7,   1),   S( 55, -28),   S( 15, -16),   S( 60,  -1),   S( 47, -15),   S( 48,  -9),   S( -2,  -9),   S(-51, -46),
            S( 18, -58),   S(  5,   3),   S( 35,  22),   S( 74, -12),   S( 36, -70),   S( 34,   4),   S( -8, -29),   S(-34, -26),
            S(  0, -18),   S( 27, -27),   S( 24, -24),   S( 39, -70),   S( -4, -34),   S( 15, -35),   S(-15, -60),   S(  7, -17),
            S(-106, -65),  S( -7,  -2),   S(-21, -37),   S( -4, -49),   S(-35, -52),   S( 23,  21),   S( -8,  -4),   S(  6,  -1),

            /* knights: bucket 1 */
            S(-52,  31),   S(-50,  72),   S(  4,  25),   S(-18,  49),   S(-24,  37),   S(-21,  31),   S(-36,  45),   S(-19,   7),
            S( 21,  41),   S( -5,  30),   S(  8,  -3),   S( -8,  18),   S( -1,  12),   S(  0,   8),   S( 17, -30),   S(-31,  -5),
            S(-23,  14),   S( -3,  14),   S(  3,   5),   S( 19,  17),   S(  9,  20),   S(-29,  22),   S(-12, -13),   S(-30,  24),
            S(  0,  28),   S( 63,  19),   S( 25,  39),   S( 18,  21),   S( 14,  25),   S( -2,  17),   S( 15,   1),   S( 23,  18),
            S( -4,  49),   S( 32,   4),   S( 17,  40),   S( 45,  12),   S( 45,  17),   S( 20,  20),   S( 17,  -2),   S(  8,  -2),
            S( 13, -10),   S( 31, -10),   S( 26,  34),   S( 60,  13),   S( 39,  16),   S( 41,  39),   S( 65, -14),   S( -2,  -2),
            S( 29,  -6),   S(  1,  12),   S(-15, -12),   S( 16,  -6),   S( 44, -28),   S( 73, -42),   S(-25,  -2),   S( 15, -54),
            S(-75, -65),   S(-25, -18),   S(-20,  11),   S(  9,  -3),   S(-13,   2),   S( -7, -38),   S( -7, -26),   S(-48, -64),

            /* knights: bucket 2 */
            S(-44,  12),   S( -5,  23),   S(-42,  54),   S(-27,  43),   S(-22,  35),   S(-40,  53),   S(-25,  52),   S(-36,  23),
            S(-41, -11),   S(-41,  11),   S(-16,  26),   S(-16,  26),   S( -7,  14),   S(-16,  44),   S(-35,  53),   S(-49, 101),
            S(-29,  18),   S(-10,  18),   S(-19,  38),   S( 13,  25),   S(  0,  29),   S( -8,  16),   S(-14,  30),   S(-39,  27),
            S(-18,  44),   S(-28,  34),   S( -2,  49),   S( -1,  47),   S( -6,  51),   S( -9,  38),   S( -9,  40),   S( -3,  34),
            S(  6,  17),   S(-20,  29),   S(-24,  50),   S(-21,  54),   S(-11,  54),   S(-16,  52),   S(-13,  42),   S(-29,  26),
            S( -2,  14),   S(  2,  32),   S(-28,  53),   S( -4,  37),   S(  0,  34),   S(-20,  44),   S(-12,  10),   S( 14,   1),
            S(-13,  23),   S(-48,  15),   S( -7,  14),   S(-21,  31),   S(-11,  -1),   S( 13,   9),   S(-28,  22),   S(-11,   0),
            S(-143,  10),  S(  3,   0),   S(-40,  41),   S(-25, -19),   S(  6, -12),   S(-71, -27),   S(  2,   4),   S(-189, -41),

            /* knights: bucket 3 */
            S(-62,  15),   S( -4, -11),   S(-52,  14),   S(-13,  23),   S(-15,  23),   S(-21,  16),   S( -2,   8),   S(-17,  -8),
            S(-27,  15),   S(-47,  16),   S(-29,   4),   S(  0,  26),   S(  2,  20),   S(-23,  -1),   S(-26,  16),   S(-42,  61),
            S(-20, -17),   S(-11,  10),   S(-14,  16),   S( 14,  28),   S( 11,  32),   S(  4,  18),   S( -9,  11),   S(-19,  57),
            S(-19,  24),   S( -7,  32),   S( -3,  54),   S(  7,  46),   S( 13,  54),   S( 13,  51),   S( 15,  33),   S(  8,  27),
            S( 11,  20),   S( -8,  34),   S( 12,  29),   S( 14,  58),   S(  3,  69),   S(  1,  76),   S( 18,  44),   S(-10,  24),
            S( -7,   7),   S( 22,  -1),   S( 35,  10),   S( 58,  18),   S( 61,   1),   S( 62,  -2),   S( 10,  10),   S(-15,  50),
            S( 23, -11),   S( 20,  -5),   S( 50, -13),   S( 55, -12),   S( 55, -20),   S( 81, -48),   S(  7, -53),   S( 24, -17),
            S(-167,  18),  S(-26, -11),   S(-10,  -2),   S(  9,  -5),   S( 33, -19),   S(-15,  16),   S(  3,  -9),   S(-74, -51),

            /* knights: bucket 4 */
            S(  7,   0),   S(-38,   3),   S(  1,  -9),   S(-12, -15),   S(-15,  -7),   S(-23, -27),   S(-14, -40),   S(-13, -14),
            S( 37,  50),   S(-12,  22),   S( 54, -63),   S( 70, -41),   S( 71, -57),   S(-20, -52),   S(  6, -12),   S( -9, -35),
            S(-14,  16),   S( 33,  36),   S( 16, -13),   S( 31,  15),   S( 10,   9),   S(-39,  28),   S(-36, -63),   S(-16, -63),
            S(-17,  12),   S( 27, -23),   S( 67,   1),   S( 41,  -2),   S( -6,  16),   S(108, -29),   S( 22, -54),   S(-15, -51),
            S( 73,  44),   S(  2,  21),   S( 86,  20),   S( 71,  13),   S( 43,  36),   S( 18,   2),   S(-19, -56),   S( -8, -40),
            S( 13,  24),   S( -7, -15),   S( 92,  16),   S(  3, -17),   S( 33, -13),   S( 16,   5),   S(  0,  11),   S( -7, -24),
            S( -7,  16),   S(-22,   3),   S(-21, -20),   S( 37,  55),   S( 31,   0),   S(  6, -26),   S( 15,  -8),   S(-33, -52),
            S( -5,   9),   S( -6, -27),   S( -5,  -5),   S( -7, -20),   S( -6,  -5),   S( 11,   6),   S(  1,   0),   S( -7, -26),

            /* knights: bucket 5 */
            S( 10,  32),   S( -4,  31),   S( 59,  16),   S( 68,  30),   S( 29,  15),   S( -5,   8),   S( -3,  33),   S(-20, -17),
            S( 21,  -7),   S( 45,  46),   S( 11,  17),   S(  8,  23),   S( 49,  16),   S( 33,   7),   S( 37,  17),   S( -4, -21),
            S(  9,  42),   S( -6,  34),   S( 64,  14),   S( 81,   8),   S(-18,  29),   S( -1,  23),   S( 27,  -4),   S( 16,  -1),
            S( 44,  51),   S( 25,  33),   S( 46,  27),   S(  1,  42),   S( 47,  12),   S( 32,  30),   S( 37,  38),   S( 15,  37),
            S( 14,  53),   S( 42,  16),   S( 65,  30),   S( 55,  36),   S(117,  23),   S( 65,  32),   S( 47,  26),   S( 10,  42),
            S(  2,   6),   S( 11,  40),   S( 25,  22),   S( 38,  43),   S( 66,  33),   S( 43,  52),   S( 33,   8),   S(  0,  12),
            S( 25,  40),   S(-24,  55),   S( 25,  46),   S( 16,  38),   S( -8,  32),   S( 18,  31),   S( -6,  47),   S( -1,  -3),
            S(  5,  20),   S(  0,  14),   S( 13,  19),   S( -9,   4),   S( -3,  24),   S( -1,  11),   S(  6,  30),   S(-33, -32),

            /* knights: bucket 6 */
            S(-16, -40),   S(-41,  -5),   S( 60,  13),   S(-22,  29),   S(  4,  21),   S( 36,  26),   S( 19,   6),   S( -2,  73),
            S(-17, -16),   S( 58,   1),   S(  9,  13),   S(  3,  18),   S(-25,  42),   S( 62,  23),   S( 13,  23),   S(-31,  -1),
            S(-22,  -6),   S( 26,   0),   S( 38,   4),   S( 65,   3),   S( 38,  15),   S(-10,  38),   S( 12,  48),   S( 34,  49),
            S( 47,  -3),   S( 74,   0),   S( 71,  20),   S( 91,   2),   S( 32,  24),   S( 63,  28),   S( 37,  35),   S(-13,  78),
            S( 18,  31),   S( 51,   1),   S( 73,  20),   S( 44,  40),   S(104,  28),   S(122,  26),   S( 41,  35),   S( 30,  34),
            S( 30,  20),   S( 38,   0),   S( 84,  24),   S( 55,  34),   S( 74,  37),   S( 62,  30),   S( 38,  26),   S( 39,  37),
            S(-22,  27),   S(-17,  21),   S(-23,  26),   S(  7,  26),   S( 23,  37),   S(  7,  40),   S( 14,  73),   S(-20,  12),
            S(-40, -44),   S( 13,  58),   S( 40,  10),   S( -2,  32),   S(  9,  32),   S(-12,   3),   S( 20,  45),   S(  6,   6),

            /* knights: bucket 7 */
            S(-29, -27),   S(-192, -32),  S(-67, -46),   S(-50, -15),   S(-23, -11),   S(-19, -17),   S(-50,  17),   S(-26, -20),
            S( -5, -81),   S( -1, -67),   S(-21, -33),   S(-35,  -7),   S(-31,   3),   S( 31, -28),   S( 22,  34),   S( 29,  -5),
            S(-68, -64),   S(-49, -40),   S(-34,   4),   S( 39, -18),   S( 38, -19),   S( 47, -20),   S(-12,  45),   S( 84,  31),
            S(-52, -15),   S( 45, -32),   S( 20,   0),   S( 68, -11),   S( 98, -24),   S( 32,  -2),   S( 27,   7),   S(-23,  35),
            S(-79, -27),   S(-24, -37),   S( 95, -32),   S(113, -31),   S(147, -13),   S( 89,  12),   S( 92,  -3),   S( 96,   4),
            S( 11, -22),   S(-12, -15),   S(-21,  -9),   S( 81,  -9),   S( 96,   5),   S(125,  -8),   S( 57, -17),   S( 26,  24),
            S(-37, -28),   S(-58, -10),   S( 17, -30),   S( 43,   3),   S( 56,   9),   S( 81, -31),   S( -3,   8),   S(  5, -13),
            S(-67, -54),   S( -2,  -9),   S(-19, -21),   S( 19,  17),   S(  6, -18),   S( 32,  -5),   S( -9, -25),   S(-20, -21),

            /* knights: bucket 8 */
            S(  0,   7),   S(-12, -15),   S(  5,  23),   S( -6, -32),   S( -9, -37),   S(-13, -50),   S(-15, -11),   S( -2, -16),
            S(  1,   1),   S( -9, -17),   S( -3, -17),   S(-23, -32),   S(-21, -22),   S(-12, -59),   S( -5, -47),   S(-18, -39),
            S( 13,  39),   S(  1, -29),   S( 40,  38),   S( -2,  -2),   S( -7, -36),   S(-12,  19),   S(-11, -17),   S( -3, -27),
            S(-30, -13),   S( -1,  -5),   S( 10,  21),   S( 11,  40),   S( 19,  17),   S(  1,   4),   S(-22, -66),   S( -6, -23),
            S( 31,  79),   S(-10, -12),   S( 25,  46),   S( 25,  12),   S( -1,  23),   S(  4,  -3),   S(  5, -30),   S(-17, -28),
            S( 18,  44),   S( -3,  10),   S( 35,  24),   S( 26, -15),   S( -5,   8),   S(  2, -15),   S(  3,  -9),   S(-11, -23),
            S( -6,  11),   S(  2,  35),   S(  8, -13),   S(  8,  -2),   S(  5,   0),   S(  9,  21),   S(  3,  15),   S( -4, -24),
            S(  2,  -1),   S( 12,  20),   S( -3,  -6),   S(  1,  -1),   S( 14,  46),   S( -5, -32),   S(  0,  -1),   S(  1,  11),

            /* knights: bucket 9 */
            S(-12, -41),   S(-18, -16),   S(-16, -44),   S( -4, -26),   S(-27, -67),   S(-20, -31),   S(  1,   4),   S(  2,   4),
            S( -6, -24),   S(-14,   5),   S( -8, -30),   S(-12,  -6),   S(-10, -10),   S( -2,  -2),   S( -2,  18),   S(-15, -40),
            S(  8,  22),   S(  1, -10),   S(-21, -26),   S(  0,   4),   S( 13,  30),   S(-28,   9),   S(-21,   7),   S(-14, -13),
            S(  9,  28),   S(-21, -23),   S( 29,   4),   S( 30,  -1),   S( 29,  -8),   S( -5,  29),   S(-12, -64),   S( -6, -40),
            S( -6, -12),   S(  7,   5),   S(  9,  23),   S(  0,   8),   S( 17,  -2),   S(  4,  -8),   S(  6, -11),   S(  5,  -4),
            S(  3,  13),   S(  3,   7),   S(  5,  22),   S( 10,   2),   S( 20,  20),   S( 10,  21),   S( 14,  55),   S(  1,   4),
            S( -3,  -1),   S( -1,  -1),   S( 20,  58),   S(  0,  43),   S( 11,  65),   S( -5, -20),   S(  9,  34),   S( -5,  -3),
            S( -5,  -9),   S(  3,   4),   S(  7,  13),   S(  3,  17),   S( 12,  27),   S( 11,  33),   S(  5,  24),   S(  0,  -6),

            /* knights: bucket 10 */
            S(-20, -59),   S(-14, -35),   S(-11, -43),   S(-39, -58),   S(-11, -10),   S(-12, -37),   S(  1,  12),   S(  2,  14),
            S(-11, -37),   S(-10, -25),   S(  1,  -1),   S(-26, -46),   S(-16, -49),   S( -9, -40),   S( -9,  -8),   S( -4, -24),
            S(-22, -62),   S(-21, -73),   S(-16, -11),   S( -3, -22),   S( 10,  -7),   S(-11, -11),   S( -2, -49),   S(-11, -12),
            S(-10, -18),   S(-21, -70),   S( -1, -20),   S( 20,   1),   S( 26,   8),   S( 14,   7),   S( 10,  17),   S( 18,  37),
            S(-16, -75),   S(-27, -47),   S( -1,  -1),   S( 27,   0),   S( 27,  10),   S(  4,  -7),   S( 11,  19),   S( 28,  73),
            S(-11, -16),   S(  0,  -4),   S(-19,   1),   S( -3,  17),   S( 27,  44),   S( 10,  35),   S( 22,  32),   S( 16,  51),
            S(  3,   5),   S(-18, -44),   S(  7,  -4),   S(  9,  35),   S( 12,  37),   S(  8,  11),   S( -3,  -8),   S( 16,  52),
            S( -6, -17),   S(  6,  17),   S( -5, -17),   S(  1, -14),   S( 10,  51),   S(  4,  29),   S(  8,  18),   S( -3, -10),

            /* knights: bucket 11 */
            S(  2,  19),   S(-20, -30),   S(-17, -69),   S(-12, -30),   S(-21, -44),   S( -2,   1),   S(-10, -28),   S( -6,  -3),
            S(-23, -46),   S(-18, -41),   S(-26, -86),   S(-20, -30),   S(-15, -24),   S(-23, -21),   S(-11, -25),   S( -9,  -1),
            S(-10, -56),   S(-23, -39),   S(-16, -16),   S(  9,   3),   S(-11,  -4),   S(-13,  17),   S(  8,  -5),   S( -2,  -8),
            S(-17, -28),   S( -8, -37),   S( -3,  15),   S( 29,  39),   S( 37,  10),   S( 19,  26),   S( -6,  12),   S(  3,  18),
            S( -4, -19),   S(-29, -72),   S( 17, -20),   S( 14,  14),   S( 19,   9),   S( 55,  27),   S(  3,  15),   S( 32,  70),
            S( -4,   3),   S( -3, -16),   S( -5,   4),   S( 34,  25),   S(  5,  12),   S( 57,  36),   S(  7,  14),   S(-11,  -7),
            S( 11,  61),   S( -6, -19),   S( -3, -23),   S(  5,   9),   S( 19,  30),   S( -4,  12),   S( 12,  45),   S( 22,  80),
            S( -7, -12),   S( -5, -25),   S(  9,  17),   S(  4,  17),   S( -7,   4),   S(  6,  12),   S( 12,  27),   S(  3,  29),

            /* knights: bucket 12 */
            S(  0,  -2),   S( -1,   4),   S( -4, -22),   S( -2,   6),   S( -1,  -4),   S( -2,   1),   S(  4,   6),   S( -1, -12),
            S( -1,  -3),   S(  3,   3),   S(  2,   8),   S( -7, -29),   S( -3,  10),   S(-11, -45),   S( -2, -17),   S(  8,  22),
            S( -6, -21),   S(  1,  -4),   S( -1, -10),   S( -2,   9),   S(  5,  18),   S(  1, -14),   S(  1,   2),   S(-10, -28),
            S( -9, -12),   S( -1, -17),   S(  6,   0),   S(  3,  49),   S(  3,  16),   S(  1,  -2),   S( -4,  -9),   S( -1,  -9),
            S(  4,  17),   S( 12,  17),   S(  1,   4),   S( 11,  29),   S( -6, -27),   S( 10,  18),   S( -6, -17),   S(  3,   8),
            S(  1,  12),   S( -2, -13),   S(  3,  54),   S( -6, -29),   S(  7,   8),   S( -1, -13),   S(  0, -10),   S( -1,  -1),
            S(  4,  20),   S(-13, -10),   S(  3,   5),   S(  6, -10),   S( -6, -24),   S( -9, -30),   S( -4, -13),   S( -1,  -5),
            S(  8,  27),   S(  4,  28),   S(  1,   2),   S(  5,  14),   S(  0,  -1),   S( -1,  -6),   S( -2,  -6),   S(  2,   9),

            /* knights: bucket 13 */
            S( -3,  -9),   S( -2,  -8),   S( -2,  -9),   S( -4, -14),   S( -7, -18),   S( -3, -17),   S( -2, -11),   S(  0,   4),
            S( -2,  -3),   S(  0,   5),   S( -2, -19),   S( -4,  -4),   S( -7, -39),   S( -2, -21),   S( -4, -15),   S(  3,  -4),
            S( -4, -23),   S(-10, -14),   S( 10,  21),   S(  1,  -7),   S( -6,  -4),   S( -5, -14),   S(  1,   4),   S(-12, -35),
            S( -7,  -2),   S(  6,  30),   S(-15, -16),   S(  0,   5),   S( -2,  -7),   S(  9,  14),   S(  1, -15),   S( -5,  -8),
            S(  4,  -1),   S(  4,  25),   S( 24,  27),   S( 13,  41),   S(  4,  27),   S( -6,   1),   S( -5, -21),   S( -1,   3),
            S( -2,   9),   S( 13,  27),   S( 15,  77),   S(-11,  14),   S(  1,  25),   S( -2, -18),   S(  6,  17),   S( -6, -14),
            S(  4,  15),   S(  4,   9),   S( -3, -11),   S( -2,  35),   S( 19,  67),   S(  0,   6),   S( -3,  12),   S( -5,  -5),
            S(  1,   6),   S( -4,  33),   S(  4,  19),   S(  1,  26),   S(  3,  26),   S(  2,   9),   S(  0,  -3),   S(  1,   4),

            /* knights: bucket 14 */
            S( -6, -35),   S( -7, -35),   S(  4,  17),   S(  0,   7),   S(-10, -43),   S(  2,   5),   S(  0,  -1),   S( -1,  -5),
            S( -2, -22),   S( -2, -16),   S(-17, -56),   S(  0, -20),   S( -4, -19),   S(  0,  -4),   S(  1,   2),   S(  0,  -2),
            S(-10, -22),   S(  4, -11),   S( -7, -27),   S(  0,   6),   S( -5, -25),   S( -1, -19),   S(  8,  24),   S(  0,  -2),
            S(  1,   5),   S( -7, -37),   S( -8, -28),   S( -3,  -6),   S( -2, -22),   S(  7,   7),   S(  6,  21),   S( -3,  -7),
            S( -2,  -8),   S( -1, -11),   S( 20,  57),   S(  1,  -1),   S( -8, -23),   S( 10,   9),   S(  2,   5),   S( -3, -14),
            S( -2,  -5),   S(  6,   4),   S( -9, -17),   S(  7,  22),   S( 10,  38),   S( -4,   9),   S(  0,  15),   S(  5,  34),
            S(  1,   6),   S( -4, -21),   S(  7,  14),   S(  0,  13),   S( -6,  27),   S( -6,  16),   S(-11,   9),   S(  3,  26),
            S(  0,   0),   S(  2,  10),   S( -4,  -7),   S(  9,  35),   S(  2,  24),   S(  6,  24),   S(  5,  24),   S(  0,   3),

            /* knights: bucket 15 */
            S( -4, -16),   S( -2, -31),   S(  4,  -3),   S( -7, -23),   S(  1,  -1),   S(  4,   1),   S(  0,  -1),   S(  1,   6),
            S( -3, -12),   S(  1,  -7),   S( -1,   5),   S(  3,   7),   S(  0, -20),   S( -1,   5),   S(  1,   3),   S( -1,  -4),
            S( -1,  -8),   S( -4,  -9),   S( -2,  -8),   S(-12, -42),   S(  3,  -1),   S(  1,   5),   S( -5, -15),   S( -3, -10),
            S(-12, -35),   S( -5, -21),   S( -9, -20),   S( -2,   3),   S(  6, -11),   S( 18,  38),   S(-10, -28),   S( -7, -18),
            S( -2,  -1),   S( -2,  -8),   S( -3, -27),   S( -8, -44),   S( -1,   0),   S(  8,   5),   S(-13, -30),   S( -2,   6),
            S(  2,   9),   S( -1,  -8),   S(  0, -14),   S( -6,  -6),   S( -2, -11),   S(-14, -10),   S( -6, -15),   S( -7, -12),
            S( -3,  -8),   S( -3,   3),   S(  0,   3),   S(  0,   3),   S( -9, -14),   S(  6,  35),   S( -6, -16),   S(  3,  27),
            S(  0,   2),   S(  0,   4),   S( -4, -11),   S(  0,  -6),   S( -3, -10),   S( -9, -18),   S(  6,  29),   S(  0,   3),

            /* bishops: bucket 0 */
            S( 36,  53),   S(  0,   5),   S( 54,  18),   S(  3,  13),   S(  1,  -4),   S(  7, -23),   S(  6, -16),   S(  0, -29),
            S( 54, -38),   S( 86,  20),   S( 44,  -3),   S( 16,  17),   S( -3,  38),   S( -2,  32),   S(-30,   7),   S( 12, -21),
            S( 14,  30),   S( 31,  28),   S( 30,  34),   S( -1,  58),   S( 12,  38),   S(-21,  53),   S(  4,   6),   S(  4, -41),
            S(  9,  15),   S( 72,   7),   S( 38,   8),   S( 40,  30),   S( -6,  55),   S( 28,  25),   S(  5,  23),   S( 13,   4),
            S(  5,  16),   S( 38,   5),   S( -1,  47),   S( 44,  23),   S( 32,  47),   S(-17,  36),   S( 19,   1),   S(-79,  28),
            S(-28,  90),   S(-17,  69),   S( 49,  17),   S( 22,  56),   S( 15,  55),   S(-15,  24),   S(-39,  50),   S(-10,  44),
            S(-48,  70),   S(-11,  25),   S( 36,  30),   S(-13,  65),   S(-81,  46),   S( 57,  33),   S( 46,  -4),   S(-41, -18),
            S(-33, -11),   S( -5,  53),   S(-35,  33),   S(-19,  24),   S( -4,  40),   S( 29,  54),   S(-29,  45),   S(-32,  16),

            /* bishops: bucket 1 */
            S( 10,  89),   S( -5,  41),   S( 29,  30),   S(  4,  25),   S( 29,  13),   S(  0,  40),   S( 15,  23),   S(-34,  27),
            S( 16,   3),   S( 42,  20),   S( 42,  17),   S( 34,  31),   S(  2,  21),   S( 23,  11),   S(-19,  24),   S( 20,  19),
            S( 48,   6),   S(  7,  38),   S( 49,  33),   S(  6,  44),   S( 13,  48),   S(-12,  41),   S( 27,  28),   S( 18, -25),
            S( 48,   3),   S( -3,  43),   S(  1,  45),   S( 25,  45),   S( -9,  49),   S( 22,  34),   S( -5,  45),   S( 41,  -2),
            S( 11,  57),   S( 11,  36),   S( 15,  34),   S(-10,  45),   S( 13,  35),   S(-37,  49),   S( 28,   7),   S(-33,  37),
            S(-24,  52),   S( 24,  58),   S(-15,  59),   S( 41,  20),   S(  9,  49),   S( 47,  36),   S( -6,  47),   S( 34,  17),
            S(  0,  66),   S(  8,  45),   S(  7,  48),   S( 61,  44),   S( 40,  49),   S(-37,  51),   S(-13,  74),   S(-39,  51),
            S( 16,  62),   S(-30,  41),   S(-41,  31),   S(  2,  41),   S( 10,  53),   S(-12,  52),   S( -5,  62),   S(-51,  92),

            /* bishops: bucket 2 */
            S( 27,  26),   S(  4,  40),   S( -2,  48),   S(-17,  45),   S(-26,  42),   S(-12,  36),   S(-43,  11),   S(-64,  96),
            S(-13,  42),   S( 15,  33),   S( 26,  28),   S(  3,  43),   S(  1,  42),   S(  9,  24),   S( 11,   8),   S(  0,  -8),
            S( -3,  19),   S(  3,  46),   S(  7,  58),   S( -7,  72),   S( -6,  53),   S(  6,  51),   S( -4,  50),   S(-22,  21),
            S( 12,  38),   S(-27,  59),   S(-13,  60),   S(  0,  67),   S(  7,  52),   S(  0,  45),   S( 21,  40),   S(  3,   9),
            S(-19,  34),   S(-10,  52),   S(-29,  51),   S(-26,  62),   S(-19,  73),   S( -1,  54),   S(  1,  38),   S( -9,  15),
            S(  7,  45),   S(-21,  42),   S( -5,  61),   S(-30,  55),   S( 26,  35),   S(-12,  68),   S( 20,  49),   S( 10,  28),
            S(-31,  53),   S(-19,  50),   S(-26,  65),   S(-19,  42),   S(-45,  70),   S(-61,  51),   S(-79,  60),   S(-24,  35),
            S(-126, 107),  S(-77,  84),   S(-68,  63),   S(-72,  68),   S(-68,  58),   S(-96,  62),   S( -3,  49),   S(-69,  84),

            /* bishops: bucket 3 */
            S( -1,  36),   S( 20,  24),   S( 11,  31),   S( -6,  38),   S(  2,  27),   S( 42,   0),   S( 15,   2),   S( 46, -54),
            S(  9,  22),   S(  6,  36),   S( 28,  13),   S(  4,  50),   S( 14,  32),   S(  5,  38),   S( 48,  28),   S( 24,   5),
            S( 15,  18),   S(  4,  49),   S(  6,  62),   S( 13,  51),   S(  1,  70),   S( 13,  58),   S( 23,  36),   S( 37,   6),
            S( 25,  14),   S( -2,  53),   S(  1,  64),   S(  9,  72),   S( 17,  59),   S( 18,  39),   S( 18,  42),   S( 11,   7),
            S(-11,  37),   S( 21,  35),   S( 12,  37),   S( 20,  60),   S( 14,  52),   S( 33,  44),   S(  8,  42),   S( 11,  42),
            S( 14,  32),   S(  7,  46),   S( 25,  52),   S( 20,  34),   S(  4,  56),   S( 43,  45),   S( 13,  49),   S(  0,  66),
            S(  2,  31),   S(-15,  58),   S( 25,  30),   S( 21,  40),   S( -3,  50),   S( 11,  42),   S(-19,  79),   S( -3,  71),
            S(-27,  99),   S(-30,  58),   S( 13,  44),   S(-37,  66),   S(-23,  65),   S(-47,  79),   S( -8,  53),   S( 73,  14),

            /* bishops: bucket 4 */
            S(-16, -16),   S(-47,  25),   S(-29,  -4),   S(-41,  36),   S(-13,  25),   S(-50,  28),   S(-24,  12),   S( 14,  20),
            S(-18,  14),   S( 23,  23),   S(-38,  58),   S( -8,  17),   S(-18,  10),   S( 59,   2),   S( -9,  17),   S( 17,  -4),
            S(-10,  37),   S(-30,  46),   S( 36,   6),   S( -6,  15),   S( 53,   9),   S( 35,  16),   S(-16, -26),   S(-57,  26),
            S(-26,   4),   S(-13,   9),   S( 57,   5),   S( 34,  16),   S(-11,  41),   S( 47, -12),   S( 32,  12),   S(  3,   5),
            S( 14,  30),   S(  8,  31),   S(-29,  52),   S( 33,  32),   S(-28,  18),   S(-33,   1),   S(-29,  12),   S(  3,  29),
            S( -5,  10),   S( 23,  15),   S(  6,  23),   S( 21,  21),   S( 40,  30),   S( -3,  18),   S( -5, -20),   S(-15,   1),
            S(-30,  21),   S( 39,  41),   S( -1,  35),   S( 11,  52),   S( -4,  -8),   S( -9,  26),   S(  4, -13),   S(  1,   6),
            S(  8,  57),   S(-14, -12),   S(  2,  33),   S(-41, -13),   S( -2,  40),   S( -6,  17),   S( -5,  -3),   S( -6,  36),

            /* bishops: bucket 5 */
            S( -3,  61),   S( -2,  58),   S(-27,  36),   S( -5,  14),   S(-20,  26),   S(-34,  32),   S( -3,  43),   S( -1,  40),
            S(-29,  71),   S(-50,  62),   S(-35,  66),   S(  3,  28),   S( -5,  39),   S(  4,  39),   S(-49,  45),   S(-33,  52),
            S(-50,  70),   S(-23,  56),   S( 23,  35),   S(  9,  29),   S( 17,  27),   S(-16,  32),   S(-15,  48),   S( 12,   9),
            S( -4,  38),   S( 46,  42),   S(-20,  56),   S( 45,  26),   S( 31,  36),   S( 26,  33),   S( -3,  26),   S( -5,  34),
            S( 79,  43),   S( 35,  23),   S( 78,  17),   S( 75,  32),   S( 64,  21),   S( 53,  17),   S( 39,  35),   S(-35,  29),
            S( 29,  52),   S( 64,  27),   S( 88,  31),   S( 15,  44),   S(-11,  51),   S( 33,   8),   S(  9,  28),   S(  1,  57),
            S( -1,  41),   S(-26,  43),   S(-26,  36),   S( -1,  50),   S(-18,  49),   S( 31,  60),   S(-20,  44),   S(  2,  18),
            S( -3,  41),   S( -1,  26),   S( 14,  29),   S( 11,  26),   S(  6,  29),   S( -4,  42),   S( 14,  80),   S( -5,  14),

            /* bishops: bucket 6 */
            S(-38,  77),   S(-25,  78),   S(-10,  38),   S(-13,  22),   S(-49,  42),   S(-59,  55),   S( 11,  67),   S(-25,  42),
            S( 21,  45),   S( -4,  39),   S( -8,  48),   S( -9,  54),   S( 20,  24),   S( -2,  27),   S(-131,  80),  S( 11,  36),
            S( 24,  23),   S( -1,  33),   S( 48,  38),   S( 37,  30),   S( 54,  12),   S( 73,  17),   S(-16,  65),   S(-97,  64),
            S( 40,  34),   S(-19,  56),   S(  5,  48),   S( 36,  34),   S( 15,  46),   S(  9,  37),   S( 15,  54),   S( -5,  19),
            S(-44,  56),   S( 27,  39),   S( 61,  15),   S( 15,  44),   S(106,  30),   S( 69,  25),   S( 45,  39),   S(-23,  41),
            S(-15,  39),   S( 24,  26),   S( 51,  39),   S( 27,  52),   S( 19,  51),   S( 63,  33),   S( 14,  43),   S(-14,  54),
            S(-20,  41),   S(-25,  26),   S( -9,  39),   S( -2,  49),   S( 22,  43),   S(-44,  60),   S( 11,  47),   S(-63,  54),
            S(  1,  68),   S( 12,  56),   S(  2,  39),   S(  3,  48),   S( 10,  30),   S( 20,  47),   S(  2,  34),   S(  6,  61),

            /* bishops: bucket 7 */
            S(-28,  39),   S(-10,  25),   S(-44,  -1),   S(-52,  14),   S(-18,   1),   S(-33,  -9),   S(-65, -37),   S(-55,   1),
            S(-59,  27),   S(-72,  26),   S(  1,  -6),   S( -1,  12),   S(-22,  19),   S(-75,  37),   S(-21,  -6),   S(-62,  -9),
            S(-45,  20),   S(  0,   5),   S(-19,  29),   S( 36,  15),   S(-31,  26),   S(-24,  17),   S(-24,  15),   S(-25,  22),
            S(-26,  11),   S( 39,  18),   S( 89, -10),   S( 53,  14),   S( 92,   4),   S(  2,  17),   S( 32,  32),   S(-25,  32),
            S(-33,  17),   S(-23,  14),   S( 21,   0),   S( 83,  -3),   S( 51,  22),   S( 57,  20),   S(-17,  43),   S( 43,  -3),
            S(-23,   8),   S(-32,   1),   S(  4,   4),   S( 19,  -2),   S( 41,   7),   S( 49,  22),   S( 49,  14),   S(-21,  27),
            S(  1, -19),   S(-30,  13),   S( 10,  11),   S(-29,  19),   S( -3,  12),   S( 13,   7),   S( 43,  22),   S(-16,  68),
            S(-11,  16),   S(-41,  22),   S(-43,  28),   S(-19,  16),   S( 14,  10),   S(  7,  14),   S( 19,  39),   S( 24,  29),

            /* bishops: bucket 8 */
            S( -3, -54),   S(-10, -50),   S(-27, -23),   S( -3, -43),   S(  8, -15),   S( -8,  14),   S( 13,  15),   S(  4,  18),
            S( -9, -28),   S(-13, -90),   S( -8, -33),   S( -7, -19),   S( 18,   4),   S( -7, -29),   S(-16, -46),   S(  6, -28),
            S( 11,   3),   S(-44, -41),   S( -8,   3),   S( 11, -32),   S(  5, -19),   S( 12, -28),   S( 21, -37),   S(-22, -47),
            S(  3,  41),   S(  1, -27),   S( -5,   9),   S(  3, -27),   S( 19,  -9),   S( 14, -28),   S(  2, -26),   S(  4, -17),
            S( 10,  30),   S( 28,  31),   S( 12,   4),   S( 57,   0),   S( 31, -35),   S(  6,  -8),   S( 11, -14),   S( -2, -32),
            S( -8,   6),   S(-14, -12),   S(  3,  16),   S( -2, -19),   S( 20,   2),   S(  4, -30),   S( -2, -65),   S(-19, -52),
            S(  5,   5),   S( 28,  21),   S(  6, -26),   S(  8, -16),   S( -4, -32),   S(  2, -31),   S(-14, -67),   S(-16, -21),
            S( -7, -21),   S( -4, -71),   S(  6,   8),   S( -7, -22),   S(-19, -61),   S( -3, -25),   S( -5, -27),   S( -2, -19),

            /* bishops: bucket 9 */
            S(-23, -56),   S(  4, -63),   S(-32, -19),   S( -3, -46),   S(-18, -68),   S(  0, -49),   S( -7, -10),   S( 22,  55),
            S(  4, -25),   S(-18, -38),   S(-11, -32),   S(  9, -26),   S(  7,  -8),   S( -1,  -7),   S( -9, -22),   S( 15,  32),
            S( 18,  27),   S(  7,  -4),   S( -6, -34),   S( 29, -22),   S( 46, -41),   S( 29, -21),   S(-24, -30),   S( 13,   9),
            S(-12, -11),   S( 14,  10),   S(  0,  -5),   S( 59, -28),   S( 47,  -8),   S( 18,  -2),   S(  9, -37),   S( -7, -36),
            S(  3,  -7),   S( 24,   4),   S( 22,  -8),   S( 39,   4),   S( 13, -46),   S( 28, -27),   S( 20,  -9),   S( -1,  -1),
            S(-10, -41),   S( 36,   4),   S(  5,  10),   S( 24,  -6),   S( 41, -14),   S( 27, -31),   S( 11, -40),   S( -9, -21),
            S(  6,   4),   S( 27,   0),   S( -6, -19),   S( 17, -26),   S( 25, -15),   S( -4, -55),   S( -5, -32),   S(  1, -25),
            S(  1,  -9),   S( -2,  -3),   S(  2, -36),   S( -7, -27),   S(-12, -57),   S(  8,   1),   S(  4, -20),   S( -4, -17),

            /* bishops: bucket 10 */
            S(  1,  17),   S(  7,  -6),   S(-36, -28),   S( -5, -30),   S(-16, -30),   S(-22, -53),   S(-12, -21),   S( -6, -30),
            S( 19,  19),   S(-10, -19),   S( 21, -32),   S( 12, -45),   S(-13, -40),   S( 13, -24),   S(-22, -76),   S( -5, -24),
            S( -2, -49),   S( 11, -41),   S(-10, -57),   S( 39, -37),   S( 52, -53),   S( -2, -15),   S(-30, -19),   S(  8,  -7),
            S(-16, -33),   S( 17, -15),   S( 23, -31),   S( 42, -25),   S( 72, -41),   S(  0,  -5),   S(  2,   8),   S( 11,   4),
            S( -8, -37),   S( 27, -34),   S( 52, -58),   S( 93, -35),   S( 40,  -4),   S( 50, -13),   S( 24,   7),   S( -5, -40),
            S(-12, -49),   S(  9, -67),   S(  2, -57),   S( 17, -33),   S( 47,  -5),   S( 42,  -1),   S( 28,   0),   S( -9, -41),
            S(-16, -86),   S(  7, -42),   S( -4, -61),   S( 31, -49),   S( -1, -33),   S(  8, -13),   S( 25,  52),   S( 14,   4),
            S(  1, -15),   S( -5, -30),   S(  5,   7),   S( -9, -39),   S(  2, -30),   S( -2, -43),   S(  1, -13),   S(  6,  19),

            /* bishops: bucket 11 */
            S( -3,  49),   S(-51,   7),   S(-58, -54),   S(-11, -28),   S(-12, -14),   S(-59, -82),   S(-16, -55),   S(-17, -38),
            S( 12,  11),   S(  3, -37),   S(-12, -15),   S(-14, -15),   S( -9, -31),   S(-32, -71),   S(-20, -67),   S(-35, -47),
            S( -8, -70),   S(  6, -56),   S( 14, -27),   S( 36, -57),   S( 28, -13),   S(  8, -28),   S( -4,  -8),   S(  3, -13),
            S( -7, -37),   S(-10, -37),   S( 23, -54),   S( 32, -47),   S( 59, -48),   S( 23, -11),   S( 23,   6),   S(  8,  23),
            S( -8, -45),   S( -1, -50),   S( 20, -43),   S( 58, -38),   S( 54, -38),   S( 50, -10),   S( -1,  24),   S( 25,  11),
            S(-15, -76),   S(  5, -69),   S( 14, -50),   S( 32,   8),   S( 36, -34),   S( 22,  -8),   S( -2,   1),   S( -8, -16),
            S(-10, -48),   S(  6, -48),   S(  3, -38),   S( 12, -52),   S(  2, -46),   S( 22, -24),   S(  5, -14),   S(  7,   5),
            S(-15, -65),   S(-18, -54),   S( -2, -52),   S( 20, -20),   S(  7, -33),   S(-23, -86),   S( -4, -44),   S(  2,  -4),

            /* bishops: bucket 12 */
            S(  5,  11),   S( -8, -28),   S(-24, -84),   S( -1,  -5),   S( -6, -30),   S( -8, -11),   S(  3,   8),   S(  5,  32),
            S( -4, -25),   S( -3, -29),   S(  3, -17),   S( -2, -12),   S( -8, -44),   S( -4,   5),   S(  3,  -3),   S( -5,  -3),
            S(  0, -17),   S( -8, -23),   S( -5, -11),   S( -2, -41),   S( -9,  -9),   S(  0, -33),   S(-20, -53),   S(  1,   2),
            S( -6, -26),   S( -5, -30),   S( -6, -41),   S(  1,  -5),   S(  9, -18),   S(  1,  13),   S( -1, -14),   S(-11, -32),
            S(  2,   3),   S(  3, -12),   S( 11, -21),   S( -7, -21),   S(  2, -22),   S(  1,  -5),   S(  3, -16),   S(  1,  -4),
            S(-16, -28),   S( -5,  24),   S( -1,  19),   S( -5, -10),   S(  4, -20),   S( -8, -27),   S(  5, -17),   S( -2, -11),
            S( -6,  -4),   S( -9,   8),   S(  0,   1),   S(-11, -13),   S( -6, -24),   S(  6,   1),   S( -6, -27),   S( -1,  -8),
            S( -1, -12),   S( -1,  16),   S( -2, -35),   S( -1, -16),   S(  7,   6),   S( -5, -44),   S(-12, -56),   S( -1,  -1),

            /* bishops: bucket 13 */
            S(-10, -53),   S( -6, -66),   S( -9, -48),   S(  9,  -2),   S( -8, -42),   S( -9, -14),   S( -9, -21),   S(  1,   5),
            S(  4,  -1),   S(  1, -38),   S( -6, -79),   S(-13, -72),   S(-13, -39),   S(-11, -36),   S( -1, -23),   S( -1, -32),
            S( -1, -18),   S( -1, -18),   S(  6, -25),   S( -2, -71),   S( 16, -48),   S(  8, -33),   S( -9, -35),   S(  5,  23),
            S(  5,  19),   S(  0, -19),   S(-11, -56),   S( -5, -63),   S( 25, -30),   S(  3,   3),   S( -1, -14),   S(-17, -80),
            S(  3,  16),   S(-14, -16),   S( -5, -51),   S( 12, -16),   S( 15,   7),   S( 15, -18),   S( -1, -53),   S( -1, -12),
            S( -9, -33),   S( -1,  -1),   S( -6, -31),   S(  0, -30),   S(  1,   2),   S( 12,  -2),   S( 12, -27),   S( -5, -23),
            S(-10, -34),   S( -4, -29),   S( -4, -15),   S(-16, -14),   S( -2, -32),   S(  6,  -3),   S( -7, -49),   S(  3,  -5),
            S( -8, -42),   S( -5, -37),   S( -6, -29),   S( -5, -27),   S( -8, -21),   S( -4, -26),   S(  5,  19),   S( -1, -24),

            /* bishops: bucket 14 */
            S(  8,  -2),   S( -7, -32),   S(-16, -48),   S( -5, -15),   S(-13, -72),   S(  5, -17),   S(-10, -71),   S( -8, -38),
            S( -1,  17),   S( -2, -26),   S(  1, -30),   S(-25, -72),   S( -5, -48),   S(-15, -93),   S( -8, -37),   S( -2, -26),
            S( -8, -27),   S(-11, -42),   S( -7, -53),   S( -7, -77),   S( -4, -68),   S( -9, -70),   S( -8, -31),   S( -1,  -8),
            S( -3, -21),   S(  3, -17),   S(  0, -18),   S( -4, -39),   S(  6, -74),   S(  2, -50),   S( -4, -39),   S(-11, -29),
            S( -8, -57),   S( -4, -34),   S( 16, -30),   S(  7, -20),   S( -3, -59),   S(  2, -48),   S(  2, -21),   S(  0, -13),
            S(  1,  -9),   S(  1, -27),   S( -7, -38),   S(-14, -67),   S( -5, -16),   S(  9,  33),   S( -3, -39),   S(-10, -31),
            S( -5, -38),   S(  4, -36),   S(-11, -39),   S( 12,  14),   S(-10, -39),   S( -3, -30),   S(  0, -19),   S( -4, -23),
            S(  0,  -3),   S( -7, -34),   S( -5, -31),   S(  0, -11),   S(  0, -14),   S( -6, -10),   S( -6,  -2),   S( -4, -21),

            /* bishops: bucket 15 */
            S( 20,  76),   S( -1,  17),   S(-18, -64),   S(  4, -13),   S( -4, -23),   S(-16, -24),   S( -6, -29),   S( -5, -17),
            S(  4,  13),   S(  1, -12),   S( 15,  38),   S( -7, -22),   S(-10, -36),   S( -9, -44),   S( -4, -21),   S( -5, -22),
            S( -8, -33),   S( -6, -26),   S( -2, -38),   S(  0, -19),   S( -7, -47),   S( -8, -22),   S( -7, -27),   S(  1,  -2),
            S( -2,  -5),   S(-12, -57),   S(  5,  -5),   S(-19, -73),   S(  6, -12),   S( -5, -38),   S(  1,  12),   S( -1,  -4),
            S(  4, -13),   S(-11, -41),   S(  3, -36),   S(-14, -32),   S( -4, -12),   S(-12, -19),   S(  6,  -5),   S( -3, -14),
            S( -9, -26),   S(-13, -65),   S(-11, -34),   S(-13, -67),   S( -6, -40),   S( -4,  -3),   S( 29,  45),   S( -2,   0),
            S(  0, -25),   S( -2, -19),   S( -1, -26),   S( -5, -32),   S(-10, -38),   S( -1, -12),   S( -1, -16),   S(  3,  17),
            S( -5, -29),   S( -3, -11),   S( -7, -29),   S( -8, -54),   S( -5, -46),   S(-13, -56),   S(-10, -19),   S(  0,  -1),

            /* rooks: bucket 0 */
            S(-13, -30),   S(  9,  -5),   S( 14, -31),   S( 21, -14),   S( 26,  -4),   S( 19, -12),   S( 12,   3),   S( 17,   2),
            S(  7, -60),   S( 46, -36),   S( 36, -26),   S( 11,  -3),   S( 29,   2),   S( 14,  -7),   S(-13,   3),   S(-43,  39),
            S(  5,  -9),   S( 27,  21),   S( 44,   2),   S( 24,  14),   S( 11,  16),   S(  2,   6),   S(-20,  20),   S(-32,   8),
            S( 35, -24),   S( 51,   7),   S( 74,  -3),   S( 54,  15),   S( 35,  -5),   S( 34,   2),   S(-15,  32),   S(-20,  30),
            S( 49, -20),   S( 90, -25),   S( 86, -22),   S( 57,   1),   S( 51,   6),   S( 44,   7),   S(  6,  12),   S( -6,  33),
            S( 81, -45),   S(108, -34),   S( 65,   5),   S( 51,   3),   S( 51,   9),   S(-21,  29),   S( 68,  -5),   S(-31,  41),
            S( 45,  14),   S(107,  -4),   S( 53,  22),   S( 49,   3),   S(  9,  20),   S( 55,   3),   S(  8,  29),   S(  0,  34),
            S(  6,  21),   S(  3,  51),   S( 32,  44),   S( 56,   6),   S( 51,   9),   S( 15,  -1),   S( 48,  15),   S( 63,  -9),

            /* rooks: bucket 1 */
            S(-82,  38),   S(-30,   7),   S(-63,  11),   S(-33, -10),   S( -8, -15),   S(-15,  -7),   S(-23,  -7),   S(-31,   9),
            S(-52,   1),   S(-84,  40),   S(-22,   2),   S(-15, -37),   S(-39,   8),   S(-42,   4),   S(-35, -21),   S(-57,  -3),
            S(  5,   4),   S(-41,  42),   S(-31,  22),   S(-40,  28),   S(-41,  28),   S( -9,   6),   S(-18,  19),   S(-42,  21),
            S(-49,  51),   S(-46,  40),   S(  3,  38),   S(-15,  35),   S(-29,  50),   S(-62,  55),   S(-23,  34),   S(-48,  29),
            S( 71,   4),   S( 53,  16),   S( 50,  11),   S(-21,  50),   S(-38,  62),   S( 43,   6),   S(  9,  21),   S(-46,  25),
            S( 60,  12),   S( -3,  44),   S( 20,  32),   S(-25,  37),   S( 41,  16),   S(-18,  30),   S( -3,  28),   S(-18,  34),
            S(-11,  38),   S( 29,  42),   S( 26,  41),   S(-72,  91),   S(-23,  44),   S(-24,  50),   S(-46,  38),   S(-30,  32),
            S( 74,   3),   S( 25,  42),   S( 30,  29),   S(-62,  74),   S(  3,  33),   S( 61,   9),   S( 16,  24),   S( 43, -12),

            /* rooks: bucket 2 */
            S(-68,  44),   S(-43,  28),   S(-41,  31),   S(-49,  28),   S(-54,  20),   S(-50,  17),   S(-38,   0),   S(-57,  32),
            S(-73,  37),   S(-71,  49),   S(-36,  35),   S(-52,  25),   S(-48,  18),   S(-63,  21),   S(-87,  23),   S(-56,  18),
            S(-76,  62),   S(-49,  61),   S(-58,  74),   S(-39,  31),   S(-57,  51),   S(-59,  53),   S(-14,  14),   S(-33,  21),
            S(-84,  73),   S(-62,  68),   S(-35,  75),   S(-37,  68),   S(-24,  57),   S( -5,  49),   S(-43,  64),   S(-10,  28),
            S(-19,  55),   S(-41,  77),   S(-48,  70),   S(-10,  50),   S( 47,  35),   S( -3,  58),   S(-20,  54),   S(-17,  45),
            S(-21,  44),   S( -6,  54),   S( 25,  45),   S(  5,  38),   S( 34,  38),   S( 27,  22),   S( 30,  21),   S(-28,  49),
            S(-53,  66),   S(-70,  91),   S(-11,  69),   S(  6,  61),   S( 23,  50),   S( 46,  35),   S(-39,  72),   S( -5,  58),
            S(-31,  75),   S( -3,  48),   S(-21,  60),   S( -3,  59),   S(-31,  62),   S(-11,  59),   S( -8,  56),   S( 33,  18),

            /* rooks: bucket 3 */
            S(  0,  69),   S(  3,  70),   S( 12,  63),   S( 22,  56),   S( 13,  60),   S( -8,  62),   S(  4,  68),   S(-13,  42),
            S(-34,  87),   S(-12,  66),   S( 14,  56),   S( 11,  59),   S( 22,  54),   S(  9,  57),   S( 31,  18),   S( 10, -21),
            S(-34,  78),   S(-16,  89),   S(  8,  77),   S( 15,  62),   S( 11,  77),   S( 18,  67),   S( 26,  57),   S(  0,  61),
            S(-29,  95),   S(-23,  95),   S( 16,  75),   S( 41,  69),   S( 33,  77),   S(  7,  99),   S( 51,  70),   S( -1,  83),
            S( -4,  95),   S( 24,  80),   S( 19,  74),   S( 46,  75),   S( 52,  72),   S( 54,  62),   S( 90,  50),   S( 50,  55),
            S( -1,  90),   S( 20,  86),   S( 26,  80),   S( 43,  68),   S( 42,  53),   S( 50,  63),   S( 95,  30),   S( 82,  33),
            S(-14, 102),   S(  1, 101),   S(  9, 104),   S( 50,  77),   S( 27,  80),   S( 65,  68),   S( 51,  75),   S(134,  14),
            S(-56, 148),   S( 33,  92),   S( 22,  85),   S( 76,  58),   S( 60,  59),   S(107,  43),   S(119,  35),   S(110,  34),

            /* rooks: bucket 4 */
            S(-99,  27),   S(-13, -36),   S(-29,  -5),   S(  0,   2),   S(-25, -19),   S( -2, -40),   S(  7, -41),   S(-12, -37),
            S(-30, -27),   S(-17, -25),   S(-44,  26),   S(  7,   6),   S(-37,  -9),   S(-10, -29),   S( -4, -45),   S(-41, -16),
            S(-18,  35),   S(-24, -35),   S(-33,   8),   S(-11, -29),   S( -8,  -1),   S( 18, -17),   S(  2,   4),   S(-54, -13),
            S(-34,  -7),   S(  4,  -1),   S(-16,  27),   S( 24,   5),   S( 22,   1),   S(-14, -18),   S(-37,  18),   S(  0,  10),
            S(-35,  -1),   S(-33,  50),   S( -2,  -3),   S( 58,  13),   S( 37, -13),   S(-32,   7),   S( 40,  15),   S( 29, -24),
            S( 16,  -3),   S( 45,  11),   S( 49,  28),   S( 39,  22),   S( 66,  -1),   S( 22,  30),   S(  2,  24),   S( 19,  17),
            S(  5,   9),   S( 17,  33),   S( 11,  22),   S( 40,  20),   S( 56,   5),   S( 15,   1),   S( 34,  16),   S( 44,   6),
            S( 24, -50),   S( 22,  21),   S( 25,  14),   S( 26,  11),   S( 17, -10),   S(  3,   5),   S( 31,   4),   S( 13,   9),

            /* rooks: bucket 5 */
            S(-55,  45),   S(  8,  11),   S(-22,  28),   S(-27,  11),   S(-14,  11),   S(-14,  17),   S(  0,  28),   S(-62,  45),
            S(-38,  30),   S(-47,  36),   S(-88,  73),   S(-64,  27),   S(-58,  42),   S(-12,   0),   S( 12,  10),   S( -7, -12),
            S(-27,  38),   S(-65,  54),   S(-70,  78),   S(-75,  49),   S(-62,  29),   S(-33,  34),   S(-29,  33),   S(-25,  12),
            S(-22,  57),   S( -1,  36),   S(-16,  56),   S(-27,  45),   S(-50,  77),   S(-33,  63),   S( 33,  37),   S(-13,  41),
            S( 21,  48),   S(-10,  63),   S( 27,  51),   S( 35,  77),   S( 37,  43),   S( 21,  46),   S( 79,  40),   S( 66,  16),
            S( 46,  58),   S( 55,  54),   S( 76,  50),   S( 56,  56),   S( 59,  53),   S( 91,  35),   S( 47,  38),   S( 34,  31),
            S(  5,  66),   S(  4,  65),   S( 41,  54),   S( 38,  58),   S( 28,  52),   S( 69,  45),   S( 82,  37),   S( 65,  51),
            S( 91,  19),   S( 83,   6),   S( 48,  52),   S( 40,  30),   S( 52,  25),   S( 75,  28),   S( 47,  33),   S( 31,  40),

            /* rooks: bucket 6 */
            S(-54,  36),   S(-15,  23),   S( -6,  26),   S(  5,   9),   S(-37,  26),   S(-65,  49),   S(-35,  49),   S(-12,  38),
            S( 19,   8),   S(-14,  36),   S( -5,  25),   S(-57,  32),   S(-49,  45),   S(-79,  68),   S(-59,  39),   S(  7,  22),
            S(-63,  63),   S(-50,  67),   S(  1,  41),   S(-76,  59),   S(-11,  29),   S(-72,  68),   S(-51,  76),   S( 44,  11),
            S(-73,  76),   S( 14,  45),   S( -7,  63),   S(-22,  62),   S(-14,  60),   S(  8,  49),   S(-65,  79),   S(-28,  46),
            S(-18,  76),   S( 21,  65),   S( 84,  35),   S(  9,  54),   S( -9,  78),   S( 14,  65),   S( -6,  73),   S( 19,  53),
            S( 10,  62),   S( 54,  60),   S(102,  35),   S( 62,  23),   S( 15,  59),   S( 59,  64),   S( 40,  47),   S(101,  37),
            S( 69,  53),   S( 77,  42),   S(121,  28),   S( 91,  28),   S( 79,  46),   S( 58,  62),   S( 80,  27),   S( 66,  50),
            S( 56,  62),   S( 71,  35),   S( 53,  45),   S( 34,  40),   S( 49,  44),   S( 71,  44),   S( 85,  43),   S( 54,  32),

            /* rooks: bucket 7 */
            S(-75,   5),   S(-36,  -5),   S(-24, -16),   S(-18,   0),   S( -2,  -7),   S(-35,  27),   S(-38,  30),   S( 23, -24),
            S(-105,  40),  S(-27,  13),   S(-15,   2),   S(-16,   0),   S(-15,  12),   S( -4,  19),   S( -7,   1),   S(-54,  10),
            S(-98,  56),   S(-22,  22),   S(-26,  27),   S( -7,   2),   S(-17,  18),   S(-25,   5),   S( -6,  -6),   S( -4,  10),
            S(-60,  36),   S( 28,  10),   S(-18,  38),   S( 55,  12),   S( 44,   7),   S( 34,  12),   S( 61,   5),   S(  5,  -2),
            S( -1,  29),   S(-19,  30),   S( 53,  -6),   S( 54,  -2),   S( 78,   3),   S(126,  -5),   S( 67,  18),   S(  3,   2),
            S( 39,  19),   S( 26,  18),   S(111, -14),   S(115, -17),   S( 77,  -4),   S(109,  -1),   S( 75,  24),   S( 43, -15),
            S( 34,  28),   S( 54,  12),   S( 76,   7),   S( 96,   0),   S(145, -22),   S( 89,   4),   S( 42,  35),   S( 24,  17),
            S( 28,  56),   S(-11,  33),   S( 54,  11),   S( 81,  -5),   S( 40,  13),   S( 10,  24),   S( 63,   2),   S( 55,  -2),

            /* rooks: bucket 8 */
            S(-16, -24),   S(-22,  14),   S( -3,  36),   S(-47, -33),   S(-13, -33),   S(-33, -44),   S(-28, -15),   S( -9,  15),
            S(-19, -11),   S(-10, -12),   S(-29,  -6),   S( -3, -30),   S(-15, -16),   S(-10, -33),   S( -8,  -8),   S(-20, -87),
            S( 10,  15),   S(  0,  -7),   S(-23,   5),   S(-12,  -1),   S(-22, -22),   S(-15, -21),   S(-15,   6),   S( -7, -15),
            S( -5, -21),   S( -1,  21),   S(-13,  24),   S( 11,  24),   S(  7,  31),   S(-16,  -9),   S( 11, -19),   S(-17, -27),
            S(-17, -16),   S( -5,   4),   S(-13,  36),   S( -3,   6),   S( -7, -13),   S(  6,  13),   S( -3, -16),   S(-19, -28),
            S( 10,  15),   S(-16,  16),   S( 19,  27),   S(  7,  -6),   S(-15, -16),   S(-10, -20),   S(  7,   8),   S(  0,  22),
            S(  7,  -6),   S(-27,  16),   S( 22,  14),   S( 19,  10),   S( 20,  -3),   S(  6, -25),   S(  7, -36),   S( -2,   0),
            S( -3, -113),  S( 15,   2),   S( 12,   8),   S(-11,  -4),   S( -8, -14),   S( -6, -24),   S(  7,  -6),   S(  2,  22),

            /* rooks: bucket 9 */
            S(-60, -21),   S(-16, -40),   S(-41, -36),   S(-63, -21),   S(-61, -20),   S(-17, -25),   S( -8, -36),   S(-55, -35),
            S( 14, -14),   S( -3, -44),   S(-35, -39),   S( -7, -34),   S(-24, -19),   S(  9, -23),   S(  6, -20),   S(-23, -23),
            S(-17, -19),   S( 12, -15),   S(-14,  -5),   S(-17,  -9),   S(-49, -30),   S(  9, -29),   S(  7, -10),   S( -5, -36),
            S( -3, -15),   S(  1,  -7),   S( -2,  -7),   S(-25, -13),   S(-18, -15),   S(-11, -27),   S(  0, -22),   S( -9, -24),
            S( -8,   4),   S(-16,  12),   S( -9, -10),   S(  4,  15),   S( 20,  -1),   S( 11, -20),   S(  4,   4),   S(-10, -41),
            S(  0,   9),   S(-21, -17),   S( -9,   5),   S(-23,  -4),   S( -8, -18),   S( 18, -11),   S(  0, -25),   S(  6, -36),
            S( 21,   6),   S( 48, -17),   S( 56,  13),   S( 30,  -8),   S( 15, -37),   S( 23, -18),   S( 40, -10),   S( 35,   8),
            S( 42, -88),   S( 13, -66),   S( 34, -11),   S( 21,   3),   S(  5, -16),   S( 13,   2),   S( 16,  -6),   S( 27,  -9),

            /* rooks: bucket 10 */
            S(-69, -91),   S(-51, -56),   S(-39, -49),   S(-48, -23),   S(-46, -61),   S(-28, -50),   S( -8, -52),   S(-20, -30),
            S(-27, -32),   S(-18, -49),   S(-15, -36),   S(-39, -15),   S(  0, -38),   S(-38, -37),   S( 30,  -2),   S( -1, -48),
            S(-44, -42),   S(-21, -42),   S( -8, -41),   S( -7, -20),   S(-30, -42),   S( -3, -32),   S( 18, -19),   S( -6, -17),
            S(-15, -14),   S(-13, -23),   S(-23, -23),   S(-14, -10),   S(-12, -21),   S(-15, -28),   S( -1,  10),   S( -7, -19),
            S( -6, -14),   S( 17, -28),   S(-12, -30),   S(  2, -42),   S(-12, -12),   S(  1,   4),   S( 23,   9),   S(-11, -22),
            S(  5, -18),   S( 18,  10),   S( -4,  -9),   S(  0, -19),   S( -4,  -8),   S( -6, -17),   S( 18, -16),   S( -9, -22),
            S( 63, -25),   S( 63, -39),   S( 46, -26),   S( 36, -31),   S( 33,  -9),   S( 32,  -6),   S( 32, -26),   S( 29, -39),
            S( 28,  -7),   S( -4, -34),   S( 21, -38),   S(  3, -13),   S( 26, -15),   S( 11, -39),   S( 19, -40),   S(  3, -21),

            /* rooks: bucket 11 */
            S(-53, -35),   S(-41, -20),   S(-29,   2),   S(-34, -69),   S(-32,  -1),   S(-41,  17),   S(-42, -53),   S(-65, -35),
            S(-45, -27),   S( -8, -25),   S(-32, -23),   S(-30,   4),   S(-25, -23),   S(-28,  -7),   S(  5,  -4),   S(-29,   3),
            S(-12, -40),   S(  7, -44),   S(-20, -11),   S(-24, -21),   S(  6, -24),   S(-17,   0),   S(-45, -19),   S(-24, -64),
            S(-14,  12),   S(-20, -25),   S(-13,   6),   S( -3,   4),   S( -4,   8),   S(-11,   7),   S(-15,  -9),   S(-23, -51),
            S(-16,  -2),   S( 15, -25),   S( 19,  20),   S( 18, -38),   S( 15,   6),   S( 36, -27),   S(  4,   1),   S(-17, -12),
            S(  1,  18),   S( 28,  11),   S( 18, -11),   S( 47,   6),   S( 31,  10),   S( 36, -16),   S(-26,  -1),   S(  2,  -2),
            S( 36,  23),   S( 28,   2),   S( 54, -14),   S( 57, -18),   S( 39,   6),   S( 28,  19),   S( 18,  32),   S( 27, -14),
            S( 29,  26),   S(  3, -11),   S(  2, -26),   S(  7, -18),   S(  2, -12),   S( 13,  -3),   S( -2,   8),   S( 25, -18),

            /* rooks: bucket 12 */
            S(  3, -39),   S(-15, -56),   S(-19, -40),   S(  4,  -3),   S( -9, -30),   S( -1, -19),   S(-30, -69),   S(-16, -20),
            S( 17,   9),   S(  1,  -1),   S(-13,  14),   S(  0,  -3),   S(-11,  -6),   S( -2, -21),   S( -1, -14),   S( -8, -45),
            S(  3,  -4),   S( -8, -17),   S(-10, -22),   S(  2,   4),   S(-11, -32),   S( 10,   3),   S( -3, -13),   S( 10,   0),
            S(-11, -20),   S(  1, -17),   S( 19,  30),   S( -3, -43),   S( -6, -35),   S(-10, -25),   S(-17, -47),   S( -9, -37),
            S(-24, -41),   S( -7,  -6),   S( 15,  16),   S(  5,  -2),   S( -8, -24),   S(  7,   0),   S( -9, -39),   S( -6, -31),
            S( -5, -12),   S( -6, -17),   S(  4,  -8),   S( 12, -11),   S(  2, -15),   S( -9, -36),   S(  5,  -3),   S( -9, -22),
            S( -6, -16),   S( -2, -17),   S(-10, -54),   S( -3, -28),   S(  9, -14),   S(-22, -77),   S(  0, -13),   S(  5,  -5),
            S(-15, -66),   S( -4, -13),   S(  4,  -2),   S( -4, -42),   S(  1, -25),   S(-10, -37),   S(-10, -65),   S( 10,   9),

            /* rooks: bucket 13 */
            S(-22, -53),   S(-15, -21),   S( -5,  -4),   S( -7,  46),   S( -3, -14),   S(-18, -46),   S( -3, -35),   S(-24, -30),
            S( -4,  -5),   S( -9, -20),   S(-32, -22),   S(-11,  -2),   S(-11,  -7),   S( -6, -18),   S(  0,   0),   S(  0, -12),
            S(-16, -51),   S(-14, -38),   S(-25, -43),   S(  4, -12),   S(  3,  29),   S( -5, -18),   S(  2,   5),   S( -5, -51),
            S(-18, -56),   S(  4,  13),   S(-18, -47),   S( 13,  24),   S( 11,   7),   S(-17, -41),   S( -5, -36),   S( -2, -23),
            S(  3, -19),   S( -1, -17),   S( 24,  31),   S(-10, -42),   S( -6, -11),   S(-10, -36),   S(-13, -62),   S( -6, -28),
            S(-12, -33),   S( -5, -17),   S(-21, -45),   S( 11, -11),   S(  8,   4),   S( 17,  25),   S(  8,  -8),   S(  0,  -8),
            S(-13, -21),   S(  7,  11),   S(  0,  -3),   S( -2,  -5),   S(  6, -16),   S( 15,  38),   S(  1, -19),   S( -7, -30),
            S(-28, -142),  S(-13, -44),   S(  4,  -5),   S(  9,  25),   S(  1,  10),   S(-16, -65),   S(-18, -72),   S(  1,   7),

            /* rooks: bucket 14 */
            S( -7, -27),   S(-22, -48),   S(  8,  -3),   S(  0,   6),   S( -6,  18),   S( -3,   1),   S( 10,   0),   S( -8,   1),
            S(-23, -48),   S(-16, -50),   S( 14,  31),   S(-17, -30),   S(-16, -29),   S(-10, -31),   S( -1,   8),   S( 12,  -6),
            S(  4,  10),   S(-12, -39),   S( -2, -17),   S(-10, -40),   S(-14, -11),   S( -6,  -1),   S(  6,  17),   S( -8, -22),
            S( -4,  -7),   S( -4, -25),   S(  0, -19),   S(-17, -36),   S( -9, -34),   S( 10,   7),   S( -4, -28),   S(-17, -21),
            S( -4, -49),   S( -1, -41),   S(-12, -53),   S(-11, -52),   S( -3, -48),   S( -5, -57),   S(  6, -34),   S( -8, -37),
            S( -6, -23),   S(  0,  -1),   S(  0, -47),   S(  0, -67),   S(  0, -59),   S(  7, -44),   S(  9,  -1),   S( -1, -20),
            S(  9,   6),   S( -1, -40),   S( -1, -61),   S(-15, -39),   S(  1, -63),   S( 12, -21),   S(  2, -43),   S( -1, -29),
            S( -3, -53),   S(  4, -16),   S(-20, -65),   S( -1, -23),   S( -7, -20),   S(  3,  23),   S(  8,  19),   S(-14, -23),

            /* rooks: bucket 15 */
            S(-13, -12),   S(-27, -68),   S( -9, -50),   S( -7, -37),   S( -7, -13),   S(-20, -22),   S(-18, -67),   S( -8,  -4),
            S( -7,  -6),   S( -6, -24),   S( -6,  -4),   S(-10, -17),   S(-21, -41),   S( -9,   1),   S(-11, -37),   S(-11, -28),
            S( -8, -26),   S(-11, -33),   S(-11, -44),   S(  0, -16),   S(  6,   3),   S( -5,   2),   S(-11, -10),   S(  0, -21),
            S( -6, -43),   S( -6, -28),   S(-12, -21),   S(-12, -40),   S(-11, -42),   S(-14, -63),   S( -9, -44),   S(-10,   3),
            S( -2, -36),   S( -5, -21),   S(  8, -17),   S( -2, -38),   S( -3, -33),   S( 15,  15),   S(  7, -27),   S(  6,  21),
            S(  4,  -6),   S(-16, -47),   S( -5, -52),   S(  7, -42),   S( -3, -41),   S( 16, -19),   S( 10, -20),   S(-10,  -2),
            S(  2,  -7),   S( 19,   0),   S( 12,  -9),   S(  0, -45),   S( -4, -54),   S(  9,  -9),   S(  4,  -3),   S( -3, -16),
            S( -8, -36),   S( -4,   1),   S(  3, -34),   S( 10, -17),   S( -1, -22),   S(  5,  -7),   S( -6, -41),   S( -4, -10),

            /* queens: bucket 0 */
            S(-30, -23),   S(-23, -66),   S( 40, -92),   S( 45, -36),   S( 30, -38),   S( 24, -11),   S( 89,  -4),   S( 19,  -1),
            S(-19,   5),   S( 39, -52),   S( 35, -12),   S( 35,  -9),   S( 30,  14),   S( 15,  21),   S( 24,  31),   S( 45,  22),
            S( 32,   7),   S( 36, -12),   S( 19,  41),   S( 21,  42),   S( 34, -15),   S( 18,  30),   S(  1,  27),   S( 18,  47),
            S(  9,  13),   S( 22,  38),   S( 15,  40),   S( -4,  70),   S( 19,  38),   S( 13,  48),   S( 11,  19),   S( 29,  -1),
            S( 40,  37),   S( 33,   6),   S( 16,  38),   S( 19,  67),   S( -9,  40),   S(  7,   4),   S( 12,   1),   S( 16, -12),
            S(-12,  62),   S( 42,  44),   S( 33,  32),   S( 36,  -3),   S( 56,  -1),   S( 15,  38),   S(  3,  19),   S( -7, -13),
            S( 41,  23),   S( 53,  12),   S(-13,  74),   S( 55,  40),   S( -3,   8),   S( -5,  -9),   S( 33,  27),   S( 30,   0),
            S( 46,  38),   S( 15,  12),   S( 74,  32),   S( 67,  58),   S( 42,  22),   S(-33,  -4),   S( 22,  21),   S( 55,  15),

            /* queens: bucket 1 */
            S(  3, -56),   S(-78, -28),   S(-64, -47),   S( -6, -97),   S(  0, -45),   S( -2, -46),   S(  6, -56),   S(-14,   7),
            S( -7, -47),   S(-16, -22),   S(  8, -28),   S(  9,  35),   S(  2,   0),   S( 17, -43),   S( 27, -55),   S( 12,  28),
            S(-19,  28),   S(  6,   1),   S( 17,  -7),   S(  6,  21),   S( -1,  46),   S(  4,  10),   S( 20,   7),   S( 11,  12),
            S(  6,  -1),   S( -8,  16),   S( -5,  43),   S( 24,  34),   S( -8,  52),   S( 15,  13),   S(-16,  26),   S(  6,  38),
            S(  7,  24),   S(-15,  61),   S( -9,  63),   S(-13,  78),   S(-16,  75),   S(  7,  39),   S(-29,  30),   S(  1,  47),
            S( 26,  21),   S( 48,  43),   S( 48,  34),   S(-13,  68),   S( -2,  66),   S( 12,  23),   S( 14,  40),   S( 27,  37),
            S(-20,  43),   S(-14,  66),   S( 30,  22),   S( 28,  58),   S(-36,  72),   S( 23,  84),   S( 17,  34),   S(-28,  57),
            S(-21,  29),   S( 26,  29),   S( 10,  24),   S( 10,  54),   S(  3,  17),   S( 13,  28),   S(  3,  32),   S(-11,  42),

            /* queens: bucket 2 */
            S( 18,  -1),   S( 22, -55),   S(  2, -25),   S(  7, -28),   S(-30,   8),   S(-36, -35),   S(-12, -62),   S(  0,  11),
            S(  3,  23),   S( 19,  23),   S( 17,  -6),   S( 32, -29),   S( 16, -15),   S( -7, -18),   S( 34, -26),   S( 43, -27),
            S( 15,  15),   S( 23, -15),   S( 13,  33),   S( 15,  40),   S(  6,  68),   S( 15,  62),   S(  9,  19),   S( 29, -16),
            S( 13,  -3),   S(-13,  55),   S( 11,  47),   S( 16,  52),   S(-11,  82),   S(  7,  67),   S( 21,  13),   S(  8,  44),
            S( 22,  -3),   S( -8,  66),   S(-28,  75),   S(-41, 121),   S(-39, 113),   S(-27, 117),   S(-19, 122),   S(-13,  99),
            S(  9,  44),   S(-15,  68),   S(-46, 106),   S( -1,  60),   S(-13,  86),   S(-54, 141),   S(-20, 123),   S(-12,  72),
            S(-25,  69),   S(-43, 121),   S(  6,  55),   S(  8,  61),   S( -7, 100),   S( 13,  61),   S(-40,  66),   S(-29,  91),
            S(-55,  81),   S(-26,  45),   S( 59,  52),   S(  9,  61),   S( 40,  65),   S(  8,  42),   S( 23,  56),   S(-25,  46),

            /* queens: bucket 3 */
            S( 66,  78),   S( 55,  77),   S( 33,  91),   S( 40,  86),   S( 63,  14),   S( 32,  18),   S( 10,  30),   S( 49,  58),
            S( 57,  93),   S( 49, 112),   S( 33, 113),   S( 48,  69),   S( 35,  77),   S( 47,  47),   S( 48,  19),   S( 32,  15),
            S( 48,  87),   S( 43,  89),   S( 38,  94),   S( 44,  75),   S( 44,  88),   S( 43,  98),   S( 38, 100),   S( 42,  56),
            S( 32, 116),   S( 34,  95),   S( 42,  98),   S( 33, 101),   S( 29,  93),   S( 27, 141),   S( 40,  95),   S( 18, 140),
            S( 36, 112),   S( 37, 115),   S( 24, 111),   S( 17, 113),   S( 25, 132),   S( 11, 136),   S(  9, 176),   S( 20, 167),
            S( 42, 122),   S( 30, 115),   S( 34, 105),   S(  8, 119),   S( 16, 142),   S( 56, 116),   S( 51, 148),   S(  8, 193),
            S( 39, 114),   S( 42, 114),   S( 83,  87),   S( 39, 106),   S( 22, 132),   S( 40, 109),   S( 61, 144),   S(128,  89),
            S( 50,  97),   S( 76,  83),   S( 50, 118),   S( 76,  88),   S( 31, 112),   S( 85,  67),   S(128,  53),   S(127,  51),

            /* queens: bucket 4 */
            S(  1, -20),   S(  4, -10),   S(-26,   9),   S( 24, -31),   S( -8, -55),   S( 38,  -1),   S(-19,  -5),   S(-11,   4),
            S( -9,  -1),   S(-10, -19),   S( 53, -13),   S(-54,  30),   S( 26, -21),   S(  4, -16),   S( -2, -24),   S(-39, -35),
            S( -3,   0),   S(-12,   1),   S( -7,  65),   S( 10,  48),   S( 40,   2),   S( 18,   8),   S( 21, -20),   S(-28, -27),
            S(-39, -27),   S(  6,  40),   S(  9,  56),   S( 18,  36),   S( 49,  55),   S( -9,  37),   S(  8, -28),   S( 10,  13),
            S(  9,   9),   S( 35,   4),   S( 36,  33),   S( 40,  38),   S( 18,  36),   S( 37,   4),   S(-22, -24),   S(  5, -17),
            S( 25,  13),   S( -4, -20),   S( 28,  49),   S( 24,  53),   S( 20,  30),   S(-10, -15),   S(-24, -29),   S(-18, -24),
            S(-45, -21),   S(-21,  34),   S(-25,  10),   S( 24,  35),   S(  6,  -2),   S( 12,  30),   S( -4, -22),   S(-27, -46),
            S(-36, -47),   S( -7,   1),   S( 39,  41),   S( -4,   5),   S(-27, -12),   S( -2,   6),   S(-19, -49),   S( -2, -11),

            /* queens: bucket 5 */
            S(-46, -27),   S(-52, -52),   S( -9, -33),   S(-29, -13),   S(-49, -32),   S( 25,  -8),   S(-33, -21),   S(  0,  -8),
            S(-39, -37),   S( -9,  -5),   S(-29,  -8),   S(-74,  13),   S( 16, -17),   S(-16, -23),   S(-47, -29),   S(-76, -11),
            S(-41, -15),   S(-44,  16),   S(-39,  -7),   S(-40,  40),   S( 36,  48),   S(-29,  29),   S( -1, -13),   S( 24,  33),
            S(-40,   1),   S(-52, -12),   S(-13,  53),   S(  6,  55),   S( 16,  44),   S(  6,  50),   S( 12,  -4),   S(-27,  10),
            S(-31,  -3),   S(-25,  17),   S(  8,  83),   S( 17,  71),   S( 37,  77),   S(  1,  23),   S( -1,  -1),   S(-21, -16),
            S(-51, -18),   S( 21,  54),   S(  5,  54),   S(  1,  54),   S( 49,  33),   S( 40,  41),   S(  9,  11),   S(  0, -42),
            S(-15,   0),   S( 13,  30),   S(  0,  75),   S( 17,  54),   S( 38,  80),   S( 16,  43),   S( 16,   8),   S(-23, -23),
            S(  5,  10),   S( 32,  45),   S(  5,  36),   S( 24,  63),   S( 19,  47),   S( 12,  22),   S(-27, -46),   S(-33, -31),

            /* queens: bucket 6 */
            S(-55, -30),   S(-29,  -7),   S(-48, -22),   S(-98, -64),   S(-65, -29),   S(-55, -42),   S(-33, -45),   S(-15, -10),
            S(-48,  -4),   S(-36,  21),   S(-19,  13),   S(-16,   0),   S(-74,  49),   S(-79,  -5),   S(-80,  -2),   S( 18,  -2),
            S(-12,   8),   S(-25,  21),   S(-19,  20),   S(-54,  79),   S(-26,  84),   S(-20,  20),   S(-47,  -7),   S( 28,  29),
            S(-67,  36),   S(-27,  40),   S(-19,  55),   S(-28,  66),   S( -3,  81),   S( 19,  58),   S( -2,  43),   S(  0,   7),
            S(-43,  41),   S(-21,  59),   S(-35,  67),   S( 33,  38),   S( 26,  73),   S( 65,  69),   S( 42,  36),   S( 29,   1),
            S(-41,  12),   S( 15,  21),   S( 65,  29),   S( 47,  61),   S( 18,  87),   S( 95, 104),   S(-24,  21),   S( -1,   5),
            S(  3,  27),   S( 26,  21),   S(  4,  75),   S(-13,  37),   S( 43,  60),   S( 45,  84),   S( -3,  41),   S(-22,   7),
            S( 11,  20),   S( 19,  31),   S( 34,  43),   S(  3,  34),   S( 42,  84),   S( 28,  34),   S( -5,  12),   S( 24,  20),

            /* queens: bucket 7 */
            S(  0, -20),   S(-36,  15),   S(-20,  -3),   S(-24,   4),   S(-28,  -9),   S(-38,  -6),   S(-27,   7),   S( -5, -24),
            S(-14,  -1),   S(-56,  15),   S(-24,   7),   S(-20,  53),   S(-25,  30),   S(-42,  28),   S(-37,  56),   S(-13, -31),
            S(-69,  10),   S(-61,  49),   S(  5,  20),   S(-18,  51),   S( 11,  49),   S( 11,  50),   S( 11,   9),   S(  7,  -9),
            S(-31,  13),   S( 14,  -5),   S(-14,  53),   S(  1,  50),   S( 29,  42),   S( 54,  33),   S( -3,  94),   S( 17,  16),
            S(-72,  48),   S(-63,  36),   S(  4,  45),   S( 34,  26),   S( 48,  24),   S( 86,  10),   S( 75,   5),   S( 27,   6),
            S(-17,  29),   S(-17,  29),   S(-42,  47),   S( 11,  -2),   S( 32,  57),   S( 93,  29),   S( 60,  30),   S( 66,  13),
            S( 17, -13),   S( -2,  25),   S(  0,   3),   S( 37,  42),   S( 16,  37),   S( 57,  35),   S( 48,  -6),   S( 93,  52),
            S( 23,  10),   S( 10,   7),   S( 37,  37),   S( 13,  25),   S( 47,  26),   S( 44,  40),   S(  6,   9),   S( 31,  37),

            /* queens: bucket 8 */
            S(-11, -28),   S(  6,  -1),   S( -6,  -4),   S(  5,  17),   S(-11,  -9),   S( -4, -15),   S(-20, -31),   S(-11, -23),
            S( -8,  -7),   S(-20, -33),   S( -3,   1),   S(-22,  -9),   S( -8, -10),   S(-13, -11),   S(-33, -75),   S( -7, -10),
            S( -6,  -5),   S(-16,  -1),   S(-11,  -6),   S( -4, -11),   S(-32, -36),   S( -8,  -8),   S(  2,  -8),   S(-18, -37),
            S(  8,  19),   S(  0,  16),   S(  5,  27),   S(-10,   9),   S(-16, -21),   S(-18, -26),   S(-13, -23),   S(-13, -34),
            S( 11,  19),   S( 10,  51),   S( 26,  42),   S(  7,  36),   S( 14,  39),   S(-11, -13),   S(-20, -35),   S(-10, -19),
            S( 14,  25),   S(  9,  20),   S(-19,  23),   S(  7,  34),   S(-30, -48),   S( -8,  -7),   S(  6,  20),   S( -6,  -7),
            S( -4, -17),   S(-26, -26),   S( 16,  39),   S( 10,  12),   S(  1,  -6),   S( -5,  -6),   S(-12, -21),   S(-17, -39),
            S(-14, -32),   S( 14,  11),   S(-19, -46),   S(-17, -36),   S( -5, -17),   S(  0,  -6),   S( -2, -20),   S( -4, -10),

            /* queens: bucket 9 */
            S(  6,  11),   S(-12, -32),   S( 11,  17),   S(-27, -28),   S(-20, -39),   S(-19, -35),   S(-17, -38),   S(-27, -49),
            S(-14, -33),   S( -9, -15),   S(-20, -17),   S(-25, -32),   S(-21, -10),   S(-16, -34),   S(  6,  -4),   S(-14, -36),
            S(  6,  15),   S(  0,  20),   S(-28,  -8),   S(-35, -57),   S(-11, -14),   S(-14, -19),   S(-16, -19),   S( -9, -23),
            S(-13, -12),   S(-26, -21),   S(  7,  45),   S( -8,  18),   S(  2,  18),   S(  8,   1),   S(-23, -40),   S(-24, -51),
            S(  8,  16),   S(  4,  23),   S( 15,  43),   S( 25,  61),   S(  2,  25),   S(-16, -27),   S( -2,  -6),   S(-14, -27),
            S(-24, -34),   S(-47, -28),   S( -6,  19),   S( 30,  55),   S(  2,   9),   S(-13,  -7),   S(-24, -40),   S( -7, -10),
            S(  0,  -4),   S(-11, -12),   S(-21,  10),   S( 16,  33),   S(  8,   5),   S( -1, -10),   S( -1,  -8),   S(-23, -46),
            S(-16, -28),   S( -9, -26),   S(-18,  -2),   S(-16,  -6),   S( -2,  -9),   S( -6,  -2),   S(  4,   0),   S(-14, -28),

            /* queens: bucket 10 */
            S( -2,  -7),   S( -8, -12),   S(-18, -34),   S(-33, -42),   S(-22, -33),   S( -7, -11),   S( -1,  -3),   S(-11, -30),
            S(-18, -27),   S(-24, -41),   S(-20, -35),   S(-19, -15),   S(  2,  12),   S(-16, -16),   S( -2, -12),   S(-15, -26),
            S(-14, -28),   S(  8,   1),   S(-16,  -5),   S( -9,  -3),   S(-35, -21),   S(-14,  -3),   S( -6,  -1),   S(  1,   0),
            S( -9,  -5),   S(  9,  10),   S(-23, -14),   S(  6,  27),   S( 18,  59),   S(-17,  21),   S(  8,  16),   S( -3,  -1),
            S(-12, -21),   S(-11, -14),   S(-15,   7),   S(  9,  40),   S(-12,  -3),   S( 13,  36),   S(  0,   2),   S(-12,  -9),
            S( -7, -11),   S(-32, -48),   S(-26, -11),   S( -8,  19),   S(  7,  42),   S( 23,  41),   S(  1, -11),   S(-12, -16),
            S(  2,  14),   S(-11,  -5),   S(-13, -17),   S(-20, -19),   S(-18, -26),   S( 10,  13),   S( -7,  -8),   S( -7, -10),
            S(-10, -11),   S( -8, -14),   S(  8,  17),   S(  6,  19),   S(  0,  -6),   S(  6,   8),   S(  6,   9),   S(-10, -20),

            /* queens: bucket 11 */
            S(-20, -49),   S(-15, -38),   S(-12, -21),   S(-12, -33),   S(-29, -30),   S( -7,  -3),   S(-12, -11),   S(-21, -27),
            S(-27, -39),   S( -7,  -3),   S(-40, -43),   S(-16,  -2),   S( 13,  14),   S(-10, -11),   S( -7,  -1),   S(-12,  -5),
            S(-40, -56),   S(-20, -28),   S(-15, -27),   S(-33, -48),   S(-16,  -2),   S( -6,  23),   S(  2,  16),   S(-30, -16),
            S(-32, -69),   S(-19, -34),   S( -6, -27),   S( 10,  24),   S( 27,  43),   S(-27,  -3),   S( 19,  22),   S(-12, -24),
            S(-19, -27),   S(-14, -31),   S(-31, -39),   S( 22,  17),   S( 14,  19),   S( 47,  86),   S( 24,  46),   S(-14, -12),
            S(-18, -42),   S(  9,  12),   S(-26, -16),   S( 23,  37),   S( 17,  32),   S( 41,  63),   S( 13,  19),   S(-12, -12),
            S(-22, -34),   S(-12,  -6),   S(  9,  10),   S( -4,  -8),   S(  7,  32),   S( 35,  56),   S( 27,  30),   S(-22, -22),
            S( -9, -15),   S(-13, -28),   S(-26, -45),   S(  9,   9),   S( -2,  -3),   S(  7,  -9),   S( 14,  14),   S(-10, -25),

            /* queens: bucket 12 */
            S(  5,   7),   S( -4,  -6),   S(  0,   1),   S( -6,  -6),   S(-11, -18),   S( -5,  -8),   S( -7, -14),   S( -6, -15),
            S( -4, -12),   S( -5, -15),   S(-17, -29),   S(-10, -25),   S( -3,  -2),   S( -9, -20),   S( -1,  -7),   S( -6, -12),
            S( -4,  -1),   S( -4,  -9),   S( 14,  18),   S(-13, -24),   S( -7, -10),   S(-11, -20),   S(-15, -31),   S( -2,  -7),
            S(  5,   4),   S( -2,   6),   S( -1,  -2),   S(  7,  23),   S(  2,   1),   S( -3,  -9),   S(  5,  13),   S( -7, -28),
            S( 10,  13),   S( 14,  21),   S( 29,  40),   S( 12,  33),   S(  3,  16),   S( -6,  -4),   S( -6, -11),   S( -6, -14),
            S(  5,  19),   S( 18,  35),   S( 33,  58),   S(  1,   1),   S( -5,  -2),   S( -7, -12),   S(  2,   9),   S( -8, -24),
            S( -2,  -5),   S( 18,  34),   S( 32,  46),   S( 23,  41),   S( -3, -10),   S(  9,  24),   S(  9,  16),   S( -3,  -7),
            S(-11, -27),   S(-10, -25),   S(-32, -49),   S( -9, -21),   S( -7,  -1),   S( -5,  -8),   S(  5,   1),   S(-13, -11),

            /* queens: bucket 13 */
            S( -1,  -7),   S( -6, -13),   S(-11, -23),   S(-12, -18),   S( -8, -19),   S( -2, -11),   S(-11, -25),   S(-12, -23),
            S(  9,  19),   S( -1,  -1),   S(  6,  11),   S( -2, -11),   S(-12, -21),   S(  5,  12),   S( -6, -13),   S(-18, -35),
            S( -1,  -3),   S(  3,   7),   S( -7, -11),   S( -6, -20),   S( -3,  -4),   S(-18, -39),   S(  0,  -2),   S( -5,  -5),
            S( -7, -11),   S( -6, -17),   S( 15,  14),   S(  9,  13),   S(  9,  14),   S(-10, -21),   S( -6, -18),   S( -5, -14),
            S(  0,  -4),   S(  6,  22),   S( 13,  19),   S( 12,  24),   S( 23,  46),   S( -4, -11),   S(  2,  -1),   S(-13, -31),
            S(  2, -10),   S( 23,  46),   S( 43,  93),   S( 25,  70),   S( -3,  18),   S( -7,  -8),   S(  6,  12),   S( -5, -12),
            S( -4, -13),   S( 16,  33),   S( 15,  37),   S( 28,  62),   S(  3,   6),   S(  4,   4),   S( -9, -22),   S(  7,  15),
            S(-21, -39),   S(-18, -36),   S( -3,  -5),   S(-15, -21),   S( -3, -13),   S(  2,   3),   S(  1,   5),   S( -5, -10),

            /* queens: bucket 14 */
            S( -5,  -9),   S( -7, -13),   S( -6, -20),   S( -3,  -9),   S(  4,  13),   S(-10, -25),   S(  0,  -1),   S( -7, -21),
            S( -8, -21),   S( -4,  -8),   S(-10, -26),   S( -9, -14),   S( -8, -14),   S( -8, -26),   S( -1,   4),   S(-10, -23),
            S( -3, -11),   S( -6, -11),   S(-21, -24),   S(-16, -25),   S( -4,  -8),   S( -2, -13),   S( -3, -11),   S(-10, -17),
            S( -9, -20),   S( 12,  18),   S( -3,  -5),   S( 21,  49),   S(  9,  14),   S(  1,  12),   S( 12,  20),   S(  0,  -8),
            S( -4,  -6),   S( -1,   0),   S(-12, -16),   S( 24,  29),   S(  4,  26),   S( 23,  43),   S(  2,  -5),   S( -8, -23),
            S( -2,  -5),   S( -4,  -3),   S(  7,  17),   S( 13,  28),   S( 24,  50),   S( -4,   6),   S(  7,  11),   S( -4, -16),
            S(  4,  10),   S(  1,  -9),   S( 20,  47),   S( 16,  32),   S(  3,   6),   S(  6,  11),   S( 19,  25),   S(  7,  14),
            S( -9, -18),   S(  0,  -3),   S(-11, -22),   S(  0, -14),   S( -9, -16),   S( -9, -20),   S(  1,  -1),   S(-10, -26),

            /* queens: bucket 15 */
            S( -1,  -7),   S( -8, -23),   S( -6, -15),   S( -4, -11),   S( -5, -11),   S( -6, -11),   S(-10, -21),   S( -5, -14),
            S( -8, -16),   S( -4, -16),   S(-16, -30),   S( -4,  -5),   S(  1,  -2),   S( -9,  -9),   S(  6,  11),   S( -1,  -2),
            S( -6, -18),   S( -3, -12),   S( -5, -14),   S( -7, -17),   S( -5, -18),   S(  7,  13),   S(  0,  -1),   S( -4,  -6),
            S( -4,  -7),   S( 10,  16),   S(-12, -16),   S(  2,  -2),   S( 10,  18),   S(  5,  13),   S(  2,   0),   S(  9,  20),
            S(  0,   2),   S(  3,  -2),   S( -4, -19),   S(  1,   2),   S( 16,  35),   S( -2, -10),   S( -6, -23),   S( -1,   0),
            S( -2,  -6),   S( -6, -15),   S(  0,  -9),   S( -7,  -5),   S(  6,  10),   S( 35,  56),   S( 12,  10),   S( -5, -17),
            S( -6, -12),   S( -5, -24),   S(  6,   9),   S(  6,  11),   S( 18,  31),   S( 21,  37),   S( 13,  17),   S(  2,   1),
            S( -5, -13),   S(  8,  11),   S(  4,   6),   S(  3,   6),   S( 10,  11),   S(-18, -40),   S( -1,  -8),   S(-19, -46),

            /* kings: bucket 0 */
            S( 14,  46),   S( 15,  61),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  69),   S( 93,  69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-14,  17),   S(-76,  27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 15,  42),   S(  1,  42),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-45,  33),   S(-46,  29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 22,  49),   S( 21,  43),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18,  59),   S( -5,  41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 54,  89),   S(  9,  70),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -32),   S( 18, -15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26,  -9),   S( 24,  14),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -42),   S(-38, -23),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11, -12),   S( -2, -13),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -7, -21),   S(-38, -13),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18,  -8),   S(-16,   3),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 28,  13),   S(-23, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 39,  39),   S(-28,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-98, -52),   S(-25, -16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54, -50),   S( 41, -32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-11, -51),   S(-39, -52),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 69, -67),   S( 23, -47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -8, -46),   S(-50, -33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 56, -51),   S( 80, -49),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -5, -25),   S(-83, -54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 67, -33),   S(  2, -62),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9, -55),   S( 28, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34, -83),   S( 16, -38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 20, -41),   S( 71, -36),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 35, -68),   S( 37, -57),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 70, -48),   S( 40, -47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 31, -65),   S(-14, -63),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 43, -39),   S(-59, -61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0, -52),   S( -4, -108),

            #endregion

            /* enemy king piece square values */
            #region enemy king piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -26),   S(-36,   6),   S(  3,  -9),   S(-18,  28),   S(  4,   9),   S( 29,  10),   S( 36, -12),   S( 53, -12),
            S( -7, -37),   S(-32,  -9),   S(-13, -13),   S(-12,   8),   S( 11,   3),   S(  2,   6),   S( 29,  -7),   S( 26,  10),
            S(  9, -20),   S(  1, -23),   S( 35, -28),   S( 14, -20),   S( 10,   1),   S( 22,  21),   S( 15,  22),   S( 57,   1),
            S( 31, -30),   S( 52, -16),   S( 58, -17),   S( 48, -10),   S( 18,  49),   S(  6,  75),   S( 17,  69),   S( 86,  40),
            S(104, -136),  S(137, -110),  S(100, -28),   S( 64,  35),   S( 57, 139),   S(  2, 135),   S( 29, 186),   S( 86, 127),
            S(-199, -174), S(-193, -213), S( 42, -195),  S( 18,  35),   S(101, 153),   S( 61, 176),   S(116, 136),   S( 37, 188),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58,  14),   S(-49,   5),   S(-29,   3),   S(-40,  21),   S(-22,   8),   S( -4,   6),   S( -4,  -4),   S( -1,  16),
            S(-55,  -3),   S(-44,  -4),   S(-41,  -3),   S(-26,   2),   S( -2,  -2),   S(-25,   3),   S( -9,  -9),   S(-25,   8),
            S(-32,  15),   S(-14,   3),   S(-19,  -3),   S( 12, -19),   S( -7,  16),   S( -3,   8),   S(-19,  12),   S(  0,  13),
            S(-18,  30),   S( 31,   5),   S(  6,   2),   S( 24,  12),   S( 19,  13),   S(-30,  36),   S( 13,  24),   S( 52,  38),
            S(  9,  30),   S( 78, -12),   S(107, -136),  S( 43, -126),  S( 65,  17),   S(  3,  41),   S( -3,  61),   S( 51,  92),
            S(136, -52),   S(-53, -54),   S(  7, -194),  S(-29, -199),  S(-61, -92),   S(-45,  64),   S( 31, 170),   S( 71, 167),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  29),   S(-37,   7),   S(-32,   3),   S(-24,  17),   S(-48,  38),   S(-36,  15),   S(-18,  -7),   S(-28,  26),
            S(-51,   8),   S(-39,   7),   S(-44,   2),   S(-45,  25),   S(-40,  18),   S(-42,   4),   S(-23, -13),   S(-51,   9),
            S(-31,  37),   S(-31,  33),   S(-17,  14),   S(-23,  19),   S(-33,  28),   S(-19,   7),   S(-25,   5),   S(-25,   8),
            S( -9,  62),   S(-17,  49),   S( -9,  34),   S( 11,  24),   S(  2,  20),   S(-11,   5),   S( 17,  10),   S( 29,  12),
            S(-28, 127),   S(-59, 114),   S(-39,  54),   S( 30, -27),   S(100, -115),  S( 75, -87),   S( 89, -31),   S( 46,  18),
            S(  2, 203),   S( 60, 125),   S(-17,  61),   S(  0, -96),   S(-46, -231),  S(-119, -230), S(  4, -88),   S(126, -50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8,  13),   S(-10,   7),   S( -5,  12),   S(-11,  37),   S(-21,  59),   S(  8,  36),   S( -2,   9),   S(-15,   1),
            S( -3,   6),   S(  1,  12),   S(-12,   8),   S(-18,  25),   S(  1,  23),   S(  5,  10),   S( -3,  -2),   S(-36,  -1),
            S( 18,  33),   S( -5,  50),   S(  5,  22),   S( -3,  11),   S( 15,   0),   S( 24,  -5),   S(  1,  -7),   S(-15,  -1),
            S( 22,  77),   S(  2,  86),   S( 11,  71),   S( 21,  27),   S( 34,   5),   S( 43, -21),   S( 27,   8),   S( 42, -15),
            S(  5, 161),   S(-24, 183),   S(-31, 179),   S( -6, 131),   S( 54,  67),   S( 74,  -9),   S( 98, -88),   S( 88, -85),
            S( 38, 163),   S( 23, 255),   S(-32, 282),   S(-25, 225),   S(-65, 100),   S(-17, -167),  S(-183, -272), S(-246, -243),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 41,   1),   S( 25, -17),   S(-27,   5),   S(  4, -29),   S(-10, -14),   S(  2, -15),   S( -3, -12),   S(-29,  23),
            S( 29, -17),   S(  5,   1),   S(  9, -22),   S(-23,   1),   S(-46, -15),   S(-18, -25),   S(-58, -11),   S(-40, -10),
            S(116, -50),   S(125, -53),   S( 37, -29),   S(-12, -19),   S(-56,   6),   S(-19,  17),   S(-63,  19),   S(-36,  25),
            S(-49, -119),  S(  7, -144),  S( 33, -33),   S(-69,  33),   S(-37,  32),   S(-53,  51),   S(-24,  50),   S( 30,  33),
            S( 34, -118),  S(-36, -137),  S(-30, -100),  S( 84,  31),   S( 65, 108),   S( 84,  55),   S( 17,  83),   S( -7, 108),
            S(  6, -39),   S( -6, -59),   S( 24, -74),   S( 34,  46),   S( 76, 120),   S( 57, 154),   S( 51, 139),   S( 53, 141),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71,  34),   S(-17,  10),   S( 21, -11),   S( 64,  -4),   S( 61, -11),   S( 18,  -6),   S(-20,  -2),   S(-55,  37),
            S(-49,   6),   S(  8,   0),   S(  6, -12),   S(  8,   7),   S(  1,   3),   S( -8, -13),   S(-56,  -3),   S(-73,  17),
            S(-18,  12),   S( 49,   8),   S( 96, -23),   S( 19,   9),   S( -8,  20),   S(-15,   6),   S(-12,   2),   S(-27,  23),
            S( 14,  10),   S( 56, -26),   S( -6, -83),   S(-52, -55),   S(-17, -16),   S(-24,  18),   S( 52,   4),   S( -9,  44),
            S( 56,   9),   S( 80, -60),   S( 70, -125),  S(  0, -69),   S( 44, -53),   S( 10,  23),   S(  9,  41),   S( -3,  88),
            S( 26,  39),   S( 40, -10),   S(-12, -94),   S(  0, -80),   S(-19, -49),   S( 54,  27),   S( 80, 128),   S( 90, 110),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-81,  16),   S(-47,  -5),   S( -2, -17),   S( 11,  -4),   S( 18,  14),   S( 21,   3),   S( 20,  -4),   S( 11,  16),
            S(-84,   3),   S(-35, -11),   S(-11, -20),   S( 60, -19),   S( -9,  15),   S( 18, -11),   S(  5,  -9),   S(  1,  -3),
            S(-51,  23),   S(-44,  19),   S( -9,   9),   S( 10,   8),   S( 43,   4),   S( 72, -21),   S( 53, -16),   S( 32,  -6),
            S(-30,  47),   S( -5,  21),   S( 29,   7),   S( 18,  -5),   S(-22, -66),   S( 19, -82),   S( 60, -32),   S( 99, -27),
            S(  6,  87),   S(-11,  45),   S( 53,   3),   S(  2, -44),   S(-22, -77),   S( 13, -92),   S( 68, -66),   S( 73,  10),
            S( 85,  67),   S(103,  80),   S( 61,  21),   S( 61, -72),   S(  0, -75),   S( 11, -98),   S(  4, -13),   S( 71,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  -8),   S(-45, -26),   S(  0, -31),   S(-45,  33),   S( 21,  12),   S( 58, -12),   S( 58, -29),   S( 58,  -9),
            S(-43, -19),   S(-40, -20),   S(-20, -35),   S(-14,  -6),   S( 10, -11),   S( 52, -33),   S( 44, -27),   S( 52, -26),
            S(-36,  16),   S(-54,  18),   S(-22,  -5),   S(-23, -10),   S(  4,  -3),   S( 41, -25),   S( 85, -34),   S( 86, -35),
            S(  4,  30),   S(-50,  56),   S(-17,  46),   S(-28,  37),   S(  5, -11),   S( 73, -68),   S( 38, -103),  S( 25, -132),
            S( 43,  59),   S(-46, 117),   S( 26, 108),   S( 12,  83),   S( -3,  51),   S( -5, -67),   S(-29, -124),  S( -9, -115),
            S(143, 100),   S(110, 140),   S(115, 116),   S( 56, 134),   S( 39,  17),   S( -8, -80),   S( -7, -76),   S(  1, -162),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 34, -23),   S(  8, -24),   S( 32,  -6),   S( -2, -21),   S(-19, -40),   S( 34, -36),   S( 41, -71),   S( 49, -50),
            S(-14, -58),   S( -6, -14),   S(-45, -64),   S(-35, -41),   S(-39, -47),   S( 43, -55),   S(  6, -54),   S(-35, -38),
            S(-64, -71),   S( 19, -73),   S( -4, -63),   S(-35, -49),   S(-20, -15),   S(-18, -22),   S(-23, -21),   S(-38,  -6),
            S(-19,  -9),   S(-19, -21),   S( 36, -20),   S( 14,  -9),   S(-12,  15),   S( -3,  36),   S(-14,  28),   S(-19,  27),
            S( 13,  25),   S(  6,   1),   S( 15,  19),   S( 32, 113),   S( 81, 131),   S( 41, 120),   S( 24,  96),   S(-27, 100),
            S( 30,  83),   S(  9,  43),   S( 22, 110),   S( 45, 117),   S( 44, 133),   S( 59, 194),   S( 58, 146),   S( -4,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 20, -13),   S( 60, -10),   S( 33, -16),   S(-14,  -5),   S( 11,   0),   S( 84, -45),   S( 81, -63),   S(-15, -24),
            S(-23, -46),   S(-20, -44),   S(-27, -61),   S(-35, -30),   S(-25, -36),   S(-35, -36),   S( 19, -54),   S( 17, -45),
            S(-85, -18),   S(-42, -50),   S(-32, -78),   S(-61, -34),   S(  8, -44),   S(  0, -41),   S(-36, -42),   S(-27, -10),
            S(-53,  -1),   S(-59, -47),   S(  8, -51),   S( -4, -34),   S( 23, -27),   S(-17,  -6),   S(-13,  -2),   S( 23,   3),
            S( -8,  26),   S( -6,   0),   S( 36,   0),   S( 22,  11),   S( 13,  56),   S( 13,  58),   S( -3,  86),   S(  1,  74),
            S(  3,  71),   S( 39,  73),   S( 25,  54),   S( 36,  20),   S( 38,  97),   S( 20,  61),   S(  5,  83),   S( 19, 111),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24, -67),   S( -5, -56),   S( -6, -25),   S( -8, -63),   S( 54, -26),   S(160, -32),   S( 64, -20),   S( 53, -28),
            S(-61, -56),   S(-50, -60),   S( 27, -69),   S( 43, -49),   S(-14, -40),   S( 25, -47),   S(  4, -46),   S(-19, -30),
            S(-52, -29),   S(-53, -48),   S(-28, -33),   S( 19, -46),   S(-26, -31),   S(-43, -62),   S(-25, -71),   S( 32, -49),
            S(-10, -14),   S( 12, -24),   S( -9, -20),   S(-20, -38),   S(-20, -31),   S(-45, -33),   S(-14, -33),   S(  9, -25),
            S( 12,  28),   S( 11,  37),   S( 50,  29),   S(  7, -10),   S( 25,  18),   S( 19,  14),   S(-17,   7),   S( 11,  24),
            S( -5,  37),   S( 13,  59),   S( 29,  52),   S( 28,  58),   S( 18,  19),   S(  4,  43),   S( -4,  19),   S( 27,  48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -49),   S(-29, -33),   S( -8, -38),   S( 22,   7),   S( -6, -23),   S(111, -12),   S(111, -38),   S( 63, -21),
            S(-43, -54),   S(-24, -79),   S(-37, -64),   S( 24, -46),   S(-41, -22),   S(-19, -42),   S(-21, -36),   S( 33, -76),
            S(-82,   1),   S(-36, -30),   S(  6, -19),   S( 42, -35),   S(-22, -47),   S( -4, -36),   S(-45, -88),   S(  4, -70),
            S(  0,  13),   S(-19,  22),   S(-11,  36),   S( 40,  -1),   S( -3, -12),   S(-28, -31),   S(-25, -53),   S(-37, -28),
            S(-13,  32),   S( -2,  82),   S( 35, 102),   S( 13,  78),   S( 52,  56),   S(  7,  15),   S( 17,  36),   S(-10,   6),
            S( 31,  83),   S( 38,  85),   S( 36,  88),   S( 45,  89),   S( 29,  63),   S( 50, 100),   S( 14,  29),   S( 39,  78),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -56),   S(-18, -13),   S(-12,   0),   S( -1,  13),   S( -8, -11),   S(-21, -49),   S( 19, -17),   S(-16, -47),
            S(-18, -47),   S(-35, -18),   S(-51, -45),   S(-29, -33),   S(-61, -48),   S(-25, -47),   S(-26, -45),   S(-25, -68),
            S(-39,   4),   S( 16, -43),   S(-17, -85),   S(-51, -102),  S(-11, -11),   S(-18,  -3),   S(-42, -31),   S(-30, -41),
            S(-29,  -3),   S( -3,   2),   S( -2,   7),   S( 12,  25),   S(  9,  63),   S( 24, 100),   S(-10,  53),   S(-38,  15),
            S( 28,  85),   S(  2,   8),   S( 12,  25),   S( 16,  71),   S( 42, 131),   S( 39, 125),   S(  7, 131),   S( 28, 106),
            S( 18,  25),   S(  4,  16),   S( 31,  80),   S( 22,  58),   S( 32, 121),   S( 45, 138),   S(-28,   1),   S( -9,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -68),   S(-16, -68),   S( -7, -14),   S( -5, -13),   S(  0,  -4),   S(-11, -33),   S(-12, -69),   S(-11, -45),
            S(-44, -58),   S(-42, -76),   S(-36, -65),   S( 18,  -9),   S(-30, -42),   S(-36, -34),   S(-28, -70),   S(-30, -58),
            S(-17, -42),   S(-16, -42),   S(-20, -91),   S(-10, -44),   S(-13, -37),   S(-33, -42),   S(-23, -54),   S(-33, -38),
            S(-18,  16),   S(-22, -30),   S(  7, -19),   S( 14,  15),   S( 14,  58),   S(-38,  59),   S(-12,  14),   S(-32,  14),
            S(-14,  45),   S( 11,  63),   S(  6,  41),   S( 31,  51),   S( 37, 121),   S( 34,  99),   S( 14, 124),   S(-15,  85),
            S( 15,  96),   S( 26,  89),   S(  5,  14),   S( 22,  52),   S( 22,  91),   S( 21, 102),   S(  2,  88),   S( -8, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -71),   S(  1, -91),   S(-10, -58),   S(  4,  12),   S( -8, -39),   S( -1, -21),   S(  3, -45),   S(-26, -48),
            S(-24, -98),   S(-76, -94),   S(-11, -59),   S(-43, -124),  S(-17, -25),   S(-19, -42),   S(-34, -27),   S(-37, -40),
            S(-23, -37),   S(-37, -46),   S(-32, -52),   S( -3, -61),   S(-33, -53),   S(-10, -73),   S(-17, -40),   S(-33,  -4),
            S(-19, -20),   S(-32, -17),   S(  5,  21),   S( -5,  11),   S( 20, -26),   S( -7,  19),   S( -9, -13),   S(-15,  36),
            S( -7,   2),   S(  9,  32),   S( 10,  76),   S(  4,  45),   S( 29,  85),   S( 13,  51),   S( 14, 102),   S( 13,  84),
            S(-14,  67),   S( 12,  53),   S( 26, 121),   S( 13,  46),   S( 24,  77),   S( 16,  87),   S( 38, 124),   S( 29, 115),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0, -33),   S(  2, -55),   S( -8,  -8),   S( -9, -20),   S(-16, -54),   S(-38, -39),   S(-16, -25),   S( -1, -57),
            S(-43, -33),   S(-42, -85),   S(-43, -115),  S( -9, -66),   S(-11, -31),   S(-36, -10),   S(-34,   1),   S(-20, -77),
            S(-42, -27),   S(-47, -33),   S(-38, -31),   S( 19, -52),   S(-19, -32),   S(-22, -45),   S( 13,  17),   S(-22, -16),
            S(-33,  24),   S(-23,  18),   S( 10,  59),   S(  4,  31),   S( 12,  42),   S( -5, -14),   S(  1, -11),   S( -8,  19),
            S( 11, 105),   S( 13,  93),   S(  8, 117),   S( 29,  71),   S( 28, 124),   S( 18,  64),   S( 11,  68),   S(  7,  43),
            S(-12,  77),   S( -4,  39),   S(  7,  93),   S( 25,  76),   S( 25,  97),   S( 24,  82),   S( 24,  55),   S( 24,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-77, -40),   S(-27, -29),   S(-25,  22),   S( 18,   8),   S(-21, -21),   S(-32,  29),   S(  7, -26),   S(-43, -24),
            S( 17, -18),   S( -1,  15),   S(-22, -25),   S( -8,  -3),   S( -7,  11),   S( -1, -17),   S(-26, -70),   S(-20, -28),
            S(-12, -13),   S( 15, -20),   S( -3,  26),   S( 31,  46),   S( -6,  18),   S(  0,  12),   S(-31,  47),   S(-15, -39),
            S(  5,  49),   S( 11,  66),   S( 27,  52),   S( 49,  25),   S(  6,  59),   S( -1,  55),   S( 28, -24),   S( -6,  -4),
            S( 30,  76),   S(-13,  70),   S( 32, 100),   S( 40,  51),   S( 67,  49),   S( 25,  16),   S( 21,   7),   S( 12,  17),
            S(114,  -5),   S(-17,  77),   S( 85,  73),   S( 72,  25),   S( 50,  61),   S( -5,  74),   S( 55, -12),   S(  3,  -6),
            S( 50,  -6),   S( -4, -10),   S( 71,  24),   S( 74,  80),   S( 70,  34),   S( 19,  40),   S(-16,  49),   S(-63,  -1),
            S(-104, -152), S(  3,  14),   S( 18,  -1),   S( 25,  53),   S(  3,  66),   S( 30,  43),   S(-21,  19),   S(  4,  16),

            /* knights: bucket 1 */
            S( 44,   5),   S(-56,   2),   S(-34,  33),   S(-52,  45),   S(-37,  66),   S(-19, -18),   S(-49,   4),   S(-28,   6),
            S(-43,  34),   S(-39,  49),   S(-32,  27),   S(-13,  25),   S(-29,  32),   S(-15,  42),   S(-31,  -2),   S(-29, -14),
            S(-40,  32),   S( -5,   8),   S(-26,  30),   S(-22,  71),   S(-20,  42),   S(-25,  31),   S(-52,  39),   S(-12,  23),
            S(-25,  84),   S( -4,  55),   S( -3,  51),   S(-33,  70),   S(-18,  66),   S(-23,  72),   S(-17,  42),   S(-31,  51),
            S( 21,   4),   S(-13,  26),   S(  6,  74),   S(-17,  67),   S( 27,  45),   S(  8,  48),   S(-11,  37),   S(  2,  59),
            S( 37,  39),   S( 32,  14),   S( 77,  40),   S(108,  34),   S( 59,  35),   S(-47,  91),   S( 50,  27),   S( 15,  47),
            S( 15,  -4),   S( 10,  -1),   S(  6, -32),   S( 18,  36),   S(  4,  26),   S( 55,   2),   S( 14,  49),   S( 30,  32),
            S(-185, -32),  S( 18,  -5),   S(-13, -50),   S(-14,   1),   S( -7,  10),   S( 45,  33),   S( 15,  67),   S(-65,  28),

            /* knights: bucket 2 */
            S(-84,  31),   S(-48,  44),   S(-42,  26),   S(-36,  52),   S(-24,  30),   S(-57,  18),   S(-35,  32),   S(-26,  12),
            S(-41,  51),   S( 11,  36),   S(-37,  45),   S(-20,  37),   S(-36,  43),   S(-27,  29),   S(-14,  39),   S(-36,  29),
            S(-50,  81),   S(-30,  38),   S(-36,  43),   S(-35,  76),   S(-26,  66),   S(-34,  31),   S(-34,  35),   S(-17,  25),
            S(-22,  64),   S(-14,  56),   S(-32,  97),   S(-37, 108),   S(-42,  90),   S(-26,  62),   S( -5,  48),   S(-20,  57),
            S(-32,  81),   S(-34,  79),   S( -6,  76),   S(-12,  78),   S(-39,  93),   S(  0,  77),   S(-46,  90),   S( 12,  29),
            S(-36,  65),   S(-25,  73),   S(-34, 104),   S( 30,  42),   S( 22,  66),   S(113,  22),   S( 47,  59),   S(-17,  14),
            S( -2,  52),   S(-52,  84),   S( 37,  19),   S( 34,   0),   S(-22,  43),   S( 42, -36),   S( 49,  40),   S( 34,   3),
            S(-39,  31),   S( 34,  68),   S(-24,  57),   S(-21, -25),   S(-28, -15),   S(-40, -41),   S(  7, -25),   S(-174, -52),

            /* knights: bucket 3 */
            S(-85,  57),   S(-23, -11),   S(  1,  21),   S(-18,  32),   S( -6,  23),   S(-14,  18),   S(-26,  11),   S(-56, -39),
            S(-25,  19),   S( -1,  34),   S( -1,  33),   S( -8,  33),   S(-10,  33),   S( 15,  18),   S( 13,   4),   S( 23,  -9),
            S(-22,  37),   S(-20,  40),   S( -8,  54),   S(-11,  82),   S(  3,  70),   S(-13,  55),   S(  3,  40),   S( 10,   6),
            S( -5,  29),   S(  4,  54),   S(  5,  83),   S(-10, 105),   S(  4, 101),   S(  2,  98),   S(  9,  85),   S( -1,  76),
            S(-21,  72),   S( -2,  67),   S( 10,  99),   S( 19, 112),   S(  3,  92),   S( 12, 114),   S(-33, 116),   S( 40, 115),
            S( -4,  42),   S( 12,  77),   S( 27,  82),   S(  5, 100),   S( 49, 104),   S(100, 125),   S( 49, 125),   S(  0, 141),
            S(-56,  87),   S(-35,  86),   S(-40, 101),   S( 20,  90),   S( 31,  87),   S( 73,  66),   S(-30,  53),   S( 97,  -2),
            S(-169,  87),  S(-14, 103),   S(-17,  82),   S( 44,  78),   S( 52,  97),   S(-66,  73),   S(  9, -30),   S(-55, -110),

            /* knights: bucket 4 */
            S(  0,  10),   S(-29,  12),   S(-46,  15),   S(-28, -29),   S(-15, -19),   S(-11, -12),   S( 15, -30),   S(-13,   4),
            S( 22,   9),   S(  7, -41),   S( -9,  16),   S(-24,   0),   S(-20,  -8),   S( 13, -23),   S( 41,  35),   S(-45,  26),
            S(-18, -19),   S(  2,   5),   S( 57,  19),   S(101, -19),   S( 48,  11),   S( 37, -26),   S(  0, -18),   S(-13, -26),
            S(-33, -26),   S( -3,   1),   S( 28,  -2),   S( 74,  14),   S( 12,  16),   S( -6,  41),   S(-34,  17),   S(-25, -18),
            S( 12, -32),   S( 10,  -8),   S( 71,  17),   S( 47,  44),   S( 75,  -4),   S(  9,  45),   S(  3,   3),   S(-58,  47),
            S( -5, -19),   S( -9,  -7),   S( 44,  -1),   S( 84,  23),   S( 23,  38),   S( 32,  37),   S(-12,   7),   S(  7,  11),
            S(-13,  -9),   S(-24, -21),   S(  2,   1),   S( 14,  19),   S( 16,  11),   S(  7,  55),   S( -8,  23),   S(-41, -10),
            S( -1,   8),   S(-17, -44),   S(-12, -50),   S(  7,  19),   S( 10,   5),   S(-18, -17),   S( -6,  16),   S( -5, -10),

            /* knights: bucket 5 */
            S( 14,  46),   S( 26,  15),   S(-26,  24),   S( 13,   3),   S( 14,   9),   S( 23,  15),   S(-22,  10),   S(  3,  25),
            S( 21,  26),   S( 39,  28),   S( 51,  -8),   S(  2,   3),   S( 45, -14),   S(-29,  17),   S(-37,  52),   S(-84,  10),
            S(-39,  24),   S(-14,   7),   S( 15,  17),   S( 25,  14),   S( 22,  28),   S(-16,  21),   S(  4,   3),   S(-46,  15),
            S( 20,  17),   S( 34, -18),   S( 50,  10),   S( 74,   2),   S( 94,  26),   S( 95,  -5),   S( 12,  12),   S(  6,  10),
            S( 63,  -3),   S( 48,  -8),   S( 86,  -9),   S( 98,  10),   S(119, -26),   S( 62,   6),   S( -5,  -1),   S( -6,  29),
            S( -3,  -7),   S( 53, -22),   S(  3, -41),   S( 20,  43),   S( 62,   2),   S( 71,  -4),   S(-17,  13),   S( 22,  30),
            S( 17,  13),   S(-16, -33),   S( -5, -56),   S( -7,  -4),   S(-17, -49),   S( 19,   9),   S( 12,  51),   S( -1,  42),
            S(-43, -60),   S(-34, -57),   S( 22, -19),   S( -8, -13),   S( -8, -17),   S(  4,  41),   S( 13,  38),   S(-15,  30),

            /* knights: bucket 6 */
            S( -6, -18),   S(-28,  11),   S( -8, -12),   S( -2,  33),   S(-39,  41),   S( 19,  22),   S(-53,  58),   S(-26,   8),
            S(  4,  10),   S(-16,  56),   S( -9, -10),   S( 50,   3),   S( 66,  -4),   S( 15,  13),   S( 43,  28),   S(-20,  45),
            S(  0,  14),   S( 71,  -3),   S( 47,  10),   S( 60,  25),   S( 44,  29),   S(-37,  35),   S( 43,  15),   S(-23,  20),
            S( 34,  25),   S( 91,  -4),   S( 60,  18),   S( 96,   5),   S(107,  -1),   S(117,   6),   S( 42,  10),   S(-12,  41),
            S( -8,  35),   S( 75,  -4),   S(155,   1),   S(141,   1),   S(116, -17),   S( 81,  10),   S(194, -33),   S( 45,  14),
            S(  7,  27),   S( 37,  -9),   S( 28,  22),   S( 39,   4),   S( 70, -19),   S( 64,  -7),   S( 30,  -3),   S( 25,  -3),
            S( 25,  29),   S( 14,  33),   S( 45,  29),   S( -2, -24),   S( 31,  22),   S( 38, -47),   S( -1,  19),   S( 14,  50),
            S(  0,  14),   S( -7,  15),   S(  3,  29),   S( -2,   6),   S( 14,  -4),   S(  5, -26),   S(  1,  24),   S(-18, -15),

            /* knights: bucket 7 */
            S( -8, -17),   S(  4, -31),   S( 37, -11),   S(-33,  17),   S(-13,  -4),   S(-41,  32),   S( 16,   3),   S( -2,  19),
            S(-15, -32),   S(  4, -21),   S(-31,   6),   S( -2,   2),   S( 46, -17),   S( 29,  10),   S( -3,   3),   S(-80,  37),
            S( -2, -40),   S(-16, -16),   S( 38, -22),   S( 46,  12),   S( 88,   1),   S( 59,   6),   S( 28,  15),   S( 42,  15),
            S(-20,  21),   S( 55, -14),   S( 66,  -9),   S( 93,   7),   S(114, -13),   S(109,  25),   S( 99,  -1),   S(113,  10),
            S( -6,  -2),   S( 32,   1),   S( 22,  26),   S( 99,   2),   S(155, -24),   S(168, -21),   S(176,   2),   S( 63,  13),
            S(  0,  14),   S( 37,   3),   S( 38,  22),   S( 77,   3),   S(136,   3),   S( 95,  -3),   S( 36,  -6),   S(-15, -69),
            S(-10,  24),   S(-21,  -4),   S(  7,  19),   S( 54,  21),   S( 86,  13),   S( 45,  19),   S(-22, -55),   S(-28, -35),
            S(-50, -53),   S(-20,   2),   S(-11,  41),   S(  6,  10),   S( -5,  14),   S( 29,  28),   S( 14, -20),   S(-10, -35),

            /* knights: bucket 8 */
            S( -2,  15),   S( 10,   9),   S(  5,  13),   S(-15, -55),   S( -4,  -6),   S( -2, -35),   S( 12,  12),   S(  1, -13),
            S( -9, -28),   S(-11, -36),   S( -7, -66),   S(-15, -17),   S( -8,   4),   S(  2,  -4),   S( -5, -15),   S( -9, -34),
            S( -8, -47),   S(-11, -11),   S( -5, -44),   S(  4, -21),   S( 13, -25),   S( 43,  16),   S(-13, -50),   S( -2, -24),
            S(-14, -64),   S( -7, -39),   S( 11,  14),   S( 24,  -8),   S(  3, -29),   S( -1, -12),   S( -8, -27),   S(-22, -55),
            S( -6, -29),   S( -7, -51),   S( 13,  -5),   S( 15, -38),   S(  8, -15),   S( 12, -34),   S( -6, -21),   S( -3, -19),
            S(  2,   1),   S( 13, -41),   S(  5, -19),   S(  3, -49),   S( 14,  13),   S(-10, -24),   S(-11, -25),   S( -7, -22),
            S(  2,   6),   S(-11, -78),   S( -9, -31),   S(  1,  -8),   S(  1,  -9),   S( 18,  -6),   S( -5,  -8),   S( -7, -12),
            S( -1,  -2),   S( -4,  -7),   S( -9, -24),   S( -1, -22),   S(  3,  13),   S( -7, -15),   S( -4, -15),   S( -7, -15),

            /* knights: bucket 9 */
            S(-13, -70),   S( -4, -19),   S(-12, -70),   S( 10, -65),   S(-17, -54),   S(-10, -23),   S( -3, -11),   S(  0,   0),
            S(-15, -50),   S(-19, -55),   S(-13, -123),  S(  2, -56),   S( -1, -42),   S(-17, -82),   S( -4, -37),   S(-11, -29),
            S( -6, -40),   S(-12, -17),   S(-12, -44),   S(  0, -78),   S( -9,  -5),   S( 29, -21),   S( -2,  -5),   S(  0, -28),
            S(-18, -56),   S(-10, -64),   S( -3, -40),   S( -4, -55),   S(  9, -47),   S( 35, -33),   S(-12, -60),   S(  7, -22),
            S(  7,   0),   S( -3, -31),   S( 22, -32),   S( -8, -54),   S( -9, -71),   S(  7, -19),   S( -3, -40),   S( -1, -31),
            S(-20, -62),   S( -9, -54),   S(-11, -34),   S( 14, -48),   S( 17, -15),   S( 11, -30),   S(-14, -62),   S(-11,  -4),
            S(-11, -16),   S(  3,   2),   S( -1, -15),   S(-27, -44),   S(  8, -16),   S(  3,   7),   S(-14, -20),   S(-10, -25),
            S(  2,   6),   S(  4,  -6),   S( -3,  10),   S(-13, -55),   S( -8, -18),   S( -7, -29),   S(  5,  23),   S( -4, -16),

            /* knights: bucket 10 */
            S( -5, -37),   S(  0,   4),   S( -8, -39),   S( -9, -15),   S(  5, -73),   S(  5, -52),   S(  6,  31),   S(  2,  13),
            S( -6, -54),   S(  8, -40),   S(  6, -44),   S(  7, -77),   S( 12, -36),   S(-17, -78),   S(-13,  -1),   S( 10,  32),
            S(  8, -29),   S(  3, -45),   S( -3, -42),   S( 39, -62),   S(-17, -54),   S( 11, -50),   S( -2, -39),   S( -3, -16),
            S( -3, -48),   S(  1, -65),   S( 20, -47),   S(  7, -53),   S(  8, -37),   S(  5, -22),   S(-17, -91),   S( -7, -59),
            S( -8, -47),   S(  3, -46),   S( 18, -48),   S( 12, -26),   S( 12, -21),   S( -1, -54),   S( -2, -23),   S( -4, -38),
            S(  3,   2),   S( -7, -35),   S(  9, -13),   S( -4, -21),   S(-10, -25),   S(-19, -54),   S( -8, -24),   S(-13, -32),
            S(  4,  -6),   S( -5,  -4),   S( -2, -30),   S(  7, -60),   S(  1, -17),   S(-26, -67),   S(-13, -37),   S(-14, -35),
            S( -2,  -6),   S( -3, -19),   S( -7, -19),   S( -8, -27),   S( -3, -32),   S( -1, -28),   S(  5,  21),   S( -4,   0),

            /* knights: bucket 11 */
            S( -5, -27),   S(-10, -21),   S(-10, -45),   S(  8, -18),   S(-40, -89),   S(-10, -16),   S(-23, -19),   S( 11,  38),
            S(  4, -12),   S(-21, -40),   S(-10, -73),   S( 51, -24),   S( 11, -18),   S( -6, -73),   S(-18, -32),   S(-12, -25),
            S(-13, -45),   S(-28, -61),   S( 19, -22),   S( 18, -12),   S( -1, -14),   S( 20, -27),   S(-15, -45),   S(-13, -39),
            S(-22, -50),   S( 26,  -8),   S( 16, -36),   S( 40, -13),   S( 50, -31),   S(  8, -33),   S(  4, -22),   S( -1,  -9),
            S(-12, -34),   S( -8, -82),   S( 10, -15),   S( 22, -23),   S( 36,  19),   S(  8, -10),   S( -8, -117),  S(-18, -42),
            S(-14, -42),   S(-16, -67),   S( 10,  -2),   S( 15, -25),   S( 10, -13),   S( -7, -51),   S( -3, -41),   S(  0,   3),
            S(  5,  18),   S( -8,  -7),   S( -7,  -9),   S(  8, -21),   S(  3, -24),   S( 14, -58),   S( -4, -26),   S( -4,  -3),
            S( -4, -21),   S(  5,  18),   S( -4, -14),   S( -3,  17),   S(  1, -11),   S(  3, -22),   S(  6,  15),   S( -1,  -4),

            /* knights: bucket 12 */
            S(-18, -60),   S(  1, -37),   S( -8, -42),   S( -7, -14),   S( -5,   4),   S(  2,  -6),   S(  2,   9),   S( -4, -12),
            S( -5, -32),   S(  0,  -7),   S(-14, -26),   S( -4, -20),   S(  1, -50),   S(  4, -10),   S(  6,   4),   S( -2, -16),
            S( -1, -16),   S( -7, -35),   S( -4, -26),   S( -1, -67),   S( -5, -33),   S(  3,  -7),   S( -4, -22),   S( -5,  -4),
            S(  4,   8),   S( -5, -76),   S(  7,   6),   S( 11, -13),   S( -2, -53),   S( 10,  13),   S( -2,  -6),   S(  0,  10),
            S(  4,  10),   S( -5, -41),   S( -4, -42),   S( 10,  -9),   S(  3,  14),   S( -2, -18),   S( -1, -16),   S(-12, -24),
            S( -6,  -8),   S(  0, -15),   S( -3, -25),   S(  9, -21),   S(-11, -33),   S( -8, -14),   S( 10,   8),   S(  3,   4),
            S( -4, -19),   S( -9, -23),   S(-10, -10),   S( -5, -11),   S(  1,   3),   S(  0,  -9),   S( -7, -26),   S( -5, -15),
            S(  0,   2),   S(  3,  18),   S(  2,   4),   S(  3,  -1),   S(  1,  -6),   S(  3,  23),   S( -2, -10),   S( -2,  -6),

            /* knights: bucket 13 */
            S( -3, -12),   S( -5, -15),   S( -4, -37),   S(  1, -13),   S( -2,  -6),   S( -7, -21),   S( -4, -12),   S(  4,  18),
            S( -5, -28),   S( -2, -14),   S(  3,  12),   S(-14, -48),   S( -8, -43),   S(  2,   7),   S( -6,  -6),   S( -2, -19),
            S(  3,   1),   S(  5,   6),   S(  1, -16),   S(  8,  -8),   S( 14,  52),   S( -5,  -5),   S(  2, -11),   S( -1, -12),
            S(  9,  14),   S(  4,  -5),   S( -1, -12),   S( 13,  10),   S(  2, -50),   S(  7, -25),   S(  6, -15),   S( 15,  26),
            S(  1,  31),   S(  4, -29),   S(  2, -58),   S( -5, -57),   S(-10, -35),   S( 11,  16),   S( -6, -39),   S( -9, -34),
            S( -1,   0),   S(  3,  11),   S( -5,  -9),   S( 10, -20),   S(-15, -33),   S( -6, -50),   S(  8,  15),   S( -4, -13),
            S(  1,   3),   S(  7,  10),   S( -6, -20),   S(  9,  19),   S(  0,   4),   S(  0,  -7),   S( -2, -12),   S(  3,  11),
            S(  1,   4),   S(  1,   5),   S( -3, -12),   S(  0,  -2),   S( -2,  -5),   S( -3, -16),   S( -1,  -5),   S(  1,   2),

            /* knights: bucket 14 */
            S(  2,   4),   S(  0,  -1),   S(  3,   3),   S( -7, -20),   S( -5, -18),   S( -4,  -4),   S(  7,  12),   S( -2,  -9),
            S( -4, -20),   S(-13, -50),   S( -1,  -1),   S( -4, -31),   S(  2, -37),   S( -4, -19),   S(  0, -11),   S(  2,  24),
            S( -1, -22),   S( -2, -33),   S(  9,  -7),   S( -7, -58),   S(  0,  -7),   S(  9,   1),   S(  4, -12),   S(  5,  27),
            S( -5, -28),   S(-11, -65),   S(-17, -65),   S(  0,  -7),   S( 16,   5),   S( -9, -37),   S(-11, -34),   S( -2,  14),
            S(  0,   6),   S(-11, -56),   S(-11, -35),   S( -6, -12),   S(  8,  46),   S( -5, -17),   S( -5, -23),   S( -1, -13),
            S( -2,  -2),   S(  4,  14),   S(  3,  38),   S( -2,  -3),   S( -8, -40),   S( -3,  -2),   S( -8, -22),   S( -8, -29),
            S( -4, -15),   S(-10, -34),   S(  3,  -7),   S(  7,  40),   S( 11,  21),   S(-13, -32),   S(  0,  -3),   S(  0,  -6),
            S(  2,   2),   S( -1,  -5),   S( -2, -12),   S(  4,   8),   S( -2,  -6),   S( -1,  -3),   S(  1,   4),   S(  0,   2),

            /* knights: bucket 15 */
            S( -4, -16),   S(  0,  -5),   S(  3,  26),   S( -7, -12),   S( -1, -17),   S( -5, -17),   S( -4, -44),   S( -1, -14),
            S( -2,  -1),   S(  5,   4),   S(  1, -18),   S( 11,  33),   S( -5, -49),   S( -4, -18),   S( -2, -11),   S(  1,  -6),
            S(  0,  -3),   S( -2, -26),   S( -4, -29),   S( 15,   6),   S(-25, -105),  S(  1, -10),   S( -3, -30),   S(  2,   2),
            S(  1,  -4),   S( -2,   4),   S( -3, -12),   S( -3, -32),   S( -4, -46),   S(  4, -20),   S(  3,  -5),   S( -4, -14),
            S( -2, -15),   S( 12,  22),   S(  0,   2),   S( -9, -19),   S( 10, -12),   S( 15,  20),   S(  6,  -2),   S(  2,  11),
            S(  1,   4),   S( -3,   0),   S(  3,  24),   S( -2, -37),   S( -6, -23),   S(  2,  43),   S(  1,  17),   S(  6,  17),
            S( -4, -14),   S(  0,   6),   S(  6,  23),   S( 14,  31),   S(  7,  20),   S(  2,   7),   S( -1,   6),   S(  3,   8),
            S(  0,   3),   S(  0, -10),   S(  1,  -1),   S( -3,  -7),   S(  4,  19),   S( -3, -10),   S(  1,   5),   S(  0,   3),

            /* bishops: bucket 0 */
            S( 34, -15),   S(-38,  43),   S(-12,  -5),   S(-21,  -9),   S( 16, -19),   S( 12, -10),   S( 77, -77),   S( 42,  10),
            S(-44,  19),   S(  4, -10),   S(-14,  19),   S( 11, -11),   S( 19, -12),   S( 73, -39),   S( 42,  27),   S( 27,  -3),
            S( -6,  35),   S(  5,  23),   S( 15,  13),   S( 21,  -8),   S( 47, -26),   S( 52,   6),   S( 56, -14),   S( 29,  -5),
            S(  5, -38),   S( 46, -64),   S( 20, -13),   S( 88, -45),   S( 71,  16),   S( 52,  16),   S( 19, -18),   S( 16,  15),
            S( 23,  10),   S( 50, -33),   S( 74, -14),   S(102,  -5),   S(117, -16),   S( 30,   9),   S( 54,  13),   S( 17, -28),
            S( 29,  22),   S( 95, -24),   S( 78,  12),   S( 22,  22),   S( 25,   7),   S( 56,  -2),   S( 14,  25),   S( -2,  -4),
            S(-54, -107),  S( 62, -44),   S( 79,  25),   S( 13,  -8),   S( 28, -15),   S( 19,  -6),   S( -9,  42),   S(-43,  19),
            S( -8, -21),   S(-21, -22),   S(-12, -49),   S( -1, -30),   S(-28, -22),   S(-17,   7),   S( -7,  22),   S(-35, -47),

            /* bishops: bucket 1 */
            S(-33,  17),   S( 52, -28),   S(-19,  23),   S( 14,   6),   S(-20,  12),   S( 23,   7),   S(  8,  25),   S( 65,  -9),
            S( 33, -19),   S( -2, -13),   S(  6,  -6),   S(-14,   2),   S( 45, -17),   S(  5,  -9),   S( 66, -10),   S(  2,  -8),
            S(-21,   5),   S( 27,  -5),   S( -8,  -1),   S( 30,  -5),   S(  9, -10),   S( 48,  -6),   S(  1,  -6),   S( 77,  -9),
            S( 31, -19),   S( 36, -16),   S( 34,   1),   S( 21,  -3),   S( 68, -12),   S(  6,   2),   S( 94, -41),   S( 20, -24),
            S( 20, -24),   S( 81, -21),   S( 37,  -8),   S(114, -20),   S( 56, -20),   S(109, -25),   S( 11,  -7),   S( 36,  11),
            S( 75, -25),   S(  9,  -9),   S( 59,  -5),   S( 85, -52),   S(132, -48),   S(-11,  -8),   S(  9,  19),   S(-18,  -8),
            S( -9, -61),   S( 11, -19),   S(-10, -48),   S(  9,  -4),   S( 21,   2),   S(  1,   7),   S( 19,  -9),   S(-21,  12),
            S(  6, -16),   S(-22,   6),   S(  4, -47),   S(-60,   2),   S( 16, -34),   S( 40, -13),   S(-15, -28),   S(-32, -41),

            /* bishops: bucket 2 */
            S( 16,  -2),   S( -3, -11),   S(  7,   7),   S(-13,  20),   S( 21,  11),   S( -9,  21),   S( 33, -13),   S( -4,  29),
            S( 34, -17),   S( 21, -13),   S( -3,   3),   S( 15,   6),   S(  2,  16),   S(  4,   5),   S( 14,  -3),   S( 23, -44),
            S( 43,   8),   S( 18,  -7),   S( 13,  14),   S(  9,  -3),   S( 12,  24),   S(  5, -11),   S(  0,  -9),   S(  6,  -2),
            S( 14, -10),   S( 48, -15),   S( 10,   6),   S( 37,  19),   S( 13,  12),   S(-10,  28),   S(-23,  13),   S(  9,  -1),
            S( 15,   4),   S( 11,  14),   S( 71, -10),   S( 33,  10),   S( 18,  28),   S(  6,  20),   S(  6,  29),   S( 31,   3),
            S(-17,  22),   S( -9,  37),   S( 20, -15),   S(103, -26),   S(100, -38),   S(114,  12),   S( 37,  14),   S( 13, -34),
            S(-12,  28),   S( 19, -10),   S( -3,  17),   S( 38,  -2),   S(-93, -35),   S(-15, -19),   S(-20,  36),   S(-21, -19),
            S(-57,  12),   S(  1,   5),   S(-16,   5),   S(-10,  22),   S(  0, -37),   S(-38, -10),   S( -8,  -6),   S(-41, -39),

            /* bishops: bucket 3 */
            S( 30,  18),   S( 44, -17),   S( 16, -15),   S( 19,  13),   S( 14,  22),   S(  3,  40),   S(  2,  44),   S( -3,  16),
            S( 35,  12),   S( 31,  -3),   S( 18,  15),   S( 21,   7),   S( 28,  14),   S( 25,  11),   S( 20,   9),   S( 36, -20),
            S(  9,   7),   S( 28,  41),   S( 24,  36),   S( 29,  24),   S( 28,  30),   S( 25,  15),   S( 17,   4),   S( 24,  16),
            S(-12,   8),   S( 19,  16),   S( 31,  42),   S( 49,  31),   S( 51,  20),   S( 21,  15),   S( 28,  -7),   S( 33, -24),
            S( 28,   5),   S( 14,  43),   S( 12,  43),   S( 60,  51),   S( 51,  42),   S( 46,  20),   S( 13,  31),   S( -8,  39),
            S(  6,  24),   S( 42,  38),   S( 20,  23),   S( 31,  30),   S( 52,  22),   S( 44,  69),   S( 55,  60),   S( 22,  86),
            S(-14,  56),   S( 30,  46),   S( 35,   8),   S(  5,  33),   S(  9,  42),   S( 28,  77),   S(-24, -20),   S( 24, -29),
            S(-79,  63),   S(-43,  60),   S(-49,  40),   S(-56,  60),   S(-28,  37),   S(-130,  76),  S(  8,   2),   S( -6,  10),

            /* bishops: bucket 4 */
            S(-14,  55),   S(-37, -13),   S(-52,  32),   S(-32, -18),   S(-25, -24),   S(-12, -30),   S(-10, -26),   S(-22, -31),
            S(-10,  57),   S(-48,  11),   S( 55, -25),   S( -3, -22),   S(-36,  -2),   S( -8, -60),   S(-18,  -7),   S(-42, -21),
            S( -6,  32),   S(-32,  -2),   S( 28, -13),   S( 12, -26),   S(-18,  -6),   S(-14, -15),   S(-40, -34),   S(-42, -23),
            S( 33, -23),   S( 51, -14),   S( 38, -34),   S(  9,  -4),   S(  3,   6),   S( 43, -22),   S(-35,  -3),   S( -2, -40),
            S( 17, -13),   S(  7, -57),   S( 39, -50),   S( 58, -29),   S( -7,  -5),   S(  2, -14),   S(  2,  27),   S(-60,  15),
            S(-67, -95),   S(-24, -67),   S( 45, -45),   S( 38, -34),   S(-16,  14),   S( 34,  19),   S( 12,  17),   S(-23,  20),
            S(  3,  -3),   S( 22,  -3),   S( -6, -70),   S( -6, -19),   S(-31, -12),   S(  5,   2),   S(-36,   4),   S( 19,  22),
            S( -3, -31),   S(-14, -36),   S( -1, -18),   S( -1, -50),   S( -8, -30),   S(-14,   0),   S( -3,  36),   S(  9,  35),

            /* bishops: bucket 5 */
            S(-22,  48),   S( -7,   8),   S(-47,  25),   S(-29,  14),   S( -3,  -2),   S(-22,  -6),   S(-34,  19),   S(-55,   5),
            S(-24,   4),   S(-24,  14),   S( 41, -20),   S(  0,  -1),   S(-29,   3),   S(-30,   2),   S(-41,   3),   S( -1,  -8),
            S( 28,  16),   S(-40,   1),   S( 17,  -4),   S(-18,   2),   S( -3,   8),   S(-70,  13),   S( 46, -28),   S(-46,  27),
            S( 21,  -3),   S(-40,   9),   S( 37, -19),   S( 71,  -9),   S(-33,  12),   S( 47, -18),   S(-44,   4),   S( -1,   3),
            S( 25, -27),   S( 17, -15),   S( 13, -24),   S(  1, -38),   S( 19, -27),   S( 12, -20),   S( 47,  -4),   S(-38,   2),
            S( -9, -19),   S(-17,  -5),   S( 64, -50),   S(-16, -21),   S(-17, -15),   S( 11, -12),   S( -4,  -8),   S(-12,  12),
            S(-45, -27),   S( -2, -25),   S(-30, -15),   S(  6,   6),   S(  6, -21),   S( -5,   5),   S( 11,  19),   S(-43,  17),
            S(-24, -20),   S(-22, -36),   S(-30, -25),   S(  1, -16),   S(-43,  18),   S( -3,   9),   S(-40,  -8),   S(  0,  47),

            /* bishops: bucket 6 */
            S(-65,  12),   S(-41,   9),   S(-29,   4),   S( -3,  14),   S(-17,   5),   S(-23,  19),   S(-54,  37),   S(-58,  30),
            S(-17,  -1),   S( 10, -37),   S(-18,  12),   S(-22,   8),   S(-42,  21),   S(-26,   1),   S(-63,  37),   S(-40,  16),
            S( -5,   5),   S(-43,  13),   S( 12, -14),   S(-18,   7),   S(  7,  14),   S(-20,   7),   S(-49,   1),   S(-29,  41),
            S(-16,   9),   S(-36,  -8),   S( 10,  -6),   S( 50,  -5),   S( 48,  -1),   S( 45, -10),   S( -6, -10),   S(-22,  12),
            S(-78,  19),   S( -7,   0),   S( 21,  -6),   S( 66, -19),   S( 30, -44),   S( -1, -29),   S( 44, -23),   S(-37,  -7),
            S(-32,  26),   S( 15,   5),   S(-32,   2),   S(-17,   5),   S( 12, -16),   S( -8,  -9),   S( 31,   1),   S(-20, -14),
            S(  5,  16),   S(-68,  45),   S( -5,  -3),   S(  0, -21),   S(-38,  -5),   S(-13,  -5),   S( -4, -32),   S(-51,   8),
            S(-27,  18),   S(-33,  -3),   S(-33,  12),   S(-37,  -1),   S( -8,   3),   S( 24, -29),   S(-28,  -1),   S(-14,  -9),

            /* bishops: bucket 7 */
            S(-21, -29),   S(-90,   9),   S(-33, -33),   S(-17,  -3),   S(-24, -20),   S(-49,   0),   S(-77, -11),   S(-38,  -3),
            S( -3, -33),   S( 11, -46),   S(  1, -25),   S(-18, -29),   S(-35,   1),   S(-29, -12),   S(-56,  -9),   S(-27,  36),
            S(-47, -17),   S(-19,  -8),   S( -3, -18),   S( -3, -10),   S( 20, -30),   S(-13, -10),   S(-89,  15),   S(-91,  74),
            S(-20,  -5),   S(-79,   9),   S(-28,   4),   S( -4, -10),   S( 72,  -7),   S(-24, -17),   S( 48, -41),   S(-40, -11),
            S(-56,  12),   S( 10, -12),   S( -3, -15),   S( 14, -14),   S( 45, -20),   S( 64, -31),   S(-34, -24),   S(-54,  -4),
            S(-87,   1),   S(-15,  15),   S( 26, -13),   S(-67,  18),   S(-22,   1),   S( 72, -34),   S( 25, -32),   S(-50, -73),
            S(-39,  -9),   S(-55,  36),   S(-50,  18),   S( 27,  -5),   S( 12, -37),   S(-13, -42),   S( 47,   0),   S(-13,  -3),
            S(-36, -21),   S(-52,   6),   S(-22,  18),   S(-24, -11),   S(  4, -26),   S( -1, -41),   S( 34, -52),   S( -6, -34),

            /* bishops: bucket 8 */
            S( 39, 177),   S(  3,  12),   S(-27, -18),   S( -9,  37),   S(  0,   6),   S( -9, -38),   S(-13, -37),   S(  1,  -8),
            S( -5,  30),   S( 18,  71),   S( 26,  28),   S( 16,  -4),   S(-17,   3),   S(  0,  28),   S(-34, -42),   S(  2, -10),
            S(-18, -40),   S(-13, -73),   S( 13,  44),   S(  5, -11),   S( 14,  25),   S( 19,  -2),   S(-23, -23),   S(-41, -85),
            S( -3,  -7),   S( 15,  48),   S(  3,  30),   S( -5,  42),   S( 34,  29),   S(  9,  14),   S(  0,   9),   S(-10, -59),
            S( 19,  90),   S( 10,  77),   S( -3,  19),   S(  9,  10),   S(  3,  55),   S(-21,  32),   S( 16, -14),   S(  6,  11),
            S( -3, -10),   S( -1,  33),   S(  1,  38),   S(  2,  11),   S(  6,  29),   S(  3,  20),   S(  6,  31),   S( -1,  25),
            S(  9,  38),   S(-19, -51),   S( 14,  27),   S( -7,  42),   S( -4,  17),   S(  3,  43),   S( 15,  65),   S(-25,   0),
            S( -2,  -3),   S( -1, -11),   S( -4,  -1),   S(  1,  18),   S(  9,  31),   S( 13,  51),   S(  1,  26),   S( 19, 120),

            /* bishops: bucket 9 */
            S(  5,  54),   S( -8,  54),   S(-18,  27),   S(-41,  -2),   S(-35, -12),   S(-13, -24),   S(-11, -22),   S(  0,  23),
            S(  4,  42),   S(-15,  32),   S(  8,  44),   S(-31, -23),   S(  0, -21),   S(-12, -27),   S(-26,  -8),   S(-33, -96),
            S( -3, -17),   S(  1,   1),   S(  4, -12),   S(  5,  -6),   S(  7,  15),   S(-12, -18),   S( -2, -35),   S(-17, -14),
            S(-12,  42),   S(  8,  15),   S(-15,   7),   S(  6,  30),   S(-34,  22),   S(-17,  10),   S(  0,  15),   S( 12,   9),
            S(  3,  32),   S(-15,  42),   S( 17,  12),   S(-28,  -9),   S(  5,  33),   S(-12,  12),   S( -8,  10),   S( -9, -34),
            S( -1,  37),   S( -9,   7),   S(-19,   8),   S( -4,   4),   S(-32,  16),   S(-18,  47),   S( -9,  17),   S(  3,  38),
            S( -7,  17),   S(-18,  16),   S( -5,  10),   S(-22,  27),   S(-11,   6),   S( -2,  29),   S( -4,  48),   S(-10,  45),
            S(  6,  51),   S(-11, -14),   S(-10,   8),   S( -2,  21),   S(  0,  25),   S(-17,  14),   S(-11,  45),   S( 15,  83),

            /* bishops: bucket 10 */
            S( -2,  28),   S( -3,  56),   S(-14, -25),   S(-27, -30),   S(-45, -15),   S(-49, -43),   S(-18,  32),   S( -7,  17),
            S(-13,  -3),   S(  0,  -7),   S(-16, -13),   S(-47, -41),   S(-25,  -6),   S(-44, -33),   S(-37, -39),   S( -8,   9),
            S(  1, -10),   S(-29, -39),   S( -7, -10),   S(-28,  -2),   S(-13, -45),   S( -1, -30),   S(-22,   6),   S( -9, -58),
            S(-17, -22),   S(-11, -27),   S(-27, -29),   S(-15,  -8),   S( -9,  -9),   S(  8,   5),   S(  3,  31),   S(-21,  -7),
            S(-18,   0),   S(-42,  23),   S( -5, -12),   S( -8,   0),   S( -1,  -3),   S(  2,  14),   S(-15,  12),   S(  9,  22),
            S(-18,  27),   S(-12,  19),   S(-36,  23),   S(-29,  -2),   S(-13, -15),   S( -5,  20),   S(-21,  13),   S(-15,  13),
            S( -4,  22),   S(-16,  14),   S(-11,  18),   S( -7,  30),   S( -5,  10),   S(-26, -37),   S(-13,   1),   S( 14,  68),
            S( -8,  52),   S( -8,  43),   S( -8,  49),   S( -7,   4),   S(-16,  28),   S( -4,  21),   S(  6,  20),   S(  4,  10),

            /* bishops: bucket 11 */
            S( 16,  12),   S(-15,  -4),   S(-32,   6),   S(-14, -19),   S(-37, -28),   S( 17,  -5),   S( -9,  12),   S(-18, 119),
            S(  0,  16),   S(-12, -17),   S(-10,   3),   S(  4, -31),   S(-14, -10),   S(-44, -12),   S(-35,  35),   S( 20,  79),
            S(-16, -37),   S( -5, -13),   S( 19,   2),   S(-17, -19),   S(-29,  -9),   S( 20,  47),   S(-10,  -4),   S(  5,  -9),
            S(  1,  -6),   S(-11, -43),   S( -5,   3),   S(-22, -28),   S( 17,   8),   S( 25,  40),   S(  3,  33),   S( -9, -50),
            S(-12,   8),   S(  3,   4),   S(-26,  39),   S(-47,  41),   S( -4,  -1),   S(  2,  23),   S(-14,  24),   S(  7,  31),
            S( -2,   6),   S(  7,  24),   S(-26,  34),   S(-34,  20),   S(-13,  64),   S(-12,  20),   S( -9,   0),   S( -9, -15),
            S(-11,  27),   S(  0, 102),   S( -4,  40),   S( 11,  37),   S(  1,   5),   S(  2,  11),   S(-25, -67),   S(-14, -25),
            S( 15, 111),   S(-11,  33),   S( 13,  59),   S(  2,  22),   S( 18,  58),   S(  9,  28),   S( -7,   3),   S(  6,  31),

            /* bishops: bucket 12 */
            S( -1,   3),   S( -4, -20),   S(  0,   1),   S(  2,  38),   S(-12, -12),   S( -5,   7),   S( -3,   0),   S(  0,  -5),
            S(  5,   9),   S(  3,  18),   S( -4,  -3),   S(  3,  19),   S( -2,  -5),   S(  8,   9),   S(-20, -36),   S( -2, -11),
            S( 10,  47),   S( 11,  51),   S( 15,  48),   S( 24,  22),   S( 15,  20),   S(  0, -32),   S(  6,   9),   S( -5,   3),
            S(  2,  39),   S(  7,  58),   S(  0,  49),   S( 10,  16),   S(  4, -35),   S(  5,  18),   S(  4,  -2),   S(  9,  16),
            S(  9,  12),   S( -2,  14),   S(  5,  31),   S( -6,  23),   S( 16,  29),   S( 10,  34),   S( -1, -30),   S(  9,  20),
            S( -1,   2),   S( -9, -34),   S( -4,   2),   S( -4,   9),   S( 16,  74),   S( 18,  42),   S( -9, -24),   S( -2,  -9),
            S( -2,   0),   S(  0,  -1),   S( -1,  -1),   S( -3,  -8),   S(  7,  43),   S(  9,  51),   S(  5,  40),   S( -3,  23),
            S(  1,  17),   S(  0,   5),   S( -4, -14),   S( -1,  -4),   S(  3,  19),   S(  0,   2),   S(  8,  71),   S(  4,  22),

            /* bishops: bucket 13 */
            S( -4,   3),   S(  0,   0),   S( -6, -38),   S( -8, -18),   S(  6,  26),   S(-14, -28),   S(-16, -26),   S( -4, -20),
            S(  1,  29),   S( -1,  14),   S( -5,  -1),   S( 18, 103),   S(-20, -19),   S( 13,  23),   S( -3, -42),   S( -4, -21),
            S(  0,   1),   S( 25,  93),   S(  7,  18),   S( 10,  12),   S( -9, -10),   S(  6,  40),   S( -1,  -9),   S(  8,  20),
            S( 18,  82),   S( 12,  75),   S(  7,  49),   S(-17,  -6),   S( 20,  47),   S( -3, -11),   S( 12,  44),   S(  8,  34),
            S(  2,  50),   S( -3,  -6),   S(  0, -23),   S(  2,  34),   S( -2,   9),   S( -3,  13),   S(  7,  27),   S( -1,  27),
            S(  6,  48),   S( -2,  -3),   S(  1,  29),   S( 11,   8),   S( -5,  53),   S(-14, -43),   S( -7,  -2),   S(  3,  11),
            S(  9,  39),   S( -6, -12),   S( -6, -10),   S(  0,  30),   S( -3,  11),   S(  0,  39),   S(  8,  32),   S(  7,  68),
            S(  0,  -9),   S( -6,  -5),   S( -4,   2),   S(  0,  10),   S( -3,   4),   S(-10, -14),   S(  9,  53),   S( 10,  58),

            /* bishops: bucket 14 */
            S( -9, -27),   S(  5,   6),   S( 10,   4),   S(  6,  45),   S(-12, -18),   S( -4, -17),   S( -6, -13),   S( -7, -33),
            S(  0,   7),   S(  3,   9),   S(  2,  31),   S(  2,  14),   S(  8,  47),   S( -1,  21),   S( -6,  16),   S(  2,  35),
            S( -1,   6),   S( -2,   3),   S( -1, -14),   S(  8,  26),   S(  9,  31),   S(  2,  43),   S(  8,  74),   S(  1,  17),
            S(  6,  42),   S(  6,  13),   S(-25, -10),   S( -2,  31),   S( -2,  20),   S(  2,  39),   S( 15,  80),   S(  2,  45),
            S(  3,  18),   S(  5,  17),   S( -9,  21),   S( -4,  33),   S(-12,   2),   S(  2,  29),   S( 15,  57),   S( -4,  12),
            S( -1,  11),   S(  8,  35),   S(  4,  60),   S(  6,  44),   S( -6,  12),   S( -5,   4),   S( -8, -13),   S( 10,  32),
            S(  9,  46),   S( 10,  38),   S(  3,  43),   S(  5,  16),   S(  2,   9),   S(  2,  17),   S(  0,  -8),   S(  3,  24),
            S(  5,  49),   S( 11,  56),   S(  5,  19),   S(  2,  13),   S( -8, -27),   S( -4, -17),   S(  9,  12),   S(  2,  15),

            /* bishops: bucket 15 */
            S(  0,  -3),   S( -3,  -6),   S(-13, -28),   S( -4, -21),   S(-14, -26),   S(  3,  -6),   S( -3, -22),   S( -5, -16),
            S(  7,  26),   S( -8,  -7),   S( -5, -23),   S( 13,  37),   S( 18,  12),   S( -2,  22),   S(  2,   1),   S( -3,  -6),
            S(  1,   6),   S( -7, -23),   S( -5,  -3),   S( 16,  31),   S( 12,  33),   S( 19,  44),   S(  8,  58),   S(  5,  42),
            S( -1,  -3),   S( 19,  60),   S(  9,  36),   S(-11,  -4),   S( -2,  21),   S(  5,  29),   S( 21,  79),   S(  3,  25),
            S( -2,  -8),   S(  0,  -7),   S( -1,   8),   S( 25,  82),   S(-10,  43),   S(  2,  28),   S( -1,  12),   S( -7,  -4),
            S(  1,   3),   S( -6,   1),   S(  2,  34),   S( 10,  32),   S( 19,  68),   S(  3,  31),   S( -6,  -4),   S(  0,   7),
            S(  2,  10),   S(  1,  17),   S( -7,  22),   S(  9,  26),   S(  6,  39),   S( -4, -10),   S( -4,   3),   S( -1,   5),
            S( -1,   2),   S( -1,  21),   S(  7,  44),   S(  7,   5),   S(  5,  24),   S( -2,   2),   S(  3,  10),   S(  2,  17),

            /* rooks: bucket 0 */
            S(-12,   6),   S( 16,   0),   S(  5,  15),   S(  7,  19),   S(-17,  65),   S(-13,  51),   S(-37,  75),   S(-54,  71),
            S( -8,  15),   S( -4,  54),   S(-30,  33),   S( -5,  58),   S( -9,  74),   S( -7,  33),   S(  1,  17),   S(-32,  59),
            S( 21, -20),   S(  4,  15),   S(-21,  41),   S(-18,  35),   S(-29,  68),   S(-27,  50),   S(-17,  51),   S( -5,  39),
            S(-14,   9),   S( 31,  15),   S(-44,  55),   S( 11,  28),   S(  7,  66),   S(-15,  62),   S( -8,  44),   S(-14,  58),
            S( 35, -43),   S( 35,  21),   S(-21,  62),   S( 24,  35),   S( 54,  31),   S( 35,  73),   S( 80,  43),   S(  7,  57),
            S( 79,  -6),   S( 52,  57),   S( 58,  31),   S(106,  55),   S(  1,  74),   S( 41,  60),   S( 11,  80),   S(-24,  69),
            S( 24,  60),   S( 51,  85),   S(118,  77),   S( 54,  32),   S( 83,  49),   S( 24,  76),   S(  5,  89),   S( -7,  79),
            S( 17,  -8),   S( 24,  14),   S( 59,  51),   S( 68,  11),   S( 79,  65),   S(103,  30),   S( 70,  42),   S( 72,  -5),

            /* rooks: bucket 1 */
            S(-60,  55),   S(-23,  12),   S(-10,  25),   S(-30,  35),   S(-33,  45),   S(-36,  46),   S(-53,  63),   S(-79,  78),
            S(-70,  61),   S(-38,  27),   S(-40,  41),   S(-20,  40),   S(-35,  22),   S(-58,  56),   S(-43,  25),   S(-38,  54),
            S(-53,  54),   S(-18,   5),   S(-32,  30),   S(-49,  47),   S(-43,  31),   S(-51,  41),   S(-59,  42),   S(-26,  40),
            S(-81,  73),   S(-36,  45),   S(-22,  42),   S(-22,  28),   S(-67,  66),   S(-62,  79),   S(-40,  51),   S(-91,  91),
            S(-39,  57),   S(-15,   8),   S( 33,  30),   S( 14,  33),   S(-10,  35),   S(-17,  77),   S(-10,  58),   S(-35,  89),
            S( 57,  32),   S( 83,  -4),   S( 55,   5),   S( 10,  65),   S( -9,  49),   S( 15,  60),   S( 28,  51),   S(  9,  81),
            S( 57,  67),   S( 48,  18),   S( 23,  53),   S( 14,  36),   S(  6,  75),   S( 17,  53),   S( 67,  65),   S( 61,  74),
            S( 29,  22),   S( 20,   1),   S( -9,  13),   S( -2,  17),   S( 41,  31),   S( 26,  31),   S( 52,  34),   S( 66,  44),

            /* rooks: bucket 2 */
            S(-68,  88),   S(-55,  81),   S(-42,  77),   S(-41,  57),   S(-31,  54),   S(-43,  45),   S(-34,  33),   S(-74,  78),
            S(-63,  85),   S(-58,  71),   S(-54,  81),   S(-54,  70),   S(-62,  74),   S(-60,  44),   S(-32,  38),   S(-48,  58),
            S(-52,  79),   S(-52,  78),   S(-56,  68),   S(-38,  60),   S(-40,  59),   S(-39,  48),   S(-37,  48),   S(-20,  51),
            S(-43,  92),   S(-48,  85),   S(-60,  91),   S(-91,  88),   S(-60,  67),   S(-55,  67),   S(-33,  45),   S(-27,  48),
            S(-50, 110),   S(-43, 101),   S(-21,  93),   S(-43,  75),   S(-42,  85),   S( 12,  56),   S(-53,  75),   S(-20,  72),
            S(  5, 102),   S( -1,  96),   S( 23,  82),   S(-10,  73),   S( 67,  42),   S( 50,  63),   S( 94,  36),   S( 57,  72),
            S( 61,  90),   S( 24,  98),   S( 54,  69),   S( 68,  49),   S( 45,  49),   S( 72,  62),   S(-48, 126),   S( 28, 105),
            S( 20,  67),   S( 75,  61),   S( 62,  49),   S(  9,  41),   S( -7,  52),   S( 43,  49),   S( 50,  40),   S( 74,  40),

            /* rooks: bucket 3 */
            S(-19, 104),   S(-14, 110),   S(-11, 136),   S( -9, 125),   S( -1,  88),   S(  3,  81),   S( 15,  56),   S(-15,  48),
            S( -8, 104),   S(-11, 111),   S(-18, 135),   S( -5, 129),   S( -7,  93),   S(  3,  65),   S( 49,  30),   S(  8,  62),
            S(  2, 109),   S( -9, 108),   S(-19, 120),   S(-21, 138),   S(  3,  86),   S( -2,  78),   S( 22,  73),   S(  8,  66),
            S( -4, 125),   S(-16, 144),   S(-16, 147),   S(-22, 140),   S(-18, 104),   S(-19, 105),   S( 14,  80),   S(-11,  77),
            S(-10, 142),   S(-18, 151),   S( 10, 153),   S( 10, 143),   S(  8, 112),   S(  1, 114),   S( 36,  87),   S( 16, 100),
            S(  4, 155),   S( 11, 143),   S( 30, 142),   S( 39, 137),   S( 93,  97),   S(111,  87),   S( 74, 108),   S( 40,  81),
            S( 12, 152),   S( -4, 161),   S( 19, 164),   S(  4, 169),   S( 38, 137),   S(104,  92),   S( 54, 148),   S(188, 117),
            S(129,  24),   S( 44, 108),   S( 35, 140),   S( 43, 122),   S( 27, 109),   S( 33, 106),   S( 57,  80),   S(129,  48),

            /* rooks: bucket 4 */
            S(-28, -22),   S( 12, -20),   S(-20, -14),   S(-18,  -3),   S(-50,  13),   S(-29,  32),   S( -2, -30),   S(-78,  28),
            S(-42, -28),   S(-42,  -2),   S( 34, -37),   S( -5, -22),   S(-11,  31),   S(-31,   4),   S(-34,  11),   S(-13,  19),
            S(-39,  -6),   S(-56, -22),   S(-35,  -6),   S(-28, -23),   S(-36, -14),   S(-56,  15),   S(-23,  15),   S(-49,  -9),
            S(-40, -42),   S( 45,  11),   S(  5, -33),   S( 20, -35),   S( 44,  -2),   S( -6,  12),   S(  4, -11),   S(-26,   6),
            S(-13, -17),   S( 42, -29),   S( 46,   7),   S( 71, -35),   S( 69,  -5),   S( 73,  12),   S( 16,  13),   S( 12,  18),
            S(-23, -58),   S(  6,  12),   S( 21, -17),   S( 45,  20),   S( 37,  24),   S( 15,  18),   S( 44,  28),   S( 33,  33),
            S(-17, -23),   S( 44,   4),   S( 72,  -9),   S( 68, -17),   S( 24,   4),   S( 21, -21),   S(-11,  -5),   S( -4,  15),
            S( 13, -19),   S( 29,  -7),   S( 63, -25),   S( 28,  10),   S( 58,   2),   S(  8,   1),   S(  5,   6),   S( 18,  -1),

            /* rooks: bucket 5 */
            S(-47,  24),   S(-19,  -2),   S(-22,   9),   S(  6,  14),   S(-14,   9),   S(-22,  10),   S(-44,  45),   S(-60,  51),
            S(  8,  -6),   S(-49, -13),   S( -6, -19),   S( -1,  -2),   S(-33,   5),   S(-25,  -9),   S(-63,  22),   S( -8,  17),
            S(-96,  39),   S(-31, -14),   S( 13, -34),   S(-10,  -2),   S(-45,  10),   S(  7, -20),   S(-79,  35),   S(-47,  22),
            S(-54,  20),   S( -3,   6),   S( 48, -28),   S( 18,   2),   S( 32, -17),   S(  8,  18),   S(-15,  16),   S(-23,  31),
            S( 48,  -3),   S( -2,  16),   S( 45,   5),   S( 29,   4),   S(-14,  21),   S( 72,   2),   S(  3,  20),   S( 76,  11),
            S( 29,  14),   S( 18,  -1),   S( 12,  -7),   S( -3,  -5),   S(  9,  24),   S( 31,  27),   S( 79,   2),   S( 34,  35),
            S( 37, -11),   S( 44, -17),   S( 15,   4),   S( 44,  14),   S( 62,  -9),   S( 40, -32),   S( 61,  -8),   S( 34,   5),
            S( 12,  12),   S( 14,   8),   S( 47, -15),   S( 19,  30),   S( 52,  14),   S( 24,  15),   S( 16,  29),   S( 45,  28),

            /* rooks: bucket 6 */
            S(-80,  56),   S(-20,  10),   S(-39,  19),   S(-32,  17),   S( 12,  -9),   S( -2,   1),   S(-19,  -1),   S(-59,  24),
            S(-86,  35),   S( -8,   8),   S(-27,   8),   S(  5,  -7),   S( -6,  -7),   S(-70,  19),   S(-42,   8),   S(-65,  29),
            S(-111,  56),  S(-36,  15),   S(-44,  12),   S( -5,   5),   S(-44,  22),   S( -4,   8),   S(-32,  -5),   S(-17, -16),
            S(-45,  37),   S(-17,  29),   S(  6,   7),   S( 25,   2),   S( 29,  -6),   S(-21,  15),   S(-31,   4),   S(-15,  20),
            S(-24,  43),   S( 42,  17),   S(124,  -4),   S( 61,   2),   S( 16,  12),   S( 28,  28),   S( 43,   3),   S(121, -26),
            S( 88,  14),   S( 80,  -2),   S(126, -10),   S( 57,  -3),   S( 14, -22),   S( 31,  34),   S( 27,   3),   S( 85,   0),
            S( 55,  -1),   S( 95, -15),   S(131, -38),   S(103, -34),   S( 38,   6),   S( 28,   6),   S( 77, -28),   S( 51, -21),
            S( 34,  -1),   S( 12,  38),   S( -9,  27),   S( 56,  -9),   S( 58,   7),   S( 57,   8),   S( 57,   3),   S( 19,  25),

            /* rooks: bucket 7 */
            S(-95,  37),   S(-87,  44),   S(-61,  39),   S(-58,  45),   S(-45,  19),   S(-32,   4),   S(-40,  15),   S(-55,  -4),
            S(-91,  41),   S(-34,   5),   S(-67,  29),   S(-71,  45),   S(-62,   3),   S(-10,  -4),   S(-12,  25),   S( -3, -45),
            S(-54,  25),   S(-41,  11),   S(-49,  16),   S(-44,  32),   S(-69,  26),   S(-40,  24),   S( 28, -10),   S( 27, -40),
            S(-105,  42),  S(  1,   8),   S( 13,   2),   S( 72, -22),   S(  0,   4),   S( 67, -17),   S( 30,  19),   S( 40, -25),
            S( 15,  21),   S( 19,  25),   S( 59,  13),   S( 73,   0),   S(129, -30),   S(127, -36),   S(125,  -8),   S(-63, -12),
            S( 27,  29),   S( 29,  15),   S(131, -11),   S(121, -18),   S(127,  -9),   S( 53,   8),   S( 32,  32),   S( -2, -15),
            S(-37,  16),   S( 25,  -2),   S( 41,   3),   S( 84, -16),   S(119, -35),   S( 89, -16),   S( 78,  10),   S( 45, -15),
            S(-16,   1),   S( 14,  21),   S( 34,  15),   S(  5,  23),   S( 10,  14),   S( 57,  -4),   S( 63,  16),   S( 48, -13),

            /* rooks: bucket 8 */
            S(-12, -45),   S( 40, -53),   S( 34, -40),   S( 24,  -8),   S(-17, -54),   S(-20, -34),   S( -6, -45),   S(-15, -25),
            S(-21, -72),   S(-10, -40),   S( 23, -14),   S(-26, -69),   S(-12, -75),   S(-17, -46),   S(  6,   8),   S(-37, -42),
            S(  3, -15),   S(  2,  -2),   S( 11,  11),   S(  4,   0),   S(  5,  12),   S( -5,   5),   S( -2,  14),   S(-11, -26),
            S(  1, -15),   S( -2, -20),   S(  0, -27),   S( 33,  16),   S(  3,  15),   S( 16,  14),   S(  2,  14),   S(-10, -30),
            S( -7, -45),   S( 25,  38),   S( 19, -20),   S( 36,  32),   S( 13,  -1),   S( -7, -30),   S(  4,  24),   S(  2,  14),
            S(-21, -11),   S( 17, -26),   S(  3, -11),   S( -2, -24),   S( 14,  36),   S( -5,  -3),   S(-11,  -5),   S(-17, -15),
            S( 26,   9),   S( 33, -10),   S( 12, -23),   S( 20, -13),   S(  8,   0),   S(  1,  22),   S( 17,  26),   S(  8,  -3),
            S( -1, -19),   S( 30, -15),   S( 11, -12),   S( 17,  11),   S(-22, -21),   S( 14,  26),   S(  7,  -2),   S(  5,   4),

            /* rooks: bucket 9 */
            S(  5, -97),   S(  6, -88),   S( 18, -95),   S( 25, -66),   S( 13, -59),   S( 11, -52),   S( 11, -56),   S( 10, -70),
            S(-41, -88),   S(  1, -63),   S( -7, -80),   S(-23, -93),   S(  2, -59),   S(-15, -23),   S(-20, -72),   S( -3, -45),
            S( -8, -33),   S(  9, -14),   S(  9,  -7),   S(-11, -59),   S(  5, -32),   S(  0, -26),   S( -6,  -8),   S(  0, -11),
            S( 16, -43),   S(  5, -22),   S(  7,   8),   S(  2,  -9),   S(-12, -55),   S( 12, -50),   S( -3, -21),   S( -2, -39),
            S( 35, -43),   S( -1, -40),   S(-18, -75),   S(  4, -23),   S( -8, -61),   S( -4, -13),   S(  2, -49),   S(-28, -50),
            S(  2, -49),   S(-27, -55),   S(-10, -43),   S( 34, -19),   S(  6, -22),   S(-13, -22),   S(-11, -33),   S(-15, -31),
            S(  7, -33),   S( -1, -28),   S(  5, -60),   S( -9, -10),   S(  8, -47),   S(  8, -20),   S(  4, -36),   S(-13, -51),
            S(-10, -13),   S(-22, -22),   S( 13, -20),   S( 10, -11),   S(  5, -25),   S(  1,  -9),   S(-10, -13),   S( -3, -10),

            /* rooks: bucket 10 */
            S( 20, -86),   S(-25, -70),   S( 19, -78),   S( 33, -107),  S(  4, -93),   S( 47, -93),   S( 31, -82),   S(-11, -73),
            S(-31, -54),   S(-18, -56),   S(-45, -56),   S(-33, -85),   S( -3, -69),   S(-12, -90),   S(  7, -54),   S(-28, -88),
            S( -2, -39),   S(-25, -20),   S(-35, -45),   S(-51, -78),   S( -3, -38),   S( -2, -16),   S( -6, -80),   S(-25, -62),
            S(-18, -33),   S(-37, -60),   S(-13, -42),   S( -6, -26),   S(  1, -22),   S(  7,  24),   S(-10, -89),   S(  1, -57),
            S(  5, -49),   S( -6, -39),   S( -6, -54),   S( -2, -72),   S( 10, -11),   S(  0, -16),   S( 15, -69),   S(-32, -93),
            S(-23, -43),   S( 21, -19),   S(-12, -54),   S( -2, -83),   S(  3, -44),   S(  7, -50),   S(-10, -65),   S( -4, -41),
            S(-25, -29),   S(  9, -52),   S( -4, -38),   S( 11, -47),   S( 28, -30),   S(  6, -37),   S(-12, -75),   S(  2, -65),
            S(-21, -51),   S(-24, -25),   S(  5,   2),   S( -9, -44),   S( -4, -10),   S( -9, -20),   S( 10, -54),   S(-10,  -8),

            /* rooks: bucket 11 */
            S(  8, -79),   S(-14, -53),   S( -8, -36),   S( 43, -54),   S(-38, -53),   S( 56, -75),   S(  0, -54),   S( -8, -68),
            S( -2, -24),   S(-25, -22),   S(-27, -30),   S(-26,  -9),   S(-19, -41),   S( 21,  -7),   S(-11, -57),   S(-32, -89),
            S(-30, -14),   S(-40,  -9),   S(-28,   3),   S(-23, -26),   S(  3, -16),   S( -5, -26),   S( 19, -17),   S(-10,  -3),
            S(-15, -55),   S(  0, -29),   S(  1, -32),   S( 11,  -9),   S( 26,   6),   S(-13, -65),   S(  3,   7),   S(-20, -59),
            S(-10, -44),   S(  5, -33),   S(  1, -15),   S(-12, -11),   S( 44, -29),   S( 21, -44),   S( 22, -13),   S(-18, -57),
            S( -5, -27),   S( -5, -38),   S( 14, -36),   S( 10, -10),   S(-18, -33),   S( 18, -17),   S( 23,  -3),   S( 13, -49),
            S(-24,  -3),   S(-33, -26),   S(  1, -25),   S(-18, -29),   S( 18, -28),   S( 24, -24),   S( 24, -26),   S( -9, -41),
            S(-10, -41),   S( 25,  21),   S( -1,  14),   S( 11,   8),   S(-14,   7),   S( 19,  -6),   S( 51, -20),   S( -6, -22),

            /* rooks: bucket 12 */
            S(-23, -83),   S( -7, -21),   S(  1, -31),   S( 10, -27),   S(  5, -26),   S( 21,   1),   S(-20, -66),   S(-18, -49),
            S(  0,  -2),   S(  5,  12),   S( 12,  20),   S(  7,  -5),   S( -4, -29),   S( 22,  13),   S(  8,   6),   S(-15, -47),
            S( -6, -23),   S(  9,  28),   S( 19,  24),   S( 14,  -2),   S(  3,  -8),   S( 17,   2),   S(  8,  25),   S(  1,  12),
            S(  0,   1),   S( -3,  -8),   S(  7,  11),   S( 21,  30),   S(  6,   6),   S(  4,   4),   S(  6,  16),   S(  0,   2),
            S( 14,  -5),   S(  7,   5),   S( 11,  -7),   S( -5, -42),   S( 12,  36),   S(-10, -23),   S(  1, -18),   S( -2, -19),
            S(-13, -58),   S( -6, -40),   S(  2, -15),   S( -7, -50),   S( 12,  32),   S( -8, -29),   S(  0,   8),   S( -3, -21),
            S(-13, -15),   S(  4, -13),   S(  7, -12),   S(  6,   0),   S(-13, -36),   S( 15,  19),   S(  1,  18),   S( -2,  -4),
            S( -4, -17),   S( -6, -21),   S(  6, -23),   S( 17,  11),   S(  0,  -5),   S( -2,  -5),   S( -4, -20),   S(  4,  10),

            /* rooks: bucket 13 */
            S(-10, -57),   S(-23, -83),   S(-15, -28),   S( -6, -52),   S( -4, -65),   S( -1, -41),   S(-34, -51),   S(-21, -57),
            S(-14, -44),   S( -2, -31),   S( -1,  -4),   S( -4, -34),   S( 22,  16),   S(  3, -47),   S( 19, -30),   S(-17, -75),
            S( -2, -45),   S( -8, -33),   S(  3, -14),   S( 11, -15),   S( 11,  -8),   S( 38,   3),   S(  9, -10),   S( -7, -63),
            S(  3, -20),   S(-13, -54),   S(  6, -12),   S(  9, -22),   S(  4, -15),   S(-13, -47),   S(  4,  -1),   S(  3,  20),
            S(  4, -32),   S(  0, -82),   S(  0, -69),   S(  4, -25),   S(  3, -47),   S( -5, -43),   S(  1,   1),   S( -2,  -9),
            S(  0,  -9),   S( -1, -28),   S( -1, -36),   S( -6, -74),   S(-13, -129),  S( -2, -46),   S(-12, -21),   S( -8, -35),
            S( -4, -34),   S(  3, -35),   S(-17, -71),   S( 11, -11),   S( -3, -57),   S(  8,  -1),   S( -8, -24),   S(  1, -29),
            S( -4, -38),   S( -6, -41),   S(-21, -48),   S( 15,  -6),   S(-11, -56),   S( -1, -16),   S( -4, -25),   S(  7,   5),

            /* rooks: bucket 14 */
            S( -7, -44),   S(-34, -64),   S(-15, -60),   S(-19, -88),   S(-20, -77),   S(  9, -44),   S(-25, -102),  S(-22, -70),
            S( 13, -34),   S(  1, -44),   S( 11, -11),   S( -4, -54),   S(  0,  -9),   S( -2, -12),   S(  2, -35),   S( -4, -69),
            S(  5,  -5),   S(  1, -24),   S( -9, -47),   S(  6, -26),   S(  5, -17),   S(  0,  -3),   S( 21,  -7),   S(-21, -78),
            S(  0, -18),   S( 21,  27),   S( 10,  33),   S(  1, -29),   S( -4, -23),   S(  3, -26),   S( -2, -24),   S(-21, -58),
            S(  0, -21),   S( 12,  37),   S( -4, -17),   S( -2, -61),   S(  2, -23),   S( 18, -25),   S( -6, -54),   S(-11, -46),
            S(  8,  -6),   S(  1,   1),   S(  9, -14),   S(  5, -87),   S(  1, -62),   S(  8, -44),   S( -6, -89),   S(-16, -45),
            S(-16, -38),   S(  1,  -4),   S( -6, -41),   S(-26, -93),   S(-10, -54),   S(  5, -11),   S( -8, -65),   S(-15, -18),
            S( -9, -37),   S( -4, -29),   S( -7, -50),   S( -1, -56),   S( -9, -50),   S(-14, -97),   S( -3, -65),   S(-11, -27),

            /* rooks: bucket 15 */
            S(-12, -60),   S( -7, -48),   S(-33, -62),   S(-17, -49),   S(  6, -22),   S( -5, -35),   S(  6,   2),   S(-21, -77),
            S(  3,  -7),   S(-14, -65),   S( -7, -42),   S( -3, -40),   S(-13, -54),   S(  3, -12),   S(  4,   3),   S( -1, -10),
            S(  8,   9),   S(  4, -24),   S(  8,  -9),   S( 11, -17),   S( -2, -43),   S(-10, -42),   S( 12,  10),   S(  2,  -8),
            S(  1,   9),   S( -2,   2),   S( 16,  46),   S( -8, -14),   S( 13,  28),   S(-14, -51),   S(  6,  -8),   S( 13, -20),
            S(  3,  -7),   S(  3,  -5),   S( 13,  15),   S(  4,   0),   S(  2,  -2),   S(  1, -28),   S( 10,  -4),   S(  1, -40),
            S( -2, -15),   S(  9,  28),   S(  4,  -3),   S( -2, -31),   S(  2, -13),   S(  7, -42),   S( 11, -17),   S( -6, -19),
            S(  3, -19),   S( -3,  -3),   S(  4,  -6),   S(  3,   0),   S(  2,  -7),   S(  0, -20),   S(  9, -43),   S(-12, -62),
            S(  2, -21),   S(  2,   5),   S( -3,   1),   S( -1,  -6),   S( -6, -21),   S( -8, -42),   S(  6, -36),   S(-12, -57),

            /* queens: bucket 0 */
            S(  4, -27),   S(-33, -76),   S(-25, -84),   S( -5, -107),  S(-10, -77),   S(-17, -67),   S(-64, -39),   S(-16, -41),
            S( -9, -46),   S(  7, -81),   S( 11, -71),   S(-21, -41),   S( -4, -25),   S(  5, -63),   S(-21, -73),   S(-32, -23),
            S(-19,  -1),   S(-11, -11),   S( 23, -58),   S(-15, -38),   S(-19,  25),   S( -7,  -9),   S(-16, -23),   S(-80, -52),
            S(-41,  52),   S( 13,  -5),   S(-30,  40),   S(-11,  41),   S( -2,  42),   S(-27,  34),   S(-49,  21),   S(-27, -68),
            S(-65,  37),   S(-37,  93),   S(-14,  74),   S(-11,  65),   S(-32,  88),   S(-46,  80),   S(-46,  24),   S(-38,  12),
            S(-12,  23),   S( 34,  82),   S( 14,  47),   S(-43,  86),   S(-52,  66),   S(-38,  49),   S(-84,  11),   S(-39, -45),
            S(  0,   0),   S(  0,   0),   S( 50,  31),   S(-28,  54),   S(-62,  47),   S(-104, 105),  S(-71,  57),   S(-113,  19),
            S(  0,   0),   S(  0,   0),   S( 27,  14),   S(  4,  -3),   S(-46,  45),   S(-38,  34),   S(-55,  40),   S(-50,  -3),

            /* queens: bucket 1 */
            S( -3, -38),   S(  5, -44),   S(  6, -55),   S( 25, -116),  S( 21, -61),   S( -3, -73),   S( 14, -34),   S(  7, -12),
            S(-23, -18),   S( 13,   9),   S( 42, -79),   S( 17, -31),   S( 29, -13),   S(  6, -17),   S(-33,   5),   S(-44, -20),
            S( 33, -46),   S( 13, -28),   S(  1,  26),   S( 17,  12),   S(-15,  31),   S( 18,   6),   S( -5,  14),   S( 15, -14),
            S( 16,  32),   S(-27,  64),   S(  2,  33),   S( 33,  56),   S(  9,  81),   S( -5,  56),   S(  6,  43),   S(-11,  69),
            S( 22, -18),   S( 16,  99),   S( -7, 103),   S(-20,  74),   S( 18,  95),   S( 32,  33),   S(-31,  89),   S(  5,  62),
            S( 57,  14),   S(112,  43),   S( 91,  67),   S(123, 108),   S( 69,  96),   S(-15, 113),   S( 17,  61),   S(  9,  25),
            S( 94,   5),   S(108,   2),   S(  0,   0),   S(  0,   0),   S( 10,  68),   S(-45,  90),   S(-13, 100),   S(-74,  55),
            S( 82,  24),   S( 41,  26),   S(  0,   0),   S(  0,   0),   S( 38,  19),   S( 47,  32),   S( 92,  37),   S(-13,  46),

            /* queens: bucket 2 */
            S( 24, -39),   S( 24, -31),   S( 41, -41),   S( 39, -26),   S( 41, -46),   S( 29, -45),   S(  4, -57),   S( 34,  -4),
            S( 25,  19),   S(  6,  22),   S( 44, -13),   S( 33,   3),   S( 56, -23),   S( 25,  -8),   S( 19, -25),   S( 10,  31),
            S( 34,  11),   S( 22,  41),   S(  8,  82),   S( 10,  50),   S( 27,  47),   S( 19,  43),   S( 29,  30),   S( 36,  25),
            S( 21,  55),   S( 13,  95),   S(  2, 102),   S( -2, 126),   S( 28, 108),   S(-10, 104),   S( 18,  98),   S( 21,  96),
            S(-17,  94),   S(  8,  55),   S( 15, 112),   S( 18, 137),   S( 29, 140),   S( 94,  71),   S( 52,  96),   S( 58,  73),
            S(-19,  86),   S(-42, 119),   S( 15, 121),   S( 69, 100),   S( 28, 126),   S(108, 139),   S(112,  86),   S( 25, 151),
            S(-10,  97),   S(-41, 111),   S(-20, 106),   S(105,  67),   S(  0,   0),   S(  0,   0),   S( 27, 110),   S( 53,  88),
            S( -2,  54),   S( 55,  45),   S( 72,  19),   S( 36,  59),   S(  0,   0),   S(  0,   0),   S( 66,  86),   S( 21,  80),

            /* queens: bucket 3 */
            S(-37,  37),   S(-27,  38),   S( -6,  30),   S( -3,  44),   S(-15,  23),   S( -6, -19),   S( -5, -76),   S(-56,  26),
            S(-45,  53),   S(-24,  38),   S(-10,  49),   S(-15,  77),   S( -3,  61),   S(  0,  30),   S( 25, -23),   S( 36, -80),
            S(-45,  63),   S(-33, 102),   S(-20, 110),   S(-36, 138),   S(-27, 108),   S(-24, 106),   S(  2,  64),   S( -7,  52),
            S(-33,  72),   S(-52, 128),   S(-50, 158),   S(-24, 165),   S(-29, 164),   S(-33, 118),   S( -9, 108),   S(-17,  95),
            S(-36,  94),   S(-43, 128),   S(-41, 152),   S(-30, 194),   S(-28, 170),   S(-23, 187),   S(-21, 155),   S(-29, 138),
            S(-61, 115),   S(-37, 145),   S(-54, 174),   S(-49, 196),   S(-30, 206),   S( 19, 163),   S(-36, 212),   S(-19, 182),
            S(-92, 140),   S(-96, 157),   S(-98, 206),   S(-75, 194),   S(-88, 217),   S( 11, 135),   S(  0,   0),   S(  0,   0),
            S(-132, 181),  S(-105, 180),  S(-77, 132),   S(-104, 182),  S(-47, 159),   S( -5, 113),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-39,   4),   S(-51, -44),   S(-37,  21),   S( -9, -27),   S(-17, -31),   S( -3,  -5),   S(-22, -19),   S(  9,   1),
            S( -6,   9),   S( 15,  41),   S(-37,  -6),   S(-23,  -2),   S(-36,  29),   S(-20,  40),   S(-41, -13),   S(-36, -36),
            S(  5,  40),   S( 39,  -5),   S( 11,  -8),   S( 26,  13),   S( 20,  11),   S( -6,  36),   S(  2, -15),   S( 14,  20),
            S( -4,  -4),   S( 37,   8),   S( 27,  19),   S( -1,  16),   S( 25,  57),   S(  1,  48),   S(-51, -32),   S(-49, -17),
            S(  0,   0),   S(  0,   0),   S( 27,   0),   S( 72,  53),   S( 25,  47),   S( 38,  31),   S(-23, -25),   S( -3,  12),
            S(  0,   0),   S(  0,   0),   S( 32,  40),   S( 47,  44),   S( 22,  26),   S( 35,  57),   S(  4,  35),   S(-31, -31),
            S( 13,  -3),   S( 27,  22),   S( 67,  35),   S( 50,  45),   S( 45,  33),   S(-22,  13),   S( -7,   1),   S(-37,  -1),
            S( 42,  21),   S(  2, -25),   S( 24,  -7),   S( 41,  23),   S( -2, -25),   S(-29, -14),   S(-36, -44),   S( 10, -10),

            /* queens: bucket 5 */
            S( 44,   3),   S( 48,   5),   S(-24, -34),   S(-30,   2),   S(  0, -26),   S( 58,  51),   S( 19,  18),   S(  1, -11),
            S(-12, -23),   S(-13, -11),   S( 19, -15),   S( -5,  30),   S(-13,  11),   S(-19,  -8),   S( 23,  -2),   S( 26,  19),
            S( 27,  18),   S( 26,  -5),   S( 23,  22),   S(-34,  40),   S(-17,  24),   S( 29,  40),   S(  5,  36),   S(-11,  18),
            S( 11, -60),   S( 48,  22),   S( 33,   1),   S( 34,  43),   S( 58,  44),   S( 11,  15),   S( 20,  23),   S(-21,   2),
            S( 64,  42),   S( 61,  21),   S(  0,   0),   S(  0,   0),   S( 20,  53),   S( 27,  16),   S( 60,  45),   S(-14,  -3),
            S( 35,  12),   S( 53,  62),   S(  0,   0),   S(  0,   0),   S( 46,  55),   S(100,  51),   S( 60,  36),   S( 48,  52),
            S( 46,  36),   S( 90,  34),   S( 75,  82),   S( 23,  40),   S( 32,  36),   S(132,  75),   S( 55,  44),   S(  6,   2),
            S( 25,  38),   S( 78,  54),   S( 88,  62),   S( 50,  23),   S( 47,  43),   S( 72,  63),   S( 20,  27),   S( 37,  40),

            /* queens: bucket 6 */
            S( 38,  29),   S(-46, -23),   S( 15,   8),   S( 34, -12),   S(  5, -13),   S(-32,  -4),   S(-33, -34),   S( 50,  35),
            S(  9,  -3),   S( 33,   9),   S( -7,  43),   S( 57,  23),   S(  5,  13),   S(-16,  12),   S(-37,  17),   S( 30,  44),
            S(-48,  32),   S( 18,  35),   S( -6,  21),   S( 17,  18),   S( 12,   1),   S( 29,   4),   S( 30,  42),   S( 85,  45),
            S( -4,  30),   S( -4,  21),   S( 50,  30),   S( 82,  45),   S( 18,  13),   S( 49,  27),   S( 91,  22),   S( 97,  50),
            S( 14,  23),   S(  8,  53),   S( 43,  58),   S( 68,  50),   S(  0,   0),   S(  0,   0),   S( 79,  67),   S( 99,  61),
            S( 25,  46),   S( 36,  55),   S( 50,  72),   S( 47,  53),   S(  0,   0),   S(  0,   0),   S( 62,  53),   S(103,  46),
            S( 58,   7),   S( -8,  29),   S( 80,  21),   S( 61,  60),   S( 59,  62),   S( 95,  79),   S(117,  55),   S(164,   7),
            S( 36,  16),   S( 47,  24),   S( 52,  48),   S(114,  79),   S(107,  37),   S(116,  51),   S(101,  47),   S( 81,   7),

            /* queens: bucket 7 */
            S(-38, -36),   S(-23,  -6),   S(-39,  11),   S(-32,   9),   S(  1, -16),   S(-36,  10),   S(-22,  -1),   S(-42, -27),
            S(-50, -10),   S(-83,  18),   S(-36,  47),   S(-22,  48),   S(-29,  34),   S(-29,  23),   S(-16,   5),   S( -9,  -7),
            S(-35, -13),   S(-23,  10),   S(-14,  33),   S( 12,  31),   S( 53,   5),   S(  7,  12),   S( 25,   1),   S( 48, -16),
            S(-58, -29),   S( -8,  -4),   S(  2,  26),   S( 26,  57),   S( 83,  22),   S( 94,  25),   S( 81,  -1),   S( 73,  17),
            S(-21,  -8),   S(-29,  40),   S( -1,  65),   S( 43,  43),   S( 68,  26),   S( 70,  40),   S(  0,   0),   S(  0,   0),
            S(-21,  12),   S( -7,  38),   S(-22,  75),   S(  8,  60),   S( 68,  14),   S(101,  51),   S(  0,   0),   S(  0,   0),
            S(-48,  32),   S(-58,  17),   S(  9,  14),   S( 65,  23),   S( 88,  36),   S(112,  29),   S( 85,  65),   S( 71,  41),
            S( -1, -15),   S( 24, -31),   S( 34,  16),   S( 40,  -2),   S( 59,  48),   S( 15,  22),   S(  3,  17),   S( 82,  12),

            /* queens: bucket 8 */
            S(-10, -30),   S( 22,  12),   S( -6, -28),   S( 13,   6),   S( -9, -25),   S( -6, -15),   S(  0, -14),   S(  1,  -1),
            S(-16, -33),   S( -2,   4),   S( 19,  17),   S( 15,  31),   S( 19,  27),   S( -3, -17),   S( -2,  12),   S( -2,  -8),
            S(  0,   0),   S(  0,   0),   S( 14,   5),   S( -6, -27),   S(  3, -16),   S( 16,  13),   S( -1,  -2),   S(-10,  -7),
            S(  0,   0),   S(  0,   0),   S(  3,   3),   S( 13,  13),   S( 11,  21),   S( 15,   5),   S(  7,  19),   S(  2,  -6),
            S( 11,  10),   S(  9,  16),   S( 11,  20),   S( 10,   0),   S(  1,  -4),   S( 14,  17),   S(  5,  -9),   S(  1,  -6),
            S( 10,  -2),   S( -8, -29),   S( 14,   9),   S( -6, -29),   S(  2,  -7),   S( 19,  14),   S(-17, -39),   S(-10, -17),
            S( -7, -33),   S(  4,  -7),   S( 10,   6),   S( 37,  41),   S( 17,   9),   S( 17,  23),   S(  8,  -1),   S(  5,  -2),
            S( -2, -17),   S( 22,  22),   S( 11,  10),   S(  8,  11),   S( 19,  13),   S(  3,  -6),   S( -6, -12),   S(-36, -60),

            /* queens: bucket 9 */
            S(  8, -11),   S(  0, -24),   S( -6, -16),   S( 14,   2),   S(  6,  -7),   S( -5, -27),   S( -5,  -9),   S( -6, -19),
            S( 22,  18),   S( 13,   0),   S(  7,  14),   S( 21,  26),   S( -3, -19),   S( -6, -23),   S( 18,  12),   S(  2,  -3),
            S(  7, -19),   S( -6, -20),   S(  0,   0),   S(  0,   0),   S( 10,   8),   S( 29,   7),   S( -2, -10),   S(  2,  -5),
            S( 12,  -2),   S( -9, -17),   S(  0,   0),   S(  0,   0),   S( 10,   4),   S( 33,  41),   S(  9,  -4),   S( -3,   1),
            S( 18,   8),   S(  6, -23),   S( 21,  33),   S(-14, -17),   S(  0, -18),   S( 20,  25),   S(  5, -27),   S(-11, -30),
            S(  8,   9),   S(  5,  -4),   S(  9,  19),   S( -3,  -8),   S( 14, -22),   S( -3, -16),   S( -9, -31),   S(-13, -41),
            S(  1,  -4),   S( 22,   2),   S( -1,   5),   S(-11, -14),   S( 28,  17),   S( 35,  40),   S( 10,  25),   S( 16,   4),
            S(  9,  -3),   S( 21,  -3),   S(-10, -25),   S( 15,  11),   S( 16,   9),   S(-10, -23),   S(  5, -25),   S( 13,   1),

            /* queens: bucket 10 */
            S( 17,  21),   S( 25,  15),   S(-15, -20),   S( -1,  -7),   S( -5, -30),   S( 24,  33),   S( 18,  11),   S( -8, -29),
            S(  4,  -3),   S(-15, -42),   S( 19,  14),   S(-19, -35),   S(  3,   3),   S( 22,   5),   S(  8,  -1),   S( -8, -11),
            S( -4,  -7),   S(  9,   2),   S(  3,  -8),   S( 15,   7),   S(  0,   0),   S(  0,   0),   S(  6,   0),   S(  1, -11),
            S( -5, -18),   S( 14,  16),   S(  9,  -1),   S( -7, -10),   S(  0,   0),   S(  0,   0),   S(  2,  -9),   S(  1, -15),
            S(  5,  11),   S( 17,  21),   S(  5,  -8),   S( 50,  62),   S(  7,   6),   S( -1,  -7),   S(  3, -19),   S( 27, -13),
            S(-12, -27),   S( 14,   9),   S( 36,  51),   S( 16,  22),   S(  5,  -4),   S( 16,  33),   S( 29,  22),   S( -8, -51),
            S(  2,  -8),   S( 23,  40),   S( 28,  53),   S( 49,  45),   S(  8,   6),   S( 26,  15),   S(  5, -23),   S(  3,  -6),
            S(-21, -51),   S(-22, -41),   S(  9,  -7),   S(-14, -35),   S( 20,  26),   S( -8, -13),   S(  1, -29),   S(-15, -30),

            /* queens: bucket 11 */
            S(-24, -23),   S(-20, -38),   S(  5, -14),   S( 19,  25),   S(  5,  10),   S(-14, -36),   S(  6, -18),   S(-13, -38),
            S(  1,  -9),   S( 14,  12),   S(-17, -22),   S(  8, -14),   S( 46,  34),   S( -3, -13),   S( 10,  -4),   S(  3, -15),
            S(  0,  -4),   S(  1,   0),   S(-12,  11),   S( -4, -19),   S(  5, -17),   S(  3, -14),   S(  0,   0),   S(  0,   0),
            S(-17, -26),   S(-23, -24),   S(-11,  -9),   S( 16,  18),   S(  1,   0),   S(  7,   5),   S(  0,   0),   S(  0,   0),
            S(  8,  15),   S(  2, -10),   S( 33,  50),   S( 31,  47),   S( 34,  52),   S( 38,  50),   S( 13,   6),   S( -8, -20),
            S(-24, -33),   S(-18, -36),   S(-10, -16),   S( 13,  15),   S( 14,   7),   S( 23,  13),   S( 14,   6),   S(  6, -25),
            S(-21, -21),   S( -7,   4),   S( -1,  -2),   S( -3,   6),   S( 21,  35),   S( 33,  28),   S(-11, -17),   S( 31,  11),
            S(-41, -98),   S( -3,  -7),   S( -6,  -9),   S(  3, -13),   S( 16,   1),   S(  0, -15),   S(-11, -22),   S( 15,  20),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  4,   7),   S(-16, -29),   S(-18, -15),   S(  1,   0),   S(  0,  -3),   S( -3,  -6),
            S(  0,   0),   S(  0,   0),   S(  2,  -2),   S( -2, -21),   S( 12,   8),   S( -6, -18),   S( -7, -17),   S( -8, -20),
            S( -4,  -9),   S( 10,  11),   S( -2, -16),   S( -4, -24),   S( 39,  68),   S(  5,   4),   S(-11, -16),   S(  6,  14),
            S( -1, -23),   S( 21,  22),   S( 28,  49),   S(  1,  -2),   S( -4, -16),   S(-11, -24),   S( -2,  -8),   S(-13, -25),
            S(-19, -28),   S( -8, -21),   S( -6, -23),   S(  2, -13),   S( -2,   0),   S(-23, -65),   S(-11, -25),   S( -5,  -6),
            S(  2,  -4),   S( -1,  -1),   S(  7,   5),   S(-13, -26),   S( -5, -21),   S(-18, -26),   S(-20, -24),   S( -2,  -7),
            S(-17, -34),   S(  6,  12),   S( -2,  -6),   S( 11,  20),   S(-12, -24),   S(-11, -26),   S( -3,  -2),   S(-13, -35),
            S( -1,  -2),   S( -9, -17),   S(  8,  20),   S(  2,   5),   S(-19, -33),   S(-10, -30),   S( -9, -15),   S(-17, -37),

            /* queens: bucket 13 */
            S(-19, -53),   S(  0,  -7),   S(  0,   0),   S(  0,   0),   S( -6, -15),   S( -6, -21),   S( -5, -15),   S( -5,  -6),
            S(-10, -32),   S(  3,   4),   S(  0,   0),   S(  0,   0),   S(-19, -37),   S(-25, -53),   S(  3,   1),   S( -7, -23),
            S( -7, -11),   S(  3,  -9),   S( -3,  -7),   S(  5,   3),   S(-17, -42),   S( -1, -13),   S(-10, -21),   S( -8, -14),
            S( -5, -13),   S(-19, -53),   S( 12,   3),   S( 13,   9),   S(  4,  17),   S( 18,  30),   S( -8, -27),   S(-17, -37),
            S(  0,  -7),   S(  0, -18),   S(  1,  -8),   S( 21,  32),   S(  2,   4),   S( -3,  -4),   S(-14, -31),   S(-10, -32),
            S( 12,  11),   S(-29, -67),   S(-13, -19),   S( 15,  25),   S( 16,  14),   S(-12, -19),   S(-27, -61),   S(-16, -42),
            S( -5, -13),   S( -9, -15),   S( 10,  20),   S( -1,  -5),   S( -9, -23),   S( -7, -27),   S( -8, -13),   S( -7, -18),
            S(-14, -37),   S( -7, -22),   S(-10, -17),   S( -4,  -5),   S( -3,  -9),   S( -5, -17),   S( -4,  -9),   S(-15, -39),

            /* queens: bucket 14 */
            S( -1, -16),   S(  0, -11),   S( 14,   9),   S(  5,  -1),   S(  0,   0),   S(  0,   0),   S( 16,   9),   S(-12, -34),
            S(  0, -14),   S(-16, -44),   S(  0,  -5),   S( -3, -18),   S(  0,   0),   S(  0,   0),   S(  5,  -1),   S(-13, -31),
            S( -6, -12),   S( -2, -25),   S(-12, -29),   S( -4, -14),   S(  2,   5),   S(  3,   5),   S(-12, -29),   S(-11, -35),
            S(  2,   1),   S( -4,  -7),   S(  4,  -1),   S(-12, -29),   S(  1,  -3),   S(-22, -56),   S( -1, -21),   S(  6,  -1),
            S( -5,  -4),   S( -2, -10),   S( -5, -26),   S( 12,  28),   S( 27,  40),   S( -4, -10),   S(  2,  -3),   S(-13, -34),
            S(-11, -24),   S( -6, -32),   S( -6,  -9),   S(-22, -47),   S(  6,  12),   S(-11, -12),   S( -2,   2),   S(-22, -53),
            S(-13, -22),   S(  0,  -8),   S( -3,  -8),   S( -3,  -9),   S( -7, -10),   S(  4, -10),   S(-25, -57),   S( -6, -12),
            S(-24, -51),   S(  4, -13),   S(-12, -24),   S( -5, -14),   S( -7, -12),   S(-12, -24),   S( -4,  -8),   S( -9, -22),

            /* queens: bucket 15 */
            S(  0,  -4),   S( -8, -19),   S( 17,  20),   S(-14, -20),   S(  5,  -1),   S(  1,  -3),   S(  0,   0),   S(  0,   0),
            S( -3, -11),   S( -6, -25),   S(-12, -24),   S(-13, -38),   S(  8,  12),   S(  1,   3),   S(  0,   0),   S(  0,   0),
            S( -3,  -7),   S(  6,  12),   S( -6,  -4),   S( -7, -31),   S(  4,   9),   S( -1,  -4),   S( -2,  -4),   S( -1,  -1),
            S( -6, -16),   S( -8, -18),   S( -1,  -5),   S( 14,  19),   S( 17,  32),   S(  8,  24),   S(  5,   8),   S( -6, -17),
            S( -3,  -8),   S( -2, -10),   S(-13, -38),   S(-15, -28),   S( 14,  30),   S(  7,   8),   S( -8, -22),   S(-22, -48),
            S( -6, -16),   S(-12, -27),   S(-19, -41),   S( -2,  -9),   S( -1,   0),   S(-14, -31),   S(  3,   5),   S(-13, -26),
            S( -2,  -2),   S(  2,   3),   S(-17, -24),   S( -3,  -2),   S(-10, -12),   S( -6, -10),   S( -5,  -9),   S( -3, -14),
            S( -7, -12),   S(-28, -65),   S(  8,   7),   S( -9, -14),   S(-16, -26),   S( -6, -19),   S(  1,  -4),   S( -4, -10),

            /* kings: bucket 0 */
            S(  3, -31),   S( 27,  -3),   S( 10,   0),   S(-19,  -1),   S(-21,   1),   S( 23, -26),   S(  8,   4),   S( 20, -56),
            S(-20,  27),   S(-12,   9),   S(  1,   7),   S(-44,  17),   S(-51,  52),   S(-13,  17),   S( -8,  47),   S(-11,  31),
            S( 21,  10),   S( 66, -23),   S( -2,  -3),   S( -8, -12),   S(-48,   5),   S(-14,  -1),   S(-30,  29),   S(  6, -24),
            S(-27, -37),   S( 12, -20),   S( -5,  -4),   S(-31,  14),   S(-50,  20),   S(-44,  23),   S(-56,  16),   S(-18,  14),
            S(-40, -81),   S( 23, -24),   S(  4, -31),   S(  3, -10),   S(-17, -10),   S(-58,  10),   S( 11, -10),   S( 13, -15),
            S( -4, -93),   S( 18, -59),   S( 37, -68),   S( -1, -27),   S(  7,  -5),   S( 16, -41),   S( 22,   5),   S(  3,   9),
            S(  0,   0),   S(  0,   0),   S(  8, -20),   S( 16, -26),   S(  7, -16),   S(  1, -17),   S( 21, -17),   S(-21, -39),
            S(  0,   0),   S(  0,   0),   S(-13, -56),   S( 10, -20),   S(  7, -18),   S(  9, -10),   S( 15,  -3),   S( 13,  19),

            /* kings: bucket 1 */
            S( 29, -37),   S( 29, -10),   S(  7, -15),   S( 28,  -8),   S(-15,   1),   S( 31, -10),   S(  6,  22),   S( 25, -22),
            S(-23,  10),   S( 15,  13),   S(  7,  -4),   S(-43,  28),   S(-42,  30),   S(  2,   9),   S(  2,  23),   S( -4,  23),
            S(-39, -11),   S( 16, -10),   S( 16, -16),   S( 15, -19),   S(-18,  -5),   S(-10, -11),   S( 32,  -4),   S( 49, -12),
            S( 15, -11),   S( 22,  -8),   S( 39, -10),   S(-24,  18),   S( -1,  25),   S(-37,  11),   S(-15,   5),   S(-67,  39),
            S(-15, -36),   S(  8, -20),   S( 33, -31),   S( 17, -18),   S(  8,  -3),   S(  4, -18),   S( 15,   0),   S( 11,  -7),
            S(  2, -14),   S( 40, -43),   S( 24, -29),   S( 38, -12),   S(  3, -11),   S(  3,  -9),   S(  6,  10),   S( -2,  -1),
            S(  7, -47),   S( 17,  14),   S(  0,   0),   S(  0,   0),   S( -7,  13),   S(-10,  -3),   S( 19,  67),   S(-23, -50),
            S(-14, -126),  S( -7,  -2),   S(  0,   0),   S(  0,   0),   S( -1, -36),   S(  5,  -4),   S( -4,  16),   S( -3, -26),

            /* kings: bucket 2 */
            S( 40, -83),   S(  7,  10),   S(  8, -20),   S( 32, -20),   S(-18,  17),   S( 43, -28),   S(  9,  23),   S( 27, -18),
            S( 33, -26),   S( -4,  37),   S(-12,   6),   S( -4,   6),   S(-22,  13),   S(-12,   3),   S( 10,  12),   S( -8,  13),
            S(-34, -17),   S(-13,   4),   S( 13, -19),   S(-14, -16),   S( -1,  -4),   S( 17, -26),   S( 23, -11),   S( 18, -13),
            S(-25,  27),   S(  2,   0),   S( -3,  11),   S(-19,  16),   S( 17,   6),   S(-21,  -4),   S( 32, -20),   S( 34, -21),
            S(-25,  -9),   S( 37,  -4),   S( -8, -18),   S( -7, -16),   S( 26, -32),   S(-24, -24),   S( 42, -29),   S(  6, -39),
            S( 18,  11),   S(  3, -14),   S( 18, -16),   S( 42, -22),   S( 45, -23),   S( 37,   0),   S( 52, -35),   S( 19, -24),
            S(-11, -24),   S( -6,  26),   S(-17, -21),   S( 22,   9),   S(  0,   0),   S(  0,   0),   S( 37,  20),   S( -9, -31),
            S( -9, -34),   S(  6, -25),   S(  0, -34),   S(  7,   9),   S(  0,   0),   S(  0,   0),   S(  4,   0),   S(-19, -138),

            /* kings: bucket 3 */
            S( 12, -72),   S(  7,  -5),   S( 18, -36),   S(-12,  -6),   S(-16, -21),   S( 34, -31),   S(  3,  17),   S(  6, -27),
            S(-12,  19),   S(-24,  38),   S( -1, -10),   S(-35,   9),   S(-52,  21),   S( -1,  -6),   S( -3,  12),   S(-19,  18),
            S( -7, -13),   S(  5,  -7),   S(  9, -17),   S(-21, -17),   S( -4,  -1),   S( 32, -31),   S( 46, -25),   S( 59, -28),
            S( -9,   7),   S(-110,  26),  S(-91,  21),   S(-73,  19),   S(-43,   4),   S(-65,  -2),   S(-82,  12),   S(-58, -12),
            S(-40,   4),   S(-20,  -7),   S(-38,   0),   S(-56,   0),   S(  3, -35),   S( -2, -41),   S(  8, -43),   S(-34, -59),
            S(-21,  -7),   S( 15, -21),   S( 11, -22),   S(-37, -18),   S( 38, -38),   S( 83, -51),   S(124, -57),   S( 49, -91),
            S(-38,  -3),   S( 26, -17),   S(  5, -26),   S( 16, -24),   S( 24, -19),   S( 42, -58),   S(  0,   0),   S(  0,   0),
            S(-11, -16),   S( -1, -26),   S( -3,  -4),   S(-10, -18),   S( -4, -101),  S( 14, -15),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-34,  -9),   S( 18,  17),   S( 17,  21),   S(  4,  10),   S( -4,  -4),   S( -8,   1),   S(  2,  21),   S( 34, -26),
            S(-71,  36),   S( 13,  26),   S(-40,  15),   S(  6,  -6),   S( 77, -24),   S(  2,  15),   S( 70, -14),   S( 17,   4),
            S( 24,   8),   S(-35,   1),   S(-25,   6),   S( -4,   5),   S(-18,   6),   S( 10, -28),   S(-37,   3),   S(  6,  -2),
            S(  1,  -2),   S( 10,  -7),   S( 41,  -6),   S( -7,   2),   S( -9,  -4),   S(-11,  21),   S(-20,  14),   S( 14,  -7),
            S(  0,   0),   S(  0,   0),   S( 10,  -9),   S( -5,   3),   S(-21,   4),   S(-24,  -6),   S(-35,  -6),   S(  1,  -3),
            S(  0,   0),   S(  0,   0),   S( -3,  -6),   S( -1,   4),   S( 14,  12),   S( 15, -12),   S( -1, -18),   S(-11,   7),
            S(  1,   5),   S(  4,   8),   S( -7, -57),   S(  8,   4),   S( 14,  26),   S(-20, -18),   S( -7,   0),   S(  9,  13),
            S( -4,  16),   S(  1,  16),   S( -4, -26),   S(  4,  15),   S( 11,  -2),   S( -4,   6),   S( -4, -35),   S(  5,  19),

            /* kings: bucket 5 */
            S(  9, -20),   S(-16,  17),   S(-30,  16),   S(-56,  19),   S(-29,  14),   S(-14,  13),   S( 53,   0),   S( 43,  -9),
            S(-13,   7),   S( 65,  -1),   S( 17,  -3),   S( 18,  -2),   S( 26,  -8),   S( 39,  -9),   S( 22,  13),   S( 60, -13),
            S( -4,  -5),   S(-28,   5),   S(-22,  -7),   S(-33,  -6),   S(-11,   3),   S(-58,   3),   S( -9,   8),   S( 12,   3),
            S(-14,   6),   S( 41, -13),   S( 31,  -5),   S( 24,  17),   S( 30,  14),   S(  9,   1),   S( 39,  -9),   S( -2,   0),
            S(-16, -16),   S(-17, -36),   S(  0,   0),   S(  0,   0),   S(-13, -21),   S(  3,  -3),   S( -9,   4),   S( -8,   2),
            S(-22,  -1),   S(-20,   0),   S(  0,   0),   S(  0,   0),   S( -7,   7),   S(-16,  13),   S(-22,  10),   S( -7,   0),
            S( -8,   4),   S(  2,  20),   S( -1,  17),   S( -1,  -2),   S(  3,  26),   S( -6,  21),   S( -9,  47),   S(  5,  11),
            S(-12, -12),   S(  7,  18),   S( -2,  20),   S(  8,  24),   S( -6,   8),   S( -9,   7),   S( -3, -13),   S( -3, -13),

            /* kings: bucket 6 */
            S( 43, -35),   S( 35,  -9),   S(  3,  -5),   S(  7,   9),   S(-14,  15),   S(-26,  15),   S( 25,  18),   S( 26,   2),
            S( 46, -17),   S( 32,  10),   S(  6,  -7),   S( 45, -17),   S( 20,   1),   S(  2,   7),   S( 25,   9),   S(-11,  13),
            S( 22, -13),   S(-24,  14),   S( -4, -11),   S(  0, -12),   S(-18,   0),   S(-46,  -2),   S(-16,  10),   S(-60,  22),
            S( 17,   5),   S( 12,  -1),   S(  7,  -4),   S( 31,   4),   S( 70,   4),   S( -4,  -3),   S( 71, -18),   S( 30, -10),
            S(-11, -15),   S(-27,   6),   S(  6, -17),   S( 16,  -3),   S(  0,   0),   S(  0,   0),   S(-13, -14),   S(-62,  -5),
            S(-18,   5),   S(  1,   5),   S(-14,   2),   S(-22,  -4),   S(  0,   0),   S(  0,   0),   S(-27,  14),   S(-41,  -3),
            S(  1, -26),   S( -7,  24),   S( -9,   6),   S( -1,  20),   S( -4,  -3),   S(  2, -21),   S(-20,  -9),   S(-30,  -8),
            S(  6,  14),   S( -6,  13),   S(  6,  24),   S( -2,   1),   S( -2,  36),   S( -2,   9),   S(  6,  30),   S( -6,  24),

            /* kings: bucket 7 */
            S(-28,  14),   S(  5,  -5),   S(-14, -13),   S(-25,  11),   S(-16,   5),   S(-49,  34),   S( -5,  29),   S( -7,  17),
            S( 51, -11),   S( 69, -35),   S( 27, -17),   S( 11,  -7),   S(-14,   6),   S(-25,  18),   S(  0,   7),   S( 10,  13),
            S( 36, -27),   S(-10,   0),   S(-28,  -5),   S(-11, -11),   S(-11, -10),   S(-54,  12),   S(-19,  10),   S(-65,  20),
            S(  4,  28),   S(  9,  -8),   S( -9,   7),   S(  8,   6),   S(  3,   7),   S( 44, -18),   S( 60, -16),   S( 27, -16),
            S( -1,   2),   S(-21,  14),   S(-23, -11),   S(-20,   5),   S(-19, -11),   S( 10, -28),   S(  0,   0),   S(  0,   0),
            S( -4, -45),   S(  5,  -8),   S( 31, -20),   S( 14, -13),   S(-11,  -9),   S(  7,  10),   S(  0,   0),   S(  0,   0),
            S(  7,   2),   S( 24,  -2),   S(  5,   6),   S(-19, -15),   S( 16, -17),   S( -3,  -3),   S(  6,  -6),   S(-10, -36),
            S(  5,  20),   S(-11, -36),   S( 30,  18),   S(  4, -34),   S(  9,  12),   S( -5, -38),   S(  6,  37),   S(-10, -17),

            /* kings: bucket 8 */
            S( -5, 101),   S(-35,  40),   S(-30,  51),   S(-10, -17),   S(-13,  -5),   S(-18,  -1),   S( 27, -13),   S(  4,  23),
            S( 21, 104),   S( 23,   8),   S( 11,  62),   S( -6,  12),   S(  7,  14),   S(-11,   9),   S(  6,   0),   S( 28,  24),
            S(  0,   0),   S(  0,   0),   S( 19,  62),   S( 14,  -3),   S( 23,   6),   S( 12, -14),   S(  5,   7),   S(  3,   1),
            S(  0,   0),   S(  0,   0),   S( 13,  24),   S( 29, -28),   S(  7,  31),   S( 15, -24),   S( 14,   9),   S( -1,  21),
            S(  3,   0),   S(  1,   2),   S(  1, -29),   S( 12,  -2),   S( -6, -17),   S(  0,  -3),   S(  2,  32),   S( -7, -32),
            S( -1,  13),   S( -3, -15),   S( -3,  -8),   S(  6, -17),   S(-16, -29),   S(-11,  -3),   S( -2, -15),   S( -1,   0),
            S(  1,  -8),   S( -3,  -5),   S(  7, -11),   S(-10, -11),   S(  4, -26),   S(  7, -15),   S( 13, -13),   S(  6, -27),
            S( -2, -10),   S( -5, -32),   S(  0, -17),   S( -2,  -3),   S(  7,  22),   S(-11, -26),   S(  7,   8),   S( 11,   7),

            /* kings: bucket 9 */
            S(-26,  62),   S(-25,  28),   S(-55,  37),   S(-73,  21),   S(-51,  32),   S(-36,  15),   S( 39,   4),   S( 29,  19),
            S(-21,  23),   S( 30,  24),   S( -8,   8),   S( 24,  26),   S( 31,  25),   S( 11,   1),   S( 16,  19),   S( 39,  -5),
            S( -5,  17),   S(  2,   8),   S(  0,   0),   S(  0,   0),   S( 10,  17),   S(-31,   3),   S( 15, -10),   S(-17,  17),
            S( -3, -11),   S(  5,  -7),   S(  0,   0),   S(  0,   0),   S( -3,   7),   S( 38,  -2),   S(  8,   3),   S( -7,  32),
            S(-12,   0),   S(  4,  12),   S( -3,  -4),   S(  3, -21),   S( 12, -16),   S( 14,  -5),   S(-10,  14),   S( -1,   0),
            S(  0,  24),   S( -8,  19),   S(  7,  37),   S( -7, -13),   S(  4,  -7),   S(-13,  24),   S(-31, -10),   S(  1,  37),
            S(  4,   1),   S(  1,  -9),   S(  2,   2),   S(  5,  -1),   S(  7,  14),   S( 21,  26),   S( -8, -30),   S(  1,  14),
            S(  7,  30),   S( -1,   3),   S( 13,   2),   S( -1, -23),   S(  0, -32),   S( -4,   8),   S(  9,   9),   S(  8,  29),

            /* kings: bucket 10 */
            S(  7,  35),   S(  0,   9),   S( -3,  16),   S(-38,  16),   S(-66,  21),   S(-131,  49),  S(-14,  36),   S(-96,  80),
            S( 23, -10),   S( 23,  12),   S( 11,  -2),   S( 13,  10),   S( 85,  14),   S( 38,   2),   S(  5,  30),   S(-15,  26),
            S( -9,   9),   S( 16,   7),   S( 20, -16),   S( -9,  15),   S(  0,   0),   S(  0,   0),   S(  4,  17),   S(-48,  13),
            S(  5,   5),   S(  3, -19),   S( -3,  -7),   S( 25,  15),   S(  0,   0),   S(  0,   0),   S( 21,  21),   S( 40, -12),
            S( 12,   9),   S( 23,  10),   S( -7,  -4),   S( -9, -33),   S(  0, -16),   S(  9,  27),   S( 16,  19),   S(-35,  23),
            S(-16,   8),   S( 13,  24),   S( -5,  19),   S(  4,  22),   S( -2,   9),   S( -7, -19),   S(-12,   8),   S( -7,  -4),
            S(  8, -21),   S( 10,  15),   S( 22,  11),   S( 17,   4),   S( 10,   7),   S(  2,  12),   S( 11,  -5),   S(  2,  32),
            S( -7, -19),   S(  9, -20),   S(  0, -35),   S(  2,  13),   S( -1, -13),   S( -3, -32),   S(-10, -32),   S( -5, -11),

            /* kings: bucket 11 */
            S(-37,  19),   S(-15,   5),   S(  2,  -1),   S(-24,   3),   S(-18,  -3),   S(-149,  58),  S(-66,  46),   S(-119, 140),
            S( 24, -36),   S(-22,  25),   S(-10, -22),   S(  2,  15),   S( 39,  11),   S( 15,  55),   S( 42,   9),   S(  4,  49),
            S( -2,  10),   S( 20,  -1),   S( -5,  -6),   S(  4,   3),   S( 62, -15),   S( 11,  37),   S(  0,   0),   S(  0,   0),
            S(  2,   8),   S( 26,   2),   S(  6,  -2),   S( 41, -13),   S( 31, -25),   S( 38,  13),   S(  0,   0),   S(  0,   0),
            S(  1,  -3),   S(-14,  -9),   S(  9, -12),   S( 14, -16),   S( 28,  -5),   S(  1, -19),   S( -1, -23),   S( 12,  33),
            S( 12,  17),   S(  9,  10),   S(  7,  -1),   S( 10, -13),   S(  5, -19),   S(  4,  -4),   S( -1,  24),   S(-10, -34),
            S(  7, -36),   S(  2, -19),   S(  1,  23),   S( 11, -38),   S( 15, -19),   S(  5,  12),   S( 10,  -6),   S( -9, -11),
            S(  7,  33),   S(  9,  -7),   S(  0, -14),   S( 10,   2),   S( -3, -10),   S( -5, -15),   S(  4,  -5),   S(  6,  29),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( -3,  81),   S(-14,  -6),   S(  8,  37),   S( -8,  -8),   S( 35,   8),   S(-10,  49),
            S(  0,   0),   S(  0,   0),   S( 20,  93),   S( -7,  -3),   S( 12,  14),   S( 10,  12),   S( 17, -13),   S(-20,  19),
            S( -4, -10),   S(  2, -32),   S( 11,   3),   S(  9,  26),   S(  7, -12),   S(  8,   0),   S(-10, -21),   S( -4,  10),
            S( -3,  -6),   S(  3,   3),   S( -8, -12),   S( -2, -48),   S( -9, -10),   S( -2,  -5),   S( -7,  11),   S( -4,  18),
            S(  5,  21),   S( -3,   9),   S( -1,   8),   S(  4, -12),   S( -3,  24),   S( -7,   8),   S( -9,   4),   S( -2, -10),
            S(  4,  13),   S(  3,  13),   S( -6,  -2),   S( -9, -21),   S(  1,  15),   S(-10,  -2),   S(-14,  15),   S( -1,  20),
            S(  8,  30),   S( -6, -23),   S(  5,  18),   S( 11,  36),   S( -2, -17),   S( -3, -15),   S(  7,  -2),   S( -2, -27),
            S( -1,  -2),   S( -1,  -5),   S(  3, -12),   S( -2, -26),   S(  4,   2),   S(  6,  10),   S( -8, -18),   S( -7, -16),

            /* kings: bucket 13 */
            S( -6,  76),   S( -5,  63),   S(  0,   0),   S(  0,   0),   S( -2,  61),   S(-31,   2),   S( 27,   3),   S(-17,  29),
            S(  0,   8),   S(-10, -28),   S(  0,   0),   S(  0,   0),   S( 17, -14),   S(  4, -11),   S(-13,   6),   S( -5,  37),
            S( -3,  17),   S(  8,  51),   S(  0,  -9),   S( -3,  22),   S( -1,   7),   S(-10, -15),   S( -3,   3),   S(  3,   4),
            S( -8, -33),   S(  3,  30),   S(  3,  -3),   S( -6, -67),   S(  0, -32),   S( 17, -11),   S(-12,  -4),   S( -9,  20),
            S(  5,  11),   S(  0,  -5),   S(  8,  27),   S(-12, -10),   S(-17,   5),   S( -1,  12),   S(-15, -10),   S(  1,  31),
            S(  5,  18),   S( -3,  19),   S(-14, -11),   S( -3,  15),   S( -7, -19),   S(-16,   9),   S(-10,   4),   S(  2,   2),
            S( -1,   6),   S(-19, -39),   S( -8, -21),   S( -2, -11),   S( -8, -24),   S(  2,   5),   S(-17, -43),   S(  3,  33),
            S(  0,  -3),   S(  2,  19),   S(  0,  28),   S( 10,   4),   S(  5,  10),   S( -3,  -2),   S(-10, -25),   S(  3,  11),

            /* kings: bucket 14 */
            S(  6,  44),   S( -9,  14),   S(-13, -16),   S( -1,  59),   S(  0,   0),   S(  0,   0),   S(-15,  94),   S(-55,  88),
            S(-10,  13),   S(-11,  -9),   S(  7,  -9),   S( 19,  -4),   S(  0,   0),   S(  0,   0),   S( 17,  25),   S(-22, -12),
            S(-11, -17),   S( -9, -32),   S(  9,   5),   S(  2,  -6),   S( 11,  -6),   S(  5,  19),   S( 20,  52),   S(-34, -17),
            S( 14,  25),   S(  6,  12),   S( -2, -50),   S(  9, -11),   S(-23, -52),   S( 12,  17),   S(  3,  15),   S( -6,  -4),
            S( 10,  29),   S( -9,   5),   S(-11, -14),   S( -8, -13),   S( -6,  25),   S(  1,  15),   S( -3,  53),   S( -2,  -4),
            S( -7, -15),   S( -2,  26),   S(  2,   6),   S( -8,   5),   S( -4,   7),   S(  0, -21),   S(-10, -27),   S(  3,   7),
            S(  6,  33),   S(-13, -37),   S(  6,  27),   S(  7,   7),   S(  3,  -4),   S(  0, -31),   S( -2, -54),   S( 11,  48),
            S( -3,  -3),   S(  5,  28),   S( -3,  -3),   S( -6, -21),   S(  3,  39),   S( -6,  -4),   S(-15, -44),   S(  0, -24),

            /* kings: bucket 15 */
            S( 10,  43),   S( -5,  -3),   S(  6, -20),   S( -5, -12),   S(-37, -22),   S(-10, 109),   S(  0,   0),   S(  0,   0),
            S(-10, -15),   S(-22,  15),   S( -6, -21),   S( 16,  38),   S( 11, -17),   S( 23,  84),   S(  0,   0),   S(  0,   0),
            S(-12,  -8),   S( 10,  -2),   S(  3, -33),   S(  2,  12),   S( 10, -42),   S( 31,  61),   S(  9,  -7),   S(-11, -12),
            S(  2, -19),   S( -4,  32),   S(  2,   5),   S(  3, -21),   S( 10, -51),   S(  0,  -4),   S( 12,  53),   S( -2, -15),
            S(  9,  13),   S(-17,  16),   S(  1,   7),   S(-10, -32),   S(  4, -15),   S( -5,   5),   S(-15, -23),   S(  2,   5),
            S( -8,   3),   S(-21,  -7),   S( -4, -16),   S(  4,  20),   S( -2,  17),   S(  5,   9),   S(  0,  16),   S(  2,  -5),
            S(  0,  23),   S( -1,  -1),   S(  5, -11),   S(  1,  -5),   S( -3, -12),   S(  6,  35),   S(-12, -20),   S(  5,   5),
            S(  2,  28),   S(  9,  13),   S(  9,  28),   S(  6,  11),   S(  2,  18),   S(  2,  33),   S(  2,  14),   S(  6,   1),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-14, -185),  S(  6, -32),   S( 16,  38),   S( 24,  79),   S( 31, 101),   S( 35, 123),   S( 41, 122),   S( 56, 107),
            S( 82,  71),

            /* bishop mobility */
            S(  3, -73),   S(  5,   6),   S( 15,  51),   S( 21,  89),   S( 26, 116),   S( 28, 137),   S( 30, 147),   S( 30, 152),
            S( 30, 156),   S( 35, 153),   S( 46, 143),   S( 70, 136),   S( 87, 134),   S(103, 112),

            /* rook mobility */
            S(-22, -11),   S(-15,  56),   S(-14,  83),   S( -9,  98),   S(-12, 123),   S( -4, 131),   S(  0, 141),   S(  9, 139),
            S( 13, 148),   S( 20, 151),   S( 28, 152),   S( 44, 146),   S( 53, 143),   S( 71, 130),   S(152,  90),

            /* queen mobility */
            S( 43, -17),   S( 51, -66),   S( 44,  46),   S( 39, 123),   S( 41, 165),   S( 41, 200),   S( 39, 243),   S( 40, 261),
            S( 40, 288),   S( 39, 308),   S( 37, 323),   S( 34, 346),   S( 32, 360),   S( 27, 373),   S( 26, 385),   S( 26, 389),
            S( 26, 389),   S( 38, 378),   S( 48, 368),   S( 69, 351),   S( 95, 327),   S(133, 295),   S(177, 262),   S(195, 247),
            S(193, 242),   S(177, 242),   S(136, 216),   S(100, 198),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  7,  29),   S(-20,  60),   S(-35,  55),   S(-31,  70),   S( 12,  19),   S(-20,  38),   S(  3,  70),   S( 41,  32),
            S( 15,  41),   S(  3,  61),   S(-14,  53),   S(-14,  43),   S( -2,  43),   S(-21,  56),   S(-35,  80),   S( 31,  37),
            S( 25,  88),   S( 14,  91),   S( -4,  73),   S( 20,  57),   S( -4,  61),   S(-34,  81),   S(-38, 119),   S( -4,  89),
            S( 40, 129),   S( 44, 133),   S( 16,  99),   S(  4,  77),   S(  2,  72),   S( -1,  96),   S(-22, 127),   S(-61, 153),
            S( 44, 163),   S( 93, 172),   S( 81, 121),   S( 34, 108),   S(-70, 106),   S( 43, 103),   S(-93, 163),   S(-123, 176),
            S(106, 209),   S( 83, 239),   S(138, 222),   S(133, 213),   S(127, 230),   S(132, 238),   S(105, 234),   S(120, 243),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  7,  25),   S(  4,  -1),   S(  6,   7),   S( 10,  35),   S( 22,  38),   S( -1, -23),   S(-18,  39),   S(  8, -33),
            S( -8,  35),   S( 30,   3),   S( -2,  50),   S( 22,  42),   S( 38,   1),   S( -3,  39),   S( 18,  -4),   S(  8,  12),
            S( -4,  16),   S( 17,  11),   S( -1,  52),   S( 17,  70),   S( 28,  34),   S( 25,  28),   S( 26,   7),   S(  4,  20),
            S( -4,  60),   S( 36,  30),   S( 32,  84),   S( 11, 104),   S( 78,  65),   S( 61,  46),   S( 36,  53),   S( 17,  18),
            S( 62,  70),   S(113,  95),   S(130, 148),   S(154, 183),   S(148, 176),   S(136, 145),   S(180, 102),   S(129,  77),
            S( 74, 194),   S(121, 297),   S(122, 279),   S(121, 233),   S( 74, 161),   S( 52, 161),   S( 55, 203),   S( 29, 137),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  51),   S( 25,  35),   S( 40,  56),   S( 40,  38),   S( 31,  36),   S( 27,  41),   S(  7,  22),   S( 43,  15),
            S(  0,  20),   S( 22,  37),   S( 15,  36),   S( 12,  50),   S( 26,  17),   S(  7,  27),   S( 25,  23),   S( -2,  15),
            S(  5,   4),   S( 25,  26),   S( 52,  34),   S( 41,  33),   S( 41,  40),   S( 61,   9),   S( 16,  27),   S( 16,   4),
            S( 75,  35),   S(124,  -3),   S(122,  92),   S(177,  91),   S(156,  87),   S( 95,  89),   S(109,   8),   S(106, -28),
            S( 94,  58),   S(154,  59),   S(186, 128),   S(189, 131),   S(161, 175),   S(114, 155),   S(169, 106),   S(-18, 131),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 22,  35),   S(  3,  26),   S( 18,  29),   S( -9,  67),   S( 79,  35),   S( 20,   4),   S(  1,  -1),   S( 24,  20),
            S(  2,  13),   S(  6,   9),   S( 20,  16),   S( 17,  29),   S( 14,  15),   S(  1,  10),   S(  7,   6),   S( 28,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -13),   S( -6,  -9),   S(-20, -16),   S(-17, -29),   S(-14, -15),   S( -1, -10),   S( -7,  -6),   S(-28,   3),
            S(-22, -35),   S( -3, -26),   S(-18, -29),   S(  9, -67),   S(-79, -35),   S(-20,  -4),   S( -1,   1),   S(-24, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -22),   S(-11,  -8),   S( -5, -28),   S(-54,  -5),   S(-23, -15),   S(-18, -23),   S(  2, -24),   S(-13, -38),
            S(-13,   0),   S(-16,  -2),   S(-23,   8),   S(  4, -17),   S(-26, -15),   S(-31,  -2),   S(-40,   5),   S(-10, -15),
            S( -9, -23),   S( -2, -26),   S(-21,  -7),   S(-29, -10),   S(-23, -31),   S(-24, -11),   S(-28,  -3),   S(-41, -18),
            S(  4, -34),   S( 14, -36),   S( 18, -29),   S(  8, -26),   S(  8, -21),   S( 61, -34),   S( 43, -31),   S( -7, -45),
            S( 33, -63),   S( 54, -75),   S( 59, -33),   S( 65, -26),   S( 68, -34),   S( 78, -19),   S(151, -67),   S( 31, -70),
            S(131, -102),  S(166, -118),  S(102, -59),   S(100, -34),   S( 95, -22),   S(150, -40),   S(150, -50),   S( 82, -92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   2),        // attacks to squares 1 from king
            S( 17,   2),        // attacks to squares 2 from king

            /* castling available */
            S( 68, -68),        // king-side castling available
            S( 18,  43),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 15,  62),   S( 16,  68),   S(  6,  64),   S( -6,  78),   S(-15,  88),   S(-32,  97),   S(-65, 117),   S(-71, 121),
            S(-93, 128),   S(-70, 130),   S(-74, 138),   S( 10, 124),   S(-10, 127),

            /* orthogonal lines */
            S( -5, -53),   S(-54, -20),   S(-76, -12),   S(-90, -10),   S(-97, -14),   S(-98, -20),   S(-99, -25),   S(-96, -26),
            S(-114, -20),  S(-132, -20),  S(-153, -25),  S(-156, -32),  S(-181, -20),  S(-348,  32),

            #endregion

            /* tempo bonus for side to move */
            S(  7,  10),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
