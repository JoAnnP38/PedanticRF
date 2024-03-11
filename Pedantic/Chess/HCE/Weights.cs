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
        public const int MAX_WEIGHTS = 12785;
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
        public const int ROOK_THREAT = 12778;       // rook threat
        public const int TEMPO = 12784;             // tempo bonus for side moving

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score RookThreat(Piece threatenedPiece)
        {
            return weights[ROOK_THREAT + (int)threatenedPiece];
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

        // Solution sample size: 6000011, generated on Mon, 11 Mar 2024 06:48:23 GMT
        // Solution K: 0.003850, error: 0.084945, accuracy: 0.5006
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S(103, 179),   S(442, 540),   S(454, 570),   S(579, 959),   S(1383, 1647), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 97, -116),  S(145, -91),   S( 31, -34),   S(-40,  21),   S(-37,  -1),   S(-26,  -4),   S(-55,  -8),   S(-47, -21),
            S(100, -109),  S( 94, -98),   S(  7, -67),   S(-14, -52),   S(-27, -24),   S(-25, -33),   S(-43, -30),   S(-36, -40),
            S( 91, -87),   S( 68, -56),   S( 14, -52),   S( 13, -67),   S( -9, -55),   S( -3, -49),   S(-22, -47),   S(-24, -40),
            S( 59, -31),   S( 45, -50),   S( 18, -44),   S( 22, -72),   S(-11, -42),   S(-22, -46),   S(-30, -39),   S(-41, -20),
            S( 62,  34),   S( 34,   3),   S( 37, -12),   S( 52, -62),   S( 33, -48),   S(-25, -34),   S(-49,  -3),   S(-55,  57),
            S( 65,  51),   S( 67,  49),   S( 36,  13),   S( 90, -41),   S(-38, -20),   S( -5,  17),   S( 14, -16),   S(-23,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 29, -28),   S( 25, -40),   S( 43, -27),   S(-10,  42),   S(-14, -19),   S(  2, -17),   S(-43,  -6),   S(-38,  17),
            S( 16, -35),   S(  7, -38),   S(  3, -43),   S(  1, -50),   S(-13, -24),   S(-10, -29),   S(-40, -15),   S(-43, -10),
            S(  7, -25),   S( 14, -29),   S( 14, -45),   S( 14, -49),   S(-24, -22),   S( 11, -43),   S(-18, -25),   S(-17, -10),
            S( 24,   3),   S( 17, -38),   S( 21, -38),   S( 14, -51),   S( -7, -22),   S(  8, -34),   S(-33, -13),   S(-33,  17),
            S( -2,  62),   S(-64,  13),   S(-27,   1),   S(  0, -17),   S(  0,  -9),   S(-18,  -1),   S(-19,   0),   S(-34,  62),
            S( 70,  48),   S( 58,  -7),   S(-15, -33),   S(-43,   0),   S(-62, -16),   S(-50,   2),   S(-20,  11),   S(-50,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29,   6),   S(-35,   6),   S(-17,   2),   S(-18,  13),   S(  6,  -9),   S( 32, -23),   S(  0, -45),   S(-12, -19),
            S(-28, -14),   S(-51,  -5),   S(-27, -35),   S(-22, -35),   S(  1, -28),   S(  8, -33),   S(-13, -37),   S(-22, -24),
            S(-40,   0),   S(-36, -13),   S(-14, -42),   S( -7, -48),   S( -6, -22),   S( 11, -30),   S(-11, -29),   S( -5, -17),
            S(-43,  19),   S(-31, -23),   S(-29, -26),   S( -9, -43),   S(  8, -39),   S( -5, -17),   S(-16,  -4),   S(-20,   9),
            S(-29,  52),   S(-88,  29),   S(-75, -14),   S(-64, -19),   S(  2,  10),   S(-24,  11),   S(-29,  19),   S(-37,  88),
            S(-27,  59),   S(-97,  67),   S(-105,   5),  S(-104, -15),  S(-28,   8),   S(-32,  26),   S(-35,  13),   S(-10,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31,  -3),   S(-43,   3),   S(-34,  -1),   S( -8, -27),   S(-13,  -1),   S( 39, -19),   S( 81, -67),   S( 56, -78),
            S(-38, -29),   S(-56, -15),   S(-34, -42),   S(-25, -28),   S(-21, -20),   S(  7, -38),   S( 53, -63),   S( 52, -72),
            S(-41, -22),   S(-28, -40),   S( -9, -58),   S( -7, -59),   S(-11, -41),   S( 17, -41),   S( 28, -47),   S( 56, -53),
            S(-38,   0),   S(-22, -51),   S(-14, -54),   S(  1, -67),   S( 16, -59),   S(  7, -49),   S( 10, -27),   S( 38,  -9),
            S( -4,  28),   S(-44,  -8),   S(-12, -53),   S(  1, -60),   S( 76, -45),   S( 59, -33),   S( 46,  11),   S( 28,  87),
            S(-50, 111),   S(-57,  37),   S(-15, -52),   S(-36, -54),   S( 45, -46),   S( 89, -20),   S( 61,   9),   S( 32,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  10),   S(-12, -12),   S( -2,   2),   S(-17,   3),   S(-38, -12),   S(-49,   8),   S(-74,  11),   S(-65,  -1),
            S(-18,   4),   S( 29, -12),   S( 40, -37),   S(  8, -24),   S(-18, -18),   S(-53, -25),   S( -5, -35),   S(-10, -30),
            S( 42, -18),   S( 61,  -9),   S(  8,   5),   S(-10, -22),   S(-36, -21),   S(-18, -40),   S(-50, -27),   S(-37,  -6),
            S(  1,  43),   S( 30,  25),   S( 53,   0),   S(  1,   1),   S( -4, -26),   S(-58,  -7),   S(  0, -37),   S( 37, -24),
            S(-26,  97),   S( 26,  71),   S( -6,  13),   S(-24, -11),   S( 20,  10),   S( -4, -15),   S(-48, -23),   S( 37,  23),
            S( 58,  68),   S( 81,  59),   S( 34,   4),   S( 16,  17),   S(  9, -24),   S(  6, -45),   S(  4, -29),   S(-19,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-79,  47),   S(-37,  22),   S(-15,  18),   S( 10,  14),   S(  5,  13),   S(-26,  16),   S(-46,   7),   S(-28,  20),
            S(-66,  24),   S(-55,  14),   S( 16,  -7),   S(-15,  20),   S(  5,  -5),   S(-14, -17),   S(-18, -13),   S(-38,  12),
            S(-55,  43),   S(-37,  24),   S( 63, -25),   S( 32, -24),   S( 42, -14),   S(-40,   0),   S(-13,  -3),   S(-24,   9),
            S(-21,  56),   S(-31,  32),   S( 30,   9),   S( 42,  11),   S(  1,   9),   S(-53,  17),   S( 45, -19),   S( 32,  10),
            S( 56,  62),   S(106,  19),   S( 58,  40),   S( 46,  25),   S(-25,  62),   S( 91,  -2),   S(-12,   6),   S(-10,  53),
            S( 66,  37),   S( 86,  32),   S( 25,   6),   S( 42,  -4),   S( 43,  -1),   S( 18,  -9),   S( -2,   1),   S( 37,  41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  28),   S(-11,   1),   S(  8,  -3),   S(-14,  10),   S(-13,  -3),   S(-35,   8),   S(-69,   6),   S(-61,  17),
            S(-42,   1),   S( -8, -21),   S(-18, -30),   S( 16, -18),   S( 50, -19),   S( 22, -21),   S(-36,  -6),   S(-71,  14),
            S(-38,  13),   S(  5,  -9),   S( 19, -25),   S( -8, -11),   S( 40, -12),   S( 64, -28),   S( -3, -10),   S(-36,  21),
            S( -9,  24),   S(-67,  17),   S( 29, -19),   S( 15,  -6),   S( 22,  10),   S( 33,   0),   S( 19,   2),   S( 23,  15),
            S(-22,  51),   S(-17,  11),   S(-37,   9),   S( 12, -11),   S( 93,  24),   S( 57,  43),   S( 55,  -4),   S( 75,  40),
            S( 75,  36),   S( 25,  14),   S( 34, -44),   S( 58, -51),   S( 38, -18),   S( 29,   2),   S( 28,   3),   S( 91,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54, -16),   S(-61,  -8),   S(-15, -13),   S(-32, -24),   S(-24, -19),   S( 33, -30),   S( -5, -40),   S(-58, -12),
            S(-51, -37),   S(-51, -40),   S(-27, -47),   S(-10, -47),   S( -2, -34),   S( 50, -50),   S( 38, -41),   S(-13, -28),
            S(-71, -19),   S(-52, -36),   S(-35, -39),   S( -7, -51),   S( -6, -23),   S( 26, -23),   S( 41, -41),   S( 37, -26),
            S(-43,  -9),   S(-57, -31),   S(-63, -36),   S(-37, -22),   S(  1, -18),   S( 18,  -2),   S( 18,   2),   S( 66, -12),
            S(-23, -11),   S(-31, -31),   S( -8, -43),   S(-14, -46),   S( 20,  11),   S(  4,  12),   S( 65,  53),   S(116,  41),
            S(  6, -14),   S(-34, -33),   S( 47, -72),   S( 30, -62),   S( 16, -28),   S( 22, -37),   S( 49,  44),   S( 83,  78),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48,  33),   S(-36,  34),   S(-10,  15),   S( -9, -12),   S( -1,  32),   S(-34,   5),   S(-83,  -9),   S(-34,  18),
            S(-39,  30),   S(-27,  25),   S(-33,  21),   S(-23,   0),   S(-18, -16),   S(-35, -29),   S(-29, -31),   S( -3, -13),
            S(-22,  69),   S( 28,  67),   S(-18,  52),   S(-26,  23),   S(  2,   7),   S(-45, -25),   S(-26, -52),   S( -6, -19),
            S( 13, 100),   S( 70, 119),   S( 56,  65),   S( 22,  63),   S(  8,  -9),   S(-52, -18),   S( -3, -21),   S(  2, -20),
            S( 63, 116),   S( 66, 105),   S( 61, 121),   S( 54,  89),   S( -2, -12),   S( 10,   5),   S( -1, -25),   S( -2,  -6),
            S(101, 148),   S( 83, 152),   S( 86, 169),   S( 50,  50),   S( 21,  30),   S( -8, -26),   S( -9, -37),   S( -1, -22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  58),   S(-90,  45),   S(-26,  10),   S( 24,  47),   S(  4,  -8),   S(-88,  27),   S(-92,  14),   S(-83,  29),
            S(-29,  18),   S( -7,   9),   S(-65,  26),   S(-30,  29),   S(-80,  15),   S(-49,   0),   S(-113,   1),  S(-67,  16),
            S(-52,  54),   S(-23,  63),   S(-21,  68),   S(-55,  67),   S(-69,  56),   S(-89,  10),   S(-37, -17),   S(-63,  13),
            S(  5,  76),   S( 55,  91),   S( 30, 118),   S( 67, 127),   S(-33,  67),   S( -1,  14),   S( 14,   6),   S( 88, -20),
            S(117,  73),   S( 69, 106),   S( 84, 120),   S( 96, 133),   S( 75, 104),   S( 43,  50),   S( 21,  -5),   S( 51,  -4),
            S( 35,  57),   S( 67, 107),   S( 96,  89),   S(116, 133),   S( 59,  94),   S(  4, -31),   S( -1, -51),   S(  2, -18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94,  14),   S(-83,   2),   S(-48,   8),   S(-10,  22),   S(-21, -24),   S(-120,  45),  S(-152,  35),  S(-54,  38),
            S(-104,  -8),  S(-38, -17),   S(-33, -21),   S(-49,   8),   S(-49,  27),   S(-26,  24),   S(-120,  21),  S(-94,  28),
            S( -5,  -7),   S(-32,   4),   S(-13,  -2),   S(-55,  53),   S(-58,  84),   S(  3,  46),   S(-90,  40),   S(-70,  47),
            S( -1,  -7),   S( 20,  -6),   S( 14,  22),   S(  8,  63),   S( 38, 102),   S( 22,  81),   S( -9,  45),   S( 29,  27),
            S( 75,   3),   S( 20,   4),   S( 36,  43),   S( 63, 115),   S( 92, 111),   S( 57,  68),   S( 65,  56),   S( 77,  26),
            S( 46, -10),   S( 15, -23),   S( 29,  46),   S( 66, 127),   S( 38,  63),   S( 39,  22),   S( 39,  50),   S( 39,  97),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94,   5),   S(-102,   9),  S(-17,   2),   S(-24,  -2),   S(  4,  11),   S(-118,  55),  S(-73,  29),   S(-50,  32),
            S(-70, -26),   S(-60, -30),   S(-55, -33),   S(-62,   9),   S(-47,   2),   S(  4,   3),   S(-93,  47),   S(-115,  42),
            S(-37, -29),   S(-21, -45),   S(-44, -11),   S(-22,  10),   S(-47,  46),   S(  3,  52),   S(-34,  58),   S(-56,  67),
            S(  5, -17),   S(-75, -18),   S( -3, -12),   S(-23,  12),   S( -5,  57),   S( 28,  58),   S( 64, 104),   S(135,  53),
            S( -7,   4),   S(-39,  -1),   S(-24,  -8),   S(-11,   8),   S( 72,  80),   S( 51,  89),   S(118,  90),   S(151,  75),
            S(  0,  -9),   S(  1, -30),   S(  7, -24),   S( 12,   1),   S( 25,  33),   S( 66, 124),   S( 99, 151),   S( 94, 120),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15,  16),   S(-20,   9),   S(-37, -35),   S(  1,   1),   S( -8,   4),   S(-17,   8),   S(  7, -15),   S(-34,   7),
            S(-21,  -9),   S( -2, -19),   S( -1,  38),   S( -2,  -1),   S(  7,  55),   S( 10, -14),   S(-28, -26),   S(-15, -59),
            S(  5,  56),   S( 16,  68),   S(  4,  17),   S( 22,   1),   S( 22,  15),   S(-26, -23),   S(-15, -44),   S(-24, -45),
            S(-11,  84),   S( 14,  92),   S( 53,  95),   S( 43,  73),   S( -4, -40),   S(-14, -40),   S(-12, -11),   S(-16, -68),
            S( 87,  67),   S( 72, 199),   S( 65, 135),   S( 24,  59),   S( -2,  -2),   S( -4, -23),   S(  5, -26),   S(-21, -85),
            S( 53, 139),   S( 52, 266),   S(101, 227),   S( 64, 112),   S(  4,  21),   S(-11, -44),   S(-11, -62),   S( -9, -73),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -26),   S(-26,   3),   S( -6,  -6),   S(  0,  16),   S( -8,   4),   S(-39, -19),   S(-37, -36),   S(-21,  -3),
            S(-20, -53),   S(-24,   3),   S(-18, -20),   S(-14, -25),   S(-45,   5),   S(-16, -17),   S(-57, -42),   S(-29, -27),
            S( 30,  17),   S( 23, -18),   S(  0,  10),   S( 24,  62),   S(-29,  36),   S(-12, -22),   S(-33, -43),   S(-23, -40),
            S( 14,  34),   S( 61,  38),   S( 37,  78),   S( 22,  94),   S( 22,  51),   S(  8, -12),   S( -2, -39),   S(  1, -64),
            S( 35,  46),   S( 70, 123),   S( 92, 163),   S( 96, 190),   S( 70,  85),   S(  6,   4),   S(  5, -63),   S(  0, -83),
            S( 41,  94),   S( 80, 157),   S(100, 227),   S(110, 230),   S( 48,  97),   S( 14,  17),   S(  4, -27),   S( 12, -38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39, -38),   S(-34, -10),   S( -2, -26),   S( -1,  -1),   S( -6, -12),   S(-55,   9),   S(-32,   2),   S( -7,  48),
            S(-41,   6),   S(-32, -32),   S(-26, -35),   S(  9,  37),   S(-25,  23),   S(  2,  10),   S(-11,   6),   S(-37,   6),
            S(-21, -44),   S(-10, -28),   S(-18, -21),   S(-14,  12),   S( -6,  37),   S( -1,  16),   S(  5,   9),   S(-12,  22),
            S( 29, -27),   S( 35,  -7),   S(  9, -34),   S(  9,  61),   S( -2, 132),   S( 26,  58),   S( -8,  16),   S( 45,  25),
            S(  6, -50),   S( 20,   2),   S( 35,  51),   S( 45, 152),   S( 90, 211),   S( 85, 148),   S( 50,  46),   S( 34,  66),
            S( 16, -22),   S( 23, -15),   S( 43,  82),   S( 51, 136),   S( 78, 233),   S( 54, 167),   S( 38, 114),   S( 23,  71),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36, -61),   S(-35, -44),   S( -9, -42),   S( -6, -14),   S( 16,  31),   S( -1,  31),   S(-13,  -6),   S( 23,  70),
            S(-12, -71),   S(-17, -27),   S(-31, -40),   S(  9,  -1),   S(-24,  13),   S( -9,  23),   S(  6,  26),   S(-24,  10),
            S(-13, -50),   S(-13, -79),   S(-17, -49),   S( -3,   1),   S(  7,  18),   S( 21,  35),   S( 18,  76),   S( 30,  60),
            S( 10, -51),   S(-34, -15),   S( -2, -13),   S( 23,  33),   S( -4,  11),   S( 25,  45),   S( 23, 109),   S(  1,  86),
            S( -2, -68),   S( -5, -46),   S(-17, -47),   S(  9, -18),   S( 50, 106),   S( 86, 104),   S( 53, 252),   S( 74,  79),
            S( 11, -20),   S( -8, -40),   S(  6,  -2),   S( 10,  32),   S( 34,  78),   S( 68, 240),   S( 18, 226),   S( 46,  82),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-46,   6),   S(-25,   2),   S(-42,   3),   S(-29, -28),   S(-21, -31),   S( -9, -39),   S(-67, -64),   S(-38, -36),
            S(-50,  94),   S( 36, -38),   S(-35,  -8),   S(  4, -38),   S( -4, -42),   S(-18, -11),   S(-42, -40),   S(-97, -31),
            S( -8,  81),   S(  1, -37),   S( 14, -19),   S(-14,  28),   S( 22,  10),   S(-34,  13),   S( -7, -18),   S(-33, -67),
            S( 18, -16),   S( 39,  18),   S( 22,   4),   S( 29,  24),   S( 10,  -5),   S(  9,  -9),   S( -1, -20),   S(-10, -10),
            S( 23, -60),   S( 44, -27),   S( 21,  -6),   S( 78, -33),   S( 58, -10),   S( 45,   7),   S( 15, -14),   S(-69, -45),
            S( 10, -24),   S(  8, -11),   S(  4,  25),   S( 48,  -8),   S( 58, -78),   S( 28, -18),   S(  7, -26),   S(-18, -51),
            S(  4, -13),   S( 29, -41),   S( 59, -39),   S(  8, -28),   S( 52, -28),   S( -4, -62),   S( 11, -31),   S(-30, -47),
            S(-91, -65),   S(-36,  -2),   S(-15, -18),   S(-29, -67),   S(-27, -44),   S( 18,  25),   S(-26, -33),   S(  6, -12),

            /* knights: bucket 1 */
            S(-42,  -9),   S(-67, 101),   S( 10,  33),   S(-16,  54),   S(-50,  52),   S(-21,  23),   S(-47,  54),   S(-12,   1),
            S( 32,   4),   S(-22,  35),   S( -1,   6),   S( -9,  37),   S( -9,  21),   S( -4,   8),   S( -2, -29),   S(-26,  -3),
            S(-35,  23),   S(  8,   9),   S(  3,  13),   S( 18,  18),   S( 12,  26),   S(-25,  25),   S(  3, -12),   S(-25,  -4),
            S(-10,  40),   S( 97,  -1),   S( 24,  43),   S( 26,  27),   S(  9,  34),   S( -1,  17),   S( 44,  -6),   S( 17,  18),
            S(  3,  37),   S( 22,  16),   S( 33,  25),   S( 34,  34),   S( 42,  33),   S( 45,  18),   S( 18,  23),   S( 13,  35),
            S( 14,  -8),   S( 16,  -7),   S( 34,  17),   S( 35,  29),   S( -8,  10),   S( 39,  28),   S( 47,   5),   S( 31,  -2),
            S( 56,   6),   S( 16,  16),   S(  8, -38),   S( 25,  27),   S(-13,   0),   S( 56, -27),   S(-52,  -1),   S( 21, -25),
            S(-126, -90),  S(-32, -41),   S(-34,  -4),   S(-23,  -3),   S(-34, -16),   S(-42, -43),   S(  3, -24),   S(-21, -64),

            /* knights: bucket 2 */
            S(-41,   3),   S(  0,  25),   S(-31,  42),   S(-20,  51),   S(-35,  53),   S(-33,  73),   S(-24,  39),   S(-28,  15),
            S( -8, -21),   S(-10,  -6),   S(  2,  10),   S( -9,  29),   S(  2,  19),   S(  6,  29),   S(-42,  57),   S(-34,  81),
            S(-20,  14),   S(  2,   8),   S( -9,  36),   S( 26,  17),   S(  6,  28),   S(  5,  17),   S( -1,  38),   S(-23,  28),
            S(-10,  36),   S(-23,  36),   S(  5,  43),   S( 14,  47),   S(  5,  55),   S( -2,  41),   S(  2,  47),   S( -5,  41),
            S( 14,  19),   S( -6,  26),   S( -8,  48),   S(-10,  59),   S(  4,  51),   S(  3,  47),   S( -2,  41),   S( -7,  33),
            S(-17,  26),   S(-18,  28),   S(-17,  48),   S(-13,  51),   S(-12,  45),   S( -2,  41),   S(-45,  13),   S( 51, -18),
            S( -4,  15),   S(-54,   5),   S(-52,  26),   S(-12,   3),   S(-30,   8),   S( 15,  10),   S(-39,  37),   S(-91,  19),
            S(-143,   9),  S( 19,  -7),   S(-70,  17),   S(-15,  -5),   S(  1,   4),   S(-53,  -9),   S( -2, -35),   S(-214, -52),

            /* knights: bucket 3 */
            S(-49, -21),   S( -1, -30),   S(-43,  17),   S(-20,  17),   S(-11,  13),   S(-17,  24),   S( -2,  14),   S(-30,   3),
            S(-15,  11),   S(-36,   9),   S(-21,  11),   S(  6,  17),   S( 11,  11),   S(-10,   3),   S(-25,  13),   S(-22,  72),
            S(-14, -26),   S( -1,  13),   S( -5,  19),   S( 22,  14),   S( 13,  30),   S( 13,  19),   S(  3,  16),   S( -4,  48),
            S(-10,  14),   S(  2,  31),   S( 11,  43),   S( 23,  45),   S( 30,  46),   S( 24,  36),   S( 29,  40),   S( 12,  31),
            S( 14,  23),   S(  2,  24),   S( 23,  30),   S( 24,  53),   S( 19,  61),   S( 26,  61),   S( 30,  51),   S(  3,  24),
            S(  1,   8),   S( 14,   8),   S( 35,   8),   S( 56,  22),   S( 56,   7),   S( 60,   3),   S(  5,  26),   S(-14,  56),
            S(  5,   4),   S( -7,   7),   S( 15,   0),   S( 13,  13),   S( 58, -19),   S( 29, -10),   S( 40, -48),   S( 59, -17),
            S(-160,  26),  S(-12, -20),   S(-27,   1),   S(-20,  32),   S( 34,   6),   S(-34,  -7),   S(  9, -27),   S(-74, -26),

            /* knights: bucket 4 */
            S(  5,  -7),   S(-46,  -2),   S(-21,  11),   S( 26, -25),   S(-15,  -9),   S(-51, -36),   S(-27, -40),   S(-24, -36),
            S(  3,   1),   S(-25,  23),   S(  6, -26),   S( 23, -27),   S( 49, -28),   S(  6, -33),   S( 28, -26),   S(  0, -50),
            S(-20,  13),   S( 46,  56),   S( 56, -32),   S( 35,   4),   S( 16, -14),   S(-16,  19),   S(-38, -32),   S(-16, -67),
            S(  8,  73),   S( 29, -50),   S( 59,   5),   S( 12,  11),   S( -4,  28),   S(119, -33),   S( -1, -42),   S( -9, -50),
            S( 57,  36),   S(-36,  52),   S( 87,  26),   S( 26,  14),   S( 63,   8),   S(-20,  -4),   S(-10, -47),   S( -6, -34),
            S(  3,   1),   S(-47,   3),   S( 94,  -8),   S( 20,   4),   S( 26,   2),   S( 39,   0),   S(  9, -12),   S(-14, -33),
            S( -4,   7),   S( 18,  24),   S(  3,  19),   S( 15,  18),   S( -4,   4),   S( 19,  -2),   S( -3, -15),   S(-10,  -2),
            S(-21,  -7),   S( -1,  -5),   S( 23,  34),   S( -9, -15),   S( -8, -25),   S( 10,  14),   S( -4,  -1),   S( -4, -15),

            /* knights: bucket 5 */
            S( 10,   9),   S(-43,  11),   S( 24,  20),   S( 42,  36),   S( 73,  17),   S( -8, -29),   S( -4,  24),   S(-21, -61),
            S( 28,  18),   S( 55,  11),   S( 33,   7),   S(  7,  16),   S( 72,   5),   S( 15,  20),   S( 36,  11),   S(-28, -21),
            S(  4,  12),   S(-13,  25),   S( 82,   5),   S( 82,  10),   S( 48,   9),   S( 32,  18),   S( 14,   7),   S( 21,  -6),
            S( 35,  39),   S( 54,  20),   S( 88,  19),   S( 34,  32),   S( 49,  24),   S( 31,  28),   S( 41,  24),   S(-13,  36),
            S( 16,  41),   S( 48,  18),   S( 59,  31),   S( 99,  24),   S( 91,  37),   S( 66,  24),   S( 20,  33),   S( 25,  18),
            S( 19,  22),   S(  5,  31),   S( 35,   9),   S( 33,  48),   S( 32,  36),   S( 16,  45),   S(  5,  -2),   S(-13,   9),
            S( 27,  75),   S(  6,  61),   S( 18,  20),   S( 33,  40),   S(  7,  25),   S( 27,  49),   S( 13,  49),   S(-19, -25),
            S(  4,  22),   S(  1,  -6),   S(  5,  25),   S(-20, -36),   S( 18,  55),   S(  0,  22),   S(  8,  30),   S(-26, -30),

            /* knights: bucket 6 */
            S(  0, -33),   S(-24,   0),   S( 58,   6),   S(-26,  34),   S( -8,  24),   S(  8,  39),   S(-53,  20),   S(-16,  22),
            S(-14, -53),   S( 58,  -9),   S( 30,   1),   S(  8,  14),   S(-14,  45),   S( 77,  22),   S( 57,  19),   S( 12,   4),
            S( 31, -51),   S( 18,   0),   S( 28,  10),   S( 50,  14),   S( 40,  32),   S(  0,  42),   S( 43,  35),   S( 14,  47),
            S( 39,  14),   S( 51,   7),   S( 76,  21),   S( 94,  13),   S( 71,  21),   S( 52,  41),   S(  6,  56),   S( 21,  57),
            S( -9,  33),   S(100, -16),   S( 87,  22),   S( 99,  31),   S(116,  32),   S(107,  31),   S( 74,  20),   S( 37,  40),
            S( 24,  22),   S( 45,  -6),   S( 43,  30),   S( 67,  26),   S( 63,  57),   S( 55,  34),   S( -1,  28),   S( 40,  38),
            S(-19,   4),   S(  1,  27),   S(-26,  19),   S( 21,  11),   S(  5,  32),   S( 27,  46),   S( -8,  38),   S(-10,   0),
            S(-30,  16),   S( 10,  46),   S( 22,   4),   S(  7,  30),   S( 14,  11),   S( -2,   9),   S( 25,  47),   S(  1, -21),

            /* knights: bucket 7 */
            S(-38, -80),   S(-197, -43),  S(-59, -41),   S(-65, -28),   S(-24, -16),   S(-17, -16),   S( -5,  -9),   S(-16,  -5),
            S(-32, -97),   S(-33, -52),   S(-23, -47),   S( -3, -21),   S(-37,   5),   S( 14, -18),   S( 11,  37),   S(  2,  15),
            S(-85, -82),   S(-40, -38),   S(-52,  16),   S( 53, -40),   S( 22,  -7),   S( 40,  -4),   S(-12,  57),   S( 30,  78),
            S(-40, -28),   S( 21, -42),   S( 10,   1),   S( 70, -14),   S( 84, -10),   S( 19,  13),   S( 26,   4),   S( -9,  40),
            S(-57, -13),   S(-25, -32),   S( 64, -24),   S(103, -18),   S(107,   1),   S( 71,  22),   S(133,  -4),   S( 73,   4),
            S(-17, -43),   S( 17, -30),   S( -1,   3),   S( 67, -11),   S( 65,  -1),   S( 91,   7),   S(100, -14),   S(  4,   3),
            S(-48, -48),   S(-73, -36),   S(  8, -24),   S( 54,   4),   S( 76,  17),   S( 56,  -8),   S(  6,  18),   S( 13,  -4),
            S(-60, -57),   S(  4,   0),   S(-23, -35),   S( 19,  13),   S( 22,  13),   S( -5, -30),   S( -2, -40),   S( -4, -10),

            /* knights: bucket 8 */
            S( -3, -11),   S( -8, -10),   S( -5,   2),   S( -8, -30),   S(-19, -60),   S(-12, -43),   S(  3,  11),   S(-10, -34),
            S(  4,  14),   S(-11, -25),   S(-17, -29),   S(-23, -52),   S(-17, -25),   S(-11, -42),   S( -8, -54),   S(-23, -44),
            S( 13,  28),   S( -5, -17),   S(  6,  -7),   S(-21,  -6),   S( -2, -28),   S(-22,  -8),   S( -7, -48),   S(-11, -68),
            S(  0,  17),   S( -5,  16),   S( -1,  34),   S(-17,  20),   S( -8,  11),   S( 24,  17),   S( -7, -31),   S( -3, -19),
            S( 30,  79),   S(  5,  40),   S( 12,  12),   S( 16,  34),   S(  1,  30),   S( 12,  11),   S(  7,  -2),   S(-11, -13),
            S( 13,  41),   S( 13,  21),   S( 43,  26),   S( 51,  17),   S(-10, -13),   S( -4,  -8),   S( -6, -47),   S(-14,  -9),
            S(  3,   8),   S(-12, -10),   S(  4,   2),   S( 16,  25),   S(  3, -26),   S(  8,  22),   S( -2,  14),   S(  1,  -6),
            S(  6,  13),   S(  4,  15),   S(  2,  17),   S(  6,   6),   S(  0,  -6),   S( -9, -27),   S(  7,  34),   S( -6, -17),

            /* knights: bucket 9 */
            S(-10, -40),   S(-17, -51),   S(-12, -35),   S( -1,  -6),   S(-26, -66),   S(-25, -59),   S( -4, -26),   S( -3, -12),
            S( -6, -42),   S(-15, -21),   S( -9, -51),   S(-20,  -2),   S(-19, -28),   S( -6, -38),   S(  3,  10),   S(-19, -73),
            S(  5,  -6),   S(-10, -23),   S( -6,  -1),   S(-14,  -8),   S(  3,   9),   S(-37, -16),   S(-19,  -7),   S( -1,  11),
            S(-11, -32),   S(  1, -14),   S( 10,   2),   S( 35,   9),   S( 30,  19),   S(  5,   7),   S(-17, -48),   S( -9, -38),
            S( 13,  34),   S( 18,   7),   S( 14,  31),   S( 19,  31),   S(  5,  13),   S(  7, -14),   S( -1, -40),   S( -3, -11),
            S( -5, -10),   S(  8,  34),   S(  9,  29),   S( 12,  -3),   S( 22,  28),   S(  9,   0),   S(  1,  29),   S(-14, -38),
            S(  4,  14),   S(-16, -13),   S( 16,  37),   S( -2,   7),   S( 14,  62),   S( -7, -24),   S(  1,   2),   S(  3,  23),
            S( -2, -12),   S(  9,  38),   S( 14,  30),   S( 17,  56),   S(  6,   5),   S(  6,  42),   S(  6,  18),   S( -1, -14),

            /* knights: bucket 10 */
            S(-19, -58),   S(-20, -78),   S( -6, -46),   S(-21, -30),   S(-11, -18),   S( -6, -31),   S( -1,  -7),   S( 10,  35),
            S(  0, -36),   S( -7, -47),   S( -2,  -4),   S(-22, -25),   S(-36, -54),   S(-12, -53),   S( -7,  -6),   S( -7, -28),
            S(-16, -83),   S(-22, -66),   S(  8,  -2),   S( -6, -10),   S( -2,   5),   S( 12, -20),   S(-15, -30),   S( -5,  -9),
            S(-14, -28),   S( -5, -43),   S(-10, -41),   S( 24,  -7),   S( 28,  25),   S( 28,   7),   S( 17,  28),   S( 17,  41),
            S(  2, -41),   S(-17, -37),   S( 20,  27),   S( 29,  17),   S( 37,  25),   S( 18,   3),   S( 16,  16),   S(  9,  48),
            S(-12, -31),   S(  6,   7),   S(-20, -21),   S(  5,   5),   S( 30,  40),   S( 23,  46),   S( 18,  59),   S( 14,  48),
            S(  5,  10),   S(-15, -34),   S(-11, -23),   S( 16,  11),   S( 11,  56),   S(  6,  31),   S( 10,  19),   S( 15,  41),
            S( -7, -30),   S(  4,   4),   S( -9,  -7),   S( -3,  -4),   S(  2,   7),   S(  6,  42),   S(  4,  26),   S( -1,  -2),

            /* knights: bucket 11 */
            S(  1,  -3),   S( -5, -31),   S( -6, -18),   S(-22, -57),   S(-18, -43),   S(-12, -36),   S( -9, -25),   S( -7, -10),
            S( -4,  -8),   S(-13, -38),   S(-10, -95),   S(-15, -22),   S(-12,  -1),   S(-41, -58),   S( -9,  -4),   S( -9,  -7),
            S(-16, -62),   S(-27, -50),   S(-35, -25),   S( 11,   4),   S(-29,   9),   S(-26,  10),   S(  0,  10),   S(  1,  18),
            S(-23, -47),   S(-11, -50),   S(-21,  12),   S(  6,  27),   S( 19,   0),   S(  3,  25),   S( -8,  19),   S( 19,  26),
            S( -7, -46),   S(-21, -69),   S( -4, -13),   S( -1, -17),   S( 12,  20),   S( 44,  34),   S( 10, -21),   S( 32,  95),
            S(-13, -22),   S(-11, -30),   S( 15,  10),   S( 34,  36),   S( 11,  31),   S( 53,  49),   S( 16,  24),   S( -9,  24),
            S(  5,  32),   S(  0, -11),   S( 13,   9),   S( 15,   3),   S( 18,  42),   S(-12,  35),   S(  7,  40),   S(  9,  47),
            S( -1,  -6),   S( -7, -38),   S( 14,  10),   S(  6,   6),   S( -2,  14),   S(  9,  13),   S(  4,  12),   S(  9,  53),

            /* knights: bucket 12 */
            S( -1,  -6),   S( -1,   3),   S(  1,   1),   S( -1,   6),   S( -5,  -2),   S( -2,  -1),   S(  4,   8),   S( -3, -17),
            S( -2,  -4),   S( -1,  -4),   S(  3,  16),   S(  0, -10),   S(  5,  19),   S( -5, -34),   S( -3, -20),   S( -5, -19),
            S(  0,  -9),   S(  4,   2),   S( -6,  -1),   S( -2,   7),   S(  8,  19),   S(  2, -29),   S(  1,  -4),   S(-12, -39),
            S( -8, -17),   S(  2,  -5),   S(-11, -40),   S(  7,  47),   S(  3,   6),   S(  5,  16),   S( -3,  -8),   S( -2, -17),
            S( 13,  37),   S(  1,  -9),   S(  3,   0),   S( -1,  21),   S(  9,  25),   S(  5,  20),   S(  2,   6),   S(  3,  13),
            S(  4,  33),   S( -2, -19),   S(  6,  50),   S( -2, -17),   S(  8,   9),   S( -3, -12),   S(  2,  -8),   S(  1,   9),
            S(  1,   4),   S( -7,   0),   S(  4,   4),   S(  5,   2),   S( -4, -13),   S(  0,  -5),   S( -3, -10),   S( -1,  -7),
            S(  7,  19),   S(  6,  29),   S(  1,   4),   S(  3,  13),   S( -1,  -7),   S( -2,  -8),   S( -4, -15),   S(  0,   3),

            /* knights: bucket 13 */
            S( -6, -10),   S(  0,  -2),   S(  0,   0),   S(  0,  -6),   S(-11, -31),   S(  0,  -4),   S( -4, -20),   S(  0,   5),
            S( -2,  -8),   S(  2,  15),   S( -1, -15),   S( -4, -19),   S(-11, -52),   S(  0, -21),   S( -2,  -3),   S( -3, -13),
            S( -2,  -5),   S( -7, -23),   S( 10,  23),   S( -1,  11),   S( -8, -27),   S(-10, -27),   S( -2, -14),   S( -7, -31),
            S(-11,  -8),   S( 12,  40),   S(  4,  12),   S( -6,   8),   S( -2, -27),   S( 10,  14),   S( -2, -20),   S( -8, -18),
            S(  1,   2),   S(  2,  30),   S( 10,  19),   S( 18,  62),   S(  2,  23),   S(  0,   4),   S( -2, -11),   S(  4,  17),
            S(  3,  28),   S(  5,   6),   S( 13,  84),   S( -9,  28),   S(  1,  33),   S(-11, -38),   S(  2,   7),   S( -1,  -3),
            S(  1,   0),   S( -3,  22),   S( 18,  50),   S(  5,   0),   S( 13,  44),   S( -9, -18),   S( -5,  -2),   S( -5, -10),
            S(  1,   4),   S(  2,  52),   S(  7,  28),   S(  3,  29),   S( -8,  -6),   S(  4,  15),   S( -2,  -5),   S(  0,   1),

            /* knights: bucket 14 */
            S( -2, -21),   S( -9, -41),   S( -4,  -9),   S( -5, -23),   S(-12, -40),   S( -2, -14),   S( -1,  -8),   S(  3,   9),
            S( -4, -23),   S( -2, -12),   S(-14, -60),   S( -6, -25),   S(  3,   8),   S(  1,   1),   S(  2,   1),   S(  0,   2),
            S( -6, -15),   S(-10, -44),   S(-22, -63),   S(  4,  30),   S(  3,  -1),   S(  0,  -2),   S( -2, -10),   S(  2,  -4),
            S(  0,   0),   S( -6, -22),   S(-10, -25),   S(-12, -26),   S( -2, -33),   S(  7,   6),   S( -2, -14),   S( -5,  -8),
            S(  1,   3),   S( -1,  -8),   S( 10,  34),   S( -4, -18),   S(  4,  15),   S(  9,   9),   S( -2, -18),   S(-14, -72),
            S(  0,  -4),   S( 10,  24),   S( -4,   3),   S( -9, -14),   S( 23,  67),   S( -6,  23),   S( -2,   8),   S(  3,  24),
            S(  2,  12),   S( -3, -14),   S(  7,   4),   S(  9,  34),   S( -4,  27),   S( -1,  14),   S(  0,  23),   S(  4,  21),
            S(  0,   0),   S(  4,  12),   S( -1,   7),   S( 14,  64),   S(  7,  20),   S( 10,  53),   S(  3,  30),   S(  1,   6),

            /* knights: bucket 15 */
            S( -2,  -6),   S( -1, -21),   S( -1, -26),   S( -5, -16),   S(  0,   7),   S( -6, -26),   S(  0,  -6),   S( -3, -10),
            S( -1, -10),   S(  3,   5),   S( -7, -28),   S( -4, -15),   S(  5,  18),   S(  1,   1),   S(  0,  -6),   S(  1,   2),
            S( -5, -15),   S(  3,   9),   S(  2,  -7),   S(-14, -52),   S(  5,  13),   S(  0,  -1),   S( -3, -17),   S( -1,   3),
            S( -9, -28),   S( -7, -33),   S( 10,   6),   S( -9,  -5),   S( -9, -21),   S( 15,  37),   S(  1,   8),   S( -1,  -1),
            S( -2,  -7),   S( -1,  15),   S(  1,  -8),   S( -3, -32),   S(  9,  16),   S(  5,  10),   S(  0,  -4),   S( -2,  -4),
            S( -3,   0),   S( -3, -12),   S(  4,  17),   S(  8,  20),   S( -3,  -9),   S( -2,  28),   S(  1,   3),   S(  6,  45),
            S( -4, -13),   S( -4,  -7),   S(  0,   1),   S(  2,   9),   S( -5,  -8),   S( -3,  11),   S( -4, -24),   S(  1,  17),
            S(  0,  -2),   S( -1,   0),   S( -3,  -8),   S(  3,   3),   S(  0,   8),   S( -9, -27),   S( 13,  57),   S( -1,  -8),

            /* bishops: bucket 0 */
            S( 17,  46),   S( 21,  -2),   S( 52,  17),   S(  1,  16),   S(  3, -15),   S(  1, -11),   S( 18, -19),   S(  0, -43),
            S( 57, -18),   S( 93,  19),   S( 27,  37),   S( 22, -14),   S(  0,  29),   S( -1,   2),   S(-25,   4),   S(  2, -32),
            S( 25,  30),   S( 45,  26),   S( 36,  39),   S(  6,  59),   S( 22,  29),   S(-19,  51),   S(  1,   6),   S(  9, -44),
            S( 38, -26),   S( 83,  -2),   S( 45,  17),   S( 40,  26),   S(  1,  50),   S( 28,  12),   S(  7,  -1),   S(  6,  -6),
            S( 11,   6),   S( 41,  18),   S(-18,  65),   S( 36,  50),   S( 46,  21),   S(  5,  35),   S( 10,  12),   S(-55,  30),
            S(-47,  60),   S( -6,  50),   S( 38,  54),   S( 37,  42),   S( 16,  62),   S( 11,   3),   S(-29,  40),   S(-13,  45),
            S(-69,  96),   S(-12,  42),   S(  8,  30),   S(-29,  77),   S(-35,  43),   S( 55,  16),   S(  5,  19),   S(-51,  27),
            S(-38, -15),   S(-20,  31),   S(-21,  59),   S(  0,  23),   S( 21,  18),   S( 20,  16),   S(-10,  45),   S( -8,  31),

            /* bishops: bucket 1 */
            S( 24,  62),   S(-16,  15),   S(  0,  33),   S( -3,  37),   S( 16,  14),   S( -4,  28),   S(-43,  44),   S(-20,   8),
            S( 39, -26),   S( 41,  14),   S( 46,  20),   S( 26,  36),   S( -3,  27),   S( 20,  -5),   S(-16,  15),   S( 14,  21),
            S( 39,   8),   S( 15,  23),   S( 51,  30),   S(  7,  39),   S( 15,  41),   S(-13,  47),   S( 31,   8),   S(  6, -28),
            S( 47,   1),   S( 35,  30),   S(  4,  37),   S( 20,  52),   S( -8,  53),   S( 19,  37),   S(-12,  35),   S( 21,  21),
            S( 33,  40),   S(  7,  45),   S(  7,  39),   S(  1,  47),   S( 20,  36),   S(-22,  52),   S( 13,  20),   S( 13,   4),
            S( -6,  31),   S( 32,  40),   S( 20,  74),   S( 46,  18),   S(-47,  84),   S(  0,  48),   S(-10,  61),   S( 36,   0),
            S( 16,  32),   S( -3,  46),   S( 27,  33),   S( 45,  42),   S( 55,  26),   S(-57,  60),   S(-11,  73),   S(-49,  73),
            S( -7,  47),   S(-29,  33),   S( -4,  44),   S(-21,  48),   S( -7,  31),   S(-22,  56),   S(  0,  65),   S( -5,  93),

            /* bishops: bucket 2 */
            S(  5,  41),   S( -9,  42),   S(  0,  33),   S(-15,  41),   S(-15,  38),   S(-23,  27),   S(-30,   4),   S(-59,  64),
            S(-17,  26),   S( 17,  24),   S( 21,  24),   S(  4,  39),   S(  1,  47),   S(  5,  27),   S(  5,  -2),   S(  6, -48),
            S(-11,  21),   S( -2,  34),   S( 10,  60),   S( -8,  65),   S(  2,  52),   S(  7,  50),   S( 12,  35),   S(-15,  -6),
            S(-15,  35),   S(-33,  60),   S(-23,  66),   S(  0,  61),   S( -7,  68),   S(  0,  46),   S( 20,  29),   S(  1,  25),
            S(-22,  48),   S( -8,  44),   S(-30,  59),   S(-29,  61),   S(-31,  78),   S( -1,  52),   S( -2,  35),   S(-15,  29),
            S(  4,  48),   S(-21,  43),   S(-43,  81),   S(-41,  66),   S( 26,  32),   S(-13,  65),   S( 23,  56),   S(  5,  30),
            S(-50,  70),   S(-37,  63),   S(-39,  78),   S(  6,  46),   S(-45,  59),   S(-29,  41),   S(-64,  62),   S(  6,  30),
            S(-107,  95),  S(-44,  76),   S(-76,  63),   S(-62,  72),   S(-65,  52),   S(-81,  65),   S(-13,  35),   S(-77,  49),

            /* bishops: bucket 3 */
            S(  0,  32),   S(  2,  43),   S( 15,  28),   S(  4,  38),   S(  1,  30),   S( 44,   5),   S( 30,   2),   S( 34, -18),
            S( -2,  34),   S( 11,  37),   S( 14,  27),   S(  7,  54),   S( 18,  39),   S( 10,  47),   S( 52,  21),   S( 23,  11),
            S( 15,  23),   S(  3,  49),   S( 10,  69),   S( 15,  53),   S( 11,  78),   S( 20,  57),   S( 32,  46),   S( 37,   3),
            S(  7,  30),   S(  4,  57),   S(  0,  72),   S( 17,  74),   S( 17,  72),   S( 23,  53),   S( 21,  62),   S( 15,  25),
            S( -8,  42),   S( 20,  45),   S(  8,  53),   S( 15,  75),   S(  9,  77),   S( 30,  56),   S(  4,  58),   S( 17,  51),
            S( 13,  58),   S( 14,  47),   S(  7,  69),   S( 18,  53),   S(  3,  66),   S( 40,  68),   S( 20,  55),   S(  0,  71),
            S( -2,  44),   S(-28,  71),   S( 32,  51),   S(  7,  52),   S( -7,  56),   S(-21,  53),   S(  0,  74),   S(-10,  79),
            S(-29,  97),   S(-26,  67),   S( 47,  35),   S(-38,  76),   S(-41,  65),   S(-44, 103),   S( 16,  49),   S( 69,  11),

            /* bishops: bucket 4 */
            S(-13,  -8),   S(-42,  -8),   S(-61,   4),   S(-43,  19),   S(-41,  35),   S(-21,   6),   S( 10, -13),   S(-11, -14),
            S(-18,  26),   S( -3,   8),   S(-16,  45),   S( -1,  11),   S( -1,   3),   S( 75, -13),   S(  2,   3),   S( 19, -17),
            S(  3,  27),   S(-42,  21),   S( 37,  28),   S(-15,  30),   S( 20,  24),   S( 41,  -2),   S(  6, -22),   S(-59,  12),
            S(-36,  37),   S( -8,  33),   S( 63,   5),   S( 55,  27),   S( 15,  16),   S( 30,  15),   S(  0,  30),   S(-14,   1),
            S( 22,  -3),   S(-35,  63),   S(  7,  66),   S( 40,  27),   S(  5,  40),   S(-23,  17),   S(-29,  11),   S(-11,   5),
            S(-10,   5),   S( 39,  35),   S( -4,  34),   S( 23,  19),   S( 19,  24),   S( 25,  33),   S(  7,   5),   S(-24,  -1),
            S(-51,  16),   S( 33,  34),   S( -1,  22),   S( 19,  48),   S( -8,  23),   S( -9,  31),   S(  5,  23),   S(  5,  -4),
            S( 11,  36),   S(-17,  -6),   S(  4,  21),   S( -1,  11),   S(  7,  18),   S(  2,  25),   S( -6, -33),   S(  7,  42),

            /* bishops: bucket 5 */
            S(-10,  -5),   S( -8,  35),   S(-43,  34),   S( -7,  26),   S(-37,  45),   S( -2,   9),   S(  6,  33),   S(-20,  22),
            S(-24,  23),   S(-51,  58),   S(-28,  57),   S(  4,  32),   S( -3,  34),   S( -2,  25),   S(-41,  25),   S(  4,  11),
            S(-14,  34),   S(-12,  52),   S(-11,  54),   S( 29,  30),   S( 16,  37),   S(-40,  35),   S(-27,  36),   S(-14,   0),
            S(  5,  36),   S( 30,  42),   S( -6,  54),   S( 31,  37),   S( 21,  37),   S( 62,  21),   S(-17,  25),   S(-10,  38),
            S( 45,  48),   S( 28,  25),   S( 76,  25),   S( 97,  36),   S( 37,  35),   S( 45,  22),   S( 41,  27),   S(-40,  37),
            S( -3,  40),   S( 37,  38),   S( 41,  47),   S( 28,  42),   S( -3,  56),   S( 32,  12),   S(  1,  31),   S( 17,  58),
            S( 28,  63),   S(-36,  46),   S( 12,  38),   S(  2,  57),   S(  8,  58),   S( 24,  60),   S( -9,  27),   S(  7,  11),
            S(-22,  44),   S( 19,  24),   S( -6,  20),   S( -8,  35),   S(  3,  46),   S(-14,  50),   S( -1,  72),   S( -3,  15),

            /* bishops: bucket 6 */
            S( -2,  29),   S(-46,  51),   S(-64,  53),   S(-29,  30),   S(-36,  29),   S(-52,  44),   S(-13,  34),   S( -2,  13),
            S(  5,  28),   S( -5,  28),   S( 12,  31),   S(-17,  42),   S(  3,  34),   S( -9,  32),   S(-80,  56),   S( 32,  38),
            S( 35,   0),   S(  8,  23),   S( 12,  48),   S( 35,  31),   S( 67,  26),   S( 31,  34),   S(-17,  44),   S(-59,  29),
            S( 34,  31),   S(-24,  53),   S( 39,  33),   S( 34,  56),   S( 44,  43),   S( 59,  27),   S( 12,  46),   S(-25,  20),
            S(-22,  48),   S( 55,  32),   S( 85,  11),   S( 87,  17),   S( 95,  36),   S( 53,  34),   S( 70,  39),   S(-12,  42),
            S( -5,  34),   S(-13,  39),   S( 26,  42),   S( 20,  51),   S( 17,  53),   S( 38,  51),   S( 39,  38),   S(-52,  68),
            S(-51,  39),   S(-19,  39),   S( 13,  39),   S( 30,  32),   S(  2,  50),   S(-11,  45),   S( 21,  44),   S(-15,  49),
            S( 16,  78),   S(-12,  42),   S( -6,  32),   S(  9,  49),   S(-12,  36),   S( 13,  47),   S(  1,  14),   S( -3,  51),

            /* bishops: bucket 7 */
            S(-35,  -5),   S(  6,  17),   S(-61,   5),   S( -9,   0),   S(-28,  -7),   S(-84,  19),   S(-51, -47),   S(-10, -12),
            S(-64,   1),   S(-59,   6),   S(-13,   8),   S( 24,  -3),   S(-21,  18),   S(-47,  39),   S(-28,   6),   S(-62,  -8),
            S(-37,  14),   S( 19, -14),   S(  6,  24),   S( 39,   5),   S(-12,  24),   S(  4,   9),   S(-38,  29),   S(-15,  29),
            S(-19,  16),   S( 27,  15),   S( 71,  -1),   S( 90,   8),   S(101,   3),   S(  9,  28),   S(  0,  57),   S( 28,  23),
            S(  2,   6),   S(-32,  12),   S( 52,   9),   S(101, -10),   S(110,  16),   S(112,  10),   S( 32,  31),   S( 41,  -4),
            S(-27,  15),   S( 21,   8),   S( 32,   1),   S( 42,   2),   S( 60,  11),   S( 79,  19),   S( 68,  19),   S(  2,  28),
            S( 12,   6),   S(-12,   9),   S(-18,  14),   S( -8,  22),   S( 18,  -3),   S( -1,  10),   S( 20,   3),   S( 12,  18),
            S(  1,  18),   S( -6,  22),   S(-50,  -3),   S( 10,  11),   S( 13,   3),   S(  4,  -6),   S( 45,  16),   S( 25,  31),

            /* bishops: bucket 8 */
            S(-10, -50),   S(-13, -63),   S(-14, -17),   S(-12, -27),   S(  1, -13),   S( -8, -11),   S( 15,  31),   S( -3, -18),
            S( -5, -33),   S(-18, -63),   S(-13, -46),   S( -7, -26),   S( 20, -14),   S( -3, -32),   S(-13, -38),   S( -7, -46),
            S( -8,  -9),   S(-11,  -7),   S(  8,  13),   S(  9, -48),   S(  1,  -9),   S( 12, -16),   S( -5, -55),   S(-32, -47),
            S(  0,  31),   S( 10,  14),   S( 19,  13),   S( 13, -11),   S(  9,   0),   S( 13, -15),   S( 14, -12),   S( -8, -41),
            S( 13,  54),   S( -1,  14),   S( 13, -10),   S( 41,  -5),   S( 22, -22),   S( 22,  14),   S( 13, -13),   S( -8, -36),
            S( -9,  -1),   S(  5, -15),   S( 19,  -2),   S(  1, -20),   S( 34, -21),   S(  1, -25),   S(  4, -30),   S(-20, -52),
            S(-15,   4),   S( 35,  22),   S(  8,  -3),   S( -8, -33),   S( -6, -42),   S(  4, -17),   S(  0, -42),   S( -8, -26),
            S( -9, -40),   S(  0, -38),   S( -1, -23),   S( -9, -47),   S(-10, -49),   S( -4, -32),   S( -5, -26),   S( -6, -34),

            /* bishops: bucket 9 */
            S(-17, -55),   S( 12, -46),   S(-17, -13),   S( -6, -22),   S(-22, -61),   S(-20, -77),   S( -2, -25),   S(  9,  14),
            S(-13, -51),   S( -2, -45),   S(-18, -30),   S( 12, -21),   S( -9, -26),   S( -3, -24),   S(-11, -54),   S( -5, -25),
            S( 15,  10),   S( 21,  -6),   S(  8, -26),   S( 17,  -9),   S( 41, -14),   S( 50, -32),   S(-24, -28),   S(  2, -30),
            S( -8, -21),   S( 33,  12),   S(  7,   4),   S( 46,   4),   S( 43, -13),   S( 28, -12),   S( -1, -31),   S( -4, -40),
            S( -8,  -8),   S( 22,  -4),   S( 35,   8),   S( 37,  -1),   S( 41, -26),   S( 25, -28),   S( 16, -12),   S( -5, -16),
            S( -8, -29),   S( 48,  10),   S( 11,  24),   S( 18,  15),   S( 44, -34),   S( 34, -37),   S(  6, -38),   S(-16, -49),
            S(  0,  -3),   S( 27,   9),   S(  7, -14),   S( 44,  12),   S( 34,  -2),   S(  8, -28),   S( -5, -38),   S(-12, -52),
            S(  1,  -5),   S(  0, -15),   S(  2, -33),   S( -7, -79),   S( -3, -30),   S( 10,  -2),   S(  5, -13),   S(-13, -68),

            /* bishops: bucket 10 */
            S(-24, -59),   S(  5, -52),   S(-36, -42),   S(-11, -50),   S( -7, -48),   S(  6, -44),   S(-11, -70),   S(-18, -83),
            S(  1, -46),   S(-21, -56),   S(  4, -53),   S( -5, -33),   S(-13, -31),   S(  4, -68),   S(-15, -48),   S(-17, -39),
            S( -1, -64),   S( 12, -40),   S(-14, -49),   S( 32, -27),   S( 33, -40),   S(  6,  -8),   S( -9, -17),   S(  6, -12),
            S(-28, -72),   S( 37, -25),   S( 41, -29),   S( 50, -36),   S( 60, -13),   S( 24,  -1),   S( -8, -29),   S(  4,  12),
            S( -5, -30),   S( 22, -46),   S( 43, -42),   S( 83, -17),   S( 48,  -4),   S( 46, -10),   S( 26,  26),   S(  7, -49),
            S( -8, -38),   S( 12, -51),   S( 30, -43),   S( 31, -33),   S( 35, -19),   S( 45,   9),   S( 11, -22),   S(  1, -23),
            S(-18, -87),   S(-11, -75),   S(  4, -44),   S( 18, -41),   S(  1, -53),   S( 28, -10),   S( 20,  29),   S( 18,  24),
            S( -4, -52),   S( -6, -52),   S(  6,   6),   S(-11, -52),   S( -2, -25),   S(  0, -28),   S( -2, -21),   S(  4,  15),

            /* bishops: bucket 11 */
            S(  2,   0),   S(-43, -22),   S(-59, -70),   S(-18, -31),   S( -8, -27),   S(-76, -81),   S( -9, -51),   S(-23, -64),
            S( -3, -53),   S(  7, -25),   S( -4, -39),   S(-14, -45),   S(-16, -32),   S( -6, -66),   S(-28, -84),   S(-36, -62),
            S( -6, -81),   S(  6, -59),   S( 15, -34),   S( 25, -44),   S( -3, -36),   S(  9, -14),   S(-13,  -6),   S(-14, -40),
            S(-12, -50),   S(-11, -45),   S( 17, -45),   S( 29, -18),   S( 66, -35),   S( 27,   7),   S( 45,  -5),   S(  6,  45),
            S(-10, -54),   S(-12, -78),   S( 17, -32),   S( 79, -32),   S( 51,  -5),   S( 63, -12),   S( 20,  33),   S( -1, -23),
            S(-23, -54),   S(  3, -53),   S( 19, -68),   S( 43, -24),   S( 20, -16),   S( 36,  21),   S( -3,  31),   S(  1,  -5),
            S( -6, -32),   S( -7, -52),   S( -5, -34),   S(  8, -38),   S( 18, -11),   S( 27, -27),   S( 11,  -5),   S(  6,   3),
            S(-12, -79),   S(-22, -44),   S(  1, -25),   S(  4, -23),   S(  6, -24),   S(-11, -83),   S( -3,  -9),   S(  5,  -3),

            /* bishops: bucket 12 */
            S(  2,   1),   S(-10, -46),   S( -4, -49),   S( -2, -27),   S( -8, -29),   S( -7, -20),   S( -4,  -9),   S(  0,   2),
            S(-10, -39),   S( -9, -44),   S(  0, -21),   S(  2, -11),   S(-10, -39),   S( -3, -16),   S( -3, -19),   S(  2,  10),
            S(  1, -20),   S( -8, -20),   S(-13, -30),   S( -2, -21),   S(  0,  -5),   S(  0, -36),   S(-10, -27),   S( 10,  16),
            S( -2, -30),   S( -2, -29),   S( -1, -25),   S(  8,   1),   S(  9,  15),   S( 10,   2),   S(  0, -18),   S( -7, -27),
            S( -9, -17),   S(-12, -36),   S( -3, -49),   S( -1,  25),   S( -8, -45),   S( -1,   1),   S(  3, -18),   S( -5, -15),
            S(-26, -44),   S(  2,  22),   S( -6,  15),   S( -3, -23),   S( 17, -13),   S( -3, -16),   S(  5, -30),   S( -2,  -8),
            S( -4, -15),   S( -7, -10),   S( -3,  14),   S( -7, -31),   S( -2, -20),   S(  8,   5),   S( -3, -16),   S( -6, -19),
            S( -1, -12),   S( -3,   7),   S(  0, -40),   S( 10,  28),   S(  7,   4),   S( -2, -24),   S(-10, -43),   S( -2,   2),

            /* bishops: bucket 13 */
            S(-11, -50),   S(-10, -73),   S( -9, -54),   S(-10, -46),   S(-15, -75),   S( -9, -13),   S(  0, -14),   S( -7, -50),
            S(  1, -19),   S( -6, -53),   S(-11, -86),   S(-13, -52),   S(  3, -20),   S(  2,   2),   S( 12,  19),   S(  1, -35),
            S( -3, -30),   S( -6,  -6),   S(  2, -30),   S( -1, -64),   S( -9, -88),   S( 19,   5),   S( -6, -31),   S(  5,  14),
            S( -3,  -1),   S(-11, -60),   S(  1, -29),   S(  7, -42),   S( 16, -40),   S(  8,  10),   S(  3,  -2),   S(-10, -53),
            S( -1,  10),   S(-15, -15),   S(  6, -44),   S( 16, -13),   S(  6,   0),   S(  5, -22),   S(  3, -52),   S( -6, -46),
            S( -5, -20),   S( -3, -25),   S(-18, -28),   S( 26,  11),   S( 12,  12),   S(  9, -29),   S(  6, -34),   S( -7, -39),
            S( -6, -35),   S(  6,   9),   S(  2,  14),   S( -5,   2),   S(-12, -55),   S( 10,   5),   S(-12, -61),   S(  3,  -2),
            S(-10, -37),   S( -1, -19),   S( -8, -39),   S(  5, -15),   S( -1, -12),   S( -5, -30),   S( -1,   2),   S( -5, -45),

            /* bishops: bucket 14 */
            S( -2, -32),   S(-13, -63),   S(-25, -84),   S(-17, -63),   S(-10, -45),   S( -8, -46),   S(-13, -77),   S( -6, -30),
            S(-11, -44),   S(  3, -12),   S( -4, -33),   S(-14, -66),   S(-16, -64),   S(-11, -72),   S(-16, -60),   S(  1, -37),
            S( -3,  -9),   S(-21, -62),   S( -3, -50),   S(  5, -64),   S( -6, -61),   S(-13, -79),   S(  1, -11),   S( -5, -33),
            S( -6, -30),   S( -8, -43),   S( -2, -64),   S(  4, -20),   S( 14, -48),   S(  3, -44),   S(-10, -58),   S( -4, -12),
            S( -6, -35),   S(  1, -32),   S(  6, -31),   S(  6, -22),   S(  6, -49),   S(  4, -15),   S(  3, -41),   S( -2, -16),
            S(  2, -34),   S( -1, -36),   S(  0, -43),   S( -3, -36),   S(-11, -38),   S( -8,  24),   S(  4, -32),   S( -6, -21),
            S(-10, -65),   S(  2, -33),   S( -4, -33),   S( -1, -14),   S(-13, -35),   S( -8, -20),   S(  6,   9),   S( -5, -24),
            S( -2, -20),   S( -4, -27),   S( -5, -26),   S( -8, -33),   S( -5, -23),   S(  5,  26),   S(  5,  19),   S( -1, -13),

            /* bishops: bucket 15 */
            S(  7,  32),   S( 10,  30),   S(-13, -45),   S( -1,  -6),   S( -4, -33),   S( -7, -17),   S( -5, -23),   S( -3, -11),
            S(  5,  18),   S(  5,  11),   S(  6,  11),   S( -3, -36),   S(-13, -47),   S( -3, -21),   S( -4, -13),   S( -1, -15),
            S( -6, -33),   S( -5, -17),   S( -4, -36),   S(  2,   9),   S( -6, -62),   S(-13, -23),   S(-10, -38),   S(  3,   7),
            S( -4, -28),   S(-14, -46),   S( -9, -48),   S(-22, -80),   S(  2, -17),   S(-11, -52),   S(  5,  23),   S( -5, -29),
            S( -2, -31),   S( -6, -22),   S( -7, -22),   S(-19, -40),   S(  6, -26),   S( -7, -32),   S(  6,  -7),   S( -4, -12),
            S( -8, -49),   S( -6, -37),   S(-18, -64),   S(-17, -68),   S(-11, -65),   S(  2,   2),   S( 14,   0),   S( -3,   3),
            S( -6, -37),   S(  6, -13),   S( -2, -18),   S( -7, -56),   S(  0, -18),   S(-10, -33),   S( -9, -12),   S(  6,  19),
            S( -9, -28),   S(  3,  12),   S( -2, -14),   S(-11, -64),   S(  1, -13),   S(-17, -62),   S( -4, -20),   S(  2,   6),

            /* rooks: bucket 0 */
            S(-39,  17),   S(  8, -12),   S(  6, -11),   S( 16, -25),   S( 15,  -5),   S( 16, -22),   S(  9,  13),   S( 17,  -2),
            S( 35, -85),   S( 60, -49),   S(  8,  -2),   S( 21,  -8),   S( 29,   5),   S( 19, -20),   S(-23,   5),   S(-45,  11),
            S(  7, -16),   S( 33,   4),   S( 43, -14),   S( 11,  22),   S( -2,  34),   S( 12,  -1),   S(-34,  15),   S(-24,  -9),
            S( 45, -44),   S( 52,  16),   S( 47,  33),   S( 56,  -3),   S( 41,  -8),   S( -9,  11),   S( -8,  17),   S(-20,  29),
            S( 55, -11),   S( 73,  -2),   S( 76,   2),   S( 50,  -3),   S( 47,   7),   S( 52,   3),   S( 12,  19),   S(-26,  26),
            S( 85, -47),   S( 75, -31),   S( 47,  10),   S( 33,  12),   S( 64,   0),   S( -3,  18),   S( 51,   6),   S(-19,  27),
            S( 30,  -4),   S( 99, -25),   S( 34,  -1),   S( 17,  25),   S( 10,  25),   S( 29,   9),   S( 10,  19),   S(-18,  22),
            S(  5,  28),   S( 11,  33),   S( 30,  21),   S( 17,  42),   S( 37,  -5),   S( 22, -14),   S(-13,  35),   S( 49, -12),

            /* rooks: bucket 1 */
            S(-68,  32),   S(-37,  10),   S(-37,   0),   S(-32, -13),   S( -9, -18),   S(-11, -14),   S(-17,  -7),   S(-22,  10),
            S(-43,  15),   S(-62,  41),   S(-15,  -5),   S(-14, -23),   S(-18,  -5),   S(-31,  -4),   S(-31, -10),   S(-69,  26),
            S(  9,  16),   S(-28,  39),   S( -7,  17),   S(-43,  40),   S(-20,  20),   S(-11,  16),   S(-14,  11),   S(-35,  19),
            S(-56,  66),   S(-64,  51),   S(  1,  44),   S(-12,  30),   S(-13,  34),   S(-43,  55),   S(-36,  42),   S(-40,  31),
            S( 67,   4),   S( 47,  30),   S( 15,  26),   S(-33,  54),   S(-10,  41),   S( 43,  16),   S( -1,  36),   S(-25,  18),
            S( 69,  12),   S( 11,  34),   S( 47,  27),   S(-17,  31),   S(  0,  14),   S(-31,  36),   S( 18,  13),   S(-40,  45),
            S(-13,  40),   S( 20,  40),   S( 32,  19),   S(-92,  88),   S(-34,  48),   S(-20,  44),   S(-27,  43),   S( -7,  23),
            S( 36,  17),   S( 47,  34),   S(  4,  36),   S(-51,  63),   S( -5,  29),   S( 60,  11),   S( -8,  23),   S( 18,  16),

            /* rooks: bucket 2 */
            S(-68,  47),   S(-46,  31),   S(-40,  31),   S(-61,  33),   S(-52,  23),   S(-58,  26),   S(-39,  -9),   S(-52,  34),
            S(-77,  42),   S(-66,  53),   S(-35,  31),   S(-41,  21),   S(-41,  15),   S(-74,  21),   S(-79,  31),   S(-73,  35),
            S(-79,  77),   S(-53,  57),   S(-48,  62),   S(-36,  31),   S(-43,  46),   S(-27,  36),   S(-12,   5),   S(-13,  12),
            S(-73,  70),   S(-42,  59),   S(-37,  76),   S(-17,  55),   S(-27,  48),   S( -4,  45),   S(-27,  65),   S(-20,  39),
            S(-20,  55),   S(-33,  73),   S(-45,  67),   S(-12,  44),   S( 21,  44),   S( 23,  47),   S(-27,  61),   S(-34,  49),
            S(-33,  60),   S(-22,  56),   S(-22,  50),   S(  5,  37),   S( 21,  40),   S( 68,  14),   S(  8,  41),   S(-39,  53),
            S(-63,  66),   S(-76,  90),   S(-39,  68),   S(-29,  67),   S(  6,  42),   S( 13,  29),   S(-53,  79),   S(-28,  59),
            S(-43,  75),   S(  0,  49),   S(-57,  63),   S(-28,  59),   S(-31,  59),   S( 17,  40),   S(-45,  82),   S( 24,  32),

            /* rooks: bucket 3 */
            S(  6,  75),   S( 11,  66),   S( 18,  66),   S( 20,  55),   S( 23,  55),   S(  0,  63),   S(  1,  73),   S( -3,  43),
            S(-32,  81),   S(-13,  76),   S( 11,  66),   S( 27,  55),   S( 29,  58),   S( 24,  52),   S( 49,  12),   S( 15, -34),
            S(-28,  86),   S( -6,  78),   S( 16,  78),   S( 16,  66),   S( 28,  70),   S( 29,  71),   S( 40,  55),   S(  8,  52),
            S(-15,  96),   S(-10,  90),   S( 31,  81),   S( 37,  75),   S( 40,  66),   S( 15, 105),   S( 74,  61),   S( 18,  78),
            S( -6, 102),   S( 29,  92),   S( 21,  76),   S( 49,  71),   S( 51,  70),   S( 74,  54),   S( 97,  57),   S( 62,  46),
            S( 13,  90),   S( 21,  85),   S( 17,  81),   S( 29,  75),   S( 33,  66),   S( 50,  63),   S(101,  43),   S( 72,  39),
            S(-23, 111),   S(-28, 120),   S( -2, 106),   S( 33,  86),   S( 35,  74),   S( 47,  72),   S( 69,  75),   S(138,  23),
            S(-45, 138),   S( 32,  93),   S( 20,  84),   S( 66,  67),   S( 74,  50),   S( 81,  48),   S(152,  38),   S(120,  40),

            /* rooks: bucket 4 */
            S(-74,  18),   S(  7, -12),   S( -6, -18),   S(  1,   2),   S(-22, -26),   S(  3, -40),   S( -2, -48),   S(-25, -23),
            S(-17,  -7),   S(-45,   3),   S(-57,  24),   S(-50,  33),   S(-19, -29),   S(-15, -34),   S( -1, -38),   S(-28, -27),
            S( -6,  15),   S(-28, -33),   S(-26,  18),   S( -4, -16),   S(-25, -16),   S(  4, -35),   S( 28, -27),   S(-97,  26),
            S(-26, -27),   S(  5,  -6),   S(-33,  32),   S( 26, -13),   S(  7,  -8),   S( 11, -11),   S( 10,  10),   S(-19,  15),
            S(-24,  12),   S(-32,  37),   S( -4,  23),   S( 61,  11),   S( 42,   2),   S(-16,  12),   S( 39,  17),   S( 20,   3),
            S( -2,  -1),   S( 42, -14),   S( 34,  36),   S( 34,  19),   S( 61,  17),   S(  3,   8),   S( -1,  13),   S( 22,  27),
            S(-24,  -2),   S( 32,  32),   S( 25,   1),   S( 59,   9),   S( 58,  -6),   S(  9,  -9),   S( 10,  15),   S( 32,  12),
            S(  9, -17),   S( 48,  36),   S( 20,  33),   S(  0,  13),   S( 28,  -9),   S( 16,  -3),   S( -7,  15),   S(-13,  -1),

            /* rooks: bucket 5 */
            S(-41,  33),   S( -9,  26),   S(-42,  40),   S(-19,  -3),   S(  5,   3),   S(-22,  27),   S( 17,  18),   S(-37,  37),
            S(-25,  20),   S(-22,  32),   S(-90,  66),   S(-75,  47),   S(-28,  32),   S(-12,   6),   S( 31,  13),   S(-15,  -2),
            S(-27,  51),   S(-63,  56),   S(-106,  79),  S(-78,  50),   S(-50,  33),   S(-26,  49),   S( -3,  18),   S(  0,  22),
            S(-41,  67),   S( 15,  44),   S( -7,  55),   S( -4,  37),   S(-45,  65),   S(-27,  66),   S( 23,  45),   S(  1,  37),
            S(-16,  65),   S(-20,  75),   S( 28,  55),   S( -1,  74),   S( 30,  56),   S(-12,  72),   S( 57,  52),   S( 43,  40),
            S( 27,  80),   S( 40,  59),   S( 62,  58),   S( 16,  71),   S( 54,  52),   S( 69,  42),   S( 66,  39),   S( 66,  43),
            S( 25,  50),   S( 48,  50),   S( 50,  52),   S( 40,  58),   S( 53,  47),   S( 59,  38),   S( 99,  38),   S( 51,  51),
            S( 21,  46),   S( 72,  29),   S( 33,  44),   S( 42,  31),   S( 64,  40),   S( 69,  28),   S( 48,  24),   S( 24,  44),

            /* rooks: bucket 6 */
            S(-30,  26),   S(-16,  28),   S( 10,  22),   S(-13,  18),   S(-22,  28),   S(-46,  41),   S(-17,  54),   S( -4,  36),
            S(  0,  13),   S(  6,  33),   S( 20,  13),   S(-36,  35),   S(-40,  44),   S(-53,  54),   S(-21,  41),   S( 23,  16),
            S(-63,  62),   S(-44,  48),   S(-24,  48),   S(-69,  61),   S(-12,  40),   S(-53,  71),   S(-30,  62),   S( 10,  19),
            S(-71,  83),   S( 17,  54),   S(-14,  69),   S(-37,  74),   S(-10,  55),   S(  9,  54),   S(-64,  96),   S( -8,  56),
            S(  4,  66),   S( 47,  63),   S( 56,  44),   S( 17,  54),   S( 10,  70),   S( 16,  76),   S(  6,  57),   S( -9,  65),
            S( 16,  67),   S( 72,  51),   S( 68,  47),   S(  1,  55),   S( 27,  55),   S( 30,  60),   S( 55,  71),   S( 92,  47),
            S( 75,  57),   S( 65,  49),   S(102,  33),   S(111,  19),   S(104,  31),   S( 60,  58),   S( 60,  53),   S( 45,  57),
            S( 68,  60),   S( 52,  56),   S( 54,  49),   S( 30,  48),   S( 54,  53),   S( 84,  38),   S( 88,  48),   S( 43,  45),

            /* rooks: bucket 7 */
            S(-63,  -4),   S(-27, -12),   S(-24, -20),   S( 10, -16),   S( 21, -30),   S(-22,  10),   S(-46,  19),   S( 21, -25),
            S(-73,  18),   S(-36,  16),   S(-31,  -5),   S( -4, -10),   S( -4,   9),   S( 28,   1),   S(-12,  12),   S(-26, -15),
            S(-62,  38),   S(-44,  12),   S(-17,  30),   S( 11,  -9),   S(  2,  11),   S(-10,  11),   S(-14,  -1),   S( -3,  17),
            S(-65,  49),   S( 14,  10),   S(-14,  33),   S( 51,   3),   S( 39,   4),   S( 44,   6),   S( 20,   8),   S( 14,   0),
            S( -1,  33),   S(-15,  29),   S( 53,  -1),   S( 79, -12),   S( 75,   8),   S(103,   1),   S( 27,  38),   S( 55,  -9),
            S( 18,  25),   S( -1,  24),   S( 85, -17),   S( 84, -15),   S( 64,   1),   S( 72,  22),   S( 66,  25),   S( 71, -15),
            S(  7,  31),   S( 50,  18),   S( 40,  18),   S( 82,  -6),   S( 97,  -7),   S(115,  -7),   S( 47,  20),   S( 37, -12),
            S( 22,  42),   S(-20,  39),   S( 31,   8),   S( 92, -16),   S( 22,   8),   S(  2,  11),   S( 82,   0),   S( 62, -17),

            /* rooks: bucket 8 */
            S(-42, -32),   S(-27,  15),   S(-35,  -2),   S(-38,  17),   S(-46, -65),   S(-39, -37),   S(-25, -43),   S( -6,  16),
            S( -3,   5),   S(-14, -23),   S(-10,  -1),   S(-11, -27),   S(-30, -25),   S( -6, -34),   S(-25, -90),   S(-21, -78),
            S( 11,   8),   S( -3,  12),   S(  6,   2),   S(-15,  -8),   S(-31, -34),   S(-23, -47),   S(-13,   6),   S( -8, -18),
            S(-19, -20),   S(-15,   8),   S(-33, -18),   S( 15,  12),   S(  1,   4),   S(-21, -23),   S(  3, -12),   S(-18,   1),
            S(-18, -32),   S( -1, -19),   S(-11,  22),   S( -2,  13),   S(  0, -11),   S( 12,  -1),   S(  1,   8),   S( -9, -20),
            S(  6,   8),   S(-19,   8),   S( 28,  39),   S(  8,   3),   S(-16, -24),   S( -1,  -5),   S(-11, -14),   S( -6,  20),
            S( -7,  -4),   S(-17,  -9),   S( 25,  13),   S( -4, -11),   S( 15, -24),   S(  8, -10),   S(  8,   4),   S( -3,  14),
            S(  7, -88),   S(-17, -37),   S(  6,  15),   S( -6, -12),   S( -7,   0),   S(  6, -24),   S( -3,  -7),   S( -1,  21),

            /* rooks: bucket 9 */
            S(-38, -33),   S( -8, -42),   S(-46, -13),   S(-64, -18),   S(-60, -17),   S(-10, -33),   S( -1, -72),   S(-57, -24),
            S(  3, -21),   S(  3, -47),   S( -5, -22),   S(-36, -40),   S(-26, -41),   S( -2,  -5),   S( -7, -58),   S(-29, -59),
            S( -5, -27),   S(  3, -32),   S(  8,  -8),   S(-34,  -6),   S(-30, -33),   S(  8, -16),   S(  7, -11),   S( -1, -19),
            S( -7, -15),   S(-13,  -6),   S(-15,  -6),   S(-36,   5),   S( -4, -16),   S(  1, -20),   S(  3, -25),   S(  9,  -7),
            S(  5,   8),   S(-25,   3),   S(  0,   5),   S(  6,  32),   S( 17,  46),   S(  6,  -5),   S(  7, -11),   S( -6, -34),
            S( 18,  26),   S(-13,  -4),   S( -4, -10),   S(-38,   8),   S(-13, -35),   S( 21,  -2),   S(  9,  -3),   S(-14, -26),
            S( 36,   6),   S( 63, -17),   S( 36,  -3),   S( 30, -19),   S( -6, -18),   S( 38,   2),   S( 40,  -3),   S( 43,   5),
            S( 47, -79),   S( 27, -28),   S( 26, -33),   S(  1,   5),   S(  5,   6),   S( 14,  -5),   S( 26,  -7),   S( 20,  -3),

            /* rooks: bucket 10 */
            S(-58, -111),  S(-56, -62),   S(-34, -68),   S(-26, -26),   S(-53, -36),   S(-50, -58),   S( 30, -69),   S(-10, -65),
            S(-11, -45),   S(-20, -36),   S(-27, -59),   S(-37, -32),   S( -1, -29),   S(-31, -33),   S( 12,  -7),   S( 11,  -8),
            S(-40, -39),   S(-28, -51),   S(-29, -45),   S(-14, -26),   S(-34,  -5),   S( -5, -28),   S( 33, -11),   S( -6, -24),
            S( -4, -15),   S(-18, -20),   S(-25, -31),   S( -7, -18),   S(-23, -10),   S(-15, -29),   S(  3,  15),   S(  3, -31),
            S( -9, -12),   S(  6, -24),   S(  6, -14),   S(  7, -38),   S( -5, -12),   S(  1,   2),   S( 28, -28),   S( 13,  12),
            S( 20,  -7),   S(  9,  15),   S( -1,  -8),   S(  3,  -4),   S(-10,  -9),   S(  8,  -3),   S( 23,  -6),   S(  6,   6),
            S( 67, -10),   S( 71, -30),   S( 55, -31),   S( 34, -32),   S( 33, -14),   S( 33,  -2),   S( 25, -37),   S( 36, -19),
            S( 35,  -1),   S(  7, -35),   S( 18, -32),   S( -2,  -6),   S( 35, -12),   S( 16,   1),   S( 30, -38),   S( -4, -32),

            /* rooks: bucket 11 */
            S(-48, -48),   S(-16, -33),   S(-27, -21),   S(-21, -84),   S(-42, -20),   S(-34,  12),   S(-39, -36),   S(-44,  -3),
            S(-43, -28),   S(  1, -28),   S(-45, -32),   S(-42, -24),   S(-32, -42),   S(-24, -26),   S(-22, -21),   S(-24,  -9),
            S(-16, -34),   S( 16, -40),   S(  6, -29),   S(  2, -22),   S(-23, -43),   S(-14,  17),   S(-38, -18),   S(-28, -37),
            S(-17,  18),   S(-18, -24),   S( -9,   4),   S( -8,  12),   S( -5,   2),   S(-10,  21),   S(  4,   9),   S(-17, -37),
            S( -8,   8),   S( 23, -35),   S( 18, -13),   S(  6, -35),   S( 23,   3),   S( 11, -22),   S( 18,  -6),   S(-14, -10),
            S(  2,  28),   S( 27,  15),   S(  8, -19),   S( 22,  -2),   S( 39,  -5),   S( 39, -18),   S(-36,  -1),   S( 11,   0),
            S( 57,  22),   S( 26,  12),   S( 56, -19),   S( 67,  -2),   S( 19,   3),   S( 27,  22),   S( 32,   9),   S( 54,   2),
            S( 23,  32),   S(  3,  -2),   S(  4, -31),   S(  9, -12),   S(  8,   3),   S( 22,   8),   S( 22,  36),   S( 16,  -7),

            /* rooks: bucket 12 */
            S( 15, -44),   S(-11, -32),   S(-17, -21),   S(-25, -33),   S(-12, -52),   S( -5, -24),   S(-24, -60),   S(-10, -10),
            S( 14,   6),   S(  2,   8),   S(-15,  11),   S(  0,  -6),   S(-17, -30),   S( -7,  -5),   S(  2, -13),   S( -9, -43),
            S( 10,  12),   S(-18, -47),   S(-14, -31),   S(-13, -33),   S( 11,  23),   S(  0,  -1),   S(  0, -14),   S(  3, -17),
            S(-12, -24),   S( -5, -17),   S(  4,  14),   S( 15,   7),   S(  8,  19),   S(-14, -54),   S(-15, -48),   S( -8, -38),
            S(-10, -15),   S(-22, -18),   S( 10,  -6),   S( -3, -13),   S( -9, -26),   S( -2, -23),   S( -4, -30),   S( -1, -15),
            S(-10, -12),   S( -4, -40),   S(  4, -13),   S(  1, -31),   S(-11, -46),   S(-12, -37),   S( -2, -27),   S(  8,  19),
            S(-17, -24),   S( -7, -49),   S(  6,  -6),   S( 10,  -9),   S( 10, -24),   S(  2, -40),   S( -2, -19),   S( 11,   7),
            S(-11, -53),   S(  4,  -8),   S( 12,   5),   S(-10, -23),   S( -2, -14),   S(-16, -41),   S(-18, -65),   S( 11,  15),

            /* rooks: bucket 13 */
            S( -3, -15),   S( -5, -46),   S(  0, -14),   S( -6,  35),   S( -4,   8),   S(-23, -47),   S( -6, -28),   S(-27, -55),
            S(-12, -47),   S( -5, -10),   S( -4,  18),   S( -8,  34),   S( -3,  -8),   S( -9, -19),   S(  7,  25),   S( -1,  -2),
            S(-15, -45),   S(-13, -22),   S( -6,  -9),   S( -9, -46),   S( -2,  28),   S(  5,  -8),   S( -3,   8),   S( -5, -32),
            S(-16, -41),   S(  3,  24),   S(-23, -44),   S( -8, -26),   S( 15,   6),   S(-18, -33),   S(  0, -22),   S( -7, -25),
            S(  7,  14),   S(-10, -32),   S( 15,  -1),   S(-16, -26),   S(-24, -36),   S(-10, -22),   S( -5, -53),   S(  3, -16),
            S(-17, -35),   S( -5, -11),   S(-12, -39),   S( 10, -30),   S( 16,   7),   S( 16,  24),   S( 10,  -6),   S(  6,  -1),
            S(-12, -14),   S( 14,  29),   S( -6,   7),   S(  5,  16),   S(  6, -11),   S(  9,   5),   S(  7,  -2),   S( -4, -22),
            S(-24, -112),  S(-23, -77),   S( -5, -24),   S(  0,  -9),   S(-10, -13),   S(-13, -58),   S( -7, -50),   S(  2,  24),

            /* rooks: bucket 14 */
            S(-23, -56),   S(-30, -39),   S(  1, -19),   S( -9,  -5),   S(-15,   2),   S(-17, -35),   S(  3, -18),   S(-22, -37),
            S(-21, -67),   S(-23, -56),   S(-12, -18),   S(-18, -41),   S(-10,  -9),   S(  0, -20),   S( -4, -13),   S(  5,   2),
            S( -1,  -3),   S( -6, -16),   S( -1,  -6),   S( -3, -12),   S(-18, -28),   S(-18, -37),   S(  6,  29),   S(  2, -12),
            S(  2,  18),   S( -6, -32),   S( -7, -29),   S( -4,   6),   S(  3,   0),   S(  5,   0),   S( -5, -37),   S( -5,  -2),
            S(  1, -47),   S( -8, -61),   S( -2, -44),   S(-11, -63),   S( -3, -29),   S(  4, -66),   S(  6, -25),   S(  9,   6),
            S( -8, -45),   S(  6,  -5),   S(  0, -42),   S( 13, -44),   S(  3, -47),   S(  9, -43),   S(  4, -24),   S( -1, -12),
            S( 16,  21),   S(  1, -64),   S( 12, -12),   S(-13, -61),   S(-10, -82),   S(  2, -41),   S( 12, -29),   S( 15,  23),
            S(-11, -51),   S( -1, -19),   S(-16, -45),   S( -7, -15),   S(-16, -43),   S(  6,  31),   S( -3,  -3),   S(  0,  -6),

            /* rooks: bucket 15 */
            S(-11, -42),   S(-17, -50),   S(-25, -70),   S(-18, -38),   S(  1, -12),   S(-10,  -2),   S(-11, -58),   S(-18, -34),
            S( -3,  14),   S(-18, -67),   S( -6,  -7),   S(-10, -17),   S(-13, -31),   S(  8,   9),   S( -7, -45),   S( 13,  30),
            S(-12, -33),   S( -4, -17),   S(-14, -64),   S(-11, -30),   S(  1,  -4),   S(  2, -16),   S(  6,  35),   S(-16, -48),
            S( -6, -43),   S(-11, -41),   S( -5, -28),   S(-18, -54),   S(-17, -52),   S(  4, -20),   S( -6, -49),   S(-11, -11),
            S( -2, -24),   S( -4, -31),   S(  4, -38),   S( -6, -38),   S( -1, -32),   S( 21,   5),   S(  0, -15),   S( -2,   4),
            S(  1, -24),   S(  0,  -7),   S( -5, -54),   S( -1, -62),   S( -9, -74),   S( 13,  -7),   S(  6, -31),   S(-11,  -7),
            S( 15,  29),   S(  7, -18),   S(  4, -40),   S(-12, -76),   S(  6,  -8),   S( 17,  37),   S(  9,  -4),   S( -8, -24),
            S( -9, -31),   S( -8, -32),   S(  3, -27),   S( -5, -43),   S(  1, -35),   S(  6,  -2),   S(-11, -65),   S( -8, -29),

            /* queens: bucket 0 */
            S( -8, -11),   S(-10, -74),   S( 36, -63),   S( 50, -46),   S( 25, -18),   S( 15, -10),   S( 57,  31),   S(  7,  -1),
            S(  0, -16),   S( 46, -74),   S( 37, -15),   S( 25,  11),   S( 32,  -4),   S(  6,  22),   S( -2,  44),   S( 63,   2),
            S( 22,   8),   S( 48,  -5),   S( 37,  19),   S( 24,  23),   S( 25,  -5),   S(  5,  27),   S(  8,   9),   S( 24,  42),
            S(  3,  34),   S( 22,  28),   S( 10,  42),   S( 21,  55),   S( 23,  39),   S( 25,  27),   S( 16,  24),   S( 24,  43),
            S( 30,  17),   S( 43,  13),   S( 13,  53),   S( 23,  69),   S( -7,  56),   S( -7,  -3),   S( 30,   3),   S( 51,   5),
            S( 21,  61),   S( 25,  45),   S( 29,  17),   S( 14,  11),   S( 31,  21),   S( 11,  33),   S( -3,  46),   S( 17,  -8),
            S( 52,  28),   S( 74,  -6),   S( 21,  63),   S( 71,  31),   S( 10,   0),   S(-12,  14),   S( 32,  24),   S( 30,  18),
            S( 40,  24),   S( 40,  24),   S( 65,  29),   S( 29,  34),   S( 60,  41),   S(-30, -28),   S( 64,  27),   S( 39,  14),

            /* queens: bucket 1 */
            S(-24, -28),   S(-57, -38),   S(-45, -25),   S(-14, -83),   S( 11, -54),   S(-15, -52),   S( 27, -46),   S(  0,  39),
            S(-19, -62),   S(-23, -10),   S( 16, -39),   S( -1,  21),   S(  2,  21),   S(  6, -13),   S( 28, -52),   S( -4,  45),
            S(-22,  19),   S( 10, -16),   S( 20,  20),   S( 11,  -5),   S( 14,  20),   S( -9,  26),   S( 38, -24),   S( 37,  36),
            S(  1, -28),   S( 13,   8),   S(  3,  41),   S( 30,  57),   S(  5,  40),   S( 19,  22),   S( 16,  19),   S( 17,  42),
            S( 36, -10),   S(  9,  25),   S( 13,  79),   S(  1,  49),   S(-23,  78),   S( 14,  50),   S( -7,  22),   S( 20,  22),
            S( 10,  44),   S( 35,  58),   S( 11,  62),   S(-40,  82),   S(-27,  59),   S(-13,  44),   S( 35,  50),   S(  3,  33),
            S(  6,  16),   S(-17,  95),   S(-10,  33),   S(-25,  88),   S(-20,  54),   S(  4,  44),   S(  7,  39),   S(-40,  44),
            S( -9,  28),   S( 23,  47),   S( 37,  32),   S( -7,  30),   S(-14,  21),   S( 11,  28),   S( 30,  27),   S(  4,  21),

            /* queens: bucket 2 */
            S( -5,  24),   S( 16, -41),   S( 18, -10),   S(  4, -15),   S(-14,  14),   S(-44,  -7),   S(-28, -49),   S( 25,  36),
            S( 23,  14),   S( 27,  21),   S( 24,  -9),   S( 28, -16),   S( 20, -19),   S( 19, -37),   S( 16, -26),   S( 49, -27),
            S( 18,   5),   S( 22,   2),   S( 16,  49),   S( 15,  37),   S(  9,  68),   S( 27,  32),   S( 26,  15),   S( 37,  11),
            S(  7,   8),   S(  8,  38),   S( -1,  70),   S( 16,  74),   S( -2,  86),   S( 14,  72),   S( 29,  28),   S(  4,  41),
            S( 27, -22),   S(  3,  42),   S(-24,  85),   S(-41, 129),   S(-29, 108),   S(-11,  95),   S(-20, 132),   S( -3, 130),
            S( 23,   3),   S( 17,  42),   S(-31,  97),   S(  1,  58),   S(-14, 111),   S(-15, 113),   S( 11,  72),   S(  3,  68),
            S(-27,  61),   S(-42,  95),   S(-16,  67),   S( 35,  69),   S( -1,  88),   S( 39,  60),   S(-46,  65),   S(-25,  75),
            S(-65,  80),   S(-22,  45),   S( 25,  46),   S( 41,  41),   S( 15,  50),   S( 30,  40),   S( 19,  34),   S(-21,  42),

            /* queens: bucket 3 */
            S( 87,  68),   S( 52,  88),   S( 55,  99),   S( 48,  87),   S( 74,  30),   S( 51,  -6),   S( 22,  14),   S( 50,  64),
            S( 76, 106),   S( 58, 113),   S( 54, 104),   S( 55,  77),   S( 55,  80),   S( 66,  45),   S( 61,   5),   S( 30,  52),
            S( 60,  76),   S( 57,  98),   S( 65,  88),   S( 62,  67),   S( 53,  96),   S( 63,  90),   S( 66, 104),   S( 71,  41),
            S( 45, 117),   S( 62,  93),   S( 58,  97),   S( 56, 100),   S( 55,  94),   S( 58, 130),   S( 68,  92),   S( 47, 131),
            S( 49, 125),   S( 64, 115),   S( 37, 128),   S( 33, 109),   S( 37, 141),   S( 16, 144),   S( 35, 151),   S( 56, 157),
            S( 49, 142),   S( 56, 114),   S( 48, 117),   S( 15, 139),   S( 48, 125),   S( 67, 129),   S( 53, 178),   S( 22, 196),
            S( 48, 133),   S( 49, 140),   S( 78, 103),   S( 66, 112),   S( 32, 137),   S( 63, 111),   S( 77, 127),   S(142,  81),
            S( 88,  81),   S( 74,  97),   S( 63, 121),   S( 77, 103),   S( 49, 117),   S( 88,  88),   S(129,  55),   S(113,  86),

            /* queens: bucket 4 */
            S( -6, -24),   S( -8, -10),   S(-34, -10),   S( -5, -18),   S( 15, -15),   S( 27, -24),   S(-46, -32),   S(-53, -23),
            S(-37, -35),   S(-12, -14),   S( 29, -10),   S(-49,  -9),   S(-21, -24),   S( 25,  -9),   S( -3,  -1),   S(-55, -32),
            S( 19,  22),   S( 14,  11),   S( 20,  35),   S(-15,  60),   S( 25,   2),   S(  7,  -4),   S(-15, -13),   S(-32,  -8),
            S(-20,  11),   S( 10,  34),   S( 23,  36),   S( -9,  29),   S( 19,  15),   S( -9,  -3),   S(-13, -33),   S( -3,   4),
            S(-26,   7),   S( 42,  21),   S( 22,   7),   S( 35,  54),   S( 23,  16),   S( 17, -14),   S(-10, -17),   S( -7, -29),
            S( 17,  26),   S( 43,  11),   S( 36,  52),   S( 36,  27),   S( 41,  29),   S(-19, -22),   S( -8, -23),   S( -2,   2),
            S(-14, -53),   S(-14,  11),   S( -7,   3),   S( 30,  49),   S(  4,   8),   S(  2,   6),   S(-20, -20),   S(-19, -35),
            S(-19, -35),   S(  7,  12),   S( 20,  43),   S( 10,  18),   S(-15,  -4),   S(  8,  25),   S(-11, -26),   S(-11, -19),

            /* queens: bucket 5 */
            S(-40, -17),   S(-21, -44),   S(-66, -53),   S(-45, -45),   S(-29, -25),   S( 15, -13),   S(-40,  -9),   S( -7, -25),
            S(-38, -20),   S(-62, -46),   S(-62, -23),   S(-33,   9),   S( -8, -25),   S(-28, -20),   S(-53,  -9),   S(-68, -40),
            S(-44,  -4),   S(-42,  -5),   S(-79,  14),   S(-45,  15),   S( 28,  74),   S( -9,  14),   S( -5, -17),   S( 26,  32),
            S(-90, -38),   S(-65, -35),   S(-14,  40),   S(  3,  57),   S( 32,  49),   S(-21,  25),   S( -7, -32),   S( 13,  34),
            S(-38,  -9),   S(-11,  28),   S(  6,  62),   S( -4,  61),   S( 46,  80),   S( 19,  55),   S(  1,  -5),   S(-44, -22),
            S(-40, -22),   S(  9,  56),   S(-10,  48),   S( 44,  54),   S( 49,  56),   S(  7,  29),   S(-20, -20),   S(  7, -16),
            S( -9,  -3),   S(  4,  21),   S( 24,  71),   S(  9,  47),   S( 41,  64),   S(  9,  28),   S( 25,   2),   S(-30, -23),
            S(  3,  15),   S(  7,  17),   S( 11,  22),   S(  4,  56),   S( 11,  26),   S( 11,  19),   S(-15, -27),   S(-28, -49),

            /* queens: bucket 6 */
            S( -1,   3),   S(-11, -36),   S(-74, -46),   S(-94, -82),   S(-113, -29),  S(-69, -58),   S(-17, -43),   S(-11,   7),
            S(-66,  -4),   S(-35, -10),   S(-58,  -5),   S(-33,  11),   S(-73,  17),   S(-99,   2),   S(-102, -38),  S( 15,  19),
            S(-31,   6),   S( -5, -28),   S(-36,  24),   S(-75,  62),   S(-42,  52),   S(-41,  -7),   S(-71, -18),   S(  4, -13),
            S(-23,  38),   S(-30,   7),   S(-34,  63),   S(-22,  74),   S(  4,  54),   S( 30,  66),   S(-14,  46),   S( 23,  -6),
            S(-52,  35),   S(  0,  54),   S(-31,  54),   S( 23,  47),   S( 14,  73),   S( 88,  54),   S( 47,  41),   S(  5,  17),
            S(-23,  38),   S(  4,  21),   S( 45,  29),   S( 49,  54),   S( 27,  49),   S( 44,  88),   S(-41,  -6),   S(-22, -10),
            S( -1,  36),   S( -4,  16),   S( -3,  69),   S( -1,  47),   S( 37,  53),   S( 40,  53),   S(-20,  21),   S(-31,   3),
            S( 14,  28),   S( 27,  47),   S( 31,  39),   S( 20,  61),   S( 68,  59),   S( 33,  22),   S(-11,  10),   S(  7,  -3),

            /* queens: bucket 7 */
            S(-34, -11),   S(-21,  27),   S(-55,  32),   S(-18, -17),   S(-50,   1),   S(-61, -10),   S( -9, -22),   S(-30, -29),
            S(-59,  -8),   S(-40, -16),   S(-35,  36),   S(-13,  30),   S( -3,  30),   S(-54,  24),   S(-45,  49),   S(-27, -49),
            S(-61, -10),   S(-86,  50),   S( -5,  17),   S(-11,  36),   S( 43,  15),   S(-12,  50),   S( -9,   9),   S(-42, -16),
            S(-55,  -9),   S( 13,   1),   S(-10,  37),   S(  7,  33),   S( 39,  26),   S( 31,  25),   S(  7,  56),   S( -4,  30),
            S(-39,  33),   S(-69,  35),   S( 34,  28),   S( 45,  14),   S( 71,   7),   S(104,  -2),   S( 23,  39),   S( 66, -15),
            S(  6,  32),   S(-41,  45),   S( -9,   3),   S( 21,   2),   S( 34,  42),   S( 90,  34),   S( 64,  15),   S( 57,  18),
            S( -7, -21),   S( -6,  13),   S( -1,  24),   S( 29,  36),   S( 21,  32),   S( 73,  38),   S( 48,  10),   S( 25,  -7),
            S( 28,  34),   S( 18,   0),   S( 36,  32),   S( 44,  20),   S( 35,  21),   S( 30,  28),   S( 36,  22),   S( 39,  34),

            /* queens: bucket 8 */
            S(-16, -25),   S(  6,  -2),   S(-19, -12),   S( -7, -18),   S( -9, -11),   S( -4, -28),   S(-28, -43),   S( -2,   3),
            S(  1,   3),   S( -8,  -9),   S( -1,   8),   S(-15,   7),   S(  9,   7),   S( -9,  -9),   S(-29, -53),   S( -5, -10),
            S( -2,  -3),   S( -4,   0),   S(-21, -34),   S( -7, -18),   S( -8, -13),   S(-18, -28),   S(-14, -22),   S( -9, -21),
            S(  3,  11),   S( 15,  25),   S( -7,   0),   S(  6,  40),   S(-26, -32),   S(-19, -26),   S(  1,   4),   S(-16, -40),
            S(  6,  12),   S(  5,  44),   S( 16,  23),   S(  1,  15),   S( 13,  16),   S(  3,  13),   S( -8,  -9),   S(-18, -24),
            S(  7,  15),   S( 12,  20),   S( -4,  21),   S( 16,  52),   S(-12,  -9),   S( -6,  -9),   S(  9,  25),   S( -3,   8),
            S( -8, -17),   S(-34, -32),   S( 24,  42),   S( 12,  17),   S(-10, -14),   S(  9,  16),   S( -5, -10),   S( -9, -25),
            S(-17, -26),   S( 12,  10),   S(-18, -31),   S( -1, -22),   S( -1, -25),   S( -8, -14),   S(-11, -33),   S( -8, -14),

            /* queens: bucket 9 */
            S(  0,  -5),   S(-17, -50),   S( -5,  -6),   S(-22, -30),   S(-21, -37),   S(-29, -33),   S(-20, -36),   S(-24, -53),
            S( -1,  -3),   S(-21, -33),   S(-28, -19),   S( -5, -23),   S(-32, -35),   S(-13, -24),   S(  3, -13),   S(-12, -27),
            S(  7,   6),   S(  1,   9),   S(-19,   3),   S(-15, -26),   S(-12,  -8),   S(-19,  -7),   S( -2,   1),   S( -9, -18),
            S(-31, -37),   S(-29, -37),   S(  4,  31),   S( -9,  -1),   S( 11,  28),   S( -2,  -6),   S(-19, -26),   S(-16, -38),
            S(-12, -19),   S( -1,  25),   S(  6,  34),   S( 12,  48),   S( 25,  48),   S( -3,  -6),   S(  1,   3),   S(-15, -24),
            S(-26, -42),   S(-32, -27),   S(-20, -10),   S(  3,  31),   S(-17,  -3),   S( -2,  -4),   S(-25, -32),   S(  1,   3),
            S(-20, -28),   S( 11,  15),   S(-16,  12),   S( -4,  10),   S( 11,  11),   S(-12, -31),   S( -3,   2),   S(-17, -40),
            S( -6, -17),   S(-12, -31),   S( -8,  -7),   S(  7,   3),   S(  5,  -3),   S(-11, -19),   S(  1,  -1),   S( -5, -15),

            /* queens: bucket 10 */
            S(-12, -18),   S(  3,   1),   S(-13, -28),   S(-13, -11),   S(-21, -28),   S( -7,  -4),   S(  4,  11),   S( -2, -14),
            S(-13, -20),   S(-22, -38),   S(-19, -29),   S(-18,  -7),   S(-11, -11),   S( -1,  -3),   S(-11, -25),   S(-20, -32),
            S( -6,  -9),   S( -2,  -6),   S( -7,   4),   S( 19,  41),   S(-24,  -4),   S( -9,   8),   S(-12, -20),   S( -3,   1),
            S( 14,  15),   S( -3,  -9),   S( -6,   9),   S(-23, -13),   S(  9,  34),   S(-21,   4),   S( -8, -13),   S(-22, -29),
            S(-19, -35),   S(-13, -22),   S(-43, -61),   S( 18,  34),   S(-11,   2),   S(  3,  12),   S( 18,  14),   S(  9,  16),
            S(-12, -12),   S(-22, -23),   S(-32, -20),   S(-16,  13),   S( 20,  33),   S(-13,  -9),   S(  9,   9),   S( -1,   6),
            S( -3,  -2),   S(-18, -30),   S(  1,  -5),   S(-20, -12),   S( -9,  -2),   S( -3,   1),   S( -8, -13),   S( -4, -14),
            S( -3,  -1),   S(-23, -49),   S( -6,  -8),   S(-11,  -1),   S( 16,  26),   S(  6,  13),   S( 12,  23),   S( -4,  -1),

            /* queens: bucket 11 */
            S( -3, -12),   S(-20, -37),   S(-26, -24),   S(-28, -42),   S(-19, -24),   S(-10, -23),   S(-19, -16),   S(-27, -47),
            S(-28, -32),   S(-13, -30),   S(-35, -37),   S(  7,   0),   S(-13, -13),   S( -2,   7),   S(  9,  16),   S( -9,  -5),
            S(-31, -34),   S(-29, -54),   S(-13, -18),   S(-22, -41),   S( -2,   3),   S( -5, -16),   S(  1,  24),   S(-21, -10),
            S(-14, -30),   S(-34, -45),   S(-16, -19),   S( 19,  37),   S( -2, -18),   S( -8,   9),   S( 23,  34),   S(-10,  -9),
            S(-17, -19),   S( -1, -11),   S( -7,   1),   S( 20,  30),   S( 16,   7),   S( 28,  58),   S( 19,  51),   S(-15,  -7),
            S(-25, -48),   S( 14,  29),   S( -6,  -4),   S( 18,  41),   S( 22,  13),   S( 48,  63),   S( 25,  28),   S(  0,   7),
            S(-18, -28),   S(-24, -35),   S( 19,  36),   S(-21, -25),   S( -9,  -4),   S( 18,  19),   S( 43,  53),   S( -6, -24),
            S(  2,   3),   S(-22, -30),   S(-22, -46),   S(  7,   2),   S(  3,   7),   S( -9, -30),   S( 13,   5),   S( -4, -42),

            /* queens: bucket 12 */
            S(  3,  -3),   S(  6,   4),   S( -9, -14),   S( -3,   4),   S( -8, -13),   S(  2,   5),   S(  0,   1),   S(-12, -24),
            S( -2,  -5),   S( -5,  -7),   S(-13, -24),   S(-12, -24),   S( -9, -24),   S( -9,  -7),   S( -3, -13),   S( -7, -16),
            S(  5,   5),   S( -6, -18),   S(  6,  -1),   S( -9, -26),   S( -6, -17),   S( -5, -10),   S( -6, -13),   S(-19, -38),
            S( -8, -16),   S( -1,   3),   S( 19,  36),   S(  3,  13),   S( 22,  36),   S(  7,   4),   S(  1,   0),   S( -4, -20),
            S(  3,   6),   S( 10,   7),   S( 31,  43),   S(  9,  25),   S(  1,   5),   S( -8, -13),   S(-16, -28),   S( -7, -15),
            S(  8,  25),   S(-10, -12),   S( 26,  47),   S( -7, -14),   S( -8, -10),   S(  2,   2),   S(  0,   4),   S(-10, -24),
            S(  4,   8),   S(  2,  12),   S(  9,  25),   S( 17,  26),   S(  2,   3),   S( 13,  20),   S(  4,   7),   S( -3,  -5),
            S(-10, -24),   S( -8, -20),   S(-25, -29),   S(-11, -14),   S(-14, -19),   S( -3,  -4),   S( -2,  -7),   S( -5,  -3),

            /* queens: bucket 13 */
            S( -4, -15),   S(-10, -23),   S(  2,  -1),   S( -6, -18),   S( -3, -11),   S(  1,  -1),   S( -9, -23),   S(-10, -21),
            S( -1,  -3),   S( -7, -19),   S(  2,   3),   S(-11, -28),   S(-17, -35),   S(  5,  10),   S( -6, -10),   S(-14, -25),
            S( -8, -16),   S(  5,   2),   S( -2,   3),   S( -9, -20),   S( -1,   5),   S( -5, -14),   S(-15, -32),   S(-10, -17),
            S(-10, -14),   S(-10, -25),   S(  8,   6),   S( 18,  23),   S( 23,  40),   S(-14, -32),   S( -6, -17),   S( -8, -20),
            S( -5,  -9),   S( 17,  44),   S( 15,  33),   S( 10,  25),   S( 22,  47),   S( -2,  -4),   S( -6, -21),   S(-12, -32),
            S( -5, -20),   S( 12,  37),   S( 34,  70),   S( 16,  35),   S(  4,  25),   S(  0,  -4),   S(  3,   5),   S( -4, -15),
            S( -4,  -5),   S(  7,  12),   S( -5,  -3),   S(  2,  17),   S( 14,  23),   S( -5,  -1),   S(  0,  -7),   S(  3,   8),
            S(-28, -61),   S(  4,   7),   S(-16, -27),   S(-20, -29),   S( 19,  26),   S(  3,   5),   S( -7, -11),   S( -9, -19),

            /* queens: bucket 14 */
            S( -3,  -3),   S( -4, -16),   S( -6, -17),   S(-14, -18),   S( -1,   0),   S( -4, -13),   S( -8, -23),   S( -7, -21),
            S( -8, -21),   S(  5,  12),   S( -8, -23),   S(  5,  -4),   S(-17, -30),   S( -9, -23),   S( -6, -17),   S( -2, -10),
            S(  5,   8),   S(-15, -30),   S(-14, -17),   S(  1,   6),   S(-12, -23),   S(  4,   2),   S(  2,   3),   S(-12, -16),
            S( -4,  -6),   S(  8,   9),   S(-13, -24),   S( 17,  36),   S(  1,  11),   S( -6,   1),   S( 10,  22),   S( -3, -11),
            S( -7, -11),   S(  4,  11),   S(-13,  -7),   S( 21,  28),   S(  5,  22),   S( 10,  13),   S(  5,   8),   S( -9, -25),
            S( -1,  -3),   S( -1,  -4),   S( 20,  38),   S(  7,  16),   S( 10,  20),   S( 14,  28),   S( -1,  -9),   S( -4, -11),
            S(  5,   8),   S(  2,  -1),   S(  5,   8),   S( 14,  20),   S( 10,  22),   S( 12,  27),   S( 10,  13),   S(  7,  11),
            S( -3,  -8),   S(  5,  10),   S( -9, -21),   S(  6,   3),   S(  1,  -1),   S(  4,   6),   S(-14, -27),   S(-16, -28),

            /* queens: bucket 15 */
            S(  4,   4),   S( -7, -22),   S(  2,   2),   S( -6, -16),   S( -3,  -6),   S( -1,  -8),   S( -7, -16),   S( -3,  -9),
            S( -1,  -3),   S( -4,  -8),   S(-17, -32),   S( -3,  -6),   S(  3,   8),   S(  0,   1),   S( 11,  20),   S(  2,   3),
            S(  2,   0),   S( -1,  -1),   S(  1,   3),   S( -4, -12),   S( -4, -15),   S( 13,  20),   S( -5, -12),   S(  2,   1),
            S( -1,   1),   S( -2, -13),   S(-12, -22),   S(  8,  12),   S(  0,  -4),   S(-13, -23),   S(  1,  -2),   S(  1,   5),
            S( -1,  -2),   S(  2,  -3),   S( -8, -22),   S(  3,   3),   S( 24,  34),   S(  5,   7),   S(-11, -29),   S( -6, -15),
            S( -4, -12),   S( -5, -11),   S( -5, -13),   S( -2,  -8),   S(  5,  12),   S( 27,  50),   S( 14,  27),   S(  2,   2),
            S( -5,  -7),   S(-10, -28),   S(  4,   4),   S( 22,  39),   S( 11,  17),   S( 21,  36),   S( 16,  26),   S(  7,  12),
            S( -9, -21),   S(  1,  -7),   S( -7, -15),   S( -4, -10),   S(  9,  13),   S( -5, -28),   S( -7, -13),   S( -6, -10),

            /* kings: bucket 0 */
            S( 18,  41),   S(  9,  69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 23,  51),   S( 93,  69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-19,  23),   S(-76,  39),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 22,  38),   S( -1,  46),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-49,  44),   S(-49,  33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 25,  48),   S( 29,  41),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19,  57),   S( -2,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 62,  81),   S( 13,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -41),   S( 35, -30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-69, -12),   S( 17,  10),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0, -44),   S(-43, -24),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 23, -17),   S(-22, -15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-15, -11),   S(-45, -12),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  8,  -5),   S(-13,  -1),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 29,  17),   S(-17, -17),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 25,  39),   S(-39,  19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87, -52),   S(-21,  -8),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-53, -38),   S( 23, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-17, -45),   S(-31, -52),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 59, -54),   S( 60, -53),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -8, -55),   S(-38, -42),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 87, -65),   S( 82, -56),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 11, -26),   S(-112, -44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 96, -44),   S( 16, -65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -46),   S( 32, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39, -98),   S( -4, -29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 36, -31),   S( 51, -39),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 16, -63),   S( 15, -61),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 51, -39),   S( 24, -37),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19, -59),   S( 14, -57),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 64, -46),   S(-47, -61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  3, -68),   S(-23, -119),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30, -11),   S(-35,  10),   S(-10,  -2),   S(-15,   3),   S(  5,   1),   S( 25,  13),   S( 27,  -7),   S( 40,  -5),
            S(-13, -26),   S(-24, -15),   S(-20,  -4),   S(-11,  -2),   S( 13,  -7),   S( -2,   4),   S( 28, -11),   S( 11,  18),
            S( 17, -25),   S( 11, -31),   S( 24, -28),   S(  8, -20),   S( 15,  -1),   S( 10,  27),   S(  4,  26),   S( 51,   7),
            S( 21, -18),   S( 44,  -9),   S( 58, -27),   S( 35,  10),   S( 23,  44),   S(-10,  82),   S( 30,  62),   S( 78,  47),
            S( 96, -56),   S(105,  -8),   S( 82,  -7),   S( 43,  21),   S( 65, 112),   S( 26,  92),   S( -9, 156),   S( 84,  93),
            S(-142, -109), S(-150, -108), S( 42, -173),  S( 64,  20),   S( 80, 142),   S( 59, 156),   S(151,  88),   S( 82, 158),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  14),   S(-58,  16),   S(-33,  -1),   S(-54,  46),   S(-33,   4),   S( -2,   5),   S( -2,  -1),   S(  4,  18),
            S(-48,  -2),   S(-44,   0),   S(-43,  -4),   S(-33,   4),   S( -8,   1),   S(-20,  -4),   S( -6,  -7),   S(-19,   7),
            S(-33,  18),   S(-13,   6),   S(-28,  -1),   S(  5, -12),   S( -3,  15),   S(-14,  15),   S(-19,  17),   S(  6,  16),
            S(-19,  31),   S( 22,  10),   S( -7,  18),   S( 13,  23),   S( 15,  19),   S(-26,  34),   S(  6,  23),   S( 46,  39),
            S( 18,  19),   S( 77, -16),   S( 93, -31),   S(104, -29),   S( 52,   3),   S( 11,  32),   S(-42,  64),   S( 72,  56),
            S(124, -43),   S(-14, -31),   S( 63, -133),  S( 43, -109),  S(-35, -61),   S(-52,  75),   S( 53, 151),   S(139, 157),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58,  36),   S(-48,  18),   S(-38,   7),   S(-25,  17),   S(-53,  35),   S(-37,  16),   S(-19,  -8),   S(-25,  22),
            S(-49,  15),   S(-42,  14),   S(-46,   6),   S(-48,  26),   S(-41,  18),   S(-51,   8),   S(-20, -16),   S(-51,   6),
            S(-26,  38),   S(-34,  35),   S(-20,  16),   S(-20,  19),   S(-30,  30),   S(-22,   6),   S(-32,   9),   S(-21,   8),
            S(-13,  71),   S(-32,  64),   S( -9,  34),   S( -1,  37),   S( -8,  33),   S(-27,  17),   S(  8,  13),   S( 32,   5),
            S(-23, 121),   S(-55, 106),   S(-41,  40),   S( 35, -28),   S(103, -22),   S( 85,   2),   S( 83, -31),   S( 44,   2),
            S( -1, 214),   S( 80, 110),   S( 24,  62),   S(  7, -88),   S(-30, -135),  S( -5, -157),  S( 42, -71),   S( 95, -38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8,  19),   S(-14,  11),   S( -8,  13),   S(-15,  34),   S(-26,  53),   S(  8,  33),   S( -4,   7),   S(-10,  -4),
            S(  4,   7),   S(  3,  10),   S(-13,  11),   S(-18,  18),   S( -3,  24),   S(  2,   7),   S( -2,  -6),   S(-35,  -1),
            S( 22,  34),   S( -2,  49),   S(  2,  24),   S( -1,  10),   S( 14,   3),   S( 18,  -3),   S( -7,  -6),   S( -6,  -6),
            S( 18,  81),   S( -5,  99),   S( 11,  65),   S( 13,  38),   S( 30,   7),   S( 34, -21),   S( 22,   6),   S( 46, -20),
            S(  4, 149),   S(-30, 170),   S(-38, 163),   S( -4, 112),   S( 33,  64),   S( 81,  -1),   S( 89, -23),   S( 85, -20),
            S(101, 128),   S( 42, 233),   S(  8, 254),   S(  3, 176),   S( -1,  71),   S(  7, -131),  S(-80, -178),  S(-164, -161),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 63, -10),   S( 16,  -6),   S(-17,  -3),   S(-16, -16),   S( -3,  -5),   S( -6, -13),   S(-50,   4),   S(-50,  31),
            S( 51, -25),   S( 17,  -2),   S(  6, -16),   S(-36,   0),   S(-23, -19),   S(-13, -22),   S(-61, -15),   S(-33, -11),
            S( 95, -29),   S(115, -41),   S( 46, -28),   S( -3, -12),   S(-49,  10),   S(-37,  22),   S(-59,  15),   S(-36,  27),
            S(-27, -92),   S( 38, -123),  S( 41, -31),   S(-21,   8),   S(-34,  20),   S(-48,  56),   S(-15,  45),   S( -4,  40),
            S( 87, -118),  S(-46, -125),  S( 21, -92),   S( 45,  -3),   S( 69,  86),   S( 34,  86),   S( 39,  49),   S( -3,  91),
            S(  4, -38),   S( -3, -53),   S( 12, -94),   S( 31,  36),   S( 62,  97),   S( 80, 147),   S( 43,  96),   S( 28, 113),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-66,  32),   S(-32,  25),   S(  3,   7),   S( 79,   0),   S( 39,  -4),   S( 19,  -9),   S(-34,   3),   S(-62,  40),
            S(-48,   9),   S(-17,  15),   S( 29, -20),   S( 19,  -4),   S(-10,   4),   S( -1, -17),   S(-64,  -3),   S(-74,  17),
            S(-12,  11),   S( 11,  24),   S( 67,   2),   S( 23,  26),   S(-25,  37),   S( -2,  -2),   S(-33,  15),   S(-50,  36),
            S( 22,  10),   S( 48, -16),   S(  5, -44),   S(-18,  -7),   S( 18, -19),   S( 24,  -5),   S( 26,   9),   S( 16,  30),
            S( 69,  -3),   S( 43, -56),   S( 25, -91),   S(-16, -60),   S( 95, -59),   S( 61,   9),   S( 48,  16),   S(-33,  87),
            S( 64,   8),   S( 45, -32),   S(  4, -82),   S( 24, -89),   S( -6, -56),   S( 65,  10),   S( 42, 106),   S( 76,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-79,  22),   S(-42,  -3),   S(-19,  -5),   S( -3,  14),   S( 29,  -2),   S( 17,   5),   S( 12,  -1),   S( -1,  20),
            S(-76,   4),   S(-33, -13),   S(-19, -18),   S( 36,  -6),   S( -9,  11),   S(  8, -10),   S(  2,  -7),   S( -7,  -2),
            S(-33,  19),   S(-40,  14),   S(  1,   1),   S( -9,  24),   S( 16,  29),   S( 43,   1),   S( 37,   0),   S( 24,   0),
            S(-35,  53),   S( 10,  21),   S(  8,   9),   S( 49,  -6),   S(-14, -35),   S( 27, -45),   S( 33, -25),   S( 96, -32),
            S( 15,  76),   S( 33,  22),   S( 28,  -1),   S( 23, -41),   S(-31, -62),   S(-21, -56),   S( 73, -77),   S(114, -29),
            S(108,  47),   S(110,  56),   S( 49,  18),   S( 57, -56),   S( -4, -80),   S(  3, -110),  S(-31, -51),   S( 81,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -10),   S(-47, -22),   S(-12, -25),   S(-67,  30),   S( 12,   1),   S( 64, -14),   S( 46, -25),   S( 67, -12),
            S(-44, -13),   S(-48, -15),   S(-36, -26),   S(-21,  -4),   S( -1,  -8),   S( 50, -34),   S( 36, -24),   S( 54, -25),
            S(-34,  15),   S(-57,  17),   S(-36,   3),   S(-19, -13),   S(  3,  -1),   S( 35, -22),   S( 80, -27),   S( 70, -20),
            S( -4,  33),   S(-65,  56),   S(-33,  45),   S(  4,  20),   S(  8,  -7),   S( 67, -66),   S( -4, -59),   S( 42, -102),
            S( 48,  40),   S(-18, 101),   S( 20,  79),   S( -1,  79),   S(  6,  10),   S(-22, -50),   S(-41, -124),  S(-21, -89),
            S(134,  69),   S(121,  91),   S(108,  88),   S( 69,  81),   S( 96, -13),   S( 11, -85),   S(-23, -93),   S(  1, -162),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  -5),   S( -2, -18),   S( 43,  -7),   S( -7, -15),   S( -1, -44),   S(  2, -31),   S( 22, -65),   S( 24, -31),
            S(-20, -56),   S(-20,  12),   S(-46, -54),   S(-43, -23),   S(-57, -46),   S( 14, -44),   S( -2, -43),   S(-19, -41),
            S(-52, -64),   S( 24, -59),   S( -4, -55),   S(-33, -59),   S(-29,  -7),   S(-20, -24),   S(-36, -11),   S(-27,  -6),
            S(  5,  -7),   S(-28, -39),   S( 19, -23),   S( 34,   4),   S( -1,  17),   S(  0,  33),   S( -3,  32),   S(  7,  25),
            S(  9,  13),   S(  5, -19),   S( 14,  19),   S( 54,  84),   S( 70, 114),   S( 52,  96),   S(  0,  73),   S(-12,  86),
            S( 26,  69),   S( 17,  46),   S( 33,  86),   S( 44,  91),   S( 33,  69),   S( 48, 170),   S( 44, 119),   S(-19,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  -5),   S( 65,  -9),   S( 31,  -7),   S( -5,  13),   S( 19, -12),   S( 71, -38),   S( 72, -59),   S(  0, -33),
            S( -9, -49),   S(-32, -40),   S(-23, -38),   S(-11, -43),   S(-23, -35),   S(-20, -39),   S( 10, -52),   S( 18, -48),
            S(-105, -15),  S( -4, -51),   S(-21, -76),   S(-45, -47),   S(  0, -29),   S( 15, -49),   S(-37, -35),   S(-27, -11),
            S(-39,   5),   S(-35, -52),   S(  1, -49),   S(  4, -17),   S( 17, -33),   S(-25, -12),   S(-35,   5),   S( 11,   6),
            S(  2,  23),   S(  2, -19),   S( 21, -19),   S( 30,  25),   S(  3,  54),   S(  8,  43),   S( -8,  49),   S( 21,  49),
            S(-23,  56),   S( 14,  17),   S(  5,  22),   S( 40,  52),   S( 35,  81),   S( 11,  51),   S( 23,  73),   S(  8,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -52),   S(-44, -51),   S( 14, -31),   S( -2, -21),   S( 37, -36),   S(164, -29),   S( 77, -22),   S( 56, -30),
            S(-43, -56),   S(-70, -54),   S( 28, -63),   S( 23, -45),   S(  1, -31),   S( 43, -44),   S( 17, -44),   S( 14, -38),
            S(-51, -32),   S(-42, -45),   S(-14, -54),   S( 28, -53),   S(-14, -41),   S(-12, -62),   S( 22, -77),   S( 44, -54),
            S(-27, -15),   S( 32, -24),   S(  7, -35),   S(-21, -46),   S(-48, -32),   S(-63, -47),   S( -7, -56),   S(-19, -28),
            S( 11,  13),   S( 19,  22),   S( 30,  -5),   S(-11, -35),   S( 53,  16),   S( 41,  22),   S(-16, -20),   S(  3,   4),
            S(-22,  10),   S(-17,  17),   S( 26,  39),   S(  6,  28),   S( 31,  69),   S( -6, -11),   S(-23, -22),   S( 11,  19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -47),   S(-22, -31),   S(-43, -23),   S( 20,   2),   S(  0, -17),   S(118,  -9),   S(104, -34),   S( 76, -11),
            S(-58, -54),   S(-67, -60),   S(-40, -57),   S(-14, -50),   S( -3, -21),   S( 15, -44),   S(  5, -33),   S( 25, -58),
            S(-71,  -3),   S(-53, -16),   S( -6, -24),   S( 25, -34),   S(-43, -42),   S( -1, -37),   S(-16, -84),   S( -3, -60),
            S( 25,  12),   S( -5,  13),   S( 59,  17),   S( 47,  -2),   S(-17,  -3),   S( 13, -37),   S(-18, -48),   S( -8, -41),
            S( 24,  20),   S(  3,  69),   S( 23,  79),   S(  3,  58),   S( 36,  43),   S(  9,  -4),   S( 14,   7),   S(  8, -21),
            S( 17,  63),   S( 37,  57),   S( 45,  83),   S( 31,  70),   S(  8,  56),   S( 38,  80),   S( 26,  40),   S( 21,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -73),   S(-13,   7),   S(-18, -15),   S(  5,  21),   S( -7, -31),   S(-22, -67),   S( 12, -12),   S(  6, -18),
            S(-12, -35),   S(-16,   0),   S(-48, -63),   S(-37, -60),   S(-50, -41),   S(-11, -63),   S(  7, -58),   S(-21, -63),
            S(-36,  -6),   S(  3, -55),   S(-17, -85),   S(-39, -72),   S(-14, -23),   S(-21, -14),   S(-48,  -3),   S(-49, -11),
            S( -9,  -1),   S(  4,   6),   S(-13, -30),   S( 14,  11),   S( 14,  61),   S(  3,  72),   S(-10,  53),   S(-21,  42),
            S( 13,  30),   S(  0,   3),   S(  5,  22),   S(  6,  46),   S( 31,  72),   S( 18,  82),   S(  7, 119),   S( 20, 107),
            S( 11,  12),   S(  6,  21),   S(  7,  50),   S( 15,  63),   S( 18,  85),   S( 29,  89),   S(-20,  33),   S(-19,  71),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39, -61),   S(-11, -43),   S(  1,   3),   S( -2,  -6),   S(  8,  -3),   S(-18, -39),   S( 10, -63),   S(-12, -52),
            S(-73, -56),   S(-29, -82),   S(-53, -63),   S( 23, -13),   S(-29, -37),   S(-26, -41),   S(-41, -47),   S( -6, -61),
            S(-29, -35),   S(-24, -65),   S( -3, -70),   S(-21, -48),   S(  3, -55),   S( 13, -56),   S(-28, -35),   S(-30, -16),
            S(-32,   6),   S(-22, -17),   S( -6, -41),   S(  0, -13),   S(-12,  32),   S(-30,  33),   S(-17,   2),   S(-22,   3),
            S( -8,  23),   S(  6,  29),   S( -3, -10),   S( 32,  63),   S( 39, 109),   S( 26,  96),   S( 12, 113),   S(-12,  99),
            S( 18, 100),   S( 27,  62),   S( -3,   2),   S( 17,  65),   S( 13,  70),   S( 16,  90),   S( -5,  61),   S( -1,  83),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -66),   S( 23, -54),   S( 13, -35),   S(  2,  11),   S( -4, -12),   S(  0,   0),   S(  4, -61),   S( 15, -12),
            S(-34, -66),   S(-77, -71),   S(-22, -77),   S(-27, -101),  S(-12, -44),   S(  7, -46),   S(-24, -46),   S(-33, -39),
            S(-41, -63),   S( -1, -48),   S(-38, -48),   S( -4, -69),   S(-17, -38),   S( -2, -53),   S(-10, -59),   S(-49, -17),
            S(-19,  -6),   S(-17, -16),   S(  7,  41),   S(-16, -20),   S( 15, -31),   S( -9, -18),   S(  7,  11),   S( -5,  46),
            S(-17, -42),   S( 16,  56),   S( 10,  83),   S(  2,  14),   S( 27,  67),   S(  8,  18),   S( 15,  88),   S( 24,  93),
            S( -4,  72),   S( 21,  60),   S( 23,  95),   S( 24,  83),   S( 13,  41),   S(  0,  51),   S( 26, 110),   S(  3,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,  -8),   S( -3, -47),   S(-26, -43),   S(-17, -43),   S( -1, -11),   S(-31, -31),   S(-23, -32),   S( -2, -53),
            S(-45, -48),   S(-19, -84),   S(-24, -97),   S(-17, -58),   S(-19, -31),   S(-29, -24),   S(-48, -12),   S(-25, -54),
            S(-27,  -8),   S(-51, -40),   S(-44, -47),   S( 14, -54),   S(-16, -30),   S(-18, -29),   S( -8, -39),   S(-36,  14),
            S(-20,  23),   S(-43, -13),   S(  2,  20),   S(-10,  21),   S( 12,  25),   S(  6,  -2),   S(  9,  15),   S(  0,  16),
            S( -4,  91),   S(  4,  61),   S(  1,  67),   S( 12,  73),   S( 37, 122),   S( 11,  47),   S( 31,  70),   S(  9,  27),
            S(-25,  31),   S( -3,  45),   S( 14,  95),   S( 22,  64),   S( 19,  85),   S( 14,  60),   S(  2,  -5),   S( 22,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-62, -26),   S(-20, -30),   S(-21,  18),   S(  9,  16),   S(-34, -15),   S(-16,   7),   S(-12,   1),   S(-89, -57),
            S( 33, -28),   S(-32,   4),   S(-18, -38),   S(  0,  -7),   S(-17,  18),   S(-12, -27),   S(-27, -30),   S(-28, -49),
            S(-17, -43),   S( 23, -26),   S( -5,  25),   S( 43,  33),   S(  5,  10),   S( 11,   8),   S(-38,  28),   S(-31, -33),
            S(  0,  21),   S( 34,  40),   S( 22,  46),   S( 44,  32),   S( 19,  42),   S( 19,  22),   S( 49, -16),   S(  1,  -5),
            S( 30,  76),   S(-14,  67),   S( 56,  88),   S( 43,  58),   S( 81,  41),   S( 19,  37),   S( 29,   7),   S( -9,  40),
            S(105, -29),   S(-23,  62),   S(136,  63),   S( 55,  30),   S( 61,  56),   S(-18,  81),   S( 32,  -6),   S(-18,  20),
            S( 44,  -9),   S( 11, -27),   S( 49,  11),   S( 77,  92),   S( 37,  24),   S(-16,  62),   S(  0,  15),   S(-35,  15),
            S(-110, -133), S( -4,  -4),   S(-16, -14),   S( 25,  66),   S( -2,  62),   S( -1,  25),   S(-42,  -8),   S( -1,  12),

            /* knights: bucket 1 */
            S( 45,   0),   S(-68,  20),   S(-21,  23),   S(-61,  47),   S(-15,  24),   S(-22, -17),   S(-41,  18),   S(-24,  -1),
            S(-57,  19),   S(-35,  59),   S(-28,  40),   S(-13,  32),   S(-21,  40),   S(-11,  52),   S(  0, -10),   S(-27, -51),
            S(-39,  35),   S( -8,  31),   S(-22,  37),   S(-12,  70),   S( -9,  58),   S(-12,  29),   S(-42,  37),   S(-22,  35),
            S(-20,  81),   S( 31,  46),   S(-11,  60),   S(-16,  73),   S( -1,  58),   S(-20,  85),   S(  9,  28),   S(-30,  78),
            S( 45,  -3),   S(  7,  22),   S(  9,  84),   S( 18,  55),   S( 32,  52),   S(  3,  60),   S( -9,  46),   S(-21,  77),
            S(  3,  42),   S( 53,   4),   S( 79,  47),   S(101,  28),   S( 39,  66),   S(-48,  83),   S( 46,  28),   S(  9,  21),
            S( 24, -21),   S( 22,  -3),   S( 29, -10),   S( 24,  43),   S( 15,  19),   S(-25,  32),   S(-21, 103),   S(-27,  39),
            S(-201, -28),  S( 20, -37),   S(-26, -65),   S( -6,   8),   S( 11,  24),   S( 24,  41),   S( 36,  39),   S(-65,  50),

            /* knights: bucket 2 */
            S(-60,   8),   S(-45,  40),   S(-29,  11),   S(-29,  35),   S(-27,  37),   S(-64,  30),   S(-29,  19),   S( -4, -18),
            S(-40,  51),   S(  1,  49),   S(-38,  45),   S(-15,  38),   S(-34,  46),   S(-20,  29),   S( 10,  31),   S(-47,  14),
            S(-40,  68),   S(-27,  42),   S(-25,  42),   S(-32,  84),   S(-21,  71),   S(-24,  34),   S(-32,  36),   S( -8,  11),
            S(-20,  72),   S(-10,  53),   S(-17,  93),   S(-29, 110),   S(-33,  88),   S( -8,  70),   S( -8,  64),   S(-12,  62),
            S(-18,  76),   S( -5,  77),   S( -3,  88),   S(  9,  72),   S(-33, 100),   S( 18,  79),   S(-16,  79),   S( 10,  15),
            S(-18,  66),   S(-23,  75),   S(-32, 101),   S( 35,  35),   S( 13,  65),   S(115,  13),   S( 49,  47),   S( -8,  23),
            S( 24,  57),   S(-44,  84),   S( 31,  45),   S( 43,   9),   S(-27,  60),   S( -9, -16),   S( 20,  45),   S( 16,   8),
            S(-52,  25),   S( 32, 107),   S( -1,  68),   S(-34,  -9),   S(-40, -18),   S(-40, -54),   S( 14, -14),   S(-145, -39),

            /* knights: bucket 3 */
            S(-82,  37),   S(-27,  -4),   S(-12,   9),   S(-15,  28),   S( -9,  29),   S(-14,  12),   S(-21,   4),   S(-50,  -9),
            S(-35,   1),   S(-11,  45),   S( -1,  21),   S( -6,  37),   S( -9,  36),   S(  8,  23),   S( 32,  -9),   S(  8, -26),
            S(-17,  32),   S(-16,  43),   S(  0,  54),   S( -7,  86),   S( 15,  70),   S( -2,  49),   S(  5,  38),   S( 10,  -1),
            S( -2,  27),   S(  9,  70),   S( 15,  87),   S( -4,  99),   S(  6, 105),   S( 22,  98),   S( 22,  80),   S(  5,  71),
            S(-16,  72),   S(  7,  84),   S( 18,  96),   S( 29, 113),   S(  9, 112),   S( 27, 115),   S(-17, 119),   S( 43, 101),
            S(-29,  74),   S(  3,  84),   S( 25,  97),   S( -3, 116),   S( 41, 105),   S(118, 108),   S( 39, 116),   S( 32, 115),
            S(-35,  71),   S(-18,  85),   S(-23,  96),   S( 39,  87),   S( 19,  93),   S(112,  35),   S(  8,  -8),   S( 43,  -9),
            S(-182,  77),  S(-22, 116),   S(-38, 124),   S( 10,  79),   S( 67, 103),   S(-61,  90),   S(-17, -58),   S(-67, -123),

            /* knights: bucket 4 */
            S(  4,  11),   S( -2,   0),   S(-77,  20),   S(-25,   2),   S(-12,   0),   S( -6, -21),   S(  0, -37),   S(-10, -15),
            S( 27,  16),   S(-16, -27),   S( 20,   9),   S( 10,  -6),   S( 23, -30),   S( 37, -27),   S(  6,  35),   S(-55,  15),
            S( 17,   3),   S( 34,  -4),   S( 90,   5),   S( 91,  -8),   S( 47,  18),   S( 37,  -7),   S( -8, -40),   S( 13, -28),
            S( -6, -11),   S( 29,   2),   S( 53, -11),   S( 74,  14),   S( 20,  24),   S( -2,  52),   S(-44,  27),   S( -5,  20),
            S( -9, -55),   S( 18, -34),   S( 86,  26),   S( 39,  56),   S( 78,   7),   S( 18,  40),   S(  5,  26),   S(-33,  41),
            S(  3, -16),   S(-15, -17),   S( 39, -24),   S( 97,  34),   S( -7,  23),   S( -2,  34),   S(-26,   1),   S( 13, -11),
            S( -9, -14),   S(-37, -46),   S(-14, -34),   S( 17,  15),   S( 42,  41),   S(  6,  61),   S(  8,  37),   S(-43, -32),
            S( -2,   5),   S( -9, -34),   S( -4, -19),   S( 10,  18),   S(-14, -40),   S(-13,  17),   S(  8,  42),   S(-15, -16),

            /* knights: bucket 5 */
            S( 12,  18),   S( 18,  23),   S( -8,  18),   S(  2,  11),   S( 14,  16),   S(  9,  23),   S(-35,  17),   S( 22,  24),
            S(  4,   8),   S( 46,  28),   S( 43, -15),   S(  4,  -2),   S( 76, -21),   S(-22,  18),   S(-34,  47),   S(-52,   2),
            S(-15,   9),   S( 18,  -8),   S( 56,   3),   S( 44,  15),   S( 45,  26),   S( -2,  16),   S( 12,  -1),   S(-36,   1),
            S( 37,   8),   S( 29, -21),   S( 87,   1),   S(107, -24),   S(102,   3),   S(119,  -8),   S( 18,   7),   S( 24,  18),
            S( 60,  -2),   S( 31,  -5),   S(118, -16),   S(137, -14),   S( 98, -15),   S( 48,  19),   S( 54,  -7),   S( 32,   8),
            S(-44, -24),   S( 18, -18),   S(-21, -43),   S( -1,  18),   S( 33, -18),   S( 46,   0),   S(-17,  13),   S( 33,  29),
            S( -2,   5),   S(-20, -29),   S(-13, -64),   S(  4, -27),   S(-18, -60),   S( 17,   4),   S(  1,  25),   S( 13,  33),
            S(-39, -58),   S(-29, -59),   S( 20,   8),   S(-12,  -9),   S(  8,   4),   S( -8,  38),   S( 18,  32),   S( -9,  23),

            /* knights: bucket 6 */
            S( -5, -30),   S(-30,  18),   S(  9,   1),   S(  6,  25),   S(-11,  26),   S(  9,  19),   S(-58,  69),   S(-21,   7),
            S( 40,   4),   S( 13,  20),   S( 25, -13),   S( 52,  13),   S( 40,   8),   S( 14,  16),   S( 33,  36),   S(-22,  44),
            S( 21,   5),   S( 36,   5),   S( 31,  17),   S( 49,  33),   S( 38,  37),   S( -1,  22),   S( 43,  20),   S(-10,  17),
            S( 42,  30),   S( 94,  -7),   S( 52,  28),   S( 62,  18),   S( 96,   5),   S( 78,  22),   S( 20,   7),   S( -6,  33),
            S(-18,  33),   S( 46,  13),   S(120,   8),   S( 93,   6),   S(129, -34),   S(114,  12),   S(167, -27),   S( 33,   5),
            S( 21,  30),   S( 44,   4),   S( 62,  20),   S( 45,  29),   S( 41,   0),   S( 50,   5),   S( 37, -19),   S( 12,   3),
            S(  7,  28),   S( 13,  33),   S( 33,  55),   S( -2,  10),   S( 23,  -2),   S( 33, -39),   S( -7, -21),   S( 18,  33),
            S( 20,   6),   S( 13,  34),   S( 10,  39),   S(  3,  15),   S( 28, -35),   S(  9,   9),   S(  8,  50),   S(-37, -74),

            /* knights: bucket 7 */
            S(-13, -24),   S(-27, -31),   S( 14,  -4),   S(-35,  26),   S(  8,  -3),   S(-38,   6),   S(-18,  12),   S(-20,  15),
            S(  8, -31),   S( 51, -38),   S(-41,  28),   S(-19,  14),   S( 25,   6),   S( 51,  -1),   S(-47,  54),   S(-55,  43),
            S(-15, -23),   S( -4, -19),   S( 14,  -7),   S( 40,  22),   S( 74,  12),   S( 51,  12),   S( 61,   6),   S(  4,  20),
            S(-39,   7),   S( 40,  -3),   S( 55,   7),   S(116,   0),   S(121, -13),   S( 71,  31),   S( 68,   6),   S( 73,  -2),
            S( 50,   2),   S( 42,   1),   S( 30,  27),   S(101,  -9),   S(133,   1),   S(157, -13),   S(184, -10),   S( 28,  21),
            S(  7,  13),   S( 29,  23),   S(  6,  37),   S( 81,  10),   S(124, -10),   S( 86,  -8),   S(  0, -39),   S( -8, -71),
            S( -8,  20),   S(-35, -13),   S( 32,  17),   S( -2,  42),   S( 79,  31),   S( 42,   3),   S(-30, -55),   S(  0,  -2),
            S(-36, -44),   S(-15, -17),   S(-12,  13),   S(-18,  -5),   S( -4,  24),   S( 21,  16),   S(  8, -14),   S(-11, -26),

            /* knights: bucket 8 */
            S(-11, -26),   S( -7, -50),   S(  6,  19),   S(-11, -72),   S(  1,   5),   S( -6, -21),   S( 10,  20),   S(  2,  -9),
            S(-15, -61),   S( -8, -43),   S(-12, -55),   S( -4, -20),   S( -4,  15),   S(  0, -24),   S(  3, -11),   S(  3, -16),
            S(-14, -84),   S(  1, -15),   S(  5, -61),   S( 19, -24),   S( -9, -44),   S( 23, -17),   S( -1, -38),   S(  1, -12),
            S(-12, -72),   S(-12, -56),   S(  5, -11),   S( 26,  -7),   S( 12, -48),   S( 27,   0),   S(-17, -40),   S(-17, -45),
            S( -9, -33),   S( -4, -34),   S(  4, -19),   S( 13, -29),   S( 26,  -3),   S( -3, -26),   S( -8, -12),   S(  3, -29),
            S( -3, -10),   S( 18, -21),   S( -9, -30),   S( 15, -33),   S( -3,  -7),   S(  6,   7),   S(-12, -35),   S( -4,  -1),
            S( -2,  10),   S( -2, -57),   S(-16, -44),   S(  6,  -9),   S( -2,  -1),   S( 29,  43),   S( -3,  -4),   S( -5, -11),
            S(  3,   2),   S( -9, -30),   S( -7, -18),   S(  3,  -2),   S( -5,   4),   S( -1,   6),   S( -2,   4),   S( -3,  -9),

            /* knights: bucket 9 */
            S(-21, -98),   S( -7, -33),   S(-26, -71),   S( -1, -52),   S(-19, -32),   S(-14, -26),   S( -8, -22),   S( -1, -18),
            S( -4, -12),   S(-21, -96),   S(-19, -123),  S( -9, -66),   S( -5, -57),   S(-12, -64),   S(-13, -65),   S(-13, -36),
            S(-10, -52),   S(-14, -41),   S(-21, -52),   S( 19, -77),   S(-10, -22),   S( 20, -21),   S( -6,  -6),   S( -6, -17),
            S(-22, -49),   S(-21, -77),   S(-13, -60),   S( -6, -68),   S(  9, -59),   S( 29, -13),   S(-11, -49),   S(  6, -28),
            S(  8,  33),   S( -3, -15),   S(-13, -88),   S(  9, -33),   S(  4, -54),   S( 11, -11),   S( 11, -27),   S( -3, -12),
            S(-16, -57),   S( -9, -48),   S( -2, -27),   S( 11, -57),   S( 22, -25),   S(  0, -42),   S( -3, -22),   S( -4,  -7),
            S(-17, -45),   S(  3,   1),   S(-10, -18),   S(-20, -47),   S(  9, -10),   S( 13,   5),   S(-12, -10),   S( -8, -17),
            S(  1,   2),   S(  0, -15),   S(-10, -14),   S( -6, -18),   S(-10, -31),   S(-12, -49),   S( -1, -16),   S(  0,  -3),

            /* knights: bucket 10 */
            S(-11, -47),   S( -4,   2),   S(-27, -76),   S(-13, -24),   S(-19, -86),   S( -8, -64),   S(  6,   0),   S( -7, -52),
            S( -5, -58),   S(  9, -30),   S(-11, -61),   S(  7, -65),   S(  2, -45),   S(-12, -85),   S(-12, -12),   S(  9,  23),
            S( -3, -42),   S(  5, -32),   S(-14, -37),   S( 15, -23),   S( -6, -77),   S( -2, -45),   S(-16, -42),   S( -6, -11),
            S( -6, -68),   S(  8, -25),   S( 27, -42),   S(  9, -49),   S( -9, -49),   S( -4, -46),   S(-19, -94),   S( -8, -57),
            S(-12, -51),   S( 12, -31),   S(  2, -25),   S( -3, -43),   S( 24,  -5),   S( -2, -64),   S( -2, -34),   S(  5, -32),
            S(  6,   0),   S(-16, -44),   S(  1, -14),   S( 12, -14),   S(-14, -34),   S(-16, -51),   S( -6, -16),   S( -3, -24),
            S( 11,  19),   S( -2, -17),   S(-16, -27),   S( 18, -40),   S( -3, -36),   S(-20, -62),   S(-10, -17),   S(-16, -28),
            S( -5, -15),   S( -2,  -8),   S(  1,  20),   S( -9, -24),   S(-10, -27),   S(-11, -39),   S(  6,   6),   S(  0,   9),

            /* knights: bucket 11 */
            S(-10, -44),   S(-24, -57),   S( 10,   1),   S( 13,  -5),   S(-46, -36),   S(-14, -22),   S(-28, -70),   S( 17,  28),
            S(-10, -31),   S(-25, -48),   S(  0, -72),   S( 34, -45),   S( 26,  -5),   S( -2, -50),   S(-10, -38),   S( -5, -11),
            S(-16, -49),   S(-17, -56),   S(  8, -16),   S( 24, -11),   S(  5,  17),   S( 32, -10),   S(  7, -32),   S(  9, -10),
            S(-11, -14),   S( 31, -22),   S( 19, -28),   S( 55,  -9),   S( 36, -29),   S( 15, -25),   S(  6, -64),   S( -2,  -9),
            S( -6,   4),   S(  4, -65),   S( -4,  -1),   S( 31, -28),   S( 37,  -2),   S( 21,  20),   S( -6, -104),  S( -6, -12),
            S(-11, -37),   S( -7, -60),   S( 12, -13),   S(  4, -17),   S( 11,   3),   S( 10,  -8),   S(  1, -41),   S( -3,  -6),
            S(  4,  13),   S( -9,  -2),   S(  4,  -4),   S( 11,   4),   S(  9, -30),   S( 32, -13),   S( 12,   8),   S(  1,   2),
            S( -1, -10),   S(-10, -24),   S(-11, -44),   S( -4,  12),   S( -2, -25),   S( -9, -37),   S(  4,  -1),   S( -2, -12),

            /* knights: bucket 12 */
            S(-12, -48),   S(  0, -25),   S(  0, -13),   S( -9, -18),   S( -4, -13),   S( -2, -14),   S( -2,  -4),   S( -3,  -9),
            S( -3, -23),   S(  0,  -7),   S( -1,  -9),   S(-16, -41),   S(  4, -34),   S( -6, -20),   S(  4,  -5),   S(  0,  -3),
            S( -8, -25),   S(-11, -47),   S( -6, -22),   S(-11, -75),   S( -8, -15),   S(  4, -10),   S( -4,  -7),   S(-12, -37),
            S(  4,   7),   S( -2, -52),   S( -5, -26),   S(  0, -15),   S( 12, -17),   S(  8,   3),   S(  9,  21),   S( -4, -19),
            S(  0,  -9),   S( -4, -33),   S( -7, -33),   S( -6, -33),   S( 11,   6),   S( -2, -20),   S(-10, -39),   S(-11, -40),
            S(  2,   1),   S(  2, -14),   S( -5, -20),   S(  2, -14),   S( -2, -12),   S(-11, -47),   S( 11,  18),   S( -1,  -4),
            S( -6, -16),   S( -6, -11),   S(-10, -11),   S( -6,  -9),   S( -1,   4),   S(-15, -38),   S( -3, -13),   S( -3,  -2),
            S(  0,   0),   S(  4,  33),   S(  1,  -3),   S( -4, -17),   S( -2,  -2),   S(  8,  24),   S(  0,  -1),   S(  0,   2),

            /* knights: bucket 13 */
            S( -2,   0),   S( -4, -46),   S( -1, -24),   S(  1, -33),   S(-11, -29),   S( -4, -14),   S( -9, -27),   S(  1,   6),
            S(  1,   2),   S( -7, -27),   S(  0, -12),   S( -8, -33),   S(-14, -44),   S(  1, -17),   S(  2,   9),   S( -9, -30),
            S( -1, -12),   S(  2,  11),   S(  3, -27),   S( -2, -39),   S(  6,  20),   S( -4,   3),   S(  7,   4),   S(  6,  13),
            S(  4,  -3),   S( -1, -17),   S( -9, -26),   S( -9, -57),   S(  6, -39),   S(  3, -20),   S( 14,  29),   S(  9,  11),
            S( -1,  17),   S(  5, -24),   S(  2, -40),   S( 11, -20),   S( -5, -23),   S( -3, -30),   S( -2, -34),   S(-11, -37),
            S( -6, -26),   S(  5,   8),   S( -1,   7),   S(  7, -23),   S( -7, -19),   S( -7, -44),   S(  7,  18),   S(  0,   2),
            S(  3,   1),   S(  6,  17),   S( -7,  -2),   S( -2, -36),   S(  9,  40),   S( -4,  -4),   S(  8,   9),   S( -1,  -5),
            S(  0,   1),   S(  2,   8),   S( -2,  -7),   S(  1,   6),   S( -4, -15),   S(  2,   1),   S( -2,  -8),   S(  0,   1),

            /* knights: bucket 14 */
            S(  2,   2),   S( -4, -10),   S(  8,  23),   S( -3, -11),   S( -5, -37),   S( -4,  19),   S(  1,   0),   S( -2, -13),
            S( -3, -16),   S( -8, -42),   S(  1, -16),   S(  3, -29),   S(  3, -22),   S( -1, -11),   S(-10, -42),   S(  4,  21),
            S( -9, -38),   S( -4, -47),   S(  5,   3),   S(-10, -73),   S(  2, -22),   S( -1, -21),   S( -5, -10),   S(  6,  18),
            S(  0,   4),   S(  1, -24),   S(-10, -31),   S(  4,   8),   S( 10,  15),   S( -7, -59),   S(  7,  14),   S( -7, -12),
            S( 11,  21),   S(-12, -39),   S(  4, -27),   S( -5, -17),   S(  6,  54),   S(  3, -18),   S( -3,  -8),   S( -1, -21),
            S(  0,   1),   S(  0,   3),   S( -1,  49),   S(  9,   5),   S(  5,   4),   S(  0,   3),   S(-10, -13),   S( -7, -25),
            S(  1,   2),   S( -5, -16),   S(  9,  12),   S(  4,  15),   S(  6,  19),   S( -6, -21),   S(  2,  -9),   S( -1,  -6),
            S(  0,   0),   S(  0,  -1),   S(  1,   4),   S(  2,  12),   S( -1,  -8),   S(  0,   0),   S(  1,  10),   S(  0,   1),

            /* knights: bucket 15 */
            S( -5, -24),   S(  0,  -1),   S(  4,  19),   S( -9, -18),   S(-11, -32),   S(-14, -56),   S(  1, -24),   S( -1,  -7),
            S( -1,  -2),   S(  0, -11),   S(-11, -40),   S( 11,  19),   S(  7, -24),   S( -7, -41),   S( -4, -19),   S(  0,  -8),
            S(  0,  -8),   S(-11, -41),   S( -8, -32),   S(  6, -26),   S(-20, -76),   S(  5, -20),   S( -2, -25),   S( -5, -17),
            S( -4, -18),   S( -4,  -8),   S(  0, -21),   S( -7,   3),   S(  5, -22),   S(  0, -23),   S(  5,  10),   S(  1,   6),
            S( -1, -13),   S(  8,   8),   S(  4, -13),   S( -5,  14),   S( 17,  23),   S(  2,  -7),   S(  3,  -6),   S( -1,  -1),
            S( -1,   3),   S( -9, -14),   S( -3,   8),   S( -7, -12),   S(  0,   3),   S(  6,  47),   S(  2,  22),   S(  5,  28),
            S( -1,  -6),   S( -3, -12),   S(  7,  30),   S(  2,   6),   S(  5,  19),   S(  5,   4),   S( -2,  -3),   S(  3,   5),
            S(  4,  12),   S(  0,  -7),   S(  0,  -1),   S( -3, -16),   S(  2,  10),   S(  0,  -1),   S(  2,  12),   S(  1,   4),

            /* bishops: bucket 0 */
            S( 34, -10),   S(-16,   6),   S(-19,   6),   S(-12, -18),   S(-13,   4),   S( 15, -11),   S( 79, -82),   S( 30, -36),
            S(-12, -22),   S(  1, -22),   S( -2,  17),   S( 13,  -2),   S( 17,  -6),   S( 78, -37),   S( 47,  33),   S( 43, -51),
            S( 17,  12),   S( 23,  -5),   S( 20,  -4),   S( 24,  -5),   S( 44, -18),   S( 49,  25),   S( 49, -27),   S( 25, -21),
            S( 32, -21),   S( 77, -75),   S( 31, -11),   S( 62, -23),   S( 81,  11),   S( 41,  24),   S(  9,  -9),   S( 22, -10),
            S( 55, -11),   S( 47, -39),   S( 90, -17),   S(124,  -5),   S(125, -23),   S( 45,   1),   S( 57,   8),   S( -3, -18),
            S( 15,   6),   S(128, -23),   S( 90,  -4),   S( 84, -10),   S( 15,   4),   S( 43,  19),   S( 68, -15),   S( -1,  -1),
            S(-71, -94),   S( 77,  23),   S( 97,  61),   S( 18,  -8),   S( 12, -17),   S( 32,  18),   S( 31,  25),   S(-36,  19),
            S(-13, -47),   S(-10,  15),   S(  1, -25),   S(-25, -15),   S(-14,  -5),   S(  1,   4),   S( 20,  25),   S( -3,  -6),

            /* bishops: bucket 1 */
            S(-42,  -3),   S(  3, -32),   S(-25,  30),   S( 24, -30),   S(-14,   6),   S( 13,  -7),   S( 45, -24),   S( 31, -40),
            S(  9, -52),   S(-10, -14),   S(  0,  -9),   S(-10,   6),   S( 39, -22),   S( 19, -16),   S( 64, -26),   S( -9, -10),
            S(-23,  -4),   S( 34, -20),   S( -5,  -6),   S( 34,  -7),   S( 11, -13),   S( 49, -21),   S(  3, -10),   S( 70, -22),
            S( 40, -45),   S( 46, -20),   S( 34, -10),   S( 25,  -8),   S( 56, -12),   S( 11,  -2),   S( 85, -40),   S(-10, -14),
            S(  3, -16),   S( 50, -20),   S( -3,  10),   S(111, -18),   S( 64, -19),   S( 94, -28),   S(  3,  -3),   S( 47, -14),
            S( 72, -33),   S( 28, -13),   S( 71, -14),   S( 68, -23),   S(126, -44),   S(-21,  -3),   S(-24,  29),   S(-18,   1),
            S(-22, -56),   S(  0, -23),   S(-18, -30),   S( 30,  23),   S( 45,   1),   S(-25,  20),   S( 13,  -1),   S(-30,  14),
            S(-15, -46),   S(-29, -10),   S(  2, -50),   S(-68, -15),   S(  4, -31),   S(  8, -14),   S( 43,  -5),   S(-29, -34),

            /* bishops: bucket 2 */
            S( 15, -15),   S(  9, -25),   S(  2,  11),   S(-18,   6),   S( 12,  13),   S(-17,  17),   S( 11,  -4),   S( 17,  -3),
            S( 24, -32),   S( 12,  -6),   S(  1, -12),   S( 16,   9),   S( -7,  11),   S( 11,   4),   S( 10,  -4),   S( 15, -53),
            S( 42,  -9),   S( 28, -15),   S(  4,  20),   S(  2,   2),   S(  9,  21),   S(  2, -11),   S(  1, -15),   S(-11,   3),
            S( 11, -18),   S( 51,  -8),   S( 11,   0),   S( 36,  18),   S(  3,  21),   S(  0,  19),   S(-21,   9),   S( -4,   0),
            S(  8,   8),   S(  2,  14),   S( 73,  -9),   S( 27,  19),   S( 28,  28),   S( 14,  25),   S( 14,  25),   S( 44, -23),
            S(-30,  22),   S(  6,  26),   S(-34,   6),   S(106, -34),   S( 47, -14),   S(107,   2),   S( 91, -11),   S( 16, -33),
            S( -4,  21),   S(-21,   9),   S( -9,  13),   S( 29,   3),   S(-44, -44),   S(-42,  -9),   S(-43,  19),   S(-13, -34),
            S(-66, -12),   S(-37,   8),   S( -7,   6),   S(-24,  13),   S(-22,  -4),   S(-22,  -2),   S( -5, -37),   S(-31, -35),

            /* bishops: bucket 3 */
            S( 29,  32),   S( 57, -35),   S( 10,   7),   S(  9,  14),   S( 14,  34),   S(  0,  44),   S(-11,  57),   S( 12,   0),
            S( 43,   0),   S( 26,  16),   S( 28,   7),   S( 24,  20),   S( 27,  18),   S( 25,  24),   S( 16,  21),   S( 39, -22),
            S(  5,   6),   S( 37,  42),   S( 29,  46),   S( 29,  34),   S( 22,  44),   S( 24,  30),   S( 16,  16),   S( 15,  28),
            S( -5,  12),   S( 17,  40),   S( 40,  53),   S( 33,  60),   S( 43,  35),   S( 31,  20),   S( 35,  -3),   S( 23, -18),
            S( 26,  19),   S( 13,  54),   S( 19,  58),   S( 63,  59),   S( 49,  54),   S( 52,  32),   S( 25,  35),   S( -8,  34),
            S(  6,  23),   S( 39,  59),   S( 23,  35),   S( 18,  53),   S( 51,  43),   S( 56,  73),   S( 53,  67),   S( 17, 110),
            S(-27,  72),   S( 29,  43),   S( 20,  34),   S( 18,  39),   S(  5,  53),   S( 77,  55),   S(-40,  14),   S( 13, -13),
            S(-45,  42),   S(-26,  44),   S(-49,  50),   S( -5,  54),   S(  4,  42),   S(-109,  71),  S( 29,  16),   S( 10,  20),

            /* bishops: bucket 4 */
            S(-38,  -1),   S(-35,  -8),   S(-42,  11),   S(-36, -12),   S(-32, -13),   S(-23, -20),   S( -5, -19),   S( -9, -61),
            S(-35,  18),   S( 28, -32),   S( 54, -31),   S(  3, -14),   S(-33,  -2),   S( 24, -52),   S(-21, -26),   S(-53, -60),
            S(-15,  15),   S(-23, -24),   S( 31,  -8),   S(-13, -28),   S( -7,  -6),   S(-29,  12),   S(-12, -10),   S(-78, -10),
            S( 22, -63),   S( 48,  -9),   S( 45, -25),   S( 59,  -3),   S( 10,   5),   S( 65,  -4),   S(-55,   5),   S(-14, -13),
            S(  6, -11),   S(  2, -93),   S( 56, -18),   S( 51, -33),   S( 19,  -1),   S( 61, -22),   S(-26,  42),   S(-30,  -1),
            S(-64, -76),   S(-16, -45),   S( 17, -29),   S( 37,  -2),   S( -7,  -7),   S( 33,  28),   S(-21,  13),   S(  1,  33),
            S( 12,  13),   S(-15, -19),   S( -6, -46),   S(-18, -46),   S(-11,  -5),   S( 19,  -1),   S(-21,   7),   S( 34,  43),
            S(-10, -14),   S( -1, -37),   S(-15, -44),   S(  1, -35),   S( -8, -45),   S(-24,  -6),   S(-17,  41),   S(  2,  26),

            /* bishops: bucket 5 */
            S(-54,  -1),   S(  2,   1),   S(-40,   6),   S(-62,  20),   S( 12, -12),   S(-37, -14),   S(-42,  -6),   S(-47,  -8),
            S(-20,  -8),   S(-41,   2),   S( 13, -17),   S( 14,  -3),   S(-37,   2),   S( 12,  -4),   S( -2, -16),   S( 14, -12),
            S( 28,  12),   S(-19, -17),   S( 26, -13),   S(-20,   0),   S(  5,   2),   S(-80,  21),   S(  8, -18),   S( -4,  13),
            S(  1,  -5),   S(-33,   0),   S( 58, -24),   S( 81, -17),   S(-21,  16),   S( 26,   3),   S(-77,  18),   S(  3,  -7),
            S( -7, -28),   S(  0, -17),   S(  9, -27),   S(  6, -35),   S( 31, -21),   S(  3,   2),   S( 30,  -6),   S(-64,   0),
            S( -8, -29),   S(-28, -11),   S( 28, -43),   S(-41, -40),   S(  5, -26),   S( 25,  -9),   S( -1,   6),   S( -5,  12),
            S(-38, -27),   S(-17, -24),   S(-29,  -8),   S( -3,   3),   S(  0, -27),   S(  8,  -8),   S( 18,  18),   S( -6,   2),
            S(-17, -15),   S(-15, -19),   S(-14, -12),   S(-13, -16),   S(-29,  14),   S(  2,   5),   S(-38,   1),   S( -6,  29),

            /* bishops: bucket 6 */
            S(-37,  -9),   S(-64,  13),   S(-40,  17),   S(  9,  -3),   S(-39,  24),   S(-53,  26),   S(-48,  30),   S(-74,  10),
            S(-47,  -3),   S(-20, -16),   S(-49,  14),   S( -1,   8),   S(-58,  19),   S(-36,  13),   S(-49,  38),   S(-45,  -5),
            S( 33, -20),   S(-61,  17),   S(  1,  -2),   S(-30,  17),   S( -4,  15),   S(-22,   8),   S(-49,  10),   S(-18,  21),
            S(-55,   7),   S(-19,  -4),   S(-37,  18),   S( 80,   0),   S( 31,  11),   S( 25,  10),   S( -1, -21),   S( 18,   9),
            S(-53,  18),   S(  3,   4),   S( 36,  -3),   S( 78,  -8),   S( -9, -21),   S(-22, -13),   S( 50, -16),   S(-14, -22),
            S(-56,  29),   S( 28,   7),   S(-16,   5),   S( 15, -10),   S(-22, -15),   S(-25, -10),   S( -8,  16),   S(-48,  -4),
            S(-25,  22),   S(-39,  24),   S( 13,  -8),   S( 15, -23),   S( -9,   7),   S( -2,  -4),   S(  1,  -7),   S(-39, -10),
            S(-34,   9),   S(  1, -13),   S(-29,  11),   S( -4,   1),   S( -9,   7),   S(  2, -37),   S(-20,   8),   S( 14, -23),

            /* bishops: bucket 7 */
            S(  6, -48),   S(-74, -37),   S(-34, -35),   S(-22,  -8),   S(-41,  -3),   S(-23, -11),   S(-60, -23),   S(-21,  -6),
            S(  3, -60),   S( 19, -49),   S(  0, -20),   S(-26, -18),   S(-36,   4),   S(-50,   9),   S(-22, -27),   S(  7,   8),
            S(-38, -29),   S(-26,  -2),   S( -5,   1),   S( 28,  -6),   S( 18, -17),   S( 29, -23),   S(-55,  -2),   S(-45,   7),
            S(-40, -21),   S(-48,  15),   S(-32,  19),   S( 16,  -4),   S(106, -18),   S( 31, -13),   S( 52, -34),   S(-48, -26),
            S( 23, -30),   S(  9,  -6),   S( 28, -14),   S( 48,  -6),   S( 59, -16),   S(104, -41),   S( 12, -21),   S(-25, -27),
            S(-68,  -2),   S(-12,  26),   S( 12,  -3),   S(-42,  15),   S( 15,  -1),   S(102, -34),   S( 21, -17),   S(-40, -86),
            S(-41, -15),   S(-39,  33),   S(-19,  13),   S( 27,  -7),   S( -6, -19),   S( -4, -30),   S( 13,  -8),   S(  2,  -1),
            S(-24, -19),   S(-26,   2),   S(-11,  12),   S(  5, -29),   S( -9, -13),   S( -2, -22),   S(  8, -40),   S( -7, -20),

            /* bishops: bucket 8 */
            S( 23,  88),   S(-17, -29),   S( 14, -13),   S(  5,  41),   S(  3,  12),   S(-13, -38),   S( -8, -30),   S( -7, -16),
            S( -9,  -5),   S( 16,  40),   S( -2,  -5),   S( -1,  -3),   S(  9,  -5),   S( 20,  28),   S(-30, -48),   S(-14, -31),
            S(-11, -39),   S(-10, -41),   S( 27,  58),   S( 32,  21),   S( 17,  21),   S( 23,  -9),   S(-16, -30),   S(-34, -69),
            S( -8, -33),   S( 28,  77),   S(  5, -21),   S( 15,  61),   S( 32,  30),   S( 32,  23),   S(  6,  20),   S(  4,  -2),
            S(  5,  69),   S( 28, 102),   S(  1,  12),   S( -8,  -1),   S( 12,  56),   S(-14,  28),   S( -5, -31),   S( 12,  36),
            S( -9, -18),   S(  9,  48),   S(  0,  34),   S( 11,  26),   S(  6,  19),   S( 18,  62),   S( 15,  30),   S(  3,  34),
            S( -3,  18),   S(-15, -43),   S( 16,  67),   S( -3,  63),   S( -4,  32),   S(  4,  29),   S( 17,  82),   S(-25,  -3),
            S( -1,   1),   S(  4,   0),   S( -7, -24),   S( -4,   1),   S(  2,  29),   S( 11,  28),   S( -8,  11),   S( 10,  74),

            /* bishops: bucket 9 */
            S(  3,  35),   S(-29,  12),   S(-23, -13),   S(-33, -12),   S(-21, -17),   S( -9, -24),   S( -7, -23),   S( -9, -12),
            S(-16, -35),   S(-11,  19),   S(  4,   7),   S( -8, -27),   S(  6, -37),   S( -7, -15),   S(-20, -22),   S(-20, -31),
            S( -4, -28),   S(  8,  32),   S( -3,  -5),   S( -8, -16),   S(  6,   8),   S( -3, -18),   S(  1,   2),   S(-16, -17),
            S(-10,  19),   S(-18,   1),   S( -4,   9),   S(  7,  31),   S(-28,  30),   S(  0,  15),   S( -8,   0),   S( 23,  19),
            S(  0,  44),   S(-18,  20),   S( 17,  20),   S(-27,   3),   S( -6,  19),   S(-24,  32),   S(  4,  23),   S(-17, -45),
            S(  1,  33),   S(-19,  14),   S( -8,  22),   S( -6,  10),   S(-29,  11),   S(-27,  27),   S(  0,  20),   S(-11,   6),
            S( -3,  26),   S(-13,  26),   S( -4,  18),   S( -8,  18),   S( -2,  26),   S( -4,  33),   S( -1,  44),   S( -4,  71),
            S(  5,  42),   S( -8,   1),   S(-11,   5),   S( -1,  10),   S(  0,  37),   S( -8,  29),   S( -4,  43),   S( 19,  86),

            /* bishops: bucket 10 */
            S(-14,  -1),   S(  1,  40),   S(-19, -35),   S(-13, -23),   S(-48, -50),   S(-40, -55),   S(-14,  31),   S( -7,  16),
            S( -2,  19),   S( -5,  -6),   S(-12, -15),   S(-35, -52),   S(-31, -15),   S(-39, -43),   S(-22, -45),   S( -7,  31),
            S(-16, -35),   S(-17, -25),   S(-24,  -8),   S( -9,   8),   S(  1, -44),   S(  4, -15),   S(-11, -10),   S( -4, -66),
            S(-10, -10),   S(-24, -37),   S(-40, -25),   S(  0,  -3),   S( -1,   9),   S(  1,  -8),   S( 11,  38),   S(-30, -23),
            S( -9,  17),   S(-49,   7),   S(-13,  -3),   S( -6,  15),   S( -9,  12),   S(-10,  12),   S(-21,   0),   S(-11,  28),
            S(-19,  22),   S(  0,   7),   S(-19,   3),   S(-20,  14),   S(-14, -16),   S( -9,  -3),   S(-25,  21),   S(  2,  24),
            S(-12,   6),   S( -6,  31),   S(-21,   3),   S( -2,  20),   S( -1,  -8),   S(-30,   0),   S(-10, -18),   S(  4,  26),
            S( -7,  54),   S(-14,  30),   S( -3,  28),   S(-15,  15),   S( -7,  31),   S( -6,  12),   S(  2,  19),   S( -4,  -1),

            /* bishops: bucket 11 */
            S( 14,  -9),   S( -2,  -2),   S(-23,   9),   S(-25,   5),   S(-16, -39),   S( 16, -32),   S(-21, -14),   S(-40,   9),
            S(  4,  29),   S(  7, -23),   S(-13,  10),   S( -9, -24),   S( -3,  -3),   S(-49, -23),   S(-28,   5),   S(  7,  -7),
            S(  2, -22),   S(  1, -11),   S( 28, -10),   S( -4,   1),   S(-19, -21),   S( 13,  20),   S(-11, -38),   S( -7, -15),
            S( 15,   7),   S( -7, -34),   S( 16,  14),   S(-18, -11),   S( 16,   6),   S( 26,  36),   S( 14,  34),   S( -8, -33),
            S(-30,   3),   S( -8,   4),   S(-31,  21),   S(-24,  31),   S(  7,  -5),   S( 15,  23),   S( -6,  24),   S(  2,  43),
            S(  0,  31),   S(-20,  -8),   S(-24,  37),   S( -6,  19),   S(-12,  57),   S(  1,  23),   S(-11,   3),   S(-19,  -7),
            S(-10,  21),   S(-21,  67),   S( 10,  43),   S(  5,  34),   S(  2,   4),   S(-26,  -3),   S(-16, -34),   S( -3,  -8),
            S(  3,  94),   S(-14,  23),   S(  9,  55),   S(  8,  23),   S(  1,  34),   S(  2,  41),   S(-18, -31),   S(  3,  12),

            /* bishops: bucket 12 */
            S( -6, -29),   S( -5, -25),   S( -5, -22),   S(  6,  18),   S( -5, -21),   S( -1,  -2),   S(  2,  10),   S(  2,  13),
            S(  1, -11),   S( 14,  46),   S( -1,  -2),   S(-14, -20),   S(  0,  -8),   S(  3, -15),   S(-19, -39),   S( -4, -30),
            S(  9,  50),   S(  6,  18),   S( 25,  64),   S( 12,  -8),   S(  1,   7),   S(  2, -22),   S( 13,  32),   S( -9,  -1),
            S(  2,  54),   S(  6,  53),   S(  6,  39),   S( 16,  21),   S( 14,  -2),   S(  2, -11),   S(  7,  13),   S(  3,  12),
            S(  9,   0),   S(  4,  18),   S( -1,  26),   S( 17,  49),   S( 13,  50),   S( 12,  28),   S(  4,   2),   S(  3,   6),
            S( -1,   0),   S(-14, -39),   S( -1,  12),   S( -2,   1),   S( 14,  41),   S(  7,  42),   S(-10, -39),   S( -1,  -6),
            S( -3,  -5),   S(  6,   9),   S( -2,   3),   S( -1,  12),   S( 11,  33),   S(  3,  47),   S(  9,  42),   S(  3,  33),
            S(  0,   4),   S(  2,   9),   S( -5, -12),   S( -2, -10),   S( -4, -12),   S(  6,  24),   S(  3,  36),   S(  6,  26),

            /* bishops: bucket 13 */
            S(-10, -34),   S( -6, -16),   S(-11, -31),   S( -9,  -3),   S( -3,  -8),   S(-11, -44),   S(-11, -34),   S( -5, -24),
            S(-10,   7),   S( -8,  -9),   S( -3,  -1),   S( 11,  68),   S(-10, -12),   S(  4,   5),   S( -9, -11),   S( -1,  -8),
            S(  3,  38),   S( 19,  85),   S(  7,  34),   S( 21,  42),   S( -3, -15),   S( 12,  52),   S( -9, -26),   S(-10, -15),
            S(  5,  58),   S(  4,  51),   S(  6,  29),   S(-14,  10),   S(  4,  21),   S( -1, -11),   S( 10,  35),   S(  6,  14),
            S(  2,  34),   S( -1,  23),   S( -2,   3),   S(  6,  25),   S( 10,  22),   S( -4,  23),   S(  8,  15),   S( -2,  18),
            S( -6,  21),   S(  2,  10),   S( -1,  39),   S( 12,  32),   S(-11,  29),   S(-17, -26),   S(  3,  24),   S(  0,   6),
            S(  4,  32),   S(-11, -34),   S( -5, -20),   S( -2,  26),   S(  1,  22),   S(  0,  41),   S(  1,  11),   S(  2,  39),
            S(  2,  -1),   S(  0,   3),   S( -1,   3),   S(  2,  10),   S(  7,  33),   S( -4, -18),   S(  9,  38),   S(  1,  11),

            /* bishops: bucket 14 */
            S( -6, -12),   S( -2,   5),   S(  7,   6),   S(-13, -10),   S(-16, -32),   S(  3,   4),   S( -4, -35),   S( -6, -52),
            S(-10, -29),   S( -4, -17),   S( -4,  12),   S( -1,   5),   S( 10,  33),   S(  2,  16),   S( -7,  -7),   S(  5,  26),
            S( -7, -16),   S(-11, -19),   S(-24, -55),   S(  6,  27),   S( 21,  46),   S(  6,  49),   S(  5,  61),   S(  0,  11),
            S(  4,  29),   S(  2,   3),   S( -8,  17),   S( -6,  48),   S(  5,  39),   S( 10,  61),   S( -3,  57),   S(  5,  54),
            S(  1,  26),   S( -8, -15),   S( -3,  30),   S(  4,  57),   S( -8, -16),   S(  2,  44),   S( 13,  41),   S( -6,  12),
            S(  0,  -1),   S(  9,  22),   S( -1,  50),   S(  7,  41),   S(  0,  47),   S( -1,  24),   S( -3,   1),   S(  2,  -7),
            S(  9,  58),   S( 13,  60),   S( -1,  26),   S(  1,   1),   S(  0, -10),   S(  3,   1),   S(  0, -21),   S(  6,  36),
            S(  2,  28),   S(  0,  27),   S(  5,  27),   S(  3,  23),   S( -5, -27),   S(  2,   9),   S( 11,  26),   S(  2,   4),

            /* bishops: bucket 15 */
            S( -2,  -2),   S(  7,  17),   S(-20, -48),   S(  4,  -5),   S(-19, -44),   S( -3, -52),   S( -4, -38),   S( -1,  -8),
            S(  7,  25),   S(  0,   7),   S(  8,  20),   S(  9,  15),   S( 24,  35),   S( -5, -23),   S(  2,  10),   S( -8, -16),
            S(  4,  -2),   S(  1, -15),   S(  1,  29),   S( 12,  13),   S( 27,  46),   S( 11,  33),   S(  6,  46),   S(  3,  19),
            S( -1,  -4),   S(  8,  22),   S(  4,  14),   S(-11, -16),   S( 11,  35),   S(  8,  17),   S( 10,  46),   S(  1,  47),
            S(-10, -11),   S(  4,  10),   S( -3,  17),   S( 19,  61),   S( 16,  65),   S( 11,  40),   S(  5,  31),   S(-10, -40),
            S(  6,   5),   S(  2,  25),   S( 11,  63),   S( -2,  29),   S( 17,  75),   S(  6,  35),   S( -2,   4),   S( -3,  -5),
            S(  2,  18),   S(  1,  24),   S( -1,  17),   S(  5,  26),   S(  6,  29),   S( -5,  -3),   S(  3,  23),   S( -1,  -1),
            S(  0,  18),   S(  5,  36),   S( 10,  52),   S(  3,  11),   S(  0,  11),   S( -2,   7),   S(  1,   8),   S(  2,  16),

            /* rooks: bucket 0 */
            S(-11,  23),   S( 13,   6),   S( -3,  13),   S(  6,  19),   S(-25,  73),   S(-13,  62),   S(-38,  80),   S(-55,  61),
            S( -5,  16),   S(  4,  36),   S(-38,  47),   S(-14,  43),   S( -6,  72),   S( -6,  41),   S(  1,  11),   S(-33,  79),
            S( 22, -17),   S(  9,  14),   S(-25,  37),   S( 10,  13),   S(-38,  79),   S(-23,  59),   S(-16,  70),   S( -1,  28),
            S( 19,  -9),   S( 37,  19),   S(-59,  70),   S( 13,  24),   S( 20,  62),   S(-28,  71),   S(-36,  81),   S(-14,  50),
            S( 38, -21),   S( 22,  25),   S(  8,  59),   S( -3,  49),   S( 37,  46),   S( 34,  54),   S( 65,  54),   S( 16,  58),
            S( 47, -13),   S( 63,  63),   S( 38,  41),   S( 92,  42),   S(-14,  86),   S( 45,  59),   S( 15,  67),   S(-47,  96),
            S( 35,  50),   S( 69,  67),   S(118,  73),   S( 30,  61),   S( 74,  57),   S( 23,  80),   S( -8,  83),   S(-15,  90),
            S( -5, -48),   S( 28,  31),   S(  6,  45),   S( 72,  -6),   S( 94,  49),   S( 67,  21),   S( 46,  36),   S( 55,  -4),

            /* rooks: bucket 1 */
            S(-47,  48),   S(-24,  20),   S(  0,  16),   S(-30,  44),   S(-36,  53),   S(-31,  58),   S(-33,  58),   S(-76,  89),
            S(-43,  46),   S(-40,  16),   S(-41,  41),   S(-35,  38),   S(-26,  28),   S(-50,  53),   S(-23,  22),   S(-17,  44),
            S(-39,  42),   S(-13,  19),   S(-23,  19),   S(-25,  38),   S(-41,  39),   S(-52,  46),   S(-45,  72),   S(-20,  61),
            S(-56,  71),   S(-19,  44),   S(-11,  47),   S(-19,  49),   S(-37,  52),   S(-46,  83),   S(-33,  65),   S(-35,  82),
            S(-17,  63),   S(-11,  12),   S( 40,  29),   S( 27,  24),   S( -8,  33),   S(-17,  82),   S(-56, 106),   S(-26, 102),
            S( 73,  27),   S( 61,   5),   S( 43,  19),   S( -2,  49),   S( 19,  25),   S( 20,  64),   S(  6,  63),   S( 12,  86),
            S( 24,  73),   S( 45,  17),   S( 17,  45),   S( -2,  23),   S( 30,  57),   S( 29,  61),   S( 30,  85),   S( 41,  96),
            S( 51,   3),   S( 65, -21),   S( -4, -20),   S(-35, -40),   S( 29,   9),   S( 35,  30),   S( 28,  55),   S( 68,  41),

            /* rooks: bucket 2 */
            S(-62,  89),   S(-52,  81),   S(-46,  73),   S(-35,  46),   S(-24,  47),   S(-36,  50),   S(-28,  32),   S(-76,  82),
            S(-56,  85),   S(-63,  83),   S(-53,  87),   S(-57,  65),   S(-55,  61),   S(-55,  51),   S(-32,  28),   S(-49,  58),
            S(-42,  72),   S(-32,  83),   S(-58,  76),   S(-40,  67),   S(-41,  55),   S(-35,  55),   S(-13,  37),   S(-23,  64),
            S(-31,  89),   S(-39,  96),   S(-61,  97),   S(-71,  85),   S(-55,  81),   S(-28,  61),   S(-32,  59),   S(-13,  49),
            S(-31, 112),   S(-44, 103),   S( -8,  94),   S(-36,  78),   S(-30,  81),   S( 17,  54),   S(-19,  71),   S(-21,  95),
            S(  5,  97),   S(  9,  95),   S( 26,  76),   S(-34,  76),   S( 40,  38),   S( 33,  65),   S( 82,  26),   S( 68,  66),
            S( 42,  93),   S( 20,  90),   S( 29,  76),   S( 39,  56),   S( 31,  32),   S( 42,  87),   S(-73, 116),   S( 16, 100),
            S( 15,  71),   S( 51,  49),   S( 67,  35),   S( -7,  47),   S(-24,  30),   S( 23,  33),   S( -9,  62),   S( 13,  59),

            /* rooks: bucket 3 */
            S( -8, 107),   S( -7, 111),   S(-11, 134),   S(  1, 124),   S(  5,  88),   S( 14,  82),   S( 33,  49),   S( -4,  45),
            S(  9, 105),   S( -1, 110),   S(-11, 135),   S(-11, 135),   S( -1,  93),   S( 15,  57),   S( 55,  26),   S( 37,  46),
            S( 14, 100),   S(  0, 125),   S(-17, 123),   S(  1, 128),   S( 11,  92),   S( 13,  68),   S( 41,  70),   S( 29,  54),
            S(  0, 127),   S( -6, 149),   S(-19, 149),   S( -8, 142),   S(-11, 116),   S( 12,  97),   S( 41,  81),   S( 15,  66),
            S( -3, 146),   S(-15, 152),   S( 22, 143),   S(  7, 150),   S( 10, 125),   S( 18, 114),   S( 57, 104),   S( 47,  77),
            S(-19, 166),   S( 11, 146),   S( 29, 147),   S( 28, 146),   S( 84,  93),   S(109,  86),   S( 85, 103),   S( 49, 105),
            S(  9, 154),   S( 11, 157),   S( 17, 164),   S( 12, 169),   S( 26, 150),   S(109,  94),   S(146, 155),   S(195, 100),
            S( 92,  48),   S( 41, 104),   S( 34, 140),   S( 14, 136),   S( 23, 108),   S( 74, 107),   S( 86,  77),   S(127,  38),

            /* rooks: bucket 4 */
            S(-15, -29),   S( 26, -29),   S(  5, -37),   S(-36,  20),   S(-35,   0),   S(-11,  14),   S(-35,  -3),   S(-70,  25),
            S(-36, -48),   S(-43,  -2),   S( -2, -30),   S( 11, -37),   S( -1,  14),   S(  5,  22),   S(-30, -12),   S(  7,  33),
            S(-22, -20),   S(-46, -32),   S(-12, -12),   S(-22, -27),   S(-45,  15),   S(-46,  16),   S(-30, -24),   S(-56,  17),
            S(-51, -27),   S( 27,  -8),   S(  5, -34),   S( 22, -19),   S( 36,   5),   S(-30,  21),   S(-23,   4),   S(-62,  19),
            S(-41, -23),   S( 33,  -2),   S( 35,  -4),   S( 67, -25),   S( 61,   0),   S( 71,  17),   S( 10,   3),   S( 26,  12),
            S(-19, -49),   S(  4, -15),   S( 25,  -9),   S( 34,  35),   S( 41,  14),   S( 14,  21),   S( 27,  10),   S( 25,  29),
            S(-31, -25),   S( 28,   5),   S( 57, -14),   S( 63, -13),   S( 25,  10),   S( 10,  17),   S(  3,  -4),   S( -7,  17),
            S( 13, -28),   S( 49,   0),   S( 47, -34),   S(  0,   0),   S( 58,  10),   S( 34, -18),   S(  5,   2),   S( 11,  24),

            /* rooks: bucket 5 */
            S(-44,  27),   S(-17, -10),   S( 14, -20),   S( 23,  11),   S( -8,   1),   S(-16,  25),   S(-34,  44),   S(-42,  36),
            S(-42,  12),   S(-23, -20),   S( 13, -25),   S( 27, -17),   S(-13,   4),   S(-13,  -7),   S(-65,  36),   S(-43,  42),
            S(-79,  35),   S( -3, -11),   S( 16, -28),   S(-32,   6),   S(-41,   6),   S( 20, -14),   S(-28,  13),   S(-13,  18),
            S(-68,  39),   S( -7,   6),   S( 51, -33),   S( 20,   8),   S( 26, -11),   S(-29,  34),   S(-17,  20),   S(  7,  31),
            S( 40,  12),   S( -1,   8),   S( -5,  25),   S( -9, -20),   S(-37,  26),   S( 74,   7),   S( 34,  11),   S( 19,  27),
            S( 48,   2),   S( 17,  -5),   S(  8,  -6),   S( -6, -17),   S( 14,  10),   S( 14,  32),   S( 62,  15),   S( 31,  25),
            S( 59, -22),   S( 30, -25),   S( -7,  -6),   S( 31,  -9),   S( 75, -14),   S( 42, -19),   S( 68, -21),   S( 27,   8),
            S( 17,  25),   S(  0,   1),   S( 24,  -7),   S(  3,  19),   S( 51,  -2),   S( 30,  25),   S( 47,  37),   S(  8,  54),

            /* rooks: bucket 6 */
            S(-62,  58),   S(-27,  31),   S(-31,  31),   S( -7,  16),   S( -7,   5),   S( 12,   3),   S( -2,   3),   S(-30,  11),
            S(-50,  38),   S( -1,   2),   S(-19,  20),   S(-11,   1),   S( 11, -10),   S(-45,  13),   S(-38,   8),   S(-42,  30),
            S(-101,  56),  S(-10,  27),   S(-17,  14),   S(-19,  12),   S(-25,  19),   S( 15,   0),   S(-48,  10),   S(-23,   2),
            S(-63,  52),   S(-11,  42),   S(-35,  33),   S( 39,  -4),   S( 37,  -5),   S( 13,  15),   S( 31,  -7),   S( -3,  27),
            S( -8,  48),   S( 45,  18),   S( 84,  17),   S( 67,   1),   S( 39,  -8),   S(-22,  28),   S( 49, -11),   S( 90,   0),
            S( 94,   8),   S( 83,   1),   S(104,   5),   S( 47,  -1),   S(  6, -27),   S( 17,  40),   S( 47, -23),   S( 81,   7),
            S( 30,   9),   S( 75,  -4),   S(116, -25),   S(106, -28),   S( 62, -14),   S( 44,  13),   S( 58, -21),   S( 70, -21),
            S( 31,  20),   S( 21,  32),   S( -2,  35),   S( 44,  -1),   S( 26,  11),   S( 68,  27),   S( 76,   3),   S( 54,  15),

            /* rooks: bucket 7 */
            S(-107,  40),  S(-73,  35),   S(-88,  57),   S(-53,  42),   S(-49,  24),   S(-34,   1),   S(-32,  17),   S(-97,  13),
            S(-82,  45),   S(-34,  12),   S(-55,  26),   S(-73,  42),   S(-50,  -3),   S(-20,  -8),   S(  3,  10),   S(-18, -44),
            S(-63,  39),   S(-71,  45),   S(-73,  32),   S(-76,  42),   S(-62,  13),   S(-39,  20),   S( 54, -15),   S(-59,  -5),
            S(-107,  49),  S(-28,  26),   S( 14,   8),   S( 53,  -8),   S(-19,  12),   S( 46,  -1),   S( 42,  13),   S(-21, -10),
            S( 12,  31),   S(  6,  28),   S( 37,  24),   S( 64,  10),   S(125, -25),   S(131, -41),   S( 89,   9),   S(-47, -17),
            S(-16,  47),   S( 18,  15),   S( 72,  16),   S( 58,  -5),   S(100,  -3),   S( 42,   6),   S( 32,  40),   S( -4, -15),
            S(-32,  22),   S( 36,  -7),   S( 59,  -3),   S( 72, -10),   S( 78, -18),   S( 69, -25),   S( 75,   4),   S( 59, -45),
            S(-78,  24),   S(-51,  36),   S(  7,  22),   S( 32,  15),   S( 29,   1),   S( 44,   3),   S( 65,   0),   S( 67, -18),

            /* rooks: bucket 8 */
            S( 17, -92),   S( 12, -49),   S( 42, -50),   S( 22, -23),   S(-12, -70),   S(-15, -32),   S( -8, -54),   S( -5, -46),
            S(-23, -85),   S(-11, -30),   S( 16, -26),   S(-19, -42),   S( -3, -66),   S( -8, -66),   S( 10,  -8),   S(-24, -26),
            S( 12,   8),   S( -9, -34),   S(  1, -10),   S(-13,  -3),   S( -1,  22),   S( 37,  40),   S(  4,  19),   S(-15, -38),
            S( -7, -43),   S(  1, -13),   S(  7,   3),   S( 29,  21),   S( 10,  -4),   S( 31,  16),   S( 13,  28),   S(-10, -43),
            S(  0, -52),   S( 17,  13),   S( 18,  -4),   S( 31,  22),   S(  9,  -6),   S( -3, -18),   S(  2,  11),   S( 14,  21),
            S(  8, -11),   S( 13, -21),   S( -1, -14),   S(-24, -48),   S(-12,  23),   S( -2,  29),   S(  7,  21),   S( 14,  38),
            S( 23,  16),   S( 26, -15),   S( 14,  -6),   S( 14, -15),   S(  8,  27),   S( 17,  37),   S(  9,  33),   S(  9,   4),
            S(-11,  15),   S( 12, -24),   S( 11, -20),   S( 17,  43),   S(-11,  18),   S(  9,   6),   S(  2,   8),   S( 12,   8),

            /* rooks: bucket 9 */
            S(-18, -99),   S( 27, -98),   S( -7, -109),  S(  0, -78),   S( 21, -79),   S( 31, -69),   S(  1, -57),   S( 24, -58),
            S(-33, -91),   S(-11, -85),   S( -9, -57),   S(-13, -86),   S(  6, -83),   S(-12, -17),   S(-37, -69),   S(-14, -37),
            S( -7, -23),   S(-10, -48),   S(  2, -20),   S(-10, -51),   S( 16, -40),   S( -2, -27),   S(-18, -19),   S( -5,  -1),
            S( 12, -23),   S(-10, -41),   S(  1, -13),   S(  2, -25),   S( -9, -53),   S( 18, -32),   S( -7, -35),   S( 10, -32),
            S( 10, -48),   S( -4, -37),   S( -3, -67),   S( -7, -32),   S( -5, -67),   S(  1, -39),   S(  3, -48),   S(-10, -38),
            S( 16, -42),   S(-33, -61),   S(-27, -68),   S( 10, -16),   S( 19, -29),   S(  9, -10),   S( -8, -14),   S( -4, -25),
            S(  6, -24),   S( 28, -34),   S(  3, -54),   S( -7,  -8),   S(  4, -38),   S(  0, -32),   S(  9,  -3),   S(-14, -36),
            S(-16,  -9),   S( 15, -24),   S( -5, -34),   S(  1, -32),   S(  8, -15),   S(  8,  19),   S(-21,  -6),   S(  6,  19),

            /* rooks: bucket 10 */
            S( -6, -84),   S( -4, -76),   S( 10, -91),   S(  3, -89),   S( 18, -107),  S( 11, -107),  S( 26, -108),  S(-33, -92),
            S(-18, -55),   S(-26, -62),   S(-21, -57),   S(-29, -86),   S(-27, -91),   S( -9, -67),   S(-14, -80),   S(-45, -83),
            S(  0, -54),   S(-46, -26),   S(-11, -52),   S(-48, -54),   S(-14, -43),   S( -9, -54),   S(  1, -57),   S( -8, -43),
            S(-18, -29),   S(-17, -55),   S( -2, -48),   S(-19, -36),   S( -4, -27),   S(  2,   4),   S(-27, -101),  S(  3, -67),
            S(  5, -44),   S( 11, -49),   S( 10, -44),   S(  4, -70),   S(  7, -29),   S(  9, -28),   S( -8, -79),   S(-21, -84),
            S(-18, -27),   S( -4, -14),   S( -1, -62),   S(-16, -92),   S(  3, -51),   S( -2, -36),   S(-23, -62),   S(  4, -48),
            S(-23, -55),   S(-23, -49),   S(-11, -42),   S( 13, -58),   S( 30, -50),   S(  6, -60),   S(-13, -74),   S(-10, -51),
            S(-14, -30),   S( -3,  -8),   S( -6,  11),   S(-28, -32),   S(  9,   2),   S( -7, -20),   S( -4, -42),   S( -7, -23),

            /* rooks: bucket 11 */
            S(-20, -71),   S(-16, -28),   S(  2, -44),   S( -6, -42),   S(-24, -39),   S( 11, -83),   S( 10, -56),   S(-14, -120),
            S(-12, -31),   S(-11, -30),   S(-60, -42),   S(-55, -11),   S(-13, -19),   S( 13, -25),   S(-19, -81),   S(-31, -102),
            S(-42, -11),   S(-39,   1),   S(  1,   6),   S(-26,  -3),   S( 35, -12),   S(  8,   0),   S( 13,  -6),   S(  2,  19),
            S(-11, -41),   S( -7, -20),   S(-20, -25),   S( 10, -27),   S( 18,  -7),   S(-16, -70),   S(  7,   8),   S( -8, -30),
            S(  0, -32),   S( 10, -45),   S( -5, -21),   S(  5,  -4),   S( 42, -12),   S( 11, -31),   S( 31,   8),   S(-16, -54),
            S(  0, -30),   S(-12, -25),   S( 17, -31),   S( 15,  -9),   S(-13, -37),   S( 31, -32),   S( 25, -20),   S( -5, -48),
            S( -1,  -1),   S(-25, -46),   S( -9,  -7),   S( -7, -14),   S(  2, -25),   S( 21, -31),   S( 39, -38),   S(  8, -15),
            S( -2,  -5),   S( 28,  20),   S( -6,  13),   S( 18,  27),   S(-11,   1),   S( 21, -13),   S( 43, -19),   S(-16,   6),

            /* rooks: bucket 12 */
            S(-28, -95),   S( -3, -11),   S(-13, -66),   S(-16, -86),   S(  1, -50),   S(  0, -26),   S(-15, -61),   S(-13, -44),
            S(  0, -27),   S(  2,   7),   S( -1,  -6),   S(  9,  -6),   S( 14,  -8),   S( 23, -15),   S( 11,  -4),   S(-15, -57),
            S(-11, -26),   S( -7,  -8),   S( 12,  15),   S( 16,  -1),   S(  4, -25),   S( 18,   5),   S(  6,   7),   S(  3,   2),
            S( -1,  12),   S(-10, -39),   S(  4,   9),   S(  2, -16),   S(  9,   0),   S( -1, -14),   S(  6,  15),   S(  1,   2),
            S( 11, -22),   S( 13,  17),   S(  0, -37),   S( -2, -32),   S( 16,  45),   S(  0,  -5),   S(  3,  -8),   S(  8,  18),
            S(  3, -29),   S(  5, -22),   S( 11, -11),   S( -7, -17),   S(  1, -10),   S(-20, -52),   S( 11,  28),   S(  4, -11),
            S(-14, -15),   S( -5, -23),   S(  1, -12),   S( -3,   5),   S(-16, -45),   S( 15,  21),   S(  0,  18),   S(  4,  13),
            S(  4,  -7),   S( -9, -24),   S( -2, -10),   S(  7, -12),   S(  7,   9),   S(  4,   7),   S( -4,  -9),   S(  2,  20),

            /* rooks: bucket 13 */
            S(-18, -61),   S(-27, -94),   S(-11, -34),   S(-16, -49),   S(-11, -61),   S( -7, -51),   S(-14, -58),   S(-27, -60),
            S(-10, -48),   S( -8, -35),   S( -3, -19),   S( -4, -27),   S( 17,   4),   S(  0, -59),   S( 14, -25),   S( -9, -67),
            S(  2, -27),   S( -8, -48),   S( -3, -13),   S(  4, -26),   S( 16, -11),   S(  6, -46),   S( 13,  -2),   S( -3, -83),
            S(  6, -14),   S(-20, -68),   S(  1, -41),   S( 12,  -7),   S( 11, -21),   S( -3, -23),   S(  1, -23),   S(  1,  12),
            S(  8,   0),   S(  2, -77),   S(  0, -53),   S(  1, -40),   S(  4, -65),   S(-10, -53),   S(  2,  10),   S(  9,   8),
            S( -4, -22),   S( -4, -44),   S( -8, -34),   S( -1, -43),   S( -1, -79),   S( 13, -18),   S( -5, -28),   S(  7,  -6),
            S(  2,  -8),   S(  3, -17),   S(-11, -71),   S( -2, -29),   S(-16, -77),   S( -1, -27),   S( -2, -16),   S( -5, -38),
            S(  2, -25),   S(-12, -39),   S(  4, -16),   S( 17,  13),   S(-18, -63),   S(  5,  -7),   S( -2, -21),   S(  7,   6),

            /* rooks: bucket 14 */
            S( -8, -51),   S(-30, -56),   S( 15, -19),   S(-15, -76),   S(-10, -73),   S( -4, -69),   S(-35, -134),  S(-14, -52),
            S(  4, -26),   S(  8, -32),   S(  7, -40),   S(-13, -65),   S( -3, -34),   S(  5,   1),   S(  5, -33),   S(-13, -81),
            S( -3,  -4),   S(  6, -12),   S(  3, -41),   S( -5, -62),   S(  3, -13),   S( -5, -20),   S(  5, -43),   S(-32, -126),
            S(-12, -35),   S( 14,  29),   S(  2,  -4),   S(  5, -20),   S( -7, -25),   S( -1, -31),   S(  7, -28),   S( -4, -20),
            S(  1,   0),   S( 10,  18),   S(  4, -19),   S(  0, -69),   S( 10, -12),   S( 24,   9),   S( -8, -64),   S( -4, -41),
            S(  3, -20),   S(  1,   2),   S( -2, -31),   S( 12, -70),   S(  6, -40),   S( -2, -70),   S( -3, -68),   S( -9, -45),
            S( -6, -12),   S( -1,  10),   S(-10, -57),   S(-21, -97),   S( -8, -54),   S(  1, -28),   S(-17, -57),   S( -5, -31),
            S(-10, -38),   S( -5, -20),   S( -9, -38),   S( -2, -42),   S( -2, -30),   S(-13, -100),  S( -7, -64),   S(-10, -33),

            /* rooks: bucket 15 */
            S(-17, -73),   S(-10, -48),   S(-32, -60),   S(-23, -63),   S(-13, -57),   S(  6, -28),   S(  4,  -7),   S(-10, -62),
            S(  0, -31),   S(-10, -20),   S( -5, -24),   S( 11, -12),   S( -7, -49),   S(  1, -19),   S( 16,  18),   S(  6,   2),
            S(  8,  13),   S(-10, -56),   S(  8, -13),   S(  5, -34),   S(  1, -40),   S( -7, -45),   S( 17,  23),   S(  0,  -6),
            S(  4,  15),   S( -1,   2),   S( 12,  25),   S( -8, -13),   S( 14,  16),   S( -4, -44),   S( 11,  -2),   S(  5,   0),
            S(  1,  -6),   S(-10, -39),   S(  0,  -6),   S( 10,   7),   S( 16,  40),   S(  7, -28),   S(  9, -17),   S( -8, -34),
            S( -5,   6),   S(  5,  14),   S(  3,  -1),   S( -4, -28),   S( -9, -37),   S(  2, -42),   S(  0, -34),   S(-10, -31),
            S(  2, -15),   S( -1,  -6),   S(  1,  -1),   S( -5, -26),   S( -4,  -9),   S(  3,  -5),   S( 12, -41),   S( -1, -26),
            S(  1, -10),   S( -1,  -2),   S(  0,  -3),   S(  6,  20),   S( -9, -31),   S(-11, -49),   S(  0, -43),   S(-30, -83),

            /* queens: bucket 0 */
            S(  1, -42),   S(-22, -54),   S(-38, -77),   S( -6, -100),  S(-12, -56),   S( 13, -84),   S(-71, -52),   S(-22, -34),
            S(-26, -42),   S( 19, -104),  S(  1, -77),   S( -7, -54),   S(  2, -44),   S( -7, -56),   S(-25, -58),   S(-66,  -7),
            S(-18,  36),   S( -9,   0),   S(  6, -46),   S(-10,  -9),   S( -8,  -1),   S( -2,  -7),   S(-32, -33),   S(-85, -85),
            S(-36,  65),   S( 15, -30),   S(-17,  17),   S(-25,  63),   S(  2,  46),   S(-19,   3),   S(-56,  29),   S(-20, -45),
            S(-21,   0),   S(  6,  64),   S(-13,  67),   S(-11,  80),   S( -2,  57),   S(-24,  67),   S(-54,  49),   S(-60,   1),
            S(-14,  -4),   S( 11,  63),   S( 29,  67),   S(-31,  99),   S(-51,  76),   S(-52,  65),   S(-88,   9),   S(-47, -29),
            S(  0,   0),   S(  0,   0),   S(  1,  35),   S( -9,  17),   S(-54,  42),   S(-78,  70),   S(-101,  78),  S(-120,  22),
            S(  0,   0),   S(  0,   0),   S( 33,  27),   S( -4,   0),   S(-28,  52),   S(-35,  16),   S(-47,  20),   S(-43, -16),

            /* queens: bucket 1 */
            S( -5, -27),   S( -9, -11),   S( -2, -64),   S( 14, -75),   S( 28, -92),   S(  7, -80),   S(  4, -43),   S(-44,  -7),
            S(-30,  -4),   S( 10,   0),   S( 28, -50),   S( 18, -19),   S( 37, -38),   S(  0, -12),   S(-29,  20),   S(-24, -30),
            S( 35, -11),   S( 14, -18),   S(  4,  -1),   S( 16,  33),   S(-14,  61),   S( 29,  23),   S(-14,  17),   S( -1, -27),
            S( 35, -21),   S( 19,  49),   S( 12,  49),   S( 26,  71),   S( -5,  79),   S(  3,  33),   S(  1,  38),   S( -2,  32),
            S(  9,  22),   S( 28,  59),   S( 40,  81),   S( 27,  64),   S( 14, 104),   S( 79,  18),   S(  0,  81),   S( 13,  29),
            S( 65,  -2),   S( 95,  41),   S( 95,  80),   S(109,  83),   S( 85,  54),   S(-29,  87),   S( 18,  72),   S( -6,  48),
            S( 93, -33),   S( 61,  15),   S(  0,   0),   S(  0,   0),   S(  3,  55),   S(-30,  70),   S(-10,  87),   S(-53,  50),
            S(104,   2),   S( 43,  22),   S(  0,   0),   S(  0,   0),   S( 43,  43),   S( 31,  40),   S( 84,  32),   S(-25,  47),

            /* queens: bucket 2 */
            S( 31, -32),   S( 42, -28),   S( 28,  12),   S( 41, -28),   S( 47, -48),   S( 32, -39),   S(-21, -36),   S( 27,   7),
            S( 17,  -7),   S( -3,  52),   S( 38,  -3),   S( 44,  15),   S( 53, -19),   S( 22,  -8),   S( 34,   8),   S( 17,  35),
            S( 41,  36),   S( 27,  39),   S( 17,  92),   S( 20,  50),   S( 36,  46),   S( 23,  57),   S( 25,  15),   S( 30,  47),
            S( 31,  53),   S( 22,  78),   S( 16,  78),   S( 10, 109),   S( 27,  85),   S(  0,  97),   S( 27,  77),   S( 36,  75),
            S( -9,  81),   S( 37,   5),   S(  0, 125),   S( 25, 126),   S( 35, 130),   S( 79,  64),   S( 56, 100),   S( 72,  57),
            S(-29, 103),   S(-30, 101),   S( 18,  99),   S( 77,  92),   S( 54,  82),   S(115, 107),   S(115,  70),   S( 37, 138),
            S( -4,  92),   S(-18,  90),   S(-13, 125),   S( 95,  58),   S(  0,   0),   S(  0,   0),   S( 36, 121),   S( 55,  97),
            S(  0,  55),   S( 62,  35),   S( 54,  32),   S( 65,  64),   S(  0,   0),   S(  0,   0),   S( 55,  75),   S( 21, 101),

            /* queens: bucket 3 */
            S(-35,  69),   S( -3,  36),   S(-11,  46),   S(  6,  49),   S( -8,  22),   S( -6,  21),   S(  7, -60),   S(-44,  24),
            S(-44,  53),   S( -6,  33),   S( -6,  62),   S(  5,  73),   S(  6,  60),   S( 13,  29),   S( 38,   4),   S( 40, -43),
            S(-29,  69),   S(-19, 101),   S(-14, 110),   S(-21, 142),   S( -3, 103),   S( -8, 101),   S(  5,  66),   S(  1,  38),
            S(-18,  67),   S(-41, 120),   S(-31, 152),   S(-15, 163),   S(-23, 160),   S(-17, 117),   S(  3, 109),   S( -5,  95),
            S(-23,  95),   S(-34, 138),   S(-32, 149),   S(-13, 187),   S( -7, 171),   S( 10, 183),   S( -1, 169),   S(-15, 115),
            S(-51, 104),   S(-43, 152),   S(-57, 187),   S(-31, 184),   S(-32, 207),   S( 15, 177),   S(-18, 209),   S( -5, 163),
            S(-85, 137),   S(-82, 152),   S(-64, 182),   S(-79, 207),   S(-66, 201),   S(  6, 136),   S(  0,   0),   S(  0,   0),
            S(-134, 189),  S(-90, 177),   S(-47, 124),   S(-79, 162),   S(-24, 138),   S( 21, 109),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-52,  -1),   S(-45, -23),   S(-30,  -1),   S(-30, -11),   S( -5,  -2),   S(-33, -20),   S(-36, -47),   S( -8,  -3),
            S(-26,   1),   S( -8,  -6),   S( 17, -15),   S(-20, -30),   S(-66,  20),   S(-21,  47),   S(-44, -10),   S(-27, -28),
            S(  7,  30),   S( 34, -32),   S(  6, -11),   S(-18, -18),   S( 27,   7),   S(-21,  -1),   S(-30, -38),   S( 43,  25),
            S(-26, -23),   S( 53,  -4),   S(  3,  18),   S(  5,  37),   S( 41,  33),   S(  2,  26),   S(-49, -39),   S(-10,  -6),
            S(  0,   0),   S(  0,   0),   S( 51,  28),   S( 54,  40),   S( 32,  47),   S( -5,  50),   S( -1, -21),   S( 30,  20),
            S(  0,   0),   S(  0,   0),   S( 18,  11),   S( 47,  46),   S( 28,  38),   S( 17,  48),   S(  2,  15),   S(-38,  -8),
            S( 25,   1),   S( 38,  42),   S( 74,  27),   S( 67,  51),   S( 55,  46),   S( 18,   9),   S(  9,  15),   S(-20,  15),
            S( 60,  19),   S( -5, -27),   S( 40,  26),   S( 46,  37),   S( 11,   2),   S( -2,   5),   S( -9, -21),   S(  1,   7),

            /* queens: bucket 5 */
            S( 23,   7),   S( 40,   9),   S( 30,  18),   S( -7, -14),   S(  6,  -4),   S( 28,  31),   S(  2,   2),   S(-11, -13),
            S( 11,  11),   S(-23,  -1),   S( 16,  -2),   S(-10,  21),   S(  2,   1),   S(-36,  -7),   S( 15,  -4),   S( 22,   4),
            S(  7,  -1),   S( 36, -17),   S( 24,  29),   S(-10,  23),   S(-17,  10),   S( 34,  -1),   S( -2,  30),   S( 14,  22),
            S( 14,  -2),   S( 38,  16),   S( 20,  -8),   S( 61,  50),   S( 71,  35),   S( 12,  12),   S( 41,  25),   S(-32,  21),
            S( 42,  -1),   S( 37,   6),   S(  0,   0),   S(  0,   0),   S( 18,  34),   S( 28,  28),   S( 74,  56),   S( 18,   2),
            S( 39,  23),   S( 52,  39),   S(  0,   0),   S(  0,   0),   S( 45,  55),   S( 70,  46),   S( 42,  22),   S( 45,  42),
            S( 70,  15),   S( 65,  11),   S( 78,  92),   S( 28,  45),   S( 73,  58),   S(120,  71),   S( 47,  49),   S( 14, -17),
            S( 34,  24),   S( 48,  22),   S( 69,  39),   S( 52,  42),   S( 47,  13),   S( 51,  36),   S( 49,  16),   S( 35, -10),

            /* queens: bucket 6 */
            S( 57,  36),   S( -7, -28),   S(-32, -27),   S( 27, -18),   S(-12,   3),   S(-28, -14),   S( 17,   6),   S( 19,   4),
            S( 27,  15),   S( 35, -12),   S( -3,  22),   S( 27,  22),   S( 15,   9),   S( 20,  32),   S(-44,   4),   S(  6,  14),
            S(-20,  24),   S( 50,   1),   S( 23,  12),   S( 22, -24),   S( 13,  33),   S(  3,   1),   S( 63,  18),   S( 62,  47),
            S(-29,  11),   S(-29,  34),   S( 42,  13),   S( 80,  28),   S( 62,  -2),   S( 35,  12),   S(104,  21),   S( 90,  28),
            S( -1,   2),   S(  2,  49),   S( 30,  39),   S( 50,  50),   S(  0,   0),   S(  0,   0),   S( 67,  49),   S( 96,  72),
            S( 21,  35),   S( 44,  31),   S( 59,  54),   S( 32,  30),   S(  0,   0),   S(  0,   0),   S( 90,  80),   S(116,  64),
            S( 29,  20),   S(-14,  18),   S( 63,  21),   S( 57,  34),   S( 44,  53),   S( 80,  70),   S(137,  42),   S(159,  18),
            S( 20,  18),   S( 36,  14),   S( 38,  23),   S( 88,  54),   S(114,  39),   S( 91,  41),   S(123,  37),   S( 60,  32),

            /* queens: bucket 7 */
            S(-25, -12),   S(-16, -17),   S(-39,   1),   S(-38,   8),   S(-18,  31),   S(-30,   0),   S( -3,  11),   S(  7, -16),
            S(-25, -47),   S(-67,  -5),   S(-33,  45),   S(-33,  58),   S(-50,  37),   S(-11,  29),   S(-25,  30),   S( -7, -14),
            S( -4, -20),   S(-24, -10),   S(-22,  28),   S( 15,  29),   S( 63,  -6),   S( 14,   3),   S( 31, -13),   S( 47,  21),
            S(-73,   6),   S(  2,  -6),   S(-18,  45),   S( 36,  41),   S( 88,  18),   S( 92,  16),   S( 82, -47),   S( 17,   6),
            S(-21, -10),   S(-13,  31),   S(  6,  50),   S( 31,  52),   S( 74,  33),   S( 56, -10),   S(  0,   0),   S(  0,   0),
            S(-40,  13),   S( -5,  49),   S(-17,  46),   S(-30,  57),   S( 62,  15),   S(114,  69),   S(  0,   0),   S(  0,   0),
            S(-27,  29),   S(-43,  -3),   S(-19,  21),   S( 21,  34),   S( 69,  14),   S(102,  10),   S( 60,  22),   S( 68,  23),
            S( -4, -30),   S( 39, -14),   S( 31,  12),   S( 44, -17),   S( 32,  43),   S( 42,   0),   S( 22,   5),   S(107,  12),

            /* queens: bucket 8 */
            S(-17, -35),   S( -1, -22),   S(-12, -42),   S(-15,  -8),   S(  4, -12),   S(  4, -12),   S( -3, -12),   S(  0,  -2),
            S(-22, -35),   S(  0,   9),   S( 20,  14),   S( -2,  -6),   S( 32,  31),   S( -7,  -8),   S(-13, -16),   S(  3,   6),
            S(  0,   0),   S(  0,   0),   S( 31,  34),   S( -4, -22),   S( 13,   0),   S( 11,  -2),   S(-13, -19),   S( -5,  -1),
            S(  0,   0),   S(  0,   0),   S( 12,  15),   S( 10,  20),   S(  3,   5),   S( -9, -17),   S( 12,  31),   S(  1, -11),
            S(  3,  -6),   S( 14,  29),   S( 25,  41),   S( 32,  29),   S( 13,  16),   S( 12,  15),   S(  2,  -4),   S(-15, -22),
            S( 24,   7),   S( 12,  -4),   S( 10,  13),   S(  8,   1),   S(  4,  13),   S(-11, -37),   S(  5,  -4),   S(-11, -26),
            S(-15, -33),   S( -3, -12),   S( 24,  25),   S( 29,  36),   S( 25,  13),   S( 11,  18),   S(  1, -12),   S(  1,   2),
            S( -7, -20),   S( 12,  13),   S(  7,  -5),   S( -5,  -4),   S( 15,   5),   S(-17, -24),   S(-12, -15),   S(-26, -53),

            /* queens: bucket 9 */
            S( 15,  -9),   S(-10, -49),   S( -7, -21),   S( 14, -23),   S( -4, -25),   S( -2, -14),   S(-11, -20),   S(-10, -30),
            S( 22,  19),   S( -2,  -9),   S( -5, -12),   S( 11,   5),   S( -6, -26),   S( -6, -31),   S( -7, -20),   S( -3, -14),
            S( -3, -29),   S( -1,  -8),   S(  0,   0),   S(  0,   0),   S(  8,  11),   S( 15,  -1),   S( -5, -11),   S(  4,   2),
            S( 14,   9),   S( -8, -20),   S(  0,   0),   S(  0,   0),   S( 11,  16),   S( 27,  17),   S( 13,  14),   S(-12,  -8),
            S( -3, -21),   S( 15,   6),   S(  5,   3),   S(-20, -24),   S( -5, -23),   S(  9,   8),   S(  5, -11),   S( 10, -13),
            S( 31,  10),   S( 13, -12),   S( 19,   6),   S(  0,  -1),   S( 16, -12),   S(  1,   8),   S(-12, -37),   S( 11,   3),
            S( 10,  14),   S( 11,  -2),   S( -9, -23),   S( 29,  35),   S( 39,  20),   S( 26,  15),   S( 12,  31),   S(-11, -36),
            S( 18,  -6),   S(  8,   1),   S(-16, -19),   S( 10,   6),   S( 20,  29),   S(  4,   3),   S(-21, -46),   S(-15, -38),

            /* queens: bucket 10 */
            S( 19,  37),   S( 20,   2),   S(  8,  11),   S( -9, -22),   S(  6, -21),   S( 18,  17),   S( -2,  -8),   S( -6, -25),
            S(  8,   8),   S( -8, -25),   S(  4, -15),   S(-14, -25),   S(-13, -23),   S( 21,  15),   S( -9, -39),   S(  2,  -4),
            S(  0,   8),   S(  0,   3),   S(  0, -12),   S( -1, -12),   S(  0,   0),   S(  0,   0),   S( 14,  14),   S( -8, -25),
            S( -6, -20),   S(  7, -15),   S( 12,   7),   S( 21,  32),   S(  0,   0),   S(  0,   0),   S( -6, -23),   S( 15,   7),
            S(  2,   1),   S( 29,  35),   S( -7, -24),   S( 51,  63),   S(  6,   5),   S( -2,  -3),   S(-13, -41),   S( 10, -13),
            S( -5, -16),   S(  2,  -3),   S( 22,  31),   S( 17,  35),   S( 18,  16),   S( 28,  45),   S( 19,  -4),   S(  3, -24),
            S(  5,   3),   S( 18,  35),   S( 12,  11),   S( 17,  13),   S(-11, -13),   S( 18,  24),   S( 19, -18),   S( -1, -21),
            S(-12, -35),   S( -5, -16),   S( 24,   7),   S(-20, -46),   S(  6,  -4),   S(  0, -10),   S( -4, -20),   S(  6,  -9),

            /* queens: bucket 11 */
            S(-16,  -7),   S(-14, -32),   S(  2,   1),   S(-10, -31),   S( 11,  18),   S(-32, -49),   S(  0, -17),   S( 14,  10),
            S(-12,  -4),   S( -1,  -7),   S(-34, -37),   S( 19,   0),   S( 42,  10),   S(  5, -10),   S( -2, -15),   S( 11,   3),
            S( -7,  -7),   S(  5,  -1),   S( -1,   2),   S( -3, -17),   S(  0,  -7),   S( -2, -17),   S(  0,   0),   S(  0,   0),
            S(-12, -23),   S(-22, -31),   S(-12,   5),   S(  8, -10),   S(-12, -29),   S(  1,   6),   S(  0,   0),   S(  0,   0),
            S(-12, -13),   S( -1,   7),   S( -2,  12),   S( -1,   3),   S( 45,  50),   S( 21,  27),   S( 22,  24),   S(-12, -24),
            S(-15, -19),   S(  1,  -5),   S(-13, -14),   S(  2, -12),   S(  8,   8),   S( 26,   6),   S(  7,  10),   S( 18, -10),
            S(-12, -21),   S(  2,   5),   S(  8,  -3),   S( -9,   1),   S( 12,  31),   S(  2, -15),   S(-24, -30),   S( -6,  -8),
            S(-35, -74),   S(  4,   5),   S(-15, -19),   S( 20,  17),   S( 26,  15),   S(  3,   4),   S( -2,  -9),   S( 21,  10),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  4,   8),   S(-17, -27),   S( -5,  10),   S(  0,  -2),   S( -3,  -2),   S( -2,  -2),
            S(  0,   0),   S(  0,   0),   S( 10,  15),   S(  3,  -5),   S(  4,  -1),   S(  0, -12),   S(  2,  -2),   S( -1,  -6),
            S( -6, -14),   S( 11,   8),   S( -6, -21),   S( 15,   5),   S( 21,  40),   S( 12,  26),   S( -5,  -9),   S(  2,   2),
            S(  2,  -6),   S( 11,   5),   S( 13,  32),   S( 12,  16),   S(  0, -19),   S( -2, -16),   S(  0,   0),   S( -1,  -6),
            S(-16, -39),   S(  2,   4),   S( 13,  23),   S( -9, -27),   S( 10,  29),   S(-12, -34),   S(-11, -25),   S( -5, -15),
            S(  0,  -7),   S( -1,  -3),   S(-16, -25),   S(  8,  13),   S(-17, -38),   S(-14, -22),   S(-19, -25),   S( -5, -15),
            S(-16, -30),   S( 11,  20),   S( -9, -22),   S(  5,   8),   S( -9, -20),   S(-15, -36),   S(  0,   3),   S( -5, -16),
            S(  8,  12),   S(  4,   0),   S(  0,   8),   S( -5,  -9),   S(-16, -30),   S(-15, -31),   S(  5,  13),   S(-14, -22),

            /* queens: bucket 13 */
            S(-10, -37),   S( -5, -17),   S(  0,   0),   S(  0,   0),   S(-26, -54),   S(-16, -27),   S(  5,  -8),   S(  1,   4),
            S( -4, -29),   S( -4,  -9),   S(  0,   0),   S(  0,   0),   S(-20, -40),   S(-18, -41),   S( -8, -21),   S(-15, -38),
            S(  3,  -4),   S( -3, -14),   S(  1,  -2),   S( 17,  29),   S( -5, -12),   S(-10, -26),   S( -6,  -7),   S(  2,   3),
            S(-16, -29),   S(-20, -55),   S( 13,   4),   S(  8,  -2),   S( 27,  57),   S( 20,  33),   S( -7, -27),   S(  7,  15),
            S(  8,  11),   S( -6, -14),   S(  5,   2),   S( 20,  33),   S( -9,  -6),   S(  4,   3),   S(-14, -28),   S( -3, -10),
            S( 16,  20),   S(-13, -36),   S(  0,  -3),   S(  8,  11),   S( -5, -16),   S(-17, -27),   S(-21, -46),   S(-11, -29),
            S( -7, -12),   S(-13, -27),   S( 11,  26),   S(  4,   1),   S(  2,  -6),   S(  5,  -4),   S(-21, -47),   S( -7, -22),
            S(-10, -35),   S( -7, -15),   S(-14, -27),   S(  0,  -2),   S( 12,  21),   S( -1, -12),   S( -7, -19),   S(-22, -37),

            /* queens: bucket 14 */
            S( -1,  -6),   S( 11,   8),   S(  1,  -5),   S(  8,  -1),   S(  0,   0),   S(  0,   0),   S(  6,  -1),   S(-11, -33),
            S(-10, -32),   S(-19, -51),   S( -5, -14),   S(  5,   0),   S(  0,   0),   S(  0,   0),   S( -1,  -1),   S( -9, -23),
            S(-17, -32),   S(-14, -42),   S(  0,  -6),   S( -7, -17),   S(  0,   3),   S(  6,  11),   S( -2, -12),   S(-15, -42),
            S( -5, -16),   S(-11, -27),   S(  6,   6),   S(-24, -46),   S(-15, -28),   S(-15, -43),   S( -3, -22),   S(  3,  -8),
            S(-10, -18),   S( 10,  12),   S(-12, -35),   S(  5,   8),   S( 19,  29),   S( -2, -18),   S(-10, -27),   S(  6,   4),
            S( -6, -14),   S(-13, -25),   S(-14, -25),   S(-23, -53),   S( 13,  23),   S( -5,  -5),   S(  3,   5),   S(-15, -17),
            S(-11, -22),   S( -9, -23),   S(  1,  -2),   S(  5,   7),   S( -4,  -1),   S(  7,   0),   S( -9, -20),   S( -4,  -8),
            S(-10, -26),   S( 15,   8),   S(-15, -24),   S( -6, -17),   S( -1,  -7),   S( -6, -16),   S( -8, -16),   S(  1,  -3),

            /* queens: bucket 15 */
            S( -5, -14),   S(-19, -37),   S(  0,  -5),   S(-11, -22),   S( 13,  22),   S( -8, -24),   S(  0,   0),   S(  0,   0),
            S( -3,  -9),   S( -5, -20),   S( -8, -18),   S(  0, -10),   S(  0,  -4),   S(  4,  11),   S(  0,   0),   S(  0,   0),
            S( -5, -13),   S(  2,  -2),   S( -8,  -4),   S(-14, -24),   S( -8, -24),   S(  3,   0),   S( -5, -15),   S( -6, -14),
            S( -3,  -7),   S(-16, -34),   S(  3,  -1),   S(  9,   9),   S(  0,   1),   S(  9,  14),   S(  4,   9),   S( -8, -21),
            S(  4,   5),   S(  3,   2),   S(-12, -29),   S(-10, -10),   S(  9,  33),   S( 17,  31),   S(-12, -20),   S( -8, -32),
            S( -1,  -9),   S(  0,  -3),   S( -4,  -6),   S(-19, -30),   S(  0,  11),   S(-12, -27),   S( -7, -17),   S(-16, -37),
            S(-12, -29),   S( -3, -11),   S( -5,   3),   S( -3, -10),   S(-15, -25),   S(  0,  -1),   S(  9,  22),   S( -7, -20),
            S( -6,  -8),   S(-21, -51),   S( -2, -16),   S( -3,  -2),   S( -5,  -9),   S( -3, -10),   S(  2,   7),   S(-10, -25),

            /* kings: bucket 0 */
            S(  5, -31),   S( 27, -13),   S( 17, -15),   S(-28,   9),   S(-22,  -1),   S( 25, -29),   S(  9,   6),   S( 24, -52),
            S(-37,  36),   S(  3,   9),   S( -7,  25),   S(-31,  11),   S(-38,  42),   S(-17,  23),   S(-17,  49),   S( -4,  22),
            S( -5,  15),   S( 91, -24),   S(  2,   4),   S(-12,  -1),   S(-33,   1),   S(  3,  -4),   S(-49,  29),   S( 15,  -6),
            S(-24, -29),   S(-18,  -5),   S(-11, -13),   S(-52,   6),   S(-60,  32),   S(-40,  16),   S(-54,  21),   S(-17,  24),
            S(-41, -84),   S( 22, -35),   S(  9, -43),   S(  6,  -7),   S(-23,  -2),   S(-22,  10),   S(  3,   2),   S(  4,  -9),
            S(  0, -103),  S( 19, -30),   S( 36, -45),   S(  9, -21),   S( 19, -28),   S( 20, -35),   S( 27,  -6),   S(-24, -20),
            S(  0,   0),   S(  0,   0),   S( 10, -21),   S( 17, -32),   S( 10,   4),   S( -7, -34),   S( -1, -25),   S(-25, -38),
            S(  0,   0),   S(  0,   0),   S(-23, -98),   S( 16, -24),   S(  9,  -4),   S( 18,   0),   S( 12,   7),   S(  3,   6),

            /* kings: bucket 1 */
            S( 13, -39),   S( 28, -12),   S(  5, -18),   S( 15,   1),   S(-18,   3),   S( 17, -11),   S(  7,  14),   S( 24, -30),
            S( 10, -12),   S( -3,  30),   S( 19, -12),   S(-41,  30),   S(-32,  29),   S( -1,   8),   S( -4,  28),   S(-16,  26),
            S(-26,  -5),   S(  5,  -3),   S( 12, -19),   S(  4, -14),   S(-36,  -1),   S( 10, -22),   S( -1,   4),   S( 57, -22),
            S( 22, -12),   S( 54, -23),   S( 27, -17),   S(-20,  15),   S( -5,  23),   S(-27,  12),   S(  3,   6),   S(-47,  26),
            S(-25, -40),   S( 10, -29),   S( 41, -41),   S(  4, -28),   S(  6, -16),   S( -7,  -9),   S( 22,  -4),   S( -7,  -6),
            S( 26,   9),   S( 19, -39),   S( 21, -11),   S( 45, -11),   S( -4, -21),   S( 17,  -3),   S( 39,   3),   S(-10,   9),
            S(  7, -25),   S( 15,  12),   S(  0,   0),   S(  0,   0),   S(-15,  14),   S( -8,   7),   S( 11,  46),   S(-14, -29),
            S(-19, -135),  S(-11, -23),   S(  0,   0),   S(  0,   0),   S( 10, -23),   S(  5,   3),   S(  1,  19),   S( -4, -46),

            /* kings: bucket 2 */
            S( 29, -63),   S( 19,  -8),   S( 17, -26),   S( 18, -11),   S(-11,   6),   S( 34, -25),   S(  5,  24),   S( 29, -21),
            S( 24, -13),   S(-15,  35),   S(  0,   5),   S( -1,   7),   S(-19,  16),   S(-11,   6),   S( 17,   4),   S( -8,  12),
            S(-13, -18),   S(-26,   6),   S( 10, -18),   S(-12, -15),   S( -6,  -6),   S(  5, -22),   S( 23, -14),   S( 33, -20),
            S( 13,  18),   S( -1,   0),   S( -1,   3),   S(-18,  13),   S( 47,  -8),   S(-21,  -1),   S( 38, -24),   S( 31, -15),
            S(-13, -23),   S(  4,   5),   S( -8, -21),   S(  8, -10),   S( 16, -32),   S(-28, -30),   S( 30, -35),   S(  9, -28),
            S( -4, -21),   S( 22,  -7),   S( 19, -29),   S( 51, -28),   S( 53, -20),   S( 21,  11),   S( 68, -34),   S( 40, -34),
            S(-16, -24),   S( -4,  19),   S(-22,  -7),   S( 21,  19),   S(  0,   0),   S(  0,   0),   S( 25,  25),   S(-12, -35),
            S( -6,   0),   S(-13, -33),   S( -2, -41),   S( -3,   2),   S(  0,   0),   S(  0,   0),   S( -6,  -7),   S(-16, -126),

            /* kings: bucket 3 */
            S( 10, -72),   S( 11,  -8),   S( 25, -43),   S( -4, -14),   S(-15, -21),   S( 35, -28),   S(  2,  17),   S(  7, -28),
            S(-17,  25),   S(-27,  39),   S(-12,   3),   S(-33,  18),   S(-47,  25),   S(  0,  -6),   S( -3,  14),   S(-13,  14),
            S( 43, -36),   S( 11,  -9),   S(  7, -18),   S(-18, -11),   S( -3,  -1),   S( 34, -32),   S( 44, -23),   S( 59, -28),
            S(-59,  29),   S(-103,  37),  S(-100,  19),  S(-93,  27),   S(-72,  17),   S(-71,   3),   S(-52,   1),   S(-82,   0),
            S(-27,  -8),   S(-19,  -1),   S(-79,   8),   S(-58,   6),   S( -5, -27),   S(  1, -45),   S(  0, -48),   S(-31, -55),
            S( -8,  -7),   S(  2, -11),   S( 15, -26),   S(-33, -31),   S( 35, -40),   S( 88, -64),   S(116, -51),   S( 41, -105),
            S(-50,  -1),   S( 27, -16),   S( -4, -34),   S( 14,  -9),   S(  4, -23),   S( 44, -51),   S(  0,   0),   S(  0,   0),
            S( -6, -29),   S(  9, -21),   S(  7, -12),   S(  2,   0),   S(  3, -90),   S(  6, -28),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-31, -30),   S(  2,  19),   S(-37,  38),   S( 22, -13),   S(  1,  -7),   S(-18,  11),   S( 17,  15),   S( 46, -38),
            S(-49,  40),   S( 27,  15),   S(-13,  10),   S(-27,  10),   S( 48, -12),   S( -3,   3),   S( 34, -12),   S(  9,  14),
            S( 19,  19),   S(-30,  -8),   S(-10,  10),   S(  7, -15),   S( 14,  -2),   S(  0, -13),   S(-25,  -9),   S(-11,  -2),
            S( -1, -12),   S( 13,   0),   S( 26,   4),   S(  2,  15),   S(  8,  -5),   S(-17,  18),   S(  1,  20),   S(  2,   7),
            S(  0,   0),   S(  0,   0),   S( 11,  -8),   S(-17,   7),   S( -9,  -6),   S(-36,   2),   S(-28,  -7),   S( -3,  11),
            S(  0,   0),   S(  0,   0),   S( -3,  -9),   S( 10,   8),   S( 19,   8),   S( -4,  -4),   S( 18,  -3),   S(  5,  -4),
            S( -4, -24),   S(  8,  44),   S(  8, -10),   S( -2, -13),   S( 12,  16),   S(-10,  -5),   S( 10,  -4),   S(  0, -14),
            S(  3,  43),   S( -6, -20),   S( -3,  -6),   S(  4,  14),   S( -2, -27),   S( 10,  29),   S(  9,   5),   S(  3,  -3),

            /* kings: bucket 5 */
            S( 30, -11),   S(-22,  29),   S(-35,  21),   S(-81,  36),   S(-32,  25),   S( -5,   9),   S( 58,  -3),   S( 42,  -5),
            S(  8,  -5),   S( 56, -10),   S( 33,  -6),   S( 52, -10),   S( 17,  -1),   S( 39, -13),   S( 49,   2),   S( 54, -14),
            S(-16, -11),   S(-12,   1),   S(  3, -12),   S(-31,  -2),   S(-28,   5),   S(-55,   4),   S(-18,  11),   S( 25, -11),
            S(-25,  12),   S( 27, -16),   S( 26, -14),   S(  3,  25),   S( 17,  16),   S( 33,  -4),   S( 37,   0),   S( 14,  -1),
            S( -6, -10),   S(-13, -30),   S(  0,   0),   S(  0,   0),   S(-12,   2),   S(-16,  -8),   S( 20,   0),   S(-30,   5),
            S(-36, -15),   S(-21,   7),   S(  0,   0),   S(  0,   0),   S( -7,   3),   S(-17,   9),   S(-34,   8),   S(-14,  -4),
            S( -2,  13),   S( -6,   9),   S(  0,  34),   S( -3,   0),   S( -9,   4),   S(-10,   7),   S( 17,  32),   S( 12,  12),
            S(-18, -24),   S(  6,  36),   S( -2,  42),   S( -2,  -7),   S( -3,  27),   S( -8,   1),   S( -7,   1),   S( -6,  15),

            /* kings: bucket 6 */
            S( 20, -22),   S( 20,  -9),   S( -3,   0),   S(  9,   7),   S( -8,  12),   S(-39,  25),   S( 18,  15),   S( 43,  -9),
            S( 65, -36),   S( 24,  20),   S( 13,   0),   S( 36, -17),   S( 31,  -4),   S(-11,  10),   S( 26,   7),   S( 11,   7),
            S(-12, -19),   S(-30,  12),   S(-20,  -4),   S(-18,  -6),   S(-14,  -3),   S(-53,   1),   S(  8,   1),   S(-45,  19),
            S( 35,   1),   S(  2,   8),   S( 28, -16),   S( 49,   2),   S( 74,   2),   S( -5,  -1),   S( 48, -12),   S( 14,  -5),
            S( -5,  -6),   S(-30,   0),   S( 23, -22),   S( 17,  -4),   S(  0,   0),   S(  0,   0),   S( -8, -15),   S(-68, -12),
            S(-26,   8),   S( -4,   9),   S(-16, -13),   S(-10,  -5),   S(  0,   0),   S(  0,   0),   S(-32,  30),   S(-40, -14),
            S(  7, -29),   S(  9,   8),   S(  8,  10),   S( -1,  19),   S(  5, -11),   S( -7,  -3),   S( -6,  -6),   S(-28, -17),
            S(  2,  23),   S(  2,  26),   S( -3,   3),   S( -3,  10),   S(  3,  10),   S(  0,  14),   S(  5,  41),   S( -1,  37),

            /* kings: bucket 7 */
            S( 31, -39),   S(  3,  -4),   S(  9, -22),   S( -6,   9),   S(-43,  12),   S(-46,  36),   S(-12,  36),   S(-15,  12),
            S( 25,  -8),   S( 49, -21),   S(-20,   6),   S(  2,  -5),   S( -8,   3),   S(-27,  21),   S( 12,   3),   S( 13,  12),
            S( 41, -14),   S( -4,  -5),   S(-37,   5),   S(-11,  -8),   S(-33,  -4),   S(-51,  15),   S( 11,  -1),   S(-73,  28),
            S(-34,  16),   S( 31, -12),   S( -6,  -6),   S( -4,  11),   S(-14,  14),   S( 76, -31),   S( 36, -17),   S( 31, -13),
            S(-14,  -1),   S(-10,  12),   S(-21, -11),   S(-30,   5),   S(-10, -21),   S( 16, -23),   S(  0,   0),   S(  0,   0),
            S(-11, -29),   S( 19,  -9),   S( 30,  -9),   S(  2, -12),   S( 18, -14),   S( 12,  20),   S(  0,   0),   S(  0,   0),
            S( 14,  15),   S( 23,   1),   S( 19, -10),   S(  7, -12),   S( 19, -25),   S( -1,   6),   S( 11,  20),   S( -2, -16),
            S(  4,   1),   S( -2, -20),   S( 20,  26),   S(  9,   1),   S(  0, -18),   S( -6, -27),   S( -4,  11),   S(-19, -49),

            /* kings: bucket 8 */
            S(-19, 110),   S(-36,  62),   S(-27,  47),   S(-20,  -4),   S(-13,  18),   S(-12,  -2),   S( 30,  -2),   S(  7,   2),
            S( 24,  84),   S( 31,  -6),   S(  8,  50),   S(  9,   7),   S( -3,  18),   S( 19,  10),   S( 10,   4),   S(  4,  23),
            S(  0,   0),   S(  0,   0),   S( 21,  29),   S(  8,   7),   S( 30,  -5),   S( 24, -15),   S( 12,  -6),   S(-15, -13),
            S(  0,   0),   S(  0,   0),   S( 15,  33),   S( 33, -18),   S( 13,  19),   S( -3, -10),   S( 14,   2),   S(  5,  43),
            S(  0, -13),   S(  5,   8),   S(  1, -32),   S(  4,   2),   S(  0, -12),   S( 14,  -7),   S(  5,  17),   S(-19, -48),
            S( -2,  11),   S( -4,  -5),   S( -1, -10),   S( -4, -17),   S(-24, -44),   S(  4,  -9),   S(-10,  -2),   S(  8,  -1),
            S( -6, -15),   S( -6, -22),   S(  5, -12),   S( -7, -43),   S(  2, -29),   S( -3, -20),   S(  4,  -6),   S(  9, -40),
            S( -3, -14),   S( -7, -25),   S(  8,  18),   S( -2,  -1),   S(  7,  44),   S(  2,  -6),   S(  6,  12),   S(  3, -11),

            /* kings: bucket 9 */
            S(-37,  52),   S(-28,  35),   S(-72,  62),   S(-65,  32),   S(-63,  44),   S(-45,  22),   S( 50,  10),   S( 32,  17),
            S(-29,  16),   S( 32,  14),   S( -3,  -7),   S( 30,  13),   S( 22,  19),   S( 16,   3),   S( 28,  14),   S( 48,  -2),
            S( -8,   6),   S(  7,  12),   S(  0,   0),   S(  0,   0),   S(  2,  21),   S(-48,   8),   S( 36,  -7),   S(-10,   7),
            S(  1,  -9),   S( -1, -10),   S(  0,   0),   S(  0,   0),   S(  8,  19),   S( 26,  -3),   S(  1,   7),   S( -9,  24),
            S( -8,  -2),   S(  4,   9),   S( -3,  -7),   S(  4,  -3),   S( 15, -15),   S(  7,   3),   S( -6,  13),   S( -6,   6),
            S(  4,  34),   S( -1,  17),   S( -1,  17),   S(  1,  -7),   S(-12,   7),   S(-16,  23),   S( -8, -18),   S( -4,  40),
            S(  7,   0),   S( -7, -30),   S(  1, -14),   S(  8,  17),   S( 13,  40),   S( 25,  28),   S( -8, -21),   S( -6, -11),
            S(  5,  32),   S(  1,  23),   S(  9,   1),   S(-15, -49),   S( -9, -50),   S(  0, -13),   S(  5, -18),   S(  6,  22),

            /* kings: bucket 10 */
            S( -4,  25),   S(-29,  24),   S( -3,  21),   S(-27,  24),   S(-60,  27),   S(-106,  39),  S(  4,  30),   S(-98,  93),
            S( 13, -12),   S( 26,  16),   S( 19, -18),   S( -3,  22),   S( 55,  12),   S( 59,   4),   S( 16,  23),   S(-41,  37),
            S( -6,   9),   S(  8,   7),   S(  6,  -8),   S( -9,   8),   S(  0,   0),   S(  0,   0),   S(  0,  11),   S(-50,  21),
            S( 11,   4),   S( 17, -21),   S( 22,  -2),   S( 35,  -2),   S(  0,   0),   S(  0,   0),   S( -5,   7),   S( 23,   0),
            S(  6,   9),   S( 14,  21),   S( 13,  -3),   S( 15, -33),   S(  5,  -9),   S(  6,   0),   S(-10,   5),   S(-19,  10),
            S( -7,  30),   S( -7,  12),   S(-21,  15),   S(  1,   4),   S(  0,  23),   S(-11, -13),   S(-17,  19),   S( -4,  20),
            S(  1, -20),   S( -8,  -8),   S( 16,   9),   S( 11,  15),   S( 10,   2),   S( -7,  -9),   S(  9, -14),   S( 10,  38),
            S(  0,  12),   S(  2, -24),   S(  0, -12),   S(  2,   8),   S(  0,   4),   S( -3, -23),   S(  3, -27),   S(  4,  30),

            /* kings: bucket 11 */
            S(-19,  29),   S(  3,  18),   S(  2,  -3),   S(-12,  10),   S(-38,   5),   S(-154,  76),  S(-37,  68),   S(-155, 150),
            S( 15, -31),   S(-10,  16),   S(-19, -31),   S(-16,  22),   S( 59,  -3),   S( -6,  49),   S( 55,  -2),   S( 21,  35),
            S(-15, -22),   S( 20,   0),   S(-19,  -4),   S( 10,  -1),   S( 63,  -4),   S( 26,  21),   S(  0,   0),   S(  0,   0),
            S( -1,  19),   S(  2,   7),   S( 28,   2),   S( 39,  -7),   S( 31, -10),   S( 28,   5),   S(  0,   0),   S(  0,   0),
            S(  3,  28),   S( -3,  -8),   S( -9,  -7),   S( 25, -16),   S( 31,  -8),   S( 11, -12),   S(  8,  -9),   S( 11,  14),
            S( 14,  19),   S(  2,  -3),   S(  0,  -2),   S(  8, -15),   S(  7, -20),   S(  1, -14),   S(-17,   3),   S( -2,  -8),
            S(  7, -10),   S(  6,  -1),   S( 11,  21),   S( -2, -23),   S(  9,   5),   S( -4, -15),   S( 12,   4),   S( -2,   4),
            S(  4,   0),   S( -6, -43),   S( -9, -23),   S(  7,  -2),   S( -2, -22),   S( -7, -19),   S(  2, -20),   S(  5,  22),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,  66),   S(-12,  22),   S( -4,  -8),   S( 11,  13),   S( 15, -11),   S(  8,  53),
            S(  0,   0),   S(  0,   0),   S( 19,  91),   S(  2,  -2),   S(  5,  13),   S( 26,  17),   S(  9,  14),   S(-11,  11),
            S( -1,   4),   S( -3, -46),   S( 10,  17),   S(  6,  18),   S(  4,  -7),   S( -7,   4),   S( -4,   7),   S(-14,  -1),
            S( -1,  -6),   S(  4,   4),   S( -9, -33),   S( 13, -37),   S(-16, -19),   S( -1,  -5),   S(-19,   7),   S( -3,  -9),
            S(  9,  32),   S( 10,  26),   S(  6,  20),   S( -3, -35),   S( -9,   2),   S( -1,  10),   S(-11,  20),   S(  1,   8),
            S(  3,  19),   S(  0,  -2),   S( -3,  -2),   S( -6,  -7),   S( 14,  25),   S(  1,   3),   S( -8,  11),   S(  7,  15),
            S(  4,   6),   S(-10, -26),   S(  7,  24),   S( -7, -14),   S( -2,  -7),   S( -5,   7),   S( 14,  24),   S( -5, -18),
            S(  1,   4),   S(  5,  16),   S(  2, -12),   S( -3, -19),   S(  2,  -8),   S(  5,   2),   S( -7, -28),   S(-15, -44),

            /* kings: bucket 13 */
            S(-12,  91),   S( -8,  49),   S(  0,   0),   S(  0,   0),   S(  1,  73),   S(-18,  -1),   S( 10,   2),   S( -8,  46),
            S(  6,  33),   S( -6,   1),   S(  0,   0),   S(  0,   0),   S( 22,  -9),   S(  2, -33),   S(-29,   5),   S(  8,   5),
            S( -5,   3),   S( -1,  13),   S( -4, -32),   S(  5,  12),   S( 12,  17),   S(-11,  13),   S(-27,   5),   S( -5,  12),
            S( -7, -27),   S(  5,  27),   S(  2,   0),   S(  3, -49),   S( -3, -38),   S(  1, -36),   S( -6,  12),   S(-16, -33),
            S(  8,  15),   S( -9,  -4),   S(  8,  32),   S( -7,  -9),   S(-21, -40),   S( -2,  -7),   S(-14,  -8),   S(  6,  26),
            S(  0,   0),   S( -2,  28),   S(  3,  25),   S( -6,  -3),   S( -3,   7),   S( -3,  20),   S( -7, -11),   S(  7,  21),
            S(  8,  22),   S( -7, -13),   S(-15, -41),   S(  1,   2),   S( -1, -31),   S(  8,  16),   S( -3, -38),   S(  5,  23),
            S(  0,  -6),   S(  0,  19),   S( 11,  51),   S(  4, -11),   S( -3,   3),   S(-13, -12),   S( -1,  -9),   S(  5,  19),

            /* kings: bucket 14 */
            S(  4,  46),   S( -2,  -2),   S(-13, -13),   S(  3,  15),   S(  0,   0),   S(  0,   0),   S(  5,  90),   S(-52,  82),
            S(-33, -18),   S(  2,   7),   S(  7,   4),   S( 11,  -6),   S(  0,   0),   S(  0,   0),   S( 13,  24),   S(-22,  -5),
            S(-10,  -2),   S(  4,  -6),   S( 21,  -1),   S( 14,  16),   S(  7, -10),   S( -7,  14),   S(  6,  39),   S(-28,  -1),
            S( -3, -14),   S(  0,   9),   S( -2,  -2),   S( -2, -51),   S(-14, -58),   S( 13,  43),   S( -3,  22),   S( 14,   3),
            S(  0,  10),   S(  1,  -2),   S(-15, -12),   S(-12, -24),   S(  1,  38),   S(  5,  21),   S( 10,  56),   S( 18,  32),
            S( -6, -11),   S( -3,  14),   S(  0,  -3),   S( -2,  33),   S( -5, -20),   S(  0, -14),   S(-21, -35),   S( -3,  -5),
            S(  3,  -4),   S( -7, -33),   S(  2,  15),   S(  3,   3),   S( -9, -29),   S(  2, -12),   S(-18, -85),   S(  3,  40),
            S( -3,  -2),   S(  7,  45),   S(  4,   1),   S(  0,   6),   S(  4,  55),   S(  3, -15),   S(-11, -50),   S(  1,  -6),

            /* kings: bucket 15 */
            S(  7,  51),   S( 10,  12),   S(  7,   4),   S(-14, -15),   S(-23,  -2),   S(-33,  72),   S(  0,   0),   S(  0,   0),
            S(  5,  -1),   S(-11, -12),   S( -1, -27),   S( 13,  43),   S( 31, -11),   S( 33,  87),   S(  0,   0),   S(  0,   0),
            S(-23,   9),   S(  6,   4),   S( -1, -19),   S( -5, -17),   S(  6, -32),   S( 21,  38),   S( 16,  23),   S( -5, -22),
            S( -2,  -8),   S( -4,  11),   S( -1,   4),   S( -8, -30),   S(  0, -52),   S(  3,  -7),   S(  3,  45),   S(  1, -14),
            S( 10,  25),   S(-12,  20),   S( -2,  38),   S(  0, -26),   S( -3,  -6),   S( -4,   5),   S(-11, -15),   S(-10, -15),
            S( -7,  11),   S(-14, -11),   S( -6,   2),   S( 10,  13),   S( -4,  17),   S(-14, -42),   S( -4,   7),   S(  8,  14),
            S(  7,  41),   S(  2,  18),   S( -4,  -7),   S( -3,  18),   S( 10,  18),   S(  8,  24),   S( -8, -24),   S(  7,  30),
            S( -4,  -5),   S(  8,  12),   S( -1,  -7),   S(  1,   2),   S(  2,  37),   S(  4,  28),   S( -6, -14),   S(  7,   9),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S( -4, -185),  S( 17, -28),   S( 27,  41),   S( 35,  76),   S( 42,  95),   S( 47, 114),   S( 53, 112),   S( 65,  99),
            S( 89,  66),

            /* bishop mobility */
            S( 10, -72),   S( 14,   3),   S( 24,  39),   S( 30,  73),   S( 35,  95),   S( 36, 113),   S( 40, 119),   S( 46, 116),
            S( 49, 119),   S( 60, 108),   S( 61, 100),   S( 93,  83),   S(108,  74),   S(152,  49),

            /* rook mobility */
            S(-10,  -9),   S( -3,  51),   S( -4,  85),   S(  0,  98),   S( -3, 121),   S(  2, 130),   S(  4, 140),   S(  8, 141),
            S(  8, 151),   S( 10, 156),   S( 16, 157),   S( 25, 155),   S( 34, 153),   S( 48, 150),   S(148, 103),

            /* queen mobility */
            S( 61, -16),   S( 69, -87),   S( 62,  27),   S( 56, 115),   S( 57, 150),   S( 57, 201),   S( 57, 232),   S( 57, 256),
            S( 60, 273),   S( 59, 291),   S( 58, 310),   S( 55, 329),   S( 57, 335),   S( 55, 345),   S( 51, 358),   S( 59, 356),
            S( 57, 355),   S( 72, 343),   S( 86, 329),   S(102, 311),   S(137, 284),   S(199, 238),   S(166, 251),   S(200, 222),
            S(204, 225),   S(158, 211),   S(113, 202),   S( 84, 173),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  3,  31),   S(-25,  60),   S(-31,  58),   S(-35,  80),   S(  9,  29),   S(-17,  42),   S( -3,  77),   S( 36,  33),
            S( 15,  40),   S( -4,  66),   S(-17,  56),   S(-13,  48),   S( -2,  45),   S(-38,  66),   S(-40,  88),   S( 37,  37),
            S( 31,  69),   S( 22,  75),   S(  9,  62),   S( 29,  45),   S(  2,  51),   S(-24,  72),   S(-23, 102),   S(  8,  68),
            S( 55,  96),   S( 65, 109),   S( 41,  72),   S( 18,  56),   S( 13,  66),   S( 15,  87),   S(-17, 113),   S(-58, 133),
            S( 70, 134),   S(108, 151),   S(107, 111),   S( 49, 101),   S(-40,  94),   S( 41,  92),   S(-27, 159),   S(-37, 135),
            S(163, 179),   S(148, 212),   S(193, 191),   S(208, 195),   S(183, 204),   S(193, 194),   S(185, 187),   S(202, 210),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 16,  15),   S(  5,  -3),   S( 11,   7),   S( 11,  27),   S( 30,  34),   S(  0, -19),   S(-13,  34),   S( 11, -31),
            S(-10,  35),   S( 30,   7),   S(  1,  42),   S( 17,  44),   S( 41,  -1),   S( -6,  40),   S( 21,  -1),   S(  4,  12),
            S(-12,  20),   S( 17,  16),   S( -3,  48),   S( 17,  67),   S( 25,  25),   S( 25,  28),   S( 37,   1),   S( -8,  22),
            S( 12,  17),   S( 29,  60),   S( 27,  69),   S( 17, 102),   S( 72,  53),   S( 77,  57),   S( 13,  43),   S( 30,  25),
            S( 71,  55),   S(119,  93),   S( 82, 135),   S(111, 168),   S(160, 135),   S(137, 107),   S(174,  92),   S(119,  48),
            S( 80, 185),   S(119, 265),   S( 89, 219),   S( 82, 190),   S( 58, 133),   S( 54, 138),   S( 46, 170),   S( 17, 108),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 18,  44),   S( 27,  33),   S( 43,  52),   S( 41,  36),   S( 32,  35),   S( 29,  41),   S(  6,  25),   S( 48,  16),
            S( -3,  25),   S( 12,  48),   S( 14,  39),   S( 10,  48),   S( 27,  16),   S(  8,  26),   S( 25,  25),   S( -4,  16),
            S(  1,   7),   S( 23,  30),   S( 50,  40),   S( 43,  31),   S( 42,  38),   S( 65,   8),   S( 22,  21),   S( 23,   0),
            S( 65,  29),   S(120,  13),   S(131,  79),   S(164,  84),   S(163,  76),   S(103,  80),   S( 87,  21),   S( 81,  -9),
            S(111,  50),   S(119,  72),   S(155, 164),   S(169, 160),   S(180, 156),   S(126, 157),   S(195, 117),   S( -8, 114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  24),   S(  0,  22),   S(  3,  36),   S( -2,  56),   S( 67,  33),   S( 23,  13),   S( -7,   2),   S( 23,  14),
            S(  1,  14),   S(  8,   6),   S( 17,  17),   S( 17,  29),   S( 14,  15),   S( -1,   9),   S(  5,   6),   S( 28,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -14),   S( -8,  -6),   S(-17, -17),   S(-17, -29),   S(-14, -15),   S(  1,  -9),   S( -5,  -6),   S(-28,   6),
            S(-25, -24),   S(  0, -22),   S( -3, -36),   S(  2, -56),   S(-67, -33),   S(-23, -13),   S(  7,  -2),   S(-23, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -22),   S( -5, -14),   S(  0, -31),   S(-62,  -7),   S(-20, -12),   S(-21, -22),   S(  7, -27),   S(-17, -36),
            S(-21,   6),   S(-11,  -6),   S(-26,  10),   S(  4, -16),   S(-31, -13),   S(-29,   1),   S(-43,   7),   S(-12, -13),
            S(-19, -15),   S( -7, -20),   S(-24,  -2),   S(-30, -11),   S(-25, -30),   S(-25, -12),   S(-15,  -7),   S(-45, -17),
            S( -2, -34),   S( 23, -43),   S( 15, -21),   S( 11, -31),   S(  6, -23),   S( 58, -35),   S( 42, -34),   S(-13, -43),
            S( 19, -56),   S( 50, -66),   S( 60, -41),   S( 74, -37),   S( 59, -33),   S( 86, -29),   S(115, -60),   S( 22, -62),
            S(109, -98),   S(167, -121),  S(119, -62),   S(124, -55),   S(104, -42),   S(119, -15),   S(144, -37),   S( 86, -86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 17,   3),        // attacks to squares 1 from king
            S( 17,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 69, -87),        // king-side castling available
            S( 15,  59),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 10, -51),   S( 11, -44),   S(  1, -50),   S(-12, -34),   S(-18, -28),   S(-37, -17),   S(-71,   4),   S(-77,   6),
            S(-91,  10),   S(-91,  18),   S(-80,  24),   S( 13,   2),   S(-27,  12),

            /* orthogonal lines */
            S(-13, -56),   S(-57, -30),   S(-81, -18),   S(-97, -16),   S(-103, -22),  S(-108, -25),  S(-108, -31),  S(-104, -34),
            S(-122, -25),  S(-143, -24),  S(-160, -31),  S(-163, -39),  S(-182, -27),  S(-361,  30),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S(  7, 214),

            /* passed pawn can advance */
            S(-11,  37),   S( -8,  63),   S(  1, 107),   S( 12, 210),

            /* blocked passed pawn */
            S(  0,   0),   S( 62,  -7),   S( 37, -14),   S( 35,  25),   S( 47,  39),   S( 60,  -5),   S(164, -21),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 19,  -7),   S( 13,  49),   S( 16,  42),   S( 15,  75),   S( 36,  91),   S(129, 113),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S( -7, -24),   S(-11, -29),   S(  9, -55),   S(-23, -38),   S(  2, -47),   S(185, -95),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 29, -29),   S( 42, -65),   S( 12, -20),   S( 13, -47),   S(  0, -145),  S(-19, -263),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S(  8,  58),   S( 36,  42),   S(101,  42),   S(  1,  40),   S(193,  84),   S(184,  60),   S(  0,   0),       // blocked by King

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-26,  81),

            /* bad bishop pawn */
            S( -5, -22),

            /* rook on open file */
            S( 43,  -9),

            /* rook on half-open file */
            S( 14,  22),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 26,  36),   S( 28,  -8),   S( 34,  23),   S( 26,   1),   S( 43, -20),

            /* pawn threats */
            S(  0,   0),   S( 69, 103),   S( 55, 117),   S( 64,  97),   S( 62,  46),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 35,  55),   S( 51,  41),   S( 78,  31),   S( 49,  51),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 22,  53),   S( 30,  49),   S(-14,  45),   S( 65,  62),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 16,  13),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
