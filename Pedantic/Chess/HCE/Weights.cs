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
        public const int MAX_WEIGHTS = 12773;
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
        public const int ROOK_ON_HALF_OPEN_FILE = 12759;    // rook on half-open file
        public const int PAWN_PUSH_THREAT = 12760;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12766;       // pawns threating opponent piece
        public const int TEMPO = 12772;             // tempo bonus for side moving

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

        public Score RookOnHalfOpenFile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[ROOK_ON_HALF_OPEN_FILE];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PushedPawnThreat(Piece threatenedPiece)
        {
            return weights[PAWN_PUSH_THREAT + (int)threatenedPiece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score PawnThreat(Piece threatenedPiece)
        {
            return weights[PAWN_THREAT + (int)threatenedPiece];
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

        // Solution sample size: 6000007, generated on Sun, 10 Mar 2024 01:12:30 GMT
        // Solution K: 0.003850, error: 0.085289, accuracy: 0.4989
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S(100, 179),   S(435, 538),   S(446, 569),   S(562, 967),   S(1363, 1648), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 97, -121),  S(152, -98),   S( 25, -32),   S(-41,  36),   S(-53,  36),   S(-31,  -1),   S(-47, -12),   S(-37, -25),
            S(103, -115),  S(101, -99),   S( -1, -53),   S(-18, -58),   S(-25, -20),   S(-22, -44),   S(-34, -35),   S(-27, -47),
            S( 87, -77),   S( 72, -61),   S(  6, -45),   S(  6, -56),   S(-17, -48),   S( -4, -52),   S(-19, -50),   S(-17, -38),
            S( 48, -27),   S( 55, -44),   S( 21, -35),   S( 22, -66),   S(-19, -42),   S(-42, -42),   S(-27, -37),   S(-23, -17),
            S( 53,  45),   S( -3,  27),   S( 66, -30),   S( 43, -65),   S( 28, -31),   S(-20, -42),   S(-40, -24),   S(-43,  52),
            S( 53,  44),   S( 50,  48),   S( 46, -45),   S( 15, -43),   S(-36, -16),   S(-24,   2),   S(  6,  18),   S( -6,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25, -25),   S( 21, -35),   S( 47, -27),   S( -8,  29),   S(-17, -11),   S(  2, -16),   S(-44,  -6),   S(-42,  19),
            S( 16, -33),   S( 12, -43),   S(  4, -42),   S( -4, -38),   S(-17, -18),   S(-11, -31),   S(-43, -14),   S(-46,  -7),
            S(  3, -18),   S(  3, -21),   S( 19, -43),   S( 19, -55),   S(-18, -27),   S(  9, -38),   S(-21, -24),   S(-20,  -4),
            S( 15,  10),   S( 15, -32),   S( 17, -38),   S( 13, -49),   S( -7, -24),   S( -4, -33),   S(-39, -10),   S(-32,  26),
            S( -6,  64),   S(-41,   4),   S(  1,  -8),   S( -8, -24),   S( 15,   1),   S(-47,  11),   S(-40,  10),   S(-51,  74),
            S( 71,  55),   S( 44,  -8),   S(-46,   5),   S(-11,  11),   S(-34, -17),   S(-38,  24),   S(-61,  15),   S(-73,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24,   1),   S(-35,   5),   S(-16,   0),   S(-19,  12),   S(  5, -13),   S( 28, -19),   S(  0, -41),   S(-14, -17),
            S(-24, -16),   S(-49,  -7),   S(-31, -31),   S(-23, -35),   S(  1, -25),   S(  2, -28),   S(-14, -33),   S(-27, -22),
            S(-37,  -3),   S(-36, -16),   S(-14, -43),   S(-11, -46),   S( -5, -25),   S( 13, -28),   S(-14, -27),   S(-10, -14),
            S(-49,  25),   S(-35, -21),   S(-33, -23),   S(-11, -40),   S( 10, -37),   S(-13, -15),   S(-11,  -6),   S(-21,  15),
            S(-35,  54),   S(-77,  24),   S(-54, -12),   S(-64, -13),   S(-14,  22),   S(-30,  11),   S(-46,  21),   S(-42,  79),
            S(-44,  77),   S(-93,  81),   S(-134,  20),  S(-78, -15),   S(-42,  22),   S(-28,  13),   S(-20,  -5),   S(-32,  64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29,  -6),   S(-42,   0),   S(-33,  -4),   S(-10, -36),   S(-13,  -6),   S( 34, -14),   S( 82, -65),   S( 55, -76),
            S(-36, -28),   S(-52, -19),   S(-37, -36),   S(-26, -29),   S(-19, -21),   S(  4, -35),   S( 54, -63),   S( 49, -70),
            S(-37, -24),   S(-24, -43),   S(-11, -56),   S(-10, -60),   S(-13, -43),   S( 16, -42),   S( 25, -46),   S( 55, -53),
            S(-40,   0),   S(-26, -44),   S(-21, -50),   S(  0, -68),   S( 19, -65),   S(  0, -44),   S( 18, -30),   S( 45, -13),
            S(-26,  32),   S(-30, -17),   S( -2, -50),   S(-12, -43),   S( 84, -51),   S( 53, -33),   S( 41,  16),   S( 23,  75),
            S(-48,  99),   S(-54,  40),   S( -2, -68),   S(-19, -50),   S( 26, -41),   S( 57, -18),   S( 60,   6),   S( 59,  58),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-70,   5),   S( 29, -34),   S(-10,   1),   S(  2,  26),   S(-29, -31),   S(-31,  11),   S(-84,  15),   S(-65,   6),
            S(-13,   3),   S( 38, -15),   S( 18, -31),   S( 40, -31),   S( -7, -21),   S(-48, -24),   S( -2, -40),   S(-11, -23),
            S( 35, -20),   S( 45,  -5),   S( 14,  -1),   S( 24, -34),   S(-22, -36),   S( 10, -45),   S(-58, -23),   S(-60,   3),
            S( 26,  31),   S(-22,  52),   S( 65,   5),   S( -5,   3),   S( 20, -31),   S(-28, -18),   S(  3, -29),   S( 32, -25),
            S( -9,  76),   S( -1,  76),   S(-16,  24),   S(-18,  -7),   S( 22,   6),   S(-35,  13),   S(-36, -24),   S( 52,  33),
            S( 54,  92),   S( 63,  66),   S( 22,  11),   S( 12,  -5),   S( 14, -31),   S( 13, -12),   S(  9,   8),   S( -9,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-73,  48),   S(-39,  22),   S(-13,  19),   S( 32,   6),   S(-22,  20),   S(-28,  21),   S(-36,   4),   S(-24,  22),
            S(-66,  24),   S(-54,  11),   S( 24, -13),   S(-15,  18),   S( 33, -14),   S(-18, -18),   S(-19, -15),   S(-44,  14),
            S(-56,  45),   S(-19,  14),   S( 78, -30),   S( 15, -16),   S( 47, -16),   S(-33,  -6),   S(-13,  -2),   S(-23,   9),
            S(-39,  63),   S(-29,  41),   S( 34,   9),   S( 52,   9),   S( -2,  11),   S(-42,   7),   S(  8,  -7),   S( 22,  14),
            S( 60,  59),   S( 73,  31),   S( 57,  39),   S( 36,  25),   S( -5,  44),   S( 73,  -5),   S(-12,   9),   S( -3,  47),
            S( 97,  40),   S( 96,  33),   S( 66, -16),   S( 23,  18),   S( 53, -10),   S( 30,  -1),   S(  9,  -8),   S( 26,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30,  20),   S( -6,  -5),   S(  2,   4),   S(-16,   3),   S( 23, -21),   S(-20,   5),   S(-48,   4),   S(-48,  13),
            S(-32,  -2),   S(  2, -27),   S( -8, -30),   S( 16, -15),   S( 51, -21),   S( 41, -24),   S(-23, -11),   S(-66,   9),
            S(-37,  14),   S( 11, -14),   S(  5, -20),   S( -8,  -9),   S( 32, -10),   S( 71, -28),   S(  2, -14),   S(-27,  17),
            S(-18,  26),   S(-24,   8),   S(  3,  -7),   S(  9,  -4),   S( 30,   5),   S( 30,   5),   S( 29,  -1),   S( 47,  13),
            S(-21,  42),   S(  0,  10),   S(-36,   9),   S( 22,  -5),   S( 58,  34),   S( 72,  32),   S( 66,   3),   S( 84,  31),
            S( 59,  35),   S( 31,   9),   S( -7, -30),   S( 39, -44),   S( 47,  -9),   S( 40, -18),   S( 43,   6),   S( 67,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49, -21),   S(-62,  -6),   S( -3, -20),   S(-42, -25),   S(-27, -24),   S( 36, -29),   S(-15, -36),   S(-64, -11),
            S(-60, -27),   S(-58, -34),   S(-20, -50),   S(  9, -57),   S(  8, -35),   S( 42, -42),   S( 44, -37),   S(-29, -24),
            S(-69, -20),   S(-52, -35),   S(-37, -43),   S( -8, -50),   S( -4, -24),   S( 30, -23),   S( 43, -39),   S( 34, -26),
            S(-44, -11),   S(-58, -26),   S(-72, -28),   S(-28, -23),   S(  0, -24),   S( 10,  -3),   S( 23,   2),   S( 65, -12),
            S(-53, -10),   S(-14, -29),   S(-31, -27),   S( -8, -51),   S( 43, -10),   S(  2,  11),   S( 62,  41),   S(102,  40),
            S(-17,   1),   S(-32, -42),   S( 21, -74),   S( 21, -33),   S( 36, -52),   S( 24, -21),   S( 58,  37),   S( 69,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  55),   S(-37,  36),   S(-10,  25),   S(-17, -36),   S(  2,  22),   S(-17,   4),   S(-74,  -2),   S(-61,  21),
            S(-62,  29),   S(-30,  15),   S(-57,   4),   S( -4,   6),   S(-53,   1),   S(-42, -28),   S(-43, -25),   S(-27,   0),
            S(-17,  66),   S( 41,  76),   S( 11,  37),   S( -9,  33),   S( -2, -20),   S(-35, -33),   S(-31, -45),   S(  3, -28),
            S( 43,  98),   S( 62, 124),   S( 67,  70),   S( 24,  46),   S( 21,  -2),   S(-17, -26),   S(-12, -16),   S(-10, -21),
            S( 53, 125),   S( 71, 113),   S( 61, 128),   S( 65, 100),   S( 24,   7),   S(  8, -12),   S( -8, -16),   S( -6,   0),
            S(100, 137),   S( 73, 156),   S(100, 163),   S( 51,  58),   S(  5,  11),   S( -7, -30),   S( -3, -44),   S(  5,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52,  54),   S(-85,  49),   S(-28,   2),   S( 15,  21),   S(  2, -13),   S(-105,  36),  S(-93,  13),   S(-84,  26),
            S(-55,  14),   S(-37,   4),   S(-69,  28),   S(-22,  32),   S(-83,  14),   S(-72,   0),   S(-81,  -2),   S(-50,   8),
            S(-47,  46),   S(-37,  62),   S( -6,  66),   S(-60,  64),   S(-58,  56),   S(-84,  10),   S(-28, -15),   S(-17,   1),
            S( 18,  70),   S( 52,  78),   S( 60, 118),   S( 62, 124),   S(-17,  61),   S(-16,  16),   S( 52,  -2),   S( 60, -14),
            S( 89,  77),   S( 61, 119),   S( 70, 121),   S( 88, 144),   S( 78, 106),   S( 29,  38),   S( 27, -21),   S(  3,  -8),
            S( 43,  58),   S( 74,  76),   S(108, 112),   S(113, 133),   S( 36,  90),   S( -3, -18),   S(  4, -13),   S(  0, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-123,  21),  S(-102,  12),  S(-19,  12),   S(  7,  23),   S(-11,   2),   S(-110,  43),  S(-115,  30),  S(-60,  37),
            S(-87,  -8),   S(-50, -15),   S(-19, -19),   S(-78,  13),   S( -4,   8),   S(-36,  20),   S(-144,  34),  S(-100,  21),
            S(-17, -12),   S(-54,  -6),   S(-20,  15),   S(-44,  48),   S(-58,  80),   S( 32,  46),   S(-85,  36),   S(-82,  47),
            S(  6, -11),   S( 23,   2),   S(  3,  29),   S(-28,  81),   S( 44, 100),   S(-32,  95),   S( 17,  47),   S( 39,  22),
            S( 42, -11),   S( 15,  -8),   S( 22,  39),   S( 75,  93),   S( 76, 115),   S( 81,  61),   S( 33,  72),   S( 70,  44),
            S( 43,  -2),   S( 14, -31),   S( 17,   9),   S( 77, 109),   S( 40,  77),   S( 56,  46),   S( 37,  34),   S( 52,  85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-108,   4),  S(-93,  -3),   S(  8,  -4),   S( -5,   0),   S( 11,  20),   S(-80,  44),   S(-40,  27),   S(-52,  32),
            S(-95, -20),   S(-46, -35),   S(-26, -44),   S(-56,  14),   S(-49,   4),   S(-26,  11),   S(-117,  55),  S(-104,  42),
            S(-43, -28),   S(-17, -46),   S(-33, -11),   S(-36,  -4),   S(-48,  34),   S( -7,  48),   S(-32,  65),   S(-43,  56),
            S( 17, -17),   S(-52, -21),   S( -8,  -7),   S(-42,  19),   S(-24,  49),   S( 42,  51),   S( 41, 102),   S( 80,  66),
            S(  7,   3),   S(-25, -30),   S(  0, -11),   S(-30,   1),   S( 65,  82),   S( 59,  76),   S(130,  97),   S(181,  64),
            S( 13,  -8),   S(  4, -23),   S( 15,  18),   S(  2, -13),   S( 31,  25),   S( 44, 102),   S( 97, 138),   S(125, 124),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10,  -9),   S(  0,  23),   S(-24,  -8),   S( -4,  -9),   S( -4,   2),   S(-18,   4),   S(-21,  -4),   S(-25,  10),
            S(-15,  -3),   S(-12,   4),   S(  6,  36),   S(  7,  14),   S( -9,  54),   S(-11,  16),   S(-41, -47),   S(-24, -59),
            S(-23,  44),   S(-12,  42),   S( 29,  42),   S( 29,  13),   S( 17,   9),   S(-32, -17),   S(-24, -44),   S(-34, -78),
            S( -8,  91),   S( 23,  70),   S( 62,  89),   S( 43,  67),   S(-10, -48),   S(-14, -51),   S( -1, -19),   S(-31, -75),
            S( 72,  68),   S( 64, 195),   S( 69, 129),   S( 20,  66),   S(-11, -24),   S( -3, -19),   S( 12,  -5),   S(-16, -71),
            S( 43, 103),   S( 58, 265),   S(123, 243),   S( 62, 110),   S( -3,  -7),   S( -3, -45),   S( -8, -55),   S(-16, -95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -31),   S(-24,  -6),   S(-11, -17),   S( -3,   0),   S( -5, -15),   S(-23,   2),   S(-32, -48),   S(-34, -15),
            S(-16, -35),   S(-42,  11),   S(  1,   3),   S( 24,  28),   S(-38,   4),   S(  0,  -5),   S(-51, -41),   S(-11,  -6),
            S( 18,  25),   S(  5, -20),   S(-15,  19),   S( 16,  42),   S(-11,  24),   S(-32, -20),   S(-11, -51),   S(-30, -50),
            S( 30,  36),   S( 66,  70),   S( 47,  96),   S(  3,  93),   S( 35,  56),   S( 21, -17),   S( 13, -28),   S( 24, -59),
            S( 39,  47),   S( 69, 110),   S(108, 185),   S( 96, 187),   S( 56, 120),   S(  9,  11),   S( 12, -56),   S( 13, -81),
            S( 46,  91),   S( 80, 148),   S( 88, 213),   S(110, 235),   S( 56, 111),   S( 12,  20),   S(  6, -21),   S( 12, -49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -40),   S(-38, -32),   S(-10, -45),   S(  0,   2),   S( -2, -17),   S(-48,  21),   S(-32, -13),   S(-16,  31),
            S(-27,  -4),   S(-14, -22),   S(-44, -45),   S( -5,   7),   S( -1,  40),   S( -8,  22),   S(-30,  15),   S(-21,  17),
            S(-11, -43),   S(-27, -13),   S(-17, -31),   S( -9,  15),   S( -1,  35),   S(-18,  18),   S(  3,  -4),   S(  4,  15),
            S(  5, -47),   S( 33,   9),   S(  7, -19),   S( 19,  56),   S(  6, 125),   S( 21,  81),   S(  5,  29),   S( 34,  20),
            S( 29, -50),   S( 30,  19),   S( 29,  15),   S( 39, 102),   S( 51, 178),   S( 82, 161),   S( 54,  66),   S( 40,  43),
            S( 23, -22),   S( 25,   9),   S( 39,  77),   S( 45, 117),   S( 80, 241),   S( 44, 161),   S( 39, 119),   S( 16,  49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62, -60),   S(-36, -42),   S(-11, -56),   S( -3,  -5),   S( 15,  19),   S(  2,  17),   S(-18,  -5),   S( 25,  47),
            S( -7, -82),   S(-39, -32),   S(-26, -38),   S(  9,  23),   S(-36,   4),   S(  4,  32),   S( -2,  27),   S(-17,  18),
            S( -8, -65),   S(-11, -87),   S(-33, -46),   S(  1,  14),   S( -4,  33),   S( 18,  44),   S( 26,  52),   S( 19,  66),
            S(  5, -29),   S(-31, -56),   S(-11, -22),   S( 19,  29),   S( 14,  45),   S( 53,  38),   S( 19, 111),   S(  9,  68),
            S(-18, -82),   S(-10, -53),   S( -1, -12),   S(  9, -18),   S( 42, 102),   S( 90, 101),   S( 56, 238),   S( 66,  81),
            S( 13, -26),   S( -6, -28),   S(  9,  -2),   S(  7,  21),   S( 42, 105),   S( 73, 233),   S( 18, 213),   S( 39, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(  0,   2),   S(  1,  -8),   S(-41,  -3),   S(-22, -30),   S(-35, -28),   S(-24, -25),   S(-39, -75),   S(-54, -28),
            S(-45,  41),   S( 31, -71),   S(-25, -14),   S(  3, -28),   S( -1, -36),   S(-15,   1),   S(-47,  -6),   S(-66, -32),
            S(  8,  71),   S(  5, -36),   S( 17, -30),   S(-19,  29),   S( 18,   9),   S(-28,  -8),   S( -5, -44),   S(-49, -38),
            S( 32, -35),   S( 18,  11),   S( 27,   7),   S( 28,  21),   S( 13,  -7),   S( -5,   5),   S( -2, -41),   S( -9,  14),
            S( 33, -61),   S( 54, -26),   S( 25,   1),   S( 82, -32),   S( 39,  -3),   S( 39,   7),   S( 14, -14),   S(-77, -40),
            S(-19, -46),   S( -3,  -5),   S( 25,  14),   S( 63, -31),   S( 40, -85),   S( 10,   7),   S( -2, -34),   S( -2, -11),
            S( 20, -15),   S( 33, -62),   S( 56, -58),   S( 52, -19),   S( 25, -28),   S(-45, -43),   S(-32, -56),   S(  2, -63),
            S(-76, -52),   S(-13,  -8),   S(-12, -12),   S(-33, -49),   S(-20, -16),   S( 17,   9),   S( -4, -13),   S( 12,   3),

            /* knights: bucket 1 */
            S(-19,  34),   S(-54,  77),   S( 12,  18),   S( -8,  41),   S( -3,  41),   S(-32,  42),   S(-33,  40),   S(  5,  -9),
            S( 16,  10),   S(-20,  51),   S(  8,  -4),   S( -7,  27),   S( -6,  22),   S( -3,   9),   S(  4, -27),   S(-32,  -8),
            S(-29,  13),   S( 13,   0),   S( 14,   4),   S( 25,  21),   S( 11,  27),   S(-22,  27),   S( -5,  -5),   S(-29,  -4),
            S(  8,  20),   S( 73,  26),   S( 33,  42),   S( 33,  25),   S( 20,  24),   S(  4,  22),   S( 29,  -6),   S( 22,  19),
            S( 17,  40),   S( 37,   6),   S( 40,  13),   S( 65,   9),   S( 43,  28),   S( 39,  24),   S( 39,  14),   S( 29,  -4),
            S( 57, -14),   S( 35,  -8),   S( 33,  35),   S( 46,  22),   S( 59, -12),   S( 42,  29),   S( 47,  -8),   S( 18, -16),
            S( 37,  -4),   S(  4,  40),   S(-22, -36),   S( 27,   5),   S( -4,   6),   S( 40,  14),   S(-26, -22),   S(-22, -29),
            S(-123, -84),  S(-39, -20),   S( -4,  32),   S( -1,  -1),   S( -1,  -2),   S(-29, -32),   S( -8, -27),   S(-55, -67),

            /* knights: bucket 2 */
            S(-81,   1),   S( -6,  28),   S(-30,  50),   S(-23,  45),   S(-36,  50),   S(-38,  43),   S(-19,  44),   S(-45,  26),
            S(-23, -15),   S(-32,   8),   S(  0,   9),   S(-11,  23),   S( -3,  17),   S(-12,  30),   S(-14,  43),   S(-42,  83),
            S(-24,  21),   S(  1,  18),   S(-13,  40),   S( 23,  16),   S( 10,  23),   S(  4,   9),   S( -5,  35),   S(-32,  37),
            S( -9,  37),   S(-12,  19),   S(  6,  38),   S(  7,  50),   S(  7,  49),   S( -3,  44),   S(-12,  49),   S( -7,  46),
            S( 21,  13),   S( -4,  39),   S(-10,  56),   S(-15,  57),   S( -6,  64),   S(-13,  63),   S( 10,  45),   S(-24,  16),
            S(-32,  25),   S(  1,  21),   S(-24,  46),   S(  3,  41),   S(  8,  45),   S(-24,  51),   S(-36,  24),   S( 23,  -2),
            S( -9,  22),   S(-35,  10),   S(-22,   2),   S(-33,  32),   S(  9,  25),   S( 58, -11),   S(-45,  33),   S(-48,  -5),
            S(-150,  13),  S( -1, -11),   S(-81,  48),   S(  4,  12),   S(  8,  -9),   S(-45, -15),   S(  7,   2),   S(-151, -40),

            /* knights: bucket 3 */
            S(-58,  -3),   S( -8, -15),   S(-51,  17),   S(-30,  31),   S(-20,  15),   S(-18,  12),   S(  0,  18),   S(-34,   1),
            S(-29,  15),   S(-50,   5),   S(-22,   3),   S(  1,  16),   S(  8,  14),   S(-27,   3),   S(-19,   1),   S(-26,  69),
            S(-24, -12),   S( -3,   9),   S(-10,  19),   S( 11,  21),   S( 18,  28),   S( 10,  14),   S( -4,  14),   S( -5,  51),
            S(-12,  12),   S( -3,  24),   S(  3,  50),   S( 15,  46),   S( 18,  54),   S( 15,  46),   S( 20,  36),   S( 11,  28),
            S( 12,  19),   S(  2,  33),   S( 16,  28),   S( 19,  56),   S(  8,  68),   S( 10,  73),   S( 33,  50),   S(-15,  33),
            S(-10,   8),   S( 12,   0),   S( 26,  18),   S( 57,  11),   S( 50,  11),   S( 94, -25),   S( 11,  14),   S( -9,  57),
            S( 24, -10),   S(  6,   5),   S( 33,  -4),   S( 47, -13),   S( 55, -13),   S( 81, -44),   S( 22, -34),   S( 50, -20),
            S(-165,  42),  S(-24,   6),   S(-36,   6),   S( 13,  16),   S( 47, -11),   S( 11,  -1),   S(  7, -15),   S(-75, -63),

            /* knights: bucket 4 */
            S( 16,  22),   S(-39,  12),   S( 11,   7),   S(-10, -23),   S(-15, -18),   S(-17, -10),   S( 13, -72),   S(-36, -46),
            S( 28,  42),   S(-30,  10),   S( -8, -40),   S( 27,   1),   S( 84, -73),   S(-20, -55),   S( 19,  12),   S( -2, -44),
            S(-14,  19),   S( 12,  19),   S( 28,  -8),   S( 91, -12),   S( 23,  -7),   S(-43,  -7),   S(-11, -53),   S(-52, -92),
            S(  9,  19),   S( 57, -40),   S( 74,   2),   S( -4,  14),   S( 13,  -5),   S(137, -29),   S( 11, -68),   S(-28, -69),
            S( 14,  39),   S(-22,  47),   S( 67,  28),   S( 11,  14),   S( 66,  11),   S( 11, -20),   S(-15, -38),   S(  5, -33),
            S(  2,  12),   S( -9,   0),   S(126, -10),   S( 16, -26),   S( 36,  -8),   S( 24,  20),   S( 14,  21),   S(-20, -36),
            S(-10,   1),   S( -5,  15),   S( 23,  16),   S( 14,  10),   S( 20,  -5),   S( 11, -45),   S( 19, -17),   S(-20, -25),
            S(-11, -11),   S( -2, -19),   S(  0, -20),   S( -3,   2),   S(-11, -26),   S(  9,  26),   S( -1,  11),   S( -6, -24),

            /* knights: bucket 5 */
            S( 19, -11),   S(-24,   1),   S( 51,  29),   S( 26,  15),   S( 32,   9),   S( 29, -30),   S(-16,  -5),   S(-10,  -2),
            S(  8, -18),   S( 11,  37),   S( 25,   8),   S( 16,   9),   S( 65,  12),   S( 33,  -2),   S( 27,   3),   S(-23, -19),
            S( 19,  13),   S(-18,  40),   S( 49,  13),   S( 83,   5),   S(  8,  30),   S(-17,  34),   S( 31, -19),   S(-11,   8),
            S( 55,  43),   S( 32,  34),   S( 58,  23),   S( 29,  29),   S( 47,  26),   S( 32,  30),   S( 60,  14),   S(  3,  22),
            S(-21,  39),   S( 41,  17),   S( 66,  42),   S( 62,  41),   S(114,  36),   S( 68,  28),   S( 56,  33),   S( 38,  17),
            S(  8,  22),   S(  9,  31),   S( 26,  14),   S( 30,  52),   S( 53,  36),   S( 22,  57),   S( 27,   1),   S( -6,  18),
            S( 23,  39),   S(  0,  65),   S( 29,  38),   S( 21,  66),   S(  0,  34),   S( 17,  44),   S( 16,  54),   S(  6,  11),
            S(-15,   5),   S( -6,  25),   S( 10,  52),   S( 10,  -4),   S( 16,  27),   S(  5,  16),   S(  9,  34),   S(-29, -41),

            /* knights: bucket 6 */
            S( -2, -45),   S( 34, -36),   S( 37,  24),   S(  1,  40),   S( -5,  21),   S( 23,  35),   S(-28,  23),   S(-19,  32),
            S(-17, -36),   S( 84, -28),   S( 22,  10),   S(-13,  17),   S(-27,  49),   S( 69,  20),   S( 47,   5),   S( 15,  13),
            S( -9, -31),   S(  2,  10),   S( 53,   1),   S( 36,  14),   S( 32,  25),   S(-15,  31),   S( 17,  46),   S( 19,  37),
            S( 22,   9),   S( 46,  -3),   S( 95,   7),   S( 90,  10),   S( 39,  25),   S( 53,  29),   S( 13,  55),   S( 17,  47),
            S(-20,  35),   S( 74,  -8),   S( 75,  26),   S( 86,  32),   S(139,  18),   S(135,  19),   S( 41,  48),   S( 30,  40),
            S( 23,   5),   S( 44,  -3),   S( 71,  24),   S( 40,  45),   S( 67,  59),   S( 43,  37),   S( 33,  37),   S( 11,  40),
            S(-15,  24),   S( -6,  17),   S(-22,  32),   S( -3,  26),   S( 15,  38),   S(  4,  51),   S(  6,  53),   S(-19,  14),
            S(-21, -18),   S( 10,  50),   S( 40,  21),   S( -7,  27),   S( 38,  27),   S( 17,  43),   S( 30,  51),   S( 10,  20),

            /* knights: bucket 7 */
            S(-21, -44),   S(-198, -44),  S(-39, -49),   S(-61, -24),   S(-39, -10),   S(-48, -27),   S( -3,   4),   S( -7, -24),
            S(-20, -96),   S(-25, -42),   S(-50, -33),   S(-32,  -4),   S(-35,   6),   S( -5, -24),   S(  8,  32),   S( 20,  26),
            S(-75, -70),   S(-57, -28),   S(-17,   0),   S( 46, -30),   S( 20,  -8),   S( 38, -11),   S(-17,  46),   S( 37,  58),
            S(-83, -23),   S( 56, -44),   S(  3,  -2),   S( 69, -16),   S( 99, -18),   S( 39,  -7),   S( 52,  -1),   S( -9,  26),
            S(-66, -42),   S(-28, -20),   S(107, -55),   S( 97, -10),   S(112,  -6),   S(110,   6),   S( 74,  22),   S( 70,  23),
            S(-17, -60),   S( 18, -38),   S( 13, -15),   S( 15,  12),   S( 71,   0),   S(114,  -7),   S( 60, -19),   S( -1,  12),
            S(-42, -17),   S(-60, -23),   S( -1, -30),   S( 54,   6),   S( 33, -14),   S( 51, -19),   S(-30,  20),   S( 21,   4),
            S(-34, -23),   S( -9,   2),   S(-28, -40),   S(  5, -22),   S( -2,   9),   S( 16, -13),   S( -6, -47),   S(-11,  -2),

            /* knights: bucket 8 */
            S( -4, -11),   S( -4,  -3),   S(  2,  17),   S( -9, -32),   S( -5, -21),   S(-11, -47),   S(  0,  17),   S( -6, -21),
            S(  3,  -1),   S( -4, -16),   S(-20, -41),   S(-30, -36),   S(-31, -27),   S(-15, -68),   S(-11, -52),   S(-19, -65),
            S(  8,  26),   S(-28, -47),   S(  2,  13),   S( -5,   0),   S(  1, -11),   S( -9,   0),   S( -8, -28),   S( -1, -41),
            S(-14,   9),   S( -3,   9),   S(-11,  -3),   S( -8,  51),   S( 11,   1),   S( -5, -28),   S( -9, -33),   S(  3,  -5),
            S( 37,  90),   S(-16,   7),   S( 15,   4),   S( 24,  40),   S( 10,  37),   S( -4,  -6),   S(  9, -13),   S( -8, -16),
            S(  7,  48),   S(  6,  22),   S( 27,   1),   S( 31,  28),   S( -5,  -2),   S(-16, -40),   S( -8, -46),   S(-10, -15),
            S( -5,   3),   S(  4,  22),   S( 14,   4),   S( 20,  24),   S(  3,   9),   S( 18,  51),   S(  1,   0),   S( -4,  -3),
            S(  3,  10),   S( 17,  53),   S(  0,   0),   S( -2,  14),   S(  3,  10),   S( -6, -22),   S(  5,  23),   S( -2,  -8),

            /* knights: bucket 9 */
            S( -8, -31),   S(-22, -46),   S(-11, -32),   S( -3, -15),   S(-31, -65),   S(-16, -65),   S( -2, -15),   S( -6, -24),
            S( -8, -18),   S(-13, -15),   S(-17, -64),   S(-16,  -9),   S( -3, -21),   S( -5, -56),   S(  3,  16),   S(-15, -45),
            S( 11,  12),   S(-28, -21),   S( 12,  -4),   S( 10,  -3),   S(-12,   0),   S(-50,  -7),   S(-10,  24),   S(-15,  -5),
            S(-16, -26),   S(-17, -26),   S(  2,  12),   S(  0,   2),   S( 19,   4),   S( 16,  47),   S(-20, -43),   S(  3,   3),
            S(  1,  27),   S( 10, -17),   S( 16,  20),   S( 23,  48),   S( 21,  20),   S( -1,  -8),   S( -3, -25),   S(  5,   7),
            S( 12,  45),   S( 18,  37),   S( 25,  31),   S( -9,  -8),   S( 19,  32),   S(  7,  18),   S( 10,  32),   S(-13, -43),
            S( -7, -31),   S( -4,  16),   S( 19,  62),   S(  3,  26),   S( 16,  51),   S( -5, -10),   S(  5,  29),   S(  3,  18),
            S( -1,  -7),   S(  4,  21),   S( 10,  25),   S(  8,  40),   S( 12,  23),   S(  1,  16),   S(  4,  15),   S( -2, -14),

            /* knights: bucket 10 */
            S(-23, -74),   S(-20, -75),   S(-19, -47),   S(-10,  -2),   S(-10, -27),   S(-17, -53),   S( -4, -13),   S( -1,   5),
            S(-16, -71),   S( -7, -35),   S(-12, -14),   S(-11, -37),   S(-18, -41),   S( -1, -35),   S(-12, -14),   S( -7, -29),
            S(-21, -86),   S(-37, -86),   S(-28,  -7),   S(-21, -42),   S(  9,   6),   S(-14, -16),   S( -9, -17),   S(-10, -19),
            S( -9, -38),   S(-12, -57),   S( -4, -26),   S( 21,   4),   S( 23,  10),   S( 38,   7),   S(  6,  22),   S( 14,  49),
            S(-11, -65),   S(-22, -31),   S( 30,  21),   S( 40,  16),   S( 24,  14),   S( 27,  18),   S( 11,  23),   S( 14,  49),
            S( -4, -10),   S( -8, -15),   S( -7, -17),   S( 16,  18),   S( 40,  42),   S( 11,  26),   S( 27,  68),   S( 18,  70),
            S(  2,   5),   S(-10, -23),   S( 10,  -6),   S( 19,  65),   S( 19,  34),   S( -3,  10),   S(  3,   0),   S(  9,  31),
            S( -3, -15),   S( -2,  -3),   S(-16, -25),   S(  2,  15),   S(  6,  33),   S(  0,  31),   S(  3,  28),   S( -1,   3),

            /* knights: bucket 11 */
            S(  1,  19),   S(-26, -42),   S(-16, -64),   S(-19, -42),   S(-32, -53),   S(-17, -24),   S(-10, -11),   S( -7,  -5),
            S( -5, -17),   S(-12, -11),   S(-15, -97),   S(-12, -45),   S(-16, -13),   S(-32, -36),   S(-16, -18),   S( -6, -16),
            S(-20, -61),   S(-41, -51),   S(-22,  -2),   S(  2,  10),   S(-20,  12),   S(-26,   7),   S( 11, -23),   S( -1,   8),
            S(-12, -25),   S( -5, -18),   S( -1,  11),   S( 17,  48),   S( 24,  17),   S( 12,  20),   S( 12,  26),   S(  4,  17),
            S( 11,  -9),   S(-44, -92),   S( 14,   1),   S( 22,   7),   S( 19,  -1),   S( 47,  36),   S( -6, -19),   S( 36,  89),
            S(-14, -14),   S( -4, -27),   S(  4,  16),   S( 32,  23),   S(  6,   8),   S( 45,  47),   S( 15,   7),   S(  3,  53),
            S( 13,  48),   S( -5,   3),   S( -1, -27),   S( 15,  21),   S( 14,  31),   S( -6,  33),   S( 18,  62),   S( 10,  70),
            S( -6,  -3),   S( -8, -43),   S( 17,  44),   S(  1,   7),   S( -8,  -7),   S(  6,   8),   S( 14,  28),   S(  7,  38),

            /* knights: bucket 12 */
            S( -5, -11),   S(  0,   3),   S( -3, -21),   S( -7,  -6),   S( -7, -13),   S( -1,  -1),   S(  1, -11),   S( -1, -12),
            S(  2,   4),   S(  3,   9),   S(  9,  35),   S( -2, -20),   S( -4, -18),   S( -3, -23),   S( -2, -20),   S(  1,   7),
            S( -6, -11),   S( 10,  20),   S( -7,  -4),   S(  1,  17),   S( -2, -11),   S( -2, -10),   S(  2,   9),   S( -7, -30),
            S( -8, -11),   S(  1,  -9),   S(  9,  10),   S( -6,  24),   S(  0,  12),   S(  7,  20),   S(  3,   8),   S(  2,  -1),
            S(  7,  25),   S(  7,   5),   S(-13, -19),   S( -2,  -9),   S( -5, -20),   S(  2,  -6),   S(  0, -15),   S(  2,   4),
            S(  4,  38),   S(  3,  -5),   S(  2,  50),   S( -2, -15),   S( -4, -21),   S(  0, -20),   S( -1, -18),   S(  0,   9),
            S(  3,  21),   S(-14,  -6),   S( -4,  -3),   S( -2,   2),   S(  1,   3),   S( -2, -12),   S(  0,   4),   S(  0,   0),
            S(  2,   8),   S(  3,   4),   S( -5, -16),   S(  7,  21),   S( -6, -12),   S( -2,  -8),   S( -4, -15),   S(  1,   5),

            /* knights: bucket 13 */
            S( -5, -17),   S(  2,   3),   S(  0,  -7),   S(  1,   1),   S( -4, -24),   S(  3,   2),   S( -6, -22),   S( -1, -10),
            S(  1,   0),   S(  2,  12),   S(  1, -23),   S( -6, -29),   S( -7, -38),   S( -1, -13),   S( -2,  -7),   S( -1,  -7),
            S( -8, -34),   S( -3, -10),   S(  7,  18),   S( -4,  -4),   S( -9,   2),   S(-12, -28),   S( -3,  -5),   S(-10, -41),
            S(-11, -15),   S( 14,  47),   S( -1,   4),   S( -1,  10),   S(  2, -10),   S(  6,   1),   S(  2,  -1),   S( -3,   0),
            S(  6,  28),   S(  4,  18),   S( 18,  28),   S( 17,  31),   S(  8,  30),   S(  4,  23),   S( -8, -28),   S(  0,  -6),
            S(  5,  33),   S( 19,  22),   S( 12,  90),   S( -5,  34),   S( 10,  35),   S( -7, -36),   S(  4,   2),   S( -4, -15),
            S(  2,   9),   S( -1,  22),   S( 13,  31),   S(  5,  48),   S( 13,  46),   S( -2,   0),   S(  2,  15),   S( -2,  -3),
            S( -1,   0),   S( 10,  72),   S( -1,  24),   S(  3,  22),   S( -1,  -2),   S(  1,   7),   S( -1,  -2),   S(  0,   3),

            /* knights: bucket 14 */
            S( -5, -27),   S( -7, -41),   S(  2,   7),   S( -1,  10),   S( -8, -30),   S( -5, -29),   S( -3,  -9),   S(  2,   4),
            S( -2, -19),   S( -3, -15),   S(-18, -73),   S( -8, -38),   S(  1,  -4),   S(  0,   5),   S(  2,  -4),   S(  0,  -3),
            S( -3,  -9),   S( -6, -33),   S(-16, -36),   S(  7,  33),   S(  3,  -2),   S(  3,   8),   S(  2,  -1),   S(  1,   6),
            S(  1,  10),   S( -4, -21),   S(  0, -33),   S( -7, -17),   S( -1, -21),   S(  8,  -9),   S(  1,  -3),   S( -5, -17),
            S( -3, -11),   S( -3, -15),   S(  4,  10),   S( -5, -14),   S( -9, -22),   S(  2, -27),   S(  5,   9),   S( -4, -25),
            S(  0,  -2),   S(  7,  21),   S( -4, -14),   S(  9,  30),   S( 12,  16),   S(-10,  24),   S(  5,  30),   S(  0,   5),
            S(  2,   5),   S(  0,  -3),   S(  7,  12),   S( -3,   3),   S( -7,  16),   S(-16,  -8),   S(  2,  22),   S( -1,   1),
            S(  0,  -4),   S(  4,  12),   S( -4,  -6),   S(  3,  10),   S(  7,  29),   S(  6,  31),   S(  1,  24),   S(  0,   3),

            /* knights: bucket 15 */
            S( -2,  -5),   S( -3, -21),   S( -5, -37),   S( -2, -14),   S( -1,  -3),   S(  2,  -7),   S(  1,  -8),   S(  3,  17),
            S( -1, -11),   S(  0,  -5),   S(  1,   9),   S( -5, -16),   S( -1, -18),   S(  2,   2),   S(  1,   3),   S( -1,  -3),
            S( -9, -36),   S( -8, -20),   S( -9, -31),   S(-18, -60),   S( -2, -14),   S( -5,  -7),   S(  0,  -3),   S(  0,  -1),
            S( -3, -18),   S( -2, -26),   S( -6, -15),   S(  0,  -4),   S(  8,   3),   S( 18,  37),   S( -4, -18),   S( -1,  -1),
            S( -3,  -6),   S( -6, -10),   S(  0, -31),   S(  0, -10),   S(  9,  25),   S(  9,   6),   S(-11, -32),   S( -3, -23),
            S( -6, -11),   S( -3,  -5),   S(  0,   2),   S(  8,  33),   S( -7, -21),   S(  3,  31),   S( -5, -11),   S(  1,  25),
            S( -3, -10),   S( -5, -12),   S( -3, -17),   S( -4,  -9),   S( -5,   2),   S(  1,  12),   S( -5, -22),   S(  9,  50),
            S(  0,  -1),   S( -1,   4),   S( -1,  -6),   S(  7,   7),   S( -2,  -3),   S( -5, -13),   S(  8,  31),   S( -1,   2),

            /* bishops: bucket 0 */
            S( 28,  28),   S( 23,  -4),   S( 58,   5),   S( 18,  11),   S(-17,   6),   S( 15, -24),   S( 30, -46),   S(-10, -21),
            S( 46, -19),   S( 84,  11),   S( 30,  21),   S( 18,   6),   S(  3,  36),   S(  5,   5),   S(-29,  20),   S( 13,  -6),
            S( 51,  13),   S( 47,  32),   S( 46,  18),   S(  9,  53),   S( 18,  30),   S(-14,  59),   S(  1,  -7),   S( 31, -63),
            S( 34,   5),   S( 70,  14),   S( 50,  24),   S( 35,  45),   S( -5,  82),   S( 36,  12),   S(  6,  19),   S(-14,  34),
            S(  6,   9),   S( 45,   5),   S( 15,  58),   S( 41,  58),   S( 41,  37),   S(-33,  67),   S( 23,  19),   S(-47,  44),
            S(-48,  87),   S(-34,  53),   S( 63,  38),   S( 58,  23),   S( 24,  60),   S(-12,  16),   S( -9,  43),   S(  9,  26),
            S(-49,  69),   S(-20,  47),   S(  2,  28),   S(-21,  79),   S(-61,  34),   S( 25,  29),   S(  1,  22),   S(-26, -20),
            S(  0,  -2),   S( 15,  45),   S(-17,  24),   S(-24,  11),   S( 16,  34),   S( 26,  31),   S(-15,  45),   S(-52,  -5),

            /* bishops: bucket 1 */
            S( 42,  48),   S( 18,  11),   S( 15,  29),   S(-17,  28),   S( 15,   5),   S(  3,  20),   S(-39,  43),   S(-46,  14),
            S( 15,  -8),   S( 20,  38),   S( 34,   8),   S( 27,  27),   S(  2,  21),   S( 23,  -1),   S(-18,  23),   S( 19,   7),
            S( 27,   4),   S( 14,  23),   S( 48,  35),   S( 10,  41),   S( 15,  41),   S( -6,  41),   S( 28,  20),   S(  5, -22),
            S( 32,   2),   S( 42,  28),   S(  7,  33),   S( 28,  56),   S( -4,  52),   S( 19,  37),   S(  7,  43),   S( 32,   3),
            S( 28,  30),   S( 19,  39),   S( 18,  42),   S( -1,  53),   S(  8,  56),   S(-18,  53),   S( 34,  19),   S(-23,  38),
            S( -8,  40),   S( 31,  48),   S( 18,  55),   S( 43,  29),   S(  0,  69),   S( 24,  53),   S(  1,  35),   S( 39,  14),
            S(-13,  53),   S( 18,  45),   S( 20,  45),   S( 10,  55),   S( 30,  43),   S(-31,  50),   S(  0,  58),   S(-54,  45),
            S( 50,  57),   S(-19,  31),   S(-22,  28),   S(-43,  37),   S(  5,  42),   S(-31,  46),   S(-36,  62),   S(-17,  89),

            /* bishops: bucket 2 */
            S( 10,  36),   S( -8,  46),   S( -6,  35),   S(-27,  51),   S(-16,  39),   S(-21,  24),   S(-27,  -3),   S(-40,  48),
            S(-20,  24),   S(  5,  24),   S( 10,  24),   S(  2,  33),   S( -5,  38),   S( 14,   7),   S(  7,   7),   S(  8, -24),
            S( -6,  26),   S( -4,  35),   S( 10,  59),   S( -4,  59),   S(  0,  52),   S(  8,  45),   S(  8,  37),   S(-10,   4),
            S( 13,  29),   S(-20,  61),   S(-23,  67),   S( -1,  66),   S(  1,  63),   S(  0,  53),   S( 17,  52),   S( 12,   5),
            S( -7,  36),   S( -4,  41),   S(-24,  55),   S(-31,  72),   S(-22,  77),   S( -5,  67),   S(  9,  40),   S(-30,  21),
            S(-13,  33),   S(-28,  47),   S(-20,  74),   S(-47,  68),   S( 19,  38),   S(-12,  73),   S( -6,  71),   S( -1,  38),
            S(-13,  29),   S(-17,  61),   S(-39,  73),   S( -6,  41),   S(-45,  61),   S(-23,  48),   S(-87,  82),   S(-29,  45),
            S(-137, 104),  S(-38,  76),   S(-106,  70),  S(-56,  63),   S(-63,  66),   S(-67,  49),   S(  3,  36),   S(-39,  94),

            /* bishops: bucket 3 */
            S( -5,  38),   S( -1,  41),   S( 10,  32),   S( -8,  44),   S( -8,  35),   S( 39,   1),   S( 24,  -4),   S( 28, -21),
            S( -2,  27),   S(  0,  40),   S( 10,  21),   S(  5,  51),   S(  9,  33),   S( 14,  39),   S( 47,  37),   S( 24,  11),
            S( 15,  25),   S( -1,  56),   S(  4,  73),   S( 11,  58),   S(  8,  77),   S( 13,  68),   S( 27,  40),   S( 40,  12),
            S( 27,  31),   S(  2,  63),   S( -5,  78),   S( 14,  77),   S( 22,  67),   S( 21,  50),   S( 17,  64),   S( 18,   9),
            S(  1,  38),   S( 22,  41),   S( 12,  56),   S( 12,  73),   S( 12,  75),   S( 23,  66),   S( 17,  49),   S( 12,  53),
            S(  1,  49),   S( 21,  46),   S( 11,  62),   S( 14,  47),   S(-11,  73),   S( 46,  57),   S( 29,  49),   S(  1,  72),
            S(-13,  44),   S( -4,  62),   S( 23,  41),   S(  6,  50),   S(  0,  47),   S(-36,  70),   S( 12,  72),   S( -3,  71),
            S(-26, 102),   S(-23,  62),   S( 35,  42),   S(-24,  66),   S(-25,  66),   S(-80,  91),   S( -4,  57),   S( 66,  37),

            /* bishops: bucket 4 */
            S(-17, -17),   S(-44,  -8),   S(-61,  10),   S(-41,  20),   S( -2,   7),   S(-22,  18),   S(-14,  -3),   S( -2,  -8),
            S(-28,  24),   S(  9,  -4),   S(  8,  40),   S(-18,  12),   S(-28,  14),   S( 80, -31),   S( 18,  -9),   S( 22, -23),
            S(-25,  13),   S(-50,  34),   S( 15,  15),   S(-15,  24),   S( 28,  17),   S(  9,  12),   S(-24, -33),   S(-78, -15),
            S(  5,  41),   S( -7,  24),   S( 76,   8),   S( 42,  30),   S(-32,  56),   S( 53,  23),   S(-14,  55),   S( -3, -32),
            S( 21,  32),   S( -2,  41),   S(-30,  60),   S( 26,  36),   S( 26,  13),   S(-46,  28),   S(-43,  20),   S(-32,  -5),
            S( -6,  44),   S( 26,  26),   S(  6,  17),   S( 55,  17),   S( 19,  41),   S( 13,  17),   S(  3,   9),   S(-11,  14),
            S(-21,  40),   S( 23,  53),   S( -2,  21),   S( -6,  67),   S( 11,  25),   S(-30,   9),   S(  2,  -4),   S( 11,   0),
            S( -7,   5),   S(-33, -25),   S( -6,   2),   S( -3,  15),   S( -5,  38),   S(  7,  18),   S(  3,  -5),   S( -1,  32),

            /* bishops: bucket 5 */
            S( -6,  17),   S( -4,  26),   S(-45,  34),   S(-17,  32),   S(-36,  39),   S(-56,  19),   S(-21,  38),   S( 10,  41),
            S(-33,  41),   S(-11,  35),   S(-34,  46),   S( 32,  22),   S(  3,  29),   S(-39,  41),   S(-20,   5),   S(-29,  12),
            S(-43,  43),   S(-26,  46),   S( 22,  34),   S(-18,  45),   S( 34,  27),   S(-14,  26),   S(-44,  51),   S(-20, -10),
            S( 10,  31),   S( 37,  37),   S(  0,  43),   S( 23,  43),   S( 58,  32),   S( 27,  37),   S(-20,  35),   S(-13,  39),
            S( 53,  40),   S( 33,  24),   S( 59,  33),   S(119,  22),   S( 51,  27),   S( 45,  25),   S( 68,  27),   S(-37,  34),
            S( 14,  37),   S( 52,  45),   S( 65,  44),   S( 27,  46),   S(-14,  47),   S( 27,  17),   S(  8,  31),   S(  2,  53),
            S( 16,  35),   S(-53,  63),   S(  6,  38),   S( 13,  37),   S(  3,  41),   S( 17,  64),   S(  7,  52),   S( -2,  27),
            S(-23,  30),   S( 12,  33),   S(-12,  26),   S(-12,  44),   S(  5,  43),   S(  0,  62),   S(  3,  65),   S( -2,  33),

            /* bishops: bucket 6 */
            S( -3,  68),   S( -5,  24),   S(-32,  38),   S(-54,  29),   S(-29,  21),   S(-64,  47),   S( -3,  50),   S(  7,  25),
            S(  8,  19),   S( 15,  15),   S( -6,  29),   S(  2,  34),   S(  8,  28),   S(-18,  34),   S(-98,  73),   S( 24,  18),
            S( 35,   7),   S(  0,  29),   S( -3,  56),   S( 37,  33),   S( 18,  41),   S( 51,  19),   S(  5,  45),   S(-53,  17),
            S( 21,  31),   S(-14,  59),   S(  4,  50),   S( 62,  38),   S( 15,  46),   S( 29,  40),   S(  7,  57),   S(-34,  25),
            S(-34,  52),   S( 13,  49),   S( 67,  26),   S( 88,  21),   S(126,  40),   S( 65,  35),   S( 45,  38),   S( -6,  33),
            S(  8,  22),   S(  2,  39),   S( 14,  44),   S( 18,  55),   S( 31,  46),   S( 82,  43),   S( 43,  35),   S(-32,  55),
            S(-12,  33),   S( 16,  40),   S( 55,  29),   S(-13,  35),   S( 25,  39),   S(  7,  41),   S( 13,  62),   S(-11,  56),
            S(  3,  70),   S(-17,  57),   S(-14,  36),   S( 16,  48),   S( -5,  27),   S( -4,  41),   S(  1,  37),   S(  9,  51),

            /* bishops: bucket 7 */
            S( 17, -11),   S(-13,   7),   S(-41, -18),   S(  5, -12),   S(-33,  -4),   S(-50,   8),   S(-56, -42),   S(-33, -11),
            S(-32,  -8),   S(-45,   5),   S(-16,   2),   S(  5,   4),   S(-18,   8),   S(-44,  28),   S(-14,  -9),   S(-43, -27),
            S(-44,   9),   S(  2, -12),   S( 41,   5),   S( 29,  25),   S(-20,  25),   S(  6,   3),   S(-11,  38),   S(-14,  11),
            S(-59,  23),   S( 15,  30),   S( 72,  -3),   S( 63,  17),   S(115,  -1),   S( 10,  17),   S( 58,  38),   S( 27,  16),
            S( -7,  -3),   S(-16,   8),   S( 85, -16),   S( 95,  -3),   S(106,   0),   S( 86,  15),   S( 20,  47),   S( 45,   5),
            S(-21, -10),   S(-27,  15),   S( 32,   1),   S( 52,   1),   S( 24,  17),   S( 89,  26),   S( 47,  17),   S(  9,  12),
            S(-31, -10),   S( -6,  12),   S(-14,  11),   S(  1,  23),   S(  6,  -5),   S( 47,   2),   S( 39,  17),   S(  7,  28),
            S( -8,   7),   S(-29,  12),   S(-23,   8),   S(-12,  12),   S( -7,   5),   S(  4,  11),   S( 10,  12),   S( 34,  42),

            /* bishops: bucket 8 */
            S( -7, -39),   S(-13, -63),   S(-38, -30),   S( -4, -33),   S(  6,  -7),   S(-17,  -2),   S(  7,  -4),   S(-10, -30),
            S( -1,  -2),   S(-18, -73),   S( -7, -25),   S( -7, -14),   S( 29,   3),   S(-17, -40),   S( -9, -52),   S(  1, -29),
            S( -1, -14),   S( -7,  15),   S(  5,  -9),   S( 26, -28),   S(  9,  -1),   S( -1, -50),   S( 12, -51),   S(-18, -39),
            S(  4,  35),   S(  0,  -2),   S( 11,  21),   S( 21, -14),   S( 20,   4),   S(  8, -33),   S(  1, -32),   S(  3, -32),
            S( 22,  33),   S( 40,  33),   S( 21,  -5),   S( 76,  19),   S( 42, -14),   S( 23,  -9),   S(  4, -16),   S( -9, -43),
            S(-18, -13),   S(  3,   2),   S( 31,   0),   S( 27, -12),   S( 16, -17),   S( 13, -10),   S(  4, -24),   S(-15, -42),
            S( -7,  -8),   S( 26, -18),   S(  2,  -3),   S(  3,  -3),   S(  0,  -4),   S(  3, -31),   S(  2, -19),   S(-14, -48),
            S( -6,  -6),   S( -3, -44),   S( -6, -25),   S( -4, -23),   S(-13, -42),   S( -3, -55),   S(-11, -36),   S( -1, -16),

            /* bishops: bucket 9 */
            S(-22, -72),   S( 14, -37),   S(-16,  -8),   S( -2, -30),   S(-21, -51),   S( -3, -40),   S(-16, -49),   S(  4,   1),
            S( -6, -33),   S(-14, -37),   S(-14, -38),   S( 13, -20),   S(-29, -24),   S( -6, -28),   S( -7, -43),   S( -8, -29),
            S( -1, -11),   S( 10, -21),   S( -5, -19),   S( 35, -13),   S( 25, -10),   S( 18, -46),   S(-15, -37),   S( -4, -34),
            S(-12, -11),   S( 20,  13),   S( 24,  -4),   S( 52, -10),   S( 45, -12),   S( 27, -24),   S( 27, -46),   S(-19, -64),
            S(  0, -24),   S( 20,   1),   S( 33,  -1),   S( 46,   0),   S( 45, -29),   S( 23, -19),   S( 21, -11),   S(  6,  -2),
            S( -5, -31),   S( 48,   8),   S( -2,  16),   S( 32,  13),   S( 42, -17),   S( 27, -39),   S(  5, -53),   S(-10, -29),
            S(  6,  -7),   S( 31,  12),   S(  6,  -9),   S( 27,  -6),   S( 28, -10),   S(  6, -20),   S( -3, -37),   S(-13, -48),
            S(  1,  -4),   S(  4,  -9),   S(  4, -50),   S( -6, -46),   S(-11, -25),   S( 11,  -3),   S(  8, -24),   S(-14, -65),

            /* bishops: bucket 10 */
            S( -4, -32),   S(  3, -55),   S(-37, -29),   S(-27, -39),   S(-15, -33),   S(-22, -62),   S( -9, -68),   S(-19, -104),
            S(  3, -44),   S(-10, -43),   S(  8, -42),   S( -2, -46),   S(  1, -50),   S(  7, -28),   S(-19, -61),   S(-14, -55),
            S( -1, -57),   S( 16, -42),   S(  6, -51),   S( 57, -33),   S( 43, -49),   S(  8, -16),   S( -5,  -7),   S(  4,   0),
            S(-11, -64),   S( 21, -19),   S( 33, -43),   S( 54, -22),   S( 56, -22),   S( 15,   1),   S( -1, -34),   S( 22,  16),
            S(-17, -39),   S( 23, -41),   S( 35, -45),   S( 71, -12),   S( 33,  12),   S( 41, -14),   S( 28,  16),   S( -8, -41),
            S(  7, -25),   S( 12, -61),   S( 17, -42),   S( 16, -45),   S( 26,  -8),   S( 44,   3),   S( 17, -24),   S( -7, -29),
            S(-10, -50),   S( -2, -58),   S( -2, -66),   S( 29, -32),   S(-13, -29),   S( 15, -20),   S( 20,  21),   S( 10,  14),
            S( -4, -66),   S(  0, -39),   S( 11,  20),   S( -6, -26),   S( -7, -33),   S( -1, -32),   S(  0,   7),   S(  6,  19),

            /* bishops: bucket 11 */
            S(-28, -28),   S(-48, -12),   S(-46, -58),   S( -5, -30),   S(-26, -29),   S(-52, -66),   S(-20, -54),   S(-32, -74),
            S(  2, -35),   S(  5, -45),   S( -4, -51),   S(-26, -44),   S(-22, -47),   S(-32, -70),   S( -9, -53),   S(-19, -22),
            S( -2, -79),   S(-10, -47),   S( 12, -27),   S( 32, -36),   S(-10, -29),   S( -8, -26),   S(-15,   0),   S(-11, -28),
            S( -8, -53),   S(  6, -57),   S( 27, -46),   S( 32, -41),   S( 63, -40),   S( 30,  -4),   S( 30, -23),   S(  8,  29),
            S(  8, -18),   S( -8, -50),   S(  7, -29),   S( 56, -37),   S( 51,  -7),   S( 33,  -3),   S( 10,  12),   S(  4, -27),
            S(-11, -85),   S( 18, -60),   S( 13, -40),   S( 11, -19),   S( 27,  -9),   S( 46,  18),   S( 21,  37),   S(-14, -28),
            S( -7, -32),   S(-16, -68),   S( -7, -47),   S(  2, -60),   S( 15, -23),   S( 54, -26),   S(  6, -15),   S( -3, -16),
            S(-13, -74),   S(-13, -40),   S(  3, -19),   S( 17,  -9),   S( 13, -34),   S(-15, -56),   S( -3, -10),   S(  4,   3),

            /* bishops: bucket 12 */
            S(  2,  -2),   S( -7, -42),   S(-13, -69),   S(-10, -55),   S(  0, -38),   S( -8, -28),   S(  4,  11),   S( -1,  -5),
            S( -6, -28),   S( -8, -46),   S(  4,  -6),   S(-10, -36),   S(-10, -25),   S(  5,  10),   S(  0,  -8),   S(  6,  23),
            S(  2, -15),   S( -4, -13),   S(-11, -35),   S(  4, -17),   S( -3,  -1),   S( -2, -42),   S(-11, -32),   S( -1, -15),
            S( -5, -19),   S( -4, -31),   S( -7, -36),   S( -3,  -9),   S(  7,  -4),   S(  9,  15),   S( -9, -30),   S( -7, -16),
            S(-11, -28),   S( -5,   1),   S(  5,  -4),   S(  1,  11),   S( -6, -58),   S(  8, -17),   S(  2, -33),   S( -1, -14),
            S(-21, -37),   S( 11,  46),   S( -6,  -5),   S(-11, -35),   S( -1, -54),   S( -3,  -7),   S(  1, -29),   S( -1, -19),
            S( -1,  -1),   S(  5,  23),   S(  3,  30),   S(-12, -36),   S(  9,  12),   S(  9,   2),   S(-10, -39),   S(  0, -10),
            S(  1,  -1),   S(  2,  28),   S( -4, -47),   S(  0, -14),   S(  1, -21),   S( -2, -25),   S( -3, -21),   S( -4, -18),

            /* bishops: bucket 13 */
            S(-18, -75),   S(-12, -71),   S( -8, -47),   S(-16, -68),   S(-11, -44),   S( -5, -18),   S( -4, -10),   S( -8, -37),
            S(  0, -15),   S( -1, -41),   S( -4, -48),   S(-15, -56),   S(  6,   5),   S( -3, -27),   S( -6, -44),   S( -6, -54),
            S( -6, -41),   S(  0, -23),   S(  5,  -7),   S( -7, -73),   S(  7, -52),   S(  9, -17),   S( -9, -29),   S( 11,  36),
            S( -4, -15),   S( -4, -37),   S(  2, -32),   S(-12, -60),   S( 20, -38),   S(  7,  -1),   S(  8,  -4),   S( -9, -44),
            S(  2,   8),   S(  3,   2),   S(  2, -26),   S( 21,  -1),   S(  6,  -2),   S( 11, -31),   S(  2, -38),   S(  0, -13),
            S( -4, -10),   S(-10, -18),   S( -2,  -1),   S(  8,  -3),   S( 12, -10),   S( 18, -11),   S( 17,  -6),   S( -4, -30),
            S( -3, -15),   S(  3, -14),   S( 11,   9),   S( -9,  -1),   S( -5, -25),   S(  6,  13),   S( -8, -61),   S(  0, -17),
            S( -9, -46),   S( -5, -30),   S(  2, -23),   S( 10,  -1),   S(  5,  10),   S( -1, -20),   S( -3,   9),   S( -4, -47),

            /* bishops: bucket 14 */
            S( -4, -43),   S(-10, -69),   S( -7, -50),   S(-14, -62),   S(-12, -65),   S( -3, -44),   S(-14, -82),   S(-13, -55),
            S(-11, -28),   S(  4,  -6),   S( -5, -37),   S(-19, -77),   S(-16, -65),   S(-11, -73),   S(-15, -74),   S(  1, -15),
            S( -1,   0),   S( -8, -45),   S( -1, -34),   S( -4, -72),   S(-14, -83),   S( -7, -71),   S( -3, -21),   S( -6, -31),
            S( -8, -26),   S(  1, -19),   S(-14, -49),   S(  4, -20),   S( 16, -45),   S(  1, -49),   S(-15, -72),   S( -4, -13),
            S(-10, -38),   S(  4, -16),   S( -4, -64),   S(  8, -46),   S(  7, -55),   S( 14, -24),   S( -3, -26),   S(  2, -14),
            S( -3, -21),   S(  4, -22),   S( -2, -20),   S(  2,   2),   S(  8, -16),   S(-10,  -5),   S( -7, -62),   S(-14, -56),
            S( -4, -44),   S( 13, -15),   S( -3, -27),   S(  6,   4),   S(-15, -41),   S( -3, -28),   S( -6, -13),   S( -3, -13),
            S( -4, -33),   S( -8, -36),   S(  1, -16),   S( -6, -22),   S( -8, -29),   S( -2,   3),   S(  0,  14),   S( -2, -17),

            /* bishops: bucket 15 */
            S(  5,  26),   S( -1,  16),   S(-17, -44),   S( -7, -38),   S( -5,  -6),   S(-13, -34),   S( -2,  -6),   S( -4, -19),
            S( -2, -19),   S(  3,  10),   S( 10,  -8),   S( -1, -27),   S(-16, -48),   S( -8, -15),   S( -2, -18),   S( -4, -13),
            S(-12, -60),   S(  1, -11),   S(-14, -49),   S( -7,  12),   S(-13, -55),   S(-10, -14),   S( -3, -18),   S(  2,   3),
            S( -5, -27),   S(-14, -62),   S(  5,  10),   S(-15, -80),   S(-12, -63),   S( -6, -48),   S(  0,   0),   S( -2, -11),
            S(  0, -24),   S(-13, -44),   S( -8, -63),   S(-17, -41),   S( -1, -28),   S(  1,  -9),   S(  4, -20),   S( -5, -22),
            S( -9, -32),   S( -3, -53),   S(-11, -44),   S(-16, -65),   S( -3, -36),   S(-10,  -1),   S( 21,  11),   S( -2,  -2),
            S( -1, -26),   S( -4, -37),   S( -3, -29),   S( -2, -30),   S( -6, -26),   S( -3, -13),   S( -8, -32),   S(  8,  34),
            S( -3, -13),   S( -2, -14),   S( -1, -20),   S( -6, -54),   S( -4, -46),   S(-21, -69),   S(-16, -44),   S(  0,  -1),

            /* rooks: bucket 0 */
            S(-25,   8),   S(  5, -18),   S(  7, -28),   S(  9, -11),   S( 11,   0),   S( 14, -25),   S( 11,  -3),   S( 16,  -6),
            S(  5, -65),   S( 38, -37),   S(  5,   8),   S( 12,  -8),   S( 52, -31),   S( 27, -18),   S(-23,   3),   S(-39,  36),
            S( 18, -18),   S( 30,  20),   S( 26,   2),   S( 14,   4),   S(  0,  23),   S( 22,  -7),   S(-19,  -6),   S(-34,  -1),
            S( 45, -28),   S( 88, -10),   S( 67,  18),   S( 70, -18),   S( 29,  -3),   S(-10,  15),   S(-15,  32),   S(-11,  12),
            S( 70, -31),   S(105, -25),   S( 75,  -6),   S( 52,   0),   S( 33,  14),   S( 32,   6),   S( -5,  21),   S(-16,  22),
            S( 94, -67),   S( 89, -39),   S( 82,  -2),   S( 21,  26),   S( 58,  16),   S(-13,  16),   S( 50,  -3),   S(-28,  37),
            S( 54, -21),   S( 85, -11),   S( 31,  22),   S( 38,  20),   S( -9,  39),   S( -4,  16),   S( -3,  22),   S(  1,  22),
            S( 41,  19),   S(-17,  41),   S( 39,  12),   S( 36,  20),   S( 46,   8),   S(  4,  21),   S( 34,  31),   S( 36,   2),

            /* rooks: bucket 1 */
            S(-75,  41),   S(-31,  -3),   S(-47,   7),   S(-33,  -9),   S( -9, -11),   S(-18,  -8),   S(-21,  -6),   S(-27,  15),
            S(-47,  28),   S(-87,  38),   S(-17,  -3),   S(-15, -27),   S(-36,   9),   S(-30,   0),   S(-49,  -9),   S(-56,   8),
            S(  4,   7),   S(-24,  46),   S(-13,  23),   S(-38,  31),   S(-37,  22),   S( -3,  14),   S(-28,  13),   S(-43,  22),
            S(-26,  44),   S(-41,  50),   S( -4,  47),   S(  1,  21),   S(-38,  52),   S(-32,  37),   S(-19,  52),   S(-41,  26),
            S( 77,   5),   S( 55,  26),   S( 35,   9),   S(-18,  41),   S(-26,  53),   S( 36,  17),   S(-16,  31),   S(-42,  17),
            S( 80,  15),   S(-29,  58),   S( 52,  20),   S( -4,  25),   S( 31,  11),   S(-42,  56),   S(-14,  24),   S(-57,  55),
            S(  6,  28),   S( 24,  35),   S( 60,  25),   S(-103,  90),  S(-52,  47),   S( 20,  43),   S(-43,  47),   S(-35,  31),
            S( 35,  21),   S( 21,  44),   S( 28,  32),   S(-40,  57),   S(  2,  37),   S( 79,  10),   S(-16,  28),   S( 43,   5),

            /* rooks: bucket 2 */
            S(-63,  44),   S(-42,  28),   S(-40,  25),   S(-52,  26),   S(-53,  24),   S(-53,  14),   S(-42,  -7),   S(-50,  35),
            S(-69,  46),   S(-71,  51),   S(-43,  38),   S(-50,  24),   S(-50,  21),   S(-59,  10),   S(-88,  40),   S(-54,  16),
            S(-61,  66),   S(-57,  69),   S(-43,  56),   S(-34,  29),   S(-49,  40),   S(-47,  45),   S(-24,  25),   S(-32,  31),
            S(-62,  68),   S(-67,  84),   S(-41,  80),   S(-31,  68),   S(-41,  62),   S( -2,  42),   S(-32,  65),   S(-19,  36),
            S(-34,  69),   S(-48,  80),   S(-27,  65),   S(-25,  59),   S( 45,  43),   S( 13,  50),   S(-15,  59),   S(-40,  52),
            S(-46,  67),   S(-24,  61),   S(-21,  50),   S(  1,  45),   S( 23,  39),   S( 80,  22),   S( 31,  21),   S(-30,  47),
            S(-63,  69),   S(-63,  89),   S(-24,  68),   S( -4,  66),   S( 41,  32),   S( 57,  16),   S(-62,  86),   S(-21,  58),
            S( -5,  65),   S(-12,  56),   S(-33,  60),   S( -4,  45),   S(-24,  53),   S( -2,  50),   S( -9,  59),   S(  3,  39),

            /* rooks: bucket 3 */
            S(  2,  78),   S(  4,  73),   S( 12,  65),   S( 20,  53),   S( 18,  51),   S(  0,  63),   S(  4,  59),   S( -4,  44),
            S(-26,  76),   S(-11,  70),   S(  8,  63),   S( 18,  61),   S( 23,  56),   S( 17,  52),   S( 39,   9),   S( 24, -31),
            S(-31,  89),   S( -1,  83),   S( 10,  82),   S( 18,  64),   S( 24,  70),   S( 30,  65),   S( 34,  51),   S( 10,  48),
            S(-24,  89),   S(-25, 104),   S( 17,  86),   S( 37,  72),   S( 38,  66),   S(  9, 111),   S( 79,  57),   S( 33,  56),
            S( -6,  99),   S( 27,  90),   S( 31,  72),   S( 52,  68),   S( 51,  77),   S( 55,  68),   S(101,  50),   S( 66,  48),
            S( -8,  98),   S(  8,  92),   S( 18,  87),   S( 37,  72),   S( 40,  58),   S( 71,  46),   S( 93,  43),   S( 90,  23),
            S(-20, 111),   S(-10, 117),   S( -1, 107),   S( 40,  86),   S( 18,  81),   S( 59,  60),   S( 45,  83),   S(131,  21),
            S(-57, 146),   S( 21, 100),   S(  7,  92),   S( 64,  74),   S( 45,  72),   S( 74,  59),   S(141,  28),   S( 99,  57),

            /* rooks: bucket 4 */
            S(-91,  14),   S(-17, -36),   S(-48,  15),   S( 13,   1),   S(-25, -27),   S(  4, -28),   S(  7, -36),   S(-18, -51),
            S(-33, -18),   S(-35,  -7),   S(-47,  20),   S(-40,  25),   S(-27,  -5),   S(-54, -30),   S( 22, -56),   S(-73,   4),
            S(-30,  13),   S(-60, -20),   S( -6,   8),   S( -7, -24),   S(-45,  11),   S(  3, -24),   S( 52, -23),   S(-51,  -2),
            S(-24, -33),   S( 20,   7),   S(-29,  12),   S( -3,   5),   S( 47, -24),   S(-16, -11),   S(-46,   4),   S(-39,  13),
            S(-44, -15),   S(-13,  17),   S(  8,  -2),   S( 21,  21),   S( 56,  -6),   S( -3,   5),   S( 52,  -2),   S( 47, -18),
            S(  6,   7),   S( 28,  -2),   S( 75,  18),   S( 18,   1),   S( 30,  14),   S( 29,  13),   S( 11,  20),   S( -2,  21),
            S( -3,   7),   S( 25,  33),   S( 25,  17),   S( 33,   4),   S( 75,   2),   S(  8,  -7),   S( 27,   4),   S( 48,  22),
            S( 11, -41),   S( 47,  37),   S( 22,  11),   S(  9,  14),   S( 24,  -5),   S( 18,  12),   S( 30,  20),   S( 14,  15),

            /* rooks: bucket 5 */
            S(-30,  28),   S(  2,  19),   S(-44,  43),   S(-30,   8),   S(-25,  10),   S(-31,  38),   S( 31,   7),   S(-36,  42),
            S( -6,  17),   S(-59,  49),   S(-89,  70),   S(-90,  53),   S(-54,  47),   S(-11,  16),   S( 45,   8),   S(-20,   3),
            S(-20,  38),   S(-34,  44),   S(-79,  62),   S(-99,  55),   S(-30,  29),   S(-12,  30),   S( -6,  37),   S(-26,  18),
            S(-41,  62),   S(-16,  53),   S(  1,  51),   S(-21,  46),   S(-16,  44),   S( 11,  49),   S(-22,  48),   S( -1,  23),
            S(-18,  65),   S(-18,  72),   S(  5,  59),   S( 16,  73),   S( 13,  55),   S( 16,  70),   S( 65,  43),   S( 33,  25),
            S( 69,  52),   S( 35,  60),   S( 59,  59),   S( 22,  79),   S( 49,  53),   S( 56,  48),   S( 37,  49),   S( 64,  42),
            S( -5,  66),   S( 49,  62),   S( 52,  35),   S( 43,  71),   S( 46,  41),   S( 46,  32),   S( 98,  32),   S( 69,  43),
            S( 70,  37),   S( 89,  15),   S( 67,  55),   S( 34,  43),   S( 53,  25),   S( 53,  39),   S( 54,  34),   S( 13,  54),

            /* rooks: bucket 6 */
            S(-50,  33),   S(-24,  37),   S(  2,  24),   S(-25,  26),   S(-17,  11),   S(-65,  49),   S(-14,  34),   S( -7,  39),
            S( -6,  17),   S(  6,  25),   S(-28,  37),   S(-52,  36),   S(-11,  36),   S(-65,  54),   S(-40,  47),   S( 16,  13),
            S(-54,  58),   S(-63,  62),   S( 22,  26),   S(-63,  55),   S(-18,  39),   S(-49,  60),   S(-49,  61),   S( 43,   7),
            S(-47,  64),   S(  1,  62),   S(  8,  58),   S(-11,  58),   S(-28,  60),   S(-15,  54),   S(-75,  84),   S(-15,  53),
            S(-10,  72),   S( 21,  70),   S( 60,  47),   S( 18,  47),   S(-35,  89),   S( 15,  73),   S(  6,  61),   S(  6,  56),
            S(  0,  64),   S( 55,  60),   S( 92,  38),   S( 25,  44),   S( 35,  51),   S( 29,  70),   S( 56,  45),   S( 77,  49),
            S( 35,  61),   S( 84,  49),   S(127,  29),   S(119,  24),   S(113,  24),   S( 50,  46),   S( 87,  43),   S( 45,  57),
            S( 90,  53),   S( 53,  42),   S( 31,  48),   S( 38,  50),   S( 61,  38),   S( 72,  46),   S( 91,  37),   S( 39,  45),

            /* rooks: bucket 7 */
            S(-62,  -1),   S(-44,  -2),   S(-28, -17),   S(-20,   0),   S(  9, -19),   S(-22,   4),   S(-72,  28),   S( 25, -25),
            S(-83,  34),   S( -6,   1),   S(-19,  -4),   S(  4, -20),   S( 23, -12),   S( 18,  -6),   S(-17,   9),   S(-21,  -9),
            S(-92,  50),   S(-25,   4),   S(-36,  31),   S( 12, -16),   S( 12,   4),   S(-32,   8),   S(-13, -13),   S(-25,  25),
            S(-78,  38),   S(-18,  32),   S(-16,  33),   S( 28,  11),   S( 28,   8),   S( 56,   4),   S( 69,  -2),   S(-41,  23),
            S(-10,  35),   S(-16,  30),   S( 42,   8),   S( 57,  -1),   S( 74,   6),   S( 94,  12),   S( 47,  33),   S( 45, -18),
            S(-22,  37),   S( 16,  25),   S( 85,  -8),   S( 91, -11),   S( 60,   4),   S( 65,  21),   S( 72,  17),   S( 70, -11),
            S( -5,  38),   S( 32,  22),   S( 61,  12),   S( 86,   1),   S(104,  -5),   S(102,  -7),   S( 71,  29),   S( 58,   4),
            S( 32,  44),   S(-15,  38),   S( 25,  12),   S( 63,  -1),   S( 43,   9),   S( 35,  13),   S( 81,  -9),   S( 64, -10),

            /* rooks: bucket 8 */
            S(-39, -49),   S(-27, -30),   S( -9,  14),   S(-33, -30),   S(-15, -50),   S(-29, -39),   S(-30, -35),   S(-20, -24),
            S( -9, -16),   S( -4, -12),   S(-10,  20),   S( -4, -26),   S(-50, -25),   S( -8, -22),   S(-20, -41),   S(-19, -66),
            S(  5,   6),   S( -5, -31),   S( 10,  -1),   S(  2,  25),   S(-22, -41),   S( -8, -32),   S( -3,  20),   S( -5,   0),
            S(-22, -17),   S(-25,   2),   S(-15,   3),   S(  3,  14),   S(  8,  27),   S(-21, -19),   S( -8, -30),   S(-18, -32),
            S( -3,  -4),   S( -1,  15),   S(-10,  44),   S( -2,  11),   S(-14, -16),   S(  3,  12),   S(-15, -18),   S(-21, -44),
            S( 13,  40),   S(-12,  10),   S( 13,  38),   S( 19,  15),   S( -1, -24),   S( -1,  -9),   S( -5, -12),   S(  3,  10),
            S( -4,   1),   S( -9,   6),   S( 17,  26),   S( 29,  14),   S( 29,   8),   S(  8, -19),   S(  8,  -8),   S( -6, -19),
            S( -2, -110),  S(  7,   0),   S( 26,   5),   S( -5, -41),   S(-10, -21),   S( -8, -56),   S( 13, -11),   S(  4,   4),

            /* rooks: bucket 9 */
            S(-57, -38),   S(  0, -38),   S(-53, -45),   S(-55, -24),   S(-46,  -7),   S(-33, -24),   S( -6, -45),   S(-58, -26),
            S( 15, -17),   S(-18, -44),   S(-32, -51),   S(-40, -23),   S(-17, -32),   S( 14,   1),   S(-23, -38),   S(-25, -49),
            S( -5, -18),   S( 17, -11),   S(  1,   3),   S(-30, -29),   S(-38, -23),   S( 14, -13),   S( 15, -14),   S(-19, -54),
            S( -2, -14),   S( 10,   8),   S( -7,  -1),   S(-29, -13),   S(  3,   0),   S(  6, -14),   S( 19,  16),   S( -7, -14),
            S( -2,   0),   S(-32,  10),   S(  7,  12),   S(  0,  22),   S(  1, -14),   S(  9, -11),   S( -4, -11),   S( -9, -52),
            S( 11,  24),   S(-15,   3),   S(-15, -16),   S(-31, -15),   S( 15, -20),   S( 19, -15),   S( -1,  -7),   S( -5, -24),
            S( 34,  -7),   S( 47,  -8),   S( 21,  -3),   S( 26,  -7),   S(  7, -14),   S( 54,  -5),   S( 24, -24),   S( 37,   0),
            S( 42, -81),   S( 35, -48),   S( 24, -19),   S(  7,  15),   S( 13,  18),   S( 19,   4),   S( 15, -18),   S( 42,  -4),

            /* rooks: bucket 10 */
            S(-49, -90),   S(-24, -59),   S(-54, -51),   S(-32, -34),   S(-40, -53),   S(-33, -42),   S(  8, -53),   S(-42, -33),
            S( -5, -45),   S( -5, -42),   S(-13, -55),   S(-28, -15),   S(-11, -33),   S(-26, -29),   S(  4, -14),   S(-12, -45),
            S(-46, -41),   S(-36, -44),   S(-29, -45),   S(-16, -13),   S(-38, -14),   S( -8,  -9),   S( 22,   8),   S( -2,  -3),
            S(-16,  -6),   S(  4, -23),   S(-22, -29),   S( -8,  -9),   S(  4,  -9),   S(-21, -10),   S(  4,  10),   S(  6, -21),
            S(  1, -22),   S( 17, -25),   S(-10, -24),   S(  8, -36),   S(-20, -21),   S( -2, -17),   S( 18,   7),   S(  0,  16),
            S( 37, -18),   S( 26,  35),   S( -1,  -7),   S( 14, -11),   S( -1, -31),   S( -4, -25),   S( 24,  -5),   S(  4, -15),
            S( 60, -12),   S( 69, -14),   S( 44, -13),   S( 40, -30),   S( 36, -41),   S( 21, -18),   S( 19, -29),   S( 16, -39),
            S( 43,  -9),   S( 11, -21),   S( 21, -22),   S( 19, -18),   S( 18, -25),   S( 10, -21),   S( 13, -41),   S( -4, -47),

            /* rooks: bucket 11 */
            S(-52, -43),   S(-36, -29),   S(-17, -34),   S(-44, -62),   S(-50, -22),   S(-11,  -7),   S(-52, -38),   S(-54, -27),
            S(-51, -35),   S(-10, -47),   S(-16, -14),   S(-32, -26),   S(-29, -26),   S(-36, -27),   S(  2,   2),   S(-23, -30),
            S(-18, -30),   S(  6, -21),   S(-12, -39),   S( -4, -17),   S( -5, -43),   S(-20,  10),   S(-31, -32),   S(-34, -48),
            S(-16,  21),   S( -3, -24),   S(-27, -13),   S( 22,   5),   S( -2,   3),   S(-12,  35),   S( 10,  -5),   S(  1, -21),
            S(-19,   6),   S(  3, -32),   S( 27,   0),   S( 30, -15),   S( 24,  -9),   S( 22,  -2),   S( 11, -19),   S( -7,  -9),
            S( 11,  25),   S( 25,  13),   S(  4,  -9),   S( 30, -12),   S( 38,   7),   S( 23, -12),   S(-16,  14),   S( 22,   5),
            S( 48,  32),   S( 19,   9),   S( 54,  -9),   S( 43, -15),   S( 40,  -6),   S( 46,  20),   S( 17,  33),   S( 32, -38),
            S( 19,  28),   S(  0,  24),   S( 13, -10),   S(  3, -17),   S(  4,  -8),   S( -1,  -8),   S(  1,  12),   S( 16,   9),

            /* rooks: bucket 12 */
            S(  6, -55),   S(-11, -25),   S(-27, -39),   S( -7, -24),   S( -5, -39),   S( -4, -34),   S(-18, -64),   S(-18, -26),
            S(  5, -11),   S(-10,  -1),   S(-11, -13),   S(-17, -24),   S(-13, -14),   S(-14, -21),   S(-13, -53),   S( -2, -33),
            S( 10,  -2),   S(-17, -30),   S( -2,  -5),   S(-12, -19),   S( -6, -24),   S( -2, -20),   S(  6,  11),   S( -2, -27),
            S(-12, -15),   S( -6, -21),   S( 22,  56),   S(  8,  -6),   S( -1,  -4),   S( -7, -32),   S(-16, -51),   S(  3, -15),
            S(-18, -21),   S(-22, -27),   S( 13,  12),   S( -3,  -8),   S( -9, -29),   S(  1, -18),   S(-11, -40),   S(  8,  13),
            S(-12, -40),   S(  2, -12),   S( 18,  27),   S(  0, -34),   S( -6, -30),   S( -8, -28),   S( -2, -24),   S(  4,  16),
            S( -1,  -6),   S( -1, -10),   S( -8, -49),   S(  1, -19),   S( 11,   5),   S( -7, -57),   S( -1, -19),   S(  6,   3),
            S(-11, -40),   S(  4,  -2),   S(  2, -16),   S( -3, -27),   S( -2, -27),   S(-16, -49),   S(-11, -23),   S(  2,  -2),

            /* rooks: bucket 13 */
            S(-16, -68),   S(-11, -53),   S( -7,  -2),   S(-13, -18),   S(  6,  14),   S(-27, -46),   S( -9, -45),   S(-11, -37),
            S( -8, -33),   S(  3,  15),   S(-25, -16),   S( -8,  10),   S( -3, -24),   S( -2, -11),   S(  2,  -4),   S( -3, -21),
            S(-11, -30),   S(-13, -24),   S( -2,  -1),   S(  2, -22),   S(  1,  15),   S(  8,   9),   S( -6,   7),   S(  0, -11),
            S( -9, -35),   S( -2, -14),   S(-22, -48),   S(-10, -28),   S(  4,  -5),   S(-16, -16),   S( -4, -24),   S( -2, -11),
            S(  4, -17),   S(  5,  16),   S( 13,  12),   S( -4, -23),   S( -6, -10),   S( -8, -36),   S( -8, -52),   S(  5,  12),
            S( -8, -16),   S(  4,  -4),   S(-22, -34),   S( 15,  -2),   S(  5,  -2),   S(  9,  11),   S(  5,  -6),   S(  4,  -2),
            S(-17, -24),   S(  8,   2),   S(  9,   9),   S(-10, -35),   S(  0, -20),   S( 16,  26),   S(  3,   1),   S( -6, -11),
            S(-15, -97),   S(-33, -93),   S(  1,  10),   S(-13, -43),   S( -1,  12),   S( -2, -29),   S(-16, -67),   S( -1,   1),

            /* rooks: bucket 14 */
            S(-23, -49),   S(-25, -63),   S(-10, -34),   S( -4,   0),   S( -8,   4),   S( -6,  -2),   S(  9, -12),   S(-11, -40),
            S(-15, -25),   S(-18, -63),   S( -6,  -8),   S(-25, -45),   S(-15, -13),   S( -4, -18),   S( -1,   1),   S(  1,   0),
            S( -4, -12),   S(-21, -58),   S( -3,  -9),   S( -9, -14),   S( -5,   3),   S( -3,   9),   S(  8,  37),   S(  0, -23),
            S(  4,  20),   S( -8, -46),   S(  0,  -9),   S(  2, -11),   S( -2,  -9),   S( -6,  -2),   S(  3, -18),   S( -5,   2),
            S(  0, -55),   S( -4, -51),   S( -6, -54),   S( -3, -38),   S( -5, -49),   S( -8, -70),   S( -3, -33),   S(  0, -10),
            S( -4, -35),   S(  4,  -9),   S(  0, -42),   S(  5, -82),   S( -6, -62),   S(-11, -53),   S(  7, -29),   S(  0,  -5),
            S( 13,   7),   S(-16, -78),   S( -1, -46),   S( -3, -61),   S(  8, -31),   S( -1, -26),   S( 11,   5),   S( 16,  16),
            S( -9, -33),   S(  1, -20),   S( -5, -36),   S(  2, -20),   S(-14, -31),   S( -7,   5),   S(  5,  29),   S( -9, -17),

            /* rooks: bucket 15 */
            S(-21, -37),   S(-12, -46),   S(-10, -31),   S( -1, -32),   S( -7, -29),   S( -9, -10),   S(-14, -33),   S( -9, -15),
            S(-15, -26),   S(-16, -34),   S( -5,  -7),   S(-12, -17),   S(-14, -25),   S( 10,  44),   S(-12, -49),   S(  6,  23),
            S(-10, -31),   S(-18, -61),   S( -8, -26),   S(  4, -19),   S(  9,  -8),   S( -1, -12),   S(-11, -27),   S( -3, -18),
            S(-15, -69),   S( -5, -36),   S( -4, -10),   S( -4, -14),   S(-11, -30),   S( -2, -44),   S(  0, -32),   S(-15,  -6),
            S( -2,  -1),   S(-10, -33),   S(  4, -29),   S(-12, -68),   S(  6, -16),   S(  0, -48),   S( -1,  -3),   S( -5,  12),
            S(  4,  -4),   S( -9, -62),   S( -1, -54),   S(  5, -38),   S( -1, -38),   S(  2, -34),   S(  5, -33),   S(-10,  -6),
            S( 12,  24),   S( 14, -15),   S(  1, -43),   S(  6, -25),   S(  5, -38),   S( 10,  31),   S( 19,  12),   S(  0,  -5),
            S(-10, -42),   S( -5,  -1),   S(  2, -33),   S(  4, -19),   S( -6, -49),   S(  1, -13),   S(  2, -25),   S( -3, -15),

            /* queens: bucket 0 */
            S(-17,  -2),   S(-38, -81),   S( 47, -83),   S( 59, -62),   S( 32, -45),   S( 24, -29),   S( 69,   9),   S(  7,  -7),
            S( -8, -20),   S( 49, -67),   S( 35, -16),   S( 28,  -9),   S( 36,   7),   S( 25, -10),   S( 31,  11),   S( 36,  27),
            S( 22,  36),   S( 39,   1),   S( 22,  31),   S( 24,  14),   S( 36, -18),   S( 12,  16),   S( 12,  22),   S( 25,  48),
            S(  9,  11),   S( 32,  29),   S(  2,  54),   S( 29,  29),   S( 20,  35),   S( 22,  18),   S( 17,  24),   S( 32,  40),
            S( 25,  54),   S( 44,  46),   S(  4,  58),   S( 30,  78),   S(-25,  56),   S(  8, -16),   S( 10,  39),   S( 61,  11),
            S( 23,  59),   S( 25,  54),   S( 57,  22),   S( 23,  24),   S( 37,   5),   S( 13,  34),   S(  9,  17),   S( 15,  -8),
            S( 58,  39),   S( 36,  40),   S(  2,  70),   S( 64,  38),   S(  5,  24),   S(  4,   3),   S( 45,  25),   S( 11,  22),
            S( 28,  42),   S( 18,  31),   S( 49,  24),   S( 20,  47),   S( 53,  49),   S(-24,   1),   S( 36,   7),   S( 14,  19),

            /* queens: bucket 1 */
            S( -1, -31),   S(-84, -27),   S(-76, -19),   S(-23, -108),  S( -2, -59),   S(-30, -40),   S( 37, -48),   S( 20,  26),
            S(-21, -47),   S(-24, -65),   S( 13, -51),   S(  4,   5),   S(  5,   3),   S( 13,  -3),   S( 31, -27),   S(  7, -18),
            S(-36,  50),   S( -1, -22),   S(  9,  23),   S(  0,   9),   S(  0,  32),   S( -7,  32),   S( 18,  24),   S( 29,  -1),
            S(  1,   0),   S(-17,   8),   S(-21,  58),   S( 38,  17),   S( 17,  25),   S( 11,  10),   S(  2,  10),   S( 11,  32),
            S( 31, -10),   S(  3,  11),   S(  0,  87),   S(  2,  72),   S(-29,  73),   S( 32,   5),   S( -2,  12),   S(  9,  58),
            S( 22,  34),   S( -6,  64),   S( 40,  55),   S(-32,  84),   S(-13,  64),   S(-44,  61),   S( 24,  14),   S( 25,  31),
            S(-34,  78),   S( -3,  45),   S(-23,  37),   S(-21,  83),   S(-18,  61),   S(  1,  44),   S( -9,  53),   S(-12,  36),
            S(  0,  14),   S( 17,  23),   S( 76,  34),   S( 10,  22),   S( -8,   3),   S( 14,  36),   S(  8,  28),   S(  4,  12),

            /* queens: bucket 2 */
            S( 13,  -9),   S( 31, -41),   S( 13, -27),   S(  6, -29),   S(-31,  25),   S(-30, -28),   S(-16, -38),   S( 24,   5),
            S( 12,  21),   S( 13,  38),   S( 24, -17),   S( 28, -31),   S( 18, -16),   S( 15, -56),   S( 27, -26),   S( 21, -18),
            S( 16,  10),   S( 21, -13),   S( 18,  33),   S( 23,  26),   S( 17,  61),   S( 28,  49),   S( 18,  14),   S( 33,  53),
            S(  6,  21),   S( -5,  53),   S(  5,  57),   S( 20,  58),   S( -4,  98),   S(  2,  89),   S( 28,  -4),   S(  4,  79),
            S( 21, -10),   S( -4,  55),   S( -5,  73),   S(-47, 141),   S(-32, 128),   S(-25, 108),   S(-18, 121),   S(  9, 100),
            S( 13,  43),   S( -1,  56),   S(-13,  74),   S( -4,  65),   S(-32, 114),   S(  2,  80),   S( 13, 108),   S( -8,  92),
            S(-32,  62),   S(-51, 114),   S(-27,  96),   S( -6,  79),   S(  9,  90),   S( 12,  60),   S(-45,  58),   S(-35,  87),
            S(-64,  75),   S(  3,  53),   S( 42,  34),   S( 19,  50),   S( 31,  75),   S( 15,  47),   S(  3,  57),   S(  5,  19),

            /* queens: bucket 3 */
            S( 67,  82),   S( 61,  82),   S( 51,  91),   S( 49,  75),   S( 69,  24),   S( 45,  12),   S(  3,  28),   S( 38,  26),
            S( 79,  97),   S( 65, 110),   S( 51, 101),   S( 51,  75),   S( 53,  77),   S( 64,  50),   S( 69,  12),   S( 23,  36),
            S( 62,  85),   S( 53,  95),   S( 64,  62),   S( 61,  68),   S( 56,  92),   S( 60,  96),   S( 59, 105),   S( 63,  47),
            S( 42, 128),   S( 58,  92),   S( 53,  92),   S( 49, 105),   S( 43,  98),   S( 48, 123),   S( 61,  89),   S( 44, 133),
            S( 48, 108),   S( 54, 115),   S( 46, 106),   S( 43, 111),   S( 36, 141),   S( 22, 134),   S( 35, 173),   S( 57, 136),
            S( 48, 122),   S( 53, 114),   S( 39, 131),   S( 15, 143),   S( 33, 128),   S( 68, 122),   S( 53, 172),   S( 31, 175),
            S( 49, 110),   S( 40, 128),   S( 73,  97),   S( 65, 115),   S( 32, 136),   S( 42, 131),   S( 70, 151),   S(152,  58),
            S( 55, 111),   S( 78,  96),   S( 54, 124),   S( 91,  71),   S( 43, 110),   S( 81,  97),   S(131,  55),   S(118,  69),

            /* queens: bucket 4 */
            S(  5,   0),   S( -5, -10),   S(-72, -13),   S(-24, -31),   S( 52, -18),   S( 15, -25),   S(-39, -33),   S(-16,   0),
            S(-34, -28),   S(-39, -37),   S( 14, -20),   S(-29,  21),   S( 41, -25),   S( -8, -14),   S(  4,  13),   S(-14,  -7),
            S(-19, -17),   S( 17,   0),   S( 19,  25),   S(-16,  56),   S( -8, -21),   S(-17, -18),   S( 17,   0),   S(-34, -33),
            S(-21,  -9),   S( 23,  18),   S(  2,  58),   S( -5,  45),   S( 17,  30),   S(  3,  17),   S( 11, -12),   S(  9, -20),
            S(  2,  11),   S( 10,   0),   S( 23,  32),   S( 58,  80),   S( 32,  17),   S( 15, -20),   S( -6,  -7),   S(-11, -25),
            S(  2,  13),   S(  7, -16),   S( 19,  47),   S( 53,  80),   S( 23,   9),   S(-16,  12),   S( -6,  -4),   S(-17,  -9),
            S( -1,  -6),   S( 22,  32),   S( 21,  21),   S( 14,  28),   S(  7,  31),   S( -9,  -5),   S(-25, -31),   S(-21, -18),
            S( -1, -17),   S(  3,  -8),   S( 39,  53),   S( 19,  16),   S(-31, -25),   S(-20,  -2),   S(-16, -26),   S(-14, -21),

            /* queens: bucket 5 */
            S(-50, -24),   S(-34, -63),   S(-15, -22),   S(-22, -22),   S(-69, -37),   S( 20, -17),   S(-18, -24),   S(  1,  -7),
            S(-20,   0),   S(-33, -16),   S(-51, -32),   S(-65, -21),   S(-12, -36),   S(-14,  -7),   S(-64, -15),   S(-60, -28),
            S(-24, -11),   S(-36,   6),   S(-63,  10),   S( 15,  51),   S(  6,  51),   S( -5,   5),   S( 18, -36),   S(-11,  18),
            S(-33, -18),   S(-68, -27),   S( 17,  59),   S( -2,  84),   S( 27,  12),   S( 10,  23),   S(  0,  -5),   S(  4,  26),
            S(-52, -13),   S( -2,  30),   S(-31,  28),   S( -1,  39),   S( 43,  54),   S( 11,  32),   S(-24, -17),   S(-26, -31),
            S(-19,   3),   S(  7,  48),   S(-24,  46),   S( 13,  33),   S( 54,  60),   S( 24,  30),   S(  0,  13),   S(-13, -21),
            S(-28,  -2),   S( 16,  30),   S( 33,  86),   S( -4,  33),   S( 17,  52),   S( 40,  56),   S( -5,   5),   S(-41, -20),
            S(-13,  14),   S( 16,  35),   S(-17,   9),   S(-13,  20),   S( 10,  17),   S(-10,  12),   S( 13,   3),   S(-24, -27),

            /* queens: bucket 6 */
            S( -5,  10),   S(-37, -32),   S(-47, -13),   S(-113, -37),  S(-71, -67),   S(-52, -27),   S(-44, -26),   S(-45, -13),
            S(-53, -16),   S(-59,  10),   S(-29,  -8),   S(-32,   1),   S(-53,  24),   S(-105,  -5),  S(-67, -27),   S( 44,   6),
            S(-25,  11),   S(-21,  -4),   S(-61,  27),   S(-83,  74),   S(-21,  49),   S(-21,  13),   S(-58, -14),   S( -6,   3),
            S(-42,  14),   S( -3,  19),   S(-16,  56),   S(-56,  89),   S( 12,  77),   S( 11,  54),   S(-15,  35),   S( 19,  12),
            S(-49,   6),   S( 32,  20),   S(-28,  81),   S( 38,  71),   S( 38,  87),   S( 68,  61),   S( 23,  21),   S( -1,  27),
            S(-39,  35),   S(  0,  14),   S( 32,  48),   S( 24,  44),   S( 19,  59),   S( 60,  89),   S(-21,  15),   S( 10,  24),
            S( -9,   5),   S(  9,  10),   S(  3,  52),   S(-15,  48),   S( 37,  56),   S(  5,  26),   S(-25,  16),   S(-32,   4),
            S( 19,  23),   S( 41,  42),   S( 19,  22),   S( 27,  48),   S( 38,  58),   S( 34,  31),   S( -6,   5),   S(  1,  -1),

            /* queens: bucket 7 */
            S(  2, -22),   S(-11,  18),   S(-45,  10),   S( -7, -29),   S(-33, -23),   S(-42, -29),   S(-26, -10),   S( 14,  16),
            S(-52,  27),   S(-49,  23),   S(-24,  23),   S(-14,  25),   S(  1,  26),   S(-36,  31),   S(-83,  55),   S(-14,  -9),
            S(-44, -15),   S(-44,  47),   S(-18,  23),   S( 12,  11),   S( 19,  39),   S( 14,  31),   S( 12,  17),   S( 15, -12),
            S(-59, -11),   S( 10,  27),   S(-33,  44),   S( -4,  43),   S( 35,  34),   S( 45,  26),   S( 16,  59),   S(-11,  16),
            S(-45,  55),   S(-68,  35),   S( 17,  31),   S( 38,  30),   S( 50,  12),   S( 77,  -1),   S( 34,  48),   S( 15,   4),
            S( -3,  46),   S(  0,  13),   S(-12,  14),   S( 23,  19),   S( 70,  49),   S( 94,  44),   S( 66,  25),   S( 78,  -2),
            S(  8,  -2),   S( -3,  26),   S( 12,  -4),   S( 39,  19),   S( 49,  24),   S( 60,  30),   S( 37, -16),   S( 38,  53),
            S( 20,  30),   S( 23,  17),   S( 51,  24),   S( 15,  34),   S( 60,  38),   S( 23,  39),   S(  5,  11),   S( 44,  42),

            /* queens: bucket 8 */
            S( -3, -17),   S(-12, -38),   S( -9,  -4),   S(  2,   2),   S(  3,   1),   S( -4, -16),   S(-26, -41),   S(  5,  16),
            S(  3,  18),   S( -1,  -5),   S(-21, -19),   S(-17, -12),   S( -7, -13),   S(-14, -16),   S(-27, -68),   S(-14, -21),
            S( -9, -18),   S(  0,   7),   S(-17, -36),   S(-12, -20),   S( -4,  -5),   S(-16, -22),   S(-15, -37),   S(-11, -16),
            S(  4,   8),   S( -3,   2),   S(  4,  12),   S(  2,  14),   S( 10,  11),   S( -7,   0),   S(  6,  13),   S(-14, -44),
            S(  4,  12),   S( -6,  29),   S( 15,  43),   S( 22,  41),   S(  9,  25),   S(  2,   3),   S( -9, -13),   S(-18, -39),
            S(  1,  13),   S( 19,  31),   S(-18,  17),   S( -5,   9),   S( -4,  -6),   S( -2,  -1),   S( -1,   1),   S( -7, -14),
            S( -8, -12),   S(-27, -32),   S( 12,  25),   S( 16,  29),   S( -9,  -2),   S(-11, -14),   S( -6, -10),   S(-19, -40),
            S(-14, -43),   S(  1,  -1),   S(-25, -52),   S( -6, -22),   S(-15, -31),   S( -4,  -8),   S( -8, -30),   S(-11, -22),

            /* queens: bucket 9 */
            S(  0,   5),   S(-15, -38),   S(  2,  -1),   S(-40, -60),   S(-30, -51),   S(-19, -32),   S(-16, -32),   S(-26, -45),
            S( -5, -14),   S( -9, -30),   S(-24, -32),   S( -2,  -8),   S(-28, -35),   S( -8, -19),   S( -2, -21),   S( -5, -10),
            S(  8,   6),   S(  4,  12),   S( -9,  21),   S(-24, -22),   S(-13, -10),   S(-29, -24),   S(-13, -21),   S(-20, -45),
            S(  7,  10),   S(-11,  -3),   S(  8,  35),   S( -3,   7),   S( -1,   6),   S(-13, -15),   S(-27, -42),   S(-10, -33),
            S( -6,  -5),   S(-21,   1),   S( -4,  13),   S(  2,  40),   S( 14,  38),   S(  6,   7),   S( -4,  -7),   S(-18, -24),
            S(-25, -31),   S(-20,   5),   S(-16,   5),   S(  3,  23),   S(-16, -31),   S(-14, -26),   S( -5,   4),   S( -5,  -4),
            S(-17, -20),   S( -8, -25),   S(-17,  10),   S( 10,  15),   S( 16,  24),   S(  1, -20),   S( -6, -13),   S(-17, -38),
            S(-17, -36),   S(-22, -45),   S( -5,   4),   S(  1,  15),   S(-11, -18),   S(  1,  11),   S(  2,   0),   S( -1,   3),

            /* queens: bucket 10 */
            S(  1,   6),   S( -2,  -2),   S(-12, -30),   S(-25, -28),   S(-19, -36),   S(-18, -28),   S( -3, -11),   S(-10, -28),
            S(-12, -18),   S(-16, -29),   S( -4, -16),   S(-29, -27),   S(-14, -16),   S(-21, -23),   S(-11, -29),   S(-25, -39),
            S(-21, -37),   S(  0,   1),   S(-38, -51),   S(-13, -12),   S(-24,  -8),   S(-23,  -2),   S(-14, -19),   S(  5,   0),
            S(-11, -25),   S( -8,  -8),   S(-10, -21),   S( -3,  16),   S( 22,  56),   S(-12,  29),   S( -8,  -8),   S(-32, -34),
            S(-19, -25),   S(  2,  -4),   S(-16,  -2),   S( -1,  42),   S( -7, -14),   S(  5,  25),   S( 12,   0),   S(-10, -20),
            S(-17, -26),   S(-21, -35),   S(-10,  11),   S(  4,  28),   S( -7,  21),   S( -1,   2),   S(  5,  -2),   S(-17, -25),
            S(-14, -19),   S(-27, -35),   S(  1,   3),   S(-24, -13),   S(-24, -39),   S( -1,   5),   S(-15, -25),   S(-11, -23),
            S( 15,  19),   S(-18, -26),   S(  9,   8),   S( -1,   1),   S( 23,  27),   S( -7, -12),   S( -2,   3),   S(-20, -34),

            /* queens: bucket 11 */
            S(-14, -28),   S(-14, -32),   S(-20, -24),   S(-28, -43),   S(-12, -10),   S(-15, -15),   S( -9,  -2),   S(-15, -25),
            S(-30, -46),   S(-19, -28),   S(-57, -52),   S(-16, -17),   S( -9, -14),   S(-13, -15),   S(  7,  -5),   S(-10,  -6),
            S(-17, -17),   S(-25, -35),   S( -5, -21),   S( -3, -20),   S( -5,  24),   S( -1,   8),   S( -9,   4),   S(-22, -25),
            S(-22, -45),   S(-14, -17),   S(-17, -37),   S( -4,  27),   S(-16, -24),   S(  1,  12),   S( 15,  22),   S(-22, -42),
            S(-19, -11),   S( -4, -23),   S(-44, -51),   S(  5,   1),   S( 10,   0),   S( 50,  89),   S( 17,  40),   S(-11,   0),
            S(-11, -32),   S(  5,   6),   S( -9,   3),   S( 22,  26),   S( 29,  35),   S( 47,  41),   S( 27,  25),   S(-19, -18),
            S( -9, -13),   S(-29, -48),   S( 21,  23),   S(-14, -26),   S( 11,  29),   S( 26,  37),   S( 30,  37),   S(-21, -27),
            S(-20, -28),   S(-19, -39),   S(-25, -48),   S(  0,  -5),   S(  4,   4),   S(-13, -43),   S( 36,  38),   S( -6, -35),

            /* queens: bucket 12 */
            S(  7,  12),   S(  3,   5),   S( -1,   1),   S( -9, -19),   S( -9, -16),   S( -5,  -5),   S( -8, -18),   S( -5,  -9),
            S( -6, -15),   S( -5, -13),   S( -7,  -5),   S( -5,  -9),   S( -3,  -6),   S(-12, -25),   S( -6, -13),   S( -8, -17),
            S( -5,  -6),   S( -6, -17),   S( 18,  19),   S(-15, -17),   S(-15, -23),   S( -6, -12),   S(-11, -27),   S(-15, -33),
            S(  3,   2),   S( -6,  -8),   S(  3,   9),   S( -5,   4),   S(  6,  12),   S(-13, -22),   S( -2,  -3),   S( -6, -18),
            S( -1,   0),   S( 21,  26),   S( 39,  67),   S( 12,  30),   S(  5,  15),   S( -1,   1),   S(-25, -54),   S( -4,  -6),
            S(  5,  18),   S(  1,   5),   S( 44,  76),   S( -5, -16),   S( -1,  -3),   S( 10,  15),   S(  9,  16),   S( -4, -21),
            S( -2,  -4),   S( 12,  25),   S( 27,  45),   S( 16,  33),   S(  6,  10),   S(  2,  -6),   S(  1,   2),   S( -3,  -4),
            S( -6, -27),   S(-11, -21),   S( -6,  -6),   S(-13, -22),   S(  3,   2),   S( -6, -10),   S(  4,   0),   S( -6,  -9),

            /* queens: bucket 13 */
            S( -6, -19),   S( -6, -14),   S( -2, -10),   S( -5,  -2),   S( -5, -13),   S( -1,  -9),   S( -9, -23),   S(-13, -24),
            S(  0,  -2),   S(  4,   3),   S( 11,  26),   S( -6, -13),   S(-18, -33),   S( -4,  -6),   S(  1,  -3),   S( -8, -17),
            S( -4, -10),   S( -5, -14),   S( -8, -14),   S(  7,   6),   S( -6, -12),   S( -9, -17),   S( -8, -19),   S(-12, -22),
            S( -3,  -8),   S(  2,  -5),   S(  8,   5),   S( 11,  19),   S( 21,  28),   S(-20, -45),   S( -1,  -8),   S( -7, -21),
            S( -3,  -4),   S( 13,  27),   S( 18,  34),   S( 14,  33),   S( 11,  30),   S(  0,   2),   S( -7, -25),   S(-10, -31),
            S(  2,   1),   S( 13,  30),   S( 35,  69),   S( 14,  33),   S( -2,  30),   S( -4, -10),   S(  9,  22),   S( -9, -21),
            S( -2,  -3),   S(  1,   8),   S( -1,  10),   S( 12,  27),   S(  6,  10),   S(  4,   7),   S(-14, -31),   S(  3,   5),
            S(-24, -51),   S( -5, -20),   S(-14, -30),   S(  1,  11),   S( -4, -15),   S( -4, -15),   S(-19, -34),   S( -1,   0),

            /* queens: bucket 14 */
            S(  1,   2),   S( -7, -22),   S( -4, -11),   S(-13, -22),   S( -2,  -4),   S(  1,  -1),   S( -2, -10),   S( -5, -15),
            S(-11, -25),   S( 11,  24),   S( -5, -10),   S( -3, -10),   S( -8,  -4),   S( -5, -18),   S( -5, -12),   S( -2,  -9),
            S(  0,  -3),   S( -8, -14),   S(-17, -16),   S( -6, -11),   S(  3,   0),   S(-12, -29),   S( -1,   2),   S(-12, -16),
            S( -3,  -6),   S(  3,   8),   S( -4,  -4),   S( 21,  43),   S(  2,  -4),   S(  6,  20),   S( 16,  29),   S(  1,  -8),
            S(  2,   9),   S( -4,  -6),   S(-19, -28),   S( -2,  -3),   S( 25,  50),   S(  9,  14),   S(  9,  15),   S( -1,  -6),
            S(  1,   2),   S(  4,  -1),   S( 20,  38),   S( -1,   0),   S( 16,  34),   S( 10,  11),   S(  3,  -6),   S( -2,  -6),
            S( -1,  -4),   S( 11,  14),   S(  5,  14),   S( 17,  28),   S( 10,  16),   S(  8,  20),   S( 13,  18),   S(  9,  19),
            S(-14, -18),   S( -2,  -8),   S(-19, -32),   S( 13,  14),   S(  0,  -4),   S(  0,   1),   S(-11, -26),   S(-13, -25),

            /* queens: bucket 15 */
            S( -7, -17),   S(-13, -32),   S( -3,  -5),   S( -5, -12),   S(  0,   2),   S( -8, -14),   S(-11, -22),   S( -3, -10),
            S( -7, -12),   S(-10, -26),   S( -4, -15),   S(  2,   3),   S(  0,   2),   S( -8, -16),   S( 15,  25),   S( -3, -10),
            S( -3,  -8),   S( -1,  -4),   S(  3,   4),   S( -7, -17),   S( -5, -16),   S(  6,  12),   S( -1,  -1),   S( -2,  -8),
            S( -4,  -8),   S(  1,  -7),   S( -4,  -5),   S( -3, -14),   S(  6,  25),   S(  2,   9),   S( 13,  23),   S(  4,   8),
            S( -7, -16),   S( -7, -17),   S( -8, -23),   S(  2,  -5),   S( 17,  29),   S( 16,  26),   S(-12, -31),   S( -5, -10),
            S( -4, -12),   S( -6, -13),   S( -2, -13),   S(-10, -17),   S(  0,  -8),   S( 31,  64),   S( -4,   1),   S(  5,  13),
            S(-10, -13),   S(  1,  -5),   S( 10,  17),   S(  7,  11),   S( 12,  17),   S( 27,  47),   S(  9,  13),   S(  6,   8),
            S( -8, -18),   S(  0,  -4),   S( -3,  -9),   S( 20,  33),   S(  0,  -8),   S(-16, -40),   S( -2,  -9),   S( -4, -13),

            /* kings: bucket 0 */
            S( 21,  43),   S(  8,  70),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 20,  55),   S( 99,  60),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-21,  23),   S(-77,  36),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 21,  39),   S( -3,  46),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-43,  36),   S(-45,  30),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  45),   S( 23,  39),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 16,  62),   S(  0,  43),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 57,  87),   S( 13,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26, -46),   S( 44, -31),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62, -12),   S( 14,  12),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  4, -43),   S(-42, -24),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 17, -16),   S(-15, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-15, -21),   S(-35, -15),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  -8),   S( -8,  -3),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 34,  14),   S(-11, -17),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 25,  41),   S(-31,  19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-101, -49),  S(  9, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56, -38),   S( 16, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-13, -46),   S(-41, -50),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 65, -61),   S( 35, -50),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  3, -53),   S(-65, -34),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 66, -59),   S( 47, -44),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -4, -25),   S(-95, -45),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 76, -35),   S(  4, -53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -42),   S( 31, -23),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -94),   S( -3, -30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 14, -38),   S( 60, -43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 29, -55),   S( 41, -73),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 56, -43),   S( 48, -46),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 40, -68),   S(-21, -64),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 45, -31),   S(-31, -60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10, -53),   S(-17, -128),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -16),   S(-38,   9),   S( -9,   3),   S(-14,  25),   S( 11,   2),   S( 29,   9),   S( 26,  -3),   S( 47, -10),
            S(-11, -36),   S(-30, -14),   S(-27,  -1),   S(-11,  -5),   S(  7,   7),   S( -4,   9),   S( 22,  -8),   S( 21,   9),
            S(  8, -15),   S( -4, -23),   S( 21, -25),   S(  6, -15),   S( 15,  -3),   S( 11,  30),   S( 11,  26),   S( 53,   5),
            S( 24, -19),   S( 45,  -9),   S( 56, -14),   S( 28,  12),   S( 29,  42),   S( -7,  78),   S( 22,  67),   S( 79,  45),
            S( 83, -43),   S(106, -11),   S( 94, -18),   S( 42,  17),   S( 67, 120),   S(  8, 109),   S( -6, 150),   S( 81,  91),
            S(-190, -95),  S(-116, -121), S( 30, -160),  S( 42,  27),   S(103, 157),   S(110, 188),   S(150,  86),   S( 38, 198),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  17),   S(-56,  17),   S(-35,  10),   S(-56,  42),   S(-20,   1),   S(  1,   6),   S(  0,   2),   S(  3,  22),
            S(-57,   3),   S(-47,   2),   S(-45,   1),   S(-25,   2),   S(-11,   4),   S(-19,   0),   S( -6,  -5),   S(-22,  16),
            S(-40,  17),   S(-20,   8),   S(-28,   5),   S(  7, -18),   S( -8,  22),   S(-14,   9),   S(-22,  19),   S(  4,  17),
            S(-22,  35),   S( 28,   8),   S(-10,  20),   S(  9,  28),   S(  7,  26),   S(-17,  29),   S(  7,  28),   S( 42,  38),
            S( 21,  16),   S( 90, -20),   S(118, -35),   S( 92, -30),   S( 37,  13),   S( -8,  36),   S(-16,  51),   S( 44,  77),
            S(148, -36),   S( -8, -32),   S( 46, -121),  S( 34, -118),  S(-56, -46),   S(-64,  68),   S( 77, 133),   S( 84, 162),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  36),   S(-44,  20),   S(-36,   8),   S(-22,  26),   S(-51,  38),   S(-32,  15),   S(-20,  -4),   S(-26,  25),
            S(-48,  14),   S(-42,  11),   S(-44,   4),   S(-45,  20),   S(-44,  21),   S(-46,   3),   S(-25, -13),   S(-53,  12),
            S(-32,  38),   S(-41,  40),   S(-16,  17),   S(-21,  18),   S(-33,  30),   S(-24,   5),   S(-28,   5),   S(-23,   8),
            S( -8,  71),   S(-32,  62),   S(-10,  33),   S( -4,  34),   S(-11,  34),   S(-21,  14),   S(  7,  14),   S( 27,   8),
            S(-37, 121),   S(-48, 105),   S(-30,  30),   S( 16, -32),   S(104, -26),   S(100,  -7),   S( 63, -18),   S( 62,  -4),
            S( 17, 199),   S( 57, 114),   S( 12,  54),   S(-18, -71),   S( -9, -169),  S(-18, -180),  S(-14, -53),   S(121, -52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9,  20),   S(-14,  17),   S( -9,  21),   S(-10,  39),   S(-23,  57),   S(  8,  38),   S( -4,  11),   S(-11,   0),
            S(  1,   8),   S(  0,  12),   S(-15,  13),   S(-16,  18),   S( -4,  26),   S(  2,  10),   S( -4,  -3),   S(-35,   4),
            S( 15,  36),   S(-10,  52),   S(  4,  23),   S( -1,   9),   S( 12,   5),   S( 15,   0),   S( -5,  -7),   S(-10,  -4),
            S( 17,  83),   S( -5,  95),   S( 12,  72),   S( 11,  36),   S( 28,  10),   S( 31, -14),   S( 13,  17),   S( 39, -14),
            S(  6, 145),   S(-36, 183),   S(-47, 176),   S(-13, 118),   S( 34,  53),   S( 72,   1),   S( 87, -10),   S( 96, -24),
            S( 69, 150),   S( 43, 234),   S( -5, 259),   S( 21, 180),   S(-10,  66),   S(  8, -134),  S(-129, -180), S(-143, -176),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 68, -11),   S( 16,   2),   S(  0, -11),   S(  6, -57),   S(-20,   3),   S( -3, -13),   S(-12,  -7),   S(-62,  32),
            S( 52, -23),   S(  3,   4),   S( 46, -30),   S(-20,  -7),   S(-21, -21),   S( -5, -28),   S(-43, -14),   S(-41,  -6),
            S( 85, -28),   S(114, -32),   S( 64, -42),   S(-11, -20),   S(-81,  16),   S( 14,   4),   S(-55,  17),   S(-71,  31),
            S(-41, -89),   S( 37, -117),  S( 57, -37),   S(-18,   4),   S(-17,  17),   S(-25,  40),   S(-12,  46),   S( -9,  37),
            S( 43, -100),  S(-31, -117),  S( 19, -76),   S( 39,  34),   S( 58,  92),   S( 33,  58),   S( 38,  55),   S( 13,  93),
            S( 15, -40),   S(  1, -52),   S(  7, -85),   S( 14,  40),   S( 35,  69),   S( 60, 119),   S( 54, 103),   S( 22, 102),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62,  40),   S(-15,  16),   S( 14,   0),   S( 54,   5),   S( 59,  -3),   S( 17, -10),   S(-27,   4),   S(-60,  39),
            S(-64,  16),   S(-19,  13),   S( 10,  -6),   S( 16,   1),   S( -1,   5),   S(  3, -16),   S(-68,   3),   S(-68,  22),
            S(-14,  11),   S( 20,  18),   S( 71,   1),   S( -4,  35),   S( -8,  30),   S(-14,   4),   S(-15,   9),   S(-33,  28),
            S( 53,   2),   S( 76, -24),   S(  0, -48),   S(-20, -13),   S( 29, -17),   S(-26,  11),   S( 29,   8),   S( 22,  28),
            S( 66,  -8),   S( 61, -55),   S( 20, -86),   S( -1, -47),   S( 70, -43),   S( 46,   0),   S( 43,  30),   S( -1,  80),
            S( 60,  21),   S( 15, -45),   S(-14, -88),   S(  0, -74),   S( -7, -62),   S( 44,  15),   S( 78, 101),   S( 90,  89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-83,  26),   S(-34,  -4),   S(  0, -12),   S(-25,  23),   S(  6,  17),   S( 21,   4),   S( 37,  -9),   S( 13,  18),
            S(-71,   4),   S(-21, -14),   S(-18, -19),   S( 44, -13),   S(-15,  20),   S( 24, -11),   S( 18, -12),   S(  1,  -1),
            S(-39,  19),   S(-39,  13),   S( -7,   7),   S(  7,  15),   S( 23,  24),   S( 67,  -8),   S( 56,  -9),   S( 35,  -4),
            S(-10,  41),   S( -8,  25),   S( 24,   4),   S( 23,  -3),   S( 12, -42),   S( 27, -51),   S( 92, -33),   S(109, -37),
            S(  4,  76),   S( 20,  23),   S( 71,  -1),   S( 20, -49),   S( -7, -67),   S(-20, -70),   S( 71, -62),   S(109, -21),
            S(114,  50),   S( 92,  61),   S( 66,   2),   S( 33, -58),   S( -4, -84),   S(-28, -94),   S( -8, -58),   S( 69,   1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-53,  -5),   S(-60, -13),   S( 13, -31),   S(-14,  -5),   S( 32,  -1),   S( 61, -14),   S( 52, -26),   S( 72, -14),
            S(-47, -14),   S(-56, -12),   S(-36, -18),   S(-21,  -3),   S( 14, -14),   S( 52, -34),   S( 28, -16),   S( 50, -20),
            S(-37,  15),   S(-50,  14),   S(-25,   0),   S(-11, -20),   S( 15,  -7),   S( 37, -22),   S( 79, -24),   S( 77, -20),
            S(-18,  41),   S(-59,  57),   S(-19,  41),   S( -7,  20),   S( 17, -14),   S( 51, -56),   S( 34, -70),   S( 43, -104),
            S( 34,  45),   S(-26,  97),   S( 40,  91),   S(  6,  74),   S( -9,  23),   S(-11, -51),   S(-47, -120),  S(-20, -93),
            S(127,  73),   S(138,  84),   S(130,  75),   S( 81, 101),   S( 91, -30),   S( -2, -71),   S( -7, -73),   S( 22, -161),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1,  -9),   S(  9, -15),   S( 44, -18),   S( -7, -22),   S(-16, -61),   S( 20, -35),   S( 51, -56),   S( 32, -39),
            S(-39, -38),   S(  7,  -5),   S(-85, -59),   S(-50, -25),   S(-33, -47),   S( 15, -39),   S(-13, -42),   S(-30, -45),
            S(-34, -72),   S( 35, -61),   S(-18, -59),   S(-32, -33),   S(-24, -16),   S(-39,  -8),   S(-31,  -7),   S(-30, -12),
            S( -7, -12),   S(-13, -28),   S( 12, -29),   S( 15,  -3),   S( -5,  -4),   S( -3,  29),   S(-27,  32),   S(  5,  38),
            S( 14,  27),   S(-12, -23),   S( 35,  43),   S( 34,  67),   S( 61, 113),   S( 40, 104),   S(  2,  84),   S(-16,  97),
            S( 13,  40),   S( 18,  69),   S( 24,  64),   S( 49, 103),   S( 26,  65),   S( 53, 182),   S( 40,  98),   S(-15,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 16, -11),   S( 59,  -9),   S( 30,  -5),   S( -3,  13),   S(  8,   4),   S( 60, -36),   S( 59, -58),   S( 21, -27),
            S(-25, -47),   S(-27, -43),   S(-20, -53),   S(-27, -25),   S( -3, -29),   S(-45, -32),   S( 11, -47),   S(  2, -32),
            S(-98, -22),   S(  1, -59),   S( -4, -77),   S(-38, -56),   S( 23, -43),   S( -3, -51),   S(-35, -38),   S(-26, -11),
            S(-41,   2),   S(-50, -50),   S( -3, -39),   S( -8, -35),   S( 44, -36),   S(-17, -22),   S(-14,  18),   S( 16,  -1),
            S(-15,  14),   S(  0, -32),   S(  7, -46),   S( 30, -12),   S( 16,  29),   S( 26,  41),   S( 14,  61),   S(-26,  55),
            S(-17,  31),   S( 24,  34),   S( 18,  47),   S( 25,  19),   S( 34,  83),   S( 26,  59),   S(  7,  59),   S( 11,  71),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -67),   S( 21, -66),   S(  0, -18),   S(  1, -32),   S( 58,  -6),   S(129, -14),   S( 63, -15),   S( 34, -26),
            S(-48, -58),   S(-65, -55),   S( 11, -64),   S( 28, -41),   S(  7, -35),   S( 25, -37),   S( 33, -44),   S( 12, -42),
            S(-56, -30),   S(-17, -55),   S(-22, -46),   S( 23, -44),   S( -2, -46),   S(  9, -68),   S( -9, -69),   S( 14, -42),
            S(-13,  -5),   S( 16, -15),   S( -9, -25),   S(  0, -43),   S(-37, -46),   S(-33, -53),   S(  0, -50),   S( -4, -36),
            S( 29,   2),   S( 33,  12),   S( 22,  -8),   S(-16, -37),   S( 35,  -4),   S( 18,  28),   S(-13, -10),   S( 19,  -8),
            S( -6,  26),   S(-13,  52),   S( 19,  19),   S( 28,  20),   S( 29,  43),   S(  0,  -8),   S(  5, -11),   S( 27,  39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -33),   S(-26, -31),   S(-33, -33),   S(  9, -13),   S( -7, -30),   S( 97,  -5),   S( 96, -34),   S( 31,   0),
            S(-12, -61),   S(-33, -70),   S(-41, -51),   S(-18, -51),   S( 13, -31),   S(-21, -38),   S(-33, -21),   S( 31, -54),
            S(-63,  -9),   S(-46, -28),   S(-26, -19),   S( 14, -42),   S(-11, -39),   S( 17, -45),   S(-35, -79),   S( 12, -65),
            S(-28,  22),   S(-26,  26),   S(  9,  26),   S( 55,   3),   S(-11,  -8),   S(-27, -38),   S(-24, -44),   S(-29, -44),
            S( -8,  35),   S(-17,  40),   S( 10,  87),   S( 27,  62),   S( 43,  44),   S(  0, -24),   S(  6,  17),   S(-25,  -3),
            S( 42,  39),   S( 42,  57),   S( 45,  82),   S( 58,  87),   S( 15,  56),   S( 38,  79),   S( 13,  40),   S( 25,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -82),   S(-23,   1),   S(-13, -24),   S( -7,   6),   S( -5, -37),   S( -6, -44),   S( 10, -47),   S( -9, -32),
            S(-29, -40),   S(-30, -18),   S(-32, -57),   S(-35, -24),   S(-34, -55),   S(-26, -30),   S(-22, -77),   S( -8, -69),
            S(-27, -24),   S( -7, -50),   S(-22, -65),   S(-27, -55),   S(-30, -44),   S(-25,  -1),   S(-27, -21),   S(-40, -56),
            S(-10,  -4),   S( 22,  33),   S( -4,   5),   S(  4,   5),   S( 13,  54),   S(  2,  78),   S( -1,  49),   S(-26,  17),
            S( -4,   4),   S(  7,  17),   S(  5,  15),   S( 23,  57),   S( 36,  88),   S( 30,  91),   S( -5,  73),   S( 25, 114),
            S(  8,   3),   S(  7,  22),   S( 27,  88),   S(  9,  34),   S( 37, 144),   S( 25, 108),   S(-26,   7),   S(-17,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -60),   S(-25, -62),   S(-11, -29),   S(  0,  -8),   S(  3, -26),   S(-43, -34),   S( -2, -66),   S(-13, -58),
            S(-50, -63),   S(-47, -65),   S(-49, -67),   S( 16, -14),   S(-43, -55),   S(-37, -43),   S(-32, -67),   S(-30, -49),
            S(-22, -33),   S(-24, -74),   S(-19, -81),   S(-21, -60),   S(-11, -53),   S(  5, -35),   S(-31, -33),   S(-13, -18),
            S(-17,  12),   S(-12,   0),   S( -7, -28),   S(  5,  19),   S(  3,  41),   S(-35,  19),   S(-19,  -7),   S(-32,  26),
            S(-20,   8),   S(  9,  41),   S( -2,  -1),   S( 15,  28),   S( 42,  87),   S( 30,  98),   S( 11, 114),   S(-18,  81),
            S( 17,  90),   S(  5,  33),   S(  8,  27),   S( 14,  29),   S( 23,  93),   S(  3,  35),   S(  1,  88),   S(-15,  50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -81),   S( 11, -82),   S(  4, -26),   S(  3,  -3),   S( -7, -37),   S(-11,  -3),   S( 13, -39),   S(  3, -32),
            S(-15, -80),   S(-78, -85),   S(-14, -84),   S(-25, -79),   S(-10, -51),   S(-15, -37),   S(-14, -46),   S(-14, -58),
            S(-12, -58),   S(-24, -73),   S(-39, -39),   S(-10, -68),   S(-28, -54),   S( -5, -62),   S(-27, -55),   S(-40, -13),
            S( -3,  16),   S(-36, -17),   S( -2,   8),   S(-22,  -5),   S(  7, -36),   S(  4,  -1),   S( -2,  -5),   S( -6,  43),
            S( -2, -24),   S(  0,  21),   S( 19,  65),   S(  0,  31),   S( 32,  68),   S(  7,  11),   S( 19,  77),   S( 19,  63),
            S( -4,  62),   S( 17,  79),   S( 17,  74),   S( 18,  69),   S( 23,  88),   S( 11,  65),   S( 30, 125),   S( 19, 110),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-14, -44),   S( -6, -70),   S(-28, -43),   S( -7, -19),   S(-11, -36),   S(-28, -28),   S(-28, -41),   S(-20, -86),
            S(-20, -44),   S(-36, -70),   S(-10, -66),   S( -1, -43),   S( -5, -27),   S(-46, -25),   S(-11, -12),   S(-34, -65),
            S(-36, -21),   S(-46, -51),   S(-26, -42),   S( 11, -53),   S(-18, -44),   S( -4, -38),   S( -8, -44),   S(-25, -12),
            S(-22,  -4),   S(-29, -19),   S( 21,  54),   S(-11,  10),   S( 20,  40),   S(  4,  16),   S(  5,  21),   S( 10,  29),
            S( -6,  46),   S( 10,  81),   S(  9,  70),   S( 18,  67),   S( 25, 131),   S( 12,  69),   S( 15,  59),   S(  9,   4),
            S( -9,  41),   S(-10,   7),   S(  5,  76),   S( 21,  76),   S( 18,  68),   S( 21,  76),   S( 16,  35),   S( 19,  71),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-49, -40),   S(-25, -38),   S( -8,   9),   S( -3,  27),   S(-45,  21),   S(-22,   7),   S(-13, -24),   S(-94, -48),
            S( 33, -37),   S(-17,  -7),   S(-18, -22),   S( -2,   8),   S(-12,   8),   S(-14, -12),   S(-14, -46),   S(-34, -29),
            S(-23,   1),   S(  6,  -7),   S(-10,  27),   S( 40,  23),   S(-24,  30),   S(  4,   2),   S(-24,  34),   S(  0, -72),
            S( -3,  60),   S( 32,  55),   S( 12,  62),   S( 50,  23),   S( 15,  44),   S( 12,  41),   S( 44, -15),   S(-13, -18),
            S( 43,  64),   S( -7,  57),   S( 57,  87),   S( 60,  36),   S( 75,  29),   S( 26,  42),   S( 16,   0),   S( 17,  10),
            S(134, -22),   S(-21, 111),   S(119,  68),   S( 55,  39),   S( 30,  66),   S(  6,  65),   S( 63, -27),   S( -8,  19),
            S( 53,  56),   S( 38,   3),   S( 53,  13),   S(107, 113),   S( 55,  21),   S( 11,  48),   S(-27,  43),   S(-29,  36),
            S(-133, -177), S(  5,   1),   S( 11,  -8),   S( 52,  61),   S( 25,  49),   S( 14,   7),   S(-35,  12),   S(  8,  16),

            /* knights: bucket 1 */
            S( 21, -23),   S(-62,   9),   S( -5,  12),   S(-64,  63),   S(-31,  50),   S(-28,  -6),   S(-28, -12),   S(-10,   6),
            S(-55,  54),   S(-55,  78),   S(-26,  35),   S(-12,  25),   S(-16,  21),   S( -4,  37),   S(-35,   5),   S(-42, -33),
            S(-30,  16),   S(-13,   9),   S(-27,  32),   S(-17,  62),   S(-14,  44),   S(-16,  31),   S(-38,  44),   S(-34,  52),
            S(-18,  75),   S( 30,  40),   S(  3,  62),   S(-14,  67),   S(-11,  66),   S(-10,  74),   S( 19,  14),   S(-43,  83),
            S( 61,  -3),   S(  5,  17),   S( 39,  59),   S(  8,  61),   S( 39,  49),   S( 16,  61),   S(-17,  50),   S(  8,  73),
            S(-20,  72),   S( 59,  23),   S(105,  31),   S(116,  35),   S( 51,  48),   S(-29,  68),   S( 39,  47),   S(-21,  62),
            S( 35, -30),   S( 11,  12),   S( 34, -25),   S( 18,  47),   S( 18,  14),   S( 31,  30),   S(-22,  86),   S( -8,  42),
            S(-189, -24),  S( 21,   4),   S(-34, -85),   S( -9,  23),   S( -2,   8),   S( 51,  31),   S( 22,  62),   S(-92,   0),

            /* knights: bucket 2 */
            S(-55,  16),   S(-41,  40),   S(-40,  17),   S(-17,  31),   S(-21,  28),   S(-56,  13),   S(-33,   2),   S(-26, -15),
            S(-19,  51),   S( -8,  50),   S(-33,  33),   S(-16,  38),   S(-33,  42),   S(-15,  29),   S(-11,  39),   S(-41,  13),
            S(-35,  56),   S(-32,  46),   S(-27,  37),   S(-31,  78),   S(-31,  64),   S(-31,  36),   S(-30,  27),   S(-19,  12),
            S(-16,  64),   S(-16,  64),   S(-20,  96),   S(-28,  96),   S(-37,  90),   S(-12,  71),   S( -1,  52),   S( -3,  47),
            S(-20,  82),   S(-14,  74),   S( -3,  80),   S( 13,  57),   S(-22,  94),   S( 25,  78),   S(-22,  84),   S( 41,  15),
            S(-15,  65),   S(  5,  67),   S(-29, 108),   S( 47,  31),   S( 54,  51),   S(130,  24),   S( 57,  37),   S(  2,   0),
            S( 31,  40),   S(-33,  65),   S( 60,  15),   S( 51,   6),   S(-30,  44),   S( 27,  -2),   S( 24,  50),   S( 29, -21),
            S(-66,  37),   S( 19,  81),   S(-14,  81),   S(-28, -29),   S(-39, -17),   S(-18, -33),   S( 26,   7),   S(-137, -85),

            /* knights: bucket 3 */
            S(-70,  39),   S(-25, -15),   S( -9,  18),   S(-11,  24),   S( -7,  30),   S(-20,  22),   S(-30,  -8),   S(-33, -35),
            S(-21,  12),   S( -2,  36),   S( -8,  29),   S(-10,  37),   S(-14,  31),   S( 13,  20),   S( 17,   1),   S(  5, -16),
            S(-19,  39),   S(-23,  43),   S( -8,  53),   S( -8,  79),   S( -2,  75),   S(-12,  57),   S(  5,  39),   S( -7,  13),
            S( -8,  35),   S(  5,  63),   S( 12,  80),   S(-12, 100),   S( -1, 108),   S( 19,  99),   S( 27,  88),   S(  5,  69),
            S(-17,  69),   S( -2,  79),   S( 16,  93),   S( 23, 112),   S( 11, 103),   S( 34, 118),   S(-23, 125),   S( 60, 110),
            S(-23,  76),   S( 19,  79),   S( 26,  86),   S(  3, 115),   S( 72,  87),   S( 82, 127),   S( 46, 106),   S( 11, 119),
            S(-40,  71),   S(-27,  87),   S(-15,  84),   S( 44,  72),   S( 10,  93),   S( 74,  58),   S( 18,   9),   S( 30, -10),
            S(-158,  90),  S(-34, 115),   S(-60, 123),   S( 33,  74),   S( 58,  98),   S(-59,  97),   S( 15, -34),   S(-54, -107),

            /* knights: bucket 4 */
            S( 16,  28),   S(  9,  -6),   S(-59,  10),   S(-31, -43),   S(-33,   6),   S( 12, -25),   S( 16, -47),   S(-17, -26),
            S( 30,  31),   S(  1, -26),   S(-18,  -7),   S(-18,   7),   S( 21, -20),   S( 21, -46),   S( 22,   9),   S(  0,  25),
            S(  7,  -2),   S( -4,   5),   S( 89,   1),   S( 79, -11),   S( 14,  32),   S( 54, -17),   S( -7, -14),   S(  0, -29),
            S(-13, -32),   S( 11,  17),   S( 65, -12),   S( 43,  17),   S( 33,  17),   S(  8,  31),   S(-14,  19),   S( 22,  10),
            S(-14, -40),   S( 13,  27),   S( 67,  20),   S( 41,  58),   S( 48,  -1),   S( 18,  45),   S( 15, -15),   S(-40,  57),
            S(-16, -37),   S(-17, -17),   S( 53,  -5),   S( 44,  24),   S( 12,  15),   S(-14,  68),   S(-14,   7),   S( 25,  23),
            S(-14, -19),   S(-22, -48),   S( -8, -29),   S(  6,  24),   S(  7,  24),   S(  1,  23),   S( -2,  39),   S(-50, -43),
            S(  0,   2),   S( -8, -28),   S( -3, -13),   S(  0,   7),   S(  8, -11),   S( -2,  12),   S(  9,  32),   S(  0,  10),

            /* knights: bucket 5 */
            S( 13,  43),   S( 13,  23),   S(-29,  14),   S(  0,  15),   S(-24,  21),   S( 34,  -3),   S(-37,  14),   S( 10,   7),
            S( 18,   5),   S( 56,  31),   S( 55,  -9),   S( -7,   6),   S( 66, -27),   S(-45,  14),   S( 11,  43),   S(-62,  -6),
            S(-58,  14),   S(  1,  -4),   S( 40,   8),   S( 48,  10),   S( 46,  11),   S( -3,  22),   S(  3,   0),   S(-53,   0),
            S( 51,  -5),   S( 51, -26),   S( 70,   5),   S( 89, -13),   S(100,  -4),   S( 95,  -4),   S( -2,  27),   S(  8,  19),
            S( 45,   1),   S( 63,  -8),   S(117, -13),   S( 91,  -7),   S(116, -22),   S( 47,  13),   S( 52,  -7),   S(  7,  26),
            S( -2, -22),   S( 40, -24),   S(-14, -41),   S(-20,  -6),   S( 38, -10),   S( 40,   8),   S( -3,  10),   S( 16,  26),
            S(  8,  -5),   S(-32, -51),   S( -2, -55),   S(-27, -36),   S( -8, -52),   S( 28,   7),   S( 17,  42),   S( 30,  31),
            S(-34, -39),   S(-33, -50),   S(  5, -23),   S(-23, -11),   S( 13,  -7),   S( -7,  21),   S( 10,   4),   S( 12,  46),

            /* knights: bucket 6 */
            S( -6, -12),   S(-28,  25),   S( -9,   0),   S(-12,  18),   S( -6,  30),   S(  5,  36),   S( 18,  25),   S(-24,  23),
            S( 18, -12),   S(-21,  32),   S(-25,   0),   S( 54,  -2),   S( 53,   3),   S( -6,  24),   S( 16,  39),   S(  0,  21),
            S(  0,  12),   S( 60,   0),   S( 29,  11),   S( 58,  25),   S( 57,  24),   S(-10,  35),   S( 36,  23),   S(-33,  39),
            S( 49,  24),   S( 93,  -3),   S( 62,  19),   S( 97,   6),   S(130, -13),   S( 82,  20),   S( 29,  11),   S(  2,  24),
            S(-30,  58),   S( 52,   5),   S(103,  17),   S(114,  -4),   S(110, -18),   S( 82,  16),   S(170, -29),   S( 18,  18),
            S( -2,  16),   S( 47,   3),   S( 55,  20),   S( 13,  35),   S( 36,  -6),   S( 37, -19),   S( -5,   3),   S( 13,  -7),
            S(  5,  28),   S( 11,  31),   S(  3,  49),   S(  6,  -3),   S( 36,  19),   S( 22, -37),   S(-12, -10),   S( 12,  48),
            S( 26,  26),   S( -3,  25),   S(  2,   1),   S( 12,   4),   S( 41, -11),   S(  7, -33),   S(  9,  58),   S(-27, -42),

            /* knights: bucket 7 */
            S( -6, -29),   S(-13, -25),   S( 45, -24),   S(-35,  16),   S( 28, -18),   S(-38,  27),   S(-10,   2),   S(-17,  29),
            S(-68, -54),   S( 12, -21),   S(-36,   5),   S(  1,   0),   S( 18,  10),   S( 29,  16),   S(-15,  24),   S(-32,   1),
            S(  5, -35),   S(-21, -12),   S( 10,   2),   S( 44,  13),   S( 88,  -2),   S( 50,  23),   S( 51,   1),   S(-13,  44),
            S(-32,   2),   S( 54,   4),   S( 60,  -3),   S( 76,  14),   S(121, -11),   S(100,   6),   S( 55,  14),   S( 52,   0),
            S( -1,   8),   S( 14,   1),   S( 40,  18),   S( 86,   8),   S(112, -13),   S(172, -18),   S(180,  -4),   S(-21,  35),
            S(-13,  14),   S( 39,  19),   S(  7,  29),   S( 84,  15),   S(116, -14),   S(105,   8),   S( 22, -22),   S( -4, -32),
            S(-22,  11),   S(-28,  24),   S( 23,  22),   S( 15,  50),   S( 67,  22),   S( 16,  22),   S(-20, -49),   S(-10, -30),
            S(-36, -27),   S(-30, -29),   S(-13,  56),   S(-23,  -2),   S(  0,   9),   S( 17,   5),   S(  0, -30),   S(  0,  -4),

            /* knights: bucket 8 */
            S( -4,  -6),   S(  4, -12),   S( -3,   1),   S(-17, -50),   S(-11, -35),   S(  2, -33),   S(  8,  20),   S( -8, -28),
            S(-18, -70),   S(-10, -72),   S(-12, -62),   S(-22, -31),   S( 14,  -7),   S(-14, -24),   S(-13, -47),   S( -2, -15),
            S(-16, -78),   S( -7, -14),   S( 14, -53),   S( 24, -18),   S( -5, -31),   S( 19,  -7),   S(  1, -18),   S(  7,  -9),
            S(-14, -67),   S(-11, -37),   S(  3,   2),   S( 22,   9),   S(  4, -43),   S( 20, -21),   S(-22, -68),   S(-20, -33),
            S( -7, -44),   S(  1, -45),   S(  1, -39),   S( 17, -18),   S( 14, -39),   S( -9, -36),   S(-11, -34),   S(  2, -33),
            S( -1, -10),   S(  7, -49),   S(  1, -33),   S( 16,  -5),   S( 12,  -1),   S( -9, -31),   S( -6, -21),   S( -8, -19),
            S( -7,  -8),   S(  4, -25),   S( -4,  -8),   S(  1,  15),   S( 11,   1),   S( 10, -16),   S(-10, -11),   S( -2,  -6),
            S(  1,   2),   S(  2,   3),   S( -4, -13),   S(  4,  -2),   S( -8, -11),   S( -7, -20),   S(  1,   2),   S( -6,  -2),

            /* knights: bucket 9 */
            S(-15, -81),   S( -1,   2),   S(-11, -49),   S(  5, -54),   S(-23, -46),   S(-12, -11),   S(  0, -17),   S(  2,  -8),
            S(-12, -42),   S(-17, -62),   S(-10, -128),  S(-10, -68),   S(-18, -39),   S( -4, -77),   S( -2, -37),   S(-10, -16),
            S(-18, -65),   S(-16, -57),   S(-10, -51),   S( 29, -66),   S( 10, -26),   S( 17, -16),   S(  3, -22),   S( -4, -20),
            S(-26, -85),   S( -6, -58),   S( -1, -49),   S( -3, -53),   S( -3, -59),   S( 12, -19),   S( -4, -54),   S( -1, -33),
            S( -2,  10),   S(-16, -62),   S( 19, -23),   S( 10, -22),   S(  1, -44),   S( -1, -41),   S(  2, -34),   S(-16, -45),
            S(-11, -39),   S(-19, -50),   S(-13, -39),   S( 20, -41),   S( -2, -32),   S( 13, -31),   S(-14, -31),   S( -7,  -7),
            S( -8, -26),   S(  3,  22),   S( -7, -21),   S(-15, -34),   S( 18,   0),   S( 13,  20),   S(-15, -10),   S( -1,   6),
            S(  1,   0),   S(  5,  -5),   S(  0,   0),   S( -6, -30),   S( -9, -29),   S(  2,  -8),   S(  5,   8),   S(  0,   9),

            /* knights: bucket 10 */
            S( -6, -24),   S(-13, -32),   S(-17, -57),   S(-16, -32),   S(-18, -50),   S(  4, -42),   S(  5,   9),   S( -3, -15),
            S( -3, -51),   S(  2, -20),   S(  4, -58),   S( -1, -74),   S(  7, -67),   S(-10, -89),   S( -3,   9),   S( -3,  32),
            S(  1, -29),   S(  1, -45),   S(  4, -47),   S( 29, -61),   S(  7, -62),   S(  0, -63),   S(-18, -49),   S( -9, -47),
            S( -6, -56),   S( -9, -62),   S( 28, -40),   S( 16, -27),   S(  6, -24),   S(  3, -14),   S(-14, -81),   S(  9, -21),
            S(-10, -37),   S( -3, -63),   S( 14, -40),   S( 11, -42),   S( 19, -38),   S( -9, -85),   S(-12, -25),   S( -4, -49),
            S(  2,   0),   S(-14, -48),   S( 13, -19),   S( -4, -23),   S(-11, -33),   S(-13, -32),   S( -6, -22),   S(-13, -36),
            S(  2, -19),   S( -7, -24),   S(  9,  -4),   S(  4, -26),   S( -1, -11),   S(-13, -57),   S(-10, -33),   S( -3,  -4),
            S( -2, -11),   S(  2,   6),   S(  0,  12),   S( -9, -26),   S( -5, -32),   S( -2, -24),   S(  3,  -1),   S( -5, -13),

            /* knights: bucket 11 */
            S( -8, -31),   S(-28, -74),   S( -5, -34),   S(  2,   7),   S(-40, -77),   S(-21, -49),   S(  2,  14),   S(  5,  20),
            S(-12, -38),   S(-32, -63),   S(  6, -66),   S( 18, -40),   S( 40,  -6),   S( -8, -63),   S( -9, -30),   S( -8, -18),
            S(-10, -58),   S(-24, -66),   S( 36,  -9),   S( 24,  -2),   S( 14,  -8),   S( 19, -32),   S( 22, -21),   S(-12, -40),
            S(-13, -25),   S( 14, -28),   S( 17, -34),   S( 26, -21),   S( 43, -32),   S( 14, -39),   S( 21, -26),   S(  7, -16),
            S(-16, -29),   S(-16, -71),   S( 12,  -9),   S( 27, -21),   S( 49,   7),   S( 10, -19),   S(-11, -72),   S(-11, -34),
            S(-12, -20),   S(-13, -80),   S( 18, -18),   S( 21,  -1),   S( 29,  11),   S( 11, -16),   S( -5, -47),   S( -4, -18),
            S(  0,  15),   S( -2,  30),   S(-10, -23),   S(  6,  12),   S( 10, -24),   S( 10, -36),   S( 11,  -4),   S( -6,   2),
            S(  0,  -8),   S( -6,  -7),   S( -4, -12),   S(  5,  17),   S(  1,   8),   S(  3, -11),   S(  2,   7),   S( -1,   0),

            /* knights: bucket 12 */
            S(-18, -61),   S( -5, -48),   S( -3, -28),   S( -2, -10),   S( -8,  -6),   S(-15, -36),   S( -3,  -4),   S( -4, -19),
            S( -4, -29),   S(  0,  -3),   S( -1,  -9),   S(-17, -63),   S(  1, -44),   S(  0, -13),   S(  9,  14),   S(  1,   0),
            S( -5, -19),   S(-14, -52),   S( -1,  -5),   S( -3, -76),   S( -3, -30),   S( -8, -27),   S( -4, -21),   S( -2,   2),
            S(  4,  21),   S( -2, -49),   S( -5, -30),   S(  5, -20),   S( 10, -30),   S(  4,  -3),   S(  5,   7),   S(  1,  12),
            S( -2,  -7),   S( -4, -50),   S( -1, -23),   S( 13,  -1),   S( 15,  60),   S( -6, -23),   S( -8, -11),   S(-11, -33),
            S( -7,  -8),   S( -2, -24),   S( -2, -38),   S(  7,  -2),   S( -5, -26),   S(-18, -52),   S(  7,  10),   S(  1,  -2),
            S( -4, -23),   S( -7, -20),   S( -4,  -8),   S( -4,  -2),   S(  2,  12),   S(-20, -49),   S(  0, -12),   S( -3,  -6),
            S(  0,   0),   S(  3,  26),   S(  1,  -3),   S(  3,   6),   S(  4,   7),   S(  7,  31),   S( -2, -11),   S(  0,  -4),

            /* knights: bucket 13 */
            S( -1,   2),   S( -4, -22),   S( -6, -29),   S(  1, -14),   S( -7, -38),   S( -4, -10),   S(-10, -28),   S(  2,  11),
            S( -3,  -8),   S( -6, -35),   S( -3, -15),   S( -9, -40),   S(-13, -51),   S(  3,  -8),   S(-10, -20),   S( -7, -18),
            S( -4, -28),   S( -2, -13),   S(  3, -19),   S(-12, -43),   S( 10,  33),   S( -3, -16),   S(  0, -11),   S(  3,  -5),
            S( -5, -20),   S( -7, -22),   S( -8, -43),   S(  2, -25),   S(  2, -28),   S( 20,   1),   S(  9,   5),   S( 13,  17),
            S(  1,  28),   S(  5, -33),   S( -4, -56),   S( -4, -73),   S( -6, -19),   S(  8, -20),   S( -7, -32),   S( -5, -25),
            S( -4, -13),   S(  3,  -3),   S(  0,   4),   S(  4, -30),   S( -7, -22),   S( -3, -44),   S(  6,  10),   S( -1,  -2),
            S( -1,  -5),   S( -1,  -2),   S(-14, -32),   S( -4, -13),   S(  0,   7),   S( -1,   6),   S(  2,  -3),   S(  1,   4),
            S(  1,   2),   S( -1,  -2),   S( -1,  -7),   S(  3,  11),   S( -2,  -6),   S(  0,   0),   S( -1,  -1),   S(  0,   1),

            /* knights: bucket 14 */
            S(  0,  -2),   S( -5, -20),   S( 10,  36),   S( -2,  -7),   S(-14, -67),   S(-11, -24),   S(  8,  10),   S( -1,  -6),
            S(  0,  -8),   S( -6, -34),   S( -6, -23),   S( -3, -49),   S(  0, -25),   S( -7, -32),   S( -8, -21),   S( -3,  12),
            S(  0, -22),   S( -9, -47),   S(  0,  -8),   S(-11, -61),   S(  2, -16),   S( -7, -12),   S(  0, -13),   S(  3,  19),
            S( -3,  -8),   S(  0, -25),   S(-11, -49),   S(  2,   8),   S(  1,  -5),   S( -6, -58),   S( -1,  -8),   S( -3,   4),
            S(  4,  -1),   S( -9, -29),   S(-16, -50),   S(  4,   5),   S(  1,   8),   S( 10,   9),   S( -3,  -4),   S( -2, -10),
            S( -1,  -3),   S( 11,  32),   S(  4,  48),   S( -1,  -4),   S(  9,  -8),   S( -2,  -8),   S( -3,  -6),   S( -4, -17),
            S(  0,  -2),   S( -5, -21),   S(  5,   9),   S(  9,  40),   S(  9,  30),   S( -6, -14),   S(  0,  -1),   S(  1,  -2),
            S(  2,   4),   S( -1,  -7),   S(  1,   6),   S(  2,  11),   S(  0,   1),   S(  1,   8),   S(  1,   4),   S( -1,  -2),

            /* knights: bucket 15 */
            S( -6, -25),   S( -3, -13),   S(  7,  23),   S( -2,  19),   S(  7,  -5),   S(-10, -53),   S( -2, -28),   S(  2,  10),
            S(  0,   0),   S(  4,  -3),   S(-12, -44),   S( 25,  51),   S(  6, -13),   S( -2, -22),   S( -5, -16),   S(  4,  11),
            S(  2,  -7),   S(-11, -36),   S( -3, -20),   S( 13,  18),   S(-14, -60),   S( -1, -40),   S( -4, -33),   S( -1, -16),
            S(  0, -21),   S( -3, -13),   S( -4, -10),   S(  4,   6),   S(  7, -25),   S(  2, -14),   S(  7,   0),   S( -1,   8),
            S( -2, -13),   S( 16,  26),   S( -4, -34),   S(  5,  27),   S( 30,  47),   S( 10,  10),   S(  8,   5),   S(  0,  -9),
            S( -2,   2),   S( -7, -16),   S(  2,  22),   S( -7, -19),   S(-11, -39),   S(  4,  29),   S(  1,  25),   S(  9,  30),
            S(  0,   1),   S(  0,  -2),   S(  5,  21),   S(  3,   3),   S(  6,  22),   S(  3,  14),   S(  0,   1),   S(  3,   4),
            S( -2,  -5),   S(  1,   5),   S(  0,   2),   S( -1,  -4),   S(  1,   5),   S( -2,  -3),   S(  1,  11),   S(  1,   5),

            /* bishops: bucket 0 */
            S( 58, -42),   S(-32,  24),   S( -8,  -4),   S( -2, -45),   S(  1,  -9),   S( 11, -16),   S( 69, -66),   S( 45,   1),
            S(-28, -20),   S( 13, -29),   S(-14,  35),   S( 14, -10),   S( 14, -12),   S( 70, -41),   S( 44,  15),   S( 63, -30),
            S(  5,  18),   S( 13,  15),   S( 18,   6),   S( 18,  -9),   S( 49, -27),   S( 54,  14),   S( 61, -23),   S( 12,  -8),
            S( 17, -44),   S( 53, -49),   S( 25,   0),   S( 76, -27),   S( 70,  25),   S( 62,  18),   S( 13,  -1),   S(  4,  -1),
            S( 58, -30),   S( 65, -24),   S( 95, -35),   S( 97,  -1),   S(132, -30),   S( 15,  30),   S( 48,   5),   S( -4,   3),
            S( 67,  22),   S(105,   5),   S(109,  -1),   S( 73,   9),   S( 39,   4),   S( 26,  37),   S( 56,  -9),   S( 10,  -1),
            S(-51, -94),   S( 84,   8),   S( 64,  66),   S( -7,  16),   S(  2, -31),   S( 14,  17),   S( -7,  24),   S(-18,  21),
            S(-19, -52),   S(  3,  11),   S(-28, -50),   S(-32,  -8),   S(-14, -18),   S(-26,  11),   S(-34,  11),   S(-21, -25),

            /* bishops: bucket 1 */
            S(-17, -23),   S( 22, -27),   S(-21,  18),   S( 29, -29),   S(-24,  11),   S( 13,   1),   S( 32, -19),   S( 45, -30),
            S(  3, -19),   S( -2,  -6),   S( 10,  -3),   S(-13,   6),   S( 40, -25),   S(  7,  -8),   S( 64, -33),   S( 11, -23),
            S(-16, -13),   S( 29, -19),   S(-14,   9),   S( 24,  -1),   S(  9, -13),   S( 51, -14),   S( 12, -16),   S( 79, -43),
            S( 48, -60),   S( 55, -24),   S( 35,  -1),   S( 18,  -7),   S( 62,  -5),   S(  9,  -1),   S( 68, -36),   S(  4,   1),
            S( 34, -44),   S( 85, -25),   S( 11,   1),   S(115, -15),   S( 80, -11),   S(109, -50),   S( 19,   1),   S( 35,  -9),
            S( 84, -55),   S( 55,  -8),   S( 57,  -7),   S( 80, -37),   S( 99, -41),   S(  4,  -1),   S(  1,  18),   S(-28,  -4),
            S( -8, -71),   S( 11, -34),   S(-13, -35),   S(  7,   0),   S( 35,  11),   S(  2,   2),   S( 35, -20),   S( -2,   2),
            S(  0, -25),   S(-19,  -4),   S( -6, -49),   S(-41,   3),   S(  1,   3),   S( 36, -29),   S( 36, -19),   S(-26, -42),

            /* bishops: bucket 2 */
            S( 17, -30),   S( -4, -19),   S(  1,   2),   S(-15,   1),   S( 14,  10),   S(-21,  11),   S( 26, -12),   S(  6, -17),
            S( 24, -31),   S( 25, -17),   S( -3,  -4),   S( 10,   3),   S(-11,  17),   S(  6,  -3),   S( -1,  -6),   S(  6, -42),
            S( 39,  -5),   S( 15,  -1),   S(  8,  16),   S( -7,   1),   S(  4,  19),   S( -6, -11),   S( -5, -15),   S( -8,  -7),
            S(  1, -14),   S( 54, -19),   S( 10,   5),   S( 48,  11),   S(  2,  20),   S( -1,  19),   S(-23,   6),   S( -2,  -4),
            S( 13,   5),   S( 18,  10),   S( 72,  -2),   S( 44,  14),   S( 33,  27),   S( 22,  10),   S( 24,  22),   S( 46, -22),
            S(-50,  38),   S( 22,  17),   S(  2,  -1),   S(105, -29),   S( 89, -25),   S(106,   1),   S( 76,  -4),   S( 23, -47),
            S(  4,  19),   S(  0, -11),   S(  7,  11),   S( 20,  15),   S(-75, -37),   S(-54, -11),   S(-19,  20),   S(-39, -31),
            S(-51, -17),   S(-27,  27),   S(-19,  15),   S(-48,  22),   S(-20, -22),   S(-47,   2),   S(-11, -26),   S(-58, -30),

            /* bishops: bucket 3 */
            S( 24,  18),   S( 57, -31),   S(  6,  -4),   S(  7,   9),   S( 18,  22),   S( -5,  47),   S( -8,  38),   S(  6, -16),
            S( 39,   1),   S( 29,  11),   S( 22,  14),   S( 13,  21),   S( 23,  20),   S( 10,  24),   S( 12,  14),   S( 24, -16),
            S(  1,   6),   S( 29,  46),   S( 23,  49),   S( 22,  32),   S( 16,  47),   S( 19,  34),   S( 10,  20),   S( 11,  18),
            S(-17,  11),   S( 21,  24),   S( 35,  52),   S( 46,  49),   S( 38,  38),   S( 25,  31),   S( 33, -17),   S( 32, -25),
            S( 20,  20),   S( 19,  47),   S( 15,  54),   S( 62,  60),   S( 55,  55),   S( 62,  25),   S( 28,  36),   S(  1,  35),
            S( 14,  24),   S( 25,  61),   S( 23,  37),   S( 33,  42),   S( 72,  29),   S( 63,  77),   S( 48,  75),   S( 19, 102),
            S(-21,  69),   S( 25,  45),   S( 30,  30),   S(  4,  43),   S( 15,  49),   S( 58,  67),   S(-48,  17),   S( 17, -22),
            S(-76,  47),   S( -6,  33),   S(-63,  56),   S(-29,  51),   S(-22,  31),   S(-117,  78),  S(  3, -19),   S( 17,  29),

            /* bishops: bucket 4 */
            S(-36, -18),   S(-38,   1),   S(-57,  19),   S(-33,  -8),   S(-22, -18),   S( -9, -14),   S(-15,  -7),   S(-38, -63),
            S(-25,  23),   S(-11,   0),   S( 26, -21),   S(-25,  -9),   S(-35,  10),   S( -1, -44),   S( -6, -24),   S(-12, -12),
            S(  3,   8),   S(-25,  -6),   S( 68, -21),   S( 30, -36),   S(  3, -13),   S(-14,  -1),   S(  6,  -9),   S(-34, -44),
            S( 10, -15),   S( 39,  -2),   S( 42, -25),   S( 14, -18),   S( 23,   6),   S( 56, -25),   S(-43,  -9),   S(  3, -17),
            S( 18,  -1),   S(  3, -59),   S( 43, -33),   S( 59, -13),   S( 50, -17),   S( 22,  16),   S( -2,   6),   S(-26,  -6),
            S(-65, -83),   S(-22, -55),   S( 31, -34),   S(  5, -16),   S(-33,  11),   S( 54,   9),   S(-13,  10),   S( -6,  14),
            S(  7,  11),   S( -3,  -7),   S( 10, -47),   S(-12, -52),   S(-18,   3),   S( 14,   9),   S(-18,  -5),   S( 43,  34),
            S(  7,  15),   S(-15, -70),   S(  2, -24),   S( -2, -40),   S(-25, -63),   S(-10,  -3),   S( -9,  50),   S( 13,  32),

            /* bishops: bucket 5 */
            S(-27,   1),   S(-10,  -5),   S(-43,   6),   S(-57,  12),   S(-27,  -3),   S( -2, -27),   S(-40, -15),   S(-39, -11),
            S(-41,   1),   S(-42,   4),   S( 31, -18),   S( 10,  -7),   S(-46,  14),   S(-13,  -2),   S( -3, -22),   S(  1, -39),
            S( 16,   1),   S(-40,  -4),   S( 27,  -4),   S(-28,   4),   S(  1,   5),   S(-63,   7),   S(-19,  -5),   S(-20,  15),
            S( 30, -22),   S(-14,   4),   S( 67, -24),   S( 69,  -6),   S(  5,   9),   S( 62, -10),   S(-70,  15),   S(-15,   3),
            S( 24, -34),   S( 15, -11),   S( -7, -21),   S(  3, -37),   S( 29, -25),   S( -1, -11),   S( 58, -10),   S(-71,   2),
            S( -7, -23),   S(-36,  -2),   S( 22, -44),   S(-16, -33),   S(-37,  -2),   S( 58, -15),   S( -2,  10),   S(-26,  16),
            S(-23, -21),   S(-13, -27),   S(-23,  -4),   S(-18,   5),   S( 11, -28),   S(  9, -13),   S(  5,  28),   S(-25,   6),
            S(-20, -51),   S( -9, -23),   S( -8, -30),   S(-15, -33),   S(-22,   4),   S( -4,  -2),   S(-43,  -8),   S(  0,  29),

            /* bishops: bucket 6 */
            S(-71,  -2),   S(-21,  -1),   S(-27,   0),   S(-33,  11),   S(-35,  11),   S(-37,  16),   S(-61,  12),   S(-64,  -1),
            S(-28, -15),   S(-27,  -9),   S(-57,  21),   S(  4,  -5),   S(-56,  15),   S(-41,  -5),   S(-52,  22),   S(-51,  -2),
            S( 10, -10),   S(-44,  11),   S( 11, -11),   S(-38,  11),   S( 18,   5),   S(-28,   6),   S(-47,  -6),   S( -9,  16),
            S(-11,  -7),   S(-24,  -4),   S(-17,  13),   S( 57,  11),   S( 39,  17),   S( 35,   0),   S( 33, -35),   S(  5,  -7),
            S(-49,  11),   S(-15,  15),   S( 58,  -8),   S( 75,  -6),   S(  8, -23),   S(-23,  -6),   S( 57, -24),   S(-22, -16),
            S(-31,  27),   S(  1,   6),   S(-23,  13),   S( 40, -12),   S(  3,  -3),   S(-19, -10),   S( 12,  10),   S( 11, -28),
            S(-19,  17),   S(-72,  31),   S( 39, -10),   S( -2, -11),   S(-18,   8),   S(  5,   0),   S( -4, -18),   S(-61,  -2),
            S(-20,  15),   S(-34,   0),   S( -5,  10),   S(  7,   2),   S( -3,   6),   S(  6, -17),   S(-34,  11),   S( -5,  -9),

            /* bishops: bucket 7 */
            S( 26, -54),   S(-41, -30),   S(-15, -43),   S(-12, -20),   S(-40, -10),   S(-32,  -5),   S(-62, -10),   S(-52, -15),
            S(  6, -76),   S(  7, -41),   S( 22, -33),   S( -7, -36),   S(-25,   4),   S(-32,  -8),   S( -2, -43),   S(-10,   1),
            S(-49, -25),   S(-28,  -6),   S( 12, -13),   S( 22, -13),   S(  9, -17),   S( 13, -15),   S(-65,   5),   S(-69,  14),
            S(-41, -23),   S(-55,   5),   S(-15,   2),   S( 45, -17),   S(118, -17),   S(-21,  -2),   S( 55, -33),   S(-13, -34),
            S(-45,  -2),   S(  4,   6),   S( 10, -10),   S( 51, -11),   S( 72, -20),   S( 46, -26),   S(-16, -14),   S(-70,  -7),
            S(-77,  17),   S( -1,  25),   S( 35,  -5),   S(-59,  18),   S(-46,   6),   S( 91, -32),   S( 42, -23),   S(-63, -105),
            S(-52,   9),   S( -4,  23),   S(-44,  18),   S( 29,  -6),   S(  2, -32),   S(  9, -39),   S( 29, -17),   S(-25, -49),
            S(-15, -16),   S(-26,   0),   S(-19,  10),   S(-16, -12),   S(  7, -14),   S(-17, -27),   S( 12, -28),   S( 13, -13),

            /* bishops: bucket 8 */
            S( 17,  92),   S(-14,  -2),   S( -3,  -4),   S(-20,  15),   S( -3,   5),   S(  2, -40),   S(-20, -42),   S( -9, -34),
            S( -6,  12),   S(  3,  37),   S(  0, -10),   S(  7,   3),   S( -1, -18),   S( 11,  20),   S(-18, -17),   S(-10, -23),
            S( -7, -37),   S(-19, -61),   S( 26,  58),   S( 32,   0),   S( 21,  31),   S(  0,   6),   S(-12, -28),   S(-30, -31),
            S( -4,  -7),   S( 12,  57),   S( -8,  14),   S( -2,  61),   S( 25,  22),   S( 17,  26),   S(-16, -10),   S( -1, -13),
            S(  9,  66),   S( 23,  95),   S( -4,   2),   S( 10,   2),   S(  0,  58),   S(-21,  23),   S(-10, -31),   S(  3,  26),
            S(-13, -28),   S( -4,  27),   S( 14,  39),   S( 11,  18),   S( 22,  13),   S( 16,  70),   S( -8,   8),   S( -1,  10),
            S(  3,  26),   S(-20, -62),   S( 16,  38),   S( -4,  42),   S(  6,  48),   S(  3,  43),   S( 13,  95),   S(-26,  -4),
            S( -5, -15),   S(  2,  -2),   S( -6,  -9),   S(  0,  25),   S(  7,  48),   S(  5,  28),   S( -2,  25),   S( 17,  89),

            /* bishops: bucket 9 */
            S( -1,  44),   S(-11,  16),   S(-13,  14),   S(-56, -15),   S(-27, -17),   S(-14, -23),   S( -9,   1),   S(-13, -27),
            S(-10, -45),   S(-17,  10),   S(  0,   8),   S(-11, -38),   S( -7, -30),   S( -3, -29),   S(-29, -38),   S(-23, -64),
            S( -8, -51),   S( 17,   8),   S( -7, -28),   S(  7,  27),   S( -3,  24),   S(-12,  -5),   S(  7, -15),   S(-19, -29),
            S(-10,  25),   S(-14,  13),   S(-15,  24),   S(  3,  29),   S(-22,  19),   S(  3,  -5),   S(  5,   6),   S(  5,  -4),
            S(  3,  51),   S(-11,  33),   S( 22,  21),   S(-14,  14),   S(-14,  21),   S(-12,  28),   S(  0,  22),   S(-10, -26),
            S( -9,  28),   S(-19,  -3),   S(-17,  25),   S( -2,  20),   S(-24,  24),   S(-34,  17),   S( -4,  18),   S( -8,  10),
            S( -1,  38),   S(-18,  25),   S( -6,  -1),   S( -6,  21),   S(-10,  -3),   S(-10,  15),   S(  9,  45),   S(  1,  38),
            S(  8,  64),   S(-10, -24),   S(-16, -19),   S(  3,  18),   S( -9,  39),   S( -8,  23),   S( -2,  61),   S( 14,  62),

            /* bishops: bucket 10 */
            S( -3,  20),   S( -5,  12),   S(-14, -33),   S(-16, -27),   S(-61, -42),   S(-41, -57),   S(-12,  13),   S( -7,   0),
            S( -8,  -8),   S( -3, -44),   S(-14, -36),   S(-19, -49),   S(-42, -22),   S(-32, -52),   S(-11, -41),   S(-10,   1),
            S(-19, -59),   S(-14, -39),   S(-15,  -2),   S( -8,  -6),   S(-12, -16),   S(  2, -18),   S(-15,   3),   S( -7, -51),
            S(-10, -10),   S(-27, -32),   S(-26, -15),   S(  1,  18),   S(-16,   1),   S(  7,  -5),   S( 19,  33),   S(-20,   1),
            S( -5,  11),   S(-35,  20),   S(-11, -11),   S(-18,  15),   S(  8,  20),   S( -8,  21),   S(-15,  17),   S( -3,  13),
            S(-23,  26),   S(-10,  14),   S(-20,  16),   S(-39,  24),   S(-27, -11),   S(  4,  14),   S(-18,   8),   S(  0,  36),
            S( -1,  38),   S(-11,  29),   S(-18,  13),   S(  0,  20),   S( -4,   2),   S(-13, -33),   S( -5,  -8),   S(  7,  56),
            S(-13,  39),   S( -2,  41),   S(  5,  36),   S(  4,  30),   S(-12,  14),   S(  6,  21),   S(  1,   2),   S(  0,  10),

            /* bishops: bucket 11 */
            S( 19,  16),   S(-18, -51),   S(-33,  -8),   S(-18,  -6),   S(-21, -27),   S( 20,  -5),   S(-23, -19),   S(-24,  42),
            S( -9,  36),   S( 21, -11),   S(  5,   2),   S( 15, -34),   S(  4,  -9),   S(-20, -17),   S(-22,  12),   S(  6,  30),
            S( -1, -36),   S( -1,  -2),   S( -3, -16),   S( -1,  -3),   S(-25, -15),   S( 14,  17),   S( -1, -28),   S(-11, -60),
            S( 11,   5),   S(  3, -29),   S( 15,  26),   S(-31, -19),   S(  4,  15),   S( 22,  42),   S( 18,  57),   S( -3, -12),
            S(-28,   4),   S(-26,  -5),   S(-41,  15),   S(-17,  42),   S( -9,   0),   S(  5,  26),   S(-25,  20),   S(  0,  30),
            S(-21,   2),   S(-13,   5),   S(-15,  47),   S(-10,  19),   S(-12,  45),   S(-18,  26),   S(  3,  14),   S(-12,   0),
            S(-15,  -3),   S(  4,  99),   S(  4,  32),   S(  1,  56),   S(  8,  22),   S(-10,  -6),   S(-16, -37),   S( -6,  -2),
            S(  9,  76),   S(-20,  34),   S(  8,  65),   S(  0,  43),   S(  2,  35),   S( -1,  15),   S( -9,   2),   S( 10,  24),

            /* bishops: bucket 12 */
            S( -7, -27),   S( -1, -11),   S(  2,  17),   S(  3,  24),   S( -6, -17),   S(-10,  -9),   S( -5,  -2),   S( -3, -10),
            S( -4, -16),   S(  5,  29),   S( -4, -24),   S( -8,  -1),   S( -1,  -9),   S( 11,  14),   S(-13, -24),   S( -2, -19),
            S(  8,  52),   S(  4,  36),   S( 12,  34),   S( 21,  16),   S(  2,   1),   S(  8, -45),   S(  9,  10),   S( -3,  -1),
            S(  5,  50),   S(  7,  64),   S(  1,  26),   S(  7,  40),   S( -2, -22),   S( -8,  -8),   S( -5,  -1),   S(  4,  -2),
            S(  6,   8),   S(  7,  29),   S(  8,  50),   S(  5,  37),   S( 10,  43),   S(  1,  26),   S( 12,  15),   S(  3,   1),
            S(  3,  11),   S( -3, -17),   S( -5,   6),   S(  1,   7),   S( 15,  63),   S( 13,  37),   S(-12, -14),   S( -5, -13),
            S( -3,  -4),   S(  5,  19),   S(  0,  -8),   S( -4,  -4),   S(  9,  49),   S(  8,  45),   S(  8,  39),   S(  3,  35),
            S(  1,   9),   S( -1,   6),   S( -1,   2),   S(  1,   7),   S(  4,  12),   S(  4,  10),   S(  7,  61),   S(  5,  30),

            /* bishops: bucket 13 */
            S( -8, -19),   S( -8, -30),   S( -5, -13),   S(-13, -10),   S( 10,  29),   S(-18, -52),   S(-16, -47),   S( -6, -33),
            S( -1,  34),   S( -8,  -5),   S( -3,   2),   S(  4,  50),   S( -9,  -6),   S(  6,  21),   S( -1,  -8),   S( -4,  -8),
            S(  2,  41),   S( 19,  74),   S( -4, -16),   S( 20,  48),   S( -2,  -7),   S(  4,  16),   S( -7,  -7),   S( -1,  -9),
            S( 11,  91),   S( 15,  67),   S(  6,  28),   S( -9,   7),   S(  7,  43),   S( -7, -10),   S( 14,  46),   S(  3,  18),
            S(  3,  65),   S(  5,  20),   S( -3,   0),   S( 13,  47),   S( -4,  18),   S(  6,  55),   S(  4,  23),   S( -1,   9),
            S(-10,  28),   S(  6,  36),   S( -2,  27),   S(  9,  34),   S( -9,  30),   S(-16, -44),   S( -1,  23),   S(  4,  18),
            S(  7,  34),   S( -4, -13),   S( -6, -16),   S(  1,  29),   S(  6,  33),   S(  6,  53),   S(  2,  18),   S(  5,  46),
            S(  0,  -6),   S( -3,   4),   S( -5,  -8),   S(  2,   3),   S(  2,  30),   S( -4, -20),   S(  8,  57),   S(  4,  27),

            /* bishops: bucket 14 */
            S(-15, -26),   S( -1,   5),   S(  5, -10),   S(  3,  35),   S(-18, -30),   S( -3,   0),   S( -9, -34),   S( -7, -33),
            S(  6,  26),   S( -5, -16),   S( -9,  -3),   S(  2,  17),   S( 11,  39),   S( -1,  17),   S(  4,  19),   S(  6,  27),
            S( -4,  -8),   S(  4,  16),   S(  7,  10),   S( 12,  43),   S( 19,  61),   S(  1,  39),   S( 12,  74),   S( -4,  11),
            S(  6,  40),   S(  4,  18),   S(-18,   2),   S( -4,  47),   S( 12,  38),   S(  8,  61),   S(  7,  70),   S( -1,  27),
            S(  0,  -2),   S(  0,  12),   S( -5,   7),   S( -3,  17),   S( -3, -10),   S(  0,  42),   S( 17,  47),   S( -3,  26),
            S( -3,   6),   S(  6,  21),   S( -4,  43),   S(  9,  27),   S(  5,  59),   S(  4,  40),   S( -4, -10),   S(  7,  25),
            S( 11,  49),   S( 16,  63),   S( -2,  31),   S(  6,  17),   S(  3,  25),   S( 12,  38),   S( -1, -21),   S(  5,  29),
            S(  5,  55),   S(  2,  40),   S(  3,  19),   S( -2, -19),   S( -3, -19),   S(  1,   0),   S( 11,  17),   S( -1,   5),

            /* bishops: bucket 15 */
            S( -1,   3),   S( -8, -21),   S( -7, -16),   S(-15, -45),   S( -9, -11),   S(-11, -52),   S( -7, -27),   S( -4, -15),
            S(  2,   6),   S(  4,   1),   S(  7,  16),   S(  5,  15),   S(  9,   1),   S( -3, -21),   S( -1,  11),   S( -2,   0),
            S( -1, -10),   S( -4,  -9),   S( -6,  29),   S( 26,  31),   S(  9,  19),   S(  7,  13),   S(  8,  48),   S(  2,  37),
            S(  1,   1),   S(  6,  26),   S(  4,  35),   S(-14, -25),   S( -3,   6),   S(  7,  30),   S( 10,  51),   S(  7,  41),
            S(-10, -20),   S( -6, -29),   S( 11,  51),   S( 31,  80),   S( 14,  63),   S( 12,  48),   S(  6,  25),   S( -5,  -4),
            S(  1,  14),   S(  3,  25),   S( 12,  54),   S(  9,  38),   S( 17,  46),   S(  4,  22),   S(  0,   3),   S( -1,  -3),
            S(  2,   0),   S(  6,  18),   S( -3,  27),   S( 13,  37),   S(  7,  22),   S( -1,   2),   S( -2,  13),   S( -1,   2),
            S(  0,   5),   S(  7,  42),   S(  2,  34),   S( 10,  11),   S(  3,  20),   S(  0,   4),   S(  3,  10),   S(  4,  19),

            /* rooks: bucket 0 */
            S( -6,  23),   S( 21,   3),   S(  1,   4),   S( 10,  10),   S(-21,  72),   S( -6,  37),   S(-32,  46),   S(-54,  61),
            S(  6,  -5),   S( 12,  26),   S(-31,  36),   S( -8,  48),   S( 10,  46),   S(  3,  23),   S(-28,  66),   S(-28,  63),
            S( -6,  13),   S(  5,  18),   S(-16,  25),   S(  2,  18),   S(-11,  43),   S(-11,  39),   S(-20,  60),   S( -7,  41),
            S(  6, -13),   S( 21,  30),   S(-39,  51),   S( 12,  27),   S( 16,  62),   S(-17,  52),   S(-36,  60),   S(-26,  46),
            S( 38, -33),   S( 46,  27),   S( 21,  36),   S( 27,  39),   S( 31,  35),   S( 36,  68),   S( 30,  66),   S( 15,  66),
            S( 52,   6),   S( 49,  83),   S( 65,  29),   S(105,  29),   S(  8,  75),   S( 19,  67),   S(  0,  68),   S(-35,  83),
            S( 35,  14),   S( 76, 103),   S(120,  80),   S( 73,  32),   S( 60,  58),   S( -1,  92),   S(-24,  96),   S(-25, 100),
            S( 28,  -5),   S( 40,  45),   S( 45,  40),   S( 32,  16),   S( 68,  55),   S( 58,  38),   S( 63,  28),   S( 44,  11),

            /* rooks: bucket 1 */
            S(-53,  55),   S(-23,  13),   S( -7,  21),   S(-36,  47),   S(-42,  52),   S(-39,  59),   S(-54,  77),   S(-77,  79),
            S(-55,  49),   S(-15,   3),   S(-35,  46),   S(-36,  47),   S(-44,  33),   S(-56,  48),   S(-12,   5),   S(-58,  61),
            S(-44,  28),   S(-31,  15),   S(-25,  26),   S(-20,  33),   S(-36,  30),   S(-46,  29),   S(-64,  65),   S(-26,  69),
            S(-62,  63),   S(-16,  29),   S(-11,  47),   S(-16,  32),   S(-60,  64),   S(-80,  99),   S(-48,  68),   S(-87,  96),
            S(-16,  51),   S(  4,  -5),   S( 22,  37),   S( 25,  29),   S(-12,  44),   S(-20,  78),   S(-30,  69),   S(-30,  95),
            S( 52,  34),   S( 66,  10),   S( 87,   5),   S( -6,  51),   S(  3,  42),   S(-11,  72),   S( 29,  68),   S( -3,  79),
            S( 32,  81),   S( 52,  23),   S( 25,  53),   S(  2,  47),   S( 62,  30),   S( 34,  60),   S( 53,  86),   S( 24, 109),
            S( 65,   2),   S( 51, -13),   S( -5, -17),   S(-12, -22),   S( 41,  17),   S( 15,  49),   S( 59,  39),   S( 74,  40),

            /* rooks: bucket 2 */
            S(-63,  84),   S(-48,  72),   S(-50,  81),   S(-41,  51),   S(-26,  50),   S(-39,  43),   S(-32,  37),   S(-73,  72),
            S(-67,  83),   S(-59,  74),   S(-43,  66),   S(-60,  63),   S(-53,  60),   S(-59,  53),   S( -4,   6),   S(-48,  44),
            S(-48,  73),   S(-47,  76),   S(-49,  66),   S(-37,  59),   S(-50,  63),   S(-36,  57),   S(-16,  31),   S(-20,  54),
            S(-40,  99),   S(-32,  84),   S(-58,  93),   S(-79,  86),   S(-53,  81),   S(-19,  59),   S(-26,  54),   S(-21,  53),
            S(-37, 109),   S(-32,  99),   S(-29, 102),   S(-36,  76),   S(-32,  73),   S( 20,  58),   S( -8,  66),   S(-16,  90),
            S( 11, 105),   S( 25,  90),   S( 11,  84),   S(-15,  75),   S( 64,  47),   S( 66,  51),   S( 79,  42),   S( 82,  67),
            S( 39, 107),   S( -5, 107),   S( 61,  71),   S( 40,  56),   S( 30,  38),   S( 55,  82),   S(-58, 127),   S( 41,  96),
            S( 60,  55),   S( 37,  83),   S( 43,  62),   S( 28,  34),   S(-27,  54),   S( 49,  59),   S( 28,  63),   S( 58,  46),

            /* rooks: bucket 3 */
            S(-13, 103),   S( -9, 105),   S(-17, 137),   S( -9, 125),   S(  0,  86),   S(  7,  81),   S( 26,  61),   S(-11,  48),
            S( -7, 102),   S(-12, 114),   S(-18, 134),   S(-15, 135),   S( -3,  87),   S(  9,  62),   S( 56,  21),   S( 10,  66),
            S(  5, 104),   S(-17, 124),   S(-25, 129),   S(-15, 135),   S(  3,  90),   S(  1,  80),   S( 39,  72),   S( 22,  58),
            S(  0, 132),   S( -6, 141),   S(-17, 148),   S(-15, 142),   S(-18, 117),   S(  6, 100),   S( 21,  92),   S( -3,  89),
            S( -5, 147),   S(-22, 159),   S(  2, 152),   S( -5, 150),   S(  1, 129),   S( 35, 108),   S( 57,  96),   S( 45,  88),
            S( -6, 165),   S( 13, 142),   S( 17, 156),   S( 13, 149),   S( 87,  94),   S( 97,  92),   S( 84, 102),   S( 21, 117),
            S(  2, 168),   S(-15, 167),   S( 17, 165),   S( -1, 172),   S( 28, 148),   S( 96, 113),   S(118, 182),   S(215, 103),
            S(104,  51),   S( 41, 113),   S( 41, 146),   S( 21, 135),   S( 55, 113),   S( 95,  93),   S( 85,  70),   S(122,  50),

            /* rooks: bucket 4 */
            S(-30, -26),   S(  3, -26),   S( -1, -31),   S(-26,   0),   S(-47,   5),   S(-26,  45),   S(-47,   7),   S(-91,  46),
            S( -6, -54),   S(-47,   1),   S( 16, -24),   S(-23, -32),   S( 28,  -3),   S(-32,  18),   S(-34,  17),   S( 10,   8),
            S(-24, -16),   S(-33, -21),   S( 11, -16),   S( -6, -39),   S(-49,  16),   S(-62,  17),   S(-34,  -6),   S(-63,  22),
            S(-42, -34),   S( 43,  -9),   S( 14, -28),   S(  8, -19),   S( 39, -11),   S(-11,   5),   S(-10, -16),   S(-14,  -2),
            S(-26, -36),   S(  2, -31),   S( 44,   2),   S( 38, -22),   S( 60,  -7),   S( 31,  12),   S( 18,   5),   S( 45,  10),
            S(-15, -67),   S(  5,  -2),   S( 17, -25),   S( 25,  14),   S( 37,   8),   S( 11,  10),   S( 14,  19),   S( 51,  24),
            S(-12, -28),   S( 48,  15),   S( 48,  -7),   S( 58, -24),   S( 36,   6),   S(  3,   2),   S(  7,  -8),   S( 23,  -3),
            S(  4, -16),   S( 36, -13),   S( 53, -31),   S( 19,  -1),   S( 43,   5),   S(  3,   7),   S( -1,  -1),   S(  1,  16),

            /* rooks: bucket 5 */
            S(-29,  20),   S(-18,   1),   S( -3,   4),   S( 31,  -4),   S(  7,   2),   S( -9,  25),   S( -8,  39),   S(-46,  39),
            S( -7, -15),   S(-37, -23),   S( 19, -34),   S( 29, -15),   S(-10, -10),   S(-23,   2),   S(-92,  32),   S(-32,  25),
            S(-48,  22),   S(-15,  -7),   S( 11, -22),   S(-36,   4),   S(-52,  17),   S( 44, -21),   S(-73,  29),   S(-45,  10),
            S(-61,  28),   S(-18,   3),   S( 41, -28),   S( 53, -12),   S( 41, -11),   S(-23,  43),   S(-10,  12),   S(-11,  39),
            S( 45,  14),   S(  5,   7),   S( -6,  33),   S( 10, -20),   S( -9,  10),   S( 85,  -6),   S( 15,  24),   S( 16,  47),
            S( 26,  24),   S(-11,  19),   S( 14,  15),   S( -2,  -2),   S( 35,  25),   S( 29,  24),   S( 65,   3),   S( 23,  38),
            S( 69,  -9),   S( 67, -33),   S( -2,  -7),   S( 39,  19),   S( 62,  -5),   S( 62, -27),   S( 70, -18),   S( 46,  10),
            S( 16,  20),   S(  9,   7),   S( 37,  -2),   S( -8,  41),   S( 52,   6),   S( 25,  21),   S( 29,  27),   S( 23,  40),

            /* rooks: bucket 6 */
            S(-59,  41),   S(-18,  16),   S(-29,  22),   S(-16,   4),   S( -4,   3),   S( 21, -17),   S( -2,   1),   S(-30,   2),
            S(-86,  43),   S( -3,  12),   S(-44,  29),   S(-45,  14),   S(-18,   1),   S(-34,   0),   S(-40,  -2),   S(-45,  10),
            S(-74,  42),   S(-14,  12),   S(-35,  13),   S(-26,  11),   S(-33,  20),   S( 17,  -1),   S(-16, -12),   S(  3,  -8),
            S(-52,  43),   S(-49,  34),   S(  5,  10),   S( 17,   7),   S( 29,   0),   S(-22,  18),   S( -5,  -3),   S( -6,  32),
            S(  0,  44),   S( 53,  20),   S(118,   1),   S( 64,  -3),   S(  7,  -3),   S( -1,  23),   S( -3,  14),   S( 83,   6),
            S( 87,  11),   S( 71,   7),   S( 92,   9),   S( 65,  -9),   S(  4, -27),   S( 30,  49),   S( 53, -17),   S( 85,  13),
            S( 35,  12),   S(114, -25),   S(122, -42),   S(120, -49),   S( 35,   0),   S( 39,   2),   S( 70, -29),   S( 79, -22),
            S( 40,   7),   S(-28,  44),   S(-35,  47),   S( 78, -21),   S( 41,  16),   S( 38,  29),   S( 53,  22),   S( 73,  12),

            /* rooks: bucket 7 */
            S(-107,  35),  S(-83,  31),   S(-103,  60),  S(-64,  41),   S(-59,  16),   S(-23, -13),   S(-57,  37),   S(-48, -18),
            S(-88,  41),   S(-68,  27),   S(-72,  32),   S(-70,  46),   S(-58,   6),   S( -9,  -6),   S( -3,   3),   S( 19, -56),
            S(-81,  33),   S(-84,  29),   S(-43,   9),   S(-74,  47),   S(-40,  16),   S(-42,  27),   S( 57, -27),   S( -3, -18),
            S(-82,  37),   S( -6,   8),   S(-12,  15),   S( 63, -18),   S(  5,  -5),   S( 52, -18),   S( 52,  23),   S( 21, -23),
            S(-29,  37),   S( -1,  33),   S( 58,  10),   S( 67,   3),   S(114, -27),   S( 99, -25),   S(110, -17),   S(-39, -22),
            S( 15,  31),   S(  3,  24),   S( 98,   4),   S( 90,  -6),   S( 89,   6),   S( 47,  17),   S( 26,   7),   S(-12, -20),
            S(-22,  20),   S( 23,  -7),   S( 71, -10),   S( 53,  -7),   S(125, -33),   S(112, -25),   S( 54,   8),   S( 32, -21),
            S(-41,   9),   S(-15,  23),   S( 35,  17),   S(  2,  11),   S( 13,   6),   S( 62,   5),   S( 72,  16),   S( 29,  -8),

            /* rooks: bucket 8 */
            S(-20, -79),   S( 30, -41),   S( 33, -48),   S( 16, -15),   S(-12, -56),   S(-13, -30),   S(  3, -40),   S(-13, -46),
            S(-11, -67),   S( -3, -30),   S(-23, -22),   S(-29, -82),   S(  7, -49),   S( -5, -31),   S(  4, -12),   S(-39, -40),
            S( 20,  10),   S( -1, -19),   S( 31,  64),   S(-21, -26),   S(  3,  18),   S( 16,  27),   S( 10,  23),   S(-22, -26),
            S( -6, -32),   S( -4, -19),   S(  4, -17),   S( 16, -14),   S( 12,  24),   S( 27,  14),   S(  6,  26),   S(  2, -15),
            S(-16, -63),   S( 14,  -5),   S( 18, -22),   S( 39,  12),   S(  8,  16),   S(-15, -34),   S( 18,  55),   S( -5,  -9),
            S( -2, -31),   S( 16, -22),   S(  7, -12),   S(-10,  -9),   S( -2,  27),   S( -4, -24),   S( -4,   0),   S(  2,  11),
            S( 14,  17),   S( 18, -31),   S( 11, -23),   S( 23,   0),   S( 11,  16),   S( 12,  23),   S( -1,  22),   S( -9,   9),
            S(  0,   3),   S( 25, -23),   S( 24, -13),   S( 27,  48),   S( -6,   2),   S( 15,  36),   S(  6,   9),   S(  6,   1),

            /* rooks: bucket 9 */
            S(-20, -101),  S(  0, -98),   S( -7, -104),  S( 36, -76),   S( 30, -81),   S( 21, -66),   S(  6, -59),   S( 17, -87),
            S(-40, -80),   S(-24, -85),   S(-11, -64),   S(-20, -65),   S( -1, -72),   S( -2, -16),   S(-30, -84),   S( -9, -42),
            S( -8, -15),   S( -8, -52),   S(  6, -17),   S(  6, -27),   S( 31, -48),   S( -7,  -5),   S(  3,  16),   S(-11,  12),
            S( 18, -26),   S( 13,   0),   S(  2,  -1),   S(  5,   0),   S( -8, -60),   S( 24, -39),   S(  0,  -7),   S( -4, -49),
            S( 25, -49),   S(-14, -57),   S(-11, -75),   S( -5, -37),   S( -5, -59),   S( -8, -39),   S(-15, -52),   S(-15, -56),
            S( 16, -38),   S(-22, -52),   S(  8, -42),   S( 19, -18),   S( 14, -27),   S( 13, -10),   S(-13, -45),   S(  6, -27),
            S(-14, -19),   S(  2, -33),   S( -6, -48),   S(  1,  -7),   S(  4, -39),   S( 12, -36),   S( -2, -26),   S(-17, -39),
            S(-17, -28),   S( -2, -34),   S( 18, -20),   S( 11,  -9),   S(  8, -18),   S(  5,  -5),   S( -6,  -7),   S(-11, -11),

            /* rooks: bucket 10 */
            S(  6, -82),   S(-22, -70),   S( 15, -91),   S( 24, -98),   S( 17, -88),   S(  9, -116),  S( 31, -89),   S( -9, -68),
            S(-32, -67),   S(-45, -57),   S(-31, -53),   S(-16, -86),   S(-33, -83),   S( -6, -75),   S( -7, -77),   S(-40, -99),
            S(-14, -33),   S(-26, -25),   S(-20, -41),   S(-39, -56),   S(-25, -44),   S( -2, -52),   S( -1, -60),   S(-23, -56),
            S(-23, -50),   S(-38, -63),   S(-10, -65),   S( -3,  -7),   S(  1, -22),   S(  6,  17),   S( -6, -78),   S(  3, -60),
            S( -7, -39),   S(  0, -33),   S(  1, -52),   S( -6, -60),   S( 11, -12),   S(  4, -48),   S( 18, -58),   S(-17, -89),
            S(-15, -37),   S( -3, -36),   S(  1, -63),   S( 17, -74),   S( 14, -32),   S(-10, -57),   S(-22, -58),   S(  2, -51),
            S(-29, -38),   S(-15, -50),   S( -5, -32),   S( -1, -51),   S( 18, -32),   S(-17, -57),   S( -2, -75),   S( -3, -48),
            S(-15, -23),   S(  6,  10),   S( 11,  -9),   S( -6, -28),   S(  6,  -1),   S(-17, -42),   S(  3, -50),   S(-22, -20),

            /* rooks: bucket 11 */
            S(-14, -76),   S(-16, -47),   S(-14, -49),   S( -6, -24),   S(  0, -49),   S( 32, -65),   S( 35, -60),   S( -5, -100),
            S( -9, -30),   S(-18, -39),   S(-40, -21),   S(-37, -37),   S(-25, -29),   S( -5, -20),   S(-27, -61),   S(-28, -74),
            S(-37,  -4),   S(-20, -15),   S(  0,   4),   S(-14, -16),   S( -5, -16),   S( 13, -11),   S( 15, -25),   S(-11, -11),
            S( -6, -45),   S( -4, -41),   S(-12,   5),   S( 19,  -5),   S( 29,  -5),   S( -4, -34),   S(  5,  -8),   S( -5, -24),
            S( -7, -38),   S(  6, -40),   S( 20,  -6),   S( -4,  -9),   S( 34,  -4),   S(  1, -39),   S( 30,  -8),   S(-13, -48),
            S(-12, -16),   S(-22, -32),   S(  8, -47),   S(  2, -25),   S(-15, -14),   S(  6, -25),   S( 31,  -3),   S( -7, -32),
            S( -6,   8),   S(-10, -39),   S(-12, -14),   S( -2, -28),   S(  4, -28),   S( 46, -25),   S( 30, -26),   S( -2, -24),
            S(  4, -17),   S( 29,  27),   S(  1,  27),   S( 19,  10),   S(-23, -19),   S( 12, -27),   S( 52, -29),   S(-19,   4),

            /* rooks: bucket 12 */
            S(-33, -115),  S(  6,  -7),   S( 11, -21),   S(-15, -78),   S( -3, -48),   S( 10, -33),   S(-32, -79),   S(-20, -59),
            S(  6,   1),   S(  2,   5),   S(  5,   4),   S(  6, -18),   S( 11,  -8),   S( 13, -14),   S(  4,  -2),   S(-19, -54),
            S(-16, -37),   S(  5,  13),   S(  9,  -2),   S( 25,  13),   S( -9, -71),   S( 16,  -1),   S(  3,   9),   S(  1, -15),
            S(  0,   0),   S(  1,  10),   S( 15,  30),   S( 12,  10),   S( 17,  29),   S(  4,  -9),   S(  8,  17),   S(  2,  14),
            S( 11, -13),   S( 17,  33),   S(  9, -13),   S(  1, -19),   S( 14,  39),   S( -5, -12),   S( -7, -46),   S( -4, -10),
            S(  2, -21),   S( -7, -27),   S(  5, -26),   S(-13, -42),   S(  2, -11),   S( -7, -24),   S(  9,  26),   S(  3,  -2),
            S(-16, -19),   S( -7, -10),   S(  5,  -1),   S(  5,  -3),   S(  2, -21),   S( 13,  11),   S(  7,  23),   S( -7, -11),
            S( -9, -21),   S( -4, -12),   S(  5, -30),   S( 12,   3),   S(  3,  -4),   S(  3,   6),   S(  4,  22),   S( -2,  14),

            /* rooks: bucket 13 */
            S(-10, -42),   S(-14, -84),   S(-24, -79),   S(-12, -44),   S( -8, -91),   S( 10, -16),   S(-35, -82),   S(-21, -61),
            S( -7, -45),   S( -3, -27),   S(  3,   5),   S( -2, -23),   S( 21,  27),   S(  0, -44),   S( 13, -38),   S( -6, -49),
            S( -3, -36),   S(-17, -54),   S( -7, -27),   S(  3, -32),   S( 16,  -7),   S( 13,  -7),   S( 18,  12),   S( -9, -77),
            S(  3, -16),   S( -3,  -9),   S( 11,   2),   S( 19,  -4),   S( 10,  -3),   S( -1, -35),   S(  8, -10),   S(  8,  30),
            S(  0, -28),   S( -5, -68),   S(  5, -41),   S( 14,  -5),   S( -1, -61),   S( -1, -18),   S( -2, -28),   S( -6, -11),
            S(  3, -14),   S( -2, -37),   S( -2, -52),   S(-10, -63),   S(-24, -128),  S(  5, -21),   S( -6, -13),   S(  1, -31),
            S(-14, -70),   S(  3, -36),   S(-28, -86),   S( 17,   5),   S(-15, -52),   S( -4, -49),   S( -4, -19),   S( -9, -48),
            S(  9,  26),   S(-16, -50),   S(-20, -48),   S(  0, -38),   S( -6, -50),   S(  0, -23),   S( -1, -12),   S( 10,  30),

            /* rooks: bucket 14 */
            S(  0, -38),   S(-39, -86),   S(  7, -34),   S(-13, -87),   S(-15, -67),   S( 21, -11),   S(-39, -130),  S(  0, -62),
            S(  8, -20),   S( 10,  -9),   S(  1, -44),   S(  1, -33),   S( -5, -25),   S(  2,   5),   S(  6, -19),   S(  0, -42),
            S(  3, -12),   S(  5,  -8),   S( -9, -41),   S( -7, -57),   S( 14,  15),   S(  0, -19),   S(  6, -39),   S(-25, -100),
            S( -9, -35),   S(  6,  12),   S(  2,  -7),   S(  2, -29),   S(-14, -48),   S(-10, -41),   S(  0, -24),   S(-15, -51),
            S(  8,   9),   S( 20,  64),   S(  8, -10),   S(  1, -47),   S(  6, -22),   S( 12,  -6),   S( -3, -49),   S(  2, -17),
            S( -6, -30),   S(  9,  29),   S( -1, -19),   S(  7, -72),   S(  0, -67),   S(-10, -80),   S( -5, -82),   S(-11, -56),
            S(-11, -30),   S(  0,   1),   S(-15, -68),   S(-23, -93),   S(  0, -33),   S(  5, -25),   S( -9, -66),   S(-11, -55),
            S(-12, -57),   S(-13, -47),   S( -5, -29),   S( -4, -69),   S(-21, -76),   S(-19, -96),   S(  1, -46),   S( -7,  -9),

            /* rooks: bucket 15 */
            S(-11, -66),   S(-14, -51),   S(-32, -61),   S(-16, -55),   S( -7, -44),   S(-14, -42),   S( -3, -19),   S(-10, -48),
            S(  4, -13),   S( -3, -34),   S( -9, -43),   S( -5, -43),   S( -3, -48),   S(  7,  -3),   S(  4,   6),   S(  3,  -5),
            S( -5, -12),   S(-11, -53),   S( 14,   0),   S( 14,  -3),   S( 15, -27),   S(  0, -15),   S( 16,   7),   S(  8,  13),
            S(  4,  15),   S( -3,  -3),   S( 16,  38),   S(-17, -44),   S(  7,  15),   S( -2, -19),   S(  5,  -5),   S( -3, -58),
            S(  7,  19),   S(  1, -15),   S(  8,  -3),   S( -1,  -7),   S(  2,  -9),   S(  3, -30),   S(  5, -22),   S(  2, -40),
            S(  7,  -2),   S(  2,   5),   S(  9,  21),   S( -6, -22),   S( 11,  -7),   S(  4, -48),   S(  2, -27),   S( -5, -13),
            S( -3, -17),   S(  3,   4),   S(  6,   8),   S( -7, -16),   S( -3, -12),   S(  2, -33),   S(  4, -14),   S(-21, -52),
            S( -8, -36),   S(  5,  19),   S(  6,  13),   S( -2,  -3),   S( -3, -12),   S(-12, -59),   S( -3, -58),   S(-28, -71),

            /* queens: bucket 0 */
            S(  6,  -5),   S(-36, -45),   S(-30, -87),   S( -2, -107),  S(  5, -80),   S(-16, -47),   S(-86, -59),   S(  6,  -9),
            S(-24, -76),   S( 10, -88),   S(  1, -66),   S( -1, -52),   S( -2, -40),   S(  1, -85),   S(-28, -70),   S(-42, -15),
            S(  1, -11),   S( -3, -28),   S( 13, -41),   S( -5, -23),   S(-14,   7),   S( -2, -33),   S(-16, -32),   S(-90, -80),
            S(-35,  52),   S( 11, -11),   S(-27,  35),   S(-15,  33),   S( -7,  51),   S(-31,  18),   S(-29, -29),   S(-31, -41),
            S(-23,  16),   S(-17,  67),   S(-34,  86),   S(-42,  88),   S(-13,  60),   S(-16,  60),   S(-72,  38),   S(-52, -14),
            S(-29,  22),   S( 19,  62),   S( 44,  69),   S(-35,  93),   S(-57,  83),   S(-33,  56),   S(-87,  26),   S(-47, -38),
            S(  0,   0),   S(  0,   0),   S( 36,  12),   S(-40,  34),   S(-22,  16),   S(-79,  66),   S(-105,  65),  S(-96,   4),
            S(  0,   0),   S(  0,   0),   S( 29,  37),   S(  3,  23),   S(-44,  43),   S(-42,  29),   S(-68,  28),   S(-48,  -1),

            /* queens: bucket 1 */
            S( 28, -38),   S( 11, -41),   S(  3, -67),   S( 20, -98),   S( 25, -54),   S(  5, -56),   S( -2, -48),   S( 16, -12),
            S(-28,   5),   S( 29, -14),   S( 37, -87),   S( 26, -20),   S( 31, -24),   S( -5, -27),   S(-38,   2),   S(-38, -16),
            S( 19,  14),   S( 23, -36),   S( 13,   7),   S( 21,   3),   S(-16,  34),   S( 28,   6),   S(-13,   6),   S( 15, -44),
            S( 21,   7),   S( 10,  51),   S( -9,  68),   S( 11,  80),   S(-10,  88),   S(-10,  47),   S( -1,  24),   S(-31,  64),
            S( 17,   2),   S( 31,  55),   S( 31,  67),   S(  6,  70),   S( 46,  68),   S( 59,  28),   S(-38,  86),   S( -3,  50),
            S( 60,  11),   S( 92,  63),   S( 88,  75),   S( 97,  95),   S( 88,  68),   S( -6,  90),   S( 16,  75),   S(  2,   0),
            S( 95,  -5),   S( 57,  18),   S(  0,   0),   S(  0,   0),   S( -3,  75),   S(-34,  76),   S(-24,  89),   S(-59,  46),
            S( 50,  35),   S( 38,   4),   S(  0,   0),   S(  0,   0),   S( 20,  40),   S( 29,  57),   S( 54,  34),   S(  3,  31),

            /* queens: bucket 2 */
            S( 30, -27),   S( 30, -40),   S( 22,  21),   S( 39, -24),   S( 46, -38),   S( 33, -51),   S(-28, -22),   S( 42, -11),
            S( 16, -12),   S( 14,  17),   S( 38, -17),   S( 41,  13),   S( 50, -15),   S( 15, -11),   S( 10,  -1),   S( 18,  50),
            S( 44,  21),   S( 28,  38),   S( 11,  84),   S(  9,  51),   S( 30,  37),   S( 14,  46),   S( 34,  19),   S( 23,  35),
            S( 31,  31),   S( 10, 100),   S( 13,  81),   S( -2, 114),   S( 31,  83),   S( 11,  82),   S( 13, 102),   S( 39,  79),
            S(-16, 100),   S( 19,  23),   S(-11, 132),   S( 38,  98),   S( 22, 154),   S( 83,  65),   S( 54,  88),   S( 67,  30),
            S(-43, 102),   S(-30, 105),   S(-10, 114),   S( 79,  94),   S( 71,  90),   S( 91,  99),   S(135,  66),   S( 19, 141),
            S(-29, 112),   S(-24,  97),   S( -7, 109),   S( 96,  56),   S(  0,   0),   S(  0,   0),   S( 36, 106),   S( 50,  82),
            S( -8,  62),   S( 51,  30),   S( 93,   7),   S( 66,  69),   S(  0,   0),   S(  0,   0),   S(104,  46),   S( 34,  90),

            /* queens: bucket 3 */
            S(-16,  54),   S(-15,  36),   S(  0,  30),   S(  8,  50),   S(  2,  11),   S(  9, -21),   S( 10, -58),   S(-40,  30),
            S(-51,  60),   S(-16,  36),   S( -2,  56),   S(  7,  76),   S(  7,  50),   S(  7,  30),   S( 33, -11),   S( 65, -71),
            S(-33,  57),   S(-13,  91),   S(-13, 122),   S(-22, 139),   S( -5,  95),   S( -6,  93),   S(  9,  69),   S(  2,  45),
            S(-18,  62),   S(-37, 111),   S(-26, 149),   S(-13, 161),   S(-19, 157),   S( -5, 115),   S( 12, 106),   S( -5, 102),
            S(-23,  86),   S(-31, 143),   S(-28, 147),   S(-28, 178),   S(-12, 176),   S(  7, 184),   S(-13, 159),   S(-17, 138),
            S(-51, 111),   S(-44, 141),   S(-59, 182),   S(-48, 201),   S(-41, 223),   S( 30, 168),   S( -6, 193),   S(-14, 194),
            S(-87, 157),   S(-74, 144),   S(-77, 201),   S(-89, 211),   S(-89, 224),   S( -9, 145),   S(  0,   0),   S(  0,   0),
            S(-111, 168),  S(-90, 157),   S(-74, 150),   S(-69, 166),   S(-33, 143),   S(-23, 119),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-43,  18),   S(-55, -62),   S(-33,   2),   S(-43, -41),   S(-14, -13),   S(  2,  -2),   S(-29, -34),   S( 19,  13),
            S(  4,  -5),   S( -5,  34),   S(-14,   7),   S(-42,  -9),   S(-22,  -2),   S(-25,  32),   S(-33, -31),   S(  6,  -1),
            S(-21,  14),   S( 23, -29),   S(  4, -13),   S(  7,   1),   S( 34,  -4),   S(-11,   5),   S(-34, -40),   S( 16,   0),
            S(-10,  -6),   S( 33,  11),   S( -8,   0),   S( 16,  44),   S( 33,  17),   S(-14,  20),   S(-14,  -3),   S(-20,  -6),
            S(  0,   0),   S(  0,   0),   S( 24,   2),   S( 64,  60),   S(  7,  32),   S(  1,  31),   S(-17,  -9),   S( 44,  38),
            S(  0,   0),   S(  0,   0),   S( 44,  27),   S( 38,  38),   S( 24,  48),   S(  4,  51),   S(-10,  20),   S(-12,  -1),
            S( 13,   3),   S(  7,  14),   S( 67,  44),   S( 62,  59),   S( 47,  22),   S(  5,  21),   S(-17,  13),   S(-45,  17),
            S( 50,  24),   S( -2, -22),   S( 50,  41),   S( 46,  27),   S( 48,   9),   S( -9,  -7),   S(-54, -45),   S( 21, -17),

            /* queens: bucket 5 */
            S( 26,   0),   S( 15,   0),   S( -5,  21),   S(-28,   3),   S(  6, -38),   S( 27,  36),   S( 21,  38),   S(  3,   0),
            S( 16,  24),   S(-12, -14),   S( -1, -18),   S( -3, -18),   S(  3,  29),   S(-27, -19),   S( 35,   7),   S( -3,  -6),
            S( 20,  24),   S( 47,  12),   S( 26,  17),   S( -3,  39),   S(-12,  26),   S( 11,  17),   S( 20,   8),   S(  6,  22),
            S( 17, -26),   S( 46,   2),   S( 41,  27),   S( 15,   8),   S( 62,  36),   S( 26,  21),   S( 25,  33),   S(  6,  30),
            S( 29,  -2),   S( 24,   4),   S(  0,   0),   S(  0,   0),   S( 31,  30),   S( 22,  -7),   S( 38,  20),   S(-15,  11),
            S( 42,  29),   S( 54,  55),   S(  0,   0),   S(  0,   0),   S( 45,  43),   S( 70,  41),   S( 27,  -9),   S( 50,  23),
            S( 61,  27),   S( 87,  16),   S( 55,  60),   S( 24,  27),   S( 52,  32),   S( 94,  49),   S( 36,  47),   S( -3,  24),
            S( 66,  81),   S( 80,  49),   S( 89,  63),   S( 58,  51),   S( 35,  26),   S( 53,  33),   S( 37,  46),   S( 30,  19),

            /* queens: bucket 6 */
            S( 27,  12),   S(-30, -32),   S(  7,  -2),   S( 22, -29),   S(-23,  -7),   S(-57, -25),   S( -2, -27),   S( -8,   0),
            S(  3,   5),   S( 17,  17),   S( 41,   3),   S( 14,  39),   S( 27, -14),   S( 12,  11),   S(-15,  16),   S( 35,  29),
            S(  8,  43),   S( 19,  19),   S( -2,  27),   S( 19,  -3),   S(-18,  36),   S( 44,  -9),   S( 57,  39),   S( 56,  41),
            S(-20,  20),   S(-39,  -1),   S( 39,  26),   S(109,  47),   S( 37,  -5),   S( 45,  30),   S( 86,  11),   S(109,  28),
            S(-16,   4),   S( 11,  33),   S( 13,  50),   S( 62,  49),   S(  0,   0),   S(  0,   0),   S( 97,  56),   S( 99,  20),
            S( 52,  37),   S( 48,  67),   S( 60,  51),   S( 31,  26),   S(  0,   0),   S(  0,   0),   S( 93,  69),   S(117,  41),
            S( 36,  21),   S(  2,  26),   S( 64,  32),   S( 48,  29),   S( 62,  67),   S( 80,  50),   S( 98,  19),   S(129,  23),
            S(  5,  10),   S( 29,   5),   S( 73,  13),   S( 85,  61),   S(125,  33),   S( 93,  44),   S(104,  46),   S( 89,  26),

            /* queens: bucket 7 */
            S(-43,  -2),   S( -2, -32),   S(-52,   4),   S(-21,  -2),   S( -3, -13),   S(-31,  11),   S( -9,  -5),   S(-38, -45),
            S(-27, -20),   S(-86,  17),   S(-35,  74),   S(-20,  27),   S(-30,  19),   S(-25,  19),   S(-16,  14),   S(-50, -17),
            S(-35, -24),   S(-32,  23),   S(  2,  31),   S( 27,  30),   S( 54,  -5),   S(  1,  -1),   S( 17,  -5),   S( 47, -11),
            S(-47,   9),   S( 17, -10),   S( -4,  35),   S( 19,  55),   S( 92,  12),   S( 78,  14),   S( 72,   5),   S( 47,   9),
            S(-23,  11),   S( -3,  23),   S( 16,  49),   S( 22,  51),   S( 59,  42),   S( 71,  44),   S(  0,   0),   S(  0,   0),
            S( -8,   7),   S(-11,  52),   S( -3,  62),   S(-10,  48),   S( 80,  11),   S(114,  69),   S(  0,   0),   S(  0,   0),
            S(-31,  21),   S(-48,  16),   S(-10,  20),   S( 45,  19),   S( 88,  22),   S(106,  30),   S( 67,  27),   S( 91,  55),
            S(  5, -42),   S( 10, -13),   S( 23,   7),   S( 50,  -2),   S( 44,  37),   S( 29,  15),   S( 32,  13),   S( 60,  35),

            /* queens: bucket 8 */
            S( -3, -33),   S( 14,   0),   S( -9, -21),   S( -5,  -9),   S(-12, -22),   S(  8,  -7),   S(  9,   1),   S( -1,  -2),
            S(-14, -32),   S(  8,   8),   S( 18,  11),   S(  4,  10),   S( 20,  18),   S( -6,  -8),   S( -3,   7),   S(  4,   5),
            S(  0,   0),   S(  0,   0),   S(  6,  -5),   S(  5,  -6),   S(  2,  -5),   S( -5, -10),   S( -3,  -5),   S( -6,  -3),
            S(  0,   0),   S(  0,   0),   S( -1,   4),   S( 13,  19),   S( 11,  18),   S( 20,  16),   S( 12,  24),   S(-12, -23),
            S(  3,   5),   S(  9,  15),   S( 10,  12),   S( 25,   5),   S( 12,   7),   S(  9,  21),   S( -3, -10),   S(-17, -23),
            S(  7,   5),   S( 12,   1),   S( 38,  55),   S(-14, -31),   S( -1,   8),   S( 19,   5),   S(  5,  -4),   S( -4, -11),
            S(-20, -43),   S(  7,   2),   S( 30,  39),   S( 28,  40),   S( 24,  20),   S(  4,   5),   S(  9,   4),   S( -7, -15),
            S(  6,   2),   S( 13,   6),   S(  5, -10),   S(  2,  -2),   S(  1,  -7),   S(-20, -33),   S(-14, -25),   S(-49, -81),

            /* queens: bucket 9 */
            S( 17,   8),   S(  9,  -5),   S(  1, -10),   S( 16,  -7),   S(  2, -10),   S( -6, -36),   S(  5,   3),   S(  6,   0),
            S( 20,  18),   S(  1,  -7),   S( -5, -25),   S(  9,   8),   S(-16, -43),   S( -1, -19),   S(  4,  -3),   S( -1,  -5),
            S( -2, -21),   S( -6, -17),   S(  0,   0),   S(  0,   0),   S( 18,  29),   S( 16,  -2),   S(-20, -24),   S(  6,   5),
            S(  4,   0),   S( -1,  -4),   S(  0,   0),   S(  0,   0),   S( -3,  -7),   S( 18,  17),   S( 14,   8),   S( -7,  -2),
            S( 10, -16),   S( 23,  18),   S( 10,  11),   S( -8,   0),   S(-13, -33),   S( 20,  11),   S(  9, -19),   S(  9, -21),
            S( 10,  -2),   S( 11,  -5),   S( 12,  12),   S(  9,  -6),   S(  5, -12),   S(  8,   3),   S(  7,   6),   S(  5, -16),
            S(-13, -15),   S( 32,  22),   S( -5,  -7),   S( 15,  27),   S( 25,   9),   S( 34,  33),   S( -1,  11),   S( 10,  -7),
            S( -8, -43),   S( 10,  -2),   S(-22, -45),   S( -1, -18),   S( 22,  19),   S( -4, -11),   S(-15, -41),   S( 17,  13),

            /* queens: bucket 10 */
            S( 13,  17),   S( -1, -37),   S( -2,  -4),   S(-16, -38),   S( 11,  -5),   S(  6,  -5),   S(-13, -30),   S(  4,  -3),
            S( -1, -10),   S(-16, -38),   S( 19,  15),   S(-10, -26),   S(  2,  -8),   S( 25,  14),   S(  6, -13),   S(-16, -35),
            S(  6,  14),   S(  6,   9),   S(  1,  -9),   S(  0, -16),   S(  0,   0),   S(  0,   0),   S( 14,  24),   S(  7,   2),
            S( -6,  -6),   S(  4,  -4),   S( 18,  10),   S( 13,  17),   S(  0,   0),   S(  0,   0),   S( -3, -23),   S( -9, -39),
            S(  7,  13),   S( 22,  14),   S(  8,   6),   S( 58,  69),   S( 13,  21),   S( -1,  -2),   S( 18,   0),   S( 23,  -7),
            S( -3,  -4),   S( -6, -28),   S( 23,  27),   S(  8,   7),   S( 11,   9),   S( 15,  24),   S( 17,   8),   S(  5, -25),
            S(  9,   3),   S( 17,  26),   S( 22,  38),   S( 20,  21),   S( 11,  15),   S( 12,   6),   S( 10, -19),   S( -5, -15),
            S(-12, -21),   S( 12,  -1),   S( 21,  -2),   S(-12, -48),   S( 18,  15),   S( -7,  -8),   S(  5,  -6),   S( 11,  -5),

            /* queens: bucket 11 */
            S(-14, -17),   S( -7, -13),   S(  1, -14),   S(-15, -41),   S(  2,  -6),   S(-30, -38),   S(  1, -17),   S( -5, -14),
            S( -3,   0),   S(-12, -11),   S(-33, -42),   S( -2,  -8),   S( 53,  15),   S(  0, -11),   S( 16,   1),   S(  9,  -7),
            S(-17, -15),   S(  0, -11),   S(-20, -13),   S( -6, -22),   S( 16,  11),   S(-20, -45),   S(  0,   0),   S(  0,   0),
            S( -7, -18),   S( -9, -15),   S(-15, -16),   S( 24,  23),   S(  8,  11),   S(  3,  11),   S(  0,   0),   S(  0,   0),
            S( -3,  -6),   S( -3,  -9),   S( 16,  19),   S( 15,  42),   S( 37,  56),   S( 28,  34),   S(  7,   8),   S(-10, -25),
            S(-19, -28),   S( -7, -24),   S( -9, -13),   S( 13,  28),   S( 15,   2),   S( 15,  -3),   S( 26,  44),   S( 16, -34),
            S(-15, -23),   S( -1,   4),   S( 16,  24),   S( -7,   4),   S( 12,  28),   S( 31,  28),   S( -8, -12),   S( 23,  -1),
            S(-19, -64),   S(  3,  -9),   S(-31, -39),   S( -7,  -2),   S(  5, -10),   S(  5,  -8),   S(  9,   9),   S(-10, -19),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 12,  21),   S( -1, -10),   S( -9,  -5),   S( -6, -18),   S(  3,  -1),   S( -3,  -2),
            S(  0,   0),   S(  0,   0),   S(  8,  11),   S( -9, -25),   S(  9,  12),   S( -6, -22),   S(  0,  -3),   S(  5,   1),
            S( -4,  -5),   S(  2,   2),   S( -5, -17),   S(  5, -17),   S( 21,  42),   S( 13,  37),   S(  4,   4),   S(  3,   2),
            S( 14,   5),   S(  7,   3),   S( 10,  23),   S(  5,   7),   S(  0, -17),   S(-16, -34),   S(  1,   2),   S(-12, -24),
            S(-15, -33),   S(  1,  -3),   S(  3,  -5),   S( -1,  -9),   S(  3,   8),   S( -4, -26),   S(-12, -29),   S( -5,  -8),
            S(  4,   2),   S(-14, -31),   S(  4,   6),   S(-10, -24),   S(-17, -40),   S( -1,  -1),   S(-14, -11),   S( -3, -10),
            S(-19, -36),   S( -2,  -3),   S( -3,  -8),   S(  8,  15),   S( -4,  -7),   S(-25, -52),   S(  3,   5),   S(-13, -32),
            S(  4,   5),   S(  0,  -2),   S( -3,   2),   S(  3,   9),   S(-26, -49),   S(-11, -29),   S(-12, -19),   S(-10, -18),

            /* queens: bucket 13 */
            S(-10, -34),   S( -3, -13),   S(  0,   0),   S(  0,   0),   S(-13, -27),   S( -4,  -4),   S(  0, -17),   S( -4,  -8),
            S(  1, -10),   S( -1, -14),   S(  0,   0),   S(  0,   0),   S(-20, -33),   S(-21, -49),   S(-13, -23),   S(-10, -29),
            S(-13, -29),   S( -4,  -7),   S( -4,  -9),   S(  8,  15),   S(-16, -35),   S(  9,  22),   S(  0,   5),   S( -3,  -1),
            S( -6, -18),   S(-11, -40),   S( 15,  18),   S( -7, -29),   S( 19,  36),   S( 20,  37),   S( -3, -17),   S( -6,  -9),
            S(  9,   3),   S( -1, -20),   S(  5, -15),   S( 18,  24),   S( -6,   1),   S(  7,   3),   S(-10, -25),   S(  3,   2),
            S(  9,   7),   S(-13, -39),   S( -4, -15),   S(  9,  15),   S( 10,   3),   S(-10,  -8),   S(-28, -56),   S(-17, -48),
            S( -2, -11),   S( -7, -12),   S(  5,   7),   S( -5, -10),   S(  0,  -5),   S( -7, -24),   S(-22, -47),   S( -8, -16),
            S(-17, -35),   S(-14, -32),   S(-13, -34),   S(  3,   3),   S(  6,  15),   S(-10, -22),   S(  5,  11),   S(-22, -41),

            /* queens: bucket 14 */
            S( 10,   9),   S( -4, -14),   S( -5, -21),   S(  4,  -6),   S(  0,   0),   S(  0,   0),   S(  6,  -8),   S(-11, -33),
            S( -9, -27),   S(-10, -34),   S( -3, -14),   S(  0, -17),   S(  0,   0),   S(  0,   0),   S( -6, -11),   S( -8, -14),
            S( -8, -20),   S(  8, -17),   S(-14, -42),   S( -2, -17),   S(  2,   8),   S(  0,   2),   S(-16, -38),   S(-16, -43),
            S( -6, -14),   S(  0,   0),   S(  7,   2),   S(-25, -49),   S(  2,   4),   S( -8, -35),   S( -3, -19),   S( -4, -17),
            S( -5,  -7),   S( -4, -22),   S(-12, -28),   S( 13,  21),   S( 16,  32),   S(  1, -16),   S( -7, -19),   S(  2,  -3),
            S(  0,   0),   S(-20, -45),   S(-12, -18),   S(-13, -36),   S( -3,  -7),   S( -4,  -2),   S(  3,   1),   S(-11, -23),
            S( -6,  -7),   S(-11, -26),   S( -3,  -9),   S( -7, -10),   S( -6,  -8),   S( -3, -14),   S(-15, -31),   S( -3,  -3),
            S(-26, -62),   S( 13,   3),   S(-11, -17),   S( -3, -10),   S( -3,  -6),   S( -6, -15),   S(-12, -17),   S( -8, -26),

            /* queens: bucket 15 */
            S( -3, -13),   S( -8, -19),   S(  8,   4),   S(-21, -41),   S(  2,  -5),   S( -5, -14),   S(  0,   0),   S(  0,   0),
            S(  3,   7),   S(  1,  -6),   S(-13, -23),   S(-13, -35),   S(  8,  12),   S(  7,  15),   S(  0,   0),   S(  0,   0),
            S(  0,  -3),   S(  4,  -2),   S(-12, -19),   S(  4,  -3),   S(-12, -27),   S(  8,  11),   S( -3,  -8),   S(  1,   0),
            S( -6, -14),   S(-10, -20),   S(  0,  -4),   S(  3,  10),   S(  9,   3),   S(  3,   1),   S( -1, -10),   S( -2, -15),
            S( -5, -12),   S(  4,   5),   S( -3,  -6),   S( 12,  16),   S( 21,  52),   S(  4,  11),   S( -8, -16),   S(-15, -34),
            S( -3,  -7),   S( -5, -13),   S(-13, -26),   S(-10, -21),   S(-10, -20),   S(-20, -51),   S( -2,  -8),   S(-11, -21),
            S( -7, -16),   S( -6, -14),   S( -7,   2),   S( -2,  -3),   S( -7,  -7),   S(  3,   3),   S( -4,  -8),   S(-19, -39),
            S(-15, -33),   S(-20, -50),   S(  0, -12),   S( -7, -14),   S(-15, -26),   S( -9, -19),   S(  4,   9),   S(-15, -25),

            /* kings: bucket 0 */
            S(  4, -30),   S( 27,  -2),   S( 15,  -2),   S(-24,   5),   S(-18,   0),   S( 37, -38),   S( 12,  10),   S( 20, -54),
            S(-20,  23),   S(-11,   9),   S( 12,  -1),   S(-34,  18),   S(-51,  50),   S(-14,  15),   S( -8,  43),   S(-13,  32),
            S( 24, -17),   S( 59,  -7),   S(  2,   6),   S( -7,  -9),   S(-23,   4),   S(-19,   0),   S(-43,  21),   S( -6, -15),
            S(-31, -10),   S( 11, -18),   S(-16,  -8),   S(-51,   8),   S(-70,  32),   S(-42,  -1),   S(-49,  31),   S(-31,  25),
            S(-41, -98),   S( 18, -37),   S( 30, -32),   S( 35,  -9),   S(-26,  -7),   S(-57,   9),   S( -5,  21),   S(-14, -16),
            S( -1, -96),   S( 23, -14),   S( 44, -71),   S( -3, -28),   S( 13, -15),   S( 10, -29),   S( 35, -17),   S(-18,  -1),
            S(  0,   0),   S(  0,   0),   S( 11, -20),   S( 10, -35),   S(-16, -38),   S(  8, -25),   S( 10, -13),   S(-15, -12),
            S(  0,   0),   S(  0,   0),   S( -9, -45),   S(  9, -53),   S( 16,   9),   S( 15,  -5),   S(  8,  14),   S(  8,  13),

            /* kings: bucket 1 */
            S(  2, -32),   S( 34, -16),   S(  9, -15),   S( 11,  -3),   S(-18,   2),   S( 25,  -8),   S(  8,  16),   S( 27, -23),
            S(-13,   4),   S(  2,  22),   S( 19, -13),   S(-41,  31),   S(-31,  21),   S( -6,  14),   S(  1,  25),   S(-15,  23),
            S( -3, -17),   S( 12, -16),   S( -7, -12),   S( 19, -15),   S(-28,   2),   S( 19, -21),   S( 28,  -8),   S( 43, -14),
            S( 12,   2),   S( 56, -30),   S( 26,  -8),   S(-33,  22),   S(-14,  22),   S(-41,  12),   S( 10,   9),   S(-21,  22),
            S(-17, -29),   S( 40, -27),   S( 32, -37),   S( 13, -21),   S(  6,  -7),   S(  2, -18),   S(  6,  -1),   S(-11,   9),
            S(  5, -25),   S( 23, -41),   S( 22, -18),   S( 56,  -8),   S(  8, -19),   S(  8,   0),   S( 10,  17),   S( -1,   2),
            S( -3, -51),   S(  5,  19),   S(  0,   0),   S(  0,   0),   S( -9,  15),   S(  0,  29),   S(  9,  35),   S(-21, -53),
            S(-19, -125),  S( -9, -26),   S(  0,   0),   S(  0,   0),   S( -2, -27),   S( -6, -38),   S(  5,  48),   S(-10, -49),

            /* kings: bucket 2 */
            S( 28, -53),   S( 14,   0),   S( 16, -23),   S( 28, -12),   S(-14,  10),   S( 34, -24),   S(  6,  27),   S( 28, -16),
            S( 26, -28),   S(-14,  38),   S( -6,  -2),   S( -9,   9),   S(-22,  14),   S( -5,   4),   S( 17,   5),   S( -9,  11),
            S(-54,   8),   S(-17,   0),   S( -3, -17),   S(-18,  -9),   S( -3,  -4),   S(  0, -21),   S( 33, -17),   S( 20, -11),
            S(  8,  15),   S(-28,   8),   S( -5,   8),   S(-15,  16),   S( 19,  -1),   S(-13,  -4),   S( 36, -25),   S( 43, -18),
            S(  0,  -5),   S( -5,  -7),   S( -7, -15),   S(-13,  -5),   S( 33, -34),   S(  2, -40),   S( 65, -35),   S(-16, -27),
            S( 10,  -7),   S( 16, -10),   S( 28, -18),   S( 36, -29),   S( 59, -14),   S( 21,  -1),   S( 59, -38),   S( 22, -30),
            S(-13,  -9),   S( 10,  22),   S(-26, -14),   S( 44,  18),   S(  0,   0),   S(  0,   0),   S( 23,  11),   S(-23, -35),
            S(-16, -49),   S(-19, -27),   S( -1, -31),   S(  6,  -2),   S(  0,   0),   S(  0,   0),   S(  0, -11),   S( -9, -102),

            /* kings: bucket 3 */
            S(  3, -65),   S( 20, -15),   S( 27, -41),   S( -2, -21),   S(-14, -25),   S( 35, -34),   S(  4,  17),   S(  7, -29),
            S(-18,  19),   S(-32,  41),   S(-16,   6),   S(-42,  15),   S(-52,  23),   S( -1,  -5),   S( -4,  15),   S(-17,  15),
            S( 11, -28),   S( 10, -17),   S( 10, -26),   S( -5, -21),   S(  3,  -1),   S( 30, -38),   S( 41, -23),   S( 56, -31),
            S(-31,  19),   S(-120,  38),  S(-101,  21),  S(-64,  17),   S(-70,  19),   S(-69,  -7),   S(-49,   0),   S(-56, -14),
            S(-14,  21),   S( -3, -19),   S(-50,  -6),   S(-82,   5),   S(-22, -31),   S(  2, -43),   S( -1, -52),   S(  4, -60),
            S(-14, -13),   S(  1, -16),   S( 15, -24),   S(-44, -16),   S( 30, -29),   S( 65, -61),   S(104, -44),   S( 37, -111),
            S(-38,  -7),   S( 17, -15),   S(  4, -40),   S( -1, -11),   S( 40, -40),   S( 49, -42),   S(  0,   0),   S(  0,   0),
            S( -3, -27),   S( 18, -12),   S(-11,  -5),   S(  7,  -6),   S( -3, -84),   S( -4, -20),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-42, -23),   S(  6,  28),   S(-26,  36),   S(  7,  12),   S( 19, -20),   S( -4,   9),   S( 17,   6),   S( 35, -19),
            S(-51,  30),   S( 21,  16),   S(-30,   9),   S(-28,   6),   S( 55, -12),   S( 20,  -8),   S( 44,  -3),   S( 35,   4),
            S(  3,  11),   S(-16,  -1),   S(-16,   0),   S(-20,   9),   S(-31,  11),   S( 16, -25),   S(-38,   3),   S(-12,   2),
            S(  0,  -9),   S( 13,  -2),   S( 28,   7),   S( 12,   3),   S( 14,  -3),   S(-12,  16),   S(  5,  13),   S(  7,   4),
            S(  0,   0),   S(  0,   0),   S( -2, -26),   S( -4,  -1),   S(-14,  -6),   S(-36,   4),   S( -2,   3),   S(-10,  12),
            S(  0,   0),   S(  0,   0),   S(  3,   8),   S( 21,  23),   S(  6,  20),   S( -9, -19),   S(  8, -15),   S(  1,   1),
            S( -4, -30),   S(  0,  13),   S( -7, -44),   S( 10,   7),   S( 14,  19),   S(-17,   3),   S( 10,  11),   S(  1,  -4),
            S(-12, -12),   S(  7,  31),   S( -1,  -1),   S(  9,  27),   S(  9,  -2),   S( -1, -18),   S(  2,  -6),   S( -2,   8),

            /* kings: bucket 5 */
            S(  8, -20),   S(-20,  25),   S(-44,  15),   S(-40,  20),   S(-20,  15),   S( -9,   9),   S( 37,   8),   S( 48,  -3),
            S(  5,   8),   S( 76, -10),   S( 43, -13),   S( 29,  -4),   S( 28,  -7),   S( 41, -13),   S( 45,   2),   S( 29,   2),
            S( -1,  -7),   S(-31,   5),   S( -4, -11),   S(-25,  -3),   S(-21,   7),   S(-64,   4),   S(  3,   1),   S( 17,   1),
            S(-21,   1),   S( 49, -18),   S( 29, -15),   S(  0,  26),   S( 40,   5),   S( 14,  -3),   S( 12,   3),   S( 12,  -1),
            S(-33, -16),   S(-13, -27),   S(  0,   0),   S(  0,   0),   S( 16,  -5),   S(-11, -12),   S(-12,   1),   S(-11,   8),
            S(-37, -13),   S( -7,   8),   S(  0,   0),   S(  0,   0),   S( -3,  18),   S(-35,   6),   S(-23,  10),   S( -8,  -3),
            S(-24, -15),   S(  1,   5),   S( 10,  57),   S( -2, -15),   S(-17,  15),   S(-16,  15),   S(  0,  38),   S(  5,  16),
            S(-15, -25),   S(  8,  39),   S(  1,  11),   S( 10,  20),   S(  1,  31),   S( -8,  23),   S( -6,  -5),   S( -4,   5),

            /* kings: bucket 6 */
            S( 41, -27),   S( 27,  -7),   S( 17,  -9),   S(-13,  12),   S( -7,  14),   S(-26,  17),   S( 18,  23),   S(  8,  16),
            S( 65, -21),   S( 21,  17),   S( 24,  -6),   S( 32, -12),   S( 32,  -5),   S( -6,  10),   S( 42,  -2),   S( 22,   1),
            S( -7,  -9),   S(-10,   2),   S(-25,  -6),   S( -8,  -8),   S(  5,  -4),   S(-66,   4),   S( -2,   3),   S(-25,  15),
            S(  4,  20),   S( 27, -10),   S( 23, -12),   S( 45,  10),   S( 68,   0),   S( -2,  -3),   S( 54, -16),   S(  8,  -6),
            S(-32,   0),   S(-25,  -5),   S(-20, -12),   S(  6,  -1),   S(  0,   0),   S(  0,   0),   S(-13, -12),   S(-54,  -9),
            S(-14,   0),   S(  3,  17),   S(-47,  11),   S(-20,  -7),   S(  0,   0),   S(  0,   0),   S(-35,  24),   S(-50, -17),
            S(  2, -27),   S(  5,  16),   S(  5,   9),   S(-18,  -7),   S( -1,   9),   S(  2,  14),   S( -8,  12),   S( -9,  -5),
            S(  6,  43),   S( -9,  -7),   S( 11,  22),   S( -3,  15),   S( -1,  -9),   S(  2,  30),   S( -1,  13),   S( -7,   5),

            /* kings: bucket 7 */
            S( 51, -37),   S( -8,  -3),   S(-36,   0),   S(-22,   9),   S(-43,   8),   S(-45,  34),   S( -9,  36),   S(-20,  22),
            S( 45,  -7),   S( 43, -18),   S( -5, -13),   S(  1,  -3),   S(-16,   8),   S(-28,  20),   S(  2,   9),   S(  4,  14),
            S( 29, -14),   S(  3, -12),   S(-28, -11),   S(-35,   2),   S(-37,   7),   S(-55,  20),   S( -1,   7),   S(-61,  20),
            S( -6,  10),   S( 12,  -6),   S(  9, -11),   S( 11,  13),   S( -3,   5),   S( 63, -29),   S( 49, -15),   S( 17, -15),
            S(-10,  -2),   S(  4,  11),   S(-17, -15),   S(-11,   1),   S(-16, -13),   S( 13, -16),   S(  0,   0),   S(  0,   0),
            S(-16, -53),   S( 16,   8),   S( 37, -10),   S( 11,  -6),   S( 10, -15),   S( 12,   3),   S(  0,   0),   S(  0,   0),
            S( 10,  10),   S( 19,  -4),   S( 16,   3),   S(-17, -22),   S( 21, -20),   S( -5, -17),   S(  5,   8),   S( -2, -15),
            S( 15,  18),   S(-10, -47),   S( 24,  12),   S(  7, -12),   S( 12,  -4),   S(-11, -10),   S(  9,  33),   S(-13, -43),

            /* kings: bucket 8 */
            S( -1, 101),   S(-37,  82),   S(-25,  47),   S(-13,  -8),   S( -9,   5),   S(-19,   7),   S(  8,  -9),   S(  0,  -8),
            S( 18,  80),   S( 23, -21),   S(-16,  65),   S(  2,  15),   S( -6,  20),   S( 13,  -2),   S( -4,  12),   S( 25,  34),
            S(  0,   0),   S(  0,   0),   S( 36,  51),   S(  7,   4),   S( 32, -10),   S( 19, -15),   S( -8,  10),   S( -6,   7),
            S(  0,   0),   S(  0,   0),   S( 15,  13),   S( 33, -20),   S( 10,  10),   S(  6,   5),   S( 14,  -5),   S(  7,  22),
            S( -1, -22),   S(  2,   7),   S( 12,  -9),   S(  3, -18),   S(  3, -27),   S(  8,  -4),   S( -5,   0),   S(-13, -39),
            S(  3,  32),   S( -4, -17),   S(  0,   9),   S( -4, -41),   S(-16,  -8),   S( 13, -14),   S( -8,  -5),   S( 13,  26),
            S( -1,  -2),   S(  0, -12),   S( 11,   9),   S( -1,  -1),   S(  2,  -8),   S(  5, -23),   S(  1,  -6),   S(  9, -52),
            S( -3, -16),   S(-11, -31),   S( -1, -36),   S( -6, -11),   S(  7,  33),   S(-11, -40),   S( 11,  18),   S(  6, -18),

            /* kings: bucket 9 */
            S(-40,  59),   S(-56,  39),   S(-62,  56),   S(-72,  27),   S(-64,  33),   S(-38,  15),   S( 97,  -4),   S( 49,  12),
            S(-40,  12),   S( 30,  32),   S( -7,  -5),   S( 20,  12),   S( 41,  16),   S( 18,   2),   S(  9,  22),   S(  8,   8),
            S(  0,  19),   S( -5,  -6),   S(  0,   0),   S(  0,   0),   S( 20,  16),   S( 10,  -7),   S( 38,  -1),   S( -9,   4),
            S(-12, -19),   S( -1,   1),   S(  0,   0),   S(  0,   0),   S(  4,   8),   S( 46,  -6),   S(-32,   7),   S(-19,  22),
            S(  2,  12),   S( -4,  -9),   S(  6,  27),   S(  2, -23),   S( -4, -18),   S( -1,   1),   S( -3,  20),   S(  2,   2),
            S( -2,  36),   S( -8,  21),   S( -4,  10),   S( -5, -15),   S( -1,  -7),   S(-17,  34),   S( -3,   0),   S( -2,   4),
            S(  5,   9),   S( -5,  -3),   S(  6,  -5),   S( -1,   7),   S( 13,  34),   S( 20,  10),   S( -7, -20),   S(  7,  16),
            S(  9,  37),   S( -6,  -6),   S( 17,  17),   S( -7, -32),   S( -4, -36),   S(  5,   9),   S( -1, -23),   S(  7,  29),

            /* kings: bucket 10 */
            S( 11,  31),   S(-11,   0),   S(-10,  13),   S(-37,  23),   S(-70,  22),   S(-122,  55),  S(-33,  44),   S(-46,  72),
            S( 12,  -7),   S( 49,  17),   S( -5,  -3),   S( 27,  12),   S( 69,  13),   S( 14,  11),   S( 16,  29),   S( -8,  22),
            S( 14,  13),   S( 28,  -3),   S(  9,  -6),   S( -3,   9),   S(  0,   0),   S(  0,   0),   S(  5,  12),   S(-33,  14),
            S(  9,  -6),   S(  2,  -3),   S( 10,   1),   S( 19,  -8),   S(  0,   0),   S(  0,   0),   S( 11,  17),   S(  1,  -1),
            S( -4,  10),   S(  6,  25),   S( 12,  -5),   S( 18, -25),   S(  6, -29),   S(  3,   7),   S(-13,   1),   S(-10,  13),
            S(-18,  -5),   S(  9,  22),   S(-11,  14),   S(  5,   4),   S( -6,  17),   S( -3,   5),   S(  0,  26),   S(-11,  19),
            S( -2, -30),   S(  2,   7),   S( 11,  -4),   S( 31,  26),   S( 20,  20),   S(-10, -18),   S( 17,  -2),   S(  4,  14),
            S( 10,  33),   S( 11, -26),   S( -4, -47),   S( -4,  -6),   S(  9,   1),   S( -5,   1),   S(-10, -40),   S( -7, -10),

            /* kings: bucket 11 */
            S(-23,  32),   S( 16,  23),   S(  9,  -9),   S(-19,   7),   S(-17,   2),   S(-153,  74),  S(-56,  67),   S(-153, 164),
            S( 14, -43),   S( -5,   1),   S(-15, -29),   S( 21,   3),   S( 36,  -2),   S( 10,  51),   S( 60,  -4),   S( 14,  32),
            S(-18, -22),   S( 11,  -8),   S(-17, -14),   S( 19, -10),   S( 54,   1),   S( 31,  25),   S(  0,   0),   S(  0,   0),
            S(  7,  15),   S(  4,  15),   S( 16,   9),   S( 36,   2),   S( 48, -11),   S( 19,  -1),   S(  0,   0),   S(  0,   0),
            S( 13,  -5),   S(  2,  -7),   S( -1,  -7),   S(  4,  -9),   S( 18, -23),   S( -5,  -6),   S(  4,  -6),   S(  5,  -4),
            S( 12,  33),   S(-15,  -2),   S( 10, -19),   S( -7,   8),   S( 11, -20),   S(  8, -11),   S(-14,   9),   S(  6,   1),
            S( 19,   0),   S( 13, -14),   S(  2,  16),   S(  8,  -3),   S(  4,  -1),   S(  9,  14),   S( 11,  -8),   S( -3, -10),
            S( 10,  15),   S( -4, -15),   S( -6, -46),   S(  5, -20),   S( -3, -19),   S( -4, -24),   S( -2, -20),   S(  6,  24),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  1,  62),   S( -6,  29),   S( -3,  18),   S( 18,  17),   S( 17,  -5),   S(-13,  53),
            S(  0,   0),   S(  0,   0),   S( 21,  96),   S( 10,   0),   S( -3,  23),   S( 25,   2),   S( 14,  -1),   S(-13,  13),
            S(  0,   8),   S(  3, -27),   S( 23,  27),   S( 12,  23),   S( -2, -28),   S( -4,  29),   S( -9, -19),   S(-15,  -9),
            S( -2, -14),   S(  8,  30),   S( -4,  -9),   S(  0, -46),   S( -7, -15),   S(  9, -13),   S( -9,  34),   S( -1,  19),
            S(  8,  26),   S(  3,   9),   S(  3,  -1),   S(  1, -13),   S(  3,  26),   S(-12,   3),   S(-20,  -6),   S( -6, -23),
            S(  6,  19),   S( -2,   7),   S( -4,  -9),   S( -4, -14),   S( -8, -19),   S( -4,   6),   S(-16,  13),   S(  5,  29),
            S(  5,  18),   S( -7, -31),   S( -2,   2),   S( -7, -31),   S( -6, -40),   S(  2,   7),   S(  2,  14),   S( -9, -43),
            S(  1,   9),   S(  2,   5),   S( -3, -17),   S(  3,   1),   S( -4, -21),   S( 12,  22),   S( -6, -27),   S(  0,  -2),

            /* kings: bucket 13 */
            S(-10,  68),   S(-11,  57),   S(  0,   0),   S(  0,   0),   S(  3,  62),   S(-37,  -1),   S( -1,  -2),   S( 13,  45),
            S(  2,  26),   S( -2,   1),   S(  0,   0),   S(  0,   0),   S( 28,  -4),   S(-17, -26),   S(-45,  15),   S( 35,   8),
            S( -9, -10),   S(  8,  36),   S( -5, -31),   S( -9,  15),   S(  0,   8),   S(-10,  -2),   S(-24,   3),   S(  9,  18),
            S( -9, -21),   S( -1,   6),   S(  2, -16),   S( -9, -41),   S(  4, -39),   S( 12,  -6),   S( -7,   7),   S( -7,  -8),
            S(  0,  -3),   S( -1,  22),   S(  2,   5),   S(-10, -11),   S( -7,  -7),   S( -3,  24),   S(-11,   9),   S(  8,  41),
            S(  9,  27),   S(-12,  -7),   S( -7,   5),   S( -1,  14),   S(-11,  15),   S(-14,   0),   S( -3,  18),   S( -2, -16),
            S( 13,  43),   S( -6, -12),   S(-16, -44),   S(  6,   3),   S( -3, -24),   S(  4, -22),   S(  1, -50),   S(  4,  29),
            S(  2,  -2),   S(  1,  16),   S(  6,  42),   S(  4,  -1),   S( -7, -13),   S( -7, -11),   S( -7, -12),   S(  6,  20),

            /* kings: bucket 14 */
            S( -1,  63),   S(-16, -12),   S( -7, -12),   S( -6,  12),   S(  0,   0),   S(  0,   0),   S( -3,  94),   S(-56,  67),
            S(-14,  15),   S(  8,  -6),   S( 11,  13),   S( 18,   5),   S(  0,   0),   S(  0,   0),   S( 10,  21),   S(-34,   5),
            S( -8,   4),   S( -6,  -6),   S( 27,   0),   S(  1, -19),   S(  4, -37),   S( -6,  13),   S(  9,  63),   S(-22, -17),
            S(  5,   8),   S(  4,  -9),   S( -2, -21),   S( 14, -45),   S( -4, -52),   S(  4,  34),   S(-12, -13),   S(  9,  17),
            S(  7,  25),   S( -4,   2),   S(-12,   4),   S( -8, -16),   S( -4,  19),   S( -1,  10),   S(  4,  40),   S(  6,  28),
            S( -1,  -6),   S( -4,  17),   S( -5, -20),   S( -9,  16),   S( -6, -21),   S( -3, -12),   S(-23, -59),   S(  8,  21),
            S(  5,   8),   S(-14, -39),   S(  4,  42),   S(  4,  -7),   S( -6, -13),   S( -2, -26),   S(-16, -66),   S( 14,  68),
            S(  3,  19),   S(  2,  45),   S(  8,  53),   S( -3, -20),   S( 10,  44),   S( -2,  -5),   S(-11, -34),   S( -3, -27),

            /* kings: bucket 15 */
            S(  4,  33),   S(  2, -11),   S(  1,  -2),   S( -2,  -4),   S(-20,  16),   S(-32,  56),   S(  0,   0),   S(  0,   0),
            S( -3,  -8),   S(-15, -14),   S( 14,  -9),   S(  4,  32),   S( 31, -25),   S( 29,  93),   S(  0,   0),   S(  0,   0),
            S(-13, -15),   S( 15,  14),   S(  8, -15),   S(  5,   4),   S(  7, -31),   S( 18,  35),   S(  7,   9),   S(-12, -21),
            S( -4, -27),   S( -8,  18),   S(  3,   2),   S(-11, -48),   S( -8, -38),   S(  7,  10),   S(  7,  55),   S( -2, -28),
            S(  7,  22),   S(-10,  32),   S( -6,  11),   S(-12, -25),   S( 10,  18),   S( -8,  24),   S( -7, -10),   S( -3,   4),
            S( -3,  27),   S(-21, -24),   S(  1,  11),   S( 11,   2),   S( -9, -16),   S(  3,  12),   S( -6,   0),   S(  4,  20),
            S(  9,  36),   S( -4,  11),   S(-11, -20),   S(  1,  11),   S(  6,  16),   S(  5,  10),   S( -8, -17),   S(  5,  17),
            S( -9, -15),   S( 16,  27),   S( -1,   6),   S(  6,  17),   S(  2,  25),   S(  3,  11),   S( -1,  -7),   S(  4,   7),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S( -8, -182),  S( 14, -35),   S( 25,  35),   S( 33,  74),   S( 40,  95),   S( 44, 115),   S( 51, 112),   S( 63, 100),
            S( 86,  69),

            /* bishop mobility */
            S(  8, -71),   S( 12,   2),   S( 22,  41),   S( 28,  75),   S( 34,  98),   S( 37, 115),   S( 41, 119),   S( 44, 117),
            S( 47, 118),   S( 53, 107),   S( 61,  98),   S( 84,  82),   S(111,  72),   S(131,  50),

            /* rook mobility */
            S(-16,  -4),   S( -7,  56),   S( -7,  86),   S( -3, 100),   S( -5, 123),   S(  0, 134),   S(  4, 142),   S(  7, 144),
            S(  7, 153),   S( 13, 156),   S( 18, 158),   S( 28, 155),   S( 37, 151),   S( 47, 145),   S(130, 101),

            /* queen mobility */
            S( 62, -64),   S( 68, -91),   S( 58,  17),   S( 53, 100),   S( 54, 150),   S( 54, 186),   S( 54, 224),   S( 54, 244),
            S( 54, 270),   S( 54, 285),   S( 52, 305),   S( 50, 322),   S( 52, 329),   S( 50, 341),   S( 45, 357),   S( 50, 359),
            S( 52, 352),   S( 68, 340),   S( 69, 336),   S( 97, 312),   S(126, 288),   S(144, 267),   S(167, 250),   S(205, 219),
            S(202, 218),   S(179, 222),   S(122, 201),   S( 81, 172),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  29),   S(-20,  59),   S(-36,  58),   S(-32,  72),   S(  7,  32),   S(-19,  42),   S( -9,  76),   S( 35,  30),
            S( 17,  39),   S(  6,  59),   S(-23,  61),   S(-10,  48),   S(  2,  48),   S(-41,  65),   S(-32,  83),   S( 32,  39),
            S( 30,  75),   S( 24,  78),   S(  6,  62),   S( 34,  44),   S(  9,  52),   S(-32,  75),   S(-22, 103),   S(  2,  73),
            S( 51,  92),   S( 65, 104),   S( 41,  72),   S( 17,  62),   S( 10,  61),   S( 10,  87),   S(-16, 110),   S(-51, 126),
            S( 76, 137),   S( 97, 157),   S( 91, 108),   S( 53, 102),   S(-38,  91),   S( 48,  91),   S( -7, 150),   S(-47, 145),
            S(156, 181),   S(141, 212),   S(184, 194),   S(187, 193),   S(193, 208),   S(183, 203),   S(196, 200),   S(198, 209),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 12,  12),   S(  4,   3),   S(  9,   3),   S( 12,  24),   S( 25,  39),   S(  2, -25),   S(-17,  38),   S( 11, -33),
            S(-10,  44),   S( 29,   0),   S(  0,  51),   S( 19,  36),   S( 40,   2),   S( -6,  35),   S( 18,   3),   S(  8,   6),
            S( -9,  25),   S( 18,  15),   S( -3,  51),   S( 14,  73),   S( 30,  28),   S( 22,  30),   S( 33,   5),   S( -7,  25),
            S( 26,  23),   S( 13,  50),   S( 41,  70),   S( 16,  94),   S( 76,  59),   S( 76,  50),   S( 22,  54),   S( 25,  22),
            S( 58,  81),   S(119,  89),   S(101, 112),   S(117, 166),   S(154, 124),   S(154, 116),   S(156, 101),   S( 79,  45),
            S( 81, 193),   S(123, 270),   S(109, 233),   S(108, 231),   S( 62, 171),   S( 32, 129),   S( 22, 136),   S(  5,  83),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  46),   S( 21,  38),   S( 44,  49),   S( 41,  35),   S( 31,  32),   S( 30,  37),   S(  8,  23),   S( 48,  12),
            S( -4,  29),   S( 15,  50),   S( 13,  39),   S( 10,  50),   S( 25,  22),   S(  8,  25),   S( 26,  24),   S( -2,  15),
            S(  3,   9),   S( 24,  29),   S( 54,  38),   S( 45,  32),   S( 44,  43),   S( 68,  13),   S( 20,  23),   S( 14,   6),
            S( 76,  27),   S(121,   5),   S(125,  74),   S(158,  87),   S(162,  80),   S(101,  84),   S( 92,  16),   S( 85, -17),
            S( 66,  24),   S(135,  33),   S(147, 138),   S(163, 152),   S(189, 164),   S( 92, 189),   S(205, 119),   S(-66, 112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 16,  24),   S(  6,  15),   S(  8,  30),   S(-10,  57),   S( 69,  34),   S( 21,   9),   S( -8,   3),   S( 27,  11),
            S(  2,  15),   S(  6,  10),   S( 18,  16),   S( 16,  28),   S( 16,  16),   S( -4,  11),   S(  3,   8),   S( 28,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -15),   S( -6, -10),   S(-18, -16),   S(-16, -28),   S(-16, -16),   S(  4, -11),   S( -3,  -8),   S(-28,   5),
            S(-16, -24),   S( -6, -15),   S( -8, -30),   S( 10, -57),   S(-69, -34),   S(-21,  -9),   S(  8,  -3),   S(-27, -11),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -22),   S(-11, -13),   S( -3, -32),   S(-54,  -5),   S(-18, -17),   S(-16, -25),   S(  6, -28),   S(-14, -37),
            S(-21,   6),   S(-14,   0),   S(-23,   6),   S(  3, -13),   S(-28, -16),   S(-30,  -1),   S(-41,   5),   S(-11, -15),
            S(-16, -15),   S( -3, -18),   S(-22,  -6),   S(-28,  -9),   S(-28, -26),   S(-22, -12),   S(-21,  -3),   S(-42, -17),
            S( -2, -31),   S( 21, -43),   S( 14, -26),   S(  8, -29),   S(  3, -16),   S( 61, -34),   S( 37, -31),   S(-14, -41),
            S( 25, -60),   S( 50, -74),   S( 54, -43),   S( 86, -44),   S( 61, -31),   S( 90, -23),   S(129, -72),   S( 34, -70),
            S(149, -117),  S(161, -116),  S(137, -73),   S(105, -52),   S( 87, -34),   S(121, -27),   S(172, -70),   S( 60, -93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   3),        // attacks to squares 1 from king
            S( 15,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 66, -85),        // king-side castling available
            S( 17,  60),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S(  0, -45),   S(  6, -40),   S( -4, -47),   S(-14, -34),   S(-24, -24),   S(-41, -14),   S(-72,   5),   S(-83,   9),
            S(-93,  11),   S(-86,  18),   S(-84,  26),   S(-13,  14),   S(  1,  13),

            /* orthogonal lines */
            S( -7, -54),   S(-60, -22),   S(-81, -13),   S(-96, -11),   S(-102, -18),  S(-107, -22),  S(-108, -28),  S(-104, -31),
            S(-122, -24),  S(-143, -22),  S(-165, -29),  S(-171, -34),  S(-192, -22),  S(-376,  38),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( -3, 208),

            /* passed pawn can advance */
            S(-15,  35),   S(-13,  65),   S( 10, 102),   S( -1, 203),

            /* blocked passed pawn */
            S(  0,   0),   S( 71, -21),   S( 36,  -7),   S( 40,  22),   S( 43,  41),   S( 50,  -3),   S(152, -27),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 31, -20),   S( 20,  45),   S( 10,  44),   S(  8,  75),   S( 43,  88),   S( 99, 120),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-26, -12),   S(-20, -20),   S( -4, -47),   S(-23, -44),   S(  5, -61),   S(188, -98),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 43, -36),   S( 16, -19),   S(  7, -14),   S( 15, -53),   S(  6, -139),  S(-42, -245),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S(  3,  59),   S( 17,  38),   S(123,  39),   S( 14,  33),   S(206,  79),   S(172,  46),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-27,  91),

            /* bad bishop pawn */
            S( -5, -22),

            /* rook on open file */
            S( 46,  -7),

            /* rook on half-open file */
            S( 14,  20),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 26,  33),   S( 29, -11),   S( 34,  26),   S( 27,  -2),   S( 46, -24),

            /* pawn threats */
            S(  0,   0),   S( 61,  97),   S( 57, 114),   S( 60,  86),   S( 44,  63),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 11,  11),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
