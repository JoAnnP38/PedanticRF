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
        public const int MAX_WEIGHTS = 12760;
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
        public const int ROOK_ON_OPEN_FILE = 12758; // rook on open file
        public const int TEMPO = 12759;             // tempo bonus for side moving

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

        public Score RookOnOpenFile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[ROOK_ON_OPEN_FILE];
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

        // Solution sample size: 6000005, generated on Wed, 06 Mar 2024 06:57:33 GMT
        // Solution K: 0.003850, error: 0.085642, accuracy: 0.4970
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 98, 179),   S(423, 539),   S(437, 569),   S(549, 971),   S(1339, 1652), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 98, -120),  S(139, -83),   S( 28, -29),   S(-55,  50),   S(-30,  15),   S(-21,  -9),   S(-55,  -3),   S(-45, -20),
            S(104, -120),  S( 97, -97),   S( -2, -54),   S(-25, -54),   S(-19, -21),   S(-28, -35),   S(-38, -24),   S(-44, -40),
            S( 93, -89),   S( 69, -48),   S( 12, -50),   S(  6, -58),   S(-12, -47),   S( -5, -47),   S(-20, -45),   S(-29, -38),
            S( 49, -32),   S( 48, -43),   S( 14, -45),   S( 10, -59),   S(-13, -48),   S(-34, -43),   S(-34, -37),   S(-43, -13),
            S( 76,  46),   S( 24,   1),   S( 62, -31),   S( 44, -59),   S( 26, -54),   S(-37, -35),   S(-32, -23),   S(-55,  55),
            S( 59,  59),   S(100,  72),   S(  0, -14),   S( 40, -29),   S(-19,  -7),   S( 21, -39),   S(-45,  12),   S(-25,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25, -31),   S( 26, -38),   S( 45, -21),   S(-10,  26),   S( -8, -13),   S(  5, -21),   S(-39, -11),   S(-42,  18),
            S( 17, -39),   S( 11, -45),   S(  4, -42),   S(-13, -41),   S(-13, -20),   S( -9, -34),   S(-32, -18),   S(-51,  -7),
            S( 15, -27),   S(  9, -22),   S( 20, -46),   S( 14, -46),   S(-11, -28),   S( 11, -41),   S(-11, -31),   S(-27,  -6),
            S( 24,   4),   S( 13, -30),   S( 23, -35),   S(  7, -40),   S(  0, -31),   S( 14, -38),   S(-40, -12),   S(-42,  25),
            S( -4,  64),   S(-40,   7),   S( 12, -25),   S( 19, -31),   S( 12,   0),   S(-14,   3),   S(-41,  18),   S(-32,  66),
            S( 49,  39),   S( 32,  -1),   S(-16, -27),   S(-58,  33),   S(-30,  -5),   S(-57,   7),   S(-58,  28),   S(-48,  70),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33,   6),   S(-35,   3),   S(-17,   3),   S(-16,   6),   S(  9, -14),   S( 31, -20),   S( -1, -43),   S(-17, -16),
            S(-35,  -9),   S(-53,  -4),   S(-33, -30),   S(-25, -35),   S(  3, -27),   S( -2, -26),   S(-13, -32),   S(-35, -20),
            S(-40,  -2),   S(-32, -16),   S(-12, -45),   S( -1, -53),   S(  2, -25),   S( 13, -26),   S(-12, -27),   S(-18, -11),
            S(-56,  25),   S(-35, -20),   S(-33, -23),   S( -6, -38),   S( 10, -37),   S(-11,  -9),   S(-22,  -3),   S(-28,  20),
            S(-45,  62),   S(-56,  23),   S(-54, -16),   S(-53, -15),   S( 19,   0),   S(-14,  13),   S(-42,  26),   S(-44,  82),
            S(-46,  72),   S(-113,  79),  S(-113,  16),  S(-52, -56),   S(-52,  23),   S(-32,   3),   S(-11,  -2),   S(-52,  73),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32,  -9),   S(-43,   3),   S(-33,  -3),   S(-11, -33),   S(-12,  -2),   S( 31, -15),   S( 80, -63),   S( 59, -77),
            S(-41, -27),   S(-52, -19),   S(-34, -40),   S(-26, -31),   S(-20, -20),   S( -3, -33),   S( 57, -62),   S( 48, -69),
            S(-40, -24),   S(-21, -47),   S( -7, -65),   S( -3, -61),   S( -7, -46),   S(  8, -35),   S( 27, -46),   S( 52, -50),
            S(-43,  -3),   S(-20, -52),   S(-13, -59),   S(  4, -67),   S( 18, -62),   S(  5, -44),   S(  9, -28),   S( 45, -12),
            S( -6,  28),   S(-23, -10),   S(  6, -51),   S(  4, -55),   S( 80, -54),   S( 58, -34),   S( 40,  23),   S( 36,  77),
            S(-47, 100),   S(-52,  28),   S( -2, -62),   S(-22, -55),   S( 15, -39),   S( 50, -28),   S( 51,  16),   S( 33,  79),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-92,  22),   S( 23, -31),   S(-22,   3),   S( 11,  17),   S(  0, -22),   S(-47,   9),   S(-87,   8),   S(-81,   7),
            S(-18,   4),   S( 38, -10),   S( 35, -33),   S(  6, -16),   S(-32, -16),   S(-42, -28),   S(-21, -37),   S(  0, -35),
            S( 66, -26),   S( 68, -17),   S( 11,  -1),   S( 14, -29),   S(-40, -25),   S( -3, -50),   S(-47, -37),   S(-33, -13),
            S( 22,  30),   S( 11,  37),   S( 49,  10),   S(  6,  13),   S( -2, -22),   S(-42, -15),   S(-13, -30),   S( 28, -23),
            S( -5,  91),   S(  1,  86),   S(-15,  45),   S( -5,   0),   S( 20, -17),   S( -6,  -8),   S(-26, -23),   S( 31,  16),
            S( 50,  75),   S( 75, 111),   S( 34,   2),   S( 18,  17),   S( -4, -38),   S( 25,  -4),   S( 29,  -4),   S(-43, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-104,  55),  S(-51,  30),   S(-30,  27),   S( 21,  10),   S(-44,  31),   S(-20,  12),   S(-46,   1),   S(-22,  15),
            S(-70,  24),   S(-63,  16),   S( 48, -22),   S(-12,  18),   S( 29, -14),   S(-12, -19),   S(-20, -14),   S(-37,   8),
            S(-57,  43),   S(-35,  21),   S( 81, -33),   S( -3, -11),   S( 32, -11),   S(-22,  -9),   S( -3,  -7),   S(-25,   7),
            S(-36,  60),   S(-29,  29),   S(  7,  17),   S( 37,  17),   S(  1,   7),   S(-64,  14),   S(-13,  -4),   S(  8,  20),
            S( 47,  64),   S(114,  27),   S( 57,  43),   S( 75,  19),   S(-13,  42),   S( 47,   9),   S( 22,   4),   S(-10,  43),
            S(101,  38),   S( 92,  22),   S( 53,  -5),   S( 52, -16),   S( 51, -22),   S( 16,   8),   S( 24,   8),   S( 38,  43),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48,  23),   S(-21,   1),   S(  2,   1),   S( 13,   0),   S(  0,  -9),   S(-22,   7),   S(-52,   1),   S(-54,  14),
            S(-40,   0),   S( -4, -24),   S( -5, -35),   S( 12, -14),   S( 40, -17),   S( 26, -21),   S(-35,  -5),   S(-66,  10),
            S(-41,  12),   S( -6, -10),   S( -8, -20),   S(-12, -11),   S( 40, -13),   S( 72, -29),   S(  4, -14),   S(-45,  22),
            S(-25,  26),   S(-54,  15),   S( 19, -13),   S( -3,   8),   S( 33,   7),   S( 45,  -2),   S( 44,  -7),   S( 53,   7),
            S(-29,  52),   S(-42,  36),   S(  3,  -1),   S( 30, -18),   S( 30,  39),   S(130,  13),   S( 59,  -1),   S( 54,  36),
            S( 69,  31),   S( 53,  12),   S(  0, -27),   S( 19, -39),   S( 54,  -1),   S( 38,  -7),   S( 52,   5),   S(100,  28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -22),   S(-55, -14),   S( -7, -15),   S(-47, -16),   S(-19, -30),   S( 45, -31),   S(-13, -38),   S(-62,  -9),
            S(-62, -26),   S(-38, -41),   S(-32, -46),   S(-11, -49),   S(-17, -29),   S( 43, -46),   S( 43, -38),   S(-21, -26),
            S(-69, -21),   S(-44, -42),   S(-30, -48),   S( -3, -46),   S(-13, -21),   S( 33, -26),   S( 40, -40),   S( 31, -29),
            S(-52, -12),   S(-60, -30),   S(-68, -34),   S(-28,  -8),   S(  3, -21),   S( 25,  -9),   S( 30,  -6),   S( 77, -16),
            S(-24, -11),   S(-20, -24),   S(-42, -20),   S(-12, -39),   S( 49,  -6),   S( 28,   2),   S( 91,  44),   S( 94,  45),
            S( 32,  -4),   S(-42, -44),   S( 31, -50),   S( 21, -65),   S( 35, -40),   S( 49, -13),   S( 46,  32),   S(115,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,  38),   S( -7,  39),   S(-24,  23),   S(-11,  -4),   S(-17, -15),   S(-42,  15),   S(-56,  -4),   S(-37,  25),
            S(-27,  33),   S(-51,  28),   S(-46,  22),   S(-13,  21),   S(-54, -19),   S(-52, -19),   S(-41, -27),   S( -2, -13),
            S(-33,  72),   S( 51,  60),   S( -7,  46),   S(-33,  29),   S( 14,  -9),   S(-37, -28),   S(-23, -45),   S( -2, -23),
            S( 63,  91),   S( 81, 114),   S( 47,  78),   S( 11,  33),   S(  2,  -2),   S( -8, -16),   S(  5,  -4),   S( -9, -21),
            S( 51, 114),   S( 70, 115),   S( 68, 119),   S( 61,  82),   S(  9, -22),   S(  8,  -2),   S( -2, -34),   S(  9, -13),
            S( 84, 141),   S( 63, 144),   S( 84, 178),   S( 48,  59),   S( -2,  -4),   S( -7, -57),   S(-10, -69),   S(  3,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-73,  59),   S(-56,  26),   S( 17,   9),   S( 10,  20),   S(-28,  -8),   S(-61,  21),   S(-84,   8),   S(-99,  37),
            S(-43,  17),   S(-70,  13),   S(-55,  19),   S( 10,  19),   S(-72,  12),   S(-48,   0),   S(-129,   8),  S(-54,  11),
            S(-60,  58),   S(-35,  61),   S(-35,  76),   S(-56,  64),   S(-54,  53),   S(-112,  16),  S(-21, -26),   S(-11,  -1),
            S(-16,  81),   S( 72,  75),   S( 52, 112),   S( 79, 118),   S(-38,  65),   S(-25,  22),   S( 40,   0),   S( 63, -16),
            S( 97,  77),   S( 58, 114),   S( 68, 124),   S( 82, 137),   S( 49, 104),   S( 26,  32),   S( 20, -25),   S( 33,  -9),
            S( 38,  52),   S( 81,  91),   S( 99,  95),   S( 99, 133),   S( 35,  78),   S( 11, -25),   S(  4, -28),   S( -6, -24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-125,  13),  S(-92,   6),   S(-29,  14),   S(-20,   0),   S(-27, -10),   S(-92,  40),   S(-120,  27),  S(-71,  30),
            S(-89, -11),   S(-29, -19),   S(-27,  -6),   S(-70,   0),   S(-36,  28),   S(-43,  23),   S(-146,  36),  S(-104,  18),
            S(-31, -17),   S(-49,  -6),   S(-37,  11),   S(-62,  49),   S(-72,  89),   S( 14,  45),   S(-98,  44),   S(-84,  46),
            S( 19,   4),   S( -8,   2),   S( 18,  15),   S( -1,  72),   S( 55,  97),   S(-12,  93),   S( 40,  37),   S( 52,  17),
            S( 47,  -7),   S( 25,   2),   S( 13,  54),   S( 77, 115),   S( 92, 111),   S( 55,  70),   S( 49,  62),   S( 69,  42),
            S( 47, -23),   S( 14, -22),   S( 45,  50),   S( 65, 111),   S( 47,  95),   S( 45,  68),   S( 42,  45),   S( 34,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-122,  10),  S(-93,  11),   S(  9, -15),   S(-25, -11),   S(  5,  28),   S(-112,  41),  S(-57,  17),   S(-83,  33),
            S(-96, -25),   S(-70, -34),   S(-39, -34),   S(-84,  19),   S(-34,   2),   S(-24,  18),   S(-79,  44),   S(-108,  48),
            S(-53, -25),   S(-42, -44),   S(-28,  -9),   S(-19,  -6),   S(-69,  37),   S(  2,  46),   S(-30,  60),   S(-22,  55),
            S( 35, -19),   S(-57, -31),   S(-16, -14),   S(-33,  24),   S(-26,  58),   S( 48,  49),   S( 55,  97),   S( 84,  69),
            S( -5,  -2),   S(-42,   9),   S( -5,  -3),   S(-22,  -1),   S( 67,  75),   S( 41,  86),   S(115,  94),   S(162,  65),
            S( 20, -12),   S(  5, -39),   S(  3, -11),   S(  1, -10),   S( 50,  43),   S( 45, 104),   S( 77, 152),   S(128, 114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7,  -2),   S(-41,  -7),   S(-36, -11),   S( -1,  -1),   S( -9,  -8),   S(-21,  -1),   S(  1,  -1),   S(-27,  20),
            S(-51, -29),   S( 17,   5),   S(-11,  13),   S( -6,  12),   S( -6,  41),   S(-15, -10),   S(-57, -36),   S( -9, -55),
            S(  8,  56),   S(-20,  42),   S( 18,  55),   S( 11, -10),   S(  6, -10),   S(-36, -21),   S(-44, -55),   S(-55, -65),
            S(  0,  75),   S( 25,  64),   S( 65, 119),   S( 55,  68),   S(-23, -36),   S(-21, -55),   S(  6, -17),   S(-37, -61),
            S( 78,  83),   S( 81, 196),   S( 60, 113),   S( 15,  60),   S(  8,   5),   S(  5, -10),   S(  2, -20),   S(-10, -88),
            S( 48, 126),   S( 41, 248),   S(115, 244),   S( 47,  96),   S( -5,  -6),   S( -5, -39),   S( -7, -55),   S(-16, -113),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -10),   S(-21,  11),   S( -5,  -9),   S(  0,   5),   S(-17, -25),   S(-27, -28),   S(-39, -38),   S(-32,  -9),
            S(-11, -39),   S(-24,   2),   S(  1,  -9),   S(  9,   5),   S(-37,   9),   S(-17,  -8),   S(-30, -53),   S(-27, -17),
            S( 27,  10),   S( 11,  -7),   S( 15,  30),   S(  6,  53),   S(-10,  41),   S(-18, -26),   S(-25, -36),   S( -4, -62),
            S( 17,  48),   S( 72,  53),   S( 48,  86),   S( 38,  95),   S( 36,  54),   S(  2, -20),   S( -3, -21),   S( 12, -54),
            S( 42,  66),   S( 89, 128),   S( 91, 173),   S(100, 206),   S( 63, 121),   S(  2,  -2),   S( -2, -54),   S( 13, -55),
            S( 29,  86),   S( 81, 146),   S(102, 243),   S(106, 243),   S( 72, 109),   S( 15,  36),   S(  3, -29),   S(  4, -37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -44),   S(-27, -20),   S(-21, -32),   S( -6, -10),   S(  4,  16),   S(-49,  21),   S(-28,   8),   S(  5,  46),
            S(-34,   6),   S(-23, -17),   S(-20, -62),   S(  3,  18),   S(-18,  26),   S( -7,  -2),   S(-33,  21),   S(-12,  -6),
            S(  0, -20),   S(  3, -32),   S( -7, -26),   S(  3,  21),   S( -9,  46),   S(-23,  22),   S(  1,  -1),   S( 19,  11),
            S( 20, -44),   S( 35, -24),   S( 11, -17),   S( 14,  64),   S( 20, 115),   S( 22,  59),   S( 10,  15),   S( 30,  33),
            S( 26, -42),   S( 23, -11),   S( 43,  37),   S( 51, 132),   S( 71, 188),   S( 96, 155),   S( 44,  79),   S( 30,  63),
            S( 28,   3),   S( 21, -21),   S( 28,  59),   S( 64, 145),   S( 74, 222),   S( 47, 181),   S( 33,  96),   S( 29,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-59, -61),   S(-30, -31),   S(-27, -67),   S( -4, -11),   S( 12,  29),   S(  2,  40),   S( -7,  22),   S( 22,  27),
            S(  0, -46),   S(-37, -26),   S(-31, -47),   S( 15,  11),   S(-20,  29),   S( -9,  41),   S( -7,  29),   S(-16,   6),
            S( -6, -77),   S( -5, -87),   S(-20,  -9),   S( -6,   4),   S(  1,  13),   S( 23,  45),   S( 18,  61),   S( 12,  63),
            S(  0, -57),   S(-34, -26),   S( -4, -31),   S(  5,  14),   S( 15,  39),   S( 47,  61),   S( 19, 106),   S(  5,  62),
            S(-10, -80),   S(-22, -98),   S(  2,   0),   S(  2, -20),   S( 50, 131),   S( 85, 109),   S( 51, 245),   S( 82,  86),
            S( 14,   0),   S(  2, -11),   S(  4,  -8),   S( 11,  30),   S( 43,  98),   S( 71, 201),   S( 27, 222),   S( 42,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-35, -14),   S(  8,   1),   S(-33,   3),   S(-15, -29),   S(-47, -15),   S( -7, -49),   S(-49, -60),   S(-33, -35),
            S(-44,  51),   S( 23, -58),   S(-49,   5),   S(  3, -20),   S( -6, -49),   S(-10, -13),   S(-34,   1),   S(-50, -42),
            S( -3,  82),   S( -9, -35),   S(  3, -15),   S(-19,  29),   S( 16,   6),   S(-30,  -4),   S(  4, -45),   S(-37, -27),
            S(  9, -12),   S( 26,  10),   S( 10,  22),   S( 22,  24),   S( 12,  -7),   S(  0, -13),   S( -9, -41),   S(  6, -43),
            S(  5, -25),   S( 29, -12),   S( 31, -16),   S( 71, -21),   S( 46, -20),   S( 36,  11),   S( -2,   3),   S(-51, -23),
            S(-15, -27),   S( 14,  16),   S( 37,   6),   S( 69, -18),   S( 54, -79),   S( 10, -14),   S(-11, -12),   S( -7, -39),
            S(  6, -17),   S( 29, -61),   S( 40, -21),   S( 51, -80),   S( 26, -22),   S( 20, -67),   S(-12, -33),   S(-30, -58),
            S(-85, -68),   S( -7,   7),   S(  9, -10),   S(-23, -97),   S( -5, -36),   S( 15,  13),   S( -4,   3),   S(  7, -12),

            /* knights: bucket 1 */
            S(-30, -17),   S(-42,  70),   S( 39,  22),   S(-14,  52),   S(-31,  60),   S( -3,  26),   S(-31,  51),   S( 19,  16),
            S( 51,  12),   S(  0,  42),   S( 16, -11),   S( -6,  36),   S(  0,   9),   S( 15,   4),   S( 22, -12),   S(-47,  24),
            S(-44,  26),   S(  8,  17),   S( -1,  14),   S( 11,  19),   S(  9,  22),   S(-23,  16),   S( -4,   1),   S(-14,   5),
            S(  0,  25),   S( 27,  25),   S( 21,  37),   S( 28,  20),   S( 14,  14),   S(  0,  31),   S( 41,  -7),   S(  9,  12),
            S(  1,  43),   S( -1,  15),   S( 25,  22),   S( 30,  27),   S( 24,  25),   S( 30,  12),   S( 21,  14),   S( 17,  20),
            S( 40,  20),   S( 36,  -2),   S( 39,  33),   S( 48,   8),   S( 18,  10),   S( 53,  26),   S( 47, -15),   S( 24, -32),
            S( 37,  -4),   S( -6,  37),   S( -1, -20),   S( 25,  25),   S( 12, -21),   S( 37, -15),   S(-12, -15),   S(  3, -22),
            S(-99, -63),   S(-35, -35),   S(-10,  10),   S( -8,  -7),   S( -8,  -7),   S(-19,  -1),   S(  4, -28),   S(-31, -60),

            /* knights: bucket 2 */
            S(-74,  15),   S(  2,  17),   S(-32,  39),   S(-22,  51),   S(-29,  45),   S(-29,  57),   S(-13,  32),   S(-45,  15),
            S(-24,  -3),   S(-24,  16),   S( -6,  15),   S( -5,  25),   S( -2,  15),   S( -6,  37),   S(-31,  64),   S(-45,  85),
            S(-35,  46),   S( -4,  24),   S(-13,  34),   S( 27,  14),   S(  9,  25),   S( -2,  12),   S( -3,  29),   S(-34,  41),
            S(-10,  32),   S(-30,  45),   S(  8,  39),   S(  8,  42),   S(  0,  45),   S(  1,  37),   S(-40,  61),   S( -4,  47),
            S( 12,  17),   S(-24,  33),   S(-23,  58),   S(-19,  61),   S( -1,  51),   S( -6,  48),   S( -5,  40),   S(-13,  23),
            S(-21,  30),   S( 27,  16),   S(-26,  59),   S( -7,  32),   S(-19,  42),   S(  0,  50),   S(-30,  35),   S( 50, -29),
            S( 19,  12),   S(-28,   1),   S( -5,  10),   S(-10,  13),   S(  2,   8),   S( 45,   5),   S(-67,  28),   S(-44,   9),
            S(-171,   5),  S(-14, -30),   S(-73,  27),   S( 12,  -4),   S(  5,  -3),   S(-52,   9),   S(  0,  15),   S(-202, -58),

            /* knights: bucket 3 */
            S(-60,  -2),   S( -1, -20),   S(-55,  27),   S(-15,  20),   S(-11,  16),   S(-19,  20),   S(  6,   5),   S(-29,  -5),
            S(-28,  10),   S(-43,  14),   S(-23,   1),   S(  3,  20),   S(  2,  20),   S(-27,  15),   S(-12, -10),   S(-21,  51),
            S(-23, -18),   S( -9,  22),   S(-12,  13),   S( 13,  19),   S(  8,  35),   S(  5,  15),   S( -2,   7),   S( -5,  41),
            S(-16,   2),   S(  0,  28),   S(  1,  50),   S( 12,  45),   S( 12,  44),   S( 12,  46),   S(  4,  35),   S( 10,  26),
            S(  9,  13),   S(-13,  32),   S( 16,  33),   S( 18,  61),   S(  6,  53),   S( 19,  56),   S( 26,  42),   S(  1,  39),
            S( 14, -12),   S( 36,  -5),   S( 33,  18),   S( 60,   9),   S( 68,  -1),   S( 92, -10),   S( 27,  -2),   S(-12,  71),
            S( -4,   2),   S( -1,  20),   S( 75, -41),   S( 47,  -1),   S( 48,  -4),   S( 82, -44),   S( 19, -33),   S( 44, -24),
            S(-120,  39),  S(  3, -26),   S(-37,  10),   S(  7,  24),   S( 24,  -2),   S(-28,  12),   S(-12, -13),   S(-53, -56),

            /* knights: bucket 4 */
            S( 11,   7),   S(-48,   4),   S( -2,  21),   S( 21, -12),   S(-22, -19),   S(-28,  -2),   S(-10, -64),   S(-20, -34),
            S( 50,  11),   S(-21,  55),   S( -3,  -8),   S(-25,   1),   S( 22, -28),   S(-21, -40),   S( 11, -42),   S(  2, -35),
            S(-12,  33),   S( 22,  20),   S( 24, -10),   S( 29,  11),   S( 18,  -9),   S(-16,   3),   S(-37, -47),   S(-45, -93),
            S(  0,  36),   S( 32, -34),   S( 72,  -4),   S( 21,  16),   S( 18,  -2),   S(126, -60),   S( 55, -51),   S(-24, -44),
            S( 39,  29),   S(  5,  18),   S( 67,  16),   S( 72,  18),   S( 85,   5),   S(-23,  -7),   S(-17, -52),   S(-12,   3),
            S(  2,  30),   S(-50, -11),   S(104,  12),   S( 21,  -8),   S(  8, -18),   S( 33,  23),   S( 26,  33),   S(-23, -26),
            S(  7,  23),   S(-19, -17),   S( 10,   8),   S( 22,  25),   S( 16,  -4),   S(  4, -20),   S(-14, -57),   S(-22, -10),
            S( -7,   5),   S( -1,  -5),   S( 17,  32),   S( -3,  -6),   S(-15, -15),   S( 12,   7),   S( -3,  13),   S( -6, -15),

            /* knights: bucket 5 */
            S( 20,   9),   S(-48,  40),   S( 52,   5),   S( 48,  36),   S( 50,  17),   S( 18, -17),   S(-21,  28),   S(-23, -27),
            S( 25,  -3),   S( 21,  30),   S(  7,  10),   S( 27,  11),   S( 70,   3),   S( 17,  23),   S( 30,   5),   S(-12,  -9),
            S( 36,   9),   S(  0,  37),   S( 87,   4),   S( 89,   3),   S( 16,  23),   S( 26,  18),   S( -2,  -2),   S(  5, -15),
            S( 54,  31),   S( 42,  19),   S( 60,  19),   S( -5,  35),   S( 42,  17),   S( 22,  32),   S( 55,  27),   S( 29,  24),
            S( 25,  47),   S( 72,   8),   S( 53,  39),   S( 82,  26),   S( 96,  35),   S( 38,  33),   S( 32,  29),   S( 13,  17),
            S( -9,  22),   S( -5,  34),   S( 16,  25),   S( 39,  52),   S( 71,  35),   S( 14,  45),   S( 22,  10),   S( -2,   6),
            S( 30,  59),   S(-17,  46),   S( 13,  35),   S( 10,  53),   S(  0,  30),   S( 16,  45),   S( 28,  40),   S(  0, -13),
            S(-17,  -2),   S( -3,   4),   S(  4,  17),   S( -4,   1),   S(  9,  32),   S( -1,  31),   S(  9,  36),   S(-29, -39),

            /* knights: bucket 6 */
            S( -8, -19),   S(-26,  -7),   S( 49,  14),   S(-60,  43),   S( -7,  23),   S( 29,  33),   S(-16,  30),   S(-21,  20),
            S(-12, -26),   S( 86,  -4),   S( 13,   6),   S(-27,  25),   S(-17,  48),   S( 80,  28),   S(  5,  25),   S(-17,  18),
            S(  5, -38),   S( 40,  -8),   S( 40,   9),   S( 77,  -1),   S( 24,  24),   S( -4,  33),   S( 30,  36),   S(-20,  50),
            S( 30,   8),   S( 59, -10),   S( 92,  13),   S( 87,   9),   S( 43,  15),   S( 55,  32),   S( 14,  43),   S(-16,  64),
            S( 36,  24),   S( 62, -10),   S( 81,  14),   S( 55,  44),   S(133,  23),   S(112,  22),   S( 26,  45),   S( 30,  49),
            S( 32,   3),   S( 39,   1),   S( 77,  19),   S( 41,  38),   S( 73,  34),   S( 39,  37),   S( 58,  25),   S( 44,  37),
            S( -6,  21),   S(  7,  42),   S(-40,  29),   S( 33,  53),   S(  0,  47),   S(  4,  53),   S(  1,  58),   S(-16,  22),
            S(-26,  16),   S( 10,  40),   S( 30,   5),   S(  8,  33),   S( 39,  50),   S( -5,  19),   S( 23,  44),   S( 24,  34),

            /* knights: bucket 7 */
            S(-35, -45),   S(-219, -39),  S(-62, -44),   S(-68, -12),   S(-21, -14),   S(-37,  -5),   S( -8, -14),   S(  4,   2),
            S(-42, -84),   S(-23, -76),   S(-26, -31),   S(-14, -28),   S(-42,   4),   S( 23, -17),   S( 36,  17),   S( -2,   9),
            S(-55, -84),   S(-48, -34),   S(-24, -11),   S( 31, -26),   S( 30, -17),   S( 29,  -6),   S(-15,  41),   S( 30,  43),
            S(-54, -19),   S( 22, -32),   S(  6,   6),   S( 56,  -4),   S( 75, -11),   S(  6,  15),   S(  9,   8),   S(-22,  49),
            S(-89, -13),   S(-23, -37),   S(109, -50),   S( 89, -18),   S(131, -20),   S( 62,  22),   S(110,  -2),   S( 72,  21),
            S( 13, -36),   S( 21, -20),   S(  0,  -5),   S( 73, -13),   S( 68,   5),   S(104,  -2),   S( 90,  -9),   S( -2,  50),
            S(-56, -43),   S(-77,   1),   S( -8, -27),   S( 43,  15),   S( 48,  -1),   S( 41,  -8),   S( -9,  15),   S( 23,   0),
            S(-39, -16),   S(  0, -14),   S(-23, -29),   S( 29,  19),   S( 21,  -5),   S( 16,  -4),   S( -2, -28),   S( -4,  18),

            /* knights: bucket 8 */
            S(  0,  -1),   S( -8, -11),   S( -1,  17),   S( -4, -26),   S(-20, -70),   S(-13, -60),   S(  0,  23),   S( -3, -24),
            S(  7,  25),   S( -3,  -5),   S( -5, -14),   S(-11,  -2),   S(-36, -27),   S(-15, -78),   S( -6, -54),   S(-27, -61),
            S(  6,  28),   S(-17, -35),   S(  6,   0),   S( -7,  -1),   S(  6, -15),   S(-33,  -9),   S(-15, -37),   S( -9, -44),
            S(-10,   7),   S( -1,  13),   S( -2,  18),   S( 23,  40),   S( 18,   6),   S( -4, -26),   S( -3, -40),   S( -9, -20),
            S( 41,  96),   S( -6,  30),   S(  2,  35),   S( 22,  43),   S(  5,  32),   S( 16,  19),   S( 17,  -2),   S(-15, -36),
            S(  8,  55),   S( 10,  38),   S( 16,  19),   S( 28,  -7),   S(  3,  39),   S( -1,  11),   S( -6, -34),   S( -4,  -3),
            S( -2,   4),   S( -2, -25),   S( -2, -21),   S(  3,  23),   S(  5, -11),   S(  7,  24),   S( 11,  45),   S(-15, -37),
            S(  9,  28),   S( 14,  35),   S( -4,  -9),   S( -4,   4),   S(  7,  30),   S(-13, -40),   S(  0,   7),   S( -4, -10),

            /* knights: bucket 9 */
            S(-10, -36),   S(-23, -45),   S(-18, -28),   S( -1, -13),   S(-15, -55),   S(-16, -33),   S( -1,   5),   S( -2, -18),
            S( -9, -27),   S(-13,   2),   S(-10, -50),   S(-15, -10),   S(-12, -19),   S(  7, -18),   S( -5,   3),   S(-11, -48),
            S(  8,  -2),   S(-18, -15),   S( -8, -17),   S(  9,   5),   S(-10,   7),   S(-27, -10),   S(-27, -22),   S( -6,   3),
            S(-18, -49),   S(-14, -14),   S( -8,   7),   S( 24,   5),   S( 20,  -3),   S(  7,  18),   S(-13, -35),   S(  9,  -1),
            S( 10,  39),   S(  9,  27),   S(  3,  30),   S( 10,  24),   S( 30,   5),   S(  8,  -2),   S(  3,  -9),   S(  2,   5),
            S( 11,  23),   S(  9,  27),   S(  7,  19),   S(  8, -12),   S( 24,  10),   S( 21,  33),   S( -4,  -4),   S(-11, -26),
            S(  0, -12),   S( -1,  22),   S( 26,  70),   S(  8,  22),   S( 25,  71),   S( -4, -26),   S( -1,   5),   S(-13, -20),
            S( -1,  -3),   S(  8,  39),   S(  6,  19),   S( 18,  59),   S( 11,  12),   S(  1,  28),   S(  9,  31),   S( -1, -16),

            /* knights: bucket 10 */
            S(-20, -56),   S(-14, -60),   S(-10, -40),   S(-27, -38),   S(-19, -38),   S(-11, -54),   S(  1,  11),   S(  4,  20),
            S(-10, -46),   S(-21, -64),   S(-15, -35),   S(-30, -43),   S(-19, -36),   S( -3, -63),   S( -8, -24),   S(  0, -18),
            S(-17, -60),   S(-18, -70),   S( 15,  -1),   S(-12, -40),   S( 11,  -4),   S(-11,  -5),   S( -6, -22),   S(-11, -21),
            S( -4,  -4),   S(-12, -52),   S( -7, -24),   S( 13,   1),   S( 32,  10),   S( 14,  22),   S( -4,   7),   S( 17,  27),
            S(-15, -51),   S( -9, -14),   S( -7,   9),   S( 42,  12),   S( 36,   8),   S( 32,  10),   S( 17,  13),   S( 21,  53),
            S(-11, -22),   S(  6,  11),   S( -6, -19),   S( -6,  13),   S( 32,  45),   S(  3,  40),   S( 28,  57),   S( 20,  42),
            S( -1,  -3),   S(-16, -41),   S(-10, -21),   S(  8,  21),   S( 20,  23),   S(  0,  18),   S(  4,  15),   S(  4,  27),
            S( -3, -19),   S(  4,  13),   S( -9, -24),   S( -1,  13),   S( 11,  48),   S(  2,  37),   S(  2,  15),   S(  1,   7),

            /* knights: bucket 11 */
            S( -4, -18),   S(-12, -25),   S(-20, -63),   S(-18, -45),   S(-18, -37),   S( -7, -23),   S( -6, -18),   S( -5,  -8),
            S(-24, -48),   S(-14, -54),   S(-15, -61),   S(-16, -32),   S(-14,  16),   S(-27, -38),   S(-14, -29),   S( -8,  -6),
            S(-13, -72),   S(-31, -47),   S(-30, -17),   S(  7,   4),   S(-41, -20),   S(-15,  16),   S( 11,   6),   S(  2,  -3),
            S(-26, -59),   S(  6, -27),   S(-25,  24),   S(  6,  14),   S( 29,  19),   S(  0,  -2),   S( 12,   3),   S(  6,   9),
            S( -5, -30),   S(-25, -54),   S(  6, -19),   S( 18, -34),   S( 18,  19),   S( 41,  29),   S(-11, -11),   S( 22,  90),
            S( -6, -11),   S( -9, -37),   S( -5,  23),   S( 48,  37),   S( -4,  -1),   S( 55,  66),   S( 17,  39),   S(  6,  59),
            S( 11,  44),   S( -3,  -4),   S(  4,   9),   S( 21,  10),   S( 16,  39),   S( -4,   0),   S(  9,  32),   S(  8,  73),
            S(  1,   1),   S(  2,  -3),   S(  9,  18),   S(  2,  -8),   S(  2,   4),   S(  0,   3),   S(  7,  15),   S( -2,  14),

            /* knights: bucket 12 */
            S( -3,  -7),   S( -3,   2),   S( -1, -15),   S( -1,  -2),   S( -4,  -3),   S( -2,  -2),   S(  2,   7),   S( -1, -10),
            S( -3, -10),   S(  1,   2),   S(  9,  24),   S(  1,  -4),   S( -6, -15),   S(-10, -47),   S(  0, -12),   S(  1,  15),
            S( -6, -16),   S(  5,   5),   S( -7, -19),   S( -4,  -9),   S( -2, -13),   S( -1,  -9),   S(  2,  -1),   S(-11, -26),
            S( -2,   1),   S( -2,  -6),   S( -3, -30),   S(  4,  29),   S( -2,  15),   S(  7,  15),   S(  2,  -5),   S( -1, -20),
            S(  9,  25),   S( -1,  -8),   S( -1,  -1),   S(-10,  -7),   S(-15, -37),   S(  9,  15),   S( -1, -12),   S( -6, -12),
            S( -3,   4),   S(  1,  -8),   S(  9,  70),   S(  0,  -2),   S(  3,  -7),   S( -4, -15),   S(  3,  -5),   S(  4,  18),
            S(  3,  16),   S(-10,  -9),   S( -3, -13),   S(  6,  13),   S(  4,   4),   S( -2, -16),   S(  0,   4),   S( -2,  -7),
            S(  4,  14),   S(  8,  33),   S( -3,  -5),   S(  6,  25),   S( -4,  -9),   S( -4, -13),   S( -4, -14),   S(  1,   6),

            /* knights: bucket 13 */
            S( -4, -10),   S( -3,  -3),   S( -2,  -8),   S( -3, -12),   S( -6, -14),   S(  0, -13),   S( -9, -28),   S(  1,   0),
            S( -2,  -6),   S( -2,  -3),   S( -3, -19),   S(  2,   6),   S( -9, -49),   S( -7, -33),   S(  3,  11),   S(  4,  -2),
            S( -9, -26),   S(-10, -42),   S(  6,  25),   S(  3,   3),   S( -2,  -3),   S( -7, -27),   S( -4, -21),   S(-13, -53),
            S(-17, -27),   S(  4,  17),   S(  7,  21),   S( -5, -17),   S(  4,  -4),   S(  8,  17),   S(  5,  18),   S( -2,  -6),
            S(  6,   6),   S( -8,  22),   S( 18,  26),   S( 13,  30),   S(-11, -17),   S(  3,   8),   S(-10, -23),   S( -5, -22),
            S( -3,   8),   S( 17,  31),   S(  3,  83),   S( -5,  17),   S( 10,  41),   S( -8, -35),   S(  2,  -1),   S( -5, -19),
            S(  1,  11),   S( -4,  18),   S( 19,  38),   S(  0,  35),   S( 13,  49),   S( -5, -12),   S( -4,  -7),   S( -4, -10),
            S(  1,   6),   S(  2,  38),   S( -3,  12),   S( -3,   0),   S( -5,  -5),   S( -1,  -2),   S(  0,   5),   S(  0,   1),

            /* knights: bucket 14 */
            S( -4, -22),   S( -6, -32),   S( -3,  -7),   S( -4, -17),   S( -8, -30),   S( -2, -16),   S(  0,  -2),   S( -3, -13),
            S( -1,  -8),   S( -6, -24),   S( -9, -45),   S( -4, -23),   S(  2,   5),   S( -2, -14),   S(  0,  -6),   S( -2, -11),
            S(  1,  -3),   S(  1, -10),   S(-20, -60),   S(  3,  13),   S(  0,  -5),   S( -3,  -5),   S( -2, -13),   S(  3,  -2),
            S(  3,  11),   S( -3, -15),   S( -8, -27),   S( -6,  -4),   S(  0, -11),   S(  9,   3),   S( -3, -11),   S( -8, -26),
            S( -2, -13),   S( -3,  -9),   S( 16,  55),   S(  9,  14),   S(  3,   3),   S( 13,   0),   S(  5,   0),   S(  1, -15),
            S( -4, -19),   S(  5,   4),   S(  1,  17),   S( 17,  21),   S( 15,  42),   S(  4,  15),   S( -1,   8),   S(  3,  29),
            S(  2,   5),   S( -2,  -5),   S( 11,   3),   S( -1,   2),   S(-11,  26),   S(-16, -11),   S(  0,  24),   S(  1,  14),
            S( -1,  -6),   S(  2,   9),   S( -5, -11),   S(  7,  29),   S( -3,  -3),   S(  1,  30),   S(  5,  32),   S(  0,   2),

            /* knights: bucket 15 */
            S(  0,  -1),   S( -1, -19),   S( -3, -25),   S( -4, -12),   S(  1,   4),   S( -1, -11),   S(  1,  -1),   S( -1,  -4),
            S( -2,  -5),   S( -1,  -9),   S(  0, -14),   S(-10, -41),   S(  0, -15),   S(  2,   9),   S(  2,   7),   S(  0,  -2),
            S( -9, -28),   S( -6,  -5),   S( -6, -27),   S( -7, -28),   S(  2,  -4),   S(  2,   4),   S(  1,   0),   S( -2,  -4),
            S( -6, -18),   S(-14, -50),   S( -8, -36),   S(  0,  -9),   S(  0, -20),   S(  5,  24),   S(  3,  -2),   S( -3, -13),
            S( -2,  -2),   S( -5, -12),   S( -5, -40),   S(  3,  -1),   S(  8,  12),   S(  2,   3),   S( -8, -17),   S( -7, -28),
            S( -5, -11),   S( -1,   5),   S( -4, -23),   S(  2,  13),   S( -6, -17),   S( -5,  -9),   S( -2,  10),   S(  1,  21),
            S( -7, -18),   S( -2,   0),   S(  2,   2),   S( -3,  -9),   S( -8,  -6),   S( -1,   9),   S(  1, -11),   S(  7,  38),
            S(  0,  -6),   S(  0,   3),   S( -5, -13),   S(  0,  -4),   S(  0,   8),   S( -5,  -8),   S(  6,  32),   S(  1,   4),

            /* bishops: bucket 0 */
            S( 56,  31),   S(-19,   3),   S( 59,   7),   S( 15,  10),   S( -8,  -3),   S( 16,  -7),   S( 25, -37),   S(  9, -21),
            S( 48, -18),   S( 85,  25),   S( 38,   5),   S( 28,   4),   S( -4,  25),   S(  9,  -3),   S(-31,  26),   S(-19, -16),
            S( 34,  28),   S( 49,  21),   S( 34,  41),   S(  4,  67),   S( 19,  43),   S(-18,  61),   S( 15,   0),   S( 22, -55),
            S( 43,   9),   S( 56,  28),   S( 40,  25),   S( 49,  43),   S( -8,  64),   S( 44,  11),   S( 10,  22),   S( 20, -21),
            S(  6,  41),   S( 44,  19),   S(  5,  57),   S( 55,  28),   S( 22,  54),   S( -6,  37),   S( 29,   4),   S(-45,  36),
            S(-20,  78),   S( -6,  45),   S( 84,  23),   S( 24,  43),   S( 35,  66),   S( 19,  23),   S(-32,  42),   S( -3,  41),
            S(-71,  55),   S( 28,  42),   S( 39,  46),   S(-25,  89),   S(-93,  55),   S( 25,  26),   S( 32,  23),   S(-21,   1),
            S(-37, -17),   S( -1,  39),   S(  4,  52),   S(-10,  30),   S(  9,  40),   S( 31,  40),   S(-21,  36),   S(-32,  22),

            /* bishops: bucket 1 */
            S( 19,  55),   S( 22,  16),   S(  8,  32),   S(  9,  10),   S( 15,  19),   S(  3,  24),   S( -8,  25),   S(-52,  23),
            S( 17,  -1),   S( 41,  10),   S( 42,  10),   S( 25,  23),   S(  2,   8),   S( 16,  -4),   S(-17,  26),   S( 29, -12),
            S( 39,  16),   S( -1,  37),   S( 51,  37),   S( -1,  51),   S( 20,  42),   S( -9,  43),   S( 32,  18),   S(  2, -15),
            S( 46,   0),   S( 27,  39),   S(  6,  38),   S( 33,  40),   S(-15,  71),   S( 21,  31),   S( 15,  26),   S( 23,   7),
            S( 29,  52),   S( -1,  47),   S(  9,  48),   S( -3,  53),   S( 44,  31),   S(-34,  43),   S( 23,   3),   S( -1,  28),
            S(-24,  50),   S( 29,  45),   S(  9,  63),   S( 47,  28),   S( 15,  55),   S( 41,  43),   S(-33,  54),   S( 52,   9),
            S( -5,  57),   S( 35,  29),   S(  6,  43),   S( 44,  48),   S( 28,  49),   S(-38,  69),   S( -5,  49),   S(-66,  41),
            S( 13,  52),   S(-41,  43),   S(-19,  19),   S(-58,  30),   S(-11,  34),   S( -8,  39),   S(-18,  58),   S(-26, 102),

            /* bishops: bucket 2 */
            S( 22,  26),   S( 15,  25),   S( -6,  34),   S(-22,  37),   S(-18,  34),   S(-17,  26),   S(-26,   1),   S(-70,  54),
            S(-19,  29),   S( 10,  21),   S( 12,  23),   S( -6,  39),   S( -1,  32),   S(  3,  10),   S(  8,   0),   S( -1, -24),
            S(  0,  15),   S(-12,  39),   S(  2,  70),   S(-10,  64),   S( -4,  51),   S(  4,  47),   S(  2,  44),   S(-23,  17),
            S( 12,  31),   S(-23,  67),   S(-21,  62),   S( -7,  69),   S(  5,  58),   S( -8,  54),   S( -3,  50),   S( -4,  14),
            S(-21,  43),   S(-19,  52),   S(-23,  58),   S(-31,  70),   S(-38,  79),   S(-18,  72),   S( -4,  39),   S(-30,  30),
            S( -1,  46),   S(-36,  63),   S(-18,  66),   S(-32,  58),   S( 11,  42),   S(-44,  86),   S(  5,  68),   S( -3,  45),
            S(-36,  52),   S(-23,  56),   S(-37,  76),   S( 16,  37),   S(-25,  63),   S(-55,  62),   S(-86,  70),   S(-27,  53),
            S(-27,  86),   S(-60,  90),   S(-95,  67),   S(-84,  71),   S(-65,  52),   S(-100,  63),  S(-10,  36),   S(-100,  45),

            /* bishops: bucket 3 */
            S( -5,  40),   S(  0,  43),   S( 10,  36),   S( -4,  38),   S( -6,  34),   S( 40,   6),   S( 26,   0),   S( 33, -43),
            S( -4,  41),   S(  5,  48),   S( 17,  17),   S(  4,  47),   S(  9,  35),   S(  6,  43),   S( 45,  36),   S( 24,   3),
            S( 25,  10),   S(-10,  62),   S(  7,  79),   S(  8,  56),   S(  5,  84),   S( 10,  64),   S( 26,  47),   S( 34,  -7),
            S( 27,  16),   S( -2,  60),   S( -4,  77),   S( 11,  84),   S( 14,  74),   S( 15,  57),   S(  3,  59),   S(  5,  24),
            S(  2,  43),   S( 12,  47),   S( 11,  56),   S( 20,  77),   S( 10,  69),   S( 23,  69),   S(  2,  55),   S(  9,  56),
            S(  5,  57),   S( 11,  62),   S(  1,  71),   S( 25,  43),   S(  9,  60),   S( 32,  66),   S( 25,  63),   S( -2,  82),
            S( -2,  40),   S(-40,  77),   S( 35,  42),   S( 11,  54),   S(  1,  49),   S(-12,  60),   S( 33,  56),   S( -2,  82),
            S(-50, 107),   S(-32,  76),   S( 37,  46),   S(-20,  59),   S(-46,  62),   S(-38,  66),   S(  0,  62),   S(132,   9),

            /* bishops: bucket 4 */
            S(-32,  -7),   S(-41,   1),   S(-62,  11),   S(-47,  23),   S(-30,  20),   S(-82,  25),   S(-31, -17),   S( 13,  -9),
            S(  3,  16),   S( 30,  13),   S(-33,  29),   S( 19,  -3),   S( -8,  -2),   S( 65, -24),   S(-30,   9),   S(  7, -19),
            S(-24,  19),   S(  6,   3),   S(  8,  23),   S(-22,  34),   S( 14,  22),   S( 25,  14),   S( -9, -13),   S(-79,  14),
            S(-12,  66),   S( 23,  41),   S( 86,   1),   S( 42,  31),   S(  7,  50),   S( 20,  15),   S( 18,  47),   S(-12, -18),
            S( 43,  25),   S( -2,  54),   S( 23,  50),   S(  7,  45),   S( 28,  15),   S(-30,  25),   S(-41,  -8),   S( -8,  17),
            S(-10,  23),   S( 18,  18),   S(  5,  19),   S( 18,  11),   S(  8,  41),   S( 11,  34),   S( 15, -10),   S(-16,  -1),
            S(-24,   4),   S( 31,  32),   S( -9,  31),   S( -3,  56),   S( -9,  22),   S(-10,  -2),   S(  4,   9),   S(  8,  15),
            S(  6,  38),   S(-14,   2),   S( -3,   9),   S(  2,  18),   S( -2,  16),   S(  4,  15),   S( -8, -21),   S( -3,  16),

            /* bishops: bucket 5 */
            S(-16,  25),   S(-15,  23),   S(  2,  15),   S(-16,  20),   S(-27,  26),   S(-21,  24),   S(-28,  25),   S(-30,  49),
            S(-55,  60),   S(-21,  44),   S(-50,  53),   S( 33,  17),   S( -9,  24),   S(-32,  26),   S(-17,   3),   S(-32,  20),
            S(-40,  54),   S(-40,  60),   S( 24,  44),   S( 32,  29),   S(  5,  46),   S(-25,  34),   S(-26,  45),   S(-19, -25),
            S( 16,  38),   S( 33,  35),   S(-23,  54),   S( 19,  46),   S( 30,  40),   S( 40,  37),   S(-35,  32),   S(  7,  35),
            S( 94,  41),   S( 22,  24),   S( 83,  21),   S( 96,  30),   S( 89,   9),   S( 51,  24),   S( 45,  24),   S(-33,  19),
            S( 34,  42),   S( 62,  26),   S( 45,  44),   S( 23,  52),   S(  0,  37),   S(  6,  21),   S( 17,  20),   S(-10,  43),
            S( -7,  53),   S(-24,  36),   S( 28,  42),   S( 20,  62),   S( 11,  58),   S( 19,  67),   S(  7,  38),   S(  0,  27),
            S( -7,  59),   S( 15,  28),   S(  9,  29),   S(  0,  36),   S( 17,  35),   S( -4,  41),   S(  6,  89),   S(  8,  23),

            /* bishops: bucket 6 */
            S( -4,  32),   S(-11,  49),   S(-16,  29),   S( -1,  19),   S(-35,  33),   S(-40,  42),   S(-18,  52),   S( 13,  25),
            S( 21,  24),   S(-10,  25),   S( -4,  39),   S(  0,  40),   S( -1,  33),   S(-19,  32),   S(-112,  70),  S( 29,  28),
            S( 26,  11),   S(  1,  36),   S( 30,  50),   S( 34,  36),   S( 70,  18),   S( 53,  19),   S(-23,  61),   S(-68,  29),
            S(  5,  25),   S(-11,  53),   S( 13,  48),   S( 65,  29),   S( 43,  41),   S( 40,  30),   S(-10,  57),   S(-16,  16),
            S(-44,  60),   S( -5,  45),   S( 64,  30),   S( 43,  37),   S(104,  38),   S( 91,  22),   S( 30,  46),   S( -9,  33),
            S(-10,  37),   S( 21,  30),   S( 11,  35),   S( 15,  51),   S( 14,  49),   S( 60,  45),   S( 49,  35),   S(-23,  67),
            S( -9,  27),   S(-10,  39),   S( 27,  35),   S(-15,  39),   S(  6,  52),   S(-32,  50),   S( 15,  46),   S( -7,  49),
            S(  3,  59),   S( 10,  69),   S( 32,  38),   S(  2,  49),   S(-11,  34),   S( 10,  34),   S( 11,  38),   S(  1,  58),

            /* bishops: bucket 7 */
            S(-19,  -5),   S(  3,  14),   S(-55,  -2),   S(-27, -10),   S( -9,  -9),   S(-56,   5),   S(-74, -30),   S(-36, -28),
            S(-49,  -2),   S(-77,  25),   S( -1,  -3),   S( 13,   0),   S( -2,   6),   S(-52,  26),   S(-14,  -8),   S(-22, -33),
            S(-42,  10),   S( 37,  -8),   S(  8,  30),   S( 65,   5),   S(-30,  33),   S( 10,   6),   S( -8,  43),   S( -8,  10),
            S(-46,  27),   S( 23,  19),   S( 77,  -8),   S( 50,  30),   S( 79,  22),   S( 27,   8),   S( 34,  32),   S( 14,   1),
            S( -4,   3),   S(-27,  11),   S( 65,  -8),   S( 94,  -7),   S( 68,  23),   S( 93,  16),   S(  7,  35),   S( 66,  -6),
            S(  1,   1),   S(  4,  14),   S( 20,   5),   S( 51,  -3),   S( 11,  14),   S( 81,  11),   S( 85,   6),   S(-15,  39),
            S( -4,  -1),   S(-11,  12),   S(  1,   9),   S(  6,  23),   S( 16,  -4),   S( 17,  10),   S( 36,  33),   S( -2,  27),
            S(-24,  13),   S(-20,  14),   S(-39,  22),   S(  2,  21),   S(-20,   0),   S( -2,   8),   S( 35,  32),   S( 27,  32),

            /* bishops: bucket 8 */
            S( -2, -35),   S( -4, -57),   S(-36, -30),   S(-12, -23),   S(  0,  -9),   S(-20, -44),   S(  4, -14),   S( -5, -13),
            S( -5, -27),   S(-33, -90),   S( -2, -15),   S( -5,  -8),   S( 21, -16),   S( -6, -50),   S( -3, -57),   S( -4, -42),
            S( -4, -17),   S( -1,   0),   S( -5, -21),   S(  9, -33),   S( 17, -12),   S(  3, -14),   S( 10, -43),   S(-29, -50),
            S(-12,  11),   S(  9, -17),   S( 14,  23),   S( 19, -14),   S( 25,   3),   S( 28, -25),   S( 15,  -8),   S(  8,   8),
            S( 16,  32),   S( 20,  45),   S(  6,   2),   S( 68,  13),   S( 28, -11),   S( 22,   9),   S( -5,  -4),   S(-14, -39),
            S( -4,  -1),   S( 13, -10),   S(  8,  25),   S( 17, -10),   S( 35,  -3),   S( -1, -25),   S( -9, -67),   S(-18, -50),
            S(-16, -37),   S( 16, -13),   S( 24,  11),   S( -4, -28),   S(  7,  -4),   S(  9,   7),   S( -2, -27),   S( -4,  -8),
            S( -5, -10),   S(  6, -44),   S(-15, -44),   S( -4, -24),   S( -2, -25),   S(  0, -38),   S( -5, -27),   S( -2, -17),

            /* bishops: bucket 9 */
            S(-16, -74),   S( 15, -35),   S(-30,  -2),   S(-10, -32),   S(-16, -41),   S(-27, -72),   S(-36, -65),   S(  0, -33),
            S( -9, -39),   S(-21, -35),   S(-19, -24),   S( 20,  -3),   S(  7, -25),   S( -4, -28),   S(-16, -40),   S(  2,  -2),
            S(  6, -28),   S( 16,  -9),   S(  0, -21),   S( 47,  -9),   S( 12, -16),   S( 12, -39),   S(-20, -21),   S(  0, -20),
            S(-22, -29),   S( 17,  -7),   S(-17, -12),   S( 71, -27),   S( 35,  -4),   S( 17, -16),   S( 10, -31),   S(-17, -71),
            S( -4, -13),   S( 23, -16),   S( 24,   4),   S( 39,   0),   S( 33, -20),   S( 42, -18),   S( 37, -11),   S( -3, -14),
            S( -5, -12),   S( 57,  15),   S( 14,   5),   S( 24,   9),   S( 24, -28),   S( 30, -16),   S(  5, -58),   S(-11, -40),
            S(  4,  -9),   S( 29,   7),   S(  6, -32),   S( 27, -17),   S( 22, -36),   S(  5, -41),   S( -6, -27),   S(  1, -10),
            S( -6, -26),   S( -6, -27),   S( -2, -31),   S(-14, -64),   S(  1, -21),   S( 13,  13),   S(  1, -24),   S( -9, -51),

            /* bishops: bucket 10 */
            S(-17, -60),   S(  3, -79),   S(-49, -28),   S( -3, -36),   S(-16, -37),   S(-12, -57),   S(-10, -76),   S(-18, -86),
            S(  8, -23),   S(-21, -45),   S( 30, -56),   S(-22, -46),   S( -2, -21),   S(  9, -42),   S(-10, -73),   S(-14, -56),
            S(  3, -34),   S( 20, -38),   S( -2, -40),   S( 29, -31),   S( 56, -44),   S( 14,  -4),   S(-17,  -7),   S(  4,   7),
            S(-13, -50),   S( 22, -39),   S( 24, -30),   S( 50, -19),   S( 54, -26),   S( 21,  -9),   S(-16, -22),   S(  4,   4),
            S(-24, -55),   S( 25, -22),   S( 52, -40),   S( 82, -11),   S( 41, -30),   S( 55, -13),   S(  9,  20),   S(-11, -50),
            S(-15, -62),   S(  7, -53),   S( 17, -41),   S(  8, -42),   S( 45, -19),   S( 57,   3),   S( 21,  -2),   S( -2, -58),
            S(-19, -85),   S(  0, -57),   S(  5, -51),   S( 10, -36),   S(  0, -48),   S( 23, -13),   S( 16,  31),   S( 12,  18),
            S( -1, -35),   S( -4, -33),   S( -7, -23),   S( -7, -31),   S( -6, -38),   S( -2, -55),   S(  0,  -7),   S(  7,  28),

            /* bishops: bucket 11 */
            S(-19, -19),   S(-43, -18),   S(-54, -52),   S(-20, -35),   S(-20, -21),   S(-55, -51),   S(  1, -25),   S(-27, -62),
            S(-15, -80),   S( 23, -39),   S(  1, -25),   S(-18, -32),   S(-18, -13),   S(-29, -65),   S( -4, -54),   S(-30, -68),
            S(-20, -84),   S( 13, -48),   S( 10, -27),   S( 25, -39),   S( -4, -14),   S(  9, -32),   S(-23, -29),   S(  9,  15),
            S( -2, -37),   S( -4, -41),   S( 13, -41),   S( 19, -36),   S( 58, -28),   S( 29,  -8),   S( 35,   1),   S(  1,  21),
            S(-18, -53),   S(-14, -74),   S( 16, -14),   S( 68, -25),   S( 51, -10),   S( 35,  10),   S( 13,  28),   S(  9, -15),
            S(-12, -83),   S( 14, -51),   S( 15, -31),   S(  9, -21),   S( 22, -21),   S( 51,  -8),   S( 18,  16),   S(-18, -18),
            S( -7, -50),   S(  0, -30),   S(  2, -39),   S(  2, -59),   S( 18, -49),   S( 51, -26),   S( 27, -25),   S( 11,   2),
            S(-17, -86),   S(-17, -56),   S( -4, -41),   S(  5, -33),   S( 14, -33),   S(-15, -65),   S(  0, -32),   S(  6,   0),

            /* bishops: bucket 12 */
            S(  2,  -2),   S( -8, -38),   S(-15, -66),   S( -9, -35),   S(  0, -33),   S(-16, -22),   S( -1,   7),   S(-11, -13),
            S( -4, -22),   S( -7, -33),   S(  6,  12),   S( -1, -21),   S(-11, -52),   S( -1,  -4),   S(  2, -13),   S( -6, -31),
            S( -3, -20),   S(-14, -46),   S( -3, -12),   S(  3, -33),   S( -1,   4),   S( -7, -47),   S(-12, -32),   S( -5,  -9),
            S(  0,  -5),   S( -4, -23),   S( -8, -62),   S(  6,   1),   S( 12,   2),   S( 10,  -2),   S( -2, -32),   S(-11, -39),
            S( -7,  -6),   S(  5,   4),   S( 14, -10),   S( -2,  -8),   S( 13,  -8),   S( -4, -32),   S(  1, -26),   S( -6, -10),
            S(-16, -47),   S(  1,  -4),   S( -4, -20),   S( -7, -31),   S(  0, -39),   S( -5, -17),   S(  1, -32),   S(  1,   0),
            S(  1,  -3),   S( -8, -23),   S(-10, -15),   S(-18, -57),   S(  9,  17),   S(  8,  -3),   S(-12, -42),   S( -5, -18),
            S(  1,   3),   S( -3,  -7),   S(  3, -20),   S(  4,  -3),   S(  6, -10),   S( -8, -40),   S(-12, -49),   S( -2,  -8),

            /* bishops: bucket 13 */
            S( -7, -55),   S( -9, -43),   S(-14, -70),   S(-10, -70),   S(  1, -37),   S( -7, -11),   S( -7, -24),   S( -9, -46),
            S( -2, -21),   S( -6, -42),   S( -2, -78),   S(-15, -50),   S(  0, -37),   S(  6,  13),   S(  0, -42),   S(  3, -14),
            S( -9, -45),   S( -6, -35),   S(  6,  -8),   S(  0, -43),   S(  2, -48),   S( 13, -25),   S(-11, -67),   S(  7,  21),
            S( -7, -17),   S(  2, -25),   S(  2, -25),   S(  5, -36),   S( 18, -34),   S(  6, -11),   S(  0, -22),   S( -8, -47),
            S( -7, -18),   S( -4, -23),   S( -1, -55),   S( 18,   2),   S(  8,  -9),   S( -1, -29),   S(  3, -38),   S( -6, -43),
            S( -2, -10),   S( -5, -41),   S(-10,  -9),   S( 11, -18),   S( 12,   3),   S(  8, -30),   S( 16,   2),   S( -6, -34),
            S( -5, -29),   S(  3, -11),   S( -1, -14),   S(-18,   1),   S( -1, -30),   S( 10,  11),   S( -9, -42),   S(  5,  13),
            S( -7, -31),   S( -2, -16),   S( -7, -19),   S( -1, -19),   S( -3,  -7),   S(  3,  -5),   S( -2, -14),   S(-10, -57),

            /* bishops: bucket 14 */
            S( -4, -39),   S(-10, -56),   S(-19, -78),   S(-10, -39),   S(-10, -61),   S( -9, -42),   S( -6, -66),   S(-13, -54),
            S(-15, -38),   S(  5, -17),   S( -4, -33),   S(-16, -52),   S(  3, -26),   S( -7, -50),   S(-13, -29),   S( -5, -34),
            S( -9, -39),   S( -6, -34),   S(-20, -76),   S( -4, -69),   S(-11, -75),   S( -3, -62),   S( -9, -42),   S( -4, -16),
            S(-10, -48),   S(-10, -29),   S( -1, -15),   S(  3, -23),   S(  1, -72),   S(  3, -34),   S( -7, -63),   S( -8, -29),
            S(-10, -42),   S(  0, -25),   S(  0, -35),   S( 20,  -1),   S( -2, -58),   S( 10, -21),   S(  9, -18),   S(  2,  -1),
            S( -2, -17),   S( -2, -35),   S(  1, -31),   S( -1, -36),   S( 13,   9),   S(  7,   6),   S(  5, -36),   S( -6, -43),
            S( -6, -45),   S( 13, -21),   S(-10, -43),   S( 15,   7),   S(-13, -51),   S(  3, -28),   S( -5, -14),   S( -4, -25),
            S( -1, -17),   S( -4, -26),   S( -2, -27),   S( -3, -28),   S( -8, -39),   S(  1,   1),   S( -2,  -4),   S( -1, -17),

            /* bishops: bucket 15 */
            S( -3,  -3),   S( -1,   1),   S(-10, -25),   S( -9, -25),   S( -9, -38),   S(-11, -24),   S( -2,  -4),   S( -5, -22),
            S( -2, -11),   S( -1,  -5),   S(  5,   8),   S(  0, -41),   S( -9, -27),   S( -3, -17),   S( -4, -21),   S( -4, -17),
            S(-10, -48),   S( -2, -21),   S(-11, -64),   S(-13, -36),   S(-12, -70),   S( -8, -13),   S( -4, -40),   S( 10,  28),
            S(  2, -10),   S(-10, -46),   S(  5,   1),   S(-10, -67),   S(  2, -39),   S( -1, -33),   S(  3,  -3),   S( -5, -21),
            S( -2, -18),   S(-11, -35),   S(-15, -66),   S(-15, -44),   S(  4, -24),   S( -7, -29),   S(  7, -21),   S( -3, -11),
            S( -6, -30),   S(  1, -51),   S( -6, -20),   S(-10, -50),   S(  5, -14),   S( -5, -16),   S(  9, -21),   S(  1,  -1),
            S( -1, -27),   S(  2,  -8),   S(  2,  -8),   S( -3, -31),   S( -8, -28),   S(  0,  -7),   S(-11, -19),   S(  0,  -4),
            S(  2,  -2),   S(  0,   3),   S( -3, -37),   S( -7, -46),   S(  2, -20),   S(-17, -49),   S(-14, -36),   S( -3, -14),

            /* rooks: bucket 0 */
            S(-24,  -2),   S(-11, -10),   S(  1, -19),   S( 17, -16),   S( 16,  -1),   S( 16, -15),   S(  4,   0),   S( 14,  -3),
            S( 21, -77),   S( 41, -21),   S( 21, -13),   S( 28, -19),   S( 21,   8),   S( 17,  -7),   S(  2, -16),   S(-40,  32),
            S( 39, -39),   S( 36,   0),   S( 56, -20),   S( 23,   3),   S( 15,  27),   S(  5,   4),   S(-24,  13),   S(-28,  -8),
            S( 45, -38),   S( 75, -17),   S( 26,  41),   S( 47,  -5),   S(  9,  12),   S(-19,  14),   S( -5,  -5),   S(-17,  19),
            S( 77, -40),   S( 79, -11),   S( 61,   5),   S( 21,  33),   S( 57,   6),   S( 45,   4),   S( 30,  22),   S( -3,  14),
            S( 86, -48),   S( 64, -16),   S( 74, -14),   S( 49,   5),   S( 70,  -7),   S(-35,  44),   S( 40,   4),   S(-28,  40),
            S( 51, -19),   S( 67,  -8),   S( 32,  18),   S( 20,  25),   S( 46,   3),   S( 16,  18),   S(-11,  23),   S(  0,  43),
            S( 18,   8),   S( 12,  28),   S( 31,  23),   S( 30,  20),   S( 51,   3),   S( 50,   6),   S( 40,  24),   S( 39,  10),

            /* rooks: bucket 1 */
            S(-77,  35),   S(-36,   3),   S(-28, -18),   S(-35, -12),   S(-11, -22),   S(-11, -17),   S(-21, -13),   S(-33,   7),
            S(-50,  10),   S(-67,  21),   S(-11,   0),   S(-24, -28),   S(-45,   7),   S(-39,   0),   S(-21, -33),   S(-40,   4),
            S(-24,  22),   S(-23,  36),   S(-18,  22),   S(-48,  41),   S(-38,  23),   S( 11,  -6),   S(-11, -16),   S(-43,  23),
            S(-68,  58),   S(-35,  36),   S( 11,  28),   S(-32,  40),   S(-29,  38),   S(-47,  43),   S(-48,  40),   S(-38,  27),
            S( 79,   0),   S( 34,  43),   S( -8,  24),   S(-17,  26),   S( -1,  18),   S( 41,  11),   S( -1,  22),   S(-33,  22),
            S( 99,   2),   S(-29,  56),   S( 31,  33),   S(-11,  31),   S( 36,  18),   S(-20,  36),   S( 17,  24),   S(-58,  45),
            S( -1,  35),   S( 26,  26),   S( 37,  24),   S(-77,  80),   S(-38,  46),   S( -1,  45),   S(-39,  39),   S(-23,  26),
            S( 34,  24),   S( 36,  20),   S(  3,  44),   S(-64,  63),   S(-20,  33),   S( 50,   7),   S( 13,  22),   S( 11,   9),

            /* rooks: bucket 2 */
            S(-65,  49),   S(-39,  32),   S(-39,  26),   S(-55,  30),   S(-53,  23),   S(-58,  28),   S(-42,  -1),   S(-51,  37),
            S(-80,  46),   S(-71,  53),   S(-36,  40),   S(-56,  30),   S(-31,  12),   S(-57,  13),   S(-60,  13),   S(-59,  17),
            S(-69,  63),   S(-47,  60),   S(-42,  57),   S(-30,  24),   S(-32,  38),   S(-32,  43),   S(-31,  24),   S(-21,  15),
            S(-68,  75),   S(-53,  66),   S(-29,  74),   S(-30,  60),   S(-35,  58),   S( -3,  41),   S(-42,  63),   S(-13,  27),
            S(-31,  70),   S(-45,  77),   S(-45,  74),   S(-18,  48),   S( 41,  39),   S( 13,  62),   S(-28,  57),   S(-39,  57),
            S(-30,  60),   S(-33,  71),   S( -7,  54),   S(-20,  57),   S( 29,  38),   S( 30,  30),   S( 27,  32),   S(-25,  56),
            S(-51,  67),   S(-64,  88),   S(-28,  72),   S(-20,  74),   S( 28,  36),   S(  1,  39),   S(-85,  92),   S(-20,  56),
            S(-26,  80),   S(  0,  58),   S(-55,  74),   S(-14,  50),   S(-17,  67),   S( -6,  48),   S(-44,  83),   S( 52,  30),

            /* rooks: bucket 3 */
            S(  1,  76),   S(  8,  70),   S( 15,  64),   S( 25,  49),   S( 21,  52),   S( -3,  61),   S(  4,  62),   S( -9,  46),
            S(-36,  85),   S(-16,  71),   S( 12,  69),   S( 14,  62),   S( 24,  58),   S( 16,  48),   S( 40,   3),   S( 32, -39),
            S(-32,  89),   S( -9,  91),   S( 13,  82),   S( 21,  61),   S( 28,  65),   S( 26,  68),   S( 19,  55),   S(  6,  57),
            S(-20,  91),   S(-28, 103),   S( 27,  79),   S( 30,  76),   S( 25,  71),   S(  1, 103),   S( 55,  58),   S( 32,  61),
            S( -5, 102),   S( 27,  87),   S( 16,  82),   S( 37,  78),   S( 45,  78),   S( 52,  71),   S( 88,  56),   S( 55,  50),
            S(  3,  97),   S( 27,  81),   S( 36,  81),   S( 26,  80),   S( 44,  65),   S( 71,  45),   S(106,  33),   S( 91,  23),
            S(-14, 105),   S(-14, 113),   S(  5, 108),   S( 33,  91),   S( 26,  89),   S( 64,  65),   S( 63,  66),   S(126,  28),
            S(-51, 150),   S(  6, 100),   S(  4,  95),   S( 46,  69),   S( 69,  65),   S( 97,  45),   S(155,  35),   S(124,  24),

            /* rooks: bucket 4 */
            S(-73,  17),   S(-20, -17),   S(-50,  16),   S(-25,   4),   S(-53,  -4),   S( 25, -55),   S( -4, -46),   S( -3, -34),
            S(-25, -17),   S(-49,  -4),   S(-38,  15),   S(-16,  22),   S( -6,  -9),   S( -2, -20),   S(-21,  -9),   S(-40, -19),
            S( 13,  15),   S(-12, -36),   S( -9,   9),   S( -6, -15),   S( -7,   4),   S( 10, -32),   S( 10, -19),   S(-70,  -2),
            S(-51, -10),   S(-38,  -7),   S(-17,  22),   S( 16,   9),   S( 25,   5),   S(-12,  -7),   S( -1,   0),   S(-19,  19),
            S(-19, -15),   S(-19,  15),   S( 11,  -4),   S( 56,  23),   S( 16,  14),   S(  8,  10),   S( 21,  23),   S( 49, -17),
            S(  7,  15),   S( 29,  -8),   S( 42,  24),   S( 44,  18),   S( 27,   3),   S( 27,  17),   S(-17,  13),   S(-10,  27),
            S(  5,   9),   S( 20,  37),   S( 29,  10),   S( 52,  21),   S( 72, -17),   S( 11, -12),   S( 45,   7),   S( 35,  38),
            S( 25, -54),   S( 47,  22),   S( 37,   7),   S( -9,  13),   S( 12, -19),   S( 29,   5),   S(  6,   5),   S( 10,   3),

            /* rooks: bucket 5 */
            S(-21,  19),   S(-20,  38),   S(-26,  19),   S(-44,  15),   S(-23,  10),   S( -4,  15),   S( 17,  12),   S(-12,  22),
            S(-25,  16),   S(-31,  43),   S(-99,  76),   S(-72,  46),   S(-48,  29),   S( -7,   8),   S(  8,   5),   S(-44,  11),
            S(-31,  46),   S(-54,  51),   S(-66,  73),   S(-92,  69),   S(-61,  36),   S(-32,  36),   S( -7,  25),   S( -5,  29),
            S(-54,  76),   S( -1,  37),   S(-11,  53),   S(-39,  53),   S(-27,  46),   S(  0,  58),   S(-26,  64),   S(-13,  28),
            S( -5,  67),   S( -3,  59),   S( 25,  54),   S( -6,  68),   S( 35,  47),   S( 41,  65),   S( 28,  49),   S( 34,  35),
            S( 54,  65),   S( 62,  50),   S( 50,  59),   S( -4,  67),   S( 60,  40),   S( 85,  41),   S( 72,  28),   S( 41,  39),
            S( 16,  61),   S( 23,  56),   S( 50,  46),   S( 32,  63),   S( 63,  49),   S( 65,  43),   S( 75,  38),   S( 75,  35),
            S( 69,  31),   S( 64,  15),   S( 35,  49),   S( 37,  27),   S( 63,  42),   S( 61,  39),   S( 49,  49),   S( 15,  44),

            /* rooks: bucket 6 */
            S(-40,  30),   S(-26,  29),   S( 18,   8),   S( -9,  14),   S(-25,  12),   S(-81,  60),   S(-49,  56),   S(-28,  45),
            S( 21,   4),   S( -9,  31),   S(-17,  41),   S(-52,  38),   S(-47,  42),   S(-45,  46),   S(-55,  45),   S( 17,  25),
            S(-49,  49),   S(-48,  65),   S(  2,  43),   S(-62,  56),   S(-16,  35),   S(-43,  65),   S(-56,  53),   S( 27,   3),
            S(-92,  85),   S( -2,  57),   S(-10,  65),   S( 10,  48),   S(-28,  58),   S(-13,  56),   S(-73,  80),   S(-45,  70),
            S( 11,  70),   S( 18,  69),   S( 72,  40),   S( 23,  44),   S( -3,  67),   S(-12,  77),   S(  9,  49),   S(-10,  48),
            S( 13,  65),   S( 72,  55),   S( 97,  35),   S( 51,  41),   S( 20,  47),   S( 25,  72),   S( 40,  68),   S( 70,  47),
            S( 50,  61),   S( 79,  43),   S(102,  30),   S( 92,  30),   S(140,  15),   S( 54,  48),   S( 84,  40),   S( 10,  68),
            S( 61,  59),   S( 75,  44),   S( 76,  39),   S( 38,  43),   S( 24,  65),   S( 92,  32),   S( 49,  58),   S( 51,  39),

            /* rooks: bucket 7 */
            S(-66,   2),   S(-34,  -6),   S(-16, -28),   S(-11,  -6),   S( -8, -12),   S(-17,  13),   S(-51,  17),   S( 22, -19),
            S(-89,  16),   S(-16,   1),   S(-35,  15),   S( 10,  -6),   S( -4,   5),   S( 33, -11),   S(-25,   7),   S(-31,   3),
            S(-110,  59),  S(-53,  20),   S(-23,  28),   S(-14,   6),   S(  2,   3),   S(-29,  16),   S(-21,  -5),   S(-18,   5),
            S(-67,  44),   S( 11,  16),   S(  7,  26),   S( 14,  28),   S( 40,   1),   S( -3,  20),   S( 44,  -6),   S( -8,  15),
            S(-42,  46),   S( -3,  26),   S( 49,  -2),   S( 40,   2),   S( 76,   0),   S(106,   6),   S( 38,  30),   S( 32,  -5),
            S(-30,  46),   S( 18,  16),   S(118, -25),   S(103, -10),   S( 67,   2),   S( 85,   8),   S( 32,  33),   S( 36,  -4),
            S( 31,  36),   S( 53,  12),   S( 53,  14),   S(102,   4),   S( 87,   3),   S(101,   2),   S( 64,  24),   S( 50,  -4),
            S(  6,  59),   S( -2,  33),   S( 80,  -4),   S(106, -23),   S( 57,  11),   S( 18,  14),   S( 72,   0),   S( 46,  -1),

            /* rooks: bucket 8 */
            S(-52, -46),   S(-20,  -7),   S(  2,  16),   S(-12,   1),   S(-39, -56),   S(-50, -50),   S(-53, -55),   S(-16,   5),
            S( -8, -20),   S(  0, -39),   S(-13,  -8),   S(  7, -16),   S(-21,   0),   S( -2, -10),   S(-10, -38),   S(-12, -82),
            S( 10,  18),   S(-16, -14),   S( 13,  22),   S(-10,  -7),   S(-24, -29),   S(-10, -33),   S(  5,  25),   S( -7,  -8),
            S( -5,  12),   S(  1,  25),   S(-15,  -2),   S( -4,  -4),   S(  2,  13),   S( -8, -10),   S( 13,  13),   S(-21, -18),
            S( -8, -28),   S( 10,  23),   S(-13,  54),   S(  2, -11),   S(-11, -19),   S( -2, -17),   S( -3,  -2),   S(-10, -40),
            S(  3,  31),   S(-16,  25),   S(  4,  35),   S( 15, -14),   S(-15, -14),   S( -8, -25),   S( -1,   0),   S(  1,  16),
            S(-14,  -9),   S(-23,  -8),   S( 24,  23),   S( -2,  -6),   S( 24,  13),   S( 10,   3),   S( 14,  10),   S(  4,   5),
            S( -3, -122),  S(  9,   4),   S(  0, -18),   S(  1,  -1),   S(  1, -22),   S(  5, -29),   S(  0,  -2),   S(  3,  24),

            /* rooks: bucket 9 */
            S(-72, -39),   S(-13, -57),   S(-32, -37),   S(-62, -23),   S(-45, -15),   S(-17, -16),   S(-12, -51),   S(-50, -54),
            S( 11, -30),   S(-13, -64),   S(-17, -27),   S(-18, -20),   S(-29, -20),   S( 13, -31),   S(  3, -36),   S(-35, -63),
            S(-15, -37),   S(  2, -25),   S(-14,  -5),   S(-31, -12),   S(-44, -26),   S( 11, -12),   S(  0, -16),   S(-13, -32),
            S(  1, -19),   S( -6, -18),   S(-15,   3),   S(-13,  -6),   S( -6, -27),   S(  1, -18),   S( 17,  17),   S( -4, -18),
            S(-18,   7),   S(-22,   0),   S( -9, -23),   S(  9,  29),   S( 16,  30),   S( 10, -11),   S(  6,   0),   S(  0, -26),
            S( 20,  17),   S( -3, -14),   S(  9,   7),   S(-34,   5),   S( 11, -18),   S( 27,   4),   S(  5,  13),   S( -4, -20),
            S( 32,   5),   S( 44, -23),   S( 25,   3),   S( 29, -13),   S( 16, -12),   S( 37,  -5),   S( 39, -22),   S( 29, -13),
            S( 42, -84),   S( 16, -47),   S( 10, -30),   S( 11,  31),   S(  5,  -2),   S( 22, -15),   S( 14,  -5),   S( 16,   2),

            /* rooks: bucket 10 */
            S(-66, -97),   S(-50, -46),   S(-33, -54),   S(-54, -19),   S(-33, -49),   S(-45, -50),   S( 30, -59),   S(  5, -56),
            S( -8, -34),   S( -2, -52),   S(-17, -46),   S(-32, -10),   S( -8, -35),   S(-26, -13),   S( 16, -18),   S(-14, -38),
            S(-38, -40),   S(-28, -37),   S(-42, -42),   S(  0,  -5),   S(-36,  -8),   S(-11,  -9),   S( 19,  -3),   S( -4,   1),
            S(  4, -18),   S(  6,  -8),   S(-25, -19),   S(  0,   5),   S( 26, -14),   S(-14, -15),   S( 19,   0),   S(-12, -19),
            S(-11,   3),   S( 12,  -5),   S(  7, -21),   S(  6, -43),   S( -9, -11),   S(  6,   9),   S( 24,  -1),   S( -7,  -5),
            S( 29,  -7),   S( 18,  20),   S(  8, -14),   S( 10, -21),   S( -6,  -8),   S( -6,  -7),   S( 18, -14),   S(  7,   6),
            S( 65,  -3),   S( 67, -28),   S( 64, -18),   S( 49, -18),   S( 11, -10),   S( 21, -19),   S( 27, -38),   S( 30, -31),
            S( 41,   3),   S(  4, -18),   S( 42,  -1),   S(  3, -18),   S( 11, -30),   S( 13, -17),   S( 23, -40),   S(  6, -29),

            /* rooks: bucket 11 */
            S(-24, -52),   S(-37, -37),   S(-35, -23),   S(-50, -65),   S(-22,   4),   S(-38, -11),   S(-42, -30),   S(-34,  -7),
            S(-50, -32),   S(  2, -36),   S(-34, -13),   S(-49, -40),   S(-31, -37),   S(-53, -29),   S(-29, -41),   S(-20,  -5),
            S(-25, -20),   S(  5, -10),   S( -6, -29),   S(-14, -23),   S(-10, -11),   S(-41,   0),   S(-31, -21),   S(-28, -41),
            S(-13,   9),   S( -3, -16),   S( -5,   6),   S( 11,  19),   S(-11, -16),   S(-26,  22),   S(  7,   1),   S(  3, -15),
            S(-14,  -6),   S( -9, -32),   S( 19,   7),   S( 36, -16),   S( 17,   4),   S( 20, -15),   S(  8, -19),   S( -8,  -4),
            S(  5,  21),   S( 14,   3),   S(  5, -19),   S( 46,   7),   S( 25,   7),   S( 23,  -5),   S(-35,  17),   S( 21,  25),
            S( 52,  21),   S( 34,   2),   S( 73, -11),   S( 58, -11),   S( 39,  -9),   S( 13,   5),   S( 26,  32),   S( 50,  -1),
            S( 38,  39),   S(  0,  -4),   S( 24,  -1),   S( 16, -22),   S(  2,  -9),   S(  3,  -6),   S( 10,  -4),   S( 14, -11),

            /* rooks: bucket 12 */
            S(  5, -39),   S(-16, -50),   S( -9, -21),   S( -5,  -5),   S(  6,  -8),   S(-11, -39),   S(-12, -49),   S(-31, -45),
            S(  8,  23),   S( -1,  -7),   S(-10,  -2),   S(  6,  13),   S( -6,  -3),   S(-12, -20),   S(  2,  -5),   S( -8, -50),
            S(  7,  11),   S(-14, -23),   S(-25, -51),   S(-11, -14),   S( -4, -25),   S(  1,   7),   S(  0,  -7),   S( -2, -21),
            S(-11, -13),   S( -2,  -3),   S( -2,  -1),   S( -7, -40),   S(  2,   4),   S(-11, -31),   S( -7, -27),   S(  1, -16),
            S(-19, -36),   S( -9, -16),   S( 10,  14),   S(  4,   8),   S( -5, -10),   S(  5, -12),   S( -8, -28),   S(  3,   2),
            S( -1, -19),   S( -3, -38),   S( 19,  22),   S(  5, -27),   S( -5, -13),   S(-10, -42),   S(  4,  -6),   S(  7,   8),
            S(-18, -20),   S( -1,  -9),   S( -7, -35),   S(  3, -26),   S(  1, -27),   S( -1, -17),   S( -2, -21),   S(  9,  10),
            S( -2, -48),   S(  8,  -1),   S(  2, -14),   S(  0, -18),   S(-11, -40),   S( -8, -23),   S(-17, -55),   S( 13,  28),

            /* rooks: bucket 13 */
            S(-14, -66),   S( -7, -34),   S(-14,  -5),   S( -2,  -2),   S( -9, -29),   S( -8, -37),   S(  3, -31),   S(-34, -59),
            S( -2,  -5),   S( -8, -19),   S(-18,   5),   S(-21, -30),   S(-10, -46),   S(  3,   3),   S(  9,  33),   S(  6,   4),
            S( -7, -20),   S(-15, -50),   S(-12, -27),   S(-10, -35),   S(  5,  45),   S(  6,  -3),   S( -5,  -7),   S( -5, -41),
            S( -1, -29),   S( -2,   0),   S(-26, -43),   S(  1,   0),   S( 14,   9),   S(-15, -25),   S( -6, -37),   S(-11, -50),
            S(  1, -20),   S(  5,  -4),   S(  6,   9),   S( -8, -17),   S( -5, -32),   S( -1,  -6),   S( -4, -43),   S( 11,   7),
            S(-25, -34),   S( -1,  11),   S(-20, -37),   S( 17,   1),   S(  9,   0),   S( 19,  38),   S(  0, -22),   S( -1, -15),
            S(-15,  -1),   S( 12,  23),   S( 10,  20),   S( -2,  -1),   S(  8, -11),   S( 14,  28),   S(  0, -26),   S( -1,   3),
            S(-14, -93),   S(-27, -76),   S(  1,  -1),   S(  0, -18),   S( -3,  -2),   S( -8, -46),   S( -9, -43),   S( 12,   4),

            /* rooks: bucket 14 */
            S(-14, -32),   S(-25, -55),   S(  4,  -4),   S( -4, -31),   S(-19,  -4),   S(  8,  13),   S(  5, -14),   S( -8, -20),
            S(-24, -62),   S(-20, -58),   S(-22, -22),   S(-28, -63),   S(-18, -42),   S( -7, -37),   S(  2,  -9),   S(  4,  -4),
            S(  1,   9),   S( -3, -14),   S(-10, -27),   S( -3, -19),   S(-11, -18),   S( -5, -10),   S(  8,  31),   S(  4,  11),
            S(  1,  17),   S(-14, -59),   S( -6, -14),   S( -9, -23),   S(  3,  -8),   S(  5,   8),   S( -8, -65),   S(-14, -27),
            S(  7, -35),   S(  0, -46),   S(  2, -56),   S( -4, -43),   S( -3, -40),   S(-14, -48),   S(  4, -39),   S(  2,   3),
            S( -8, -36),   S( -9, -39),   S(  2, -38),   S( -4, -69),   S( -3, -83),   S( -1, -63),   S( -2, -39),   S(  2, -12),
            S( 19,  17),   S( -6, -46),   S( -7, -42),   S(  3, -56),   S(  5, -83),   S(  5, -31),   S( 18, -20),   S(  7,  -8),
            S(-14, -53),   S(  9,  12),   S( -2, -24),   S(  2,  -2),   S(-15, -52),   S( -1,  14),   S( -7, -17),   S(  2,   1),

            /* rooks: bucket 15 */
            S(-14, -24),   S(-13, -53),   S( -3, -26),   S(-15, -41),   S(  8,  -6),   S( -3, -10),   S(-10, -27),   S(-15, -24),
            S( -7,  15),   S(-14, -38),   S( -3, -17),   S( -5, -11),   S( -2, -14),   S(-12, -14),   S(-12, -40),   S( -3, -21),
            S( -9, -36),   S( -9, -40),   S(-13, -63),   S( -1, -11),   S( -7, -26),   S( -9, -30),   S( 10,   7),   S(-12, -47),
            S( -8, -42),   S( -1, -18),   S( -7, -14),   S( -7, -32),   S( -8, -21),   S( -7, -37),   S(  2, -17),   S(-12, -11),
            S( -1, -22),   S(-16, -62),   S(  3, -19),   S( -2, -33),   S( -5, -35),   S(  0, -28),   S(  1, -20),   S( -5,  -1),
            S( -3, -22),   S(  7,  10),   S(  8, -25),   S(  5, -40),   S(  2, -25),   S( 22,  -5),   S(  7,  -8),   S(-10,  -2),
            S(  7,  15),   S(  4, -44),   S(  7, -22),   S(  2, -30),   S(  4, -27),   S( 21,  16),   S( 13,   5),   S( -8, -28),
            S( -4, -26),   S( -9, -31),   S( -2, -26),   S(  4, -21),   S(  5, -30),   S(  3,  -1),   S(-15, -57),   S( -2, -15),

            /* queens: bucket 0 */
            S(-14, -10),   S(-49, -71),   S( 44, -93),   S( 47, -59),   S( 43, -61),   S( 16, -21),   S( 39,  19),   S( -1,  15),
            S(-41, -23),   S( 58, -71),   S( 42, -31),   S( 39,  -6),   S( 31,  11),   S( 25,  -4),   S(  9,  34),   S( 34,  -1),
            S( -2,  13),   S( 39, -12),   S( 24,  31),   S( 27,  16),   S( 34, -21),   S( 10,  19),   S(  6,  33),   S( 27,  48),
            S(  0,  20),   S( 15,  30),   S( -9,  42),   S( -5,  52),   S( 22,  30),   S(  1,  45),   S( 11,  20),   S(  8,  24),
            S( 30,  21),   S( 39,  30),   S( 41,  -8),   S( 27,  25),   S( -5,  36),   S(-11,  -5),   S( 24,  13),   S( 19,  15),
            S( 44,  36),   S( 34,  68),   S(  2,  30),   S( 25,  25),   S( 56,   9),   S(  3,  29),   S( 20,  17),   S(  8, -11),
            S( 19,  31),   S( 67,  48),   S( 19,  57),   S( 69,  22),   S( -3,  17),   S(-11,   0),   S( 26,  -1),   S(  9,  12),
            S( 51,  56),   S( -3,  25),   S( 47,  29),   S( 65,  46),   S( 52,  49),   S(  6,  29),   S( 59,  24),   S( 73,  30),

            /* queens: bucket 1 */
            S(-16, -28),   S(-66, -25),   S(-48, -31),   S( -9, -112),  S( -3, -20),   S(-23, -30),   S( 19, -59),   S( 21,  10),
            S( -3, -39),   S( 16, -84),   S( 19, -49),   S(  8,  26),   S(  3,  13),   S(  7,   7),   S( 31, -49),   S( 20,   3),
            S(-13, -16),   S(  7, -11),   S(  8,  47),   S(  8,  25),   S(  3,  35),   S( -7,  31),   S( 18,   7),   S( 29,  55),
            S( 18, -27),   S(-32,  52),   S( -8,  43),   S( 23,  31),   S(  8,  55),   S( 18,  20),   S(  3,  -9),   S( 24,   2),
            S( 21,   4),   S( -6,  39),   S( -7,  88),   S( -6,  78),   S( -5,  90),   S( 31,  27),   S(  9, -11),   S(  1,  72),
            S( -3,  45),   S(  2,  68),   S( 34,  68),   S(-30,  76),   S( -6,  77),   S(-21,  49),   S( 23,  15),   S( 29,  27),
            S( -1,   5),   S( -1,  79),   S(-14,  13),   S(-17,  61),   S(-30,  66),   S(-19,  72),   S( 24,  33),   S(-19,  61),
            S(-11,  11),   S( 18,  13),   S( 29,  49),   S( 13,  30),   S(  9,  39),   S( 32,  23),   S( 10,  43),   S(-32,   3),

            /* queens: bucket 2 */
            S(  5,   5),   S( 28, -67),   S( 16, -33),   S(  6, -34),   S(-16, -24),   S(-55, -33),   S(-25, -28),   S( -9,  23),
            S(  5,  12),   S( 29,  36),   S( 21, -25),   S( 36, -26),   S( 20,  -9),   S(  4, -27),   S( 23, -31),   S( 32, -16),
            S(  3,  42),   S( 21,   2),   S( 15,  43),   S( 16,  23),   S(  6,  63),   S( 18,  47),   S( 16,   5),   S( 44, -13),
            S(  9,  12),   S( -7,  72),   S(  1,  49),   S(  6,  59),   S(  0,  86),   S(  9,  79),   S( 16,  19),   S( -1,  61),
            S( 17, -11),   S(-12,  52),   S(-21,  94),   S(-46, 122),   S(-25, 124),   S( -8, 100),   S(-23, 126),   S(-15, 119),
            S( -3,  55),   S( 14,  48),   S( -6,  73),   S(  6,  62),   S(-14,  97),   S(-28, 112),   S( 12,  94),   S( 12,  63),
            S(-38,  78),   S(-43, 102),   S(-28,  94),   S( -9,  80),   S(-15,  95),   S( 33,  47),   S(-57,  76),   S(-34,  73),
            S(-70,  96),   S(  4,  52),   S( 35,  29),   S( 19,  62),   S( 38,  54),   S( 42,  48),   S( 30,  39),   S(-14,  56),

            /* queens: bucket 3 */
            S( 71,  92),   S( 62,  79),   S( 49, 104),   S( 45,  83),   S( 74,  21),   S( 38,  17),   S(  5,  15),   S( 48,  48),
            S( 56, 115),   S( 65, 105),   S( 46,  98),   S( 58,  76),   S( 48,  81),   S( 58,  52),   S( 74,  -5),   S( 29,  18),
            S( 58,  94),   S( 51, 106),   S( 57,  85),   S( 58,  64),   S( 52,  88),   S( 50,  99),   S( 58, 104),   S( 59,  65),
            S( 37, 127),   S( 47,  96),   S( 48,  88),   S( 39, 106),   S( 46,  82),   S( 38, 129),   S( 54,  92),   S( 33, 142),
            S( 41, 116),   S( 51, 122),   S( 31, 115),   S( 23, 107),   S( 29, 130),   S( 23, 141),   S( 24, 174),   S( 37, 151),
            S( 43, 131),   S( 54, 116),   S( 45, 104),   S( 22, 132),   S( 26, 158),   S( 55, 118),   S( 58, 146),   S( 20, 200),
            S( 42, 133),   S( 62, 106),   S( 79,  86),   S( 48, 123),   S( 20, 137),   S( 49, 117),   S( 64, 136),   S(144,  65),
            S( 68,  85),   S( 81, 113),   S( 64, 112),   S( 72, 107),   S( 36, 105),   S( 77,  91),   S(139,  67),   S(111,  59),

            /* queens: bucket 4 */
            S(-20, -26),   S(-22, -37),   S(-19, -29),   S(-13,  -4),   S( 37, -12),   S( 51,  -8),   S(-55, -60),   S(-34,  -4),
            S(-23, -32),   S(-47, -31),   S( 24, -32),   S(-47,  18),   S(-23, -19),   S( 16, -26),   S(-32, -32),   S(-48, -21),
            S(  1,   1),   S(-24, -29),   S( 16,  55),   S(-10,  39),   S( 32,  -6),   S( 41,   7),   S( -8, -23),   S(-37, -39),
            S( -9,   2),   S( 26,  17),   S( -8,  22),   S( 11,  20),   S( 38,  30),   S( 16,  15),   S( 11, -34),   S(  5,  -3),
            S(-13,  16),   S(  7,  -2),   S( -1,  29),   S( 35,  55),   S(  7,  35),   S( -4, -12),   S(-13, -20),   S( -3, -25),
            S( 26,  30),   S( 30,  19),   S( 32,  50),   S( 36,  54),   S( 12,   6),   S(  0,  16),   S(  1,   0),   S( -3,   6),
            S(-24, -42),   S(-16,  26),   S( 15,  41),   S( 13,  45),   S(  9,  18),   S( -4,   2),   S(-11, -16),   S( -8, -12),
            S(-16, -41),   S( -1,  -7),   S( 22,  21),   S( 10,   8),   S(-22,   3),   S(  2,   8),   S(-13, -37),   S(  4, -11),

            /* queens: bucket 5 */
            S(-26,  -9),   S(-40, -43),   S(-39, -28),   S(-44, -42),   S(-60, -27),   S( 28,  -6),   S(-27, -26),   S( -6,   4),
            S(-12, -25),   S(-34,  -1),   S(-54, -42),   S(-52,  -6),   S( 24, -21),   S( 11,   2),   S(-72, -23),   S(-41, -16),
            S(-28,  13),   S(-51,   1),   S(-66, -18),   S(-17,  43),   S( 58,  50),   S(-15,  28),   S(  0,   4),   S( 19,  35),
            S(-57, -22),   S(-62,  -6),   S( 15,  46),   S(  9,  66),   S( 16,  20),   S( -8,  10),   S( -5,  -3),   S( 22,  19),
            S(-15,   0),   S( -2,  21),   S(-13,  46),   S(-23,  46),   S( 22,  54),   S(-34,   1),   S(  3,  14),   S(-48, -29),
            S(-49,  -4),   S( 14,  46),   S(-10,  43),   S( 18,  25),   S( 26,  44),   S(  5,   3),   S(  3,   8),   S( 12, -26),
            S(-22, -34),   S(-12,   7),   S( 19,  73),   S(  6,  34),   S( 35,  52),   S( 24,  47),   S( 14,   5),   S(-23, -19),
            S( -2,   3),   S( 18,  29),   S( 17,  18),   S(  7,  35),   S( 15,  28),   S(  5,  26),   S(-10, -26),   S(-17, -22),

            /* queens: bucket 6 */
            S( 14,  11),   S(-27, -10),   S(-28, -22),   S(-43, -58),   S(-81, -34),   S(-53, -46),   S(-39, -27),   S(-36, -19),
            S(-37, -12),   S(-56,  -6),   S(-41,   9),   S(-63,  53),   S(-51,   9),   S(-93,   0),   S(-93, -50),   S( 14,  21),
            S(-65, -16),   S(-19, -11),   S(-40,  -1),   S(-78,  87),   S(-46,  70),   S(-19,   5),   S(-48, -19),   S( 13,  13),
            S(-14,  27),   S( -3,  37),   S(-30,  68),   S(-30,  58),   S( -5,  78),   S(  1,  72),   S( 23,  46),   S(  2, -11),
            S(-81,   4),   S( 19,  36),   S(-32,  68),   S( 20,  48),   S( 44,  71),   S( 56,  54),   S( 22,  28),   S( 20,   1),
            S( -8,  59),   S(  8,   8),   S( 40,  42),   S( 55,  56),   S( -4,  69),   S( 55,  91),   S(-43,   0),   S( -6,   2),
            S( 13,  48),   S(-10,  52),   S(  6,  56),   S(-32,  34),   S( 33,  74),   S( 37,  74),   S(-16,  14),   S(-34,   5),
            S(  0,  32),   S( 19,  29),   S( 19,  52),   S( 12,  41),   S( 29,  54),   S( 16,  39),   S( -3,  16),   S(  0,  11),

            /* queens: bucket 7 */
            S( -1,   4),   S(-29,  23),   S(-29,   5),   S(-25,   1),   S(-46,  -9),   S(-46, -18),   S(-33, -19),   S( -8, -48),
            S(-29,   2),   S(-55,  27),   S(-36,  35),   S( -5,   7),   S(-14,  19),   S(-41,  33),   S(-49,  61),   S(-27, -17),
            S(-32,  -4),   S(-52,  51),   S( -4,  25),   S(-15,  48),   S( 33,  13),   S(-21,  60),   S(  0,   2),   S( -6,  -4),
            S(-29, -26),   S(  5,   4),   S( -7,  49),   S(-25,  51),   S( 27,  32),   S( 43,  29),   S(  7,  66),   S( 16,  17),
            S(-48,  18),   S(-65,  55),   S( -3,  27),   S( 37,  11),   S( 24,  28),   S( 73,   8),   S(  5,  27),   S( 56, -14),
            S(-20,  30),   S(-10,  26),   S(  8,   6),   S( 32,   3),   S( 59,  39),   S( 85,  27),   S( 66,  28),   S( 53,   2),
            S( 29,  -8),   S( 20,  22),   S( 35,   1),   S( 49,  18),   S( 52,   5),   S( 70,  36),   S( 65,   5),   S( 24,  17),
            S( 22,  12),   S( 45,   9),   S( 44,  21),   S( 50,  21),   S( 69,  22),   S( 10,  22),   S(  7,   4),   S( 56,  29),

            /* queens: bucket 8 */
            S(-11, -22),   S( -9, -29),   S(-10,  -6),   S(  5,   2),   S( -8,  -5),   S(-14, -30),   S(-26, -51),   S(  3,  10),
            S( -1,   4),   S(-20, -23),   S( -4,   4),   S(-25, -11),   S(-13, -21),   S(-18, -22),   S(-31, -58),   S( -8, -16),
            S(  1,   2),   S(-18, -14),   S(-24, -22),   S(-29, -50),   S( -6,   1),   S(-12, -26),   S(-13, -41),   S(-25, -47),
            S(-11,  -2),   S( 18,  25),   S(-10,   2),   S( 17,  42),   S(  7,  11),   S(-14, -14),   S(  1,  13),   S(-10, -32),
            S( 11,  24),   S(  7,  43),   S(  2,  15),   S( 14,  53),   S( 29,  47),   S(  7,  14),   S( -3,  -9),   S(-22, -37),
            S(  2,   9),   S(  0,  16),   S(-24,   4),   S(  7,  34),   S(-19, -26),   S( -1,  -3),   S( 10,  25),   S(-13, -24),
            S(-10, -15),   S(-13,  -6),   S(  6,  19),   S( -2,   2),   S(  1,   6),   S(  1,  -4),   S(-14, -20),   S(-10, -25),
            S(-24, -38),   S( 10,  12),   S(-17, -43),   S(-17, -39),   S(  0, -15),   S(-16, -33),   S( -8, -31),   S( -2,  -9),

            /* queens: bucket 9 */
            S( -1,  -1),   S(-21, -46),   S(  0,  -8),   S(-27, -31),   S(-20, -31),   S(-29, -42),   S(-15, -36),   S(-29, -48),
            S( -6,  -6),   S(-14, -25),   S(-14,  -3),   S( -7, -12),   S(-17, -16),   S(-22, -32),   S( -1, -20),   S(-15, -34),
            S( 13,  16),   S( -2,  14),   S(-14,   9),   S(-16, -19),   S(-19, -24),   S(-24, -28),   S( -4,  -1),   S(-11, -17),
            S(-13, -11),   S(-18, -18),   S(  1,  28),   S(  8,  17),   S( 10,  14),   S( -9, -18),   S(-21, -25),   S( -1, -22),
            S(  3,  12),   S( -3,  32),   S(  2,  18),   S( 22,  67),   S( -4,  12),   S( 11,  22),   S( 12,  13),   S(-12, -24),
            S(-26, -39),   S( -1,   6),   S(-14,  -6),   S( -8,  10),   S( -9, -10),   S(-10,  -7),   S(-22, -28),   S(-24, -36),
            S(-20, -28),   S( -1,   3),   S( -8,  20),   S( -2,  -9),   S(  8,   5),   S(  9,   0),   S(  2,   0),   S( -9, -13),
            S(-21, -32),   S(-20, -37),   S(  0,  10),   S(  2,   3),   S( -7, -23),   S(-18, -30),   S( -5, -12),   S( -9, -25),

            /* queens: bucket 10 */
            S( -7, -12),   S( -8,  -4),   S(-11, -26),   S(-43, -53),   S(-25, -45),   S(-23, -32),   S( -5, -16),   S(-11, -21),
            S(-23, -41),   S(-18, -28),   S(-11,  -2),   S(-21, -23),   S(-15, -21),   S(-23, -28),   S( -6, -18),   S(-23, -30),
            S(  4,  -2),   S(-18, -27),   S(-22, -33),   S( -9,   7),   S(-34, -42),   S(-23,  -7),   S( -4,   0),   S( -7, -10),
            S( -1, -16),   S(-11, -16),   S( -9,  -8),   S(  3,   8),   S( 12,  30),   S(-13,   2),   S( -5, -16),   S(-21, -27),
            S(-17, -28),   S( -7, -20),   S(-22, -13),   S( 11,  40),   S( -7, -16),   S( -2,   8),   S(  9,   0),   S(-13, -11),
            S( -1,  -2),   S(-31, -51),   S(-12,   7),   S( -1,  36),   S(  7,  23),   S( -3,  18),   S( 12,  14),   S(-16, -15),
            S( -5,  -9),   S(-16, -17),   S(  5,  10),   S(-15, -10),   S(-16, -19),   S(-11,   3),   S(-15, -27),   S( -9, -22),
            S( -2, -10),   S( -8, -17),   S(  0,  -3),   S( -2, -18),   S(  6,  10),   S( -3,  -4),   S(  9,   7),   S( -5, -12),

            /* queens: bucket 11 */
            S(-16, -32),   S(-27, -50),   S(-26, -41),   S( -8, -26),   S(-19, -19),   S( -6, -18),   S(-10, -20),   S(-14, -25),
            S( -9, -25),   S(-20, -31),   S(-53, -50),   S(  9,   0),   S(-26, -24),   S(-19, -31),   S(-17, -17),   S( -6,   5),
            S(-24, -32),   S(-19, -42),   S(-12, -31),   S(-16, -32),   S(-11,  -2),   S(  2,  10),   S( 17,  43),   S(-35, -27),
            S(-25, -43),   S(-26, -32),   S( -9, -27),   S(-10,   7),   S( 19,  23),   S(-12,   2),   S( 16,  30),   S(-10, -14),
            S(  6,  15),   S(  0, -13),   S(-19, -37),   S( 13,  34),   S( 18,  -5),   S( 18,  37),   S( 28,  51),   S(  1,  10),
            S(-32, -54),   S( -7,  -2),   S(-20, -21),   S( 13,  12),   S( 17,  16),   S( 60,  63),   S( 20,  36),   S( -6,   2),
            S(-10,  -9),   S(-15, -19),   S(  4,  12),   S(  2,   5),   S( 17,  41),   S( 19,  24),   S( 35,  41),   S( -2, -17),
            S(-15, -27),   S(-22, -56),   S(-13, -22),   S( -6,  -9),   S( -4,   5),   S(-10, -23),   S( 28,  28),   S( -7, -27),

            /* queens: bucket 12 */
            S(  5,   8),   S(  4,   2),   S(  2,  11),   S(-12, -19),   S(-11, -17),   S(  0,  -3),   S(  2,   5),   S( -5,  -7),
            S( -6, -13),   S( -8, -13),   S( -7, -14),   S(-10, -21),   S( -5, -11),   S(-10, -21),   S( -6, -19),   S(  0,  -1),
            S( -9, -15),   S(  3,   2),   S( 18,  29),   S( -1,  -5),   S(  1,  -3),   S(-13, -21),   S(-16, -35),   S(-20, -41),
            S(  9,  19),   S( -6,  -9),   S(  4,  23),   S( -2,   4),   S( 14,  18),   S(  0,  -1),   S(  5,  10),   S(  0, -13),
            S(  2,   1),   S(  8,  19),   S( 28,  48),   S( -5,   7),   S( -2,   0),   S(  1,   2),   S(-15, -31),   S( -2,  -4),
            S( -1,  -4),   S( -4,  -8),   S( 40,  72),   S(-12, -19),   S( -5,  -9),   S(  9,  15),   S(  5,  16),   S( -3, -13),
            S(  1,   6),   S(  4,   2),   S( 27,  49),   S( 20,  36),   S(  4,   4),   S(  0,   5),   S(  8,  18),   S( -2,  -7),
            S( -6, -15),   S(-14, -31),   S(-29, -31),   S(-11, -20),   S(  1,   5),   S(  0,   3),   S( -5, -12),   S(-12, -16),

            /* queens: bucket 13 */
            S( -9, -26),   S(-10, -27),   S( -9, -20),   S( -7, -11),   S(  0,   3),   S(  3,  -1),   S( -5, -17),   S(-10, -18),
            S( -1,  -3),   S( -4,  -5),   S(  6,   4),   S( -9, -18),   S(-12, -30),   S(  1,  -1),   S( -5, -17),   S(-21, -42),
            S(  1,   0),   S(  5,   7),   S( -7,  -6),   S(  1,   0),   S(-11, -22),   S(-13, -30),   S( -8, -21),   S(-11, -24),
            S( -1,   0),   S( -4, -17),   S(  9,  13),   S( 26,  27),   S( 17,  37),   S(-10, -30),   S(-10, -23),   S( -3, -13),
            S( -6,  -2),   S( -2,   4),   S( 24,  45),   S( 17,  41),   S( 24,  48),   S( -5, -17),   S(-10, -30),   S( -4, -10),
            S( -1, -10),   S(  5,  26),   S( 41,  71),   S( 15,  56),   S(-12,   1),   S( -9, -13),   S(  9,  23),   S(-14, -34),
            S( -5,  -8),   S(-12, -15),   S(  4,  12),   S( 22,  56),   S(  7,  16),   S(  2,  -1),   S(-10, -25),   S(  2,   3),
            S(-18, -37),   S(-11, -23),   S(-14, -25),   S( -6,   1),   S( 12,  11),   S( -1,  -5),   S(-12, -17),   S(-10, -18),

            /* queens: bucket 14 */
            S( -5,  -4),   S(  0,  -3),   S( -4, -11),   S( -6, -16),   S(  6,   8),   S( -6, -13),   S(  3,   0),   S( -5, -14),
            S( -7, -17),   S(  5,   9),   S( -7, -19),   S( -6, -18),   S( -9, -12),   S(-18, -45),   S(  1,   1),   S( -7, -18),
            S(  0,  -5),   S( -9, -20),   S( -5, -16),   S(-11, -22),   S( -3, -12),   S(  1,  -6),   S( -3,  -3),   S( -8, -19),
            S(  0,  -3),   S(  1,  -1),   S( -7,  -5),   S( 21,  42),   S( -2,  -6),   S(  5,  16),   S( 15,  28),   S(  3,  -2),
            S(  9,  12),   S(  1,   4),   S(-22, -13),   S( 15,  11),   S( 22,  37),   S( 10,  18),   S(  9,   8),   S( -1,  -9),
            S( -4, -10),   S(  4,   9),   S( 24,  48),   S( 10,  20),   S( 24,  65),   S(  8,  25),   S(  0, -10),   S( -7, -13),
            S(  1,   4),   S( 11,  14),   S(  5,   8),   S( 13,  21),   S( 12,   9),   S( 24,  41),   S( 15,  23),   S(  2,   2),
            S( -4, -11),   S( -5, -10),   S( -8, -15),   S( -6, -15),   S( -4,  -4),   S(  2,  -3),   S(-15, -36),   S(-21, -47),

            /* queens: bucket 15 */
            S( -5, -12),   S( -3, -11),   S( -3,  -2),   S(  4,   5),   S( -7, -13),   S(-12, -30),   S(  0,  -4),   S(  3,  -3),
            S( -1,  -9),   S( -7, -17),   S( -4, -10),   S( -4, -12),   S(  7,   5),   S( -6,  -7),   S( 17,  25),   S(  3,   1),
            S( -5, -19),   S( -2,  -9),   S( -1,   1),   S(-13, -27),   S( -4, -10),   S(  5,   8),   S( -4,  -9),   S(  8,  10),
            S( -5,  -7),   S(  4,   5),   S( -1,   0),   S(  6,   1),   S(  0,  -2),   S( -3,   3),   S( 12,  12),   S(  4,   8),
            S( -7, -16),   S( -3,  -6),   S( -6, -22),   S( -5, -19),   S(  1,   3),   S( 14,  19),   S( -2, -15),   S(  0,  -8),
            S( -6, -11),   S( -7, -17),   S(  9,  14),   S(-19, -33),   S(  1,  -3),   S( 24,  56),   S(  6,  14),   S(  0,  -5),
            S( -4,  -6),   S( -4, -15),   S(  9,  13),   S(  1,   0),   S(  9,  16),   S( 15,  24),   S( 13,  16),   S(  2,   6),
            S( -6, -13),   S( -5, -10),   S(  4,   9),   S(  9,  10),   S(  8,  12),   S(  0,  -7),   S(  2,  -2),   S( -6, -23),

            /* kings: bucket 0 */
            S(  4,  47),   S( 11,  70),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 21,  58),   S( 86,  69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-22,  24),   S(-74,  37),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 17,  39),   S( -8,  43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-43,  38),   S(-43,  29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 26,  42),   S( 28,  36),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 20,  59),   S( -6,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 59,  84),   S(  9,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -47),   S( 27, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-59,   0),   S( -4,  16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  8, -46),   S(-44, -24),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  9, -12),   S(-22, -13),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-21, -14),   S(-34, -15),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  8,  -5),   S(-20,   1),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 37,  10),   S(-11, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 25,  42),   S(-36,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-92, -48),   S(  9, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-69, -39),   S( 19, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-11, -42),   S(-29, -50),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 54, -49),   S( 33, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  3, -56),   S(-66, -32),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 74, -64),   S( 66, -55),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -4, -21),   S(-92, -46),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 95, -48),   S(  4, -59),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -56),   S( 31, -18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -97),   S( 15, -40),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -26),   S( 65, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 27, -69),   S( 22, -71),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 61, -48),   S( 38, -48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 51, -81),   S(-11, -52),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 50, -35),   S(-49, -54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  5, -59),   S(  5, -108),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -11),   S(-39,  11),   S(-10,   0),   S(-20,  12),   S(  5,   3),   S( 22,  17),   S( 30, -10),   S( 39,  -1),
            S(-14, -33),   S(-39,  -3),   S(-16,  -8),   S(-18,   1),   S(  8,   7),   S(-11,   9),   S( 19,  -4),   S( 11,  17),
            S(  6, -19),   S(  4, -24),   S( 24, -26),   S(  5, -23),   S( 16,  -1),   S( 18,  25),   S(  8,  30),   S( 48,  12),
            S( 25, -24),   S( 50,  -6),   S( 52, -20),   S( 34,  14),   S( 27,  48),   S(-12,  78),   S( 27,  73),   S( 67,  57),
            S( 98, -43),   S( 89,  23),   S( 98, -21),   S( 27,  30),   S( 53, 111),   S( 27,  94),   S(  6, 157),   S( 78, 105),
            S(-164, -92),  S(-132, -105), S( 53, -177),  S( 38,  21),   S( 68, 153),   S( 81, 161),   S(133,  79),   S( 54, 192),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  17),   S(-54,  17),   S(-38,   3),   S(-58,  47),   S(-33,  -1),   S( -7,   8),   S( -7,   1),   S( -6,  21),
            S(-51,   0),   S(-44,   2),   S(-47,   1),   S(-36,   6),   S( -9,   2),   S(-27,  -1),   S( -8,  -9),   S(-25,   6),
            S(-34,  15),   S(-11,   6),   S(-24,  -1),   S(  1, -17),   S( -6,  19),   S(-14,  13),   S(-14,  11),   S( -1,  17),
            S( -8,  24),   S( 34,   5),   S(  0,   8),   S( 18,  25),   S( 20,  18),   S(-16,  20),   S( 18,  20),   S( 39,  36),
            S( 31,  15),   S( 62, -13),   S(113, -35),   S( 85, -26),   S( 62,   8),   S(  4,  26),   S(-27,  57),   S( 38,  77),
            S(147, -42),   S( -8, -29),   S( 47, -115),  S( 44, -111),  S( -2, -66),   S(-20,  40),   S( 41, 154),   S( 84, 156),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  38),   S(-42,  19),   S(-38,   6),   S(-24,  26),   S(-54,  39),   S(-38,  18),   S(-21,  -5),   S(-31,  26),
            S(-52,  14),   S(-44,  14),   S(-45,   3),   S(-45,  22),   S(-41,  18),   S(-48,   7),   S(-25, -13),   S(-56,  12),
            S(-39,  40),   S(-29,  37),   S(-21,  21),   S(-23,  17),   S(-31,  29),   S(-23,   7),   S(-21,   2),   S(-27,  10),
            S(-10,  68),   S(-20,  54),   S( -2,  28),   S(  3,  33),   S(  0,  30),   S(-24,  13),   S( 19,  11),   S( 34,   0),
            S(-31, 119),   S(-24,  96),   S(-24,  37),   S( 31, -27),   S(123, -25),   S(106, -13),   S(100, -35),   S( 52,  -1),
            S( 25, 205),   S( 93, 105),   S(  8,  57),   S(  2, -61),   S(-65, -151),  S(-21, -148),  S( -2, -57),   S(130, -46),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13,  21),   S(-11,  13),   S(-10,  15),   S(-12,  33),   S(-27,  52),   S(  4,  39),   S( -6,  13),   S(-16,   3),
            S( -1,   8),   S(  1,  13),   S(-16,  11),   S(-19,  22),   S( -6,  28),   S( -2,  10),   S( -7,   1),   S(-36,   0),
            S( 11,  35),   S( -2,  49),   S(  4,  23),   S( -5,   8),   S( 14,   1),   S( 17,   0),   S(  3,  -8),   S( -9,  -3),
            S( 17,  80),   S(  3,  89),   S( 15,  65),   S( 15,  35),   S( 35,   7),   S( 32, -20),   S( 34,   8),   S( 43, -16),
            S(  0, 153),   S(-17, 173),   S(-29, 172),   S( 10, 109),   S( 47,  53),   S( 83, -11),   S(118, -29),   S( 98, -28),
            S( 66, 145),   S( 34, 230),   S( -2, 271),   S( -4, 194),   S( -4,  73),   S( 29, -145),  S(-85, -169),  S(-165, -171),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 55,   0),   S(  8,  -3),   S(-25,   0),   S( 18, -17),   S( -6,  -3),   S(  5, -11),   S(-28,  -3),   S(-44,  29),
            S( 25, -16),   S( 27,  -4),   S( 29, -30),   S( -7,  -5),   S(-51,  -4),   S( -2, -32),   S(-60, -13),   S(-35, -10),
            S( 81, -33),   S(113, -45),   S( 49, -31),   S(  8, -23),   S(-77,  17),   S(-16,   7),   S(-45,   9),   S(-32,  15),
            S(-11, -93),   S(  0, -105),  S( 39, -19),   S( -1,  11),   S(-41,  29),   S(-35,  43),   S(-35,  48),   S( 13,  36),
            S( 65, -102),  S(-46, -117),  S(  1, -92),   S( 80,   1),   S( 77,  90),   S( 55,  66),   S( 22,  53),   S(-12,  99),
            S( 16, -57),   S(-15, -71),   S( 10, -82),   S( 10,  12),   S( 65,  99),   S( 65, 131),   S( 28, 118),   S( 43,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-70,  35),   S(-20,  18),   S(  5,   5),   S( 31,  -6),   S( 45,  -2),   S( 17, -10),   S(-33,   7),   S(-65,  36),
            S(-54,  10),   S( -1,   5),   S(  8,  -8),   S(  5,   0),   S( -3,   0),   S( -6, -15),   S(-70,   4),   S(-92,  24),
            S(-24,  18),   S( 35,   8),   S( 80,  -3),   S( 18,  20),   S( -9,  31),   S(  0,   2),   S(-20,   7),   S(-43,  24),
            S( 27,   8),   S( 48, -18),   S(-12, -46),   S(-22,  -5),   S( -6, -14),   S(-18,   8),   S( 42,   5),   S(  0,  31),
            S( 67,   1),   S( 38, -41),   S( 73, -99),   S( -3, -54),   S( 64, -55),   S( 15,  15),   S( 10,  28),   S( 27,  75),
            S( 48,  25),   S( 48, -22),   S(  7, -98),   S( 22, -64),   S(-18, -74),   S( 48,  32),   S( 63, 120),   S( 84,  97),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-88,  23),   S(-49,   0),   S(-23,  -2),   S(-17,  15),   S(-15,  28),   S( 23,   3),   S( 14,  -1),   S( 13,  17),
            S(-72,   2),   S(-39, -10),   S(-30, -13),   S( 34,  -5),   S(-11,  14),   S( 15, -10),   S(  4,  -7),   S( -1,  -3),
            S(-40,  16),   S(-28,   8),   S(-11,   7),   S(  6,   8),   S( 22,  21),   S( 55,  -2),   S( 57,  -9),   S( 22,   2),
            S(-43,  53),   S( 13,  16),   S( 35,  -4),   S( 27,  -1),   S(-11, -30),   S( 11, -43),   S( 57, -27),   S(112, -38),
            S( 23,  73),   S( 55,  15),   S( 47,  -3),   S( 18, -43),   S(-19, -60),   S(-15, -57),   S( 62, -67),   S( 96, -23),
            S(122,  39),   S(128,  47),   S( 63,  -9),   S( 51, -55),   S( 12, -109),  S( -2, -86),   S(-17, -46),   S( 68, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-66,  -2),   S(-47, -20),   S( -2, -27),   S(-23,  34),   S( 50, -10),   S( 65, -15),   S( 58, -27),   S( 49,   1),
            S(-41, -18),   S(-48, -14),   S(-31, -33),   S(-35,   9),   S( 19, -12),   S( 48, -32),   S( 33, -20),   S( 46, -20),
            S(-34,   7),   S(-59,  21),   S(-27,   2),   S(-33,  -9),   S(  8,  -7),   S( 46, -27),   S( 79, -24),   S( 76, -23),
            S(  3,  28),   S(-27,  42),   S(-29,  47),   S( -3,  22),   S( 21, -15),   S( 77, -69),   S(  5, -60),   S( 24, -96),
            S( 30,  44),   S(-27, 109),   S( 36,  98),   S( 16,  67),   S(-13,  37),   S(  3, -67),   S(-35, -120),  S(-41, -78),
            S(120,  75),   S(109,  90),   S(108,  81),   S( 82,  90),   S( 81,  -7),   S(-10, -108),  S( 15, -86),   S(  5, -158),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -3,  -2),   S( -8,  -6),   S( 56,   0),   S(-13, -19),   S(-17, -73),   S( 42, -39),   S( 47, -56),   S( 38, -29),
            S(-26, -41),   S( -5,  -7),   S(-45, -41),   S(-59, -14),   S(-30, -48),   S( 12, -44),   S(-16, -40),   S(-20, -42),
            S(-20, -67),   S( 16, -64),   S(-25, -62),   S(-38, -45),   S(-24,  -7),   S(-26, -11),   S(-16, -22),   S(-62,   2),
            S( -8, -11),   S(-34, -26),   S( 21,  -9),   S( 20,   2),   S(-39,  -1),   S(  3,  18),   S( -2,  34),   S(-16,  34),
            S( 17,  29),   S( 10, -24),   S( 19,   6),   S( 36,  89),   S( 73, 103),   S( 47,  87),   S(  0,  99),   S(-22,  87),
            S( 37,  65),   S(  7,  54),   S( 27,  73),   S( 47,  78),   S( 43,  81),   S( 44, 138),   S( 44,  91),   S( -1,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  -2),   S( 79, -23),   S( 46, -18),   S(  1,  -4),   S(  4, -12),   S( 78, -44),   S( 70, -61),   S( 20, -30),
            S( -3, -46),   S(-57, -34),   S( -9, -45),   S(-12, -33),   S(-21, -35),   S(-29, -39),   S( -1, -43),   S( 21, -52),
            S(-69, -16),   S(-37, -53),   S(-15, -73),   S(-39, -43),   S( 29, -46),   S( -9, -46),   S(-45, -40),   S(-18, -15),
            S(-55,   6),   S(-50, -44),   S(-14, -52),   S(-43, -18),   S( 14, -40),   S(-21, -16),   S(-36,  10),   S(  3,  -3),
            S(  3,  14),   S( 21, -21),   S(  9, -17),   S( 40,   1),   S( -9,  35),   S( 27,  40),   S( -7,  74),   S(  6,  42),
            S( -7,  48),   S( 29,  28),   S( 11,  39),   S( 32,   9),   S( 45,  95),   S( 14,  58),   S( 14,  66),   S( 15,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -62),   S(-26, -43),   S(  0, -27),   S( -3, -47),   S( 47, -28),   S(170, -40),   S( 81, -19),   S( 53, -28),
            S(-69, -45),   S(-58, -59),   S( 23, -83),   S( 42, -38),   S( 13, -38),   S(  9, -37),   S( 55, -47),   S( 19, -44),
            S(-30, -44),   S(-28, -57),   S( -1, -52),   S(  4, -32),   S(  5, -51),   S(-12, -69),   S(-23, -64),   S(  1, -42),
            S(-13, -18),   S(-16, -15),   S(-16, -14),   S(  4, -47),   S(-19, -40),   S(-35, -36),   S(-17, -47),   S( -2, -29),
            S( 31,   4),   S( 31,  26),   S( 27,   4),   S( -3, -26),   S( 24, -29),   S( 22,  12),   S( -6, -17),   S( 12,  -5),
            S( -6,  12),   S(-15,  28),   S( 13,  36),   S( 27,  40),   S( 24,  37),   S( -3,   7),   S( -5,   0),   S( 18,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -52),   S(-17, -31),   S(-28, -36),   S( 12,  -7),   S( 17, -16),   S(108,  -8),   S(100, -40),   S( 84, -21),
            S(-39, -56),   S(-55, -63),   S( -2, -68),   S( -2, -49),   S(  6, -39),   S(-20, -41),   S( -7, -28),   S( 18, -59),
            S(-50, -19),   S(-17, -44),   S(  7, -24),   S( 35, -43),   S(-41, -42),   S( -1, -34),   S(-58, -77),   S( 16, -70),
            S( -9,  10),   S( -3,  13),   S( 40,  17),   S( 65,   1),   S(-18, -14),   S( 20, -40),   S(-28, -57),   S(-21, -32),
            S(-37,  25),   S( 17,  68),   S(  3,  86),   S( 45,  66),   S( 39,  56),   S(  2,  -6),   S( 11,   7),   S(  1, -17),
            S( 24,  64),   S( 22,  43),   S( 35,  80),   S( 29,  93),   S( 25,  24),   S( 44, 101),   S( 21,  44),   S( 38,  46),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -89),   S(-26, -11),   S(-16,   8),   S(  1,   5),   S(  0,  -3),   S(-18, -68),   S( 10, -21),   S( 20, -37),
            S(-16, -44),   S(-41, -11),   S(-32, -50),   S(-38, -62),   S(-23, -51),   S(-11, -53),   S( -1, -58),   S(-12, -53),
            S(-45, -18),   S(  6, -62),   S(-18, -79),   S(-29, -99),   S(  4, -38),   S(-29,  -3),   S(-53, -17),   S(-30, -49),
            S(-12, -10),   S( -1,   4),   S(-11,  -9),   S(  2,  11),   S( 17,  43),   S( -4,  80),   S(-12,  52),   S(-17,  49),
            S(  9,  11),   S(  2,   1),   S(  6,  22),   S( 13,  53),   S( 38, 126),   S( 38, 126),   S(  8, 113),   S( 17, 103),
            S( 12,  20),   S( 10,  52),   S( 12,  56),   S( 21,  83),   S( 27, 109),   S( 22,  76),   S(-32,  -2),   S(-16,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -49),   S(-32, -56),   S(  8, -16),   S( -5, -22),   S(  4, -11),   S(-19, -44),   S( -8, -67),   S(-11, -49),
            S(-48, -52),   S(-36, -65),   S(-37, -48),   S( 12,   3),   S(-34, -51),   S(-53, -43),   S(-25, -71),   S(  6, -62),
            S(-24, -32),   S(-34, -61),   S(-15, -61),   S(  4, -34),   S(-12, -40),   S(-21, -43),   S(-31, -32),   S(-31, -19),
            S(-34,  21),   S(-11,  -7),   S(  2, -13),   S(  1,  27),   S(  7,  49),   S(-24,  31),   S(-36, -12),   S(-24,  19),
            S( -6,  17),   S( -2,  13),   S( -2,  -4),   S( 24,  60),   S( 27,  87),   S( 23,  78),   S( 11,  99),   S(-21,  88),
            S( 19,  97),   S( 23,  69),   S(  4,   9),   S(  8,   7),   S( 20, 102),   S( 14,  88),   S(  8,  50),   S( -9,  76),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -79),   S(  6, -57),   S(  1, -35),   S( -4,   1),   S(-11, -36),   S(-13,  -6),   S( 12, -49),   S(-10, -35),
            S(-45, -87),   S(-86, -74),   S(-25, -84),   S(-37, -100),  S(-28, -35),   S(-19, -39),   S(-22, -35),   S(-34, -39),
            S(-29, -33),   S(-49, -57),   S(-31, -39),   S( -6, -39),   S(-29, -39),   S(-20, -75),   S(-22, -55),   S(-30, -22),
            S(  9,  11),   S(-15, -46),   S( -7,  22),   S(-20, -36),   S(  6, -22),   S(  7,  18),   S( 10,   5),   S(-11,  34),
            S( -6, -11),   S( 14,  51),   S( 12,  68),   S( -3,  15),   S( 22,  41),   S( 11,  31),   S(  9,  82),   S( 21,  74),
            S(-10,  40),   S( 17,  76),   S( 26, 122),   S( 12,  46),   S( 22,  70),   S(  0,  35),   S( 13,  84),   S( 16, 118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -41),   S(  3, -61),   S(-27, -31),   S(-12, -30),   S( -7, -29),   S(-18, -23),   S(-11, -26),   S(-16, -74),
            S(-39, -37),   S(-35, -67),   S(-31, -84),   S(-21, -71),   S(-22, -18),   S(-45,  -2),   S(-30, -12),   S(-32, -74),
            S(-34, -35),   S(-46, -39),   S(-39, -19),   S( 32, -48),   S(-36, -34),   S( -2, -28),   S(  2, -25),   S( -9, -17),
            S(-24,  30),   S(-30,  -4),   S(  4,  40),   S(  7,  43),   S( 19,  40),   S( -1,   4),   S(  2,  11),   S( -9,  21),
            S( -5,  73),   S(  3,  37),   S( 12,  82),   S( 13,  69),   S( 27, 112),   S( 19,  77),   S( 11,  55),   S( 14,  19),
            S(-19,  11),   S( -4,  18),   S( 18,  96),   S( 16,  59),   S( 20,  98),   S( 23,  95),   S( 17,  26),   S( 16,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-94, -55),   S(-13, -17),   S( -5,   1),   S(-23,  32),   S(-35,  13),   S(-43,  53),   S(-24,   6),   S(-50, -29),
            S( 14, -17),   S(-11,   2),   S(-28,  -2),   S( -3,  10),   S( -7,   2),   S(-41,  33),   S(-46, -21),   S(-25, -18),
            S(-16, -29),   S( 22, -28),   S(  6,  19),   S( 40,  39),   S( -8,  29),   S(  2,  12),   S(-30,  48),   S(-41, -36),
            S(  6,  34),   S( 23,  83),   S( 22,  41),   S( 52,  11),   S( 10,  58),   S( 11,  26),   S( 54,  -3),   S( -2, -15),
            S( 22,  77),   S( -3,  69),   S( 38,  95),   S( 35,  59),   S( 73,  40),   S( 23,  24),   S( 16,   8),   S(  4,  11),
            S(106, -45),   S(-13,  80),   S(120,  54),   S( 83,  17),   S( 60,  50),   S(-17,  80),   S( 39, -10),   S(-18,  23),
            S( 93,  18),   S(  0, -15),   S( 43,  23),   S( 90,  77),   S( 51,  49),   S( 28,  27),   S(-35,  14),   S(-52,  13),
            S(-104, -149), S( -1, -12),   S( -3,  -9),   S( 44,  44),   S( 40,  66),   S(  8,  47),   S(-16,  20),   S(-23,  -5),

            /* knights: bucket 1 */
            S( 31,  16),   S(-63,  31),   S(-41,  16),   S(-24,  22),   S(-41,  66),   S( -1, -27),   S(-46,  28),   S(-27,  -7),
            S(-43,  50),   S(-51,  68),   S(-34,  47),   S(-17,  40),   S(-24,  37),   S(-18,  44),   S(-29,  22),   S(-15, -60),
            S(-38,  26),   S( -9,   5),   S(-29,  37),   S(-22,  65),   S(-11,  35),   S(-21,  29),   S(-44,  33),   S(-30,  36),
            S(-20,  88),   S(  8,  53),   S( -5,  52),   S(-16,  67),   S( -7,  72),   S(-23,  80),   S( 11,  29),   S(-31,  65),
            S( 40,   3),   S(-11,  21),   S( 16,  70),   S( -8,  64),   S( 37,  48),   S(  9,  57),   S(-13,  52),   S(-18,  77),
            S(  1,  51),   S( 54,  16),   S( 77,  49),   S(133,  32),   S( 79,  45),   S(-40,  84),   S( 60,  12),   S( 30,  45),
            S( 14,  -7),   S(  6,  16),   S( 34, -16),   S( 38,  51),   S( 24,  29),   S( 19,  29),   S( 20,  71),   S( -9,  54),
            S(-165, -20),  S( 21,   8),   S(-24, -73),   S(-39,   1),   S(-20,   8),   S( 38,  35),   S( 22,  34),   S(-55,  19),

            /* knights: bucket 2 */
            S(-48,   8),   S(-42,  39),   S(-36,  22),   S(-26,  40),   S(-29,  42),   S(-57,  23),   S(-32,  18),   S(-16, -13),
            S(-29,  28),   S(  6,  33),   S(-33,  39),   S(-18,  36),   S(-27,  38),   S(-19,  17),   S(  7,  13),   S(-39,   8),
            S(-33,  53),   S(-26,  35),   S(-29,  40),   S(-30,  75),   S(-23,  61),   S(-28,  29),   S(-31,  31),   S(-16,  14),
            S(-14,  74),   S(-32,  68),   S(-25,  89),   S(-38, 101),   S(-35,  84),   S(-20,  62),   S(  7,  46),   S( -8,  37),
            S(-33,  86),   S(-20,  82),   S(-12,  80),   S(  0,  63),   S(-34,  92),   S( -3,  80),   S(-37,  77),   S( -3,  17),
            S(-23,  75),   S(-16,  79),   S(-40, 106),   S( 58,  32),   S( 39,  68),   S(128,   5),   S( 54,  42),   S( -9,  12),
            S( 10,  59),   S(-57,  64),   S( 51,  26),   S( 56,   0),   S(-50,  61),   S( 48, -17),   S( 19,  53),   S( 34, -13),
            S(-66,  42),   S( 13,  73),   S(-14,  74),   S( 20, -12),   S(-29, -18),   S(-48, -42),   S( 18,  11),   S(-116, -64),

            /* knights: bucket 3 */
            S(-78,  53),   S(-25, -11),   S( -5,  17),   S(-19,  38),   S( -9,  28),   S(-16,  11),   S(-30,  -1),   S(-42, -29),
            S(-21, -17),   S( -2,  39),   S( -4,  32),   S( -9,  32),   S( -8,  29),   S( 17,   9),   S( 10,  -7),   S(  5,  -6),
            S(-17,  30),   S(-19,  27),   S( -7,  49),   S(-11,  85),   S(  4,  69),   S(-11,  52),   S( -1,  39),   S( -2,  12),
            S( -5,  46),   S(  1,  57),   S(  3,  84),   S(-14, 101),   S(  8, 103),   S(  8,  93),   S( 28,  82),   S( -2,  78),
            S(-19,  83),   S(  2,  77),   S( 10,  89),   S( 17, 109),   S(  3, 101),   S(  5, 119),   S(-40, 116),   S( 31,  94),
            S(-25,  76),   S(  1,  75),   S( 28,  89),   S( -1, 116),   S( 51, 103),   S( 94, 124),   S( 32, 121),   S(  0, 130),
            S( -6,  79),   S(-21,  69),   S(-51, 120),   S( 36,  75),   S( 37,  78),   S( 78,  52),   S( 24,  12),   S( 70,   9),
            S(-175,  86),  S(-21, 119),   S(-41, 111),   S( 19,  68),   S( 49,  89),   S(-77,  89),   S(-10, -34),   S(-52, -100),

            /* knights: bucket 4 */
            S( 25,  44),   S(  0,  47),   S(-48, -12),   S(-22,  -7),   S(-27, -11),   S(-22,  -7),   S( 60, -28),   S(-20, -20),
            S( 35,  10),   S( -7, -38),   S(-13,  19),   S(-21,  13),   S(-23, -15),   S( 39, -48),   S(  5, -32),   S(-58,  -5),
            S( -3, -14),   S( -6, -14),   S( 60,  -3),   S( 70,  -5),   S( 25,  24),   S( 40, -19),   S( 14, -26),   S(-21, -41),
            S(-18, -33),   S( 26,  -5),   S( 44, -16),   S( 55,  12),   S(  8,  34),   S( 31,  18),   S(-39,  41),   S(-31,  -3),
            S( 19, -44),   S( 22, -17),   S( 77,  10),   S( 79,  55),   S( 91, -16),   S( 33,  28),   S( 29, -17),   S(-58,  28),
            S( -1, -31),   S(  5,  -6),   S( 48,   1),   S( 67,   0),   S(-11,  36),   S( 26,  37),   S(-13,   7),   S( 32,  24),
            S(-26, -41),   S(-23, -35),   S( -4, -25),   S( 10,   2),   S( 28,  25),   S( -8,   2),   S(  7,  37),   S(-57, -47),
            S( -3,   1),   S(-12, -36),   S( -2, -24),   S( 28,  22),   S( 15,  -5),   S( -1,  19),   S( -3,  12),   S(-13, -19),

            /* knights: bucket 5 */
            S( 11,  12),   S(  9,  18),   S(-32,  36),   S( -6,  16),   S( 27,  24),   S(  3,  11),   S(  8,   3),   S( 21,  32),
            S( 10,  22),   S( 53,  26),   S( 50, -13),   S(-11,  14),   S( 46,  -8),   S(  4,  -9),   S( -9,  45),   S(-28,  10),
            S(-35,  18),   S( -6,  -1),   S( 19,   7),   S( 80,   0),   S( 40,  23),   S(  4,   6),   S( 23,   3),   S(-28,   6),
            S( 35,   1),   S(  8, -12),   S( 80,  -9),   S( 70,  -3),   S( 89,  12),   S( 73, -10),   S(  6,  13),   S( 23,   8),
            S( 67,  11),   S( 42,  -4),   S( 97, -14),   S(104, -13),   S(103,  -7),   S( 58,  22),   S(-15,  16),   S( 28,  13),
            S( -6, -14),   S( 50, -26),   S(  0, -58),   S(-14,   4),   S( 34, -11),   S( 57,   6),   S(  8,  25),   S( 44,  31),
            S( 14,  25),   S(-27, -50),   S(  8, -70),   S(-12, -31),   S(-13, -47),   S(  3,   7),   S(  5,  49),   S(  0,  41),
            S(-25,  -3),   S(-27, -59),   S(  5, -20),   S(-10, -22),   S( 10,  -3),   S( -6,   6),   S( 18,  53),   S(-19,  23),

            /* knights: bucket 6 */
            S(  1,  -8),   S(-22,  32),   S(  7,  -7),   S(-15,  34),   S(  1,  28),   S( 50,   6),   S(-29,  47),   S(-14,  15),
            S(  4,   1),   S( 18,  44),   S( -9,   5),   S( 55,  -4),   S( 50,   5),   S( 15,  11),   S( 17,  40),   S(-34,  46),
            S( 28,  10),   S( 32,   3),   S( 37,  10),   S( 63,  29),   S( 38,  33),   S(-16,  22),   S( 36,  20),   S(-14,  32),
            S( 37,  28),   S( 82,   3),   S( 41,  18),   S( 72,  15),   S( 97,  -3),   S( 49,  28),   S( 42,  11),   S(  4,  39),
            S( -7,  42),   S( 78,  -6),   S(144,   1),   S(153,  -2),   S(139, -26),   S( 72,  24),   S(184, -32),   S( 26,  24),
            S( 35,  14),   S( 42,   0),   S( 63,  22),   S( 39,  14),   S( 48,  -5),   S( 61,   0),   S( 55, -12),   S( 47,  -6),
            S( -4,  25),   S(  5,  42),   S( 57,  46),   S(  2,  -9),   S( 16,  14),   S( 29, -39),   S(-25, -20),   S( 22,  38),
            S( 25,  22),   S( 13,  28),   S( 16,  29),   S(  0,   7),   S( 30, -12),   S(  9,   3),   S( -1,  30),   S(-35, -32),

            /* knights: bucket 7 */
            S(-34, -38),   S(-22, -25),   S( 16,  -4),   S(-17,  13),   S( 13, -11),   S(-22,  23),   S( -7,  -6),   S( -6,  33),
            S( -7, -31),   S(  1, -34),   S(-16,  13),   S( -9,  10),   S(  7,  10),   S( 27,  14),   S( 36,  11),   S(-56,  25),
            S(-27, -10),   S(-12, -16),   S( 27, -17),   S( 41,  15),   S( 53,  14),   S( 33,  14),   S(  0,  23),   S( 20,  26),
            S(-31, -18),   S( 45, -14),   S( 56,  -6),   S(110,  -4),   S(133, -11),   S( 88,   7),   S( 42,  13),   S( 78, -12),
            S(  6,  -4),   S(-26,  43),   S(  4,  37),   S( 83,   2),   S(166, -14),   S(183, -19),   S(200,  -7),   S( 50,  12),
            S( -7,  40),   S( 46,  11),   S(-11,  31),   S( 57,  26),   S(133,   7),   S(110,   7),   S( 36,   7),   S(-18, -60),
            S(-40,   9),   S(-15,   8),   S( 31,  21),   S( 16,  29),   S( 72,   1),   S( 31,  16),   S(-33, -25),   S(-27, -34),
            S(-32, -30),   S( 15,  18),   S( -6,  23),   S( -7,  -6),   S(  1,  -5),   S( 15,   6),   S(  0, -14),   S( -3, -14),

            /* knights: bucket 8 */
            S( -4, -15),   S( -6, -40),   S( 15,  28),   S( -5, -46),   S(  4, -22),   S( 16, -25),   S( 12,  13),   S( -5, -31),
            S( -8, -46),   S(-11, -58),   S( -7, -50),   S( -5, -33),   S(-15,  -1),   S( -3, -18),   S( 11,  13),   S(  7,  12),
            S( -3, -42),   S(-14,   0),   S(  8, -73),   S( 24,  -3),   S( -2, -39),   S( 26,  -2),   S(  2, -64),   S( -7, -24),
            S(-30, -97),   S(-18, -66),   S(  3,   3),   S( 35,   2),   S( 13, -43),   S( 12, -25),   S(-25, -38),   S(-22, -72),
            S(-13, -42),   S(  0, -28),   S( -4, -41),   S( 23, -13),   S(  7, -11),   S( -7, -28),   S(  5, -15),   S(  2, -21),
            S(  2,   1),   S( 18,  -2),   S(  3,  -1),   S(  7, -19),   S( -7,  -6),   S( -1, -36),   S( -3, -11),   S( -9, -27),
            S( -6,  -4),   S( -6, -70),   S(-15, -48),   S(  3,  -8),   S( 10, -16),   S( -6, -36),   S( -6,  -8),   S( -3,  -8),
            S(  2,   4),   S( -1,  -3),   S(-14, -40),   S( -2, -17),   S( -4,   9),   S(  0,  10),   S( -4,  -9),   S( -1,  -4),

            /* knights: bucket 9 */
            S(-18, -74),   S( -9, -42),   S(  4, -57),   S(-14, -44),   S(-14, -39),   S(-15, -24),   S(  9,   0),   S( -2, -21),
            S(-13, -44),   S(-21, -55),   S(-21, -130),  S(-10, -76),   S( -6, -56),   S(-14, -82),   S( -1, -50),   S(-20, -45),
            S( -7, -53),   S(  2, -32),   S(-10, -57),   S( -6, -80),   S(  6, -20),   S( 37, -35),   S(  2, -15),   S( -6, -34),
            S(-23, -67),   S( -5, -61),   S(  5, -46),   S( -5, -86),   S( 11, -32),   S( 29, -31),   S(-11, -77),   S(  2, -12),
            S( -4, -11),   S(-11, -37),   S( 14, -37),   S( 14, -18),   S( -6, -64),   S(  6, -28),   S( -1, -33),   S(  1, -15),
            S(-13, -46),   S( -8, -66),   S( -7, -32),   S( 15, -61),   S(  0, -28),   S(  0, -16),   S(  1, -14),   S( -5,  -2),
            S(-11, -35),   S(  5,  19),   S( -3, -17),   S(-14, -29),   S( 12, -21),   S( 15,  40),   S(-21, -26),   S( -9,  -8),
            S(  3,   9),   S( -2, -15),   S( -8,  -5),   S(  3,  14),   S( -3, -16),   S( -7,  -5),   S(  9,  30),   S(  5,  26),

            /* knights: bucket 10 */
            S(  0, -23),   S( -9, -43),   S(-16, -72),   S(-12, -42),   S(-24, -80),   S( 10, -77),   S(  7,  26),   S( -2, -19),
            S( -7, -52),   S(  7,  -3),   S(-11, -60),   S(  0, -58),   S( 13, -43),   S(-24, -97),   S(-14, -25),   S(  0,  50),
            S(-16, -40),   S(  8, -33),   S( 15, -34),   S( 32, -47),   S( -7, -63),   S( -5, -54),   S(  7, -27),   S( -3,  -5),
            S(  3, -43),   S( -2, -38),   S( 18, -33),   S( 16, -41),   S( -4, -29),   S( -2, -39),   S(-24, -87),   S( -1, -39),
            S( -4, -33),   S(-12, -36),   S( 12, -34),   S( -3, -39),   S( 11, -47),   S(  4, -79),   S(  2, -19),   S(  4, -30),
            S( -6, -19),   S(-15, -37),   S( -5, -36),   S(  7,  -3),   S(-13, -38),   S(-23, -45),   S( -9, -38),   S(-16, -22),
            S(  8,   4),   S( -3, -10),   S( -2, -29),   S(  4, -60),   S( -8, -38),   S(-15, -68),   S(-12, -25),   S(-13, -25),
            S(  1,  -5),   S( -8, -42),   S( -5,   5),   S( -2, -17),   S( -4, -28),   S(-12, -46),   S(  3,  11),   S( -4, -12),

            /* knights: bucket 11 */
            S(-10, -45),   S(-24, -63),   S(-17, -51),   S( 10, -17),   S(-52, -78),   S( -8, -12),   S(-12,   5),   S(  9,  19),
            S(  4,  -5),   S(-33, -101),  S( -6, -57),   S( 34, -25),   S(  4, -11),   S(  1, -40),   S( -4, -21),   S( -1, -18),
            S( -7, -51),   S( -5, -12),   S( -1, -22),   S( 16, -17),   S( 19,  17),   S( 16, -26),   S(-25, -99),   S(-14, -45),
            S(-24, -43),   S( 23, -19),   S( 13, -17),   S( 38,  -5),   S( 28, -21),   S( 13, -42),   S(  2, -48),   S(  0, -12),
            S(  3,   9),   S(  1, -62),   S(  1,  -9),   S( 18, -24),   S( 54,  26),   S( 21,  17),   S(-12, -106),  S( -2,   2),
            S( -9, -31),   S( -5, -81),   S( 16,  -1),   S( 33, -26),   S( 19,   9),   S(-12, -49),   S(-12, -49),   S(  0,  -4),
            S( -5, -19),   S(  1,  28),   S(  2,  -7),   S( 11,   7),   S( 20,  -5),   S( 15, -43),   S(  1, -37),   S( -9, -18),
            S( -2, -22),   S(  1,   0),   S( -7, -33),   S( -9,  -2),   S( -1,  -4),   S( -1, -19),   S(  0,  -1),   S( -2,  -4),

            /* knights: bucket 12 */
            S(-21, -67),   S( -3, -48),   S( -1, -20),   S( -5,  -7),   S( -8,  -9),   S( -3, -16),   S( -4,  -9),   S( -2,  -7),
            S( -6, -34),   S(  1,   4),   S( -9, -22),   S(  0, -28),   S( -7, -39),   S(  4,   4),   S(  4,  -4),   S(  3,   4),
            S( -2, -11),   S(-15, -68),   S( -4, -27),   S(-11, -78),   S(-11, -52),   S(  8,  12),   S(  3,  -9),   S( -5,  -8),
            S(  0,   2),   S( -7, -76),   S( -4, -21),   S( 10,  -6),   S( -4, -78),   S(  9,   3),   S(  0,  -1),   S(  5,  22),
            S( -4, -17),   S( -9, -45),   S( -6, -37),   S(  5, -10),   S(  7,   5),   S( -2, -19),   S( -4, -18),   S(-11, -37),
            S( -7, -16),   S(  5,  -5),   S( -3, -21),   S(  4, -36),   S(-11, -40),   S(  3,  24),   S(  5,  -8),   S(  2,   4),
            S( -7, -24),   S( -4,  -8),   S( -8,  -7),   S(  0,  12),   S( -1,  -1),   S(-15, -33),   S(  4,   1),   S( -4,  -9),
            S(  0,   0),   S(  2,  20),   S(  2,   2),   S( -2, -14),   S(  6,  15),   S(  1,  13),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S(  0,   4),   S( -2,  -3),   S( -7, -28),   S(  3, -17),   S(  0,  -7),   S( -7, -19),   S( -7, -17),   S(  2,  10),
            S(  1,  -1),   S( -6, -24),   S(  2,  -3),   S(  3, -21),   S( -9, -43),   S(  3, -14),   S(  3,   1),   S(-11, -42),
            S(  1,  -3),   S(  5,  11),   S(  3, -15),   S(  1, -33),   S(  1,   1),   S( -5, -23),   S(  3, -11),   S(  0,  -9),
            S( -1, -11),   S( -2, -10),   S( -8, -19),   S( -6, -37),   S( -8, -73),   S( 11,   3),   S( 11,  21),   S(  7,  10),
            S(-11,  -9),   S( -5, -38),   S(  1, -36),   S(  3,  -3),   S(-27, -79),   S( -3, -30),   S( -2, -31),   S( -5, -12),
            S( -1,   0),   S( -2,   6),   S(  4,  25),   S(  3, -32),   S(  0, -12),   S(  3, -27),   S(  7,  14),   S(  1,   3),
            S(  3,   4),   S(  6,  17),   S( -5,  -6),   S( -1,  -3),   S(  5,  32),   S( -5, -10),   S(  5,   7),   S( -1,  -7),
            S(  0,   2),   S( -1,   3),   S(  1,   4),   S( -2,   0),   S( -4, -11),   S(  2,   4),   S(  1,   4),   S(  0,   3),

            /* knights: bucket 14 */
            S(  1,   2),   S(  1,   1),   S(  7,  23),   S( -5, -21),   S( -8, -40),   S( -3,  -1),   S( -1,  -2),   S(  1,   1),
            S( -7, -26),   S(-13, -57),   S(  0, -25),   S( -4, -29),   S( -2, -34),   S(  7,   0),   S( -8, -25),   S(  5,  28),
            S( -7, -27),   S( -4, -26),   S( 14,   9),   S(-14, -66),   S( -3, -18),   S(  0,   3),   S( -4, -20),   S(  1,  -2),
            S( -1,  -9),   S( -5, -34),   S(-13, -46),   S(  4,  -2),   S(  5,   4),   S( -2, -31),   S( 10,  26),   S( -7,  -9),
            S( 15,  28),   S(-19, -56),   S( -1, -39),   S( -9, -39),   S(  0,  19),   S(  5,  14),   S(  1,   2),   S( -2, -11),
            S( -4, -10),   S( 12,  39),   S(  4,  51),   S(  7,  26),   S( -2, -25),   S( -5,  -9),   S( -2,  11),   S( -4, -23),
            S(  1,   2),   S(-11, -37),   S(  1,  -8),   S( 10,  62),   S(  3,  27),   S( -4, -16),   S(  2,   9),   S( -1,  -4),
            S(  0,   0),   S(  1,   5),   S(  0,  -2),   S(  2,  12),   S( -3, -13),   S( -1,  -4),   S(  3,   7),   S(  0,   3),

            /* knights: bucket 15 */
            S( -7, -34),   S( -5, -16),   S(  7,  36),   S( -5,  -5),   S( -6, -33),   S( -7, -41),   S( -9, -70),   S( -5, -35),
            S(  2,   6),   S(  2,   7),   S( -5, -30),   S( 13,  30),   S(  6,  -8),   S(-11, -54),   S( -5, -16),   S(  4,   5),
            S(  0,  -9),   S( -6, -32),   S( -1, -35),   S( 15, -10),   S(-16, -88),   S( -2, -25),   S( -2, -29),   S( -5, -23),
            S(  0, -10),   S( -5, -20),   S(  0,  -6),   S(  4,   4),   S( -1, -69),   S( -3,  -7),   S( -1, -21),   S( -1,   0),
            S(  3,  14),   S( 12,  29),   S(  2,  -7),   S(-10,  -7),   S(  8,   1),   S(  5,   0),   S( 10,   3),   S(  5,  18),
            S( -1,   3),   S( -6,  -5),   S(  3,  14),   S(-10, -19),   S(-11, -40),   S(  3,  20),   S(  3,  13),   S(  5,  24),
            S( -1,  -1),   S( -7, -19),   S(  6,  23),   S(  8,  23),   S(  5,  30),   S(  9,  22),   S( -2,  -4),   S(  2,   2),
            S(  1,   1),   S( -1,  -6),   S(  0,   0),   S( -1,  -3),   S(  3,  16),   S(  1,   3),   S(  1,  13),   S(  0,   2),

            /* bishops: bucket 0 */
            S( 52, -52),   S(-49,  36),   S( -6,  -8),   S( -4, -15),   S( 17, -16),   S( 13,  -9),   S( 94, -81),   S( 24,  -2),
            S(-30,  21),   S( 18, -28),   S( -9,  27),   S(  9,  11),   S( 19,   3),   S( 81, -34),   S( 52,  17),   S( 48, -30),
            S(  8,  15),   S( 13,   5),   S( 21,   9),   S( 20,   0),   S( 51, -13),   S( 44,  26),   S( 49, -11),   S( 37, -32),
            S( 13, -35),   S( 63, -45),   S( 34,  -6),   S( 75, -33),   S( 88,  10),   S( 53,  24),   S( 22, -19),   S( 27,  -3),
            S( 40,   1),   S( 46, -34),   S( 88, -20),   S( 99,  16),   S(126, -26),   S( 34,  22),   S( 47,  12),   S(  8,  -1),
            S( 54,  23),   S(108,   3),   S( 79,   6),   S( 64,   8),   S( 17,  16),   S( 70,   9),   S( 66,   8),   S(  0,  -3),
            S(-56, -91),   S( 60,  33),   S( 97,  54),   S( 11,   5),   S( 27, -32),   S( 28,  25),   S( 26,  10),   S(  8,  20),
            S(-16, -45),   S( 11,  17),   S(  6, -35),   S(-33, -34),   S(  1, -15),   S( -5,   9),   S(-12, -15),   S(-22, -25),

            /* bishops: bucket 1 */
            S(-21, -30),   S( 23, -40),   S(-24,  23),   S(  7, -10),   S( -3,  -5),   S(  8,   5),   S( 22,  -1),   S( 42, -39),
            S( 27, -56),   S( -4, -16),   S( 14, -23),   S(-17,   7),   S( 38, -17),   S( 19, -20),   S( 56, -14),   S(-13, -21),
            S(-16,  -2),   S( 36, -19),   S( -7, -10),   S( 21,  -6),   S(  6, -14),   S( 44, -19),   S(  9, -17),   S( 62,  -9),
            S( 22, -23),   S( 30, -21),   S( 25, -10),   S(  3,   9),   S( 50,  -7),   S(  7,  -4),   S( 63, -22),   S(  5, -18),
            S(  7, -20),   S( 71, -26),   S( 12,  -1),   S( 99, -18),   S( 75, -16),   S( 88, -38),   S( 11,   6),   S( 34,  -8),
            S( 49, -28),   S( 84, -38),   S( 65, -13),   S( 57, -28),   S(135, -37),   S(-12,   2),   S( 15,  19),   S(-32,  -5),
            S( -1, -75),   S( 20, -14),   S(  6, -31),   S( 10,   7),   S( 61,   6),   S( 15,  -9),   S( 30, -17),   S(-50,  37),
            S( -8, -26),   S(-62,  -8),   S( -8, -56),   S(-56,  -3),   S( 13, -26),   S( 30, -15),   S( 29, -18),   S(-24, -47),

            /* bishops: bucket 2 */
            S( 20, -33),   S(  1, -30),   S(  4,   3),   S(-15,   7),   S( 19,   1),   S(-19,   9),   S(  6, -17),   S(  3,  -7),
            S( 18, -39),   S( 19, -12),   S( -8,  -1),   S( 17,  -1),   S( -8,  17),   S( 11,  -1),   S(  7, -15),   S( 23, -67),
            S( 35,  -6),   S( 21,  -9),   S(  7,   7),   S( -3,  -2),   S(  3,  17),   S( -4,  -9),   S(-10, -22),   S( -4, -11),
            S(  1, -17),   S( 36,  -7),   S( -1,  13),   S( 41,  19),   S(  3,  16),   S(-11,  24),   S(-19,   3),   S(  7, -15),
            S(  5,  -3),   S(  6,  14),   S( 68,  -8),   S( 29,  14),   S( 29,  28),   S(  7,  20),   S(  8,  28),   S( 36, -32),
            S(-38,  19),   S(  5,  28),   S(  2,  -2),   S( 99, -33),   S( 75, -21),   S(128,   0),   S( 55,  10),   S( 23, -49),
            S( -5,  33),   S( 22,  -8),   S( -1,  11),   S( 24,  -4),   S(-76, -14),   S(-52,   7),   S(-28,  32),   S(-31, -30),
            S(-65, -33),   S(-31,  15),   S(-11,   6),   S(-30,  23),   S(-36, -35),   S(-30,  16),   S(  9, -50),   S(-34, -41),

            /* bishops: bucket 3 */
            S( 28,  21),   S( 52, -29),   S(  9,  -8),   S( 12,  10),   S( 15,  28),   S( -6,  43),   S(-23,  66),   S(  6,   3),
            S( 40, -11),   S( 24,   3),   S( 17,  17),   S( 16,  21),   S( 21,  25),   S( 13,  28),   S( 13,  16),   S( 29, -20),
            S( -5,  19),   S( 31,  44),   S( 18,  44),   S( 22,  35),   S( 15,  42),   S( 16,  36),   S(  9,  14),   S( 10,  40),
            S(-23,  17),   S( 11,  37),   S( 26,  52),   S( 38,  49),   S( 38,  35),   S( 15,  28),   S( 32,  -1),   S( 34, -31),
            S(  9,  14),   S( 11,  54),   S( 10,  59),   S( 51,  59),   S( 50,  61),   S( 41,  33),   S( 12,  47),   S(-18,  40),
            S(  7,  19),   S( 31,  49),   S( 40,  25),   S( 17,  48),   S( 44,  42),   S( 45,  82),   S( 47,  60),   S( 13, 105),
            S(-20,  69),   S( 46,  35),   S( 24,  22),   S(  9,  34),   S( 14,  48),   S( 29,  77),   S(-28,   8),   S( 24, -14),
            S(-35,  37),   S(-31,  39),   S(-44,  34),   S(-39,  70),   S(  4,  40),   S(-79,  57),   S(-18,  13),   S( 28,  21),

            /* bishops: bucket 4 */
            S(-40,  -4),   S(-24, -11),   S(-45,  15),   S(-41, -13),   S(-39, -22),   S(-20, -13),   S(-29, -42),   S(-13, -47),
            S(-33,  31),   S(-12, -10),   S( 40, -22),   S(-17, -10),   S(-27,  -2),   S( -2, -24),   S( -4, -17),   S(-40, -29),
            S( 27,   6),   S(-22, -10),   S( 23, -26),   S(  0, -13),   S(  9, -19),   S(-26,  -3),   S(-13, -28),   S(-64, -11),
            S( 17, -25),   S( 37, -39),   S( 63, -22),   S( 61,  -6),   S(  2,  -2),   S( 60, -10),   S(-29,   3),   S( -7, -13),
            S( 34,  31),   S(-10, -51),   S( 39, -41),   S( 51, -21),   S(  5,  10),   S( 67, -10),   S( 11,  24),   S(-50, -22),
            S(-47, -69),   S(-15, -42),   S( 36, -10),   S( 14, -30),   S( -6, -17),   S( 21,  25),   S( 13,  -2),   S(  4,  11),
            S(  5,   1),   S(-14, -33),   S( 37, -28),   S(-11, -31),   S( -3,   4),   S( 17,   9),   S(-19,  -3),   S( 20,  57),
            S( 11,  24),   S(  9, -45),   S( -4, -43),   S(  9, -49),   S( -6, -25),   S(-27, -16),   S( -7,  34),   S( 15,  23),

            /* bishops: bucket 5 */
            S(-36,  12),   S(  5, -25),   S(-52,  15),   S(-42,  10),   S( -9, -13),   S(-31, -14),   S(-31,   0),   S(-47, -42),
            S(-36, -19),   S(-58,  18),   S( 60, -31),   S(  1,  -7),   S(-39,  11),   S(-16,  -1),   S(-40,   4),   S( -5, -16),
            S( 22, -10),   S(-34,  -1),   S( 28, -11),   S(-24,  -6),   S(-15,   5),   S(-75,  11),   S( 24, -12),   S(-28,  16),
            S( 32, -18),   S( 12,  -2),   S( 25, -12),   S( 65,  -6),   S(-39,  20),   S( 74, -17),   S(-72,  14),   S(-23,  13),
            S( 22, -42),   S( -4,  -5),   S(  1, -21),   S( 13, -30),   S( 40, -12),   S(  9,  -9),   S( 26,   0),   S(-59,   9),
            S(-14, -23),   S(  2,  -8),   S( 42, -32),   S(-15, -20),   S(-15,  -6),   S( 19,  -3),   S( 13,   4),   S(-55,  32),
            S(-64, -26),   S(-14, -21),   S(-16, -18),   S( -8,   7),   S( -2, -28),   S( -3,  -8),   S( 30,  22),   S(-31,   2),
            S(-11, -11),   S(-17, -32),   S( -5, -32),   S(-10, -16),   S(-37,   0),   S( -7,  11),   S(-21, -17),   S(  2,  40),

            /* bishops: bucket 6 */
            S(-59,  -8),   S(-35,   7),   S(-50,  18),   S(  7,  -2),   S(-45,  17),   S(  2,  -9),   S(-72,  11),   S(-67,  15),
            S(-28, -30),   S(-17, -27),   S(-69,  25),   S( -4,   6),   S(-29,  10),   S(-43,  10),   S(-63,  26),   S(-35,  -4),
            S( -1,  -6),   S(-51,   7),   S( 12, -20),   S(-31,  14),   S(-12,  19),   S( 14,  -9),   S(-32,  -6),   S(-40,  24),
            S( 12, -14),   S(-26,  -3),   S(-25,  18),   S( 73,   0),   S( 44,   5),   S( 26,   8),   S( 13, -18),   S(-10,   4),
            S(-56,   6),   S(-16,   8),   S( 48, -16),   S( 76, -11),   S(-13, -25),   S(  3, -18),   S( 52, -21),   S( 17, -20),
            S(-17,  12),   S( -2,  14),   S(-38,  11),   S( 30,  -8),   S(  8,   6),   S(  2, -18),   S( -9,  10),   S(  1, -24),
            S( -5,   8),   S(-52,  28),   S( -3, -11),   S(  1,  -9),   S(-33,  -1),   S( -6,  -7),   S( -3, -10),   S(-50,  -4),
            S(-35,  18),   S(-32,   7),   S(-29,  -2),   S(-10,   7),   S(-30,  13),   S( 21, -27),   S(-46,   7),   S( -6, -28),

            /* bishops: bucket 7 */
            S(  4, -30),   S(-45, -38),   S(-32, -35),   S( -1, -18),   S(-40,  -9),   S(-40, -13),   S(-59, -32),   S(-51,   8),
            S(-15, -42),   S(  5, -31),   S( 38, -31),   S(  5, -34),   S(-17,  -4),   S(-33,   0),   S(-12, -36),   S( -2,   4),
            S(-31, -44),   S( -2, -27),   S(-16,  -5),   S( 50, -23),   S( 13, -23),   S( -6, -16),   S(-76,  -3),   S(-65,  34),
            S(-26, -17),   S(-48,   7),   S(-31,  19),   S( 36, -14),   S(108,  -1),   S( 25, -20),   S( 13, -38),   S(-27, -14),
            S(  4, -27),   S(  8,   6),   S( 23, -20),   S( 35, -11),   S( 94, -33),   S( 48, -15),   S(-29, -24),   S(-18, -15),
            S(-75,  -6),   S(  0,  13),   S( 12,   5),   S(-47,   6),   S(-14,   5),   S( 44, -25),   S( 23, -17),   S(-59, -70),
            S(-36, -13),   S(-15,  23),   S(-33,   0),   S( 47, -10),   S( -1, -17),   S( -7, -27),   S( 29, -12),   S(  1, -18),
            S(-23, -29),   S(-14,   8),   S(-25,   4),   S(-22, -22),   S(-13, -12),   S(-15, -14),   S( 26, -28),   S(-14, -28),

            /* bishops: bucket 8 */
            S( 21,  73),   S(-10, -20),   S( -9, -16),   S( -7,  28),   S( -3,  14),   S( -7, -41),   S(-24, -38),   S( -9, -34),
            S( -5,   0),   S( 16,  25),   S(-10,  19),   S( 11,  -3),   S(  6,  -3),   S(  1,  -9),   S(-22, -19),   S( -6, -25),
            S(-20, -44),   S(-16, -42),   S( 27,  59),   S( 32, -24),   S( 25,  11),   S( 17,  20),   S( -6, -17),   S(-27, -55),
            S( -7, -19),   S( 18,  59),   S(  4,  -3),   S(  6,  45),   S( 24,   0),   S( 36,  66),   S(-11,   2),   S( -2, -31),
            S(  7,  65),   S( 20,  81),   S(  4,  25),   S(  8,  18),   S( -1,  40),   S(-30,   7),   S( -3, -31),   S(  6,  34),
            S(-11, -22),   S(  7,  47),   S( -7,  23),   S( 11,  29),   S( 14,  25),   S( 15,  32),   S( -9,  16),   S( -2,  11),
            S( -4,  18),   S(-11, -33),   S( 19,  44),   S( -6,  58),   S( -2,  27),   S(  9,  36),   S(  3,  73),   S(-11,  16),
            S(  3,  16),   S(  3,  11),   S( -3,  -5),   S(  7,  42),   S( 11,  52),   S(  7,  36),   S(  0,  35),   S( 12,  85),

            /* bishops: bucket 9 */
            S(  2,  36),   S(-27,   5),   S(-20,  18),   S(-25, -17),   S(-19, -25),   S( -7, -13),   S( -2,  -1),   S(-15, -30),
            S( -7,   7),   S( -1,  23),   S( -5,  16),   S( -1, -23),   S(-28, -34),   S(  2, -47),   S(-14, -29),   S(-25, -60),
            S( -3, -23),   S( 15,  11),   S(  6, -17),   S(  2,  40),   S( -6,   5),   S( -6,  -6),   S(-10, -19),   S( -7, -22),
            S( -6,  31),   S( -9,   7),   S( -4,  15),   S( 12,  35),   S(-22,  40),   S( -9,  11),   S(  9,  18),   S(  0,  -9),
            S(  3,  32),   S( -7,  32),   S( 38,  36),   S( -1,  23),   S( 12,  36),   S( 15,  12),   S(  6,  17),   S(-18, -27),
            S( -6,  48),   S(-33, -10),   S( -4,  25),   S( -2,  18),   S(-30,  32),   S(  2,  34),   S(  9,  18),   S(  8,  15),
            S( -7,   0),   S( -9,  30),   S(-12,   2),   S(-23,   9),   S(-21,  -3),   S(-10,  23),   S( -3,  22),   S( -5,  56),
            S(  3,  33),   S(  1,   4),   S(-12,   9),   S( -1,   7),   S(-20,  21),   S( -5,  34),   S(-14,  46),   S( 16,  62),

            /* bishops: bucket 10 */
            S(-11, -22),   S(  3,  17),   S(-26, -32),   S(-29, -44),   S(-65, -21),   S(-50, -64),   S( -7,  30),   S( -5,  -5),
            S( -8, -16),   S(-25, -28),   S(-19, -19),   S(-21, -33),   S(-16, -24),   S(-34, -25),   S(-32, -35),   S( -7,  14),
            S(-24, -38),   S(-11, -43),   S(-28, -25),   S(-21, -12),   S(-10, -13),   S(  0, -21),   S(-12,  10),   S(  0, -65),
            S(-14, -15),   S( -3,   3),   S(-22, -33),   S( -8,   6),   S( -4,  10),   S( 16,  -4),   S(  7,  26),   S(-15, -12),
            S(-15, -13),   S(-55,  25),   S( -3,  -3),   S(  1,   4),   S(  6,   2),   S(  7,  32),   S(-19,  -4),   S( -8,   2),
            S(  0,  23),   S( -4,   0),   S(-34,  19),   S(-28,  10),   S(-29, -12),   S( -7,  12),   S( -5,  14),   S( -1,   7),
            S( -3,  16),   S( -8,  12),   S(-23,   0),   S(-10,  17),   S(-16,   8),   S(-19, -30),   S( -8,  -4),   S( 16,  56),
            S( -7,  43),   S( -7,  29),   S(  0,  31),   S( -9,   9),   S( -9,  30),   S(-11,   8),   S(  1,  10),   S(  4,   7),

            /* bishops: bucket 11 */
            S( 10,   0),   S( -5,  17),   S(-14,  19),   S(-31, -30),   S(-19,  -3),   S( 15, -25),   S(-30, -23),   S(-56,  -4),
            S(-14,  14),   S( 15, -11),   S(  4,   8),   S(  0, -34),   S(  5, -11),   S(-54, -11),   S(-31,  -1),   S(  4,   6),
            S(  4, -21),   S( -1,  -8),   S( 20,  -4),   S( -3, -27),   S(  4, -11),   S( 15,  11),   S(  1, -30),   S( -2, -21),
            S(  2,  18),   S(-10, -31),   S( 22,  -8),   S(-28, -26),   S( -4,  11),   S( 26,  15),   S(  2,  43),   S( -6, -26),
            S(-25,   3),   S(-19, -14),   S(-25,  13),   S(-30,  33),   S(  1,   8),   S(  7,  18),   S( -9,  23),   S( -6,  12),
            S(  0,  14),   S(  0,   9),   S( -9,  56),   S( -4,  17),   S( -7,  50),   S(-17,  26),   S( -7, -21),   S( -7,   9),
            S(-21, -12),   S( -6,  86),   S( 14,  39),   S( 13,  54),   S(  0,  24),   S( -5,   0),   S(-17, -39),   S( -2,   0),
            S( 14, 106),   S(-13,  18),   S(  9,  62),   S(  8,  43),   S( -3,  35),   S(-11, -13),   S( -7,  14),   S(  3,  12),

            /* bishops: bucket 12 */
            S( -4, -16),   S( -1, -12),   S( -4, -19),   S(  7,  43),   S( -6, -16),   S( -2,  10),   S( -1,   3),   S( -2,  -3),
            S(  4,  10),   S(  6,  29),   S(  3,  -9),   S( 11,  -6),   S(  9,  16),   S(  1,   4),   S(-11,  -3),   S( -2,  -7),
            S(  4,  27),   S(  6,  15),   S( 21,  77),   S( 26,  39),   S(  6,  43),   S(  8, -34),   S(  7,   4),   S(-11,  -7),
            S(  6,  63),   S( 11,  54),   S(  3,  32),   S( 23,  70),   S( 13,   1),   S( -9, -31),   S(  0,  17),   S(  0,   4),
            S( 13,  25),   S( 10,  51),   S(  2,  32),   S( 11,  43),   S(  3,  25),   S( 12,  54),   S(  9,   3),   S( 13,  45),
            S(  1,  13),   S(-14, -32),   S(-13, -15),   S( -4,  -7),   S( 12,  52),   S( 10,  19),   S(-11, -27),   S( -2, -31),
            S(  0,   2),   S( -1,  -2),   S( -3, -11),   S(  1,   6),   S(  4,  23),   S( 17,  66),   S( 16,  52),   S(  5,  21),
            S(  1,   9),   S(  0,  11),   S( -1,   4),   S( -4,  -9),   S(  5,   9),   S(  3,  -2),   S(  2,  42),   S(  7,  36),

            /* bishops: bucket 13 */
            S( -9,   4),   S( -6, -12),   S(-12, -39),   S(-13, -25),   S(  3,  29),   S(-14, -33),   S(-15, -34),   S( -7, -29),
            S(-13,   3),   S( -1,  -7),   S( -3,  -4),   S( 12,  73),   S(-16, -18),   S(  9,  15),   S(  4, -10),   S( -3, -11),
            S( -1,   3),   S( 20,  84),   S(  4,  25),   S( 16,  15),   S(  1,  16),   S( 21,  21),   S(-13,  -8),   S( -1,  -5),
            S( 19,  76),   S(  9,  74),   S(  0,  36),   S(-17,  -5),   S( 18,  65),   S(  0,  -9),   S( 10,  36),   S(  3,   7),
            S( 10,  60),   S(  5,  30),   S( 17,  19),   S( 14,  30),   S( -9, -10),   S(  2,  52),   S(  3,  15),   S(  2,  16),
            S( -1,  28),   S( -1,  17),   S( -4,  11),   S( 15,  40),   S( -1,  40),   S( -9, -22),   S(-10, -15),   S(  4,  24),
            S(  4,  12),   S( -4,  -8),   S( -5,  -6),   S( -7,  -1),   S( -3,  18),   S(  7,  65),   S(  5,  36),   S(  5,  48),
            S(  1,  -3),   S(  1,   7),   S( -2,  -3),   S( -7,  -7),   S( -1,  14),   S( -2,  -5),   S(  8,  46),   S(  9,  44),

            /* bishops: bucket 14 */
            S(-15, -37),   S(  0,  15),   S(  5, -11),   S( -5,  15),   S(-16, -27),   S( -4,   9),   S( -9, -44),   S( -7, -46),
            S( -2,   0),   S(  5,   0),   S( -6,   4),   S( -1,  -7),   S(  5,  32),   S( -1,  13),   S( -8, -11),   S( -4, -29),
            S( -3,  -2),   S(  6,  27),   S( -7,   8),   S( 14,  45),   S( 12,  55),   S(  4,  63),   S(  4,  76),   S(  3,  27),
            S( -3,   7),   S(  5,  17),   S(-11,  28),   S(  1,  60),   S(  0,  52),   S(  6,  28),   S( 11,  67),   S( -3,  14),
            S(  1,  15),   S( -7,  14),   S( -8,  18),   S(  1,  46),   S( -1,  14),   S( -1,  21),   S(  4,  14),   S( -5,  23),
            S(  4,  22),   S( 11,  21),   S(  0,  43),   S( -6,  29),   S(  5,  41),   S(  1,  14),   S( -2,   1),   S( 13,  37),
            S( 10,  59),   S( 11,  34),   S( 11,  65),   S(  3,   7),   S(  1,   9),   S(  2,  18),   S( -8, -46),   S(  1,  26),
            S(  8,  50),   S(  1,  36),   S(  7,  39),   S( -1,   5),   S( -9, -29),   S(  0, -11),   S(  8,  20),   S(  2,   3),

            /* bishops: bucket 15 */
            S(  0,   4),   S( -4,  -7),   S(  2,  -3),   S(-18, -46),   S( -9, -27),   S(-11, -43),   S( -7, -48),   S( -2, -20),
            S(  8,  32),   S(  3,  10),   S(  1,  -5),   S(  3,   8),   S(  2, -27),   S( -9, -22),   S( -5,  -7),   S( -7, -13),
            S( -7, -29),   S(  6,  -7),   S(  4,  44),   S( 10,  36),   S( 13,  33),   S( 10,  23),   S( 11,  52),   S(  5,  33),
            S(  2,  -1),   S(  3,  17),   S(  9,  58),   S(-17,  -7),   S( -2,  18),   S(  5,  37),   S(  8,  34),   S(  3,  27),
            S( -5,  -3),   S(-10, -30),   S(  4,  30),   S( 34,  92),   S( 13,  61),   S(  7,  25),   S(  6,  28),   S( -8, -14),
            S(  6,   8),   S( -5,   7),   S( 14,  66),   S(  0,  11),   S( 17,  50),   S(  9,  41),   S( -1,   6),   S( -2,  -8),
            S( -2,   9),   S(  6,   8),   S( -9,   6),   S( 12,  45),   S( 12,  48),   S( -3,  -7),   S(  5,  40),   S( -3,  -3),
            S(  3,  13),   S(  6,  49),   S(  7,  31),   S(  3,   1),   S(  3,   8),   S( -3,   6),   S(  2,   8),   S(  3,  12),

            /* rooks: bucket 0 */
            S(-15,  23),   S( 14,  10),   S(  5,  17),   S( -5,  38),   S(-26,  75),   S(-12,  55),   S(-40,  71),   S(-52,  58),
            S( -9,  25),   S(  3,  33),   S(-37,  45),   S( -3,  34),   S( -9,  66),   S(  5,  16),   S(-12,  22),   S(-44,  63),
            S(  8,   5),   S(  5,   0),   S(-27,  49),   S( -5,  21),   S(-30,  46),   S(-26,  41),   S( -8,  58),   S( -6,  30),
            S(-16,  27),   S( 38,  14),   S(-40,  52),   S(  4,  17),   S(  7,  65),   S(-14,  40),   S(-30,  78),   S(-31,  56),
            S( 35, -47),   S( 42,  21),   S( -4,  57),   S( 20,  36),   S(  9,  65),   S( 10,  77),   S( 65,  37),   S( -1,  65),
            S( 45,  -1),   S( 62,  58),   S( 73,  45),   S( 61,  69),   S(  5,  81),   S( 42,  57),   S(-25,  94),   S(-35,  79),
            S(  6,  42),   S( 58,  57),   S( 92,  72),   S( 45,  50),   S( 83,  71),   S( -8,  97),   S(-11,  89),   S(-33, 105),
            S( 34,  -5),   S( 22,   4),   S( 65,  75),   S( 61,  13),   S( 45,  55),   S( 97,  17),   S( 28,  49),   S( 75, -16),

            /* rooks: bucket 1 */
            S(-51,  45),   S(-28,  20),   S(  2,  20),   S(-34,  41),   S(-36,  46),   S(-34,  44),   S(-31,  59),   S(-72,  67),
            S(-65,  56),   S(-20,  -1),   S(-29,  30),   S(-36,  45),   S(-36,  32),   S(-64,  59),   S(-45,  28),   S(-32,  39),
            S(-43,  32),   S(-21,   9),   S(-35,  13),   S(-43,  36),   S(-40,  26),   S(-55,  24),   S(-53,  65),   S(-15,  36),
            S(-71,  65),   S( -9,  24),   S(-37,  43),   S(-30,  34),   S(-57,  57),   S(-94, 102),   S(-17,  52),   S(-76,  90),
            S(-62,  60),   S(-19,  18),   S(  4,  37),   S( -6,  42),   S(-18,  39),   S(-38,  87),   S(-53,  71),   S(-45,  91),
            S( 59,  34),   S( 55,   7),   S( 61,  14),   S( 28,  50),   S( 31,  27),   S( 18,  56),   S(  6,  50),   S(  1,  83),
            S( 46,  75),   S( 29,  30),   S( 62,  44),   S( 20,  76),   S( 32,  35),   S( 19,  82),   S( 34,  98),   S( 40,  98),
            S( 33,  14),   S( 11,   6),   S( -7,  -1),   S( -4,   1),   S( 65,   7),   S( 32,  26),   S( 23,  50),   S( 75,  36),

            /* rooks: bucket 2 */
            S(-63,  83),   S(-51,  74),   S(-46,  82),   S(-41,  53),   S(-28,  54),   S(-37,  49),   S(-27,  40),   S(-78,  84),
            S(-50,  79),   S(-60,  79),   S(-59,  74),   S(-53,  58),   S(-54,  65),   S(-47,  45),   S(-26,  28),   S(-58,  57),
            S(-50,  79),   S(-47,  72),   S(-61,  67),   S(-44,  62),   S(-51,  59),   S(-40,  50),   S(-30,  45),   S(-25,  51),
            S(-53, 101),   S(-54,  94),   S(-75,  99),   S(-81,  84),   S(-45,  67),   S(-37,  63),   S(-24,  42),   S(-31,  52),
            S(-54, 110),   S(-41, 100),   S(-33,  98),   S(-29,  79),   S(-34,  79),   S( 22,  58),   S(-33,  68),   S(-25,  97),
            S( 12, 102),   S( 10,  95),   S( 41,  71),   S( -3,  73),   S( 38,  42),   S( 40,  78),   S(109,  21),   S( 56,  80),
            S( 76,  86),   S( 13, 108),   S( 45,  85),   S( 68,  41),   S( 54,  23),   S( 81,  99),   S(-54, 124),   S(  1, 112),
            S( 31,  69),   S( 74,  61),   S( 59,  57),   S( 27,  34),   S(-27,  53),   S( 31,  43),   S( 11,  48),   S( 38,  53),

            /* rooks: bucket 3 */
            S(-10, 103),   S( -8, 106),   S(-12, 137),   S( -6, 125),   S(  1,  87),   S( 10,  80),   S( 27,  66),   S( -4,  42),
            S(  5,  96),   S(  0, 104),   S(-19, 126),   S( -5, 125),   S( -3,  87),   S( 15,  61),   S( 62,  26),   S( 10,  61),
            S(  6,  99),   S(-10, 113),   S(-25, 123),   S(-17, 134),   S( -4,  92),   S(  3,  74),   S( 42,  73),   S( 16,  58),
            S( -8, 135),   S( -5, 139),   S(-27, 147),   S(-12, 136),   S( -5,  99),   S(  7,  94),   S( 28,  83),   S(-19,  89),
            S( -3, 141),   S(-22, 154),   S(  8, 153),   S( 12, 147),   S(  0, 118),   S( 22,  99),   S( 37, 105),   S( 25,  89),
            S( -9, 159),   S( 15, 143),   S( 13, 154),   S( 33, 140),   S( 94,  92),   S( 97,  93),   S( 63, 115),   S(  8, 114),
            S(  7, 159),   S( -7, 170),   S( 13, 169),   S(  4, 175),   S( 31, 146),   S( 96, 107),   S(129, 168),   S(174, 118),
            S(116,  39),   S( 67,  99),   S( 48, 139),   S( 41, 129),   S( 21, 117),   S( 91,  90),   S( 65,  61),   S(132,  25),

            /* rooks: bucket 4 */
            S(-32, -13),   S(  9, -19),   S( 53, -53),   S( -4, -10),   S(-68,  22),   S(-24,  19),   S(-24, -16),   S(-86,  29),
            S(-52, -20),   S(-32,  -8),   S(-13, -23),   S(  6, -36),   S( -2,  18),   S(-20,  14),   S(-23,  16),   S(  3,   6),
            S(-27, -20),   S(-31, -23),   S(-38, -11),   S(-13, -29),   S(-51,  -2),   S(-23,  28),   S(-29,  -3),   S(-56,  12),
            S(-54, -35),   S( 21,  -9),   S( 25, -39),   S(-11, -31),   S( 30,  -9),   S(-37,  39),   S( -1,  -8),   S(-51,   7),
            S( 31, -22),   S( 40, -37),   S( 45,   3),   S( 49,   4),   S( 86, -16),   S( 51,  17),   S(  7,  15),   S( 15,  15),
            S(-21, -51),   S( 14,  21),   S( 24,   1),   S( 12,  16),   S( 48,  18),   S( 16,  21),   S( 16,  35),   S( 18,  28),
            S( -8, -27),   S( 40,   6),   S( 33,  -6),   S( 45, -14),   S( 26,   9),   S(-10,   0),   S( -1,  -7),   S( 37,  13),
            S( 13, -19),   S(  7, -16),   S( 40, -23),   S( -9,  -4),   S( 31,  -2),   S( 24,   2),   S( 13, -10),   S( -5,  25),

            /* rooks: bucket 5 */
            S(-41,  32),   S(  9,  -9),   S( 14,   0),   S( 20,   4),   S( -1,  11),   S(  9,   8),   S(-40,  52),   S(-39,  40),
            S(-16,   1),   S(-73,  -7),   S(-14, -28),   S(  9, -11),   S(-24,   3),   S(-28,   5),   S(-77,  36),   S(-43,  28),
            S(-75,  27),   S(-19, -18),   S( 35, -37),   S(-35,   1),   S(-19,   4),   S( 27, -21),   S(-59,  38),   S(-30,  -2),
            S(-36,  15),   S(-30,   4),   S( 73, -34),   S( 33,  -5),   S( 41,  -5),   S(-48,  42),   S(-34,  19),   S(  4,  18),
            S( 27,  16),   S( 18,  -2),   S( 24,  20),   S( 13, -14),   S(-10,  25),   S(109, -17),   S( 16,  28),   S( 63,  22),
            S(  7,  24),   S(  9,  11),   S( 10,  10),   S(-16,  -9),   S( 27,  20),   S( 36,  20),   S( 81,   0),   S( 38,  39),
            S( 72, -13),   S( 50, -11),   S(  3,   4),   S( 44,  11),   S( 66, -10),   S( 53, -19),   S( 70,  -4),   S( 46,   5),
            S( 36,   7),   S( 11,   5),   S( 44, -12),   S(  1,  42),   S( 41,   7),   S( 24,  17),   S( 35,  13),   S( 21,  45),

            /* rooks: bucket 6 */
            S(-66,  48),   S(-31,  22),   S(-30,  32),   S(-33,  27),   S(  2,   2),   S(  9,  -4),   S(-17,   6),   S(-33,   3),
            S(-68,  41),   S( 13,  -1),   S(-23,  14),   S(-38,  22),   S( -9,  -3),   S(-25,  -2),   S(-48,   2),   S(-18,  18),
            S(-61,  35),   S(-44,  26),   S( -2,  -9),   S(-27,  12),   S(-15,  10),   S(-30,   5),   S(-28, -15),   S(-33,  -7),
            S(-76,  53),   S(-21,  34),   S(-15,  18),   S( 26,  -4),   S( 13,  13),   S(-11,   7),   S( -9,  -4),   S(-28,  36),
            S(-31,  52),   S( 22,  26),   S( 93,   2),   S( 57,   8),   S( 33,  -8),   S( 13,  18),   S( 50,   2),   S( 84,  -9),
            S(113,   2),   S( 84,   5),   S(130,  -6),   S( 41,   4),   S(  6, -29),   S( 31,  56),   S( 30,  -4),   S( 87,  -2),
            S( 48,   3),   S( 99, -15),   S(119, -35),   S( 99, -34),   S( 48, -16),   S( 47,   0),   S( 85, -34),   S( 82, -24),
            S( 32,   1),   S(  6,  36),   S(-16,  32),   S( 40,   0),   S( 45,   5),   S( 81,  23),   S( 62,   0),   S( 67,   3),

            /* rooks: bucket 7 */
            S(-110,  41),  S(-97,  49),   S(-71,  50),   S(-73,  53),   S(-46,  15),   S(-33,   0),   S(-41,  28),   S(-52, -15),
            S(-77,  35),   S(-39,  14),   S(-61,  30),   S(-54,  35),   S(-41,   2),   S( -2, -14),   S( 18,  10),   S( 16, -50),
            S(-97,  43),   S(-76,  30),   S(-24,   3),   S(-81,  37),   S(-66,  30),   S(-45,  18),   S( 25, -12),   S(-27, -26),
            S(-83,  40),   S( -2,  12),   S( -6,   9),   S( 55,  -9),   S(-36,  25),   S( 42,  -5),   S( 17,  15),   S( 62, -30),
            S( 31,  19),   S( -9,  38),   S( 56,  15),   S( 56,  15),   S(148, -41),   S(127, -36),   S( 96,   7),   S(-24, -22),
            S(  2,  26),   S( -9,  25),   S(103,   4),   S(120, -25),   S(102,  -6),   S( 55,  14),   S( 41,  49),   S(-13,  -9),
            S(-43,  25),   S( 31,   0),   S( 55,   2),   S( 66,  -8),   S(135, -38),   S( 89, -26),   S( 65,  -1),   S( 50, -30),
            S(-67,  18),   S(-20,  35),   S( 44,  12),   S( 22,  18),   S( 33,  -2),   S( 58,  11),   S( 67,  12),   S( 12,   6),

            /* rooks: bucket 8 */
            S( 21, -73),   S( -9, -61),   S( 37, -42),   S(  6, -24),   S(-10, -78),   S(  1, -19),   S(  9, -56),   S(-19, -30),
            S(-32, -89),   S( -4, -26),   S( 13,  -6),   S(-23, -63),   S( 13, -60),   S(-13, -49),   S(  9, -25),   S(-26, -47),
            S(  9,  23),   S( -5, -19),   S( 18,  29),   S(-15,  -4),   S( -6,  44),   S( 33,  18),   S( -8,  18),   S(-10, -11),
            S(  3, -11),   S( -1, -11),   S(  1, -12),   S( 20,  13),   S( 28,  34),   S( 33,  11),   S( -6, -13),   S( -7, -20),
            S(-17, -73),   S( 31,  21),   S( 20,  -6),   S( 27,  18),   S( 12,   2),   S(-17, -23),   S( 18,  39),   S( 14,  -6),
            S(-13, -37),   S( 20, -26),   S( 18, -14),   S( -7, -14),   S(  5,  49),   S(-26,  -3),   S( -8,  13),   S( -2,   7),
            S( 19,   1),   S( 28,   0),   S( 15,  -9),   S( 24,  17),   S(  8,   0),   S( 15,  14),   S(  5,   6),   S( 10,  -2),
            S(-12,  11),   S( 40,  -5),   S( -7, -24),   S( 18,  38),   S(-11, -10),   S( 20,  47),   S( 12,  22),   S( 14,  34),

            /* rooks: bucket 9 */
            S(-12, -97),   S( 20, -97),   S(-15, -102),  S( 22, -79),   S( 21, -85),   S(  6, -70),   S( -8, -54),   S(-13, -73),
            S(-28, -80),   S(-29, -104),  S( -6, -79),   S(-13, -76),   S( -5, -76),   S(-28, -29),   S(-27, -70),   S(-16, -67),
            S(-25, -32),   S(  7, -27),   S(  8,   2),   S( 12,  -4),   S( 30, -55),   S(  1,  -9),   S( -6, -11),   S( -7,  -7),
            S( 10, -33),   S(  3,  -7),   S(  0,  -9),   S(  4,  -5),   S(-11, -83),   S( 21, -10),   S(-12, -38),   S(  6, -44),
            S( 10, -45),   S( -1, -30),   S(-10, -65),   S( -8, -36),   S( -6, -65),   S( -8, -17),   S(  9, -56),   S(-19, -42),
            S( 19, -44),   S(-16, -58),   S( -6, -28),   S( 22, -17),   S( 13, -15),   S(-15, -31),   S(-18, -28),   S( -9, -42),
            S(  8, -11),   S( 19, -38),   S( 14, -56),   S(-12,   7),   S( 17, -42),   S( 19, -17),   S(  1,  -5),   S(-15, -58),
            S(-19, -18),   S(  7, -21),   S(  9, -11),   S( -3,   0),   S( 12, -17),   S(-11,  14),   S(-14, -12),   S(  3,   2),

            /* rooks: bucket 10 */
            S( 11, -104),  S(-26, -68),   S( 20, -85),   S( 39, -115),  S( 10, -73),   S( 10, -93),   S(  8, -111),  S( 12, -80),
            S(-26, -67),   S(-23, -63),   S(-31, -54),   S(-32, -83),   S( -6, -72),   S(  6, -79),   S(  6, -62),   S(-50, -91),
            S( 10, -36),   S(-14, -32),   S(-18, -49),   S(-45, -87),   S( -4,  -8),   S( 13, -22),   S( -3, -54),   S( -5, -48),
            S(-24, -41),   S(-27, -46),   S( -6, -57),   S(-15, -30),   S(  1, -35),   S( -2, -23),   S(-15, -92),   S(  8, -68),
            S( 27, -52),   S(-16, -45),   S(  0, -53),   S(-13, -75),   S(  2, -25),   S( -4, -27),   S( -2, -69),   S( -6, -78),
            S(-19, -39),   S( -9, -17),   S(  0, -53),   S(-10, -66),   S(  2, -50),   S(-14, -50),   S(-11, -56),   S(  2, -47),
            S(-34, -35),   S(-11, -51),   S(  1, -48),   S(-14, -61),   S( 22, -35),   S(-31, -22),   S(-12, -60),   S( 13, -43),
            S(-19, -36),   S(-10,   4),   S(  6,  11),   S(-11, -20),   S( 11,  -7),   S(-27,  -9),   S( -4, -40),   S(-17, -17),

            /* rooks: bucket 11 */
            S(-58, -74),   S(-16, -52),   S(-16, -63),   S( -4, -38),   S(-13, -48),   S( 27, -67),   S(  6, -66),   S(-19, -92),
            S(-13, -48),   S(-30, -35),   S(-35, -39),   S(-42, -36),   S(-26, -22),   S( 25, -24),   S(-11, -61),   S(-20, -91),
            S(-28,   3),   S(-37,   4),   S(  7,   1),   S(  0, -10),   S(  8, -20),   S( 10, -13),   S( 12, -26),   S(-10,   2),
            S(-12, -47),   S( -4, -27),   S( -4,  -9),   S( 20,  14),   S( 28,   9),   S(-24, -56),   S( 10,  12),   S( -6, -33),
            S(-15, -35),   S( 18, -32),   S( 10, -13),   S( 15,  -7),   S( 41,  -2),   S( 10, -55),   S( 19,   6),   S( -6, -27),
            S( -1, -35),   S( -4, -21),   S(  8, -33),   S(  9, -18),   S(-21, -25),   S( 34, -12),   S( 28, -22),   S( -2, -32),
            S( -2,  -2),   S(-25, -42),   S( -8, -27),   S(  6, -36),   S(  8, -19),   S( 42, -21),   S( 32, -31),   S(-27, -34),
            S(  7, -17),   S( 29,   9),   S( -7,  23),   S( 22,  19),   S(-24,  -4),   S( 35,  -6),   S( 60, -14),   S( -8,   1),

            /* rooks: bucket 12 */
            S(-16, -89),   S(  0, -13),   S(  9, -22),   S(-12, -62),   S(-17, -64),   S( 14,  -9),   S(-23, -58),   S(-15, -65),
            S( -5, -16),   S(  4,   0),   S( 12,  14),   S(  3, -20),   S(  3,  -5),   S( 20,  15),   S( 11,  13),   S(-22, -64),
            S(  5,  -3),   S(  8,  24),   S(  1,  -6),   S( 23,  -1),   S(  0, -49),   S( 13,  -1),   S(  5,  16),   S(  2,  23),
            S( -6,  -9),   S( -2,  -8),   S( 16,  32),   S(  6,  -4),   S( 21,  27),   S( 10,  13),   S(  9,  22),   S(  0,  -1),
            S( 16,  15),   S(  3,  13),   S( -1, -27),   S(  1, -23),   S(  8,  10),   S(-13, -11),   S(  3,  -6),   S(  3,  10),
            S(  6,  -4),   S( -7,  -9),   S(  5, -20),   S( -9, -51),   S( -1,  -8),   S( -3,  -9),   S( -1, -15),   S(  4,   9),
            S(-23, -41),   S(-14, -42),   S( 11, -12),   S( -1, -13),   S( -2, -13),   S(  5, -16),   S( -2,   0),   S( -4, -11),
            S(  4,  -8),   S( -1, -15),   S( -1, -39),   S(  9,   5),   S( 11,  -1),   S( -2,  -2),   S(  3,   1),   S( 10,  25),

            /* rooks: bucket 13 */
            S(-30, -73),   S(-33, -68),   S(-18, -42),   S( -6, -34),   S(  0, -59),   S( -3, -34),   S(-35, -59),   S(-19, -48),
            S(-14, -47),   S( -6, -39),   S(  1,   1),   S(  0, -15),   S(  3, -11),   S(  2, -51),   S(  9, -44),   S( -3, -35),
            S(  4, -29),   S(  6, -39),   S(-10, -36),   S(  8, -26),   S( 15,  -7),   S(  3, -54),   S( -3, -32),   S( -7, -83),
            S( 12,  11),   S( -9, -47),   S( 12, -19),   S( 13,   2),   S( -3, -30),   S(-11, -30),   S(  4,   9),   S( -1,   4),
            S( 11,  -8),   S(  5, -74),   S( -2, -43),   S(  4, -24),   S( 17, -33),   S( -2, -23),   S( -7, -17),   S( -1, -16),
            S(  6,  -5),   S( -2, -41),   S(  7, -38),   S( -8, -64),   S(-12, -88),   S(  5, -20),   S( -5,  -9),   S( -8, -37),
            S(  5,  -2),   S(  2, -40),   S( -8, -46),   S(  5, -13),   S(-12, -89),   S(  1, -21),   S(  0, -15),   S( -5, -28),
            S(  0, -14),   S( -9, -44),   S( -5, -19),   S(  4, -28),   S( -5, -57),   S( -1, -22),   S( -5, -27),   S(  6,  -4),

            /* rooks: bucket 14 */
            S(  2, -50),   S(-29, -61),   S(-10, -56),   S(-12, -100),  S( -6, -68),   S( 14, -27),   S(-26, -105),  S(-36, -80),
            S( 10,  -9),   S( 17,   0),   S(  9, -14),   S( -4, -45),   S(  2, -15),   S( -6, -24),   S(-12, -62),   S(  1, -56),
            S(-13, -35),   S(  3,  -1),   S( -3, -32),   S( -2, -34),   S( 12,  -1),   S(  2,  -2),   S(  4, -29),   S( -9, -58),
            S(  1, -10),   S( 16,  18),   S(  2,  -4),   S( -3, -29),   S( -1, -21),   S( -1, -43),   S( 13,  -1),   S(-16, -54),
            S(  6,   0),   S( 14,  36),   S(  1,  -9),   S(  2, -40),   S(  6, -26),   S( 23,  -1),   S(-11, -64),   S( -4, -30),
            S(  2, -13),   S( -2,  -3),   S( -9, -55),   S(  4, -70),   S( -9, -79),   S( -2, -68),   S(-12, -84),   S(-10, -36),
            S(-12, -18),   S( -2, -22),   S(-16, -52),   S(-24, -98),   S( -6, -46),   S(  0, -39),   S(-22, -81),   S( -8, -36),
            S(-13, -46),   S( -6, -26),   S( -5, -50),   S(-13, -80),   S(-10, -59),   S(-15, -98),   S(  1, -47),   S( -1, -25),

            /* rooks: bucket 15 */
            S(-12, -59),   S(-12, -65),   S(-41, -70),   S(-18, -81),   S( -6, -45),   S(-19, -70),   S( -2, -21),   S(-17, -55),
            S(  6, -14),   S(-15, -54),   S(  6,   1),   S(  4, -13),   S( -8, -56),   S( -1, -10),   S(  2,  -4),   S(  4,   0),
            S( -2,   8),   S( -7, -37),   S(  1, -22),   S( 15,  -8),   S( 15, -28),   S(  0, -13),   S( 15,   7),   S( -5, -19),
            S(  2,  12),   S( -7, -22),   S( 17,  50),   S( -4,   6),   S( 11,  13),   S(  4,  -8),   S( 10,  -1),   S(  9, -18),
            S(  2,  -4),   S( -1, -26),   S( -1,  -6),   S( -2, -11),   S(  3,   5),   S( 19,  15),   S(  8, -25),   S( -5, -45),
            S(  5,  28),   S(  0,  -3),   S(  3,   5),   S(  4,  -4),   S( -1, -18),   S(  4, -54),   S( 12,  -9),   S(  2,   1),
            S(  1, -10),   S(-10, -28),   S(  6,  -8),   S(  3, -11),   S(  5,  10),   S( -4, -33),   S(  0, -52),   S( -6, -26),
            S( -4,  -5),   S(  1,   2),   S( -1,  -2),   S( -2,  -7),   S(  1,  -7),   S(-19, -70),   S( -2, -37),   S( -9, -39),

            /* queens: bucket 0 */
            S( -5, -23),   S(-21, -92),   S(-34, -69),   S(  0, -106),  S(  6, -97),   S(-12, -58),   S(-52, -49),   S(-16, -29),
            S(  2, -80),   S(  2, -85),   S(  3, -52),   S(-17, -49),   S(  3, -26),   S( -5, -54),   S(-28, -59),   S(-17, -66),
            S( -8,   5),   S( -3, -59),   S( 19, -46),   S(-18, -13),   S( -1, -38),   S(-12, -13),   S(-22, -26),   S(-96, -67),
            S(-31,  27),   S( -1,  26),   S(-23,  43),   S(-14,  57),   S(-13,  68),   S(-16,   8),   S(-32, -18),   S(-12, -46),
            S(-47,  15),   S(-34, 110),   S( -9,  63),   S(-25,  82),   S(  4,  79),   S(-32,  56),   S(-49,  26),   S(-37,   3),
            S( -7,  66),   S( 25,  67),   S( 34,  73),   S(-33,  86),   S(-63,  69),   S(-64,  80),   S(-86,  30),   S(-34, -23),
            S(  0,   0),   S(  0,   0),   S(  8,  17),   S(-14,  20),   S(-27,  34),   S(-66,  75),   S(-77,  67),   S(-112,  12),
            S(  0,   0),   S(  0,   0),   S( 26,  26),   S( -5,  -8),   S(-31,  41),   S(-67,  27),   S(-59,   3),   S(-69,  -8),

            /* queens: bucket 1 */
            S( 13, -34),   S(-19, -26),   S( 19, -108),  S( 26, -85),   S( 35, -68),   S( -2, -77),   S(-18, -18),   S(-26,  14),
            S(-41,  35),   S( 16,  -5),   S( 39, -51),   S( 16, -18),   S( 33, -21),   S(  3, -15),   S(-26,  -7),   S(-39,   4),
            S( 26, -15),   S( 24, -28),   S(  0,  20),   S( 11,  23),   S(-19,  52),   S( 24,  28),   S(-11,  20),   S( 21, -41),
            S( 29,   5),   S(-11,  47),   S(  4,  35),   S( 23,  84),   S(  2,  78),   S( -1,  62),   S(  1,  42),   S(-17,  46),
            S( 21,  16),   S( 13,  58),   S( 21,  97),   S( 24,  86),   S( 39,  80),   S( 44,  31),   S( -5,  75),   S( 14,  51),
            S( 54,  29),   S( 85,  52),   S(103,  70),   S(127,  84),   S( 87,  82),   S(  5, 102),   S( 34,  41),   S( -4,  11),
            S( 92,   4),   S( 74,  38),   S(  0,   0),   S(  0,   0),   S(  2,  58),   S(-29,  86),   S(-27, 115),   S(-26,  48),
            S( 72,  20),   S( 53,   2),   S(  0,   0),   S(  0,   0),   S( 28,  14),   S( 33,  64),   S( 88,  22),   S(-11,  25),

            /* queens: bucket 2 */
            S( 24, -35),   S( 27, -45),   S( 23,   3),   S( 44, -52),   S( 50, -41),   S( 29, -56),   S(  1, -54),   S( 22,   1),
            S( 16,  18),   S(  5,  10),   S( 38, -11),   S( 35,   2),   S( 52, -25),   S( 17, -19),   S( 12,  -3),   S(  4,  48),
            S( 33,  15),   S( 23,  23),   S( 17,  49),   S( 12,  42),   S( 23,  49),   S( 19,  47),   S( 22,  28),   S( 12,  39),
            S( 27,  42),   S( 19,  65),   S(  9,  99),   S( 13, 104),   S( 28,  70),   S( -7, 107),   S( 27,  69),   S( 26,  94),
            S(  2,  55),   S(  6,  18),   S(-11, 126),   S( 15, 127),   S( 16, 127),   S( 87,  43),   S( 46,  95),   S( 61,  61),
            S(-34,  94),   S(-36, 112),   S( 10, 107),   S( 66,  86),   S( 32,  92),   S( 87, 126),   S(123,  73),   S( 17, 143),
            S(-25, 101),   S(-36,  89),   S( -3,  97),   S( 73,  73),   S(  0,   0),   S(  0,   0),   S( 23, 121),   S( 49,  80),
            S(  5,  53),   S( 41,  20),   S( 57,  15),   S( 23,  69),   S(  0,   0),   S(  0,   0),   S( 74,  77),   S( 35,  90),

            /* queens: bucket 3 */
            S(-23,  45),   S(-15,  43),   S( -1,  26),   S( 11,  46),   S( -9,  28),   S(  6, -18),   S( 11, -41),   S(-34,  31),
            S(-30,  39),   S(-21,  46),   S( -1,  57),   S( -2,  71),   S(  6,  56),   S( 12,  18),   S( 33, -11),   S( 55, -65),
            S(-33,  61),   S(-20,  82),   S(-18, 113),   S(-28, 139),   S(-11, 101),   S( -8,  99),   S(  3,  63),   S( -4,  38),
            S(-19,  60),   S(-39, 118),   S(-28, 145),   S(-12, 163),   S(-23, 160),   S(-17, 119),   S(  2, 103),   S( -5,  92),
            S(-27, 108),   S(-34, 132),   S(-32, 152),   S(-22, 197),   S(-15, 173),   S(-30, 210),   S(-25, 168),   S(-24, 148),
            S(-43,  96),   S(-38, 132),   S(-59, 190),   S(-49, 193),   S(-38, 200),   S( 25, 177),   S(-14, 197),   S(-11, 169),
            S(-77, 130),   S(-90, 155),   S(-74, 197),   S(-88, 209),   S(-71, 214),   S( 36, 122),   S(  0,   0),   S(  0,   0),
            S(-119, 181),  S(-85, 139),   S(-66, 129),   S(-64, 151),   S(-37, 156),   S( -4, 121),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S( -5,  44),   S(-62, -31),   S(  2,  -2),   S(-37, -25),   S(  3,   1),   S( -9, -12),   S(-59, -35),   S(  5,  -1),
            S( -2, -14),   S( 14,   5),   S(-42,  -1),   S(  6, -21),   S(-70,  21),   S(-14,  33),   S(-51, -34),   S(-13, -14),
            S( -6,  46),   S( 25, -14),   S( 16,  -3),   S( 15,  -5),   S( 22, -10),   S(-25,   5),   S( -8, -39),   S( 42,  18),
            S( 16,   5),   S( 38,  12),   S(-13,   5),   S( 32,  27),   S( 50,  22),   S(-24,  15),   S(-38, -16),   S(  6,  19),
            S(  0,   0),   S(  0,   0),   S( 39,  29),   S( 70,  67),   S(  7,  36),   S( 12,  30),   S(-14, -19),   S( -8,   5),
            S(  0,   0),   S(  0,   0),   S( 37,  25),   S( 45,  72),   S( 19,  41),   S( 13,  28),   S(  4,  34),   S(-40, -26),
            S( 20,   5),   S( 33,  47),   S( 50,  26),   S( 44,  33),   S( 36,  16),   S( 11,  10),   S(  7, -18),   S(-72, -11),
            S( 46,  16),   S( 21,  15),   S( 47,  30),   S( 64,  39),   S( 14,   3),   S(  2, -11),   S(-16, -26),   S( -9, -17),

            /* queens: bucket 5 */
            S( 10,  -1),   S( 16,  -9),   S(-11,  25),   S(-47,  18),   S( 36,  -5),   S( 14,  37),   S( 19,   0),   S( -2, -14),
            S(-16, -16),   S(  7,  15),   S(  4, -11),   S( -6, -11),   S(  7,  22),   S(-59, -44),   S( 11, -16),   S( 20,   8),
            S( 46,  27),   S( 14,  -4),   S( 27,  32),   S( 13,  11),   S(  3,  27),   S( 25,  32),   S( 17,  50),   S(-13,  12),
            S(  3, -19),   S( 66,  43),   S( 48,   6),   S( 31,  54),   S( 69,  41),   S( 19,  24),   S( 42,  57),   S(  1,  30),
            S( 42,   8),   S( 65,  22),   S(  0,   0),   S(  0,   0),   S( 43,  55),   S( 11,  10),   S( 49,  40),   S(-30,   2),
            S( 49,  18),   S( 49,  53),   S(  0,   0),   S(  0,   0),   S( 33,  38),   S( 58,  37),   S( 29,  14),   S( 52,  33),
            S( 76,  42),   S( 85,  21),   S( 59,  68),   S( 12,  29),   S( 52,  45),   S( 68,  70),   S( 73,  25),   S( -8,   4),
            S( 15,  12),   S( 38,  18),   S( 66,  44),   S( 61,  45),   S( 49,  37),   S( 68,  56),   S( 42,  37),   S( 41,  22),

            /* queens: bucket 6 */
            S( 29,  42),   S(-28, -27),   S( 27, -20),   S( 13,   9),   S(-20, -14),   S(-28,  13),   S(-24, -25),   S( 10,   5),
            S( 37,  16),   S( 60,  -5),   S( 39,  25),   S( 29,  17),   S( 19,   3),   S(-10,  12),   S(-36,   5),   S(  2,  20),
            S(-31,  36),   S( 13,  48),   S(  7,  40),   S( -2,   8),   S( 38,  -9),   S( 20,   4),   S( 43,  27),   S( 63,  40),
            S(-14,  14),   S(-20,  40),   S( 25,  31),   S( 86,  56),   S( 32, -24),   S( 70,  44),   S( 83,  37),   S( 84,  23),
            S( -3,  -4),   S( 29,  56),   S( 24,  59),   S( 71,  53),   S(  0,   0),   S(  0,   0),   S( 72,  50),   S(111,  42),
            S(  9,  27),   S( 72,  31),   S( 59,  73),   S( 43,  24),   S(  0,   0),   S(  0,   0),   S(112,  79),   S(115,  25),
            S( 26,  -2),   S( -9,  26),   S( 76,   8),   S( 68,  49),   S( 63,  72),   S( 71,  65),   S(129,  40),   S(145,  13),
            S( 34,  24),   S( 35,  -7),   S( 57,  19),   S( 84,  49),   S(134,  39),   S(113,  45),   S(117,  33),   S(101,  40),

            /* queens: bucket 7 */
            S(-26,  -8),   S(-17,  -7),   S(-39,   7),   S(-25, -17),   S(-10, -13),   S(-66,  16),   S(-43, -10),   S(-32, -32),
            S(-46, -10),   S(-86,   3),   S(-27,  20),   S(-33,  49),   S(-15,  16),   S(-10,  21),   S(-17,   9),   S(-49,   1),
            S(-38, -13),   S(-38,  25),   S( -9,  31),   S(  7,  35),   S( 44,  -2),   S( 20,   9),   S( 11, -14),   S( 51, -15),
            S(-35,   3),   S(-36, -22),   S(  3,  48),   S( 16,  43),   S( 96,  15),   S( 72,  24),   S( 81, -24),   S( 61,  -9),
            S(-18,  20),   S(-37,   5),   S( 11,  68),   S(  6,  74),   S( 38,  38),   S( 73,  22),   S(  0,   0),   S(  0,   0),
            S(-36,   8),   S(-16,  33),   S(  1,  56),   S(-19,  41),   S( 62,   5),   S(125,  66),   S(  0,   0),   S(  0,   0),
            S(-50,  30),   S(-33,  -6),   S( 24,   3),   S( 44,  21),   S( 57,  48),   S( 86,  29),   S( 53,  17),   S( 69,  36),
            S( -5, -15),   S(  3, -24),   S( 58,  26),   S( 65, -13),   S( 61,  41),   S( 50,   7),   S(  2,  11),   S( 87,  12),

            /* queens: bucket 8 */
            S(-17, -36),   S( -4, -26),   S( -1, -23),   S( -7, -18),   S(-13, -35),   S( 15,   3),   S(  3, -10),   S(  3,   1),
            S( -7, -15),   S( -5, -11),   S( 26,  19),   S( 17,  20),   S( 25,  27),   S( -1,   0),   S( -4,   4),   S(  5,   6),
            S(  0,   0),   S(  0,   0),   S( -2, -15),   S( -9, -31),   S(  8,  -2),   S( 14,  18),   S( -1,  -9),   S(  5,   7),
            S(  0,   0),   S(  0,   0),   S( 10,  17),   S( -9, -16),   S( 25,  44),   S(-12, -29),   S( 21,  49),   S(  1,  -4),
            S(  9,  11),   S( 10,  16),   S(  3,  -9),   S( 28,  -5),   S( -7, -16),   S( 10,  -6),   S( 16,  12),   S(-13, -16),
            S(  1,  -6),   S(  4,  -5),   S( 22,  24),   S( -2,  -9),   S(  3,   0),   S( 13,   8),   S( -2, -18),   S(-10, -24),
            S(-20, -60),   S(  4,  -7),   S( 30,  36),   S( 27,  35),   S(  9,  -8),   S( 11,  18),   S( -1, -17),   S(  2,  -2),
            S( 12,   4),   S( 17,  23),   S( 11,   8),   S(  5,   4),   S( 10,  12),   S(-26, -40),   S(-24, -45),   S(-36, -61),

            /* queens: bucket 9 */
            S( 18,  10),   S(-12, -53),   S(  6,   1),   S( 34,  19),   S( 16,  13),   S(  3, -26),   S(-12, -17),   S( -4, -17),
            S(  4, -21),   S(  1,  -7),   S( -8, -22),   S( 20,  28),   S(-14, -53),   S( 17,  17),   S( -2,  -6),   S( -4, -11),
            S(-14, -31),   S( -6, -17),   S(  0,   0),   S(  0,   0),   S( 12,  18),   S( 25,  14),   S( -1,  -2),   S( 13,   7),
            S(  1, -14),   S(-11, -30),   S(  0,   0),   S(  0,   0),   S( -8, -16),   S( 23,  16),   S( 20,  21),   S( -1,   6),
            S(  6,  -4),   S( 11,   7),   S(  9,  22),   S(-17, -24),   S(-13, -33),   S( 24,  20),   S(  9, -27),   S( -4,  -7),
            S( 23,  12),   S( 11,   6),   S( 11,  -1),   S( 22,  12),   S(  4, -17),   S(-15, -17),   S( -5,  -7),   S(-14, -43),
            S(  7,  17),   S( 25,   7),   S(  2,   7),   S(-10,  -6),   S( 35,  34),   S( 39,  38),   S( -2,   4),   S( 13,  -5),
            S(-12, -25),   S(  8, -15),   S(-26, -40),   S(  6,  -5),   S( -7, -15),   S(  7,   5),   S( 27,   6),   S(  4, -27),

            /* queens: bucket 10 */
            S( 10,  10),   S( 18,   2),   S( -1,   4),   S(  5,  -1),   S( -2, -25),   S(  9,   3),   S(  3,   0),   S(  7,  -4),
            S(  7,   2),   S(-20, -49),   S( 17,  -6),   S(  2, -12),   S( -8, -19),   S( 10, -17),   S(-13, -42),   S( -9, -21),
            S( -4,  -5),   S(  5,  12),   S(  3,  -1),   S( -6, -27),   S(  0,   0),   S(  0,   0),   S( 12,  17),   S(  4,  -2),
            S( -2, -11),   S( 16,   6),   S( 14,   9),   S( 16,  30),   S(  0,   0),   S(  0,   0),   S( -8, -25),   S(-13, -23),
            S( 13,  19),   S( 24,  21),   S( -4, -20),   S( 32,  41),   S( 11,  11),   S( -2,  -4),   S(  2, -17),   S( 28,  -2),
            S(-10, -23),   S( -8, -24),   S( 27,  29),   S( 26,  37),   S( 13,   3),   S(  9,   9),   S(  5, -16),   S(  2, -37),
            S(  4,  -5),   S( 12,  26),   S( 25,  38),   S( 23,  29),   S( 13,   6),   S( 11,  10),   S(  8,   2),   S( 11,   0),
            S(-13, -34),   S(  2,   3),   S( 20,   4),   S(-13, -43),   S( 27,  29),   S( -4,  -6),   S( -5, -19),   S(  4, -26),

            /* queens: bucket 11 */
            S( -5,   2),   S( -1,  -8),   S( -9, -32),   S(-23, -48),   S( -8,   3),   S(-17, -44),   S( -1, -18),   S(  3,  -9),
            S(  2,  10),   S(-11,  -9),   S(-41, -49),   S( 16,   1),   S( 34,  13),   S( 17,  11),   S( 14,  -5),   S(  3, -17),
            S(  1,   8),   S(  8, -10),   S(-14,  18),   S(-10, -33),   S(  2,  -8),   S(-14, -28),   S(  0,   0),   S(  0,   0),
            S( -4,   1),   S(-17,  -7),   S(-13,  -6),   S(-11, -25),   S( -5, -18),   S( -5, -10),   S(  0,   0),   S(  0,   0),
            S( -8,  -4),   S(  7,   6),   S(  4,   1),   S( -2, -24),   S( 73,  92),   S( 14,  12),   S(  9,   7),   S(-10, -26),
            S(-12, -17),   S(  0, -11),   S(-24, -39),   S(  4, -12),   S( 26,  41),   S( 14,   5),   S(  7,  -1),   S( 26,   6),
            S(-15, -26),   S(  9,  14),   S( 12,  11),   S(  5,  16),   S( 12,  38),   S(  1,  -2),   S(-22, -26),   S( 29,  32),
            S(-35, -82),   S(  5, -20),   S(-27, -25),   S( -2, -24),   S( 16,  27),   S(  4,  -2),   S(-10,  -1),   S( 11,   2),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  7,  10),   S( -1, -11),   S(  0,  13),   S(-12, -27),   S(  2,  -6),   S( -3,  -3),
            S(  0,   0),   S(  0,   0),   S(  5,   8),   S( -2, -12),   S( -4, -14),   S(-11, -31),   S( -8, -16),   S( -2,  -8),
            S( -8, -13),   S(  9,   7),   S( -8, -19),   S( -1, -24),   S( 32,  58),   S(  4,   1),   S( -2, -10),   S(  5,  14),
            S(  4, -10),   S(  9,   7),   S(  2,   0),   S(  7,  17),   S( 10,  13),   S(  8,   9),   S( -4, -11),   S(-12, -21),
            S(-20, -36),   S(  6,   5),   S(-11, -26),   S(-21, -59),   S(  0,   4),   S( -2, -17),   S(-15, -40),   S( -1,  -2),
            S(  0, -12),   S( -1,  -3),   S(  2,  -2),   S(-14, -30),   S(-14, -32),   S(-14,  -9),   S(-18, -26),   S( -2,  -4),
            S(-12, -19),   S(  2,   5),   S( -6, -15),   S(  1,   4),   S(  0,  -2),   S(-14, -35),   S(  0,   3),   S(-11, -38),
            S(  6,   9),   S(  0,  -3),   S(  7,  18),   S(  3,   4),   S( -9, -21),   S( -5, -17),   S( -5, -12),   S(-15, -34),

            /* queens: bucket 13 */
            S(-14, -38),   S(-11, -23),   S(  0,   0),   S(  0,   0),   S(-18, -38),   S( -1,  -6),   S(-10, -36),   S( -6, -12),
            S( -3, -22),   S( -6, -11),   S(  0,   0),   S(  0,   0),   S(-11, -19),   S(-25, -55),   S(-11, -35),   S( -2, -12),
            S( -6, -22),   S(-12, -26),   S( -4,  -7),   S( 12,  19),   S( -7, -18),   S( -9, -12),   S( -7, -13),   S(  0,   0),
            S( -3, -16),   S(-12, -40),   S( 11,   8),   S(  5,  -6),   S( 15,  27),   S( 21,  27),   S(-13, -39),   S( -6, -19),
            S(  7,   3),   S( -5, -19),   S(  0, -10),   S( 13,  25),   S(-15, -32),   S( -1, -12),   S( -4, -15),   S(  2,   6),
            S(  6,   3),   S(-17, -41),   S( -3,  -6),   S(  5,   3),   S(  8,   3),   S( -9, -19),   S(-20, -37),   S(-12, -43),
            S( -6, -18),   S(  0,  -4),   S( 14,  27),   S( -8, -20),   S(-11, -29),   S( 12,   3),   S(-14, -31),   S( -6, -20),
            S(-11, -36),   S(  6,   1),   S( -5, -11),   S(  3,   4),   S( 12,  25),   S(-10, -29),   S( -4, -15),   S(-14, -21),

            /* queens: bucket 14 */
            S( -9, -26),   S( 19,  17),   S(  4,  -6),   S( -3, -17),   S(  0,   0),   S(  0,   0),   S(  8,   5),   S(-11, -29),
            S(-12, -21),   S(-17, -41),   S( -8, -26),   S( -5, -21),   S(  0,   0),   S(  0,   0),   S(  1,   0),   S(-16, -28),
            S( -3,  -8),   S( -2, -27),   S(  0, -16),   S( -1,  -5),   S(  3,   6),   S( -2,  -8),   S( -8, -22),   S(-22, -54),
            S( -4,  -9),   S( -5,  -9),   S( -6, -15),   S(-26, -47),   S( -1,  -2),   S(-19, -45),   S( -4, -18),   S( -2,  -8),
            S( -9, -13),   S( -6, -16),   S(-11, -33),   S(  4,  12),   S( 22,  34),   S( -9, -19),   S(  9,  -1),   S( -9, -28),
            S(-13, -27),   S( -4,  -8),   S(-11, -25),   S(-21, -44),   S(  2,   1),   S( -1,  -4),   S(  5,   9),   S(-18, -29),
            S(-14, -27),   S( -5, -14),   S(  4,   6),   S( -3, -10),   S(  0,  -1),   S(  4,  -2),   S(-16, -34),   S( -6, -10),
            S(-23, -50),   S(  7,   6),   S(-18, -24),   S( -4, -11),   S( -9, -18),   S( -9, -16),   S(-10, -23),   S(  0,  -7),

            /* queens: bucket 15 */
            S( -1,  -7),   S( -9, -25),   S(  7,   3),   S(-24, -43),   S(  2,  -7),   S(-12, -28),   S(  0,   0),   S(  0,   0),
            S( -6, -20),   S(  0,  -8),   S( -5,  -8),   S( -8, -18),   S(  4,  11),   S(  2,   5),   S(  0,   0),   S(  0,   0),
            S( -7, -16),   S(  7,  12),   S( -4,   4),   S( 11,  24),   S(-18, -44),   S(  4,  -1),   S( -6, -17),   S(  1,  -1),
            S( -5, -10),   S(-10, -21),   S( -5, -17),   S( -2,  -5),   S(  7,  14),   S(  0,  -6),   S(  0,  -9),   S( -9, -24),
            S( -2,  -3),   S(  2,  -3),   S(-17, -39),   S( -8,  -7),   S( -6,   8),   S(  1,   0),   S( -4,  -6),   S( -9, -27),
            S(  3,   3),   S(-11, -26),   S( -8, -16),   S(-21, -39),   S(  0,   8),   S(-13, -28),   S(  9,  12),   S(-11, -23),
            S( -9, -20),   S( -4,  -9),   S( -7,   3),   S(  8,   4),   S( -8, -17),   S( -2,   0),   S(  7,  16),   S( -5,  -8),
            S( -6, -17),   S(-28, -60),   S(  2,  -3),   S( -6,  -6),   S(-22, -30),   S( -8, -25),   S(  3,   8),   S( -6, -15),

            /* kings: bucket 0 */
            S(  1, -27),   S( 24,  -4),   S( 10, -15),   S(-24,   5),   S(-31,  13),   S( 21, -29),   S(  6,   3),   S( 21, -56),
            S(-12,  11),   S(-12,  20),   S( -4,  13),   S(-50,  20),   S(-41,  36),   S(-11,  15),   S( -7,  38),   S(-27,  43),
            S( -1,  -3),   S( 48, -17),   S( -5,   5),   S( -1, -14),   S(-40,   4),   S(-24,   1),   S(-42,  23),   S( 17, -14),
            S( 12, -38),   S(  1,  -4),   S( -2, -18),   S(-21,  11),   S(-53,  24),   S(-37,   6),   S(-61,  29),   S(-15,  11),
            S(-40, -96),   S( 17, -25),   S( 14, -37),   S( 37, -15),   S( -6, -15),   S(-61,  17),   S(-12,   8),   S( -4, -27),
            S( -5, -109),  S( 30, -25),   S( 41, -68),   S(  0, -26),   S( 17,  -6),   S( 31, -45),   S( 20, -13),   S(  7,  -5),
            S(  0,   0),   S(  0,   0),   S(  2, -26),   S( 28, -31),   S(  9, -11),   S(  6,  -7),   S(-13, -36),   S(-12,   9),
            S(  0,   0),   S(  0,   0),   S( -8, -52),   S( 10, -51),   S( 13, -35),   S( 16,  17),   S( 25,  44),   S( 10,   6),

            /* kings: bucket 1 */
            S( 38, -34),   S( 34, -22),   S( 13, -18),   S(  8,   7),   S(-15,   3),   S( 21, -14),   S( 14,  12),   S( 39, -33),
            S(-24,  14),   S( 18,  17),   S( 19, -14),   S(-43,  26),   S(-30,  26),   S(  0,   8),   S( -2,  31),   S(-16,  22),
            S( -6, -21),   S( 18, -15),   S( 38, -25),   S(-13,  -9),   S(-36,  -3),   S(  7, -16),   S( 12,   1),   S( 73, -27),
            S( -3, -10),   S( 27, -15),   S( -2,  -5),   S(-17,  15),   S(  0,  19),   S(-24,  10),   S(-17,   8),   S(-34,  21),
            S( -5, -24),   S( 23, -27),   S( 15, -49),   S(-15, -12),   S( 24, -22),   S(-18, -20),   S(  0,  -3),   S( -1,  -1),
            S( 14,  -5),   S( 23, -48),   S( 26, -10),   S( 41,  -6),   S(  3,  -4),   S( 24,  -5),   S( 36,  15),   S( -1,  19),
            S( -2, -47),   S(  6,  10),   S(  0,   0),   S(  0,   0),   S( -5,  21),   S(  1,  30),   S( -1,  50),   S(-27, -52),
            S(-13, -105),  S( -5,   0),   S(  0,   0),   S(  0,   0),   S(  1, -39),   S(  4, -36),   S( -2,  20),   S( -6, -42),

            /* kings: bucket 2 */
            S( 26, -55),   S( 15,  -1),   S( 11, -21),   S( 28, -19),   S(-10,   8),   S( 40, -26),   S(  6,  26),   S( 34, -20),
            S( 31, -26),   S(  1,  34),   S( -3,   4),   S(-10,  14),   S(-25,  16),   S(-12,   6),   S( 19,   7),   S( -6,  11),
            S(-25, -14),   S(-20,   1),   S(-12,  -8),   S( -5, -10),   S(-11,   0),   S( 10, -23),   S( 36, -17),   S( 26, -16),
            S(  7,  10),   S(-47,  26),   S(  2,   5),   S(-23,  17),   S( 16,   2),   S(-28,   2),   S( 17, -15),   S( 40, -15),
            S(-14,  -8),   S( 13,   4),   S(-15, -15),   S( 12, -14),   S( 28, -38),   S(-21, -34),   S( 39, -31),   S( 25, -46),
            S( -6, -25),   S(  2,  -8),   S( 20, -21),   S( 38, -24),   S( 62, -20),   S( 34,  10),   S( 57, -27),   S( -2, -24),
            S(-11,   2),   S( -7,  14),   S(-35,  -7),   S( 25,  -1),   S(  0,   0),   S(  0,   0),   S( 26,  27),   S(-21, -41),
            S( -7, -34),   S(-15, -21),   S(  6, -32),   S(  2,  -5),   S(  0,   0),   S(  0,   0),   S(  7,   3),   S(-10, -126),

            /* kings: bucket 3 */
            S( 22, -73),   S( 13, -10),   S( 17, -31),   S( -9, -16),   S(-16, -19),   S( 33, -34),   S(  2,  17),   S( 10, -29),
            S(-14,  20),   S(-28,  46),   S(-17,   7),   S(-35,  10),   S(-51,  22),   S(  0,  -5),   S( -7,  16),   S(-13,  11),
            S( 18, -43),   S( 24, -12),   S(  7, -19),   S(-18, -12),   S(  0,  -2),   S( 26, -32),   S( 35, -16),   S( 58, -28),
            S(-44,  43),   S(-112,  35),  S(-84,  24),   S(-101,  28),  S(-70,  18),   S(-64,  -2),   S(-65,   4),   S(-73, -13),
            S(-32,  14),   S(-29, -14),   S(-21, -11),   S(-31,   4),   S( -7, -30),   S( 24, -52),   S(  1, -50),   S(-46, -57),
            S(-31, -17),   S( 28, -12),   S(-20, -32),   S(-41, -19),   S( 17, -28),   S( 76, -58),   S(125, -49),   S( 64, -112),
            S(-26,   2),   S( 15, -13),   S( -7, -34),   S( 23, -32),   S( 26, -33),   S( 48, -31),   S(  0,   0),   S(  0,   0),
            S(-18, -40),   S( 10, -22),   S( -1,   6),   S(  2, -10),   S( 17, -88),   S(  5, -30),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-27, -13),   S(  6,  30),   S( -3,  32),   S(  1,  10),   S( 43, -24),   S(-10,   0),   S(  0,  29),   S( 48, -38),
            S(-47,  24),   S(  7,  21),   S(-14,  17),   S(-22,   2),   S( 56, -15),   S( -3,  15),   S( 44,  -9),   S( 22,  -4),
            S( -7,  19),   S(-21,   5),   S( -7,   5),   S(  9,  -8),   S( -1,  10),   S( 23, -30),   S(-22, -11),   S(-34,  11),
            S( 12, -31),   S( 15,   6),   S( 28,  -2),   S(-11,  13),   S(-10,  -2),   S( -8,  19),   S( 14,  21),   S(  7,  13),
            S(  0,   0),   S(  0,   0),   S(  1, -11),   S( -3,   5),   S(-12,   4),   S(-30,  -7),   S(-34,  -7),   S(  3,  23),
            S(  0,   0),   S(  0,   0),   S(  4,  19),   S( 12,  -3),   S( 12,  14),   S(-14, -15),   S( -4, -33),   S(  1,  13),
            S( -2, -20),   S(  8,  37),   S(  4, -37),   S( 10,   2),   S(  2,   4),   S(-14,  -6),   S( 16,  23),   S( -3, -13),
            S( -3,  25),   S(  4,   0),   S( -1,  -7),   S(  5,   9),   S(  1, -25),   S(  2,  11),   S(-12, -29),   S( -1,   4),

            /* kings: bucket 5 */
            S( 10,  -2),   S(-23,  12),   S(-13,  10),   S(-37,  19),   S(-15,  13),   S( -6,   1),   S( 68,  -9),   S( 47,  -5),
            S(  0,   9),   S( 42,  -3),   S( 35,  -8),   S(  7,   4),   S( 29,  -6),   S( 28,  -9),   S( 39,   4),   S( 43, -14),
            S( 16,   1),   S(-44,  12),   S(-25,  -4),   S(-22,  -6),   S(  5,  -3),   S(-61,   4),   S(-22,   9),   S( 19,  -8),
            S(-35,  -2),   S( 44, -18),   S( 37, -12),   S( 10,  21),   S( 37,   3),   S( 11,  -1),   S( 19,  13),   S(-16,   3),
            S(-14, -16),   S( -4, -31),   S(  0,   0),   S(  0,   0),   S(-14,  15),   S(  7, -12),   S( -1,   0),   S(-25,  -3),
            S(-25, -15),   S(-26,   5),   S(  0,   0),   S(  0,   0),   S(-22,  17),   S(-31,  -3),   S( -7,  15),   S(-20, -10),
            S(-11, -19),   S( 12,  33),   S(  9,  55),   S( -4, -10),   S( -6,   8),   S(  0,  20),   S(  4,   8),   S( 13,   5),
            S(-17, -40),   S(  6,  29),   S( -6,  23),   S(  3,  37),   S( -3,  22),   S( -7,  17),   S(  3,  12),   S(  4,  25),

            /* kings: bucket 6 */
            S( 38, -15),   S(  9,   0),   S( -8,   6),   S( -3,   4),   S(  3,   4),   S(-14,  12),   S( 24,  19),   S(  5,  11),
            S( 52, -30),   S( 54,   8),   S(  6,  -3),   S( 54, -15),   S( 21,   2),   S( -2,   6),   S( 24,   5),   S( 41,  -9),
            S( -5, -10),   S(-13,   9),   S( -4, -12),   S(-14,  -2),   S( -7,  -6),   S(-54,   1),   S(  8,   2),   S(-43,  23),
            S(-18,  15),   S( 32, -10),   S(  2,   2),   S( 28,   8),   S( 78,  -3),   S(-14,   7),   S( 92, -24),   S( 18, -13),
            S(-18,  -2),   S(-27,  -5),   S(-14, -16),   S(-13,   1),   S(  0,   0),   S(  0,   0),   S(-10, -17),   S(-56, -16),
            S(-22,  -6),   S( 10,   9),   S(-27,  -2),   S(-14,  -8),   S(  0,   0),   S(  0,   0),   S(-40,  33),   S(-37,  -1),
            S(  3,   7),   S(  9,  -6),   S( 10, -12),   S(-10,  17),   S(  4,   3),   S(  4,   3),   S(  5,   8),   S(-28, -13),
            S( -2,  31),   S(-10,   5),   S(  1,   8),   S(  5,  29),   S(  4,  15),   S( -5,  22),   S( -7,  19),   S( -9,  -1),

            /* kings: bucket 7 */
            S( 33, -12),   S(  1,  -5),   S(-10, -20),   S( -1,   7),   S(-46,  16),   S(-31,  27),   S(-11,  35),   S( -9,  25),
            S( 22, -10),   S( 50, -25),   S(  6,  -3),   S(-30,  13),   S( -2,   0),   S(-41,  22),   S( 18,  -2),   S( 14,   6),
            S( 40, -25),   S(-20,  -2),   S(-14,  -6),   S(-26,   0),   S(-48,   5),   S(-67,  18),   S(  8,   5),   S(-65,  20),
            S(-24,  -1),   S( 40,  -5),   S(  6, -10),   S( 35,  -2),   S(-12,   9),   S( 55, -24),   S( 33,  -8),   S( 29, -20),
            S( -2,  -7),   S(  6,   5),   S(-15, -10),   S(-39,   8),   S(-11, -15),   S(  5, -15),   S(  0,   0),   S(  0,   0),
            S(-17, -32),   S( -6,   0),   S( 19, -23),   S( 21,  -4),   S( 11,  -9),   S( 24,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  20),   S(  6,  -5),   S( 11,  -7),   S(-19, -29),   S( 21, -32),   S( -8, -24),   S( 11,  17),   S( -3, -23),
            S(  0,   8),   S( -7, -43),   S( 30,  50),   S(  6,  -9),   S( 15,  29),   S( -6, -11),   S(  1,  13),   S(-16, -41),

            /* kings: bucket 8 */
            S(-11, 102),   S(-26,  75),   S(-27,  52),   S(-20,  -7),   S( -1,  18),   S(-11,  -3),   S( 50, -15),   S(-16,   9),
            S( 14,  78),   S( 20,  -1),   S( -3,  51),   S( -4,   7),   S( -4,  16),   S( 19,   5),   S( 20,   6),   S(  7,  30),
            S(  0,   0),   S(  0,   0),   S( 32,  50),   S( 12,   6),   S( 29,  -5),   S(  7, -11),   S(-11,  16),   S( -2,  -4),
            S(  0,   0),   S(  0,   0),   S(  9,  28),   S( 16, -24),   S(  2,  16),   S( 18,   6),   S( 30, -20),   S(  1,  39),
            S(  2, -13),   S(  2,  -4),   S( 14, -15),   S(  8,   1),   S(-18, -28),   S(  8, -11),   S( 14,  -2),   S( -2, -14),
            S( -3,  -1),   S( -1,  17),   S( -2,   5),   S( -6, -24),   S( -7, -13),   S(  1,  -5),   S(-11,   0),   S(  4,   2),
            S( -4, -19),   S(  0,  -9),   S(  7,  25),   S( -7, -28),   S(  0, -15),   S(  0, -31),   S(  0,  -6),   S(  2, -49),
            S(  2,   5),   S(-13, -44),   S( -1, -30),   S( -5, -11),   S(  7,  26),   S(-10, -37),   S(  7, -13),   S(  3, -11),

            /* kings: bucket 9 */
            S(-53,  52),   S(-41,  50),   S(-33,  47),   S(-69,  44),   S(-74,  26),   S(-28,  26),   S( 78,   5),   S(  9,  31),
            S(-24,  28),   S( 26,  17),   S(  1, -16),   S( 36,  10),   S( 25,  21),   S( 21,   2),   S(  7,  23),   S( 17,  -3),
            S(-18,   5),   S( -2,   2),   S(  0,   0),   S(  0,   0),   S(  7,  15),   S(  6,  -1),   S( 23,   3),   S( -3,   9),
            S(  3,  -8),   S(  4,  -9),   S(  0,   0),   S(  0,   0),   S( 19,  19),   S( 36,  -3),   S(  5,   6),   S(-13,  10),
            S(  0,   4),   S(  3,  26),   S(  6,  17),   S(  2, -22),   S(  4,   7),   S( -4,  -9),   S(  8,   7),   S(-20,  -4),
            S(  0,   8),   S(-16,  -6),   S(  6,   2),   S(-14,   3),   S( -1, -17),   S(-16,  37),   S(-18, -18),   S( -2,  48),
            S(  2, -12),   S( -6, -17),   S(  7, -16),   S(  6,  11),   S(  4,  21),   S( 19,  15),   S( -3, -15),   S( 10,  15),
            S(  6,  37),   S(  3,  -2),   S( 10,   0),   S( -9, -44),   S( -6, -36),   S( -8,   0),   S(  6, -21),   S(  6,  24),

            /* kings: bucket 10 */
            S( -9,  26),   S(-14, -16),   S(-17,  24),   S(-30,  21),   S(-62,  24),   S(-141,  54),  S(  1,  38),   S(-94,  85),
            S( 28,   9),   S( 16,  29),   S(-15,  -4),   S( 28,  11),   S( 60,  10),   S( 52,   4),   S( 12,  25),   S(-28,  28),
            S(  9,  10),   S( 16,   7),   S(  5,  -7),   S(  8,   4),   S(  0,   0),   S(  0,   0),   S(  9,  15),   S(-59,  21),
            S( 13,  -1),   S(  6, -12),   S( 32,  -3),   S( 15,  -7),   S(  0,   0),   S(  0,   0),   S(  8,  13),   S( 16, -10),
            S( -1,   7),   S( 23,  -2),   S( 14,  -6),   S(  8, -29),   S(  7,  -3),   S( -4,  12),   S( -4,   8),   S(-13,   9),
            S( -8,  28),   S(  1,  24),   S(-10,  14),   S(  6,   3),   S(  5,  -6),   S( -8,  -3),   S( -2,  15),   S(-10,  17),
            S(  1, -33),   S(  3, -13),   S( 16,   3),   S( 16,  18),   S( 15,  13),   S(-15,  -9),   S( 17, -20),   S(  4,  36),
            S(  4,  28),   S( 24,   4),   S(  4, -24),   S( -2,   3),   S(  0,  -7),   S(  0, -12),   S( -7, -47),   S( 10,  28),

            /* kings: bucket 11 */
            S(-60,  22),   S( 20,  29),   S(-27, -27),   S(-27,  -5),   S(-33,  -1),   S(-160,  74),  S(-74,  69),   S(-141, 150),
            S( 33, -33),   S( -4,  19),   S( -3, -11),   S( 24,  13),   S( 62,   8),   S( 19,  42),   S( 43,  10),   S( 39,  31),
            S(-14, -11),   S( 39, -17),   S(-25,  -3),   S(  5,   4),   S( 67,  -4),   S( 18,  29),   S(  0,   0),   S(  0,   0),
            S( 16,  27),   S( -7,   9),   S( 19,   9),   S( 53, -15),   S( 36, -21),   S( 21,  -2),   S(  0,   0),   S(  0,   0),
            S( -9, -33),   S(-10,  10),   S(-12,  -6),   S( 28, -19),   S( 25, -11),   S( -5, -25),   S( 14,  23),   S(  9,   6),
            S(  8,  26),   S(  5,  10),   S( 12,   4),   S(  4, -11),   S( 18,  -9),   S( -9,  -3),   S(-16,  18),   S( -8, -46),
            S( 13,   7),   S( 12,  15),   S(  8,  30),   S( -2, -27),   S( 20,   6),   S( -2,  -5),   S(  2, -19),   S(-10, -22),
            S(  9,   6),   S(  3, -29),   S( -8, -31),   S(  4,  -9),   S( -8, -21),   S( -8, -18),   S(  1, -12),   S(  6,  22),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  3,  81),   S(-15,  -6),   S(  8,   6),   S(  8,  22),   S( 17, -12),   S( -7,  41),
            S(  0,   0),   S(  0,   0),   S( 22,  91),   S(  3, -23),   S( 13,   8),   S( 11,   3),   S( 25,   6),   S(-16,  36),
            S( -1, -15),   S(  3, -16),   S( 13,  12),   S(  2,  27),   S( -2, -21),   S(  8,   7),   S( -6, -30),   S(  0,  12),
            S( -2,   6),   S( -7, -18),   S(-13, -22),   S(  7, -20),   S(-14, -24),   S(  7,   2),   S(-15,  11),   S(  9,  29),
            S(  7,  31),   S(  4,  20),   S(  7,  17),   S( -3, -16),   S( -5,  -2),   S(-12,   2),   S(-19,   0),   S( -6, -14),
            S(  5,  15),   S( -3,   1),   S(  3,  12),   S(-12, -20),   S(  1,  18),   S(-16,  -5),   S(-19, -16),   S(  7,  31),
            S(  2,   7),   S( -5, -18),   S(  1,   7),   S(  0,   9),   S( -6,   1),   S(  0,   1),   S( 12,  44),   S( -2, -24),
            S(  1,   0),   S(  2,  12),   S(  0, -19),   S(  2,  -1),   S( -1, -13),   S(  8,   4),   S(-10, -26),   S( -2, -12),

            /* kings: bucket 13 */
            S(-10,  65),   S( -4,  62),   S(  0,   0),   S(  0,   0),   S(  2,  77),   S(-18,   5),   S( 17,   3),   S(-13,  30),
            S( -3,  50),   S(-15, -19),   S(  0,   0),   S(  0,   0),   S( 30,  -1),   S( -9, -33),   S(-21,   9),   S(  0,  32),
            S(-12,  24),   S(  4,  36),   S(-10, -49),   S(  7,  -4),   S( -1,  13),   S( -8,   7),   S( -5,  -1),   S( -3,  17),
            S(-12, -37),   S(  3,  11),   S(  0,  -1),   S(  1, -52),   S(  5, -16),   S( -2, -45),   S( -6,   4),   S(-14, -27),
            S(  2,  12),   S(  5,   9),   S( -3,  18),   S(-11,  -6),   S(-19, -26),   S( -2,  19),   S(-14,   7),   S( 11,  37),
            S(  2,  14),   S( -2,   9),   S(  7,  36),   S( -7,   2),   S( -7, -16),   S( -6,  21),   S(  1,  -8),   S(  0,   0),
            S(  5,  19),   S(-12, -33),   S( -8, -14),   S( -3,   2),   S(  1,  -4),   S(  0, -29),   S(  0, -22),   S(  4,  14),
            S(  0,  -6),   S(  4,  23),   S(  6,  18),   S(  5,  -6),   S(  6,   7),   S( -2,   1),   S( -4, -10),   S(  4,  15),

            /* kings: bucket 14 */
            S( -3,  39),   S(-13,   5),   S(-26,  -3),   S(  4,  29),   S(  0,   0),   S(  0,   0),   S(-10,  92),   S(-57,  87),
            S(-20, -14),   S( -6,   7),   S( 11,  -5),   S( 10,   0),   S(  0,   0),   S(  0,   0),   S(  3,  -5),   S(-32,   6),
            S(  4,  17),   S( -6, -21),   S( 19,   4),   S( 10,  -2),   S(  6, -19),   S(  0,  12),   S( 12,  42),   S(-24, -16),
            S(  6,   3),   S(  5,  18),   S(-15, -31),   S(  3, -38),   S(-22, -54),   S(  9,  20),   S(-15,  -3),   S(  5,  16),
            S(  4,  37),   S(  0,  -1),   S( -2,   5),   S( -6, -20),   S( -1,  12),   S(  7,  56),   S(  8,  43),   S(  7,  31),
            S(  6,  26),   S( -7,  -5),   S( -2,  -2),   S(  1,  19),   S( -2, -25),   S( -3, -30),   S( -8, -20),   S(-10, -18),
            S(  2,  -7),   S( -7, -42),   S( -8,   7),   S(  6,  20),   S( -7, -15),   S(  2, -27),   S(-14, -86),   S( 15,  73),
            S(  0,  13),   S(  4,  44),   S( -1,  -4),   S( -3,   1),   S(  7,  50),   S( -2,  -9),   S( -2, -19),   S(  5, -10),

            /* kings: bucket 15 */
            S(  5,  34),   S( -1, -10),   S(  9,  -3),   S(  0,  13),   S(-28,   0),   S(-25,  79),   S(  0,   0),   S(  0,   0),
            S( -6, -27),   S( -8, -20),   S( -3, -28),   S( 30,  38),   S( 12, -27),   S( 38,  89),   S(  0,   0),   S(  0,   0),
            S(-17,  -8),   S( 10,  -9),   S(  3, -20),   S(-15,  -8),   S(  2, -17),   S( 25,  32),   S( 15,   6),   S(-17, -42),
            S(  5,  24),   S(  1,   8),   S(  3,   7),   S(-15, -29),   S(  5, -38),   S(  6,  11),   S(  5,  62),   S(  4,   5),
            S( 12,  29),   S(-13,   2),   S( -7,  26),   S(-17, -37),   S(  2,   1),   S( -1,  29),   S( -1,   1),   S(  2,  17),
            S(  6,  47),   S(-17,   1),   S( -9, -21),   S( 14,  31),   S(-17, -24),   S( -8,  -1),   S( -7,  -3),   S(  7,  16),
            S(  7,  29),   S(  0,  19),   S( -7, -10),   S(  5,  19),   S( -8, -32),   S(  1,  24),   S( -7, -30),   S(  5,  23),
            S( -8, -29),   S(  3,  -1),   S( -2,   9),   S(  0, -14),   S(  0,  29),   S(  3,  24),   S( -1,  -2),   S(  3,   9),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-11, -188),  S( 10, -36),   S( 21,  35),   S( 30,  74),   S( 37,  95),   S( 41, 117),   S( 48, 116),   S( 58, 107),
            S( 84,  72),

            /* bishop mobility */
            S(  7, -82),   S(  9,   2),   S( 19,  40),   S( 25,  75),   S( 31,  97),   S( 34, 113),   S( 37, 120),   S( 39, 118),
            S( 42, 121),   S( 50, 109),   S( 52,  97),   S( 85,  82),   S(100,  73),   S(145,  43),

            /* rook mobility */
            S(-16, -13),   S( -8,  57),   S( -8,  88),   S( -4, 104),   S( -7, 129),   S(  0, 136),   S(  2, 147),   S(  7, 148),
            S(  9, 157),   S( 12, 163),   S( 18, 166),   S( 27, 164),   S( 35, 161),   S( 47, 155),   S(122, 113),

            /* queen mobility */
            S( 61, -49),   S( 63, -85),   S( 57,  24),   S( 52,  90),   S( 51, 144),   S( 51, 181),   S( 49, 224),   S( 48, 247),
            S( 51, 261),   S( 48, 282),   S( 48, 299),   S( 44, 317),   S( 44, 327),   S( 40, 340),   S( 37, 353),   S( 39, 358),
            S( 41, 354),   S( 55, 343),   S( 62, 334),   S( 85, 311),   S(114, 291),   S(150, 260),   S(138, 263),   S(217, 209),
            S(194, 228),   S(167, 211),   S(120, 202),   S( 84, 171),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,  28),   S(-19,  60),   S(-39,  52),   S(-16,  69),   S(  8,  24),   S(-18,  41),   S(  2,  74),   S( 36,  29),
            S( 18,  38),   S( -1,  65),   S(-10,  51),   S( -3,  46),   S( -2,  45),   S(-31,  65),   S(-43,  86),   S( 37,  34),
            S( 38,  65),   S( 23,  75),   S( 16,  56),   S( 34,  46),   S( 14,  48),   S(-19,  66),   S(-19,  99),   S(  9,  67),
            S( 58,  84),   S( 64, 101),   S( 44,  67),   S( 28,  42),   S( 18,  57),   S( 30,  76),   S( -5, 104),   S(-47, 123),
            S( 66, 128),   S( 98, 139),   S( 85, 101),   S( 49,  90),   S(-21,  80),   S( 55,  89),   S(-15, 158),   S(-49, 154),
            S(165, 178),   S(153, 212),   S(181, 203),   S(185, 192),   S(198, 208),   S(203, 189),   S(176, 194),   S(207, 202),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  15),   S(  4,   2),   S( 10,   2),   S(  9,  39),   S( 21,  46),   S(  2, -25),   S(-17,  39),   S( 11, -34),
            S( -8,  31),   S( 29,  10),   S(  0,  41),   S( 23,  43),   S( 36,   1),   S(  1,  39),   S( 20,  -4),   S(  9,  15),
            S(-10,  14),   S( 18,  21),   S( -3,  50),   S( 18,  71),   S( 31,  25),   S( 25,  31),   S( 27,   3),   S( -2,  26),
            S(  4,  36),   S( 31,  47),   S( 33,  75),   S( 15,  98),   S( 74,  57),   S( 74,  46),   S( 25,  54),   S( 13,  30),
            S( 51,  44),   S(119,  73),   S(116, 139),   S(122, 171),   S(121, 133),   S(127, 128),   S(146,  97),   S( 69,  54),
            S( 78, 197),   S(116, 280),   S(112, 254),   S(116, 242),   S( 72, 170),   S( 56, 137),   S( 48, 140),   S( 18,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 14,  43),   S( 27,  36),   S( 41,  53),   S( 43,  33),   S( 29,  34),   S( 32,  39),   S(  7,  22),   S( 49,  14),
            S( -1,  27),   S( 15,  49),   S( 13,  40),   S(  9,  54),   S( 25,  21),   S(  7,  25),   S( 31,  24),   S( -3,  17),
            S( -4,  13),   S( 20,  36),   S( 50,  46),   S( 42,  36),   S( 42,  41),   S( 65,  17),   S( 15,  26),   S( 18,   4),
            S( 82,  24),   S(126,   6),   S(131,  77),   S(169,  90),   S(142,  89),   S( 95,  92),   S( 94,  18),   S( 84, -14),
            S(107,  12),   S(178,  18),   S(182, 155),   S(176, 172),   S(166, 182),   S(106, 143),   S(193, 118),   S(-21, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 30,  24),   S( 17,  21),   S( 25,  28),   S(  3,  56),   S( 68,  37),   S( 30,   7),   S( 10,  -5),   S( 34,   9),
            S(  2,  15),   S(  8,   7),   S( 22,  14),   S( 18,  33),   S( 17,  14),   S(  1,   9),   S(  4,   7),   S( 30,  -7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -15),   S( -8,  -7),   S(-22, -14),   S(-18, -33),   S(-17, -14),   S( -1,  -9),   S( -4,  -7),   S(-30,   7),
            S(-30, -24),   S(-17, -21),   S(-25, -28),   S( -3, -56),   S(-68, -37),   S(-30,  -7),   S(-10,   5),   S(-34,  -9),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -23),   S(-12, -13),   S( -1, -29),   S(-56,  -7),   S(-15, -15),   S(-17, -24),   S(  3, -25),   S(-15, -38),
            S(-17,   3),   S(-14,  -4),   S(-27,   9),   S(  4, -17),   S(-31, -15),   S(-24,  -1),   S(-42,   5),   S( -8, -12),
            S(-16, -13),   S(-11, -16),   S(-25,  -2),   S(-30,  -8),   S(-23, -31),   S(-25, -10),   S(-24,   1),   S(-39, -18),
            S(  4, -33),   S( 15, -38),   S( 17, -23),   S(  9, -28),   S(  6, -21),   S( 59, -32),   S( 35, -33),   S( -7, -45),
            S( 38, -65),   S( 54, -76),   S( 56, -33),   S( 63, -32),   S( 64, -27),   S( 93, -23),   S(139, -79),   S( 39, -75),
            S(120, -110),  S(166, -125),  S(137, -83),   S(135, -66),   S( 97, -45),   S(168, -39),   S(137, -56),   S( 69, -85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   2),        // attacks to squares 1 from king
            S( 16,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 65, -80),        // king-side castling available
            S( 19,  49),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S(  3, -48),   S(  4, -41),   S( -5, -48),   S(-19, -33),   S(-25, -26),   S(-41, -17),   S(-76,   4),   S(-86,   6),
            S(-95,  11),   S(-76,  11),   S(-79,  22),   S( 26,   1),   S(-38,  12),

            /* orthogonal lines */
            S( -5, -59),   S(-58, -26),   S(-81, -15),   S(-96, -12),   S(-103, -20),  S(-106, -26),  S(-107, -30),  S(-105, -31),
            S(-122, -25),  S(-140, -24),  S(-162, -30),  S(-159, -40),  S(-179, -29),  S(-385,  39),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( -1, 209),

            /* passed pawn can advance */
            S(-15,  38),   S(-22,  73),   S(  1, 107),   S( -4, 205),

            /* blocked passed pawn */
            S(  0,   0),   S( 59, -23),   S( 56, -20),   S( 54,  21),   S( 65,  26),   S( 75, -13),   S(173, -24),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 24,  -7),   S( 40,  36),   S( 29,  34),   S( 26,  66),   S( 64,  78),   S(127, 113),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S( -6,  -8),   S(  5, -18),   S( 18, -39),   S(  9, -37),   S( 33, -47),   S(205, -85),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 21, -16),   S( 58, -62),   S( 29, -30),   S( 30, -56),   S( 31, -169),  S(-22, -231),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 11,  33),   S( -1,  39),   S(127,  31),   S(  6,  33),   S(217,  79),   S(172,  54),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-30,  95),

            /* bad bishop pawn */
            S( -5, -22),

            /* rook on open file */
            S( 38, -16),

            #endregion

            /* tempo bonus for side to move */
            S(  8,  11),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
