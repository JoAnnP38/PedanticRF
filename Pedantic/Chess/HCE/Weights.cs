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
        public const int MAX_WEIGHTS = 12786;
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
        public const int ROOK_BEHIND_PASSER = 12756;
        public const int BISHOP_PAIR = 12757;       // bonus for having both bishops
        public const int BAD_BISHOP_PAWN = 12758;   // bad bishop pawn
        public const int ROOK_ON_OPEN_FILE = 12759; // rook on open file
        public const int ROOK_ON_HALF_OPEN_FILE = 12760;    // rook on half-open file
        public const int PAWN_PUSH_THREAT = 12761;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12767;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12773;      // minor piece threat
        public const int ROOK_THREAT = 12779;       // rook threat
        public const int TEMPO = 12785;             // tempo bonus for side moving

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

        public Score RookBehindPassedPawn
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[ROOK_BEHIND_PASSER];
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

        // Solution sample size: 6000010, generated on Mon, 11 Mar 2024 21:35:39 GMT
        // Solution K: 0.003850, error: 0.084920, accuracy: 0.5007
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S(104, 179),   S(441, 540),   S(451, 572),   S(578, 962),   S(1386, 1645), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 93, -111),  S(147, -98),   S( 27, -30),   S(-22,  20),   S(-34,  20),   S(-22,  -5),   S(-54,  -7),   S(-41, -22),
            S( 98, -106),  S(101, -102),  S( -4, -49),   S(-15, -46),   S(-26, -20),   S(-28, -34),   S(-42, -26),   S(-39, -39),
            S( 85, -77),   S( 75, -66),   S( 18, -51),   S( 16, -71),   S(-10, -46),   S(  1, -52),   S(-29, -42),   S(-20, -40),
            S( 50, -19),   S( 55, -48),   S( 19, -40),   S( 14, -66),   S(-15, -49),   S(-36, -45),   S(-26, -42),   S(-31, -18),
            S( 38,  59),   S( 41,   3),   S( 34,  -9),   S( 33, -61),   S( 10, -28),   S(-26, -26),   S(-36,  -9),   S(-51,  43),
            S( 96,  53),   S( 68,  87),   S( -9, -39),   S( 49, -43),   S(-55, -26),   S( 22,  14),   S(  2, -13),   S( -2,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 34, -32),   S( 23, -34),   S( 48, -27),   S( -4,   8),   S(-22,  -6),   S( -1, -13),   S(-46,  -2),   S(-40,  20),
            S( 23, -35),   S(  7, -40),   S( 10, -46),   S( -4, -41),   S(-12, -24),   S(-12, -29),   S(-41, -15),   S(-46,  -8),
            S( 19, -31),   S( 19, -30),   S( 21, -46),   S( 21, -53),   S(-19, -24),   S(  7, -37),   S(-21, -23),   S(-22,  -6),
            S( 25,   4),   S( 17, -32),   S( 12, -29),   S(  7, -43),   S( -9, -20),   S( -1, -31),   S(-33, -20),   S(-31,  19),
            S( 20,  49),   S(-66,  15),   S( -5, -16),   S(  4, -28),   S( 20, -15),   S(-40,  12),   S(-48,  24),   S(-48,  67),
            S( 39,  51),   S( -8,   7),   S(-20, -28),   S(-29,  16),   S(-29, -25),   S(-73,   3),   S(-33,  -9),   S(-51,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26,   3),   S(-37,   7),   S(-17,   4),   S(-21,  22),   S(  5,  -7),   S( 29, -19),   S( -1, -44),   S(-15, -12),
            S(-27, -12),   S(-49,  -7),   S(-26, -33),   S(-23, -35),   S(  2, -28),   S(  1, -26),   S(-13, -37),   S(-28, -24),
            S(-35,  -5),   S(-34, -16),   S(-13, -42),   S( -5, -50),   S( -3, -25),   S( 14, -26),   S(-15, -27),   S(-13, -15),
            S(-42,  19),   S(-40, -17),   S(-33, -24),   S(-12, -39),   S(  8, -39),   S(-21, -10),   S(-12,  -8),   S(-22,  14),
            S(-40,  51),   S(-86,  17),   S(-66, -13),   S(-60, -15),   S( 18,   1),   S(-33,  12),   S(-14,  13),   S(-46,  87),
            S(-53,  70),   S(-92,  69),   S(-98,   4),   S(-77, -26),   S(-18, -15),   S(-14,  25),   S(-35,   3),   S(-45,  71),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28,  -8),   S(-44,   3),   S(-31,  -7),   S(-14, -22),   S(-15,  -3),   S( 38, -17),   S( 82, -65),   S( 55, -74),
            S(-36, -24),   S(-49, -24),   S(-32, -40),   S(-25, -28),   S(-20, -22),   S(  5, -34),   S( 57, -65),   S( 46, -67),
            S(-37, -24),   S(-20, -47),   S( -7, -61),   S( -4, -65),   S( -9, -46),   S( 16, -41),   S( 23, -47),   S( 49, -49),
            S(-37,  -3),   S(-26, -43),   S(-18, -54),   S( -1, -59),   S( 18, -62),   S(  1, -43),   S( 18, -28),   S( 44, -10),
            S(-17,  29),   S(-44, -15),   S(-12, -48),   S(-12, -53),   S( 84, -61),   S( 57, -33),   S( 45,  19),   S( 31,  70),
            S(-50, 111),   S(-51,  40),   S(  0, -49),   S(-39, -52),   S( 16, -50),   S( 94, -49),   S( 47,  12),   S( 48,  71),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  20),   S(  2, -12),   S(-31,  11),   S( -2,   5),   S(  3, -39),   S(-21,   5),   S(-60,  -2),   S(-50,  -1),
            S(-37,  15),   S( 33,  -8),   S( 46, -43),   S( 33, -27),   S(-15, -21),   S(-47, -20),   S(-28, -34),   S(-15, -26),
            S( 30, -12),   S( 47, -14),   S(-12,  10),   S(  9, -35),   S(-27, -19),   S(-15, -35),   S(-41, -30),   S(-34, -13),
            S( 14,  37),   S(  0,  41),   S( 56,   0),   S( 42,  -2),   S( 25, -38),   S(-50, -11),   S( -4, -37),   S( 31, -24),
            S(-11,  75),   S( 28,  62),   S(  6,  19),   S(-39,  16),   S( 13, -10),   S(-25,  -2),   S(-39, -28),   S( 30,  33),
            S( 60, 102),   S( 62,  83),   S(  1,   6),   S(  8,   7),   S( 18, -39),   S(  6, -13),   S( 17, -13),   S(-34,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-59,  40),   S(-50,  29),   S(-16,  22),   S( 37,  -6),   S( -9,  17),   S(-17,  13),   S(-33,   3),   S(-13,  17),
            S(-46,  22),   S(-66,  18),   S( 38, -11),   S(-10,  19),   S( 14,  -5),   S(-30, -11),   S(-23, -13),   S(-41,  14),
            S(-49,  42),   S(-45,  24),   S( 83, -31),   S( 12, -18),   S( 37,  -9),   S(-39,  -3),   S( -1,  -6),   S(-21,  11),
            S(-62,  72),   S( -3,  30),   S( 25,   8),   S( 68,  11),   S( -8,  13),   S(-53,  19),   S(  8,  -6),   S( 31,  14),
            S( 78,  52),   S(103,  14),   S( 57,  55),   S( 43,  22),   S(-69,  69),   S( 61,   1),   S( 40,  -1),   S( -9,  56),
            S( 80,  36),   S( 73,  38),   S( 65,  27),   S( 36,  31),   S( 58, -10),   S( -2,  -5),   S( 28,  19),   S( 27,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34,  24),   S(-22,   5),   S(-18,  10),   S(-10,  11),   S( 15, -13),   S(-26,   7),   S(-54,   3),   S(-58,  19),
            S(-30,   1),   S( -5, -26),   S( -9, -33),   S( -2, -10),   S( 62, -25),   S( 30, -19),   S(-16, -14),   S(-70,  10),
            S(-27,   9),   S(  5, -15),   S( 16, -21),   S(-18,  -8),   S( 39, -11),   S( 61, -27),   S( -2, -13),   S(-39,  20),
            S(-11,  23),   S(-48,  15),   S( 25, -14),   S(  8,   0),   S( 29,   5),   S( 35,   2),   S( 40,  -8),   S( 24,  17),
            S(-58,  58),   S(-33,  14),   S( -3,  -3),   S( 11, -11),   S( 38,  37),   S(100,  26),   S( 57,  -5),   S( 64,  36),
            S( 67,  37),   S( 23,  17),   S(  5, -16),   S( 51, -71),   S( 49,  -4),   S( 47,   0),   S( 22,   4),   S( 91,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -18),   S(-70,  -2),   S(-27,  -3),   S(-70, -11),   S(-25, -17),   S( 36, -30),   S(-12, -38),   S(-48, -14),
            S(-59, -26),   S(-58, -33),   S(-42, -36),   S(-13, -48),   S(  3, -32),   S( 44, -46),   S( 42, -40),   S(-14, -28),
            S(-54, -25),   S(-56, -33),   S(-38, -36),   S(  0, -46),   S( -2, -24),   S( 28, -24),   S( 33, -37),   S( 33, -29),
            S(-25, -19),   S(-69, -26),   S(-57, -42),   S(-47, -11),   S( 16, -28),   S( 12,  -5),   S( 35,  -5),   S( 68, -17),
            S(-37, -10),   S(-19, -37),   S(-66, -21),   S( 11, -54),   S( 55,  -3),   S( 26,   0),   S( 53,  45),   S(105,  29),
            S(  3, -15),   S(-33, -25),   S( 12, -57),   S( 32, -65),   S( 21, -43),   S( 30, -18),   S( 55,  63),   S( 72,  84),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39,  30),   S(-21,  29),   S(-16,  31),   S(-18, -29),   S( -6,  18),   S(-63,  17),   S(-71,  13),   S(-68,  19),
            S(-50,  23),   S(-42,  12),   S(-40,  24),   S(-34,   0),   S(-54, -13),   S(-20, -19),   S(-27, -28),   S(-14,  -6),
            S(-12,  67),   S( -3,  72),   S( -6,  44),   S(-20,  33),   S(-18, -10),   S(-68, -21),   S( -8, -47),   S( -8, -26),
            S( 19, 100),   S( 54, 113),   S( 82,  70),   S( 23,  42),   S(-30,  -5),   S(-23, -11),   S(-10, -24),   S( 12, -29),
            S( 69, 117),   S( 66, 118),   S( 75, 122),   S( 64, 112),   S( 15,  -2),   S(  6,   1),   S( -5, -40),   S( -8,   4),
            S(105, 144),   S( 98, 156),   S( 92, 182),   S( 49,  56),   S(  4,  22),   S( -3, -11),   S(-10, -55),   S( -3, -23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52,  55),   S(-53,  35),   S(  6,  16),   S( 18,  14),   S( -9, -15),   S(-55,  20),   S(-67,   4),   S(-59,  27),
            S(-35,  21),   S(-37,   6),   S(-68,  18),   S( -2,  41),   S(-65,   4),   S(-51,   1),   S(-106,  -2),  S(-39,  13),
            S(-49,  45),   S( -7,  56),   S(-27,  69),   S(-60,  66),   S(-49,  44),   S(-73,  10),   S(-55, -15),   S( -6,  -1),
            S(  6,  74),   S( 72,  73),   S( 60,  95),   S( 48, 121),   S(-52,  70),   S(-19,  19),   S( 27,   0),   S( 30,  -8),
            S( 93,  78),   S( 93, 109),   S( 85, 123),   S( 98, 129),   S( 61, 104),   S( 28,  38),   S( 30, -13),   S( 29, -13),
            S( 30,  49),   S( 52,  78),   S(102,  99),   S(107, 146),   S( 40,  77),   S( 12, -14),   S(  7, -17),   S(-10, -31),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-97,  29),   S(-115,  15),  S(-30,  15),   S(  6,  24),   S(-16, -17),   S(-115,  55),  S(-131,  42),  S(-65,  35),
            S(-101,   1),  S(-50,  -9),   S(-46, -12),   S(-57,  15),   S(-30,  24),   S(-39,  22),   S(-119,  31),  S(-96,  17),
            S(-16, -13),   S(-55, -10),   S(-32,   5),   S(-63,  57),   S(-78,  91),   S( 32,  36),   S(-71,  31),   S(-74,  43),
            S( 42,  -7),   S( 10,   6),   S(  1,  30),   S(-32,  93),   S( 38,  91),   S(-10,  88),   S(  6,  44),   S( 46,  14),
            S( 40,  -2),   S( 20,  -6),   S( 31,  33),   S( 60, 104),   S( 67, 123),   S( 94,  70),   S( 48,  66),   S( 63,  27),
            S( 59,  -8),   S(  8, -30),   S( 32,  16),   S( 59, 118),   S( 45,  99),   S( 54,  51),   S( 36,  51),   S( 48,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-115,  14),  S(-74,   6),   S( 11, -10),   S( -2,  24),   S(-26,  -2),   S(-68,  41),   S(-25,  19),   S(-70,  36),
            S(-79, -21),   S(-58, -32),   S(-42, -35),   S(-70,  14),   S(-53,   2),   S( -7,   8),   S(-107,  55),  S(-95,  45),
            S(-31, -35),   S(-51, -35),   S(-42,  -4),   S(-19,   0),   S(-54,  32),   S( -1,  51),   S(-69,  72),   S(-45,  64),
            S( 26, -18),   S(-39, -23),   S(-18,  -4),   S(  5,  16),   S(-22,  51),   S( 66,  42),   S( 26, 106),   S(111,  58),
            S( 14,   0),   S(-31, -23),   S( 10,  13),   S(-42,  -4),   S( 58,  73),   S( 67,  72),   S(126, 100),   S(174,  64),
            S( -9,  -6),   S(  4, -48),   S(  9, -18),   S( 10,  -3),   S( 47,  40),   S( 37, 109),   S( 73, 144),   S(124, 125),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26,  14),   S(-27,  18),   S(-25, -16),   S( -1,  -2),   S( -6,   6),   S(-21,   4),   S(-32, -24),   S(-34,  -4),
            S(-37, -31),   S(  8,  10),   S(  0,  31),   S( -2,   1),   S(-28,  23),   S(-16, -10),   S(-44, -29),   S(-20, -54),
            S(-13,  53),   S(  0,  54),   S( 21,  52),   S( -2,  13),   S(  9,  -3),   S(-30, -36),   S(-22, -72),   S(-21, -52),
            S( -3,  61),   S(  2,  72),   S( 63,  90),   S( 35,  50),   S(-19, -38),   S(-10, -43),   S(  4, -26),   S(-37, -66),
            S( 78,  72),   S( 70, 198),   S( 68, 132),   S( 20,  54),   S( 16,  35),   S( -4, -19),   S( -1, -48),   S( -2, -63),
            S( 55, 125),   S( 62, 260),   S(105, 227),   S( 33,  80),   S( -1,  -4),   S( -2, -50),   S(  0, -16),   S(-16, -88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47, -14),   S( -6,   5),   S( -3,  -3),   S(  2,   8),   S( -4, -11),   S(-44, -37),   S(-45, -23),   S(-38,   0),
            S(  1, -29),   S(-22, -20),   S(-10,  -2),   S(  6,  12),   S(-51,  12),   S(-11, -23),   S(-52, -51),   S(-35, -17),
            S( 21,  16),   S( 21,  -7),   S( -5,  21),   S( 13,  50),   S(-14,  23),   S(-15, -15),   S(-33, -36),   S( 18, -57),
            S( 28,  50),   S( 55,  59),   S( 41,  85),   S( 25,  93),   S( 26,  33),   S( 20, -30),   S( 10, -37),   S( 10, -77),
            S( 29,  65),   S( 81, 147),   S(103, 172),   S( 96, 205),   S( 55,  77),   S(  6,  -9),   S(  9, -52),   S( 12, -68),
            S( 26,  83),   S( 96, 146),   S( 95, 233),   S(115, 241),   S( 51, 115),   S(  9, -13),   S(  4, -30),   S( 16, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-65, -47),   S(-55, -27),   S( -7, -30),   S( -2,  -4),   S( -3,   4),   S(-47,  20),   S(-25, -11),   S( -8,  20),
            S(-17,   5),   S(-27, -38),   S(-39, -50),   S(  7,  35),   S(-16,  35),   S( 14,   4),   S(-34,  10),   S(-17,  -8),
            S(-14, -37),   S( -7, -16),   S(-20, -30),   S(  6,  36),   S(  2,  45),   S(-17,  20),   S( -2, -11),   S( 12,  13),
            S( 21, -39),   S( 17, -22),   S( 13, -17),   S( 18,  56),   S( 13, 119),   S( -2,  69),   S(  8,  27),   S( 33,  15),
            S( 12, -39),   S( 28,  -1),   S( 40,  -2),   S( 41, 117),   S( 90, 199),   S( 97, 145),   S( 51,  51),   S( 33,  41),
            S( 10, -46),   S( 20, -29),   S( 46,  64),   S( 41, 124),   S( 65, 251),   S( 51, 163),   S( 38,  92),   S( 22,  48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -47),   S(-37, -16),   S(-18, -45),   S(  0,  -4),   S( 12,  26),   S(-10,  45),   S(-16,  28),   S( 13,  35),
            S(  2, -63),   S(-50, -37),   S(-25, -51),   S(  4,  11),   S(-22,  10),   S( 12,  27),   S( -4,  29),   S(-15,  14),
            S(  1, -64),   S( -7, -70),   S(-26, -73),   S(  0,   5),   S(  6,  14),   S( 35,  40),   S( 27,  60),   S( 33,  50),
            S( 16, -36),   S(-28, -30),   S( -7, -13),   S( 12,  43),   S( 13,  41),   S( 50,  49),   S( 16,  97),   S(  8,  79),
            S( -9, -67),   S( -9, -61),   S( -1, -18),   S( 13,   2),   S( 49, 123),   S( 89, 131),   S( 45, 252),   S( 55,  70),
            S( 13,  13),   S(  4, -11),   S(  0, -13),   S(  8,  30),   S( 30,  76),   S( 58, 216),   S( 34, 228),   S( 45, 106),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-32,  -4),   S(  8,  -1),   S(-44,   9),   S(-34, -11),   S(-51, -27),   S(-16, -26),   S(-54, -70),   S(-56, -43),
            S(-29,  52),   S( 13, -31),   S(-28, -13),   S(  7, -25),   S(  2, -33),   S( -9, -21),   S(-17, -12),   S(-61, -33),
            S(  2,  77),   S( -6, -21),   S( 11, -30),   S(-10,  20),   S( 30,   0),   S(-27, -17),   S( -3, -21),   S(-58, -57),
            S(  7, -58),   S( 56, -19),   S(  9,  24),   S( 26,  14),   S(  9,  -1),   S( 13, -22),   S(-15, -31),   S( -8, -17),
            S(  6, -31),   S( 55,  -5),   S( 23, -10),   S( 82, -18),   S( 30,  -1),   S( 56, -20),   S( 18, -39),   S(-74, -17),
            S( -2, -22),   S( -9, -18),   S( 57, -13),   S( 82, -39),   S( 44, -98),   S( 39, -28),   S(  3, -20),   S(  7, -53),
            S(-50, -28),   S( 32, -45),   S( 25, -19),   S( 34, -59),   S(  5, -34),   S(-15, -51),   S( 14, -39),   S(-22, -30),
            S(-89, -69),   S(-24, -22),   S( 19,  15),   S(-16, -54),   S(-19, -31),   S( 20,  29),   S(  4, -24),   S(  5,  -9),

            /* knights: bucket 1 */
            S(-36, -10),   S(-72,  99),   S(  8,  19),   S( -6,  36),   S( -7,  31),   S(-24,   2),   S(-36,  25),   S( -1,  -1),
            S( 40,  20),   S(  2,  35),   S( 23,  -6),   S( -8,  34),   S( -5,  26),   S(  3,  -2),   S( 14, -29),   S(-35,   3),
            S(-18,  16),   S(  5,  11),   S(  6,  12),   S( 24,  16),   S( 17,  27),   S(-23,  28),   S(  1, -10),   S(-35,  19),
            S( -6,  35),   S( 57,  29),   S( 30,  39),   S( 23,  30),   S( 17,  22),   S(  6,  31),   S( 29,   2),   S(  8,  27),
            S( -7,  51),   S( 30,   6),   S( 42,  24),   S( 47,  20),   S( 29,  44),   S( 34,  12),   S( 14,  13),   S( 28,   7),
            S( 23, -22),   S( 20,   0),   S( 28,  27),   S( 47,  15),   S( 51,   2),   S( 46,  19),   S( 45, -20),   S( 25,  -2),
            S( 38,  -1),   S(-11,  34),   S(-14, -14),   S(  9,   8),   S( 18,  -6),   S( 42, -45),   S(-43,   3),   S(-32,   2),
            S(-90, -72),   S(-36, -18),   S(  4,  -4),   S(  8,  10),   S(-12,  -6),   S(-23, -13),   S( -3, -18),   S(-22, -74),

            /* knights: bucket 2 */
            S(-57,   8),   S( -3,  15),   S(-23,  41),   S(-16,  43),   S(-32,  50),   S(-32,  61),   S(-26,  36),   S(-46,  -7),
            S(-36,  12),   S(-25,  15),   S(-15,  20),   S( -9,  30),   S( -1,  23),   S( -4,  46),   S(-48,  62),   S(-33,  84),
            S(-26,  21),   S( -1,  22),   S(-10,  35),   S( 21,  19),   S( 13,  25),   S(  3,  19),   S(  0,  33),   S(-45,  30),
            S(-14,  46),   S(-22,  36),   S(  0,  53),   S( 10,  47),   S(  1,  54),   S(  2,  45),   S(  7,  46),   S( -9,  55),
            S( 27,  22),   S(-11,  35),   S( -8,  61),   S(-14,  64),   S( -3,  69),   S(-14,  63),   S( -4,  44),   S(-29,  34),
            S(-22,  22),   S(-11,  30),   S(-23,  60),   S( -3,  48),   S(-43,  58),   S(-11,  41),   S(-36,  32),   S( 38, -23),
            S(-15,  29),   S(-51,  24),   S(-28,  35),   S(-34,  34),   S( -3,  -1),   S(  0,  20),   S(-40,  24),   S(-42,   0),
            S(-161,  25),  S( -9, -19),   S(-70,  18),   S(-14, -19),   S( 21, -11),   S(-50,   0),   S( 14,  22),   S(-220, -49),

            /* knights: bucket 3 */
            S(-61,  -7),   S( -4, -20),   S(-55,  23),   S(-14,  12),   S(-13,  20),   S(-16,  26),   S(  1,  17),   S(-20,  -5),
            S(-29,  15),   S(-45,  22),   S(-25,  11),   S(  6,  16),   S( 10,  17),   S( -6,   1),   S(-12,   8),   S(-17,  67),
            S(-15, -18),   S( -4,  10),   S( -6,  22),   S( 15,  21),   S( 21,  23),   S( 15,  16),   S(  0,  19),   S(-12,  47),
            S(-15,  24),   S( 14,  18),   S( 10,  49),   S( 18,  45),   S( 25,  39),   S( 22,  44),   S( 27,  33),   S( 16,  36),
            S( 16,  14),   S(  0,  42),   S( 32,  26),   S( 20,  62),   S( 16,  67),   S( 16,  68),   S( 31,  51),   S( -7,  33),
            S(  4,   7),   S(  7,  12),   S( 32,  15),   S( 56,  20),   S( 55,   2),   S( 77,  -8),   S( 19,  15),   S(-24,  80),
            S( 22, -13),   S(  3,  21),   S( 60, -21),   S( 47,   7),   S( 49, -18),   S( 47, -24),   S( 36, -72),   S( 50, -27),
            S(-117,  12),  S( -8, -18),   S(-37,   9),   S( 13,   3),   S( 24, -16),   S( -6, -18),   S( -1, -10),   S(-71, -38),

            /* knights: bucket 4 */
            S(  8,  19),   S(-54, -11),   S(  5, -29),   S(-12,   0),   S(-21, -42),   S(-48, -36),   S(-21, -61),   S(-29, -39),
            S( 64,  39),   S( -5,   8),   S(-23, -29),   S(  3, -27),   S(  7, -31),   S(  0, -49),   S( 11, -36),   S(-14, -31),
            S( -3,  32),   S( 22,  51),   S( 43,  -8),   S( 53, -15),   S( 20,   0),   S(-26,  19),   S(-22, -66),   S(-13, -76),
            S(  9,  50),   S( 57, -20),   S( 70,   7),   S( 57,  -2),   S( 29, -10),   S(109, -24),   S( 25, -44),   S(  8, -41),
            S( 66,  40),   S(-44,  38),   S( 43,  31),   S( 44,   4),   S( 60,  16),   S(  9, -17),   S( 36, -52),   S( 21, -17),
            S(  2,  12),   S(-33,  -4),   S( 98,  16),   S( 16, -17),   S(  7,  -6),   S( 26,  15),   S(  8,  -4),   S(-10, -37),
            S(  7,  20),   S(-37, -21),   S(-14, -19),   S( 14,  46),   S( 30,   2),   S(  1, -35),   S( 16,  -7),   S(-23, -18),
            S(-17,   0),   S( -9, -23),   S( 12,   4),   S( -5,   4),   S(  7,  29),   S( 17,  27),   S( -4,  -6),   S(-10, -23),

            /* knights: bucket 5 */
            S( 17,   1),   S( -3,   8),   S( 21,  30),   S( 17,  30),   S( 27,   3),   S(-20,   4),   S( 27,   5),   S(-17, -24),
            S( 39, -19),   S( 27,  43),   S( 19,   3),   S( 28,   5),   S( 77,  10),   S( 26,   0),   S(  7,  12),   S(-21, -52),
            S(  0,   5),   S(-33,  42),   S( 72,  13),   S( 71,   7),   S(  5,  37),   S( 21,  17),   S( -2,  -1),   S(  3, -24),
            S( 42,  23),   S( 43,  29),   S( 90,  18),   S( 28,  45),   S( 30,  23),   S( 38,  32),   S( 49,  33),   S( 16,  15),
            S( 29,  38),   S( 55,   7),   S( 74,  37),   S( 74,  33),   S( 88,  37),   S( 52,  34),   S( 37,  38),   S( 31,  38),
            S(-18,   2),   S( 22,  21),   S( 31,  19),   S( -4,  58),   S( 53,  34),   S( 29,  52),   S(-10,   1),   S(  2,  20),
            S( 32,  49),   S(  1,  25),   S(  3,  36),   S( 11,  53),   S(  4,  32),   S(  5,  38),   S( 29,  44),   S(-12, -16),
            S( -5,  17),   S( -5,  19),   S(-11,  23),   S(-13, -37),   S(  3,  20),   S( -1,  34),   S( 18,  44),   S(-21,  -8),

            /* knights: bucket 6 */
            S( -4, -57),   S( 17, -22),   S( 51,  26),   S(-18,  17),   S(-22,  27),   S( 20,  61),   S( 30,   4),   S(-15,  14),
            S( -3, -53),   S( 47,  -3),   S( 39,  -5),   S(-19,  17),   S(-23,  58),   S( 25,  33),   S( 32,  15),   S( 17,  23),
            S(-14, -15),   S( 15,   2),   S( 29,  10),   S( 46,  14),   S( 10,  46),   S(-11,  43),   S( 39,  44),   S( 35,  31),
            S( 69, -12),   S( 71,  -9),   S( 67,  24),   S( 91,  17),   S( 57,  19),   S( 37,  44),   S( 25,  57),   S( 18,  38),
            S(-15,  29),   S(110, -24),   S( 97,  19),   S( 87,  24),   S(145,  26),   S(113,  35),   S( 53,  37),   S( 44,  34),
            S( 37,  27),   S( 77,  -8),   S( 14,  29),   S( 79,  31),   S( 47,  55),   S( 38,  21),   S( 24,  12),   S( 76,  46),
            S( -5,   9),   S(  2,   9),   S(-34,  37),   S( 23,  26),   S( 17,  29),   S(  3,  40),   S(  8,  51),   S( -3,  16),
            S(-33,   3),   S( 22,  41),   S( 38,  23),   S( -2,  27),   S( 30,  31),   S( 18,  40),   S( 10,  29),   S( 16,  23),

            /* knights: bucket 7 */
            S(-23, -46),   S(-204, -23),  S(-38, -31),   S(-63, -39),   S(-26, -21),   S(-35, -14),   S( -7,  26),   S(-18,  -3),
            S(-51, -90),   S( -1, -41),   S(-18, -59),   S(-36,  -5),   S(-27,  -1),   S(  7, -16),   S( 31,  25),   S(-10,  42),
            S(-58, -82),   S(-70, -15),   S(-21,  -2),   S( 57, -31),   S( 33, -17),   S( 36, -10),   S(  2,  55),   S( 40,  55),
            S(-53, -32),   S( 20, -46),   S(  7,  -3),   S( 60, -11),   S( 75, -10),   S( 25,  11),   S( 61,  -5),   S(-27,  40),
            S(-64, -22),   S( -9, -28),   S( 56, -22),   S( 69,  -9),   S(133,  -8),   S( 89,  15),   S( 93,   5),   S( 69,  24),
            S( 16, -39),   S( 13, -28),   S( -2,  -6),   S(  5,  20),   S( 57,  -1),   S( 87,  14),   S( 95, -12),   S(  4,  16),
            S(-55, -45),   S(-57, -34),   S( 26, -27),   S( 34,  27),   S( 24,  10),   S( 50, -11),   S( -2,  36),   S( 18,  -2),
            S(-42, -37),   S( 12,  20),   S( -8, -19),   S( 25,   7),   S( 19,  28),   S( 11, -13),   S(  0, -19),   S(-11, -11),

            /* knights: bucket 8 */
            S( -2,   2),   S( -3,  -3),   S( -5,  11),   S( -6, -13),   S( -7, -24),   S( -6, -49),   S( -9,  -5),   S(-10, -41),
            S(  0, -20),   S( -3,  -2),   S(-14, -14),   S(-33, -55),   S(-25, -34),   S(-26, -92),   S( -6, -33),   S(-18, -38),
            S(  2,   3),   S(-27, -37),   S( 17,  17),   S(  0, -26),   S( -4, -26),   S(-18, -12),   S(-19, -45),   S(  0, -21),
            S(-18,  17),   S(  9,  25),   S(  4,  19),   S( -4,  27),   S( 28,   2),   S( 13, -16),   S( -6, -44),   S(-11, -29),
            S( 44,  86),   S(-13, -13),   S( 19,  11),   S( 23,  25),   S(  0,  27),   S(-12, -17),   S( 12, -18),   S(-11, -10),
            S( 13,  58),   S( -8,  13),   S( 30,  12),   S( 47,   2),   S( -5, -13),   S( -5,   5),   S(-14, -63),   S(-12, -21),
            S( 14,  36),   S(  5,  11),   S( -2, -16),   S(  1, -17),   S(  8, -11),   S(  1,  -9),   S(  2,   8),   S( -1,  -5),
            S(  5,  13),   S( 20,  61),   S(  1,   9),   S( -7,  -4),   S(  8,  22),   S( -5, -15),   S(  4,  23),   S( -3, -12),

            /* knights: bucket 9 */
            S( -9, -35),   S(-17, -32),   S(-19, -57),   S( -4, -27),   S(-29, -59),   S(-19, -63),   S( -5, -15),   S( -3, -19),
            S(-19, -63),   S(-14, -37),   S(-17, -83),   S(-11,   2),   S( -4, -27),   S( -3, -21),   S(  2,  -4),   S(-17, -60),
            S( 10,  -1),   S( -3, -47),   S( 10, -11),   S( 14,   8),   S(-13,  10),   S(-38, -18),   S( -6,  -3),   S(-13, -38),
            S(-13, -26),   S(-10, -11),   S( -7,  15),   S( 37,   3),   S( 30,  11),   S( 12,  33),   S(-25, -69),   S( -2, -10),
            S(  1,  14),   S(  1,  -6),   S( 17,  35),   S( 19,  26),   S( 15,   3),   S(  8,  -7),   S(  4, -24),   S(  9,   8),
            S(  1,  13),   S( 17,  33),   S( 19,  43),   S(  4,   0),   S( 24,  41),   S(  5,  12),   S( 18,  50),   S( -6, -19),
            S( -2, -20),   S( -6,  27),   S(  8,  33),   S( -6,  12),   S( 10,  53),   S(  3,  10),   S(  4,   9),   S( -4,  -9),
            S( -5, -18),   S(  6,  26),   S(  7,  12),   S(  2,  14),   S(  5, -15),   S(  4,  25),   S(  6,  14),   S( -1, -13),

            /* knights: bucket 10 */
            S(-18, -56),   S( -3, -24),   S(-13, -47),   S(-23, -47),   S( -3,   3),   S(-18, -59),   S( -3,   4),   S(  5,  14),
            S( -9, -40),   S( -5, -15),   S( -5, -14),   S(-28, -24),   S(-41, -67),   S(-13, -42),   S( -6,  -8),   S( -3, -25),
            S(-27, -72),   S(-27, -63),   S(-17,  -8),   S(-11, -16),   S( 10,  -7),   S( -9,   6),   S( -5,  -4),   S( -7, -16),
            S(  1,  -3),   S(-18, -55),   S(  2, -33),   S(  2,  -1),   S(  3,  10),   S( 38,  -3),   S(  4,  32),   S(  7,  14),
            S(-17, -43),   S(-21, -27),   S(-12,  -2),   S( 38,  12),   S( 24,   6),   S( 20,  20),   S( 15,  30),   S( 14,  20),
            S(-11, -28),   S(  3,  -1),   S(  2,  17),   S(  0,  11),   S( 26,  48),   S( 22,  36),   S( 39,  86),   S( 19,  67),
            S( -2,  -6),   S(-14, -40),   S(-14, -37),   S( 18,  35),   S( 19,  46),   S(  8,  15),   S( -3, -20),   S(  6,  20),
            S( -4, -16),   S(  4,   9),   S(-11,  -6),   S( -2,   8),   S(  6,  46),   S(  3,  11),   S( 13,  41),   S( -2,  -2),

            /* knights: bucket 11 */
            S( -4,  -3),   S( -9, -36),   S( -2, -49),   S(-11, -23),   S(-29, -48),   S( -2, -37),   S(  2,  -3),   S(-11, -29),
            S( -3,  -6),   S(-25, -77),   S( -3, -69),   S(-15, -44),   S(-14, -10),   S(-15, -13),   S(-12, -35),   S(-11, -18),
            S(-21, -82),   S(-33, -67),   S(-30, -14),   S(  8,   0),   S(-27, -10),   S(-14,  18),   S( 24,  11),   S( -2,   8),
            S(-22, -25),   S( -5, -19),   S(  2,   9),   S( -6,  20),   S( 24,  13),   S( -7,   6),   S(  6,  18),   S( 22,  46),
            S( -9, -20),   S(-44, -77),   S(  0,  14),   S(  3, -26),   S(-12,  18),   S( 39,  46),   S(-10, -15),   S( 16,  77),
            S( -1,   7),   S( -6, -35),   S( -3,  18),   S( 46,  51),   S(  7,   8),   S( 53,  48),   S(  3,  25),   S( 22,  62),
            S( 11,  42),   S( -2, -21),   S( 10,  -3),   S( 13,  -3),   S( 11,  22),   S( -9,  -8),   S( 14,  41),   S(  5,  62),
            S(  3,  11),   S( -6, -32),   S( 12,  44),   S( 13,  40),   S(  0,  -1),   S( -3, -14),   S(  4,   4),   S(  5,  33),

            /* knights: bucket 12 */
            S( -2, -11),   S( -3,   0),   S( -2, -19),   S( -5,  -2),   S( -1,  -2),   S( -5, -17),   S( -2, -11),   S(  0,  -3),
            S( -2,  -2),   S( -3,  -7),   S(  6,  27),   S( -6, -23),   S( -3,  -7),   S( -8, -40),   S(  1,  -3),   S(  1,  -8),
            S( -4,  -2),   S(  8,  21),   S( -6, -11),   S(  0,  11),   S( -2, -18),   S( -2,  -9),   S(  3,   3),   S( -6, -23),
            S( -5,  -9),   S( -1,  -1),   S( -8,  -4),   S(  7,  48),   S(  0,  -3),   S(  6,  22),   S( -6, -13),   S(  2,  -7),
            S( 10,  26),   S(  7,  -1),   S( 10,  19),   S(-12,  -3),   S( -2, -13),   S(  6,  11),   S(  2,   3),   S(  5,   7),
            S(  0,  25),   S(  4,  -6),   S( -6,  32),   S( -5, -12),   S(  4,   3),   S( -4, -19),   S(  2,  -4),   S(  1,   9),
            S(  2,   9),   S(  1,  20),   S(  2,  -2),   S(  4,   0),   S( -5, -19),   S( -5, -19),   S( -1,  -1),   S( -2,  -8),
            S( -1,   0),   S(  9,  48),   S( -3,  -6),   S(  5,  18),   S( -5, -12),   S( -4, -15),   S( -7, -23),   S(  1,   7),

            /* knights: bucket 13 */
            S( -3,  -5),   S( -6, -22),   S( -4, -12),   S( -5, -12),   S( -9, -26),   S(  0,  -7),   S( -3, -13),   S( -1,  -1),
            S(  1,   2),   S(  2,   6),   S(  0, -21),   S( -3, -10),   S( -8, -32),   S( -1,  -6),   S(  0,  -5),   S( -4, -25),
            S( -2, -10),   S(-11, -25),   S(  8,  29),   S(  7,  18),   S( -2,   2),   S(-15, -36),   S( -3,  -5),   S( -8, -28),
            S(-15, -19),   S(  8,  26),   S(  4,  -2),   S( -2,   5),   S( -5,  -6),   S( 10,  20),   S(  4,   7),   S( -4, -11),
            S(  6,  10),   S( -9,   6),   S(  1,   0),   S( 13,  24),   S(  2,   9),   S(  2,  14),   S(  0, -11),   S(  0,   3),
            S(  0,  14),   S(  6,  -1),   S(  1,  63),   S(-17,  38),   S(  0,  26),   S(-13, -49),   S(  5,   7),   S( -7, -17),
            S( -1,   6),   S( -2,   7),   S(  4,   2),   S(  1,  33),   S( 12,  65),   S( -5,   2),   S(  0,  14),   S( -2,  -3),
            S( -1,   0),   S(  1,  31),   S( -7, -12),   S(  3,  22),   S( -2,  18),   S(  1,   9),   S( -3,  -6),   S(  1,   3),

            /* knights: bucket 14 */
            S( -3, -20),   S( -5, -33),   S( -5,  -4),   S(  1,   7),   S(-12, -46),   S( -2, -20),   S(  0,  -5),   S(  0,  -6),
            S(  1,  -1),   S( -4, -17),   S(-12, -43),   S( -9, -28),   S(  2,  -3),   S(  1,   0),   S(  3,   7),   S(  1,   2),
            S( -4,  -9),   S( -1, -12),   S(-18, -54),   S(  3,  27),   S( -2, -11),   S(  4,   1),   S(  3,  -8),   S( -1,  -6),
            S(  0,   3),   S( -7, -29),   S( -9, -31),   S(  1,   7),   S( -2, -21),   S(  8,  -2),   S( -6, -23),   S( -6, -12),
            S(  3,  18),   S(  3,   0),   S(  9,  37),   S( -2, -10),   S(-17,  -8),   S( 11,  12),   S(  8,   7),   S( -5, -34),
            S( -3, -16),   S(  6,   8),   S( -2,   5),   S(  5,  18),   S( 15,  51),   S(  6,  34),   S(  0,   7),   S(  1,   4),
            S(  4,  14),   S(  0,  -2),   S( 11,  23),   S(  8,  28),   S(  2,  51),   S(  0,  24),   S(  4,  31),   S(  2,  15),
            S(  0,  -3),   S(  4,  14),   S( -5, -13),   S(  3,  41),   S(  6,  28),   S(  6,  22),   S(  4,  32),   S(  0,   3),

            /* knights: bucket 15 */
            S( -1,   1),   S( -6, -38),   S( -4, -36),   S( -8, -16),   S( -1,  -4),   S(  1, -14),   S( -1,  -5),   S( -2, -11),
            S( -3, -21),   S( -2, -12),   S( -2, -12),   S( -2, -20),   S(  3,  -6),   S( -5, -24),   S( -1,  -9),   S(  0,   1),
            S( -1,  -3),   S( -2,   7),   S(  6,  -8),   S( -8, -24),   S(  3,   1),   S(  5,  10),   S(-13, -32),   S( -1,  -4),
            S( -2, -12),   S( -8, -29),   S( -4, -29),   S( -3,  -9),   S( -4,   7),   S( 12,  36),   S(  2,   5),   S( -4,  -9),
            S( -3,  -7),   S(  5,  11),   S( -3,  -9),   S( -2, -13),   S( 10,  14),   S( 22,  42),   S( -4,  -9),   S(-10, -34),
            S( -3,  -1),   S( -2,  -6),   S( -7, -19),   S( -2,  13),   S( -6, -37),   S(  4,  25),   S(-10, -15),   S(  9,  32),
            S( -3,  -8),   S(  0,   2),   S( -2, -11),   S(  0,   3),   S(-12, -24),   S(  3,  20),   S(  0, -10),   S(  2,  21),
            S( -1,  -4),   S(  1,   5),   S( -1,  -3),   S(  1,   1),   S( -3, -14),   S( -4, -12),   S(  7,  33),   S( -1,   0),

            /* bishops: bucket 0 */
            S( 36,  35),   S(-26,  19),   S( 50,  13),   S( 11,  14),   S(-19,  15),   S(  4,  -1),   S(  3, -37),   S(  6, -47),
            S( 35, -32),   S( 85,  23),   S( 32,  28),   S( 20,   1),   S(  1,  23),   S(-10, -10),   S(-27,   9),   S( 10, -45),
            S( 25,  22),   S( 46,  36),   S( 29,  42),   S( 13,  52),   S( 16,  32),   S(-12,  52),   S( 12, -28),   S( 19, -38),
            S( 22,  -1),   S( 66,   9),   S( 40,  18),   S( 38,  28),   S( -3,  57),   S( 41,   2),   S(  7,   9),   S(-10,  33),
            S( 10,  13),   S( 39,   3),   S( -3,  44),   S( 56,  38),   S( 48,  28),   S( -3,  37),   S( 12,  18),   S(-64,  55),
            S(-38,  71),   S(-26,  57),   S( 39,  27),   S( 30,  54),   S( 39,  54),   S(  0,  26),   S(-20,  35),   S(-18,  35),
            S(-40,  60),   S( -8,  31),   S( 21,  36),   S(-40,  76),   S(-66,  35),   S( 52,  16),   S( 23,  20),   S(-17,  26),
            S(-46, -40),   S( -4,  36),   S(-10,  51),   S( 19,  39),   S( 18,  45),   S( 36,  53),   S(  0,  40),   S(-51,  23),

            /* bishops: bucket 1 */
            S( 33,  51),   S(-28,  49),   S( 14,  21),   S( 15,  24),   S( 12,  11),   S( -7,  29),   S(-19,  41),   S(-18,  -1),
            S( 17, -15),   S( 35,  19),   S( 53,   9),   S( 24,  37),   S(  3,  17),   S(  9,   3),   S(-14,  20),   S(  1, -13),
            S( 38,  13),   S( 17,  22),   S( 45,  31),   S( 13,  42),   S( 14,  35),   S( -2,  32),   S( 28,  12),   S( 15, -20),
            S( 48,   1),   S( 25,  35),   S(  4,  35),   S( 28,  49),   S(-11,  55),   S( 29,  27),   S( -8,  51),   S( 20,   4),
            S( 43,  31),   S( 16,  23),   S( 24,  40),   S( -3,  56),   S(  0,  63),   S(-16,  60),   S( 26,   6),   S(  5,  20),
            S(-30,  47),   S( 10,  54),   S( -7,  66),   S( 51,  25),   S(-32,  67),   S(-19,  64),   S(-21,  63),   S( 56,  -4),
            S( -5,  51),   S(  8,  46),   S(  2,  59),   S( 16,  63),   S( 16,  56),   S(-34,  47),   S( -2,  69),   S(-37,  43),
            S(-14,  45),   S( -1,  22),   S(-20,  36),   S( 10,  30),   S( -8,  55),   S(  4,  40),   S(-20,  48),   S(-47,  97),

            /* bishops: bucket 2 */
            S( 18,  27),   S( -5,  34),   S(  0,  30),   S(-17,  53),   S(-12,  42),   S(-23,  33),   S(-26,   0),   S(-44,  54),
            S(-10,  24),   S( 12,  28),   S( 21,  22),   S(  6,  40),   S(  0,  43),   S(  8,  15),   S( 10,   5),   S( -8, -22),
            S( -7,  25),   S( -5,  44),   S( 14,  61),   S( -2,  64),   S( -1,  52),   S( 10,  52),   S(  9,  34),   S(-17,   9),
            S(-11,  35),   S(-18,  60),   S(-20,  61),   S( -3,  66),   S(-12,  64),   S( 11,  50),   S(  6,  50),   S( -8,  36),
            S( -9,  40),   S(-16,  50),   S(-33,  55),   S(-36,  72),   S(-20,  75),   S(-15,  71),   S(  0,  42),   S(-24,  29),
            S( -9,  27),   S(-28,  47),   S(-17,  62),   S(-33,  61),   S( -3,  46),   S(-20,  76),   S( 19,  52),   S( -2,  38),
            S(-46,  61),   S(-15,  70),   S(-23,  68),   S(-17,  50),   S(-43,  77),   S(-47,  41),   S(-75,  69),   S(-28,  58),
            S(-107,  92),  S(-59,  93),   S(-45,  53),   S(-96,  81),   S(-66,  49),   S(-84,  72),   S(-12,  39),   S(-73,  74),

            /* bishops: bucket 3 */
            S(  9,  52),   S(  2,  31),   S( 15,  36),   S(  1,  40),   S(  9,  30),   S( 42,   1),   S( 26,  -7),   S( 33, -22),
            S(  0,  28),   S(  6,  43),   S( 21,  22),   S(  9,  50),   S( 16,  35),   S(  1,  50),   S( 53,  33),   S( 17,  10),
            S( 16,  26),   S(  5,  51),   S( 17,  65),   S( 15,  56),   S( 11,  71),   S( 20,  65),   S( 30,  46),   S( 34,  21),
            S( 13,  27),   S( -7,  66),   S(  2,  70),   S( 16,  79),   S( 13,  73),   S( 31,  50),   S( 18,  66),   S(  8,  26),
            S(  3,  40),   S( 12,  44),   S( 19,  50),   S( 14,  78),   S(  9,  78),   S( 27,  57),   S(  8,  60),   S( 16,  56),
            S( 11,  40),   S( 11,  53),   S(  4,  67),   S( 16,  46),   S( 10,  60),   S( 33,  63),   S( 20,  44),   S( -3,  93),
            S( -5,  39),   S(-17,  74),   S( 39,  45),   S( -5,  55),   S( -7,  54),   S(-18,  48),   S(-12,  79),   S(  7,  69),
            S(-35,  99),   S(-38,  69),   S( 34,  37),   S( -4,  69),   S(-32,  63),   S( -7,  69),   S( -2,  46),   S( 66,  31),

            /* bishops: bucket 4 */
            S(-23, -29),   S( -8,  -3),   S(-66,  -4),   S(-57,  21),   S(-11,   4),   S(-40,  -7),   S(  2,  23),   S(  8,  -9),
            S(-20,  18),   S(  1,  20),   S(-26,  41),   S( -6,  18),   S(-34,  -3),   S( 52, -11),   S( -6, -10),   S( -1, -58),
            S( -2,   8),   S(-42,  17),   S(-27,  30),   S(-56,  51),   S( 27,  18),   S( 84,  -6),   S(-12, -27),   S(-63,  16),
            S( -1,  39),   S( 14,   8),   S( 33,  31),   S( 40,  25),   S(  5,  36),   S( 35,   3),   S(-14,  28),   S( -8, -25),
            S( 37,  -7),   S(  6,  36),   S( -3,  69),   S( 50,  28),   S(-12,  26),   S(-46,  28),   S(-29,   8),   S(-10,  15),
            S(-15,  15),   S( 17,  21),   S(  4,  29),   S( 37,  10),   S( 25,  37),   S( 13,  13),   S( -1,   8),   S(-24,  -8),
            S(-29,  11),   S( 38,  16),   S(  2,  20),   S(  8,  35),   S(  3,  10),   S(-15,  18),   S( 11,   3),   S(-10, -24),
            S( 15,  45),   S(-18, -14),   S( -1,  15),   S(-15,  -2),   S(  8,  30),   S( -2,  20),   S(  2,  13),   S(-10,   6),

            /* bishops: bucket 5 */
            S(-20,  25),   S(-16,  33),   S(-47,  38),   S(-42,  25),   S(-26,  24),   S( -3,  12),   S(  1,  33),   S(-11,  28),
            S(-32,  29),   S(-22,  53),   S( -3,  46),   S( 25,  27),   S( -7,  38),   S( -2,  19),   S(-19,   6),   S(-36,  22),
            S( -9,  33),   S(-41,  57),   S( 36,  42),   S( 37,  32),   S(  7,  47),   S(-34,  42),   S(-13,  32),   S( -9,  -4),
            S( -1,  28),   S( 10,  34),   S( 14,  46),   S( 33,  48),   S(  9,  49),   S( 24,  40),   S(-27,  44),   S(  7,  32),
            S( 39,  44),   S( 46,  17),   S( 62,  32),   S( 90,  35),   S( 45,  37),   S( 39,  37),   S( 20,  35),   S(-27,  16),
            S( 59,  11),   S( 45,  47),   S( 85,  36),   S(-23,  56),   S( -1,  45),   S( 18,   8),   S(  9,  37),   S( -2,  49),
            S( 10,  57),   S(-69,  49),   S( -9,  42),   S( 19,  47),   S(  9,  52),   S( 11,  56),   S(  2,  49),   S(  1,   8),
            S(-20,  55),   S( 12,  48),   S( -6,  17),   S( -4,  45),   S(  6,  39),   S(  9,  54),   S(  8,  54),   S(  3,  32),

            /* bishops: bucket 6 */
            S( -8,  42),   S( -2,  28),   S(-25,  30),   S(-30,  30),   S(-28,  35),   S(-64,  51),   S(-18,  48),   S( -8,  19),
            S( 23,  10),   S(  9,  17),   S( -3,  35),   S(-17,  45),   S(  4,  35),   S(  3,  25),   S(-131,  82),  S( 36,  43),
            S(  8,  15),   S( -3,  34),   S( 23,  43),   S( 44,  31),   S( 76,  15),   S( 35,  31),   S(  9,  38),   S(-60,  54),
            S( 27,  30),   S( 34,  42),   S( 26,  42),   S( 60,  35),   S( 29,  43),   S( 37,  31),   S( 55,  44),   S(-30,  25),
            S(-31,  54),   S( 47,  31),   S( 78,  21),   S( 52,  31),   S(109,  38),   S( 87,  29),   S( 54,  35),   S( -5,  27),
            S(  3,  30),   S( 10,  26),   S( 42,  33),   S( 57,  32),   S( 19,  56),   S( 40,  48),   S( 36,  34),   S(-40,  58),
            S(-13,  29),   S(-25,  48),   S( 32,  28),   S(  2,  43),   S( 29,  43),   S( 20,  42),   S(  7,  53),   S(-42,  54),
            S(  6,  69),   S(  2,  59),   S(  6,  43),   S(-13,  61),   S( -3,  35),   S(-10,  40),   S( -8,  35),   S(-12,  52),

            /* bishops: bucket 7 */
            S(  2, -10),   S(-32,  15),   S(-74,  16),   S(-29,   5),   S(-35,  -9),   S(-73,   0),   S(-70, -22),   S(-43, -16),
            S(-41, -11),   S(-65,  19),   S(-21,  -2),   S( 15,   3),   S( -4,  11),   S(-46,  25),   S(-20,  -9),   S(-34, -11),
            S(-29,   3),   S( 15, -16),   S( 22,  12),   S( 43,   5),   S(-24,  27),   S( -9,  19),   S( -2,  18),   S(-22,  18),
            S(-36,  29),   S( 19,  30),   S( 69,  -1),   S( 86,  13),   S(105,  -1),   S(  2,  30),   S( 38,  37),   S(-12,  30),
            S(-13,   4),   S(-25,   3),   S( 56,  -6),   S( 86,   2),   S( 85,  23),   S( 92,   9),   S( 15,  33),   S( 87, -11),
            S(-27,  -2),   S(-22,  14),   S(-12,  13),   S( 36,   5),   S( 42,  21),   S( 80,  25),   S( 68,  21),   S(-10,  30),
            S(  1,   8),   S(-13,  16),   S(-15,  15),   S(  6,  24),   S(  9,   1),   S( 22,   7),   S( 28,  28),   S(  7,  34),
            S(  5,  19),   S( -2,   7),   S(-30,   2),   S( -7,  26),   S(  1,   2),   S( 19,  14),   S( 25,  18),   S( 33,  53),

            /* bishops: bucket 8 */
            S(-14, -65),   S(-14, -70),   S(-40, -31),   S(  6, -15),   S(  2, -13),   S(-15, -39),   S( 18,  15),   S( -2, -16),
            S( -6, -46),   S(-13, -80),   S(-15, -50),   S( -3, -18),   S(  7,  -6),   S( 10, -13),   S( -6, -70),   S( -9, -60),
            S( -4, -19),   S( -8,  -6),   S( -4, -14),   S( 19, -19),   S(  4, -16),   S( -7, -30),   S( -3, -39),   S(-20, -47),
            S( 11,  35),   S( -1, -25),   S( 27,  11),   S( 16,  -3),   S( 12, -14),   S( 13, -44),   S(  5, -20),   S( -3, -32),
            S( 15,  32),   S( 22,  62),   S( 19,   2),   S( 57,  -1),   S( 30, -38),   S( 16,   5),   S(  5, -14),   S(-10, -46),
            S(  1,   0),   S( 13, -18),   S( 25,  21),   S( 11,   0),   S( 24,  10),   S( -1, -23),   S( -4, -58),   S(-14, -31),
            S(-13, -12),   S( 17,   6),   S( 15,   2),   S( -1, -15),   S( -7, -17),   S(  9, -13),   S( -1, -39),   S(-11, -25),
            S(-12, -34),   S(  0, -38),   S( -2,  -9),   S( -4, -27),   S(-15, -64),   S(  0, -36),   S( -8, -37),   S( -2, -19),

            /* bishops: bucket 9 */
            S(-15, -43),   S(  1, -58),   S(-17,  -4),   S(-16, -51),   S(-17, -42),   S(  2, -48),   S(-19, -70),   S(  2,   6),
            S(-11, -35),   S(-21, -38),   S(-20, -39),   S(  6, -25),   S( 20,  -5),   S( -1, -33),   S(  1, -46),   S( -8, -32),
            S( -1, -31),   S( 16,  -9),   S( 16, -23),   S( 24, -16),   S( 42, -29),   S( 23, -36),   S( -6, -21),   S( -4,  -7),
            S(-16, -25),   S( 26,  16),   S( 17, -24),   S( 72, -24),   S( 43, -10),   S( 14,  -2),   S( 11, -25),   S(-15, -53),
            S(-14, -24),   S( 39,  14),   S( 29,   5),   S( 60,   5),   S( 31, -21),   S( 33, -23),   S( 15, -19),   S(-11, -16),
            S(-11, -44),   S( 43,  17),   S( 20,  14),   S( 21,   7),   S( 34, -33),   S( 17, -34),   S(  5, -40),   S(-21, -52),
            S(  6,   5),   S( 29,  -2),   S( 17,   4),   S( 37,  -3),   S( 24, -10),   S( -5, -47),   S( -4, -12),   S(-14, -65),
            S( -1, -26),   S( 10, -23),   S(  3, -34),   S(-19, -48),   S(  0, -27),   S(  1, -25),   S(  3, -33),   S(-12, -61),

            /* bishops: bucket 10 */
            S(-25, -51),   S(  4, -51),   S(-59, -48),   S(-12, -51),   S(-11, -33),   S(-25, -58),   S( -3, -47),   S(-11, -54),
            S( -7, -52),   S(-13, -54),   S(  7, -47),   S(-12, -60),   S( -5, -39),   S( -1, -40),   S( -6, -62),   S(  0, -30),
            S(-12, -50),   S(  2, -52),   S(  1, -59),   S( 29, -57),   S( 41, -42),   S( 15, -10),   S(-10,  -2),   S(  0, -20),
            S(-16, -78),   S( 15, -43),   S( 30, -32),   S( 63, -36),   S( 64, -22),   S( 20,  -4),   S( -4, -19),   S(  4,  -2),
            S(-17, -37),   S( 33, -31),   S( 47, -41),   S( 99, -22),   S( 66, -11),   S( 48,  -3),   S( 18,  22),   S( -8, -36),
            S(-16, -59),   S( 20, -68),   S( 26, -26),   S(  8, -52),   S( 36, -23),   S( 49,  10),   S( 24, -11),   S(  6, -26),
            S(-23, -103),  S( -1, -46),   S( -6, -64),   S( 12,  -5),   S( -3, -47),   S( 20,   2),   S( 25,  47),   S( 10,  17),
            S( -1, -51),   S( -2, -37),   S(  3,  -4),   S( -6, -34),   S( -4, -33),   S( -9, -48),   S(  7,  -1),   S(  6,   5),

            /* bishops: bucket 11 */
            S( -7,  -8),   S(-27, -23),   S(-61, -57),   S( -7,  -8),   S(-12, -36),   S(-49, -63),   S(-20, -54),   S(-30, -59),
            S(  6, -35),   S( -1, -32),   S( -5, -29),   S(-22, -46),   S(-35, -32),   S(-26, -59),   S(-15, -58),   S(-27, -49),
            S(-23, -93),   S(  0, -56),   S( 10, -27),   S( 29, -64),   S(  4, -40),   S( 17, -23),   S( -8,  -6),   S(  2, -32),
            S(-10, -31),   S(  5, -60),   S( 40, -52),   S(  9, -38),   S( 55, -34),   S( 20,  -3),   S( 39, -19),   S(  4,  13),
            S( -2, -21),   S(-15, -54),   S( 23, -14),   S( 57, -36),   S( 40, -19),   S( 46, -14),   S(  4,  16),   S( 20,  -7),
            S(-19, -80),   S(  9, -63),   S( 18, -58),   S( 41, -18),   S( 10, -28),   S( 38,   2),   S( 13,  27),   S(-10, -37),
            S(-10, -57),   S(  3, -46),   S(  0, -44),   S(  4, -52),   S( 13,  -9),   S( 39, -15),   S( 16, -12),   S( -3, -11),
            S(-11, -58),   S(-19, -45),   S(  5, -26),   S(  5, -46),   S( 20, -33),   S(-15, -59),   S( -3, -19),   S( -3, -12),

            /* bishops: bucket 12 */
            S( -1, -12),   S(-11, -40),   S(-13, -60),   S( -4, -37),   S(-13, -39),   S( -1,  -2),   S( -3,  -5),   S(  4,   8),
            S( -8, -32),   S( -9, -34),   S(  1,  -2),   S( -5, -14),   S(-12, -55),   S(  3,   0),   S(  0, -16),   S( -2,  -7),
            S(  0, -17),   S(-14, -18),   S( -2,  -7),   S(  1, -40),   S(  5,  27),   S( -9, -39),   S(-18, -37),   S(  0,   1),
            S( -2, -15),   S( -3, -33),   S(-11, -33),   S(  0, -14),   S(  4,   1),   S( 12,  16),   S( -9, -45),   S( -3, -16),
            S(  2,  15),   S(-13, -30),   S(  0, -32),   S(-10, -20),   S(  9,  -9),   S( -5, -41),   S(  4, -29),   S(-10, -37),
            S(-18, -21),   S( -2,  21),   S(  2,  10),   S(-15, -39),   S(  7, -18),   S(  3,   6),   S(  3, -21),   S(  3,   5),
            S( -5, -25),   S(  0,   1),   S(  8,  23),   S(-11, -51),   S( -1, -18),   S( 16,  32),   S(-12, -36),   S( -2,  -8),
            S(  1,   0),   S( -5,  -7),   S(  1, -39),   S( -2, -22),   S(  7,  -3),   S(  1,  -5),   S(-10, -29),   S( -1,  -8),

            /* bishops: bucket 13 */
            S(-12, -68),   S(-10, -69),   S(-11, -47),   S( -9, -65),   S( -8, -57),   S( -9, -30),   S(  2,  15),   S(-10, -55),
            S( -5, -37),   S( -5, -47),   S( -7, -44),   S( -8, -29),   S( -4, -27),   S( -4, -22),   S(  1,  -5),   S(  0, -45),
            S( -1, -13),   S(  0,   7),   S( 11,  -7),   S( -3, -70),   S( -2, -62),   S( 23, -10),   S( -2, -23),   S( -1,  -5),
            S( -6, -20),   S(  4, -24),   S(  3, -35),   S( -8, -48),   S( 16, -34),   S(  4,   2),   S(  5,  -2),   S( -6, -49),
            S( -3,  -3),   S( -6, -17),   S(  8, -37),   S( 36,  24),   S( -5, -39),   S( 12, -16),   S( -1, -53),   S( -3, -49),
            S( -1,  -9),   S( -3,  -5),   S( -2, -15),   S( 12, -10),   S( 13,  -2),   S(  9, -24),   S( 15, -11),   S( -2, -24),
            S( -6, -31),   S(  1, -31),   S(  1,   5),   S( -5,  15),   S( -7, -39),   S(  4,  -8),   S(-13, -77),   S(  4,   2),
            S(-10, -32),   S( -4, -13),   S(  2, -12),   S( 10,  -7),   S( -2, -15),   S( -6, -33),   S( -2, -11),   S( -9, -64),

            /* bishops: bucket 14 */
            S( -3, -38),   S(-15, -69),   S(-11, -44),   S(-22, -89),   S( -8, -46),   S( -4, -38),   S(-12, -76),   S( -6, -39),
            S( -6, -32),   S(  1, -16),   S(  8, -14),   S(-22, -79),   S( -3, -46),   S(-12, -88),   S(-12, -40),   S(  2, -18),
            S( -4, -11),   S(-10, -37),   S(-10, -64),   S(  3, -41),   S( -4, -62),   S(-17, -71),   S( -9, -64),   S( -4, -19),
            S( -4, -30),   S( -1, -31),   S(  9,   9),   S(  9, -12),   S( 14, -55),   S(  4, -51),   S(-19, -79),   S( -1, -13),
            S( -6, -29),   S(  2, -23),   S(  2, -45),   S( 13, -35),   S(  4, -41),   S(  8, -20),   S(  3, -12),   S( -4, -30),
            S(  2,  -3),   S(  5, -25),   S( -4, -28),   S( -3, -15),   S( 10,   5),   S(  7,  25),   S( -1, -21),   S( -5, -25),
            S( -7, -54),   S( 20,  14),   S( -5, -24),   S(  1, -17),   S( -7, -40),   S(  7,  -2),   S( -1,   3),   S( -4, -26),
            S( -1, -11),   S( -3, -30),   S( -2, -15),   S(-10, -44),   S( -9, -35),   S(-12, -45),   S( -4,   2),   S(  0, -12),

            /* bishops: bucket 15 */
            S(  5,  30),   S(  6,  18),   S(-15, -57),   S( -2,  -5),   S( -8, -41),   S( -6, -16),   S(-11, -44),   S( -1,  -4),
            S(  2,   8),   S(  1,  -2),   S(  7,  -3),   S( -5, -39),   S(-10, -23),   S(  4,  -3),   S( -5, -20),   S( -2,  -8),
            S( -8, -33),   S(-12, -42),   S( -5, -38),   S( -7,  -5),   S( -1, -53),   S( -8, -47),   S(-10, -28),   S(  3, -11),
            S(  1,  -4),   S(-17, -53),   S(  6,   9),   S(-15, -68),   S(  9,  -5),   S( -6, -35),   S(  7,  15),   S( -3,  -7),
            S( -1, -22),   S( -5, -20),   S( -3, -36),   S(-10, -38),   S( -7, -32),   S( -6, -46),   S(  7, -13),   S( -2, -10),
            S( -4, -37),   S( -5, -69),   S(-13, -43),   S(-18, -60),   S(-12, -43),   S( -1, -14),   S( 20,  14),   S(  2,  11),
            S(  0, -15),   S( -2, -26),   S(  1,  -7),   S( -4, -45),   S( -7, -44),   S( -2, -16),   S( -5, -20),   S(  9,  25),
            S( -9, -38),   S( -2,  -8),   S( -2, -25),   S( -3, -38),   S( -6, -31),   S(-18, -60),   S( -7, -15),   S(  0,  -1),

            /* rooks: bucket 0 */
            S(-21,  -5),   S(  0,  -8),   S(  8, -30),   S( 15, -22),   S( 14, -13),   S( 13, -22),   S( 12,   5),   S( 20, -11),
            S(-10, -48),   S( 56, -39),   S( 27, -18),   S( 21, -21),   S( 46, -33),   S(  0,  -9),   S(-23,   5),   S(-45,   9),
            S(  5, -31),   S( 29,   3),   S( 35,   3),   S( 22,   7),   S( -4,  32),   S( 25,   3),   S(-17,  21),   S(-33,   1),
            S(  3, -24),   S( 78,  -4),   S( 27,  40),   S( 57, -11),   S( 23,  10),   S( 15,   7),   S(-21,  10),   S(-27,  28),
            S( 85, -46),   S( 96,  -5),   S( 80,  -1),   S( 36,  -6),   S( 52,   9),   S( 21,  11),   S( 13,  17),   S(-24,  26),
            S( 99, -70),   S( 68, -38),   S( 48,  12),   S( 28,  13),   S( 70,   3),   S(-10,  17),   S( 48, -10),   S(-27,  37),
            S( 43, -18),   S( 80,  -5),   S( -3,  17),   S( 15,  20),   S( 11,  22),   S(  4,  18),   S(  0,  19),   S(  5,  20),
            S( 17,  17),   S(  6,  30),   S( 38,  18),   S( 34,  28),   S( 35,  24),   S( 49,   0),   S(-16,  22),   S( 10,  22),

            /* rooks: bucket 1 */
            S(-70,  32),   S(-35,  20),   S(-40,  -7),   S(-33, -11),   S(-15, -19),   S(-18, -10),   S(-16,  -9),   S(-26,  18),
            S(-21,   6),   S(-63,  30),   S(-14,   6),   S(-12, -33),   S(-30,  -8),   S(-38,   0),   S(-26, -16),   S(-57,  11),
            S( 10,   5),   S(-14,  32),   S(-11,  25),   S(-46,  35),   S(-42,  43),   S(  3,  -2),   S(-19,  11),   S(-38,  21),
            S(-33,  47),   S(-44,  50),   S(  3,  35),   S(-17,  31),   S( -5,  33),   S(-49,  61),   S(-29,  27),   S(-39,  33),
            S( 62,  30),   S( 49,  34),   S(  4,  30),   S(-32,  56),   S( 12,  24),   S( 30,  -2),   S( 24,  17),   S(-52,  37),
            S( 63,  11),   S(-19,  70),   S( 36,  23),   S( 10,  26),   S( 31,  16),   S(  9,  21),   S(  2,  31),   S(-20,  21),
            S(-11,  42),   S( 12,  37),   S( 43,  21),   S(-87,  79),   S(-46,  50),   S(-10,  50),   S(-42,  42),   S(-74,  56),
            S( 65,  24),   S( 38,  32),   S( 13,  35),   S(-84,  83),   S(-18,  28),   S( 25,  13),   S(  1,  36),   S( -5,  14),

            /* rooks: bucket 2 */
            S(-62,  45),   S(-41,  32),   S(-35,  29),   S(-53,  26),   S(-49,  13),   S(-50,  17),   S(-34,  -8),   S(-48,  31),
            S(-62,  41),   S(-57,  38),   S(-43,  34),   S(-49,  20),   S(-36,  17),   S(-60,  17),   S(-86,  42),   S(-65,  30),
            S(-68,  71),   S(-39,  48),   S(-30,  55),   S(-42,  36),   S(-47,  45),   S(-38,  50),   S(-10,  15),   S(-21,  16),
            S(-75,  73),   S(-50,  69),   S(-25,  70),   S(-34,  58),   S(-26,  55),   S( -6,  58),   S(-47,  68),   S(-11,  30),
            S(-10,  57),   S(-50,  69),   S(-39,  65),   S(-25,  50),   S( 14,  49),   S(  0,  54),   S(-25,  66),   S(-42,  63),
            S(-40,  63),   S(-27,  56),   S(  7,  33),   S( -2,  40),   S(  4,  51),   S( 37,  35),   S( 21,  38),   S(-34,  49),
            S(-71,  71),   S(-62,  80),   S(-18,  65),   S(  1,  56),   S( 28,  43),   S( 72,   7),   S(-31,  60),   S(-60,  75),
            S(-47,  78),   S(-11,  59),   S(-48,  67),   S(  4,  39),   S(-24,  61),   S(  5,  51),   S(-55,  81),   S( 19,  43),

            /* rooks: bucket 3 */
            S(  8,  70),   S( 11,  67),   S( 21,  61),   S( 22,  50),   S( 19,  49),   S(  2,  66),   S( 11,  73),   S( -3,  47),
            S(-16,  80),   S(  1,  61),   S( 12,  65),   S( 18,  59),   S( 28,  54),   S( 17,  59),   S( 47,  14),   S( 28, -45),
            S(-22,  79),   S(  1,  80),   S( 16,  83),   S( 16,  65),   S( 29,  71),   S( 33,  66),   S( 50,  57),   S( 13,  57),
            S(-18,  87),   S(-25,  97),   S( 35,  78),   S( 39,  67),   S( 36,  71),   S(  8, 111),   S( 62,  64),   S( 24,  83),
            S(  3,  96),   S( 35,  78),   S( 37,  65),   S( 48,  64),   S( 63,  66),   S( 59,  71),   S( 99,  59),   S( 64,  48),
            S( -6, 106),   S( 18,  76),   S( 27,  86),   S( 26,  67),   S( 41,  55),   S( 57,  42),   S( 93,  39),   S( 86,  40),
            S(-16, 103),   S(-22, 111),   S( -1, 101),   S( 18,  91),   S( 39,  79),   S( 69,  62),   S( 62,  85),   S(145,  20),
            S(-49, 145),   S( 12, 105),   S( 24,  84),   S( 47,  70),   S( 68,  53),   S( 71,  59),   S(148,  40),   S(127,  41),

            /* rooks: bucket 4 */
            S(-95,  28),   S(-25,  -2),   S(-59,   5),   S( -2,  10),   S(-28, -17),   S(  4, -41),   S( 12, -35),   S(-10, -31),
            S(-30, -23),   S(-40,  19),   S(-55,  23),   S(-17,   8),   S(-65,   2),   S(  5, -43),   S( 21, -63),   S(-54,  -4),
            S(  7,  25),   S(-17, -14),   S(-36,  14),   S(-32, -12),   S(-19,   3),   S(-11, -17),   S( 33, -24),   S(-87,  -5),
            S(-53, -27),   S(-20,   9),   S(-22,  33),   S( 39,   1),   S( 17, -13),   S( -3,  -2),   S( -7,  18),   S(  3,  23),
            S(-28,   6),   S(-12,  20),   S( -3,   9),   S( 57,  12),   S( 16,  -7),   S( -4,  12),   S( 26,  31),   S( 45,  -7),
            S( 14,  -7),   S( 17,  12),   S( 63,   3),   S(  5,  27),   S( 45,   6),   S( 24,  24),   S( 25,   1),   S( 26,  22),
            S(  6, -13),   S(  9,  48),   S( 33,   6),   S( 57,  20),   S( 53, -22),   S( -5,   1),   S( 42,   3),   S( 18,  17),
            S( 12, -50),   S( 48,  48),   S( 23,  10),   S( 26,  -8),   S( 27, -16),   S(  9,   9),   S( 17,  -9),   S( 27,  13),

            /* rooks: bucket 5 */
            S(-37,  31),   S(-12,  33),   S(-51,  52),   S(-16,  -9),   S(-15,  15),   S(  4,  12),   S( 29,   4),   S(-26,  34),
            S(  0,  20),   S(-48,  42),   S(-57,  50),   S(-61,  41),   S(-54,  42),   S(-15,  19),   S( 33, -10),   S(  6,  -4),
            S(-43,  49),   S(-52,  53),   S(-74,  58),   S(-63,  56),   S(-55,  33),   S( 13,  23),   S(-31,  37),   S(-17,  17),
            S(-53,  64),   S( 12,  29),   S(-11,  58),   S( -5,  41),   S(-23,  60),   S( -2,  55),   S( -6,  48),   S( -9,  28),
            S(  0,  64),   S(  0,  63),   S( -3,  55),   S( 15,  60),   S( 46,  48),   S( 19,  75),   S( 55,  47),   S( 15,  39),
            S( 44,  66),   S( 43,  51),   S( 56,  56),   S( 19,  59),   S( 49,  53),   S( 86,  47),   S( 24,  62),   S( 72,  31),
            S( 33,  44),   S( 31,  54),   S( 50,  55),   S( 39,  53),   S( 65,  46),   S( 65,  32),   S( 69,  33),   S( 54,  39),
            S( 66,  40),   S( 54,  31),   S( 44,  56),   S( 15,  37),   S( 41,  39),   S( 58,  45),   S( 50,  27),   S( 23,  42),

            /* rooks: bucket 6 */
            S(-48,  25),   S(-11,  17),   S(  5,  14),   S(-20,  15),   S(-18,  17),   S(-61,  50),   S(-14,  45),   S(  3,  28),
            S(-37,  24),   S( -7,  24),   S(  1,  18),   S(-58,  37),   S(-32,  38),   S(-61,  57),   S(-40,  38),   S( 35,  -1),
            S(-43,  51),   S(-60,  53),   S(-18,  44),   S(-67,  55),   S(-22,  37),   S(-60,  85),   S(-60,  79),   S( 35,   7),
            S(-55,  68),   S( -3,  49),   S( -8,  60),   S(-23,  53),   S(-40,  54),   S( 24,  50),   S(-74,  73),   S(-19,  58),
            S(-39,  80),   S( 19,  61),   S( 51,  39),   S( 28,  36),   S(  2,  71),   S( 22,  63),   S( 12,  66),   S( 18,  47),
            S( -4,  73),   S( 55,  53),   S( 74,  43),   S( 46,  25),   S( 22,  58),   S( 51,  64),   S( 68,  53),   S( 38,  49),
            S( 30,  61),   S( 57,  45),   S( 82,  28),   S( 60,  34),   S(100,  25),   S( 33,  56),   S( 95,  44),   S( 28,  55),
            S( 59,  60),   S( 67,  40),   S( 82,  30),   S( 31,  50),   S( 85,  39),   S( 70,  34),   S( 79,  38),   S( 29,  52),

            /* rooks: bucket 7 */
            S(-56,  -5),   S(-13, -16),   S( -3, -21),   S( -7, -12),   S( 19, -21),   S(  1,  -1),   S(-47,  24),   S( 19, -20),
            S(-83,  32),   S(-42,   9),   S(-37,   7),   S(  1, -13),   S( 21,  -6),   S( 35,  -5),   S(-22,   9),   S(-16,  -9),
            S(-91,  49),   S(-50,  20),   S( -9,  17),   S( 11, -16),   S( 23,   1),   S(-26,  15),   S( -9,  -8),   S( 29,  -7),
            S(-61,  42),   S(-17,  28),   S(  8,  24),   S( 45,  10),   S( 22,  16),   S( 51,   5),   S( 52, -10),   S(-17,  17),
            S(-21,  40),   S(-45,  37),   S( 41,  -3),   S( 39,   4),   S( 84,   4),   S( 94,   1),   S( 34,  35),   S( 30,  -4),
            S( -5,  43),   S(-10,  24),   S( 83, -12),   S( 62,   0),   S( 84,  -3),   S( 82,   7),   S( 56,  30),   S( 46, -10),
            S(  4,  35),   S( 51,  13),   S( 71,   7),   S(113,  -5),   S(114, -12),   S( 68,  14),   S( 50,  23),   S( 89, -20),
            S(  0,  66),   S(  3,  34),   S( 29,  20),   S( 87, -19),   S( 14,  21),   S( 36,  20),   S( 67,  -2),   S( 46,   6),

            /* rooks: bucket 8 */
            S(-46, -32),   S(-28,   7),   S(  6,   5),   S(-28,  -8),   S(-17, -17),   S(-44, -64),   S(-31, -43),   S(-36,   8),
            S(-15, -31),   S(-23, -31),   S(-32, -11),   S(  9, -24),   S(-27,  -5),   S( -3, -12),   S( -1,  -8),   S(-23, -94),
            S(  8,   4),   S( -3, -13),   S( 17,  23),   S(-10,   1),   S(-11, -32),   S(-24, -32),   S(  0,  25),   S( -9, -22),
            S(-13, -15),   S( -6,  31),   S( -2,   6),   S( -5,   2),   S(  9,  23),   S(-28, -32),   S( -5, -24),   S(-17, -13),
            S(-15, -17),   S( -1,   2),   S(-13,  35),   S( -9,  19),   S(-13, -19),   S(  8,  -2),   S(  3,  -2),   S(  0, -38),
            S( -2,  25),   S(-12,  17),   S(  6,  21),   S( 33,  30),   S(-22, -42),   S( -4, -33),   S(  4,   6),   S(  8,  47),
            S(  2,  11),   S(-22,  12),   S( 17,  37),   S(  8,  -3),   S( 16, -10),   S(-10, -42),   S( 23,   0),   S(  5,   7),
            S( -3, -96),   S(  8,   1),   S(  8,  10),   S(-13, -30),   S( -2, -17),   S( 10, -43),   S(  2,  15),   S(  7,  28),

            /* rooks: bucket 9 */
            S(-43, -39),   S(  3, -55),   S(-47, -55),   S(-68, -29),   S(-47, -10),   S(-29, -24),   S(-18, -36),   S(-43, -38),
            S( 14, -17),   S(-22, -65),   S(-18, -32),   S(-17, -31),   S(-41, -36),   S( 11, -20),   S( -9, -45),   S(-32, -41),
            S( -1, -46),   S( 19, -29),   S(  0,  -5),   S(-36, -19),   S(-38, -11),   S(  9, -24),   S(  1, -16),   S(-11, -25),
            S(-10, -31),   S( -5, -18),   S( -7, -10),   S(-39, -11),   S(-10, -30),   S( -6, -23),   S(  6,  15),   S( -8, -20),
            S( -2,   6),   S(-18,  -3),   S( -7,  -1),   S( -9,   2),   S( 18,  28),   S( 12,  -7),   S(  6,  -1),   S(  2, -43),
            S(  6,  24),   S(-13,  -6),   S( -4,  -3),   S(-20,   1),   S(  3, -36),   S( 13,   2),   S(  7,  -4),   S(  8, -17),
            S( 24,  12),   S( 48, -23),   S( 24,  13),   S( 50, -10),   S( 17,  -4),   S( 22, -10),   S( 40, -25),   S( 34, -11),
            S( 54, -77),   S( 19, -33),   S( 19, -16),   S( 20,  14),   S( 16,   8),   S( 15,  -8),   S(  2, -21),   S( 17, -29),

            /* rooks: bucket 10 */
            S(-73, -76),   S(-52, -56),   S(-18, -86),   S(-50, -17),   S(-51, -45),   S(-40, -65),   S( 11, -54),   S(-48, -49),
            S(-22, -27),   S( -7, -62),   S(-28, -65),   S(-55, -25),   S( -9, -38),   S(-23,  -6),   S( 17,   3),   S( -4, -30),
            S(-42, -42),   S(-35, -54),   S(-19, -33),   S( -6, -19),   S(-33,  -8),   S(-23, -21),   S( 19,   2),   S(  6,   0),
            S(-27, -41),   S( 18, -19),   S(-31, -35),   S(-13,   2),   S( -6, -15),   S( -4, -35),   S( 15,  15),   S( -4, -31),
            S(  0, -19),   S(-10, -19),   S(  3,  -9),   S(  5, -29),   S(-13, -30),   S(  6,   9),   S( 21, -10),   S( -8,  -3),
            S( 22,   2),   S( 33,  18),   S( -3, -16),   S(  1,  -8),   S( -9, -20),   S( 10,  11),   S( 20, -20),   S( -9, -15),
            S( 68, -19),   S( 66, -32),   S( 59, -20),   S( 42, -32),   S( 27, -23),   S( 22,   5),   S( 14, -26),   S( 37, -37),
            S( 39,  -8),   S( -1, -31),   S( 20,  -7),   S(  8,   5),   S( 31,  -6),   S( 17,  -1),   S( 18, -28),   S( 10, -29),

            /* rooks: bucket 11 */
            S(-61, -56),   S(-49, -33),   S(-34, -38),   S(-39, -71),   S(-17, -21),   S(-13,  10),   S(-42, -26),   S(-53, -40),
            S(-45, -27),   S( 15, -43),   S(-22, -40),   S(-45, -21),   S(-34, -14),   S(-24, -20),   S(-11, -27),   S(-22,   7),
            S(-31, -45),   S( 14, -21),   S( -5,  -9),   S(-11, -35),   S(  0, -25),   S(-47, -17),   S(-20, -19),   S(-40, -69),
            S(-18,  26),   S(-10, -20),   S(-14, -17),   S(  6,   6),   S( -3, -18),   S(-13,  42),   S( 21,  -1),   S(-11, -56),
            S( -4,  11),   S( 16, -37),   S( 23, -13),   S( 21, -32),   S( 23,  15),   S( 23,   1),   S(  6, -20),   S(-24, -55),
            S(  6,  38),   S( 33,   9),   S(  4, -22),   S( 42,  -6),   S( 31,   9),   S( 31,   5),   S(-28, -11),   S( -8,  -7),
            S( 38,  29),   S( 14,  24),   S( 40, -15),   S( 63, -11),   S(  3,  -4),   S( 39,   2),   S( 19,  16),   S( 42,  10),
            S( 37,  39),   S(  4,  10),   S( 20,  12),   S(  7, -36),   S( -6, -20),   S(  3, -28),   S( 18,  23),   S( 20,   6),

            /* rooks: bucket 12 */
            S( -7, -79),   S(-20, -55),   S(-12, -25),   S(-13, -39),   S(  1, -34),   S(  5, -14),   S(-31, -74),   S(-15, -41),
            S( -4, -31),   S(  7,   6),   S(-13, -10),   S( -5,  -6),   S(-10,  -1),   S( -6, -24),   S( -4, -23),   S( -8, -44),
            S( 11,  13),   S(-17, -36),   S(-17, -26),   S(-15, -25),   S(-10, -40),   S( -3,  -5),   S( -4, -17),   S( 10,  23),
            S(  5,  -5),   S( -1, -16),   S(  5,  -8),   S(  0, -22),   S( -4, -13),   S(-16, -51),   S(-12, -41),   S( -5, -22),
            S( -9, -22),   S(-13,  -4),   S( 26,  33),   S( 10,  18),   S(-11, -36),   S(  5,  -5),   S(-12, -44),   S(  0,  -8),
            S(-10, -30),   S(  4, -28),   S( 22,  24),   S( -1, -32),   S( -6, -18),   S( -3, -32),   S( -4, -37),   S( 12,  18),
            S(-12, -24),   S(-12, -23),   S( -6,  -4),   S( 15,  10),   S(  1, -39),   S( -7, -48),   S( -2, -24),   S(  5,   3),
            S( -2, -22),   S(  4,   2),   S(  3, -17),   S(  8,  -7),   S( -4, -30),   S( -7, -34),   S(-14, -58),   S( 14,  24),

            /* rooks: bucket 13 */
            S( -7, -45),   S( -3, -38),   S(-13, -10),   S(-27, -12),   S( 12,  27),   S(-11, -26),   S( -9, -30),   S(-19, -10),
            S( -9, -27),   S(  2,   1),   S(-10,  20),   S(-12, -12),   S(-18, -53),   S( -9, -25),   S(-13, -40),   S( -2, -14),
            S(-10, -44),   S( -2,   1),   S(-17, -46),   S(  0,  -5),   S( -1,  32),   S( -2, -11),   S( -4, -12),   S( -6, -41),
            S(-11, -27),   S( -3, -15),   S(-22, -50),   S(-22, -42),   S( 13,  13),   S(-17, -63),   S( -2, -24),   S( -7, -12),
            S(  2, -14),   S( 12,  10),   S( 22,  31),   S(  0, -26),   S(-13, -16),   S( -5, -25),   S( -6, -51),   S(  2,  -9),
            S(-19, -38),   S( -1,  -9),   S(-17, -33),   S(  6, -12),   S( -2, -39),   S( 12,  15),   S(  0,  -5),   S(  9,  18),
            S(  4,   9),   S( -3,  -4),   S(  0,   4),   S( -9, -22),   S( 11,  14),   S( 22,  48),   S(  2, -25),   S(  0,  14),
            S(-28, -130),  S(-19, -65),   S(  8,  16),   S( -6, -26),   S( -8, -18),   S(-11, -53),   S(-21, -72),   S(  1,   1),

            /* rooks: bucket 14 */
            S(-12, -31),   S(-19, -50),   S(  1,  -3),   S(-12, -36),   S( -5,  -5),   S( -4,   0),   S( -9, -73),   S(-10, -29),
            S(-24, -49),   S(-27, -63),   S( -1,  29),   S(-23, -44),   S(-19, -31),   S(-12, -33),   S(  2,   0),   S( -4, -22),
            S(-17, -52),   S( -8, -23),   S( -9, -29),   S(-12, -29),   S(-19, -33),   S(-12, -25),   S( -9, -16),   S(  3,  -6),
            S(  3,  20),   S( -9, -14),   S( -5,   3),   S( -2,   3),   S( -1, -22),   S(  0,  -4),   S( -4, -45),   S( -8, -16),
            S( -4, -52),   S(  2, -26),   S(  3, -28),   S( -8, -52),   S(  3, -21),   S(  5, -44),   S(  6, -20),   S(  0, -12),
            S( -5, -49),   S( -4, -23),   S(  3, -42),   S(  2, -65),   S( 10, -30),   S(  4, -43),   S(  7, -18),   S(  0, -21),
            S(  6, -14),   S(  2, -42),   S(  7, -29),   S(  8, -47),   S(  5, -47),   S(  5, -17),   S( 14,   8),   S( -3, -18),
            S( -8, -36),   S(-11, -28),   S(-17, -63),   S(  6, -30),   S( -5, -17),   S(  6,  17),   S(  2,  -5),   S( -6, -23),

            /* rooks: bucket 15 */
            S(-15, -11),   S(-26, -75),   S(  7, -12),   S(  0,  11),   S( -6, -15),   S(-18, -30),   S(-13, -48),   S( -9, -29),
            S(-26, -35),   S(-26, -59),   S( -2,   7),   S( -8, -25),   S(-21, -39),   S(-10, -26),   S(-16, -59),   S(  8,  17),
            S( -7, -19),   S(-10, -29),   S( -7, -18),   S( -2, -14),   S(  5,  -5),   S(-10, -34),   S( -3, -13),   S(-10, -43),
            S( -3, -32),   S(  2, -25),   S( -3, -15),   S( -8, -32),   S(-13, -35),   S(-11, -43),   S(  2, -21),   S( -9,  27),
            S( -7, -23),   S( -6, -21),   S(  4, -20),   S(  6, -11),   S(  2, -17),   S(  1, -28),   S( -5, -17),   S( -5,   7),
            S(  1, -10),   S( -6, -18),   S(  6, -44),   S(  0, -47),   S(  1, -39),   S(  5, -37),   S(  0, -55),   S(-12, -28),
            S( 11,  17),   S( 14,   8),   S( 13, -28),   S( -4, -50),   S( -1, -33),   S( 22,  36),   S(  8, -15),   S( -7, -15),
            S( -4, -19),   S( -9, -21),   S(-11, -52),   S( -3, -28),   S(  0, -36),   S(  7,   1),   S(  6, -13),   S( -1, -16),

            /* queens: bucket 0 */
            S(-40, -21),   S(-33, -69),   S( 46, -96),   S( 35, -36),   S( 28, -40),   S( 22, -18),   S( 36,  29),   S(  5,  38),
            S(-16, -29),   S( 41, -83),   S( 28,  -6),   S( 27,  -1),   S( 36,  -2),   S( 10,   5),   S( 22,  26),   S( 26,  40),
            S( 29, -15),   S( 36,  -2),   S( 30,  27),   S( 20,  30),   S( 24,   7),   S(  8,  47),   S( -3,  34),   S( 25,  28),
            S( 12,  16),   S( 27,  38),   S( -2,  53),   S( 33,  31),   S( 37,  21),   S( 29,  27),   S(  0,  29),   S( 15,  45),
            S( 12,  71),   S( 44,  25),   S( 22,  12),   S( 14,  82),   S( -9,  27),   S( 17,   2),   S(  9,  20),   S( 41,  -7),
            S( 18,  48),   S( 42,  50),   S( -7,  49),   S( 19,  29),   S( 50,  -8),   S( 32,  19),   S( 51,   9),   S( 24, -15),
            S( 19,  29),   S( 52,  21),   S( 34,  39),   S( 63,  29),   S(  0,  24),   S( 10,  -8),   S( 10,  28),   S( 36,  17),
            S( 37,  33),   S( 50,  47),   S( 53,  33),   S( 71,  70),   S( 69,  59),   S(-21,   5),   S( 67,  29),   S( 78,  11),

            /* queens: bucket 1 */
            S( -2, -22),   S(-67, -34),   S(-80, -28),   S( -6, -103),  S(  0, -29),   S(-10, -31),   S( 26, -37),   S(  4,  11),
            S(-12, -23),   S(-14, -40),   S( 18, -41),   S(  4,  24),   S( 14, -19),   S( -1,   6),   S( 24,  -6),   S( 19,   4),
            S(-22,  25),   S(  5, -17),   S(  8,  13),   S( 16,  -3),   S(  9,  46),   S(  4,   4),   S( 21,  -2),   S( 27,  35),
            S( 11, -26),   S( -8,  38),   S(  0,  63),   S( 34,  26),   S( 12,  49),   S( 22,   7),   S(  6, -20),   S( 19,  -2),
            S( 32, -12),   S( 13,  -1),   S(  7,  79),   S(-17,  63),   S( -6,  67),   S( 17,  33),   S(  3,   6),   S( -6,  43),
            S( 16,  47),   S( 27,  56),   S( 21,  67),   S(-33,  92),   S(-14,  80),   S(-21,  41),   S( 51,  51),   S( 26,  27),
            S(-23,  31),   S(-32, 106),   S( -2,  20),   S(-26,  66),   S( -8,  67),   S( 26,  71),   S( 17,  35),   S(-21,  43),
            S( -7,   0),   S( 26,  35),   S( 48,  44),   S(-18,  21),   S(  3,  39),   S(-14,  21),   S( 11,  40),   S( 10,  32),

            /* queens: bucket 2 */
            S(  2,  35),   S( 19, -63),   S( 15, -25),   S(  6, -23),   S( -5, -11),   S(-31,   6),   S(-36, -15),   S(  2,  59),
            S( 22,   1),   S( 26,  13),   S( 24, -16),   S( 33, -23),   S( 22, -18),   S( 29, -55),   S( 25, -15),   S( 33, -38),
            S( 20,   3),   S( 19,  15),   S( 20,  26),   S( 16,  37),   S( 19,  43),   S( 24,  61),   S( 15,  18),   S( 29,  23),
            S( 19,  -3),   S(  8,  48),   S( 10,  40),   S( 23,  52),   S( -6,  87),   S(  1, 105),   S( 12,  29),   S(  8,  44),
            S( 16,  -3),   S( -9,  65),   S(-18,  72),   S(-20, 113),   S(-28, 117),   S(-32, 119),   S(-19, 125),   S( -6, 120),
            S( 15,   8),   S(  9,  48),   S(-23, 102),   S( -8,  67),   S(-33, 123),   S(-44, 101),   S( 11,  94),   S( -1,  66),
            S(-39,  62),   S(-40, 103),   S(-16,  76),   S( 39,  71),   S(-17,  77),   S( 34,  47),   S(-37,  57),   S(-21,  71),
            S(-58,  81),   S( 14,  55),   S( 38,  43),   S( 77,  39),   S( 18,  65),   S( 33,  18),   S(  9,  38),   S(-56,  64),

            /* queens: bucket 3 */
            S( 77,  91),   S( 56,  87),   S( 48,  94),   S( 49,  91),   S( 70,  31),   S( 52,  18),   S( 20,   9),   S( 41,  52),
            S( 62, 118),   S( 64, 105),   S( 50, 101),   S( 56,  82),   S( 55,  76),   S( 61,  47),   S( 77,   2),   S( 22,  59),
            S( 56, 101),   S( 51, 105),   S( 61,  80),   S( 56,  74),   S( 61,  78),   S( 66,  90),   S( 61,  88),   S( 62,  60),
            S( 48, 113),   S( 52, 103),   S( 56,  86),   S( 53,  97),   S( 53,  91),   S( 51, 128),   S( 63,  98),   S( 40, 144),
            S( 50, 117),   S( 53, 114),   S( 46, 108),   S( 46, 106),   S( 37, 133),   S( 34, 129),   S( 44, 149),   S( 48, 153),
            S( 45, 133),   S( 51, 123),   S( 52, 106),   S( 32, 130),   S( 46, 138),   S( 73, 103),   S( 50, 165),   S( 34, 201),
            S( 65, 110),   S( 61, 110),   S( 77,  89),   S( 59, 126),   S( 34, 135),   S( 68, 101),   S( 57, 154),   S(154,  73),
            S( 71,  97),   S( 93, 108),   S( 86,  98),   S( 84, 101),   S( 47, 103),   S( 86, 101),   S(127,  72),   S(141,  57),

            /* queens: bucket 4 */
            S(-30, -33),   S(-22, -15),   S(-15, -26),   S( 27,  -8),   S(  7, -35),   S( 37, -22),   S(-55, -27),   S(-51, -31),
            S(-17, -16),   S( -6,   0),   S( 23, -12),   S(-32,   8),   S( -5, -16),   S( 40, -26),   S(-48, -35),   S(-48, -35),
            S( -2,  -2),   S(  7,   1),   S( 35,  42),   S( -3,  45),   S( 27,  21),   S( 23,   5),   S( 22,  10),   S(-16, -39),
            S( -2,   6),   S(  0,  32),   S(-13,  67),   S(-20,  25),   S( 31,  43),   S(-12,  28),   S( 16,   4),   S( -9, -10),
            S(-23,   8),   S( 27,  15),   S( 42,  64),   S( 25,  65),   S( 31,  51),   S( 31,   0),   S(-33, -17),   S( 19, -17),
            S(-17,   5),   S( 18,  16),   S( 56,  50),   S( 47,  47),   S( 32,  21),   S( 14,   9),   S(-24, -34),   S(-28, -19),
            S(-33, -35),   S(-16,  27),   S( 24,  42),   S( 29,  47),   S( 12,   1),   S( -3,  13),   S(-20, -43),   S(-39, -43),
            S(-20, -35),   S(-13, -23),   S( 39,  43),   S( -1,  13),   S(-25, -24),   S(-18, -25),   S(-15, -31),   S(-24, -41),

            /* queens: bucket 5 */
            S(-25,  -5),   S(-20, -34),   S(-20, -29),   S(-42, -28),   S(-43, -19),   S(  2, -14),   S(-18,  -6),   S( -8,  -3),
            S(-40, -25),   S(-25,   1),   S(-59, -27),   S(-48,  -5),   S( -9, -41),   S(-21,  -3),   S(-25,  -2),   S(-72, -24),
            S(-16,  19),   S(-34,   6),   S(-49,  30),   S(-21,  28),   S( 32,  80),   S(-10,  20),   S( 15,  12),   S(-12,  17),
            S(-74, -35),   S(-66, -54),   S(-23,  38),   S( -9,  76),   S(  1,  18),   S(-27,  10),   S( 12,   1),   S(-19,  -1),
            S(-17,  11),   S(-10,   5),   S(  3,  47),   S(-13,  55),   S( 47,  64),   S(  0,  17),   S(-16, -26),   S(-12,  -4),
            S(-10,  27),   S(  2,  38),   S(-23,  56),   S( 33,  45),   S( 46,  59),   S( 18,  23),   S(  7,  19),   S(-19, -20),
            S( -3,  -6),   S( 11,  42),   S( 10,  64),   S(-14,  13),   S(  7,  32),   S( 30,  69),   S( 23,  27),   S(-24, -34),
            S( -4,  15),   S( 12,   4),   S(-21,  -8),   S( -5,  47),   S( 14,  28),   S( 18,  41),   S(  6,   4),   S(-22, -31),

            /* queens: bucket 6 */
            S(-11,  12),   S(-29, -38),   S(-33, -42),   S(-88, -59),   S(-90, -37),   S(-59, -49),   S(-36, -59),   S( 15,  15),
            S(-47, -11),   S(-44,   8),   S(-52,  15),   S(-28,  19),   S(-80,  22),   S(-82,  11),   S(-107, -38),  S(-22,  20),
            S(-57,  -7),   S( -2,  -6),   S(-24,  32),   S(-105,  93),  S(-23,  60),   S(-40,  15),   S(-28,   2),   S( 32,  41),
            S(-31,  13),   S(-16,   2),   S(-18,  44),   S(-39,  78),   S(  0,  73),   S(  6,  40),   S( -7,  11),   S( 34,  14),
            S(-38,  16),   S( -9,  18),   S(-15,  58),   S( 22,  49),   S( 44,  77),   S( 54,  47),   S( 30,  34),   S( -8,  20),
            S(-25,  25),   S(  5,  13),   S( 47,  32),   S( 38,  60),   S( 23,  71),   S( 68,  91),   S( -2,  18),   S(-48, -23),
            S( -2,  21),   S( 34,  15),   S(-23,  46),   S(  6,  61),   S( 43,  80),   S( 21,  77),   S(-28, -10),   S(-36,  -8),
            S( 17,  40),   S( 14,  46),   S( 20,  38),   S(  2,  16),   S( 45,  49),   S( 21,  39),   S(-20,  26),   S(  1,  10),

            /* queens: bucket 7 */
            S( -4,  -4),   S(-26,  -5),   S(-52,  25),   S(-26,   4),   S(-33,   6),   S(-47, -10),   S(-19,  16),   S(-16,  -4),
            S(-67,   8),   S(-66,  14),   S(-24,   8),   S(-25,  32),   S( -4,   3),   S(-30,  27),   S(-62,  41),   S(-12, -42),
            S(-56,   6),   S(-64,  12),   S(-19,  17),   S(  5,  21),   S(  6,  45),   S( 12,  25),   S( -7,  -6),   S( -7, -15),
            S(-49, -14),   S( 39,   3),   S(-27,  31),   S( -8,  40),   S( 32,  32),   S( 34,  44),   S( -6,  71),   S( -6,  25),
            S(-58,  19),   S(-66,  38),   S(-18,  36),   S( 58,   5),   S( 64,  18),   S( 63,  12),   S( 58,  26),   S( 11,  -1),
            S(-28,  43),   S( -3,  31),   S( -4,   1),   S( 31,  13),   S( 61,  44),   S( 79,  44),   S( 76,   0),   S( 54,  20),
            S(  5, -13),   S( -1,   9),   S( 16,   0),   S( 60,  18),   S( 42,  24),   S( 66,  27),   S( 59,  10),   S( 56,  33),
            S( 23,  20),   S(  9,   2),   S( 18,  32),   S( 53,  25),   S( 48,  31),   S( 29,  15),   S(  5,   5),   S( 38,  40),

            /* queens: bucket 8 */
            S( -8, -12),   S( -3,  -6),   S(-11,  -1),   S( -1,  -1),   S( -4,  -8),   S(  4,   1),   S(-25, -46),   S( -3,  -2),
            S( -2,  11),   S(-12, -17),   S( -4,   0),   S(-14,  -1),   S(  7,  10),   S(-19, -35),   S(-31, -71),   S(-17, -36),
            S( -1,  -4),   S(-29, -30),   S(-15, -29),   S( -4,  -7),   S(-17, -32),   S( -9,  -5),   S(-23, -41),   S(-26, -46),
            S( -8,  -4),   S( -3,  -6),   S( 14,  26),   S( 10,  26),   S( -2,   8),   S( -2,   7),   S(  4,  12),   S(-13, -44),
            S( 14,  22),   S(  3,  45),   S( 13,  34),   S( 15,  42),   S( 25,  47),   S(  0,  -1),   S( -4, -11),   S(-23, -41),
            S( -9,  -5),   S(  1,   5),   S(-39,   8),   S( -1,  32),   S(  0,  -2),   S( -7, -18),   S(  3,  10),   S(  4,   9),
            S(-11, -19),   S(-37, -35),   S( 22,  45),   S( 12,  18),   S(-11,  -5),   S( -3,  -7),   S( -4,  -5),   S( -2,  -5),
            S(-12, -18),   S( -5, -21),   S(-28, -55),   S(  7,  -1),   S(-12, -25),   S(-11, -30),   S(-11, -33),   S( -2, -14),

            /* queens: bucket 9 */
            S( -5, -11),   S(-21, -46),   S(  5,   2),   S(-23, -28),   S(-22, -41),   S(-16, -29),   S(-15, -29),   S(-20, -27),
            S( -2, -19),   S(-19, -39),   S(-16,  -8),   S( -8, -25),   S(-33, -43),   S(-16, -21),   S( 12,  -3),   S( -3,  -8),
            S(  4,   6),   S( -9,  -5),   S(-11,  18),   S(-20, -32),   S(-29, -30),   S(-13,  -9),   S(  0,   2),   S(-15, -28),
            S(-13, -13),   S(-13, -19),   S( 14,  42),   S( -9,  -6),   S(  7,  18),   S( -5, -13),   S(-12, -15),   S(-16, -43),
            S(  2,  13),   S(  9,  32),   S( 17,  50),   S( 23,  59),   S( 14,  30),   S( 11,  21),   S(  4,  10),   S(-21, -20),
            S(-26, -41),   S(-27, -20),   S( -3,   8),   S( 31,  57),   S( 10,   7),   S( -7, -10),   S(-21, -28),   S( -3,  -5),
            S(-20, -34),   S(-16, -35),   S(-22,  -3),   S(  8,  22),   S(  9,  17),   S(-11, -32),   S(  1,  -1),   S(-27, -46),
            S( -8, -26),   S(-22, -47),   S(  8,  29),   S( 12,  25),   S( 11,  11),   S(-16, -23),   S(  6,   9),   S( -2,  -5),

            /* queens: bucket 10 */
            S( -5, -17),   S( -2,   1),   S(-18, -34),   S(-21, -34),   S(-13, -28),   S(-10,  -6),   S(  1,  -3),   S( -8, -13),
            S( -5, -23),   S(-18, -30),   S(-16, -21),   S(-30, -36),   S( -4,   7),   S(-24, -29),   S(  3,  -3),   S(-21, -33),
            S( -6, -17),   S( -6, -18),   S(-34, -30),   S(-21,  -6),   S(-45, -25),   S(-25,  -5),   S(-16, -16),   S(  3,  -2),
            S(-10, -13),   S(  1,   0),   S(-10,   0),   S(-12,  -1),   S(  2,  21),   S(-33, -10),   S( -6, -10),   S(-23, -35),
            S( -3,  -6),   S( 11,  17),   S(-11,  -3),   S( 28,  54),   S( -3,   0),   S(  4,  16),   S(  5,   8),   S(-12, -23),
            S(-10,  -9),   S(-17, -38),   S(-22, -15),   S(-21,  16),   S(  3,  23),   S( 13,  29),   S( -4, -10),   S( -6,  -4),
            S(-10,  -6),   S( -8,  -6),   S(  8,  26),   S(-17,  -6),   S(-15, -19),   S(  4,  14),   S(-20, -29),   S(-17, -28),
            S(  8,  13),   S(-13, -25),   S(-10, -17),   S(  0,  12),   S( 11,  11),   S(-16, -23),   S(  5,  17),   S( -1,  -8),

            /* queens: bucket 11 */
            S(-14, -31),   S( -3, -15),   S(-31, -32),   S(-16, -36),   S( -2,   2),   S(-35, -43),   S(-18, -26),   S(-24, -48),
            S(-14, -21),   S( -9, -24),   S(-41, -56),   S(-30, -27),   S(-12,  -6),   S( -8,  -4),   S(  1,   7),   S( -3,   7),
            S(-28, -42),   S(-12, -22),   S(-25, -50),   S(-23, -49),   S( -4,  14),   S(  4,  13),   S( 12,  31),   S(-21, -20),
            S(-23, -40),   S(-28, -40),   S(-11, -10),   S( 13,  53),   S( 16,  11),   S(-12,  -3),   S(  7,  25),   S(-13, -12),
            S( -6, -18),   S(-10, -22),   S( -8, -22),   S( 26,  22),   S(  3,   3),   S( 32,  65),   S( 26,  35),   S(  2,   9),
            S(-24, -39),   S(  2,  13),   S(-14,  -1),   S(  7,   2),   S( 26,  40),   S( 47,  55),   S( 13,   7),   S(-10,  -8),
            S(  5,  14),   S(-15, -26),   S( 25,  39),   S( -9,  -8),   S( 14,  24),   S( -4,  23),   S( 36,  38),   S( -5, -27),
            S(-19, -26),   S( -6, -23),   S(-11, -20),   S(-13, -21),   S(  6,   1),   S( -6, -25),   S( 13,  12),   S(-24, -55),

            /* queens: bucket 12 */
            S(  7,  18),   S(  1,  -4),   S(  0,   3),   S(-10,  -5),   S( -9, -20),   S( -8, -12),   S(  0,   3),   S( -8, -13),
            S( -9, -21),   S( -5,  -8),   S(-16, -26),   S(-11, -29),   S( -7, -12),   S( -8, -14),   S(  0,   0),   S( -9, -15),
            S( -2, -10),   S(  1,   4),   S( 12,   8),   S( -6, -14),   S( -3,  -8),   S(-19, -32),   S(-18, -41),   S(-12, -23),
            S( 11,  22),   S( -5,  -4),   S( 12,  23),   S( 11,  21),   S(  6,   3),   S( -7, -12),   S( -2,  -3),   S(  3,  -4),
            S(  3,  -2),   S( 12,  16),   S( 28,  57),   S(  9,  11),   S( -1,   8),   S( -1,   3),   S(-21, -38),   S( -1,   0),
            S( 10,  21),   S(  3,   9),   S( 30,  60),   S( -2, -21),   S(  4,   8),   S( -2,  -7),   S(  1,  13),   S( -4, -13),
            S( -3,  -9),   S(  7,   9),   S( 30,  61),   S( -2,  -5),   S( -2,   0),   S(  2,   1),   S(  3,   6),   S(-13, -26),
            S( -5, -24),   S(-21, -39),   S(-18, -27),   S(  3,   6),   S( -1,   4),   S(  0,   3),   S( 10,  19),   S( -6,  -5),

            /* queens: bucket 13 */
            S( -8, -21),   S(-11, -27),   S(-11, -24),   S(-12, -21),   S( -4, -11),   S( -8, -25),   S(-13, -29),   S( -6, -16),
            S( -1,   0),   S(  7,  17),   S( 11,  24),   S(-14, -29),   S( -6,  -9),   S( -2,  -2),   S( -3, -10),   S( -6, -14),
            S( -4,  -7),   S( -3,  -7),   S( -5, -14),   S( -3, -18),   S(  1,   2),   S(-12, -25),   S( -3,   0),   S(-17, -29),
            S(  0,   7),   S( -3, -14),   S(  8,  10),   S( 10,  12),   S( 19,  29),   S( -5, -15),   S(-10, -26),   S(-12, -29),
            S( -9, -21),   S(  0,  15),   S( 10,  33),   S( 16,  33),   S( 24,  48),   S(-12, -30),   S( -6, -22),   S(-11, -22),
            S(  3,   2),   S( 21,  37),   S( 47,  87),   S( 12,  40),   S(-10,   2),   S( -6, -10),   S( 12,  22),   S( -5, -12),
            S( -7, -11),   S(  5,  14),   S( 20,  44),   S( 21,  49),   S(  8,  28),   S( 11,  23),   S(  5,   5),   S(  4,  10),
            S(-22, -45),   S( -6, -24),   S(-11, -24),   S(-24, -36),   S( 12,  14),   S(  3,   1),   S(-15, -28),   S(-12, -23),

            /* queens: bucket 14 */
            S(  0,   0),   S( -6, -20),   S(  1,  -3),   S( -7,  -8),   S(  1,   5),   S( -9, -23),   S( -6, -12),   S(-12, -30),
            S( -8, -22),   S(  0,  -1),   S(-10, -26),   S(  1,   2),   S(-13, -30),   S(-16, -44),   S(  2,   2),   S( -6, -18),
            S(  5,   7),   S(-13, -29),   S(-10, -11),   S(-13, -22),   S( -4,  -5),   S( -4, -17),   S( -1,  -1),   S(-17, -26),
            S(-11, -19),   S( 13,  21),   S(-12, -23),   S( 21,  50),   S( 16,  23),   S(  2,  10),   S( 14,  25),   S(  3,  -4),
            S(  2,   6),   S( -6, -10),   S(-22, -27),   S(  5,  -4),   S(  7,  19),   S( 18,  30),   S(  8,   3),   S(-11, -25),
            S( -7, -13),   S(  2,  -4),   S( 22,  42),   S( 10,  20),   S( 12,  30),   S( 30,  59),   S( -1, -15),   S( -5, -15),
            S(  2,   6),   S(  5,  -5),   S(  6,  21),   S(  3,  12),   S( 12,  14),   S( 12,  20),   S( 10,  12),   S(  3,   6),
            S( -4,  -9),   S(  5,   5),   S( -1,  -9),   S( 12,  10),   S(  0,  12),   S(  2,  -2),   S(-13, -31),   S(-27, -55),

            /* queens: bucket 15 */
            S( -5,  -9),   S( -7, -25),   S( -5, -10),   S( -7, -17),   S( -6, -10),   S(  1,   2),   S( -7, -19),   S( -3,  -9),
            S( -1,  -6),   S( -5, -13),   S( -1,   1),   S( -4,  -7),   S(  1,   1),   S(  3,  14),   S(  1,   1),   S(  5,   8),
            S( -7, -17),   S( -4, -14),   S( -5, -14),   S(  0,   2),   S(  1,  -2),   S(  1,   0),   S(  4,  10),   S(  0,  -3),
            S( -5, -10),   S(  2,   7),   S(  4,   9),   S( 16,  28),   S(  3,  -1),   S(-18, -23),   S(  2,   4),   S( -7,  -9),
            S( -1,  -2),   S(-10, -24),   S( -5, -18),   S( -5, -12),   S( -1,  -2),   S( 10,  13),   S( -6, -18),   S(  2,   2),
            S( -5, -16),   S( -8, -20),   S(  3,   1),   S( -9, -20),   S( -8, -20),   S( 23,  44),   S( -7, -11),   S( -7, -19),
            S( -1,   4),   S( -1, -15),   S( 15,  25),   S(  2,   1),   S( 11,  27),   S(  6,   7),   S( 16,  26),   S(  6,  11),
            S( -6, -13),   S(  2,  -3),   S( -6, -19),   S(  6,  12),   S(  4,   1),   S( -5, -20),   S(  1,  -1),   S(-27, -55),

            /* kings: bucket 0 */
            S( 21,  40),   S( 11,  66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  55),   S( 92,  62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(-25,  16),   S(-79,  36),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 29,  36),   S(  5,  44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-46,  37),   S(-42,  29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 30,  44),   S( 22,  42),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 16,  60),   S( -2,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 62,  85),   S( 10,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -43),   S( 33, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-63,  -8),   S(  6,  15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 16, -52),   S(-39, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 18, -25),   S(-16, -18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10, -15),   S(-40, -11),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  -7),   S(-20,  -2),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 34,  14),   S(-14, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 26,  41),   S(-41,  16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94, -50),   S( -4, -15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56, -39),   S( 44, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  5, -47),   S(-34, -47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 66, -53),   S( 32, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 11, -56),   S(-57, -34),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 66, -59),   S( 67, -52),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  6, -24),   S(-102, -48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 75, -37),   S( -4, -60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9, -48),   S( 27, -13),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -109),  S(-21, -19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  8, -20),   S( 71, -39),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 27, -65),   S( 14, -69),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 26, -35),   S( 53, -40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19, -58),   S( 16, -54),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 31, -29),   S(-41, -64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -4, -56),   S(-20, -111),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -11),   S(-41,  11),   S(-12,   1),   S(-20,   4),   S(  4,   3),   S( 24,  14),   S( 30,  -9),   S( 46,  -6),
            S(-15, -32),   S(-36, -14),   S(-20,  -9),   S(-15,   2),   S(  5,   7),   S( -4,   3),   S( 23, -11),   S( 22,  12),
            S(  7, -17),   S( -1, -23),   S( 22, -27),   S(  2, -26),   S( 13,   4),   S( 19,  24),   S(  8,  28),   S( 58,   5),
            S( 26, -21),   S( 45, -11),   S( 58, -27),   S( 31,   7),   S( 17,  44),   S(-11,  77),   S( 24,  62),   S( 76,  46),
            S( 89, -45),   S(115,  -9),   S(106, -29),   S( 33,  14),   S( 46, 136),   S( 21,  82),   S( 14, 146),   S( 80,  97),
            S(-150, -109), S(-118, -116), S( 61, -146),  S( 18,  48),   S( 66, 148),   S(104, 143),   S(144, 100),   S( 54, 166),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,  15),   S(-59,  22),   S(-36,   8),   S(-52,  33),   S(-30,   3),   S( -4,   7),   S( -3,  -2),   S( -2,  25),
            S(-42,  -4),   S(-46,  -4),   S(-48,   5),   S(-30,   6),   S(-10,   6),   S(-19,  -2),   S( -7,  -7),   S(-23,  12),
            S(-23,   6),   S(-24,  15),   S(-31,   1),   S(  1,  -9),   S( -5,  19),   S(-12,   8),   S(-21,  18),   S(  4,  16),
            S(  0,  22),   S( 27,   8),   S(-11,  16),   S( 15,  29),   S( 12,  21),   S(-22,  28),   S(  3,  28),   S( 50,  35),
            S( 40,   8),   S( 54,  -7),   S( 96, -34),   S( 72, -16),   S( 68,   1),   S( 34,  32),   S(-49,  56),   S( 39,  74),
            S(160, -51),   S(-46, -33),   S( 42, -120),  S( 26, -96),   S(-35, -54),   S(-47,  38),   S( 51, 121),   S( 71, 158),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  41),   S(-46,  20),   S(-37,  10),   S(-18,  15),   S(-51,  35),   S(-35,  14),   S(-19,  -5),   S(-23,  21),
            S(-51,  18),   S(-43,  12),   S(-47,   6),   S(-42,  23),   S(-43,  19),   S(-47,   5),   S(-25, -14),   S(-50,   9),
            S(-31,  39),   S(-36,  40),   S(-19,  17),   S(-21,  17),   S(-34,  32),   S(-20,   4),   S(-26,   4),   S(-15,   5),
            S(-16,  72),   S(-27,  58),   S( -7,  33),   S(  5,  27),   S( -9,  30),   S(-18,  13),   S(  6,  13),   S( 35,   2),
            S(-25, 119),   S(-37, 100),   S(-33,  37),   S( 14, -21),   S( 92, -16),   S( 94,  -2),   S( 93, -38),   S( 48,  -5),
            S(  3, 212),   S( 68, 116),   S(-24,  72),   S(-12, -71),   S(-20, -156),  S(-31, -153),  S( -3, -63),   S(118, -56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  22),   S(-16,  18),   S( -9,  16),   S( -9,  35),   S(-23,  54),   S(  7,  36),   S( -6,   9),   S(-10,  -2),
            S( -1,  11),   S( -3,  14),   S(-17,  11),   S(-15,  20),   S( -2,  22),   S(  2,   9),   S( -6,  -4),   S(-33,   0),
            S( 18,  33),   S( -9,  54),   S(  3,  25),   S( -2,   8),   S( 14,   5),   S( 20,  -6),   S(  0, -10),   S( -3,  -4),
            S( 18,  85),   S( -2,  88),   S( 13,  66),   S( 17,  26),   S( 29,   7),   S( 38, -19),   S( 15,  11),   S( 42, -14),
            S( -2, 153),   S(-22, 173),   S(-29, 164),   S(-12, 113),   S( 34,  59),   S( 65,  10),   S( 87, -21),   S( 83, -26),
            S( 65, 138),   S( 38, 230),   S(  7, 252),   S( 22, 179),   S(-11,  64),   S(-14, -135),  S(-103, -177), S(-158, -161),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 97, -18),   S( 12,  -5),   S( -9,  -6),   S( 15, -25),   S(  0, -18),   S( -8, -12),   S(-17,  -5),   S(-58,  28),
            S( 53, -24),   S(  9,   0),   S( 11, -22),   S(-33,   1),   S(-42, -14),   S(-23, -20),   S(-43, -17),   S(-43,  -9),
            S( 99, -30),   S(107, -35),   S( 36, -24),   S(-21, -19),   S(-49,   4),   S(-40,  16),   S(-61,  14),   S(-43,  18),
            S(-14, -83),   S( 45, -117),  S( 39, -17),   S(-31,  22),   S(-33,  20),   S(-21,  41),   S(-18,  43),   S( -6,  44),
            S( 54, -98),   S(-25, -117),  S(-24, -92),   S( 45, -12),   S( 76,  78),   S( 82,  54),   S( 26,  61),   S( 14,  79),
            S( 14, -37),   S(-10, -49),   S( -5, -125),  S( 29,  21),   S( 39,  53),   S( 62, 127),   S( 66, 116),   S( 50, 114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  35),   S(-33,  24),   S( -4,  11),   S( 29,   2),   S( 65, -16),   S( 28, -11),   S(-26,   3),   S(-66,  42),
            S(-61,  15),   S(-10,   6),   S( 15, -17),   S( 26,   0),   S(-11,   5),   S( 27, -22),   S(-54,  -5),   S(-59,  16),
            S(  6,   5),   S(  2,  24),   S( 64,   3),   S( 24,  21),   S(-16,  35),   S(  1,  -1),   S( -4,   2),   S(-48,  29),
            S( 45,   4),   S( 64, -24),   S(  9, -54),   S(-28,  -6),   S( 22, -19),   S(  0,   3),   S( 56,  -3),   S( 24,  25),
            S( 36,  -2),   S( 57, -56),   S( 14, -78),   S(-10, -62),   S( 82, -51),   S( 68,   6),   S( 25,  21),   S(-23,  83),
            S( 56,  34),   S( 32, -42),   S(-15, -86),   S( 19, -84),   S(  6, -72),   S( 82,  26),   S( 76, 116),   S( 96, 110),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-82,  30),   S(-40,   1),   S( -7, -10),   S( -3,   1),   S( -3,   9),   S( 13,   7),   S(  8,  -2),   S(  5,  16),
            S(-70,   6),   S(-36, -11),   S(-26, -12),   S( 38,  -3),   S(-13,  16),   S(  3,  -6),   S(  2,  -6),   S( -5,  -1),
            S(-35,  19),   S(-25,  11),   S(  4,   2),   S(  3,  13),   S( 24,  24),   S( 46,  -2),   S( 39,  -3),   S( 38,  -6),
            S(-17,  46),   S(  1,  21),   S( 28,   1),   S( 18,  -2),   S(-10, -33),   S( 35, -49),   S( 33, -27),   S(104, -33),
            S( 29,  73),   S( 40,  24),   S( 46,  -9),   S( 11, -42),   S(-31, -58),   S(-11, -57),   S( 65, -63),   S(105, -28),
            S(100,  45),   S(122,  43),   S( 77,  11),   S( 42, -74),   S(-10, -107),  S(  4, -69),   S(-13, -46),   S( 59, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62,   6),   S(-47, -20),   S(-11, -22),   S(-49,  31),   S( 33,  -3),   S( 58, -12),   S( 57, -27),   S( 60,  -8),
            S(-51, -14),   S(-51, -16),   S(-36, -24),   S(-35,   4),   S( 12,  -8),   S( 45, -31),   S( 48, -26),   S( 52, -22),
            S(-35,  16),   S(-63,  27),   S(-36,   9),   S(-33,  -8),   S( 13,  -7),   S( 35, -20),   S( 79, -24),   S( 82, -25),
            S(-15,  38),   S(-62,  60),   S(-24,  50),   S(-18,  27),   S( 28, -19),   S( 42, -51),   S(  5, -51),   S( 34, -97),
            S( 15,  60),   S(-48, 115),   S( 47,  84),   S( -7,  79),   S(  9,  25),   S(-21, -49),   S(-13, -149),  S( -4, -101),
            S(116,  88),   S( 96, 100),   S(108,  93),   S( 44,  95),   S( 97,   2),   S( -1, -107),  S(-20, -109),  S(  5, -150),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -11),   S(  3, -13),   S( 43,  10),   S( -8, -21),   S(-28, -45),   S( 70, -48),   S( 36, -66),   S( 38, -31),
            S(-14, -57),   S( -7, -11),   S(-56, -67),   S(-47, -20),   S(-35, -51),   S( 47, -45),   S( -4, -48),   S(-41, -31),
            S(-40, -66),   S( 37, -76),   S( 12, -59),   S(-14, -57),   S(-27, -12),   S(-32, -20),   S(-61,  -5),   S(-34,  -7),
            S(  2, -15),   S(-30, -33),   S( 23,   0),   S( 20,   5),   S(-33,   9),   S(-14,  29),   S( 11,  30),   S(-36,  40),
            S( 12,  21),   S(-11, -51),   S(  6,   9),   S( 39,  92),   S( 67, 120),   S( 46,  92),   S( 22,  90),   S(-23,  84),
            S( 33,  77),   S( 17,  54),   S( 24,  76),   S( 49,  80),   S( 43,  94),   S( 48, 157),   S( 38, 112),   S(  1,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 31,  -7),   S( 53,  -9),   S( 37,  -1),   S( 10,   2),   S( 10,  -8),   S( 68, -34),   S( 73, -59),   S(-20, -23),
            S( -7, -41),   S(-39, -39),   S(  1, -39),   S(-29,  -3),   S(-39, -28),   S(-13, -39),   S(-11, -48),   S( -1, -43),
            S(-74, -25),   S(-11, -56),   S(  0, -79),   S(-58, -50),   S( -3, -40),   S(  4, -40),   S(-59, -35),   S(-31,  -8),
            S(-33,   4),   S(-63, -51),   S( 22, -62),   S(  2, -23),   S( 13, -32),   S(-17,  -6),   S(-19,   0),   S( 26,   9),
            S( -9,  20),   S( 12, -39),   S( 13, -30),   S( 27, -17),   S( 15,  52),   S(  8,  46),   S( -6,  51),   S( -8,  44),
            S( -2,  62),   S( 14,  32),   S(  8,  18),   S( 28,   9),   S( 23,  85),   S( 23,  62),   S(  1,  71),   S(  2,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -56),   S(-15, -41),   S( -3, -21),   S( -7, -37),   S( 35, -21),   S(159, -29),   S( 93, -22),   S( 37, -37),
            S(-62, -49),   S(-51, -60),   S( 59, -72),   S( 28, -31),   S( 14, -30),   S( 25, -39),   S(  8, -39),   S( -9, -45),
            S(-67, -29),   S(-15, -58),   S(  3, -52),   S( 28, -50),   S( 24, -47),   S(  1, -61),   S( 10, -67),   S( 34, -53),
            S( 22, -21),   S( -1, -14),   S(-13, -27),   S(-26, -32),   S(-48, -29),   S(-46, -50),   S( 13, -48),   S( -3, -36),
            S( -4,  18),   S( 42,  11),   S( 32,   7),   S(  6, -44),   S( 22, -10),   S( 20,  13),   S( -8, -18),   S( 11,   2),
            S(-19,  17),   S( -2,  25),   S( 36,  52),   S( 27,  58),   S( 26,  47),   S(-11,  -5),   S(  3,   2),   S( 15,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31, -44),   S(-23, -38),   S(-25, -32),   S( 14,   7),   S( -2, -28),   S( 99,  -4),   S(110, -33),   S( 31,  -3),
            S(-35, -45),   S(-62, -62),   S(-27, -63),   S(-12, -46),   S(-12, -37),   S(-21, -42),   S(  9, -31),   S( 14, -53),
            S(-45, -15),   S( -2, -29),   S(-31, -11),   S( 27, -39),   S(-28, -43),   S( 25, -40),   S(-28, -80),   S(  8, -61),
            S( -2,   5),   S(-20,  23),   S( 23,  27),   S( 50,  -4),   S(-11, -15),   S(-34, -48),   S(-32, -56),   S(-21, -48),
            S(  6,   9),   S(  3,  67),   S( 44,  86),   S( 25,  51),   S( 24,  40),   S( 10, -11),   S(  9,  27),   S(  0, -25),
            S( 27,  52),   S( 14,  57),   S( 37,  94),   S( 32,  55),   S( 28,  67),   S( 38,  83),   S( 12,  18),   S( 26,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -105),  S( -3,  15),   S(-12,  -7),   S( -3,   3),   S(  0, -10),   S(-20, -53),   S( 13, -27),   S( 19, -50),
            S( -9, -55),   S(-18,   0),   S(-36, -67),   S(-36, -62),   S(-49, -90),   S(-21, -42),   S(  2, -47),   S(-23, -46),
            S(-30,  -4),   S(  1, -79),   S( -3, -85),   S(-20, -79),   S(-19, -59),   S(-19,  -6),   S(-58, -29),   S(-39, -33),
            S(-18, -12),   S( 12,  41),   S(-14, -26),   S( 18,  43),   S(  5,  36),   S(  1,  82),   S( -6,  37),   S(-35,  15),
            S( 21,  -5),   S( -3,  -7),   S( 16,  36),   S( 23,  70),   S( 42, 130),   S( 19,  90),   S( 10, 110),   S(  8, 102),
            S(  5, -18),   S(  9,  33),   S( 24,  75),   S( 20,  67),   S( 18,  78),   S( 32, 113),   S(-14,  66),   S(-15,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -51),   S(-23, -76),   S(  0, -17),   S(  1,   3),   S(  5,  -7),   S( -4, -44),   S(  3, -72),   S(-13, -64),
            S(-43, -30),   S(-29, -70),   S(-42, -67),   S(  1, -16),   S(-31, -53),   S(-46, -38),   S(-18, -73),   S( -8, -73),
            S(-33, -42),   S(-27, -89),   S( -7, -58),   S( -7, -32),   S(-17, -40),   S(-15, -27),   S(-34, -35),   S(-29, -21),
            S(-22,  25),   S(-17, -19),   S(  8, -26),   S( -3, -10),   S( -5,  33),   S(-33,  38),   S( -2,  -8),   S(-33,   5),
            S(-13,  22),   S( -2,  16),   S(  9,  34),   S( 31,  57),   S( 24,  93),   S( 23,  87),   S(  7,  72),   S( -2,  90),
            S( 14, 106),   S( 38,  85),   S(  1,   4),   S( 18,  45),   S( 26, 110),   S( 15,  80),   S(  3,  78),   S(  1,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-18, -55),   S( -1, -67),   S( 19, -14),   S( -1,  -6),   S(-10, -29),   S(-10,  -8),   S(  2, -32),   S(  1, -26),
            S(-25, -70),   S(-64, -107),  S(-23, -60),   S(-43, -93),   S( -5, -45),   S(-11, -45),   S(-17, -25),   S(-19, -42),
            S( -6, -56),   S(-44, -41),   S(-30, -55),   S(-11, -56),   S(-23, -47),   S(-18, -65),   S(-39, -55),   S(-36, -13),
            S(-16,   9),   S(-18, -19),   S(  5,  20),   S( -9,  -1),   S( 13, -41),   S( -9,   6),   S( -1, -13),   S(-13,   4),
            S( -4, -19),   S(  3,  18),   S( 16,  66),   S(  0,  19),   S( 18,  36),   S(  4,  29),   S( 23,  84),   S( 11,  72),
            S(-18,   6),   S( 14,  75),   S( 23,  77),   S( 24,  64),   S( 21,  62),   S(  6,  48),   S( 25,  78),   S( 27, 102),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5, -19),   S(  7, -42),   S(-12, -39),   S( -1,  -9),   S( -9, -35),   S(-27, -13),   S(-11,   6),   S( -1, -60),
            S( -6, -32),   S(-20, -75),   S(-10, -69),   S( -8, -46),   S(  0, -24),   S(-41,   6),   S(-41, -28),   S(-27, -58),
            S(-40, -30),   S(-45, -46),   S(-56, -69),   S( 33, -41),   S(-29, -31),   S( -2, -32),   S(  5, -42),   S(-26,  11),
            S(-48,  27),   S(-35, -13),   S(  2,  23),   S(  5,  36),   S( 25,  26),   S(  0,  10),   S(  3,  -4),   S(  4,  40),
            S(  2,  56),   S( 22,  95),   S(  1,  72),   S( 27,  90),   S( 22,  93),   S( 15,  72),   S( 24,  98),   S( 15,  49),
            S( -7,  72),   S(  2,  38),   S(  4,  74),   S( 20,  62),   S( 27,  97),   S( 19,  77),   S( 14,  38),   S( 14,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-41, -41),   S(-22, -41),   S( -8, -10),   S(-11,  23),   S(-18, -36),   S(-36,  22),   S(-13, -27),   S(-71, -49),
            S( -5,   2),   S(-26,  -2),   S(-27,  -9),   S( -8,   3),   S( -9,  11),   S(-24,  -1),   S( -4, -70),   S(-49, -25),
            S( -2, -41),   S( 14,   1),   S( -4,  37),   S( 32,  50),   S(-11,  27),   S(  6,   9),   S(-29,  18),   S(-26, -26),
            S(  9,  16),   S( 16,  67),   S( 27,  39),   S( 43,  29),   S( 17,  66),   S(  4,  49),   S( 44, -13),   S(-21,  13),
            S( 46,  81),   S( -7,  66),   S( 47,  94),   S( 56,  44),   S( 83,  32),   S( 12,  52),   S( 20,  17),   S(  4,  18),
            S(115, -14),   S(-14,  62),   S(128,  45),   S( 61,  23),   S( 26,  70),   S(-24,  86),   S( 38,  31),   S(  8,  38),
            S( 60,   0),   S(-32, -33),   S( 34,   4),   S( 58,  55),   S( 34,  42),   S(-10,  48),   S(-45,  19),   S(-13,   0),
            S(-107, -141), S( -1,  -7),   S( 22,  19),   S( 28,  66),   S( -4,  27),   S(  7,  26),   S(-26,  18),   S(-21, -12),

            /* knights: bucket 1 */
            S( 31,  -2),   S(-63,  37),   S( -8,  -3),   S(-40,  33),   S(-32,  35),   S(-23, -22),   S(-40,  15),   S(-39, -10),
            S(-39,  17),   S(-54,  74),   S(-40,  56),   S(-12,  34),   S(-22,  34),   S(-22,  72),   S(-22,  33),   S(-33, -36),
            S(-36,  38),   S(  1,  10),   S(-17,  29),   S( -5,  64),   S( -6,  46),   S( -7,  19),   S(-37,  36),   S( -4,  33),
            S(-15,  90),   S( 36,  51),   S( -8,  59),   S(-11,  67),   S(  2,  68),   S(-21,  75),   S( 18,  18),   S(-27,  41),
            S( 57,  -3),   S(  5,  22),   S( 12,  70),   S( 20,  58),   S( 28,  59),   S(  5,  63),   S( -1,  46),   S(  3,  81),
            S(  2,  49),   S( 53,  -3),   S( 58,  50),   S(103,  25),   S( 89,  19),   S(-55,  86),   S( 25,  38),   S( 23,  45),
            S( 16, -16),   S( 13, -17),   S( 33, -25),   S( 26,  42),   S( 19,  44),   S(  0,  23),   S( 16,  48),   S(-37,  49),
            S(-185, -43),  S( 14, -13),   S(-27, -54),   S(-10,   9),   S(  7,  36),   S( 31,  38),   S( 37,  22),   S(-56,  36),

            /* knights: bucket 2 */
            S(-86,  17),   S(-40,  44),   S(-45,  18),   S(-34,  53),   S(-21,  25),   S(-53,  20),   S(-31,  19),   S(-40, -11),
            S(-17,   8),   S(-10,  48),   S(-26,  39),   S(-15,  45),   S(-32,  40),   S(-21,  30),   S(  3,  22),   S(-52,  50),
            S(-34,  68),   S(-24,  45),   S(-23,  46),   S(-22,  83),   S(-25,  73),   S(-24,  32),   S(-29,  24),   S(  3,  10),
            S( -9,  67),   S( -5,  57),   S(-18,  93),   S(-24, 101),   S(-25,  91),   S( -7,  77),   S( -5,  56),   S(-12,  47),
            S(-18,  75),   S( -9,  77),   S(  4,  78),   S( 10,  67),   S(-25,  89),   S( 31,  70),   S(-15,  82),   S( 25,  20),
            S(-10,  79),   S(-26,  67),   S(-37, 103),   S( 30,  42),   S( 31,  54),   S(119,  27),   S( 65,   1),   S(  1,   1),
            S( 45,  39),   S(-65,  73),   S( 31,  24),   S( 32,   3),   S( -2,  26),   S( 15,   6),   S( 60,  26),   S(  2,  -1),
            S(-80,  13),   S( 29,  86),   S(-14,  76),   S(-23, -42),   S(-25, -16),   S(-42, -49),   S( 26,   0),   S(-162, -61),

            /* knights: bucket 3 */
            S(-71,  52),   S(-26,   2),   S( -6,  15),   S(-18,  45),   S( -9,  23),   S(-16,  14),   S(-22,  -5),   S(-29, -35),
            S(-26,  -4),   S( -7,  37),   S(  0,  35),   S( -7,  42),   S( -9,  34),   S(  4,  26),   S( 21,  -1),   S(  5, -24),
            S(-18,  38),   S(-15,  43),   S( -2,  60),   S(  1,  83),   S(  9,  74),   S( -5,  56),   S(  8,  36),   S( 20,   3),
            S(  3,  22),   S(  1,  67),   S( 15,  83),   S(  1, 103),   S( 12, 116),   S( 20,  96),   S( 28,  83),   S(  1,  75),
            S(-17,  77),   S(  6,  78),   S( 11,  97),   S( 31, 112),   S( 16, 100),   S( 36, 111),   S(-18, 127),   S( 52, 112),
            S(-29,  60),   S( 11,  78),   S( 18, 101),   S( -3, 117),   S( 33, 120),   S(101, 128),   S( 30, 110),   S( 39,  89),
            S(-62, 104),   S(-21,  69),   S(-58, 109),   S(  9,  93),   S( 19, 111),   S( 86,  54),   S(  9,  -5),   S( 60, -10),
            S(-173,  63),  S(-20, 109),   S(-25,  91),   S( 21,  71),   S( 77,  78),   S(-39,  86),   S(-30, -66),   S(-52, -95),

            /* knights: bucket 4 */
            S(  4,  31),   S( 10,   3),   S(-75,   2),   S(-34, -14),   S(-15,   0),   S(-12, -18),   S( 47,   6),   S(-31, -37),
            S( 19,  24),   S( -1, -28),   S( 19,  17),   S( -7,   9),   S( 16, -30),   S( 16, -51),   S( 12,  13),   S( -8,   3),
            S(-15, -22),   S( 11, -14),   S( 59,  -6),   S( 96,  -4),   S( 17,  16),   S( 55, -16),   S(  5, -19),   S(-29, -38),
            S(-26, -42),   S( 56,  -8),   S( 29,   0),   S( 79,   0),   S( 15,  15),   S( 30,   9),   S(-27,  17),   S(-10,  -4),
            S(  6, -24),   S(  2, -18),   S( 57,   8),   S( 48,  58),   S( 77,  -2),   S(-11,  61),   S(  0,   8),   S(-60,  19),
            S(-15, -48),   S(  1, -15),   S( 24, -23),   S( 57,  11),   S(-31,  32),   S( 11,  17),   S(-21,  29),   S( 30,  30),
            S(-19, -33),   S( -8, -10),   S(  5,   4),   S( 31,  41),   S(  5,  31),   S( 10,  40),   S( 14,  49),   S(-25,   4),
            S(  8,  17),   S(-10, -37),   S(-17, -29),   S(  5, -14),   S(  1,   3),   S(-10,  13),   S(  2,  16),   S(-12,   6),

            /* knights: bucket 5 */
            S( 16,  -5),   S( 37,  40),   S(-26,  28),   S(  9,   1),   S(  2,  22),   S( 13,  21),   S(-38,   0),   S(  0,   0),
            S( 29,  20),   S( 57,  28),   S( 71, -10),   S( 14,   0),   S( 41, -11),   S( 20,   5),   S(-15,  58),   S(-39,  33),
            S(-42,  12),   S(-30,   9),   S( 43,   1),   S( 41,  18),   S( 30,  17),   S( -5,  27),   S( 15,  -2),   S(-49,  10),
            S( 55,  12),   S( 54, -31),   S( 83,   0),   S( 87,  -4),   S(102,   2),   S( 97,  -4),   S( 29,  19),   S( 27,  18),
            S( 60,  -6),   S( 73, -18),   S(103,  -9),   S( 89,  -7),   S( 82, -16),   S( 63,  19),   S( 23,  -4),   S( 63,  10),
            S( 14, -22),   S( 43, -19),   S( 19, -33),   S(  9,  16),   S( 58, -14),   S( 74,   3),   S( 12,   8),   S( 20,  39),
            S( -1,  -3),   S(-43, -54),   S(-27, -67),   S( -9, -25),   S(-17, -53),   S( -8,  15),   S(  8,  55),   S(-10,  44),
            S(-32, -50),   S(-24, -56),   S(  2, -33),   S(-16, -17),   S( 14,  17),   S( 12,   0),   S( 24,  41),   S(  0,  19),

            /* knights: bucket 6 */
            S(  4,  -2),   S(-37,  17),   S(-10,  -7),   S(  2,  15),   S(-38,  42),   S( 18,  28),   S( -9,  47),   S(-28, -14),
            S( 38,   2),   S(  8,  25),   S(  3, -11),   S( 61,   6),   S( 37,   8),   S( -5,  18),   S(  0,  41),   S( 12,  28),
            S( 18,   6),   S( 27,  15),   S( 50,   7),   S( 59,  30),   S( 46,  29),   S(-25,  40),   S( 48,  15),   S(-30,  33),
            S( 30,  29),   S( 54,  13),   S( 63,  12),   S( 67,  18),   S( 76,  10),   S( 73,  14),   S( 23,   2),   S(-13,  37),
            S(-34,  51),   S( 70,  11),   S(119,   5),   S( 88,  18),   S(116, -25),   S( 76,   5),   S(190, -41),   S( -8,   2),
            S(  4,  22),   S( 41,  -3),   S( 68,  22),   S( 59,  11),   S( 38, -26),   S( 58, -12),   S( -5,  -8),   S( 28, -25),
            S( -1,  56),   S( 21,  37),   S( 69,  36),   S( 12,  -4),   S( 54,  15),   S( 33, -39),   S( 12,   6),   S( 14,  33),
            S( 29,  23),   S(-15,   4),   S( 10,  18),   S( 17,  21),   S( 24,  -5),   S( -2,  -6),   S(  5,  49),   S(-45, -59),

            /* knights: bucket 7 */
            S(-26, -36),   S(  6, -32),   S(  6,  -2),   S(-28,  21),   S(-28,  -8),   S(-53,  35),   S( 30, -10),   S(  6,  20),
            S( -3, -48),   S( 22, -17),   S(-34,  21),   S(-39,  14),   S( 37,  -6),   S( 43,  14),   S( -4,  20),   S(-41, -13),
            S(-16, -53),   S(-29,   8),   S( 34, -21),   S( 36,  22),   S( 61,  15),   S( 49,  20),   S( 35,  15),   S( -7,  39),
            S(-40,  45),   S( 65, -14),   S( 48,   5),   S( 78,  12),   S(131, -10),   S(102,  24),   S( 88,  -7),   S( 87,  -8),
            S( 18,  19),   S( -8,  15),   S( 19,  38),   S( 95,  -5),   S(103,  -1),   S(179, -33),   S(189, -13),   S( -6,  17),
            S(-19,  23),   S( 38,  11),   S(-18,  29),   S( 60,  23),   S(113,   3),   S(103,  -4),   S( 41, -15),   S(  8, -46),
            S(-10,  26),   S( -7,   0),   S( 14,  13),   S( 18,  33),   S( 78,  37),   S( 35,  25),   S(-10, -29),   S(-38, -54),
            S(  1, -22),   S( -4,  11),   S(  7,  39),   S( -3,  -1),   S( -5,   9),   S( 24,   3),   S( 17,  15),   S( -1,  -4),

            /* knights: bucket 8 */
            S(  4,  10),   S(  2, -19),   S( 23,  46),   S(-26, -75),   S(  1,   5),   S( -6, -49),   S( 11,   8),   S( -4, -36),
            S( -7, -35),   S(-10, -43),   S(-17, -83),   S( -9, -17),   S(  5,  17),   S(  3, -33),   S(  2,  -3),   S( -1, -12),
            S(-17, -91),   S(-12, -31),   S(  2, -52),   S( 20,  -9),   S( 11, -51),   S( 13, -15),   S( 14, -28),   S( -3, -42),
            S(-28, -93),   S(-12, -52),   S(  7,   1),   S( 19, -16),   S(  6, -55),   S(  7,  -7),   S(-14, -27),   S(-15, -42),
            S( -6, -33),   S(  2, -52),   S(  9,  -7),   S( 13, -30),   S( 10, -13),   S(-15, -24),   S(  1, -23),   S(  1, -16),
            S(  0,  -8),   S(  6, -41),   S( -5, -49),   S( 14, -21),   S(-13, -34),   S(  2, -26),   S(-12, -39),   S(  3,  25),
            S( -5,  -1),   S(  2, -50),   S( -6, -13),   S(  6,   0),   S(  1, -31),   S(  2, -17),   S( -4,   5),   S( -4, -10),
            S(  3,   8),   S( -1,  -6),   S( -4, -11),   S(  2,  -2),   S( -5,  13),   S( -4, -10),   S( -6, -14),   S( -6, -11),

            /* knights: bucket 9 */
            S(-16, -71),   S( -6, -14),   S(-25, -77),   S(  0, -60),   S(-13, -25),   S(-14, -30),   S( -2,  -2),   S(  8,   2),
            S( -6, -21),   S(-20, -82),   S(-38, -145),  S(-24, -79),   S(-10, -62),   S(-17, -80),   S(  7, -10),   S(-19, -28),
            S( -5, -52),   S(-21, -55),   S(-19, -34),   S( 19, -76),   S( 21, -27),   S( 27,  -1),   S(-12, -29),   S( -3, -20),
            S(-16, -60),   S( -5, -73),   S(  7, -43),   S( -5, -76),   S(-14, -66),   S( 20,  -8),   S(-10, -62),   S( 12, -16),
            S(  1,  14),   S(-12, -26),   S( 10, -34),   S( -6, -64),   S(-14, -57),   S(  8,  -4),   S(  7,  -4),   S( -9, -18),
            S(-24, -67),   S(-27, -61),   S(-18, -36),   S(  6, -38),   S( 13, -13),   S( -8, -24),   S(  2, -37),   S( -9,  12),
            S(-11, -34),   S( 10,  12),   S(-10, -37),   S(-19, -48),   S(  8, -20),   S( -3,  -4),   S(-13,   0),   S( -8, -30),
            S(  0,   0),   S(  5,   9),   S(  4,  -4),   S( -5, -28),   S( -6,  -6),   S( -1,   2),   S(  2,  12),   S(  2,  14),

            /* knights: bucket 10 */
            S( -1, -11),   S(-16, -48),   S(  0, -42),   S(-15, -19),   S(-16, -74),   S( 10, -59),   S( -5,   2),   S( -4, -40),
            S(  1, -11),   S(  6, -26),   S( -1, -65),   S( -1, -78),   S(  0, -49),   S(-19, -111),  S( -5, -20),   S(  0,  17),
            S(  5,  -4),   S(  2, -37),   S(  9, -23),   S( 17, -68),   S( -2, -62),   S( 17, -38),   S( -4, -33),   S(-10, -10),
            S( -8, -48),   S( -9, -60),   S( 15, -46),   S( 25, -33),   S(  0, -43),   S(-10, -41),   S(-10, -67),   S( -4, -73),
            S( -9, -49),   S(  0, -37),   S( 22, -43),   S(  0, -44),   S(  7, -23),   S( -6, -74),   S( -3, -16),   S( 11, -13),
            S( -1,  -6),   S(-15, -14),   S( 19, -15),   S(  3,  -3),   S( -9, -21),   S(-22, -46),   S(  4,  -9),   S( -8, -27),
            S( -1, -14),   S( -6, -29),   S(-14, -48),   S( 12, -39),   S( -9, -45),   S(-23, -73),   S(-15, -37),   S( -5, -20),
            S(  1,  -5),   S(  5,   5),   S( -4,  18),   S( -2,  -7),   S(  1, -25),   S( -9, -36),   S( 11,  11),   S(  0,  -3),

            /* knights: bucket 11 */
            S(-10, -56),   S(-27, -52),   S( -5, -29),   S(  7, -24),   S(-33, -55),   S(-14,  -9),   S(-17, -34),   S( 18,  54),
            S(-12, -47),   S(-27, -65),   S(-12, -72),   S( 31, -48),   S( 18,   0),   S( -2, -50),   S(-17, -36),   S(-15, -41),
            S(-13, -64),   S(-26, -35),   S( -4, -38),   S( 27,  -5),   S(  4,  -8),   S( 24, -26),   S(-15, -50),   S(-14, -51),
            S(-17, -27),   S( 21, -18),   S( 18, -18),   S( 35, -21),   S( 31, -45),   S(  0, -40),   S( 16, -19),   S(  6, -10),
            S(-14, -14),   S(  7, -66),   S( 15,  -5),   S( 14,  -8),   S( 46,   9),   S( 17,   3),   S(-16, -94),   S( -3, -22),
            S( -5, -18),   S(-16, -86),   S(  9,  -2),   S( 19, -28),   S( 14,  -9),   S( 17,   5),   S(  1, -34),   S(  0,  -4),
            S( -9, -27),   S(  2,  28),   S(-11, -24),   S(-10, -20),   S( -3, -27),   S( 17, -19),   S( -3, -30),   S( -2,   5),
            S( -2,  -9),   S(  5,  20),   S(-10, -26),   S( -1,  22),   S( 10,   4),   S( 10,   7),   S(  4,  17),   S(  1,  -1),

            /* knights: bucket 12 */
            S(-19, -52),   S( -9, -46),   S( -4, -39),   S( -2,  -3),   S( -5,   5),   S( -4, -15),   S( -2,   1),   S( -2,  -8),
            S( -8, -50),   S(  0,  -7),   S(  1, -17),   S( -9, -32),   S( -1, -24),   S( -8, -26),   S(  6,   9),   S(  1,  -7),
            S(  1,   5),   S(-16, -70),   S( -4, -22),   S(-11, -84),   S(  5,  -4),   S(  6,   3),   S(  2,   1),   S( -7, -22),
            S(  6,  27),   S( -7, -66),   S(  4,  -2),   S(  3, -23),   S( -4, -60),   S(  3,  -2),   S( -6,  -4),   S(  4,   9),
            S(  0,  -7),   S( -3, -49),   S( -7, -47),   S(  0, -42),   S( 11,  21),   S( -3, -15),   S( -2, -11),   S(-10, -25),
            S(  1,   7),   S(  2,  -4),   S( -4, -10),   S( -5, -26),   S( -9, -40),   S( -8, -16),   S( 15,  15),   S( -3, -11),
            S( -4, -13),   S( -4,  -7),   S(  0,   2),   S(  5,  20),   S( -1,   5),   S(-18, -41),   S( -7, -20),   S( -4, -10),
            S(  0,  -1),   S(  3,  17),   S(  1,   0),   S( -1, -10),   S(  2,   4),   S(  4,  11),   S(  1,   2),   S( -1,  -7),

            /* knights: bucket 13 */
            S( -5, -21),   S( -5, -36),   S( -5, -43),   S( -2, -13),   S( -8, -31),   S( -3, -12),   S( -7, -20),   S(  1,   6),
            S( -2, -27),   S( -6, -24),   S( -1,  -6),   S( -8, -35),   S( -9, -43),   S(  3, -13),   S( -8, -31),   S( -5, -16),
            S(  4,  17),   S(  1,   4),   S(  1, -15),   S( -2, -28),   S(  4,  10),   S( -8, -18),   S( 17,  26),   S( -2,  -4),
            S( -2, -15),   S(  4, -14),   S(-10, -38),   S( 10,   1),   S(  5, -29),   S( 20,  -3),   S(  5,  -1),   S( 18,  30),
            S( -2,  21),   S( -1, -24),   S(  1, -37),   S(  6, -13),   S( -4, -34),   S( -1, -13),   S(-11, -48),   S( -4, -20),
            S( -1,  -4),   S( -2, -11),   S(  0,  -5),   S( 15, -10),   S( -5,  -6),   S(-11, -72),   S(  0,   7),   S( -1, -10),
            S(  2,   3),   S(  4,   2),   S( -3,  -3),   S( -4, -17),   S(  4,  26),   S( -5, -14),   S(  6,   6),   S(  2,   9),
            S(  3,   8),   S( -3,   1),   S( -2,  -8),   S(  1,   2),   S( -3,  -9),   S(  2,   3),   S( -1,  -1),   S(  1,   4),

            /* knights: bucket 14 */
            S(  1,  -4),   S( -3, -17),   S(  2,   0),   S( -6, -26),   S( -7, -52),   S( -7, -19),   S(  4,   1),   S( -2, -10),
            S( -1,  -9),   S(-12, -55),   S( -4, -33),   S( -3, -42),   S( -3, -22),   S(  1,  -6),   S(-12, -44),   S(  4,  36),
            S( -4, -28),   S(  2,  -7),   S(  9,   3),   S(-14, -59),   S( -2, -27),   S( -5, -28),   S( -7, -24),   S(  1,  -2),
            S( -2,  -9),   S(  1, -18),   S(-19, -58),   S(  2,  -9),   S(  4, -16),   S(  4, -27),   S( -9, -35),   S( -1,   8),
            S( 11,  20),   S(-18, -68),   S( -5, -36),   S( -7, -50),   S(  6,  62),   S( -3,  -8),   S( -7,   2),   S(  0, -18),
            S(  2,   6),   S(  7,  26),   S(  3,  58),   S(  4,  11),   S( -5,   6),   S( -5, -24),   S( -8, -16),   S( -4, -13),
            S( -1,  -1),   S( -3, -15),   S( -4, -17),   S(  8,  56),   S( -4,  -8),   S(-11, -38),   S(  5,   2),   S(  3,   5),
            S( -1,  -2),   S(  0,   3),   S( -2,  -6),   S(  2,  10),   S( -6, -23),   S( -2,  -5),   S(  3,   9),   S( -1,   1),

            /* knights: bucket 15 */
            S(-10, -43),   S( -2,  -1),   S(  6,  32),   S( -5, -14),   S( -6, -25),   S(-15, -65),   S( -6, -63),   S( -5, -33),
            S(  0,   2),   S(  7,  17),   S( -6, -27),   S(  9,  -4),   S(  1, -27),   S( -7, -39),   S( -3, -16),   S(  3,   4),
            S(  3,   1),   S( -8, -39),   S(  3, -20),   S( 17, -21),   S(-15, -74),   S( -1, -36),   S(  2, -20),   S(  0,  -5),
            S( -2, -17),   S(  0,  18),   S(  7,  10),   S(  8,  15),   S(  4, -43),   S(  1, -15),   S(  9,  35),   S(  0,   8),
            S(  0,  -8),   S(  9,  23),   S(  8,   4),   S(  3,  20),   S( 12,  19),   S(  4,  24),   S(  2, -34),   S(  3,  -1),
            S( -5,  -6),   S( -1,   5),   S(  4,  17),   S( -3,   6),   S(-16, -38),   S(  7,  31),   S(  2,   3),   S(  5,  19),
            S( -1,  -2),   S(  0,   4),   S(  7,  28),   S(  5,  20),   S(  3,  24),   S( -1,  -4),   S(  2,  12),   S(  3,   5),
            S(  4,  12),   S( -2, -12),   S( -1,  -2),   S(  0,  -3),   S(  0,   4),   S(  3,  13),   S(  1,   8),   S(  1,   7),

            /* bishops: bucket 0 */
            S( 46, -45),   S(-34,  29),   S(-13,  -7),   S(-20, -21),   S(  1, -10),   S( 14, -11),   S( 76, -83),   S( 29, -29),
            S(-42, -11),   S(  6, -20),   S(-13,  16),   S(  5,   9),   S( 14,   1),   S( 67, -21),   S( 44,  26),   S( 48, -20),
            S( 14,  18),   S(-10,  27),   S( 16,   5),   S( 13,   3),   S( 52, -12),   S( 39,  29),   S( 51, -17),   S( 27, -27),
            S( 13, -61),   S( 58, -66),   S( 15,  -2),   S( 82, -28),   S( 92,   2),   S( 47,  20),   S(  5, -13),   S( -9,  32),
            S( 61, -24),   S( 56, -43),   S( 70,  -1),   S( 84,  -4),   S(130, -22),   S( 10,  37),   S( 39,  37),   S(  2,  11),
            S( 39,  22),   S(122, -10),   S(121,  -4),   S( 78,   0),   S(  2,   9),   S( 29,  29),   S( 64,   2),   S(-13,  39),
            S(-54, -92),   S(107,  24),   S(113,  65),   S(-10,  14),   S( 46, -14),   S( 16,   6),   S( -7,  29),   S(-25,  54),
            S( -4, -41),   S( -8,  -8),   S(-20, -38),   S(-12, -12),   S(-20, -10),   S( -9,   8),   S(-37,  10),   S(-38, -30),

            /* bishops: bucket 1 */
            S(-12, -10),   S(  9, -37),   S(-21,  27),   S( 21, -13),   S(  3,   0),   S( 14,  -9),   S( 44, -14),   S( 52, -29),
            S( 16, -46),   S( -5, -18),   S( -3, -20),   S(-11,   7),   S( 40, -23),   S( 14,  -4),   S( 58, -23),   S(  6, -32),
            S(-26,   2),   S( 39, -24),   S(  0,  -7),   S( 31, -11),   S( 14,  -7),   S( 45, -26),   S( 20, -21),   S( 88, -37),
            S( 28, -36),   S( 60, -23),   S( 35,  -9),   S( 33,  -2),   S( 60,  -8),   S(  0,   5),   S( 58, -36),   S( -6,  -9),
            S( 38, -50),   S( 69, -26),   S( 25,  -3),   S(116, -29),   S( 54,  -6),   S( 96, -38),   S(  6,   2),   S( 42, -11),
            S( 94, -46),   S( 48, -14),   S( 62,  -8),   S( 73, -38),   S(133, -49),   S(-29,  14),   S( 13,  17),   S(-20, -20),
            S(  2, -84),   S( -3, -18),   S( -2, -12),   S( 15,  11),   S( 40,   1),   S(  2,  26),   S( 18,  -7),   S( -5,  38),
            S(-25, -64),   S(-19,  -9),   S( -6, -74),   S(-43, -23),   S( 13, -17),   S( 24, -38),   S( 34, -28),   S(-39, -26),

            /* bishops: bucket 2 */
            S( 17, -32),   S( -6, -15),   S(  4,   8),   S(-20,   5),   S(  8,  14),   S(-11,   8),   S( 20,  -8),   S( -2,   4),
            S( 23, -27),   S( 17, -13),   S( -1,  -7),   S( 14,   9),   S( -3,  11),   S( 23,  -6),   S(  9, -10),   S( 31, -65),
            S( 33, -12),   S( 26,  -9),   S(  9,  16),   S(  4,   0),   S( 10,  24),   S(  0, -11),   S(  9, -24),   S( -3, -17),
            S(  7, -10),   S( 47, -13),   S( 14,   0),   S( 37,  20),   S( 14,  13),   S( -9,  23),   S(-15,   3),   S(  8,  -3),
            S( 18,  -2),   S( 13,  11),   S( 87, -15),   S( 38,  10),   S( 28,  25),   S( 17,  13),   S( 12,  29),   S( 69, -37),
            S( -7,  16),   S( 18,  24),   S( 12,  -8),   S( 89, -24),   S( 72, -23),   S( 97,  14),   S( 84, -15),   S( 24, -32),
            S( -2,  23),   S(-18,   5),   S(-12,  18),   S( 26,  13),   S(-90,  -8),   S(-28, -24),   S(-30,  21),   S(-29, -21),
            S(-72,  -8),   S(-17,  21),   S(  5,   2),   S(-38,  13),   S(-24, -31),   S(-22, -14),   S(-15, -16),   S(-53, -37),

            /* bishops: bucket 3 */
            S( 26,   9),   S( 59, -20),   S( 12,   2),   S( 11,  10),   S( 11,  30),   S(  2,  46),   S( -3,  56),   S( 15, -18),
            S( 42,   8),   S( 31,   8),   S( 23,  16),   S( 24,  20),   S( 29,  24),   S( 32,  27),   S( 14,  17),   S( 43, -16),
            S(  4,   9),   S( 35,  43),   S( 23,  50),   S( 28,  34),   S( 25,  47),   S( 22,  28),   S( 24,   6),   S( 14,  21),
            S( -9,  33),   S( 27,  29),   S( 40,  48),   S( 38,  58),   S( 46,  37),   S( 23,  27),   S( 33, -10),   S( 34, -31),
            S( 16,  26),   S( 21,  54),   S( 13,  57),   S( 62,  60),   S( 48,  61),   S( 58,  34),   S( 22,  39),   S(  0,  35),
            S( 13,  37),   S( 43,  54),   S( 33,  39),   S( 17,  57),   S( 65,  34),   S( 64,  80),   S( 56,  68),   S( 16,  90),
            S(-22,  75),   S( 11,  45),   S( 14,  34),   S( 15,  36),   S( 18,  55),   S( 45,  72),   S(-41,  22),   S( 11, -22),
            S(-62,  61),   S(-19,  52),   S(-16,  38),   S(-51,  64),   S( 20,  35),   S(-98,  69),   S( 15, -15),   S( -4,  25),

            /* bishops: bucket 4 */
            S(-32,   0),   S(-50, -33),   S(-54,  24),   S(-31,  -9),   S(-19, -32),   S(  8, -24),   S(-28, -31),   S(-41, -47),
            S(-28,  21),   S( -6,  14),   S( 63, -43),   S(-33,  -8),   S(-59,   2),   S(  0, -38),   S( -6, -28),   S(-38, -17),
            S( 13,  25),   S(-49, -11),   S( 64, -34),   S( 16, -30),   S( -6, -26),   S(-31,   2),   S( -4,   0),   S(-58, -29),
            S( 32, -37),   S( 60,  -5),   S( 25, -31),   S( 68,  -8),   S(-14,  26),   S( 91, -22),   S(-41, -17),   S(-28, -18),
            S(  5,  -3),   S(  4, -68),   S( 40, -37),   S( 71, -34),   S( 21,  -1),   S( 24, -13),   S( -9,  26),   S(-48, -28),
            S(-64, -84),   S(-37, -56),   S( 24, -12),   S( 24, -12),   S(-32,  -3),   S( 14,  18),   S(-34,   3),   S(-26,  25),
            S(  9,   6),   S(  8,  -3),   S(  3, -62),   S( -5, -49),   S(-32,  -1),   S( 29,  -2),   S(-15,  -6),   S( 14,  35),
            S(-19, -56),   S(  7, -30),   S(-15, -23),   S(  1, -25),   S(  5, -41),   S(-11,   2),   S(  4,  38),   S(  4,  14),

            /* bishops: bucket 5 */
            S(-53,  -5),   S(-15, -16),   S(-53,   7),   S(-70,  22),   S(-18,   7),   S(-76,  11),   S(-57,   4),   S(-42, -37),
            S(-16,  -7),   S(-14,   0),   S( -6,   1),   S(  7,  -9),   S(-45,   6),   S( -6,   1),   S(-22,  -9),   S(-17, -37),
            S( 22,   0),   S(-42,  -3),   S( 16, -14),   S(-42,   6),   S(  1,  -1),   S(-45,  -2),   S(-17,  -1),   S(-30,  -5),
            S( 23, -15),   S(-30,   9),   S( 75, -26),   S( 75, -14),   S(-23,  17),   S( 32, -12),   S(-56,  17),   S(-45,  -1),
            S( 15, -25),   S( 13,  -4),   S(  0, -19),   S(-10, -28),   S( 20, -33),   S(  3,  -5),   S( 72, -25),   S(-32, -10),
            S( -9, -19),   S(-25, -13),   S( 30, -49),   S(-45, -20),   S(-11, -20),   S( 12, -11),   S( -7,  -4),   S(-15,  15),
            S(-30, -38),   S(  6, -25),   S(-12,  -1),   S(-24,   0),   S(  9, -25),   S( 28,  -4),   S( 27,  17),   S(-18,  -6),
            S( -7, -13),   S(-29, -33),   S( 17, -13),   S( -4, -30),   S(-35,  11),   S(  3,  -8),   S(-38,  -8),   S( -6,  19),

            /* bishops: bucket 6 */
            S(-28,  -6),   S(-37,   2),   S(-57,  15),   S(-13,   4),   S(-19,   5),   S(-38,  17),   S(-56,  18),   S(-61,   4),
            S(-41, -10),   S(-50,  -5),   S(-38,  11),   S( -8,   0),   S(-56,  22),   S(-28,   5),   S(-36,  16),   S(-64,  -9),
            S( 10, -14),   S(-27,  -2),   S( 17, -14),   S(-29,  15),   S(  9,  10),   S(-18,   1),   S(-23, -17),   S(-38,  12),
            S(-42,   3),   S(-25,  -5),   S(-40,  17),   S( 67,  -1),   S( 45,  10),   S( 22,   2),   S( 32, -21),   S(-15,  -4),
            S(-57,  22),   S(-16,  13),   S( 28,  -2),   S( 80, -17),   S( -2, -26),   S( 20, -21),   S( 22, -11),   S(-22, -13),
            S(-51,  31),   S( -6,   4),   S(-31,  15),   S( 33, -11),   S( -9, -14),   S( 12,  -5),   S( 10,  -1),   S( -3, -20),
            S(-24,  10),   S(-85,  37),   S( 21,  -7),   S(-15, -22),   S(-33,   9),   S(  5,   2),   S( -3, -30),   S(-47,  -6),
            S(-33,  16),   S(-13, -16),   S(-24,  15),   S( -9,   5),   S(-23,   4),   S( 41, -37),   S(-20, -10),   S(-13, -25),

            /* bishops: bucket 7 */
            S(-15, -24),   S(-56, -28),   S(-31, -45),   S(-27,  -6),   S(-21, -15),   S(-40,  -7),   S(-47, -25),   S(-24, -18),
            S( 31, -62),   S(  8, -36),   S( 41, -30),   S( -4, -37),   S(-41,   8),   S(-56,  11),   S(-18, -33),   S(-35, -19),
            S(-48, -25),   S(-28,  -3),   S( -7,   0),   S( 14, -16),   S( 27, -23),   S( 40, -37),   S(-56,   0),   S(-85,  33),
            S(-20, -22),   S(-43,   6),   S(-19,  12),   S( 25, -11),   S( 97,   5),   S( 53, -27),   S( 54, -33),   S( 11, -34),
            S( -8, -23),   S( 23,   4),   S( 11, -11),   S( 24,  -6),   S( 58, -21),   S( 77, -33),   S(-35, -24),   S(-46, -38),
            S(-73,  16),   S(-47,  37),   S(-18,   3),   S(-35,   1),   S(-34,   9),   S( 57, -38),   S( 21, -11),   S(-43, -68),
            S(-41,  -5),   S( -5,  15),   S(-28,  14),   S( 49, -19),   S(  4, -31),   S(  9, -44),   S( 14,  -9),   S(-10, -34),
            S(-28, -22),   S(-17, -15),   S(-23,  16),   S(  7, -21),   S(-20,  -9),   S(  0, -25),   S( 26, -36),   S( -9, -16),

            /* bishops: bucket 8 */
            S( 16,  87),   S(-22, -16),   S(  0, -22),   S(  0,   1),   S( 11,  24),   S( -4, -36),   S(-26, -50),   S( -9, -32),
            S( -9,   2),   S( -7,  23),   S(  1,  -1),   S( 26,  19),   S( 11,   2),   S( -7,   5),   S(-22, -40),   S( -7, -32),
            S(-11, -36),   S( -9, -27),   S( 13,  33),   S( 23, -15),   S( 32,  26),   S( 34,  30),   S( -4, -25),   S(-27, -32),
            S( -2,   6),   S( 15,  60),   S( 13,  34),   S(  4,  58),   S( 13,   8),   S( 19,  18),   S(-14,   1),   S(  2, -17),
            S(  1,  65),   S( 17,  81),   S(  4,  13),   S(  3,   5),   S( -1,  39),   S(-23,  35),   S( -2, -44),   S(  7,  30),
            S(-15, -29),   S( -9,  25),   S(  9,  36),   S(  9,  20),   S( -8,  12),   S(  4,  39),   S(-10,  27),   S( -1,  -1),
            S(  5,  21),   S(-16, -53),   S( 17,  44),   S(-13,  40),   S(-13,   8),   S( -2,  35),   S(  8,  71),   S(-14,  -2),
            S( -4,  -2),   S(  8,   1),   S( -6, -11),   S(  9,  27),   S(  5,  34),   S( 16,  47),   S( -1,  37),   S( 14,  90),

            /* bishops: bucket 9 */
            S( 10,  51),   S(-20,   9),   S(-22,   7),   S(-47, -29),   S(-28,  -9),   S(-11, -27),   S(-10,  -3),   S( -1,  11),
            S( -9, -14),   S(-11,  21),   S(-11,   5),   S(-16,  -8),   S( -1, -37),   S( -6, -25),   S( -5,  -7),   S(-30, -70),
            S( -7, -27),   S(  4,   1),   S(  8,   3),   S(  7,  12),   S( 11,  20),   S( -8, -24),   S(  4, -15),   S(-15, -12),
            S(-11,   1),   S(-14,  -3),   S( -2,  26),   S(  6,  13),   S(-33,  18),   S( -1,  -5),   S( 14,  33),   S( -2,  -5),
            S( -7,  20),   S(-19,  26),   S( 24,  15),   S(-10,   4),   S(-21,  30),   S(  0,  15),   S( -3,  24),   S(-17, -49),
            S( -1,  12),   S(-14,   4),   S(-11,  24),   S(  3,  20),   S(-22,   7),   S(  1,  20),   S( -2,  18),   S(-12,  21),
            S(  4,  34),   S(-14,  25),   S(-17,  -7),   S(-22,  11),   S(  1,  13),   S(  5,  30),   S(  3,  43),   S(  3,  52),
            S(  3,  49),   S(-13, -29),   S( -3,   8),   S( -1,  18),   S( -2,  24),   S(-17,  19),   S(-17,  47),   S( 14,  86),

            /* bishops: bucket 10 */
            S( -4,  -6),   S(-10,  10),   S(-24, -11),   S(-15, -42),   S(-51, -47),   S(-38, -43),   S( -8,  32),   S( -6,  15),
            S(-16, -29),   S( -3, -23),   S(-16, -19),   S(-31, -44),   S( -6, -28),   S(-28, -35),   S(-20, -34),   S( -8,  10),
            S(-13, -32),   S(-20, -33),   S( -4,  -6),   S( -4,  18),   S(-25, -37),   S(  3, -25),   S(-12, -17),   S( 15, -35),
            S( -4,  -1),   S(-20, -22),   S(-25, -15),   S( -7,  -1),   S( -8,  23),   S( 23,  12),   S(  5,  20),   S(-25, -10),
            S(-13,  21),   S(-32,  26),   S( 14,  -5),   S( 16,   8),   S(  9,   1),   S( -4,  28),   S(-27,   6),   S(  1,   7),
            S( -2,  33),   S(-10,  16),   S(-31,  10),   S(-33,  14),   S(-16,  -2),   S( -6,  12),   S(-12,  23),   S( -7,  19),
            S(-17,  22),   S(-22,  26),   S(-16,   9),   S(-13,  25),   S( -9,   1),   S(-29, -30),   S(-10,  -3),   S(  6,  32),
            S(-14,  44),   S( -8,  54),   S( 10,  43),   S(-16,   6),   S(-16,  18),   S( -3,  12),   S(  2,  -2),   S( -5, -18),

            /* bishops: bucket 11 */
            S( 21,   5),   S( -8,  -3),   S(-18,  12),   S(-13,  -8),   S(-25, -31),   S( -5, -17),   S(-22,   5),   S(-21,  45),
            S(-14, -20),   S( 10, -35),   S(  6,   0),   S(  3, -33),   S(  1,  -5),   S(-47,   2),   S(-37,   6),   S(  6,  -2),
            S( -8, -55),   S(  8,  -1),   S(  0,  -9),   S(  0, -30),   S(-15,  -4),   S( 16,  19),   S(-10, -39),   S( -9, -33),
            S( -4,  13),   S(-14, -45),   S( 17,  10),   S(-30, -34),   S( -9,   6),   S( 34,  27),   S(  3,  50),   S( -7,  -7),
            S(-30,  15),   S( -6, -15),   S(-13,  14),   S(-19,  40),   S(  5,   5),   S(  7,   9),   S(-12,  29),   S( -5,  19),
            S(-18,  -6),   S( -7,  23),   S(-21,  35),   S(-23,  22),   S(  0,  63),   S(-17,  43),   S(  0,  12),   S(-12,   0),
            S(-17,  15),   S(-13,  90),   S(  0,  33),   S( -7,  47),   S(  1,  13),   S(-19,  19),   S(  2,   3),   S( -6,   7),
            S( 12,  98),   S(-14,  40),   S(  9,  64),   S(  3,  22),   S( -2,  21),   S( -6,  12),   S(-15, -21),   S(  6,  30),

            /* bishops: bucket 12 */
            S( -9, -29),   S( -1, -24),   S( -7, -21),   S(  9,  25),   S( -3,  -1),   S( -8, -22),   S( -1,   8),   S( -1,  -6),
            S( -2, -16),   S(  5,  25),   S(  4,  -3),   S( -1, -15),   S( -1, -12),   S(  6,   1),   S(-10, -14),   S( -3, -12),
            S(  4,  33),   S(  0,  13),   S( 20,  61),   S( 27,  26),   S(-18, -25),   S(-12, -44),   S(  8,  12),   S( -3,  -8),
            S(  4,  48),   S( 10,  61),   S(  4,  40),   S( 17,  48),   S(  8,  18),   S(  1, -19),   S(  3,   4),   S( 12,  36),
            S(  9,  12),   S( 10,  44),   S( -3,  17),   S( 20,  63),   S( 15,  47),   S( -3,  20),   S( 10,  14),   S( -7, -15),
            S( -1,   2),   S(-14, -37),   S( -1,  19),   S( -7, -12),   S( 18,  66),   S(  7,  26),   S(-12, -11),   S( -2, -14),
            S( -5,  -8),   S(  2,  13),   S( -1,  -9),   S(  6,  26),   S(  8,  41),   S( 13,  61),   S( 18,  65),   S( -7,   6),
            S(  1,  14),   S( -3,   3),   S( -2,  -4),   S(  0,   0),   S(  6,  18),   S(  2,   5),   S(  9,  62),   S(  6,  35),

            /* bishops: bucket 13 */
            S( -8, -33),   S( -4,   4),   S(-10, -31),   S( -6,  -9),   S(  5,  27),   S(-12, -54),   S(-18, -43),   S( -5, -27),
            S( -5,  22),   S( -2,  -3),   S( -1,   9),   S(  7,  52),   S(-14, -16),   S( -2, -19),   S( -1, -24),   S( -5, -16),
            S(  0,  24),   S( 18,  82),   S(  4,  28),   S( 13,  15),   S(  2,  12),   S( 14,  37),   S(  4,  -4),   S(  0, -11),
            S( 10,  74),   S(  4,  49),   S( 12,  47),   S(-14,  -8),   S(  7,  48),   S(-13,  -5),   S(  9,  41),   S(  8,  28),
            S(  4,  64),   S( -3,   8),   S(  1,   3),   S(  7,  43),   S( -5,  19),   S(  1,  41),   S(  7,   7),   S(  6,  52),
            S( -8,  14),   S( -1,  18),   S( -3,  25),   S( 14,  26),   S( -2,  50),   S( -8, -19),   S( -1,  30),   S(  3,  26),
            S(  6,  32),   S(-11, -21),   S( -9,  -9),   S(  4,  26),   S( -2,   9),   S(  5,  44),   S(  7,  33),   S(  2,  43),
            S(  2,  -2),   S(  1,  15),   S(  1,   0),   S(  8,  27),   S(  1,  13),   S( -4,  -2),   S(  5,  38),   S(  5,  26),

            /* bishops: bucket 14 */
            S( -5,  -8),   S(  1,  10),   S( -1,   9),   S(  4,  34),   S(-21, -53),   S( -3,   3),   S(-10, -26),   S( -4, -17),
            S( -2, -10),   S(  6,  23),   S(  5,  42),   S(  1,  11),   S( 10,  44),   S( -4,   7),   S( -4,   1),   S(  2,  33),
            S( -3, -13),   S( -8, -10),   S( -5, -17),   S( 12,  24),   S( 14,  48),   S( 20,  81),   S(  2,  46),   S(  2,  56),
            S(  2,  24),   S(  2,  -5),   S( -7,  10),   S( -6,  47),   S( -6,  11),   S( 10,  47),   S(  8,  83),   S( -4,  28),
            S(  3,  16),   S( -3,  16),   S( -1,  37),   S(  6,  56),   S( -7,  -7),   S( 10,  48),   S( 21,  48),   S(  2,  39),
            S( -6,   8),   S( 22,  50),   S( -4,  37),   S(  2,  35),   S(  6,  56),   S( -4,   4),   S(  0,   6),   S(  3,  25),
            S(  7,  44),   S(  9,  40),   S( 12,  61),   S(  4,  19),   S( -3,   3),   S( -2,   5),   S( -5, -28),   S( -1,   6),
            S(  8,  52),   S(  7,  52),   S(  7,  28),   S(  3,  19),   S(  0,  -9),   S(  1,   7),   S( 13,  34),   S(  1,   1),

            /* bishops: bucket 15 */
            S( -2,  -3),   S( -6, -10),   S(-15, -38),   S(-10, -23),   S(-20, -38),   S( -2, -49),   S( -4, -32),   S( -6, -26),
            S( -2,  -2),   S( -1,  -8),   S(  5,   9),   S( -3,  -3),   S( 10,   5),   S( -7, -35),   S( -2,   6),   S( -2,  -8),
            S(  0, -13),   S(  2,  -8),   S(  1,  16),   S( 21,  26),   S( 16,  30),   S( 11,  23),   S(  6,  39),   S(  4,  47),
            S( -1,   2),   S( 11,  20),   S(  5,  30),   S( -2, -12),   S(  0,  16),   S(  8,  54),   S(  7,  27),   S( -3,  31),
            S( -6,   0),   S(  5, -17),   S(  6,  48),   S( 26,  70),   S( 19,  71),   S(  4,  18),   S( -5,  -1),   S( -4,  -4),
            S( -5, -18),   S(-11, -24),   S( 21,  59),   S(  3,  24),   S( 20,  71),   S(  6,  38),   S( -4,   3),   S( -3,  -1),
            S( -7, -22),   S(  4,  25),   S( -4,  36),   S(  9,  49),   S(  8,  30),   S( -4,   5),   S( -2,   8),   S( -2,  -1),
            S(  3,  21),   S(  4,  31),   S(  4,  42),   S(  1,  -2),   S(  3,  20),   S( -2,   1),   S(  4,  20),   S(  7,  23),

            /* rooks: bucket 0 */
            S( -5,  12),   S( 16,   1),   S(  1,   2),   S(  6,  12),   S(-15,  62),   S( -9,  38),   S(-27,  43),   S(-54,  57),
            S(  7, -11),   S(  4,  32),   S(-24,  25),   S(  7,  47),   S(  5,  53),   S(  7,  24),   S(-10,  24),   S(-29,  63),
            S( 21, -26),   S( 10,  11),   S(-32,  35),   S(  5,  25),   S(-29,  76),   S(-21,  34),   S(-17,  40),   S(-12,  41),
            S(  0,   6),   S( 36,  20),   S(-24,  36),   S( 21,  17),   S(  8,  64),   S(-15,  36),   S(-29,  75),   S(-23,  42),
            S( 60, -42),   S( 50,  41),   S( 11,  33),   S( 18,  42),   S( 55,  25),   S(  0,  97),   S(  3,  81),   S( 21,  62),
            S( 60, -13),   S( 59,  51),   S( 51,  24),   S( 99,  25),   S(  6,  75),   S( 58,  55),   S(-16, 100),   S(-47,  85),
            S( 25,  45),   S( 81,  92),   S(121,  40),   S( 58,  51),   S( 84,  48),   S( 12,  80),   S( -7,  72),   S( -1,  75),
            S( 21, -20),   S( 38,  26),   S( 28,  54),   S( 62,   0),   S( 66,  47),   S( 86,  36),   S( 52,  38),   S( 36,  -7),

            /* rooks: bucket 1 */
            S(-57,  68),   S(-20,   9),   S( -8,  24),   S(-36,  39),   S(-36,  60),   S(-35,  52),   S(-47,  70),   S(-75,  85),
            S(-62,  56),   S(-22,   9),   S(-39,  42),   S(-43,  47),   S(-27,  24),   S(-53,  58),   S(  2,  13),   S(-21,  49),
            S(-28,  38),   S(-37,  29),   S(-11,  11),   S(-26,  33),   S(-53,  37),   S(-48,  47),   S(-74,  74),   S(-10,  43),
            S(-68,  67),   S(  6,  30),   S(-27,  48),   S(-34,  39),   S(-64,  62),   S(-48,  76),   S(-24,  53),   S(-76, 100),
            S(-46,  71),   S(  4,   6),   S( 24,  35),   S( 44,  16),   S(-11,  25),   S( -1,  81),   S(-34,  79),   S(-55, 101),
            S( 51,  25),   S( 56,   7),   S( 38,   2),   S( -8,  48),   S(-14,  36),   S(  9,  54),   S( 32,  51),   S( 11,  80),
            S( 70,  47),   S( 29,  22),   S( 26,  46),   S(-14,  32),   S( 35,  34),   S( -1,  71),   S( 38,  94),   S( 37,  93),
            S( 40,   8),   S( 24,   6),   S( -1, -17),   S( -8,  -8),   S( 48,  10),   S(  9,  36),   S( 49,  54),   S( 68,  30),

            /* rooks: bucket 2 */
            S(-65,  94),   S(-48,  75),   S(-49,  77),   S(-40,  50),   S(-25,  49),   S(-38,  44),   S(-36,  36),   S(-75,  81),
            S(-57,  86),   S(-64,  89),   S(-54,  79),   S(-49,  55),   S(-57,  66),   S(-50,  37),   S(-25,  26),   S(-33,  49),
            S(-54,  88),   S(-38,  81),   S(-57,  70),   S(-37,  61),   S(-43,  55),   S(-40,  52),   S(-22,  29),   S(-12,  47),
            S(-51, 104),   S(-41,  94),   S(-52,  87),   S(-68,  82),   S(-50,  73),   S(-37,  63),   S(-17,  63),   S(-28,  61),
            S(-31, 106),   S(-38, 103),   S(-26, 103),   S(-22,  70),   S(-35,  82),   S( 19,  63),   S( -9,  65),   S(-10,  86),
            S(  3, 101),   S( 16,  93),   S( 30,  76),   S(-42,  83),   S( 50,  45),   S( 52,  65),   S( 72,  36),   S( 67,  71),
            S( 54,  90),   S(  0, 104),   S( 12,  88),   S( 66,  43),   S( 34,  48),   S( 29,  66),   S(-31, 112),   S( 13, 107),
            S( 10,  68),   S( 41,  63),   S( 51,  36),   S(-17,  48),   S(-18,  28),   S( 23,  23),   S(-11,  63),   S(  5,  57),

            /* rooks: bucket 3 */
            S( -9, 108),   S( -7, 107),   S(-12, 134),   S(  1, 123),   S( 10,  88),   S( 14,  78),   S( 26,  56),   S( -2,  41),
            S( -2, 101),   S(-12, 122),   S(-11, 133),   S( -2, 131),   S( -2,  96),   S( 22,  53),   S( 54,  26),   S( 26,  54),
            S( 11, 102),   S( -6, 121),   S(-17, 127),   S(  0, 132),   S(  8,  87),   S( 14,  70),   S( 29,  75),   S( 33,  46),
            S(  1, 141),   S(  2, 148),   S(-27, 156),   S( -6, 143),   S( -5, 108),   S( 15,  92),   S( 42,  86),   S(  0,  80),
            S( -9, 152),   S(-21, 165),   S(  4, 161),   S( 16, 151),   S(  3, 129),   S( 32, 110),   S( 68,  85),   S( 52,  85),
            S( -2, 154),   S( 26, 142),   S( 19, 151),   S( 31, 151),   S( 87,  94),   S(109,  94),   S( 98,  89),   S( 41, 100),
            S( -6, 170),   S(  3, 165),   S( 18, 170),   S( 19, 166),   S( 11, 155),   S( 76, 115),   S( 95, 172),   S(164, 105),
            S( 99,  41),   S( 58,  96),   S( 22, 145),   S( 23, 130),   S( 18, 123),   S( 86,  95),   S( 49,  69),   S(133,  51),

            /* rooks: bucket 4 */
            S( -8, -33),   S( -1, -10),   S( 35, -43),   S(-29,  15),   S(-46,  18),   S(-16,  25),   S(-46,   5),   S(-79,  33),
            S(-21, -66),   S(-42, -11),   S( 31, -34),   S(  0, -22),   S(-12,   6),   S(-19,   9),   S(-23,  20),   S( 16,  23),
            S( -6, -43),   S(-31, -13),   S(-33,  -9),   S(-17, -26),   S(-33,   9),   S(-75,  18),   S( -8,  18),   S(-39,  12),
            S(-35, -51),   S( 35,   7),   S( 33, -42),   S(-16, -26),   S( 34,  -7),   S(-16,  20),   S(-32,  10),   S(-31,   5),
            S(-20, -51),   S( 25, -34),   S( 44,  -4),   S( 41, -26),   S( 67,  -5),   S( 36,  22),   S( -8,  -3),   S(  8,  24),
            S(  2,   9),   S(  5,  -6),   S( 24, -21),   S( 48,  22),   S( 23,  26),   S( 16,  31),   S( 18,  31),   S( 28,  32),
            S(  6, -34),   S( 29,   9),   S( 50, -13),   S( 56, -11),   S( 42,   9),   S(-15,   2),   S( 11, -10),   S( 10,  13),
            S( 16, -17),   S(  1,  -2),   S( 39, -32),   S(  8, -12),   S( 76,   8),   S( 29,  14),   S( 18,   2),   S( 19,  41),

            /* rooks: bucket 5 */
            S(  1,   1),   S( 15, -19),   S( 11,  -9),   S( 17,   8),   S( -2,   9),   S( -9,  22),   S(-34,  53),   S(-32,  28),
            S( 13, -24),   S(-27, -21),   S( -3, -28),   S(  9, -10),   S(-36,  11),   S(-54,  11),   S(-95,  54),   S( -3,   7),
            S(-93,  41),   S( 30, -36),   S(-10, -22),   S(-34,   3),   S(-61,   9),   S( 34, -15),   S(-79,  28),   S(-65,  19),
            S(-79,  34),   S(-16,   1),   S( 46, -30),   S( 52, -17),   S( 26, -15),   S( -2,  28),   S(-22,  33),   S(  2,  23),
            S( 37,  -4),   S( 11,   6),   S( 21,  22),   S( 20, -19),   S(  2,  13),   S( 97,  -8),   S( 31,  24),   S( 40,  38),
            S(  7,  13),   S(-15,  11),   S( -9,  -2),   S( -9,   2),   S( 26,   5),   S( 13,  20),   S( 69,  10),   S( 49,  38),
            S( 26,   2),   S( 14,  -7),   S(-15,  -7),   S( 27,   8),   S( 53, -17),   S( 56, -27),   S( 88, -22),   S( -4,  15),
            S( 46,  10),   S( 41,  -3),   S( 43,  -2),   S(  5,  22),   S( 28,   6),   S( 31,  22),   S( 35,  28),   S( 16,  36),

            /* rooks: bucket 6 */
            S(-60,  49),   S(-16,  17),   S(-24,  17),   S(-29,  13),   S(-14,   7),   S(  6,   0),   S(  6, -17),   S(-26,  14),
            S(-114,  49),  S( 12,   7),   S(-48,  27),   S( -5,   0),   S( -5, -12),   S(-27,  -3),   S(-36,  -6),   S(-53,  30),
            S(-86,  43),   S( -7,  16),   S(-28,  11),   S(-12,   1),   S(-20,   9),   S( -5,  -3),   S( -7, -16),   S(-21,  -4),
            S(-73,  56),   S(-49,  45),   S(  2,  13),   S( 24,   5),   S( 26,  -8),   S( -3,   4),   S(-20,  17),   S(  1,  19),
            S(-12,  40),   S( 33,  27),   S( 97,   6),   S( 26,   9),   S( 29,  -5),   S(  8,  23),   S( 36,   6),   S( 80,  -7),
            S( 63,   9),   S( 44,  19),   S(108,  -4),   S( 45,  -4),   S(  2, -22),   S( 22,  41),   S( 63, -24),   S( 71,  13),
            S( 32,  14),   S(104, -15),   S( 98, -28),   S( 84, -35),   S( 57, -22),   S( 37,   3),   S( 81, -39),   S( 76, -23),
            S( 34,  11),   S(-21,  45),   S( 25,  18),   S( 53, -13),   S( 37,   7),   S( 54,  23),   S( 65,  13),   S( 45,  15),

            /* rooks: bucket 7 */
            S(-107,  45),  S(-85,  42),   S(-55,  37),   S(-60,  48),   S(-39,  12),   S(-23, -12),   S(-43,  16),   S(-69, -13),
            S(-87,  34),   S(-46,  22),   S(-73,  37),   S(-74,  51),   S(-48,   4),   S(  0, -18),   S( 11,   0),   S(-18, -34),
            S(-91,  33),   S(-75,  39),   S(-23,  10),   S(-90,  49),   S(-42,   8),   S(-44,  20),   S( 40, -24),   S( -9, -37),
            S(-73,  39),   S(-20,  21),   S(-10,  11),   S( 45,  -6),   S( -6,   7),   S( 26,  -2),   S( 56,  11),   S( -8, -12),
            S( -5,  26),   S(  9,  28),   S( 50,  24),   S( 30,  23),   S(134, -32),   S(112, -41),   S(115, -12),   S(-66,  -4),
            S( 21,  20),   S( 14,  24),   S(117,  -1),   S( 82,  -6),   S(111, -17),   S( 81,   8),   S( 28,  35),   S(-10, -24),
            S(-24,  21),   S( 10,   8),   S( 54,   5),   S( 71,  -5),   S(109, -29),   S(104, -22),   S( 59,   1),   S( 34, -31),
            S(-62,   8),   S(-16,  26),   S( 21,  18),   S( 42,   8),   S( 50,  -1),   S( 62,   2),   S( 33,  24),   S( 33, -12),

            /* rooks: bucket 8 */
            S(  1, -91),   S(  2, -64),   S( 41, -51),   S( 20, -20),   S(-13, -55),   S(-13, -47),   S(-10, -69),   S(-14, -33),
            S(-23, -88),   S(  3, -37),   S( 27, -14),   S( -4, -75),   S( -1, -70),   S(  6, -34),   S( -2, -12),   S(-48, -67),
            S( 12,  -5),   S( -8, -31),   S( 19,  12),   S(-17, -16),   S(  0,  16),   S( 39,  30),   S(  4,  34),   S(-12, -23),
            S(-15, -42),   S(  4,  -6),   S( -7, -32),   S( 10, -20),   S( -1, -13),   S( 32,   8),   S(  5,  24),   S( -6, -17),
            S(-10, -42),   S(  9,  -4),   S( 15,  -1),   S( 34,  18),   S( 10,  22),   S(-15, -15),   S( 10,  22),   S( -4, -14),
            S(-10, -24),   S( 16, -29),   S( 23,  -1),   S(-11, -24),   S( 15,  41),   S(-15, -10),   S( -7,   1),   S(  1,  18),
            S( 18,   4),   S( 25, -10),   S( 10, -12),   S( 28,  29),   S( 22,  40),   S( 20,  23),   S(  7,  18),   S( 10,  31),
            S( -6,  -1),   S( 19, -31),   S( 15, -13),   S( 25,  31),   S(-20,  -1),   S( 12,  17),   S( -2,   5),   S(  6,  18),

            /* rooks: bucket 9 */
            S(-19, -100),  S(  6, -93),   S(  0, -105),  S(  0, -78),   S( 34, -74),   S( 19, -52),   S( -1, -36),   S( -3, -72),
            S(-36, -62),   S(-12, -108),  S( -9, -75),   S(-21, -65),   S( -7, -89),   S(-37, -26),   S(-22, -78),   S(-21, -55),
            S( -3, -38),   S(-10, -33),   S( 25,  28),   S(-18, -47),   S( 10, -61),   S(-15, -18),   S(-11, -12),   S(  3,  29),
            S(  9, -44),   S(  6, -16),   S(  7,  -2),   S( -9, -34),   S(  2, -49),   S( 20, -11),   S( -4, -38),   S(  0, -34),
            S( 16, -46),   S(  2, -36),   S( -1, -66),   S(  1, -16),   S(-10, -61),   S(-16, -36),   S(-11, -54),   S( -5, -42),
            S( -3, -55),   S(-26, -54),   S( -7, -50),   S( 20, -23),   S( 18, -27),   S( -7, -18),   S( -7, -15),   S( -9, -24),
            S(  6,  -6),   S(  6, -46),   S( -7, -62),   S( -6, -18),   S( 17, -27),   S( 26, -10),   S(  9, -16),   S(-23, -50),
            S( -4,  -5),   S( -9, -18),   S(  4,  -9),   S(  4,  -5),   S( 17, -20),   S(  4,  17),   S(-19, -20),   S(  8,   5),

            /* rooks: bucket 10 */
            S( 12, -76),   S(-12, -75),   S( 10, -88),   S( 17, -112),  S( 27, -100),  S( 38, -121),  S( 42, -120),  S( -6, -95),
            S(-27, -67),   S(-29, -67),   S(-21, -55),   S(-32, -93),   S( -8, -90),   S(-15, -93),   S(  7, -94),   S(-37, -111),
            S(  2, -30),   S(-35, -41),   S(-12, -63),   S(-52, -82),   S( -5, -28),   S( -6, -46),   S( 15, -82),   S(-14, -31),
            S(-21, -49),   S(-34, -69),   S( -8, -42),   S( -7, -36),   S(  4, -19),   S( -4, -11),   S(-20, -102),  S(  7, -64),
            S( -3, -50),   S(  7, -40),   S( -9, -60),   S( -8, -57),   S(  3, -28),   S(  1, -35),   S( 19, -81),   S(-18, -89),
            S(-18, -40),   S(  2, -15),   S(-10, -45),   S(-11, -74),   S( 11, -45),   S(  6, -51),   S(-28, -65),   S( -4, -44),
            S(-36, -46),   S(-29, -35),   S(-12, -50),   S(  8, -54),   S( 31, -51),   S(  4, -35),   S(-16, -76),   S( -8, -51),
            S(-23, -23),   S(  4,   8),   S(-10,  -4),   S(-23, -36),   S(-10,  -5),   S(-25, -12),   S(  0, -42),   S(-13,  -8),

            /* rooks: bucket 11 */
            S(-30, -68),   S(-29, -36),   S(  1, -64),   S(  5, -37),   S(-12, -41),   S( 47, -84),   S( 36, -53),   S(-16, -87),
            S(-12, -32),   S(-19, -39),   S(-43, -34),   S(-35, -31),   S(-29, -11),   S(  2,  -9),   S(-29, -69),   S(-19, -65),
            S(-16,  -2),   S(-26,  -9),   S(  5,   2),   S(-22,  -2),   S(  5, -26),   S( 12,  -9),   S( 14, -39),   S(-22, -36),
            S(-18, -56),   S(  4, -40),   S(  1,   3),   S( 28,  11),   S( 11,  -1),   S(-12, -62),   S(  8,   8),   S(  0, -12),
            S(  1, -54),   S(  2,   1),   S(  1,  -5),   S( -4,  -3),   S( 45,  -6),   S( 18, -51),   S( 27,  12),   S(-29, -78),
            S(-25, -45),   S(-27, -15),   S( 17, -33),   S( 27,  -9),   S(-16, -24),   S( 14, -25),   S( 27, -28),   S( 12, -40),
            S(-19, -22),   S(-19, -25),   S( 17,  -6),   S(-15, -38),   S( -7, -18),   S( 44, -26),   S( 31, -16),   S( 20, -34),
            S(  6,  20),   S( 28,  20),   S(  1,  16),   S( 10,  15),   S(-23,   7),   S( 18,  -8),   S( 60, -22),   S(-13, -14),

            /* rooks: bucket 12 */
            S(-25, -113),  S( -1, -13),   S(  0, -30),   S(-26, -86),   S( -4, -49),   S( 19,   2),   S( -3, -26),   S(-17, -54),
            S(  3,  -9),   S(  0,  -7),   S( 10,  10),   S(  1,  -4),   S( -1, -15),   S( 20,  -7),   S(  1, -18),   S(-26, -67),
            S( -9, -28),   S(  3,  20),   S(  1, -10),   S( 14,  -3),   S( 15,  -5),   S( 13, -16),   S(  4,  11),   S(  8,  21),
            S( -6, -12),   S(-10, -12),   S(  8,  18),   S( 15,  28),   S(  7,  -1),   S(  8,  17),   S(  6,  12),   S(  2,   2),
            S( 26,  28),   S( 10,  21),   S(  8,  -9),   S( -5, -46),   S(  8,  15),   S(-12, -16),   S(  2,  -3),   S(  5,  13),
            S( -8, -25),   S( -3, -47),   S(  8,   0),   S(-11, -40),   S(  8,   4),   S( -4, -25),   S(  1,  17),   S(  8,   2),
            S(-17, -21),   S( -8, -29),   S( 18,  17),   S(  8,  19),   S(-17, -47),   S( 11,   4),   S(  7,  30),   S(  3,   6),
            S(-10, -24),   S( -2, -19),   S(  9, -13),   S(  1,  -6),   S(  6,   6),   S( -4, -21),   S( -2, -10),   S(  9,  30),

            /* rooks: bucket 13 */
            S(-20, -78),   S(-23, -75),   S(-19, -56),   S( -4, -44),   S(-26, -86),   S( 15, -30),   S(-20, -58),   S(-20, -44),
            S(-15, -40),   S( -5, -42),   S(  0,  -5),   S( -5, -34),   S(  4, -30),   S( -1, -49),   S( 19, -27),   S( -7, -47),
            S(-16, -70),   S(-10, -48),   S( -3, -20),   S( 10, -18),   S( 20,  14),   S( 10, -27),   S( 21, -11),   S( -6, -86),
            S(  8,  -4),   S( -7, -43),   S( 16,  11),   S( 17,  -2),   S( 13,   7),   S( -8, -34),   S(  5,   5),   S(  0,   9),
            S(  6, -17),   S( -3, -86),   S(  3, -49),   S( 13, -11),   S( 15, -33),   S(  1, -12),   S( -2,   1),   S(  1, -20),
            S(  9,   8),   S(-10, -52),   S( -9, -44),   S(-18, -95),   S( -6, -92),   S(  3, -19),   S(-11, -38),   S( -6, -32),
            S(-11, -62),   S(-12, -61),   S(-16, -61),   S( -6, -24),   S( -5, -68),   S( -1, -22),   S(  3, -28),   S( -1, -11),
            S(  0, -22),   S(-13, -39),   S(  1, -33),   S(  8,   0),   S( -2, -50),   S( 10,  16),   S( -2, -14),   S(  2,  -8),

            /* rooks: bucket 14 */
            S(  8, -48),   S(-23, -51),   S( -8, -46),   S(-22, -96),   S(-12, -65),   S(  6, -30),   S(-46, -142),  S(-16, -62),
            S(  9, -27),   S( -2, -46),   S( -4, -68),   S(-12, -80),   S( -5, -34),   S(  6,  12),   S( -5, -51),   S(-11, -79),
            S( -3, -25),   S( -3, -14),   S(  2, -25),   S( 10,  -6),   S(  9,  -9),   S( -4, -14),   S(  7, -26),   S(-20, -104),
            S( -4,  -2),   S(  8,   6),   S(  1,  -2),   S( -9, -42),   S( -1, -37),   S( -3, -21),   S( -4, -40),   S( -5, -31),
            S(  4,   7),   S( 12,  31),   S(  1, -20),   S(  3, -60),   S(  4, -23),   S( 15, -10),   S( -1, -55),   S(  0, -25),
            S(  6,  -1),   S(  1,  -4),   S( -4, -49),   S(  8, -58),   S(  0, -70),   S(  3, -66),   S( -8, -73),   S(-21, -76),
            S( -8,   3),   S(  1,  -5),   S( -4, -42),   S(-21, -84),   S( -9, -43),   S(  0, -11),   S(-22, -78),   S(-15, -39),
            S(  1,  -7),   S( -1, -11),   S( -1, -20),   S( -9, -62),   S(-21, -79),   S(-28, -118),  S( -1, -63),   S(  2,  -2),

            /* rooks: bucket 15 */
            S(-16, -81),   S(-17, -62),   S(-14, -21),   S(-18, -74),   S( -4, -37),   S( -7, -37),   S( -2, -27),   S(-19, -79),
            S(  7,  -6),   S(-13, -54),   S(  9,   0),   S(-19, -54),   S( -4, -34),   S(  8,  -4),   S( 11,  20),   S(  8,  14),
            S(  1,  -6),   S(  6, -29),   S(  9,   7),   S(  4, -40),   S(  3, -27),   S(-14, -47),   S( 14,  28),   S(  5,   2),
            S( -3,   8),   S( -6,   9),   S( -1,  -9),   S(-11, -39),   S( 11,  15),   S( -6, -40),   S(  1, -45),   S( 10, -15),
            S( 12,  18),   S( -5, -33),   S(  9,  23),   S( -2, -16),   S( 11,  19),   S(  8, -15),   S( 14,  -3),   S(-10, -58),
            S(  2,  21),   S(  7,  43),   S(  6,   9),   S( -6, -27),   S( -6, -43),   S( 10, -47),   S( -5, -52),   S( -3,  -4),
            S(  0, -18),   S( -7,  -8),   S(  7,   1),   S(  3, -22),   S( -4, -12),   S(  1, -34),   S(  5, -39),   S( -7, -47),
            S(  4,  -7),   S(  9,  26),   S(  4,  10),   S(  5,   4),   S(  3,  16),   S( -5, -49),   S( -3, -42),   S(-14, -37),

            /* queens: bucket 0 */
            S(-13,   4),   S(-42, -71),   S(-42, -54),   S( -2, -116),  S(  0, -76),   S(  1, -46),   S(-92, -31),   S( -2,  -3),
            S(-30, -34),   S( 27, -105),  S(  0, -50),   S(-11, -45),   S(  1, -22),   S( -9, -35),   S(-11, -79),   S(-61, -12),
            S(-22,  22),   S(  0, -17),   S( 17, -57),   S(-14,  -3),   S(-13,  11),   S( -3, -29),   S(-19, -24),   S(-79, -112),
            S(-35,  26),   S( 11, -29),   S(-17,  49),   S(-19,  55),   S(-10,  51),   S(-25,  11),   S(-33,   4),   S(-21, -28),
            S(-15,  -3),   S(-27, 100),   S(  5,  39),   S(  0,  70),   S( 20,  37),   S(-21,  46),   S(-43,  22),   S(-50, -25),
            S( -7,  10),   S( 41,  77),   S( 37,  76),   S(-53, 112),   S(-57,  80),   S(-33,  59),   S(-85,  14),   S(-60, -28),
            S(  0,   0),   S(  0,   0),   S( 35,  22),   S(-20,  32),   S(-24,  28),   S(-89,  91),   S(-93,  68),   S(-111,   3),
            S(  0,   0),   S(  0,   0),   S( 19,  12),   S(  2, -19),   S(-61,  53),   S(-48,  28),   S(-74,  23),   S(-30, -30),

            /* queens: bucket 1 */
            S(-10, -27),   S( -2, -46),   S( 13, -80),   S( 22, -105),  S( 22, -58),   S(-14, -67),   S( -4, -22),   S(-18,  24),
            S(-20,   8),   S( 18,   7),   S( 37, -52),   S( 23,  -3),   S( 31, -16),   S( -7,  -6),   S(-57,  50),   S(-30, -28),
            S( 32, -32),   S( 15,   1),   S(  5,  11),   S( 22,   5),   S(-18,  70),   S( 19,  54),   S(  0,   7),   S( 10,  -7),
            S( 28, -32),   S(  6,  34),   S(  2,  48),   S( 20,  57),   S(  3,  99),   S(-11,  57),   S( 25,  17),   S( -9,  37),
            S( 19,  33),   S( 38,  26),   S( 31,  85),   S( 12,  85),   S( 33,  98),   S( 53,  28),   S(-16,  63),   S( 13,  68),
            S( 21,  18),   S( 76,  48),   S( 95,  42),   S( 91,  80),   S( 65,  62),   S(  6, 103),   S( 31,  38),   S(  6,  16),
            S(103, -10),   S( 66,  17),   S(  0,   0),   S(  0,   0),   S(  4,  74),   S(-31,  81),   S(-18,  92),   S(-45,  40),
            S( 61,  12),   S( 67,  32),   S(  0,   0),   S(  0,   0),   S( 30,  36),   S( 44,  40),   S( 93,  40),   S( -8,  45),

            /* queens: bucket 2 */
            S( 18, -33),   S( 28, -28),   S( 28,   8),   S( 38, -35),   S( 41, -33),   S( 34, -63),   S(-11, -43),   S(-19,  15),
            S( 14,  24),   S( -1,  46),   S( 39, -10),   S( 38,   4),   S( 53, -25),   S( 20,  -2),   S( 32, -29),   S(  3,  51),
            S( 37,  26),   S( 31,  23),   S( 18,  82),   S( 17,  64),   S( 28,  54),   S( 17,  55),   S( 39,  32),   S( 21,  44),
            S( 26,  29),   S( 22,  71),   S( 10, 102),   S(  1, 120),   S( 33,  90),   S( 11,  82),   S( 31,  75),   S( 43,  84),
            S( -4,  76),   S( 34,  49),   S(  3, 122),   S( 26, 111),   S( 38, 124),   S( 84,  53),   S( 72,  93),   S( 70,  78),
            S(-38, 103),   S(-38, 111),   S( -8, 115),   S( 92,  59),   S( 63,  67),   S( 97, 127),   S(129,  57),   S( 23, 136),
            S(-15,  77),   S(-28, 107),   S(  8, 106),   S( 77,  57),   S(  0,   0),   S(  0,   0),   S( 22,  96),   S( 46,  81),
            S( 45,  14),   S( 19,  36),   S( 64,  23),   S( 75,  67),   S(  0,   0),   S(  0,   0),   S( 87,  95),   S( 19,  98),

            /* queens: bucket 3 */
            S(-34,  54),   S(-14,  48),   S(  1,  39),   S(  6,  47),   S( -6,  27),   S( -4, -16),   S(  2, -30),   S(-33,  33),
            S(-34,  50),   S(-17,  52),   S( -3,  65),   S(  2,  83),   S(  6,  65),   S( 11,  38),   S( 28,   5),   S( 50, -53),
            S(-28,  48),   S(-11,  86),   S(-14, 127),   S(-19, 146),   S(-11, 119),   S(-10, 106),   S( 10,  90),   S(  2,  39),
            S(-27,  96),   S(-36, 120),   S(-29, 163),   S(-15, 168),   S(-17, 164),   S(-12, 125),   S( 11, 104),   S(  1,  94),
            S(-24,  93),   S(-19, 132),   S(-28, 154),   S(-27, 192),   S(-12, 184),   S( -3, 201),   S( -8, 168),   S(-10, 127),
            S(-44, 110),   S(-35, 145),   S(-53, 188),   S(-34, 184),   S(-33, 212),   S( 33, 173),   S( -9, 197),   S(-18, 183),
            S(-83, 139),   S(-86, 161),   S(-64, 193),   S(-73, 186),   S(-70, 218),   S( 19, 119),   S(  0,   0),   S(  0,   0),
            S(-135, 187),  S(-74, 146),   S(-57, 148),   S(-56, 154),   S(-26, 151),   S(-23, 139),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-23,   9),   S(-71, -49),   S(  8,  13),   S(-20, -14),   S(-24, -21),   S(-13,   5),   S(-27, -29),   S( 25,  14),
            S(  2,   6),   S(-10,  16),   S( 15,  15),   S(-29, -28),   S( -3,  -4),   S(  6,  14),   S(-41, -11),   S(-23, -12),
            S(  0,  31),   S( 16, -25),   S( 18,  -6),   S( 15,  -4),   S( 38,  -7),   S( 28,  18),   S( -9, -23),   S( 17,   9),
            S(-19, -22),   S( 19,  -6),   S( 16,  -5),   S(  9,  14),   S( 47,  43),   S(-28,  33),   S(-37,  -8),   S(-23,   8),
            S(  0,   0),   S(  0,   0),   S( 39,  31),   S( 53,  48),   S(  9,  44),   S(  5,  47),   S( 10,  -4),   S(-28,   3),
            S(  0,   0),   S(  0,   0),   S( 52,  54),   S( 48,  65),   S( 16,  34),   S(  9,  34),   S( 13,  32),   S(-37, -11),
            S( 14,   7),   S( 20,  15),   S( 44,  28),   S( 58,  47),   S( 61,  42),   S( -7, -17),   S( 25,  20),   S(-36,  -7),
            S( 55,  20),   S(-17, -25),   S( 28, -14),   S( 67,  49),   S( 12,   4),   S(-29, -21),   S(-18, -19),   S(-32, -23),

            /* queens: bucket 5 */
            S( 18,  -7),   S( 27, -13),   S( -1,  -2),   S(  7,  23),   S( 34, -21),   S( 15,  17),   S( 42,  24),   S( -9, -19),
            S( 13,  -7),   S(-17,  11),   S(-33, -13),   S( -6,  -2),   S(-12,  34),   S(-17, -32),   S( 47,  15),   S( -4,   1),
            S( 33,  18),   S( 16, -39),   S( 10,   2),   S(-37,  13),   S(  1,  22),   S( 30,  27),   S( 21,  42),   S( 15,  22),
            S(  6, -34),   S( 63,  25),   S( 17,  13),   S( 48,  40),   S( 73,  27),   S( 11,   4),   S( 14,  77),   S(-26,   5),
            S( 38,  32),   S( 16, -14),   S(  0,   0),   S(  0,   0),   S( 28,  16),   S( 22,  24),   S( 22,  17),   S(  8,  22),
            S( 44,  30),   S( 63,  60),   S(  0,   0),   S(  0,   0),   S( 69,  62),   S( 84,  45),   S( 39, -10),   S( 49,  34),
            S( 54,  22),   S( 89,   4),   S( 69,  83),   S( 35,  52),   S( 54,  24),   S(101,  58),   S( 53,  61),   S( 42,  -6),
            S( 28,  37),   S( 68,  46),   S( 94,  58),   S( 48,  31),   S( 53,  59),   S( 58,  37),   S( 37,  33),   S(  6, -11),

            /* queens: bucket 6 */
            S( 36,  20),   S(-10,  -7),   S( 25,  -1),   S( 30,  -6),   S(-15,   4),   S(-18, -17),   S(-16, -19),   S( -2,  13),
            S( -4,  21),   S( 62,  10),   S( 29,  25),   S( 21,  34),   S( 48, -10),   S(-12,  -1),   S(-55,  -6),   S(  7,  33),
            S(-41,  13),   S(  7,  25),   S( 35,  18),   S( 20, -10),   S( 23,   5),   S( 40, -14),   S( 50,  26),   S( 51,  37),
            S(-26,  -5),   S(-11,  37),   S( 32,  18),   S( 93,  38),   S( 29,  -3),   S( 38,  35),   S( 83,  29),   S( 95,  38),
            S(-36,   0),   S(-14,  32),   S( 34,  58),   S( 69,  16),   S(  0,   0),   S(  0,   0),   S( 80,  56),   S( 89,  74),
            S( 12,  22),   S( 59,  33),   S( 60,  62),   S( 67,  57),   S(  0,   0),   S(  0,   0),   S(119,  65),   S(122,  38),
            S( 34,  15),   S( -1,   9),   S( 81,  38),   S( 60,  32),   S( 50,  64),   S( 84,  66),   S( 96,  31),   S(145,  17),
            S( 22,  13),   S( 74,   8),   S( 66,  31),   S(107,  62),   S(124,  39),   S( 92,  43),   S( 93,  28),   S(101,  21),

            /* queens: bucket 7 */
            S(-27, -13),   S( -1, -21),   S(-40,  12),   S(-47,  26),   S(  5, -21),   S(-40,   1),   S(-55, -22),   S( -1,  -8),
            S(-24, -23),   S(-74,  -3),   S( -8,  36),   S( -9,  39),   S(-25,   4),   S(  9,   7),   S(-14,  -1),   S(-38,  -4),
            S(-29, -15),   S(-28,  15),   S(  0,  32),   S(  0,  48),   S( 33,  11),   S( 22, -14),   S(  3, -12),   S( 46, -13),
            S(  2, -20),   S(-10,  -5),   S(  8,  34),   S( 38,  51),   S( 88,  18),   S( 66,   6),   S( 56,  -8),   S( 13, -10),
            S(-19,  12),   S(-16,  15),   S( 20,  78),   S( 15,  54),   S( 66,  37),   S( 75,  36),   S(  0,   0),   S(  0,   0),
            S(  1,   4),   S( -8,  19),   S(-41,  68),   S(-13,  52),   S( 68,  -8),   S(117,  64),   S(  0,   0),   S(  0,   0),
            S(-51,  48),   S(-48,   8),   S(-11,  16),   S( 53,  11),   S( 63,  36),   S(115,  17),   S( 70,  41),   S( 65,  37),
            S( 11, -24),   S( 18, -17),   S( 20,  27),   S( 40,  -1),   S( 60,  28),   S( 51,  -7),   S(-18,  -9),   S( 86,   3),

            /* queens: bucket 8 */
            S( -7, -26),   S( 11, -14),   S(-34, -65),   S(  2, -11),   S( -3, -20),   S( 13,   4),   S(  5, -18),   S(  2,  -1),
            S(-22, -37),   S( -1,   1),   S(-10, -27),   S( 10,  22),   S(  5,   7),   S( -2, -14),   S(-12, -15),   S( -4,  -8),
            S(  0,   0),   S(  0,   0),   S(  8,   0),   S(  4,  -8),   S( 20,   9),   S( -6,  -7),   S( -8,  -8),   S( -6,  -2),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S(  2,  16),   S(  6,  -1),   S( -4, -18),   S( 16,  34),   S(  3,  -9),
            S(  9,   4),   S(  6,  12),   S( 14,  23),   S( 46,  26),   S(  6,  13),   S( 16,  30),   S(  5,   8),   S(-17, -34),
            S(  6,  -7),   S(  3, -10),   S( 19,  26),   S( -1, -17),   S( 19,  24),   S( 31,  19),   S(  0, -21),   S(-10, -24),
            S(-16, -33),   S(  5,  -7),   S( 25,  23),   S( 19,  24),   S(  3,  -8),   S( 21,  29),   S(  4, -15),   S(  8,   3),
            S(  6,  -6),   S( -6, -11),   S(  8,  -1),   S( 10,  26),   S(  8, -10),   S(-20, -37),   S(  3,   6),   S(-35, -67),

            /* queens: bucket 9 */
            S( 30,  26),   S(-18, -48),   S( -3, -17),   S( 17,   0),   S( -8, -12),   S( -1, -16),   S( -7, -15),   S( -2, -11),
            S( 25,  25),   S( -3, -17),   S( -7, -13),   S( 17,   6),   S(-12, -27),   S( -3, -20),   S( -1,  -3),   S( -4, -15),
            S(-11, -32),   S( -7, -19),   S(  0,   0),   S(  0,   0),   S( 10,   6),   S( 29,  20),   S( -8,  -4),   S( 11,  10),
            S( 18,  16),   S(-17, -35),   S(  0,   0),   S(  0,   0),   S(  7,  -1),   S( 32,  27),   S(  4,   4),   S( -9,  -4),
            S( 25,  23),   S( 14,   2),   S( 10,  12),   S( -8,  -9),   S(  2, -15),   S( 26,  31),   S( -2, -25),   S( -6, -36),
            S(  3,   3),   S(  1, -28),   S( 23,  40),   S( -3,   0),   S(  8, -12),   S(-12, -24),   S( -5,  -7),   S( -2, -23),
            S(-15, -18),   S( 35,  16),   S( 13,  26),   S(  0,   4),   S( 17,  20),   S( 19,   5),   S( -3,   5),   S( 17,  -2),
            S( -8, -23),   S( 20,   0),   S(-21, -36),   S(  5,   7),   S(  7,  10),   S( -5, -23),   S(-10, -24),   S(  3, -20),

            /* queens: bucket 10 */
            S( 16,  37),   S( 19,   2),   S( -7, -14),   S(  4,  -2),   S(  2, -11),   S( 27,  29),   S(  8,  11),   S( -6, -14),
            S( -3, -14),   S(-14, -32),   S( 21,   5),   S(-13, -43),   S( -4, -10),   S( 18,   0),   S(  8,  -8),   S( -3, -19),
            S( -2,  -2),   S( 13,  16),   S(  9,  -5),   S( 12,   8),   S(  0,   0),   S(  0,   0),   S( 12,   4),   S(  0,   0),
            S( -6, -21),   S(  5, -11),   S(  6,   3),   S( -8, -17),   S(  0,   0),   S(  0,   0),   S( -1, -15),   S( -8, -37),
            S(  6,   0),   S( 30,  16),   S(  0,  -1),   S( 43,  40),   S(  9,  11),   S( -2,  -8),   S(  4, -10),   S( 17, -11),
            S(  2,  -1),   S(  4, -12),   S( 25,  23),   S( 10,  18),   S( 22,  20),   S( 20,  29),   S( 13,  12),   S(-13, -46),
            S( -5, -26),   S( 23,  42),   S( 16,  17),   S( 22,  22),   S( -5, -17),   S( 19,  17),   S( 25,  -5),   S(  3, -13),
            S(-15, -18),   S( -4, -23),   S( 12, -16),   S(-22, -57),   S( 29,  31),   S(  4,  -4),   S( -2, -22),   S( 12, -26),

            /* queens: bucket 11 */
            S(-15, -10),   S( -5, -18),   S(-13, -28),   S(-11, -20),   S(  7,   6),   S(-13, -24),   S(  7, -12),   S( -5, -27),
            S(-10,  -9),   S(  6, -14),   S(-31, -39),   S( -6, -15),   S(  9,  -6),   S( -1, -10),   S( 17,   6),   S( 11, -10),
            S( 11,  20),   S(  6,   6),   S( -8,  11),   S(-14, -40),   S( -4,  -3),   S(-27, -45),   S(  0,   0),   S(  0,   0),
            S( -8,  -6),   S(-10,  -7),   S(  1,  16),   S( 10,  -3),   S( -2, -22),   S(  6,   6),   S(  0,   0),   S(  0,   0),
            S( 12,  11),   S(  9,  10),   S( -3,  11),   S(  7,  14),   S( 55,  77),   S( 31,  41),   S(  1,  -1),   S(-10, -17),
            S(-24, -23),   S( -5, -14),   S(-23, -35),   S( -2,   1),   S( 20,  38),   S(  5, -22),   S(  8,   5),   S( 20,  -3),
            S(-14, -21),   S( -5, -11),   S( 17,  15),   S( 10,  27),   S( 10,  18),   S( 31,  29),   S(-14,   0),   S(  7, -10),
            S(-33, -74),   S(  2, -20),   S(-12, -31),   S(  7,  19),   S( 33,   7),   S( 16,  16),   S( 12,   8),   S(  7,  14),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  8,  15),   S( -5, -20),   S( -7,   5),   S( -9, -24),   S(  0,  -6),   S(  0,   1),
            S(  0,   0),   S(  0,   0),   S(  6,   6),   S( -1, -20),   S(  3,  -5),   S(-11, -28),   S( -8, -19),   S(  4,   5),
            S( -3,  -7),   S(  1,  -8),   S(-19, -46),   S(-12, -42),   S( 24,  46),   S(  4,  17),   S( -1,  -6),   S( 10,  12),
            S(  2,   0),   S( 20,  30),   S(  8,  24),   S(  8,  11),   S( 13,  -2),   S(-10, -15),   S( -5, -11),   S( -7, -12),
            S(-17, -35),   S(  1,  -8),   S(  7,  -5),   S(-16, -48),   S(  8,  23),   S(-19, -59),   S(-19, -53),   S( -1,   2),
            S(  1,  -7),   S( -8, -23),   S(-10, -13),   S(  5,   6),   S( -8, -23),   S(-11, -10),   S( -1,   4),   S( -5, -11),
            S(-12, -21),   S(  6,  10),   S(  2,   3),   S( 14,  26),   S( -3, -10),   S(-16, -38),   S(  5,   5),   S( -4, -24),
            S(  8,  14),   S(  0,  -3),   S(  9,  22),   S(  0,   3),   S( -7, -19),   S(-23, -48),   S(-13, -25),   S(-13, -19),

            /* queens: bucket 13 */
            S(-15, -52),   S(-16, -41),   S(  0,   0),   S(  0,   0),   S(  2,   0),   S( -2,  -1),   S( -8, -21),   S( -2,  -4),
            S( -3, -30),   S(  2,  -4),   S(  0,   0),   S(  0,   0),   S(-18, -32),   S(-16, -38),   S(  2,   6),   S( -6, -20),
            S( -7, -21),   S(-11, -28),   S( -6, -17),   S(  1,  -1),   S(-15, -31),   S( -2, -10),   S(-14, -21),   S(  1,   6),
            S( -4, -11),   S(-16, -45),   S( 11,   6),   S(  8,   8),   S( 21,  32),   S( 20,  35),   S(-10, -28),   S(-16, -35),
            S( 20,  27),   S(-10, -25),   S(  3, -12),   S( 24,  34),   S( -8, -22),   S(-23, -48),   S(-13, -31),   S( 11,  22),
            S(  4,   7),   S(-19, -45),   S( 10,  15),   S( 17,  29),   S(  7,   3),   S(-25, -45),   S(-16, -35),   S(-18, -44),
            S( -6,  -8),   S( -2,  -4),   S( 14,  28),   S( -7, -18),   S(  8,  12),   S(  4, -11),   S(-25, -52),   S(-18, -42),
            S( -5, -22),   S( -1,  -7),   S( -9, -20),   S(  2,   2),   S(  6,  13),   S(-10, -28),   S(  7,  13),   S(-21, -37),

            /* queens: bucket 14 */
            S(  4,   2),   S(  8,   2),   S( -9, -25),   S(  1,  -7),   S(  0,   0),   S(  0,   0),   S(  4, -10),   S(-10, -31),
            S(-13, -35),   S(-18, -52),   S( -2,  -8),   S( -8, -24),   S(  0,   0),   S(  0,   0),   S( -6, -12),   S(-12, -30),
            S( -6, -15),   S( -4, -35),   S( -7, -20),   S( -6, -19),   S(  4,   9),   S(  3,   3),   S(  5,   9),   S(-16, -44),
            S( -5, -14),   S(  1,   2),   S(  4,   9),   S( -1,  -4),   S( -7, -16),   S(-15, -38),   S( -8, -19),   S( -3, -14),
            S(-19, -36),   S(  6,   2),   S( -1, -16),   S( 15,  25),   S(  6,  11),   S( -2,  -7),   S( -5, -18),   S( -5, -22),
            S(-10, -20),   S(  8,  -6),   S(-19, -35),   S(-18, -46),   S( 11,  23),   S(  5,   8),   S(-16, -35),   S(-10, -18),
            S( -6, -13),   S( -7, -19),   S( -5, -12),   S(  0,   2),   S(  1,   4),   S(  5,   4),   S(-17, -34),   S( -6,   0),
            S(-23, -44),   S( 10,  -7),   S( -9, -13),   S( -2,  -5),   S(-11, -21),   S( -5,  -9),   S( -7, -14),   S(  2,  -7),

            /* queens: bucket 15 */
            S( -4, -10),   S(  2,   1),   S(  6,   1),   S( -7, -12),   S(  4,  -1),   S( -3,  -9),   S(  0,   0),   S(  0,   0),
            S( -5, -12),   S( -1,  -6),   S( -8,  -9),   S(-11, -28),   S( -1, -12),   S(  4,   8),   S(  0,   0),   S(  0,   0),
            S(  1,  -1),   S(  3,   5),   S( -6,   2),   S( -6, -15),   S(-11, -31),   S(  3,  -4),   S( -4, -13),   S( -6, -14),
            S( -2,  -1),   S(-12, -25),   S( -3, -10),   S(  4,  -5),   S(  3,  -3),   S(  2,  -2),   S(-10, -24),   S( -6, -21),
            S( -4,  -9),   S(  0,  -1),   S(-10, -24),   S( 15,  25),   S( 14,  27),   S( 13,  20),   S(  3,  -4),   S(-18, -45),
            S(  0,  -7),   S( -5, -12),   S( -7, -12),   S(-10, -29),   S( -2,  -2),   S(-11, -26),   S( -4, -10),   S( -8, -24),
            S( -7, -17),   S(-10, -22),   S(  4,   8),   S(  2,   1),   S( -6, -13),   S( -4,  -9),   S(  5,  12),   S(-15, -30),
            S(  1,   6),   S(-24, -54),   S( -2, -10),   S( -6, -10),   S( -6, -12),   S(-12, -29),   S(  6,  14),   S(-10, -20),

            /* kings: bucket 0 */
            S(  8, -34),   S( 18,   1),   S( 22, -11),   S( -6,  -7),   S(-19,   2),   S( 27, -31),   S( 13,  -2),   S( 16, -47),
            S(-21,  28),   S( -6,   6),   S(-17,  16),   S(-42,  18),   S(-38,  42),   S( -9,  21),   S( -8,  48),   S(-15,  33),
            S(  1,   2),   S( 69, -28),   S(-33,  17),   S(-10,  -4),   S( -8,  -6),   S(-28,   4),   S(-53,  27),   S( 23, -15),
            S(-10, -22),   S(  5, -12),   S(  0, -15),   S(-47,   8),   S(-52,  31),   S(-35,   2),   S(-23,  28),   S(-44,  13),
            S(-56, -91),   S( 19, -35),   S( 28, -36),   S( 21,  -6),   S(-17, -16),   S(-18,  12),   S(-28,   3),   S(  1,  15),
            S( -6, -109),  S( 29, -34),   S( 35, -69),   S( -5,  -1),   S(  9, -27),   S( 10, -13),   S( 26,  -3),   S(  8,  -1),
            S(  0,   0),   S(  0,   0),   S(  8, -20),   S(  8, -38),   S( 12, -26),   S(  8,  10),   S(-18, -17),   S(-27, -18),
            S(  0,   0),   S(  0,   0),   S(-12, -62),   S( 17, -35),   S( 10, -20),   S(  9, -10),   S(  6,   7),   S(  0,   1),

            /* kings: bucket 1 */
            S(  1, -30),   S( 30, -21),   S( 16, -14),   S( 13,   3),   S(-14,  -8),   S( 25, -18),   S(  9,  15),   S( 24, -21),
            S(  2,   9),   S(  5,  24),   S( 17, -18),   S(-48,  29),   S(-38,  30),   S(  1,  12),   S(  1,  28),   S(-11,  15),
            S(-13, -11),   S(  7,  -9),   S( 21, -24),   S( -5, -11),   S(-53,   5),   S(  6, -15),   S( 16,  -7),   S( 57, -24),
            S(  2,  -1),   S( 39, -26),   S( 22,  -9),   S(-12,  14),   S(-23,  28),   S(-13,   4),   S(-22,  20),   S(-24,  31),
            S( -9, -41),   S( 38, -33),   S( 30, -47),   S(  5, -13),   S(  5, -13),   S( 19, -19),   S( 12,   4),   S(-22,  12),
            S(  6, -19),   S( 44, -43),   S( 14, -15),   S( 30, -10),   S( 21, -19),   S(  2,  -5),   S( 18,   5),   S(-16,   1),
            S( -1, -41),   S(  3, -16),   S(  0,   0),   S(  0,   0),   S( -8,   9),   S(  6,  22),   S(  8,  60),   S( -7, -40),
            S(-11, -99),   S( -1,  12),   S(  0,   0),   S(  0,   0),   S( -2, -49),   S(  7, -12),   S(  0,  27),   S( -5, -29),

            /* kings: bucket 2 */
            S( 29, -71),   S( 16,  -4),   S( 20, -18),   S( 30, -20),   S(-13,  10),   S( 38, -25),   S(  4,  30),   S( 26, -17),
            S(  1,  -1),   S( -4,  29),   S(-11,   6),   S(-16,  15),   S(-26,  18),   S( -6,   1),   S( 17,   6),   S( -1,  10),
            S(-49, -10),   S( -7,  -6),   S( 14, -20),   S(-26, -13),   S( -7,  -2),   S(  1, -20),   S( 32, -17),   S( 15, -10),
            S(  1,  28),   S(-13,  12),   S(  2,   7),   S(-16,  17),   S( 12,   5),   S(-22,   1),   S( 38, -27),   S( 22,  -5),
            S(-27,   2),   S( 21,   2),   S(-21, -16),   S( -6, -10),   S( 34, -33),   S(-35, -22),   S( 69, -42),   S( 37, -39),
            S( -3, -27),   S(  0,  -2),   S( 22, -18),   S( 52, -18),   S( 57, -30),   S( 30,   1),   S( 62, -33),   S( 27, -35),
            S(-19, -17),   S( -2,  23),   S( -2, -13),   S( 26,  20),   S(  0,   0),   S(  0,   0),   S( 30,  44),   S(  0, -35),
            S( -3, -30),   S(-17, -39),   S(  1, -44),   S(  4, -10),   S(  0,   0),   S(  0,   0),   S( -4, -10),   S(-22, -161),

            /* kings: bucket 3 */
            S(  9, -78),   S(  9,  -6),   S( 26, -32),   S( -4, -13),   S(-15, -24),   S( 33, -31),   S(  3,  17),   S(  8, -29),
            S( -1,  14),   S(-32,  46),   S(-18,  -2),   S(-37,  12),   S(-51,  23),   S(  5, -10),   S( -7,  15),   S(-13,  14),
            S( 45, -48),   S( 12, -15),   S(-13, -15),   S(-16, -18),   S( -7,  -2),   S( 30, -36),   S( 44, -25),   S( 55, -26),
            S(-43,  25),   S(-104,  33),  S(-87,  24),   S(-69,  18),   S(-86,  23),   S(-72,   3),   S(-62,   0),   S(-60,  -6),
            S(-33,  12),   S(-18,  -2),   S(-38,  -7),   S(-62,   7),   S(-11, -32),   S( 11, -40),   S(  9, -57),   S(-26, -63),
            S(-28, -25),   S( 18, -20),   S(-11, -12),   S(-21, -21),   S( 23, -31),   S( 93, -74),   S(125, -50),   S( 13, -90),
            S(-17,  24),   S( 18, -14),   S(  0, -32),   S( 15, -24),   S( -8, -23),   S( 59, -46),   S(  0,   0),   S(  0,   0),
            S( -7, -17),   S( 10, -47),   S(  0,   3),   S( -5, -15),   S(  4, -71),   S( 10, -17),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-50, -12),   S(-34,  39),   S(-21,  39),   S(  5,  -2),   S(  5, -18),   S( -2,   1),   S(  0,  22),   S( 46, -35),
            S(-16,  18),   S( 31,  14),   S(  0,   6),   S(-19,   5),   S( 65, -22),   S( -1,  15),   S( 37,   8),   S(  7,  12),
            S( 17,  21),   S(-17,  -1),   S(-24,   9),   S( -3,   1),   S( -1,   1),   S( 27, -33),   S(-14, -11),   S( -3,   2),
            S( -5, -17),   S(  5,  -3),   S( 36,   3),   S(-14,   6),   S( 18,   5),   S(  2,   9),   S( -9,  20),   S( -6,  -2),
            S(  0,   0),   S(  0,   0),   S(  6, -21),   S(-10,   1),   S(-13,   5),   S(-30,  -9),   S(-22,  16),   S( -9, -14),
            S(  0,   0),   S(  0,   0),   S(  4,  22),   S( 17,   3),   S( -1,  12),   S( -2,  -8),   S( -2,   0),   S( 10,   2),
            S( -8, -41),   S(  8,  41),   S(  1, -28),   S( 14,  -1),   S( 14,  23),   S(-19, -12),   S( 12, -12),   S(  5,  -8),
            S(-11,  -8),   S(  0,  10),   S(  4,   3),   S(  0,  11),   S(  0, -18),   S(  4,  14),   S( -1,  -3),   S(  2,  16),

            /* kings: bucket 5 */
            S( 44, -20),   S( 36,  -5),   S(-34,  17),   S(-44,  20),   S(-42,  28),   S(  5,   0),   S( 36,   6),   S( 55, -12),
            S(-26,  15),   S( 35,   3),   S( 21,  -1),   S( 26,  -4),   S( 45, -12),   S( 16,  -4),   S( 34,  10),   S( 61, -16),
            S( 21,  -9),   S(-32,  11),   S( 10, -17),   S(-18,  -9),   S(-33,   6),   S(-64,   6),   S(-12,   4),   S( -3,   4),
            S(-12,  -1),   S( 29, -19),   S( 29,   2),   S(-20,  23),   S( 28,  13),   S(  4,   4),   S( 22,   9),   S( -1,  -4),
            S( -6, -21),   S(-11, -22),   S(  0,   0),   S(  0,   0),   S(  0,  -7),   S(-16, -12),   S( -3,   4),   S(-17,   7),
            S(-40, -14),   S(-17,  12),   S(  0,   0),   S(  0,   0),   S(  2,  12),   S(-26,  10),   S(-18,  10),   S( -3,  -7),
            S(-21,  -1),   S( -5,   8),   S(  1,  40),   S( -1, -22),   S(-13,  19),   S( -4,  15),   S( -4,  30),   S( 14, -14),
            S(-11, -37),   S(  5,  62),   S(  2,  47),   S(  4,  39),   S( -7,   9),   S( -1,  31),   S( -9,  -9),   S(  4,  29),

            /* kings: bucket 6 */
            S( 38, -30),   S( 38,  -7),   S( 11,  -5),   S( -6,   9),   S( -7,  10),   S(-11,  12),   S( 28,  15),   S( 33,  13),
            S( 40, -24),   S(  7,  27),   S( 39, -13),   S( 37, -13),   S( 31,  -3),   S( -2,   4),   S( 36,  -2),   S( 26,  -4),
            S( -4,  -8),   S(-36,   9),   S(  6, -15),   S(-10, -10),   S(-21,  -3),   S(-43,  -3),   S( -2,   0),   S(-37,  13),
            S(-11,  16),   S(  5,   2),   S( 17,  -3),   S( 57,  -2),   S( 81,   2),   S(-17,   2),   S( 56, -13),   S(  6,   5),
            S( -5, -10),   S(-45,  10),   S(-12, -14),   S( -1,   1),   S(  0,   0),   S(  0,   0),   S(-12, -15),   S(-58, -11),
            S(-13,  -6),   S( -8,   6),   S(-15,   1),   S( -4,  -4),   S(  0,   0),   S(  0,   0),   S(-49,  23),   S(-48,  -5),
            S( -3, -26),   S( 10,  22),   S( -5,   5),   S( -1,  25),   S(  7,   4),   S( -2,   4),   S( -3, -18),   S(-37,   7),
            S(  5,  24),   S( -6,   2),   S(  0,  -9),   S( -7,   1),   S( -1,  24),   S(  0,   7),   S(  4,  37),   S( -4,  36),

            /* kings: bucket 7 */
            S( 37, -32),   S( -3,   4),   S( -6, -17),   S(-33,  10),   S(-32,   6),   S(-49,  36),   S(-16,  39),   S( -5,  12),
            S( 26,   1),   S( 39, -30),   S(  2,  -4),   S(  7,  -8),   S(  2,   3),   S(-28,  18),   S(  6,   6),   S(  4,  12),
            S(  9,  -4),   S(-36,   2),   S(-12, -11),   S(-27,  -4),   S(-37,  -2),   S(-55,  12),   S(-10,   8),   S(-68,  24),
            S( 13,   3),   S( 46, -10),   S(  6,   3),   S( 29,  -1),   S( 20,   1),   S( 52, -17),   S( 49, -18),   S( 34, -17),
            S(-13, -10),   S(-24,  12),   S(-26,  -9),   S(-28,   9),   S(  1, -13),   S( -8, -23),   S(  0,   0),   S(  0,   0),
            S( -3, -28),   S(  5,  -8),   S( 25, -12),   S(  9,  -2),   S( -1, -15),   S( 23,  20),   S(  0,   0),   S(  0,   0),
            S( 27,  27),   S( 21, -19),   S( -8,   6),   S( -1, -22),   S( 21, -14),   S( -4,  -9),   S( 11,  20),   S(-11, -44),
            S(  5,   5),   S(-17, -41),   S( 33,  25),   S(  3, -20),   S( 12,  17),   S( -4, -20),   S(  6,  20),   S(-13, -30),

            /* kings: bucket 8 */
            S(-11, 117),   S(-41,  69),   S(-43,  67),   S(-26,  -3),   S( -1,   6),   S(  4, -18),   S(  9,   3),   S( 27,  -4),
            S( 28,  90),   S( 41, -11),   S(-13,  66),   S(  1,   3),   S(-14,  24),   S( 19,   9),   S(  0,   9),   S( 24,  26),
            S(  0,   0),   S(  0,   0),   S( 32,  49),   S( 15,  13),   S( 37, -17),   S(  3,  -3),   S( 12,  12),   S( -8,  -9),
            S(  0,   0),   S(  0,   0),   S(  6,   9),   S( 21, -27),   S( 19,  28),   S( 13,  -8),   S( 29,   7),   S(  4,  23),
            S(  1, -14),   S(  6,  13),   S(  8, -25),   S( 22,   4),   S(  6, -30),   S( -2,   7),   S(-10,  11),   S(-13, -40),
            S( -4,   9),   S( -3,  -7),   S( -4,   3),   S(-13, -37),   S(-19, -16),   S(-15, -10),   S(-14,   5),   S(  5,  -3),
            S( -2,  -1),   S( -7, -34),   S(  9,  -1),   S(  6,  -2),   S( -3, -41),   S(  2, -10),   S(  5,   3),   S( -3, -40),
            S( -3, -18),   S( -9, -33),   S( -4, -22),   S(-10, -33),   S(  9,  36),   S( -8, -30),   S(  1, -17),   S(  1,   1),

            /* kings: bucket 9 */
            S(-38,  42),   S(-38,  38),   S(-56,  60),   S(-72,  25),   S(-74,  36),   S(-38,  19),   S( 60,   5),   S(  0,  17),
            S(-35,  14),   S( 31,  19),   S( 14,  -8),   S( 35,  14),   S( 36,  16),   S( 29,  -7),   S( 30,   9),   S( 41,   2),
            S(-15,   9),   S( -1,  -2),   S(  0,   0),   S(  0,   0),   S( 14,  18),   S(-20,   2),   S( 26,  -3),   S( 17,   7),
            S( -5, -16),   S(  3,   4),   S(  0,   0),   S(  0,   0),   S(  6,  -2),   S( 33,  -2),   S(-16,  -2),   S( -7,  13),
            S( -2,   9),   S(  1,   3),   S(  4,   7),   S( 10, -12),   S( -8,  -9),   S(-18,  -4),   S(-12,  -1),   S(-15,  11),
            S(  3,  38),   S(-15,   5),   S(  4,  23),   S(-18, -18),   S(  5,   8),   S( -4,   8),   S(-18, -19),   S(  2,  51),
            S(  7,   8),   S( -8, -13),   S( -1,   8),   S( 10,  37),   S(  6,  32),   S( 17,  16),   S( -3,  -5),   S( -5,   1),
            S( 10,  58),   S( -4,  -2),   S( 10,   6),   S( -2, -32),   S( -9, -50),   S(  4,  -6),   S(  7,   0),   S( -3,  -3),

            /* kings: bucket 10 */
            S(-19,  32),   S(-29,   2),   S(-29,  22),   S(-46,  19),   S(-60,  20),   S(-114,  45),  S( -8,  38),   S(-79,  78),
            S( 25,  -2),   S( 37,  12),   S(  2,  -3),   S( 27,  20),   S( 64,  13),   S( 34,   9),   S( 24,  26),   S(-52,  39),
            S(  5,  15),   S( 34,   4),   S( 27, -16),   S( 11,   9),   S(  0,   0),   S(  0,   0),   S(  3,   3),   S(-38,  19),
            S( 18,  -4),   S(-12, -15),   S(-15,  12),   S( 16,  -4),   S(  0,   0),   S(  0,   0),   S(  6,  17),   S( 16,  -6),
            S( 10,   4),   S( 21,   6),   S( 20,  -2),   S(  6, -29),   S(  5, -17),   S(  0,  19),   S(  3,   4),   S(-10,  21),
            S(-11,  19),   S( 10,  19),   S(  4,  15),   S( -6,  14),   S(  4,  17),   S( -9, -19),   S( -7,  11),   S( -5,   3),
            S( -5, -41),   S(  0, -11),   S( 10,  -6),   S( 13,   3),   S(  9,  14),   S(-14, -18),   S(  9, -25),   S( 10,  35),
            S(  3,   3),   S( 17, -12),   S(  1,  -9),   S(  2,  29),   S(  0,  -2),   S(  3,  -8),   S( -4, -15),   S( -4,   9),

            /* kings: bucket 11 */
            S(-36,  14),   S(  8,   5),   S(  0, -19),   S( -7,  21),   S(-25,   7),   S(-171,  82),  S(-22,  55),   S(-148, 157),
            S( 15, -49),   S(  5,  17),   S(  5, -31),   S( 10,   5),   S( 36,   4),   S(-26,  52),   S( 42,  10),   S( 14,  34),
            S(-14, -17),   S( 33, -14),   S(-33, -17),   S( 26,   5),   S( 67,  -8),   S( 33,  22),   S(  0,   0),   S(  0,   0),
            S( -7,   9),   S( 10,  19),   S( 14,   0),   S( 34,  -4),   S( 44, -14),   S( 24,   9),   S(  0,   0),   S(  0,   0),
            S(  7,   4),   S( -5,  -3),   S( -2, -21),   S( 27,  -1),   S( 16, -15),   S(  5,  -6),   S(  0,  -6),   S(  0, -17),
            S( 16,  29),   S( 15,  -3),   S( 17, -14),   S(  2,  -5),   S(  3, -27),   S( -4,   7),   S( -1,  21),   S(  0,   2),
            S( -1, -40),   S(  8,   9),   S(  8,  12),   S( -1, -30),   S(  7,  -9),   S(  3,   2),   S(  4, -32),   S(  0,   3),
            S( 13,  26),   S( -4, -26),   S( -4,  -2),   S(  5,   8),   S( -9, -28),   S( -2,   1),   S(  4, -16),   S( 11,  30),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( -4,  66),   S(-12,  18),   S(  8,  -3),   S(  2,  13),   S( -3, -18),   S( 12,  52),
            S(  0,   0),   S(  0,   0),   S( 25,  83),   S(  0, -21),   S( 19,  38),   S( 11,  11),   S(  5,  -5),   S(-18,  24),
            S(  1,   3),   S(  8,  -5),   S( 17,  15),   S( 19,  24),   S( 10, -12),   S( -5,  12),   S( -5,   9),   S(-25,   2),
            S( -5,  -7),   S(  6,   8),   S(-13, -52),   S(  9, -20),   S(-12, -15),   S(  0,  -8),   S(-13,   0),   S(  7,  17),
            S(  9,  28),   S(  2,  17),   S(  8,  16),   S(  1, -20),   S(  0,  34),   S( -6,  14),   S( -8,  -6),   S( -8, -12),
            S( 14,  45),   S( -4,  -4),   S( -8, -29),   S( -7, -20),   S( -5,   5),   S( -4,   7),   S( -8,  33),   S(  7,  18),
            S(  8,  29),   S( -6, -16),   S(  0,   3),   S(-10, -24),   S( -3,  -4),   S(  8,  -5),   S( 15,  33),   S( -5, -23),
            S( -4, -16),   S(  2,   3),   S( -2, -28),   S( -2, -16),   S( -6, -10),   S(  6,   3),   S(-10, -53),   S( -4, -19),

            /* kings: bucket 13 */
            S( -4,  85),   S( -3,  71),   S(  0,   0),   S(  0,   0),   S( -2,  79),   S(-35,  14),   S( 17,   6),   S(-23,  39),
            S( -5,  15),   S( -9, -20),   S(  0,   0),   S(  0,   0),   S(  8, -20),   S(  0, -25),   S(-18,  15),   S( 13,  12),
            S( -3,   2),   S(  0,  33),   S( -2, -34),   S( -1,   7),   S(  8,  15),   S( -5,  20),   S(-24,   4),   S(  0,   7),
            S(-17, -47),   S(  2,  27),   S( -3, -30),   S(  0, -46),   S(  3, -44),   S(  8, -16),   S( -6,   9),   S(  3,  -9),
            S(  1,  12),   S( -3,   2),   S( 14,  46),   S(-14, -31),   S(-12, -12),   S(  3,  -6),   S( -4,  14),   S( 10,  32),
            S(  5,  15),   S( -4,  28),   S(-14, -28),   S( -3,  -7),   S(-10, -11),   S( -7,  11),   S(-12, -27),   S( -3,  -9),
            S( 11,  27),   S( -3,   0),   S(-10, -26),   S(  2,   6),   S( -7, -20),   S( -3,  -3),   S( -4, -45),   S( 11,  26),
            S(  4,  12),   S(  2,  26),   S(  2,  20),   S(  6,   1),   S(  2,   0),   S( -8, -10),   S(  1,  -5),   S(  9,  25),

            /* kings: bucket 14 */
            S(  6,  59),   S(-13, -10),   S(-18, -32),   S( -3,  28),   S(  0,   0),   S(  0,   0),   S( 17,  86),   S(-68,  63),
            S(-16,  11),   S(-15, -16),   S(  0,   7),   S( 19,  24),   S(  0,   0),   S(  0,   0),   S( 23,   4),   S(-35,   3),
            S( -7,  12),   S(  2,  -5),   S( 20,  -2),   S( -4, -18),   S( 11, -12),   S( -3,   5),   S( 11,  51),   S(-27,  -9),
            S(  7,  -9),   S( -2,  -6),   S(  6, -18),   S(  4, -37),   S(-20, -58),   S(  9,  26),   S( -7,   0),   S( -2, -16),
            S(  3,  17),   S(  5,   3),   S( -5,  -5),   S(-15, -35),   S( -5,  21),   S( 13,  17),   S(  9,  48),   S(  4,  25),
            S(  0,   9),   S( -8,  17),   S(  5,  18),   S( -3,  30),   S( -4,   1),   S( -2, -26),   S(-13, -39),   S( -1,  -2),
            S( -1,  -9),   S( -4, -19),   S(  9,  27),   S(  7,  18),   S( -5,  -8),   S(  2, -29),   S(-18, -77),   S( 11,  74),
            S(  2,  -2),   S( -2,  46),   S( -1, -12),   S(  1,  -2),   S( 10,  62),   S( -7, -27),   S( -8, -27),   S(  3, -19),

            /* kings: bucket 15 */
            S( 19,  54),   S( 10,   4),   S(-16, -38),   S( -3,  -4),   S(-32,  11),   S(-29,  88),   S(  0,   0),   S(  0,   0),
            S( -2, -40),   S( 12,  12),   S( -4, -18),   S( 27,  34),   S( 21, -26),   S( 34,  95),   S(  0,   0),   S(  0,   0),
            S(-25, -25),   S(  8, -10),   S( -3,   0),   S( -6,  -7),   S( 17, -20),   S( 32,  43),   S( 16,  15),   S(-15, -32),
            S(  3,  30),   S( -4,  12),   S( 11,  -4),   S( -7, -28),   S( -7, -66),   S( -2, -16),   S(  3,  54),   S(  5,  -5),
            S( 14,  35),   S(-13,   0),   S( -5,  19),   S( -5, -30),   S(  9, -13),   S(-10,   3),   S( -7,   7),   S( -5,  10),
            S(  5,  23),   S(-10,  -5),   S(  5,  12),   S(  8,   8),   S( -5,  11),   S( -5,  -3),   S( -6,  -2),   S(  0,   1),
            S( 13,  46),   S( -4,  19),   S( -8,  -1),   S( -6,   0),   S(  1,  -5),   S(  1,   1),   S( -8, -10),   S(  1,   1),
            S( -9, -21),   S(  3,  18),   S( -2,   7),   S( -1,  -6),   S(  7,  37),   S(  3,  20),   S(  2,  -1),   S(  5,   9),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S( -7, -176),  S( 16, -25),   S( 28,  39),   S( 36,  76),   S( 43,  96),   S( 47, 115),   S( 53, 113),   S( 64, 101),
            S( 89,  67),

            /* bishop mobility */
            S(  8, -65),   S( 13,   2),   S( 24,  40),   S( 30,  73),   S( 34,  96),   S( 37, 113),   S( 41, 118),   S( 45, 118),
            S( 49, 117),   S( 60, 107),   S( 65,  98),   S( 95,  85),   S(118,  74),   S(127,  54),

            /* rook mobility */
            S(-12, -14),   S( -4,  48),   S( -4,  80),   S( -1,  93),   S( -2, 116),   S(  2, 124),   S(  5, 134),   S(  7, 135),
            S(  7, 144),   S( 10, 149),   S( 17, 151),   S( 26, 149),   S( 36, 145),   S( 50, 141),   S(137,  99),

            /* queen mobility */
            S( 73, -47),   S( 70, -104),  S( 63,  65),   S( 59, 101),   S( 61, 153),   S( 59, 195),   S( 59, 235),   S( 59, 258),
            S( 61, 275),   S( 60, 296),   S( 59, 313),   S( 56, 333),   S( 57, 339),   S( 55, 351),   S( 57, 356),   S( 53, 366),
            S( 62, 356),   S( 64, 354),   S( 80, 340),   S(117, 309),   S(129, 294),   S(173, 257),   S(186, 246),   S(237, 207),
            S(187, 235),   S(193, 212),   S(117, 197),   S( 83, 177),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  31),   S(-26,  62),   S(-34,  54),   S(-26,  76),   S(  3,  32),   S(-22,  43),   S( -4,  76),   S( 29,  36),
            S( 18,  39),   S( -5,  65),   S(-19,  59),   S(-11,  47),   S( -4,  49),   S(-33,  60),   S(-48,  87),   S( 31,  41),
            S( 25,  69),   S( 22,  73),   S(  5,  60),   S( 30,  42),   S(  0,  54),   S(-32,  70),   S(-35, 105),   S( -2,  76),
            S( 37,  95),   S( 55, 105),   S( 32,  70),   S( 12,  56),   S( 10,  58),   S(  3,  87),   S(-21, 112),   S(-67, 134),
            S( 65, 133),   S(105, 148),   S( 82, 104),   S( 56,  96),   S(-42,  87),   S( 51,  76),   S( -6, 156),   S(-44, 149),
            S(154, 188),   S(131, 223),   S(172, 195),   S(181, 194),   S(193, 200),   S(203, 192),   S(191, 204),   S(200, 207),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 16,   9),   S(  5,  -1),   S( 12,   0),   S( 13,  15),   S( 28,  34),   S(  2, -23),   S(-15,  34),   S( 10, -31),
            S( -8,  33),   S( 30,   4),   S(  1,  49),   S( 18,  36),   S( 41,   0),   S( -3,  36),   S( 20,  -1),   S(  9,   9),
            S(-15,  32),   S( 19,  10),   S( -4,  54),   S( 17,  65),   S( 23,  25),   S( 27,  32),   S( 31,   1),   S( -6,  28),
            S( 18,  37),   S( 18,  41),   S( 41,  84),   S(  8,  89),   S( 77,  66),   S( 61,  50),   S( 25,  54),   S( 25,  32),
            S( 12,  63),   S(105, 107),   S(127, 131),   S(116, 163),   S(153, 160),   S(143, 138),   S(173,  97),   S(109,  45),
            S( 77, 184),   S(124, 273),   S(113, 242),   S(107, 218),   S( 69, 149),   S( 46, 124),   S( 46, 158),   S( 22, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 19,  36),   S( 26,  34),   S( 47,  44),   S( 42,  31),   S( 33,  32),   S( 33,  36),   S(  9,  22),   S( 50,  11),
            S( -5,  31),   S( 13,  46),   S( 13,  37),   S( 10,  51),   S( 26,  18),   S(  7,  25),   S( 26,  26),   S( -3,  17),
            S( -1,  10),   S( 23,  34),   S( 51,  46),   S( 43,  33),   S( 41,  48),   S( 64,  13),   S( 16,  29),   S( 19,   4),
            S( 79,  36),   S(113,  23),   S(131,  83),   S(165,  93),   S(167,  78),   S(100,  90),   S( 89,  21),   S( 94, -11),
            S(103,   4),   S(149,  39),   S(152, 169),   S(193, 169),   S(178, 173),   S(115, 163),   S(144, 139),   S(-14, 117),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 19,  23),   S(  1,  17),   S( 15,  28),   S( -7,  48),   S( 64,  31),   S( 16,  15),   S( -1,  -3),   S( 22,   9),
            S(  2,  14),   S(  6,   9),   S( 18,  19),   S( 17,  27),   S( 14,  17),   S( -1,   9),   S(  3,   8),   S( 29,  -6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -14),   S( -6,  -9),   S(-18, -19),   S(-17, -27),   S(-14, -17),   S(  1,  -9),   S( -3,  -8),   S(-29,   6),
            S(-19, -23),   S( -1, -17),   S(-15, -28),   S(  7, -48),   S(-64, -31),   S(-16, -15),   S(  1,   3),   S(-22,  -9),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -24),   S( -8, -16),   S( -4, -31),   S(-64,   3),   S(-16, -19),   S(-17, -25),   S(  8, -29),   S(-18, -38),
            S(-20,   1),   S(-16,   1),   S(-23,   5),   S(  3, -18),   S(-31, -15),   S(-27,  -2),   S(-39,   5),   S(-11, -15),
            S(-18, -14),   S( -8, -16),   S(-26,  -4),   S(-30,  -6),   S(-25, -31),   S(-26, -10),   S(-18,  -2),   S(-46, -17),
            S( -5, -32),   S( 18, -37),   S( 16, -18),   S( 11, -32),   S(  4, -17),   S( 60, -34),   S( 38, -32),   S( -9, -44),
            S( 28, -60),   S( 50, -72),   S( 56, -34),   S( 79, -37),   S( 58, -27),   S( 66, -12),   S(120, -69),   S( 28, -65),
            S(140, -124),  S(165, -127),  S(149, -81),   S(125, -58),   S( 82, -22),   S(143, -36),   S(143, -58),   S( 81, -99),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 16,   3),        // attacks to squares 1 from king
            S( 16,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 70, -85),        // king-side castling available
            S( 16,  56),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S(  6, -48),   S(  9, -42),   S(  0, -48),   S(-12, -35),   S(-20, -28),   S(-37, -18),   S(-72,   2),   S(-83,   6),
            S(-91,   9),   S(-96,  16),   S(-78,  20),   S( 27,  -3),   S(-45,  16),

            /* orthogonal lines */
            S(-29, -50),   S(-56, -26),   S(-78, -16),   S(-94, -15),   S(-99, -21),   S(-104, -24),  S(-106, -29),  S(-102, -32),
            S(-120, -25),  S(-142, -23),  S(-168, -28),  S(-165, -38),  S(-179, -28),  S(-376,  37),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 11, 209),

            /* passed pawn can advance */
            S(-13,  37),   S( -7,  67),   S(  2, 107),   S( 21, 209),

            /* blocked passed pawn */
            S(  0,   0),   S( 48, -29),   S( 21,  -6),   S( 42,  21),   S( 38,  43),   S( 46,   6),   S(177, -22),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 44, -21),   S( 32,  36),   S( 14,  45),   S( 10,  79),   S( 39,  94),   S(130, 113),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-25, -12),   S( -8, -30),   S(  1, -37),   S(-17, -27),   S(  1, -36),   S(185, -76),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 30, -42),   S( 21, -24),   S( 15, -19),   S( 17, -47),   S( 12, -142),  S(-43, -215),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 16,  40),   S( 26,  48),   S(118,  35),   S( 11,  38),   S(204,  85),   S(179,  62),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 15,  55),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-26,  81),

            /* bad bishop pawn */
            S( -5, -22),

            /* rook on open file */
            S( 44,   0),

            /* rook on half-open file */
            S( 14,  29),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 26,  37),   S( 29,  -8),   S( 36,  22),   S( 28,  -1),   S( 44, -21),

            /* pawn threats */
            S(  0,   0),   S( 72,  97),   S( 54, 114),   S( 71,  90),   S( 57,  43),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 35,  54),   S( 50,  43),   S( 79,  32),   S( 48,  62),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 21,  54),   S( 30,  47),   S( -7,  38),   S( 69,  53),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  13),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
