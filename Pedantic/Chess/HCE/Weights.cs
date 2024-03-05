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
        public const int MAX_WEIGHTS = 12759;
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
        public const int BAD_BISHOP_PAWN = 12757;   // bad bishop pawn
        public const int TEMPO = 12758;             // tempo bonus for side moving

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

        public Score BadBishopPawn
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[BAD_BISHOP_PAWN];
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

        // Solution sample size: 6000007, generated on Mon, 04 Mar 2024 19:16:17 GMT
        // Solution K: 0.003850, error: 0.085652, accuracy: 0.4969
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S(100, 179),   S(423, 539),   S(433, 572),   S(562, 957),   S(1339, 1654), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 97, -117),  S(143, -92),   S( 22, -27),   S(-30,  20),   S(-44,  21),   S(-23,  -8),   S(-49,  -5),   S(-46, -18),
            S(105, -112),  S( 92, -100),  S( -7, -55),   S(-21, -52),   S(-26, -24),   S(-24, -34),   S(-49, -23),   S(-40, -39),
            S( 96, -80),   S( 69, -55),   S( 12, -51),   S(  7, -66),   S( -9, -56),   S( -1, -53),   S(-19, -51),   S(-26, -43),
            S( 49, -23),   S( 48, -41),   S( 13, -40),   S( 14, -76),   S(-10, -53),   S(-35, -39),   S(-32, -34),   S(-35, -18),
            S( 90,  39),   S( 41,  -1),   S( 49, -21),   S( 40, -66),   S( 29, -53),   S( -6, -43),   S(-33, -17),   S(-47,  55),
            S( 96,  35),   S( 74,  80),   S( 20, -18),   S( 53, -67),   S(-32, -37),   S( -9,  -5),   S( 34, -26),   S(-24,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 30, -31),   S( 30, -40),   S( 49, -30),   S( -5,   4),   S( -2, -21),   S(  7, -20),   S(-41,  -8),   S(-43,  16),
            S( 23, -39),   S( 17, -47),   S( 10, -46),   S( -8, -37),   S(-12, -22),   S( -9, -34),   S(-42, -14),   S(-50, -10),
            S( 15, -25),   S( 17, -25),   S( 24, -42),   S(  9, -49),   S(-13, -24),   S(  8, -37),   S(-13, -26),   S(-25, -12),
            S( 22,   3),   S( 23, -34),   S( 16, -28),   S( 12, -44),   S(  5, -30),   S(  3, -28),   S(-30, -20),   S(-37,  19),
            S( -5,  67),   S(-28,  -3),   S(  9, -13),   S( -4, -20),   S( 30,  -5),   S(-25,   4),   S(-22,  30),   S(-49,  72),
            S( 85,  55),   S(  7, -16),   S(-53,  -7),   S(-28,  40),   S(-39, -17),   S(-76,  -8),   S(-26,  31),   S(-12,  61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31,   4),   S(-36,   5),   S(-20,  -3),   S(-17,  13),   S( 10, -17),   S( 30, -22),   S(  2, -44),   S(-21, -14),
            S(-30, -11),   S(-54,  -6),   S(-32, -30),   S(-27, -27),   S(  3, -27),   S(  1, -25),   S(-19, -29),   S(-34, -21),
            S(-43,  -1),   S(-32, -15),   S(-14, -42),   S( -6, -51),   S(  0, -25),   S(  9, -25),   S( -6, -27),   S(-21, -10),
            S(-52,  19),   S(-32, -22),   S(-32, -22),   S( -8, -42),   S( 14, -38),   S( -8, -16),   S(-16,  -9),   S(-29,  17),
            S(-44,  60),   S(-57,  16),   S(-58,  -9),   S(-53, -21),   S( 16,  10),   S(-36,  32),   S(-25,  34),   S(-46,  90),
            S(-56,  74),   S(-89,  71),   S(-127,  17),  S(-47, -30),   S(-23,   3),   S( -7,   4),   S(-16,  15),   S(-14,  51),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31,  -7),   S(-41,   2),   S(-33,  -7),   S( -2, -50),   S(-15,  -9),   S( 30, -14),   S( 79, -62),   S( 58, -77),
            S(-38, -27),   S(-52, -22),   S(-38, -36),   S(-25, -28),   S(-21, -21),   S( -2, -30),   S( 49, -57),   S( 50, -69),
            S(-42, -24),   S(-20, -44),   S( -8, -61),   S( -7, -61),   S(-12, -45),   S(  2, -36),   S( 30, -46),   S( 52, -50),
            S(-46,   0),   S(-21, -48),   S(-14, -53),   S(  1, -66),   S( 18, -62),   S(  2, -42),   S( 15, -31),   S( 43, -10),
            S(-17,  35),   S(-43,  -8),   S(  2, -54),   S( -4, -56),   S( 78, -49),   S( 51, -30),   S( 56,  18),   S( 38,  72),
            S(-57, 108),   S(-49,  30),   S(-18, -54),   S(-48, -45),   S( 28, -43),   S( 53, -34),   S( 55,  18),   S( 19,  89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-70,  10),   S(  9, -25),   S( 10, -12),   S(-14,  11),   S(-29, -42),   S(-57,  23),   S(-85,   4),   S(-76,   8),
            S(-19,   5),   S( 38,  -9),   S( 42, -36),   S( -8,  -9),   S( -7, -24),   S(-54, -22),   S(  4, -50),   S(-15, -30),
            S( 35,  -6),   S( 49,  -4),   S( 23,  -5),   S( 15, -29),   S(-53, -18),   S(-13, -37),   S(-64, -23),   S(-30, -16),
            S( 32,  27),   S( 26,  31),   S( 63,   6),   S( -6,  13),   S(  0, -29),   S(-49, -14),   S(  1, -37),   S( 40, -30),
            S( 36,  75),   S( 26,  71),   S(  3,  25),   S(-11, -15),   S( 38, -10),   S(-17,  -1),   S(-48, -16),   S( 46,  12),
            S( 72,  89),   S( 63,  60),   S( 32,  31),   S(  9,   5),   S( 17, -28),   S( 22, -24),   S( 17,   2),   S(-35,   5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-81,  45),   S(-43,  21),   S(-20,  18),   S( 31,   0),   S( -1,  14),   S(-24,  16),   S(-23,  -6),   S(-38,  24),
            S(-66,  26),   S(-47,   8),   S( 24, -11),   S( 13,   7),   S(  3, -10),   S(-23, -15),   S(-27, -10),   S(-52,  16),
            S(-47,  44),   S(-23,  20),   S( 69, -29),   S( 14, -14),   S( 37, -10),   S(-47,   2),   S(-12,  -3),   S(-30,  11),
            S(-28,  58),   S(-11,  28),   S( 30,  10),   S( 63,  12),   S( -6,  10),   S(-60,  14),   S( 28, -11),   S( -7,  19),
            S( 36,  59),   S( 77,  26),   S( 89,  35),   S( 38,  34),   S( -5,  58),   S( 48,   3),   S( -9,  23),   S(  8,  54),
            S( 97,  53),   S( 80,  20),   S( 55,   0),   S( 60,   9),   S( 43,   6),   S( -6,   9),   S(-10,  18),   S( 32,  45),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,  23),   S( -5,  -2),   S(-11,   8),   S(-11,   7),   S(  3, -15),   S(-21,   5),   S(-56,   4),   S(-56,  15),
            S(-39,   4),   S( 12, -29),   S( -1, -32),   S( 17, -13),   S( 54, -18),   S( 40, -24),   S(-19, -10),   S(-78,  14),
            S(-37,  13),   S(  6, -11),   S( -7, -17),   S( -1, -13),   S( 42, -12),   S( 70, -30),   S(  5, -14),   S(-46,  21),
            S(-41,  34),   S(-67,  19),   S( 21, -13),   S(  9,  -2),   S( 42,  -1),   S( 45,   1),   S( 42,  -8),   S( 39,  11),
            S(-61,  58),   S(-15,  12),   S(  1,  -5),   S( 23, -14),   S( 55,  27),   S(101,  30),   S( 85,  -3),   S( 69,  42),
            S( 56,  35),   S( 34,   6),   S( 14, -24),   S( 43, -39),   S( 47,   9),   S( 52, -10),   S( 49,  -1),   S( 77,  32),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61, -16),   S(-51, -14),   S(-14, -16),   S(-52, -11),   S(-13, -28),   S( 27, -26),   S(  4, -44),   S(-42, -16),
            S(-56, -29),   S(-53, -35),   S(-25, -51),   S(  1, -51),   S(  6, -36),   S( 40, -43),   S( 52, -42),   S(-12, -28),
            S(-69, -21),   S(-50, -33),   S(-31, -47),   S(-12, -44),   S(  2, -25),   S( 29, -23),   S( 52, -38),   S( 47, -33),
            S(-47, -17),   S(-54, -33),   S(-74, -32),   S(-17, -24),   S(  4, -23),   S( 19,  -6),   S( 52, -12),   S( 88, -22),
            S(-44,  -5),   S(-21, -35),   S(-53, -30),   S(  1, -61),   S( 19,   9),   S( -2,  20),   S( 71,  46),   S(110,  44),
            S(-17,   7),   S(-47,  -3),   S( 32, -71),   S( -7, -50),   S( 37, -22),   S( 40, -52),   S( 19,  48),   S( 83,  69),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,  56),   S(-48,  36),   S(-11,  16),   S(-22, -30),   S(-10,  15),   S(-40,  10),   S(-62,  -3),   S(-73,  14),
            S(-49,  34),   S(-25,  28),   S(-47,  18),   S(-25,  -6),   S(-34,  -8),   S(-38, -18),   S(-13, -40),   S( 11, -14),
            S(-39,  77),   S( 25,  74),   S(-23,  47),   S( -8,  18),   S(  5, -13),   S(-48, -27),   S(-27, -45),   S(  5, -34),
            S( 32, 111),   S( 46, 120),   S( 73,  63),   S( 12,  50),   S( -7,  -8),   S(-18, -16),   S(-24,  -9),   S( -6, -21),
            S( 66, 115),   S( 61, 112),   S( 56, 131),   S( 50,  90),   S( 15,  14),   S(  7, -29),   S(  0, -38),   S(  9,   7),
            S( 84, 145),   S( 83, 151),   S( 89, 169),   S( 52,  72),   S( 10,   5),   S( -9, -34),   S(-12, -68),   S( 14,   2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-74,  52),   S(-67,  32),   S(-20,  10),   S( 13,  35),   S(  0,  14),   S(-78,  27),   S(-73,   4),   S(-99,  36),
            S(-63,  20),   S(-59,  13),   S(-53,  29),   S( -3,  30),   S(-92,   9),   S(-69,  -7),   S(-108,   4),  S(-47,   8),
            S(-57,  60),   S(-41,  61),   S(-25,  66),   S(-76,  69),   S(-41,  51),   S(-81,  11),   S(-28, -21),   S(-45,   7),
            S(  8,  87),   S( 26,  83),   S( 55, 114),   S( 76, 119),   S(-49,  70),   S(-13,  21),   S( 40,   3),   S( 53, -19),
            S( 79,  69),   S( 81,  93),   S(112, 117),   S(114, 127),   S( 82, 104),   S( 60,  35),   S( 27, -23),   S( 42, -12),
            S( 42,  47),   S( 79,  98),   S( 93,  85),   S( 96, 139),   S( 38,  87),   S( 15,   1),   S(  2, -17),   S( -5, -24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-141,  31),  S(-120,   4),  S( -6,   1),   S(  1,   9),   S(-31, -12),   S(-127,  50),  S(-141,  31),  S(-76,  35),
            S(-106,  -2),  S(-37, -19),   S(-12, -20),   S(-55,  -8),   S(-26,  16),   S(-40,  22),   S(-110,  26),  S(-109,  23),
            S(-40,  -5),   S(-27, -12),   S(-13,   5),   S(-61,  50),   S(-92,  96),   S( 14,  42),   S(-81,  33),   S(-116,  51),
            S( 16,  -8),   S( -4,   1),   S( 12,  30),   S(-10,  79),   S( 17, 101),   S(  0,  82),   S( 11,  39),   S( 67,  12),
            S( 74, -12),   S( 11, -13),   S( 42,  57),   S( 81, 103),   S( 80, 121),   S( 83,  69),   S( 40,  76),   S( 47,  46),
            S( 39, -16),   S( 17, -35),   S( 37,  20),   S( 90, 121),   S( 47,  69),   S( 71,  50),   S( 36,  35),   S( 38,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-108,  -4),  S(-90,   5),   S(  9,  -7),   S(-13,  22),   S( 12,  13),   S(-104,  43),  S(-70,  23),   S(-86,  37),
            S(-74, -19),   S(-64, -31),   S(-53, -33),   S(-41,  -4),   S(-69,  16),   S(-10,   8),   S(-102,  49),  S(-119,  50),
            S(-39, -29),   S(-52, -41),   S(  6, -19),   S(-26,  -6),   S(-44,  27),   S( 25,  40),   S(-74,  68),   S( -9,  55),
            S( 20, -20),   S(-29, -35),   S(-36, -17),   S(-25,  27),   S(  2,  46),   S( 39,  53),   S( 47, 104),   S(110,  56),
            S(  2,   2),   S(-38, -12),   S(  3,  -5),   S(-42,  19),   S( 61,  76),   S( 48,  76),   S(121,  95),   S(169,  61),
            S( 14, -37),   S(  4, -23),   S( 11, -52),   S( 15,   5),   S( 36,  45),   S( 42, 107),   S( 83, 129),   S(139, 120),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  18),   S(-11,  21),   S(-29, -24),   S(  5,  13),   S(-14, -15),   S(-12,   2),   S(-26, -10),   S(-28,   5),
            S(-43, -34),   S(  1, -22),   S(-13,   7),   S( -3, -13),   S( -8,  50),   S(-16,   3),   S(-33, -17),   S(-13, -55),
            S(  1,  49),   S(  6,  56),   S( 23,  59),   S( 21,  44),   S(  9, -14),   S(-14, -26),   S(-32, -46),   S(-32, -51),
            S(  8,  74),   S( 34,  62),   S( 42, 111),   S( 27,  47),   S( -6,  -7),   S(  0, -40),   S( 18, -22),   S(-32, -53),
            S( 51,  77),   S( 78, 206),   S( 68, 125),   S( 24,  55),   S(  6,  12),   S(  1, -38),   S(  3, -34),   S(-11, -55),
            S( 47, 130),   S( 65, 255),   S(108, 223),   S( 29,  65),   S(  0,  10),   S( -6, -54),   S(-11, -53),   S(-18, -96),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34, -15),   S(  1,  17),   S( -2,  12),   S( -6,  -9),   S( -6,  -9),   S(-27, -17),   S(-38, -32),   S(-43,   0),
            S(-21, -34),   S(-24,   8),   S( -7,  14),   S(  9,  19),   S(-35,   8),   S(-14,   5),   S(-50, -42),   S(-27, -12),
            S( 15,  25),   S(  1, -10),   S(  3,  15),   S( -3,  39),   S( -6,  23),   S(-12, -10),   S(-28, -49),   S(-10, -46),
            S(  6,  31),   S( 42,  54),   S( 34,  78),   S(  5,  72),   S( 22,  46),   S(  6,  -4),   S( 26, -35),   S( 21, -54),
            S( 24,  45),   S( 88, 121),   S(104, 164),   S(121, 195),   S( 42,  86),   S(  4,  -4),   S( 20, -32),   S( 21, -68),
            S( 19,  87),   S( 82, 161),   S(103, 229),   S(111, 237),   S( 55,  95),   S( -3,  -1),   S(  5,   2),   S( 10, -28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31, -36),   S(-30, -26),   S(  0, -23),   S( -3, -12),   S( -7,  -8),   S(-37,   7),   S(-38,  -8),   S(-13,  32),
            S(-44, -13),   S(-28, -22),   S(-25, -54),   S(-17,  12),   S(  2,  40),   S( 11,  10),   S( -7,   3),   S(-32,  15),
            S(-21, -40),   S(-23, -23),   S(-20, -19),   S(-11,  29),   S(  7,  46),   S(-19,  13),   S(-17,   0),   S( 18,  19),
            S( 23, -35),   S( 30, -11),   S( 21, -14),   S( 19,  50),   S( 11, 119),   S( 11,  70),   S(  5,  33),   S( 25,  21),
            S( 15, -16),   S( 20,  -6),   S( 40,  38),   S( 49, 141),   S( 73, 200),   S( 77, 144),   S( 52,  70),   S( 26,  35),
            S( 23,  -3),   S( 23,  -2),   S( 48, 112),   S( 50, 109),   S( 69, 249),   S( 57, 173),   S( 45, 112),   S( 24,  43),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -53),   S(-35, -43),   S(-14, -41),   S( -4,  -6),   S( 16,  29),   S( -1,  33),   S( -7,  -6),   S( 22,  57),
            S( -8, -86),   S(-36, -36),   S(-16, -58),   S( -3,   8),   S(-27,  20),   S(  7,  45),   S(  6,  24),   S(-25,  25),
            S( -7, -67),   S(-13, -85),   S(-22, -58),   S( -6,  29),   S(  9,  22),   S( 43,  44),   S( -3,  66),   S( 19,  69),
            S(  2, -55),   S(-45, -66),   S( -5, -25),   S( 18,  31),   S(  5,  39),   S( 51,  58),   S(  8, 102),   S( 12,  64),
            S(-14, -79),   S(-11, -77),   S( -3,  -6),   S( 16,   8),   S( 46, 128),   S( 82, 110),   S( 46, 235),   S( 71,  80),
            S(  5, -32),   S(  3,  -8),   S(  7,   4),   S(  9,  36),   S( 22,  69),   S( 75, 248),   S( 30, 213),   S( 50,  61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S( -7, -27),   S(  6,   5),   S(-47,  18),   S(-37, -30),   S(-21, -23),   S(  6, -24),   S(-58, -67),   S(-29, -12),
            S(-45,  71),   S( 33, -53),   S(-27,  -9),   S(  5, -40),   S( -6, -25),   S(  3, -41),   S(-11, -18),   S(-54, -29),
            S(-10,  86),   S( -6, -37),   S(  4, -15),   S(-24,  20),   S(  7,  13),   S(-33, -10),   S( -3, -53),   S(-51, -25),
            S( 27, -45),   S( 47,  18),   S( 20,  23),   S( 22,  27),   S( 14, -17),   S( -4, -12),   S( 23, -40),   S( -2, -16),
            S(  3, -47),   S( 55, -28),   S( 16, -15),   S( 64, -15),   S( 33, -15),   S( 25,  30),   S(  7, -18),   S(-81, -17),
            S( -4, -39),   S( 11,  28),   S( 38,  10),   S( 63, -18),   S( 43, -82),   S( 24,  12),   S(-24, -22),   S(-43, -29),
            S(-17, -21),   S( 17, -37),   S( 30, -21),   S( 66, -63),   S( 62, -40),   S( -4, -63),   S( 13, -43),   S( 21, -39),
            S(-68, -66),   S(-27,   8),   S(-23, -21),   S(  2, -51),   S(-13, -12),   S( 14,  12),   S(  0,   3),   S( 23, -16),

            /* knights: bucket 1 */
            S(-52,  -5),   S(-46,  77),   S( 22,  22),   S(-34,  59),   S( -9,  43),   S(-32,  30),   S(-35,  46),   S(-24,  -3),
            S( 43,   8),   S( 27,   6),   S(  0,   0),   S(-12,  29),   S(  5,  15),   S(  8,  16),   S( 25, -34),   S(-24, -31),
            S(-25,  27),   S( -4,  12),   S(  0,  14),   S( 15,  20),   S( 14,  17),   S(-23,  20),   S(-14,   1),   S(-25,  11),
            S(  5,  23),   S( 77,  16),   S( 17,  41),   S( 21,  27),   S( 13,  25),   S( -3,  30),   S( 36, -11),   S(  3,  20),
            S(  5,  30),   S(  2,  18),   S( 41,  12),   S( 41,  21),   S( 34,  20),   S( 16,  23),   S( 13,  19),   S( 11,  -3),
            S(  6,  13),   S( 26,  21),   S( 30,  33),   S( 38,   3),   S( 42,  16),   S( 51,  36),   S( 66,  -5),   S(-13, -20),
            S( 42,   0),   S( 21,  -2),   S(  8, -55),   S( 35,  30),   S(  1, -21),   S( 32, -28),   S(-50,  -6),   S(-12, -24),
            S(-80, -67),   S(-17, -21),   S(-24, -11),   S(-18,  -4),   S(-12,   5),   S(-10, -23),   S(-25, -50),   S(-75, -87),

            /* knights: bucket 2 */
            S(-34,   8),   S(  1,  29),   S(-37,  50),   S(-27,  52),   S(-27,  43),   S(-34,  65),   S(-19,  34),   S(-58,  17),
            S( -8, -16),   S(-14,   9),   S( -4,  19),   S(-12,  31),   S(  2,   7),   S( -5,  26),   S(-38,  46),   S(-53,  72),
            S(-27,  24),   S(-10,  14),   S(-11,  32),   S( 18,  23),   S( 12,  17),   S( -5,  14),   S( -8,  33),   S(-39,  50),
            S( -8,  37),   S(-17,  29),   S(  5,  42),   S(  6,  42),   S(  2,  48),   S(-11,  41),   S(-12,  45),   S(-12,  56),
            S( 10,  22),   S(-19,  40),   S(-21,  52),   S(-27,  66),   S( -2,  49),   S(-22,  59),   S( -8,  44),   S(-27,  31),
            S(-22,  36),   S( 10,  31),   S(-26,  50),   S(-15,  47),   S( -9,  47),   S(-16,  51),   S(-37,  28),   S( 23,   2),
            S(  0,  19),   S(-48,  20),   S(-22,   2),   S(-41,  34),   S(-10,  27),   S( 20,   6),   S(-19,  10),   S(-53,  25),
            S(-154,  13),  S( 16,  15),   S(-98,  26),   S(-35,  -2),   S( 17,  -6),   S(-25,  16),   S(  9, -19),   S(-185, -34),

            /* knights: bucket 3 */
            S(-80,  29),   S(  0, -18),   S(-47,  18),   S(-27,  28),   S(-17,  29),   S(-18,  12),   S(  0,  -1),   S(-33,  15),
            S(-17,  19),   S(-37,   9),   S(-23,  -1),   S(  3,  18),   S( 11,  17),   S(-25,   5),   S(-28,  11),   S(-36,  72),
            S(-16, -21),   S(-10,   7),   S( -9,  18),   S( 12,  18),   S( 11,  27),   S(  4,  18),   S( -9,   9),   S( -5,  39),
            S(-16,  15),   S(  2,  24),   S(  1,  50),   S( 12,  44),   S( 17,  49),   S( 10,  45),   S( 19,  26),   S(  5,  38),
            S(  1,  20),   S( -3,  28),   S( 15,  26),   S( 10,  61),   S( 13,  50),   S( 11,  66),   S( 25,  46),   S(-12,  25),
            S( 19,  -2),   S( 20,   0),   S( 57,   2),   S( 58,  12),   S( 69,  -5),   S( 82,  -3),   S( 16,  29),   S(  1,  40),
            S(  7,  -5),   S( -7,  28),   S( 52, -16),   S( 41,   0),   S( 55, -14),   S( 63, -25),   S( 43, -56),   S( 45, -21),
            S(-132,  26),  S(-36,   9),   S(  3, -10),   S( 11,   4),   S(  8,   1),   S(-17, -13),   S( -6, -19),   S(-62, -34),

            /* knights: bucket 4 */
            S( 11,  16),   S(-70, -23),   S( 53, -12),   S( 17,   3),   S(-11,  -8),   S(-46, -43),   S( 17, -66),   S(-33, -45),
            S( 36,  38),   S( -8,  39),   S(  7, -17),   S( 29, -26),   S( 56, -59),   S(-20, -31),   S( 19, -20),   S( 16, -12),
            S(-22,   3),   S( 29,  28),   S( 37, -16),   S( 33,  -9),   S( 19,  -7),   S(-41,  23),   S(-34, -42),   S(-30, -86),
            S( -3,  30),   S( 16, -33),   S( 80,  -2),   S( 35,   7),   S( 32,  -2),   S(114, -44),   S( 16, -61),   S(  4, -38),
            S( 44,  16),   S(-63,  55),   S( 57,  16),   S( 53,  15),   S( 52,  10),   S( -5,  -1),   S(-10, -41),   S(  5, -13),
            S( -3,   9),   S(-46,   2),   S( 81,   8),   S( 43,  -8),   S(  3, -10),   S( 44,   1),   S( 10,  19),   S( -9, -21),
            S(  7,  22),   S( -3,  16),   S(  5,  15),   S( 24,  27),   S( 18, -36),   S( 14, -38),   S(  7,  -6),   S(-20,   3),
            S(  9,  16),   S(  1,   4),   S( 18,  23),   S( -4, -18),   S(  1, -23),   S( 17,  27),   S( -5, -14),   S(  2, -10),

            /* knights: bucket 5 */
            S(  7,  -9),   S(-22,  34),   S( 54,  14),   S( 38,  45),   S( 48,  -2),   S( 39, -19),   S(-18,   3),   S(-13,  -6),
            S(  6, -23),   S(  5,  34),   S( 29,  15),   S( 10,  20),   S( 30,  21),   S( 40,   2),   S( 46,   1),   S( -2,  -7),
            S( -2,  17),   S( -5,  17),   S( 64,   2),   S( 93,   3),   S( 30,  17),   S( 12,  20),   S(  9,  13),   S( 16,  -4),
            S( 46,  40),   S( 19,  39),   S( 70,  21),   S(  9,  44),   S( 36,  26),   S( 45,  18),   S( 56,  18),   S(  9,  25),
            S( 39,  32),   S( 53,  18),   S( 80,  35),   S( 84,  36),   S(117,  19),   S( 54,  31),   S( 20,  29),   S( 34,  22),
            S( 11,  18),   S( 17,  44),   S( 52,  24),   S( 36,  52),   S( 41,  39),   S( -3,  65),   S( 12,  -8),   S(-11,  14),
            S(  9,  53),   S(-11,  47),   S( 43,  40),   S( 25,  64),   S(-18,   7),   S( 12,  30),   S( 22,  47),   S(  6,  11),
            S(  7,  10),   S( -7,   7),   S( 11,  48),   S( -6, -11),   S( 15,  39),   S( -6,  15),   S(  9,  22),   S(-11,   8),

            /* knights: bucket 6 */
            S( 10, -13),   S(-18, -11),   S( 51,  16),   S(-12,  13),   S(-45,  41),   S( 21,  35),   S(-27,  21),   S( -9,  41),
            S(-22, -41),   S( 46,  -5),   S( 39,  -4),   S( -8,  22),   S(-20,  38),   S( 63,  31),   S( 38,  14),   S(-21,  11),
            S( 17, -30),   S( 18, -15),   S( 31,  10),   S( 50,  13),   S( 18,  23),   S( -4,  31),   S( 42,  38),   S( 15,  48),
            S( 28,   5),   S( 55,  -8),   S( 74,  24),   S( 79,  17),   S( 56,   9),   S( 68,  24),   S(  3,  46),   S( 21,  63),
            S( -5,  30),   S( 88, -15),   S( 75,  22),   S( 86,  25),   S(102,  22),   S(122,  25),   S( 43,  38),   S( 26,  32),
            S( -4,  19),   S( 62,  -8),   S( 59,  20),   S( 78,  30),   S( 89,  46),   S( 48,  33),   S( 45,  31),   S( 47,  55),
            S(  1,  12),   S( 34,  26),   S(-43,  47),   S( 36,  29),   S( 12,  23),   S( 26,  57),   S( 17,  54),   S(-26,  32),
            S(-51, -31),   S( 17,  33),   S( 21,  18),   S( -5,  33),   S( 24,  18),   S( 24,  40),   S( 34,  77),   S( 16,  21),

            /* knights: bucket 7 */
            S(-37, -51),   S(-200, -31),  S(-80, -48),   S(-75, -22),   S(-31, -17),   S(-45, -19),   S(-28,   5),   S(-23, -29),
            S(-47, -97),   S(-10, -44),   S(-57, -26),   S(  3, -28),   S(-24,   8),   S( 39, -33),   S( 27,  43),   S( 22, -14),
            S(-45, -73),   S(-66, -20),   S(-43,  -8),   S( 19, -25),   S( 30, -14),   S( 49, -21),   S( 21,  22),   S( 14,  40),
            S(-73, -23),   S( 24, -27),   S( 18,  -2),   S( 73,  -9),   S( 77,  -9),   S( 19,  13),   S( -5,  17),   S( -4,  25),
            S(-73, -18),   S( -5, -48),   S( 84, -24),   S(103, -16),   S(122, -13),   S(100,  10),   S( 91,  12),   S( 69,  33),
            S(-15, -52),   S( 20, -13),   S(-10,  -7),   S( 71, -20),   S( 67,   6),   S(102,  -9),   S(106, -25),   S( -9,  28),
            S(-38, -28),   S(-60, -17),   S(  3, -31),   S( 39,  21),   S( 17,  31),   S( 59,  -7),   S(  8,  38),   S( 23,  -8),
            S(-65, -62),   S( -4, -14),   S(-29, -30),   S( 27,   9),   S(-11, -16),   S(  5,  -4),   S(  8, -14),   S( -7, -15),

            /* knights: bucket 8 */
            S( -4,  -3),   S( -4,  -8),   S( -2,   8),   S( -2, -11),   S(-12, -66),   S( -1, -14),   S(  4,   8),   S( -9, -32),
            S(  2,   0),   S( -7,  -6),   S(-14, -36),   S(-15, -32),   S(-33, -41),   S(-17, -50),   S(-15, -53),   S(-17, -41),
            S(  5,  18),   S( -5, -20),   S(  1,  -8),   S(-13, -14),   S( -2, -35),   S( -9,  10),   S(-14, -39),   S( -5, -43),
            S(-30,  15),   S(-15, -20),   S(  8,  24),   S(-11,  39),   S( 26,  18),   S( 10, -13),   S( -7, -24),   S( -5, -38),
            S( 42,  88),   S( -7,  24),   S( 12,  16),   S( 28,  43),   S( 12,  28),   S(-12, -23),   S(  6, -33),   S(  2,  15),
            S( 22,  73),   S(  0,  -5),   S( 30,  20),   S( 37,   9),   S( -8,   8),   S(-11,  -8),   S( -6, -50),   S(  0,  17),
            S(  7,  14),   S(  1,  -4),   S(  9,   6),   S(  2,  -5),   S( 10,  17),   S(  2,   2),   S(  1,  19),   S( -4, -14),
            S(  7,  20),   S(  9,  22),   S( -5,   6),   S(  4,  13),   S( 12,  45),   S( -1, -12),   S( -2,  -7),   S( -5, -15),

            /* knights: bucket 9 */
            S( -6, -31),   S(-25, -33),   S(-28, -81),   S( -7, -37),   S(-28, -80),   S(-15, -45),   S(  2, -12),   S( -2, -22),
            S(-12, -45),   S( -7,  23),   S(  7,  -8),   S(-14,  -8),   S( -1, -27),   S(-11, -50),   S( -1,  -4),   S(-16, -53),
            S( 16,  28),   S(  1,  -5),   S(  5, -23),   S( -2,  -6),   S( 10,   8),   S(-32, -21),   S(-17,   1),   S(-10,  -6),
            S(  5,  12),   S(-11, -30),   S(  2,   6),   S( 11,   0),   S( 18,  13),   S(  1,  20),   S(-19, -49),   S(  6,   0),
            S(  8,  42),   S(-20,  -9),   S(  7,  39),   S( -3,   6),   S( 22,  29),   S(  7,   0),   S( -6, -37),   S(  1,  -9),
            S(  2,  10),   S( 10,  19),   S( 26,  23),   S(  1,  12),   S( 31,  24),   S( 12,  21),   S(  7,  20),   S(-10, -50),
            S(  6,  22),   S( -6,   6),   S( 19,  43),   S(  2,  36),   S(  8,  31),   S(-12, -24),   S(  5,  20),   S(  5,  30),
            S(  6,  18),   S( -3,  -7),   S( 11,  17),   S( 16,  60),   S( 10,   8),   S(  1,  13),   S(  6,  20),   S( -2, -13),

            /* knights: bucket 10 */
            S(-14, -51),   S(-23, -71),   S(-12, -56),   S(-22, -28),   S( -8, -34),   S(-25, -68),   S( -6, -14),   S(  2,   8),
            S(-10, -61),   S( -8, -56),   S( -6, -37),   S(-17, -23),   S(-26, -55),   S(-13, -50),   S(  0,  -1),   S(  2,  -7),
            S(-15, -77),   S(-13, -58),   S(-26, -23),   S(-24, -19),   S( 13,   6),   S(-22, -29),   S(-25, -32),   S(-12, -26),
            S( -1, -30),   S(-11, -45),   S( -2, -32),   S( 16,   2),   S( 28,   4),   S( -1, -20),   S( -5,   0),   S( 14,  41),
            S( -9, -48),   S(-10, -21),   S(  7,   3),   S( 58,  11),   S( 14,  11),   S( 25,   5),   S(  7,  14),   S( 13,  44),
            S( -9, -12),   S(-12, -50),   S(  2, -10),   S(  9,  37),   S( 24,  40),   S( 10,  29),   S( 14,  42),   S( 17,  81),
            S( 12,  33),   S(-10, -32),   S(  6, -10),   S( 32,  42),   S( 14,  38),   S( 14,  31),   S(  4,   5),   S( 11,  44),
            S( -1,  -7),   S(  7,  26),   S( -5,  -9),   S( -1,   2),   S(  6,  34),   S(  3,  22),   S( 12,  41),   S( -2,  -6),

            /* knights: bucket 11 */
            S(  1,   4),   S( -5,  -1),   S( -9, -56),   S(-11, -39),   S(-19, -27),   S(-22, -25),   S( -2, -12),   S( -7, -13),
            S(-22, -43),   S( -9, -25),   S(-19, -66),   S(-17, -31),   S(-15,  17),   S(-29, -36),   S( -9, -22),   S( -7,  -2),
            S(-23, -96),   S(-29, -85),   S(-27, -15),   S(  3,  -1),   S(-26,   7),   S(-16,  10),   S( 14,  -8),   S( -3,  11),
            S( -4, -12),   S( -3, -33),   S(-22,  -4),   S( 12,  33),   S( 12,  18),   S(  9,  25),   S(-10,   3),   S(  9,  15),
            S(-14, -38),   S(-27, -43),   S(-17, -14),   S( 16, -18),   S( 20,  19),   S( 29,  29),   S(  8,   2),   S( 23,  74),
            S(-10, -13),   S(-11, -29),   S(  3,  20),   S( 36,  32),   S(  9,   1),   S( 52,  29),   S(  3, -12),   S(  9,  38),
            S(  6,  28),   S( -8, -19),   S( -1,  -5),   S( -1,   6),   S(  9,  52),   S(-10,  32),   S( 11,  22),   S( 12,  77),
            S(  1,  10),   S( -7, -38),   S( 10,  33),   S(  2,   2),   S(  0,   1),   S( 11,  22),   S(  2,  -4),   S( -2,  12),

            /* knights: bucket 12 */
            S( -3, -11),   S( -7,  -7),   S( -2, -17),   S( -4,  -6),   S( -4,  -3),   S( -5, -16),   S(  2,  -6),   S( -1,  -9),
            S( -2,  -7),   S(  4,  10),   S(  1,  -4),   S(  1,  -8),   S(  7,  16),   S( -8, -39),   S( -2, -12),   S(  3,   1),
            S( -3,  -8),   S(  0,   0),   S( -2,   1),   S( -6,  -3),   S(  4,  -3),   S( -4, -24),   S( -4, -26),   S(-12, -41),
            S( -6, -12),   S( -3,  -9),   S( -5, -22),   S( 12,  47),   S( -6, -16),   S(  4,  20),   S(  0,   0),   S(  0,  -9),
            S(  9,  16),   S(  6,   5),   S(-18, -37),   S( -2,  13),   S( -4, -19),   S( 12,  27),   S( -4, -14),   S(  4,  10),
            S( -4,   0),   S(-16, -30),   S( -2,  27),   S(  8,  21),   S( -3, -19),   S( -2,  -8),   S( -1, -12),   S(  2,  15),
            S(  4,  18),   S(-16, -16),   S( -7,  -3),   S( -4,  14),   S(  0,   1),   S( -5, -21),   S(  2,  11),   S( -1,  -3),
            S(  4,  12),   S( -6,   0),   S( -1,  -3),   S(  1,  20),   S( -5,  -9),   S( -3, -12),   S(  1,   3),   S(  0,   1),

            /* knights: bucket 13 */
            S( -5, -19),   S( -1,   1),   S( -2,  -9),   S( -3,  -4),   S( -1,  -4),   S( -2, -14),   S( -6, -24),   S(  3,  10),
            S( -4, -10),   S(  3,  16),   S( -2, -38),   S( -1, -16),   S( -5, -24),   S( -4, -24),   S( -1, -10),   S(  3,  -4),
            S( -5, -25),   S( -3,   2),   S(  4,   8),   S(  2,  -2),   S(-12, -23),   S( -4,  -4),   S(  4,   8),   S( -7, -30),
            S(  0,  10),   S( 10,  33),   S(  6,   4),   S( -8, -12),   S(  2,   4),   S( 16,  38),   S( -2, -20),   S( -3,   0),
            S(  8,  21),   S( -3,  27),   S( 22,  43),   S( 20,  38),   S(  3,  -5),   S(  2,  14),   S( -6, -18),   S(  3,  10),
            S(  2,  22),   S( 15,  27),   S( -1,  57),   S(-21,   2),   S(  7,  43),   S( -3, -16),   S(  2,  13),   S( -4,  -8),
            S( -3, -16),   S(  4,  31),   S( 10,  17),   S(  2,  12),   S( 11,  44),   S( -8, -15),   S( -1,   2),   S( -8, -16),
            S( -2,  -6),   S( -3,  31),   S( -9, -10),   S(  9,  37),   S(  0,  12),   S(  0,   0),   S(  0,   5),   S(  0,  -4),

            /* knights: bucket 14 */
            S(  1,  -5),   S( -9, -39),   S( -4,  -1),   S( -4, -17),   S(-14, -56),   S( -5, -31),   S( -1,  -5),   S( -1,  -6),
            S(  0,   0),   S(  0,   4),   S(-16, -66),   S( -7, -31),   S(  0, -16),   S( -2, -13),   S(  2,   8),   S(  0,  -6),
            S( -8, -20),   S( -3, -16),   S(-15, -36),   S(  6,  14),   S(  1,   9),   S(  1,   3),   S(  4,   9),   S(  1,   7),
            S(  1,   9),   S( -9, -37),   S(-11, -37),   S(-15, -48),   S(  5, -13),   S(  8,   2),   S(  3,   7),   S( -9,  -4),
            S(  0,   7),   S( -3, -16),   S( 12,  61),   S( -8, -22),   S( -3,   1),   S(  3, -15),   S(  6,   5),   S( -7, -23),
            S( -4, -15),   S(  8,  17),   S( -1,   7),   S( 14,  18),   S( 13,  24),   S( -6,  13),   S(  7,  41),   S(  3,  18),
            S(  2,   9),   S(  0,   0),   S( 16,  27),   S(  4,  23),   S(-14,  -4),   S(  6,  20),   S( -4,  16),   S(  3,  19),
            S(  0,  -1),   S(  3,  18),   S( -2,   2),   S( 15,  47),   S(  3,  10),   S(  1,  20),   S(  2,  29),   S(  0,   3),

            /* knights: bucket 15 */
            S( -3, -15),   S( -1,  -6),   S(  1, -22),   S(-10, -26),   S( -1,   4),   S(  6,   6),   S(  3,   8),   S(  1,   3),
            S( -2, -19),   S(  0,  -6),   S( -1, -14),   S( -2,  -9),   S( -6, -17),   S(  1,   7),   S(  0,   0),   S( -1,  -6),
            S( -9, -33),   S( -4, -18),   S( -3, -10),   S(-10, -40),   S(-15, -34),   S(  4,   2),   S( -1,  -2),   S( -2, -16),
            S( -3,  -5),   S(-10, -38),   S(  1, -17),   S(  2,   4),   S(  3, -19),   S( 12,  17),   S(  9,  33),   S( -9, -18),
            S( -4,  -7),   S( -7, -20),   S( -6, -33),   S(  2,   2),   S(  9,   5),   S(  2,   9),   S(-19, -56),   S( -5,  -4),
            S( -1,   3),   S(  0,   1),   S( -8, -37),   S(  3,   6),   S(-11, -37),   S(  4,  46),   S(  2,  -2),   S(  1,  29),
            S( -3, -11),   S( -4, -10),   S(  2,   7),   S( -5,  -7),   S( -3, -10),   S(  7,  34),   S( -5, -13),   S( -1,  12),
            S(  0,   1),   S(  1,   2),   S(  1,   5),   S(  4,   9),   S(  0,   2),   S( -9, -18),   S(  4,  22),   S(  1,   5),

            /* bishops: bucket 0 */
            S( 31,  32),   S(  3,   1),   S( 55,  11),   S(  6,  15),   S( -6, -16),   S( 11, -11),   S( 36, -44),   S( -7, -36),
            S( 48, -28),   S( 77,  23),   S( 46,   6),   S( 16,  17),   S(-10,  36),   S( -3,   2),   S(-17,   7),   S(  8,  -6),
            S( 19,  12),   S( 46,  15),   S( 30,  52),   S(  6,  55),   S( 10,  39),   S(-16,  66),   S(  7,  10),   S( 20, -60),
            S( 17,  -1),   S( 51,   3),   S( 34,  39),   S( 37,  40),   S(-14,  68),   S( 38,   7),   S(  1,  21),   S(-12,  15),
            S( 19,  12),   S( 32,  18),   S(  3,  58),   S( 42,  47),   S( 35,  41),   S(-14,  61),   S( 19,  10),   S(-53,  39),
            S(-40,  60),   S(-25,  72),   S( 48,  40),   S( 58,  44),   S( 48,  60),   S(-12,  40),   S(-24,  41),   S( -4,  41),
            S(-42,  66),   S( -2,  54),   S( 11,  45),   S(-34,  71),   S(-45,  14),   S( 58,  31),   S( 40,  15),   S(-16,  11),
            S(-55, -24),   S( -2,  56),   S(-11,  48),   S( -9,  20),   S(  2,  17),   S( 19,  47),   S(-22,  86),   S(-28,  28),

            /* bishops: bucket 1 */
            S( 33,  55),   S(-10,  44),   S( 16,  19),   S( -4,  20),   S(  5,  12),   S(  2,  25),   S(-11,  44),   S(-63,  52),
            S( 25, -13),   S( 38,  18),   S( 48,  12),   S( 28,  30),   S( -3,  20),   S( 21,   6),   S(-16,  18),   S( 10, -11),
            S( 40,  15),   S( 12,  20),   S( 36,  46),   S(  4,  39),   S(  9,  40),   S(-13,  41),   S( 24,  21),   S(  7,  -9),
            S( 51,   4),   S( 26,  29),   S(  5,  37),   S( 23,  47),   S(-15,  70),   S( 19,  22),   S(  4,  25),   S( 26,   1),
            S( 31,  33),   S( -8,  60),   S( 12,  47),   S( -3,  58),   S( 14,  39),   S(-18,  46),   S( 26,  14),   S(-25,  33),
            S(-30,  62),   S( 12,  58),   S(  1,  76),   S( 57,  20),   S( 21,  58),   S( 26,  41),   S(-19,  56),   S( 30,  37),
            S(  0,  41),   S( 28,  34),   S( 12,  36),   S( 29,  53),   S( 10,  62),   S(-21,  53),   S(-28,  56),   S(-28,  28),
            S( 12,  58),   S(-26,  43),   S(-24,  36),   S(-30,  45),   S( -7,  55),   S( -5,  32),   S(-50,  39),   S(-36, 109),

            /* bishops: bucket 2 */
            S(  2,  28),   S(  6,  27),   S(  2,  25),   S(-18,  35),   S(-15,  37),   S(-18,  30),   S(-45,   4),   S(-32,  41),
            S(-18,  26),   S( 11,  20),   S( 10,  28),   S(  0,  39),   S( -5,  34),   S(  1,  15),   S(  9,  -5),   S( 10, -46),
            S(  0,  17),   S( -6,  35),   S(  6,  61),   S( -9,  60),   S( -8,  53),   S(  0,  49),   S( -6,  39),   S(-11,   1),
            S( -6,  39),   S(-32,  61),   S(-21,  60),   S( -7,  70),   S( -4,  68),   S(  0,  45),   S(  8,  38),   S( -1,  18),
            S(-17,  46),   S(-20,  50),   S(-28,  62),   S(-22,  62),   S(-26,  83),   S( -8,  60),   S( -2,  37),   S(-23,  32),
            S(-10,  41),   S(-31,  55),   S( -6,  65),   S(-38,  64),   S(  2,  43),   S(-44,  86),   S(  5,  61),   S(-16,  54),
            S(-17,  32),   S(-17,  53),   S(-27,  79),   S( 33,  32),   S(-33,  69),   S(-73,  44),   S(-62,  67),   S(-36,  51),
            S(-81, 108),   S(-48,  69),   S(-72,  63),   S(-53,  57),   S(-80,  51),   S(-48,  61),   S(  9,  31),   S(-68,  65),

            /* bishops: bucket 3 */
            S(-15,  49),   S(  6,  36),   S( 15,  35),   S( -4,  43),   S( -2,  29),   S( 40,   7),   S( 34, -15),   S( 47, -41),
            S( -6,  24),   S(  1,  45),   S( 15,  25),   S(  3,  59),   S(  8,  41),   S( -1,  45),   S( 46,  32),   S( 31, -10),
            S( 15,  40),   S( -2,  54),   S(  6,  75),   S( 10,  58),   S(  4,  79),   S(  7,  67),   S( 18,  52),   S( 42,   5),
            S( 17,  15),   S( -8,  65),   S( -4,  79),   S(  6,  86),   S( 13,  77),   S( 13,  56),   S(  8,  60),   S( -2,  25),
            S(-11,  38),   S(  8,  54),   S(  5,  60),   S( 17,  81),   S( 16,  70),   S( 30,  62),   S(  5,  58),   S(  6,  63),
            S( 11,  35),   S(  7,  51),   S( 17,  68),   S( 26,  48),   S(-12,  66),   S( 51,  62),   S( 23,  48),   S(  3,  67),
            S(-17,  47),   S(-21,  68),   S( 31,  38),   S( -5,  61),   S(  1,  45),   S(-24,  51),   S( -9,  82),   S( -6,  74),
            S(-44,  94),   S(-35,  67),   S( 38,  48),   S(-11,  59),   S(  7,  61),   S(-60,  98),   S( 12,  51),   S( 85,  13),

            /* bishops: bucket 4 */
            S( -3, -29),   S(-29,   5),   S(-51,   7),   S(-58,  24),   S( -2,  -1),   S(-54,  17),   S( -1,  -1),   S(  1, -43),
            S(-13, -19),   S(  6,   8),   S( -5,  32),   S( -9,  -4),   S(-31,   5),   S( 85,  -9),   S(-21,  -9),   S( -3, -12),
            S( -4,  26),   S(-37,  22),   S(  0,  24),   S(-10,  27),   S( 47,  -3),   S( 36,  14),   S( 12, -26),   S(-86, -22),
            S(-16,  25),   S( 13,  21),   S( 44,  23),   S( 61,  20),   S( -2,  54),   S( 51,   5),   S( 13,  27),   S(-17, -15),
            S( 19,  13),   S(-12,  38),   S( 12,  51),   S(  9,  34),   S( 28,  13),   S(-35,  46),   S(-37,  12),   S(-16,  19),
            S(  7,  15),   S( 53,  12),   S(-18,  32),   S( 24,  22),   S( 41,  21),   S( 20,  21),   S( -6, -16),   S(-32,   0),
            S(-21,  34),   S( 35,  29),   S(  0,  22),   S(  3,  45),   S( -5,  32),   S(  0,  23),   S( 12,  16),   S( -6,  -4),
            S(  2,   5),   S(-19,  -3),   S(  0,  12),   S(-25,  -1),   S(  0,  30),   S( -7,   1),   S(  2,   1),   S( -3,   9),

            /* bishops: bucket 5 */
            S(  5,   6),   S( -3,  22),   S(-39,  30),   S(-29,  22),   S( -3,  18),   S(-21,  22),   S(-34,  41),   S(-11,  19),
            S(-29,  46),   S( -4,  31),   S(-58,  57),   S( -7,  30),   S( -4,  29),   S( -1,  15),   S(-60,  24),   S(-22,  18),
            S(-31,  47),   S(-44,  55),   S( 45,  34),   S( 21,  31),   S( -4,  43),   S(  0,  28),   S(-23,  35),   S(-15, -13),
            S( 36,  20),   S( 29,  37),   S(-15,  55),   S( 17,  44),   S( 52,  37),   S( 29,  30),   S( -1,  32),   S(  8,  33),
            S( 40,  51),   S( 25,  19),   S(102,  23),   S( 95,  25),   S( 66,  24),   S( 62,  29),   S( 44,  44),   S(-31,  35),
            S(-15,  43),   S( 45,  36),   S( 68,  41),   S( 23,  48),   S(-11,  50),   S(  1,  22),   S( -8,  36),   S( 18,  37),
            S(  0,  50),   S(-31,  38),   S(-32,  49),   S( 13,  42),   S( 18,  57),   S( 22,  53),   S( -1,  39),   S( -6,  10),
            S( -6,  34),   S( 15,  40),   S(  4,  32),   S(  2,  36),   S(  4,  36),   S(  5,  40),   S( -2,  64),   S( -9,  25),

            /* bishops: bucket 6 */
            S(-23,  60),   S(-38,  44),   S(-32,  30),   S(-22,  22),   S(-36,  38),   S(-51,  46),   S(-17,  54),   S( -3,  30),
            S( 38,   3),   S( -5,  24),   S(  6,  23),   S(  8,  35),   S( 24,  21),   S( -9,  21),   S(-129,  68),  S( 55,  34),
            S(  6,   8),   S(-43,  43),   S( 17,  54),   S( 20,  33),   S( 50,  17),   S( 43,  20),   S(  3,  48),   S(-104,  47),
            S( 32,  30),   S( -4,  47),   S( 23,  42),   S( 52,  40),   S( 38,  43),   S( 32,  29),   S(  1,  61),   S( -7,   7),
            S(-36,  50),   S( 18,  39),   S( 72,  23),   S( 50,  34),   S( 83,  48),   S( 78,  23),   S( 49,  35),   S( 12,  45),
            S(  5,  21),   S( 11,  30),   S( 16,  42),   S( 14,  52),   S( 23,  48),   S( 41,  50),   S( 55,  36),   S(-13,  61),
            S(  9,  26),   S( 18,  38),   S( 14,  33),   S(  5,  37),   S(  8,  44),   S( -6,  36),   S( 25,  53),   S(-19,  35),
            S( -1,  48),   S(  1,  49),   S(-14,  35),   S(-12,  51),   S(-14,  45),   S(  7,  51),   S( -1,  25),   S(-24,  34),

            /* bishops: bucket 7 */
            S( 10,   1),   S(-11,  10),   S(-50,   3),   S( -1, -17),   S(  0, -19),   S(-67,  21),   S(-56, -24),   S(-42,  -6),
            S(-24, -21),   S(-55,   9),   S(-18,   3),   S( 11,   5),   S( -8,  11),   S(-59,  25),   S(-17,  -7),   S(-19,   8),
            S(-37,   6),   S( 23, -11),   S( 27,  17),   S( 26,  13),   S(-26,  25),   S(-16,  18),   S(  9,  21),   S(  8,   9),
            S(-47,  24),   S( 18,  26),   S( 64,   5),   S( 72,  13),   S(117,  12),   S(  2,  25),   S(  8,  38),   S( 33,  15),
            S( 24, -12),   S(-17,   6),   S( 20,  20),   S(105,  -9),   S(103,   3),   S( 93,  20),   S( 25,  30),   S( 56,  -7),
            S( -9, -17),   S( 15,   4),   S(  5,   5),   S( 50,   3),   S( 32,   6),   S(107,  20),   S( 56,  10),   S(  6,  24),
            S(  6,  -3),   S( -9,  15),   S( 23,   0),   S( 36,   6),   S(  8,   9),   S( 40,  -9),   S( 26,  18),   S(-12,  23),
            S(  6,  15),   S(-28,  34),   S(-33,  17),   S( -2,   9),   S( 11,  11),   S( 27,  18),   S( 30,  26),   S( 22,  56),

            /* bishops: bucket 8 */
            S( -6, -41),   S(-12, -58),   S(-57, -70),   S( -2,  -7),   S( 10,  -4),   S(-11, -34),   S(  2,  10),   S(-11,  -7),
            S(  1,  -6),   S(-21, -67),   S(-10, -22),   S(-16, -35),   S( 24,  -9),   S(-19, -38),   S(-15, -62),   S(  3, -41),
            S(-13, -20),   S(-10,  -3),   S( -9,   9),   S( 10, -39),   S(  3, -22),   S(  7,  -8),   S(  8, -24),   S(-36, -43),
            S(  4,  38),   S( 11,   8),   S(  6,  -4),   S(  0,  -3),   S(  8,  -5),   S( 21, -19),   S(  1,  -5),   S( -6, -31),
            S( 17,  55),   S( 16,  29),   S( 28,   0),   S( 55,  -9),   S( 30, -13),   S( 21,   5),   S( 20, -17),   S( -7, -61),
            S( 11,  12),   S( 14,   9),   S( 41,  15),   S(  8, -16),   S( 16, -19),   S(  3, -34),   S( -9, -71),   S(-21, -50),
            S( -5, -14),   S( 28,   9),   S( 11,   3),   S( -3, -23),   S( -2, -18),   S(  7,  -2),   S( -2, -23),   S(-23, -43),
            S(-11, -18),   S(  1, -25),   S(  1,  -9),   S( -1, -27),   S(-18, -60),   S( -2, -41),   S(-11, -33),   S( -7, -44),

            /* bishops: bucket 9 */
            S(-21, -81),   S( 15, -51),   S(-14, -15),   S(-18, -38),   S(-26, -59),   S( -3, -41),   S(-11, -39),   S(  4,  -2),
            S( -1, -38),   S(-10, -35),   S( -8, -32),   S( 15, -26),   S(-15, -26),   S( -3, -39),   S(-12, -14),   S( -7, -20),
            S(  1, -27),   S( 17,   4),   S(  2, -14),   S( 22, -12),   S( 58, -12),   S( 31, -39),   S(-18, -18),   S( 17,  -7),
            S( -9, -32),   S( 34,   7),   S( 12, -14),   S( 73, -13),   S( 42, -15),   S(  8,   7),   S(  4, -35),   S(-15, -58),
            S(  6,  -2),   S( 34,  -5),   S( 29,   1),   S( 40,   0),   S( 25, -26),   S( 32, -11),   S( 23,  -6),   S(-19, -27),
            S( -6, -24),   S( 42,  20),   S(  2,  21),   S(  1,  -3),   S( 35, -21),   S( 27, -31),   S( 11, -23),   S(-27, -39),
            S(  8,  14),   S( 33,  16),   S( 11,  -8),   S( 21, -21),   S( 21, -17),   S(  8, -30),   S(  1, -25),   S( -7, -51),
            S(  0,  -6),   S( -6, -34),   S( -1, -36),   S(-19, -61),   S( -6, -35),   S(  2,  -2),   S(  5,   4),   S(-11, -67),

            /* bishops: bucket 10 */
            S(-12, -33),   S(  7, -49),   S(-34, -31),   S( -9, -44),   S(-17, -31),   S(-25, -57),   S( -5, -46),   S(-19, -76),
            S(  7, -24),   S(-10, -26),   S( 17, -54),   S(-16, -41),   S( -2, -34),   S( 21, -46),   S(-22, -83),   S(-11, -12),
            S( -5, -51),   S( 15, -49),   S( -4, -55),   S( 41, -29),   S( 48, -51),   S(  3, -25),   S(  2,  -2),   S( -6, -10),
            S( -7, -56),   S(  8, -37),   S( 23, -38),   S( 45, -29),   S( 76, -26),   S( 15, -13),   S( -3, -19),   S( 20,  13),
            S(-12, -41),   S( 15, -40),   S( 49, -41),   S( 75, -27),   S( 66,  -7),   S( 43,   1),   S(  4,  -9),   S( -4, -53),
            S(-10, -42),   S(  8, -71),   S( 14, -44),   S(  9, -46),   S( 39, -36),   S( 43,   2),   S( 16,  -9),   S( -9, -39),
            S(-13, -69),   S(  8, -78),   S(  3, -50),   S( 16, -47),   S( -7, -43),   S( 19, -21),   S( 16,  23),   S( 11,  13),
            S(  0, -44),   S( -8, -45),   S(  5,  -2),   S( -5, -32),   S(-10, -48),   S( -2, -54),   S(  4,  13),   S( 11,  26),

            /* bishops: bucket 11 */
            S(-21, -34),   S(-35, -28),   S(-65, -81),   S(-34, -51),   S( -6,   6),   S(-57, -71),   S(-20, -73),   S(-26, -71),
            S(-11, -59),   S( 22, -34),   S(  8, -16),   S( -5, -18),   S(-22, -44),   S(-10, -38),   S( -8, -44),   S(-39, -70),
            S( -7, -73),   S( 13, -49),   S( -7, -36),   S( 48, -44),   S(  5, -18),   S( 12, -25),   S(-12,  -6),   S( -1, -15),
            S(-13, -39),   S( -1, -40),   S( 27, -48),   S( 48, -28),   S( 73, -24),   S( 27,  17),   S( 21,  -2),   S( -6,  10),
            S( -2, -33),   S(  2, -63),   S( 15, -18),   S( 70, -34),   S( 39, -10),   S( 43,   1),   S( 13,  20),   S( 17, -20),
            S(-19, -76),   S(  2, -78),   S( 17, -33),   S( 37, -25),   S( 29, -28),   S( 30,  15),   S( 33,  36),   S(-10, -11),
            S(-10, -41),   S(  3, -56),   S( -8, -59),   S(  7, -49),   S(  3, -24),   S( 37, -38),   S(  8,  -6),   S( 21,  42),
            S(-15, -66),   S(-13, -37),   S( -3, -40),   S( 10, -36),   S( 15, -24),   S( -6, -51),   S( -2, -25),   S(  4,   2),

            /* bishops: bucket 12 */
            S(  0,  -9),   S(-10, -44),   S(-12, -58),   S(  1, -16),   S( -8, -33),   S( -2, -15),   S(  0,   0),   S( -4, -10),
            S( -9, -44),   S( -9, -49),   S( -1, -31),   S( -1, -18),   S(-11, -29),   S(  2,   9),   S( -9, -46),   S(  2,  -1),
            S( -1, -20),   S(-11, -31),   S( -4, -30),   S( -5, -36),   S(  7,  30),   S( -3, -37),   S( -8, -49),   S( -7, -29),
            S(  1, -19),   S(  6, -12),   S( -9, -39),   S(  1,  13),   S( -7, -31),   S( 10,  19),   S( -7, -34),   S( -8, -26),
            S(-12, -32),   S(  1, -18),   S(  3, -30),   S(  3,  11),   S(  6, -29),   S(-12, -46),   S( -2, -47),   S( -7, -19),
            S(-26, -47),   S(  1,  28),   S(  2, -20),   S( -3, -22),   S(  5,  -7),   S(-14, -30),   S(  1, -27),   S(  2,  -1),
            S( -3,  -9),   S( -9,  -2),   S( -2,  -3),   S( -9, -20),   S( -3, -32),   S(  9,   2),   S( -5, -32),   S( -2, -13),
            S( -2, -15),   S( -8, -18),   S( -3, -36),   S( -5, -11),   S(  5,  -5),   S( -1, -21),   S( -2, -26),   S(  0,   7),

            /* bishops: bucket 13 */
            S( -3, -36),   S( -9, -73),   S( -7, -38),   S( -4, -51),   S(-10, -53),   S(-12, -14),   S( -9, -23),   S( -5, -40),
            S( -4, -30),   S(  0, -41),   S( -8, -56),   S(-14, -52),   S( -7, -14),   S( -5, -24),   S(  0, -26),   S(  0, -37),
            S( -9, -40),   S( -2, -19),   S(  5, -19),   S(  4, -59),   S(  0, -38),   S(  9, -43),   S(-17, -61),   S(  6,  31),
            S( -1,   5),   S( -2, -33),   S( 11,  -6),   S( -7, -64),   S( 12, -61),   S( 14,   8),   S(  1,   7),   S(-13, -53),
            S( -3,  -9),   S( -2,   7),   S( -3, -52),   S( 24,  -8),   S(  9, -11),   S(  0, -53),   S( -6, -41),   S( -1, -19),
            S(  1,  -1),   S( -3, -28),   S(-14,  -3),   S( 17,  15),   S(  5,  17),   S(  8, -12),   S( 21, -19),   S( -5, -37),
            S( -3, -25),   S(-15, -32),   S( -2,  -9),   S(  1,  19),   S( -8, -45),   S(  7,  20),   S(-12, -69),   S(  5,  11),
            S( -6, -33),   S( -3, -20),   S( -5, -28),   S(  1, -23),   S(  0,   4),   S( -5, -30),   S(  0,  -5),   S( -6, -35),

            /* bishops: bucket 14 */
            S( -3, -44),   S(-20, -80),   S(-12, -49),   S(-13, -56),   S( -8, -51),   S( -4, -42),   S(-12, -76),   S( -8, -31),
            S( -9, -17),   S( -3, -36),   S( -2, -13),   S(-21, -66),   S(-11, -56),   S( -5, -71),   S(-14, -44),   S(  1, -30),
            S( -7, -28),   S( -5, -23),   S(-10, -66),   S(  7, -36),   S(-11, -73),   S( -4, -63),   S( -3, -38),   S( -8, -33),
            S(-11, -47),   S(  7,   1),   S(-18, -55),   S(  0, -48),   S(  9, -75),   S(  0, -57),   S(-17, -70),   S( -6, -22),
            S( -2, -18),   S(  9,  -5),   S(  3, -42),   S( 10, -24),   S(  3, -73),   S(  6, -37),   S(  9,  -9),   S( -3, -28),
            S( -1, -31),   S( -1, -38),   S(  8, -12),   S( -7, -30),   S( 11,   2),   S( -4,  16),   S(  7, -19),   S(-13, -59),
            S( -7, -41),   S( -1, -37),   S( -5, -20),   S(  6, -12),   S(-14, -48),   S( -4, -29),   S( -1, -20),   S( -3, -16),
            S( -5, -46),   S( -8, -40),   S( -5, -32),   S(  0,  -4),   S( -9, -41),   S( -6, -20),   S(  3,  22),   S(  0,  -3),

            /* bishops: bucket 15 */
            S( -1,   3),   S(  6,  21),   S(-13, -70),   S( 10,   1),   S( -1,  -3),   S( -6,  -9),   S( -6, -31),   S( -4, -20),
            S(  9,  23),   S(  1,   5),   S(  0,  -7),   S( -6, -43),   S( -6,  -7),   S( -1,  -1),   S( -4, -14),   S( -7, -29),
            S(-15, -60),   S(-11, -40),   S(-21, -79),   S(  2,  -6),   S(-22, -83),   S( -7, -26),   S(-16, -46),   S(  1, -17),
            S( -1, -10),   S( -8, -39),   S( -6, -15),   S(-12, -26),   S(  8,   4),   S(  1, -16),   S(  3,   4),   S( -2,  -3),
            S(  4,  -9),   S(-11, -28),   S( -7, -45),   S(-12, -29),   S(  3,  -8),   S(-12, -32),   S(  6, -22),   S( -5, -26),
            S( -5, -48),   S(-11, -65),   S( -8, -28),   S(-15, -67),   S( -6, -36),   S( -4,  -4),   S( 27,  29),   S( -2,  -2),
            S( -4, -30),   S( -1, -23),   S( -2, -19),   S( -3, -22),   S( -7, -32),   S( -5,  -1),   S(-10, -28),   S(  5,  17),
            S(  0, -18),   S(  1,   5),   S(  1, -19),   S(  0, -19),   S( -3, -42),   S(-13, -50),   S(-14, -30),   S(  1,   5),

            /* rooks: bucket 0 */
            S(-15,  -4),   S( -6,  -5),   S( 10, -28),   S( 12, -17),   S( 19,  -8),   S( 18, -21),   S(  6,   3),   S( 16,   3),
            S( 15, -74),   S( 25, -26),   S( 24, -21),   S( 22, -14),   S( 39, -13),   S( 23, -34),   S(-15,   7),   S(-32,  20),
            S(-15,   3),   S( 26,   8),   S( 33,   8),   S( 13,  15),   S( 13,  18),   S( 18, -12),   S(-26,   4),   S(-30,  -7),
            S( 40, -30),   S( 68,  -5),   S( 26,  28),   S( 52,  -4),   S( 36,  -3),   S(-11,  27),   S(-21,  17),   S(-37,  32),
            S( 32, -23),   S( 90, -26),   S( 83, -22),   S( 64, -11),   S( 55,  12),   S( 38,   7),   S(  2,  32),   S(  3,  16),
            S( 92, -54),   S( 85, -40),   S( 64,   0),   S( 21,   9),   S( 59,   7),   S(-14,   4),   S( 55,   1),   S(-29,  16),
            S( 66, -22),   S( 62, -19),   S( -6,  44),   S( 32,  26),   S(  2,   9),   S( 42, -11),   S( 22,  20),   S(  6,  23),
            S(  9,   8),   S( 10,  28),   S( 51,  13),   S( 14,  25),   S( 26,   6),   S( 20, -21),   S( 32,  23),   S( 25,  10),

            /* rooks: bucket 1 */
            S(-81,  36),   S(-42,  13),   S(-47,  -2),   S(-36,  -5),   S( -3, -22),   S(-19,  -8),   S(-27,  -5),   S(-28,   9),
            S(-46,  17),   S(-52,  19),   S(-14, -11),   S(-10, -26),   S(-13,  -6),   S(-18, -15),   S(-41, -18),   S(-63,   9),
            S(  5,  14),   S(-51,  53),   S(-45,  33),   S(-58,  51),   S(-28,  20),   S( -7,  16),   S(-21,   2),   S(-50,  18),
            S(-48,  57),   S(-61,  59),   S(-16,  49),   S(-31,  46),   S(-11,  28),   S(-41,  52),   S(-22,  36),   S(-20,  22),
            S( 54,  16),   S( 26,  46),   S(-19,  34),   S(-33,  51),   S( 15,  23),   S( 17,  22),   S(-10,  30),   S(-44,  29),
            S( 87,   4),   S( -9,  51),   S(  4,  42),   S(-10,  29),   S(  0,  37),   S(-16,  33),   S(-12,  47),   S(-38,  42),
            S( -3,  45),   S(  9,  41),   S( 41,  30),   S(-71,  83),   S( -1,  34),   S(  7,  40),   S(-32,  44),   S(  0,  32),
            S( 73,  14),   S( 26,  37),   S( 30,  26),   S(-41,  66),   S(-18,  46),   S( 24,  22),   S( -4,  40),   S( 60,  -2),

            /* rooks: bucket 2 */
            S(-72,  47),   S(-47,  36),   S(-39,  22),   S(-54,  28),   S(-59,  26),   S(-57,  19),   S(-45,  -3),   S(-53,  34),
            S(-96,  48),   S(-68,  45),   S(-43,  34),   S(-43,  19),   S(-42,  16),   S(-57,  14),   S(-67,  25),   S(-65,   5),
            S(-68,  62),   S(-70,  64),   S(-40,  55),   S(-50,  40),   S(-48,  48),   S(-56,  57),   S(-28,  24),   S(-40,  28),
            S(-83,  76),   S(-56,  73),   S(-37,  84),   S(-32,  64),   S(-31,  46),   S(  4,  40),   S(-46,  63),   S(-27,  38),
            S(-29,  62),   S(-46,  76),   S(-50,  71),   S( -7,  49),   S( 40,  37),   S(  8,  48),   S(-47,  66),   S(-37,  53),
            S(-20,  56),   S(-26,  56),   S(-23,  60),   S(  7,  36),   S( 19,  36),   S( 24,  52),   S( 16,  32),   S(-27,  46),
            S(-61,  66),   S(-63,  89),   S(-20,  73),   S( 20,  58),   S( 26,  42),   S( 46,  20),   S(-32,  71),   S(-36,  66),
            S(-33,  84),   S(-32,  69),   S(-37,  65),   S( 10,  42),   S(-44,  74),   S(-13,  53),   S(-41,  78),   S( 36,  30),

            /* rooks: bucket 3 */
            S(  1,  72),   S(  3,  75),   S( 16,  65),   S( 24,  55),   S( 20,  52),   S( -6,  62),   S(  3,  69),   S( -8,  41),
            S(-35,  86),   S( -8,  71),   S( 10,  65),   S( 25,  57),   S( 23,  63),   S( 13,  62),   S( 41,  15),   S( 28, -43),
            S(-28,  84),   S( -7,  86),   S( 16,  76),   S( 10,  73),   S( 24,  68),   S( 22,  69),   S( 24,  53),   S( -1,  54),
            S(-26,  98),   S(-21, 101),   S( 19,  91),   S( 39,  71),   S( 27,  72),   S(  8, 106),   S( 49,  61),   S(  1,  84),
            S(-10, 105),   S( 24,  91),   S( 25,  72),   S( 45,  77),   S( 45,  83),   S( 50,  71),   S( 76,  53),   S( 57,  50),
            S( 13,  97),   S( 20,  86),   S( 31,  80),   S( 37,  71),   S( 52,  54),   S( 61,  53),   S( 97,  34),   S(100,  30),
            S(  3,  97),   S(  3, 106),   S( 18, 107),   S( 38,  96),   S( 42,  77),   S( 58,  69),   S( 39,  88),   S(137,  28),
            S(-34, 141),   S( 26, 101),   S( 14,  91),   S( 60,  71),   S( 87,  51),   S( 97,  46),   S(115,  55),   S(108,  33),

            /* rooks: bucket 4 */
            S(-72,   9),   S( -1, -29),   S(-29, -13),   S(-10,   2),   S(-14, -30),   S(-16, -41),   S( 10, -23),   S(-19, -24),
            S(-28, -36),   S(-34,  -7),   S(-22,  -6),   S(-34,  24),   S(-31,   0),   S(-32, -17),   S(-27, -40),   S(-40,  -6),
            S(-17,   7),   S(-35, -26),   S(-22,  12),   S( 14, -16),   S(-36,   9),   S( 12, -24),   S( 24, -15),   S(-58,  -9),
            S(-79, -21),   S(  6,  14),   S(-39,  20),   S( 35,  -2),   S(  3,   6),   S(-29,  -2),   S(-42,  20),   S(-22,  28),
            S(-38,   4),   S(-17,  49),   S( -3,   8),   S( 44,  20),   S( 51,  21),   S( -1,   0),   S( 57,  16),   S( 35,  -8),
            S( 13, -20),   S( 10,  -8),   S( 58,   5),   S( 45,  25),   S( 48,  14),   S( 33,   7),   S( 17,  11),   S( 38,  18),
            S(-37,  20),   S(-12,  40),   S( 28,  20),   S( 54,  19),   S( 74, -12),   S(  8,   0),   S( 57,  22),   S( 27,  29),
            S( 19, -47),   S( 24,  27),   S( 13,   5),   S( 17,  12),   S( 12, -16),   S( 12,   4),   S( 14,  -4),   S(  2,  10),

            /* rooks: bucket 5 */
            S(-32,  30),   S(-36,  38),   S(-28,  28),   S(-52,  16),   S( -5,   3),   S(-41,  35),   S(  5,  22),   S(-31,  32),
            S(-37,  33),   S(-41,  40),   S(-107,  77),  S(-68,  45),   S(-71,  43),   S(  8,  -2),   S( 30,   1),   S(-20,   8),
            S( -7,  38),   S(-76,  63),   S(-79,  77),   S(-78,  69),   S(-38,  15),   S(-50,  52),   S( -4,  18),   S( 10,  10),
            S( -2,  68),   S( 20,  47),   S(  5,  44),   S(-12,  39),   S(-14,  46),   S(-30,  80),   S( 19,  33),   S(  6,  24),
            S(  0,  68),   S(  0,  64),   S( 40,  49),   S( 18,  73),   S( 42,  54),   S( 22,  56),   S( 72,  48),   S( -7,  40),
            S( 25,  73),   S( 18,  68),   S( 77,  42),   S( 56,  56),   S( 57,  42),   S( 54,  44),   S( 60,  33),   S( 41,  45),
            S( 23,  62),   S( 25,  58),   S( 75,  50),   S( 42,  73),   S( 67,  36),   S( 73,  42),   S( 82,  34),   S( 60,  50),
            S( 84,  31),   S( 63,  34),   S( 30,  59),   S( 33,  37),   S( 78,  24),   S( 74,  36),   S( 56,  28),   S( 24,  42),

            /* rooks: bucket 6 */
            S(-48,  28),   S(-14,  26),   S( -9,  21),   S( -2,  12),   S(-51,  34),   S(-54,  40),   S(-50,  54),   S(-34,  52),
            S(  2,   9),   S( 14,  22),   S(  1,  25),   S(-54,  37),   S(-50,  51),   S(-52,  55),   S(-77,  54),   S( 37,  11),
            S(-57,  59),   S(-22,  37),   S(  7,  30),   S(-39,  44),   S(-58,  63),   S(-77,  75),   S(-29,  56),   S( 16,  20),
            S(-71,  82),   S( -1,  55),   S( -6,  67),   S(  0,  50),   S(-22,  55),   S(-12,  61),   S(-42,  71),   S(-15,  42),
            S( 12,  67),   S( 24,  73),   S( 45,  43),   S( 56,  28),   S(-32,  83),   S( 33,  59),   S( 37,  48),   S(  7,  60),
            S( 20,  64),   S( 93,  41),   S( 80,  38),   S( 32,  39),   S( 23,  53),   S( 22,  73),   S( 50,  70),   S( 63,  45),
            S( 51,  59),   S( 83,  46),   S( 94,  38),   S(105,  26),   S(110,  11),   S( 60,  49),   S( 62,  44),   S( 46,  65),
            S( 58,  63),   S( 45,  52),   S( 82,  39),   S( 19,  62),   S( 78,  34),   S(100,  40),   S( 74,  40),   S( 36,  52),

            /* rooks: bucket 7 */
            S(-81,   3),   S(-30,  -5),   S(-31, -15),   S(-14,  -7),   S( -1, -20),   S(-21,  15),   S(-41,   8),   S( 36, -30),
            S(-69,  22),   S(-44,   8),   S(-32,  -3),   S( -8,  -7),   S(-14,   7),   S( 12,   0),   S(-36,  20),   S(-25,  -6),
            S(-111,  56),  S(-66,  29),   S(-11,  12),   S(  1,  -3),   S( -8,  13),   S(-46,  17),   S(  1,  -6),   S( 11,  12),
            S(-47,  37),   S( 22,  20),   S( 45,   7),   S( 49,   9),   S( 28,   5),   S( 28,   3),   S( 38,  -4),   S( -9,  10),
            S( -4,  35),   S(-21,  35),   S( 74, -10),   S( 33,  13),   S( 57,  10),   S(144, -25),   S( 20,  35),   S( 36,  -9),
            S(-41,  51),   S( 50,   2),   S( 86,  -7),   S(122, -21),   S( 88,  -2),   S( 74,  13),   S( 50,  29),   S( 58, -18),
            S( 22,  28),   S( 59,  11),   S( 82,   8),   S(120,  -5),   S( 79,   5),   S( 72,  18),   S( 66,  20),   S( 26,   9),
            S(  6,  60),   S( -3,  33),   S( 37,  22),   S( 96, -14),   S( 53,  11),   S(  1,  18),   S( 61,   9),   S( 66,  -1),

            /* rooks: bucket 8 */
            S(-30, -34),   S(-25, -16),   S(  5,  31),   S(-34,  -1),   S(-38, -53),   S(-44, -64),   S(-35, -44),   S(-36,   2),
            S( -8, -18),   S(-17, -13),   S(-12, -11),   S( -4, -18),   S(-34, -50),   S(-20, -34),   S(-17, -42),   S(-18, -73),
            S( 24,  16),   S( -1, -14),   S( -5,  -2),   S(  0,   6),   S(-27, -53),   S( -9, -12),   S(  5,  31),   S( -6, -20),
            S( -2, -13),   S( -7,   2),   S(  0,  28),   S( -1, -23),   S(  5,  14),   S(-16, -20),   S( -2,  -5),   S(-17, -12),
            S(-20, -25),   S( -3,   0),   S(-15,  50),   S(  2,  26),   S( -6, -10),   S(  7, -15),   S(  5, -20),   S(-20, -42),
            S( 11,  21),   S(-13,   5),   S( 29,  45),   S( 16,  -9),   S( -2,   9),   S(  0, -18),   S(  3, -11),   S(  5,  39),
            S(-13,   1),   S(-18,   7),   S( 14,  20),   S(  5,  -2),   S( 29,  21),   S( 14,  -2),   S( 18,   0),   S(-15, -18),
            S( -4, -109),  S( -8, -22),   S( 17,  25),   S(  2, -15),   S(  8,  11),   S(  8,   9),   S(  6,   5),   S(  6,  11),

            /* rooks: bucket 9 */
            S(-43, -39),   S(-21, -38),   S(-35, -46),   S(-61, -20),   S(-37,  21),   S(-20, -14),   S( -2, -57),   S(-39, -47),
            S(  7, -22),   S(-11, -54),   S(-31, -34),   S(-39, -32),   S(-25, -36),   S(  4,  -6),   S( -6, -53),   S(-29, -49),
            S( -5, -41),   S(  0, -32),   S(  1,  -7),   S(-31,  -5),   S(-37, -37),   S( -2,  -8),   S(  8, -25),   S( -6, -22),
            S(  1, -26),   S(-10, -16),   S(-21,  11),   S( -9,   1),   S(-16,  -3),   S( -6, -24),   S(  6,  14),   S( -8, -11),
            S(-12,  -7),   S(  3,  -1),   S(  7,   9),   S( 16,  30),   S( 12,   7),   S( 18,  13),   S(  0,  -9),   S( -2, -39),
            S(-11,  11),   S(-13, -27),   S(-12,   0),   S(-18,  -1),   S(  1, -22),   S( 16,   1),   S( 10,  -3),   S( 11, -11),
            S( 42,   7),   S( 47,  -9),   S( 25,  10),   S( 40,   4),   S(  9, -14),   S( 46, -14),   S( 34, -33),   S( 42, -14),
            S( 50, -81),   S( 32, -21),   S( 28,  -8),   S( 16,  13),   S( -4,  -3),   S( 15,   9),   S( 23,  -6),   S( 32,  -6),

            /* rooks: bucket 10 */
            S(-58, -114),  S(-12, -47),   S(-10, -66),   S(-52, -42),   S(-59, -36),   S(-50, -53),   S(  2, -56),   S(-55, -32),
            S( -8, -31),   S(  4, -41),   S(-22, -63),   S(-44, -13),   S(  2, -31),   S(-27,  -9),   S( 16, -33),   S( -3, -50),
            S(-27, -41),   S(-14, -34),   S( -8, -22),   S( -9, -16),   S(-38,  -2),   S( -5, -21),   S( 11, -22),   S(-14,  -1),
            S(-24, -34),   S( 11, -21),   S(-33,   4),   S(  0,  -3),   S(  9, -12),   S( -6, -20),   S( 14,  16),   S(-16, -49),
            S( -5,  -7),   S( -3, -14),   S( -4, -22),   S( 12, -36),   S(-21, -33),   S( -5,  23),   S( 35,   5),   S(-14,  -9),
            S( 29,  -2),   S( 13,  21),   S( -5, -15),   S(  7,  -9),   S(-30, -24),   S(  6,  -3),   S( 35,   6),   S(  3, -16),
            S( 59, -11),   S( 61, -24),   S( 67, -30),   S( 44, -42),   S( 40, -27),   S( 49,  -3),   S( 21, -31),   S( 32, -41),
            S( 34,   2),   S(  6, -29),   S( 27, -25),   S(  5, -15),   S( 24, -21),   S( 14,  -2),   S( 16, -41),   S( -5, -46),

            /* rooks: bucket 11 */
            S(-86, -25),   S(-22, -47),   S(-54, -16),   S( -4, -54),   S(-26,  -8),   S(-21,  -7),   S(-53, -50),   S(-49, -22),
            S(-32, -17),   S(-13, -39),   S(-15, -11),   S(-38, -16),   S(-37, -29),   S(-20,  -8),   S( -7, -26),   S(-17,  16),
            S( -6, -44),   S( 12, -14),   S( -1, -27),   S( -3, -27),   S( -5, -10),   S(-32, -10),   S(-14, -16),   S(-36, -64),
            S(  2,  29),   S( -8, -31),   S( 10,  18),   S(-10,   9),   S(-12, -15),   S(-21,  25),   S( 16, -10),   S( -6, -34),
            S( -2,  12),   S( 11, -16),   S( 12,   6),   S( 25, -24),   S( 25,   1),   S( 31, -12),   S(  1,  -5),   S( -2, -13),
            S( 20,  21),   S( 37,   3),   S( -1, -15),   S( 52,  -4),   S( 30,   7),   S( 50,  -4),   S( -6,  14),   S( 15,   3),
            S( 46,  36),   S( 23,  13),   S( 53, -14),   S( 66, -12),   S( 28,  10),   S( 29,  32),   S( 30,  28),   S( 44,   0),
            S( 25,  33),   S(  4,   4),   S( 23, -16),   S(  4,  -3),   S(  1,   3),   S( 10,  -6),   S(  4,  -1),   S( 23,  19),

            /* rooks: bucket 12 */
            S( -1, -54),   S(-10, -54),   S( -9,  -2),   S( -4, -22),   S(  0, -21),   S( -1, -29),   S(-12, -42),   S(-31, -52),
            S(  4,   9),   S( -4, -23),   S( -9,  16),   S(  1, -10),   S(-10,  -2),   S( -7,  -7),   S(  9,   0),   S( -3, -39),
            S(  4, -13),   S( -9, -33),   S(-22, -27),   S(-24, -47),   S(-11, -27),   S(  3,  -1),   S(-12, -47),   S(  1,   6),
            S(-10,  -3),   S( -2,  -3),   S( 12,  26),   S(  9,   2),   S( -2,  -5),   S( -8, -26),   S( -7, -29),   S(  3, -19),
            S(-17, -39),   S(-17, -30),   S(  8,   2),   S( -3, -11),   S(-15, -37),   S( 12,  10),   S( -4, -22),   S(  2,  -4),
            S( -1,   3),   S(  0, -24),   S( 19,  -6),   S( 14,   5),   S(  0, -15),   S( -2, -24),   S(  1, -32),   S(  3,  11),
            S( -9, -25),   S(  0, -17),   S( -7, -47),   S(  0, -28),   S( 16,   2),   S( -1, -39),   S( -4, -40),   S(  5,  11),
            S( -3, -43),   S(  8,  14),   S( 12,   2),   S(  8,  -4),   S( 10,   4),   S( -7, -29),   S(-10, -54),   S( 11,  22),

            /* rooks: bucket 13 */
            S( -4, -43),   S( -1, -25),   S( -9, -24),   S( -1,  44),   S( -1, -10),   S(-18, -42),   S(  3, -21),   S(-24, -40),
            S( -2, -25),   S( -2,   0),   S( -9,  -4),   S( -1,  17),   S( -5,  -7),   S(  0,  -5),   S( -3,  -2),   S(  0, -23),
            S( -3, -29),   S(-21, -45),   S( -8, -10),   S( -7, -25),   S(  4,  46),   S(  0, -23),   S(  0,  13),   S( -7, -29),
            S(-12, -37),   S( -2,   6),   S(-23, -39),   S( -2, -30),   S( 14,  22),   S(-14, -36),   S( -4, -32),   S( -9, -34),
            S(  8,   0),   S( -2, -38),   S(  8,  15),   S(-11, -61),   S(-13, -20),   S( -9, -21),   S(-11, -67),   S( -2,  -4),
            S(-20, -33),   S(  3,   4),   S(-12, -47),   S( 14,  -9),   S(  6,  -4),   S( 11,   4),   S(  4, -13),   S(  2,   1),
            S(-14, -22),   S( 12,  23),   S( -1,   2),   S(  3,  13),   S( 12,  12),   S( 13,  24),   S( -4, -38),   S( -1, -14),
            S(-25, -118),  S(-18, -71),   S(  1,  -2),   S( -8, -48),   S( -3,  -1),   S(-10, -45),   S(-21, -67),   S( -4,   3),

            /* rooks: bucket 14 */
            S(-19, -40),   S(-31, -78),   S( -8, -33),   S( -9, -25),   S(-22,  -5),   S( -7, -13),   S(  9,  -8),   S(-24, -42),
            S(-22, -65),   S(-22, -59),   S(  0,  12),   S(-21, -37),   S( -6, -19),   S(-13, -50),   S(  5,  13),   S(  7,  -6),
            S(-16, -27),   S(-10, -20),   S(  0, -10),   S(-18, -29),   S(-22, -32),   S(-16, -34),   S( 15,  51),   S(  1, -11),
            S(  0,   2),   S(-12, -14),   S( -5, -38),   S(  0,  -1),   S( -9, -32),   S(  1,  -7),   S(-18, -64),   S(-10,  -7),
            S(  9, -33),   S( -2, -30),   S( -5, -56),   S(-12, -50),   S( -3, -55),   S( -4, -50),   S(  3, -33),   S(  3,  -6),
            S(  0, -19),   S( -3, -13),   S(  4, -43),   S( -6, -70),   S( -8, -72),   S( -7, -44),   S(  9, -16),   S( -9, -44),
            S(  4, -20),   S(  4, -21),   S(  2, -52),   S(-13, -81),   S(  0, -34),   S(  3, -29),   S( 14, -10),   S(  9, -10),
            S( -9, -33),   S( 13,  21),   S(-15, -58),   S(  0, -15),   S(-14, -28),   S(-13,   0),   S(  4,   9),   S( -6, -16),

            /* rooks: bucket 15 */
            S( -7, -23),   S(-14, -27),   S( -8, -42),   S( -9, -18),   S( -7, -28),   S( -5,  -2),   S(-12, -35),   S(  2, -14),
            S(-12,   1),   S(-14, -37),   S(  1,  -4),   S(-10, -21),   S(-11, -41),   S(-12, -24),   S(-22, -59),   S(  7,   2),
            S( -4, -12),   S(-11, -41),   S( -1, -27),   S(  8, -12),   S( 13,   4),   S(-12, -36),   S( -4,  -3),   S( -5, -21),
            S(-13, -47),   S( -8, -54),   S(-13, -29),   S( -9, -30),   S(-15, -45),   S(-10, -27),   S(  3,  -6),   S(-14, -13),
            S(-18, -38),   S(-12, -41),   S(  8, -23),   S(  2, -24),   S( 10,   7),   S(  2, -21),   S(  7,   8),   S( -3,  -6),
            S( -9, -46),   S( -1, -13),   S(  3, -46),   S(  8, -46),   S( -3, -49),   S(  2, -24),   S(  6, -41),   S( -9, -21),
            S(  9,  26),   S( 12,  -5),   S( 13, -16),   S(  4, -45),   S(  0, -37),   S(  2,  -7),   S( 18,  22),   S( -3, -16),
            S(  0, -14),   S( -5,  -9),   S( -1, -36),   S( 10, -15),   S( -7, -59),   S(  2,  -8),   S(  4, -10),   S( -2, -15),

            /* queens: bucket 0 */
            S(-20, -24),   S(-12, -67),   S( 42, -84),   S( 59, -64),   S( 34, -49),   S( 12,  -6),   S( 47,  16),   S( 28,   0),
            S(-25, -26),   S( 39, -80),   S( 42, -33),   S( 31,   6),   S( 34,  22),   S(  2,  47),   S( 32, -11),   S( 45,   9),
            S(  9,   8),   S( 24,   6),   S( 20,  42),   S( 26,  36),   S( 24,  11),   S( 12,  40),   S(  1,  34),   S( 18,  80),
            S( 17, -30),   S( 15,  37),   S( 16,  37),   S( 20,  30),   S( 18,  37),   S(  7,  46),   S( 13,  38),   S(  9,  25),
            S( 35,  10),   S(  8,  50),   S( 19,  16),   S( 24,  54),   S(-15,  45),   S( 22, -30),   S( 12,  11),   S( 29,  39),
            S( 14,  44),   S( 25,  65),   S( 19,  41),   S( 34,   6),   S( 34,   8),   S(  4,  49),   S(  7,  18),   S( 29, -52),
            S( 62,  23),   S( 64,  23),   S( 13,  47),   S( 62,  46),   S( 19,  16),   S(-12,  -9),   S( 22,  28),   S( 16,  23),
            S( 73,  37),   S( 36,  22),   S( 43,  15),   S( 66,  48),   S( 57,  62),   S(-17,  12),   S( 68,  27),   S( 64,   9),

            /* queens: bucket 1 */
            S( -8, -38),   S(-92, -18),   S(-40, -17),   S(-16, -118),  S(  4, -51),   S(-12, -15),   S( 39, -47),   S(  0,  16),
            S(-16, -30),   S(-15, -34),   S( 20, -50),   S(  6,  40),   S(  8,   1),   S( 16,  -3),   S( 20, -37),   S(  6,  17),
            S(-23,  40),   S( 11,  -6),   S( 17,  -2),   S( -2,  17),   S(  4,  42),   S(  0,  17),   S( 19,  -8),   S( 29,  23),
            S( 10, -14),   S(-26,  40),   S(-10,  51),   S( 24,  34),   S(-11,  69),   S( 14,  18),   S( 12,  30),   S( 18,  15),
            S( 36,  -9),   S(-10,   9),   S( -6,  75),   S( -3,  65),   S(-23,  70),   S(  8,  17),   S(-20,  18),   S(  8,   2),
            S(  4,  60),   S( 12,  68),   S( 30,  93),   S(-15,  65),   S(-13,  71),   S(-34,  36),   S( 28,   5),   S( 12,  23),
            S(-10,  57),   S(-13,  84),   S(-12,  22),   S(-22,  98),   S(-63,  65),   S(-21,  60),   S( 55,  28),   S(-10,  21),
            S(  8,  11),   S( -7,  28),   S( 37,  58),   S(  3,  26),   S( 15,  42),   S( 12,   5),   S( 27,  30),   S( 39,  36),

            /* queens: bucket 2 */
            S( 25, -16),   S( 30, -50),   S( 21, -40),   S(  7, -23),   S(-14,  -4),   S(-51,  -7),   S(-14, -56),   S(  2,  15),
            S( 23,   3),   S( 23,   4),   S( 28, -15),   S( 31, -28),   S( 30, -17),   S( 31, -73),   S( 31, -24),   S( 31, -35),
            S( 20,  10),   S( 20,   9),   S( 21,  31),   S( 18,  27),   S( 18,  50),   S( 26,  49),   S( 13,   3),   S( 38,   5),
            S(  9,  27),   S(-11,  62),   S(  4,  41),   S( 11,  64),   S(-12,  79),   S(  5,  86),   S( 22,  -5),   S( 14,  58),
            S( 11,  17),   S(-16,  72),   S(-27,  92),   S(-40, 122),   S(-16,  98),   S(-19, 105),   S(-21, 131),   S(-10, 107),
            S( 33,   1),   S(  2,  63),   S( -5,  71),   S( -5,  60),   S(-11,  88),   S(-38, 103),   S(-17, 114),   S(  0,  81),
            S(-30,  67),   S(-43,  97),   S(-37, 104),   S( 13,  76),   S(-15, 108),   S( 56,  48),   S(-27,  64),   S(-45,  83),
            S(-86, 106),   S(  0,  54),   S( 42,  58),   S( 39,  55),   S( 26,  54),   S( 33,  48),   S( 19,  49),   S(-31,  46),

            /* queens: bucket 3 */
            S( 81, 106),   S( 63,  84),   S( 54,  94),   S( 46,  83),   S( 70,  21),   S( 49, -13),   S( 18,   6),   S( 41,  40),
            S( 62, 118),   S( 59,  97),   S( 49, 106),   S( 53,  77),   S( 54,  86),   S( 58,  52),   S( 59,  15),   S( 38,  24),
            S( 61,  94),   S( 52, 105),   S( 60,  83),   S( 55,  72),   S( 55,  84),   S( 56, 104),   S( 49, 109),   S( 54,  71),
            S( 38, 126),   S( 51,  92),   S( 49,  93),   S( 40, 102),   S( 32, 104),   S( 31, 140),   S( 47,  96),   S( 38, 137),
            S( 40, 112),   S( 46, 114),   S( 28, 121),   S( 24, 112),   S( 25, 137),   S( 13, 139),   S( 33, 164),   S( 38, 149),
            S( 47, 131),   S( 56, 115),   S( 53, 110),   S( 19, 134),   S( 17, 155),   S( 59, 119),   S( 43, 169),   S(  7, 195),
            S( 49, 119),   S( 47, 121),   S( 70,  99),   S( 62, 110),   S( 45, 138),   S( 63, 120),   S( 55, 151),   S(138,  66),
            S( 82,  75),   S( 73,  94),   S( 66,  99),   S( 69,  92),   S( 23, 131),   S( 94,  82),   S(132,  58),   S(114,  77),

            /* queens: bucket 4 */
            S(  1, -24),   S(-20, -18),   S( -6,   0),   S(-11, -31),   S(-22, -19),   S( 70,  10),   S(-89, -69),   S(-18,   9),
            S(-36, -43),   S(-38, -30),   S( 36, -31),   S(-65,   7),   S( -9, -10),   S( 30,  -8),   S( -1,  -7),   S(-32, -35),
            S(  1, -10),   S(-18,  -7),   S( 36,  17),   S( -4,  46),   S( 14,   4),   S(  7, -20),   S(-23, -10),   S(-29, -18),
            S(-22,  -8),   S(  9,  -2),   S(  0,  49),   S(-24,  28),   S( 52,  20),   S( 31,  38),   S( -3,  -7),   S( 22,  20),
            S(  0,  17),   S( 18,  19),   S( 41,  37),   S( 34,  37),   S( 24,  53),   S( 29,   7),   S(-29, -24),   S( 12, -16),
            S( 39,  34),   S( 25,  19),   S( 24,  58),   S( 28,  47),   S( 35,  13),   S( -2,  -2),   S(-23, -21),   S(  0,  -6),
            S( -4, -23),   S( -9,  26),   S( -3,  11),   S( 19,  22),   S( 11,  31),   S( 13,  16),   S(-13, -43),   S(-15, -12),
            S(-29, -40),   S(  5,   4),   S( 31,  48),   S( 10,  20),   S(-30, -23),   S(-15,  -3),   S(-28, -50),   S(-37, -51),

            /* queens: bucket 5 */
            S(-38, -18),   S(-38, -38),   S(-22, -32),   S(-67, -26),   S(-33, -27),   S(  8, -23),   S( -7, -13),   S(  5,   7),
            S(-28, -25),   S(-27,   3),   S(-29, -41),   S(-58,   3),   S(-22, -34),   S(-53, -24),   S(-54, -11),   S(-58, -12),
            S(-48, -26),   S(-51,  -7),   S(-66,  -7),   S(-19,  19),   S( 29,  60),   S( -8,  14),   S( 21,  -9),   S( -5, -13),
            S(-56,   3),   S(-29, -26),   S(-16,  38),   S( 14,  88),   S( 51,  18),   S(-22,  18),   S(  7,   3),   S(-19, -14),
            S(-53, -19),   S(-32,  14),   S(-13,  64),   S( 21,  57),   S( 37,  67),   S( 11,  32),   S( 10,  23),   S(-25, -24),
            S(-34,  -3),   S(  9,  34),   S(-13,  40),   S( 21,  23),   S( 34,  50),   S( -5,  -3),   S( 23,  12),   S(-16, -18),
            S(-19,   8),   S(-11,  15),   S( 15,  53),   S( -4,  28),   S( 27,  39),   S(  9,  43),   S( 14,  16),   S(-17, -16),
            S( 24,  19),   S(  4,  17),   S( 10,  19),   S( -2,  25),   S(  8,  24),   S(  9,  21),   S( -5, -23),   S(-30, -38),

            /* queens: bucket 6 */
            S(-48, -26),   S(-19, -30),   S(-43, -29),   S(-56, -55),   S(-91, -54),   S(-70, -40),   S(-62, -54),   S(-19, -11),
            S(-65, -16),   S(-66, -14),   S(-44,   4),   S(-61,  11),   S(-69,  54),   S(-111,   7),  S(-91, -31),   S( 21,   6),
            S(-61, -13),   S(-11,  -2),   S(-36,  32),   S(-67,  64),   S(-38,  60),   S(-30,  -7),   S(-43, -13),   S(  7,  14),
            S(-19,  38),   S( -2,  39),   S(-32,  51),   S(-12,  58),   S( 29,  50),   S( 28,  36),   S(-13,  35),   S( 39,  31),
            S(-62,  31),   S( 22,  41),   S(-22,  67),   S(-16,  51),   S( -3,  65),   S( 84,  20),   S( 45,  39),   S( -6,  29),
            S(-25,  49),   S( 14,  30),   S( 11,  30),   S( 64,  79),   S( 19,  61),   S( 52,  81),   S(-20,  26),   S(  5,  28),
            S( 29,  47),   S(  5,  12),   S( 17,  60),   S( 18,  45),   S( 41,  67),   S(  9,  52),   S(-17,   4),   S(-30,  24),
            S(  7,  32),   S(  8,  20),   S( 32,  43),   S( 21,  20),   S( 67,  65),   S( 19,  17),   S(-14,  10),   S( -2,   0),

            /* queens: bucket 7 */
            S( -5,  -9),   S(-50,  27),   S(-43,  30),   S(-27,  -7),   S(-47,  10),   S(-42, -37),   S(-35,   0),   S(-25, -12),
            S(-48, -10),   S(-18,   2),   S(-44,  27),   S( -2,  18),   S(-28,  48),   S(-40,  29),   S(-65,  58),   S(-39, -10),
            S(-17, -10),   S(-48,  31),   S( -3,  21),   S(  1,  36),   S(  8,  39),   S( 12,  43),   S(  3,   8),   S(-33, -35),
            S(-61, -11),   S( 15,   3),   S( -1,  33),   S(  0,  43),   S( 42,  31),   S( 54,  24),   S(-14,  73),   S( -2,   2),
            S(-40,  38),   S(-54,  35),   S( 12,  27),   S( 29,   7),   S( 57,   6),   S( 93,  -2),   S(  4,  56),   S( 15,   1),
            S(-46,  39),   S( -7,  13),   S(-10,  10),   S( 19,  15),   S( 12,  50),   S(104,  14),   S( 93,   2),   S( 35,  18),
            S(-12, -14),   S(  2,  26),   S( 33,   2),   S( 37,  26),   S( 45,  36),   S( 72,  25),   S( 36,  -2),   S( 57,  20),
            S(  8,  19),   S( 59,   6),   S( 41,  31),   S( 26,  35),   S( 55,  34),   S( 17,  20),   S( 16,   3),   S( 36,  23),

            /* queens: bucket 8 */
            S(  0,  -8),   S( -8, -23),   S( -1,   8),   S(-11, -17),   S( -3,   2),   S( -8, -24),   S(-17, -29),   S(-17, -20),
            S( -5,   2),   S(-20, -25),   S(-10,   6),   S(-21, -11),   S( -2, -19),   S(-14, -26),   S(-25, -64),   S( -9, -22),
            S( -7, -12),   S( -6,  -4),   S( -6,   5),   S(-17, -24),   S(-22, -26),   S(-17, -16),   S(-17, -43),   S(-21, -40),
            S(-10,  -5),   S(  9,  14),   S( -2,   2),   S(  9,  36),   S(  0,  13),   S( -8,   2),   S(-13, -19),   S(-24, -56),
            S( 17,  38),   S( 10,  36),   S(  8,  22),   S( 28,  53),   S( 11,  45),   S(  0,  -4),   S(-15, -14),   S(-13, -16),
            S(  5,  17),   S( -6,  -4),   S(-17,   7),   S( -3,  18),   S(-19, -22),   S( -6,  -9),   S( -2,   1),   S(  0,   6),
            S(  8,   8),   S(-13, -18),   S( 13,  20),   S( -8,   1),   S( -3,  12),   S(  9,  14),   S( -8, -16),   S(-15, -28),
            S(-37, -76),   S( 11,  19),   S( -3, -16),   S(-12, -23),   S(  0,  -7),   S( -1,  -4),   S(-11, -34),   S(  6,   4),

            /* queens: bucket 9 */
            S( -7, -22),   S(-13, -36),   S(  5,   0),   S(-21, -27),   S(-31, -55),   S(-18, -29),   S(-14, -32),   S(-26, -43),
            S(  0,  -9),   S( -7, -24),   S(-15,  -3),   S(-28, -45),   S(-31, -45),   S(-20, -29),   S( -2, -18),   S( -9, -12),
            S( 11,  23),   S(-15, -22),   S(-18,   3),   S(-31, -27),   S(-28, -43),   S(-27, -30),   S(-10,  -9),   S(-13, -38),
            S( -9,  -5),   S(  1,  10),   S( 12,  45),   S(  1,   6),   S( 14,  31),   S(-10, -38),   S(-21, -28),   S( -3, -25),
            S(  1,   9),   S( -8,   7),   S( 21,  43),   S( 21,  63),   S( 22,  43),   S(-17, -11),   S( -7,  -8),   S(-10,  -8),
            S(-44, -55),   S(-18,   1),   S(  2,  23),   S( -1,  25),   S(-34, -44),   S( -3,   8),   S(-23, -32),   S(  6,  11),
            S(-18, -29),   S( -2,  11),   S(-21,  -3),   S( 13,   9),   S( -5, -17),   S( 10,  -1),   S(  5,   3),   S(-32, -52),
            S(-17, -30),   S(-12, -22),   S( -1,   1),   S( -1,  12),   S(  2,  -6),   S(-16, -15),   S( -7, -22),   S( -8, -19),

            /* queens: bucket 10 */
            S( -7, -10),   S( -5,  -2),   S(-18, -39),   S(-29, -34),   S(-16, -18),   S( -2, -13),   S( -5, -11),   S( -4,  -9),
            S(-15, -29),   S(-14,  -8),   S(-21, -36),   S(-23, -22),   S(-22, -26),   S(-17, -29),   S(-10, -26),   S(-25, -34),
            S(  1,  -2),   S( -4,  -7),   S( -4,   5),   S(-12, -18),   S(-21,   6),   S(-21,  17),   S( -4,  -6),   S(  2,   3),
            S( -8, -17),   S(  0,  -5),   S(-21, -22),   S(-24, -35),   S( 27,  53),   S(-28,   2),   S( -4,  -8),   S(-30, -42),
            S(-15, -29),   S( -6, -18),   S(  3,   6),   S( 16,  51),   S( -8,   9),   S(  6,  22),   S(  5, -22),   S( -9, -10),
            S(-10,  -7),   S(-22, -35),   S(-11,  15),   S( -5,  21),   S( 11,  42),   S( -5,  12),   S(  8,   1),   S(-13, -23),
            S( -4,   3),   S(-34, -49),   S( -7,  -6),   S(-40, -44),   S(  4,  11),   S(  2,  14),   S(-11, -16),   S( -5,  -9),
            S( -2,  -5),   S(-10, -20),   S(  6,   4),   S(  9,  16),   S( 16,  15),   S(  6,   6),   S( 10,  11),   S( -8, -21),

            /* queens: bucket 11 */
            S( -9, -24),   S( -8, -27),   S(-18, -27),   S(-18, -38),   S(-28, -38),   S(-20, -18),   S(-15, -21),   S(-29, -48),
            S(-11, -20),   S( -8, -15),   S(-42, -35),   S(-24, -25),   S(-17, -17),   S(-25, -25),   S(  6,   2),   S(-16, -17),
            S(-27, -40),   S(-31, -43),   S(-14, -25),   S(  1,  -7),   S( -8, -14),   S( -2, -19),   S(  5,  15),   S(-14,  -6),
            S(-13, -43),   S(-35, -38),   S( -8, -14),   S(  8,  23),   S(  9,   9),   S(  6,  15),   S( 22,  25),   S(-10,  -9),
            S(-17, -27),   S( -4, -17),   S(-25, -28),   S( 15,  16),   S(  8,  -7),   S( 30,  56),   S( -1,  21),   S(  3,  13),
            S(-19, -38),   S( -2,  -3),   S( -7,   1),   S( 12,  16),   S( 31,  58),   S( 48,  53),   S( 10,   9),   S( -8,   2),
            S(-13, -20),   S(-13, -20),   S(  7,   4),   S(-11,  -1),   S(  2,  13),   S( 14,  39),   S( 27,  17),   S(-25, -41),
            S(-16, -25),   S(  6,   3),   S(-18, -35),   S( 16,  10),   S(-13, -25),   S(-10, -28),   S( 19,  25),   S(-24, -52),

            /* queens: bucket 12 */
            S(  9,   9),   S(  4,   5),   S( -5,  -8),   S(-16, -26),   S( -5,  -8),   S(  2,   4),   S(  1,   2),   S(-12, -20),
            S( -5, -14),   S(-16, -29),   S( -5,  -1),   S( -2,  -5),   S(-10, -20),   S( -6,  -9),   S( -3, -12),   S( -8, -12),
            S(  6,   9),   S( -5, -10),   S( 16,  17),   S(-13, -26),   S( -6, -13),   S( -5, -10),   S(-19, -46),   S( -8, -17),
            S( 13,  22),   S( -5,  -6),   S(  6,  10),   S( -2,   3),   S(  9,  12),   S(  3,  -1),   S( -1,  -2),   S(-13, -32),
            S(  8,   6),   S(  2,  12),   S( 26,  46),   S(  6,  26),   S(  7,  24),   S( -8, -20),   S(-15, -27),   S( -3,  -4),
            S(  9,  25),   S( 12,  24),   S( 15,  34),   S(-14, -23),   S(  4,  10),   S(  9,  19),   S(  7,  16),   S(  0,  -7),
            S(  2,   6),   S( 16,  35),   S( 23,  41),   S(  1,   4),   S(  1,  -6),   S(  4,   6),   S( -3,  -5),   S( -1,  -2),
            S( -4, -14),   S(-28, -58),   S(-15, -13),   S(  1,   0),   S( -1,   0),   S(  3,   9),   S(  1,  -3),   S(-15, -17),

            /* queens: bucket 13 */
            S( -5, -16),   S(-11, -25),   S( -7, -18),   S( -1,  -4),   S(-10, -26),   S(  0,  -4),   S( -9, -20),   S( -7, -15),
            S(  1,  -1),   S(  7,  17),   S(  4,   6),   S( -4, -12),   S(-11, -25),   S(  7,  13),   S( -4,  -8),   S(-15, -32),
            S(  2,   2),   S( -5, -12),   S( -9, -11),   S(  7,  -5),   S(  0,  -3),   S(-16, -33),   S( -4, -12),   S(-26, -45),
            S( -9, -24),   S(  3,  -1),   S( 14,  14),   S( 16,  29),   S( 12,  20),   S(-11, -33),   S(  0,  -5),   S( -4, -11),
            S(-11, -17),   S(  8,  21),   S( 15,  28),   S( 12,  31),   S( 27,  43),   S( -2,  -2),   S(  1,  -7),   S( -9, -22),
            S(  2,  -1),   S(  2,  13),   S( 35,  70),   S( 33,  61),   S(  1,  25),   S( -6, -15),   S(  4,   9),   S(-10, -22),
            S( -6, -11),   S( 12,  26),   S( -2,  12),   S( 15,  36),   S(-11, -22),   S(  1,  -3),   S(  2,   0),   S(  4,   7),
            S(-19, -45),   S(-16, -35),   S(-23, -47),   S(-16, -23),   S(  8,   9),   S(  5,   2),   S( -8, -12),   S( -5, -10),

            /* queens: bucket 14 */
            S( -4, -11),   S( -4, -16),   S( -2,  -8),   S( -2,  -8),   S(  1,   1),   S( -7, -16),   S( -3, -12),   S( -9, -26),
            S(-10, -23),   S(  8,  16),   S(-11, -25),   S(  0, -10),   S(-17, -34),   S( -6, -18),   S(  1,   4),   S(-11, -25),
            S( -1,  -7),   S( -8, -22),   S(-25, -32),   S( -9, -15),   S(-11, -17),   S(-13, -31),   S( -2,  -1),   S(-11, -16),
            S(-11, -24),   S(  6,   5),   S( -7, -14),   S(  9,  18),   S(  3,   9),   S( -3,   4),   S(  6,  12),   S( -9, -24),
            S( -7, -10),   S( -1,   2),   S(-20, -28),   S( 21,  30),   S( 23,  51),   S( 17,  21),   S( 18,  31),   S(  0,  -7),
            S(-11, -23),   S(  1,   8),   S( 15,  27),   S( 13,  22),   S( 20,  39),   S( 26,  48),   S(  7,  11),   S( -4, -12),
            S(  4,   9),   S(  7,   5),   S( 24,  43),   S( 23,  40),   S( 10,   8),   S( 14,  35),   S( 15,  17),   S(  4,   8),
            S(-15, -21),   S(  8,  11),   S(-14, -20),   S( 19,  11),   S( -3, -10),   S( -1, -10),   S(-12, -22),   S(-21, -42),

            /* queens: bucket 15 */
            S(  2,   2),   S( -3, -16),   S( -3, -11),   S( -2,  -6),   S( -8,  -7),   S( -1,  -7),   S(-15, -34),   S(  0,  -1),
            S( -4, -10),   S( -9, -21),   S( -9, -21),   S( -4,  -8),   S( -6, -16),   S(  0,   6),   S( 15,  29),   S( -2,  -6),
            S(  0,  -6),   S( -5, -12),   S( -3,  -8),   S( -9, -24),   S( -1,  -7),   S( 10,  15),   S( -5, -12),   S( -1,  -5),
            S( -5, -14),   S( -3,  -8),   S( -2,  -2),   S(  2,   0),   S( -5,   0),   S(  2,   8),   S(  9,  11),   S(  0,  -6),
            S(-11, -23),   S( -4, -11),   S(-11, -25),   S( -1,  -9),   S(  8,  17),   S(  1,  -3),   S( -6, -15),   S(  0,  -7),
            S( -2,  -8),   S(  1,   0),   S( -6, -11),   S(  5,   9),   S(-17, -43),   S( 26,  56),   S(  3,   7),   S( -3,  -6),
            S( -1,  -1),   S( -7, -28),   S( 11,  17),   S( 11,  15),   S( 15,  32),   S(  2,   1),   S( 16,  19),   S(  8,  14),
            S( -3,  -7),   S( -6, -13),   S(  1,  -3),   S(  7,   6),   S(  8,  12),   S( -2, -16),   S( -8, -19),   S(-12, -35),

            /* kings: bucket 0 */
            S( 13,  47),   S( 19,  69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  55),   S( 82,  71),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-21,  21),   S(-64,  28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 13,  36),   S( -6,  43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-44,  40),   S(-44,  32),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 27,  44),   S( 27,  35),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19,  57),   S( -2,  39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60,  83),   S( 10,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -41),   S( 28, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  -4),   S(  8,  12),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -46),   S(-38, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 16, -15),   S(-13, -12),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-11, -18),   S(-41, -11),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  -8),   S(-18,   1),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 35,  12),   S(-19, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 24,  45),   S(-34,  19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-110, -47),  S(  1, -13),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -56),   S( 17, -16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  2, -47),   S(-36, -51),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 69, -57),   S( 15, -41),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  6, -53),   S(-54, -36),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 69, -62),   S( 70, -47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  3, -23),   S(-103, -54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 74, -41),   S( 16, -62),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -5, -40),   S( 45, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -115),  S( -1, -18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 24, -28),   S( 49, -30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 13, -63),   S( 25, -72),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 52, -43),   S( 30, -44),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15, -68),   S( -3, -61),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23, -26),   S(-36, -52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10, -62),   S(-17, -110),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36,  -5),   S(-41,  15),   S( -6,  -1),   S(-24,  43),   S( -2,  -2),   S( 24,  13),   S( 24,  -7),   S( 34,  -1),
            S(-20, -31),   S(-31,  -6),   S(-19,  -6),   S(-10,  -6),   S(  8,  -1),   S( -6,  10),   S( 21,  -3),   S( 14,  16),
            S(  4, -20),   S(  0, -22),   S( 27, -30),   S(  9, -25),   S( 14,  -4),   S( 23,  31),   S(  9,  25),   S( 39,  10),
            S( 23, -22),   S( 42,  -8),   S( 50, -15),   S( 46, -11),   S( 17,  49),   S( -9,  72),   S( 20,  65),   S( 77,  42),
            S( 76, -48),   S(116,   1),   S(111,  -3),   S( 58,  14),   S( 71, 103),   S( 23,  99),   S( 15, 151),   S(100,  90),
            S(-183, -99),  S(-132, -108), S( 46, -148),  S( 40,  33),   S(114, 153),   S( 83, 158),   S(160,  88),   S( 81, 153),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61,  17),   S(-51,  14),   S(-33,   9),   S(-47,  42),   S(-35,  10),   S( -9,  10),   S( -4,  -5),   S( -1,  17),
            S(-58,  -3),   S(-42,  -3),   S(-38,  -6),   S(-24,   2),   S( -6,   1),   S(-34,   7),   S( -4, -10),   S(-27,  13),
            S(-39,  12),   S(-12,   5),   S(-20,  -1),   S( 10, -18),   S( -1,  16),   S(-11,  11),   S(-21,  14),   S( -4,  14),
            S(-21,  26),   S( 35,   0),   S( -3,  15),   S( 22,  24),   S( 17,  14),   S(-23,  24),   S( 15,  22),   S( 40,  42),
            S( 31,  12),   S( 83, -19),   S(128, -39),   S(103, -20),   S( 46,  14),   S( 18,  27),   S(-32,  57),   S( 45,  84),
            S(172, -43),   S( 11, -35),   S( 51, -132),  S( 36, -117),  S(-35, -73),   S(-27,  63),   S( 78, 141),   S( 92, 152),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-65,  42),   S(-43,  16),   S(-39,  11),   S(-26,  17),   S(-57,  44),   S(-39,  20),   S(-24,  -4),   S(-28,  21),
            S(-58,  15),   S(-42,  14),   S(-46,   1),   S(-43,  19),   S(-42,  19),   S(-48,   7),   S(-24, -13),   S(-53,  11),
            S(-40,  41),   S(-34,  40),   S(-19,  17),   S(-23,  18),   S(-31,  30),   S(-23,  10),   S(-33,   9),   S(-22,   8),
            S(-14,  66),   S(-27,  55),   S( -8,  36),   S(  5,  29),   S( -7,  30),   S(-23,  18),   S(  8,  12),   S( 32,   2),
            S(-29, 114),   S(-51,  94),   S( -8,  24),   S( 30, -21),   S(100, -22),   S( 92,  -4),   S( 82, -22),   S( 66,  -2),
            S( -3, 211),   S( 54, 112),   S(  1,  50),   S(-19, -64),   S(-44, -146),  S(-35, -144),  S( 11, -77),   S( 84, -44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-14,  23),   S(-14,  12),   S(-10,  21),   S(-17,  37),   S(-25,  52),   S(  4,  39),   S( -5,  10),   S(-17,   0),
            S( -5,   7),   S(  0,  13),   S(-17,  15),   S(-19,  19),   S( -3,  24),   S(  1,   9),   S( -3,  -3),   S(-38,   5),
            S(  9,  36),   S( -9,  51),   S(  2,  24),   S( -3,   9),   S( 19,  -1),   S( 21,   1),   S( -1,  -8),   S(-13,  -1),
            S( 17,  83),   S( -3,  87),   S(  8,  70),   S( 15,  32),   S( 30,   6),   S( 29, -14),   S( 22,  12),   S( 42, -17),
            S( -1, 148),   S(-22, 163),   S(-29, 168),   S(-12, 121),   S( 39,  53),   S( 80,  -2),   S( 79,  -7),   S( 96, -16),
            S( 72, 137),   S( 46, 220),   S(-12, 265),   S(  8, 186),   S(  0,  59),   S( 14, -137),  S(-120, -179), S(-182, -156),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 76,  -3),   S( 19,  -7),   S( -7,  -7),   S(-11, -31),   S( -6, -16),   S(-11, -13),   S(-16, -10),   S(-57,  26),
            S( 61, -31),   S( 27,  -4),   S( 16, -22),   S(-23,  -4),   S(-27, -17),   S(-14, -24),   S(-42, -17),   S(-50,  -4),
            S( 89, -30),   S(130, -46),   S( 55, -33),   S(-11, -14),   S(-68,  10),   S(-35,  16),   S(-74,  20),   S(-25,   7),
            S(-14, -88),   S( 18, -120),  S( 44, -28),   S(-47,  19),   S(-11,  22),   S(-63,  59),   S( 22,  32),   S(  3,  29),
            S( 33, -100),  S(-48, -115),  S( 19, -92),   S( 52,   0),   S( 72,  77),   S( 37,  73),   S( 65,  57),   S( -9,  88),
            S(  6, -55),   S(-14, -56),   S( -4, -112),  S( 37,  52),   S( 43,  89),   S( 61, 143),   S( 49, 127),   S( 42, 109),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-63,  35),   S(-43,  28),   S( 10,   5),   S( 81,   0),   S( 68, -17),   S( 32, -12),   S(-22,   1),   S(-62,  40),
            S(-50,   9),   S(-10,   5),   S(  6, -10),   S(  7,   5),   S( -7,   3),   S(-12,  -8),   S(-59,   0),   S(-84,  23),
            S(-15,  16),   S( 24,  13),   S( 77,  -5),   S(  8,  25),   S( -3,  21),   S( -6,   2),   S(-27,  11),   S(-52,  30),
            S( 24,   6),   S( 50, -27),   S(-24, -38),   S( -9, -17),   S(-12,  -8),   S( 11,   2),   S( 52,   1),   S( 27,  19),
            S( 50,  -7),   S( 53, -50),   S( 29, -82),   S( -5, -64),   S( 70, -49),   S( 58,  -3),   S( 64,  19),   S(  1,  87),
            S( 66,  13),   S( 24, -20),   S(  1, -77),   S( 18, -95),   S(-10, -69),   S( 45,  37),   S( 56,  90),   S( 89,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-88,  32),   S(-41,  -2),   S(-21,  -1),   S(  7,  10),   S( 22,  10),   S( 32,   1),   S( 22,  -4),   S( -8,  21),
            S(-71,   0),   S(-30, -11),   S(-23, -15),   S( 30,  -6),   S(  3,   9),   S( 16, -13),   S(  5,  -8),   S(-15,   2),
            S(-38,  19),   S(-28,  11),   S( -6,   4),   S( 13,   7),   S( 30,  21),   S( 68, -10),   S( 62, -12),   S( 20,  -1),
            S( -8,  45),   S(  9,  17),   S( 22,   6),   S( 25,   0),   S(  5, -39),   S(  4, -40),   S( 54, -27),   S( 90, -32),
            S( 26,  68),   S( 47,  15),   S( 54,   0),   S( 12, -47),   S(  0, -64),   S(-45, -51),   S( 50, -53),   S(102, -12),
            S(115,  49),   S(130,  54),   S( 65,   2),   S( 61, -65),   S(  7, -101),  S(  1, -99),   S(-12, -43),   S( 68,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  -1),   S(-46, -19),   S( -1, -30),   S( -9,   2),   S( 49, -10),   S( 63, -13),   S( 56, -28),   S( 59,  -7),
            S(-46, -17),   S(-57, -13),   S(-47, -21),   S(-35,   0),   S(  6,  -7),   S( 57, -35),   S( 49, -25),   S( 62, -26),
            S(-41,  17),   S(-54,  16),   S(-19,  -8),   S(-33, -10),   S( 25, -15),   S( 47, -26),   S( 80, -25),   S( 75, -25),
            S( -3,  33),   S(-58,  49),   S( -7,  34),   S(-11,  25),   S( 23, -16),   S( 57, -59),   S( 16, -60),   S( 39, -97),
            S( 35,  41),   S(-16, 100),   S( 55,  83),   S( 31,  70),   S( 25,  21),   S( -1, -62),   S(-11, -126),  S(-16, -82),
            S(108,  79),   S(108, 103),   S(120,  99),   S( 73, 102),   S( 52, -27),   S(  1, -91),   S( -6, -103),  S( 10, -148),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 35, -10),   S( 22, -11),   S( 47, -13),   S(-13, -39),   S(-30, -72),   S( 22, -41),   S( 46, -65),   S( 31, -37),
            S(-36, -34),   S(-10,   0),   S(-58, -61),   S(-51, -32),   S(-69, -30),   S(  9, -47),   S(-17, -43),   S(-41, -46),
            S(-33, -60),   S( 16, -71),   S(-25, -45),   S(-25, -59),   S( -2, -13),   S(-46, -14),   S(-22, -15),   S(-58,  -4),
            S( -1, -11),   S(-28, -23),   S( 37, -20),   S( 19, -23),   S(  8,  -4),   S( 12,  29),   S(-11,  32),   S( -9,  37),
            S( 16,  63),   S(-17, -20),   S( 28,  58),   S( 34,  79),   S( 54, 112),   S( 40, 111),   S( -2,  78),   S( -4,  69),
            S( 32,  73),   S( 11,  42),   S( 25,  60),   S( 33,  72),   S( 53,  97),   S( 49, 157),   S( 33, 110),   S(  8,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 39,  -2),   S( 78, -18),   S( 48,  -8),   S(  6,  -2),   S( 16,   1),   S( 63, -39),   S( 85, -62),   S(-38, -22),
            S(  0, -41),   S(-25, -54),   S(-24, -45),   S( -8, -19),   S(-33, -29),   S(-23, -41),   S(-15, -45),   S(  5, -47),
            S(-82, -14),   S(-16, -52),   S(  5, -83),   S(-66, -38),   S(-13, -37),   S(  5, -54),   S(-50, -33),   S(-28, -11),
            S(-50,   1),   S(-48, -50),   S(  4, -56),   S(-19, -15),   S(-16, -38),   S(  4, -31),   S(-14,   5),   S( 29, -13),
            S(-29,   8),   S(  7, -23),   S( 23, -28),   S( 19,  15),   S(  8,  61),   S( 36,  19),   S(-12,  43),   S( -4,  51),
            S(-14,  47),   S( 23,  44),   S( 13,  45),   S( 32,  16),   S( 27,  69),   S(  7,  67),   S( 11,  73),   S( 25,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -60),   S( -9, -55),   S(  5, -27),   S(  3, -26),   S( 41, -30),   S(161, -32),   S( 68, -22),   S( 69, -33),
            S(-51, -54),   S(-61, -55),   S( 20, -73),   S( 50, -54),   S(  1, -32),   S( 15, -34),   S( 37, -45),   S( -1, -42),
            S(-52, -31),   S(-22, -48),   S( -4, -50),   S( 17, -53),   S(-17, -36),   S(-16, -64),   S(-16, -73),   S( 41, -51),
            S(-23, -15),   S(  4, -32),   S(-10, -36),   S(-30, -49),   S(-50, -44),   S(-38, -43),   S( -5, -54),   S( -7, -30),
            S(  8,   4),   S( 35,  20),   S( 32,   0),   S( -1, -40),   S( 47,   8),   S( -3,   0),   S(-13,  -7),   S( 10,   6),
            S(-10,  27),   S( -7,  55),   S( 27,  55),   S( 31,  52),   S( 34,  56),   S( 12,   9),   S(-10,   4),   S( 26,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -51),   S(-33, -39),   S( -4, -37),   S(-14, -34),   S( 21,  -5),   S(101, -14),   S(112, -49),   S( 55, -18),
            S(-66, -47),   S(-36, -70),   S(-21, -68),   S(-12, -46),   S( -9, -27),   S(-21, -35),   S( 11, -33),   S( 38, -60),
            S(-61,  -6),   S(-54, -27),   S( -2, -23),   S(-15, -27),   S(-58, -37),   S(-10, -37),   S(-39, -73),   S( 30, -74),
            S(-16,  17),   S(-17,   4),   S( 26,  22),   S( 57,  -3),   S(  3, -10),   S( -9, -33),   S(-35, -46),   S(-12, -47),
            S( 18,  11),   S(  7,  62),   S( 35,  75),   S( 51,  70),   S( 31,  51),   S( 15, -10),   S( 17,  44),   S( -8, -19),
            S( 36,  62),   S( 27,  69),   S( 14,  71),   S( 18,  70),   S( 16,  38),   S( 47,  86),   S( 16,  36),   S( 23,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -87),   S( -7,  15),   S(-15,  -5),   S( -3,   6),   S( -4, -22),   S(-31, -47),   S( -2, -26),   S( -4,  -8),
            S(-17, -44),   S(-38,  -5),   S(-19, -45),   S(-28, -27),   S(-34, -57),   S(-14, -32),   S( -9, -51),   S(  3, -42),
            S(-44, -20),   S( 11, -36),   S(-12, -64),   S(-23, -75),   S(-21, -22),   S(-28,  -2),   S(-40, -25),   S(-46, -26),
            S(-16,   8),   S(  2,  11),   S( -2,   9),   S( 16,  26),   S( 13,  66),   S(  7,  85),   S(  0,  63),   S(-36,  34),
            S( 10,  45),   S(  8,  32),   S(  6,  11),   S( 10,  48),   S( 36, 113),   S( 32, 100),   S( 10, 111),   S(  5, 111),
            S( 12,  34),   S(  6,  26),   S( 23,  63),   S( 15,  53),   S( 30, 124),   S( 32,  84),   S(-28, -10),   S( -9,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -65),   S(-31, -61),   S( 10,  -1),   S( -7, -23),   S(  3, -31),   S(-32, -30),   S( -2, -50),   S( -7, -45),
            S(-62, -56),   S(-35, -52),   S(-31, -54),   S(  6, -27),   S(-11, -47),   S(-40, -30),   S(-53, -58),   S(-21, -42),
            S(-44, -25),   S(-20, -61),   S( -6, -63),   S(-10, -54),   S( -8, -41),   S(  6, -42),   S(-43, -40),   S(-18, -18),
            S(-31,  39),   S( -8,  -5),   S( 15,   8),   S( -3,  22),   S(  4,  42),   S(-23,  40),   S( -1,  -4),   S(-32,  33),
            S( -8,  14),   S(  1,   6),   S( 12,  27),   S( 27,  50),   S( 41, 120),   S( 27,  84),   S( 13, 122),   S( -3,  89),
            S( 10,  60),   S( 33,  86),   S(  7,   2),   S( 10,  28),   S( 21,  93),   S( 14,  80),   S( 10, 102),   S( 15, 101),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36, -72),   S( 10, -82),   S(  0, -41),   S(  5,  13),   S(-12, -39),   S(  5, -20),   S(  7, -56),   S( -8, -39),
            S(-26, -82),   S(-82, -81),   S( -8, -89),   S(-40, -120),  S(-23, -57),   S(-13, -46),   S(-31, -18),   S(-25, -51),
            S(-17, -48),   S(-27, -67),   S(-41, -57),   S(  2, -40),   S(-14, -65),   S(-20, -58),   S( -9, -40),   S(-43,  -7),
            S( -6,  -2),   S(-22,   6),   S(-13,  -9),   S( -5, -19),   S(  6, -25),   S( 10, -14),   S(  6, -24),   S( -6,  22),
            S(-11, -14),   S(  1,  17),   S( -4,  51),   S(  6,  27),   S( 22,  59),   S( 13,  45),   S( 27,  79),   S( 14,  93),
            S( -8,  42),   S(  6,  30),   S( 29, 109),   S( 27,  85),   S( 18,  73),   S(  2,  19),   S( 26,  95),   S( 31, 116),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -5, -25),   S(  3, -45),   S(-27, -67),   S(-13, -33),   S(-11, -26),   S(-27, -27),   S(-21, -21),   S( -1, -42),
            S(-21, -50),   S(-43, -108),  S(-17, -81),   S( -7, -38),   S( -8,  -6),   S(-25,   3),   S(-35,  -8),   S(-25, -63),
            S(-27, -11),   S(-56, -38),   S(-31, -33),   S( 28,   0),   S(-22, -29),   S(-20, -28),   S( -6, -38),   S(-18,   8),
            S(-35,  13),   S(-28,  -9),   S(  3,  33),   S( -1,  16),   S( 22,  34),   S( 12,  27),   S(  6,  -1),   S(  6,  30),
            S( 10,  75),   S( 15,  32),   S( -5,  68),   S( 15,  50),   S( 20, 109),   S(  9,  34),   S(  6,  46),   S(  9,  -8),
            S(-12,  38),   S(-12,  18),   S( 11,  76),   S( 23,  58),   S( 25,  95),   S(  8,  26),   S( 18,  48),   S( 12,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-56, -29),   S(-46,  -4),   S(-10,  -3),   S( -2,  31),   S(-36,  -4),   S(-29,  27),   S( -6, -11),   S(-73, -19),
            S( 37, -56),   S(-32,   2),   S(-30, -17),   S(  3, -14),   S(-18,  13),   S(-28,  12),   S(-18, -78),   S(-15, -59),
            S(  3, -35),   S( 11,   4),   S( -5,  26),   S( 33,  40),   S(-18,  42),   S(  2,   6),   S(-27,  25),   S(-20, -55),
            S(-10,  56),   S( 23,  66),   S( 13,  61),   S( 34,  38),   S(  4,  55),   S(  9,  44),   S( 46, -46),   S(-20,  30),
            S( 39,  65),   S(-19,  63),   S( 43,  73),   S( 53,  46),   S( 72,  43),   S( 15,  39),   S( 15,  -5),   S( -1,  13),
            S(132, -22),   S(-42, 103),   S(116,  26),   S( 79,  40),   S( 75,  41),   S(-45,  90),   S( 55,  -5),   S(-26,  25),
            S( 37,  -6),   S(-14, -11),   S( 49, -28),   S( 81,  73),   S( 79,  58),   S( 33,  33),   S( -7,  11),   S(-21,  26),
            S(-122, -162), S(  1,  -3),   S( 19,  -1),   S( 10,  36),   S( 12,  48),   S( 34,  16),   S(-22,  12),   S( -1,  10),

            /* knights: bucket 1 */
            S(-18,  -1),   S(-62,  16),   S(-36,  31),   S(-52,  63),   S(-34,  50),   S(-28, -25),   S(-33,  -2),   S(-27,   2),
            S(-56,  40),   S(-15,  57),   S(-36,  51),   S(-15,  32),   S(-26,  35),   S( -2,  23),   S(-15,  12),   S(-32, -36),
            S(-36,  17),   S( -2,   4),   S(-24,  26),   S(-14,  65),   S(-14,  50),   S(-21,  19),   S(-48,  47),   S(-18,  27),
            S(-14,  87),   S(  4,  53),   S(  3,  47),   S(-18,  68),   S(-14,  67),   S(-14,  75),   S(-15,  41),   S(-29,  49),
            S( 56,   2),   S(-18,  32),   S( 14,  80),   S( -7,  66),   S( 29,  54),   S(  8,  49),   S(-19,  57),   S(-20,  92),
            S(-16,  62),   S( 70, -12),   S(115,  16),   S(102,  42),   S( 59,  33),   S(-52,  77),   S( 43,  18),   S( 37,  30),
            S( 29,   6),   S( 35,   3),   S( 32,  -7),   S( 40,  38),   S( 14,  25),   S( 37,  14),   S( 15,  79),   S(-24,  44),
            S(-154, -43),  S( 18, -27),   S(-22, -61),   S( -4,  14),   S( 11,  11),   S( 50,  62),   S(  0,  34),   S(-90,  14),

            /* knights: bucket 2 */
            S(-64,   9),   S(-40,  28),   S(-26,   4),   S(-14,  29),   S(-20,  24),   S(-48,  15),   S(-31,  22),   S(-46,  -4),
            S(-36,  11),   S(  4,  40),   S(-34,  37),   S(-15,  36),   S(-36,  47),   S(-12,  12),   S(  1,  24),   S(-17,   7),
            S(-43,  59),   S(-25,  46),   S(-31,  37),   S(-26,  69),   S(-22,  62),   S(-29,  33),   S(-31,  38),   S(-14,   4),
            S( -8,  56),   S(-13,  56),   S(-24,  89),   S(-37,  92),   S(-34,  79),   S(-12,  64),   S( -8,  66),   S( -6,  35),
            S(-20,  76),   S(-30,  76),   S( -6,  79),   S( 12,  51),   S(-35,  93),   S(  3,  74),   S(-37,  76),   S( 16,  26),
            S(-51,  95),   S( -8,  74),   S(-33, 109),   S( 62,  34),   S( 46,  55),   S(130,  17),   S( 57,  45),   S(-42,  36),
            S( 58,  55),   S(-23,  61),   S( 53,  20),   S( 35,  -4),   S(-43,  44),   S( 20,  -1),   S( 39,  23),   S( -2,   5),
            S(-60,  49),   S( 29,  72),   S( -6,  81),   S( -6,   0),   S(-25, -23),   S( -6, -34),   S( 20,   5),   S(-144, -42),

            /* knights: bucket 3 */
            S(-66,  56),   S(-25, -14),   S( -9,  10),   S( -9,  40),   S( -6,  15),   S(-11,  17),   S(-24,   9),   S(-42, -36),
            S(-36,  17),   S(-12,  47),   S( -6,  34),   S( -8,  35),   S(-16,  35),   S( 18,   8),   S( 28,  -3),   S( 15, -19),
            S(-24,  31),   S(-17,  43),   S(-10,  49),   S( -9,  83),   S(  4,  73),   S(-10,  52),   S(  7,  37),   S(  1,  16),
            S( -4,  32),   S( -8,  72),   S(  4,  82),   S(-14, 100),   S(  4,  99),   S(  8, 101),   S(  9,  93),   S(  3,  71),
            S(-10,  79),   S( -6,  77),   S(  8,  97),   S( 25, 112),   S( -4, 104),   S(  7, 114),   S(-38, 120),   S( 45, 109),
            S(-35,  70),   S( 20,  71),   S( 11,  95),   S( 13, 108),   S( 50, 112),   S( 87, 126),   S( 39, 108),   S(-12, 140),
            S(-36,  85),   S(-24,  78),   S(-33, 102),   S( 32,  81),   S( 23,  97),   S( 71,  67),   S( 21,  13),   S( 73,  -7),
            S(-163,  64),  S(-45, 114),   S(-83, 123),   S( 25,  92),   S( 40, 108),   S(-79,  76),   S(  6, -31),   S(-37, -104),

            /* knights: bucket 4 */
            S(  6,   2),   S(  6,  14),   S(-58,  -4),   S(-38,  -7),   S(-33,   4),   S(-19, -30),   S( -4, -41),   S(-19, -27),
            S( 35,   8),   S( 18, -29),   S( 11,  18),   S(-26,  13),   S( 35, -25),   S(  0, -47),   S( 15,   3),   S(-28,  16),
            S( -7, -32),   S( 49, -25),   S( 45, -14),   S( 87, -10),   S( 29,  23),   S( 40, -16),   S(-24, -25),   S( 14, -23),
            S(-29, -25),   S(  2, -27),   S( 76, -14),   S( 56,  18),   S( 23,  18),   S( 39,  27),   S(-47,  22),   S(-16,   5),
            S( -1, -37),   S(  5, -12),   S( 75,  36),   S( 35,  60),   S( 80,   4),   S( 16,  48),   S( 20,  -4),   S(-61,  29),
            S( -3,  -6),   S(-10, -18),   S( 44,  -5),   S( 91,   5),   S(-19,  20),   S(  5,  48),   S(  1,  35),   S( 33,  40),
            S( -6,   5),   S(-28, -11),   S(  1,   1),   S( 14,  41),   S( 21,  21),   S( -9,  41),   S( -2,  24),   S(-39, -14),
            S( -6,  -1),   S(-12, -34),   S(-12, -19),   S(  6,   2),   S(  3,  11),   S( -5,   3),   S(  8,  31),   S( -6,  -4),

            /* knights: bucket 5 */
            S( 31,  52),   S( -5,  23),   S(-19,  27),   S( 18,  14),   S( 26,  29),   S( 31,  13),   S( -5,  14),   S( -3, -14),
            S( 12,  29),   S( 32,  33),   S( 35,  -6),   S(-13,   5),   S( 83, -28),   S(  4,  -3),   S( 18,  49),   S(-48,  28),
            S( -5,  -8),   S(-26,  12),   S( 20,   6),   S( 48,   6),   S( 47,   6),   S(-16,  18),   S( 35, -16),   S(-46,  18),
            S( 61,  -9),   S( 51, -33),   S( 71,  -5),   S( 73, -13),   S( 99,  -5),   S( 77,   7),   S( -2,  15),   S( 40,   7),
            S( 49, -10),   S( 50, -12),   S(111,  -5),   S(114, -14),   S(137, -23),   S( 57,  19),   S( 46,   0),   S( 18,  36),
            S( -3, -23),   S( 33, -36),   S( 17, -33),   S(-29,   5),   S( 39, -16),   S( 58,   6),   S( -4,  14),   S( 32,  29),
            S(-12,   5),   S(-42, -43),   S( -5, -31),   S( -5, -12),   S(-21, -60),   S( 33,   1),   S(  2,  30),   S( 19,  40),
            S(-31, -32),   S(-31, -50),   S( -8, -40),   S(-17, -29),   S( 14,   5),   S(-20,  19),   S( 17,  38),   S( 10,  11),

            /* knights: bucket 6 */
            S( 22,  -2),   S(-25,  16),   S( 11, -13),   S(-42,  32),   S(-18,  26),   S( 30,  21),   S(-38,  56),   S(-21,  40),
            S( 25, -15),   S( 19,  30),   S( 12,  -7),   S( 57,   9),   S( 71,  -6),   S(-15,  17),   S( -6,  59),   S(-41,  56),
            S(  7,  13),   S( 40,  13),   S( 46,  10),   S( 39,  32),   S( 50,  25),   S(-42,  33),   S( 23,  16),   S(-52,  38),
            S( 47,  39),   S( 71,  -4),   S( 71,   8),   S( 97,  -3),   S(101,  -4),   S( 84,   7),   S( 67,   7),   S(-21,  46),
            S(-16,  46),   S( 69,   0),   S(116,   4),   S(105,   4),   S(125, -15),   S(101,  10),   S(146, -18),   S( -6,  16),
            S( 22,  25),   S( 36,   5),   S( 56,  14),   S( 72,  13),   S( 49, -14),   S( 52,   0),   S( 16,  -6),   S( 38, -17),
            S( 12,  39),   S( 20,  36),   S( 47,  42),   S( -3,  -9),   S( 30,  22),   S( -7, -39),   S(-17, -18),   S(  5,  53),
            S( 30,  19),   S( 11,  27),   S( 13,  29),   S( 15,  23),   S( 19, -25),   S(  7,   3),   S(  5,  26),   S(-38, -43),

            /* knights: bucket 7 */
            S(-11, -21),   S(  8, -37),   S( 23, -28),   S(-32,  11),   S(-27,  12),   S(-66,  54),   S( 11,  -3),   S(  0,  36),
            S(-22, -38),   S( 17, -28),   S(-17,  10),   S( 17,   2),   S( 21,  10),   S( 53,   5),   S(-30,  31),   S(-61,  59),
            S( -9, -52),   S(-57,  20),   S( 26, -11),   S( 17,  38),   S( 75,   8),   S( 44,   8),   S( 52,   8),   S(-14,  35),
            S(-33,  25),   S( 72,  -7),   S( 33,  11),   S(105,   0),   S(108,  -2),   S( 88,   7),   S( 82,   0),   S( 82,  14),
            S(  9,  18),   S( 16,  21),   S( 19,  32),   S( 85,   6),   S(177, -14),   S(175, -27),   S(196,   3),   S( 44,  21),
            S(-30,  12),   S( 13,  30),   S( 11,  37),   S( 63,  18),   S(108,  13),   S(124, -14),   S( 28, -27),   S(-22, -64),
            S(-23,  16),   S(-11,  18),   S(  9,  26),   S( 27,  44),   S( 76,  22),   S( 25,  17),   S(  1, -26),   S(  2,   2),
            S( -8,  -9),   S(-18,  15),   S(  3,  23),   S( -6,  11),   S(-15,  31),   S( 30,  -7),   S(  7, -12),   S( -5, -18),

            /* knights: bucket 8 */
            S( -1,  -9),   S( 10,   8),   S(  6,   1),   S(-35, -89),   S( -2, -32),   S( -7, -37),   S( 16,  20),   S( -3, -28),
            S(-19, -69),   S( -9, -57),   S( -4, -46),   S( -2, -16),   S(  0, -30),   S( -6, -43),   S(  2, -18),   S( -1, -20),
            S(-17, -81),   S(-11, -29),   S(  4, -94),   S( 17, -46),   S( 14, -47),   S( 39,   0),   S( -8, -38),   S( 10, -12),
            S(-19, -75),   S(-12, -55),   S( 15,  27),   S( 24,  18),   S( 13, -33),   S(  3, -18),   S(-23, -34),   S(-17, -50),
            S(-13, -38),   S(  3, -27),   S(  6,  -8),   S( 14, -40),   S( 13, -32),   S(-18, -33),   S( -3,  -5),   S(  4,  -7),
            S(  3,   9),   S(  2, -55),   S(  6, -16),   S( 11, -21),   S( -3, -45),   S(  3, -35),   S( -4, -21),   S(-13, -15),
            S( -9,  -8),   S( -2, -44),   S( -7, -25),   S(  4, -13),   S(  4,  -2),   S(  0, -37),   S( -3,   8),   S( -4,  -9),
            S(  1,   2),   S( -4, -14),   S( -7, -28),   S(  2,   6),   S( -5,  -1),   S(  2,  17),   S( -1,  -8),   S( -4,  -7),

            /* knights: bucket 9 */
            S(-15, -61),   S( -7, -26),   S( -8, -60),   S(-12, -69),   S(-23, -31),   S(-18, -28),   S( -7, -43),   S( -1, -16),
            S( -7, -19),   S(-18, -56),   S( -4, -112),  S( -5, -58),   S(-13, -40),   S(-19, -75),   S(  1, -39),   S(-15, -50),
            S(-10, -61),   S(-20, -69),   S( -7, -37),   S( -1, -67),   S( -3, -22),   S( 32, -11),   S(  1, -23),   S( -4, -30),
            S(-25, -60),   S(  0, -38),   S( -4, -52),   S(-18, -75),   S( -1, -55),   S( 16, -25),   S(-15, -70),   S(  3, -23),
            S(  0,  12),   S(-11, -21),   S( -2, -29),   S( 32, -17),   S(-31, -87),   S(-15, -24),   S( -5, -41),   S(  5, -31),
            S(-16, -70),   S(-16, -75),   S( -3, -58),   S( 17, -43),   S(  2, -20),   S( 12, -13),   S(  2, -28),   S(  6,  38),
            S( -3, -14),   S( -9, -30),   S( -5, -22),   S(-15, -29),   S( -8, -51),   S( 11,  20),   S(  8,  34),   S( -3,  13),
            S(  2,  15),   S( -2, -14),   S( -6,  -1),   S(  0, -17),   S( -2, -25),   S( -2, -15),   S(  4,  14),   S( -2,  -2),

            /* knights: bucket 10 */
            S(-13, -58),   S(-15, -48),   S(-12, -61),   S(-19, -16),   S( -5, -74),   S(  2, -55),   S( -4,  -2),   S(  0, -19),
            S( -6, -34),   S( 20, -15),   S(-10, -53),   S( -3, -91),   S( 14, -64),   S( -7, -82),   S( -5, -17),   S(  2,  22),
            S(  2,  -4),   S( 11, -48),   S( -1, -28),   S( 13, -57),   S( -2, -56),   S(  4, -55),   S( -5, -38),   S( -9,  -8),
            S( -1, -30),   S(-10, -53),   S( 22, -62),   S( 22, -33),   S( 11, -55),   S(  3, -40),   S( -4, -60),   S( -1, -49),
            S(-13, -43),   S(  9, -45),   S(  2, -40),   S(  3, -26),   S(  0, -38),   S( -5, -60),   S(  7, -32),   S(  0, -34),
            S( -2,  -1),   S(  8, -16),   S(  8, -21),   S( -2,  -5),   S(-18, -33),   S(-20, -52),   S(-14, -48),   S(-17, -43),
            S(  5,   1),   S(  2, -11),   S(-12, -54),   S( 21, -27),   S( -9, -40),   S( -3, -44),   S(-12, -28),   S( -6, -11),
            S( -5, -15),   S(  3,  10),   S( -4,  -2),   S(-11, -46),   S( -6, -43),   S( -3, -12),   S(  0, -16),   S(  0,  -1),

            /* knights: bucket 11 */
            S( -6, -19),   S(-12, -25),   S( -4, -23),   S(  2,  -8),   S(-30, -67),   S(  1, -31),   S( -5, -12),   S(  7,  29),
            S( -3, -18),   S(-12, -29),   S( -5, -94),   S( 38, -51),   S( 13, -10),   S( -7, -55),   S(  2, -22),   S(-14, -36),
            S( -9, -37),   S(-15, -57),   S( 35, -28),   S( 14, -23),   S(  5,  -7),   S( 20,  -6),   S( -8, -71),   S( -5, -30),
            S(-10,  -8),   S( 10, -20),   S( 10, -45),   S( 34, -21),   S( 47, -29),   S( 10, -34),   S( 10, -30),   S(  0, -18),
            S(-20, -13),   S(  0, -52),   S( 12,  -5),   S( 15, -17),   S( 46,  13),   S( 20,  11),   S(-12, -99),   S(-12, -45),
            S( -7, -44),   S( -6, -55),   S( 20,   0),   S( 11, -28),   S( 40,  11),   S( -1,   3),   S(-12, -58),   S( -5,  -6),
            S( -3, -16),   S(  3,  18),   S(  3,  -1),   S( 10,  -5),   S(  5, -29),   S( 17, -47),   S(  4, -27),   S( -2,  -7),
            S(  3,  -4),   S( -4, -16),   S( -9, -27),   S(  0,  28),   S(  7,   6),   S( -6, -39),   S( -1,  -8),   S( -2,  -3),

            /* knights: bucket 12 */
            S(-19, -68),   S(  1, -24),   S(  1,   1),   S( -5,  -5),   S( -5,  -3),   S(  1,  -6),   S( -4,  -9),   S( -2,  -6),
            S( -7, -50),   S(  2,   6),   S( -6, -35),   S( -6, -16),   S(  2, -13),   S( 12,  18),   S(  8,   5),   S(  3,   7),
            S(  2,   3),   S( -9, -49),   S( -2,  -6),   S( -6, -71),   S( -5, -32),   S( -3, -33),   S(  0,  -8),   S( -5, -16),
            S(  1,   3),   S(-10, -86),   S(  0, -19),   S( 15,   5),   S(  7, -43),   S( -2,  -5),   S(  3,   4),   S(  4,  21),
            S( -6, -28),   S(-13, -49),   S( -4, -51),   S( -5, -42),   S( 13,  22),   S(  3,   3),   S(-11, -32),   S(-12, -27),
            S( -7, -19),   S(  2, -19),   S( -3, -30),   S( -6, -31),   S( -9, -43),   S(  1,  -7),   S(  5,  -3),   S(  1,   8),
            S( -1,  -7),   S( -3,  -5),   S( -9, -10),   S(  1,  14),   S( -6, -13),   S(-12, -24),   S( -7, -21),   S( -5, -10),
            S(  0,   0),   S(  3,  23),   S(  0,  -8),   S(  2,   0),   S(  0,  -4),   S(  3,  14),   S(  0,  -4),   S(  1,  -2),

            /* knights: bucket 13 */
            S( -5, -14),   S(-10, -42),   S( -6, -61),   S( -4, -21),   S( -5, -24),   S( -8, -29),   S( -6, -16),   S(  1,   4),
            S( -8, -22),   S( -1,  -4),   S( -1, -13),   S( -3, -26),   S(-11, -48),   S( -7, -35),   S( -3, -19),   S( -4, -19),
            S(  1,   5),   S(  1,  -1),   S(  4, -11),   S(  7, -15),   S(  5,   8),   S( -4, -19),   S(  7,   3),   S( -7, -35),
            S( -3, -10),   S(  0, -10),   S( -5, -27),   S(  8,  -5),   S(  3, -49),   S( -5, -25),   S(  1, -20),   S( 15,  14),
            S( -3,   7),   S( -4, -45),   S( -1, -56),   S(  2, -32),   S( -7, -47),   S( 12,  13),   S( -6, -44),   S( -2,  -1),
            S( -7, -26),   S( 10,  31),   S(-12, -22),   S( 13,  10),   S( -7,  -8),   S( -1, -51),   S(  6,  25),   S(  1,   4),
            S( -1,  -6),   S(  5,   3),   S(  1,  10),   S( -5, -29),   S( -2,   1),   S( -3,  -5),   S(  3,   2),   S(  1,  -3),
            S(  1,   3),   S(  0,   0),   S(  1,   4),   S(  0,   7),   S(  0,   1),   S( -2,  -9),   S(  1,   2),   S(  0,   1),

            /* knights: bucket 14 */
            S(  1,   1),   S(  0,   0),   S(  3,   7),   S( -4,  -9),   S( -7, -47),   S(-11, -24),   S( -2,  -4),   S( -1,  -6),
            S( -1,  -5),   S(-13, -55),   S( -2, -31),   S( -6, -29),   S(  2, -23),   S( -2, -13),   S( -4, -16),   S(  3,  26),
            S( -2, -12),   S( -7, -68),   S(  2, -12),   S(-15, -80),   S( -2, -44),   S(  1,  -7),   S( -3, -20),   S(  0,   7),
            S( -2,  -6),   S(  2, -27),   S(-10, -15),   S( -9, -38),   S( 12,  26),   S( -8, -37),   S(  2,   2),   S( -4,   7),
            S(  5,  14),   S(-16, -54),   S( -9, -49),   S( -2,  -9),   S( -5,  -7),   S(-11, -37),   S( -4, -22),   S( -3, -13),
            S( -1,  -2),   S(  2,  11),   S( 12,  69),   S(  5,  17),   S( -2,  -2),   S(  1,  13),   S( -4,  -2),   S( -7, -29),
            S( -2,  -5),   S( -2,  -8),   S(  6,   9),   S( 15,  64),   S(  2,  12),   S( -7, -33),   S(  1,   8),   S(  3,   1),
            S(  2,   2),   S(  0,   0),   S(  0,  -1),   S(  5,  23),   S(  0,  -4),   S(  0,   1),   S(  2,   3),   S(  0,   3),

            /* knights: bucket 15 */
            S( -5, -22),   S(  2,  16),   S(  6,  30),   S( -5,  -6),   S(  0, -10),   S( -8, -46),   S( -7, -57),   S( -3, -17),
            S( -3,  -8),   S(  3,  -1),   S(-12, -40),   S( 11,  19),   S(  5,  -7),   S(  2, -16),   S( -3, -19),   S(  3,   2),
            S( -2, -14),   S( -5, -25),   S(  2,  -9),   S(  4, -14),   S(-29, -92),   S(  0, -32),   S( -2, -14),   S(  3,   1),
            S( -3, -18),   S( -4, -15),   S( -7, -27),   S(  0,   6),   S(  6, -35),   S(  1, -15),   S( -2, -17),   S( -2,   2),
            S(  2,   4),   S( 15,  31),   S(-12, -49),   S( -3,  18),   S( 18,  18),   S(  2,  14),   S( -5, -33),   S(  2,  18),
            S( -1,   3),   S( -6,  -9),   S( -7, -11),   S(-10, -28),   S( -9, -32),   S(  7,  35),   S(  4,  20),   S(  5,  18),
            S( -4, -11),   S( -8, -21),   S(  6,  19),   S(  0,  12),   S(  5,  19),   S(  8,  22),   S(  1,   3),   S(  4,   4),
            S(  3,   9),   S( -2, -19),   S(  1,   3),   S( -2,  -6),   S(  2,   7),   S(  3,   8),   S(  2,  10),   S(  1,   4),

            /* bishops: bucket 0 */
            S( 33, -30),   S( -9,  26),   S(-12,   5),   S(  2, -47),   S(  9, -16),   S(  9,  -2),   S( 88, -98),   S( 29,   6),
            S(  0, -30),   S(  5,  -1),   S( -6,   3),   S( 10,   1),   S( 12,  -3),   S( 56, -14),   S( 40,  19),   S( 61, -14),
            S( 17,  -2),   S(  7,  19),   S( 25,   9),   S( 19, -12),   S( 41, -10),   S( 52,  23),   S( 51, -15),   S( 20, -26),
            S( -4, -40),   S( 58, -51),   S( 23,  -5),   S( 79, -30),   S( 73,  15),   S( 61,  22),   S( 11,  -2),   S( 10, -15),
            S( 32, -15),   S( 38, -24),   S(101, -25),   S(130, -28),   S(141, -34),   S(  7,  23),   S( 54,   2),   S( -2,   2),
            S( 47,   1),   S( 96,  -9),   S( 98,   6),   S( 38,   4),   S( 17,  15),   S( 77,   3),   S( 57,  -6),   S(-24,   3),
            S(-46, -94),   S( 75,  34),   S( 93,  54),   S( 35,  -5),   S( 15, -35),   S( 33,  12),   S( 30,   6),   S(-23,  47),
            S(-17, -44),   S(-10,   2),   S(-12, -38),   S(-18,  -5),   S(-13, -40),   S(-18, -10),   S(-31,  29),   S(  8, -12),

            /* bishops: bucket 1 */
            S(-25, -13),   S(  9, -29),   S(-16,  15),   S( 23, -24),   S(-19,   5),   S(  9,  -2),   S( 55, -30),   S( 46, -32),
            S( 14, -46),   S(  1, -17),   S(  5, -14),   S(-12,   2),   S( 38, -22),   S(  7,  -7),   S( 58, -14),   S( 23, -31),
            S(-30,  -7),   S( 17,  -5),   S(  5, -13),   S( 30, -12),   S( 11, -14),   S( 38, -14),   S( 23, -25),   S( 80, -34),
            S( 18, -40),   S( 33, -22),   S( 29,  -8),   S( 15,   6),   S( 66, -15),   S( 12,  -5),   S( 70, -45),   S( 20, -22),
            S( 34, -56),   S( 67, -12),   S( 21, -12),   S( 96, -12),   S( 57, -10),   S(108, -46),   S( 18,  -2),   S( 20, -14),
            S( 40, -40),   S( 40, -17),   S( 79, -12),   S( 70, -39),   S(140, -39),   S(-18,   3),   S( -1,  13),   S(-18, -27),
            S( -2, -67),   S( 29, -24),   S( -4, -28),   S( 23,  -2),   S( 15,  -2),   S(  8,   6),   S( 24,  -9),   S( -2,  15),
            S(-14, -34),   S( -6,  -8),   S(  6, -70),   S(-53, -15),   S( 17, -11),   S( 22, -31),   S( 35, -25),   S(-68, -26),

            /* bishops: bucket 2 */
            S(  8, -27),   S(-14, -12),   S(  1,   6),   S(-15,   7),   S( 16,   2),   S(-17,   4),   S( 15, -11),   S(  3,  -9),
            S( 29, -25),   S( 21, -20),   S(  1, -11),   S( 16,  -5),   S( -4,  11),   S( 17,  -5),   S(  4,  -8),   S(  8, -51),
            S( 34,  -3),   S( 12,   0),   S(  1,  17),   S( -4,  -1),   S( 11,  13),   S( -4,  -3),   S(  3, -29),   S(-13,  -5),
            S( 13, -16),   S( 48, -11),   S( -1,  11),   S( 42,  12),   S( 10,   6),   S(-11,  26),   S(-30,  15),   S( 10,   3),
            S( 24,  -4),   S(  2,  20),   S( 73,  -6),   S( 26,  12),   S( 19,  30),   S( -8,  22),   S(  6,  23),   S( 25,  -9),
            S(-25,  16),   S( 20,  24),   S(-11,  -2),   S( 95, -25),   S( 81, -16),   S( 73,  29),   S( 62,  -2),   S( 15, -34),
            S( 16,  16),   S(  2,  -4),   S( -5,  17),   S(  6,   1),   S(-75, -23),   S(-23, -28),   S(-40,  32),   S(-32, -23),
            S(-79,  -7),   S(-56,  15),   S( -1, -13),   S(  3,   9),   S(-23, -35),   S(-28,  -2),   S(-22, -25),   S(-37, -27),

            /* bishops: bucket 3 */
            S( 42,   1),   S( 50, -32),   S(  7,  -5),   S(  9,  11),   S( 15,  32),   S( -3,  45),   S(-13,  65),   S( -9,  -1),
            S( 45,   1),   S( 31,   3),   S( 22,   7),   S( 17,  15),   S( 26,  17),   S( 22,  27),   S( 13,  21),   S( 31, -21),
            S(  2,   8),   S( 28,  49),   S( 20,  46),   S( 22,  34),   S( 20,  43),   S( 20,  32),   S( 18,  14),   S(  5,  34),
            S( -7,  12),   S( 20,  32),   S( 26,  55),   S( 46,  48),   S( 47,  28),   S( 17,  37),   S( 31,   1),   S( 40, -31),
            S( 24,  29),   S( 18,  43),   S( 17,  50),   S( 53,  67),   S( 42,  66),   S( 36,  39),   S( 10,  38),   S( -8,  29),
            S( 13,  34),   S( 43,  47),   S( 24,  35),   S( 20,  51),   S( 70,  39),   S( 40,  77),   S( 54,  72),   S( 15,  99),
            S( -3,  56),   S( 36,  38),   S( 24,  35),   S( 17,  38),   S( 12,  50),   S( 49,  76),   S(-19,  11),   S( 24, -30),
            S(-34,  35),   S(-20,  57),   S(-43,  32),   S(-35,  71),   S(-12,  28),   S(-87,  71),   S(  5,  -1),   S(  3,  14),

            /* bishops: bucket 4 */
            S(-36,  23),   S(-25, -20),   S(-26, -15),   S(-44, -16),   S(-22, -36),   S(  2, -26),   S(-25, -22),   S(-36, -92),
            S(-41,   9),   S(-23, -10),   S( 73, -35),   S(-20,  -2),   S(-51,  11),   S( 13, -29),   S(-29, -11),   S(-44, -65),
            S(-12,  -1),   S(-26, -11),   S( 37, -23),   S(  9, -34),   S( 12, -17),   S(-35,  17),   S(-22, -20),   S(-58, -15),
            S( 23, -42),   S( 41,  -8),   S( 36, -20),   S( 37, -14),   S(  7,   7),   S( 30,  -6),   S(-45,  -6),   S(-14, -12),
            S(  1, -18),   S( 13, -73),   S( 41, -34),   S( 27, -19),   S( 34,   0),   S( 53, -18),   S( 31,  22),   S(-37, -15),
            S(-53, -77),   S(-25, -58),   S(  2, -26),   S( 68, -21),   S(-19,  -8),   S( 34,   5),   S( -9,   6),   S(  9,  33),
            S( 17,  21),   S( -4, -23),   S(  1, -68),   S(-24, -47),   S(-17, -10),   S(  8,   2),   S(-23,  14),   S(  7,  27),
            S(  0, -29),   S(  4, -40),   S(-11,  -2),   S(-18, -55),   S( -7, -38),   S( -3, -16),   S(-12,  37),   S(  2,  12),

            /* bishops: bucket 5 */
            S(-48,  15),   S(-10, -14),   S(-34,   9),   S(-59,  14),   S(  6,  -9),   S(-29, -20),   S(-53,   4),   S(-36, -13),
            S(-19, -13),   S(-64,  18),   S( 27, -15),   S( 14,  -6),   S(-48,  13),   S( -3,  -2),   S(-59,  11),   S( -2, -16),
            S( 26, -19),   S(-37,  -5),   S( 34, -19),   S(-21,   8),   S( -7,   5),   S(-66,  11),   S( 14,  -1),   S(-13,  23),
            S(  9, -12),   S(-31,  -1),   S( 42, -17),   S( 42,   0),   S(-31,  16),   S( 58,  -8),   S(-89,  29),   S(  5,  -6),
            S(  8, -15),   S(  1, -10),   S( -1, -22),   S( 15, -40),   S( 41, -25),   S(  9,  -6),   S( 39,  -2),   S(-47,  10),
            S(  2, -17),   S(  3,  -7),   S( 42, -45),   S(-48, -28),   S(-16, -14),   S( 11,  -9),   S(-23,   5),   S(  2,  19),
            S(-36, -21),   S(  5, -26),   S(-25, -14),   S(  2,  14),   S(  0, -17),   S( 22,  -7),   S( 17,  24),   S(-29,  23),
            S(-15, -26),   S(-23, -28),   S(-14, -26),   S( -1, -24),   S(-41,  12),   S(-12,   2),   S(-24,  -5),   S(  2,  32),

            /* bishops: bucket 6 */
            S(-57, -27),   S(-25,  -8),   S(-45,   4),   S(-39,  15),   S(-10,   4),   S( -5,  -3),   S(-48,  15),   S(-60,   0),
            S(-17,  -9),   S(-17, -22),   S(-33,  14),   S(-21,   4),   S(-53,  16),   S(-20,   4),   S(-39,  12),   S( -6, -15),
            S(-38,   4),   S(-19,   3),   S(-18,  -3),   S(-40,  17),   S(-24,  20),   S( -3,   1),   S(-53,  10),   S(-22,   4),
            S( 10, -12),   S(-34,  -8),   S(-43,  14),   S( 80,  -5),   S( 38,   9),   S( 21,   5),   S( 24, -21),   S( -6, -11),
            S(-63,   8),   S(-37,  10),   S( 48, -14),   S( 95, -16),   S( 19, -36),   S(-15, -13),   S( 39, -22),   S( -4,  -7),
            S(-46,  21),   S(-21,   8),   S(-20,   1),   S( 11,   3),   S(  9, -10),   S(-10, -21),   S( 22,   4),   S(-44,  -5),
            S(-39,  16),   S(-47,  31),   S( 11, -12),   S( -8, -21),   S(-20,   3),   S(  7,  -1),   S( -8, -17),   S(-31,   0),
            S(-27,  19),   S(-24,  -9),   S(-25,   5),   S(-24,   5),   S(-17,   5),   S( 37, -28),   S(-13,   4),   S( -9, -14),

            /* bishops: bucket 7 */
            S( 28, -59),   S(-56, -10),   S(-37, -38),   S(-22,   0),   S(-33,  -3),   S(-56,  -4),   S(-83, -10),   S(-29, -28),
            S(  3, -58),   S( 32, -52),   S( 15, -27),   S( -6, -29),   S(-26,  -2),   S(-39,   4),   S(-37, -20),   S(-18,   3),
            S(-36, -34),   S(-38,   7),   S( -5,   0),   S( 37, -25),   S(  4, -11),   S( 20, -22),   S(-52,  -4),   S(-61,  17),
            S(-18, -14),   S(-19,   5),   S( -4,  -1),   S(  6, -16),   S( 82,  -9),   S( 26, -13),   S( 30, -28),   S(-68,  -6),
            S(-14, -17),   S(  3,   8),   S(-12,  -6),   S( 26,  -1),   S( 81, -27),   S(104, -39),   S(-20, -20),   S(-43, -19),
            S(-70,  18),   S( -8,  24),   S( -7,  -4),   S(-41,   5),   S(-27,  14),   S( 69, -43),   S( 36, -18),   S(-56, -111),
            S(-27, -11),   S(-10,  29),   S(-32,  17),   S( 53,  -6),   S( 28, -36),   S(-13, -23),   S( 18, -11),   S(-23, -53),
            S(-23, -18),   S(-41,  -5),   S(-55,  -1),   S(-13, -25),   S( -9, -15),   S(  2, -30),   S( 20, -26),   S(-13, -12),

            /* bishops: bucket 8 */
            S( 16,  70),   S(-11,  -4),   S(  9, -27),   S( -6,  30),   S(  1,  11),   S( -6, -40),   S(-21, -56),   S(-11, -37),
            S(  1,   8),   S( 23,  52),   S( -2,  40),   S(  1,   3),   S(  2, -17),   S(  7,  -6),   S(-33, -74),   S( -8, -19),
            S( -8, -37),   S(-10, -51),   S( 16,  41),   S( 21,   5),   S( 32,  21),   S( 11,   9),   S(-12, -33),   S(-42, -79),
            S(  5,  16),   S( 25,  72),   S(  1,   8),   S( -8,  49),   S( 33,  34),   S( 29,  43),   S( -5,   1),   S( -5, -14),
            S(  8,  74),   S( 12,  94),   S( 10,  39),   S(  9,  15),   S( -5,  66),   S(-27,  -3),   S( 12, -21),   S( 12,  14),
            S(  1,   6),   S( -3,  24),   S(  7,  53),   S(  9,  29),   S(  3,  11),   S(  8,  32),   S( -4,  24),   S( -3,   6),
            S(  0,  14),   S(-22, -84),   S( 11,  44),   S(  8,  54),   S( -6,  20),   S( 15,  53),   S(  6,  73),   S( -7,  26),
            S( -5,  -4),   S(  3,   8),   S(  7,   9),   S(  4,  28),   S(  9,  44),   S( 11,  37),   S( -5,  43),   S( 16,  86),

            /* bishops: bucket 9 */
            S(  6,  64),   S(-30,  -3),   S( -8,  -4),   S(-35, -18),   S(-22, -20),   S(-10,  -1),   S( -4, -19),   S(-13, -28),
            S(-10,  -9),   S(-16,  15),   S(-17,  11),   S(-27, -17),   S( -9, -26),   S( 11, -22),   S(-21, -33),   S(-22, -58),
            S(  3, -18),   S(  2,   7),   S(  0,  -6),   S( -6,   9),   S( -5,   7),   S( -7, -27),   S(  8,  -3),   S(-35, -46),
            S(  3,  23),   S(  3,  28),   S(-23,   1),   S(  4,  43),   S( -8,  35),   S(  2,   8),   S( -2,  -5),   S( 11,  24),
            S(  8,  36),   S(-12,  23),   S( 20,  25),   S(-23,  11),   S(  6,  25),   S(-16,  22),   S( 10,  19),   S( -5, -51),
            S( -3,  37),   S( -8,  17),   S(-13,  22),   S( 11,  33),   S(-16,  24),   S(-14,  27),   S(  6,  35),   S( -7,  36),
            S( -3,  35),   S(-14,  21),   S(-14,   7),   S(-15,  21),   S(  0,  -2),   S( -5,  22),   S( 11,  41),   S(  0,  65),
            S(  8,  42),   S( -5, -14),   S(  0,  11),   S( -2,  16),   S( -1,  36),   S(-14,  34),   S( -3,  63),   S( 19,  84),

            /* bishops: bucket 10 */
            S( -9,   0),   S(  1,  23),   S(-16, -34),   S(-13, -40),   S(-51, -32),   S(-44, -41),   S(-27,  12),   S( -7,   8),
            S(-15, -41),   S(  6, -31),   S(-12,  -4),   S(-45, -41),   S(-37,  -8),   S(-47, -49),   S(-20, -36),   S( -6,  29),
            S(-12, -52),   S(-17, -37),   S(-32,  -9),   S( -3,  23),   S( -5, -24),   S( -9, -16),   S(-11, -17),   S( -7, -51),
            S(-16, -32),   S(-37, -28),   S(-34, -26),   S( -3,  12),   S(  4,  40),   S( 18,  20),   S(  9,  39),   S(-28, -20),
            S(-15,  -1),   S(-51,  20),   S(-12,  -5),   S(  6,  -6),   S(  2,   4),   S( -2,  10),   S(-32,  10),   S( -2,   5),
            S(-13,  13),   S(-19,  -2),   S(-27,  17),   S(-13,   4),   S(-22, -12),   S(-11,  19),   S(-22,  15),   S( -7,  14),
            S( -4,  12),   S( -8,  12),   S(-20,   2),   S(-18,  31),   S(-20,   7),   S(-24, -38),   S(-13,   1),   S(  2,  36),
            S( -7,  43),   S(  1,  44),   S( 11,  62),   S(-10,   8),   S(-15,  17),   S( -9,  10),   S(  2,   5),   S( -7,  -8),

            /* bishops: bucket 11 */
            S( 10,  -4),   S(-20, -45),   S( -9, -11),   S(-12,  -4),   S(-22, -29),   S( 12, -33),   S(-25, -38),   S(-24,  38),
            S(  1,  18),   S(  6,  -9),   S(-28, -19),   S( 14,  -9),   S(-33,  -6),   S(-46, -18),   S(-42, -19),   S(  2,  16),
            S(  0, -32),   S( -5, -28),   S( 18, -11),   S( -7, -19),   S(-32, -14),   S(  5,  -5),   S(  9, -15),   S(  1, -18),
            S(  2,  16),   S( -8, -28),   S( 21,  11),   S(-29, -23),   S( 12,   9),   S(  9,  42),   S(  5,  30),   S( -8, -24),
            S( -9,  32),   S(-13, -10),   S(-25,  22),   S(-33,  32),   S( -2,  -4),   S( 15,  15),   S( -4,  39),   S( -1,  34),
            S(-14,  -2),   S(-10,  -3),   S(-19,  50),   S(  0,  24),   S(-23,  51),   S( -2,  33),   S(  3,   5),   S( -4,  -5),
            S(-11,  10),   S(  2,  89),   S( -3,  39),   S( 14,  32),   S( -4,   6),   S(-25, -18),   S(-18, -49),   S(-14,   1),
            S( -2,  75),   S(-13,  35),   S(  8,  69),   S(  2,  40),   S(  9,  44),   S( -2,  17),   S(-12,  24),   S(  3,   5),

            /* bishops: bucket 12 */
            S( -4, -12),   S(  1, -14),   S(  1,   7),   S(  8,  21),   S( -2,   5),   S( -2,   8),   S(  0,   6),   S( -4,  -5),
            S( -5, -19),   S(  3,  15),   S(  5,   5),   S( -8,  -9),   S( -4, -12),   S(  5,  -6),   S(-14, -26),   S( -2,  -8),
            S(  5,  41),   S(  3,  27),   S( 10,  29),   S( 12,   2),   S(  4,  24),   S( -1, -34),   S(  2,   4),   S( -5,  -2),
            S( -3,  46),   S(  4,  48),   S(  9,  48),   S( 17,  37),   S(  2, -16),   S( -3, -27),   S(  7,  13),   S( 10,  30),
            S( 13,  16),   S( 15,  50),   S(  1,  32),   S( 16,  42),   S( 16,  43),   S( 18,  58),   S(  5,  -9),   S(  1, -10),
            S(  3,  11),   S( -8, -26),   S(  4,  27),   S( -2,   3),   S( 18,  62),   S( 11,  39),   S( -4, -12),   S( -5, -33),
            S( -1,  -1),   S(  3,  24),   S(  2,   3),   S(  5,   5),   S( 11,  54),   S(  8,  62),   S( 13,  46),   S(  4,  22),
            S(  1,  15),   S( -1,   5),   S(  3,  10),   S( -3,  -9),   S(  2,   4),   S(  8,  32),   S(  4,  55),   S(  4,  30),

            /* bishops: bucket 13 */
            S(-10, -21),   S( -2,  -5),   S( -8, -30),   S(-11, -14),   S( 11,  24),   S(-11, -28),   S(-13, -29),   S( -5, -32),
            S( -2,  24),   S( -6,  -7),   S( -3,   0),   S( 11,  52),   S(-15, -21),   S( 12,  18),   S(  2, -28),   S( -3, -19),
            S(  0,  27),   S( 22,  81),   S( 10,  53),   S( 19,  30),   S( -6, -15),   S( 18,  50),   S( -1,   3),   S(-11, -19),
            S( 14,  66),   S( 20, 107),   S( 15,  45),   S( -9,   9),   S( 19,  56),   S( -4,  -6),   S( 12,  47),   S(  0,   1),
            S(  8,  53),   S(  2,   6),   S(  9,  16),   S(  6,  34),   S( -4,   6),   S(  3,  30),   S(  5,   7),   S( -1,  26),
            S( -4,  20),   S(  1,  23),   S( 10,  43),   S( 13,  30),   S( -9,  24),   S( -3,   9),   S( -4,   9),   S(  1,  19),
            S( 10,  40),   S( -6, -12),   S( -2,  -5),   S( -4,   9),   S(  1,  11),   S(  3,  47),   S(  6,  25),   S(  2,  39),
            S( -2, -17),   S( -2,  -4),   S(  0,  -6),   S(  5,  17),   S(  0,  28),   S( -1,   4),   S(  4,  40),   S(  6,  32),

            /* bishops: bucket 14 */
            S(-12, -33),   S(  1,  10),   S( 17,  12),   S(  0,  26),   S(-13, -19),   S( -4,  -2),   S(  2,  11),   S( -5, -11),
            S( -6, -17),   S(  4,   7),   S( -2,  17),   S(  5,  28),   S(  9,  49),   S(  0,  20),   S(  9,  19),   S(  2,  27),
            S( -2,  -2),   S( -8, -20),   S( -9, -13),   S( 10,  24),   S( 13,  42),   S( 14,  52),   S( -4,  45),   S(  1,   8),
            S(  4,  35),   S( -5,  -8),   S(-22, -18),   S( -2,  58),   S(  4,  29),   S(  0,  25),   S( -2,  51),   S( -2,  15),
            S(  3,  27),   S(  0,   7),   S(-11,  10),   S( -5,  43),   S(  2,  -5),   S(  2,  46),   S(  7,  23),   S(  0,  34),
            S( -7,  -6),   S( 18,  42),   S(  0,  41),   S(  5,  29),   S(  3,  61),   S(  1,  23),   S(  0,  11),   S( 18,  51),
            S( 14,  69),   S(  3,  29),   S(  6,  33),   S(  3,   0),   S( -5,  -3),   S( 13,  48),   S( -7, -39),   S(  2,   8),
            S(  4,  40),   S(  4,  45),   S(  0,  12),   S(  0,  -5),   S( -6, -25),   S( -4, -17),   S(  9,  13),   S(  2,  18),

            /* bishops: bucket 15 */
            S( -1,   1),   S( -7, -15),   S(-14, -30),   S( -4, -16),   S(-16, -29),   S(  4,  -6),   S( -3, -23),   S( -2,  -8),
            S( 12,  41),   S(  0,  -6),   S( -2,   1),   S(  2,   2),   S( 24,  19),   S( -6, -26),   S( -1,   2),   S( -5, -19),
            S( -3, -11),   S( -1,  -9),   S( -1,  15),   S( 33,  71),   S(  9,  34),   S(  9,  16),   S( 12,  61),   S(  3,  41),
            S(  1,  -5),   S(  9,  26),   S( 11,  57),   S(  1,  29),   S(  3,  18),   S( 16,  29),   S( 14,  47),   S(  3,  29),
            S( -3,   5),   S(  8,  14),   S(  6,  59),   S( 32,  92),   S(  1,  57),   S( 16,  49),   S(  2,  22),   S( -9, -11),
            S( -6, -27),   S( -2,   6),   S( 16,  77),   S(  7,  12),   S( 16,  37),   S(  6,  37),   S(  0,   1),   S(  0,  -1),
            S( -2,  -4),   S( -6, -11),   S(-11,  18),   S( 19,  52),   S( 10,  53),   S(-10, -35),   S(  3,  28),   S( -3,  -9),
            S(  3,  15),   S(  5,  43),   S(  6,  34),   S(  3,   8),   S(  0,   4),   S(  4,  12),   S(  4,  17),   S(  3,  14),

            /* rooks: bucket 0 */
            S( -9,  16),   S( 19,   3),   S(  5,  16),   S(  4,  25),   S(-18,  65),   S(-10,  57),   S(-28,  62),   S(-57,  61),
            S( -8,  14),   S( -1,  32),   S(-32,  45),   S(  0,  33),   S(  2,  45),   S(-16,  46),   S(-13,  10),   S(-36,  73),
            S( 10, -10),   S( -7,  11),   S(-21,  49),   S( -2,  22),   S(-37,  67),   S(-21,  32),   S(-14,  54),   S(-18,  48),
            S(-30,  27),   S( 31,  18),   S(-49,  55),   S( 29,  20),   S(  6,  63),   S(-15,  55),   S(-25,  75),   S(-36,  46),
            S( 59, -40),   S( 14,  35),   S( -1,  45),   S( 20,  54),   S( 25,  47),   S( 36,  60),   S( 47,  56),   S( 31,  61),
            S( 40,  -4),   S( 48,  55),   S( 63,  44),   S(137,  23),   S( 19,  76),   S( 29,  68),   S(-10,  87),   S(-21,  84),
            S( 25,  53),   S( 62,  69),   S( 87,  55),   S( 78,  40),   S( 90,  58),   S( 40,  76),   S( 18,  77),   S(-33,  91),
            S( 12,   1),   S( 47,  45),   S( 34,  54),   S( 27,   2),   S( 95,  59),   S( 73,  40),   S( 39,  38),   S( 62, -25),

            /* rooks: bucket 1 */
            S(-60,  56),   S(-18,  15),   S(-11,  37),   S(-30,  46),   S(-34,  55),   S(-34,  52),   S(-44,  79),   S(-72,  67),
            S(-59,  57),   S(-28,  12),   S(-22,  32),   S(-33,  48),   S(-49,  39),   S(-62,  45),   S(-20,  24),   S(-49,  56),
            S(-47,  49),   S(-10,   2),   S(-10,  19),   S(-11,  19),   S(-48,  34),   S(-64,  40),   S(-60,  76),   S(-16,  39),
            S(-62,  66),   S(-41,  28),   S(-35,  54),   S(-45,  37),   S(-60,  66),   S(-83,  93),   S( -7,  43),   S(-68,  83),
            S(-43,  62),   S( -1,   5),   S( 37,  18),   S( 28,  28),   S( -9,  40),   S(  0,  68),   S(-27,  63),   S(-17,  92),
            S( 34,  44),   S( 62,  14),   S( 29,  35),   S( 43,  58),   S(  3,  40),   S( 11,  65),   S(  5,  64),   S( 16,  79),
            S( 51,  55),   S( 53,  31),   S( 33,  50),   S( 38,  65),   S( 35,  48),   S( 56,  53),   S( 25,  83),   S( 46, 100),
            S( 50,  10),   S( 54,  -6),   S(-12,  -6),   S( 13,  -4),   S( 78,  -1),   S( 28,  41),   S( 33,  55),   S( 74,  44),

            /* rooks: bucket 2 */
            S(-67,  85),   S(-52,  75),   S(-42,  77),   S(-36,  51),   S(-33,  61),   S(-43,  52),   S(-33,  34),   S(-78,  71),
            S(-62,  89),   S(-66,  84),   S(-49,  77),   S(-61,  71),   S(-58,  67),   S(-55,  43),   S(-42,  48),   S(-49,  33),
            S(-67,  83),   S(-41,  82),   S(-63,  75),   S(-42,  61),   S(-47,  61),   S(-43,  52),   S(-24,  39),   S(-14,  46),
            S(-30,  91),   S(-38,  79),   S(-74,  94),   S(-83,  81),   S(-53,  80),   S(-72,  78),   S(-35,  57),   S(-16,  44),
            S(-34,  98),   S(-69, 115),   S(-32, 105),   S(-40,  76),   S(-39,  82),   S( -1,  65),   S(-15,  70),   S(-23,  87),
            S(  2, 102),   S( 26,  97),   S( 28,  86),   S( -2,  70),   S( 74,  33),   S( 25,  58),   S(107,  20),   S( 74,  69),
            S( 47, 103),   S( -3, 117),   S( 56,  76),   S( 71,  45),   S( 46,  38),   S( 46,  89),   S(-40, 119),   S( 16, 104),
            S( 24,  70),   S( 45,  65),   S( 55,  46),   S( -4,  40),   S(-32,  43),   S( 26,  40),   S(-11,  58),   S( 65,  42),

            /* rooks: bucket 3 */
            S(-14, 104),   S( -7, 102),   S( -8, 139),   S( -2, 130),   S(  2,  93),   S( 11,  80),   S( 22,  63),   S(-12,  43),
            S(  1,  98),   S(-10, 109),   S( -9, 133),   S(-10, 134),   S(  0,  85),   S( 12,  57),   S( 49,  30),   S( 13,  68),
            S(  3, 105),   S(-14, 118),   S(-22, 131),   S( -6, 129),   S( -2,  94),   S(  0,  78),   S( 32,  72),   S( 18,  71),
            S(  3, 126),   S( -9, 140),   S(-16, 142),   S(-14, 140),   S( -7, 111),   S( -7, 100),   S( 22,  87),   S(  0,  71),
            S(  6, 136),   S(-18, 155),   S(  9, 154),   S( 13, 142),   S(  9, 115),   S( 13, 108),   S( 50,  94),   S( 24,  95),
            S( -8, 160),   S( 23, 143),   S( 39, 146),   S( 47, 143),   S( 96,  96),   S(106,  96),   S( 82, 112),   S( 38,  99),
            S( 12, 162),   S(  0, 167),   S( 18, 163),   S( 15, 162),   S( 31, 145),   S(110,  95),   S(145, 156),   S(171,  99),
            S(112,  33),   S( 54, 110),   S( 65, 129),   S( 18, 137),   S( 29, 116),   S( 66,  97),   S( 66,  77),   S(105,  42),

            /* rooks: bucket 4 */
            S( -8, -33),   S(  8, -12),   S( 10, -20),   S(-28,  -4),   S(-48,  20),   S(-35,  27),   S(-41,  -2),   S(-77,  28),
            S(-29, -34),   S(-52, -21),   S( 28, -56),   S( 14, -49),   S(-22,  19),   S(-13,  15),   S(-15,  19),   S( -3,  16),
            S(  2, -23),   S(-28, -22),   S(-28,  -9),   S(-32, -31),   S(-65,   5),   S(-85,  45),   S(-27, -12),   S(-53,   5),
            S(-65, -18),   S( 54,  -3),   S( 10, -29),   S( 35, -46),   S( 49,  -9),   S(-21,  24),   S(-35,  -3),   S(-23,  11),
            S(-33,   2),   S( 47, -27),   S( 44,  12),   S( 57, -18),   S( 84, -12),   S( 52,  26),   S(-10,  18),   S(  5,  24),
            S( -5, -22),   S(  2, -15),   S(  4, -39),   S( 34,  17),   S( 22,  24),   S( 16,  13),   S( 24,  25),   S( 43,  25),
            S( -8, -27),   S( 13,  -5),   S( 49, -12),   S( 49, -11),   S( 63,  -3),   S(-15,   4),   S( 21, -32),   S( 23, -15),
            S( 31,  -9),   S( 25,  10),   S( 56, -36),   S( 30,   2),   S( 38,  11),   S( 30,  -1),   S( 25,  -2),   S(  3,  29),

            /* rooks: bucket 5 */
            S(-56,  39),   S(-42,  13),   S( -5,   3),   S( 36,   4),   S(  6,   0),   S(-29,  39),   S(-14,  40),   S(-63,  34),
            S( -7,  -7),   S(-40, -14),   S(  6, -28),   S( 17, -12),   S(-15,   3),   S(-40,  10),   S(-87,  51),   S( 23,   4),
            S(-68,  24),   S(-44,   4),   S(  4, -29),   S(-65,  20),   S(-45,   5),   S(  8, -30),   S(-40,  11),   S(-65,  23),
            S(-56,  25),   S(-15,   2),   S( 46, -36),   S( 38,  -2),   S( 19,  -6),   S(-30,  27),   S(-25,  26),   S(-28,  34),
            S( 29,   8),   S( 12,   7),   S(  5,  19),   S( 37, -23),   S(-21,  12),   S( 86,  -6),   S( -7,  18),   S( 36,  30),
            S( 60,   4),   S(  9,  14),   S(  6,  -4),   S( -9, -13),   S( 30,   7),   S( 13,  32),   S(105,  -6),   S( 47,  37),
            S( 65, -24),   S( 29, -17),   S( -5,  -8),   S( 32,  10),   S( 67, -15),   S( 52, -27),   S( 43,  -4),   S( 35,   3),
            S( 23,  17),   S(-10,  16),   S( 54, -10),   S( 13,  23),   S( 56,   6),   S( 42,  16),   S( 21,  19),   S( 30,  41),

            /* rooks: bucket 6 */
            S(-76,  57),   S(-19,  19),   S(-20,  20),   S(-25,  19),   S( -1,   9),   S( 25, -11),   S(-22,  12),   S(-43,  11),
            S(-96,  52),   S( -7,   4),   S(-46,  32),   S(  5,   2),   S( 13,  -7),   S(-13,  -8),   S(-23, -12),   S(-20,  14),
            S(-90,  47),   S(-34,  34),   S(-20,  17),   S(  0,  -8),   S(-14,  12),   S(  7,  -5),   S(-21, -19),   S(  7, -19),
            S(-60,  36),   S(-32,  34),   S( -5,  16),   S( 41,  -4),   S( 18,  21),   S(-12,   9),   S(-20,   8),   S(-21,  31),
            S(-35,  51),   S( 48,  17),   S( 84,  15),   S( 73,  -3),   S( 33,   0),   S( 37,  15),   S( 44,   1),   S( 91,  -3),
            S(123,   1),   S( 86,   2),   S(107,   4),   S( 49,  -1),   S(  8, -17),   S( 28,  37),   S( 34,   1),   S( 96,   4),
            S( 20,  19),   S( 92,  -7),   S(125, -39),   S(107, -44),   S( 41, -10),   S( 38,  -2),   S( 78, -29),   S( 89, -30),
            S( 51,  -1),   S(-19,  43),   S(-10,  25),   S( 53,  -6),   S( 32,   7),   S( 32,  28),   S( 35,  22),   S( 34,  19),

            /* rooks: bucket 7 */
            S(-106,  46),  S(-94,  40),   S(-69,  42),   S(-65,  43),   S(-39,  16),   S(-25,  -6),   S(-43,  21),   S(-67,  -5),
            S(-96,  42),   S(-20,  11),   S(-66,  35),   S(-84,  48),   S(-64,   7),   S(  2,  -7),   S( -4,   7),   S( 23, -52),
            S(-79,  31),   S(-99,  48),   S(-31,  11),   S(-63,  45),   S(-57,  22),   S(-67,  27),   S( 46, -23),   S(-24, -21),
            S(-76,  35),   S( -5,   1),   S( 12,   0),   S( 41, -13),   S(  4,  -6),   S( 59, -19),   S( 53,   2),   S( 12, -13),
            S(-14,  32),   S( -1,  28),   S( 55,  16),   S( 72,   0),   S(121, -29),   S(132, -43),   S( 93,  -2),   S(-68,  -6),
            S( 10,  34),   S( 21,  23),   S(112,   1),   S( 93,  -8),   S( 75,   7),   S( 39,  23),   S( 32,  35),   S(-22, -37),
            S( 13,   4),   S( 52,  -9),   S( 56,   0),   S( 93, -24),   S(126, -36),   S( 89, -19),   S( 63,  -2),   S( 37, -44),
            S(-33,  12),   S(-39,  29),   S( 17,  14),   S( 33,   8),   S( 22,   4),   S( 56,   3),   S( 69,  12),   S( 63, -12),

            /* rooks: bucket 8 */
            S( 18, -87),   S( 27, -53),   S( 29, -55),   S( 34, -24),   S(  1, -33),   S(-21, -41),   S(-17, -68),   S( -8, -24),
            S(-26, -90),   S( -3, -42),   S(  3, -37),   S(-31, -87),   S(-14, -47),   S( -9, -23),   S( -9, -29),   S(-38, -34),
            S( 16,   0),   S( -5, -24),   S( 13,  -5),   S(-10,   3),   S(  8,  10),   S( 26,  31),   S( 17,  47),   S(-23, -39),
            S(  0, -18),   S(  2,   5),   S(  9,   5),   S( 29,  -6),   S(  2,  19),   S( 17,  12),   S(  9,  -2),   S(  6, -20),
            S(  1, -23),   S( 24,  34),   S( 16, -11),   S( 23,  22),   S( 15,  25),   S( -4, -10),   S( 16,  36),   S( -5,  22),
            S(-10, -22),   S( 20, -14),   S(  1, -32),   S( -8, -25),   S(  4,   6),   S( -5,  10),   S( -3,  14),   S(-12,   3),
            S( -3,  22),   S( 31,  -9),   S( 16, -14),   S( 33,  16),   S(  7,   8),   S( 11,  19),   S( 19,  54),   S( 13,  38),
            S(-11,  -2),   S(  4, -27),   S( 19,  -7),   S( 31,  41),   S( -4,   2),   S(  7,  12),   S( 14,  19),   S(  2,   7),

            /* rooks: bucket 9 */
            S(  0, -92),   S( 16, -115),  S( 28, -93),   S( 12, -78),   S( 20, -90),   S( 12, -68),   S(  1, -20),   S( 27, -64),
            S(-40, -79),   S(-11, -89),   S(-16, -69),   S(  0, -61),   S(  4, -80),   S( -9, -31),   S(-21, -58),   S(-11, -48),
            S( -4, -34),   S(  6, -36),   S(  8,   5),   S( -9, -29),   S(  8, -65),   S(-15, -26),   S( -1,  -8),   S(  6, -17),
            S( 18, -16),   S( 14,  -7),   S(  5,   7),   S( -5, -22),   S( -6, -66),   S( 21, -22),   S( -7, -14),   S(  7, -28),
            S( 19, -52),   S( -3, -35),   S( 10, -45),   S( -4, -33),   S( -6, -52),   S(-10, -24),   S( -5, -41),   S(-17, -47),
            S( -2, -50),   S(-18, -57),   S(-10, -47),   S( 19, -11),   S( -1, -19),   S( -4, -26),   S(-21, -32),   S(-18, -33),
            S(  2, -12),   S( -6, -31),   S( 21, -43),   S( -8,  -4),   S( -5, -24),   S( 14, -23),   S(  6, -20),   S(-24, -64),
            S(-19,  -8),   S(  4, -10),   S( 14, -20),   S(  3,   0),   S( -3, -36),   S( -6,  10),   S(-28, -23),   S( 10, -18),

            /* rooks: bucket 10 */
            S(  8, -80),   S(-38, -65),   S(  0, -87),   S(  8, -82),   S( 14, -96),   S( 21, -113),  S( 17, -103),  S( -7, -67),
            S(-26, -71),   S(-33, -55),   S(-15, -60),   S(-38, -93),   S(-10, -80),   S(  4, -65),   S(-17, -80),   S(-35, -106),
            S(  7, -54),   S(-41, -43),   S(-35, -43),   S(-45, -74),   S(-15, -30),   S(  6, -29),   S(-21, -87),   S(-15, -29),
            S(-17, -46),   S(-28, -51),   S(  4, -61),   S(-10, -19),   S(  9,  -2),   S(  3,   3),   S( -9, -75),   S( -9, -61),
            S(-12, -47),   S(  0, -46),   S( -4, -67),   S(-16, -65),   S( 17, -26),   S(  1, -26),   S( 17, -72),   S(  1, -82),
            S(-18, -27),   S( 11, -20),   S(  1, -44),   S(-14, -60),   S( -9, -42),   S( 14, -42),   S(-10, -72),   S(-10, -59),
            S(-20, -33),   S( -3, -35),   S(  2, -55),   S(-12, -41),   S( 22, -32),   S( -3, -36),   S(-28, -72),   S(  3, -36),
            S(-23, -35),   S(  1,   1),   S(  2, -19),   S( -2, -29),   S( -1,   5),   S(-11,  -1),   S( 15, -68),   S(  0,   4),

            /* rooks: bucket 11 */
            S(-29, -66),   S( -7, -38),   S(-18, -66),   S(  0, -14),   S(-33, -35),   S( 14, -76),   S( -5, -55),   S(  0, -82),
            S( -3, -19),   S(-18, -34),   S(-41, -35),   S(-52, -41),   S(-34, -27),   S( 17, -12),   S(-36, -57),   S(-39, -75),
            S(-33,   1),   S(-29,  20),   S( 23,  11),   S(-27, -11),   S(  4, -15),   S(-13, -26),   S( 14,  -1),   S(-20, -19),
            S(-10, -34),   S(  5, -23),   S(  0,  -5),   S( 33, -10),   S( 12, -30),   S( -8, -37),   S(  7,   9),   S( -5, -25),
            S(  6, -37),   S( 16, -23),   S( 10,  -3),   S(  3,  -2),   S( 31, -20),   S( 22, -34),   S( 32,   7),   S(-25, -62),
            S(-20, -36),   S(-14, -35),   S( 10, -16),   S(  3, -23),   S(-13, -15),   S( 27,  -1),   S( 34,  -7),   S(  5, -44),
            S(-21, -23),   S(-29, -48),   S(-14, -15),   S(  3,  -7),   S(  9, -30),   S( 19, -21),   S( 18, -16),   S( -6, -25),
            S(-10, -22),   S( 27,   7),   S( -5,  35),   S( 13,  -6),   S( -1,  17),   S( 11, -13),   S( 52, -24),   S(-24,   9),

            /* rooks: bucket 12 */
            S(-29, -92),   S( -1, -18),   S(  0, -46),   S(-19, -85),   S(-17, -66),   S( -4, -36),   S(-28, -69),   S(-14, -50),
            S(  2, -17),   S(  1,  -3),   S( 12,  25),   S( 16,  30),   S( 12, -15),   S( 17, -11),   S(  2, -11),   S(-22, -62),
            S(  1,  -9),   S(  0,   6),   S( 18,  28),   S( 20,   6),   S( -1, -16),   S( 16,  -7),   S(  8,  27),   S(  0,  17),
            S(-10, -17),   S(-11, -38),   S(  6,  12),   S( 11,  15),   S( 15,  -2),   S(  6, -11),   S( 16,  32),   S(  2,   9),
            S( -1, -31),   S(  7,  -6),   S(  2, -37),   S( -2, -31),   S( 10,  14),   S( -8, -16),   S(  5,  -3),   S( -6, -29),
            S(  0, -18),   S(-12, -54),   S( -8, -45),   S( -8, -32),   S(  1,  -3),   S(-11, -44),   S( 10,  15),   S(  0,  11),
            S(-15, -14),   S( -4, -21),   S(  9, -18),   S( -2,  -9),   S( -6, -24),   S( 11,   5),   S(  0,  -2),   S( -1,   1),
            S(  4,   9),   S( -1,  -5),   S( -1, -26),   S( 12,  10),   S(  8,  20),   S(  0,  -1),   S( -2, -17),   S( -5,  13),

            /* rooks: bucket 13 */
            S(-28, -89),   S(-21, -65),   S(-28, -63),   S(-14, -43),   S(-33, -107),  S( -4, -35),   S(-24, -55),   S(-35, -56),
            S(-16, -48),   S(-13, -59),   S( -1, -10),   S(  3, -13),   S( 17,   8),   S(-12, -74),   S(  4, -48),   S( -7, -52),
            S( -5, -55),   S( -8, -63),   S(-10, -28),   S(  4, -26),   S(  8, -21),   S(  7, -39),   S( 21, -17),   S(  1, -49),
            S(  6, -14),   S(  4,  -7),   S( 18,  17),   S( 17,   7),   S( 14,  -4),   S(  5,  -8),   S(  0, -10),   S( -2,   4),
            S(  0, -17),   S( -9, -87),   S(-15, -74),   S(  9, -10),   S(  9, -35),   S(  6,   0),   S( -2,  -3),   S( 11,  28),
            S( -9, -38),   S(-10, -49),   S(-11, -58),   S(-15, -77),   S(-15, -95),   S( -5, -50),   S(  0, -18),   S( -1,  -1),
            S( -4, -42),   S(  7, -20),   S( -7, -46),   S( 15,  -9),   S(-13, -69),   S(  8,  -7),   S( -1, -26),   S( -3, -16),
            S(  9,  -5),   S(-13, -24),   S(  1,  -5),   S( 23,  16),   S(-22, -74),   S(  0, -18),   S(  6,  11),   S(  2,  -4),

            /* rooks: bucket 14 */
            S( -9, -70),   S(-27, -53),   S(-10, -51),   S( -8, -71),   S( -5, -69),   S(  7, -43),   S(-18, -118),  S(-28, -78),
            S( -2, -53),   S( 13,  15),   S(  9, -43),   S(  3, -28),   S(  1, -13),   S( -4, -12),   S( -1, -39),   S( -2, -55),
            S( -3, -11),   S(  6, -17),   S( -4, -68),   S(  5, -32),   S( 14,   0),   S(  9,   8),   S( 11, -45),   S(-17, -85),
            S( -1, -33),   S(  8,  24),   S(  9,  25),   S(  1, -28),   S(-12, -45),   S( -3, -24),   S( -4, -29),   S(-10, -54),
            S(  9,  29),   S( 16,  31),   S( -2, -16),   S( -3, -60),   S(  3, -16),   S( 15,   2),   S( -4, -56),   S(-19, -55),
            S( 12,  11),   S(  1,   1),   S( -2, -43),   S(  1, -78),   S(  3, -60),   S(  5, -57),   S(-13, -72),   S(-12, -55),
            S(-12, -39),   S(  5,  -4),   S(  1, -17),   S(-21, -94),   S(  9,   4),   S( 10, -20),   S(  2, -62),   S( -2, -32),
            S( -5, -37),   S( -2, -15),   S( -7, -33),   S(-14, -73),   S(  1, -50),   S( -3, -67),   S(  4, -35),   S(  1,  -9),

            /* rooks: bucket 15 */
            S(-13, -65),   S( -5, -51),   S(-39, -74),   S(-36, -72),   S( -7, -32),   S( -4, -27),   S(  2, -12),   S(-29, -76),
            S( 15,   4),   S( -1, -48),   S( -6, -32),   S(-14, -46),   S(-12, -51),   S(  1, -28),   S( 11,   6),   S( -2, -12),
            S( -3,  -1),   S(  0, -43),   S( 11,   9),   S( 16,   7),   S( 15,  -8),   S( -4, -25),   S(  6,   7),   S(  3,  -3),
            S(  5,  23),   S( -1,  10),   S( 14,  43),   S( -2,  -5),   S(  4, -21),   S( -7, -30),   S(  3, -33),   S( -4, -31),
            S(  4,  -9),   S(  0,  -6),   S(  7,  12),   S(  9,   3),   S(  7,  16),   S(  2, -45),   S(  7, -12),   S(-10, -67),
            S( -6, -19),   S(  2,   5),   S(  5,  16),   S( -7, -19),   S(  3, -14),   S(  0, -50),   S(  1, -25),   S( -7, -37),
            S(  3, -21),   S(  0,   2),   S(  9,  16),   S( -2, -31),   S(  1,  -7),   S( 11, -25),   S(  6, -30),   S( -3, -30),
            S( -7, -30),   S(  1,   5),   S(  6,  23),   S(  5,   7),   S( -6,  -7),   S( -7, -56),   S(  5, -27),   S(-13, -39),

            /* queens: bucket 0 */
            S( -3, -58),   S(-28, -31),   S(-37, -51),   S(  5, -138),  S(  3, -90),   S(  8, -65),   S(-61, -44),   S(-25, -29),
            S(-26, -44),   S( 23, -88),   S(  5, -61),   S(-18, -38),   S( -3, -45),   S( -5, -62),   S(-45, -58),   S(-12, -21),
            S(-13,  32),   S( -6, -43),   S( 30, -86),   S( -3, -31),   S( -3, -23),   S( -5, -13),   S(-30, -23),   S(-94, -53),
            S(-34,  37),   S( 21,   8),   S(-40,  72),   S(-17,  46),   S( -9,  63),   S( -8,  19),   S(-44,  -8),   S( -7, -70),
            S(-35,  -1),   S(-30, 116),   S( -3,  37),   S(-21,  85),   S(-11,  64),   S(-20,  57),   S(-54,  35),   S(-64,  13),
            S(-57,  67),   S( 17,  79),   S( 45,  76),   S(-48, 105),   S(-65,  81),   S(-73,  90),   S(-76,  25),   S(-45, -13),
            S(  0,   0),   S(  0,   0),   S( 35,  20),   S(-34,  22),   S(-36,  22),   S(-84,  79),   S(-66,  47),   S(-106,  28),
            S(  0,   0),   S(  0,   0),   S( 16,  40),   S( 13,  11),   S(-71,  34),   S(-21,  29),   S(-53,   0),   S(-68,  -9),

            /* queens: bucket 1 */
            S(  6, -36),   S( 11,  -9),   S( 18, -103),  S( 33, -113),  S( 30, -61),   S( 18, -53),   S(-23, -42),   S( -7, -13),
            S(-38,  42),   S( 36, -20),   S( 37, -82),   S( 25, -28),   S( 24,   0),   S(-16,   1),   S(-22,   6),   S(-35, -27),
            S( 41, -26),   S( 25, -27),   S(  8,  19),   S( 25,  13),   S(-15,  52),   S( 10,  33),   S( -3,  13),   S(  4, -18),
            S( 31, -35),   S(  1,  28),   S( 24,  -5),   S(  9,  55),   S( -5,  98),   S(-21,  83),   S( 19,  31),   S(-20,  58),
            S(  4,  17),   S( 12,  64),   S( 20,  89),   S(  8,  83),   S( 13,  88),   S( 83, -19),   S(-51,  89),   S( 18,  41),
            S( 60,  -9),   S( 69,  45),   S( 81,  39),   S( 92,  90),   S( 78, 105),   S( 14,  82),   S( 38,  71),   S(-16,  49),
            S( 87,  11),   S( 58,  15),   S(  0,   0),   S(  0,   0),   S(  6,  52),   S(-44,  77),   S(-18,  92),   S(-45,  53),
            S(109,  -1),   S( 63,  40),   S(  0,   0),   S(  0,   0),   S( 35,  36),   S( 50,  46),   S( 67,  20),   S(-22,  11),

            /* queens: bucket 2 */
            S( 32, -44),   S( 22, -41),   S( 32,  -3),   S( 45, -27),   S( 45, -57),   S( 25, -38),   S(-19, -54),   S( 30,  18),
            S( 24, -18),   S( 24,  15),   S( 35, -11),   S( 39,  -5),   S( 45, -21),   S( 14,   7),   S( 14,   3),   S(  3,  67),
            S( 25,  17),   S( 24,  10),   S(  6,  89),   S( 17,  36),   S( 24,  38),   S( 12,  46),   S( 30,  22),   S( 31,  32),
            S( 29,  24),   S( 21,  96),   S( 16,  82),   S( 10,  89),   S( 34,  72),   S( -2,  95),   S( 25,  79),   S( 17, 106),
            S( -6,  83),   S(  8,  45),   S(  5, 108),   S( 12, 130),   S( 23, 132),   S( 86,  65),   S( 38, 103),   S( 60,  55),
            S(-57, 118),   S(-27,  95),   S(-14, 112),   S( 89,  70),   S( 67,  93),   S( 89, 120),   S(103,  91),   S( 28, 151),
            S(  2,  55),   S(-27,  93),   S(-34, 121),   S( 86,  46),   S(  0,   0),   S(  0,   0),   S( 12, 118),   S( 48, 101),
            S( -5,  62),   S( 54,  23),   S( 80,  13),   S( 67,  65),   S(  0,   0),   S(  0,   0),   S( 85,  38),   S( 34,  98),

            /* queens: bucket 3 */
            S(-37,  39),   S(-18,  42),   S( -6,  35),   S( 11,  49),   S( -2,  25),   S(  0,   4),   S( 11, -58),   S(-26,  29),
            S(-35,  46),   S(-13,  49),   S( -5,  50),   S(  1,  72),   S(  1,  45),   S( 10,  15),   S( 36, -23),   S( 52, -83),
            S(-37,  56),   S(-22,  79),   S(-22, 119),   S(-25, 134),   S(-15, 104),   S(-17,  91),   S(  5,  62),   S( -1,  43),
            S(-23,  56),   S(-44, 119),   S(-36, 158),   S(-15, 165),   S(-12, 148),   S(-15, 115),   S(  4, 102),   S(-14,  82),
            S(-27, 109),   S(-34, 137),   S(-32, 144),   S(-21, 183),   S(-21, 178),   S(-15, 197),   S(-23, 146),   S(-25, 140),
            S(-44,  97),   S(-43, 134),   S(-61, 181),   S(-38, 174),   S(-22, 198),   S( 20, 174),   S(  0, 192),   S( -1, 183),
            S(-82, 140),   S(-78, 154),   S(-65, 184),   S(-85, 200),   S(-92, 215),   S( 10, 126),   S(  0,   0),   S(  0,   0),
            S(-133, 186),  S(-85, 159),   S(-59, 129),   S(-73, 171),   S(-45, 154),   S(-11, 132),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-24,   0),   S(-38, -17),   S( 21,  19),   S(-51,  -4),   S(-24, -28),   S(-21, -13),   S(-32, -29),   S( -8,  -7),
            S(-20,  -4),   S(-35,   9),   S(-24,  -2),   S(-22, -26),   S(-36,  10),   S( -2,  12),   S(-61, -50),   S(-27, -30),
            S(-17,  54),   S( 18, -15),   S( 12, -10),   S( 12, -13),   S( 21,  12),   S( 32, -25),   S(-22, -21),   S( 26,   8),
            S(  8,  20),   S( 17,   0),   S( -9,  -7),   S(  5,  40),   S( 57,  39),   S( -4,  23),   S(-37, -15),   S( 15,  21),
            S(  0,   0),   S(  0,   0),   S( 49,  32),   S( 47,  40),   S( 18,  58),   S( 17,  37),   S( -9,  -5),   S( -3,  23),
            S(  0,   0),   S(  0,   0),   S( 65,  66),   S( 69,  77),   S( -5,  29),   S( 12,  30),   S(-10,  13),   S(-28,  -4),
            S( 37,  26),   S( 26,  21),   S( 62,  22),   S( 51,  60),   S( 38,   8),   S( 19,  38),   S( 13,  -8),   S(-48,  -7),
            S( 26,  -2),   S(-19, -15),   S( 32,  38),   S( 54,  39),   S(  6, -16),   S(-31, -25),   S(-16, -33),   S( 18, -18),

            /* queens: bucket 5 */
            S( 37,   8),   S( 13, -35),   S( -3,  15),   S(-37,   9),   S( 21, -19),   S( 34,  44),   S( 11,  -5),   S(  3,  -4),
            S(  5,  -6),   S(  8,  -1),   S( 11, -18),   S(-39,  -9),   S( -2,  33),   S(-46, -15),   S( 13,   3),   S(  4,   2),
            S( 49,   9),   S( 23, -11),   S( 34,  22),   S( -6,  31),   S(-14,  24),   S( 47,  18),   S(-30,  19),   S(-15,   4),
            S(  5, -18),   S( 49,  12),   S( 19,   7),   S( 39,  30),   S( 70,  40),   S( 32,   7),   S( 19,  25),   S(  0,  20),
            S( 36,  15),   S( 23,   2),   S(  0,   0),   S(  0,   0),   S( 31,  24),   S( 37,  25),   S( 60,  58),   S( 11,  26),
            S( 55,  34),   S( 57,  57),   S(  0,   0),   S(  0,   0),   S( 19,  12),   S( 72,  59),   S( 15,  12),   S( 15,  27),
            S( 60,  26),   S( 86,  40),   S( 50,  51),   S( 40,  48),   S( 70,  40),   S(117,  67),   S( 53,  34),   S( 24,  24),
            S( 23,  20),   S( 62,  42),   S( 94,  77),   S( 41,  30),   S( 63,  41),   S( 74,  38),   S( 53,  48),   S( 23,  -4),

            /* queens: bucket 6 */
            S( 45,  39),   S(-33, -46),   S( 30,   3),   S(  0,  -2),   S(-18,  -7),   S(-31,  -1),   S(-15,   1),   S( 13,   7),
            S(-18, -11),   S( 27,  16),   S(  2,  53),   S(  3,  22),   S( 20,   0),   S(  0, -14),   S(-37,  -6),   S( 16,  17),
            S(-19,  13),   S(-10,  18),   S( 34,   1),   S(  4,  13),   S( 10,  25),   S( 23,   0),   S( 68,  44),   S( 53,  55),
            S( -3,  35),   S(  8,  -1),   S( 50,  18),   S( 70,  32),   S( 36, -11),   S( 46,  28),   S( 76,  42),   S( 98,  26),
            S( -2, -13),   S( -3,  30),   S( 14,  60),   S( 71,  72),   S(  0,   0),   S(  0,   0),   S( 64,  38),   S(109,  53),
            S( 46,  34),   S( 62,  54),   S( 49,  60),   S( 45,  49),   S(  0,   0),   S(  0,   0),   S(103,  58),   S(144,  49),
            S( 33,  23),   S(-18,  21),   S( 82,  12),   S( 64,  43),   S( 64,  63),   S( 54,  47),   S(115,  36),   S(153,  15),
            S( 24,   9),   S( 50,  -5),   S( 53,  25),   S( 92,  59),   S(138,  48),   S( 96,  66),   S(101,  37),   S( 66,  -3),

            /* queens: bucket 7 */
            S(-42,  -3),   S(-29, -47),   S(-27, -24),   S(-39,  20),   S(-38,  16),   S(-14,   2),   S(-20,  -2),   S(-25, -30),
            S(-19, -35),   S(-82,  18),   S( 20,  13),   S(-36,  58),   S(-43,  17),   S(-38,  44),   S( -3,  -3),   S(-49,   9),
            S(-21, -10),   S(-44,  21),   S(-14,  32),   S( -5,  48),   S( 42,  -8),   S( 13, -15),   S( 28, -29),   S( 49, -22),
            S(-53,  23),   S(-22,   7),   S( 13,  33),   S( 44,  32),   S( 96,  31),   S( 86,  16),   S( 38,   3),   S( 33, -22),
            S(-16, -14),   S(-20,   2),   S(-21,  60),   S( 33,  52),   S( 68,  36),   S( 73,  17),   S(  0,   0),   S(  0,   0),
            S(-13, -22),   S(-21,  30),   S(-34,  63),   S( 13,  22),   S( 41,  30),   S(125,  54),   S(  0,   0),   S(  0,   0),
            S(-48,   8),   S(-37,  10),   S( -9,  35),   S( 45,  28),   S( 36,  36),   S(116,  15),   S( 72,  30),   S( 91,  62),
            S( 21, -16),   S( 46,  -3),   S( 39,  -1),   S( 83,  -3),   S( 47,  33),   S( 35,  11),   S(-17,  10),   S( 84,   8),

            /* queens: bucket 8 */
            S(-12, -34),   S( 10,  -6),   S(  3, -10),   S(  7,  -7),   S(-24, -37),   S(  4,  -7),   S( -3, -11),   S(  0,   3),
            S(-18, -36),   S(-14, -18),   S( 26,  19),   S(  8,  17),   S( 12,  12),   S(  0, -11),   S( -7,  -5),   S( -7, -15),
            S(  0,   0),   S(  0,   0),   S( 14,   9),   S(  1, -29),   S(  8,  -4),   S(-12,  -4),   S( -9, -16),   S(  0,  12),
            S(  0,   0),   S(  0,   0),   S(  8,  12),   S( 13,  24),   S(  7,  10),   S(  1,  -4),   S(  5,  12),   S(  0,  -5),
            S( -1, -11),   S(  2,   5),   S( 17,  21),   S( 23,  -7),   S( -4,   2),   S(  2, -11),   S( 14,   6),   S(-15, -21),
            S(  1, -23),   S( 15,   2),   S( 27,  41),   S( -4, -19),   S(  8,  15),   S( 13,   1),   S(  6,  -9),   S(-16, -33),
            S( -2, -10),   S(  5, -10),   S( 20,  29),   S( 27,  34),   S( 19,   8),   S(  6,  17),   S(  6,  -5),   S(  1, -12),
            S(  4, -23),   S( 10,  12),   S(  3,   7),   S(  8,   7),   S( 18,   1),   S( -9, -21),   S( -6, -17),   S(-22, -45),

            /* queens: bucket 9 */
            S( 16,   8),   S( -3, -17),   S(  4,  -7),   S( 20,   1),   S(  4, -11),   S(  0,  -4),   S(-19, -34),   S( -6, -21),
            S( 15,  -1),   S(-12, -43),   S(  6,   0),   S( 21,  16),   S( -9, -31),   S( -5, -12),   S( -6, -16),   S( -5, -19),
            S(-10, -30),   S(  3,  -4),   S(  0,   0),   S(  0,   0),   S( 26,  34),   S( 18,  20),   S(-14, -22),   S(  9,  -4),
            S( 25,  18),   S(-19, -36),   S(  0,   0),   S(  0,   0),   S(  6,   4),   S(  9, -15),   S( 16,  21),   S( -9,  -8),
            S( 13,   6),   S( 25,  14),   S( 10,  15),   S(-14, -27),   S(  3,  -1),   S(  9,  10),   S(  1, -17),   S(  4, -15),
            S(  9,  -5),   S( 12, -11),   S( 15,   0),   S(  2,  -7),   S( 12, -13),   S(-14, -13),   S( -6, -17),   S( -6, -26),
            S( -7,  -9),   S( 30,  21),   S( -7, -19),   S(  9,  30),   S( 32,  22),   S( 34,  26),   S(  6,  10),   S( -7, -27),
            S(  3, -11),   S(  8,  -7),   S(-10, -22),   S(  2, -15),   S( 30,  29),   S(-12, -29),   S(  4, -21),   S( -6, -34),

            /* queens: bucket 10 */
            S( 11,  16),   S( 17,   9),   S(  3,   0),   S( -4, -24),   S(-12, -42),   S(  4, -12),   S(  2, -16),   S(  3, -12),
            S( 14,  18),   S(-10, -21),   S(  9, -13),   S(  1,  -9),   S(  2,  -9),   S( 21,  14),   S( 17,   7),   S(-16, -41),
            S(  2,   5),   S( -1,  -9),   S( -9, -23),   S(  3, -12),   S(  0,   0),   S(  0,   0),   S( 11,  17),   S(-22, -35),
            S( -9, -19),   S( 14,   6),   S( 10,  -3),   S( -7, -13),   S(  0,   0),   S(  0,   0),   S(-13, -28),   S(  1, -31),
            S( 16,  24),   S( 20,   6),   S(  7,  -2),   S( 47,  58),   S( 17,  35),   S( -2,  -8),   S(  5,  -1),   S( 19,  -9),
            S( -7,  -8),   S( 13,  18),   S( 33,  38),   S( 12,  25),   S( -2,   2),   S( 13,  28),   S( 39,  21),   S( -8, -40),
            S( -1, -12),   S( 20,  46),   S( 30,  44),   S( 16,  11),   S( 28,  44),   S( 19,  19),   S(  5, -15),   S(-19, -36),
            S(-22, -53),   S(-19, -26),   S( 25,   5),   S(-20, -44),   S(  9,   7),   S(  6, -12),   S( -4, -32),   S(  6, -16),

            /* queens: bucket 11 */
            S( -2,   9),   S( -1,  -6),   S(  1, -25),   S(-22, -36),   S( -7, -12),   S(-22, -41),   S( -4, -24),   S(-12, -24),
            S( -6,  -9),   S( -2,  -6),   S(-20, -38),   S( -5, -21),   S( 49,  26),   S(  3, -11),   S( 34,  30),   S( 19,  10),
            S(  2,  -1),   S(  4,  -4),   S( -2,  12),   S(-14, -34),   S( -6,   1),   S(-12, -29),   S(  0,   0),   S(  0,   0),
            S(-13, -20),   S( -4,  -7),   S(-15,  -3),   S(  3, -15),   S( -3,  -2),   S(  7,  21),   S(  0,   0),   S(  0,   0),
            S( -6,   2),   S(  5,   9),   S( 17,  33),   S(  4,  18),   S( 67,  92),   S( 35,  38),   S( 12,  10),   S(-11, -20),
            S(-15, -20),   S(-10, -20),   S(-17, -31),   S( -8,  -8),   S( 14,  21),   S( 23,   3),   S(  4,   5),   S( 12, -20),
            S(  3,  12),   S( -9, -10),   S(  2,  -7),   S( -6,  -7),   S(  9,  41),   S( 13, -14),   S(  0,   3),   S(  1, -22),
            S(-24, -77),   S( -8, -27),   S(-18, -16),   S( -4,   3),   S( 20,  25),   S( -1, -15),   S(  3,   7),   S( 19,  15),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  4,   8),   S(-13, -24),   S(  7,  24),   S(-11, -29),   S( -4, -19),   S( -3,  -3),
            S(  0,   0),   S(  0,   0),   S( 13,  26),   S(-10, -33),   S( -2, -15),   S(  1,  -4),   S(  1,   0),   S(  0,  -6),
            S(-10, -23),   S(  9,  12),   S( -2,  -6),   S(  3, -17),   S( 21,  42),   S( 14,  29),   S(-11, -20),   S(  6,   4),
            S( -5, -25),   S( 11,  10),   S(  9,  18),   S(  1,  11),   S( -2, -10),   S( -1, -18),   S( -5, -13),   S(-12, -22),
            S(-18, -37),   S( -4,  -4),   S(  1,  -4),   S(-11, -36),   S(-14, -19),   S(-19, -56),   S(-13, -39),   S( -4, -14),
            S( -2, -10),   S(  0,   0),   S( -7,  -5),   S(  8,  12),   S(-14, -31),   S(-15, -15),   S(-25, -35),   S( -5, -19),
            S( -3,  -7),   S(  4,   8),   S(  5,   0),   S(  9,  12),   S( -1, -11),   S(-11, -24),   S( -2,  -4),   S( -4, -18),
            S(  9,  13),   S(  2,   1),   S(  8,   5),   S(  6,  11),   S( -2,  -5),   S(-21, -48),   S( -9, -18),   S(-22, -43),

            /* queens: bucket 13 */
            S( -7, -30),   S( -5, -15),   S(  0,   0),   S(  0,   0),   S(-12, -30),   S(-10, -27),   S(  0, -24),   S( -8, -16),
            S( -4, -18),   S( -4, -11),   S(  0,   0),   S(  0,   0),   S(-14, -20),   S(-12, -29),   S( -7, -17),   S( -4, -13),
            S(  1, -10),   S( -4, -18),   S( -2,  -9),   S( 19,  28),   S(-20, -42),   S( -5, -19),   S( -3,  -4),   S( -5,  -7),
            S(-15, -31),   S(-11, -30),   S(  8,   2),   S( 10,  11),   S( 24,  50),   S( 19,  34),   S(  1,  -7),   S(-17, -38),
            S(  2, -11),   S( -2, -25),   S(  3, -13),   S( 14,  22),   S( -8, -20),   S(-13, -36),   S(-16, -33),   S( 13,  29),
            S( 15,  27),   S(-13, -41),   S(  2,   1),   S( 12,  21),   S(  4,   6),   S(-16, -25),   S(-31, -56),   S(-11, -33),
            S(  8,  13),   S(-10, -16),   S( 10,  24),   S(-12, -18),   S( -1,  -9),   S(  3,   2),   S(-13, -33),   S(-10, -26),
            S(-22, -43),   S( -7, -12),   S(-10, -17),   S( -2,  -5),   S( -5,  -9),   S( -3, -10),   S(  6,   8),   S(-15, -31),

            /* queens: bucket 14 */
            S( -7, -22),   S( 12,   5),   S(  0, -12),   S( -4, -17),   S(  0,   0),   S(  0,   0),   S(  1,  -6),   S( -5, -18),
            S(-17, -49),   S( -9, -26),   S(-10, -28),   S( -3, -16),   S(  0,   0),   S(  0,   0),   S(  3,   9),   S(-15, -34),
            S( -7, -13),   S(  4, -15),   S(  0,  -7),   S( -2, -11),   S(  2,   4),   S(  6,   8),   S(-10, -18),   S(-16, -46),
            S( -5, -12),   S( -2,  -1),   S(  7,   2),   S(-13, -41),   S( -3,  -4),   S(-11, -24),   S(  1, -15),   S( -5, -14),
            S( -8, -15),   S( -5, -13),   S( -4, -19),   S( 19,  38),   S(  1,  -7),   S( -5, -20),   S(  8,   1),   S( -5, -19),
            S( -4,  -7),   S(-12, -34),   S(-14, -27),   S(-14, -30),   S(  2,   1),   S(-12, -23),   S( -5, -18),   S(-20, -35),
            S(-10, -17),   S( -5, -13),   S(  3,   6),   S( -5, -14),   S( -7,  -6),   S( -4, -13),   S(-16, -36),   S(-14, -21),
            S(-20, -32),   S(  3,  -8),   S(-20, -33),   S( -6,  -9),   S(  3,   8),   S( -3,  -8),   S(-10, -19),   S(  0,   0),

            /* queens: bucket 15 */
            S( -2,  -9),   S(-12, -25),   S(  8,   8),   S(-23, -39),   S( 13,  15),   S( -1,  -4),   S(  0,   0),   S(  0,   0),
            S(  1,   3),   S(  0, -10),   S( -9, -16),   S(  2,  -5),   S(  6,   0),   S(  1,   0),   S(  0,   0),   S(  0,   0),
            S( -5, -11),   S( 11,  17),   S(  0,   2),   S( -4,  -7),   S( -8, -24),   S(  6,   1),   S( -5, -15),   S(  0,  -3),
            S( -6, -12),   S( -3,  -6),   S(  3,   3),   S(  8,  16),   S(  5,  -5),   S(  4,   6),   S( 10,  16),   S( -4, -16),
            S( -4,  -9),   S( -1,  -3),   S( -5,  -8),   S( -3,  -8),   S( 23,  57),   S(  6,  10),   S(  1,   0),   S(-17, -43),
            S(  1,  -4),   S( -3,  -3),   S(-15, -32),   S(-20, -38),   S(-11, -19),   S(-15, -33),   S( -6, -14),   S(-13, -24),
            S( -5,  -8),   S( -7, -16),   S(-18, -26),   S(  0,  -5),   S(-11, -21),   S(  0,  -5),   S(  4,   9),   S( -8, -17),
            S( -2,  -5),   S(-20, -48),   S(  3,  -7),   S( -1,  -3),   S(-11, -13),   S( -8, -15),   S(  2,   4),   S(-13, -17),

            /* kings: bucket 0 */
            S(  2, -30),   S( 22, -11),   S( 16,  -5),   S(-21, -14),   S(-19,   0),   S( 34, -31),   S( 14,   4),   S( 28, -53),
            S(-29,  34),   S(  3,   9),   S( -4,   9),   S(-55,  25),   S(-53,  42),   S(-10,  15),   S(-13,  36),   S(-16,  34),
            S(  8,   0),   S( 55, -14),   S(-22,  10),   S( -2,  -7),   S(-21,  -2),   S(-19,  -2),   S(-43,  30),   S(  8, -27),
            S( -8, -29),   S(  5, -18),   S(-24, -20),   S(-29,  17),   S(-62,  20),   S(-25,   8),   S(-36,  24),   S(-45,  27),
            S(-66, -95),   S( 23, -39),   S( 32, -40),   S(  5, -13),   S(-24,  -3),   S(-47,   9),   S( 14, -20),   S( 20,  -5),
            S( -6, -94),   S( 29, -19),   S( 26, -63),   S( 13, -14),   S( 14, -13),   S(  3, -24),   S( 19,  10),   S( 11, -15),
            S(  0,   0),   S(  0,   0),   S(  0, -53),   S( 30, -13),   S(  4, -21),   S(-10, -27),   S(  3,  -5),   S(-12, -20),
            S(  0,   0),   S(  0,   0),   S(-20, -76),   S( 19, -29),   S( 17,   2),   S( 13,  12),   S( 11,  11),   S(  8,  18),

            /* kings: bucket 1 */
            S( 33, -30),   S( 17, -11),   S(  9, -14),   S( 24,  -9),   S(-16,   3),   S( 28, -13),   S( 10,  18),   S( 27, -23),
            S( -3,   7),   S( 18,  15),   S(  9,  -4),   S(-43,  28),   S(-24,  19),   S( -4,  14),   S(  0,  25),   S( -7,  19),
            S(  9, -24),   S( 19, -11),   S( 20, -21),   S( 18, -16),   S(-46,  10),   S( -8, -15),   S(  8,   4),   S( 50, -17),
            S( -7, -23),   S( 14, -12),   S(  6,  -5),   S( -3,  13),   S( -5,  26),   S(-20,   7),   S(-24,  17),   S( -3,  13),
            S(-15, -25),   S(  7, -29),   S( 34, -42),   S( -7,  -9),   S( 29, -17),   S( -9, -17),   S(  3,   4),   S(  5,  15),
            S( 19, -27),   S( 29, -38),   S( 26, -23),   S( 38,  -9),   S(  6,  -8),   S( -1,   3),   S(  2,   9),   S(-10,  -6),
            S( -8, -33),   S(  4,  15),   S(  0,   0),   S(  0,   0),   S(-20, -10),   S(  8,   7),   S(  9,  58),   S(-12, -46),
            S(-16, -115),  S( -7, -19),   S(  0,   0),   S(  0,   0),   S(  6, -19),   S(  8,  22),   S(  3,  23),   S(-13, -59),

            /* kings: bucket 2 */
            S( 40, -68),   S(  5,   3),   S( 16, -28),   S( 17,  -8),   S(-13,  12),   S( 39, -28),   S(  9,  24),   S( 30, -15),
            S( 19,  -9),   S( -4,  33),   S(-17,  11),   S( -3,   5),   S(-26,  17),   S( -7,   4),   S( 11,   8),   S( -5,   9),
            S(-27, -10),   S(-12,  -5),   S( 30, -27),   S(-26, -10),   S( -7,  -3),   S(  4, -23),   S( 38, -14),   S( 30, -22),
            S( 10,  22),   S(-46,  17),   S(  3,   0),   S(-37,  17),   S(  6,   6),   S( -5, -10),   S( 19, -17),   S( 40, -23),
            S( -6, -17),   S( -7,  -4),   S( 11, -27),   S(  6,  -9),   S( 28, -33),   S( -6, -27),   S( 42, -35),   S( -8, -23),
            S( 18,  10),   S( 25, -10),   S( 10, -20),   S( 31, -30),   S( 63, -22),   S( 20,  -4),   S( 54, -30),   S( 25, -35),
            S(-11, -22),   S( -2,  16),   S(-17, -13),   S( 21,  21),   S(  0,   0),   S(  0,   0),   S(  8,  21),   S(-14, -33),
            S(-19, -30),   S(-13, -27),   S(-15, -59),   S( 10,  14),   S(  0,   0),   S(  0,   0),   S(  6,  28),   S(-23, -132),

            /* kings: bucket 3 */
            S(  4, -65),   S(  0,   1),   S( 24, -37),   S(-18,  -6),   S(-15, -25),   S( 38, -36),   S(  5,  15),   S(  8, -30),
            S(-11,  24),   S(-16,  30),   S( -9,  -5),   S(-35,  12),   S(-61,  28),   S( -1,  -3),   S( -9,  17),   S(-14,  14),
            S( 15, -31),   S( 22, -24),   S(  7, -17),   S( -3, -20),   S( -3,  -2),   S( 29, -32),   S( 36, -21),   S( 51, -24),
            S(-44,  24),   S(-91,  32),   S(-91,  23),   S(-84,  14),   S(-52,  13),   S(-73,   3),   S(-64,   0),   S(-65,  -8),
            S(-28,  14),   S(  7, -15),   S(-77,  -6),   S(-49,  -1),   S( -7, -29),   S( -7, -39),   S(-19, -45),   S(-31, -47),
            S(-25,  -5),   S( -5, -18),   S( 18, -24),   S(-38, -12),   S( 22, -37),   S( 80, -63),   S(118, -50),   S( 33, -114),
            S(-38,  -2),   S( 30, -18),   S( 17, -35),   S( 10, -17),   S( 26, -26),   S( 42, -37),   S(  0,   0),   S(  0,   0),
            S(  7,  -2),   S(  4, -30),   S(  1, -14),   S(  9,   6),   S( 11, -77),   S(  7, -27),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-35,  -8),   S( 16,  16),   S(-17,  36),   S(  4,   8),   S( 21, -12),   S(-51,  26),   S(  0,  25),   S( 32, -17),
            S(-37,  21),   S( -7,  31),   S(-17,  22),   S( -6,   5),   S( 70, -27),   S( 36, -11),   S( 51, -10),   S( 14,   0),
            S( -5,  16),   S(-16,  -5),   S(-16,  12),   S(-16,   2),   S(-16,  10),   S( 14, -24),   S(-29,   5),   S( 10,   1),
            S(  6,  -8),   S( 14,  -4),   S( 37,   7),   S(-15,   5),   S( 12, -13),   S(-18,  29),   S( -5,  10),   S( 18,  -4),
            S(  0,   0),   S(  0,   0),   S(  8, -25),   S( -6,  -1),   S(-30,  14),   S(-22,   6),   S(-27,  -5),   S(  0,   7),
            S(  0,   0),   S(  0,   0),   S(  0,   7),   S(  1,   5),   S( -1,   8),   S( -3, -27),   S( 12, -30),   S( 17,  23),
            S( -2,  -4),   S( -1,  10),   S(  7, -18),   S( 10, -13),   S( 23,  22),   S(-14,   7),   S(  1, -20),   S(  0, -20),
            S( -7,  12),   S(  5,   9),   S(  6,   8),   S( -2,  -4),   S( -2, -34),   S( -3,  -5),   S( -2,  -7),   S(  7,  18),

            /* kings: bucket 5 */
            S(  7,  -2),   S(  3,   4),   S(-44,  12),   S(-37,  23),   S(-24,  18),   S(  0,   8),   S( 43,   3),   S( 48,  -3),
            S( 21,  -3),   S( 47,   2),   S( 31,  -5),   S(  4,   2),   S( 12,   3),   S( 42,  -9),   S( 29,   9),   S( 52, -11),
            S( 10, -13),   S(-30,   6),   S( -7,  -6),   S(-16,  -4),   S(-14,   6),   S(-63,   6),   S( -2,   6),   S(  7,   1),
            S(-15,  -6),   S( 46, -13),   S(  4,  -4),   S( 19,  13),   S( 29,   7),   S(  7,   5),   S( 38,   1),   S(-17,  -3),
            S(-23,  -9),   S(-12, -35),   S(  0,   0),   S(  0,   0),   S( -6,   8),   S( 13,  -4),   S(  9,   0),   S(-24,   1),
            S(-22,  -7),   S(-15,  -4),   S(  0,   0),   S(  0,   0),   S( -4,   0),   S(-18,   2),   S(-34,  18),   S(-14, -10),
            S( -9,  -8),   S(  3,  31),   S(  1,  42),   S( -5,  -7),   S(-14,   7),   S(-10,  19),   S( 19,  50),   S( -4, -12),
            S(-13, -26),   S(  5,  23),   S(  4,  55),   S(  2,   7),   S( -6,  15),   S(-11,   6),   S(  1,   4),   S( -5, -17),

            /* kings: bucket 6 */
            S(  7,  -9),   S( 39, -17),   S(-16,  -4),   S( 12,  -2),   S(-22,  21),   S(-33,  21),   S( 27,  16),   S( 16,  10),
            S( 45, -16),   S( 34,  18),   S( 27,  -5),   S( 53, -16),   S( 42,  -9),   S(  8,   7),   S( 23,   4),   S( 21,  -1),
            S(  9, -19),   S(-24,   3),   S( -6, -13),   S( -8,  -6),   S(-11,  -3),   S(-48,  -1),   S(  0,   5),   S(-26,  12),
            S( -2,   5),   S( 26,  10),   S( 25, -11),   S( 34,   4),   S( 74,   3),   S(-15,   1),   S( 68, -15),   S( -7,  -2),
            S(-10,   3),   S(-11,  -2),   S(-12,  -9),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S(-14,  -8),   S(-54, -17),
            S(-22,  -3),   S( -6,   5),   S(-10,  -3),   S(-20,   7),   S(  0,   0),   S(  0,   0),   S(-33,  36),   S(-48, -13),
            S( 10, -20),   S(  3,  26),   S( -3,   6),   S( -6,  20),   S( -1,  -8),   S(  2,  -2),   S(-11,  12),   S(-14, -17),
            S(  5,  35),   S(-16, -19),   S(  3,  25),   S(-10,  -7),   S(  0,  23),   S( -4,  16),   S(  1,  32),   S(-10,   0),

            /* kings: bucket 7 */
            S( 55, -55),   S(-11,  10),   S( -1, -22),   S(-16,  11),   S(-40,  12),   S(-32,  27),   S( -8,  37),   S(  4,  14),
            S( 31,   7),   S( 44, -19),   S(  2,  -6),   S(-17,  -1),   S(-19,  10),   S(-37,  26),   S(  5,   6),   S(  9,  11),
            S( 17, -16),   S(-11,   6),   S(-39,   5),   S(-16,  -4),   S(-19,  -3),   S(-42,  10),   S(  6,   8),   S(-62,  22),
            S(-20,   3),   S( 20,  -6),   S(  9,  -7),   S( 19,   0),   S(-15,  15),   S( 44, -25),   S( 14, -13),   S( 46, -21),
            S(  1, -16),   S( -6,  13),   S( -1, -17),   S( -7,   2),   S(-15, -11),   S( 20, -30),   S(  0,   0),   S(  0,   0),
            S( -9, -26),   S(  0,  -1),   S( 18, -14),   S( 16,  -6),   S(  7,   4),   S(  7,  -3),   S(  0,   0),   S(  0,   0),
            S( 11,  11),   S( 22,  -9),   S( 17,  -8),   S( -3, -22),   S( 12, -12),   S(  2,   7),   S(  7,  23),   S(-17, -62),
            S(  6, -10),   S(  1, -17),   S( 24,  32),   S(  5,  -7),   S( -4,  -4),   S(-17, -33),   S(  6,  38),   S(-19, -43),

            /* kings: bucket 8 */
            S( -3, 113),   S(  2,  63),   S(-19,  65),   S( -7, -20),   S( -6,   9),   S(-13,  -4),   S( 53, -14),   S(-21,  10),
            S( 13,  77),   S( 10,  -9),   S( -2,  60),   S(-11,  10),   S( -4,   3),   S( -7,  12),   S( -6,   1),   S( 23,  31),
            S(  0,   0),   S(  0,   0),   S( 22,  40),   S( 11,   7),   S( 31,  -5),   S( -3,  -5),   S(  9,   9),   S( -6,  10),
            S(  0,   0),   S(  0,   0),   S( 19,  30),   S( 22, -20),   S(  8,  16),   S( 12,  -9),   S( 13,  -8),   S( -3,  18),
            S(  0,   1),   S(  0,  -2),   S(  9,  -2),   S(  9,  -2),   S( -3, -18),   S( -4, -10),   S(  8,  11),   S(-10, -33),
            S(  2,  18),   S( -3, -18),   S( -5, -11),   S( -3, -36),   S( -5, -17),   S( -1,   7),   S( -3, -11),   S(  3, -12),
            S( -1, -14),   S( -6, -25),   S(  5, -22),   S( -1, -11),   S(  1,   1),   S(  7, -17),   S( -1,  16),   S( -2, -35),
            S( -3, -17),   S( -5, -33),   S(  5, -10),   S(  0,   5),   S(  5,  24),   S( -2, -18),   S(  1, -24),   S(  6, -12),

            /* kings: bucket 9 */
            S(-20,  55),   S(-58,  41),   S(-66,  46),   S(-60,  33),   S(-80,  41),   S(-61,  31),   S( 54,  10),   S( 16,  16),
            S( -9,  20),   S( 12,  15),   S(  0,  -6),   S( 37,  13),   S( 47,  20),   S( 36,  -5),   S( 54,  13),   S( 23,   4),
            S(-11,  14),   S(  2,   6),   S(  0,   0),   S(  0,   0),   S( 11,  12),   S(-13,  -4),   S( 10,  -4),   S(-32,  19),
            S(  5,  -3),   S(  1,  -4),   S(  0,   0),   S(  0,   0),   S(  5,  10),   S( 41,  -5),   S(  2,   1),   S( -5,  19),
            S( -5,  -7),   S(  3,  24),   S( -1,   7),   S(  0,  -3),   S(  1, -14),   S(-19,  -8),   S( -7,  15),   S( 12,  -4),
            S(  2,  25),   S(-10,  10),   S( 10,   5),   S( -9,  -9),   S(  9,   7),   S(-10,  31),   S( -1,  -8),   S( -9,  34),
            S(  7,   9),   S(  4, -21),   S(  6,  18),   S( -6,  13),   S(  6,  28),   S( 19,  14),   S(-10, -22),   S( 20,  63),
            S(  4,   5),   S( -8, -21),   S(  3, -20),   S( -9, -40),   S( -3, -26),   S(  1, -13),   S( -2, -42),   S(  9,  30),

            /* kings: bucket 10 */
            S(-16,  26),   S(  7,  -5),   S( -7,  16),   S(-42,  22),   S(-58,  27),   S(-114,  42),  S(-11,  39),   S(-70,  79),
            S( 13,  -8),   S( 49,  18),   S(  6,  -9),   S( 16,  11),   S( 49,  13),   S( 25,  12),   S( 17,  23),   S(-22,  32),
            S(  5,   6),   S( 17,  -2),   S( -6,  -8),   S( -7,  11),   S(  0,   0),   S(  0,   0),   S( 17,  17),   S(-52,  17),
            S( 15,   5),   S( 19, -11),   S( 10,  -8),   S( 19,   1),   S(  0,   0),   S(  0,   0),   S( 18,  12),   S(  2,  -6),
            S( -5,   2),   S( 35,  14),   S(  9,  -3),   S( 12, -34),   S( 12, -15),   S(  7,  -1),   S(  3,   9),   S(-29,  20),
            S( -8,   1),   S(-11,  21),   S( -2,  24),   S(  1,  -5),   S(-10,  21),   S( -9,  -5),   S(-17,  28),   S( -5,  17),
            S( -6, -42),   S(  0,   1),   S(  1,  -6),   S(  8,  11),   S( 13,   2),   S(-10,  -9),   S( 10, -32),   S( 10,  40),
            S( -6, -14),   S( 17,  15),   S(  7,   7),   S(  3,  30),   S( -3, -23),   S(  0, -12),   S(-10, -48),   S( 10,  55),

            /* kings: bucket 11 */
            S(-38,  21),   S(  1,   4),   S( -2, -14),   S( -8,   7),   S(-13,  -7),   S(-155,  72),  S(-51,  54),   S(-142, 152),
            S( -9, -24),   S(-18,  29),   S( -8, -14),   S( 18,  -3),   S( 56,   8),   S(  6,  45),   S( 75,   0),   S( 27,  43),
            S(-23,  -8),   S( 11,  -8),   S( -5,  -2),   S(  0,   9),   S( 62,  -5),   S( 21,  16),   S(  0,   0),   S(  0,   0),
            S(  1,   8),   S(  8,  15),   S( 21,  -2),   S( 47, -16),   S( 40,  -9),   S( 26,   1),   S(  0,   0),   S(  0,   0),
            S( -4,   1),   S(-18,  14),   S( -4,  -3),   S(  7,  -8),   S( 35, -16),   S( -1, -12),   S(  7,  13),   S(  0, -20),
            S( 19,  13),   S(  5,  16),   S(  9,  -7),   S( -8, -20),   S(  5, -25),   S(  9, -22),   S( -6,  12),   S(  0,  -5),
            S( 20,  15),   S(  4, -13),   S(  4,  31),   S(  9, -17),   S( 15,  20),   S( -2,   4),   S(  4, -10),   S( -7, -21),
            S(  5,   5),   S(  2, -13),   S(-14, -50),   S(  0, -10),   S( -9, -24),   S(  1,  -9),   S(  2, -10),   S( 11,  36),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  1,  59),   S( -5,  16),   S( 13,  26),   S(  5,  11),   S( -7, -27),   S(-13,  37),
            S(  0,   0),   S(  0,   0),   S( 23,  97),   S(  8, -17),   S( 16,  26),   S( 10,  -2),   S( 27,  17),   S(-10,  26),
            S(  0,   1),   S(  0, -27),   S( 10,  14),   S( 11,  25),   S( -3, -11),   S(  3,   6),   S( -3, -21),   S(-18,  -2),
            S(  2,  -2),   S(  3,   1),   S( -8, -32),   S(  1, -28),   S( -6, -26),   S(  5,   9),   S(-13,  50),   S( -6,  -3),
            S( 13,  32),   S( -1,  12),   S(  0,  30),   S(  4,   4),   S(  0,  18),   S( -9,   6),   S(-10,  11),   S(-17, -39),
            S(  2,  20),   S(  2,  24),   S( -8,  -3),   S( -3,  -2),   S(  4,  -2),   S( -1,  19),   S(-12,   1),   S(  9,  33),
            S(  3,  10),   S( -6, -22),   S(  3,  14),   S( -7, -19),   S( -3, -23),   S( -2,   1),   S(  5,   0),   S( -5, -14),
            S(  1,   9),   S(  2,   3),   S(  4, -12),   S( -5, -40),   S( -1,   0),   S(  2,  -9),   S(-13, -37),   S(-10, -23),

            /* kings: bucket 13 */
            S(-20,  63),   S(  5, 108),   S(  0,   0),   S(  0,   0),   S( -3,  66),   S(-20,   2),   S(  8,  -2),   S( -6,  35),
            S( -7,  10),   S( -7, -14),   S(  0,   0),   S(  0,   0),   S( 34,  -1),   S( -9, -26),   S(-42,  16),   S(  4,  14),
            S( -8,   5),   S( -4,  31),   S( -4, -38),   S( -6, -23),   S(  9,   6),   S( -3,  -8),   S(-23,   7),   S(  5,   8),
            S( -7,  -9),   S( -1,  -1),   S(  6,  -6),   S(  3, -32),   S( 12, -24),   S( -2, -18),   S(-10,   0),   S(-14, -10),
            S(  2,  11),   S( -5,   7),   S(  5,  16),   S( -1, -18),   S( -9, -41),   S( -1,   4),   S( -8,   8),   S(  5,  33),
            S(  1,  11),   S( -6,   9),   S(  4,  34),   S( -4,  -2),   S(-16, -29),   S(  6,  26),   S( -5, -24),   S(  8,  29),
            S(  8,  25),   S( -6,  -2),   S( -8, -12),   S(  4,   1),   S( -4, -25),   S(  2, -18),   S(  8, -47),   S(  2,  17),
            S(  2,   2),   S(  1,  33),   S(  0,   7),   S(  4,   4),   S( -7,  -9),   S( -2,  -5),   S( -3,   4),   S(  8,  30),

            /* kings: bucket 14 */
            S(  5,  61),   S( -9, -13),   S( -8, -15),   S(  1,  37),   S(  0,   0),   S(  0,   0),   S( -9, 105),   S(-41,  67),
            S(-13,  -8),   S( -6,  -4),   S( -4, -29),   S(  9,   7),   S(  0,   0),   S(  0,   0),   S( 15,  -5),   S(-51,  -3),
            S(-10,   4),   S(  3,  19),   S( 16,  -9),   S( 11,  -2),   S(  7, -33),   S(  7,  47),   S(  8,  29),   S(-27, -14),
            S(  0,   3),   S(  3, -17),   S(  5, -15),   S(  9, -23),   S(-21, -63),   S( 10,  21),   S( 15,   4),   S( 11,  24),
            S(  7,  29),   S(  0,   2),   S( -4,   7),   S(-11, -26),   S( -7,  34),   S(  7,  36),   S(  6,  31),   S(  5,  20),
            S( -8, -28),   S( -9,   6),   S( -7, -12),   S( -1,  28),   S( -6, -10),   S(  5, -21),   S( -9, -35),   S( -5,  -8),
            S(  2,   7),   S( -6, -60),   S(  2,  26),   S( -2,  -5),   S(  2,   4),   S( -8, -46),   S(-17, -78),   S( 13,  86),
            S(  7,  28),   S(  1,  56),   S(  8,  36),   S(  2,   0),   S(  8,  60),   S( -2, -18),   S(-12, -46),   S(  0, -11),

            /* kings: bucket 15 */
            S(  2,  36),   S(  2, -11),   S(  7,  16),   S( -9, -18),   S(-17, -17),   S(-10,  90),   S(  0,   0),   S(  0,   0),
            S( -4, -30),   S(-10,   0),   S(-10, -30),   S( 25,  52),   S( 30, -31),   S( 19,  80),   S(  0,   0),   S(  0,   0),
            S(-14,  11),   S(  5,  -9),   S( -2, -10),   S( -9,  -1),   S(  9, -23),   S( 22,  47),   S( 12,  -7),   S(-11, -11),
            S(  3,   4),   S(  0,  24),   S(  9,  12),   S(-10, -31),   S(  4, -43),   S(  0,  -8),   S(  9,  42),   S( 13,  19),
            S( 12,  14),   S(-14,  23),   S( -3,   1),   S(-17, -54),   S( -1, -24),   S(-12,   9),   S( -5,  -8),   S( -5,   4),
            S(  4,  24),   S( -6,  -6),   S( -1,   1),   S(  5,  23),   S( -4,   4),   S( -3,  -3),   S( -3,  -5),   S(  3,  14),
            S(  9,  23),   S(  7,  38),   S( -6,  -6),   S( -5,  11),   S(  0, -19),   S(  7,  31),   S( -9, -14),   S(  5,  12),
            S( -5,  -7),   S( 11,  20),   S( -2,   1),   S( -1,  -9),   S(  3,  18),   S(  5,  20),   S( -1,   6),   S(  5,  -4),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-11, -194),  S( 10, -34),   S( 21,  33),   S( 29,  71),   S( 37,  93),   S( 40, 114),   S( 47, 114),   S( 57, 104),
            S( 79,  73),

            /* bishop mobility */
            S(  6, -77),   S( 10,  -2),   S( 19,  40),   S( 25,  73),   S( 31,  96),   S( 34, 112),   S( 36, 119),   S( 40, 116),
            S( 41, 119),   S( 50, 107),   S( 50,  98),   S( 80,  82),   S( 80,  83),   S(130,  46),

            /* rook mobility */
            S(-20,   1),   S(-13,  61),   S(-13,  94),   S( -9, 112),   S(-11, 135),   S( -2, 141),   S(  2, 154),   S(  9, 153),
            S( 13, 161),   S( 20, 164),   S( 30, 166),   S( 40, 163),   S( 51, 159),   S( 69, 149),   S(155, 104),

            /* queen mobility */
            S( 61, -43),   S( 64, -107),  S( 55,  34),   S( 50, 115),   S( 52, 138),   S( 50, 181),   S( 51, 217),   S( 50, 243),
            S( 51, 263),   S( 50, 282),   S( 46, 302),   S( 45, 316),   S( 43, 332),   S( 45, 335),   S( 42, 347),   S( 40, 354),
            S( 45, 349),   S( 46, 348),   S( 69, 328),   S( 83, 309),   S(103, 293),   S(162, 252),   S(150, 250),   S(176, 223),
            S(160, 234),   S(164, 212),   S(109, 188),   S( 92, 189),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  27),   S(-16,  58),   S(-34,  59),   S(-31,  82),   S( 24,  24),   S(-18,  42),   S( -7,  80),   S( 28,  35),
            S( 18,  38),   S(  3,  60),   S(-17,  57),   S( -6,  45),   S( -3,  47),   S(-34,  59),   S(-35,  81),   S( 36,  35),
            S( 40,  66),   S( 30,  68),   S( 13,  57),   S( 34,  45),   S( 10,  48),   S(-20,  64),   S(-16,  99),   S( 18,  64),
            S( 60,  88),   S( 66, 105),   S( 44,  67),   S( 22,  56),   S( 19,  58),   S( 29,  79),   S( -9, 107),   S(-39, 122),
            S( 75, 127),   S(123, 146),   S( 86, 101),   S( 62,  84),   S(-23,  80),   S( 53,  83),   S(  4, 131),   S(-47, 131),
            S(153, 179),   S(141, 217),   S(173, 198),   S(188, 193),   S(192, 202),   S(195, 186),   S(195, 198),   S(218, 201),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,   9),   S(  3,   4),   S( 10,   0),   S(  8,  35),   S( 24,  41),   S( -1, -20),   S(-16,  34),   S(  8, -28),
            S(-11,  41),   S( 29,   3),   S(  1,  49),   S( 22,  36),   S( 37,   1),   S( -5,  40),   S( 21,  -5),   S(  7,  11),
            S( -4,  18),   S( 15,  11),   S( -3,  57),   S( 18,  63),   S( 30,  29),   S( 24,  31),   S( 31,   1),   S( -5,  27),
            S( 16,  35),   S( 17,  53),   S( 49,  76),   S( -4,  98),   S( 89,  57),   S( 68,  43),   S( 37,  55),   S(  7,  30),
            S( 59,  35),   S( 96, 117),   S(113, 146),   S(170, 127),   S(156, 129),   S(146, 142),   S(146, 103),   S( 68,  44),
            S( 84, 181),   S(129, 272),   S(117, 264),   S(111, 236),   S( 67, 157),   S( 65, 158),   S( 63, 172),   S( 25, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 12,  47),   S( 26,  35),   S( 43,  54),   S( 41,  41),   S( 26,  40),   S( 27,  41),   S(  7,  22),   S( 45,  15),
            S(  0,  25),   S( 16,  45),   S( 14,  40),   S( 12,  52),   S( 26,  20),   S(  8,  25),   S( 27,  23),   S( -1,  16),
            S(  1,   8),   S( 22,  32),   S( 51,  39),   S( 42,  35),   S( 41,  44),   S( 64,   9),   S( 20,  23),   S( 16,   6),
            S( 69,  41),   S(127,   1),   S(125,  82),   S(167,  90),   S(159,  77),   S(103,  83),   S( 91,  11),   S( 85,  -9),
            S(101,  28),   S(146,  24),   S(159, 154),   S(148, 147),   S(189, 173),   S(155, 175),   S(176, 127),   S(-38,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  25),   S(  7,  19),   S( 23,  28),   S( -6,  63),   S( 67,  36),   S( 27,  12),   S(  0,   3),   S( 36,  16),
            S(  4,  12),   S(  8,   6),   S( 19,  17),   S( 18,  28),   S( 17,  13),   S(  1,   8),   S(  7,   5),   S( 31,  -8),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -4, -12),   S( -8,  -6),   S(-19, -17),   S(-18, -28),   S(-17, -13),   S( -1,  -8),   S( -7,  -5),   S(-31,   8),
            S(-25, -25),   S( -7, -19),   S(-23, -28),   S(  6, -63),   S(-67, -36),   S(-27, -12),   S(  0,  -3),   S(-36, -16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -27),   S(-11, -14),   S(  0, -32),   S(-47,  -9),   S(-18, -10),   S(-18, -25),   S(  7, -24),   S(-18, -36),
            S(-19,   5),   S(-15,   0),   S(-25,   7),   S(  4, -17),   S(-34, -11),   S(-27,  -2),   S(-48,   5),   S(-11, -13),
            S(-12, -17),   S(-12, -16),   S(-24,  -1),   S(-28,  -9),   S(-27, -26),   S(-23, -10),   S(-31,   1),   S(-40, -16),
            S(  1, -35),   S( 18, -37),   S( 15, -25),   S(  8, -30),   S(  6, -18),   S( 66, -40),   S( 34, -31),   S( -9, -42),
            S( 28, -59),   S( 50, -71),   S( 62, -41),   S( 73, -32),   S( 59, -28),   S( 82, -20),   S(108, -64),   S( 35, -64),
            S(129, -113),  S(182, -137),  S(162, -82),   S(115, -55),   S(123, -43),   S(161, -35),   S(114, -47),   S( 55, -90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   2),        // attacks to squares 1 from king
            S( 17,   2),        // attacks to squares 2 from king

            /* castling available */
            S( 67, -73),        // king-side castling available
            S( 22,  36),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( -2, -45),   S(  3, -41),   S( -7, -45),   S(-19, -31),   S(-27, -23),   S(-45, -14),   S(-76,   6),   S(-89,  10),
            S(-103,  15),  S(-80,  17),   S(-75,  23),   S( 11,   6),   S( -5,  10),

            /* orthogonal lines */
            S( -8, -61),   S(-58, -30),   S(-83, -17),   S(-96, -14),   S(-103, -20),  S(-106, -26),  S(-104, -32),  S(-103, -34),
            S(-119, -27),  S(-135, -29),  S(-167, -30),  S(-168, -36),  S(-185, -29),  S(-367,  31),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S(  5, 216),

            /* passed pawn can advance */
            S(-18,  40),   S(-17,  71),   S( -3, 109),   S( 21, 196),

            /* blocked passed pawn */
            S(  0,   0),   S( 55, -12),   S( 44,  -2),   S( 56,  18),   S( 67,  29),   S( 69, -13),   S(177, -34),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 48, -24),   S( 34,  37),   S( 28,  44),   S( 27,  66),   S( 64,  77),   S(122, 115),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-33,   1),   S(-34,   4),   S(  9, -37),   S(  2, -35),   S( 30, -46),   S(197, -82),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 13, -14),   S( 46, -49),   S( 21, -15),   S( 39, -67),   S( 33, -149),  S(-25, -254),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S(  7,  40),   S( 39,  40),   S(109,  34),   S( 32,  25),   S(219,  77),   S(162,  52),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-27,  94),

            /* bad bishop pawn */
            S( -5, -22),

            #endregion

            /* tempo bonus for side to move */
            S(  7,  11),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
