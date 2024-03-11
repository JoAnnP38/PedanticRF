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
        public const int MAX_WEIGHTS = 12779;
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
        public const int MINOR_THREAT = 12772;      // minor piece threat
        public const int TEMPO = 12778;             // tempo bonus for side moving

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score MinorThreat(Piece threatenedPiece)
        {
            return weights[MINOR_THREAT + (int)threatenedPiece];
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

        // Solution sample size: 6000000, generated on Sun, 10 Mar 2024 23:54:44 GMT
        // Solution K: 0.003850, error: 0.085042, accuracy: 0.5003
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S(105, 178),   S(440, 537),   S(452, 569),   S(579, 964),   S(1384, 1645), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 95, -117),  S(151, -96),   S( 30, -35),   S(-45,   5),   S(-43,  22),   S(-30,  -7),   S(-51,  -7),   S(-43, -20),
            S(105, -116),  S( 96, -94),   S(  0, -56),   S(-18, -51),   S(-27, -23),   S(-23, -38),   S(-41, -33),   S(-33, -43),
            S( 88, -83),   S( 70, -56),   S(  8, -49),   S(  5, -60),   S(-13, -56),   S(-10, -52),   S(-21, -50),   S(-26, -38),
            S( 71, -37),   S( 47, -42),   S( 16, -42),   S( 17, -70),   S(-15, -48),   S(-29, -42),   S(-30, -41),   S(-24, -16),
            S( 64,  34),   S( 41,  -8),   S( 22, -13),   S( 53, -68),   S(  6, -30),   S(-26, -39),   S(-41,  -3),   S(-33,  39),
            S( 90,  54),   S( 84,  62),   S( 24,  15),   S( 59, -45),   S(-56, -43),   S(-43,  -4),   S(-16,  -2),   S(  0,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26, -32),   S( 26, -38),   S( 52, -30),   S( -6,  16),   S( -4, -25),   S(  1, -15),   S(-42,  -7),   S(-39,  20),
            S( 15, -35),   S( 13, -44),   S(  9, -44),   S( -3, -48),   S(-20, -15),   S(-12, -28),   S(-41, -18),   S(-40, -14),
            S(  8, -26),   S( 10, -23),   S( 19, -41),   S( 17, -54),   S(-19, -23),   S(  1, -37),   S(-19, -25),   S(-19, -13),
            S(  5,  15),   S( 13, -27),   S( 16, -35),   S( 12, -40),   S(-11, -17),   S(  4, -28),   S(-25, -16),   S(-26,  19),
            S( 11,  67),   S(-50,   4),   S(-24, -16),   S( -9, -18),   S( 37, -25),   S(-24,  -3),   S(-29,  20),   S(-32,  73),
            S( 26,  54),   S( 28, -15),   S(-35, -21),   S(  3,  22),   S(-42, -30),   S(-35,   6),   S(-21,  -3),   S(-74,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27,   1),   S(-38,   8),   S(-17,   0),   S(-23,  12),   S(  7, -11),   S( 30, -21),   S( -3, -42),   S(-15, -17),
            S(-25, -14),   S(-54,  -2),   S(-25, -32),   S(-29, -31),   S( -1, -26),   S(  3, -29),   S(-18, -33),   S(-23, -24),
            S(-38,  -2),   S(-36, -15),   S(-14, -41),   S( -9, -48),   S( -6, -25),   S( 10, -28),   S(-19, -24),   S(-15, -14),
            S(-47,  21),   S(-37, -17),   S(-30, -29),   S(-13, -39),   S(  5, -35),   S(-15, -10),   S(-14,  -8),   S(-21,  14),
            S(-34,  67),   S(-87,  26),   S(-68, -16),   S(-60, -12),   S(  1,  14),   S(-45,  16),   S(-33,  39),   S(-38,  90),
            S(-38,  66),   S(-127,  74),  S(-120,  14),  S(-37, -58),   S(-25,  -4),   S(  5,   3),   S( -9,   5),   S(-58,  69),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30,  -8),   S(-43,   1),   S(-30,  -6),   S(-17, -30),   S(-13,  -4),   S( 37, -18),   S( 80, -63),   S( 57, -78),
            S(-36, -26),   S(-55, -15),   S(-33, -42),   S(-28, -29),   S(-22, -22),   S(  6, -37),   S( 52, -65),   S( 53, -71),
            S(-40, -23),   S(-23, -43),   S( -9, -61),   S( -7, -61),   S(-15, -43),   S( 11, -42),   S( 23, -47),   S( 50, -50),
            S(-40,   0),   S(-27, -41),   S(-18, -54),   S(  1, -66),   S( 13, -61),   S(  5, -45),   S( 18, -31),   S( 44, -13),
            S(-13,  32),   S(-32, -19),   S( -8, -58),   S( -2, -45),   S( 79, -46),   S( 50, -38),   S( 46,  19),   S( 40,  76),
            S(-33,  96),   S(-46,  36),   S( -9, -58),   S(-41, -40),   S( 30, -53),   S( 77,  -7),   S( 76,  17),   S( 35,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  -2),   S(  1, -18),   S(-20,   6),   S( 12,   3),   S( -5, -18),   S(-48,   8),   S(-87,  18),   S(-48,  -6),
            S(-24,   9),   S( 42, -18),   S( 33, -31),   S( 14, -22),   S(  0, -23),   S(-38, -33),   S(-16, -39),   S(-15, -27),
            S( 50, -21),   S( 62,  -5),   S( 18, -10),   S(-12, -24),   S(-36, -21),   S(-13, -43),   S(-57, -24),   S(-26, -14),
            S( 24,  30),   S(  3,  40),   S( 33,   6),   S( 19,   0),   S( 10, -32),   S(-50, -16),   S(  1, -38),   S( 69, -32),
            S( 19,  80),   S(  9,  69),   S(-16,  23),   S( -9, -10),   S( 39,  10),   S(-28,  -8),   S(-24, -30),   S( 51,  28),
            S( 72,  91),   S( 48,  72),   S( 16,  23),   S( 18,   0),   S( 14, -35),   S(  3, -23),   S(  9,  -5),   S(-32,  13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-93,  49),   S(-75,  38),   S(  2,  12),   S(  3,   5),   S(-37,  23),   S(-31,  19),   S(-32,   3),   S(-34,  23),
            S(-73,  27),   S(-54,  19),   S( 26, -12),   S(  4,  14),   S( 11,  -9),   S( -2, -22),   S(-12, -17),   S(-51,  15),
            S(-63,  45),   S(-37,  25),   S( 67, -26),   S( 20, -17),   S( 57, -19),   S(-27,  -3),   S( -9,  -6),   S(-14,   9),
            S(-33,  62),   S( -3,  35),   S( 24,  16),   S( 28,  22),   S( 24,   1),   S(-54,  17),   S( 12,  -9),   S( 20,  20),
            S( 41,  62),   S( 85,  28),   S( 91,  29),   S( 39,  33),   S(-14,  56),   S( 30,   5),   S( 12,   2),   S( -6,  62),
            S( 83,  49),   S( 86,  34),   S( 49,   8),   S( 51,   6),   S( 44,  -2),   S( 18,  -3),   S( -6, -24),   S( 58,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41,  22),   S(-34,  10),   S( -4,   4),   S(-23,  -9),   S( 43, -29),   S(-17,   4),   S(-50,   1),   S(-70,  21),
            S(-38,   4),   S( -9, -22),   S(-10, -29),   S( 26, -19),   S( 48, -18),   S( 36, -25),   S(-28, -11),   S(-75,  12),
            S(-45,  14),   S( 11, -18),   S(-13, -16),   S( -5, -11),   S( 35, -11),   S( 62, -27),   S( -6, -13),   S(-41,  21),
            S(-23,  27),   S(-37,  12),   S( 25, -15),   S( 15,  -4),   S( 29,   8),   S( 35,   1),   S( 30,  -1),   S( 36,  14),
            S(-49,  64),   S(-28,  17),   S(-25,   6),   S(  7,  -6),   S( 63,  38),   S(102,  18),   S( 88, -13),   S( 96,  29),
            S( 80,  16),   S( 41,  14),   S( 24, -25),   S(  8, -31),   S( 42,  -4),   S( 41, -19),   S( 52,  -1),   S( 96,  28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -23),   S(-46, -14),   S(  5, -23),   S(-28, -22),   S(  1, -37),   S( 42, -31),   S( -2, -43),   S(-63, -12),
            S(-64, -23),   S(-50, -31),   S(-37, -42),   S( -9, -48),   S(-12, -25),   S( 46, -45),   S( 36, -36),   S(-15, -25),
            S(-60, -23),   S(-55, -34),   S(-21, -49),   S(  0, -46),   S(  6, -25),   S( 37, -27),   S( 35, -39),   S( 22, -23),
            S(-44, -15),   S(-58, -31),   S(-58, -39),   S(-23, -24),   S(  5, -22),   S( 23,  -7),   S( 24,  -1),   S( 67, -12),
            S(-11,  -9),   S(-12, -29),   S(-60, -32),   S(-22, -41),   S( 29,  10),   S( 28,  -4),   S( 37,  53),   S( 72,  52),
            S(-14,   6),   S(-35, -23),   S( 37, -74),   S(  1, -49),   S( 35, -43),   S( 33, -23),   S( 58,  55),   S( 95,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33,  31),   S(-55,  42),   S(-33,  12),   S(-10, -10),   S(  4,  19),   S(-53,  16),   S(-44,  -9),   S(-18,  10),
            S(-38,  33),   S(-35,  20),   S(-48,  30),   S(-39, -12),   S(-17, -11),   S( -9, -30),   S(-34, -28),   S(-26,  -2),
            S(-25,  69),   S( 22,  65),   S(-30,  43),   S(-21,  29),   S( 18, -11),   S(-24, -36),   S(-30, -48),   S( 11, -28),
            S( 34,  95),   S( 58, 118),   S( 42,  66),   S(  5,  35),   S(  5,  -7),   S(-27, -27),   S(-47, -20),   S( -4, -27),
            S( 50, 110),   S( 56, 110),   S( 76, 109),   S( 65,  87),   S(  4,  10),   S(  3, -12),   S(-10, -38),   S(-15, -28),
            S(108, 143),   S( 91, 164),   S( 88, 165),   S( 48,  73),   S( -2,  -5),   S(-10, -28),   S(-11, -51),   S(  7, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87,  65),   S(-64,  39),   S(-29,  14),   S(  4,  19),   S(  3,   7),   S(-80,  28),   S(-91,  15),   S(-60,  23),
            S(-48,  20),   S(-28,   0),   S(-67,  24),   S( -2,  26),   S(-61,  12),   S(-64,   1),   S(-102,  -8),  S(-53,   1),
            S(-63,  57),   S(-34,  67),   S( -6,  66),   S(-51,  66),   S(-62,  60),   S(-88,  12),   S(-35, -21),   S(-28,   1),
            S( -2,  79),   S( 39,  84),   S( 47, 118),   S( 87, 117),   S(-15,  59),   S(-16,  25),   S( 35,  -2),   S( 34, -10),
            S( 82,  80),   S( 76, 107),   S( 69, 116),   S(112, 138),   S( 82, 118),   S( 32,  16),   S( 34, -15),   S( 26, -18),
            S( 39,  59),   S( 59,  80),   S(118,  81),   S(103, 140),   S( 48,  93),   S( -5, -20),   S(  4, -26),   S( -7, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-102,  15),  S(-95,  13),   S(-28,  12),   S( -7,  24),   S(-18,  -9),   S(-109,  41),  S(-135,  41),  S(-96,  48),
            S(-113,  -1),  S(-57, -22),   S(-57,  -9),   S(-37,   1),   S( -6,  22),   S(-60,  25),   S(-126,  30),  S(-94,  26),
            S(-13,  -2),   S(-54,  -1),   S( -7,   5),   S(-65,  53),   S(-70,  95),   S( 24,  38),   S(-93,  38),   S(-86,  46),
            S( 28,  -7),   S( -7,   5),   S( 27,  12),   S(-21,  74),   S( 29, 103),   S( -7,  82),   S( 25,  37),   S( 66,  14),
            S( 72,   2),   S( 25, -20),   S( 20,  49),   S( 63, 116),   S( 97, 120),   S( 93,  68),   S( 25,  69),   S( 78,  41),
            S( 53,  -2),   S( 11, -46),   S( 32,  38),   S( 74,  94),   S( 34,  97),   S( 46,  54),   S( 43,  56),   S( 45,  76),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-112,   9),  S(-79,  12),   S(  1,   0),   S( -2,  15),   S( -7,  17),   S(-85,  41),   S(-41,  24),   S(-53,  37),
            S(-43, -34),   S(-67, -34),   S(-41, -44),   S(-85,  10),   S(-46,  11),   S(-34,  21),   S(-90,  48),   S(-87,  40),
            S(-32, -33),   S(-16, -59),   S(-43, -14),   S(-27,   5),   S(-56,  32),   S(  0,  51),   S(-35,  62),   S(-57,  63),
            S( 13, -19),   S(-45, -12),   S(  4,  -6),   S(-67,   9),   S(-11,  46),   S( 23,  49),   S( 56,  93),   S( 95,  63),
            S( 17,   5),   S(-38,  -9),   S(-16, -28),   S(-12,  21),   S( 68,  77),   S( 69,  68),   S(100,  97),   S(130,  68),
            S(  9, -10),   S(  9, -11),   S(  3, -26),   S(  6, -23),   S( 42,  54),   S( 35, 104),   S( 79, 142),   S(124, 109),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -5,   4),   S(-29, -24),   S(-30,  -9),   S(  5,  11),   S(-12, -15),   S( -6,  -2),   S(  2, -13),   S(-16,  19),
            S(-32, -17),   S(  3,  11),   S(  0,  48),   S( -4,  -2),   S( -7,  18),   S(-12,  -4),   S(-27, -13),   S(  1, -65),
            S( -6,  59),   S( -3,  74),   S( 15,  57),   S( 22,  15),   S(  2, -12),   S(-34, -26),   S(-24, -46),   S(-34, -42),
            S(  1,  86),   S(  8,  80),   S( 60, 107),   S( 41,  63),   S( -1, -36),   S(-24, -47),   S( -9, -26),   S(-55, -61),
            S( 83,  73),   S( 69, 209),   S( 77, 150),   S( 16,  40),   S( 16,   4),   S(  3,  -6),   S(  0, -57),   S( -8, -71),
            S( 36, 118),   S( 45, 247),   S(115, 237),   S( 51,  99),   S(  0,  16),   S( -5, -43),   S(-11, -71),   S(-17, -80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56, -14),   S(-32,   0),   S(  1,  17),   S(  0,   7),   S( -8, -25),   S(-39, -16),   S(-14, -27),   S(-24,  -8),
            S( -5, -32),   S(-23,  -8),   S( -1, -24),   S( 15,  41),   S(-34,   3),   S(  7, -14),   S(-41, -39),   S(-23,  -7),
            S( 19,  29),   S(  6, -14),   S( -1,  31),   S( 11,  58),   S( -8,  42),   S(-18, -32),   S(-19, -48),   S(-20, -54),
            S( 19,  35),   S( 55,  37),   S( 46,  81),   S( 43,  95),   S( 21,  62),   S( 14,   2),   S( 28, -34),   S( 13, -78),
            S( 29,  45),   S( 81, 127),   S(107, 178),   S(109, 197),   S( 44, 107),   S(  3, -10),   S( 17, -31),   S( 15, -69),
            S( 23,  97),   S(102, 184),   S(107, 225),   S(114, 246),   S( 56, 117),   S(  8,  17),   S(  7,   0),   S( 10, -47),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36, -45),   S(-48, -52),   S(-16, -33),   S( -8, -19),   S( -9, -10),   S(-36,  27),   S(-45, -19),   S( -8,  28),
            S(-31,  -9),   S(-17, -44),   S(-23, -66),   S( 17,  44),   S(-25,  24),   S(  3,  26),   S(-15,  17),   S(-34,   1),
            S(-13, -33),   S(-23, -17),   S(-19, -14),   S( -3,  27),   S(-21,  45),   S( 21,  17),   S(  4,  11),   S( -8,  11),
            S( 17, -34),   S( 24, -31),   S(  9, -17),   S( 15,  56),   S( -6, 123),   S( 32,  79),   S( 20,  27),   S( 44,  27),
            S( 14, -51),   S( 29,  -1),   S( 24,   5),   S( 34,  85),   S( 64, 183),   S( 75, 151),   S( 59,  57),   S( 41,  83),
            S( 22, -23),   S( 25, -55),   S( 43,  83),   S( 53, 145),   S( 78, 239),   S( 59, 190),   S( 29, 122),   S( 32,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56, -48),   S(-35, -25),   S(-16, -40),   S(  2,   6),   S( 14,  35),   S( -7,  55),   S(-16, -16),   S(  1,  31),
            S(-22, -78),   S(-27, -45),   S(-24, -57),   S(  5,  -5),   S(-21,   0),   S(  9,  45),   S(  7,  27),   S(-16,  28),
            S(  2, -87),   S(-20, -73),   S( -2, -33),   S( -7,   4),   S( 16,  42),   S( 34,  28),   S( 24,  75),   S( 31,  56),
            S( 11, -24),   S(-26, -31),   S(-17, -32),   S( 14,  24),   S( 20,  68),   S( 48,  79),   S( 11, 106),   S( 12,  59),
            S( -8, -74),   S( -9, -66),   S(  1,   3),   S( 19,  14),   S( 36,  94),   S( 92, 139),   S( 54, 240),   S( 71,  74),
            S(  6, -33),   S( -3, -19),   S(  3, -12),   S(  5,  22),   S( 31,  78),   S( 64, 215),   S( 23, 226),   S( 46,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-28,  -4),   S( 12,  -8),   S(-35,   4),   S( -8, -28),   S(-38,  -7),   S( -9, -71),   S(-54, -33),   S(-55, -27),
            S(-45,  84),   S( 40, -38),   S(-44,  10),   S(  0, -28),   S( -2, -37),   S(-12, -20),   S(-35, -32),   S(-56, -30),
            S(-15,  77),   S( -1, -12),   S( 10, -15),   S(-14,  15),   S( 31,   0),   S(-31,   5),   S(-11, -24),   S(-45, -50),
            S( 11, -30),   S( 35,  18),   S( 11,  24),   S( 24,  23),   S( 13,   3),   S( -1,  -6),   S(-13, -35),   S( -7, -18),
            S( -9,   0),   S( 42,  -9),   S( 29, -14),   S( 90, -33),   S( 49,  -6),   S( 32,  22),   S( 10,  -2),   S(-42, -56),
            S( 10, -40),   S( -3,  10),   S( 38,  10),   S( 47,   7),   S( 60, -78),   S( 52,   2),   S( 27, -64),   S( 31, -27),
            S(  0,   7),   S( 19, -72),   S( 48, -34),   S( 22, -42),   S( 10, -16),   S( 19, -55),   S(-27, -55),   S(-25, -26),
            S(-75, -55),   S(-11,  -5),   S(-36, -35),   S( -6, -59),   S(-30, -22),   S(  2,  -2),   S(  1,   1),   S( 29,  13),

            /* knights: bucket 1 */
            S(-70, -13),   S(-55,  82),   S( 47,  11),   S( -6,  42),   S(-29,  50),   S(-38,  31),   S(-38,  72),   S(-16,  25),
            S( 44,  18),   S(-17,  34),   S( 16,  -6),   S( -6,  30),   S(  2,  12),   S( 22,  -4),   S( 26, -16),   S(-28,  -4),
            S(-31,  26),   S( 17,   8),   S( 14,   8),   S( 12,  23),   S(  4,  21),   S(-20,  20),   S( -5,  -3),   S(-31,  18),
            S(  0,  29),   S( 56,  31),   S( 31,  48),   S( 19,  40),   S( 19,  16),   S(  7,  14),   S( 35,  -4),   S( 18,  22),
            S(  5,  38),   S( 36,  11),   S( 55,  11),   S( 36,  30),   S( 38,  34),   S( 44,  14),   S( 28,   8),   S(  6,  31),
            S( 40,  -2),   S( 17,  -1),   S( 23,  34),   S( 40,  26),   S( 11,  25),   S(  5,  60),   S( 61, -16),   S( -4, -12),
            S( 58, -22),   S(  5,  -3),   S(  7, -27),   S(  5,  49),   S( 44,  -2),   S( 56, -20),   S(-45,  -2),   S( 10, -14),
            S(-119, -69),  S(-34, -48),   S(-28,  -4),   S(-13, -25),   S( -8, -10),   S(-28, -32),   S(  1,  -5),   S(-16, -39),

            /* knights: bucket 2 */
            S(-67,  11),   S(  1,  22),   S(-33,  38),   S(-14,  41),   S(-34,  50),   S(-35,  47),   S(-20,  27),   S(-24,  17),
            S(-27, -12),   S(-29,  17),   S( -4,  12),   S( -9,  26),   S(  1,   9),   S( -1,  30),   S(-29,  56),   S(-52,  73),
            S(-27,  18),   S(  4,  17),   S( -5,  36),   S( 15,  21),   S( 19,  15),   S(  5,  17),   S( -4,  36),   S(-31,  24),
            S( -8,  36),   S(-24,  39),   S(  6,  42),   S(  8,  48),   S(  5,  50),   S( -1,  49),   S( 12,  34),   S(-12,  47),
            S( 26,   8),   S( -8,  24),   S(-11,  50),   S(-25,  62),   S( -2,  60),   S( -6,  57),   S(  3,  49),   S(-18,  29),
            S(-14,  19),   S(  8,  13),   S(-26,  48),   S(-13,  40),   S(-23,  44),   S( -8,  31),   S(-43,  11),   S( 37, -22),
            S(-25,  27),   S(-31,  10),   S(-44,  20),   S(-18,  29),   S(  1,   7),   S(  2,  15),   S(-48,  44),   S(-55,   0),
            S(-152,  -5),  S( 11,  -9),   S(-74,  31),   S(-15,   5),   S( 11,   8),   S(-33, -18),   S( -2,  10),   S(-192, -54),

            /* knights: bucket 3 */
            S(-50, -19),   S( -2, -14),   S(-32,   4),   S(-18,  20),   S(-16,  23),   S(-11,   7),   S(  6,   7),   S(-32, -14),
            S(-29,  18),   S(-39,  11),   S(-18,  -6),   S(  5,  16),   S(  8,  11),   S(-15,   1),   S(-10,  -5),   S(-24,  60),
            S(-21, -32),   S(  2,  10),   S( -3,  15),   S( 16,  16),   S( 22,  22),   S( 15,  17),   S(  2,  11),   S( -9,  46),
            S( -9,   7),   S(  3,  37),   S( 10,  48),   S( 13,  46),   S( 24,  45),   S( 17,  51),   S( 30,  26),   S( 12,  40),
            S( 11,   9),   S( -2,  28),   S( 25,  29),   S( 19,  55),   S( 15,  62),   S( 15,  76),   S( 33,  55),   S( -2,  31),
            S( -9,  10),   S( 13,   1),   S( 32,  14),   S( 52,  14),   S( 54,  -5),   S( 71,  -7),   S( -8,  21),   S( -3,  50),
            S( -7,  22),   S(-12,  19),   S( 24, -13),   S( 50,   3),   S( 48, -29),   S( 47, -17),   S( 27, -43),   S( 50, -21),
            S(-140,  27),  S(-35,  11),   S(  8, -13),   S(  1,  24),   S( 10,   9),   S(-24,   7),   S(-10, -12),   S(-83, -84),

            /* knights: bucket 4 */
            S( 15,  -8),   S(-46,  -4),   S( -6,   7),   S( -4,  -3),   S( 10, -16),   S(-41, -21),   S( 18, -70),   S(-19, -35),
            S( 46,  51),   S(-18,  29),   S( -9,  -6),   S( 39, -49),   S( 23, -36),   S(-18, -48),   S( 24, -25),   S(-12, -41),
            S(-16,   7),   S(  0,  22),   S( 27,  -9),   S( 32,   3),   S( 11, -13),   S( -2,  10),   S(-54, -51),   S(-17, -64),
            S( 10,  72),   S( 50, -38),   S( 71,   8),   S( 31,  11),   S( 25,  -2),   S(127, -33),   S( 30, -42),   S( -1, -39),
            S( 85,  32),   S(-35,  24),   S( 48,  20),   S( 51,  15),   S( 58,  27),   S( -5, -10),   S( 14, -46),   S(-31, -55),
            S( 21,  42),   S(-56,  -6),   S( 88,  27),   S( 19,  -1),   S( 13,  -2),   S( 17,  -1),   S( 32,  38),   S(-25, -23),
            S( -4, -13),   S(-22,  -7),   S(-12, -15),   S(  3,  12),   S(  5,  -8),   S( -2, -32),   S(  5, -25),   S(-20, -23),
            S(-15,  -4),   S(  9,  11),   S( 25,  52),   S( -2,   5),   S(-10, -26),   S( 19,  32),   S(  2,   7),   S(  2, -10),

            /* knights: bucket 5 */
            S( 19,  10),   S(  1,   6),   S( 30,  13),   S( 43,  24),   S( 51,   3),   S( 24,  -7),   S(-20,   9),   S(  4,  -3),
            S( 29, -10),   S( 33,  24),   S( 21,  14),   S(  0,  17),   S( 69,   8),   S(  7,  30),   S( 28,  -1),   S(-29, -31),
            S( 10,  19),   S(-33,  46),   S( 65,   6),   S(113,  -2),   S( 12,  29),   S(  5,  27),   S(  7,  -3),   S(  7, -18),
            S( 40,  24),   S( 37,  20),   S( 50,  31),   S( 32,  37),   S( 39,  21),   S( 13,  36),   S( 33,  29),   S( 35,  18),
            S( 18,  42),   S( 48,  17),   S( 51,  46),   S( 97,  36),   S(134,  29),   S( 48,  29),   S( 54,  33),   S( 29,  22),
            S(-13,  28),   S(  5,  26),   S( 38,  11),   S( 36,  49),   S( 29,  47),   S( 48,  45),   S( 21,   8),   S( -2,  14),
            S(  7,  52),   S( 10,  46),   S( 28,  34),   S( 26,  59),   S( -3,  20),   S( 10,  22),   S( 16,  45),   S( -1,  -7),
            S(-13,  16),   S(  8,  -5),   S(  4,  36),   S( -1,   3),   S(  6,  31),   S( -8,  15),   S(  7,  26),   S(-29, -43),

            /* knights: bucket 6 */
            S( -1, -37),   S(-17, -29),   S( 41,  11),   S(-27,  32),   S(  0,  20),   S( 19,  47),   S( -4,  19),   S(-10,  12),
            S(-12, -52),   S( 60,  -6),   S( 53, -11),   S( -2,  14),   S(-15,  34),   S( 52,  25),   S( 30,  29),   S( -5, -28),
            S(  9, -19),   S(  2,  -1),   S( 44,   6),   S( 45,  19),   S(  3,  34),   S(-11,  33),   S( 48,  23),   S( 28,  26),
            S( 44, -14),   S( 55,   3),   S( 90,  18),   S( 82,  14),   S( 70,  17),   S( 41,  41),   S(  3,  55),   S( 19,  49),
            S(  1,  19),   S(122, -21),   S( 93,  10),   S( 87,  17),   S(111,  31),   S(142,  18),   S( 33,  56),   S( 46,  38),
            S(  4,  29),   S( 45,   0),   S( 48,  28),   S( 79,  26),   S( 82,  36),   S( 47,  25),   S( 25,  10),   S( 30,  46),
            S( -8,  16),   S( -4,  21),   S(-46,  14),   S( 20,  33),   S( 12,  36),   S( 19,  46),   S( 30,  76),   S( -8,   3),
            S(-37,   6),   S( 17,  41),   S( 28,   5),   S(  4,  32),   S( 28,  27),   S( 11,  45),   S( 13,  39),   S( 14,   2),

            /* knights: bucket 7 */
            S(-33, -65),   S(-205, -12),  S(-98, -24),   S(-62, -32),   S(-64,   3),   S(-36, -27),   S(-34,  -5),   S( -5,  -6),
            S(-60, -95),   S(-18, -51),   S(-54, -29),   S(-30,   0),   S(-47,  14),   S( 14, -16),   S( 32,  28),   S( 48,   6),
            S(-86, -74),   S(-54, -23),   S(-22,   0),   S( 41, -30),   S( 19, -11),   S( 32,  -5),   S(-18,  56),   S( 69,  34),
            S(-86, -30),   S( 40, -42),   S( 18, -11),   S( 38,   0),   S( 87, -11),   S( 33,   1),   S( 10,   7),   S(-39,  51),
            S(-60, -24),   S(-33, -29),   S( 65, -35),   S( 95, -21),   S(129,  -3),   S( 77,  23),   S(119,  -7),   S( 82,  28),
            S(-20, -39),   S( 19, -38),   S( 37, -29),   S( 33,  -3),   S( 90,   5),   S(119, -15),   S( 71, -15),   S(-32,  34),
            S(-35, -52),   S(-55, -13),   S( -6, -23),   S( 28,   9),   S( 48,  12),   S( 34,  -5),   S( 19,  29),   S(-10,  -3),
            S(-50, -33),   S( -2, -31),   S(-24, -24),   S( 24,  14),   S( 12,   3),   S( 27,  -1),   S( -5, -40),   S( -1,  -1),

            /* knights: bucket 8 */
            S(  1,   5),   S(-15, -25),   S(-11,  -5),   S( -5, -12),   S( -7, -25),   S(-14, -63),   S( -5,   0),   S(-15, -55),
            S(  4,   6),   S(-16, -29),   S( -8, -23),   S(-21, -59),   S(-36, -32),   S(-23, -77),   S(-10, -55),   S(-29, -71),
            S(  8,  17),   S(-26, -46),   S( 28,   8),   S(-17, -21),   S( -9, -27),   S(-22,   1),   S(-15, -32),   S(-14, -42),
            S(-23, -34),   S(-28,   4),   S(-21,   4),   S( 14,  24),   S( 29,  24),   S(  0, -12),   S(-17, -72),   S( -1, -12),
            S( 37,  85),   S( -5,  36),   S( 12,  47),   S( 34,  44),   S(  2,  10),   S(  4,  12),   S( 22,  -2),   S( -8,  -7),
            S(  5,  27),   S(  4,  14),   S( 36,  17),   S( 28,   6),   S( -4,   9),   S(  3,   5),   S( -8, -22),   S( -8,  -9),
            S( 10,  25),   S(  8,  23),   S( -8, -23),   S( 12,   5),   S(  7,  -1),   S(  9,   7),   S( -1,  15),   S( -7, -18),
            S(  4,   1),   S( 17,  35),   S( -5,   3),   S( -2,   4),   S(  6,  16),   S(-12, -44),   S(  2,   7),   S( -4,  -8),

            /* knights: bucket 9 */
            S(-15, -63),   S(-17, -26),   S(-19, -64),   S(  1,   3),   S(-20, -60),   S( -6, -18),   S( -7, -21),   S( -4, -28),
            S( -8, -15),   S(-15, -14),   S(-16, -60),   S( -7,  -4),   S( -5, -19),   S( -4, -32),   S(-12, -28),   S(-12, -31),
            S(  6,  16),   S( -9, -33),   S( -7, -19),   S(  9,  -1),   S(-20,  11),   S(-27,  -4),   S( -7, -10),   S(-15, -25),
            S( -4, -25),   S( -6, -16),   S(  2,  22),   S( 30,  26),   S( 32,   1),   S(  2,  10),   S(-13, -55),   S(  1, -21),
            S( -7,   5),   S( 13,  -3),   S(  7,  11),   S( -4,  23),   S(  8,   1),   S(  8,  -4),   S(  4, -26),   S( 14,  24),
            S(  7,  28),   S( 19,  23),   S( 23,  25),   S( -3,  -1),   S( 23,  17),   S( 11,  33),   S(  2,  21),   S(-16, -49),
            S( -4, -21),   S( -1,  18),   S( 22,  44),   S( -2,  24),   S( 18,  79),   S( -6, -12),   S(  1,  16),   S( -5, -10),
            S(  0,   1),   S(  5,  29),   S(  3,  23),   S(  6,  34),   S(  6,  -5),   S( -1,  20),   S(  0,   7),   S(  0, -10),

            /* knights: bucket 10 */
            S(-17, -50),   S(-26, -82),   S(-17, -48),   S(-33, -51),   S(-19, -36),   S( -4, -37),   S( -5,  -5),   S(  3,   6),
            S( -8, -49),   S( -6, -30),   S(-11, -16),   S(-14, -14),   S( -3, -22),   S(-14, -47),   S(-16, -25),   S( -2, -21),
            S(-11, -51),   S(-13, -71),   S(-19,   9),   S(-15,  -1),   S( 19,   3),   S(-32, -22),   S(-14, -26),   S(-14, -51),
            S( -8, -27),   S( -9, -51),   S(  9, -15),   S( 39,   9),   S(  9,   8),   S( 24,   5),   S( -8,   1),   S( 26,  45),
            S(-11, -60),   S(-28, -48),   S(  5,  15),   S( 47,   7),   S( 28,  32),   S( 21,  -5),   S(  6,  22),   S( 32,  78),
            S(-20, -63),   S(  6,   5),   S( -7,  12),   S( -5,  15),   S( 38,  40),   S( 22,  39),   S( 28,  69),   S(  9,  32),
            S(  0,  -8),   S( -4,  -3),   S(  0,  -3),   S( 15,  31),   S(  3,  18),   S( 19,  43),   S(  4,  -3),   S( 16,  50),
            S( -1, -18),   S(  1,  -1),   S( -7,  -5),   S(  2,  34),   S( 10,  38),   S( -4,  -1),   S(  5,  14),   S( -3,  -1),

            /* knights: bucket 11 */
            S( -2,  -2),   S(-20, -48),   S( -4, -63),   S(-18, -46),   S(-16, -37),   S( -5, -13),   S( -7, -19),   S( -6,  -5),
            S(-18, -32),   S(-16, -33),   S( -8, -78),   S(-18, -25),   S( -2,  -7),   S(-29, -41),   S(-18, -28),   S( -8,  -5),
            S(-13, -53),   S(-45, -65),   S(-34, -15),   S( -2, -14),   S(-16,   3),   S(-23,  -6),   S( 17,  15),   S( -5,  15),
            S(-15, -29),   S(-12, -44),   S(-28,  -9),   S(-16,  34),   S( 23,  30),   S(  7,   7),   S(-18,  10),   S(  3,  18),
            S( -9, -26),   S(-31, -71),   S( 14, -19),   S( 22,   4),   S(  8,   3),   S( 58,  41),   S( -1,  -8),   S( 24,  95),
            S(-19, -28),   S(-11, -41),   S(  0,  12),   S( 36,  32),   S( 11,   5),   S( 59,  56),   S( 13,  25),   S(  3,  68),
            S(  7,  43),   S(-13, -32),   S(  6,  -6),   S( 10,  -8),   S(  7,  20),   S( 11,  38),   S(  7,  28),   S( 20,  74),
            S( -2,   0),   S(  3,   7),   S( 14,  28),   S(  4,   3),   S( -8, -10),   S(  4,  15),   S(  5,  10),   S(  2,  33),

            /* knights: bucket 12 */
            S( -4,  -9),   S( -4,  -9),   S(  0, -13),   S( -4, -12),   S( -7, -21),   S(  0,   3),   S(  8,  16),   S( -4, -21),
            S(  0,  -1),   S(  4,  10),   S( -2,  -4),   S(  8,   4),   S( -5, -13),   S( -5, -40),   S( -1,  -5),   S(  1,  -6),
            S( -1,   5),   S( 11,  24),   S(-14, -28),   S( -6,   6),   S(  1,   4),   S(  7,   5),   S(  3,   7),   S( -6, -21),
            S( -3,   4),   S( -9, -31),   S( -6, -21),   S( 14,  51),   S(-14, -18),   S(  1,  10),   S(  3,   2),   S( -1, -18),
            S(  7,  21),   S(  6,  -5),   S(  5,  19),   S( 13,  40),   S( 12,  27),   S( -3,  10),   S( -5, -24),   S( -3, -11),
            S( -2,   5),   S( -5, -20),   S( -4,  33),   S(  1, -15),   S(  4,   6),   S( -4, -26),   S(  1,  -9),   S(  1,  12),
            S(  3,  12),   S(-20, -27),   S(  2,   5),   S( -5, -24),   S( -1, -10),   S( -5, -19),   S(  0,   2),   S(  0,   6),
            S(  2,   9),   S(  7,  34),   S( -3, -11),   S(  1,   9),   S( -1,  -8),   S( -1,  -9),   S( -2,  -5),   S(  1,   1),

            /* knights: bucket 13 */
            S( -3, -12),   S(  0,  -6),   S(  0,  -3),   S( -3, -13),   S( -8, -23),   S( -1, -12),   S( -3, -19),   S( -1,   1),
            S( -6, -18),   S( -2,  -7),   S( -1, -21),   S( -6, -14),   S( -8, -38),   S(-11, -46),   S(  0,  -6),   S( -2,  -9),
            S( -5, -26),   S(-11, -28),   S(  5,  11),   S(  6,   2),   S( -2,  21),   S(-12, -32),   S( -4,  -2),   S( -7, -25),
            S(-12,   0),   S( 11,  27),   S(  9,  29),   S( -3,   4),   S(  8,  22),   S( 13,  21),   S(  8,  12),   S( -6,  -5),
            S(  6,  14),   S( -3,  31),   S( 15,  21),   S( 14,  37),   S(-10,  -6),   S(  6,  26),   S( -3, -12),   S(  4,  16),
            S(  0,  12),   S( 19,  37),   S(  7,  75),   S(-15,  17),   S(  8,  41),   S( -9, -25),   S(  3,  18),   S( -3, -18),
            S(  1,   6),   S( -1,  16),   S( 17,  26),   S( -2,  21),   S( 20,  80),   S( -8, -23),   S( -4,  -8),   S(  1,   6),
            S(  1,   3),   S( -1,  56),   S(-11,  -3),   S(  5,  34),   S(  3,  33),   S(  2,  11),   S( -3,  -4),   S(  0,  -3),

            /* knights: bucket 14 */
            S( -3, -21),   S( -2, -21),   S(  1,   7),   S( -5, -13),   S( -9, -39),   S( -2, -11),   S( -2,  -7),   S(  4,   7),
            S( -5, -22),   S(  0,  -2),   S( -9, -47),   S( -1, -19),   S(  2, -12),   S(  1,   0),   S(  1,  -2),   S(  3,  12),
            S( -4, -18),   S( -6, -38),   S(-16, -55),   S(  9,  32),   S( -1,   8),   S(  6,  -4),   S( -1,  -5),   S(  1,  -1),
            S(  1,  10),   S( -7, -32),   S(-14, -59),   S( -8, -20),   S( -5, -35),   S( 13,  30),   S( -7, -10),   S(-12, -13),
            S(  2,   5),   S( -7, -34),   S( 13,  56),   S( -3, -10),   S(-22, -32),   S( 16,   9),   S(  3,  -5),   S( -8, -30),
            S( -3,  -8),   S( 10,  28),   S( -5,  -1),   S(  8,  20),   S(  8,  11),   S( -1,  28),   S( 10,  39),   S(  0,  10),
            S(  1,   3),   S( -3,  -9),   S(  8,   7),   S(  2,  12),   S( -3,  36),   S( -5,  12),   S( -7,   9),   S(  2,  11),
            S(  0,  -5),   S(  1,  14),   S( -5, -13),   S(  6,  38),   S(  5,  21),   S(  0,   6),   S(  6,  41),   S( -1,   1),

            /* knights: bucket 15 */
            S( -1,   1),   S(  0, -10),   S(  3, -11),   S( -6, -11),   S( -4, -15),   S(  4,   3),   S(  2,   0),   S( -1,  -4),
            S( -4, -21),   S(  0,  -7),   S(  4,  11),   S( -2, -18),   S(  2,  -3),   S(  1,   4),   S(  0,  -7),   S( -1,  -3),
            S( -3, -13),   S( -5,   1),   S( -3, -10),   S(-10, -48),   S( -6, -24),   S(  6,   0),   S(  1,  -6),   S( -2, -16),
            S(-13, -35),   S( -7, -35),   S( -3, -20),   S( -2,  13),   S(  6,   2),   S( 18,  36),   S(  3,  11),   S( -2,   2),
            S( -2,  -3),   S( -4, -16),   S(  0, -13),   S(  1, -12),   S(  4,   1),   S(  9,  13),   S(-11, -24),   S( -1,  -9),
            S( -1,   2),   S(  0,   9),   S( -4, -17),   S(  6,  36),   S( -2, -11),   S( -5,   5),   S( -1,  -5),   S( -3,  19),
            S( -6, -17),   S( -1,   0),   S(  2,  -1),   S( -3,   1),   S( -2,  -3),   S(  8,  45),   S( -7, -17),   S(  4,  27),
            S(  0,  -1),   S(  1,   4),   S( -5, -12),   S(  0, -11),   S( -4, -23),   S( -4,  -2),   S(  7,  30),   S( -2,  -4),

            /* bishops: bucket 0 */
            S(  7,  48),   S(  6, -13),   S( 57,   7),   S( 18,  11),   S(  2,   2),   S(  5,  -4),   S( 25, -46),   S(  7, -62),
            S( 37, -27),   S( 93,  11),   S( 54,   3),   S( 16,   0),   S(  6,  21),   S(  2,   5),   S(-20,   4),   S(  5, -25),
            S( 34,  27),   S( 50,  16),   S( 38,  40),   S( 16,  49),   S( 13,  38),   S(  1,  44),   S( 17,   1),   S( 23, -41),
            S( 23,  22),   S( 53,  25),   S( 51,  24),   S( 41,  37),   S( -1,  57),   S( 20,  27),   S( 10,  10),   S(-15,   3),
            S(  4, -11),   S( 37,  -2),   S(-19,  70),   S( 60,  37),   S( 29,  45),   S(  8,  42),   S( 18,  16),   S(-54,  40),
            S(-32,  85),   S( -1,  38),   S( 23,  41),   S( 26,  47),   S( 37,  61),   S( 27,  21),   S(-18,  43),   S(  7,  23),
            S(-67,  57),   S(-12,  49),   S(  7,  33),   S(-55,  84),   S(-71,  24),   S( 35,  27),   S(-19,  19),   S(-25,  20),
            S(-17, -17),   S(  2,  33),   S(-25,  38),   S( -9,  53),   S(-28,  21),   S(-14,  47),   S(-12,  44),   S(-30,  29),

            /* bishops: bucket 1 */
            S( 57,  32),   S( 12,  19),   S( 14,  23),   S(-16,  39),   S( 15,  16),   S( -2,  24),   S(  6,  17),   S(-37,  32),
            S( 18,  -5),   S( 37,  21),   S( 55,  11),   S( 29,  23),   S(  9,  15),   S( 22,  -2),   S(-24,  31),   S(  2,   9),
            S( 43,  13),   S( 15,  23),   S( 48,  39),   S( 17,  41),   S( 17,  43),   S( -8,  44),   S( 27,  13),   S( -2,  -3),
            S( 36,  -5),   S( 43,  33),   S( 12,  37),   S( 32,  49),   S( -1,  54),   S( 21,  22),   S( -9,  57),   S( 13,  -1),
            S( 59,  30),   S( 16,  38),   S( 21,  44),   S( 24,  41),   S(  3,  49),   S(-29,  46),   S( 19,  15),   S(  2,  36),
            S(-29,  58),   S( 10,  51),   S( 23,  49),   S( 49,  23),   S( 14,  55),   S( 24,  45),   S(-18,  53),   S( 21,  13),
            S(-16,  55),   S(-13,  41),   S( 17,  49),   S( 26,  68),   S( 41,  33),   S(-13,  57),   S(-28,  59),   S(-65,  64),
            S( 33,  64),   S(-29,  39),   S(-23,  36),   S(-50,  49),   S(  3,  38),   S(-31,  24),   S(-28,  51),   S(-23,  94),

            /* bishops: bucket 2 */
            S( 20,  24),   S( -2,  19),   S( -2,  31),   S(-15,  32),   S( -9,  36),   S(-22,  29),   S(-21,  -1),   S(-46,  32),
            S(-19,  23),   S( 13,  25),   S( 18,  20),   S(  3,  27),   S(  3,  34),   S( 10,  10),   S(  9,  -1),   S(  0, -28),
            S( -2,   6),   S( -5,  29),   S( 10,  65),   S(  1,  57),   S(  3,  49),   S( 11,  40),   S(  6,  28),   S( -8,  -1),
            S( -2,  35),   S(-25,  61),   S(-16,  57),   S( -2,  62),   S( -9,  61),   S(  5,  46),   S(  5,  39),   S(-11,  17),
            S( -7,  32),   S(-18,  48),   S(-18,  44),   S(-40,  69),   S(-41,  75),   S(-14,  59),   S( -3,  33),   S(-18,  15),
            S( 10,  31),   S(-32,  46),   S(-11,  64),   S(-37,  63),   S( 22,  32),   S(-32,  73),   S(  0,  57),   S(-13,  46),
            S(-15,  50),   S(-46,  52),   S(-47,  80),   S(-15,  41),   S(-76,  75),   S(-48,  47),   S(-79,  79),   S(-34,  44),
            S(-101, 104),  S(-86,  92),   S(-54,  56),   S(-67,  60),   S(-71,  61),   S(-82,  67),   S(  2,  40),   S(-98,  54),

            /* bishops: bucket 3 */
            S( -6,  43),   S(  5,  27),   S( 13,  33),   S(  3,  33),   S(  2,  42),   S( 41,   0),   S( 38, -16),   S( 28,   0),
            S( -9,  27),   S(  6,  34),   S( 18,  23),   S(  3,  45),   S( 16,  35),   S(  9,  42),   S( 50,  34),   S( 14,  17),
            S( 20,   8),   S( -7,  55),   S(  6,  73),   S( 17,  50),   S( 12,  77),   S( 18,  60),   S( 22,  48),   S( 40,   0),
            S( 12,  27),   S(  3,  58),   S(  4,  72),   S( 15,  77),   S( 11,  73),   S( 20,  53),   S( 16,  64),   S(  1,  23),
            S( -4,  41),   S( 12,  48),   S( 14,  45),   S( 14,  81),   S(  1,  70),   S( 19,  73),   S(  6,  52),   S( 16,  55),
            S(  8,  51),   S(  6,  63),   S( 21,  59),   S( 15,  52),   S(  6,  59),   S( 26,  68),   S( 27,  60),   S( -9,  81),
            S( -5,  36),   S(-27,  69),   S( 27,  47),   S(  1,  52),   S(-11,  45),   S(-23,  52),   S( -9,  70),   S( -3,  66),
            S(-28, 105),   S(-38,  65),   S( 66,  28),   S(  0,  57),   S(-40,  69),   S(-21,  70),   S( 12,  49),   S( 71,  26),

            /* bishops: bucket 4 */
            S(-14, -24),   S(-28,  14),   S(-55,  33),   S(-56,  32),   S(-48,  16),   S(-24,   4),   S(-11, -11),   S( -1,   3),
            S(-30,  19),   S(  7,   8),   S(  9,  23),   S(-15,   5),   S(-11,  -6),   S( 75,  -8),   S(-11,  -2),   S( 11, -27),
            S(-18,  34),   S(-12,  18),   S( 17,  21),   S(-12,  24),   S( 26,  24),   S( 27,  11),   S(-30, -11),   S(-70, -15),
            S(-25,  29),   S(  9,  32),   S( 63,  22),   S( 45,  37),   S( 12,  37),   S( 58,   3),   S( 12,  26),   S( 22,   1),
            S( 23,  18),   S( 11,  32),   S(-20,  65),   S( 21,  30),   S( 31,  35),   S(-26,  19),   S(-20,  10),   S( -7,   4),
            S(  5,   4),   S( 49,   9),   S( 20,  13),   S( 29,  21),   S( 19,  37),   S( 12,  22),   S( -4,   5),   S( 13,  -2),
            S(-36,   7),   S( 41,  34),   S( 10,  30),   S( 14,  40),   S(-32,  21),   S(-10,  -2),   S( -2,  -8),   S( -8,   8),
            S( 11,  40),   S(-31,  -8),   S(-10,  34),   S(-10,   2),   S(  2,  27),   S(  1,  12),   S( -4, -11),   S(  0,  14),

            /* bishops: bucket 5 */
            S(-16,   8),   S(-28,  21),   S(-46,  42),   S(-28,  25),   S(-55,  39),   S(  3,  10),   S(-17,  27),   S(-17,  38),
            S( -8,  33),   S(-31,  43),   S(-57,  69),   S( 17,  17),   S(-22,  35),   S(-10,  22),   S(-43,  14),   S(-26,   9),
            S(-55,  41),   S(-17,  51),   S( 51,  28),   S( 23,  32),   S(  4,  45),   S(-21,  34),   S(-24,  37),   S(-16, -16),
            S( 25,  25),   S( 28,  38),   S(  1,  54),   S( 20,  41),   S( 48,  33),   S( 52,  32),   S(  4,  31),   S( -4,  40),
            S( 49,  50),   S( 43,  33),   S( 85,  22),   S( 88,  35),   S( 71,  17),   S( 56,  30),   S( 30,  21),   S(-31,  23),
            S( 10,  27),   S( 59,  42),   S( 89,  43),   S( 60,  40),   S(-19,  52),   S( -7,  20),   S( -1,  28),   S(  9,  37),
            S( -2,  51),   S(-16,  51),   S(  5,  41),   S( 39,  38),   S(  8,  39),   S(  6,  56),   S( -1,  42),   S( -2,  23),
            S(-16,  46),   S( 22,  27),   S(  2,  30),   S( 13,  29),   S( 16,  46),   S( -5,  50),   S(  1,  83),   S(  2,  12),

            /* bishops: bucket 6 */
            S(  9,  34),   S( 16,  25),   S(-30,  43),   S( -5,  25),   S(-14,  27),   S(-62,  44),   S(-27,  45),   S(-19,  24),
            S(  3,  19),   S(  5,  27),   S(  3,  34),   S( 29,  20),   S(  9,  32),   S(-21,  29),   S(-111,  55),  S( 28,   5),
            S(-24,  18),   S(-29,  34),   S( 47,  44),   S( 42,  28),   S( 56,  21),   S( 35,  27),   S( 29,  34),   S(-83,  38),
            S( 19,  33),   S(-16,  48),   S(  8,  49),   S( 73,  24),   S( 39,  40),   S( 13,  34),   S( 36,  51),   S(-38,  21),
            S(-50,  66),   S( 48,  28),   S( 63,  22),   S( 63,  35),   S(123,  35),   S( 77,  23),   S( 53,  29),   S( 14,  29),
            S( -6,  22),   S(-17,  42),   S( 20,  43),   S( 34,  41),   S( 29,  46),   S( 66,  36),   S( 36,  34),   S(-18,  54),
            S(-16,  16),   S(-20,  35),   S( 21,  30),   S(  1,  38),   S(  6,  45),   S( -8,  41),   S(  3,  55),   S(-17,  36),
            S(-14,  62),   S(-16,  50),   S( -3,  41),   S(-40,  52),   S( -1,  38),   S(  0,  56),   S( -2,  32),   S( 10,  48),

            /* bishops: bucket 7 */
            S(-33,   7),   S( -7,   6),   S(-42, -12),   S(-48,   7),   S(-32,  -4),   S(-79,  16),   S(-55, -43),   S(-54, -11),
            S(-50, -17),   S(-37,  -5),   S(-25,   9),   S( -3,   0),   S(-29,  20),   S(-46,  27),   S(-35,   9),   S(-27, -25),
            S(-23,  -4),   S( 14,  -9),   S(  8,  35),   S( 13,  13),   S(-30,  33),   S(  3,  12),   S( -6,  19),   S(-35,  32),
            S(-45,  23),   S( 36,  17),   S( 67, -12),   S( 68,   3),   S( 97,  11),   S( 34,   8),   S( 27,  31),   S( 39,   8),
            S( -9,   3),   S(-23,   4),   S( 43,   2),   S( 82,  10),   S( 76,  23),   S(100,  14),   S( 20,  27),   S( 37,  -6),
            S( 15, -30),   S(-11,  10),   S( -6,  12),   S( 55,   4),   S( 38,  11),   S( 52,  39),   S( 53,  25),   S( 26,   2),
            S(-21,  -8),   S(-13,  12),   S( -1,  14),   S(  2,  23),   S( 18,   0),   S( 21,  12),   S( 22,  15),   S( 25,  31),
            S( -5,  13),   S(-24,  18),   S(-46,  12),   S( 13,  21),   S(  8,   4),   S(  2,  26),   S(  1,  26),   S( 28,  46),

            /* bishops: bucket 8 */
            S( -5, -43),   S(-12, -51),   S(-55, -46),   S( -5, -33),   S(  5,   2),   S(-12, -27),   S(  6,  -2),   S( -9, -25),
            S( -9, -25),   S(-24, -59),   S(-11, -39),   S(-13, -21),   S( 29,  12),   S( -7, -26),   S(-20, -36),   S(  0, -29),
            S(-17, -35),   S(-13,  -9),   S(  1,   6),   S( 15, -31),   S(  4,  -8),   S(  8, -25),   S( 19, -33),   S(-29, -40),
            S( -2,  16),   S( 14,  18),   S( -9,  -6),   S( 19,   8),   S( 15,   1),   S( 15, -23),   S(  0, -15),   S( -2, -38),
            S( 23,  62),   S(  5,  25),   S( 38,   1),   S( 60, -15),   S( 30,  -5),   S( 24,  17),   S(  2, -29),   S( -5, -36),
            S(-15, -22),   S( 26,  18),   S( 38,  13),   S( -6, -23),   S( 26,   8),   S( 13, -19),   S( -2, -49),   S(-17, -50),
            S( -2,  -4),   S( 31,  36),   S(  1, -14),   S(  4,  -7),   S(  3, -22),   S(  8,  -8),   S(  5, -19),   S(-15, -41),
            S(-13, -30),   S( 15, -15),   S( -2, -15),   S(  0, -23),   S( -8, -44),   S( -4, -52),   S( -9, -46),   S( -8, -41),

            /* bishops: bucket 9 */
            S(-10, -35),   S(  9, -47),   S(-28,  -5),   S(  2, -35),   S(-22, -62),   S(  4, -57),   S(-14, -52),   S(  6,  -5),
            S(-12, -50),   S(-13, -63),   S(-28, -32),   S( 22, -16),   S( -4, -21),   S(  0, -39),   S( -7, -25),   S(  1,  -9),
            S( 12,   0),   S( 12,  14),   S( 10, -34),   S( 35, -12),   S( 34, -29),   S( 38, -33),   S(-21, -30),   S( 13, -12),
            S(-13, -17),   S( 27,   4),   S( -9, -13),   S( 79, -27),   S( 46, -14),   S( 19,  -3),   S( 21, -28),   S( -2, -39),
            S( 13,  -2),   S( 25,  -5),   S( 37,  15),   S( 39,  10),   S( 49, -23),   S( 12, -28),   S( 21, -19),   S( -7, -14),
            S( -1, -20),   S( 41,  13),   S(  5,  12),   S( 13,   6),   S( 40, -22),   S( 22, -32),   S(  4, -56),   S( -7,  -2),
            S( -6, -25),   S( 17,  -1),   S( 12,  -9),   S( 20, -11),   S( 33,   9),   S( 12, -49),   S( -4, -20),   S( -2, -29),
            S(  2,   2),   S( -3, -20),   S( -1, -44),   S( -2, -57),   S( -8, -52),   S(  3, -12),   S( -1, -16),   S(-10, -47),

            /* bishops: bucket 10 */
            S(-19, -44),   S(  6, -60),   S(-39, -27),   S( -3, -37),   S(-25, -25),   S(-33, -62),   S( -4, -38),   S( -9, -81),
            S(  0, -57),   S(-24, -34),   S( 22, -48),   S( -6, -55),   S( -6, -35),   S(  9, -19),   S(-12, -60),   S(-17, -37),
            S(-15, -51),   S( 14, -44),   S( -1, -59),   S( 36, -33),   S( 45, -38),   S(-18, -12),   S(-22,   1),   S(  5,   6),
            S( -4, -50),   S( 16, -45),   S( 38, -35),   S( 48, -39),   S( 70, -21),   S( 32,  -7),   S(  1,  -7),   S( 16,  19),
            S(-22, -76),   S( 18, -38),   S( 55, -42),   S( 62, -19),   S( 44, -16),   S( 47,  10),   S(  1,   2),   S(  1, -40),
            S( -3, -50),   S( 17, -63),   S( 14, -36),   S( 14, -40),   S( 46, -17),   S( 40,   7),   S( 15, -26),   S( 14,  -7),
            S(-20, -80),   S(  9, -54),   S(  2, -64),   S( 31, -22),   S( 10, -46),   S( 29,  -6),   S( 13,  16),   S(  9,  25),
            S( -7, -51),   S(-13, -58),   S(  4,  11),   S(  6, -34),   S( -4, -33),   S(  0, -47),   S(  1,  -3),   S(  4,   4),

            /* bishops: bucket 11 */
            S(  1,  14),   S(-45, -11),   S(-31, -52),   S(-12,  -2),   S(-16, -17),   S(-49, -76),   S( -7, -26),   S(-31, -84),
            S(-16, -85),   S(  5, -49),   S( 10, -18),   S(-25, -38),   S(-21, -38),   S(-25, -62),   S( -3, -48),   S(-18, -27),
            S( -7, -51),   S( 22, -56),   S(  1, -25),   S( 35, -48),   S(  1, -36),   S( 11, -27),   S(-12, -18),   S(  1,  -6),
            S( -8, -34),   S(-12, -58),   S( 35, -47),   S(  4, -51),   S( 50, -29),   S( 31,  12),   S( 27,   4),   S(  4,  22),
            S(-14, -43),   S( -9, -63),   S( 28, -35),   S( 64, -34),   S( 70, -13),   S( 49,  10),   S( 16,  38),   S( 14,  -7),
            S( -4, -44),   S( -4, -75),   S( 15, -56),   S( 16, -33),   S( 31, -32),   S( 53, -11),   S( 31,  27),   S( -3,  -2),
            S( -3, -34),   S(  3, -51),   S(  6, -25),   S( 10, -42),   S( -2, -28),   S( 34, -22),   S( 13, -39),   S( -4,  -8),
            S(-11, -75),   S(-15, -59),   S(  1, -20),   S( 20, -25),   S( 10, -36),   S(-22, -86),   S(  4, -16),   S( -1,  -8),

            /* bishops: bucket 12 */
            S(  2,  -2),   S( -5, -13),   S(-20, -70),   S( -3, -18),   S(  0, -30),   S( -4, -11),   S( -4,  -8),   S(  3,   5),
            S( -4, -29),   S( -5, -24),   S( -2, -32),   S( -1, -10),   S( -4, -44),   S(  6,  12),   S( -5, -25),   S(  0,  -4),
            S( -2, -20),   S( -2, -17),   S(-17, -32),   S(  4, -26),   S( -3,  -9),   S(  0, -35),   S(-14, -53),   S(-11, -38),
            S( -5, -19),   S(  0, -29),   S( -2, -22),   S( -9, -25),   S(  2, -20),   S(  9,  12),   S( -7, -35),   S(-13, -41),
            S(-12, -29),   S(  2,  -9),   S(  7, -35),   S(  0,  19),   S(  9,  -8),   S(-17, -33),   S(  8,  -7),   S(  0,   7),
            S(-20, -37),   S(  4,  33),   S(  7,   4),   S(-20, -55),   S( -3, -15),   S( -3, -10),   S(  3, -16),   S(  0,  -9),
            S( -2,  -9),   S( -5,  -1),   S( -2,   7),   S( -9, -24),   S(  2, -18),   S(  8,  -1),   S( -4, -18),   S(  3,  12),
            S( -1, -17),   S(  2,   9),   S( -6, -25),   S(  1,  -5),   S(  5, -13),   S( -2,  -9),   S( -4, -23),   S( -2,  -4),

            /* bishops: bucket 13 */
            S(-11, -70),   S( -9, -60),   S(-10, -29),   S( -7, -58),   S(-14, -78),   S( -9, -16),   S( -9, -25),   S( -1, -25),
            S( -2, -18),   S(  1, -25),   S(-12, -94),   S( -4,  -9),   S(  5,   0),   S( -2, -37),   S(  4, -10),   S( -1, -36),
            S( -6, -30),   S( 10,   2),   S(  8,  -8),   S(  5, -45),   S( -1, -73),   S( 21, -24),   S(-11, -42),   S(  9,  36),
            S( -2,  -7),   S( -1, -25),   S(-11, -30),   S(-12, -57),   S( 22, -34),   S(  4, -11),   S(  3,  -5),   S( -5, -43),
            S(  0,  -2),   S( -6,  -7),   S( -7, -44),   S( 22, -16),   S(  5,  -5),   S( 11,  -5),   S(  4, -35),   S( -2, -22),
            S( -4, -20),   S( -6, -15),   S( -9,  13),   S(  7, -14),   S(  3,   5),   S( 22,   5),   S( 17, -11),   S( -1, -14),
            S( -8, -34),   S( -9, -20),   S(  6,   6),   S(-10,  11),   S( -8, -56),   S(  8,  31),   S( -5, -34),   S(  3,  -8),
            S(-10, -38),   S(  7,   6),   S( -8, -28),   S( -3, -36),   S( -2,  -4),   S( -9, -38),   S( -5, -22),   S( -5, -36),

            /* bishops: bucket 14 */
            S( -8, -63),   S( -6, -56),   S(-22, -74),   S( -5, -41),   S(-11, -52),   S( -1, -29),   S(-12, -89),   S( -6, -34),
            S(-15, -59),   S( -3, -30),   S( -2, -37),   S(-19, -85),   S( -7, -61),   S( -6, -71),   S(-15, -59),   S( -1, -39),
            S( -4,  -5),   S(-15, -44),   S(-16, -74),   S( -1, -50),   S( -6, -66),   S(-13, -76),   S( -6, -34),   S(  1, -12),
            S( -3, -21),   S( -5, -37),   S(  4, -25),   S( 10, -12),   S( 18, -44),   S(  3, -39),   S(-15, -69),   S( -4, -17),
            S( -4, -28),   S(  2, -16),   S(  7, -27),   S(  2, -36),   S( -2, -68),   S( -3, -41),   S( 12,  -1),   S( -4, -36),
            S(  4, -10),   S( -4, -44),   S( -3, -34),   S(  3,   1),   S(  3, -12),   S(  2,  39),   S( -6, -51),   S( -9, -51),
            S( -6, -49),   S(  2, -35),   S(  1, -30),   S( 11, -12),   S( -2,  -6),   S(  5, -12),   S( -3,   3),   S( -3,  -9),
            S( -7, -46),   S( -4, -30),   S( -7, -33),   S( -6, -37),   S( -7, -31),   S( -2,   6),   S(  9,  35),   S( -1, -13),

            /* bishops: bucket 15 */
            S(  3,  15),   S(  5,  15),   S(-18, -67),   S( -3, -26),   S( -3, -20),   S(-13, -28),   S( -1,   0),   S( -2, -18),
            S(  4,  15),   S(  6,  28),   S(  3, -13),   S(  3, -36),   S( -5, -27),   S( -1,  -3),   S( -6, -23),   S( -4, -21),
            S( -9, -37),   S( -7, -17),   S(-19, -53),   S( -4, -10),   S(-22, -66),   S(-11, -39),   S( -2, -25),   S( -3, -22),
            S( -1, -20),   S(-10, -41),   S(  2,  -2),   S(-16, -55),   S(  5, -20),   S( -3, -30),   S(  6,   6),   S(  1,  -1),
            S(  1,  -9),   S( -9, -29),   S(  4, -17),   S(-21, -62),   S(  1, -26),   S(-12, -41),   S(  9,   9),   S(-11, -43),
            S( -8, -35),   S( -2, -61),   S(-10, -49),   S(-13, -71),   S( -7, -43),   S( -3,   3),   S( 30,  43),   S( -3,  -8),
            S(  0, -16),   S(  1, -22),   S( -3, -19),   S( -2, -32),   S( -4, -36),   S(  4,   3),   S(-13, -31),   S(  5,   7),
            S( -1,  -8),   S( -6, -18),   S( -4, -22),   S( -3, -38),   S( -3, -36),   S(-20, -55),   S(-13, -45),   S( -1,  -6),

            /* rooks: bucket 0 */
            S(-48,  21),   S(  6, -26),   S(  4, -28),   S( 12, -16),   S(  5,  -4),   S( 11, -31),   S(  5,  -7),   S( 15, -12),
            S( 21, -61),   S( 43, -36),   S( 17,  -5),   S( 17, -12),   S( 41, -24),   S(  7,  -1),   S(-27,  10),   S(-39,  13),
            S(  3,  -8),   S( 16,  22),   S( 33,   4),   S( 23,   5),   S(  1,  29),   S( 22, -23),   S(-38,  16),   S(-36, -17),
            S( 40, -26),   S( 76, -10),   S( 39,  31),   S( 63, -13),   S( 32,  -9),   S( 29, -17),   S( 17,  -4),   S(-34,  31),
            S( 62, -27),   S( 76,   3),   S( 60,   3),   S( 54,  -9),   S( 46,   5),   S( 55,  -7),   S( 18,  -4),   S(-27,  22),
            S(102, -69),   S( 61, -19),   S( 89, -22),   S( 34,  -4),   S( 64,   1),   S(-39,  24),   S( 55,  12),   S(-22,  27),
            S( 39,  -5),   S( 63,  -1),   S( 26,  15),   S( 21,   7),   S(  1,  23),   S(  4,  33),   S( -2,  25),   S( 33,   4),
            S(  5,  24),   S(-12,  33),   S( 31,  24),   S( 21,  26),   S( 25,   7),   S( 48,  -1),   S( 25,  14),   S( 25,   0),

            /* rooks: bucket 1 */
            S(-77,  44),   S(-41,   5),   S(-42,   5),   S(-35,  -3),   S(-15, -19),   S(-26, -10),   S(-32,   0),   S(-38,  25),
            S(-49,  14),   S(-57,  22),   S(-11,  -3),   S(-17, -18),   S(-24,   1),   S(-23,  -6),   S(-27, -15),   S(-82,  30),
            S(  1,  20),   S(-19,  45),   S(-19,  30),   S(-33,  35),   S(-42,  31),   S( -5,   7),   S(-21,  21),   S(-46,  30),
            S(-46,  61),   S(-32,  47),   S( -2,  48),   S( -7,  38),   S(-34,  38),   S(-48,  58),   S(-17,  43),   S(-34,  25),
            S( 71,  14),   S( 28,  48),   S( 44,  17),   S( -4,  52),   S(  3,  36),   S( 20,  27),   S( -4,  36),   S(-31,  17),
            S( 69,  15),   S( 10,  40),   S( 28,  37),   S(-17,  32),   S( 61,   1),   S(  7,  38),   S( 45,  -1),   S(-40,  47),
            S(-11,  49),   S(  7,  39),   S( 44,  36),   S(-71,  81),   S(-40,  53),   S( 18,  47),   S(-51,  50),   S(-37,  39),
            S( 51,  30),   S( 12,  40),   S( -4,  35),   S(-66,  66),   S( 28,  34),   S( 23,  21),   S(-15,  32),   S( 38,   8),

            /* rooks: bucket 2 */
            S(-63,  41),   S(-48,  32),   S(-38,  23),   S(-50,  28),   S(-53,  15),   S(-54,  15),   S(-38, -16),   S(-54,  29),
            S(-62,  32),   S(-64,  48),   S(-43,  31),   S(-51,  27),   S(-38,   8),   S(-34, -13),   S(-65,  22),   S(-57,   7),
            S(-75,  64),   S(-49,  55),   S(-41,  59),   S(-38,  37),   S(-39,  44),   S(-46,  46),   S( -5,  10),   S(-29,  20),
            S(-62,  64),   S(-63,  68),   S(-34,  73),   S(-22,  57),   S(-21,  51),   S( 19,  35),   S(-37,  64),   S(-17,  39),
            S( -6,  59),   S(-44,  73),   S(-30,  65),   S( -6,  44),   S( 41,  41),   S(  7,  61),   S(-15,  64),   S(-33,  42),
            S(-17,  52),   S(-23,  62),   S( -8,  47),   S(  9,  41),   S( 32,  41),   S( 38,  35),   S(  7,  39),   S(  0,  40),
            S(-65,  74),   S(-59,  78),   S(-27,  68),   S(  4,  58),   S( 17,  40),   S( 80,   8),   S(-58,  80),   S(-20,  62),
            S(-18,  71),   S(-29,  60),   S(-31,  66),   S(-19,  57),   S(  5,  57),   S( 11,  46),   S(-41,  71),   S(-18,  39),

            /* rooks: bucket 3 */
            S(  4,  74),   S(  0,  71),   S( 16,  64),   S( 23,  59),   S( 18,  53),   S( -3,  62),   S(  3,  65),   S(-12,  42),
            S(-28,  84),   S( -1,  67),   S( 10,  63),   S( 18,  61),   S( 27,  52),   S( 23,  52),   S( 51,   7),   S( 21, -26),
            S(-32,  89),   S( -1,  86),   S( 14,  80),   S( 16,  73),   S( 21,  81),   S( 24,  79),   S( 45,  47),   S(  2,  63),
            S(-10,  89),   S(-22, 102),   S( 32,  79),   S( 43,  66),   S( 29,  74),   S( 14, 110),   S( 76,  69),   S( 22,  78),
            S(  4,  99),   S( 32,  75),   S( 32,  69),   S( 56,  72),   S( 47,  78),   S( 63,  63),   S(103,  47),   S( 61,  37),
            S(  3, 102),   S( 26,  81),   S( 29,  78),   S( 37,  77),   S( 52,  57),   S( 65,  45),   S(109,  34),   S( 95,  33),
            S(-19, 107),   S(-14, 110),   S(  8, 101),   S( 35,  89),   S( 23,  73),   S( 60,  69),   S(103,  59),   S(129,  34),
            S(-45, 143),   S( 28,  95),   S( 35,  83),   S( 57,  67),   S( 75,  60),   S( 75,  53),   S(158,  32),   S(116,  26),

            /* rooks: bucket 4 */
            S(-97,  25),   S( 20, -43),   S(-57,  15),   S( -6,   5),   S(-23, -17),   S( 25, -52),   S(-25, -19),   S(  1, -48),
            S(-16,  -9),   S(-47,  -5),   S(-49,   8),   S(-35,  20),   S(-49,  -1),   S( -8, -20),   S( -2, -41),   S(-28, -26),
            S(-15,  -5),   S(-36, -16),   S(  1,   1),   S( 19, -29),   S(-19,  -9),   S( 10, -38),   S( -2,   0),   S(-29, -25),
            S(-34, -31),   S( 33,   4),   S(-55,  28),   S( 22,  14),   S( 40, -18),   S(-15,   1),   S(-30,  44),   S(-15,   7),
            S(-32,  -6),   S(-31,  32),   S(  4,  13),   S( 49,  28),   S( 11,   4),   S(-11,  -4),   S( 34,  16),   S( 17,   5),
            S(-16,  11),   S( 24,   8),   S( 37,  15),   S( 39,  16),   S( 36,   8),   S( 47,  22),   S(-23,  40),   S( 24,  30),
            S(-11,  -9),   S( 35,  33),   S( 35,  26),   S( 28,  30),   S( 47,   4),   S( 35,   0),   S( 13,  15),   S( 31,  10),
            S( 15, -72),   S( 35,  14),   S( 47,  -1),   S( 19,  -1),   S( 21,   5),   S( 23,  -6),   S(  3,  20),   S(-10,   3),

            /* rooks: bucket 5 */
            S(-45,  35),   S(-31,  44),   S(-26,  32),   S( -7,  -1),   S( -7,   4),   S(-20,  27),   S(  1,  16),   S(-25,  27),
            S(-44,  42),   S(-54,  37),   S(-68,  60),   S(-68,  53),   S( -8,  13),   S(-14,   9),   S(  7,  22),   S(-17,  -3),
            S(-20,  43),   S(-56,  54),   S(-72,  70),   S(-73,  60),   S(-51,  36),   S( -1,  27),   S( -1,  25),   S(-40,  23),
            S(-70,  71),   S(-21,  51),   S(-19,  63),   S( -8,  38),   S(-29,  64),   S(-28,  69),   S( 12,  53),   S(-14,  25),
            S(-22,  69),   S( 23,  57),   S( 22,  54),   S(  9,  78),   S( 38,  47),   S(  3,  72),   S( 68,  47),   S( -3,  49),
            S( 42,  63),   S( 38,  58),   S( 57,  62),   S( 22,  58),   S( 57,  60),   S( 65,  48),   S( 48,  42),   S( 48,  48),
            S( 20,  55),   S( 21,  61),   S( 55,  51),   S( 49,  58),   S( 47,  47),   S( 78,  49),   S( 70,  52),   S( 69,  49),
            S( 76,  34),   S( 69,  26),   S( 16,  74),   S( 34,  41),   S( 47,  40),   S( 53,  38),   S( 77,  23),   S(  6,  38),

            /* rooks: bucket 6 */
            S(-64,  38),   S(-22,  20),   S(-19,  24),   S(-20,  27),   S(-49,  31),   S(-68,  49),   S(-28,  45),   S(-22,  40),
            S(  3,  11),   S(-44,  47),   S(-13,  23),   S(-35,  27),   S(-44,  39),   S(-85,  59),   S(-28,  34),   S( 13,  10),
            S(-39,  47),   S(-63,  57),   S(-10,  43),   S(-48,  44),   S(-31,  43),   S(-34,  61),   S( -1,  54),   S( 14,  13),
            S(-79,  83),   S( 30,  46),   S(-25,  74),   S(-36,  62),   S( -3,  40),   S(-10,  57),   S(-85,  80),   S(  6,  47),
            S(-11,  74),   S( 20,  63),   S( 68,  40),   S(  8,  52),   S(-32,  94),   S( 14,  69),   S( 18,  53),   S( 29,  44),
            S( 15,  66),   S( 83,  45),   S( 74,  41),   S( 30,  45),   S(  3,  62),   S( 51,  53),   S( 22,  60),   S( 88,  45),
            S( 62,  49),   S( 75,  44),   S(119,  25),   S( 82,  31),   S(112,  20),   S( 46,  54),   S( 95,  41),   S( 45,  51),
            S( 69,  59),   S( 49,  45),   S( 55,  44),   S( 30,  50),   S( 73,  37),   S( 77,  36),   S( 89,  36),   S( 54,  31),

            /* rooks: bucket 7 */
            S(-62,  -4),   S(-43,  -1),   S(-24, -16),   S(-21,   2),   S(  3, -12),   S(-30,  16),   S(-40,  14),   S( 26, -27),
            S(-51,  10),   S(-34,  11),   S(-39,   8),   S(-24,  10),   S(  2,  -6),   S( 17,   1),   S(-17,  14),   S(-39,  -7),
            S(-75,  50),   S(-28,  13),   S(-41,  40),   S(-16,   4),   S(  6,   7),   S(-27,  13),   S( -4, -21),   S( 15,  21),
            S(-75,  44),   S(-12,  34),   S(  9,  22),   S( 28,  18),   S(-12,  31),   S( 31,  18),   S( 10,  11),   S(  0,  20),
            S(-24,  44),   S( 10,  26),   S( 32,   8),   S( 47,  12),   S( 72,   4),   S( 88,   7),   S( 45,  34),   S( 64, -15),
            S( 31,  27),   S(-11,  27),   S(116, -25),   S( 90,  -6),   S( 68,   5),   S( 89,  -7),   S( 86,  17),   S( 36,  -2),
            S( 12,  36),   S( 26,  20),   S( 62,  11),   S( 99,   1),   S(121, -13),   S(106,   0),   S( 47,  23),   S( 46,   1),
            S( 11,  51),   S(-13,  35),   S(  1,  37),   S( 95, -19),   S( 22,  22),   S( -4,  29),   S( 75,  -4),   S( 56, -10),

            /* rooks: bucket 8 */
            S(-28, -39),   S(-18,  14),   S( -1,   5),   S(-25, -13),   S(-41, -58),   S(-41, -60),   S(-18, -22),   S(-24, -16),
            S(-13,  -8),   S( -7, -36),   S( -2,  -1),   S(  4, -30),   S(-25,  -7),   S(-12, -42),   S( -2, -15),   S(-19, -74),
            S(  9,  13),   S(  3,  -9),   S(-20,  -9),   S(-37, -11),   S(-32, -57),   S(-27, -46),   S(  8,  26),   S(  3, -16),
            S(-14, -13),   S(-21,  -2),   S( -8,   9),   S( -4,  -6),   S( 12,  32),   S(-14,  -4),   S( -1, -25),   S( -3,  14),
            S( -5, -10),   S(-14,  10),   S( -4,  55),   S(-10,   2),   S( -5,  -8),   S( -8,  -5),   S(  3,  -1),   S(-10, -31),
            S(  7,  29),   S(-26,   8),   S( 21,  30),   S( 22,   6),   S( -7,  -5),   S( 10,  15),   S(  1,   3),   S(  7,  54),
            S( -6,   2),   S( -3,   2),   S(  9,  15),   S(  4, -13),   S(  7, -15),   S( -2, -22),   S( 25,  -1),   S( -2, -22),
            S( -4, -116),  S(-12, -31),   S(  4, -10),   S(  5,   2),   S(-25, -51),   S(  4, -23),   S( 13,  21),   S( -3,  26),

            /* rooks: bucket 9 */
            S(-78, -51),   S( -5, -43),   S(-27, -55),   S(-47, -20),   S(-63, -25),   S(-33, -44),   S(-40, -57),   S(-58, -51),
            S(  9, -29),   S(-14, -31),   S(-38, -41),   S( -8, -17),   S(-26, -31),   S( 18, -10),   S(-23, -59),   S(-34, -67),
            S( -5, -15),   S( 18, -22),   S(  5,  -9),   S(-12, -11),   S(-32, -22),   S(  0, -24),   S( 16,   1),   S( -4, -35),
            S(  5,  -8),   S( -6,  -8),   S(-16, -10),   S(-14,   5),   S(-12,  -5),   S( -4, -21),   S( 10,  10),   S( -5,  -5),
            S( -5,   9),   S( -2,   9),   S( -5, -31),   S(  2,  20),   S(  3,   5),   S( 18,  -7),   S(  7,   6),   S( -5, -41),
            S( 13,  14),   S( -9, -13),   S( -1,  -2),   S(-21,  -9),   S(  9, -23),   S(  9,  -6),   S( 16,   4),   S(  4, -12),
            S( 31,   2),   S( 52,  -9),   S( 27, -14),   S( 35, -11),   S( 30, -21),   S( 36,   2),   S( 40,  -7),   S( 48,  -6),
            S( 37, -71),   S( 24, -23),   S( 20,   2),   S( 12,  25),   S( 18,  31),   S(  9, -10),   S( 21, -12),   S( 15,  -3),

            /* rooks: bucket 10 */
            S(-64, -111),  S(-30, -53),   S(-31, -52),   S(-42, -42),   S(-32, -24),   S(-48, -55),   S( 10, -48),   S(-23, -55),
            S(-32, -41),   S( -9, -25),   S(-14, -55),   S(-47, -12),   S( -5, -42),   S(-38, -15),   S( 11, -22),   S(  2, -28),
            S(-39, -60),   S(-21, -39),   S(-31, -43),   S(-18, -13),   S(-45,  -3),   S( -5, -15),   S(-12, -14),   S( -9, -19),
            S(-12, -22),   S(  0, -25),   S(-36,  -6),   S(-14, -28),   S( -2, -16),   S(-13, -16),   S( 26,  34),   S( -9, -44),
            S(  2, -15),   S( 16, -29),   S(-13, -15),   S(-12, -45),   S(-24, -23),   S( -6,  -6),   S( 16,  14),   S( -8,  -7),
            S( 28,  10),   S( 22,  23),   S(  8,   1),   S( -7, -11),   S(-22,   7),   S( -5,  -7),   S( 23, -12),   S(  9,  -8),
            S( 64, -12),   S( 79, -35),   S( 50, -15),   S( 56, -24),   S( 26, -12),   S( 15, -14),   S( 37, -10),   S( 25, -19),
            S( 41,  -7),   S( 12, -19),   S( 28, -16),   S( 16,  24),   S( 33,  -6),   S( 32,   7),   S( 27, -26),   S(  2, -36),

            /* rooks: bucket 11 */
            S(-83, -31),   S(-33, -31),   S(-46, -24),   S(-54, -71),   S(-23, -14),   S(-41,   6),   S(-18, -18),   S(-53,  -7),
            S(-53, -28),   S(-31, -48),   S(-19, -46),   S(-18, -28),   S(-34, -39),   S(-53, -21),   S(-24, -34),   S(-20,  -9),
            S(-47, -35),   S( -9, -24),   S(  2,  -2),   S(  1, -15),   S(-13, -23),   S( -7,   7),   S(-46, -39),   S(-29, -50),
            S(-17,  13),   S( -4, -21),   S( -1, -14),   S( 15,  30),   S( -5,  -4),   S(-14,  21),   S(  9,  10),   S( -8, -32),
            S(-25,  -3),   S(-11, -38),   S( 20,  -4),   S( 24, -12),   S( 23, -13),   S( 29,   6),   S( -6,  -3),   S(-20, -29),
            S( -5,  27),   S( 33,  17),   S(  8, -12),   S( 63,  -1),   S( 27,  14),   S( 17,  -7),   S(-19,  -4),   S(  1,  -1),
            S( 47,  34),   S( 39,   0),   S( 50, -21),   S( 59, -16),   S( 25, -12),   S( 26,  15),   S( 41,  13),   S( 54,  -6),
            S( 28,  28),   S(  4,  -3),   S(  6, -16),   S( 15, -19),   S( 11, -22),   S( 21, -10),   S( 17,  18),   S( 33,  -7),

            /* rooks: bucket 12 */
            S(  0, -64),   S( -5, -40),   S(-24, -49),   S(-24, -30),   S( -2, -30),   S( -7, -40),   S(-13, -49),   S( -3, -17),
            S( 12,  15),   S( -5, -20),   S( -6,  13),   S(-19, -46),   S( -4,   5),   S(-10, -26),   S(  5,  -3),   S( -9, -57),
            S(  8,   5),   S( -8, -33),   S( -8,  -5),   S( -5, -13),   S( -7, -31),   S(  4, -11),   S(  3,   2),   S(  0,  -3),
            S(-18, -29),   S( -1, -12),   S(  1,   4),   S( -1,  -5),   S(  4,  16),   S( -5, -32),   S(-17, -57),   S(-13, -45),
            S( -6,  -2),   S(-15, -28),   S( 17,  13),   S(  6,  -6),   S( -9, -33),   S(  4, -13),   S( -6, -29),   S(  9,  10),
            S( -4, -19),   S(  3, -38),   S(  9,  15),   S(  2, -28),   S(-14, -39),   S( -6, -23),   S(  1, -24),   S( -1, -15),
            S(-13, -39),   S(  4,  -8),   S(  6, -32),   S(  9, -25),   S( 12,   8),   S(-13, -68),   S( -5, -27),   S(  3,  -8),
            S(  0, -31),   S( 15,  18),   S(  7, -19),   S(  1, -30),   S(  0,  -9),   S( -5, -20),   S(-20, -59),   S(  8,  23),

            /* rooks: bucket 13 */
            S(-17, -53),   S( 11,  -9),   S(-11,  -4),   S( -8,  15),   S( -4, -14),   S(-18, -39),   S( -3, -26),   S(-16, -38),
            S( 13,  -2),   S(  0,   4),   S( -9,  17),   S(-13,  -9),   S(-11,  -8),   S( -8, -13),   S( -3, -11),   S(  1,  -8),
            S(-12, -41),   S( -8, -37),   S(-12, -18),   S(  1, -21),   S(  7,  25),   S(  0, -20),   S( -6, -29),   S( -1, -36),
            S(-17, -55),   S( -1,   5),   S(-27, -66),   S(-10,  -3),   S( 14,   4),   S(-17, -42),   S( -8, -37),   S( -2, -24),
            S(  7, -17),   S( -2,  -6),   S( 12,  -7),   S( -6, -35),   S(-10, -23),   S(  2,  -8),   S(-10, -57),   S(  2,  -7),
            S(-26, -43),   S(  2,  10),   S( -1, -31),   S(  5, -23),   S( 10,   0),   S( 11,  22),   S(  0, -11),   S(  1,  -5),
            S(-17, -17),   S(  6,  14),   S( -6, -14),   S( -7,  -5),   S(  5, -17),   S(  8,   8),   S( -8, -58),   S(  2,  -4),
            S(-19, -115),  S(-14, -53),   S(  7,   9),   S( -1, -15),   S( -6, -12),   S( -1, -21),   S(-13, -73),   S(  1,   2),

            /* rooks: bucket 14 */
            S( -1,   2),   S(-31, -70),   S( -5, -36),   S( -3,   0),   S( -4,  19),   S( -1, -15),   S(  6, -26),   S(-18, -48),
            S(-24, -41),   S(-13, -44),   S( -4,  -9),   S(-22, -46),   S(-12, -27),   S( -7, -25),   S(  1,   2),   S( 18,  25),
            S(-17, -36),   S( -1,  -3),   S(-19, -58),   S( -7, -21),   S(-13,  -9),   S(-18, -37),   S(  6,  41),   S(  8,  15),
            S( -3,  -5),   S(-19, -60),   S( -1,  -2),   S( -5, -10),   S(  2, -12),   S( -2,   7),   S(-16, -61),   S(-16, -23),
            S( -8, -46),   S(  1, -29),   S( -2, -34),   S(-11, -82),   S(  0, -37),   S(-14, -58),   S( -5, -43),   S( -7, -36),
            S( -1, -29),   S( -2, -20),   S( -1, -53),   S(  5, -51),   S( -4, -63),   S(  4, -36),   S( 13,   4),   S( -8, -31),
            S( 20,  -7),   S( -3, -37),   S(  2, -51),   S( -1, -67),   S(  6, -62),   S( -3, -41),   S( 18,  -7),   S(  2,  -8),
            S(-12, -27),   S(  7,   3),   S(-17, -75),   S(  4, -28),   S( -7, -37),   S( -7,  -1),   S( 13,  24),   S( -2, -14),

            /* rooks: bucket 15 */
            S(-17, -28),   S(-15, -52),   S( -3, -22),   S(-21, -37),   S(-10, -29),   S( -2,  19),   S( -9, -24),   S( -7, -10),
            S( -6,   4),   S(-25, -59),   S( -4,  -5),   S(-10, -14),   S(-17, -36),   S(  5,   9),   S(-19, -43),   S(  8,  23),
            S( -5, -18),   S(-12, -39),   S(-14, -49),   S(-14, -53),   S( 18,  14),   S(  3, -17),   S( 10,  21),   S( -7, -33),
            S( -9, -43),   S( -8, -49),   S(-13, -13),   S( -8, -38),   S( -2, -19),   S(-13, -50),   S( -3, -31),   S(-18, -19),
            S( -1, -23),   S(  0, -23),   S(  5, -19),   S(  4, -11),   S( -2, -32),   S(  6, -12),   S( -6, -39),   S( -2,  18),
            S(  4, -25),   S(  4,   2),   S(  2, -63),   S( -3, -59),   S( -3, -41),   S(  4, -70),   S(  5, -28),   S(-13, -25),
            S( 12,  42),   S( 24,  22),   S( 16, -15),   S(  0, -43),   S(  2, -29),   S( 21,  29),   S(  3, -28),   S( -1,  -3),
            S( -9, -26),   S( -6, -14),   S( -7, -52),   S(  5, -19),   S(  2, -44),   S(  8,  -7),   S(-11, -56),   S(  7,  16),

            /* queens: bucket 0 */
            S(-46, -31),   S(-47, -80),   S( 51, -110),  S( 60, -39),   S( 42, -66),   S( 40, -56),   S( 38,  16),   S(  9, -14),
            S( -4,  -9),   S( 37, -68),   S( 36,  -9),   S( 30,   2),   S( 28,  38),   S( 10,  28),   S( 13,  58),   S( 34,  -6),
            S(  4,   4),   S( 44,   4),   S( 32,  20),   S( 29,  14),   S( 28,   5),   S(  7,  57),   S(  4,  21),   S( 26,  31),
            S(  8,  29),   S( 16,  40),   S( 11,  46),   S(  4,  71),   S( 17,  44),   S(  3,  55),   S( 22,   8),   S(  2,  42),
            S( 31,  38),   S( 45,  27),   S( 11,  13),   S( 46,  58),   S(-16,  51),   S( -6,  -2),   S( 24,  28),   S( 49,   2),
            S( 25,  32),   S( 25,  60),   S( 39,  -3),   S( 24,   3),   S( 36,   3),   S( 16,  36),   S( 17,  40),   S(  9, -29),
            S( 72,  17),   S( 36,  30),   S( 20,  60),   S( 56,  27),   S( 16,  27),   S(-11, -12),   S( 22,  32),   S( -5,   9),
            S( 41,  51),   S( 18,  37),   S( 49,  39),   S( 55,  33),   S( 37,  38),   S(-11,   6),   S( 77,  19),   S( 52,   7),

            /* queens: bucket 1 */
            S(-18, -40),   S(-56, -45),   S(-72, -43),   S( -8, -76),   S(  6, -48),   S( -3, -72),   S( 31, -66),   S(  7,  17),
            S(  4, -31),   S(  6, -38),   S( 22, -65),   S(  3,  35),   S( 13,  -4),   S( 11, -10),   S( 34, -50),   S( -5,   7),
            S(-27,  17),   S(  7, -12),   S( 21, -10),   S(  5,   6),   S(  3,  62),   S( -9,  42),   S( 21,  13),   S( 31,  19),
            S( 16, -29),   S( -2,  23),   S(-25,  80),   S( 19,  43),   S(  8,  38),   S( 20,  -8),   S(-17,  32),   S( 11,  16),
            S( 27, -12),   S(-15,  38),   S( -3,  54),   S(-23,  87),   S( -8,  66),   S(  6,  48),   S(-10,  37),   S(  9,  23),
            S( 16,  31),   S( 25,  30),   S( 35,  47),   S(-41,  58),   S(-14,  87),   S(-41,  55),   S( 53,  17),   S( 11,  45),
            S( -2,  46),   S( 11,  70),   S(-28,  39),   S(-30, 102),   S(-25,  76),   S( 12,  13),   S(  0,  25),   S(-38,  59),
            S(-37,  -7),   S( -3,  57),   S( 22,  25),   S(-11,  18),   S( 14,  29),   S(  7,  31),   S( 14,  22),   S(-13,  12),

            /* queens: bucket 2 */
            S( 13,   4),   S( 25, -59),   S(  9, -44),   S(  6, -39),   S(-33,  13),   S(-15, -37),   S(-29, -45),   S( 19,  15),
            S( 13,  -3),   S( 26,  36),   S( 22,  -9),   S( 27, -22),   S( 24, -17),   S( 21, -44),   S( 21, -30),   S( 40, -29),
            S(  4,  17),   S( 19,   9),   S(  8,  35),   S( 22,  26),   S( 14,  73),   S( 25,  47),   S( 20,  10),   S( 34,  12),
            S(  9,   9),   S(-10,  55),   S(  4,  55),   S(  0,  70),   S( -1,  85),   S(-10, 112),   S( 12,  39),   S( 10,  46),
            S( 16,   5),   S(  0,  60),   S(-12,  68),   S(-29, 122),   S(-33, 124),   S(-10,  92),   S( -1, 115),   S( -6,  96),
            S( 10,  16),   S(  2,  37),   S(-16,  73),   S( -4,  57),   S(-15, 106),   S( -7, 109),   S(-21,  95),   S( -8, 100),
            S(-12,  73),   S(-52, 112),   S(-15,  81),   S( -3,  70),   S(-16, 111),   S( 49,  72),   S(-41,  58),   S(-35, 107),
            S(-62, 109),   S( 16,  48),   S( 45,  35),   S( 35,  63),   S( 24,  55),   S( 38,  30),   S( 23,  56),   S(-48,  59),

            /* queens: bucket 3 */
            S( 88,  77),   S( 57,  94),   S( 58, 100),   S( 52,  86),   S( 73,  30),   S( 44,  26),   S( 25,  19),   S( 43,  50),
            S( 66, 112),   S( 67, 107),   S( 54, 111),   S( 57,  81),   S( 59,  82),   S( 62,  58),   S( 76,   0),   S( 37,  47),
            S( 49,  96),   S( 59, 101),   S( 59,  90),   S( 67,  63),   S( 59, 100),   S( 66,  92),   S( 71,  92),   S( 73,  37),
            S( 47, 130),   S( 56,  88),   S( 59,  89),   S( 45, 108),   S( 56,  96),   S( 43, 132),   S( 58,  90),   S( 49, 135),
            S( 59, 104),   S( 68, 107),   S( 43, 103),   S( 35, 115),   S( 26, 146),   S( 30, 130),   S( 40, 162),   S( 48, 144),
            S( 39, 132),   S( 44, 128),   S( 42, 124),   S( 23, 141),   S( 45, 141),   S( 91, 106),   S( 52, 150),   S( 30, 183),
            S( 54, 130),   S( 46, 116),   S( 96,  91),   S( 58, 113),   S( 40, 127),   S( 53, 130),   S( 57, 148),   S(143,  49),
            S( 85,  86),   S( 99, 102),   S( 56, 111),   S( 85,  98),   S( 33, 118),   S(100,  72),   S(145,  53),   S(126,  59),

            /* queens: bucket 4 */
            S( 10, -16),   S(  4,  -7),   S(-44,  -2),   S(-23, -27),   S(  2, -19),   S( 13, -18),   S(-47, -31),   S(-19,  -7),
            S(-30, -26),   S(-44, -33),   S( 14, -26),   S(-45,   3),   S(  2, -43),   S( -3, -34),   S( -1,   2),   S(-41,  -8),
            S(  2,  -1),   S(-10,  -8),   S(  0,  38),   S( 20,  29),   S( 11, -20),   S( 21,   0),   S( 12,  -7),   S(-22, -35),
            S(-11,   7),   S( 21,  10),   S( -8,  27),   S( 10,  61),   S( 39,  50),   S( 21,  33),   S( 32,  -9),   S(-13, -10),
            S(-11,  39),   S( 32,  22),   S( 17,  19),   S( 43,  48),   S( 24,  33),   S( 29,   2),   S(-25, -29),   S(-23, -27),
            S( 12,  21),   S( 48,  21),   S( 36,  62),   S( 39,  46),   S(  2,  -2),   S(-15, -10),   S( -9,   1),   S(-27,  -6),
            S(-32,  -6),   S(-23,  10),   S( 17,  13),   S( 37,  50),   S( 26,  21),   S( -4,  19),   S(-11, -28),   S( -7, -26),
            S(-22, -38),   S( -3, -12),   S( 30,  44),   S(  8,  20),   S(-24, -16),   S( -7, -13),   S(-28, -43),   S(  2, -16),

            /* queens: bucket 5 */
            S(-55, -40),   S(-36, -45),   S( -5, -34),   S(-45, -22),   S(-56, -29),   S(-13, -17),   S( -6, -19),   S(  7,  23),
            S(-47, -15),   S(-71, -26),   S(-38, -25),   S(-57,  -3),   S(-44, -26),   S(-57, -28),   S(-39,  -5),   S(-39,  -8),
            S(-45, -22),   S(-79, -15),   S(-57,   1),   S(-42,  16),   S( 47,  57),   S( -7,  11),   S(  7, -19),   S(-10,   8),
            S(-67, -24),   S(-60, -24),   S( 18,  43),   S( 39,  60),   S(  2,  22),   S( 13,  14),   S( -9, -27),   S(-18,  10),
            S(-42,   2),   S( -4,  30),   S(  3,  60),   S( -2,  74),   S( 41,  78),   S(  5,  24),   S(  9,   8),   S(-21, -15),
            S(-28,  11),   S( 30,  51),   S(-13,  46),   S(  7,  39),   S( 60,  49),   S( 31,  54),   S( 28,  16),   S(  7,  -8),
            S( -7,  16),   S(  7,  25),   S(-28,  29),   S( 21,  55),   S( 16,  44),   S( 21,  41),   S( 48,  45),   S(-28, -12),
            S( -8,  -8),   S( -2,  -7),   S( 23,  27),   S(  5,  51),   S( 17,  22),   S( -3,   6),   S(  2, -16),   S(-42, -48),

            /* queens: bucket 6 */
            S(-23,  -6),   S(-42, -35),   S(-41, -21),   S(-82, -54),   S(-53, -28),   S(-37, -26),   S(-29, -30),   S(-39, -18),
            S(-65,   5),   S( -6,   9),   S(-54,   2),   S(-20,  15),   S(-68,  35),   S(-86,  -9),   S(-80, -28),   S(-17, -24),
            S(-33, -19),   S(-24,  -6),   S(-28,  28),   S(-103,  84),  S(-20,  55),   S(-51,  -8),   S(-30,  12),   S( 13,  26),
            S(-43,  33),   S(-34,  -1),   S( -5,  60),   S(-11,  59),   S( 22,  73),   S( 45,  54),   S( 13,   5),   S( 12,   7),
            S(-67,  34),   S(  2,  44),   S(-26,  63),   S( 26,  47),   S( 62,  81),   S( 79,  53),   S( 34,  23),   S(  2,  47),
            S(-40,  49),   S(  0,  11),   S( 34,  24),   S( 26,  59),   S( 54,  77),   S( 46,  94),   S( -6,  27),   S(-30,  14),
            S(  4,  36),   S(-23,  -3),   S(  7,  54),   S(-10,  28),   S( 54,  68),   S( 34,  72),   S(-31,  31),   S(-43,   0),
            S( 26,  28),   S( 37,  52),   S( 35,  28),   S(-11,  16),   S( 33,  48),   S( 42,  69),   S(-13,   8),   S( -4,  -2),

            /* queens: bucket 7 */
            S(-11, -15),   S(-22,  14),   S(-65,  19),   S(-26,  20),   S(-42,  -5),   S(-54,  -8),   S( -3,   7),   S(-18,   0),
            S(-48,   4),   S(-23,   7),   S(-32,  40),   S(  2,  17),   S(-17,  23),   S(-46,  32),   S(-77,  41),   S(-30, -26),
            S(-41,   1),   S(-33,  49),   S(-18,  34),   S(-23,  36),   S( 24,  36),   S( 17,  20),   S( -3,  24),   S(-18,   7),
            S(-58, -22),   S(  3,  31),   S(-11,  41),   S( 25,  22),   S( 48,  30),   S( 48,  18),   S(  4,  48),   S( 14,   2),
            S(-16,  16),   S(-65,  39),   S(  1,  25),   S( 11,  30),   S( 51,  21),   S(101,  -6),   S(  5,  39),   S( 36,   3),
            S(-24,  43),   S(  4,  36),   S(-35,  15),   S( 26,  14),   S( 51,  47),   S( 83,  42),   S( 56,  10),   S( 48,  26),
            S( 32,   5),   S( 34,   0),   S( 20,   7),   S( 35,   5),   S( 33,  24),   S( 64,  37),   S( 40,   7),   S( 67,  22),
            S(  5,  23),   S( 29,  17),   S( 32,  19),   S( 41,  40),   S( 52,  42),   S( 35,  37),   S( 16, -10),   S( 42,  41),

            /* queens: bucket 8 */
            S( -1, -11),   S( -4,  -7),   S(-20, -11),   S(  0,   3),   S( -8,  -9),   S(-11, -29),   S(-14, -17),   S(-10,  -5),
            S(  3,  15),   S(-14, -23),   S(  8,  14),   S(-20,   6),   S(-12, -17),   S(-11, -30),   S(-14, -37),   S( -4,  -4),
            S( -1,   3),   S( -6,  -4),   S( -7,   9),   S(  3,  13),   S(-34, -44),   S(-22, -35),   S(-17, -41),   S(-15, -26),
            S(  0,  10),   S( 11,  22),   S(  4,  10),   S( -2,  14),   S( 14,  20),   S(-28, -34),   S( -3,  -1),   S(  2, -17),
            S( 15,  33),   S( 16,  42),   S( 13,  60),   S(  3,  21),   S( 19,  37),   S( -5,  -6),   S( -6, -15),   S( -9, -20),
            S(  9,  21),   S(  0,  16),   S(-21,  15),   S( 10,  30),   S(-21, -38),   S( -8, -11),   S( -6, -12),   S( -3,  -5),
            S(-12, -22),   S(-23, -21),   S( -3,  14),   S( 18,  37),   S( -5,   0),   S( -9, -20),   S( -3,  -5),   S(-13, -33),
            S(-20, -27),   S( -7,  -7),   S(-18, -30),   S(-10, -21),   S(-11, -25),   S(-10, -15),   S(-11, -40),   S( -3,  -7),

            /* queens: bucket 9 */
            S(  0,  -4),   S(-12, -28),   S( -3,  -5),   S(-27, -43),   S(-30, -41),   S( -7, -20),   S(-19, -37),   S(-20, -34),
            S( -5, -13),   S(-13, -17),   S(-29, -37),   S( -4,  -9),   S(-37, -44),   S(-12, -12),   S( -4, -25),   S( -5, -12),
            S(  6,  12),   S( -3,  14),   S(-34, -10),   S(-14, -20),   S(-20, -38),   S(-15, -12),   S(-19, -26),   S(-11, -18),
            S(-25, -39),   S(-11, -11),   S(-15,   6),   S(-16, -17),   S( 18,  36),   S( -4,  -5),   S(-23, -44),   S(-18, -34),
            S(  8,  16),   S(  7,  34),   S( 15,  47),   S( 10,  45),   S(  1,  28),   S( -8, -16),   S(  0,  -9),   S(  2,  14),
            S(-19, -33),   S(-30, -29),   S(-13,   2),   S(  6,  23),   S( -5,   2),   S( -7,  -4),   S(-12, -26),   S( -6,  -8),
            S(-20, -31),   S(-10, -25),   S(-39, -17),   S( 21,  38),   S(-12, -27),   S( -8, -30),   S(  8,  20),   S(-10, -14),
            S(-13, -17),   S( -6, -21),   S(  0,   0),   S(  8,   4),   S( 12,  13),   S(-14, -19),   S(  2,   0),   S( -8, -11),

            /* queens: bucket 10 */
            S(  0,   6),   S(-14, -23),   S(-13, -29),   S(-29, -32),   S(-12, -16),   S( -9,  -3),   S(-10, -25),   S( -8, -25),
            S(-12, -31),   S(-19, -34),   S(-11, -24),   S(-24, -34),   S(-11, -19),   S( -7, -12),   S( -8, -20),   S(-27, -28),
            S( -3,  -6),   S(-22, -40),   S(-18, -10),   S(-17, -16),   S(-21,   1),   S(-16,   2),   S( -6, -12),   S( -2,  -4),
            S( -8, -12),   S( -4,   2),   S(-16, -11),   S( -8,  -1),   S( -4,  22),   S(-28, -15),   S( -6,  -9),   S(-25, -26),
            S(-11, -15),   S( -2,  -7),   S(-31, -27),   S( 17,  40),   S(-17, -22),   S( -3,  11),   S( 13,  10),   S(-10, -11),
            S(-13, -12),   S(-33, -57),   S(-15, -17),   S( -8,  17),   S(  5,  58),   S(  2,  12),   S(-10, -30),   S( -9,  -7),
            S( -7, -16),   S( -9,  -5),   S( 12,  28),   S(-11,  -5),   S( 14,  20),   S( -1,  20),   S(-12, -35),   S( -6, -13),
            S( -2,   8),   S(-15, -30),   S( -8, -22),   S( -1,   8),   S(  5,  -4),   S( -2,  -1),   S(  4,  16),   S(-17, -33),

            /* queens: bucket 11 */
            S(-14, -37),   S(-10, -26),   S(  0, -14),   S( -3, -28),   S(-12, -20),   S(-15, -11),   S(-20, -29),   S(-24, -43),
            S(-19, -31),   S(-18, -22),   S(-30, -57),   S(-18, -39),   S(  5,   8),   S(-15, -12),   S(  2,   4),   S(  1,   9),
            S(-18, -33),   S(-19, -35),   S(-21, -28),   S(-29, -46),   S(-17, -24),   S(-22, -19),   S(  6,  29),   S( -7,  -6),
            S(-13, -29),   S(-21, -18),   S(  6,   8),   S( 18,  38),   S( -8,  -3),   S(-18,  -6),   S( 22,  38),   S( -7, -21),
            S(-20, -27),   S( -1,  -6),   S(-32, -45),   S( 19,  33),   S( 10,  -7),   S( 48,  81),   S( 24,  50),   S( -1,   4),
            S(  0,  -1),   S( -8, -17),   S(-13, -22),   S(  2,  12),   S(-10, -18),   S( 32,  53),   S( -1, -17),   S(-11,  -7),
            S(-13, -17),   S(-14, -17),   S( 28,  41),   S( -1,   0),   S(-15,  -4),   S( 31,  55),   S( 38,  43),   S(-11, -28),
            S( -5, -15),   S(-16, -30),   S(-18, -32),   S( 14,  17),   S(  5,  15),   S( -1, -42),   S(  0,   6),   S(-11, -41),

            /* queens: bucket 12 */
            S(  7,   6),   S( -5,  -9),   S( -8, -12),   S( -6, -11),   S( -6,  -6),   S(  0,  -1),   S(  1,   0),   S(-13, -26),
            S( -2,  -5),   S( -3,  -5),   S(-17, -31),   S( -9, -20),   S( -8, -19),   S( -8, -11),   S( -4, -10),   S( -4, -12),
            S(  3,   1),   S( -4, -10),   S(  9,   9),   S( -3,  -8),   S( -1,  -6),   S(-11, -19),   S(-16, -42),   S(-10, -20),
            S(  0,   1),   S(  2,   9),   S(  4,  19),   S( 20,  41),   S(  8,  17),   S( -3, -13),   S(  1,   7),   S( -9, -30),
            S( -3,  -8),   S( 20,  28),   S( 32,  53),   S( 11,  25),   S(  4,  12),   S(  1,  -1),   S(-15, -30),   S(  0,   3),
            S(  5,  10),   S(  0,  -3),   S( 30,  53),   S(  1,  -5),   S( 12,  17),   S( -4,  -3),   S(  5,  14),   S( -5, -12),
            S(  3,   5),   S( 10,   9),   S( 14,  29),   S(  2,  10),   S( -2,  -3),   S(  3,   5),   S(  5,   8),   S( -7, -15),
            S( -5, -20),   S(-21, -50),   S(-18, -13),   S(-11, -29),   S( -7,  -4),   S( -8, -13),   S(  2,  -1),   S(-15, -23),

            /* queens: bucket 13 */
            S(  0,  -2),   S( -1,  -5),   S(  1,  -3),   S(-14, -21),   S(-12, -28),   S(  0,  -6),   S(-15, -31),   S( -5,  -8),
            S(  0,   4),   S(  0,  -2),   S(  1,   3),   S( -7, -16),   S(-18, -34),   S(  1,   4),   S(  3,   5),   S(-17, -34),
            S( -6, -14),   S( -4, -14),   S( -9, -22),   S( -4, -13),   S(-13, -26),   S(-10, -14),   S(  0,   0),   S(-20, -38),
            S( -3,  -4),   S(  3,  -5),   S(  7,   5),   S( 14,  11),   S( 19,  30),   S( -1, -12),   S( -8, -25),   S( -1,  -5),
            S( -9, -10),   S(  0,  18),   S( 18,  27),   S(  2,   4),   S( 26,  49),   S(  3,  -7),   S( -7, -21),   S( -5, -11),
            S( -2, -10),   S(  8,  23),   S( 37,  72),   S( 28,  55),   S( -7,   4),   S(  0,   2),   S(  0,   1),   S(-13, -31),
            S( -5, -14),   S(  6,  14),   S( -3,  11),   S(  5,  32),   S( -3,  -6),   S(  0,  -5),   S( -1,  -3),   S(  7,  16),
            S(-14, -32),   S( -8, -18),   S( -4,  -8),   S(-16, -37),   S(  0,  -8),   S(  6,  10),   S(-18, -32),   S( -6, -16),

            /* queens: bucket 14 */
            S(  0,   2),   S( -5, -17),   S( -3, -10),   S(  1,  -4),   S( -2,  -6),   S( -5, -15),   S(-10, -20),   S(-11, -28),
            S( -9, -19),   S(  6,  11),   S( -5, -14),   S( -1,  -5),   S(-14, -34),   S( -9, -25),   S( -1,  -6),   S(-15, -38),
            S(  7,   8),   S(-11, -30),   S( -6,  -2),   S( -3, -12),   S( -2,  -6),   S( -5, -18),   S( -4,  -8),   S( -9,  -8),
            S( -5, -13),   S(  1,  -5),   S( -8, -11),   S( 11,  29),   S( -1,   7),   S(  0,   9),   S(  8,  12),   S(  1,  -2),
            S( 14,  28),   S(  2,   3),   S(-12,  -7),   S( 16,  13),   S( 19,  27),   S( 11,  13),   S( 12,  18),   S(-10, -25),
            S( -7, -13),   S(  9,  13),   S( 19,  38),   S(  5,  13),   S( 17,  31),   S( 11,  22),   S( 12,  20),   S(-10, -18),
            S( -7, -11),   S(  7,   2),   S( 10,  20),   S(  9,  26),   S(  1,   6),   S( 12,  27),   S( 17,  28),   S(  6,  10),
            S( -3,   0),   S(  0,  -3),   S(  2,   2),   S(  0,  -7),   S(  1,  -2),   S(  2,  -8),   S( -9, -11),   S(-15, -31),

            /* queens: bucket 15 */
            S( -1,  -7),   S(  1,  -5),   S( -2,  -6),   S( -5, -15),   S( -1,   2),   S(  5,  11),   S( -9, -22),   S( -5, -16),
            S( -6, -16),   S( -5, -15),   S( -9, -17),   S( -4, -14),   S( -8, -12),   S( -2,   0),   S( 11,  25),   S( -3, -10),
            S(  1,  -3),   S( -1,  -5),   S( -2,  -5),   S( -2,  -5),   S(  1,  -2),   S(  1,   0),   S( -7, -13),   S( -1,  -7),
            S( -7, -19),   S(  2,  -1),   S( -2,  -1),   S(  6,   4),   S(  2,   1),   S(-10,  -9),   S(  9,  17),   S(  4,   4),
            S( -2,  -2),   S( -2, -10),   S( -6, -18),   S( -3, -12),   S(  3,   5),   S(  7,  -1),   S( -3, -12),   S( -8, -14),
            S(  3,   0),   S( -2,   0),   S(  1,  -1),   S( -4,  -9),   S( -4, -18),   S( 34,  62),   S( 10,  11),   S(  0,   3),
            S(-10, -18),   S(  0, -17),   S(  3,  -3),   S(  6,   7),   S(  7,   8),   S( 25,  45),   S( -4, -16),   S(  7,  11),
            S( -4,  -8),   S( -4, -12),   S( -5, -12),   S( 14,  17),   S(  5,   4),   S(  5,  -3),   S( -5, -15),   S(-12, -27),

            /* kings: bucket 0 */
            S( 17,  54),   S(  9,  70),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,  62),   S( 91,  68),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-23,  19),   S(-83,  32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 28,  33),   S(  4,  42),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-49,  39),   S(-45,  32),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 30,  43),   S( 27,  37),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 17,  61),   S( -5,  41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 64,  84),   S( 13,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-19, -39),   S( 36, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -16),   S(  8,  15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 15, -54),   S(-48, -29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -21),   S(-18, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-12, -14),   S(-31, -10),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  2,  -3),   S(-12,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 36,  10),   S(-10, -22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 33,  34),   S(-46,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-92, -41),   S( -9, -10),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -49),   S( 19, -13),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -2, -43),   S(-25, -55),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 58, -56),   S( 36, -45),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -8, -53),   S(-45, -33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 65, -61),   S( 58, -45),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  9, -25),   S(-91, -55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 71, -35),   S( 12, -53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -48),   S( 65, -31),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -102),  S(-21, -31),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 10, -30),   S( 70, -38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 15, -55),   S( 23, -68),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 69, -47),   S( 30, -39),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 28, -64),   S( -6, -61),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 56, -41),   S(-40, -60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 16, -58),   S(-28, -113),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -12),   S(-42,  12),   S(-11,   0),   S(-11,   4),   S(  7,   0),   S( 24,   9),   S( 24,   0),   S( 45,  -6),
            S(-14, -25),   S(-35, -11),   S(-17, -12),   S(-17,   6),   S(  7,   6),   S( -7,   6),   S( 22,  -8),   S( 18,  11),
            S( 11, -16),   S( -7, -21),   S( 25, -32),   S(  5, -17),   S( 18,  -6),   S( 18,  16),   S( 11,  31),   S( 53,   7),
            S( 23, -20),   S( 41,  -4),   S( 59, -25),   S( 34,  -1),   S( 10,  46),   S(-13,  74),   S( 10,  72),   S( 76,  38),
            S( 81, -32),   S(106,  -7),   S( 84, -22),   S( 51,   6),   S( 37, 135),   S( 33,  73),   S(  6, 130),   S( 86,  98),
            S(-150, -106), S(-158, -108), S( 65, -168),  S( 51,  48),   S( 86, 160),   S( 54, 185),   S(180, 119),   S( 65, 157),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  21),   S(-59,  17),   S(-37,   2),   S(-51,  58),   S(-27,   1),   S( -4,   7),   S( -5,  -1),   S(  1,  21),
            S(-50,   0),   S(-45,  -5),   S(-47,  -1),   S(-32,  10),   S( -4,  -3),   S(-24,   1),   S( -8,  -8),   S(-23,  11),
            S(-30,  12),   S(-21,   8),   S(-27,   1),   S(  0, -14),   S(  2,  16),   S(-12,   8),   S(-25,  20),   S( 12,  10),
            S(-17,  30),   S( 23,   7),   S( -7,  13),   S( 13,  25),   S( 12,  22),   S(-23,  30),   S(  3,  23),   S( 52,  34),
            S( 31,  16),   S( 80, -22),   S(111, -37),   S( 86, -26),   S( 63,  -4),   S(  5,  36),   S(-40,  61),   S( 37,  80),
            S(166, -43),   S(  7, -35),   S( 46, -131),  S( 52, -109),  S( -9, -73),   S(-31,  47),   S( 65, 128),   S( 75, 160),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  36),   S(-48,  19),   S(-39,  10),   S(-18,  17),   S(-56,  41),   S(-31,  14),   S(-22,  -7),   S(-26,  24),
            S(-51,  14),   S(-44,  12),   S(-49,   6),   S(-44,  18),   S(-42,  17),   S(-47,   4),   S(-24, -12),   S(-56,  13),
            S(-26,  37),   S(-43,  41),   S(-22,  20),   S(-22,  16),   S(-32,  28),   S(-16,   3),   S(-30,   7),   S(-18,   4),
            S(-10,  73),   S(-27,  58),   S( -9,  34),   S( -1,  32),   S( -8,  30),   S(-23,  14),   S(  0,  11),   S( 32,   3),
            S(-22, 120),   S(-44,  98),   S(-21,  29),   S( 33, -27),   S(113, -25),   S(101,  -6),   S( 94, -34),   S( 41,   1),
            S(  2, 203),   S( 62, 118),   S(-16,  73),   S(-11, -72),   S(-16, -168),  S(-28, -148),  S( 17, -66),   S(112, -58),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9,  22),   S(-17,  17),   S(-14,  19),   S( -7,  41),   S(-26,  51),   S(  8,  37),   S( -6,  10),   S(-11,  -1),
            S( -1,   8),   S( -1,  11),   S(-17,  11),   S(-15,  17),   S(  0,  23),   S(  3,   6),   S( -4,  -3),   S(-37,   1),
            S( 19,  35),   S(-11,  51),   S(  1,  25),   S( -3,   8),   S( 17,   0),   S( 21,  -4),   S( -3,  -8),   S( -4,  -7),
            S( 18,  85),   S( -6,  94),   S(  9,  71),   S( 13,  33),   S( 30,   7),   S( 26, -13),   S( 13,   9),   S( 44, -18),
            S(  9, 155),   S(-23, 166),   S(-40, 173),   S(-14, 122),   S( 35,  64),   S( 68,   9),   S( 83, -23),   S( 80, -19),
            S( 65, 143),   S( 41, 221),   S( 14, 247),   S( 18, 176),   S(-13,  84),   S( 34, -150),  S(-119, -176), S(-152, -167),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 74, -13),   S( 14, -10),   S(  8,  -7),   S(-16, -33),   S( 26, -19),   S(  5, -16),   S( 11, -14),   S(-45,  27),
            S( 64, -28),   S( 15,  -5),   S( 13, -20),   S(-27,   2),   S(-19, -30),   S(  0, -37),   S(-46, -17),   S(-37,  -9),
            S( 90, -29),   S(106, -32),   S( 38, -26),   S(-34,  -8),   S(-67,  14),   S(-30,  10),   S(-66,  17),   S(-46,  21),
            S(  3, -94),   S( 46, -113),  S( 43, -27),   S(-25,  21),   S(-12,  13),   S(-19,  40),   S(  8,  36),   S(  4,  36),
            S( 52, -89),   S(-31, -123),  S( -6, -87),   S( 55,  31),   S( 74,  88),   S( 36,  78),   S(-13,  64),   S( -5,  83),
            S(  6, -44),   S( -8, -76),   S( 25, -74),   S(  4,  -7),   S( 50, 107),   S( 64, 157),   S( 46, 125),   S( 69, 119),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-80,  39),   S(-26,  20),   S(  8,   0),   S( 46,  12),   S( 55,  -8),   S( 20, -10),   S(-26,   3),   S(-63,  41),
            S(-77,  17),   S( -1,   8),   S( 16, -13),   S(-14,  11),   S( -2,   6),   S( -6, -11),   S(-32, -12),   S(-64,  16),
            S(-25,  12),   S( 30,  13),   S( 77,  -2),   S( 20,  22),   S(-22,  33),   S(-22,   7),   S(-16,   7),   S(-32,  27),
            S( 20,  14),   S( 36, -16),   S(-11, -45),   S(-27, -13),   S(  2, -13),   S(  5,   2),   S( 37,   0),   S( 32,  23),
            S(104, -12),   S( 57, -63),   S( 44, -82),   S(-16, -63),   S( 84, -50),   S( 53,  10),   S( 65,  17),   S(  8,  84),
            S( 52,  15),   S( 43, -19),   S( 15, -91),   S( 34, -99),   S(-33, -57),   S( 81,  16),   S( 44,  91),   S( 75,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-84,  27),   S(-47,  -4),   S(  7, -12),   S(-24,  12),   S( 27,   0),   S( 20,   3),   S( 17,  -1),   S(  5,  17),
            S(-74,   1),   S(-38, -12),   S(-19, -19),   S( 30,  -7),   S(-14,  17),   S( -4,  -5),   S( 12,  -9),   S(-12,   0),
            S(-53,  26),   S(-32,   7),   S(  1,   4),   S( 11,  12),   S( 22,  26),   S( 45,  -1),   S( 49,  -5),   S( 21,  -1),
            S(-42,  60),   S( -7,  26),   S( 30,   4),   S( 48, -11),   S(-12, -29),   S( 30, -46),   S( 54, -32),   S( 95, -34),
            S( 15,  76),   S( 26,  23),   S( 24,   1),   S( 23, -43),   S(-12, -65),   S(-16, -56),   S( 75, -71),   S(114, -24),
            S(110,  52),   S(123,  41),   S( 80,  14),   S( 52, -50),   S(-11, -109),  S( -4, -87),   S(  8, -45),   S( 85,   9),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,   0),   S(-57, -14),   S( -2, -31),   S(-33,  37),   S( 34,   0),   S( 64, -14),   S( 56, -28),   S( 56,  -7),
            S(-51, -15),   S(-57, -14),   S(-28, -28),   S(-22,  -6),   S( 14, -12),   S( 51, -33),   S( 40, -23),   S( 46, -23),
            S(-29,   9),   S(-56,  17),   S(-29,   4),   S(-32,  -9),   S( 22, -12),   S( 47, -26),   S( 75, -24),   S( 63, -18),
            S( -9,  36),   S(-64,  60),   S(-29,  46),   S( -5,  23),   S(  6, -10),   S( 60, -56),   S( 35, -72),   S( 57, -106),
            S( 19,  54),   S(-42, 111),   S( 12,  93),   S(  6,  69),   S( 41,   7),   S( -1, -56),   S(-28, -105),  S( 18, -103),
            S(151,  66),   S( 99,  98),   S( 91,  94),   S( 47,  88),   S( 73, -11),   S(-25, -113),  S(-21, -89),   S( 22, -153),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  -4),   S(  8, -19),   S( 27,  10),   S(-14, -32),   S( -9, -56),   S( 52, -53),   S( 41, -66),   S( 50, -41),
            S(  5, -56),   S(-31,  -3),   S(-42, -60),   S(-33, -32),   S(-84, -33),   S( 38, -47),   S(  1, -47),   S(-17, -34),
            S(-44, -63),   S( 49, -54),   S( 18, -50),   S(-35, -52),   S(-37,  -4),   S(-33, -19),   S(-72,   1),   S(-77,  10),
            S(-22, -17),   S(  1, -41),   S( 21, -35),   S( 28, -19),   S(  3,  20),   S(-29,  32),   S(-12,  32),   S( -6,  33),
            S(  5,  12),   S(  6, -22),   S( 17,  20),   S( 29,  69),   S( 67, 137),   S( 34,  89),   S( 16,  89),   S( 14,  99),
            S( 37,  88),   S(  8,  27),   S( 24,  84),   S( 40,  98),   S( 34,  84),   S( 46, 176),   S( 38,  91),   S(  7,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 31,  -6),   S( 59,  -6),   S( 38,  -4),   S(  0,  -1),   S( 13,  -1),   S( 76, -40),   S( 81, -63),   S(  5, -39),
            S( -2, -45),   S(-26, -47),   S(-44, -34),   S(-25, -43),   S(-21, -33),   S(-18, -37),   S(  9, -55),   S(  9, -46),
            S(-96, -18),   S(-16, -55),   S( 35, -74),   S(-59, -49),   S( 14, -38),   S( 16, -52),   S(-61, -36),   S(-40, -13),
            S(-22,   1),   S(-46, -44),   S(  0, -58),   S(-39, -30),   S( -8, -36),   S(-19, -15),   S(  6,  -7),   S( 24,  -1),
            S(  0,  19),   S( -4, -13),   S( 23, -21),   S( 36,  12),   S( -5,  54),   S( 21,  36),   S( -5,  55),   S( 10,  36),
            S(-12,  37),   S( 21,  65),   S( 15,  31),   S( 25,  -3),   S( 32,  72),   S(  0,  40),   S(  8,  48),   S(  0,  74),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -59),   S( -4, -54),   S( 23, -28),   S(-25, -59),   S( 44, -39),   S(130, -14),   S( 73, -21),   S( 53, -25),
            S(-30, -60),   S(-57, -59),   S( 56, -73),   S( 11, -49),   S( 22, -38),   S( 32, -36),   S( 10, -47),   S( 22, -42),
            S(-46, -28),   S(-47, -40),   S(-30, -37),   S( 22, -54),   S( -7, -39),   S(-24, -61),   S( 33, -77),   S( 36, -49),
            S( -9, -14),   S(-10, -10),   S(-26, -37),   S(-18, -41),   S(-29, -34),   S(-22, -40),   S(  2, -45),   S(-11, -13),
            S(  5,   1),   S( 27,  27),   S( 27,  13),   S( 13, -22),   S( 21,  11),   S( 26,  -7),   S(-29, -28),   S(  8,   1),
            S(-18,  24),   S(  9,  26),   S( 18,  42),   S( 20,  19),   S( 29,  34),   S( -1,   0),   S( -2,  11),   S( 14,  16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-68, -39),   S( -4, -41),   S( -1, -43),   S(  6, -10),   S( 11, -17),   S( 90,  -5),   S(117, -37),   S( 67,  -5),
            S(-66, -55),   S(-50, -62),   S(-42, -57),   S( -8, -60),   S( -6, -30),   S( -5, -46),   S( -8, -31),   S( 43, -65),
            S(-52, -12),   S( -9, -42),   S(-25, -17),   S( 23, -34),   S(-60, -31),   S( 37, -45),   S(-24, -81),   S(  2, -57),
            S(  6,  -2),   S(-28,  21),   S( 41,  28),   S( 54, -15),   S(  6, -18),   S(-14, -24),   S(-20, -66),   S(-41, -36),
            S(  8,   6),   S( 12,  55),   S( -3,  60),   S( 41,  32),   S( 48,  52),   S( -2,  -5),   S(  8,   2),   S( -7, -11),
            S( 16,  53),   S( 39,  59),   S( 33,  88),   S( 48, 120),   S( 32,  55),   S( 49,  97),   S( 10,  35),   S(  7,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -94),   S( -6,   9),   S( -6,  20),   S( -1,  -1),   S(  5,   8),   S(-29, -57),   S( 23, -19),   S(  1, -22),
            S( -3, -29),   S( -5,   8),   S(-37, -69),   S(-36, -53),   S(-42, -63),   S( -2, -47),   S(-16, -49),   S( -4, -47),
            S(-16,   3),   S(  9, -56),   S(-25, -103),  S(-29, -73),   S(-19, -26),   S( -7,   5),   S(-56, -13),   S(-45, -18),
            S( -6,   6),   S(  2,  -1),   S( -4, -17),   S( 15,  16),   S( 18,  49),   S(  6,  84),   S(-33,  32),   S(-36,  27),
            S( 15,  26),   S(  8,  21),   S( 19,  43),   S( 19,  68),   S( 36, 132),   S( 26,  98),   S( 10, 137),   S(  1, 104),
            S( 21,  14),   S(  9,  27),   S( 10,  26),   S( 18,  61),   S( 22, 106),   S( 31, 115),   S(-19,  28),   S(-23,  49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -63),   S(-36, -72),   S( -1,  -8),   S( -5, -21),   S(  2, -27),   S(-14, -35),   S(-12, -50),   S(-20, -51),
            S(-74, -51),   S(-23, -58),   S(-35, -63),   S( 16,  17),   S(-33, -50),   S(-32, -28),   S(-35, -65),   S(-16, -72),
            S(-38, -23),   S(-10, -59),   S(-15, -66),   S(-20, -58),   S(-15, -55),   S( -9, -54),   S(-35, -45),   S(-17, -26),
            S(-23,  19),   S( -7, -20),   S( 24,   7),   S(-13,   1),   S(  3,  37),   S(-32,  44),   S(-12,  10),   S(-21,  28),
            S(-13,  23),   S( -2,   1),   S(  5,  -7),   S( 26,  60),   S( 24,  64),   S( 27,  78),   S( 15,  97),   S(-25,  78),
            S( 14,  94),   S( 30,  79),   S(  3,  11),   S( 20,  51),   S( 23,  81),   S(  3,  49),   S( -3,  52),   S( -7,  72),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34, -89),   S( -4, -69),   S(-10, -53),   S( -1,   3),   S( -7, -31),   S(-13,   3),   S(  5, -51),   S(-18, -36),
            S(-23, -71),   S(-72, -88),   S(-13, -48),   S(-32, -80),   S(-11, -46),   S(-19, -47),   S(-58, -25),   S(  0, -38),
            S(-37, -65),   S(-38, -49),   S(-32, -18),   S(  9, -41),   S( -9, -50),   S( -8, -62),   S(-10, -50),   S(-49, -16),
            S( -8,  -2),   S(-25, -17),   S(-11, -12),   S( -4,   8),   S(  6, -30),   S( -1,   8),   S( 18,  10),   S( -4,  21),
            S( -8, -23),   S(  8,  39),   S(  4,  15),   S(  2,  38),   S( 23,  35),   S( -2,  11),   S( 22,  77),   S( 20,  85),
            S(-15,  50),   S(  2,  46),   S( 27,  97),   S( 26,  78),   S( 21,  84),   S( -1,  18),   S( 28, 108),   S( 27, 136),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6, -58),   S( -5, -41),   S(-20, -32),   S( -8, -22),   S(-10, -37),   S(-31, -12),   S(-16, -21),   S(-12, -66),
            S(-25, -37),   S(-43, -77),   S(-17, -98),   S(-13, -57),   S(-16, -16),   S(-22,   8),   S(-42,   0),   S(-17, -70),
            S(-38, -38),   S(-52, -43),   S(-47, -53),   S( 38,  -9),   S(-24,  -4),   S(-14, -36),   S( -7, -33),   S(-29, -10),
            S(-30,  17),   S(-19,  -2),   S(  4,  51),   S( -4,  30),   S( 25,  55),   S(  3,   3),   S( 11,  20),   S( -4,  21),
            S(  1,  46),   S(  5,  65),   S(  7,  65),   S( 14,  66),   S( 27, 125),   S(  7,  42),   S( 18,  80),   S( 14,  39),
            S(-18,  21),   S( -5,  46),   S(  9,  99),   S( 27,  69),   S( 14,  65),   S( 23,  84),   S( 17,  45),   S( 15,  49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-92, -60),   S(-27, -21),   S(-19, -18),   S(  7,   3),   S(-16, -10),   S(-21,  14),   S(  3, -38),   S(-70, -45),
            S( 10, -13),   S(-15,  55),   S(-28,  -1),   S( -6,   3),   S(-12,  -1),   S(-15, -19),   S(-27, -85),   S(-23, -59),
            S( -4,  -8),   S( 12, -22),   S( -4,  26),   S( 33,  47),   S(-20,  43),   S(  8,  13),   S(-32,  31),   S(-36, -19),
            S( -9,  43),   S( 35,  63),   S( 22,  51),   S( 41,  49),   S( 26,  39),   S(  5,  52),   S( 52, -12),   S( -6,   0),
            S( 44,  99),   S(-12,  69),   S( 62,  68),   S( 42,  52),   S( 70,  40),   S(  8,  44),   S( 10,   2),   S(  9,   2),
            S(104, -61),   S(-24,  70),   S(136,  51),   S( 91,  24),   S( 58,  47),   S(-33,  76),   S( 44,   8),   S(-28,   5),
            S( 47,  -7),   S(-17, -39),   S( 51,   2),   S( 64,  29),   S( 69,  39),   S(  5,  68),   S(-35,  15),   S(-41,  10),
            S(-113, -165), S(  2,   4),   S(  7,  18),   S( 35,  69),   S(  6,  16),   S(  9,  20),   S(-37,  10),   S(  2,   4),

            /* knights: bucket 1 */
            S( 17,   1),   S(-63,  13),   S(-11,  27),   S(-44,  38),   S(-28,  39),   S( -8, -40),   S(-42,   2),   S(-50,   7),
            S(-63,  42),   S(-42,  60),   S(-29,  40),   S( -7,  27),   S(-21,  43),   S(-19,  45),   S(-15,  23),   S(-26, -60),
            S(-29,  30),   S(-12,  17),   S(-19,  33),   S(  3,  62),   S(-15,  57),   S(-12,  32),   S(-42,  35),   S(-10,  22),
            S(  0,  63),   S(  7,  63),   S(  3,  50),   S( -8,  71),   S( 11,  56),   S(-23,  79),   S(-16,  58),   S(-41,  71),
            S( 54,  -1),   S( 14,  31),   S( 10,  89),   S( 23,  58),   S( 23,  55),   S( 11,  58),   S( -8,  55),   S(-11,  81),
            S(-11,  52),   S( 67,  -7),   S( 94,  22),   S( 76,  45),   S( 47,  47),   S(-40,  69),   S( 43,  33),   S( 12,  34),
            S( 17,  -9),   S( 16, -13),   S( 21,   2),   S( 47,  59),   S( 23,  24),   S(  4,  23),   S( 13,  64),   S(-35,  40),
            S(-201, -20),  S( 12,  -7),   S(-34, -61),   S(-28,   7),   S(  4,  25),   S( 47,  48),   S( 28,  24),   S(-69,   9),

            /* knights: bucket 2 */
            S(-51,   5),   S(-41,  39),   S(-42,  26),   S(-32,  37),   S(-16,  19),   S(-57,  25),   S(-35,  17),   S(  5,  -4),
            S(-33,  20),   S( -5,  51),   S(-30,  39),   S(-14,  34),   S(-29,  47),   S(-20,  21),   S( -5,  29),   S(-35,  22),
            S(-30,  50),   S(-27,  42),   S(-26,  42),   S(-17,  74),   S(-21,  70),   S(-23,  36),   S(-20,  28),   S(  0,   3),
            S(-18,  79),   S(-18,  65),   S(-25, 101),   S(-21,  94),   S(-27,  91),   S( -3,  69),   S(-10,  63),   S(-12,  48),
            S(-34,  87),   S(-12,  81),   S( -4,  88),   S( 19,  67),   S(-26,  92),   S( 25,  71),   S(-25,  82),   S( 23,  19),
            S(-39,  82),   S(-30,  71),   S(-43, 119),   S( 36,  32),   S( 43,  47),   S(137,  12),   S( 54,  35),   S( -8,   6),
            S(  8,  63),   S(-49,  72),   S( 27,  25),   S(-12,  17),   S( -4,  48),   S( -7,   4),   S( 34,  28),   S( 13, -19),
            S(-44,  23),   S( 18,  69),   S(  0,  73),   S( -8, -14),   S(-38, -19),   S(-24, -41),   S( 33,  40),   S(-123, -67),

            /* knights: bucket 3 */
            S(-74,  48),   S(-26, -18),   S(-18,  18),   S(-13,  25),   S( -4,  17),   S(-18,  22),   S(-30,   9),   S(-29, -46),
            S(-25,   4),   S( -8,  27),   S( -5,  30),   S( -7,  36),   S( -7,  33),   S( 11,  21),   S( 18,  13),   S( 11,  -9),
            S(-10,  41),   S(-19,  38),   S( -4,  59),   S(  0,  77),   S(  5,  80),   S( -5,  57),   S(  8,  42),   S( 15,  -3),
            S( -6,  39),   S( 10,  48),   S( 13,  83),   S(  5,  98),   S( 12, 104),   S( 25,  95),   S( 20, 103),   S(  5,  67),
            S(-19,  88),   S( 10,  84),   S( 15,  99),   S( 33, 111),   S( 16, 106),   S( 33, 116),   S(-21, 125),   S( 48, 106),
            S(-27,  66),   S(  7,  82),   S( 26,  88),   S( -2, 118),   S( 57, 107),   S(118, 109),   S( 50, 115),   S(  8, 119),
            S(-34,  56),   S(-28,  89),   S(-31, 111),   S( -3,  89),   S( 21,  96),   S(106,  44),   S(-16,  19),   S( 60,   4),
            S(-172,  51),  S(-20,  88),   S(-60,  86),   S( 10,  62),   S( 47,  93),   S(-70,  55),   S(-30, -61),   S(-74, -133),

            /* knights: bucket 4 */
            S(  3,  -2),   S( -5,   2),   S(-49,   4),   S(-30,  -7),   S(-39,  28),   S(-23,   4),   S(  2, -45),   S(-24, -16),
            S( 34,  42),   S(  6, -36),   S(  9,  13),   S(  4,   0),   S( 13, -21),   S( 25, -48),   S( 10,  -3),   S(-27,  21),
            S(-25, -16),   S( 46,  -6),   S( 92,   5),   S( 84,   8),   S( 54,  15),   S( 33, -19),   S(-10,  -5),   S(-17, -54),
            S(-41, -30),   S( 14,  -5),   S( 19,   8),   S( 40,  25),   S( 32,  20),   S(-12,  35),   S(-28,  -8),   S( 13,  -6),
            S(  6, -78),   S( 11,  17),   S( 43,  37),   S( 28,  49),   S( 78,   4),   S( 22,  37),   S( 53,  28),   S(-33,  27),
            S(  3, -25),   S(-23, -39),   S( 38, -20),   S( 50,  23),   S( -3,  33),   S( 10,  59),   S(-24,  -8),   S( 21,  -6),
            S(-21, -43),   S(-46, -44),   S( -6, -21),   S( 25,  37),   S( 29,  29),   S(  7,  36),   S(  5,  25),   S(-31,  -7),
            S(  6,  16),   S(-10, -38),   S(-13, -28),   S( 23,  27),   S(  6,   3),   S(-10,  22),   S(  2,  29),   S(  0,  -5),

            /* knights: bucket 5 */
            S(  7,  14),   S( 29,  22),   S(-33,  26),   S(  8,  10),   S(-14,  41),   S( 29,   5),   S(-11,  -7),   S(  7,  14),
            S( 15,  27),   S( 46,  -1),   S( 41,  -8),   S( 10,   2),   S( 55, -18),   S( -8,   1),   S(  9,  37),   S(-56,   0),
            S(-61,  16),   S( -1,  -1),   S( 54,   5),   S( 33,  16),   S( 49,  20),   S(-16,  29),   S(-11,   7),   S(-26,  -8),
            S( 44,   9),   S( 17, -31),   S( 93,  -7),   S( 40,   8),   S( 93,   1),   S( 99,  -4),   S(  9,  20),   S( 40,  16),
            S( 50,   8),   S( 44,  -5),   S(110, -17),   S(131,  -9),   S(113, -35),   S( 53,  26),   S( 66, -12),   S( 34,  22),
            S(-16,   1),   S( 40, -15),   S( 22, -21),   S( 14,   9),   S( 41,  -5),   S( 23,   4),   S( 10,  12),   S( 21,  36),
            S(  8,   9),   S(-19, -33),   S( -5, -48),   S( -5,   6),   S( -9, -42),   S( 14,   8),   S( -4,  31),   S(  5,  43),
            S(-45, -62),   S(-22, -32),   S( 16,  -3),   S(-20,  -8),   S(  2, -11),   S( -8,  18),   S(  4,  11),   S( -3,  18),

            /* knights: bucket 6 */
            S( 16,   7),   S(-38,  26),   S(  7,  -3),   S( -7,  18),   S(-28,  21),   S( 30,  18),   S(-18,  33),   S(-15,  32),
            S( 27,  -2),   S( -6,  32),   S( -3,  -1),   S( 31,   8),   S( 49,  11),   S( -6,  25),   S( -1,  31),   S(-13,  50),
            S(-13,  21),   S( 33,  16),   S( 47,  10),   S( 37,  37),   S( 46,  33),   S(-23,  32),   S( 43,  22),   S( -9,  28),
            S( 43,  28),   S( 93,  -6),   S( 43,  23),   S( 80,   9),   S( 85,   6),   S( 81,  21),   S( 22,   1),   S(-49,  64),
            S(  5,  39),   S( 67,  14),   S(141,   1),   S(109,  13),   S(138, -31),   S( 71,  23),   S(157, -25),   S( 30,  21),
            S( 20,  10),   S( 37,  10),   S( 51,  24),   S( 28,  19),   S( 25, -10),   S( 51,   3),   S( 29,  -2),   S(  4,  -7),
            S( -2,  23),   S( 25,  24),   S( 22,  38),   S(  2,   0),   S( 29,  13),   S( 19, -39),   S( -6,   2),   S( 11,  46),
            S(  5,  32),   S( 17,  28),   S( 10,  21),   S(-10,   6),   S( 29, -18),   S( -4,  -9),   S(  9,  24),   S(-35, -55),

            /* knights: bucket 7 */
            S( -7, -15),   S(  5, -43),   S( 42,   0),   S(-34,  29),   S(-10, -10),   S(-32,  13),   S(-27,   2),   S(  0,  28),
            S(-16, -50),   S(  6, -20),   S(-16,   3),   S( -8,   6),   S( 35,   3),   S( 34,  14),   S( 32,  11),   S(-49,  -4),
            S(-19, -45),   S(-58,   8),   S( 27, -19),   S( 27,  24),   S( 72,  19),   S( 56,  13),   S( 39,  12),   S(  5,  37),
            S(-24,  14),   S( 56,   2),   S( 53,  -2),   S(100,   3),   S(119,  -5),   S( 73,  32),   S( 86,  10),   S( 99, -13),
            S( 21,   6),   S( 38,   3),   S( 16,  37),   S( 88,   4),   S(107,   7),   S(158,  -3),   S(191,  -4),   S(  3,  37),
            S( -9, -11),   S( 30,  21),   S(  9,  27),   S( 76,   9),   S(121,  10),   S(107,  -8),   S( 25,   1),   S(  3, -58),
            S(  2,  37),   S(  0,   0),   S( 30,  21),   S( 28,  28),   S( 43,  20),   S( 48,  19),   S( -7, -26),   S(  7,  -5),
            S(-20, -15),   S( -6,  23),   S(-12,  34),   S(  6, -14),   S(-26,  -2),   S( 17,   9),   S( 17,  40),   S(  0, -19),

            /* knights: bucket 8 */
            S( -9, -21),   S(  1, -30),   S(  8,   0),   S(-14, -67),   S(-14, -23),   S( -7, -54),   S( 16,  21),   S( -1, -17),
            S(-12, -57),   S( -7, -57),   S( -6, -52),   S(-16, -30),   S(-23,  -9),   S(  4,  -4),   S( -6, -35),   S( -5, -20),
            S(-13, -64),   S( -6, -26),   S(  6, -57),   S( 20, -31),   S( 18, -26),   S( 26, -23),   S( -3, -44),   S(  2, -24),
            S(-29, -96),   S(  0, -21),   S( 10,  -3),   S( 28,   5),   S( 17, -47),   S(  9, -10),   S(-15, -38),   S( -8, -44),
            S( -9, -29),   S( -3, -58),   S(  4, -14),   S( 25, -21),   S( 15, -12),   S(-19, -28),   S( -5, -10),   S( -7, -30),
            S( -1,   5),   S( 13,   1),   S(  0, -33),   S( 10,  -2),   S(  7, -11),   S( 14, -15),   S(-16, -24),   S( -7,  -7),
            S( -5,  -7),   S( -1, -51),   S(  0, -10),   S(  8,  13),   S(  5,   3),   S( 11,   8),   S( -4, -23),   S( -1,   0),
            S(  1,   1),   S(  1,  15),   S( -8, -13),   S(  1, -22),   S(  2,   4),   S( -4,  -3),   S( -4, -13),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-21, -83),   S(  1,  11),   S(-16, -83),   S(-11, -59),   S(-19, -28),   S(-12, -29),   S( 13,  30),   S( -9, -46),
            S(-11, -33),   S(-20, -68),   S(-16, -127),  S( -9, -65),   S(-21, -72),   S(-19, -74),   S( -1, -29),   S(-17, -24),
            S( -9, -75),   S( -8, -37),   S( -4, -47),   S( -5, -89),   S( -7, -32),   S( 35, -35),   S( -3,  -3),   S( -8, -14),
            S(-19, -64),   S(-19, -75),   S( -2, -61),   S( -5, -67),   S(  7, -51),   S( 32, -17),   S(-13, -84),   S( -2, -30),
            S(  8,   0),   S(-18, -51),   S( 14, -22),   S( 11, -40),   S(-13, -75),   S( 16, -12),   S(  4, -19),   S( -3, -12),
            S(-24, -68),   S(-14, -61),   S( -7, -33),   S(  3, -38),   S( 12, -15),   S( 14, -25),   S(  9,  -5),   S(  2,  30),
            S(-17, -36),   S(  2,  -4),   S(-11, -13),   S(-20, -43),   S(  7, -33),   S(  5,  21),   S( -6,  14),   S(-12, -43),
            S(  6,  15),   S(  1, -10),   S( -1,  -7),   S(  0,  -4),   S(  4,  -2),   S( -5, -41),   S(  4,   5),   S(  5,  23),

            /* knights: bucket 10 */
            S( -3, -29),   S( -9, -32),   S(-20, -52),   S(  1,  -1),   S(-24, -77),   S(-14, -49),   S( 14,  20),   S(  3,   0),
            S( -8, -53),   S( 14, -17),   S( -2, -61),   S(  1, -66),   S(  0, -35),   S(-31, -117),  S(-22, -42),   S( 10,  32),
            S(  3,   2),   S( 11, -37),   S(  6, -29),   S( 25, -55),   S(-31, -71),   S(  8, -44),   S( -3, -26),   S(-13, -21),
            S(  6, -51),   S( -5, -43),   S( 25, -43),   S( 10, -45),   S( -1, -47),   S( -6, -54),   S(-13, -75),   S( -9, -55),
            S(-17, -42),   S( -4, -51),   S( 17, -46),   S( -1, -21),   S(  4, -38),   S(  3, -71),   S( -6, -28),   S( 12, -21),
            S( -3,   5),   S( -3, -23),   S( -5,  -2),   S( -8,  -9),   S(-17, -32),   S(-12, -36),   S( -2, -19),   S(-16, -34),
            S( 10,  25),   S(  1, -13),   S(-17, -69),   S( 15, -50),   S( -9, -32),   S(-17, -84),   S( -6, -18),   S(-10, -22),
            S(  1,   0),   S( -3, -15),   S(-11, -19),   S(  0, -21),   S( -1, -18),   S(-10, -45),   S(  4, -10),   S(  2,  23),

            /* knights: bucket 11 */
            S( -9, -50),   S(-18, -59),   S(  0, -22),   S( 10, -28),   S(-31, -69),   S(  7, -17),   S( -9, -11),   S( 13,  25),
            S( -6, -26),   S(-24, -67),   S(-16, -68),   S( 47, -31),   S( 30, -18),   S(  6, -53),   S( -2, -22),   S( -4, -23),
            S(-11, -43),   S(-17, -58),   S( -3, -43),   S( 36,  -3),   S( 11,  -9),   S( 30, -15),   S( -3, -53),   S(-12, -56),
            S(-17, -22),   S( 10, -46),   S( 12, -35),   S( 32,  -3),   S( 32, -28),   S( 20, -12),   S( 11,  -8),   S(  0, -15),
            S(-17, -40),   S(  1, -87),   S( 17,  -8),   S( 24, -22),   S( 42,  17),   S( 21,  12),   S( -6, -81),   S( -2, -11),
            S( -7, -18),   S( -4, -44),   S( 19,  18),   S( 28, -10),   S( 25,   5),   S( -4, -20),   S( -9, -56),   S( -3, -12),
            S( -8, -20),   S( -1,  18),   S(-22, -58),   S( 12,   2),   S( -2, -31),   S(  9, -51),   S( 10,  -2),   S( -3,  -1),
            S(  0,  -1),   S(  2,   6),   S( -9, -38),   S(  1,  25),   S( -5,   1),   S(  8, -12),   S( -2,   8),   S( -1,  -4),

            /* knights: bucket 12 */
            S(-18, -63),   S( -2, -37),   S( -1,  -9),   S(  0,  14),   S(  4,  18),   S(  1,  -7),   S( -1,   1),   S( -3, -10),
            S( -4, -21),   S(  4,  18),   S( -6, -34),   S( -5, -34),   S( 10, -34),   S(  3,   6),   S(  0,  -4),   S(  1,  -5),
            S( -7, -23),   S(-14, -60),   S( -2, -25),   S( -8, -81),   S( -8, -36),   S(  5,  -1),   S( -4, -16),   S( -5, -17),
            S(  1,  -3),   S( -4, -56),   S(  6,   5),   S(  3, -23),   S( 12, -18),   S(  2,  -3),   S( -2,  -1),   S(  5,  24),
            S( -2, -18),   S( -9, -53),   S(  4, -29),   S( -3, -23),   S( 16,  38),   S(  5,  16),   S(-10, -25),   S( -6, -13),
            S( -3,  -8),   S(  0, -25),   S( -1, -13),   S(  1, -17),   S( -9, -21),   S(-10, -17),   S(  5,  13),   S(  0,  -2),
            S( -4, -15),   S( -3,  -6),   S( -9, -10),   S( -4, -20),   S( -2,  -2),   S(-11, -25),   S( -5, -18),   S( -4,  -6),
            S(  0,   0),   S(  4,  27),   S(  1,  -1),   S(  2,  -5),   S(  2,   6),   S(  3,  18),   S(  1,   5),   S(  0,   1),

            /* knights: bucket 13 */
            S( -3,  -6),   S( -4, -16),   S(-10, -72),   S( -4, -26),   S( -4, -18),   S( -6, -29),   S( -2,  -6),   S(  1,   2),
            S( -6, -17),   S( -3, -27),   S(  1,   7),   S( -1, -26),   S( -6, -39),   S(  3, -15),   S(  1,   4),   S( -9, -31),
            S(  2,  -8),   S(  4,  24),   S(  0, -27),   S( -8, -45),   S(  5,  17),   S( -7, -16),   S(  5,  -8),   S( -3,  -5),
            S( -5, -30),   S( -3, -14),   S( -7, -45),   S(  8,  18),   S(  4, -39),   S( -7, -46),   S( -8, -51),   S( 22,  39),
            S(  4,  18),   S( -8, -50),   S( -3, -66),   S(  3, -44),   S( -8, -30),   S(  5, -21),   S(-12, -45),   S( -5, -19),
            S(  1,   3),   S(  1,   4),   S(-11, -26),   S(  5, -32),   S( -2, -12),   S( -4, -39),   S(  2,   3),   S( -1, -13),
            S(  3,   9),   S(  7,   8),   S( -6,   0),   S(  1,  -8),   S(  5,  29),   S(  1,   9),   S( -2,  -6),   S(  1,   2),
            S(  0,   2),   S(  0,  -2),   S( -2,  -6),   S(  2,   6),   S( -1,  -7),   S(  2,   7),   S( -1,   4),   S(  1,   4),

            /* knights: bucket 14 */
            S(  0,  -1),   S( -4, -13),   S(  5,   0),   S( -6, -21),   S(  2, -18),   S(  3,  14),   S(  4,  -6),   S( -3, -20),
            S( -2, -13),   S(-11, -47),   S(  4, -14),   S( -1, -40),   S(  0, -32),   S( -4, -10),   S( -5, -26),   S( -1,  28),
            S(  0, -14),   S(-18, -80),   S(  8,  -2),   S( -2, -37),   S( -2,  -7),   S(  1, -10),   S(  0,  -9),   S(  2,   3),
            S(  0,  -3),   S( -3, -43),   S( -9, -42),   S(  3,  -8),   S(  3,  -1),   S( -2, -22),   S( -7, -29),   S( -3,  11),
            S( 11,  17),   S(-21, -71),   S(-12, -32),   S(  6,  -8),   S( -6,  18),   S(  0, -30),   S( -6, -18),   S(  4,  -1),
            S( -2, -12),   S(  5,  13),   S( -6,  43),   S(  9,  15),   S( -3, -13),   S( -3,   6),   S( -6,  -8),   S( -1,   3),
            S( -1,  -3),   S( -9, -27),   S( -1, -14),   S(  6,  35),   S(  2,  27),   S( -7, -20),   S(  0,  -1),   S(  5,   6),
            S(  0,   1),   S( -2,  -9),   S(  0,  -3),   S(  2,  17),   S( -2,  -8),   S( -1,  -8),   S( -1,  -2),   S(  0,   0),

            /* knights: bucket 15 */
            S( -6, -27),   S( -4, -25),   S(  4,  24),   S( -7, -15),   S( -7, -31),   S( -5, -32),   S( -2, -43),   S( -3, -33),
            S(  0,   0),   S(  4,   0),   S( -8, -41),   S( 10,  32),   S(  5, -24),   S(-14, -71),   S( -4, -26),   S( -2, -15),
            S(  3,  -4),   S(-12, -31),   S( -5, -29),   S(  6, -27),   S(-29, -120),  S( -2, -38),   S( -6, -37),   S( -1, -11),
            S( -5, -31),   S( -6, -18),   S( -6, -13),   S(  3,  22),   S( -3, -25),   S(  1, -16),   S(  2,  -4),   S( -4, -13),
            S( -2,  -8),   S( 14,  27),   S( -5, -30),   S( -3,  18),   S( 23,  12),   S( 12,  11),   S(  3, -19),   S( -1,  -3),
            S( -2,  -6),   S(-12, -24),   S( -6, -16),   S(-11, -27),   S(-20, -55),   S(  2,  31),   S(  1,  11),   S(  7,  20),
            S( -5, -13),   S( -3,  -3),   S( 10,  37),   S(  3,   5),   S(  7,  36),   S(  8,  24),   S(  0,   9),   S(  6,   9),
            S(  2,   9),   S( -1,  -9),   S(  0,   3),   S(  1,  10),   S(  2,  13),   S( -1,   3),   S(  1,  13),   S(  1,   4),

            /* bishops: bucket 0 */
            S( 44,  -8),   S(-13,  17),   S(-12,   3),   S(-25,  -9),   S( -5,   1),   S( 20, -12),   S(100, -94),   S( 38,   3),
            S(-46, -17),   S( 18, -25),   S(  2,  18),   S( 19,  -2),   S( 19, -10),   S( 64, -22),   S( 48,  20),   S( 55, -28),
            S(-12,  46),   S( 10,  13),   S( 29,  11),   S( 19, -10),   S( 42,  -8),   S( 57,  17),   S( 45,  -2),   S( 18, -16),
            S( 16, -41),   S( 50, -34),   S( 23,   1),   S( 66, -20),   S( 82,  11),   S( 65,  16),   S( 14, -22),   S(  4,  19),
            S( 56, -25),   S( 55, -20),   S( 82, -13),   S( 93,   0),   S(136, -37),   S( 22,  30),   S( 38,  38),   S(  6, -19),
            S( 52, -15),   S(120, -12),   S(114,  -5),   S( 56,  -3),   S( 34, -11),   S( 10,  31),   S( 61,   9),   S(  7, -16),
            S(-70, -124),  S( 73,  22),   S( 84,  57),   S( -8,  -8),   S( 29, -12),   S( 16,  21),   S( -1,  49),   S(-55,  48),
            S( -9, -35),   S(-20, -26),   S( -4, -31),   S( 16, -31),   S(-30, -15),   S(-32,   1),   S(-26,   5),   S(-10, -26),

            /* bishops: bucket 1 */
            S(-49, -12),   S( 34, -32),   S(-25,  28),   S( 12, -12),   S( -7,   5),   S( 16,   5),   S( 38, -10),   S( 51, -26),
            S( 15, -40),   S( -4, -10),   S( -1, -21),   S(  2,   1),   S( 42, -30),   S( 15, -19),   S( 66, -33),   S( 23, -36),
            S(-14,  -3),   S( 39, -18),   S(  2, -11),   S( 28,  -8),   S( 10,  -9),   S( 55, -23),   S( 13, -19),   S( 76, -41),
            S( 32, -24),   S( 66, -33),   S( 31,  -7),   S( 23,  -5),   S( 80, -19),   S( 16,   9),   S( 77, -38),   S( -7,  -1),
            S( 19, -45),   S( 73, -29),   S( 12,   4),   S(116, -31),   S( 76,  -9),   S(119, -51),   S( 11,  10),   S( 27,  -7),
            S( 82, -42),   S( 40, -10),   S( 76, -14),   S( 90, -36),   S(139, -53),   S( -5,  -8),   S( 17,   3),   S(-14,  -5),
            S(  8, -93),   S( 24, -29),   S(-14, -42),   S(  6, -14),   S( 21,  15),   S(  7,   1),   S( 15, -10),   S(-20,   9),
            S(-12, -49),   S(-19,   7),   S(-12, -47),   S(-58,   4),   S(-35, -25),   S( 21, -23),   S( 38, -14),   S(-60, -41),

            /* bishops: bucket 2 */
            S( 38, -42),   S(  1, -20),   S(  4,   7),   S(-20,   4),   S( 18,   1),   S(-10,   5),   S( 47, -34),   S( -3,   7),
            S( 35, -32),   S( 19, -10),   S( -2, -11),   S( 19,   6),   S( -4,  13),   S( 17,  -9),   S( 11, -16),   S( 31, -59),
            S( 34,  -3),   S( 30, -12),   S( 12,   3),   S( -1,   2),   S( 10,  14),   S(  2, -10),   S( 11, -16),   S(-16,   0),
            S( 12, -22),   S( 53, -14),   S( 12,   2),   S( 32,  19),   S( 11,   7),   S(  0,  21),   S(-12,   1),   S(  7,   5),
            S( 13,   2),   S( 14,  12),   S( 76,  -9),   S( 30,   9),   S( 35,  26),   S( 27,  10),   S( 11,  26),   S( 32, -21),
            S(-21,  14),   S( 16,  17),   S(-12,   3),   S(102, -27),   S( 64, -13),   S( 97,  13),   S( 76,  -7),   S( 38, -55),
            S(-49,  47),   S(  5,  -3),   S(  2,  20),   S( 21,   9),   S(-100, -28),  S(-37, -24),   S(-60,  31),   S(-35, -16),
            S(-56, -16),   S(-11,  13),   S(  2,  -1),   S(-49,  21),   S(-39, -44),   S(-55, -19),   S(-22, -28),   S(-37, -37),

            /* bishops: bucket 3 */
            S( 36,   7),   S( 54, -18),   S( 10,  -6),   S( 10,  10),   S( 18,  18),   S(  3,  45),   S(-15,  56),   S( 16, -20),
            S( 45,   8),   S( 30,   6),   S( 22,  13),   S( 25,  21),   S( 26,  19),   S( 24,  28),   S( 15,  13),   S( 47, -37),
            S( -6,  25),   S( 41,  44),   S( 30,  36),   S( 25,  30),   S( 19,  47),   S( 24,  28),   S( 26,  14),   S( 10,  33),
            S( -9,  28),   S( 20,  34),   S( 31,  52),   S( 35,  55),   S( 45,  35),   S( 30,  27),   S( 31,  -3),   S( 33, -13),
            S( 21,  18),   S( 19,  50),   S( 14,  58),   S( 60,  61),   S( 57,  63),   S( 59,  28),   S( 22,  42),   S( -1,  33),
            S(  6,  30),   S( 35,  51),   S( 11,  45),   S( 19,  44),   S( 37,  55),   S( 68,  70),   S( 36,  73),   S( 30,  93),
            S(-18,  66),   S( 19,  51),   S(  9,  43),   S( 10,  32),   S(  6,  55),   S( 46,  75),   S(-42,  21),   S( 48, -58),
            S(-47,  42),   S(-20,  45),   S(-59,  47),   S(-45,  54),   S( 32,  16),   S(-92,  54),   S( 16,   3),   S( 15,   8),

            /* bishops: bucket 4 */
            S(-21,  36),   S( -8, -33),   S(-52,  15),   S(-37,  -5),   S(-27, -31),   S(  5, -23),   S( -7,  -3),   S(-27, -82),
            S(-38,  17),   S(  2,  -5),   S( 64, -34),   S( -7,  -3),   S(-25,  -7),   S( -6, -36),   S(-41,   9),   S(-64, -47),
            S( -2,   7),   S(-46, -11),   S( 44, -31),   S( 48, -34),   S( 22, -15),   S(-37,   2),   S(-21, -34),   S(-67, -29),
            S(  6, -40),   S( 64, -31),   S( 44, -29),   S( 42,  16),   S(  8,   0),   S( 38,  -2),   S(-43,  14),   S(-24, -27),
            S(  9,  -9),   S(  0, -70),   S( 34, -36),   S( 74, -33),   S( 28,  -7),   S( -8,   7),   S( 27,  11),   S(-53,  -4),
            S(-67, -97),   S(-19, -34),   S( 14, -46),   S( 22, -22),   S(-17,   8),   S( 24,   2),   S(-14,   0),   S( -5,  21),
            S(  2,  11),   S(-24, -35),   S(  9, -45),   S(  4, -19),   S(-15,  -7),   S( 30,   8),   S(-12,  -4),   S( 31,  38),
            S(-11, -34),   S( -9, -40),   S(-10, -33),   S(  0, -53),   S(-12, -39),   S(-25,  -4),   S(-19,  27),   S( 12,  24),

            /* bishops: bucket 5 */
            S(-28,   7),   S(-20,   0),   S(-50,  12),   S(-38,  11),   S(-24,   1),   S(-52,  -7),   S(-20,  17),   S(-44, -21),
            S(-31, -18),   S(-36,  17),   S( 36, -19),   S( 17,  -3),   S(-36,  -8),   S(  2,  -7),   S(-59,  -2),   S( -9, -22),
            S( 17,   0),   S(-32,  -3),   S( 41, -17),   S(-14,   1),   S( 13,  -1),   S(-36,   0),   S( 28, -10),   S(-24,  21),
            S( 17, -14),   S(-41,   2),   S( 39, -17),   S( 87, -18),   S(-48,  21),   S( 53, -14),   S(-72,   9),   S(  4,  -3),
            S( 16, -31),   S( -4,  -7),   S(  5, -24),   S(  9, -30),   S(  1, -15),   S(-15,  -1),   S( 47,   5),   S(-17,  -5),
            S(-15, -16),   S(-18,  -1),   S( 46, -52),   S(-52, -38),   S( 15, -25),   S( 21, -11),   S(-13,   7),   S(-17,  16),
            S(-45, -25),   S( -9, -16),   S(-34, -18),   S(-19,  -8),   S(  8, -15),   S( -8,  -1),   S(  5,  24),   S(-21,   2),
            S(-24, -19),   S(-31, -32),   S( -1, -27),   S( 23, -15),   S(-30,  20),   S(-15,  -1),   S(-28,  -7),   S(-18,  18),

            /* bishops: bucket 6 */
            S(-36,   9),   S(-42,   1),   S(-35,   9),   S(-24,   8),   S(-49,  12),   S(-22,   2),   S(-43,  27),   S(-79,  14),
            S(-36, -15),   S(-30, -15),   S(-45,   9),   S(-26,  11),   S(-57,  20),   S(-50,  12),   S(-64,  33),   S(-53,  13),
            S( 31, -20),   S(-26,   3),   S(-12,  -3),   S(-40,  14),   S(-26,  22),   S(-36,  10),   S( -6,  -8),   S(-16,   5),
            S(-40,   4),   S(-29,  -1),   S(-19,   5),   S( 78,   0),   S( 52,   4),   S( 36,   3),   S(  5, -16),   S(-15,  -9),
            S(-70,   9),   S(-21,  12),   S( 46, -10),   S( 31,   3),   S(-24, -13),   S(  5, -22),   S( 40,  -9),   S(  0, -28),
            S(-43,  23),   S( -9,   2),   S( -7,   4),   S( 18, -10),   S(  1,  14),   S( -5, -12),   S( 19,  10),   S( -1, -10),
            S( -9,  12),   S(-70,  35),   S( 11, -15),   S(-10, -20),   S(-14,   7),   S( -9,   8),   S(  9, -14),   S(-37,  -6),
            S(-29,   6),   S(-48,  15),   S(-10,   2),   S( -9,   0),   S(-17,   9),   S( 17, -34),   S(-32,   3),   S(-10,  -7),

            /* bishops: bucket 7 */
            S(-18, -53),   S(-33, -27),   S(-30, -41),   S(-14, -10),   S(-21, -23),   S(-48,  -2),   S(-66, -28),   S(-65,  -4),
            S(  6, -56),   S(  9, -46),   S( 27, -44),   S(-19, -22),   S(-21,   0),   S(-39,   5),   S(-20, -32),   S(-43,  10),
            S(-50, -24),   S( -5, -12),   S(  4, -10),   S( 22, -16),   S( 25, -21),   S( 23, -26),   S(-55,   1),   S(-55,  37),
            S(-25, -20),   S(-46,   7),   S( -1,   1),   S( 25, -17),   S( 90,  -5),   S( 36, -27),   S( 52, -39),   S(-14, -34),
            S(-17, -16),   S( 37,  -9),   S( 19, -20),   S( 43, -25),   S( 92, -27),   S( 72, -38),   S(-28, -22),   S(-31, -24),
            S(-39, -16),   S(-32,  28),   S( 37, -18),   S(-38,   4),   S(-22,   1),   S( 69, -26),   S(  0, -12),   S(-63, -71),
            S(-39, -14),   S(-38,  16),   S(-34,   8),   S( 19, -10),   S(-16, -23),   S( 32, -30),   S(-10, -24),   S(-27, -29),
            S(-18, -10),   S(-30,   0),   S(-32,   6),   S( -7, -14),   S( -9, -15),   S( -6, -33),   S( 27, -38),   S(-10, -20),

            /* bishops: bucket 8 */
            S(  2,  38),   S( -7,  -2),   S(-17, -28),   S(  3,  36),   S( -7,   9),   S(  5, -29),   S(-19, -48),   S( -9, -29),
            S(-13,  20),   S( 19,  38),   S( 11,   2),   S( 13,  -4),   S( -4, -11),   S( 17,  13),   S(-16, -25),   S( -1,  -7),
            S(-17, -49),   S(-10, -45),   S( 15,  46),   S( 20, -12),   S( 36,  33),   S( 22,  21),   S(-20, -44),   S(-42, -62),
            S( -7, -14),   S( 16,  54),   S( 13,   5),   S(  7,  47),   S( 23,  23),   S( 14,  34),   S( -6,   8),   S( -6, -30),
            S( 11,  67),   S( 15,  76),   S( -4,  22),   S( -1, -15),   S(  0,  72),   S(-15,  29),   S(  2, -55),   S(  7,  32),
            S( -6,   4),   S( -1,  35),   S( -1,  39),   S(  5,  21),   S( 21,  21),   S( 11,  53),   S(  5,  42),   S(  5,  39),
            S(  2,  31),   S(-13, -42),   S(  9,  35),   S( 17,  57),   S( -4,  10),   S( 10,  37),   S(  8,  62),   S(-14,  20),
            S( -4,  -5),   S( -2,  -7),   S( -5, -16),   S( -8,   7),   S(  9,  33),   S(  4,  29),   S(-13,  18),   S( 18,  95),

            /* bishops: bucket 9 */
            S(  6,  56),   S(-29,   5),   S(-10,  -1),   S(-38, -26),   S(-52,  -8),   S(-33, -22),   S(-18, -24),   S(-13, -38),
            S(-16,  -9),   S( -3,  25),   S( -3,  12),   S(-23,  -1),   S(-13, -36),   S( -3, -11),   S( -4, -14),   S(-22, -62),
            S(-18, -30),   S(  2,  21),   S(  3, -26),   S(  2,  14),   S( -1,   2),   S(-10,  -2),   S(-20, -12),   S(-26, -42),
            S( -2,  40),   S( -9,  24),   S(  9,  22),   S( -3,  11),   S(-34,  16),   S(  0,  10),   S( 14,   8),   S(  5,   0),
            S( -5,  22),   S( -8,  34),   S( 38,  23),   S(-12,   2),   S( -8,  27),   S( -7,  35),   S( -3,   9),   S(-12, -56),
            S( -9,  22),   S( -1,  11),   S( -9,  26),   S( -3,  15),   S(-31,  11),   S(-11,  31),   S( -8,  24),   S(-11,  29),
            S(  1,  32),   S(-14,  29),   S( 11,   5),   S(  0,  21),   S(-10,   7),   S( -1,  28),   S( -6,  47),   S( -8,  66),
            S(  9,  52),   S( -1,  -3),   S(  3,  12),   S(  5,  28),   S(  3,  34),   S( -4,  38),   S( -6,  51),   S( 24,  78),

            /* bishops: bucket 10 */
            S( -3,  14),   S( -6,  35),   S(-36, -38),   S(-15, -36),   S(-33, -17),   S(-63, -51),   S(-29,  14),   S(  0,  13),
            S(-20, -55),   S(-16, -37),   S( -4, -17),   S(-46, -50),   S(-22, -19),   S(-40, -44),   S(-24, -16),   S(-12,  -6),
            S( -4, -29),   S( -9,  -8),   S(-12,  -2),   S( 17,   5),   S(-16, -40),   S(-11, -29),   S(-17,  -5),   S( 10, -18),
            S(-11, -10),   S(-20, -25),   S(-38, -12),   S( 27, -13),   S(  5,  24),   S( -5,   4),   S(  9,  21),   S(-18, -14),
            S(  4,  20),   S(-38,  25),   S( -1,  -2),   S(-12,  14),   S(  5,   5),   S( -2,  45),   S(-33,  16),   S(-10,  21),
            S(-11,  18),   S(  6,  14),   S(-20,  17),   S(-15,  14),   S(  5,   0),   S( -3,  22),   S( -6,  19),   S(  2,  28),
            S(  3,  14),   S(-18,  23),   S( -5,   4),   S( -5,  25),   S(  4,  13),   S(-41, -31),   S( -6,  -4),   S( 13,  73),
            S( -5,  55),   S(-10,  40),   S( -4,  62),   S(-17,  -2),   S( -6,  26),   S(-10,   8),   S(  6,  15),   S( -1,   2),

            /* bishops: bucket 11 */
            S( 16, -14),   S(-15, -13),   S( -1,   3),   S(-12, -20),   S(-29, -10),   S(  7, -23),   S(-40, -33),   S(-34,  22),
            S(-19, -15),   S( 14, -30),   S(-29, -10),   S(  6, -25),   S(  2,   1),   S(-23, -10),   S(-32,  -2),   S(  2,  23),
            S( -5, -45),   S(-19, -20),   S(  7,   1),   S( 13, -32),   S(-25,   3),   S(  1,   9),   S(-15, -30),   S(-13, -44),
            S(  3,   7),   S( -7, -25),   S( -7,   6),   S(-31, -31),   S( 18,  14),   S( 18,  49),   S( 12,  30),   S(-10, -34),
            S(-18,   6),   S(-16, -15),   S(-19,  22),   S(-27,  36),   S(-17,  20),   S(  2,  25),   S(-11,  37),   S(  1,  21),
            S(-10,  12),   S(-11,  -5),   S(-30,  26),   S(  0,  39),   S(  5,  57),   S(-14,  11),   S(  0,  -2),   S(-17,   7),
            S(-18,  -2),   S( -8,  81),   S( 15,  38),   S(  5,  45),   S(  7,  20),   S(  0,   7),   S(-18, -53),   S(-14, -19),
            S(  7,  91),   S(-23,  38),   S(  8,  46),   S(  1,  27),   S(  5,  36),   S(  0,  24),   S(-12,  12),   S(  7,  34),

            /* bishops: bucket 12 */
            S( -6, -18),   S( -4, -21),   S( -3, -19),   S(-10, -16),   S(-11, -18),   S( -3, -14),   S(  5,  19),   S( -1,  -5),
            S( -4, -18),   S( 10,  27),   S(  6,   2),   S( -6, -40),   S(  7,  21),   S(  5, -10),   S(-17, -25),   S(  0,  -3),
            S(  6,  43),   S(  4,  34),   S( 19,  59),   S( 25,  45),   S( -4, -13),   S( -7, -49),   S(  5,   9),   S(  3,  12),
            S( -2,  33),   S(  6,  57),   S(  7,  22),   S( 11,  34),   S( 16, -16),   S(  0,   1),   S( 12,  22),   S(  8,  31),
            S(  4,   7),   S( -2,   8),   S( 10,  48),   S(  8,  56),   S( 13,  22),   S(  9,  28),   S(  4,   9),   S(  4,  16),
            S(  5,  15),   S(-17, -32),   S(  1,  24),   S( -2,  11),   S( 13,  56),   S(  5,  35),   S( -8, -25),   S( -2, -12),
            S( -5, -14),   S(  3,  19),   S(  3,   4),   S(  1,  11),   S(  0,  10),   S( 12,  58),   S(  7,  39),   S( -2,  32),
            S(  0,  10),   S(  0,  -1),   S(  1,   6),   S( -2,  -5),   S( -3,  -5),   S( -1,   9),   S(  3,  46),   S(  6,  34),

            /* bishops: bucket 13 */
            S( -9, -36),   S( -7, -29),   S( -8, -15),   S(  2,  -5),   S(  8,  33),   S(-14, -38),   S(-26, -53),   S( -5, -23),
            S( -8, -14),   S( -9,  -3),   S( -1,   0),   S(  0,  16),   S( -5,   4),   S( 10,   3),   S(  0, -17),   S(  1,  -2),
            S( -5,  -1),   S( 15,  72),   S(  1,  35),   S( 23,  45),   S(  8,  31),   S( 21,  28),   S(-17, -30),   S( -4,   4),
            S( 20,  95),   S( 11,  72),   S(  5,  55),   S( -7, -11),   S( 13,  52),   S(-12, -24),   S( 13,  44),   S(  3,   6),
            S(  9,  64),   S(  1,  20),   S( 11,  21),   S(  7,  22),   S(  3,  14),   S( -3,  36),   S(  1,  -9),   S(  2,  23),
            S(  0,  25),   S(  0,   6),   S(  4,  36),   S(  1,   0),   S(-18,  36),   S(  0,   0),   S(-13, -10),   S(  1,  15),
            S(  5,  26),   S(-11, -23),   S( -5,   6),   S(  7,  49),   S( -2,  -1),   S( 10,  69),   S(  9,  40),   S(  3,  48),
            S(  2,   3),   S( -1,   4),   S( -4, -15),   S(  1,  19),   S(  2,  17),   S( -5,  -3),   S(  7,  50),   S(  1,  14),

            /* bishops: bucket 14 */
            S(-11, -21),   S(  0,  -6),   S(  8,   8),   S(  9,  61),   S(-15, -25),   S( -3, -23),   S( -8, -35),   S( -7, -19),
            S( -7, -19),   S(  0,   1),   S( -7,  14),   S( -1,  -4),   S(  8,  36),   S(  2,  18),   S( -1,   4),   S(  0,  14),
            S( -5,  -5),   S( -2,   5),   S( -5,   5),   S( 18,  53),   S(  6,  30),   S(  1,  41),   S(  9,  72),   S(  0,  12),
            S(  3,  14),   S( -2,   1),   S( -5,   5),   S(-10,  36),   S(  1,  35),   S(  5,  38),   S( 11,  76),   S(  9,  50),
            S(  9,  37),   S( -5,  -8),   S( -3,  19),   S(  3,  71),   S(  0,  10),   S( -6,  30),   S( 14,  38),   S(  5,  53),
            S( -1,  30),   S(  7,  21),   S( -1,  52),   S(  4,  18),   S( -3,  41),   S(  0,   7),   S( -1,   8),   S(  6,  20),
            S(  6,  52),   S(  8,  27),   S(  7,  52),   S(  8,  29),   S(  1,   9),   S( -2,   5),   S(  1,  -7),   S(  5,  34),
            S(  8,  57),   S(  3,  47),   S( 10,  45),   S(  7,  15),   S( -4,  -6),   S( -1,   2),   S(  9,  28),   S( -2, -15),

            /* bishops: bucket 15 */
            S( -2,  -4),   S( -6, -12),   S(  4,   3),   S(  4,   2),   S(-10, -22),   S( -5, -33),   S( -6, -47),   S(  1,  -2),
            S( 12,  34),   S(  0,   5),   S(-10, -19),   S(  2,   8),   S( 20,  17),   S(  2, -18),   S( -1,  -2),   S( -2,  -4),
            S( -3,  -6),   S( -5, -16),   S( -5,  18),   S( 13,   2),   S(  4,   5),   S( 12,  34),   S(  7,  60),   S(  0,  31),
            S(  1,   3),   S(  9,  23),   S(  9,  48),   S(-11,  -9),   S(  3,  37),   S(  7,  30),   S( 14,  65),   S(  4,  35),
            S( -9, -16),   S(  3,  10),   S(  4,  25),   S( 23,  71),   S(  4,  41),   S(  8,  22),   S(  6,  22),   S(-11, -21),
            S(  4,  16),   S(  1,  13),   S( 10,  62),   S(  6,  42),   S( 14,  34),   S( 14,  83),   S(  4,  22),   S( -2,   0),
            S(  3,  12),   S(  8,  38),   S( -5,  25),   S( 11,  46),   S(  0,  18),   S( -7, -12),   S( -4,  -3),   S( -1,   4),
            S(  4,  19),   S(  3,  36),   S(  4,  43),   S(  7,  18),   S(  2,  18),   S(  0,  15),   S(  3,  15),   S(  2,  13),

            /* rooks: bucket 0 */
            S(-13,  12),   S( 15,  17),   S( -2,   4),   S( -7,  22),   S(-26,  73),   S( -9,  49),   S(-42,  80),   S(-56,  62),
            S(  3,   3),   S(-19,  55),   S(-41,  29),   S( -5,  40),   S( -2,  56),   S(-11,  28),   S(-31,  28),   S(-48,  89),
            S( 21, -19),   S(  2,  -1),   S(-12,  23),   S(  7,  16),   S(-35,  66),   S(-27,  62),   S(-20,  45),   S( -2,  42),
            S( -4,   6),   S( 36,  23),   S(-53,  53),   S( 17,  26),   S( 33,  43),   S(-28,  65),   S(-32,  64),   S(-40,  52),
            S( 51, -41),   S( 14,  54),   S( 29,  37),   S( 25,  40),   S( 27,  49),   S( 20,  66),   S( 37,  49),   S(  8,  61),
            S( 60, -14),   S( 46,  68),   S( 87,  11),   S( 99,  39),   S( -7,  69),   S( 41,  58),   S( 12,  74),   S(-64,  96),
            S( 42,  22),   S( 71,  88),   S(129,  55),   S( 53,  46),   S( 82,  46),   S(  8,  83),   S(-17,  94),   S(-13,  84),
            S( -9, -17),   S( 31,  19),   S( 15,  52),   S( 42,   8),   S( 88,  52),   S( 56,  41),   S( 64,  34),   S( 48,   7),

            /* rooks: bucket 1 */
            S(-57,  53),   S(-20,  13),   S(-17,  38),   S(-38,  38),   S(-41,  60),   S(-36,  56),   S(-48,  75),   S(-78,  86),
            S(-57,  56),   S(-43,  15),   S(-38,  41),   S(-30,  32),   S(-32,  26),   S(-61,  55),   S(-27,  18),   S(-38,  64),
            S(-39,  40),   S(-39,  29),   S(-25,  28),   S(-40,  38),   S(-45,  42),   S(-39,  30),   S(-88,  80),   S( -8,  34),
            S(-55,  68),   S(-20,  41),   S(-31,  46),   S(-24,  50),   S(-52,  67),   S(-59,  78),   S(-27,  63),   S(-69,  97),
            S(-38,  60),   S( 20,   1),   S( 34,  29),   S( 22,  28),   S(-18,  41),   S(-12,  89),   S(-26,  69),   S(-40,  99),
            S(101,  17),   S( 71,   4),   S( 52,  20),   S( 31,  46),   S( 11,  28),   S(  1,  69),   S( 25,  59),   S( 22,  68),
            S( 42,  74),   S( 32,  42),   S(  9,  57),   S( 49,  55),   S( 30,  50),   S( 33,  65),   S( 35,  90),   S( 35,  92),
            S( 80,   3),   S( 13,  -3),   S( 25,  -5),   S(-22,   0),   S( 39,  20),   S( 49,  46),   S( 63,  39),   S( 57,  47),

            /* rooks: bucket 2 */
            S(-67,  93),   S(-44,  74),   S(-46,  76),   S(-42,  46),   S(-29,  54),   S(-40,  52),   S(-35,  39),   S(-75,  74),
            S(-60,  83),   S(-58,  72),   S(-49,  75),   S(-59,  59),   S(-57,  65),   S(-60,  47),   S(-18,  16),   S(-53,  66),
            S(-48,  75),   S(-35,  69),   S(-63,  71),   S(-37,  55),   S(-52,  60),   S(-31,  43),   S(-35,  45),   S(-23,  63),
            S(-55,  99),   S(-21,  78),   S(-42,  79),   S(-64,  79),   S(-45,  73),   S(-40,  63),   S(-40,  60),   S(-37,  67),
            S(-26, 105),   S(-24,  98),   S(-10,  99),   S(-37,  72),   S(-45,  89),   S(  6,  70),   S( -6,  70),   S(-21,  99),
            S(-14, 111),   S(  8,  95),   S( 37,  76),   S(-15,  71),   S( 26,  43),   S( 29,  66),   S( 69,  39),   S( 53,  72),
            S( 59,  93),   S( 14, 105),   S( 37,  83),   S( 58,  54),   S( 48,  38),   S( 60,  89),   S(-20, 102),   S( -1, 112),
            S( 36,  70),   S( 45,  72),   S( 34,  52),   S( 16,  49),   S(-34,  37),   S( 32,  52),   S(  8,  43),   S( 59,  53),

            /* rooks: bucket 3 */
            S(-12, 108),   S( -3, 109),   S(-15, 138),   S( -8, 125),   S(  4,  89),   S( 11,  84),   S( 27,  63),   S( -4,  47),
            S( -1, 104),   S(-15, 114),   S(-18, 141),   S( -8, 129),   S( -2,  96),   S( 11,  59),   S( 49,  32),   S( 22,  57),
            S( 17,  93),   S(-13, 120),   S(-19, 128),   S( -7, 133),   S(  8,  88),   S( 14,  72),   S( 36,  73),   S( 35,  57),
            S(-12, 139),   S( -3, 146),   S(-21, 151),   S( -7, 141),   S( -4, 112),   S(  9,  95),   S( 26,  87),   S( 11,  73),
            S(-14, 150),   S(-10, 155),   S(  8, 157),   S(  7, 146),   S( 19, 122),   S( 28, 111),   S( 61, 100),   S( 47,  94),
            S( -6, 156),   S(  8, 147),   S( 18, 155),   S( 30, 142),   S( 88,  92),   S(109,  88),   S( 76, 111),   S( 37,  86),
            S(  7, 163),   S( -6, 172),   S( 27, 166),   S(  6, 168),   S( 42, 151),   S(118,  90),   S(128, 159),   S(197, 107),
            S(111,  39),   S( 38, 114),   S( 36, 142),   S( 34, 131),   S( 42, 113),   S( 85, 102),   S( 73,  75),   S(140,  63),

            /* rooks: bucket 4 */
            S(-20, -29),   S(  5, -19),   S(  8, -32),   S( -5, -14),   S(-42,  15),   S(-15,  43),   S(-31,  -4),   S(-99,  44),
            S( -4, -41),   S(-32, -21),   S( 13, -34),   S( -3, -23),   S( -3,   8),   S( -9,  15),   S( -5,   1),   S(  9,  27),
            S(-22, -21),   S( -9, -28),   S(-18, -18),   S(-25, -26),   S(-52,  -1),   S(-36,   8),   S(  2, -20),   S(-60,  12),
            S(-60, -34),   S( 47,  -4),   S(-32, -13),   S(  6, -34),   S( 10,  10),   S(-18,  13),   S(-34,  15),   S( -7,   8),
            S(-11,  -8),   S( 49, -25),   S( 35,  12),   S( 62, -11),   S( 74,  -4),   S( 41,  40),   S( 19,  18),   S(  5,  20),
            S(-10, -42),   S(  4,  -7),   S( 12, -23),   S( 39,  25),   S( 48,  16),   S(  3,  22),   S( 14,  34),   S( 39,  16),
            S(-25, -26),   S( 40,  22),   S( 54,  -9),   S( 58, -27),   S( 28,  20),   S( 30,  -9),   S(  4,  -8),   S(-16,  26),
            S( 24, -13),   S( 25, -12),   S( 42, -16),   S(  6,  -5),   S( 48,  10),   S( 20,   2),   S(-13,  12),   S( -1,  18),

            /* rooks: bucket 5 */
            S(-52,  32),   S(-11,  -8),   S(  2,  -2),   S( 18,  -2),   S(  3,   6),   S(-10,  22),   S(-27,  36),   S(-63,  48),
            S(-36,  -2),   S(-42, -10),   S( 14, -31),   S( 36, -24),   S(  5, -18),   S(-10,   0),   S(-64,  29),   S(-23,  22),
            S(-78,  32),   S(-40, -10),   S(  2, -28),   S(-24,  -3),   S(-26,  -3),   S(  6,  -7),   S(-74,  34),   S(-52,  24),
            S(-82,  25),   S(  1,  -2),   S( 12, -15),   S( 15,  10),   S( 19,  -7),   S(-22,  26),   S(-11,  14),   S( -5,  25),
            S( 44,  12),   S( -4,  21),   S(-11,  25),   S(  8, -21),   S(-20,  18),   S( 65,  -2),   S( -1,  29),   S( 41,  23),
            S( 37,  12),   S( 14,  -8),   S( -4, -12),   S(-16, -16),   S( 12, -11),   S( 42,  24),   S( 94,   9),   S( 53,  25),
            S( 22,   1),   S( 52, -19),   S(  4,  11),   S( 18,  20),   S( 48, -16),   S( 49, -20),   S(122, -36),   S( 72,  -8),
            S( 16,  15),   S( -4,  11),   S( 37, -15),   S(  9,  32),   S( 71,   2),   S( 57,  14),   S( 35,  31),   S(  4,  51),

            /* rooks: bucket 6 */
            S(-69,  41),   S(-13,  11),   S(-24,  21),   S(-30,  12),   S(  7,  -5),   S(-11,   6),   S(-11,   6),   S(-57,  24),
            S(-79,  38),   S(-19,  17),   S(-23,  18),   S(-14,   2),   S( 13, -13),   S(-28,  -2),   S(-37,  -2),   S(-38,  39),
            S(-79,  44),   S(  8,   9),   S(-18,   8),   S(-28,  13),   S(-53,  18),   S( 26, -12),   S(  2, -12),   S( -6, -12),
            S(-52,  46),   S(-16,  29),   S( -9,  21),   S( 51,  -9),   S( -2,  13),   S(-10,  10),   S(-29,   7),   S( 26,  13),
            S(-37,  52),   S( 66,  11),   S(112,   0),   S( 43,   4),   S( 12,  -6),   S(  0,  24),   S( 35,   9),   S( 95,  -2),
            S( 92,   4),   S( 94,   1),   S(112,   1),   S( 52,  -8),   S(  8,  -8),   S( 27,  42),   S( 64, -19),   S( 61,   3),
            S( 18,  15),   S(107, -18),   S(108, -33),   S(110, -37),   S( 37,  -3),   S( 57,  -1),   S( 63, -22),   S(105, -34),
            S( 25,  14),   S( 16,  38),   S( -4,  23),   S( 69, -14),   S( 19,  13),   S( 25,  38),   S( 74,   0),   S( 66,  16),

            /* rooks: bucket 7 */
            S(-109,  38),  S(-89,  52),   S(-74,  47),   S(-50,  30),   S(-56,  25),   S(-26,  -6),   S(-32,   4),   S(-63,  -2),
            S(-88,  44),   S(-52,  18),   S(-60,  25),   S(-82,  35),   S(-52,   0),   S( -9, -18),   S( -3,   5),   S( 25, -63),
            S(-83,  23),   S(-91,  43),   S(-64,  20),   S(-60,  34),   S(-63,  23),   S(-40,  17),   S( 55, -18),   S(-19, -21),
            S(-69,  34),   S(-55,  29),   S( -9,   9),   S( 53, -15),   S( -2,   5),   S( 46,  -9),   S( 53,  -7),   S(  9, -23),
            S( 24,  22),   S( -2,  30),   S( 42,  18),   S( 72,  -2),   S(108, -20),   S(107, -34),   S( 98, -13),   S(-16, -32),
            S(  4,  30),   S( 28,  17),   S(117,  -3),   S( 87, -14),   S( 93, -10),   S( 79,   3),   S( 28,  21),   S(-13, -11),
            S(-16,  11),   S( 22,  -2),   S( 63,  -8),   S( 83, -19),   S(127, -35),   S( 90, -29),   S( 53,  -6),   S( 33, -27),
            S(-52,  16),   S(-20,  29),   S( -2,  15),   S( 29,  15),   S( 52, -10),   S( 66,   0),   S( 77,   6),   S( 31,   3),

            /* rooks: bucket 8 */
            S(-16, -88),   S( 11, -48),   S( 49, -62),   S( 29, -26),   S( -6, -52),   S( -9, -19),   S(-17, -47),   S(  5, -18),
            S(-21, -77),   S(  0,  -9),   S( 17, -12),   S(-46, -68),   S(  5, -65),   S(-20, -52),   S( -6, -19),   S(-25, -48),
            S(  7,  14),   S(  6,  -7),   S( 19,  16),   S(-13, -14),   S( 15,  19),   S( 20,  16),   S(  3,  39),   S(-12, -36),
            S(-11, -35),   S(  1,  -8),   S(  5,  -2),   S( 23,   7),   S( 14,  19),   S( 26,  32),   S( 12,  22),   S(-19, -43),
            S( -6, -41),   S(  8,  17),   S( 16, -12),   S( 23,  17),   S(  6,   4),   S( -8, -18),   S( 15,  36),   S( -6,   1),
            S( -2, -29),   S( 21, -13),   S( 21, -24),   S(-19, -35),   S( 12,  59),   S(-12,  -1),   S(-11,  -5),   S( 19,  23),
            S(  8,  12),   S( 27,   3),   S( 10, -30),   S(  1, -37),   S( -1,  -3),   S( 11,   7),   S( -7,   8),   S(  4,  25),
            S(  7,  -3),   S( 24, -14),   S( -2, -21),   S( 24,  22),   S(-11,  -8),   S( 20,  63),   S( -4,  -2),   S(  4,  -4),

            /* rooks: bucket 9 */
            S(-25, -98),   S(-11, -95),   S(  0, -125),  S( 20, -71),   S(  3, -75),   S( 31, -81),   S(-13, -68),   S( 20, -81),
            S(-33, -76),   S(-19, -87),   S(-20, -64),   S(-32, -81),   S( -1, -80),   S( -9,  -2),   S(-27, -59),   S(-28, -86),
            S(-20, -14),   S( -2, -17),   S( 11, -16),   S( -1, -19),   S(  9, -47),   S( -3, -16),   S(  0, -17),   S( -1,   5),
            S( 13, -45),   S( 13, -25),   S(  2,  -8),   S( -1, -11),   S(  0, -50),   S( 36, -31),   S( 13, -14),   S(-17, -42),
            S( 11, -40),   S(-10, -47),   S( -1, -66),   S(  1, -23),   S( -7, -49),   S(  3, -20),   S(-13, -62),   S( -5, -39),
            S(  9, -28),   S(-37, -62),   S(-13, -47),   S( 15, -42),   S(  9, -33),   S(  1, -19),   S(-10, -20),   S(  4, -20),
            S( 20, -25),   S(  3, -54),   S(  9, -58),   S( -5,   4),   S( 12, -31),   S( 12, -27),   S( 14,   8),   S(-10, -41),
            S(-24, -22),   S(  2, -16),   S(  4, -20),   S( -1,   5),   S(  1, -24),   S( -1,  -6),   S(-14, -10),   S( -5,   2),

            /* rooks: bucket 10 */
            S( 13, -105),  S( -1, -58),   S( 13, -80),   S( 43, -92),   S(  5, -104),  S( 14, -111),  S( 24, -92),   S(-19, -88),
            S(-25, -73),   S(-32, -74),   S(-31, -51),   S(-15, -88),   S(-17, -97),   S(  1, -84),   S(  0, -71),   S(-45, -105),
            S(-24, -42),   S(-35, -24),   S(-33, -67),   S(-27, -74),   S(  3, -15),   S( -2, -37),   S( -2, -55),   S(-15, -34),
            S(-26, -57),   S(-17, -52),   S(  6, -39),   S(-17, -27),   S(  6,  -8),   S(  0, -11),   S( -9, -77),   S( 10, -50),
            S(  0, -35),   S( -6, -58),   S(  3, -47),   S(  4, -63),   S( 28, -27),   S( -4, -33),   S( 15, -80),   S(-22, -79),
            S(-20, -41),   S(-19, -32),   S(  8, -36),   S( -7, -53),   S( 26, -42),   S( 12, -42),   S(-17, -68),   S( 15, -58),
            S(-25, -39),   S(-10, -28),   S( -3, -69),   S( 15, -51),   S( 10, -26),   S(-12, -32),   S(-22, -82),   S( 13, -54),
            S(-22, -40),   S(-20,  -9),   S(  5,  -7),   S(-19, -31),   S(  3,   1),   S(-30, -19),   S( 16, -50),   S(-20, -13),

            /* rooks: bucket 11 */
            S(-14, -57),   S(-37, -34),   S( 17, -49),   S(-23, -35),   S(-29, -48),   S( 27, -63),   S( -1, -63),   S( -3, -95),
            S(  9, -25),   S(-18, -47),   S(-27, -40),   S(-42, -26),   S(-32, -14),   S(  9, -13),   S(-19, -60),   S(-31, -90),
            S(-24,  12),   S(-22, -15),   S(-16,  16),   S(-13, -15),   S(  6,  -6),   S( 11,  -9),   S( 30, -29),   S(-11, -16),
            S( -6, -38),   S(-21, -35),   S(  9, -13),   S( 20,   2),   S( 33,   2),   S(-18, -52),   S( 14,  25),   S( -8, -28),
            S( -2, -28),   S(-10, -23),   S( 23, -24),   S(  8,   8),   S( 38, -29),   S( 30, -32),   S( 26,  -1),   S(-28, -57),
            S( -7, -36),   S( -4,  -9),   S( 17, -29),   S(  7, -27),   S(-10, -25),   S( 19, -19),   S( 33,   7),   S(  5, -55),
            S( -7,  -5),   S(-18, -53),   S( -3, -12),   S(-16, -16),   S( -7, -17),   S( 35, -30),   S( 12, -29),   S(  8, -30),
            S(  1, -14),   S( 35,  17),   S(  0,  14),   S(  2,  -5),   S( 12,  15),   S(  7,  -3),   S( 48, -20),   S(-26,  -7),

            /* rooks: bucket 12 */
            S(-16, -71),   S( -7, -26),   S( -2, -29),   S(-14, -39),   S(-11, -58),   S(  3, -16),   S( -7, -23),   S(-26, -99),
            S( 11,   8),   S(  1,  -2),   S(  2,  -9),   S( 10,   9),   S( 12, -17),   S( 19,  -2),   S(  8, -12),   S(-13, -46),
            S( -2, -22),   S(  5,  20),   S( 14,  10),   S( 10,  -8),   S(  7, -31),   S(  5, -34),   S( 14,  44),   S( -3,   0),
            S( -2,   3),   S( -5, -13),   S( 13,  23),   S( 13,  22),   S(  5,  18),   S(  2,   0),   S(  9,  18),   S(  6,  11),
            S(  8, -29),   S( 11,   6),   S(  7,   0),   S( -5, -42),   S(  9,  32),   S( -8,  -9),   S( -8, -34),   S( -7, -20),
            S( -1, -26),   S( -3, -21),   S(  0, -32),   S( -7, -29),   S( 18,  32),   S(-11, -37),   S( 14,  20),   S(  1,  -2),
            S(-15, -23),   S(  1, -11),   S(  4, -19),   S( -6, -18),   S(-12, -37),   S(  9,  27),   S(  6,  24),   S( -2,  -4),
            S( -8, -18),   S( -4, -17),   S(  9, -26),   S( 10,  12),   S(  9,  15),   S( -4, -21),   S( -2,  -1),   S(  3,  24),

            /* rooks: bucket 13 */
            S(-30, -74),   S(-21, -67),   S(-31, -71),   S(  2, -28),   S(-23, -86),   S(-11, -70),   S(-30, -64),   S(-32, -75),
            S( -1, -25),   S( -6, -49),   S( -7, -25),   S(  1, -16),   S(  5,  -7),   S(  2, -41),   S(  9, -25),   S(-12, -70),
            S(-14, -53),   S(  2, -20),   S(  7,   3),   S(  4, -20),   S( 17,  11),   S( 11, -40),   S( 10, -23),   S( -4, -55),
            S(  4, -23),   S(-10, -49),   S(  4, -27),   S(  7,  -9),   S( 16,   0),   S(  2, -22),   S( -2, -15),   S(  0,  13),
            S( 10,   7),   S(  5, -46),   S(-14, -64),   S( 11, -21),   S(  5, -44),   S( -7, -41),   S(  2,   0),   S(-11, -29),
            S( -4, -13),   S(-16, -48),   S(-15, -57),   S(  3, -65),   S(  0, -76),   S( -8, -40),   S( -7, -25),   S(  7, -15),
            S( -1, -10),   S(  5, -38),   S(-13, -48),   S(  8,  10),   S(-12, -66),   S(  2,  -4),   S(  0, -14),   S( -7, -52),
            S( -4, -30),   S(-21, -44),   S( -8, -29),   S( 27,   5),   S( -1, -49),   S( -1, -27),   S(  6,   6),   S( -2, -31),

            /* rooks: bucket 14 */
            S( -6, -48),   S(-40, -70),   S(-16, -39),   S( -3, -71),   S(-19, -83),   S( -4, -62),   S(-29, -112),  S(-35, -101),
            S(  9, -10),   S( 17, -19),   S( 18,  -5),   S( -1, -35),   S( -1, -17),   S( -7, -23),   S( -1, -43),   S( -4, -46),
            S( -6, -22),   S( -6, -39),   S( -1, -36),   S( 10, -31),   S( -1, -23),   S( -3, -21),   S( 11, -47),   S(-28, -94),
            S(  1, -11),   S(  0,  -8),   S(  6,   3),   S( -7, -59),   S( -4, -40),   S( -6, -14),   S( 13, -35),   S(-11, -50),
            S(  0,  -6),   S(  7,  18),   S( -2,  -7),   S(  8, -38),   S(-14, -71),   S( 23, -11),   S(  0, -52),   S(-11, -36),
            S(  1, -11),   S( 13,  25),   S(  6, -25),   S(  2, -79),   S( 11, -44),   S(  2, -71),   S(-10, -78),   S(-10, -44),
            S(-15, -35),   S( -3,  -6),   S(-12, -65),   S(-27, -105),  S( -3, -37),   S(  5, -16),   S( -8, -62),   S(  0, -23),
            S( -9, -31),   S(  1,   2),   S( -3, -32),   S(  2, -30),   S(-15, -77),   S(-13, -98),   S(  1, -48),   S(-10, -34),

            /* rooks: bucket 15 */
            S(-11, -48),   S(-11, -71),   S(-36, -56),   S(-22, -58),   S( -1, -31),   S(-19, -81),   S(  0,  -9),   S(-18, -67),
            S(  2, -18),   S(-14, -44),   S( -1, -50),   S(  4, -16),   S( -4, -38),   S(  0, -18),   S( 16,  24),   S(  0,  -9),
            S( -7,  -5),   S(  5, -21),   S(  2, -12),   S( 14, -17),   S( 14, -21),   S( -7, -36),   S( 18,  16),   S(  1, -11),
            S( -3,  -3),   S( -7, -10),   S(  7,  15),   S( -6,   0),   S(  6,   5),   S( -3, -28),   S(  6, -28),   S( -1, -33),
            S(  0,  -9),   S( -3, -20),   S(  1, -12),   S( -2,  -3),   S(  9,  15),   S( -2, -41),   S( 14,  -8),   S( -1, -50),
            S(  5,  26),   S(  9,  35),   S(  3,  17),   S(  6,   8),   S( -5, -39),   S( 10, -32),   S( 12, -11),   S(  1,  14),
            S( -1, -24),   S(-10, -24),   S(  7,   6),   S( -2, -25),   S( -9, -36),   S(-10, -55),   S( -5, -51),   S(-12, -57),
            S(  7,  13),   S(  3,   8),   S(  2,  12),   S(  1,   1),   S( -2,  -5),   S( -2, -48),   S(  3, -47),   S(-22, -62),

            /* queens: bucket 0 */
            S(  5, -40),   S(-22, -47),   S(-29, -95),   S( -3, -104),  S( -5, -70),   S(  9, -70),   S(-46, -49),   S(-29, -26),
            S( -4, -61),   S(  6, -76),   S(  6, -83),   S(-10, -23),   S( -4, -28),   S( -5, -45),   S(-32, -47),   S(-35, -22),
            S( -8,   8),   S( -3, -31),   S( 27, -55),   S( -9, -21),   S(-10, -17),   S(  6, -44),   S( -4, -56),   S(-94, -95),
            S(-45,  66),   S( 26, -20),   S(-16,  38),   S( -1,  51),   S(  3,  30),   S(-17,  18),   S(-15, -12),   S(-19, -45),
            S(-24,  27),   S(-18,  76),   S( 11,  51),   S(-34, 109),   S(-15,  77),   S(-40, 105),   S(-65,  60),   S(-53,   2),
            S(-43,  14),   S( 23,  77),   S( 16,  45),   S(-23,  83),   S(-72,  85),   S(-65,  73),   S(-57,  19),   S(-67, -12),
            S(  0,   0),   S(  0,   0),   S( 23,  28),   S(-31,  54),   S(-23,  26),   S(-65,  76),   S(-83,  58),   S(-108,  30),
            S(  0,   0),   S(  0,   0),   S( 16,  31),   S( -5,  -8),   S(-23,  47),   S(-56,  10),   S(-35,   2),   S(-35, -21),

            /* queens: bucket 1 */
            S( 32, -36),   S(-18, -23),   S(  8, -91),   S( 14, -106),  S( 16, -50),   S( 22, -71),   S( 17, -50),   S(-24,  11),
            S(-49,  26),   S( 24, -29),   S( 24, -57),   S( 21, -32),   S( 24, -32),   S( -6, -27),   S(-45,  24),   S(-89, -22),
            S( 35, -12),   S( 15, -24),   S( 27, -12),   S(  2,  18),   S(-25,  51),   S( 15,  17),   S(-13,  34),   S(  0,   1),
            S( 27, -23),   S(-17,  57),   S(  0,  36),   S( 23,  70),   S(  9,  68),   S( -5,  46),   S( 26,  26),   S(-14,  34),
            S( 18,  25),   S( 30,  54),   S( 24,  96),   S( 20,  74),   S( 10, 110),   S( 66,  13),   S(-11,  53),   S(  0,  47),
            S( 64,  21),   S( 90,  17),   S( 81,  65),   S(109, 114),   S( 72,  62),   S(  2,  97),   S( 27,  70),   S( -7,  29),
            S(105, -33),   S( 72,  -7),   S(  0,   0),   S(  0,   0),   S( 14,  89),   S(-36,  60),   S(-35,  82),   S(-53,  61),
            S( 71,  18),   S( 45,  25),   S(  0,   0),   S(  0,   0),   S( 19,  23),   S( 63,  50),   S(100,  16),   S(-20,  57),

            /* queens: bucket 2 */
            S( 12, -32),   S( 28,  -2),   S( 31,  -8),   S( 39, -34),   S( 45, -56),   S( 36, -80),   S(  2, -58),   S( 25,  22),
            S( 23,   7),   S( -2,  44),   S( 38, -20),   S( 39,  -1),   S( 46, -19),   S( 25,   2),   S( 23,   6),   S( 20,  11),
            S( 38,  39),   S( 25,  26),   S( 15,  84),   S( 11,  61),   S( 27,  35),   S( 17,  55),   S( 31,  20),   S( 27,  36),
            S( 30,  16),   S( 24,  88),   S( 12,  99),   S( 21, 106),   S( 28,  83),   S( 13, 112),   S( 33,  77),   S( 25,  87),
            S(  6,  60),   S( 20,  35),   S(  9, 103),   S( 18, 121),   S( 38, 120),   S( 98,  41),   S( 52, 117),   S( 78,  57),
            S(-33, 105),   S(-25,  96),   S( 19,  91),   S( 68,  91),   S( 86,  67),   S( 92, 119),   S(138,  43),   S( 37, 126),
            S(-23,  90),   S(-22, 111),   S(  1, 101),   S( 82,  84),   S(  0,   0),   S(  0,   0),   S( 27, 134),   S( 46, 111),
            S( 12,  35),   S( 56,  15),   S( 75,  27),   S( 37,  69),   S(  0,   0),   S(  0,   0),   S( 67,  67),   S( 17, 105),

            /* queens: bucket 3 */
            S(-37,  64),   S( -8,  35),   S( -6,  37),   S(  6,  51),   S( -2,  18),   S(  1,  -5),   S(-19, -31),   S(-33,  26),
            S(-32,  48),   S(-18,  46),   S( -5,  61),   S(  3,  79),   S(  4,  58),   S( 12,  31),   S( 35,  -7),   S( 47, -49),
            S(-23,  69),   S(-17,  92),   S( -6, 113),   S(-25, 150),   S(-10, 102),   S( -8, 107),   S(  7,  70),   S(  1,  50),
            S(-19,  68),   S(-34, 136),   S(-30, 157),   S(  0, 154),   S(-30, 177),   S( -2, 124),   S( 19, 110),   S( -7,  99),
            S(-26, 105),   S(-36, 147),   S(-20, 153),   S(-21, 195),   S( -6, 178),   S( 11, 189),   S(  3, 164),   S( -8, 131),
            S(-33, 104),   S(-28, 135),   S(-46, 178),   S(-40, 191),   S(-23, 202),   S( 14, 181),   S(-15, 219),   S(-15, 191),
            S(-93, 139),   S(-83, 171),   S(-80, 198),   S(-81, 208),   S(-79, 227),   S(  7, 132),   S(  0,   0),   S(  0,   0),
            S(-138, 199),  S(-96, 153),   S(-60, 142),   S(-66, 160),   S(-46, 167),   S( 10, 120),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-51,  -1),   S(-25, -48),   S(  4,  30),   S(-27, -18),   S(-27,  -7),   S(-17, -26),   S(-43, -40),   S( 15, -11),
            S(-30, -22),   S(  2,   9),   S( -1,  -6),   S(-33, -31),   S(-45,  17),   S(-32,  16),   S(-69, -32),   S( -6,  -5),
            S( 13,  40),   S( 13, -49),   S(  6,   8),   S(-12,   0),   S( 39,  22),   S(  6, -14),   S(-24, -27),   S( 44,  13),
            S( 17, -10),   S( 29,   9),   S( 12,  14),   S( -6,  24),   S( 67,  53),   S( -7,  12),   S(-57, -26),   S(-22,  -9),
            S(  0,   0),   S(  0,   0),   S( 39,  29),   S( 72,  50),   S( 22,  68),   S(  7,  30),   S( -3,   8),   S( -8,   8),
            S(  0,   0),   S(  0,   0),   S( 38,  49),   S( 17,  24),   S( 17,  26),   S( 47,  31),   S( 11,  17),   S(-30,  -2),
            S( 32,   6),   S( 21,  31),   S( 64,  23),   S( 65,  47),   S( 44,  10),   S(  9,  33),   S(-16,  24),   S(-38,  -2),
            S( 54,  30),   S( 16,  15),   S( 19,   5),   S( 49,  36),   S( 12,   4),   S(-23, -26),   S(-47, -49),   S(-25, -29),

            /* queens: bucket 5 */
            S( 33,  11),   S( 25,  -4),   S( -6,   1),   S(  4,  14),   S( 25,   3),   S( 24,  30),   S(  9,   4),   S( 10,  -3),
            S(  0,  20),   S( 10,  -8),   S( 32,  -3),   S( 11,  17),   S( 10,  19),   S(-41, -15),   S( 16, -15),   S( 38,  25),
            S( -7, -22),   S( 29,   3),   S( 23,  27),   S(-34,  17),   S( 35,  35),   S( 28,  36),   S(  2,  32),   S(-15,   6),
            S(  5, -24),   S( 35,  16),   S( 27,   9),   S( 33,  23),   S( 70,  36),   S( 37,  13),   S(  0,  36),   S(-25,  17),
            S( 49,   5),   S( 63,  36),   S(  0,   0),   S(  0,   0),   S( 36,  35),   S( 28,   8),   S( 55,  23),   S(-28,   9),
            S( 20,  24),   S( 59,  53),   S(  0,   0),   S(  0,   0),   S( 32,  26),   S( 61,  26),   S( 19,  -3),   S( 48,  48),
            S( 82,  33),   S( 84,   9),   S( 63,  65),   S( 29,  46),   S( 60,  55),   S(137,  56),   S( 37,  64),   S( 15,  -9),
            S( 57,  50),   S( 57,  56),   S( 89,  42),   S( 40,  33),   S( 53,  36),   S( 94,  67),   S( 22,  27),   S( 33,  17),

            /* queens: bucket 6 */
            S( 37,  36),   S(-31, -41),   S(  3, -11),   S( -4,  16),   S( -3,   5),   S(-40,   4),   S( -9, -21),   S( 21,  12),
            S( 20,  15),   S( 33,  -3),   S( 29,  29),   S( 39,  37),   S( 38,  -8),   S( -5,  -7),   S( -3,   6),   S(  3,   0),
            S(-27,  37),   S( 13,  28),   S(-14,  58),   S( 43,  -4),   S(  0,  -4),   S( 23, -12),   S( 70,  31),   S( 77,  58),
            S(-39,  -5),   S(-33,  18),   S( 24,  45),   S( 77,  28),   S( 29, -10),   S( 30,  15),   S( 88,  31),   S(112,  48),
            S( 10,  24),   S(  7,  43),   S( 35,  60),   S( 57,  35),   S(  0,   0),   S(  0,   0),   S( 62,  42),   S(112,  56),
            S( 17,  42),   S( 50,  51),   S( 58,  67),   S( 44,  28),   S(  0,   0),   S(  0,   0),   S( 59,  45),   S( 76,  48),
            S( 34,   7),   S(-12,  31),   S( 94,  13),   S( 85,  45),   S( 30,  30),   S( 83,  70),   S(126,  24),   S(132,  10),
            S(  2,   7),   S( 24,  12),   S( 84,  35),   S( 95,  52),   S(122,  56),   S( 98,  51),   S(127,  60),   S( 84,  -7),

            /* queens: bucket 7 */
            S(-13, -34),   S(  0, -22),   S(-37,  15),   S(  3, -26),   S( 12, -21),   S(-43,   9),   S(-14,  -6),   S(-29, -48),
            S(-46,  -8),   S(-61,  -3),   S(-37,  38),   S(-22,  47),   S(-32,  27),   S(-23,  31),   S(  9,  -2),   S(-52, -29),
            S(-31,  17),   S(-42, -11),   S(-27,  55),   S( 52,   8),   S( 28,  18),   S( 32,  -2),   S( 30,  -8),   S( 78, -22),
            S(-66,  27),   S(-10,  -4),   S( 15,  14),   S( 23,  59),   S( 80,  26),   S( 69,  26),   S( 52, -11),   S( 25, -15),
            S(-52,  -3),   S(-16,  12),   S( -8,  77),   S( 35,  67),   S( 61,  24),   S( 82,  20),   S(  0,   0),   S(  0,   0),
            S( -1,  -4),   S( -6,  35),   S( -1,  51),   S(-13,  59),   S( 54,  15),   S(118,  64),   S(  0,   0),   S(  0,   0),
            S(-42,   7),   S(-18,  -3),   S(-35,  21),   S( 43,  38),   S( 64,  42),   S( 74,  23),   S( 76,  36),   S( 82,  43),
            S(  4, -15),   S( 38, -34),   S( 28,  34),   S( 43,  -9),   S( 66,  22),   S( 44,  25),   S( 13,   2),   S( 72,   7),

            /* queens: bucket 8 */
            S( -2, -15),   S( -2, -36),   S( -8, -44),   S(  4,   2),   S(  3,  -7),   S( 21,  22),   S(  3,  -9),   S( -5, -12),
            S(-12, -24),   S(  2,  15),   S( 25,  13),   S(  4,   6),   S( 12,   4),   S(  1,   1),   S( 14,  28),   S(  8,  11),
            S(  0,   0),   S(  0,   0),   S( -2, -26),   S( -2, -15),   S( 18,  -2),   S(-12, -14),   S( -4,   1),   S( -4,   6),
            S(  0,   0),   S(  0,   0),   S( 17,  22),   S( 11,  28),   S( 15,  -2),   S( -6, -14),   S( -2,   3),   S( -4, -11),
            S(  6,   4),   S(  8,  18),   S( 14,  20),   S( 23,  10),   S(  3,  -4),   S(  8,   4),   S( 12,  -7),   S( -9, -15),
            S( 12,  15),   S(  0, -25),   S( 24,  32),   S( -2,  -5),   S( 11,  10),   S( 18,   8),   S( -2, -21),   S(  1, -10),
            S(  1, -13),   S(-14, -29),   S(  4,   5),   S( 25,  38),   S( 22, -10),   S( 12,  21),   S( -9, -33),   S(  4,  -1),
            S(  8,   0),   S( 13,  11),   S( 20,  17),   S( 14,   1),   S(  6,  -5),   S(-21, -50),   S(-16, -27),   S(-42, -76),

            /* queens: bucket 9 */
            S( 16,   7),   S(-27, -44),   S( -5, -21),   S( 27,  24),   S( -7, -37),   S(  9,  -2),   S( -9, -21),   S( -8, -28),
            S( 30,  18),   S( 12,   0),   S(  1, -10),   S( 19,  15),   S(-15, -41),   S( -2, -16),   S(  6,   6),   S( -5, -18),
            S(-28, -70),   S(  3,   4),   S(  0,   0),   S(  0,   0),   S( 11,   9),   S( 25,  24),   S( -9, -14),   S(  7,   3),
            S( 13,   8),   S( -7, -19),   S(  0,   0),   S(  0,   0),   S(  4,   4),   S( 28,  21),   S(  8, -14),   S(-11, -11),
            S( 23,  12),   S( 21,   9),   S(  3,   6),   S(-11, -19),   S( -3, -28),   S( 21,   8),   S(  3, -26),   S(  7,  -4),
            S( 13,  14),   S( 11,   6),   S(  4,  -3),   S(  9,  -8),   S(  7, -15),   S( -7, -28),   S( -2, -15),   S( -3, -15),
            S( 19,  21),   S( 25,   8),   S( -2,  -3),   S( -7,  -8),   S( 29,   4),   S( 25,  17),   S( 11,  16),   S( -3, -25),
            S(  5,  -7),   S( 17,  -5),   S( -8, -16),   S(  2,  -7),   S( -3, -10),   S( -4, -14),   S(-14, -51),   S(  4, -12),

            /* queens: bucket 10 */
            S(  6,   0),   S(  9,   5),   S( -7,  -9),   S(  8,   3),   S( -4, -20),   S(  7,   1),   S(  0,  -2),   S( -2, -15),
            S(  7,   5),   S(-13, -42),   S(  4, -18),   S( -4, -22),   S( -7, -14),   S( 14,   3),   S( 11,  -8),   S( -4, -16),
            S(  9,  14),   S(  2,  -4),   S( 12,   1),   S(  2, -10),   S(  0,   0),   S(  0,   0),   S(  5,   0),   S(-13, -31),
            S(-10, -21),   S( 19,  -2),   S(  1,  -8),   S( 16,  25),   S(  0,   0),   S(  0,   0),   S(  4, -14),   S( 18,   2),
            S(  6,  13),   S( 21,  18),   S( -2, -20),   S( 43,  47),   S( 10,  15),   S(  3,  -3),   S(  2, -17),   S( 35,  15),
            S(-10, -20),   S( -4, -19),   S( 32,  32),   S(  3,  20),   S( 24,  10),   S( 25,  39),   S( 17,  -1),   S( 12, -22),
            S(  4,   0),   S( 18,  27),   S( 23,  34),   S( 19,  21),   S(  9,   6),   S( 23,  20),   S( 12,  -5),   S( -1, -19),
            S(-21, -40),   S( -6, -17),   S(  7, -31),   S( -7, -46),   S( 22,  29),   S(-19, -32),   S(  6, -13),   S(-10, -38),

            /* queens: bucket 11 */
            S(-27, -21),   S(-10, -22),   S(  6,   1),   S( -8, -25),   S( -9,  -8),   S(-17, -24),   S( 11,  -4),   S( -2, -12),
            S( -3,  -8),   S( -4,  -3),   S(-21, -14),   S( -1, -17),   S( 34,  -2),   S( 10,  14),   S( 14,  -1),   S( 13,   1),
            S( -6,  -2),   S( -1,   3),   S(-21, -15),   S( 14, -17),   S(-11, -19),   S(-33, -40),   S(  0,   0),   S(  0,   0),
            S( -3, -16),   S(-24, -22),   S( -3,  -1),   S( 13,   4),   S(  5,   5),   S( -3,  -8),   S(  0,   0),   S(  0,   0),
            S( -4, -10),   S( 12,  12),   S(  2,  20),   S( 12,   6),   S( 51,  62),   S( 31,  36),   S( 11,  15),   S(-10, -24),
            S(-15, -18),   S(-10, -16),   S(-30, -38),   S( 20,  21),   S( 16,  24),   S(  9,  -2),   S( 19,   4),   S(  7, -45),
            S(-15, -14),   S(-19, -24),   S( 12,  19),   S( -1,  10),   S(  9,  22),   S( 10,  13),   S(  0,  11),   S( 17,  -2),
            S(-43, -83),   S( -3, -24),   S(-31, -40),   S( 29,  28),   S(-11, -21),   S( 13,   6),   S( -6, -23),   S(  9,   2),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 11,  15),   S( -9, -22),   S(-10,  -3),   S( -4, -19),   S(  2,  -2),   S(  1,   6),
            S(  0,   0),   S(  0,   0),   S(  7,   5),   S( -7, -20),   S(  2, -11),   S(-12, -35),   S( -9, -16),   S(  0,  -7),
            S( -4,  -8),   S(  6,   0),   S( -6, -11),   S( -9, -26),   S( 36,  66),   S(  4,  13),   S(  1,  -7),   S(  4,   6),
            S(  5,  -8),   S( 24,  29),   S( 10,  20),   S( 13,  19),   S(  4,   7),   S(-16, -29),   S(  7,  14),   S(-10, -20),
            S(-19, -41),   S(  9,  16),   S( -3, -16),   S(-16, -36),   S( -7,  -9),   S(-31, -74),   S( -5, -19),   S( -4,  -8),
            S(  5,   4),   S( -7, -15),   S( -5,  -9),   S(-11, -23),   S(-13, -20),   S(-16, -26),   S(-19, -29),   S( -9, -21),
            S( -9, -16),   S( 14,  27),   S( -3, -13),   S(  0,  -7),   S( -2,  -3),   S(-19, -46),   S(  4,   1),   S( -6,  -9),
            S(  6,   4),   S( -6, -14),   S( -3,   4),   S(  0,  -2),   S(-16, -32),   S( -9, -25),   S( -7, -11),   S( -8, -17),

            /* queens: bucket 13 */
            S( -4, -18),   S( -8, -14),   S(  0,   0),   S(  0,   0),   S(-14, -29),   S(  2,  -1),   S( -1, -18),   S( -7, -11),
            S( -5, -23),   S( -6, -16),   S(  0,   0),   S(  0,   0),   S( -1,  -5),   S(-20, -40),   S( -9, -18),   S( -2,  -9),
            S( -6, -19),   S(  2,  -8),   S( -2,  -6),   S( -5, -11),   S(-12, -34),   S( -5,  -5),   S(-15, -29),   S(  4,   8),
            S( -7, -18),   S(-17, -50),   S( 20,  22),   S(  3, -15),   S( 29,  50),   S( 20,  28),   S( -6, -22),   S( -2, -11),
            S( 19,  26),   S(-14, -42),   S(-17, -44),   S( 12,  16),   S( -3, -10),   S( -3, -16),   S( -6, -13),   S(  1,  -1),
            S(  9,   7),   S(-17, -45),   S( -4,  -4),   S(  7,   7),   S(  3,   1),   S( -3,   0),   S(-20, -37),   S(-15, -45),
            S(  7,  12),   S( -8, -21),   S( -2,  -2),   S( -3, -13),   S( -8, -22),   S( 11,   7),   S(-11, -33),   S( -7, -16),
            S(-13, -33),   S( -7, -15),   S( -3,  -6),   S(  4,   8),   S( -4,  -8),   S(  4,  -4),   S(  6,  18),   S(-22, -34),

            /* queens: bucket 14 */
            S( -1, -17),   S(  7,   3),   S( -4, -14),   S(  3,  -7),   S(  0,   0),   S(  0,   0),   S( -1, -12),   S( -9, -30),
            S(-13, -39),   S(-18, -42),   S( -6, -20),   S( -6, -20),   S(  0,   0),   S(  0,   0),   S(  5,   8),   S(-17, -39),
            S( -6, -13),   S( -6, -26),   S( -7, -14),   S( -3, -10),   S(  5,  15),   S(  9,  17),   S( -7, -17),   S(-17, -48),
            S( -5, -11),   S( -4,  -7),   S(  5,   3),   S(-12, -21),   S( -8, -24),   S(-18, -43),   S( -4, -22),   S( -6, -19),
            S( -5,  -6),   S( 16,  23),   S(-19, -46),   S(  9,  19),   S( 21,  31),   S( -8, -13),   S(-11, -24),   S( -4, -20),
            S( -6, -12),   S( -1, -22),   S(-12, -23),   S(-24, -55),   S(  8,  15),   S(  6,  15),   S( -7, -13),   S(-16, -24),
            S(-14, -28),   S( -6, -22),   S( -5, -16),   S(  0,  -1),   S( -4,  -3),   S(  1,  -5),   S( -8, -21),   S( -8, -15),
            S(-10, -23),   S(  2, -15),   S(-13, -21),   S(-13, -30),   S(  5,   6),   S( -9, -21),   S(-10, -13),   S( -5, -11),

            /* queens: bucket 15 */
            S( -7, -19),   S( -3, -10),   S(  7,   0),   S(-17, -27),   S( 13,  25),   S(-12, -26),   S(  0,   0),   S(  0,   0),
            S( -1,  -4),   S(  4,   3),   S(-11, -25),   S(-14, -41),   S( -6, -13),   S(  2,   5),   S(  0,   0),   S(  0,   0),
            S( -3, -10),   S(  7,   4),   S(-20, -36),   S( -7, -10),   S(-19, -44),   S( -1,  -7),   S(  4,   5),   S( -2,  -7),
            S( -4,  -7),   S(-12, -24),   S( -8, -23),   S(  9,  19),   S(  6,  12),   S(  7,  19),   S( -2,  -7),   S( -3, -15),
            S(  0,   2),   S(  6,  11),   S(-10, -19),   S( -5, -12),   S(  9,  28),   S( 15,  28),   S(  3,   0),   S(-14, -31),
            S(  5,   5),   S(-10, -18),   S(-16, -32),   S(-15, -37),   S(  4,  13),   S(-10, -19),   S(  1,  -1),   S( -4, -14),
            S( -5, -15),   S(-10, -23),   S( -9, -13),   S( -2,  -2),   S( -9, -19),   S( -2,   0),   S(  3,  10),   S(-14, -24),
            S( -8, -17),   S(-17, -38),   S(  2, -12),   S( -4,  -5),   S( -7,  -3),   S( -8, -17),   S(  3,   8),   S(-10, -19),

            /* kings: bucket 0 */
            S(-14,  -1),   S( 21,  -8),   S( 17, -14),   S(-20,  11),   S(-19,  -3),   S( 26, -30),   S(  7,   7),   S( 22, -52),
            S(-16,  15),   S(  9,  -5),   S(-14,  15),   S(-62,  21),   S(-33,  35),   S(-13,  18),   S(-18,  53),   S(-14,  27),
            S( 27,  -7),   S( 23,  -7),   S( -5,  -7),   S(-15,   0),   S(-16,  -4),   S( -9, -10),   S(-38,  19),   S( 10, -28),
            S(-26, -25),   S(  7, -20),   S(  9, -11),   S(-32,   4),   S(-44,  31),   S(-29,   7),   S(-47,  19),   S( -9,  22),
            S(-52, -89),   S( 19, -26),   S( 14, -30),   S( -4, -21),   S(-16,  -8),   S(-18,  26),   S(-22,  13),   S( 11,  10),
            S( -7, -101),  S( 26, -29),   S( 36, -59),   S(  5, -23),   S(  5, -19),   S( 22, -44),   S(  2, -13),   S(-20, -12),
            S(  0,   0),   S(  0,   0),   S(  7, -24),   S( 15, -15),   S( 33, -21),   S( 11,  -3),   S( 15, -14),   S(-18, -34),
            S(  0,   0),   S(  0,   0),   S( -1, -57),   S(  9, -42),   S( 10, -12),   S( 13, -17),   S( 12,  11),   S( -1,  -7),

            /* kings: bucket 1 */
            S( 34, -38),   S( 41, -25),   S( 10, -11),   S( 27,  -7),   S(-11,  -2),   S( 29, -12),   S( 16,  15),   S( 25, -11),
            S( -7,  10),   S(  7,  19),   S( 12, -12),   S(-51,  30),   S(-26,  23),   S(  2,   7),   S(  0,  26),   S( -8,  18),
            S(-23,  -7),   S( 17,  -9),   S( 13, -18),   S( 26, -18),   S(-37,   3),   S(  0, -23),   S(  8,   3),   S( 56, -27),
            S(  9, -12),   S( 33, -12),   S(  8,  -6),   S(-27,  12),   S( -5,  19),   S(-31,   9),   S(  5,  18),   S(-47,  27),
            S(-15, -29),   S( 16, -20),   S( 20, -42),   S( 17,  -3),   S(  3, -11),   S(-14, -14),   S(  5,  -1),   S(  0,  14),
            S( 18, -12),   S( 30, -36),   S( 23, -31),   S( 26, -26),   S(  9, -13),   S(  9,  -1),   S( 26,  20),   S( -7, -11),
            S(-13, -32),   S(  1,   4),   S(  0,   0),   S(  0,   0),   S( -5,  15),   S(-13,  10),   S( -4,  52),   S( -5, -35),
            S(-21, -119),  S( -6,  -6),   S(  0,   0),   S(  0,   0),   S( -2, -34),   S( 11,  11),   S( -2,  10),   S(-11, -37),

            /* kings: bucket 2 */
            S( 32, -66),   S(  9,   5),   S(  6, -16),   S( 37, -23),   S(-14,  11),   S( 39, -26),   S(  6,  24),   S( 26, -13),
            S( 20, -14),   S(-14,  41),   S(-18,  10),   S(-18,  12),   S(-28,  18),   S( -3,   1),   S(  8,  10),   S( -7,   7),
            S(-46,  -3),   S(-12,   0),   S(-27,  -7),   S(  2, -17),   S(-20,   1),   S(  1, -25),   S( 40, -20),   S(  9, -12),
            S( -2,  26),   S(-11,  -1),   S( -8,   7),   S(-16,  10),   S( 23,   0),   S(-21,  -4),   S( 17, -18),   S( 46, -21),
            S(-13,  -1),   S( 22, -10),   S( -7, -24),   S( 25, -12),   S( 56, -41),   S(-15, -35),   S( 68, -36),   S( -3, -26),
            S(  6,   6),   S( 14, -10),   S( 38, -22),   S( 56, -26),   S( 67, -25),   S( 27,  14),   S( 74, -45),   S( 26, -32),
            S(-14, -47),   S(  0,  29),   S(-28, -23),   S( 25,   5),   S(  0,   0),   S(  0,   0),   S( 13,  18),   S(-24, -28),
            S( -5,  -1),   S(-18, -31),   S( -5, -42),   S( 18,  18),   S(  0,   0),   S(  0,   0),   S( -1,  -3),   S(-16, -120),

            /* kings: bucket 3 */
            S(  4, -66),   S( 10, -11),   S( 20, -43),   S(  2, -17),   S(-14, -21),   S( 39, -37),   S(  4,  15),   S(  9, -27),
            S(  2,  10),   S(-22,  37),   S(-24,   6),   S(-44,  11),   S(-57,  28),   S(  4,  -8),   S( -9,  16),   S(-13,   9),
            S( 49, -51),   S( 14,  -6),   S(  4, -23),   S(-13, -17),   S( -8,  -4),   S( 24, -37),   S( 44, -24),   S( 56, -31),
            S(-21,  21),   S(-135,  48),  S(-69,  14),   S(-60,  17),   S(-84,  24),   S(-72,  -2),   S(-62,   0),   S(-66,  -7),
            S(-15,   4),   S(-39,   2),   S(-33,  -9),   S(-69,  10),   S(-14, -34),   S(  6, -50),   S(-11, -51),   S(  5, -67),
            S( -4, -19),   S(  3, -16),   S( -9, -28),   S(-29, -21),   S(  7, -25),   S( 69, -54),   S(114, -45),   S( 45, -111),
            S(-35,  12),   S( 32,  -6),   S(-11, -34),   S( 24, -23),   S(  9, -35),   S( 49, -39),   S(  0,   0),   S(  0,   0),
            S(-12, -16),   S(  5, -20),   S(  3,  -6),   S(  1, -13),   S(  4, -92),   S(  6,  -7),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S( -9,  -9),   S( -9,  16),   S(-26,  25),   S(-13,  16),   S( -1, -12),   S(-20,  11),   S( -6,  25),   S( 32, -32),
            S(-31,  28),   S( 17,  24),   S( -5,  19),   S(  6, -18),   S( 78, -16),   S( 11,  -4),   S( 63, -12),   S(  6,   6),
            S( -4,  16),   S(  2, -14),   S(-24,  11),   S( -3,   0),   S( -7,   0),   S( -1, -15),   S(-30,  -5),   S(  5,  -9),
            S( -1, -19),   S(  3,  17),   S( -3,  -3),   S(  4,   9),   S( 21,  -6),   S(-17,  13),   S(-13,  19),   S(  6,  18),
            S(  0,   0),   S(  0,   0),   S(  8, -12),   S(  3,  16),   S(-25,   2),   S(-22,   1),   S(-13,   0),   S( -3, -12),
            S(  0,   0),   S(  0,   0),   S( -4,  -5),   S( 17,   4),   S( -3,  26),   S( -5, -24),   S(  3, -14),   S(  5,  27),
            S( -2, -16),   S( -1,   7),   S(  2, -25),   S( 15,   1),   S( 11,  27),   S(-26, -17),   S(  6,  -3),   S(  6,   0),
            S( -9,   6),   S(  6,  30),   S( -5,  -9),   S(  0,   8),   S(  0, -18),   S(  5,  15),   S( -3, -29),   S(  3,  14),

            /* kings: bucket 5 */
            S( 25, -20),   S(-16,  18),   S(-53,  22),   S(-47,  27),   S(-22,  22),   S(  1,   6),   S( 37,  10),   S( 40,  -6),
            S(-22,  14),   S( 55,   0),   S( 40,  -8),   S( 31,  -6),   S( 33,  -7),   S( 33,  -9),   S( 37,   3),   S( 24,  -2),
            S( 17,  -9),   S(-20,   4),   S( 17, -15),   S(-56,   5),   S(  1,  -3),   S(-45,  -5),   S(  8,  -2),   S( 21,  -7),
            S(-26,   7),   S( 54, -20),   S( 46, -15),   S( -3,  21),   S( 41,  14),   S( -5,   7),   S( 30,  -1),   S( -8,   1),
            S(-17, -24),   S(-20, -34),   S(  0,   0),   S(  0,   0),   S( -3,   0),   S(-11, -21),   S( -8,   1),   S(-22,   0),
            S(-28,  -4),   S(-22,   3),   S(  0,   0),   S(  0,   0),   S(  8,  34),   S(-24,   8),   S(-34,   8),   S(-10,  17),
            S(-13,   0),   S(  6,  15),   S(  2,   6),   S( -4,  19),   S(-15,  33),   S(-12,  19),   S(  4,   9),   S( 10,  -1),
            S(-10,  -8),   S( 10,  45),   S(  4,  45),   S(  9,  51),   S( -7,  17),   S( -6,  -3),   S( -3,  -1),   S( -4,   2),

            /* kings: bucket 6 */
            S( -8, -32),   S( 31,   4),   S( 17,  -4),   S( -4,  15),   S(-19,  20),   S(-27,  19),   S( 15,  26),   S( 43,   5),
            S( 44, -14),   S( 29,  15),   S( 16,  -2),   S( 41, -14),   S( 45,  -5),   S( 10,   2),   S( 38,   3),   S( 35,  -7),
            S( -7,   1),   S(-10,   5),   S(  5,  -9),   S(-24,  -2),   S( 10,  -7),   S(-62,   3),   S( -1,   4),   S(-40,  15),
            S( -2,   8),   S( 22,  -7),   S(  7,  -5),   S( 23,   2),   S( 56,   7),   S(  0,  -5),   S( 64, -17),   S(  2,  -5),
            S(  6, -14),   S(-44,  -1),   S(-17, -19),   S(  1,  10),   S(  0,   0),   S(  0,   0),   S( -6, -11),   S(-75,   4),
            S(-15,  12),   S( 12,  10),   S(-26,   0),   S(-15, -11),   S(  0,   0),   S(  0,   0),   S(-33,  20),   S(-40, -20),
            S(  1, -31),   S( -3,  14),   S(  7,  16),   S( -9,  -1),   S(  4,  18),   S( -1,  -4),   S( -3,  -1),   S(-22, -13),
            S(  6,  22),   S( -3,   4),   S(  2,   6),   S( -7, -19),   S(  8,  20),   S( -7,   8),   S(  5,  24),   S( -8, -10),

            /* kings: bucket 7 */
            S( 34, -14),   S( -3,  -1),   S(  6, -21),   S(-31,  13),   S(-50,  13),   S(-40,  36),   S(-12,  38),   S(-17,  20),
            S( 26,   1),   S( 31, -21),   S( -8,  -1),   S(  8, -12),   S(  3,   4),   S(-27,  20),   S( 14,   5),   S(  9,  14),
            S( 23, -20),   S(-24,  12),   S(-31,  -1),   S(-12,  -1),   S(-36,   6),   S(-56,  12),   S( -4,   6),   S(-67,  24),
            S(-11,   2),   S( 44, -17),   S(-12,  -3),   S(  4,  14),   S( 10,   3),   S( 61, -21),   S( 44, -10),   S( 32, -23),
            S(-11, -18),   S( -2,  17),   S(  0, -12),   S(-15,   6),   S(-27,  -8),   S( -1, -24),   S(  0,   0),   S(  0,   0),
            S(-12, -38),   S(  6,   5),   S( 29, -25),   S( 11,  -6),   S(  6,   8),   S(  4,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  29),   S( 45,  12),   S( 20,   2),   S( -5, -26),   S( 15, -30),   S(  5,  -3),   S( 12,  27),   S( -6, -30),
            S(  6,  -9),   S(-12, -45),   S( 19,  19),   S(  3,   0),   S( 12,  29),   S(-17, -42),   S(  2,  -2),   S(-19, -42),

            /* kings: bucket 8 */
            S( -9,  90),   S(-27,  73),   S(-39,  57),   S(-19, -20),   S(-13,  -4),   S( -8,   2),   S( 38, -13),   S( -8,  -7),
            S( 20,  87),   S( 31,  -8),   S( 10,  46),   S( -2,  12),   S(-13,  15),   S(  3,   1),   S(  3,  11),   S( 22,  36),
            S(  0,   0),   S(  0,   0),   S( 28,  44),   S( 12,   2),   S( -2,  -5),   S( 21, -23),   S( -5,  18),   S(  5,  -4),
            S(  0,   0),   S(  0,   0),   S( 16,  35),   S( 32, -17),   S(  1,  17),   S( 23,   5),   S( 21, -12),   S( -1,  30),
            S(  1,   3),   S(  5,  15),   S( 10, -10),   S( 13,  -7),   S( -6, -30),   S( 12,  -7),   S(  2,   1),   S(-12, -36),
            S( -3,  -7),   S( -4, -11),   S( -7,   0),   S( -4, -15),   S( -7,  -7),   S( -3,   3),   S( -7,   4),   S(  8,   4),
            S( -4, -22),   S( -8, -61),   S( 15,  14),   S(-10, -33),   S(-14, -42),   S(  5, -10),   S( 13,   9),   S(  2, -55),
            S( -1,  -6),   S(-13, -35),   S(  2,  -4),   S( -4, -12),   S(  6,  16),   S( -6,  -9),   S(  9,   5),   S(  9,  20),

            /* kings: bucket 9 */
            S(-26,  47),   S(-25,  38),   S(-51,  51),   S(-75,  26),   S(-81,  30),   S(-63,  26),   S( 57,   6),   S( 14,  17),
            S(-24,  23),   S( 24,  25),   S( 20,   2),   S( 20,  23),   S( 22,  20),   S( 10,   7),   S( 33,  19),   S( 36,   2),
            S(-25,  20),   S(  1,   8),   S(  0,   0),   S(  0,   0),   S( 26,  12),   S( -3,  -5),   S( -7,   0),   S( -7,  13),
            S(  5, -16),   S( -5, -20),   S(  0,   0),   S(  0,   0),   S( -1,  21),   S( 35,  -3),   S( -6,   0),   S( -4,  23),
            S(-16,  -2),   S( 10,  -9),   S( -3,   4),   S(  9, -13),   S( 12,  -7),   S( -2,  -7),   S(  0,  24),   S( -1,  12),
            S(  1,  27),   S( -6,  16),   S(  3,  28),   S( -9, -20),   S(  7,  14),   S( -7,  23),   S(-15,  -6),   S(-14,  13),
            S(  2,  -1),   S(  0,  -2),   S(  0,  -9),   S( -3,  -1),   S(  9,   0),   S( 19,   5),   S( 10,   5),   S(  0,  -9),
            S(  7,  27),   S( -1,   7),   S( 13,   7),   S( -6, -25),   S( -3, -27),   S(  1, -11),   S(  0, -30),   S(  8,  38),

            /* kings: bucket 10 */
            S(  4,  20),   S(-28,   4),   S( -6,  21),   S(-44,  23),   S(-57,  32),   S(-128,  49),  S( -9,  46),   S(-78,  73),
            S( 15,  -3),   S( 17,  22),   S(  4, -16),   S( 38,  12),   S( 65,   7),   S( -6,  12),   S( 28,  21),   S( -8,  30),
            S(  2,  -6),   S( 23,  -4),   S( 15,  -4),   S(-10,   0),   S(  0,   0),   S(  0,   0),   S( 24,   3),   S(-43,   8),
            S( 28,   9),   S(  3,  -5),   S( 15,  -3),   S( 11,  -9),   S(  0,   0),   S(  0,   0),   S( 16,  14),   S( 25,  -1),
            S( -2,  14),   S( 26,   0),   S( -3,   1),   S(  8, -34),   S(  4,   5),   S( -4,   1),   S( -3,   5),   S(-18,  26),
            S(-17,   8),   S( -3,  18),   S( -8,  20),   S( -7, -10),   S(  1,  10),   S( -1, -16),   S( -8,  15),   S( -1,   7),
            S(  8, -31),   S(  2,  13),   S( 14,   2),   S( 22,  12),   S( 20,  24),   S( -4,  -9),   S( 22,  -1),   S(  0,  14),
            S( -4,   3),   S(  9,   3),   S( -2, -23),   S( -5,   8),   S(  2,  -2),   S( -8, -30),   S( -5, -17),   S(  3,  27),

            /* kings: bucket 11 */
            S(-31,  23),   S( 20,   8),   S( -9, -21),   S(-20,  12),   S(-36,   3),   S(-154,  70),  S(-60,  56),   S(-147, 164),
            S( 20, -40),   S( 19,  12),   S(  1, -25),   S(  3,  11),   S( 19,   6),   S( 11,  55),   S( 34,   4),   S( 30,  42),
            S(-14,  -9),   S( 26,  -4),   S(  6,   0),   S( 13,  -2),   S( 52,  -3),   S( 42,  34),   S(  0,   0),   S(  0,   0),
            S(  1,   9),   S( -4,   3),   S(  7, -16),   S( 48,  -9),   S( 23, -18),   S( 39,  -7),   S(  0,   0),   S(  0,   0),
            S(  8,  10),   S( -3,  -1),   S( 19, -17),   S(  9, -26),   S( 21, -12),   S( -1, -13),   S(  7,  -8),   S( -1,  -1),
            S(  5,   6),   S( -2,  15),   S(  5,   0),   S( -2,   1),   S(  2, -21),   S(  8,  -7),   S( -2,  27),   S( -4, -18),
            S(  5, -14),   S( 18,  10),   S(  9,  51),   S( -2, -19),   S( 16,  -2),   S( -5,  -9),   S(  3, -24),   S( -1,  14),
            S(  4,   8),   S( -4, -46),   S(-18, -25),   S(  6, -17),   S( -9, -19),   S( -4,  -6),   S(  6,  -6),   S(  7,  22),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  8,  73),   S(-10,  11),   S( -3, -10),   S(  0,  19),   S( 22, -17),   S(-14,  30),
            S(  0,   0),   S(  0,   0),   S( 24,  83),   S( 16, -10),   S(  8,  31),   S( 32,   8),   S( 23,   0),   S(-23,  17),
            S(  3,  18),   S( -3, -51),   S( 10,  14),   S(  9,  29),   S(  9, -24),   S(  0,  29),   S(-10,   0),   S(-22,   8),
            S(  0, -18),   S(  6,  25),   S(-12, -53),   S(  1, -43),   S(-14, -31),   S(  3,  17),   S(-14,   9),   S( -9,  -4),
            S( 14,  45),   S(  4,  30),   S( -2,  -2),   S( -1, -31),   S( -6,  -1),   S( -7, -10),   S( -9,   2),   S( -1,   0),
            S(  7,  33),   S(  0,  18),   S( -1,   8),   S( -3,  -6),   S(  5, -14),   S( -6,  35),   S(-22,   3),   S( 10,  38),
            S(  6,  25),   S( -2, -12),   S( -3, -10),   S(  0,   2),   S( -2, -11),   S( -2,  11),   S(  8,  17),   S( -2,   4),
            S( -2,  -5),   S( -2,  -9),   S(  2, -10),   S(  2,  -4),   S( -7, -36),   S(  1,   1),   S(  0, -10),   S( -3, -12),

            /* kings: bucket 13 */
            S( -6,  93),   S(  5,  77),   S(  0,   0),   S(  0,   0),   S(  6,  80),   S(-24,  -6),   S(  8,  -6),   S(  0,  48),
            S(-17, -13),   S(-11, -24),   S(  0,   0),   S(  0,   0),   S( 18, -10),   S( -7, -20),   S(-23,   6),   S( -2,  32),
            S( -9,   0),   S(  9,  29),   S(-10, -66),   S( -4, -28),   S( -1,  26),   S(  1,  16),   S(-18,   7),   S(  7,   2),
            S( -9, -22),   S( -4,  18),   S(  6,  17),   S( -3, -42),   S(  7, -31),   S(  0, -17),   S(-13,   3),   S(-12,  -7),
            S(  5,  22),   S( -2,   8),   S( 16,  61),   S( -7, -17),   S(-11,   1),   S(  2,  33),   S( -5,  -5),   S(  2,   7),
            S(  4,  14),   S( -9,  -9),   S(-12, -19),   S( -2,   1),   S(-12, -34),   S( -8,   0),   S( -2,  -6),   S( 11,  13),
            S( 10,  27),   S(-10,  -7),   S( -6,   1),   S(  5,   2),   S( -5, -29),   S( -1, -13),   S(-10, -58),   S( 11,  49),
            S( -1, -17),   S(  1,  20),   S(  1,  12),   S(  0, -14),   S(  2,  10),   S(  0,   7),   S(  4,   2),   S(  4,  12),

            /* kings: bucket 14 */
            S( -1,  63),   S(-21, -13),   S(-16, -18),   S( -2,  24),   S(  0,   0),   S(  0,   0),   S(  8,  93),   S(-48,  70),
            S(-33,  12),   S(-13, -10),   S(  7, -10),   S( 20,   3),   S(  0,   0),   S(  0,   0),   S( 11,   5),   S(-36,   6),
            S( -3,   0),   S( -2, -17),   S( 18,  -3),   S(  9,  -5),   S(  5, -16),   S( -3,  18),   S( 12,  24),   S(-34,  12),
            S(  3,  -3),   S( 11,  14),   S(  1, -20),   S( 12, -39),   S(-14, -48),   S(  8,   5),   S(  4,   6),   S(  3,   6),
            S(  4,  32),   S( 10,   4),   S(-15,  -6),   S(-10, -21),   S( -4,  21),   S(  4,  26),   S(  9,  52),   S(  7,   8),
            S( -4, -16),   S( -1,  26),   S(  5,  26),   S( -7,  34),   S( -1, -14),   S( -9, -17),   S(-14, -50),   S(  0,  25),
            S(  8,  19),   S( -7, -58),   S( -7,   2),   S(  6,  14),   S( -4,  -7),   S( -1, -24),   S(-17, -76),   S(  5,  51),
            S(  1,  14),   S(  7,  60),   S(  5,   9),   S(  0,   0),   S(  7,  44),   S( -2, -13),   S(-10, -37),   S( -3, -25),

            /* kings: bucket 15 */
            S( -2,  26),   S( -7,   1),   S( 15,   4),   S(-12, -19),   S(-24,   3),   S(-15,  80),   S(  0,   0),   S(  0,   0),
            S(  6,  -1),   S( -5,  19),   S( -4, -32),   S( 23,  50),   S( 27, -30),   S( 39,  85),   S(  0,   0),   S(  0,   0),
            S(-17,  -1),   S(  8,   9),   S(  7,  -9),   S( -9,   9),   S( 17, -16),   S( 23,  62),   S( 17,  28),   S( -8, -16),
            S(  1,  11),   S( -4,  -1),   S(  5,  12),   S(-10, -21),   S(-13, -47),   S(  1,   1),   S(  9,  52),   S( -7, -19),
            S( -2,  -3),   S(-12,  10),   S(-10,  16),   S(-18, -59),   S( -1, -15),   S(  1,  44),   S( -9, -11),   S( -6, -21),
            S(  5,  34),   S(-23, -20),   S(  1,   2),   S( 10,  38),   S(-14, -43),   S( -7, -10),   S( -3,  23),   S(  3,   4),
            S(  7,  27),   S(  0,  -6),   S( -6, -12),   S(  3,  11),   S(  6,   0),   S(  9,  26),   S( -9, -23),   S(  3,   5),
            S( -7,  -8),   S( -3, -12),   S( -3,  14),   S( -3, -17),   S(  4,  38),   S(  6,  27),   S(  3,  18),   S(  3,   0),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S( -7, -180),  S( 15, -24),   S( 27,  39),   S( 35,  73),   S( 42,  94),   S( 47, 111),   S( 52, 109),   S( 64,  96),
            S( 80,  66),

            /* bishop mobility */
            S( 11, -78),   S( 14,  -1),   S( 25,  37),   S( 30,  71),   S( 35,  95),   S( 36, 113),   S( 40, 118),   S( 45, 116),
            S( 48, 118),   S( 55, 108),   S( 63,  97),   S( 85,  84),   S(108,  74),   S(129,  53),

            /* rook mobility */
            S(-13,  -4),   S( -5,  55),   S( -5,  87),   S( -2, 102),   S( -4, 125),   S(  2, 133),   S(  4, 143),   S(  9, 142),
            S( 10, 151),   S( 13, 156),   S( 20, 155),   S( 28, 154),   S( 39, 148),   S( 48, 144),   S(133, 100),

            /* queen mobility */
            S( 67, -21),   S( 71, -76),   S( 63,  47),   S( 57, 113),   S( 58, 158),   S( 59, 193),   S( 59, 228),   S( 60, 246),
            S( 59, 269),   S( 60, 285),   S( 57, 306),   S( 56, 324),   S( 57, 331),   S( 54, 344),   S( 54, 354),   S( 55, 355),
            S( 60, 352),   S( 68, 341),   S( 82, 330),   S(105, 305),   S(134, 282),   S(180, 245),   S(156, 254),   S(204, 216),
            S(178, 230),   S(189, 202),   S(123, 207),   S( 89, 183),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  29),   S(-19,  57),   S(-40,  62),   S(-15,  74),   S( 18,  24),   S(-29,  48),   S( -8,  77),   S( 33,  36),
            S( 15,  41),   S(  1,  61),   S(-17,  58),   S(-11,  46),   S( -2,  45),   S(-50,  69),   S(-39,  84),   S( 37,  36),
            S( 32,  71),   S( 19,  77),   S(  9,  60),   S( 30,  47),   S(  5,  53),   S(-30,  70),   S(-25, 103),   S(  5,  72),
            S( 56,  89),   S( 69,  98),   S( 41,  72),   S( 16,  58),   S( 12,  60),   S( 16,  82),   S(-17, 112),   S(-45, 126),
            S( 67, 127),   S( 89, 166),   S( 93, 113),   S( 51,  99),   S(-38,  91),   S( 60,  87),   S(-23, 162),   S(-47, 143),
            S(168, 179),   S(141, 214),   S(189, 197),   S(201, 193),   S(186, 202),   S(181, 197),   S(199, 197),   S(217, 198),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 14,  13),   S(  4,   0),   S( 10,   3),   S(  7,  46),   S( 24,  44),   S(  2, -28),   S(-15,  39),   S( 10, -35),
            S( -9,  45),   S( 29,  -6),   S(  1,  54),   S( 19,  38),   S( 41,   1),   S( -5,  36),   S( 22,   1),   S(  3,  10),
            S(-12,  28),   S( 16,  12),   S(  1,  51),   S( 13,  65),   S( 28,  30),   S( 26,  26),   S( 31,   9),   S( -6,  22),
            S( 10,  43),   S( 25,  32),   S( 37,  88),   S( 14,  96),   S( 77,  54),   S( 71,  49),   S( 16,  58),   S( 27,  29),
            S( 73,  92),   S(114,  83),   S(100, 112),   S(152, 179),   S(165, 124),   S(137, 116),   S(152,  84),   S( 89,  27),
            S( 66, 187),   S(109, 276),   S(104, 233),   S( 96, 206),   S( 61, 147),   S( 53, 115),   S( 34, 121),   S(  3,  69),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 17,  41),   S( 23,  35),   S( 43,  48),   S( 43,  36),   S( 30,  35),   S( 29,  39),   S(  7,  23),   S( 47,  14),
            S( -2,  25),   S( 13,  48),   S( 14,  38),   S( 11,  49),   S( 26,  16),   S(  8,  26),   S( 25,  26),   S( -1,  17),
            S(  2,   6),   S( 22,  26),   S( 51,  43),   S( 40,  33),   S( 43,  41),   S( 66,   9),   S( 18,  26),   S( 19,   6),
            S( 72,  27),   S(114,   8),   S(140,  69),   S(159,  86),   S(155,  78),   S( 81,  97),   S( 99,   6),   S( 80, -12),
            S(106,  14),   S(133,  53),   S(151, 162),   S(173, 152),   S(155, 195),   S(113, 154),   S(197, 120),   S(-26, 104),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  27),   S(  9,  15),   S(  9,  27),   S( -9,  64),   S( 70,  30),   S( 11,  16),   S(  2,  -5),   S( 26,  14),
            S(  1,  15),   S(  6,   9),   S( 17,  19),   S( 17,  29),   S( 14,  16),   S( -4,  13),   S(  3,   7),   S( 29,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -15),   S( -6,  -9),   S(-17, -19),   S(-17, -29),   S(-14, -16),   S(  4, -13),   S( -3,  -7),   S(-29,   5),
            S(-25, -27),   S( -9, -15),   S( -9, -27),   S(  9, -64),   S(-70, -30),   S(-11, -16),   S( -2,   5),   S(-26, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-19, -19),   S( -8, -16),   S(  0, -30),   S(-65,   1),   S(-19, -12),   S(-20, -23),   S( -3, -23),   S(-21, -36),
            S(-24,   5),   S(-14,  -2),   S(-25,   8),   S(  4, -16),   S(-28, -16),   S(-26,  -2),   S(-41,   6),   S(-14, -12),
            S(-19, -15),   S( -7, -17),   S(-23,  -4),   S(-29, -10),   S(-25, -28),   S(-22, -11),   S(-19,  -4),   S(-48, -15),
            S( -5, -32),   S( 20, -41),   S( 17, -23),   S(  9, -30),   S(  3, -17),   S( 58, -34),   S( 36, -30),   S(-16, -41),
            S( 21, -56),   S( 66, -83),   S( 64, -43),   S( 74, -40),   S( 71, -45),   S( 70, -17),   S(108, -71),   S( 24, -63),
            S(113, -107),  S(201, -138),  S(152, -83),   S(117, -59),   S(113, -43),   S(154, -28),   S(156, -62),   S( 53, -79),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 17,   2),        // attacks to squares 1 from king
            S( 17,   2),        // attacks to squares 2 from king

            /* castling available */
            S( 71, -91),        // king-side castling available
            S( 18,  55),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S(  6, -46),   S(  7, -38),   S( -2, -44),   S(-15, -31),   S(-22, -23),   S(-40, -13),   S(-75,   8),   S(-82,  11),
            S(-94,  15),   S(-86,  22),   S(-79,  31),   S( -7,  12),   S(-17,  23),

            /* orthogonal lines */
            S(-16, -48),   S(-53, -26),   S(-77, -14),   S(-92, -13),   S(-98, -18),   S(-101, -24),  S(-102, -28),  S(-100, -30),
            S(-118, -23),  S(-137, -22),  S(-158, -29),  S(-160, -36),  S(-188, -22),  S(-370,  40),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 15, 218),

            /* passed pawn can advance */
            S(-14,  36),   S(-12,  68),   S( -1, 106),   S( 32, 206),

            /* blocked passed pawn */
            S(  0,   0),   S( 59, -18),   S( 43, -19),   S( 36,  20),   S( 44,  38),   S( 47,   3),   S(160, -30),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 34, -10),   S( 16,  49),   S( 15,  47),   S( 19,  67),   S( 38,  90),   S(130, 114),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-28, -16),   S(-25, -23),   S(  4, -52),   S(-23, -46),   S(  6, -55),   S(197, -105),  S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 21, -22),   S( 21, -26),   S(  7,  -8),   S( 15, -48),   S( 21, -175),  S( 15, -268),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S(  1,  44),   S( 16,  43),   S( 92,  44),   S( 18,  36),   S(202,  79),   S(186,  45),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-26,  82),

            /* bad bishop pawn */
            S( -5, -21),

            /* rook on open file */
            S( 45,  -6),

            /* rook on half-open file */
            S( 14,  22),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 25,  38),   S( 29, -10),   S( 34,  24),   S( 28,  -1),   S( 40, -20),

            /* pawn threats */
            S(  0,   0),   S( 69,  93),   S( 56, 116),   S( 63,  92),   S( 57,  55),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 36,  52),   S( 51,  38),   S( 79,  35),   S( 46,  58),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 16,  11),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
