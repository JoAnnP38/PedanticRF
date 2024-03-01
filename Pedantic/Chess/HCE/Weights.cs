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
        public const int MAX_WEIGHTS = 12713;
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
        public const int TEMPO = 12712;             // tempo bonus for side moving

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

        // Solution sample size: 6000005, generated on Thu, 29 Feb 2024 10:38:55 GMT
        // Solution K: 0.003850, error: 0.086367, accuracy: 0.4940
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 92, 179),   S(421, 528),   S(406, 627),   S(564, 957),   S(1293, 1703), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(101, -126),  S(152, -98),   S( 27, -36),   S(-26,  22),   S(-55,  32),   S(-20,  -9),   S(-49,  -8),   S(-45, -13),
            S(107, -122),  S( 99, -102),  S( -2, -63),   S(-28, -48),   S(-22, -22),   S(-28, -31),   S(-39, -32),   S(-39, -38),
            S( 97, -89),   S( 73, -59),   S( 16, -53),   S(  7, -63),   S( -8, -55),   S(  8, -62),   S(-21, -44),   S(-28, -39),
            S( 71, -45),   S( 49, -43),   S( 15, -45),   S(  8, -52),   S(-13, -42),   S(-29, -41),   S(-25, -41),   S(-34, -21),
            S( 65,  37),   S( 25,   7),   S( 53, -31),   S( 26, -66),   S( 19, -38),   S(-24, -32),   S(-29,  -1),   S(-42,  64),
            S( 63,  60),   S( 69,  62),   S(-23, -24),   S( 23, -42),   S(-45, -32),   S(-38, -11),   S( 26, -25),   S(-35,  17),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 32, -34),   S( 27, -41),   S( 43, -27),   S(-20,  17),   S(-10, -30),   S( 12, -25),   S(-37, -11),   S(-44,  16),
            S( 26, -42),   S(  8, -40),   S(  7, -50),   S(-11, -46),   S(-11, -26),   S(-12, -32),   S(-35, -24),   S(-51, -11),
            S( 16, -29),   S( 17, -27),   S( 24, -44),   S( 11, -48),   S(-16, -27),   S( 14, -42),   S(-20, -24),   S(-30,  -8),
            S( 23,   1),   S( 16, -33),   S( 25, -44),   S(  2, -37),   S(  0, -27),   S(  6, -42),   S(-36, -11),   S(-35,  19),
            S(  1,  58),   S(-25,   7),   S( -6,  -1),   S( -9, -18),   S( 37, -29),   S( -9,  -9),   S(-36,  18),   S( -8,  61),
            S( 48,  61),   S(  2,   1),   S(-77,   9),   S(-54,  25),   S(-48,   1),   S(-101,   3),  S(-62,  10),   S(-72,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27,  -3),   S(-34,  -1),   S(-15,  -6),   S(-18,   3),   S(  6, -13),   S( 36, -27),   S(  4, -45),   S(-17, -17),
            S(-27, -21),   S(-51,  -7),   S(-30, -39),   S(-23, -40),   S(  4, -31),   S(  1, -30),   S(-14, -35),   S(-31, -22),
            S(-40,  -4),   S(-32, -15),   S(-14, -45),   S(  0, -55),   S(  0, -24),   S( 14, -27),   S( -6, -26),   S(-20, -14),
            S(-53,  18),   S(-26, -29),   S(-30, -33),   S( -6, -44),   S( 12, -42),   S( -9, -19),   S(-12, -12),   S(-24,  15),
            S(-47,  66),   S(-73,  19),   S(-54, -11),   S(-55, -17),   S( 26,   3),   S(-27,  12),   S(-33,  32),   S(-36, 102),
            S(-87,  85),   S(-159,  68),  S(-98, -11),   S(-95, -15),   S(-47,   9),   S(-38,  18),   S(-56,  31),   S(-71,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -11),   S(-38,  -6),   S(-29, -10),   S(-12, -26),   S(-12, -11),   S( 31, -15),   S( 83, -67),   S( 58, -80),
            S(-36, -31),   S(-52, -22),   S(-34, -45),   S(-24, -31),   S(-20, -23),   S( -2, -33),   S( 54, -65),   S( 50, -74),
            S(-41, -24),   S(-20, -46),   S( -9, -61),   S( -2, -65),   S( -9, -47),   S(  6, -34),   S( 32, -48),   S( 53, -56),
            S(-44,  -4),   S(-18, -52),   S(-17, -61),   S(  2, -64),   S( 15, -62),   S( -3, -40),   S( 18, -33),   S( 46, -16),
            S(-22,  38),   S(-38,  -6),   S(-12, -38),   S(  2, -48),   S( 85, -44),   S( 53, -37),   S( 56,  40),   S( 41,  89),
            S(-87, 128),   S(-93,  38),   S(-62, -37),   S(-54, -46),   S(-19, -29),   S( 63, -38),   S(-12,  49),   S(  8,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -15),   S( 29, -43),   S(-13,   4),   S(-18,   2),   S( -9, -15),   S(-50,  18),   S(-89,  12),   S(-58,  -1),
            S( -5, -19),   S( 31,  -9),   S( 44, -43),   S( 19, -21),   S(-18, -18),   S(-64, -15),   S(-25, -37),   S( -7, -21),
            S( 39, -20),   S( 70, -21),   S( 19,  -6),   S( -9, -19),   S(-69, -10),   S( 17, -50),   S(-60, -18),   S(-40,  -6),
            S(  3,  38),   S( 10,  34),   S( 49,   1),   S(  7,   8),   S( 11, -32),   S(-54,  -8),   S( -5, -37),   S( 26, -29),
            S( 14,  91),   S(  4,  77),   S( -4,  28),   S(-19,  25),   S( 22,  35),   S(-24, -23),   S(-32, -20),   S( 48,  25),
            S( 44, 101),   S( 71, 109),   S( 42,   2),   S(  1, -19),   S(  8, -25),   S( 11, -20),   S( 19,  10),   S(-39,   9),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-105,  58),  S(-49,  21),   S(-26,  18),   S( 51, -15),   S( -1,  -4),   S(-13,   8),   S(-37,   3),   S(-20,  16),
            S(-88,  27),   S(-46,   4),   S(  5, -18),   S(  9,   7),   S( 18, -16),   S(-16, -14),   S(-25, -12),   S(-53,  18),
            S(-53,  42),   S(-21,  18),   S( 79, -39),   S( 21, -29),   S( 38, -13),   S(-40,   2),   S(  1,  -9),   S(-10,   1),
            S(-52,  63),   S(-17,  22),   S( 27,   1),   S( 51,  11),   S(  7,   7),   S(-27,   7),   S( 28, -14),   S(  8,  17),
            S( 57,  64),   S( 74,  23),   S( 68,  41),   S( 37,  40),   S(-19,  49),   S( 61,  -4),   S( -9,  15),   S(-12,  63),
            S( 85,  57),   S( 59,  36),   S( 67,  15),   S( 67,  -7),   S( 29,   7),   S(  0, -18),   S(-14,   4),   S( 43,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  19),   S(-28,   1),   S(  0,   0),   S(-31,   0),   S( 23, -28),   S(-19,  -5),   S(-50,  -2),   S(-82,  26),
            S(-38,  -5),   S( -7, -25),   S(-13, -36),   S( 34, -28),   S( 57, -30),   S( 29, -25),   S(-34, -12),   S(-83,  16),
            S(-42,   8),   S(  5, -14),   S(  5, -23),   S(-15, -11),   S( 32, -15),   S( 70, -33),   S(-13,  -8),   S(-47,  21),
            S(-25,  21),   S(-56,  12),   S( 12, -18),   S( 20,  -5),   S( 57,  -3),   S( 30,   3),   S( 26,  -4),   S( 33,  11),
            S(-21,  50),   S(-48,  22),   S(  8,  -4),   S( 21, -12),   S( 39,  43),   S( 75,  30),   S( 60,  10),   S( 92,  37),
            S( 20,  58),   S( 49,  -2),   S( 10, -37),   S( -3, -29),   S( 40, -13),   S( 47,   2),   S( 22,  15),   S( 65,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47, -20),   S(-58, -16),   S(  4, -24),   S(-52, -20),   S(-13, -25),   S( 39, -34),   S(  6, -53),   S(-33, -33),
            S(-54, -34),   S(-48, -38),   S(-28, -51),   S(-14, -46),   S(  4, -35),   S( 45, -46),   S( 54, -48),   S(-17, -34),
            S(-71, -19),   S(-53, -32),   S(-28, -47),   S(  4, -46),   S(  3, -25),   S( 20, -21),   S( 40, -44),   S( 30, -33),
            S(-43, -19),   S(-51, -35),   S(-45, -52),   S(-59,  -1),   S(  9, -24),   S( 23,  -5),   S( 46, -14),   S( 57, -11),
            S(-64,   2),   S(  3, -35),   S(-38, -18),   S(-18, -41),   S( 43,   5),   S(  2,  15),   S( 82,  57),   S(108,  59),
            S( 11, -10),   S(-25, -25),   S( 16, -48),   S(  5, -57),   S( 12, -37),   S( 18, -12),   S( 45,  59),   S( 51,  94),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36,  27),   S(-44,  47),   S( -1,   8),   S( -8,  -3),   S(  3,  23),   S(-58,  24),   S(-63,  -8),   S(-37,  14),
            S(-50,  20),   S(-10,  11),   S(-17,  10),   S(-32,  -3),   S(-65,   1),   S(-11, -30),   S(-51, -27),   S( -3,  -7),
            S(-12,  52),   S( 49,  63),   S( -3,  44),   S(-30,  26),   S(  1,  -2),   S(-51, -17),   S(-37, -31),   S( -2, -17),
            S( 24,  87),   S( 54, 114),   S( 38,  74),   S(  6,  40),   S(-14,  -3),   S(-33, -28),   S(  0, -23),   S(  1, -15),
            S( 36, 117),   S( 67, 119),   S( 53, 127),   S( 54, 104),   S( 17,  12),   S( 15, -28),   S( 20, -23),   S( 15, -17),
            S( 77, 155),   S( 99, 150),   S(101, 187),   S( 50,  79),   S( 10,  13),   S( -1, -10),   S( -2, -27),   S(  8,   9),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-77,  55),   S(-44,  32),   S( -9,  19),   S( 22,  40),   S( -8,   5),   S(-78,  33),   S(-83,  16),   S(-91,  29),
            S(-62,  21),   S(-39,   8),   S(-31,   9),   S( 13,  14),   S(-94,  11),   S(-57,   7),   S(-126,   6),  S(-58,   6),
            S(-56,  48),   S(-38,  62),   S(  2,  55),   S(-56,  64),   S(-54,  45),   S(-61,   9),   S(-41, -14),   S(-42,   4),
            S(  5,  68),   S( 64,  86),   S( 49,  95),   S( 86, 104),   S(-33,  64),   S(-36,  23),   S( 45, -10),   S( 30, -21),
            S( 93,  85),   S( 61, 133),   S( 90, 128),   S(130, 137),   S( 69, 110),   S( 34,  30),   S( 44, -29),   S( 36,   7),
            S( 29,  83),   S( 86,  98),   S(124,  96),   S(109, 139),   S( 53,  95),   S( 14,   9),   S( 14, -32),   S(  6,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-100,  11),  S(-86,   1),   S( -7,   6),   S( -1,  26),   S(-18, -15),   S(-126,  47),  S(-128,  27),  S(-90,  42),
            S(-104,  -9),  S(-30, -25),   S(-29, -10),   S(-57,   9),   S(-45,  31),   S(-38,  23),   S(-112,  20),  S(-104,  28),
            S( -4, -13),   S(-61,   5),   S(-35,   5),   S(-102,  71),  S(-103,  91),  S( 25,  30),   S(-77,  32),   S(-50,  41),
            S(-14,   1),   S(-10,  14),   S( 42,  17),   S( 15,  71),   S( 22,  92),   S(  8,  79),   S( -7,  39),   S( 38,  28),
            S( 57,  -3),   S( 12, -10),   S( 45,  54),   S( 66, 104),   S( 90, 140),   S( 64,  74),   S( 37,  78),   S( 89,  51),
            S( 44, -13),   S( 22, -16),   S( 30,  18),   S( 75, 106),   S( 56,  96),   S( 55,  56),   S( 45,  60),   S( 41,  89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-133,   8),  S(-73,  -1),   S( 27, -16),   S(-13,   0),   S(-18,   5),   S(-94,  41),   S(-54,  15),   S(-66,  31),
            S(-87, -14),   S(-73, -36),   S(-61, -38),   S(-75,  17),   S(-43,   6),   S( 13,   3),   S(-83,  43),   S(-99,  42),
            S(-57, -33),   S(-77, -33),   S(-22, -17),   S(-41,  -4),   S(-68,  40),   S(-12,  44),   S(-40,  56),   S(-74,  57),
            S( 23, -24),   S(-50, -35),   S(-17, -19),   S(-50,  16),   S(  8,  51),   S( 56,  50),   S( 31,  84),   S(116,  46),
            S( -6,   6),   S(-18, -19),   S( 10, -18),   S(-41,  -9),   S( 66,  99),   S( 64,  75),   S(152, 109),   S(146,  91),
            S(  0, -22),   S(  3, -31),   S( 12, -33),   S( 18,   2),   S( 41,  45),   S( 40, 113),   S( 73, 135),   S(120, 125),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1,   4),   S(-27, -21),   S(-26, -40),   S(  0,  -2),   S(-10, -16),   S(  1,  13),   S(-16,  -4),   S(-42, -16),
            S(-12, -20),   S(-10,  -5),   S( -1,  21),   S( 18,  13),   S(-18,  43),   S( -6,  -8),   S(-34, -18),   S(-21, -59),
            S(-17,  67),   S(  6,  49),   S( 20,  49),   S( -9,  10),   S( 17,   5),   S(-32, -32),   S(-39, -74),   S(-28, -54),
            S(-10,  84),   S( 22,  91),   S( 59, 112),   S( 40,  67),   S(-27, -46),   S(  0, -44),   S( 22,  -3),   S(-22, -62),
            S( 72,  88),   S( 78, 200),   S( 82, 134),   S( 27,  87),   S(  5, -22),   S( -2, -20),   S(  4, -45),   S(-10, -51),
            S( 52, 138),   S( 64, 246),   S(126, 273),   S( 45,  98),   S( -1,  -1),   S( -6, -65),   S( -5, -40),   S(-16, -88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30, -23),   S(-17,   0),   S(  8,  26),   S( -5,  -9),   S( -1,   2),   S(-25, -28),   S(-49, -45),   S(-34,  18),
            S(-13, -41),   S(-36, -31),   S(-19, -22),   S(  9,   6),   S(-22,  29),   S( 11,  -7),   S(-49, -44),   S(-24, -22),
            S(  8,  12),   S( 26, -22),   S(-11,  35),   S( 29,  52),   S(-11,  37),   S(-38, -37),   S(-31, -50),   S( -1, -63),
            S( -7,  43),   S( 43,  61),   S( 37,  86),   S( 30, 104),   S( 38,  47),   S( 23, -27),   S( 12, -18),   S( 32, -49),
            S( 39,  56),   S( 80, 142),   S( 99, 152),   S(103, 184),   S( 71, 142),   S( 11,  -4),   S(  8, -63),   S( 22, -47),
            S( 57,  80),   S( 99, 184),   S( 91, 206),   S(109, 252),   S( 66, 122),   S(  9,   8),   S(  7, -13),   S(  9, -32),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52, -40),   S(-41, -29),   S(-17, -46),   S( -2,  -1),   S( -4,  -3),   S(-56,   9),   S(-43, -11),   S( -9,  39),
            S(-28, -18),   S(-39, -47),   S(-39, -60),   S(  5,  30),   S(-14,  32),   S( 16,  -3),   S(-21,  15),   S(-16,  11),
            S(-10, -43),   S(-10, -22),   S(-23, -21),   S(-16,  -3),   S(-16,  40),   S( -4,   7),   S(-12,   2),   S( 13,  22),
            S(  5, -51),   S( 17, -26),   S( 19, -15),   S( 12,  46),   S( 19, 133),   S(  2,  84),   S(  2,  27),   S( 23,  26),
            S( 31, -30),   S( 23,   2),   S( 38,  18),   S( 36, 112),   S( 83, 187),   S( 89, 132),   S( 52,  71),   S( 55,  54),
            S( 25, -34),   S( 41,   9),   S( 49,  97),   S( 61, 152),   S( 76, 230),   S( 45, 171),   S( 48, 131),   S( 55,  91),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -60),   S(-25, -42),   S(-24, -63),   S( -1,   4),   S(  9,  25),   S(  6,  18),   S(-21, -14),   S( -2,  41),
            S( -5, -74),   S(-25, -11),   S( -8, -29),   S( 13,  16),   S(-27,   9),   S( 16,  43),   S( 14,  41),   S(-11,  19),
            S(-17, -61),   S(-13, -64),   S(-17, -56),   S( -1,  17),   S( 37,  27),   S( 14,  13),   S( 22,  55),   S( 16,  63),
            S( -5, -40),   S(-43, -47),   S( -7, -13),   S( 10,  16),   S( 13,  51),   S( 62,  76),   S( 19, 127),   S( 23,  83),
            S(-19, -83),   S(-13, -79),   S( -2, -13),   S( 12,  -7),   S( 47, 125),   S( 94, 142),   S( 56, 250),   S( 70,  87),
            S( 21, -23),   S( -3, -32),   S(  9,  10),   S(  5,  24),   S( 34,  98),   S( 98, 276),   S( 20, 207),   S( 47,  85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-36,  -3),   S( -7,   1),   S(-21, -22),   S( -4, -59),   S(-28, -29),   S(-26, -11),   S(-61, -43),   S(-17,   1),
            S(-45,  26),   S( 29, -49),   S(-26, -23),   S(  1, -21),   S( -3, -56),   S(-16, -10),   S(-23, -36),   S(-65, -49),
            S( -7,  60),   S( -9, -25),   S(  4, -33),   S( -7,  20),   S( 17,   5),   S(-31,  -5),   S( -8, -48),   S(-61, -17),
            S( 15, -37),   S( 20, -11),   S( 14,  17),   S( 19,  25),   S( 14, -21),   S( -9,  -4),   S(-28, -24),   S( 11, -32),
            S( 13, -43),   S( 32, -16),   S( 18,   5),   S( 73, -36),   S( 39, -10),   S( 35,  10),   S(  8, -13),   S(-56, -38),
            S(  0, -20),   S( -4,  12),   S( 31,  -4),   S( 58, -19),   S( 43, -69),   S( 32,  -3),   S( -9, -35),   S( 24, -50),
            S( 14, -24),   S( 59, -55),   S(  1, -39),   S( 28, -74),   S( 36, -58),   S(  0, -54),   S( 18, -28),   S(-17, -39),
            S(-102, -80),  S(-17,   2),   S(-28, -50),   S( -2, -65),   S(-38, -41),   S( 27,  28),   S(-14, -20),   S( 11, -12),

            /* knights: bucket 1 */
            S(-42,  28),   S(-61, 107),   S(  4,   8),   S(-19,  53),   S(-27,  39),   S(-19,  29),   S(-36,  65),   S(-31,  15),
            S( 30,  18),   S( 10,  19),   S( -5,   7),   S(-13,  27),   S( -6,  14),   S( -9,  12),   S(  1, -16),   S(-38,  15),
            S(-32,  25),   S( -2,  13),   S(  0,   6),   S( 20,  16),   S(  6,  24),   S(-29,  21),   S(-14,  -2),   S(-27,  17),
            S(  0,  24),   S( 31,  36),   S( 25,  35),   S( 19,  29),   S(  7,  25),   S(-13,  35),   S(  5,  10),   S(  5,  25),
            S( 26,  34),   S( 24,  -2),   S( 23,  27),   S( 34,  11),   S( 31,  31),   S( 16,  38),   S( 21,   1),   S(  5,  30),
            S( 19,  -1),   S( 48,  -6),   S( 33,  27),   S( 52,   4),   S( 51,  -2),   S( 50,  46),   S( 51,   5),   S(  3, -13),
            S( 52,  21),   S( 45,   9),   S( 20, -36),   S( 69,  -2),   S( 37, -20),   S( 33, -16),   S(-46,  -3),   S( 21, -33),
            S(-93, -80),   S(-23, -23),   S(-18, -21),   S( -9,  -4),   S(  4,   1),   S(-23, -20),   S(-10, -15),   S(-31, -42),

            /* knights: bucket 2 */
            S(-56,   0),   S( -2,  37),   S(-32,  56),   S(-31,  56),   S(-30,  44),   S(-41,  62),   S(-28,  47),   S(-47,  22),
            S(-29, -25),   S(-20,  10),   S( -7,  14),   S(-16,  26),   S( -8,  14),   S(-17,  28),   S(-46,  57),   S(-48,  75),
            S(-27,  24),   S( -9,  22),   S(-17,  39),   S( 17,  29),   S(  5,  25),   S( -8,  20),   S( -8,  29),   S(-43,  46),
            S(-13,  38),   S(-41,  40),   S(  0,  40),   S(  3,  43),   S( -8,  59),   S(-12,  44),   S(-17,  64),   S( -5,  55),
            S(  8,  26),   S(-26,  31),   S(-14,  54),   S(-31,  58),   S(-14,  55),   S(-13,  55),   S(-16,  33),   S(-20,  27),
            S(-30,  38),   S(  8,  13),   S(-20,  52),   S(-25,  47),   S(  5,  35),   S(  2,  41),   S(-30,  14),   S( 68, -28),
            S(  9,  30),   S(-54,  26),   S(-19,  13),   S(-31,  24),   S( 39, -18),   S( 17,  10),   S(-65,  43),   S(-44, -11),
            S(-139, -15),  S( -7, -20),   S(-61,  35),   S(-18,   4),   S(  9, -10),   S(-65,  -7),   S( 35,  25),   S(-215, -82),

            /* knights: bucket 3 */
            S(-60, -10),   S( -3, -13),   S(-48,  30),   S(-21,  26),   S(-29,  34),   S(-23,  26),   S(  1,   4),   S(-13, -27),
            S(-33,  32),   S(-56,  20),   S(-29,  11),   S(  0,  15),   S(  4,  19),   S(-32,  13),   S(-32,  23),   S(-39,  61),
            S(-22, -25),   S(-11,  20),   S(-14,  21),   S( 14,  28),   S( 13,  30),   S(  3,  22),   S( -7,  10),   S(-11,  53),
            S(-19,  14),   S(-14,  35),   S( -4,  60),   S(  8,  54),   S( 11,  54),   S(  7,  46),   S(  8,  37),   S( 10,  45),
            S( -6,  37),   S(-16,  38),   S( 12,  34),   S(  7,  66),   S( -1,  64),   S(  8,  67),   S( 10,  52),   S( -8,  40),
            S( 25, -12),   S( 29,  -7),   S( 36,  12),   S( 43,  26),   S( 51,   8),   S( 76,  -2),   S( 13,  15),   S(-16,  63),
            S(  2,   9),   S( -6,  23),   S( 41, -17),   S( 53, -10),   S( 47,  -8),   S( 72, -33),   S( 32, -51),   S( 21, -21),
            S(-159,  50),  S( -7,   6),   S(-30,   7),   S( 28,   9),   S( 48, -30),   S(  2,  -3),   S( 36,   3),   S(-86, -51),

            /* knights: bucket 4 */
            S(  1,  -9),   S(-51, -21),   S(-12,  25),   S( -3, -27),   S(  5, -31),   S(-20, -36),   S( 27, -39),   S(-26, -42),
            S( 49,  38),   S(-35,  -2),   S(  8, -26),   S( 19, -32),   S( 53, -48),   S(-21, -35),   S( 21, -25),   S(  3, -30),
            S(-34,  -7),   S( 38,  19),   S( 65, -24),   S( 60,   1),   S( -3,  -6),   S( -4,   6),   S(-23, -50),   S(-22, -91),
            S(-11,  42),   S( 45, -50),   S(110, -17),   S( 38,  17),   S( 27,  -8),   S(130, -30),   S( 44, -54),   S( 23, -27),
            S( 52,  25),   S(-21,  34),   S( 69,  25),   S( 27,  24),   S( 63,  12),   S( -6,   9),   S( -7, -54),   S(-19, -47),
            S(  1,  -2),   S(-45, -18),   S( 94,   0),   S( 58,  -7),   S( 17, -17),   S( 31,   2),   S( 32,  28),   S(-23, -35),
            S(-13, -12),   S(-17, -11),   S( -9,  12),   S(  4,  16),   S(  5, -26),   S(  6, -27),   S( -4, -17),   S(-19, -11),
            S(-21,  -5),   S(  1, -13),   S(  8,  19),   S( -3, -20),   S( -7,  -8),   S( 16,  39),   S(  0,   2),   S( -2, -12),

            /* knights: bucket 5 */
            S( 25,  16),   S(-32,  36),   S( 36,  44),   S( 42,  35),   S( 24,  22),   S( 14,  -6),   S( -7,  24),   S( -7,   0),
            S(  7,  -3),   S( 33,  44),   S( 21,  13),   S( 25,  14),   S( 75,  14),   S( 31,  24),   S( 31,  19),   S(-32, -31),
            S( 18,  24),   S(-24,  38),   S( 33,  24),   S( 73,   7),   S( -7,  28),   S(-11,  30),   S( 30, -16),   S(-30,  -4),
            S( 66,  31),   S( 12,  30),   S( 63,  19),   S( -7,  43),   S( 19,  31),   S( 36,  28),   S( 48,  36),   S( -8,  35),
            S( 34,  43),   S( 33,  23),   S( 77,  36),   S( 83,  26),   S(112,  29),   S( 35,  38),   S( 51,  27),   S( 35,  29),
            S( -8,  43),   S(-14,  47),   S( 56,  20),   S( 59,  41),   S( 36,  39),   S( 16,  44),   S(  7,  -6),   S( -9,  16),
            S( 17,  31),   S(-10,  47),   S( 27,  39),   S( 24,  61),   S(  8,  37),   S(  9,  43),   S(  7,  37),   S(  4, -12),
            S( 30,  40),   S(-11, -16),   S( 15,  33),   S(  5,   4),   S( 19,  38),   S(  2,  26),   S(  9,  38),   S(-17, -28),

            /* knights: bucket 6 */
            S( 13,  -5),   S(  4, -18),   S( 31,  11),   S(-18,  37),   S( 10,  14),   S( 37,  40),   S(-31,  21),   S(-17,  35),
            S(-25, -46),   S( 74,  -4),   S( 37,  -2),   S( 12,  12),   S(-35,  54),   S( 76,  17),   S( 31,  26),   S( 12,   7),
            S(-27,  -4),   S( 22,   4),   S( 29,  20),   S( 56,  14),   S( 35,  22),   S( -6,  35),   S( 41,  31),   S( 11,  55),
            S( 31,  10),   S( 95,  -9),   S( 85,  22),   S( 94,  10),   S( 67,  15),   S( 40,  33),   S( 29,  49),   S(  8,  59),
            S( 32,  24),   S(101, -16),   S(106,   8),   S(100,  18),   S(133,  16),   S(149,   8),   S( 60,  38),   S( 48,  40),
            S( 32,  27),   S( 40,   2),   S( 51,  39),   S( 62,  33),   S( 80,  41),   S( 18,  30),   S( 25,  19),   S( 47,  53),
            S(  8,  28),   S(-13,  23),   S(-19,  41),   S( 23,  27),   S(  9,  37),   S( 23,  59),   S( 35,  84),   S( -9,  11),
            S(-40, -18),   S( 16,  66),   S( 21,  22),   S(  0,  22),   S( 24,  41),   S( 11,  48),   S( 12,  23),   S(  3,   5),

            /* knights: bucket 7 */
            S(-42, -53),   S(-207, -10),  S(-40, -46),   S(-44, -36),   S(  9, -17),   S(  0, -34),   S( 14, -21),   S(-23, -12),
            S(-19, -65),   S(  6, -53),   S(-45, -37),   S(-16, -14),   S(-32,   5),   S( 29, -31),   S( 31,  15),   S( 39,  34),
            S(-85, -63),   S(-51, -15),   S(-26,   3),   S( 76, -40),   S( 13,  -8),   S( 32, -12),   S(-26,  58),   S( 37,  47),
            S(-68, -15),   S( 34, -31),   S(  5,  -1),   S( 80,  -9),   S( 89, -16),   S( 30,  -4),   S( 24,  -5),   S(-22,  30),
            S(-59, -30),   S(-16, -52),   S( 69, -35),   S(107, -28),   S(109,   6),   S( 79,  18),   S(101,  -4),   S( 92,  10),
            S(-14, -67),   S( -2, -39),   S( 17,   0),   S( 49,  -2),   S( 83,   2),   S(122, -18),   S(101, -38),   S(-14,  25),
            S(-30, -30),   S(-53, -18),   S( 19, -23),   S( 35,   8),   S( 59,  12),   S( 56,   9),   S( -5, -12),   S( 10,  -9),
            S(-53, -42),   S(  2, -10),   S(-27, -14),   S( 14,   9),   S(  9,  -4),   S( -7, -22),   S( -5, -16),   S(-14,   7),

            /* knights: bucket 8 */
            S(  2,   1),   S( -4,  -7),   S(  1,  15),   S(-12, -33),   S( -8, -23),   S( -8, -39),   S(-13, -13),   S(-10, -42),
            S( -1,  -8),   S( -8, -10),   S(-13, -12),   S(-26,  -2),   S(-44, -32),   S(-14, -58),   S( -4, -37),   S(-21, -49),
            S(  4,  23),   S(-15, -36),   S( 29,  20),   S(-15, -12),   S(  0, -29),   S(-11,   7),   S(-17, -41),   S( -4, -25),
            S(-16,  -2),   S(-10,  -2),   S( -3,  27),   S(-17,   6),   S( 28,  18),   S(  2, -20),   S( -7, -40),   S( -3, -21),
            S( 35,  98),   S( -2,  20),   S( 24,  24),   S( 13,  43),   S( 17,  42),   S( -2, -20),   S(  8,  -6),   S(-11, -29),
            S(  6,  44),   S(  1,  27),   S( 32,  10),   S( 31,  -5),   S(-10,   3),   S(  2,  -3),   S( -6, -49),   S(-12, -10),
            S( -1, -15),   S( 10,  12),   S( -1,  -3),   S( 14,   5),   S( 24,  25),   S(  8,  16),   S( -3,  -7),   S( -4,  -3),
            S(  4,  -1),   S( 16,  37),   S( -1,  11),   S(  0,  13),   S(  3,  13),   S( -5, -23),   S(  2,  14),   S( -2,  -6),

            /* knights: bucket 9 */
            S( -9, -44),   S(-31, -54),   S(-23, -57),   S( -7, -16),   S(-23, -59),   S(-33, -89),   S( -4,  -6),   S(  3,  15),
            S( -6, -29),   S(-15, -11),   S(-15, -47),   S(-11,   7),   S( -3, -17),   S(  5,   1),   S( -1, -10),   S( -8, -14),
            S(  8,  35),   S(-17, -36),   S(  2,  -9),   S( -1,  -3),   S( 12,  14),   S(-31,  10),   S(-21,  -9),   S(-14,   0),
            S(-11, -32),   S(-11,   0),   S( 14,  19),   S( 32,  19),   S( 22,   7),   S( 11,  19),   S(-15, -58),   S( -6, -36),
            S( -3,  27),   S(  8,   8),   S( 16,   9),   S( 10,  27),   S( 10,  22),   S( -2,  -7),   S(  7, -14),   S(  0,  -2),
            S(  7,  25),   S( 13,  32),   S( 11,  30),   S( -3,   2),   S( 36,  28),   S( -3,   9),   S( 14,  58),   S(-10, -24),
            S( -2,   3),   S( -7,  22),   S( 18,  44),   S(  1,  19),   S(  3,  31),   S( -2, -21),   S(  6,  35),   S( -4,   7),
            S(  0,  -1),   S( 10,  32),   S( 14,  35),   S( 10,  62),   S( 11,   4),   S(  1,  19),   S(  6,  24),   S(  2,  10),

            /* knights: bucket 10 */
            S(-16, -38),   S( -9, -21),   S(-16, -54),   S(-25, -39),   S(-13, -11),   S(-16, -61),   S( -4,  -8),   S( 12,  37),
            S( -3, -21),   S( -9, -41),   S( -2,  -7),   S(-28, -47),   S(-20, -45),   S(-14, -95),   S(-13,  -8),   S( -5, -19),
            S(  0, -32),   S(-17, -65),   S( -6,  18),   S(  1, -15),   S( 17,  15),   S(-28, -16),   S(-22, -33),   S( -3,  -3),
            S(  3,   2),   S( -7, -32),   S( -9, -37),   S( 36,  -1),   S( 24,  -1),   S( 30,   8),   S(  2,  -4),   S( 20,  68),
            S(-18, -71),   S(-20, -37),   S( 11,   3),   S( 61,  10),   S( 32,   1),   S( 26,   4),   S( 13,   3),   S( 21,  57),
            S(-15, -16),   S( -1,  -3),   S(-17, -17),   S( 14,  30),   S(  5,  48),   S( 22,  37),   S( 25,  55),   S( 26,  82),
            S(  6,  12),   S(-18, -32),   S( -6, -29),   S( 19,  55),   S(  3,  40),   S(  8,  24),   S( -3, -31),   S(  9,  25),
            S(-10, -41),   S( -4, -23),   S(  0,  11),   S(  1,  -3),   S( 13,  54),   S(  5,  52),   S( -2,   9),   S( -1,  -7),

            /* knights: bucket 11 */
            S(  7,  26),   S(-20, -39),   S(-16, -59),   S(-18, -27),   S(-16, -38),   S(-12, -20),   S( -1, -19),   S( -7,  -9),
            S(-11, -35),   S(-15, -52),   S(-24, -102),  S(  2,  -3),   S(-11, -28),   S(-23, -53),   S(-12, -21),   S(-10,  -9),
            S(-15, -49),   S(-19, -42),   S(-43, -23),   S( -4,  11),   S(-24,  16),   S( -2,   4),   S( 15,  -4),   S( -3,  -4),
            S(-20, -30),   S( -4, -35),   S(-40,  -1),   S(  6,  25),   S( 33,  11),   S( 21,  13),   S( -3,  23),   S(  1, -13),
            S(-11, -42),   S(-23, -58),   S( 12, -13),   S( 36, -20),   S( 25,  23),   S( 43,  37),   S( -2, -32),   S( 24,  71),
            S(-11, -12),   S(-20, -51),   S( -2,   5),   S( 38,  34),   S( 10,   6),   S( 53,  46),   S( 20,  30),   S(  5,  29),
            S(  7,  41),   S( -8, -15),   S( -3, -38),   S( 16,  23),   S(  6,  29),   S(-12,  15),   S( -1,  29),   S( 20,  78),
            S( -4,  -1),   S( -4, -28),   S( 17,  41),   S(  8,   8),   S( -7,   9),   S(  6,  23),   S(  9,  21),   S(  1,  14),

            /* knights: bucket 12 */
            S( -3, -14),   S(  0,  -4),   S( -4, -31),   S( -3, -10),   S(  0,   0),   S( -2, -13),   S(  6,   3),   S( -1, -11),
            S( -3,  -5),   S(  2,   8),   S(  2,   5),   S( -1, -12),   S(-10, -15),   S( -6, -38),   S( -2,  -9),   S(  1,  -2),
            S( -2, -12),   S(  2,   0),   S( -8, -28),   S(-10, -16),   S( -3,  -4),   S( -2,  -7),   S( -2, -16),   S( -9, -29),
            S(  3,  10),   S(  3,   4),   S(  0,  -5),   S(  5,  36),   S( -2,   8),   S(  1,   3),   S(  0,  -8),   S(  1,  -7),
            S(  4,  11),   S(  6,   3),   S(-10, -17),   S( -1,   1),   S(  1, -30),   S(  4,  11),   S( -1, -17),   S( -3, -10),
            S( -5,  16),   S(  7, -17),   S(  6,  53),   S(  5,   3),   S(  6,  10),   S(  0,  -3),   S(  1,  -6),   S(  0,   5),
            S(  7,  23),   S(-13, -13),   S( -3, -10),   S(  3,   9),   S( -1, -12),   S( -8, -24),   S(  1,   5),   S(  0,   2),
            S(  4,  17),   S(  4,  38),   S(  1,   3),   S(  2,   4),   S( -4, -16),   S( -2,  -4),   S( -3, -10),   S(  1,   6),

            /* knights: bucket 13 */
            S( -4,  -6),   S(  1,  -3),   S(  1,   3),   S(  1,   1),   S( -8, -24),   S( -1, -12),   S( -7, -28),   S( -2,  -8),
            S(  1,   5),   S(  0,   6),   S(  2, -14),   S(  2,  -9),   S( -4, -39),   S(  3,   1),   S(  4,  18),   S(  1, -10),
            S( -3, -24),   S( -8, -40),   S(  5,  23),   S( -3, -10),   S( -3, -11),   S( -2, -10),   S(  2,  11),   S( -9, -31),
            S(-15, -14),   S(  8,  31),   S(  3,  22),   S( -9,  -8),   S(  2,   5),   S( 10,   4),   S( -1, -21),   S( -2,   3),
            S(  3,   5),   S(  6,  36),   S( 12,  18),   S(  9,  22),   S( -3, -18),   S( -1, -10),   S( -1,  -7),   S(  3,   8),
            S(  1,  18),   S( 14,  28),   S(  0,  67),   S( -9,  16),   S(  4,  25),   S( -6, -17),   S(  5,  14),   S( -5, -21),
            S(  0,   0),   S(  2,  11),   S( -4, -14),   S(  0,  24),   S( 20,  73),   S( -8, -21),   S( -1,   5),   S( -8, -14),
            S(  1,   8),   S( -2,  22),   S(  1,  23),   S( -3,   2),   S( -6,  -1),   S(  4,  19),   S(  0,   3),   S(  1,   4),

            /* knights: bucket 14 */
            S(  0, -11),   S( -9, -49),   S( -2,  -5),   S(  0,   1),   S(-11, -45),   S(  0,  -6),   S(  0,   0),   S( -3, -13),
            S( -3, -29),   S( -1, -11),   S(-12, -47),   S( -8, -41),   S(  6,   7),   S(  0,  -3),   S(  2,  15),   S(  4,  11),
            S( -7, -17),   S( -4, -33),   S(-21, -48),   S(  2,  10),   S(  4,   9),   S(  2,  -5),   S(  0,   2),   S(  3,   6),
            S(  0,   1),   S( -4, -16),   S( -4, -33),   S( -8, -13),   S( -1, -11),   S(  9,   2),   S( -1,  -3),   S( -5,  -3),
            S(  0,   5),   S(  1,   2),   S(  3,  16),   S(  5,   0),   S( -8,  -2),   S(  9,  23),   S( -2, -14),   S( -3, -27),
            S( -1,  -4),   S(  2,   5),   S( -6,  -1),   S( -1, -13),   S( 14,  43),   S( -3,   8),   S(  4,  19),   S(  2,  13),
            S(  1,   4),   S( -3, -11),   S( 10,   6),   S( -4,  -1),   S( -6,  24),   S(-12,  -7),   S( -5,   5),   S(  1,   8),
            S(  0,  -4),   S(  3,   9),   S( -3,  -5),   S( -2,  10),   S( -3,  -3),   S(  2,  22),   S(  2,  20),   S(  0,   3),

            /* knights: bucket 15 */
            S( -2,  -6),   S(  0, -17),   S(  4,  -3),   S( -9, -21),   S(  0,   6),   S(  2,  -7),   S(  0,  -2),   S(  0,   5),
            S( -3, -14),   S(  0,  -1),   S( -4, -13),   S( -8, -23),   S(  2,   1),   S(  1,   7),   S( -2, -14),   S(  2,  10),
            S(  0,  -8),   S( -3,  -4),   S(-10, -46),   S(-16, -71),   S( -8, -11),   S(  1,   9),   S( -5, -13),   S( -3, -17),
            S( -7, -16),   S( -3, -19),   S( -4, -19),   S(  4,   5),   S( -4,  -9),   S( 15,  39),   S(  5,  13),   S( -2,  -6),
            S( -1,   1),   S(-11, -22),   S(  6, -12),   S( -7, -22),   S(  0,   0),   S(  8,  26),   S(-10, -26),   S( -1, -15),
            S( -3,  -2),   S( -2,  -7),   S( -8, -29),   S(  9,  30),   S(-10, -29),   S( -1,  21),   S( -2, -12),   S(  6,  38),
            S( -3,  -9),   S( -4,  -5),   S(  3,  12),   S( -3, -11),   S( -4,  -1),   S( -4,   2),   S( -1, -17),   S(  2,  14),
            S(  0,   0),   S(  0,   1),   S( -3,  -8),   S( -4,  -3),   S( -1,   1),   S( -4, -14),   S( 10,  37),   S( -2,  -2),

            /* bishops: bucket 0 */
            S( 24,   9),   S(-10,  15),   S( 59,  10),   S(  0,  23),   S(-12,   9),   S(  9, -26),   S( 18, -38),   S( 21, -44),
            S( 39,  12),   S( 88,  23),   S( 18,  17),   S( 12,  24),   S(-10,  35),   S( 17,   1),   S(-32,   9),   S(  5, -65),
            S( 40,  23),   S( 37,  12),   S( 38,  43),   S( -6,  61),   S( 16,  40),   S(-21,  57),   S(  0,   9),   S( 12, -40),
            S( 22, -21),   S( 59,   3),   S( 32,  18),   S( 53,  16),   S( -7,  51),   S( 43,   2),   S(-11,  36),   S(  8, -23),
            S( 25,   9),   S( 36,  11),   S( 10,  45),   S( 43,  38),   S( 50,  33),   S( -2,  32),   S( 20,   9),   S(-37,  25),
            S(-14,  41),   S( -7,  44),   S( 24,  41),   S( 61,  38),   S( 21,  61),   S(  7,   6),   S(-55,  55),   S( -7,  49),
            S(-58,  84),   S(  7,  43),   S( 21,  54),   S( 19,  66),   S(-57,  26),   S( 29,  30),   S( 21,  32),   S(-32,  11),
            S( -9,  -3),   S( 12,  49),   S(-27,  40),   S(-15,  24),   S( -4,  31),   S(  2,  11),   S( 15,  52),   S(-42,  40),

            /* bishops: bucket 1 */
            S( 35,  74),   S(-21,  53),   S( 29,  20),   S(  8,  27),   S(  5,  26),   S( -2,  38),   S(-20,  49),   S(-43,  31),
            S( 14,  29),   S( 38,  32),   S( 52,  10),   S( 29,  30),   S(  0,  21),   S(  8,  22),   S(-23,  27),   S( 14,   6),
            S( 41,  30),   S(  6,  48),   S( 45,  36),   S(  6,  39),   S( 12,  42),   S( -6,  37),   S( 33,   4),   S( -6,   2),
            S( 48,   1),   S( 26,  28),   S( 11,  25),   S( 27,  42),   S( -4,  54),   S( 30,  24),   S(-10,  41),   S( 24,  23),
            S( 28,  34),   S( 12,  38),   S( 20,  26),   S( -2,  56),   S( 22,  30),   S(-38,  55),   S( 31,  11),   S(-12,  49),
            S(-10,  47),   S( 13,  48),   S( 37,  47),   S( 48,  30),   S( 11,  54),   S( 36,  38),   S(-22,  60),   S( 28,  31),
            S(-12,  68),   S( -6,  42),   S( 44,  41),   S( -4,  57),   S( 50,  32),   S( -2,  49),   S( 21,  61),   S(-14,  57),
            S(  7,  65),   S(-27,  42),   S(-17,  44),   S(-23,  40),   S(  9,  55),   S(-43,  46),   S(-21,  64),   S(-50,  99),

            /* bishops: bucket 2 */
            S( -1,  48),   S( 11,  39),   S(  1,  35),   S(-18,  58),   S( -4,  32),   S(-14,  41),   S(-10, -10),   S(-51,  95),
            S(  2,  52),   S( 16,  29),   S( 19,  30),   S(  0,  38),   S(  2,  38),   S( 16,  11),   S( 10,   7),   S(  4, -12),
            S(  2,  24),   S( -5,  40),   S(  8,  60),   S( -6,  66),   S( -5,  60),   S(  9,  51),   S( -2,  45),   S(-13,  12),
            S( -8,  50),   S(-28,  59),   S(-11,  61),   S( -4,  65),   S(  3,  57),   S(  0,  50),   S( 16,  41),   S( -1,  15),
            S(-11,  54),   S(-15,  48),   S(-30,  50),   S(-14,  52),   S(-32,  69),   S(-11,  67),   S(  3,  44),   S(-18,  43),
            S(-15,  51),   S(-24,  48),   S(-12,  69),   S(-24,  51),   S( 21,  40),   S(-31,  78),   S( 16,  62),   S( 10,  35),
            S(-12,  45),   S( -9,  54),   S(-39,  80),   S(-12,  37),   S(-33,  68),   S(-25,  48),   S(-63,  56),   S(-32,  60),
            S(-96, 118),   S(-93,  99),   S(-51,  64),   S(-105,  81),  S(-64,  58),   S(-59,  61),   S(-39,  55),   S(-44,  57),

            /* bishops: bucket 3 */
            S(-17,  44),   S( 14,  37),   S( 14,  26),   S(  0,  39),   S( -4,  20),   S( 40,   2),   S( 29, -19),   S( 30, -30),
            S( 12,  26),   S(  8,  35),   S( 21,  22),   S(  3,  50),   S( 13,  29),   S(  1,  45),   S( 47,  30),   S( 23,   9),
            S( 27,  15),   S( -3,  52),   S(  6,  64),   S( 12,  49),   S(  4,  72),   S( 15,  63),   S( 18,  40),   S( 31,  12),
            S( 16,  25),   S( -8,  51),   S(  4,  70),   S(  7,  73),   S( 21,  61),   S( 18,  44),   S( 17,  53),   S(  7,   3),
            S(  0,  39),   S( 15,  35),   S( 13,  37),   S( 26,  62),   S(  9,  55),   S( 25,  49),   S(  8,  36),   S( 12,  46),
            S(  5,  51),   S( 13,  41),   S( 14,  60),   S( 24,  36),   S( 22,  50),   S( 36,  60),   S( 30,  40),   S(  2,  71),
            S(-13,  36),   S(-15,  63),   S( 38,  31),   S(  7,  49),   S(-20,  46),   S(-13,  45),   S( -2,  64),   S( -2,  80),
            S(-53, 113),   S(-44,  51),   S( 16,  54),   S( 10,  58),   S(-39,  71),   S(-66,  97),   S(-12,  50),   S( 33,  32),

            /* bishops: bucket 4 */
            S( -9,   1),   S( -9,  30),   S(-42,  10),   S(-52,  23),   S(-20,  14),   S(-30,  15),   S(-11,  -8),   S(  7,  11),
            S(-21,  46),   S( 44,   6),   S(-20,  43),   S(-18,  13),   S(-26,  39),   S( 68,  -3),   S( -6,   8),   S( 43,  18),
            S(-38,  -9),   S(-45,  29),   S( 36,   9),   S(-16,  23),   S( 40,  12),   S( 43,  15),   S(-17, -20),   S(-71,  30),
            S(-29,  35),   S(  2,  29),   S( 38,  29),   S( 37,  29),   S(-34,  42),   S( 15,  13),   S(  9,  34),   S( -3,   6),
            S( 18,   6),   S( 10,  46),   S( -9,  55),   S( 15,  36),   S( -8,  15),   S(-48,  17),   S(-32,   0),   S(-10,  -2),
            S( -9,  10),   S( 42,  -2),   S(  7,  20),   S( 23,  19),   S( 41,  26),   S( -2,  16),   S( -9, -26),   S(-13,   5),
            S(-29,   7),   S( 39,  23),   S(-20,  14),   S(  4,  52),   S( -7,  13),   S(  9,  15),   S(-13,  10),   S( -4,  -7),
            S(  2,  30),   S(-24, -30),   S( -2,  21),   S(-26,  -6),   S(-12,  12),   S( -5,  12),   S( -3,   3),   S( -5,  22),

            /* bishops: bucket 5 */
            S( -3,  53),   S(-15,  62),   S(-40,  44),   S(-17,  27),   S(-46,  37),   S( 10,  24),   S(  5,  43),   S(-24,  46),
            S(-35,  62),   S(-57,  71),   S(-22,  59),   S( 29,  16),   S( 30,  28),   S(-28,  48),   S( -8,  17),   S(-10,  35),
            S(-21,  71),   S(-37,  72),   S( 12,  36),   S(  7,  42),   S( 24,  27),   S(-22,  36),   S(  1,  30),   S( 13,   1),
            S( 41,  38),   S( 40,  35),   S(-29,  51),   S( 37,  28),   S(-15,  38),   S( 19,  36),   S(-12,  26),   S(  0,  40),
            S( 62,  49),   S( 22,  21),   S( 62,  27),   S( 82,  29),   S( 61,  15),   S( 38,  24),   S( 34,  32),   S(-26,  28),
            S( 10,  40),   S( 19,  49),   S( 87,  44),   S(  3,  43),   S(-20,  63),   S( 21,   9),   S( -8,  23),   S( 25,  48),
            S( 10,  48),   S(-19,  40),   S( -9,  42),   S( 13,  41),   S( 15,  50),   S( -6,  58),   S( -9,  37),   S(  1,  42),
            S( -5,  50),   S( 11,  29),   S(-10,  28),   S( -2,  38),   S( 12,  35),   S( 13,  63),   S( 19,  87),   S( 12,  25),

            /* bishops: bucket 6 */
            S(-45,  80),   S(-13,  64),   S(-47,  62),   S(-23,  47),   S(  9,  14),   S(-33,  47),   S(-10,  74),   S( -3,  57),
            S( 18,  62),   S( -6,  40),   S(  1,  49),   S(  2,  48),   S( -5,  37),   S(  6,  25),   S(-102,  70),  S( 15,  69),
            S( -2,  22),   S(-20,  38),   S( 26,  47),   S( 35,  36),   S( 77,  12),   S( 51,  28),   S( 13,  50),   S(-78,  71),
            S( 26,  43),   S(-16,  58),   S( 12,  41),   S( 58,  29),   S( 45,  37),   S( 20,  29),   S(-21,  67),   S(-54,  27),
            S(-19,  49),   S(  0,  44),   S( 55,  24),   S( 64,  29),   S(125,  23),   S( 75,  22),   S( 37,  39),   S(  9,  40),
            S( 16,  33),   S(-18,  29),   S( 27,  36),   S(  3,  57),   S( -6,  56),   S( 48,  54),   S( 25,  39),   S(-28,  71),
            S(-27,  28),   S(-10,  37),   S(  5,  42),   S( -3,  40),   S( 45,  40),   S(-28,  59),   S( 13,  50),   S(-36,  41),
            S( 11,  72),   S(  8,  62),   S( 12,  55),   S(-12,  60),   S(  4,  35),   S( 14,  43),   S( -1,  35),   S(  3,  60),

            /* bishops: bucket 7 */
            S(-18,  31),   S(-16,  47),   S(-53,  -7),   S(-56,  24),   S(-13,  -5),   S(-82,  22),   S(-95, -20),   S(-53,  -4),
            S(-67,  26),   S(-89,  30),   S(-24,  20),   S(  2,  12),   S(-26,  12),   S(-58,  31),   S(-42,  -1),   S(-51, -24),
            S(-30,  20),   S( 19,  -9),   S(  2,  31),   S( 48,   4),   S(-33,  36),   S( -3,   8),   S(-32,  26),   S(-32,  24),
            S(-49,  25),   S( 13,  33),   S( 93, -14),   S( 56,  13),   S( 88,  12),   S(-23,  25),   S( 40,  35),   S(-22,  39),
            S(-35,   7),   S(-25,   1),   S( 40,   7),   S( 74,  -8),   S( 53,  27),   S( 66,  24),   S( 11,  42),   S( -1,   9),
            S(-62,  15),   S(-51,  26),   S(-12,   8),   S( -5,  18),   S( 33,   9),   S( 61,  37),   S( 60,  19),   S(  7,  25),
            S( -3,  -9),   S(-30,   8),   S(  7,   7),   S(  5,  -4),   S( 21,   2),   S(  2,  20),   S( 36,  14),   S(-10,  37),
            S( -9,   7),   S(-30,  29),   S(-46,  18),   S(-23,  24),   S(  2,   5),   S( 27,   9),   S( 24,  45),   S( 27,  27),

            /* bishops: bucket 8 */
            S(  5, -13),   S( -6, -56),   S(-56, -39),   S(-11, -37),   S(  9, -14),   S( -5, -25),   S( 13,  13),   S(  5,  23),
            S(-17, -58),   S(-35, -58),   S( -4, -47),   S( -9, -27),   S( 18,  -6),   S( -6, -44),   S( -2, -49),   S( -5, -35),
            S(-12, -48),   S(-13,   4),   S( -4, -12),   S( 15, -49),   S( 20, -31),   S( -4, -38),   S(  3, -47),   S(-47, -55),
            S(-10,   7),   S( -1, -13),   S( 13,   9),   S(  8, -12),   S( 10,   0),   S(  6, -18),   S(  3,   0),   S(  0, -30),
            S( 18,  39),   S(  8,  26),   S( 17,  -8),   S( 73,  -8),   S( 28, -21),   S( 12,  -8),   S(-14, -41),   S( -5, -40),
            S(-17,  -5),   S(  2, -17),   S(  9,   8),   S(  7, -25),   S( 25,  -6),   S(  0, -30),   S( -8, -39),   S(-24, -72),
            S( -4,   6),   S( 20,  -9),   S( -1,  11),   S(  4, -15),   S(  3, -13),   S(  6, -18),   S(  8,  -3),   S(-12, -32),
            S( -6, -20),   S( -2, -45),   S( -7, -45),   S(  7,   5),   S(-12, -57),   S( -5, -44),   S( -7, -31),   S( -5, -31),

            /* bishops: bucket 9 */
            S( -7, -27),   S(  8, -57),   S(-16,  -9),   S( -9, -39),   S(-22, -55),   S(-14, -46),   S( -6, -19),   S( 12,  43),
            S( -1, -11),   S( -5, -34),   S(-10, -26),   S( 33, -31),   S( -9,  -6),   S(-13, -55),   S(-10, -17),   S( 15,  24),
            S(  6,   5),   S(  4,  -1),   S(  6, -33),   S( 33, -48),   S( 30, -40),   S( 33, -29),   S(-16, -13),   S( 18,   5),
            S( -8, -18),   S( 26,  24),   S( 22, -27),   S( 51, -24),   S( 42, -21),   S( 16,  -9),   S( 15, -39),   S(-14, -59),
            S(  4, -14),   S( 30,  -1),   S( 33, -15),   S( 52,  -6),   S( 15, -39),   S( 40, -29),   S( 13, -18),   S(-15, -26),
            S( -7, -31),   S( 55,   5),   S( 20,  13),   S( 23,  10),   S( 30, -20),   S( 30, -18),   S( -3, -57),   S( -9, -17),
            S(  3,  -9),   S( 26,  12),   S( -9, -31),   S( 17, -19),   S( 17, -16),   S( 10, -40),   S(  0, -33),   S( -4, -29),
            S( -2, -26),   S( -4, -13),   S(  4, -42),   S( -7, -55),   S( -4, -59),   S(  9,  -4),   S( -4, -43),   S(-10, -51),

            /* bishops: bucket 10 */
            S(  8,  12),   S(  8,  -3),   S(-26, -22),   S(  4, -40),   S(-26, -23),   S(-16, -22),   S(  7, -36),   S(-10, -43),
            S(  9, -20),   S(-16, -38),   S( 27, -36),   S(  1, -52),   S( -5, -39),   S( 14, -38),   S(-26, -56),   S( -6,  -7),
            S( -6, -29),   S( 22, -23),   S(-19, -51),   S( 43, -34),   S( 45, -58),   S( -1,  -8),   S(-17,  -3),   S( -6,  14),
            S(-13, -53),   S( 25, -15),   S( 20, -37),   S( 49, -37),   S( 64, -26),   S( 15, -11),   S(  4, -17),   S( 15,  17),
            S(-11, -24),   S( 31, -43),   S( 50, -47),   S( 70, -32),   S( 34,  -9),   S( 38, -32),   S( 18,  19),   S( 10, -43),
            S( -7, -58),   S( 12, -62),   S( 33, -50),   S( 22, -44),   S( 45, -24),   S( 34, -15),   S( 18, -25),   S( -4, -28),
            S( -9, -66),   S(  4, -45),   S(-18, -51),   S( 22, -29),   S(  0, -52),   S( 16, -27),   S( 20,  29),   S( 19,  42),
            S( -4, -63),   S( -3, -28),   S(  3,   6),   S( -2, -30),   S( -1, -30),   S( -4, -50),   S(  7,   2),   S(  7,   6),

            /* bishops: bucket 11 */
            S(-11,  23),   S(-44,   2),   S(-57, -39),   S(-17, -11),   S(-22, -29),   S(-63, -65),   S(-13, -51),   S(-22, -37),
            S(  4,   2),   S( 12, -30),   S(  3, -22),   S(-17, -14),   S(-11, -35),   S(-20, -59),   S(  0, -65),   S(-22, -44),
            S( 10, -23),   S(  4, -68),   S( 13, -58),   S( 24, -53),   S(  3, -13),   S( 12, -26),   S(-17,   3),   S(  9,   0),
            S( -6, -31),   S(  2, -51),   S( 37, -48),   S( 27, -50),   S( 60, -39),   S( 42,   0),   S( 40, -13),   S( 12,  59),
            S( -8, -43),   S( -3, -59),   S( -4, -43),   S( 53, -52),   S( 55, -19),   S( 50,   5),   S(  8,  10),   S(  9,   3),
            S(-13, -52),   S(  6, -55),   S( 19, -51),   S( 34, -20),   S( 11, -30),   S( 55,  -4),   S( 19,  36),   S(-29, -55),
            S(-10, -50),   S( -9, -56),   S(-11, -44),   S( 11, -53),   S( 11, -27),   S( 27, -34),   S( 12, -22),   S(  6,   2),
            S( -8, -61),   S(-20, -56),   S( -9, -55),   S( 15,  -4),   S( 17,   4),   S(-11, -67),   S(  5,   1),   S(  3,   2),

            /* bishops: bucket 12 */
            S(  1,  -5),   S( -5, -25),   S(-20, -77),   S(-10, -16),   S(  3,  15),   S( -4,  -9),   S(  0,  17),   S(  7,  40),
            S( -3, -16),   S(-14, -58),   S( -1, -14),   S( -8, -35),   S(  2,  -9),   S(  5,  21),   S(  1, -12),   S(  5,  20),
            S( -1, -27),   S(-10, -24),   S( -8, -36),   S( -1, -48),   S(  2,   0),   S( -5, -38),   S(-11, -52),   S(  7,  17),
            S( -2, -29),   S(  1, -19),   S(-16, -70),   S( -7, -27),   S(  2, -11),   S(  5,  10),   S( -1, -12),   S(  1,  -8),
            S( -9, -12),   S( -3,  -8),   S(  3, -21),   S( -5, -17),   S(  1, -24),   S( -9, -42),   S(  0, -31),   S( -4,  -6),
            S(-23, -41),   S(  5,  19),   S( -8,  -1),   S(-19, -50),   S( -2, -55),   S(  4,  -6),   S(  9,   8),   S( -4, -10),
            S( -3, -11),   S(-17, -21),   S( -5,  -6),   S(-17, -47),   S( -2,  -9),   S(  8,  -6),   S( -6, -31),   S( -2, -13),
            S(  0, -15),   S( -3,  -6),   S( -4, -55),   S(  0,  -6),   S(  4, -20),   S(  1, -17),   S(-11, -60),   S(  0,   6),

            /* bishops: bucket 13 */
            S(-10, -63),   S( -5, -36),   S( -9, -38),   S( -4, -33),   S(  1, -30),   S( -9,   0),   S(  1,   6),   S( -4, -23),
            S(  4,  -4),   S( -9, -58),   S(-10, -89),   S(-13, -53),   S(  8, -26),   S( -1, -22),   S(  4,   0),   S(  2, -13),
            S( -2, -23),   S(  0, -12),   S(  1, -47),   S( 13, -24),   S(  1, -54),   S( 12, -25),   S(  1,  -9),   S(  6,  14),
            S(  7,  29),   S( -4, -42),   S(  4,  -7),   S( -5, -55),   S( 22, -31),   S(  6, -33),   S( -1, -18),   S(-12, -39),
            S(  4,  15),   S(  4,   8),   S(-13, -84),   S( 17,  -1),   S(  0, -24),   S( 12, -17),   S(  1, -57),   S(  3, -10),
            S( -3, -16),   S( -2, -24),   S(-20, -18),   S( 11, -13),   S(  0, -11),   S(  7, -38),   S(  8, -31),   S( -7, -42),
            S( -6, -40),   S( -4, -11),   S( -4, -32),   S(-11,  15),   S( -5, -24),   S(  0,  -1),   S(-14, -65),   S(  3,   0),
            S(-10, -26),   S( -3, -23),   S( -4, -13),   S(  1, -24),   S( -1,  -7),   S( -3, -31),   S( -1,  -8),   S( -2, -27),

            /* bishops: bucket 14 */
            S(  4,  -3),   S(-10, -46),   S(-11, -34),   S(-23, -76),   S( -8, -43),   S(-10, -46),   S(-11, -89),   S( -4, -23),
            S( -3, -20),   S(  2,  -1),   S(  4, -22),   S(-20, -83),   S(-16, -61),   S(-12, -81),   S(-12, -39),   S( -1, -33),
            S( -5, -15),   S( -6, -33),   S( -8, -58),   S( -6, -57),   S(-15, -79),   S(-16, -74),   S( -8, -40),   S( -1, -17),
            S( -4, -16),   S( -8, -18),   S( -2, -33),   S(  9, -22),   S( 14, -82),   S(  1, -64),   S(-11, -74),   S( -5, -26),
            S(-10, -48),   S( -2, -36),   S( 12, -44),   S(  4, -38),   S( 13, -43),   S(  0, -38),   S( 11, -12),   S( -3, -30),
            S( -1, -16),   S(  4, -38),   S( -8, -54),   S(  0, -51),   S(  8, -28),   S( -2,  17),   S(-10, -50),   S(-10, -34),
            S( -5, -47),   S(  0, -56),   S(-10, -56),   S(  1, -12),   S(-10, -28),   S(  4,  -7),   S( -7,  -9),   S( -5, -33),
            S( -5, -43),   S( -1,  -9),   S( -3, -21),   S( -9, -36),   S( -3, -16),   S(  5,  12),   S( -3,  -1),   S(  0, -13),

            /* bishops: bucket 15 */
            S(  6,  29),   S(  4,  31),   S(-19, -53),   S( 12,  18),   S(-10, -34),   S(-10, -25),   S(  1,   2),   S( -7, -37),
            S(  2,  -1),   S(  4,  17),   S( 10,   8),   S( -6, -42),   S(-11, -46),   S( -4, -29),   S( -1,   1),   S( -1,  -6),
            S( -9, -44),   S( -6, -26),   S(-10, -44),   S(  4,   8),   S( -7, -59),   S(-10, -22),   S(  3, -11),   S(  4,   3),
            S(  2,  -2),   S(-13, -40),   S( -2, -27),   S(-21, -80),   S(  4,  -5),   S( -2, -51),   S(  2,   2),   S( -4, -15),
            S(  0, -18),   S(-16, -46),   S( -4, -30),   S(-19, -66),   S(  3, -19),   S( -1, -14),   S(  9,   0),   S(-10, -25),
            S( -7, -40),   S( -6, -51),   S(-14, -56),   S( -7, -62),   S( -6, -23),   S(-10, -15),   S( 18,  11),   S(  0,   3),
            S(  0, -14),   S(  0, -10),   S( -6, -26),   S(  2, -11),   S(-18, -61),   S(  0,  -8),   S(-10, -19),   S(  2,  11),
            S(  0,  -5),   S( -2, -15),   S( -6, -40),   S(-10, -64),   S( -8, -48),   S(-22, -57),   S(-15, -24),   S(  1,   2),

            /* rooks: bucket 0 */
            S(-40,  11),   S( 11, -22),   S(  7, -22),   S( 14,  -9),   S( 13,   8),   S( 16, -25),   S( 10,   5),   S( 10,   7),
            S( 26, -79),   S( 28, -48),   S( 18,  -1),   S(  8,  -4),   S( 39,  -9),   S( 15,  -7),   S(  2,  -8),   S(-53,  40),
            S(-16,  20),   S( 19,   5),   S( 54, -23),   S(  8,  16),   S( 12,  17),   S( 20, -20),   S(-31,   7),   S(-48,  17),
            S( 20, -11),   S( 60,  -1),   S( 48,  11),   S( 56,   6),   S( 29, -17),   S( 12,   1),   S( -2,   0),   S(-28,  26),
            S( 60, -46),   S( 83,   6),   S( 97, -16),   S( 58,  -9),   S( 72,  -6),   S( 16,   3),   S( 30,   1),   S( -9,  22),
            S( 51, -36),   S( 76, -17),   S( 65,   3),   S( 48,  -4),   S( 74,  -2),   S( -6,  25),   S( 70,   6),   S( -9,  18),
            S( 33,  -9),   S( 76,  12),   S( 20,  20),   S( 29,  33),   S(  6,  42),   S( 14,  17),   S( 14,  32),   S( 19,  23),
            S( 31,   7),   S( 13,  36),   S( 15,  27),   S( 64,  -1),   S( 24,   4),   S( 59,  -5),   S( -4,  40),   S( 44,  -3),

            /* rooks: bucket 1 */
            S(-80,  36),   S(-41,  19),   S(-25, -12),   S(-29, -12),   S( -7, -22),   S(-17,  -9),   S(-20, -10),   S(-36,  14),
            S(-46,  16),   S(-57,  10),   S(-23,   4),   S(-17, -33),   S(-32,  -3),   S(-30,  -7),   S(-44, -10),   S(-52,   6),
            S(  5,  15),   S(-45,  44),   S(-19,  19),   S(-34,  31),   S(-38,  22),   S(-20,   8),   S(-20,  14),   S(-40,   9),
            S(-31,  44),   S(-50,  41),   S(  4,  41),   S(-11,  24),   S(-18,  43),   S(-56,  57),   S(-29,  40),   S(-38,  22),
            S( 56,  14),   S( 44,  38),   S(-27,  55),   S(-40,  53),   S(-23,  41),   S( 38,  19),   S(  4,  14),   S(-36,  18),
            S( 83,   3),   S(  0,  38),   S( 32,  36),   S( 14,  26),   S( 42,  17),   S(-26,  44),   S( -4,  26),   S(-55,  51),
            S( -5,  46),   S(-10,  57),   S( 24,  53),   S(-54,  75),   S( 23,  34),   S( 29,  45),   S(-47,  57),   S(-26,  40),
            S( 70,   2),   S( 22,  31),   S( 36,  24),   S(-66,  62),   S(-12,  46),   S( 58,   6),   S(  2,  23),   S( 28,  17),

            /* rooks: bucket 2 */
            S(-65,  37),   S(-45,  36),   S(-40,  27),   S(-53,  31),   S(-52,  12),   S(-56,  18),   S(-36,   0),   S(-53,  28),
            S(-82,  51),   S(-74,  42),   S(-39,  36),   S(-49,  22),   S(-41,   9),   S(-56,  27),   S(-69,  30),   S(-66,  17),
            S(-71,  69),   S(-56,  64),   S(-60,  68),   S(-48,  33),   S(-52,  55),   S(-35,  44),   S(-13,  20),   S(-32,  13),
            S(-86,  78),   S(-58,  71),   S(-21,  69),   S(-15,  52),   S(-35,  60),   S( -3,  48),   S(-49,  72),   S(-25,  44),
            S(-20,  59),   S(-48,  73),   S(-33,  74),   S(  7,  47),   S( 38,  37),   S(  8,  47),   S(-42,  70),   S(-32,  51),
            S(-25,  61),   S(-25,  58),   S( -4,  55),   S( -4,  53),   S(  6,  45),   S( 22,  41),   S( 10,  29),   S(  5,  30),
            S(-70,  65),   S(-51,  80),   S( -6,  63),   S( -5,  72),   S( -1,  59),   S( 29,  31),   S(-56,  91),   S(-13,  54),
            S(-38,  77),   S(-26,  68),   S(-32,  66),   S(-14,  51),   S(-31,  59),   S( -6,  52),   S(-20,  65),   S( 50,  23),

            /* rooks: bucket 3 */
            S(  0,  70),   S(  4,  73),   S( 16,  63),   S( 24,  57),   S( 14,  57),   S( -8,  60),   S(  2,  70),   S(-16,  56),
            S(-35,  94),   S(-15,  71),   S( 11,  68),   S( 17,  53),   S( 19,  59),   S( 15,  59),   S( 38,  23),   S( 19, -35),
            S(-33,  87),   S(-17,  93),   S(  8,  81),   S(  7,  66),   S( 25,  67),   S( 13,  69),   S( 23,  64),   S( -6,  53),
            S(-17,  91),   S(-31,  98),   S( 27,  77),   S( 34,  75),   S( 30,  75),   S( -7, 111),   S( 40,  77),   S(  3,  73),
            S(-17, 103),   S( 25,  85),   S( 36,  69),   S( 53,  71),   S( 61,  71),   S( 48,  71),   S( 84,  64),   S( 46,  41),
            S( 14,  91),   S( 14,  82),   S( 43,  69),   S( 44,  72),   S( 43,  58),   S( 56,  48),   S( 91,  39),   S( 87,  32),
            S(-14, 100),   S( -7, 112),   S( 11,  99),   S( 48,  83),   S( 45,  77),   S( 82,  61),   S( 65,  72),   S(116,  18),
            S(-37, 135),   S( 33,  93),   S( 24,  85),   S( 80,  48),   S( 67,  52),   S( 95,  44),   S(111,  47),   S( 99,  40),

            /* rooks: bucket 4 */
            S(-103,  37),  S(  4, -20),   S(-45,   3),   S(-18,  21),   S(-14, -21),   S( 12, -49),   S( -3, -30),   S(-12, -23),
            S(-26, -19),   S(-33,   0),   S(-27,  14),   S(-40,  24),   S(-21, -19),   S(-54,  -5),   S( 31, -61),   S(-24, -14),
            S(  0,   9),   S(-41, -25),   S(-13,  13),   S(-28, -10),   S(-30,   9),   S(  0, -30),   S( 13, -24),   S(-63,  -8),
            S(-34, -21),   S(  4,  10),   S(-51,  44),   S( 48,  -6),   S( 55,  -2),   S(-15,   0),   S( -2,  -2),   S(-25,   5),
            S(-23,   5),   S(-15,  24),   S(-25,  21),   S( 55,   7),   S(  4,   5),   S(  1,  12),   S( 38,  16),   S( 14,  -7),
            S( 28,  -4),   S( 11,   1),   S( 53,   3),   S( 40,  21),   S( 28,  12),   S( 27,  25),   S(-10,  18),   S( 25,  29),
            S(  6, -10),   S( 25,  45),   S( 36,   8),   S( 52,  14),   S( 45,  -4),   S( 16,  -7),   S( 47,  12),   S( 30,  16),
            S( 28, -47),   S( 38,  34),   S( 28,   1),   S( 29,   4),   S( -7,  -4),   S( 15,  -9),   S( 10,  13),   S( 22,   9),

            /* rooks: bucket 5 */
            S(-60,  50),   S(-27,  40),   S(-55,  42),   S(-23,  -5),   S(-38,  21),   S(-28,  36),   S(-26,  36),   S(-38,  39),
            S( 13,  22),   S(-51,  46),   S(-99,  71),   S(-74,  36),   S(-46,  25),   S(  7,   7),   S( -4,  27),   S(-41,  10),
            S(-35,  48),   S(-35,  43),   S(-38,  60),   S(-72,  52),   S(-72,  49),   S(-51,  43),   S(-12,  32),   S(-17,  11),
            S(-24,  65),   S( 11,  30),   S( -9,  54),   S(-53,  52),   S(-21,  48),   S(  5,  61),   S(-17,  43),   S( -7,  16),
            S( -7,  60),   S(  4,  50),   S( 28,  46),   S( 35,  61),   S( 16,  49),   S( 50,  47),   S( 77,  48),   S( 32,  25),
            S( 65,  58),   S( 74,  50),   S( 38,  67),   S( 60,  52),   S( 69,  43),   S( 69,  43),   S( 72,  36),   S( 62,  22),
            S( 45,  45),   S( -2,  67),   S( 49,  51),   S( 37,  80),   S( 37,  48),   S( 72,  52),   S( 88,  37),   S( 80,  36),
            S( 95,  29),   S( 62,  23),   S( 55,  45),   S( 25,  46),   S( 52,  36),   S( 49,  42),   S( 47,  42),   S( 35,  36),

            /* rooks: bucket 6 */
            S(-51,  35),   S(-38,  42),   S( -7,  30),   S( -2,  17),   S(-21,  13),   S(-86,  54),   S(-37,  57),   S(-26,  52),
            S( -8,  18),   S(  1,  25),   S( -8,  32),   S(-61,  39),   S(-75,  63),   S(-67,  70),   S(-63,  45),   S(-13,  26),
            S(-48,  56),   S(-47,  58),   S( -6,  46),   S(-64,  54),   S(-60,  53),   S(-56,  63),   S(-23,  69),   S( 14,  22),
            S(-76,  78),   S( -9,  56),   S(  5,  66),   S(-21,  54),   S(-39,  59),   S( 13,  46),   S(-60,  87),   S(-25,  57),
            S(-21,  77),   S( 20,  72),   S( 75,  46),   S( 39,  45),   S( -8,  85),   S(  7,  74),   S( 28,  54),   S(-18,  67),
            S( 20,  61),   S( 73,  47),   S( 95,  44),   S( 43,  40),   S( 53,  54),   S( 45,  60),   S( 60,  63),   S(112,  34),
            S( 65,  54),   S(108,  40),   S(105,  33),   S(124,  21),   S( 99,  40),   S( 61,  57),   S( 60,  53),   S( 64,  44),
            S( 39,  70),   S( 85,  44),   S( 67,  44),   S( 53,  38),   S( 64,  56),   S( 65,  42),   S( 83,  43),   S( 49,  46),

            /* rooks: bucket 7 */
            S(-66,   2),   S(-24,  -8),   S(-17, -19),   S(-15,   1),   S(  8, -12),   S(-35,  23),   S(-29,   2),   S( 22, -21),
            S(-64,  30),   S(-38,   5),   S(-19,  -2),   S(-10,  -2),   S(  5,   4),   S( 23,   3),   S(-25,  -6),   S(-40,   3),
            S(-88,  58),   S(-50,  29),   S(-17,  29),   S(-12,   0),   S(  9,  12),   S(-40,  11),   S(-22,  -4),   S( 19,  -5),
            S(-46,  32),   S( 29,  13),   S( 29,  18),   S( 55,   7),   S( 45,   1),   S( 29,   2),   S( 36,  -6),   S(  4,   7),
            S( -1,  32),   S(-15,  33),   S( 75,  -7),   S( 50,   4),   S( 66,  12),   S(100,   0),   S( 30,  38),   S( 63, -21),
            S(  2,  31),   S( 23,  13),   S(111, -15),   S(105, -14),   S( 98, -11),   S( 81,   4),   S( 67,  13),   S( 47, -15),
            S( 20,  26),   S( 38,  21),   S( 62,  15),   S(105,   4),   S( 98,  10),   S(106,  -1),   S( 52,  27),   S( 72, -10),
            S( 39,  46),   S( 20,  28),   S( 47,  12),   S(123, -28),   S( 38,  15),   S( 15,  19),   S( 50,  -1),   S( 67, -21),

            /* rooks: bucket 8 */
            S(-56, -23),   S(-18,  16),   S(-15,  -7),   S(-45,   6),   S(-36, -49),   S(-38, -53),   S(-26, -34),   S(-37,   9),
            S(-11, -23),   S(-28, -28),   S(-15,  -4),   S( -9, -17),   S(-10,   4),   S( -2, -21),   S(-18, -28),   S(-25, -86),
            S(  2,  -1),   S(  5,  16),   S(-20,  -6),   S( -3,   9),   S(-25, -47),   S(-22, -29),   S( -5,  10),   S( -9, -16),
            S( -1,  16),   S( -7,  10),   S(-11,  -3),   S( 11,  10),   S(  3,   8),   S( -9, -25),   S( -8, -32),   S(-22, -30),
            S(-28, -55),   S( -7,  -7),   S(-25,  16),   S(-12, -10),   S(  0, -23),   S( 13, -10),   S(  4,   0),   S(-13, -51),
            S( -7,   0),   S(-30,   0),   S( -3,  26),   S( 15,   2),   S( -1,   1),   S(  7,  -7),   S( -4,   0),   S(  0,  31),
            S(  1,   2),   S(-17,  -3),   S(  6,  14),   S(  8, -20),   S( 21, -20),   S( -3, -25),   S( 18, -10),   S( 14,  16),
            S(-16, -135),  S( -2, -20),   S( 12,  18),   S( -5, -13),   S( -9,  -3),   S(-13, -41),   S(  9,  20),   S( -3,  -3),

            /* rooks: bucket 9 */
            S(-45, -38),   S(-16, -52),   S(-33,  -8),   S(-68, -41),   S(-29,  -6),   S( -8, -20),   S( -4, -66),   S(-54, -48),
            S( 22, -37),   S( 13, -53),   S(-25, -55),   S(-26, -26),   S(-22, -35),   S(  5, -28),   S( -2, -29),   S(-28, -42),
            S(  5, -37),   S( 13, -36),   S( -7, -16),   S(-13, -22),   S(-38, -31),   S(  6, -11),   S(  7, -25),   S(  2, -18),
            S(-11, -10),   S(  2,  -5),   S(-20,  -4),   S(-35,  -9),   S( -9, -20),   S( -3, -20),   S( 17,   0),   S( -5, -20),
            S(  5,   0),   S(-18,   5),   S( -4,  13),   S(  0,  22),   S(  4,   1),   S( 24,  -5),   S( -6, -16),   S(  0, -26),
            S(  8,   4),   S(-15, -20),   S( -8, -26),   S(-28,  -7),   S( -1, -10),   S( 15, -26),   S(  4,  -3),   S(  3, -21),
            S( 37,   0),   S( 52, -14),   S( 38,  -7),   S( 33, -10),   S( 11, -14),   S( 39,  -9),   S(  8, -38),   S( 38,  -9),
            S( 50, -90),   S( 22, -43),   S( 13, -34),   S( 37,  39),   S( 15,  12),   S(  6, -32),   S(  1, -27),   S( 15, -14),

            /* rooks: bucket 10 */
            S(-67, -85),   S(-50, -53),   S(-43, -72),   S(-53, -36),   S(-38, -34),   S(-47, -59),   S( 15, -37),   S(-16, -47),
            S(-19, -33),   S(-15, -44),   S(-29, -45),   S(-60,  -4),   S(  2, -49),   S(-51, -29),   S( 19, -13),   S(  3, -21),
            S(-51, -50),   S(-25, -36),   S(-21, -14),   S( -3, -20),   S(-55, -26),   S(  4, -28),   S(  7,   7),   S( -3, -10),
            S(-29, -38),   S(  9, -15),   S(-31, -22),   S(-20, -11),   S(-11, -14),   S(  4, -30),   S( 17,  27),   S( -5, -31),
            S(  5, -35),   S(  7, -35),   S(  6, -21),   S(  8, -34),   S(-16, -17),   S(  3,   4),   S( 21, -12),   S(  7, -14),
            S( 26, -10),   S( 20,   3),   S( 18,  -8),   S( -4,   1),   S( -5,  -9),   S( 10, -30),   S( 31, -12),   S( -2,  -7),
            S( 69, -26),   S( 60, -23),   S( 64, -23),   S( 57, -22),   S( 46,  -9),   S( 29, -15),   S( 29, -48),   S( 27, -29),
            S( 39, -12),   S(  2, -36),   S( 13, -29),   S( 12, -12),   S( 35,   1),   S( 10,   4),   S( 21, -44),   S(  5, -40),

            /* rooks: bucket 11 */
            S(-65, -44),   S(-38, -23),   S(-32,   1),   S(-29, -71),   S(-30, -19),   S(-46,  14),   S(-43, -20),   S(-52, -12),
            S(-42, -12),   S(-12, -31),   S(-56, -42),   S(-29, -22),   S(-42, -25),   S(-25, -21),   S(-23, -35),   S( -2,  16),
            S( -6, -28),   S(-18, -26),   S( 18, -13),   S(  2, -15),   S( -7,  12),   S(-14,  17),   S(-22, -18),   S(-30, -51),
            S(-33,   3),   S( -9, -37),   S(-19,   1),   S(-13,   9),   S(  0,  10),   S(-19,  25),   S(  4, -18),   S(-21, -54),
            S(-21,  -1),   S( 29, -34),   S(  2,  -6),   S( 30, -45),   S( 20, -12),   S(  7, -11),   S(  3, -10),   S(-17, -34),
            S( -3,  15),   S( 29,  17),   S( 23,   0),   S( 52, -14),   S( 27,   2),   S( 19,  -7),   S(-15,  -8),   S(  5, -17),
            S( 32,  25),   S( 32, -16),   S( 48, -14),   S( 40, -27),   S( 32, -12),   S( 21,   3),   S( 16,   1),   S( 58, -15),
            S( 18,  22),   S(  7,  -7),   S( 10, -31),   S( 18, -23),   S(  6, -19),   S( 12, -10),   S( 17,  19),   S(  8,   0),

            /* rooks: bucket 12 */
            S( -5, -52),   S(-20, -46),   S(-11,  -8),   S(  0, -10),   S( -5, -26),   S(  1, -14),   S(-18, -48),   S(-18, -26),
            S(  3,   2),   S( -3,   0),   S( -9, -13),   S( 11,  13),   S( -1,  25),   S( -4,   1),   S( -4, -29),   S( -7, -30),
            S( -3, -24),   S(-13, -28),   S( -3, -24),   S( -5, -19),   S( -5, -26),   S( -1, -23),   S(  0,  -9),   S(  3,   6),
            S(-13, -21),   S(-15, -31),   S( 16,  27),   S(  7,   0),   S( -5,  -4),   S(-12, -41),   S(-13, -47),   S( -5, -28),
            S(-12, -25),   S(-16,   8),   S(  4, -30),   S(  2, -16),   S(-15, -32),   S(  5,  -1),   S(  0,  -9),   S(-11, -24),
            S( -4, -21),   S( -1, -12),   S( 15,  10),   S(  5, -38),   S( -2, -15),   S(-12, -45),   S(  5, -17),   S( -9, -33),
            S( -8, -28),   S( -1,  -6),   S( -3, -37),   S( -4, -30),   S(  2, -31),   S( -8, -46),   S(-19, -60),   S(  1, -18),
            S( -9, -47),   S( -1,  -2),   S(  6, -10),   S( -6, -34),   S(-11, -58),   S(  0, -28),   S(-17, -81),   S( 18,  40),

            /* rooks: bucket 13 */
            S(-13, -31),   S( -4, -43),   S(-16,   3),   S(  1,  21),   S(  2,  23),   S( -7, -11),   S( -9, -58),   S(-25, -36),
            S( -4, -31),   S( -3, -21),   S(-15,  -2),   S( -6,   9),   S(-15, -42),   S(-11, -13),   S(  4,   8),   S(  6,  -4),
            S(-14, -64),   S(-21, -48),   S(-17, -31),   S(  6, -25),   S(  5,  27),   S( -5, -11),   S( -4, -22),   S( -9, -39),
            S(-10, -23),   S(  0,  15),   S(-33, -58),   S(-13, -28),   S(  1, -11),   S(-17, -24),   S( -6, -41),   S( -1,  -4),
            S(  9,   0),   S(  1,  -3),   S( -3,   4),   S(-10, -28),   S(-16, -28),   S( -7, -21),   S(-11, -55),   S(  5,  11),
            S(-20, -45),   S( -3,  -2),   S(-12, -40),   S(  7, -11),   S(  8,  -4),   S( 18,  24),   S(  6,  -6),   S( -3, -24),
            S( -5,  -5),   S(  3,   8),   S( 13,  19),   S(  1,   6),   S( -2, -12),   S(  2,   1),   S(  8, -17),   S(-10, -21),
            S(-24, -140),  S(-20, -69),   S(  2,  -5),   S(  4,  -6),   S(  0,  16),   S(-13, -61),   S( -6, -40),   S( -5,  -4),

            /* rooks: bucket 14 */
            S( -2,  18),   S(-21, -50),   S(  8, -16),   S( -3,  -4),   S(-17,  16),   S( -2,   1),   S(  3, -30),   S( -4,  -3),
            S(-12,  -8),   S(-15, -53),   S( -3,  14),   S(-35, -55),   S(-20, -65),   S( -6, -34),   S(  3,   6),   S( -1, -18),
            S( -7, -42),   S(-11, -51),   S( -6,  -6),   S(  0,  -7),   S(-26, -55),   S( -5,   0),   S(  5,  18),   S( -2,  -8),
            S( -5,  -8),   S(-16, -67),   S(  3,   4),   S( -8,   3),   S(  0, -19),   S( -2,  10),   S( -9, -44),   S(-23, -46),
            S(  1, -47),   S(  7, -35),   S( -6, -43),   S(-19, -73),   S(  0, -46),   S(  0, -51),   S( -5, -28),   S( -1, -10),
            S( -1, -15),   S(  8,  -7),   S( -1, -63),   S(  2, -66),   S( -3, -78),   S(  5, -20),   S(  5, -27),   S( -2,  -2),
            S( 15,  12),   S(  4, -41),   S(  2, -62),   S(  4, -49),   S( 13, -27),   S( 12, -16),   S( 23,  -6),   S(  8,   7),
            S(-11, -50),   S(-13, -44),   S(-11, -63),   S( -3, -10),   S(-19, -56),   S( -1,   6),   S( -6, -22),   S( -6,  -8),

            /* rooks: bucket 15 */
            S( -8, -11),   S(-15, -43),   S( -9, -46),   S(-17, -33),   S(  3,  -6),   S( -3, -13),   S(-19, -67),   S( -7, -31),
            S(-14, -37),   S(-20, -42),   S(  7,  22),   S(-11, -16),   S(-10, -20),   S(-12, -12),   S(-21, -55),   S(  5,  19),
            S( -4, -39),   S(-12, -41),   S(-18, -56),   S(  0,  -9),   S(  5, -24),   S( -6, -25),   S(  0,   0),   S(-13, -36),
            S(-11, -64),   S(  4,  -4),   S(-14, -39),   S(  2,  -5),   S(-17, -51),   S(  7, -19),   S(  6,   1),   S( -6,   9),
            S( -4, -28),   S( -6, -27),   S(  4, -31),   S(  1, -27),   S(  2, -22),   S( 10,  -8),   S(  3, -44),   S( -6,  10),
            S( -1, -12),   S( -4, -11),   S(  4, -36),   S( -3, -58),   S(-10, -67),   S( 17,  -6),   S( -1, -38),   S( -4,  10),
            S( 22,  42),   S( 15,  -9),   S( 13,  -6),   S( -7, -71),   S(  0, -40),   S( 19,   3),   S( 10, -11),   S( -4, -17),
            S( -9, -21),   S(-12, -24),   S( -1, -26),   S( -3, -33),   S(  7,  -2),   S(  0, -18),   S( -8, -20),   S( -7, -30),

            /* queens: bucket 0 */
            S(-17, -12),   S(  1, -76),   S( 62, -91),   S( 65, -59),   S( 49, -66),   S( 40, -39),   S( 42,  29),   S(  8,  -3),
            S(-11, -30),   S( 54, -79),   S( 39, -19),   S( 39, -24),   S( 35,  11),   S( 24,  31),   S( 16,  45),   S( 55,   3),
            S( 10,  29),   S( 53, -26),   S( 26,  32),   S( 20,  42),   S( 18,   8),   S(  9,  30),   S( 12,  21),   S( 23,  72),
            S(  4,  17),   S(  2,  57),   S(  1,  48),   S(  0,  59),   S( 20,  39),   S( 27,  15),   S(  2,  67),   S( 19,  13),
            S( 39,  30),   S( 37,  29),   S( 23,  25),   S( 17,  35),   S(-13,  25),   S( 20, -24),   S( 16,   6),   S( 30,  17),
            S( 26,  64),   S( 27,  76),   S( 40,  30),   S( 33,  -9),   S( 44,  12),   S( 16,  42),   S( 25,  29),   S(  6, -45),
            S( 48,  23),   S( 59,  26),   S( 21,  64),   S( 71,  50),   S(-16,  42),   S( -6,  -9),   S( -3,  14),   S( 30,   8),
            S( 66,  47),   S( 20,  25),   S( 53,  29),   S( 24,  28),   S( 23,  43),   S(-24,  13),   S( 76,  42),   S( 19,  19),

            /* queens: bucket 1 */
            S(  1, -40),   S(-81, -14),   S(-59, -29),   S(-10, -95),   S( 12, -64),   S( -1, -83),   S( 25, -49),   S( 12,  23),
            S(-19, -42),   S(-31, -28),   S(  8, -52),   S(  6,   3),   S(  5,  11),   S( 15, -24),   S( 20, -37),   S( 18, -11),
            S(-37,  30),   S(  3, -27),   S(  7,   9),   S(  7,  23),   S( -5,  52),   S( -7,  47),   S( 13,   4),   S( 21,  46),
            S( 12,  -1),   S( -7,  10),   S(-18,  64),   S( 45,   9),   S( 16,  40),   S( 11,  45),   S( -4,   0),   S( 11,  53),
            S(  4,  -2),   S( -7,  24),   S(  2,  61),   S(-21,  74),   S(-19,  77),   S(  0,  41),   S(-21,  30),   S( 15,  33),
            S( 18,  43),   S( 23,  55),   S( -2,  73),   S(-10,  52),   S(-36,  74),   S(-35,  46),   S( 27,  49),   S( 17,  38),
            S(-18,  52),   S(  6,  70),   S( -5,  19),   S(-10,  85),   S(-35,  69),   S( -8,  52),   S(-23,  39),   S( -9,  39),
            S( 14,   8),   S(  8,  31),   S( 44,  20),   S( -3,  28),   S( 26,  37),   S( -3,  24),   S(  8,  46),   S(-27,  27),

            /* queens: bucket 2 */
            S( 34, -28),   S( 31, -61),   S( 14, -46),   S(  5, -26),   S(-18,   7),   S(-37, -23),   S(-35, -26),   S( 32,  27),
            S(  8,   6),   S( 20,  16),   S( 20, -22),   S( 24, -11),   S( 19, -18),   S( 11, -35),   S( 29, -31),   S( 33, -21),
            S( 23,   4),   S( 21,   8),   S( 21,  38),   S( 10,  30),   S( -5,  92),   S( 16,  50),   S( 12,  15),   S( 28,   7),
            S(  8,  -5),   S(-14,  42),   S(  0,  55),   S( 10,  57),   S(-10,  85),   S(  2, 102),   S(  7,  43),   S( 12,  25),
            S( 13,   3),   S(-11,  67),   S(-22,  75),   S(-39, 122),   S(-37, 113),   S(-27, 114),   S(-18, 114),   S(-11, 117),
            S( 21,  48),   S( 20,  39),   S(-32, 115),   S(-12,  75),   S(-27, 104),   S( -9, 104),   S(  9, 102),   S( -7,  77),
            S(-30,  59),   S(-47, 115),   S(-26,  81),   S( 24,  63),   S(-16, 104),   S( 30,  58),   S(-60,  75),   S(-27,  79),
            S(-63,  80),   S( 12,  52),   S( 41,  57),   S( 16,  53),   S( 44,  67),   S( 17,  34),   S(  6,  57),   S(-13,  44),

            /* queens: bucket 3 */
            S( 69,  91),   S( 59,  79),   S( 44,  88),   S( 40,  90),   S( 65,  29),   S( 29,  28),   S( 22,   1),   S( 38,  38),
            S( 60,  91),   S( 54, 101),   S( 38, 107),   S( 41,  85),   S( 44,  71),   S( 52,  57),   S( 55,   2),   S(  2,  53),
            S( 52,  84),   S( 41, 103),   S( 49,  84),   S( 43,  72),   S( 40,  90),   S( 43, 102),   S( 44, 108),   S( 52,  47),
            S( 30, 129),   S( 34,  97),   S( 42,  93),   S( 39,  96),   S( 39,  98),   S( 32, 139),   S( 41, 102),   S( 19, 141),
            S( 34, 100),   S( 43, 120),   S( 21, 111),   S( 14, 123),   S( 20, 122),   S(  7, 141),   S( 14, 168),   S( 28, 147),
            S( 51, 125),   S( 47, 114),   S( 41, 106),   S( 32, 113),   S( 28, 139),   S( 65, 118),   S( 44, 177),   S( -6, 205),
            S( 33, 115),   S( 34, 120),   S( 63, 100),   S( 47, 108),   S( 23, 133),   S( 49, 110),   S( 72, 118),   S(141,  54),
            S( 69,  91),   S( 81,  97),   S( 42, 123),   S( 59,  90),   S( 39, 100),   S( 92,  64),   S(148,  49),   S(128,  59),

            /* queens: bucket 4 */
            S( -3, -19),   S(-33, -17),   S(-44,   3),   S(-12, -23),   S( -3, -15),   S(  9,  -6),   S(-42, -17),   S(-33, -19),
            S(-24, -14),   S(-30, -15),   S( 31,   2),   S(-31,  24),   S(  3,  -8),   S( 25, -22),   S(-15,   5),   S(-41, -48),
            S( -8, -10),   S( 12,  -5),   S(  8,  36),   S( 37,  47),   S( 10,  13),   S( 21,  -9),   S(  0, -31),   S(-26, -30),
            S(-53, -27),   S( 29,  20),   S(-12,  41),   S( -5,  57),   S( 17,  18),   S( 36,  33),   S( 12, -18),   S(-15,  -9),
            S(-12,  11),   S( 29,   4),   S(  4,  33),   S( 61,  66),   S( 19,  32),   S( 40,  -2),   S(-28, -18),   S( -5, -30),
            S( -6,   5),   S( 24,   8),   S( 24,  83),   S( 67,  75),   S( 22,  41),   S( 12,  14),   S(-26, -31),   S( 24,  13),
            S(-33, -17),   S(-13,  34),   S(-21,  -9),   S( 24,  31),   S(  7, -14),   S(-13,  -6),   S(-13, -33),   S(-14, -25),
            S(-20, -48),   S(-20, -14),   S( 22,  53),   S( 17,   8),   S(-14,   1),   S(  8,  20),   S(-19, -35),   S(-25, -38),

            /* queens: bucket 5 */
            S(-54, -29),   S(-44, -55),   S(-16, -11),   S(  9, -43),   S(-34, -12),   S( 12, -20),   S(-38, -19),   S(-13, -14),
            S(-68, -40),   S(-41,  -6),   S(-42,  -8),   S(-54,   5),   S( 10,  -5),   S(-69, -28),   S(-16,  -6),   S(-41, -29),
            S(-18,  15),   S(-59,  -8),   S(-85,   5),   S( -6,  49),   S( 40,  71),   S( -2, -33),   S( 24,  -3),   S( -2, -12),
            S(-63, -19),   S(-54, -28),   S( 11,  52),   S(  1,  86),   S( 23,  57),   S( -6,  23),   S( 15,   4),   S( -5,  34),
            S(-41,  -1),   S(-25,  10),   S( -5,  49),   S( 35,  79),   S( 51, 101),   S(  7,  27),   S( 15,  22),   S(-42, -22),
            S(-51,  -4),   S( 23,  41),   S(  4,  66),   S(-15,  46),   S(  3,  41),   S(  1,   3),   S( -7, -10),   S( 30,   5),
            S( -2,  14),   S(  5,  41),   S( 37,  88),   S(-11,  26),   S( 16,  36),   S( 25,  44),   S( 26,  29),   S(-10,   0),
            S(-12, -11),   S(  2,  19),   S( -8,  -5),   S(  7,  58),   S( 16,  39),   S( -9,   5),   S(-12, -37),   S(-22, -27),

            /* queens: bucket 6 */
            S(-47,  -5),   S(-36, -39),   S(-48, -16),   S(-98, -91),   S(-66, -70),   S(-49, -62),   S(-47, -68),   S( -8,  15),
            S(-52, -13),   S(-25,  21),   S(-36,   8),   S(-21,  11),   S(-80,  26),   S(-79,   3),   S(-109, -25),  S(-11,  -7),
            S(-11,  18),   S(-37,  12),   S(-56,  47),   S(-85,  81),   S(-25,  60),   S(-32,   7),   S(-49, -32),   S( 54,  37),
            S(-18,  23),   S( 19,   7),   S(-26,  63),   S(-55,  81),   S( 17,  74),   S( 35,  59),   S(-14,  11),   S( 14,  -8),
            S(-60,  21),   S(  4,  39),   S(-20,  57),   S( 45,  55),   S( 28,  63),   S( 68,  64),   S( 58,  45),   S( 14,  16),
            S(-42,  50),   S(-11,   3),   S( 51,  28),   S( 49,  60),   S( 12,  67),   S( 52,  93),   S(-20,  19),   S(-40,   1),
            S( 15,  22),   S(  0,  25),   S(  9,  50),   S( -1,  37),   S( 61,  70),   S( 16,  85),   S( -4,  24),   S(-60,  -4),
            S( 18,  28),   S( 19,  49),   S( 16,  44),   S( 18,  49),   S( 57,  63),   S( 28,  42),   S(-22,   8),   S( -1,  20),

            /* queens: bucket 7 */
            S( -4,  -8),   S(-40,  18),   S(-36,  -5),   S(-16,   4),   S(-46,   1),   S(-48, -20),   S(-22,   3),   S(-34, -30),
            S(-47,   9),   S(-65,  43),   S(-15,  23),   S(  4,  25),   S(-16,  31),   S(-45,  22),   S(-68,  49),   S(-24, -12),
            S(-49,  -1),   S(-56,  59),   S( 10,  32),   S( -6,  40),   S(  9,  51),   S( -1,  53),   S(  5,  -4),   S(-14, -11),
            S(-47,  11),   S( 20,   2),   S(-10,  30),   S(-15,  56),   S( 56,  34),   S( 47,  31),   S(  8,  66),   S(  9,  37),
            S(-15,   6),   S(-47,  42),   S(-15,  30),   S( 38,  28),   S( 49,  17),   S( 82,  16),   S( 27,  39),   S( 37,   8),
            S( -8,  31),   S(-26,  22),   S( -5,  -6),   S( 26,  19),   S( 32,  45),   S( 69,  56),   S( 77,  31),   S( 68,  49),
            S(-10, -20),   S( 26,  26),   S( 54,  12),   S( 15,  35),   S( 31,  38),   S( 45,  34),   S( 45,  10),   S( 46,  38),
            S(  4,  27),   S( 16,  -3),   S( 45,  25),   S( 38,  26),   S( 48,  54),   S( 36,  20),   S( 18,  16),   S( 34,  57),

            /* queens: bucket 8 */
            S(-11, -21),   S(  3,   1),   S(-11, -13),   S(  0, -11),   S( -5, -10),   S(-13, -23),   S(-30, -49),   S(-11, -11),
            S( -6,  -2),   S( -8, -18),   S( -3,   6),   S(-14,  -3),   S(-21, -17),   S(-17, -33),   S(-18, -48),   S( -1,   0),
            S(  5,   7),   S(-22, -12),   S( -6, -11),   S(-10,  -8),   S(-13, -11),   S( -1,  -8),   S(  6,  -5),   S(-15, -25),
            S( -3,   1),   S( 13,  17),   S( 13,  16),   S( -3,   2),   S( -3,  -2),   S(-20, -22),   S( -1,   3),   S(-14, -35),
            S( 13,  33),   S( -3,  45),   S( 28,  54),   S( 18,  37),   S( 21,  43),   S( 10,  15),   S( -5,  -2),   S(-17, -27),
            S(  7,  15),   S( 12,  25),   S(-17,  16),   S(  3,  16),   S(  1,   1),   S( -5, -10),   S(  3,   8),   S(  0,   0),
            S( -9, -14),   S(-25, -20),   S( 12,  36),   S(  8,  12),   S(-10,  -6),   S( -8, -15),   S(-13, -32),   S( -7, -18),
            S(-27, -55),   S( 12,  32),   S(-26, -60),   S( -2, -10),   S(-14, -36),   S(-13, -20),   S( -9, -32),   S( -5,  -7),

            /* queens: bucket 9 */
            S( -4, -10),   S(-18, -42),   S( -2, -11),   S(-34, -43),   S(-33, -59),   S(-19, -27),   S(-13, -23),   S(-17, -37),
            S( -1,  -8),   S(-25, -48),   S(-19, -14),   S(-28, -38),   S(-28, -33),   S(-29, -42),   S( -2, -21),   S(-10, -25),
            S(  9,  15),   S(  1,  15),   S(-31,  -3),   S(-18, -22),   S(-22, -24),   S(-17,  -5),   S(-14, -27),   S(  2,   0),
            S(-12, -18),   S( -4,  -7),   S( 18,  58),   S(  4,  32),   S(  6,  24),   S( -5,   5),   S(-22, -34),   S(-10, -30),
            S( 17,  27),   S( -5,  16),   S( 10,  31),   S( 18,  59),   S(  3,  31),   S( -6,   5),   S( 16,  27),   S(-15, -19),
            S(-25, -36),   S(-20, -14),   S( -3,  17),   S(  3,  26),   S(-17, -22),   S( -6,  -1),   S(-19, -36),   S( -7,   0),
            S(-17, -27),   S(-14, -22),   S(-24,   5),   S(  8,  22),   S( 10,  12),   S(  4, -14),   S( -1,  -2),   S(-17, -29),
            S( -4,  -6),   S(-42, -68),   S(-12,   2),   S( 24,  41),   S( -1, -16),   S(-11, -23),   S(  1,  -3),   S( -8, -16),

            /* queens: bucket 10 */
            S( -4, -12),   S( -6,  -7),   S(-13, -28),   S(-30, -45),   S(-25, -39),   S( -3,   6),   S( -4, -15),   S(-16, -29),
            S( -9, -17),   S(-10, -17),   S(-14, -20),   S(-22, -26),   S(-11, -27),   S(-15, -19),   S( -6, -12),   S(-22, -29),
            S(-15, -25),   S( -3,  -9),   S(-31, -36),   S(-12,  -9),   S(-39, -22),   S(  3,  24),   S( -5,  -3),   S( -5,  -9),
            S( -8,  -1),   S(  0,  -4),   S(-19, -18),   S( -2,  13),   S( -2,  21),   S(-43, -25),   S( -2,  -3),   S(-37, -56),
            S(-15, -24),   S(-14, -26),   S(-16,   2),   S( 15,  32),   S( -3,   3),   S( -5,  10),   S(-13, -34),   S( -8,  -3),
            S(-13, -18),   S(-17, -19),   S(-28,  -6),   S(  5,  31),   S( 15,  46),   S(  2,  16),   S(  8,  11),   S( -7, -14),
            S(  2,   6),   S(-32, -48),   S( -1,  13),   S(-23, -27),   S( 10,  10),   S(  6,  32),   S( -5, -19),   S( -7, -11),
            S(  0,   2),   S(  0,   0),   S( -3, -14),   S(  3,  19),   S( 18,  21),   S( -1,   5),   S(  8,  14),   S(-12, -34),

            /* queens: bucket 11 */
            S(-12, -27),   S(-10, -48),   S(-32, -38),   S( -7, -31),   S(-12, -12),   S( -7,  -7),   S(-17, -23),   S(-27, -41),
            S(-16, -20),   S(-15, -21),   S(-50, -61),   S( -9,  -1),   S( -4,   9),   S(-18, -22),   S( -4,  -5),   S(-17,  -9),
            S(-26, -26),   S(-13, -27),   S(-12, -20),   S(-37, -61),   S( -8, -14),   S(  4,  26),   S(  1,  26),   S(-15, -14),
            S(-12, -38),   S(-20, -21),   S(-18, -37),   S( 12,  24),   S(  7,   9),   S( -6,   5),   S( 22,  38),   S(  7,   7),
            S(-13, -16),   S( -4, -20),   S(-25, -25),   S( 14,  34),   S( 26,  48),   S( 25,  66),   S( 14,  29),   S( -1,  12),
            S(-19, -41),   S(-10, -12),   S(-21, -21),   S(  3,   4),   S( 31,  23),   S( 62,  56),   S( 18,  23),   S(  3,  -5),
            S( -3,  -6),   S(-23, -36),   S(  8,   4),   S(-12, -10),   S(  6,  15),   S( 30,  55),   S( 33,  43),   S(-20, -36),
            S(-10, -16),   S(  0,  -6),   S(-20, -44),   S(  7,   0),   S(  5,  11),   S( -1,  -9),   S( 22,  31),   S( -1, -25),

            /* queens: bucket 12 */
            S( 11,  18),   S( -2,   0),   S( -7, -11),   S( -4,   1),   S(-11, -15),   S( -4,  -8),   S(  3,   7),   S(-16, -23),
            S(  0,   2),   S( -1,  -4),   S(-20, -43),   S(-12, -28),   S(-11, -25),   S( -4,  -6),   S( -2, -10),   S( -4,  -8),
            S(  2,   6),   S( -2,   2),   S( 22,  30),   S( -5, -14),   S(  3,   5),   S(-15, -30),   S( -9, -22),   S(-16, -33),
            S(  4,   4),   S(  0,   2),   S(  0,  -1),   S( 15,  37),   S( 11,  21),   S( -2,  -9),   S(  4,   9),   S(-11, -28),
            S(  5,   8),   S( 11,  25),   S( 34,  64),   S(  9,  29),   S( -7,   4),   S( -9, -17),   S(-17, -35),   S( -8, -17),
            S(  1,   8),   S(  6,   7),   S( 18,  29),   S( -1,  -4),   S(  5,  11),   S(  5,  15),   S(  6,  11),   S( -7, -21),
            S( -1,  -3),   S( 19,  41),   S( 30,  39),   S( 16,  26),   S( 15,  30),   S(  2,   3),   S(  8,  13),   S( -5, -11),
            S( -8, -30),   S(-23, -51),   S(-24, -34),   S( -4, -17),   S( -6, -10),   S(  0,   5),   S( -4, -12),   S( -8, -16),

            /* queens: bucket 13 */
            S(  0,   2),   S( -7, -20),   S( -5, -15),   S( -7,  -8),   S(-10, -22),   S(  0,  -7),   S( -7, -17),   S( -7, -13),
            S( -3,  -7),   S(  3,   9),   S( -2,  -7),   S( -6, -15),   S(-14, -26),   S( 10,  24),   S( -4, -11),   S(-10, -21),
            S( -5, -13),   S(  1,  -4),   S(-14, -20),   S(-15, -34),   S( -5,  -5),   S( -8, -24),   S( -1,  -5),   S(-11, -19),
            S( -6, -12),   S(  2,   3),   S(  7,   9),   S( 18,  32),   S( 21,  35),   S(-18, -47),   S(-11, -28),   S(-11, -27),
            S( -4,  -8),   S( -2,   5),   S( 28,  51),   S( 18,  37),   S( 27,  60),   S( -2,  -1),   S( -9, -27),   S( -9, -27),
            S( -3,  -8),   S( 13,  35),   S( 44,  86),   S( 15,  53),   S(-10,  -3),   S(  2,  10),   S(  8,  21),   S(-17, -36),
            S( -7, -14),   S(  3,  10),   S( -6,  11),   S( 14,  43),   S( -1,   5),   S( -2,  -7),   S( -2, -11),   S(  2,   0),
            S(-21, -45),   S( -5, -17),   S(  0,   3),   S(-14, -26),   S(  5,   3),   S(  5,  10),   S( -3,  -3),   S(-11, -23),

            /* queens: bucket 14 */
            S( -7, -14),   S( -6, -16),   S( -1,  -4),   S( -6,  -9),   S(  2,   4),   S( -2,  -6),   S( -5, -15),   S( -9, -22),
            S( -8, -20),   S(  5,  13),   S(  0,   2),   S( -7, -19),   S( -6, -12),   S( -8, -19),   S(  2,   3),   S( -8, -24),
            S( -4, -11),   S(-10, -21),   S( -1,  -4),   S( -4,  -8),   S(  3,   7),   S(-13, -28),   S(  0,   1),   S(-10,  -8),
            S( -8, -20),   S( 13,  21),   S( -6, -10),   S( 17,  36),   S( 14,  34),   S(  4,  18),   S( 15,  27),   S(  6,   2),
            S(  3,   6),   S( -5, -11),   S( -8,  11),   S(  8,   6),   S( 14,  29),   S(  4,  -5),   S(  5,  -2),   S( -4, -20),
            S( -9, -18),   S( -1, -10),   S( 14,  28),   S(  4,   8),   S( 10,  35),   S( 19,  43),   S( 11,  15),   S( -5, -17),
            S( 11,  25),   S(  8,   5),   S( 21,  38),   S( 22,  43),   S(  6,  15),   S( 10,  27),   S( 14,  21),   S(  3,   5),
            S( -3,  -5),   S( -7, -11),   S(-13, -27),   S( 11,  13),   S(-11, -24),   S(  2,   4),   S(  6,  13),   S(-14, -35),

            /* queens: bucket 15 */
            S(-12, -28),   S( -1, -10),   S( -6, -20),   S( -7, -23),   S( -1,  -5),   S(-12, -27),   S(-13, -31),   S( -7, -21),
            S( -8, -21),   S(  0,   0),   S( -2,  -9),   S( -3,  -7),   S( -4,  -2),   S( -5,  -5),   S( 15,  27),   S( -8, -24),
            S( -2, -16),   S( -1,  -6),   S(  3,   5),   S( -2,  -8),   S(  3,  -2),   S(  7,  14),   S( -6, -10),   S(  0,   0),
            S( -6, -13),   S(  3,   3),   S(-11, -16),   S( 14,  23),   S( -2,   4),   S(  1,   5),   S(  5,   3),   S(  1,  -2),
            S( -3,  -4),   S( -4, -13),   S(-11, -29),   S( -4, -12),   S( 15,  26),   S(  9,   7),   S( -8, -18),   S( -3, -11),
            S(  3,   1),   S( -5, -12),   S( -4, -13),   S(  1,  10),   S( -6, -20),   S( 10,  25),   S( -5,  -6),   S( -7, -16),
            S(  0,   0),   S(  7,   5),   S( 14,  29),   S( 11,  24),   S(  7,  11),   S( 25,  42),   S( 20,  36),   S(  0,  -1),
            S( -2,  -2),   S(  5,   6),   S( -2,  -6),   S(  1,   2),   S(  7,   6),   S( 10,   1),   S(  0,  -3),   S(-20, -37),

            /* kings: bucket 0 */
            S( -3,  55),   S( 16,  66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  68),   S(104,  70),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-18,  17),   S(-72,  23),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 18,  40),   S(  7,  41),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-43,  29),   S(-42,  21),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 21,  45),   S( 27,  38),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 20,  56),   S( -6,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 57,  86),   S( 12,  69),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24, -46),   S( 30, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58,  -3),   S( 13,  18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -3, -43),   S(-37, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 13, -13),   S( -8, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-18, -16),   S(-44,  -9),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  -7),   S(-26,   4),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 24,  21),   S(-19, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 42,  36),   S(-34,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86, -45),   S(-22, -11),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-63, -39),   S( 23, -14),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-11, -49),   S(-30, -53),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 58, -53),   S( 36, -48),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0, -52),   S(-41, -33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 62, -57),   S( 47, -44),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  8, -26),   S(-91, -38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 86, -31),   S(  8, -50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -45),   S( 39, -31),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49, -104),  S(  7, -37),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 20, -32),   S( 42, -32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 30, -63),   S( 20, -70),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60, -43),   S( 39, -41),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 41, -65),   S(  5, -57),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 53, -35),   S(-44, -56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-18, -55),   S(  5, -113),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king piece relative square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24, -22),   S(-36,   2),   S(  1,  -4),   S(-20,  20),   S( 18,  -1),   S( 26,  14),   S( 29,  -6),   S( 44,  -5),
            S(-10, -38),   S(-34,  -9),   S(-11, -15),   S( -6,   0),   S(  7,  10),   S(  4,  -3),   S( 23,  -7),   S( 21,   7),
            S(  8, -22),   S( -4, -20),   S( 35, -28),   S(  5, -18),   S( 15,   4),   S( 17,  19),   S(  2,  31),   S( 56,   6),
            S( 23, -29),   S( 46,  -9),   S( 63, -24),   S( 34,  10),   S( 26,  43),   S(-11,  85),   S( 21,  77),   S( 70,  57),
            S( 97, -128),  S(123, -107),  S( 91,  -8),   S( 38,  51),   S( 54, 153),   S( 24,  99),   S(  2, 176),   S( 96, 123),
            S(-237, -155), S(-225, -210), S( 71, -178),  S( 27,  90),   S( 86, 193),   S( 67, 162),   S(109, 137),   S( 43, 191),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  14),   S(-49,   9),   S(-34,   4),   S(-49,  41),   S(-24,   5),   S( -5,   4),   S( -4,  -6),   S(  1,  12),
            S(-54,  -4),   S(-39,  -5),   S(-43,  -4),   S(-23,   3),   S( -4,   4),   S(-20,   3),   S(-12,  -8),   S(-19,   7),
            S(-38,  12),   S(-12,   6),   S(-20,  -2),   S(  9, -20),   S( -3,  16),   S(-10,   7),   S(-26,  17),   S(  8,   8),
            S(-23,  40),   S( 31,   7),   S(-10,  11),   S( 32,  11),   S( 16,  11),   S(-18,  34),   S( 14,  21),   S( 40,  39),
            S( 17,  35),   S( 86, -18),   S(113, -124),  S( 68, -131),  S( 31,  13),   S( 25,  33),   S(-32,  70),   S( 48,  90),
            S(129, -57),   S(-22, -35),   S( -7, -194),  S(-12, -208),  S(-55, -73),   S(-42,  39),   S( 38, 149),   S( 58, 193),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  34),   S(-38,   6),   S(-35,   2),   S(-24,  19),   S(-45,  35),   S(-35,  15),   S(-19,  -9),   S(-25,  19),
            S(-54,  13),   S(-36,   5),   S(-45,   3),   S(-44,  24),   S(-44,  21),   S(-41,   4),   S(-22, -14),   S(-52,   9),
            S(-36,  33),   S(-28,  32),   S(-16,  17),   S(-26,  19),   S(-30,  27),   S(-20,   5),   S(-25,   4),   S(-19,   5),
            S(-14,  68),   S(-24,  54),   S( -8,  36),   S(  5,  26),   S(  2,  20),   S(-14,   9),   S( 12,  14),   S( 26,  10),
            S(-40, 133),   S(-49, 108),   S(-22,  44),   S( 38, -30),   S( 98, -119),  S( 76, -86),   S( 86, -17),   S( 34,  31),
            S(-50, 212),   S( 92, 105),   S(-46,  74),   S( -3, -89),   S(-53, -234),  S(-126, -242), S(-30, -58),   S( 97, -53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10,  13),   S(-12,   9),   S( -9,  12),   S(-10,  37),   S(-21,  55),   S(  9,  34),   S( -3,   9),   S(-11,  -5),
            S( -1,   3),   S(  4,   8),   S(-14,   7),   S(-16,  21),   S( -4,  26),   S(  6,   9),   S( -1,  -3),   S(-33,  -1),
            S( 13,  29),   S( -8,  50),   S(  5,  22),   S( -6,   6),   S( 16,   0),   S( 19,  -4),   S(  0,  -8),   S( -6,  -5),
            S( 16,  80),   S( -5,  92),   S( 13,  69),   S( 14,  38),   S( 33,   5),   S( 38, -19),   S( 24,  10),   S( 41, -14),
            S( -8, 167),   S(-31, 184),   S(-20, 169),   S( -1, 129),   S( 31,  67),   S( 80, -19),   S( 96, -86),   S( 81, -71),
            S( 34, 168),   S(  6, 261),   S(-44, 269),   S(-42, 227),   S(-48, 106),   S(-61, -147),  S(-163, -273), S(-243, -238),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 80, -20),   S( 38, -15),   S( 15, -21),   S(-22, -38),   S(-11,  -5),   S(  6, -12),   S(-14,  -8),   S(-49,  31),
            S( 32, -25),   S( 21,  -1),   S( 16, -21),   S( -4, -11),   S(-32, -16),   S(-25, -14),   S(-58, -10),   S(-46, -11),
            S( 82, -44),   S(125, -52),   S( 47, -31),   S(-21, -19),   S(-91,  22),   S( -3,   6),   S(-62,  23),   S(-56,  28),
            S(-58, -123),  S( 42, -149),  S( 59, -37),   S(-22,  21),   S(-18,  29),   S(-50,  56),   S(-14,  44),   S( 31,  37),
            S( 35, -102),  S(-45, -123),  S(  8, -91),   S( 47,  35),   S( 81, 105),   S( 34,  60),   S( 35,  80),   S( 33, 102),
            S( 31, -46),   S( -2, -70),   S( 24, -33),   S( 28,  30),   S( 45, 100),   S( 80, 177),   S( 46, 137),   S( 62, 130),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-64,  29),   S(-33,  15),   S(  6,  -3),   S( 78, -22),   S( 66, -17),   S( 17,  -5),   S(-16,  -2),   S(-74,  43),
            S(-59,  13),   S( -3,   5),   S( 24, -15),   S( 12,   4),   S(  4,   1),   S( 17, -17),   S(-38,  -8),   S(-87,  20),
            S(-14,   8),   S( 15,  15),   S( 79, -17),   S( 15,   9),   S( -2,  26),   S(-15,   4),   S( -8,   2),   S(-50,  33),
            S( 14,  16),   S( 36, -14),   S(  3, -84),   S(-31, -55),   S(-31,  -9),   S(-22,  17),   S( 49,   8),   S(-19,  45),
            S( 69,  12),   S( 39, -47),   S( 67, -109),  S(-22, -70),   S( 58, -59),   S( 28,  23),   S( 41,  47),   S( 23,  90),
            S( 44,  25),   S( 35,   5),   S( -2, -96),   S(-11, -95),   S(-29, -53),   S( 74,  34),   S( 87, 125),   S( 85, 105),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-81,  21),   S(-54,  -1),   S(-10, -15),   S(-10,   7),   S( 24,   5),   S( 19,   2),   S( 13,  -5),   S( -4,  19),
            S(-63,  -6),   S(-44, -10),   S(-27, -16),   S( 41,  -8),   S(-10,  17),   S( 15,  -9),   S(  4,  -9),   S(-13,   1),
            S(-38,  12),   S(-34,  12),   S(-11,   9),   S(  6,   2),   S( 32,  11),   S( 69, -20),   S( 53, -12),   S( 21,  -1),
            S(-24,  44),   S(-11,  28),   S(  9,  16),   S( 18,  -2),   S(-23, -62),   S( 35, -82),   S( 65, -36),   S( 79, -16),
            S( 38,  70),   S( 21,  38),   S( -2,  29),   S(  5, -46),   S( 17, -90),   S(-14, -70),   S( 74, -55),   S( 83,  17),
            S(129,  54),   S(121,  76),   S( 41,  29),   S( 49, -62),   S(  8, -113),  S(  2, -104),  S( 10, -42),   S( 95,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -11),   S(-53, -20),   S(  5, -37),   S(-42,  23),   S( 27,  -6),   S( 68, -18),   S( 60, -31),   S( 64,  -9),
            S(-36, -25),   S(-54, -15),   S(-23, -32),   S(-33,   8),   S(  4,  -3),   S( 45, -28),   S( 44, -26),   S( 49, -23),
            S(-35,  12),   S(-51,  12),   S(-37,   5),   S(-29, -13),   S(  4,   0),   S( 37, -27),   S( 90, -34),   S( 85, -33),
            S(  5,  27),   S(-60,  60),   S(-26,  48),   S(-11,  26),   S( 10,  -9),   S( 57, -61),   S( 58, -110),  S( 39, -141),
            S( 28,  63),   S(-46, 133),   S( 38, 104),   S(-12,  91),   S( -2,  40),   S(-17, -56),   S(-29, -127),  S( 43, -114),
            S(140, 100),   S(129, 117),   S(137,  95),   S( 73, 115),   S( 78,  13),   S(  7, -54),   S(-13, -76),   S( -6, -160),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 35, -14),   S(  1, -25),   S( 52,  -2),   S(-15, -35),   S(-27, -60),   S( 57, -60),   S( 34, -58),   S( 28, -39),
            S(-37, -53),   S(-11, -29),   S(-62, -51),   S(-56, -20),   S(-40, -41),   S( -8, -42),   S(-10, -51),   S(-47, -33),
            S(-45, -72),   S(  3, -58),   S(-32, -53),   S(-28, -49),   S( -9, -17),   S(-38, -17),   S(-46, -14),   S(-47,  -2),
            S(-32,  -2),   S(-35, -17),   S( 33, -14),   S( 38,   0),   S(  4,   1),   S(-12,  43),   S(-21,  31),   S( 11,  33),
            S( 21,  40),   S( -3, -29),   S( 17,  45),   S( 39,  80),   S( 69, 129),   S( 63, 111),   S( 18,  79),   S( -6, 112),
            S( 29,  86),   S(  0,  16),   S( 41,  94),   S( 33,  29),   S( 47, 113),   S( 59, 183),   S( 53, 136),   S( 11,  86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 45, -10),   S( 62, -15),   S( 34,  -7),   S( -1,   6),   S( 15,  -1),   S( 82, -45),   S( 64, -65),   S(-25, -18),
            S(-20, -47),   S(-40, -50),   S(-37, -54),   S(-28, -36),   S(-20, -35),   S( -5, -42),   S( 15, -51),   S(  2, -44),
            S(-92, -26),   S( 19, -68),   S(-12, -73),   S(-23, -50),   S( -7, -32),   S(  3, -46),   S(-46, -37),   S(-37, -13),
            S(-43,  -4),   S(-38, -51),   S(  3, -55),   S(-17, -20),   S( 17, -35),   S( 11, -17),   S(-19,   1),   S( 22,  -4),
            S(-18,  31),   S(  8, -14),   S( 21,   9),   S( 41,  37),   S( 29,  83),   S( 20,  45),   S( 22,  57),   S( 22,  66),
            S( -7,  75),   S( 34,  41),   S( 11,  31),   S( 32,   6),   S( 39,  94),   S( 22,  47),   S( 17, 107),   S( 28,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -58),   S( -3, -63),   S(  6, -23),   S(  3, -42),   S( 25, -24),   S(156, -35),   S( 85, -30),   S( 45, -33),
            S(-56, -48),   S(-68, -58),   S( 36, -71),   S( 49, -49),   S( 32, -55),   S(  6, -44),   S(  3, -40),   S( 38, -46),
            S(-56, -32),   S(-20, -45),   S( 17, -57),   S( 18, -36),   S(-26, -45),   S(  1, -71),   S(  2, -69),   S(  7, -44),
            S(-12, -20),   S(-20, -10),   S(-10, -23),   S(-27, -33),   S(-35, -40),   S(-41, -46),   S(-19, -39),   S(-24, -20),
            S(  7,  25),   S( 36,  37),   S( 57,  32),   S(  1,  -8),   S( 55,  -8),   S( 40,  38),   S(  3,  20),   S(  5,  31),
            S(  0,  44),   S( 15,  60),   S( 39,  52),   S( 20,  35),   S( 33,  55),   S( 17,  24),   S(  4,  17),   S( 26,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -41),   S( -8, -51),   S(  7, -46),   S( -7, -24),   S( -2, -24),   S( 70,  -7),   S(120, -40),   S( 77, -24),
            S(-17, -66),   S(-62, -60),   S(-23, -74),   S( 10, -57),   S(-15, -31),   S( -3, -33),   S(  6, -48),   S( 23, -64),
            S(-48, -18),   S(-15, -32),   S(-13, -24),   S( 20, -38),   S(-55, -32),   S( -8, -32),   S(-59, -61),   S(-10, -66),
            S(-20,  17),   S( 35,   8),   S( 23,  22),   S( 37,   0),   S(  2, -14),   S( -3, -43),   S(-40, -43),   S( -8, -42),
            S(  2,  34),   S( 11,  68),   S( 27,  95),   S( 37,  47),   S( 40,  62),   S( -4,  -7),   S( 20,  36),   S( 17,   2),
            S( 29,  66),   S( 50,  67),   S( 54, 104),   S( 63, 115),   S( 13,  42),   S( 53, 137),   S( 10,  22),   S( 25,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30, -75),   S(-10,  -8),   S(-17, -10),   S(  9,  23),   S( -2, -24),   S(-31, -42),   S(  9, -43),   S(  0, -23),
            S(-18, -48),   S(-17,  -8),   S(-45, -103),  S(-39, -57),   S(-46, -74),   S( -9, -34),   S(-22, -45),   S(-21, -53),
            S(-35,  17),   S( -3, -55),   S(-14, -93),   S(-24, -60),   S( -8, -28),   S(-25,  -1),   S(-59, -36),   S(-41, -44),
            S(-28, -11),   S( -6, -13),   S(-18, -40),   S( -1,  -3),   S( 29,  69),   S( -1,  79),   S(  0,  43),   S(-26,  50),
            S(  5,  17),   S( -3, -10),   S( 16,  39),   S( 12,  59),   S( 60, 169),   S( 34, 112),   S( 15, 147),   S( 18, 111),
            S( 11,  34),   S( 17,  50),   S( 25,  63),   S( 20,  88),   S( 26,  92),   S( 41, 142),   S(-31,  42),   S( -8,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -55),   S(-21, -48),   S(  3,   1),   S(  0,   1),   S( -4, -39),   S(-21, -29),   S(  2, -82),   S(-41, -34),
            S(-58, -66),   S(-31, -78),   S(-46, -68),   S(  8, -10),   S(-32, -54),   S(-45, -47),   S(-32, -77),   S( -8, -47),
            S(-25, -26),   S(-36, -42),   S(-16, -76),   S(-29, -59),   S(-13, -43),   S(-17, -39),   S(-33, -42),   S(-19, -38),
            S(-24,  38),   S( -6,  -7),   S(  2, -23),   S( -1,  26),   S( -7,  64),   S(-19,  51),   S(-19,   6),   S(-23,  19),
            S(-10,  34),   S( 13,  55),   S( -3,   1),   S( 35,  85),   S( 53, 142),   S( 34, 112),   S( 26, 116),   S(  4,  98),
            S( 10, 100),   S( 42, 117),   S( -3, -25),   S( 21,  72),   S( 31, 106),   S( 23, 122),   S(  1,  82),   S(  9, 102),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -99),   S( 14, -72),   S( -4, -36),   S(  4,  17),   S(-14, -38),   S(-16, -19),   S(  9, -33),   S(  6, -38),
            S(-29, -89),   S(-88, -89),   S(-14, -76),   S(-27, -101),  S(-16, -32),   S(-10, -49),   S(-36, -32),   S(-23, -43),
            S(-17, -60),   S(-40, -61),   S(-20, -58),   S( -8, -75),   S(-21, -53),   S(-16, -77),   S(-15, -67),   S(-50,  -9),
            S(-21,  -9),   S(-29, -18),   S( -1,   2),   S(  0,  28),   S( 18, -28),   S(-11, -10),   S( -4,   2),   S( -7,  27),
            S( -7, -30),   S( 24,  60),   S( -2,  30),   S(  1,  31),   S( 29,  68),   S(  0,  30),   S( 18,  83),   S( 29, 105),
            S( -4,  63),   S(  5,  41),   S( 38, 132),   S( 24,  84),   S( 22,  81),   S(  8,  54),   S( 40, 138),   S( 24, 139),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -33),   S(  1, -49),   S(-17, -49),   S(-11, -18),   S(-10, -17),   S(-35, -17),   S(-11, -10),   S(-12, -66),
            S( -6, -53),   S(-27, -72),   S(-23, -77),   S(-23, -65),   S(-25, -31),   S(-40, -11),   S(-39, -10),   S(-39, -55),
            S(-39, -16),   S(-49, -33),   S(-49, -38),   S( 29,  -7),   S(-26, -24),   S(-13, -50),   S( -4, -14),   S(-26,  12),
            S(-37,  17),   S(-24,  13),   S( 19,  37),   S( 10,  29),   S( 23,  43),   S(  4,  31),   S(  4,  10),   S( -3,  20),
            S( -5,  82),   S(  8,  68),   S( 17, 108),   S( 34,  90),   S( 37, 141),   S( 15,  78),   S( 16,  72),   S( 15,  51),
            S(-11,  63),   S( -1,  51),   S( 13, 109),   S( 25,  68),   S( 31, 117),   S( 17,  57),   S( 23,  50),   S( 33, 115),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-62, -36),   S(-22, -15),   S(-32,  21),   S( 10,  29),   S(-13, -17),   S(-16,  18),   S(-15,   9),   S(-87, -21),
            S( 41, -51),   S(  1,  -7),   S(-33,   5),   S( -2,  -2),   S(-10,   6),   S(-17,   0),   S(-28, -34),   S(-23,  -1),
            S( -9, -37),   S(  6,   0),   S(  1,  12),   S( 31,  43),   S(-12,  33),   S( -5,   9),   S(-27,   6),   S(-19, -26),
            S( 13,  13),   S( 10,  87),   S( 15,  58),   S( 43,  23),   S( 11,  51),   S(  5,  48),   S( 44, -23),   S(-27,  17),
            S( 39, 107),   S(-17,  83),   S( 40,  86),   S( 26,  65),   S( 79,  19),   S(  8,  34),   S( 11,  27),   S(  8,   4),
            S(111, -41),   S(-32,  56),   S(112,  64),   S( 96,  11),   S( 70,  44),   S(-32,  92),   S( 35,  10),   S( -2,  26),
            S( 56,   9),   S(  1, -14),   S( 34,  33),   S( 79,  58),   S( 80,  35),   S(  8,  37),   S(-15,  23),   S(-51,   7),
            S(-108, -156), S( -6,  -7),   S( 15,   3),   S( 41,  77),   S( 18,  42),   S( 23,  46),   S( -7,  26),   S(-35,  -3),

            /* knights: bucket 1 */
            S( 28,  22),   S(-64,  18),   S(-50,  23),   S(-47,  27),   S(-28,  47),   S(-26, -27),   S(-39,   7),   S(-14, -10),
            S(-59,  42),   S(-59,  70),   S(-31,  28),   S(-17,  27),   S(-20,  19),   S( -3,  37),   S(-30,  18),   S(-16, -40),
            S(-40,  32),   S(-15,  23),   S(-32,  34),   S(-22,  63),   S(-15,  46),   S(-28,  29),   S(-46,  27),   S(-39,  48),
            S(-25,  90),   S( 17,  47),   S( -5,  49),   S(-28,  65),   S(-10,  61),   S(-21,  73),   S(-30,  56),   S(-30,  39),
            S( 51,  -9),   S(-16,  24),   S( 25,  70),   S( -6,  63),   S( 17,  61),   S( -9,  65),   S(-12,  40),   S(-25,  77),
            S(-15,  38),   S( 58,  10),   S( 78,  37),   S( 92,  47),   S( 42,  47),   S(-18,  73),   S( 39,  32),   S( 17,  30),
            S( 35, -17),   S( 31, -18),   S( 42, -39),   S( 24,  62),   S( 21,  -4),   S( 29,  17),   S( 30,  55),   S(-41,  52),
            S(-191, -34),  S( 23,  -7),   S(-43, -59),   S(  3,   8),   S(-13,  16),   S( 34,  46),   S( 49,  56),   S(-71,  23),

            /* knights: bucket 2 */
            S(-69,  24),   S(-44,  25),   S(-42,  15),   S(-22,  23),   S(-10,  28),   S(-50,   8),   S(-31,  17),   S(-12, -18),
            S(-38,  37),   S(  4,  42),   S(-39,  35),   S(-21,  41),   S(-32,  39),   S(-25,  21),   S(  3,  23),   S(-33,  21),
            S(-44,  72),   S(-31,  41),   S(-35,  34),   S(-37,  75),   S(-32,  66),   S(-34,  30),   S(-33,  38),   S(-19,   9),
            S(-22,  73),   S(-28,  58),   S(-31,  98),   S(-40,  97),   S(-38,  82),   S(-15,  65),   S(  0,  43),   S(-14,  35),
            S(-20,  70),   S(-30,  76),   S(-14,  80),   S( -1,  72),   S(-32,  86),   S( -2,  77),   S(-38,  70),   S( 16,  14),
            S(-46,  85),   S(-25,  76),   S(-38, 105),   S( 42,  34),   S( 48,  49),   S(133,   9),   S( 71,  15),   S(  5,   3),
            S( 18,  55),   S(-53,  78),   S( 54,  20),   S( 12,  11),   S(-37,  46),   S( 32,   4),   S(  8,  13),   S( 16,  -3),
            S(-59,  26),   S( 24,  76),   S(-29,  69),   S(-27, -39),   S(-31, -14),   S(-52, -62),   S( 11, -25),   S(-150, -44),

            /* knights: bucket 3 */
            S(-78,  59),   S(-28, -13),   S(-20,   9),   S(-18,  41),   S(  0,  24),   S(-17,  11),   S(-25,   4),   S(-48, -39),
            S(-26,  14),   S(  2,  28),   S( -7,  36),   S(-11,  41),   S(-14,  36),   S( 17,  19),   S( 23,  -9),   S( 13,  -4),
            S(-26,  51),   S(-24,  37),   S(-13,  54),   S(-14,  88),   S( -1,  73),   S(-16,  54),   S( -4,  49),   S(  5,   2),
            S( -7,  43),   S( 10,  61),   S(  3,  83),   S(-14, 102),   S(  4, 104),   S(  5, 102),   S( 13,  87),   S( -8,  69),
            S(-11,  63),   S( -3,  75),   S(  8,  95),   S( 21, 114),   S(  0,  98),   S(  2, 123),   S(-31, 117),   S( 31, 105),
            S(-39,  73),   S( -1,  83),   S( 27,  88),   S( 14, 103),   S( 60,  99),   S( 84, 114),   S( 36, 127),   S(  9, 122),
            S(-18,  74),   S(-16,  66),   S(-25,  86),   S( 36,  81),   S( 24,  89),   S( 63,  58),   S(-24,  46),   S(109, -27),
            S(-158,  69),  S( -9, 100),   S(-50, 127),   S( 32,  65),   S( 78,  82),   S(-50, 104),   S(  2, -27),   S(-67, -119),

            /* knights: bucket 4 */
            S(  4,  25),   S( 17,  10),   S(-54,  10),   S(-41, -12),   S(-41,  16),   S(-17, -52),   S( 26, -26),   S(-21, -22),
            S( 21,  20),   S( 28, -26),   S(-13,  15),   S(-25,  18),   S(  7, -22),   S( 39, -46),   S(  7,  23),   S(-42, -11),
            S(-37, -16),   S( 24, -21),   S( 60,   4),   S( 78,  -6),   S( 12,  32),   S( 22, -14),   S(  1, -42),   S(  4, -23),
            S(-23, -39),   S( 24,   0),   S( 59,   0),   S( 22,  13),   S( -2,  39),   S(  4,  25),   S(-17,  31),   S( 13,   2),
            S( 19, -68),   S( 15,  31),   S( 91,  13),   S( 53,  50),   S( 55,  -9),   S( -4,  48),   S( 36, -25),   S(-30,  63),
            S( -9, -30),   S(-21, -43),   S( 39,  -6),   S( 49,  16),   S(-42,  25),   S( -5,  51),   S( -4,  -6),   S( 49,   9),
            S( -2,  -5),   S(-25, -40),   S( 17,  -5),   S(  8, -35),   S( 36,  56),   S( -5,  43),   S( 15,  38),   S(-36, -20),
            S( 10,  22),   S(-17, -50),   S(  2, -15),   S(  0, -11),   S( 11,  -3),   S(-24,  -8),   S( -2,  28),   S( -2,   4),

            /* knights: bucket 5 */
            S( 23,  20),   S( -4,  23),   S(-29,  39),   S(  2,  19),   S( -6,  32),   S( 14,  22),   S(-25,  23),   S( 11,  27),
            S( 29,  15),   S( 50,  26),   S( 30,  -8),   S( 10,   1),   S( 21, -14),   S( -2,   2),   S(-12,  49),   S(-70,  -1),
            S(-41,  23),   S( 11,  -9),   S( 27,  12),   S( 69,  11),   S( 20,  22),   S( 18,  18),   S(  0,  11),   S(-26,   6),
            S(  6,  17),   S( 34, -18),   S( 57,  17),   S( 83,  -2),   S( 99,   5),   S( 87,   1),   S(-11,   6),   S( 62,   5),
            S( 80, -11),   S( 61,  -9),   S(108,  -9),   S(118,   2),   S( 94,  11),   S( 37,  10),   S( 34, -14),   S( 38,  -1),
            S( 18, -15),   S( 23,  -1),   S( 17, -28),   S( 19,  44),   S( 38,   2),   S( 50,  10),   S( -7,  13),   S( 38,  28),
            S( -9, -14),   S(-16, -32),   S( -5, -55),   S(  3,  -8),   S(  7, -39),   S(  4,   6),   S( 19,  39),   S( 10,  39),
            S(-49, -71),   S(-28, -60),   S(  3, -32),   S(-19, -31),   S(-13, -24),   S( -1,  15),   S( 16,  47),   S( -3,  20),

            /* knights: bucket 6 */
            S(  4,  -5),   S(-43,  43),   S( 29,  -6),   S( -3,  27),   S( -5,  20),   S( 51,   5),   S(-24,  41),   S( -3,  41),
            S( 15,   8),   S(  0,  35),   S( 27, -10),   S( 66,  -3),   S( 64,  -5),   S(-37,  31),   S(-15,  46),   S(-50,  58),
            S( 17,  12),   S( 63,  -7),   S( 44,   8),   S( 46,  30),   S( 66,  26),   S(-25,  26),   S( 19,  26),   S(-42,  35),
            S( 49,  19),   S( 74,  -1),   S( 56,  20),   S( 89,   8),   S( 66,  14),   S(102,  11),   S( 61,  -2),   S(-24,  42),
            S(-11,  32),   S( 80,  -4),   S(124,  -1),   S(137,   5),   S(108,  -9),   S(106,  15),   S(168, -12),   S( 33,  34),
            S( 22,  12),   S( 32,   3),   S( 52,  12),   S( 50,  19),   S( 74, -14),   S( 42,  10),   S( 43,   3),   S( 20, -18),
            S( -2,  16),   S( 37,   9),   S( 32,  42),   S(  1,  -7),   S( 36,  29),   S( 28, -39),   S( -2, -21),   S( 10,  30),
            S( 30,  23),   S( 14,  27),   S( 22,  31),   S( 22,  20),   S( 30, -21),   S(  9,  -4),   S(  4,  39),   S(-34, -54),

            /* knights: bucket 7 */
            S(-11, -25),   S(-22, -35),   S( 33, -22),   S(-26,  -3),   S(-29,  17),   S(-57,  26),   S( 16,   1),   S(-16,  30),
            S(-24, -37),   S( -6,  -9),   S(-16, -12),   S(  6,   6),   S( 22,   3),   S( 25,  24),   S( -2,  15),   S(-33,  17),
            S( -7, -26),   S(-51,   2),   S( 38, -24),   S( 68,  12),   S( 86,  -1),   S( 52,   8),   S( 49,   3),   S(-42,  56),
            S(-22,  18),   S( 33,   4),   S( 76, -10),   S(111,  -3),   S(119, -16),   S(109,  18),   S( 39,   6),   S( 65,   6),
            S(-27,  23),   S( 14,   6),   S( 30,  19),   S( 97, -11),   S(151, -13),   S(205, -31),   S(182, -10),   S( 54,  11),
            S(-17,  -2),   S( 64,  -8),   S( 48,   8),   S( 58,  19),   S(124, -11),   S(140, -16),   S( 28,  -5),   S(-20, -42),
            S(  9,  18),   S(-28,  25),   S(  3,  22),   S( 66,  17),   S(101,   5),   S( 30,  16),   S(-44, -71),   S(-17, -14),
            S(-43, -38),   S(-27,   2),   S(  7,  33),   S(-14,  -8),   S(  0,  19),   S( 28,  40),   S(  2,  -5),   S(  4,   1),

            /* knights: bucket 8 */
            S( -1,  -4),   S( 13, -21),   S( 12,  -5),   S( -6, -42),   S(  1,  -3),   S( -7, -21),   S( 14,  15),   S(  0, -22),
            S(-11, -42),   S(-12, -69),   S( -8, -48),   S( -3, -33),   S(-10,   5),   S(  2, -36),   S(  5,  15),   S( -3, -20),
            S(-10, -45),   S(-17, -43),   S(  7, -39),   S( 18, -38),   S( 11, -28),   S( 45, -11),   S(  4, -46),   S(  7,  -6),
            S(-13, -59),   S( -6, -32),   S(  8,  -7),   S( 39,  10),   S( 22, -61),   S( -8, -47),   S(-14, -34),   S(-19, -45),
            S( -5, -26),   S( -7, -55),   S(  5, -27),   S( 30, -36),   S( 23, -13),   S(-14, -37),   S(  1, -19),   S( -8, -38),
            S(  2,  -1),   S(  8, -46),   S(  2,  -4),   S( 16, -13),   S(  2,  -8),   S( 18, -11),   S(-15, -15),   S(-16, -34),
            S(  2,  22),   S(  0, -37),   S(-24, -55),   S(  6,   8),   S( 11,   7),   S( 12,  -6),   S( -5,  -9),   S( -4, -10),
            S(  1,   1),   S( -3,  -7),   S( -9, -12),   S(  3,   2),   S(  0,  15),   S(  3,  10),   S(  1,  -3),   S( -6, -11),

            /* knights: bucket 9 */
            S(-21, -82),   S(  1,  -4),   S(-11, -71),   S(  0, -35),   S(-21, -41),   S(-19, -21),   S( -4, -20),   S(  4,  -3),
            S( -6,  -1),   S(-18, -59),   S(-13, -111),  S(-11, -69),   S(  8, -58),   S(-18, -77),   S(  1, -44),   S(-15, -26),
            S(-14, -71),   S( -8, -27),   S(-12, -43),   S( 12, -80),   S( -1, -18),   S( 30,  -6),   S( -7, -49),   S( -3, -29),
            S(-23, -63),   S(-18, -58),   S(  4, -40),   S( -6, -52),   S( -3, -52),   S( 15, -23),   S(-15, -57),   S( 18,  10),
            S( 10,  29),   S( -5, -20),   S(  9, -44),   S(  3, -33),   S( -6, -56),   S(  9, -47),   S( -5, -30),   S(  2, -23),
            S(-10, -34),   S(-20, -75),   S(-12, -35),   S(  6, -36),   S( 19,   0),   S(  3, -32),   S(  6, -35),   S(  4,  33),
            S(-19, -30),   S( -5, -22),   S(-17, -44),   S(-12, -32),   S(  3, -26),   S(  0,  -2),   S(-14,   1),   S( -8,  -8),
            S(  1,   4),   S(  1, -16),   S(-14, -10),   S(  2,  -5),   S(-11, -40),   S( -5,  14),   S(  1,   7),   S(  1,  -2),

            /* knights: bucket 10 */
            S( -2, -38),   S( -2,  -8),   S( -4, -28),   S(-12, -52),   S(-22, -79),   S(  2, -43),   S(  0,  -9),   S( -3,  -6),
            S(-10, -64),   S(  1, -32),   S(-17, -48),   S( -9, -66),   S(  5, -23),   S(-17, -84),   S(-25, -25),   S(  4,  45),
            S(  7,  -9),   S( -4, -53),   S(  4, -42),   S( 20, -59),   S( -1, -45),   S( -2, -32),   S( -4, -32),   S( -8, -20),
            S( -5, -53),   S(-10, -67),   S( 26, -35),   S( 32, -37),   S( 24, -13),   S( -7, -45),   S(-11, -82),   S( -3, -62),
            S(-14, -56),   S(  7, -63),   S( 16, -39),   S(  3, -48),   S( 15, -17),   S( -5, -75),   S( -7, -44),   S( -7, -42),
            S( -2,  12),   S(-12, -26),   S( -6, -21),   S( -1, -24),   S(-10, -29),   S(-15, -45),   S( -1, -31),   S(-12, -29),
            S(  8,  12),   S( -6,  -3),   S(-23, -36),   S( 17, -25),   S(-11, -48),   S( -8, -56),   S( -3,  -7),   S(-12, -42),
            S( -2, -14),   S( -1, -12),   S(  4,  21),   S( -2, -20),   S(  7, -15),   S( -4, -30),   S(  0, -15),   S( -2,   2),

            /* knights: bucket 11 */
            S( -6, -23),   S(-23, -50),   S(  2, -35),   S(  1, -10),   S(-41, -56),   S(  3,  -9),   S(-20, -41),   S( 15,  41),
            S(-18, -39),   S(-28, -62),   S(-11, -92),   S( 40, -32),   S( 28, -25),   S(-15, -72),   S(  0, -13),   S(-10, -28),
            S(-20, -54),   S(-17, -58),   S( 22, -19),   S( 33, -32),   S(  6,  11),   S( 31, -10),   S(  2, -52),   S(-19, -60),
            S( -9, -34),   S( 16, -19),   S( 22, -30),   S( 36, -16),   S( 50, -33),   S( 11, -27),   S( 16, -23),   S(  2,   0),
            S( -9, -12),   S(-15, -64),   S(  6,   2),   S( 23, -24),   S( 60,  15),   S( 31,  34),   S( -1, -82),   S( -8, -30),
            S( -9, -17),   S(  6, -49),   S( 11, -16),   S( 23, -17),   S(  5,  30),   S(-10, -19),   S( -7, -44),   S(  0,   2),
            S(  1, -11),   S( -6,   0),   S( -4,  -9),   S(  2, -23),   S(  3, -42),   S(  1, -83),   S( -1, -23),   S( -1,  -4),
            S(  1,  -4),   S(  2,   8),   S( -6, -17),   S( -8,  -1),   S( -7,  -9),   S( -2, -26),   S(  8,  22),   S( -5, -14),

            /* knights: bucket 12 */
            S(-15, -48),   S(  2, -16),   S(  4,  -6),   S( -7,  -5),   S( -3,  -3),   S(  1,  -9),   S(  1,   3),   S( -4, -11),
            S( -6, -37),   S(  1,   5),   S(  0, -17),   S(  0,  -9),   S(  0, -37),   S(  5,   6),   S(  5,   8),   S( -1, -11),
            S(  0,  -4),   S( -7, -31),   S( -4, -16),   S(  1, -61),   S( -5,  -7),   S(  9,  -2),   S( -1,  -2),   S( -6, -14),
            S(  2,   1),   S( -2, -63),   S( -3, -35),   S(  6, -12),   S( 16, -48),   S(  5,   1),   S(  6,  16),   S(  4,  23),
            S(  4,  17),   S(-14, -56),   S( -3, -43),   S( -7, -46),   S(  7,  24),   S( -3,  -5),   S( -6, -28),   S( -8, -11),
            S( -3,  -2),   S( -4, -11),   S( -5, -39),   S( 11,   0),   S( -8, -43),   S( -1, -28),   S(  8,   6),   S( -1,  -8),
            S( -2,  -9),   S( -3,  -8),   S( -9, -16),   S( -4,  -3),   S(  3,  13),   S( -9, -22),   S( -4, -10),   S( -8, -16),
            S(  0,   1),   S(  3,  27),   S(  1,  -2),   S( -1,  -6),   S(  2,   5),   S(  4,  19),   S( -1,  -1),   S(  0,   0),

            /* knights: bucket 13 */
            S(  0,   4),   S(  2,   3),   S( -9, -54),   S( -1, -33),   S(  0,  -3),   S( -6, -21),   S( -6, -13),   S(  4,  18),
            S( -4, -14),   S( -5, -24),   S(  3,  11),   S(-12, -33),   S(-14, -56),   S( -2, -16),   S(  1, -12),   S( -2, -11),
            S( -3, -11),   S(  1,  -1),   S( -7, -23),   S( -4, -33),   S( -2,   1),   S(-12, -47),   S(  7,  -7),   S( -2,  -1),
            S(  3,  11),   S(  1,   5),   S( -2, -18),   S(  3, -25),   S( -2, -49),   S( 18,  17),   S(  0, -13),   S( 12,   5),
            S( -3,   8),   S( -7, -52),   S( -5, -59),   S( -6, -61),   S(-23, -76),   S(  8, -16),   S( -9, -45),   S( -9, -28),
            S(  0,  -9),   S(  2,   4),   S(-10, -21),   S(  0, -31),   S( -6, -20),   S( -5, -48),   S(  6,  10),   S(  0,   1),
            S(  0,   3),   S(  4,  10),   S( -6,  -9),   S( -7, -37),   S(  1,  15),   S( -2,  -4),   S(  1,   3),   S(  0,   4),
            S(  1,   3),   S(  3,   8),   S( -2, -12),   S( -1,  -1),   S(  1,  -1),   S(  4,  11),   S(  1,   2),   S(  0,   1),

            /* knights: bucket 14 */
            S( -1,  -6),   S( -4,  -9),   S(  7,  24),   S( -5, -17),   S(-10, -38),   S( -3,  -3),   S(  3,  11),   S( -1,  -7),
            S( -5, -19),   S(-10, -43),   S(-10, -44),   S( -5, -58),   S(  0, -31),   S(  4,  -4),   S( -3, -21),   S(  7,  42),
            S( -2, -19),   S(  0, -24),   S( 10,   7),   S(-10, -81),   S(-13, -46),   S( 12,  22),   S( -4, -10),   S(  5,  43),
            S( -2,  -9),   S(  0, -24),   S(-20, -57),   S(  3,   9),   S(  3,  10),   S( -6, -49),   S(  6,  -2),   S(  0,  17),
            S(  1,   3),   S( -3, -24),   S( -3, -33),   S(  1, -30),   S(  5,  16),   S( -3, -31),   S( -3,   0),   S(  1,  -8),
            S( -2,  -9),   S(  2,  12),   S(  0,  53),   S(  3,   5),   S( -4, -23),   S( -4,   1),   S( -1,  25),   S( -6, -10),
            S( -1,  -8),   S(  0,   0),   S(  8,   8),   S( 10,  55),   S(  4,   3),   S( -5, -12),   S(  1,  -5),   S(  0,  -5),
            S( -1,  -2),   S(  1,   7),   S(  1,  -3),   S( -2,   3),   S( -2,  -8),   S( -1,   1),   S(  0,  -1),   S(  0,   2),

            /* knights: bucket 15 */
            S( -5, -21),   S( -2, -10),   S(  2,  14),   S( -3,  -9),   S( -4, -36),   S( -9, -29),   S( -4, -23),   S( -3, -30),
            S(  2,   6),   S(  5,  -8),   S( -8, -31),   S(  9,  31),   S(  1, -12),   S( -1, -24),   S( -4, -25),   S(  2,  -1),
            S(  8,  13),   S( -7, -26),   S( -6, -32),   S( 13,  -5),   S(-14, -78),   S(  3,  -6),   S( -4, -28),   S(  1,  -3),
            S( -4, -20),   S( -6, -18),   S( -2, -13),   S(  4,  -3),   S( 11, -37),   S( -2, -21),   S(  2,  -1),   S(  2,  20),
            S( -2, -29),   S( 11,  12),   S( -3, -25),   S( -4,   5),   S( 17,  16),   S( 17,  46),   S( -8, -41),   S( -2,  -4),
            S( -1,   7),   S(  1,   9),   S( -4,  -4),   S(-16, -40),   S(-15, -31),   S(  4,  28),   S(  2, -18),   S(  7,  22),
            S( -2,  -8),   S( -3,  -9),   S(  3,   9),   S(  7,  25),   S( -1,  10),   S(  8,  18),   S(  1,   8),   S(  6,   7),
            S(  3,   6),   S( -2, -16),   S(  2,   9),   S( -1,  -7),   S( -1,  -3),   S(  2,   8),   S(  1,   6),   S(  1,   5),

            /* bishops: bucket 0 */
            S( 13, -14),   S( -7,  30),   S(-10,   1),   S( -1, -39),   S( 12, -24),   S( 23, -31),   S( 76, -59),   S( 11,  10),
            S(-35, -21),   S(  8, -24),   S( -1, -26),   S( 12, -13),   S( 21, -13),   S( 52, -18),   S( 47,  14),   S( 63, -48),
            S( 17,   3),   S( 15,   1),   S( 20,   1),   S( 24,  -1),   S( 43, -18),   S( 49,  16),   S( 53, -22),   S( 29,  -2),
            S( 10, -21),   S( 55, -39),   S( 20,  -5),   S( 76, -34),   S( 66,  21),   S( 38,  24),   S( 10, -12),   S( 20,   5),
            S( 50, -12),   S( 43, -27),   S( 69, -11),   S(101,  -8),   S(127, -32),   S( 21,  19),   S( 44,   1),   S( -6,  32),
            S( 20,  25),   S( 85, -15),   S( 98,  -2),   S( 30,  18),   S( 39, -10),   S( 52,   6),   S( 48,  -3),   S( 17,   0),
            S(-75, -127),  S( 79, -40),   S( 91,  20),   S(-29,  11),   S( 38, -30),   S( 42, -26),   S(  0,  19),   S(-13,  47),
            S(  7, -32),   S(-11, -15),   S( 15, -49),   S(-26, -27),   S(-20, -16),   S(-26,  -2),   S(-27,  31),   S(-29, -21),

            /* bishops: bucket 1 */
            S(-51,  26),   S(  7,  -7),   S(-21,  27),   S( 30, -11),   S( -2,   5),   S( 22,   2),   S( 50, -27),   S( 47, -16),
            S( 18, -20),   S( -8,  -6),   S( 15, -20),   S( -7,   0),   S( 47, -22),   S(  2, -15),   S( 64, -13),   S(  5, -19),
            S(-19,  -5),   S( 34, -12),   S(  2, -16),   S( 36,  -8),   S(  4,  -6),   S( 54, -19),   S(  3,  -1),   S( 85,  -7),
            S( 45, -28),   S( 54, -29),   S( 31,   1),   S( 23, -11),   S( 62, -12),   S(  2,  -5),   S( 69, -25),   S( -8,   5),
            S( 26, -43),   S( 75, -17),   S(  5,  10),   S(113, -28),   S( 42,   2),   S( 97, -41),   S(  8,  -3),   S( 21,   4),
            S( 66, -48),   S( 50, -17),   S( 81, -16),   S( 89, -59),   S(136, -55),   S(-19,   5),   S(  0,  24),   S(-33,  -7),
            S(-13, -66),   S(  8, -20),   S(-11, -57),   S( 30, -10),   S( 65,  -9),   S( 33,   4),   S(  6,  10),   S(-44,  17),
            S( -7, -27),   S(-24,   7),   S(-11, -57),   S(-47,  22),   S(  2, -19),   S( 19, -43),   S( 20, -14),   S(-65, -53),

            /* bishops: bucket 2 */
            S( 31, -17),   S(-14,   3),   S(  8,  12),   S( -8,   8),   S( 17,  20),   S( -9,  10),   S( 15,  -5),   S(  7,  24),
            S( 13, -15),   S( 20, -15),   S(  0,  -3),   S( 20,   7),   S(  1,  15),   S( 10,  -1),   S( 13,   0),   S( 26, -39),
            S( 32,  10),   S( 26,  -8),   S(  5,  16),   S(  6,  -4),   S( 14,  14),   S(  1, -10),   S(  9, -24),   S( -8,  15),
            S( 26, -21),   S( 71, -27),   S(  4,   3),   S( 48,  11),   S(  8,   9),   S( -5,  20),   S(-28,   9),   S(  9,  13),
            S( 17,  -8),   S( 17,  13),   S( 70, -13),   S( 33,   5),   S( 35,  18),   S( 19,  14),   S(  8,  19),   S( 24,  -6),
            S(-30,  33),   S( 21,  15),   S(  1,   0),   S(103, -40),   S( 56, -14),   S(117, -10),   S( 49,   5),   S(  6, -33),
            S(  3,  22),   S( 25, -13),   S( -1,  12),   S( 49,   6),   S(-77, -51),   S(-49,  -6),   S( -8,  18),   S(-12, -43),
            S(-83,  -4),   S(-26,  33),   S( -5,   3),   S(-52,   6),   S(  2, -26),   S(-19,   5),   S(-13, -39),   S(-44, -11),

            /* bishops: bucket 3 */
            S( 41,  10),   S( 47, -30),   S( 13, -12),   S( 13,   6),   S( 22,  19),   S(  2,  42),   S( -6,  65),   S( 13,   3),
            S( 28,  15),   S( 28,  -4),   S( 23,   6),   S( 21,  11),   S( 27,  20),   S( 26,   2),   S( 17,  15),   S( 36, -30),
            S( -5,  12),   S( 34,  38),   S( 24,  33),   S( 27,  31),   S( 24,  29),   S( 22,  15),   S( 21,   4),   S( 23,  15),
            S( -8,  14),   S( 26,  20),   S( 26,  43),   S( 48,  31),   S( 40,  29),   S( 18,  15),   S( 26,  -9),   S( 36, -29),
            S( 15,  10),   S( 21,  43),   S( 12,  43),   S( 53,  50),   S( 52,  44),   S( 45,  31),   S( 13,  34),   S( -5,  31),
            S( 23,   7),   S( 35,  39),   S( 31,  23),   S( 28,  27),   S( 54,  23),   S( 46,  70),   S( 50,  55),   S( 18,  80),
            S(  0,  35),   S( 24,  43),   S( 21,  20),   S(-10,  53),   S( 21,  45),   S( 29,  75),   S(-18, -15),   S( 20,  -2),
            S(-49,  50),   S(-27,  51),   S(-75,  50),   S(-53,  56),   S(-21,  28),   S(-116,  48),  S( -8,   4),   S(-18,  -8),

            /* bishops: bucket 4 */
            S(-38,  36),   S(-28,   8),   S(-33,  15),   S(-40,   1),   S(-39, -17),   S(-33, -38),   S( -7,  -1),   S(-34, -43),
            S(-32,  47),   S(-46,   1),   S( 58, -28),   S(  5, -12),   S(-41,   1),   S( 12, -44),   S( -3, -42),   S(-23,  -9),
            S(  1,  38),   S(-33,  10),   S( 26,  13),   S( 30, -38),   S( 44, -40),   S(-23,   2),   S(-16, -25),   S(-55, -13),
            S( 21, -28),   S( 48,  -3),   S( 66, -42),   S( 27,  -1),   S(-24,  13),   S( 53, -15),   S(-49,  -7),   S(  0, -10),
            S( -3, -19),   S( -7, -81),   S( 27, -43),   S( 70, -39),   S( 42,  -8),   S(  8,   2),   S(-10,  20),   S(-45, -11),
            S(-43, -83),   S(-20, -51),   S( 39, -33),   S( 35, -15),   S(-14,   8),   S( 39,  10),   S( 10,  16),   S( -7,  19),
            S(  7,   7),   S(-10, -24),   S(-22, -66),   S(  4, -37),   S(-34,  -1),   S( 10,  13),   S(  3,   7),   S( 12,  28),
            S( -4, -13),   S(-14, -33),   S( -7, -38),   S( -4, -39),   S(-21, -44),   S(-14, -12),   S( -6,  54),   S(  6,  27),

            /* bishops: bucket 5 */
            S(-52,  38),   S( -7, -12),   S(-59,  11),   S(-83,  31),   S(  5,  -4),   S(-28,  -9),   S(-35,  12),   S(-26, -26),
            S(-21,  -8),   S(-23,   5),   S( 18,  -6),   S( 19,  -8),   S(-56,  16),   S( 11,  -8),   S(-30,  -2),   S( 10,   0),
            S( 16,  12),   S(-42,   3),   S( 22,   0),   S(-14,   4),   S( -2,   2),   S(-45,   9),   S(  1,  23),   S(-15,   8),
            S(  5,   1),   S(-56,  -8),   S( 43, -11),   S( 72, -16),   S(  3,  -7),   S( 39, -18),   S(-73,   2),   S(-17,   4),
            S( 31, -21),   S(  3,  -9),   S( 19, -36),   S(  1, -49),   S( 16, -28),   S(-14,  -7),   S( 23,   2),   S(-40, -19),
            S( -7, -10),   S( -2,  -2),   S( 30, -48),   S(-17, -38),   S(-36, -19),   S( 15, -18),   S(-22,   9),   S( -5,   9),
            S(-27, -30),   S(-10, -31),   S(-18,  -3),   S(  2,   7),   S(  9, -33),   S(-15,  -5),   S( 21,  26),   S(-26,   2),
            S(-14, -20),   S(-29, -19),   S(-10,  -9),   S(  1, -32),   S(-46,  -4),   S(-15,  11),   S(-22, -14),   S( 14,  14),

            /* bishops: bucket 6 */
            S(-48,  -6),   S(-46,  17),   S(-31,   7),   S(-18,  17),   S(-40,  25),   S(-21,  16),   S(-39,  27),   S(-65,  33),
            S(-28,   8),   S(-23, -11),   S(-55,  27),   S(-21,  11),   S(-34,  23),   S(-27,   2),   S(-51,  39),   S(-29,  15),
            S( 29,  -4),   S(-54,  23),   S( 25, -21),   S(-30,  19),   S(-19,  20),   S(-41,  18),   S(-20,   1),   S(-54,  43),
            S(-35,  14),   S(-46,  -5),   S( -7,   9),   S( 63,  -8),   S( 22,  12),   S( 41,  -3),   S( 20, -16),   S(-39,   9),
            S(-35,  14),   S(-17,  14),   S( 32, -13),   S( 82, -16),   S( 23, -36),   S(  9, -20),   S( 51, -26),   S(-30, -19),
            S(-60,  34),   S(-40,  14),   S(-30,   2),   S(-12,  -7),   S( -8,  -1),   S(-19, -24),   S(  3,  19),   S( 10, -18),
            S(-25,   9),   S(-56,  37),   S(  1, -12),   S(-17, -12),   S(-44,   4),   S(-10, -13),   S( -1, -14),   S(-46,  -5),
            S(-24,  16),   S(-28, -15),   S(-34,  13),   S( -7,  -7),   S( -5,   6),   S( 44, -36),   S(-17,   9),   S( -6, -13),

            /* bishops: bucket 7 */
            S(-14, -16),   S(-56,  -8),   S(-47, -30),   S(-53,  -1),   S(-39,  -6),   S(-36,  -5),   S(-68,  -7),   S(-57,  33),
            S( -7, -40),   S( 16, -43),   S( 13, -37),   S( -4, -33),   S(-24,   7),   S(-36,  -8),   S(-22, -31),   S(-44,  26),
            S(-46, -14),   S(-41,   6),   S(  6, -19),   S( 16, -13),   S(-15,  -5),   S( -5, -17),   S(-80,  15),   S(-115,  61),
            S(-35, -26),   S(-58,   5),   S( -1,  -8),   S( 25, -16),   S( 94, -11),   S( 20, -22),   S( 33, -25),   S(-30, -17),
            S(-35,  -6),   S( -2,  -5),   S( 35, -20),   S( 15, -12),   S( 50, -21),   S( 67, -34),   S( 34, -39),   S(-18, -21),
            S(-67,   2),   S(-16,  22),   S(-16,  -4),   S(-69,   3),   S( 10, -21),   S( 66, -33),   S(  2, -18),   S(-80, -82),
            S(-59,  -2),   S(-47,  26),   S(-54,  22),   S( 26,  -2),   S( 26, -44),   S(  3, -29),   S( -4,  -4),   S(-12, -30),
            S(-16, -34),   S(-58,  13),   S(-22,   8),   S(-21, -20),   S(  5, -16),   S(-16, -15),   S(  6, -38),   S( -3, -22),

            /* bishops: bucket 8 */
            S( 45, 157),   S( -8,  24),   S(  2, -11),   S( -9,  31),   S(  4,  17),   S(  0, -24),   S(-23, -57),   S( -6, -20),
            S( 11,  54),   S( 17,  86),   S(  3,  50),   S( -7,   1),   S(  5, -13),   S(-13, -12),   S(-28, -36),   S( -7, -12),
            S( -6, -26),   S(-12, -33),   S( 19,  49),   S( -1,  -9),   S( 43,  23),   S( 22,  -1),   S(-12, -22),   S(-31, -55),
            S(  1,   1),   S( 15,  63),   S(  1,  -4),   S(  4,  29),   S( 42,  11),   S( 24,  30),   S(  3,  11),   S( -5, -18),
            S(  9,  78),   S( 22,  94),   S( -5,  21),   S( -6, -12),   S(-16,  46),   S(-30,   4),   S( -1, -36),   S(  6,   2),
            S(-15, -22),   S( -7,  38),   S( 10,  33),   S( 10,  23),   S( -6,   7),   S(  4,  48),   S( -4,  31),   S(  5,  41),
            S( -1,  21),   S(-20, -56),   S( 12,  55),   S(-10,  47),   S(  2,  17),   S( 14,  46),   S(  8,  84),   S(-20,  14),
            S( -5,   5),   S(  6,  21),   S( -3,   5),   S(  2,  27),   S(  8,  28),   S(  8,  45),   S(  6,  57),   S( 22, 125),

            /* bishops: bucket 9 */
            S( 16, 100),   S( -2,  53),   S(-18,  44),   S(-35,  -7),   S(-27,  -6),   S(-15, -28),   S(-20, -29),   S( -1,   6),
            S(  4,  39),   S(-10,  47),   S( 13,  36),   S(-16, -17),   S( -5, -33),   S(  1, -34),   S(-29,  -9),   S(-13, -33),
            S(  5, -15),   S(  3,  22),   S( -4, -34),   S( -4,  16),   S( 10,  -3),   S(-24, -12),   S(  0, -14),   S(-10, -23),
            S( -9,  25),   S(-19,   3),   S(  1,  11),   S(-24,  -4),   S(-22,  23),   S(-16,   1),   S(  9, -10),   S( -5,   0),
            S(  6,  34),   S(-19,  18),   S( 15, -11),   S(-29,  -3),   S(-18,  14),   S(-20,  11),   S( -8,  24),   S( -4, -21),
            S( -5,  22),   S(-29,   2),   S(-18,   5),   S(  5,   5),   S(-12,  14),   S(-21,  24),   S(-11,  25),   S( -5,  10),
            S(  2,  25),   S(-14,  20),   S(-14,  12),   S( -4,   7),   S( -7,  15),   S(-10,  20),   S(  8,  41),   S(  8,  80),
            S(  4,  56),   S( -4,  -8),   S(  1,  15),   S( -4,  -3),   S( -2,  23),   S( -2,  36),   S( -4,  57),   S( 25,  91),

            /* bishops: bucket 10 */
            S( -9,  -6),   S(  7,  77),   S(-23, -33),   S(-22, -41),   S(-50, -17),   S(-40, -33),   S(-17,  46),   S( -5,  11),
            S(  1,  35),   S(-21, -31),   S( -4,  -9),   S(-43, -45),   S(-49,  -5),   S(-42, -18),   S(-18, -27),   S( -9,  10),
            S( -5, -29),   S(-15, -39),   S(-22,  -8),   S( -9,  18),   S(-31, -44),   S( -1, -37),   S( -8,  37),   S(  6, -26),
            S(-14, -25),   S(-31, -30),   S(-47, -15),   S( -6,  -3),   S(-23, -23),   S( 12,   8),   S(  0,  37),   S(-11,  18),
            S(-11,  17),   S(-33,   9),   S( -9,   0),   S(  9,   6),   S( -5, -12),   S( -6,  29),   S(-27,  14),   S(  3,  19),
            S( -9,  22),   S( -6,  21),   S(-27,   9),   S(-38,  23),   S(-23,   3),   S(-12,   5),   S(-16,   5),   S(  1,  35),
            S( -1,  21),   S(-16,  10),   S(-14,  10),   S( -2,  25),   S( -5,  23),   S(-25, -32),   S( -6,   5),   S( 12,  56),
            S(-16,  30),   S(  1,  55),   S(  2,  36),   S( -9,  18),   S(-15,  13),   S(-13, -15),   S(  1,  -7),   S( -5,  -4),

            /* bishops: bucket 11 */
            S( 11,  18),   S(-14,  -9),   S(-44,   6),   S(-15,   5),   S(-25, -38),   S(  6,   3),   S(-10,   8),   S(-10, 126),
            S(-13,  -1),   S(  8, -23),   S(-12,   7),   S(-10, -18),   S( -5, -22),   S(-42,  -5),   S(-51,  42),   S(  4,  54),
            S( -5, -37),   S(-17, -25),   S(  7, -33),   S(-23, -29),   S(-18,   0),   S( 23,  31),   S(-15, -62),   S( -1,   4),
            S(  1,  -9),   S( -5, -28),   S( 21,  -2),   S(-29, -25),   S( 12,  24),   S( 12,  43),   S( 22,  45),   S(-15, -52),
            S(-29,  33),   S(-23,  -4),   S(-17,  11),   S(-16,  36),   S(  2,   9),   S( 13,  29),   S( -7,  28),   S(  6,  50),
            S(-21,  15),   S(-17,   3),   S(-15,  27),   S(-23,  23),   S(-26,  30),   S(-14,  32),   S(  1,  17),   S( -7,  11),
            S(-14,  24),   S(-12,  80),   S(  5,  40),   S(  4,  23),   S( -1,  10),   S( -4,  16),   S(-11, -50),   S( -9, -10),
            S( 15, 127),   S(-17,  -1),   S(  1,  43),   S( -1,  37),   S(  1,  31),   S(  2,  33),   S(-16, -11),   S(  6,   7),

            /* bishops: bucket 12 */
            S( -6, -23),   S( -4, -24),   S( -1,  -3),   S( 14,  57),   S( -9, -27),   S( -5,   6),   S( -2,  -4),   S( -2,  -1),
            S(  1,   4),   S(  8,  40),   S( -2,  -8),   S(  1, -18),   S( -2,  -8),   S(  5,  -1),   S( -9,  -7),   S( -1,  -5),
            S(  5,  29),   S(  6,  44),   S(  5,  21),   S( 12,  12),   S(  6,   8),   S(-10, -51),   S( 10,  21),   S( -6,  -6),
            S( 10,  70),   S( 13,  75),   S(  5,  37),   S( 16,  44),   S(  0, -25),   S( -3, -20),   S( -2,  -1),   S(  4,   7),
            S(  6,   9),   S(  1,  15),   S(  1,  15),   S(  4,  38),   S(  6,  21),   S( 14,  27),   S( -2, -13),   S(  6,  28),
            S( -1,  14),   S(-10, -33),   S( -5,   7),   S( -2,   0),   S( 13,  67),   S( 16,  53),   S(-18, -54),   S(  0, -13),
            S( -2,   2),   S(  6,  27),   S(  1,   0),   S(  8,   3),   S(  5,  23),   S( 12,  63),   S(  9,  48),   S(  9,  56),
            S(  1,   4),   S(  1,  11),   S( -3,  -7),   S(  1,   8),   S(  2,  15),   S(  6,  32),   S( 11,  70),   S(  7,  37),

            /* bishops: bucket 13 */
            S( -1,  22),   S(  6,  32),   S( -5, -18),   S(-12, -15),   S(  4,  27),   S(-18, -12),   S(-11, -24),   S( -3, -30),
            S( -1,  43),   S(  0,  30),   S( -7,  -4),   S(  8,  75),   S( -9,  13),   S( 18,  35),   S(  1, -11),   S(  3,   6),
            S(  6,  28),   S( 14,  70),   S( -7,  -6),   S( 23,  38),   S(-16, -26),   S( 13,  35),   S( -5,   1),   S(-11,  -6),
            S( 20,  87),   S( 11,  86),   S( 12,  52),   S(-14,   8),   S(  2,  38),   S(-16, -45),   S( 14,  37),   S( -1,  11),
            S(  1,  46),   S(  0,  29),   S( -3, -17),   S(  8,  25),   S( -2,  16),   S( -3,  28),   S( -2, -12),   S( -2,  -3),
            S( -5,  22),   S(  2,  14),   S( -2,  25),   S( 15,  25),   S( -4,  57),   S( -2,  -9),   S(-11,   1),   S(  5,  31),
            S( 11,  46),   S(  2,  -2),   S( -4,   1),   S(  2,  26),   S( -4,  -3),   S(  5,  53),   S(  4,  19),   S(  0,  33),
            S(  0, -12),   S( -7, -14),   S( -2,  -4),   S(  5,  16),   S(  9,  41),   S( -3,   8),   S(  7,  38),   S(  8,  35),

            /* bishops: bucket 14 */
            S(-10, -16),   S(  6,  29),   S( 10,  14),   S(  7,  52),   S(-16, -16),   S(  2,   8),   S( -3,  -3),   S(  0,   5),
            S(  0,  12),   S(  4,  11),   S(  4,  25),   S( -7,  -3),   S( 12,  45),   S(  3,  41),   S( -8,  23),   S(  4,  33),
            S( -3,  -6),   S( -3,  30),   S( -1,   8),   S(  6,  15),   S( 18,  58),   S( 17,  69),   S(  8,  64),   S(  1,  45),
            S(  2,  36),   S(  4,  20),   S(-13, -16),   S(-10,  42),   S( -6,  41),   S(  0,  42),   S( 15,  92),   S( -3,  37),
            S(  7,  31),   S(  1,  11),   S(-15,  24),   S( 10,  45),   S( -6, -14),   S(  4,  21),   S( 24,  64),   S( -6,  11),
            S( -4,  10),   S( 10,  26),   S(  0,  30),   S(  3,  34),   S( -1,  27),   S( -8, -17),   S( -4,  -8),   S(  2,  12),
            S( 12,  78),   S(  6,  35),   S(  1,  28),   S( -3,  -1),   S(  0,  24),   S( -4,  -4),   S( -5, -34),   S(  3,  30),
            S(  2,  42),   S(  0,  25),   S(  4,  23),   S(  5,  30),   S( -4,  -8),   S(  1,  -5),   S( 12,  18),   S(  4,  17),

            /* bishops: bucket 15 */
            S( -1,  -9),   S( -2,  -3),   S(-13, -29),   S( -1,  -5),   S(-18, -11),   S(  5,  -3),   S( -4, -34),   S( -3, -15),
            S( 12,  47),   S( -8, -18),   S(  4,  10),   S(  3,  18),   S( 14,  15),   S( -9, -19),   S( -4,  -5),   S( -5,  -4),
            S(  2,   4),   S( -8, -19),   S(  4,  24),   S( 15,  21),   S(  7,  39),   S( 14,  44),   S(  6,  37),   S(  6,  37),
            S(  1,  -3),   S(  9,  36),   S(  3,  27),   S(-23, -15),   S(  6,  32),   S( 10,  34),   S( 11,  50),   S(  6,  54),
            S( -6, -12),   S( -5, -16),   S(  2,  23),   S( 18,  57),   S( 13,  69),   S(  6,  18),   S( -1,  17),   S( -4,  -6),
            S( -1,  -5),   S( -8, -16),   S( 16,  77),   S( 14,  34),   S( 23,  66),   S(  4,  54),   S( -1,   1),   S( -6, -21),
            S(  3,  27),   S(  5,  26),   S( -8,   3),   S(  9,  24),   S( 13,  60),   S(  0,   5),   S( -1,  19),   S( -1,   4),
            S(  4,  18),   S(  6,  29),   S(  2,  30),   S(  7,  21),   S(  5,  23),   S(  0,   4),   S(  4,  17),   S(  7,  33),

            /* rooks: bucket 0 */
            S(-16,  30),   S( 11,   5),   S(  0,  20),   S(  2,  26),   S(-12,  61),   S( -9,  56),   S(-28,  57),   S(-56,  71),
            S(-12,   3),   S( 16,  16),   S(-47,  58),   S(  7,  43),   S( -5,  61),   S( -1,  30),   S(-15,   1),   S(-32,  88),
            S( 14, -15),   S(-13,   5),   S(-12,  28),   S(  3,  28),   S(-44,  96),   S(-21,  44),   S(-16,  37),   S(  2,  41),
            S(-15,  11),   S( 34,  16),   S(-41,  48),   S(  5,  38),   S( 14,  69),   S(-12,  60),   S(-27,  57),   S(-17,  50),
            S( 15, -41),   S( 23,  24),   S( -4,  25),   S( 16,  42),   S( 31,  45),   S( 44,  77),   S( 51,  52),   S( -4,  68),
            S( 43,  -1),   S( 70,  45),   S( 79,  33),   S(112,  33),   S(  3,  79),   S( 53,  55),   S( -2,  81),   S(-36,  89),
            S( 48,  31),   S( 17,  66),   S(131,  58),   S( 73,  35),   S(113,  37),   S( 30,  75),   S( -7,  86),   S(-28,  90),
            S( 25, -30),   S( 25,  14),   S(  7,  22),   S( 52,  26),   S( 70,  55),   S( 68,  37),   S( 44,  44),   S( 81, -18),

            /* rooks: bucket 1 */
            S(-58,  50),   S(-25,  15),   S(-12,  32),   S(-35,  36),   S(-31,  50),   S(-32,  46),   S(-49,  60),   S(-77,  69),
            S(-41,  28),   S(-27,  12),   S(-45,  46),   S(-41,  52),   S(-27,  26),   S(-64,  53),   S(-45,  33),   S(-48,  59),
            S(-54,  36),   S(-32,  14),   S(-19,  14),   S(-39,  47),   S(-61,  43),   S(-55,  39),   S(-72,  75),   S(-44,  61),
            S(-81,  75),   S( -5,  19),   S(-21,  34),   S(-27,  37),   S(-46,  48),   S(-70,  84),   S(-47,  68),   S(-85,  79),
            S(-36,  50),   S(-19,  11),   S( 37,  17),   S( 24,  42),   S(-22,  43),   S(-17,  72),   S(-33,  86),   S(-41,  98),
            S( 63,  25),   S( 47,  21),   S( 50,  23),   S( 25,  47),   S( 15,  30),   S(-10,  73),   S( 11,  60),   S( 64,  59),
            S( 56,  72),   S( 59,  23),   S( 48,  69),   S( 21,  61),   S( 53,  40),   S( 22,  54),   S( 40,  83),   S( 70,  78),
            S( 56,   3),   S( 38,  -7),   S( 18, -15),   S(-29,   3),   S( 42,  12),   S( 48,  27),   S( 36,  43),   S(105,  23),

            /* rooks: bucket 2 */
            S(-67,  88),   S(-50,  68),   S(-43,  74),   S(-39,  50),   S(-30,  59),   S(-45,  51),   S(-35,  33),   S(-78,  75),
            S(-62,  72),   S(-61,  75),   S(-56,  77),   S(-59,  73),   S(-66,  68),   S(-63,  46),   S(-28,  25),   S(-46,  60),
            S(-70,  87),   S(-50,  78),   S(-59,  64),   S(-34,  59),   S(-53,  63),   S(-44,  55),   S(-30,  33),   S(-20,  45),
            S(-55,  90),   S(-41,  87),   S(-75,  99),   S(-74,  78),   S(-59,  74),   S(-37,  57),   S(-28,  44),   S(-20,  43),
            S(-35,  96),   S(-56, 107),   S(-24,  90),   S(-18,  59),   S(-38,  78),   S(-10,  71),   S(-37,  75),   S(  2,  69),
            S(  2, 106),   S( 18,  91),   S( 42,  72),   S( -6,  75),   S( 89,  35),   S( 56,  59),   S(114,  28),   S( 60,  69),
            S( 82,  83),   S( 21,  99),   S( 50,  72),   S( 63,  49),   S(  8,  43),   S( 56,  70),   S(-32,  99),   S( 19, 101),
            S( 50,  62),   S( 68,  62),   S( 56,  45),   S( 18,  48),   S(-30,  48),   S( 23,  30),   S( 34,  45),   S( 28,  62),

            /* rooks: bucket 3 */
            S(-19, 107),   S(-14, 107),   S(-14, 135),   S( -8, 124),   S(  0,  92),   S(  4,  84),   S( 16,  64),   S(-12,  41),
            S( -6,  98),   S(-11, 112),   S(-20, 137),   S( -8, 130),   S( -4,  93),   S(  6,  54),   S( 42,  32),   S( 12,  60),
            S(  1,  95),   S(-13, 114),   S(-19, 125),   S( -3, 129),   S( -7,  94),   S(  3,  79),   S( 25,  72),   S( 19,  56),
            S(-16, 133),   S( -7, 144),   S(-24, 148),   S(-14, 139),   S(-18, 112),   S(  4,  92),   S( 25,  84),   S(-12,  76),
            S(  9, 129),   S(-26, 159),   S( -4, 154),   S(  7, 138),   S( -3, 117),   S(  6, 118),   S( 26, 102),   S( 15,  97),
            S(-11, 155),   S( 15, 145),   S( 20, 150),   S( 44, 132),   S( 97,  89),   S(103,  94),   S( 88, 100),   S( 29,  96),
            S( 12, 158),   S( -8, 165),   S( 19, 167),   S( 16, 155),   S( 17, 145),   S(111,  85),   S(112, 144),   S(199, 104),
            S(110,  36),   S( 42, 104),   S( 46, 138),   S( 59, 119),   S( 29, 117),   S( 45, 110),   S( 66,  94),   S(105,  59),

            /* rooks: bucket 4 */
            S(-39,  -6),   S( 20,  -3),   S( 21, -25),   S(-48,  15),   S(-36,   8),   S( -6,  26),   S(-16, -11),   S(-94,  68),
            S(-30, -44),   S(-54,  15),   S( 18, -32),   S(  3, -33),   S( 10,   0),   S( -3,   2),   S(-22,   7),   S(-13,  29),
            S(-21, -25),   S(-47, -16),   S(-11, -19),   S(-34, -10),   S(-49,   4),   S(-75,  26),   S(-14, -11),   S(-50,   3),
            S(-34, -33),   S( 28,  -7),   S(  6, -30),   S( 30, -24),   S( 30,   3),   S(-30,   8),   S( 12,  -8),   S(-29,   7),
            S(-19, -35),   S( 38, -16),   S( 58,   4),   S( 55, -15),   S( 47,   1),   S( 67,  12),   S( 20,   0),   S(-12,  23),
            S( -6, -31),   S(  9,   5),   S( 30,  -3),   S( 25,  11),   S( 25,   8),   S(-12,  26),   S( 37,   7),   S( 13,  16),
            S(-12, -49),   S( 31,  22),   S( 30, -37),   S( 62, -18),   S( 47, -15),   S( -9,   3),   S( -9, -17),   S( 25,   8),
            S( 20, -27),   S( 16,   1),   S( 27, -28),   S( 15, -14),   S( 44,   9),   S( 35,  -9),   S( -4,  -3),   S( 13,  -7),

            /* rooks: bucket 5 */
            S(-58,  39),   S(-34,  11),   S(-11,   6),   S( 31,  -2),   S(-18,  22),   S(  4,  17),   S(-19,  34),   S(-66,  46),
            S(-27,   4),   S(-40,  -7),   S(  0, -21),   S(-11,   5),   S(-14, -10),   S(-25,  11),   S(-40,  18),   S(-22,  28),
            S(-57,  13),   S(-31,  -9),   S( 10, -26),   S(-34,   6),   S(-41,  -2),   S( 18,  -9),   S(-52,  20),   S(-10,  17),
            S(-30,  14),   S(-18,   8),   S( 61, -28),   S( 11,  22),   S( -5,   8),   S(-21,  24),   S( -8,  13),   S(-13,  36),
            S( 32,   5),   S( 33,  14),   S( 28,  21),   S( 47,  -5),   S( 24,   6),   S( 82,  -5),   S( 11,   9),   S( 64,  20),
            S( 33,   5),   S( -7,   2),   S( -1,  25),   S(  1,  11),   S( 21,  19),   S( -2,  36),   S( 82,   7),   S( 65,  34),
            S( 43,  -5),   S( 49, -14),   S(  8,  12),   S( 36,  22),   S( 40,  -7),   S( 61, -17),   S( 71, -17),   S( 58,   3),
            S( 15,  21),   S( 33, -10),   S( 69, -17),   S( 43,  18),   S( 15,  19),   S( 56,  19),   S(  1,  34),   S( 34,  33),

            /* rooks: bucket 6 */
            S(-68,  53),   S(-22,  14),   S(-33,  26),   S( -4,   5),   S(-16,  17),   S( -3,   5),   S(-10,   3),   S(-46,  18),
            S(-68,  29),   S( -7,  13),   S(-33,  21),   S(-14,   7),   S( 16,  -7),   S(-43,   8),   S(-58,  21),   S(-48,  12),
            S(-87,  44),   S(-19,  12),   S(-18,   8),   S(-25,   8),   S(-19,  10),   S(  1,   8),   S(-18,  -6),   S(-64,   3),
            S(-56,  45),   S(-17,  31),   S( 32,  -2),   S( 12,  17),   S( 57,  -1),   S( 13,  14),   S(-27,  12),   S(  1,  25),
            S(-62,  64),   S( 65,  12),   S(105,   8),   S( 71,   4),   S( 41,   7),   S( 46,  35),   S( 55,  -6),   S( 73,  14),
            S(122,   0),   S(101,   3),   S(116,  -6),   S( 52,  -6),   S( -9,  -9),   S( 26,  49),   S( 40,   9),   S(104,   0),
            S( 33,  17),   S(111, -14),   S( 79, -14),   S(109, -40),   S( 43,   2),   S( 41,  17),   S( 55, -24),   S( 60, -13),
            S( 64,   1),   S(-12,  38),   S( -6,  26),   S( 95,  -7),   S( 47,  10),   S( 44,  31),   S( 71,   7),   S( 47,  13),

            /* rooks: bucket 7 */
            S(-99,  41),   S(-84,  37),   S(-70,  51),   S(-68,  39),   S(-53,  18),   S(-28, -13),   S(-36,  17),   S(-67,   5),
            S(-81,  27),   S(-45,  18),   S(-73,  37),   S(-85,  53),   S(-56,   2),   S(-28,   2),   S( 13,  15),   S(  2, -37),
            S(-65,  20),   S(-73,  27),   S(-33,  11),   S(-48,  33),   S(-65,  20),   S(-37,  24),   S( 25, -13),   S(  1, -29),
            S(-83,  36),   S(-26,  18),   S( -6,   7),   S( 79, -22),   S( 14, -15),   S( 47,  -8),   S( 12,  18),   S(  4,  -4),
            S( 22,  14),   S( 37,  16),   S( 31,  20),   S( 68,   1),   S(112, -33),   S(128, -31),   S(124, -12),   S(-29, -11),
            S( 31,  20),   S( 35,  20),   S(116, -10),   S(109,  -9),   S(115, -18),   S( 62,  19),   S( 26,  31),   S(  9,  -2),
            S(-37,  23),   S( 29,  -5),   S( 83, -11),   S( 93, -20),   S( 95, -24),   S(101, -24),   S( 39,  18),   S( 51, -23),
            S(-59,  10),   S(-21,  21),   S( 25,  18),   S( 23,  22),   S( 47,   2),   S( 56,   0),   S( 19,  24),   S( 27,  -2),

            /* rooks: bucket 8 */
            S(  5, -58),   S( 16, -50),   S( 38, -34),   S( 55,  -2),   S( -5, -80),   S( -6, -22),   S( -5, -67),   S(  6, -29),
            S( -1, -59),   S( -2, -41),   S( 25, -14),   S(-20, -64),   S( -2, -51),   S(-14, -48),   S(  3, -17),   S(-39, -51),
            S(  8,   3),   S( -7, -12),   S( 12, -13),   S( -1,   8),   S(  6,   8),   S( 14,  12),   S( 11,  51),   S( -9, -39),
            S( -7, -25),   S( -2, -11),   S(  9,  -5),   S( 33,   9),   S( 17,  22),   S( 22,  -3),   S(  4,  12),   S(-14, -34),
            S(  1, -42),   S( 21,  21),   S( 19, -14),   S( 30,   9),   S( -1,  -8),   S(-14,  -1),   S( -2,   3),   S(-15, -28),
            S(-13, -42),   S( 11, -37),   S( 13, -21),   S( 13,  -8),   S(  0,   8),   S( -9,  18),   S(-13,  -7),   S(  5,   6),
            S( 22,  14),   S( 36,  15),   S(  6, -29),   S( 19,   2),   S(  5,  -4),   S( 24,  17),   S( 15,  28),   S( 11,  22),
            S( -2,  -3),   S( 24, -22),   S( 20, -16),   S( 38,  45),   S( -3,  28),   S(  1, -22),   S( 20,  19),   S(  8,  -6),

            /* rooks: bucket 9 */
            S(-11, -78),   S( 17, -95),   S( 11, -101),  S( 20, -79),   S( 15, -65),   S(-11, -64),   S( -7, -29),   S( 26, -48),
            S(-39, -72),   S(-12, -64),   S(-11, -83),   S(-22, -79),   S(-27, -53),   S(-19, -15),   S(-28, -72),   S(-14, -61),
            S(-17, -34),   S( -9, -47),   S( 17,  25),   S( -2, -30),   S( -5, -49),   S(  8, -21),   S(-13,   5),   S(-12,   8),
            S( 12, -50),   S(  7, -30),   S( -2,  -5),   S(  2,  -9),   S(-13, -77),   S( 16, -25),   S(-12, -45),   S(  1, -19),
            S( 21, -53),   S(  2, -37),   S( -3, -50),   S( -2, -21),   S(-19, -72),   S(-24, -35),   S( -4, -55),   S(  1, -56),
            S( -3, -43),   S(-41, -61),   S( -8, -53),   S( 30,  -1),   S( 33, -30),   S( 13,  -7),   S(-26, -29),   S( -1, -24),
            S(  1, -11),   S(  7, -26),   S(  2, -50),   S(-19, -19),   S( 20, -34),   S( 13, -22),   S(  5, -23),   S(-19, -57),
            S(-21, -18),   S(  0, -26),   S(  9, -16),   S( 11,  10),   S(  8, -32),   S(  5,   7),   S(-20, -19),   S(  4,  -6),

            /* rooks: bucket 10 */
            S( 15, -91),   S(-19, -60),   S(-25, -75),   S(  6, -87),   S( 22, -88),   S(  4, -92),   S(  9, -96),   S(-18, -90),
            S(-30, -61),   S(-29, -63),   S(-11, -70),   S(-22, -72),   S(-24, -69),   S( 13, -56),   S( -5, -60),   S(-48, -93),
            S(  1, -53),   S(-24, -48),   S(-34, -50),   S(-48, -56),   S( -2, -25),   S( -2, -33),   S( 16, -51),   S(-16, -29),
            S(-13, -19),   S(-39, -68),   S( -1, -35),   S( -3, -16),   S( -6, -35),   S(  1,  -4),   S(-10, -91),   S(  1, -62),
            S(  9, -41),   S(-13, -39),   S( -1, -53),   S(  1, -69),   S( 17, -14),   S(  7, -24),   S( 25, -71),   S( -6, -82),
            S(-19, -43),   S( -1, -23),   S( -6, -57),   S(  3, -70),   S( 19, -40),   S( 14, -51),   S(-33, -63),   S(-13, -59),
            S(-23, -47),   S(  2, -32),   S(  1, -49),   S( -2, -55),   S( 14, -35),   S(  1, -43),   S( 12, -83),   S(  0, -72),
            S(-23, -46),   S( -1, -10),   S(  3,  12),   S(  8, -33),   S(-10,   6),   S(-23, -21),   S( -4, -66),   S(-19, -21),

            /* rooks: bucket 11 */
            S(  0, -66),   S(-24, -43),   S(  1, -51),   S( 13, -41),   S( -5, -54),   S( 41, -67),   S(  6, -38),   S(-25, -68),
            S(  1, -29),   S(-25, -24),   S(-35, -40),   S(-53, -25),   S(-22, -33),   S(  4, -18),   S(-20, -62),   S(-31, -61),
            S(-34,   3),   S(-39,  -2),   S(  2,  -3),   S(-19,  -9),   S(  2, -18),   S( -6, -29),   S( 12, -31),   S(-16, -13),
            S(-11, -36),   S( -2, -54),   S( -4,  -3),   S( 22, -24),   S( 27,  -2),   S( -3, -34),   S(  5,  15),   S( -8, -40),
            S(-11, -52),   S(  4, -31),   S(  0, -25),   S( 12, -11),   S( 42, -27),   S( 10, -39),   S( 30,  22),   S(-25, -64),
            S( -4, -47),   S( -9, -27),   S( 11, -31),   S(  7, -37),   S(-31, -29),   S( 29, -27),   S( 35,  -8),   S( -6, -47),
            S(-15,  -2),   S(-21, -22),   S(  0, -27),   S(  4, -13),   S( 24, -19),   S( 37, -10),   S( 31, -23),   S(  4, -45),
            S(  0, -19),   S( 31,   4),   S( -9,   5),   S( 14,  -4),   S(-16,   1),   S( 26, -17),   S( 50, -22),   S( -8,  -3),

            /* rooks: bucket 12 */
            S(-15, -92),   S( -4, -19),   S( 10, -14),   S(-11, -38),   S(  7, -20),   S( 15,  15),   S(-24, -76),   S(-21, -58),
            S( 13,  20),   S( -3, -20),   S(  6,   6),   S(  6, -23),   S(  3, -43),   S(  8, -22),   S(  5, -16),   S(-13, -53),
            S( -4, -10),   S(  7,  24),   S( 22,  26),   S( 24,  12),   S(  7, -22),   S(  6, -17),   S(  8,  29),   S(  7,  25),
            S( -4,  -2),   S(  6,  16),   S(  8,  -1),   S(  3,   0),   S(  5,  -1),   S( 12,  21),   S( 12,  22),   S( -4, -11),
            S( 16,  14),   S(  6,   1),   S(  9,  -3),   S( -6, -41),   S(  7,   4),   S( -7, -20),   S(  2, -10),   S( -1, -22),
            S( -4, -33),   S(  0, -23),   S( -2, -51),   S( -8, -40),   S( 15,  30),   S( -8, -35),   S(  6,   1),   S(-13, -38),
            S( -5, -13),   S( -7, -24),   S(  9, -20),   S(  4,   5),   S(  2, -14),   S( 11,   3),   S(  7,  24),   S(  2,   9),
            S( -5, -26),   S( -1,   6),   S(  3, -34),   S( -6, -44),   S(  1, -19),   S(  0,   2),   S( -8, -18),   S(  0,  19),

            /* rooks: bucket 13 */
            S(-14, -52),   S(-10, -64),   S(-29, -77),   S(  1, -30),   S(-26, -87),   S(  8, -21),   S(-27, -54),   S(-19, -44),
            S(-11, -54),   S(-18, -72),   S(  1,   1),   S( -2, -30),   S( 12,  -9),   S(  4, -31),   S(  8, -42),   S( -5, -35),
            S(  4, -36),   S( -5, -45),   S( -3,  -9),   S(  1, -34),   S( 11, -24),   S(  8, -29),   S( 15, -19),   S(-20, -109),
            S( -1, -20),   S( -7, -47),   S( 16,  25),   S(  1, -28),   S( 11, -22),   S(-15, -29),   S( -2, -15),   S( -1,  -3),
            S(  4, -28),   S( -4, -71),   S( -7, -74),   S( -4, -58),   S( 10, -36),   S( -1, -29),   S(  3,  11),   S( -1,  -6),
            S(  5,   1),   S(-17, -58),   S(  0, -53),   S(-20, -81),   S( -7, -82),   S( -5, -54),   S( -6, -33),   S( -5, -32),
            S( -4, -27),   S( -9, -53),   S( -8, -42),   S( 12, -11),   S( -8, -64),   S(  4, -25),   S( -1, -26),   S( -6, -40),
            S( -6, -26),   S( -9, -42),   S(  4, -35),   S( 18, -19),   S( -6, -56),   S( -7, -40),   S( -2,  -8),   S(  0,  -2),

            /* rooks: bucket 14 */
            S(  3, -37),   S(-37, -59),   S(  0, -36),   S(-20, -93),   S( -7, -52),   S(  5, -50),   S(-32, -103),  S(-32, -102),
            S( 15,  -2),   S(  0, -40),   S( 10, -20),   S(-13, -53),   S( -1, -19),   S(  4,   0),   S(  2, -17),   S(  2, -67),
            S( -2, -11),   S(  6,   1),   S(-11, -50),   S(  1, -44),   S(  6, -16),   S( -4, -17),   S(  0, -64),   S(-24, -100),
            S(-10, -21),   S(  6,  32),   S( -6, -15),   S( -8, -47),   S(-19, -64),   S( -7, -30),   S( 10, -11),   S(-23, -63),
            S(  5,  -5),   S(  8,  27),   S( -1,  -8),   S( -2, -60),   S(  7, -27),   S( 10, -24),   S( -3, -57),   S(  0, -37),
            S( -1,  -5),   S(  3,   9),   S(  6, -19),   S(  1, -82),   S( -3, -64),   S( -6, -78),   S(  2, -67),   S(-19, -55),
            S(-14, -43),   S( -7, -13),   S(-14, -48),   S(-21, -110),  S( -1, -13),   S(  4, -10),   S(-22, -74),   S(  2, -35),
            S( -5, -58),   S( -6, -28),   S( -7, -44),   S( -7, -66),   S(-18, -70),   S(-15, -88),   S( -9, -60),   S( -2, -19),

            /* rooks: bucket 15 */
            S(-26, -78),   S( -3, -25),   S(-23, -57),   S(-20, -62),   S( -4, -40),   S(-13, -59),   S( -1, -19),   S( -9, -54),
            S( 18,  15),   S(-14, -63),   S( -8, -52),   S(-13, -47),   S( -7, -45),   S(  3,  -3),   S(  6,   0),   S(  3,  -5),
            S( -4, -24),   S( -6, -29),   S(  0,   4),   S( -1, -39),   S( -6, -56),   S(-13, -44),   S(  9,  14),   S(  5,   9),
            S( -3,   0),   S(  1,  -5),   S(  8,   1),   S( -6,   3),   S( 24,  42),   S(  5,  -9),   S(  6, -19),   S( -2, -36),
            S(  8,   8),   S(  7,   4),   S(  4,   6),   S( -2, -15),   S(  7,   5),   S(  3, -42),   S(  5, -31),   S(  1, -46),
            S(  0,   7),   S(  7,  23),   S(  4,   3),   S(-10, -34),   S( -5, -27),   S( 11, -42),   S( 11, -21),   S( -1, -13),
            S( -3, -26),   S(  2,   9),   S(  2,   3),   S(  5,   6),   S(  6,  32),   S(  1, -37),   S(-10, -35),   S(-11, -55),
            S(  0, -33),   S( -1, -15),   S(  4,  15),   S( -2, -15),   S(  4,  13),   S( -7, -54),   S(  6, -29),   S( -8, -44),

            /* queens: bucket 0 */
            S(-26, -16),   S(-32, -70),   S(-30, -83),   S( -1, -105),  S( -2, -75),   S(  0, -67),   S(-75, -53),   S( -5, -41),
            S(-27, -43),   S( 10, -75),   S( 12, -62),   S(-23, -24),   S(  3, -51),   S( -5, -63),   S(-28, -52),   S(-26, -19),
            S(  4, -19),   S(  4, -59),   S( 13, -47),   S( -8, -25),   S(-15,  24),   S(-14,   7),   S(-13, -41),   S(-87, -85),
            S(-30,  59),   S(  6,  -8),   S(-21,  42),   S(-17,  61),   S(-24,  81),   S(-21,   3),   S(-53,  29),   S(-16, -39),
            S(-52,  33),   S(-36,  96),   S(-14,  65),   S(-23,  75),   S( -7,  91),   S(-19,  70),   S(-45,  48),   S(-42, -22),
            S(-31,  52),   S( 10,  73),   S( 40,  85),   S(  9,  85),   S(-69, 100),   S(-42,  48),   S(-94,   7),   S(-43,  16),
            S(  0,   0),   S(  0,   0),   S( 45,  38),   S(-26,  29),   S(-44,  56),   S(-88,  83),   S(-92,  49),   S(-105, -11),
            S(  0,   0),   S(  0,   0),   S( 20,  27),   S( -6,  -5),   S(-52,  31),   S(-39,  39),   S(-65,  13),   S(-60,  13),

            /* queens: bucket 1 */
            S( 26, -39),   S(  0, -58),   S( 16, -132),  S( 26, -108),  S( 35, -95),   S(  4, -82),   S(-10, -25),   S(-16, -12),
            S(-35,   9),   S( 25, -43),   S( 27, -61),   S( 12,   1),   S( 33, -31),   S( -3, -29),   S(-42,  39),   S(-36, -32),
            S( 34,  -4),   S(  9,  -9),   S(  7,  12),   S(  9,  55),   S(-23,  73),   S( 15,  18),   S(-16,  37),   S( 17, -38),
            S( 31, -14),   S( 18,  39),   S( -7,  36),   S( 19,  74),   S( -3,  76),   S(-23,  59),   S(  8,  53),   S(-33,  66),
            S( 35,  14),   S( 24,  58),   S( 30,  96),   S( 10,  97),   S( 18, 108),   S( 59,  22),   S(-46, 106),   S(  7,  37),
            S( 54,   0),   S( 94,  32),   S( 93,  66),   S( 98,  76),   S( 64,  81),   S(  1, 111),   S( 20,  54),   S(  8,  37),
            S( 78,   9),   S( 55,  37),   S(  0,   0),   S(  0,   0),   S( 25,  80),   S(-21,  68),   S(-12,  87),   S(-45,  38),
            S( 90,  -8),   S( 62,  18),   S(  0,   0),   S(  0,   0),   S( 21,  20),   S( 17,  51),   S( 67,  21),   S(-55,  64),

            /* queens: bucket 2 */
            S( 32, -42),   S( 19, -35),   S( 22,  26),   S( 44, -27),   S( 57, -97),   S( 47, -67),   S( -9, -58),   S( 16,  16),
            S( 25,  -3),   S(  5,  20),   S( 42, -11),   S( 42,   4),   S( 49, -13),   S(  9,  11),   S( 23, -17),   S( 33,  15),
            S( 17,  38),   S( 26,  25),   S( 13,  72),   S( 15,  53),   S( 29,  44),   S( 22,  49),   S( 32,  36),   S( 12,  63),
            S( 32,  14),   S( 22,  89),   S(  9, 114),   S(  6, 111),   S( 25,  95),   S(  0,  97),   S( 20,  76),   S( 26,  86),
            S( -4,  72),   S(  9,  42),   S(-23, 172),   S( 23, 124),   S( 28, 141),   S( 57,  88),   S( 50,  95),   S( 57,  80),
            S(-48, 105),   S(-24,  96),   S(  4, 112),   S( 87, 101),   S( 58, 108),   S( 86, 127),   S( 99,  90),   S( 38, 127),
            S( -8,  77),   S(-10,  88),   S(-19, 113),   S( 99,  56),   S(  0,   0),   S(  0,   0),   S( 11, 142),   S( 35, 118),
            S( -9,  69),   S( 24,  44),   S( 26,  54),   S( 45,  91),   S(  0,   0),   S(  0,   0),   S( 73,  68),   S( 26, 101),

            /* queens: bucket 3 */
            S(-36,  30),   S(-27,  37),   S( -9,  32),   S(  0,  39),   S(-13,  17),   S( -7,  -8),   S( -9, -51),   S(-28,   5),
            S(-46,  57),   S(-28,  52),   S(-12,  54),   S( -5,  61),   S( -8,  68),   S( -4,  26),   S( 29, -15),   S( 47, -29),
            S(-44,  72),   S(-27,  88),   S(-27, 113),   S(-32, 139),   S(-20, 104),   S(-19,  98),   S( -3,  60),   S(-12,  49),
            S(-30,  63),   S(-50, 135),   S(-45, 163),   S(-28, 175),   S(-34, 161),   S(-31, 116),   S( -3, 103),   S(-11,  83),
            S(-35, 113),   S(-47, 131),   S(-27, 144),   S(-19, 177),   S(-22, 183),   S(-22, 191),   S(-23, 166),   S(-31, 143),
            S(-65, 101),   S(-46, 145),   S(-64, 191),   S(-68, 210),   S(-46, 215),   S(  3, 179),   S(-24, 190),   S(-13, 188),
            S(-94, 158),   S(-89, 155),   S(-77, 193),   S(-80, 203),   S(-74, 210),   S(  3, 152),   S(  0,   0),   S(  0,   0),
            S(-138, 192),  S(-103, 159),  S(-59, 129),   S(-72, 172),   S(-50, 155),   S( -3, 122),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-41,   7),   S(-61, -69),   S( -6,  -1),   S(-20, -26),   S( -1, -14),   S(-13, -20),   S(-25, -25),   S(  3,  11),
            S(-26, -10),   S(  8,  54),   S(-16, -17),   S( -4, -29),   S(  0,   4),   S(-26,  12),   S(-61, -31),   S(-18, -35),
            S( 34,  53),   S( 31,  -5),   S(-18,   4),   S( -1,  14),   S( 19,  39),   S(  8,  -1),   S(-40, -18),   S( 38,  25),
            S( -5,   1),   S( 29,   5),   S( -2,  15),   S( -1,  33),   S( 52,  33),   S(-13,  18),   S(-69, -51),   S(-15, -11),
            S(  0,   0),   S(  0,   0),   S( 48,  23),   S( 94,  55),   S( -5,  22),   S( 12,  24),   S( 12,  -3),   S( -7,  -8),
            S(  0,   0),   S(  0,   0),   S( 10,  12),   S( 69,  52),   S( 24,  27),   S( 35,  43),   S( -8,   0),   S( -6,   1),
            S( 11,   3),   S( 46,  50),   S( 84,  44),   S( 76,  71),   S( 41,  39),   S(  1,  21),   S( -4,  11),   S(-44,  -4),
            S( 50,  17),   S(  5,   2),   S( 42,  42),   S( 40,  24),   S( -5, -17),   S(-35, -23),   S( -7, -20),   S(  9,  -7),

            /* queens: bucket 5 */
            S( 21,   3),   S( 45,   5),   S( 17,  25),   S( -7,  -5),   S( -9, -43),   S(  6,  10),   S( 13,   4),   S( -3,  -9),
            S(  2,  -4),   S(-12,   5),   S( -7, -26),   S(  6,  19),   S(-29,  22),   S(-53, -13),   S( 13,  -8),   S( 14,  20),
            S( 24,  13),   S( 34,  28),   S( 41,  25),   S( 14,  26),   S(-15,  19),   S( 58,  27),   S( 56,  49),   S(-14,  13),
            S(-19, -38),   S( 67,  17),   S( 34,   6),   S( 21,  31),   S( 82,  41),   S( 46,  25),   S( 11,  55),   S(-22,  19),
            S( 24,   7),   S( 46,  13),   S(  0,   0),   S(  0,   0),   S( 27,  25),   S( 54,  32),   S( 43,  18),   S( -7,  15),
            S( 37,  25),   S( 52,  64),   S(  0,   0),   S(  0,   0),   S( 31,  22),   S( 73,  62),   S( 54,  16),   S( 38,  46),
            S( 78,  23),   S( 74,  20),   S( 60,  60),   S( 34,  51),   S( 68,  72),   S(116,  67),   S( 47,  46),   S( 17,  -6),
            S( 20,  30),   S( 51,  28),   S( 74,  38),   S( 57,  37),   S( 73,  39),   S( 46,  56),   S( 56,  55),   S( 20,  -3),

            /* queens: bucket 6 */
            S(  6,   5),   S( -4,  -6),   S( -7,  -7),   S( 17, -15),   S(-11, -16),   S(-23, -22),   S(-41, -36),   S(  0,  25),
            S(-13,   5),   S( 10,   9),   S( 25,  36),   S( 25,  25),   S( 15,  43),   S(  2, -20),   S(-19,  20),   S(-24,  12),
            S(-24,  45),   S( -7,  43),   S(-12,  33),   S(  8,  21),   S(-15,  15),   S( 19,  27),   S( 54,  24),   S( 98,  82),
            S(-16,  21),   S(-26,  34),   S( 37,  13),   S(105,  38),   S( 37,  -6),   S( 50,  16),   S(116,  55),   S( 97,  38),
            S( 22,  36),   S(  4,  26),   S( 60,  52),   S( 70,  68),   S(  0,   0),   S(  0,   0),   S( 61,  32),   S(123,  85),
            S( 36,  41),   S( 56,  26),   S( 52,  74),   S( 37,  50),   S(  0,   0),   S(  0,   0),   S( 90,  64),   S( 86,  40),
            S( 31,  15),   S( -3,  39),   S( 68,  36),   S( 60,  23),   S( 49,  63),   S( 78,  76),   S(120,  43),   S(168,  21),
            S( 54,  12),   S( 55,   2),   S( 66,  14),   S(107,  65),   S(139,  52),   S(120,  51),   S( 97,  42),   S( 89,  16),

            /* queens: bucket 7 */
            S(-12, -15),   S(-27, -35),   S(-32,  12),   S(-39,  17),   S( -7,   2),   S(-67,  11),   S(-18,   5),   S(-18, -29),
            S(-14, -22),   S(-83,  25),   S(-20,  69),   S( -5,  28),   S(-25,  20),   S(-23,  26),   S(-14,  25),   S(-29,   4),
            S(-37,   1),   S(-19,   1),   S( -4,  41),   S( 47,  18),   S( 25,  19),   S( 30,  -3),   S( 33,  -7),   S( 58, -14),
            S(-78,  15),   S( 19, -22),   S( 31,  35),   S( 22,  60),   S(110,  18),   S( 96,  14),   S( 75, -17),   S( 38,   2),
            S( -3, -30),   S(-18,   6),   S(-16,  80),   S(  0,  62),   S( 88,  40),   S( 77,  48),   S(  0,   0),   S(  0,   0),
            S(-16, -20),   S( -7,  38),   S(-13,  61),   S( 19,  41),   S( 51,  15),   S(105,  57),   S(  0,   0),   S(  0,   0),
            S(-23,  45),   S(-39,   8),   S(  9,  18),   S( 16,  46),   S( 52,  38),   S( 88,  48),   S( 55,  29),   S( 61,  54),
            S( 20, -15),   S(  5,   2),   S( 35,   1),   S( 52,  -1),   S( 74,  36),   S( 55,   7),   S( 11,   5),   S( 74,  -7),

            /* queens: bucket 8 */
            S(-12, -22),   S(  0, -19),   S(-19, -49),   S( -1,  -4),   S( -8, -23),   S( -2, -14),   S(  9,  -2),   S( -3, -11),
            S(-12, -22),   S( -4,  -8),   S( 19,  20),   S( 12,  17),   S(  9,  -9),   S(  3, -13),   S( -8,  -1),   S(  2,   1),
            S(  0,   0),   S(  0,   0),   S( 16,   4),   S( -9, -34),   S(  7,  -8),   S( -9,   8),   S(-18, -28),   S(  0,   2),
            S(  0,   0),   S(  0,   0),   S(  3,   7),   S( 22,  32),   S(  0,   0),   S( 10,  -3),   S(  2,  23),   S(  4,  -2),
            S( -2,  -5),   S(  8,  13),   S( -2,  -8),   S( 49,  42),   S( 13,  10),   S(  4,  -6),   S( 11,   4),   S(-16, -20),
            S(  6,  -8),   S( 11,  -5),   S(  7,   5),   S(  8,  -5),   S( 11,  22),   S( 27,  26),   S(  6,  10),   S(-15, -24),
            S(-15, -33),   S( -2, -10),   S( 26,  37),   S( 35,  51),   S( 10,  -2),   S(  0,  17),   S( -3, -24),   S( -7, -16),
            S( 12,  -3),   S(  3,   6),   S(  7,  -1),   S(  2,   6),   S(  7,   5),   S(-15, -26),   S( -6, -12),   S(-36, -52),

            /* queens: bucket 9 */
            S( 26,  15),   S(-15, -39),   S( -7, -24),   S( 23,   3),   S(  6,  -6),   S( 17,  17),   S(-13, -22),   S( -3, -20),
            S(  8,  -7),   S(  9,  -8),   S( -7, -24),   S( -2, -11),   S(-18, -50),   S( 11,   5),   S(  2,  -8),   S( -3, -14),
            S( -8, -35),   S( -4, -18),   S(  0,   0),   S(  0,   0),   S( 13,  20),   S( 18,   2),   S( -6, -17),   S( 12,  12),
            S( 22,   8),   S( -4, -13),   S(  0,   0),   S(  0,   0),   S( 24,  36),   S( 25,  26),   S( 27,  39),   S(-22, -15),
            S(  6, -17),   S( 18,  -1),   S(  2,  10),   S(-21, -32),   S( 10,   5),   S( 14,  12),   S(  8,  -8),   S(-14, -43),
            S( 25,  22),   S(  0, -14),   S( 18,  29),   S(  0, -13),   S( 10,   1),   S( -3,  -2),   S(  5,  -3),   S( -3, -19),
            S(  5,  -5),   S( 18, -19),   S( -6, -10),   S( -4,  -4),   S( 14,  -4),   S( 20,  13),   S(  0,   8),   S( 13, -12),
            S( 13, -15),   S(  7,  -7),   S(-16, -35),   S(  8,   6),   S( 10,  20),   S(  5,  -2),   S( -2, -27),   S(  6, -18),

            /* queens: bucket 10 */
            S( 11,  18),   S( 17,  10),   S( 10,  13),   S( -1, -17),   S(  8,  -8),   S( 19,  26),   S( -3, -18),   S(-11, -23),
            S(  5,  -6),   S(-11, -31),   S( 13,  -7),   S(-10, -16),   S(  6,   5),   S( 25,   8),   S(  0, -21),   S(-11, -31),
            S( -6, -11),   S(  0,  -6),   S( 15,   3),   S(  1, -11),   S(  0,   0),   S(  0,   0),   S(  7,   8),   S(  5,   3),
            S( -8, -13),   S(  6,  -6),   S( 10,   7),   S( 11,  19),   S(  0,   0),   S(  0,   0),   S(  2, -10),   S( -1, -22),
            S( 15,  24),   S( 22,  23),   S(  2, -28),   S( 32,  28),   S( 16,  23),   S(  2,  -1),   S( -2, -28),   S( 22,  -5),
            S( -4, -17),   S(  7,   3),   S( 33,  41),   S( 19,  17),   S(  9,  15),   S( 24,  23),   S( 32,  19),   S(  0, -25),
            S(  6,   3),   S( 15,  30),   S( 12,  15),   S( 17,  22),   S(  0, -24),   S( 25,  13),   S(  3, -22),   S( -6, -29),
            S(-17, -49),   S( -1, -11),   S( 16,  -5),   S( -5, -26),   S( 25,  31),   S(-12, -19),   S( 12,   4),   S(  1, -24),

            /* queens: bucket 11 */
            S( -7,  -8),   S( -9, -16),   S(-24, -42),   S(-20, -49),   S( -7,   6),   S(-14, -34),   S( -2, -17),   S( -8, -26),
            S( -2,  -4),   S( 17,  25),   S( -5, -13),   S( -4, -13),   S( 34,   4),   S( 18,  12),   S( 21,   7),   S(-11, -28),
            S( -4,  -3),   S( -7, -11),   S(-18,  -2),   S( -5, -18),   S( 15,   1),   S(-26, -40),   S(  0,   0),   S(  0,   0),
            S( -3, -11),   S( -2,   4),   S(-28, -17),   S( 14,  -3),   S( 15,  18),   S(  0,   8),   S(  0,   0),   S(  0,   0),
            S( -3,  -3),   S(  1,   0),   S( 15,  31),   S( 12,  29),   S( 29,  47),   S( 27,  23),   S( 16,   3),   S(-10, -26),
            S(-16, -21),   S( -2,  -2),   S(-14, -28),   S( -8, -18),   S(  2,   0),   S( 19,  11),   S( 30,  32),   S( 14, -21),
            S( -8,  -7),   S(  2,   9),   S( 16,  21),   S( -8,  -1),   S(  9,  33),   S(-22, -36),   S(  1,   8),   S( 22,   9),
            S(-40, -96),   S(  6, -21),   S(-33, -49),   S(  5,   8),   S(  7,   4),   S( 16,  -9),   S(  2,   4),   S(  5, -19),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 10,  17),   S( -7, -17),   S( -1,  16),   S(  0,  -2),   S(  3,   0),   S( -5,  -6),
            S(  0,   0),   S(  0,   0),   S(  9,  17),   S(-15, -38),   S(  9,   5),   S( -2, -16),   S( -4, -10),   S( -3, -13),
            S( -5, -12),   S(  3,   2),   S( -6, -17),   S( -2, -31),   S( 32,  62),   S(  0,  16),   S(-11, -23),   S(  8,  13),
            S( 15,   9),   S( 13,   5),   S(  7,  13),   S(  1,   8),   S(  3,   2),   S( -6,  -4),   S(  2,   5),   S( -5,  -8),
            S(-25, -51),   S(  1,  -5),   S(  1, -13),   S(  1, -17),   S( -4,  -7),   S(-21, -57),   S( -8, -21),   S( -5, -12),
            S( 11,  13),   S( -1,  -6),   S(  1,   3),   S(-17, -29),   S(-10, -29),   S(-25, -26),   S( -5,   6),   S( -3, -10),
            S(-19, -37),   S( -6,  -5),   S( -8, -22),   S(  6,   8),   S( -5,  -7),   S(-17, -34),   S(  4,  12),   S( -9, -30),
            S(  9,  21),   S( -8, -18),   S( -2,  -7),   S( -6, -11),   S(-10, -28),   S(-23, -54),   S(  5,  12),   S(-20, -33),

            /* queens: bucket 13 */
            S( -6, -25),   S( -5, -10),   S(  0,   0),   S(  0,   0),   S(-13, -18),   S( -4, -14),   S(  7,   2),   S( -6, -14),
            S( -6, -27),   S(  8,  11),   S(  0,   0),   S(  0,   0),   S(-14, -26),   S(-19, -41),   S( -7,  -9),   S(-10, -28),
            S( -4, -24),   S(  6,   1),   S(  8,  12),   S( 11,  14),   S(-10, -30),   S(  6,  11),   S( -9, -11),   S( -4,  -5),
            S(-15, -29),   S( -8, -29),   S(  6,   0),   S( 10,   2),   S( 19,  33),   S( 23,  40),   S( -7, -20),   S(-12, -28),
            S( 15,  11),   S( -7, -21),   S(  6,   6),   S( 18,  22),   S( -5,  -6),   S( -6, -20),   S(-25, -54),   S(  0,  -2),
            S(  5,  -3),   S( -9, -32),   S( -3, -12),   S( 11,  19),   S(  3,  -9),   S( -9, -17),   S(-18, -40),   S(-21, -46),
            S( -1,  -6),   S( -4,  -9),   S(  7,  10),   S( -5, -11),   S(  4,   8),   S(  8,  -5),   S(-19, -41),   S( -7, -24),
            S( -9, -28),   S( -5, -18),   S( -5, -15),   S(  4,  11),   S(  1,   6),   S(  4,  -3),   S(  6,   7),   S( -8, -28),

            /* queens: bucket 14 */
            S(  1, -15),   S( 17,  10),   S(  7,   5),   S( -3, -14),   S(  0,   0),   S(  0,   0),   S(  7,  -4),   S(-12, -25),
            S(-16, -35),   S(-18, -45),   S( -7, -22),   S( -2, -14),   S(  0,   0),   S(  0,   0),   S(  8,  15),   S(-11, -29),
            S( -5, -10),   S( -5, -27),   S( -8, -17),   S(-12, -31),   S(  7,  18),   S(  7,  11),   S( -9, -22),   S(-23, -60),
            S(-11, -25),   S( -6, -13),   S(  2,  -5),   S(-18, -33),   S(-11, -21),   S(-13, -35),   S( -6, -33),   S(  0, -11),
            S( -8, -15),   S( -2, -11),   S(-17, -40),   S( 18,  34),   S( 13,  18),   S( -8, -14),   S(  4, -13),   S( -4, -19),
            S( -8, -19),   S( -7, -29),   S(-11, -19),   S(-23, -46),   S(  5,   9),   S(  2,   0),   S(-18, -34),   S(-21, -35),
            S(-14, -27),   S(  0,  -7),   S(  2,   3),   S(  2,   9),   S( -9, -14),   S( 13,  16),   S( -9, -25),   S(-16, -31),
            S(-20, -41),   S( 12,  -3),   S( -5,  -3),   S(-16, -30),   S(  4,   5),   S( -1,   1),   S( -8, -21),   S( -7, -19),

            /* queens: bucket 15 */
            S( -8, -28),   S(  0,  -2),   S(  8,   0),   S( -6, -28),   S( 17,  28),   S(-13, -30),   S(  0,   0),   S(  0,   0),
            S( -2, -10),   S( -5, -21),   S(-15, -37),   S( -4, -19),   S(  4,  11),   S(  0,   3),   S(  0,   0),   S(  0,   0),
            S(  1,  -2),   S(  8,  11),   S(-15, -18),   S( -1,  -3),   S( -6, -18),   S( -1, -13),   S( -3,  -9),   S(  0,  -1),
            S( -6, -11),   S(-13, -27),   S( -3,  -6),   S( 11,  25),   S( -4, -15),   S(  9,  19),   S( -9, -15),   S( -7, -22),
            S( -2,  -4),   S( -2,  -6),   S(-12, -30),   S( -6, -11),   S( 14,  36),   S(  5,  10),   S(  2,   0),   S( -8, -26),
            S(  3,   1),   S(-18, -35),   S(-13, -29),   S(-17, -29),   S(-18, -29),   S(-19, -47),   S(  5,   4),   S(-14, -31),
            S( -4,  -9),   S( -2,   0),   S( -5,  -8),   S( -5, -15),   S( -9, -12),   S( -2, -12),   S(  1,   1),   S(  2,   2),
            S(-16, -32),   S(-19, -45),   S( -4, -20),   S( -8,  -8),   S( -4, -10),   S(  1,  -5),   S(  2,   0),   S( -9, -16),

            /* kings: bucket 0 */
            S( 13, -25),   S( 17,  -2),   S(  7,   1),   S(-29,   5),   S(-24,   3),   S( 35, -42),   S(  8,   9),   S( 14, -47),
            S(-16,  30),   S(-15,  -1),   S(  2,   4),   S(-35,   4),   S(-42,  40),   S(-12,  13),   S(-13,  42),   S(  1,  17),
            S( 43,  -2),   S( 52, -20),   S(  1,   1),   S( 11, -11),   S(-37,  12),   S(  0,  -9),   S(-31,  30),   S(  8, -13),
            S( -8, -27),   S(-30, -14),   S(-20,  -7),   S(-58,  23),   S(-47,  26),   S(-70,  14),   S(-37,  14),   S(-47,  32),
            S(-49, -74),   S( 26, -19),   S( 37, -23),   S( -5,  -7),   S(-34,  -3),   S(-33,   9),   S(-10,   8),   S(  6,  -7),
            S( -6, -90),   S( 32, -59),   S( 34, -65),   S( -5, -24),   S( 21, -22),   S( 14, -32),   S( 12,   2),   S(  1, -16),
            S(  0,   0),   S(  0,   0),   S(  6, -45),   S( 21, -43),   S( 20, -16),   S( 13, -35),   S(  8,   5),   S(-24, -25),
            S(  0,   0),   S(  0,   0),   S( -4, -59),   S( 22, -21),   S(  6, -33),   S( 11,  -3),   S( 11,   7),   S(  4,  -4),

            /* kings: bucket 1 */
            S( 33, -32),   S( 33, -16),   S( 15, -15),   S( 19,  -3),   S(-11,   1),   S( 30, -12),   S( 11,  18),   S( 23, -19),
            S(-20,  14),   S(  7,  20),   S( 13, -12),   S(-48,  29),   S(-23,  15),   S( -9,  11),   S( -2,  22),   S( -2,  21),
            S(-18,  -9),   S( 18, -24),   S(-11,  -9),   S( 13, -16),   S(-41,   7),   S(  3, -16),   S(  1,   4),   S( 48,  -9),
            S( 11,   9),   S( 26, -20),   S( 28, -14),   S( -7,  14),   S(-24,  29),   S(-25,  10),   S( -7,  11),   S(-29,  20),
            S(-21, -26),   S(  0, -18),   S( 32, -32),   S( 39, -17),   S( 34,  -8),   S(-30, -13),   S( 14,   3),   S(-23,   5),
            S(  9, -17),   S( 31, -47),   S( 22, -23),   S( 34, -25),   S(  3, -17),   S( 26,  -7),   S( 12,  13),   S( -6,  14),
            S(  0, -41),   S(  7,   9),   S(  0,   0),   S(  0,   0),   S(-10,   7),   S( -2,  27),   S(  2,  54),   S(-12, -47),
            S(-18, -126),  S(-11,  -9),   S(  0,   0),   S(  0,   0),   S(  1, -28),   S(  8,  -9),   S( -5,  29),   S(  0, -38),

            /* kings: bucket 2 */
            S( 42, -60),   S(  4,   8),   S(  5, -16),   S( 20,  -4),   S(-11,   7),   S( 35, -20),   S(  5,  29),   S( 31, -20),
            S( 34, -15),   S(-21,  38),   S(  5,  -3),   S(-14,  10),   S(-22,  15),   S( -9,   2),   S( 19,   3),   S(-10,   9),
            S(-26, -13),   S(-11,  -4),   S( -1, -12),   S( -8, -13),   S(  5,  -4),   S( 16, -26),   S( 35, -20),   S( 14, -18),
            S(-22,  31),   S(-32,   9),   S(-18,   7),   S( -8,  17),   S( 15,   3),   S(-33,   0),   S( 14, -17),   S( 59, -11),
            S( -8, -17),   S( 19,  -6),   S( -4, -14),   S(  0,   1),   S( 33, -31),   S(-11, -28),   S( 54, -29),   S( 11, -39),
            S( 10,  26),   S( 10,  -9),   S(  7, -22),   S( 45, -33),   S( 68, -25),   S(  9,   4),   S( 64, -36),   S( 25, -29),
            S(-22, -24),   S( 14,  37),   S(-24, -12),   S( 20,  23),   S(  0,   0),   S(  0,   0),   S( 27,   4),   S(-13, -30),
            S(-16, -26),   S(-11, -29),   S(  1, -32),   S(  7,  -5),   S(  0,   0),   S(  0,   0),   S(  3,   5),   S(-16, -127),

            /* kings: bucket 3 */
            S( 16, -60),   S(  7,  -3),   S( 22, -38),   S( -3, -11),   S(-14, -19),   S( 36, -32),   S(  2,  18),   S(  9, -26),
            S( -4,  13),   S(-34,  35),   S(-16,  -3),   S(-42,  13),   S(-51,  23),   S( -1,  -8),   S( -6,  13),   S(-17,  12),
            S( 36, -33),   S( -5,  -4),   S( -1, -20),   S(-20, -14),   S(-12,   1),   S( 33, -33),   S( 44, -26),   S( 66, -35),
            S(-60,  18),   S(-113,  39),  S(-98,  25),   S(-48,  11),   S(-79,  16),   S(-42,  -6),   S(-76,   8),   S(-72,  -2),
            S(-33,  26),   S(-15, -27),   S(-62,  -9),   S(-58,   9),   S(-16, -24),   S( 16, -45),   S( -6, -47),   S( 10, -65),
            S(-26, -13),   S( 12, -13),   S( 21, -27),   S(-32, -20),   S( 26, -32),   S( 65, -62),   S(104, -56),   S( 39, -102),
            S(-37,  -1),   S( 44,   7),   S(  9, -39),   S( 12, -19),   S(  8, -23),   S( 40, -48),   S(  0,   0),   S(  0,   0),
            S(-13, -22),   S( 12, -23),   S(  8, -17),   S(-18,  -3),   S(  1, -71),   S( 12, -26),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-58, -13),   S( 25,  12),   S(-36,  32),   S( 26,   1),   S( 25, -22),   S(-15,  20),   S( 21,  17),   S( 52, -24),
            S(-51,  39),   S( 25,  25),   S(-25,  19),   S( -2,  -5),   S( 63, -22),   S( 16,   2),   S( 46, -14),   S( 25,   1),
            S( -5,  21),   S(-14,  -3),   S(  2,  -1),   S(-29,   7),   S(-29,  12),   S(-11, -12),   S(-49,   0),   S(-10,  -9),
            S( 10,  -9),   S( 10, -10),   S( 21,   7),   S(-23,  13),   S( 13,  -5),   S( -5,   8),   S( -4,  20),   S(  8,   5),
            S(  0,   0),   S(  0,   0),   S(  8, -21),   S(  3,  -1),   S(-10,  12),   S(-18,  -7),   S(-16,   5),   S(-19,   7),
            S(  0,   0),   S(  0,   0),   S( -3,  22),   S( 12,   7),   S(  7,  -4),   S(  6,  -5),   S(  4, -13),   S(  4,  -5),
            S( -4, -18),   S( -5,   2),   S(  0, -46),   S(  2,   7),   S( 28,  38),   S(-14, -12),   S( 14,   2),   S(  5,  -4),
            S( -2,  29),   S(  6,  29),   S( -4, -28),   S(  9,  34),   S(  2, -28),   S( -5, -14),   S( -7, -29),   S(  2,   7),

            /* kings: bucket 5 */
            S(  5, -10),   S( -9,   6),   S(-47,  21),   S(-26,  10),   S(-13,  13),   S(  0,   4),   S( 45,   6),   S( 44,  -3),
            S( 19,  -3),   S( 50,   1),   S( 14,   2),   S( 29,  -5),   S( 12,  -6),   S( 35,  -8),   S( 31,   7),   S( 48, -13),
            S( -8,   4),   S(-28,   8),   S(  7, -11),   S( -4, -12),   S( -8,  -1),   S(-79,   3),   S(-24,   4),   S(  3,  -4),
            S( -8,  -7),   S( 72, -30),   S( 23,  -9),   S(-25,  30),   S( 37,  15),   S( 14,   0),   S( 33,   0),   S( 16,  -2),
            S(-12, -15),   S(-13, -25),   S(  0,   0),   S(  0,   0),   S(-11,  -6),   S( -7, -12),   S( -6,   1),   S(-18,   6),
            S(-23,  -5),   S(-24, -16),   S(  0,   0),   S(  0,   0),   S( -3,  16),   S(-32,  13),   S(-28,  15),   S(-16,  -1),
            S(-13,  17),   S( -4,  13),   S( -3,  21),   S( -1,  -9),   S(-19,  15),   S( -4,  11),   S( 14,  22),   S( 12,  23),
            S(-10,  -9),   S(  0,  29),   S(  2,  44),   S(  8,  39),   S( -2,  21),   S( -2,  36),   S( -7, -25),   S( -4,   2),

            /* kings: bucket 6 */
            S( 41, -18),   S( 15,  -9),   S( 27, -22),   S(  3,  10),   S(-18,  12),   S(-19,  16),   S( 16,  24),   S( 33,  -3),
            S( 50, -25),   S( 47,   8),   S( 10,  -5),   S( 46, -14),   S( 22,   2),   S(  2,   4),   S( 28,   4),   S(  8,   6),
            S(  9,   1),   S(-23,   1),   S( -4,  -6),   S(  3, -11),   S(-15,  -3),   S(-70,   4),   S(  8,  -2),   S(-56,  18),
            S(  0,   4),   S( 45, -10),   S( 20, -13),   S( 24,  11),   S( 96,  -6),   S( -9,   4),   S( 70, -14),   S( 30,  -5),
            S( -5, -22),   S(-46,   8),   S( -6, -12),   S(  2,  -4),   S(  0,   0),   S(  0,   0),   S(-17, -18),   S(-62, -10),
            S(-16,   9),   S(-11,  10),   S(-27,   6),   S(-22,  -7),   S(  0,   0),   S(  0,   0),   S(-29,  30),   S(-42,  -7),
            S(  0, -38),   S( -3,  28),   S(  2,   9),   S(-10,   5),   S(  5,  11),   S( -8,   1),   S(-13,   2),   S(-26, -19),
            S(  8,  37),   S( -7,   3),   S(  1,  20),   S(  1,  -7),   S( -1,  14),   S( -1,   3),   S(  3,  26),   S( -1,  30),

            /* kings: bucket 7 */
            S( 34, -39),   S( -7,  11),   S( 10, -20),   S(-30,   6),   S(-39,  10),   S(-56,  38),   S( -6,  34),   S( -8,  18),
            S( 23,  -3),   S( 57, -36),   S( 24, -17),   S(-18,   5),   S(  1,   1),   S(-27,  18),   S( 10,   4),   S(  4,  17),
            S( 12,  -8),   S( -7,   3),   S(-35,  -2),   S(-18,  -4),   S(-26,  -1),   S(-47,  12),   S(-23,   9),   S(-70,  18),
            S( 11,  10),   S( 40, -20),   S( 11,  -1),   S( 17,   8),   S( -8,   9),   S( 35, -18),   S( 69, -19),   S( 28, -10),
            S( -2,  -1),   S( -8,  12),   S(-14, -14),   S(-27,   6),   S(-24,  -5),   S(  1, -20),   S(  0,   0),   S(  0,   0),
            S(-13, -35),   S( 20,  -4),   S( 27, -15),   S( 25,  -7),   S(-11, -13),   S(  9,   4),   S(  0,   0),   S(  0,   0),
            S( 17,  24),   S( 12, -10),   S(  7,   4),   S( -6, -26),   S( 18, -30),   S( -1,   8),   S(  9,   7),   S(  3,  -4),
            S( -1,  -9),   S(-13, -32),   S( 22,  20),   S(  0, -26),   S( 17,  12),   S(-15, -45),   S(  1,   2),   S(-10, -30),

            /* kings: bucket 8 */
            S(-30,  72),   S(-13,  61),   S(-37,  40),   S(-10,  -8),   S( -9,   4),   S(-22, -12),   S( 27, -27),   S( -2,   5),
            S( 25,  92),   S( 16,  13),   S( 23,  65),   S(  4,   8),   S( -1,   8),   S(  1,   6),   S(  2,  12),   S( 33,  41),
            S(  0,   0),   S(  0,   0),   S( 20,  49),   S( 10,  10),   S( 46,  -3),   S(  6,  -6),   S( -3,  25),   S( -5,   0),
            S(  0,   0),   S(  0,   0),   S( 17,  39),   S( 25, -35),   S(  7,  12),   S( 20,  -9),   S( 14, -15),   S( -4,  17),
            S(  3,  -2),   S(  1,   4),   S( 12, -24),   S( 16,   0),   S(  8, -20),   S( 13,   4),   S( -5,   5),   S( -7, -37),
            S( -4,   4),   S(  0,  -6),   S( -8,  -3),   S(-10, -38),   S(-17,  -6),   S(-11,  -5),   S( -2,  13),   S(  8,  11),
            S(  0, -15),   S(-10, -48),   S( -1, -34),   S(  3,  10),   S(  6, -18),   S(  6,  -8),   S(  9,  10),   S(  4, -45),
            S( -5,  -8),   S( -9, -48),   S(  6,  -7),   S( -2,  -4),   S(  8,  41),   S(-13, -40),   S(  6,   1),   S(  7,  -5),

            /* kings: bucket 9 */
            S(-33,  41),   S(-28,  28),   S(-50,  37),   S(-75,  23),   S(-56,  28),   S(-44,  15),   S( 70,  -1),   S( -9,  22),
            S(-29,  16),   S( 28,  35),   S(-31,  18),   S( 29,  21),   S( 29,  24),   S( 23,  -3),   S( 26,  17),   S( 43,   6),
            S(-10,  28),   S( -2,   7),   S(  0,   0),   S(  0,   0),   S( 23,  16),   S( -6,  -5),   S( 22,  -3),   S(  0,   4),
            S(  2, -25),   S(-11, -17),   S(  0,   0),   S(  0,   0),   S( 18,  14),   S( 19,  -4),   S(-11,  10),   S(-24,  22),
            S( -2,  13),   S( -2, -12),   S( -1,  15),   S(  7, -18),   S( 10, -14),   S( -4,  -4),   S( 11,  14),   S( -5,   1),
            S(  0,  37),   S( -6,  28),   S(  2,  -1),   S( -9,   5),   S( 11,  12),   S( -7,  14),   S( -9, -11),   S(  1,  28),
            S(  0,   1),   S( -4, -25),   S(  4,  21),   S( -7,  -8),   S( -2,  -5),   S( 16,  26),   S(  0,  -4),   S(  5,  11),
            S(  9,  57),   S( -3,  -6),   S( 11,  16),   S( -1, -25),   S(  0, -32),   S(  6,   2),   S(-10, -26),   S(  7,  38),

            /* kings: bucket 10 */
            S( -9,  44),   S(-19,   0),   S(-17,  -3),   S(-32,  34),   S(-70,  17),   S(-106,  40),  S( 12,  27),   S(-82,  79),
            S(  4,   6),   S( 56,  -1),   S(  0,  -5),   S( 15,  18),   S( 63,  17),   S( 21,  12),   S( 21,  27),   S(-41,  36),
            S( -4,   5),   S( -1,   1),   S( 13,  -8),   S( -9,  13),   S(  0,   0),   S(  0,   0),   S( 21,   5),   S(-34,   9),
            S(  8,   2),   S( 11,  -4),   S( 14, -14),   S( 35,   0),   S(  0,   0),   S(  0,   0),   S( 26,  14),   S( 14,   6),
            S( -4,  -2),   S( 30,   9),   S(  7,  11),   S( -9, -40),   S(  6, -21),   S(  5,   3),   S(  9,   3),   S(-23,   5),
            S(-11,  13),   S( -9,   7),   S(-14,  10),   S(  7,  15),   S( -9,  27),   S( -2,  -9),   S( -8,  19),   S( -7,  24),
            S(  3, -10),   S( 11,   4),   S(  9,  -4),   S( 12,  30),   S( 16,  22),   S( -4,  -8),   S(  8, -20),   S(  1,  11),
            S(  5,   9),   S(  8, -45),   S(  1,   8),   S( -3,   3),   S(  6, -12),   S(  3,  -8),   S( -3, -23),   S(  3,  11),

            /* kings: bucket 11 */
            S(-55,  36),   S( 25,  -4),   S( -7, -16),   S(-32,   3),   S(-36,  -5),   S(-167,  73),  S(-35,  45),   S(-139, 144),
            S( 21, -36),   S(  0,  18),   S(  5, -31),   S( 35,   8),   S( 40,  12),   S( 18,  47),   S( 42,  15),   S( -8,  60),
            S(-12, -14),   S( 33,  -7),   S( -9,  -5),   S( -9,   2),   S( 65,  -4),   S( 17,  42),   S(  0,   0),   S(  0,   0),
            S(  0,  35),   S(  5,  -8),   S( 27,  -2),   S( 57, -17),   S( 37, -25),   S( 30, -10),   S(  0,   0),   S(  0,   0),
            S( 12,  -3),   S( -2,  -5),   S( -8,   2),   S(  8,  -6),   S( 28, -18),   S( 10,  -7),   S(  2,  -9),   S(  7,  22),
            S( 13,   4),   S(-16,  13),   S( 13,  -5),   S(-12, -23),   S( -3, -27),   S( -6,   1),   S( -2,  11),   S( -8, -24),
            S(  4, -35),   S(  2,   0),   S( 15,  31),   S(  4,  -4),   S(  2,   6),   S(  3,   4),   S( -5, -50),   S( -8,  -3),
            S(  2, -17),   S( 19,  15),   S(-10, -36),   S(  3, -11),   S( -2,  -9),   S( -6, -26),   S(  0, -13),   S( 10,  33),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  7,  93),   S(-10,   9),   S(  3,   2),   S(  7,   4),   S(  0, -16),   S(-17,  43),
            S(  0,   0),   S(  0,   0),   S( 20,  85),   S(  4, -16),   S(  9,  28),   S( 16,   3),   S( 11,  -7),   S( -3,  19),
            S( -4, -19),   S(  2, -34),   S( 16,  31),   S( 11,  22),   S(  6, -27),   S(  6,   6),   S( -2,   1),   S(-15,   8),
            S( -5, -31),   S( 11,  41),   S( -6, -47),   S(  5, -55),   S(-19, -31),   S(  8,  22),   S( -6,   7),   S(  8,  11),
            S( 12,  47),   S(  3,  29),   S(  0, -18),   S(  6,  -9),   S( -9,   2),   S(-13,   1),   S(-12,   4),   S( -4,  16),
            S(  6,  30),   S(  3,  15),   S( -1,  17),   S( -7, -19),   S(  2,  30),   S( -7,  10),   S(-11,   2),   S( -1,  16),
            S(  9,  29),   S( -7, -18),   S(  4,  28),   S( -2,   2),   S(  0,  -5),   S( -7,   0),   S(  6,  29),   S( -8, -25),
            S( -2,  -7),   S(  0,  -5),   S(  1, -21),   S( -2, -23),   S( -2, -20),   S(  6,   4),   S( -2,  -3),   S(-11, -38),

            /* kings: bucket 13 */
            S(-21,  74),   S( -3,  72),   S(  0,   0),   S(  0,   0),   S(  2,  89),   S(-23, -34),   S( 13,  -7),   S(  1,  52),
            S(-17,  21),   S( -5,  -1),   S(  0,   0),   S(  0,   0),   S( 19,   2),   S(  0, -29),   S(-30,  21),   S(  4,  16),
            S(-11, -13),   S(  8,  32),   S( -7, -54),   S( -1,  -1),   S( -1,   7),   S( -3,  13),   S( -9,  -4),   S(  1,  27),
            S( -6, -14),   S(  1,  28),   S(  4,   8),   S( -2, -47),   S(  6, -24),   S(  3, -24),   S( -1,  13),   S(-14,  -8),
            S(  1,   9),   S( -6, -12),   S(  5,  26),   S( -9, -38),   S( -3,  -8),   S( -6, -10),   S( -8,   4),   S(  1,  25),
            S( -1,  10),   S(  1,  28),   S( -8, -12),   S(  5,  20),   S( -8, -29),   S( -3,  10),   S(-11, -20),   S(  5,  11),
            S(  7,  19),   S( -8, -11),   S( -6,  -2),   S(  2,  -8),   S( -4, -33),   S(  6, -15),   S(  0, -33),   S(  9,  35),
            S(  0,  -6),   S(  0,  12),   S(  1,  14),   S(  3,  -5),   S( -3,  -2),   S( -2,  27),   S( -3,  -5),   S( 10,  31),

            /* kings: bucket 14 */
            S(  5,  74),   S(-25,   3),   S(-12, -28),   S( -2,  24),   S(  0,   0),   S(  0,   0),   S(  6,  93),   S(-62,  59),
            S(-23,  11),   S( -6,  -4),   S( -1,  -7),   S( 16,  12),   S(  0,   0),   S(  0,   0),   S( 29,  24),   S(-32,  -8),
            S(-13,  -8),   S(  8,   2),   S( 13, -19),   S(  5,   4),   S(  2, -20),   S(  1,  16),   S(  8,  27),   S(-13,  13),
            S( -1,   5),   S( -3,  16),   S(  1, -28),   S(  9, -40),   S(-10, -41),   S( 13,  17),   S(-17,  14),   S(  3,   2),
            S(  3,  11),   S(  0,  13),   S( -8, -11),   S( -4, -14),   S(-18,   0),   S(  8,  29),   S( -3,  22),   S(  5,  18),
            S(-12, -33),   S(  1,  34),   S( -4,  -2),   S( -4,  35),   S(-10, -14),   S( -1, -10),   S(  3, -26),   S( -1,  11),
            S(  1,   1),   S( -2, -33),   S(  6,  29),   S(  1,   2),   S( -4, -14),   S(  0, -15),   S(-15, -67),   S(  6,  52),
            S( -2,   2),   S(  5,  50),   S( -1,  10),   S( -4, -17),   S(  8,  45),   S( -4, -16),   S( -8, -25),   S(  1, -24),

            /* kings: bucket 15 */
            S(  3,  38),   S(  6,   6),   S( 22,  32),   S(-14, -34),   S(-40, -11),   S(-19,  85),   S(  0,   0),   S(  0,   0),
            S( -1, -30),   S( -7,   4),   S( -5, -24),   S( 13,  23),   S( 21, -36),   S( 37, 111),   S(  0,   0),   S(  0,   0),
            S(-25, -17),   S( 17,  15),   S( -3,  -5),   S(-13, -10),   S( -3, -36),   S( 26,  39),   S( 21,  37),   S(-13, -22),
            S(  3,  -7),   S(-10,  33),   S(  6,  18),   S( -7, -26),   S(  0, -51),   S( 15,  10),   S(  3,  47),   S(-13, -42),
            S(  6,  -8),   S(-17,  31),   S(-15,  -7),   S(-15, -54),   S( -2, -11),   S(  2,  37),   S( -3,   6),   S( -1,  11),
            S(  4,  12),   S(-18,   4),   S( 12,  19),   S( 17,  25),   S( -5,   1),   S( -5,  -6),   S(  0,   4),   S(  6,  15),
            S( 13,  38),   S( -4,  -8),   S( -6,  -5),   S( -3,  -5),   S(  3, -16),   S(  0,   1),   S( -5, -19),   S(  0,   4),
            S( -7, -13),   S( 12,  20),   S( -3,  -7),   S( -3, -11),   S(  4,  30),   S(  8,  49),   S(  2,  16),   S(  3,  -1),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-14, -197),  S(  6, -30),   S( 16,  40),   S( 26,  77),   S( 33, 101),   S( 37, 121),   S( 43, 121),   S( 56, 106),
            S( 81,  71),

            /* bishop mobility */
            S(  3, -70),   S(  6,   7),   S( 16,  50),   S( 22,  88),   S( 26, 116),   S( 30, 136),   S( 31, 146),   S( 31, 150),
            S( 32, 152),   S( 37, 150),   S( 48, 142),   S( 62, 136),   S( 95, 126),   S(128, 105),

            /* rook mobility */
            S(-23,  -7),   S(-15,  52),   S(-14,  79),   S( -9,  97),   S(-10, 119),   S( -4, 128),   S(  1, 137),   S( 10, 136),
            S( 13, 146),   S( 19, 149),   S( 29, 150),   S( 45, 144),   S( 52, 141),   S( 69, 130),   S(150,  90),

            /* queen mobility */
            S( 48, -25),   S( 54, -86),   S( 42,  59),   S( 39, 123),   S( 41, 161),   S( 41, 204),   S( 40, 239),   S( 40, 265),
            S( 41, 284),   S( 38, 305),   S( 38, 324),   S( 33, 346),   S( 33, 355),   S( 31, 363),   S( 31, 378),   S( 31, 382),
            S( 27, 385),   S( 41, 373),   S( 51, 364),   S( 68, 343),   S( 93, 325),   S(150, 280),   S(141, 280),   S(188, 247),
            S(213, 237),   S(173, 244),   S(124, 221),   S( 91, 187),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  31),   S(-15,  56),   S(-32,  55),   S(-24,  74),   S(  3,  29),   S(-22,  40),   S( -1,  73),   S( 35,  31),
            S( 11,  46),   S(  6,  60),   S(-15,  53),   S(-18,  47),   S( -5,  42),   S(-28,  56),   S(-31,  78),   S( 43,  33),
            S( 25,  90),   S( 19,  90),   S(  1,  69),   S( 23,  55),   S( -4,  63),   S(-34,  81),   S(-31, 113),   S( -1,  86),
            S( 43, 122),   S( 49, 131),   S( 19,  96),   S(  5,  70),   S(  2,  72),   S(  5,  96),   S(-31, 132),   S(-50, 146),
            S( 56, 159),   S( 87, 176),   S( 73, 117),   S( 18, 115),   S(-60, 104),   S( 33, 106),   S(-82, 158),   S(-127, 163),
            S( 90, 208),   S(105, 231),   S(145, 217),   S(130, 213),   S(134, 232),   S(137, 229),   S(112, 243),   S(131, 244),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  21),   S(  4,  -2),   S(  7,  11),   S(  9,  34),   S( 20,  42),   S( -2, -23),   S(-17,  40),   S(  9, -32),
            S(-12,  34),   S( 29,   8),   S( -2,  44),   S( 21,  43),   S( 37,   2),   S( -1,  34),   S( 19,   0),   S(  8,   6),
            S( -7,  19),   S( 18,  12),   S( -4,  50),   S( 20,  74),   S( 27,  28),   S( 28,  32),   S( 31,  -2),   S( -5,  28),
            S(  9,  43),   S( 26,  37),   S( 41,  92),   S(  3,  93),   S( 88,  61),   S( 65,  51),   S( 30,  44),   S( 16,  29),
            S( 51,  99),   S(120, 123),   S(114, 124),   S(136, 180),   S(147, 180),   S(137, 148),   S(184, 109),   S(110,  54),
            S( 78, 201),   S(124, 284),   S(112, 246),   S(113, 248),   S( 86, 206),   S( 56, 151),   S( 38, 166),   S( 18, 121),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  50),   S( 26,  36),   S( 40,  59),   S( 40,  38),   S( 29,  36),   S( 29,  39),   S(  7,  25),   S( 45,  14),
            S( -1,  25),   S( 18,  43),   S( 14,  34),   S( 13,  50),   S( 27,  14),   S(  9,  27),   S( 29,  19),   S( -3,  16),
            S(  4,   5),   S( 26,  24),   S( 51,  39),   S( 41,  28),   S( 42,  42),   S( 64,   8),   S( 15,  27),   S( 19,   1),
            S( 77,  15),   S(127,  -3),   S(139,  65),   S(165,  88),   S(161,  74),   S( 94,  85),   S(101,   9),   S( 85, -23),
            S(111,  46),   S(145,  36),   S(189, 167),   S(195, 154),   S(190, 163),   S(129, 177),   S(182, 124),   S(-29, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 18,  34),   S(  8,  23),   S( 19,  36),   S(  1,  56),   S( 70,  28),   S( 25,   1),   S( -2,   5),   S( 25,  22),
            S(  1,  15),   S(  8,   7),   S( 19,  17),   S( 16,  32),   S( 15,  15),   S(  1,  10),   S(  3,   9),   S( 27,  -2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -15),   S( -8,  -7),   S(-19, -17),   S(-16, -32),   S(-15, -15),   S( -1, -10),   S( -3,  -9),   S(-27,   2),
            S(-18, -34),   S( -8, -23),   S(-19, -36),   S( -1, -56),   S(-70, -28),   S(-25,  -1),   S(  2,  -5),   S(-25, -22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -24),   S(-11, -10),   S(  0, -28),   S(-57,  -5),   S(-18, -12),   S(-18, -22),   S(  1, -24),   S(-13, -37),
            S(-17,   3),   S(-16,  -3),   S(-27,  13),   S(  4, -18),   S(-30, -16),   S(-24,  -6),   S(-40,   4),   S(-10, -17),
            S(-11, -18),   S( -7, -22),   S(-26,  -3),   S(-28, -10),   S(-22, -34),   S(-24,  -9),   S(-15,  -8),   S(-40, -19),
            S(  2, -33),   S( 20, -45),   S( 17, -22),   S(  7, -32),   S(  3, -16),   S( 60, -37),   S( 41, -33),   S( -5, -47),
            S( 34, -68),   S( 65, -82),   S( 53, -31),   S( 64, -33),   S( 68, -38),   S( 77, -21),   S(140, -71),   S( 35, -74),
            S(130, -99),   S(178, -128),  S(129, -73),   S(110, -50),   S( 83, -31),   S(126, -28),   S(142, -71),   S( 70, -97),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 16,   2),        // attacks to squares 1 from king
            S( 17,   1),        // attacks to squares 2 from king

            /* castling available */
            S( 71, -73),        // king-side castling available
            S( 13,  50),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 16,  58),   S( 18,  66),   S(  8,  64),   S( -4,  78),   S(-14,  87),   S(-31,  98),   S(-61, 116),   S(-73, 122),
            S(-88, 126),   S(-68, 130),   S(-66, 139),   S( 27, 122),   S(  0, 129),

            /* orthogonal lines */
            S( -8, -50),   S(-57, -22),   S(-79,  -9),   S(-93,  -7),   S(-100, -12),  S(-102, -17),  S(-101, -23),  S(-99, -24),
            S(-116, -20),  S(-138, -19),  S(-163, -25),  S(-166, -30),  S(-174, -23),  S(-345,  29),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S(  5, 255),

            #endregion

            /* tempo bonus for side to move */
            S(  8,   9),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
