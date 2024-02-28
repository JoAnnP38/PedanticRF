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
        public const int MAX_WEIGHTS = 12685;
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
        public const int TEMPO = 12684;             // tempo bonus for side moving

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

        // Solution sample size: 6000007, generated on Tue, 27 Feb 2024 16:42:16 GMT
        // Solution K: 0.003850, error: 0.086542, accuracy: 0.4930
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 95, 179),   S(426, 527),   S(431, 564),   S(526, 977),   S(1278, 1686), S(  0,   0),

            /* friendly king piece square values */
            #region friendly king piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(106, -121),  S(152, -85),   S( 42, -30),   S(-50,  39),   S(-41,  17),   S(-26,  -1),   S(-53,  -6),   S(-46, -19),
            S(112, -122),  S( 98, -95),   S(  3, -63),   S(-15, -62),   S(-24, -21),   S(-22, -38),   S(-48, -18),   S(-42, -38),
            S( 92, -85),   S( 69, -55),   S( 13, -52),   S( -5, -56),   S( -5, -61),   S( -5, -48),   S(-24, -46),   S(-26, -37),
            S( 51, -29),   S( 42, -39),   S( 10, -32),   S( 10, -58),   S( -6, -50),   S(-31, -45),   S(-35, -36),   S(-42, -17),
            S( 67,  37),   S( 24, -13),   S( 38, -18),   S( 51, -57),   S( 29, -51),   S(-27, -52),   S(-40,   3),   S(-45,  63),
            S( 60,  57),   S(  2,  72),   S(-33, -10),   S(  9, -52),   S(-29, -29),   S(  7, -16),   S(-20, -31),   S(-54,  39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 28, -36),   S( 33, -43),   S( 53, -30),   S(  0,  28),   S( -4, -14),   S(  6, -22),   S(-44,  -5),   S(-36,  11),
            S( 13, -38),   S(  6, -44),   S( 11, -48),   S( -6, -43),   S( -7, -28),   S(-12, -34),   S(-42, -20),   S(-47, -15),
            S(  4, -23),   S( 11, -25),   S( 23, -45),   S( 10, -51),   S(-16, -29),   S( 10, -43),   S(-20, -28),   S(-20, -18),
            S( 10,   6),   S( 11, -41),   S( 10, -42),   S(  3, -43),   S( -3, -27),   S( -7, -29),   S(-36, -18),   S(-34,  15),
            S(-16,  67),   S(-45,   3),   S( -7, -14),   S( -2, -29),   S( 29, -27),   S(-31,  16),   S(-41,  26),   S(-35,  72),
            S( 64,  68),   S( -2, -23),   S(-54, -22),   S(-51,  15),   S(-78,   4),   S(-59,  -4),   S(-72,  17),   S(-122, 101),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29,  -3),   S(-28,  -6),   S(-19,   1),   S( -2,   2),   S( 16, -13),   S( 42, -24),   S(  8, -48),   S(-17, -19),
            S(-29, -22),   S(-54, -10),   S(-27, -40),   S(-20, -43),   S(  5, -28),   S(  7, -27),   S(-13, -40),   S(-31, -24),
            S(-45,   0),   S(-28, -22),   S(-14, -46),   S( -6, -56),   S(  2, -26),   S( 12, -26),   S( -4, -33),   S(-18, -14),
            S(-51,  16),   S(-30, -33),   S(-35, -33),   S( -9, -49),   S(  8, -40),   S(-13, -13),   S(-16, -11),   S(-32,  11),
            S(-48,  66),   S(-61,   9),   S(-61, -20),   S(-64, -12),   S(  9,   9),   S(-13,  10),   S( -8,  19),   S(-44, 100),
            S(-93,  94),   S(-147,  74),  S(-136,   3),  S(-71, -48),   S(-47,  12),   S(-81,  17),   S(-55,  14),   S(-69,  72),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -11),   S(-41,  -4),   S(-31, -13),   S( -1, -49),   S(-10, -11),   S( 41, -13),   S( 86, -59),   S( 61, -77),
            S(-39, -31),   S(-55, -23),   S(-36, -46),   S(-22, -37),   S(-22, -26),   S(  2, -37),   S( 52, -61),   S( 53, -73),
            S(-45, -23),   S(-22, -46),   S(-11, -63),   S( -5, -68),   S( -8, -50),   S(  4, -38),   S( 26, -47),   S( 57, -54),
            S(-45,  -6),   S(-19, -57),   S(-22, -58),   S( -2, -73),   S( 12, -63),   S( -5, -46),   S(  8, -29),   S( 44, -14),
            S(-18,  36),   S(-37, -13),   S( -6, -64),   S( -8, -52),   S( 67, -38),   S( 50, -33),   S( 45,  32),   S( 33,  90),
            S(-101, 126),  S(-100,  38),  S(-32, -49),   S(-59, -59),   S(-21, -34),   S( 31, -37),   S( 19,  49),   S( 32,  76),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47, -16),   S( 34, -38),   S(-15,  -7),   S(-16,  10),   S(-29, -26),   S(-28,   0),   S(-68,   5),   S(-76,   9),
            S(  4, -15),   S( 66, -13),   S( 28, -26),   S( 22, -24),   S(-30, -12),   S(-59, -16),   S(-32, -30),   S(-11, -34),
            S( 47, -20),   S( 83, -19),   S( -2,  -2),   S(  1, -29),   S(-46, -24),   S( -7, -44),   S(-53, -30),   S(-33,  -9),
            S( 47,  15),   S(  5,  35),   S( 25,   9),   S( 33, -18),   S(-12, -32),   S(-53,  -8),   S( 21, -45),   S( 30, -26),
            S(  5,  79),   S( 37,  70),   S(-24,   8),   S(-20,  18),   S( 45,  -6),   S(-21,  -7),   S(-37, -36),   S( 43,  25),
            S( 53,  99),   S( 85,  73),   S( 17,  25),   S( 28,  -4),   S( 18, -27),   S(  2, -18),   S( 19, -16),   S(-44,  15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-76,  45),   S(-50,  18),   S( 10,   4),   S( 43,  -2),   S(-12,  14),   S( -8,   5),   S(-36,   3),   S(-41,  23),
            S(-71,  23),   S(-41,   5),   S( 43, -21),   S(  5,  16),   S( 24, -13),   S(-20, -15),   S(-28, -13),   S(-70,  19),
            S(-68,  46),   S(-33,  18),   S( 88, -42),   S( 21, -27),   S( 43, -15),   S(-41,   1),   S(-17,  -3),   S(-28,  11),
            S(-38,  58),   S(-25,  24),   S( 26,   7),   S( 56,   9),   S( 12,   1),   S(-62,  14),   S( 18, -13),   S( 11,   8),
            S( 46,  73),   S( 68,  39),   S( 73,  37),   S( 19,  31),   S(-46,  55),   S( 51,   5),   S(-17,  20),   S( -7,  60),
            S( 92,  43),   S( 65,  28),   S( 41,   5),   S( 40,   1),   S( 44,   6),   S(  3,  19),   S(  5,   3),   S( 23,  44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43,  22),   S(-20,  -1),   S(-10,   2),   S( 12, -17),   S( 32, -30),   S(  5,  -9),   S(-54,  -3),   S(-55,  14),
            S(-46,  -2),   S( -1, -26),   S(-11, -33),   S( 39, -22),   S( 74, -23),   S( 57, -30),   S(-23, -15),   S(-78,  12),
            S(-52,  12),   S( -1, -12),   S(-11, -22),   S(-22, -10),   S( 38, -17),   S( 61, -28),   S(  3, -17),   S(-45,  22),
            S(-38,  24),   S(-55,   6),   S( 29, -25),   S(  3,  -3),   S( 34,   5),   S( 49,  -1),   S( 30,  -8),   S( 32,   7),
            S(-19,  54),   S(-29,  18),   S( -2, -10),   S( 36, -20),   S( 43,  36),   S( 87,  31),   S( 65,   6),   S( 64,  45),
            S( 43,  55),   S( 26,   2),   S( -2, -26),   S( 17, -50),   S( 50, -13),   S( 49, -26),   S( 41,   7),   S( 84,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-67, -13),   S(-56, -11),   S(-10, -19),   S(-54, -27),   S(-26, -21),   S( 58, -45),   S( 33, -61),   S(-40, -30),
            S(-56, -34),   S(-58, -40),   S(-32, -50),   S( -2, -51),   S(  0, -33),   S( 67, -49),   S( 71, -45),   S( -6, -39),
            S(-78, -18),   S(-66, -29),   S(-36, -43),   S(-11, -48),   S(  2, -24),   S( 35, -26),   S( 58, -49),   S( 49, -38),
            S(-52, -17),   S(-68, -36),   S(-79, -40),   S(-36, -21),   S( -4, -24),   S( 27, -12),   S( 30,  -8),   S( 85, -25),
            S(-36,  -3),   S( -1, -36),   S(-35, -39),   S( -3, -50),   S( 18,   4),   S(  4,  21),   S( 74,  54),   S(123,  50),
            S(-19,  -1),   S(-28, -36),   S( 58, -59),   S(-12, -65),   S( 16, -52),   S(  7, -31),   S( 22,  57),   S( 49,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  28),   S(-21,  21),   S( 13,  30),   S(-20, -28),   S(-11,  -3),   S(-82,  10),   S(-66,   5),   S(-61,  23),
            S(-57,  33),   S( -2,  -6),   S(-34,  22),   S(-50, -26),   S(-62,  -4),   S(-37, -21),   S(-21, -35),   S(-12,   0),
            S(-10,  65),   S( 22,  60),   S( 26,  29),   S(-13,  35),   S(  0,  -6),   S(-18, -35),   S(-41, -43),   S(-10, -23),
            S(  8,  99),   S( 65, 112),   S( 79,  65),   S( 26,  36),   S( -2,  -5),   S(-51, -21),   S(  0, -14),   S( 24, -17),
            S( 36, 137),   S( 71, 134),   S( 42, 134),   S( 67, 105),   S( 25,  -6),   S( 14,  11),   S(-10, -42),   S( 13, -12),
            S(109, 158),   S( 96, 163),   S( 87, 177),   S( 50,  59),   S(  8,  -2),   S(  6,  -1),   S( -3, -33),   S( -4, -34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-93,  52),   S(-74,  43),   S(-17,  17),   S( 12,  31),   S(-18, -20),   S(-80,  29),   S(-95,  14),   S(-108,  34),
            S(-48,  12),   S(-26,  10),   S(-49,  20),   S(-10,  26),   S(-78,   8),   S(-77,  -1),   S(-138,   9),  S(-61,  11),
            S(-48,  53),   S(-26,  55),   S( -1,  62),   S(-55,  67),   S(-69,  54),   S(-106,  15),  S(-36, -18),   S(-48,   9),
            S( 20,  72),   S( 89,  69),   S( 65, 111),   S(101, 120),   S(-32,  67),   S(-37,  28),   S(  2,   4),   S( 69, -20),
            S( 83,  86),   S( 85, 129),   S( 87, 133),   S(111, 135),   S( 79, 102),   S( 34,  36),   S( 38, -18),   S( 41,  10),
            S( 26,  77),   S( 72,  94),   S(107, 103),   S(112, 163),   S( 46, 104),   S(  0,   6),   S(  8, -21),   S(  3,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-138,  21),  S(-129,  10),  S( -9,  -8),   S( -4,  38),   S(-21, -11),   S(-125,  43),  S(-151,  34),  S(-113,  41),
            S(-129,   6),  S(-53, -18),   S(-13, -26),   S(-79,  19),   S( -9,  14),   S(-23,  16),   S(-120,  22),  S(-99,  20),
            S( -4, -12),   S(-67,   2),   S(-36,  21),   S(-75,  53),   S(-70,  82),   S( 50,  27),   S(-107,  34),  S(-79,  41),
            S( 35,  -9),   S( -4,  12),   S( 17,  18),   S(-20,  81),   S( 50,  95),   S(  3,  82),   S( 48,  32),   S( 19,  22),
            S( 66,  -2),   S(  8,   5),   S( 45,  59),   S( 76, 107),   S( 82, 130),   S( 93,  79),   S( 52,  73),   S( 96,  50),
            S( 47,  -2),   S( 33,  -7),   S( 37,  34),   S( 74, 134),   S( 52,  77),   S( 49,  66),   S( 37,  50),   S( 45,  85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-148,  11),  S(-133,  13),  S(-24, -21),   S(-30, -15),   S( -7,  16),   S(-113,  46),  S(-49,  24),   S(-84,  42),
            S(-74, -27),   S(-65, -23),   S(-45, -35),   S(-40,  13),   S(-35,   7),   S(-33,   7),   S(-69,  33),   S(-101,  33),
            S(-59, -26),   S(-47, -45),   S(-56,  -3),   S(-19,   2),   S(-47,  30),   S( 13,  40),   S(-13,  52),   S(-30,  51),
            S( 13, -28),   S(-32, -37),   S(-39, -22),   S(-15,  11),   S( 24,  46),   S( 57,  41),   S( 67,  93),   S(109,  45),
            S( 14,   6),   S(-37, -15),   S( -9,   2),   S(-21,  29),   S( 70,  87),   S( 33,  90),   S(157, 110),   S(149,  89),
            S( 14, -16),   S(  7, -15),   S( 24, -13),   S( 11, -20),   S( 33,  29),   S( 38, 113),   S( 81, 149),   S(133, 123),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  20),   S(-16,  -6),   S(-26, -10),   S(  1,   1),   S(-15, -25),   S(-19,  10),   S(-39, -10),   S(-34,  10),
            S(-22,  -7),   S(  9,  -7),   S(  5,  38),   S(  1,  -6),   S(  0,  48),   S( -2,  -1),   S(-44, -23),   S(-29, -53),
            S(-15,  47),   S( -2,  42),   S( 26,  74),   S( 31,  37),   S( -3,   1),   S(-17, -39),   S(-27, -43),   S(-36, -45),
            S(  5,  91),   S( 24,  98),   S( 59, 104),   S( 55,  79),   S( -9, -42),   S(-18, -50),   S(  5,  -7),   S(-41, -68),
            S( 66,  92),   S( 91, 217),   S( 69, 150),   S( 21,  56),   S(  4, -18),   S(  1, -17),   S( -4, -45),   S(-18, -80),
            S( 50, 134),   S( 55, 279),   S(123, 247),   S( 63, 100),   S( -2,   9),   S( -9, -50),   S( -9, -56),   S(-16, -81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -31),   S(-22,   1),   S( -1,  14),   S( -3,   2),   S( -2,  12),   S(-30, -26),   S(-29, -28),   S(-34,  14),
            S(-18, -54),   S(-34, -19),   S(-17, -32),   S( -1,   9),   S(-37,  40),   S(-14,  -8),   S(-49, -38),   S(-29, -21),
            S( 17,   6),   S( -3, -15),   S( 13,  25),   S( 26,  71),   S(-12,  25),   S(-18, -21),   S(-27, -54),   S(  1, -54),
            S(  8,  30),   S( 61,  39),   S( 51,  80),   S( 30,  75),   S( 21,  39),   S( 26, -23),   S(  5, -26),   S( 15, -75),
            S( 34,  70),   S( 79, 138),   S(109, 179),   S(119, 198),   S( 52, 126),   S( -8, -10),   S( 10, -34),   S(  3, -78),
            S( 31,  70),   S(112, 182),   S(106, 242),   S(103, 253),   S( 69, 145),   S(  9,  15),   S(  1, -18),   S( 16, -26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -52),   S(-21, -40),   S(  1,  -9),   S( -4, -13),   S(  0,  -2),   S(-42,  39),   S(-41, -13),   S( -1,  45),
            S(-27,  -8),   S(-30, -50),   S(-26, -63),   S(  9,  22),   S(-26,  32),   S(-14,   1),   S(-15,  10),   S(-13,   5),
            S( 10, -23),   S(  1,  -8),   S(-18, -24),   S( -5,  22),   S( -3,  33),   S(-12,   7),   S(  3, -12),   S( -3,  25),
            S(  4, -45),   S( 21, -18),   S( 17, -28),   S( 21,  62),   S( 29, 134),   S( 15,  83),   S(  1,  17),   S( 44,  38),
            S(  8, -41),   S( 30,  16),   S( 38,  50),   S( 49, 135),   S( 78, 202),   S(101, 149),   S( 60,  78),   S( 47,  72),
            S( 30, -14),   S( 34,   2),   S( 50,  96),   S( 67, 193),   S( 88, 231),   S( 60, 179),   S( 49, 136),   S( 46,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -36),   S(-38, -44),   S(-23, -36),   S( -2,  -8),   S(  8,  33),   S(  3,  45),   S(-27, -21),   S( 18,  53),
            S( -7, -51),   S(-28, -38),   S(-32, -44),   S(  6,  27),   S(-38,   8),   S(  4,  33),   S(  8,  45),   S( -2,  19),
            S( 12, -60),   S(-10, -87),   S(-12, -40),   S( -2,   7),   S(  1,  23),   S( 33,  52),   S( 29,  87),   S(  1,  57),
            S( -6, -46),   S(-51, -44),   S( -1, -14),   S( 15,  44),   S(  1,  26),   S( 54,  57),   S( 35, 150),   S( 17,  81),
            S(-16, -81),   S(-14, -63),   S(-18, -54),   S( 11,   3),   S( 53, 121),   S( 99, 134),   S( 69, 269),   S( 78,  93),
            S( 13, -12),   S(  0, -13),   S( -3, -27),   S( 15,  50),   S( 41, 105),   S( 80, 249),   S( 32, 230),   S( 59,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-12, -48),   S( 13,   2),   S(-19,   8),   S(-21,  -2),   S(-34,  -8),   S(  3, -64),   S(-53, -78),   S(-30, -33),
            S(-39,  43),   S( 37, -37),   S(-26,  -9),   S(  5, -38),   S( -8, -21),   S(-27, -16),   S(-57,  -5),   S(-67, -73),
            S( 12,  58),   S(-12,  -8),   S( 11, -47),   S(-29,  28),   S( 28,  -8),   S(-24, -17),   S( -7, -54),   S(-46, -71),
            S( 24, -44),   S( 43, -27),   S(  7,  29),   S( 16,  24),   S( 18, -13),   S(  1,  -9),   S(  9, -59),   S(  6,  -6),
            S(  7, -20),   S( 34,  -6),   S( 19,  -2),   S( 71, -21),   S( 39,  -8),   S( 33,  10),   S( -9,   1),   S(-64, -33),
            S( 11, -27),   S( 29,   4),   S( 35,   4),   S( 61, -27),   S( 24, -82),   S( 15, -24),   S(-21, -37),   S(-25, -32),
            S( -6, -16),   S(  9, -53),   S( 25, -43),   S( 87, -41),   S(  8, -24),   S(  5, -77),   S(-13, -75),   S(-36, -70),
            S(-71, -62),   S(  6,  19),   S( -8, -30),   S( -4, -61),   S(-21, -24),   S( 13,  12),   S( -9,  -7),   S( 15,  12),

            /* knights: bucket 1 */
            S(-30,  38),   S(-48,  81),   S( 24,   8),   S(-19,  53),   S( -5,  26),   S(-27,  37),   S(-39,  47),   S(-17,  17),
            S(  6,  29),   S( -4,  27),   S(  4, -10),   S( -3,  14),   S(  2,  14),   S( 10, -10),   S( 17, -13),   S(  0,  -2),
            S(-47,  43),   S( 16,  -6),   S(  4,  -2),   S( 14,  21),   S(  7,  17),   S(-27,   7),   S(-10,  -5),   S(-31,  16),
            S( -8,  36),   S( 58,  16),   S( 12,  48),   S( 22,  27),   S(  5,  17),   S(  2,  20),   S( 10,   0),   S( -7,  21),
            S(-11,  45),   S( 18,  -9),   S( 11,  21),   S( 36,   9),   S( 16,  17),   S( 24,  11),   S( 11,   6),   S(  8,  16),
            S( 19,   2),   S( 18, -21),   S( 37,  22),   S( 47,  11),   S(  2,  15),   S( 32,  24),   S( 55,  -8),   S( -7,   2),
            S( 17,   5),   S( 22,   0),   S(  2, -30),   S(  4,  16),   S( 45, -13),   S( 43,  -9),   S(-21,  10),   S( -6, -35),
            S(-116, -62),  S(-17, -14),   S(-21, -18),   S(  6,   4),   S( -9,  -8),   S(-23,   8),   S(-17, -44),   S(-12, -75),

            /* knights: bucket 2 */
            S(-65,  22),   S( -1,  39),   S(-31,  34),   S(-22,  41),   S(-26,  46),   S(-24,  48),   S(-19,  34),   S(-30,   7),
            S(-45,   0),   S(-37,  14),   S(-16,  16),   S( -6,  17),   S( -1,  14),   S( -8,  35),   S(-17,  46),   S(-38,  68),
            S(-23,  22),   S(  0,  11),   S( -9,  25),   S( 22,   9),   S(  7,  20),   S( -2,   3),   S(-10,  23),   S(-40,  21),
            S( -6,  39),   S(-40,  46),   S(  0,  40),   S(  4,  41),   S( -3,  52),   S( -3,  45),   S(  4,  35),   S( -3,  35),
            S(  6,  11),   S(-21,  37),   S(-26,  59),   S(-20,  56),   S(-11,  53),   S(  0,  47),   S( -9,  31),   S( -3,   7),
            S(-24,  35),   S( -1,  29),   S(-26,  49),   S( -6,  19),   S( -5,  36),   S(-15,  41),   S(-38,  31),   S( 38, -12),
            S( 19,   0),   S(-32,  11),   S(-25,   5),   S(-13,   5),   S( 16, -11),   S( 31,  -7),   S(-29,  34),   S(-16,  -8),
            S(-142,  -5),  S(  4,  -6),   S(-86,  37),   S(-22, -24),   S(  1, -18),   S(-62,  13),   S(  7,  -2),   S(-158, -53),

            /* knights: bucket 3 */
            S(-74,  11),   S( -5,  -3),   S(-60,  19),   S(-21,   7),   S(-17,  33),   S( -8,  30),   S(  8,  18),   S(-40, -18),
            S(-30,  13),   S(-48,  -8),   S(-32,   1),   S( -5,  14),   S(  3,  13),   S(-18,  -1),   S(-26,  14),   S(-38,  61),
            S(-15, -23),   S(-12,  10),   S(-19,  13),   S(  6,  21),   S(  8,  24),   S(  1,  13),   S(-13,  13),   S(-29,  68),
            S(-18,  15),   S(-26,  36),   S( -4,  51),   S(  4,  46),   S( 12,  45),   S(  9,  50),   S( 15,  26),   S(  0,  35),
            S(  5,  13),   S(-11,  29),   S(  3,  35),   S( 10,  55),   S( -1,  56),   S( 10,  59),   S( 11,  43),   S(-13,  21),
            S( -8,   5),   S( 25, -11),   S( 30,   8),   S( 52,   9),   S( 60, -12),   S( 84, -16),   S( 17,  -5),   S(-25,  54),
            S( -9,  12),   S( -4,  14),   S( 49, -31),   S(  7,  -2),   S( 46, -19),   S( 68, -57),   S( 15, -40),   S( 33, -12),
            S(-137,  22),  S(-16, -12),   S(-26,   3),   S(-17,   6),   S( 20,   0),   S(-38,  -6),   S(  0, -27),   S(-88, -21),

            /* knights: bucket 4 */
            S( -1, -12),   S(-52,   1),   S(  6,  20),   S( 41,   0),   S( -4, -12),   S(-50, -52),   S(  2, -52),   S(-16, -30),
            S( 58,  46),   S(  7,  52),   S(-10, -39),   S(  3, -19),   S( 49, -30),   S( 17, -37),   S(  5, -26),   S(-25, -46),
            S( -5,  25),   S( 32,  18),   S( 33, -17),   S( 62,   3),   S( 30, -26),   S(-48,  11),   S(-15, -54),   S(-14, -70),
            S( 20,  49),   S( 62, -37),   S( 95, -12),   S( 54,  15),   S( 56,   1),   S( 76, -22),   S(  5, -40),   S(  4, -37),
            S( 42,  36),   S(-37,  57),   S( 52,  26),   S( 59,  -2),   S( 41,  22),   S( -7,  19),   S( 35, -42),   S( -7, -30),
            S( 21,  35),   S( -4, -20),   S( 85,   3),   S( 17, -21),   S( 49,   0),   S( 32,  20),   S( 15,  24),   S(-13, -24),
            S(  6,  33),   S(  4,  -1),   S( 11,  10),   S( 19,  20),   S( 18, -14),   S(  7, -36),   S( -6, -30),   S(-29, -28),
            S(-13, -10),   S( -6,  -8),   S(  8,  14),   S(-12, -19),   S(  4,   1),   S( 24,  41),   S(  2,  28),   S( -4, -20),

            /* knights: bucket 5 */
            S( 26,  26),   S(  2,  13),   S( 18,  11),   S( 27,  42),   S( 27,  -3),   S( 14,  -5),   S(-36,  10),   S(-28, -27),
            S( 18,   1),   S( 60,  30),   S( 40,   0),   S(  6,  26),   S( 64,  17),   S( 11,  22),   S(  6,  22),   S(-17,   6),
            S( 22,  37),   S(-10,  38),   S( 89,  -2),   S( 78,  -2),   S(  0,  30),   S( -2,  21),   S(  5,   6),   S(-13,  -2),
            S( 30,  48),   S( 43,  33),   S( 73,  19),   S( 35,  16),   S( 67,   7),   S( 26,  29),   S( 44,  30),   S( 13,  23),
            S( 38,  27),   S( 50,  15),   S( 73,  29),   S( 70,  34),   S(122,  25),   S( 54,  29),   S( 45,  26),   S(-10,  18),
            S(  5,  16),   S(-22,  32),   S(  9,  25),   S( 39,  44),   S( 43,  26),   S( 26,  39),   S( 29,   3),   S( -8,   4),
            S(  8,  47),   S(  4,  59),   S( 46,  31),   S( 17,  42),   S(-11,  35),   S( 10,  38),   S( 21,  59),   S(  0,  -4),
            S(  5,  30),   S( -8, -24),   S(  4,  35),   S(-15,  -7),   S( 16,  47),   S( -2,  28),   S(  9,  30),   S(-15, -22),

            /* knights: bucket 6 */
            S( 18,  -4),   S(  7,   4),   S( 26,  22),   S(-14,  39),   S(-15,  51),   S( 15,  60),   S( 11,  22),   S(  2,  56),
            S(  9, -29),   S( 82,  -2),   S( 33,   5),   S(  6,  13),   S( -4,  38),   S( 65,  23),   S( 69,  31),   S(-30,  41),
            S( -5, -18),   S( 14,   0),   S( 40,   7),   S( 65,   4),   S( 22,  35),   S(  5,  24),   S( 20,  54),   S( 18,  56),
            S( 66,  -9),   S( 40,  16),   S( 71,  26),   S(102,   4),   S( 48,  22),   S( 48,  34),   S( -3,  54),   S( 35,  44),
            S( 14,  21),   S(109, -10),   S(116,   7),   S( 95,  28),   S(137,  19),   S(132,  18),   S( 58,  21),   S( 52,  52),
            S( 30,   3),   S( 19,   4),   S( 77,  12),   S( 47,  36),   S( 86,  47),   S( 39,  28),   S( 20,  32),   S( 55,  47),
            S(  4,  37),   S(  4,  37),   S(-25,  33),   S( 28,  30),   S( 36,  31),   S( 12,  32),   S( 23,  58),   S(-20,  34),
            S(-33,   2),   S( 10,  40),   S( 23,  18),   S(  0,  30),   S( 32,  26),   S( 16,  45),   S( 26,  39),   S(  7,   6),

            /* knights: bucket 7 */
            S(-36, -40),   S(-188, -36),  S(-80, -17),   S(-35, -40),   S( 12, -33),   S(-59,   3),   S(-17,  15),   S(-34, -41),
            S(-47, -78),   S(-29, -30),   S(-47, -16),   S(-28, -26),   S(-20,  -3),   S( 26, -35),   S( 51,  11),   S(  9,  17),
            S(-61, -41),   S(-82, -16),   S(-22, -10),   S( 38, -29),   S(  9,  -1),   S( 49, -17),   S(-14,  57),   S( 45,  39),
            S(-62, -26),   S( 38, -42),   S( -7,   9),   S(100, -28),   S( 87, -13),   S( 34,  -1),   S( 27,   2),   S(-10,  37),
            S(-76, -27),   S(  2, -33),   S( 83, -32),   S( 82, -15),   S(148, -32),   S( 96,  10),   S( 82,   8),   S( 88,  19),
            S(-16, -37),   S(  8, -43),   S( 38, -27),   S( 54,  -3),   S( 62,   1),   S( 93, -23),   S( 88, -34),   S(  5,  31),
            S(-23, -34),   S(-61, -18),   S(-11, -28),   S( 36,   4),   S( 83,  17),   S( 39, -11),   S( -7,   2),   S(  0,   4),
            S(-70, -54),   S( -5,  18),   S(-17, -29),   S( 14,  20),   S(  0,  -1),   S(  8,  14),   S( -2, -21),   S(  3,  11),

            /* knights: bucket 8 */
            S( -4,  -4),   S( -5,   5),   S(  1,   9),   S(-10, -38),   S( -6,  -6),   S( -9, -30),   S(-10,   7),   S( -8, -31),
            S(  4,   9),   S(-13, -29),   S( -6, -39),   S( -7,  -6),   S(-32, -24),   S(-17, -73),   S(-12, -67),   S(-16, -49),
            S(  1,  21),   S(-14, -27),   S( 10,  10),   S( -4,   6),   S( -3, -30),   S(-19,  -2),   S( -5, -38),   S( -9, -22),
            S(  1,  44),   S(-10,   5),   S( -2,  14),   S( -3,  26),   S( 14,  -1),   S( -6,  -7),   S( -9, -38),   S(-11, -36),
            S( 29,  74),   S(-17,  -4),   S( 19,  26),   S( 24,  44),   S( -4,  28),   S(  7,  18),   S( 17,  17),   S(-10, -17),
            S( 15,  63),   S( -4,  10),   S( 36,  18),   S( 43,  43),   S( 10,  30),   S( -4,  -2),   S(  2, -29),   S(-17, -27),
            S( -5,  -1),   S(  6,  17),   S( -6, -31),   S( 11, -16),   S(  7,  19),   S( 17,  45),   S( -2,  -2),   S(  0,  -3),
            S(  7,  20),   S(  6,  27),   S(  9,  20),   S(  5,  13),   S(  8,  36),   S( -7, -32),   S(  0,   7),   S( -2,  -4),

            /* knights: bucket 9 */
            S( -6, -29),   S(-25, -44),   S(-18, -50),   S(  0,  -2),   S(-39, -91),   S(-20, -63),   S( -1,   1),   S(  2,  16),
            S(-18, -41),   S( -4,  13),   S( -7, -19),   S(-15,   0),   S( -1,   8),   S(-16, -47),   S( -5,   7),   S(-19, -51),
            S(  9,  -5),   S( -9, -15),   S(  2,  -1),   S( 15,   3),   S(  3,   8),   S(-34,   9),   S(-18,  12),   S( -4, -10),
            S(-12, -18),   S(  0,   3),   S( 15,   4),   S( 39,  20),   S( 21,  -9),   S(  1,  34),   S(-19, -54),   S(  7,  20),
            S( -2,  18),   S(  4,  -3),   S( 16,   5),   S( 11,  31),   S( 20,  21),   S(  1, -24),   S( 13, -22),   S(  3,  -6),
            S(  2,  -6),   S( 24,  49),   S(  5,  17),   S( 15, -14),   S( 22,  16),   S( 11,  16),   S(  6,  33),   S( -5, -26),
            S( -2, -18),   S(-17,  -7),   S( 25,  58),   S( -2,  18),   S( 16,  55),   S( -3,   4),   S( 10,  25),   S( -3,   8),
            S( -5, -18),   S(  2,  10),   S( 10,  22),   S( 17,  50),   S( 13,  10),   S( -3, -10),   S(  9,  36),   S( -1, -14),

            /* knights: bucket 10 */
            S(-19, -54),   S(-22, -66),   S( -1,  -7),   S(-32, -22),   S(-16, -23),   S(-12, -45),   S( -3,  -4),   S(  6,  28),
            S(-12, -41),   S( -9, -37),   S(-24, -37),   S(-15, -48),   S(-44, -70),   S( -6, -64),   S( -7,   5),   S( -5,   1),
            S(-10, -43),   S(-18, -53),   S( -7, -19),   S( -1, -25),   S( 14,  12),   S( -3,   5),   S(-14,  -8),   S( -8, -25),
            S( -6, -17),   S(  4, -38),   S( -7, -23),   S( 13,  -5),   S( 13,  -3),   S( 38,  -3),   S( -2,  38),   S( 13,  40),
            S( -9, -38),   S(-10, -29),   S( 10,   9),   S( 48,  -1),   S( 33,   8),   S( 22,   4),   S(  3,  16),   S( 23,  50),
            S( -5, -12),   S( -7, -16),   S( -7,  -6),   S( -6,  -6),   S( 33,  41),   S(  1,  30),   S( 15,  56),   S( 14,  63),
            S(  7,  20),   S(-15, -23),   S( -2,  -1),   S( 24,  51),   S( 10,  26),   S( -4,  27),   S( -3, -25),   S( 17,  50),
            S(  1,  -5),   S( -2, -10),   S(  4,  17),   S(  4, -25),   S( 12,  43),   S(  3,  19),   S(  6,  25),   S( -3, -14),

            /* knights: bucket 11 */
            S(  2,   8),   S(-25, -29),   S( -7, -42),   S( -9, -28),   S(-17, -45),   S(-15, -38),   S(  1,  -4),   S( -7,  -9),
            S(-11, -16),   S(-14, -46),   S(-11, -83),   S(-16, -31),   S(-22, -14),   S(-23, -25),   S(-19, -33),   S( -9,   1),
            S(-13, -48),   S(-49, -51),   S(-28,  -7),   S(  6,   9),   S(-12,  23),   S(-23,  28),   S( 12,  -8),   S( -8,  -3),
            S(-23, -37),   S( -9, -41),   S(  1,  22),   S( 27,  21),   S( 22,  16),   S( 14,   1),   S( -4,   4),   S(  0,  33),
            S(-21, -48),   S(-32, -58),   S( 10, -19),   S( 16, -24),   S( 15,  29),   S( 42,  29),   S(  8,  15),   S( 29,  99),
            S( -9,   0),   S(-12, -17),   S( -2,  25),   S( 42,  41),   S(  3,  -3),   S( 47,  34),   S( 16,  24),   S( 10,  23),
            S(  7,  37),   S( -4,   2),   S(-10, -30),   S( 14,   4),   S(  2,  23),   S( -7,  21),   S(  7,  40),   S( 15,  88),
            S( -5,  -3),   S(  0, -14),   S( 15,  29),   S(  5,   6),   S( -6,  -5),   S(  7,  12),   S(  3,   3),   S( -1,  15),

            /* knights: bucket 12 */
            S( -4, -11),   S(  1,   7),   S( -5, -25),   S( -2,  -4),   S( -4,  -1),   S( -3,   0),   S(  4,   4),   S( -2, -13),
            S( -1,  -3),   S(  2,   9),   S(  5,  20),   S(  1,  -7),   S( -6,  -9),   S( -3, -24),   S( -1, -17),   S(  1,  12),
            S( -2, -10),   S(  2,   2),   S(-10, -29),   S( -5, -12),   S(  2,  -2),   S( 10,  12),   S(  3,  -5),   S( -5, -21),
            S( -5,  -9),   S( -1,  -1),   S( -2, -18),   S(  0,  22),   S( -4,  -4),   S( -2, -12),   S(  3,   4),   S( -1, -23),
            S(  2,  11),   S( -3, -16),   S(  0, -18),   S( 10,  26),   S( -2, -24),   S(  1,   2),   S( -2,  -6),   S(  0,   4),
            S( -4,   7),   S(  0,  -3),   S(  9,  58),   S( -2, -19),   S( -1, -19),   S( -2, -11),   S(  1,  -2),   S( -3,  -2),
            S(  5,  22),   S(-17, -23),   S(  4,  13),   S( 16,  45),   S( -2, -13),   S( -4, -20),   S( -2,  -6),   S(  0,  -3),
            S(  3,  14),   S( 10,  36),   S(  3,   2),   S(  0,  10),   S( -6, -14),   S( -2,  -8),   S( -2, -11),   S(  2,   9),

            /* knights: bucket 13 */
            S( -4,  -9),   S( -1,  -3),   S( -1,  -4),   S( -2,  -7),   S( -3, -17),   S( -3, -18),   S( -5, -28),   S(  3,   9),
            S( -2,  -2),   S(  0,   1),   S(  0,  -7),   S( -3,  -1),   S( -9, -49),   S(  1,  -2),   S(  5,  13),   S(  3,   2),
            S( -9, -25),   S( -7, -29),   S(  9,  26),   S(  8,  19),   S( -3,  -4),   S(-14, -17),   S( -2,   7),   S( -7, -22),
            S(-19, -23),   S(  7,  21),   S(  4,  41),   S(-12, -24),   S( -1,   7),   S( 12,  23),   S(  6,  11),   S( -1,   5),
            S(  3,  15),   S(-13,  -5),   S(  3,  -8),   S(  4,  15),   S( 10,  25),   S(  5,  30),   S( -6, -25),   S( -1,   0),
            S(  1,  15),   S( 17,  31),   S(  2,  73),   S(  4,  58),   S(  6,  48),   S( -7, -27),   S(  8,  25),   S( -4,  -7),
            S(  0,   6),   S(  3,  19),   S(  5,   3),   S( -4,  12),   S( 18,  70),   S( -3, -13),   S(  0,  17),   S( -7, -11),
            S(  1,   5),   S(  1,  39),   S(  5,  29),   S(  5,  29),   S( -5,   5),   S(  5,  23),   S(  0,   1),   S(  0,  -1),

            /* knights: bucket 14 */
            S(  0,  -4),   S(-11, -49),   S( -4, -12),   S( -2,   8),   S(-13, -43),   S(  0,  -6),   S(  0,   0),   S(  4,  10),
            S(  1,   2),   S( -4, -24),   S(-13, -48),   S(  3, -14),   S( -2, -26),   S( -2, -26),   S( -1,  -3),   S(  0,  -7),
            S( -6, -17),   S(  0, -15),   S(-12, -35),   S(  5,   0),   S( -6,  -8),   S(  7,  15),   S(  2, -12),   S(  1,   8),
            S(  0,   3),   S( -7, -30),   S(-16, -61),   S( -3,  -5),   S(  8,  -8),   S( -5, -33),   S( -5, -13),   S(-13, -28),
            S(  2,   8),   S(  2, -13),   S( 10,  46),   S(  1, -12),   S(-14, -13),   S(  5,  -7),   S( 14,  28),   S( -7, -33),
            S( -3, -11),   S(  6,  17),   S( -4,   3),   S( 14,  31),   S( 15,  53),   S(  5,  42),   S(  0,   7),   S(  2,  23),
            S(  1,   9),   S( -2,  -8),   S(  7,  21),   S( -1,   2),   S(-13,  11),   S( -1,   6),   S(  3,  30),   S(  0,   5),
            S(  0,  -4),   S(  1,   2),   S( -4, -13),   S(  4,  18),   S(  5,  18),   S( -3,  -2),   S(  3,  25),   S(  0,   4),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -5, -38),   S( -3, -30),   S( -6, -18),   S( -2,  -4),   S( -2, -15),   S(  0,  -7),   S(  1,   4),
            S( -3, -18),   S( -2, -12),   S(  0,  -2),   S( -1,  18),   S( -4, -23),   S(  2,   2),   S( -3, -14),   S(  0,   6),
            S( -8, -29),   S( -9,  -4),   S(  5,   0),   S( -7, -29),   S( -9, -14),   S(  3, -11),   S( -4, -11),   S( -2, -11),
            S(-11, -31),   S( -8, -37),   S( -7, -47),   S( -1,   0),   S(  8,  17),   S(  7,  26),   S(  1,  14),   S(  0,  12),
            S( -3,  -8),   S( -4, -11),   S(  7,   3),   S( -6, -19),   S(  7,  28),   S( 11,  27),   S(-10, -13),   S(  1,   2),
            S(  1,   2),   S(  2,   0),   S( -5, -12),   S(  4,  23),   S( -4,  -9),   S(  8,  56),   S( -2, -16),   S( -9, -21),
            S( -5, -17),   S( -1,   2),   S( -2, -13),   S( -3,  -7),   S( -1,   6),   S(  3,  22),   S( -4, -16),   S(  3,  20),
            S(  0,   0),   S( -2,  -4),   S(  3,   6),   S( -4,  -2),   S(  1,  11),   S( -3, -19),   S(  5,  30),   S(  0,  -2),

            /* bishops: bucket 0 */
            S( 49,  38),   S(  4,   5),   S( 62,  43),   S(  6,  33),   S(-14,  20),   S(  2,   0),   S(  3,  -8),   S(  4, -68),
            S( 32,   8),   S( 86,  36),   S( 38,  22),   S(  9,  23),   S(-10,  41),   S(-14,  49),   S(-31,  15),   S(  0, -36),
            S( 46,   3),   S( 38,  23),   S( 30,  35),   S(  6,  49),   S( 17,  35),   S(-19,  49),   S(-13,  11),   S(  9, -33),
            S( 16,  20),   S( 65,  27),   S( 29,  14),   S( 26,  44),   S( -1,  48),   S( 30,  25),   S(-16,  29),   S( -5,  12),
            S(  4,  21),   S( 34,  -4),   S(-33,  70),   S( 38,  36),   S( 25,  31),   S(-16,  32),   S( 11,  10),   S(-46,  37),
            S(-50,  68),   S(-17,  70),   S(  0,  56),   S( 50,  26),   S( 36,  43),   S( -2,  20),   S(-34,  52),   S( -6,  64),
            S(-34,  82),   S(  3,  36),   S( 24,  35),   S(-26,  62),   S(-33,  30),   S( 40,  22),   S( 32,   7),   S(-43,  -1),
            S(-53,   4),   S(  0,  51),   S(-25,  29),   S(  6,  28),   S( 21,  24),   S( 26,  53),   S(-10,  34),   S(-40,  28),

            /* bishops: bucket 1 */
            S( 39,  93),   S(  7,  43),   S( 22,  36),   S( 17,  20),   S(  2,  29),   S( -2,  47),   S(  2,  43),   S(-47,  36),
            S(-10,  37),   S( 53,  20),   S( 42,  15),   S( 37,  36),   S(  8,  20),   S( 19,  18),   S(-28,  39),   S( 28,  13),
            S( 36,  31),   S(  9,  32),   S( 47,  31),   S( 10,  34),   S( 24,  30),   S(-11,  36),   S( 12,  19),   S(  2, -14),
            S( 56,  12),   S( 19,  33),   S(  3,  32),   S( 36,  38),   S(-19,  62),   S( 22,  27),   S( -3,  48),   S( 26,  20),
            S( 32,  29),   S( -1,  50),   S( 20,  28),   S(-20,  65),   S( 20,  38),   S(-36,  57),   S( 26,   3),   S(-18,  28),
            S(-18,  47),   S( 29,  32),   S( 31,  50),   S( 57,  18),   S( -6,  61),   S(  8,  45),   S(  6,  40),   S( 39,  10),
            S( 24,  41),   S(-20,  48),   S(  4,  49),   S( 11,  56),   S( 22,  36),   S(-16,  39),   S( -7,  53),   S(-67,  52),
            S( 27,  43),   S(-47,  31),   S(-11,  28),   S(-12,  20),   S(-15,  39),   S(-32,  30),   S(  3,  51),   S(-60, 105),

            /* bishops: bucket 2 */
            S(  8,  44),   S( -2,  56),   S( -1,  50),   S(-25,  74),   S(  2,  34),   S( -7,  35),   S(-26,   1),   S(-76,  81),
            S(  2,  49),   S( 13,  41),   S( 17,  42),   S(  2,  44),   S(  4,  51),   S( 24,  17),   S( 11,  18),   S( -3, -24),
            S( -3,  26),   S( -7,  45),   S( 10,  56),   S( -5,  67),   S(  0,  50),   S(  6,  57),   S(  0,  44),   S(-15,  17),
            S(  5,  44),   S(-22,  58),   S(-15,  63),   S( -3,  62),   S(  4,  64),   S( -1,  57),   S(  8,  49),   S( -3,  17),
            S(-17,  45),   S( -8,  47),   S(-21,  49),   S(-20,  60),   S(-24,  72),   S(-12,  75),   S( -5,  46),   S(-23,  31),
            S(  4,  35),   S(-27,  50),   S(-13,  59),   S(-21,  62),   S( -5,  47),   S(-43,  73),   S( -9,  72),   S(  4,  58),
            S(-31,  57),   S(-22,  51),   S(-42,  76),   S(-11,  47),   S(-65,  86),   S(-27,  46),   S(-44,  42),   S(-49,  56),
            S(-77, 107),   S(-69,  85),   S(-52,  68),   S(-71,  65),   S(-83,  67),   S(-84,  67),   S( -3,  51),   S(-76,  69),

            /* bishops: bucket 3 */
            S(-20,  59),   S( -4,  58),   S(  4,  43),   S(-10,  56),   S(-11,  43),   S( 43,  23),   S( 28,   6),   S( 32, -41),
            S( -9,  59),   S( -1,  53),   S( 11,  35),   S( -8,  63),   S(  4,  52),   S( 11,  47),   S( 42,  49),   S( 18,  11),
            S( 12,  25),   S(-12,  62),   S( -3,  70),   S(  3,  66),   S( -2,  75),   S(  1,  74),   S( 16,  45),   S( 19,  41),
            S(  8,  42),   S(-14,  60),   S(-10,  83),   S(  1,  77),   S( 11,  71),   S(  8,  56),   S(  6,  64),   S( -3,  22),
            S( -6,  43),   S(  7,  49),   S(  6,  41),   S( 13,  72),   S(  7,  61),   S( 14,  68),   S( -8,  51),   S(  1,  55),
            S(  2,  53),   S(  5,  44),   S(  3,  61),   S( 17,  45),   S( -8,  64),   S( 30,  48),   S( 30,  44),   S(-11,  86),
            S(-15,  45),   S(-14,  58),   S( 22,  48),   S( 16,  42),   S(  9,  26),   S(-31,  73),   S(-26,  81),   S(  7,  65),
            S(-49, 106),   S(-40,  47),   S( 24,  57),   S(-27,  54),   S(-24,  72),   S(-79,  96),   S( -4,  69),   S( 70,  37),

            /* bishops: bucket 4 */
            S( -4,  -7),   S(-41,  16),   S(-70,  10),   S(-57,  23),   S( -5,  18),   S(-48,   4),   S( -5, -12),   S( 27,  15),
            S(-30,   8),   S( 26,  19),   S( 19,  22),   S(-14,  14),   S( 12,   7),   S( 57, -17),   S( 13,  10),   S(  9,  14),
            S(-24,  13),   S(-28,  26),   S( 45,  23),   S(-14,  23),   S( 16,  19),   S( 45,  -1),   S(  2, -14),   S(-60,  31),
            S(-10,  22),   S(-24,  38),   S( 40,  22),   S( 43,  28),   S( -3,  32),   S( 26,   8),   S( 37,  49),   S(-10, -14),
            S( 42,  -5),   S( -2,  44),   S( -8,  52),   S( 27,  22),   S( -7,  18),   S(-32,  24),   S(-42,   5),   S(  4,  39),
            S( -2,   9),   S( 21,  19),   S( -6,  30),   S( 22,  24),   S( 23,  28),   S(  8,   9),   S(  4,   3),   S( -3, -15),
            S(-12,  22),   S( 60,  25),   S(  1,  12),   S( 22,  35),   S(-21,   7),   S(  0,  42),   S( -3,  15),   S( -6, -21),
            S( -6,   7),   S(-12,  -4),   S( -8,  -1),   S(-10,  -7),   S( -7,  13),   S( -5,  -3),   S(  0,  -3),   S(  0,  21),

            /* bishops: bucket 5 */
            S(-18,  36),   S( -2,  49),   S(-21,  33),   S(  0,  20),   S(-18,  25),   S( 10,  13),   S( -1,  48),   S( -3,  40),
            S(  0,  64),   S(-39,  58),   S(-16,  52),   S( 55,   4),   S( 20,  28),   S( -5,  25),   S(-16,  17),   S(-38,  47),
            S(-21,  44),   S( -9,  57),   S( 26,  29),   S( 32,  28),   S( 37,  19),   S(-19,  28),   S(-10,  31),   S( 27,   4),
            S( 25,  26),   S( 27,  30),   S(-41,  65),   S( 16,  28),   S( 32,  27),   S( 52,  24),   S( -4,  21),   S(-12,  48),
            S(  9,  54),   S( 49,  20),   S( 88,  20),   S(116,  19),   S( 47,  13),   S( 73,  12),   S( 39,  20),   S(-29,  27),
            S( 44,  29),   S( 30,  32),   S( 62,  41),   S( 26,  52),   S(-26,  46),   S( 15,  -1),   S(-11,  18),   S(-17,  40),
            S( 12,  58),   S(-33,  36),   S( 11,  34),   S( 24,  29),   S( 16,  33),   S(  0,  56),   S( -9,  22),   S( 10,  12),
            S(-22,  48),   S( 19,  35),   S( -1,  24),   S(  7,  39),   S( 19,  37),   S(  0,  51),   S( 14,  82),   S( -3,  37),

            /* bishops: bucket 6 */
            S(-58,  84),   S( -7,  44),   S( -9,  40),   S( -3,  30),   S(-17,  33),   S(-60,  50),   S(-32,  46),   S( 14,  72),
            S( 40,  40),   S(  7,  27),   S( -1,  39),   S( 22,  37),   S( 28,  18),   S( -7,  24),   S(-97,  65),   S( 27,  53),
            S( 42,  14),   S(-21,  40),   S( 29,  44),   S( 73,  19),   S( 78,  12),   S( 61,  20),   S( -3,  43),   S(-27,  42),
            S( 49,  39),   S(  5,  47),   S( 30,  37),   S( 78,  22),   S( 51,  36),   S( 42,  29),   S( 25,  51),   S(-32,  27),
            S(-55,  64),   S(  2,  40),   S( 72,   6),   S( 62,  21),   S(135,  32),   S(105,  20),   S( 32,  40),   S(  2,  42),
            S( 15,  17),   S(-18,  27),   S( 29,  31),   S( -5,  51),   S( 26,  48),   S( 60,  41),   S( 35,  30),   S(-24,  46),
            S(-33,  34),   S(  4,  22),   S( 15,  40),   S(-22,  39),   S( 21,  40),   S(  7,  44),   S( 15,  56),   S(-29,  42),
            S( -6,  55),   S(  6,  55),   S( 16,  46),   S(  0,  44),   S( -3,  27),   S( 18,  31),   S( -2,  33),   S( -4,  54),

            /* bishops: bucket 7 */
            S(-15,  22),   S( 19,  23),   S(-34,  -7),   S(-23,  10),   S(  5, -15),   S(-53,  -6),   S(-58, -38),   S(-44,  28),
            S(-66,  37),   S(-85,  41),   S(-27,  21),   S(  9,   2),   S(-16,  16),   S(-56,  30),   S( -7,  -9),   S(-33, -28),
            S(-53,  30),   S( 21, -17),   S( -3,  20),   S( 53,   9),   S(-26,  28),   S(  0,  21),   S(-17,  29),   S( -4,   2),
            S(-45,  27),   S( 42,  18),   S(113, -25),   S( 97,   2),   S( 83,  15),   S( 14,  17),   S( 54,  39),   S(  3,  20),
            S(-34,   8),   S(-29,   1),   S( 39,  -3),   S( 79,   0),   S( 74,  10),   S(110,  -3),   S( 28,  29),   S( 49,  11),
            S(-43,   8),   S(  3,  -3),   S( 44, -23),   S( 36,  -2),   S( 58,   1),   S(115,  13),   S( 74,  29),   S(-13,  47),
            S( -5, -18),   S(-15,   5),   S( -5,  12),   S(  0,  13),   S( -5,  -9),   S(  6,  12),   S( 44,  23),   S(  3,  18),
            S( 21,  16),   S(-17,  25),   S(-39,  17),   S(-15,   5),   S(  0,  12),   S( 13,   8),   S( 41,  19),   S( 24,  35),

            /* bishops: bucket 8 */
            S( -7, -46),   S(-22, -84),   S(-45, -34),   S( -6, -35),   S( 11,   1),   S(-16, -31),   S(  7,  -9),   S( -3,  12),
            S( -5, -14),   S(-32, -88),   S( -2, -55),   S( -8,   3),   S( 23, -16),   S( -8, -41),   S( -4, -41),   S(  6,  -7),
            S( -8, -20),   S(-20,  15),   S( -6, -20),   S( 31, -34),   S( 28, -22),   S(  5, -36),   S(  0, -37),   S(-39, -38),
            S( 12,  36),   S(  2, -34),   S(  5,  -6),   S( 24, -28),   S( 18,  -9),   S(  2, -28),   S( -6, -37),   S(  6,  -2),
            S( 23,  25),   S( 42,  66),   S( 20,  -4),   S( 46, -16),   S( 23, -43),   S( 11, -24),   S( -5, -41),   S( -4, -51),
            S(-12,   4),   S( 13,  12),   S( 19,   3),   S( -4, -11),   S( 23,  -4),   S( 12, -22),   S( -8, -63),   S(-27, -75),
            S( -6, -17),   S( 24,  15),   S(  9, -36),   S( -3, -21),   S( -1, -15),   S( -3, -37),   S( -6, -37),   S( -9, -18),
            S(  0,  -4),   S( -2, -54),   S(  0, -14),   S(  3, -18),   S( -8, -41),   S( -4, -44),   S( -5, -39),   S(  0, -10),

            /* bishops: bucket 9 */
            S( -4, -33),   S( 11, -39),   S(-20,  -3),   S(  1, -27),   S(-18, -61),   S(-31, -47),   S( -8,  -6),   S(  9,  49),
            S( -2, -26),   S( -2, -20),   S(-17, -12),   S(  6, -29),   S( -4, -23),   S( -6, -30),   S( -2, -14),   S( 12,  25),
            S(  5,  -6),   S( 20,   2),   S(  9, -38),   S( 29, -22),   S( 40, -28),   S( 25, -34),   S(-12, -35),   S(  5, -14),
            S(-12, -18),   S( 17,  10),   S( 19, -20),   S( 63, -21),   S( 46, -21),   S( 27, -18),   S( 15, -44),   S(-22, -58),
            S( 19,  -6),   S( 16, -11),   S( 34,  -9),   S( 54,  -4),   S( 19, -30),   S( 21, -24),   S(  4, -28),   S( -6, -14),
            S( -3, -24),   S( 47,  17),   S( 31,  34),   S( 45,  18),   S( 33, -20),   S( 27, -37),   S(  7, -63),   S(-18, -42),
            S(  6, -11),   S( 22,  -9),   S(  4, -22),   S( 12, -18),   S( 23,  -1),   S(  0, -58),   S(  2, -15),   S(-12, -53),
            S( -6, -23),   S( -1, -11),   S( -2, -56),   S( -8, -57),   S(-18, -66),   S(  7,  -2),   S(  1, -21),   S( -9, -47),

            /* bishops: bucket 10 */
            S( -7,  12),   S(  3, -33),   S(-25, -14),   S( -4, -25),   S(  3,  -6),   S(-25, -33),   S(  3, -36),   S( -8, -52),
            S( 14, -12),   S(-13, -20),   S( 18, -47),   S(  0, -52),   S(-24, -44),   S(  9, -33),   S( -9, -48),   S(-15, -24),
            S( 12, -25),   S( 13, -37),   S( 11, -52),   S( 27, -55),   S( 45, -61),   S(  3, -26),   S(-22,  -9),   S( -3,  12),
            S(-20, -72),   S( 11, -42),   S( 24, -60),   S( 41, -35),   S( 71, -41),   S( 24,  -5),   S(-13, -30),   S( 24,  23),
            S(  0,  -8),   S( 38, -45),   S( 46, -47),   S( 76, -30),   S( 42, -14),   S( 51,   8),   S( 23,   6),   S(-10, -69),
            S( -9, -46),   S( 25, -54),   S( 13, -61),   S( 30, -57),   S( 41, -29),   S( 34,  10),   S( 12, -22),   S(  7, -25),
            S(-14, -68),   S(  1, -61),   S( -4, -62),   S( 19, -23),   S(  9, -55),   S( 14, -14),   S( 23,  29),   S( 10,  12),
            S( -4, -56),   S( -9, -33),   S( -6,   4),   S( -3, -32),   S(  9, -18),   S( -2, -25),   S( -1, -24),   S(  5,   9),

            /* bishops: bucket 11 */
            S( -7,  21),   S(-38,  18),   S(-50, -75),   S(-16,  -3),   S(-12,  -3),   S(-15, -64),   S(-17, -61),   S(-28, -43),
            S( -1, -30),   S( -5, -30),   S(  2, -14),   S( -6, -52),   S(-44, -36),   S(-30, -68),   S(-18, -54),   S(-15, -38),
            S(  5, -48),   S( 15, -53),   S(  1, -28),   S( 34, -49),   S(  9, -30),   S(  9, -37),   S(-16,  -1),   S( -8, -29),
            S( -7, -52),   S(-10, -45),   S( 37, -68),   S( 37, -56),   S( 66, -51),   S( 35, -12),   S( 41,  -9),   S( 24,  52),
            S( -5, -49),   S( 11, -64),   S( 19, -44),   S( 52, -48),   S( 41, -20),   S( 58, -11),   S( 18,  26),   S(  8, -41),
            S(-24, -92),   S( 15, -66),   S(  9, -69),   S( 37, -42),   S( 15, -32),   S( 29,   3),   S( 11,  45),   S( -3,  -6),
            S( -5, -58),   S( -4, -78),   S(-10, -83),   S(  3, -50),   S(  6, -42),   S( 27, -17),   S( 18, -27),   S( 12,   6),
            S(-22, -96),   S(-15, -53),   S( -2, -43),   S(  4, -33),   S( 18, -34),   S(-17, -92),   S( -1, -40),   S(  7,  -1),

            /* bishops: bucket 12 */
            S(  0,  -5),   S( -6, -23),   S(-20, -57),   S( -5, -42),   S( -6, -32),   S(-10, -18),   S( -1,   4),   S( 11,  23),
            S( -4, -29),   S( -8, -35),   S( -6, -24),   S( -3, -17),   S(  2, -10),   S(  2,  13),   S( -1,   4),   S(  6,   7),
            S( -8, -32),   S(-14, -44),   S(  4, -13),   S( -8, -50),   S( -4,  -7),   S(  2, -27),   S(-15, -45),   S( -4, -24),
            S( -8, -33),   S(  6,  -4),   S( -2, -27),   S( -2, -17),   S(  6, -13),   S(  2, -11),   S( -3, -33),   S( -7, -24),
            S( -4,   1),   S( -5,  -3),   S( -1, -44),   S( -7, -17),   S(  9, -36),   S(-12, -52),   S(  4, -25),   S( -2,  -2),
            S(-11, -13),   S(  2,  27),   S( -7,  24),   S(-13, -55),   S(  4, -49),   S( -4, -26),   S(  1, -35),   S( -2, -10),
            S( -5, -16),   S( -1,   3),   S( -2,  15),   S( -8, -32),   S(  1, -13),   S(  8,  14),   S( -9, -32),   S( -2, -13),
            S( -1,  -8),   S( -8, -24),   S( -4, -39),   S(  4,   7),   S(  2, -10),   S( -1,  -9),   S(-14, -36),   S(  1,  12),

            /* bishops: bucket 13 */
            S( -5, -49),   S( -7, -38),   S(-13, -39),   S(-13, -80),   S(-12, -64),   S( -6, -22),   S(  0, -11),   S(  0,  -7),
            S(  0,   5),   S(  4, -26),   S(  2, -51),   S( -7, -26),   S(  0, -38),   S( -1, -22),   S(  5,  -5),   S(  7,   0),
            S( -5, -30),   S(-10, -15),   S( 12, -17),   S(  7, -51),   S(  3, -54),   S(  6, -49),   S(-11, -28),   S(  5,  32),
            S(  3,  25),   S( -5, -46),   S( -4, -45),   S(-14, -71),   S(  8, -65),   S(  0, -23),   S(  0, -21),   S( -7, -30),
            S(  1,  11),   S( -6, -10),   S( -9, -71),   S( 24,  -7),   S(  7, -19),   S( 10, -39),   S(  0, -36),   S( -5, -44),
            S(  4,   5),   S( -3, -11),   S(-13, -10),   S( 15,  -4),   S(  6,   1),   S( 15, -33),   S(  7, -51),   S( -3, -23),
            S( -8, -45),   S(  2,  -6),   S(  4,  -3),   S( -9,  -7),   S(  3, -32),   S(  6,  -3),   S(-16, -89),   S( -1, -20),
            S( -1, -15),   S( -2, -25),   S( -1, -19),   S(  0, -24),   S( -4, -23),   S(-10, -53),   S( -3, -18),   S( -6, -39),

            /* bishops: bucket 14 */
            S(  1, -25),   S(  0,  -6),   S(-19, -74),   S(-18, -68),   S( -9, -65),   S( -3, -27),   S( -8, -64),   S(-12, -52),
            S(-10, -29),   S(  0, -21),   S(  0, -41),   S(-17, -84),   S( -8, -56),   S(  0, -63),   S(-12, -54),   S( -2, -32),
            S(  0, -12),   S( -7, -28),   S(-11, -79),   S(  0, -68),   S( -7, -55),   S( -9, -60),   S( -7, -50),   S( -1, -11),
            S(  1, -10),   S( -3, -38),   S( -5, -39),   S(  1, -42),   S(  7, -62),   S(  9, -35),   S(-14, -81),   S( -3, -20),
            S( -9, -42),   S( -9, -53),   S(  0, -33),   S(  5, -67),   S(  6, -65),   S(-11, -68),   S(  6, -15),   S(  3, -14),
            S(  0, -38),   S( -3, -44),   S(-11, -62),   S( -4, -53),   S(  8,  -1),   S(-13,  23),   S(  4, -45),   S( -8, -55),
            S( -7, -50),   S(  6, -30),   S( -6, -53),   S(  9,   4),   S(-11, -34),   S(-11, -25),   S(  3,  10),   S( -5, -26),
            S( -2, -24),   S(  0, -12),   S( -4, -19),   S(-10, -39),   S(  0,  -1),   S( -1,   0),   S(  0,  12),   S(  4,   1),

            /* bishops: bucket 15 */
            S(  8,  43),   S(  3,  26),   S(-21, -68),   S(  4, -22),   S( -4, -25),   S(-10, -14),   S( -3, -25),   S(  0,  -2),
            S(  6,  10),   S(  3,   3),   S(  8,   0),   S( -6, -52),   S(-11, -56),   S( -4, -33),   S( -6, -18),   S( -4, -13),
            S( -7, -27),   S(  1,   0),   S(-11, -52),   S( -2, -16),   S( -9, -60),   S( -6, -22),   S(  0, -29),   S(  6,   9),
            S( -6, -16),   S(-10, -20),   S(  3, -13),   S(-18, -71),   S(  3, -29),   S(-11, -57),   S( -4, -15),   S( -7, -22),
            S( -5, -37),   S(-13, -43),   S( -2, -34),   S(-22, -59),   S( -9, -56),   S(-10, -45),   S( 15,   1),   S(  0, -15),
            S( -6, -33),   S(-13, -75),   S(-11, -50),   S(-16, -59),   S( -4, -55),   S( -8, -25),   S( 18,   5),   S( -5, -12),
            S( -2, -20),   S(  2, -21),   S( -3, -28),   S( -4, -34),   S(-13, -43),   S(  5,   2),   S( -8, -33),   S(  6,  25),
            S( -4, -28),   S( -6, -27),   S( -6, -39),   S( -5, -47),   S(-10, -49),   S(-17, -68),   S(-12, -38),   S( -1,  -6),

            /* rooks: bucket 0 */
            S(-27,  -4),   S( 17,  -5),   S( 24, -19),   S( 22,  -8),   S( 17, -13),   S( 13, -14),   S( 11,  -6),   S( 18,  -9),
            S( 11, -81),   S( 41, -15),   S( 27,   0),   S( -1,   7),   S( 44, -13),   S( 18, -19),   S(-10,   5),   S(-53,  31),
            S(-19,   0),   S( 18,  14),   S( 35, -17),   S( 21,   8),   S( -5,  29),   S( 15, -10),   S(-32,   6),   S(-40,   8),
            S( 46, -29),   S( 69,  -8),   S( 73,  -9),   S( 73, -22),   S( 62, -18),   S(  3,   8),   S( -1,   9),   S(-25,  19),
            S( 62, -33),   S( 87, -12),   S( 83,   6),   S( 62,   3),   S( 55,  -8),   S( 11,  24),   S( 13,  -1),   S(  3,  -3),
            S( 83, -57),   S( 90, -36),   S( 58,   4),   S( 30,  14),   S( 72,   1),   S( -1,  18),   S( 54, -14),   S(-24,  20),
            S( 37, -15),   S( 64,  -5),   S( 53,   7),   S( 58,  13),   S( 34,  11),   S( 47,  -5),   S( 39,  11),   S( -7,  27),
            S( 16,  13),   S(  3,  54),   S( 25,  25),   S( 33,  13),   S(  6,  14),   S( 36, -20),   S( 53,  17),   S( 42,  -5),

            /* rooks: bucket 1 */
            S(-73,  27),   S(-25,  -9),   S(-28, -23),   S(-24,  -2),   S( -8, -18),   S(-17, -12),   S(-21, -12),   S(-25,  -3),
            S(-62,  22),   S(-55,  18),   S(-14,   6),   S( -8, -19),   S(-35,   8),   S(-32,  -1),   S(-39, -10),   S(-70,  13),
            S( 10,  -9),   S(-17,  25),   S(-17,  25),   S(-38,  42),   S(-46,  20),   S( -5,   0),   S(-19,  -5),   S(-41,  13),
            S(-35,  39),   S(-65,  50),   S(-22,  51),   S(-19,  33),   S(-22,  46),   S(-54,  54),   S(-18,  30),   S(-42,  27),
            S( 75,  -2),   S( 53,  27),   S(  5,  26),   S(-34,  54),   S(  7,  31),   S( 46,  -5),   S( -1,  28),   S(-53,  36),
            S( 48,  26),   S(-13,  51),   S( 40,  24),   S( -6,  32),   S( 28,  20),   S(-11,  50),   S( 22,  21),   S( -4,  18),
            S(  8,  26),   S( 30,  33),   S(  9,  47),   S(-67,  86),   S(-26,  49),   S( 16,  49),   S(-21,  26),   S(  4,  21),
            S( 28,  30),   S( 19,  33),   S(-17,  44),   S(-72,  71),   S(-17,  38),   S( 28,   8),   S( -1,  24),   S( 30,  11),

            /* rooks: bucket 2 */
            S(-68,  45),   S(-42,  39),   S(-36,  28),   S(-44,  30),   S(-44,  12),   S(-48,  14),   S(-33,  -8),   S(-51,  27),
            S(-78,  42),   S(-75,  59),   S(-39,  37),   S(-52,  31),   S(-38,  26),   S(-56,  20),   S(-53,  17),   S(-66,  19),
            S(-74,  60),   S(-57,  60),   S(-47,  65),   S(-47,  36),   S(-47,  47),   S(-55,  58),   S(-18,  16),   S(-37,  29),
            S(-72,  80),   S(-52,  71),   S(-38,  79),   S(-21,  59),   S(-27,  59),   S(-27,  71),   S(-29,  58),   S(-21,  37),
            S(-21,  62),   S(-63,  86),   S(-51,  81),   S(-18,  54),   S( 40,  39),   S(  0,  65),   S(-36,  57),   S(-46,  63),
            S( -8,  54),   S(-32,  68),   S(-11,  57),   S(  7,  49),   S(  8,  49),   S( 52,  28),   S( 33,  15),   S(-13,  43),
            S(-55,  66),   S(-55,  87),   S(-17,  65),   S( 15,  60),   S( 24,  54),   S( 25,  42),   S(-43,  84),   S( -9,  57),
            S(-29,  85),   S( -2,  51),   S(-81,  87),   S(-21,  60),   S(-42,  66),   S(-36,  71),   S(-11,  61),   S( 11,  42),

            /* rooks: bucket 3 */
            S(  7,  62),   S( 13,  67),   S( 22,  56),   S( 30,  55),   S( 23,  56),   S( 14,  57),   S( 33,  59),   S(  2,  32),
            S(-30,  79),   S( -8,  65),   S(  8,  57),   S( 23,  49),   S( 27,  55),   S( 29,  44),   S( 53,  10),   S( 36, -52),
            S(-38,  83),   S(-12,  74),   S(  9,  71),   S( 21,  54),   S( 32,  50),   S( 26,  56),   S( 21,  55),   S( -1,  45),
            S(-18,  90),   S( -8,  87),   S( 32,  74),   S( 47,  58),   S( 41,  65),   S( 14,  90),   S( 62,  57),   S( 18,  70),
            S(  7,  86),   S( 26,  75),   S( 25,  69),   S( 56,  59),   S( 70,  54),   S( 51,  65),   S( 90,  43),   S( 63,  43),
            S( 12,  87),   S( 33,  71),   S( 38,  69),   S( 53,  56),   S( 51,  45),   S( 53,  48),   S( 84,  32),   S(101,  21),
            S( -4,  89),   S( 14, 100),   S( 19,  88),   S( 59,  78),   S( 36,  83),   S( 56,  54),   S( 42,  76),   S(144,  12),
            S(-25, 137),   S( 25,  90),   S( 34,  75),   S( 64,  65),   S( 84,  50),   S( 87,  43),   S(106,  43),   S(111,  22),

            /* rooks: bucket 4 */
            S(-78,  26),   S( -8,  -1),   S(-59,  -5),   S(  6, -12),   S(-34, -19),   S(  1, -51),   S(-15, -19),   S(-11, -35),
            S(-14, -30),   S(-12, -14),   S(-59,  30),   S(-12,   8),   S(-56,  -6),   S(-19, -25),   S(-20, -36),   S(-60,  -5),
            S( 16,  16),   S(-16, -26),   S(-16,  -6),   S( 12,  -6),   S(-21,  -2),   S(-12, -14),   S(  2,  -4),   S(-80, -12),
            S(-61, -17),   S( 17, -11),   S(-55,  21),   S( 15,  17),   S( 34,  15),   S( 14,  -6),   S(-36,  16),   S(-24,  30),
            S(-40,  -7),   S(-27,  52),   S(  7, -11),   S( 49,  18),   S( 15,  15),   S(  5,  18),   S(  7,  19),   S( 21,   2),
            S( 29, -27),   S( 38,  15),   S( 49,  10),   S( 20,  12),   S( 36,  -9),   S( 21,  26),   S(  8,   9),   S( -2,  26),
            S(-26,   6),   S(  4,  26),   S(  9,   3),   S( 39,  16),   S( 40,  -5),   S( 17,   2),   S( 38,  17),   S( 27,  27),
            S( 20, -60),   S( 48,  33),   S( 31,  19),   S( 12,  15),   S( 25,  -6),   S(  6,  10),   S( 14,  -7),   S(  8,  -3),

            /* rooks: bucket 5 */
            S(-51,  51),   S(-38,  48),   S(-78,  54),   S(-20,  14),   S(-30,  22),   S(-38,  47),   S( -2,  27),   S(-62,  51),
            S(-32,  31),   S(-52,  40),   S(-89,  72),   S(-70,  38),   S(-47,  30),   S( -4,   6),   S( -2,  23),   S(-54,  17),
            S(-10,  39),   S(-63,  60),   S(-47,  67),   S(-57,  53),   S(-79,  42),   S(-37,  54),   S(  8,  32),   S( -9,  23),
            S(-43,  74),   S(-33,  60),   S(-13,  61),   S(-32,  63),   S(-18,  52),   S(-13,  70),   S(  8,  54),   S(-25,  39),
            S( 37,  44),   S( 15,  51),   S( 28,  49),   S(  7,  77),   S( 38,  44),   S(  6,  70),   S( 70,  49),   S(  7,  50),
            S( 58,  57),   S( 45,  57),   S( 48,  61),   S(  5,  88),   S( 51,  47),   S( 85,  45),   S( 54,  53),   S( 42,  44),
            S( 22,  53),   S( 32,  63),   S( 46,  63),   S( 40,  63),   S( 41,  52),   S( 61,  52),   S(101,  36),   S( 76,  51),
            S( 73,  40),   S( 66,  19),   S( 45,  50),   S( 33,  52),   S( 59,  31),   S( 74,  34),   S( 31,  36),   S( 12,  51),

            /* rooks: bucket 6 */
            S(-68,  54),   S(-43,  51),   S(-10,  30),   S(-19,  26),   S(-29,  13),   S(-93,  60),   S(-66,  75),   S(-24,  48),
            S(-29,  32),   S( -1,  38),   S( -4,  23),   S(-45,  38),   S(-53,  50),   S(-47,  55),   S(-64,  56),   S(-13,  40),
            S(-60,  57),   S(-65,  66),   S(  4,  46),   S(-50,  64),   S(-52,  59),   S(-38,  74),   S(-23,  66),   S(-16,  36),
            S(-71,  84),   S(  8,  60),   S(-38,  76),   S(-19,  60),   S(-23,  66),   S(-12,  71),   S(-60,  81),   S(-27,  56),
            S(-20,  79),   S( 35,  65),   S( 72,  45),   S( 16,  50),   S(-33,  94),   S( 27,  76),   S( 18,  61),   S(  7,  54),
            S(-20,  83),   S( 53,  61),   S( 94,  37),   S( 36,  42),   S( 33,  63),   S( 38,  79),   S( 36,  69),   S( 69,  53),
            S( 55,  62),   S( 62,  57),   S( 90,  40),   S( 68,  46),   S( 97,  36),   S( 57,  64),   S( 92,  41),   S( 65,  54),
            S( 63,  66),   S( 63,  50),   S( 39,  54),   S( 16,  60),   S( 59,  50),   S( 70,  53),   S( 59,  56),   S( 56,  46),

            /* rooks: bucket 7 */
            S(-72,   4),   S(-41,  -2),   S(-29, -14),   S(-21,  -4),   S(-26,  -6),   S(-45,  18),   S(-61,  27),   S( -2, -14),
            S(-93,  43),   S(-40,  10),   S(-33,   1),   S(-39,   9),   S( -1,  12),   S( -4,  15),   S(-10,   5),   S(-32,  -5),
            S(-105,  57),  S(-43,  17),   S(-44,  39),   S(-18,   9),   S(  3,  10),   S(-22,  10),   S(-20,   7),   S( -3,  15),
            S(-81,  45),   S( -6,  28),   S( 15,  15),   S( 14,  20),   S( 31,  12),   S( 23,   9),   S( 28,   8),   S(-16,  -3),
            S(-34,  43),   S(-29,  35),   S( 41,  -6),   S( 57,  -8),   S( 71,   4),   S( 81,  16),   S( 49,  34),   S( 71, -15),
            S(-34,  42),   S( 21,  11),   S( 89, -18),   S( 86,  -7),   S( 73,  -5),   S( 85,  13),   S( 58,  28),   S( 41,  -3),
            S(  5,  32),   S( 12,  26),   S( 86,   3),   S( 81,  11),   S( 78,   5),   S(109,   3),   S( 57,  30),   S( 53,  -4),
            S( 22,  57),   S(-28,  41),   S( 62,   9),   S( 94, -19),   S( 47,   8),   S( 14,  12),   S( 76,   1),   S( 41,  -7),

            /* rooks: bucket 8 */
            S(-41, -48),   S(-43, -14),   S(-11,  10),   S(-24,  -6),   S(-37, -30),   S(-35, -34),   S(-20, -35),   S(-34,  29),
            S( -8,  11),   S(-27, -26),   S(  1,  -1),   S(-18, -35),   S(-18, -12),   S(-17, -33),   S( -9, -23),   S(-29, -84),
            S(  7,   1),   S( -2, -22),   S( -7,  11),   S( -6,   4),   S(-15, -27),   S(-22, -36),   S(  7,  27),   S(-11, -18),
            S( -3, -11),   S( -5,  26),   S(-20,  -8),   S(  2,  10),   S(  0,   6),   S(-21, -17),   S(  7,  -7),   S(-21,  -9),
            S(-19, -16),   S(  1,  29),   S( -8,  46),   S( -6,  12),   S( -5, -10),   S(  2, -18),   S(  0,  11),   S(-20, -59),
            S(  4,  39),   S(-22,  -7),   S( 17,  36),   S( 11,   0),   S(  9,  22),   S( -5, -38),   S(  1,  -6),   S( 10,  27),
            S(  0,   1),   S(-11,  11),   S( 16,  13),   S( 17, -22),   S( 13,   0),   S( -8, -15),   S( 15,   0),   S(  4,  -6),
            S(-26, -109),  S( -3, -27),   S( 14,  -2),   S(-10, -33),   S( -5, -24),   S(  1,  -9),   S(  6,  -7),   S( -2,  16),

            /* rooks: bucket 9 */
            S(-70,  -4),   S(-13, -32),   S(-36, -19),   S(-50,  -7),   S(-18,  18),   S( -5, -25),   S( -3, -42),   S(-60, -32),
            S(  7, -15),   S( -7, -29),   S(-28, -33),   S( -4, -25),   S(-45, -29),   S(  4, -21),   S( -2, -21),   S(-29, -49),
            S(-11, -25),   S(-11, -35),   S( -7,   0),   S(-38, -12),   S(-44, -32),   S( 12, -17),   S( 15,  -5),   S(-13, -33),
            S(-11, -29),   S(  0,   4),   S(-16,  -5),   S(-22,  -2),   S(-21, -14),   S(  3,  -6),   S(  4,  -3),   S(-17, -23),
            S( -7,   6),   S(-12,   4),   S(  2,   2),   S( 21,  45),   S( 22,  27),   S( 28,  -7),   S( -1,   6),   S(-14, -48),
            S(  8,  23),   S(-27,  -8),   S(-11,   5),   S(-15,  14),   S( 12, -21),   S( 26,   8),   S( 12,  -8),   S(  9, -36),
            S( 31,   0),   S( 39,  -8),   S( 46,  19),   S( 40,  -4),   S( 15, -27),   S( 40, -18),   S( 27, -18),   S( 47,  12),
            S( 40, -84),   S( 26, -49),   S( 18,  -6),   S(  0,  10),   S(  7,  -5),   S( 13, -13),   S( 10, -15),   S( 10,  -1),

            /* rooks: bucket 10 */
            S(-60, -82),   S(-27, -42),   S(-40, -58),   S(-37, -14),   S(-53, -51),   S(-54, -35),   S(-12, -51),   S(-28,  -4),
            S(-14, -29),   S(-10, -51),   S(-15, -48),   S(-24, -21),   S(-15, -27),   S(-28,  -6),   S( 11,  -8),   S( -2, -27),
            S(-41, -33),   S(-23, -30),   S(-29, -48),   S(-27, -13),   S(-27,  -4),   S(-12, -30),   S( 23, -11),   S( 17, -19),
            S(-27, -19),   S(-11, -29),   S(-26, -13),   S(-16, -25),   S(  8,   3),   S(-18, -36),   S( -6,  28),   S(-12, -41),
            S(-12, -16),   S(  9,  -9),   S(  6,  -3),   S( 12, -15),   S(-16,  -9),   S(  6,   0),   S( 19,  -9),   S(  5,  -4),
            S( 29,  -6),   S( 32,   3),   S(  0,   7),   S( 18,   2),   S(-12, -10),   S( -1, -16),   S( 17,  -4),   S(  6,  -6),
            S( 82, -14),   S( 74, -25),   S( 68, -19),   S( 45, -25),   S( 24,  -9),   S( 21,  -6),   S( 30, -39),   S( 40, -23),
            S( 44,  -1),   S( 14,  -3),   S( 18, -19),   S(  7, -17),   S( 31, -22),   S( 23,  -9),   S( 18, -52),   S( -5, -45),

            /* rooks: bucket 11 */
            S(-90, -30),   S(-21, -11),   S(-30, -19),   S(-39, -61),   S(-17, -21),   S(-37,   8),   S(-40, -25),   S(-47,  -3),
            S(-31,   5),   S(-23, -37),   S(-30, -32),   S(-47,  -4),   S(-44, -31),   S(-30, -11),   S(-16, -27),   S(  3,   5),
            S(-34, -22),   S(  3, -29),   S( -8,   0),   S(-34, -15),   S( -6, -13),   S(-17,  15),   S(-26, -38),   S(-21, -25),
            S(-36,  17),   S( -9, -22),   S(-10,   9),   S( -7,  36),   S(-21,  -5),   S(-17,  24),   S(  7, -17),   S(-12, -12),
            S(-20,   9),   S( 25, -21),   S( 32,  -4),   S( 15,   4),   S( 15, -19),   S( 12,  -6),   S(  3, -16),   S(-17, -18),
            S( 24,  35),   S( 26,  27),   S(  2,  -7),   S( 53,  12),   S( 22,   2),   S( 28, -11),   S( -4,  31),   S( 34,  19),
            S( 24,  29),   S( 36,   3),   S( 46,  -8),   S( 42,  -9),   S( 29,   8),   S( 20,  11),   S( 18,  20),   S( 51,  10),
            S( 27,  21),   S( 11,  14),   S( 23,  -2),   S( 19,  -3),   S(-19,  -4),   S(  1, -21),   S( 10,  35),   S( 20,  -7),

            /* rooks: bucket 12 */
            S( -8, -69),   S(-20, -68),   S(-27,  -8),   S( -5,  -5),   S( -1, -19),   S( -5, -18),   S(-27, -76),   S(-31, -47),
            S( 13,  21),   S(  0,  -6),   S(-19,  10),   S(  5,  24),   S(-10, -13),   S(-19, -33),   S( -1, -27),   S( -6, -36),
            S( 10,   4),   S(  3, -14),   S( -7, -22),   S( -4,   6),   S(-12, -26),   S(  7,   2),   S( -1, -13),   S( -3, -10),
            S(-21, -36),   S(  1, -12),   S( 11,  14),   S(  3, -15),   S(  1,   1),   S(-17, -48),   S(-19, -59),   S( -7, -44),
            S(-10, -10),   S( -6,  -7),   S( 10,  -3),   S( -2, -15),   S( -6, -13),   S(  8,  -6),   S( -9, -31),   S( -9, -43),
            S(-24, -36),   S(  7, -15),   S( 19,  29),   S(  5, -19),   S( -1,  12),   S( -9, -45),   S(  4, -25),   S( -3,  -7),
            S(-12, -31),   S(  4,  15),   S( -5, -46),   S( 14, -10),   S(  1, -13),   S( -9, -61),   S( -8, -24),   S(  0,  -5),
            S(-11, -60),   S(  4,  16),   S(  2, -23),   S(  2, -13),   S(  3, -19),   S(-18, -35),   S(-12, -55),   S( 10,  25),

            /* rooks: bucket 13 */
            S(-18, -65),   S( -9, -20),   S( -8,  15),   S(  4,  46),   S(  0,   1),   S(-26, -47),   S(  1, -34),   S(-21, -28),
            S(  4,  -2),   S(-10, -15),   S(-11,  -4),   S(  7,  21),   S(-10, -25),   S( -3,  -4),   S(  5,  14),   S( -2, -12),
            S( -4, -35),   S(-10, -32),   S(-16, -30),   S( -5, -32),   S( -2,  11),   S( 10,   0),   S( -4, -10),   S( -9, -41),
            S(-10, -43),   S(  1,  15),   S(-16, -30),   S(  0,   8),   S(  8,  -3),   S(-19, -35),   S( 11,  -1),   S( -8, -37),
            S( -9, -34),   S(-14, -27),   S( 14,   7),   S( -4, -27),   S( -7,  10),   S(-12, -35),   S(-17, -51),   S( -7, -15),
            S(-22, -32),   S(  0,  -7),   S( -9,  -4),   S( -2, -39),   S(  4, -12),   S( 14,  27),   S(  0, -24),   S(  2,  -2),
            S(-10, -20),   S(  1,  -5),   S( 17,  48),   S( -7,   1),   S( -4, -35),   S( 15,  28),   S(  9,  -8),   S(-13, -28),
            S(-43, -155),  S(-17, -66),   S( -1,  -1),   S( -6, -11),   S(  0,  -3),   S( -7, -34),   S(-18, -72),   S( -8, -30),

            /* rooks: bucket 14 */
            S( -3,  10),   S(-17, -45),   S(  3, -19),   S(-27, -47),   S(  2,  35),   S(-13, -25),   S(  2,  -6),   S(-11, -15),
            S(-24, -46),   S(-16, -38),   S( 10,  25),   S(-19, -19),   S( -9,  -6),   S(  1, -14),   S(  3, -11),   S( -3, -11),
            S( -6, -12),   S( -5,  -9),   S( -2, -23),   S( -6, -13),   S(-14, -12),   S(-10, -12),   S(  1,  10),   S(  3,  -9),
            S( -4,  -8),   S(-21, -63),   S(  4,  -1),   S(-11, -27),   S( -1, -21),   S(  0,   4),   S( -9, -53),   S(  3,   9),
            S(  0, -47),   S(-13, -71),   S( -2, -40),   S(-23, -88),   S(  4, -45),   S(-15, -66),   S( -3, -43),   S( -2,  -8),
            S(-10, -48),   S( -5, -31),   S(  3, -38),   S(  2, -68),   S(  2, -49),   S(  0, -47),   S(  3, -13),   S( -6, -31),
            S(  4,  -8),   S( -7, -55),   S(  3, -31),   S( -1, -26),   S(  7, -31),   S( 13,  -6),   S(  4, -15),   S(  3, -14),
            S(-15, -43),   S(  1, -15),   S( -8, -62),   S( -1,  -4),   S(-17, -38),   S( -4,  26),   S(  9,  15),   S( -6, -16),

            /* rooks: bucket 15 */
            S(  0,  26),   S(-18, -49),   S( -9, -29),   S(-18, -48),   S(-10, -28),   S(-11, -12),   S(-14, -34),   S( -6, -22),
            S(-12, -25),   S(-15, -29),   S( -5,   6),   S(-16, -28),   S(-12, -25),   S( -3,   2),   S(-21, -50),   S(  5,  -3),
            S( -7, -26),   S( -4, -22),   S(  1,  -2),   S( -9, -52),   S( 28,  11),   S( -6,   5),   S( 10,  10),   S(-12, -30),
            S( -5, -38),   S(-12, -50),   S(  1,   9),   S(  0, -28),   S( -7, -32),   S(-11, -23),   S(  7,  14),   S(-19, -13),
            S( -1, -16),   S( -3, -25),   S(  7,  -7),   S(  0, -27),   S(  2, -29),   S( 14,   0),   S(  3,  -5),   S( -4,  11),
            S( -7, -33),   S(  2, -18),   S( -1, -44),   S(  6, -38),   S(  1, -32),   S( 23,  -5),   S(  7, -18),   S( -1,  -1),
            S(  1,  -1),   S( 12, -14),   S( 10, -14),   S( -9, -71),   S( -7, -48),   S(  3,   7),   S( 13,  10),   S( -3, -12),
            S( -7, -19),   S( -9, -34),   S(  1, -17),   S( -5, -46),   S(  4, -24),   S(  2, -14),   S( -8, -39),   S( -1,  -4),

            /* queens: bucket 0 */
            S(-46, -12),   S(-45, -63),   S( 61, -69),   S( 55, -67),   S( 40, -36),   S( 38, -36),   S( 50,  23),   S(  6,  -2),
            S(-22, -16),   S( 53, -76),   S( 50, -16),   S( 36, -16),   S( 28,  18),   S( 26,  35),   S( 33,  24),   S( 44, -10),
            S( 27,   6),   S( 41,  16),   S( 30,  32),   S( 20,  44),   S( 14,  11),   S( 16,  44),   S(  0,  57),   S( 29,  20),
            S( 11,  33),   S(  9,  28),   S(  3,  68),   S( -2,  78),   S( 23,  24),   S( 18,  44),   S( 27,  42),   S( 10,  35),
            S( 39,   8),   S( 19,  31),   S(  9,  38),   S( 33,  57),   S(-12,  42),   S(  8,  -8),   S( 10,  23),   S( 23,  30),
            S( 34,  40),   S( 37,  60),   S( 27,  43),   S( 53,   5),   S( 62, -21),   S( 20,  30),   S( 20,  28),   S( 26, -43),
            S( 64,  23),   S( 40,  54),   S( 36,  34),   S( 27,  40),   S( -6,  12),   S(-16,   9),   S( 22,  38),   S( 24,  23),
            S( 22,   7),   S( 36,  37),   S( 50,  20),   S( 47,  49),   S( 21,  14),   S(  9,  23),   S( 58,  30),   S( 27,   7),

            /* queens: bucket 1 */
            S( 10, -26),   S(-50, -50),   S(-44, -28),   S(-16, -80),   S(-10, -23),   S(-19, -57),   S( 21, -49),   S(  6,   0),
            S(-19, -57),   S(-10, -25),   S( 12, -40),   S( 10,  35),   S( -1,   9),   S( 20, -20),   S(  9, -51),   S(  1,   3),
            S(-41,  46),   S( -1,  -7),   S( 15,  -2),   S( -4,  27),   S( -3,  34),   S(-10,  33),   S( 17,  -5),   S(  8,  28),
            S( 14,  10),   S(-22,  52),   S(-10,  60),   S( 28,  33),   S(-10,  66),   S( 11,  29),   S(-14,  22),   S( 11,  -1),
            S(  5, -24),   S(-12,  13),   S(-26,  76),   S(-30,  90),   S(-10,  63),   S(-22,  38),   S(-19,   9),   S( -3,  28),
            S( 19,  39),   S(  8,  62),   S( 24,  43),   S(-28,  82),   S( -7,  77),   S(-19,  28),   S( 27,   9),   S(  4,  18),
            S(  0,  45),   S( -1,  67),   S(-20,  24),   S( 15,  90),   S(-41,  65),   S( 31,  25),   S( -1,  40),   S( -5,  30),
            S(-56,  -2),   S(-15,  20),   S( 43,  39),   S( 28,  21),   S(  3,  37),   S( 43,  46),   S( 34,  39),   S( -1,  28),

            /* queens: bucket 2 */
            S( 13,  11),   S( 22, -45),   S( 14, -56),   S( 15, -41),   S( -4,  -2),   S(-36, -37),   S(-22, -41),   S( 23,   7),
            S( 16, -15),   S( 10,  28),   S( 18,  -6),   S( 30, -23),   S( 23, -10),   S( 28, -47),   S( 21, -17),   S( 51, -60),
            S(  9,   7),   S( 10,   5),   S( 18,  30),   S( 15,  19),   S(  2,  65),   S( 19,  44),   S( 18,  -5),   S( 28,   5),
            S( 15,   5),   S( -6,  48),   S( 11,  29),   S( 17,  53),   S(-15,  86),   S(  3,  83),   S( 14,  27),   S(  7,  46),
            S(  3, -24),   S( -9,  49),   S(-34,  80),   S(-56, 141),   S(-37, 110),   S(-30,  98),   S(-29, 122),   S(-21, 115),
            S(  5,  45),   S(  6,  45),   S(-33,  92),   S(-13,  70),   S(-42, 126),   S(-32, 125),   S(-29, 103),   S(  0,  90),
            S(-22,  69),   S(-62, 121),   S(-34,  75),   S( 25,  78),   S(-34, 100),   S( 22,  49),   S(-35,  57),   S(-58,  68),
            S(-71,  81),   S(-17,  59),   S(  3,  63),   S( 44,  32),   S( 46,  53),   S( 37,  38),   S( 18,  29),   S(-23,  30),

            /* queens: bucket 3 */
            S( 78,  85),   S( 55,  73),   S( 44,  96),   S( 42,  86),   S( 70,  37),   S( 49,  31),   S( 31,  13),   S( 40,  57),
            S( 55, 109),   S( 46,  99),   S( 39, 107),   S( 43,  82),   S( 42,  78),   S( 65,  56),   S( 61,  22),   S(  5,  44),
            S( 58,  83),   S( 35, 103),   S( 47,  87),   S( 44,  74),   S( 39,  94),   S( 51,  96),   S( 47, 117),   S( 41,  72),
            S( 42, 120),   S( 36,  97),   S( 41,  85),   S( 39, 101),   S( 31, 102),   S( 30, 141),   S( 44, 109),   S( 22, 153),
            S( 29, 114),   S( 35, 113),   S( 31, 109),   S( 20, 108),   S( 14, 142),   S( 16, 128),   S(  1, 181),   S( 30, 165),
            S( 34, 137),   S( 58, 116),   S( 46, 105),   S( 18, 120),   S( 22, 149),   S( 58, 114),   S( 46, 149),   S( 29, 188),
            S( 48, 135),   S( 39, 113),   S( 52,  91),   S( 60, 100),   S( 44, 119),   S( 33, 132),   S( 72, 140),   S(131,  73),
            S( 84,  94),   S( 88,  93),   S( 77,  87),   S( 85,  94),   S( 28, 115),   S( 87,  66),   S(126,  62),   S(118,  65),

            /* queens: bucket 4 */
            S(  5, -19),   S( -4,  -2),   S(-35, -25),   S(-48, -53),   S(  0, -46),   S( 21,   4),   S(-54, -38),   S(-66, -26),
            S(-13, -10),   S( -2,   2),   S( 53, -27),   S(-52,   7),   S(  1, -18),   S( 15, -33),   S(  1,   3),   S(-20,  -9),
            S( 17,  10),   S( 13,  13),   S(  9,  38),   S(  6,  47),   S( 14, -13),   S( 32,  19),   S(  4, -20),   S(-14,  -8),
            S(-25,  -3),   S( 23,  42),   S(-10,  42),   S(-20,  48),   S( 59,  34),   S( 10,  17),   S(-15, -27),   S(-18,  -8),
            S(  7,  21),   S(  1,  -4),   S( 38,  38),   S( 46,  44),   S( 38,  43),   S( 11, -27),   S( -3, -14),   S(  3, -20),
            S( -4,   2),   S( 47,  14),   S( 12,  35),   S(  0,  32),   S( 53,  30),   S(-24,  -8),   S(-34, -31),   S(-26,  -4),
            S(-45, -22),   S(-11,  19),   S(  3,  27),   S( 15,  41),   S( 11,  19),   S( 11,  15),   S( -7, -32),   S( -3,  -3),
            S( -8, -12),   S( -9, -26),   S( 27,  33),   S(  6,  26),   S(  3,  14),   S(-22, -16),   S(-22, -43),   S(  3, -14),

            /* queens: bucket 5 */
            S(-57, -30),   S(-53, -35),   S(-33, -42),   S(-36, -54),   S(-56, -29),   S( 28,  11),   S(-35, -14),   S(  2, -24),
            S(-51, -27),   S(-15,  -1),   S(-54, -22),   S(-60, -12),   S(  5, -19),   S(-35, -23),   S(-54, -11),   S(-45, -28),
            S(-26,  16),   S(-44,  19),   S(-54,  -1),   S( -5,  28),   S( 25,  70),   S(-36,  31),   S(-19, -27),   S(  4,   7),
            S(-36,   1),   S(-58, -14),   S( 18,  46),   S(  4,  70),   S( 35,  46),   S( -1,  28),   S(  1, -23),   S(-19,   6),
            S(-43,  -7),   S(-29,  10),   S( -7,  75),   S( 31,  77),   S( 19,  64),   S(-19,  19),   S( 15,  26),   S(-44, -31),
            S(-44, -11),   S( 13,  41),   S(-19,  46),   S( 28,  42),   S( 14,  53),   S( 16,  17),   S( 13,  25),   S( 12, -14),
            S(-11,  14),   S(  7,  39),   S( 25,  83),   S( 13,  64),   S( 44,  74),   S( 29,  52),   S( 21,  30),   S(-21, -13),
            S(  4, -17),   S( 11,   5),   S( 22,  28),   S( 11,  54),   S( 20,  28),   S( -8,  -6),   S( -2, -30),   S(-26, -35),

            /* queens: bucket 6 */
            S(-20,  -2),   S(-67, -42),   S(-41, -48),   S(-128, -47),  S(-59, -43),   S(-68, -54),   S(-57, -44),   S(  8,  28),
            S(-56, -17),   S(-52,  20),   S(-31, -14),   S(-34, -13),   S(-69,  40),   S(-82,  14),   S(-94,  -8),   S(-25,   2),
            S(-19,   7),   S( -4,  -7),   S(-30,  39),   S(-72,  96),   S( -6,  71),   S(-16,  33),   S(-44,  -9),   S(-26,   4),
            S(-51,  33),   S(  9,  10),   S(-19,  67),   S(-44,  81),   S( 18,  82),   S( 12,  54),   S( 10,  43),   S( -5,  23),
            S(-57,  15),   S( 25,  39),   S(-35,  56),   S( 26,  47),   S( 49,  77),   S( 59,  68),   S( 59,  38),   S(-11,  38),
            S(-18,  45),   S( -8,  20),   S( 12,  35),   S( 27,  41),   S(  4,  58),   S( 66, 103),   S( -9,   6),   S(  5,  13),
            S(-39,  -2),   S( 33,  37),   S(  9,  52),   S( -5,  48),   S( 58,  73),   S( 27,  62),   S( -9,  16),   S(-36,  -2),
            S( 11,  39),   S(-13,  34),   S( 17,  40),   S( 17,  46),   S( 30,  69),   S( 21,  46),   S( -4,  28),   S( 15,  16),

            /* queens: bucket 7 */
            S( -1, -13),   S(-25, -24),   S(-31,   6),   S(-27,  13),   S(-37,  -1),   S(-34,   9),   S(-14, -18),   S(  5,  -9),
            S(-50,  28),   S(-72,  10),   S(-28,   5),   S(-22,  63),   S( -2,  10),   S(-32,  37),   S(-30,  65),   S(-17, -32),
            S(-52, -29),   S(-32,  32),   S(-29,  45),   S(  0,  46),   S( 20,  46),   S( -1,  64),   S( 12,   6),   S( 16,  -6),
            S(-40,   1),   S(  3,  -1),   S(-35,  58),   S( 13,  51),   S( 53,  28),   S( 45,  39),   S(-14,  83),   S(-21,  40),
            S(-22,  26),   S(-55,  40),   S( 10,  33),   S( 30,  20),   S( 62,  21),   S( 77,  19),   S( 11,  36),   S( 78,  -1),
            S(-29,  35),   S(-12,  20),   S( -6,  35),   S( 17,   7),   S( 50,  68),   S( 73,  33),   S( 62,  43),   S(  8,  33),
            S( 31,  -5),   S( 10,  24),   S( 39,   2),   S( 25,  43),   S( 53,  32),   S( 80,  50),   S( 45, -21),   S( 34,  40),
            S( 10,  32),   S( 23,  24),   S( 46,  28),   S( 30,  55),   S( 26,  54),   S( 23,  14),   S( 30,   9),   S( 33,  36),

            /* queens: bucket 8 */
            S(-12, -27),   S(  3,   0),   S(-12,   4),   S( -7, -11),   S( -8, -21),   S( -9, -15),   S(-23, -37),   S( -5,   4),
            S( -9, -11),   S(-18, -17),   S(-15,  -7),   S(-25,  -6),   S( -4,  -8),   S(-19, -36),   S(-25, -48),   S( -7, -14),
            S(  2,   5),   S(-13,  -1),   S(-16, -21),   S(  5,   0),   S(-31, -37),   S(-18, -20),   S(-16, -32),   S(-13, -21),
            S(-11,  -5),   S( 12,  32),   S( -2,   8),   S( -2,  12),   S( -7, -21),   S(  2,   5),   S( -8, -18),   S( -3, -17),
            S(  7,  18),   S( 19,  74),   S( 33,  43),   S(  6,  23),   S( 18,  29),   S(-17, -18),   S(-13, -16),   S(-10, -12),
            S( -1,   1),   S( 19,  22),   S( -1,  41),   S(  8,  29),   S(  1,   2),   S(-10, -15),   S(-14, -18),   S(  6,  12),
            S(-10, -17),   S(-12,  -3),   S( 12,  38),   S(  6,  -1),   S( -3,  -1),   S(-15, -26),   S(-16, -30),   S( -8, -20),
            S(-18, -43),   S( 14,  25),   S(-25, -46),   S(-23, -43),   S(-15, -37),   S(-14, -24),   S( -6, -15),   S( -8, -18),

            /* queens: bucket 9 */
            S( -3,  -9),   S(-10, -20),   S(  0,   4),   S(-24, -28),   S(-31, -51),   S(-26, -34),   S(-29, -56),   S(-25, -43),
            S( -4,  -9),   S( -5,  -7),   S(-16,  -4),   S( -8,  -6),   S(-43, -49),   S(-31, -53),   S(-12, -36),   S(-10, -25),
            S( 10,  19),   S(  0,   6),   S(-12,  30),   S(-11, -28),   S(-29, -32),   S(-16, -11),   S(-10,  -2),   S(-14, -32),
            S(-18, -22),   S( -6,  -4),   S(  7,  40),   S(-11,   7),   S(-18,  -2),   S(-16, -11),   S(-22, -37),   S( -9, -36),
            S(  2,   3),   S(  8,  28),   S( 16,  49),   S( 30,  79),   S( -1,  20),   S( -2, -12),   S(  3,   4),   S(-14, -25),
            S(-30, -39),   S(-21, -17),   S(  4,  27),   S(  6,  47),   S(-22, -19),   S( 13,  28),   S(-16, -24),   S( -8, -10),
            S(-26, -41),   S( -3,   1),   S(-21,   6),   S( 15,  39),   S(  4,   5),   S(  4, -11),   S(  3,   7),   S(-21, -36),
            S(-19, -29),   S(  2,   4),   S( -8, -10),   S( -6,  -3),   S(  7,   9),   S(-16, -35),   S( -1,  -7),   S(  4,   3),

            /* queens: bucket 10 */
            S( -9, -14),   S(-10, -16),   S(-21, -40),   S(-35, -44),   S(-23, -29),   S( -3,   3),   S( -3,  -9),   S( -9, -16),
            S(-24, -39),   S(-15, -21),   S(-11, -27),   S( -6,   7),   S(-12, -15),   S(-27, -27),   S( -7, -17),   S(-17, -25),
            S( -4,  -1),   S( -8, -24),   S(-15,  -6),   S(-14,   1),   S(-20,   1),   S( -9,  11),   S(  0,  19),   S(  5,  17),
            S( -9, -11),   S( -4,  -6),   S(-13,  -8),   S(  5,  10),   S( 14,  46),   S(-40, -20),   S( -7, -18),   S(-26, -37),
            S( -8, -13),   S( -8, -21),   S(-32, -18),   S( 19,  45),   S(  0,  21),   S(  2,  24),   S(  9,   2),   S( -8,  -8),
            S(-16, -26),   S(-20, -29),   S(-16,  -2),   S(-10,  36),   S( -1,  30),   S(  5,  26),   S(  3,   6),   S(-14, -20),
            S(-14, -26),   S(-22, -32),   S(  1,   2),   S(-32, -31),   S( -5,  -8),   S( 15,  37),   S(-16, -33),   S(-12, -15),
            S(  8,   5),   S(-27, -40),   S(  2,   6),   S(  7,  26),   S( 15,  34),   S(  1,  -1),   S(  4,   9),   S( -4, -20),

            /* queens: bucket 11 */
            S(-22, -49),   S(-17, -44),   S(-19, -28),   S( -5, -31),   S(-14, -16),   S(-22, -26),   S(-13, -12),   S(-14, -19),
            S(-22, -41),   S(  2,  -3),   S(-56, -60),   S(-34, -38),   S( -8,   7),   S(-31, -35),   S( -1,  -3),   S(-10,   3),
            S(-22, -28),   S(-16, -41),   S(-12, -25),   S(-28, -48),   S( -5,   6),   S(-10,   4),   S(-11, -12),   S(-24, -25),
            S(-25, -55),   S(-10, -12),   S(-16, -23),   S(  4,  29),   S(  9,  -2),   S( -4,   5),   S( 18,  32),   S(-21, -25),
            S(-15, -19),   S( -3, -15),   S(-20, -23),   S( 23,  43),   S( 26,  23),   S( 48,  93),   S( 19,  44),   S(-13,  -6),
            S(-10, -22),   S(-10, -18),   S(-10, -19),   S( 13,  32),   S( 23,  21),   S( 49,  77),   S( 14,  18),   S( -6,   6),
            S(  3,   2),   S(-21, -34),   S( 17,  36),   S(-19, -22),   S(  5,  19),   S( 26,  35),   S( 26,  30),   S( -2, -16),
            S(-10, -23),   S( -7, -27),   S( -5, -13),   S(  6,   3),   S(  3,  -3),   S( -6, -38),   S( 21,  22),   S( -4, -35),

            /* queens: bucket 12 */
            S(  9,  20),   S( -1,  -3),   S( -5, -10),   S( -5,  -9),   S( -8, -16),   S( -6, -12),   S( -5, -11),   S(-16, -27),
            S( -4, -10),   S( -2,   1),   S(-10, -10),   S( -6, -19),   S( -7, -14),   S(-13, -17),   S( -7, -23),   S(-12, -20),
            S(  4,   1),   S( -2,  -3),   S( 11,  16),   S(  1,  -7),   S( -1,  -2),   S( -7, -15),   S(-14, -34),   S( -4, -11),
            S(  2,   2),   S( -2,   4),   S( -5,   0),   S(  2,  11),   S(  7,   3),   S( -1, -13),   S( -3,   0),   S(  0, -11),
            S( -5,  -7),   S(  6,   5),   S( 29,  57),   S(  5,  19),   S(  0,  13),   S(  2,   4),   S(-15, -25),   S(  1,   5),
            S(  2,   6),   S(  3,   7),   S( 21,  54),   S(  6,  -2),   S(  8,  11),   S( 10,  21),   S(  3,  11),   S( -7, -16),
            S(  3,   9),   S( 13,  27),   S( 33,  51),   S(  3,   6),   S(  5,  13),   S(  5,   7),   S(  6,  10),   S( -1,  -5),
            S(-14, -35),   S(-18, -37),   S(-27, -46),   S(-18, -42),   S( -8, -15),   S(  1,   5),   S( -3, -17),   S( -8,  -7),

            /* queens: bucket 13 */
            S( -6, -19),   S( -3,  -6),   S(  3,   9),   S(-16, -28),   S( -3,  -8),   S(  0, -11),   S(-14, -31),   S( -1,  -1),
            S(  2,   3),   S(  9,  18),   S(  4,   5),   S( -6, -15),   S( -6,  -6),   S( -2,  -4),   S( -9, -23),   S(-18, -31),
            S( -6, -10),   S(-14, -30),   S( -6, -13),   S( -5, -21),   S( -4,  -1),   S(-14, -30),   S(-10, -28),   S(-18, -31),
            S( -4,  -3),   S(-16, -35),   S( 10,   7),   S( 26,  30),   S( 22,  40),   S( -8, -28),   S( -9, -24),   S(-18, -41),
            S(-14, -26),   S(  8,  23),   S(  5,  17),   S(  3,  21),   S( 26,  51),   S( -6, -15),   S( -7, -22),   S( -9, -19),
            S(  0,  -5),   S(  5,  29),   S( 42,  78),   S( 42,  90),   S( -5,  14),   S( -2,   0),   S(  1,   1),   S( -2,  -9),
            S( -6, -14),   S(  7,  18),   S(  1,  15),   S(  9,  30),   S(  8,  10),   S( -4, -11),   S(  3,   2),   S(  5,  11),
            S(-24, -49),   S(-12, -23),   S(-17, -23),   S( -4,  -1),   S(  5,  -1),   S(  5,   5),   S( -5, -10),   S( -8, -16),

            /* queens: bucket 14 */
            S( -8, -17),   S( -5, -15),   S( -2,  -9),   S( -5, -12),   S( -2, -12),   S( -6, -14),   S( -4, -10),   S(-12, -25),
            S( -6, -18),   S(  0,  -1),   S(-12, -24),   S( -9, -25),   S(-11, -25),   S( -7, -27),   S( -6, -13),   S(-15, -35),
            S( -1,  -5),   S( -8, -17),   S(-12,  -9),   S(-15, -32),   S(  6,   8),   S(-13, -31),   S(  3,   9),   S(-16, -26),
            S( -9, -17),   S(-10, -25),   S(  0,   3),   S(  9,  28),   S(  3,   0),   S( -1,  -4),   S( 11,  23),   S(  6,   2),
            S(  5,  15),   S(  8,  18),   S( -2,   5),   S(  9,  19),   S( 27,  61),   S( 25,  46),   S(  3,   0),   S( -6, -19),
            S(-11, -24),   S( -1,  -7),   S( 30,  57),   S( -6, -12),   S( 17,  39),   S( 11,  27),   S(  6,   3),   S( -3, -12),
            S( -1,  -2),   S( 10,  10),   S( 15,  26),   S( 35,  65),   S( 31,  53),   S( 11,  32),   S( 18,  29),   S(  9,  16),
            S(-13, -23),   S( 10,  12),   S(-18, -37),   S(  8,   1),   S( -7, -14),   S(  7,  12),   S(-11, -22),   S(-27, -60),

            /* queens: bucket 15 */
            S( -8, -26),   S(  2,  -5),   S(-10, -22),   S(  0,  -3),   S( -6,  -7),   S( -5, -13),   S( -7, -15),   S( -1, -12),
            S(  0,  -5),   S( -2,  -9),   S( -8, -14),   S( -4, -13),   S(  8,  17),   S( -8, -14),   S( -1,  -3),   S( -4,  -9),
            S(  1,   0),   S( -2, -10),   S( -1,  -4),   S( -3, -11),   S(  3,   2),   S( -4,  -8),   S( -8, -18),   S( -4,  -7),
            S(-15, -26),   S( -3, -14),   S(-13, -20),   S( 11,  15),   S(  2,  10),   S( -6, -14),   S( -3, -15),   S(  0,   4),
            S( -1,   5),   S(  0,  -6),   S(-12, -28),   S(  3,  -3),   S(  6,  12),   S( 11,  25),   S(  0,  -7),   S(  2,  -1),
            S(  3,   2),   S( -6, -16),   S(  1,  -4),   S( -8, -14),   S(  3,   7),   S( 17,  29),   S( 11,  26),   S( -7, -16),
            S( -3,   0),   S( -1, -18),   S(  9,  16),   S( 10,  14),   S(  6,   5),   S( 16,  29),   S( 19,  34),   S(  5,   7),
            S( -5, -11),   S(-15, -38),   S(  4,   9),   S(  6,   7),   S( 10,  15),   S( -7, -15),   S( -5, -11),   S( -8, -23),

            /* kings: bucket 0 */
            S( 28,  31),   S( 13,  56),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 24,  63),   S( 77,  76),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-22,  17),   S(-76,  20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -1,  50),   S( -8,  47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-44,  28),   S(-44,  26),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 11,  52),   S( 19,  43),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19,  53),   S(  5,  30),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 51,  95),   S( 23,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -39),   S( 15, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38,  -7),   S( 15,  16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -2, -41),   S(-43, -22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 12, -11),   S( -4, -14),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-21, -12),   S(-43,  -8),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  9,   0),   S(-20,   7),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 13,  24),   S( -9, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 33,  45),   S(-16,  19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-100, -45),  S(-22, -10),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -55),   S( 15, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -44),   S(-21, -48),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 52, -56),   S( 38, -46),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -8, -44),   S(-34, -36),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 71, -63),   S( 74, -53),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -9, -22),   S(-99, -52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 82, -43),   S( 41, -73),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -4, -68),   S( 42, -32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -97),   S(-17, -43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -22),   S( 32, -32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 41, -74),   S( 43, -78),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 76, -58),   S( 31, -44),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 25, -73),   S(-13, -69),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 68, -47),   S(-61, -53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -2, -61),   S(-11, -114),

            #endregion

            /* enemy king piece square values */
            #region enemy king piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -20),   S(-35,   6),   S( -1,  -2),   S(-11,  14),   S(  4,  11),   S( 23,  20),   S( 30,  -4),   S( 41,  -3),
            S(-13, -35),   S(-29, -16),   S(-15, -16),   S( -3, -15),   S( 14,  -2),   S( -4,  12),   S( 25,  -4),   S( 22,   6),
            S( 11, -22),   S(  4, -23),   S( 35, -27),   S( 13, -24),   S( 10,   5),   S( 13,  28),   S( 15,  25),   S( 51,   9),
            S( 24, -34),   S( 47, -13),   S( 64, -28),   S( 37,   2),   S( 17,  50),   S( -7,  86),   S( 18,  76),   S( 81,  41),
            S( 98, -134),  S(136, -106),  S(102, -26),   S( 37,  42),   S( 56, 155),   S( -7, 142),   S( 18, 157),   S(110, 110),
            S(-239, -178), S(-204, -217), S( 32, -175),  S( 41,  53),   S( 85, 155),   S( 95, 148),   S(150, 117),   S( 64, 189),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-63,  20),   S(-46,   2),   S(-29,   0),   S(-44,  26),   S(-18, -13),   S( -1,   5),   S(  1,  -3),   S(  2,  16),
            S(-58,  -2),   S(-39,  -7),   S(-43,  -5),   S(-20,  -4),   S(  4,  -4),   S(-26,   1),   S( -8, -11),   S(-23,   7),
            S(-41,  15),   S(-16,   9),   S(-20,  -3),   S( 12, -24),   S(  0,  11),   S(-11,   7),   S(-24,  18),   S( -2,  13),
            S(-29,  34),   S( 33,   4),   S( -3,   7),   S( 26,  12),   S( 21,  12),   S(-29,  38),   S( 11,  28),   S( 44,  40),
            S(  0,  38),   S( 77, -20),   S(116, -147),  S( 47, -129),  S( 57,  -3),   S( 13,  37),   S(-46,  81),   S( 49,  98),
            S(157, -48),   S(-10, -55),   S( 15, -228),  S( -7, -222),  S(-63, -87),   S(-87,  58),   S( 16, 162),   S( 86, 169),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  33),   S(-41,   8),   S(-36,   8),   S(-24,  20),   S(-47,  38),   S(-31,  14),   S(-18,  -7),   S(-25,  22),
            S(-51,   9),   S(-37,   9),   S(-43,   1),   S(-43,  19),   S(-39,  17),   S(-48,   3),   S(-24, -13),   S(-51,   9),
            S(-33,  38),   S(-34,  35),   S(-18,  17),   S(-22,  17),   S(-31,  28),   S(-20,   3),   S(-22,   1),   S(-21,   5),
            S(-12,  64),   S(-34,  60),   S( -3,  35),   S(  7,  28),   S( -3,  19),   S(-13,   5),   S( 14,  16),   S( 35,   5),
            S(-39, 131),   S(-62, 118),   S(-39,  50),   S( 33, -24),   S(111, -124),  S( 78, -83),   S( 72, -29),   S( 47,  17),
            S(-14, 202),   S( 45, 129),   S(-17,  51),   S(-11, -103),  S(-38, -248),  S(-103, -245), S(-63, -88),   S(100, -37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12,  15),   S(-12,   8),   S(-11,  19),   S(-16,  43),   S(-23,  59),   S( 10,  39),   S( -4,  12),   S(-16,  -2),
            S(  1,   2),   S(  5,   7),   S(-14,  10),   S(-18,  20),   S(  0,  25),   S(  0,  15),   S( -3,  -3),   S(-36,   0),
            S( 16,  32),   S( -5,  47),   S(  5,  25),   S( -4,  10),   S( 13,   3),   S( 20,  -1),   S(  5, -13),   S(-12,  -6),
            S( 18,  76),   S( -3,  88),   S( 16,  67),   S( 15,  39),   S( 30,   7),   S( 43, -23),   S( 29,   6),   S( 47, -23),
            S( -4, 166),   S(-28, 182),   S(-40, 186),   S( -7, 136),   S( 42,  62),   S( 91, -17),   S( 89, -95),   S( 91, -93),
            S( 27, 168),   S( 15, 258),   S(-44, 279),   S(-21, 210),   S(-74, 114),   S(-42, -160),  S(-180, -285), S(-259, -239),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 53, -11),   S( 18,  -9),   S(  8, -16),   S( 29, -25),   S(-15,  -2),   S(  5, -13),   S( -9,  -7),   S(-66,  38),
            S( 51, -31),   S(  8,  -1),   S( 34, -26),   S( 17, -20),   S(-26, -22),   S(-27, -17),   S(-42, -17),   S(-55,  -6),
            S( 87, -42),   S(124, -58),   S( 43, -25),   S(-27,  -4),   S(-66,  16),   S(-16,  10),   S(-40,   3),   S(-51,  27),
            S(-25, -130),  S( 37, -170),  S( 28, -30),   S(-25,  14),   S(  1,  11),   S(-43,  49),   S(-10,  48),   S(  6,  32),
            S( 48, -115),  S(-41, -127),  S(  6, -104),  S( 60,  16),   S( 81, 100),   S( 69,  73),   S( 46,  88),   S( 16, 116),
            S(  6, -30),   S(-21, -84),   S(  3, -73),   S( 19,  36),   S( 58, 109),   S( 52, 147),   S( 58, 136),   S( 31, 136),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-51,  27),   S( -8,  12),   S( 23, -11),   S( 77, -15),   S( 80, -25),   S( 27,  -8),   S(-20,   2),   S(-70,  43),
            S(-51,   8),   S( 24,  -7),   S( 21, -22),   S(  7,  -4),   S(-16,   8),   S(  1, -14),   S(-46,  -5),   S(-66,  16),
            S(-17,  13),   S( 32,  17),   S( 83, -15),   S( 18,   8),   S(-17,  25),   S(-18,   8),   S(-31,  10),   S(-38,  27),
            S( 15,  11),   S( 69, -30),   S(-14, -90),   S(-20, -62),   S(  2, -28),   S( -7,  12),   S( 53,   8),   S( 13,  32),
            S( 36,  17),   S( 63, -59),   S( 36, -117),  S( -3, -80),   S( 62, -75),   S(-16,  42),   S( 39,  43),   S( 11,  90),
            S( 49,  40),   S( 26,   7),   S( 16, -87),   S( 15, -86),   S( 24, -35),   S( 63,  47),   S( 87, 135),   S( 92, 124),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-79,  16),   S(-50,  -2),   S( -7, -10),   S( 21,  -7),   S( 38,   5),   S( 30,   0),   S( 21,  -5),   S(  8,  17),
            S(-68,  -4),   S(-35, -14),   S(-28, -16),   S( 44, -14),   S( -2,   9),   S( 17, -15),   S( -5,  -3),   S(  1,  -1),
            S(-35,  14),   S(-41,  16),   S( -7,  10),   S(  8,  15),   S( 42,   3),   S( 75, -23),   S( 46, -10),   S( 25,   0),
            S(-42,  49),   S( 23,  14),   S( 14,  10),   S( 20,  -5),   S(-35, -69),   S(  5, -79),   S( 53, -27),   S( 98, -25),
            S(-10,  91),   S(  8,  32),   S( 38,   4),   S(  8, -50),   S(-38, -94),   S(-14, -78),   S(104, -70),   S( 82,  11),
            S(117,  59),   S(128,  69),   S( 46,  33),   S( 46, -53),   S(-14, -94),   S(-28, -106),  S( 11, -35),   S( 56,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61,  -2),   S(-32, -32),   S( -9, -20),   S(-43,  25),   S( 32,   4),   S( 75, -17),   S( 70, -34),   S( 62, -12),
            S(-49, -18),   S(-46, -18),   S(-42, -26),   S(-30,   2),   S(  5,  -5),   S( 62, -36),   S( 40, -24),   S( 55, -27),
            S(-27,  11),   S(-47,  15),   S(-16,  -3),   S(-28, -13),   S(  1,   0),   S( 46, -26),   S( 87, -37),   S( 81, -37),
            S(  6,  26),   S(-63,  64),   S(-24,  46),   S(-17,  34),   S( -6,  -5),   S( 53, -66),   S( 18, -103),  S( 13, -139),
            S( 50,  56),   S(  6, 107),   S( 30, 108),   S( 26,  70),   S(-37,  55),   S( -6, -69),   S(-58, -132),  S(  1, -106),
            S(134, 108),   S(124, 133),   S(130, 112),   S( 60, 123),   S( 89,  17),   S( 11, -72),   S(  4, -57),   S( 20, -142),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 16, -14),   S(  4, -26),   S( 50, -11),   S(-13, -30),   S(-42, -62),   S( 25, -45),   S( 35, -60),   S( 43, -34),
            S(-63, -60),   S(-51, -17),   S(-63, -51),   S(-56, -34),   S(-42, -42),   S( 19, -56),   S( -9, -46),   S(-24, -34),
            S(-56, -64),   S( 22, -73),   S(-25, -58),   S(-18, -54),   S(-32, -13),   S(-32, -19),   S(-43, -18),   S(-30,  -9),
            S(-25, -10),   S(-46, -44),   S( 29,  -7),   S( 45, -12),   S( 10,   2),   S(-14,  47),   S( 13,  34),   S(  7,  26),
            S( 37,  22),   S(  1, -12),   S(  6,  28),   S( 58, 125),   S( 79, 148),   S( 56, 112),   S( 30,  89),   S( -2, 103),
            S( 28,  59),   S(  6,  39),   S( 29, 106),   S( 52, 137),   S( 36, 108),   S( 56, 178),   S( 44, 119),   S( -2,  76),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 29,  -8),   S( 25,   6),   S( 25,  -7),   S(-10, -14),   S( -7, -15),   S(112, -45),   S(106, -69),   S(  2, -27),
            S(-19, -38),   S(-55, -41),   S(  0, -64),   S(-10, -40),   S(-44, -32),   S(-18, -46),   S( 15, -57),   S(  3, -44),
            S(-88,  -9),   S(-43, -50),   S(-13, -81),   S(-59, -43),   S( 16, -38),   S(-15, -42),   S(-56, -38),   S(-50,  -5),
            S(-39,   6),   S(-43, -45),   S(-17, -47),   S(-23, -38),   S(  8, -29),   S(-23,   0),   S(-14,  11),   S( 22,   7),
            S( -4,  33),   S( 10,  -7),   S( 24, -10),   S( 32,  36),   S( 25,  53),   S( 31,  66),   S(  3,  71),   S(-12,  78),
            S(-11,  78),   S( 22,  66),   S( 12,  32),   S( 31,  24),   S( 40,  91),   S( 31,  77),   S( 11,  99),   S( 24,  86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -55),   S(-12, -55),   S( 23, -29),   S( -6, -33),   S( 49, -28),   S(153, -33),   S( 70, -21),   S( 48, -22),
            S(-40, -53),   S(-36, -61),   S( 21, -68),   S( 30, -41),   S(-12, -48),   S( 11, -50),   S( 19, -44),   S( 19, -46),
            S(-79, -32),   S(-37, -49),   S( -9, -44),   S( 12, -46),   S( -2, -55),   S( -9, -76),   S(-14, -66),   S( 26, -46),
            S(  4, -17),   S( 10, -18),   S(-11, -11),   S( 20, -42),   S(-38, -35),   S(-61, -41),   S(-10, -49),   S(-20, -22),
            S( -2,  35),   S( 35,  53),   S( 23,  18),   S( 17,  -5),   S( 26,  17),   S( 12,   5),   S(-15,   9),   S(  5,  25),
            S(-26,  64),   S( 10,  43),   S( 30,  78),   S( 31,  87),   S( 23,  48),   S(  3,  11),   S( -3,  23),   S( 36,  52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -44),   S( -8, -46),   S( -7, -28),   S( -4,  -6),   S(  6, -14),   S(127, -18),   S(103, -42),   S( 58, -11),
            S(-34, -65),   S(-40, -77),   S(-12, -81),   S( 11, -57),   S(-10, -32),   S(-14, -56),   S( -5, -47),   S(  7, -63),
            S(-37, -17),   S(-39, -27),   S(-35, -10),   S( 23, -42),   S(-21, -46),   S(  5, -39),   S(-33, -81),   S( -5, -68),
            S(-20,  11),   S(-32,  26),   S( 24,  24),   S( 42,   9),   S(  4,  -5),   S(-12, -25),   S(-33, -48),   S(-35, -49),
            S(  9,  33),   S( -6,  87),   S( 24,  97),   S( 35,  84),   S( 46,  61),   S( 14,  15),   S( 29,  54),   S( -2,  -6),
            S( 39,  79),   S( 27,  93),   S( 42,  90),   S( 33,  98),   S( 28,  56),   S( 50, 123),   S( 22,  40),   S( 40,  46),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -77),   S(-23,  -9),   S(-31, -25),   S( -1,   3),   S(  1, -25),   S(-18, -65),   S( 23, -33),   S(  3, -20),
            S(-17, -40),   S(-30,  13),   S(-46, -76),   S(-35, -39),   S(-23, -39),   S( -1, -49),   S( -8, -57),   S(  4, -73),
            S(-39,  -4),   S(  7, -54),   S(-26, -95),   S(-28, -43),   S(  9,  -9),   S(-20,  -1),   S(-57, -17),   S(-23, -29),
            S( -4,   6),   S(  8,  42),   S(-13, -27),   S( 13,  17),   S( 11,  68),   S(  2,  79),   S(  3,  65),   S(-39,  35),
            S(  9,  23),   S( 13,  29),   S( 16,  44),   S( 24,  85),   S( 38, 127),   S( 40, 103),   S(  7, 144),   S( 11, 114),
            S( 11,  11),   S(  6,  28),   S( 26,  67),   S( 15,  63),   S( 35, 110),   S( 38, 128),   S(-16,  49),   S( -8,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -40),   S(-36, -75),   S( 10,  13),   S( -5, -30),   S( 12, -11),   S(  0, -34),   S(  3, -61),   S(-12, -56),
            S(-66, -52),   S(-51, -88),   S(-43, -65),   S( -5, -19),   S(-24, -45),   S(-12, -41),   S(-30, -53),   S(-51, -70),
            S(-29, -20),   S(-27, -79),   S(-12, -75),   S( -6, -56),   S(-31, -64),   S(  1, -34),   S(-18, -49),   S(-30, -24),
            S(-19,  22),   S(-10, -36),   S(  5, -22),   S(  1,  10),   S( 10,  54),   S(-19,  37),   S(-27,  17),   S(-32,  19),
            S(  3,  53),   S( 17,  81),   S(  2,  11),   S( 30,  59),   S( 37, 136),   S( 27, 123),   S( 24, 135),   S(  4, 112),
            S( 18, 140),   S( 22,  77),   S( 17,  54),   S( 23,  72),   S( 26, 101),   S( 35, 144),   S( 13,  95),   S(  4, 101),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -102),  S(  2, -51),   S( -3, -43),   S(  5,   3),   S(-15, -47),   S(  8, -17),   S( -4, -41),   S(-19, -33),
            S(-48, -71),   S(-79, -74),   S(-28, -82),   S(-37, -95),   S(-19, -54),   S( -4, -47),   S(-37, -37),   S(-22, -29),
            S(-30, -57),   S(-43, -62),   S(-37, -34),   S(-12, -55),   S(-28, -76),   S(-11, -75),   S( -1, -57),   S(-34,  -8),
            S(-38,  13),   S(-19, -17),   S(-17,  36),   S(-10,   7),   S(  5, -36),   S(-11,  -7),   S(  7,   8),   S(  6,  56),
            S( -5,   4),   S( 14,  38),   S( 18,  89),   S( -3,  26),   S( 19,  63),   S( 14,  66),   S( 27, 107),   S( 26, 124),
            S( 10,  86),   S( 14,  75),   S( 32, 108),   S( 14,  45),   S( 20,  85),   S( 15,  88),   S( 32, 138),   S( 31, 150),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -30),   S( -8, -51),   S( -8, -25),   S( -9, -20),   S(-20, -40),   S(-40, -25),   S(-18, -16),   S(-13, -48),
            S(-12, -50),   S(-26, -89),   S( -4, -65),   S(  0, -38),   S( -2,  -6),   S(-41,  -9),   S(-30,  -6),   S(-33, -47),
            S(-29,  -2),   S(-43, -50),   S(-36, -40),   S( 28, -32),   S(-20, -31),   S(-12, -32),   S(  0, -10),   S(-17, -16),
            S(-23,  29),   S(-24,   3),   S(  2,  16),   S(  0,  36),   S( 32,  57),   S(  6,  -1),   S( -6,  -5),   S(  4,  27),
            S( -3,  69),   S(  2,  64),   S( 14,  98),   S( 25,  61),   S( 33, 145),   S( 15,  83),   S( 26,  92),   S( 18,  48),
            S(-13,  42),   S(  1,  62),   S(  3,  88),   S( 22,  82),   S( 30, 120),   S( 21,  73),   S( 16,  52),   S( 10,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-90, -28),   S(-26, -30),   S(-25,   4),   S( 14,   6),   S(-29, -30),   S(-43,  49),   S(-12,  -2),   S(-67, -39),
            S( 10, -20),   S(-19,  -3),   S(-28,  -8),   S( -3,   0),   S( -5,   8),   S(-18,   3),   S(-34, -39),   S(-38,  16),
            S(-27,  -3),   S(  5, -10),   S( -6,  24),   S( 46,  32),   S(-13,  22),   S(  1,  -4),   S(-41,  34),   S( -7, -42),
            S( -4,  67),   S( 35,  52),   S( 19,  33),   S( 45,  19),   S(  2,  54),   S(  9,  25),   S( 17,   3),   S( -6, -10),
            S( 27,  81),   S(-32,  74),   S( 27,  88),   S( 29,  52),   S( 62,  48),   S( 15,  30),   S( 19,   9),   S(  3,  14),
            S(101, -44),   S(-10,  58),   S(116,  65),   S( 84,  17),   S( 69,  67),   S(-20,  73),   S( 24,  -2),   S(-40,   5),
            S( 37,  13),   S(  2,  -9),   S( 19, -16),   S( 80,  68),   S( 64,  22),   S(  1,  29),   S(-50,  15),   S(-38,  22),
            S(-100, -153), S(  1,  -3),   S( 23,  32),   S(  9,  61),   S(  8,  37),   S( 14,  17),   S(-41, -17),   S( -1,   0),

            /* knights: bucket 1 */
            S( 27,  18),   S(-68,  11),   S(-36,  13),   S(-19,  23),   S(-40,  52),   S(-30, -29),   S(-40,   3),   S(  1,  14),
            S(-62,  51),   S(-33,  46),   S(-27,  42),   S(-13,  21),   S(-22,  22),   S(  0,  28),   S(-33,  23),   S(-11, -50),
            S(-43,   8),   S(-17,   9),   S(-24,  20),   S(-19,  60),   S(-11,  38),   S(-19,  17),   S(-50,  29),   S(-36,  55),
            S(-26,  71),   S( 13,  34),   S( -4,  50),   S(-21,  63),   S(-11,  57),   S(-29,  68),   S(-24,  39),   S(-19,  48),
            S( 48,   3),   S(-24,  19),   S( 25,  62),   S( -1,  49),   S( 40,  40),   S( -3,  53),   S(-13,  44),   S(-15,  67),
            S(  5,  35),   S( 52,  20),   S( 88,  28),   S(103,  25),   S( 50,  38),   S(-44,  76),   S( 64,  24),   S( 20,  38),
            S( 14,  -4),   S(  4,   3),   S( 39, -40),   S( 24,  31),   S( 28,  34),   S( 34,  17),   S( 20,  47),   S(-19,  34),
            S(-189, -53),  S(  4, -10),   S(-16, -60),   S(-17,   0),   S( -5,   9),   S( 43,  36),   S( 42,  74),   S(-69,  16),

            /* knights: bucket 2 */
            S(-69,  -5),   S(-38,  20),   S(-35,  36),   S(-15,  31),   S(-21,  28),   S(-53,  -9),   S(-26,   9),   S(-24, -29),
            S(-34,  21),   S( -7,  47),   S(-30,  32),   S( -9,  36),   S(-27,  30),   S(-10,  10),   S(  9,  18),   S(-27,  16),
            S(-46,  67),   S(-28,  29),   S(-24,  31),   S(-28,  74),   S(-24,  57),   S(-28,  25),   S(-25,  23),   S( -8,  14),
            S(-19,  69),   S(-11,  57),   S(-24,  88),   S(-34,  92),   S(-30,  81),   S(-18,  60),   S(  3,  49),   S(-12,  48),
            S(-38,  88),   S(-30,  70),   S( -3,  76),   S(  4,  62),   S(-29,  77),   S(  3,  66),   S(-38,  82),   S( 18,  14),
            S(-33,  88),   S(-11,  73),   S(-30,  99),   S( 30,  43),   S( 19,  71),   S(130,  10),   S( 76,  29),   S(-28,  40),
            S( 10,  47),   S(-52,  63),   S( 33,  28),   S( 22,  -4),   S(-19,  27),   S( -5, -15),   S(  0,  24),   S( 11,  -7),
            S(-54,  28),   S( 39,  49),   S(-13,  58),   S(-28, -19),   S(-20,  -9),   S(-37, -75),   S( 31,   4),   S(-94, -61),

            /* knights: bucket 3 */
            S(-59,  23),   S(-31, -22),   S( -7,  16),   S(-12,  35),   S(-14,  33),   S(-23,  11),   S(-25,   2),   S(-43, -40),
            S(-33,  18),   S( -7,  42),   S( -6,  28),   S(-10,  36),   S(-17,  32),   S( 12,  15),   S(  9,   0),   S( 12, -20),
            S(-35,  46),   S(-26,  36),   S(-10,  48),   S(-12,  86),   S(  0,  72),   S(-16,  56),   S(  1,  33),   S( 14, -17),
            S(-12,  32),   S( 17,  49),   S( -1,  85),   S(-15, 100),   S( -1, 104),   S(  1,  89),   S(  6,  87),   S( -1,  66),
            S(-32,  86),   S(-11,  69),   S( 13,  87),   S( 13, 112),   S( -1,  94),   S(  2, 113),   S(-36, 119),   S( 30, 111),
            S(-28,  67),   S(  4,  75),   S( 20,  90),   S(  0, 110),   S( 48, 105),   S( 67, 138),   S( 31, 107),   S(  4, 122),
            S(-30,  74),   S(-17,  72),   S(-39, 103),   S( 62,  68),   S( 37,  84),   S( 74,  61),   S(  5,  16),   S( 38,  -8),
            S(-163,  73),  S(-26, 107),   S(-60, 118),   S( 32,  77),   S( 17,  93),   S(-118,  97),  S(-20, -46),   S(-60, -108),

            /* knights: bucket 4 */
            S( 23,  48),   S( 33,  21),   S(-70,  10),   S(-44, -26),   S(-20,  11),   S( 15, -20),   S( 37, -21),   S(-22, -23),
            S(  3,  13),   S(  2,  -6),   S(-19,   5),   S(-13,  17),   S(-24, -17),   S( 49, -63),   S( 19,  14),   S(-46, -22),
            S( 16, -14),   S( 12,  -7),   S( 67,   0),   S( 98, -23),   S( 30,  29),   S( 34, -12),   S(-23,  -6),   S(-14, -41),
            S( -7, -34),   S( 25,   3),   S( 41,  -3),   S( 79,   9),   S( 39,  15),   S(-11,  45),   S(-55,  31),   S( 11,  33),
            S( 12, -54),   S( -2, -25),   S( 64,  26),   S( 51,  55),   S( 66,  -8),   S( 43,  35),   S( 50,  -5),   S(-47,  32),
            S(  4, -12),   S(  0, -12),   S( 18, -26),   S( 80,  25),   S( -7,  45),   S( 13,  53),   S(-28,   8),   S( 24,   1),
            S(-17, -22),   S(-26, -40),   S( -5, -28),   S( 17,   8),   S( 29,  27),   S(  5,  29),   S( 19,  29),   S(-39, -22),
            S( -1,  16),   S(-10, -36),   S(-13, -47),   S(  1, -25),   S( -2,  -3),   S( -9,  31),   S(-11,  12),   S( -8, -13),

            /* knights: bucket 5 */
            S(  4,   6),   S( 19,  19),   S(-39,  43),   S(-14,  23),   S(  9,  26),   S( 26,  -2),   S(-47,  13),   S( 12,  24),
            S( 25,  22),   S( 38,  19),   S( 48,  -3),   S(  7,   8),   S( 66, -18),   S(-26,   2),   S(  9,  54),   S(-82,  28),
            S(  0,  15),   S(-29,   1),   S( 40,   5),   S( 30,  15),   S( 19,  27),   S(  8,  15),   S( 13,  16),   S(-65,  28),
            S( 50,  12),   S( 12, -15),   S( 59,   2),   S( 88, -11),   S(105,   3),   S( 82,  -9),   S( 31,  10),   S( 23,  17),
            S( 75,   4),   S( 67,  -1),   S( 85,   0),   S(122, -22),   S(111,  -9),   S( 69,   8),   S( 35,  -3),   S(-11,   5),
            S(  0, -13),   S( 29, -18),   S( 10, -28),   S( 21,  20),   S( 55,  -7),   S( 39,   2),   S( 17,   1),   S( 25,   9),
            S( 13,  18),   S(-36, -42),   S( 38, -21),   S(-16,  -8),   S(  2, -15),   S( 36,   2),   S( 28,  43),   S( 11,  38),
            S(-18,  -2),   S(-21, -63),   S( 12,  -2),   S(-20, -50),   S( 16,  -5),   S( -4,  23),   S( 13,  32),   S( -3,  25),

            /* knights: bucket 6 */
            S(  7,  28),   S(-16,  17),   S( -8,   8),   S(-22,  28),   S(  6,  24),   S( 19,   8),   S(-10,  49),   S(-16,  44),
            S( 32,   5),   S( 32,  30),   S( 19,  -8),   S( 47,   5),   S( 58,   2),   S(  0,  20),   S(  9,  37),   S(-34,  55),
            S( 23,  20),   S( 56,  14),   S( 34,  14),   S( 73,  23),   S( 33,  34),   S(-23,  31),   S(  8,  25),   S(-23,  25),
            S( 40,  25),   S(100,  -6),   S( 74,   8),   S(127,   4),   S( 99,   1),   S( 85,   8),   S( 50,   1),   S(-23,  46),
            S(-14,  47),   S( 95,   0),   S(128,   2),   S(108,  24),   S( 97,  -4),   S( 93,  19),   S(153, -13),   S( 40,  22),
            S( 17,  30),   S( 32,   6),   S( 50,  21),   S( 48,  22),   S( 46, -16),   S( 57, -14),   S(  7,   9),   S( 33, -13),
            S( 19,  21),   S( 21,  36),   S( 22,  37),   S(-10,   6),   S( 24,  14),   S( 44, -32),   S(  1,  -4),   S( 13,  51),
            S( 24,  34),   S(  2,  -4),   S( 10,  33),   S(  5,  29),   S( 38,  -6),   S(  8,   0),   S(  2,  28),   S(-36, -37),

            /* knights: bucket 7 */
            S(-13, -22),   S(-19, -22),   S( 25,   0),   S( -2,  19),   S( 24, -15),   S(-49,  16),   S(  1,  -1),   S(-26,  35),
            S(-12, -27),   S( 39, -22),   S(-41,   4),   S(-27,  23),   S( 33, -12),   S( 42,  13),   S( -5,  23),   S(-45,  34),
            S(  9, -24),   S(-42,   2),   S( 26, -13),   S( 22,  32),   S( 80,   4),   S( 57,  15),   S( 41,   5),   S(-18,  43),
            S( -5,   3),   S( 10,   8),   S( 58,  -7),   S(103,   4),   S(140, -24),   S(110,  12),   S( 70,  -1),   S( 62,  13),
            S( 10,  -1),   S( 38,  -5),   S( 45,  16),   S(106, -13),   S(158, -17),   S(157, -25),   S(188,   0),   S( 51,  -5),
            S( -7,  18),   S( 25,  23),   S( 21,  16),   S( 75,   8),   S(143, -14),   S(110, -16),   S( 23,  -2),   S( -9, -48),
            S(-10,  26),   S( 12,   5),   S( 35,   4),   S( 33,  55),   S( 68,  17),   S( 45,  29),   S(-22, -50),   S( -7, -22),
            S(-54, -59),   S(-21,  11),   S( -4,  16),   S(  5,   8),   S( 17,  14),   S( 36,  35),   S(  4, -16),   S( -5, -21),

            /* knights: bucket 8 */
            S(  1,  10),   S( 11,   5),   S( 11,   3),   S(-20, -52),   S(  5,  -6),   S(  7, -45),   S( 15,  25),   S( -9, -33),
            S( -9, -50),   S( -5, -53),   S(-17, -70),   S( -7, -16),   S(-14,   8),   S( -6, -14),   S( -3, -34),   S(-10, -38),
            S(-10, -67),   S(-19, -35),   S(  4, -70),   S( 24, -36),   S( 24, -25),   S( 23, -18),   S(  9, -18),   S(  0,   9),
            S(-21, -70),   S(-11, -20),   S( 13,  -2),   S( 31,  -1),   S( 21, -34),   S(  7, -12),   S(-20, -52),   S(-14, -29),
            S(-11, -47),   S( -1, -27),   S(  7,  -7),   S( 38,  -3),   S( 14,   0),   S(  4,  -7),   S( 21,  25),   S(  1, -13),
            S(  2,   1),   S(  8, -28),   S( 15,  -4),   S( 15,   3),   S( -8, -20),   S(  4, -31),   S( -7,  -6),   S(-12, -36),
            S(  1,  11),   S(  2, -39),   S(-15, -41),   S(  4,  -5),   S(  8, -14),   S(  5, -13),   S( -5,  20),   S( -2, -12),
            S(  1,   5),   S( -1,  -1),   S( -9, -24),   S( 11,  20),   S(-10, -19),   S( -2,  -7),   S( -1,   1),   S(  0,  -2),

            /* knights: bucket 9 */
            S( -7, -46),   S(  3,   1),   S( -9, -64),   S( -5, -57),   S(-14, -20),   S( -3,  18),   S( -2,  -4),   S( -8, -34),
            S(-16, -32),   S(-11, -54),   S(-21, -131),  S(-12, -73),   S( -8, -60),   S(-17, -88),   S(  1, -36),   S(-15, -19),
            S( -3, -32),   S(  0, -12),   S(-13, -49),   S(  2, -63),   S(  2, -34),   S( 37, -21),   S( -3, -14),   S( -6, -13),
            S(-19, -69),   S(-18, -66),   S( -6, -40),   S( -2, -74),   S( -7, -44),   S( 18, -34),   S(-11, -60),   S( 12, -24),
            S( -5,  12),   S(-21, -47),   S(  5, -49),   S( 12, -37),   S( -6, -80),   S( 28, -13),   S(-10, -22),   S(  1,   7),
            S(-14, -49),   S( -9, -62),   S( -2, -52),   S( 10, -70),   S( 14,  -6),   S(  4, -39),   S( -2, -14),   S(  2,   9),
            S(-14, -16),   S(  4,  -4),   S( -4,  -2),   S(-24, -62),   S( 21,  14),   S( -2, -11),   S( -6, -28),   S(-12, -17),
            S(  0,  -2),   S(  3,   5),   S( -1,  18),   S( -7, -29),   S( -3, -11),   S(  6,   8),   S(  5,  15),   S(  4,  16),

            /* knights: bucket 10 */
            S( -2, -25),   S(-13, -43),   S(-14, -54),   S(-10, -19),   S(-12, -70),   S( 13, -46),   S(-14,  -8),   S( -3,  -1),
            S( -2, -44),   S(  8, -12),   S(-13, -62),   S(-11, -87),   S(-11, -73),   S(-18, -92),   S( -2,   1),   S( -8,  25),
            S(  8,  -9),   S( 15, -32),   S(-11, -39),   S( 33, -39),   S(-11, -55),   S(  7, -38),   S(  7, -34),   S( -7,   1),
            S( -7, -57),   S(-11, -68),   S( 15, -47),   S( 10, -57),   S(  4, -44),   S(-12, -35),   S(  1, -63),   S( -7, -66),
            S(-28, -77),   S( 10, -42),   S( 13, -43),   S(  7, -42),   S( 15, -14),   S(  3, -62),   S( -2, -17),   S(  8, -52),
            S(  1,  12),   S( -8, -39),   S(  2, -30),   S(  0, -18),   S(-23, -52),   S( -7, -54),   S( -4, -29),   S(-13, -34),
            S( 11,  11),   S( -1,   4),   S( -8, -61),   S( 23, -25),   S( -7, -23),   S(-22, -43),   S( -7, -14),   S( -9, -21),
            S(  0,   0),   S(  3,   1),   S( -4,  -2),   S( -7, -26),   S( -3, -28),   S( -6, -25),   S(  7,  -6),   S(  1,   8),

            /* knights: bucket 11 */
            S(-11, -46),   S(-18, -52),   S( -8, -45),   S( 11,   8),   S(-45, -52),   S(-11, -40),   S(  5,  -2),   S( 10,  31),
            S( -2,  -8),   S(-32, -58),   S(-13, -71),   S( 31, -14),   S( 27, -18),   S(-15, -67),   S( -7, -32),   S(-12, -30),
            S( -3, -33),   S( -5, -46),   S( 28, -25),   S( 29, -18),   S(  8,  -9),   S( 17, -22),   S(  8, -43),   S( -9, -40),
            S(-10, -32),   S( 30,  14),   S( 12, -19),   S( 39, -28),   S( 42, -27),   S(  1, -32),   S( 17,   6),   S(  0, -15),
            S(-13, -19),   S( -4, -76),   S( -8, -23),   S( 26, -22),   S( 38,   3),   S( 24,   3),   S(-14, -86),   S( -7, -15),
            S(-10, -40),   S( -5, -63),   S(  5, -11),   S(  5, -23),   S( 10,   0),   S(  6, -16),   S(  1, -36),   S(  5,   4),
            S(  2,  -5),   S(  1,  15),   S(-12, -15),   S( 11,  11),   S( 22, -27),   S(  7, -46),   S(  3, -21),   S( -2,   3),
            S( -3, -14),   S(  3,   6),   S( -9, -22),   S(  3,  25),   S(  0,  15),   S(  1, -24),   S(  5,  19),   S( -1,  -3),

            /* knights: bucket 12 */
            S(-15, -47),   S(  1, -30),   S( -4, -17),   S(  1,  10),   S( -7, -15),   S(  0,  -2),   S( -1,   5),   S( -3, -10),
            S( -5, -28),   S(  2,   5),   S( -3, -11),   S( -1, -17),   S( 12, -15),   S(  7,  18),   S(  6,   1),   S(  2,   0),
            S( -2, -10),   S( -7, -33),   S( -9, -34),   S( -9, -78),   S( -8, -33),   S(  6,  10),   S(  1, -11),   S(-10, -34),
            S(  3,  11),   S( -4, -69),   S(  0,   2),   S(  4, -32),   S( 12, -38),   S(  4,  -9),   S(  7,   4),   S(  7,  19),
            S(-13, -44),   S( -9, -43),   S(  5,  -5),   S(  4,  -3),   S( 18,  43),   S(  2,   2),   S( -4, -16),   S( -7,  -5),
            S( -1,  -3),   S( -5, -19),   S( -1, -13),   S( -2, -30),   S( -7, -25),   S( -2, -26),   S(  2,  -3),   S( -3, -15),
            S( -8, -24),   S( -3,  -2),   S( -6, -16),   S( -2,  15),   S(  2,  11),   S(-12, -35),   S(  1,  -3),   S( -7, -19),
            S(  0,   1),   S(  5,  34),   S(  1,   1),   S(  1, -12),   S(  5,  11),   S(  4,  13),   S( -1,  -7),   S(  2,   8),

            /* knights: bucket 13 */
            S( -2, -12),   S( -1,  10),   S( -9, -53),   S( -1, -13),   S( -2, -17),   S( -2,   7),   S(-15, -51),   S(  2,  14),
            S(  2,   6),   S( -8, -28),   S(  5,  18),   S( -8, -42),   S( -7, -48),   S(-10, -36),   S( -4, -13),   S( -6, -17),
            S( -2, -20),   S(  2,   7),   S( -3, -37),   S(  5, -21),   S(  7,  17),   S( -9, -20),   S(  3, -12),   S( -4, -17),
            S(  2,   4),   S( -3, -19),   S( -5, -14),   S( 13,  21),   S(  0, -54),   S( 16,   3),   S(  4,  -7),   S( 11,   8),
            S(  4,  26),   S(  4, -26),   S(  2, -35),   S( -3, -69),   S( -7, -38),   S(  9, -12),   S( -7, -55),   S( -5, -23),
            S( -3,  -5),   S(  2,   1),   S(  2, -11),   S( 12,   0),   S( -3,  -8),   S(  1, -18),   S( -5, -11),   S( -2,  -5),
            S(  3,   8),   S(  3,  10),   S( -6,   0),   S( -8, -36),   S(  2,  15),   S( -2,  -6),   S(  4,   9),   S(  2,   7),
            S(  0,   0),   S(  0,   3),   S( -3,  -9),   S( -1,  -5),   S(  4,   9),   S(  2,   4),   S(  1,   7),   S(  1,   3),

            /* knights: bucket 14 */
            S(  0,  -7),   S(  3,   6),   S( 13,  30),   S(  0,   6),   S( -6, -42),   S(  0,   3),   S(  6,  10),   S(  0,  -7),
            S( -3, -14),   S(-11, -38),   S( -2, -26),   S( -3, -39),   S(  5, -17),   S( -1,  -3),   S( -3, -25),   S(  6,  38),
            S( -7, -25),   S(-13, -60),   S(  8,  -4),   S(-17, -76),   S( -4, -14),   S(  3,   1),   S( -4, -27),   S(  3,   9),
            S(  1,   3),   S(  1, -23),   S(-12, -46),   S(  4,  -2),   S(  4, -12),   S( -8, -59),   S( -4, -16),   S(  1,  30),
            S(  3,   3),   S(-17, -70),   S(-13, -57),   S( -7, -29),   S(  2,  30),   S( -1, -33),   S( -6, -29),   S(  3, -14),
            S(  0,   0),   S(  3,   4),   S(  2,  47),   S(  6,  -4),   S( -9, -44),   S(-11, -20),   S( -5,  -3),   S( -8, -24),
            S( -2, -10),   S(-12, -36),   S(  6,  -3),   S(  0,  23),   S( -1,  10),   S( -5, -23),   S(  3,   0),   S(  2,   3),
            S(  2,   3),   S(  0,   2),   S(  1,   2),   S( -1,  -2),   S( -2,  -4),   S( -1,  -4),   S(  1,   5),   S(  1,   4),

            /* knights: bucket 15 */
            S( -3, -15),   S(  3,   8),   S(  3,  22),   S( -6,   2),   S( -2, -22),   S(-10, -31),   S( -3, -31),   S(  1,   0),
            S( -2,  -3),   S(  7,  -5),   S( -3, -13),   S(  7,  11),   S(  5, -10),   S( -2, -25),   S( -3, -14),   S(  1,   2),
            S(  1,  -6),   S( -5, -31),   S(  5, -10),   S( 21,   7),   S(-19, -84),   S(  2, -16),   S( -4, -29),   S( -1, -10),
            S(  1, -19),   S( -3,  -5),   S(  0, -14),   S( -3, -22),   S(  4, -25),   S(  2,   4),   S( -4, -31),   S( -4,  -9),
            S(  3,  12),   S( 16,  46),   S( -8, -41),   S( -7, -19),   S(  9,  -8),   S(  6,  15),   S(  6,  -5),   S(  1,   3),
            S( -1,   0),   S( -3,   2),   S(  1,   9),   S(-10,  -8),   S( -8, -23),   S(  4,  44),   S(  4,  13),   S( 10,  32),
            S( -4, -12),   S( -5, -12),   S(  6,  26),   S(  7,  17),   S( -4, -11),   S(  6,  11),   S(  0,  -3),   S(  6,   7),
            S(  1,   4),   S( -1,  -6),   S(  4,  20),   S( -2, -10),   S(  4,  19),   S(  1,   9),   S(  0,   7),   S(  0,   0),

            /* bishops: bucket 0 */
            S( 23,   6),   S( -8,  23),   S(-18,  16),   S(-23,  -7),   S( 10,   1),   S( 12,   6),   S( 85, -68),   S( 33, -23),
            S(-26,  24),   S( -2, -14),   S( -5,  32),   S( 12,  10),   S( 15,   4),   S( 68, -31),   S( 41,  44),   S( 54, -17),
            S( 11,  19),   S( -2,  44),   S( 21,  11),   S( 16,  13),   S( 36,   0),   S( 46,  36),   S( 42, -19),   S( 28,  -2),
            S( 16, -25),   S( 49, -38),   S( 34,  -6),   S( 68, -26),   S( 77,  26),   S( 40,  34),   S( 30, -21),   S( 26,  40),
            S( 28,   2),   S( 51, -25),   S( 98, -27),   S( 95,   2),   S(129,  -7),   S( 32,   9),   S( 60,   4),   S( 19,  -9),
            S( 37,   1),   S(106,  -1),   S( 74,  28),   S( 70,   9),   S( 30,  33),   S( 58,  19),   S( 62,  15),   S(-19,  28),
            S(-50, -106),  S( 41, -16),   S( 76,  29),   S(  2,  -1),   S( 22, -18),   S( 23,   7),   S( -7,  45),   S(-11,  27),
            S(  0, -38),   S(-14,   1),   S( -8, -37),   S(-15, -25),   S( 11,  -8),   S(-23,   7),   S( -6,  21),   S(-15, -30),

            /* bishops: bucket 1 */
            S(-59,  23),   S( 21,  -4),   S(-22,  32),   S( 33, -13),   S(  3,   5),   S( 26,  14),   S( 27,  15),   S( 74, -14),
            S( 21, -18),   S( -4, -14),   S( 11,  -2),   S(-10,  13),   S( 42, -12),   S( -1,   6),   S( 69, -15),   S( -6, -12),
            S(-27,   9),   S( 41,  -5),   S( -1,  -5),   S( 34,  -5),   S( 12,  -6),   S( 59, -11),   S(  7,   0),   S( 83, -17),
            S( 23,  -6),   S( 12,  -6),   S( 36,   8),   S( 22, -10),   S( 64,  -9),   S(  8,  -6),   S( 80, -29),   S( -7,  14),
            S( 23, -29),   S( 64,  -4),   S( 20,   2),   S(104, -20),   S( 55, -18),   S( 98, -31),   S( 17,   4),   S( 33,  -3),
            S( 91, -35),   S( 67, -18),   S( 87, -28),   S(108, -55),   S(126, -38),   S(-36,  22),   S( 19,  23),   S(  1, -27),
            S( -1, -62),   S( 29, -18),   S(-46, -49),   S( -9, -50),   S( 66,   3),   S(-10,  24),   S( 32, -32),   S(-46,  17),
            S(-16,  -9),   S(  0,  -8),   S( 13, -31),   S(-61, -10),   S( 14, -28),   S( 32, -24),   S( 15,   4),   S(-41, -26),

            /* bishops: bucket 2 */
            S( 26, -16),   S(-19,  12),   S(  5,  17),   S( -6,  15),   S( 26,  25),   S( -6,  21),   S( 23,  -6),   S( 20,  36),
            S( 22, -10),   S( 17, -10),   S( -1,   8),   S( 19,  13),   S( -3,  27),   S( 13,  16),   S( 11,   4),   S( 17, -30),
            S( 39,   8),   S( 23,   2),   S(  4,  28),   S(  3,  11),   S(  6,  32),   S(  6,  -4),   S(  1,  -1),   S(  4,   7),
            S( 19,  -8),   S( 56,  -7),   S(  9,  12),   S( 38,  23),   S( 12,  18),   S(-15,  29),   S(-22,   4),   S(  7,  16),
            S(  8,  16),   S( 11,  22),   S( 66,   0),   S( 35,  11),   S( 22,  28),   S( 26,  14),   S(  9,  35),   S( 31,   7),
            S(-32,  48),   S( 11,  32),   S( 10,   3),   S( 99, -38),   S( 83, -12),   S( 93,   5),   S( 56,   9),   S( 16, -36),
            S(-19,  52),   S( 37,  -1),   S(  8,  18),   S( 31,  13),   S(-91, -37),   S(-59,  -9),   S(  0,  26),   S(-13, -37),
            S(-40, -25),   S(-33,  27),   S( 12,   3),   S(-51,  20),   S(-51, -24),   S(-56,  -8),   S(  2, -22),   S(-86, -12),

            /* bishops: bucket 3 */
            S( 23,  35),   S( 44,  -9),   S(  2,  12),   S(  5,  25),   S(  9,  46),   S( -4,  61),   S( -4,  70),   S(  0,  24),
            S( 30,  30),   S( 17,  17),   S( 13,  26),   S( 14,  32),   S( 16,  38),   S( 10,  38),   S(  7,  29),   S( 18,   8),
            S( -7,  29),   S( 23,  58),   S( 13,  63),   S( 18,  44),   S( 13,  54),   S( 16,  37),   S(  6,  27),   S( 11,  29),
            S(-14,  23),   S( 10,  43),   S( 20,  64),   S( 36,  54),   S( 37,  42),   S(  7,  35),   S( 24,   0),   S( 24,  -3),
            S( -1,  36),   S(  8,  63),   S(  4,  63),   S( 45,  72),   S( 34,  69),   S( 44,  31),   S(  9,  48),   S(-17,  56),
            S( -1,  49),   S( 22,  68),   S( 20,  52),   S(  7,  55),   S( 69,  34),   S( 54,  81),   S( 33,  72),   S( 13,  91),
            S(-23,  80),   S( 14,  67),   S( 12,  43),   S(-16,  64),   S( -8,  78),   S( 22,  81),   S(-49,  -2),   S(  2, -17),
            S(-57,  69),   S(-43,  68),   S(-79,  71),   S(-25,  72),   S(-25,  57),   S(-118,  77),  S(-20,   5),   S( -5,  37),

            /* bishops: bucket 4 */
            S( -9,  46),   S( -4,   4),   S(-54,  35),   S(-20,   1),   S(-29, -34),   S(  8, -27),   S(-16, -29),   S(-22, -27),
            S(-17,  57),   S( 26,   6),   S( 73, -26),   S( -5,  -1),   S(-49,  -4),   S( 13, -39),   S(-16, -17),   S(-17, -31),
            S(  3,  39),   S(-36,  18),   S( 63, -16),   S( 35, -31),   S( 18, -17),   S(-31,   4),   S(-17, -30),   S(-68,   7),
            S( 37, -18),   S( 45,  -6),   S( 35, -27),   S( 39,   5),   S( -5,  -9),   S( 40, -10),   S(-37, -22),   S( -3, -22),
            S( -2, -14),   S( -8, -87),   S( 23, -53),   S( 62, -35),   S( 46, -19),   S( 19, -18),   S(-24,  29),   S(-56,  -3),
            S(-70, -73),   S(-30, -62),   S( 24, -38),   S( 39, -27),   S(-19,  -5),   S( 35,  -3),   S(-22,  -3),   S(-24,   4),
            S( 10,  15),   S(-19, -37),   S( 12, -60),   S(-12, -31),   S(-16, -12),   S( 19,  12),   S(  0,   6),   S( 10,   4),
            S(-19, -20),   S( -9, -37),   S( 12, -14),   S( -5, -46),   S( -1, -52),   S(-16,  -7),   S(  6,  51),   S(  4,  19),

            /* bishops: bucket 5 */
            S(-51,  22),   S(-23,  -2),   S(-38,  -6),   S(-26,   5),   S(  4,  -6),   S(-12, -30),   S(-39, -19),   S(-47,   0),
            S(-32, -19),   S(-28,   0),   S( 23, -16),   S( 26, -16),   S(-23,  -9),   S(  2, -18),   S(-29,  -5),   S(-10,  -8),
            S( -6,  11),   S(-30,  -9),   S( 20, -13),   S(-21,  -9),   S( 32, -15),   S(-52,  -4),   S( 37, -10),   S( 13,  -5),
            S( 33, -20),   S(  2, -12),   S( 60, -40),   S( 86, -36),   S(-29,  -4),   S( 48, -33),   S(-71,  -2),   S( -9,   2),
            S( 51, -41),   S( 24, -28),   S(-17, -38),   S(-11, -54),   S( 24, -57),   S( 17, -30),   S( 35, -10),   S(-23, -22),
            S( -8, -19),   S(-32, -11),   S(  8, -54),   S(-24, -47),   S(-36, -28),   S( 31, -23),   S(  6, -12),   S(-15,  21),
            S(  9, -33),   S(  4, -35),   S(-17, -39),   S(-12, -13),   S( 24, -47),   S( 14, -17),   S(  3,  14),   S( -9,   3),
            S(-21, -40),   S(-24, -38),   S(-26, -32),   S( 11, -33),   S(-32,  -5),   S(  3, -11),   S(-34, -27),   S( -9,  10),

            /* bishops: bucket 6 */
            S(-41, -14),   S(-34,  -1),   S(-41,   0),   S(  8,  -6),   S( -4,   6),   S( 10,  -3),   S(-36,  28),   S(-63,  29),
            S(-17,   9),   S(-23, -26),   S(-37,   6),   S( 16,  -6),   S( -4,  -1),   S(-22,  -1),   S( -4,   6),   S(-20,  21),
            S( 63, -26),   S(-37,   3),   S( 42, -30),   S( 21,  -7),   S(  9,   5),   S( -7,  -9),   S(-41,   0),   S(-31,  22),
            S(-19,   8),   S(-16, -11),   S(  8, -10),   S( 69,  -7),   S( 75, -21),   S( 56, -22),   S( 26, -33),   S(-16, -16),
            S(-40,   3),   S( 12,  -9),   S( 45, -21),   S( 80, -33),   S( -5, -48),   S(  2, -42),   S( 42, -30),   S( 19, -18),
            S(-20,  19),   S( 18, -13),   S( 14, -14),   S( 30, -31),   S(  9, -24),   S( -2, -30),   S(  3, -15),   S(-20, -20),
            S( -8,  17),   S(-10,  15),   S( 45, -27),   S(  7, -28),   S(-32, -19),   S( -3, -13),   S(  5, -27),   S(-56,  -4),
            S(-34,  -2),   S(-24, -12),   S(-18,   0),   S( 14,  -9),   S(-16, -14),   S( 23, -36),   S( -9, -15),   S( -7, -37),

            /* bishops: bucket 7 */
            S(  1, -25),   S(-47,  -3),   S(-27, -31),   S(-15,  -6),   S(-38,   4),   S( -8,  -7),   S(-59,  -8),   S(-60,  16),
            S(-29, -35),   S(-31,  -9),   S( 13, -16),   S(-10, -22),   S(-16,   6),   S( -6, -12),   S(-46,  -2),   S(-19,  40),
            S(-28, -10),   S(-20,  -3),   S(  7,  -1),   S( 52, -27),   S( 38, -31),   S( 11, -16),   S(-90,  28),   S(-82,  66),
            S(-33,  -9),   S(-31,   3),   S(-28,  11),   S( 47, -23),   S(103, -14),   S( 30, -28),   S( 57, -37),   S(-35, -11),
            S(-57,  14),   S( 31,   0),   S( 27, -20),   S( 38, -20),   S( 62, -19),   S( 42, -31),   S(-39, -40),   S(-63, -54),
            S(-52,  15),   S( -9,  22),   S( 13,  -2),   S(-70,   9),   S(-44,  14),   S( 69, -34),   S( 28, -30),   S(-72, -93),
            S(-39,  -1),   S(-29,  32),   S(-46,  16),   S( 48, -10),   S(  8, -17),   S(  2, -38),   S( 25,  -3),   S(-30, -36),
            S(-24, -19),   S(-16,   6),   S( -8,   8),   S( -3,   1),   S(-13, -11),   S( -8, -12),   S( 41, -65),   S( -2, -19),

            /* bishops: bucket 8 */
            S( 33, 158),   S(-20,  -3),   S( 15,  22),   S(-20,  27),   S( 12,  42),   S(  0, -42),   S(-19, -35),   S( -3,  -8),
            S(  5,  39),   S( 16,  78),   S( 12,  14),   S( 13,  12),   S(  3,  -2),   S( -6,   8),   S(-19, -21),   S( -3, -15),
            S( -5, -19),   S(-16, -66),   S( 26,  55),   S( 23, -19),   S( 43,  32),   S( 10,  -2),   S( -7,  -6),   S(-41, -69),
            S( -3, -12),   S( 11,  51),   S( -3, -12),   S( 10,  30),   S( 33,  21),   S( 20,   5),   S(-10,  12),   S( -9, -31),
            S(  8,  65),   S(  3,  58),   S(  5,  21),   S( 26,  13),   S(  9,  37),   S(-23,  -4),   S(  7, -26),   S(  2,  25),
            S(-14, -20),   S(  7,  32),   S(  6,  42),   S( 11,  42),   S( -4,  25),   S(  9,  45),   S(  4,  45),   S(  7,  40),
            S( -7,  -2),   S(-13, -50),   S( 17,  20),   S( -2,  38),   S( -3,  17),   S( 11,  47),   S(  6,  69),   S(-33, -11),
            S(  1,  17),   S( -1,  -1),   S( -2,  -5),   S( -2,   9),   S( 24,  60),   S( 13,  52),   S( -4,  29),   S( 21, 113),

            /* bishops: bucket 9 */
            S(  7,  69),   S( -6,  48),   S(-14,   3),   S(-54, -25),   S(-30, -23),   S(-12, -41),   S(-10,  -6),   S(-11, -20),
            S( -8,  22),   S(-16,   9),   S( 21,  21),   S(-27, -39),   S(-35, -46),   S( -9, -46),   S(-18, -40),   S(-18, -61),
            S(-15, -46),   S( -5, -10),   S(-23, -44),   S(-16, -19),   S(  3,  -9),   S(-16, -20),   S(  9, -12),   S(-12, -19),
            S(-10,  10),   S(-23,  -1),   S( -6,  -9),   S( -4,   2),   S(-38,  -7),   S( 10,  -4),   S( -2,   3),   S( -4, -32),
            S( -1,  21),   S(-16,  -2),   S( 15,  -9),   S(-21, -27),   S(-25,  -7),   S(  5, -23),   S(  2,  -1),   S(-13, -57),
            S( -1,  -4),   S(-29, -11),   S(-23,  -6),   S(  9,   9),   S(-34, -23),   S(-13,  11),   S(-17, -13),   S( -2,  23),
            S( -9,  15),   S(-19,   5),   S( 12,  11),   S(-17,  -1),   S(-15, -16),   S( -8,   1),   S( -1,   6),   S( -8,  33),
            S( -1,  20),   S( -4, -10),   S(-11,  -6),   S(  2, -21),   S(  0,  -4),   S(-11,  28),   S(-17,  35),   S( 19,  74),

            /* bishops: bucket 10 */
            S(-15, -28),   S( -2,  45),   S(-27, -36),   S(-30, -48),   S(-45, -39),   S(-40, -42),   S(-19,  24),   S(-10,   8),
            S(  3,  31),   S(-11, -12),   S(-18, -52),   S(-40, -67),   S( -5, -39),   S(-38, -50),   S(-15, -36),   S( -4,  13),
            S(-12, -45),   S(-19, -39),   S(-26, -16),   S( -7,  -5),   S(-24, -52),   S(-16, -47),   S(-30, -14),   S(  7, -34),
            S( -3, -20),   S(-25, -38),   S(-34, -44),   S( -6, -22),   S(-13, -16),   S( -3, -20),   S( 14,  23),   S(-25, -21),
            S(-14,  -3),   S(-48,   7),   S(-10, -34),   S( 15, -15),   S(  6, -14),   S( -4,   0),   S(-37, -19),   S( -9,  -6),
            S(-15,  38),   S( -8,   0),   S(-25,   6),   S(-31, -12),   S(-13, -30),   S( -7,  -8),   S(-16,  -7),   S(  4,   2),
            S( -1,  -9),   S(-23, -19),   S(-24,  -6),   S(-12,  15),   S( -6,  -5),   S(-22, -41),   S( -9,  -7),   S(  8,  58),
            S( -5,  43),   S( -9,  44),   S(  0,  26),   S(-12, -23),   S(-22,  11),   S(-10,  -8),   S(  5,  -6),   S(  3,  19),

            /* bishops: bucket 11 */
            S( 10,  22),   S( -1,  -7),   S( 11,  -8),   S( -9,  16),   S(-31, -15),   S(-14,   5),   S(-24,   1),   S(-12, 128),
            S( -7,   2),   S( 24,  -1),   S( -5,  17),   S(  4, -19),   S( -8, -17),   S(-53, -15),   S(-32,  26),   S( 11,  48),
            S(  4, -31),   S(  1, -13),   S( 29, -30),   S(-23, -40),   S( -1, -18),   S( 18,  28),   S( -4, -44),   S( -5, -23),
            S( -1, -11),   S(-16, -29),   S( 14,   0),   S(-19, -34),   S( -6,   6),   S( 28,  41),   S( -4,  23),   S(-11, -35),
            S(-13,   8),   S(-27, -31),   S(-28,   0),   S(-34,  18),   S( -1,   1),   S( 23,  21),   S(-17,  28),   S( -3,  43),
            S(-14, -19),   S(-20,   5),   S(-26,  32),   S(-11,   2),   S(-12,  57),   S(-22,  14),   S(  4,  -2),   S(-13,  19),
            S(-13,  -7),   S(-12,  72),   S(-16,  31),   S(  6,  37),   S( 14,  14),   S(  6,  -8),   S( -9, -26),   S( -7,  -4),
            S( 11, 105),   S(-18,   5),   S(  2,  43),   S( -2,  40),   S(  7,  32),   S( -6,  11),   S(  4,  21),   S(  2,   3),

            /* bishops: bucket 12 */
            S( -2, -10),   S( -5, -29),   S( -2,  -9),   S(-12,   6),   S( -7, -13),   S(-10,   0),   S(  1,   9),   S( -2, -11),
            S( -1,   4),   S( 16,  53),   S( -2,   6),   S( -3, -22),   S( 11,  20),   S( 12,  11),   S(-11, -28),   S( -1,  -8),
            S(  5,  32),   S( 11,  46),   S( 12,  39),   S( 31,  32),   S( 11,  28),   S( -5, -50),   S(  0,  -5),   S(  2,   5),
            S(  7,  63),   S(  2,  50),   S( -3,   1),   S( 11,  34),   S( 12, -14),   S(  0, -12),   S(  1,  12),   S(  6,  25),
            S(  2,  -5),   S( 11,  51),   S(  5,  34),   S( 11,  33),   S( 11,  23),   S( 15,  41),   S( 16,  12),   S(  8,  14),
            S(  1,   2),   S( -9, -29),   S( -9,   8),   S( -3,  15),   S( 10,  42),   S( 11,  42),   S( -9, -20),   S(  2,  -7),
            S( -3,  -3),   S(  0,   9),   S( -2,  -8),   S(  6,  25),   S(  3,  33),   S( 23,  63),   S( 13,  39),   S(  8,  37),
            S(  0,   8),   S( -4,  -7),   S( -3,  -6),   S( -4, -11),   S(  7,  32),   S(  0,  20),   S(  4,  39),   S(  8,  46),

            /* bishops: bucket 13 */
            S( -3,   1),   S( -3,  35),   S( -8, -20),   S( -5,  17),   S(  5,  23),   S( -7, -31),   S(-17, -35),   S( -5, -27),
            S( -3,  25),   S( -3,  12),   S(-10, -16),   S( 10,  70),   S(-14, -18),   S(  3,   9),   S(  4,  -5),   S(  1,   2),
            S( -3,  10),   S( 21,  88),   S( -4,  -1),   S( 20,  40),   S( -9, -30),   S( 20,  32),   S(-12, -27),   S(  6,  -1),
            S( 11,  72),   S( 17,  87),   S(  5,  46),   S(-12, -16),   S( 11,  60),   S( 13,  -2),   S(  9,  44),   S(  5,  12),
            S(  6,  70),   S(  3,  24),   S(  2,   8),   S( -1,  24),   S(  5,  15),   S(  4,  30),   S(  5,  18),   S( -4,  -8),
            S( -6,  25),   S( -3,   6),   S( -8,  16),   S(  9,  24),   S( -5,  39),   S(-11, -22),   S( -4,  -8),   S(  6,  33),
            S(  8,  28),   S( -3,   0),   S( -7, -17),   S(  0,  27),   S( -8,  -5),   S(  5,  61),   S( 11,  29),   S(  3,  39),
            S(  3,   7),   S( -3,   1),   S( -7, -24),   S(  3,  12),   S( -4,   7),   S( -2, -23),   S(  7,  42),   S( 19,  83),

            /* bishops: bucket 14 */
            S( -4,   4),   S(  6,  31),   S(  5, -14),   S( -3,  35),   S(-21, -49),   S(  2,  14),   S( -8, -32),   S( -6, -15),
            S( -4, -12),   S(  7,   8),   S(  6,  28),   S( -1,  -3),   S(  6,  31),   S(  4,  39),   S(  7,  58),   S(  0,  20),
            S( -2,   6),   S( -4,  -7),   S(-13, -24),   S(  6,   1),   S(  4,   7),   S( -4,  45),   S(  1,  43),   S( -4,  28),
            S(  8,  44),   S( -7,  -9),   S( -6,  10),   S(-13,  18),   S( -2,  12),   S(  8,  36),   S( 14,  82),   S( 11,  88),
            S(  1,  25),   S( -5,  37),   S( -6,  18),   S( -7,  16),   S(-11, -36),   S( -2,  32),   S( 12,  31),   S(  5,  52),
            S( -3,  17),   S(  9,  41),   S( -2,  22),   S(  1,  24),   S(  2,  39),   S( -6,  -4),   S(-10, -15),   S(  1,  17),
            S(  8,  38),   S(  4,  21),   S(  3,  55),   S(  4,  18),   S( -4,   2),   S(  1,  21),   S( -4, -30),   S(  4,  33),
            S( -3,  24),   S( -1,  24),   S( -1,  14),   S(  7,  27),   S( -7, -17),   S( -3, -19),   S( 10,  32),   S(  4,  16),

            /* bishops: bucket 15 */
            S(  0,   6),   S(  0,  -1),   S( -7, -16),   S(  0,  -4),   S( -5, -20),   S(  4,  -3),   S( -4, -34),   S( -7, -19),
            S(  8,  42),   S( -1,   6),   S(  0,  -1),   S(  3,  11),   S( 19,   9),   S(  0,  15),   S( -1,   8),   S( -3,   1),
            S(  4,   3),   S(  4,   3),   S(  4,   1),   S( 15,  12),   S( 10,  12),   S(  9,  21),   S( 15,  60),   S(  4,  36),
            S(  0,   2),   S(  8,  23),   S(  3,  29),   S(-16, -16),   S(  1,   7),   S( 16,  48),   S(  9,  40),   S(  5,  52),
            S( -7,  -8),   S( -7,   5),   S(  8,  31),   S( 20,  66),   S(  7,  48),   S( 12,  56),   S( -2,  20),   S( -1,   7),
            S( -6, -17),   S( -7,   3),   S(  8,  51),   S(  7,  48),   S( 13,  36),   S( -1,  19),   S(  1,  14),   S( -2,  -8),
            S(  4,  23),   S(  7,  41),   S( -8,  19),   S( 10,  39),   S( 13,  43),   S( -2,   5),   S( -2,   6),   S(  0,   4),
            S( -2,   0),   S( -3,  30),   S(  7,  39),   S(  6,   3),   S(  1,  18),   S( -4,  -1),   S(  1,  -1),   S(  4,   6),

            /* rooks: bucket 0 */
            S(-10,  15),   S( 11,  10),   S(  9,   6),   S(  9,   9),   S(-12,  67),   S(  0,  39),   S(-38,  70),   S(-48,  50),
            S( -3,   6),   S(  0,  22),   S(-37,  55),   S( 13,  33),   S(  7,  46),   S(  7,  22),   S(-11,   1),   S(-34,  71),
            S( 17,  -6),   S(  9,   0),   S( -1,  32),   S( 10,   1),   S(-24,  64),   S(-36,  54),   S(-21,  48),   S(-10,  44),
            S(-14,   4),   S( 37,  18),   S(-48,  56),   S( 13,  37),   S( 27,  46),   S(-45,  56),   S(-15,  49),   S(-24,  36),
            S( 23, -28),   S( 22,  36),   S( -2,  38),   S(  5,  63),   S( 14,  58),   S( 33,  60),   S( 43,  48),   S( 11,  52),
            S( 40,   0),   S(103,  32),   S( 68,  45),   S(106,  37),   S( 34,  65),   S( 60,  47),   S( -7,  70),   S(-37,  77),
            S( 21,  37),   S( 70,  87),   S(120,  47),   S( 92,  33),   S( 68,  57),   S( 36,  74),   S(  5,  69),   S(-10,  80),
            S( 25,   0),   S( 32,  27),   S( 22,  61),   S( 34,  13),   S( 93,  37),   S( 84,   9),   S( 50,  23),   S( 41,  14),

            /* rooks: bucket 1 */
            S(-59,  48),   S(-30,  17),   S(-21,  45),   S(-38,  46),   S(-33,  58),   S(-39,  58),   S(-54,  79),   S(-90,  90),
            S(-83,  63),   S(-41,  23),   S(-28,  44),   S(-43,  61),   S(-35,  32),   S(-71,  61),   S(-37,  19),   S(-54,  58),
            S(-43,  57),   S(-14,   9),   S(-37,  26),   S(-35,  38),   S(-48,  52),   S(-62,  44),   S(-64,  74),   S(-13,  38),
            S(-83,  73),   S(-34,  43),   S(-19,  41),   S(-51,  38),   S(-54,  63),   S(-88,  95),   S(-48,  64),   S(-87,  96),
            S(-38,  55),   S( -6,  18),   S( 17,  27),   S( 45,  12),   S(-17,  47),   S(-26,  84),   S(-23,  69),   S(-18,  79),
            S( 67,  25),   S( 44,  15),   S( 44,  29),   S( -5,  66),   S(-13,  56),   S(-13,  78),   S( 21,  61),   S( 29,  73),
            S( 41,  60),   S( 38,  22),   S( 39,  63),   S( 32,  74),   S( 49,  53),   S( 27,  58),   S( 41,  85),   S( 57,  79),
            S( 68,   2),   S( 52,   2),   S(  2, -12),   S(-13,  11),   S( 40,  23),   S(-13,  52),   S( 65,  41),   S( 88,  29),

            /* rooks: bucket 2 */
            S(-64,  89),   S(-52,  79),   S(-40,  82),   S(-33,  54),   S(-20,  60),   S(-31,  42),   S(-31,  40),   S(-77,  85),
            S(-58,  80),   S(-55,  77),   S(-38,  78),   S(-46,  69),   S(-58,  72),   S(-46,  42),   S(-34,  37),   S(-50,  67),
            S(-44,  84),   S(-22,  70),   S(-51,  66),   S(-41,  67),   S(-57,  72),   S(-36,  47),   S(-18,  47),   S(-10,  46),
            S(-52, 103),   S(-46,  90),   S(-67, 104),   S(-91,  96),   S(-64,  76),   S(-49,  66),   S(-32,  52),   S(-25,  61),
            S(-38, 101),   S(-30, 105),   S(-31, 110),   S(-36,  79),   S(-41,  83),   S( 12,  59),   S(-34,  77),   S(-22,  81),
            S(  1, 105),   S( 14, 101),   S( 31,  84),   S(-19,  88),   S( 51,  64),   S( 24,  89),   S(120,  36),   S( 95,  62),
            S( 77,  88),   S(  4, 110),   S( 57,  76),   S( 63,  49),   S( 23,  39),   S( 38,  94),   S(-35, 121),   S( 38,  94),
            S( 22,  66),   S( 37,  82),   S( 57,  55),   S(-18,  59),   S(-48,  55),   S( 33,  66),   S( 25,  55),   S( 44,  53),

            /* rooks: bucket 3 */
            S( -9,  92),   S( -5,  92),   S( -4, 125),   S(  3, 114),   S( 13,  84),   S( 17,  73),   S( 26,  53),   S( -7,  36),
            S( 10,  83),   S(  1,  96),   S(  2, 119),   S(  1, 121),   S(  8,  77),   S( 15,  52),   S( 48,  29),   S( 25,  46),
            S( 21,  87),   S(  1, 110),   S( -6, 117),   S(  0, 119),   S(  2,  91),   S(  6,  75),   S( 47,  55),   S( 29,  51),
            S( -1, 117),   S(-13, 136),   S(-19, 141),   S(-12, 133),   S( -9, 100),   S( -2,  91),   S( 28,  73),   S(-10,  75),
            S( -1, 131),   S(-15, 147),   S( 23, 141),   S( 17, 134),   S( -2, 120),   S( 26,  99),   S( 48,  84),   S( 20,  97),
            S(  8, 142),   S( 24, 134),   S( 33, 146),   S( 48, 127),   S( 92,  92),   S(120,  78),   S( 86, 100),   S( 32,  97),
            S( 24, 143),   S( -5, 154),   S( 34, 149),   S( 20, 149),   S( 43, 127),   S(119,  81),   S(128, 132),   S(201, 102),
            S(100,  23),   S( 73,  84),   S( 40, 139),   S( 36, 121),   S( 17, 114),   S( 94,  81),   S( 69,  68),   S(105,  58),

            /* rooks: bucket 4 */
            S(-64,   5),   S( 12, -17),   S(  4, -23),   S(-16,  -6),   S(-60,  35),   S(-43,  38),   S(-36,   4),   S(-70,  47),
            S(-68, -18),   S(-50,   0),   S(  5, -22),   S(-15, -19),   S(-51,  21),   S( 11,   4),   S(-46,  18),   S(-16,  18),
            S(-15, -31),   S(-37, -19),   S(-15, -21),   S(-23, -29),   S(-86,  25),   S(-50,  24),   S(-32,  -8),   S(-70,  -1),
            S(-40, -36),   S( 27,  -2),   S(  5, -27),   S( 26, -27),   S( 62, -15),   S(  0,   7),   S( -3,  -7),   S(-23,  -3),
            S( -7, -28),   S( 14, -41),   S( 58,  -6),   S( 51, -26),   S( 96,  -8),   S( 41,  17),   S( 11,   5),   S( 15,   0),
            S( -8,  -9),   S(  3,  -6),   S( 26,  22),   S( 11,  -4),   S( 45,  18),   S( 43,  20),   S( 55,  25),   S( 16,  33),
            S(-10, -33),   S( 45,  17),   S( 53,  -9),   S( 50, -20),   S( 32,  -2),   S( 14,  -8),   S( 31, -23),   S( 21,   7),
            S(  5, -12),   S( 29,  -2),   S( 16, -28),   S( -1,   3),   S( 40,   7),   S( 16,  16),   S(  3, -19),   S( 22,   6),

            /* rooks: bucket 5 */
            S(-53,  47),   S( -3,  -7),   S( 15,  -2),   S( 17,   9),   S(-16,  28),   S(-20,  40),   S(-33,  54),   S(-58,  43),
            S(-21,  18),   S(-54,   3),   S(-18,  -7),   S( -5,  -2),   S(-33,  16),   S(-23,  15),   S(-62,  23),   S(-18,  38),
            S(-77,  40),   S(-34,   0),   S(  5, -17),   S(-36,   9),   S(-30,  13),   S( 26, -19),   S(-44,  23),   S(-77,  40),
            S(-61,  28),   S( -4,  21),   S(  8,  -3),   S( 29,  15),   S(  4,  12),   S(-21,  33),   S(-13,  22),   S(-28,  50),
            S( 56,   5),   S(  1,  21),   S( 16,  50),   S(  9,  18),   S( 23,  16),   S( 63,  10),   S( 23,  25),   S( 55,  21),
            S( 48,  19),   S( -9,   4),   S( 10,  22),   S(  4,  14),   S( 32,  44),   S( 24,  31),   S( 38,  30),   S( 31,  44),
            S( 44,   3),   S( 38,  -5),   S( -6,  16),   S( 34,  36),   S( 23,   6),   S( 10,   0),   S( 63,   7),   S( 54,   0),
            S(  1,  24),   S(  3,   8),   S( 34,  -7),   S(-14,  29),   S( 47,  14),   S( 24,  23),   S(  8,  40),   S( 12,  51),

            /* rooks: bucket 6 */
            S(-71,  56),   S(-34,  29),   S(-52,  49),   S(-28,  26),   S(-10,  20),   S(  3,  11),   S( -7,   8),   S(-68,  35),
            S(-90,  50),   S(-42,  28),   S(-37,  33),   S( -6,  13),   S( -3,   9),   S(-27,   5),   S(-22,  -2),   S(-67,  35),
            S(-85,  57),   S(-43,  36),   S(-23,  14),   S(-28,  18),   S(-10,  18),   S( -5,  14),   S(-23,   3),   S(-34,   5),
            S(-96,  65),   S(-51,  50),   S( 35,  12),   S( 39,  12),   S( 26,  15),   S(-14,  18),   S(-26,  21),   S(-35,  52),
            S(-42,  60),   S( 44,  26),   S( 83,  17),   S( 64,  11),   S( 39,   9),   S(  5,  37),   S( 18,  22),   S( 79,   8),
            S( 67,  18),   S( 66,  16),   S( 94,  12),   S( 22,  23),   S(  0, -12),   S( 26,  66),   S( 41, -10),   S( 95,  13),
            S( 13,  31),   S( 91,  -3),   S( 82,  -1),   S( 57,  -8),   S( 47,   1),   S( 27,  21),   S( 74, -15),   S( 56,  -8),
            S( 27,   6),   S( -4,  47),   S(  1,  32),   S( 72,  -5),   S( 46,  23),   S( 37,  39),   S( 63,  18),   S( 62,  25),

            /* rooks: bucket 7 */
            S(-115,  39),  S(-93,  34),   S(-77,  38),   S(-66,  40),   S(-50,  21),   S(-28, -11),   S(-48,  22),   S(-49,  -6),
            S(-93,  34),   S(-35,  -2),   S(-71,  31),   S(-98,  54),   S(-59,  11),   S(-15,  -7),   S(-22,  17),   S(-29, -28),
            S(-104,  32),  S(-90,  40),   S(-54,  24),   S(-73,  42),   S(-60,  31),   S(-43,  23),   S( 10, -13),   S(-36, -14),
            S(-66,  28),   S(  3,  -6),   S( -8,   6),   S( 69, -22),   S( 12,  -8),   S( 37,  -3),   S( 57,  14),   S( 31, -24),
            S(  6,  19),   S( 17,  25),   S( 58,   6),   S( 59,   6),   S(136, -29),   S(122, -33),   S( 92,  -3),   S(-51, -18),
            S(  3,  20),   S( 16,  13),   S(106,  -5),   S( 96, -10),   S(106, -13),   S( 76,   5),   S( 24,  19),   S( -5, -10),
            S(-51,  24),   S( 40, -13),   S( 54,  -2),   S( 74, -19),   S( 81, -24),   S(105, -27),   S( 78,   5),   S( 48, -30),
            S(-29,  -4),   S(-27,  23),   S(  2,  25),   S( 11,  12),   S( 45,  -1),   S( 53,   4),   S( 48,  12),   S( 31,  -2),

            /* rooks: bucket 8 */
            S( 10, -52),   S(  9, -55),   S( 42, -31),   S( 31, -28),   S(-12, -46),   S(-20, -36),   S( 16, -63),   S( -9, -28),
            S(-18, -80),   S(  4, -32),   S(  0,   2),   S(-15, -82),   S( -4, -78),   S(-14, -47),   S( 17,   3),   S(-35, -59),
            S( 11,  21),   S(  6,  12),   S( 12, -14),   S(-20,   1),   S( -5,   9),   S( 30,  25),   S( 27,  38),   S(-18, -34),
            S(  0,  -9),   S(  5,   4),   S(  2,  -5),   S( 21,  13),   S( 23,  40),   S( 26,  28),   S( -4,   0),   S(-16, -30),
            S( -9, -69),   S( 17,  15),   S( 22, -13),   S( 20,  14),   S(  5,   3),   S( -3, -23),   S(  7,  23),   S(-10,   4),
            S( -4, -14),   S( 10, -29),   S( 10, -34),   S(-10, -26),   S( -5,  11),   S(-22,  -9),   S(-25, -26),   S( -1,   6),
            S( 21,  -2),   S( 30,   5),   S( 11,  -1),   S( 30,  -6),   S( 10,  10),   S( 32,  40),   S( 15,  33),   S(  4,  -7),
            S(-11,  15),   S( 22, -12),   S( 11, -11),   S( 24,  44),   S(-19, -12),   S( 12,   7),   S( -5,  -1),   S(  3,  15),

            /* rooks: bucket 9 */
            S(  6, -92),   S( 17, -88),   S( 15, -80),   S( 26, -52),   S( 12, -78),   S(  6, -63),   S(-16, -29),   S( -3, -49),
            S(-45, -56),   S(-12, -66),   S(-11, -64),   S(-14, -54),   S(-18, -62),   S( -9, -18),   S( -2, -34),   S( -9, -51),
            S(-19, -34),   S( -8, -32),   S(  6,   0),   S(  1, -13),   S( -4, -46),   S(-14,  -1),   S(-11, -25),   S(-14,  -5),
            S(  4, -43),   S(  2, -14),   S( 13,   4),   S( -2, -14),   S(  4, -33),   S( 37, -22),   S( -6, -13),   S(  7, -33),
            S( 24, -48),   S(  3, -21),   S( -3, -45),   S( -4,  -3),   S(  3, -45),   S( -8, -17),   S( -2, -44),   S(-17, -43),
            S( -1, -39),   S(-25, -45),   S(-14, -33),   S( 24, -17),   S( 23,   3),   S(-12,  -5),   S(-21, -24),   S(  5, -20),
            S( 22,  -2),   S(  6, -43),   S( 12, -41),   S( -8,  -1),   S(  2, -25),   S( 15, -16),   S(  1,  -2),   S(-10, -44),
            S(-18, -15),   S(  8, -30),   S( 11, -17),   S( 10,  -4),   S(  3, -22),   S(  7,  37),   S(-10, -13),   S( -3,  13),

            /* rooks: bucket 10 */
            S( -9, -62),   S(-28, -50),   S( -5, -95),   S( 21, -82),   S( 19, -91),   S(  3, -113),  S( 13, -90),   S(-27, -66),
            S(-21, -46),   S(-22, -71),   S(-27, -52),   S(-23, -59),   S(-19, -77),   S( -7, -58),   S( -1, -57),   S(-40, -75),
            S(-17, -44),   S(-33, -33),   S(-29, -41),   S(-42, -56),   S( -9, -21),   S(  8, -25),   S( -9, -40),   S(-24, -42),
            S(-19, -42),   S(-34, -63),   S(-14, -45),   S( -3,   2),   S(  1, -33),   S( -3, -19),   S( -7, -72),   S( -7, -64),
            S( -1, -35),   S( 12, -38),   S( 15, -49),   S( -5, -72),   S(  8, -30),   S(-13, -40),   S( 19, -57),   S(-20, -72),
            S( -8, -32),   S( 10, -13),   S(  2, -44),   S(  9, -56),   S(  6, -23),   S(  6, -39),   S(-13, -45),   S( -3, -43),
            S(-35, -30),   S(-11, -49),   S(-10, -48),   S(  5, -32),   S( 19, -39),   S(-22, -39),   S( -6, -57),   S( -2, -54),
            S(-24, -45),   S(-14,   3),   S( -2,  -6),   S( -7, -22),   S(  5,  -2),   S(-13,   0),   S(  3, -41),   S(-19, -31),

            /* rooks: bucket 11 */
            S(-45, -76),   S(-17, -45),   S(-34, -43),   S(  1, -39),   S(-17, -29),   S( 66, -59),   S( 10, -65),   S(  9, -90),
            S(  2, -22),   S(-25, -44),   S(-29, -29),   S(-29, -17),   S(-21,  -9),   S(  1,  -3),   S( -6, -54),   S(-30, -72),
            S(-53,   8),   S(-39,   4),   S(-18,   1),   S(-21, -12),   S( 14, -28),   S( -6, -41),   S(  9, -49),   S( -8,   0),
            S(-18, -42),   S(-10, -60),   S( -6, -15),   S( 20, -26),   S( 27, -16),   S(-23, -70),   S(  1,   4),   S( -7, -28),
            S( 13, -42),   S( 12, -36),   S(  9, -19),   S( 11, -15),   S( 34,   0),   S(  8, -40),   S( 25,   4),   S(-20, -64),
            S(-48, -42),   S(-26, -11),   S( 10, -42),   S( 22, -28),   S(-17, -26),   S( 35, -32),   S( 28,  -5),   S( 13, -49),
            S( -9, -13),   S(-16, -38),   S(  1, -17),   S( -1, -41),   S(  7, -30),   S( 31, -26),   S( 29, -30),   S(  7, -44),
            S(-10, -31),   S( 21,  13),   S( -2,  27),   S( 19,  -8),   S(-29, -25),   S(  4, -15),   S( 41, -24),   S(-17, -15),

            /* rooks: bucket 12 */
            S(-22, -99),   S(  1,  -8),   S(  1, -37),   S(  1, -32),   S( -4, -47),   S(  8, -32),   S(-19, -54),   S(-32, -77),
            S( 10,   1),   S(  1,   0),   S( 11,  18),   S( -6, -34),   S( 14,   7),   S( 13, -30),   S( -7, -17),   S( -6, -39),
            S(-13, -35),   S(  4,  12),   S( 21,  31),   S( 12, -17),   S( 10, -40),   S( 20,   4),   S( 21,  28),   S(  3,  11),
            S(-10, -16),   S(  3, -13),   S( 13,   1),   S(  5, -13),   S(  5,  12),   S(  7,   1),   S( 18,  35),   S(  1,  10),
            S( -7, -31),   S( 13,  11),   S( 12,  -1),   S( -6, -53),   S(  8,   2),   S( -7,   6),   S( -1, -18),   S( -7, -22),
            S( -5, -51),   S(  3, -30),   S(  4, -21),   S( -7, -22),   S( 16,  20),   S( -5, -25),   S(  8,  18),   S(  2, -14),
            S(-22, -43),   S( -8,  -4),   S( 14,  -6),   S(  2, -14),   S( -4, -23),   S( 17,  10),   S(  0,  11),   S(  4,  16),
            S(  0,  -3),   S( -6, -11),   S(  4, -17),   S( 13,  -1),   S( 10,   6),   S(  0,  -1),   S( -4, -11),   S( -4,  22),

            /* rooks: bucket 13 */
            S( -7, -42),   S(-12, -60),   S(-19, -56),   S(  3, -23),   S(-16, -78),   S( 16, -14),   S(-26, -65),   S(-19, -47),
            S( -6, -22),   S( -2, -28),   S(  1,  -7),   S( -7, -32),   S( 16,  10),   S( -2, -47),   S( 12, -27),   S(-18, -80),
            S(  1, -44),   S(-10, -54),   S(-10, -38),   S( 13,   5),   S( 17,  -5),   S( 26, -10),   S( 27,  13),   S(  0, -67),
            S(  4, -15),   S(-15, -51),   S( -2, -46),   S(  7, -22),   S(  7,  -5),   S(  4,   1),   S(  1,  -1),   S(  0,   7),
            S(  5, -15),   S( -1, -84),   S(-12, -84),   S( -3, -47),   S(  9, -46),   S( -1, -19),   S( -5,  -3),   S(  6,   1),
            S(  4, -10),   S( -2, -41),   S(-13, -66),   S( -6, -68),   S(-16, -114),  S( -4, -35),   S( -4, -10),   S( -8, -41),
            S( -7, -56),   S(  2, -43),   S(-11, -52),   S( 16,   3),   S( -6, -54),   S(  8, -24),   S( -6, -39),   S( -2, -14),
            S(  4, -12),   S(-16, -40),   S( -7, -47),   S(  6, -32),   S( -6, -27),   S(  0, -12),   S(  4,  10),   S( -3, -28),

            /* rooks: bucket 14 */
            S(  2, -13),   S(-30, -43),   S(-21, -65),   S(-13, -82),   S(-22, -79),   S( 22,   1),   S(-32, -128),  S(-13, -71),
            S(  7, -33),   S(  4, -17),   S( 15, -17),   S(  2, -30),   S( -3, -31),   S(  0,   2),   S( -4, -52),   S(-13, -88),
            S(  7, -21),   S(  7, -10),   S(  3, -28),   S( 11, -12),   S(  3, -26),   S( -7, -19),   S(  0, -50),   S(-19, -93),
            S( -7, -32),   S(  0,   7),   S(  9,  31),   S( 11,  -8),   S( -8, -49),   S(  0, -12),   S(  4, -24),   S(-10, -39),
            S( 11,  15),   S( 10,  37),   S( -5, -14),   S( 13,  -2),   S(  6, -36),   S( 17,  -3),   S(-14, -54),   S(-12, -48),
            S(  0,  -9),   S(  5,  -1),   S( -4, -31),   S( -4, -84),   S( -4, -73),   S( -9, -76),   S( -7, -97),   S( -3, -27),
            S(-21, -51),   S( -1, -12),   S( -7, -64),   S(-21, -99),   S( -4, -28),   S(  4, -18),   S(-13, -77),   S(-10, -31),
            S(-12, -53),   S( -4, -17),   S( -1, -23),   S( -4, -49),   S( -8, -69),   S(-33, -111),  S( -8, -73),   S(-21, -71),

            /* rooks: bucket 15 */
            S(-18, -76),   S(  2,  -7),   S(-31, -42),   S(-25, -51),   S(  3, -28),   S(-12, -57),   S(  9,   0),   S(-19, -66),
            S( 11,   6),   S(  6, -14),   S(-19, -79),   S(  5, -19),   S(-11, -45),   S(  2,  -9),   S(  9,   9),   S(  1, -10),
            S( -6,  -5),   S( -8, -50),   S(  9,  28),   S(  0, -49),   S(  9, -47),   S( -2, -29),   S( 10,  -1),   S(  4,   0),
            S(  2,   2),   S(  3,  15),   S( 18,  39),   S(-18, -31),   S( 10,  17),   S( -3, -25),   S(  5,  -8),   S( -9, -69),
            S(  8,  13),   S( -3, -20),   S(  6,  21),   S(  2,  -2),   S(  6,   6),   S(  4, -22),   S(  7, -41),   S( -2, -50),
            S( -1, -14),   S(  2,   2),   S(  4,   7),   S(  5,  -9),   S( -3, -33),   S( 11, -47),   S( 20, -15),   S(-12, -56),
            S( -1, -28),   S( -7, -17),   S( 12,  19),   S(  1, -17),   S( -1,  -6),   S( -1, -40),   S(  2,  -7),   S( -1, -22),
            S( -7, -34),   S(  2,   4),   S(  0,   8),   S(  0,  -5),   S( -4, -20),   S(-15, -82),   S( -4, -36),   S(-17, -48),

            /* queens: bucket 0 */
            S( -2, -37),   S(-30, -81),   S(-22, -60),   S( -1, -94),   S( -5, -88),   S( -9, -42),   S(-75, -13),   S( -9, -49),
            S(-10, -54),   S( 13, -69),   S(  7, -81),   S(-24, -17),   S(-10,  -3),   S(-25, -26),   S(-44, -52),   S( -9, -21),
            S(-31,  12),   S(  0, -21),   S( 17, -62),   S(-25,   2),   S( -3, -12),   S( -8, -33),   S(-39,  -5),   S(-71, -104),
            S(-40,  32),   S(  7,   5),   S(-17,  26),   S( -6,  37),   S( -2,  53),   S(-29,  30),   S(-46,  -2),   S(-33, -30),
            S(-28, -20),   S(-37, 104),   S(-15,  81),   S(-15,  73),   S(  5,  65),   S(-43,  71),   S(-44,  39),   S(-37, -22),
            S(-34,  38),   S( 20,  35),   S( 28,  61),   S( -5,  74),   S(-66,  81),   S(-25,  45),   S(-80,  14),   S(-62, -26),
            S(  0,   0),   S(  0,   0),   S( 28,  46),   S(-37,  41),   S(-52,  46),   S(-90,  95),   S(-103,  63),  S(-106,  15),
            S(  0,   0),   S(  0,   0),   S( 19,   6),   S(  3,   4),   S(-36,  50),   S(-37,  15),   S(-55,  22),   S(-58,  24),

            /* queens: bucket 1 */
            S( 25, -54),   S(  1, -26),   S( 17, -116),  S( 27, -106),  S( 35, -83),   S(  9, -73),   S(  7, -33),   S( 25,  10),
            S(-56,  11),   S( 13,  17),   S( 35, -70),   S( 15, -19),   S( 31, -24),   S( -6, -18),   S(-17,   1),   S(-19, -27),
            S( 49, -43),   S( 15,  13),   S(  6,  -4),   S( 19,  36),   S(-13,  57),   S( 13,  26),   S(-25,  14),   S( 14,  -9),
            S( 14, -17),   S(  8,  66),   S(  3,  51),   S( 13,  91),   S( -4,  88),   S( -7,  25),   S(  2,  43),   S( -6,  55),
            S( 18,  26),   S( 29,  74),   S( 12,  97),   S(-18,  83),   S( 15, 125),   S( 45,  44),   S(-37, 121),   S( 13,  58),
            S( 59,   7),   S(134,  47),   S(100,  75),   S( 95, 108),   S( 69,  88),   S( -9, 102),   S( 27,  64),   S(  3,  24),
            S( 83,   2),   S( 36,  24),   S(  0,   0),   S(  0,   0),   S( 21,  76),   S(-24,  58),   S(-23,  99),   S(-29,  43),
            S( 82,   9),   S( 57,  25),   S(  0,   0),   S(  0,   0),   S( 25,  58),   S( 56,  46),   S( 57,  29),   S(-17,  40),

            /* queens: bucket 2 */
            S( 18, -34),   S( 31, -25),   S( 30,  17),   S( 40, -33),   S( 44, -60),   S( 31, -33),   S( -8, -47),   S( 28,  -5),
            S( 25, -14),   S( 11,  37),   S( 36, -14),   S( 37,  10),   S( 47, -12),   S( 20, -19),   S( 24,   8),   S( 22,  14),
            S( 19,  67),   S( 30,  27),   S( 13,  75),   S( 12,  58),   S( 31,  48),   S( 17,  42),   S( 24,  20),   S( 31,  34),
            S( 15,  30),   S(  6,  85),   S( -4, 133),   S( -3, 123),   S( 35,  78),   S( -3,  83),   S( 23,  96),   S( 20,  96),
            S( 10,  82),   S(  7,  45),   S(-18, 132),   S( 11, 134),   S( 11, 141),   S( 81,  53),   S( 37, 113),   S( 52,  86),
            S(-38,  98),   S(-43, 102),   S(-15, 134),   S( 78, 103),   S( 55,  84),   S( 91, 123),   S(108, 103),   S( 13, 154),
            S( -3,  65),   S(-16,  87),   S( -7, 119),   S( 68,  86),   S(  0,   0),   S(  0,   0),   S( 24, 145),   S( 38,  97),
            S(-12,  51),   S( 43,  29),   S( 69,  35),   S( 41,  66),   S(  0,   0),   S(  0,   0),   S( 71,  80),   S( 34,  77),

            /* queens: bucket 3 */
            S(-50,  39),   S(-26,  38),   S(-13,  31),   S( -2,  45),   S(-18,   9),   S( -9, -20),   S( 11, -76),   S(-60,  30),
            S(-43,  43),   S(-19,  42),   S(-13,  52),   S( -8,  68),   S( -6,  58),   S( -5,  23),   S( 20, -10),   S( 45, -56),
            S(-53,  69),   S(-23,  81),   S(-31, 119),   S(-34, 141),   S(-18, 103),   S(-27, 110),   S( -7,  59),   S(-10,  47),
            S(-44,  71),   S(-51, 123),   S(-46, 167),   S(-28, 168),   S(-31, 166),   S(-31, 114),   S(-15, 105),   S(-19,  83),
            S(-28,  97),   S(-35, 138),   S(-47, 162),   S(-29, 186),   S(-27, 173),   S(-36, 210),   S(-28, 159),   S(-35, 131),
            S(-57, 109),   S(-69, 154),   S(-63, 182),   S(-58, 206),   S(-49, 208),   S(  0, 191),   S(-17, 201),   S(-40, 182),
            S(-100, 128),  S(-95, 160),   S(-62, 186),   S(-88, 182),   S(-96, 214),   S( 10, 128),   S(  0,   0),   S(  0,   0),
            S(-154, 193),  S(-134, 171),  S(-64, 127),   S(-110, 161),  S(-63, 157),   S( -6, 111),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-39,  26),   S(-57, -59),   S( 16,   2),   S(-19, -37),   S(-19, -12),   S( -4,   1),   S(-32, -29),   S( 39,  32),
            S(-28,   2),   S(  9,  24),   S(-18,  -7),   S(-34,  -9),   S(-15,  14),   S( 29,  48),   S(-53, -37),   S(-45, -35),
            S( 31,  57),   S(  5, -25),   S( 17,  13),   S( 10,  24),   S( 14,  40),   S( 21,   6),   S( -3, -25),   S( -2,  -5),
            S(-16, -14),   S( 37,  17),   S(  7,  -5),   S( -7,  53),   S( 53,  17),   S(  5,  28),   S(-43,  -7),   S(-40,   3),
            S(  0,   0),   S(  0,   0),   S( 45,  29),   S( 69,  37),   S( 21,  26),   S(-12,  30),   S(  0,  -1),   S( 11,   5),
            S(  0,   0),   S(  0,   0),   S( 28,  19),   S( 53,  79),   S( 34,  51),   S( 30,  24),   S( -5,   8),   S(-17, -25),
            S( 19,  -8),   S(  9,  12),   S( 46,  15),   S( 55,  49),   S( 26,  29),   S(-29,  11),   S( 16,   7),   S(-30,  -3),
            S( 21,   1),   S(-16,  -3),   S( 27,  10),   S( 41,  32),   S( 13,  -3),   S(  7, -18),   S(-18, -17),   S( 18,  -5),

            /* queens: bucket 5 */
            S(  4,   9),   S(-15,  -2),   S(-19, -28),   S(-32,   8),   S( 35, -25),   S( 34,  40),   S(  8,   6),   S( -3,  -5),
            S(  2, -13),   S(  8,  -9),   S(  7, -15),   S(-15,  20),   S(  7,  22),   S(-19, -23),   S( 10,  -4),   S( -9,   2),
            S( 36,  18),   S( 21,  -2),   S( 43,  32),   S( -3,  30),   S( 17,  35),   S( 51,  32),   S( 20,  51),   S(-20,   1),
            S( 14, -33),   S( 58,  26),   S( 19,  -1),   S( 52,  49),   S( 69,  37),   S( 13,  26),   S( 32,  40),   S(-19,  15),
            S( 26,  11),   S( 50,  13),   S(  0,   0),   S(  0,   0),   S( 26,  14),   S( 25,  11),   S( 30,   8),   S( 25,  13),
            S( 45,  12),   S( 34,  41),   S(  0,   0),   S(  0,   0),   S( 55,  40),   S( 73,  55),   S( 31,  21),   S( 51,  52),
            S( 73,  -5),   S(115,  32),   S( 72,  80),   S( 33,  51),   S( 83,  67),   S(146,  77),   S( 60,  51),   S( 24,  18),
            S( 25,  33),   S( 49,  15),   S( 59,  30),   S( 49,  26),   S( 55,  59),   S( 57,  57),   S( 48,  10),   S( 21,   6),

            /* queens: bucket 6 */
            S( 34,  13),   S( -3,  -3),   S( 24, -14),   S(  9, -24),   S(  1,   9),   S(-20, -17),   S(-18, -35),   S(-10,   5),
            S( 24,   6),   S( 14,  25),   S( 55,  20),   S( 31,  12),   S( 44,  21),   S( 21, -29),   S(-11,  33),   S( -5,  34),
            S( -8,  32),   S(  8,  14),   S(-11,  37),   S( 23,  -4),   S(-29,  36),   S( 12,  27),   S( 57,  35),   S( 57,  51),
            S( 12,  30),   S(-22,  -3),   S( 31,   9),   S( 88,  54),   S( 48,  -4),   S( 46,  24),   S( 89,  34),   S( 81,  41),
            S( -1,  32),   S(-18,  30),   S( 66,  83),   S( 58,  54),   S(  0,   0),   S(  0,   0),   S( 88,  59),   S( 57,  77),
            S( 16,  26),   S( 57,  44),   S( 65,  63),   S( 26,  38),   S(  0,   0),   S(  0,   0),   S(102,  71),   S(140,  32),
            S( 33,  20),   S(-11,  37),   S( 78,  39),   S( 64,  30),   S( 60,  72),   S( 98, 104),   S(145,  60),   S(125,  36),
            S( 34,  23),   S( 73,  23),   S( 68,  57),   S(111,  52),   S(109,  31),   S( 96,  51),   S(120,  35),   S( 86,  -6),

            /* queens: bucket 7 */
            S(-30, -29),   S( 14, -41),   S(-34,  -8),   S(-18,   7),   S(-14, -20),   S(-38,  -3),   S(-29,   8),   S(-19, -27),
            S(-51,   4),   S(-81,  32),   S(-39,  69),   S(-21,  35),   S(-35,  36),   S(  2,   9),   S(  6,   7),   S(-29,  -5),
            S(-16, -28),   S(-34,   5),   S( -8,  47),   S( 42,  22),   S( 33,  32),   S(  2,  27),   S( 29, -15),   S( 20,  -1),
            S(-37,   6),   S( -3,   3),   S( 14,  29),   S( 29,  47),   S( 74,  35),   S( 65,  32),   S( 62, -12),   S( 32,  24),
            S( -1,   7),   S(-15,  10),   S(  6,  52),   S( 20,  69),   S( 62,  47),   S( 82,  43),   S(  0,   0),   S(  0,   0),
            S(-37,  -9),   S(  1,  28),   S( -9,  76),   S(  0,  46),   S( 44,  13),   S(103,  54),   S(  0,   0),   S(  0,   0),
            S(-75,  47),   S(-39,  12),   S(  7,  26),   S( 43,  40),   S( 99,  14),   S(102,  29),   S( 44,  17),   S( 68,  48),
            S( 15, -36),   S( 12, -21),   S( 47,  15),   S( 90, -29),   S( 79,  43),   S( 58,  12),   S( 22,  -8),   S( 74,  18),

            /* queens: bucket 8 */
            S( -7, -24),   S( -7, -23),   S(-13, -33),   S( 14,   3),   S( -2,  -9),   S( 25,  22),   S( -4, -15),   S( -1,  -2),
            S(  2,   0),   S(  1,   2),   S( 17,  -2),   S( 10,   8),   S( 17,  11),   S( -4,  -9),   S(-16, -15),   S(  2,   4),
            S(  0,   0),   S(  0,   0),   S(  6,   1),   S(-14, -34),   S(  8,  -3),   S(  0,  -9),   S(-12,  -8),   S(  5,  12),
            S(  0,   0),   S(  0,   0),   S(  4,   6),   S(  2, -10),   S(  3,  10),   S(  8,  -7),   S(  0,  18),   S( -8, -22),
            S( 10,   5),   S( 12,  18),   S( 11,  22),   S( 34,  17),   S( 12,   0),   S( 11,   2),   S( 14,  13),   S( -7, -17),
            S( -8, -23),   S( 19,   4),   S( 16,  26),   S(-18, -43),   S(  3,  11),   S( 17,  12),   S(-19, -34),   S( -4, -12),
            S(-10, -26),   S( 11,  10),   S( 11,   6),   S( 49,  62),   S(  8, -11),   S(  2,  11),   S(  4,  -6),   S(  0, -13),
            S( -6, -16),   S( 10,   7),   S( 23,  21),   S( 11,  12),   S(  9,  -7),   S(-14, -18),   S(  0,   5),   S(-32, -56),

            /* queens: bucket 9 */
            S( 21,   5),   S(-12, -29),   S( -4, -17),   S(  8, -24),   S( 13,   9),   S(  6,  -7),   S(-12, -28),   S( -2, -13),
            S( 11,  -7),   S( 15,   3),   S(-11, -28),   S( 23,  25),   S(-11, -26),   S( -2, -21),   S( -3,  -4),   S( -4, -16),
            S(  7, -16),   S( -2, -14),   S(  0,   0),   S(  0,   0),   S(  3,  -7),   S( 11,   4),   S(-18, -39),   S( 14,   7),
            S( 21,  22),   S(-11, -26),   S(  0,   0),   S(  0,   0),   S( 14,  16),   S( 24,  12),   S( 11,   3),   S(-16,  -2),
            S(  7, -18),   S(  6, -21),   S( -8, -17),   S(-11,  -8),   S(  7, -24),   S( 19,   9),   S( -6, -43),   S(-11, -38),
            S( 26,  26),   S( 11, -14),   S( 19,  10),   S( 12,   9),   S(  3, -16),   S( -7,  -1),   S(-10, -19),   S(  7,  -9),
            S( 13,  11),   S( 24,  11),   S(  1,   9),   S( 16,  17),   S( 30,  28),   S( 37,  29),   S(  2,  -1),   S(  9, -16),
            S( 11, -15),   S(  9, -30),   S(-25, -48),   S( 13,  13),   S( 10,  -1),   S( -6, -21),   S(  7, -18),   S(  6,  -5),

            /* queens: bucket 10 */
            S( 12,  11),   S( 25,  22),   S(  2,  -7),   S(  0, -20),   S(  4, -14),   S(  8,   5),   S( -7, -20),   S( -8, -28),
            S( -3, -18),   S(-12, -28),   S( 22,  13),   S(-22, -45),   S( -7, -12),   S( 17,  -6),   S( -2, -23),   S( -9, -29),
            S(  0,  -1),   S(  9,  18),   S( 10,  -8),   S( -4, -26),   S(  0,   0),   S(  0,   0),   S(  2,   2),   S(  1,   1),
            S(  9,   4),   S(  7,  -3),   S( 11,   9),   S(  8,   6),   S(  0,   0),   S(  0,   0),   S( -5, -21),   S( -6, -31),
            S( -7,  -8),   S( 23,  27),   S( -5, -14),   S( 65,  79),   S( 16,  17),   S(  1,  -8),   S( 15,   0),   S( 25,   3),
            S( -4, -18),   S( 11,   8),   S( 40,  50),   S( 23,  22),   S( 19,  24),   S( 29,  45),   S( -2, -11),   S( 10, -19),
            S( 17,   8),   S( 20,  44),   S( 23,  26),   S( 21,  23),   S( 29,  29),   S( 10,  -7),   S( 14, -14),   S(  2,  -3),
            S( -6, -25),   S(  9,  -3),   S( 21,  -2),   S( -6, -41),   S( -1,  -9),   S( -6,  -6),   S(-11, -44),   S(  6, -24),

            /* queens: bucket 11 */
            S(-19, -30),   S(-13, -13),   S( -7,  -6),   S(-29, -40),   S( -8,  -5),   S(-11, -31),   S(-11, -22),   S(  3,  -1),
            S( -7,  -1),   S(  6,  -3),   S(-14, -18),   S( 17,   0),   S( 30,   3),   S(-13, -26),   S( 11, -14),   S( -2, -21),
            S( -4,   4),   S( -2,  -5),   S(-10,  12),   S(  4,  -9),   S( 20,   9),   S(-16, -28),   S(  0,   0),   S(  0,   0),
            S(-12, -17),   S(-10,  -4),   S(  3,   9),   S( 24,   9),   S(  9,   3),   S( 12,  25),   S(  0,   0),   S(  0,   0),
            S(-16,  -9),   S( 14,   1),   S(  8,  22),   S(  3,  16),   S( 57,  68),   S( 26,  21),   S( 14,  17),   S( -5, -14),
            S( -7,  -7),   S(  3,   7),   S(-31, -30),   S(  0,  -8),   S( 26,  23),   S(  7,   4),   S(  8,  -6),   S( 12, -16),
            S( -9, -15),   S( -2,   9),   S(  4,  -7),   S(-12, -11),   S( 22,  40),   S( 19,  -4),   S(  4, -21),   S(  1,  -6),
            S(-29, -68),   S( -6,  -6),   S(-20, -24),   S( 26,  34),   S( -1,  -5),   S(  3,  -7),   S( -1, -10),   S( 12,   3),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  4,  -1),   S(-20, -23),   S(  1,  17),   S( -9, -25),   S( -8, -18),   S( -3,  -8),
            S(  0,   0),   S(  0,   0),   S(  2,  -3),   S(  1,  -7),   S( -9, -32),   S(-15, -39),   S( -3,  -9),   S( -5, -13),
            S( -9, -18),   S(  0,   0),   S(  6,  -7),   S(  8, -25),   S( 18,  39),   S(  3,  15),   S(  0,   0),   S(  4,   6),
            S( 18,   8),   S( 10,  12),   S( 10,  21),   S( -4, -15),   S(  7,   1),   S( -1,   2),   S(  0,   1),   S( -6, -15),
            S(-26, -53),   S( -3, -11),   S( 11,   2),   S(-10, -43),   S( 10,  23),   S(-14, -42),   S(-16, -40),   S( -9, -19),
            S( 12,  14),   S( -1,  -4),   S( -4,  -2),   S(-20, -39),   S(-19, -44),   S( -8,  -4),   S(-15, -21),   S( -7, -19),
            S( -5,  -9),   S(  8,  12),   S( -4, -10),   S( 13,  21),   S(  0,   1),   S(-20, -36),   S(  5,   5),   S(-13, -21),
            S(  0,  -7),   S( -4,  -7),   S( -3,  -1),   S(  3,   2),   S(  1,   1),   S(-21, -42),   S( -2,  -3),   S(-16, -35),

            /* queens: bucket 13 */
            S(  7,  -4),   S(  3,  -6),   S(  0,   0),   S(  0,   0),   S( -6, -15),   S(-13, -28),   S(  8,  -4),   S(-10, -21),
            S(-16, -50),   S(  4,   7),   S(  0,   0),   S(  0,   0),   S(-12, -23),   S(-20, -39),   S( -7, -14),   S(-12, -34),
            S( -1, -13),   S(  2, -10),   S(  1,   5),   S(  5,   2),   S(-19, -42),   S(  0, -12),   S( -5, -13),   S(  3,   7),
            S( -1, -11),   S(-26, -62),   S(  2,  -3),   S(  9,   8),   S( 12,  25),   S( 18,  27),   S(-12, -31),   S(-14, -31),
            S( -7, -29),   S( -2, -18),   S(  0, -18),   S( 20,  22),   S( -9, -16),   S( 10,   3),   S( -9, -16),   S(  9,  18),
            S( -1, -11),   S(-19, -48),   S(  2,   0),   S(  7,   8),   S(  9,   2),   S(-14, -21),   S(-21, -41),   S(-20, -50),
            S( -4,  -7),   S(  0,  -9),   S( 14,  24),   S( -2,  -7),   S( -8, -25),   S(  2,  -3),   S(-19, -46),   S( -3,  -9),
            S(-27, -63),   S(-19, -36),   S( -3, -10),   S(  1,   4),   S( -5,  -7),   S( -7, -26),   S(  6,   8),   S(-15, -28),

            /* queens: bucket 14 */
            S( -2, -16),   S( -4, -16),   S(  6,   7),   S(  5,  -5),   S(  0,   0),   S(  0,   0),   S(  1,  -4),   S( -9, -26),
            S( -8, -17),   S(-20, -50),   S( -5, -17),   S(  4,  -4),   S(  0,   0),   S(  0,   0),   S(  6,  12),   S(-13, -34),
            S( -6, -13),   S( 11, -14),   S( -7, -11),   S( 10,  10),   S(  0,   5),   S(  6,  13),   S(-15, -38),   S(-13, -43),
            S( -9, -22),   S( -8, -19),   S( -2, -10),   S( -9, -21),   S( -8, -23),   S( -6, -27),   S( -3, -24),   S( -1,  -9),
            S(-10, -15),   S( -2, -15),   S( -9, -23),   S( 22,  46),   S( 25,  38),   S(-15, -25),   S(-12, -22),   S(  0,  -3),
            S( -9, -19),   S(-16, -40),   S(-17, -32),   S(-16, -41),   S(  9,  19),   S( -8,  -6),   S( -1,  -5),   S(-12, -21),
            S( -7, -15),   S(  0,   1),   S( -7, -18),   S(  2,   2),   S( -3,  -4),   S(  1,  -4),   S( -9, -29),   S( -6,  -6),
            S( -9, -26),   S( 12,   8),   S(-15, -22),   S(-11, -25),   S(  6,   9),   S(-11, -18),   S( -9, -16),   S(-10, -26),

            /* queens: bucket 15 */
            S( -4, -14),   S(-10, -23),   S( 11,  13),   S( -9, -24),   S( 15,  18),   S( -1,  -3),   S(  0,   0),   S(  0,   0),
            S( -7, -21),   S(  0,  -9),   S(-10, -14),   S(-24, -63),   S(  5,  14),   S(  4,   7),   S(  0,   0),   S(  0,   0),
            S( -3,  -7),   S(  5,  -1),   S(-13, -20),   S(-10, -26),   S( -2,  -8),   S( 18,  33),   S( -2,  -9),   S( -2,  -5),
            S( -6, -15),   S(-10, -22),   S( -5, -21),   S(  0,  -9),   S(  0,  -1),   S( -3, -10),   S( -8, -25),   S( -6, -18),
            S( -1,  -3),   S(  0,  -5),   S(-17, -37),   S( -1,   4),   S( 18,  50),   S(  3,  12),   S(  4,   2),   S( -9, -16),
            S( -1,  -9),   S(-12, -26),   S( -7, -13),   S(-10, -22),   S(  8,  34),   S(-13, -30),   S( -1, -11),   S(-13, -35),
            S( -8, -19),   S( -3,  -4),   S( -6,  -5),   S( -2,  -3),   S(-10, -16),   S(  2,  -4),   S( -5,  -9),   S( -4, -21),
            S(-22, -53),   S(-22, -56),   S( -5, -18),   S( -2,   1),   S(-10, -15),   S( -8, -21),   S(  2,   4),   S(-11, -25),

            /* kings: bucket 0 */
            S(  8, -41),   S( 19,   4),   S( 14, -18),   S(-34,   5),   S(-15,   3),   S( 29, -34),   S( 11,   3),   S( 23, -64),
            S(-20,  28),   S( -8,  12),   S(-12,  23),   S(-45,  27),   S(-45,  47),   S(-13,  18),   S(-24,  59),   S(-14,  42),
            S( 23,  -5),   S( 53,  -8),   S( 17,   4),   S( -8, -14),   S(-29,   7),   S(-17,   0),   S(-51,  30),   S(  2, -23),
            S(-38, -35),   S(-17,   0),   S( 12,  -8),   S(-25,   4),   S(-79,  27),   S(-34,   4),   S(-42,  18),   S(-12,  30),
            S(-61, -86),   S( 21, -30),   S( 28, -33),   S( 17, -16),   S(-36,   1),   S(-23,  16),   S(-13,   8),   S( 23,  -5),
            S(  5, -79),   S( 26, -36),   S( 37, -58),   S( 13, -30),   S( 21, -19),   S( 19, -40),   S(  7, -12),   S( -6, -18),
            S(  0,   0),   S(  0,   0),   S(  6, -21),   S(  9, -45),   S(  3, -19),   S(  0, -15),   S(  6, -18),   S(-25, -23),
            S(  0,   0),   S(  0,   0),   S(-14, -86),   S( 16, -36),   S( 11,  -6),   S(  7, -14),   S( 23,  25),   S(  4,   6),

            /* kings: bucket 1 */
            S( 24, -48),   S( 33, -21),   S(  6,  -4),   S( 18, -10),   S( -4,  -6),   S( 30, -14),   S( 14,  16),   S( 23, -19),
            S( 12,  -3),   S(  6,  30),   S( 15, -14),   S(-41,  27),   S(-33,  28),   S( -9,  16),   S(  1,  25),   S(-10,  28),
            S(  1,  -4),   S( 13, -13),   S(  5, -17),   S( 15, -19),   S(-27,   0),   S(  8, -18),   S(  8,   2),   S( 50, -15),
            S(  2, -11),   S( 43, -11),   S( 39,  -9),   S(-11,  15),   S( -4,  22),   S(-69,  22),   S( -2,  14),   S(-29,  29),
            S(-20, -37),   S( 10, -16),   S( 32, -49),   S( -1, -15),   S( 26, -18),   S( -7, -19),   S( -8,  11),   S(-17, -11),
            S( 11, -23),   S( 31, -39),   S( 19, -28),   S( 31,  -7),   S(  8,  -5),   S(  5,   3),   S( 16,  17),   S(  9,   6),
            S( 20, -23),   S(  1, -14),   S(  0,   0),   S(  0,   0),   S( -1,  -6),   S(  3,  10),   S(  2,  46),   S( -9, -38),
            S(-21, -131),  S(  2, -11),   S(  0,   0),   S(  0,   0),   S(  4, -23),   S( 10,   2),   S(  0,  21),   S(  0, -36),

            /* kings: bucket 2 */
            S( 13, -62),   S( 18, -11),   S( 16, -26),   S( 16, -14),   S(-14,   5),   S( 40, -25),   S(  9,  26),   S( 27, -15),
            S( 22,  -9),   S( -8,  35),   S(-10,   8),   S(-13,   9),   S(-27,  19),   S( -7,   5),   S(  8,  11),   S(-11,  17),
            S(-20, -11),   S(-26,  11),   S( -6, -14),   S(-21,  -7),   S( -7,  -4),   S(  9, -26),   S( 45, -19),   S( 14, -12),
            S( 11,   9),   S(-47,  25),   S(  3,   7),   S( -3,  10),   S( 22,   2),   S(-27,   3),   S( 39, -23),   S( 36,  -8),
            S( -4, -16),   S( 38,   0),   S( 15, -27),   S(  5, -12),   S( 36, -33),   S(-19, -23),   S( 50, -39),   S( -2, -27),
            S(  0,   0),   S( 24,  -5),   S( 19, -17),   S( 37, -33),   S( 53, -15),   S( 26, -15),   S( 78, -30),   S( 25, -34),
            S(-10,  -8),   S( -7,  36),   S( -7,  -4),   S( 23,   2),   S(  0,   0),   S(  0,   0),   S( 23,  14),   S(-20, -29),
            S( -5, -24),   S(-12, -32),   S(  2, -55),   S(  9,   3),   S(  0,   0),   S(  0,   0),   S( -2, -15),   S(-26, -147),

            /* kings: bucket 3 */
            S(  3, -64),   S( 11, -14),   S( 23, -38),   S(-10, -16),   S(-14, -26),   S( 30, -33),   S(  7,  13),   S( 10, -29),
            S(  2,  11),   S(-23,  38),   S( -8,  -7),   S(-41,  15),   S(-49,  21),   S(  0,  -7),   S(-10,  19),   S(-16,  18),
            S( 19, -34),   S(  1,  -6),   S( -9, -11),   S( -5, -24),   S(-12,   1),   S( 29, -30),   S( 47, -24),   S( 52, -23),
            S(-33,  24),   S(-98,  28),   S(-88,  22),   S(-93,  29),   S(-78,  24),   S(-61,  -5),   S(-85,   4),   S(-61,  -9),
            S(-42,   8),   S(-15, -18),   S(-69,  -8),   S(-59,  -2),   S(-32, -20),   S( 18, -46),   S( -3, -51),   S(-27, -56),
            S( -9, -18),   S( -3, -19),   S( -5, -15),   S(-46,  -7),   S( 29, -33),   S( 99, -60),   S(111, -48),   S( 47, -110),
            S(-45, -21),   S( 27,  -2),   S(  2, -35),   S( -7,  -9),   S( 27, -27),   S( 44, -48),   S(  0,   0),   S(  0,   0),
            S( -1, -12),   S(  4, -15),   S(  0,  -3),   S(  9,  -9),   S(  5, -96),   S(  9,  -8),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-20,  -4),   S( 36,   2),   S(-22,  24),   S(  3,  -7),   S( -4, -16),   S( 12,  -7),   S( 12,  12),   S( 51, -24),
            S(-47,  26),   S(  6,  23),   S(-41,  22),   S(  0,   1),   S( 58, -13),   S( 17,  -1),   S( 39,  -4),   S(  9,   3),
            S(-13,  17),   S(-13,  -5),   S(-42,   8),   S(-30,   3),   S(-18,   6),   S( -5, -20),   S(-37,   2),   S( -4,  -2),
            S( 13,  -8),   S( 13,  -4),   S( 44,   3),   S( 18,  14),   S(  9,  -2),   S(  4,   8),   S(-17,  26),   S( -1,  16),
            S(  0,   0),   S(  0,   0),   S(  7, -17),   S(  5,  -5),   S( -6,   9),   S(-32, -12),   S(-21,  -1),   S(-11,  -3),
            S(  0,   0),   S(  0,   0),   S( -7,  10),   S( 11,  11),   S(  6,  -5),   S( -3, -18),   S(  5, -25),   S( 11,   0),
            S( -1,  10),   S( -4, -13),   S( -1, -38),   S(  8,   0),   S( 15,  19),   S(-21,  -5),   S( -2, -17),   S(  6,   3),
            S( -4,  39),   S(  5,  10),   S(  2,   5),   S(  6,  20),   S(  0, -19),   S(  0,  10),   S(  9,  12),   S(  7,  -8),

            /* kings: bucket 5 */
            S( 14, -19),   S(-45,  28),   S(-68,  23),   S(-38,  19),   S(-25,  14),   S(  0,   1),   S( 59,  -7),   S( 73, -16),
            S(-25,   9),   S( 59,  -5),   S( 34,  -9),   S( 19,  -1),   S(  2,   3),   S( 49, -13),   S( 29,  10),   S( 35,  -5),
            S(  1, -10),   S(-40,   8),   S( 30, -19),   S(-34,  -5),   S( -9,   2),   S(-51,  -6),   S( -5,   5),   S( 30,  -4),
            S(-16, -12),   S( 65, -16),   S( 12,  -2),   S( -6,  23),   S( 49,  15),   S( -2,   4),   S( 22,   4),   S(  6,  -3),
            S(-16,  -7),   S(-18, -49),   S(  0,   0),   S(  0,   0),   S( -1, -16),   S(  0,  -7),   S(  8,  -4),   S(-14,   3),
            S(-35,  -8),   S(-17,   6),   S(  0,   0),   S(  0,   0),   S( -6,  23),   S(-35,  17),   S(-19,  14),   S( -8,  -4),
            S(-10,  16),   S(  5,  13),   S(  2,  44),   S( -7, -29),   S( -1,  32),   S( -9,  12),   S( 10,  25),   S(  2,  11),
            S(-12, -15),   S(  5,  33),   S(  7,  46),   S(  5,  18),   S( -6,   6),   S(-16,   4),   S( -4, -13),   S( -3,   8),

            /* kings: bucket 6 */
            S( 37, -19),   S( 35, -15),   S( 17, -15),   S( 14,   3),   S(  2,  11),   S(-40,  20),   S( 20,  18),   S( 16,   7),
            S( 57, -18),   S( 30,  12),   S( 13,  -3),   S( 43, -10),   S( 33,  -2),   S(  2,   5),   S( 33,   4),   S( 40,  -9),
            S(-13, -14),   S(-20,   5),   S(-18, -10),   S( -7,  -9),   S(-13,  -4),   S(-68,   4),   S(  0,   3),   S(-44,  19),
            S( 14,  16),   S( 30,   1),   S( -5,   1),   S( 45,   4),   S( 77,   1),   S(  8,  -3),   S( 86, -28),   S(  5,  -2),
            S( -5, -19),   S(-44,  11),   S( -7, -19),   S( -9,  -4),   S(  0,   0),   S(  0,   0),   S(-24, -23),   S(-63,  -8),
            S(-12,  -3),   S(  9,   8),   S(-16,   2),   S( -9,  -1),   S(  0,   0),   S(  0,   0),   S(-28,  24),   S(-41,  -5),
            S( -2, -22),   S(  2,  28),   S( -7,  -1),   S(  0,  25),   S( -3,  -5),   S(  4,  -4),   S( -7,   1),   S(-31, -22),
            S( -4,   5),   S(-18,   2),   S( 12,  23),   S(  1,   4),   S( -1,  25),   S(  2,  15),   S( -3,  25),   S(-14,  -4),

            /* kings: bucket 7 */
            S( 55, -36),   S(  6, -11),   S(-11, -20),   S(-10,   0),   S(-32,   3),   S(-69,  39),   S(-21,  40),   S( -4,  18),
            S( 25,   1),   S( 21, -12),   S(  4,  -7),   S(-11,  -1),   S( 11,   1),   S(-42,  22),   S( 14,   3),   S( 19,   7),
            S( 30, -29),   S( -9,   3),   S(-35,  -4),   S(-27,  -1),   S(-34,  -3),   S(-43,   7),   S(-33,  14),   S(-61,  17),
            S( -6, -11),   S( 45,  -7),   S( -1,  -3),   S(  8,   4),   S(-27,  17),   S( 59, -18),   S( 48, -17),   S( 49, -17),
            S(  9, -16),   S(-24,  16),   S(-29,  -8),   S(  4,  -2),   S(-16, -13),   S( 13, -36),   S(  0,   0),   S(  0,   0),
            S(-25, -41),   S(  2,   0),   S( 19, -10),   S(  8, -15),   S( 13,   1),   S(  9,   4),   S(  0,   0),   S(  0,   0),
            S( 16,  20),   S( 33, -25),   S(  8,  -4),   S( -2, -10),   S( 13, -13),   S(-11, -11),   S( 14,  23),   S( -8, -34),
            S(  6,  23),   S( -9, -52),   S( 34,  29),   S( -5, -12),   S( 22,  14),   S(-13, -30),   S(  6,  28),   S(-13, -24),

            /* kings: bucket 8 */
            S(-12,  70),   S(-19,  56),   S(-14,  51),   S(-26, -21),   S(-21, -11),   S(-22,   6),   S( 25,  -9),   S(  8,   6),
            S( 13,  96),   S( 28,   7),   S(  8,  64),   S(  0,  16),   S(-17,  17),   S( -1,   8),   S(  8,  18),   S( 29,  32),
            S(  0,   0),   S(  0,   0),   S( 26,  52),   S( 23,  -6),   S( 14,   4),   S( 23, -14),   S(  5,  10),   S(  9,  -1),
            S(  0,   0),   S(  0,   0),   S(  8,  33),   S( 28, -15),   S(  7,  16),   S(  8,  -4),   S( 21, -11),   S(  4,  41),
            S( -2, -26),   S(  3,  25),   S(  5,  -9),   S(  6,  -7),   S(  1, -25),   S(-14,   0),   S(  6,   4),   S( -6, -27),
            S(  2,   8),   S( -4,  -8),   S(  2,  -6),   S(  0, -23),   S( -3,  -5),   S(  3,  -1),   S(-15,   0),   S(  7,  -6),
            S( -5, -37),   S( -5, -30),   S(  8,  -3),   S( -4, -14),   S( 11, -26),   S(  1, -18),   S( 11,  -5),   S( 11, -23),
            S(  1,   4),   S(-12, -44),   S(  2, -12),   S( -4, -28),   S(  5,  26),   S( -5, -13),   S(  7, -12),   S(  4,  -1),

            /* kings: bucket 9 */
            S(-35,  56),   S(-51,  39),   S(-55,  49),   S(-65,  32),   S(-91,  40),   S(-49,  18),   S( 61,  -8),   S( 39,  24),
            S(-30,  19),   S( 22,  24),   S(  5,  -4),   S( 34,  27),   S( 32,  30),   S( 32,   1),   S( 39,  21),   S( 42,  -5),
            S(-12,  -2),   S( 12,  12),   S(  0,   0),   S(  0,   0),   S( 12,  16),   S(-27,  -2),   S(  6,   5),   S(  8,   2),
            S(-10,   2),   S( -6, -12),   S(  0,   0),   S(  0,   0),   S(  7,  14),   S( 50,  -2),   S(-19,   8),   S(  2,  20),
            S( -8,   0),   S(  7,  11),   S(  2,   8),   S(  5,  -4),   S( -2,  -8),   S( -2,   4),   S(  0,   7),   S(-12,  -7),
            S(  1,  23),   S(-13,  11),   S(  4,   6),   S(-10,  -9),   S(  1,   7),   S( -4,  28),   S(-13,  -3),   S(  3,  48),
            S(  0, -14),   S(  0, -14),   S(  1,   4),   S( -1,  22),   S(  3,  32),   S( 15,   4),   S(  5,  -2),   S( -4, -11),
            S(  5,  19),   S(-10, -14),   S(  0, -22),   S(  0, -12),   S(-10, -40),   S( -4,  -1),   S( -7, -21),   S( -5,  11),

            /* kings: bucket 10 */
            S( -7,  23),   S(-43,  10),   S( -7,   9),   S(-38,  27),   S(-70,  15),   S(-104,  42),  S(-33,  35),   S(-92,  84),
            S( 27, -15),   S( 42,  22),   S(  0,  -5),   S( 12,  10),   S( 69,  17),   S( 17,  16),   S( 53,  11),   S(-40,  37),
            S(  8,  37),   S( 13,   0),   S( 35, -12),   S(-14,  15),   S(  0,   0),   S(  0,   0),   S(  2,  10),   S(-50,   7),
            S( 16,   2),   S( -3, -12),   S( -4, -16),   S( 24,  -3),   S(  0,   0),   S(  0,   0),   S( 14,  33),   S( 18,  -4),
            S(  2, -13),   S( 18,  10),   S(  5, -13),   S(  4, -22),   S(  3,  -6),   S(  9,  12),   S(  6,  25),   S(-14,  11),
            S(  2,   4),   S( -9,  29),   S(-24,  -4),   S( 21,  10),   S( -8,   8),   S( -4, -13),   S(-12,   8),   S(-10,  -3),
            S(  1, -38),   S(  8,  -2),   S(  6,   5),   S( 16,   6),   S(  7,  13),   S( -8,   7),   S( 34,   8),   S( -4,  18),
            S(  0,   9),   S( 21,  27),   S(  3,  -3),   S(  2,   8),   S( -2,  -8),   S(  0, -28),   S(-10, -50),   S(  6,  31),

            /* kings: bucket 11 */
            S(-21,  55),   S( 20,  -1),   S(-16, -30),   S(  0,  13),   S(-22,  -3),   S(-142,  67),  S(-75,  64),   S(-125, 135),
            S(  3, -39),   S(-14,  13),   S( -8, -23),   S( 23,  14),   S( 12,   6),   S(  5,  55),   S( 44,  15),   S( 38,  45),
            S(-12,  -5),   S( 21,  -7),   S(-25,  16),   S( 29,  -1),   S( 59,  -4),   S( 17,  24),   S(  0,   0),   S(  0,   0),
            S(  4,  20),   S(  4,  22),   S(  4, -13),   S( 25, -13),   S( 48, -22),   S( 31,  16),   S(  0,   0),   S(  0,   0),
            S(  2,  18),   S( -2, -11),   S( -1, -16),   S( -4, -22),   S( 19, -14),   S(  2,  -7),   S(  7,  -6),   S(  2, -11),
            S(  9,  27),   S(  0,  -5),   S( 22,  -6),   S(  4,  -1),   S( 11,  -8),   S( -2, -13),   S( -9,  16),   S(  1,   1),
            S(  7, -39),   S( 14,  25),   S( 10,  24),   S( -1,  -9),   S( 14,   1),   S( -2,   3),   S(  5, -23),   S( -6, -13),
            S( -1,   0),   S(  0, -24),   S(-13, -45),   S( 10,  11),   S( -2,  -3),   S( -9, -32),   S( -6, -27),   S( 12,  36),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  3, 105),   S(-18,   6),   S(  4,  10),   S(  4,  10),   S( 17, -23),   S(-27,  26),
            S(  0,   0),   S(  0,   0),   S( 17,  94),   S( -4, -24),   S( 18,  15),   S(  7, -11),   S( 33,  14),   S( -8,  32),
            S( -4,  -6),   S( -1, -49),   S( 13,  22),   S(  3,  17),   S( 16,   6),   S(  7,  23),   S( -9,  -7),   S(-19,   2),
            S( -2, -10),   S( 11,  19),   S( -6, -24),   S(  2, -61),   S( -8, -29),   S(  5, -16),   S(-10,  22),   S( -9,  18),
            S( 12,  43),   S( -1,  17),   S(  3,  29),   S(  9,   0),   S(-10,  -7),   S(-10,   4),   S(-13,  20),   S( -2,  -4),
            S(  6,  23),   S(  6,  24),   S(  2,  16),   S(-10, -22),   S( -3, -17),   S( -6,  24),   S(-12, -14),   S(  7,  35),
            S(  9,  27),   S( -3,  -7),   S( -3, -12),   S( -1,  17),   S( -9, -47),   S( 13,  18),   S( -1, -25),   S(  2, -15),
            S( -3,  -9),   S( -2, -11),   S(  3,  -5),   S( -1, -21),   S(  0,  -5),   S(  4,   4),   S( -2,   0),   S(  4,   9),

            /* kings: bucket 13 */
            S( -7,  85),   S( -6,  75),   S(  0,   0),   S(  0,   0),   S(  6,  96),   S(-30, -13),   S( 20,   1),   S(  3,  30),
            S( -2,  36),   S( -3,  -9),   S(  0,   0),   S(  0,   0),   S( 11, -11),   S(-14, -19),   S(-19,  14),   S( -7,  12),
            S( -8,   3),   S(  8,  28),   S( -8, -48),   S(  0,  14),   S(  2,  -5),   S(-10,  13),   S(-10,  -4),   S( -4,  14),
            S(-14, -32),   S( -2,  14),   S(  6,   3),   S( -5, -48),   S( 10, -31),   S( -9, -29),   S( -5,   2),   S(-17, -15),
            S(  5,  20),   S(  0,  15),   S( 19,  47),   S( -5, -23),   S( -9,  13),   S(  0,  -5),   S( -8, -15),   S(  9,  27),
            S(  5,  30),   S(-13,  -8),   S(-10, -12),   S( -4,  -6),   S(-10, -25),   S( -8,   1),   S( -9, -11),   S(  3,  19),
            S(  8,  18),   S( -8,  -6),   S(-12, -20),   S(  2,   8),   S(  5, -29),   S(  1,  -7),   S( -7, -48),   S(  6,  40),
            S(  2,  -1),   S( -1,  10),   S( 10,  38),   S(  1, -26),   S(  0,  -4),   S( -3,  -5),   S(  5,  17),   S(  6,  19),

            /* kings: bucket 14 */
            S( -3,  49),   S(-14,   4),   S(-16,  -7),   S( -2,  37),   S(  0,   0),   S(  0,   0),   S(-10, 102),   S(-65,  77),
            S( -9,   7),   S(  4,  -5),   S( -3,  -8),   S(  5,  -5),   S(  0,   0),   S(  0,   0),   S( 22,  15),   S(-32, -16),
            S(-11,   1),   S(  6, -14),   S( 19,  -1),   S( 13,   4),   S(  2, -28),   S(  3,  26),   S(  8,  55),   S(-21, -12),
            S(  9,  21),   S(  2, -12),   S( -4, -46),   S(  4, -12),   S(-16, -50),   S(  9,  21),   S( -4,  -1),   S(  7,  -2),
            S(  7,  34),   S( -2,  -3),   S( -4,  -7),   S(  0, -21),   S(  6,  11),   S( 10,  30),   S(  4,  56),   S(  3,  13),
            S(-10, -26),   S( -5,  25),   S( -1,  -8),   S(  0,  40),   S( -9, -12),   S( -4, -14),   S( -5, -38),   S( -1,  -1),
            S(  4,  20),   S(-19, -64),   S(  1,  26),   S( -2,  -3),   S( -5,  -8),   S(  3, -14),   S(-13, -56),   S( 16,  71),
            S(  1,   7),   S(  6,  68),   S(  0,  22),   S( -4,   1),   S(  3,  34),   S( -1, -13),   S(-14, -53),   S(  2, -17),

            /* kings: bucket 15 */
            S(  1,  40),   S(  6,  16),   S( 13,   1),   S(-14, -12),   S(-27,  -2),   S(-17, 106),   S(  0,   0),   S(  0,   0),
            S( -1, -24),   S(-14, -23),   S(-15, -12),   S( 23,  24),   S( 32, -31),   S( 36, 108),   S(  0,   0),   S(  0,   0),
            S(-18,   2),   S(  6,   4),   S( -2, -31),   S( -3,  12),   S( 16, -28),   S( 24,  52),   S( 15,   9),   S(-13, -12),
            S(  8,  16),   S(-15, -11),   S(  6,  18),   S( -5, -31),   S(  4, -46),   S( 11,  22),   S(  9,  63),   S(-11, -54),
            S(  7,  37),   S(-20,   7),   S(  8,  21),   S( -7, -12),   S(  4,   7),   S( -5,  13),   S( -8,  -6),   S(  3,  27),
            S( -1,  14),   S(-17, -16),   S(  6,  16),   S(  3,  -1),   S(-18, -23),   S( -7,  -4),   S(  0,  20),   S( -1, -14),
            S(  2,  13),   S(  2,  26),   S( -6,   5),   S(  3,  -6),   S(  5,   1),   S(  1,   4),   S( -2,  -1),   S(  0,   3),
            S(-10, -12),   S(  3,   3),   S( -6, -16),   S( -1, -10),   S( -3,  24),   S(  6,  28),   S( -2,  -5),   S(  4,   3),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-13, -192),  S(  9, -41),   S( 19,  37),   S( 27,  76),   S( 34,  99),   S( 37, 123),   S( 44, 121),   S( 55, 112),
            S( 79,  79),

            /* bishop mobility */
            S(  7, -91),   S( 12, -19),   S( 21,  29),   S( 27,  69),   S( 32,  94),   S( 35, 115),   S( 37, 126),   S( 38, 128),
            S( 40, 132),   S( 46, 128),   S( 55, 122),   S( 76, 116),   S(101, 107),   S(139,  87),

            /* rook mobility */
            S(-26,  10),   S(-17,  74),   S(-17, 105),   S(-13, 121),   S(-14, 145),   S( -6, 153),   S( -2, 162),   S(  6, 161),
            S( 11, 169),   S( 17, 173),   S( 26, 175),   S( 37, 172),   S( 47, 167),   S( 61, 157),   S(142, 118),

            /* queen mobility */
            S( 40, -25),   S( 44, -61),   S( 40,  52),   S( 36, 124),   S( 38, 163),   S( 37, 202),   S( 37, 238),   S( 38, 259),
            S( 39, 278),   S( 36, 302),   S( 35, 317),   S( 31, 340),   S( 28, 354),   S( 29, 361),   S( 25, 376),   S( 25, 380),
            S( 25, 382),   S( 39, 365),   S( 50, 356),   S( 69, 337),   S( 91, 315),   S(130, 285),   S(159, 253),   S(191, 236),
            S(189, 230),   S(178, 216),   S(128, 210),   S( 89, 188),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  26),   S(-22,  62),   S(-34,  55),   S(-31,  70),   S( 18,  25),   S(-15,  37),   S(  5,  69),   S( 32,  33),
            S( 17,  38),   S( -2,  62),   S(-21,  55),   S(-18,  47),   S( -2,  46),   S(-38,  59),   S(-32,  80),   S( 37,  38),
            S( 24,  86),   S( 12,  94),   S(  3,  69),   S( 18,  54),   S( -1,  62),   S(-33,  80),   S(-43, 118),   S(  0,  89),
            S( 38, 124),   S( 52, 131),   S( 15,  97),   S( 10,  65),   S( -3,  77),   S(  6,  96),   S(-25, 129),   S(-63, 157),
            S( 58, 157),   S( 91, 172),   S( 84, 125),   S( 28, 106),   S(-67, 107),   S( 12, 100),   S(-87, 169),   S(-128, 166),
            S( 94, 215),   S( 77, 239),   S(137, 215),   S(139, 218),   S(146, 228),   S(122, 230),   S(106, 237),   S(113, 238),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  20),   S(  2,  -2),   S( 10,   3),   S(  5,  31),   S( 18,  43),   S( -1, -31),   S(-19,  37),   S( 11, -33),
            S(-14,  46),   S( 30,  -1),   S( -5,  59),   S( 23,  37),   S( 31,   8),   S(  4,  30),   S( 15,   2),   S(  9,   5),
            S( -3,  15),   S( 15,  10),   S(  0,  56),   S( 18,  62),   S( 30,  32),   S( 24,  27),   S( 28,   5),   S( -3,  23),
            S(  6,  31),   S( 39,  48),   S( 24,  85),   S( 19,  99),   S( 76,  60),   S( 65,  52),   S( 24,  45),   S( 13,  34),
            S( 58,  59),   S(111,  93),   S( 91, 153),   S(168, 207),   S(175, 149),   S(143, 163),   S(199, 128),   S(121,  47),
            S( 89, 201),   S(145, 282),   S(136, 246),   S(137, 250),   S( 89, 199),   S( 60, 160),   S( 61, 190),   S( 28, 134),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  50),   S( 24,  37),   S( 40,  57),   S( 39,  43),   S( 28,  38),   S( 27,  39),   S( 12,  21),   S( 47,  13),
            S(  1,  20),   S( 14,  44),   S( 13,  37),   S( 12,  46),   S( 25,  18),   S( 11,  24),   S( 29,  22),   S(  0,  13),
            S(  1,   3),   S( 17,  31),   S( 52,  38),   S( 41,  29),   S( 43,  41),   S( 58,   8),   S( 12,  23),   S( 15,   7),
            S( 76,  23),   S(121,   2),   S(126,  89),   S(166,  97),   S(157,  84),   S( 99,  81),   S( 93,  16),   S( 82, -15),
            S( 70,  57),   S(154,  51),   S(214, 153),   S(178, 157),   S(179, 170),   S(116, 179),   S(222, 109),   S( -6, 107),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 23,  32),   S(  5,  23),   S( 11,  29),   S( -4,  63),   S( 70,  31),   S( 25,   7),   S( -8,   4),   S( 27,  22),
            S(  3,  12),   S(  7,   7),   S( 20,  16),   S( 17,  31),   S( 14,  16),   S( -1,   9),   S(  4,   8),   S( 30,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -3, -12),   S( -7,  -7),   S(-20, -16),   S(-17, -31),   S(-14, -16),   S(  1,  -9),   S( -4,  -8),   S(-30,   6),
            S(-23, -32),   S( -5, -23),   S(-11, -29),   S(  4, -63),   S(-70, -31),   S(-25,  -7),   S(  8,  -4),   S(-27, -22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -21),   S( -7, -14),   S(  0, -28),   S(-53,  -8),   S(-18, -12),   S(-21, -23),   S( -2, -24),   S(-12, -41),
            S(-19,   4),   S(-14,   0),   S(-25,  12),   S(  4, -16),   S(-30, -16),   S(-20,  -6),   S(-37,   3),   S(-11, -16),
            S(-12, -21),   S( -6, -21),   S(-26,  -3),   S(-29, -10),   S(-25, -32),   S(-22, -11),   S(-17,  -6),   S(-42, -20),
            S(  4, -35),   S( 18, -39),   S( 17, -23),   S(  6, -27),   S(  3, -15),   S( 54, -35),   S( 30, -32),   S(-11, -45),
            S( 29, -64),   S( 55, -74),   S( 52, -34),   S( 60, -24),   S( 69, -27),   S( 70, -16),   S(123, -70),   S( 23, -65),
            S(120, -108),  S(179, -126),  S(113, -53),   S( 98, -46),   S( 73, -28),   S(174, -41),   S(120, -38),   S( 65, -82),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 16,   3),        // attacks to squares 1 from king
            S( 17,   1),        // attacks to squares 2 from king

            /* castling available */
            S( 70, -70),        // king-side castling available
            S( 19,  40),        // queen-side castling available

            #endregion

            /* tempo bonus for side to move */
            S(  7,  10),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
