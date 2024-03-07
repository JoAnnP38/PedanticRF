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
        public const int MAX_WEIGHTS = 12761;
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
        public const int TEMPO = 12760;             // tempo bonus for side moving

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

        // Solution sample size: 6000004, generated on Wed, 06 Mar 2024 09:36:39 GMT
        // Solution K: 0.003850, error: 0.085577, accuracy: 0.4975
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S(102, 179),   S(423, 538),   S(436, 569),   S(549, 967),   S(1347, 1648), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 92, -106),  S(147, -89),   S( 21, -28),   S(-32,  48),   S(-36,   9),   S(-27, -10),   S(-57,   0),   S(-44, -18),
            S(106, -111),  S(105, -104),  S( -6, -50),   S( -9, -52),   S(-22, -29),   S(-29, -30),   S(-45, -30),   S(-40, -32),
            S( 91, -80),   S( 71, -57),   S( 16, -51),   S( 10, -63),   S(-11, -53),   S( -1, -58),   S(-24, -44),   S(-25, -40),
            S( 67, -37),   S( 46, -42),   S( 20, -41),   S( 25, -78),   S( -6, -49),   S(-35, -46),   S(-32, -37),   S(-36,  -9),
            S( 75,  28),   S( 12,  30),   S( 72, -21),   S( 60, -66),   S( 34, -26),   S(-31, -19),   S(-33, -11),   S(-41,  50),
            S( 80,  64),   S( 71,  64),   S( 32,  -8),   S( 52, -65),   S(-54, -17),   S(-18, -10),   S(-19,   3),   S(-17,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 30, -26),   S( 25, -34),   S( 46, -24),   S( 13,   0),   S( -3, -21),   S(  1, -20),   S(-46,  -5),   S(-40,  18),
            S( 29, -41),   S( 10, -44),   S( 13, -45),   S(  2, -49),   S(-16, -19),   S( -7, -36),   S(-39, -15),   S(-48, -10),
            S( 14, -21),   S( 19, -29),   S( 21, -43),   S( 23, -62),   S(-16, -23),   S(  7, -40),   S(-23, -22),   S(-27,  -8),
            S( 30,   0),   S( 22, -37),   S( 31, -35),   S( 23, -48),   S(  5, -23),   S(  2, -36),   S(-35, -11),   S(-35,  25),
            S(-15,  69),   S(-68,  13),   S(-12, -11),   S( 20, -30),   S( 17, -12),   S( -8, -14),   S(-31,  10),   S(-51,  72),
            S( 17,  59),   S( 33, -10),   S(-44, -17),   S(-18,  27),   S(-25,  -1),   S(-70,  12),   S(-31,  13),   S(-63,  74),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30,   6),   S(-37,   7),   S(-23,   8),   S(-20,  10),   S(  8, -16),   S( 26, -21),   S( -2, -45),   S(-17, -16),
            S(-33, -10),   S(-53,  -4),   S(-33, -28),   S(-28, -32),   S(  1, -26),   S(  0, -27),   S(-17, -34),   S(-30, -24),
            S(-40,  -1),   S(-30, -18),   S(-15, -41),   S( -6, -53),   S( -2, -19),   S( 14, -27),   S(-10, -27),   S(-19, -11),
            S(-54,  26),   S(-32, -26),   S(-32, -19),   S( -8, -41),   S( 17, -40),   S(-16, -11),   S(-23,  -1),   S(-23,  12),
            S(-43,  58),   S(-82,  23),   S(-61, -18),   S(-49, -20),   S(  4,  11),   S(-14,   9),   S(-40,  31),   S(-48,  91),
            S(-54,  76),   S(-104,  70),  S(-105, -11),  S(-61, -21),   S(-41,  21),   S(-38,  23),   S(-22,  22),   S(-20,  50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31,  -6),   S(-42,   3),   S(-36,  -2),   S( -9, -39),   S(-12,  -7),   S( 32, -16),   S( 80, -65),   S( 56, -78),
            S(-38, -27),   S(-55, -18),   S(-39, -36),   S(-29, -27),   S(-19, -22),   S(  3, -34),   S( 53, -64),   S( 50, -73),
            S(-39, -22),   S(-22, -44),   S( -8, -61),   S( -6, -62),   S( -8, -43),   S( 17, -42),   S( 26, -48),   S( 53, -53),
            S(-42,  -6),   S(-25, -46),   S(-15, -47),   S(  3, -63),   S( 24, -63),   S(  1, -45),   S( 12, -29),   S( 48, -15),
            S(-18,  33),   S(-42, -10),   S(-11, -47),   S( 13, -58),   S( 73, -49),   S( 63, -42),   S( 60,   4),   S( 38,  62),
            S(-43,  97),   S(-61,  35),   S(-17, -56),   S(-29, -43),   S( 66, -70),   S( 77, -47),   S( 58,  29),   S( 50,  70),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  22),   S(  6, -20),   S( 17,  -5),   S(-25,   5),   S(-36, -20),   S(-42,  12),   S(-80,   2),   S(-60,  -3),
            S(-12,  -3),   S( 24,  -9),   S( 36, -39),   S( 36, -30),   S(  0, -29),   S(-60, -24),   S( -6, -44),   S(-16, -26),
            S( 54, -15),   S( 52,  -7),   S(  6,   5),   S( 25, -28),   S(-55, -17),   S( -2, -46),   S(-49, -28),   S(-20, -18),
            S( 26,  24),   S( 13,  26),   S( 31,  16),   S(  1,   2),   S( 13, -35),   S(-64,  -7),   S( -9, -28),   S( 27, -27),
            S(-17,  83),   S( 44,  61),   S( 27,  19),   S(-31,   7),   S( 27,   5),   S(  2, -16),   S(-21, -21),   S( 36,  20),
            S( 85,  95),   S( 69,  53),   S( 43,   7),   S( 15, -16),   S( 11, -36),   S(  1, -34),   S(  2,   2),   S(-26,  34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-80,  50),   S(-54,  31),   S(-21,  18),   S( 20,  11),   S( -8,  12),   S(-32,  13),   S(-39,   2),   S(-27,  21),
            S(-62,  21),   S(-68,  15),   S( 17, -10),   S( -6,  15),   S( 13,  -9),   S(-24, -15),   S(-23, -15),   S(-36,   8),
            S(-44,  40),   S(-23,  18),   S( 72, -26),   S( 30, -24),   S( 56, -15),   S(-40,  -2),   S(-17,  -7),   S(-27,  13),
            S(-31,  56),   S(-22,  30),   S( 52,   4),   S( 37,  22),   S( -5,   8),   S(-67,  15),   S(  2,  -9),   S( 21,  14),
            S( 67,  58),   S( 93,  21),   S( 72,  35),   S( 22,  47),   S(-40,  53),   S( 77,  -4),   S( 14,  11),   S(-14,  61),
            S( 83,  45),   S( 74,  21),   S( 71,   1),   S( 17,   2),   S( 48,   6),   S( 28,  -1),   S( 23,  15),   S( 64,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46,  27),   S(-16,   3),   S( -5,   6),   S(-26,  15),   S(-27,   1),   S( -4,  -2),   S(-55,   3),   S(-63,  19),
            S(-36,  -1),   S( -7, -23),   S( -7, -30),   S( 15, -15),   S( 49, -21),   S( 37, -23),   S(-25, -12),   S(-70,  10),
            S(-32,   9),   S(  1,  -9),   S(  8, -20),   S( -3, -17),   S( 41, -12),   S( 77, -32),   S( 11, -16),   S(-48,  21),
            S(-26,  29),   S(-56,  16),   S( 33, -12),   S( 19,  -5),   S( 50,   0),   S( 46,  -3),   S( 61, -11),   S( 33,  13),
            S(-51,  50),   S(-25,   7),   S(-17,   1),   S(  0,   0),   S( 54,  31),   S( 83,  33),   S( 58,  -9),   S( 83,  28),
            S( 85,  24),   S( 47,  29),   S( -7, -22),   S( 30, -47),   S( 47,  -5),   S( 32,  -2),   S( 41, -12),   S( 67,  15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55, -18),   S(-61,  -4),   S( -3, -17),   S(-68,   7),   S(-19, -24),   S( 24, -26),   S(  0, -45),   S(-57, -14),
            S(-52, -30),   S(-34, -45),   S(-23, -47),   S( -6, -43),   S( -5, -31),   S( 39, -43),   S( 42, -41),   S(-27, -23),
            S(-62, -21),   S(-48, -33),   S(-31, -42),   S(  2, -50),   S( -1, -20),   S( 38, -27),   S( 50, -42),   S( 31, -28),
            S(-61, -11),   S(-61, -32),   S(-70, -28),   S(-27, -17),   S(  8, -26),   S( 20,  -6),   S( 33,  -3),   S( 64, -11),
            S(-41,  -4),   S(-28, -33),   S(-20, -38),   S( 10, -43),   S( 39,  -8),   S( 10,  16),   S( 74,  33),   S( 93,  37),
            S( -4,   8),   S(-14, -40),   S( 15, -68),   S( 18, -42),   S(  5, -32),   S( 62, -28),   S( 31,  41),   S( 61,  74),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  59),   S( -7,  23),   S(-13,  33),   S(-11, -26),   S(  1,   5),   S(-59,  22),   S(-52,   3),   S(-34,   5),
            S(-74,  33),   S(-49,  25),   S(-23,  23),   S(-36,   7),   S(-40, -18),   S(-32, -27),   S(-30, -25),   S(-26, -11),
            S(  3,  66),   S( 14,  76),   S( -9,  40),   S( 12,  20),   S( -1,  -6),   S(-73, -19),   S(-49, -44),   S( -6, -22),
            S( 69,  93),   S( 54, 113),   S( 61,  65),   S( 22,  25),   S(-13,  -8),   S(-22, -33),   S(-37, -13),   S( 11, -27),
            S( 55, 117),   S( 74, 100),   S( 43, 130),   S( 63, 121),   S( 15,  17),   S( 13,   9),   S(  4, -25),   S(-25,  -3),
            S( 87, 142),   S( 76, 147),   S( 86, 168),   S( 31,  28),   S(  7,  -3),   S( -7,  -3),   S(-14, -65),   S(  3, -15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71,  59),   S(-57,  40),   S(-24,  16),   S( 18,  29),   S( -1,  10),   S(-69,  25),   S(-86,  16),   S(-113,  36),
            S(-32,  19),   S(-29,  10),   S(-49,  19),   S( 20,  17),   S(-72,   3),   S(-75,   4),   S(-123,   5),  S(-52,   6),
            S(-54,  55),   S(-34,  66),   S(-24,  70),   S(-65,  66),   S(-70,  60),   S(-99,  17),   S(-45, -18),   S(-23,  -4),
            S( 12,  71),   S( 58,  82),   S( 58, 115),   S( 61, 127),   S( -3,  60),   S(-12,  12),   S( 58,  -8),   S( 52, -18),
            S( 96,  61),   S( 64, 116),   S( 42, 114),   S(102, 135),   S( 72, 109),   S( 21,  26),   S( 16, -19),   S( 45, -16),
            S( 38,  58),   S( 61,  85),   S(119, 102),   S(105, 130),   S( 37,  81),   S(  5,  -3),   S(  1, -33),   S(  3, -25),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-97,  17),   S(-122,  14),  S(  0,  12),   S( -1,   2),   S(-20,   3),   S(-112,  44),  S(-116,  34),  S(-68,  26),
            S(-78,  -6),   S(-47, -20),   S(-34,  -7),   S(-67,  15),   S(-46,  30),   S(-12,  21),   S(-141,  33),  S(-105,  18),
            S(-34,  -8),   S(-64,  -3),   S(-12,  -1),   S(-68,  58),   S(-82,  95),   S( -1,  41),   S(-82,  38),   S(-79,  43),
            S( 14, -11),   S( -9,  -4),   S( 24,  27),   S( -2,  71),   S( 46,  93),   S( -4,  96),   S(  7,  34),   S( 51,  20),
            S( 67,  -9),   S( 19, -17),   S( 63,  25),   S( 45, 103),   S( 87, 111),   S( 75,  66),   S( 52,  65),   S( 83,  33),
            S( 46,  -2),   S( 28, -32),   S( 28,  17),   S( 73, 108),   S( 63,  92),   S( 56,  65),   S( 53,  41),   S( 51,  79),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-99,  -3),   S(-76,   3),   S( -4,  -1),   S(-13,  -6),   S( -6,  24),   S(-109,  47),  S(-53,  11),   S(-77,  50),
            S(-95, -15),   S(-69, -33),   S(-28, -41),   S(-83,  18),   S(-60,  10),   S(-14,  16),   S(-79,  41),   S(-89,  39),
            S(-38, -30),   S(-39, -41),   S(-31, -14),   S(-62,  16),   S(-48,  26),   S( -1,  51),   S(-61,  69),   S(-70,  70),
            S( 10, -16),   S(-44, -14),   S(-14, -18),   S(-31,  29),   S(-22,  57),   S( 44,  48),   S( 53,  90),   S(103,  59),
            S(  8,   3),   S(-16, -23),   S( -7,   0),   S(-17,   8),   S( 78,  84),   S( 72,  76),   S(110,  89),   S(157,  71),
            S( 23, -21),   S( -1, -32),   S(  5, -30),   S(  6,  -3),   S( 29,  26),   S( 35, 111),   S( 69, 131),   S(105, 118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33,  13),   S(-23,  16),   S(-35, -33),   S(  6,  14),   S(-14,  -5),   S(  0, -10),   S(-33, -28),   S(-23,  12),
            S(-32, -15),   S( -4,   8),   S(  2,  34),   S( -5, -14),   S(-38,  12),   S(-10,  10),   S(-42, -32),   S( -3, -47),
            S(-10,  62),   S(-14,  47),   S( 30,  35),   S( 16,  33),   S(  6,   2),   S(-36, -35),   S(-29, -57),   S(-16, -59),
            S(  5,  83),   S( 25,  78),   S( 38,  85),   S( 41,  81),   S(-12, -32),   S(-26, -53),   S(  7,  -4),   S(-39, -76),
            S( 68,  79),   S( 86, 191),   S( 58, 119),   S( 18,  38),   S(  6,  15),   S( -3, -20),   S( -6, -58),   S( -8, -52),
            S( 60, 105),   S( 62, 251),   S(111, 251),   S( 63, 129),   S( -2,   9),   S( -7, -49),   S( -4, -45),   S( -6, -71),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -18),   S(-20,   4),   S(  2,  14),   S( -3,  -3),   S(-17, -42),   S( -4, -12),   S(-27, -37),   S(-23,  -7),
            S( -1, -55),   S(-24,  -1),   S( -8, -33),   S(  6,   5),   S(-35,  32),   S(  3, -13),   S(-35, -44),   S(-28, -20),
            S(  4,   8),   S( 23,  -5),   S(  6,  29),   S( 20,  53),   S(  1,  34),   S(-19, -29),   S(-18, -19),   S(-10, -48),
            S( 10,  29),   S( 46,  58),   S( 44,  89),   S( 34,  94),   S( 25,  59),   S( 27, -10),   S( 24, -15),   S( 42, -56),
            S( 42,  61),   S( 80, 112),   S(105, 196),   S(107, 202),   S( 47,  92),   S(  5,  24),   S(  6, -54),   S( 10, -70),
            S( 27,  84),   S( 95, 187),   S( 85, 236),   S(100, 248),   S( 52, 110),   S( 14, -14),   S(  2, -35),   S(  5, -45),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -62),   S(-41, -21),   S( -4, -55),   S( -1,   1),   S( -8, -10),   S(-20,  36),   S(-19,  14),   S( -2,  27),
            S( -5, -20),   S(-30, -43),   S(-17, -27),   S( -4,   7),   S(-13,  29),   S( -5,   0),   S(-16,  23),   S(-26,   5),
            S(-21, -31),   S(-17, -16),   S(-19, -19),   S(  5,  -3),   S( -6,  47),   S( -9,  27),   S( 14,  10),   S(  3,  20),
            S( 14, -40),   S( 36, -13),   S( 20, -27),   S( 13,  65),   S(  6, 125),   S(  7,  82),   S(  0,  44),   S( 29,  16),
            S(  5, -46),   S( 20,  -7),   S( 43,  17),   S( 42,  98),   S( 62, 187),   S( 84, 157),   S( 57,  72),   S( 38,  54),
            S( 13, -14),   S( 19, -14),   S( 48, 102),   S( 61, 168),   S( 78, 215),   S( 49, 169),   S( 57, 141),   S( 31,  58),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55, -48),   S(-25, -20),   S( -6, -27),   S( -8, -21),   S( 13,  31),   S(-11,  37),   S(-36, -24),   S( 12,  37),
            S(-11, -80),   S(-33, -19),   S(-30, -62),   S( 11,  17),   S(-11,  33),   S(  5,  33),   S(  7,  29),   S(  4,  39),
            S( -3, -59),   S( -1, -68),   S(-16, -34),   S(  5,  27),   S( 21,  22),   S( 17,  33),   S(  7,  65),   S(  5,  72),
            S(  4, -43),   S(-36, -49),   S( -5, -19),   S( 19,  17),   S(-13,  48),   S( 38,  66),   S( 23, 105),   S(  6,  92),
            S(-15, -82),   S(-13, -54),   S( -5, -18),   S(  9,   0),   S( 39, 109),   S( 92, 118),   S( 65, 263),   S( 72,  88),
            S(  0, -42),   S( -3, -32),   S(  4,  -4),   S(  3,   2),   S( 38, 106),   S( 70, 224),   S( 28, 217),   S( 47,  84),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-43,  -4),   S( -6,  -1),   S(-32,  12),   S(-17, -35),   S(-51,  -9),   S(-31, -27),   S(-56, -57),   S(-38, -24),
            S(-44,  50),   S( 15, -39),   S(-27, -24),   S(  3, -18),   S( -1, -43),   S(  2, -30),   S(-18, -26),   S(-85, -38),
            S(  1,  88),   S(-20, -24),   S(  7, -18),   S(-22,  13),   S( 19,   1),   S(-27, -10),   S(  1, -53),   S(-47, -44),
            S(  4, -10),   S( 53, -18),   S( 19,   7),   S( 17,  35),   S( 21, -29),   S(-18,  14),   S( -3, -42),   S(  2, -14),
            S( 14, -43),   S( 47, -18),   S( 25,  -7),   S( 67, -27),   S( 32,  -2),   S( 19,  19),   S(  3,  -1),   S(-82,  -8),
            S( -4, -11),   S(  5,   0),   S( 51, -15),   S( 78, -25),   S( 34, -70),   S( 34, -15),   S( -8, -13),   S(-37, -24),
            S(-14, -61),   S( 23, -33),   S( 21, -29),   S( 42, -35),   S( 10, -57),   S(-12, -54),   S(-12, -38),   S( -3, -51),
            S(-82, -40),   S(-13,   4),   S(-10, -26),   S(-31, -70),   S( -5, -14),   S( 15,   9),   S( -6,   6),   S( 10, -11),

            /* knights: bucket 1 */
            S(-46,   3),   S(-41,  87),   S( 33,  23),   S(-16,  49),   S(-20,  50),   S(-42,  35),   S(-43,  52),   S( 38,  14),
            S( 43,   1),   S(  8,  40),   S( -2,  11),   S( -5,  28),   S( -5,  23),   S(  6,   8),   S( 22, -19),   S(-40,  13),
            S(-37,  50),   S(  1,  12),   S( 12,   4),   S( 25,  19),   S( 14,  17),   S(-20,  19),   S(-17,  -2),   S(-22,  -9),
            S( -1,  31),   S( 35,  23),   S( 17,  47),   S( 20,  36),   S(  7,  28),   S(-11,  22),   S( 21, -21),   S(  9,  24),
            S( -9,  37),   S( 22,  14),   S( 24,  39),   S( 43,  18),   S( 29,  31),   S( 10,  17),   S(  9,  20),   S( 12,  14),
            S( 36,   0),   S( 39, -13),   S( 16,  35),   S( 54,  11),   S( 33,   3),   S( 35,  32),   S( 73,  -9),   S( -5,   3),
            S(  5,  13),   S(  8,  29),   S(-18, -32),   S( 23,  20),   S( 26, -40),   S( 63, -35),   S(-19, -10),   S(-24, -34),
            S(-90, -55),   S(-44, -40),   S( -4,  -4),   S(  4,   8),   S(-11,  41),   S(-17, -17),   S( -1, -16),   S(-49, -44),

            /* knights: bucket 2 */
            S(-54,  11),   S( -1,  34),   S(-26,  49),   S(-21,  53),   S(-11,  35),   S(-41,  56),   S(-20,  36),   S(-48,  13),
            S(-17, -24),   S(-12,  15),   S( -4,  24),   S(-11,  31),   S(  0,  12),   S( -9,  30),   S(-28,  51),   S(-33,  79),
            S(-26,  29),   S( -5,  12),   S(-12,  36),   S( 22,  19),   S(  8,  21),   S( -1,  14),   S( -4,  26),   S(-39,  39),
            S(-13,  43),   S(-29,  40),   S(  1,  39),   S( 10,  43),   S( -1,  48),   S( -2,  30),   S(-11,  41),   S( -7,  45),
            S( -9,  30),   S(-23,  37),   S(-22,  58),   S(-21,  56),   S( -2,  44),   S(-26,  49),   S( -7,  36),   S( -5,  23),
            S(-36,  15),   S( 22,  21),   S(-26,  48),   S( 13,  34),   S( 20,  37),   S(-16,  44),   S(-39,  39),   S( 77, -32),
            S( -5,  35),   S(-19, -18),   S( 11,  -9),   S(-27,  22),   S(-21,   2),   S( 39,   0),   S(-76,  47),   S(-15,  -6),
            S(-158,  12),  S(  2, -11),   S(-64,  14),   S(-11,  19),   S( 10,   1),   S(-42,   8),   S(  7, -12),   S(-199, -70),

            /* knights: bucket 3 */
            S(-70,  -3),   S( -1, -20),   S(-45,  22),   S(-11,  16),   S( -9,  15),   S(-21,  14),   S(  7,   2),   S(-21,  10),
            S(-33,  33),   S(-42,   5),   S(-26,   9),   S(  2,  26),   S(  9,  10),   S(-21,  10),   S(-15,   6),   S(-25,  60),
            S(-16, -21),   S(-11,  13),   S( -8,  16),   S( 19,  21),   S( 21,  28),   S(  7,  18),   S( -5,   9),   S(-12,  49),
            S(-18,  14),   S( -2,  30),   S(  3,  49),   S( 13,  46),   S( 18,  50),   S(  6,  45),   S( 24,  17),   S( 14,  29),
            S(  0,  20),   S(-10,  32),   S( 12,  36),   S( 17,  54),   S( 13,  51),   S( 12,  66),   S( 26,  43),   S(  7,  23),
            S( -9,  16),   S( 31, -11),   S( 43,   7),   S( 59,  19),   S( 64,   1),   S( 85,  -6),   S(  3,  19),   S( -7,  45),
            S(-15,  12),   S(  2,  11),   S( 48, -18),   S( 53,  -9),   S( 49, -14),   S( 45, -35),   S( 58, -52),   S( 50, -47),
            S(-126,  12),  S(-28, -14),   S(-22,   8),   S(  2,  11),   S( 51, -12),   S(-51,  -4),   S( -7, -26),   S(-71, -79),

            /* knights: bucket 4 */
            S(  7,   3),   S(-40, -30),   S(  5,  18),   S(-22, -11),   S(-14, -29),   S(-33, -25),   S( 12, -78),   S(-31, -53),
            S( 26,  17),   S(-29,  38),   S(  0, -30),   S( 17, -11),   S( 41, -26),   S(-16, -60),   S(  8, -48),   S(-24, -51),
            S( 11,  47),   S( 57,  31),   S( 37, -12),   S( 40,  -7),   S(  4,  28),   S(-49,   8),   S(-48, -33),   S( 19, -58),
            S(  3,  45),   S( 48,  -7),   S( 80,  -9),   S( 33,  24),   S( 54,  -9),   S(131, -22),   S( 19, -58),   S( 14, -40),
            S( 65,  11),   S(-31,  46),   S( 52,  42),   S( 73,   5),   S( 40,   7),   S(-14,  13),   S(-26, -43),   S(  0, -25),
            S( 13,  25),   S( -4,   3),   S( 95,  13),   S( 36, -14),   S( 21, -12),   S( 24, -13),   S( -1,  24),   S(-15, -26),
            S( -5,  18),   S(-30,  -4),   S( -4,  -7),   S(  5,   9),   S( 31, -12),   S( 24, -25),   S( 13, -22),   S(-21, -14),
            S(  0,  29),   S( -2, -16),   S(  8,  27),   S( -6,  -6),   S(  3,   7),   S( 13,  27),   S( -6,  -9),   S(  0, -15),

            /* knights: bucket 5 */
            S( 13,   5),   S(-38,   4),   S( 58,  29),   S( 17,  29),   S( 24,  17),   S(  4,  -9),   S( 22,  25),   S(-24, -41),
            S(  8, -26),   S( 48,  34),   S( 21,  10),   S( 21,  15),   S( 64,  13),   S(  4,  30),   S( 25,   2),   S(-18, -35),
            S( 23,  23),   S(-12,  34),   S( 58,  13),   S( 81,  12),   S(  4,  27),   S( 30,  13),   S( 12,   3),   S( 13,  -7),
            S( 51,  42),   S( 44,  24),   S( 37,  27),   S(  8,  34),   S( 70,  11),   S( 27,  32),   S( 40,  15),   S(-29,  23),
            S( 27,  51),   S( 69,  15),   S( 63,  37),   S( 59,  28),   S( 91,  30),   S( 47,  27),   S( -2,  20),   S( 32,  18),
            S( 11,  12),   S(  2,  53),   S(  0,  33),   S( 69,  37),   S( 43,  43),   S( 33,  63),   S( 22,   5),   S(-26,   6),
            S( 18,  57),   S(-11,  60),   S( 27,  40),   S( 24,  52),   S( 13,  28),   S( 15,  49),   S( 22,  55),   S(  1, -19),
            S( -3,  21),   S( -9,  -1),   S(  1,  28),   S( 10,   6),   S(  7,  11),   S( -7,  26),   S(  9,  27),   S(-26, -37),

            /* knights: bucket 6 */
            S(  8, -28),   S( -9, -15),   S( 37,  14),   S(-23,  29),   S(-13,  33),   S( 31,  24),   S(-12,  24),   S(-32,  27),
            S( -3, -44),   S( 60, -17),   S( 50,  -8),   S( -6,  24),   S(-16,  49),   S( 82,  18),   S( 25,  38),   S( 21,  23),
            S( -7, -23),   S( 19,  -9),   S( 23,  13),   S( 51,  18),   S(-15,  39),   S(-23,  39),   S( 35,  26),   S( 25,  33),
            S( 40,  -5),   S( 38,   0),   S( 80,  11),   S( 94,  13),   S( 42,  26),   S( 56,  29),   S(  7,  62),   S(-12,  61),
            S(  8,  21),   S( 97, -10),   S( 98,   1),   S( 76,  26),   S(110,  24),   S(137,  19),   S( 67,  26),   S( 18,  50),
            S(  9,  13),   S( 18,  -6),   S( 48,  21),   S( 48,  45),   S( 70,  53),   S( 33,  31),   S( 13,  31),   S( 50,  46),
            S(-14,  20),   S(  8,  21),   S(-35,  30),   S( 39,  33),   S( 24,  50),   S( 37,  32),   S( 20,  56),   S(-27,  -1),
            S(-41,  -5),   S(  3,  30),   S( 34,  29),   S(  5,  32),   S( 25,  33),   S( 10,  20),   S( 27,  56),   S( 14,  26),

            /* knights: bucket 7 */
            S(-34, -49),   S(-185, -35),  S(-66, -62),   S(-64, -16),   S(-20, -30),   S(-32, -10),   S(-41,  13),   S(-15,  -8),
            S(-29, -97),   S(-22, -54),   S(-20, -36),   S(  0, -23),   S(-39,   1),   S(  3, -30),   S( 30,  24),   S( 45,  16),
            S(-73, -79),   S(-46, -46),   S(-36,   3),   S( 23, -16),   S( 27, -16),   S( 33, -15),   S(  4,  30),   S( 34,  45),
            S(-31, -35),   S( 18, -46),   S(  4,   4),   S( 84, -24),   S( 66,  -4),   S( 31, -10),   S( 13,  24),   S( -8,  29),
            S(-36, -32),   S(  7, -44),   S( 97, -28),   S( 84, -12),   S(103,  -6),   S( 97,   7),   S(110,  -3),   S( 96,   7),
            S(  9, -41),   S(  2, -27),   S( -3,  -7),   S( 55,   3),   S(102,  -1),   S( 99,  -3),   S( 85, -23),   S(  3,  21),
            S(-27, -22),   S(-61, -17),   S(-17, -17),   S( 47,   2),   S( 45,  27),   S( 67,  -2),   S(-25,   0),   S(  5,  30),
            S(-43, -38),   S( -7, -14),   S(-30, -26),   S( 23,  17),   S(  3,   1),   S( 23,  11),   S( -4, -29),   S( -3, -17),

            /* knights: bucket 8 */
            S(  0,   4),   S(-10,  -2),   S(  3,  22),   S( -4, -28),   S(  2,   3),   S( -7, -53),   S(-12, -15),   S( -4, -21),
            S(  3,  10),   S( -5,  11),   S( -7, -10),   S(-21, -24),   S(-38, -30),   S(-18, -65),   S(-16, -55),   S(-20, -31),
            S(  5,  22),   S( -4, -15),   S( 18,  15),   S(-13, -10),   S(  9,  -6),   S( -4,  26),   S( -9, -18),   S( -2,  -9),
            S(-20,   1),   S(-16, -13),   S(-16,  -1),   S( -6,  20),   S( 10,  12),   S(  7, -24),   S( -4, -25),   S( -6, -25),
            S( 36,  91),   S( -2,   1),   S( 15,  26),   S( 38,  26),   S( 20,  28),   S(  2,  17),   S( 21,  11),   S(-10, -28),
            S(  8,  44),   S( 16,  43),   S( 44,  10),   S( 30,  28),   S(-10,  -4),   S(  2,  19),   S( -6, -44),   S( -4,  23),
            S( -2,  -6),   S(-10, -14),   S( -2, -18),   S(  7,  -1),   S( 18,  19),   S( 14,  34),   S( -2, -15),   S( -2,  -4),
            S(  9,  21),   S( 16,  34),   S(  3,   0),   S(  4,  19),   S(  8,  32),   S( -8, -33),   S(  4,  13),   S( -2,   4),

            /* knights: bucket 9 */
            S( -9, -33),   S(-13, -24),   S(-19, -64),   S( -5, -16),   S(-18, -58),   S(-17, -33),   S( -1,   7),   S( -2, -20),
            S(-14, -50),   S(-15,  -3),   S(-25, -95),   S(-28,  -9),   S(-15, -32),   S(-18, -41),   S(  5,  25),   S(-23, -68),
            S(  5, -19),   S(-16, -12),   S( -2, -20),   S(  4, -11),   S( -2,  11),   S(-42,  -5),   S(-16,  -7),   S(-13, -12),
            S( -8, -13),   S(-17,  -9),   S(  3,   2),   S( 25,   0),   S( 27,  15),   S(  0,  12),   S(-17, -49),   S(  1, -39),
            S( 16,  40),   S( -6,  -8),   S(  6,  29),   S( 13,  15),   S( 20,  12),   S(-10,  -4),   S(  6, -12),   S( -6, -22),
            S( 11,  17),   S(  1,  12),   S( -5,  31),   S( -1,   9),   S( 30,  13),   S( 10,  14),   S(  7,  20),   S( -9, -33),
            S( -2,   0),   S( -8, -12),   S( 15,  45),   S(  4,  34),   S(  8,  49),   S(-12, -17),   S( -1,  16),   S( -7,  10),
            S( -7, -16),   S(  7,  27),   S(  5,  15),   S( 10,  29),   S(  3,  -5),   S(  3,  14),   S(  0,  11),   S(  1,   0),

            /* knights: bucket 10 */
            S(-22, -63),   S(-26, -88),   S(-10, -53),   S(-14,  -4),   S( -6, -30),   S( -3, -33),   S( -4,  15),   S(  4,   9),
            S( -7, -37),   S( -7, -46),   S(  0,  -7),   S(-18, -32),   S( -8, -26),   S( -5, -36),   S( -1,  20),   S( -2, -15),
            S(-16, -78),   S(-20, -56),   S(-32,  -4),   S(-20, -23),   S( 11, -12),   S(-20,  -8),   S( 10,   4),   S(-10, -31),
            S(-18, -34),   S(-12, -52),   S( -2, -47),   S(  9, -18),   S( 22,  -1),   S( 23,   6),   S( -1,  22),   S(  9,  37),
            S(-14, -52),   S(-23, -49),   S(  8,  20),   S( 47,   3),   S( 37,  12),   S( 18,  30),   S(  8,  14),   S( 29,  68),
            S(-10, -34),   S(-15, -40),   S(  2,  -1),   S(  1,  10),   S( 24,  43),   S( 18,  46),   S( 19,  63),   S( 23,  75),
            S( -3,  -8),   S( -7,  -6),   S(  3,   1),   S( 16,  50),   S( 14,  49),   S(  7,   5),   S(  3,  15),   S(  9,  25),
            S( -1, -12),   S( 11,  37),   S( -5,  -3),   S(  0,   1),   S(  6,  38),   S(  9,  28),   S( 11,  29),   S( -7, -23),

            /* knights: bucket 11 */
            S(  1,   6),   S(-16, -30),   S(-18, -66),   S(-28, -56),   S(-28, -56),   S( -3, -30),   S(  1, -13),   S( -4,   2),
            S(-16, -36),   S(-26, -72),   S(-29, -87),   S( -8, -33),   S(-24, -31),   S(-29, -52),   S(-13, -42),   S( -8, -14),
            S(-25, -96),   S(-27, -58),   S(-37, -26),   S( -1,  -2),   S(-42, -16),   S(-25,  17),   S( 10,  13),   S( -1,   4),
            S(-16, -43),   S( -2, -11),   S(  8,   3),   S(  0,  14),   S( 24,  31),   S(  1,  22),   S( -4,  15),   S(  0,  11),
            S( -8, -21),   S(-32, -71),   S( 15,   0),   S( 16,  -6),   S( -1,  24),   S( 48,  24),   S(  2,   0),   S( 38,  89),
            S( -3,   1),   S(  4, -16),   S( -6,   8),   S( 38,  53),   S( 12,  17),   S( 41,  59),   S( 19,  23),   S( 13,  40),
            S(  7,  33),   S( -3,  -8),   S(  1, -13),   S( 10,  11),   S(  9,  36),   S( -9,  21),   S( -1,  20),   S( 19,  74),
            S( -5, -11),   S( -5, -25),   S(  6,  19),   S(  4,  15),   S(  1,  10),   S(  2,  -5),   S(  5,   8),   S(  8,  36),

            /* knights: bucket 12 */
            S( -4, -11),   S(  1,   7),   S(  3,  -2),   S( -4,  -8),   S( -7, -18),   S(  2,  12),   S(  5,   7),   S( -2, -15),
            S( -1,   0),   S( -1,  -2),   S(  3,  11),   S( -9, -30),   S( -2,  12),   S( -5, -34),   S( -2, -20),   S(  2,   4),
            S( -5, -11),   S(  0,  -3),   S( -7,  -2),   S( -3, -13),   S( -4, -11),   S( -7, -27),   S( -1, -12),   S( -6, -20),
            S( -4, -17),   S( -3,   2),   S( -6, -17),   S(  7,  49),   S(  1,  22),   S( -1,  -2),   S( -5, -19),   S(  1, -10),
            S( 10,  29),   S(  0,   8),   S(-11,  -4),   S( -3,   6),   S(  0,   2),   S(  0,   8),   S( -3,  -7),   S( -6, -21),
            S( -3,   8),   S( -6, -23),   S( 10,  70),   S( -6, -12),   S(  2, -19),   S( -6, -24),   S(  3,   0),   S( -3,  -6),
            S(  0,   5),   S(-19, -34),   S( -5,   4),   S( -4, -13),   S( -3, -13),   S( -2,  -4),   S( -2,  -4),   S(  1,   5),
            S(  3,   9),   S(  4,  36),   S( -1,   1),   S(  3,   9),   S( -1,  -1),   S( -3, -12),   S( -5, -19),   S(  0,   0),

            /* knights: bucket 13 */
            S( -8, -30),   S(  0,  -4),   S( -1,  -6),   S(  2,   5),   S( -4, -24),   S(  1,  -2),   S( -8, -29),   S(  0,   1),
            S(  0,   4),   S(  1,   3),   S(  2, -17),   S( -5,  -7),   S( -7, -33),   S( -2, -16),   S(  0,   1),   S(  1,  -6),
            S( -2, -15),   S( -8, -31),   S(  4,   7),   S(  8,  15),   S(-15, -36),   S( -8, -21),   S( -1,  -8),   S( -8, -31),
            S( -7,   0),   S( 10,  38),   S(  5,  56),   S(  9,  21),   S(  3,   3),   S( 12,  26),   S(  3,  -3),   S( -2,   5),
            S(  2,  -5),   S(  1,  21),   S( 10,  -6),   S( 14,  56),   S( -1,  23),   S(  1,  22),   S(  0,  -9),   S(  4,  18),
            S(  1,   7),   S(  3,  19),   S( -2,  43),   S( -4,  51),   S( 12,  40),   S( -4, -16),   S(  2,  -1),   S( -6, -21),
            S(  1,   8),   S(  8,  28),   S( 14,  24),   S(  9,  49),   S( 18,  68),   S( -4,  -7),   S( -5,  -7),   S(-10, -25),
            S(  0,   2),   S(  3,  49),   S( -3,   5),   S(  0,   3),   S(  1,  10),   S(  1,   5),   S( -2,  -3),   S(  1,   5),

            /* knights: bucket 14 */
            S( -4, -20),   S( -6, -28),   S( -6, -10),   S( -5,  -5),   S(-11, -48),   S( -2, -17),   S( -2,  -9),   S( -1, -12),
            S(  0, -16),   S( -3, -11),   S(-12, -51),   S( -8, -41),   S( -6, -29),   S(  1,  -4),   S(  3,  17),   S(  0,  -3),
            S( -3, -16),   S( -8, -29),   S( -9, -20),   S(  9,  24),   S(  2,   3),   S( -6, -20),   S(  6,   9),   S( -2,  -3),
            S(  0,   3),   S( -4, -23),   S( -6, -30),   S(-18, -41),   S(  2, -10),   S( 11,  13),   S(  6,  28),   S( -7, -12),
            S(  0,  11),   S( -6, -30),   S( -2,   9),   S( -1, -25),   S(  5,   6),   S(  3, -36),   S(  4,  10),   S( -6, -39),
            S( -1,  -6),   S(  7,   7),   S(  4,  27),   S( 10, -11),   S(  4,  10),   S( -7,  15),   S(  4,  19),   S(  2,  15),
            S(  1,   1),   S( -3, -11),   S( 11,  20),   S( -3,   4),   S(  1,  40),   S( -6,   8),   S(  0,  42),   S(  0,   6),
            S(  0,  -4),   S(  3,  12),   S(  1,  18),   S(  1,   0),   S(  9,  30),   S(  0,  16),   S(  2,  19),   S(  1,   5),

            /* knights: bucket 15 */
            S( -2, -11),   S(  1,  -4),   S( -3, -25),   S( -9, -28),   S( -1,  -2),   S( -1,  -2),   S(  0,  -3),   S(  3,  15),
            S( -3, -15),   S(  2,   3),   S(  1,  -9),   S( -8, -45),   S(  7,  14),   S(  0,   8),   S( -1,  -7),   S( -1,  -8),
            S( -8, -32),   S( -8,  -4),   S( -3, -34),   S(-13, -35),   S(  1,  -7),   S( -2,   6),   S( -1,  -7),   S( -1,  -4),
            S( -4,  -5),   S(-10, -51),   S(  0, -37),   S(  1,  -2),   S(  0, -24),   S(  9,  25),   S(  8,  18),   S( -3, -13),
            S(  1,   5),   S( -1,   4),   S(  3,  -9),   S(-11, -43),   S(  6,   9),   S(  6,  10),   S( -4, -18),   S( -5, -23),
            S( -4,  -6),   S( -5, -13),   S( -3, -17),   S(  1,  -1),   S( -4,  -8),   S( -3,   3),   S( -2,   9),   S(  1,  25),
            S( -2,  -7),   S( -3,   1),   S(  0,   1),   S( -2,  -1),   S(-11, -16),   S(  9,  38),   S( -1,  -7),   S(  4,  28),
            S( -1,  -8),   S(  0,   5),   S( -2,  -7),   S(  4,  -6),   S( -1,  -9),   S(-10, -34),   S( 15,  64),   S(  1,   3),

            /* bishops: bucket 0 */
            S( 53,  17),   S(-23,  25),   S( 59,  12),   S( -8,  26),   S( -8,   4),   S( 15, -22),   S( 33, -49),   S( 51, -49),
            S( 52, -13),   S( 85,  36),   S( 37,  19),   S( 23,  -3),   S( -4,  40),   S(  8,   5),   S(-26,  31),   S(  2, -14),
            S( 34,  41),   S( 43,  33),   S( 25,  44),   S(  6,  67),   S( 15,  35),   S( -3,  46),   S(  5,  -5),   S( 34, -60),
            S( 27,  27),   S( 73,  11),   S( 61,   3),   S( 44,  28),   S(  4,  50),   S( 33,  26),   S(  4,  32),   S( -7,   3),
            S(  7,  21),   S( 47,   7),   S(-10,  61),   S( 50,  48),   S( 54,  22),   S(-15,  42),   S( 32,   5),   S(-42,  29),
            S(-15,  42),   S(-11,  57),   S( 35,  39),   S( 37,  44),   S( 25,  66),   S( 10,  19),   S(-50,  73),   S( 25,  32),
            S(-39,  69),   S(  0,  36),   S( 24,  30),   S(-34,  80),   S(-58,  30),   S( 52,   8),   S( 23,  35),   S(-27,  15),
            S(-16,   6),   S( -6,  53),   S( -4,  41),   S(  0,  27),   S(  6,  19),   S( 16,  26),   S(-17,  61),   S(-10,  26),

            /* bishops: bucket 1 */
            S( 17,  75),   S(  1,  20),   S( 15,  34),   S( -7,  28),   S( 24, -10),   S( -5,  24),   S(-12,  17),   S(-53,  14),
            S( 15,   3),   S( 36,  31),   S( 41,  15),   S( 30,  16),   S( -2,  14),   S( 12,  11),   S(-17,  20),   S( 28,  -3),
            S( 37,  19),   S(  4,  29),   S( 55,  30),   S( 11,  32),   S( 10,  49),   S( -7,  40),   S( 28,   7),   S(-11,   8),
            S( 49, -13),   S( 10,  48),   S(  5,  33),   S( 30,  49),   S(-28,  63),   S( 16,  34),   S(-13,  36),   S( 25,  -6),
            S( 39,  46),   S(  7,  44),   S( 13,  54),   S( -6,  62),   S( 10,  49),   S(-44,  59),   S( 31,  18),   S(-18,  31),
            S(-22,  51),   S( 20,  56),   S(  9,  60),   S( 41,  36),   S(  7,  52),   S(  2,  55),   S(-26,  50),   S( 34,  27),
            S( 15,  40),   S( 19,  18),   S(  9,  65),   S( 37,  57),   S( 18,  54),   S(-31,  60),   S( -3,  77),   S(-49,  58),
            S( 29,  58),   S(-36,  37),   S(-37,  42),   S(-13,  35),   S( -8,  36),   S(  6,  44),   S(-14,  54),   S(-27, 101),

            /* bishops: bucket 2 */
            S( 18,  28),   S( -9,  31),   S( -4,  29),   S(-21,  48),   S(-12,  41),   S(-23,  27),   S(-45,  -4),   S(-71,  60),
            S(  1,  23),   S( 18,  26),   S( 14,  19),   S( -1,  33),   S( -7,  36),   S( 10,  -1),   S(  4,   7),   S( -2, -20),
            S( -4,   6),   S(-17,  46),   S(  9,  63),   S(-10,  59),   S( -6,  53),   S(  7,  41),   S(  1,  33),   S(-18,   0),
            S( 12,  28),   S(-19,  55),   S(-25,  57),   S( -5,  69),   S(-10,  72),   S( -6,  55),   S(  5,  36),   S( -2,  12),
            S(-28,  41),   S(-15,  55),   S(-31,  52),   S(-39,  73),   S(-26,  77),   S(-19,  61),   S(  4,  38),   S(-24,  23),
            S(-30,  50),   S(-38,  50),   S(-33,  69),   S(-26,  63),   S( 11,  39),   S(-17,  67),   S( 22,  61),   S( 10,  37),
            S(-55,  50),   S(-17,  57),   S(-41,  72),   S( 23,  24),   S(-32,  62),   S(-41,  40),   S(-71,  72),   S(-10,  23),
            S(-98, 104),   S(-48,  78),   S(-74,  59),   S(-51,  53),   S(-67,  43),   S(-67,  63),   S(-15,  37),   S(-65,  50),

            /* bishops: bucket 3 */
            S(-14,  48),   S( -1,  29),   S(  9,  36),   S(-10,  42),   S( -2,  34),   S( 38,   4),   S( 15,  -8),   S( 41, -21),
            S(  7,  19),   S( 14,  39),   S( 18,  14),   S(  5,  53),   S(  8,  39),   S(  7,  43),   S( 49,  34),   S( 24,  -9),
            S( 20,  18),   S( -8,  61),   S( 11,  72),   S( 11,  53),   S(  6,  77),   S( 11,  66),   S( 24,  45),   S( 28,  14),
            S( 21,  23),   S( -2,  64),   S( -6,  79),   S( 12,  80),   S( 18,  72),   S( 16,  58),   S(  6,  63),   S( 11,  14),
            S(-15,  51),   S( 16,  50),   S(  3,  60),   S( 15,  82),   S( 11,  77),   S( 30,  54),   S( 12,  61),   S( 13,  64),
            S( -3,  49),   S( 22,  43),   S( 19,  61),   S( 29,  50),   S(  7,  60),   S( 47,  62),   S( 23,  45),   S(  1,  84),
            S( -8,  42),   S(-23,  63),   S( 35,  44),   S(  6,  61),   S( -7,  60),   S( -2,  50),   S(  1,  78),   S(-11,  83),
            S(-58, 103),   S(-39,  59),   S( 36,  51),   S(-15,  68),   S(-45,  64),   S(-62,  86),   S(  2,  52),   S( 70,  22),

            /* bishops: bucket 4 */
            S(-11, -25),   S(-36,  -6),   S(-46,   0),   S(-31,  31),   S(-24,  29),   S(-31,  17),   S(-24,   8),   S(-14, -41),
            S(-23,  23),   S( -6,   3),   S( -3,  27),   S(-24,  15),   S(-45,  23),   S( 60,  -9),   S( 10,  -5),   S( 22, -19),
            S(-17,  -6),   S(-45,  17),   S( 25,  18),   S( -4,  17),   S( 56,  11),   S( 25,  10),   S(-35,   4),   S(-44,  12),
            S(-14,  41),   S( 14,  31),   S( 53,  30),   S( 59,  19),   S(-31,  42),   S( 46,  17),   S( 33,  33),   S( 17, -23),
            S( -7,  11),   S(  6,  45),   S(  7,  62),   S( 38,  26),   S(  2,  21),   S(-33,  30),   S(-39,   7),   S(-11,   6),
            S(-28,  41),   S( 43,  15),   S( 24,  31),   S( 38,   8),   S( 13,  47),   S(  8,  22),   S( 16,  -5),   S(  0,  11),
            S(-24,  25),   S( 30,  43),   S(-11,  16),   S( -6,  42),   S(-10,  14),   S( 13,  18),   S(  2,  21),   S( -3, -16),
            S(  5,  33),   S(-25, -14),   S(  7,  33),   S(-15,   4),   S(-17,   7),   S(  7,  12),   S( -2,  -4),   S(  4,  39),

            /* bishops: bucket 5 */
            S(-13,   2),   S( -1,  23),   S(-43,  39),   S(-26,  27),   S(-23,  30),   S(  4,  -6),   S(-28,  43),   S( 21,  25),
            S(-14,  23),   S(-56,  55),   S( -9,  47),   S(  1,  32),   S(  2,  22),   S(-29,  31),   S(-37,  22),   S(  4,  -1),
            S(-27,  35),   S(-31,  64),   S( 40,  37),   S( 31,  22),   S( 22,  33),   S(-24,  39),   S(-20,  36),   S( 19,  -7),
            S( 38,  29),   S( 12,  48),   S(  7,  39),   S( 34,  39),   S( 52,  36),   S( 27,  41),   S(  4,  21),   S(-23,  34),
            S( 56,  44),   S( 35,  33),   S( 78,  26),   S( 73,  30),   S( 78,  11),   S( 40,  40),   S(  7,  39),   S(-24,  32),
            S( 22,  31),   S( 45,  33),   S( 71,  43),   S( 41,  40),   S( -8,  45),   S( 31,  19),   S( -3,  27),   S(  9,  32),
            S(-13,  30),   S(-25,  43),   S( -6,  42),   S( 16,  47),   S( 14,  53),   S( -4,  68),   S(-12,  51),   S(  4,  21),
            S(-14,  44),   S(  6,  27),   S( -8,  30),   S( -9,  36),   S( 12,  43),   S( 25,  45),   S( 11,  68),   S(  2,  45),

            /* bishops: bucket 6 */
            S(-23,  42),   S( 12,  25),   S(-36,  42),   S(-30,  32),   S(-48,  43),   S(-61,  40),   S( 14,  40),   S(-36,  -2),
            S( -1,  20),   S(-24,  36),   S(  1,  28),   S(-21,  49),   S( -1,  32),   S(-37,  35),   S(-91,  68),   S( 38,  39),
            S( 60,  -3),   S( 10,  23),   S( 35,  44),   S( 25,  31),   S( 35,  31),   S( 60,  23),   S(-23,  62),   S(-65,  33),
            S( 38,  25),   S(-15,  58),   S( 19,  49),   S( 58,  44),   S( 26,  46),   S( 57,  23),   S(-10,  60),   S(-16,  11),
            S(-31,  52),   S(  7,  46),   S( 65,  28),   S( 44,  37),   S(111,  39),   S( 70,  33),   S( 36,  46),   S(-12,  50),
            S( 28,  14),   S( -1,  37),   S( 35,  34),   S( 39,  47),   S( 30,  47),   S( 32,  54),   S( 25,  24),   S(-26,  56),
            S( -5,  34),   S(-20,  33),   S( 29,  36),   S(  9,  28),   S( 40,  47),   S(  4,  55),   S(  2,  56),   S(-51,  52),
            S(-11,  55),   S( -9,  65),   S( -1,  44),   S(-19,  49),   S(-11,  36),   S( 12,  30),   S(-17,  42),   S( -5,  58),

            /* bishops: bucket 7 */
            S( -6,   4),   S(-29,  15),   S(-35,  -8),   S(-13, -11),   S(-33,   8),   S(-72,  18),   S(-71, -17),   S(-34,  -7),
            S(-47,  -9),   S(-71,  24),   S(-19,   1),   S( 27, -10),   S(-11,  18),   S(-50,  20),   S(-18,  -2),   S(-42, -21),
            S(-32,  14),   S( 41, -18),   S(  5,  31),   S( 38,  17),   S(-29,  32),   S(-11,  19),   S(-10,   7),   S(-11,   9),
            S(-78,  27),   S( 32,  22),   S( 86,   1),   S(101,   8),   S(110,   0),   S( 29,  15),   S( 24,  27),   S(  7,  17),
            S(-36,  -5),   S(-27,  13),   S( 31,   9),   S( 75,   6),   S( 91,  18),   S(110,  14),   S( 15,  45),   S( 38,   6),
            S(-24,  -1),   S( -3,  10),   S( 28,  -5),   S( 35,   7),   S( 28,  22),   S( 77,  35),   S( 84,   9),   S(-13,  38),
            S(-12,  -1),   S( -3,   2),   S( -5,  11),   S(-10,  27),   S( 13, -11),   S( 16,  18),   S( 19,  23),   S( 15,  34),
            S( -6,   0),   S(-26,  23),   S(-26,  -1),   S( -5,   9),   S( 21,   6),   S(  8,   9),   S( 30,   6),   S(  3,  -3),

            /* bishops: bucket 8 */
            S( -8, -67),   S(-12, -62),   S(-32, -28),   S( -5, -20),   S(  0, -19),   S( -9, -41),   S( 12,  16),   S(  2,  -7),
            S( -6, -32),   S(-43, -86),   S(-15, -48),   S(  9,  12),   S(  7,  -8),   S(-14, -59),   S(-17, -43),   S( -1, -45),
            S(  4,  -9),   S(-18, -10),   S( -9, -35),   S( 16, -20),   S( 22,  -1),   S( 11, -37),   S(  9, -26),   S(-29, -65),
            S(  0,  22),   S(  6,  -2),   S(  8,  -6),   S( 19,   0),   S( 29,   9),   S(  0, -30),   S(  0, -23),   S( -2, -32),
            S( 22,  43),   S( 14,  13),   S( 27,  11),   S( 46,  -4),   S( 25, -25),   S( 23,   5),   S(  4, -20),   S(-12, -42),
            S(-19,  -4),   S( -3, -11),   S(  9,  22),   S( 10, -18),   S( 26, -13),   S(  2,  -8),   S(-13, -73),   S(-21, -57),
            S(  4,   9),   S( 15,  -1),   S( 14,  -7),   S( -4, -30),   S( -2, -22),   S(  9, -13),   S( -5, -43),   S(-12, -28),
            S(  0,  -9),   S( 12, -10),   S( -6, -43),   S(-11, -34),   S(-13, -45),   S(  2, -26),   S( -2, -19),   S( -8, -42),

            /* bishops: bucket 9 */
            S(-20, -55),   S(  9, -46),   S(-11, -15),   S(-18, -51),   S(-17, -63),   S( -8, -40),   S(-20, -39),   S(  3, -14),
            S(-11, -31),   S( -7, -46),   S( -3, -24),   S( 12, -28),   S( -2, -13),   S(  5, -21),   S( -4, -41),   S(  1, -18),
            S(  3, -23),   S( 29,   3),   S(  5,  -3),   S( 32, -18),   S( 54, -24),   S(  6, -41),   S( -6, -22),   S(  1, -31),
            S( -3, -11),   S( 23,   9),   S(  6,  -8),   S( 64, -26),   S( 44,  -7),   S( 23, -14),   S(  5, -34),   S(-19, -65),
            S( -1, -44),   S( 25,  -3),   S( 31,   5),   S( 32,   0),   S( 20, -26),   S( 30, -19),   S(  3, -24),   S(-14, -28),
            S(-11, -28),   S( 45,   8),   S( 13,  37),   S( 22,  12),   S( 34, -15),   S( 34, -29),   S(  5, -48),   S(-20, -32),
            S( -4, -22),   S( 36,  22),   S( 11,  -1),   S(  8, -22),   S( 23,  -4),   S(  1, -39),   S( -1, -13),   S( -3, -29),
            S( -2, -10),   S(  7,  -7),   S(  6, -21),   S(-11, -61),   S( -8, -39),   S(  5, -21),   S( -6, -29),   S(-13, -62),

            /* bishops: bucket 10 */
            S(-22, -36),   S(  3, -55),   S(-19, -32),   S(-18, -45),   S( -9, -29),   S(-24, -46),   S( -1, -42),   S(-14, -89),
            S(  2, -37),   S(-10, -47),   S( 12, -37),   S( -1, -27),   S( -5, -40),   S( 10, -36),   S(-20, -65),   S(-10, -22),
            S(-14, -57),   S(  6, -34),   S( -2, -41),   S( 22, -43),   S( 40, -50),   S(  5,  -1),   S(-20, -10),   S( 10,  -8),
            S( -4, -54),   S( 26, -21),   S( 37, -18),   S( 43, -47),   S( 83, -29),   S( 13,  -9),   S(  0,   1),   S(  4,  -9),
            S( -6, -13),   S( 31, -34),   S( 54, -38),   S( 79, -17),   S( 53, -15),   S( 45,  -4),   S( 35,   8),   S( 11, -19),
            S( -5, -63),   S(  8, -54),   S( 15, -42),   S( 19, -34),   S( 36, -23),   S( 35,   0),   S( 21, -10),   S( -9, -41),
            S(-11, -55),   S( -1, -53),   S(  6, -46),   S( 18, -23),   S(  1, -45),   S( 30, -16),   S( 13,  14),   S( 13,  29),
            S(  0, -29),   S( -9, -53),   S(  4,  -4),   S( -2, -28),   S(  1, -13),   S( -4, -44),   S( -5, -29),   S( 10,  29),

            /* bishops: bucket 11 */
            S(-16,  10),   S(-39, -28),   S(-54, -77),   S(-17, -34),   S(-27, -42),   S(-58, -64),   S(-11, -54),   S(-29, -70),
            S(-11, -75),   S(  5, -60),   S(-19, -13),   S(-24, -46),   S(-22, -31),   S(-10, -45),   S(-21, -79),   S(-39, -72),
            S( -3, -58),   S(  7, -75),   S( -2, -20),   S( 36, -35),   S(  8, -28),   S(  4, -32),   S(-18, -26),   S(-15, -31),
            S(-19, -60),   S(  1, -40),   S( 18, -53),   S( 36, -47),   S( 67, -33),   S( 33, -17),   S( 49,  -8),   S(  5,  41),
            S( -1, -53),   S(-23, -60),   S( 34, -34),   S( 74, -28),   S( 38, -12),   S( 67,   4),   S(  5,  19),   S(  5, -38),
            S(-10, -74),   S( 14, -55),   S( 11, -51),   S(  4, -39),   S( 25, -22),   S( 51, -11),   S( 28,  37),   S(-10,   0),
            S( -8, -34),   S(  3, -42),   S( -6, -43),   S(  8, -48),   S( 13, -23),   S( 38, -31),   S( 17, -18),   S(  5,   9),
            S(-19, -78),   S(-11, -28),   S(  6, -35),   S( 17,  -2),   S( 20, -20),   S(-16, -69),   S(  7,  -8),   S( 10,  17),

            /* bishops: bucket 12 */
            S(  2,   0),   S( -5, -30),   S( -7, -41),   S( -9, -36),   S( -5, -22),   S( -5,  -4),   S(  0,   2),   S( -2,   1),
            S( -4, -25),   S(-14, -52),   S( -3, -13),   S( -1, -12),   S(-21, -56),   S(  6,  -1),   S(  1,  -5),   S( -1,  -7),
            S(  1, -18),   S(-16, -11),   S( -4, -12),   S(  1, -28),   S( -1, -14),   S( -2, -45),   S(-16, -60),   S( -3, -17),
            S(-12, -43),   S(  0, -30),   S(  1, -35),   S(  6,  -6),   S(  4, -16),   S(  8,  21),   S( -6, -44),   S( -7, -27),
            S( -1, -12),   S(-13, -28),   S( 15,  -9),   S(  1,  21),   S(  7, -32),   S(  3,  -8),   S(  9,  -3),   S( -4, -12),
            S(-20, -32),   S( 17,  45),   S( -9, -12),   S(-10, -49),   S(-10, -27),   S( -3, -18),   S(  1, -24),   S(  2,   5),
            S( -6, -11),   S( -4,   1),   S(  1, -10),   S( -7, -26),   S(  3,   7),   S(  9,   6),   S( -8, -34),   S( -6, -24),
            S( -1, -17),   S( -3,  -8),   S( -3, -35),   S(  5,   1),   S(  4,  -7),   S( -5, -27),   S(-13, -38),   S(  0,  10),

            /* bishops: bucket 13 */
            S(-13, -73),   S( -8, -58),   S(-14, -68),   S( -6, -64),   S( -7, -41),   S(-10, -17),   S( -7, -36),   S( -9, -51),
            S( -2, -22),   S( -3, -56),   S(-15, -61),   S(-13, -47),   S( -4, -38),   S(  1, -13),   S(  2, -11),   S(  3, -21),
            S( -8, -42),   S( -7, -13),   S( 14, -39),   S(  9, -48),   S(  5, -56),   S( 21, -24),   S( -3, -37),   S( 10,  44),
            S( -2,  -4),   S( -1, -32),   S(  2, -25),   S( -7, -55),   S( 21, -28),   S(  3,  -9),   S( -2,  -7),   S(-11, -40),
            S(  4,  24),   S(-12,  -5),   S( -1, -67),   S( 12, -28),   S( 10, -18),   S(  5, -37),   S(  2, -50),   S( -2, -17),
            S(  1,  -8),   S( -4, -24),   S(-14, -24),   S( 17, -10),   S( -1, -23),   S(  6, -31),   S( 10, -37),   S( -7, -45),
            S( -7, -37),   S( -4, -23),   S(  3,   7),   S( -9, -14),   S(-15, -43),   S(  3,   8),   S(-10, -64),   S( -1, -28),
            S(-11, -46),   S( -3, -16),   S( -7, -30),   S(  8,  -5),   S(  3,  -5),   S( -8, -44),   S( -1,  -4),   S( -5, -48),

            /* bishops: bucket 14 */
            S( -6, -65),   S( -8, -44),   S(-24, -89),   S(-12, -58),   S(-13, -57),   S( -6, -33),   S(-12, -82),   S( -6, -17),
            S( -6, -23),   S(  0, -41),   S(  1, -15),   S(-16, -64),   S( -1, -40),   S( -4, -56),   S(-14, -67),   S(  3, -11),
            S( -5, -11),   S(-11, -35),   S(-11, -62),   S(  2, -41),   S(  1, -53),   S(-17, -86),   S( -5, -42),   S( -6, -40),
            S( -5, -43),   S( -8, -42),   S(  5, -26),   S(  0, -35),   S( 11, -73),   S(  9, -33),   S(-10, -53),   S( -6, -17),
            S(-10, -57),   S(  4, -22),   S(  3, -43),   S(  7, -21),   S(  4, -51),   S(-10, -50),   S(  1, -28),   S( -5, -26),
            S( -1, -32),   S( -1, -29),   S( -2, -45),   S(-10, -44),   S( -2, -16),   S( -5,   1),   S(  7, -48),   S( -7, -35),
            S( -8, -55),   S(  9, -29),   S(-10, -40),   S(  6, -13),   S( -9, -47),   S( -5, -10),   S( -7, -15),   S( -2, -10),
            S( -5, -28),   S( -2, -31),   S( -2, -14),   S(-13, -45),   S( -7, -35),   S(  1,   0),   S(  5,  27),   S( -1, -16),

            /* bishops: bucket 15 */
            S(  9,  42),   S(  7,  17),   S(-12, -36),   S(  0, -11),   S( -1, -27),   S( -7, -21),   S( -8, -38),   S( -4, -17),
            S(  2,   1),   S( -4, -10),   S(  4, -17),   S( -2, -18),   S(-10, -44),   S( -2, -14),   S( -4, -16),   S( -1,   2),
            S( -9, -45),   S( -6, -33),   S( -7, -45),   S( -5, -14),   S( -6, -58),   S(-12, -15),   S(  0, -24),   S(  7,  20),
            S( -6, -30),   S(-21, -56),   S(  4,  -5),   S(-11, -58),   S(-12, -28),   S(-10, -46),   S(  6,   5),   S(  1,  -2),
            S( -6, -40),   S(-10, -29),   S( -5, -39),   S(-16, -35),   S( 13,  -2),   S( -3,  -5),   S(  2, -16),   S( -8, -33),
            S( -7, -30),   S(  1, -38),   S( -8, -31),   S( -7, -42),   S(  5, -22),   S(  0,  15),   S( 22,  10),   S(  2,   5),
            S( -2, -23),   S(  1, -16),   S(  1,  -9),   S( -5, -35),   S( -5, -27),   S( -2, -11),   S(-12, -26),   S(  3,   7),
            S( -1,  -9),   S( -5, -16),   S( -2, -21),   S( -4, -45),   S( -7, -53),   S(-15, -54),   S( -6, -22),   S(  0,  -2),

            /* rooks: bucket 0 */
            S(-26,   1),   S(-14,  10),   S(  2, -29),   S( 15, -18),   S( 14, -11),   S( 19, -33),   S(  6,  11),   S( 23,  -9),
            S( 34, -93),   S( 38, -16),   S( 20, -24),   S( 22, -22),   S( 35,  -7),   S(  5,  -3),   S(-20,   3),   S(-37,  21),
            S( 11, -26),   S( 19,   4),   S( 39,  -9),   S( 11,   3),   S( -1,   8),   S( 21, -15),   S(-13,  -1),   S(-37, -12),
            S( 47, -44),   S( 79, -13),   S( 72, -12),   S( 50,  -4),   S( 20,   8),   S(-15,  21),   S(-23,  16),   S(-18,   7),
            S( 60, -40),   S( 78,   1),   S( 50,  13),   S( 28,   0),   S( 41,   4),   S( 35,  -6),   S( -5,   6),   S(-32,  15),
            S( 67, -42),   S( 96, -41),   S( 77,  -4),   S( 33,  17),   S( 43,  23),   S(-33,  23),   S( 25,  19),   S(-32,  31),
            S( 39,  -9),   S( 74,  -3),   S( 17,  19),   S( 33,  13),   S(  6,  16),   S( 19,  15),   S(  5,  11),   S(-10,  32),
            S( 65,   0),   S(-29,  64),   S( 39,  30),   S( 41,  14),   S( 21,  15),   S(  5, -11),   S( 22,  14),   S( 45,   5),

            /* rooks: bucket 1 */
            S(-82,  47),   S(-30,  -5),   S(-30, -10),   S(-31,  -8),   S(-10, -21),   S(-21,  -6),   S(-17, -10),   S(-29,  18),
            S(-46,  15),   S(-56,  18),   S( -2,  -6),   S(-12, -37),   S(-32,   5),   S(-22, -20),   S(-36, -24),   S(-52,  10),
            S(  9,  13),   S(-44,  43),   S(-16,  19),   S(-28,  20),   S(-53,  30),   S(-21,  17),   S(-10, -10),   S(-37,  21),
            S(-54,  52),   S(-47,  46),   S(  3,  29),   S(-29,  43),   S(-25,  32),   S(-53,  45),   S(-29,  35),   S(-42,  19),
            S( 74,   9),   S( 29,  29),   S(  0,  33),   S(-18,  40),   S(-20,  48),   S( 29,  13),   S(  7,  19),   S(-26,  15),
            S( 59,  15),   S( 31,  44),   S( 44,  34),   S( -8,  32),   S(  3,  44),   S(  6,  32),   S(  7,  24),   S(-33,  37),
            S( -9,  46),   S( 21,  43),   S( 45,  19),   S(-52,  67),   S(-20,  37),   S( 26,  31),   S(-54,  38),   S(-57,  54),
            S( 29,  38),   S( 19,  37),   S( 53,  23),   S(-32,  67),   S(  0,  32),   S( 29,  21),   S(-13,  40),   S( 33,  19),

            /* rooks: bucket 2 */
            S(-65,  45),   S(-43,  24),   S(-46,  32),   S(-57,  31),   S(-49,  14),   S(-48,  10),   S(-40,  -7),   S(-49,  30),
            S(-71,  40),   S(-72,  54),   S(-49,  31),   S(-55,  24),   S(-48,  19),   S(-74,  22),   S(-65,  18),   S(-61,  25),
            S(-69,  61),   S(-41,  50),   S(-48,  64),   S(-40,  31),   S(-55,  44),   S(-43,  34),   S(-13,  16),   S(-32,  13),
            S(-95,  81),   S(-66,  71),   S(-43,  72),   S(-32,  51),   S(-46,  62),   S(  8,  30),   S(-12,  45),   S(-22,  28),
            S(-29,  61),   S(-45,  74),   S(-56,  72),   S(-22,  52),   S( 24,  35),   S(-16,  66),   S(-36,  62),   S(-35,  45),
            S(-13,  47),   S(-25,  58),   S(-16,  52),   S(  1,  49),   S( 32,  38),   S( 46,  20),   S( 37,  24),   S(-17,  36),
            S(-55,  63),   S(-69,  83),   S(-23,  65),   S(  0,  63),   S( 11,  52),   S( 29,  33),   S(-41,  81),   S(  4,  49),
            S(-25,  71),   S( -4,  58),   S(-68,  72),   S(  6,  49),   S(-38,  59),   S(-11,  55),   S(-15,  64),   S( -4,  48),

            /* rooks: bucket 3 */
            S(  6,  72),   S(  5,  64),   S(  9,  66),   S( 20,  59),   S( 17,  52),   S( -1,  64),   S(  2,  76),   S( -3,  47),
            S(-26,  85),   S( -7,  66),   S(  5,  65),   S( 17,  57),   S( 20,  56),   S( 13,  57),   S( 40,   9),   S( 33, -33),
            S(-34,  85),   S( -3,  79),   S(  2,  86),   S( 15,  63),   S( 14,  79),   S( 17,  73),   S( 28,  63),   S( 10,  58),
            S(-32,  91),   S(-26, 101),   S( 15,  86),   S( 26,  76),   S( 28,  79),   S( 12, 100),   S( 57,  62),   S( 10,  73),
            S( -5, 100),   S( 31,  83),   S( 17,  78),   S( 39,  69),   S( 49,  70),   S( 44,  72),   S( 99,  51),   S( 56,  49),
            S( -6, 104),   S( 23,  85),   S( 18,  87),   S( 28,  79),   S( 42,  65),   S( 47,  59),   S(106,  28),   S( 74,  49),
            S(-20, 103),   S( -4, 106),   S( 11, 100),   S( 37,  89),   S( 34,  79),   S( 73,  56),   S( 72,  73),   S(149,  22),
            S(-50, 144),   S( 14, 108),   S( 11,  95),   S( 65,  67),   S( 56,  62),   S( 84,  55),   S(148,  29),   S(120,  37),

            /* rooks: bucket 4 */
            S(-92,  34),   S(-24,  -6),   S(-24,  -4),   S( 16,  -6),   S( -9, -25),   S(  5, -47),   S( -4, -25),   S( 20, -58),
            S(-10, -32),   S(-29, -14),   S(-35,  13),   S(-18,   5),   S(-29, -12),   S( -3, -29),   S(  8, -22),   S(-79,  18),
            S( -9,   0),   S( -9, -12),   S(  3, -11),   S( -8, -19),   S(-15, -10),   S( -3, -17),   S( 26, -24),   S(-59,  -6),
            S(-33, -24),   S(  7,  22),   S(-55,  40),   S( 40,  -8),   S( 21, -22),   S(-23,  -8),   S(-24,   3),   S(  5,  -5),
            S(-24,  20),   S(-20,  30),   S( -6,  16),   S( 18,  33),   S(  9,  15),   S(-15,  -2),   S( 31,  14),   S( 30, -16),
            S(  9,   1),   S( 20,   2),   S( 49,  18),   S( 52,  27),   S( 34,  -8),   S( 22,  31),   S( 20,  13),   S(  4,  27),
            S(-21,   7),   S( 32,  39),   S( 36,   6),   S( 43,  27),   S( 50,  16),   S( 25, -19),   S( 34,   5),   S( 39,  19),
            S(  8, -54),   S( 44,  47),   S( 39,  20),   S(  9,  15),   S( 16, -16),   S( 19,   5),   S( 11,  10),   S(  7,  24),

            /* rooks: bucket 5 */
            S(-53,  42),   S(-26,  25),   S(-26,  26),   S(-11,  11),   S(  8,  -7),   S(-11,  27),   S( 36,  11),   S(-14,  33),
            S(-39,  27),   S(-50,  40),   S(-92,  58),   S(-60,  42),   S(-26,  28),   S(-18,   5),   S( 25,  -2),   S( -4,  10),
            S(-21,  45),   S(-52,  52),   S(-75,  81),   S(-88,  65),   S(-43,  26),   S(-15,  39),   S(-21,  24),   S(-17,  14),
            S(-30,  65),   S( -7,  46),   S(-24,  57),   S(-58,  60),   S(-66,  75),   S(-19,  63),   S(-33,  57),   S(-12,  27),
            S( 25,  47),   S( -9,  66),   S( 13,  62),   S(-19,  76),   S( 33,  48),   S( 15,  58),   S( 63,  45),   S( 28,  38),
            S( 74,  56),   S( 30,  72),   S( 41,  62),   S( 27,  65),   S( 61,  51),   S( 74,  48),   S( 31,  49),   S( 53,  44),
            S( 18,  53),   S( 38,  58),   S( 70,  41),   S( 13,  70),   S( 62,  41),   S( 69,  48),   S( 84,  47),   S( 78,  48),
            S( 48,  40),   S( 51,  29),   S( 19,  47),   S( 14,  40),   S( 46,  32),   S( 88,  26),   S( 65,  36),   S( 27,  41),

            /* rooks: bucket 6 */
            S(-35,  22),   S(-15,  22),   S( 14,  13),   S( -8,  19),   S(-45,  27),   S(-56,  45),   S(-53,  68),   S(-20,  49),
            S(-21,  25),   S( 20,  11),   S( -5,  28),   S(-56,  43),   S(-24,  32),   S(-50,  50),   S(-28,  31),   S( 44,  22),
            S(-58,  56),   S(-39,  42),   S(-20,  46),   S(-77,  63),   S(-13,  33),   S(-53,  69),   S(-47,  76),   S( 22,   2),
            S(-44,  62),   S( 19,  55),   S(-29,  74),   S(  2,  41),   S( 13,  40),   S(  4,  59),   S(-80,  83),   S(-35,  55),
            S( -8,  71),   S( -4,  80),   S( 82,  28),   S( 48,  33),   S( 29,  61),   S(  6,  70),   S( 11,  62),   S( 15,  46),
            S( -2,  69),   S( 57,  56),   S( 74,  45),   S( 43,  37),   S( 60,  45),   S( 48,  59),   S( 44,  57),   S( 64,  48),
            S( 49,  56),   S( 92,  38),   S( 95,  41),   S( 91,  20),   S(102,  35),   S( 62,  50),   S( 86,  47),   S( 40,  53),
            S( 58,  59),   S( 78,  42),   S( 28,  52),   S( 25,  50),   S( 82,  44),   S( 84,  34),   S( 69,  49),   S( 55,  34),

            /* rooks: bucket 7 */
            S(-49, -11),   S(-21, -13),   S(-31,  -4),   S(-16,  -1),   S( 11, -14),   S(-34,  20),   S(-51,  39),   S( 16, -21),
            S(-69,  26),   S(-42,  14),   S(-32,  -1),   S( -2,  -6),   S(  6,   6),   S( 20,   0),   S(  0, -16),   S(-34,   0),
            S(-70,  50),   S(-57,  27),   S(  4,  19),   S(-15,   1),   S( 14,   3),   S(-39,   7),   S(-21,  -1),   S( 10,   8),
            S(-47,  37),   S(-10,  33),   S(-15,  33),   S( 51,   2),   S( 23,  12),   S( 26,  13),   S( 27,  -4),   S(  0,   1),
            S( -9,  33),   S(  2,  32),   S( 47,  -6),   S( 56, -10),   S( 56,  13),   S(128, -12),   S( 39,  25),   S( 56, -21),
            S(-15,  36),   S( 32,  17),   S( 85, -14),   S( 95, -16),   S( 62,   4),   S( 95,  15),   S( 72,  27),   S( 45,   2),
            S( 16,  29),   S( 52,  13),   S( 52,  11),   S(116,  -6),   S( 99,  -8),   S( 78,  25),   S( 59,  19),   S( 49,  -1),
            S( -2,  60),   S( 14,  34),   S( 58,  10),   S(108, -19),   S( 56,   7),   S( -3,  14),   S( 44,   1),   S( 32,  -2),

            /* rooks: bucket 8 */
            S(-31, -14),   S(-16,  -1),   S( -3,   0),   S(-20,  -1),   S(-28, -56),   S(-18, -37),   S(-26, -35),   S(-21, -24),
            S(-15,  -5),   S(-22, -48),   S(-17,  -2),   S( -5,   6),   S(-32, -28),   S(-16, -55),   S( -1, -18),   S(-21, -71),
            S( 11,   5),   S( -5,   6),   S( -8,  -6),   S(-11,   7),   S(-29, -66),   S(-19, -18),   S(  5,   9),   S( -6,  -9),
            S(-15, -11),   S( -4,  17),   S(-28, -10),   S(  4,  25),   S(  1,   7),   S(-13, -33),   S( -4,  -8),   S(-14,   3),
            S(-16, -27),   S( -4,  12),   S(-19,  29),   S( -5,  12),   S(-10, -16),   S(  7,   4),   S( -6,  -1),   S( -4, -40),
            S(  5,  29),   S(-19,  -7),   S( 13,  32),   S( 11,   1),   S( -4, -13),   S( -1,  -7),   S( -5,   2),   S(  6,   4),
            S(  0,   7),   S(-16,  -7),   S( 16,  13),   S(  1,   4),   S( 31,  18),   S( -4, -28),   S( 12,   6),   S( -4, -15),
            S( -7, -127),  S( -5, -13),   S( 13,  13),   S( 14,   8),   S(  1, -12),   S(  6,  -3),   S(  3, -13),   S(  4,  11),

            /* rooks: bucket 9 */
            S(-71, -29),   S( -6, -46),   S(-33, -36),   S(-58, -26),   S(-45,  -7),   S(-40, -11),   S(  4, -62),   S(-59, -39),
            S( 19, -15),   S(-20, -55),   S(-32, -35),   S(-19, -31),   S(-30, -39),   S( 11, -26),   S(-16, -45),   S(-16, -33),
            S(  0, -11),   S( 24, -46),   S(-22,   9),   S(-26, -14),   S(-50, -25),   S( 25, -21),   S(  4,  -8),   S(-17, -42),
            S(-11, -16),   S( -9,  21),   S( -4,  -7),   S(-38,  -4),   S(  0,   9),   S(  7, -14),   S(  9,   0),   S(-10,   0),
            S( -7,  -7),   S(-15,   6),   S(  0, -14),   S( 24,  44),   S( 15,  28),   S(  6, -24),   S(  7, -17),   S(  1, -28),
            S( -1,   4),   S(-14, -19),   S( -8,  -7),   S(-33,   7),   S( 20,  -1),   S( 23,   4),   S( 14,   5),   S( -1, -33),
            S( 43,   8),   S( 55, -19),   S( 38,  25),   S( 38,   3),   S(  2, -36),   S( 42, -22),   S( 24, -27),   S( 39,   2),
            S( 46, -68),   S( 18, -56),   S( 27,  -7),   S(  5,  18),   S( 12,  17),   S( 19,   2),   S( 16, -19),   S( 23, -15),

            /* rooks: bucket 10 */
            S(-62, -108),  S(-51, -53),   S(-35, -55),   S(-33, -34),   S(-63, -46),   S(-43, -69),   S( 17, -48),   S(-35, -41),
            S(-11, -35),   S(-11, -43),   S( -6, -56),   S(-46, -18),   S( -4, -27),   S(-12,  -1),   S( -5,  -8),   S(  3, -41),
            S(-35, -30),   S(-23, -41),   S(-11, -45),   S( -8, -15),   S(-42,  -3),   S(-17, -20),   S( 15, -24),   S( -5, -21),
            S(-15, -24),   S(  2,  -7),   S(-16,  -9),   S(-14, -19),   S(-12, -17),   S(-13, -47),   S(  8,  -2),   S( -8, -19),
            S(  0, -12),   S( 16, -24),   S( -6, -12),   S(  7, -35),   S(-16, -11),   S( -8,  -8),   S( 25,  -8),   S(  2, -11),
            S( 22,  -3),   S( 16,   6),   S( -2, -12),   S(-20, -27),   S( -5, -23),   S(  3,   0),   S( 36,   7),   S(-10, -14),
            S( 79, -23),   S( 72, -28),   S( 66,  -9),   S( 43, -26),   S( 37, -26),   S( 18,  -8),   S( 35, -18),   S( 47, -37),
            S( 14,  -3),   S( -1, -26),   S( 28, -17),   S(  8, -20),   S( 22, -22),   S( 23,   3),   S( 22, -16),   S(  3, -26),

            /* rooks: bucket 11 */
            S(-62, -48),   S(-41, -46),   S(-32, -11),   S(-24, -76),   S(-19,  -8),   S(-25,  15),   S(-43, -66),   S(-49, -18),
            S(-44, -36),   S(-18, -53),   S(-34, -17),   S(-44, -21),   S(-31, -32),   S(-26, -10),   S(-27, -28),   S(-24,  -6),
            S(-14, -29),   S(-10, -36),   S(  0, -17),   S(-16, -25),   S(-10,   2),   S(-24, -10),   S(-38, -50),   S(-42, -66),
            S(-25,  28),   S( -3, -26),   S(-17,   2),   S(  0,  16),   S(-12,   5),   S(  3,  35),   S( 17,   8),   S( -9, -34),
            S(-18,   2),   S(  3, -28),   S(  2,  12),   S( 31, -12),   S( 25,  -1),   S( 19,  -3),   S( -7, -26),   S(-10, -17),
            S(  5,  24),   S( 32,  17),   S( 15,  -4),   S( 36, -11),   S( 33,  19),   S( 61, -16),   S(-33,  -9),   S( 16,   2),
            S( 61,  35),   S( 36,   7),   S( 45, -26),   S( 53,  -6),   S( 20,  12),   S( 20,  20),   S( 17,  26),   S( 40,  -1),
            S( 31,  28),   S( 21,  12),   S( 15,  -6),   S( 16, -22),   S( -3, -11),   S( 17,  -8),   S( 16,  18),   S(  9, -20),

            /* rooks: bucket 12 */
            S(  1, -38),   S(-26, -56),   S(-21, -43),   S(-18, -19),   S(  3,  -6),   S(-11, -61),   S(-32, -84),   S(-24, -24),
            S(  6,   5),   S(  7,   9),   S( -4,  30),   S( -8,   1),   S(  4,  11),   S( -5, -15),   S(  9,  -1),   S( -5, -35),
            S( 12,   6),   S(-15, -24),   S(-13, -22),   S(-19, -49),   S(-15, -36),   S(  9,  -4),   S(  0,  -7),   S(  5,   8),
            S( -6, -20),   S(-10, -45),   S(  9,  15),   S(  8,   6),   S(  1,  -1),   S( -6, -24),   S( -7, -29),   S( -1, -16),
            S(-19, -31),   S(-16, -26),   S(  8,  -2),   S(  2,  -6),   S(-18, -52),   S( 11,   3),   S( -1, -21),   S(  6,   2),
            S(-14, -19),   S( -8, -40),   S(  6,  21),   S(  8, -14),   S( -2, -23),   S( -4, -28),   S(  5,   0),   S(  5,   3),
            S( -5,   2),   S( -5, -12),   S( -8, -49),   S(  2, -40),   S(  8, -33),   S( -2, -42),   S( -7, -28),   S(  0,   1),
            S(-11, -31),   S(  7,   1),   S(  1, -14),   S(  4,  -6),   S(  6,   5),   S(-13, -32),   S(-11, -60),   S(  7,   5),

            /* rooks: bucket 13 */
            S( -8, -49),   S( -8, -28),   S(-26, -30),   S(-12,   3),   S( -2,   2),   S(-11, -44),   S( -1, -48),   S(-20, -33),
            S(  2, -14),   S(  9,  18),   S(-12,   1),   S(-12,   1),   S(-15, -18),   S( -5,  -2),   S(  2,   2),   S( -2, -18),
            S( -9, -48),   S(-10, -36),   S(-19, -52),   S(-10, -39),   S(  4,  31),   S(  7,  -6),   S(  1,  11),   S(  1,   2),
            S(-10, -44),   S( -4,   3),   S(-16, -23),   S(-13, -20),   S(  8,  11),   S(-17, -42),   S(  8,   3),   S( -4, -12),
            S( 12,   6),   S( -3, -24),   S( -1,  -8),   S( -9, -30),   S(-13, -18),   S( -7, -26),   S(  1, -24),   S(  1,   8),
            S(-15, -33),   S( -4, -16),   S(-19, -46),   S( 17,   6),   S(  1, -33),   S(  7,   5),   S( 11,  -1),   S(  4, -15),
            S( -2,  13),   S(  7,  -2),   S( 14,  24),   S( -7, -13),   S( 13,  28),   S( 17,  30),   S(  9,  -7),   S(  0,  -1),
            S(-21, -105),  S(-25, -93),   S(  0,   5),   S( -9, -22),   S(  9,  14),   S( -5, -22),   S( -5, -44),   S( 10,  12),

            /* rooks: bucket 14 */
            S(-20, -49),   S(-28, -51),   S( -1, -34),   S(-14, -41),   S( -2,  39),   S(  1,  16),   S(  8, -19),   S( -7, -35),
            S(-23, -43),   S(-27, -63),   S( -6,  -1),   S(-23, -50),   S(-10, -14),   S(-14, -36),   S( -1,  21),   S( -6, -29),
            S( -3, -16),   S(  0, -11),   S(  1,  -9),   S( -8, -15),   S( -6, -19),   S(-15, -21),   S( 12,  50),   S(  3, -12),
            S(  0,  15),   S(-10, -44),   S(  1, -17),   S( -5,  -7),   S( -6, -15),   S( -1, -18),   S(-13, -50),   S( -7,   9),
            S(  2, -37),   S(  1, -38),   S( -7, -48),   S( -6, -53),   S( -7, -57),   S(  0, -43),   S(  1, -49),   S(  3,  -5),
            S(  1, -26),   S( -5, -24),   S(  1, -24),   S(  2, -67),   S(-11, -80),   S( 10, -44),   S(  6, -11),   S( -3,  -6),
            S(  2,  12),   S(  5, -20),   S(  6, -36),   S(  0, -55),   S(  5, -71),   S( -3, -31),   S( 18,  13),   S( -2, -18),
            S( -6, -33),   S( -3, -21),   S(-12, -80),   S( -3, -37),   S( -5, -34),   S( -5,  20),   S( -7,  -8),   S(-11, -17),

            /* rooks: bucket 15 */
            S(-10, -30),   S(-14, -51),   S(-11, -51),   S(-14, -53),   S(-13, -31),   S(-14,  -6),   S(-13, -40),   S(-18, -47),
            S(-14,  -9),   S(-11, -44),   S( -6,  -9),   S(-13,  -7),   S(-24, -52),   S( -6, -20),   S(-17, -54),   S(  2,   4),
            S( -8, -34),   S( -7, -17),   S(-16, -42),   S( -1,  -3),   S( -2, -14),   S( -8, -19),   S( -7,   1),   S( -6, -32),
            S(  0, -26),   S( -7, -42),   S(-10, -21),   S( -9, -27),   S( -8, -35),   S( -8, -39),   S( -2, -26),   S( -8,   9),
            S(  0, -21),   S( -4, -29),   S( 13,  -6),   S(-10, -24),   S(  0, -16),   S( 12,  -9),   S(  3, -23),   S(  1,  36),
            S(  2,   2),   S( -6, -28),   S( -3, -47),   S( -2, -53),   S( -6, -46),   S( -3, -64),   S( 11, -26),   S( -6,   0),
            S(  4,   1),   S( 14, -14),   S(  5, -33),   S( -2, -63),   S(  2, -44),   S(  6,   6),   S(  0, -26),   S( -6, -33),
            S(  2, -28),   S( -7, -26),   S(  0, -14),   S(  2, -16),   S(  6, -23),   S(  4, -10),   S(  9,   4),   S(  4,  -5),

            /* queens: bucket 0 */
            S(-30, -14),   S(-29, -60),   S( 28, -85),   S( 63, -67),   S( 33, -26),   S( 22,  -4),   S( 49,  36),   S(  7, -11),
            S(-17, -23),   S( 38, -51),   S( 44, -29),   S( 28,   4),   S( 29,  36),   S( 31,   1),   S( 23,  51),   S( 43,  15),
            S( 14,   0),   S( 45,  -3),   S( 28,  24),   S( 25,  16),   S( 32,  -9),   S( 18,  25),   S(  5,  16),   S( 27,  53),
            S( 36, -17),   S( 21,  60),   S( 15,  38),   S( 13,  54),   S( 40,   7),   S(  6,  55),   S( 24,   4),   S( 31,  13),
            S( 35,  26),   S( 23,  41),   S( 22,  33),   S( 12,  63),   S( -2,  23),   S( -2,   1),   S( 10,  15),   S( 22,   1),
            S( 29,  42),   S( 20,  59),   S( 51,  10),   S( 22,  23),   S( 68,   9),   S( 44,  32),   S( 16,  17),   S( 28, -41),
            S( 61,  37),   S( 66,  15),   S(  8,  46),   S( 62,  39),   S(  8,  15),   S( -6,   4),   S( 39,   8),   S( 33,  13),
            S( 36,  20),   S( 25,  44),   S( 57,  51),   S( 32,  36),   S( 34,  33),   S(-38, -11),   S( 45,  23),   S( 37,  24),

            /* queens: bucket 1 */
            S(  0, -52),   S(-82,  -5),   S(-60,  -3),   S(-13, -94),   S(  9, -33),   S(-16, -56),   S( 39, -70),   S( -6,  24),
            S(-10, -71),   S(-17, -37),   S( 15, -33),   S(  8,   6),   S(  8,  15),   S( 14,   3),   S( 31, -48),   S( -6,  28),
            S(-28,  29),   S( 10, -37),   S( 13,  20),   S(  3,  17),   S(  4,  30),   S( -3,  26),   S( 21, -14),   S( 30,  29),
            S(  9,   9),   S(-13,  15),   S( -8,  45),   S( 28,  37),   S(  2,  24),   S( 23,  -8),   S( -9,   4),   S( 17,  27),
            S( 35, -19),   S( 14,  44),   S(  1,  67),   S(-18,  81),   S( -5,  62),   S(  1,  16),   S( -4,   2),   S(  3,  43),
            S( 19,  42),   S( 25,  45),   S( 44,  39),   S(-37,  67),   S( -5,  55),   S(-47,  54),   S( 42,  20),   S( 37,  18),
            S( -5,  44),   S(-37,  86),   S(  8,  40),   S( -9,  99),   S( -8,  64),   S(-15,  45),   S( 11,  50),   S(  4,  32),
            S(-30,   0),   S( 15,   4),   S( 29,  52),   S(-19,  38),   S(  7,  27),   S(-26,  29),   S( 30,  33),   S( 21,  43),

            /* queens: bucket 2 */
            S( 15, -10),   S( 25, -47),   S(  5, -20),   S(  4, -23),   S(-22,   6),   S(-36, -33),   S(-33, -31),   S(  1,   8),
            S( 21, -12),   S( 21,   7),   S( 24, -26),   S( 28, -27),   S( 24, -29),   S( 14, -51),   S( 32, -23),   S( 36, -34),
            S(  8,  18),   S( 20, -10),   S( 16,  24),   S( 18,  17),   S(  7,  56),   S( 15,  38),   S( 10,  -3),   S( 40,  12),
            S( 10,  28),   S(  0,  35),   S(  1,  46),   S(  9,  56),   S(-14,  91),   S( -2,  87),   S( 20,  17),   S( 18,  52),
            S(  8,  18),   S(-10,  53),   S(-27,  85),   S(-32, 108),   S(-20, 102),   S(-26, 130),   S(-42, 156),   S( -9, 102),
            S( 12,  34),   S(  7,  57),   S(-16,  89),   S(-11,  62),   S(-15,  98),   S(-18,  98),   S( -7,  96),   S( 15,  56),
            S(-37,  78),   S(-39,  95),   S(-20,  90),   S(  8,  75),   S(-16, 124),   S( 42,  33),   S(-42,  68),   S(-33,  86),
            S(-37,  95),   S(  1,  55),   S( 90,  39),   S( 47,  47),   S( 13,  69),   S( 31,  33),   S( 12,  52),   S(-13,  25),

            /* queens: bucket 3 */
            S( 80,  83),   S( 62,  73),   S( 48,  90),   S( 48,  86),   S( 73,  38),   S( 51,  10),   S(  3,  21),   S( 38,  42),
            S( 61, 109),   S( 67,  88),   S( 52,  99),   S( 54,  77),   S( 54,  79),   S( 63,  56),   S( 77,  -1),   S( 41,  21),
            S( 58,  86),   S( 54,  95),   S( 59,  75),   S( 59,  65),   S( 55,  91),   S( 53,  97),   S( 47, 104),   S( 69,  51),
            S( 46, 124),   S( 48,  95),   S( 49,  91),   S( 42, 105),   S( 42,  94),   S( 40, 126),   S( 50, 102),   S( 44, 138),
            S( 50,  97),   S( 49, 115),   S( 33, 122),   S( 36,  96),   S( 29, 136),   S( 22, 130),   S( 21, 164),   S( 45, 144),
            S( 48, 124),   S( 61, 110),   S( 31, 133),   S( -2, 142),   S( 43, 154),   S( 78, 106),   S( 58, 161),   S( 20, 210),
            S( 36, 147),   S( 42, 130),   S( 86,  90),   S( 64, 106),   S( 48, 129),   S( 34, 148),   S( 76, 137),   S(155,  67),
            S( 68, 108),   S( 93,  94),   S( 79, 110),   S( 96,  95),   S( 47,  91),   S( 99,  67),   S(116,  60),   S(101,  90),

            /* queens: bucket 4 */
            S( -4, -22),   S(-20, -21),   S(-56, -28),   S( -9, -33),   S(  0, -34),   S( 53,   7),   S(-45, -41),   S(-10,   7),
            S(-14, -28),   S(-45, -35),   S( 34, -13),   S(-30,  10),   S(-13,   1),   S( 32,  17),   S(-13,  -2),   S(-42, -27),
            S(  2,  -7),   S( 26,  26),   S(  4,  44),   S(-12,  26),   S( 25,  12),   S( 36,  -5),   S( -3, -23),   S(-37, -27),
            S(-22,  -9),   S( 20,   1),   S(  0,  22),   S( -4,  31),   S( 34,  44),   S( 24,  13),   S( 35, -10),   S(-24, -14),
            S(  9,  15),   S( 40,  22),   S( 28,  44),   S( 26,  37),   S(  3,  35),   S( 31,  -7),   S(-44, -48),   S(-21, -39),
            S( 14,  17),   S( 32,   2),   S( 41,  53),   S( 57,  61),   S( 21,   6),   S( -6,  -4),   S(-15, -12),   S(-20, -16),
            S(-48, -27),   S(-15,  17),   S( 18,  29),   S( 58,  62),   S( -6, -10),   S( -5,   1),   S( -8, -21),   S(-30, -34),
            S(-28, -26),   S( -8,  -5),   S( 23,  47),   S(  9,  24),   S(-23, -19),   S( -6, -15),   S(-32, -56),   S( 12,   1),

            /* queens: bucket 5 */
            S(-33,  -5),   S(-42, -52),   S(-19, -10),   S(-27, -27),   S(-62, -32),   S( 42,   8),   S(  2, -12),   S( 12,  -1),
            S(-44, -23),   S(-40, -16),   S(-69, -40),   S(-56,  12),   S(-33, -53),   S(-27, -13),   S(-75, -28),   S(-37, -23),
            S(-26,   6),   S(-94, -16),   S(-79, -19),   S( -4,  42),   S( 50,  75),   S(  3,   8),   S( 30,  18),   S( -9,  15),
            S(-72, -28),   S(-65, -19),   S(  7,  31),   S( -1,  79),   S(  5,  16),   S( 12,  24),   S( -7, -33),   S(-54,   1),
            S(-59, -35),   S( -4,  18),   S(  0,  63),   S(  8,  66),   S( 36,  53),   S(-15,   3),   S( -8,  13),   S(-23,   3),
            S(  2,  15),   S(  0,  36),   S(-30,   8),   S( 34,  44),   S( 34,  55),   S(  8,  34),   S(  9,  11),   S( -3,  -1),
            S(-24,  -3),   S( 19,  51),   S( 22,  85),   S( -4,  32),   S( 19,  54),   S( 38,  79),   S(  7,   6),   S(-30, -40),
            S(-12,   8),   S( 25,  35),   S(  7,  18),   S(  6,  51),   S(  3,  19),   S( 13,  27),   S( 15,  10),   S(-38, -48),

            /* queens: bucket 6 */
            S(-21,  -9),   S(-48, -32),   S(-47, -20),   S(-90, -73),   S(-84, -53),   S(-55, -39),   S(-22, -44),   S( 22,  18),
            S(-41,  -5),   S(-27, -11),   S(-24,  -9),   S(-47,   7),   S(-62,  31),   S(-107, -16),  S(-87, -23),   S(-35,   2),
            S(-49,  25),   S( -4,  17),   S(-42,  35),   S(-74,  82),   S(-39,  64),   S(-22, -18),   S(-54, -28),   S(-15,  -5),
            S(-16,  37),   S(  7,  25),   S(-24,  53),   S(-33,  73),   S( 18,  83),   S( 16,  55),   S( -9,  26),   S( 25,  51),
            S(-49,  38),   S(-13,  28),   S(-43,  70),   S( 33,  38),   S(  3,  88),   S( 63,  56),   S( 25,  40),   S(  5,   5),
            S(-34,  43),   S(-11,  25),   S( 56,  40),   S( 38,  37),   S(  8,  53),   S( 57,  89),   S(-24,  29),   S(-32, -11),
            S(-12,  12),   S( -4,  26),   S(  5,  49),   S(-21,  25),   S( 46,  59),   S( 25,  47),   S(-12,  46),   S(-20,  14),
            S( -6,  29),   S( 19,  33),   S( 18,  57),   S( 10,  41),   S( 46,  51),   S( 38,  61),   S( -4,   4),   S(-16,  -1),

            /* queens: bucket 7 */
            S(-15,  -9),   S(-11,   6),   S(-29,   0),   S(-22, -13),   S(-21,  -8),   S(-52, -18),   S( -9,  17),   S(-12,   2),
            S(-55,  -2),   S(-45,   2),   S(-37,  31),   S(  5,  15),   S( -1,  17),   S(-55,  35),   S(-76,  50),   S(-12, -33),
            S(-38,   4),   S(-46,  49),   S( -8,  13),   S(-13,  31),   S(  9,  32),   S(-14,  49),   S(  4,  10),   S(-10, -15),
            S(-51,   0),   S( -7,  17),   S(-44,  52),   S(  1,  30),   S( 41,  23),   S( 55,  16),   S(-27,  84),   S(  2,  38),
            S(-47,  28),   S(-49,  40),   S( 20,  33),   S( 10,  29),   S( 54,  25),   S( 66,  10),   S( 41,  26),   S( 32,   7),
            S(-48,  37),   S(  3,  10),   S(  4,  11),   S( 37,  16),   S( 44,  61),   S( 77,  13),   S( 98, -10),   S(  8,  28),
            S(  1,  -2),   S( 12,  39),   S( 24,   9),   S( 20,  24),   S( 38,  42),   S( 41,  28),   S( 45,  -9),   S( 40,  18),
            S( 17,  26),   S( 19,  -1),   S( 20,  27),   S( 50,  43),   S( 66,  23),   S( 42,  42),   S( 22,  -6),   S( 45,  65),

            /* queens: bucket 8 */
            S(-11, -17),   S( 12,  14),   S(-24, -19),   S(  0,  -6),   S( -6,  -9),   S(-11, -22),   S(-29, -39),   S(-12, -16),
            S( -3,   7),   S( -2,  -6),   S(-17, -17),   S(-13,   0),   S( -2, -17),   S(-14, -20),   S(-19, -53),   S(-10, -21),
            S(  5,  10),   S(-15, -11),   S(-16, -16),   S(  2,   0),   S(-31, -29),   S( -5,  -9),   S( -8, -25),   S(-11, -22),
            S( 12,  23),   S(  1,   5),   S( -4,   8),   S(-14,   8),   S(  5,  15),   S(-15, -12),   S( -7, -27),   S(-13, -49),
            S( 10,  23),   S( -2,  38),   S( 12,  30),   S( 23,  38),   S( 16,  39),   S(  6,  13),   S(-11, -10),   S(-11, -13),
            S(  7,  17),   S(-15,  -5),   S(-21,  19),   S( 18,  37),   S( -2,  -4),   S(  0,   2),   S(  2,   8),   S( -6, -14),
            S(-12, -25),   S(-51, -57),   S( -3,  26),   S( 16,  26),   S(-18, -25),   S(-11, -11),   S( -1,   4),   S( -8, -24),
            S( -4,  -5),   S( 12,  13),   S(-39, -74),   S( -6, -29),   S( -4, -31),   S( -3, -10),   S( -8, -34),   S(  0,  -4),

            /* queens: bucket 9 */
            S(  2,   2),   S(-15, -50),   S(  2,   4),   S(-35, -50),   S(-38, -72),   S(-23, -25),   S( -6, -13),   S(-29, -42),
            S( -6,  -7),   S(-28, -54),   S(-28, -38),   S(-20, -21),   S(-25, -29),   S(-15, -24),   S(-14, -28),   S( -5, -11),
            S(  8,  18),   S(-11,  -6),   S(-25,   8),   S(-24, -36),   S(-39, -57),   S(-14,  -9),   S(  0,   1),   S(-19, -32),
            S( -9, -12),   S(-14, -17),   S(  8,  30),   S(  9,  36),   S(  3,   9),   S(  1,  -6),   S( -7, -11),   S( -5, -26),
            S(  2,  11),   S( -1,  17),   S( -4,   9),   S( 25,  68),   S( 13,  53),   S( 15,  24),   S( -1,   1),   S(-21, -16),
            S(-41, -51),   S(-24,   5),   S( -3,   9),   S( 19,  42),   S(  0,   1),   S(-15,  -7),   S(-18, -31),   S(  0,  -3),
            S(-18, -19),   S(-13, -25),   S(-18,  20),   S( -1,  15),   S( 11,  18),   S( -3, -24),   S(  2,  -1),   S(-34, -63),
            S(-17, -36),   S(-31, -68),   S( -1,   0),   S( 19,  26),   S(  6,   0),   S( -4,  -8),   S(  3,   0),   S( -8, -11),

            /* queens: bucket 10 */
            S( -2,  -5),   S( -7, -20),   S(-18, -34),   S(-34, -35),   S(-18, -35),   S( -6, -13),   S( -8, -18),   S(-12, -23),
            S(-12, -24),   S(-14, -30),   S(-29, -42),   S(-17,  -9),   S(-19, -22),   S(-40, -36),   S(  5,  -4),   S(-33, -49),
            S(-13, -26),   S( -8, -18),   S( -9, -18),   S(  4,  22),   S( -5,  30),   S(-14,  10),   S( -2,  -3),   S(  0,   4),
            S(  6,   9),   S( -4, -13),   S(-13,   0),   S(  1,   6),   S(  9,  26),   S(-25,  12),   S(-11, -20),   S(-28, -43),
            S(-22, -37),   S( -7, -16),   S(-13,  -9),   S(  2,  26),   S(-13, -28),   S( -3,   1),   S( 14,   9),   S( -8, -17),
            S( -5,  -5),   S(-28, -39),   S(-15,  -1),   S( 10,  50),   S(-12,  28),   S(  0,  17),   S(  1,  -4),   S(-13, -22),
            S( -6,  -3),   S(-27, -39),   S( -1,   5),   S(-27, -16),   S(-18, -11),   S( 12,  12),   S( -7, -22),   S(-17, -27),
            S(  5,  15),   S( -3,  -4),   S(  0,   1),   S(  4,   9),   S(  6,   4),   S( -2,   0),   S(  6,  19),   S(  0, -19),

            /* queens: bucket 11 */
            S( -9, -31),   S(-17, -33),   S( -5,   5),   S(-10, -30),   S(-12, -26),   S(-21, -22),   S( -7, -11),   S(-17, -37),
            S( -9, -16),   S(-15, -32),   S(-39, -54),   S(-42, -42),   S( -2,  19),   S( -2,   1),   S( -3,   5),   S( -7,  -2),
            S(-29, -36),   S( -7, -27),   S( -4, -22),   S(-32, -41),   S(-18,  -5),   S(-31, -21),   S(-15,   7),   S(-21, -25),
            S(-17, -43),   S(-10,  -9),   S( -7, -23),   S( 10,  30),   S(  5,   7),   S(  3,  11),   S( 19,  30),   S( -3,  -7),
            S( -6,   1),   S(-15, -35),   S(-24, -34),   S( 16,  33),   S( 21,  16),   S( 25,  52),   S( 38,  75),   S(  3,  14),
            S(-13, -45),   S(  3,   3),   S(-22, -19),   S(  9,  20),   S( 14,  12),   S( 44,  47),   S( 30,  22),   S(-15, -13),
            S(-19, -28),   S(  1,   1),   S( 12,  32),   S(-27, -42),   S(  9,  20),   S( 17,  31),   S( 29,  40),   S( -3, -14),
            S(-19, -40),   S(-17, -33),   S(-12, -29),   S(  5,   5),   S( -8, -14),   S(  0, -24),   S( 19,  13),   S(-20, -49),

            /* queens: bucket 12 */
            S(  5,  10),   S(  1,  -3),   S( -5,  -2),   S(-13, -13),   S( -7, -11),   S( -6, -12),   S( -2,  -4),   S( -8, -14),
            S(  5,   6),   S( -5, -15),   S(-10, -16),   S( -6, -15),   S(-10, -20),   S(-13, -21),   S( -6, -12),   S(-11, -21),
            S(  3,  -3),   S( -1,  -4),   S( 21,  26),   S( -8, -19),   S( -9, -15),   S(-13, -24),   S(-12, -24),   S(-10, -18),
            S(  1,   0),   S(  1,   5),   S(  0,   5),   S(  0,  13),   S(  5,  15),   S( -6,  -9),   S( -1,   6),   S(-10, -26),
            S(  1,   1),   S(  3,  10),   S( 20,  36),   S(  4,   7),   S(  4,  19),   S(  4,   9),   S(-18, -28),   S(  0,   2),
            S( 11,  22),   S(-12, -17),   S( 37,  67),   S(  1, -10),   S( -8, -11),   S(  1,   4),   S(  1,   4),   S( -8, -23),
            S( -3,  -7),   S( 12,  24),   S( 19,  33),   S( 10,  19),   S(  1,  -5),   S(  8,  14),   S(  1,  -4),   S(  3,  11),
            S( -7, -21),   S(-21, -46),   S(-29, -37),   S(-17, -28),   S(  9,  16),   S(  0,  -2),   S(  2,  -6),   S( -2,   1),

            /* queens: bucket 13 */
            S( -6, -18),   S(-12, -27),   S(  1,  -1),   S( -9, -14),   S( -6, -10),   S( -3, -10),   S(-14, -32),   S( -6, -12),
            S(  0,  -2),   S(  6,  16),   S(  3,   7),   S(-14, -31),   S(-14, -31),   S( -1,  -2),   S(  0,  -3),   S( -7, -16),
            S( -5,  -9),   S(  2,   2),   S( -6, -11),   S( -7, -30),   S( -5,   0),   S( -6, -13),   S( -5, -10),   S(-18, -28),
            S( -5, -13),   S(  6,  13),   S(  9,   3),   S( 25,  36),   S( 29,  48),   S(-20, -46),   S(-15, -36),   S(-13, -30),
            S(-12, -22),   S(  5,  16),   S( 17,  35),   S( 10,  27),   S( 23,  48),   S(-12, -24),   S(-13, -34),   S( -7, -12),
            S( -5, -10),   S( 12,  30),   S( 40,  77),   S( 25,  69),   S( -6,   6),   S(  0,  12),   S(  7,  19),   S(-13, -34),
            S( -4,  -9),   S(  7,  18),   S( -3,  -7),   S( 21,  45),   S( -4,  -7),   S( -4,  -7),   S(  1,   0),   S( 16,  34),
            S(-26, -52),   S(-13, -23),   S( -6, -12),   S(-26, -40),   S( -1,  -9),   S(  5,   8),   S( -8, -12),   S(-10, -22),

            /* queens: bucket 14 */
            S( -1,   0),   S(  0,  -1),   S( -2,  -9),   S( -9, -17),   S(  0,  -4),   S( -5, -15),   S(  0,  -8),   S( -7, -21),
            S( -5, -12),   S(  9,  17),   S( -7, -18),   S(  3,  -3),   S(-16, -26),   S(-14, -40),   S( -4,  -8),   S( -5, -17),
            S( -5,  -5),   S( -9, -20),   S(-19, -24),   S( -6, -16),   S( -1,  -1),   S( -7, -22),   S( -3,  -5),   S(-14, -19),
            S(-10, -19),   S( 13,  25),   S(  0,   1),   S( 13,  31),   S(  5,  13),   S( -8,  -4),   S(  2,   5),   S( -2, -12),
            S( 10,  21),   S( -2,   3),   S(-10,  -5),   S( 23,  26),   S( 12,  29),   S( 19,  30),   S( 12,  15),   S( -5, -14),
            S( -3, -10),   S( -7, -16),   S(  9,  14),   S(  3,  15),   S( 12,  23),   S( 17,  42),   S(  8,   8),   S( -1,  -8),
            S(  0,   3),   S( 11,   9),   S( 13,  19),   S( 10,  16),   S( 20,  32),   S(  6,  15),   S(  8,   8),   S(  1,   1),
            S(  4,   5),   S(-10, -16),   S( -7, -22),   S(  3, -18),   S(  2,   5),   S( -2,  -9),   S( -8, -19),   S(-12, -31),

            /* queens: bucket 15 */
            S( -5, -10),   S( -4, -17),   S( -3,  -7),   S( -6, -15),   S( -4,  -6),   S( -4,  -5),   S( -3,  -8),   S( -1,  -5),
            S( -6, -21),   S( -1,  -4),   S(-14, -25),   S(  1,   2),   S( -1,  -2),   S( -3,  -3),   S( 10,  20),   S(  2,   3),
            S( -2,  -5),   S(-11, -28),   S( -5, -10),   S( -5, -10),   S(  0,  -5),   S( 10,  17),   S( -2,   1),   S(  2,   0),
            S(-13, -19),   S(  1,  -2),   S( -3,   2),   S(  6,   2),   S( -2,  -2),   S( -3,  -3),   S(  8,   9),   S( -3,  -3),
            S( -1,   3),   S( -6, -19),   S( -6, -16),   S( -1, -14),   S(  9,  13),   S( 17,  29),   S( -5, -15),   S( -2,  -7),
            S(  1,   6),   S( -3,  -9),   S(  5,  12),   S( -2, -13),   S(  3,  -1),   S( 13,  25),   S(  3,  16),   S(  4,  10),
            S( -5,  -9),   S( -4, -17),   S(  8,   9),   S( 15,  27),   S( 10,  18),   S( 16,  35),   S(  8,   1),   S(  1,   3),
            S( -4,  -5),   S(-10, -25),   S( -7, -13),   S( 11,   9),   S(  2,   1),   S(  0, -14),   S( -3,  -9),   S( -6, -17),

            /* kings: bucket 0 */
            S( 24,  41),   S(  4,  71),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  57),   S( 84,  72),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-24,  23),   S(-69,  32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 18,  41),   S(  0,  46),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-44,  38),   S(-45,  32),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 24,  49),   S( 27,  38),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 17,  61),   S( -2,  41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 62,  87),   S( 11,  64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -45),   S( 32, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-53,  -8),   S( 17,  11),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  4, -43),   S(-38, -29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-11,  -8),   S(  1, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-17, -17),   S(-30, -14),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,  -4),   S(-20,   4),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 33,  13),   S(-12, -18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 30,  43),   S(-41,  16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-109, -46),  S(  3, -16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57, -45),   S( 34, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-15, -48),   S(-22, -57),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 64, -61),   S( 16, -39),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-24, -46),   S(-37, -39),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 66, -59),   S( 89, -48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  7, -30),   S(-88, -55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 55, -38),   S(  6, -57),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31, -39),   S( 41, -29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -93),   S(-25, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 22, -40),   S( 90, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11, -65),   S( 14, -70),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 59, -49),   S( 69, -53),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  4, -61),   S(-15, -60),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 48, -44),   S(-60, -61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  3, -69),   S(  5, -118),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -13),   S(-47,  15),   S( -9,  -6),   S( -5,   0),   S( -5,   2),   S( 19,  16),   S( 29,  -8),   S( 36,   1),
            S(-13, -34),   S(-37, -10),   S(-19,  -7),   S(-16,   0),   S(  2,   5),   S( -8,   7),   S( 24,  -7),   S( 14,  16),
            S( 10, -20),   S( -5, -21),   S( 25, -26),   S(  6, -24),   S( 15,  -1),   S( 12,  23),   S(  5,  35),   S( 44,  13),
            S( 32, -25),   S( 46,  -9),   S( 57, -30),   S( 41,   7),   S( 21,  32),   S( -3,  77),   S( 24,  60),   S( 71,  48),
            S( 93, -64),   S(104,  -7),   S( 95,  -9),   S( 64,  17),   S( 42, 123),   S( 14,  98),   S( 44, 123),   S( 99,  92),
            S(-196, -97),  S(-157, -99),  S( 42, -152),  S( 19,  50),   S( 95, 149),   S( 73, 149),   S(117, 103),   S( 76, 141),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  18),   S(-60,  20),   S(-31,   5),   S(-54,  58),   S(-34,   6),   S( -4,   9),   S( -1,  -1),   S(  3,  18),
            S(-53,   4),   S(-51,   3),   S(-44,  -4),   S(-27,  -3),   S( -1,  -3),   S(-22,   2),   S(-11,  -5),   S(-17,   6),
            S(-39,  21),   S(-15,   9),   S(-20,  -3),   S( 10, -23),   S(  5,  13),   S( -7,  10),   S(-22,  21),   S(  3,  15),
            S(-13,  31),   S( 33,   1),   S(-13,  19),   S( 23,  21),   S( 14,  21),   S(-13,  26),   S( 16,  24),   S( 43,  43),
            S( 18,  17),   S(105, -35),   S( 96, -24),   S(101, -37),   S( 46,   7),   S( 39,  26),   S(-44,  56),   S( 46,  89),
            S(170, -51),   S(  9, -44),   S( 40, -126),  S( 30, -125),  S(-34, -69),   S(-23,  54),   S( 54, 135),   S( 96, 131),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-63,  43),   S(-52,  23),   S(-38,   9),   S(-30,  28),   S(-57,  35),   S(-37,  17),   S(-21,  -8),   S(-29,  23),
            S(-53,  18),   S(-50,  19),   S(-49,   6),   S(-48,  22),   S(-44,  20),   S(-51,   8),   S(-25, -12),   S(-53,  12),
            S(-35,  41),   S(-37,  41),   S(-20,  14),   S(-23,  19),   S(-31,  29),   S(-26,   8),   S(-26,   5),   S(-22,   8),
            S( -8,  70),   S(-29,  59),   S(-10,  30),   S(  3,  34),   S( -4,  29),   S(-16,  19),   S(  9,  15),   S( 30,   4),
            S(-36, 126),   S(-52, 102),   S(-24,  37),   S( 26, -26),   S(115, -27),   S(106,  -5),   S( 84, -33),   S( 46,   5),
            S(  9, 204),   S( 56, 114),   S( 36,  44),   S( 23, -84),   S(-17, -176),  S(-17, -173),  S( -4, -76),   S(114, -44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12,  24),   S(-20,  18),   S(-11,  18),   S(-15,  44),   S(-30,  57),   S(  5,  38),   S( -6,  10),   S(-15,   0),
            S(  1,   5),   S( -2,  14),   S(-18,  14),   S(-18,  20),   S( -6,  25),   S(  0,  10),   S( -6,  -3),   S(-35,   1),
            S( 13,  36),   S( -7,  50),   S(  3,  21),   S( -2,   8),   S( 16,   2),   S( 14,   1),   S(  1,  -8),   S(-10,  -2),
            S( 23,  79),   S( -1,  87),   S( 12,  65),   S( 18,  33),   S( 33,   6),   S( 34, -13),   S( 26,  10),   S( 43, -16),
            S(  2, 148),   S(-22, 168),   S(-27, 169),   S(-15, 121),   S( 49,  51),   S( 87,  -5),   S( 95, -25),   S( 92, -19),
            S( 92, 133),   S( 48, 218),   S( 12, 256),   S( -2, 197),   S(-38,  90),   S( -4, -142),  S(-93, -188),  S(-170, -162),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 70, -11),   S( 25, -10),   S(-31,   4),   S(  6, -28),   S(  0, -13),   S(-20,  -5),   S(  5, -13),   S(-42,  21),
            S( 53, -25),   S( 22,  -5),   S( 18, -15),   S(-23, -11),   S( -9, -30),   S(-13, -20),   S(-47, -15),   S(-50, -10),
            S(105, -32),   S(127, -48),   S( 58, -32),   S(  4, -10),   S(-58,   6),   S(-27,  15),   S(-75,  23),   S(-44,  22),
            S(-11, -87),   S( 26, -117),  S( 61, -28),   S(-24,  22),   S(  3,  16),   S(-67,  50),   S(-17,  39),   S( 27,  25),
            S( 38, -102),  S(-36, -120),  S(  8, -84),   S( 63, -22),   S( 64,  79),   S( 34,  61),   S( 32,  62),   S( -6,  87),
            S(  8, -31),   S( -8, -69),   S( -6, -103),  S( 29,  36),   S( 78,  97),   S( 53, 148),   S( 43, 116),   S( 28, 116),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-69,  38),   S(-17,  21),   S(  4,   3),   S( 48,  -9),   S( 80, -17),   S( 26,  -9),   S(-33,  -2),   S(-56,  36),
            S(-74,  13),   S(  3,   1),   S( 26, -18),   S(-11,  11),   S(  2,   1),   S(  6, -13),   S(-70,   1),   S(-70,  17),
            S(-24,  17),   S( 29,  14),   S( 70,  -5),   S( 28,  15),   S( -3,  26),   S(-13,   6),   S(-36,   9),   S(-41,  28),
            S( 34,   7),   S( 55, -21),   S(  9, -52),   S(-42,   2),   S(  9, -14),   S(  5,   5),   S( 45,   3),   S( 23,  24),
            S( 71,  -9),   S( 57, -55),   S( 53, -90),   S( 18, -77),   S( 45, -40),   S( 69,   2),   S(  7,  21),   S(  0,  79),
            S( 60,  18),   S( 39, -26),   S( -9, -80),   S( 12, -79),   S( -3, -74),   S( 43,  30),   S( 75, 100),   S( 85,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-80,  27),   S(-40,   2),   S(-13,  -6),   S( -8,  17),   S( 18,   4),   S( 24,   4),   S( 18,  -5),   S(  1,  20),
            S(-59,   1),   S(-33, -11),   S(-28, -13),   S( 45, -13),   S(  1,   9),   S(  8,  -8),   S( 13, -11),   S( -2,  -1),
            S(-32,  18),   S(-37,  17),   S( -1,   5),   S( 14,   7),   S( 35,  16),   S( 69, -13),   S( 61, -11),   S( 20,   0),
            S(-26,  48),   S(  9,  20),   S( 33,  -2),   S( 48,  -7),   S( -3, -32),   S( 10, -37),   S( 53, -26),   S(111, -36),
            S(  5,  66),   S( 31,  21),   S( 47,   4),   S( 48, -55),   S(-13, -71),   S(-16, -57),   S( 49, -75),   S(127, -19),
            S(105,  37),   S(106,  51),   S( 62,  -3),   S( 69, -78),   S( 13, -117),  S( -3, -106),  S(  0, -57),   S( 56, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,   4),   S(-46, -17),   S(-12, -22),   S(-37,  13),   S( 10,   5),   S( 66, -15),   S( 52, -26),   S( 60,  -9),
            S(-47, -15),   S(-47, -16),   S(-42, -18),   S(-22,  -1),   S( -4,  -4),   S( 49, -35),   S( 40, -24),   S( 60, -28),
            S(-34,  15),   S(-50,  18),   S(-14,  -9),   S(-10, -20),   S( 13,  -6),   S( 31, -18),   S( 76, -22),   S( 78, -25),
            S( -4,  33),   S(-36,  48),   S(-18,  38),   S(  6,  26),   S( 24, -13),   S( 84, -62),   S( 30, -68),   S( 28, -96),
            S( 31,  39),   S(-47, 112),   S( 64,  79),   S(-23,  88),   S(-13,  32),   S( 12, -56),   S(-23, -135),  S(-16, -95),
            S(111,  81),   S(125, 101),   S( 89, 101),   S( 53,  83),   S( 82,  -2),   S( 12, -98),   S( -2, -100),  S( 16, -147),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 28, -15),   S(-11, -12),   S( 14,   9),   S(-22, -38),   S(-17, -64),   S( 43, -43),   S( 33, -66),   S( 48, -37),
            S( -6, -48),   S(-14, -11),   S(-34, -59),   S(-87, -37),   S(-73, -37),   S( 17, -47),   S(  5, -52),   S(-32, -37),
            S(-47, -67),   S( 41, -74),   S( -1, -50),   S(-50, -40),   S(-32,  -8),   S(-35, -16),   S(-32, -22),   S(-46,   1),
            S(-24, -16),   S(-27, -20),   S( 32, -13),   S( 46,   0),   S(-12,   8),   S(-13,  37),   S(  9,  27),   S( -3,  25),
            S( 13,  40),   S(  3, -15),   S(  8,  27),   S( 44, 102),   S( 57, 138),   S( 38, 114),   S(  2,  95),   S(-26, 109),
            S( 20,  48),   S(  9,  39),   S( 27,  68),   S( 48, 102),   S( 55, 123),   S( 45, 153),   S( 45, 105),   S( -6,  58),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 34,  -2),   S( 88, -16),   S( 52,  -6),   S( -2,  -2),   S( -2, -12),   S( 67, -42),   S( 68, -59),   S(  1, -29),
            S(-16, -41),   S(-42, -35),   S(-24, -41),   S(-22, -26),   S( -3, -33),   S(-15, -43),   S(  2, -49),   S( 25, -52),
            S(-69, -22),   S(-17, -54),   S( -7, -77),   S(-40, -47),   S(  0, -30),   S(  3, -49),   S(-24, -42),   S(-37, -11),
            S(-42,   4),   S(-72, -40),   S( 18, -54),   S(-17, -26),   S(  3, -27),   S(-22, -15),   S(-25,   9),   S( 17,  -5),
            S(-21,  11),   S(-17, -36),   S( 21, -28),   S( 45,  26),   S( 12,  49),   S( 13,  28),   S(  3,  53),   S(-15,  54),
            S(-10,  33),   S( 38,  71),   S( 13,  28),   S( 17,   3),   S( 17,  53),   S( 11,  50),   S(  4,  79),   S( 12,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -60),   S( 21, -61),   S(  5, -28),   S( -1, -32),   S( 49, -23),   S(160, -30),   S( 76, -28),   S( 51, -29),
            S(-26, -63),   S(-52, -64),   S( 27, -61),   S( 36, -40),   S(  7, -28),   S(  7, -28),   S(  7, -44),   S( 17, -39),
            S(-60, -28),   S(-56, -38),   S( -9, -40),   S( 18, -43),   S(-27, -37),   S(-12, -65),   S( -6, -70),   S( 27, -42),
            S(-22, -14),   S(  9, -14),   S( 15, -27),   S(-22, -42),   S(-15, -36),   S(-38, -43),   S( -5, -46),   S(-21, -25),
            S( -8,   4),   S( 20,   3),   S( 17,  13),   S( -3, -19),   S( 39,   3),   S( 23,  23),   S( 16, -27),   S( 32,   3),
            S(  6,  -3),   S( -2,  13),   S( 27,  42),   S( 17,  34),   S( 24,  31),   S( -7,  12),   S( -8, -19),   S( 21,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-77, -32),   S( 14, -43),   S( -5, -32),   S(  8,  14),   S( 20, -18),   S( 93,  -4),   S( 65, -33),   S( 77, -17),
            S(-36, -60),   S(-54, -64),   S(-24, -80),   S( 13, -45),   S(-15, -26),   S( -2, -40),   S( 16, -31),   S(-11, -37),
            S(-49, -19),   S(-24, -32),   S(-26, -24),   S( 32, -45),   S(-24, -42),   S(  3, -42),   S(-51, -77),   S( -5, -62),
            S( 10,   4),   S(-29,   8),   S( 19,  37),   S( 34,  -5),   S(  9, -18),   S(-17, -26),   S(-44, -42),   S( -4, -34),
            S( 12,   7),   S(  8,  60),   S( 22,  74),   S( 33,  56),   S( 36,  45),   S( -1,  -9),   S(  2,   3),   S(-10, -11),
            S( 24,  54),   S( 36,  49),   S( 34,  86),   S( 32,  48),   S( 21,  49),   S( 40,  91),   S( 18,  31),   S( 39,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -59),   S( -6,   7),   S( -9,   3),   S(  2,  13),   S( -4,  -7),   S( -7, -78),   S( 39,  -6),   S( 16, -22),
            S(-16, -21),   S(-31, -12),   S(-29, -65),   S(-16, -27),   S(-41, -78),   S( -9, -67),   S(-20, -52),   S(-20, -62),
            S( -9,   0),   S( -7, -60),   S(-24, -81),   S(-30, -73),   S(-15, -29),   S( -7, -23),   S(-48, -11),   S(-60, -35),
            S( -5,   5),   S( 14,   8),   S(-14,  -5),   S( 10,  12),   S( 27,  60),   S(-13,  73),   S(-17,  57),   S(-30,  34),
            S( 23,  24),   S( 10,  11),   S(  1,  24),   S(  9,  63),   S( 26,  72),   S( 15,  68),   S( 15, 117),   S( -4,  87),
            S( 15,  30),   S( 13,  38),   S( 19,  65),   S( 22,  70),   S( 31, 113),   S( 34, 119),   S(-13,  43),   S(-29,  25),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26, -60),   S(-20, -47),   S(  7,  -5),   S(-10, -41),   S(  0, -24),   S( -7, -23),   S(-10, -65),   S(-17, -42),
            S(-74, -75),   S(-30, -63),   S(-36, -47),   S( -3, -33),   S(-32, -44),   S(-32, -39),   S(-23, -63),   S(-14, -51),
            S(-44, -45),   S(-24, -63),   S(-19, -60),   S(-14, -47),   S(-21, -13),   S( -4, -48),   S(-40, -23),   S(-28, -32),
            S(-39,   6),   S( -3, -18),   S( -8, -23),   S( -1,  17),   S(  9,  48),   S(-44,  20),   S(-14,   9),   S(-24,  22),
            S( -9,  24),   S(  1,  28),   S( -5, -30),   S( 19,  32),   S( 41, 127),   S( 31, 117),   S( 28, 134),   S(-18,  77),
            S(  9, 104),   S( 23,  69),   S(  7,  29),   S( 27,  68),   S( 13,  63),   S( 16,  79),   S( -3,  73),   S(  1, 114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -81),   S(  5, -47),   S( -4, -24),   S( -2,   0),   S(-13, -55),   S( -7,  -8),   S( -6, -47),   S(-17, -52),
            S(-26, -82),   S(-82, -78),   S(-12, -76),   S(-36, -109),  S(-24, -36),   S(-16, -27),   S(-37, -21),   S(-13, -51),
            S(-20, -59),   S(-28, -51),   S(-11, -41),   S( -5, -54),   S( -4, -47),   S(-22, -73),   S(-34, -53),   S(-41,   5),
            S(-20,  -6),   S(-53, -46),   S(-11,  21),   S( -8, -14),   S(  7, -61),   S(  7, -12),   S(  8, -12),   S(-16,  28),
            S( -9,   3),   S( 11,  65),   S( 13,  60),   S( -4,  42),   S( 33,  63),   S(  3,  14),   S( 20,  78),   S( 16,  72),
            S(-10,  66),   S( 13,  62),   S( 30, 117),   S( 21,  81),   S( 23,  88),   S(  1,  55),   S( 32, 120),   S( 27, 117),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -29),   S( 17, -43),   S(  1, -45),   S( -8, -22),   S( -4,  -6),   S(-36, -36),   S(-22, -34),   S(-14, -67),
            S(-25, -53),   S(-33, -74),   S(-21, -84),   S( -2, -42),   S(  2, -19),   S(-31,  -9),   S(-32, -24),   S(-14, -59),
            S(-25,  -5),   S(-53, -23),   S(-43, -26),   S( 31, -21),   S(-20,  -8),   S(  3, -35),   S(  7, -19),   S(-10, -19),
            S(-27,  -8),   S(-37,  23),   S(  9,  55),   S(-10,  12),   S(  0,  50),   S(-14, -17),   S( -4,  15),   S( -9,  35),
            S(  2,  57),   S(  5,  17),   S(  3,  53),   S( 22,  81),   S( 30, 104),   S( 21,  88),   S( 26,  85),   S( 13,  -1),
            S(-25,  54),   S( -6,  10),   S(  6,  80),   S( 23,  71),   S( 20,  98),   S( 20,  79),   S( 12,  59),   S( 20,  69),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-113, -53),  S(-35, -19),   S( -2, -36),   S(  3,  18),   S(-21, -21),   S(-27,  25),   S(  5, -18),   S(-98, -60),
            S(  4, -13),   S(-14,  18),   S(-25, -12),   S( -6,   6),   S(  0,  11),   S(-19,  -2),   S(-29, -35),   S(-21, -25),
            S( 20, -55),   S( 15,   1),   S( -3,  27),   S( 40,  32),   S( -4,  21),   S(  2,  17),   S(-22,  30),   S(-27, -14),
            S(  8,  26),   S( 15,  68),   S( 22,  51),   S( 50,  20),   S(  8,  59),   S( 12,  39),   S( 23,   7),   S( -4, -12),
            S( 25,  68),   S(-10,  64),   S( 39,  75),   S( 45,  57),   S( 81,  48),   S( 27,  19),   S( 22,   0),   S( 16,  -3),
            S( 98, -22),   S(-10,  54),   S(116,  18),   S( 80,  27),   S( 80,  53),   S(-34,  65),   S( 40,  29),   S(-24,  29),
            S( 10, -21),   S(-25, -41),   S( 53,  -4),   S( 68,  65),   S( 51,  52),   S( -9,  46),   S(-12,  11),   S( -6,  -4),
            S(-94, -153),  S( -5,  -5),   S(-11, -10),   S( 28,  46),   S(-15,  35),   S( 27,  28),   S(-61,   1),   S(  1,  15),

            /* knights: bucket 1 */
            S( 15,  -1),   S(-65,  36),   S(-23,  14),   S(-66,  55),   S(-29,  37),   S(-16,  -5),   S(-27,   2),   S( 39,  11),
            S(-39,  43),   S(-42,  54),   S(-31,  37),   S(-13,  29),   S(-25,  34),   S( -7,  41),   S(-24,  17),   S( -7, -47),
            S(-42,  30),   S( -3,  12),   S(-27,  28),   S(-12,  58),   S(-15,  51),   S(-21,  30),   S(-50,  53),   S(-14,  32),
            S( -3,  68),   S( 28,  42),   S( -3,  58),   S(-13,  60),   S( -9,  67),   S(-28,  81),   S(-13,  52),   S(-15,  41),
            S( 49,   7),   S(-12,  17),   S(  1,  89),   S( -1,  64),   S( 28,  57),   S(  6,  58),   S( -3,  45),   S(-21,  76),
            S(-11,  35),   S( 50,  -7),   S(102,  37),   S(104,  50),   S( 60,  60),   S(-25,  77),   S( 30,  50),   S( 29,  48),
            S(  7, -24),   S( 41,  -9),   S( 28,  -6),   S( 35,  41),   S(-15,  12),   S( 59,  -4),   S( 11,  84),   S(-27,  68),
            S(-167, -52),  S( 12,  -2),   S(-50, -78),   S(-15,   7),   S(-13,   4),   S( 40,  35),   S( 16,  34),   S(-75,  33),

            /* knights: bucket 2 */
            S(-67,  27),   S(-42,  39),   S(-44,  20),   S(-24,  33),   S(-28,  43),   S(-52,  24),   S(-28,  26),   S(-29,  -3),
            S(-41,  28),   S( -1,  48),   S(-37,  37),   S(-16,  37),   S(-34,  44),   S(-15,  20),   S( -4,  19),   S(-32,  18),
            S(-45,  56),   S(-22,  42),   S(-33,  41),   S(-33,  76),   S(-30,  62),   S(-31,  36),   S(-34,  35),   S( -4,  14),
            S(-12,  67),   S(-22,  62),   S(-26,  88),   S(-38,  94),   S(-32,  74),   S(-12,  63),   S( -9,  51),   S(-10,  50),
            S(-20,  78),   S(-20,  77),   S(-10,  75),   S(  5,  69),   S(-36,  93),   S( 10,  73),   S(-39,  88),   S( 10,  20),
            S(-29,  75),   S(-19,  91),   S(-25, 108),   S( 34,  34),   S( 41,  45),   S(117,  17),   S( 67,  27),   S(  7,   8),
            S(  0,  63),   S(-62,  76),   S( 33,  35),   S( 22,  11),   S(  4,  47),   S( 18,  37),   S( 17,  43),   S( 31,   7),
            S(-75,  48),   S( 23,  83),   S(  5,  62),   S(-49, -38),   S( -8,   1),   S(-40, -57),   S( 46,  21),   S(-143, -53),

            /* knights: bucket 3 */
            S(-46,  34),   S(-20, -16),   S(-11,  16),   S(-17,  33),   S( -7,  32),   S( -5,  14),   S(-22,   4),   S(-49, -14),
            S(-21,  16),   S(  1,  33),   S(  2,  21),   S( -5,  30),   S(-12,  38),   S( 15,  17),   S( 13,   4),   S( 12, -13),
            S(-20,  34),   S(-12,  40),   S( -9,  54),   S(-11,  81),   S( -1,  71),   S( -9,  51),   S(  7,  38),   S(  9,  12),
            S(  3,  32),   S(  3,  62),   S(  7,  81),   S(-10,  95),   S(  7,  98),   S( 17,  93),   S( 19,  80),   S( -2,  78),
            S( -8,  79),   S(  2,  75),   S( 13,  94),   S( 23, 116),   S(  0, 106),   S( 12, 120),   S(-34, 121),   S( 29, 108),
            S(-14,  67),   S(  7,  82),   S( 24,  93),   S(  8, 108),   S( 69,  89),   S( 76, 128),   S( 59,  99),   S( -4, 139),
            S(-15,  73),   S(-17,  83),   S(-37, 110),   S( 30,  80),   S( 55,  77),   S( 96,  48),   S( 16,  19),   S( 64,  13),
            S(-177,  98),  S(-14,  94),   S(-53, 106),   S( 29,  74),   S( 73,  66),   S(-92,  79),   S(-11, -41),   S(-59, -149),

            /* knights: bucket 4 */
            S(  9,  33),   S(  3,   2),   S(-28,   4),   S(-51, -26),   S(-35,   4),   S(-23, -26),   S(  3, -33),   S(-16,  -9),
            S( 31, -11),   S(-10, -46),   S(-19,  15),   S( -3,  -3),   S( 13, -10),   S(  8, -39),   S( 36,  36),   S(-42,  -6),
            S(  2,  12),   S( 14,  -8),   S( 55,  20),   S( 85, -12),   S( 38,  25),   S( 52, -23),   S(  0, -15),   S(  3, -41),
            S(-20, -23),   S( 12, -18),   S( 11, -14),   S( 72,   6),   S( 19,  32),   S( -5,  37),   S( -4,  10),   S(-28, -22),
            S( 18, -50),   S(  9,  -8),   S( 71,  17),   S( 50,  43),   S(104,   8),   S( 24,  51),   S(  2,  20),   S(-34,  38),
            S(-12, -35),   S(-11, -28),   S( 31,  11),   S( 67,  30),   S(  7,  19),   S( 15,  44),   S(-38,  23),   S( 30,   4),
            S( -3,   4),   S(-23, -40),   S( -9,  -9),   S( 28,   3),   S( 24,  24),   S(  2,  19),   S( -1,  16),   S(-52, -32),
            S(  5,  12),   S(-18, -49),   S(  4,  -6),   S( 17,  10),   S( 20,  18),   S( -4,  23),   S( -2,  24),   S( -7,  -9),

            /* knights: bucket 5 */
            S( 10,  16),   S( 10,  15),   S(-23,  28),   S( -9,   6),   S(  4,  20),   S(-21,  12),   S(-37,  23),   S(  8,  25),
            S( 23,  34),   S( 69,  25),   S( 53, -17),   S( -2,  -2),   S( 44, -14),   S(-26,  17),   S(  0,  61),   S(-60,  11),
            S(-21,  -6),   S( -6,   5),   S( 20,   3),   S( 49,   2),   S( 35,  13),   S(  9,   8),   S( 15,   7),   S(-55,   8),
            S( 23,  12),   S( 29, -27),   S( 83,   0),   S( 74,  -3),   S( 87,  10),   S( 93,  10),   S(  3,  20),   S( 27,  28),
            S( 63,   3),   S( 51,   0),   S(119, -17),   S(135, -12),   S(138, -26),   S( 44,  34),   S( 38,   1),   S( 18,  25),
            S(  7, -12),   S( 38, -28),   S( 11, -32),   S(-15,   6),   S( 11,  -1),   S( 46,   6),   S(-21,  29),   S( 30,  20),
            S( -3,   1),   S(-19, -35),   S(  1, -48),   S(  0,  -1),   S( -6, -43),   S( 21,  11),   S(  0,  42),   S( 11,  25),
            S(-26, -29),   S(-32, -65),   S( 16, -16),   S( -6, -40),   S(  5,  13),   S(  4,  27),   S( 13,  39),   S( -3,   5),

            /* knights: bucket 6 */
            S(  3,   3),   S(-15,  18),   S(  6,  -6),   S(-26,  38),   S(  0,  20),   S( 29,  29),   S(-36,  50),   S(-14,   7),
            S(  9, -38),   S( 11,  31),   S( -4,  -2),   S( 41,   5),   S( 62,   1),   S(  8,  18),   S( 16,  40),   S(-37,  47),
            S(-12,  16),   S( 52,   1),   S( 57,   4),   S( 40,  25),   S( 44,  28),   S(-38,  35),   S( 30,  28),   S( -4,  25),
            S( 43,  22),   S( 87,   3),   S( 42,  20),   S( 80,   3),   S( 96,  -8),   S( 64,  21),   S( 35,  -7),   S(-51,  58),
            S( -9,  50),   S( 61,   6),   S(137,   2),   S(137,   2),   S( 88,  -2),   S( 47,  30),   S(149, -10),   S( 39,   7),
            S( 11,  20),   S( 41,  -3),   S( 44,  25),   S( 80,   0),   S( 59,   2),   S( 79, -20),   S(  8,  -8),   S( 25,  -1),
            S( 11,  25),   S( 22,  47),   S( 28,  53),   S( -5, -13),   S( 33,  21),   S( 42, -23),   S(  7,   6),   S( 15,  41),
            S( 26,  33),   S(-19,  16),   S( 16,  14),   S(  8,  21),   S( 18, -29),   S(  7, -11),   S( 26,  47),   S(-33, -45),

            /* knights: bucket 7 */
            S( -7,  13),   S(  4, -29),   S( -1,  -4),   S(-20,  26),   S(  3,  12),   S(-52,  32),   S(  2, -30),   S( -4,  28),
            S( -9, -49),   S(-22, -16),   S( -1, -12),   S(-26,  15),   S( 10,   5),   S( 18,  21),   S( 24,  14),   S(-53,  27),
            S(-22, -42),   S(-56,   9),   S(  7,  -5),   S( 39,  14),   S( 49,  14),   S( 40,  21),   S( 26,  19),   S( 16,  26),
            S(-57,  20),   S( 22,   3),   S( 36,   8),   S( 92,   3),   S(133, -18),   S( 99,  17),   S( 69,   5),   S( 91,   3),
            S( 14,   4),   S( 31,  -2),   S( 27,  28),   S(108,   3),   S(134,   9),   S(159, -13),   S(216, -16),   S( 72,   7),
            S( -3,  39),   S( 57,  27),   S( -7,  30),   S( 98,   0),   S(116,  -9),   S(101,  14),   S( 33, -10),   S(-11, -53),
            S(-29,  41),   S(-19,  16),   S( 28,  31),   S( 58,  33),   S( 52,  40),   S( 60,  41),   S(-22, -27),   S(-10, -35),
            S(-18, -26),   S(  2,  30),   S(  8,  49),   S(  6,  10),   S( -3,  22),   S(  4,   3),   S( 17, -13),   S( -4, -17),

            /* knights: bucket 8 */
            S( -6, -16),   S(  1, -19),   S(  7,  -9),   S(-13, -51),   S(  1,  -1),   S( -6, -39),   S( 11,  34),   S( -3, -31),
            S( -8, -47),   S(-10, -55),   S(  4, -68),   S(-12, -26),   S( -1, -10),   S(  2, -45),   S( -4, -29),   S( -6, -26),
            S( -9, -53),   S( -1, -31),   S( 11, -54),   S(  3, -59),   S( 36, -12),   S( 16, -35),   S( -1, -43),   S(  1, -22),
            S(-19, -72),   S(-11, -39),   S(  9,   2),   S( 12,  11),   S(  9, -54),   S(  6, -37),   S(-22, -34),   S(-19, -43),
            S( -5, -20),   S(  3, -30),   S( -1, -34),   S(  3, -45),   S(  8, -32),   S(-11, -29),   S( -9,  -8),   S( -1, -17),
            S(  1,   0),   S(  6, -56),   S(  8,  11),   S(  8, -30),   S(  0, -19),   S(-10, -43),   S( -4,  -6),   S( -5, -14),
            S( -3,   8),   S( -6, -64),   S(-19, -35),   S( -1,   1),   S(  4, -25),   S(  6, -25),   S(-12, -14),   S( -8, -31),
            S( -1,   0),   S( -4,  -4),   S(-10, -23),   S(  6,   0),   S(  6,  28),   S(  0,  -7),   S(  2,   6),   S( -4,  -3),

            /* knights: bucket 9 */
            S(-14, -68),   S( -6, -31),   S(-13, -67),   S( -3, -78),   S(-14, -30),   S(-18, -33),   S(  8, -19),   S(  1,  -6),
            S( -6, -12),   S(-18, -70),   S(-16, -115),  S(-10, -97),   S(-13, -76),   S( -8, -90),   S(  1, -30),   S( -4, -21),
            S( -7, -65),   S(-11, -55),   S(  6, -44),   S( -2, -70),   S( -8, -34),   S( 25,  -3),   S( 12, -27),   S( -6, -37),
            S(-20, -53),   S( -1, -57),   S(  5, -36),   S( -9, -71),   S( -1, -44),   S( 25, -16),   S(-17, -51),   S( 12,   2),
            S( -8, -26),   S(  0, -28),   S( -5, -47),   S( -4, -63),   S(-11, -66),   S( 15, -31),   S( -3, -47),   S( -3, -23),
            S(-18, -68),   S(-16, -71),   S(-16, -36),   S( 20, -52),   S(  5, -33),   S( 12, -24),   S( -6, -18),   S( -1,   8),
            S(-14, -20),   S(  8,  10),   S( -6, -13),   S(-19, -43),   S(  8, -21),   S(  4,   7),   S(-21, -27),   S(-13, -18),
            S(  0,  -2),   S(  4,  -6),   S(  4,  11),   S(  2, -11),   S(  0,  -7),   S( -5, -26),   S( -1,  -5),   S( -2,  -4),

            /* knights: bucket 10 */
            S( -4, -29),   S( -8, -35),   S( -9, -39),   S( -8, -25),   S( -6, -81),   S( -1, -43),   S( -9,  -7),   S( -6, -41),
            S( -1, -42),   S( 18,  -5),   S(-23, -71),   S(  2, -63),   S( 10, -63),   S(-21, -88),   S(-13, -14),   S(  3,  21),
            S(  8, -24),   S(-11, -51),   S( 10, -41),   S( 14, -55),   S(-14, -68),   S(-13, -41),   S(-11, -48),   S(-11, -23),
            S( -6, -45),   S(-15, -57),   S( 39, -44),   S( 28, -50),   S( 16, -26),   S(-16, -45),   S( -8, -72),   S( -2, -60),
            S(-16, -61),   S( -1, -58),   S(  4, -24),   S( -4, -54),   S( 12,  -9),   S( -8, -81),   S(-11, -24),   S(  2, -35),
            S(  1,  -7),   S(-17, -44),   S( -7, -35),   S(  3,  -1),   S(-16, -30),   S(-11, -22),   S( -6, -43),   S(-15, -52),
            S(  4,   9),   S( -3, -13),   S(  0, -57),   S(  5, -49),   S(  4,  -3),   S(-13, -50),   S(-16, -37),   S( -9, -22),
            S( -1,   4),   S( -1,   8),   S( -5,   1),   S( -7, -22),   S( 14,  10),   S( -6, -32),   S(  2,  -4),   S(  0,   9),

            /* knights: bucket 11 */
            S(-14, -54),   S(-21, -62),   S( -1, -16),   S( 12, -15),   S(-39, -80),   S( -9, -11),   S( -9, -17),   S(  8,  29),
            S(-16, -41),   S(-28, -50),   S(-21, -76),   S( 30, -45),   S( 15,  -5),   S(  2, -41),   S( -9, -37),   S(-14, -39),
            S( -6, -26),   S(-25, -52),   S( 22, -51),   S( 24, -20),   S( -3,  10),   S( 21, -35),   S(  7, -58),   S(  3, -12),
            S( -9, -22),   S( 14, -19),   S( 27, -30),   S( 46, -22),   S( 59,  -3),   S(  7, -29),   S( 16, -22),   S(-14, -36),
            S( -8,   1),   S( 14, -86),   S(-10, -14),   S( 16, -27),   S( 35,  -2),   S( 12,  -8),   S( -7, -94),   S(-15, -30),
            S(-12, -53),   S( -7, -40),   S(  6, -13),   S( 32, -11),   S( 33,  16),   S(  1,  -8),   S(  3, -34),   S( -1, -14),
            S(-16, -39),   S(-10,  -9),   S(-13, -27),   S(  8,  -7),   S(  6, -26),   S(  7, -51),   S( -1, -40),   S(-11, -22),
            S( -2, -17),   S( -9, -14),   S( -4, -10),   S(  2,  15),   S(  7,  15),   S(  1, -20),   S(  5,  10),   S( -2,  -2),

            /* knights: bucket 12 */
            S(-22, -87),   S( -3, -24),   S( -4, -37),   S(  2,   7),   S( -8, -21),   S( -7, -15),   S(  2,   9),   S( -1,  -2),
            S( -7, -43),   S(  0,   0),   S( -1, -20),   S( -5, -14),   S( 15, -15),   S(  3,  -5),   S(  2,   2),   S(  0, -12),
            S( -1,  -4),   S( -8, -44),   S( -8, -34),   S(  2, -48),   S( -5, -17),   S(  0,  -8),   S( -3, -16),   S( -7, -20),
            S(  3,   8),   S( -7, -61),   S( -6, -39),   S(  7,  -5),   S( 12, -37),   S(  4,  -1),   S(  4,   8),   S(  4,   2),
            S(  3,  13),   S( -4, -32),   S( -4, -39),   S( -4, -21),   S( -2,   0),   S(  6,  13),   S( -2, -11),   S( -5, -22),
            S(  1,  -5),   S(  4,  -7),   S(-11, -46),   S(  6, -23),   S( -7, -23),   S(  2,  -4),   S( 14,  14),   S(  1,   8),
            S( -5, -21),   S( -3,  -5),   S( -6,   0),   S(  3,  30),   S( -4,  -5),   S(-15, -23),   S( -2,  -9),   S( -3,  -7),
            S(  0,  -1),   S(  2,  19),   S( -1,  -5),   S(  1,   1),   S(  2,   0),   S(  3,   1),   S( -1,  -6),   S(  0,   0),

            /* knights: bucket 13 */
            S( -3,  -4),   S( -4, -21),   S(  3, -10),   S( -1, -34),   S( -4, -25),   S( -8, -19),   S( -6, -15),   S(  2,   8),
            S( -5, -23),   S( -4, -13),   S(  3,   4),   S( -5, -10),   S( -8, -55),   S( -7, -47),   S( -5, -30),   S( -1,  -8),
            S(  2,  -5),   S( -1,   3),   S( -2, -27),   S( -1, -16),   S(  1,  -1),   S( -2,  -6),   S(  7,  -4),   S(  0, -13),
            S( -1,  -5),   S(  0,   2),   S( -3, -32),   S(  9,  15),   S( -2, -42),   S(  7,  -5),   S(  7,  12),   S( 12,  13),
            S( -2,  15),   S(-12, -53),   S( -4, -30),   S( -5, -63),   S(-14, -46),   S(  8, -13),   S( -2, -12),   S(-13, -35),
            S( -3,  -3),   S( -1,   1),   S( -4,  -3),   S( 10,   8),   S(-14, -32),   S( -7, -39),   S( -2, -18),   S( -1,  -3),
            S(  1,  -3),   S( -1,  -4),   S(-13, -35),   S( -1, -10),   S(  2,  22),   S( -8, -21),   S(  3,   5),   S( -1,  -7),
            S(  0,   0),   S(  2,  14),   S( -1,  -4),   S( -2,  -3),   S(  0,   3),   S(  5,  14),   S( -2,  -3),   S(  0,   1),

            /* knights: bucket 14 */
            S(  0,  -4),   S(  2,   2),   S( 12,  33),   S( -1,  -5),   S( -1, -24),   S( -2, -13),   S(  4,   3),   S( -3, -20),
            S( -7, -28),   S( -7, -40),   S(  6,  -5),   S(  4, -17),   S( -1, -23),   S(  3,   0),   S( -6, -27),   S(  3,  38),
            S(  0, -19),   S( -9, -57),   S( 13,  19),   S(  8, -18),   S( -8, -32),   S( -1, -18),   S(  2,  -6),   S(  2,  22),
            S( -2, -11),   S( -5, -36),   S( -9, -46),   S(  3,  -5),   S(  3, -13),   S(-10, -55),   S( -4, -20),   S( -5,   2),
            S(  4,  10),   S(-14, -38),   S( -2, -19),   S(-19, -30),   S( -3,  25),   S( -5, -37),   S(  1,  -8),   S(  1, -11),
            S(  1,   5),   S(  6,  18),   S(  5,  72),   S(  6,  15),   S(  0, -17),   S( -1,   2),   S( -2,   3),   S( -4,  -4),
            S(  3,  11),   S( -7, -29),   S(  6,   1),   S(  9,  58),   S(  6,  23),   S( -7, -23),   S(  2,   4),   S(  2,   1),
            S(  2,   1),   S(  0,  -3),   S(  0,  -1),   S(  4,  16),   S( -3, -12),   S( -2,  -4),   S( -1,  -1),   S(  1,   4),

            /* knights: bucket 15 */
            S( -6, -29),   S( -2,  -8),   S(  5,  14),   S(  3,  -3),   S( -5, -35),   S( -7, -28),   S( -2, -31),   S( -4, -24),
            S( -2,  -8),   S( -2, -19),   S( -6, -24),   S(  7,  16),   S( 10, -12),   S( -4, -36),   S( -6, -25),   S(  3,  -5),
            S( -3, -13),   S(-10, -31),   S(  0,  -7),   S( 11,  -9),   S(-22, -90),   S(  0, -21),   S( -2, -28),   S( -1,  -4),
            S( -1, -15),   S( -4,  -8),   S( -3, -25),   S(  0,   0),   S(  5, -25),   S( -5, -18),   S( -4, -25),   S(  0,   1),
            S(  2,   9),   S(  7,  18),   S(  8,   2),   S( -4,  22),   S( 12,   3),   S( -1,   3),   S(  2, -29),   S(  4,  20),
            S(  4,  19),   S(  0,  -3),   S( -6,  -7),   S( -6, -16),   S( -8, -26),   S( -1,  12),   S(  2,  11),   S(  3,  21),
            S( -5, -13),   S( -1,   0),   S(  3,  10),   S(  3,   7),   S(  1,  10),   S(  3,  15),   S(  3,  16),   S( -2,  -2),
            S(  3,  10),   S( -1, -12),   S( -1,   0),   S( -2,  -5),   S(  1,   3),   S( -1,   3),   S(  2,  13),   S(  1,   5),

            /* bishops: bucket 0 */
            S( 43, -46),   S(-24,  50),   S( -4, -12),   S(-15, -29),   S( 21, -27),   S( 14, -16),   S( 79, -68),   S( 36, -14),
            S( -2, -51),   S(  9, -28),   S( -7,  15),   S( 12,  10),   S( 23,  -3),   S( 61, -24),   S( 48,  16),   S( 60, -24),
            S( 21,  18),   S( 13,  16),   S( 23,   7),   S( 26,   5),   S( 51, -12),   S( 46,  24),   S( 64, -31),   S( 33, -39),
            S( 28, -50),   S( 30, -40),   S( 20,   4),   S( 78, -22),   S( 83,  15),   S( 61,  19),   S( 19,  -5),   S( 22,  17),
            S( 21,  16),   S( 43,  -6),   S( 82, -18),   S( 99,  22),   S(148, -35),   S( 28,  35),   S( 59,  -9),   S( 17, -14),
            S( 23,  54),   S( 95,  11),   S( 81,  -1),   S( 75,  13),   S( 25,   6),   S( 63,  26),   S( 79,   3),   S(-17,  19),
            S(-30, -102),  S( 98,  25),   S( 61,  55),   S(-13,   5),   S( 26, -15),   S( 58,   5),   S(-12,  35),   S(-30,  60),
            S(-12, -43),   S(-10,  -6),   S( -9, -29),   S(-23,  -6),   S(-18, -21),   S(-15,   1),   S( -3,  39),   S(-25, -23),

            /* bishops: bucket 1 */
            S(-23, -10),   S( 13, -36),   S(-27,  28),   S( 30, -18),   S(-24,  10),   S( 11,   2),   S(  5,   6),   S( 40, -42),
            S(  3, -36),   S(-11, -22),   S( -4,  -7),   S(-18,   6),   S( 36, -24),   S(  6, -11),   S( 70, -40),   S(  3, -27),
            S(-19,  -8),   S( 30, -14),   S( -4,  -7),   S( 24,  -7),   S( 10, -13),   S( 46,  -9),   S(  2,  -1),   S( 81, -21),
            S( 17, -35),   S( 38, -17),   S( 36, -10),   S( 17,   0),   S( 61,  -8),   S( 11,   0),   S( 80, -38),   S( -2,  -5),
            S( 27, -43),   S( 57, -24),   S( -2,   9),   S(100, -10),   S( 60,  -4),   S(109, -47),   S( 10,  -9),   S( 40,  -9),
            S( 64, -39),   S( 35, -12),   S( 73, -10),   S( 86, -48),   S(132, -46),   S( -1,   4),   S( 14,  17),   S( -7, -29),
            S(-30, -68),   S(  6, -20),   S(  5, -44),   S( 12,  23),   S( 31,  -4),   S( -3,  20),   S( 27, -15),   S(-41,  32),
            S( -8, -31),   S(-45,   6),   S( -5, -43),   S(-48,   8),   S( 18, -16),   S( 33, -30),   S( 43, -25),   S(-32, -32),

            /* bishops: bucket 2 */
            S( 16, -32),   S( -2, -16),   S(  3,   4),   S(-13,   7),   S( 16,   9),   S(-17,  13),   S( 40, -22),   S(  7,  -3),
            S(  5, -16),   S( 13,  -8),   S( -7,  -6),   S( 11,   4),   S( -7,  10),   S(  8,   7),   S(  4,  -9),   S( 29, -55),
            S( 37,  -2),   S( 17,  -6),   S( -1,  14),   S( -7,   2),   S(  7,  20),   S(-11,   1),   S( -5, -15),   S( -8,   0),
            S(  5, -13),   S( 39,  -7),   S(  5,   7),   S( 40,  22),   S(  6,  17),   S(-12,  28),   S(-32,  15),   S(  8,  -8),
            S( 26,  -2),   S( -1,  16),   S( 67,   1),   S( 29,  16),   S( 25,  29),   S( 11,  14),   S(  0,  26),   S( 20, -16),
            S(-11,   9),   S( -7,  35),   S( 11,  -6),   S( 96, -28),   S( 68, -13),   S(121,  -1),   S( 58,  14),   S(  4, -34),
            S(  3,  19),   S( 12,   1),   S( 11,  15),   S(  2,   6),   S(-54, -27),   S(-33,  -9),   S(-42,  35),   S(-16, -33),
            S(-68, -14),   S(-32,  20),   S(  8,  -1),   S(-40,  26),   S(-30, -42),   S(-51, -16),   S(-39, -22),   S(-36, -34),

            /* bishops: bucket 3 */
            S( 38,  11),   S( 62, -36),   S( 11,   0),   S( 20,   2),   S( 12,  30),   S(  1,  46),   S(  0,  63),   S( -9,   8),
            S( 34,   8),   S( 20,  10),   S( 21,  20),   S( 16,  21),   S( 26,  21),   S( 16,  35),   S( 12,  21),   S( 34,  -5),
            S( -1,  12),   S( 38,  40),   S( 15,  55),   S( 23,  38),   S( 20,  46),   S( 20,  31),   S( 13,  20),   S( 19,  25),
            S(-12,  20),   S( 16,  34),   S( 32,  55),   S( 43,  49),   S( 40,  38),   S( 19,  30),   S( 38, -10),   S( 33, -28),
            S( 31,   9),   S( 13,  48),   S( 17,  61),   S( 58,  60),   S( 44,  65),   S( 45,  33),   S( 10,  37),   S(-16,  36),
            S( 21,  25),   S( 23,  66),   S( 27,  43),   S( 15,  47),   S( 69,  35),   S( 36,  89),   S( 64,  59),   S( 17,  88),
            S(-18,  58),   S( 42,  39),   S( 25,  29),   S( 21,  31),   S( 22,  46),   S( 23,  74),   S(-35,  22),   S( 25, -34),
            S(-55,  53),   S(-15,  50),   S(-70,  47),   S(-11,  42),   S(  2,  46),   S(-70,  62),   S( 23,  18),   S(-11,  13),

            /* bishops: bucket 4 */
            S(-28,   4),   S(-56,  -6),   S(-77,  35),   S(-43,   5),   S(-22, -33),   S(-21, -20),   S(-23, -21),   S(-18, -82),
            S(-36,   8),   S(  9, -36),   S( 45, -25),   S(-20, -12),   S(-29,   1),   S(-11, -31),   S( -8, -19),   S(-41, -45),
            S( 14,  20),   S(-22, -16),   S( 71, -29),   S( 22, -41),   S( -7, -10),   S(-19,   6),   S(-49, -23),   S(-61, -16),
            S( 11, -41),   S( 78, -22),   S( 28, -25),   S( 38, -15),   S( -2,  11),   S( 54, -20),   S(-50,  -3),   S( -6, -26),
            S( -4, -32),   S( -8, -70),   S( 55, -25),   S( 51, -23),   S( 26,  -4),   S( 34,   9),   S( -3,  42),   S(-20,   5),
            S(-82, -93),   S(-14, -67),   S( 21, -40),   S(  8, -19),   S(-13,   9),   S( 18,  17),   S(-36,   2),   S(-18,   5),
            S( -2, -10),   S(-21, -44),   S( -2, -57),   S(  0, -43),   S(-25,   5),   S( 18,  -3),   S(-14,  13),   S( 17,  42),
            S(  8,  21),   S( -7, -33),   S(  8, -12),   S(-20, -56),   S(-18, -35),   S(  5,   1),   S( -3,  66),   S( 11,  25),

            /* bishops: bucket 5 */
            S(-58,  15),   S( -5,  -3),   S(-60,   6),   S(-78,  23),   S(-28,   6),   S(-29, -21),   S(-15,   5),   S(-43, -12),
            S(-28,  -7),   S(-56,   8),   S( 24,  -6),   S(  1,  -7),   S(-88,  26),   S( 14,  -3),   S(-14, -16),   S(  2, -24),
            S( 21,   6),   S(-58,   0),   S( 25,  -7),   S(  6,  -8),   S(  4,   2),   S(-43,   3),   S(  1,  -1),   S(-27,   6),
            S(  6, -18),   S(-11,   8),   S( 36, -15),   S( 67, -11),   S(-28,  11),   S( 39, -13),   S(-75,  12),   S(-21,   1),
            S( 15, -30),   S(  4, -18),   S(  8, -26),   S(-13, -28),   S( 17, -10),   S(  2,  -6),   S( 52,  -7),   S(-47, -15),
            S(  4, -10),   S( -1,  -2),   S( 36, -41),   S(  3, -20),   S(-56,  -2),   S( 30, -10),   S( -8,  10),   S(  6,  15),
            S(-35, -15),   S( -5, -27),   S(  0,  -3),   S( -2,  -5),   S(  9, -11),   S( 45, -16),   S( 34,  16),   S( -8,  -2),
            S(-22, -35),   S(-28, -33),   S(  1, -27),   S( -1, -21),   S(-39,   9),   S( -5,  13),   S(-51,  -3),   S( -9,  47),

            /* bishops: bucket 6 */
            S(-56,   2),   S(-26,   4),   S(-46,  12),   S(-42,  16),   S(-25,  12),   S(-11,   6),   S(-45,  12),   S(-24,   2),
            S( -5,   3),   S(-57,  -4),   S(-38,  15),   S(-20,  11),   S(-56,  21),   S(-32,  11),   S(-55,  21),   S(-27,  -3),
            S( 23, -12),   S(-59,  18),   S(  0,  -9),   S(-39,  23),   S(-11,  14),   S(-42,  12),   S(-42,  -2),   S(-24,  15),
            S(  7,  -9),   S(-28,  -2),   S( 16,   5),   S( 78,  -1),   S( 68,  -5),   S( 31,   5),   S( -1, -19),   S( -4,  15),
            S(-99,  40),   S(  2,  -1),   S( 35,  -4),   S( 80, -13),   S(  9,  -6),   S( -2, -33),   S( 34,  -9),   S(-23, -21),
            S(-20,  23),   S( 32,  -6),   S(  2,   7),   S( 36, -15),   S(-17, -15),   S(-25, -22),   S(-11,  10),   S( 10, -35),
            S(-21,  23),   S(-54,  46),   S(  8, -10),   S( 10,  -8),   S(-22,   8),   S(-23,   0),   S(-14, -10),   S(-37, -10),
            S(-41,  13),   S(-18,  -2),   S(-24,  13),   S( 10,   8),   S(-12,  11),   S( 40, -35),   S(-23,  -5),   S( 10, -11),

            /* bishops: bucket 7 */
            S( 21, -30),   S(-57, -12),   S( -3, -44),   S(-32,  -1),   S(-26, -20),   S(-45,   5),   S(-74,  10),   S(-59, -23),
            S(  7, -58),   S( 16, -49),   S( 27, -33),   S( -8, -29),   S(-29,   2),   S(-32,  -5),   S(-32, -31),   S( -7, -10),
            S(-22, -35),   S( -5, -14),   S(  5, -10),   S( 50, -24),   S( 16, -19),   S( 18, -23),   S(-50,   3),   S(-50,  16),
            S(  4, -29),   S(-43,   1),   S( -9,   1),   S( 46, -17),   S(131, -21),   S( -2,  -7),   S( 60, -23),   S(-14, -28),
            S(-39,  -8),   S( 18,  -9),   S( 14, -18),   S( 27,   3),   S( 94, -27),   S( 69, -13),   S(-14, -15),   S(-51, -18),
            S(-68,  10),   S( 11,  22),   S( 15,  -3),   S(-13,  -9),   S(-24,   0),   S( 74, -38),   S( 28, -14),   S(-69, -91),
            S(-40, -16),   S(-26,  24),   S(-42,  14),   S( 35,  -8),   S( -2, -29),   S( 14, -31),   S( 16,  -9),   S(-11,  -2),
            S(-24, -33),   S(-34,  -7),   S(-29,   6),   S(  6, -14),   S(-17, -16),   S( -2, -25),   S( 30, -29),   S(  2, -28),

            /* bishops: bucket 8 */
            S( 10,  68),   S(-15,  -3),   S(  8, -21),   S( -5,  25),   S( -3,  10),   S(-23, -21),   S(-18, -51),   S( -8, -39),
            S( -5, -14),   S(  7,  30),   S(  6,  33),   S( 23,  -3),   S(-11, -27),   S(  9,   4),   S(-19, -20),   S(  0, -17),
            S( -6, -42),   S( -9, -38),   S( 18,  41),   S( 24,  -9),   S( 28,  24),   S( 16,   9),   S(-19, -22),   S(-29, -71),
            S(  0,   4),   S( 15,  56),   S(  4,  14),   S( 10,  30),   S( 10,   6),   S( 27,  37),   S( -9, -20),   S(  0, -39),
            S(  2,  31),   S( 21,  76),   S( 14,  33),   S( -2,   1),   S( -9,  44),   S(-24,  29),   S( 18, -24),   S( 13,  33),
            S(-20, -35),   S( -9,   6),   S(  0,  40),   S( 11,  29),   S( 20,  23),   S(  1,  39),   S( -2,  14),   S(  9,  29),
            S(-12,  -2),   S(-17, -63),   S( 13,  54),   S(  5,  46),   S( -2,  12),   S( 20,  64),   S(  9,  74),   S(-24, -10),
            S(  2,  20),   S(  7,  21),   S( -2,  -1),   S( -4,  10),   S(  6,  41),   S(  6,  24),   S(-11,  24),   S( 24, 107),

            /* bishops: bucket 9 */
            S(  5,  46),   S(-19,  12),   S(-27,  -6),   S(-35, -10),   S(-25, -33),   S(-24, -57),   S(-21, -57),   S( -7, -17),
            S(-13,  -4),   S(  1,   7),   S( 11,   7),   S(-12, -13),   S( -6, -23),   S( -8, -33),   S(-12, -12),   S(-24, -78),
            S( -7, -39),   S( -1, -18),   S(  0, -25),   S(  2,  41),   S( 16,   5),   S(  5, -21),   S(  7,  -7),   S(-16, -24),
            S(-11,  25),   S(-13,   6),   S(-10,  21),   S(  5,  30),   S(-15,  38),   S(  0, -10),   S( -5,  20),   S( -6,   0),
            S( 11,  46),   S(-11,  37),   S(  4,  28),   S(-28,   5),   S(  0,  18),   S( -6,  15),   S(  5,  10),   S(-24, -37),
            S(-10,   3),   S(-20,  -5),   S(-26,  17),   S( -8,  24),   S(-26,  21),   S(-25,  35),   S( -4,  17),   S(  4,  43),
            S(  3,  48),   S(-25,  17),   S( -5,  -5),   S(-24,   4),   S(  3,  28),   S( -8,  25),   S( -1,  37),   S(-12,  44),
            S(  4,  53),   S( -4,  -7),   S(-12, -12),   S( -4,   0),   S(  0,  31),   S(-14,  16),   S(-10,  42),   S( 22,  89),

            /* bishops: bucket 10 */
            S(-16,  10),   S(-12,  15),   S(-20, -42),   S(-24, -43),   S(-55, -17),   S(-28, -58),   S(-31,  13),   S( -9,   6),
            S(-17, -16),   S(-17, -24),   S(-17, -44),   S(-17, -31),   S(-47,  -9),   S(-42, -34),   S(-16, -17),   S( -8,  27),
            S(-10, -38),   S(-24, -31),   S(  1,  -9),   S( -3,  15),   S( -5, -30),   S(  2,  -1),   S(-20, -12),   S( -3, -62),
            S(-12, -22),   S(-28, -41),   S(-25, -16),   S(  6,  23),   S(-10,  21),   S(  5,  -3),   S(  1,  17),   S(-30, -11),
            S( -6,  -7),   S(-58,  19),   S(  5,  -9),   S(  7,  12),   S( 14,  12),   S( -5,  38),   S(-28,  -5),   S( -3,   8),
            S(-18,  15),   S( -4,  14),   S(-19,  12),   S(-27,  19),   S(  1,  -6),   S(  1,   6),   S( -4,  22),   S(  2,  41),
            S(  0,  26),   S( -9,  24),   S(-18,  15),   S(-12,  17),   S(-16,   6),   S(-29, -36),   S(-16,  -8),   S(  2,  21),
            S( -7,  55),   S( -6,  38),   S( -1,  33),   S(-17,  12),   S(-12,  20),   S(-10,  14),   S(  2,  -5),   S( -5,   1),

            /* bishops: bucket 11 */
            S( -4, -16),   S( -1,  -2),   S(  1,  14),   S(-15,  -7),   S(-40, -45),   S( -1, -41),   S(-27, -19),   S(-28,  31),
            S( -5,  -6),   S( 13,  -9),   S( -1,   3),   S( -2, -11),   S(-30, -21),   S(-27,  -5),   S(-44,   5),   S(  6,  22),
            S( -1, -23),   S(-13, -20),   S( 19, -11),   S( -5,  -8),   S(  1, -12),   S( 26,  20),   S( 20,  -9),   S( -3,  -6),
            S( -5,   4),   S(-12, -34),   S( 33,   6),   S( -2, -34),   S(-30,   3),   S( 35,  50),   S(  3,   1),   S(-14, -48),
            S( -4,  27),   S(-10,  13),   S(-24,  22),   S(-39,  29),   S(-13,   5),   S(  6,  16),   S( -1,  41),   S(  3,  28),
            S(-30, -10),   S(  2,  29),   S(-35,  29),   S(-19,  17),   S(-16,  49),   S(-14,  26),   S( -3,   7),   S( -3,  -6),
            S(-21,  -3),   S( -5,  87),   S(  0,  40),   S(  5,  57),   S( 12,  18),   S( -4,  28),   S(-14, -34),   S(-14, -11),
            S(  2,  85),   S(-13,  33),   S( 11,  56),   S(  2,  25),   S( 11,  25),   S(  1,  34),   S(-20, -31),   S(  7,  24),

            /* bishops: bucket 12 */
            S( -4, -12),   S( -4, -27),   S( -7, -15),   S( -5, -17),   S(-10, -12),   S( -7,   0),   S( -6, -10),   S( -2, -17),
            S(  1, -16),   S( 12,  34),   S(  0, -17),   S( -7, -19),   S(  5,  13),   S( -4, -16),   S(-17, -28),   S(  0,  -5),
            S(  5,  29),   S( 11,  44),   S( 15,  39),   S(  8,  11),   S(  8,  30),   S(-17, -60),   S(  6,   9),   S( -4,   2),
            S(  3,  65),   S( 11,  54),   S(  1,  36),   S( 23,  58),   S( 14,  -2),   S(  4, -10),   S(  8,  11),   S(  9,  23),
            S(  9,   9),   S( 12,  46),   S(  6,  34),   S(  9,  53),   S(  8,  16),   S( 11,  38),   S( 10,   9),   S(  5,  12),
            S( -1,  -2),   S(-12, -24),   S(  0,  21),   S( -5,   1),   S(  7,  60),   S( 11,  35),   S(-10, -37),   S(  2,  13),
            S( -1,   3),   S(  7,  32),   S(  1,   3),   S(  3,   4),   S(  3,  34),   S( 13,  34),   S( 13,  49),   S(  1,  19),
            S(  0,   6),   S(  0,   9),   S(  1,  10),   S( -2,  -7),   S(  1,   5),   S( -2,   0),   S( 10,  50),   S(  7,  41),

            /* bishops: bucket 13 */
            S(-11, -14),   S( -6, -27),   S(-12, -33),   S(-12, -17),   S(  8,  39),   S(-20, -28),   S(-12, -22),   S( -8, -44),
            S(  0,  32),   S( -8, -17),   S( -5,   0),   S( 12,  68),   S(-13, -34),   S( 11,  10),   S( -3, -18),   S(  0,  -4),
            S( -3,   3),   S( 16,  89),   S(  2,  29),   S( 22,  32),   S(  1,  17),   S(  5,   6),   S( -6,   3),   S(-13, -27),
            S( 12,  59),   S( 13,  86),   S(  2,  38),   S(-11, -20),   S( 19,  62),   S(  0, -23),   S( 12,  46),   S( -1,   4),
            S(  0,  49),   S( -3,  24),   S(  5,   6),   S(  2,  42),   S(  5,   0),   S(  1,  44),   S(  7,  19),   S( -7,  -4),
            S(  2,  31),   S(  0,  14),   S( -4,  22),   S(  8,  28),   S(-10,  33),   S( -4, -20),   S( -4,  -5),   S(  6,  23),
            S(  8,  30),   S(-12, -28),   S( -7,  -9),   S(  1,  21),   S( -2,  10),   S(  3,  65),   S( -2,  14),   S(  5,  54),
            S(  0, -18),   S( -3,  -2),   S(  1,   5),   S(  4,  16),   S( -4,  13),   S( -7, -11),   S(  4,  44),   S(  1,  -6),

            /* bishops: bucket 14 */
            S(-16, -28),   S( -3,  -9),   S( 17,  13),   S(  1,  26),   S(-13,  -5),   S( -4,  -8),   S(-16, -57),   S( -9, -38),
            S( -1,  -6),   S( -2, -21),   S( -1,   2),   S(  3,   6),   S(  9,  37),   S( -6,  -6),   S( 11,  32),   S(  1,  20),
            S( -4, -10),   S( -1,  11),   S(  4,  15),   S(  5,  25),   S(  8,  38),   S(  8,  39),   S(  4,  51),   S( 10,  51),
            S( -3,   3),   S(  9,  33),   S(-20, -12),   S( -8,  41),   S(  3,  27),   S(  7,  40),   S( 13,  82),   S(  1,  38),
            S(  3,  17),   S(  2,   7),   S(  2,  17),   S( -8,  28),   S( -2, -17),   S(  2,  42),   S( 12,  36),   S(  1,  43),
            S( -2,  14),   S( 14,  33),   S(  3,  45),   S( -2,  30),   S(  6,  39),   S( -5,  15),   S( -7, -36),   S( 10,  30),
            S( 10,  54),   S(  7,  22),   S(  9,  49),   S(  3,  -8),   S( -6, -16),   S( -1,   6),   S(  1, -14),   S(  4,  27),
            S(  8,  47),   S(  4,  30),   S(  6,  27),   S(  4,  22),   S( -6, -24),   S( -4, -14),   S(  9,  27),   S( -1,  -5),

            /* bishops: bucket 15 */
            S( -2,  -7),   S( -7, -26),   S(-10, -33),   S( -2,  -8),   S(-12, -20),   S(  3,   2),   S( -4, -35),   S(  0,  -2),
            S(  6,  30),   S(  5,  14),   S( -4,  -9),   S(  9,  15),   S(  8,   2),   S(  1,  -6),   S( -3,  -4),   S( -6,  -9),
            S( -5, -14),   S(  7,  -7),   S(  4,  22),   S(  6,   5),   S( 10,  14),   S(  7,  12),   S( -4,   0),   S(  0,  31),
            S(  2,   3),   S(  7,  22),   S( 14,  46),   S(-15, -35),   S( -3,  13),   S( 18,  52),   S( 20,  75),   S( -5,  18),
            S( -8,  -6),   S(  4, -26),   S(  0,  31),   S( 25,  85),   S( 11,  64),   S(  5,  18),   S(  4,  28),   S( -7,   7),
            S(  2,  19),   S( -1,  17),   S( 10,  66),   S( -3,  13),   S( 17,  41),   S(  2,  46),   S( -3,   5),   S( -1,  -4),
            S(  5,  29),   S(  6,  22),   S( -8,  26),   S( 10,  19),   S(  9,  42),   S( -9, -19),   S(  1,  21),   S(  0,   4),
            S(  1,  10),   S(  6,  52),   S(  2,  26),   S(  6,   0),   S(  0,  -9),   S( -1,   5),   S(  1,  10),   S(  2,  15),

            /* rooks: bucket 0 */
            S(-10,  25),   S( 13,  11),   S(  1,  15),   S(  1,  29),   S(-19,  66),   S(-10,  48),   S(-42,  67),   S(-54,  63),
            S( -5,  -3),   S(  7,  31),   S(-36,  42),   S(-11,  48),   S( -1,  66),   S(-15,  51),   S(-12,  19),   S(-46,  71),
            S(  6, -10),   S(  5,  16),   S(-13,  41),   S( 18,  -3),   S(-14,  55),   S(-24,  47),   S(-25,  60),   S( 19,   3),
            S( -4,   4),   S( 44,   2),   S(-60,  69),   S( 19,  29),   S( -5,  77),   S(-12,  57),   S(-52,  86),   S(-23,  52),
            S( 49, -39),   S( 13,  22),   S(-13,  54),   S( 18,  52),   S( 19,  51),   S( 33,  76),   S( 41,  72),   S( 30,  50),
            S( 43,  -2),   S( 60,  37),   S( 62,  39),   S( 79,  40),   S(-24, 103),   S( 51,  57),   S( -1,  84),   S(-23,  82),
            S( 47,  37),   S( 96,  80),   S(120,  57),   S( 84,  28),   S( 54,  71),   S( -4,  89),   S(-42,  88),   S(-27,  86),
            S( -1, -10),   S( 34,  34),   S( 29,  49),   S( 79,  -6),   S( 65,  69),   S( 86,  41),   S( 40,  55),   S( 51,  -6),

            /* rooks: bucket 1 */
            S(-50,  54),   S(-16,  22),   S( -4,  25),   S(-38,  40),   S(-37,  53),   S(-37,  55),   S(-44,  70),   S(-79,  83),
            S(-49,  53),   S(-29,   8),   S(-35,  41),   S(-25,  33),   S(-19,  17),   S(-50,  44),   S(-15,  12),   S(-32,  38),
            S(-56,  39),   S(-40,  26),   S(-17,  11),   S(-20,  24),   S(-40,  24),   S(-45,  25),   S(-68,  65),   S(-39,  69),
            S(-65,  75),   S(-43,  37),   S(-34,  54),   S(-18,  32),   S(-61,  62),   S(-78,  92),   S(-29,  71),   S(-74,  93),
            S(-48,  68),   S(-15,  15),   S( 25,  34),   S( 16,  35),   S( -8,  33),   S(-45,  99),   S( -4,  70),   S(-26, 103),
            S( 47,  38),   S( 64,   5),   S( 53,   3),   S( 39,  44),   S(-30,  57),   S( 23,  59),   S( 38,  53),   S( 15,  74),
            S(  6,  85),   S( 65,  18),   S( 16,  39),   S( 37,  37),   S( 55,  41),   S( 10,  66),   S( 51,  78),   S( 25, 101),
            S( 65,  -1),   S( 20,  19),   S( 17,  -7),   S( -9,   2),   S( 29,  12),   S( 24,  24),   S( 57,  41),   S( 66,  49),

            /* rooks: bucket 2 */
            S(-61,  86),   S(-49,  83),   S(-45,  75),   S(-41,  47),   S(-27,  48),   S(-40,  45),   S(-34,  32),   S(-73,  78),
            S(-58,  77),   S(-61,  72),   S(-45,  73),   S(-59,  62),   S(-52,  59),   S(-57,  46),   S(-22,  18),   S(-47,  40),
            S(-38,  71),   S(-32,  70),   S(-65,  66),   S(-53,  69),   S(-47,  54),   S(-35,  51),   S(-28,  30),   S(-12,  44),
            S(-33,  94),   S(-43,  82),   S(-64,  95),   S(-84,  84),   S(-57,  74),   S(-50,  60),   S(-38,  57),   S(-26,  64),
            S(-44, 113),   S(-56, 108),   S(-44, 104),   S(-52,  82),   S(-45,  79),   S(  3,  66),   S(-24,  69),   S(-29,  90),
            S(  5,  99),   S( 24,  86),   S( 17,  80),   S(-36,  84),   S( 84,  25),   S( 54,  63),   S( 76,  41),   S( 70,  68),
            S( 67,  93),   S( 18, 105),   S( 47,  75),   S( 70,  49),   S( 12,  43),   S( 84,  90),   S(-39, 120),   S( 21, 107),
            S( 44,  69),   S( 33,  74),   S( 61,  51),   S(-10,  52),   S(-31,  51),   S( 43,  67),   S( 18,  51),   S( 34,  60),

            /* rooks: bucket 3 */
            S(-13, 110),   S( -6, 112),   S(-10, 135),   S( -7, 123),   S(  3,  89),   S( 10,  82),   S( 28,  51),   S( -5,  43),
            S( -4, 102),   S(-13, 115),   S(-15, 137),   S( -9, 132),   S( -1,  90),   S( 19,  50),   S( 57,  23),   S( 19,  49),
            S( 15,  91),   S(-18, 125),   S(-18, 124),   S(-12, 134),   S(  6,  89),   S( 11,  76),   S( 36,  65),   S( 18,  62),
            S(  5, 130),   S( -9, 140),   S(-20, 149),   S(-14, 145),   S(-22, 114),   S( -8, 102),   S( 29,  85),   S( -3,  84),
            S( -5, 142),   S(-29, 158),   S(  5, 156),   S(  4, 152),   S( -5, 124),   S( 19, 104),   S( 40,  98),   S( 35,  91),
            S( -1, 158),   S(  3, 153),   S( 21, 157),   S( 31, 141),   S( 86,  90),   S(114,  83),   S( 79, 116),   S( 35,  97),
            S(  5, 166),   S( -4, 170),   S( 15, 169),   S(  6, 172),   S( 24, 151),   S(106,  97),   S(133, 180),   S(193, 125),
            S(108,  42),   S( 54, 104),   S( 57, 131),   S( 31, 127),   S( 55, 111),   S( 69, 100),   S( 79,  81),   S(127,  58),

            /* rooks: bucket 4 */
            S( -4, -39),   S(  5,  -9),   S(  0, -27),   S( -5, -27),   S(-44,  32),   S(-10,  22),   S(-45,  10),   S(-83,  42),
            S(-22, -43),   S(-77, -15),   S( -5, -28),   S(  1, -35),   S(-17,  29),   S(  2,   8),   S(-44,  30),   S(-12,  32),
            S(-13, -12),   S(-13,  -2),   S(-30, -20),   S(-53, -27),   S(-55,   6),   S(-52,  16),   S( -8, -31),   S(-65,  14),
            S(-55, -26),   S( 25,  -6),   S(  6, -27),   S(  3, -14),   S( 39,   0),   S(  8,  -3),   S(  1,  -9),   S(-40,  14),
            S(-10, -33),   S( 19, -44),   S( 51,  -6),   S( 72, -23),   S( 65, -12),   S( 37,  28),   S( -1,  11),   S( 36,  15),
            S( -6,  -9),   S(  2,  -2),   S( 28,  -2),   S( 32,  28),   S( 38,  20),   S(  6,  21),   S( 20,  29),   S(  5,  17),
            S(-18, -45),   S( 36, -11),   S( 54, -14),   S( 63, -18),   S( 55,   3),   S( -3,  15),   S(  8, -23),   S( 38,  -1),
            S(-13,  -3),   S( 20,   6),   S( 47, -24),   S(  1, -16),   S( 43,   5),   S(  5,   5),   S( 10,   3),   S( -2,  15),

            /* rooks: bucket 5 */
            S(-56,  36),   S(-24,   2),   S(-12,   8),   S( 31, -13),   S( -6,  16),   S( -4,  20),   S(-39,  54),   S(-53,  34),
            S(-25,  10),   S(-23, -30),   S( 26, -42),   S( 17,  -9),   S(-16,  -6),   S(-37,   3),   S(-56,  26),   S(-20,  12),
            S(-53,  23),   S( -2, -25),   S(  2, -34),   S(-55,  13),   S(-43,  -1),   S(  8, -11),   S(-33,  15),   S(-13,  15),
            S(-75,  32),   S( -7,  -4),   S(  9, -16),   S( 34,   2),   S( -6,   2),   S( -8,  22),   S(  2,   9),   S(-14,  50),
            S( 39,   3),   S( 10,   0),   S(  0,  15),   S(  0,  -1),   S( 25,  -3),   S(113, -11),   S(  1,  18),   S( 42,  26),
            S( 11,  16),   S( 26,   3),   S( 14,  16),   S( -9, -17),   S( 21,  16),   S( 38,  16),   S( 80,  15),   S(  7,  52),
            S( 53, -10),   S( 56, -24),   S( -4,  -1),   S( 54,  20),   S( 69, -18),   S( 64, -30),   S(111, -27),   S( 53,  -9),
            S(  4,  21),   S( -9,   6),   S( 45, -11),   S( 17,  23),   S( 75,   4),   S(  8,  20),   S( 25,  34),   S( 28,  26),

            /* rooks: bucket 6 */
            S(-68,  45),   S(-43,  34),   S(-22,  18),   S( -5,   3),   S( -7,   8),   S(  7,  -4),   S(-13,  -1),   S(-19,  -4),
            S(-81,  44),   S( -2,   7),   S(-64,  37),   S(-15,   5),   S(-10,  -5),   S(-16,  -4),   S(-23, -10),   S(-52,  17),
            S(-82,  44),   S(-13,  17),   S( -8,   2),   S(-38,  11),   S(-24,  16),   S(-29,  12),   S(-19, -12),   S(-26,  -2),
            S(-49,  39),   S(-60,  49),   S(  3,   5),   S(  8,   3),   S( 16,   8),   S(  1,  11),   S(-36,   6),   S( -7,  24),
            S(-11,  47),   S( 62,  10),   S( 72,  14),   S( 37,   8),   S( 25,   3),   S( 27,  23),   S( 46,  -3),   S( 66,   8),
            S(118,  -3),   S( 91,   4),   S( 99,   1),   S( 41,  -6),   S(  6, -32),   S( 20,  30),   S( 36,  -3),   S( 78,   8),
            S( 51,   6),   S(109, -18),   S(122, -32),   S( 78, -21),   S( 31,  -4),   S( 42, -11),   S( 67, -29),   S( 97, -32),
            S( 43,   9),   S(-14,  44),   S(-30,  33),   S( 88, -18),   S( 50,  10),   S( 47,  25),   S( 43,  14),   S( 48,  19),

            /* rooks: bucket 7 */
            S(-100,  37),  S(-69,  33),   S(-69,  43),   S(-52,  37),   S(-37,   4),   S(-31,  -5),   S(-41,  14),   S(-72,   7),
            S(-78,  35),   S(-42,  19),   S(-47,  25),   S(-76,  42),   S(-62,  10),   S(-22,  -4),   S(  3,  15),   S( 23, -57),
            S(-70,  31),   S(-82,  30),   S(-40,  15),   S(-57,  35),   S(-88,  25),   S(-47,  18),   S(-14,   4),   S(-11, -27),
            S(-85,  41),   S( -2,   5),   S(  2,   8),   S( 48,  -8),   S(  7,  -4),   S( 31,  -3),   S( 43,   0),   S( 52, -30),
            S(  1,  30),   S( 16,  27),   S( 28,  17),   S( 62,   9),   S( 95,  -7),   S( 91, -26),   S( 95, -32),   S(-17, -29),
            S(-14,  40),   S( 48,   3),   S( 82,  16),   S( 97,  -8),   S(113, -21),   S( 45,   6),   S( 26,  20),   S(-12, -19),
            S(-18,  17),   S( 34,  -4),   S( 44,   3),   S( 53,  -7),   S(111, -30),   S( 96, -21),   S( 42,  11),   S( 30, -33),
            S(-49,  17),   S( 13,  18),   S(  4,  24),   S( 23,  13),   S( 22,  16),   S( 53,   3),   S( 82,   6),   S( 40,  -7),

            /* rooks: bucket 8 */
            S( -9, -73),   S( 30, -51),   S( 32, -66),   S( 35, -15),   S(  4, -62),   S( -8, -34),   S(-16, -75),   S(  6, -42),
            S(-24, -99),   S( -3, -53),   S(  6,   8),   S(-29, -80),   S( -7, -61),   S( -4, -41),   S(-17, -22),   S(-33, -42),
            S(  4,  15),   S(  2, -17),   S(  8,   7),   S( -1,  11),   S(  5,  14),   S( 19,  13),   S(  5,  35),   S( -3, -33),
            S(-10, -42),   S( -2, -15),   S( -6, -46),   S( 15,  11),   S( 20,  14),   S( 29,   9),   S(  1,  33),   S( -2, -13),
            S(  2, -41),   S(  9,  25),   S( 19, -13),   S( 22,   2),   S(  4,  19),   S( -9, -24),   S(  6,  19),   S( -6, -13),
            S(-13, -35),   S(  7, -29),   S( -7, -35),   S( -3, -29),   S(  5,  25),   S(-14,  11),   S(  1,  -2),   S(  3,  25),
            S( 16,  22),   S( 20, -44),   S(  7, -13),   S( 24,   4),   S(  8,  15),   S( 18,  31),   S(  3,  17),   S(  2,  10),
            S( -7,   2),   S( 36, -22),   S(  7, -14),   S( 39,  54),   S( -8,   7),   S( 22,  29),   S(  1,   7),   S(  5,  30),

            /* rooks: bucket 9 */
            S(  5, -91),   S( 31, -100),  S(  2, -118),  S( 19, -66),   S( 19, -88),   S( 14, -88),   S(  5, -63),   S( -4, -83),
            S(-44, -89),   S(-17, -94),   S(-12, -65),   S(-10, -76),   S( 14, -94),   S(-21, -13),   S(-27, -82),   S(-12, -25),
            S( -8, -37),   S(-12, -46),   S( 11,   0),   S(  5, -20),   S(  9, -40),   S(-20, -22),   S(  1,   3),   S(-13,   5),
            S( 12, -60),   S( 19,   1),   S( 10,   4),   S( -3, -18),   S( -4, -66),   S( 22, -36),   S(-17, -38),   S(-11, -50),
            S( 44, -49),   S( 12, -29),   S( -6, -41),   S(  0, -38),   S( -5, -63),   S(-29, -37),   S(-13, -52),   S(-12, -36),
            S( 15, -52),   S(-29, -55),   S(-14, -65),   S( 24, -17),   S( 21, -28),   S( -1, -13),   S( -5, -26),   S(-13, -58),
            S( 10,   2),   S( 14, -22),   S( 11, -56),   S(-14, -26),   S( 19, -29),   S( -9, -39),   S(  5,  14),   S(-23, -70),
            S(-31, -17),   S( -3, -24),   S( -4, -15),   S( -1,   2),   S(  6, -21),   S( 12,  21),   S(-12,   5),   S(  8,  -2),

            /* rooks: bucket 10 */
            S( -3, -84),   S(-40, -84),   S( -2, -94),   S( 16, -88),   S( 18, -102),  S( 21, -112),  S( 31, -87),   S( -8, -75),
            S(-17, -45),   S(-28, -54),   S(-13, -57),   S(-39, -91),   S( -7, -61),   S(-12, -92),   S( -9, -76),   S(-34, -92),
            S( -2, -23),   S(-24, -22),   S(-24, -41),   S(-43, -65),   S(-12, -21),   S( 17, -22),   S(  4, -64),   S( -7, -24),
            S(  3, -35),   S(-25, -65),   S(-11, -54),   S( -2,  -8),   S(  4, -13),   S(  3,   6),   S( -3, -67),   S(  5, -37),
            S( 16, -34),   S( 25, -38),   S( -1, -53),   S( -2, -63),   S(  8, -36),   S( -5, -56),   S( 22, -63),   S( -7, -69),
            S(-26, -33),   S(-15, -12),   S( -5, -43),   S(-10, -82),   S(  4, -52),   S(  2, -44),   S(-16, -57),   S(-17, -43),
            S(-30, -36),   S(-27, -42),   S( 11, -27),   S(  7, -36),   S( 24, -36),   S(  1, -52),   S(-18, -82),   S(  1, -48),
            S(-13, -25),   S( -7,  -7),   S( -8, -12),   S(-21, -35),   S(  1,  -2),   S(-21, -20),   S( -5, -40),   S(-15, -38),

            /* rooks: bucket 11 */
            S( -9, -68),   S(-44, -54),   S(-19, -61),   S(-10, -17),   S(-13, -20),   S( 30, -86),   S( 40, -56),   S(  6, -110),
            S(  8, -35),   S( -7, -34),   S(-45, -45),   S(-45, -20),   S(-29, -30),   S(  4, -34),   S(-11, -55),   S(-12, -64),
            S(-32,  -3),   S(-24, -14),   S(  0,   5),   S(-24,  -3),   S(  8, -12),   S(  6, -18),   S( 21, -30),   S(-28, -23),
            S(-18, -27),   S(-14, -22),   S(  4,  -5),   S( 16, -10),   S(  9, -19),   S(-18, -55),   S(  4,   2),   S( -4, -22),
            S(  6, -34),   S( -8, -17),   S(  5,   3),   S( -2, -10),   S( 39, -22),   S( 21, -37),   S( 26,  -4),   S( -9, -63),
            S(-16, -35),   S( -9, -21),   S( 11, -27),   S(  0, -24),   S(-10, -25),   S( 29, -25),   S( 24, -14),   S( 11, -54),
            S(-19, -15),   S(-21, -22),   S( 26,  -1),   S(-14, -35),   S( 23, -32),   S( 38, -25),   S( 18, -19),   S( -3,  -9),
            S( 11,  -2),   S( 21,  -5),   S(-12,   5),   S( 13,  16),   S(-19,  15),   S( 12, -16),   S( 55, -26),   S(-18, -10),

            /* rooks: bucket 12 */
            S(-39, -114),  S( -1, -20),   S( -3, -61),   S(-12, -67),   S(-17, -72),   S( 31,  19),   S( -8, -41),   S(-28, -87),
            S(  3, -18),   S(  0,  -5),   S(  6,   2),   S(  3, -11),   S( 10, -23),   S(  9, -19),   S(  8,  12),   S(-13, -45),
            S( -6, -24),   S(  5,  20),   S(  3,  -7),   S( 14, -18),   S( -7, -54),   S( 14,   9),   S(  8,  22),   S( -2,  -9),
            S( -1,   8),   S(  2, -16),   S( 13,  21),   S( 14,  18),   S( 10,  16),   S( -1,   2),   S(  9,  19),   S(  0,  -4),
            S(  8, -13),   S(  9,  21),   S(  5, -24),   S( -1, -32),   S( 11,  36),   S(-12, -26),   S(  0, -32),   S(  1,   3),
            S(  3,  -7),   S( -2, -10),   S( -5, -39),   S( -3, -12),   S( 16,  10),   S( -8, -25),   S( 10,  10),   S(  4, -19),
            S(-10, -18),   S( -4,  -8),   S(  7, -18),   S(  2, -11),   S( -8, -33),   S( 12,   2),   S( 12,  38),   S( -3, -11),
            S( -8,  -3),   S(  7,   6),   S( -2, -33),   S( 14,  14),   S( 13,  21),   S(  0,  -8),   S( -2,  -6),   S(  1,  13),

            /* rooks: bucket 13 */
            S(-29, -86),   S(-29, -103),  S(-18, -41),   S(  2, -43),   S(-19, -88),   S( 16,   5),   S(-25, -67),   S(-18, -46),
            S( -1, -28),   S( -7, -24),   S( -2, -10),   S( -2, -25),   S( 19,   5),   S(  1, -49),   S( 16, -20),   S(  1, -31),
            S( -1, -52),   S( -7, -36),   S( -7, -23),   S(  4, -18),   S( 11, -13),   S( 20, -32),   S( 27,  10),   S(-14, -83),
            S( -5, -31),   S( -9, -35),   S(  7, -12),   S(  3, -32),   S( 17, -12),   S(-18, -68),   S( -4, -12),   S(  3,   5),
            S( -4, -30),   S( -4, -79),   S( -1, -55),   S(  1, -36),   S(  9, -49),   S(  0, -36),   S(  0,  -9),   S(  5,   6),
            S( -2, -10),   S( -5, -46),   S(  2, -31),   S(-15, -73),   S( -2, -88),   S(  9, -11),   S( -3, -18),   S(  2, -10),
            S( -3, -32),   S( -1, -35),   S(-10, -58),   S(-11, -37),   S( -9, -51),   S( -1, -34),   S( -2, -34),   S( -6, -39),
            S( -3, -29),   S(-17, -44),   S(  6,  -8),   S( 10,  -3),   S( -5, -51),   S(  2,  -6),   S( -6, -28),   S(  4,  14),

            /* rooks: bucket 14 */
            S( -9, -28),   S(-40, -73),   S(-18, -62),   S(-12, -85),   S(  0, -34),   S(  0, -59),   S(-41, -131),  S(-12, -80),
            S( 15, -15),   S( 10,  -7),   S(  8, -39),   S( -2, -27),   S( -5, -34),   S( -2,  -6),   S(  2, -35),   S( -5, -58),
            S(  1,  -5),   S(  4, -10),   S( -2, -27),   S( -3, -57),   S(  3, -12),   S( -4, -30),   S(  3, -49),   S(-27, -94),
            S( -8, -11),   S( 20,  49),   S(  8,  14),   S( -8, -39),   S( -8, -25),   S(  1, -19),   S( -4, -32),   S(-25, -67),
            S( -1, -20),   S( 16,  45),   S( -3, -48),   S( 14, -20),   S( -3, -48),   S( 24,   0),   S( -1, -42),   S(-10, -38),
            S( -3, -23),   S( -6, -23),   S(  5, -10),   S(  7, -59),   S(  0, -55),   S( -3, -54),   S(  3, -65),   S( -6, -32),
            S( -8, -36),   S(  1,  -3),   S(-16, -69),   S(-23, -101),  S( -4, -33),   S( -2, -21),   S( -6, -66),   S( -5, -28),
            S(-13, -42),   S( -9, -44),   S(  6,  -1),   S(  0, -67),   S( -9, -66),   S(-10, -82),   S( -9, -69),   S( -3,  -8),

            /* rooks: bucket 15 */
            S(-14, -73),   S(-24, -77),   S(-24, -45),   S(-19, -75),   S( -8, -42),   S( -9, -47),   S(  9,  -6),   S(-10, -62),
            S(  6,  -3),   S(  0, -46),   S( -7, -53),   S(  7, -18),   S( -9, -33),   S(  5,  -1),   S(  6,   8),   S(  1,  -2),
            S(  5,   0),   S(-12, -55),   S( 14,   8),   S(  8, -34),   S(  2, -30),   S( -6, -25),   S( 10,   7),   S(  6,   9),
            S(  0,  10),   S(  5,  12),   S(  8,   7),   S(-11, -19),   S( 12,  12),   S(  3, -22),   S( -3, -29),   S( -5, -26),
            S(  8,   9),   S( -2, -25),   S( 10,   4),   S(  4, -11),   S(  8,  10),   S(  8, -18),   S( 13, -21),   S(-11, -53),
            S(  4,  13),   S( -4,  -7),   S(  0,  10),   S( 11,   7),   S( -6, -23),   S(  4, -68),   S( 13,  -8),   S(  2,  -8),
            S(  2,  -5),   S( -9, -19),   S(  7,   1),   S(  0, -14),   S( -6, -26),   S(  6, -25),   S( -4, -53),   S( -6, -41),
            S(-11, -26),   S(  5,  15),   S(  3,  25),   S(  2,   3),   S( -4, -20),   S( -9, -48),   S(  9, -13),   S(-11, -60),

            /* queens: bucket 0 */
            S(-14, -37),   S(-29, -69),   S(-35, -43),   S( -2, -90),   S( -9, -71),   S( 23, -85),   S(-79, -27),   S(-25, -15),
            S(  4, -61),   S( 11, -82),   S( 11, -83),   S( -6, -55),   S(  7, -42),   S( -2, -81),   S(-34, -54),   S(-35, -20),
            S( -8,   5),   S( -6,  -4),   S( 19, -46),   S( -9,  -8),   S( -4,   1),   S( -2,   3),   S(-12, -30),   S(-73, -105),
            S(-39,  57),   S(  6, -24),   S(-14,  39),   S(-20,  41),   S(-12,  74),   S(-31,  38),   S(-50,   5),   S(-15, -55),
            S(-26,  14),   S(-26,  95),   S(-25,  64),   S(  8,  68),   S(-12,  81),   S(-28,  66),   S(-45,  44),   S(-40,  12),
            S(-15,  19),   S( 22,  75),   S( 36,  63),   S(-33, 107),   S(-59,  73),   S(-53,  60),   S(-61,  -2),   S(-74,   0),
            S(  0,   0),   S(  0,   0),   S( 33,  28),   S(-51,  51),   S(-46,  54),   S(-82,  90),   S(-101,  60),  S(-102,  10),
            S(  0,   0),   S(  0,   0),   S( 17,  27),   S( -3, -10),   S(-48,  23),   S(-32,  23),   S(-58,  16),   S(-64,  -3),

            /* queens: bucket 1 */
            S( 11, -41),   S(  1, -27),   S( 11, -75),   S( 15, -92),   S( 19, -73),   S(  7, -69),   S( -1, -34),   S(  2,  21),
            S(-28, -25),   S( 28, -26),   S( 37, -91),   S( 15, -15),   S( 25, -25),   S(-13, -10),   S(-38,   4),   S(-28, -20),
            S( 20, -16),   S( 14,  -9),   S(  6,   2),   S(  8,   5),   S(-28,  68),   S( 15,  19),   S(-16,  21),   S(  8, -33),
            S( 28, -29),   S( -2,  12),   S( -6,  39),   S(  9,  57),   S(  5,  78),   S(  2,  42),   S( -2,  64),   S(-41,  63),
            S( 20,  19),   S( 36,  72),   S( 22,  76),   S( 10,  58),   S( 31,  95),   S( 37,  40),   S(-42,  91),   S( -2,  38),
            S( 47,   1),   S(106,  31),   S(115,  79),   S(110, 100),   S( 50,  72),   S( 21,  90),   S( 20,  70),   S( -5,   0),
            S( 98, -20),   S( 61,  45),   S(  0,   0),   S(  0,   0),   S( 35,  81),   S(-54,  88),   S(-18,  93),   S(-69,  54),
            S( 91,  13),   S( 58,  38),   S(  0,   0),   S(  0,   0),   S( 41,  31),   S( 41,  53),   S( 57,  52),   S(-36,  47),

            /* queens: bucket 2 */
            S( 29, -49),   S( 24, -38),   S( 32,   6),   S( 41, -53),   S( 50, -81),   S( 28, -63),   S(  4, -64),   S( 39,   3),
            S(  9,  21),   S(  8,  15),   S( 38, -10),   S( 37,   9),   S( 46, -22),   S( 18,  -6),   S(  7,  -8),   S( -6,  36),
            S( 21,  35),   S( 23,  21),   S(  5,  81),   S( 13,  47),   S( 27,  28),   S( 25,  24),   S( 32,  10),   S( 12,  69),
            S( 23,  36),   S(  3, 110),   S(  8,  93),   S(  2, 110),   S( 32,  76),   S(  3, 106),   S( 29,  69),   S( 21,  74),
            S( -7,  80),   S(  7,  35),   S(  1,  93),   S(  6, 133),   S( 26, 141),   S( 81,  63),   S( 57,  93),   S( 66,  58),
            S(-32, 107),   S(-52, 103),   S( -2, 105),   S( 82, 103),   S( 70,  69),   S(103, 113),   S(127,  64),   S( 29, 130),
            S(-41, 114),   S(-30, 107),   S( -7,  81),   S( 86,  78),   S(  0,   0),   S(  0,   0),   S( 20, 101),   S( 27,  92),
            S(-21,  70),   S( 52,  23),   S( 66,  18),   S( 34,  85),   S(  0,   0),   S(  0,   0),   S( 83,  59),   S( 17,  82),

            /* queens: bucket 3 */
            S(-39,  57),   S(-17,  52),   S( -2,  40),   S(  7,  49),   S( -8,  14),   S( -4,   4),   S( 18, -49),   S(-28,  15),
            S(-31,  55),   S(-20,  54),   S( -6,  60),   S(  1,  73),   S(  2,  52),   S(  7,  21),   S( 27, -15),   S( 45, -54),
            S(-29,  63),   S(-22,  95),   S(-20, 123),   S(-27, 140),   S(-15, 105),   S(-11, 103),   S( 13,  73),   S( -7,  45),
            S(-27,  56),   S(-36, 112),   S(-29, 149),   S(-15, 161),   S(-19, 154),   S(-13, 117),   S(  7, 102),   S(-14,  90),
            S(-30, 113),   S(-38, 147),   S(-39, 150),   S(-29, 195),   S(-18, 171),   S(-15, 188),   S(-26, 179),   S(-25, 147),
            S(-52, 113),   S(-40, 138),   S(-47, 163),   S(-31, 180),   S(-39, 189),   S( 11, 177),   S(-26, 211),   S( -7, 172),
            S(-80, 127),   S(-78, 147),   S(-86, 202),   S(-79, 193),   S(-97, 217),   S( 18, 132),   S(  0,   0),   S(  0,   0),
            S(-135, 179),  S(-75, 131),   S(-57, 121),   S(-75, 151),   S(-14, 148),   S( 11, 106),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-51,   0),   S(-45, -33),   S( -3,  23),   S(-35, -49),   S(-38,  11),   S(-31,  -4),   S(-38, -34),   S( 33,  10),
            S( -6,   0),   S(-11, -15),   S(-25,  11),   S(-59, -29),   S(-24,  35),   S( -6,  31),   S(-63, -34),   S( -7, -21),
            S(  4,  47),   S( 21, -28),   S(  7,  -9),   S( 24,   4),   S( 30,   1),   S( 15,  -1),   S(-43, -43),   S( 48,   6),
            S(  0,   7),   S( 51,  11),   S(-11,  -6),   S( 34,  13),   S( 27,  13),   S( 21,  41),   S(-34, -37),   S(-16,   2),
            S(  0,   0),   S(  0,   0),   S( 36,  19),   S( 43,  44),   S( 13,  49),   S( 29,  24),   S( -6,   8),   S(-19,  10),
            S(  0,   0),   S(  0,   0),   S( 32,  31),   S( 63,  75),   S( 36,  58),   S( 10,  51),   S(  8,  23),   S(-50, -16),
            S( 21,  11),   S( 47,  54),   S( 61,  30),   S( 70,  51),   S( 34,  12),   S(-14,  18),   S(  3,   9),   S(-48, -38),
            S( 53,  49),   S(-12,  -7),   S( 35,  14),   S( 61,  48),   S(-20, -27),   S(-34, -41),   S(  1, -14),   S( 14,   3),

            /* queens: bucket 5 */
            S( 38,   5),   S( 22,   3),   S(-20,  18),   S(-40,  -4),   S( 12, -31),   S( 52,  48),   S( 32,  28),   S(-21, -20),
            S( -6,  -1),   S(  6,   1),   S( 39, -22),   S(-30,  10),   S(-16,  -6),   S(-38, -13),   S( 64,  21),   S( 24,   3),
            S( 29,  17),   S( 64,  15),   S( 17,  10),   S(  7,  20),   S(  5,  21),   S(  7,  13),   S(  4,  35),   S(  4,   8),
            S( 27, -22),   S( 73,  65),   S( 18, -14),   S( 37,  61),   S( 64,  27),   S( 16,  25),   S( 34,  24),   S(-24,  -9),
            S( 56,  14),   S( 45,  14),   S(  0,   0),   S(  0,   0),   S(  8,  16),   S( 19,  14),   S( 51,  54),   S( 18,  30),
            S( 25,   1),   S( 45,  42),   S(  0,   0),   S(  0,   0),   S( 40,  37),   S( 73,  40),   S( 37,   4),   S( 33,  19),
            S( 18, -16),   S( 59,  26),   S( 60,  74),   S( 25,  46),   S( 55,  47),   S( 98,  33),   S( 73,  41),   S( 35,   9),
            S( 41,  15),   S( 48,  11),   S( 91,  59),   S( 49,  33),   S( 37,  21),   S( 70,  80),   S( 60,  30),   S( 48,  21),

            /* queens: bucket 6 */
            S( 37,  -1),   S(-13, -13),   S( -6, -31),   S( 10,   2),   S( 13,   1),   S(-26, -14),   S(-13,  -6),   S( 14,  20),
            S( -5, -10),   S( 55,   9),   S( 13,  39),   S( 75,  -1),   S( 34,  22),   S(-21,  -4),   S(-39,  12),   S( -5,  12),
            S(-22,  18),   S( 19,  12),   S( 12,  28),   S( 23,  20),   S(  4,   9),   S( 31,  -2),   S( 71,  37),   S( 62,  48),
            S(-16,  15),   S(-33,  19),   S( 41,  30),   S( 86,  68),   S( 56,   4),   S( 51,  24),   S( 53,  13),   S(124,  16),
            S(  4,  19),   S( 17,  19),   S( 21,  94),   S( 50,  74),   S(  0,   0),   S(  0,   0),   S( 66,  44),   S(105,  66),
            S( 20,  11),   S( 75,  37),   S( 56,  48),   S( 20,  37),   S(  0,   0),   S(  0,   0),   S(128,  70),   S(121,  41),
            S( 11,  19),   S( -3,  26),   S( 77,  19),   S( 56,  29),   S( 72,  72),   S( 75,  58),   S(112,  32),   S(160, -22),
            S( 16,  19),   S( 33,  12),   S( 70,  39),   S(111,  50),   S(134,  46),   S( 78,  33),   S(120,  39),   S( 66,   2),

            /* queens: bucket 7 */
            S(-28,  -8),   S(-13, -29),   S(-38,   9),   S( -7, -25),   S( -8, -13),   S(-25,  19),   S(-18,  16),   S(-30, -44),
            S(-16, -17),   S(-77,  28),   S(-24,  18),   S(-34,  45),   S(-38,  22),   S(-23,  23),   S( -8,  22),   S(-34,   3),
            S(-41,  17),   S(-22, -29),   S(  7,  30),   S( 19,  21),   S( 18,   5),   S( 32, -14),   S(  6,   6),   S( 13,  -9),
            S(-19,   8),   S(-10, -15),   S(  5,  25),   S( 20,  53),   S( 89,  10),   S( 91,  -9),   S( 75, -26),   S( 17,  11),
            S( -9,   1),   S(-31,  19),   S(  7,  47),   S( 39,  41),   S( 70,  31),   S( 56,  33),   S(  0,   0),   S(  0,   0),
            S( -8,  17),   S(-44,  25),   S(-23,  50),   S( -7,  64),   S( 58,   1),   S(114,  69),   S(  0,   0),   S(  0,   0),
            S(-62,  34),   S(-39,  -6),   S( -2,  19),   S( 52,  17),   S( 84,  17),   S(107,  23),   S( 68,  41),   S( 90,  49),
            S(-14, -42),   S( 23, -17),   S( 17,  10),   S( 55, -15),   S( 60,  42),   S( 45,   1),   S(-13,  24),   S( 78,  20),

            /* queens: bucket 8 */
            S(  3, -16),   S(  1, -15),   S( -7, -23),   S(  3, -18),   S(  9,   0),   S(  1,  -6),   S(  2,  -9),   S(  1,   3),
            S(-12, -19),   S( -5,  -5),   S(  5, -10),   S( -1,   8),   S( 33,  24),   S( 16,  15),   S( -1,   0),   S( -2,  -1),
            S(  0,   0),   S(  0,   0),   S(  3, -17),   S(-10, -36),   S( 16,   1),   S(-10,   3),   S(  3,   5),   S(  3,  12),
            S(  0,   0),   S(  0,   0),   S( -5, -13),   S( 11,  14),   S( -5, -14),   S(  7,   0),   S( -1,  16),   S(  4,   2),
            S( -9, -17),   S(  1,   3),   S( 12,  17),   S( 23,   7),   S(  9,   3),   S(  3,  -5),   S(  5,  13),   S( -7, -11),
            S( 18,  12),   S( -2, -24),   S( 25,  30),   S( 10,  10),   S(  8,   9),   S( 20,  24),   S( -1, -12),   S( -9, -22),
            S( -8, -26),   S(  4,   2),   S( 43,  55),   S( 34,  40),   S( 17,   7),   S( -4,  -9),   S(  2,  -5),   S( -2,  -6),
            S(  5,  10),   S( 11,   5),   S( 15,  17),   S( 12,   8),   S(  5,  -8),   S( -6, -17),   S(-10, -30),   S(-25, -43),

            /* queens: bucket 9 */
            S( 25,  21),   S(  5,  -9),   S(  3,  -8),   S( 22, -11),   S( 17,  12),   S(  6,  -7),   S( -7,  -8),   S(  1,  -7),
            S( 12,   1),   S(  1, -28),   S( -4, -15),   S( 25,  27),   S(-15, -44),   S( -4, -23),   S(  3,  -6),   S( -9, -21),
            S(  2, -15),   S(-10, -32),   S(  0,   0),   S(  0,   0),   S( 16,  21),   S( 27,  18),   S(-13, -19),   S( -5, -11),
            S( 10,   9),   S( -9, -20),   S(  0,   0),   S(  0,   0),   S( -6, -10),   S( 25,  19),   S( 23,  21),   S( -2,  16),
            S( 19,  -2),   S(  4, -12),   S(  9,   9),   S( -8,  -5),   S( -2, -30),   S( 22,  25),   S( 19,  -4),   S(-14, -40),
            S( 22,  20),   S(  3, -14),   S( 29,  31),   S(  6, -17),   S( 10, -21),   S(  1, -14),   S(  2,   4),   S(-12, -27),
            S(  3,   7),   S( 27,  16),   S(  8,  12),   S(  4,  13),   S( 22,  15),   S( 21,  14),   S( -5,  -4),   S( 16,   2),
            S(  9, -15),   S( 19,   2),   S(-11, -25),   S( 11,   3),   S(  8,   1),   S(  9,  12),   S(-23, -53),   S( -4, -16),

            /* queens: bucket 10 */
            S(  1,  -1),   S( 10,  -2),   S( -5, -18),   S(  1, -15),   S(  2, -23),   S(  6,   3),   S(  5,  -4),   S(  3, -21),
            S(  7,  10),   S(-11, -23),   S(  0, -27),   S(-10, -35),   S(  2,  -3),   S( 14,   0),   S(  9,  -1),   S(  0,  -8),
            S(  1,   0),   S(  8,  13),   S( -7, -26),   S( -5, -17),   S(  0,   0),   S(  0,   0),   S(  8,   8),   S( -6, -10),
            S( 14,  20),   S( 15,  10),   S( 12,   3),   S( 14,  18),   S(  0,   0),   S(  0,   0),   S( -1, -15),   S(  1, -27),
            S(  4,   6),   S( 27,  15),   S(  3, -13),   S( 45,  55),   S( 11,  12),   S( -6, -11),   S(  1, -27),   S( 33,  10),
            S( -5, -12),   S( 12,   3),   S( 31,  29),   S( 21,  25),   S( 15,   7),   S( 20,  22),   S(  7, -32),   S(  2, -35),
            S( 10,   8),   S(  2,  10),   S(  9,   9),   S(  9,   4),   S( -7, -22),   S( 19,  33),   S( 15, -15),   S( -4, -23),
            S(-22, -41),   S(-13, -31),   S( 26,  13),   S( -7, -28),   S(  6,  17),   S(-15, -36),   S(  7,  -1),   S(  4, -25),

            /* queens: bucket 11 */
            S(  0,   3),   S( -4, -17),   S( -9,  -9),   S(-17, -22),   S(-12,  -6),   S(-20, -22),   S( -1, -31),   S(  1, -14),
            S( -1, -11),   S(-13, -16),   S(-41, -33),   S(  5, -10),   S( 26,  19),   S(  6,  -2),   S( 14,   1),   S(  1, -10),
            S(-10, -18),   S( -3,  -9),   S(-19,  -2),   S( -7, -23),   S( 15,  17),   S(-11, -24),   S(  0,   0),   S(  0,   0),
            S( -6, -27),   S(-12,  -8),   S( -3,  25),   S(  3,  18),   S( 10,  11),   S( -4, -18),   S(  0,   0),   S(  0,   0),
            S( -8,  -1),   S(  6,  -6),   S( 16,  37),   S(  8,  16),   S( 43,  50),   S( 13,  15),   S(  1,   5),   S(-17, -38),
            S( -5,  -3),   S(  0, -10),   S(  3, -13),   S(  1,   5),   S(  7,  11),   S(  5, -17),   S( 11,  14),   S( 19, -34),
            S( -6,  -9),   S( -3,   4),   S( 17,   9),   S( -3,   2),   S( 26,  40),   S( 18,  13),   S(  6,  11),   S( 25,  -4),
            S(-23, -57),   S( -7,  -1),   S(-19, -35),   S( 12,  24),   S( 25,  22),   S( -3, -13),   S(  0, -17),   S( 21,  19),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  8,   9),   S( -5, -12),   S(-17,  -8),   S(  0,  -5),   S(  2,  -5),   S(  3,  10),
            S(  0,   0),   S(  0,   0),   S( 10,  10),   S(  2,  -6),   S(-11, -29),   S( -6, -23),   S( -8, -19),   S(  1,  -4),
            S( -3,  -4),   S( 10,  12),   S(  1,  -6),   S(  4, -25),   S( 19,  38),   S( 12,  30),   S(  4,   0),   S( 12,  19),
            S(  5,   2),   S( 10,  -1),   S( 21,  30),   S( -5,  -9),   S(  6,  -6),   S( -6, -12),   S( -5, -11),   S( -2,  -3),
            S(-25, -49),   S(  2,  -3),   S( 12,   4),   S(-15, -42),   S( 12,  31),   S( -5, -31),   S(-17, -33),   S( -2,  -5),
            S(  6,   5),   S( -5,  -9),   S(  7,  14),   S(-15, -35),   S(  3,   3),   S(-25, -33),   S(-14, -20),   S( -2,  -6),
            S(-12, -20),   S( -6,  -8),   S( -5, -13),   S(  9,  14),   S( -7, -16),   S(-13, -30),   S(  3,   4),   S(  2, -14),
            S(  4,   1),   S( -6, -13),   S(  3,  10),   S( -1,   0),   S(-14, -37),   S( -6, -17),   S(-12, -19),   S(-13, -27),

            /* queens: bucket 13 */
            S( -2, -33),   S(-17, -40),   S(  0,   0),   S(  0,   0),   S( -6, -19),   S( -7, -24),   S(  1, -11),   S( -4,  -7),
            S( -8, -24),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S( -7, -17),   S(-12, -27),   S( -1,  -2),   S( -6, -24),
            S( -6, -15),   S( -1, -14),   S(  1,  -4),   S(  4,   1),   S( -5, -18),   S(-14, -34),   S(-15, -28),   S(  0,  -2),
            S( -3, -12),   S( -8, -25),   S(  9,   5),   S( 10,  -2),   S( 29,  56),   S(  8,   3),   S(-10, -28),   S( -6, -17),
            S(  0,  -6),   S( -7, -24),   S(  0, -17),   S( 13,   6),   S( -1,  -4),   S(  4,   8),   S(-13, -30),   S(-10, -29),
            S( 11,  10),   S(-30, -60),   S( -9, -12),   S(  9,  15),   S(  8,  -2),   S(-17, -31),   S(-19, -42),   S(-14, -38),
            S( -3,  -9),   S( -2,  -7),   S(  6,  10),   S(  4,  10),   S( -2,  -8),   S( -7, -21),   S(-11, -27),   S( -7, -16),
            S(-11, -29),   S( -1,  -9),   S( -7, -15),   S( -2,  -6),   S( -7, -13),   S(  2,  -4),   S(  2,  -4),   S( -6, -20),

            /* queens: bucket 14 */
            S(  8,   2),   S(  4,  -8),   S( -7, -17),   S(  0, -12),   S(  0,   0),   S(  0,   0),   S(  6,  -4),   S(-15, -34),
            S(-12, -29),   S(-20, -48),   S( -1, -10),   S( -5, -22),   S(  0,   0),   S(  0,   0),   S( -8, -19),   S(-20, -49),
            S(-11, -24),   S( -2, -30),   S( -3, -17),   S( 10,  12),   S( -5,  -6),   S(  3,   8),   S( -6, -30),   S(-23, -48),
            S( -7, -18),   S(  0,  -4),   S(  2,  -2),   S(-12, -26),   S(  4,  10),   S(-22, -59),   S( -2, -21),   S(-11, -26),
            S(-10, -20),   S(  2,   0),   S( -3, -17),   S( 13,  31),   S( 11,   7),   S( -7, -24),   S(-16, -31),   S( -2, -11),
            S(-11, -23),   S( -9, -30),   S(-24, -45),   S( -6, -23),   S(  6,   9),   S( -1,  -2),   S( -7, -13),   S(-13, -23),
            S( -8, -15),   S(-12, -24),   S(  4,   3),   S( -4,  -6),   S( -5,  -6),   S(  7,  11),   S( -9, -31),   S(  3,   6),
            S(-22, -48),   S(  8,  -9),   S( -5,  -7),   S(-12, -31),   S(  1,   0),   S( -5, -10),   S(-13, -25),   S(  5,  -3),

            /* queens: bucket 15 */
            S(  1,  -3),   S(-13, -25),   S(  1,   4),   S( -5, -18),   S(  9,   1),   S( -4, -13),   S(  0,   0),   S(  0,   0),
            S( -2,  -7),   S( -3, -20),   S(-14, -32),   S( -3, -17),   S( 10,  20),   S(  6,  13),   S(  0,   0),   S(  0,   0),
            S( -3,  -4),   S( -2,  -4),   S(-11, -14),   S(-15, -29),   S( -5, -13),   S(  5,   1),   S( -5, -15),   S( -1,  -4),
            S( -5, -11),   S( -8, -20),   S( -4, -17),   S(  2,   1),   S(  1,  -6),   S(  4,   6),   S( -9, -27),   S(  2,  -4),
            S( -3,  -5),   S( -1,  -6),   S( -8, -12),   S(  3, -24),   S(  6,  19),   S( 19,  39),   S(-11, -22),   S(-13, -28),
            S(  2,  -2),   S(-11, -20),   S(-10, -27),   S(-18, -30),   S(  9,  18),   S(-15, -29),   S(  5,  -2),   S( -4, -13),
            S( -2,  -4),   S( -7, -20),   S( -4,   1),   S( -3, -15),   S(-11, -20),   S(  2,   5),   S(  5,  17),   S( -8, -24),
            S( -9, -12),   S(-30, -74),   S( -6, -25),   S( -9, -11),   S(-11, -18),   S(  0,  -1),   S(  7,  15),   S( -3,  -4),

            /* kings: bucket 0 */
            S(-10, -23),   S( 27,  -7),   S( 15,  -8),   S(-16, -13),   S(-15, -13),   S( 30, -34),   S( 13,   7),   S( 23, -55),
            S(-10,  24),   S( -8,   7),   S(-14,  22),   S(-54,  25),   S(-40,  38),   S(-14,  29),   S(-10,  36),   S(-11,  29),
            S( 25,  -6),   S( 51,  -3),   S(-15,  16),   S( -8,   0),   S(-37,   1),   S( -6,   1),   S(-66,  19),   S(  8, -19),
            S(-11, -25),   S(-10, -30),   S(-19, -12),   S(-23,  -3),   S(-67,  32),   S(-34,   6),   S(-35,  21),   S( 17,  22),
            S(-48, -88),   S( 27, -26),   S( 12, -32),   S(  4, -17),   S(-56, -10),   S(-19,  10),   S(-22,   3),   S( 13,  13),
            S(  3, -80),   S( 30, -26),   S( 39, -64),   S(  5, -30),   S( 16, -16),   S( 25, -32),   S( 33,  -5),   S( -5, -34),
            S(  0,   0),   S(  0,   0),   S(  7, -33),   S( 26, -39),   S( 16,  -7),   S(  4, -19),   S(-11,  -7),   S(-25, -14),
            S(  0,   0),   S(  0,   0),   S(-12, -77),   S( 15, -23),   S(  4, -13),   S( 17,  15),   S( 13,   6),   S(  1,   0),

            /* kings: bucket 1 */
            S( 19, -46),   S( 37, -22),   S( 10,  -2),   S( 13,  -4),   S( -8,  -4),   S( 31, -17),   S( 13,  17),   S( 27, -22),
            S(  3,   6),   S(  9,  30),   S( 27, -23),   S(-53,  29),   S(-39,  32),   S(  3,   5),   S(  8,  19),   S( -9,  21),
            S(-24, -12),   S( 13, -13),   S(-11, -12),   S(  3, -15),   S(-30,   1),   S( 24, -30),   S( 13,  -6),   S( 42, -15),
            S( 14,   4),   S( 20, -18),   S(  9,  -3),   S(-23,  15),   S(-14,  26),   S(-50,  15),   S( 12,  -1),   S(-23,  34),
            S(-15, -30),   S( -8, -30),   S( 30, -37),   S( 32, -24),   S( 25,  -7),   S(-13,  -8),   S(  2,  10),   S(-14,  -3),
            S( 20, -11),   S( 20, -40),   S( 19,  -5),   S( 48, -13),   S(  9, -28),   S( 18,   1),   S(  3,  21),   S( -5,   0),
            S(-10, -51),   S( 25,   9),   S(  0,   0),   S(  0,   0),   S(-16,   4),   S( 12,  27),   S( -5,  38),   S(-14, -53),
            S(-14, -119),  S(  0,  25),   S(  0,   0),   S(  0,   0),   S( -3, -39),   S(  6, -11),   S( -8,   5),   S( -4, -54),

            /* kings: bucket 2 */
            S( 11, -56),   S( 12,   2),   S( 13, -19),   S( 18,  -8),   S(-11,   9),   S( 36, -26),   S(  5,  25),   S( 29, -21),
            S( 32,  -8),   S(-11,  28),   S( -6,   5),   S(-14,   6),   S(-19,  13),   S(-13,   9),   S(  8,   8),   S(-10,  13),
            S(-23, -10),   S(  7, -10),   S(  7, -20),   S(-36,  -7),   S( -2,  -6),   S(  3, -23),   S( 43, -18),   S( 27, -19),
            S( -3,  25),   S(-27,  16),   S( -7,   8),   S( 10,   2),   S( 11,   3),   S(-22,  -2),   S( -3, -15),   S( 32, -17),
            S(-20,   1),   S( 15,  -5),   S( 19, -24),   S(  7, -14),   S( 33, -34),   S(-18, -31),   S( 56, -36),   S( 38, -39),
            S( -8, -12),   S( 16,  -4),   S( 19, -20),   S( 35, -34),   S( 54, -13),   S( 37,  -4),   S( 75, -27),   S(  3, -42),
            S(-13, -14),   S(  5,  21),   S(-27, -17),   S( 19,   7),   S(  0,   0),   S(  0,   0),   S( 36,  37),   S(-23, -37),
            S( -7, -17),   S(-17, -38),   S( -4, -51),   S( 10,  19),   S(  0,   0),   S(  0,   0),   S( -3,  -9),   S(-16, -140),

            /* kings: bucket 3 */
            S( -1, -63),   S( 13, -13),   S( 23, -33),   S(-15,  -8),   S(-11, -23),   S( 36, -35),   S(  6,  14),   S(  8, -27),
            S(-17,  28),   S(-24,  42),   S(-17,  -2),   S(-39,  10),   S(-49,  20),   S( -4,  -2),   S( -8,  15),   S(-15,  13),
            S( 15, -34),   S(  6, -10),   S( -9, -13),   S(-11, -15),   S( -7,   2),   S( 29, -37),   S( 38, -23),   S( 58, -29),
            S(-48,  39),   S(-71,  16),   S(-68,  13),   S(-108,  28),  S(-69,  19),   S(-62,  -6),   S(-54,  -6),   S(-63,  -5),
            S(-33,  17),   S( -4, -14),   S(-63,  -4),   S(-69,  -1),   S(-13, -26),   S(  4, -41),   S( 12, -54),   S( -9, -62),
            S(-30, -22),   S( 13, -18),   S( 23, -32),   S(-66,  -5),   S( 37, -39),   S( 96, -59),   S(105, -53),   S( 60, -101),
            S(-44,   4),   S( 32,  -2),   S(-11, -29),   S( 19, -33),   S( 28, -22),   S( 48, -53),   S(  0,   0),   S(  0,   0),
            S(-18, -47),   S( -1, -29),   S( -3,  -2),   S(-10,  -7),   S( -8, -84),   S(  1, -28),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-47, -10),   S(  3,  23),   S( -5,  33),   S( 14, -12),   S(  6, -22),   S(-17,   7),   S( 15,  11),   S( 13, -11),
            S(-27,  25),   S( 13,  25),   S(-14,  13),   S(-17,   6),   S( 53, -14),   S(  3,   8),   S( 51,  -4),   S( 17,  -5),
            S(-11,  23),   S(-24,  -8),   S( -6,  -2),   S(-11,   5),   S(-11,   2),   S( -5, -12),   S(-52,   2),   S( 12,  -9),
            S( 25,  -5),   S(  9, -10),   S( 29,  -1),   S( 20,   2),   S( 27,  -8),   S(-13,  14),   S( -7,  13),   S( 17,   0),
            S(  0,   0),   S(  0,   0),   S( 10,  -6),   S(  6,  16),   S(-10,  18),   S(-37,   0),   S(-18,  -3),   S(-17,   9),
            S(  0,   0),   S(  0,   0),   S( -7, -23),   S( 11,   0),   S( 12,  18),   S(-14, -45),   S( -8,  -1),   S(  6,   9),
            S( -3, -14),   S( -4,   9),   S(  2, -36),   S( 10,   5),   S( 11,  20),   S(-15,  -5),   S(  6,   8),   S(  8,  -5),
            S(  8,  63),   S( -7, -10),   S( -6, -20),   S(  1,  15),   S(  5,   8),   S( -5,  -2),   S( -3, -11),   S(  3,   9),

            /* kings: bucket 5 */
            S( 35, -14),   S( -5,   5),   S(-47,  19),   S(-16,  12),   S(-24,  21),   S(  3,   8),   S( 61,   0),   S( 52,  -9),
            S(-16,   5),   S( 49,   3),   S( 34,  -3),   S( 42,  -8),   S( 23,  -4),   S( 24,  -5),   S( 43,   2),   S( 40,  -9),
            S(  9,  -2),   S(-35,   9),   S(  4, -13),   S(-10,  -3),   S(-10,   4),   S(-52,   2),   S( -3,   1),   S(  8,   8),
            S(-18,  -5),   S( 13,  -8),   S( -4,  -6),   S( 11,  20),   S( 45,   5),   S( 15,  -1),   S( 16,   5),   S( -2,   1),
            S(-23, -12),   S(-21, -25),   S(  0,   0),   S(  0,   0),   S( -9,   2),   S( -5,  -1),   S( -9,  -4),   S(-14,   6),
            S(-38,  -6),   S(-26,  -2),   S(  0,   0),   S(  0,   0),   S(-15,   3),   S(-34,   7),   S(-16,  15),   S(-21,   5),
            S( -7,  16),   S( -3,  13),   S(  5,  36),   S( -1, -10),   S( -7,  22),   S( -8,   6),   S(  0,  23),   S(  2,  -5),
            S(-16, -38),   S(  1,  30),   S(  3,  37),   S( -2, -16),   S( -1,  23),   S( -1,  36),   S(  2,  11),   S(  3,  13),

            /* kings: bucket 6 */
            S( 32, -27),   S( 29,  -1),   S(-11,   3),   S(-11,  13),   S(-19,  22),   S(-19,  15),   S( 25,  18),   S( 29,   5),
            S( 44, -29),   S( 39,  14),   S( 35, -11),   S( 50, -15),   S( 22,  -4),   S( 19,   4),   S( 27,   3),   S( 16,   7),
            S(  5,  -8),   S(-13,   5),   S(-15, -10),   S( -7,  -7),   S(  4,  -3),   S(-52,  -2),   S(  2,   0),   S(-37,  18),
            S(-10,   6),   S( 21,   0),   S( 13,  -3),   S( 14,   7),   S( 58,   2),   S( -7,   1),   S( 81, -29),   S( -2,   7),
            S(-11, -15),   S(-25,   4),   S(-14, -11),   S( 13,  -1),   S(  0,   0),   S(  0,   0),   S(  1, -26),   S(-69,  -4),
            S(-21,  -5),   S(  8,  12),   S(-12,   6),   S(-18, -12),   S(  0,   0),   S(  0,   0),   S(-38,  13),   S(-35, -10),
            S(  6,   6),   S( -3,   4),   S(-11,  10),   S( -4,  11),   S( -1,  -7),   S(  1,  -3),   S( -4,  -6),   S(-24, -25),
            S(  7,  44),   S( -5,  -1),   S( -1,  10),   S(  3,   6),   S( -6,   2),   S(  1,  22),   S(  4,  45),   S( -8,   3),

            /* kings: bucket 7 */
            S( 50, -47),   S(-27,  24),   S( -8, -17),   S( -3,  14),   S(-53,  19),   S(-29,  28),   S(-17,  41),   S(-14,  23),
            S( 15,   1),   S( 43, -24),   S(-22,  -2),   S(-13,  -4),   S(-10,   7),   S(-11,  16),   S( 14,   1),   S(  7,  12),
            S( 22, -22),   S(-30,   9),   S(-25,  -3),   S( -4,  -9),   S(-32,   0),   S(-53,  12),   S(  9,   2),   S(-48,  17),
            S( -7,  13),   S( 48,  -6),   S(-17,   3),   S( 27,  -5),   S(-16,  11),   S( 60, -21),   S( 31, -13),   S( 11,  -8),
            S( -4, -20),   S( -3,   2),   S(-12, -14),   S(-19,  10),   S( -6,  -9),   S( 19, -22),   S(  0,   0),   S(  0,   0),
            S( -2, -32),   S(  2,  -4),   S( 11,  -9),   S(  5,  -3),   S( 11, -11),   S(  8,   7),   S(  0,   0),   S(  0,   0),
            S(  3,  -1),   S( 15, -14),   S( 33,   6),   S( -6, -36),   S( 17, -15),   S( -5,   3),   S( 15,  22),   S( -6, -32),
            S(  4, -10),   S(-13, -50),   S( 32,  33),   S( -4, -29),   S(  9,   8),   S(-10, -41),   S( 12,  45),   S(-12, -24),

            /* kings: bucket 8 */
            S( -6,  86),   S(-40,  85),   S(-31,  55),   S(-13, -17),   S(-10,   4),   S(-19,   2),   S( 13,  -2),   S( 12,   2),
            S( 10,  60),   S( 25, -15),   S(  9,  59),   S( 19,  10),   S(  8,  15),   S( 17,  -2),   S(  5,  17),   S( 22,  20),
            S(  0,   0),   S(  0,   0),   S( 25,  48),   S( 25,   2),   S( 29,  -5),   S( -6, -14),   S( -1,  10),   S(-18,   7),
            S(  0,   0),   S(  0,   0),   S( 19,  21),   S( 39, -23),   S( 21,  26),   S(  3,  -3),   S( 25,  -1),   S(  0,  37),
            S(  3,   6),   S(  5,   2),   S(  5, -18),   S(  3,  -8),   S(  5, -29),   S( -7,  -2),   S( 15,  17),   S( -5, -21),
            S(  2,  20),   S(-10, -27),   S( -7,  -7),   S( -1, -14),   S(-11, -19),   S( -8,  12),   S(-27, -12),   S(  5,   4),
            S( -7, -35),   S( -7, -30),   S(  7,  -3),   S(  3,   8),   S(  2, -23),   S( -5,  -4),   S(  6, -11),   S(  1, -51),
            S( -4, -16),   S( -7, -34),   S( -3, -24),   S( -5, -17),   S(  3,  27),   S( -8, -18),   S( 11,  14),   S(  7,   7),

            /* kings: bucket 9 */
            S(-34,  61),   S(-35,  27),   S(-75,  46),   S(-73,  35),   S(-68,  44),   S(-55,  32),   S( 81,  -1),   S( 36,  16),
            S(-30,  34),   S( 39,  22),   S( -2,  -5),   S( 21,  12),   S( 27,  18),   S( 15,   0),   S( 24,  19),   S( 37,   6),
            S(-22,  10),   S(  6,  18),   S(  0,   0),   S(  0,   0),   S(  0,  10),   S(-12,  -2),   S( 30,  -5),   S(-12,   5),
            S( -5, -22),   S(  3,   3),   S(  0,   0),   S(  0,   0),   S(  6,   9),   S( 38,   2),   S( -4,   7),   S(  0,   8),
            S(  1,  -2),   S(  4,   4),   S( -3,   4),   S(  0, -14),   S( -3,   2),   S( 20,  -6),   S( -7,   7),   S(-12,  -4),
            S( -4,  18),   S(  1,  26),   S( 10,  26),   S( -6, -17),   S( -5,  20),   S(  3,  14),   S(-23,  -7),   S( -4,  39),
            S(  4,   4),   S( -7, -37),   S(  0,  -6),   S(  2,   8),   S(  2,   2),   S( 28,  -1),   S( -8, -27),   S(  5,  25),
            S(  8,  34),   S(  0,  17),   S(  6,  14),   S( -1, -10),   S( -9, -58),   S(  1,  26),   S(  1, -39),   S( 13,  40),

            /* kings: bucket 10 */
            S( 24,  28),   S(-22,  15),   S(-14,   4),   S(-50,  28),   S(-69,  28),   S(-144,  54),  S(-17,  42),   S(-101,  81),
            S( 25,  -5),   S(  8,  10),   S(  7,  -5),   S( 17,  14),   S( 81,   6),   S( 36,   6),   S( 12,  22),   S(-15,  25),
            S( 16,  14),   S( 21,   3),   S( 27, -17),   S(  0,   7),   S(  0,   0),   S(  0,   0),   S(  6,   7),   S(-50,  22),
            S( 14,   6),   S( -2, -14),   S( 21,  -7),   S( 15,   8),   S(  0,   0),   S(  0,   0),   S(  5,  11),   S( 10,  -7),
            S( -2,  -7),   S( 19,  12),   S( -1,   8),   S( -4, -37),   S(  7, -17),   S(  7,  17),   S(  5,  -2),   S(-11,  28),
            S( -6,   8),   S( 10,  25),   S(-16,  22),   S(  6, -22),   S( -6,  -1),   S( -9,   1),   S(  4,  16),   S( -9,   7),
            S(  6, -27),   S(  0, -11),   S( 14,   0),   S( 23,  26),   S(  8,   0),   S(  0,  -3),   S( 15, -20),   S( 11,  63),
            S( -1,  10),   S( 17,   1),   S( -5, -25),   S( -1,   3),   S(  0,  -2),   S( -3, -13),   S( -6, -30),   S(  3,   5),

            /* kings: bucket 11 */
            S(-31,  57),   S( -3,  -7),   S(  9, -15),   S(-17,   2),   S(-28,   3),   S(-158,  68),  S(-81,  76),   S(-137, 150),
            S( 24, -47),   S( -9,  20),   S(-13, -23),   S( 34,   5),   S( 26,  16),   S( -7,  52),   S( 52,  -3),   S( 10,  40),
            S(  4, -20),   S( 30,  -9),   S( -2, -17),   S(  4,   6),   S( 47,  -8),   S( 34,  38),   S(  0,   0),   S(  0,   0),
            S( -2,   0),   S(  7,  16),   S( 28,  -9),   S( 29,  -4),   S( 28, -15),   S( 32,  12),   S(  0,   0),   S(  0,   0),
            S(  5,  15),   S(  3,   5),   S( 10,  -5),   S( 16, -18),   S( 11,  -8),   S(  1, -13),   S(  4, -10),   S( 10,   7),
            S(  6,  -1),   S( -2,  -6),   S( 10,  -6),   S(  9,  -7),   S(  5, -11),   S( -4, -20),   S(-10,  22),   S( -4, -20),
            S(  4, -22),   S( 19,  18),   S( 21,  22),   S(  9, -26),   S( 14,  21),   S( 10,   1),   S(  6, -13),   S( -4, -13),
            S( 11,  15),   S( -1, -31),   S(-10, -36),   S(  7,   2),   S( -8, -14),   S( -8, -32),   S(  4, -19),   S( 14,  50),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  3,  53),   S(-20,   7),   S(  1, -11),   S( 15,  21),   S(  4,  -7),   S( 13,  39),
            S(  0,   0),   S(  0,   0),   S( 21,  87),   S( -5,  -6),   S(  8,  39),   S(  7,  -3),   S( 20,  17),   S( -8,  25),
            S(  2,   4),   S( -4, -60),   S( 20,  40),   S( 12,  18),   S( 17,   0),   S( -8, -16),   S(  5,  10),   S( -3,   6),
            S(  0, -17),   S(  9,  24),   S( -7, -23),   S( -1, -55),   S(-17, -29),   S(  4,  -8),   S(-10,  41),   S( -1,  17),
            S( 10,  29),   S(  7,  31),   S(  1,  -4),   S(  9,   1),   S( -8,  14),   S(-10,  -7),   S(-10,   3),   S(-15,  -9),
            S(  7,  26),   S(  1,  29),   S( -7, -18),   S( -8,   3),   S(  2,   4),   S( -5,  17),   S(-15, -13),   S(  6,  39),
            S(  5,  15),   S( -2,  -9),   S( -1, -15),   S(  2,  11),   S( -3,  -8),   S(  6,  -6),   S(  9,  18),   S(-14, -39),
            S( -4,  -8),   S(  1,   2),   S(  0, -27),   S( -2, -25),   S(  2,  -8),   S(  0, -21),   S( -2, -27),   S(  0,  -4),

            /* kings: bucket 13 */
            S( -8,  77),   S( -4,  70),   S(  0,   0),   S(  0,   0),   S( -5,  68),   S(-20,  11),   S( 15,  11),   S( -7,  44),
            S(-21,  33),   S( -4,  -9),   S(  0,   0),   S(  0,   0),   S( 27, -17),   S(  1, -17),   S( -4,  12),   S(  1,   8),
            S( -7,  -2),   S(  8,  40),   S( -4, -37),   S( -5,  12),   S( -2,  -6),   S(  4,   2),   S( -5,  10),   S( -5,  14),
            S(-11, -13),   S(  3,  12),   S(  3,  -4),   S( -1, -20),   S(  3, -29),   S(  8,  -5),   S(-19,  16),   S(-27, -15),
            S(  1,  10),   S( -1,  18),   S(  1,   1),   S( -8, -24),   S(-10, -23),   S( -1,   9),   S(-23,  17),   S( -3,   6),
            S(  3,  14),   S( -4,  -6),   S(  6,  22),   S( -6,  -7),   S(-14,  -9),   S( -7,  19),   S( -9,  -1),   S(  8,  16),
            S(  9,  22),   S( -7,  11),   S(-13, -29),   S(  1, -10),   S( -6, -43),   S(  4, -29),   S( -8, -58),   S(  9,  31),
            S(  0, -13),   S( -1,  36),   S(  0,  18),   S( 11,  20),   S(  6,  11),   S(-10, -15),   S( -4, -13),   S(  4,   9),

            /* kings: bucket 14 */
            S( 10,  48),   S(-13, -12),   S(-10, -22),   S(  7,  36),   S(  0,   0),   S(  0,   0),   S(  3,  86),   S(-54,  73),
            S(-26, -15),   S(-12,   3),   S( -3,  -2),   S(  7,   4),   S(  0,   0),   S(  0,   0),   S(  9,  23),   S(-26,   5),
            S(-13, -14),   S(  6,  -9),   S( 13,  -4),   S(  0, -30),   S(  5, -15),   S(  5,  47),   S(  8,  55),   S(-23,  -9),
            S(  1, -19),   S( 11,  20),   S(-12, -13),   S( 16, -40),   S(-10, -47),   S(  5,   2),   S(  2,   5),   S(  2,  -7),
            S(  7,  17),   S(  7,  -4),   S(-15,   4),   S( -8, -16),   S( -8,  32),   S(  3,  14),   S(  0,  30),   S(  6,  22),
            S(  0,   1),   S( -5,   4),   S(  0,   2),   S(  1,  42),   S( -2, -11),   S(  1, -18),   S(-14, -41),   S(  0,  13),
            S(  8,  16),   S(-17, -39),   S(  4,  36),   S( -1,  12),   S( -1,  -7),   S( -2, -14),   S( -7, -66),   S(  8,  61),
            S( -1,   8),   S(  4,  56),   S( 10,  41),   S( -7, -12),   S(  8,  44),   S( -4, -24),   S(-12, -44),   S( -2, -28),

            /* kings: bucket 15 */
            S(  0,  23),   S( 15,   0),   S( 13,  11),   S(  6,   2),   S(-31,  14),   S(-27,  81),   S(  0,   0),   S(  0,   0),
            S(  7,  -5),   S(  0,  -2),   S( -4, -14),   S( 17,  60),   S( 29,  -8),   S( 34,  69),   S(  0,   0),   S(  0,   0),
            S(-17,   8),   S(  3,  -5),   S(  2, -23),   S( -7, -15),   S(  7, -21),   S( 22,  46),   S( 12,   1),   S(-16, -36),
            S(  5,   0),   S( -6,  -4),   S(  4,  11),   S( -6, -15),   S( -1, -52),   S(  3,  10),   S( 10,  58),   S(-14, -36),
            S(  3,  20),   S(-20,   2),   S( -1,  15),   S(-14, -40),   S( -2, -32),   S( -4,  -4),   S( -5,  16),   S(-11, -28),
            S(  5,  26),   S(-14,  -5),   S( -6, -15),   S( 10,  42),   S(-16,   4),   S( -1,  13),   S( -4,   2),   S(  1,   7),
            S(  7,  37),   S( -1,  13),   S( -5,  -1),   S(  1,   7),   S(  1,  -5),   S(  1,  17),   S(  3,  -5),   S(  3,  23),
            S( -2,  -4),   S(  3,   7),   S(  4,  32),   S( -1,  -6),   S(  5,  32),   S(  6,  36),   S( -6, -13),   S(  0,  -5),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-10, -197),  S( 10, -37),   S( 21,  35),   S( 29,  75),   S( 37,  95),   S( 40, 117),   S( 48, 114),   S( 58, 103),
            S( 85,  71),

            /* bishop mobility */
            S(  8, -82),   S( 10,   2),   S( 20,  39),   S( 26,  72),   S( 31,  97),   S( 35, 112),   S( 36, 119),   S( 41, 115),
            S( 42, 119),   S( 49, 107),   S( 56,  96),   S( 81,  83),   S( 95,  71),   S( 97,  55),

            /* rook mobility */
            S(-14,  -7),   S( -5,  49),   S( -7,  85),   S( -3, 100),   S( -6, 123),   S( -2, 134),   S(  0, 144),   S(  5, 144),
            S(  5, 154),   S(  9, 157),   S( 14, 159),   S( 25, 155),   S( 31, 153),   S( 45, 146),   S(134, 100),

            /* queen mobility */
            S( 64, -48),   S( 69, -90),   S( 57,  23),   S( 54,  93),   S( 54, 141),   S( 52, 187),   S( 51, 224),   S( 51, 243),
            S( 52, 266),   S( 51, 282),   S( 48, 301),   S( 45, 323),   S( 46, 329),   S( 43, 346),   S( 43, 355),   S( 41, 360),
            S( 44, 354),   S( 58, 347),   S( 77, 328),   S( 88, 313),   S(110, 292),   S(162, 255),   S(157, 253),   S(214, 211),
            S(201, 219),   S(153, 218),   S(107, 169),   S( 88, 179),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  7,  29),   S(-15,  54),   S(-28,  55),   S(-27,  61),   S( 28,  22),   S(-20,  44),   S( -3,  80),   S( 33,  36),
            S( 20,  39),   S(  7,  61),   S(-11,  54),   S(-12,  48),   S(  5,  45),   S(-31,  61),   S(-27,  81),   S( 39,  36),
            S( 38,  66),   S( 30,  72),   S( 13,  61),   S( 33,  48),   S(  9,  53),   S(-11,  67),   S(-14, 100),   S(  9,  69),
            S( 58,  86),   S( 72, 101),   S( 46,  68),   S( 23,  52),   S( 18,  57),   S( 20,  82),   S( -7, 109),   S(-47, 129),
            S( 79, 135),   S(126, 152),   S( 93, 104),   S( 65,  87),   S(-20,  84),   S( 63,  84),   S( -3, 160),   S(-38, 140),
            S(156, 179),   S(152, 211),   S(181, 202),   S(188, 201),   S(203, 204),   S(185, 200),   S(184, 204),   S(210, 208),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 16,  10),   S(  2,  -1),   S( 10,   4),   S(  8,  17),   S( 22,  45),   S(  3, -25),   S(-20,  42),   S( 10, -34),
            S(-14,  33),   S( 33,  12),   S(  0,  41),   S( 24,  41),   S( 37,  -1),   S( -1,  40),   S( 19,  -3),   S(  5,  16),
            S(-10,  22),   S( 20,  11),   S( -5,  55),   S( 21,  70),   S( 27,  29),   S( 26,  29),   S( 29,   7),   S(  1,  20),
            S(  4,  32),   S( 30,  50),   S( 35,  76),   S( 16,  94),   S( 74,  64),   S( 68,  44),   S( 31,  51),   S( 14,  30),
            S( 45,  60),   S( 98,  87),   S(121, 143),   S(131, 168),   S(125, 125),   S(153, 153),   S(159, 103),   S( 77,  28),
            S( 75, 182),   S(122, 265),   S(116, 243),   S(117, 228),   S( 78, 164),   S( 64, 160),   S( 53, 165),   S( 17,  94),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 12,  44),   S( 26,  34),   S( 42,  49),   S( 42,  35),   S( 29,  38),   S( 30,  40),   S(  8,  24),   S( 42,  19),
            S( -1,  25),   S( 13,  48),   S( 14,  39),   S( 10,  54),   S( 25,  15),   S(  8,  28),   S( 26,  26),   S(  0,  16),
            S(  0,  11),   S( 22,  37),   S( 52,  43),   S( 43,  29),   S( 40,  41),   S( 68,   9),   S( 15,  24),   S( 22,  -2),
            S( 69,  34),   S(113,  10),   S(131,  79),   S(152,  97),   S(153,  82),   S( 92,  90),   S( 94,  16),   S( 81, -18),
            S(111,  42),   S(178,  46),   S(180, 147),   S(188, 162),   S(149, 181),   S(170, 147),   S(162, 143),   S(-33, 118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 19,  24),   S(  7,  12),   S( 19,  27),   S( -3,  53),   S( 67,  35),   S( 30,   8),   S(  4,  -7),   S( 29,  14),
            S(  3,  13),   S(  8,   8),   S( 20,  17),   S( 19,  29),   S( 18,  13),   S( -1,  10),   S(  4,   7),   S( 31,  -8),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -3, -13),   S( -8,  -8),   S(-20, -17),   S(-19, -29),   S(-18, -13),   S(  1, -10),   S( -4,  -7),   S(-31,   8),
            S(-19, -24),   S( -7, -12),   S(-19, -27),   S(  3, -53),   S(-67, -35),   S(-30,  -8),   S( -4,   7),   S(-29, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -24),   S(-11, -14),   S( -1, -32),   S(-57,   3),   S(-22,  -6),   S(-19, -25),   S(  4, -24),   S(-18, -35),
            S(-25,   8),   S(-16,   0),   S(-25,   7),   S(  1, -12),   S(-35, -12),   S(-30,  -2),   S(-44,   7),   S(-13, -11),
            S(-17, -15),   S(-12, -13),   S(-25,  -2),   S(-29,  -6),   S(-24, -32),   S(-25, -11),   S(-17,  -4),   S(-43, -15),
            S( -4, -27),   S( 19, -33),   S( 17, -25),   S(  7, -28),   S(  4, -20),   S( 59, -39),   S( 37, -35),   S(-10, -44),
            S( 27, -58),   S( 50, -75),   S( 67, -44),   S( 65, -30),   S( 65, -28),   S( 85, -28),   S(135, -75),   S( 32, -68),
            S(134, -107),  S(178, -131),  S(162, -88),   S(105, -56),   S( 81, -40),   S(175, -47),   S(151, -58),   S( 78, -96),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   3),        // attacks to squares 1 from king
            S( 15,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 67, -79),        // king-side castling available
            S( 19,  47),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S(  4, -51),   S(  6, -42),   S( -4, -46),   S(-16, -34),   S(-25, -25),   S(-43, -15),   S(-74,   3),   S(-81,   6),
            S(-101,  12),  S(-85,  17),   S(-63,  19),   S( 10,   5),   S(  0,   7),

            /* orthogonal lines */
            S(-16, -54),   S(-59, -28),   S(-81, -15),   S(-95, -14),   S(-102, -20),  S(-106, -24),  S(-107, -30),  S(-103, -34),
            S(-123, -24),  S(-139, -24),  S(-157, -32),  S(-170, -36),  S(-190, -25),  S(-364,  33),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 11, 222),

            /* passed pawn can advance */
            S(-18,  37),   S(-18,  70),   S( -5, 111),   S( 24, 201),

            /* blocked passed pawn */
            S(  0,   0),   S( 50, -10),   S( 69, -23),   S( 56,  21),   S( 61,  30),   S( 73,  -6),   S(170, -40),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 27,  -6),   S( 25,  46),   S( 31,  40),   S( 32,  67),   S( 61,  81),   S(135, 104),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-28, -16),   S(-30, -14),   S( 10, -54),   S( -9, -49),   S( 19, -60),   S(192, -97),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 16, -27),   S( 45, -33),   S( 12,   8),   S( 36, -56),   S( 27, -153),  S(-14, -246),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( -3,  59),   S( 56,  47),   S(134,  28),   S(  0,  41),   S(226,  73),   S(187,  46),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-30,  97),

            /* bad bishop pawn */
            S( -6, -22),

            /* rook on open file */
            S( 46,  -7),

            /* rook on half-open file */
            S( 13,  21),

            #endregion

            /* tempo bonus for side to move */
            S(  8,  11),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
