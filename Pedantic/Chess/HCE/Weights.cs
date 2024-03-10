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
        public const int MAX_WEIGHTS = 12767;
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
        public const int TEMPO = 12766;             // tempo bonus for side moving

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

        // Solution sample size: 6000021, generated on Sat, 09 Mar 2024 21:27:23 GMT
        // Solution K: 0.003850, error: 0.085486, accuracy: 0.4982
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S(101, 180),   S(431, 538),   S(442, 569),   S(557, 966),   S(1355, 1646), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 97, -114),  S(145, -89),   S( 29, -32),   S(-30,  47),   S(-29,   5),   S(-29,  -7),   S(-53,  -3),   S(-41, -17),
            S(106, -112),  S( 91, -94),   S( -6, -52),   S(-21, -49),   S(-23, -27),   S(-26, -35),   S(-43, -23),   S(-38, -37),
            S( 91, -83),   S( 59, -48),   S( 12, -50),   S(  7, -58),   S(-11, -55),   S( -9, -45),   S(-24, -42),   S(-30, -32),
            S( 53, -25),   S( 43, -42),   S( 22, -40),   S( 19, -72),   S(-12, -44),   S(-36, -34),   S(-32, -40),   S(-34, -20),
            S( 56,  46),   S( 43,   4),   S( 46, -30),   S( 53, -73),   S( 16, -26),   S(-23, -35),   S(-27, -12),   S(-44,  50),
            S( 80,  36),   S( 59,  74),   S( 23, -20),   S( 30, -44),   S( -9, -27),   S(-27,  -5),   S( 31, -27),   S(  0,  30),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 24, -27),   S( 28, -43),   S( 51, -29),   S( -4,  12),   S(-13, -18),   S(  2, -16),   S(-43, -11),   S(-38,  15),
            S( 19, -36),   S(  9, -49),   S( 11, -50),   S( -9, -40),   S(-17, -21),   S(-11, -33),   S(-40, -19),   S(-44, -12),
            S(  7, -25),   S(  6, -21),   S( 24, -50),   S( 16, -55),   S(-20, -27),   S(  6, -40),   S(-19, -28),   S(-22, -11),
            S( 11,   9),   S(  6, -34),   S( 22, -33),   S( 18, -48),   S( -1, -24),   S( 11, -37),   S(-33, -17),   S(-31,  23),
            S(  5,  57),   S(-33,  -2),   S( -7,  -8),   S(  1, -24),   S( 21, -19),   S(-33,  -3),   S(-37,  16),   S(-27,  57),
            S( 44,  53),   S( 40,   7),   S(-34, -22),   S(-42,  32),   S(-16,  -5),   S(-70,  27),   S(-58,  29),   S(-30,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22,  -1),   S(-37,   5),   S(-15,   0),   S(-21,  11),   S(  8, -11),   S( 28, -23),   S(  2, -41),   S(-13, -17),
            S(-24, -17),   S(-52,  -9),   S(-32, -34),   S(-25, -32),   S( -1, -25),   S(  3, -27),   S(-14, -35),   S(-25, -24),
            S(-36,  -5),   S(-39, -13),   S(-15, -43),   S(-11, -49),   S( -7, -21),   S( 11, -24),   S(-12, -23),   S(-14, -13),
            S(-44,  20),   S(-30, -28),   S(-25, -29),   S( -9, -41),   S( 10, -34),   S(-13, -13),   S(-15, -10),   S(-23,  15),
            S(-36,  59),   S(-72,  23),   S(-62,  -6),   S(-58, -19),   S(  1,   9),   S(-33,  14),   S(-35,  27),   S(-32,  78),
            S(-44,  74),   S(-108,  67),  S(-116,   3),  S(-46, -54),   S(-22,  17),   S( -2,  -3),   S(-32,  10),   S(-33,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -11),   S(-42,   1),   S(-31,  -5),   S(-17, -27),   S(-10, -11),   S( 34, -17),   S( 79, -62),   S( 55, -75),
            S(-33, -31),   S(-55, -19),   S(-37, -40),   S(-30, -28),   S(-21, -21),   S(  5, -36),   S( 54, -63),   S( 51, -72),
            S(-36, -25),   S(-27, -39),   S(-12, -59),   S(-12, -59),   S(-13, -40),   S(  9, -37),   S( 26, -45),   S( 49, -50),
            S(-41,  -3),   S(-26, -48),   S(-11, -54),   S(  4, -71),   S( 22, -64),   S( -2, -38),   S( 17, -30),   S( 43,  -9),
            S(-12,  30),   S(-34, -17),   S( -6, -42),   S( -1, -55),   S( 72, -39),   S( 50, -36),   S( 48,   5),   S( 42,  68),
            S(-33,  97),   S(-54,  40),   S(-29, -46),   S(-27, -51),   S( 23, -41),   S( 82, -56),   S( 46,   8),   S( 54,  64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-80,  15),   S( 18, -25),   S( -1,  -2),   S(-14,  17),   S(-41, -17),   S(-45,   9),   S(-78,  12),   S(-63,  -2),
            S(-27,   5),   S( 29, -10),   S( 30, -38),   S( 21, -24),   S( -1, -34),   S(-61, -19),   S(-31, -34),   S(  8, -30),
            S( 42, -14),   S( 45,  -3),   S(  4,  -6),   S( 11, -36),   S(-24, -36),   S( -8, -41),   S(-49, -26),   S(-30, -18),
            S( 29,  27),   S(  7,  34),   S( 51,   6),   S( -4,  12),   S( 18, -30),   S(-62,  -6),   S(  2, -44),   S( 50, -25),
            S(-10,  80),   S( 13,  63),   S(  9,  25),   S( -1,  -5),   S(  4,  14),   S( -3,  -8),   S(-39, -35),   S( 58,  25),
            S( 50, 103),   S( 69,  60),   S( 21,   2),   S( 38,   0),   S( -3, -57),   S( -1, -32),   S(  4,  -9),   S(-33,  -2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  45),   S(-56,  27),   S(-10,  20),   S( 22, -15),   S(-12,   9),   S(-20,  13),   S(-58,  10),   S(-38,  23),
            S(-59,  19),   S(-58,  14),   S( 35, -15),   S( 15,   9),   S( 13,  -9),   S(-16, -19),   S(-40,  -9),   S(-62,  18),
            S(-42,  38),   S(-35,  24),   S( 86, -34),   S( 19, -25),   S( 59, -20),   S(-41,   2),   S(-24,   0),   S(-43,  15),
            S(-40,  60),   S(-16,  31),   S( 43,   4),   S( 48,  12),   S( 13,   2),   S(-49,  11),   S(-10,  -4),   S(  7,  24),
            S( 60,  60),   S( 76,  25),   S( 45,  48),   S( 51,  17),   S(-10,  43),   S( 75,  -5),   S( -1,  19),   S( -1,  53),
            S(113,  42),   S( 47,  26),   S( 62, -13),   S( 44,  -6),   S( 37,  -9),   S( 29,  -1),   S( 10,   0),   S( 28,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  27),   S(-17,   1),   S(-17,  10),   S( -1,   5),   S(  1,  -9),   S(-28,   7),   S(-49,   0),   S(-72,  24),
            S(-36,   0),   S(  2, -27),   S(  0, -32),   S( 23, -16),   S( 57, -20),   S( 42, -24),   S(-17, -15),   S(-72,  13),
            S(-36,  10),   S(  0,  -5),   S( -4, -17),   S( -5, -14),   S( 38, -10),   S( 73, -32),   S(  6, -15),   S(-39,  22),
            S(-18,  27),   S(-38,  15),   S( 28, -14),   S( 12,  -8),   S( 43,   4),   S( 47,   2),   S( 34,  -6),   S( 39,  13),
            S(-43,  52),   S(-39,  21),   S(  6,   1),   S( 10, -12),   S( 55,  46),   S( 61,  25),   S( 73,  -2),   S( 65,  28),
            S( 61,  35),   S( 34,  21),   S( 20, -34),   S( 22, -46),   S( 56,  12),   S( 37,  -2),   S( 44,  -2),   S( 63,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55, -18),   S(-53, -12),   S( -8, -16),   S(-37, -21),   S(-31, -25),   S( 26, -26),   S( -9, -35),   S(-64, -12),
            S(-60, -29),   S(-60, -34),   S(-39, -43),   S(-28, -38),   S( -8, -31),   S( 39, -43),   S( 41, -40),   S(-21, -25),
            S(-77, -18),   S(-43, -38),   S(-24, -48),   S(-13, -48),   S( -4, -22),   S( 28, -22),   S( 36, -38),   S( 23, -26),
            S(-39, -21),   S(-55, -31),   S(-71, -31),   S(-44, -15),   S( -1, -19),   S( 24,  -2),   S( 29,   0),   S( 65, -12),
            S(-40,  -2),   S(-46, -26),   S(-48, -21),   S( -1, -54),   S( 58,  -4),   S( 38,   3),   S( 82,  46),   S( 92,  38),
            S(-30,   4),   S(  9, -28),   S( 52, -70),   S( 37, -59),   S(  8, -29),   S( 36, -20),   S( 56,  53),   S( 84,  61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  42),   S(-54,  50),   S(-18,  31),   S(-18, -39),   S(  1,  19),   S(-56,  15),   S(-62,  -5),   S(-56,  22),
            S(-40,  39),   S(-10,  17),   S(-68,  23),   S(-28,  12),   S(-41, -20),   S(-22, -17),   S(-72, -21),   S(-27,  -3),
            S(-27,  64),   S( 35,  69),   S( -5,  57),   S(-31,  19),   S(  4,   0),   S(-33, -32),   S(-37, -37),   S( -8, -27),
            S( 22, 110),   S( 46, 127),   S( 70,  68),   S( 14,  37),   S( -1,   2),   S(-16, -16),   S( -3, -21),   S( 10, -18),
            S( 39, 120),   S( 74, 112),   S( 55, 124),   S( 52, 111),   S( -6,   1),   S( 15,  -9),   S(  8, -12),   S( -4,   0),
            S(101, 139),   S( 79, 145),   S(105, 183),   S( 59,  99),   S(  2,   7),   S(  8,  -5),   S(-12, -37),   S(  9, -10),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-90,  61),   S(-95,  36),   S(-10,  28),   S( 20,  37),   S(  0, -11),   S(-62,  25),   S(-88,  17),   S(-62,  26),
            S(-71,  30),   S(-39,   2),   S(-36,  11),   S(  8,  20),   S(-67,   3),   S(-77,   7),   S(-137,   7),  S(-48,  11),
            S(-58,  53),   S(-41,  55),   S(-49,  73),   S(-41,  67),   S(-30,  52),   S(-78,  11),   S(-46, -19),   S( -6,  -3),
            S( 12,  75),   S( 51,  85),   S( 49, 100),   S( 67, 121),   S(-41,  62),   S(  7,  11),   S( 42,  -4),   S( 40, -11),
            S(114,  60),   S( 63, 121),   S( 93, 122),   S( 89, 134),   S( 72, 100),   S( 29,  26),   S( 17, -15),   S( 34, -17),
            S( 32,  54),   S( 93,  72),   S(117,  89),   S(110, 117),   S( 57,  82),   S(  4, -11),   S(  8,  -7),   S( -5, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-102,  20),  S(-109,  19),  S(  4,   5),   S( -2,  21),   S(-16,   5),   S(-122,  47),  S(-122,  31),  S(-66,  36),
            S(-108,  -7),  S(-57, -12),   S(-21, -21),   S(-38,  -4),   S(-51,  34),   S(-49,  21),   S(-156,  33),  S(-93,  20),
            S(-26,  -7),   S(-37,  -1),   S(-12,  -2),   S(-46,  57),   S(-80,  87),   S(  7,  40),   S(-71,  29),   S(-89,  48),
            S(  7,  -4),   S( 10,  12),   S(  3,  23),   S(-12,  71),   S( 41,  89),   S(-23,  83),   S( -7,  43),   S( 30,  31),
            S( 70,  -2),   S( 19,   0),   S( 33,  47),   S( 60, 105),   S( 94, 112),   S( 80,  65),   S( 55,  72),   S( 48,  42),
            S( 51,   4),   S( 21, -24),   S( 24,  30),   S( 75, 103),   S( 44,  95),   S( 51,  58),   S( 44,  50),   S( 52,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-144,  18),  S(-100,   8),  S( -5,  -8),   S( -2,  10),   S( -9,  16),   S(-65,  38),   S(-85,  36),   S(-42,  33),
            S(-80, -27),   S(-85, -33),   S(-74, -34),   S(-47,   3),   S(-48,   4),   S(-29,  22),   S(-87,  39),   S(-117,  38),
            S(  7, -43),   S(-22, -45),   S(-50, -18),   S(-14,  -3),   S(-78,  44),   S( 20,  40),   S(-36,  61),   S(-37,  61),
            S( 20, -17),   S(-51, -21),   S(-35, -12),   S(-24,  15),   S(-29,  58),   S( 58,  46),   S( 33, 100),   S(105,  61),
            S( -3,   7),   S(-26, -11),   S(  7, -15),   S(-27,  14),   S( 64,  77),   S( 55,  85),   S(115,  95),   S(147,  73),
            S( 15,  -8),   S( -5, -41),   S( 22,  14),   S( -3, -35),   S( 39,  28),   S( 25,  87),   S(104, 157),   S(109, 113),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  27),   S(-21, -11),   S(-40, -31),   S( -4, -10),   S( -6,  -3),   S(-10,  -7),   S(-13,  -2),   S(-31,   3),
            S(-42, -22),   S( 15,  -2),   S(  4,  56),   S(  4,   5),   S(-12,  48),   S( -3, -13),   S(-33, -30),   S(-19, -44),
            S(-12,  39),   S( 42,  71),   S( 22,  45),   S( 28,  23),   S(  9, -11),   S(-20, -19),   S(-15, -35),   S(-32, -69),
            S( -2,  78),   S( 13,  72),   S( 47,  86),   S( 52,  75),   S( -3, -22),   S(-29, -57),   S(  6, -39),   S(-31, -73),
            S( 74,  70),   S( 76, 209),   S( 79, 124),   S(  1,  42),   S(  5,  -5),   S(  1,  -7),   S( -4, -42),   S( -2, -25),
            S( 53, 128),   S( 49, 249),   S(114, 244),   S( 41,  81),   S( -8, -17),   S(-12, -63),   S( -6, -45),   S(-16, -87),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55, -38),   S(-24,  19),   S(-20, -11),   S(  0,  10),   S( -5,  11),   S(-20, -24),   S(-54, -44),   S(-31,  -6),
            S( -3, -45),   S(-15,   7),   S(-21, -19),   S( 13,  34),   S(-41,  16),   S(  8,  -8),   S(-23, -29),   S(-17, -15),
            S( 12,  17),   S( 12, -24),   S(  4,  30),   S( 13,  76),   S(-11,  28),   S(-23, -32),   S( -6, -46),   S( -7, -60),
            S(  8,  26),   S( 50,  64),   S( 28,  88),   S( 15, 100),   S( 30,  42),   S( 26, -10),   S( 27, -20),   S( 31, -63),
            S( 37,  45),   S( 72, 118),   S( 96, 167),   S( 97, 182),   S( 54,  96),   S( 17,   6),   S( 17, -65),   S( 17, -64),
            S( 28,  60),   S( 85, 141),   S( 78, 202),   S(105, 239),   S( 51, 106),   S( 19,  30),   S( -6, -43),   S( 11, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -49),   S(-27, -28),   S( -4, -17),   S( -2,  -4),   S(  0,  -6),   S(-27,  15),   S(-28, -18),   S(-14,  33),
            S(-24,   0),   S(-31, -35),   S(-36, -23),   S( 10,  31),   S(-21,  28),   S(  3,  21),   S(-19,   9),   S( -9,  12),
            S(-22, -19),   S(-18, -24),   S( -6, -22),   S(  9,  37),   S(-30,  58),   S( 12,  16),   S( -1,  -2),   S( 20,  28),
            S( 32, -43),   S( 26, -19),   S( 20, -26),   S( 11,  75),   S( 15, 115),   S( 15,  57),   S( -6,  28),   S( 22,  34),
            S( 17, -16),   S( 20,   2),   S( 42,  33),   S( 44, 112),   S( 75, 189),   S( 86, 152),   S( 46,  61),   S( 32,  63),
            S( 11, -14),   S( 24, -24),   S( 45,  69),   S( 44, 152),   S( 78, 222),   S( 57, 176),   S( 42, 114),   S( 27,  82),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -53),   S(-25, -46),   S(-18, -66),   S( -2,   0),   S( 11,  32),   S(  8,  60),   S(-22, -21),   S( 21,  51),
            S(-13, -54),   S(-13, -10),   S(-15, -41),   S(  7,  10),   S(-20,  19),   S( 17,  28),   S(  0,  37),   S(-11,  15),
            S( -2, -83),   S( -8, -86),   S(-19, -63),   S(  0,  31),   S( 10,  27),   S( 26,  17),   S( 29,  78),   S( 27,  66),
            S( 13, -46),   S(-31, -46),   S(-12, -40),   S( 16,  15),   S( -1,  39),   S( 44,  40),   S( 34, 121),   S(  3,  74),
            S(-14, -70),   S(-16, -92),   S( -3, -26),   S( 17,  20),   S( 34,  85),   S( 84, 111),   S( 66, 256),   S( 63,  67),
            S( 10, -13),   S(  6,   1),   S(  9,  14),   S( 17,  43),   S( 24,  96),   S( 74, 241),   S( 21, 211),   S( 30,  99),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-22, -12),   S( 13,  13),   S(-35,   4),   S(-14, -20),   S(-26, -45),   S(-22, -30),   S(-58, -71),   S(-36,   3),
            S(-35,  43),   S( 49, -49),   S(-47,  10),   S( -8, -30),   S(-15, -18),   S(  0, -40),   S( -1, -70),   S(-79, -41),
            S( -9,  64),   S( -7, -24),   S( 12, -27),   S( -8,  41),   S( 21,  14),   S(-31,   0),   S(-10, -39),   S(-41, -70),
            S(  7, -26),   S( 54, -34),   S( 21,   2),   S( 35,  15),   S( 13,   0),   S(  3,  -7),   S( -9, -22),   S(  0,  -1),
            S( 29, -52),   S( 43, -17),   S(  8,  18),   S( 68, -17),   S( 40,  -6),   S( 50,  -8),   S( 13,  -2),   S(-67, -39),
            S( 28,  -5),   S(-11,   7),   S( 27,  23),   S( 57, -13),   S( 57, -89),   S( 21, -15),   S( 41, -41),   S(-13, -11),
            S( -3, -52),   S( 39, -63),   S( 57, -29),   S( 69, -30),   S( 13, -31),   S(-35, -51),   S(-14, -73),   S( 11, -39),
            S(-78, -87),   S(-27, -27),   S(-12, -29),   S( -4, -62),   S(-22, -37),   S( 40,  48),   S(-14, -26),   S(  7,  -6),

            /* knights: bucket 1 */
            S(-24,  -4),   S(-52,  75),   S(  3,  35),   S(-22,  63),   S(-19,  42),   S(-31,  40),   S(-45,  48),   S(-21, -33),
            S( 28, -13),   S(  5,  39),   S(  5,   1),   S( -7,  25),   S( -6,  16),   S(  2,  15),   S(  2,  -1),   S(-23,  -3),
            S(-24,  29),   S( -2,  15),   S( 10,  10),   S( 28,  23),   S(  8,  29),   S(-18,  18),   S( -5,  -3),   S(-27,  19),
            S( -7,  37),   S( 56,  19),   S( 27,  41),   S( 25,  33),   S( 13,  24),   S( 11,  21),   S( 20,  -4),   S(  8,  18),
            S(  5,  30),   S( 22,  10),   S( 42,  21),   S( 37,  29),   S( 44,  35),   S( 14,  38),   S( 28,   1),   S( 18,   6),
            S( -3,  11),   S( 61, -28),   S( 35,  30),   S( 50,  22),   S( 27,  20),   S( 21,  51),   S( 51,   1),   S( 15,  -6),
            S( 32,  21),   S( 27,   8),   S( 10, -25),   S( -5,  34),   S( 60,  -8),   S( 82, -30),   S(-43,   7),   S(-12, -16),
            S(-104, -78),  S(-35, -49),   S(  5,  -6),   S( -9,  -4),   S( -9, -23),   S(-15, -20),   S(-18, -40),   S(-67, -61),

            /* knights: bucket 2 */
            S(-48,   7),   S( -5,  26),   S(-27,  39),   S(-28,  50),   S(-36,  57),   S(-32,  63),   S(-26,  37),   S(-26,  34),
            S(-32,  -5),   S(-24,  25),   S( -8,  11),   S(-11,  31),   S( -4,  13),   S(-10,  33),   S(-41,  71),   S(-56,  70),
            S(-30,  28),   S(  1,  16),   S(-11,  35),   S( 25,  28),   S( 11,  24),   S(  2,  12),   S(-10,  37),   S(-37,  33),
            S(-11,  40),   S(-34,  38),   S( 10,  44),   S(  5,  48),   S(  9,  42),   S(  3,  47),   S( -1,  57),   S( -2,  37),
            S( 20,   2),   S( -8,  31),   S(-14,  51),   S(-19,  64),   S(  5,  49),   S(-13,  54),   S(  1,  42),   S(-23,  28),
            S( -7,  12),   S( 14,  24),   S(-15,  49),   S(-12,  52),   S(-13,  56),   S(  3,  34),   S(-41,  11),   S( 11, -11),
            S(-14,  27),   S(-34,  11),   S(-33,  28),   S( 11,  11),   S(-22,   1),   S( 30,   3),   S(-63,  30),   S(-35,  -7),
            S(-134,  32),  S(  7, -14),   S(-78,  17),   S(  9, -20),   S( -8,   2),   S(-86,  -8),   S(  8, -11),   S(-199, -69),

            /* knights: bucket 3 */
            S(-68, -10),   S( -4, -13),   S(-52,  16),   S(-29,  32),   S(-14,  15),   S(-14,  14),   S( -6,   5),   S(-38,  -3),
            S(-29,  38),   S(-32,  13),   S(-22,   1),   S(  1,  25),   S(  6,  19),   S(-20,   3),   S(-25,   5),   S(-33,  69),
            S(-19, -21),   S(  1,   3),   S( -7,  15),   S( 18,  28),   S( 18,  29),   S( 11,  11),   S( -4,  18),   S( -8,  50),
            S(-18,  14),   S( -7,  26),   S(  7,  48),   S( 17,  44),   S( 19,  46),   S( 19,  49),   S( 14,  41),   S(  8,  35),
            S(  8,   8),   S(  1,  24),   S( 10,  32),   S( 20,  53),   S( 16,  58),   S( 16,  64),   S( 32,  44),   S( -4,  28),
            S(-17,  23),   S( 32,   2),   S( 44,  14),   S( 55,  23),   S( 63,  -1),   S( 89,  -2),   S(  5,  13),   S( -1,  34),
            S(  1,   2),   S( -9,  30),   S( 65, -28),   S( 35, -14),   S( 51, -31),   S( 63, -39),   S( 43, -57),   S( 39, -29),
            S(-145,  48),  S(-25, -12),   S(-11, -25),   S(-48,  33),   S( 46, -19),   S(-68,  24),   S(-13,   7),   S(-77, -81),

            /* knights: bucket 4 */
            S(  7,   1),   S(-59,   0),   S( 36,  10),   S(  0, -43),   S( -5, -21),   S(-47, -34),   S(  5, -75),   S(-38, -59),
            S( 27,  36),   S(-16,  -2),   S( 32, -30),   S( 38, -25),   S( 27, -24),   S( -9, -62),   S( -3, -22),   S(-27, -58),
            S(-16,  49),   S( 10,  21),   S( 24, -13),   S( 33,  11),   S( 15,  -2),   S(-52,  14),   S(-54, -38),   S(-27, -70),
            S(  3,  66),   S( 51, -35),   S( 92, -14),   S( 26,   2),   S( 49,   8),   S( 99, -42),   S( 18, -59),   S(-11, -50),
            S( 58,  30),   S(-43,  48),   S( 85,  21),   S( 65,  21),   S( 12,  53),   S( 28,  -8),   S(  7, -34),   S(-22, -28),
            S( -2,   7),   S(-33,  27),   S( 65,  17),   S( 20,  -5),   S( 34,   6),   S( 24,  27),   S(  8,   0),   S( -2, -26),
            S( -2,  -8),   S(-12,  35),   S(  6, -11),   S( 10,   3),   S( -1, -31),   S( 16, -33),   S( 12, -17),   S(-19, -28),
            S(-12,   5),   S(  5, -12),   S( 20,  12),   S( -5, -16),   S( 14,  19),   S(  7,  21),   S( -7, -13),   S( -3, -14),

            /* knights: bucket 5 */
            S( 11,   1),   S(  1,   4),   S( 16,  27),   S( 14,  24),   S(  9,  -9),   S( 13, -23),   S(-14,  39),   S(-26, -39),
            S( 21, -15),   S( 37,  25),   S( 37,  -1),   S( 20,  17),   S( 56,  20),   S( 22,  28),   S( 55,  22),   S(  4, -11),
            S( 19,  26),   S( -4,  27),   S( 45,  21),   S( 75,  10),   S( 15,  25),   S(  8,  27),   S( 11,  17),   S(  5, -17),
            S( 50,  27),   S( 41,  12),   S( 37,  33),   S(  5,  40),   S( 55,  28),   S( 42,  36),   S( 59,  23),   S(-20,  23),
            S( 13,  44),   S( 53,  17),   S( 68,  28),   S( 60,  34),   S(118,  35),   S( 38,  35),   S( 59,  21),   S( 45,  35),
            S(-15,  20),   S( -6,  41),   S( 32,  20),   S( 29,  56),   S( 48,  40),   S( 23,  54),   S( 38,  19),   S( -6,  14),
            S( 28,  58),   S(  2,  61),   S( 34,  40),   S( 23,  49),   S( -3,  11),   S( 11,  29),   S( 21,  38),   S( -5,   0),
            S(-16, -20),   S(  6,  21),   S(  0,  20),   S( -7, -14),   S(  5,  29),   S( -9,  -5),   S(  6,  20),   S(-20, -20),

            /* knights: bucket 6 */
            S(-12, -59),   S( -4, -17),   S( 49,  12),   S(-25,  28),   S(  8,  21),   S(  9,  36),   S( 24,  29),   S(-15,  -5),
            S( -5, -34),   S( 93, -13),   S( 42, -10),   S(-12,  21),   S(-28,  43),   S( 81,  19),   S( 35,  19),   S( 19,  20),
            S( -7, -26),   S( 17,   0),   S( 29,   9),   S( 71,  13),   S(-11,  43),   S(-20,  47),   S( 27,  29),   S( 13,  26),
            S( 41,   3),   S( 53,   0),   S( 68,  25),   S( 70,  22),   S( 45,  24),   S( 55,  40),   S( 27,  56),   S(  7,  38),
            S(-24,  26),   S(107, -22),   S( 99,  17),   S( 92,  26),   S(133,  23),   S(104,  31),   S( 39,  45),   S( 38,  45),
            S( 27,  23),   S( 51, -16),   S( 66,  26),   S( 79,  37),   S( 71,  43),   S( 30,  46),   S(  1,  45),   S( 44,  42),
            S(-19,   5),   S(  1,  19),   S(-22,  45),   S( 21,  44),   S(  6,  46),   S( 22,  48),   S( 11,  59),   S(-13,  17),
            S(-41, -11),   S(  7,  31),   S( 35,   9),   S(  6,  35),   S( 27,  31),   S( 15,  45),   S(  6,  35),   S(  6,  -2),

            /* knights: bucket 7 */
            S(-48, -52),   S(-220, -40),  S(-43, -25),   S(-67, -18),   S( -8, -30),   S(-26, -13),   S( -8,   9),   S( -6, -12),
            S(-46, -98),   S(-55, -42),   S(-50, -30),   S(  8, -40),   S(-47,  13),   S( 11, -23),   S( 48,  22),   S( 23,  16),
            S(-56, -72),   S(-75, -18),   S(-28,   1),   S(  9,  -2),   S( 18,  -7),   S( 29, -11),   S( 15,  36),   S( 37,  60),
            S(-37, -34),   S( 40, -22),   S(  1,  12),   S( 58,  -4),   S( 79,  -5),   S( 31,   4),   S( 16,  10),   S(  0,  23),
            S(-82, -31),   S(-13, -27),   S( 85, -38),   S( 78,  -5),   S(119,  -1),   S(101,   9),   S(102,  -2),   S(108,  12),
            S(  4, -53),   S(-23, -29),   S( -5,   0),   S( 66,   2),   S( 74,  10),   S(101,  -7),   S(107, -20),   S(-57,  39),
            S(-19, -27),   S(-62, -27),   S( 21, -29),   S( 41,  -4),   S( 50,  14),   S( 68,  10),   S( 14,  36),   S(  5, -24),
            S(-48, -38),   S(-19, -15),   S(-42, -34),   S( 25,   5),   S(  6,  11),   S( 14,  -3),   S( 15,  -4),   S(  3,   8),

            /* knights: bucket 8 */
            S( -1,   2),   S( -1,   3),   S(  2,  14),   S( 11, -13),   S(-16, -67),   S(-19, -69),   S( -4,  -1),   S( -3, -27),
            S(  0,  -9),   S(-11, -24),   S(  0, -13),   S(-26, -25),   S(-16,  -3),   S(-31, -93),   S(-19, -78),   S(-19, -45),
            S(  5,   6),   S(-13, -25),   S( 14, -17),   S( -5,   7),   S( -8, -41),   S(-37,  -8),   S(-10, -40),   S( -4, -37),
            S(-23,  -4),   S(  2,  10),   S(  4,   2),   S( 13,  33),   S( 18,  19),   S(  3, -30),   S( -9, -43),   S( -6, -20),
            S( 39,  92),   S(-19,  -6),   S( 18,  30),   S( 21,  42),   S( -4,  23),   S( -3,   1),   S( 15,  -2),   S( -3,  -8),
            S(  6,  34),   S( -4,  -3),   S( 35,  15),   S( 32,  -4),   S( -3,   8),   S( -3, -26),   S( -6, -53),   S( -5,   2),
            S(  1,   4),   S( -6,  14),   S(  9,  11),   S(  9,  -4),   S( 12,   8),   S(  6,  18),   S(  2,  14),   S( -4, -12),
            S(  5,   5),   S( 11,  29),   S(  7,  33),   S( -2,  -3),   S( 12,  32),   S( -4, -24),   S(  5,  19),   S( -2,   1),

            /* knights: bucket 9 */
            S(-11, -46),   S(-28, -47),   S(-19, -58),   S(-10, -25),   S(-16, -46),   S(-18, -34),   S(  1,  -6),   S( -3, -18),
            S(-14, -46),   S( -7,  13),   S(-15, -42),   S(-27, -20),   S(  4, -14),   S(-10, -24),   S(  0,  -6),   S(-19, -59),
            S( -1, -19),   S( -7, -18),   S(  1,  -4),   S(-10,  -7),   S( -3,  -4),   S(-27,  15),   S(-10, -27),   S(-14,   2),
            S(-21, -26),   S(-18, -21),   S(  4,  17),   S( 36,  -2),   S( 14,  -5),   S( 12,  29),   S(-18, -29),   S( -2, -10),
            S( 10,  33),   S(  4,  -2),   S( 15,  10),   S( 26,  30),   S( 16,  -1),   S(  6,  14),   S(  8,  -6),   S(  5, -10),
            S(  1,  11),   S( 26,  51),   S( 17,  40),   S(  0, -11),   S( 35,  38),   S( 11,  25),   S(  3,  17),   S(-17, -59),
            S(  0,  -5),   S( -3,  10),   S( 17,  48),   S(-12,  31),   S( 11,  42),   S( -8,   6),   S(  8,  13),   S( -5,   2),
            S( -3,  -6),   S(  7,  19),   S( 17,  43),   S( 17,  60),   S( 10,   1),   S(  6,  25),   S(  1,  10),   S( -1,  -9),

            /* knights: bucket 10 */
            S(-15, -42),   S(-12, -53),   S(-14, -59),   S(-28, -39),   S( -8,  -5),   S(-20, -55),   S(  2,  -3),   S(  9,  13),
            S(-14, -71),   S(-11, -43),   S( -9,  -8),   S(-20, -53),   S(-22, -49),   S(  2, -40),   S( -8, -27),   S(  0,  -1),
            S(-19, -55),   S(-18, -76),   S(-17,  -8),   S( -2, -26),   S( 10,   3),   S( -3,  -6),   S(-33, -35),   S( -7, -25),
            S( -5, -23),   S( -4, -58),   S(  1, -35),   S( 38,  11),   S( 21,   3),   S(  8,   1),   S(  4,  11),   S( 19,  40),
            S( -9, -45),   S(-20, -33),   S(  2,   7),   S( 34,   3),   S( 18,  -5),   S( 21,   1),   S( 10,   4),   S( 17,  48),
            S(-12, -45),   S(-11, -49),   S( -2,  11),   S( 12,  26),   S( 38,  63),   S( 16,  24),   S( 34,  81),   S( 23,  77),
            S(  3,   3),   S(-12, -37),   S( -8, -14),   S( 17,  44),   S(  8,  43),   S( 11,  39),   S(  3,  -1),   S( 11,  19),
            S( -9, -31),   S( -1,   1),   S(  1,  18),   S( -2, -17),   S(  7,  20),   S(  3,  34),   S(  2,  25),   S( -4,  -9),

            /* knights: bucket 11 */
            S( -1,   6),   S(-25, -27),   S(-10, -66),   S(-10, -42),   S(-27, -55),   S(-13,  -6),   S(  4,   6),   S( -7, -19),
            S(-16, -35),   S(-24, -77),   S( -9, -61),   S(-13, -28),   S(-11,  -2),   S(-31, -38),   S(-14, -23),   S(-10, -18),
            S(-16, -68),   S(-41, -50),   S(-28,  -4),   S(  1,  -6),   S(-27, -18),   S(-12,  24),   S( 10,  16),   S(-15, -13),
            S(-21, -51),   S( -3, -26),   S(-10,  19),   S( 12,  32),   S( 20,  11),   S(  1,   8),   S(-23,   6),   S(  0,  15),
            S( -7, -17),   S(-31, -79),   S(  9, -14),   S( 31,  10),   S( 23,  27),   S( 53,  51),   S(  4,  21),   S( 25,  90),
            S(-16, -19),   S(  0, -15),   S( -4,  32),   S( 35,  16),   S( -2,  -9),   S( 46,  54),   S( 14,  29),   S(  5,  15),
            S( 11,  49),   S( -6, -14),   S(  8,  -1),   S( 17,  28),   S( 14,  47),   S( -8,   2),   S( 16,  53),   S( 14,  79),
            S( -5, -13),   S( -5, -32),   S( 13,  36),   S(  6,   5),   S( -5,  -7),   S( -4, -19),   S(  2,   6),   S(  7,  27),

            /* knights: bucket 12 */
            S( -3, -11),   S( -2,   1),   S( -5, -37),   S( -2,   5),   S( -4, -15),   S( -4, -18),   S(  1,  -9),   S(  0,  -3),
            S( -1,  -4),   S( -1,   1),   S(  0,   7),   S(  1,   9),   S(-12, -22),   S( -5, -31),   S( -2, -23),   S(  1,  -1),
            S( -2, -14),   S(  5,   8),   S(-15, -27),   S( -6, -13),   S(  3,  19),   S( -1, -16),   S(  1,  -3),   S( -8, -27),
            S( -3,  -9),   S(  1,   2),   S(  0,  -3),   S( 12,  43),   S(  7,  20),   S(  0,   4),   S(  2,   0),   S(  6,  16),
            S( 12,  27),   S(  8,  32),   S( -5,  -8),   S(  5,  23),   S(  0, -10),   S(  3,  20),   S( -9, -27),   S( -2,   3),
            S( -1,  10),   S(  0, -11),   S(-16,  17),   S(  2,  -4),   S(  2,   2),   S( -6, -34),   S(  0,  -9),   S(  0,   4),
            S( -4, -16),   S(-19, -19),   S( 11,  21),   S(  8,  22),   S( -1,  -9),   S( -8, -17),   S( -2,  -7),   S(  1,   8),
            S(  3,  14),   S(  6,  32),   S( -8, -17),   S(  2,  11),   S( -1,  -2),   S( -5, -18),   S( -5, -19),   S(  0,   3),

            /* knights: bucket 13 */
            S( -4, -10),   S(  2,   7),   S( -2,  -9),   S( -3, -14),   S( -7,  -8),   S( -1, -13),   S( -9, -36),   S( -1,   2),
            S( -6, -19),   S( -1,  -3),   S(  4,  -2),   S( -6, -26),   S(-13, -43),   S(  1,  -4),   S(  2,  11),   S( -1,  -7),
            S( -2,  -8),   S( -8, -17),   S(  1,   5),   S( -1, -15),   S( -5,  -2),   S( -9, -12),   S( -4,   5),   S( -9, -29),
            S(-15, -12),   S(  8,  21),   S(  3,  28),   S( -6,  -2),   S(  6,  -3),   S( 13,   1),   S( -4, -23),   S( -2,  -9),
            S(  5,  17),   S(  4,  44),   S( 18,  27),   S(  8,  36),   S(  5,  14),   S(  4,  18),   S( -4, -13),   S(  4,  12),
            S(  1,  14),   S( 10,  14),   S(  1,  80),   S( -9,   3),   S(  4,  29),   S( -7, -23),   S(  8,  22),   S( -3,  -8),
            S(  5,  20),   S( -2,  24),   S( -5,  -9),   S( -3,  19),   S(  9,  57),   S( -5,   2),   S( -2,  -5),   S( -4, -10),
            S(  1,   7),   S(  5,  39),   S( -2,   8),   S(  6,  24),   S(  4,  28),   S(  4,  15),   S(  0,   0),   S(  1,   4),

            /* knights: bucket 14 */
            S(  2,   1),   S(-10, -46),   S(-10, -23),   S( -4,   2),   S( -7, -33),   S( -1, -13),   S( -1,  -5),   S(  2,  -3),
            S( -2, -11),   S( -3, -14),   S(-16, -48),   S( -7, -34),   S(  7,  -5),   S( -3, -24),   S(  4,  10),   S(  1,   1),
            S(  2,   8),   S( -6, -23),   S(-12, -47),   S(  4,   3),   S(  1, -10),   S(  6,  12),   S(  7,   9),   S( -2,  -6),
            S(  0,   2),   S( -3,  -6),   S(-11, -26),   S( -3, -16),   S(  0, -23),   S( -2, -20),   S( -6,  -9),   S( -7,  -8),
            S(  0,   5),   S(  2,  -5),   S( 12,  44),   S( -4, -35),   S( -4,  -9),   S( 15,   8),   S(  0,  -3),   S( -6, -18),
            S( -6, -26),   S( 10,  18),   S( -1,  12),   S(  6,  -8),   S( 12,  34),   S(  1,   1),   S(  6,  19),   S(  0,   1),
            S(  2,   2),   S( -1,  -1),   S(  2,   2),   S( -4,  -1),   S(  1,  50),   S( -5,  13),   S(  4,  34),   S(  1,   8),
            S( -1,  -5),   S(  6,  20),   S(  1,   7),   S(  7,  44),   S(  6,  33),   S(  9,  46),   S(  0,  15),   S(  0,   2),

            /* knights: bucket 15 */
            S( -1,  -9),   S(  2,  -2),   S( -1, -20),   S(  0,   0),   S( -2,  -3),   S( -1, -27),   S(  1,  -1),   S( -1,  -1),
            S( -3, -16),   S( -1, -12),   S( -3,  -8),   S( -2, -11),   S( -3, -17),   S( -3,  -9),   S( -1,  -5),   S(  0,  -2),
            S( -3, -15),   S( -2, -10),   S(  3,   0),   S(-12, -54),   S(  5,  -4),   S(  5,  13),   S( -6, -16),   S( -2,  -7),
            S( -4, -12),   S( -6, -34),   S(  3, -35),   S( -2,  -1),   S( -3,  -8),   S( 23,  72),   S( -5,  -4),   S(  1,  10),
            S(  0,   4),   S( -4,  -3),   S(  3,   3),   S( -4, -16),   S(  4,   6),   S(  5,   3),   S(-15, -35),   S(-11, -31),
            S( -4,  -4),   S( -1,  -3),   S(-10, -27),   S(  4,  16),   S( -4, -13),   S(  6,  33),   S(-10,  -7),   S(  0,  21),
            S(-10, -32),   S( -8, -16),   S( -1,  -3),   S( -4, -10),   S( -8, -22),   S( 13,  54),   S( -2, -29),   S(  3,  24),
            S(  0,  -2),   S( -1,   1),   S(  1,  -1),   S(  1,   2),   S( -3, -10),   S(-10, -19),   S( 13,  53),   S( -2,  -6),

            /* bishops: bucket 0 */
            S( 34,  38),   S( -6,  26),   S( 53,   3),   S(  6,  28),   S(-20,  10),   S(  8,   0),   S( 18, -25),   S( -5, -20),
            S( 42, -20),   S( 86,  13),   S( 42,   9),   S( 19,   5),   S( -4,  33),   S(-11,   7),   S(-25,   8),   S( 14, -49),
            S( 19,  33),   S( 44,  28),   S( 41,  46),   S(  3,  58),   S( 18,  30),   S(-18,  67),   S(  4,  -9),   S( 25, -47),
            S(  5,   3),   S( 63,   7),   S( 42,  24),   S( 38,  46),   S( -2,  50),   S( 27,  37),   S(  7,   4),   S( -3, -19),
            S( 13,  11),   S( 54, -19),   S(-11,  60),   S( 58,  26),   S( 30,  44),   S( -4,  33),   S( 18,  22),   S(-69,  57),
            S(-29,  59),   S(-20,  65),   S( 45,  36),   S( 41,  30),   S( 26,  56),   S(  6,  13),   S(-45,  49),   S( -1,  48),
            S(-31,  47),   S( 49,  33),   S( 20,  48),   S(-29,  72),   S(-59,  29),   S( 45,  18),   S( 37,  25),   S(-30,  -7),
            S(-52, -17),   S(-11,  37),   S(-10,  43),   S( 10,  30),   S(  6,  33),   S( 16,  26),   S(-10,  48),   S(-37,  47),

            /* bishops: bucket 1 */
            S( 54,  31),   S(  3,  26),   S(  7,  26),   S(  4,  18),   S(  3,  15),   S(  7,  21),   S(  0,   6),   S(-47,  42),
            S( 38, -21),   S( 36,  23),   S( 52,   4),   S( 33,  26),   S(  5,  13),   S( 14,   5),   S(-21,  29),   S(  4,   2),
            S( 30,  17),   S( 16,  25),   S( 50,  32),   S( 13,  31),   S( 14,  42),   S( -4,  42),   S( 31,  15),   S( -7,  -8),
            S( 51,   3),   S( 52,  23),   S( 10,  28),   S( 31,  48),   S( -7,  55),   S( 22,  29),   S( -7,  40),   S( 12,  11),
            S( 46,  30),   S( 21,  36),   S( 25,  34),   S( 11,  47),   S( 17,  46),   S(-29,  59),   S( 30,   3),   S(-30,  41),
            S(-42,  31),   S( 30,  37),   S( 24,  65),   S( 17,  44),   S( -7,  72),   S( 46,  38),   S( -5,  46),   S( 26,  22),
            S( -4,  48),   S( -8,  53),   S(  2,  52),   S(  0,  50),   S( 32,  43),   S(-13,  57),   S(-26,  62),   S(-48,  47),
            S(-10,  56),   S(-33,  46),   S(-22,  31),   S(-35,  35),   S( 17,  48),   S(-21,  33),   S( -5,  49),   S(-24,  89),

            /* bishops: bucket 2 */
            S( 17,  42),   S(  7,  33),   S( -4,  23),   S(-18,  40),   S( -4,  17),   S(-19,  23),   S(-17, -16),   S(-29,  35),
            S(-19,  23),   S( 11,  21),   S( 19,  22),   S(  1,  32),   S( -4,  33),   S(  8,   0),   S(  6,  -1),   S(  1, -27),
            S( -3,  13),   S( -6,  35),   S(  6,  65),   S( -8,  57),   S(  2,  49),   S(  9,  42),   S(  6,  30),   S(-14,   2),
            S( 15,  24),   S(-32,  62),   S(-20,  61),   S(  3,  68),   S( -3,  63),   S(  4,  53),   S( 11,  42),   S(  7,   9),
            S(-16,  42),   S( -5,  40),   S(-33,  64),   S(-25,  68),   S(-26,  79),   S( -1,  63),   S( 13,  28),   S(-23,  26),
            S(-29,  35),   S(-30,  48),   S(  0,  63),   S(-43,  62),   S(  8,  51),   S(-13,  73),   S( 14,  64),   S( 10,  41),
            S(-27,  55),   S(-25,  51),   S(-50,  84),   S(-15,  32),   S(-73,  85),   S(-19,  44),   S(-70,  62),   S(-19,  28),
            S(-88,  94),   S(-65,  90),   S(-101,  73),  S(-88,  69),   S(-64,  45),   S(-123,  71),  S( 16,  26),   S(-84,  76),

            /* bishops: bucket 3 */
            S(-13,  52),   S( 15,  30),   S( 10,  29),   S( -6,  32),   S(  0,  30),   S( 41,  -3),   S( 26, -25),   S( 53, -46),
            S( -6,  34),   S(  6,  44),   S( 16,  26),   S(  5,  56),   S( 10,  35),   S(  9,  30),   S( 47,  34),   S( 28, -10),
            S( 21,  11),   S( -4,  52),   S(  6,  76),   S( 11,  52),   S( 11,  77),   S( 17,  63),   S( 28,  43),   S( 50, -10),
            S( 24,  15),   S(  1,  58),   S( -2,  78),   S( 15,  81),   S( 18,  65),   S( 21,  59),   S( 13,  61),   S( 10,  15),
            S( -2,  43),   S( 16,  42),   S( 21,  45),   S( 25,  75),   S( 10,  71),   S( 36,  47),   S( 16,  51),   S(  6,  62),
            S(  1,  40),   S(  7,  52),   S( 26,  56),   S(  7,  52),   S(  9,  60),   S( 40,  63),   S( 34,  43),   S(  9,  65),
            S( -3,  45),   S(-19,  66),   S( 25,  53),   S( 14,  45),   S( -4,  58),   S(-22,  66),   S( -4,  62),   S( 13,  69),
            S(-39, 101),   S(-37,  70),   S( 35,  48),   S(-20,  70),   S(-72,  83),   S(-26,  70),   S(-27,  54),   S( 92,  40),

            /* bishops: bucket 4 */
            S(-20, -34),   S(-58,  27),   S(-47,   6),   S(-29,  -1),   S(-27,  25),   S(-29,  11),   S(-22,  -6),   S( -4, -18),
            S(-35,  23),   S( 53,  -7),   S(  0,  26),   S(-44,  37),   S(-13,   8),   S( 72, -30),   S(-28,  -9),   S( 17, -38),
            S(-16,  20),   S(-55,  16),   S( 25,  17),   S(-24,  31),   S( 27,   9),   S( 33,  12),   S(  1, -16),   S(-79,  15),
            S(-16,   7),   S(-12,  28),   S( 49,  22),   S( 65,  35),   S(-17,  43),   S( 60,  10),   S( -8,  26),   S( 11, -17),
            S( -2,  22),   S( -1,  49),   S(-14,  64),   S( 25,  46),   S( 13,  28),   S(-33,  40),   S(-41,  15),   S(  5,  35),
            S( -4,  14),   S( 47,  29),   S(  7,  12),   S( 11,   7),   S(  9,  42),   S(  7,  24),   S(  4, -10),   S(-16,  13),
            S(-31,  12),   S( 52,  39),   S( -5,  37),   S( 12,  65),   S(  5,  31),   S(-10,  12),   S( 16,  17),   S( -2, -23),
            S( 12,  38),   S(-21,   9),   S(  2,   7),   S(-13,  -8),   S(  5,  37),   S( -4,  10),   S(-11,  -4),   S(-14,  -3),

            /* bishops: bucket 5 */
            S(-21,  35),   S(  4,  30),   S( -9,  25),   S(-55,  38),   S(  1,  19),   S(-20,  16),   S(-34,  23),   S(  3,  28),
            S(-29,  25),   S(-39,  44),   S(-43,  52),   S( 48,  15),   S( -2,  23),   S( -5,  14),   S(-15,   2),   S(-34,  22),
            S(-31,  44),   S(-24,  46),   S( 26,  41),   S(  6,  35),   S(-29,  50),   S(-33,  38),   S(-39,  43),   S(-30,   3),
            S( 27,  34),   S( 34,  44),   S(-21,  52),   S( 40,  39),   S( 53,  31),   S( 41,  40),   S(  4,  26),   S( 11,  34),
            S( 55,  42),   S( 33,  39),   S( 56,  31),   S( 93,  30),   S( 75,  26),   S( 60,  24),   S( 24,  36),   S( -9,  23),
            S(  7,  44),   S( 73,  35),   S( 67,  35),   S( 20,  49),   S(-14,  43),   S(  9,  21),   S( 11,  45),   S( 10,  43),
            S( -3,  50),   S(-28,  46),   S(  1,  47),   S( 24,  51),   S( 15,  44),   S(-16,  65),   S(  2,  44),   S(  6,  23),
            S(-18,  51),   S(  2,  13),   S( 13,  24),   S(  6,  49),   S( 24,  40),   S(  3,  42),   S(  2,  67),   S(-17, -17),

            /* bishops: bucket 6 */
            S( -5,  58),   S(  4,  33),   S(-34,  31),   S(-13,  23),   S(-25,  28),   S(-83,  53),   S( -9,  47),   S(-17,  35),
            S( -8,  23),   S(-22,  34),   S( -4,  28),   S(-21,  45),   S(  9,  25),   S(-14,  27),   S(-88,  60),   S( 23,  34),
            S( 33,   8),   S(-19,  33),   S( 41,  39),   S( 41,  28),   S( 63,  18),   S( 51,  24),   S( 22,  35),   S(-71,  42),
            S( 47,  33),   S(  7,  52),   S(  1,  58),   S( 44,  42),   S(  6,  55),   S( 59,  27),   S( 41,  43),   S(-21,  22),
            S( -7,  52),   S( 34,  44),   S( 51,  25),   S( 51,  38),   S(101,  47),   S( 67,  27),   S( 32,  49),   S( -5,  63),
            S(-23,  25),   S( 10,  31),   S( 20,  42),   S( 27,  46),   S(  0,  65),   S( 57,  50),   S( 37,  32),   S(-30,  68),
            S(-44,  38),   S( -3,  40),   S( 35,  30),   S( 23,  31),   S( 25,  50),   S(-18,  39),   S( 24,  47),   S(-16,  47),
            S( -4,  65),   S( -5,  44),   S( 21,  33),   S( 14,  49),   S(-22,  41),   S( 16,  45),   S(  7,  41),   S( 14,  60),

            /* bishops: bucket 7 */
            S(  8,  -8),   S(-19,  19),   S(-41, -13),   S(-33,  -3),   S(-39,  -4),   S(-108,  27),  S(-63, -39),   S(-64, -15),
            S(-26,  -9),   S(-78,  23),   S(-29,  12),   S( 15,   0),   S(-20,   9),   S(-69,  31),   S(-24,  -7),   S(-48, -21),
            S(-53,   9),   S(  7, -15),   S(  5,  26),   S( 25,  21),   S(-37,  35),   S( -9,  18),   S(-13,  12),   S( -7,   9),
            S(-46,  35),   S( 14,  33),   S(108, -10),   S( 64,  18),   S(101,  10),   S( 19,  27),   S( 46,  20),   S( 10,  18),
            S( -9,   2),   S(-44,  19),   S( 53,   8),   S( 89,   9),   S( 91,   7),   S( 97,  19),   S( 45,  27),   S( 48,  -1),
            S(-13,  -9),   S(-14,  19),   S( 30,   2),   S( 46,   8),   S( 61,  -5),   S( 70,  24),   S( 86,  18),   S(  2,  30),
            S( -8,  -7),   S(-26,  19),   S(  4,  14),   S(-16,  14),   S( 16,  -1),   S( 34,   5),   S( 15,  12),   S( 14,  35),
            S(-22,  -7),   S(-11,  29),   S(-22,  18),   S( -9,  12),   S( 12,  12),   S(  8,   5),   S( 27,  16),   S( 27,  19),

            /* bishops: bucket 8 */
            S(-17, -68),   S( -7, -62),   S(-35, -49),   S(-12, -36),   S(  5,  -4),   S( -5, -37),   S( 20,  34),   S(  1,  -1),
            S(  2, -10),   S(-19, -79),   S( -7, -26),   S(-23, -36),   S( 18, -22),   S( -6, -31),   S( -7, -52),   S( 15,  -6),
            S(  8,  12),   S( -7,   5),   S(-18, -10),   S( 26, -26),   S(  0,  -8),   S( 16, -22),   S( -4, -34),   S(-21, -19),
            S(  8,  30),   S( 12,  -2),   S( 16,   5),   S( 11,   0),   S(  7,  -1),   S( 10, -15),   S(  2, -16),   S( -7, -46),
            S( 19,  41),   S(  9,  30),   S( 25,  -2),   S( 62,  15),   S( 24, -14),   S( 18,  -7),   S( 17, -22),   S( -4, -30),
            S( -9,  -1),   S( 15,   5),   S(  5,  18),   S(  8, -26),   S( 42, -14),   S(  1, -46),   S( -5, -47),   S(-13, -36),
            S( -5,  -6),   S( 28,  15),   S( 11,  -7),   S(  0,  -1),   S( 13, -18),   S( -1, -27),   S( -6, -26),   S(-16, -21),
            S( -9, -27),   S( -3, -50),   S(  3, -18),   S( -5, -25),   S(-12, -54),   S( -2, -32),   S( -6, -36),   S( -6, -38),

            /* bishops: bucket 9 */
            S(-15, -64),   S(  1, -81),   S(-24,  12),   S(-17, -44),   S( -8, -51),   S(-14, -56),   S(-13, -36),   S( 12,  -8),
            S(-12, -51),   S(-16, -36),   S(-29, -37),   S(  5, -16),   S(-26, -30),   S(-14, -40),   S( -7, -35),   S( -4,  -6),
            S(  9, -15),   S( 13, -12),   S(  1, -23),   S( 13, -28),   S( 52, -28),   S( 24, -44),   S(-18,  -1),   S( -1, -25),
            S(-12, -33),   S( 22,   5),   S(  5, -18),   S( 53, -21),   S( 46,   0),   S(  4, -15),   S( 19, -24),   S(-13, -69),
            S(  8,  -8),   S( 26, -16),   S( 37,  -5),   S( 38, -10),   S( 35, -34),   S( 22, -28),   S( 26, -19),   S(-11, -19),
            S(  3, -16),   S( 34,  -5),   S(  5,  11),   S( 13,  -1),   S( 29, -14),   S( 30, -19),   S( -2, -53),   S(-14, -46),
            S(  3,  -1),   S( 31,  -3),   S( -5, -31),   S( 21, -13),   S( 32, -23),   S(  5, -51),   S( -2, -24),   S( -4, -33),
            S(  3,   2),   S( -5, -33),   S(  0, -22),   S(-14, -53),   S(-14, -38),   S( 11,   3),   S( -6, -44),   S(-12, -71),

            /* bishops: bucket 10 */
            S(-19, -59),   S( -1, -43),   S(-32, -18),   S(-11, -28),   S( -5, -24),   S(-38, -62),   S( -8, -56),   S(-21, -89),
            S(  9, -34),   S(-30, -49),   S(  2, -55),   S( -3, -40),   S( -4, -37),   S( 17, -16),   S(-18, -60),   S(-12, -43),
            S(  3, -54),   S( 23, -42),   S(  6, -59),   S( 34, -39),   S( 25, -32),   S(  3, -20),   S(-12,  -7),   S(  7,  12),
            S(  6, -26),   S( 16, -27),   S( 11, -29),   S( 33, -37),   S( 55, -20),   S( 11, -22),   S(-15, -13),   S(  9, -21),
            S(-16, -46),   S( 18, -30),   S( 54, -51),   S( 65, -19),   S( 49, -18),   S( 48,  -6),   S( 33,  -2),   S( -4, -55),
            S( -3, -22),   S( 19, -45),   S(  8, -32),   S(  6, -44),   S( 44, -34),   S( 39,  17),   S( 15, -24),   S(  0, -33),
            S(-21, -86),   S( -7, -54),   S(-11, -65),   S( 19, -21),   S(  0, -32),   S( 12,  -2),   S( 28,  49),   S( 15,  36),
            S( -8, -58),   S( -4, -29),   S(  1,   0),   S(  0, -15),   S(-12, -38),   S( -1, -46),   S(  2, -12),   S(  4,   7),

            /* bishops: bucket 11 */
            S(-19, -12),   S(-38, -29),   S(-61, -44),   S(-17, -44),   S(-14, -42),   S(-79, -85),   S( -5, -28),   S(-23, -71),
            S(-13, -70),   S( 14, -52),   S(  2, -19),   S( -9, -37),   S(-32, -35),   S(-25, -41),   S(-33, -57),   S(-38, -57),
            S( -5, -78),   S( 27, -55),   S(-24, -18),   S( 44, -53),   S(  7, -24),   S(  2, -34),   S( -8,  -2),   S(-17, -11),
            S(-20, -37),   S(-17, -47),   S( 44, -45),   S( 41, -26),   S( 53, -31),   S( 12,   3),   S( 26,  -8),   S( 17,  22),
            S( -7, -32),   S( -2, -61),   S( 22, -10),   S( 77, -31),   S( 59, -11),   S( 41,  -4),   S( 13,  24),   S( 10,   4),
            S( -6, -26),   S( 12, -58),   S( -2, -49),   S( 37, -23),   S( 10, -28),   S( 47,   7),   S( 23,  -1),   S(-16, -24),
            S( -9, -38),   S( 11, -28),   S(  0, -22),   S( 12, -49),   S( 18, -17),   S( 32, -44),   S( 13, -39),   S(  3,   3),
            S( -9, -67),   S(-12, -41),   S(  2, -53),   S( 10, -25),   S( 10, -49),   S(-23, -75),   S(  9,   2),   S(  1, -11),

            /* bishops: bucket 12 */
            S(  1,  -1),   S(-12, -41),   S(-11, -43),   S( -5, -32),   S( -1,  -4),   S( -1,   8),   S( -2,   6),   S( -5, -16),
            S( -3, -22),   S(-10, -44),   S(  2, -20),   S( -3,  10),   S(-10, -45),   S(  3, -10),   S(  0,   2),   S( -2,  -6),
            S( -5, -23),   S(-20, -34),   S(  7,  -4),   S(  1, -23),   S(  4,  -7),   S(  2, -11),   S(-16, -58),   S(  4,  -5),
            S( -3, -12),   S(  9,  -7),   S(-12, -40),   S( -5, -12),   S(  7, -12),   S(  4,  24),   S( -5, -29),   S( -7, -27),
            S( -7, -24),   S(-10,  -7),   S(  5, -36),   S( -3,  -3),   S( -1, -21),   S(  1, -18),   S(  4, -25),   S( -5,  -1),
            S(-15, -26),   S( 16,  41),   S(  2,  23),   S( -3, -25),   S( 19, -22),   S( -8, -36),   S(  2, -22),   S(  0, -10),
            S( -6, -13),   S( -3, -21),   S(  2,  14),   S(-14, -39),   S(  3,  -5),   S(  2, -13),   S( -7, -24),   S( -3,  -1),
            S( -1, -11),   S( -5,  -5),   S( -6, -50),   S(  0,  -8),   S(  1, -18),   S(  0, -11),   S(-17, -50),   S( -1,  -2),

            /* bishops: bucket 13 */
            S(-10, -62),   S( -5, -50),   S(-14, -66),   S( -2, -26),   S(-10, -61),   S(-10, -20),   S(  2,   0),   S(-12, -61),
            S( -1, -14),   S( -5, -56),   S( -9, -56),   S(-11, -60),   S(  1,  -9),   S( -1, -10),   S(  0, -33),   S(  4, -19),
            S( -8, -34),   S(  4,  -3),   S(  3, -24),   S( -2, -64),   S(  3, -50),   S( 12, -28),   S(-10, -32),   S(  8,  29),
            S( -5, -19),   S( -2, -37),   S(  2, -44),   S( -9, -51),   S( 23, -13),   S( 12,  -5),   S(  4,   7),   S( -6, -20),
            S( -2,   1),   S( -5,  -6),   S( -5, -58),   S( 23, -10),   S(  9, -26),   S(  2, -33),   S( -5, -51),   S( -2, -39),
            S( -2, -15),   S(  3,  -9),   S(-11,  -7),   S( 11,   1),   S(  1, -11),   S( 14, -39),   S( 17,  12),   S( -2, -15),
            S( -4, -22),   S( -4, -23),   S(-10, -28),   S( -9,  -8),   S( -6, -31),   S(  6,  17),   S( -9, -61),   S(  3,   1),
            S( -9, -45),   S( -2,  -2),   S(  2,  -9),   S( -3, -17),   S(  9,  20),   S( -8, -45),   S(  3,  11),   S( -7, -41),

            /* bishops: bucket 14 */
            S( -9, -52),   S(-11, -52),   S(-12, -53),   S(-14, -68),   S( -6, -45),   S( -7, -54),   S(-13, -89),   S( -6, -25),
            S(-12, -27),   S( -4, -28),   S( -1, -17),   S(-21, -91),   S(  2, -19),   S( -5, -50),   S(-14, -57),   S(  3, -10),
            S( -5, -32),   S( -7, -31),   S(-13, -66),   S( -1, -50),   S( -6, -54),   S(-11, -80),   S( -6, -30),   S( -2,  -9),
            S( -8, -40),   S(  2, -19),   S( -6, -23),   S(  1, -29),   S( 17, -33),   S( -5, -64),   S(-21, -75),   S( -6, -22),
            S( -5, -30),   S(  6, -27),   S(  1, -37),   S(  2, -41),   S(  3, -51),   S(  1, -21),   S(  4, -18),   S( -8, -36),
            S( -5, -47),   S( -4, -39),   S( -6, -44),   S( -8, -32),   S(  9, -12),   S(  2,  18),   S(  3, -32),   S(-11, -40),
            S( -8, -41),   S(  9, -23),   S( -4, -36),   S(  8, -15),   S(-10, -39),   S( -8, -41),   S( -2, -17),   S( -4, -19),
            S( -7, -42),   S( -8, -39),   S(  0,  -6),   S( -9, -46),   S( -4, -29),   S(  2,  -1),   S( 11,  39),   S( -3, -34),

            /* bishops: bucket 15 */
            S( 10,  34),   S( -1,   4),   S(-12, -55),   S( -1, -25),   S( -8, -34),   S( -8, -10),   S( -2, -11),   S( -5, -25),
            S(  8,  14),   S(  2,   4),   S(  9,  20),   S(  3, -10),   S( -4, -15),   S(  4,  13),   S( -4, -22),   S( -3, -15),
            S( -8, -39),   S( -6, -26),   S( -3, -43),   S(  0,  12),   S( -5, -72),   S(-12, -22),   S(  1, -17),   S(  3,  -1),
            S( -3, -21),   S(-15, -56),   S( -1,  -1),   S( -9, -59),   S(-11, -45),   S(-12, -57),   S(  6,   5),   S( -3,  -6),
            S(  0, -14),   S(  3, -11),   S( -3, -44),   S(-20, -69),   S(  4, -21),   S( -1,   0),   S(  8,   0),   S( -3, -30),
            S(-11, -48),   S( -4, -64),   S(-13, -48),   S(-10, -61),   S(  2, -30),   S(  6,  17),   S( 23,  49),   S( -3,  -3),
            S( -2, -26),   S( -2, -36),   S( -3, -24),   S(  1,  -9),   S(-13, -45),   S(  5,   0),   S(-11, -23),   S(  5,  12),
            S( -7, -31),   S( -2,  -3),   S( -5, -44),   S( -5, -40),   S( -8, -46),   S(-15, -43),   S(-13, -62),   S(  2,   6),

            /* rooks: bucket 0 */
            S(-25,  -3),   S( 11, -27),   S(  2, -13),   S( 18, -19),   S( 14,  -3),   S( 10, -13),   S( 13,   8),   S( 22,  -3),
            S( 30, -90),   S( 39, -24),   S( 18, -10),   S(  9, -15),   S( 42, -16),   S( 33, -45),   S(-11,  11),   S(-37,  18),
            S( 11, -10),   S( 28,  10),   S( 48,  -5),   S( 17,  11),   S(  5,  30),   S( 11,   8),   S(-21,  26),   S(-39,  12),
            S( 20, -35),   S( 60,   1),   S( 61,  14),   S( 63,  -3),   S( 45,  -8),   S( -5,   2),   S(-11,  28),   S(-29,  31),
            S( 47, -15),   S( 60,   2),   S(103,  -4),   S( 51,  -3),   S( 54,  -3),   S( 27,   6),   S( 18,  23),   S(-20,  22),
            S( 91, -51),   S(107, -34),   S( 64,   4),   S( 41,   5),   S( 68,  10),   S(-49,  37),   S( 58,  10),   S( -9,  14),
            S( 47, -13),   S( 78,  -2),   S( 22,  34),   S( 46,   2),   S(  0,  21),   S(  4,  11),   S( 16,  24),   S(-19,  31),
            S( 36,   8),   S( 19,  16),   S( 50,   7),   S( 14,  27),   S(-13,   6),   S( 34,  10),   S( 34,  27),   S( 38, -10),

            /* rooks: bucket 1 */
            S(-64,  27),   S(-42,  12),   S(-44,  -7),   S(-35,  -6),   S(-19, -10),   S(-23, -10),   S(-27,  -1),   S(-30,  15),
            S(-61,  17),   S(-64,  27),   S(-34,  15),   S(-20, -27),   S(-31,   6),   S(-36,   1),   S(-38,  -8),   S(-55,   3),
            S(  8,  12),   S(-39,  51),   S(-13,  15),   S(-41,  40),   S(-31,  24),   S(  9, -11),   S(-12,  14),   S(-45,  22),
            S(-41,  37),   S(-31,  50),   S( -8,  39),   S(-16,  30),   S(-45,  52),   S(-25,  46),   S(-16,  48),   S(-17,  10),
            S( 41,  27),   S( 43,  25),   S(  6,  42),   S(-32,  53),   S( -1,  45),   S( 34,  20),   S(  7,  25),   S(-43,  22),
            S( 87,   8),   S(  5,  42),   S( 38,  26),   S( -7,  24),   S( 11,  23),   S( -1,  28),   S(  9,  21),   S(-61,  53),
            S(-42,  55),   S( 20,  35),   S( 46,  35),   S(-79,  75),   S(-22,  47),   S( 11,  52),   S(-22,  13),   S(-71,  50),
            S( 57,  19),   S( 27,  22),   S( 21,  28),   S(-41,  67),   S(  4,  23),   S( 44,   8),   S( -4,  31),   S( 11,  16),

            /* rooks: bucket 2 */
            S(-64,  49),   S(-40,  25),   S(-42,  22),   S(-54,  25),   S(-55,  22),   S(-51,  11),   S(-34,  -9),   S(-49,  33),
            S(-72,  35),   S(-66,  43),   S(-36,  24),   S(-46,  15),   S(-34,  10),   S(-48,   4),   S(-60,  14),   S(-60,  13),
            S(-64,  58),   S(-40,  50),   S(-49,  65),   S(-30,  22),   S(-55,  49),   S(-33,  43),   S(-13,  10),   S(-23,  16),
            S(-79,  62),   S(-53,  74),   S(-36,  70),   S(-12,  58),   S(-34,  49),   S( -5,  32),   S(-18,  51),   S(-19,  35),
            S(-12,  58),   S(-43,  75),   S(-25,  65),   S(-13,  57),   S( 34,  37),   S(  5,  61),   S(-19,  54),   S(-30,  53),
            S(-14,  48),   S(-39,  66),   S(-14,  54),   S(  6,  34),   S( -7,  53),   S( 56,  31),   S( 15,  37),   S(-25,  40),
            S(-58,  62),   S(-59,  82),   S(-21,  72),   S( 23,  58),   S( 27,  46),   S( 52,  24),   S(-46,  82),   S(-26,  54),
            S( -5,  67),   S(  9,  52),   S(-52,  80),   S( -9,  47),   S( 11,  50),   S(-11,  50),   S(-29,  68),   S( 50,  25),

            /* rooks: bucket 3 */
            S(  2,  76),   S(  5,  70),   S( 12,  64),   S( 19,  55),   S( 17,  53),   S( -4,  70),   S(  2,  73),   S( -9,  48),
            S(-23,  83),   S( -9,  71),   S(  9,  63),   S( 11,  61),   S( 24,  55),   S( 15,  56),   S( 47,  15),   S( 17, -37),
            S(-29,  84),   S(  0,  77),   S( 13,  81),   S( 15,  72),   S( 22,  77),   S( 25,  73),   S( 44,  50),   S( 19,  40),
            S(-26,  93),   S(-20, 102),   S( 20,  80),   S( 34,  76),   S( 20,  77),   S(  5, 110),   S( 68,  70),   S( 20,  69),
            S(  8,  97),   S( 39,  76),   S( 26,  78),   S( 49,  76),   S( 36,  84),   S( 64,  68),   S(104,  55),   S( 59,  48),
            S( -6, 102),   S( 18,  84),   S( 17,  86),   S( 37,  73),   S( 32,  57),   S( 57,  56),   S(111,  40),   S( 94,  22),
            S(-33, 109),   S(-19, 117),   S(  5, 103),   S( 43,  92),   S( 36,  79),   S( 49,  72),   S( 58,  75),   S(123,  25),
            S(-44, 144),   S( 13, 106),   S( 14,  86),   S( 58,  67),   S( 74,  57),   S( 93,  49),   S(122,  44),   S(125,  32),

            /* rooks: bucket 4 */
            S(-62,  13),   S( 26, -29),   S(-57,  14),   S( -3,   7),   S( -2, -43),   S( 15, -49),   S(-11, -25),   S(  8, -59),
            S( -8, -39),   S(-26,   6),   S(-70,  35),   S( -9,   3),   S(-16,  -1),   S(-24, -14),   S(-29, -25),   S(-53, -23),
            S(-21,  31),   S(-63, -18),   S(-16,   8),   S( 21, -18),   S(-24,  18),   S(-11, -23),   S( 11,  -8),   S(-61, -24),
            S(-48,   2),   S( 23,   1),   S(-32,  25),   S(  9,  -3),   S( 32,  -1),   S( -9, -10),   S(-18,  26),   S(-19,   7),
            S(-63,  14),   S(-36,  30),   S( -3,   8),   S( 56,  27),   S( 35,   2),   S(-12,  -9),   S( 46,  -8),   S( 13,  -4),
            S(-12,  21),   S( 45,   5),   S( 60,  14),   S( 24,  19),   S( 50,  18),   S( 10,  30),   S( 15,   9),   S(  8,  19),
            S(  5,  16),   S( 30,  36),   S( 56,  11),   S( 39,  29),   S( 61,  -1),   S( 14, -17),   S( 21,  25),   S( 25,  20),
            S( 20, -47),   S( 46,  21),   S( 37,  10),   S( 33,   6),   S( 15,   0),   S( 16, -16),   S( 13,   4),   S( 18,  12),

            /* rooks: bucket 5 */
            S(-36,  44),   S(  5,  21),   S( -8,  19),   S(-16,  10),   S( 17,  -5),   S( -8,  24),   S( 22,  15),   S(-32,  40),
            S(-14,  16),   S(-42,  30),   S(-60,  56),   S(-60,  33),   S(-64,  47),   S(-21,  11),   S( 24,   1),   S( -9,  -1),
            S(-31,  44),   S(-51,  54),   S(-95,  78),   S(-73,  55),   S(-52,  31),   S(-12,  32),   S(-21,  42),   S(-13,  31),
            S(-51,  60),   S(-45,  61),   S(-33,  53),   S(-34,  53),   S(-49,  78),   S(-22,  56),   S(  2,  51),   S( -1,  29),
            S(  2,  64),   S(-15,  79),   S( 16,  65),   S(  2,  81),   S( 81,  29),   S(  5,  65),   S( 65,  46),   S( 15,  28),
            S( 55,  59),   S( 33,  67),   S( 19,  61),   S(  7,  70),   S( 72,  37),   S( 56,  46),   S( 44,  51),   S( 47,  40),
            S( 40,  50),   S( 37,  63),   S( 54,  49),   S( 46,  56),   S( 58,  45),   S( 76,  36),   S( 83,  29),   S(104,  29),
            S( 58,  42),   S( 83,  19),   S( 35,  50),   S( 20,  26),   S( 66,  33),   S( 61,  33),   S( 40,  39),   S( 20,  58),

            /* rooks: bucket 6 */
            S(-49,  36),   S(-25,  35),   S( -6,  23),   S(-14,  21),   S(-45,  28),   S(-45,  43),   S(-34,  63),   S(-13,  41),
            S(-23,  30),   S( 11,  24),   S(-14,  28),   S(-47,  35),   S(-47,  47),   S(-84,  60),   S(-65,  48),   S( 21,  18),
            S(-41,  49),   S(-12,  37),   S(  9,  31),   S(-65,  58),   S(-19,  41),   S(-46,  72),   S(-70,  71),   S( 14,  19),
            S(-68,  75),   S( 26,  57),   S(-15,  61),   S(-25,  57),   S(-11,  57),   S( -7,  49),   S(-97,  90),   S(  2,  56),
            S(  4,  71),   S( 29,  70),   S( 55,  49),   S( 22,  47),   S(  0,  86),   S( 43,  71),   S( 35,  58),   S(  7,  58),
            S(-11,  75),   S( 62,  52),   S( 73,  47),   S( 43,  38),   S( 38,  43),   S( 35,  81),   S( 45,  60),   S(102,  32),
            S( 55,  54),   S( 57,  55),   S( 89,  42),   S( 87,  34),   S( 95,  44),   S( 53,  54),   S( 97,  35),   S( 67,  48),
            S( 85,  62),   S( 47,  59),   S( 66,  36),   S( -1,  52),   S( 63,  51),   S( 83,  44),   S(112,  32),   S( 26,  58),

            /* rooks: bucket 7 */
            S(-69,  -2),   S(-42,  11),   S(-23, -20),   S(-11, -12),   S(  0, -14),   S(-23,  15),   S(-47,  29),   S(  1, -11),
            S(-65,  13),   S(-31,   3),   S(-15,  -4),   S( -9,   0),   S(  1,   8),   S( 11,   7),   S(  5,  -1),   S(-37,   2),
            S(-103,  59),  S(-51,  24),   S( -5,  23),   S(-23,   7),   S( 17,   0),   S(-25,   7),   S(-11,  12),   S(-36,  13),
            S(-73,  33),   S(-17,  38),   S(  1,  20),   S( 55,   7),   S( -1,  17),   S( 23,   9),   S( 58, -12),   S(-15,  10),
            S( -5,  36),   S(-10,  34),   S( 42,  -2),   S( 70, -11),   S( 35,  22),   S(112,   4),   S( 54,  30),   S( 36,  -3),
            S( -2,  37),   S( -6,  25),   S(106, -23),   S( 69,  -6),   S( 90, -15),   S( 92,  16),   S( 59,  38),   S( 29,   2),
            S( 21,  31),   S( 48,  16),   S( 67,  13),   S( 86,   4),   S( 77,  11),   S( 86,   9),   S( 57,  33),   S( 70,  -1),
            S(  4,  56),   S( 23,  33),   S( 16,  22),   S(105, -31),   S( 31,  15),   S(  1,  21),   S( 70,  -5),   S( 68,  -8),

            /* rooks: bucket 8 */
            S(-31, -12),   S(-27, -22),   S( -8,  10),   S(-50,   7),   S(-23, -45),   S(-35, -56),   S(-50, -58),   S(-24,   1),
            S(-20,  -3),   S( -2,   0),   S(-20, -23),   S( -9, -11),   S(-26,   1),   S(-10, -37),   S( -6, -37),   S( -2, -62),
            S(  8,  -2),   S( 12,   1),   S(  1, -11),   S(-24,  -2),   S(-29, -24),   S(-16, -10),   S(  1,  10),   S( -7, -18),
            S(-13, -18),   S(-19,  -7),   S(  4,   8),   S(  6,   5),   S( -7,  -2),   S(-28, -11),   S(-11, -34),   S(-17, -22),
            S(-15, -39),   S(  3,   6),   S(-20,  25),   S(-13,   4),   S(-15, -28),   S( 13,   0),   S(-12, -18),   S(-13, -37),
            S( 10,  21),   S(-19,   0),   S( 22,  27),   S( 20,  -1),   S( 12,  25),   S( -3, -15),   S( -6, -20),   S(  4,  53),
            S( -8,  -4),   S(  6,  -9),   S( 18,  10),   S( 16,  -7),   S( 34,  -1),   S( 18,   0),   S( 10,  -7),   S(  4,   4),
            S(  0, -84),   S( -5, -10),   S(  9,  13),   S( -8, -21),   S( -1,  -1),   S(  2, -16),   S( 16,  10),   S( 12,  21),

            /* rooks: bucket 9 */
            S(-43, -49),   S(-20, -46),   S(-48, -28),   S(-76, -18),   S(-49,  -7),   S(-48, -33),   S( -2, -43),   S(-68, -37),
            S( 23, -29),   S( -8, -49),   S(-28, -57),   S(-20, -28),   S(-30, -59),   S( 19,  -6),   S(-10, -41),   S(-26, -63),
            S( -4, -44),   S( 15, -39),   S(-12,  -5),   S(-48, -22),   S(-46, -26),   S( 18, -11),   S(  1, -21),   S(-12, -11),
            S(  7, -16),   S( -6,  -9),   S(-21,  -9),   S(-24, -11),   S(  2, -10),   S( -6, -16),   S( 15,  -1),   S(-17, -31),
            S( -6,  15),   S(-26,   4),   S( -1,  14),   S(  1,  10),   S( 23,  30),   S( 24, -14),   S( -4,   5),   S( 13, -42),
            S(  1,   1),   S(-15, -13),   S( -1,  -4),   S(-32,   3),   S(  3, -44),   S( 16,   2),   S(  8, -21),   S(  4, -26),
            S( 51,   9),   S( 36, -21),   S( 23, -13),   S( 36,  -4),   S(  7, -32),   S( 24, -17),   S( 23, -31),   S( 34,  13),
            S( 29, -72),   S( 12, -39),   S( 13,  -5),   S( 23,  29),   S( -2,  13),   S(  8, -25),   S(  8, -21),   S( 26,  -5),

            /* rooks: bucket 10 */
            S(-72, -109),  S(-47, -48),   S(-11, -69),   S(-62, -18),   S(-57, -54),   S(-37, -53),   S(  0, -52),   S(-35, -44),
            S(-28, -36),   S(-22, -44),   S(-16, -61),   S(-47, -21),   S(  7, -27),   S(-30, -31),   S( 19, -14),   S( -6, -46),
            S(-36, -37),   S(-22, -41),   S(-35, -30),   S( -3, -13),   S(-29, -17),   S( -5, -25),   S( 21, -29),   S(  2, -15),
            S( -7, -12),   S(-10, -21),   S(-24, -34),   S(-28, -15),   S( -4,  -4),   S(  1, -10),   S( -9, -12),   S(-14, -16),
            S(  1, -26),   S(  4, -15),   S(-13, -30),   S( 20, -34),   S(-12,  -5),   S(  5,  -4),   S( 17,  18),   S( 10,   6),
            S( 31,  -5),   S( 14,   9),   S(  9, -12),   S( 16,  -7),   S(  1, -10),   S(  2, -21),   S( 18,  -2),   S( -6, -11),
            S( 73, -12),   S( 74, -23),   S( 55, -23),   S( 45, -33),   S( 39, -20),   S( 29,   2),   S( 39, -35),   S( 35, -18),
            S( 34,   5),   S( 16, -16),   S( 14, -39),   S( 12,  -6),   S( 34,  -4),   S( 29,   0),   S( 20, -25),   S(  6, -31),

            /* rooks: bucket 11 */
            S(-63, -38),   S(-41, -31),   S(-25, -30),   S(-35, -59),   S(-28, -12),   S(-41, -31),   S(-30, -42),   S(-52,  -9),
            S(-43, -25),   S(  3, -58),   S(-18, -11),   S(-22, -22),   S(-22, -29),   S(-31, -28),   S( -8, -17),   S( -7,  -8),
            S(-14, -44),   S( -3, -31),   S(-14, -19),   S(-17, -16),   S(-23,  -7),   S(-31,   3),   S(-30, -31),   S(-23, -40),
            S(-32,  11),   S(-15, -28),   S(-14,  -1),   S(-17,   8),   S(  3, -14),   S(-13,  23),   S( -9, -32),   S(-22, -30),
            S( -7,  17),   S( 14, -14),   S(  7,   0),   S( 12, -18),   S( 21,  -4),   S( 16,  -4),   S(  4,   6),   S(-27, -35),
            S( 25,  38),   S(  7,   0),   S( 14,  -2),   S( 35,   8),   S( 28,   1),   S( 21,   9),   S(-33, -24),   S(  7,  23),
            S( 60,  45),   S( 27,   2),   S( 45,  -5),   S( 49,  -1),   S( 10,   8),   S( 45,  17),   S( 33,  35),   S( 45, -20),
            S( 37,  25),   S(  6, -11),   S(  7,  -8),   S( 27, -14),   S( -2, -15),   S(  7,   6),   S( 15,  14),   S( 17,  -9),

            /* rooks: bucket 12 */
            S( 13, -48),   S(-17, -41),   S(-13, -13),   S(-26, -24),   S( -1,   2),   S( -6, -50),   S(-38, -90),   S(-25, -56),
            S( 14,   6),   S( -1,  -9),   S( -8,  25),   S(-10, -27),   S(  4,  14),   S( -5, -24),   S( -7, -26),   S( -6, -35),
            S( 11,  -2),   S(-13, -13),   S(-17, -25),   S(-19, -50),   S(-20, -46),   S(  1, -10),   S( -3, -14),   S(  0, -24),
            S(  1,  13),   S(  0, -10),   S( 14,  30),   S(  3,  -3),   S(-11, -35),   S( -8, -40),   S( -5,  -4),   S( -1, -26),
            S(-11, -14),   S(-11,  -2),   S( 10, -23),   S( 10,  11),   S(-11, -34),   S(  3,  -8),   S(-12, -35),   S( -4, -28),
            S(-11, -12),   S( -3, -35),   S( 22,  -4),   S(  3, -20),   S(  0, -23),   S( -5, -29),   S( -6, -46),   S( -1,   1),
            S(  4,  -5),   S(  0, -13),   S(  2,  -7),   S(  8,  -3),   S(  6, -13),   S( -3, -34),   S(  2, -13),   S(  3, -15),
            S(-16, -62),   S(  8,  24),   S( 11,  -4),   S(-15, -36),   S(-14, -46),   S(-16, -52),   S(-13, -70),   S(  9,  17),

            /* rooks: bucket 13 */
            S(-32, -64),   S( -8, -17),   S(-12,  -7),   S( -7,  34),   S(  7,  30),   S(-17, -30),   S(-15, -52),   S(-27, -33),
            S( -4, -27),   S( -3,   1),   S( -3,  16),   S( -2,   2),   S(-13, -45),   S(  2,   4),   S(  7,  19),   S( -3, -23),
            S( -4, -21),   S(-14, -32),   S( -3,   3),   S( -7, -38),   S( 13,  40),   S(  5, -21),   S( -5, -25),   S( -5, -33),
            S(-16, -56),   S(  2,  22),   S(-10,  -5),   S( -4, -14),   S(  4,   0),   S(-18, -38),   S(  3, -18),   S(  0,  -9),
            S( 10,  -8),   S(-10, -23),   S( 16,  23),   S(-21, -65),   S(-11, -16),   S( -9, -45),   S(-15, -58),   S( 10,  10),
            S(-10, -33),   S( -5,  -8),   S(-22, -46),   S(  8, -18),   S(  9,  -7),   S(  9,   6),   S(  7,   0),   S( -1, -13),
            S(-13, -23),   S(  6,  13),   S(  2,  20),   S(  3,   7),   S( -4, -22),   S( 16,  18),   S(  3, -28),   S(-10, -46),
            S(-19, -113),  S(-17, -81),   S( -2,  -8),   S( -4,   1),   S( -6, -10),   S(  0, -13),   S(-15, -61),   S( -9, -27),

            /* rooks: bucket 14 */
            S(-13, -50),   S(-36, -50),   S(  8,  13),   S( -7, -38),   S(-13,  -1),   S( -8, -13),   S(  1, -60),   S(-25, -44),
            S(-15, -25),   S(-23, -61),   S( -5,  12),   S(-21, -42),   S(-26, -56),   S( -9, -38),   S(  1,  10),   S( 16,  28),
            S( -6, -20),   S( -8, -29),   S(  0,  -3),   S( -6, -22),   S(-27, -40),   S(-12, -29),   S( -2,   1),   S( -4,  -9),
            S(  8,  18),   S(-20, -68),   S( -1,  -3),   S(  2,  12),   S( 12,   4),   S( -3,  -6),   S( -6, -68),   S(-17, -13),
            S(-11, -63),   S( -5, -42),   S(-11, -64),   S(-12, -66),   S(  0, -36),   S(-12, -47),   S( -3, -60),   S(  5,  -7),
            S( -8, -56),   S(  3,   1),   S( -4, -50),   S(  3, -35),   S( -7, -76),   S(  7, -38),   S(  4, -14),   S( -5, -35),
            S(  9, -21),   S(  3, -34),   S(-15, -60),   S( 11, -56),   S( -2, -58),   S(  3, -37),   S( 21,   0),   S( -8, -29),
            S( -6, -42),   S( -7, -27),   S( -9, -44),   S( 11,  23),   S( -9, -21),   S( -4,  -2),   S(  9,   0),   S(-12, -21),

            /* rooks: bucket 15 */
            S( -7, -27),   S(-23, -67),   S(-12, -45),   S(-12, -30),   S( -2, -16),   S(  0,  -4),   S(-21, -64),   S(-18, -21),
            S(-16, -39),   S(-21, -48),   S(-10, -18),   S( -5,   2),   S(  1,   0),   S(  3,  22),   S(-20, -53),   S(  7,  11),
            S( -9, -26),   S(-14, -43),   S(-24, -80),   S( -3, -20),   S( 10,  11),   S( -2, -16),   S( -6,  -9),   S( -6, -30),
            S( -9, -44),   S( -8, -49),   S(  1, -10),   S( -4, -17),   S( -7, -34),   S( -4, -10),   S( -6, -29),   S(-11,  -5),
            S(-12, -38),   S( -5, -47),   S(  2, -18),   S( -8, -43),   S( -6, -42),   S(  2, -23),   S(  0, -12),   S(  0,   9),
            S(  5,  -5),   S( -5, -24),   S(  3, -34),   S(  4, -72),   S(-10, -82),   S(  2, -44),   S(  9, -22),   S( -6,  -7),
            S(  5,  -1),   S(  4, -34),   S( 17,  -4),   S(  0, -48),   S( -5, -44),   S(  1,  -7),   S(  4,  -7),   S(  2,  -6),
            S( -4, -21),   S(  1,  -3),   S(  0, -26),   S(  0, -21),   S(  3, -42),   S( 20,  40),   S(  4, -16),   S(  1, -11),

            /* queens: bucket 0 */
            S(-25, -24),   S(-16, -43),   S( 51, -84),   S( 61, -77),   S( 35, -77),   S( 23, -21),   S( 60,  -8),   S( -5,  -9),
            S(-23, -19),   S( 27, -42),   S( 34,  -5),   S( 29,   0),   S( 23,  31),   S( 19,  16),   S( 22,  46),   S( 21,   9),
            S( 32,   0),   S( 43,   1),   S( 22,  22),   S( 20,  33),   S( 16,  20),   S(  4,  34),   S(  0,  36),   S( 25,  43),
            S( 15,  31),   S( 19,  17),   S( 26,  26),   S( 25,  42),   S( 23,  33),   S( 27,  20),   S( 15,  19),   S( 17,  21),
            S( 36,  48),   S( 24,  15),   S( 20,  35),   S( 36,  60),   S(-31,  51),   S( 24, -26),   S( 20,  12),   S( 29,   0),
            S( 20,  63),   S( 30,  61),   S( 25,  27),   S( 16,  46),   S( 30,   0),   S(  5,  53),   S( 48,  31),   S(  5,   8),
            S( 55,  23),   S( 51,  44),   S( 11,  69),   S( 69,  29),   S(  2,  16),   S( 22,  -9),   S(  6,  14),   S( 30,  16),
            S( 59,  24),   S(  6,  32),   S( 59,  31),   S( 51,  59),   S( 48,  53),   S(-21,   1),   S( 70,   2),   S( 45,  16),

            /* queens: bucket 1 */
            S( -7, -30),   S(-70,  -6),   S(-29, -23),   S( -7, -102),  S(  6, -39),   S(-18, -39),   S( 42, -21),   S(  6,   0),
            S( -2, -16),   S(-15, -56),   S( 28, -69),   S( 12,  27),   S(  8,  -8),   S( 17, -15),   S( 28, -16),   S(  4,  10),
            S(-34,  52),   S(  4,   9),   S( 11,  17),   S(  5,  14),   S(  4,  38),   S( -3,   4),   S( 27, -19),   S( 26,  21),
            S( 10,   5),   S(  2,   8),   S(-11,  67),   S( 35,  52),   S( -3,  43),   S( 12,   2),   S( -1,  -9),   S( 19,  12),
            S( 24, -12),   S( 33,  -7),   S(-14,  69),   S( -4,  50),   S(-12,  62),   S(  7,  33),   S(-21,  36),   S( 27,   5),
            S( 22,  30),   S( 16,  62),   S( 24,  60),   S(-37,  87),   S(-33,  37),   S(-38,  65),   S( 46,  32),   S( 26,  42),
            S(-13,  60),   S(  7,  59),   S( -7,  22),   S(  5,  59),   S(-26,  81),   S( 11,  59),   S( -5,  60),   S(-30,  42),
            S( 13,  -2),   S(  8,  29),   S( 49,  43),   S( 26,  19),   S( 17,  41),   S( 25,  23),   S( 20,  46),   S(-19,  30),

            /* queens: bucket 2 */
            S( 17,  -6),   S( 35, -77),   S( 23, -61),   S(  6, -23),   S(-20,  13),   S(-29, -40),   S(-18, -13),   S(  1,  38),
            S( 12,  -3),   S( 22,  15),   S( 23, -12),   S( 27, -19),   S( 20, -18),   S( 16, -54),   S( 15, -12),   S( 42, -34),
            S( 28,  -4),   S( 25,  -5),   S( 13,  36),   S( 16,  28),   S( 15,  58),   S( 21,  40),   S( 17,  21),   S( 36,  10),
            S(  9,   1),   S(-14,  63),   S( -1,  55),   S( 22,  55),   S( -4,  72),   S(  9,  87),   S( 19,  11),   S( 14,  35),
            S( 13,   9),   S(  6,  48),   S(-10,  71),   S(-53, 120),   S(-16, 106),   S(-30, 119),   S(-17, 121),   S(-12, 116),
            S(  6,  13),   S(-18,  68),   S(-21,  87),   S(-11,  71),   S(-37, 123),   S(-31, 131),   S( -9, 100),   S(  4,  75),
            S(-16,  36),   S(-47, 121),   S(-34,  90),   S(  6,  75),   S(-30, 109),   S(  6,  84),   S(-56,  74),   S(-11,  73),
            S(-58,  86),   S( 14,  61),   S( 33,  35),   S( 56,  47),   S( 15,  66),   S( 41,  45),   S( 18,  24),   S(  9,  21),

            /* queens: bucket 3 */
            S( 82,  71),   S( 69,  73),   S( 53,  98),   S( 47,  86),   S( 69,  28),   S( 55,  -2),   S( 34,  -6),   S( 47,  30),
            S( 66, 108),   S( 64, 106),   S( 51, 102),   S( 54,  80),   S( 52,  74),   S( 60,  43),   S( 73,   6),   S( 30,  47),
            S( 60,  90),   S( 55, 107),   S( 56,  87),   S( 56,  77),   S( 55,  88),   S( 56,  96),   S( 56, 118),   S( 74,  33),
            S( 42, 128),   S( 39, 103),   S( 56,  82),   S( 52, 106),   S( 43,  95),   S( 38, 139),   S( 60,  96),   S( 41, 131),
            S( 53,  96),   S( 60,  99),   S( 39, 115),   S( 34, 100),   S( 35, 137),   S( 18, 142),   S( 20, 174),   S( 40, 153),
            S( 53, 118),   S( 52, 117),   S( 43, 113),   S( 31, 132),   S( 47, 127),   S( 74, 113),   S( 49, 162),   S( 28, 193),
            S( 45, 139),   S( 46, 130),   S( 68, 105),   S( 61, 115),   S( 41, 131),   S( 36, 122),   S( 78, 143),   S(159,  53),
            S( 66, 109),   S( 97,  94),   S( 64, 111),   S( 85, 104),   S( 30, 109),   S( 95,  78),   S(132,  41),   S(115,  53),

            /* queens: bucket 4 */
            S( -3, -24),   S(-25, -25),   S(-48, -21),   S(-11, -22),   S( 29, -31),   S( -8,   8),   S(-74, -42),   S(-28, -17),
            S(-17, -37),   S(-54, -35),   S( 49, -19),   S(-34,   4),   S(  2, -17),   S(  4, -33),   S(  3,  10),   S(-42, -34),
            S(  0,   1),   S( 32,  16),   S( 11,  32),   S( 25,  23),   S( 29, -23),   S(  4, -16),   S(-18, -37),   S(-34, -15),
            S(-18,  -4),   S( 15,  33),   S(-25,  51),   S(-32,  35),   S( 35,  27),   S( 33,  26),   S(  6, -31),   S( 13,  20),
            S( -5,  13),   S( 23,   9),   S( 11,  44),   S( 37,  61),   S( 15,  34),   S( 39,   1),   S(-17, -14),   S(-15, -34),
            S( -9,  15),   S(  6, -16),   S(-14,  43),   S( 37,  59),   S( 33,  19),   S( -3, -10),   S(-19, -18),   S(-11, -10),
            S( 18,   5),   S(-18,   5),   S( 16,  21),   S( 48,  79),   S( -8,   4),   S( 19,  22),   S(  8, -19),   S(-38, -46),
            S(-16, -29),   S( -8, -27),   S( 44,  61),   S(  7,  21),   S(-20,  -7),   S(-13,   0),   S(-10, -38),   S(-24, -39),

            /* queens: bucket 5 */
            S(-42, -32),   S(-53, -53),   S(-46, -48),   S(-50, -38),   S(-71, -42),   S( 16,  -9),   S(-30,   4),   S(-17, -14),
            S(-16, -11),   S(-54, -17),   S(-63, -40),   S(-37,   6),   S(  6, -44),   S(-25, -12),   S(-68, -21),   S(-52, -38),
            S(-29,  11),   S(-37, -10),   S(-47,  18),   S( -3,  36),   S( 59,  67),   S(-10,   4),   S( 15,  -9),   S( 20,  10),
            S(-54, -11),   S(-49, -15),   S(  8,  43),   S(-17,  72),   S( 19,  24),   S(-21,  27),   S( 19,  -9),   S( -3,  46),
            S(-23,  -4),   S(-34,  22),   S(-11,  50),   S( 35,  74),   S( 38,  61),   S(-38, -15),   S( 15,  20),   S( -1,  -3),
            S(-40,   3),   S(  5,  30),   S(-13,  33),   S( 13,  30),   S( 11,  48),   S( 27,  29),   S( -7,   3),   S(-20, -17),
            S( -9, -23),   S( -2,   1),   S(  9,  59),   S( 20,  47),   S( 29,  51),   S( 42,  55),   S( 23,  23),   S(-12, -18),
            S( -3,  -1),   S(  9,  11),   S( -5,  -3),   S( -2,  41),   S( 22,  51),   S( 19,  31),   S(-21, -44),   S(-20, -14),

            /* queens: bucket 6 */
            S(-34,  -4),   S(-25,  -3),   S(-57, -12),   S(-64, -57),   S(-80, -61),   S(-43, -34),   S(-63, -38),   S( -7,  14),
            S(-67, -36),   S(-25,  -8),   S(-46,  27),   S(-14,   7),   S(-59,  29),   S(-82, -16),   S(-93, -26),   S(-30,   3),
            S(-23,  18),   S(-18,  29),   S(-70,  16),   S(-116,  87),  S(-16,  55),   S(-24, -13),   S(-25, -19),   S(-30, -22),
            S(-33,  16),   S(-30,  11),   S(-34,  50),   S(-53,  76),   S(  1,  55),   S( 11,  70),   S( -8,  39),   S( 17, -31),
            S(-71,  32),   S( -6,  63),   S(-25,  68),   S(  2,  65),   S( 35,  66),   S( 59,  50),   S( 62,  49),   S( -2,  32),
            S(-28,  44),   S(-16,  25),   S( 51,  15),   S( 47,  41),   S( -2,  67),   S( 61,  76),   S(-34,   0),   S(-14,   3),
            S( -7,  -6),   S( 11,   7),   S( 15,  61),   S(-22,  31),   S( 17,  52),   S( 41,  65),   S(-30,   7),   S(-42, -10),
            S( 23,  40),   S( -1,  35),   S(  2,   9),   S( -8,  42),   S( 35,  48),   S( 18,  25),   S(-19,   4),   S( 25,  12),

            /* queens: bucket 7 */
            S( -6, -10),   S(-34,  54),   S(-47,  -1),   S(-17, -14),   S(-32,   4),   S(-50, -34),   S(-50,   0),   S(-49, -19),
            S(-38, -14),   S(-37,  -4),   S(-20,  24),   S(-11,  21),   S(-19,  19),   S(-21,  18),   S(-29,  60),   S(  1, -31),
            S(-70,  14),   S(-62,  41),   S( -7,  34),   S(-11,  44),   S( 19,  36),   S( 10,  38),   S( 18,  -1),   S(-34,  -6),
            S(-41,   4),   S(-17,  -3),   S( -7,  20),   S( -6,  50),   S( 53,  18),   S( 34,  46),   S(  6,  55),   S( 12,  32),
            S(-30,  28),   S(-62,  32),   S(-10,  38),   S( 49,   6),   S( 55,  23),   S( 79,  12),   S( 32,  24),   S( 38,   9),
            S(-13,  30),   S( -4,   0),   S(-14,  29),   S(  9,  25),   S( 17,  57),   S( 66,  41),   S( 71,  15),   S( 42,   4),
            S(  4,   4),   S(  4,  13),   S( 15,   0),   S( 22,  31),   S( 49,  38),   S( 68,  32),   S( 35,   4),   S( 61,  48),
            S( 15,  32),   S( 36,  14),   S( 20,  33),   S( 49,  60),   S( 64,  33),   S( 10,  20),   S(  3,  -8),   S( 52,  41),

            /* queens: bucket 8 */
            S(-16, -31),   S( -6, -26),   S(-13, -13),   S( -2,   2),   S( -3, -10),   S(-11, -25),   S(-27, -42),   S(-12, -21),
            S( -5,  -3),   S(-17, -26),   S(-18, -18),   S(-23,  -2),   S(-24, -34),   S( -3, -14),   S(-28, -51),   S(-11, -15),
            S( -5, -10),   S( -8,   7),   S(-12, -31),   S(  0,  -2),   S(-28, -34),   S(-16, -23),   S(-19, -52),   S(-15, -23),
            S(-11,   1),   S( 20,  34),   S( 11,  33),   S( -6,   9),   S( -8, -11),   S(-12, -19),   S(  5,   4),   S( -3, -24),
            S( 15,  36),   S( 13,  36),   S( 26,  64),   S( -1,  24),   S(  8,  35),   S(  8,  24),   S( -2,  -6),   S(-18, -35),
            S( 11,  26),   S( -6,  -2),   S(-25,   3),   S( 13,  48),   S( -2,  -4),   S( -3,   1),   S(-12, -17),   S(  3,   5),
            S(-21, -32),   S(-16, -19),   S( 10,  31),   S(  6,   5),   S( 15,  23),   S(  1,   1),   S( -8,  -6),   S( -7, -23),
            S(-21, -43),   S( 15,  10),   S(-19, -46),   S(-11, -24),   S(-11, -17),   S( -8, -24),   S(-10, -37),   S(-12, -23),

            /* queens: bucket 9 */
            S(  5,  14),   S(-14, -31),   S( -9, -14),   S(-30, -49),   S(-30, -56),   S(-21, -20),   S(-14, -36),   S(-16, -36),
            S(  5,  16),   S(-11, -13),   S(-15, -18),   S(-19, -21),   S(-17, -20),   S(-16, -28),   S( -1, -19),   S( -4, -11),
            S(  4,   7),   S( -7,   4),   S( -9,  44),   S(-25, -52),   S(-24, -33),   S(-15, -13),   S(-10, -14),   S(-14, -29),
            S(-18, -17),   S( -6,  -5),   S( 22,  56),   S( -3,   2),   S(  2,  15),   S(  1,   2),   S(-18, -34),   S( -4, -20),
            S(  6,  21),   S( -9,  11),   S(  4,  22),   S( 24,  54),   S(  1,  35),   S(-13,  -1),   S(  4,  14),   S(-13, -15),
            S(-31, -46),   S(-27,  -8),   S( -1,  16),   S(-15,   3),   S(  0,  -6),   S(  3,  12),   S(-16, -17),   S( -8,  -4),
            S(-21, -27),   S(-18, -26),   S(-11,  23),   S( 11,  26),   S( 11,   4),   S(-16, -36),   S(  6,  17),   S(-22, -46),
            S(-21, -47),   S(-12, -22),   S( -1,  13),   S( 14,  26),   S(  2,  -4),   S(-11, -17),   S(  1,   0),   S(  0,   6),

            /* queens: bucket 10 */
            S( -4,  -8),   S(  5,  16),   S(-11, -19),   S(-39, -62),   S(-12, -22),   S(-10,  -4),   S( -6, -20),   S( -9, -11),
            S(-13, -31),   S(-19, -29),   S(-22, -35),   S( -4,  -5),   S(-16, -11),   S(-31, -44),   S( -8,  -7),   S(-12, -16),
            S( -5, -11),   S( -2, -11),   S( -8, -10),   S( -8,  -3),   S(-35, -31),   S(-11,   1),   S(-13, -16),   S(  4,   6),
            S(  1,  11),   S( -9, -10),   S(-16, -22),   S(-10, -15),   S(  6,  24),   S(-28,   6),   S(-10, -13),   S(-21, -37),
            S(-21, -34),   S(  5,   5),   S(-26, -21),   S(  2,  38),   S( -7,   7),   S( 17,  37),   S(  7,  -2),   S(-11, -22),
            S(-10, -23),   S(-15, -27),   S(  0,  25),   S( -9,  32),   S(  0,  34),   S(  0,  24),   S(  6,  -4),   S(-17, -26),
            S( -2,  -3),   S(-16, -17),   S( -4, -14),   S( -4,  13),   S( 14,  32),   S(  4,   7),   S(-21, -40),   S(  9,  22),
            S(  0,   0),   S(-16, -33),   S( -7,  -6),   S(-21, -21),   S( 12,  -3),   S( -3, -11),   S( -1,   7),   S(-13, -19),

            /* queens: bucket 11 */
            S(  1, -12),   S(-21, -36),   S(-25, -19),   S(-11, -23),   S(-20, -39),   S(  1,  -1),   S( -8, -12),   S(-12, -38),
            S( -9, -16),   S(-17, -14),   S(-45, -53),   S(-32, -23),   S(-13, -12),   S(-24, -37),   S(  5,   6),   S(-11, -11),
            S(-34, -34),   S(-32, -54),   S(-15, -18),   S(-28, -28),   S( 20,  38),   S(-12,  -2),   S(  0,  10),   S(-20, -15),
            S(-18, -28),   S(-15, -34),   S(-22, -33),   S(  6,  18),   S(  2,  -5),   S(-16,   0),   S( 21,  29),   S(-22, -27),
            S( -8,  -6),   S( -2, -17),   S(-16, -14),   S( 34,  41),   S(  7, -14),   S( 26,  45),   S( 26,  47),   S( -3,   3),
            S( -9, -26),   S(  1,   4),   S(-11,   0),   S(  9,   5),   S( 29,  30),   S( 46,  36),   S( 16,  16),   S(-19, -13),
            S(-10, -14),   S(-20, -37),   S(  2,  15),   S(-20, -27),   S(  7,  16),   S( 17,  18),   S( 36,  46),   S(-11, -23),
            S(-21, -34),   S(-12, -35),   S(-19, -29),   S(  6,  -4),   S(  3,  -2),   S(-23, -51),   S( 22,  18),   S(-15, -47),

            /* queens: bucket 12 */
            S(  5,   4),   S(  7,   9),   S(  1,   6),   S(-13, -17),   S( -5,  -6),   S( -6, -12),   S(  2,   6),   S(  0,   3),
            S( -6,  -8),   S( -3, -11),   S( -7,  -9),   S( -4, -13),   S(-10, -26),   S(-11, -16),   S( -5, -17),   S( -5, -10),
            S( -9, -16),   S( -1,   2),   S( 24,  18),   S(-12, -31),   S(  2,   1),   S(-13, -22),   S(-10, -23),   S( -5, -11),
            S( -5,  -9),   S( -2,   2),   S( 10,  25),   S(  3,  -4),   S( 17,  26),   S( -2, -14),   S(  6,  15),   S(-10, -23),
            S( -9, -20),   S( 18,  44),   S( 34,  65),   S(  1,  15),   S(  3,  19),   S(  1,  -3),   S( -9, -16),   S( -9, -22),
            S(  9,  22),   S(  9,  31),   S( 20,  29),   S(  0, -10),   S(  2,   6),   S(-16, -31),   S(  3,  10),   S( -1, -11),
            S( -3,  -3),   S(  2,   0),   S( 22,  35),   S(  6,  18),   S( 13,  21),   S(  3,   9),   S(  2,   3),   S(  4,  10),
            S(-16, -46),   S(-11, -29),   S(-11,  -3),   S(-18, -23),   S(-10,  -9),   S( -4, -10),   S( 12,  18),   S(  3,  16),

            /* queens: bucket 13 */
            S( -2,  -9),   S( -8, -18),   S( -9, -21),   S( -8, -11),   S( -5, -11),   S(  2,  -6),   S( -9, -27),   S( -6, -15),
            S(  0,   1),   S(  3,  10),   S( -2,  -2),   S( -6, -10),   S( -7, -16),   S(  0,   1),   S( -7, -18),   S(-13, -26),
            S( -1,  -3),   S( -2, -11),   S(  4,  14),   S(  0, -12),   S(-17, -32),   S(-10, -20),   S(-14, -28),   S(-12, -20),
            S( -8, -14),   S(  7,  10),   S( -4, -15),   S( 15,  33),   S( 10,  16),   S(-13, -38),   S(-14, -32),   S( -1,  -6),
            S( -3,  -4),   S(  8,  20),   S(  4,   3),   S( 19,  36),   S( 28,  62),   S(  0, -11),   S(-13, -32),   S( -8, -19),
            S( -3,  -7),   S( 13,  40),   S( 36,  59),   S( 27,  63),   S(  3,  19),   S(  0,  -1),   S(  0,  -6),   S( -9, -20),
            S(-11, -21),   S(  8,  11),   S(  9,  29),   S( 16,  46),   S( -7,  -3),   S(  8,  12),   S( -7, -15),   S( 16,  37),
            S(-22, -47),   S( -8, -29),   S( -7,  -9),   S(-13, -18),   S(  8,   5),   S( 16,  26),   S(-10, -19),   S(  5,  10),

            /* queens: bucket 14 */
            S( -3,  -6),   S(  2,   1),   S( -3, -11),   S( -9, -24),   S(  8,  16),   S( -6, -15),   S( -1, -10),   S( -6, -18),
            S( -7, -18),   S( 12,  25),   S( -6, -22),   S( -9, -16),   S( -6, -15),   S(  2,  -6),   S( -1,  -1),   S( -8, -23),
            S( -2,  -5),   S(-17, -33),   S( -7, -14),   S( -5,  -7),   S(  2,   2),   S( -3,  -8),   S(  4,   8),   S(-14, -18),
            S( -7, -16),   S(  1,  -2),   S(-10, -14),   S( 18,  39),   S( 11,  23),   S( -4,  -1),   S( 15,  28),   S(  2,  -1),
            S( -5, -11),   S( -6,  -9),   S( -7, -12),   S(  6,  -4),   S( 13,  32),   S(  6,  12),   S( -2, -13),   S( -9, -22),
            S( -7, -14),   S(  0,  -4),   S( 12,  24),   S( 10,  26),   S(  9,  33),   S( 11,  28),   S(  2,  -3),   S( -6, -13),
            S(  1,  -1),   S(  3,  -8),   S( 16,  27),   S(  7,  17),   S( 13,  23),   S(  6,  13),   S( 16,  28),   S(  2,   2),
            S(  0,  -9),   S( -7, -19),   S( -7,  -6),   S( 18,  14),   S(  2,   0),   S( -3, -11),   S(-12, -25),   S(-12, -27),

            /* queens: bucket 15 */
            S(  2,  -3),   S(-12, -31),   S(  1,   0),   S( -2,  -8),   S( -8, -12),   S( -8, -19),   S( -2,  -7),   S(  3,   2),
            S( -9, -18),   S( -6, -17),   S(  0,  -2),   S(  3,   7),   S( -1,  -7),   S(-10, -16),   S(  4,  10),   S(  0,  -2),
            S(  1,   3),   S( -5, -11),   S( -4, -11),   S( -1,  -4),   S( -1,  -4),   S( 12,  22),   S( -7, -13),   S(  0,   0),
            S( -1,  -6),   S(  4,  -2),   S(  4,   9),   S( 10,  13),   S(  1,   1),   S( -6,  -8),   S(  4,   2),   S( -1,   3),
            S( -2,   0),   S(  0,  -8),   S(-13, -29),   S( -2, -12),   S( 12,  22),   S(  4,   5),   S( 10,   9),   S(  2,  -4),
            S( -4,  -7),   S( -5, -16),   S(  7,  11),   S( -6, -19),   S( 11,  15),   S( 21,  34),   S( -4, -10),   S(-11, -28),
            S( -7, -10),   S( -7, -23),   S(  4,   7),   S(  7,   4),   S( -8, -13),   S( 15,  28),   S(  9,  17),   S(  5,   6),
            S( -2,  -5),   S( -3, -15),   S( -6, -12),   S(  4,   2),   S(  1,  -5),   S(  9,  11),   S(  0,  -6),   S(-14, -28),

            /* kings: bucket 0 */
            S( 19,  48),   S( 12,  65),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  51),   S( 97,  63),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-14,  21),   S(-78,  34),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 27,  38),   S(  6,  44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-47,  41),   S(-43,  31),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 26,  45),   S( 25,  40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19,  61),   S(  0,  43),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 56,  87),   S(  5,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -41),   S( 22, -22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  -3),   S( -5,  20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0, -46),   S(-35, -22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11, -15),   S(-24, -11),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-17, -16),   S(-35, -17),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10,  -5),   S(-11,  -5),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 24,  14),   S( -9, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 21,  40),   S(-42,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94, -51),   S(  3, -22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57, -47),   S( 18, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-34, -38),   S(-15, -51),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 66, -55),   S( 47, -45),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  7, -56),   S(-38, -35),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 63, -56),   S( 64, -54),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -4, -30),   S(-106, -50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 83, -48),   S( -3, -60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8, -48),   S( 37, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31, -77),   S(  3, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 28, -37),   S( 73, -38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 22, -64),   S( 19, -62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 64, -48),   S( 18, -48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19, -68),   S( -6, -59),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 69, -45),   S(-27, -68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-25, -48),   S(-21, -124),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27,  -6),   S(-40,   8),   S(-10,   2),   S(-12,   3),   S( 10,  -1),   S( 24,  14),   S( 30,  -5),   S( 38,   0),
            S(-14, -28),   S(-38, -11),   S(-23,  -4),   S(-15,  -4),   S(  5,   9),   S( -7,  17),   S( 23,  -7),   S( 17,  16),
            S( 18, -24),   S( -9, -13),   S( 24, -28),   S(  2, -10),   S( 12,   2),   S( 20,  29),   S(  3,  31),   S( 50,  13),
            S( 38, -27),   S( 47, -12),   S( 45, -18),   S( 29,   7),   S( 18,  35),   S(-11,  75),   S( 16,  75),   S( 74,  53),
            S( 98, -62),   S(100,  -1),   S( 86,  -3),   S( 46,  19),   S( 42, 133),   S( 24,  93),   S( 15, 144),   S( 92,  90),
            S(-150, -100), S(-122, -107), S( 38, -155),  S( 43,  19),   S(111, 153),   S( 91, 152),   S(126,  74),   S( 61, 163),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  18),   S(-53,  12),   S(-35,   3),   S(-51,  47),   S(-27,   5),   S( -1,   7),   S( -2,  -1),   S(  1,  18),
            S(-55,   1),   S(-40,  -6),   S(-45,   6),   S(-24,   4),   S( -7,   0),   S(-25,   3),   S( -9,  -6),   S(-17,   8),
            S(-35,  13),   S(-16,  10),   S(-22,  -5),   S( 10, -18),   S(  0,  12),   S(-12,  10),   S(-17,  14),   S(  5,  11),
            S(-10,  30),   S( 33,   2),   S(  0,   7),   S( 18,  22),   S( 11,  19),   S(-30,  28),   S( 20,  18),   S( 37,  39),
            S( 17,  13),   S( 88, -28),   S(121, -36),   S(101, -33),   S( 47,   7),   S(  4,  39),   S(-26,  52),   S( 44,  82),
            S(167, -55),   S( -2, -36),   S( 58, -123),  S( 37, -111),  S(-42, -54),   S(-61,  68),   S( 42, 129),   S( 97, 133),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58,  39),   S(-47,  18),   S(-40,   8),   S(-21,  15),   S(-52,  37),   S(-36,  17),   S(-22,  -4),   S(-32,  24),
            S(-51,  16),   S(-43,  10),   S(-46,   6),   S(-45,  23),   S(-42,  20),   S(-44,   5),   S(-26, -11),   S(-54,  14),
            S(-31,  36),   S(-38,  37),   S(-19,  16),   S(-23,  20),   S(-32,  29),   S(-21,   6),   S(-32,  10),   S(-23,   9),
            S( -8,  70),   S(-31,  57),   S(-16,  34),   S(  4,  30),   S( -7,  29),   S(-19,  11),   S( 13,  10),   S( 28,   6),
            S(-25, 118),   S(-46, 100),   S(-31,  42),   S( 38, -34),   S(108, -27),   S(108, -15),   S( 89, -29),   S( 51,  -1),
            S(-28, 220),   S( 86, 109),   S( -9,  67),   S( 12, -80),   S( -9, -161),  S(-36, -156),  S( 15, -79),   S(110, -47),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13,  23),   S(-17,  16),   S(-14,  18),   S( -7,  34),   S(-26,  59),   S(  6,  38),   S( -8,  12),   S(-14,  -1),
            S( -4,  11),   S( -1,  11),   S(-18,  17),   S(-15,  20),   S( -4,  24),   S(  0,  12),   S(-10,   0),   S(-34,   2),
            S( 14,  36),   S( -8,  53),   S(  2,  21),   S( -2,  11),   S( 13,   4),   S( 19,   0),   S( -6,  -5),   S( -7,  -5),
            S( 22,  80),   S( -2,  94),   S(  6,  63),   S( 14,  33),   S( 29,   6),   S( 36, -22),   S( 22,  12),   S( 41, -14),
            S( -3, 152),   S(-26, 171),   S(-36, 166),   S(-15, 121),   S( 41,  49),   S( 91, -13),   S( 87, -10),   S( 91, -24),
            S( 59, 149),   S( 32, 211),   S(  2, 259),   S( -1, 189),   S( -8,  73),   S( -7, -141),  S(-80, -177),  S(-171, -153),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 60, -10),   S( 18,   4),   S(-17,  -2),   S(  8, -23),   S( -7,  -2),   S(-24,  -3),   S( -8,  -8),   S(-47,  29),
            S( 57, -28),   S( 18,  -3),   S(  5, -13),   S(-23,  -9),   S(-44, -15),   S(  1, -33),   S(-50, -12),   S(-40,  -1),
            S( 99, -39),   S(127, -49),   S( 23, -26),   S(-15, -19),   S(-81,  13),   S(-29,  11),   S(-35,  14),   S(-29,  17),
            S(-56, -77),   S( 40, -109),  S( 55, -32),   S(-18,   7),   S(-13,  11),   S(-48,  49),   S(-15,  50),   S(  9,  33),
            S( 66, -108),  S(-31, -122),  S( 14, -87),   S( 32,  16),   S( 75,  74),   S( 29,  53),   S( 13,  63),   S(  1,  74),
            S( 30, -33),   S(  1, -56),   S( 20, -78),   S( 14,  19),   S( 45,  61),   S( 69, 127),   S( 53, 133),   S( 28,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  30),   S(-18,  20),   S( 28,  -7),   S( 48,   3),   S( 60,  -1),   S( 22,  -9),   S(-34,   4),   S(-79,  44),
            S(-77,  19),   S(  3,   5),   S( 14, -12),   S(  0,   8),   S( 14,  -1),   S(  0, -12),   S(-51,  -5),   S(-90,  24),
            S(  0,   8),   S( 19,  12),   S( 69,  -1),   S(  7,  27),   S(  2,  23),   S(-20,  10),   S(-29,   9),   S(-61,  34),
            S( 65,  -4),   S( 77, -28),   S(  6, -51),   S( -2, -21),   S(  4, -15),   S(  5,   5),   S( 59,   0),   S( 10,  29),
            S( 82, -18),   S( 32, -49),   S( 70, -91),   S( 14, -61),   S( 70, -46),   S( 48,   9),   S( 55,  22),   S(-11,  80),
            S( 86,  14),   S( 23,  -9),   S( -4, -67),   S( 18, -73),   S( -8, -73),   S( 73,   6),   S( 66,  97),   S( 62,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-76,  23),   S(-58,   5),   S(  3, -16),   S(-19,  19),   S( 30,   3),   S( 26,  -1),   S( 19,  -2),   S(  6,  17),
            S(-68,   4),   S(-37, -10),   S(-23,  -9),   S( 42, -10),   S( -6,  14),   S( 13, -11),   S(  2,  -7),   S( 12,  -6),
            S(-33,  16),   S(-41,  18),   S(  5,  -1),   S( 14,  11),   S( 38,  19),   S( 72, -13),   S( 62, -11),   S( 35,  -4),
            S(-29,  52),   S( -6,  27),   S( 35,   0),   S( 42,  -6),   S(  1, -36),   S( 23, -46),   S( 50, -31),   S(104, -30),
            S( 20,  68),   S( 31,  24),   S( 45,   1),   S(  5, -40),   S( -4, -62),   S(-11, -65),   S( 47, -56),   S(117, -21),
            S(113,  51),   S(109,  45),   S( 68,  -5),   S( 79, -76),   S( -2, -104),  S(  0, -94),   S(-23, -50),   S( 63,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,   3),   S(-60, -12),   S(  2, -31),   S(-36,  22),   S( 16,   2),   S( 55, -10),   S( 51, -27),   S( 59, -11),
            S(-36, -21),   S(-59, -14),   S(-53, -19),   S(-35,   3),   S(  1,  -8),   S( 53, -35),   S( 24, -16),   S( 47, -22),
            S(-47,  20),   S(-44,  14),   S(-31,   4),   S(-32,  -9),   S(  4,  -6),   S( 37, -24),   S( 78, -25),   S( 84, -27),
            S( -5,  35),   S(-60,  57),   S(-33,  46),   S( -9,  22),   S( 15, -13),   S( 64, -68),   S( 28, -67),   S( 27, -95),
            S(  5,  55),   S(-18, 101),   S( 41,  87),   S( 15,  71),   S(-10,  27),   S(-18, -59),   S(-16, -124),  S( -6, -94),
            S(131,  70),   S(117,  99),   S(132,  91),   S( 63,  93),   S( 58, -20),   S(  7, -93),   S(-20, -109),  S( 43, -161),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25, -14),   S( 25, -12),   S( 62,   8),   S( -4, -10),   S(-22, -68),   S( 45, -47),   S( 46, -70),   S( 45, -39),
            S( -9, -44),   S(-13,  -1),   S(-39, -76),   S(-59, -30),   S(-60, -38),   S( 32, -47),   S(  8, -57),   S( -1, -45),
            S(-30, -71),   S( 32, -58),   S(  4, -53),   S(-39, -51),   S(-57,   0),   S(-41, -18),   S(-14, -17),   S(-61,   4),
            S( -3, -12),   S(-38, -31),   S( 37, -15),   S( 19,  -7),   S(  4,  -3),   S( -1,  25),   S(  2,  19),   S(  7,  21),
            S(  3,  31),   S(  1, -16),   S( 17,  41),   S( 32,  53),   S( 55, 122),   S( 27,  89),   S( 22,  71),   S(-11, 102),
            S( 16,  58),   S( 15,  55),   S( 29,  58),   S( 43,  83),   S( 41,  88),   S( 49, 141),   S( 49, 112),   S( 17,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 40, -10),   S( 53, -11),   S( 44,  -9),   S(  3,  14),   S(  5,   9),   S( 63, -32),   S( 77, -58),   S( -7, -29),
            S( -9, -54),   S(-59, -40),   S(-11, -45),   S(-29, -28),   S(-15, -30),   S(-45, -25),   S( -4, -41),   S( 20, -47),
            S(-82, -15),   S(-16, -59),   S(-13, -79),   S(-35, -35),   S( -3, -37),   S( 21, -50),   S(-39, -27),   S(-30,  -9),
            S(-32,   1),   S(-60, -54),   S(  0, -63),   S(-13, -31),   S( 32, -45),   S(  7, -19),   S(-29,   6),   S( 30,   1),
            S( -1,  22),   S(-13, -32),   S(  9, -17),   S( 26,   2),   S(  4,  39),   S( 38,  46),   S( 15,  57),   S(-14,  54),
            S( -5,  41),   S( 19,  38),   S( 13,  17),   S( 24,   9),   S( 34,  97),   S(  7,  50),   S( 18,  71),   S( -1,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -55),   S(-26, -47),   S(-19, -15),   S( -5, -27),   S( 55, -16),   S(192, -38),   S( 69, -25),   S( 80, -31),
            S(-47, -55),   S(-67, -56),   S( 31, -67),   S( 39, -34),   S( 20, -31),   S( 42, -39),   S( 27, -52),   S( 29, -43),
            S(-51, -28),   S(-51, -44),   S(-15, -46),   S( 20, -44),   S( -4, -45),   S(-17, -68),   S( -6, -69),   S( 25, -49),
            S(-25,  -9),   S( -2, -10),   S( -4, -28),   S(-31, -52),   S(-41, -30),   S(-26, -37),   S(-28, -51),   S(-14, -24),
            S( -3,   5),   S( 27,  17),   S( 31,  13),   S(  1, -32),   S( 30,  -3),   S( 28,  31),   S(-26, -12),   S( 22,   2),
            S(-23,  18),   S( -6,  31),   S( 33,  40),   S( 26,  43),   S( 30,  33),   S(  4,  15),   S( -5,  18),   S( 23,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -34),   S( -5, -38),   S( 12, -47),   S(  2,  -2),   S( -1, -33),   S(112, -12),   S( 79, -38),   S( 98, -20),
            S(-47, -56),   S(-50, -75),   S(-13, -69),   S(  1, -53),   S( -4, -37),   S(-12, -44),   S(  7, -37),   S( -1, -52),
            S(-40, -21),   S(-31, -23),   S(-32, -24),   S( 32, -39),   S(-46, -38),   S( 19, -52),   S(-69, -86),   S(  4, -64),
            S(-26,  16),   S(-14,  26),   S( 48,  17),   S( 46,   0),   S( 10, -13),   S(-20, -48),   S(-50, -47),   S(-32, -33),
            S( 10,  12),   S(-11,  52),   S( 12,  84),   S( 26,  62),   S( 31,  15),   S(-16,  -2),   S( 18,   8),   S( -2,  -2),
            S( 25,  54),   S( 29,  59),   S( 30,  60),   S( 43,  89),   S( 17,  37),   S( 36,  80),   S( 34,  64),   S( 32,  49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -91),   S( -9, -10),   S(-18, -12),   S( -2,   5),   S(  3,  -8),   S(-18, -55),   S(  6, -21),   S( 26,  -8),
            S(-13, -46),   S(-16,  -4),   S(-41, -71),   S(-35, -44),   S(-56, -69),   S(-18, -30),   S(-36, -39),   S(-14, -44),
            S(-37, -20),   S(  6, -42),   S(-10, -90),   S(-23, -81),   S(-18, -59),   S(-24,   1),   S(-38,   1),   S(-50, -26),
            S(-27, -23),   S(  1,  16),   S( -9,  -6),   S(  0,  22),   S( 20,  51),   S(  0,  70),   S( -3,  71),   S(-42,  43),
            S(  6,  11),   S( -3, -14),   S(  8,   9),   S( 18,  55),   S( 43, 105),   S( 32,  94),   S(  5, 133),   S(  9, 116),
            S( 17,  35),   S(  3,  28),   S( 14,  46),   S( 11,  26),   S( 35, 117),   S( 29, 130),   S(-25,  16),   S(-22,  48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -72),   S(-31, -83),   S(  8,  -2),   S( -5, -37),   S(  5,  -1),   S(-38, -43),   S( -2, -57),   S( -7, -45),
            S(-41, -51),   S(-23, -56),   S(-32, -64),   S(  8, -15),   S(-45, -51),   S(-42, -40),   S(-36, -68),   S(-31, -56),
            S(-22, -23),   S(-26, -49),   S( -9, -52),   S( -1, -46),   S( -6, -45),   S( -8, -56),   S(-26, -34),   S(-27, -26),
            S(-23,  14),   S( -4, -17),   S(  6, -37),   S(  4,  19),   S( 14,  63),   S(-42,  36),   S(-32,   8),   S(-18,  20),
            S( -4,  47),   S( 14,  48),   S(  5,  -1),   S( 23,  56),   S( 42,  98),   S( 20,  89),   S( 25,  99),   S(-12,  62),
            S(  6,  60),   S( 22,  61),   S( -1,   0),   S( 12,   9),   S( 19,  96),   S( 18,  81),   S(  4,  64),   S(  9, 113),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -69),   S( -2, -78),   S(  0, -42),   S( -5,  -3),   S(-16, -49),   S( -4,   1),   S(  0, -38),   S( -6, -39),
            S(-31, -75),   S(-59, -79),   S(-36, -76),   S(-29, -77),   S(-22, -41),   S(-16, -44),   S(-31, -41),   S(-18, -51),
            S(-26, -59),   S(-12, -47),   S(-19, -49),   S( -5, -28),   S( -2, -23),   S(-18, -59),   S(-22, -55),   S(-51,   0),
            S(-18,  -4),   S(-38, -20),   S(  2,  22),   S( -9,   4),   S( 13, -42),   S( -1,  10),   S(  5, -28),   S( -2,  29),
            S( -4, -13),   S( 13,  22),   S(  8,  44),   S( -1,  -7),   S( 21,  56),   S( 18,  29),   S( 14,  75),   S( 17,  71),
            S( -2,  53),   S( 14,  37),   S( 31, 116),   S( 27,  99),   S( 16,  69),   S( 18,  77),   S( 24, 118),   S( 21, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -3, -20),   S(  9, -41),   S(-19, -27),   S(-13, -28),   S(-11, -39),   S(-40, -36),   S(-23, -18),   S(-18, -59),
            S(-26, -46),   S(-39, -64),   S(-30, -81),   S( -3, -44),   S( -8, -25),   S(-37, -26),   S(-35,  -6),   S(-31, -71),
            S(-57, -36),   S(-48, -56),   S(-52, -53),   S(  5, -40),   S(-18, -18),   S( 10, -25),   S(  5, -25),   S(-18, -11),
            S(-23,  26),   S(-44,  -3),   S(  2,  24),   S(  2,  25),   S( 11,  61),   S(  7,  15),   S(  4,   5),   S( -6,  38),
            S(  3,  99),   S(  6,  80),   S(  5,  74),   S( 22,  84),   S( 27, 124),   S(  8,  65),   S( 19,  67),   S( 13,  47),
            S(-27,  30),   S(  2,  46),   S( 11,  82),   S( 27,  82),   S( 22,  97),   S( 15,  64),   S( 18,  48),   S( 14,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-79, -42),   S(-54,  31),   S(-22, -24),   S(-12,  36),   S(-28, -12),   S(-30,  -2),   S(-14,   3),   S(-74, -52),
            S( 17, -50),   S(-25,   3),   S(-30,  -3),   S( -1,  -7),   S(-19,  25),   S(-22,  14),   S(-19, -44),   S(-28,  17),
            S(-31, -13),   S(  6,  -7),   S(-13,  29),   S( 24,  39),   S( -2,  13),   S(  5,  21),   S(-33,  37),   S(-35, -31),
            S( 22,  16),   S( 32,  58),   S(  8,  57),   S( 42,  21),   S( 10,  50),   S( 15,  23),   S( 41,  -3),   S(-15,  24),
            S( 62,  86),   S(-14,  70),   S( 52,  83),   S( 41,  46),   S( 67,  42),   S( 15,  43),   S( 11,  22),   S(  3,  18),
            S( 88, -26),   S(-26,  71),   S(100,  70),   S( 94,  10),   S( 54,  60),   S(-36,  77),   S( 30,   2),   S( 16,  27),
            S( 42,  12),   S( -5, -11),   S( 47,   2),   S( 53,  69),   S( 62,  30),   S(  4,  52),   S(-19,  25),   S(-31,  12),
            S(-95, -136),  S( -7,  -4),   S( 12,  -1),   S( 45,  49),   S( -4,  32),   S( 20,  42),   S(-25,  11),   S(  8,  19),

            /* knights: bucket 1 */
            S( 10,  -5),   S(-66,  22),   S(-30,  13),   S(-67,  54),   S(-30,  44),   S(-22, -24),   S(-30,   3),   S( -6,   3),
            S(-48,  29),   S(-54,  58),   S(-52,  52),   S(-14,  29),   S(-22,  39),   S(-17,  35),   S(-25,  18),   S(-29, -37),
            S(-29,  12),   S(-13,  20),   S(-27,  35),   S(-28,  70),   S(-24,  59),   S(-19,  34),   S(-39,  47),   S(-13,  19),
            S(-11,  84),   S( 40,  34),   S( -7,  55),   S(-10,  67),   S( -5,  55),   S(-16,  72),   S( -6,  39),   S(-27,  68),
            S( 35,  24),   S(  9,  15),   S( 16,  87),   S(  2,  75),   S( 34,  44),   S(  8,  60),   S(-12,  49),   S(-10,  73),
            S(-32,  39),   S( 53,  11),   S( 94,  33),   S(106,  34),   S( 51,  47),   S(-55,  81),   S( 33,  52),   S( -2,  61),
            S( 31, -18),   S( 31,  15),   S( 69, -19),   S( 48,  49),   S( 10,  26),   S( 40,   2),   S( 29,  75),   S( -8,  42),
            S(-176, -66),  S( 10, -13),   S(-39, -75),   S(-44, -28),   S( 15,  28),   S( 40,  50),   S( -9,  17),   S(-82,  24),

            /* knights: bucket 2 */
            S(-96,   5),   S(-40,  37),   S(-42,  24),   S(-35,  44),   S(-25,  33),   S(-69,  38),   S(-25,  14),   S(-30, -23),
            S(-36,  35),   S( 16,  25),   S(-29,  35),   S(-16,  35),   S(-30,  40),   S(-25,  19),   S(  9,  14),   S(-29,  12),
            S(-39,  53),   S(-28,  43),   S(-27,  36),   S(-29,  71),   S(-29,  72),   S(-28,  33),   S(-24,  21),   S(-12,   3),
            S(-13,  75),   S(-16,  62),   S(-26,  96),   S(-30,  98),   S(-34,  88),   S( -9,  68),   S(-18,  57),   S( -9,  54),
            S(-14,  84),   S(-15,  77),   S( -2,  85),   S( 13,  64),   S(-28,  94),   S(  5,  90),   S(-28,  79),   S( 16,  24),
            S(-22,  67),   S(-22,  67),   S(-35, 103),   S( 52,  32),   S( 57,  48),   S(116,  14),   S( 74,  29),   S(-34,  37),
            S( -7,  54),   S(-44,  73),   S( 46,  15),   S( 21,  15),   S( -9,  48),   S( 32, -19),   S( 31,  30),   S(  0,  11),
            S(-59,  39),   S( 30,  58),   S( 20,  63),   S(-43, -35),   S(-28, -13),   S(-61, -53),   S( 10, -36),   S(-142, -65),

            /* knights: bucket 3 */
            S(-62,  37),   S(-24, -17),   S(-17,  20),   S(-11,  30),   S( -7,  16),   S(-21,  16),   S(-21,   1),   S(-38, -21),
            S(-28,  -9),   S(-22,  41),   S( -4,  25),   S( -7,  31),   S(-12,  35),   S(  9,  17),   S( 24, -14),   S( 18, -24),
            S(-17,  29),   S(-23,  42),   S(-10,  56),   S(-12,  78),   S(  0,  76),   S(-12,  62),   S(  3,  40),   S(  2,   9),
            S( -2,  32),   S(  9,  66),   S(  7,  78),   S(-12,  96),   S(  0, 109),   S( 13,  93),   S( 29,  71),   S(  9,  64),
            S(-16,  82),   S( -3,  82),   S( 24,  88),   S( 24, 115),   S(  2, 109),   S( 23, 127),   S(-26, 136),   S( 43, 114),
            S(-18,  60),   S(  0,  83),   S( 21,  88),   S( 10,  99),   S( 56, 107),   S( 82, 132),   S( 55, 109),   S( -7, 137),
            S(-24,  82),   S(-23,  73),   S(-38, 101),   S( 43,  75),   S( 38,  89),   S( 74,  63),   S( 28,  22),   S( 82, -12),
            S(-184,  57),  S(-20, 111),   S(-68, 121),   S( 17,  96),   S( 47,  99),   S(-91, 101),   S(-39, -75),   S(-59, -105),

            /* knights: bucket 4 */
            S( 24,  52),   S(-23,   0),   S(-30,  21),   S(-28, -20),   S(-32,  -2),   S(-10, -31),   S( 19, -32),   S(-26, -43),
            S(  5, -19),   S( 16, -30),   S( -7,  12),   S(  0,   0),   S(-14, -15),   S(  2, -65),   S( 12,  35),   S(-24,  30),
            S( -6,   5),   S( 28,  -5),   S( 84,   7),   S( 79,  -6),   S( 41,   4),   S( 21, -19),   S(-20,  -8),   S(  1, -34),
            S(-51, -21),   S( 12,  -9),   S( 53, -12),   S( 62,  11),   S( 61,   3),   S(-10,  47),   S(-30,  43),   S(  2,  -4),
            S( 27, -49),   S(  7, -34),   S( 79,  18),   S( 24,  53),   S( 95, -15),   S( 25,  54),   S( 62, -21),   S(-27,  37),
            S( -8, -50),   S(-20, -45),   S( 39, -16),   S( 66,  21),   S(-12,  21),   S(  5,  62),   S(-16,  11),   S( 13,   6),
            S(-16, -37),   S(-33, -31),   S( 12,   4),   S( 15,  23),   S( 33,   1),   S(-16,  40),   S( 13,  54),   S(-44, -36),
            S(  5,   9),   S( -4, -22),   S( -3, -28),   S( 18,  17),   S( 18,  17),   S(-19,   2),   S(  5,  18),   S(-20, -17),

            /* knights: bucket 5 */
            S( 28,  30),   S( 52,   2),   S(-17,  13),   S(-37,  38),   S( 28,  25),   S(-10,  23),   S(  8,  32),   S(  8,   8),
            S( 24,  11),   S( 61,   2),   S( 53,  -9),   S(-14,  17),   S( 39, -17),   S( -4,   3),   S(  2,  21),   S(-72, -18),
            S(  4,   4),   S( 20,   2),   S( 27,  13),   S( 43,  10),   S(  9,  34),   S(  6,  21),   S(  4,  20),   S(-33,   8),
            S( 62, -12),   S( 45, -14),   S( 58,  12),   S( 79, -18),   S(104,  -9),   S( 74,  13),   S(-18,   9),   S( 28,  10),
            S( 51,   5),   S( 45,   4),   S(109, -13),   S(121,  -1),   S( 80,  -1),   S( 40,  24),   S( 40,   8),   S( 25,  -1),
            S( 12, -16),   S( 42, -29),   S(  0, -30),   S( 20,  27),   S( 31, -10),   S( 51,  -8),   S( -8,  15),   S( 37,  17),
            S( 15,  27),   S(-24, -26),   S( 12, -47),   S(-10,  -9),   S(-12, -43),   S( 16,   9),   S( 12,  55),   S(  9,  27),
            S(-21, -11),   S(-25, -65),   S(  6, -12),   S( -5, -14),   S( 11,  -5),   S( -1,  37),   S( 31,  71),   S(-30,   7),

            /* knights: bucket 6 */
            S( 21,  -1),   S(-26,   7),   S(  2,  -4),   S(-17,  17),   S(-11,  19),   S( 22,   7),   S(-54,  40),   S(-24,  21),
            S( 11, -40),   S( 35,  36),   S( 34, -12),   S( 38,   7),   S( 43,  16),   S( 39,   9),   S(-12,  40),   S(-33,  28),
            S(-11,  31),   S( 39,   3),   S( 57,   3),   S( 34,  31),   S( 70,  24),   S(-21,  36),   S( 43,  29),   S(-51,  38),
            S( 32,  31),   S( 70,   6),   S( 73,  11),   S(114,  -5),   S(105,  -3),   S(125,  -6),   S( 46,  -6),   S(-17,  50),
            S(-31,  60),   S( 73,   5),   S(124,  10),   S(126,   1),   S(108,  -9),   S(108,   3),   S(137, -13),   S( 29,  18),
            S(  9,  31),   S( 57,   4),   S( 45,  19),   S( 22,  21),   S( 52,  -7),   S( 70, -18),   S( 40,   3),   S( 24, -14),
            S( 19,  27),   S( 25,  24),   S( 36,  33),   S(  0,  -7),   S( 14,  24),   S( 49, -20),   S(-23,   3),   S( 20,  39),
            S(  5,   9),   S( -4,  22),   S( 20,  22),   S( 21,  23),   S( 32, -23),   S(  9,  14),   S( 13,  60),   S(-30, -58),

            /* knights: bucket 7 */
            S(-19, -65),   S(-21, -30),   S( 43, -21),   S(-13,  14),   S(  0,  -3),   S(-47,  19),   S( -5,   9),   S(-23,  20),
            S(-46, -23),   S( 22, -28),   S( -4,  -1),   S(-15,   6),   S(  5,  -1),   S( 46,  -7),   S( -2,  17),   S(-46,  26),
            S(-30, -32),   S(-33,   4),   S( 14,   0),   S( 56,   7),   S( 55,  19),   S( 57,  18),   S( 19,  14),   S( 13,  28),
            S(-56,  35),   S( 49,  -9),   S( 72, -11),   S( 92,  11),   S(113,   3),   S( 98,   2),   S( 53,  22),   S( 50,   0),
            S(  9,  23),   S( 25,   9),   S( 16,  35),   S(102,   0),   S(184, -21),   S(174, -24),   S(225,  -3),   S( 31,  21),
            S(-17,   6),   S( 51,  25),   S(  0,  36),   S( 44,  24),   S(118,  -2),   S(143, -14),   S( 48,  -7),   S(-10, -61),
            S(-17,  21),   S( -7,  28),   S(  6,  25),   S( 36,  25),   S( 30,  29),   S( 46,  30),   S(-18, -48),   S(-15, -38),
            S(-40, -65),   S(-25, -24),   S(  8,  30),   S(  0,  18),   S(  4,  10),   S( 17,  18),   S( -1, -19),   S(  3, -12),

            /* knights: bucket 8 */
            S(  2,  18),   S(  7,  -7),   S( -1,  -3),   S(-16, -62),   S(-14, -28),   S(-13, -54),   S(  4,   2),   S( -7, -45),
            S(-12, -54),   S( -8, -49),   S( -9, -47),   S(-15, -31),   S(-10,  16),   S(  5,  -1),   S( 11,  18),   S( -7, -32),
            S(-13, -71),   S(-18, -32),   S(  4, -84),   S( 15, -19),   S( 14, -31),   S( 30,  -7),   S(  9, -40),   S(  1, -27),
            S(-20, -80),   S(-16, -60),   S( 12, -15),   S( 20,   5),   S( -2, -48),   S( -7, -42),   S(-10, -27),   S(-13, -38),
            S(-13, -29),   S( -4, -63),   S(  6, -17),   S(  7, -39),   S( 14, -21),   S( -2, -11),   S(-18, -57),   S( -1, -11),
            S( -1,  -3),   S( 14,  -3),   S(  7, -14),   S(  5, -13),   S(  0, -15),   S(  2, -23),   S( -5,  -3),   S(-17, -54),
            S( -2,   4),   S( -3, -48),   S( -7, -27),   S( 11,  12),   S(  1, -29),   S( 17,  -8),   S(  5,   8),   S( -3,  -8),
            S(  0,  -2),   S( -5,  -6),   S( -1,   6),   S( 15,  17),   S(  1,  24),   S( -2,  10),   S( -6, -11),   S( -6, -11),

            /* knights: bucket 9 */
            S(-17, -84),   S( -7,  -7),   S(-20, -76),   S( -2, -55),   S(-15, -22),   S(  4, -12),   S(  1, -26),   S(  2, -21),
            S( -3, -15),   S(-29, -102),  S(-20, -111),  S( -5, -71),   S(-10, -72),   S(-25, -78),   S(  6, -37),   S(-19, -49),
            S( -6, -48),   S(-14, -52),   S( -6, -44),   S( 22, -75),   S( -8, -35),   S( 34, -21),   S( -4,   1),   S( -1,  -4),
            S(-19, -76),   S(-11, -62),   S(  4, -57),   S( -2, -47),   S(  5, -42),   S( 21,   3),   S(-17, -76),   S( -1, -27),
            S(  4,  -4),   S(  7,   2),   S(  4, -48),   S( 15, -32),   S( -2, -49),   S( 10, -16),   S( -1, -40),   S( -4, -42),
            S(-19, -61),   S(-11, -50),   S(-10, -37),   S( 15, -52),   S( 15, -23),   S( -7, -43),   S(  4, -17),   S(-15,  14),
            S(-12, -36),   S(  5,  12),   S(  2, -10),   S(-29, -55),   S( 21,  -5),   S(  7,   4),   S(-19,   1),   S( -2,   7),
            S( -1,  -3),   S(  3, -10),   S(  1, -13),   S( -6, -10),   S(-13, -40),   S( -4, -31),   S(  6,  10),   S( -1,   6),

            /* knights: bucket 10 */
            S( -5, -47),   S(-14, -59),   S( -4, -27),   S( -5, -18),   S(-13, -87),   S( 19, -53),   S( -3,   6),   S(  4,   4),
            S( -7, -43),   S(  9, -20),   S(-13, -50),   S(  9, -65),   S( 18, -50),   S(-13, -64),   S( -7, -28),   S( -4,  21),
            S(  4, -12),   S(  7, -48),   S( 14, -19),   S( 31, -53),   S( -4, -46),   S(  8, -41),   S( -7, -36),   S( -6, -18),
            S(  1, -45),   S( -3, -57),   S( 14, -36),   S( 24, -31),   S( 11, -43),   S( -5, -44),   S(-12, -75),   S( -9, -52),
            S(-25, -69),   S( -5, -46),   S(  5, -34),   S(  3, -43),   S( 16, -41),   S(  5, -65),   S( -6, -15),   S( 11, -22),
            S(  5,   3),   S(-12, -34),   S( -8, -16),   S(  2, -13),   S( -9, -27),   S(-21, -53),   S( -8, -55),   S( -2, -36),
            S(  5,  -2),   S( -5, -22),   S( -4, -43),   S( 16, -26),   S( -5, -11),   S(-13, -50),   S(-19, -37),   S(-15, -35),
            S(  2,   0),   S( -4, -32),   S(  2,  19),   S( -9, -34),   S(  6, -19),   S(-12, -36),   S(  7,  10),   S( -5,  -9),

            /* knights: bucket 11 */
            S( -7, -24),   S(-30, -61),   S(-13, -36),   S( 13, -35),   S(-23, -63),   S( -4,  -9),   S( -4, -13),   S( 11,  24),
            S(  5, -12),   S(-25, -59),   S( -1, -74),   S( 31, -36),   S( 31, -10),   S( -5, -57),   S(  1, -28),   S(-15, -40),
            S(  3, -26),   S(-18, -65),   S( 21, -35),   S( 29,  -6),   S(  9,   2),   S( 25, -10),   S(-13, -73),   S(-10, -33),
            S(-29, -39),   S( 28, -31),   S( 17, -21),   S( 32, -27),   S( 50, -20),   S( 11, -26),   S( 12, -19),   S( -1, -25),
            S(-12, -14),   S(  4, -54),   S(  2,   7),   S( 15, -36),   S( 34,  -3),   S( 16,  31),   S(-18, -86),   S(-17, -42),
            S( -9, -17),   S( 16, -41),   S( 33,  28),   S( 21,  17),   S( 25,  16),   S(  4, -18),   S(-14, -63),   S( -1,   0),
            S(-11, -42),   S( -9, -10),   S(  3,   0),   S( 12,  -7),   S( 13, -17),   S( 17, -35),   S(  5, -42),   S( -4, -12),
            S(  0, -14),   S( -6,  -5),   S(-11, -42),   S( -2,  18),   S( -6, -21),   S( -1, -28),   S(  5,  11),   S( -5, -14),

            /* knights: bucket 12 */
            S(-23, -79),   S( -7, -39),   S(  0, -14),   S(  6,  18),   S( -4,  -4),   S( -2, -22),   S(  0,   2),   S( -3, -14),
            S( -5, -38),   S(  1,   5),   S( -6, -14),   S( -5, -23),   S( -8, -47),   S(  1,  -4),   S( -1, -14),   S(  3,  -2),
            S( -3, -17),   S( -8, -39),   S(-10, -38),   S( -3, -60),   S( -1, -33),   S(  0,  -1),   S( -6, -26),   S(-12, -40),
            S(  5,  20),   S( -1, -49),   S(  5,   8),   S( 16,  -3),   S(  7, -40),   S(  4,   1),   S(  9,  12),   S( -2,   1),
            S( -9, -35),   S( -9, -48),   S( 12,  -3),   S( -5, -18),   S( 13,  38),   S(-10, -30),   S( -4, -12),   S( -2,  -5),
            S( -3,  -4),   S( -1, -17),   S(  1,  -2),   S(  7, -12),   S( -1, -22),   S(  2,  -7),   S( 12,   4),   S( -4, -15),
            S( -5, -24),   S( -2,  -1),   S( -4,   1),   S(  0,  12),   S( -3,  -8),   S(-15, -26),   S(-14, -38),   S( -5, -12),
            S(  0,   0),   S(  4,  23),   S( -1,  -6),   S(  0,  -8),   S(  5,   9),   S(  1,  12),   S(  0,  -3),   S(  2,   4),

            /* knights: bucket 13 */
            S(  0,   0),   S( -4,  -7),   S( -8, -46),   S( -6, -35),   S(-14, -53),   S( -3, -13),   S(-10, -25),   S(  4,  14),
            S( -4, -22),   S( -5, -18),   S( -1,  -9),   S(-11, -50),   S( -2, -13),   S( -4, -39),   S( -2, -13),   S( -2, -16),
            S(  3,  -3),   S(  1,   7),   S( -3, -30),   S(  4, -16),   S(  2,  13),   S( -2,   6),   S(  4, -17),   S( -2,  -4),
            S( -8, -28),   S( -1,  -7),   S( -2, -28),   S( 13,  22),   S(  2, -22),   S(  6, -11),   S( -4, -21),   S( 14,  12),
            S( -1,   4),   S( -1, -38),   S(-10, -65),   S(  8, -18),   S( -2, -42),   S(  6, -29),   S(  0, -20),   S(  0,  -9),
            S( -2, -10),   S( -6, -24),   S(-14, -33),   S(  3, -35),   S(  3,  18),   S( -8, -43),   S(  3,   4),   S(  0,   3),
            S(  2,   4),   S(  2,  -6),   S(-10, -33),   S( -6, -28),   S( -1,   7),   S(  2,  18),   S( -4,  -8),   S(  1,   3),
            S(  3,   9),   S( -1,   0),   S( -1,  -5),   S(  0,  -2),   S(  1,   3),   S(  0,  -2),   S(  2,   6),   S(  0,   0),

            /* knights: bucket 14 */
            S(  0,   2),   S(  1,   9),   S(  5,   3),   S( -6, -15),   S(  3, -10),   S( -3, -11),   S( -3,  -7),   S( -3, -18),
            S( -2,  -9),   S( -8, -38),   S(  4,  -7),   S(  7, -25),   S( -1, -23),   S(  1,  -5),   S(-11, -39),   S(  5,  44),
            S( -5, -23),   S( -9, -52),   S( 13,  11),   S( -9, -53),   S( -4, -49),   S( -3, -10),   S( -2, -19),   S(  4,   4),
            S( -3, -14),   S( -7, -44),   S(-16, -47),   S( -4, -12),   S(  8,   0),   S(  0, -33),   S( -5, -13),   S( -1,   4),
            S(  8,  15),   S(-13, -38),   S( -1, -34),   S( -8, -16),   S( -5,  10),   S( -4, -28),   S( -2, -12),   S( -3, -23),
            S( -3,  -9),   S(  0,   3),   S(  5,  63),   S( 10,   3),   S(  3,  -1),   S( -3,   3),   S( -1,  17),   S( -7, -24),
            S( -2,  -7),   S( -7, -26),   S(  9,  14),   S(  9,  41),   S(  2,  27),   S( -9, -24),   S(  3,  -2),   S(  2,   6),
            S(  1,   0),   S(  1,   2),   S(  1,   2),   S( -1, -11),   S( -2,  -6),   S(  0,   0),   S(  1,   1),   S( -1,   0),

            /* knights: bucket 15 */
            S( -7, -35),   S(  2,   7),   S(  6,  25),   S( -5,  -7),   S(-11, -38),   S( -8, -40),   S( -3, -37),   S( -3, -24),
            S(  0,   1),   S(  9,   7),   S( -1, -20),   S(  8,  15),   S(  2, -13),   S(-14, -66),   S( -7, -30),   S(  4,   6),
            S( -1,  -7),   S( -5, -29),   S(  0, -27),   S( 17, -20),   S(-12, -61),   S( -7, -50),   S( -3, -31),   S( -5, -19),
            S(  3,   5),   S( -2,   4),   S(  8,  13),   S( -1,   9),   S(  7, -29),   S(  4,   1),   S(  3,   0),   S(  1,   9),
            S( -4, -30),   S( 10,  36),   S( -9, -40),   S(  6,  33),   S( 18,  40),   S( -5,  -4),   S( -3, -33),   S(  1,   0),
            S(  0,   0),   S( -7, -13),   S(  2,   1),   S(-11, -30),   S(-12, -39),   S(  7,  44),   S( -3,   9),   S(  4,  17),
            S( -6, -18),   S( -6, -17),   S(  4,  21),   S(  8,  19),   S(  4,   5),   S(  3,  10),   S( -2,  -8),   S( -1,  -2),
            S(  2,   7),   S( -1,  -9),   S(  0,   3),   S( -4, -17),   S(  2,   7),   S(  0,   4),   S(  1,   6),   S(  0,   3),

            /* bishops: bucket 0 */
            S( 41, -20),   S(-38,  57),   S(-16,   6),   S(-16,  -1),   S( 10,  -9),   S(  3,  -7),   S(120, -92),   S( 20, -14),
            S(-45,  14),   S( 14, -34),   S(-14,  16),   S(  8,   0),   S( 16,  -9),   S( 81, -27),   S( 40,  37),   S( 59, -25),
            S( 24,   1),   S( 15,  16),   S( 18,  -9),   S( 19,  -5),   S( 44, -12),   S( 47,  38),   S( 59, -21),   S( 20,  -6),
            S( 27, -36),   S( 40, -37),   S( 25,  -2),   S( 82, -27),   S( 75,  21),   S( 52,  14),   S( 22, -24),   S( 22,   9),
            S( 44, -15),   S( 34, -27),   S( 82, -18),   S(104, -18),   S(149, -32),   S(  3,  15),   S( 63,  -1),   S(  9,  -9),
            S( 17,  32),   S(117,  -7),   S( 73,  -2),   S( 70,  -3),   S( 36,   8),   S( 72,  -1),   S( 65,   5),   S( -1,  -5),
            S(-44, -98),   S(119,  12),   S( 93,  85),   S(  8, -17),   S( 15, -23),   S( 50, -18),   S( -2,  35),   S(  4,  15),
            S(-29, -41),   S(-25, -10),   S( 10, -40),   S(-33, -23),   S(-13, -39),   S(-11,   6),   S( -8,  -3),   S(-21, -18),

            /* bishops: bucket 1 */
            S(-28, -22),   S( 34, -34),   S(-13,  17),   S(  2, -12),   S( -1,   4),   S(  9,  -5),   S( 30, -17),   S( 34, -29),
            S( 21, -52),   S( -5, -14),   S( -9,  -5),   S( -5,  -2),   S( 36, -23),   S(  5,  -6),   S( 57, -24),   S( 13, -26),
            S(-16, -20),   S( 29, -18),   S(  7, -14),   S( 25, -11),   S( 12, -15),   S( 40, -10),   S( 22, -23),   S( 63, -18),
            S( 40, -42),   S( 33, -12),   S( 28,  -9),   S( 21,  -2),   S( 57, -15),   S( 13,  -2),   S( 67, -29),   S(  9,  -7),
            S( 28, -52),   S( 69, -21),   S( 22,  -1),   S(109, -21),   S( 78, -13),   S( 93, -29),   S( 21,  -7),   S( 31, -13),
            S( 64, -52),   S( 39, -16),   S( 73, -21),   S( 63, -31),   S(129, -51),   S(  2,  -4),   S( 14,  10),   S(-14,  -9),
            S( -5, -80),   S(-18, -24),   S( -8, -36),   S( -6,  16),   S( 15,   9),   S(-20,   2),   S( 32, -17),   S( -2,   5),
            S(-52, -40),   S(-44, -13),   S(-10, -39),   S(-53,  -4),   S(-10, -26),   S( 60, -37),   S( 29,  -4),   S(-41, -38),

            /* bishops: bucket 2 */
            S( 13, -34),   S( -9, -20),   S(  5,   8),   S( -7,  -3),   S( 32,  -2),   S(-21,  14),   S( 22, -13),   S( -8,  -2),
            S( 25, -23),   S( 17, -17),   S(-11,   3),   S( 14,   8),   S( -7,  12),   S( 18,  -2),   S(  3,  -4),   S( 24, -54),
            S( 40,   0),   S( 18,  -5),   S( 13,   3),   S( -2,   2),   S(  3,  20),   S(-10,  -4),   S( -1, -11),   S(-20,  -6),
            S( -3,  -8),   S( 63, -12),   S(  8,   1),   S( 41,  17),   S(  8,  17),   S( -4,  18),   S(-23,   6),   S(  6,  -4),
            S( 11,   9),   S( 16,  13),   S( 78, -11),   S( 21,  13),   S( 38,  24),   S( 31,  13),   S( 17,  25),   S( 34, -11),
            S( -7,  19),   S(  0,  26),   S(  9,  -6),   S( 79, -15),   S( 62, -17),   S( 99,  11),   S( 80, -18),   S( 14, -39),
            S(-10,  17),   S( 27, -17),   S(  1,  11),   S( 39,   7),   S(-61, -12),   S(-25, -22),   S(-32,  36),   S(-29, -31),
            S(-83, -17),   S(-47,  26),   S( 25,  -7),   S(-46,  14),   S(-28, -27),   S(-38,   4),   S( 18, -30),   S(-37, -37),

            /* bishops: bucket 3 */
            S( 35,  10),   S( 42, -31),   S( 10,  -1),   S( 12,   9),   S( 10,  32),   S( -5,  54),   S( -9,  70),   S( -7,   4),
            S( 46,   0),   S( 26,   6),   S( 23,   4),   S( 16,  15),   S( 25,  23),   S( 22,  36),   S( 14,  13),   S( 28, -15),
            S( -3,  27),   S( 34,  47),   S( 25,  42),   S( 26,  33),   S( 15,  46),   S( 17,  33),   S( 13,  20),   S(  2,  32),
            S(-14,  21),   S( 21,  29),   S( 36,  48),   S( 42,  48),   S( 44,  39),   S( 27,  14),   S( 35,  -5),   S( 40, -30),
            S( 15,  23),   S( 24,  43),   S( 13,  51),   S( 56,  58),   S( 59,  55),   S( 49,  28),   S( 22,  33),   S(  2,  28),
            S( 22,  21),   S( 39,  58),   S(  5,  46),   S( 36,  47),   S( 53,  37),   S( 59,  66),   S( 34,  75),   S(  8, 102),
            S(-25,  61),   S( 33,  45),   S( 19,  36),   S( 16,  39),   S( 30,  41),   S( 41,  76),   S(-16,  22),   S( 10, -15),
            S(-25,  43),   S(-21,  42),   S(-78,  56),   S(-42,  60),   S(-28,  47),   S(-149,  71),  S( -6,   6),   S( -3,   9),

            /* bishops: bucket 4 */
            S(-26,  -4),   S(-31,  -6),   S( -7,  -3),   S(-31, -20),   S(-28, -25),   S(-31, -34),   S( -9,   1),   S(-20, -77),
            S(-19,  12),   S(-16, -12),   S( 72, -29),   S(-11, -17),   S(-72,  15),   S( -4, -46),   S( -6, -26),   S(-46, -28),
            S( -5,  10),   S(-28,  -7),   S( 50, -20),   S( 13, -31),   S( 42, -23),   S(-23,   5),   S( -2, -32),   S(-38,   1),
            S( 22, -45),   S( 39,  -7),   S( 51, -28),   S( 33, -10),   S(  4,  14),   S( 68, -24),   S(-36, -14),   S(-26, -26),
            S(  4, -23),   S(  0, -66),   S( 46, -49),   S( 76, -14),   S( -9,  -3),   S( 42,  11),   S( 24,  29),   S(-45, -18),
            S(-70, -78),   S(-19, -44),   S( 17, -38),   S(  4,  -9),   S(  0,   0),   S( 42,  17),   S( -1,   9),   S( 15,  48),
            S( -3,  -5),   S(  5, -10),   S( -9, -53),   S(-16, -49),   S(-39,   2),   S(  4,  13),   S( -4,   8),   S( 20,   9),
            S(  0,  -9),   S(  8, -45),   S(-18, -39),   S( -9, -35),   S(  1, -33),   S( -4,  -6),   S( -5,  48),   S( -2,   2),

            /* bishops: bucket 5 */
            S(-51,  -4),   S(  7, -32),   S(-72,  19),   S(-51,  14),   S(  7, -10),   S(-69,  -1),   S(-46,   6),   S(-38, -30),
            S(-45,  -7),   S(-51,  19),   S( 45, -23),   S(-22,   3),   S(-39,  10),   S( 22,  -9),   S(-40,  -5),   S( -6, -38),
            S(  3,  11),   S(-36,  -3),   S( -2,  -1),   S(-22,  -2),   S( -8,   8),   S(-54,   9),   S(  0,  -3),   S( -9,  -1),
            S( 24, -26),   S( -7,  -5),   S( 17, -13),   S( 81,  -9),   S( -7,   4),   S( 23,   1),   S(-65,  12),   S(-13,  -1),
            S( 22, -36),   S(  4,  -7),   S(-17,  -8),   S(  2, -28),   S( 10,  -8),   S( 37, -12),   S( 36,  -2),   S(-75,  -2),
            S(-29,   3),   S( 20, -11),   S( 28, -35),   S(-38, -28),   S(-17, -14),   S( 12, -15),   S( -3,  -1),   S( -4,  13),
            S(-19, -28),   S( -1, -28),   S(-59,   0),   S(-13,   2),   S( 20, -31),   S(  5,  -9),   S( 28,  23),   S(-20,  -5),
            S(-20, -27),   S(-26, -38),   S( -7, -30),   S( 11, -24),   S(-46,   8),   S(-15,  11),   S(-30,  -9),   S( 11,  15),

            /* bishops: bucket 6 */
            S(-67,  -7),   S(-18,  -2),   S(-56,  12),   S( 14,  -5),   S(-29,  20),   S(-23,   6),   S(-59,  24),   S(-69,   2),
            S(-39,  -4),   S(-32, -19),   S(-40,  15),   S(-15,   0),   S(-51,  18),   S(-64,  27),   S(-55,  22),   S(-44,  -6),
            S( 28, -15),   S(-34,  15),   S( -8,  -7),   S(-42,  21),   S(-20,  23),   S(-19,  -3),   S(-47,   3),   S(  5,   5),
            S(-21,  -4),   S(-43,   9),   S(-40,  19),   S( 70,  -4),   S( 35,  14),   S( 40,   1),   S( -2, -18),   S(  7,  -5),
            S(-62,  17),   S( 11,   1),   S( 35, -11),   S( 68, -13),   S( 13, -21),   S(  3, -11),   S( 63, -25),   S(-27, -18),
            S(-49,  27),   S( 22,  -1),   S( -5,  -5),   S( 26,   0),   S(-10,  -5),   S( -2, -14),   S(  8,   5),   S(-17, -22),
            S(-45,  12),   S(-56,  27),   S( 29, -19),   S( 10,  -6),   S(-28,  13),   S( -6,  -3),   S(  1, -13),   S(-43,   3),
            S(-28,   7),   S(-13,  -2),   S(-23,  10),   S( -8,  -1),   S( -6,   4),   S( 10, -20),   S(-31,  13),   S(  0, -22),

            /* bishops: bucket 7 */
            S( 31, -38),   S(-55, -42),   S(-24, -46),   S(-30,   0),   S(-45,   2),   S(-41,  -5),   S(-77, -26),   S( -2,  -1),
            S( 25, -77),   S( -2, -37),   S(  0, -24),   S( -9, -28),   S(-28,   6),   S(-56,   7),   S(-17, -38),   S(-27,  11),
            S(-37, -36),   S(-45,  -1),   S(-13,  -1),   S( 33, -22),   S( 12, -26),   S( 26, -24),   S(-63,  -3),   S(-87,  37),
            S( 13,  -8),   S(-21, -10),   S( -3,   4),   S(  1,   1),   S( 83,  -4),   S( -7,  -5),   S( 79, -40),   S(  2, -41),
            S(-13,  -4),   S( 26,  -7),   S(  1,  -9),   S( 32,  -7),   S( 63, -14),   S( 66, -36),   S(-45, -18),   S(-36,   3),
            S(-68,  17),   S(  2,  13),   S( 24,  -7),   S(-53,  13),   S(-15,   6),   S( 66, -27),   S( 22, -25),   S(-39, -70),
            S(-43,   7),   S(-13,  26),   S(-34,   8),   S(  1,   5),   S( 12, -26),   S(  0, -37),   S( -9,   5),   S(-18, -10),
            S(-40, -32),   S(-50,   4),   S( -2,  -1),   S( 22, -19),   S( 13, -10),   S( 19, -14),   S( 20, -49),   S(-16, -20),

            /* bishops: bucket 8 */
            S( 23,  88),   S(-18, -27),   S( -8,  -6),   S(-20,  16),   S( -7,  10),   S(  2, -47),   S(-16, -43),   S(-12, -50),
            S(-15,  11),   S( -3,  31),   S(  5,  31),   S( 18,  13),   S( 15,  -3),   S( 16,   5),   S(-37, -66),   S(-13, -31),
            S(-13, -47),   S(-17, -65),   S( 21,  46),   S( 27, -17),   S( 34,  -3),   S( 28,   4),   S(-25, -32),   S(-22, -36),
            S( -3,  -3),   S( 21,  70),   S( -2,  -9),   S( 15,  61),   S( 13,  26),   S( 20,  33),   S(-17, -14),   S( -7, -21),
            S( 10,  74),   S( 30,  97),   S(  7,  48),   S(  9,   0),   S( -3,  44),   S(-26,  21),   S(  4, -28),   S(  0,  25),
            S(-13, -25),   S( -1,  31),   S( -4,  31),   S(  2,  14),   S( 15,  28),   S(  8,  67),   S( 12,  47),   S(  7,  27),
            S(  2,  36),   S(-13, -32),   S( 14,  49),   S(-10,  37),   S( -1,  23),   S( 10,  52),   S( 10,  69),   S(-13, -10),
            S( -3,   9),   S(  9,  16),   S( -6, -13),   S( -6,   5),   S(  8,  47),   S(  6,  41),   S(-11,  27),   S( 21,  89),

            /* bishops: bucket 9 */
            S( 14,  49),   S(-19,   8),   S(-24,  16),   S(-48, -32),   S(-18, -17),   S( -1, -11),   S(-10, -24),   S(-11, -14),
            S(-12, -20),   S(  0,  10),   S(-21,   8),   S(-11, -28),   S( -2, -26),   S( 11, -32),   S(-17, -32),   S(-18, -79),
            S( -5, -26),   S(  3, -10),   S(  2,   5),   S( -6,  23),   S( -4,  11),   S(  4, -20),   S(  8,   1),   S(-17, -27),
            S( -3,  21),   S(-17,   9),   S(-11,  24),   S( -2,  23),   S(-29,  36),   S(  3,  14),   S( -3,   8),   S(  0, -18),
            S( 11,  38),   S(-15,  21),   S( 18,  12),   S(-14, -10),   S( -5,  17),   S(-23,  18),   S(  9,  19),   S(-14, -27),
            S( -7,  27),   S(-24,  -2),   S(  2,  25),   S(-12,  13),   S(-31,   9),   S(-21,  13),   S(  0,  31),   S(-20,  15),
            S(  3,  23),   S(-14,  28),   S( -6,  -5),   S( -9,  18),   S( -6,  -1),   S(  3,  18),   S(  1,  41),   S( -2,  57),
            S(  3,  34),   S( -4,  -9),   S(  0,   7),   S( -1,  10),   S( -4,  19),   S( -5,  31),   S( -8,  19),   S( 16,  78),

            /* bishops: bucket 10 */
            S( -5,  -2),   S( -6,  38),   S(-29, -26),   S(-20, -26),   S(-48, -35),   S(-44, -53),   S( -7,  32),   S( -6, -12),
            S(-15,  -8),   S(  7, -30),   S(-16, -29),   S(-28, -42),   S(-25, -17),   S(-44, -37),   S(-37, -33),   S( -8,  20),
            S(-16, -52),   S( -5, -23),   S(-19, -14),   S( -5,  21),   S( -6, -29),   S( -5, -12),   S(-20, -13),   S( -9, -45),
            S(-15, -21),   S(-10, -14),   S(-38, -14),   S(  5,   0),   S( -1,   5),   S(  4,   7),   S( 14,  36),   S(-27,  -8),
            S( -6,   3),   S(-49,  32),   S(  4,  -4),   S( 13,   4),   S( 19,   5),   S(  8,  39),   S(-15,  17),   S(  3,   1),
            S(-20,  29),   S(-11,  24),   S(-19,   5),   S(-32,  20),   S(  0, -18),   S(-11,   5),   S(-19,  26),   S(  2,  -2),
            S( -8,  23),   S(-16,  25),   S(-22,  -3),   S( -8,  26),   S(-10,  -3),   S(-38, -31),   S( -7,  -4),   S( 11,  52),
            S( -8,  31),   S( -3,  50),   S( -6,  16),   S(-17,   1),   S(-16,  27),   S( -4,  -3),   S( -3,   7),   S( -6,  -9),

            /* bishops: bucket 11 */
            S(  4,  -9),   S(-18, -19),   S(-16, -19),   S(-29,   0),   S(-22, -44),   S( 13, -25),   S(-32, -25),   S(-19,  35),
            S( -3,  25),   S( 12, -23),   S(  3, -10),   S( -1, -21),   S(  2, -14),   S(-47, -12),   S( -1,   1),   S(  2,  23),
            S(-18, -51),   S(  7,  -2),   S( 21,   1),   S(-16, -17),   S(-17, -20),   S( 12,  16),   S( 15, -14),   S( -8, -33),
            S(  1,   4),   S(-18, -44),   S( 29,   3),   S(-20, -31),   S(-12,   2),   S( 27,  39),   S( 13,  41),   S(-14, -53),
            S(-12,   9),   S(-27,  -1),   S(-18,  25),   S(-19,  56),   S( -9,  17),   S(  3,  17),   S(  2,  32),   S(  6,  45),
            S(-13,  19),   S(-19,  -3),   S(-24,  36),   S(-14,  19),   S(-20,  61),   S(-13,  22),   S(  1,   5),   S(-15,  -3),
            S(-15,   8),   S( -3,  98),   S(-10,  48),   S( 13,  45),   S(  8,  13),   S(  4,  16),   S( -6, -36),   S( -6,   2),
            S( 12,  91),   S(-15,  30),   S( 21,  74),   S(  2,  36),   S(  6,  57),   S(-19,  -4),   S( -4,  17),   S(  3,  11),

            /* bishops: bucket 12 */
            S( -4, -10),   S(  1,  -8),   S( -5, -16),   S(-10,  -5),   S(-17, -26),   S( -8, -19),   S( -3,  -5),   S( -3, -14),
            S(  1, -14),   S(  9,  26),   S( -4,  -7),   S( 10,   5),   S(  1,  -8),   S( -2,  -5),   S( -7, -13),   S( -3, -18),
            S( 12,  56),   S(  8,  35),   S( 16,  61),   S( 14,  12),   S(  7,  28),   S( -4, -27),   S(  9,   5),   S(-11, -19),
            S(  5,  65),   S( 10,  62),   S(  8,  33),   S(  9,  23),   S(  9,   8),   S( 20,  25),   S(  5,   3),   S(  8,  13),
            S(  8,   3),   S(  6,  15),   S(  9,  42),   S( 10,  25),   S( 21,  71),   S( 14,  43),   S(  2,  -9),   S(  6,   9),
            S( -2,  -8),   S(-10, -25),   S(  2,  13),   S( -4,  -4),   S( 13,  64),   S( 10,  31),   S( -9, -33),   S( -4, -20),
            S( -4,  -6),   S(  2,  16),   S(  0,  -7),   S(  8,  10),   S( 10,  35),   S( 12,  53),   S(  6,  24),   S(  5,  44),
            S(  3,  19),   S(  1,   8),   S(  1,  13),   S( -3, -15),   S( -2, -10),   S(  1,  -2),   S(  4,  41),   S(  4,  25),

            /* bishops: bucket 13 */
            S(-10, -38),   S( -6, -10),   S( -5, -20),   S(-10, -14),   S(  3,  17),   S(-14, -41),   S(-12, -37),   S( -5, -26),
            S( -8,   1),   S(  0,  15),   S(  0,  15),   S(  4,  36),   S(-18, -28),   S(  4,  15),   S(-12, -57),   S(  2,   0),
            S(  1,  23),   S( 17,  79),   S( 13,  51),   S( 15,  22),   S( -4,  10),   S(  8,  36),   S(-10,  -7),   S( -1,  -7),
            S( 16,  83),   S(  5,  64),   S( 11,  49),   S(-13,  17),   S( 18,  38),   S( -1,   2),   S(  0,  30),   S(  5,  21),
            S( 10,  69),   S( -4,  20),   S( -3,  -8),   S(  1,  43),   S( -2,  21),   S(  5,  35),   S(  3,   6),   S(  2,  37),
            S( -8,  13),   S( -1,  28),   S(  0,  22),   S(  9,  17),   S(-11,  54),   S( -5, -14),   S(  4,  10),   S(  6,  33),
            S(  7,  19),   S(-12, -22),   S( -2,  10),   S(  1,  16),   S(  1,   8),   S( -3,  36),   S(  1,  13),   S(  6,  60),
            S(  0,  -9),   S(  1,  15),   S( -8, -10),   S(  6,  23),   S(  3,  18),   S( -3,  -7),   S(  4,  33),   S(  2,  17),

            /* bishops: bucket 14 */
            S(-13, -18),   S(  2,  15),   S( 17,   0),   S( -5,  12),   S(-12, -13),   S(  1,  -9),   S( -7, -37),   S( -6, -24),
            S( -3, -16),   S( -6, -38),   S( -3,  29),   S(  1,  22),   S(  9,  39),   S(  1,   4),   S( 16,  48),   S(  6,  25),
            S( -5,   0),   S( -7, -14),   S( -6, -28),   S(  8,  27),   S( 17,  64),   S( 13,  49),   S( 15,  61),   S(  5,  47),
            S( -1,  10),   S(  9,  16),   S(-11,  17),   S(  0,  67),   S( -4,  19),   S( -5,  20),   S( 20,  91),   S( -1,  41),
            S(  2,  20),   S(  4,  24),   S(-14,  13),   S(  4,  35),   S( -9,   0),   S( -2,  16),   S( 14,  26),   S(  4,  65),
            S( -7, -13),   S( 23,  48),   S(  4,  35),   S(  2,  21),   S( -9,  20),   S(  2,  30),   S(  1,  24),   S( 11,  28),
            S(  7,  55),   S(  4,  21),   S(  3,  22),   S( 10,  27),   S( 13,  45),   S( -5,  -4),   S( -3, -29),   S(  0,  22),
            S(  1,  41),   S(  8,  49),   S(  9,  27),   S(  6,  34),   S( -3,  -8),   S( -7, -22),   S( 12,  27),   S(  3,   8),

            /* bishops: bucket 15 */
            S( -2,   1),   S(  0,   3),   S(-18, -43),   S(  1, -16),   S(-11, -16),   S( -8, -43),   S( -6, -37),   S( -1,  -9),
            S(  9,  32),   S(  3,  10),   S( -3,  -7),   S( 11,  33),   S(  2,   2),   S( -7, -15),   S( -2,   8),   S( -1,  -2),
            S(  7,   8),   S( -1, -23),   S( 11,  27),   S( 14,  36),   S( 18,  28),   S(  3,   2),   S( 10,  52),   S(  6,  48),
            S(  0, -10),   S( 10,  16),   S(  6,  45),   S(-15,  17),   S(  8,  32),   S(  4,  35),   S(  6,  39),   S( -5,  23),
            S( -4,   5),   S(  5, -10),   S(  8,  23),   S( 30,  74),   S(-13,  14),   S( 10,  39),   S(  3,  37),   S( -6,  -1),
            S(  6,  14),   S(  1,  12),   S( 21,  88),   S(  2,  19),   S( 20,  55),   S(  2,  39),   S( -5,  -8),   S(  2,   2),
            S(  5,  27),   S(  2,   5),   S( -2,  26),   S(  5,  34),   S(  9,  39),   S( -5,   1),   S( -5,  -6),   S(  0,   2),
            S(  3,  13),   S( -2,  22),   S(  3,  48),   S( 10,  20),   S(  0,  15),   S( -2,   4),   S(  1,   7),   S(  5,  18),

            /* rooks: bucket 0 */
            S( -8,  21),   S( 17,  10),   S(  0,  24),   S( -3,  35),   S(-20,  70),   S( -8,  46),   S(-39,  70),   S(-56,  65),
            S(  6,  -8),   S( 19,  16),   S(-36,  48),   S(  8,  40),   S( -1,  63),   S( -2,  39),   S( -1,   0),   S(-20,  58),
            S(  3,  -3),   S( 23, -25),   S(-19,  40),   S( -5,  20),   S(-36,  77),   S(-13,  36),   S(-22,  56),   S( -5,  44),
            S(-17,  33),   S( 27,  34),   S(-27,  38),   S( 16,  22),   S( 22,  50),   S(-37,  59),   S(-24,  47),   S( -2,  31),
            S( 38, -26),   S( 29,  36),   S( 23,  42),   S( 35,  35),   S( 21,  48),   S( 18,  76),   S( 35,  55),   S(  0,  64),
            S( 35, -15),   S( 65,  38),   S( 81,  30),   S( 93,  34),   S( 11,  79),   S( 52,  62),   S( -8,  87),   S(-44,  94),
            S( 41,  41),   S( 79,  74),   S( 89,  73),   S( 78,  48),   S( 94,  54),   S( 44,  86),   S(-26,  89),   S(-22, 104),
            S( 26, -22),   S( 23,  18),   S( 19,  40),   S( 72,  17),   S( 96,  41),   S( 53,  43),   S( 54,  34),   S( 35,  22),

            /* rooks: bucket 1 */
            S(-51,  51),   S(-17,  10),   S(-11,  33),   S(-42,  46),   S(-34,  51),   S(-34,  48),   S(-48,  69),   S(-81,  85),
            S(-61,  52),   S(-28,   4),   S(-16,  25),   S(-27,  46),   S(-51,  40),   S(-72,  66),   S(-26,  18),   S(-13,  47),
            S(-24,  24),   S(-27,   8),   S(-30,  17),   S(-43,  43),   S(-45,  34),   S(-38,  29),   S(-63,  78),   S(-21,  57),
            S(-58,  79),   S(-18,  24),   S(-18,  47),   S(-35,  44),   S(-78,  79),   S(-69,  87),   S(-33,  58),   S(-73,  89),
            S(-40,  56),   S(  7,  -1),   S(  0,  36),   S( 33,  24),   S(-11,  39),   S(-26,  84),   S(-19,  69),   S(-14,  92),
            S( 67,  31),   S( 18,  33),   S( 49,  12),   S( -9,  62),   S(  4,  41),   S( -1,  67),   S( 51,  45),   S( 18,  73),
            S( 32,  84),   S( 36,  31),   S( 32,  54),   S( 23,  51),   S( 19,  48),   S( 13,  79),   S( 39,  81),   S( 41, 104),
            S( 68,  16),   S( 41, -16),   S(  2,  -1),   S(-14, -10),   S( 45,  24),   S( 29,  43),   S( 32,  70),   S( 98,  32),

            /* rooks: bucket 2 */
            S(-60,  82),   S(-48,  71),   S(-47,  78),   S(-44,  53),   S(-26,  50),   S(-37,  45),   S(-37,  39),   S(-76,  87),
            S(-67,  93),   S(-64,  85),   S(-49,  72),   S(-56,  65),   S(-55,  62),   S(-48,  44),   S(-25,  26),   S(-55,  72),
            S(-52,  86),   S(-49,  76),   S(-60,  65),   S(-37,  54),   S(-53,  61),   S(-34,  52),   S(-28,  39),   S(-29,  58),
            S(-44,  95),   S(-48,  88),   S(-44,  87),   S(-82,  81),   S(-58,  86),   S(-30,  60),   S(-31,  52),   S(-35,  57),
            S(-38, 102),   S(-42, 102),   S(-27,  94),   S(-38,  70),   S(-26,  78),   S( 30,  48),   S( -6,  62),   S(-24,  93),
            S( -4, 111),   S(  2,  99),   S( 21,  88),   S(-16,  76),   S( 53,  52),   S( 38,  68),   S( 94,  29),   S( 64,  81),
            S( 68,  94),   S(  7, 110),   S( 62,  73),   S( 72,  40),   S( 10,  42),   S( -7,  89),   S(-47, 135),   S( 26, 110),
            S( 22,  71),   S( 49,  75),   S( 37,  69),   S( -2,  54),   S(-45,  37),   S( 35,  51),   S( 30,  51),   S( 45,  54),

            /* rooks: bucket 3 */
            S( -9, 103),   S( -7, 106),   S(-13, 138),   S( -6, 125),   S(  4,  87),   S( 11,  78),   S( 26,  57),   S( -7,  45),
            S( -5, 102),   S(-11, 112),   S(-19, 138),   S( -4, 131),   S( -4,  88),   S( 12,  59),   S( 45,  31),   S( 24,  55),
            S(  9, 103),   S(-16, 126),   S(-20, 121),   S(-15, 136),   S(  9,  83),   S(  5,  78),   S( 27,  77),   S( 17,  68),
            S(  2, 131),   S(-13, 143),   S(-21, 152),   S(-17, 143),   S( -4, 108),   S(  6, 102),   S( 29,  93),   S(  1,  80),
            S(-18, 149),   S(-30, 162),   S(  2, 157),   S(  1, 149),   S( 15, 120),   S( 16, 112),   S( 45, 108),   S( 38,  98),
            S( -6, 159),   S( 17, 145),   S( 24, 151),   S( 24, 147),   S( 96,  97),   S( 87, 104),   S( 84,  90),   S( 19, 116),
            S( 19, 159),   S(  3, 164),   S( 12, 169),   S( -1, 165),   S( 17, 151),   S( 96, 102),   S(122, 167),   S(166, 101),
            S( 88,  53),   S( 62,  98),   S( 37, 150),   S( 21, 141),   S( 32, 115),   S( 78, 106),   S( 91,  70),   S(125,  58),

            /* rooks: bucket 4 */
            S(-26, -28),   S( 19, -24),   S( -5, -13),   S(-31, -16),   S(-49,  11),   S(-21,  19),   S(-38, -10),   S(-96,  45),
            S( -5, -39),   S(-60,  -6),   S(  1, -24),   S(  3, -35),   S( 13,  -3),   S(-21,   0),   S(-60,  24),   S( -9,  18),
            S(-21, -10),   S(-22,   2),   S(-32,   5),   S(-26, -36),   S(-28,  -8),   S(-59,   9),   S(-26,   5),   S(-48,  15),
            S(-63, -23),   S( 38, -16),   S( 20, -28),   S( 44, -35),   S( 27,  16),   S(-12,  28),   S(-32,  -2),   S(-10,  -8),
            S(-17, -33),   S(  1, -42),   S( 48,   6),   S( 47, -23),   S( 69,  -4),   S( 50,  20),   S( 18,  16),   S( 24,  18),
            S(-10, -37),   S(  1, -15),   S( 27, -19),   S( 22,  34),   S( 48,  29),   S(  9,  22),   S(  3,  35),   S( 47,  30),
            S(-26, -36),   S( 17,  -4),   S( 60,  -1),   S( 89, -23),   S( 52, -11),   S( -8,   4),   S( 51, -16),   S(  6,  14),
            S(  3, -13),   S( 35,  20),   S( 55, -31),   S( 19, -15),   S( 43,  11),   S( 14,  10),   S( 11,  -1),   S( 15,  13),

            /* rooks: bucket 5 */
            S(-64,  35),   S( -6, -11),   S(  9,  -6),   S( 13,   8),   S( 17,   0),   S( -3,  21),   S(-49,  51),   S(-62,  43),
            S(-24,  15),   S(-22, -32),   S(  9, -27),   S( 27, -20),   S( -6,  -6),   S( -8,  -4),   S(-61,  26),   S(-23,  37),
            S(-53,  18),   S(-44,   4),   S( 21, -44),   S(-18,  -9),   S(-60,  10),   S(  7, -13),   S(-41,  28),   S(-19,  22),
            S(-71,  37),   S(-33,  -1),   S( 23, -13),   S( 45,   7),   S( 17,  -2),   S(-30,  54),   S(-43,  26),   S(-22,  30),
            S( 43,   7),   S(  2,   6),   S( 16,  30),   S( 12, -15),   S(-20,  16),   S(104,  -1),   S( 44,  13),   S( 53,  21),
            S( 50,  -4),   S( -8,  28),   S(  9,   7),   S( -8, -19),   S( 15,  10),   S( 13,  19),   S( 73,  10),   S( -1,  53),
            S( 53,  -9),   S( 45, -18),   S(-16,   0),   S( 40,  16),   S( 66, -21),   S( 67, -37),   S( 64,  -4),   S( 70,  -5),
            S( 36,   6),   S(  2,  -2),   S( 46,  -3),   S( 19,  28),   S( 46,  10),   S( 37,  16),   S( 10,  39),   S(  0,  48),

            /* rooks: bucket 6 */
            S(-93,  66),   S(-20,  17),   S(-50,  34),   S(-28,  17),   S(-14,  11),   S(-11,   4),   S( -3,  -9),   S(-24,   2),
            S(-30,  24),   S(-14,   8),   S(-48,  31),   S(-35,  20),   S(-27,   7),   S(-58,  16),   S(-22,  -7),   S(-36,  20),
            S(-103,  57),  S(-17,  19),   S( -9,   8),   S(-20,   8),   S(-23,  10),   S(  6,  -7),   S(  0, -25),   S(-31,   1),
            S(-63,  57),   S(-26,  29),   S(-28,  33),   S( 35,   0),   S(  5,   6),   S(  3,  16),   S( 22,  -5),   S(  7,  30),
            S(-32,  53),   S( 54,  13),   S(106,   3),   S( 49,  -1),   S( 27,  -6),   S(  5,   5),   S( 46,   3),   S( 76,   2),
            S( 96,  11),   S( 99,  -6),   S( 98,   1),   S( 41,   6),   S(  2, -16),   S( 15,  36),   S( 31,   8),   S(104,   8),
            S( 42,  13),   S( 77,  -3),   S(116, -33),   S(102, -36),   S( 25,   2),   S( 35,  -1),   S( 78, -28),   S( 69, -13),
            S( 66,  -3),   S( 16,  33),   S( -9,  30),   S( 64,  -5),   S( 39,  13),   S( 50,  26),   S( 57,   8),   S( 67,  12),

            /* rooks: bucket 7 */
            S(-113,  37),  S(-71,  38),   S(-72,  51),   S(-73,  50),   S(-51,  17),   S(-34, -11),   S(-29,  -4),   S(-68,  -7),
            S(-96,  42),   S(-50,  24),   S(-49,  24),   S(-61,  40),   S(-50,   6),   S(  0, -24),   S( 11,   3),   S(  4, -61),
            S(-86,  33),   S(-59,  40),   S(-70,  26),   S(-66,  36),   S(-76,  24),   S(-61,  33),   S( 27, -13),   S(-18,  -7),
            S(-75,  40),   S(-44,  17),   S(-11,  16),   S( 15,   0),   S( 12,  -2),   S( 40,   1),   S( 54,  -7),   S(  3, -11),
            S( -9,  37),   S( -2,  34),   S( 40,  17),   S( 69,   7),   S(127, -26),   S(141, -49),   S(107,   9),   S(-65,  -3),
            S( 20,  31),   S( 47,  10),   S( 87,  16),   S(114, -14),   S(130, -13),   S( 48,   3),   S( 28,  30),   S( 11,  11),
            S(-48,  32),   S( 35,  -5),   S( 55,   1),   S( 81, -13),   S( 92, -26),   S(136, -39),   S( 89,  -8),   S( 59, -36),
            S(-34,   8),   S( 24,  14),   S( 11,  26),   S( 13,  29),   S( 34,   2),   S( 64,   7),   S( 52,  20),   S( 34, -10),

            /* rooks: bucket 8 */
            S( 14, -72),   S(-16, -65),   S( 49, -64),   S( 33, -14),   S(-12, -43),   S(-32, -39),   S(  9, -45),   S(-12, -30),
            S(-26, -89),   S(  3, -25),   S(  0, -19),   S(-28, -72),   S( 15, -44),   S( -9, -49),   S( -2,  18),   S(-39, -42),
            S( -1,   1),   S(  3, -17),   S(  6, -25),   S(-32,  -9),   S( 20,  41),   S( 30,  25),   S(  4,  19),   S(-20, -23),
            S( -2, -10),   S( -1, -16),   S(  6,  -1),   S( 18, -12),   S( 25,  47),   S( 28,  34),   S( -6,   4),   S(-18, -19),
            S(  4, -33),   S( 16,  20),   S( 31, -26),   S( 15,  -1),   S( 11, -21),   S(-11, -31),   S( 10,  10),   S(-21, -22),
            S( -7, -31),   S( 14,  -9),   S( -2, -20),   S( -8, -17),   S( -1,  29),   S(-22, -15),   S(-12,   0),   S(  0,   3),
            S( 16,  15),   S( 37,  -2),   S(  1,  -9),   S( 19,  16),   S( 23,  48),   S( 12,  22),   S(  8,  13),   S(  2,   5),
            S(  2,   3),   S( 27, -16),   S(  9,  -8),   S( 19,  25),   S(  0,   3),   S( 24,  30),   S(  9,  -1),   S( -5, -15),

            /* rooks: bucket 9 */
            S( -5, -99),   S( -8, -113),  S( 19, -107),  S(  9, -79),   S(  7, -82),   S( 14, -63),   S(-23, -70),   S(  7, -62),
            S(-37, -88),   S( -2, -81),   S(-13, -76),   S( -5, -78),   S( -4, -93),   S(-15, -25),   S(-28, -63),   S(-16, -52),
            S(  4, -39),   S(-16, -45),   S( -3, -18),   S( -5, -35),   S(  8, -39),   S( 14,  -5),   S(  5, -11),   S(-12,  15),
            S( -5, -56),   S(  0, -25),   S(  3,  -8),   S(  6,   8),   S(-13, -41),   S( 18, -51),   S(  2, -11),   S( -2, -19),
            S( 18, -54),   S(-15, -38),   S( -6, -65),   S( -1, -29),   S( -7, -62),   S(-14, -42),   S(-12, -51),   S(  2, -42),
            S( -2, -39),   S( -6, -51),   S(-10, -37),   S( 15,  -7),   S(  4, -52),   S( -9, -19),   S(-24, -37),   S(  3, -31),
            S( 24,  -7),   S(  5, -40),   S( 22, -55),   S(-12, -14),   S( 11, -41),   S(  5, -16),   S(  2, -21),   S(-30, -64),
            S(-13, -16),   S( -6, -15),   S( 12, -20),   S( 14,   8),   S( 10, -27),   S(  4,   6),   S( -2, -20),   S(  4, -10),

            /* rooks: bucket 10 */
            S( -5, -94),   S(-35, -68),   S( -5, -93),   S( 17, -94),   S( 31, -101),  S( 17, -111),  S( 24, -107),  S(-18, -83),
            S(-13, -68),   S(-19, -76),   S(-40, -54),   S(-34, -77),   S(  8, -70),   S(-11, -94),   S( -2, -74),   S(-39, -90),
            S( -1, -36),   S(-18, -39),   S(-22, -60),   S(-50, -77),   S(-16, -39),   S(  4, -32),   S( -9, -71),   S(-17, -60),
            S(-10, -36),   S(-20, -43),   S( -6, -48),   S( -8, -27),   S( -6, -42),   S( -3,   2),   S(-10, -89),   S(  8, -69),
            S( 12, -51),   S( 10, -39),   S( -2, -55),   S( -1, -64),   S( 14, -13),   S(-13, -26),   S( -4, -80),   S(-22, -82),
            S(-18, -25),   S(-13, -21),   S(  7, -49),   S( -2, -74),   S(  3, -43),   S( 12, -58),   S( -3, -79),   S( -7, -63),
            S(-21, -37),   S(-12, -65),   S(  4, -51),   S( -3, -43),   S( 16, -48),   S( -2, -25),   S(-12, -71),   S( -8, -59),
            S(-12, -25),   S(-11,  15),   S(  5,  -8),   S(-25, -27),   S( -3,  15),   S(-16,  -9),   S(  4, -47),   S( -7, -43),

            /* rooks: bucket 11 */
            S(  1, -66),   S(-29, -21),   S( 15, -66),   S(  8, -25),   S(-49, -42),   S( 38, -84),   S(  4, -65),   S(-17, -102),
            S(-10, -31),   S(-26, -33),   S(-36, -48),   S(-54, -34),   S(-15, -18),   S( 14, -41),   S( -2, -53),   S(-12, -88),
            S(-28,  11),   S(-18,  -3),   S( 11,  10),   S(-17,  -5),   S(  0,  -8),   S( 17, -15),   S( -9, -50),   S(-11,   0),
            S(-19, -60),   S( -3, -52),   S(  3, -23),   S(  5, -17),   S(  7, -28),   S( -8, -66),   S(  2,   2),   S( -3, -17),
            S(  1, -37),   S( 16, -12),   S( -3, -25),   S( -3,  -4),   S( 34, -25),   S( 22, -28),   S( 28,   1),   S(-17, -48),
            S( -6, -21),   S(-14, -20),   S(  5, -28),   S( 20, -27),   S(-12, -30),   S( 32, -23),   S( 20, -18),   S(  5, -36),
            S(-16, -26),   S(-18, -36),   S(-14, -10),   S(-12, -33),   S(  9, -24),   S( 40, -23),   S( 32, -41),   S( 21, -36),
            S( -9, -15),   S( 27,  13),   S(  1,  18),   S( 21,  22),   S( -9,   8),   S( 21, -16),   S( 61, -37),   S(  9,   2),

            /* rooks: bucket 12 */
            S(-20, -100),  S(  2,  -9),   S(  7, -40),   S(-17, -78),   S(  3, -48),   S( 15,  -7),   S(-21, -66),   S(-19, -71),
            S(  4, -11),   S(  5,  15),   S( 10,  14),   S(  4, -15),   S(  3, -35),   S( 19,  -5),   S( -6,  -5),   S(-14, -37),
            S( -6, -21),   S(  2,  15),   S( 17,  32),   S( 17, -15),   S(  5, -39),   S( 21,  17),   S(  9,  20),   S( -3,  -1),
            S(  3,   3),   S( -3,  -1),   S(  9,  23),   S( 10,  19),   S(  4, -10),   S(  3, -11),   S( 14,  32),   S(  2,  13),
            S(  8,  -9),   S( 12,  16),   S(  7,  14),   S(  2, -22),   S( 17,  52),   S( -8,  -5),   S(  1, -21),   S(  4, -10),
            S(  7, -10),   S( -8, -34),   S(  6, -34),   S(-12, -44),   S(  5,  -3),   S(  3,  -7),   S(  9,  12),   S( -5, -24),
            S(-23, -59),   S( -3,   1),   S( 14,   8),   S( -2,   8),   S( -4, -14),   S( 13,  14),   S(  5,  23),   S(-12, -33),
            S( -2, -20),   S( -3, -23),   S( -1, -21),   S(  9,  -4),   S( -1, -14),   S( -1,  -8),   S( -4, -18),   S(  5,  35),

            /* rooks: bucket 13 */
            S(-17, -52),   S(-16, -77),   S(-20, -55),   S( -3, -31),   S(  1, -41),   S( 15,  -8),   S(-27, -51),   S(-27, -73),
            S(-14, -74),   S( -3, -41),   S(  0,  -4),   S( -6, -33),   S( 17,   5),   S(  6, -12),   S( 12, -13),   S(-14, -74),
            S(  5, -46),   S(-10, -62),   S( -3,  -9),   S( 20,  17),   S( 21,   6),   S( 17, -38),   S( 12, -10),   S(-17, -142),
            S( -2, -31),   S(  7, -12),   S( -6, -38),   S(  9, -12),   S(  5, -21),   S(-13, -34),   S(  1,  -9),   S(  3,   7),
            S(  5, -23),   S(  5, -50),   S( -8, -60),   S(  8, -28),   S(  1, -34),   S(  4, -10),   S(  7,   8),   S(  5,  -2),
            S(  0, -21),   S(-15, -61),   S(-15, -63),   S(  2, -40),   S( -7, -94),   S(  6,  -6),   S(-10, -30),   S(-14, -59),
            S(  0, -15),   S( 10, -37),   S( -1, -28),   S(  3, -18),   S( -5, -71),   S( -5, -47),   S( -4, -20),   S( -8, -56),
            S( -3, -24),   S(-14, -50),   S( -4, -55),   S( 11, -19),   S( -7, -55),   S(  3, -18),   S( -2, -18),   S(  5,   0),

            /* rooks: bucket 14 */
            S(-13, -67),   S(-32, -42),   S( -6, -32),   S(-18, -91),   S(-14, -59),   S( -3, -54),   S(-36, -132),  S(-17, -79),
            S(  2, -44),   S( 15,   3),   S( 15, -29),   S( -8, -55),   S(  3,  -7),   S(  4,   5),   S(  2, -33),   S( -3, -66),
            S( -2, -45),   S( -3, -16),   S(  5, -43),   S( -1, -39),   S(  5,  -1),   S( -5, -31),   S( -1, -45),   S(-15, -82),
            S(-14, -39),   S(  8,  32),   S( 11,  33),   S( -3, -31),   S( -7, -47),   S( -4, -35),   S(  2, -25),   S(-20, -57),
            S(  9,  -2),   S(  8,  24),   S( -1,  -4),   S( 11, -26),   S(  2, -26),   S( 20,   7),   S( -7, -54),   S( -8, -40),
            S( -4, -36),   S( -1,  -4),   S( -5, -40),   S(  2, -102),  S(-11, -79),   S( -2, -56),   S( -3, -67),   S(-15, -62),
            S(-17, -24),   S(  2,  -6),   S(-13, -65),   S(-18, -80),   S(-13, -60),   S(  7, -15),   S(-10, -66),   S(-12, -52),
            S( -8, -20),   S( -1,  -4),   S(-10, -54),   S( -4, -62),   S(-17, -71),   S(-17, -105),  S(  7, -43),   S( -3,  -7),

            /* rooks: bucket 15 */
            S(-13, -75),   S(-11, -45),   S(-36, -66),   S(-16, -59),   S( -2, -47),   S(-21, -60),   S( -5, -19),   S( -8, -71),
            S(  7,  14),   S(-11, -65),   S(  2, -33),   S( -5, -25),   S(-10, -48),   S( -3, -14),   S(  8,   2),   S(  1,  -6),
            S(  3,   2),   S(  5, -33),   S(  8,  19),   S(  7, -38),   S(  3, -31),   S( -9, -35),   S(  2,  -2),   S(  4,  -6),
            S( -4, -21),   S( -6, -14),   S( 14,  21),   S( -3,  -1),   S( 10,  19),   S(-10, -42),   S(  5, -18),   S( -9, -42),
            S(  7,   9),   S( -2, -20),   S(  5,  -9),   S( -6, -18),   S(  5,  11),   S( -4, -41),   S( 11, -35),   S( -8, -57),
            S(  7,  24),   S( -1,   8),   S(  2,   7),   S( -2, -24),   S( -7, -46),   S(  9, -41),   S( 11, -11),   S( -4, -36),
            S(  0, -31),   S( -8, -18),   S(  6,  -3),   S(  9,   9),   S(  6,   3),   S(  8, -40),   S(  1, -50),   S( -2, -16),
            S(  0,  -3),   S( -5, -17),   S(  5,  21),   S( -3,  -8),   S( -5, -13),   S( -8, -41),   S(  6, -29),   S(-21, -68),

            /* queens: bucket 0 */
            S( 14, -45),   S(-32, -78),   S(-24, -92),   S(  2, -122),  S(-11, -38),   S( -2, -78),   S(-47, -38),   S(-15,   1),
            S( -2, -78),   S( 18, -97),   S(  6, -76),   S(-13, -36),   S( 12, -63),   S(  4, -70),   S(-38, -58),   S(-41,  -4),
            S(-22,  25),   S(-18,   6),   S( 30, -68),   S(-17,   0),   S( 10, -40),   S( 17, -44),   S(-14, -55),   S(-80, -80),
            S(-43,  76),   S(  7, -17),   S(-33,  57),   S(-22,  52),   S(  1,  33),   S(-43,  49),   S(-34,  12),   S( -5, -69),
            S(-31,   3),   S(-21,  85),   S( -3,  45),   S( -7,  71),   S(  0,  61),   S(-24,  65),   S(-47,  37),   S(-39,  -3),
            S(-26,  30),   S(  7,  69),   S( 12,  42),   S(-30,  89),   S(-68,  89),   S(-70,  85),   S(-51, -10),   S(-69,  -4),
            S(  0,   0),   S(  0,   0),   S( 15,  25),   S(-43,  49),   S(-49,  39),   S(-91,  92),   S(-99,  59),   S(-120,  29),
            S(  0,   0),   S(  0,   0),   S(  3,  30),   S( -2,  15),   S(-59,  42),   S(-30,  14),   S(-63,  21),   S(-51, -19),

            /* queens: bucket 1 */
            S( 22, -48),   S(-12, -20),   S( -1, -61),   S( 17, -83),   S( 43, -94),   S(-11, -80),   S(-22, -30),   S(-24,  -9),
            S(-38, -12),   S(  0,  -5),   S( 31, -51),   S( 11, -25),   S( 36,  -8),   S( 14, -46),   S(-24,  27),   S(-26, -14),
            S(  6, -12),   S(  9,  -7),   S( -8,  24),   S(  5,  57),   S(-21,  58),   S( 25,  16),   S(  0, -15),   S(  3, -14),
            S( 23,  -6),   S(-11,  77),   S( 12,  59),   S( 31,  59),   S(  9,  70),   S(-12,  57),   S( 25, -14),   S( -2,  64),
            S( 36,  22),   S( 31,  39),   S( 13,  75),   S(-22,  92),   S(  6, 105),   S( 53,  10),   S(-10,  94),   S( 17,  50),
            S( 50,   3),   S( 80,  59),   S( 82,  57),   S(100,  92),   S( 32,  83),   S( -9, 105),   S(  0,  72),   S( -7,  53),
            S( 86,  -8),   S( 41,  14),   S(  0,   0),   S(  0,   0),   S(  5,  78),   S(-36,  73),   S(-34,  87),   S(-38,  23),
            S( 83,  -7),   S( 46,   8),   S(  0,   0),   S(  0,   0),   S( 43,  15),   S( 77,  47),   S( 97,  31),   S(-18,  43),

            /* queens: bucket 2 */
            S( 24, -26),   S( 19, -13),   S( 21,  26),   S( 42, -42),   S( 48, -66),   S( 28, -59),   S(-18, -58),   S( 26,  24),
            S( 25,  11),   S( 20,  24),   S( 39, -13),   S( 37,   9),   S( 50,  -6),   S( 27, -24),   S(  4,  -1),   S( 26,  31),
            S( 25,  19),   S( 23,  33),   S( 18,  84),   S( 13,  53),   S( 27,  43),   S( 21,  42),   S( 25,  31),   S( 32,  11),
            S( 29,  13),   S( 21, 106),   S( 14,  98),   S(  2, 108),   S( 29,  88),   S( 15,  66),   S( 21,  74),   S( 20, 113),
            S( -7,  77),   S(  5,  44),   S(-12, 141),   S( 22, 117),   S( 29, 143),   S( 67,  54),   S( 54, 108),   S( 70,  39),
            S(-17,  69),   S(-55, 113),   S(-13, 123),   S( 54, 106),   S( 48,  93),   S( 84, 104),   S(123,  74),   S( 25, 142),
            S(-24,  80),   S(-34, 103),   S( 16, 101),   S( 91,  53),   S(  0,   0),   S(  0,   0),   S( 12, 115),   S( 45,  70),
            S( 20,  37),   S( 36,  45),   S( 80,  25),   S( 68,  56),   S(  0,   0),   S(  0,   0),   S( 85,  73),   S(  3, 114),

            /* queens: bucket 3 */
            S(-32,  50),   S(-20,  47),   S( -7,  40),   S(  8,  43),   S( -3,  22),   S(-14,  11),   S( -3, -49),   S(-23,  29),
            S(-36,  60),   S(-15,  37),   S( -3,  55),   S(  3,  68),   S(  5,  62),   S( 12,  25),   S( 32, -17),   S( 53, -65),
            S(-37,  65),   S(-21,  88),   S(-15, 117),   S(-20, 131),   S( -5,  97),   S( -7, 101),   S(  4,  61),   S( -6,  52),
            S(-20,  68),   S(-22, 105),   S(-32, 155),   S(-21, 165),   S(-25, 165),   S( -2, 114),   S(  6, 104),   S( -4,  93),
            S(-27,  98),   S(-35, 143),   S(-32, 157),   S(-13, 179),   S(-14, 173),   S(  2, 191),   S(  0, 151),   S(-12, 141),
            S(-53, 116),   S(-49, 158),   S(-51, 183),   S(-62, 202),   S(-24, 205),   S(  0, 193),   S(-16, 195),   S(-13, 173),
            S(-82, 133),   S(-80, 153),   S(-78, 202),   S(-82, 192),   S(-92, 228),   S( 36, 117),   S(  0,   0),   S(  0,   0),
            S(-114, 169),  S(-92, 149),   S(-73, 147),   S(-94, 171),   S(-49, 161),   S(-30, 148),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-58, -15),   S(-49, -36),   S(  7,  11),   S(-10, -48),   S(-12,  -8),   S( 22,   0),   S(-46, -45),   S(  6,   4),
            S( -2,   6),   S(-45,  -6),   S( -4,  -1),   S(  8, -13),   S(-56,  15),   S( -2,  30),   S(-39, -13),   S( -3,  -5),
            S(  8,  29),   S( 17, -32),   S( 24,  21),   S(  4,  -5),   S( 10,  11),   S(-20,   8),   S(-50, -44),   S( 26,  13),
            S( 16,  21),   S( 34,  -5),   S( 23,   5),   S( -3,  16),   S( 35,   3),   S( -9,  40),   S(-18,  -7),   S(-27, -19),
            S(  0,   0),   S(  0,   0),   S( 54,  33),   S( 66,  65),   S( 18,  60),   S(  3,  22),   S( -5,   6),   S(  2,  22),
            S(  0,   0),   S(  0,   0),   S( 21,  34),   S( 41,  49),   S( 40,  62),   S( 35,  53),   S(-12,  25),   S(-10,  -4),
            S( 32,  16),   S( 28,  33),   S( 44,  40),   S( 62,  67),   S( 57,  37),   S( 15,  20),   S(  2,  10),   S(-19,  -7),
            S( 24,  18),   S( -3,   0),   S( 30,  42),   S( 61,  46),   S(  8, -25),   S(-29, -37),   S(-35, -31),   S(  3, -18),

            /* queens: bucket 5 */
            S(  9,  -4),   S( 35,  -2),   S( -8,  10),   S(-32, -15),   S(  4, -20),   S( 43,  43),   S( 12,  15),   S(-25, -18),
            S( 12,  15),   S( -4,   0),   S( 28, -26),   S(-16,  22),   S( 26,  32),   S(-69, -21),   S( 26,   9),   S(-14,  -5),
            S( 32,  -1),   S( 90,   9),   S( 18,  14),   S(-25,  15),   S( 14,  23),   S( 22,  16),   S( 18,  44),   S( 22,  22),
            S( 14,  -4),   S( 41,  11),   S( 24, -12),   S( 21,  30),   S( 71,  36),   S( 35,   7),   S( 13,  31),   S(-19,  32),
            S( 32,  39),   S( 48,  15),   S(  0,   0),   S(  0,   0),   S( 19,  15),   S( 16,   8),   S( 15,  36),   S(-10,  14),
            S( 36,  12),   S( 44,  35),   S(  0,   0),   S(  0,   0),   S( 48,  45),   S( 85,  52),   S( 16,  10),   S( 39,  26),
            S( 68,  24),   S(105,  23),   S( 68,  80),   S( 20,  25),   S( 79,  60),   S( 91,  71),   S( 45,  71),   S( 24,   6),
            S( 22,  44),   S( 67,  36),   S( 90,  35),   S( 61,  45),   S( 41,  58),   S( 78,  60),   S( 31,  10),   S( 33,  15),

            /* queens: bucket 6 */
            S( 45,  25),   S( -5, -43),   S( 29,  -8),   S( 26, -12),   S( -8, -15),   S(-33, -25),   S(-15,  -8),   S(  5,   8),
            S( 26,   6),   S( 33,  -2),   S( 12,  17),   S( 36, -16),   S( 20, -16),   S( -6,  -1),   S(-37,  23),   S( -2,  11),
            S(-27,  54),   S( 42,  12),   S(-10,   8),   S( 19, -22),   S( 20,  -3),   S( -1,  12),   S( 65,  31),   S( 39,  34),
            S( 15,  22),   S(-54,  28),   S( 36,  11),   S( 78,  32),   S( 25, -22),   S( 31,  22),   S( 82,  39),   S(107,  23),
            S(-15,   6),   S( 16,  34),   S( 26,  51),   S( 93,  74),   S(  0,   0),   S(  0,   0),   S( 64,  34),   S( 97,  34),
            S( 20,  55),   S( 61,  30),   S( 62,  41),   S( 64,  43),   S(  0,   0),   S(  0,   0),   S(121,  91),   S(103,  31),
            S( 32,  -6),   S( -8,   7),   S( 63,  29),   S( 76,  43),   S( 52,  72),   S( 83,  77),   S(111,  32),   S(157,  -8),
            S( 19,   8),   S( 40,  -3),   S( 77,  51),   S(109,  52),   S(121,  53),   S( 91,  44),   S(130,  28),   S(109,  22),

            /* queens: bucket 7 */
            S(-12,  -2),   S(-26, -14),   S(-23, -20),   S(-41,  21),   S( -8, -16),   S(-30,  15),   S(-30,  22),   S(-24, -32),
            S(-54, -18),   S(-78,  23),   S(-25,  36),   S(-28,  45),   S(-34,  36),   S( -7,  -3),   S( -2,   5),   S(-55, -13),
            S(-23, -36),   S(-57,  12),   S(-21,  33),   S( 14,  35),   S( 36,  20),   S( 36, -11),   S( 23, -28),   S( 32, -25),
            S(-76,   2),   S( -7, -20),   S(  7,  29),   S( 23,  45),   S( 95,  41),   S( 63,  23),   S( 78, -16),   S( 72,   0),
            S(  1,   3),   S(  9,  17),   S( -6,  64),   S( 35,  53),   S( 68,  31),   S( 70,  31),   S(  0,   0),   S(  0,   0),
            S(-15,  -5),   S(-17,  38),   S( -2,  60),   S(-11,  42),   S( 57,  12),   S(120,  49),   S(  0,   0),   S(  0,   0),
            S(-27,  18),   S(-27, -10),   S(-12,  26),   S( 52,  24),   S( 72,  28),   S(119,  14),   S( 62,  41),   S( 75,  54),
            S( 15, -21),   S( 31, -22),   S( 35,  21),   S( 52, -22),   S( 72,  35),   S( 53,   4),   S( 26,  13),   S(103,  16),

            /* queens: bucket 8 */
            S(-14, -29),   S( 24,  12),   S(-17, -44),   S(  2, -12),   S( -8, -17),   S(  5, -11),   S( 10,  -3),   S(  3,   8),
            S(-12, -20),   S( -5,   5),   S( 19,  12),   S(  6,  12),   S(  3, -13),   S( -1,  -9),   S( 10,  19),   S( -3,  -4),
            S(  0,   0),   S(  0,   0),   S(  9,  -2),   S(  4, -21),   S( 17,  10),   S(  6,   8),   S( -7,  -6),   S( -6,  -2),
            S(  0,   0),   S(  0,   0),   S(  3,   7),   S(  3,   4),   S(  4,   6),   S(  2,  -8),   S( 13,  33),   S( -4, -14),
            S(  8,   3),   S(  1,   9),   S( 15,  33),   S( 27,   3),   S( -6,  -9),   S(  6,  -2),   S( 28,  23),   S(-14, -19),
            S( 14,   9),   S(  7, -22),   S( 17,  21),   S(  4,   7),   S( -3,   5),   S( 25,  16),   S(-15, -46),   S(-18, -38),
            S( -1, -35),   S( -8, -23),   S( 18,  12),   S( 22,  21),   S(  3,  -7),   S( 12,  20),   S( -4, -13),   S(  2,   2),
            S(  5,  -9),   S( 23,  20),   S( 20,  15),   S(  6,  17),   S( 21,   6),   S( -9, -16),   S(-22, -45),   S(-17, -32),

            /* queens: bucket 9 */
            S( 22,  15),   S( -8, -33),   S( -8, -25),   S( 29,   3),   S( -2, -31),   S( -8, -33),   S( -5, -15),   S( -9, -27),
            S( 16,   0),   S( 12,   1),   S(  1,  -2),   S( 18,  20),   S(-22, -52),   S( -7, -23),   S(  6,  11),   S( -4, -14),
            S( 11,  -6),   S( -4, -23),   S(  0,   0),   S(  0,   0),   S( 16,  21),   S( 22,   8),   S( -6, -11),   S(  7,  -1),
            S( -1,  -3),   S( -6, -15),   S(  0,   0),   S(  0,   0),   S(  4,  -1),   S( 19,  17),   S( 20,  24),   S(-14, -10),
            S( 12,  -9),   S( 20,   9),   S( 10,  18),   S(-20, -33),   S( 13,   2),   S( 24,  23),   S(  7, -24),   S( -5, -20),
            S( 16,  22),   S( 21,   3),   S(  7,  -4),   S(  5, -11),   S(  3, -18),   S(  7,  -3),   S( -3, -19),   S( -8, -29),
            S(  1,  -3),   S( 18,   0),   S( 10,   8),   S(  2,  -1),   S( 29,  24),   S( 34,  36),   S( -2,   1),   S( 18,  -9),
            S( 12, -21),   S( 13,  -6),   S(-23, -37),   S( 17,   6),   S(  1,   7),   S( -4, -11),   S( -2, -23),   S( -3, -30),

            /* queens: bucket 10 */
            S( 14,  20),   S( 13,   3),   S(  2,  -3),   S(  1,  -1),   S( -8, -34),   S( 27,  35),   S( -3, -15),   S( -6, -26),
            S(  8,  10),   S( -3, -18),   S(  9,  -9),   S(  4,  -4),   S( -4, -14),   S(  8,  -4),   S(-10, -46),   S( -1, -17),
            S( 14,  18),   S( 10,  11),   S( 22,  24),   S(  3, -11),   S(  0,   0),   S(  0,   0),   S(  5,   5),   S(-19, -30),
            S(-12, -32),   S(  5,  -2),   S(  5,   3),   S( 14,  13),   S(  0,   0),   S(  0,   0),   S( -2,  -7),   S( -5, -31),
            S(-11, -21),   S( 18,  14),   S(  1,   1),   S( 43,  41),   S( 12,  19),   S(  9,  18),   S( 15,   3),   S( 15,   8),
            S( -7, -15),   S(  5,  -8),   S( 40,  45),   S( 15,  18),   S( 11,   7),   S(  0,  -7),   S( 15,  -8),   S( -5, -49),
            S(  6,  -6),   S(  5,  15),   S( 26,  35),   S( 31,  29),   S( 15,   9),   S(  3,  -2),   S(  4, -13),   S(  4,   2),
            S(-31, -67),   S(  4,  -6),   S(  7, -13),   S( -8, -31),   S( 12,  11),   S(-13, -34),   S( -8, -28),   S(  6, -17),

            /* queens: bucket 11 */
            S( -5,   4),   S( -2,  -2),   S(-21, -40),   S(-27, -54),   S(  9,  18),   S(  7, -13),   S(  4,  -8),   S(  1, -17),
            S(  2,  16),   S(  2,   3),   S(-23, -18),   S( 12,  -3),   S( 18,  -6),   S( 11,   9),   S( 23,  19),   S(-12, -38),
            S(  3,  -5),   S( 13,  15),   S(-25,  -8),   S( 13,  -7),   S(  5,  -2),   S(-35, -52),   S(  0,   0),   S(  0,   0),
            S(-13, -19),   S(  3,  11),   S( -1,   3),   S(  9,  -5),   S(  4,  -9),   S( -5,  -3),   S(  0,   0),   S(  0,   0),
            S( -3,  -3),   S(  4,  -4),   S(  2,   3),   S(  9,  26),   S( 62,  82),   S( 17,  34),   S(  5,   2),   S(-11, -22),
            S(-17, -32),   S(-14, -23),   S(-22, -29),   S(  7,   7),   S( -2,   3),   S( 17,   7),   S( -4, -28),   S( 28,  -8),
            S(-13,  -7),   S(  1,   2),   S( 15,  15),   S(  2,  13),   S( 11,  24),   S( 17,   7),   S( 22,  17),   S( 19,  22),
            S(-24, -72),   S( -3, -21),   S(-12, -22),   S(-10,  -4),   S( 26,  22),   S( -2,  -8),   S(-27, -31),   S(  7,   5),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  4,   3),   S(-25, -24),   S(  3,  16),   S( -8, -20),   S( -2,  -4),   S( -8, -20),
            S(  0,   0),   S(  0,   0),   S(  7,   4),   S( -5, -18),   S(  2,  -5),   S( -8, -30),   S(  1,  -4),   S(  5,   2),
            S( -1,  -4),   S( -5, -14),   S(  8,  12),   S( -1, -23),   S( 27,  49),   S(  3,   9),   S(  4,   0),   S(  5,   8),
            S(  2,  -6),   S(  9,  10),   S( 15,  23),   S(  7,  12),   S( -9, -35),   S(-12, -27),   S( -2,  -6),   S( -4,  -7),
            S(-15, -37),   S(  6,   4),   S(  1,   4),   S(-22, -50),   S( -1,  -3),   S(-23, -58),   S(-14, -34),   S( -6,  -8),
            S( -1,  -3),   S(-16, -32),   S( -8, -25),   S( -6, -14),   S(-13, -29),   S(-15, -17),   S( -8,  -7),   S( -8, -21),
            S(-14, -25),   S(  5,   6),   S( -4,  -9),   S(  8,  13),   S( -5, -19),   S(-11, -30),   S(  8,  13),   S( -2, -18),
            S(  9,  22),   S( -8, -17),   S( -5,  -4),   S(  4,   0),   S(-13, -32),   S(-15, -38),   S(-13, -27),   S(-20, -43),

            /* queens: bucket 13 */
            S(-11, -42),   S( -2,  -5),   S(  0,   0),   S(  0,   0),   S(-23, -44),   S(-19, -34),   S(  4, -22),   S( -5, -14),
            S(-20, -48),   S( -9, -26),   S(  0,   0),   S(  0,   0),   S( -9, -19),   S(-16, -38),   S( -4,  -3),   S( -7, -19),
            S( -8, -20),   S(  6,   0),   S(  3,   1),   S(  0,  -6),   S(-12, -30),   S( 10,   6),   S(-14, -25),   S(  1,   3),
            S(  0,   4),   S( -7, -29),   S( -3, -10),   S( 14,  19),   S( 13,  26),   S( 24,  44),   S( -5, -28),   S(-10, -21),
            S(  9,  14),   S(-10, -37),   S( -8, -33),   S( 10,  11),   S(-10, -26),   S( -8, -14),   S( -1,  -5),   S(  1,  -1),
            S(  1,  -2),   S(-15, -38),   S(  4,   8),   S( 16,  31),   S( 11,   4),   S( -9, -20),   S(-18, -40),   S(-20, -50),
            S( -1,   2),   S( -6, -16),   S( -1,   2),   S( -2,  -7),   S( -3, -11),   S( 10,  10),   S(-17, -37),   S( -2,  -2),
            S( -9, -31),   S(-14, -37),   S(-19, -35),   S(  8,  19),   S( -4,  -3),   S( -1,  -3),   S( -4, -12),   S(-12, -29),

            /* queens: bucket 14 */
            S(  6,  -2),   S( 19,  14),   S(  6,  -5),   S(  3, -11),   S(  0,   0),   S(  0,   0),   S(  8,   1),   S( -7, -22),
            S(-13, -32),   S(-14, -41),   S( -9, -29),   S(  0, -11),   S(  0,   0),   S(  0,   0),   S( -6,  -8),   S( -4, -19),
            S( -1,   0),   S( -3, -30),   S(  2,   3),   S(  0,  -2),   S(  5,  12),   S( -1,  -3),   S(  1,   0),   S( -7, -18),
            S( -6, -16),   S(-11, -28),   S(  0, -11),   S(-12, -16),   S( -6, -11),   S(-14, -37),   S(-12, -43),   S( -6, -18),
            S(  0,   1),   S(  2,  -3),   S(-13, -30),   S( 12,  26),   S(  4,   5),   S( -3,   0),   S( -1,  -4),   S(-19, -45),
            S( -5, -11),   S( -2, -15),   S(-11, -20),   S(-27, -57),   S(  1,   4),   S(-12, -23),   S( -6, -14),   S(-16, -30),
            S(-14, -23),   S(-11, -23),   S( -4, -14),   S(  5,  15),   S(  1,   4),   S( -1,  -4),   S( -5,  -8),   S(-10, -15),
            S(-10, -29),   S(  4, -14),   S(-15, -29),   S(-10, -22),   S(  7,  11),   S( -6, -13),   S( -3,  -8),   S( -3, -13),

            /* queens: bucket 15 */
            S( -3,  -8),   S( -7, -21),   S(  0,  -6),   S(-14, -27),   S(  7,  -7),   S(-14, -28),   S(  0,   0),   S(  0,   0),
            S( -3, -11),   S(  0,  -8),   S(-15, -21),   S( -2,  -3),   S(  3,   4),   S(  3,   7),   S(  0,   0),   S(  0,   0),
            S( -2, -13),   S(  6,  14),   S(-15, -16),   S( -5, -14),   S( -3, -18),   S(  4,   4),   S( -5, -13),   S( -2,  -5),
            S( -6, -12),   S(-13, -28),   S( -2,  -9),   S(  4,   2),   S( 12,  18),   S(  6,  10),   S(-10, -29),   S(  2,  -1),
            S( -2,  -7),   S( -4, -12),   S( -8, -19),   S(  5,  -4),   S(  9,  36),   S(  3,  12),   S(  4,   5),   S( -9, -30),
            S(  1,  -3),   S( -9, -17),   S( -2,  -5),   S(-12, -10),   S( -1,  -1),   S(-15, -35),   S( -3, -11),   S( -3,  -3),
            S(-11, -22),   S( -6, -15),   S( -8,  -9),   S(  8,  11),   S(-13, -21),   S( -7, -16),   S( -4,  -5),   S( -2,  -5),
            S( -8, -13),   S(-22, -54),   S(  0,  -7),   S( -8, -13),   S( -8, -13),   S(-14, -33),   S(  4,  11),   S( -6, -12),

            /* kings: bucket 0 */
            S( -1, -38),   S( 25,  -8),   S( 14, -10),   S( -9,   4),   S(-27,   9),   S( 28, -35),   S(  3,  13),   S( 18, -53),
            S(-20,  36),   S( -5,  12),   S(-13,  11),   S(-48,  20),   S(-33,  32),   S(-18,  27),   S( -9,  44),   S(-19,  35),
            S( 17,   2),   S( 91, -27),   S( -1,   4),   S(-31,  -2),   S(-45,   8),   S(-23,   6),   S(-53,  26),   S( 11, -20),
            S(-36,  -7),   S(-17, -11),   S(-19, -11),   S(-29,  16),   S(-78,  37),   S(-25,   1),   S(-14,   5),   S( -8,  28),
            S(-49, -79),   S( 26, -47),   S( 27, -29),   S(  5, -16),   S(-17, -22),   S(-23,  12),   S( -3,   5),   S( 10,  -1),
            S( -3, -113),  S( 18, -30),   S( 30, -69),   S(  8, -20),   S(  6,  -7),   S( 10, -37),   S( 16,  -4),   S(  4, -30),
            S(  0,   0),   S(  0,   0),   S(  2, -42),   S( 22, -44),   S(  8, -24),   S( -3, -32),   S( -2, -30),   S( -9,   8),
            S(  0,   0),   S(  0,   0),   S( -8, -49),   S( 18, -16),   S( 10,   5),   S( 13,   2),   S( 10,  16),   S( 10,  12),

            /* kings: bucket 1 */
            S( 20, -30),   S( 29, -19),   S( 16, -15),   S( 10,   5),   S(-12,  -3),   S( 19, -14),   S(  7,  11),   S( 22, -20),
            S( -2,   9),   S(  7,  14),   S( 13, -12),   S(-42,  23),   S(-35,  29),   S(-12,  16),   S( -4,  27),   S(-10,  29),
            S(-17,  -9),   S( -8,  -4),   S( 14, -15),   S( 15, -24),   S(-37,   1),   S(  8, -17),   S( 21,  -5),   S( 36,  -9),
            S(  1, -18),   S( 25, -13),   S( 14,  -3),   S(-19,  14),   S(-18,  21),   S(-26,   8),   S(  8,   6),   S(-48,  33),
            S( -1, -30),   S( 22, -23),   S( 16, -36),   S( 38, -24),   S(  8,  -9),   S( 16, -19),   S(  1,  10),   S( -5,   3),
            S( -4, -25),   S( 20, -53),   S( 34,   1),   S( 42, -21),   S( -3, -21),   S( 21,   3),   S( 23,   9),   S(-14, -20),
            S(  8, -24),   S( 14,   1),   S(  0,   0),   S(  0,   0),   S( -5,  16),   S(  5,  20),   S(-12,  36),   S(-11, -28),
            S(-19, -130),  S( -5, -11),   S(  0,   0),   S(  0,   0),   S( -1, -33),   S(  9, -10),   S(  2,  47),   S(-10, -58),

            /* kings: bucket 2 */
            S( 27, -62),   S( 16,  -5),   S( 20, -28),   S( 32, -15),   S(-13,  10),   S( 41, -25),   S(  7,  26),   S( 32, -22),
            S( 11,  -4),   S(  2,  27),   S( -7,   6),   S(-11,  11),   S(-28,  16),   S( -9,   4),   S( 18,   4),   S( -1,   7),
            S(-44,  -4),   S( -3,  -1),   S( 13, -13),   S(-50,  -3),   S(-17,  -3),   S( 20, -29),   S( 40, -17),   S( 28, -26),
            S( -8,  13),   S( -5,  13),   S(  1,  -1),   S(-18,  14),   S( 37,  -3),   S(-43,   5),   S(-16,  -5),   S( 42, -19),
            S(-22, -14),   S( 29,  -5),   S( -8, -12),   S(  8, -18),   S( 25, -30),   S( -9, -33),   S( 46, -34),   S(  7, -40),
            S(  1,   5),   S( -3, -12),   S( 26, -23),   S( 40, -34),   S( 67, -18),   S( 24,   5),   S( 68, -31),   S( 18, -36),
            S( -8, -12),   S( -6,  31),   S(-13, -15),   S(  9,  17),   S(  0,   0),   S(  0,   0),   S( 27,  34),   S(-17, -22),
            S( -6, -16),   S( -9, -32),   S( -5, -49),   S(  4,   2),   S(  0,   0),   S(  0,   0),   S( -1,  -1),   S(-24, -151),

            /* kings: bucket 3 */
            S( 13, -79),   S(  9,   1),   S( 24, -41),   S(  0, -17),   S(-15, -24),   S( 33, -32),   S(  4,  16),   S(  7, -31),
            S(  6,  11),   S(-26,  37),   S(-16,   2),   S(-36,  13),   S(-56,  28),   S( -2,  -5),   S( -8,  17),   S(-10,  18),
            S( 41, -45),   S(  7,  -8),   S( 17, -23),   S(-11, -21),   S(-12,  -1),   S( 38, -36),   S( 53, -28),   S( 52, -29),
            S(-39,  25),   S(-107,  41),  S(-94,  23),   S(-81,  20),   S(-71,  22),   S(-89,   4),   S(-63,   0),   S(-68,  -8),
            S(-32,  26),   S(-39,  -3),   S(-46,   2),   S(-66,   1),   S(-11, -25),   S(  2, -45),   S(  8, -45),   S(-33, -58),
            S(-27, -29),   S(  6, -14),   S( 14, -30),   S(-35,  -8),   S(  8, -38),   S( 82, -55),   S(109, -52),   S( 61, -109),
            S(-42, -14),   S( 37,  -8),   S(  4, -30),   S( 21, -28),   S( 27, -16),   S( 38, -44),   S(  0,   0),   S(  0,   0),
            S(-12, -34),   S( -1, -28),   S(  3,  -5),   S(  3, -20),   S( 19, -90),   S(  3, -44),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-47, -28),   S(  1,  26),   S(-35,  36),   S( 13,   6),   S(  6, -11),   S(-20,   3),   S( 11,  15),   S( 19, -25),
            S(-47,  23),   S( 32,  22),   S( 20,   7),   S(  1,  -4),   S( 61, -12),   S( 12,  -1),   S( 37,   8),   S( 30, -12),
            S( -5,  13),   S(-27,  -1),   S(-26,  17),   S( -5,  -9),   S(-10,  -5),   S( -6, -16),   S(-36,   4),   S(-14,  11),
            S(  6,  -6),   S( 26,  -6),   S( 28,   4),   S( -3,  10),   S( -2,   0),   S( 13,  12),   S( -7,   8),   S( 31,   3),
            S(  0,   0),   S(  0,   0),   S(  7, -27),   S(  6,  20),   S( -7,  -9),   S(-30,   1),   S(-30,  14),   S(-13,   4),
            S(  0,   0),   S(  0,   0),   S(  4,  22),   S( 17, -11),   S( 17,  21),   S( 10, -24),   S(  1, -27),   S(  5,   2),
            S( -5, -38),   S(  7,  12),   S(  1, -40),   S( 26,  28),   S(  7,  23),   S( -7,  -5),   S(  2,  14),   S(  2,  -4),
            S( -2,  42),   S(  4,  13),   S( -4,  -8),   S( -5,  -3),   S( -1,   2),   S( -8, -18),   S(  1, -19),   S( -2,  -7),

            /* kings: bucket 5 */
            S( 11, -13),   S(  4,  11),   S(-62,  17),   S(-41,  30),   S(-13,  11),   S( 29,  -7),   S( 60,  -3),   S( 52,  -5),
            S( -8,  -5),   S( 71,  -1),   S( 51, -14),   S( 22,  -1),   S(  9,   2),   S( 31,  -4),   S( 21,  12),   S( 37,  -4),
            S(  0, -10),   S(-22,   7),   S(-10,  -7),   S(  4, -11),   S(  1,  -1),   S(-37,  -5),   S( 14,   1),   S( 38, -11),
            S(  6,  -8),   S( 22, -12),   S( 21,  -8),   S(-15,  25),   S( -6,  19),   S(  0,   6),   S(-16,  10),   S(  4,  -1),
            S( -6, -15),   S(-13, -24),   S(  0,   0),   S(  0,   0),   S( -2,  -6),   S(-26,   1),   S(  5,  -3),   S(-10,  -5),
            S(-34, -14),   S(-15,   8),   S(  0,   0),   S(  0,   0),   S( -8,  -6),   S(-37,  18),   S(-22,  17),   S(-21,   9),
            S(-17, -18),   S(  9,  17),   S(  6,  32),   S( -8,  -4),   S(-18,   3),   S(-25,  16),   S( 17,  24),   S( 20,  -4),
            S(-15, -29),   S(  4,  51),   S(  4,  34),   S(  3,  19),   S(-13,   1),   S( -3,  45),   S( -5,  -3),   S( -4,  14),

            /* kings: bucket 6 */
            S( 40, -32),   S(  8,   4),   S(-19,   1),   S( -9,  15),   S(  5,  12),   S(-28,  20),   S( 22,  19),   S( 32,  -6),
            S( 82, -33),   S( 41,   6),   S( 33,  -5),   S( 60, -19),   S( 21,   1),   S(  1,   5),   S( 36,   5),   S( 29,   1),
            S(  2,  -6),   S( -8,   4),   S(  3, -10),   S(-20,  -5),   S(  8, -12),   S(-59,   6),   S(  2,   0),   S(-41,  19),
            S(-12,  12),   S( 23,   0),   S( 15,  -4),   S( 46,  -1),   S( 56,   5),   S( -1,  -2),   S( 83, -25),   S(-20,   4),
            S(-11, -13),   S( -4,   3),   S(-27,  -7),   S( -4,  -3),   S(  0,   0),   S(  0,   0),   S(-35, -13),   S(-61, -15),
            S(-14,   8),   S( -9,  -1),   S(-12,   5),   S(-21,  -8),   S(  0,   0),   S(  0,   0),   S(-13,  33),   S(-58,  -6),
            S(  7,   1),   S(-14,  11),   S(-16,  21),   S( -5,  13),   S(  3, -15),   S(  1,   1),   S( -6, -14),   S(-13,   7),
            S( -3,  12),   S( -1,   7),   S(  1,  12),   S( -5,  10),   S(  6,  31),   S( -1,  15),   S(  0,  39),   S( -7,  11),

            /* kings: bucket 7 */
            S( 36, -39),   S( -4,   6),   S( 39, -33),   S(-46,  20),   S(-29,   6),   S(-35,  34),   S(-10,  34),   S(-18,  25),
            S(  1,  14),   S( 34, -22),   S(-20,  -4),   S(  7,  -8),   S( -2,   3),   S(-31,  24),   S(  6,   6),   S( 14,  13),
            S( 30, -15),   S(-28,   1),   S(-56,  13),   S( -3, -11),   S(-31,  -4),   S(-60,  16),   S(-28,  14),   S(-56,  18),
            S( -8,   5),   S( 32, -17),   S( 16,  -5),   S(  2,   5),   S( 14,   6),   S( 55, -19),   S( 39, -14),   S( 48, -21),
            S(-10, -11),   S( -9,   9),   S(-17,  -6),   S(-33,   7),   S( -3, -18),   S( 25, -12),   S(  0,   0),   S(  0,   0),
            S( -1, -34),   S( 31,  -5),   S( 36, -10),   S( 16,  -7),   S( 15,  -7),   S(  0,   9),   S(  0,   0),   S(  0,   0),
            S( 20,  18),   S( 15,  -2),   S( 11,   1),   S( -9, -32),   S( 31, -28),   S(-10,   5),   S(  0,  30),   S( -7, -38),
            S(  5,   5),   S( -5, -37),   S( 23,  20),   S(  8,  -8),   S(  3,   2),   S(-11, -25),   S(  0,   2),   S(-16, -45),

            /* kings: bucket 8 */
            S( -4, 108),   S(-16,  69),   S(-30,  52),   S(-11,   1),   S(-21,   7),   S( -6,  -4),   S( 43, -20),   S( -9,  16),
            S( 14,  86),   S( 16,   4),   S( -7,  66),   S( 10,   9),   S(  7,  22),   S( 12,   1),   S( 36,  -4),   S( 20,  30),
            S(  0,   0),   S(  0,   0),   S( 10,  38),   S( 25,   6),   S( 14,   0),   S(  7, -12),   S(  3,   5),   S(-13,   1),
            S(  0,   0),   S(  0,   0),   S(  9,  18),   S( 23, -15),   S( 11,  26),   S(  6,  -9),   S(  0,   1),   S( -1,  33),
            S(  2,   0),   S(  4,  12),   S(  5, -19),   S( 16,  -3),   S(  3, -19),   S(-10,  -8),   S(  4,   1),   S(-14, -31),
            S(  0,   5),   S( -5, -16),   S( -7,  10),   S(  5, -16),   S(  1,  -8),   S(  3, -12),   S(-17,   4),   S(  8,  13),
            S( -5, -21),   S( -6, -37),   S(  9, -14),   S(  0,  -6),   S(  0, -51),   S(  8,  -3),   S( 13,  16),   S( -9, -74),
            S( -1,  -4),   S( -5, -27),   S(  1, -19),   S(-10, -30),   S(  3,  34),   S( -3, -17),   S(  3,   7),   S(  1,  -9),

            /* kings: bucket 9 */
            S(-33,  53),   S(-49,  40),   S(-64,  50),   S(-72,  41),   S(-68,  40),   S(-54,  33),   S( 59,   0),   S( 34,  29),
            S(-26,  22),   S( 37,  20),   S(-16, -11),   S( 22,   8),   S( 41,  13),   S( 14,   2),   S( 18,  13),   S( 24,  -7),
            S(-25,  20),   S(  4,   7),   S(  0,   0),   S(  0,   0),   S(  9,  21),   S(-17,  -8),   S( 10, -12),   S(-10,   6),
            S(-13, -30),   S(  1,  -2),   S(  0,   0),   S(  0,   0),   S( 28,   5),   S( 47,  -3),   S(  1,  13),   S( -3,  11),
            S(-10,   6),   S( -1,  -5),   S(  0,   8),   S( 12,  -7),   S( -1, -11),   S(  7, -12),   S(  2,   8),   S( -5,  -1),
            S(  7,  16),   S(-14,  12),   S(  1,  22),   S(-12, -22),   S( -7,   1),   S( -6,  36),   S(  1,  -1),   S( -7,  33),
            S(  4,  -8),   S( -9,  -7),   S(  6,   2),   S( 10,  23),   S(  6,  39),   S( 23,  -1),   S(  0, -59),   S(  2,   9),
            S(  8,  36),   S(  0,   7),   S(  2, -13),   S(  6,   0),   S( -6, -45),   S( -2,   2),   S(  6,   5),   S( 11,  30),

            /* kings: bucket 10 */
            S(-11,  23),   S( -5,  14),   S(-13,  19),   S(-36,  25),   S(-63,  24),   S(-132,  51),  S(-13,  38),   S(-71,  85),
            S( 12,  -5),   S( 28,  20),   S(  7,  -9),   S(  0,  11),   S( 78,   4),   S( 19,   9),   S( 34,  17),   S(-30,  30),
            S(  9,  30),   S(-15,   1),   S( 17,  -8),   S( 12,   5),   S(  0,   0),   S(  0,   0),   S( -9,  -3),   S(-37,  17),
            S(  7, -11),   S(  9, -10),   S( 18,  -9),   S( 28,   4),   S(  0,   0),   S(  0,   0),   S( 20,   7),   S( -9,   8),
            S( -4,  -3),   S( 24,   9),   S( 13,   5),   S(  1, -28),   S(  0, -14),   S( -3,   4),   S( 11,  11),   S(-12,  -6),
            S( -9,  22),   S( -8,  17),   S(-12,  23),   S(  5, -14),   S(  4,  15),   S( -1,  -5),   S(-15,  25),   S( -2,  26),
            S(  6, -17),   S(  9,   4),   S( 11,   5),   S( 18,  29),   S(  8,   7),   S( -2,   5),   S( 25, -18),   S(  1,  23),
            S( -1,   7),   S(  7, -26),   S(  2,  -4),   S( -4, -13),   S( -5, -14),   S( -1, -28),   S(-10, -40),   S(  9,  56),

            /* kings: bucket 11 */
            S(-24,  60),   S(  3, -13),   S( 20,   5),   S(-17,   3),   S(-37,  18),   S(-155,  72),  S(-65,  75),   S(-141, 145),
            S( 11, -38),   S(-19,  21),   S(-13, -27),   S(  5,  15),   S( 25,   5),   S( 35,  42),   S( 70,  -6),   S(  3,  41),
            S( -7, -16),   S( 15, -14),   S( -3,  -5),   S(  8,   1),   S( 69,  -8),   S( 24,  24),   S(  0,   0),   S(  0,   0),
            S( 24,  28),   S(  5,  11),   S( -3,  -4),   S( 45, -12),   S( 40, -17),   S( 34,   2),   S(  0,   0),   S(  0,   0),
            S(  5,  -7),   S(-12,   2),   S( -9,  -8),   S( 13,  -4),   S( 19, -18),   S( 12,  -4),   S(  9, -13),   S(  6,  20),
            S( 10,  20),   S( 17,   0),   S( -2, -26),   S(  7,   1),   S( -4, -35),   S( -9,   2),   S( -8, -12),   S( -7,   5),
            S(  9, -17),   S( 14, -22),   S(  7,  18),   S(  6, -23),   S(  8,   2),   S( 10,  -9),   S(  6,  -9),   S( -4, -17),
            S( 15,  27),   S(  3, -11),   S( -3,   2),   S(  5, -11),   S( -8, -27),   S( -5,  -2),   S(  3,  -3),   S(  9,  26),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,  73),   S(-13,  -2),   S( -9,  -6),   S(  3,   1),   S( 29,  13),   S(  6,  45),
            S(  0,   0),   S(  0,   0),   S( 20, 103),   S( -6,  -7),   S( 19,  33),   S( 15,  -3),   S( -1,  10),   S(-18,  15),
            S( -1,  -3),   S(  0, -40),   S( 20,  16),   S( 11,  21),   S( 15,  -9),   S( -3,   3),   S( -3,  16),   S(-11,  -1),
            S( -5, -16),   S(  2,  30),   S(-11, -23),   S( -2, -35),   S( -9, -28),   S(  8,   3),   S(-18,   3),   S( -4,  -2),
            S( 10,  22),   S( -3,  26),   S(  3,  -6),   S(  7,   7),   S( -4,  26),   S( -9,  -3),   S(-21, -11),   S( -7,   2),
            S(  8,  32),   S(  2,   8),   S( -6, -17),   S( -7, -13),   S(  5,  27),   S(-13, -18),   S(-13,   0),   S(  0,  31),
            S(  7,  18),   S( -5, -14),   S( -1, -11),   S(  1,  -2),   S( -1,  11),   S( -5, -20),   S( 12,   6),   S(  5,   3),
            S( -1,  -1),   S( -1,  -3),   S(  2, -20),   S(  1,  -5),   S( -1, -17),   S(  3,  -4),   S(-19, -47),   S(-10, -29),

            /* kings: bucket 13 */
            S(-18,  78),   S( -3,  71),   S(  0,   0),   S(  0,   0),   S( -1,  74),   S(-26,  10),   S(  9,  -3),   S(  3,  44),
            S( -7,  12),   S( -8, -13),   S(  0,   0),   S(  0,   0),   S( 17, -21),   S( 15, -18),   S(-41,  26),   S( -2,  18),
            S( -6,  -1),   S(  1,  26),   S( -5, -45),   S(  0,  24),   S(  4,  22),   S(  1,   1),   S(-18,   4),   S( -4,   5),
            S( -8, -10),   S( -4,   8),   S( 11,  -4),   S(-11, -54),   S(  8, -47),   S( 12, -33),   S(  2,  23),   S(-12, -12),
            S(  9,  34),   S(  1,  20),   S(  7,  23),   S(-13, -41),   S( -9,  -7),   S( -9,   8),   S(-10,  -6),   S(  4,  22),
            S( -1,   7),   S( -8,   7),   S(-11,  -8),   S( -7,  15),   S( -5, -11),   S( -4,  -7),   S( -8,  -8),   S(  0,   7),
            S(  7,  24),   S(-12,  -4),   S( -2,  -2),   S(  2,   1),   S( -7, -37),   S(  0,   7),   S(  0, -30),   S(  6,  13),
            S(  4,   3),   S( -2,  15),   S(  4,  10),   S( -3,  -9),   S(  2,   9),   S( -6, -13),   S( -7, -13),   S( 10,  39),

            /* kings: bucket 14 */
            S( 10,  68),   S(-17, -14),   S( -6,  -2),   S( -9,   8),   S(  0,   0),   S(  0,   0),   S(  3,  90),   S(-39,  87),
            S( -9,  -4),   S(-11,  -1),   S( -1,  -3),   S(  8,   0),   S(  0,   0),   S(  0,   0),   S(-12,  15),   S(-38,   0),
            S( -5,  -7),   S( 13,   4),   S( 15, -13),   S( 18,   6),   S(  6, -15),   S( -1,  28),   S( -3,  33),   S(-20,  -3),
            S(  6,  14),   S( -5, -12),   S( 10, -17),   S( 17, -41),   S(-18, -47),   S(  4,   1),   S(  4,  11),   S(  6,   5),
            S( -1,   5),   S( -5,  -6),   S( -8,  13),   S( -6, -40),   S( -2,  13),   S(  4,  20),   S(  9,  38),   S( -1,  25),
            S(  0,   7),   S( -2,  32),   S( -3,   2),   S( -4,  31),   S( -1,  11),   S( -1, -15),   S(-18, -47),   S(  5,  20),
            S(  4,   2),   S(-10, -47),   S( 12,  36),   S(  0, -11),   S( -1,  -2),   S( -4, -17),   S( -4, -58),   S(  7,  45),
            S(  5,  30),   S(  5,  45),   S( -1,  -4),   S(  0,  14),   S(  7,  32),   S( -2, -14),   S(-10, -55),   S(  1, -13),

            /* kings: bucket 15 */
            S( 11,  42),   S(  6, -10),   S(  2, -13),   S( -9, -30),   S(-30,  13),   S(-15,  87),   S(  0,   0),   S(  0,   0),
            S( -7, -29),   S(-17,  -7),   S(  4, -19),   S( 33,  60),   S( 36, -31),   S( 25,  81),   S(  0,   0),   S(  0,   0),
            S(-19, -15),   S(  7,  15),   S( -8, -22),   S(-16,   0),   S( 10, -34),   S( 13,  38),   S( 18,  37),   S(-10, -15),
            S( 12,   7),   S( -3,   9),   S(  5,   4),   S(-10, -14),   S( -4, -51),   S(  8,   7),   S( 16,  53),   S( -2, -28),
            S( 11,  12),   S(-16,  -8),   S(-10,   0),   S( -4, -18),   S(  6, -10),   S( -9,  21),   S( -6,  -3),   S( -1,  16),
            S(  5,  39),   S( -7,  19),   S(-13,  -6),   S(  7,  41),   S(-19, -27),   S( -3,  -6),   S( -9,   2),   S(  3, -12),
            S( 14,  48),   S( -1,  26),   S( -4, -16),   S( -7, -11),   S(  6,   3),   S(-10,   2),   S( -1, -12),   S( -1,   1),
            S( -9, -22),   S(  8,  14),   S(  3,  18),   S( -1,   0),   S(  4,  39),   S(  6,  40),   S(  1,  12),   S(  1,  -1),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S( -6, -178),  S( 15, -32),   S( 24,  38),   S( 31,  72),   S( 38,  93),   S( 42, 113),   S( 48, 110),   S( 58,  97),
            S( 85,  62),

            /* bishop mobility */
            S(  6, -70),   S( 11,   1),   S( 21,  39),   S( 27,  72),   S( 32,  96),   S( 36, 112),   S( 38, 119),   S( 42, 116),
            S( 44, 118),   S( 52, 107),   S( 55,  96),   S( 83,  80),   S(100,  70),   S(141,  43),

            /* rook mobility */
            S(-14, -18),   S( -6,  50),   S( -7,  84),   S( -3,  98),   S( -6, 122),   S( -1, 131),   S(  1, 142),   S(  6, 142),
            S(  6, 152),   S( 12, 154),   S( 16, 156),   S( 26, 153),   S( 32, 151),   S( 46, 145),   S(130,  99),

            /* queen mobility */
            S( 60, -31),   S( 68, -99),   S( 59,  34),   S( 55,  97),   S( 57, 140),   S( 54, 190),   S( 54, 228),   S( 54, 247),
            S( 55, 264),   S( 55, 283),   S( 54, 300),   S( 51, 319),   S( 50, 335),   S( 48, 344),   S( 43, 358),   S( 47, 354),
            S( 56, 352),   S( 60, 347),   S( 71, 336),   S( 94, 307),   S(113, 294),   S(159, 259),   S(172, 239),   S(210, 204),
            S(162, 239),   S(177, 225),   S(116, 205),   S( 80, 178),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  7,  28),   S(-18,  58),   S(-32,  57),   S(-14,  67),   S( 20,  30),   S(-14,  38),   S(  2,  75),   S( 30,  34),
            S( 15,  41),   S(  2,  63),   S( -9,  53),   S( -1,  40),   S(  0,  45),   S(-41,  65),   S(-35,  83),   S( 36,  36),
            S( 35,  68),   S( 26,  73),   S( 17,  57),   S( 37,  43),   S(  8,  50),   S(-26,  68),   S(-24, 100),   S( 15,  67),
            S( 59,  89),   S( 69, 101),   S( 44,  69),   S( 22,  53),   S( 13,  65),   S( 24,  83),   S(-21, 115),   S(-47, 124),
            S( 73, 131),   S( 98, 153),   S(103,  92),   S( 60,  93),   S(-34,  85),   S( 67,  93),   S( -7, 150),   S(-43, 143),
            S(156, 182),   S(152, 208),   S(195, 196),   S(201, 184),   S(197, 198),   S(196, 200),   S(196, 207),   S(205, 203),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 14,  13),   S(  3,   1),   S(  8,   4),   S( 11,  23),   S( 23,  41),   S(  2, -25),   S(-15,  36),   S(  9, -32),
            S( -8,  39),   S( 27,   5),   S(  1,  47),   S( 19,  33),   S( 40,   4),   S( -4,  33),   S( 21,   0),   S(  3,  12),
            S( -8,  32),   S( 20,   3),   S( -1,  60),   S( 16,  65),   S( 27,  35),   S( 26,  21),   S( 31,   9),   S( -3,  18),
            S( 17,  32),   S( 11,  48),   S( 52,  76),   S(  4,  96),   S( 81,  62),   S( 65,  48),   S( 27,  51),   S( 24,  20),
            S( 52,  51),   S(112,  88),   S( 89, 116),   S(119, 160),   S(131, 147),   S(118, 125),   S(155,  94),   S( 68,  20),
            S( 76, 201),   S(122, 279),   S(101, 208),   S(105, 201),   S( 82, 172),   S( 68, 164),   S( 52, 176),   S( 11,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 17,  41),   S( 23,  38),   S( 42,  50),   S( 40,  35),   S( 30,  33),   S( 27,  37),   S(  8,  24),   S( 43,  12),
            S( -2,  29),   S( 13,  45),   S( 13,  40),   S( 11,  49),   S( 24,  17),   S(  8,  24),   S( 25,  23),   S(  1,  16),
            S( -1,   6),   S( 20,  34),   S( 48,  43),   S( 39,  35),   S( 43,  37),   S( 63,   8),   S( 17,  24),   S( 24,   0),
            S( 59,  42),   S(120,  13),   S(119,  86),   S(159,  91),   S(161,  78),   S( 94,  79),   S( 97,   4),   S( 85,  -9),
            S( 89,  22),   S(151,  45),   S(157, 148),   S(181, 162),   S(133, 180),   S(127, 160),   S(200,  98),   S(-37,  99),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 21,  26),   S(  6,  19),   S( 10,  37),   S( -2,  45),   S( 63,  38),   S( 27,   2),   S(  4,  -2),   S( 32,  11),
            S(  2,  15),   S(  5,  11),   S( 18,  17),   S( 17,  28),   S( 16,  15),   S( -3,   9),   S(  6,   5),   S( 29,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -15),   S( -5, -11),   S(-18, -17),   S(-17, -28),   S(-16, -15),   S(  3,  -9),   S( -6,  -5),   S(-29,   5),
            S(-21, -26),   S( -6, -19),   S(-10, -37),   S(  2, -45),   S(-63, -38),   S(-27,  -2),   S( -4,   2),   S(-32, -11),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-19, -21),   S( -9, -14),   S( -3, -30),   S(-52,  -2),   S(-20, -15),   S(-19, -21),   S(  2, -27),   S(-19, -35),
            S(-22,   4),   S(-17,   2),   S(-25,   6),   S(  1, -16),   S(-31, -14),   S(-27,  -3),   S(-43,   7),   S(-17, -11),
            S(-17, -13),   S( -5, -20),   S(-25,  -1),   S(-28, -10),   S(-28, -27),   S(-26,  -9),   S(-23,  -4),   S(-44, -17),
            S( -6, -32),   S( 17, -38),   S( 15, -21),   S(  8, -27),   S(  3, -20),   S( 66, -40),   S( 31, -30),   S(-12, -45),
            S( 23, -56),   S( 55, -74),   S( 55, -34),   S( 68, -33),   S( 62, -28),   S( 81, -33),   S(137, -81),   S( 25, -60),
            S(145, -118),  S(173, -124),  S(131, -81),   S(116, -46),   S( 90, -28),   S(136, -30),   S(109, -60),   S( 92, -88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   2),        // attacks to squares 1 from king
            S( 16,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 72, -85),        // king-side castling available
            S( 16,  48),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S(  4, -49),   S(  6, -41),   S( -3, -48),   S(-14, -35),   S(-22, -27),   S(-39, -17),   S(-70,   1),   S(-80,   5),
            S(-91,   9),   S(-71,  11),   S(-87,  26),   S(  9,   3),   S(-11,   7),

            /* orthogonal lines */
            S(-18, -51),   S(-62, -22),   S(-82, -14),   S(-98, -11),   S(-104, -17),  S(-108, -22),  S(-108, -27),  S(-105, -31),
            S(-124, -22),  S(-147, -21),  S(-165, -28),  S(-167, -36),  S(-181, -27),  S(-369,  37),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S(  4, 212),

            /* passed pawn can advance */
            S(-18,  38),   S(-14,  69),   S(  0, 107),   S(  8, 204),

            /* blocked passed pawn */
            S(  0,   0),   S( 36,   0),   S( 43, -12),   S( 46,  17),   S( 55,  33),   S( 52,   2),   S(144, -20),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 66, -31),   S( 30,  39),   S( 16,  45),   S( 16,  71),   S( 41,  89),   S(113, 120),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-10, -18),   S(  0, -35),   S(  5, -53),   S(-15, -50),   S( 13, -53),   S(186, -97),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 11, -13),   S( 38, -44),   S( -5,   9),   S( 16, -45),   S( 10, -138),  S(-28, -249),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 17,  50),   S( 39,  42),   S( 96,  36),   S( 10,  37),   S(219,  75),   S(179,  60),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-29,  94),

            /* bad bishop pawn */
            S( -6, -22),

            /* rook on open file */
            S( 46,  -7),

            /* rook on half-open file */
            S( 14,  20),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 25,  35),   S( 26,  -9),   S( 33,  22),   S( 24,   2),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S(  7,  10),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
