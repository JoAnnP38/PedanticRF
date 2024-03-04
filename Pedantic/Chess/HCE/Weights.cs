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
        public const int MAX_WEIGHTS = 12758;
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
        public const int BISHOP_PAIR = 12756;       // bonus for having both bishops
        public const int TEMPO = 12757;             // tempo bonus for side moving

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

        public Score BishopPair
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[BISHOP_PAIR];
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

        // Solution sample size: 6000008, generated on Mon, 04 Mar 2024 00:43:44 GMT
        // Solution K: 0.003850, error: 0.085835, accuracy: 0.4960
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 95, 175),   S(421, 531),   S(411, 547),   S(562, 962),   S(1335, 1649), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(102, -118),  S(153, -94),   S( 32, -36),   S(-28,   6),   S(-28,  16),   S(-21, -11),   S(-48,  -9),   S(-45, -20),
            S(109, -118),  S( 99, -98),   S(  0, -55),   S(-21, -44),   S(-17, -26),   S(-20, -34),   S(-40, -29),   S(-38, -45),
            S( 95, -82),   S( 69, -53),   S( 19, -55),   S(  4, -63),   S(  1, -67),   S(  1, -56),   S(-23, -48),   S(-25, -41),
            S( 65, -35),   S( 52, -46),   S( 17, -40),   S( 21, -73),   S( -3, -57),   S(-28, -51),   S(-38, -31),   S(-40, -17),
            S( 63,  47),   S( 40,  -4),   S( 58, -31),   S( 47, -63),   S( 12, -43),   S(-20, -32),   S(-34, -29),   S(-65,  46),
            S( 66,  42),   S( 47,  58),   S( -6,   2),   S( 41, -33),   S(-42,  -1),   S(-36,  -8),   S( 17, -17),   S( -7,   6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25, -25),   S( 26, -38),   S( 37, -19),   S(-21,  21),   S( -8, -18),   S(  6, -18),   S(-35,  -8),   S(-38,  19),
            S( 15, -32),   S(  9, -41),   S(  6, -40),   S( -9, -39),   S(-14, -21),   S(-11, -30),   S(-34, -22),   S(-44, -11),
            S( 15, -24),   S( 21, -31),   S( 24, -49),   S(  6, -47),   S(-21, -22),   S( 10, -38),   S(-11, -28),   S(-21,  -9),
            S( 16,   8),   S( 11, -32),   S( 12, -28),   S(  0, -41),   S( -4, -19),   S( -5, -34),   S(-32, -16),   S(-37,  16),
            S( -6,  63),   S(-21, -17),   S(  5,  -8),   S(-17, -24),   S( 31, -12),   S(-21,   1),   S(-44,  11),   S(-39,  62),
            S( 59,  51),   S( 30, -16),   S(-45, -23),   S(  5,  -7),   S(-18, -14),   S(-62,  10),   S(-50,  20),   S(-69,  62),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31,   4),   S(-34,   1),   S(-20,  -2),   S(-16,   3),   S(  6, -13),   S( 31, -19),   S(  5, -44),   S(-18, -15),
            S(-32, -15),   S(-53,  -4),   S(-31, -33),   S(-24, -28),   S(  2, -26),   S(  3, -23),   S(-14, -34),   S(-32, -22),
            S(-40,  -3),   S(-30, -18),   S(-11, -45),   S( -5, -56),   S(  3, -26),   S( 12, -23),   S(-10, -26),   S(-17, -12),
            S(-52,  20),   S(-35, -23),   S(-34, -27),   S( -4, -47),   S( 14, -35),   S( -9, -16),   S(-17,  -4),   S(-29,  17),
            S(-41,  57),   S(-67,  13),   S(-57, -30),   S(-60, -14),   S( -6,  13),   S(-22,  28),   S(-40,  16),   S(-54,  89),
            S(-62,  82),   S(-137,  94),  S(-130,  13),  S(-76, -15),   S(-38,   7),   S( 10,   2),   S( -8, -19),   S(-20,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30,  -8),   S(-40,  -1),   S(-32,  -6),   S( -8, -34),   S(-14,   0),   S( 31, -11),   S( 82, -60),   S( 59, -75),
            S(-36, -30),   S(-54, -19),   S(-35, -38),   S(-24, -27),   S(-21, -19),   S(  2, -32),   S( 54, -63),   S( 54, -73),
            S(-38, -25),   S(-22, -44),   S( -6, -61),   S( -5, -68),   S( -7, -47),   S(  7, -36),   S( 28, -45),   S( 56, -52),
            S(-43,  -6),   S(-26, -46),   S(-21, -53),   S(  3, -70),   S( 18, -64),   S( -3, -42),   S( 11, -23),   S( 43, -12),
            S(-25,  38),   S(-39, -15),   S( -5, -51),   S(  1, -59),   S( 66, -47),   S( 45, -24),   S( 38,  16),   S( 34,  71),
            S(-53, 111),   S(-42,  30),   S(-10, -57),   S(-58, -32),   S( 27, -59),   S( 64, -27),   S( 73,  -8),   S( 46,  83),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87,  19),   S(  6, -25),   S( 19,  -8),   S(-18,  -3),   S(  3, -37),   S(-14,  -3),   S(-76,   6),   S(-46,  -6),
            S( -4,  -4),   S( 36,  -9),   S( 22, -27),   S( 11, -16),   S( -5, -22),   S(-40, -25),   S(-41, -29),   S(-15, -27),
            S( 44, -11),   S( 40,  -3),   S( 15,  -5),   S(-11, -19),   S(-36, -20),   S(  0, -47),   S(-38, -31),   S(-35, -18),
            S( 31,  27),   S(  2,  42),   S( 47,   4),   S(  6,   5),   S( 14, -30),   S(-41, -15),   S(-22, -28),   S( 44, -26),
            S( 15,  75),   S( -7,  68),   S(  7,   6),   S(-17,   4),   S( 16,   0),   S(  6,  -5),   S(-45, -37),   S( -3,  27),
            S( 60,  91),   S( 73,  52),   S( 46,  29),   S( 37,   1),   S( 18, -34),   S( 24,   1),   S( 21,  10),   S(-35,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-73,  46),   S(-47,  21),   S(-20,  21),   S( -4,  15),   S(-39,  26),   S(-16,  10),   S(-21,  -4),   S(-40,  26),
            S(-70,  21),   S(-59,  15),   S( 35, -18),   S(-21,  12),   S( 14,  -5),   S(-25, -13),   S(-18, -17),   S(-56,  17),
            S(-51,  46),   S(-20,  18),   S( 68, -29),   S( 12, -26),   S( 32, -11),   S(-34,  -1),   S(-13,  -6),   S(-37,  16),
            S(-40,  61),   S( -1,  24),   S( 18,  10),   S( 53,  10),   S(  2,   2),   S(-44,   7),   S( 25, -11),   S( -3,  21),
            S( 51,  61),   S( 55,  40),   S( 63,  41),   S( 37,  27),   S(-15,  55),   S( 55,  -1),   S(  7,   2),   S( 23,  43),
            S( 94,  42),   S( 94,  30),   S( 63,  -9),   S( 34,  -3),   S( 35, -22),   S( 26,   1),   S( -9,  -8),   S( 50,  34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44,  23),   S(-25,   3),   S( -3,  -1),   S( -3, -14),   S(  8, -10),   S(-14,   5),   S(-43,  -1),   S(-67,  24),
            S(-47,   1),   S(-11, -22),   S(-10, -32),   S( 32, -21),   S( 40, -15),   S( 30, -20),   S(-29, -12),   S(-80,  16),
            S(-54,  18),   S(  2, -13),   S( -8, -18),   S( -8, -13),   S( 31, -11),   S( 65, -26),   S(  6, -13),   S(-48,  23),
            S(-30,  27),   S(-42,  11),   S( 20, -13),   S(  2,  -3),   S( 40,   3),   S( 44,  -4),   S( 31,  -2),   S( 20,  17),
            S(-63,  55),   S(-44,  19),   S(-19,   2),   S(  2, -12),   S( 47,  30),   S( 82,  31),   S( 54,  -5),   S( 43,  45),
            S( 50,  49),   S( 40,   2),   S( 11, -32),   S( 38, -52),   S( 56, -24),   S( 65,  -1),   S( 37,  -4),   S( 80,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58, -17),   S(-53, -18),   S(-13, -16),   S(-68,  -9),   S(-29, -19),   S( 24, -22),   S(-11, -36),   S(-44, -14),
            S(-65, -28),   S(-50, -33),   S(-36, -45),   S(-10, -46),   S( -1, -29),   S( 51, -43),   S( 42, -38),   S(-28, -20),
            S(-72, -22),   S(-49, -35),   S(-19, -52),   S( -6, -50),   S( -9, -24),   S( 26, -19),   S( 41, -38),   S( 34, -27),
            S(-55, -15),   S(-51, -38),   S(-83, -29),   S(-52,  -6),   S( -4, -19),   S( -1,   0),   S( 20,   2),   S( 66,  -9),
            S(-36,  -4),   S(-26, -34),   S(-45, -25),   S( 19, -73),   S( 62,  -5),   S( 32,  11),   S( 93,  30),   S(118,  38),
            S( 24,  -9),   S(-19, -30),   S( 21, -61),   S( 30, -47),   S( 34, -31),   S( 34, -40),   S( 49,  47),   S( 80,  70),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  37),   S(-27,  49),   S( -6,  24),   S(-14, -16),   S( -4, -11),   S(-42,  14),   S(-73,   1),   S(-75,  20),
            S(-56,  38),   S(-24,  19),   S(-46,  16),   S(-28,   7),   S(-32,  -2),   S(-39, -28),   S(-23, -30),   S(-17,   1),
            S(-47,  72),   S( 21,  79),   S( 23,  44),   S(-25,  30),   S(  1,   4),   S(-33, -28),   S(-18, -49),   S(  6, -24),
            S( 12, 101),   S( 59, 120),   S( 88,  63),   S( 13,  59),   S(  3,   8),   S(-35, -19),   S( 15, -18),   S( 22, -20),
            S( 76, 124),   S( 70, 113),   S( 65, 114),   S( 36,  88),   S(  6, -15),   S( 14, -10),   S(-14, -19),   S(  8,   1),
            S(102, 148),   S( 96, 162),   S( 88, 175),   S( 43,  83),   S( -5,  10),   S( -4, -46),   S( -4, -23),   S( -6, -48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-69,  54),   S(-61,  37),   S(-34,   1),   S( 10,  15),   S( -2,   4),   S(-103,  31),  S(-104,  21),  S(-95,  30),
            S(-45,  11),   S(-21,  11),   S(-44,  22),   S( -6,  36),   S(-103,  19),  S(-77,  10),   S(-106,  -1),  S(-26,   5),
            S(-48,  54),   S(-23,  64),   S(-33,  79),   S(-71,  74),   S(-65,  60),   S(-105,  19),  S( -9, -15),   S(-26,   3),
            S( 15,  76),   S( 55,  74),   S( 74, 112),   S( 64, 138),   S(-27,  62),   S( -4,  16),   S( 33,   5),   S( 57, -14),
            S(101,  67),   S( 59, 111),   S( 85, 125),   S(108, 109),   S( 43, 107),   S( 42,  34),   S( 22, -35),   S( 18, -12),
            S( 24,  46),   S( 71,  78),   S(125,  98),   S(100, 128),   S( 60, 104),   S( -3, -13),   S( 11, -13),   S(-10, -37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-146,  18),  S(-124,  10),  S(  0,  -7),   S( 21,  47),   S(-27,   2),   S(-117,  41),  S(-122,  33),  S(-117,  44),
            S(-105,  -5),  S(-51, -18),   S(-43, -11),   S(-51,  -3),   S(-21,  23),   S(-24,  16),   S(-141,  33),  S(-106,  30),
            S(-15,  -8),   S(-53,   2),   S(-20,  -1),   S(-80,  53),   S(-85,  85),   S( 14,  40),   S(-94,  38),   S(-62,  45),
            S( 30,  -7),   S( 12,  -4),   S(-13,  32),   S(  1,  74),   S( 49,  97),   S(-26,  97),   S(  8,  42),   S( 40,  22),
            S( 89, -20),   S( 36, -14),   S( 46,  44),   S( 76,  97),   S(136, 108),   S( 65,  71),   S( 31,  63),   S( 57,  51),
            S( 46,  -9),   S( 12, -33),   S( 33,  21),   S( 74, 108),   S( 45,  97),   S( 32,  64),   S( 44,  45),   S( 38,  86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-81,  -2),   S(-93,   7),   S( -2,  -3),   S( -9,  10),   S( -2,  34),   S(-98,  43),   S(-70,  27),   S(-69,  43),
            S(-84, -15),   S(-49, -31),   S(-45, -35),   S(-66,  -2),   S(-49,  10),   S( -8,  15),   S(-92,  48),   S(-94,  46),
            S(-49, -24),   S(-20, -45),   S(-39, -10),   S( 22, -14),   S(-46,  40),   S(  1,  48),   S(-34,  64),   S(-55,  65),
            S( 11, -16),   S(-47, -25),   S( -9,   2),   S(-37,   8),   S( -3,  45),   S( 36,  57),   S( 46,  98),   S(110,  62),
            S( 26,   3),   S(-22,  -3),   S(-10,  -7),   S(-18,  19),   S( 61,  67),   S( 67,  71),   S( 94,  93),   S(157,  63),
            S(  6, -32),   S( -1, -29),   S( 14, -37),   S(  2, -34),   S( 36,  41),   S( 26,  98),   S( 75, 136),   S(122, 122),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -13),   S(-23,  -5),   S(-30,  -9),   S( -4,  -7),   S(-12, -12),   S(-13, -10),   S( -9,  -6),   S(-13,  -3),
            S(-36, -42),   S(-16, -14),   S(  0,   5),   S( -1, -15),   S(-11,  -3),   S( -8,   0),   S(-18, -19),   S( -4, -67),
            S(-29,  43),   S(  8,  63),   S( 19,  46),   S( 20,  37),   S( 11,   3),   S(-34, -34),   S(-18, -57),   S(-33, -69),
            S( -3,  71),   S( 37,  66),   S( 65, 107),   S( 39,  78),   S( -8, -27),   S(-15, -49),   S(  0,  -1),   S(-37, -59),
            S( 71,  82),   S( 76, 195),   S( 77, 141),   S( 14,  46),   S(  8,   5),   S(  1, -34),   S(  2, -29),   S(-23, -92),
            S( 55, 125),   S( 47, 248),   S(114, 241),   S( 52,  96),   S( -4,  -3),   S(-10, -50),   S( -9, -47),   S( -3, -64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -11),   S(-19,  17),   S( 14,  35),   S( -3,   2),   S(-11, -13),   S(-56, -32),   S(-38, -33),   S(-26,  -3),
            S(-19, -35),   S(-25,   0),   S( -8, -14),   S( 25,  50),   S(-51, -12),   S(-30, -21),   S(-49, -42),   S(-39, -16),
            S( 16,  13),   S( 23, -15),   S( -1,  10),   S( 21,  63),   S(-34,  28),   S( -3, -26),   S(-29, -44),   S( -5, -57),
            S( 24,  42),   S( 67,  57),   S( 47,  79),   S( 16,  81),   S( 20,  60),   S(  9, -12),   S(  5, -28),   S( 24, -36),
            S( 43,  41),   S( 70, 113),   S( 98, 186),   S( 97, 201),   S( 54, 110),   S(  2,  -9),   S(  1, -69),   S(  6, -60),
            S( 28,  73),   S( 85, 142),   S( 91, 207),   S(120, 242),   S( 61, 125),   S(  8,   2),   S(  3, -17),   S( 13, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39, -38),   S(-12, -42),   S( -1,  -9),   S( -2,  -8),   S( -2,   1),   S(-37,   5),   S(-26,   8),   S( -5,  14),
            S(-17,  -4),   S(-13, -40),   S(-42, -74),   S(-14,  -4),   S( -9,  27),   S(-13,  14),   S(-15,  38),   S(-27,   9),
            S(-10, -25),   S(-18, -21),   S(-23, -25),   S(  1,   6),   S(-17,  46),   S( -5,  13),   S(-14,   5),   S(  0,  21),
            S( 22, -36),   S( -1, -24),   S(  8, -25),   S( 15,  57),   S( 14, 130),   S( 24,  78),   S(  5,  36),   S(  9,  12),
            S( 15, -37),   S( 21, -17),   S( 34,  17),   S( 51, 130),   S( 78, 202),   S( 91, 150),   S( 50,  62),   S( 41,  39),
            S( 19,   4),   S( 12, -29),   S( 37,  83),   S( 39, 111),   S( 79, 215),   S( 51, 154),   S( 42, 103),   S( 27,  61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -58),   S(-30, -25),   S(-11, -56),   S( -4,  -5),   S( 14,  37),   S( -6,  40),   S(-34, -27),   S( 22,  44),
            S( -3, -64),   S(-41, -47),   S(-18, -62),   S( 13,  27),   S(-18,  31),   S( 10,  32),   S( 15,  44),   S(-22,  17),
            S( -6, -65),   S( -5, -75),   S(-21, -42),   S( -5,  27),   S( 23,  25),   S( 30,  30),   S( 25,  62),   S(  5,  62),
            S( 18, -54),   S(-26, -45),   S(-11, -30),   S( 16,  25),   S( -3,  33),   S( 37,  35),   S( 23, 104),   S(  4,  72),
            S(-21, -86),   S( -9, -72),   S(  0, -13),   S(  9,   5),   S( 50, 118),   S( 95, 125),   S( 50, 245),   S( 69,  82),
            S(  4, -37),   S(  9,   9),   S(  9,  11),   S( 11,  38),   S( 32,  75),   S( 76, 239),   S( 22, 227),   S( 53,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S( -9, -30),   S( -5,  30),   S(-30,  10),   S(-30,  -4),   S(-43,  -7),   S( -6, -42),   S(-65, -64),   S(-49, -25),
            S(-33,  57),   S( 46, -73),   S(-41,  -1),   S(  6, -26),   S( -2, -44),   S(-28,  -2),   S(-32, -35),   S(-68, -45),
            S( 21,  37),   S( -4, -36),   S( 14, -38),   S( -3,  11),   S( 11,   0),   S(-26,   2),   S( -1, -38),   S(-39, -51),
            S( 18, -28),   S( 31,   5),   S( 20,   8),   S( 19,  35),   S( 17, -16),   S( -6,  -7),   S(  9, -36),   S(-11,   3),
            S( -5, -22),   S( 41, -14),   S( 18, -10),   S( 63, -26),   S( 50,  -7),   S( 29,   8),   S(  7,  -5),   S(-72, -18),
            S( 19, -23),   S( 11,  -5),   S( 43,   6),   S( 40,   1),   S( 32, -54),   S( 38,  -3),   S(  6, -31),   S(-16, -12),
            S( 35,  13),   S( 25, -44),   S( 32, -44),   S( 46, -14),   S( 32, -43),   S( -6, -53),   S(-30, -44),   S(-23, -41),
            S(-87, -80),   S(-20, -22),   S(-11, -27),   S(-23, -66),   S(-39, -23),   S(  9, -12),   S( -5,  -7),   S( 24,   6),

            /* knights: bucket 1 */
            S(-91,  30),   S(-42,  87),   S(  0,  33),   S( -4,  37),   S( -6,  43),   S(-36,  23),   S(-32,  44),   S(-13, -56),
            S( -5,  32),   S(-11,  41),   S( -1,  -1),   S(-10,  25),   S( -2,  16),   S(  8,  -4),   S( -7, -36),   S(-29,   1),
            S(-27,  31),   S(  6,   7),   S(  4,   9),   S( 20,  15),   S( 12,  13),   S(-28,  27),   S(-10, -14),   S(-21,  -1),
            S(  1,  27),   S( 47,  12),   S( 16,  43),   S( 26,  22),   S( -3,  26),   S( -5,  29),   S( 31,   6),   S(  5,  30),
            S( -4,  41),   S( 14,   3),   S( 44,  11),   S( 35,  22),   S( 33,  36),   S( 25,  27),   S( 15,   3),   S(  3,  14),
            S(-13,  22),   S( 32,  -4),   S( 32,  36),   S( 50,   7),   S( 15,   4),   S( 36,  35),   S( 49, -22),   S( 36,  -9),
            S( 45,  -8),   S( 22,   7),   S(-12,  11),   S(  9,  18),   S( 34, -18),   S( 52, -21),   S(-16,  16),   S( -1, -40),
            S(-82, -72),   S(-22, -29),   S( -3,   7),   S( 17,  -7),   S(-15,   2),   S(  2,  20),   S( -5, -11),   S(-55, -52),

            /* knights: bucket 2 */
            S(-37,   4),   S( -7,  28),   S(-16,  37),   S(-30,  56),   S(-32,  47),   S(-29,  58),   S(-20,  38),   S(-46,  19),
            S(-11, -19),   S(-30,  35),   S( -2,   9),   S(-14,  31),   S( -5,  14),   S(-18,  37),   S(-28,  36),   S(-41,  71),
            S(-31,  39),   S( -1,  16),   S(-15,  34),   S( 12,  24),   S(  3,  24),   S( -3,  15),   S( -8,  27),   S(-34,  41),
            S( -6,  51),   S(-22,  40),   S(  3,  47),   S(  4,  51),   S( -8,  53),   S(  3,  36),   S(-13,  60),   S( -8,  51),
            S( 15,  15),   S(-23,  35),   S(-25,  55),   S(-24,  59),   S( -9,  58),   S(-21,  53),   S(-11,  51),   S(-14,  23),
            S(  5,  16),   S(-10,  35),   S(-21,  56),   S( -6,  36),   S( 11,  36),   S( -1,  35),   S(-26,  21),   S( 14,  13),
            S(-28,  26),   S(-21,  -4),   S(  6,   1),   S(-11,  18),   S(  4,  -4),   S( 22,  32),   S(-60,  35),   S(-53,  32),
            S(-153,   2),  S(-14,  20),   S(-67,  30),   S(-11,  -3),   S(-12, -15),   S(-50,   0),   S(-15, -12),   S(-199, -40),

            /* knights: bucket 3 */
            S(-72,   0),   S( -5, -23),   S(-46,  39),   S(-12,   9),   S(-12,  22),   S(-17,  23),   S( -3,  16),   S(-27, -15),
            S(-27,   2),   S(-41,  23),   S(-15,  -4),   S(  2,  19),   S(  9,  10),   S(-20,   1),   S(-22,   7),   S(-26,  51),
            S(-16, -12),   S( -5,  16),   S( -9,  11),   S( 18,  17),   S( 14,  25),   S(  9,  14),   S( -5,  12),   S(-12,  50),
            S(-11,  11),   S(-10,  32),   S(  5,  44),   S( 14,  46),   S( 12,  46),   S( 12,  44),   S( 13,  35),   S(  8,  37),
            S( 10,   8),   S( -7,  30),   S( 15,  32),   S( 10,  60),   S( 11,  61),   S(  3,  66),   S( 21,  48),   S( -7,  33),
            S(  1,  12),   S( 29,  -3),   S( 35,  19),   S( 63,  12),   S( 67,   4),   S( 45,  19),   S( 18,  16),   S( -1,  54),
            S( 10,   7),   S(  3,  11),   S( 54, -22),   S( 43,  -9),   S( 52, -21),   S( 72, -37),   S( 43, -37),   S( 44, -22),
            S(-152,  77),  S(-16, -17),   S(-32,  -3),   S(  0,   7),   S( 57, -12),   S(-55, -13),   S( 24, -14),   S(-83, -53),

            /* knights: bucket 4 */
            S(  4,   0),   S(-32,  12),   S(  0,   7),   S( 19, -10),   S(-13, -42),   S( -9, -14),   S(  2, -75),   S(-25, -42),
            S( 54,  34),   S(-35,  49),   S(-16,   0),   S( 10, -39),   S( 21, -26),   S( 20, -55),   S(-17, -41),   S(-13, -53),
            S(-22,  44),   S( 14,  24),   S( 16, -13),   S( 37,   4),   S(  5, -13),   S(-34,  11),   S(-55, -51),   S( -3, -64),
            S( 20,  23),   S( 27, -22),   S( 66, -13),   S( 34,  13),   S( 29,  -3),   S(122, -28),   S( 12, -58),   S( -4, -59),
            S( 61,  33),   S(-54,  68),   S( 61,  34),   S( 52,  10),   S( 62,  18),   S(  9,   9),   S(  9, -41),   S( -2, -52),
            S( -6,  34),   S(-28,  -6),   S(117,  -8),   S( 29,  -7),   S( 10,  -5),   S( 39,  26),   S( 21,  34),   S(-23, -44),
            S(-12, -10),   S(-32, -15),   S( -4,  22),   S( 19,  43),   S( 25,   9),   S(  2, -37),   S( -2, -19),   S(-31, -22),
            S(-26, -16),   S( 11,  10),   S( 16,   6),   S(  0,   8),   S( -6, -11),   S( -4, -13),   S( -3,   8),   S(  0, -18),

            /* knights: bucket 5 */
            S( 17, -11),   S(-28,  19),   S( 15,  18),   S(  0,  40),   S( 42,  -7),   S(  5,   1),   S(-13,  34),   S(-23, -44),
            S( 21, -22),   S( 59,  30),   S( -6,  20),   S( 21,   7),   S( 45,  24),   S( 38,  11),   S( 28,  11),   S( 15,  -5),
            S(  8,  24),   S(-22,  41),   S( 55,  15),   S( 70,  10),   S( -1,  28),   S( 19,  20),   S( 36, -13),   S( -7, -32),
            S( 56,  41),   S( 42,  21),   S( 44,  26),   S(  2,  38),   S( 79,   7),   S( 57,  27),   S( 52,  26),   S(  4,  31),
            S( 15,  40),   S( 70,   9),   S( 63,  31),   S( 80,  28),   S(105,  32),   S( 55,  29),   S( 46,  20),   S( 43,  46),
            S( -5,  26),   S(-27,  58),   S( 28,  18),   S( 38,  41),   S( 21,  45),   S( 30,  63),   S( 41,  -4),   S(-12,  16),
            S( 28,  54),   S( -5,  45),   S( 51,  22),   S( 16,  53),   S( -5,  45),   S( 12,  34),   S( 25,  69),   S( 10,  10),
            S( -2,   7),   S( -5,  -1),   S(  2,  28),   S(-19,  12),   S( 15,  35),   S( -9,  42),   S(  9,  44),   S(-16, -10),

            /* knights: bucket 6 */
            S(  9, -12),   S( 14, -27),   S( 54,  30),   S(-33,  29),   S(-17,  36),   S( 31,  44),   S( -7,  21),   S(-19,  31),
            S(-13, -39),   S( 70,  -9),   S( 36,   0),   S(-23,  24),   S( -9,  39),   S( 67,  18),   S( 27,  33),   S( -3,  17),
            S( -4, -35),   S(  9,  -1),   S( 39,   3),   S( 50,  22),   S( 11,  30),   S(-33,  55),   S( 14,  45),   S( 29,  41),
            S(  8,   2),   S( 76,  -4),   S( 74,   9),   S( 83,  16),   S( 43,  23),   S( 10,  42),   S( 30,  53),   S( 19,  50),
            S(  8,  15),   S(112, -31),   S( 80,  11),   S( 91,  24),   S( 96,  39),   S(135,  25),   S( 47,  49),   S( 41,  43),
            S( 29,  15),   S( 48,  -4),   S( 81,  26),   S( 52,  48),   S( 99,  36),   S( 58,  27),   S( 27,  34),   S( 35,  45),
            S(-16,  18),   S(  3,  20),   S(-47,  43),   S( 25,  23),   S( 20,  43),   S( 25,  50),   S( 30,  64),   S( -9,  33),
            S(-34,   0),   S(  5,  40),   S( 28,   8),   S(-14,  28),   S( 29,  25),   S( 16,  38),   S( 22,  52),   S( 18,  35),

            /* knights: bucket 7 */
            S(-18, -65),   S(-205, -23),  S(-84, -34),   S(-50, -37),   S(-18, -23),   S(-18, -14),   S(-20,  -5),   S( -6,  -3),
            S(-28, -82),   S(-27, -56),   S(-56, -36),   S(-12, -13),   S(-39,   9),   S( 43, -44),   S( 13,  28),   S( 14,  15),
            S(-62, -71),   S(-41, -52),   S(-34,  -9),   S( 31, -22),   S( 26, -16),   S( 45, -17),   S( 23,  49),   S( 11,  62),
            S(-49, -25),   S( 46, -52),   S( 40,  -7),   S( 72,  -7),   S( 71,  -8),   S( -5,  12),   S(  7,  15),   S(-38,  29),
            S(-54, -30),   S(-20, -52),   S( 88, -26),   S( 98, -19),   S( 98,  16),   S( 90,   8),   S( 78,  16),   S( 84,  12),
            S(-15, -29),   S( 13, -27),   S( 15,   6),   S( 43,  -4),   S( 96,   6),   S(121, -16),   S( 69, -18),   S( -4,  52),
            S(-42, -40),   S(-38, -25),   S( 14, -11),   S( 46,  21),   S(  4,  17),   S( 66,  -3),   S( 11,  28),   S( 14, -12),
            S(-43, -48),   S(  1, -20),   S(-11, -32),   S( 27,  22),   S( 30,   2),   S( 18,   8),   S( -9, -47),   S( -7,   3),

            /* knights: bucket 8 */
            S(  3,  10),   S( -7,  -7),   S( -6,  -3),   S(  1,  -5),   S(-10, -31),   S(-18, -54),   S( -2,   9),   S( -7, -34),
            S( -4, -15),   S( -9, -26),   S(-14, -30),   S( -7, -25),   S(-36, -41),   S(-17, -64),   S( -7, -53),   S(-21, -56),
            S(  2,  17),   S(-19, -33),   S(  9,   9),   S(-24, -46),   S(  1, -12),   S(-15,  13),   S( -7, -17),   S(  1, -32),
            S(-15,  -6),   S(  5,  -3),   S( -4,  17),   S(-19,  30),   S( 18,  -7),   S(  0,  -9),   S(-12, -31),   S( -8, -33),
            S( 19,  67),   S( -5,   5),   S( 12,  34),   S( 39,  57),   S( 10,  47),   S( -5, -11),   S( 13,   8),   S( -9, -10),
            S( 11,  48),   S(  7,  13),   S( 21,  23),   S( 35,  10),   S( -6,  22),   S(-10,  -5),   S(-12, -50),   S(-15, -16),
            S(  7,  17),   S( 13,  43),   S( -8, -20),   S( 12, -14),   S( 19,   7),   S(  6,   9),   S(  2,   3),   S( -7, -14),
            S(  5,  10),   S( 18,  45),   S(  0,  12),   S(  0,  16),   S(  6,  17),   S( -1,   3),   S(  5,  20),   S( -1,   0),

            /* knights: bucket 9 */
            S(-11, -40),   S(-26, -44),   S(-23, -62),   S(  2,   5),   S(-29, -79),   S(-21, -59),   S( -1, -27),   S( -6, -36),
            S(-18, -46),   S( -6,   5),   S(-11, -53),   S(-10,   7),   S(  8,  -3),   S(  5, -28),   S(  5,  31),   S( -9, -22),
            S(  0, -14),   S( -7, -38),   S( -8, -39),   S( 19,  -1),   S(  7,  11),   S(-45,  11),   S(-19,  -7),   S( -8, -18),
            S(-17, -25),   S( -7,  -2),   S( -4,  25),   S( 14,   9),   S( 22,  -3),   S( 15,  16),   S(-15, -61),   S(  2, -34),
            S(  6,  26),   S( 12,  -6),   S(  3,  24),   S( -6,  26),   S( 10,   4),   S(  5,  10),   S(  9, -14),   S( -2, -14),
            S( -3,  18),   S( 19,  35),   S( 34,  34),   S( 13, -33),   S( 23,  11),   S( 11,  25),   S( -1,  12),   S(-11, -34),
            S(  0,  -1),   S(-16, -15),   S( 18,  51),   S( -2,  28),   S( 12,  46),   S(-21, -49),   S(  7,  32),   S( -3,  -6),
            S( -1,  -9),   S(  0,  16),   S(  6,  25),   S( 16,  42),   S( 11,  16),   S(  7,  30),   S( -1,   3),   S( -1, -10),

            /* knights: bucket 10 */
            S(-22, -62),   S(-17, -56),   S(-21, -68),   S(-32, -74),   S( -2,  -6),   S( -4, -34),   S( -4,   0),   S(  4,   7),
            S(-10, -56),   S(-11, -55),   S(-11, -12),   S(-17, -22),   S(-22, -39),   S( -5, -51),   S(  0,   0),   S( -3, -12),
            S(-29, -98),   S(-13, -77),   S(-31,  -3),   S( -2,   1),   S( 11,  13),   S(-16, -37),   S(-22, -37),   S( -5, -18),
            S(-13, -23),   S( -3, -36),   S( -9, -28),   S( 26,  -7),   S( 21,  10),   S( 29,   2),   S(  1, -19),   S( 17,  49),
            S(-16, -48),   S(-11, -36),   S(  0,  -2),   S( 60,  -4),   S( 31,  10),   S( 24,   6),   S( 20,   9),   S( 18,  46),
            S(-19, -65),   S(  8,  21),   S(-11,  -8),   S( -1,  15),   S( 27,  50),   S( 27,  12),   S( 21,  79),   S( 17,  72),
            S(  4,   1),   S( -9, -29),   S(  9,  -3),   S( 21,  47),   S( 11,  50),   S( 18,  36),   S(  3,  13),   S(  9,  24),
            S( -4, -23),   S(  3,  12),   S( -3,  -5),   S(  7,   6),   S( 10,  36),   S(  4,  53),   S(  1,  14),   S( -2,   2),

            /* knights: bucket 11 */
            S(  0,   4),   S( -2,  -7),   S(-22, -88),   S(-20, -37),   S(-28, -40),   S( -5, -24),   S(  2,  -3),   S( -6,   2),
            S(-11, -21),   S(-10, -29),   S(-25, -82),   S(  1,   5),   S(-10,  -7),   S(-40, -54),   S(-14, -45),   S( -7,   0),
            S(-13, -63),   S(-19, -42),   S(-31, -14),   S(  3,  18),   S(-33,  -8),   S(-22,  33),   S( 18, -17),   S(-14,  -7),
            S(-25, -70),   S(-18, -53),   S(-31, -21),   S( 17,   8),   S( 31,  15),   S( 10,  43),   S( -8, -16),   S(  3,   6),
            S( -1, -15),   S(-23, -56),   S( 20,  -5),   S( 21, -11),   S( 18,  14),   S( 56,  32),   S(-13, -13),   S( 26,  71),
            S( -7,  15),   S( -5, -26),   S( -7,  -6),   S( 39,  33),   S( -2,   3),   S( 58,  37),   S( 16,  31),   S(  3,  48),
            S( 15,  64),   S( -6,  -6),   S( -5, -25),   S( 13,  15),   S( 20,  35),   S( -4,  10),   S( 20,  55),   S( 12,  86),
            S( -4,  -3),   S( -3, -13),   S( 12,  34),   S(  6,  21),   S( -1,   6),   S(  1,   0),   S( 10,  19),   S(  5,  38),

            /* knights: bucket 12 */
            S( -2,  -4),   S(  2,   4),   S( -3, -23),   S( -3,   8),   S( -4,  -5),   S(  0,   1),   S(  1,   4),   S( -1, -10),
            S(  0,   6),   S(  0,  -1),   S(  1,  -1),   S(  1, -12),   S( -1,  20),   S(-10, -41),   S( -2, -17),   S(  2,  -8),
            S( -1,   4),   S(  3,   7),   S(  0,   5),   S(  2,  13),   S(  1,  -1),   S( -2, -16),   S( -1, -15),   S(-12, -42),
            S( -1,  -6),   S( -1,  -5),   S(  2,  -2),   S( 17,  64),   S(  4,  28),   S(  0,   0),   S( -5,  -8),   S(  3,   4),
            S(  7,  20),   S(  2,   6),   S( -8, -15),   S(  6,  22),   S( -2, -14),   S(  6,  28),   S(  0,  -9),   S(  1,   1),
            S( -6,  -8),   S( -7, -24),   S( -3,  29),   S(  8,   9),   S(  0,  -7),   S(  1,   2),   S(  0,  -8),   S( -2,   3),
            S(  2,   6),   S(-14, -15),   S( -3,   7),   S(  4,  -7),   S( -4, -18),   S( -5, -18),   S(  0,   5),   S(  0,   3),
            S(  4,  14),   S( -2,  11),   S(  4,  12),   S(  0,   8),   S( -2, -10),   S( -3, -13),   S(  0,  -2),   S(  1,   3),

            /* knights: bucket 13 */
            S( -6, -16),   S( -1,  -1),   S(  1,  -3),   S( -7, -19),   S(-12, -24),   S( -2, -22),   S( -5, -19),   S(  1,   3),
            S( -4, -13),   S( -1,   1),   S(  4,   5),   S( -8, -32),   S( -5, -32),   S(  3,   3),   S( -3, -14),   S( -2, -16),
            S( -2, -22),   S( -2,   9),   S( 12,  29),   S( -4, -15),   S(  0,   5),   S(-11, -29),   S(  0,  -9),   S( -7, -38),
            S( -4,  -7),   S(  6,  17),   S(  8,  28),   S( -1,  -8),   S(  9,  28),   S( 12,  21),   S(  1,   3),   S( -2,  -2),
            S( 13,  35),   S(  1,  30),   S( 15,  28),   S(  1,  13),   S( -6,  10),   S( -9, -16),   S( -4, -14),   S(  1,   1),
            S(  2,  19),   S( 18,  32),   S(  8,  66),   S(-11,  25),   S(  9,  52),   S( -9, -26),   S(  4,  18),   S( -6, -14),
            S(  1,  -1),   S(  1,  36),   S( 11,  34),   S( -7,   4),   S( 21,  49),   S( -4,   1),   S(  4,  16),   S( -5,  -8),
            S( -1,  -1),   S(  3,  32),   S( -9,  -9),   S(  5,  35),   S(  5,  35),   S(  0,   4),   S( -3,  -9),   S(  0,   1),

            /* knights: bucket 14 */
            S( -4, -21),   S(-10, -51),   S( -1,  -5),   S( -1,   6),   S( -7, -37),   S( -4, -27),   S( -2,  -7),   S(  0,  -3),
            S(  0,  -2),   S( -5, -23),   S(-12, -53),   S(-10, -52),   S( -1,  -8),   S( -2, -20),   S( -2,  -9),   S(  1,  -4),
            S( -8, -21),   S( -5, -21),   S(-15, -50),   S(  4,  12),   S( -2,  -4),   S( -1,   5),   S(  5,   9),   S(  1,  -4),
            S(  0,   3),   S( -1, -11),   S(  1,  -4),   S( -6,   3),   S(  7,  -2),   S( 13,  -4),   S(  1,   7),   S( -9, -29),
            S( -2,  -4),   S(  0,  -9),   S( 24,  72),   S(  8,  17),   S(  9,  18),   S( 14,  33),   S( -3, -10),   S( -7, -39),
            S( -4, -13),   S(  7,  18),   S( -5,  -7),   S(  9,  31),   S(  6,  40),   S( -6,  -4),   S(  4,  20),   S(  2,  15),
            S(  2,  10),   S( -3,  -9),   S(  3,   0),   S(  0,  -2),   S(-11,  18),   S(  2,  26),   S( -3,  12),   S(  0,  16),
            S(  0,  -4),   S(  0,   3),   S(  2,  17),   S(  2,   3),   S(  9,  33),   S(  7,  44),   S(  1,  10),   S( -1,   1),

            /* knights: bucket 15 */
            S( -2, -10),   S( -4, -36),   S( -3, -24),   S( -3, -11),   S(  1,  -1),   S( -1, -13),   S( -2, -11),   S(  2,   8),
            S( -2, -15),   S( -2, -11),   S(  4,  -4),   S( -8, -35),   S( -3,  -9),   S( -1,   3),   S(  1,   0),   S( -2,  -7),
            S( -4,  -9),   S(  4,   9),   S(  1, -12),   S(-10, -45),   S(  1,  -7),   S(  0,   1),   S(  0,  -7),   S( -2,  -2),
            S( -8, -21),   S( -4, -19),   S( -6, -38),   S( -5, -11),   S(  8,  25),   S( 18,  55),   S( -7, -15),   S( -1,   0),
            S( -2,  -4),   S( -6, -15),   S( -3, -30),   S( -5, -29),   S( 10,  17),   S( 13,  13),   S(-15, -46),   S( -5, -34),
            S(  0,  -2),   S(  4,  11),   S( -5, -22),   S( -8, -15),   S(-10, -35),   S( -4,  20),   S( -7, -13),   S( -2,   0),
            S( -5, -16),   S( -2,  -3),   S( -1,  -7),   S(  2,   8),   S( -6, -14),   S(  3,  25),   S(  3, -11),   S(  7,  36),
            S(  0,   0),   S( -1,   2),   S( -5, -12),   S(  2, -11),   S( -1,  -3),   S( -5, -17),   S(  4,  23),   S(  2,   8),

            /* bishops: bucket 0 */
            S( 43,  38),   S( 17,   3),   S( 58,   5),   S(  8,  28),   S(-10,  12),   S( 11, -20),   S(  5, -20),   S( 17, -47),
            S( 53, -28),   S( 86,  21),   S( 29,  23),   S( 18,  14),   S( -1,  34),   S( -2,   6),   S(-28,  13),   S( -5, -40),
            S( 31,  17),   S( 36,  32),   S( 27,  46),   S(  1,  59),   S( 18,  22),   S( -7,  31),   S(  0,   1),   S( 17, -54),
            S( 26,   4),   S( 69,   6),   S( 37,  12),   S( 42,  34),   S(-15,  71),   S( 27,  13),   S(  4,  12),   S( 20, -33),
            S( 16, -24),   S( 26,  19),   S(-10,  57),   S( 60,  38),   S( 46,  28),   S(-24,  46),   S( 10,  24),   S(-72,  43),
            S(-58,  98),   S( -2,  53),   S( 38,  38),   S( 69,  26),   S( 23,  62),   S( -1,   8),   S(-34,  47),   S(-27,  50),
            S(-49,  79),   S(-11,  52),   S( 39,  23),   S(-14,  79),   S(-42,  47),   S( 44,  37),   S( 38,  15),   S(-31,  -9),
            S(-39,   1),   S(  2,  65),   S( -2,  47),   S(-19,  34),   S( 20,  30),   S( 28,  46),   S( -5,  48),   S(-16,  33),

            /* bishops: bucket 1 */
            S( 34,  54),   S(-22,  34),   S( 12,  16),   S(  2,  36),   S(  5,   8),   S( -1,  28),   S( 17,   3),   S(-76,  23),
            S( 13,  -7),   S( 34,  21),   S( 43,  10),   S( 24,  26),   S(  7,  11),   S(  7,   8),   S(-26,  30),   S( -8,  24),
            S( 38,   8),   S(  8,  32),   S( 45,  24),   S(  7,  34),   S( 11,  43),   S(-12,  39),   S( 38,   4),   S(  7, -15),
            S( 68, -21),   S( 21,  25),   S(  5,  29),   S( 24,  44),   S(-19,  66),   S( 15,  29),   S(-26,  47),   S( 29,  -6),
            S( 45,  33),   S(  3,  47),   S( 11,  35),   S(-17,  53),   S( 19,  31),   S(-37,  54),   S( 16,   1),   S(-20,  42),
            S(  0,  33),   S( 33,  33),   S( -2,  61),   S( 55,  20),   S(-17,  53),   S( 25,  32),   S(-26,  50),   S( 21,   0),
            S(  4,  37),   S(-14,  43),   S( 61,  29),   S( 21,  50),   S(  2,  44),   S(-19,  56),   S( 22,  35),   S(-22,  67),
            S( 16,  49),   S(-33,  26),   S(-34,  33),   S(-40,  36),   S( -4,  35),   S(  4,  28),   S(-25,  37),   S(-44,  88),

            /* bishops: bucket 2 */
            S( 24,  16),   S(  0,  31),   S( -7,  29),   S(-22,  37),   S(-33,  50),   S(-16,  29),   S(-41,   9),   S(-50,  46),
            S( -9,  34),   S( 11,  19),   S( 13,  26),   S(-10,  32),   S(  1,  40),   S( -4,  29),   S(  6,   0),   S( -6, -25),
            S( -3,  20),   S(  1,  36),   S(  1,  58),   S( -8,  59),   S( -6,  47),   S(  1,  56),   S(  4,  39),   S( -9,  11),
            S(  8,  31),   S(-36,  68),   S(-17,  56),   S(-15,  67),   S(  0,  61),   S(-10,  56),   S( 15,  37),   S( -4,  25),
            S(-20,  38),   S(-14,  47),   S(-30,  52),   S(-28,  61),   S(-47,  84),   S(  8,  57),   S(-13,  40),   S(-24,  26),
            S(  7,  38),   S(-15,  44),   S(-15,  69),   S(-34,  56),   S( 45,  24),   S(-34,  78),   S( 15,  61),   S(-16,  51),
            S(-15,  48),   S(-25,  56),   S(-47,  77),   S(-20,  51),   S(-60,  70),   S(-32,  52),   S(-69,  60),   S(-23,  48),
            S(-113,  89),  S(-55,  81),   S(-65,  54),   S(-68,  65),   S(-69,  53),   S(-45,  50),   S( -7,  52),   S(-62,  54),

            /* bishops: bucket 3 */
            S(-11,  43),   S( 11,  33),   S( 11,  30),   S( -4,  42),   S(  2,  24),   S( 37,  10),   S( 17,  10),   S( 48, -19),
            S( -4,  41),   S( 10,  29),   S( 14,  20),   S( -1,  51),   S( 12,  43),   S( -4,  56),   S( 41,  43),   S( 14,   0),
            S( 20,  16),   S( -3,  55),   S(  5,  66),   S(  9,  54),   S(  4,  76),   S( 11,  69),   S( 29,  47),   S( 30,  10),
            S( 19,  24),   S( -2,  55),   S( -3,  72),   S(  8,  69),   S( 16,  70),   S( 10,  49),   S( 16,  60),   S(  8,  13),
            S(  1,  32),   S( 11,  55),   S(  8,  48),   S( 15,  77),   S( 11,  63),   S( 18,  61),   S( -1,  46),   S(  4,  62),
            S(  0,  55),   S( 12,  46),   S(  5,  75),   S( 18,  45),   S(  5,  55),   S( 29,  64),   S( 16,  60),   S(-14,  78),
            S( -9,  46),   S(-29,  72),   S( 30,  44),   S(  9,  60),   S(  3,  43),   S( -3,  57),   S(-14,  70),   S( -3,  85),
            S(-38, 111),   S(-23,  62),   S( 41,  44),   S( -1,  66),   S(-51,  89),   S(-51,  99),   S( 20,  63),   S( 75,  43),

            /* bishops: bucket 4 */
            S(-20, -23),   S(-24, -12),   S(-56,   1),   S(-38,  16),   S(-18,   1),   S(-57,   9),   S(  6,   6),   S( 19, -29),
            S(-26, -10),   S( 14,   4),   S(-31,  23),   S(-22,  -6),   S(-26,   3),   S( 68, -17),   S( 16,  -8),   S( 26, -38),
            S(-14,  16),   S(-56,  54),   S( 23,   7),   S(-12,  18),   S( 19,   8),   S( 53,  11),   S(-24, -20),   S(-21,   2),
            S(  7,  58),   S( 17,  23),   S( 37,  29),   S( 37,  23),   S(  7,  32),   S( 24,   4),   S( 31,  35),   S(-12, -20),
            S(  6,   2),   S(-12,  43),   S( -7,  59),   S( 28,  39),   S( -3,  10),   S(-27,  25),   S(-53,   7),   S( -8,  24),
            S( 12,  26),   S( -4,  23),   S( 15,  40),   S( 17,  36),   S( 32,  28),   S(  1,  37),   S(-27,   9),   S(-15,   9),
            S(-26,  26),   S( 60,  19),   S(-16,  16),   S( 19,  42),   S( -8,  -2),   S(-25,   1),   S( -1, -13),   S(-10,  -8),
            S(  1,  28),   S(-12,  -1),   S(  4,   1),   S(-12,  -7),   S(  4,  26),   S(  1,  -1),   S(  0,   1),   S( -8,  19),

            /* bishops: bucket 5 */
            S(-22,   6),   S(-23,  24),   S(-29,  20),   S( -8,  25),   S( -9,  21),   S( -2,  -2),   S(-21,  25),   S(-16,  35),
            S(-33,  27),   S(-45,  55),   S(-57,  65),   S( 30,  17),   S(-16,  33),   S(-12,  26),   S(-43,  17),   S( -9,   4),
            S(-49,  45),   S(-22,  52),   S( 23,  37),   S( 12,  25),   S( 15,  34),   S(-22,  34),   S(-52,  46),   S(-34,  -6),
            S( 30,  36),   S( -2,  47),   S(-44,  57),   S(  5,  45),   S( 34,  39),   S( 34,  35),   S(-37,  33),   S(-32,  48),
            S( 43,  45),   S( 31,  23),   S( 61,  32),   S( 98,  18),   S( 64,   9),   S( 68,  13),   S( 66,  14),   S(-41,  20),
            S(  8,  40),   S( 26,  43),   S( 55,  52),   S( 50,  40),   S(  7,  50),   S( 35,   5),   S(-18,  26),   S( -8,  52),
            S(  5,  39),   S(-40,  50),   S(-10,  37),   S( 18,  36),   S( 13,  42),   S( -7,  65),   S( -1,  44),   S( -2,   4),
            S( -3,  60),   S( 13,  35),   S( -3,  31),   S( -8,  51),   S( 18,  48),   S(  7,  43),   S( 13,  75),   S( -2,  18),

            /* bishops: bucket 6 */
            S(-50,  51),   S(-10,  38),   S(-32,  38),   S(-19,  25),   S(-53,  34),   S(-65,  48),   S(  0,  39),   S( -2,  13),
            S(  7,  25),   S( -7,  24),   S(  9,  31),   S( 10,  32),   S(  9,  33),   S( 19,  19),   S(-104,  59),  S( 44,  18),
            S(  9,  11),   S(-12,  35),   S( 29,  39),   S( 36,  30),   S( 50,  16),   S( 66,  15),   S( -9,  51),   S(-58,  38),
            S( 16,  37),   S(  1,  42),   S( 45,  29),   S( 18,  43),   S( 51,  42),   S( 32,  23),   S( 15,  54),   S(-31,  28),
            S(-33,  55),   S( 38,  35),   S( 57,  17),   S( 48,  38),   S(108,  29),   S( 68,  27),   S( 41,  24),   S( -8,  33),
            S(-34,  45),   S( 13,  22),   S( 39,  41),   S( 34,  47),   S(  9,  47),   S( 62,  48),   S( 33,  45),   S(-13,  61),
            S(-25,  20),   S(  9,  37),   S( 13,  28),   S(-26,  39),   S( 42,  39),   S( 44,  34),   S(  6,  68),   S(-16,  36),
            S( -1,  71),   S(  2,  47),   S(  1,  45),   S( -2,  47),   S(-11,  39),   S( -4,  37),   S(  3,  38),   S(  4,  47),

            /* bishops: bucket 7 */
            S( 16,  -6),   S( -1,  11),   S(-26, -30),   S(-26,   0),   S(-20,  -9),   S(-70,  13),   S(-56, -38),   S(-31,  -4),
            S(-38,   8),   S(-60,  11),   S(-18,  13),   S( 16,   3),   S(-18,  18),   S(-90,  57),   S(-11, -13),   S(-42, -12),
            S(-18,  16),   S( 37,  -6),   S(  7,  18),   S( 46,   8),   S(-23,  29),   S( -6,  18),   S( 21,  25),   S( -6,  14),
            S(-63,  32),   S( 30,  26),   S( 71,  -6),   S( 75,   4),   S( 93,  16),   S(  3,  18),   S( 21,  41),   S( 34,   7),
            S(-31,  25),   S( -9,   6),   S( 37,   4),   S(112,   1),   S( 95,  17),   S( 92,  21),   S(  1,  48),   S( 47,  -8),
            S(-26,   2),   S(-17,  21),   S(  4,   8),   S( 46,   5),   S( 51,  12),   S( 89,  22),   S( 96,  23),   S(  7,  43),
            S( -3,   6),   S(-27,  17),   S(  2,   5),   S(-20,  20),   S(  3,  -5),   S( 33,   5),   S( 36,   8),   S( -6,  32),
            S(  6,   2),   S(-39,  34),   S(-32,  21),   S( -7,  18),   S(  8,  13),   S(  3,  11),   S( 12,  33),   S( 26,  37),

            /* bishops: bucket 8 */
            S( -9, -60),   S( -6, -70),   S(-28, -14),   S(-12, -44),   S(  5,   2),   S( -3, -31),   S( 13,  23),   S(  0,   6),
            S( -1, -15),   S(-40, -67),   S( -5, -35),   S( -7, -16),   S( 17, -20),   S( -6, -17),   S(  2, -24),   S(  3, -22),
            S( -5,  -9),   S( -8,   5),   S(-17, -21),   S( 12, -25),   S(  6,  -4),   S(  6, -32),   S( -8, -56),   S(-48, -43),
            S(  7,  26),   S( 13,   6),   S( 36,   4),   S( 25, -28),   S( 11,   2),   S( 15, -19),   S(  3, -13),   S( -3, -19),
            S(  2,  34),   S( 14,  52),   S( 24, -13),   S( 59,  -5),   S( 30, -27),   S( 18, -12),   S( 11, -16),   S( -1, -34),
            S( -7, -11),   S(  7, -14),   S( 28,  30),   S( 13, -28),   S( 28,  -7),   S(  3, -25),   S( -1, -47),   S(-17, -63),
            S( -1,   9),   S( 26,  -6),   S( -2,  -4),   S(  5, -11),   S(  3, -34),   S( 11,  -1),   S( -6, -48),   S( -8,  -9),
            S( -8, -35),   S( -1, -45),   S( -7, -35),   S( -4,  -7),   S(-11, -43),   S( -5, -51),   S( -8, -42),   S( -8, -44),

            /* bishops: bucket 9 */
            S( -5, -56),   S( 17, -41),   S(-20, -30),   S( -4, -34),   S(-29, -57),   S( -6, -58),   S(-16, -32),   S(  9, -16),
            S(-12, -60),   S(-20, -42),   S(-17, -23),   S( 29, -15),   S( -2, -32),   S( -8, -36),   S(-11, -29),   S(  1,   6),
            S(  2, -10),   S( 12, -27),   S( 19, -22),   S( 35, -20),   S( 20, -40),   S( 41, -48),   S(-24, -30),   S( -2, -30),
            S(-13, -24),   S( 22,  16),   S(-10, -22),   S( 54, -27),   S( 38, -12),   S( 21,  -8),   S(  4, -40),   S(-23, -66),
            S( 10,  -2),   S( 16, -21),   S( 44,   9),   S( 40,  -5),   S( 37, -26),   S( 18, -29),   S( 18, -20),   S( -2, -23),
            S(-11, -58),   S( 40,   9),   S( 10,  19),   S( 28,  13),   S( 42,  -9),   S( 21, -18),   S( 13, -47),   S(-19, -47),
            S( -4, -22),   S( 31,  23),   S( 12, -17),   S( 10, -18),   S( 30, -24),   S( 11, -42),   S( -2, -40),   S( -8, -34),
            S( -4, -24),   S(  8,   6),   S( -2, -49),   S(-15, -53),   S(-14, -58),   S(  5,  -1),   S(  9, -31),   S(-14, -52),

            /* bishops: bucket 10 */
            S(-17, -38),   S(  7, -42),   S(-39, -23),   S( -1, -46),   S(-12, -56),   S(-19, -42),   S(-19, -90),   S( -7, -63),
            S( 11, -15),   S(-20, -42),   S( 28, -39),   S(-19, -55),   S(-17, -30),   S(  9, -52),   S(-17, -60),   S(-10, -26),
            S( -4, -34),   S( 11, -35),   S(-19, -52),   S( 18, -24),   S( 36, -36),   S(  5, -18),   S(-23,  -7),   S(  8,  11),
            S( -6, -55),   S( 19, -26),   S( 31, -29),   S( 63, -24),   S( 55, -27),   S( 23, -18),   S(  1,  -7),   S( 14,   5),
            S( -7, -28),   S( 20, -29),   S( 30, -55),   S( 74, -24),   S( 41,   0),   S( 44,   2),   S(  6,  19),   S( -3, -44),
            S(-15, -42),   S( 10, -65),   S( 27, -43),   S(  7, -47),   S( 45, -17),   S( 41,  -2),   S( 14,  -6),   S(  0, -29),
            S( -7, -58),   S( -6, -50),   S(  3, -46),   S( 11, -26),   S(  0, -42),   S(  3, -15),   S( 20,  55),   S( 12,  11),
            S(  3, -39),   S( -6, -40),   S( 12,  10),   S( -1, -34),   S(-10, -44),   S( -3, -48),   S(  2,  -6),   S( 10,  29),

            /* bishops: bucket 11 */
            S( -5, -18),   S(-33,  -8),   S(-52, -66),   S(-23, -11),   S(-19, -14),   S(-50, -62),   S( -9, -35),   S(-23, -63),
            S( -9, -41),   S( 12, -46),   S(-17, -21),   S(-16, -32),   S(-30, -29),   S(-10, -56),   S(-14, -66),   S(-29, -63),
            S(-13, -56),   S( 28, -60),   S( 24, -35),   S( 26, -47),   S(  0, -35),   S( 15, -39),   S(-19, -12),   S(-12, -13),
            S( -7, -15),   S(-23, -36),   S( 14, -42),   S( 20, -47),   S( 73, -37),   S( 32,  -3),   S( 18, -13),   S( 23,  46),
            S( -4, -35),   S( -4, -67),   S( 24, -41),   S( 60, -46),   S( 63,  -8),   S( 64,  -5),   S( 18,  46),   S(  1, -34),
            S(-19, -79),   S( 16, -62),   S( -7, -68),   S( 15, -30),   S( 25, -17),   S( 43,  -4),   S( -4,  25),   S( -9, -21),
            S(-17, -69),   S( -6, -49),   S(  5, -37),   S( -3, -54),   S( 17,  13),   S( 30, -28),   S( 12, -20),   S(  6, -10),
            S(-11, -44),   S(-17, -48),   S(  2, -28),   S(  3, -35),   S( 15,  -5),   S(-16, -78),   S(  0,  -7),   S(  2,  -9),

            /* bishops: bucket 12 */
            S(  4,  10),   S( -6, -21),   S( -8, -53),   S(-15, -42),   S( -2, -14),   S(  2,  -4),   S( -6, -10),   S(  3,  20),
            S( -4, -30),   S( -5, -30),   S( -1, -26),   S( -6, -31),   S(-12, -50),   S(  4,  -3),   S(  1, -14),   S( -2,  -8),
            S(  2, -10),   S(-17, -39),   S(  0,  -1),   S(  2, -30),   S( -2,   5),   S(  2,  -4),   S(-22, -57),   S(  1,  -7),
            S(  1, -17),   S(  3, -26),   S( -4, -38),   S( -6, -18),   S( -3, -50),   S(  8,  14),   S( -6, -19),   S( -3, -20),
            S( -9, -22),   S( -5,   2),   S(  0, -15),   S(-13, -23),   S(  2, -34),   S( -2, -30),   S(  4, -20),   S( -2, -13),
            S(-13, -35),   S(  9,  24),   S(-14, -24),   S( -5, -31),   S(  6,   3),   S(-12, -54),   S(  6,  -2),   S(  2,   4),
            S( -1,   4),   S( -1,  21),   S(  5,  35),   S(-10, -36),   S(  6,   6),   S( 11,  19),   S( -3, -19),   S( -1,  -7),
            S( -2,  -5),   S( -7, -10),   S(  0, -39),   S(  7,  17),   S(  1, -12),   S( -2, -12),   S( -9, -56),   S( -2,  -3),

            /* bishops: bucket 13 */
            S(-14, -68),   S( -5, -51),   S( -7, -48),   S( -8, -68),   S(-16, -61),   S( -6,  13),   S(  3,   8),   S(  2, -11),
            S( -2, -21),   S( -5, -63),   S(-11, -90),   S(-10, -44),   S( -3, -50),   S(  0, -27),   S( -1, -16),   S(  5,  -7),
            S( -9, -52),   S( -8, -31),   S( 14, -10),   S( -5, -62),   S(  3, -69),   S( 12, -36),   S(  2,   1),   S(  6,  32),
            S(  1,   3),   S(  2, -30),   S( -3, -37),   S(-14, -78),   S( 25, -48),   S(  0, -16),   S(  3,  -9),   S(-10, -47),
            S(  3,   8),   S( -9, -20),   S( -5, -64),   S( 30,  -8),   S(  0, -16),   S(  9, -24),   S( -8, -66),   S(  0, -17),
            S( -1,  -6),   S( -4, -10),   S( -9, -34),   S( 19,  16),   S(  9,  13),   S( 18, -11),   S(  7, -40),   S( -2, -21),
            S( -4, -26),   S( -5,   0),   S(  6,  18),   S( -7,   9),   S(-11, -50),   S( 10,  14),   S( -7, -59),   S(  4,   3),
            S( -8, -23),   S( -5, -20),   S( -9, -31),   S(  4, -17),   S( -5, -26),   S( -6, -36),   S( -1, -15),   S( -5, -48),

            /* bishops: bucket 14 */
            S( -7, -69),   S(-10, -57),   S(-16, -62),   S(-19, -63),   S(-11, -58),   S( -1, -43),   S(-13, -74),   S( -7, -32),
            S(-12, -39),   S( -1, -15),   S( 10,  -4),   S(-27, -97),   S( -3, -53),   S( -7, -81),   S(-13, -45),   S(  8,   2),
            S( -5, -16),   S( -3, -28),   S(-10, -63),   S(  1, -42),   S( -5, -58),   S(-13, -71),   S( -9, -44),   S( -1, -16),
            S(  0, -12),   S( -8, -39),   S(  1, -16),   S(  4, -13),   S( 12, -55),   S( -4, -86),   S(-13, -56),   S( -4, -13),
            S( -7, -33),   S(  1, -39),   S(  7, -36),   S(  7, -47),   S(  9, -28),   S( -1, -24),   S(  5, -15),   S( -1, -25),
            S(  0, -15),   S( -7, -36),   S(  0, -24),   S( -5, -30),   S(  3,  -6),   S(  6,  23),   S(  4, -48),   S( -9, -44),
            S( -5, -49),   S( 10, -30),   S( -9, -39),   S(  6,  -2),   S(-13, -32),   S( 10,   6),   S( -5,  -5),   S( -4, -18),
            S( -9, -44),   S(  1, -14),   S( -1,  -8),   S(-14, -52),   S( -7, -25),   S( -2,  -2),   S(  3,  22),   S(  0, -14),

            /* bishops: bucket 15 */
            S(  6,  40),   S( 10,  33),   S(-24, -79),   S(  5,  -4),   S( -9, -17),   S( -8, -26),   S(-10, -44),   S( -7, -31),
            S(  2,   2),   S(  8,  19),   S(  6,   2),   S(  3, -24),   S( -8, -32),   S( -7, -44),   S(  1,  -3),   S( -1,  -9),
            S(-11, -47),   S( -8, -37),   S(-11, -64),   S( -7, -11),   S( -1, -57),   S( -1,  -4),   S(-12, -35),   S(  6,   8),
            S( -3, -14),   S(-21, -63),   S(  0,   0),   S( -1, -34),   S( -7, -26),   S( -1, -43),   S(  1,   0),   S( -6, -17),
            S( -4, -28),   S( -9, -42),   S( -3, -41),   S(-12, -23),   S( -4, -30),   S( -5,  -9),   S(  6,   0),   S(  3,  -3),
            S( -6, -40),   S( -4, -59),   S(-11, -47),   S(-18, -73),   S(  1, -21),   S(  3,   3),   S(  6,   8),   S( -4,  -6),
            S( -4, -36),   S(  1, -20),   S(  5, -15),   S(  0,  -5),   S(-10, -45),   S( -2, -20),   S(-10, -33),   S(  8,  29),
            S(  3,   4),   S( -1,  -8),   S( -5, -33),   S( -8, -56),   S( -2, -21),   S(-19, -52),   S(-13, -33),   S(  0,  -1),

            /* rooks: bucket 0 */
            S(-34,   7),   S( 10, -26),   S( 13, -38),   S( 18, -16),   S( 21, -10),   S(  8,   0),   S(  1,  12),   S( 16,   0),
            S( 25, -53),   S( 39, -31),   S( 23,  -9),   S( 17,   4),   S( 46,  -8),   S( 12,  -5),   S(-10,  10),   S(-31,  36),
            S(-13,   2),   S( 21,   0),   S( 36,  -3),   S( 22,  10),   S(  8,  10),   S( 23, -17),   S(-28,  17),   S(-35,   0),
            S( 36, -44),   S( 49,  -2),   S( 62,   5),   S( 46,   6),   S( 32,  12),   S(-10,  23),   S(  3,  15),   S(-23,  17),
            S( 49, -36),   S( 54,   8),   S( 99, -11),   S( 48,  -6),   S( 43,  15),   S( 54, -13),   S(  3,  13),   S( -2,  10),
            S( 97, -63),   S(113, -60),   S( 46,   5),   S( 48,  16),   S( 82, -16),   S(-38,  30),   S( 60,   1),   S(-36,  34),
            S( 54, -27),   S( 93, -25),   S( 34,   6),   S( 54,   0),   S(-30,  52),   S( 31,   0),   S(  6,  20),   S( -8,  27),
            S( 33,  10),   S(  3,  42),   S( 51,   6),   S( 55,  10),   S( 34,  21),   S( 24,  11),   S( 27,  26),   S( 33,  16),

            /* rooks: bucket 1 */
            S(-84,  40),   S(-40,  13),   S(-38,  -3),   S(-33,  -6),   S( -4, -26),   S(-17, -12),   S(-28,   0),   S(-30,  16),
            S(-23, -11),   S(-58,  26),   S(-16,   3),   S(-16, -14),   S(-25,  -5),   S(-48,  -1),   S(-36, -11),   S(-53,   6),
            S( -1,  14),   S(-42,  44),   S(-26,  17),   S(-36,  28),   S(-39,  30),   S( -2,   7),   S(-19,   0),   S(-38,  12),
            S(-67,  57),   S(-69,  56),   S(-32,  48),   S(-33,  43),   S(-23,  35),   S(-41,  44),   S(-26,  54),   S(-31,  14),
            S( 80,  -3),   S( 34,  42),   S( -5,  29),   S(-22,  44),   S( 12,  28),   S(  7,  27),   S( 23,  21),   S(-36,  13),
            S( 70,  13),   S( 14,  39),   S( 26,  30),   S(-21,  36),   S( 48,  10),   S(-18,  38),   S( 42,  10),   S(-34,  33),
            S(-11,  43),   S(  8,  43),   S( 39,  28),   S(-35,  54),   S(-24,  44),   S( 17,  44),   S(-12,  26),   S(  5,  21),
            S( 57,  27),   S( 20,  36),   S(-20,  44),   S(-25,  55),   S(  0,  29),   S( 55,   6),   S(-17,  37),   S( 14,  16),

            /* rooks: bucket 2 */
            S(-67,  43),   S(-40,  29),   S(-32,  18),   S(-51,  26),   S(-57,  25),   S(-58,   9),   S(-48,   8),   S(-49,  25),
            S(-71,  37),   S(-73,  45),   S(-40,  29),   S(-44,  25),   S(-32,  18),   S(-47,  13),   S(-73,  28),   S(-56,  21),
            S(-83,  74),   S(-54,  61),   S(-41,  53),   S(-44,  39),   S(-58,  48),   S(-63,  56),   S(-26,  24),   S(-32,  22),
            S(-74,  72),   S(-56,  72),   S(-21,  67),   S(-24,  56),   S(-21,  47),   S(  4,  42),   S(-25,  37),   S(-17,  30),
            S(-35,  65),   S(-52,  79),   S(-38,  62),   S(-27,  61),   S( 43,  39),   S( -3,  61),   S(-20,  58),   S(-42,  53),
            S(-21,  57),   S(-31,  59),   S( -6,  53),   S(  9,  45),   S( 21,  41),   S( 58,  19),   S( 38,  24),   S(-34,  49),
            S(-61,  68),   S(-68,  89),   S(-15,  65),   S(  6,  56),   S( 47,  36),   S( 28,  25),   S(-61,  85),   S(-13,  50),
            S(-16,  71),   S( 15,  50),   S(-39,  55),   S( -5,  59),   S(-34,  58),   S(-26,  54),   S( 29,  56),   S( 20,  38),

            /* rooks: bucket 3 */
            S(  1,  74),   S(  7,  72),   S( 19,  60),   S( 22,  56),   S( 16,  56),   S( -8,  64),   S( -4,  80),   S(-12,  46),
            S(-35,  86),   S( -7,  68),   S( 11,  62),   S( 16,  71),   S( 26,  60),   S( 11,  63),   S( 46,  11),   S( 20, -27),
            S(-38,  88),   S(-15,  94),   S( 12,  77),   S( 19,  67),   S( 19,  73),   S(  9,  80),   S( 28,  52),   S( -3,  57),
            S(-20,  94),   S(-22, 104),   S( 17,  86),   S( 35,  68),   S( 28,  74),   S(  2, 113),   S( 48,  65),   S( -6,  83),
            S( -1,  95),   S( 38,  83),   S( 18,  75),   S( 49,  71),   S( 54,  81),   S( 47,  71),   S( 94,  44),   S( 45,  57),
            S(  6,  93),   S( 21,  88),   S( 33,  76),   S( 47,  72),   S( 38,  66),   S( 48,  53),   S( 99,  32),   S( 87,  32),
            S(  0,  96),   S(  5, 102),   S( 15,  98),   S( 30,  95),   S( 34,  86),   S( 58,  62),   S( 40,  86),   S(116,  20),
            S(-42, 144),   S( 18,  99),   S( 25,  82),   S( 46,  81),   S( 74,  53),   S( 83,  50),   S(137,  39),   S(103,  39),

            /* rooks: bucket 4 */
            S(-78,  12),   S(-12, -15),   S(-78,  23),   S(-26,   6),   S(-38, -14),   S(  6, -61),   S(-14, -46),   S( -9, -32),
            S(-34, -29),   S(-35,   0),   S(-40,   2),   S(-23,   9),   S(-56,  -1),   S(-12, -32),   S(-18, -35),   S(-38, -21),
            S( -8,  -2),   S(-27, -34),   S(-47,  11),   S(-26, -15),   S( 10,  -4),   S( -1, -41),   S( 27, -27),   S(-58, -22),
            S(-12, -29),   S( -6,   2),   S(-35,  10),   S( 19,   6),   S( 25, -12),   S(-28,  -3),   S(-30,  -4),   S(-29,  28),
            S(-16,  -3),   S(-44,  49),   S( -6,  12),   S( 51,  10),   S(-19,  23),   S( 23,  -1),   S( 43,   9),   S( 29,  -4),
            S( -9,  16),   S( 21,  -4),   S( 26,   3),   S( 52,  28),   S( 41, -10),   S( 13,  34),   S( -7,  23),   S( 13,  24),
            S( -2,   5),   S( 11,  35),   S( 33,  20),   S( 34,   7),   S( 62, -22),   S( 23, -10),   S( 60,  -6),   S( 47,  12),
            S( -3, -47),   S( 28,  35),   S( 31,   6),   S( 22,   2),   S( 25, -32),   S( 22, -11),   S( 10,  20),   S(  9,   2),

            /* rooks: bucket 5 */
            S(-47,  36),   S( -2,  26),   S(-57,  45),   S(-31,  14),   S(-29,  12),   S(-32,  34),   S( 38,   6),   S(-25,  33),
            S(  1,  13),   S(-45,  33),   S(-77,  56),   S(-75,  55),   S(-27,  25),   S( 23,   0),   S( 38,  -2),   S( -3,   2),
            S(-12,  34),   S(-37,  52),   S(-83,  69),   S(-87,  44),   S(-51,  38),   S(-29,  41),   S( -9,  32),   S(-23,  15),
            S(-48,  80),   S(  2,  45),   S(-36,  77),   S(-16,  47),   S( -7,  51),   S( -5,  65),   S( 11,  51),   S(-26,  33),
            S(  2,  62),   S( 12,  60),   S( 39,  39),   S(  1,  76),   S( 30,  50),   S( 24,  69),   S( 51,  54),   S( 16,  35),
            S( 44,  67),   S( 35,  56),   S( 75,  55),   S( 73,  51),   S( 67,  42),   S(106,  31),   S( 51,  57),   S( 46,  38),
            S( 13,  59),   S( 28,  72),   S( 50,  54),   S( 22,  65),   S( 50,  44),   S( 80,  47),   S( 68,  48),   S( 58,  42),
            S( 66,  31),   S( 59,  28),   S( 60,  48),   S( 22,  54),   S( 25,  51),   S( 71,  35),   S( 41,  43),   S( 23,  46),

            /* rooks: bucket 6 */
            S(-66,  38),   S( -8,  18),   S( -2,  12),   S( -5,   9),   S(-35,  26),   S(-72,  52),   S(-42,  65),   S(-31,  45),
            S(-18,  21),   S(-15,  33),   S( -7,  20),   S(-36,  37),   S(-57,  50),   S(-99,  73),   S(-73,  51),   S( 12,  16),
            S(-42,  47),   S(-30,  52),   S( -3,  37),   S(-39,  44),   S(-16,  35),   S(-58,  80),   S(-47,  69),   S( 29,  27),
            S(-49,  66),   S(  0,  61),   S(  3,  58),   S(-18,  59),   S(-10,  51),   S( 22,  44),   S(-91,  81),   S(  0,  54),
            S(-34,  79),   S( 16,  69),   S( 65,  37),   S( 47,  42),   S( -6,  84),   S( 12,  69),   S( -9,  61),   S( -4,  65),
            S( 20,  66),   S( 57,  55),   S( 77,  38),   S( 56,  37),   S( 27,  60),   S( 40,  58),   S( 43,  53),   S( 92,  40),
            S( 67,  57),   S( 75,  41),   S( 90,  35),   S( 79,  33),   S(117,  28),   S( 58,  54),   S( 86,  40),   S( 49,  49),
            S( 63,  61),   S( 55,  49),   S( 71,  47),   S( 38,  56),   S( 61,  41),   S( 84,  47),   S( 75,  35),   S( 59,  39),

            /* rooks: bucket 7 */
            S(-71,  -2),   S(-56,   2),   S(-29, -11),   S(-41,   7),   S( 11, -29),   S(-33,  12),   S(-36,   9),   S(  9, -19),
            S(-103,  26),  S(-51,  21),   S(-56,  10),   S( -7,  -2),   S(-18,  20),   S(-11,  20),   S(-26,   7),   S(-47,  -8),
            S(-97,  56),   S(-30,   9),   S(-19,  18),   S(-13,  -1),   S( 11,   0),   S(-17,   7),   S(-27,  -1),   S( -8,  23),
            S(-58,  43),   S(  3,  31),   S( 12,  17),   S( 59,   0),   S( 35,   8),   S( 40,   5),   S( 16,  -1),   S( -8,   0),
            S(-14,  34),   S( 13,  26),   S( 45,  -3),   S( 49,   3),   S( 72,   7),   S(115,  -7),   S( 38,  34),   S( 63, -13),
            S(-23,  42),   S( 21,  16),   S( 88, -11),   S( 91,  -5),   S(119, -12),   S( 72,  -1),   S( 69,  21),   S( 39,   3),
            S( 36,  28),   S( 53,   9),   S( 67,  13),   S( 91,  -4),   S(102,   0),   S(104,  -1),   S( 31,  27),   S( 12,   8),
            S( -2,  62),   S(  6,  28),   S( 36,  14),   S(115, -23),   S( 45,  21),   S( 31,  12),   S( 72,  -7),   S( 50,   2),

            /* rooks: bucket 8 */
            S(-41, -36),   S(-26,  18),   S(  2,  15),   S( -7, -17),   S( -9, -25),   S(-37, -50),   S(-31, -60),   S(-27,  -9),
            S(-15, -16),   S(-23, -41),   S(-14, -22),   S(-15, -27),   S(-30, -30),   S(-17, -44),   S(  1, -25),   S(-28, -77),
            S(  9,   1),   S( -3,  -5),   S( -4,  18),   S(-12,  10),   S(-31, -63),   S(-14, -32),   S(-24,   5),   S( -4, -11),
            S(-10, -24),   S( -4,   0),   S(-11,  13),   S( 11,  -2),   S( 12,  21),   S(-21,  -5),   S(  5,   8),   S(-33, -50),
            S(-17, -31),   S( -6,   2),   S( -4,  44),   S(  2,  12),   S( -7,  15),   S(  6,  22),   S(-12,  -3),   S(-20, -53),
            S( 10,  21),   S( -9,  20),   S( 20,  38),   S( 22,  17),   S(-14,   4),   S(  0, -16),   S( -5, -17),   S(  0,  11),
            S(  0,  -2),   S(-12,   1),   S( 28,  42),   S( 11,   9),   S( 32,   4),   S(  1, -37),   S(  7, -26),   S(  4,  -5),
            S(-15, -131),  S(  3,  -5),   S( 31,  28),   S(  1, -14),   S(-12, -30),   S( -6, -31),   S(  8,  12),   S(  6,  14),

            /* rooks: bucket 9 */
            S(-67, -41),   S(-13, -44),   S(-35, -45),   S(-51, -24),   S(-31,  -1),   S(-30, -40),   S( -1, -41),   S(-67, -26),
            S(  1, -33),   S( -1, -46),   S(-31, -27),   S(-18, -35),   S(-41, -61),   S( -4, -11),   S( 11, -22),   S(-24, -42),
            S(-11, -35),   S(  9, -25),   S(-14, -24),   S(-33, -13),   S(-39, -21),   S(  5, -32),   S( 15,  -5),   S(-18, -22),
            S( -6, -23),   S(  5,  -4),   S( -7,  -1),   S( -6,  -6),   S(-10,  -5),   S(-15, -21),   S( 12,  12),   S( -5,  -8),
            S(-14,   8),   S(-15,   7),   S(-10,  -5),   S( 17,  38),   S( -1,  10),   S(  7,  -5),   S(  4,  -2),   S( -1, -35),
            S(  5,   6),   S(-11, -24),   S(  3,  -4),   S(-28,   8),   S( 19,  -9),   S( 22,  -1),   S( 12,   9),   S( -1,  -7),
            S( 38,  12),   S( 39,  -6),   S( 31,  12),   S( 42,   8),   S( 18, -18),   S( 33,  -1),   S( 50, -18),   S( 43,  -3),
            S( 61, -75),   S( 35, -38),   S( 23,  -4),   S( 27,  52),   S(  1,   0),   S(  5, -25),   S( 17, -16),   S( 21,  16),

            /* rooks: bucket 10 */
            S(-71, -103),  S(-31, -59),   S(-25, -74),   S(-46, -35),   S(-36, -47),   S(-39, -52),   S( 32, -59),   S(-49, -41),
            S(-12, -37),   S( -5, -39),   S(-18, -55),   S(-33,   1),   S(  1, -38),   S(-14, -18),   S( 19, -17),   S(  2, -37),
            S(-25, -34),   S(-27, -46),   S(-19, -22),   S( -7, -17),   S(-28, -12),   S(-11, -20),   S( 24,  -4),   S( -9,  -2),
            S(-18, -23),   S(  2,  -1),   S(-21, -24),   S( -7, -34),   S(  7, -14),   S( -8, -19),   S( -3,  -2),   S( -6,  -5),
            S(-22, -24),   S( 10, -30),   S( 17, -17),   S(  4, -25),   S(-12,  -3),   S( 17,   8),   S( 19,  15),   S(-13,  -4),
            S( 33,  -7),   S( 23,  12),   S(  7, -19),   S( 10,  -8),   S(-15, -27),   S(  7,  -9),   S( 25, -17),   S(  7,   3),
            S( 78,  -7),   S( 66, -30),   S( 40, -16),   S( 47, -37),   S( 33,  -6),   S( 22, -21),   S( 27, -30),   S( 18, -37),
            S( 49, -10),   S(  5, -20),   S( 20, -15),   S( 14,  -8),   S( 32, -18),   S( 23,  10),   S( 13, -32),   S( -1, -41),

            /* rooks: bucket 11 */
            S(-75, -38),   S(-46, -20),   S(-47,  -4),   S(-52, -42),   S(-24,   5),   S(  4,   3),   S(-49, -65),   S(-81, -27),
            S(-46, -29),   S( -3, -38),   S(-32, -22),   S(-37, -31),   S(-29, -27),   S(-34, -10),   S(  1, -39),   S(-44, -13),
            S(-35, -19),   S( -3, -19),   S(  3, -15),   S(  3, -18),   S(  0, -13),   S(-21,   0),   S(-22, -11),   S(-21, -57),
            S(-21,  25),   S(  8, -14),   S(  1,  -1),   S(  8,  -6),   S(-11, -13),   S(-12,  26),   S(  6,   4),   S(  9, -25),
            S( -4,   9),   S(  2, -25),   S( 36,  12),   S( 35, -45),   S( 18, -13),   S( 15, -16),   S( 16,  -1),   S(-20,  -9),
            S( -4,  24),   S( 21,  12),   S( 14,  -3),   S( 45,   8),   S( 35,  20),   S( 22, -24),   S(-10,  24),   S( 21,  27),
            S( 23,  44),   S( 42,  11),   S( 49, -21),   S( 74,  13),   S( 27,  18),   S( 41,  35),   S( 10,  13),   S( 26,   9),
            S( 25,  33),   S(  7,   1),   S( 13, -30),   S( 12, -12),   S( -5,  -4),   S(  3,   0),   S( 19,  30),   S( 22,  -3),

            /* rooks: bucket 12 */
            S( 13, -18),   S(-10, -61),   S(-13, -19),   S(-26, -43),   S(-13, -32),   S( -3, -34),   S(-29, -83),   S(-30, -48),
            S(  9,   8),   S(  9,  22),   S( -3,  31),   S(-14, -37),   S(-17, -20),   S( -3, -13),   S( -8, -27),   S( -4, -30),
            S( 13,  22),   S(-11, -48),   S( -6, -22),   S(-28, -39),   S( -3, -16),   S(  5,  -3),   S( -3, -23),   S(  1,  -8),
            S( -7, -24),   S(-12, -11),   S( 10,  17),   S(  6, -13),   S( -3, -16),   S(-20, -61),   S( -9, -44),   S(  1,  -3),
            S(-14, -21),   S( -5,  -8),   S( 17,   8),   S( -6, -25),   S( -6, -29),   S( 10,   4),   S(-10, -34),   S(  4,   9),
            S(-11, -28),   S(-18, -46),   S(  9, -10),   S(  4, -22),   S( -3, -18),   S( -3, -24),   S( -4, -32),   S(  4,   0),
            S(-15, -40),   S( -9, -20),   S( -6, -44),   S( 22,  17),   S(  8, -12),   S( -9, -49),   S( -7, -32),   S(  5,  -1),
            S(-12, -77),   S(  5, -21),   S(  6,   4),   S(  0, -14),   S(  5,  -8),   S(  0, -12),   S( -7, -41),   S(  9,  20),

            /* rooks: bucket 13 */
            S(-25, -74),   S(-13, -44),   S(  5,  15),   S(-16,   9),   S( -2, -10),   S(-17, -42),   S(-11, -50),   S(-21, -36),
            S(  1,  -9),   S( -1,   8),   S(-11,  27),   S(-17,  -8),   S(-11, -16),   S(-12, -18),   S( -2,   3),   S( -8, -59),
            S(-10, -50),   S( -7, -23),   S( -4,   2),   S(  3, -15),   S(  7,  49),   S(  8,  -1),   S( -3, -17),   S( -8, -45),
            S( -9, -29),   S(  9,  -1),   S(-15, -23),   S(-11, -17),   S( 10,  10),   S(-25, -38),   S( -2, -37),   S( -8, -35),
            S(  7,  -4),   S(-10, -31),   S( 10,  14),   S( -2, -20),   S(-19, -50),   S( -5, -10),   S(-19, -63),   S(  5,   1),
            S(-15, -28),   S(  9,  14),   S(-16, -35),   S(  7, -20),   S(  5,  -8),   S(  8,  17),   S(  5, -13),   S( -9, -29),
            S( -1,  22),   S(  7,  31),   S( 24,  25),   S(  0,   7),   S( 11, -19),   S( 15,  29),   S(  2,  -3),   S( -5,   0),
            S(-20, -92),   S(-13, -55),   S(  1,   8),   S( -3, -17),   S(  5,   9),   S( -7, -51),   S(-13, -69),   S( -5,   2),

            /* rooks: bucket 14 */
            S( -9, -32),   S(-37, -91),   S( -2, -10),   S( -3, -14),   S(-11,  -6),   S( -2,  10),   S( 17,  13),   S(  0, -11),
            S(-23, -47),   S(-26, -72),   S( -2,   4),   S(-25, -62),   S( -7, -17),   S(-17, -49),   S( -1,  -3),   S(  5,  -9),
            S(  0, -14),   S(-11, -29),   S(  4,  -4),   S( -1,  -4),   S(-11, -21),   S(-15, -22),   S( 15,  48),   S(  6,  -5),
            S( -3,  14),   S(-16, -46),   S( -5, -40),   S(-17, -29),   S(  0, -17),   S(  3,  12),   S( -4, -38),   S(-13,   1),
            S( 10, -10),   S(  2, -39),   S(-10, -68),   S(-14, -81),   S( -4, -51),   S( -5, -50),   S(  9, -13),   S( -8, -28),
            S( -3, -36),   S( -5, -26),   S(  1, -37),   S(  4, -81),   S(  5, -69),   S(  6, -41),   S(  1, -26),   S(  7,  17),
            S( 15,  14),   S( -9, -50),   S( -6, -60),   S( -1, -54),   S( -1, -77),   S(  7, -16),   S( 15,  -8),   S( 15,  16),
            S( -7, -43),   S( -7, -30),   S(-12, -80),   S(  1, -10),   S(  0,  -6),   S( -5,   9),   S( -4, -19),   S( -2,   3),

            /* rooks: bucket 15 */
            S(-15, -39),   S(-13, -68),   S( -5, -45),   S( -8, -12),   S( -6, -24),   S(  5,  23),   S(-14, -53),   S( -3,  -6),
            S(-18, -30),   S(-18, -58),   S( -2,  -5),   S(-22, -42),   S(-11, -37),   S(  2,  17),   S(-20, -46),   S(  0, -21),
            S(-19, -46),   S( -6, -30),   S( -6, -24),   S(-14, -39),   S( 15,  15),   S( -1,  -2),   S( -9, -10),   S( -9, -23),
            S( -9, -52),   S( -3, -28),   S( -8, -16),   S( -6, -30),   S(-10, -36),   S( -8, -48),   S(  8,   3),   S( -9,   7),
            S( -4, -27),   S( -5, -39),   S( 11,  13),   S(  4,  -8),   S(  1, -34),   S(  8, -20),   S( -9, -33),   S( 12,  41),
            S( -3,  -4),   S( -3,  -9),   S( -2, -52),   S( -7, -80),   S(-10, -63),   S(  9, -19),   S(  4, -30),   S(-13, -27),
            S( -5,  -5),   S( 18,   6),   S( 12, -18),   S(-11, -68),   S(  2, -55),   S( 22,  28),   S( 21,  30),   S(  1,   1),
            S(  0,   2),   S( -5, -15),   S(  4, -21),   S( -8, -51),   S( -1, -40),   S( -2, -37),   S(-11, -50),   S( -9, -43),

            /* queens: bucket 0 */
            S(-22, -16),   S(-44, -44),   S( 26, -54),   S( 59, -75),   S( 40, -56),   S( 37, -29),   S( 41,  -6),   S( 18,  -6),
            S(-22,  -2),   S( 30, -55),   S( 41, -27),   S( 36, -17),   S( 31,   8),   S( 14,  40),   S( 20,  43),   S( 50, -11),
            S( 23,  -9),   S( 40, -18),   S( 23,  13),   S( 21,  25),   S( 27,  -6),   S(  3,  18),   S( -8,  20),   S( 22,  59),
            S(  4,  18),   S( 31,  13),   S( 19,  23),   S( 21,  49),   S( 25,  29),   S(  6,  38),   S(-11,  65),   S( 22,   6),
            S( 20,  25),   S( 21,  57),   S( 21,  20),   S( 15,  62),   S(  4,  28),   S(-14, -27),   S( 13,  27),   S( 36,   9),
            S( 24,  51),   S( 21,  55),   S( 34,  44),   S( 19,  51),   S( 53,  -4),   S( -1,  51),   S( 27,  11),   S( -9, -19),
            S( 46,  14),   S( 75,  16),   S( 39,  30),   S( 54,  24),   S( -1,  10),   S(-13, -17),   S( 37,  23),   S( 26,  -8),
            S( 29,  37),   S( 18,  49),   S( 55,  11),   S( 11,  40),   S( 61,  55),   S(-20, -12),   S( 58,  34),   S( 43,  12),

            /* queens: bucket 1 */
            S( -2, -32),   S(-88, -14),   S(-41, -26),   S(  6, -111),  S(  8, -57),   S(-38, -25),   S( 36, -51),   S(  0,  22),
            S(-14, -54),   S( -6, -55),   S( 10, -34),   S(  6,  -1),   S(  7,  13),   S( 10, -23),   S( 30, -44),   S( 19,  17),
            S(-28,  36),   S(  4,   1),   S(  1,  38),   S(  2,  20),   S(  1,  41),   S(-12,  11),   S( 12,  -3),   S( 32,   3),
            S( 15,   6),   S(-12,  24),   S( -8,  35),   S( 35,  31),   S(-15,  58),   S( 23,  -4),   S(-10,  25),   S( 33,   1),
            S( 28, -42),   S(  5,  40),   S( -9,  89),   S(-31,  81),   S(-13,  64),   S( 10,  26),   S(-11,  31),   S(-22,  43),
            S(-16,  51),   S( 21,  48),   S( 34,  82),   S( -8,  50),   S( -5,  69),   S( 17,  50),   S( 38,  24),   S( 18,  18),
            S(-13,  33),   S(-31,  92),   S( -7,  28),   S(-26, 110),   S(-13,  51),   S(-10,  55),   S( -2,  45),   S( -3,  21),
            S(  4,  -4),   S( 27,  33),   S( 46,  44),   S(-12,  17),   S( -4,  28),   S(  5,  19),   S(  5,  49),   S( 13,  36),

            /* queens: bucket 2 */
            S( 10,   4),   S( 28, -67),   S( 24, -22),   S(  4, -22),   S(-20,  -9),   S(-43,   9),   S(-22, -10),   S( 32,  10),
            S( 20,  -4),   S( 28,   4),   S( 25, -24),   S( 30, -31),   S( 20, -31),   S( 17, -42),   S( 18,  -1),   S( 33, -35),
            S( 22,   2),   S( 25,  -1),   S( 11,  33),   S( 15,  40),   S(  7,  49),   S( 15,  48),   S( 14,  17),   S( 41,   9),
            S( 13,  12),   S(  6,  47),   S( -1,  49),   S( 14,  52),   S( -9,  84),   S(  8,  86),   S( 18,  20),   S( 18,  45),
            S(  9,  15),   S( -6,  40),   S(-18,  77),   S(-38, 129),   S(-46, 132),   S(-40, 136),   S(-43, 125),   S(  4,  90),
            S(  8,  23),   S(-13,  55),   S(-19,  98),   S(-11,  64),   S(-20, 112),   S(-15, 104),   S( -2,  96),   S( -6,  78),
            S(-25,  63),   S(-38, 116),   S(-31,  94),   S( 18,  74),   S(-17,  97),   S( 42,  47),   S(-49,  91),   S(-37,  81),
            S(-73, 111),   S(-38,  70),   S( 39,  57),   S( 78,  21),   S( 23,  59),   S( 30,  68),   S( 14,  40),   S( 20,  26),

            /* queens: bucket 3 */
            S( 64,  94),   S( 64,  89),   S( 63,  89),   S( 45,  95),   S( 68,  31),   S( 52,  16),   S( 24,  -7),   S( 37,  63),
            S( 62, 112),   S( 60,  92),   S( 48, 101),   S( 52,  88),   S( 48,  83),   S( 63,  54),   S( 65,  19),   S( 34,  50),
            S( 49, 111),   S( 53,  98),   S( 54,  84),   S( 56,  77),   S( 51,  97),   S( 46,  94),   S( 49, 101),   S( 49,  64),
            S( 42, 126),   S( 48, 110),   S( 41, 101),   S( 47, 106),   S( 45,  88),   S( 34, 148),   S( 62,  82),   S( 37, 149),
            S( 44, 119),   S( 43, 125),   S( 28, 127),   S( 38, 107),   S( 27, 132),   S( 18, 136),   S( 25, 154),   S( 37, 160),
            S( 55, 101),   S( 52, 114),   S( 52, 116),   S( 16, 135),   S( 27, 143),   S( 72, 117),   S( 47, 171),   S( 13, 194),
            S( 48, 122),   S( 59, 115),   S( 63,  98),   S( 76,  83),   S( 26, 140),   S( 55, 127),   S( 64, 144),   S(137,  75),
            S( 68, 108),   S(100, 100),   S( 55, 128),   S( 63, 103),   S( 26, 121),   S( 93, 100),   S(140,  46),   S(114,  52),

            /* queens: bucket 4 */
            S( -7, -18),   S( -8, -20),   S(-41, -16),   S( 31,   5),   S( 34, -39),   S( 25, -10),   S(-36, -22),   S(-25,  -4),
            S(-40, -35),   S(-27,  -5),   S( 39, -11),   S(-59,  20),   S(-18,  -1),   S(-14, -27),   S(-12, -22),   S( -7,   2),
            S( 11,  12),   S( 49,  25),   S(  4,  15),   S(-12,  16),   S( 40,   2),   S(  2,  -6),   S( 17,   0),   S(-17, -23),
            S(-34, -19),   S( -3,   3),   S(  3,  35),   S(  3,  32),   S( 51,  36),   S( 24,  30),   S( 12, -16),   S(-11, -10),
            S(-14,  11),   S( -5,  -2),   S( 34,  28),   S( 30,  49),   S( 32,  47),   S(  8, -20),   S(-33, -50),   S(  5, -39),
            S( 15,  33),   S( 41,  14),   S( 34,  49),   S( 42,  50),   S( 37,  37),   S(-17, -16),   S(-15, -25),   S(-27,  -7),
            S(-22, -26),   S( 27,  50),   S( -7,  10),   S( 21,  38),   S(  9,  22),   S( -9,   2),   S(-11, -17),   S(-22, -19),
            S(-17, -32),   S(  1, -21),   S( 17,  29),   S( 15,  26),   S(-33, -23),   S(-21,  -7),   S(-39, -61),   S(-14, -19),

            /* queens: bucket 5 */
            S(-46, -30),   S(-40, -53),   S( -2, -32),   S(-19, -72),   S(-51,  13),   S(  9,  -8),   S(-18, -25),   S(-28, -39),
            S(-45, -26),   S(-53, -24),   S(-36, -17),   S(-49,   8),   S(  2, -55),   S( -4,  -7),   S(-43, -10),   S(-51, -22),
            S(-18,  15),   S(-88, -35),   S(-64,  -8),   S( -7,  43),   S( 47,  48),   S( 11,  22),   S(-17, -35),   S( 20,   3),
            S(-43,   0),   S(-75, -19),   S( 16,  32),   S( 39,  64),   S( 14,  33),   S( -3,  23),   S( -1, -20),   S(-16,  15),
            S(-51, -12),   S(-13,  13),   S(  5,  31),   S( -8,  50),   S( 38,  42),   S(-20,   2),   S(  4,  16),   S(-35, -17),
            S(-15,  20),   S(  3,  57),   S(-29,  32),   S( 10,  23),   S( 38,  48),   S( 12,  12),   S( 14,  10),   S(  0, -21),
            S( -8, -20),   S(-12,  21),   S(  4,  46),   S( 19,  61),   S( 36,  70),   S( 18,  59),   S( 23,  13),   S(-13, -25),
            S(  3,  -5),   S( 12,  17),   S( -4,   6),   S( 25,  77),   S( 27,  39),   S(  6,  18),   S( 16,   6),   S(-26, -32),

            /* queens: bucket 6 */
            S(-27, -16),   S(  6, -12),   S(-55, -15),   S(-74, -56),   S(-78, -35),   S(-53, -67),   S(-47, -43),   S(-18,   5),
            S(-85, -17),   S(-51,   7),   S(-21,   2),   S(-30,   7),   S(-67,  33),   S(-86, -16),   S(-110, -26),  S(-27,  20),
            S(-32,  -5),   S(-37,  25),   S(-21,  18),   S(-59,  71),   S( -9,  41),   S(  7,  18),   S(-30,  -2),   S( 21,  17),
            S(-18,  32),   S(-12,  27),   S(-14,  48),   S(-59,  72),   S(  4,  60),   S( 28,  62),   S(  0,  39),   S( 21,   2),
            S(-30,  56),   S( -3,  32),   S(-21,  31),   S( 11,  53),   S( 45,  83),   S( 58,  61),   S( 43,  27),   S(  8,  18),
            S(-27,  53),   S( -6,  23),   S( 61,  31),   S( 41,  89),   S( 29,  65),   S( 61,  97),   S(-32,  36),   S( 19,  19),
            S(  6,  26),   S( -6,  -6),   S( 17,  59),   S( -6,  19),   S( 50,  87),   S( 28,  66),   S(  1,  34),   S(-48,   0),
            S( -1,  33),   S( 21,  44),   S( 32,  31),   S(  6,  21),   S( 45,  65),   S( 22,  37),   S(-12,  18),   S(-12,  -9),

            /* queens: bucket 7 */
            S( -6, -27),   S(-30,   9),   S(-50,  32),   S(-14,  -2),   S(-26, -11),   S(-21, -33),   S(-10,  10),   S(-24, -20),
            S( -5, -20),   S(-50,  13),   S(-37,  28),   S(-24,  27),   S(-19,  17),   S(-40,  26),   S(-47,  28),   S(-27, -54),
            S(-77,   9),   S(-55,  65),   S(-15,  33),   S(-14,  50),   S( 13,  45),   S(-15,  52),   S( 10,   1),   S(  3,   9),
            S(-57, -20),   S( 17,  14),   S(-33,  43),   S(-18,  46),   S( 33,  15),   S( 48,  23),   S( 12,  54),   S(-16,  32),
            S(-29,  39),   S(-52,  32),   S( -1,  33),   S( 50,  13),   S(  2,  39),   S( 91,  11),   S(  1,  40),   S( 50, -15),
            S(-17,  30),   S(-17,  31),   S(-26,  23),   S( 10,   4),   S( 40,  38),   S( 66,  45),   S( 66,   4),   S( 28,  27),
            S( 19, -10),   S( 16,  37),   S(  0,  33),   S( 28,  19),   S( 46,  14),   S( 88,  22),   S( 47, -17),   S( 63,  38),
            S( 33,  21),   S( 11,  12),   S( 35,  21),   S( 30,  27),   S( 39,  33),   S( 32,  29),   S( 11,   6),   S( 49,  43),

            /* queens: bucket 8 */
            S( -7, -21),   S( -8, -29),   S(-28, -38),   S(-12, -13),   S(-13, -16),   S(  2, -15),   S(-11, -15),   S(  2,  -3),
            S(  4,  10),   S( -8,  -4),   S( -7,   1),   S(-15,  -2),   S( -2, -11),   S(-21, -30),   S(-21, -51),   S( -3,  -2),
            S( -2,  -1),   S(-15,  -5),   S(-11, -23),   S(  0,  -8),   S(-17, -15),   S(-15, -25),   S(-13, -41),   S(-13, -20),
            S(-16, -10),   S( 18,  23),   S( 16,  30),   S( -3,  27),   S( -7,  11),   S(-19, -22),   S( -2,   3),   S( -7, -33),
            S( 12,  28),   S( -4,  21),   S( 22,  55),   S( 10,  30),   S( 19,  28),   S(  4,  15),   S(-15, -26),   S(-22, -34),
            S(  7,  20),   S( 16,  29),   S(-44, -14),   S( 13,  35),   S(-11, -17),   S(  0,   2),   S(-13, -20),   S(  2,   1),
            S( -7, -14),   S(-24, -38),   S( 19,  28),   S(  8,  10),   S( -1,  13),   S(  0,  -5),   S(-14, -24),   S(-11, -25),
            S(-22, -32),   S( 12,  15),   S(-30, -54),   S(  2,  -5),   S( -7, -17),   S( -9, -24),   S( -6, -23),   S( -4,  -5),

            /* queens: bucket 9 */
            S(  4,   4),   S(-21, -51),   S(  2,  -3),   S(-28, -42),   S(-28, -44),   S(-33, -49),   S(-23, -41),   S(-16, -27),
            S( -5, -11),   S(  2,   4),   S(-26, -29),   S(-13, -21),   S(-36, -42),   S(-17, -22),   S(-15, -44),   S(-13, -30),
            S( 13,  22),   S(-12,   0),   S(-25, -10),   S(-18, -27),   S(-21, -15),   S(-19, -25),   S(-11, -19),   S(-17, -28),
            S( -4,  -9),   S(-30, -26),   S( 15,  26),   S(  6,  28),   S(  7,  14),   S(  1, -12),   S(-21, -30),   S( -4, -24),
            S( -3, -11),   S( -1,  18),   S( 18,  49),   S( 19,  56),   S( 12,  34),   S(  0,   8),   S(  3,   0),   S( -4,  -7),
            S(-43, -60),   S(-19,  -6),   S( 10,  38),   S(  0,  25),   S(  0,   8),   S( -1,  -8),   S(-15, -13),   S(-15, -24),
            S( -8, -14),   S( -8, -23),   S(-13,  31),   S( -4,  10),   S( 12,  10),   S( -4, -23),   S( -5,  -4),   S(-14, -26),
            S(-16, -30),   S(-23, -48),   S(  3,   0),   S( 14,  16),   S( -2,  -9),   S( -7, -11),   S(  3,  -7),   S(-20, -38),

            /* queens: bucket 10 */
            S( -3,  -6),   S(  1,  -2),   S(-13, -24),   S(-32, -49),   S( -3,  -2),   S( -1,  11),   S( -9, -21),   S( -6,  -8),
            S( -9, -18),   S(-15, -33),   S(-19, -34),   S(-11,  -6),   S(-36, -52),   S(-21, -31),   S(  6,   2),   S(-27, -38),
            S(-14, -28),   S( -6, -18),   S(-16, -24),   S(-23, -39),   S(-47, -29),   S(-15,   7),   S(  6,  17),   S( -6,  -9),
            S( -5,  -8),   S( -2,  -1),   S( -7,  -5),   S(-18, -42),   S(  6,  26),   S(-25,   3),   S(-13, -27),   S(-24, -32),
            S( -8,  -7),   S( -6, -16),   S(-31, -24),   S(  2,  19),   S(-10, -10),   S(  0,   6),   S(  7,  -3),   S( -3,  -1),
            S( -7, -11),   S(-26, -34),   S( -6,  -1),   S(-22,  -1),   S(-28,  -1),   S(  2,  27),   S(  5,  -1),   S( -6,  -7),
            S(-13, -21),   S(-14, -23),   S(  5,  11),   S(-24, -28),   S( -1,   0),   S(  4,   9),   S(-15, -15),   S( -8, -16),
            S(  9,  16),   S(-27, -36),   S( -2,   6),   S( 10,  15),   S( 10,  11),   S(-10, -18),   S(  1,  -1),   S(  7,  -1),

            /* queens: bucket 11 */
            S(-14, -35),   S(-20, -40),   S(-14, -26),   S(  2, -19),   S(-27, -30),   S(  8,  11),   S(-14, -13),   S(-23, -41),
            S( -4, -25),   S(-14, -19),   S(-56, -63),   S(-26, -39),   S(-11, -10),   S( -2,   1),   S(  5,  14),   S( -9,  -5),
            S(-41, -43),   S(-18, -39),   S(-18, -21),   S(-16, -26),   S(-20,  -5),   S( -9,  -6),   S(-10,   9),   S(-27, -28),
            S(-27, -57),   S(-29, -36),   S(-12, -27),   S( 15,  50),   S( -7,  -8),   S( -6,  -1),   S(  8,  19),   S(-20, -33),
            S( -8, -11),   S(  5,   1),   S(-28, -42),   S( 24,  29),   S( 19,  20),   S( 20,  47),   S( 38,  63),   S(-15, -12),
            S(-13, -23),   S( -1,   1),   S(-21, -17),   S(  2,   4),   S( 26,  40),   S( 22,  48),   S( 17,  22),   S(  3,  -1),
            S(-24, -41),   S(-25, -39),   S(  5,  10),   S(-10,  -2),   S(-13, -19),   S( 37,  50),   S( 38,  37),   S(  1,  -8),
            S(-25, -51),   S(  0,  -3),   S(-31, -57),   S( -2,  -7),   S( -2,  -7),   S(  1, -10),   S(  3,  -2),   S( -9, -36),

            /* queens: bucket 12 */
            S(  6,  12),   S( -9, -18),   S( -6, -11),   S(-10, -11),   S(-14, -18),   S( -4,  -9),   S(  1,   2),   S( -6,  -9),
            S( -3,  -9),   S( -7, -12),   S(-15, -26),   S( -7,  -9),   S( -6, -11),   S(-12, -17),   S( -9, -20),   S( -7, -12),
            S(  0,   0),   S(  2,   3),   S(  6,   4),   S( -3,  -1),   S(-10, -20),   S( -9, -22),   S(-13, -30),   S(-14, -27),
            S(  1,  -1),   S(  4,   1),   S( 10,  23),   S( 10,  28),   S( 18,  32),   S(  1,  -1),   S(  1,   3),   S( -6, -24),
            S(  4,   0),   S( -7,  -5),   S( 32,  63),   S( -4,  -6),   S(  7,  28),   S(  2,   4),   S(-13, -30),   S(  2,   3),
            S( -3,  -2),   S(  0,  19),   S( 26,  43),   S( 23,  28),   S( 12,  24),   S(  4,   4),   S(  2,   4),   S(-11, -24),
            S( -1,   1),   S( -2,   4),   S( 26,  46),   S( 33,  63),   S(  3,   3),   S(  5,   8),   S( -1,   1),   S( -8, -13),
            S(-18, -43),   S(-13, -25),   S( -6, -10),   S(-17, -38),   S(  3,   4),   S( -1,   1),   S(  6,  16),   S(-10, -10),

            /* queens: bucket 13 */
            S( -4, -10),   S( -6, -10),   S( -5, -13),   S( -3, -12),   S( -7, -19),   S( -3, -16),   S( -7, -21),   S( -6, -15),
            S(  5,  12),   S(  3,   4),   S( -1,  -5),   S(-10, -23),   S( -9, -22),   S(  4,  12),   S(  1,   1),   S(-14, -24),
            S(-12, -23),   S(  0,  -3),   S( -4,  -6),   S(-13, -39),   S( -7, -13),   S( -3, -10),   S( -3, -11),   S(-17, -32),
            S( -2,  -1),   S(  1,  -2),   S( -1, -11),   S( -3,  -2),   S( 15,  24),   S(-12, -33),   S( -2,  -9),   S(-14, -30),
            S(-11, -21),   S(  1,  24),   S( 13,  32),   S( 13,  28),   S( 39,  72),   S(  2,   0),   S(-12, -31),   S( -8, -23),
            S(  0,  -3),   S(  1,  10),   S( 37,  82),   S(  8,  34),   S(-17,  -9),   S( -5, -14),   S(  1,   4),   S(-14, -37),
            S(-12, -23),   S( -2,   0),   S(  8,  30),   S( 16,  26),   S(  3,   2),   S( -6,  -4),   S( -2,  -6),   S( 13,  30),
            S(-15, -34),   S(-17, -30),   S(-13, -22),   S( -1,  -3),   S(  3,   4),   S( -4, -13),   S( -9, -21),   S( -1,  -3),

            /* queens: bucket 14 */
            S( -7, -16),   S( -4, -16),   S( -2, -11),   S( -4, -10),   S(  6,  11),   S( -3, -12),   S(  1,  -7),   S(-12, -29),
            S( -9, -20),   S(  9,  23),   S( -4, -17),   S( -6, -11),   S(-19, -34),   S( -9, -26),   S( -1,   0),   S( -6, -15),
            S(  0,  -4),   S(-10, -18),   S( -9,  -7),   S( -6,   1),   S(  5,   4),   S( -2, -11),   S(  1,   0),   S(-17, -25),
            S( -6, -14),   S(  3,  -3),   S( -5,  -5),   S( 10,  24),   S( 11,  23),   S(  1,  14),   S( 14,  28),   S( 12,  20),
            S( -5, -14),   S(  4,  15),   S(-20, -34),   S(  9,   5),   S( 10,  16),   S( 14,  25),   S( 11,   7),   S( -2, -12),
            S( -1,  -2),   S(  6,  11),   S( 13,  28),   S(  7,  10),   S(  0,  14),   S( 12,  24),   S(  3,   5),   S( -1,  -9),
            S( -6,  -9),   S( -1,  -6),   S( 14,  27),   S( 14,  26),   S(  7,  19),   S( 11,  31),   S(  6,   1),   S(  2,   3),
            S(-12, -21),   S( -6, -18),   S( -7, -22),   S(  3,  -5),   S( -2,   1),   S( -1,  -2),   S(-19, -35),   S(-17, -41),

            /* queens: bucket 15 */
            S( -5, -13),   S( -5, -19),   S( -4, -10),   S( -5, -10),   S(  0,   3),   S( -5, -10),   S(-10, -22),   S( -6, -24),
            S(  3,   5),   S( -9, -22),   S( -8, -21),   S( -2,  -8),   S(  0,  -6),   S( -5,  -5),   S( 16,  31),   S(  7,  14),
            S( -2, -10),   S( -4,  -9),   S(  1,   1),   S(-11, -23),   S( -5, -18),   S(  9,  17),   S(  0,   0),   S(  0,  -2),
            S( -1,  -9),   S( -3,  -7),   S( -5,  -8),   S(  6,   3),   S( -2, -13),   S(-13, -10),   S(  5,   6),   S(  1,   5),
            S( -2,  -3),   S( -3, -13),   S( -4, -11),   S(  7,   5),   S(  5,   7),   S(  8,  16),   S(-10, -23),   S(  3,   5),
            S( -2,  -7),   S( -4,  -7),   S(  1,  -7),   S( -9, -12),   S( -8, -21),   S( 34,  62),   S( -2, -10),   S( -1,  -7),
            S( -2,   2),   S( -3, -17),   S( 10,  22),   S(  6,   9),   S( 13,  24),   S( 33,  57),   S( 16,  31),   S(  6,   9),
            S(  0,   3),   S( -4,  -6),   S( -4, -10),   S( 15,  28),   S( -3,  -4),   S(-10, -29),   S(-12, -22),   S( -5, -12),

            /* kings: bucket 0 */
            S(  9,  53),   S(  5,  78),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  68),   S( 91,  74),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-17,  28),   S(-71,  37),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 13,  44),   S(  2,  44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-45,  38),   S(-39,  29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  42),   S( 25,  38),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18,  61),   S( -5,  45),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60,  86),   S( 11,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30, -39),   S( 34, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-66,  -6),   S(  7,  13),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  9, -49),   S(-40, -24),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  5, -12),   S(  0, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-11, -21),   S(-23, -17),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  -7),   S(-13,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 26,  10),   S(-15, -22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 33,  35),   S(-43,  13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94, -44),   S(-14, -10),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -48),   S( 29, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -9, -46),   S(-38, -47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 71, -60),   S( 39, -52),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -9, -56),   S(-51, -39),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 63, -66),   S( 58, -52),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0, -26),   S(-102, -54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 84, -43),   S(-12, -49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -48),   S( 54, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34, -98),   S(  0, -18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 20, -27),   S( 59, -43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -64),   S( 24, -73),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 62, -49),   S( 51, -44),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  8, -70),   S(-14, -44),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 52, -42),   S(-27, -62),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -3, -64),   S(-17, -112),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -11),   S(-40,  12),   S( -1,  -7),   S(-20,  21),   S(  7,   3),   S( 30,   4),   S( 30,  -9),   S( 42,  -4),
            S(-15, -28),   S(-31,  -3),   S(-15, -15),   S( -7, -10),   S(  9,   2),   S( -1,   1),   S( 23,  -4),   S( 19,  11),
            S(  5, -15),   S( -1, -12),   S( 29, -31),   S( 13, -21),   S( 17,  -5),   S( 21,  21),   S(  6,  32),   S( 49,   4),
            S( 22, -18),   S( 46,   0),   S( 59, -19),   S( 47,   2),   S( 21,  41),   S( -2,  75),   S( 24,  65),   S( 72,  47),
            S( 98, -52),   S(103,   4),   S( 93, -25),   S( 27,  47),   S( 59, 126),   S( 44,  89),   S( 26, 135),   S( 77,  94),
            S(-160, -92),  S(-139, -102), S( 30, -167),  S( 62,   7),   S(119, 139),   S( 90, 138),   S(118,  83),   S(101, 161),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62,  22),   S(-50,  11),   S(-35,  10),   S(-50,  46),   S(-24,   6),   S( -9,  11),   S(  8,  -7),   S(  4,  15),
            S(-57,  -1),   S(-41,  -2),   S(-46,  -1),   S(-26,   3),   S( -9,   5),   S(-27,   7),   S( -1,  -8),   S(-17,  10),
            S(-39,  16),   S(-14,  12),   S(-25,   1),   S( 11, -16),   S( -7,  19),   S( -9,  10),   S(-16,  19),   S(  5,  10),
            S(-18,  29),   S( 40,   4),   S( -7,  16),   S( 25,  24),   S( 18,  20),   S(-20,  32),   S( 26,  20),   S( 51,  37),
            S( 12,  15),   S( 71, -30),   S(104, -28),   S(108, -28),   S( 55,   3),   S( 21,  38),   S(-46,  59),   S( 26,  83),
            S(166, -51),   S( 24, -31),   S( 26, -116),  S(  1, -114),  S(-37, -61),   S(-48,  71),   S( 55, 131),   S(114, 150),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-59,  35),   S(-42,  13),   S(-36,   6),   S(-29,  28),   S(-51,  41),   S(-38,  15),   S(-21,  -3),   S(-26,  20),
            S(-53,  13),   S(-39,  12),   S(-47,   5),   S(-46,  24),   S(-44,  24),   S(-52,  10),   S(-22, -11),   S(-50,   9),
            S(-38,  42),   S(-34,  38),   S(-22,  18),   S(-24,  23),   S(-32,  26),   S(-24,   7),   S(-24,   5),   S(-25,   7),
            S(-14,  70),   S(-21,  53),   S( -3,  30),   S(  2,  34),   S( -6,  37),   S(-17,  13),   S( 18,  12),   S( 29,   5),
            S(-28, 117),   S(-44,  90),   S(-17,  26),   S( 35, -31),   S(128, -30),   S(103,  -5),   S( 85, -35),   S( 44,  -9),
            S(-25, 217),   S( 68, 121),   S( 28,  55),   S(  1, -70),   S(  4, -169),  S(-19, -173),  S( 34, -45),   S(132, -64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  17),   S(-12,  11),   S( -8,  17),   S(-16,  45),   S(-23,  52),   S(  5,  36),   S( -3,  12),   S(-16,   1),
            S( -2,   4),   S(  5,  14),   S(-17,   8),   S(-19,  21),   S( -4,  26),   S( -2,  15),   S( -2,   2),   S(-39,   5),
            S( 10,  36),   S( -5,  51),   S(  2,  24),   S( -3,  12),   S( 14,   1),   S( 18,   0),   S(  3,  -7),   S(-14,  -1),
            S( 17,  81),   S(  5,  87),   S( 16,  64),   S( 15,  36),   S( 30,   8),   S( 37, -17),   S( 26,  13),   S( 42, -12),
            S( 10, 143),   S(-36, 162),   S(-34, 161),   S( -7, 110),   S( 54,  44),   S( 96,  -5),   S( 88, -23),   S( 88, -26),
            S( 60, 146),   S( 49, 222),   S( -1, 273),   S( 29, 168),   S(-37,  95),   S(-10, -130),  S(-102, -172), S(-186, -151),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 67,  -7),   S( 35,  -9),   S(-27,   3),   S(  3, -29),   S( -7, -16),   S(-21,  -7),   S(-37,  -1),   S(-55,  29),
            S( 42, -18),   S( 34,   0),   S( 26, -33),   S(-22,   2),   S(-46, -16),   S(-37, -14),   S(-71,  -6),   S(-24, -16),
            S( 97, -26),   S(127, -34),   S( 86, -45),   S(-26,  -6),   S(-85,  15),   S(-33,  12),   S(-43,   5),   S(-77,  28),
            S(-38, -79),   S( 52, -120),  S( 38, -16),   S(-42,  20),   S(-14,  26),   S(-73,  58),   S(-16,  49),   S(  0,  45),
            S( 26, -95),   S( -9, -110),  S( 33, -93),   S( 40,   2),   S( 46,  86),   S( 80,  52),   S( 36,  55),   S(  5,  87),
            S( 17, -33),   S(-12, -77),   S(  0, -96),   S( 30,  37),   S( 55,  94),   S( 67, 144),   S( 51, 118),   S( 39, 130),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62,  33),   S(-30,  18),   S( 19,  -5),   S( 67,  -5),   S( 36,   0),   S( 13,  -5),   S(-26,   0),   S(-62,  40),
            S(-42,   5),   S(-15,  11),   S(  8, -13),   S( 16,   2),   S(-10,   2),   S(-10,  -8),   S(-47,  -5),   S(-90,  25),
            S(-12,  13),   S( 23,  17),   S( 77,  -1),   S( 41,  17),   S(-17,  25),   S(-19,   6),   S(-20,   7),   S(-46,  30),
            S( 44,   0),   S( 26, -18),   S(-14, -40),   S(-13, -20),   S( 14, -18),   S(-21,   6),   S( 33,  10),   S(  7,  32),
            S( 81, -12),   S( 37, -55),   S( 38, -84),   S(-23, -61),   S( 34, -38),   S( 33,  11),   S(  7,  19),   S(-20,  80),
            S( 55,  22),   S( 23, -22),   S(  6, -73),   S( 10, -86),   S( -1, -63),   S( 41,  26),   S( 84, 107),   S( 68, 105),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87,  24),   S(-36,  -6),   S(-10,  -9),   S(-11,  21),   S( 39,  -2),   S( 25,   2),   S( 10,   0),   S( -1,  20),
            S(-69,  -2),   S(-32, -11),   S(-26, -16),   S( 27,  -2),   S(  0,  11),   S( 22, -12),   S(  6,  -6),   S( -8,  -1),
            S(-52,  23),   S(-41,  15),   S( -8,   4),   S( 11,  12),   S( 29,  21),   S( 67, -14),   S( 54,  -7),   S( 26,  -1),
            S(-15,  40),   S(  4,  21),   S( 29,   6),   S( 34,  -4),   S(-24, -31),   S( 48, -58),   S( 36, -23),   S(100, -29),
            S( 39,  64),   S( 50,   4),   S( 73, -16),   S(  3, -44),   S(  1, -71),   S(-26, -56),   S( 70, -81),   S( 85, -21),
            S(126,  38),   S(113,  60),   S( 91,   0),   S( 55, -67),   S(  2, -95),   S(  3, -101),  S(  8, -39),   S( 61,  -2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48,  -9),   S(-50, -18),   S(-29, -17),   S(-33,  17),   S(  3,   7),   S( 53, -12),   S( 37, -19),   S( 59,  -9),
            S(-51, -22),   S(-40, -22),   S(-33, -30),   S(-46,   9),   S( -3,  -4),   S( 47, -29),   S( 38, -19),   S( 53, -20),
            S(-38,  14),   S(-50,  15),   S(-41,   8),   S(-43,  -2),   S( 12,  -8),   S( 36, -26),   S( 85, -20),   S( 73, -23),
            S( -1,  28),   S(-38,  44),   S(-37,  47),   S(-34,  38),   S(  3,  -9),   S( 71, -71),   S( -3, -57),   S( 11, -98),
            S( 32,  41),   S(-33,  87),   S( 55,  80),   S( 21,  60),   S(-12,  37),   S(-41, -42),   S(-45, -143),  S(-14, -100),
            S(129,  77),   S(106, 117),   S(101,  87),   S( 40, 106),   S( 68, -25),   S( 17, -89),   S(-16, -104),  S(  1, -157),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  -6),   S(-21,  -4),   S( 55,  -4),   S( -7, -27),   S(-21, -55),   S( 30, -30),   S( 68, -60),   S( 43, -37),
            S(-38, -35),   S(-21, -11),   S(-55, -59),   S(-35, -29),   S(-45, -44),   S(  8, -36),   S(  5, -55),   S(-44, -38),
            S(-30, -51),   S(  6, -46),   S(  1, -54),   S(-33, -42),   S(-24,  -1),   S(-27, -12),   S(-22, -21),   S(-47,   4),
            S( -7,   1),   S(-29, -27),   S( 17,   4),   S( 14,  -8),   S( 11,   6),   S(-30,  42),   S(-13,  27),   S( -4,  34),
            S( 19,  38),   S(-19, -26),   S( 17,  31),   S( 44,  82),   S( 55, 105),   S( 40,  94),   S(  1,  93),   S( -4,  83),
            S( 11,  61),   S( -3,   0),   S( 20,  29),   S( 39, 101),   S( 40,  89),   S( 52, 175),   S( 36, 120),   S( -5,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 14,  -7),   S( 65, -13),   S( 58, -15),   S( -2, -18),   S( -2,   4),   S( 87, -43),   S( 76, -64),   S(-23, -19),
            S(  4, -49),   S(-33, -46),   S(-20, -56),   S(-24, -21),   S(-15, -41),   S( 12, -47),   S(  5, -50),   S( 28, -44),
            S(-87, -21),   S( -7, -58),   S(-13, -76),   S(-43, -48),   S( 32, -39),   S(-16, -43),   S(-58, -30),   S(-17, -11),
            S(-63,  -7),   S(-76, -33),   S(  0, -65),   S(-13, -37),   S( 31, -40),   S(-10, -26),   S( -5,   3),   S( 10,   8),
            S(-36,   9),   S( -5, -31),   S( 22, -48),   S( 25,   7),   S( 11,  43),   S( 12,  41),   S( -5,  57),   S( -6,  52),
            S( -5,  48),   S( 19,  45),   S( 11,   9),   S( 21,  -1),   S( 32,  70),   S( 16,  26),   S( 19,  80),   S( -1,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -62),   S(-16, -52),   S(-24, -20),   S( -8, -34),   S( 45,  -9),   S(171, -40),   S( 85, -28),   S( 51, -23),
            S(-42, -55),   S(-72, -55),   S( 37, -73),   S( 50, -53),   S( 11, -27),   S( 38, -40),   S( -2, -44),   S( 16, -41),
            S(-70, -31),   S(-19, -51),   S( -1, -62),   S( 23, -51),   S(-51, -51),   S(  8, -74),   S( -5, -67),   S( 52, -59),
            S( -7, -23),   S(  0, -20),   S(-15, -35),   S( -8, -49),   S(-23, -35),   S(-63, -45),   S(-10, -46),   S(-14, -30),
            S( -7,   3),   S( 25,   4),   S( 12,  22),   S( 13, -20),   S( 33, -21),   S( 16,  12),   S(-41, -28),   S( 10,  -5),
            S( -2,  18),   S( -2,  27),   S( 28,  43),   S( 18,  41),   S( 21,  39),   S(  1,  31),   S( -9, -21),   S( 15,  28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -48),   S( -9, -40),   S( 10, -33),   S(  1, -30),   S( -1, -25),   S(105, -10),   S(119, -37),   S( 62, -18),
            S(-58, -54),   S(-40, -65),   S(-49, -60),   S( -3, -46),   S(  9, -38),   S(-12, -42),   S(  9, -28),   S( -3, -57),
            S(-55, -11),   S(-11, -33),   S(-19, -19),   S(  8, -31),   S(-21, -36),   S( -1, -34),   S(-39, -83),   S( -4, -65),
            S(  1,   5),   S(  0,  23),   S( 23,  31),   S( 28,   0),   S( 26, -16),   S(-16, -31),   S(-37, -32),   S(-22, -40),
            S( 13,   4),   S(  0,  61),   S(  4,  86),   S( 35,  32),   S( 34,  73),   S(  2,   6),   S( -9, -30),   S(-11, -20),
            S( 22,  61),   S( 28,  77),   S( 47,  95),   S( 41,  73),   S( 23,  52),   S( 26,  66),   S( 13,   6),   S( 24,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -81),   S(-14,  -6),   S(-16,  -5),   S( -9, -12),   S(  5, -16),   S(-15, -51),   S( 18, -16),   S( -5, -30),
            S(-19, -30),   S(-39, -11),   S(-37, -58),   S(-22, -31),   S(-29, -50),   S(-13, -44),   S(-33, -42),   S(-16, -58),
            S(-26, -16),   S(  2, -52),   S(-10, -92),   S(-41, -101),  S( -7, -15),   S(-15, -18),   S(-46, -26),   S(-46, -26),
            S(-22, -21),   S( -8, -23),   S(-17, -20),   S(  4,   1),   S(  0,  29),   S( -9,  77),   S(  2,  75),   S(-35,  27),
            S( 12,  31),   S(  3,  14),   S(  9,  48),   S( 20,  64),   S( 43, 116),   S( 32,  87),   S( 10, 126),   S(  4, 139),
            S( 14,  10),   S(  3,   2),   S( 16,  69),   S( 19,  78),   S( 32, 116),   S( 34, 102),   S(-15,  21),   S(-17,  21),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36, -36),   S(-20, -55),   S( 13,  10),   S(-10, -33),   S(  7, -21),   S(  5, -43),   S(  4, -61),   S( -8, -61),
            S(-62, -65),   S(-57, -68),   S(-37, -54),   S(  7, -33),   S(-30, -42),   S(-32, -44),   S(-24, -64),   S(  5, -52),
            S(-27, -33),   S(-29, -71),   S(  6, -56),   S(-18, -68),   S( -3, -54),   S(-13, -46),   S(-29, -46),   S(-18, -31),
            S(-13,  23),   S( -7, -26),   S( 13, -12),   S(  1,  19),   S(  1,  22),   S(-22,  44),   S(-16,  12),   S(-30,  11),
            S( -8,  19),   S(  9,  40),   S(  2, -15),   S( 13,  16),   S( 34, 110),   S( 18,  84),   S( 13, 106),   S( -7,  60),
            S( 11, 102),   S( 26,  49),   S( -1,  11),   S( 11,  29),   S( 16,  76),   S( 14,  83),   S( -6,  46),   S(  0,  73),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -82),   S( -5, -60),   S(  8, -24),   S( -3,   9),   S(-11, -33),   S(-28,  -7),   S( 12, -62),   S(  3, -13),
            S(-32, -70),   S(-77, -81),   S( -8, -68),   S(-22, -85),   S(-27, -58),   S( -9, -33),   S(-10, -36),   S( -9, -44),
            S(-28, -66),   S(-37, -72),   S(-19, -38),   S(  0, -61),   S(-13, -51),   S(-12, -57),   S( -9, -55),   S(-33,  -1),
            S(-11,   9),   S(-31, -30),   S( -6,  15),   S(-17, -10),   S(  3, -20),   S( -3,  -4),   S( -6,   6),   S( -3,  23),
            S( -3, -14),   S(  3,  40),   S(  6,  59),   S( -2,  34),   S( 32,  67),   S( 12,  16),   S( 21,  84),   S( 13,  73),
            S(-13,  30),   S( 10,  38),   S( 20,  90),   S( 16,  69),   S( 21,  70),   S(  1,  34),   S( 21,  86),   S( 25, 110),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -39),   S( -5, -43),   S(-25, -54),   S( -8, -14),   S( -1, -19),   S(-27,  -4),   S( -6, -12),   S( -5, -52),
            S(-31, -52),   S(-47, -88),   S(-33, -84),   S( -4, -36),   S( -6, -30),   S(-53, -25),   S(-33, -30),   S(-32, -78),
            S(-29, -20),   S(-43, -35),   S(-44, -53),   S( 29, -39),   S(-25, -19),   S( -4, -19),   S(  8, -14),   S(-15,   1),
            S(-35,  20),   S(-36,  -1),   S( -7,  35),   S( 11,  43),   S( 19,  44),   S( -1,  -7),   S(-18, -14),   S( -2,  50),
            S(  8, 101),   S(  8,  74),   S(  4, 104),   S( 15,  55),   S( 20,  85),   S( 13,  80),   S( 17,  63),   S( 12,  36),
            S(-27,  19),   S( -5,  24),   S(  3,  85),   S( 30,  91),   S( 25,  85),   S( 18,  68),   S( 23,  61),   S( 14,  59),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-107, -44),  S(-28, -15),   S(-18,  -3),   S(  7,   6),   S(-28, -23),   S(-26,  22),   S( -2,   0),   S(-99, -51),
            S( 10, -15),   S(-26,  10),   S(-30, -26),   S(  0,   0),   S( -6,  11),   S(-13,  17),   S(-24, -32),   S(-21, -52),
            S( -4, -40),   S( 11,  11),   S(  2,  29),   S( 35,  31),   S( -7,  16),   S(  3,   7),   S(-31,  11),   S(-19, -40),
            S( 13,  37),   S( 25,  68),   S( 16,  46),   S( 45,  30),   S( 17,  51),   S(  6,  53),   S( 33,  -3),   S( -1,  -7),
            S( 56,  59),   S(-14,  64),   S( 45,  93),   S( 50,  43),   S( 56,  54),   S( 28,  35),   S( 17,  31),   S( -6,  41),
            S( 70, -24),   S(-32,  71),   S(137,  22),   S( 73,  38),   S( 70,  46),   S(  1,  72),   S( 57,  -5),   S(-21,   8),
            S( 30, -24),   S( -9, -40),   S( 50,  47),   S( 31,  61),   S( 69,  41),   S( 36,  34),   S(-37,  49),   S(-32,  28),
            S(-93, -161),  S( -8, -15),   S( -4, -18),   S( 19,  44),   S( 32,  79),   S( 29,  42),   S(-45,   7),   S(  5,   3),

            /* knights: bucket 1 */
            S( 32,   6),   S(-64,  46),   S(-36,  17),   S(-50,  49),   S(-38,  57),   S(-26, -17),   S(-30,  14),   S(-48, -10),
            S(-36,   8),   S(-44,  63),   S(-41,  43),   S(-13,  25),   S(-25,  39),   S(  7,  18),   S(-19,  14),   S(-31, -53),
            S(-28,  19),   S(-14,  20),   S(-25,  31),   S(-20,  71),   S(-23,  62),   S(-21,  19),   S(-52,  39),   S(-22,   4),
            S( -9,  57),   S(  3,  60),   S(-10,  64),   S(-15,  61),   S(-13,  69),   S(-22,  75),   S(-21,  41),   S(-27,  39),
            S( 44,   7),   S(-26,  32),   S( 12,  85),   S(-15,  71),   S( 24,  50),   S(  4,  71),   S(-16,  43),   S(-22,  69),
            S( 18,  61),   S( 49,  21),   S(105,  25),   S(127,  46),   S( 63,  48),   S(-11,  68),   S( 33,  39),   S(  0,  45),
            S( 43,   3),   S( 34,  -7),   S( 35,  -6),   S( 43,  63),   S(-13,  31),   S( 29,  26),   S(  5,  78),   S(-37,  64),
            S(-171, -39),  S( -6, -40),   S(-34, -76),   S(-10,  17),   S( -1,   3),   S( 26,  52),   S( 22,  45),   S(-90,  38),

            /* knights: bucket 2 */
            S(-49, -13),   S(-37,  43),   S(-51,  31),   S(-31,  39),   S(-23,  28),   S(-55,   6),   S(-30,  23),   S(  5, -22),
            S(-33,  27),   S(  6,  34),   S(-38,  42),   S(-15,  34),   S(-29,  43),   S(-13,  21),   S(  1,  35),   S(-34,  38),
            S(-42,  58),   S(-32,  42),   S(-30,  39),   S(-35,  74),   S(-20,  64),   S(-33,  40),   S(-30,  33),   S(-16,  15),
            S(-21,  73),   S( -4,  52),   S(-30,  95),   S(-41, 101),   S(-27,  77),   S(-20,  63),   S( -6,  57),   S(-12,  50),
            S(-22,  92),   S(-25,  85),   S(-11,  87),   S(  3,  73),   S(-35,  84),   S(  6,  86),   S(-41,  74),   S( 24,  15),
            S(-17,  83),   S( -7,  81),   S(-42, 114),   S( 41,  51),   S( 16,  87),   S(117,  28),   S( 59,  37),   S(-21,   9),
            S( 21,  42),   S(-42,  79),   S( 58,  18),   S( 22,  13),   S(-13,  43),   S( 31,  -7),   S( 48,  42),   S( -7,  28),
            S(-22,  11),   S(  4,  72),   S( 25,  77),   S(-19, -17),   S(-36,  -4),   S(-48, -46),   S( 35,  26),   S(-154, -48),

            /* knights: bucket 3 */
            S(-65,  49),   S(-18, -18),   S(-15,  -4),   S(-16,  36),   S( -3,  11),   S(-16,  15),   S(-20,  -5),   S(-30, -47),
            S(-24,   6),   S(-13,  37),   S(-14,  43),   S( -7,  34),   S(-13,  38),   S( 12,  24),   S( 17,  -1),   S( 15, -19),
            S(-25,  46),   S(-20,  33),   S( -9,  58),   S(-11,  77),   S(  5,  74),   S(-13,  53),   S(  6,  32),   S(  6,   5),
            S( -7,  35),   S( 11,  65),   S(  4,  85),   S(-14, 100),   S(  9, 108),   S(  7, 102),   S( 18,  81),   S(  1,  62),
            S(-19,  77),   S( -2,  79),   S( 13,  95),   S( 28, 112),   S( -1,  99),   S( 17, 117),   S(-35, 117),   S( 41,  97),
            S(-31,  68),   S( 20,  69),   S( 25,  89),   S( 11, 109),   S( 57,  95),   S( 92, 133),   S( 44, 118),   S(  2, 139),
            S(-35,  83),   S(-27,  88),   S(-40, 113),   S( 43,  81),   S( 30,  94),   S( 95,  40),   S( -3,  27),   S( 70,   1),
            S(-209,  74),  S(-15,  88),   S(-25, 112),   S( 17,  87),   S( 54, 101),   S(-75,  94),   S(  6, -25),   S(-49, -95),

            /* knights: bucket 4 */
            S(  9,  23),   S(-36,   3),   S(-75,  18),   S(-33, -34),   S(-43,  43),   S(-10,  -9),   S( 22,  11),   S(-17, -28),
            S( 13,  27),   S(  9, -18),   S(-26,  12),   S(-17,   6),   S(  2, -27),   S( 29, -71),   S(-21,  -1),   S(-44,  -5),
            S(  2, -25),   S( 28, -31),   S( 65,  -7),   S( 64, -12),   S( 17,  33),   S( 68, -43),   S(  2, -33),   S(-46, -43),
            S(-21, -34),   S( 20, -25),   S( 52,   7),   S( 61,   4),   S( 20,  32),   S( 22,  34),   S(-27,  41),   S(-34,  27),
            S(-15, -62),   S( 39, -19),   S( 76,  16),   S( 59,  39),   S( 48,  -2),   S( 25,  46),   S(  6,  15),   S(-36,  28),
            S( -1, -18),   S(-30, -23),   S( 50, -11),   S( 72,  20),   S(-25,  31),   S(  2,  36),   S( -3,  24),   S( 33,  22),
            S(-25, -34),   S(-40, -25),   S(-11, -14),   S( 17,  11),   S( 44,  38),   S( -8,  38),   S( 11,  43),   S(-41, -36),
            S( -2,  11),   S( -3, -22),   S(-18, -29),   S(  2,  20),   S(  8,   5),   S(-12,  10),   S( -2,  21),   S(-20, -10),

            /* knights: bucket 5 */
            S( 19,  35),   S(  3,  45),   S(-11,  15),   S(-16,   4),   S(-15,  16),   S( 48,  -3),   S(-42,  -5),   S(  4,  17),
            S(  9,   9),   S( 56,  21),   S( 20,   7),   S(  7,   9),   S( 49,  -9),   S( -4,   5),   S( -6,  26),   S(-48,   4),
            S(-20,  -1),   S(  1,   2),   S( 20,   8),   S( 59,   5),   S( 24,  20),   S( -1,  23),   S( 43,   5),   S(-17,  24),
            S( 40,  11),   S( 55, -19),   S( 61,  10),   S(108,  -8),   S(112,   6),   S( 95,   1),   S(  9,   3),   S( 23,   6),
            S( 62,   2),   S( 82,   7),   S(112, -11),   S(115,  -7),   S(116, -10),   S( 49,  17),   S( 12,   2),   S( 37,  22),
            S( -2, -15),   S( 35, -18),   S( 16, -27),   S( 12,  12),   S( 76,   6),   S( 12,   5),   S( -6,  25),   S( 28,  37),
            S( 14,  24),   S(-27, -50),   S( 12, -28),   S(-21, -21),   S( -1, -58),   S( 13,  15),   S( 19,  49),   S( 33,  29),
            S(-28, -35),   S(-18, -36),   S(  9, -15),   S(-20, -43),   S(  5, -11),   S( -3,  31),   S(  9,  36),   S(  5,  33),

            /* knights: bucket 6 */
            S(-15,  -8),   S(-15,  -8),   S(-17,  -3),   S( -5,  37),   S(  3,  31),   S( 11,  17),   S(-31,  47),   S(-33,  14),
            S( 32, -20),   S( -7,  29),   S( 44, -18),   S( 31,   7),   S( 65,   6),   S(-10,  34),   S( 19,  38),   S( -5,  35),
            S( 10,   8),   S( 54,  -2),   S( 43,  12),   S( 54,  29),   S( 28,  36),   S(-40,  37),   S(  8,  28),   S( -9,  35),
            S( 63,  23),   S( 93,  -4),   S( 59,  23),   S( 98,   2),   S( 80,  10),   S( 78,   6),   S( 38, -12),   S(-23,  49),
            S(-20,  58),   S( 51,  21),   S(121,   5),   S(120,  15),   S(113, -17),   S( 66,  28),   S(169, -16),   S( 23,  12),
            S(  5,  21),   S( 30,  10),   S( 61,  17),   S( 20,  40),   S( 39,  -2),   S( 58,   9),   S( 12,  -1),   S(  5,   4),
            S( -1,  33),   S( 33,  42),   S( 40,  46),   S( 28, -17),   S( 40,  30),   S( 33, -42),   S( -6,   0),   S( -2,  15),
            S(  6,  43),   S( 11,  49),   S(  2,  29),   S( 18,  18),   S( 28, -18),   S( 13,  10),   S(  8,  32),   S(-23, -21),

            /* knights: bucket 7 */
            S( -6, -24),   S( -1, -22),   S(  6, -22),   S(-39,  13),   S(  2, -18),   S(-33,  13),   S(  4,   7),   S( -2,  21),
            S(-13, -35),   S( 30, -36),   S(-40,   0),   S(-26,  13),   S( 25,  -2),   S( 48,  14),   S( -7,  41),   S(-53,  25),
            S(-32, -30),   S( -7, -24),   S( 32, -13),   S( 52,  17),   S( 73,  10),   S( 34,  21),   S( 27,   6),   S(-14,  29),
            S(-49,  36),   S( 29,   9),   S( 57,   6),   S( 89,  11),   S( 98,   1),   S( 98,  23),   S( 77,   8),   S( 73,   5),
            S(  3,  20),   S( 34,  -1),   S( 16,  41),   S( 83,  14),   S(153,  -5),   S(174, -12),   S(212, -14),   S( 33,  26),
            S(-20,   9),   S( 41,   0),   S( 19,  15),   S( 67,  23),   S(120,   0),   S(140,  -4),   S( 26, -37),   S( 11, -52),
            S(  7,  25),   S( 25,   7),   S( 42,   9),   S( 26,  38),   S( 54,  15),   S( 20,  16),   S( -8, -35),   S(-10, -28),
            S(-41, -45),   S( -1,  10),   S(-12,  33),   S(  6,  21),   S(  3,   9),   S( 19,   5),   S( -8, -45),   S( -1,  -5),

            /* knights: bucket 8 */
            S( -5,   6),   S(  4,  -8),   S(  7, -12),   S(-24, -63),   S(  0, -24),   S( -2, -27),   S(  8,   0),   S( -8, -51),
            S(-13, -34),   S( -8, -40),   S(-10, -61),   S( -7, -32),   S( -7,   3),   S( -2, -48),   S( -3, -33),   S(  3, -18),
            S( -1, -27),   S( -6, -30),   S( -5, -76),   S( 10, -23),   S( 12, -26),   S( 26,  -1),   S(  4, -53),   S(  9, -29),
            S(-20, -75),   S(-16, -50),   S(  3,   5),   S( 10, -19),   S( 10, -38),   S(  4,  -2),   S(-22, -64),   S(-22, -42),
            S( -7, -31),   S(  0, -51),   S(  6, -23),   S( 16, -48),   S( -2, -24),   S(  0, -15),   S( -2, -23),   S( -6, -43),
            S( -1,   6),   S( 19,  -7),   S( -3, -38),   S( -1, -41),   S(  9,  -6),   S( -2, -15),   S( -4, -34),   S(-10, -29),
            S( -1,  16),   S(-12, -62),   S(-15, -35),   S( 15,   5),   S(  3, -20),   S( 17,   1),   S( -6, -10),   S( -1,  15),
            S(  3,   8),   S( -4,  -9),   S( -7, -14),   S(  6,   2),   S(  0,  11),   S( -5, -13),   S( -6,  -7),   S( -2,  -9),

            /* knights: bucket 9 */
            S(-14, -68),   S( -7, -23),   S( -7, -66),   S( -1, -59),   S(-16,   1),   S(-19, -32),   S( -7, -23),   S(  1,  -6),
            S(-14, -46),   S(-14, -56),   S(-16, -94),   S( -8, -70),   S(-12, -57),   S(  1, -75),   S( -5, -46),   S(-21, -35),
            S(-10, -55),   S(-17, -34),   S(-14, -48),   S(  5, -61),   S( -1, -26),   S( 27,  -4),   S( -6, -15),   S( 10,   3),
            S(-25, -65),   S( -1, -42),   S( -8, -60),   S(  4, -40),   S(  2, -30),   S(  9, -26),   S(-22, -80),   S( 10, -15),
            S( 13,  27),   S(-18, -39),   S( 10, -24),   S( -1, -42),   S( -3, -64),   S(  4, -47),   S(  2, -36),   S(  3, -21),
            S(-14, -60),   S( -4, -32),   S(-13, -47),   S( 22, -24),   S(  2, -34),   S( -6, -40),   S( -1, -33),   S(  7,  26),
            S(-12, -33),   S( -5, -14),   S(-22, -14),   S(-13, -59),   S(  8,   4),   S(  8,  13),   S( -4,  -6),   S(-13, -33),
            S(  2,   3),   S(  2, -13),   S(  1,  -5),   S( -9, -47),   S( -3, -14),   S(  3,  -4),   S(  3,  -5),   S(  3,  22),

            /* knights: bucket 10 */
            S( -9, -42),   S(-13, -30),   S(-10, -35),   S(-14, -28),   S( -5, -80),   S(  8, -70),   S( -1,   9),   S( -3, -36),
            S( -6, -38),   S( 18, -18),   S(  6, -47),   S(-15, -84),   S(  5, -56),   S(-17, -79),   S(-21, -18),   S(  9,  48),
            S(  0, -25),   S(  2, -36),   S(  7, -37),   S( 31, -41),   S( 12, -52),   S(  2, -39),   S(  7, -22),   S(  0, -13),
            S( -5, -73),   S(  3, -50),   S( 34, -49),   S(  6, -35),   S(  7, -40),   S( -7, -35),   S(  5, -31),   S( 12, -29),
            S(-10, -61),   S( -1, -39),   S( 23, -26),   S( 12, -32),   S( 26, -29),   S(  7, -49),   S(  4,  -3),   S(  0, -47),
            S( -4, -13),   S(  0, -40),   S( -3, -19),   S(  0, -13),   S(-10, -28),   S(  0, -46),   S( -7, -18),   S(-16, -37),
            S(  7,   8),   S( -3, -25),   S( -1, -45),   S( 17,  -1),   S( -4, -24),   S(-28, -64),   S( -5, -27),   S( -9, -21),
            S( -1, -12),   S(  0,  11),   S(  5,  19),   S(  3,  -7),   S( -2, -29),   S(-10, -34),   S(  7,   1),   S( -1,   1),

            /* knights: bucket 11 */
            S( -3, -25),   S(-23, -55),   S( -1, -45),   S( 13,  -9),   S(-33, -70),   S( -1, -16),   S(-17, -30),   S( 13,  26),
            S( -3, -21),   S(-23, -48),   S(  1, -66),   S( 55, -34),   S( 17,  -7),   S( -2, -53),   S(-19, -65),   S( -9, -11),
            S(-14, -45),   S(-22, -47),   S( 14, -16),   S( 22, -19),   S(  1,   3),   S( 23,  -8),   S( -1, -58),   S( -9, -39),
            S( -5, -40),   S(  9, -22),   S( 31, -28),   S( 24, -10),   S( 53, -22),   S( 14, -16),   S(  5, -56),   S( -9, -19),
            S(-11, -18),   S(  7, -74),   S(  2,   0),   S( 21, -19),   S( 55,   7),   S( 11,   5),   S( -2, -94),   S( -9,  -8),
            S( -6, -23),   S(-12, -65),   S( 11,  -1),   S( 21, -14),   S( -2, -11),   S( -6, -20),   S( -8, -49),   S( -3, -15),
            S(  4,   9),   S(  6,  25),   S(-15, -38),   S(  6,  -2),   S(  6, -46),   S(  2, -39),   S(  9,  -7),   S(  1,   5),
            S( -1, -11),   S( -5, -10),   S(-11, -23),   S(  2,  26),   S(  3, -22),   S(  0,   1),   S(  0,  -8),   S(  1,   0),

            /* knights: bucket 12 */
            S(-16, -63),   S( -1, -29),   S( -2, -20),   S( -1,   4),   S( -4,   6),   S( -2, -15),   S( -2,  -5),   S( -2,  -9),
            S( -8, -46),   S( -1, -10),   S( -3, -26),   S( -3, -38),   S(  5, -14),   S(  4, -11),   S(  1,  -5),   S(  0, -10),
            S( -6, -22),   S(-12, -48),   S( -5, -25),   S(-11, -96),   S(-13, -43),   S(  6,   3),   S( -4, -10),   S( -4, -15),
            S(  2,  13),   S( -1, -42),   S(  0,  -8),   S(  2, -37),   S(  7, -42),   S(  4,   1),   S(  9,  21),   S( -1, -12),
            S( -2, -23),   S( -8, -49),   S(  0, -35),   S( -9, -48),   S( 20,  45),   S(  2, -15),   S( -4,  -7),   S(-13, -30),
            S( -3, -10),   S(  0, -17),   S( -1,   4),   S( -5, -43),   S( -3, -33),   S(  0, -11),   S( 11,  16),   S(  0,   3),
            S( -3, -11),   S( -5, -14),   S(-11, -26),   S(  2,  25),   S( -2,   4),   S( -8, -21),   S( -2,  -5),   S(  0,   0),
            S(  0,   1),   S(  3,  20),   S(  0,  -1),   S( -1,  -9),   S(  2,   2),   S(  1,  13),   S(  0,   0),   S(  0,   3),

            /* knights: bucket 13 */
            S( -6, -21),   S( -4, -16),   S( -4, -48),   S(  2, -17),   S( -7, -38),   S(  1,  -3),   S( -8, -27),   S(  4,  20),
            S(  3,   0),   S( -7, -24),   S( -1,  -7),   S( -7, -23),   S( -8, -46),   S( -6, -13),   S( -6,  -8),   S( -5, -23),
            S(  0, -10),   S(  1,   7),   S(-10, -36),   S(  4,  -8),   S(  3,  11),   S( -6, -27),   S( 16,   6),   S( -3, -17),
            S( -2, -37),   S( -8, -29),   S( -8, -55),   S(  4,   1),   S( -5, -60),   S(  5,  -4),   S(  3,   4),   S(  8,   8),
            S(  0,  30),   S(  1, -31),   S( -9, -78),   S(  4, -33),   S(  1,  -8),   S(  2, -31),   S( -8, -40),   S( -5, -38),
            S( -3, -12),   S(  8,  17),   S( -4,   8),   S( -4, -61),   S(-11, -27),   S( -4, -40),   S(  4,   5),   S(  1,  -2),
            S( -1,  -3),   S(  1,   3),   S( -1,   4),   S(  8,   5),   S(  1,  -2),   S( -2,   1),   S(  7,  12),   S(  1,   1),
            S(  1,   3),   S( -1,  -2),   S(  0,   0),   S( -2,  -4),   S(  1,   7),   S( -2,  -2),   S(  1,   4),   S(  0,   1),

            /* knights: bucket 14 */
            S( -1,  -9),   S(  0,  -2),   S(  9,  28),   S( -5, -12),   S( -3, -31),   S( -3,   8),   S(  1,   2),   S( -1,  -7),
            S( -5, -24),   S(-11, -50),   S( -2, -24),   S(  0, -17),   S( -5, -28),   S( -4, -22),   S( -6, -26),   S(  3,  36),
            S( -3, -30),   S( -5, -29),   S(  5, -11),   S(-10, -51),   S( -1, -31),   S(  7,  11),   S( -4, -26),   S(  0,  -4),
            S( -1,  -2),   S( -6, -38),   S(-12, -45),   S(  9,  14),   S( 11,  15),   S( -3, -46),   S( -7, -26),   S( -2,   2),
            S( -1,  -1),   S(-23, -66),   S( -4, -46),   S( -4, -14),   S( 15,  61),   S(  0, -22),   S( -1, -16),   S(  3,   7),
            S(  1,   6),   S(  3,  27),   S(  4,  51),   S(  2,   1),   S( -1,  -2),   S( -9, -17),   S( -3,   3),   S( -4,  -9),
            S(  2,   6),   S( -3, -12),   S( -3, -15),   S(  3,  18),   S(  6,  14),   S(-10, -39),   S(  5,  10),   S(  2,  -5),
            S(  2,   5),   S( -2,  -7),   S( -1,  -5),   S(  5,  20),   S( -2,  -5),   S(  0,   5),   S(  2,   4),   S(  0,   2),

            /* knights: bucket 15 */
            S( -7, -30),   S( -1,  -9),   S( 10,  37),   S(  8,  23),   S( -1, -29),   S( -3, -15),   S( -6, -53),   S( -2, -26),
            S( -1,  -3),   S(  3,   0),   S(-11, -47),   S( 14,  22),   S(  3, -10),   S(-12, -54),   S( -3, -20),   S(  2,  -3),
            S(  2,  -4),   S( -8, -29),   S( -1, -34),   S( 11, -18),   S(-28, -100),  S(  0, -35),   S( -5, -35),   S( -5, -18),
            S( -3, -18),   S( -6, -22),   S( -8, -29),   S(  1,  12),   S( -2, -47),   S(  2, -21),   S( -1, -31),   S( -2,   7),
            S( -3, -17),   S(  7,  15),   S( -5, -39),   S( -5,  14),   S( 15,  28),   S(  6,   8),   S( -4, -40),   S(  0, -23),
            S( -1,  -3),   S(  1,   9),   S( -4,   4),   S(-14, -49),   S( -7, -22),   S(  2,  32),   S(  1,   9),   S(  8,  25),
            S( -3,  -9),   S( -3, -10),   S(  7,  26),   S(  3,  -7),   S( -1,  24),   S(  9,  31),   S(  3,  16),   S(  6,  13),
            S(  3,   8),   S( -2, -14),   S( -2,  -2),   S( -3, -11),   S(  2,   8),   S(  2,  10),   S(  1,   8),   S(  0,   3),

            /* bishops: bucket 0 */
            S( 43, -28),   S(-23,  25),   S( -6,  -8),   S(-11, -16),   S( -1, -27),   S( 28, -21),   S( 89, -66),   S( 23,  -9),
            S(-35, -22),   S(  5, -23),   S(  6,  -5),   S( 11,  -2),   S( 19,  -7),   S( 62, -37),   S( 51,  28),   S( 59, -34),
            S( 14,  28),   S( 21,   8),   S( 25,  -4),   S( 19,  -3),   S( 42, -18),   S( 50,  16),   S( 45, -20),   S( 22,  -6),
            S( 20, -41),   S( 48, -53),   S( 19,  10),   S( 69, -29),   S( 72,  17),   S( 52,  24),   S( 11,  -7),   S( 18,  16),
            S( 34,   1),   S( 43, -11),   S( 83, -18),   S(114, -14),   S(124, -28),   S( 37,  11),   S( 48,  16),   S( -2,   9),
            S( 21,  29),   S( 98,  10),   S( 64,   9),   S( 61,  -1),   S( 36,   1),   S( 47,  16),   S( 32,  23),   S(-10,   4),
            S(-68, -102),  S( 92,  31),   S( 82,  57),   S( -7,  -8),   S(-12, -16),   S( 39,  -8),   S( -3,  25),   S( -1,  27),
            S(-21, -46),   S(-22,   0),   S(-11, -22),   S(-12, -29),   S(  7,   4),   S( 16,  18),   S(-18,  13),   S(-37, -47),

            /* bishops: bucket 1 */
            S(-15, -23),   S( 26, -42),   S(-22,  26),   S(  8,  -3),   S( -2,  -6),   S( 14,   7),   S( 58, -23),   S( 45, -27),
            S(  6, -29),   S( -5, -14),   S( -4, -12),   S(-10,  -3),   S( 34, -22),   S(  9, -10),   S( 65, -31),   S(  2, -19),
            S(-23,  -7),   S( 34, -19),   S( -8,  -5),   S( 27,  -9),   S(  8, -22),   S( 36, -13),   S( 10, -27),   S( 68, -18),
            S( 27, -22),   S( 41, -29),   S( 23,   0),   S( 36, -15),   S( 69, -14),   S( 18, -13),   S( 74, -27),   S( -1, -11),
            S( -2, -43),   S( 69, -29),   S(  3,   4),   S( 93, -21),   S( 55, -24),   S(100, -47),   S(  6,   4),   S( 33,  -5),
            S( 70, -47),   S( 71, -23),   S( 64, -23),   S(102, -54),   S(106, -45),   S( -7,  -2),   S( 17,  -6),   S(-25, -12),
            S( 11, -81),   S(-17,  -4),   S( -3, -68),   S(  4,  16),   S( 68,  -4),   S( -6,  11),   S( 35, -34),   S(-50,  23),
            S(-11, -46),   S(-26, -22),   S( -7, -56),   S(-41,  -4),   S(-12, -29),   S( 36, -24),   S( 35, -36),   S(-26, -37),

            /* bishops: bucket 2 */
            S( 23, -34),   S(  7, -20),   S(  3,   4),   S(-24,   5),   S( 15,   5),   S(-16,  14),   S( 25, -13),   S( 16,   1),
            S( 17, -26),   S( 11,  -7),   S( -1, -14),   S( 16,  -2),   S(-12,  13),   S( 18,  -6),   S(  6,  -2),   S( 25, -48),
            S( 31,   2),   S( 11,  -5),   S( 10,   6),   S( -4,   3),   S(  4,  15),   S( -1, -11),   S(-13, -17),   S(-10,  -3),
            S(  1,  -8),   S( 47, -19),   S( -3,   6),   S( 37,  15),   S( 12,   3),   S(-15,  26),   S(-18,   1),   S(  1,  -1),
            S( 24,  -8),   S(  3,  11),   S( 62,  -7),   S( 30,   7),   S( 18,  29),   S( 15,  17),   S(  6,  31),   S( 16,  -1),
            S(-28,  20),   S( -7,  20),   S(  1,  -8),   S( 91, -30),   S(104, -34),   S(113,   0),   S( 82,  -7),   S(  8, -30),
            S(-11,  20),   S( 12,   5),   S(-11,  17),   S( 14,  10),   S(-75, -25),   S(-36, -10),   S(-18,   6),   S(-13, -27),
            S(-88, -10),   S(  5, -12),   S(  0,  -1),   S(-51,  28),   S(-34, -32),   S(-26,  -6),   S(-23, -23),   S(-57, -18),

            /* bishops: bucket 3 */
            S( 26,  18),   S( 47, -22),   S(  7,  -3),   S(  6,  13),   S(  6,  29),   S( -2,  46),   S(-12,  55),   S( -7,   9),
            S( 41,   4),   S( 18,  19),   S( 18,  21),   S( 18,  14),   S( 20,  22),   S( 24,  17),   S( 15,  21),   S( 37,  -8),
            S( -4,  16),   S( 25,  49),   S( 16,  44),   S( 22,  40),   S( 15,  43),   S( 17,  35),   S(  4,  13),   S( 15,  39),
            S(-14,  21),   S( 15,  30),   S( 23,  61),   S( 41,  51),   S( 38,  36),   S( 15,  31),   S( 19,  -1),   S( 26, -15),
            S(  8,  23),   S( 13,  52),   S( 15,  48),   S( 59,  57),   S( 41,  52),   S( 49,  34),   S( 15,  37),   S( -8,  41),
            S( 15,  19),   S( 29,  48),   S( 35,  25),   S( 28,  39),   S( 55,  46),   S( 59,  63),   S( 56,  52),   S( 24, 100),
            S(-10,  51),   S( 33,  45),   S( 27,  23),   S(  5,  40),   S(  8,  46),   S( 39,  72),   S(-28,  12),   S( 16, -28),
            S(-41,  57),   S(-39,  52),   S(-42,  36),   S(-46,  52),   S(-41,  44),   S(-105,  63),  S( 27,   7),   S( 31,  20),

            /* bishops: bucket 4 */
            S(-22,  -3),   S(-36, -22),   S(-75,  26),   S(-33, -18),   S(-34, -28),   S( 20, -22),   S(-15, -28),   S(-13, -86),
            S(-15,  11),   S( 13, -25),   S( 56, -40),   S( 12, -27),   S(-13, -16),   S( -6, -34),   S(-17, -20),   S(-30, -36),
            S( 29,  36),   S(-13,  -6),   S( 27, -35),   S(  8, -27),   S( 14, -40),   S(-20,   1),   S(-25, -54),   S(-34,  -7),
            S( 39, -37),   S( 48, -16),   S( 30, -29),   S( -6,   1),   S( 16,   1),   S( 67, -26),   S(-67,  10),   S(  6,  -5),
            S( 13,   5),   S( -2, -66),   S( 31, -41),   S( 79, -45),   S( 13,  -7),   S( 30, -23),   S( -9,  22),   S(-51,  -6),
            S(-57, -80),   S(-24, -61),   S( 14, -18),   S( 14, -25),   S( -6,  -3),   S( 50,  10),   S( -6,  -3),   S(-21,   2),
            S(  1,   9),   S( -5,  -8),   S( 13, -32),   S(-11, -51),   S(-14, -13),   S( 18,  -4),   S(-14,  -7),   S( 36,  27),
            S(  3,  -1),   S( 15, -12),   S(  7, -47),   S( -6, -59),   S(  1, -40),   S( -6, -25),   S( -1,  47),   S(  7,  24),

            /* bishops: bucket 5 */
            S(-67,  12),   S(  0, -20),   S( -7, -12),   S(-35,   4),   S(-17, -20),   S(-44,   5),   S(-29,  -1),   S(-27, -17),
            S(-53,   2),   S(-35,   5),   S( -9,  -3),   S( -4,  -9),   S(-44,   5),   S(-21,  -4),   S(-24,   1),   S( -7, -43),
            S( 19,  -6),   S( -6, -11),   S( 10, -14),   S(-20,   6),   S( 13, -12),   S(-31,   3),   S(-18,  -5),   S( -7,  13),
            S( 27, -11),   S(-34,  -8),   S( 53, -14),   S( 68, -21),   S(-20,  -5),   S( 40, -16),   S(-55,   8),   S(-15,   5),
            S( 26, -36),   S( -8, -11),   S(-14, -21),   S(-15, -39),   S( 39, -33),   S(-11,   2),   S( 30,   2),   S(-58,  15),
            S(-10, -16),   S(-26, -14),   S( 23, -35),   S(-19, -23),   S(-14,  -8),   S(  8, -10),   S(-31,   3),   S(-38,  25),
            S(-17, -13),   S( -7, -30),   S(-13, -12),   S(-14,   6),   S(  3, -27),   S( 27,  -5),   S(  4,  14),   S( -9,   7),
            S(-22, -43),   S(-31, -42),   S(-13,  -9),   S(-10, -26),   S(-45,  16),   S(  5,  10),   S(-36,  -9),   S(  3,  32),

            /* bishops: bucket 6 */
            S(-55,  -9),   S(-12,  -2),   S(-12,   4),   S(-24,   4),   S(-25,   3),   S(-26,  13),   S(-74,  23),   S(-34,  12),
            S(-27,  -3),   S(-15, -24),   S(-69,  27),   S(-20,   8),   S(-41,   9),   S(-49,   9),   S(-50,  29),   S(-40,  -3),
            S( 11,  -8),   S(-36,   8),   S( -3, -14),   S(-22,  11),   S( -8,  13),   S( -5,  -4),   S(-37,  -7),   S(-21,  21),
            S( 11,   0),   S(-55,  -4),   S(-15,  12),   S( 49,  -2),   S( 38,   0),   S( -2,  -3),   S( 11, -17),   S(  4,  -2),
            S(-63,  11),   S( 11,  -1),   S( 41, -18),   S( 82, -21),   S(  9, -20),   S( 14, -16),   S(  2,  -1),   S(-22,  -8),
            S(-55,  22),   S(-14,   4),   S(-32,   1),   S( 11,  -5),   S(  0,  -4),   S(-13, -23),   S(-25,  11),   S(-40, -18),
            S(-30,  12),   S(-65,  27),   S( 27, -20),   S(-17, -16),   S(-22,  -2),   S( -3,   9),   S( 23, -24),   S(-54,  -5),
            S(-23,  35),   S(-22,   6),   S(-19,  10),   S(-22,   7),   S(-13,  11),   S( 13, -24),   S(-13,  -6),   S( -9, -27),

            /* bishops: bucket 7 */
            S( 11, -44),   S(-53, -17),   S(-24, -28),   S( -2, -14),   S(-41,  -5),   S(-30,  -2),   S(-56,  -8),   S(-49,  -3),
            S( 28, -62),   S(-12, -31),   S( 27, -35),   S( -9, -27),   S(-17,   0),   S(-58,  10),   S(-38, -17),   S(-22,   5),
            S(-44, -26),   S(-28,   0),   S( 16, -22),   S( 52, -20),   S( 37, -25),   S(  7, -14),   S(-55,   8),   S(-73,  33),
            S(-33, -23),   S(-45,   9),   S( 10,  -3),   S( 18,  -7),   S(100,  -1),   S( 23, -25),   S( 32, -33),   S(-19, -14),
            S(-18, -14),   S( 16,  -5),   S( 21, -16),   S( 21,  -4),   S(105, -40),   S( 44, -19),   S(-28,  -3),   S(-31, -17),
            S(-93,  19),   S(  6,   6),   S( 33, -14),   S(-32,   6),   S( -8,   0),   S( 52, -21),   S( -7, -12),   S(-62, -70),
            S(-28, -14),   S(-31,  22),   S(-27,  11),   S( 17,   2),   S( 33, -27),   S( 47, -47),   S( 22, -10),   S(-21, -31),
            S(-27,  -9),   S(-40,  -3),   S(-42,  20),   S(-12, -18),   S( 21, -22),   S( -5, -19),   S( 14, -21),   S(  2, -26),

            /* bishops: bucket 8 */
            S( 18,  72),   S( -7,  17),   S(  1, -25),   S( -9,  21),   S(  3,   1),   S(-21, -27),   S(-26, -68),   S( -5, -25),
            S(  7,  23),   S( 12,  49),   S( -4,   4),   S(  1,  -7),   S(  6,  -6),   S(  9,   0),   S(-40, -66),   S( -8, -38),
            S(-17, -57),   S( -5, -56),   S( 17,  43),   S( -4,  -7),   S(  0,  12),   S( 19,  15),   S(-13, -22),   S(-34, -37),
            S( -4,  -7),   S( 21,  87),   S(  9,  14),   S( 19,  59),   S( 22,  20),   S( 15,  30),   S(-15, -14),   S( -4, -25),
            S(  6,  66),   S( 12,  88),   S(  6,  36),   S(  3,  19),   S( -4,  49),   S(-12,  17),   S(  1, -53),   S(  0,  -4),
            S(-10, -44),   S(  1,  39),   S(  3,  31),   S(  4,  16),   S( 11,  19),   S(  5,  41),   S( -2,  32),   S( 10,  43),
            S( -9,  -2),   S(-12, -35),   S( 24,  59),   S( -2,  57),   S(-12,   6),   S(  6,  47),   S( 11,  76),   S(-22,  -8),
            S( -2,   2),   S(  7,  14),   S( -6, -21),   S(  6,  37),   S(  4,  28),   S( 11,  48),   S( -3,  46),   S(  8,  87),

            /* bishops: bucket 9 */
            S(  6,  42),   S(-22,  13),   S(-24,  24),   S(-26, -11),   S( -7,   3),   S( -1, -15),   S(-16, -19),   S( -6,  -4),
            S( -3,  11),   S(-15, -20),   S( -2,  16),   S(-26,  -1),   S(-22, -30),   S(  0, -39),   S(-31, -11),   S(-33, -62),
            S(-16, -40),   S(  8,  29),   S( -1, -13),   S(  5,  12),   S(-18,   8),   S(-12, -11),   S(  8,  11),   S(-14, -18),
            S( -8,  10),   S( -7,  -9),   S( -6,  20),   S( 11,  28),   S(-25,  33),   S(  0,   1),   S(  7,  -7),   S(  3, -20),
            S( 13,  38),   S(-11,  19),   S(  6,   1),   S(-18, -14),   S( -7,  29),   S(-21,  13),   S( -1,  -2),   S(-14, -36),
            S( 13,  63),   S(-29, -12),   S( -9,  28),   S(  5,  20),   S(-34,  11),   S(-10,  23),   S( -5,  24),   S(-10,  16),
            S( -4,  27),   S(-13,  27),   S( -8,   1),   S(-19,  25),   S(  3,  14),   S(  0,  31),   S( -7,  47),   S( -5,  64),
            S(  8,  58),   S( -8,   1),   S( -3,  -8),   S(  4,  22),   S( -5,  31),   S(-21,  12),   S(-10,  40),   S( 17,  76),

            /* bishops: bucket 10 */
            S( -2,   4),   S( -4,  52),   S(-14, -42),   S(-16, -30),   S(-62, -40),   S(-41, -42),   S(-39,   5),   S(-11,   3),
            S(-15, -29),   S(-14, -22),   S(-27, -33),   S(-30, -58),   S(-26, -26),   S(-34, -33),   S(-14, -38),   S( -4,  57),
            S(-20, -38),   S(-42, -40),   S(-21, -19),   S( -9,   2),   S( -2, -18),   S( -1, -33),   S(-20,  -4),   S(  5, -59),
            S(-20, -10),   S(-17, -20),   S(-27, -30),   S( -3,   2),   S( -1,  -1),   S( 10,  -9),   S( -1,  34),   S(-21, -22),
            S(-17, -19),   S(-52,  21),   S( -5, -20),   S(-11,   9),   S(  4,  -8),   S(  1,  29),   S(-18,  -4),   S( 16,  20),
            S(-15,  18),   S(  2,   8),   S(-37,  20),   S(-17,   7),   S(  5,   0),   S( -7,  18),   S(-20,  16),   S( -4,  29),
            S( -3,  28),   S(-24,  13),   S(-14,  17),   S(  8,  32),   S(-14,   9),   S(-36, -33),   S( -7,   4),   S( 13,  60),
            S( -9,  24),   S( -9,  49),   S(  8,  54),   S( -1,  20),   S(-14,  31),   S(-15,  -9),   S( -1,  -7),   S( -9, -13),

            /* bishops: bucket 11 */
            S( 15,   4),   S( -3, -11),   S(-14,  -4),   S(-18, -13),   S(-45, -31),   S( 19, -18),   S(  0,  25),   S(-27,  34),
            S(-21, -17),   S( 25,  -7),   S( -8,   5),   S( -5, -21),   S(  0,  -7),   S(-56, -21),   S(-34,  28),   S( -5,  20),
            S(  0, -34),   S( -9,  -1),   S( 44, -12),   S(-18, -18),   S(-19, -10),   S( 47,  23),   S(  3, -27),   S(-14, -49),
            S( 17,  -6),   S(-14, -37),   S( 23,   6),   S(-33, -30),   S( -2,   4),   S( -6,  46),   S( 17,  50),   S( -6, -36),
            S(-21,  -4),   S(-21, -16),   S(-25,  26),   S(-12,  27),   S(  9,   0),   S( 15,   8),   S(-11,  25),   S( -1,  13),
            S( -8,   9),   S(-12,  12),   S(-17,  50),   S( -1,  12),   S( -8,  60),   S( -5,  29),   S(  0,   8),   S( -4,  -5),
            S(-12,  16),   S( -9,  89),   S(  0,  40),   S( -1,  33),   S( -4,  16),   S( -5,   3),   S(-12, -24),   S( -5,   7),
            S( 16, 121),   S(-17,  10),   S( 19,  65),   S(  3,  29),   S(  7,  48),   S(  1,   9),   S( -7,   0),   S(  4,  27),

            /* bishops: bucket 12 */
            S( -4, -15),   S( -3, -23),   S( -8, -28),   S(  9,   9),   S(-12, -21),   S(-17, -22),   S( -7, -10),   S( -1,  -1),
            S( -2, -11),   S( 11,  29),   S(  9,  13),   S( -7, -19),   S( -2,  -9),   S(  6,   7),   S(-15,   2),   S(  0,   5),
            S(  7,  41),   S( 11,  48),   S( 13,  58),   S(  5,  -5),   S(  7,  27),   S( -4, -40),   S(  5,   5),   S( -4,  -9),
            S(  9,  78),   S( 10,  76),   S(  4,  24),   S(  8,  27),   S( 16,  24),   S( -4, -42),   S(  2,  19),   S(  3,  -1),
            S(  4,   1),   S( 11,  35),   S( -1,  27),   S( 14,  44),   S( 14,  69),   S(  6,  29),   S(  1, -20),   S(  2,   4),
            S(  1,   6),   S( -4, -16),   S(-10,  -1),   S( -1,  13),   S( 15,  65),   S(  8,  25),   S(-11, -10),   S(  0,  -1),
            S( -1,   7),   S(  4,  15),   S(  0,  -4),   S(  8,  28),   S(  7,  31),   S( 15,  69),   S(  6,  38),   S(  0,  37),
            S(  2,  18),   S(  1,   9),   S( -1,   2),   S( -3, -11),   S(  0,   2),   S( -1,  -2),   S(  6,  41),   S(  4,  30),

            /* bishops: bucket 13 */
            S(-15, -46),   S( -1,  11),   S(-18, -58),   S(-11,   3),   S(  9,  46),   S(-21, -63),   S(-13, -19),   S( -4, -30),
            S( -8,  -3),   S( -3,  -5),   S( -1,  14),   S(  8,  58),   S(-10,  -5),   S(  9,   2),   S(  1, -22),   S( -3,  -8),
            S(  2,  18),   S( 18,  71),   S(  5,  15),   S( 18,  31),   S( -2,  22),   S( 18,  40),   S( -5,   4),   S( -6, -23),
            S( 24,  92),   S(  9,  70),   S( -9,  18),   S(-15,   5),   S( 10,  52),   S(  0,  -1),   S( 14,  54),   S(  7,  23),
            S(  5,  58),   S( -3,  24),   S(  8,  12),   S(  4,  48),   S(  6,   3),   S(  2,  44),   S(  8,  27),   S(  1,   7),
            S(  0,  45),   S( -1,   9),   S( -4,  13),   S(  6,  34),   S( -7,  65),   S( -8, -25),   S( -1,   3),   S(  0,  -2),
            S(  5,  25),   S(-10, -33),   S( -8, -16),   S(  4,  27),   S( -2,   1),   S(  6,  67),   S(  5,  16),   S(  8,  65),
            S(  0,  -8),   S( -4,   6),   S( -1, -19),   S(  1,   9),   S(  3,  22),   S( -5,  -7),   S(  8,  52),   S( 11,  42),

            /* bishops: bucket 14 */
            S(-11, -23),   S( -3, -11),   S(  0, -24),   S( -4,  16),   S( -9, -19),   S( -3,   6),   S( -7, -38),   S( -6,  -8),
            S( -3,  -7),   S(  1,  -3),   S(  4,  20),   S(  3,   7),   S( 12,  59),   S(  6,  32),   S( -6,  -8),   S(  6,  21),
            S( -4,   5),   S(-14, -18),   S( -2,  16),   S( 10,  36),   S(  1,  36),   S( 10,  61),   S(  7,  58),   S( -3,  12),
            S(  3,  42),   S(  7,  11),   S(-16,   0),   S(-19,  17),   S( -4,  15),   S( -1,  34),   S(  1,  50),   S( -3,  11),
            S(  4,  22),   S(  1,  37),   S( -5,  10),   S( 10,  51),   S( -9,   5),   S(  3,  43),   S( 20,  44),   S( -2,  35),
            S( -7,  -4),   S(  8,  40),   S(  0,  35),   S( 12,  31),   S(  2,  55),   S(  0,  14),   S(  6,  28),   S(  7,  24),
            S( 13,  75),   S( 14,  70),   S(  5,  50),   S(  0,   5),   S( -3,   5),   S(  9,  43),   S( -6, -43),   S(  1,   8),
            S(  2,  34),   S( 10,  66),   S(  2,   7),   S(  0,  18),   S( -1, -22),   S(  1, -11),   S( 12,  30),   S(  2,   6),

            /* bishops: bucket 15 */
            S(  0,   3),   S( -2,  -5),   S(-10, -27),   S( -6, -24),   S(-17,   1),   S(-10, -28),   S( -8, -43),   S( -3, -21),
            S( 10,  40),   S( -6,   0),   S(  7,  11),   S(  0,  -1),   S(  3, -20),   S( -1,   0),   S(  1,  20),   S(  0,   1),
            S( -5, -13),   S( -1, -15),   S(  1,  19),   S( 17,  15),   S(  6,  26),   S(  8,   9),   S( 15,  62),   S(  4,  48),
            S(  4,  17),   S( 18,  17),   S(  7,  23),   S(-12, -17),   S( -9,  10),   S( 11,  25),   S( 16,  64),   S( -2,  37),
            S( -4,   7),   S( -5, -37),   S(  4,  28),   S( 23,  66),   S(  4,  43),   S( 15,  48),   S(  5,  18),   S( -5,  -9),
            S(  0,  -1),   S(  0,  -1),   S( 25, 100),   S( 14,  38),   S( 15,  47),   S(  5,  55),   S( -1,   9),   S( -2,  -6),
            S(  3,  10),   S(  1,   0),   S(  0,  37),   S( 10,  33),   S(  9,  57),   S( -5,  -5),   S(  5,  30),   S(  0,   2),
            S(  4,  26),   S(  8,  55),   S(  9,  51),   S(  4,   4),   S(  1,  12),   S(  0,   5),   S(  3,  13),   S(  6,  18),

            /* rooks: bucket 0 */
            S(-14,  12),   S(  7,  14),   S(  1,  14),   S( -1,  31),   S(-18,  65),   S( -4,  45),   S(-37,  81),   S(-61,  64),
            S(  0,   3),   S(-12,  46),   S(-40,  48),   S(  5,  20),   S(  3,  49),   S(  3,  39),   S( -5,  -7),   S(-28,  59),
            S( 16,  -8),   S( -7,  20),   S(-25,  42),   S( -8,  20),   S(-42,  89),   S(-17,  45),   S(-27,  59),   S(-23,  55),
            S(-26,  28),   S( 34,  19),   S(-35,  50),   S( 30,  18),   S( 13,  72),   S(-20,  68),   S(-29,  69),   S(-10,  50),
            S( 30, -27),   S( 12,  42),   S(  4,  50),   S( 37,  29),   S( 54,  27),   S( 27,  74),   S( 40,  54),   S(  6,  71),
            S( 10,   6),   S( 73,  58),   S( 58,  31),   S(130,  46),   S( 12,  86),   S( 49,  60),   S(-29, 103),   S(-51, 103),
            S( 70,  52),   S( 70,  84),   S(106,  87),   S( 54,  55),   S( 87,  45),   S( 31,  79),   S( 19,  78),   S( -6,  87),
            S( 10, -18),   S( 15,  13),   S( 20,  59),   S( 42,  16),   S( 91,  52),   S( 77,  31),   S( 25,  37),   S( 53, -12),

            /* rooks: bucket 1 */
            S(-67,  56),   S(-30,  10),   S(-23,  41),   S(-44,  53),   S(-38,  58),   S(-40,  58),   S(-43,  64),   S(-79,  72),
            S(-48,  27),   S(-39,  17),   S(-18,  32),   S(-37,  40),   S(-53,  32),   S(-59,  50),   S(-18,  18),   S(-49,  54),
            S(-47,  37),   S(-22,   7),   S(-31,  30),   S(-54,  55),   S(-36,  41),   S(-55,  36),   S(-56,  56),   S(-44,  64),
            S(-56,  59),   S(-45,  39),   S(-45,  62),   S(-37,  43),   S(-42,  44),   S(-64,  80),   S(-28,  59),   S(-90, 104),
            S(-60,  62),   S(-34,  18),   S(  5,  44),   S( 34,  28),   S(-24,  48),   S(-33,  80),   S(-40,  82),   S(-18,  97),
            S( 62,  32),   S( 69,   0),   S( 65,   6),   S( 19,  45),   S(  9,  34),   S( 12,  76),   S( 23,  50),   S( -3,  90),
            S( 58,  70),   S( 62,  26),   S( 23,  33),   S( 30,  36),   S( 16,  57),   S( 33,  63),   S( 37,  83),   S( 25, 105),
            S( 41,  15),   S( 16,  -5),   S( 14,  -4),   S(  7,  -3),   S( 45,  22),   S( 30,  30),   S( 59,  37),   S( 81,  34),

            /* rooks: bucket 2 */
            S(-66,  82),   S(-54,  74),   S(-44,  73),   S(-36,  49),   S(-27,  55),   S(-41,  49),   S(-22,  24),   S(-79,  75),
            S(-60,  85),   S(-57,  72),   S(-51,  78),   S(-54,  56),   S(-57,  66),   S(-63,  51),   S(-31,  31),   S(-47,  46),
            S(-49,  72),   S(-33,  64),   S(-60,  66),   S(-52,  62),   S(-53,  60),   S(-34,  44),   S(-32,  45),   S(-27,  54),
            S(-46,  92),   S(-51,  86),   S(-58,  91),   S(-89,  90),   S(-58,  73),   S(-54,  57),   S(-29,  51),   S(-29,  61),
            S(-34, 102),   S(-39,  98),   S(-33,  97),   S(-39,  84),   S(-45,  80),   S(  4,  65),   S(-11,  66),   S(-13,  82),
            S(  8, 102),   S(  4,  98),   S( 11,  90),   S(-13,  74),   S( 74,  33),   S( 32,  79),   S(126,  20),   S( 84,  61),
            S( 70,  88),   S(  8, 113),   S( 66,  69),   S( 76,  38),   S( 27,  38),   S( 74,  95),   S(-45, 120),   S( 36, 103),
            S( 46,  57),   S( 49,  68),   S( 53,  52),   S(-16,  47),   S(-35,  66),   S( 47,  49),   S( 49,  37),   S( 33,  58),

            /* rooks: bucket 3 */
            S(-16, 102),   S(-12, 103),   S(-14, 141),   S( -4, 127),   S(  4,  90),   S(  9,  77),   S( 27,  53),   S(-13,  46),
            S( -4, 101),   S(-13, 115),   S(-14, 133),   S( -6, 123),   S(-10,  96),   S(  9,  55),   S( 41,  36),   S( 12,  54),
            S( 11,  96),   S(-13, 112),   S(-24, 130),   S(-16, 134),   S(  0,  94),   S(  5,  77),   S( 26,  76),   S( 23,  50),
            S( -7, 127),   S(-10, 136),   S(-17, 147),   S( -8, 141),   S( -8, 109),   S(-10,  91),   S( 16,  89),   S(-12,  83),
            S( -1, 146),   S(-32, 162),   S( 17, 153),   S( 12, 144),   S(  1, 116),   S( 17, 111),   S( 42, 104),   S( 13, 106),
            S(  5, 156),   S( 20, 144),   S( 27, 152),   S( 39, 133),   S(105,  91),   S(106, 104),   S( 84, 101),   S( 37,  96),
            S(  6, 162),   S( -9, 171),   S( 19, 167),   S( 23, 165),   S( 32, 138),   S(107, 108),   S(106, 182),   S(161, 130),
            S(112,  37),   S( 66,  96),   S( 49, 143),   S( 45, 121),   S( 47, 115),   S( 74,  99),   S( 64,  99),   S(136,  60),

            /* rooks: bucket 4 */
            S(-42,  -5),   S( 35, -43),   S( 37, -32),   S(-30,  -1),   S(-63,  12),   S(-32,  29),   S(-40,  -6),   S(-63,   2),
            S(-14, -54),   S(-55,   1),   S(  9, -36),   S(-13, -29),   S(-13,   2),   S(-19,  -3),   S(-45,   7),   S( 11,   1),
            S(-21, -13),   S(-17, -20),   S(-24,  -9),   S(-11, -47),   S(-57,   7),   S(-87,  27),   S(-35, -20),   S(-37,  -2),
            S(-44, -25),   S( 20,  15),   S( 29, -27),   S(  5, -35),   S( 56, -15),   S(-11,  -1),   S(-16,   6),   S(-26,  16),
            S(-31, -18),   S( 40,  -3),   S( 64,  -7),   S( 60, -29),   S( 49, -10),   S( 60,  20),   S( 20,   9),   S( 23,  26),
            S(-12, -46),   S( 17,  24),   S( 11, -24),   S( 29,  18),   S( 66,  13),   S(  0,  17),   S( 38,   7),   S( 26,  22),
            S( -5, -24),   S( 23,  19),   S( 57, -14),   S( 55,  -7),   S( 46,   0),   S( 13,   1),   S(  6,  -3),   S(  5,  14),
            S( 12, -14),   S( 36,   5),   S( 55, -14),   S( 27, -13),   S( 17,  -5),   S( 17,   4),   S( 11,   4),   S( -4,   7),

            /* rooks: bucket 5 */
            S(-54,  30),   S(-27,   1),   S(  8,   9),   S( 17,  -6),   S(  8,  10),   S(-15,  25),   S(-18,  34),   S(-59,  56),
            S(-29,   0),   S(-20, -17),   S(-25,  -5),   S(  0, -14),   S( -3,  -6),   S(-23,  -1),   S(-49,  18),   S(-20,  29),
            S(-58,  17),   S(-30, -18),   S(-10, -13),   S( -4, -14),   S(-12,  -8),   S( 34, -15),   S(-53,  33),   S(-52,   9),
            S(-75,  31),   S(-18,   7),   S( 50, -23),   S( 60,   3),   S( 21,  -1),   S(-44,  43),   S( -1,  22),   S(-15,  36),
            S( 34,   6),   S( -7,  21),   S( 24,  33),   S( 20, -22),   S(-34,  29),   S( 90,  -8),   S( 15,  23),   S( 57,  30),
            S( 53,  -1),   S(  8,   7),   S(  1,   0),   S( -1,  -1),   S( 22,  28),   S( 56,  23),   S( 85,  11),   S( 43,  42),
            S( 55, -17),   S( 35, -19),   S( 14,   6),   S( 35,  11),   S( 73, -19),   S( 51, -23),   S( 77, -15),   S( 64,  -2),
            S( 27,  16),   S( 29,   2),   S( 61,   1),   S( -4,  29),   S( 79,  -3),   S( 36,  20),   S( 23,  31),   S( 17,  49),

            /* rooks: bucket 6 */
            S(-64,  38),   S(-47,  25),   S(-33,  24),   S(-16,  11),   S(  1,   3),   S(-11,   7),   S(-14,  -7),   S(-41,   8),
            S(-85,  47),   S( 15,  -5),   S(-29,  16),   S(-14,   1),   S( -7,  -9),   S(-45,   3),   S(-39,  -5),   S(-22,   4),
            S(-106,  46),  S(-37,  23),   S(-11,   7),   S(-12,  -2),   S(-17,   8),   S(-12,   8),   S(-20, -19),   S( -1, -24),
            S(-52,  42),   S(-31,  28),   S(-17,  21),   S( 36,  -2),   S( 34,   3),   S(  6,  13),   S(-35,  18),   S( 21,  13),
            S( -8,  51),   S( 41,  12),   S(127,  -1),   S( 83,  -9),   S( 44,  -8),   S( 30,  26),   S( 15,  12),   S( 58,   4),
            S( 85,  12),   S( 94,  -2),   S( 89,   8),   S( 60,  -8),   S( -4, -38),   S( 21,  24),   S( 44,  -4),   S( 94,  10),
            S( 14,  16),   S(104, -13),   S( 98, -24),   S( 84, -28),   S( 39, -12),   S( 26,  10),   S( 62, -25),   S( 88, -30),
            S( 41,   4),   S( -6,  42),   S( 16,  21),   S( 96, -21),   S( 22,  19),   S( 37,  29),   S( 57,   3),   S( 34,  10),

            /* rooks: bucket 7 */
            S(-103,  41),  S(-76,  29),   S(-68,  46),   S(-72,  53),   S(-48,  21),   S(-38,   1),   S(-56,  24),   S(-47,  -9),
            S(-100,  43),  S(-58,  25),   S(-57,  29),   S(-89,  45),   S(-46,  -6),   S(-19,  -7),   S(-18,  18),   S( -6, -41),
            S(-94,  31),   S(-74,  24),   S(-46,  17),   S(-77,  43),   S(-36,  -1),   S(-51,  25),   S( 28,  -6),   S(-18, -18),
            S(-63,  29),   S(-10,   5),   S(  5,   2),   S( 67, -23),   S( -5,   0),   S( 71, -22),   S( 28,  32),   S(  1,  -6),
            S(  4,  29),   S( 12,  24),   S( 39,  21),   S( 35,  18),   S(127, -39),   S(122, -35),   S(120, -25),   S(-48, -12),
            S( 10,  36),   S( 13,  22),   S(120,   1),   S(102, -18),   S(108, -13),   S( 51,  14),   S( 25,  32),   S( -8,  -8),
            S(-17,  17),   S( 26,  -2),   S( 64,  -5),   S( 92, -23),   S( 88, -20),   S(105, -23),   S( 59,   0),   S( 45, -34),
            S(-51,  11),   S(  2,  18),   S( 26,  15),   S( 31,   4),   S( 29,  18),   S( 55,   4),   S( 82,  12),   S( 40,  -9),

            /* rooks: bucket 8 */
            S( -2, -63),   S( 38, -74),   S( 23, -36),   S( 24, -20),   S(-10, -54),   S(-14, -25),   S( 11, -42),   S( -1, -36),
            S(-26, -104),  S(-17, -48),   S( 18, -21),   S(-40, -63),   S(  3, -65),   S( -1, -17),   S( 19, -14),   S(-38, -40),
            S( 10,  14),   S( 16,   6),   S( 14,  12),   S( -6, -16),   S( 12,  46),   S( 20,  13),   S(  2,  33),   S( -8,   1),
            S( -6, -28),   S( -1, -12),   S( -2, -30),   S( 10, -24),   S( 18,  15),   S( 16,  -2),   S( 11,  12),   S(-10, -20),
            S(  6, -42),   S( 24,   2),   S( 18, -31),   S( 10, -10),   S(  5, -11),   S(  3,  -9),   S( -5,  18),   S( 12,  16),
            S( -2, -11),   S(  8, -19),   S(  4, -24),   S(  0, -24),   S(  7,  27),   S( -4,   2),   S(  3,  15),   S(  9,  20),
            S( 10,  -1),   S( 38, -11),   S(  2, -15),   S( 21,   6),   S(  4,  12),   S(  8,  33),   S(  8,  12),   S(  4,  19),
            S( -1,  20),   S(  9,   5),   S( 16, -17),   S( 38,  55),   S(-12, -15),   S(  1,  22),   S( -6,   0),   S(  3,  17),

            /* rooks: bucket 9 */
            S(-26, -106),  S( 14, -108),  S( 14, -109),  S( 15, -68),   S( 34, -98),   S( -4, -73),   S( -6, -52),   S( -5, -83),
            S(-29, -90),   S( -9, -81),   S(  0, -68),   S(-13, -68),   S( -7, -81),   S(-29,  -4),   S(-30, -56),   S(-19, -75),
            S( -8, -27),   S( -1, -26),   S( 22,  13),   S(-12, -28),   S( 29, -51),   S( 22,   2),   S( -9, -19),   S(-17,  -5),
            S( 15, -52),   S(  6,   3),   S( 10,  11),   S( -7, -26),   S(-13, -67),   S( 18,  -5),   S( -9, -49),   S( -5, -45),
            S( 33, -42),   S(  9, -23),   S(-13, -60),   S( -6, -20),   S(-15, -60),   S(-25, -21),   S(  1, -55),   S(-16, -58),
            S(  4, -49),   S(-33, -67),   S( -2, -48),   S( 18, -29),   S( 20, -29),   S( -7, -21),   S( -9, -44),   S(-19, -35),
            S( 11, -22),   S(  4, -34),   S(  9, -45),   S( -1,  -7),   S( 15, -52),   S(  6, -38),   S( 11,  -9),   S(-14, -49),
            S( -7, -18),   S( -7, -19),   S( -5, -20),   S( -1,  -5),   S( 11, -17),   S( 10,   7),   S( -6, -13),   S(  1,  -1),

            /* rooks: bucket 10 */
            S( -2, -89),   S(-31, -76),   S(  9, -94),   S( 11, -102),  S( 19, -91),   S( 17, -103),  S(  5, -115),  S(-24, -87),
            S(-28, -56),   S(-42, -68),   S(-39, -50),   S(-33, -89),   S( -9, -79),   S(-22, -70),   S(  5, -48),   S(-46, -94),
            S(-12, -39),   S(-29, -28),   S(-28, -68),   S(-42, -73),   S(-10, -41),   S(  7, -21),   S( 16, -40),   S(-14, -29),
            S(-27, -44),   S(-19, -64),   S(-21, -74),   S( -6, -17),   S( 12,  21),   S(  5,  20),   S(-15, -89),   S(  1, -58),
            S( 10, -52),   S( 27, -32),   S(  8, -51),   S(  1, -57),   S( 16, -15),   S(  0, -25),   S(  6, -66),   S(-18, -84),
            S(-18, -25),   S(  3, -45),   S( -8, -58),   S( -6, -76),   S( 19, -23),   S( -3, -46),   S(-19, -70),   S( -9, -54),
            S(-37, -45),   S( -1, -68),   S(  8, -58),   S( -9, -44),   S( 21, -35),   S( -4, -35),   S(-17, -77),   S(-15, -67),
            S(-21, -35),   S(  6,   0),   S( -2,  10),   S(-14, -33),   S( -7,  12),   S( -5, -36),   S( -6, -52),   S(-27, -12),

            /* rooks: bucket 11 */
            S(-14, -69),   S( -2, -46),   S( -2, -42),   S( 15, -38),   S(-25, -37),   S( 48, -71),   S(  4, -53),   S(-24, -69),
            S(-10, -23),   S(-18, -39),   S(-41, -37),   S(-21, -24),   S(-17, -14),   S(  2, -31),   S(-13, -48),   S(-44, -99),
            S(-41, -11),   S(-27,  -1),   S( 13,  16),   S( -6, -12),   S( 13, -19),   S( -4,  -9),   S( 11, -35),   S( -5, -12),
            S( -1, -34),   S(  1, -23),   S(  5,  -7),   S( 22,   9),   S( 31, -10),   S(-12, -65),   S(  9,  16),   S( -5, -22),
            S(-20, -41),   S( 17, -41),   S(  9, -26),   S( 15, -10),   S( 22, -15),   S( 27, -40),   S( 26,   7),   S( -8, -66),
            S(-30, -40),   S(-12, -26),   S( 18, -12),   S( 11, -21),   S(-13, -29),   S( 39,  -8),   S( 26,  -5),   S(  6, -46),
            S(-22, -15),   S(-14, -55),   S( -5, -12),   S( -3, -15),   S( 32,  -8),   S( 38, -17),   S( 26, -25),   S(-24, -17),
            S( -7, -14),   S( 23,  10),   S(  0,  10),   S( 28,  24),   S( -6,   9),   S(  4,  -4),   S( 57, -25),   S( -6,   1),

            /* rooks: bucket 12 */
            S(-20, -78),   S( -4, -14),   S(  1, -37),   S(-24, -81),   S(  1, -28),   S(  8,  -5),   S(-14, -49),   S(-17, -59),
            S(  2, -21),   S(  2,   1),   S( 18,  25),   S( 20,  18),   S( 15,  -4),   S( 11, -11),   S( 14,  14),   S(  2, -27),
            S(-12, -32),   S(  6,  20),   S(  5,   8),   S( 13,   1),   S(  0, -12),   S( 12,  -7),   S( 10,  23),   S( -2, -13),
            S(  3,   2),   S(  2,  -1),   S( 18,  29),   S(  8,  -6),   S(  7,  -3),   S(  1,  -5),   S(  8,  19),   S( -7, -17),
            S( -3, -36),   S(  7,   4),   S( -2, -18),   S( -2, -31),   S( 16,  43),   S( -4, -10),   S( -9, -37),   S(  0, -14),
            S( -3, -19),   S(  3, -16),   S(  7, -26),   S( -4, -27),   S(  9,   1),   S(-19, -52),   S( 11,   6),   S( -2,  -9),
            S(-21, -21),   S( -5, -21),   S(  7,  -5),   S(  2,   2),   S(  8, -15),   S(  9,  -2),   S(  0,  13),   S( -6, -17),
            S(-12, -44),   S( -6, -24),   S(  2, -47),   S(  5,   0),   S(  2, -13),   S(  1,  -1),   S( -5, -15),   S( -1,  18),

            /* rooks: bucket 13 */
            S(-17, -69),   S(-10, -61),   S(-18, -56),   S(-13, -38),   S(-22, -96),   S( 12, -42),   S(-31, -65),   S(-23, -59),
            S( -8, -27),   S(  1, -28),   S(  1,  -2),   S(  0, -20),   S( 18,  16),   S( -2, -35),   S( 16, -40),   S(-11, -61),
            S( -1, -19),   S( -1, -42),   S( -5, -12),   S(  5, -25),   S( 12,   2),   S(  2, -18),   S( 12, -14),   S( -9, -96),
            S(  7, -22),   S(  1,  -6),   S(  1, -28),   S(  8, -32),   S( 10, -20),   S( -5, -28),   S(  0, -12),   S(  4,   7),
            S( 10,  -3),   S( -6, -82),   S(  1, -46),   S(  8,  -8),   S( 16, -40),   S(  0,  -8),   S(  3,  11),   S(  3,   7),
            S(  8, -16),   S( -5, -43),   S(-11, -58),   S(  5, -38),   S(-16, -107),  S( -2, -31),   S( -7, -22),   S( -9, -32),
            S( 10,  -4),   S( 11, -22),   S( -8, -59),   S( 15,  -1),   S(  2, -54),   S( -5, -43),   S( -2, -15),   S(  2,   3),
            S(  3, -22),   S(-16, -55),   S(-12, -43),   S( 18,  -7),   S( -4, -37),   S( -1, -20),   S(  6,   9),   S(  2,  -8),

            /* rooks: bucket 14 */
            S( -1, -39),   S(-39, -61),   S(-21, -46),   S(-23, -84),   S(-15, -54),   S( 13, -17),   S(-39, -116),  S( -7, -52),
            S(  8, -20),   S( -1, -41),   S(  0, -58),   S( -6, -39),   S( -1, -17),   S(  3,   3),   S( -4, -43),   S( -8, -64),
            S( -1, -21),   S(  4, -22),   S(  4, -41),   S(  0, -37),   S(  5, -19),   S( -2, -23),   S( 14, -33),   S(-17, -56),
            S(  6,   2),   S( 10,  13),   S(  4,   2),   S( -3, -31),   S( -8, -38),   S( -4, -32),   S(  4, -35),   S(-18, -42),
            S( -1,  -3),   S( 17,  32),   S( 13,   6),   S( -2, -45),   S(  4,  -4),   S( 15, -14),   S( -6, -44),   S(-10, -27),
            S(  6, -10),   S( -2,  -6),   S( -1, -42),   S( 13, -87),   S(  2, -61),   S( -4, -72),   S(-14, -89),   S( -3, -32),
            S(-13, -30),   S( -8, -24),   S( -9, -43),   S(-28, -101),  S( -1, -30),   S(  4, -23),   S(-17, -67),   S( -5, -41),
            S( -9, -37),   S( -8, -28),   S(-14, -59),   S(  4, -33),   S(-18, -63),   S(-21, -104),  S( -2, -58),   S( -5, -13),

            /* rooks: bucket 15 */
            S(-15, -78),   S(-13, -50),   S(-35, -67),   S(-17, -58),   S(-21, -52),   S(-16, -64),   S(  4, -20),   S( -9, -39),
            S(  6,  -8),   S(  1, -29),   S( -1, -34),   S( -3, -39),   S(-15, -65),   S(  2, -12),   S(  3,  -1),   S(  2,  -8),
            S(  3,  14),   S( -4, -57),   S(  0, -11),   S( 13,  -5),   S(  7, -37),   S(-14, -61),   S( 10,  14),   S(  2, -11),
            S( -5, -11),   S( -6,   2),   S( 13,  25),   S(-11, -21),   S(  6,   7),   S( -5, -14),   S(  9, -10),   S(  3, -26),
            S(  9,   9),   S( -4, -28),   S(  2, -11),   S(  1,   7),   S(  8,   9),   S(  8, -22),   S( 10, -13),   S(-13, -64),
            S(  1,  -4),   S(-12, -27),   S(  2,   1),   S(  2,  -5),   S( -6, -34),   S( -3, -71),   S( 17, -11),   S(-14, -23),
            S(  0, -20),   S( -2,  -2),   S(  0,  -8),   S( -2, -19),   S( -2, -11),   S( 11, -33),   S(  1, -46),   S(-16, -63),
            S(  6,  19),   S(  9,  35),   S(  6,  28),   S(  3,  12),   S( -9, -33),   S( -5, -55),   S( -1, -38),   S(-10, -39),

            /* queens: bucket 0 */
            S(-20, -13),   S(-30, -38),   S(-36, -90),   S( 11, -128),  S(-11, -74),   S( -8, -77),   S(-35, -38),   S( -5, -27),
            S(-25, -31),   S( 24, -95),   S(  9, -68),   S(-13, -32),   S(  6, -73),   S( -5, -66),   S(-31, -84),   S(-49, -10),
            S( -6,  24),   S( -2, -34),   S( 18, -63),   S(-20, -25),   S(-14,  -8),   S(  5, -36),   S(-23,  -5),   S(-82, -70),
            S(-49,  73),   S(  9, -23),   S(-21,  41),   S(-35,  50),   S(-13,  54),   S(-29,   9),   S(-45,   8),   S( -8, -67),
            S(-65,  41),   S(-46,  75),   S( -8,  56),   S(-27,  85),   S(-16,  82),   S(-30,  63),   S(-48,  38),   S(-33, -28),
            S(-14,  22),   S( 30,  85),   S( 28,  62),   S(-21,  95),   S(-40,  77),   S(-63,  68),   S(-93,   3),   S(-55,  -2),
            S(  0,   0),   S(  0,   0),   S( 53,  45),   S(-17,  43),   S(-35,  27),   S(-75,  88),   S(-113,  76),  S(-93,  12),
            S(  0,   0),   S(  0,   0),   S( 42,  20),   S( 13,   2),   S(-45,  44),   S(-31,  21),   S(-84,  15),   S(-42,  -9),

            /* queens: bucket 1 */
            S(  1, -49),   S(  2, -36),   S( -3, -79),   S( 26, -108),  S( 33, -76),   S(  2, -40),   S( 19, -26),   S( -9,  16),
            S(-37,  27),   S( 20,   6),   S( 35, -52),   S( 15, -12),   S( 32, -22),   S( -6, -12),   S(-27,   5),   S(-20, -26),
            S( 30, -33),   S(  9, -25),   S(  7,  -2),   S( 19,   0),   S( -9,  21),   S( 17,  36),   S( -9,  11),   S( 17, -17),
            S( 30, -12),   S( -2,  40),   S( 13,  21),   S( 35,  44),   S(  7,  75),   S( -1,  27),   S( 10,  32),   S(-26,  46),
            S(  4,   9),   S( -3,  79),   S( 13,  97),   S( -1,  62),   S(  4, 119),   S( 47,  37),   S(-13,  56),   S( -2,  26),
            S( 46,  20),   S( 77,  67),   S( 96,  76),   S( 83,  98),   S( 79,  85),   S(-18,  97),   S( 15,  78),   S(-10,  18),
            S( 82,  -4),   S( 55,  10),   S(  0,   0),   S(  0,   0),   S( -9,  78),   S(-46,  82),   S( -8,  74),   S(-44,  44),
            S( 71,  26),   S( 71,  18),   S(  0,   0),   S(  0,   0),   S( 44,  42),   S( 31,  30),   S( 98,  24),   S(-37,  37),

            /* queens: bucket 2 */
            S( 27, -13),   S( 27, -32),   S( 17,   7),   S( 44, -36),   S( 53, -75),   S( 31, -80),   S( -2, -30),   S(  3,  21),
            S( 25,  -7),   S(  8,  42),   S( 39, -27),   S( 39,   1),   S( 51, -24),   S( 21, -15),   S( 12,   4),   S(  1,  29),
            S(  7,  67),   S( 20,  58),   S( 13,  72),   S( 14,  43),   S( 28,  33),   S( 25,  44),   S( 34,  17),   S( 38,  21),
            S( 25,  22),   S(  6,  80),   S( 17,  77),   S(  2, 109),   S( 28,  88),   S(  8,  77),   S( 11,  82),   S( 21,  66),
            S(-10,  75),   S( 12,  38),   S( -2, 105),   S( -7, 150),   S( 31, 141),   S( 77,  74),   S( 48, 111),   S( 57,  51),
            S(-35,  95),   S(-34, 114),   S(  3, 100),   S( 76,  94),   S( 60,  96),   S(109, 103),   S(123,  78),   S( 28, 149),
            S( -2,  63),   S(-23, 100),   S( -7, 111),   S( 84,  66),   S(  0,   0),   S(  0,   0),   S(  5, 137),   S( 50,  98),
            S( -1,  46),   S(-13,  49),   S( 75,  23),   S( 45,  67),   S(  0,   0),   S(  0,   0),   S( 68,  85),   S( 58,  75),

            /* queens: bucket 3 */
            S(-14,  43),   S(-15,  37),   S(-13,  37),   S( 13,  42),   S( -1,  25),   S( -8,  -7),   S( 13, -68),   S(-28,  30),
            S(-32,  49),   S(-15,  64),   S( -2,  59),   S(  1,  72),   S(  7,  55),   S(  3,  28),   S( 29,  -9),   S( 49, -65),
            S(-30,  59),   S(-23,  96),   S(-15, 113),   S(-25, 135),   S(-13, 105),   S( -4, 107),   S( 10,  69),   S(  8,  36),
            S(-24,  63),   S(-47, 121),   S(-27, 161),   S(-19, 169),   S(-18, 165),   S(-18, 119),   S( -5, 117),   S(-18,  93),
            S(-23,  91),   S(-28, 138),   S(-25, 143),   S(-22, 181),   S( -6, 161),   S( -9, 195),   S(-25, 181),   S(-25, 139),
            S(-55, 130),   S(-32, 138),   S(-55, 178),   S(-40, 190),   S(-26, 200),   S( 15, 183),   S(-13, 200),   S( -7, 190),
            S(-72, 126),   S(-91, 170),   S(-51, 177),   S(-80, 211),   S(-73, 211),   S( 23, 114),   S(  0,   0),   S(  0,   0),
            S(-122, 180),  S(-102, 159),  S(-70, 146),   S(-59, 156),   S(-47, 168),   S(-34, 122),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-40,   0),   S(-46, -38),   S( 32,  32),   S( -5, -25),   S(-11, -14),   S( -5,   8),   S(-46, -43),   S( 33,  14),
            S( -5,  -7),   S(-25,   7),   S( -2, -43),   S(-49, -17),   S(-57,  10),   S(-28,  33),   S(-32, -18),   S( 11, -13),
            S( 12,  37),   S( 33, -20),   S(  4,  11),   S(  2,   6),   S(  8,   9),   S(  9,   2),   S(-34, -37),   S( 33,  12),
            S( 11,   9),   S( 32,   7),   S(  5,  13),   S( 14,  25),   S( 23,  37),   S(-19,  24),   S(-46, -22),   S(-24,   2),
            S(  0,   0),   S(  0,   0),   S( 49,  35),   S( 52,  56),   S( 31,  48),   S(-11,   9),   S(-41, -39),   S( 13,  29),
            S(  0,   0),   S(  0,   0),   S( 42,  34),   S( 44,  42),   S(  6,  30),   S(  6,  25),   S(  9,  32),   S(-23,  -7),
            S( 10, -12),   S( 29,  34),   S( 59,  38),   S( 50,  76),   S( 66,  26),   S(  3,  31),   S( 17,  -2),   S(-28,   3),
            S( 57,  38),   S( -6,  -1),   S( 42,  21),   S( 61,  57),   S( -5, -25),   S(-27, -12),   S(-37, -46),   S( -8, -17),

            /* queens: bucket 5 */
            S( 33,   2),   S(  7, -21),   S(-25, -10),   S(-49,  11),   S( 12, -31),   S( 34,  35),   S( 10,   6),   S( 13, -16),
            S(  7,  -9),   S(-11,  -2),   S(-16,  -8),   S(-20,  -6),   S(-31,  21),   S( -4, -15),   S( 18,   8),   S( 17,   3),
            S( 29,  21),   S( 61,   9),   S( 25,   9),   S(-15,  30),   S( 12,  31),   S(-12,  30),   S(-22,  30),   S( 22,  23),
            S( -4, -33),   S( 43,  31),   S( 30,  -6),   S( 45,  34),   S( 75,  35),   S( 22,  19),   S( 32,  32),   S(  6,  26),
            S( 36,   7),   S( 30,   6),   S(  0,   0),   S(  0,   0),   S( 17,  20),   S( 12,  16),   S( 33,  28),   S(-19,  10),
            S( 23,  27),   S( 41,  33),   S(  0,   0),   S(  0,   0),   S( 19,   7),   S(108,  58),   S( 37,   1),   S( 49,  40),
            S( 81,  13),   S( 72,  21),   S( 59,  85),   S( 32,  45),   S( 63,  66),   S( 88,  81),   S( 56,  51),   S( 19,  -3),
            S( 20,  33),   S( 50,  33),   S( 65,  32),   S( 53,  33),   S( 56,  34),   S( 65,  36),   S( 20,  15),   S( 27,   2),

            /* queens: bucket 6 */
            S( 24,  12),   S(-21, -15),   S( 23,  -8),   S( -1,   3),   S( -9,  -5),   S(  7, -23),   S(-37, -26),   S(  0,   1),
            S( 11, -21),   S( 37,  -1),   S( 38,  22),   S( 41,  11),   S( -3,  18),   S( 16,  30),   S(-44,  26),   S(  5,  29),
            S(-52,  10),   S(  2,  35),   S(-11,  28),   S( 33,   3),   S( 13,  26),   S( 42,   2),   S( 43,  15),   S( 39,  30),
            S(-14,   0),   S( -7,  37),   S( 34,  22),   S( 89,  32),   S( 30,  10),   S( 55,  31),   S( 76,   3),   S(104,  14),
            S( 21,  31),   S( -8,  38),   S( 24,  47),   S( 61,  67),   S(  0,   0),   S(  0,   0),   S( 70,  42),   S(111,  61),
            S( 26,  29),   S( 67,  29),   S( 39,  66),   S( 19,  35),   S(  0,   0),   S(  0,   0),   S(105,  65),   S( 97,  28),
            S( 56,  13),   S( 29,  27),   S( 75,  39),   S( 70,  49),   S( 48,  71),   S( 94,  61),   S(107,  41),   S(147,  40),
            S( -1,  -2),   S( 46, -10),   S( 53,  18),   S( 80,  46),   S(116,  48),   S(101,  60),   S(130,  62),   S( 85,   0),

            /* queens: bucket 7 */
            S(-42, -13),   S( -2, -44),   S(-12, -27),   S(-15, -10),   S(-14,   0),   S(-66,  17),   S(-27,  -3),   S(-31, -37),
            S(-41,  -3),   S(-65,   9),   S(-33,  57),   S(-21,  32),   S(-32,  14),   S(-10,  18),   S(-19,  16),   S(-44, -25),
            S(-22, -19),   S(-43,  -1),   S(-18,  49),   S( 17,  37),   S( 36,  -3),   S(  6,  18),   S( 38, -17),   S( 31,  -9),
            S(-40,  -9),   S(  1, -29),   S(  7,  43),   S( 20,  42),   S(101,   5),   S( 74,   6),   S( 84, -14),   S( 46,  10),
            S( 11, -20),   S(-19,   0),   S( -3,  69),   S( 37,  34),   S( 56,  38),   S( 49,  13),   S(  0,   0),   S(  0,   0),
            S( 18, -23),   S(-37,  51),   S(-21,  56),   S(-10,  57),   S( 55,  15),   S( 97,  50),   S(  0,   0),   S(  0,   0),
            S(-46,  46),   S(-25, -12),   S(-30,  45),   S( 80,  20),   S( 74,  39),   S(124,  33),   S( 96,  60),   S( 75,  24),
            S( 19, -30),   S( 42, -26),   S( 17,  28),   S( 51, -10),   S( 36,  48),   S( 47,   2),   S( 22,  19),   S( 92,  11),

            /* queens: bucket 8 */
            S(-10, -30),   S(  9,  -4),   S(  6,  -5),   S(-10,  -6),   S(  9,  -7),   S( 32,  23),   S( -8, -15),   S( -2, -10),
            S(-16, -30),   S( -5,   4),   S( 11,  -2),   S( -1,   9),   S( -1,   4),   S(  2,  -1),   S( -6,   5),   S( -2,  -6),
            S(  0,   0),   S(  0,   0),   S(  3, -10),   S(  6,  -5),   S( 12, -11),   S(  6,  13),   S(  7,   3),   S( -2,   1),
            S(  0,   0),   S(  0,   0),   S(  9,  10),   S(  9,  15),   S( -3, -10),   S(-13, -23),   S(  6,  24),   S( -1,  -2),
            S(  3,  -3),   S(  3,  14),   S(  8,   9),   S( 53,  40),   S(  2,  -7),   S( -3, -13),   S( 13,   0),   S(-11, -24),
            S(  0, -12),   S( 19,   8),   S( 26,  38),   S(-12, -36),   S( -3,  -3),   S( 19,  24),   S(  7,  -6),   S(  2,  -5),
            S(-16, -28),   S( -2, -19),   S( 11,   8),   S( 31,  33),   S( 15,  -3),   S(  2,   1),   S(  6, -10),   S(  7,   0),
            S(  8,  -6),   S( -3, -18),   S( 18,  10),   S(  7,   0),   S(  5,  -9),   S(-14, -21),   S( -8,  -9),   S(-32, -55),

            /* queens: bucket 9 */
            S( 19,   8),   S( -8, -40),   S( -2, -14),   S( 13,  -6),   S(  9,   5),   S( -9, -28),   S( -4, -11),   S( -2, -18),
            S( 26,  11),   S(  9,  -5),   S( -9, -20),   S( 18,  20),   S(-16, -38),   S( -2, -17),   S(  6,  11),   S(  6,   4),
            S( -7, -28),   S(  2,  -6),   S(  0,   0),   S(  0,   0),   S(  8,   8),   S( 12,  -3),   S( -6, -17),   S(  2,  -1),
            S( 25,  13),   S(-15, -29),   S(  0,   0),   S(  0,   0),   S( 10,  11),   S( 34,  34),   S( 24,  29),   S( -7,  -3),
            S( 14,  -5),   S( 20,  16),   S(  2,  -4),   S(-19, -37),   S(-10, -23),   S( 32,  37),   S(  0, -23),   S( -5, -26),
            S( 17,  26),   S(  4, -13),   S( 26,  16),   S(  9,  -7),   S( -5, -27),   S( -4,  -5),   S(-14, -34),   S(-17, -34),
            S(-10,   1),   S( 17, -12),   S( -3,   1),   S( -5,  -4),   S( 24,  15),   S( 33,  32),   S( -6,   4),   S( 12, -10),
            S( -4, -20),   S( 24,  15),   S(-18, -37),   S( 23,  24),   S(  9,  -2),   S(  3,   2),   S(-12, -46),   S( -2, -22),

            /* queens: bucket 10 */
            S( 13,  21),   S(  7,  -1),   S( 16,  17),   S(  2, -16),   S( -6, -31),   S( 11,  10),   S(  3, -12),   S(-16, -36),
            S( 12,   5),   S(-15, -23),   S( 25,   3),   S(-10, -30),   S( -5, -12),   S( 10,   8),   S(  2, -17),   S(  1,  -8),
            S( -1,   1),   S( 14,  19),   S(  8,  -8),   S(  9,   3),   S(  0,   0),   S(  0,   0),   S( 17,  28),   S(-14, -25),
            S( -5, -17),   S( 16,   5),   S( 16,   8),   S(  3,  -5),   S(  0,   0),   S(  0,   0),   S(  7,  -6),   S(  4, -18),
            S(  0,  -8),   S( 22,   3),   S(  1,  -9),   S( 42,  41),   S( 14,  17),   S(  2,   1),   S(  0,  -2),   S( 32,  14),
            S( -5, -12),   S( 10,  20),   S( 38,  59),   S( 15,  32),   S( 23,  18),   S( 10,  13),   S(  8,  -6),   S(-11, -48),
            S( 13,   6),   S( -3,  -2),   S( 23,  44),   S( 19,  22),   S( 15,  10),   S( 27,  14),   S(  8, -24),   S(  1, -12),
            S(-30, -53),   S( -9,  -9),   S( 14,  -8),   S(-19, -39),   S( 13,  15),   S(  6,   1),   S( -9, -28),   S( 10,   4),

            /* queens: bucket 11 */
            S( -8,  -6),   S(-15, -11),   S(  2, -12),   S( -8, -29),   S(  0,  20),   S(  9, -22),   S( -6, -17),   S(-10, -27),
            S( -7,   3),   S(-21, -31),   S(-28, -37),   S(  9,  -7),   S( 54,  -2),   S( 12,   2),   S( 31,  22),   S( 11,  10),
            S(  6,   9),   S( -5, -13),   S(-27, -26),   S( -4, -22),   S( -7, -23),   S(-33, -43),   S(  0,   0),   S(  0,   0),
            S(-19, -23),   S( -7,  -4),   S(  1,   6),   S( 12,   8),   S(-11, -26),   S(  2,  12),   S(  0,   0),   S(  0,   0),
            S( -5,  -9),   S( 23,  30),   S( 27,  38),   S( 12,   9),   S( 56,  72),   S( 26,  45),   S( 12,   4),   S( -4, -10),
            S(-25, -30),   S(-15, -19),   S( -2,  -2),   S( 22,  33),   S( 13,  12),   S( 18,   5),   S( 26,  21),   S( -4, -33),
            S(-21, -18),   S(  5,  15),   S( -7,  -9),   S( -7, -17),   S( 23,  35),   S( 13,   3),   S(-10, -15),   S(-11, -31),
            S(-29, -72),   S( -1,   1),   S(-30, -45),   S( -7,  -3),   S( 21,  14),   S(  3,  -9),   S( -6, -15),   S( 15,  -5),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 12,  20),   S( -1, -15),   S( -8,   5),   S( -6, -17),   S( -1,   0),   S( -1,  -5),
            S(  0,   0),   S(  0,   0),   S( 14,  18),   S(  0, -18),   S(  1, -12),   S(-18, -42),   S( -2,  -7),   S( -1,  -6),
            S( -5, -15),   S(  6,   8),   S( -7, -26),   S(  0, -30),   S( 29,  52),   S(  6,  27),   S( -7, -16),   S(  3,   5),
            S(  9,  -1),   S(  3,  -8),   S( -2,  -8),   S(  7,  13),   S( -1, -22),   S(  4,   4),   S( -1,  -4),   S( -3,  -7),
            S(  0,  -5),   S( -1,  -9),   S(  0, -16),   S(-13, -41),   S(  8,  23),   S(-12, -48),   S(  4,  -2),   S(-12, -24),
            S(  1,   2),   S( -5, -12),   S(-19, -35),   S( -3,  -7),   S(-10, -22),   S( -3,  17),   S(-18, -18),   S( -6, -16),
            S(-25, -47),   S(  4,  12),   S(-10, -22),   S( -6, -17),   S( -7, -11),   S(-14, -36),   S( -1,   3),   S( -6, -24),
            S( 11,  19),   S(  2,   3),   S(  2,  -3),   S(  2,   5),   S( -7, -13),   S(-25, -46),   S(-11, -18),   S( -9, -20),

            /* queens: bucket 13 */
            S( -9, -23),   S( -4, -15),   S(  0,   0),   S(  0,   0),   S( -2,  -3),   S(  4,   1),   S( 12,  -4),   S( -1,  -2),
            S(-11, -40),   S( -3, -19),   S(  0,   0),   S(  0,   0),   S( -7, -14),   S(-17, -40),   S( -8, -23),   S( -8, -20),
            S( -7, -13),   S( -5, -14),   S( -3,  -6),   S(  4,   3),   S( -7, -26),   S(-16, -38),   S( -6, -13),   S(  5,   9),
            S(  3,   1),   S( -8, -29),   S(  4,  -4),   S(  9,   0),   S( 16,  32),   S( 29,  56),   S( -1, -12),   S( -8, -19),
            S( -1, -14),   S( -7, -29),   S( -9, -33),   S( 14,  21),   S(-12, -33),   S( -5, -13),   S(-18, -41),   S( -1,  -5),
            S( 12,  12),   S(-24, -59),   S( -4,  -9),   S(  8,   9),   S(  6,  11),   S(-10, -23),   S(-32, -55),   S(-15, -38),
            S(-20, -35),   S( -9, -20),   S( 15,  28),   S( -2,  -7),   S( -9, -24),   S( 14,   3),   S(-12, -30),   S( -4, -11),
            S( -7, -27),   S( -7, -21),   S( -2, -10),   S( -3,  -7),   S(  3,   3),   S( -8, -25),   S(  6,  11),   S(-20, -34),

            /* queens: bucket 14 */
            S( -7, -31),   S( 16,  -2),   S( -5, -22),   S(  5,  -1),   S(  0,   0),   S(  0,   0),   S( 10,  11),   S(-12, -30),
            S(-11, -32),   S(-15, -43),   S( -9, -25),   S( -6, -27),   S(  0,   0),   S(  0,   0),   S(  3,   0),   S(-14, -34),
            S( -7, -15),   S( -3, -32),   S(  9,   0),   S( -8, -27),   S( 13,  28),   S(  6,  10),   S(-10, -23),   S(-10, -33),
            S( -6, -15),   S( -5,  -9),   S(  8,   7),   S(-11, -38),   S( -9, -13),   S(-10, -28),   S( -4, -25),   S(  2,  -3),
            S( -4,  -3),   S( -5, -24),   S( -7, -24),   S(  3,   3),   S( 11,  19),   S( -4,  -5),   S( -6, -13),   S(  4,  -5),
            S( -8, -19),   S(-10, -23),   S(-13, -31),   S(-21, -51),   S(  7,  17),   S( -3, -16),   S(-16, -36),   S(-16, -39),
            S(-15, -26),   S( -5, -18),   S( -3, -11),   S( -6,  -8),   S( -4,  -6),   S( -2,  -7),   S(-18, -41),   S(  2,   7),
            S(-16, -32),   S( 12,  -1),   S(-16, -29),   S(  0,  -9),   S(  6,  10),   S(  1,   4),   S(-15, -37),   S(  5,   7),

            /* queens: bucket 15 */
            S( -8, -27),   S(-12, -26),   S( 13,  12),   S( -9, -16),   S(  5,  -5),   S( -5, -12),   S(  0,   0),   S(  0,   0),
            S( -5, -11),   S(  5,   4),   S(-14, -26),   S( -5, -12),   S( -4,  -7),   S( -1,  -1),   S(  0,   0),   S(  0,   0),
            S( -5, -16),   S( -5, -14),   S( -7, -10),   S(  3,   4),   S( -7, -25),   S(  6,   1),   S( -6, -17),   S(  1,  -1),
            S( -4,  -9),   S(-11, -24),   S( -9, -23),   S(  7,  16),   S( 10,  23),   S(  4,   7),   S( -5, -13),   S( -7, -21),
            S( -4,  -7),   S(  5,  10),   S(  0,   2),   S( -4, -16),   S( 16,  46),   S(  3,   1),   S( -8, -20),   S( -4, -22),
            S(  2,  -3),   S( -6, -12),   S( -6, -11),   S(-20, -39),   S( -3,   0),   S(-13, -27),   S( -4, -13),   S(-12, -27),
            S( -5, -11),   S( -4,  -9),   S(-10, -14),   S( -3,  -5),   S(-12, -19),   S( -8, -14),   S( 12,  24),   S(-12, -20),
            S(-11, -27),   S(-13, -36),   S(  1,  -3),   S(  3,   3),   S(-16, -28),   S( -6, -16),   S(  3,   7),   S( -5, -12),

            /* kings: bucket 0 */
            S( 11, -39),   S( 34,  -6),   S( 21, -10),   S(-34,   6),   S(-12,  -5),   S( 25, -29),   S(  9,  12),   S( 20, -52),
            S(-26,  32),   S(-21,  16),   S(  0,  14),   S(-33,  13),   S(-44,  38),   S(-11,  18),   S( -4,  36),   S(-15,  37),
            S(-14,   8),   S( 39, -13),   S(-13,  15),   S(-14,  -8),   S( -7,  -5),   S(-22,   0),   S(-42,  17),   S( -1, -21),
            S(-27, -40),   S( 28, -22),   S(-20, -11),   S(-37,   0),   S(-38,  20),   S(-51,   5),   S(-20,  17),   S(-36,  33),
            S(-45, -86),   S( 18, -41),   S( 23, -29),   S(  6,  -7),   S(-37,  11),   S(-18,  -4),   S( -6,  -1),   S( 10,   2),
            S( 10, -106),  S( 24, -16),   S( 47, -70),   S( -3, -31),   S(  9, -11),   S( 17, -42),   S( 47,   4),   S(-18, -14),
            S(  0,   0),   S(  0,   0),   S( -1, -32),   S( 19, -45),   S( 21,  -4),   S(  9, -31),   S( 15,  -9),   S(-26, -14),
            S(  0,   0),   S(  0,   0),   S(-11, -71),   S( 23, -18),   S(  6, -12),   S(  8,  -7),   S(  3,  -9),   S(  2,   5),

            /* kings: bucket 1 */
            S(  3, -36),   S( 35, -19),   S( 13, -17),   S( 11,   2),   S(-16,   4),   S( 21, -17),   S( 10,  14),   S( 27, -31),
            S( -4,  11),   S(  9,  19),   S( 15, -10),   S(-40,  25),   S(-20,  23),   S(-16,  24),   S(  1,  22),   S(-24,  34),
            S( -4, -12),   S(  2, -10),   S(  6, -15),   S( 18, -24),   S(-33,   4),   S( 12, -22),   S( -2,   7),   S( 67, -23),
            S( 16, -14),   S( 22,  -9),   S( 14,   6),   S(-15,  14),   S(-22,  23),   S(-32,   6),   S( 14,   0),   S(-52,  31),
            S(-29, -56),   S(  2, -25),   S( 38, -43),   S( 13, -19),   S( -4, -12),   S( -8, -19),   S( 13,   2),   S( -8,   8),
            S( 19,  -9),   S( 44, -39),   S( 23,  -8),   S( 33, -19),   S( 12, -19),   S( 31,   0),   S( 29,  -1),   S( -6,   0),
            S(  4, -31),   S(  4,   2),   S(  0,   0),   S(  0,   0),   S(-21,  20),   S( 10,  11),   S(  7,  40),   S(-23, -52),
            S(-17, -123),  S( -1,   0),   S(  0,   0),   S(  0,   0),   S( -1, -27),   S( 16,  -6),   S(  1,  22),   S( -3, -48),

            /* kings: bucket 2 */
            S( 28, -54),   S(  8,   7),   S( 13, -27),   S( 32, -19),   S(-15,  14),   S( 35, -27),   S(  5,  26),   S( 25, -17),
            S( 26, -24),   S(-12,  34),   S(  7,   0),   S(-21,  15),   S(-18,  16),   S(-10,   2),   S(  8,  11),   S(-11,  12),
            S(-25,  -6),   S( -9,  -4),   S(  0, -14),   S(-13, -14),   S( -3,  -2),   S( 25, -30),   S( 35, -15),   S( 17, -12),
            S( -3,  19),   S(-31,  17),   S(-24,   7),   S( -4,  14),   S( 25,  -4),   S(-26,  -4),   S( 16, -22),   S( 48, -23),
            S(-16,  -3),   S(  5,  -2),   S( -8, -22),   S(  7, -14),   S( 27, -34),   S(-29, -31),   S( 58, -39),   S(  3, -29),
            S( -2,  -3),   S(  9, -16),   S( 24, -23),   S( 46, -23),   S( 59, -15),   S( 29,   8),   S( 52, -33),   S( 16, -28),
            S(-15, -31),   S(  4,  18),   S(-22, -31),   S( 17,  14),   S(  0,   0),   S(  0,   0),   S( 19,  31),   S(  1, -28),
            S( -6, -16),   S(  0, -16),   S(  6, -33),   S( 11,  15),   S(  0,   0),   S(  0,   0),   S(  5,   4),   S(-22, -136),

            /* kings: bucket 3 */
            S(  6, -66),   S( 12,  -7),   S( 23, -41),   S( -4, -12),   S(-10, -25),   S( 31, -33),   S(  5,  16),   S(  8, -31),
            S(-11,  13),   S(-27,  37),   S(-13,   4),   S(-34,  13),   S(-52,  26),   S(  1,  -8),   S( -8,  17),   S(-18,  16),
            S( 35, -27),   S( -4,  -1),   S( 17, -19),   S( -1, -23),   S( 14,  -9),   S( 32, -36),   S( 51, -25),   S( 54, -26),
            S(-24,  14),   S(-117,  35),  S(-82,  17),   S(-88,  26),   S(-72,  16),   S(-66,  -4),   S(-76,   5),   S(-72,  -6),
            S(-33,   6),   S(-22,  -1),   S(-56,   5),   S(-61,   0),   S( -5, -30),   S( 23, -47),   S(-17, -42),   S( -9, -58),
            S(-21, -10),   S( 11, -16),   S( 12, -36),   S(-44, -17),   S( 35, -38),   S( 75, -57),   S(108, -55),   S( 30, -118),
            S(-33,   5),   S( 27, -17),   S( -6, -40),   S(  1, -14),   S( 27, -19),   S( 24, -53),   S(  0,   0),   S(  0,   0),
            S(-13, -27),   S(  4, -28),   S(-14,   4),   S( -9,  -9),   S(  4, -79),   S(  7, -33),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-30,  13),   S( -4,  30),   S(-14,  25),   S(  1,   3),   S(  5, -10),   S(  4,   0),   S( 26,   9),   S(  2,  -2),
            S(-25,  17),   S( 32,  10),   S(-17,   9),   S( -5,   9),   S( 39,  -5),   S( 20, -12),   S( 68, -21),   S( 14,  -6),
            S( -5,  26),   S(  2, -10),   S(-10,   6),   S( -7,   2),   S(-36,   6),   S( 21, -27),   S(-15,  -8),   S( -3,   3),
            S(-10, -18),   S( 13,   2),   S( -4,   7),   S( -6,   3),   S( 10,  -6),   S(-10,  20),   S( -9,  12),   S( 19,  11),
            S(  0,   0),   S(  0,   0),   S( 10, -24),   S( -4,   1),   S(-19,   1),   S(-31,   9),   S(-10,  15),   S(-14,  14),
            S(  0,   0),   S(  0,   0),   S(  6,  20),   S(  9,  -4),   S(  8,  11),   S(-18, -20),   S(  5, -28),   S( -6,  -7),
            S( -2, -24),   S( 12,  33),   S(  0, -50),   S( 11,   3),   S( 16,  28),   S( -7,   9),   S(  3,  -5),   S(  3, -11),
            S( -8,  -6),   S(  8,  22),   S(  0,   7),   S(  7,  27),   S( -5, -22),   S( -8,  -4),   S(  6,  -6),   S(  3,   8),

            /* kings: bucket 5 */
            S( 38,  -9),   S( -1,  21),   S(-64,  21),   S(-44,  25),   S(-47,  25),   S(  6,   5),   S( 53,   1),   S( 23,  -3),
            S( -7,   4),   S( 52, -11),   S( 37, -12),   S( 39,  -8),   S( 43, -12),   S( 34,  -7),   S( 47,  -2),   S( 27,   2),
            S(-10,   2),   S(-21,   2),   S(-20,  -4),   S(-28,  -3),   S(-12,   4),   S(-73,   7),   S( -5,   5),   S( 15,  -8),
            S(-17,  -3),   S( 73, -19),   S( 24, -10),   S( 24,  17),   S( 31,  12),   S( 15,  -3),   S( 15,   5),   S(  4,   6),
            S( -6, -30),   S( -4, -20),   S(  0,   0),   S(  0,   0),   S(-10,   4),   S(-38,  -2),   S(-15,   0),   S(  3,  -3),
            S(-39,  -4),   S(-28,   8),   S(  0,   0),   S(  0,   0),   S(  0,  13),   S(-44,   2),   S(-30,  11),   S( -8,  -1),
            S(-21,  -5),   S( -4,  21),   S(  7,  38),   S( -7, -14),   S(-11,  20),   S(  3,  21),   S( 17,  19),   S( 15,   8),
            S(-16, -24),   S(  4,  53),   S( -1,  40),   S( 10,  18),   S( -7,  -7),   S( -3,  12),   S(-12,  11),   S(  0,  -2),

            /* kings: bucket 6 */
            S( 25, -29),   S( 35,  -9),   S(  3,  -2),   S(  2,   7),   S(-15,  19),   S(-32,  19),   S( 25,  20),   S( 20,  11),
            S( 36,  -2),   S( 21,  18),   S( 16,   0),   S( 49, -13),   S( 19,   4),   S(  0,   7),   S( 19,   9),   S( 14,   3),
            S( -4, -17),   S(-11,   2),   S(-19,  -8),   S( -6,  -8),   S(-12,  -1),   S(-54,  -1),   S( -2,   5),   S(-68,  27),
            S( 11,  23),   S( 21,   0),   S( 43, -11),   S( 27,   6),   S( 65,   3),   S(  4,  -2),   S( 96, -26),   S( 31,  -5),
            S( -1,  -1),   S(-35,   2),   S( -8, -17),   S( -8,   0),   S(  0,   0),   S(  0,   0),   S( -2, -16),   S(-66,  -8),
            S(-13,   5),   S( 11,   4),   S(-30,   0),   S( -7,  -2),   S(  0,   0),   S(  0,   0),   S(-50,  21),   S(-46, -15),
            S(  4, -22),   S(  5,  21),   S(  4,  -6),   S( -2,  17),   S(  0,  13),   S( -9, -24),   S(-16,  11),   S(-24, -13),
            S(  2,  31),   S( -2,  -6),   S(  9,  32),   S(-13, -15),   S( -1,  -2),   S( -5,  30),   S(  3,  30),   S(-12, -12),

            /* kings: bucket 7 */
            S( 38, -20),   S( -9, -10),   S(  6, -12),   S(-17,  11),   S(-46,  17),   S(-45,  33),   S( -6,  34),   S(  5,  13),
            S( 18,   8),   S( 54, -25),   S( -7,  -8),   S(-10,  -5),   S(  0,   4),   S(-25,  18),   S( -2,  10),   S( 12,   9),
            S( 23, -20),   S(-26,   2),   S(-32,   1),   S(-27,  -4),   S(-35,   2),   S(-56,  13),   S(-12,  10),   S(-57,  17),
            S(-23,   8),   S( 34,  -8),   S( 12,  -7),   S( 17,   4),   S(  9,   1),   S( 31, -20),   S( 34,  -9),   S( 38, -20),
            S(  1,  -6),   S( 10,  19),   S( -9, -16),   S(-22,   2),   S(-10, -20),   S( 11, -25),   S(  0,   0),   S(  0,   0),
            S( -9, -47),   S( -5,  -1),   S( 33, -12),   S(  5,  -3),   S(  0, -10),   S(  8,   8),   S(  0,   0),   S(  0,   0),
            S( 12,  37),   S( 24,  -9),   S( 13,   3),   S( -4, -19),   S( 37, -34),   S( -2,   1),   S(  6,   9),   S( -5, -26),
            S( 14,   9),   S(-14, -35),   S( 24,  18),   S(  7,   0),   S( 18,  28),   S(-19, -25),   S(  1,  10),   S(-11, -11),

            /* kings: bucket 8 */
            S(-11, 107),   S(-29,  71),   S(-40,  49),   S( -5,  10),   S( -5,   0),   S( -2,  18),   S( 33, -17),   S( 28,   1),
            S( 19,  81),   S( 16, -14),   S(  3,  67),   S(  7,   1),   S(  5,  11),   S(  5,  -5),   S( 16,   4),   S(-11,  34),
            S(  0,   0),   S(  0,   0),   S( 16,  38),   S( 16,  15),   S( 15,  -8),   S(  5,  -4),   S(-12,  12),   S(  7,  12),
            S(  0,   0),   S(  0,   0),   S( 14,  21),   S( 30, -28),   S(  5,  14),   S( 13, -12),   S( 14, -14),   S( -6,  23),
            S(  0,   1),   S(  8,  13),   S(  7, -31),   S(  8,  -6),   S(  5, -28),   S( -8, -20),   S(  5,  -4),   S( -7, -38),
            S( -7, -20),   S( -1,   6),   S( -1, -15),   S( -2,   1),   S( -9, -19),   S(-20,   6),   S(-13, -13),   S( 14,  15),
            S( -4, -10),   S(  2, -11),   S(  4,  -1),   S( -2, -14),   S(  0, -23),   S( -3, -23),   S(  8,   0),   S( 12, -45),
            S(  1,  15),   S(-12, -52),   S(  6,   4),   S( -4, -27),   S(  4,  34),   S( -2,  -5),   S( -2, -25),   S(  1,  -2),

            /* kings: bucket 9 */
            S(-36,  52),   S(-49,  40),   S(-48,  47),   S(-75,  26),   S(-74,  41),   S(-55,  31),   S( 56,  -1),   S( 17,  20),
            S(-22,  16),   S( 34,  29),   S( -4,  -6),   S( 21,  27),   S( 39,  14),   S( 22,   0),   S( 33,  26),   S( 44,  -2),
            S(-15,  16),   S(  1,   2),   S(  0,   0),   S(  0,   0),   S( 22,  11),   S( -5,  -4),   S( 17,   0),   S(  2,   6),
            S( -9, -14),   S(  1,   4),   S(  0,   0),   S(  0,   0),   S( -8,  27),   S( 44,  -9),   S(-19,  10),   S( -8,  13),
            S(-10,  13),   S(  6,  -6),   S( -4,   3),   S(  5,  -6),   S( 16,  -5),   S( -1,   0),   S(  1,  21),   S( -6,  -5),
            S(  1,  33),   S( -8,  11),   S( -1,  16),   S(  0, -13),   S( -6,   4),   S( -7,  14),   S( -4,  -6),   S(  5,  36),
            S(  3,  -2),   S( -3, -12),   S( 10,  35),   S(  3,  29),   S(  2,  33),   S( 30,  22),   S( -8, -11),   S(  8,  20),
            S(  2,  15),   S( -9, -17),   S( -3, -36),   S(-13, -52),   S(-15, -65),   S( -1,   3),   S( -3, -34),   S( 12,  48),

            /* kings: bucket 10 */
            S(  8,  36),   S(-24,   7),   S(-21,  15),   S(-27,  23),   S(-65,  25),   S(-116,  43),  S(-29,  49),   S(-86,  69),
            S( 14, -16),   S( 32,  18),   S( -7,   2),   S( 23,  11),   S( 72,  10),   S( 31,   6),   S( 10,  25),   S(-22,  32),
            S( 18,  17),   S( 24,  -5),   S( 31, -12),   S( 11,   3),   S(  0,   0),   S(  0,   0),   S(  9,  15),   S(-24,  13),
            S( 16,   6),   S(  2, -18),   S( 21,  -9),   S( 29,   4),   S(  0,   0),   S(  0,   0),   S(  3,  28),   S(  5,  -5),
            S(  2,  -2),   S( 28,  14),   S(  4,  -2),   S(  7, -20),   S( -5, -12),   S(  8,  13),   S(  2,   3),   S(-37,  19),
            S(  5,  45),   S( -5,   3),   S(-16,   0),   S(  3,   8),   S( -6,   1),   S(  3,  -3),   S(-11,  25),   S(-11,   9),
            S(  1, -43),   S( -3,  -5),   S(  6,  -7),   S( 17,   5),   S(  8,   7),   S( -7,   5),   S( 15,   1),   S( -5,  11),
            S( -2,  -2),   S( 19,  17),   S( -4, -24),   S(  0,   6),   S(  5,  -1),   S( -5, -25),   S(  4,  -8),   S(  9,  48),

            /* kings: bucket 11 */
            S(-52,  21),   S(-11,  -7),   S(  0, -19),   S( -3,  16),   S(-26,   0),   S(-134,  64),  S(-54,  66),   S(-153, 155),
            S( 34, -29),   S( -4,  23),   S(-14, -23),   S(-11,  17),   S( 31,   6),   S(  4,  56),   S( 55,   9),   S( 50,  35),
            S(  2, -31),   S( 30,   3),   S( 15,  -9),   S(-15,   5),   S( 57,  -1),   S( 34,  19),   S(  0,   0),   S(  0,   0),
            S(  3,  29),   S( -9,   7),   S( 14,  -4),   S( 39,  -2),   S( 42,  -9),   S( 32,   9),   S(  0,   0),   S(  0,   0),
            S(  3,  -3),   S(-17,  15),   S(  0,  -4),   S( -3, -21),   S( 34, -20),   S( 12, -13),   S(  8,   1),   S(  5,   0),
            S( 14,  24),   S(  2,   5),   S(  3,  -3),   S(  3, -18),   S(  4, -23),   S(  6,   0),   S(-15,   0),   S(  1,  -1),
            S( 13,  -4),   S( 15,  -2),   S(  9,  15),   S(  8, -23),   S(  6, -18),   S(  7,   3),   S(  5, -15),   S(-10, -20),
            S(  3,  -9),   S(  4, -17),   S(-15, -43),   S(  7,   7),   S( -8, -19),   S( -4, -11),   S(  7,   0),   S(  9,  27),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  3,  78),   S( -4,  28),   S(  1, -11),   S(  3,  25),   S( 11,  -4),   S(  1,  56),
            S(  0,   0),   S(  0,   0),   S( 12,  81),   S(  0, -29),   S( -1,  23),   S( 14,   6),   S( 12,  -5),   S( -9,  20),
            S(  1,   2),   S(  0, -25),   S( 19,  11),   S( 24,  32),   S(  2, -15),   S( -9,  22),   S( -4,  -6),   S(-18, -16),
            S( -7,   6),   S( -4,  -8),   S( -5, -35),   S( -2, -52),   S(-17, -37),   S( 16,   6),   S(-13,  20),   S( -1,   1),
            S( 10,  28),   S(  1,   5),   S(  5,  35),   S(  9,  -6),   S(  1,   9),   S( -9,  -1),   S(-12,  16),   S(-14, -25),
            S(  1,  12),   S(  1,  14),   S( -3,   3),   S( -2, -16),   S(  0,  13),   S( -7,  15),   S(-17,   6),   S(  9,  33),
            S(  3,  22),   S( -4, -20),   S(  2,  21),   S(  2,  -6),   S(  1,   0),   S( -5,   5),   S(  9, -14),   S( -4, -10),
            S( -2,  -2),   S(  3,  -1),   S( -3, -22),   S( -5, -26),   S( -5, -17),   S( 12,  22),   S( -3, -11),   S(-12, -37),

            /* kings: bucket 13 */
            S(-13,  66),   S( -7,  69),   S(  0,   0),   S(  0,   0),   S(  1,  75),   S(-25,  -7),   S( 11,  -7),   S(  6,  56),
            S( -2,  42),   S(-10, -12),   S(  0,   0),   S(  0,   0),   S( 13, -13),   S(  0, -17),   S(-13,   4),   S( 24,  10),
            S(-15,  -1),   S(  7,  24),   S(-10, -56),   S(  2,  24),   S( -6,   2),   S( -5,   4),   S( -9,   7),   S( -4,   8),
            S(-12, -34),   S(  1,  23),   S(  2,   2),   S( -3, -50),   S( -1, -23),   S( 12, -12),   S(-10,   5),   S(-17, -22),
            S(  3,  25),   S( -3,   9),   S( 10,  35),   S( -6, -46),   S(  5,  22),   S( -1,  18),   S(-22,  -5),   S( 15,  37),
            S(  4,   9),   S( -9,  -5),   S( -1,  26),   S(  1,  10),   S(-16, -23),   S( -6,   3),   S( -5,  -1),   S(  3,  13),
            S(  8,  31),   S(-11, -21),   S( -6, -27),   S(  0,   1),   S(-10, -41),   S( -1, -22),   S(  1, -45),   S(  9,  30),
            S(  3,   4),   S(  4,  20),   S(  1,  18),   S(  5,   8),   S(  4,  20),   S(-14, -13),   S( -3, -14),   S(  6,  16),

            /* kings: bucket 14 */
            S( -8,  39),   S(  3,  25),   S(  0,  -5),   S(-11,  10),   S(  0,   0),   S(  0,   0),   S(  1,  96),   S(-42,  77),
            S(-25,  -9),   S(-14,   0),   S(  2,  -3),   S(  4,  -1),   S(  0,   0),   S(  0,   0),   S(  1,  23),   S(-22, -14),
            S( -6,  -8),   S(  5,  -7),   S( 19,   9),   S( -9, -12),   S(  3, -22),   S(  4,  30),   S(  1,  28),   S(-37,  -7),
            S(  3, -12),   S(  3,  15),   S(  6, -25),   S(  1, -18),   S( -3, -50),   S( 11,  26),   S(  2,   5),   S( -1,   4),
            S( 11,  41),   S( -4, -23),   S(-15, -14),   S(  1,   6),   S( -4,  17),   S( 12,  20),   S(  0,  26),   S(  2,  14),
            S( -1, -13),   S( -5,  13),   S( -6,  -9),   S(  4,  25),   S(-12, -19),   S(  3,  -4),   S( -2,  -9),   S( -1,  14),
            S(  4,  -6),   S( -4, -59),   S(  7,  21),   S( 11,  15),   S( -8, -16),   S(  3,  -7),   S(-10, -54),   S( 13,  62),
            S(  0,   5),   S( -2,  50),   S(  2,  20),   S(  1,   0),   S(  4,  48),   S(  0, -25),   S(-13, -44),   S( -3, -27),

            /* kings: bucket 15 */
            S(  7,  38),   S(  0, -11),   S( 13,  26),   S(-15, -25),   S(-28, -10),   S(-22,  74),   S(  0,   0),   S(  0,   0),
            S( -6, -19),   S(  5,  19),   S( -5, -19),   S( 25,  51),   S( 16, -28),   S( 31,  98),   S(  0,   0),   S(  0,   0),
            S(-22, -10),   S( 14,   2),   S( -3, -23),   S(-16,  -7),   S( 16, -18),   S( 25,  35),   S( 11,   3),   S(-10, -29),
            S(  9,  23),   S(-17,  -1),   S( -1,  19),   S(  0, -25),   S(  5, -36),   S(  3,   3),   S(  6,  52),   S(  2, -10),
            S(  3,  19),   S(-11,  24),   S(-10,   7),   S( -8, -23),   S(-14, -56),   S( -4,  23),   S( -8,  -4),   S( -9,  -7),
            S( -4,  26),   S( -8,   2),   S(  1,  -4),   S(  6,  -3),   S( -5,  10),   S(  0, -28),   S(  2,   5),   S(  4,  14),
            S( 12,  27),   S( -3,  26),   S( -3,  30),   S( -7, -10),   S(  0, -34),   S(  5,  16),   S(-14, -28),   S(  8,  17),
            S(  5,  29),   S( -3, -10),   S(  0,  11),   S( -4, -17),   S(  4,  47),   S(  4,  35),   S(  2,   1),   S(  4,  11),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-11, -189),  S(  9, -27),   S( 20,  44),   S( 28,  82),   S( 35, 102),   S( 40, 123),   S( 45, 121),   S( 56, 109),
            S( 81,  73),

            /* bishop mobility */
            S(  1, -92),   S(  5, -11),   S( 15,  27),   S( 22,  65),   S( 26,  90),   S( 31, 108),   S( 31, 118),   S( 33, 118),
            S( 34, 122),   S( 43, 113),   S( 54, 105),   S( 67,  96),   S( 91,  92),   S(126,  67),

            /* rook mobility */
            S(-21,  -4),   S(-13,  57),   S(-13,  87),   S( -9, 106),   S(-10, 129),   S( -3, 137),   S(  1, 148),   S(  8, 148),
            S( 14, 155),   S( 20, 159),   S( 28, 161),   S( 42, 157),   S( 48, 155),   S( 69, 144),   S(155,  99),

            /* queen mobility */
            S( 54, -54),   S( 61, -66),   S( 54,  29),   S( 50,  93),   S( 51, 138),   S( 50, 175),   S( 49, 216),   S( 48, 241),
            S( 48, 263),   S( 49, 276),   S( 45, 304),   S( 41, 325),   S( 43, 332),   S( 41, 343),   S( 36, 356),   S( 37, 360),
            S( 44, 355),   S( 50, 346),   S( 71, 327),   S( 96, 301),   S(111, 285),   S(152, 257),   S(185, 231),   S(183, 221),
            S(183, 227),   S(168, 207),   S(124, 208),   S( 75, 166),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  30),   S(-21,  60),   S(-39,  59),   S(-26,  67),   S(  8,  23),   S(-11,  34),   S(-14,  86),   S( 35,  32),
            S( 20,  40),   S(  1,  62),   S(-10,  54),   S(-10,  43),   S(  3,  42),   S(-34,  61),   S(-31,  82),   S( 33,  38),
            S( 37,  66),   S( 20,  75),   S( 11,  59),   S( 35,  46),   S(  8,  54),   S(-29,  69),   S(-30, 105),   S(  7,  71),
            S( 57,  89),   S( 64, 103),   S( 44,  69),   S( 24,  51),   S( 19,  54),   S( 13,  83),   S(-10, 108),   S(-39, 123),
            S( 65, 129),   S(117, 156),   S(102,  97),   S( 56,  96),   S(-32,  86),   S( 47,  82),   S(  3, 161),   S(-33, 148),
            S(148, 184),   S(141, 204),   S(178, 195),   S(187, 189),   S(207, 197),   S(192, 189),   S(205, 193),   S(209, 203),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  12),   S(  2,   5),   S( 10,   3),   S( 12,  24),   S( 21,  43),   S(  0, -25),   S(-20,  40),   S( 10, -34),
            S( -9,  40),   S( 28,   1),   S(  1,  53),   S( 21,  33),   S( 36,   1),   S(  1,  35),   S( 16,  -2),   S( 11,  11),
            S( -5,  25),   S( 17,   5),   S( -2,  58),   S( 16,  67),   S( 31,  31),   S( 22,  27),   S( 30,   6),   S(  0,  20),
            S(  7,  41),   S( 32,  43),   S( 34,  85),   S( 16,  92),   S( 79,  52),   S( 72,  48),   S( 31,  48),   S( 18,  27),
            S( 79,  47),   S(119,  80),   S( 97, 131),   S(129, 152),   S(151, 122),   S(138, 118),   S(141,  86),   S( 78,  35),
            S( 81, 204),   S(123, 280),   S(112, 231),   S(120, 233),   S( 81, 181),   S( 62, 154),   S( 46, 153),   S(  8,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  53),   S( 25,  34),   S( 42,  56),   S( 40,  40),   S( 30,  39),   S( 29,  39),   S(  7,  24),   S( 45,  16),
            S(  0,  21),   S( 15,  47),   S( 13,  37),   S( 11,  50),   S( 24,  20),   S(  9,  29),   S( 28,  22),   S( -1,  19),
            S( -2,   8),   S( 20,  25),   S( 51,  40),   S( 40,  34),   S( 40,  42),   S( 62,  15),   S( 18,  22),   S( 16,   5),
            S( 78,  31),   S(128,   5),   S(119,  89),   S(166,  93),   S(150,  84),   S(110,  74),   S( 96,   8),   S( 97, -28),
            S( 91,  22),   S(149,  56),   S(151, 150),   S(198, 160),   S(176, 203),   S(135, 162),   S(160, 128),   S(-48, 121),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  25),   S(  4,  15),   S( 18,  25),   S(-10,  59),   S( 75,  21),   S( 35,   4),   S( -3, -10),   S( 27,   4),
            S(  3,  13),   S(  7,   6),   S( 19,  18),   S( 17,  28),   S( 15,  15),   S(  2,   6),   S(  5,   6),   S( 28,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -3, -13),   S( -7,  -6),   S(-19, -18),   S(-17, -28),   S(-15, -15),   S( -2,  -6),   S( -5,  -6),   S(-28,   5),
            S(-25, -25),   S( -4, -15),   S(-18, -25),   S( 10, -59),   S(-75, -21),   S(-35,  -4),   S(  3,  10),   S(-27,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-14, -23),   S( -8, -13),   S(  1, -32),   S(-50,  -6),   S(-16, -17),   S(-18, -20),   S(  1, -26),   S(-11, -39),
            S(-17,   6),   S(-17,  -2),   S(-23,   8),   S(  6, -15),   S(-28, -15),   S(-25,  -4),   S(-46,   6),   S(-10, -14),
            S(-10, -19),   S(-10, -20),   S(-27,  -2),   S(-28,  -9),   S(-24, -29),   S(-21, -11),   S(-19,  -4),   S(-37, -18),
            S(  0, -31),   S( 17, -37),   S( 19, -27),   S(  4, -26),   S(  8, -20),   S( 69, -34),   S( 37, -35),   S( -6, -45),
            S( 35, -62),   S( 66, -80),   S( 58, -36),   S( 66, -30),   S( 70, -36),   S( 80, -24),   S(128, -74),   S( 41, -76),
            S(141, -126),  S(171, -123),  S(135, -74),   S(113, -47),   S( 83, -32),   S(130, -30),   S(154, -62),   S(111, -107),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 16,   2),        // attacks to squares 1 from king
            S( 17,   1),        // attacks to squares 2 from king

            /* castling available */
            S( 69, -74),        // king-side castling available
            S( 20,  53),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 20, -51),   S( 22, -42),   S( 13, -47),   S( -2, -32),   S( -9, -25),   S(-26, -16),   S(-59,   2),   S(-70,   6),
            S(-84,  11),   S(-62,  12),   S(-92,  25),   S( 16,  12),   S(  6,   5),

            /* orthogonal lines */
            S(-10, -64),   S(-64, -30),   S(-88, -19),   S(-102, -17),  S(-107, -24),  S(-110, -29),  S(-109, -35),  S(-109, -36),
            S(-126, -29),  S(-146, -29),  S(-173, -33),  S(-170, -41),  S(-183, -33),  S(-402,  37),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 10, 198),

            /* passed pawn can advance */
            S(-16,  38),   S(-18,  70),   S( -5, 112),   S(  6, 203),

            /* blocked passed pawn */
            S(  0,   0),   S( 32, -19),   S( 49, -16),   S( 53,  27),   S( 60,  43),   S( 75,  -8),   S(170, -28),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 49,  -9),   S( 32,  51),   S( 19,  48),   S( 21,  66),   S( 55,  79),   S(124, 119),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-51,  12),   S(-13,  -9),   S(  9, -40),   S(  8, -39),   S( 43, -57),   S(204, -82),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 21, -24),   S( 52, -55),   S( 15,  -1),   S( 35, -52),   S( 31, -155),  S(  5, -264),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( -2,  50),   S( 29,  43),   S(137,  33),   S( 16,  35),   S(227,  74),   S(189,  48),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S( 19, 165),

            #endregion

            /* tempo bonus for side to move */
            S(  8,  11),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
