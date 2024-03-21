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

        // Solution sample size: 6000012, generated on Wed, 20 Mar 2024 20:10:31 GMT
        // Solution K: 0.003850, error: 0.084108, accuracy: 0.5059
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 71, 219),   S(385, 663),   S(413, 656),   S(540, 1079),  S(1355, 1814), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(104, -127),  S(157, -107),  S( 35, -41),   S(-27,  -6),   S(-39,   1),   S(-29,   2),   S(-58,   6),   S(-36, -28),
            S(123, -131),  S(112, -116),  S(  8, -66),   S(-13, -55),   S(-16, -16),   S(-14, -38),   S(-37, -32),   S(-23, -54),
            S(108, -94),   S( 71, -67),   S( 21, -69),   S( 21, -74),   S( -1, -62),   S( 11, -54),   S( -5, -58),   S(  9, -62),
            S( 78, -27),   S( 73, -68),   S( 38, -58),   S( 29, -71),   S( -1, -61),   S(-13, -51),   S(-20, -39),   S(  3, -38),
            S( 66,  60),   S( 50,   0),   S( 68, -49),   S( 63, -72),   S(  9, -32),   S( -7, -38),   S(-31,   0),   S(-25,  34),
            S( 72,  35),   S( 72,  57),   S(-28,  -5),   S( 16, -45),   S(-35, -22),   S(-28,  26),   S(  3,  36),   S( 48,   6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 36, -36),   S( 35, -43),   S( 50, -27),   S(  6,  25),   S(-17, -11),   S(  8, -24),   S(-42,  -3),   S(-35,  17),
            S( 40, -54),   S( 24, -51),   S( 17, -55),   S( 11, -56),   S( -6, -30),   S(  3, -39),   S(-32, -18),   S(-28, -22),
            S( 31, -40),   S( 13, -35),   S( 28, -58),   S( 29, -65),   S(-16, -32),   S( 28, -53),   S(  1, -36),   S(  9, -28),
            S( 47, -10),   S( 36, -52),   S( 36, -55),   S( 18, -51),   S(  1, -27),   S( 13, -41),   S(-19, -24),   S(  3,  -2),
            S( 40,  40),   S(-35,  -9),   S( 19, -47),   S( 23, -65),   S( 40, -46),   S(  1,   3),   S(-23,  14),   S(-15,  65),
            S( 44,  72),   S( 18,  -9),   S(-38, -23),   S(-31,  20),   S(-24,  -6),   S(-64,  34),   S(-51,  31),   S(-47,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-19,   1),   S(-22,  -1),   S(-12,  -4),   S( -5,   4),   S(  7, -15),   S( 35, -26),   S(  5, -51),   S( -7, -25),
            S( -5, -30),   S(-27, -22),   S(-19, -39),   S(-19, -38),   S( 14, -40),   S( 14, -38),   S(  2, -49),   S(-10, -39),
            S( -9, -21),   S(-19, -31),   S( -2, -55),   S(  3, -63),   S(  0, -30),   S( 30, -42),   S(  9, -41),   S( 19, -34),
            S( -8,  -5),   S( -3, -45),   S( -9, -48),   S(  1, -55),   S( 22, -51),   S(  1, -24),   S(  2, -16),   S( 13,  -8),
            S( -3,  32),   S(-30, -17),   S(-45, -34),   S(-42, -42),   S( 19, -14),   S( 13,  -9),   S(-18,  19),   S( -9,  66),
            S(-63,  87),   S(-84,  56),   S(-89, -18),   S(-80, -15),   S(-39,  19),   S(-13,  29),   S( 11,  -7),   S(-21,  87),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-14, -20),   S(-28,  -7),   S(-26,  -8),   S(  7, -48),   S( -4, -11),   S( 45, -27),   S( 90, -78),   S( 68, -91),
            S( -9, -48),   S(-27, -40),   S(-22, -51),   S(-14, -37),   S( -4, -34),   S( 24, -53),   S( 74, -86),   S( 69, -89),
            S( -4, -48),   S( -4, -63),   S(  6, -72),   S(  9, -79),   S( -2, -54),   S( 39, -60),   S( 51, -68),   S( 87, -74),
            S(  2, -31),   S(  7, -74),   S( 10, -83),   S( 14, -79),   S( 35, -80),   S( 25, -64),   S( 38, -53),   S( 81, -33),
            S( 23,   6),   S( -1, -39),   S( 20, -84),   S( 16, -74),   S( 95, -76),   S( 78, -49),   S( 64,  -1),   S( 64,  55),
            S(-30, 104),   S( -5,  21),   S(-22, -45),   S( 25, -90),   S( 68, -82),   S( 82, -15),   S( 76,  13),   S( 74,  49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-109,  29),  S(-21,  -5),   S(-30,   7),   S(  9,  28),   S( -8, -36),   S(-83,  32),   S(-50,  -4),   S(-46,  -4),
            S(-51,  18),   S( 44, -18),   S( 25, -42),   S(  6, -20),   S(  1, -20),   S(-27, -31),   S( 21, -59),   S( -3, -32),
            S( 34, -14),   S( 42, -11),   S( -7,   5),   S(  1, -31),   S(-42, -26),   S(-19, -39),   S(-15, -42),   S(  3, -25),
            S(-20,  50),   S(  5,  32),   S( 15,  20),   S(-10,  12),   S(  6, -32),   S(-33, -28),   S(-11, -35),   S( 58, -28),
            S(-32,  95),   S(-12,  83),   S(-23,  33),   S(  0,   5),   S( -5,  27),   S(-17, -14),   S(-31, -37),   S( 84,  17),
            S( 85,  67),   S( 44, 112),   S(  9,  33),   S(  7,  14),   S(  5, -28),   S( 12,  -8),   S( 14,  23),   S(-18,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-101,  54),  S(-99,  47),   S(-17,  11),   S( -4,   6),   S( -1,   8),   S(-48,  28),   S(-60,   8),   S(-27,  21),
            S(-54,  15),   S(-66,  12),   S( 20, -17),   S(  6,   2),   S( -7,  -8),   S(-21, -22),   S(-23, -18),   S(-36,   2),
            S(-46,  36),   S(-56,  25),   S( 54, -32),   S( -7, -26),   S( 16, -10),   S(-30, -12),   S(-19,  -8),   S( 29, -15),
            S(-49,  56),   S(-66,  43),   S( -3,   9),   S( 19,  20),   S(-25,   7),   S(-52,   1),   S(  8, -10),   S( 24,   8),
            S( 57,  52),   S(  7,  46),   S(  6,  49),   S( 16,  28),   S( -7,  36),   S( 75, -22),   S( 18,  -7),   S( 22,  18),
            S( 54,  54),   S( 47,  29),   S( 47,  -7),   S( 27, -23),   S( 55,  -6),   S( 36,  -5),   S(  3,   8),   S(  8,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33,  20),   S(-53,  21),   S(-36,  14),   S(-45,  21),   S(  8, -20),   S(-45,   9),   S(-70,   4),   S(-72,  17),
            S(-32,  -8),   S( -1, -35),   S(-28, -33),   S( -4, -12),   S( 38, -23),   S( 26, -29),   S(-35, -13),   S(-76,   5),
            S( -9,  -5),   S(-14, -14),   S(-11, -26),   S(-32, -11),   S( 13, -14),   S( 76, -42),   S(  2, -21),   S(-25,   8),
            S(-13,  15),   S(-74,  11),   S( -9, -19),   S(-20,   2),   S( 23,  -1),   S( 42, -16),   S( 23,  -7),   S( 44,   1),
            S( 12,  18),   S(-73,  12),   S(  2, -26),   S( 14, -23),   S( 63,  32),   S( 55,  28),   S( 53,  -5),   S( 33,  38),
            S( 62,  30),   S( 17,  15),   S(  8, -43),   S( 16, -56),   S( 41, -10),   S( 25,   2),   S( 25,  -3),   S( 30,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52, -22),   S(-51, -21),   S(-20,  -7),   S(-59,  10),   S(-28, -10),   S( 21, -26),   S(-25, -46),   S(-49, -29),
            S(-30, -49),   S(-38, -51),   S(-41, -46),   S(-19, -49),   S( -1, -45),   S( 53, -61),   S( 57, -65),   S( -3, -45),
            S(-23, -50),   S(-51, -43),   S(-22, -60),   S(-13, -53),   S(-14, -27),   S( 44, -43),   S( 50, -57),   S( 47, -43),
            S(  0, -50),   S(-41, -54),   S(-72, -46),   S(-34, -30),   S( 12, -36),   S( 20, -17),   S( 19,  -9),   S( 72, -22),
            S(  9, -41),   S( -6, -68),   S(-61, -38),   S( 10, -72),   S( 22,  -1),   S( 30,   2),   S( 41,  59),   S( 87,  34),
            S( 22,  -9),   S(-21, -35),   S( 56, -66),   S( -6, -50),   S(-11, -18),   S( 26,   1),   S( 57,   9),   S( 89,  74),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33,  66),   S(-32,  45),   S( 17,  16),   S(-13,  -2),   S( 19,   4),   S( 46, -11),   S(-53,   2),   S(-28,  12),
            S(-77,  59),   S(-57,  55),   S(-49,  25),   S( -5,  19),   S(-51, -15),   S(-52, -20),   S(-59, -14),   S( -3, -13),
            S(-68, 121),   S( -1,  98),   S(  4,  50),   S(-30,  42),   S( 11, -21),   S(-77, -16),   S(-92, -28),   S(-42, -14),
            S(  5, 153),   S( 24, 156),   S( -4, 106),   S( 26,  64),   S(-18,   9),   S(-19, -33),   S(-20,  -7),   S(-59,  12),
            S(-11, 183),   S( 25, 190),   S( 12, 186),   S( 58,  96),   S( 10,  -7),   S( -4,  17),   S(-15, -35),   S(-13,   8),
            S( 41, 211),   S( 59, 227),   S( 72, 221),   S( 50,  78),   S( -1,  10),   S(-16,  14),   S(-15, -51),   S(  6, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-115,  82),  S(-91,  60),   S( -4,  19),   S( 15,  29),   S(  4,   0),   S(-70,  22),   S(-64,   7),   S(-82,  31),
            S(-73,  42),   S(-77,  31),   S(-66,  38),   S( -5,  46),   S(-53,  12),   S(-31, -16),   S(-118,   8),  S(-12,  -2),
            S(-82,  74),   S(-103, 101),  S(-48,  91),   S(-116,  95),  S(-79,  66),   S(-81,  11),   S(-36, -19),   S(-45,  11),
            S(-65, 125),   S(-51, 123),   S(-18, 142),   S( 28, 164),   S(-26,  65),   S(-23,  15),   S( 10,  16),   S(-46,  23),
            S( 39, 117),   S( 30, 148),   S( 23, 167),   S( 41, 171),   S( 50, 130),   S(-17,  38),   S(-15,   6),   S( 10,   2),
            S(  4,  86),   S(  9, 133),   S( 75, 155),   S( 75, 221),   S( 26, 121),   S( -1, -25),   S( -9,  -4),   S(-32, -15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-101,  22),  S(-75,  -4),   S(-25,   6),   S(  0,   9),   S(-15,  18),   S(-68,  28),   S(-108,  30),  S(-52,  19),
            S(-120,  16),  S(-85,   0),   S( 18, -22),   S(-48, -20),   S(-37,  34),   S(-73,  38),   S(-132,  30),  S(-97,  21),
            S(-19,  -6),   S(-86,   2),   S(-26, -10),   S(-85,  57),   S(-75,  93),   S(  4,  47),   S(-103,  51),  S(-80,  52),
            S(-102,  25),  S(-71,  37),   S( -1,  26),   S(-63,  98),   S( 57,  97),   S(-56,  91),   S(-40,  65),   S(-12,  34),
            S(-24,  47),   S(-10,   8),   S( -7,  59),   S( 25, 127),   S( 85, 119),   S( 19,  71),   S( 14,  83),   S(  9,  54),
            S( 17,   8),   S(-17, -16),   S( 25,  22),   S( 43, 146),   S(  3, 156),   S( 28,  84),   S( -6,  74),   S(  9, 100),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  -8),   S(-99,  31),   S( 48, -49),   S( -1,  21),   S( -7,  48),   S(-98,  53),   S(-59,  34),   S(-67,  53),
            S(-63, -20),   S(-90, -18),   S(-67, -27),   S(-35,  16),   S(-44,  12),   S( 12,  11),   S(-105,  67),  S(-130,  60),
            S( -9, -36),   S(-39, -38),   S(-60,  19),   S(-55,  12),   S(-54,  38),   S(-20,  61),   S(-75,  97),   S(-56,  77),
            S(-49,  16),   S(-96, -17),   S(  3, -23),   S(-29,  17),   S( 20,  43),   S(-15,  86),   S( 12, 122),   S( 37,  85),
            S(-35,   9),   S(-44,  -9),   S(  2,  16),   S( -7,  11),   S( 47,  88),   S( 23, 117),   S( 61, 152),   S( 78, 124),
            S(-37,  15),   S(-11,  17),   S( 16,  17),   S(  1, -19),   S( 16,  60),   S( 19, 131),   S( 59, 210),   S( 51, 206),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20,  23),   S(-16,  -4),   S(-25,  11),   S(  3,  13),   S(  6,  18),   S( -5,   2),   S(-13, -27),   S( -7, -37),
            S(-39, -18),   S( 18,  16),   S(-11,  10),   S( 14,  -5),   S(-21,  25),   S(-21, -25),   S(-29, -30),   S(-44, -50),
            S(-10,  47),   S(-38, 109),   S( 31,  68),   S(  9,  48),   S( -2,  17),   S(-42,   9),   S(-62, -57),   S(-44, -76),
            S(-46, 100),   S(-37, 128),   S( 22, 125),   S( 39, 119),   S(-40, -47),   S(-32, -30),   S(-25, -14),   S(-70, -40),
            S( -7, 103),   S( 48, 227),   S( 54, 158),   S( 11,  50),   S(  4,  25),   S(  1, -26),   S( -5, -38),   S(-39, -67),
            S( 47, 148),   S( 63, 239),   S(120, 251),   S( 40, 100),   S(  1,  22),   S(-10, -14),   S(-13, -53),   S(-15, -38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58, -21),   S( -9,  -6),   S( -8,  34),   S(  1,   8),   S(-17, -10),   S(-49, -27),   S(-46, -56),   S(-53, -17),
            S(-72, -14),   S(-88,  42),   S(-31,  45),   S( 15,  31),   S(-52,  -2),   S(-41,  -9),   S(-96, -15),   S(-69,  -1),
            S(-36,  61),   S(-48,  60),   S(-64,  99),   S(-19,  90),   S( -9,   0),   S(-44, -41),   S(-64, -48),   S(-82, -36),
            S(-81,  97),   S(  0, 123),   S(-30, 150),   S(-10, 115),   S(  1,  55),   S(-19,  23),   S( -9,   4),   S(-59, -43),
            S(-14, 117),   S( 53, 163),   S( 84, 225),   S( 69, 261),   S( 37, 177),   S(-25,   4),   S(  4, -53),   S(-21, -43),
            S( 39,  79),   S( 72, 163),   S( 84, 201),   S( 95, 263),   S( 47, 145),   S( -5,  11),   S(  2,   6),   S(-15,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -54),   S(-27,  -7),   S(-10, -15),   S( -4,  -7),   S( -7, -14),   S(-31,  -6),   S(-47, -38),   S( -5,  31),
            S(-40,   9),   S(-68,  -4),   S(-37, -62),   S(  4,  36),   S(-44,  75),   S(-23,  29),   S(-38,  24),   S(-40,  26),
            S(-71,  -9),   S(-61,  -5),   S(-34,   4),   S( -9,  48),   S(-27,  62),   S(-61,  47),   S(-52,  14),   S(-64,  47),
            S(-51,  -6),   S(-21,  43),   S( -8,  44),   S(  0, 130),   S(  1, 165),   S(-39,  93),   S(-30,  40),   S(-40,  82),
            S(-14, -27),   S(  5,  26),   S( 14,  81),   S( 27, 126),   S( 46, 238),   S( 55, 179),   S(  6,  92),   S( 42,  49),
            S(-29,   7),   S( 22,  24),   S( 27, 125),   S( 43, 170),   S( 71, 227),   S( 51, 107),   S( 11, 123),   S( 12,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -43),   S(-29, -36),   S( -4, -31),   S(  0,   6),   S( 19,  19),   S( -4,  73),   S(-16, -40),   S( 15,  27),
            S(-56, -17),   S(-40, -24),   S(-26, -57),   S( 37,   8),   S(-29,  -7),   S(-10,  24),   S(  0,  22),   S(-21,  -2),
            S(-27, -62),   S(-48, -61),   S(-14, -31),   S( 14,   1),   S( 16,  42),   S(-14,  78),   S( -5,  82),   S(-12,  80),
            S(-45, -30),   S(-49, -13),   S(-47,  -7),   S(  7,  45),   S(-16,  57),   S( 15, 103),   S(-34, 159),   S(-12,  87),
            S(-41, -52),   S(-34, -16),   S(-25, -16),   S(  3,  23),   S( 42, 126),   S( 59, 182),   S( 62, 258),   S( 68,  82),
            S( -5,  28),   S(-14, -17),   S( -2,  14),   S(  5,  22),   S( 19,  36),   S( 72, 179),   S( 22, 182),   S( 36,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-22,  12),   S(-22,  16),   S(-49,  35),   S(-36,   0),   S(-45, -39),   S(-17, -37),   S(-49, -64),   S(-46,  -4),
            S(-46,  61),   S( 70, -68),   S(-45,  16),   S(  0, -10),   S( -5, -19),   S(-14, -21),   S(-67,  10),   S(-69, -23),
            S( -3,  56),   S(  4, -41),   S(  9, -16),   S(-25,  56),   S( 15,  27),   S(-28,  15),   S( -8, -22),   S(-49, -37),
            S( 20, -36),   S( 39,   9),   S( 12,  29),   S( 32,  34),   S( 24,  -9),   S(  3,   4),   S( -2, -25),   S(-17,  -3),
            S(  4, -46),   S( 50,  -2),   S( 13,  18),   S( 65,  -5),   S( 57, -30),   S( 52,  -5),   S( 15,  -1),   S(-57, -44),
            S(  3, -26),   S( -2,   5),   S( 49, -12),   S( 51,  -7),   S( 14, -35),   S( 42,  11),   S(-24, -27),   S(-28, -25),
            S( 19, -15),   S( 28, -50),   S( 44, -62),   S( 24, -34),   S( 18, -14),   S(-20, -56),   S(  6, -51),   S(-62, -23),
            S(-86, -43),   S(-21,  -9),   S(-12, -22),   S(  1, -50),   S(-58, -44),   S( 23,  38),   S( -4,   7),   S( -1,  -3),

            /* knights: bucket 1 */
            S(-55,  44),   S(-50,  88),   S( -4,  40),   S(-12,  54),   S(-30,  38),   S(-36,  23),   S(-37,  58),   S( -6, -15),
            S( 28,  11),   S( -1,  39),   S( -2,  27),   S(-11,  41),   S( -5,  29),   S( -1,   9),   S(  8, -35),   S(-47,  30),
            S(-37,  37),   S( 11,   9),   S( 11,   7),   S( 12,  28),   S( 12,  38),   S(-24,  27),   S(-16,   6),   S(-29,  23),
            S(  7,  27),   S( 61,  25),   S( 23,  52),   S( 18,  37),   S( 12,  41),   S( -3,  36),   S( 45,  -2),   S( -9,  36),
            S(-23,  55),   S( 23,  25),   S( 42,  26),   S( 65,  10),   S( 25,  39),   S( 42,   6),   S( 20,  17),   S( 18,  13),
            S(  2,  33),   S( 51, -18),   S( 35,  32),   S( 24,  23),   S( 27,  11),   S( 21,  51),   S( 40, -13),   S( 10, -32),
            S( 15,   8),   S( 44,  -4),   S(-13,  -5),   S(  0,  39),   S( 19,  -9),   S( 57, -22),   S(-17, -21),   S(-12,  -4),
            S(-108, -68),  S(-11, -21),   S( -2,  -8),   S(  1,   4),   S(-25, -12),   S(-17, -44),   S( -4,  -4),   S(-19, -42),

            /* knights: bucket 2 */
            S(-72,  24),   S( -2,   8),   S(-43,  48),   S(-31,  55),   S(-26,  53),   S(-28,  64),   S(-22,  29),   S(-35,  -2),
            S(-32, -23),   S(-31,  26),   S( -3,  20),   S(-10,  31),   S(  1,  25),   S( -8,  40),   S(-51,  59),   S(-44,  73),
            S(-26,  29),   S(  0,  22),   S( -8,  34),   S( 29,  18),   S(  3,  40),   S(  3,  19),   S( -5,  41),   S(-21,  27),
            S(-12,  43),   S(-22,  49),   S( 10,  48),   S( 10,  53),   S(  4,  51),   S( -5,  43),   S( 11,  36),   S( -2,  28),
            S(  1,  20),   S(-12,  40),   S(-12,  58),   S(-11,  57),   S(  3,  48),   S( -4,  46),   S(  0,  44),   S(-17,  39),
            S(-23,  38),   S( -2,  27),   S(-46,  72),   S(-34,  53),   S( -6,  41),   S(-10,  49),   S(-28,   9),   S( 10,  10),
            S( 10,  10),   S(-55,  24),   S(-26,  13),   S(-45,  37),   S( 12,   3),   S(-20,  35),   S(-62,  37),   S(-47,   8),
            S(-172,  14),  S( -6,  26),   S(-80,  26),   S( -9, -17),   S(  5,  14),   S(-38,  16),   S( 19,  -9),   S(-182, -80),

            /* knights: bucket 3 */
            S(-48, -19),   S( 10, -27),   S(-43,   6),   S(-11,   8),   S(  0,   3),   S( -2,   7),   S( 14,  -3),   S(-17, -13),
            S(-13,  11),   S(-27,   0),   S( -8,  -2),   S( 16,   9),   S( 25,   3),   S(-14,   6),   S(  2, -13),   S(-14,  47),
            S(  2, -31),   S( 10,   4),   S(  7,   4),   S( 36,   7),   S( 32,  26),   S( 27,   4),   S(  8,   8),   S( 14,  28),
            S(  0,   4),   S( 20,  19),   S( 26,  36),   S( 31,  46),   S( 37,  38),   S( 34,  37),   S( 37,  23),   S( 24,  21),
            S( 25,   9),   S( 14,  26),   S( 34,  20),   S( 40,  48),   S( 26,  57),   S( 38,  43),   S( 44,  49),   S( 14,  25),
            S( 28,  -3),   S( 52, -32),   S( 51,   9),   S( 65,   6),   S( 62,  -8),   S( 75,   2),   S(  5,   9),   S(  4,  45),
            S( -6,  -3),   S(  7,   2),   S( 53, -32),   S( 42,  -6),   S( 63, -34),   S( 63, -36),   S( 36, -70),   S( 52, -44),
            S(-134,  23),  S(-36,  -3),   S(-15, -15),   S(-16,  -4),   S( 63,  -7),   S(-31, -39),   S(-25, -29),   S(-76, -66),

            /* knights: bucket 4 */
            S( 13,  32),   S(-58,  18),   S(  2,  28),   S(-11, -25),   S(-13, -29),   S(-33, -49),   S(  0, -59),   S(-44, -58),
            S( 36,  42),   S(-21,  37),   S( 16, -33),   S( 41, -14),   S( 47, -12),   S( -1, -36),   S( 31,  -7),   S(-17, -31),
            S( -5,  33),   S( 48,  18),   S( -6,   6),   S( 30,  20),   S( 20,  -3),   S(-66,  30),   S(-39, -24),   S( -7, -83),
            S( -3,  61),   S( 37, -22),   S( 68,  11),   S( 18,  44),   S( 23,   6),   S(109, -22),   S(-12, -54),   S(  1, -18),
            S( 80,  30),   S(  0,  50),   S( 55,  50),   S( 28,  28),   S( 67,  42),   S(-19,  48),   S(-20, -36),   S(  0, -12),
            S(-19,  -7),   S(-22,  12),   S( 91,  33),   S( 29,  16),   S(-12,  19),   S( 26,  48),   S(  9,  43),   S(-18, -56),
            S( -5,  -1),   S( -5,  -9),   S( 20,  12),   S(  5,  50),   S(  1,   8),   S( 24,   4),   S( -3,  -1),   S( -9,   1),
            S(-21, -17),   S( 11,  18),   S( 16,   1),   S(  5,   6),   S(  9,  -3),   S( -4,  -8),   S( -5, -13),   S( -2, -15),

            /* knights: bucket 5 */
            S(  5, -34),   S(-16,  35),   S( 36,  18),   S( 25,  47),   S( 40,  29),   S(-30,   3),   S( -2,  10),   S(-24, -45),
            S(  6,  -2),   S( 39,  40),   S(  9,  24),   S( -9,  45),   S( 19,  42),   S(-11,  43),   S( 22,   1),   S(-21, -37),
            S( 17,  41),   S(-27,  52),   S( 52,  13),   S( 33,  46),   S(-34,  66),   S(-12,  44),   S( 25,   0),   S( -2,  -6),
            S( 48,  53),   S( 23,  36),   S( 43,  38),   S(-27,  64),   S(  5,  56),   S(  5,  59),   S( 34,  60),   S(  5,  47),
            S( 37,  49),   S( 49,  30),   S( 73,  41),   S( 49,  50),   S( 81,  45),   S( 38,  34),   S( 43,  44),   S( 26,  30),
            S(-13,  40),   S(-27,  43),   S(  1,  45),   S( 10,  54),   S( 48,  58),   S(  2,  67),   S( 38,   7),   S(-14,  10),
            S( 27,  63),   S(-18,  51),   S( 43,  54),   S(  9,  58),   S( 14,  43),   S( 25,  78),   S( 24,  50),   S(  1, -17),
            S(  0,  17),   S(-12,  -6),   S(  2,  52),   S(-16,   4),   S(  3,  78),   S(  3,  36),   S(  3,  19),   S(-23, -35),

            /* knights: bucket 6 */
            S( -4, -48),   S(-22, -18),   S( 17,  16),   S(-59,  50),   S(-44,  47),   S(  6,  60),   S( 24,  16),   S( -8,  28),
            S( -5, -58),   S( 62,   5),   S( 25,  11),   S(-21,  38),   S(-46,  64),   S( 20,  50),   S( 49,  53),   S(-13,  39),
            S(-22,  -6),   S( 14,  -7),   S( 29,  14),   S( 13,  32),   S(-16,  61),   S(-32,  61),   S( 22,  47),   S(-16,  61),
            S( 14,   0),   S( 33,  23),   S( 40,  40),   S( 78,  25),   S( 14,  50),   S(  6,  53),   S(  4,  73),   S(-17,  61),
            S(  4,  30),   S( 66,   8),   S( 55,  39),   S( 76,  35),   S( 97,  39),   S( 94,  30),   S( 54,  52),   S( 44,  59),
            S( 36,  25),   S( 34,  19),   S( 45,  34),   S( 49,  45),   S( 71,  57),   S( 40,  39),   S( 16,  40),   S( 70,  53),
            S(-33,  14),   S(-18,  45),   S(-55,  42),   S( 14,  31),   S( -2,  32),   S( 21,  62),   S( 10,  64),   S(-13,  26),
            S(-45, -10),   S( 29,  53),   S( 29,  43),   S( -1,  29),   S( 13,  21),   S(  7,  41),   S( 19,  60),   S(  4,   5),

            /* knights: bucket 7 */
            S(-47, -75),   S(-208, -29),  S(-60, -43),   S(-70,  -9),   S(-65,   7),   S(-51, -25),   S(-25, -14),   S(-23,  -7),
            S(-53, -90),   S(-36, -28),   S(-68, -15),   S(-43,  -2),   S(-60,  21),   S( 45, -26),   S(-22,  57),   S( 35,  10),
            S(-75, -76),   S(-77, -27),   S(-52,   2),   S( 21, -16),   S( 10,  -6),   S( 12,   8),   S(-16,  49),   S( 14,  39),
            S(-80, -13),   S(  4, -20),   S(-19,  19),   S( 39,   1),   S( 69,  -1),   S(  7,  12),   S( 24,  14),   S( -3,  14),
            S(-83, -23),   S(-45,   0),   S( 61, -14),   S( 98, -14),   S(110,   5),   S( 35,  39),   S( 85,   2),   S( 72,  24),
            S( 20, -29),   S( -9, -29),   S(-44,   4),   S( 38,   0),   S( 56,  34),   S( 77,   5),   S(100, -40),   S(  1,   7),
            S(-70, -47),   S(-74, -19),   S( 11, -30),   S( 51,  13),   S( 16,  24),   S( 63,  -1),   S( -2,  43),   S( 41,   5),
            S(-53, -44),   S(  0, -13),   S(-24, -14),   S( 12,  11),   S(  8,  -1),   S( 31,  19),   S( -8, -26),   S( -7,  10),

            /* knights: bucket 8 */
            S( -3,   0),   S(-19, -24),   S( -9, -26),   S(  2,  10),   S( -7, -31),   S(-13, -68),   S( -8, -25),   S( -6, -21),
            S(  7,   9),   S(-14, -26),   S( -3, -16),   S(-13, -32),   S(-33, -38),   S(-11, -70),   S(-15, -68),   S(-21, -53),
            S( -5,   3),   S(-17, -52),   S( 38,  -3),   S(-13, -13),   S( 12, -30),   S( -9, -45),   S(-15, -43),   S(-10, -34),
            S(-12,  -2),   S( -1,   3),   S(  0,  30),   S(-18,  48),   S( 20,   8),   S( 18,  18),   S( -5, -35),   S( -9, -46),
            S( 37,  74),   S(  2, -17),   S( 16,  27),   S( 29,   2),   S(  5,  26),   S( 12,   4),   S( -1, -29),   S( -1,   7),
            S(  9,  29),   S(  9,  26),   S( 22,  17),   S( 46,  20),   S( -2,   4),   S( -6,  11),   S( -4, -26),   S( -1,   7),
            S( -1,  14),   S(  3,  16),   S( -9,  -6),   S(  4,  -1),   S(  3,  10),   S( 15,  52),   S(  5,  18),   S( -4, -10),
            S(  3,   6),   S(  9,  25),   S(  6,   9),   S( -4,  -5),   S(  7,  13),   S( -6, -15),   S(  4,   2),   S( -5, -14),

            /* knights: bucket 9 */
            S(-10, -30),   S(-23, -29),   S(-20, -64),   S( -7, -32),   S(-26, -45),   S(-11, -53),   S(  0,  -8),   S( -1,  -5),
            S(-15, -55),   S(-18,  -3),   S(-22, -80),   S(-15,   7),   S(  1,   0),   S( -2, -30),   S(  7,  10),   S(-14, -45),
            S( -4, -14),   S(-12, -17),   S( -3,   6),   S(  1,  24),   S( -4,  32),   S(-24,  21),   S(-12, -15),   S( -6,  -1),
            S(-10,  -2),   S( -4, -16),   S( 18,  28),   S( 48,  19),   S( 36,  17),   S( 16,  49),   S(-15, -62),   S( -3, -16),
            S( 10,  55),   S(  9,   6),   S(  6,  51),   S(-15,  55),   S(  2,  10),   S(  6, -22),   S( 16, -38),   S(  8,  15),
            S(  0,  -1),   S(  6,  22),   S( 15,  28),   S(-33,  -3),   S( 43,  45),   S( 31,  17),   S( 11,  16),   S(-11, -28),
            S(  3,  18),   S(  2,   6),   S( 14,  41),   S( 15,   6),   S(  1,  38),   S(-12, -31),   S(  3,  15),   S(  2,  10),
            S(  4,   0),   S(  7,   8),   S( 13,   5),   S(  4,  -1),   S(  6,  13),   S(  8,  22),   S(  6,   4),   S( -2, -12),

            /* knights: bucket 10 */
            S(-19, -51),   S(-19, -70),   S(-11, -39),   S(-22, -51),   S(-11,   5),   S(-17, -41),   S( -7,   7),   S(  7,  26),
            S(-13, -56),   S( -6,   2),   S( -2,   1),   S(-19, -26),   S(-28, -47),   S( -3, -13),   S(-23, -34),   S( -4,   2),
            S(-21, -68),   S(-26, -48),   S(  4, -12),   S(-18,  -7),   S( 35,  10),   S( 21, -10),   S(-22, -13),   S( -1,   8),
            S( -8, -21),   S(-17, -65),   S( -4, -32),   S(  7,   8),   S(-18,  53),   S( 15,   9),   S( 10,   6),   S( 12,  52),
            S( -7, -57),   S(-17, -32),   S( 10,  17),   S( 21,  47),   S( 19,  46),   S( 14,  38),   S(  8,   5),   S( 21,  29),
            S( -8, -34),   S( -9, -19),   S(-17, -19),   S( -5,  32),   S( 36,  58),   S( 34,  16),   S( 34,  78),   S( 17,  44),
            S(  4,   4),   S( -2, -15),   S(  8, -13),   S( 20,   6),   S( 25,  45),   S( -2,   8),   S( -3,   9),   S( 14,  29),
            S( -2, -15),   S( -1,   1),   S(-13, -13),   S(  7,  10),   S( 16,  43),   S(  2,  23),   S(  6,  21),   S( -1,  -6),

            /* knights: bucket 11 */
            S(  0,   1),   S(-10,   0),   S(-11, -65),   S(-17, -36),   S(-30, -56),   S(-13, -25),   S( -4,  -6),   S( -5,  -9),
            S(-11, -28),   S(-19, -27),   S(-24, -108),  S(-19, -32),   S(  2,  -5),   S(-23, -49),   S(-13, -32),   S(-10, -27),
            S(-13, -65),   S(-21, -85),   S(-23, -19),   S( -3,  29),   S(-20,   5),   S(-11,  10),   S(  9,  -3),   S( -2,  -1),
            S(-22, -48),   S(-17, -26),   S(-20,   9),   S( 21,  42),   S( 25,  33),   S(-12,  11),   S(-12,   1),   S( 18,  44),
            S(-15, -56),   S(-40, -85),   S( 15,   3),   S(  1,   7),   S( 11,  12),   S( 19,  52),   S( 12, -12),   S( 29,  43),
            S(-13, -42),   S(-12, -36),   S(  2,  13),   S( 40,  29),   S( -2, -13),   S( 50,  58),   S( 12,  17),   S( 12,  16),
            S( 11,  28),   S( -3, -12),   S( -6, -32),   S(  8,  -4),   S( 32,  39),   S(-14, -21),   S(  7,  26),   S( 23,  70),
            S(  0,  18),   S( -7, -41),   S(  7,  -4),   S(  6,  10),   S(  1,  17),   S(  3,  16),   S(  8,  12),   S(  8,  29),

            /* knights: bucket 12 */
            S( -4, -17),   S(  0,   3),   S( -1, -22),   S( -4,  -7),   S( -5,  -6),   S( -3,  -6),   S(  4,  11),   S( -2, -15),
            S(  1,   3),   S(  1,   6),   S(  6,  23),   S( -5, -17),   S( -2,  17),   S( -4, -24),   S(  2,   1),   S(  4,  16),
            S( -3,   0),   S(  2,  -7),   S( -8,  -1),   S( -4, -11),   S( -5, -13),   S( -4, -15),   S(  1,   5),   S(-10, -26),
            S( -8, -12),   S( -5,  -9),   S( -7, -21),   S(  8,   6),   S(  6,   6),   S(  6,  14),   S( -4,  -9),   S( -1,  -2),
            S(  6,   4),   S(  3,   3),   S(-10, -30),   S( -5,   8),   S( -1,  -4),   S(  9,  28),   S( -3, -19),   S( -4,  -3),
            S( -2,  -4),   S(-16, -31),   S( -1,   1),   S(  1,  -5),   S( 10,   5),   S( -6, -20),   S( -1,  -6),   S(  1,   1),
            S(  1,  11),   S(-12,  -9),   S( -1,   5),   S(  3,  -8),   S(-10, -13),   S( -5, -15),   S( -1,   3),   S(  1,   7),
            S(  3,   7),   S( -1,   6),   S(  6,  17),   S(  2,   4),   S( -3,  -6),   S( -4, -13),   S( -3, -12),   S( -1,  -2),

            /* knights: bucket 13 */
            S( -8, -26),   S(  3,  10),   S(  2,  -1),   S(  0,  -1),   S( -4,  -3),   S( -3, -13),   S( -3, -17),   S( -2,  -7),
            S( -4, -12),   S(  0,  -2),   S( -1,  -5),   S( -7, -20),   S(-15, -51),   S( -7, -25),   S( -1,   1),   S( -1, -11),
            S( -2,  -6),   S( -6, -25),   S(  9,  11),   S( -5,   9),   S(-11, -38),   S(  0, -12),   S(  1,   2),   S( -9, -31),
            S( -4,   1),   S(  6,  21),   S(  4,  -2),   S( -6, -24),   S( -4, -27),   S( 11,   0),   S(  1,   1),   S( -6,  -7),
            S(  5,  13),   S( -1,  13),   S( 16,  15),   S( 10,  23),   S(  6, -21),   S( -3,   8),   S( -4, -21),   S(  2,   2),
            S( -2, -11),   S( 18,  17),   S(  9,  38),   S( -9,  24),   S(  9,   6),   S(-12, -40),   S(  5,   8),   S( -4,   1),
            S(  2,   8),   S(  6,  12),   S( 11,   2),   S( -4,   3),   S( 22,  35),   S( -9, -32),   S( -5,   2),   S(-10, -20),
            S(  0,   0),   S(  3,  11),   S( -3, -17),   S(  6,   4),   S( -1,  -2),   S(  7,   0),   S(  0,  -7),   S( -2,  -2),

            /* knights: bucket 14 */
            S( -1, -15),   S( -8, -44),   S( -3,  -6),   S( -5,   7),   S( -2,  -5),   S(  0,  -5),   S( -3,  -9),   S(  3,  13),
            S( -1, -11),   S( -6, -13),   S(-16, -60),   S(-11, -48),   S(  1, -12),   S(  3,  12),   S(  1,   5),   S(  1,   5),
            S(-11, -27),   S( -5, -20),   S(-10, -32),   S(  2,  -9),   S(  4,   8),   S(  1,  -2),   S( -1, -10),   S(  3,  20),
            S(  3,  12),   S( -4, -35),   S(-12, -27),   S( -8, -35),   S( -7, -18),   S(  6,  -3),   S( -3, -19),   S( -6,   9),
            S(  2,  10),   S( -2,  -8),   S(  4,  24),   S( -6, -44),   S( -6, -20),   S(  4,   8),   S(  6,   5),   S(-11, -21),
            S( -3,  -1),   S(  8,  18),   S(-10, -31),   S( 14,  10),   S(  6,  -1),   S( -2,  -3),   S( -4,  -1),   S( -2,  -6),
            S(  1,   1),   S( -3, -13),   S( 15,  13),   S( -2, -12),   S( -2,   2),   S( -4, -12),   S( -6,   1),   S(  0,  -1),
            S( -1,  -3),   S(  3,   7),   S( -4, -10),   S(  6,  -3),   S(  4,  26),   S(  3,  19),   S( -1,  -4),   S( -2,  -6),

            /* knights: bucket 15 */
            S( -1,  -2),   S(  0,  -7),   S( -3, -13),   S( -5, -15),   S( -4,   1),   S(  0,  -5),   S( -1,  -2),   S(  0,  10),
            S( -3, -20),   S( -2, -11),   S( -1,  -6),   S( -7, -29),   S(  2,   2),   S( -1,  -9),   S(  1,  10),   S( -2,  -9),
            S(-12, -37),   S( -2, -25),   S( -2,  -5),   S(-13, -38),   S(-19, -58),   S( -1,   1),   S( -2,  -1),   S( -2,   1),
            S( -7, -21),   S( -8, -33),   S( -3, -20),   S( -6, -11),   S( -6, -38),   S( -2, -16),   S(  4,  10),   S( -3,   3),
            S( -2, -11),   S(  2,  13),   S(  3,   1),   S(-10, -17),   S( -3,   7),   S( 10,  14),   S(-13, -21),   S( -8, -16),
            S(  1,   2),   S(  2,   7),   S(  3, -17),   S(  0,  -7),   S( -3,  -4),   S( -4,   0),   S( -1,  16),   S( -1,  14),
            S( -5, -19),   S( -3, -10),   S( -2, -20),   S( -4,   6),   S(-12, -11),   S( -6,   7),   S(-11,  -8),   S(  4,  17),
            S(  0,   0),   S( -2,  -2),   S( -3,  -7),   S(  6,   0),   S( -1,   8),   S(-18, -18),   S(  3,   8),   S( -5,  -1),

            /* bishops: bucket 0 */
            S( 31,  68),   S( 30,  -9),   S( 56,  11),   S(  6,  26),   S( -5, -11),   S(  2, -15),   S( -2, -29),   S(-15, -41),
            S( 37, -23),   S( 89, -10),   S( 53, -10),   S( 16, -24),   S( 10,  12),   S(  6, -31),   S(-26,   4),   S(-19, -54),
            S( 12,  41),   S( 55,   3),   S( 37,  26),   S(  6,  56),   S( 15,  19),   S( -3,  28),   S( 16, -37),   S( 11, -46),
            S( 20,  -5),   S( 75, -12),   S( 48,   6),   S( 34,  31),   S( 11,  11),   S( 27,   2),   S(  4, -12),   S( -5, -33),
            S( 12,   2),   S( 47,  -3),   S(  7,  15),   S( 67,   1),   S( 78, -21),   S(  6,   9),   S( 21, -12),   S(-32,   8),
            S(-44,  36),   S( 13,  21),   S( 70,  13),   S( 74, -32),   S( 44,   6),   S(-15,  14),   S(-10, -15),   S( 13,  -4),
            S(-43,  29),   S( 25,   8),   S( 42,  -8),   S(  8,  35),   S(-46,  -1),   S( 32,  22),   S( 21, -13),   S(-13, -10),
            S(-22, -45),   S( 18,  -3),   S(-48,  -1),   S( 24,   5),   S( 29,  30),   S( 53,   8),   S(-31,  19),   S(-13,  -6),

            /* bishops: bucket 1 */
            S( 27,  13),   S(-16,  28),   S(  3,  27),   S( -4,  24),   S(-15,  23),   S( -8,  32),   S(-18,  15),   S(-37,  23),
            S(  0, -29),   S( 42,   1),   S( 43,   5),   S( 28,  24),   S( -1,   9),   S( 22, -31),   S(-26,  17),   S(  6, -13),
            S( 44, -17),   S( 18,  -1),   S( 62,   5),   S( 12,  22),   S( 18,  20),   S(-12,  30),   S( 25,   0),   S(  0, -27),
            S( 10,  18),   S( 11,  23),   S( 17,   6),   S( 31,  22),   S( -7,  44),   S( 20,   7),   S( -4,  10),   S( -3,  19),
            S( 53,  31),   S( 22,  23),   S(  3,  39),   S( -1,  38),   S( 34,   2),   S(  2,  14),   S( 30, -17),   S(-31,  18),
            S(-11,  22),   S( 35,  24),   S(  6,  41),   S( 75, -12),   S( 15,  31),   S( 40,  -1),   S( 12,  28),   S( 27, -10),
            S( -7,  26),   S(-14,  42),   S( 26,  26),   S( 21,  17),   S( 46,  14),   S(-47,  36),   S( 12,  13),   S(-23,  35),
            S( 43, -18),   S( -9, -16),   S(  0, -10),   S(-22,  14),   S( 13,  23),   S(-11,  -8),   S(  2, -16),   S( 22,  31),

            /* bishops: bucket 2 */
            S( 18,   3),   S( -4,  20),   S(  0,  13),   S(-20,  34),   S(-22,  36),   S(-23,  23),   S(-21,  -8),   S(-54,  34),
            S(-19,  17),   S( 14,   2),   S( 21,   7),   S(  6,  19),   S(  1,  23),   S( 15,   3),   S(  8, -19),   S( -1, -40),
            S( -7,   0),   S(-13,  22),   S( 19,  22),   S(  1,  35),   S(  4,  29),   S( 12,  27),   S( 14,  15),   S(-22, -11),
            S(  5,   4),   S( -6,   6),   S(-12,  35),   S(  3,  47),   S( -3,  49),   S(  7,  18),   S(  5,  26),   S(-14,  -6),
            S(  1,   1),   S( -6,  22),   S(-16,  32),   S(-25,  36),   S( -6,  34),   S(  9,  32),   S(  8,   7),   S(-22,  16),
            S(  7,   5),   S( -6,  10),   S( -3,  31),   S(-25,  32),   S(  9,   1),   S(-19,  27),   S(  3,  57),   S(  7,  17),
            S(-21,  16),   S(-11,  41),   S(-40,  48),   S( 46,  -4),   S(-28,   9),   S(-15, -10),   S(-48,  53),   S(-39,  20),
            S(-65,  50),   S(-38,  45),   S(-63,  47),   S(-28,  25),   S(-51,  20),   S( -1,   5),   S( 25, -12),   S(-49,  28),

            /* bishops: bucket 3 */
            S(  1,  34),   S( 21,   4),   S( 22,  15),   S( 16,  23),   S( 16,   9),   S( 46,  -7),   S( 43, -22),   S( 58, -64),
            S(  9,  -4),   S( 17,  19),   S( 31,  -4),   S( 18,  31),   S( 27,  13),   S( 17,  30),   S( 60,   9),   S( 28,  -6),
            S( 27,   0),   S(  4,  25),   S( 29,  31),   S( 30,  22),   S( 24,  50),   S( 36,  33),   S( 43,  20),   S( 44,  -1),
            S( 26,   3),   S( 17,  21),   S( 17,  44),   S( 25,  53),   S( 29,  44),   S( 37,  30),   S( 35,  22),   S(  6,  -1),
            S( 27,  -2),   S( 33,  20),   S( 31,  21),   S( 21,  57),   S( 24,  48),   S( 41,  36),   S( 25,  19),   S( 28,  35),
            S( 27,   3),   S( 23,  25),   S( 47,  22),   S( 40,  16),   S( 31,   8),   S( 49,  33),   S( 40,  14),   S(  3,  75),
            S( 12,  13),   S( 10,  34),   S( 28,  18),   S( 13,  14),   S( 21,  -4),   S( 31,   1),   S( 10,  40),   S( 28,  20),
            S(-21,  73),   S( -5,  21),   S( 66,   4),   S( -2,  31),   S( -4,  32),   S( 14,  15),   S( 44, -12),   S( 94, -11),

            /* bishops: bucket 4 */
            S(-25, -52),   S(-52, -20),   S(-73,  -6),   S(-18,   8),   S(-47,  22),   S(-55,  30),   S(  3, -37),   S(  1, -17),
            S(-19,   1),   S( 14,  -3),   S(-22,  41),   S(-21,   1),   S(-39,  -1),   S( 50, -18),   S(-18,  17),   S( 24,  -5),
            S(-24,   5),   S(-40,  57),   S( -3,  13),   S(-18,  18),   S( 47,   1),   S( 36,   5),   S(-32, -31),   S(-59, -29),
            S(-25,  33),   S( 25,  14),   S( 38,  29),   S( 17,  46),   S(  3,  27),   S( 29,  12),   S( 58,  -1),   S( 14, -60),
            S(-15,   5),   S(  6,  46),   S( -4,  58),   S( 38,  23),   S( 31,  12),   S(  8, -30),   S(-11, -29),   S(  0, -25),
            S(  2,  15),   S( 15,  27),   S(-22,  32),   S(  1,  -5),   S( 42, -11),   S( 14,  18),   S( -3, -38),   S(-11, -14),
            S(-25,   5),   S( 52,  25),   S(  4,  30),   S( 13,  44),   S( 23,   2),   S(-15,  -7),   S(  8,   2),   S( 19, -24),
            S(  6,  -7),   S(-24, -61),   S(  6,   0),   S(-14,  -1),   S( -3, -18),   S( 13,   0),   S( -1,  -4),   S(  4,  14),

            /* bishops: bucket 5 */
            S( 10,  -6),   S(-25,  34),   S(-58,  38),   S(-53,  39),   S(-47,  30),   S(-19,  22),   S(-14,  34),   S( -1,  36),
            S(-28,  49),   S(-14,  21),   S(-61,  63),   S( -8,  24),   S(-53,  50),   S(-30,  22),   S(-34,  -6),   S( -8,  -7),
            S(-17,  14),   S( -2,  40),   S(-25,  41),   S(-38,  57),   S( -3,  32),   S(-25,  18),   S(-18,  33),   S(-23,  -8),
            S( 14,  10),   S( 10,  31),   S(-39,  61),   S( -9,  40),   S( 36,  30),   S( 18,  24),   S(  8,  -7),   S( 18,  34),
            S( 19,  60),   S( 12,  18),   S( 45,  30),   S( 84,  22),   S( 40,  27),   S( 28,  15),   S( 33,  16),   S( 19,   9),
            S(-18,  43),   S( 62,  20),   S( 29,  41),   S( 34,  26),   S(  1,  21),   S( 10,  -4),   S(-19,  47),   S( -3,  29),
            S( 10,  45),   S( 20,   8),   S( 11,  31),   S(  3,  29),   S( 15,  12),   S( -5,  48),   S(  8,  33),   S(  1,  35),
            S( -8,  14),   S(  7,  26),   S( -2,  47),   S( 20,  41),   S( -4,  48),   S( 22,  27),   S( 22,  53),   S(-13,  -7),

            /* bishops: bucket 6 */
            S(  5,  26),   S(-15,  40),   S(-45,  35),   S(-63,  34),   S(-64,  36),   S(-35,  35),   S(-30,  52),   S( -8, -12),
            S(  3,   2),   S(  1,  -1),   S(-18,  27),   S(-16,  35),   S(-27,  40),   S(-19,  26),   S(-135,  65),  S( 32,   3),
            S( 29,  -5),   S( 31, -15),   S( 55,  10),   S( 28,  27),   S( 31,  22),   S(  6,  25),   S(  9,  34),   S(-63,  34),
            S( 30,  39),   S( 29,   7),   S( 56,   8),   S( 32,  36),   S( 42,  29),   S( 24,  20),   S( 36,  30),   S(-19,   3),
            S(  5,  12),   S( 50,   7),   S( 45,  11),   S( 35,  25),   S( 85,  35),   S( 69,  21),   S( 36,  30),   S(-30,  35),
            S( -7,  -1),   S(-69,  46),   S( 24,  29),   S( -8,  45),   S( 76,   4),   S( 17,  45),   S(  7,  50),   S( -6,  64),
            S(-15,  12),   S(-31,  43),   S( 33,  28),   S(  7,   7),   S( 31,  28),   S( 22,  35),   S(  0,  41),   S( -6,  11),
            S( 11,  47),   S(  9,  35),   S(  8,  26),   S( -5,  37),   S(-29,  44),   S( 12,  11),   S( 17,  22),   S(  6,  16),

            /* bishops: bucket 7 */
            S(  3, -28),   S( 19,  -6),   S(-37, -31),   S(-17,  -7),   S(-13, -26),   S(-122,  37),  S(-48, -49),   S(-62,   9),
            S(-40, -45),   S(-91,   5),   S(-20, -12),   S(  8, -20),   S(-34,  10),   S(-57,  14),   S(-53,  10),   S(-28,  -8),
            S(-22, -21),   S(-11, -16),   S( 30, -19),   S( 37, -12),   S(-35,  14),   S(-20,  10),   S(-45,  34),   S(-41,  35),
            S(-29,  -8),   S( 76, -27),   S( 95, -34),   S( 67,  -6),   S( 92, -14),   S(-11,  22),   S( 11,  37),   S( 15,  19),
            S(  4, -45),   S(-45,   6),   S( 84, -46),   S( 92, -10),   S( 60,  34),   S( 65,  25),   S(-25,  49),   S(  8,   8),
            S(-20, -19),   S(-36,  17),   S( 35, -15),   S( 13,  -4),   S( 17,   4),   S( 81,  14),   S( 82,  14),   S( 21, -13),
            S( 13, -42),   S(-25,  13),   S(  6, -13),   S(  7, -19),   S(  7, -22),   S( 49, -39),   S( 24,  21),   S( 12,  10),
            S(  5,  11),   S( -6,  10),   S(-36,  17),   S( -2, -25),   S( 28, -33),   S( 18, -45),   S( 18,  15),   S( 17,  13),

            /* bishops: bucket 8 */
            S(-11, -20),   S(  1, -31),   S(-38, -19),   S( -2,   5),   S(-11,  21),   S(-21,  -2),   S(  6,  15),   S( -2,  -4),
            S(-12, -21),   S(-30, -45),   S(-24, -44),   S(  8,  -1),   S( 16,  -1),   S(-34, -16),   S( -6, -29),   S(-17,  -3),
            S(  9,   8),   S(-10,  13),   S(-35,  38),   S( -7,  27),   S(-24,   8),   S(  1, -25),   S( 15, -47),   S(-25, -25),
            S( -2,  25),   S(-32,  22),   S(  1,  36),   S(  0,  12),   S( 11,  36),   S(-20,  26),   S( 19,  -6),   S( -5, -14),
            S( 11,  30),   S(  9,  65),   S(-14,  29),   S( 50,  37),   S(  7,   0),   S( 13,  13),   S( 20,  -6),   S(-20, -45),
            S(-11,  -1),   S( -8,  -7),   S( 49,  44),   S(-18,  40),   S( 38,  11),   S( -2, -31),   S(-19, -16),   S(-24, -48),
            S(-13, -11),   S( 26,  30),   S( -9,  20),   S( -2,   3),   S(  9,  15),   S(  0,   7),   S( -9, -10),   S( -7, -46),
            S(  2,  17),   S(  8, -43),   S(  1, -21),   S( -3, -22),   S(-20,  -8),   S( -2,  -1),   S( -2,   5),   S( -3,  18),

            /* bishops: bucket 9 */
            S(-19,  -1),   S(-11, -40),   S(-12,   4),   S(-22,   2),   S(-33, -12),   S(-20, -62),   S(-21, -38),   S(  8,   2),
            S(  0,  -2),   S(-28, -14),   S( 10,  -1),   S(-10,  18),   S(-41,  32),   S(-23, -16),   S( -4,  -4),   S(  7,  15),
            S( 16,  22),   S( 24,  19),   S(-20,   8),   S( -4,  18),   S( 27,  19),   S(-11,   9),   S( -7, -29),   S(  8,  32),
            S( -3,  14),   S( 18,  41),   S(-11,  30),   S(  1,  32),   S( 34,  49),   S( 22,  -6),   S( 12,  -8),   S(-13, -22),
            S( -9,  -5),   S( 19,  15),   S( -1,  46),   S( 29,  31),   S(-10,   8),   S(  6,  29),   S(-19,  35),   S(  3,  -2),
            S(-13,  11),   S( 26,  85),   S(  1,  60),   S( -2,  57),   S( 14,  17),   S(-11,  29),   S(-14, -15),   S(-16,   2),
            S( -1,  -6),   S( 24,   4),   S(  7,   1),   S(-12,  25),   S( 19,  33),   S( 13,   5),   S( 14,  -3),   S( -6, -30),
            S(  3, -27),   S(  2,  43),   S( -7,  36),   S(-21, -11),   S(-21, -34),   S(  9,  56),   S(  9,   6),   S( -6,  10),

            /* bishops: bucket 10 */
            S(-22, -27),   S(  2, -63),   S(-42, -19),   S(-18, -29),   S( -8,   5),   S(-33, -14),   S(-17, -59),   S(-17, -16),
            S(  4,  -6),   S(-18,  -9),   S(-22, -20),   S(-40,  21),   S(-42, -10),   S(-19,  27),   S(-22, -68),   S(-15, -26),
            S( -5, -42),   S(  6, -38),   S(-30, -10),   S(-13,  18),   S(-17,   6),   S(-42,  36),   S(-12,  17),   S(  4,  17),
            S( -2, -24),   S(  8,  20),   S( 14, -25),   S( 31,  -1),   S( 17,  16),   S(  5,  43),   S( 19,  23),   S( 11,  28),
            S(-20, -28),   S( 18,   8),   S( -8,  17),   S( 49,  19),   S( -1,  78),   S( 16,  50),   S( 28,  25),   S( -5,   0),
            S( -2,  -6),   S(-14, -13),   S( 10, -17),   S( -4,   4),   S( -2,  34),   S( 44,  53),   S( 10,  56),   S( -1,  14),
            S(-19,   2),   S(-11, -33),   S( -4, -26),   S( 13,  24),   S( -5, -11),   S( 19,  34),   S( 29,  61),   S( 10,  28),
            S(-11, -25),   S(  4,  19),   S( 15,   6),   S( -1,  -9),   S(  6,  19),   S( -9,   3),   S( 12,   4),   S( 10,  39),

            /* bishops: bucket 11 */
            S(  0,  36),   S(-42, -22),   S(-44, -66),   S(-20, -15),   S(-23, -18),   S(-63, -59),   S( -9, -15),   S(-30, -37),
            S( -1, -21),   S( 25,   0),   S(-19, -46),   S(-37, -33),   S(-59,   2),   S(-18, -62),   S( -7, -23),   S(-28, -40),
            S(-30, -68),   S( 11, -54),   S(-23, -20),   S( -2,   4),   S(-23, -31),   S(-32,  14),   S(-33,   7),   S( 11,  26),
            S(-13, -37),   S(-15, -35),   S(  0, -10),   S(  0, -27),   S( 24, -16),   S( 13,  77),   S(  3,  54),   S( 22,  46),
            S(-14, -21),   S(-19, -64),   S(-30,  30),   S( 44, -23),   S( 42,  13),   S(  2,  48),   S(-13,  29),   S( 17,  29),
            S(-27, -70),   S(-24,  -8),   S(  3, -16),   S( 16,  41),   S(  0,  19),   S( 21,  62),   S( 26,  11),   S( -9,  -6),
            S( -7, -24),   S(-20, -21),   S( -5, -10),   S( -6, -11),   S( 11,   0),   S( 17, -12),   S(  8,  30),   S( 12,   5),
            S(-23,   0),   S(-25, -22),   S(  1,   1),   S(  4,   3),   S( 28,   1),   S(-22, -33),   S( -6,  19),   S(  8, -10),

            /* bishops: bucket 12 */
            S(  1,   8),   S(-11, -33),   S(-16, -36),   S(-19, -55),   S( -8, -22),   S(-13, -45),   S( -3,  23),   S( -2,  -1),
            S( -8, -12),   S(-18, -47),   S( -7,   2),   S( -6,  10),   S(-18, -50),   S(  6,  28),   S( -3,   6),   S( -4,  -6),
            S(  1,  -6),   S(-20, -15),   S(-13, -10),   S( -7,  -9),   S(  8,  -9),   S( -4, -10),   S( -8, -38),   S(  6,   6),
            S( -9, -12),   S(-15, -21),   S(-14, -50),   S(-14,   7),   S(  0,  13),   S(  2,   2),   S( -5,  -7),   S(-13, -12),
            S( -1,   3),   S( -6,  -4),   S( -6,  26),   S(  4,   3),   S( -3, -12),   S(-10,   1),   S(  5,   5),   S( -6,  -9),
            S(-16, -10),   S( -5,  36),   S(  1,  20),   S(-22,  -9),   S( 12,   5),   S( -8,  -4),   S( -4,  -3),   S( -1,  -3),
            S( -4,  -2),   S( -3,  24),   S(  4,  12),   S(  0,  28),   S( -2,   6),   S( 16,  16),   S( -4, -31),   S( -5,  -2),
            S(  1,  10),   S(  4,   2),   S( -5, -12),   S(  5,  18),   S( -4,  -9),   S(  0,   2),   S( -1,  17),   S(  1,  -1),

            /* bishops: bucket 13 */
            S( -8, -45),   S( -9, -23),   S( -5,  -7),   S(-20,   5),   S(-24, -33),   S(-26, -11),   S( -1,  -4),   S( -8, -12),
            S( -5,  -9),   S( -4,  -5),   S(  0,  -6),   S(-19, -12),   S(-13,  14),   S(-12,   9),   S( 14,  23),   S(  0,  -5),
            S( -9,   0),   S( -1,  17),   S(  2,  31),   S(-18,  17),   S(-15, -19),   S( -1,  -2),   S( -5, -42),   S(  2,   8),
            S( -3,  -4),   S(-23,  -5),   S( -6,   9),   S(-35,   3),   S(  4,  -8),   S( -7, -10),   S( -6,  12),   S(-11, -21),
            S( -4,   9),   S(-18,   9),   S(-16, -15),   S( 14,   9),   S( -4,  56),   S( -5,   7),   S( -9,  -2),   S( -6, -13),
            S( -2,   1),   S( -6,  -1),   S(-17, -12),   S( 30, -13),   S( -1,  31),   S(  9,  -2),   S(  6,  27),   S( -8, -16),
            S( -6, -17),   S(-11,  24),   S( 11,  37),   S(  2,  36),   S(-11,  10),   S(  6,   4),   S(-15, -25),   S(  4,  13),
            S( -3,  -4),   S(  7,  16),   S( -5,   7),   S( -1, -28),   S(  0,   5),   S( -8, -18),   S( -5,   1),   S( -6, -23),

            /* bishops: bucket 14 */
            S( -1,  -2),   S(-13,  -6),   S(-23, -21),   S(-15, -47),   S(-17, -47),   S( -7, -22),   S(-16, -35),   S(-13, -27),
            S( -8, -20),   S(  3,  -2),   S( -5,   1),   S(-22, -40),   S(-13, -16),   S(-17,   4),   S(-17, -16),   S( -3, -15),
            S( -1,   5),   S(-13, -24),   S(-16, -11),   S(-15, -35),   S(-28, -24),   S(-18, -19),   S( -8,   5),   S( -7,  -1),
            S(-12, -34),   S(  1,  -5),   S( -8,  -3),   S(-14,  32),   S( -8,   8),   S(-13,  18),   S(-25, -24),   S( -5, -17),
            S( -5,  -8),   S(-18,  -5),   S( -5, -11),   S(  1,  -4),   S(-23, -14),   S(-16, -15),   S(  6,   0),   S(  3, -18),
            S(  0,   7),   S( -6,  -1),   S(-11,  -8),   S( -8,  -7),   S(  0,   7),   S( -3,   5),   S(-20,  20),   S(-10,  -7),
            S( -6, -28),   S(  1, -14),   S(  0,  12),   S( -1,   3),   S(-10, -11),   S( -2,   5),   S( -5, -26),   S( -4,  -5),
            S( -4, -10),   S( -7, -13),   S( -5,  -5),   S( -8,  -3),   S(-13, -13),   S( -1,  19),   S( -3, -12),   S(  2,   5),

            /* bishops: bucket 15 */
            S(  7,  26),   S(  5,   4),   S(-19, -20),   S( -4, -16),   S( -9,  -2),   S(-14, -24),   S( -5,  -7),   S( -1,  -3),
            S(  4,   9),   S(  8,  19),   S(  0, -27),   S( -6,  -9),   S(-19, -31),   S( -6, -10),   S( -1,  -4),   S( -1,  -5),
            S( -9, -17),   S( -1,  -8),   S(-11,   3),   S( -9, -23),   S(-15,  13),   S( -3,  21),   S( -9, -15),   S( -2,  12),
            S( -1,   6),   S(-12, -10),   S( -2, -31),   S(-26, -27),   S(-11,   5),   S(  1,  -4),   S( -1,  15),   S(  2,  -8),
            S( -1, -17),   S(-15, -18),   S(-16,   2),   S(-22, -53),   S( -8, -38),   S(-13,   2),   S(  8,  18),   S( -3, -17),
            S(-11, -32),   S(-14, -10),   S(-16, -25),   S(-24, -14),   S( -2,  -4),   S( -7,  12),   S( 12,  40),   S( -3,   2),
            S( -4,  -1),   S(  0,   4),   S( -1,  -2),   S( -6, -13),   S(-15, -28),   S(-10,   4),   S(-16,   0),   S(  3,   2),
            S( -6, -18),   S( -2,  -2),   S(-11,  -3),   S( -9,  -5),   S( -6,  -7),   S(-19, -29),   S( -8, -34),   S(  4,   7),

            /* rooks: bucket 0 */
            S(-44,  24),   S( -6, -15),   S(-13,  -9),   S( -2,   4),   S(-10,  23),   S( -1,  -1),   S(  1,  11),   S( -4,  24),
            S( 35, -58),   S( 27, -20),   S( 10,  -3),   S(-13,  19),   S( 17,   8),   S(  2,  -6),   S(-21,  -9),   S(-63,  51),
            S( 13, -19),   S( 16,   3),   S( 35,  -9),   S( 18,   8),   S(-16,  51),   S(  8,  -4),   S(-28,  -6),   S(-49,  23),
            S( 24, -21),   S( 60,  -9),   S( 37,  32),   S( 40,   7),   S( 20,  16),   S(-22,  35),   S(-19,  18),   S(-46,  43),
            S( 34, -19),   S( 83, -15),   S( 70,  -9),   S( 36,  -8),   S( 49,  -6),   S( 24,  12),   S(-15,  53),   S(-34,  51),
            S( 88, -71),   S( 72, -32),   S( 59,  -3),   S( 37,  12),   S( 54, -11),   S(-26,  38),   S( 13,  15),   S(-35,  39),
            S( 56, -23),   S( 87,  -9),   S( 14,  25),   S( 36,   8),   S(  4,   3),   S( -2,   9),   S(-38,  65),   S(-21,  28),
            S( 54,  -2),   S( 32,  36),   S( 34,  46),   S( 45,  19),   S( 27,  29),   S( 20,  -2),   S(-19,  63),   S( 17,  29),

            /* rooks: bucket 1 */
            S(-86,  33),   S(-53,   2),   S(-58,   6),   S(-43, -10),   S(-32, -13),   S(-29,  -8),   S(-34,   0),   S(-40,  16),
            S(-47,  -9),   S(-68,   4),   S(-27,  -9),   S(-37, -30),   S(-36,  -5),   S(-52,  -9),   S(-32, -41),   S(-57,   0),
            S(  4,   5),   S(-33,  38),   S(-24,  12),   S(-26,   3),   S(-56,  37),   S(-34,  20),   S(-28,  10),   S(-49,   5),
            S(-54,  52),   S(-61,  47),   S( 17,  20),   S(-22,  32),   S(-40,  48),   S(-41,  46),   S(-24,  40),   S(-48,  29),
            S( 60,   9),   S( 16,  49),   S(-20,  32),   S(-48,  51),   S( -4,  22),   S( 49,  15),   S(  3,  17),   S(-40,  20),
            S( 59,   7),   S(  0,  38),   S( 26,  18),   S(-18,  19),   S( 19,  19),   S(-26,  44),   S(-10,  23),   S(-47,  43),
            S(-51,  56),   S( 10,  30),   S( 10,  45),   S(-39,  48),   S(-47,  57),   S( 11,  41),   S(-28,  33),   S(-47,  31),
            S( 13,  23),   S( 34,  42),   S( 27,  16),   S(-45,  49),   S(  6,  10),   S(  1,  35),   S(-22,  44),   S(  8,  14),

            /* rooks: bucket 2 */
            S(-75,  41),   S(-50,  24),   S(-46,  14),   S(-60,  25),   S(-63,  14),   S(-52,   2),   S(-39, -22),   S(-53,  21),
            S(-79,  39),   S(-70,  37),   S(-49,  20),   S(-66,  12),   S(-34, -16),   S(-51, -15),   S(-69,  -5),   S(-61,   1),
            S(-73,  59),   S(-49,  42),   S(-44,  47),   S(-31,   5),   S(-51,  27),   S(-42,  19),   S(-22,  19),   S(-42,  19),
            S(-87,  66),   S(-58,  59),   S(-46,  64),   S(-28,  48),   S(-17,  28),   S( -7,  38),   S(-41,  49),   S(-23,  19),
            S(-41,  62),   S(-54,  69),   S(-46,  63),   S(-29,  43),   S( 26,  16),   S( -4,  56),   S(-18,  40),   S(-24,  30),
            S(-55,  62),   S(-42,  46),   S(-48,  51),   S( 21,   8),   S( -8,  37),   S(  7,  38),   S(-24,  48),   S(-36,  43),
            S(-73,  53),   S(-73,  78),   S(-39,  61),   S( -1,  49),   S( 10,  28),   S( 13,  20),   S(-61,  72),   S(-17,  48),
            S(-39,  74),   S(-24,  55),   S(-65,  62),   S(-11,  46),   S(-62,  72),   S(-19,  49),   S(-20,  68),   S( 15,  42),

            /* rooks: bucket 3 */
            S(  1,  71),   S( -4,  73),   S(  9,  60),   S( 14,  63),   S( 10,  54),   S( -8,  67),   S( -5,  86),   S(-10,  38),
            S(-29,  82),   S( -3,  65),   S(  0,  62),   S( 11,  53),   S( 18,  49),   S( 11,  56),   S( 57, -20),   S( 23, -53),
            S(-33,  85),   S( -6,  74),   S(  5,  82),   S( 17,  66),   S( 15,  75),   S( 17,  68),   S( 29,  75),   S(  2,  54),
            S(-28,  92),   S(-28,  92),   S( 20,  77),   S( 21,  76),   S( 28,  70),   S( 14, 101),   S( 61,  66),   S(  7,  80),
            S(-11,  99),   S( 26,  87),   S( 26,  69),   S( 40,  73),   S( 52,  60),   S( 47,  64),   S( 85,  58),   S( 62,  44),
            S(-19,  99),   S( 15,  79),   S( 10,  78),   S( 30,  66),   S( 25,  62),   S( 39,  63),   S(107,  23),   S(113,  17),
            S(-45, 107),   S(-20, 110),   S( -2,  96),   S( 35,  76),   S( 28,  73),   S( 36,  75),   S( 48,  76),   S( 84,  35),
            S(-66, 154),   S( -2, 106),   S( 28,  73),   S( 32,  76),   S( 74,  59),   S( 99,  45),   S(144,  39),   S(100,  40),

            /* rooks: bucket 4 */
            S(-87,  26),   S( -9,   5),   S(-45,  -3),   S(-13,   9),   S(-32, -27),   S( -3, -46),   S(-15,   0),   S( -8, -35),
            S(-38, -19),   S(-60,  -5),   S(-52,  18),   S(-20,  13),   S( -6, -30),   S(-46, -18),   S( -6, -14),   S(-40,  -9),
            S(  1,   2),   S(-47, -43),   S(-23,   5),   S( -7,  -2),   S( 11, -16),   S(-30, -10),   S( 22, -38),   S(-63,  -7),
            S(-39, -29),   S( 27,   8),   S(-23,  25),   S( 52,   1),   S(  9,   7),   S(  9, -13),   S(  2,   3),   S(-28,  28),
            S(-18, -22),   S(-18,  18),   S(  5,  27),   S(108,   2),   S(  9,  14),   S( 11,  25),   S( 55,  31),   S(  8,  -7),
            S( 39,  -3),   S( 31,  11),   S( 74,  23),   S( 15,   8),   S( 67,  11),   S( 31,  10),   S( 18,  18),   S( 10,  40),
            S( 29, -15),   S( 50,  31),   S( 41,  51),   S( 48,  27),   S( 49,  28),   S( 14,   6),   S(  0,  17),   S( 42,  28),
            S( 46, -59),   S( 55,  23),   S( 17,  32),   S( 26,  13),   S( 38, -17),   S(  7,   1),   S( 32,  11),   S( 11,  15),

            /* rooks: bucket 5 */
            S(-11,  10),   S(-54,  63),   S(-55,  41),   S(-30,  22),   S(-13,  18),   S(-49,  54),   S(-10,  33),   S(-45,  43),
            S(-34,  36),   S(-41,  23),   S(-77,  57),   S(-46,  20),   S(-38,  15),   S(-18,  16),   S(  4,  14),   S(-22,  20),
            S(-29,  53),   S(-62,  53),   S(-61,  65),   S(-107,  63),  S(-24,  14),   S(-12,   8),   S( -5,  58),   S(-20,  31),
            S(-40,  76),   S(  4,  40),   S(-13,  53),   S(-17,  38),   S(-30,  70),   S(-22,  84),   S( -9,  40),   S( 15,  30),
            S( 13,  64),   S(-17,  86),   S( 15,  55),   S( 35,  52),   S( 45,  44),   S( 22,  56),   S( 55,  55),   S( 57,  33),
            S( 62,  69),   S( 17,  77),   S( 56,  45),   S( 27,  72),   S( 67,  60),   S( 64,  61),   S( 43,  62),   S( 55,  43),
            S( 59,  35),   S( 22,  64),   S( 40,  63),   S( 86,  34),   S( 56,  50),   S( 61,  63),   S( 92,  33),   S(100,  27),
            S( 94,  45),   S( 81,  33),   S( 23,  44),   S( 28,  29),   S( 20,  56),   S( 51,  53),   S( 40,  49),   S(  4,  53),

            /* rooks: bucket 6 */
            S(-73,  37),   S(-40,  28),   S(-30,  28),   S(-22,  18),   S(-52,  23),   S(-86,  47),   S(-45,  58),   S(-29,  44),
            S(-39,  32),   S(-32,  44),   S(-38,  43),   S(-76,  40),   S(-48,  34),   S(-98,  68),   S(-93,  66),   S(-35,  46),
            S(-46,  52),   S(-33,  36),   S(-31,  43),   S(-61,  49),   S(  0,  15),   S(-70,  65),   S(-66,  75),   S( 18,  18),
            S(-55,  76),   S(  5,  45),   S(-25,  64),   S( 17,  36),   S(-31,  40),   S( 20,  41),   S(-68,  68),   S( -9,  56),
            S(-30,  82),   S( 13,  64),   S( 54,  53),   S( 34,  39),   S( 21,  51),   S( 43,  53),   S( 19,  66),   S( -5,  67),
            S(  8,  63),   S( 43,  55),   S(107,  29),   S( 22,  44),   S( 35,  41),   S( 32,  65),   S( 43,  58),   S( 41,  57),
            S( 30,  64),   S( 68,  40),   S( 84,  30),   S( 30,  42),   S(104,  31),   S( 68,  55),   S( 86,  52),   S( 28,  52),
            S( 35,  77),   S( 38,  58),   S( -7,  62),   S( 40,  37),   S( 66,  37),   S( 49,  63),   S( 66,  56),   S( 10,  61),

            /* rooks: bucket 7 */
            S(-70, -12),   S(-36, -10),   S(-40, -17),   S(-19,  -1),   S(-16,  -7),   S(-57,  32),   S(-57,  18),   S(  2, -20),
            S(-99,  37),   S(-42,  10),   S(-41,   3),   S( -3, -23),   S(-15,   7),   S(  7,   2),   S(-44,   1),   S(-36,   6),
            S(-114,  65),  S(-58,  19),   S(  3,   4),   S( -8,  -9),   S(  6,   1),   S(-27,   3),   S(-25,   3),   S( 21,  -6),
            S(-57,  32),   S( 13,  17),   S( 14,  14),   S( 41,  -4),   S( 18,   6),   S( 18,  10),   S( 53, -23),   S( 31, -11),
            S(-14,  43),   S(  1,  28),   S( 31,   3),   S( 56,  -2),   S( 72,  -1),   S( 79,   5),   S( 49,  18),   S( 54, -20),
            S(-39,  45),   S(  9,  16),   S( 87, -15),   S( 88, -25),   S( 69,  -6),   S( 55,  19),   S( 82,  17),   S( 31, -17),
            S( -8,  39),   S( 19,  22),   S( 60,   6),   S( 76,   2),   S( 96,  -1),   S( 97,  -7),   S( 57,  29),   S( 26,  12),
            S( 20,  60),   S(-43,  50),   S( 18,  19),   S( 96, -18),   S( 32,  15),   S(  0,  16),   S( 70, -11),   S( 62,  -4),

            /* rooks: bucket 8 */
            S(-47, -43),   S(-20,  -5),   S(  2,  15),   S( 12,   0),   S(-10, -43),   S(-11, -51),   S(-27, -37),   S(  0, -21),
            S( -8, -20),   S( 11,  15),   S(-12, -24),   S( 11,  -2),   S(  7, -38),   S(-11,  -8),   S(-20, -84),   S( -9, -72),
            S(  2,  25),   S(  5,  -7),   S( -3,   3),   S(  6, -11),   S( -8, -38),   S(  7, -44),   S( -4,   9),   S(  4,  14),
            S(-14, -34),   S( -7, -16),   S(  2,  25),   S( 14,   1),   S(  6, -13),   S(-21, -29),   S(  0, -14),   S(-10,  -6),
            S(-12, -25),   S(  6, -13),   S( -2,  20),   S( 12, -14),   S(-11,   9),   S( 22,  10),   S(  8, -24),   S(  1, -60),
            S( -1,  21),   S(-29, -31),   S( 38,  34),   S( 30,   0),   S(-10, -18),   S(  1, -34),   S( 14,  13),   S( 12,  27),
            S(-11,  -2),   S( 17,  -6),   S( 28, -12),   S( 25,  -1),   S( 27,  -5),   S( 35, -12),   S( 24, -20),   S(  6,  22),
            S( 17, -127),  S( 18, -11),   S(  6,  34),   S(  1, -29),   S(  3,  16),   S(  4, -29),   S( 17,   1),   S( 22,  -2),

            /* rooks: bucket 9 */
            S(-43, -10),   S( -7, -44),   S(  4, -27),   S(-46, -13),   S( 14,  12),   S(  4, -22),   S( 24, -38),   S(-28, -37),
            S( 41, -11),   S(-13, -39),   S( -4, -18),   S(-45, -11),   S(-21, -22),   S( 38,  -4),   S( 12, -47),   S( -9, -49),
            S( 13, -13),   S( 12,   3),   S(-11,  11),   S(-21, -23),   S(-25, -10),   S( 19, -28),   S( 19,  31),   S( -7,  11),
            S( -3,  29),   S(  9,   8),   S( 10,  23),   S(  9,  -5),   S(  7,   3),   S( 15, -15),   S( 26,  41),   S(  3,  14),
            S(-20,  15),   S(-12,   1),   S(  0,  -1),   S(  9,  28),   S( 45,  39),   S( 18,  25),   S( 23,  -3),   S( -3,   3),
            S( 12,  30),   S( -5,  15),   S(  0,   9),   S(-21, -12),   S( 17,  -4),   S( 15,  15),   S(  3,  37),   S( 24,  29),
            S( 74,  -3),   S( 61,   8),   S( 40,  19),   S( 47, -15),   S( 33,  -3),   S( 43,  -2),   S( 62, -15),   S( 48,  26),
            S( 73, -76),   S( 47, -46),   S( 30,  19),   S( 27,  26),   S( -9,  38),   S( 23,  34),   S( 22,   1),   S( 52,   9),

            /* rooks: bucket 10 */
            S(-56, -102),  S(  1, -36),   S(-36, -10),   S(-18,   1),   S(-51, -13),   S(-57,  -2),   S( 23,  -8),   S(-35, -15),
            S(  7, -13),   S(  8,  -7),   S(  7, -54),   S( 13, -12),   S( 31,   1),   S( 16,  -6),   S( 31,   0),   S(  9,   2),
            S(-14, -12),   S( -7,  -8),   S( -5, -12),   S( 12,  -6),   S(-22,  19),   S( -2, -13),   S( 43,  54),   S( 20,  15),
            S(  8,   9),   S( 13, -18),   S(  3, -22),   S(-14,  -8),   S( 33, -10),   S( 19, -14),   S( 17,  10),   S(  4, -40),
            S(  8,   6),   S( 22,  -2),   S( 21,  19),   S( 21, -23),   S(  3,  13),   S(  8,   5),   S( 37,  21),   S( -9,  45),
            S( 30,  30),   S( 36,  32),   S( 12,  31),   S( 10,   0),   S( -3, -13),   S( 13,  -4),   S( 45,  37),   S( 15,  22),
            S( 74,  18),   S( 85,  -9),   S( 76,   4),   S( 57, -19),   S( 65, -14),   S( 44,  21),   S( 19,   9),   S( 29,   9),
            S( 65,  13),   S( 10,   1),   S( 28,   8),   S( 16,  27),   S( 36,  -5),   S( 32,  47),   S( 15, -17),   S( 29,   8),

            /* rooks: bucket 11 */
            S(-55, -41),   S(-35, -44),   S( -8, -26),   S(-44, -74),   S( 11, -37),   S( -7,  -4),   S(-53, -47),   S(-46, -23),
            S(-28, -24),   S( -3, -59),   S(-12, -27),   S( -4, -35),   S(-26, -50),   S(-18, -16),   S(  8, -23),   S(-31, -16),
            S(  0, -30),   S( 14, -31),   S( 21, -30),   S( 19, -11),   S(  1,  -6),   S( 23,  -5),   S(-26,   3),   S(-14, -58),
            S(-10,  27),   S(  7,  -9),   S( -9,   3),   S( 15,   1),   S(-19, -33),   S( -4,   6),   S( 26,   4),   S( 18, -39),
            S( -8,  29),   S( 12, -37),   S( 37, -17),   S( 21,  -9),   S( 31, -10),   S( 17, -15),   S( -4,  -4),   S(-12, -21),
            S( 40,  13),   S( 59,  -7),   S( 30, -14),   S( 41,  22),   S( 63,  22),   S( 16,   2),   S(-15,  -1),   S( 12,  13),
            S( 67,  26),   S( 53,   6),   S( 76, -30),   S( 79,  -9),   S( 61,   2),   S( 43,  23),   S( 39,  42),   S( 60,  -6),
            S( 56,  38),   S( 13,  14),   S( 18,   4),   S( 27, -24),   S( -3,  -8),   S( 31,  18),   S( 21,   2),   S( 46,  20),

            /* rooks: bucket 12 */
            S( 13,   7),   S(-16, -76),   S(-11, -35),   S(-20, -47),   S(  4,  -2),   S( -3, -30),   S(-24, -73),   S(-30, -56),
            S( 12,  14),   S(  8,  13),   S(-11, -34),   S(  3,  -2),   S( -8,   3),   S( -2,  -7),   S( -8, -18),   S(-18, -35),
            S(  3,   5),   S(  5,  -9),   S(-23, -55),   S(-32, -51),   S( -1, -19),   S(  7, -10),   S(  3,  -4),   S(  4,   9),
            S(-15, -11),   S( -3, -19),   S( -3,   1),   S(  9,  -5),   S(  5,   5),   S( -9, -29),   S( -8, -21),   S(  4, -26),
            S( -8, -25),   S(  1, -16),   S(  7,   4),   S(  1, -14),   S( -3, -27),   S( 12, -22),   S( -3, -16),   S(  3, -24),
            S( -3,  34),   S( -8, -19),   S( 33,  46),   S(  8,   0),   S(-15, -33),   S(-11, -41),   S( -4, -32),   S(  0,  -4),
            S(-22, -51),   S(-15, -54),   S(-23, -88),   S(  1, -15),   S( 10,  -8),   S( -3, -33),   S(-16, -35),   S(  7,  -4),
            S(-11, -49),   S(  6,  22),   S(  4,  15),   S(-10, -27),   S( -8, -35),   S( -4, -24),   S(-17, -40),   S(  9, -11),

            /* rooks: bucket 13 */
            S( -8, -28),   S(  2,  -6),   S(  1, -20),   S(  4,   8),   S(  5,  -7),   S(-22, -55),   S(  4, -27),   S(-19, -17),
            S(  3,  -1),   S(-11, -21),   S( -8,   0),   S(-10, -13),   S( -8,   0),   S(  2, -21),   S(  5,  10),   S(  0, -30),
            S(-18, -40),   S( -5, -28),   S( -2, -36),   S(-13, -56),   S(  6,  21),   S( 11,  19),   S(  9,   4),   S(  0, -24),
            S( -5, -46),   S( 16,  33),   S(-18, -42),   S(  8,  20),   S(  3, -12),   S(-13, -31),   S( -1, -25),   S(  2, -10),
            S( 12, -13),   S(  4, -39),   S( 13,   6),   S(-23, -19),   S(-30, -38),   S(-10, -33),   S(-13, -47),   S( 11, -11),
            S(-17, -38),   S( 14, -29),   S( -7, -11),   S(  3,   4),   S( -3, -49),   S( 13,   9),   S(  5, -28),   S( 10,   2),
            S(  8,  -7),   S( 21,  37),   S( 22,  31),   S( 13,  -4),   S( 11, -38),   S( 31,   8),   S( 12, -11),   S(  5, -41),
            S(-32, -155),  S(-22, -76),   S( 11,   9),   S(  1,   1),   S( -3,   1),   S( -5, -40),   S(-17, -31),   S(  3, -13),

            /* rooks: bucket 14 */
            S( -5, -32),   S(-16, -61),   S(  9,  -5),   S(  2, -38),   S(  3, -32),   S(  6,   3),   S(  2, -50),   S(  2, -30),
            S(-22, -39),   S(-14, -74),   S( -4,  16),   S( -8, -17),   S(-13,   6),   S( -8, -60),   S(  2,  35),   S( -2, -14),
            S(  5, -25),   S( -2,   4),   S( -6,  -1),   S(-13,   7),   S( -5, -19),   S( -9, -16),   S(  4,  33),   S(  6,  -3),
            S( 12,  14),   S(-16, -42),   S( -3, -26),   S(-13,   1),   S(  8,  11),   S(  1,  -1),   S( -3, -41),   S(  2, -28),
            S(  1, -35),   S( -1, -39),   S(-14, -41),   S(-15, -29),   S( -7, -38),   S( -8, -28),   S( -5, -22),   S(  4, -23),
            S( 15,  -2),   S(  0,   1),   S(-13, -28),   S( 15,  -7),   S(  2, -44),   S( -3,  -5),   S(  7,  29),   S( -5,  -9),
            S( 31,  25),   S(  2, -37),   S( -4, -33),   S(  1, -17),   S(  9, -44),   S(  1, -25),   S(  8,   9),   S(  8,   5),
            S( -6, -26),   S( 12, -16),   S( -9, -45),   S(  9,  -1),   S(-12, -38),   S(  3,   6),   S(  7,   2),   S( -8,  -6),

            /* rooks: bucket 15 */
            S( 11, -35),   S(  1, -36),   S( -5, -18),   S(-13, -36),   S( -9, -47),   S( -5, -30),   S(-27, -63),   S(  8,  31),
            S( -9, -28),   S(-20, -30),   S(  0,   3),   S( -8, -34),   S( -3,  -1),   S(  3,  -8),   S(-19, -46),   S(  9,  12),
            S( -6,  -6),   S(-21, -35),   S(-12, -28),   S( -2,  -6),   S(  2, -45),   S( -1, -12),   S( -3, -13),   S( -6, -23),
            S( -2, -49),   S( -2, -33),   S(-10,  -9),   S( -7, -25),   S( -2,  -3),   S( -7, -40),   S(  0,  -1),   S( -5,   7),
            S(  0,  -6),   S( -4,  -3),   S(  8, -10),   S( -7, -23),   S( -2, -11),   S(  5,   0),   S( -2,  -6),   S(  1,  15),
            S( -1,  -6),   S(  4, -12),   S( -1, -35),   S(  1, -16),   S( -3, -12),   S( -4,   6),   S(  7, -28),   S(-15, -22),
            S(  4,  28),   S(  6,  -5),   S( 15, -25),   S( -3, -27),   S(  2,  -4),   S( -2,   5),   S(  0, -18),   S( -5,  -4),
            S(  8, -16),   S( -8, -19),   S(  1, -34),   S( -3, -12),   S( -7,  -2),   S( -7, -39),   S(  1, -23),   S(  3,  -1),

            /* queens: bucket 0 */
            S(-36, -10),   S(-39, -53),   S( 50, -68),   S( 64, -63),   S( 30, -16),   S( 32, -15),   S( 62,   1),   S( 22,  15),
            S(-14,  -2),   S( 39, -40),   S( 47, -32),   S( 32,  15),   S( 30,  20),   S( 37, -17),   S( 11,  68),   S( 31,  26),
            S( 25,   3),   S( 47,   2),   S( 39,  19),   S( 19,  47),   S( 28,   8),   S( 21,   9),   S(  8,  37),   S( 23,  57),
            S( 19,  17),   S( 31,  80),   S( 17,  52),   S( 15,  73),   S( 32,  40),   S( 12,  61),   S( 10,  44),   S( 14,  41),
            S( 46,  35),   S( 36,  35),   S( 26,  37),   S( 17,  84),   S( 15,  36),   S( -1,  17),   S( 22,  49),   S( 37,  -9),
            S(  1,  74),   S( 30,  69),   S( 18,  31),   S(  9,  12),   S( 40,  10),   S( -6,  66),   S( 12,  30),   S( 22, -22),
            S( 52,  50),   S( 34,  65),   S( 43,  76),   S( 38,  67),   S( 31,  33),   S(  0,  14),   S( 31,  56),   S( 48,  51),
            S( 32,  29),   S( 18,  29),   S( 53,  50),   S( 44,  51),   S( 40,  32),   S(-41,  20),   S( 47,  29),   S( 28,  22),

            /* queens: bucket 1 */
            S( -8, -18),   S(-89, -22),   S(-60, -21),   S(-28, -68),   S(-12, -28),   S(-23, -50),   S(  4, -63),   S(-11,  32),
            S(-26, -19),   S( -8, -16),   S(  5, -11),   S( -7,  36),   S( -4,   3),   S( -7,  27),   S( 22, -62),   S( -1,  18),
            S(-36,  56),   S( 12, -51),   S( 16,  23),   S( -4,  -8),   S( -2,  34),   S(-13,  35),   S( 19,   3),   S( 27,  38),
            S( -2, -32),   S( -6,  23),   S(-16,  44),   S( 16,  50),   S( -5,  41),   S( 13,  10),   S(  8, -15),   S( 21, -26),
            S( 31,  -7),   S(  4,  37),   S(  0,  86),   S( -6,  50),   S( -5,  34),   S(  3,  38),   S(-14,  11),   S(-12,  21),
            S( 21,   6),   S( 12,  60),   S( 29,  98),   S(-15,  72),   S(-26,  55),   S(-21,  52),   S( 24,  39),   S( 29,  33),
            S( -6,  49),   S( 30,  49),   S(-32,  55),   S(-24,  67),   S(-17,  62),   S( 14,  36),   S(  2,  58),   S( -2,  50),
            S( -6, -15),   S(  6,  15),   S( 15,  33),   S(-40,  24),   S(-29,  31),   S( 37,  48),   S(-19,  25),   S(-25,  14),

            /* queens: bucket 2 */
            S( 13,  28),   S( 20, -57),   S( 14, -44),   S( -3, -26),   S( -7, -18),   S(-54, -22),   S(-11, -46),   S( 25,   6),
            S( 13,  20),   S( 26,  22),   S( 19, -17),   S( 19, -12),   S( 13, -13),   S( 11, -32),   S(  7,   4),   S( 47, -46),
            S(  9,  33),   S( 16, -12),   S(  6,  59),   S( 13,  21),   S( 12,  62),   S( 18,  53),   S(  8,  14),   S( 24,  -9),
            S(  5,  13),   S(  0,  43),   S(  2,  30),   S( 18,  54),   S(-15,  95),   S(  2,  90),   S( 11,  35),   S( -4,  51),
            S( 12,  17),   S( -1,  51),   S(-15,  81),   S(-24,  97),   S(-41, 119),   S(-26, 116),   S(-13, 106),   S(-11, 116),
            S( 34,   6),   S(  8,  46),   S(-16,  75),   S( -7,  52),   S(-31,  94),   S(-38, 115),   S(  2, 116),   S( 10,  77),
            S(-30,  62),   S(-42,  90),   S(-23,  86),   S(  6,  98),   S(  0,  97),   S( 37,  45),   S(-37,  55),   S(-32,  60),
            S(-72, 129),   S(  0,  48),   S( 32,  32),   S( 46,  37),   S( 44,  57),   S( 21,  43),   S( 26,  54),   S(-15,  44),

            /* queens: bucket 3 */
            S( 93,  80),   S( 56,  96),   S( 52, 107),   S( 42,  92),   S( 73,  33),   S( 33,  41),   S( 10,  46),   S( 41,  31),
            S( 67, 140),   S( 65, 105),   S( 48, 114),   S( 51,  98),   S( 49,  86),   S( 59,  73),   S( 60,  24),   S( 43,  36),
            S( 60, 103),   S( 50, 106),   S( 61,  88),   S( 53,  78),   S( 57, 107),   S( 58, 114),   S( 62, 112),   S( 60,  82),
            S( 40, 136),   S( 52,  97),   S( 49, 108),   S( 55, 103),   S( 43, 107),   S( 47, 133),   S( 64, 100),   S( 38, 131),
            S( 60, 107),   S( 40, 139),   S( 48, 113),   S( 24, 129),   S( 28, 142),   S( 25, 137),   S( 34, 180),   S( 40, 168),
            S( 56, 141),   S( 55, 131),   S( 45, 113),   S( 16, 142),   S( 41, 145),   S( 63, 128),   S( 61, 154),   S( 22, 225),
            S( 41, 139),   S( 54, 116),   S( 70, 109),   S( 85, 109),   S( 57, 119),   S( 47, 124),   S( 64, 166),   S(135,  95),
            S( 91,  99),   S( 94, 111),   S( 68, 114),   S( 72, 115),   S( 12, 160),   S(103,  82),   S(134,  76),   S(156,  58),

            /* queens: bucket 4 */
            S( 22,  -8),   S(-15, -13),   S(-10, -13),   S( -7, -24),   S( -6,   2),   S(  7,  -5),   S(-78, -28),   S(-24,  -7),
            S(-31, -13),   S(-25,  -6),   S( 12, -10),   S(-19,   0),   S( 31,   4),   S( 35, -34),   S( 16,  -4),   S(-20, -16),
            S(  1,  11),   S(  2,   3),   S( 33,  21),   S(  8,  37),   S(-28,   8),   S( 37,  -4),   S(  2, -20),   S(-40, -15),
            S( -9,   7),   S( 13,  21),   S(  5,  28),   S(-18,  46),   S( 46,  30),   S(  1,   7),   S( 13,  17),   S(-12,  -1),
            S(-30, -10),   S( 37,   8),   S( 33,  30),   S( 43,  42),   S( 24,  31),   S(  1,   3),   S(-25,  15),   S( 16,  -8),
            S( -7,  10),   S( 37,  26),   S( 62,  87),   S( 80,  86),   S( 36,  26),   S(-10,   9),   S(-28, -22),   S(-27,  -7),
            S(-33, -26),   S(-20,  34),   S( 16,  31),   S( 20,  26),   S( 20,  24),   S(-16,  -6),   S(-10, -35),   S(-38, -42),
            S( -2,  -6),   S( -2,  14),   S( 42,  45),   S( 12,  38),   S(-20, -11),   S(-12, -11),   S( -8, -14),   S(  7,  -2),

            /* queens: bucket 5 */
            S(-34, -10),   S(-25, -30),   S(-30, -36),   S(-39, -39),   S(-51, -39),   S( 41,  10),   S(-32, -21),   S(  2,   6),
            S(-68, -28),   S(-41,   3),   S(-92, -29),   S(-62,  -9),   S(-27, -29),   S(-39, -20),   S(-50, -20),   S(-33,  -7),
            S(-52, -21),   S(-53,  -1),   S(-69, -23),   S(-26,  16),   S( 34,  50),   S(-32, -25),   S( -4,  11),   S( -5,  24),
            S(-86, -31),   S(-72, -10),   S(  2,  43),   S( -5,  73),   S( -8,  24),   S(  9,  20),   S(-14, -16),   S( -7,  41),
            S(-50, -14),   S(-17,  13),   S( 11,  56),   S( 25,  42),   S( 39,  41),   S(  3,  13),   S( 33,  28),   S(-12, -19),
            S(-48,  17),   S( -8,  29),   S( -6,  34),   S( 18,  41),   S( 52,  68),   S(  7,  10),   S(  6,  16),   S( -6,  -1),
            S(-15,   2),   S( -7,  10),   S(  8,  84),   S(  4,  54),   S(  8,  35),   S( 28,  41),   S( -1,   3),   S(-31, -40),
            S(-13,   3),   S( 23,  15),   S( 14,  22),   S( -1,  49),   S( 46,  91),   S( 17,  39),   S(  6, -16),   S(-20, -25),

            /* queens: bucket 6 */
            S(-49,   1),   S(-51, -41),   S(-67, -31),   S(-69, -69),   S(-94, -43),   S(-61, -52),   S(-42, -43),   S(-30,  10),
            S(-69, -23),   S(-51, -19),   S(-20, -39),   S(-41,  11),   S(-74,  -9),   S(-115, -20),  S(-73, -12),   S( -6,  32),
            S( -9,  21),   S( -9,  34),   S(-47,  45),   S(-99,  79),   S(-29,  45),   S(-53,  -5),   S(-63, -39),   S( 31,  11),
            S(-43,  26),   S(-63,  11),   S(-36,  65),   S(-37,  72),   S(-12,  78),   S(  2,  43),   S( -5,  42),   S(-10,   6),
            S(-36,  57),   S( 16,  36),   S(-28,  60),   S( 15,  52),   S( 23,  60),   S( 57,  42),   S( 29,  36),   S(-26,  32),
            S(-65,   7),   S( 15,  40),   S( 32,  43),   S( 33,  49),   S( -6,  75),   S( 78,  85),   S(-16,   1),   S(  9,  31),
            S( -4,   0),   S( -9,  -7),   S(-21,  47),   S( 14,  54),   S( 45,  77),   S( 30,  56),   S( -9,  39),   S(-23,  13),
            S( 18,  31),   S( 15,   8),   S( 14,  31),   S( -3,  19),   S( 50,  60),   S( 39,  50),   S(-29,   7),   S( 11,  21),

            /* queens: bucket 7 */
            S(-29, -11),   S(-16,  -7),   S(-51,  39),   S(-34,   4),   S(-55,  12),   S(-44, -58),   S(-33,  -9),   S(-37, -37),
            S(-10, -31),   S(-66,   4),   S(-29,  31),   S( -2,  26),   S(-26,  29),   S(-30,  12),   S(-67,  24),   S(-47, -26),
            S(-68,  18),   S(-61,  22),   S( -8,  34),   S(-19,  32),   S( 33,  14),   S(  6,  36),   S(  8,  16),   S(-16,  15),
            S(-67,  12),   S( 36,   8),   S(-32,  47),   S(-11,  51),   S( 45,  16),   S( 27,  25),   S(-16,  78),   S(  1,   9),
            S(-46,  28),   S(-43,  49),   S( 23,  30),   S( 24,   6),   S( 70,  -8),   S( 85,  12),   S( 50,  12),   S( 39, -10),
            S(-15,  62),   S(-13,  23),   S(-23,   8),   S( 16,  -5),   S( 58,  56),   S( 70,  46),   S( 93,  15),   S( 26,  55),
            S( 13,  20),   S(-20,  56),   S(  7,   3),   S( 35,  34),   S( 50,  26),   S( 44,  60),   S( 58,  34),   S( 77,  52),
            S(  1,   8),   S( 37,  15),   S( 49,  42),   S( 37,  33),   S( 40,  24),   S( 34,  42),   S( 39,   1),   S( 38,  56),

            /* queens: bucket 8 */
            S(-11, -14),   S( -7,   6),   S(-15,  -5),   S(-11, -10),   S(-14, -11),   S(  1, -17),   S(-19, -18),   S(-17, -19),
            S(-10, -11),   S(-15, -19),   S(-10,  -1),   S(-21, -10),   S( 10,  14),   S( -9,  -9),   S(-15, -38),   S(-18, -29),
            S(  1,   3),   S(-15, -12),   S(-10,  -6),   S(  2, -13),   S(-20,  -6),   S(-24, -25),   S( -2,  -3),   S( -6, -15),
            S( -1,   7),   S( 18,  22),   S( -2,  14),   S( 11,  15),   S( 12,  22),   S( -9,  -6),   S(  3,   4),   S(  1, -10),
            S( 10,  25),   S(  2,  54),   S( 20,  45),   S( 11,  34),   S( 21,  37),   S( -4, -13),   S( -4,  -6),   S(-15, -27),
            S( 14,  23),   S( 19,   8),   S(-15,  19),   S( 15,  36),   S( -5,  -5),   S( -6,   4),   S(  4,  12),   S(  2,  13),
            S( -7,  -9),   S( -2,  -8),   S( 15,  42),   S( 16,  29),   S(-17,   9),   S( -5,  12),   S( -4, -10),   S( -1,  -7),
            S(-25, -48),   S( 18,  16),   S( -1, -32),   S(  0,   0),   S(-14, -40),   S(  5,  -3),   S(  4,  -4),   S( -4,  -2),

            /* queens: bucket 9 */
            S(  8,  11),   S(-16, -25),   S( -5, -12),   S(-33, -38),   S(-25, -41),   S(-18, -29),   S( -6, -17),   S(-26, -43),
            S( -4, -11),   S(-14, -15),   S(-17, -17),   S(-11, -14),   S( -7,  -8),   S(-26, -32),   S(  8,   6),   S(  4,  10),
            S(  2,   2),   S(  3,  20),   S( -9,  29),   S( -9, -15),   S( -9, -17),   S(-12,   8),   S(  3,   2),   S( -3, -10),
            S( -3,  -2),   S(-18,  -6),   S( 21,  58),   S( 11,  35),   S( 14,  22),   S( 12,  -6),   S(-15, -25),   S( -9,  -3),
            S(-14, -19),   S( 16,  37),   S( 15,  50),   S( 27,  61),   S( 21,  48),   S(  9,  17),   S( 14,  29),   S( -8, -17),
            S(-13, -12),   S(-14, -10),   S( -4,  26),   S( 13,  23),   S(-17,  -9),   S( -2,  -2),   S(-17, -18),   S(-19, -17),
            S(  4,  -6),   S(  6,  15),   S(-18,  18),   S(  0,   9),   S( -7,  -8),   S( 13,   9),   S( -3,  -6),   S(-16, -36),
            S( -7, -10),   S(  7, -31),   S( 17,  -7),   S(  1,   8),   S( 24,  14),   S(-24, -13),   S( 16,   2),   S(  2,  -4),

            /* queens: bucket 10 */
            S( -5, -12),   S( -5,  -3),   S(-11, -11),   S(-33, -28),   S(-18, -18),   S(  0, -14),   S(  7,   9),   S( -2,   1),
            S(-16, -30),   S(  8,  -6),   S( -8, -18),   S(-16,   7),   S(-16,  -3),   S(-11, -13),   S(  3,  -5),   S(-24, -31),
            S( -1, -18),   S( -5,  -5),   S(-26, -19),   S( -8,   6),   S(-21, -20),   S( -9,   4),   S( -8,  -7),   S( -1,   1),
            S( -4,  -3),   S(-19, -31),   S(-16, -17),   S(  1,  24),   S( 39,  47),   S(-21,   1),   S(  1,   7),   S( -4,   0),
            S(-12,  -7),   S( -9, -22),   S( -8, -13),   S( -8,  22),   S( -4,  -6),   S(  1,  14),   S( 12,  -1),   S( -3,   3),
            S(-11,  -2),   S(-33, -45),   S(-32, -34),   S( -1,  17),   S( 12,  34),   S( -4,  16),   S(  5,  -2),   S( -9, -13),
            S(  1,  -3),   S(-23, -34),   S( -4,   0),   S(-10,  -7),   S(  6,  15),   S( 11,  15),   S( -9, -19),   S( -4,   2),
            S( 11,   1),   S(  0, -18),   S( 15,   8),   S( -9, -32),   S(  7,   6),   S(  6,  19),   S(  5,  10),   S(  1,  -1),

            /* queens: bucket 11 */
            S(-10, -13),   S(-10, -22),   S(-24, -22),   S(-23, -25),   S(-10, -20),   S(-26, -30),   S(-12,  -9),   S(-18, -28),
            S( -4, -22),   S( -7,  -7),   S(-38, -34),   S(-21, -21),   S( -7,  -1),   S(-16,   2),   S( -7, -13),   S(-11,  -8),
            S(-31, -32),   S( -1, -25),   S( 10, -23),   S( -5, -36),   S( -5,  16),   S(-13,   1),   S( -4,  17),   S(-14, -10),
            S(-14, -30),   S(-32, -33),   S(-12, -37),   S( 31,  73),   S( 25,  21),   S( 19,  46),   S( 32,  29),   S( -7,   6),
            S( -8,  -6),   S( -4, -11),   S(-22, -23),   S( 20,  27),   S( 35,  38),   S( 36,  70),   S( 21,  34),   S(  3,   3),
            S(-22, -43),   S( -3,  -2),   S(  5,  17),   S( 12,  12),   S( 31,  20),   S( 56,  44),   S( -1,   0),   S( -3,   5),
            S(-12, -14),   S(-13, -19),   S( 24,  18),   S( -1,  18),   S(  8,  11),   S( 36,  56),   S( 49,  47),   S(  4,   1),
            S(-15, -24),   S(  2,  -4),   S( -3, -17),   S( -2,  -4),   S( 11,  16),   S(-16, -24),   S( 14,   4),   S(-14, -32),

            /* queens: bucket 12 */
            S(  9,   0),   S(  3,   9),   S( -4,  -5),   S( -5,   0),   S(-12, -13),   S( -4,  -9),   S(  0,  -7),   S( -6, -17),
            S( -4,  -5),   S( -5,  -9),   S( -2,   6),   S( -1,   2),   S( -6,  -4),   S( -6,  -1),   S( -2, -12),   S( -7, -16),
            S( -3,  -3),   S( -3,  -4),   S( 12,  13),   S(  2,   1),   S( -1,  -6),   S(-11, -23),   S(-12, -25),   S( -2,   4),
            S(  8,  19),   S(  2,  10),   S( 11,  24),   S(  4,  17),   S(  6,  24),   S( -4, -14),   S(  0,  -7),   S(  2,  -5),
            S(  3,   2),   S(  3,  23),   S( 29,  54),   S( 11,  35),   S( 10,  28),   S(  6,  18),   S( -7, -10),   S(  1, -14),
            S(  4,  15),   S( 13,  23),   S( 32,  42),   S( 10,  19),   S(  5,  11),   S(  1,   6),   S( 15,   6),   S( -7, -21),
            S(  1,  -5),   S( 20,  34),   S( 25,  25),   S(  9,   4),   S( 12,  23),   S(  4,   2),   S(  3,  -7),   S( -7,  -9),
            S(-14, -36),   S(-14, -26),   S(-23, -38),   S(  2,  -4),   S(  2, -19),   S( -7, -15),   S( -5,   1),   S(-14, -17),

            /* queens: bucket 13 */
            S( -4,  -5),   S(-10, -18),   S( -4, -15),   S(  1,   1),   S( -2,  -7),   S( -1,   1),   S( -7,  -5),   S( -5, -13),
            S(  2,   8),   S(  3,  10),   S(  9,  17),   S( -3,  -1),   S( -7,  -8),   S( -2,   7),   S( -6,  -8),   S(-11, -23),
            S( -5, -10),   S(  5,  14),   S(-10,  -8),   S(  8,  21),   S(  5,  12),   S( -5,  -7),   S( -7, -20),   S(-12, -18),
            S( -4,  -9),   S(  4,  10),   S( 16,  23),   S( 18,  13),   S( 11,  13),   S( -1,   7),   S(-14, -30),   S( -4,  -4),
            S( -2,   4),   S( -4,  -1),   S( -3,  32),   S( 27,  55),   S( 36,  60),   S( -8,  -9),   S( -7, -13),   S( -5, -13),
            S( -8, -12),   S( -5,   5),   S( 47,  96),   S( 22,  42),   S(-11,  -4),   S(  0,   7),   S(  1,  -7),   S( -6, -15),
            S( -6, -15),   S( 11,   3),   S(  2,  12),   S(  9,  24),   S(  3,  24),   S(  8,  11),   S( -6, -13),   S(  1,   6),
            S(-16, -22),   S(  8,   5),   S(-14, -26),   S(-11, -14),   S( 18,  20),   S(  6,  10),   S(-11,  -1),   S( -4,   0),

            /* queens: bucket 14 */
            S(  1,   5),   S( -2,  -4),   S( -6, -14),   S( -4,  -5),   S(  2,   3),   S( -1,  -8),   S( -1,  -8),   S(-10, -19),
            S( -3, -10),   S(  4,  14),   S(  2,   1),   S( -2, -17),   S( -5,  -7),   S( -6, -22),   S(  3,  10),   S( -4, -11),
            S(  0,  -1),   S(-14, -12),   S( -5, -10),   S( -1,   2),   S(  9,   4),   S(  3,  -1),   S( 10,  16),   S(-11, -10),
            S( -7,  -2),   S( 11,   9),   S(-10,  -5),   S( 28,  38),   S(  3,   4),   S(  2,  12),   S( 11,  19),   S(  2,   0),
            S(  1,   1),   S( 12,  13),   S(-12,  -9),   S( 23,  44),   S( 15,  43),   S( 23,  39),   S(  4,  14),   S(  0,  -4),
            S(  0,  -3),   S(  8,  21),   S( 23,  39),   S(  5,   6),   S( 21,  29),   S( 10,  34),   S(  6,  11),   S( -8, -14),
            S(  7,  17),   S( 11,  19),   S(  7,  23),   S( 18,  29),   S(  9,   8),   S( 11,  35),   S(  4,  -1),   S(  4,   5),
            S( -3,  -2),   S( -6,  -5),   S(-12, -33),   S(  2,   4),   S(  3,   5),   S(  6,   4),   S(-13, -15),   S(-16, -20),

            /* queens: bucket 15 */
            S( -6, -11),   S(  0,  -3),   S( -3,  -6),   S( -3,  -2),   S( -2,  -1),   S( -5, -13),   S(-10, -16),   S(  3,  -4),
            S( -3,  -5),   S(  3,   2),   S( -8, -24),   S( -3,  -2),   S(  4,  15),   S( -3,  -6),   S( 17,  26),   S(  6,  10),
            S(  1,  -9),   S( -7, -20),   S( -1,   4),   S( -4,  -9),   S(  0,  -5),   S(  7,  19),   S( -4,  -8),   S(  4,   0),
            S(  0,  -9),   S( -3,  -4),   S( -9,  -4),   S(  6,   0),   S(  3,   4),   S(-15, -18),   S( 10,  16),   S(  7,   0),
            S(  1,   6),   S( -3,  -5),   S( -8, -13),   S( -1,   1),   S( 12,  26),   S( 10,  19),   S(-13, -22),   S( -5, -12),
            S( -1,   3),   S(  1,   0),   S( -3,   3),   S(  1,  -1),   S( 14,   9),   S( 13,  23),   S(  0,   4),   S(-11, -31),
            S( -6,  -7),   S(  0, -11),   S( 12,  14),   S( 11,  15),   S( 19,  26),   S( 27,  49),   S( 10,  16),   S(  4,   2),
            S( -1,  -1),   S( -2,  -6),   S(  2,   7),   S(  3,  -1),   S( -5, -19),   S(  1,   2),   S(  6,   2),   S( -3, -12),

            /* kings: bucket 0 */
            S( 75,   4),   S( 52,  54),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 46,  48),   S(138,  55),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 49,  17),   S(-11,  34),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 82,  45),   S( 69,  55),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-15,  38),   S( -9,  32),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 54,  61),   S( 63,  47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -9,  52),   S(-23,  19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  85),   S(-45,  63),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 52, -72),   S( 80, -32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -16),   S( 24,  22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 51, -20),   S( 25,  -4),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  8,  33),   S(-11,  34),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60,  10),   S( 25,  16),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  51),   S( 11,  48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 88,  23),   S( 49, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 48,  67),   S( 14,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39, -128),  S(-10, -47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-64, -101),  S(-105,  -8),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 12, -54),   S(-54, -43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-34, -29),   S(-60, -22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-32, -31),   S(-24, -35),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-62, -16),   S(-86,  15),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-31, -35),   S(-45, -100),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-98,   3),   S(-26, -83),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-69, -90),   S(-46, -32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -228),  S(-29, -91),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-53, -59),   S(  6, -53),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-62, -75),   S(-21, -97),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-13, -58),   S(-110, -21),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15, -119),  S(-85, -71),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-102, -11),  S(-51, -116),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-75, -49),   S( -8, -222),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9, -22),   S(-31,  11),   S(  8,  -5),   S( -2,  38),   S( 17,  12),   S( 41,   6),   S( 46, -18),   S( 54, -10),
            S(-16, -21),   S(-34,  -4),   S( -8, -12),   S(  4, -17),   S( 10,   5),   S(  1,   3),   S( 35, -11),   S( 22,  13),
            S(  7, -19),   S(  3, -23),   S( 29, -27),   S( 10, -18),   S( 25,   3),   S( 10,  39),   S( 10,  43),   S( 42,  12),
            S( 18, -18),   S( 38,   0),   S( 61, -24),   S( 39,  15),   S( 16,  58),   S(-11,  84),   S( 19,  79),   S( 63,  57),
            S( 99, -51),   S(129, -24),   S( 89,  13),   S( 71,  -7),   S( 55, 144),   S( -3, 150),   S( 26, 163),   S( 77, 122),
            S(-224, -58),  S(-107, -141), S(-15, -163),  S( 36,  40),   S( 90, 180),   S( 63, 182),   S( 94, 169),   S( 93, 158),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48,  20),   S(-48,  20),   S(-14,  -2),   S(-46,  48),   S(-20,   5),   S( 15,  -1),   S( 17, -10),   S( 12,  17),
            S(-58,  15),   S(-49,  12),   S(-34,   2),   S(-18,   0),   S(  6,   5),   S(-17,   0),   S(  1,  -7),   S(-22,  15),
            S(-36,  23),   S(-19,  20),   S(-20,   8),   S(  6, -11),   S(  9,  17),   S(-12,  11),   S(-28,  30),   S(-12,  31),
            S(-34,  45),   S( 12,  23),   S( -5,  21),   S( 22,  31),   S( 18,  31),   S(-23,  42),   S( -2,  45),   S( 29,  60),
            S(  7,  24),   S( 59,  -2),   S( 99, -23),   S( 81, -12),   S( 26,  31),   S(  7,  27),   S(-36,  62),   S( 22, 102),
            S(  6,  66),   S(-69,  -8),   S(-33, -93),   S( -3, -101),  S(-48, -69),   S( 25,  46),   S( 39, 204),   S( 96, 197),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49,  41),   S(-35,  18),   S(-18,   5),   S(-14,  24),   S(-31,  26),   S(-15,   5),   S( -4, -16),   S( -8,   9),
            S(-56,  32),   S(-44,  22),   S(-32,   3),   S(-30,  20),   S(-30,  17),   S(-38,   1),   S(-15, -16),   S(-43,   5),
            S(-39,  49),   S(-34,  52),   S( -9,  17),   S(-12,  15),   S(-16,  25),   S(-19,   3),   S(-28,   5),   S(-28,  11),
            S(-25,  91),   S(-38,  76),   S(-10,  48),   S( 11,  45),   S( -2,  36),   S(-21,  20),   S( 13,  19),   S( 20,  13),
            S(-29, 129),   S(-54, 132),   S(  2,  17),   S( 31, -31),   S( 94, -13),   S( 88,  -9),   S( 85, -42),   S( 40,   3),
            S( 12, 254),   S( 83, 165),   S(-14,  86),   S( 19, -85),   S(-38, -175),  S(-100, -149), S(-14, -82),   S( 19,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,  15),   S( -6,  11),   S(  9,   7),   S(  0,  31),   S( -5,  38),   S( 28,  20),   S( 15,  -7),   S(  9, -24),
            S( -4,  17),   S( -2,  19),   S(  1,   0),   S( -1,   5),   S( 11,  12),   S( 12,  -5),   S(  8, -15),   S(-23, -11),
            S( 11,  39),   S( -2,  55),   S( 12,  21),   S(  9,   2),   S( 30,  -7),   S( 25, -15),   S(  2, -19),   S(-11, -11),
            S(  7,  89),   S( -8, 106),   S( 14,  71),   S( 23,  44),   S( 39,   1),   S( 40, -25),   S( 22,   8),   S( 34, -17),
            S(  2, 154),   S(-13, 168),   S(-16, 175),   S( -4, 118),   S( 42,  56),   S( 89, -25),   S(123, -58),   S( 92, -42),
            S(117, 117),   S( 39, 249),   S( 52, 236),   S(-15, 217),   S(-24,  94),   S( -2, -173),  S(-65, -267),  S(-123, -194),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 52,   7),   S( 18,  -6),   S(-22,  -1),   S( -3,   7),   S(-21,  -5),   S(  2, -15),   S(-13,  -1),   S(-61,  39),
            S( 17,   4),   S( 14,   1),   S(-19,   5),   S(-22,   2),   S(-25, -19),   S(-17, -19),   S(-24, -25),   S(-48,  -4),
            S( 72, -15),   S(116, -37),   S( 44, -16),   S(-11, -13),   S(-71,  21),   S(-11,   8),   S(-53,  21),   S(-73,  35),
            S(-96, -65),   S(-36, -75),   S( 63, -55),   S(-43,  18),   S(-17,  21),   S(-65,  69),   S(-48,  62),   S(-42,  75),
            S( -2, -82),   S(-63, -117),  S(-23, -72),   S( 50, -17),   S( 89,  95),   S( -8,  96),   S( -1,  82),   S( 21,  80),
            S(  0, -70),   S(-10, -73),   S( 12, -82),   S(  4,  26),   S( 57, 119),   S( 65, 162),   S( 42, 175),   S( 41, 131),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-68,  43),   S(-57,  41),   S(-10,  15),   S( 41,   9),   S( 57,  -6),   S( -1,  -1),   S(-47,  13),   S(-58,  39),
            S(-105,  47),  S(-57,  39),   S(-22,  12),   S(  0,  18),   S(-25,  19),   S(-27,   2),   S(-66,   4),   S(-83,  29),
            S(-44,  34),   S(-30,  53),   S( 25,  32),   S( 22,  28),   S(-39,  53),   S(-56,  33),   S(-81,  36),   S(-55,  43),
            S(-39,  47),   S(-15,   9),   S(-68, -27),   S(-10,  -9),   S(-21,   8),   S(-53,  36),   S(-28,  42),   S(-41,  62),
            S( 16,  20),   S(  2, -35),   S( 24, -88),   S( 37, -84),   S( 16, -33),   S( 40,   6),   S(  2,  62),   S(-67, 117),
            S( 35,  39),   S( 12,  10),   S(-16, -36),   S( -8, -63),   S(-29, -45),   S( 58,  27),   S( 73, 135),   S( 26, 137),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-88,  38),   S(-56,  10),   S(-23,  -1),   S( -5,  13),   S(-19,  36),   S(  5,  11),   S(  4,   4),   S(  2,  20),
            S(-86,  25),   S(-61,  11),   S(-48,   4),   S( 29,   3),   S(-34,  30),   S(-18,   4),   S(-18,   8),   S(-10,   8),
            S(-50,  38),   S(-65,  40),   S(-37,  29),   S(-44,  43),   S( -3,  43),   S( 12,  20),   S( 17,  16),   S( -7,  20),
            S(-96,  99),   S(-30,  49),   S(-11,  29),   S(-15,  32),   S( -8, -29),   S(-33, -22),   S(-38,  18),   S( 18,  11),
            S(-28, 109),   S(-29,  64),   S( 24,  19),   S( -9, -32),   S( 11, -81),   S(-49, -64),   S(-32, -34),   S( 78,  -8),
            S(100,  70),   S( 75,  80),   S( 41,  31),   S( 85, -97),   S(  3, -106),  S(-50, -26),   S(-15, -32),   S( 79,  12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,   3),   S(-35, -29),   S(-19, -20),   S(-20,  18),   S( 31,  -3),   S( 68, -23),   S( 48, -30),   S( 61, -19),
            S(-59,   2),   S(-53,  -8),   S(-38, -20),   S(-27,  -4),   S( 12, -11),   S( 47, -33),   S( 32, -20),   S( 55, -21),
            S(-48,  28),   S(-80,  39),   S(-31,   2),   S(-45,  -1),   S(  2,  -1),   S( 30, -19),   S( 59, -13),   S( 63, -17),
            S(-37,  63),   S(-90,  80),   S(-34,  53),   S(-25,  47),   S( -6,   3),   S( 66, -63),   S(-12, -57),   S( 12, -99),
            S(-11,  70),   S(-72, 142),   S( -3, 131),   S(-11,  90),   S(  9,  19),   S(-12, -76),   S(-59, -154),  S( -2, -109),
            S(141,  83),   S( 85, 131),   S(123, 108),   S( 69,  98),   S( 41,  -7),   S(  4, -129),  S(-10, -72),   S(-14, -169),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2,  16),   S(-24,  17),   S( 33,   2),   S(-14, -30),   S(-28, -91),   S(-36, -25),   S(  0, -48),   S( 10, -50),
            S( -8, -59),   S(  0, -26),   S(-78, -57),   S(-76, -13),   S(-55, -54),   S( 74, -77),   S( 13, -58),   S(-11, -59),
            S( 26, -83),   S( 16, -72),   S(-10, -80),   S(-42, -60),   S( -8, -35),   S( 32, -35),   S(-62,  -6),   S(-15, -26),
            S( -9, -26),   S(-46, -19),   S( 21, -17),   S(  8,  24),   S( 21,  -6),   S( 18,  20),   S( -2,  29),   S(-14,  33),
            S( 34,   1),   S(  5,  -3),   S( -6,  13),   S( 31, 107),   S( 56, 111),   S( 46, 118),   S( 24, 103),   S(-18, 127),
            S( 37,  48),   S( 10,  43),   S( 24,  72),   S( 33,  99),   S( 28,  69),   S( 51, 187),   S( 42, 107),   S(-21, 112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 44,   0),   S( 37,  25),   S( 10,  18),   S(  5,   6),   S( -3,  -5),   S( 27, -25),   S( 43, -58),   S(-30, -21),
            S( 40, -46),   S( -5, -51),   S( -7, -55),   S( -6, -35),   S(-10, -30),   S(-49, -29),   S(-66, -19),   S( -2, -39),
            S(-19, -34),   S(-53, -28),   S(  7, -75),   S(-84, -40),   S( -7, -32),   S( 12, -49),   S(-54, -29),   S( 16, -31),
            S(-41,  -3),   S(-13, -59),   S(-21, -35),   S(-39, -16),   S( -5, -21),   S(  1, -28),   S( 10,   8),   S(-12,  18),
            S(  8,  14),   S(-14,  -8),   S( 13,  11),   S( 29,  14),   S( 15,  89),   S( 18,  50),   S(-28,  92),   S(-19,  63),
            S(-13,  74),   S( 26,  67),   S(  0,  83),   S( 20,  71),   S( 38, 110),   S(  4,  91),   S( 17, 112),   S(  3,  91),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39, -37),   S( -8, -49),   S(-45, -11),   S( -3,  -3),   S( 14,  -3),   S( 79,   6),   S( 12,  -1),   S( -4, -17),
            S(-25, -59),   S(-90, -35),   S(-32, -50),   S( 34, -49),   S(  7, -35),   S( -5, -26),   S( 13, -35),   S( 16, -45),
            S(-16, -37),   S(-89, -18),   S(-42, -46),   S(-39, -17),   S(-39, -48),   S( -2, -60),   S(-35, -58),   S( 63, -65),
            S(-22,   3),   S(-15,  13),   S(-41, -23),   S(-40, -54),   S(  1, -68),   S(-76, -13),   S(-30, -50),   S( 15, -41),
            S( 16,  15),   S( 27,  25),   S( -4,  20),   S(-22, -17),   S( 33,  29),   S( 14,  21),   S(-34,  -1),   S( 30,  -9),
            S( -4,  21),   S( -3,  53),   S( 34,  88),   S( -3,  82),   S( 29,  90),   S( -3,  19),   S(-20,  12),   S( 18,  61),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-65, -45),   S(-29, -46),   S(-20, -43),   S( -1, -11),   S( 12, -15),   S( 39,   6),   S( 39, -19),   S( 16,   6),
            S(-37, -59),   S(-42, -67),   S( -6, -83),   S(-18, -67),   S(-29, -39),   S( 12, -46),   S( -4, -40),   S( 55, -73),
            S(-41, -12),   S(-98,  -3),   S(-29, -12),   S( -8, -34),   S(-109, -26),  S(  8, -53),   S( 30, -115),  S( 81, -97),
            S(-30,  25),   S(-42,  27),   S( 10,  36),   S( 20,  32),   S( -6, -17),   S(-37, -20),   S(-34, -45),   S( 47, -110),
            S(-26,  27),   S(-27,  89),   S(-21,  72),   S( 16,  69),   S( 39,  91),   S( -7, -14),   S( -5,  40),   S( 20, -30),
            S( -3,  73),   S( 25,  50),   S( 20,  72),   S( 28,  91),   S( 17,  77),   S( 50, 124),   S(  8,  45),   S( 17,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -127),  S( 27, -50),   S(-24, -31),   S(  3, -16),   S(  2,  -4),   S(-29, -74),   S(  8, -45),   S( 11, -36),
            S( 41, -108),  S( 32, -51),   S(-21, -75),   S(-39, -59),   S( -8, -97),   S(-25, -64),   S( -8, -94),   S( -2, -60),
            S(-20, -42),   S( -6, -91),   S(-21, -75),   S(-14, -126),  S(-12, -43),   S(-17,  -7),   S(-31, -72),   S(-23, -64),
            S(-15, -37),   S( -4, -13),   S(-26, -26),   S( -2,   7),   S(  9,  43),   S( 14,  28),   S( -4,  24),   S( -3,  26),
            S(  6,  19),   S( 15, -11),   S( -3,  13),   S( 22,  77),   S( 39,  80),   S( 38, 111),   S( 17, 108),   S( 12,  65),
            S( -2,  19),   S(  0,  44),   S( 10,  53),   S(  9,  59),   S( 29, 126),   S( 34,  94),   S(-25, -56),   S(-17,  13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10, -99),   S( 38, -89),   S( 19,  31),   S( -3,  -5),   S( 10,  -9),   S(-27, -38),   S( -1, -76),   S( -8, -89),
            S( 34, -116),  S( 13, -96),   S( -6, -93),   S( 13, -13),   S(-28, -46),   S(  3, -90),   S(  6, -105),  S(  1, -108),
            S( 11, -86),   S(-22, -76),   S(  8, -78),   S( 22, -60),   S(-36, -29),   S( 14, -50),   S(  2, -84),   S( 52, -81),
            S( 24, -35),   S(-10, -83),   S(-12, -29),   S(  7,  18),   S( 15, -15),   S(-29,  13),   S(  9, -26),   S( -4, -23),
            S(  2,  48),   S(  7,  16),   S( -7,  -3),   S( 18,  27),   S( 38,  91),   S( 30, 100),   S(  9, 134),   S( -3,  58),
            S(  7,  84),   S( 31,  42),   S( -1,  27),   S(  9,  34),   S( 25,  64),   S(  3,  35),   S(  2,  63),   S( -3,  78),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24, -82),   S(  6, -69),   S(-19, -60),   S( -2,   1),   S( -9, -23),   S(  6,   6),   S(  7, -87),   S(-15, -25),
            S( 25, -121),  S(-38, -117),  S(  3, -94),   S(-15, -60),   S(-20, -48),   S( 34, -61),   S( 12, -64),   S( 10, -76),
            S( 33, -101),  S(-18, -91),   S(  3, -55),   S( -4, -41),   S(-33, -69),   S(  1, -98),   S(  0, -93),   S( 38, -62),
            S( -8,   8),   S( -8, -26),   S(-21,  10),   S(-18,   9),   S( 13, -53),   S(  8, -19),   S( 18, -17),   S( 26,  10),
            S(-12, -25),   S(  0,  18),   S(  4,  66),   S( -9,  31),   S( 16,  77),   S( -4,  22),   S( 23,  48),   S( 21,  32),
            S( -9,  53),   S( 12,  52),   S( 33,  74),   S( 24,  80),   S( 17,  62),   S(  4,  30),   S( 22,  72),   S( 23, 111),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -28),   S( 24, -35),   S(-11, -17),   S(-18, -43),   S( -8, -29),   S(-10, -32),   S(-15, -70),   S( 18, -94),
            S( -2, -104),  S(-18, -121),  S(-23, -118),  S( -8, -32),   S( -9, -21),   S(-12, -36),   S( 39, -82),   S(  8, -109),
            S(-15, -39),   S(-26, -73),   S(-47, -43),   S( 15, -50),   S(-39, -52),   S(  6, -66),   S(  5, -43),   S( -2, -27),
            S( 23, -15),   S(-35,   1),   S(-10,  49),   S(-21,  41),   S( 25,   8),   S( -3, -18),   S( -1,  15),   S(-10,  25),
            S(  8,  49),   S(  8,  69),   S(  0,  75),   S(  9,  51),   S( 16,  56),   S( 12,  74),   S( 18,  64),   S(  6,  49),
            S(-27,  33),   S(-14,  -5),   S(  8,  83),   S( 23,  59),   S( 25,  60),   S( 16,  55),   S(  4,  45),   S(  4, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-86, -36),   S(-27,   2),   S(-12, -18),   S(-21,  21),   S(-21,  -9),   S(-22,  -7),   S(  2, -35),   S(-83, -37),
            S(-18, -53),   S(-22,  -4),   S(-29, -45),   S(  0,  -6),   S( -5,  10),   S(-25, -17),   S(-27, -24),   S(-13, -40),
            S(-20, -37),   S( 20, -39),   S(  3,   9),   S( 35,  29),   S(  0,  -1),   S( 14,   2),   S(-20,  17),   S(-18, -52),
            S( -2,  25),   S( 62,  29),   S( 16,  59),   S( 54,  -3),   S( 20,  37),   S( 22,  12),   S( 42, -62),   S( -3, -27),
            S( 51,  77),   S( -4,  46),   S( 68,  80),   S( 56,  22),   S( 87,  14),   S( 30,   7),   S( 28, -15),   S( 18, -18),
            S(125, -46),   S(-25,  47),   S( 83,  34),   S( 70,  25),   S( 55,  42),   S(-34,  76),   S( 33,   4),   S(-29,   4),
            S(  2,  -9),   S(-12, -21),   S( 37,   9),   S( 60,  61),   S( 54,   3),   S( 33,   9),   S(-12,  27),   S(-23,  11),
            S(-106, -121), S( -5,  -8),   S(  7,  17),   S( 26,  -1),   S(  9,  51),   S(  2, -20),   S(-39,   0),   S(-21,   5),

            /* knights: bucket 1 */
            S(  7, -36),   S(-59,  10),   S(-42,  25),   S(-65,  36),   S(-25,  43),   S( -1, -52),   S(-30, -22),   S( -9, -10),
            S(-70,  20),   S(-46,  53),   S(-27,  30),   S( -2,  13),   S(-19,  22),   S(  3,  24),   S(-40,  29),   S(-32, -71),
            S(-36,  10),   S(  2,  -2),   S(-14,  21),   S(-16,  61),   S( -4,  36),   S( -1,   8),   S(-28,  15),   S(-28,  27),
            S(-16,  61),   S( 36,  28),   S( -9,  62),   S(-14,  58),   S(  4,  56),   S(-14,  62),   S( -5,  25),   S(-28,  51),
            S( 48, -14),   S( 15,   5),   S( 29,  50),   S(  7,  50),   S( 44,  34),   S( 13,  59),   S( -7,  61),   S(-21,  76),
            S( -9,  22),   S( 57, -22),   S( 88,   2),   S( 87,  53),   S( 62,  20),   S(-44,  76),   S( 23,  32),   S( 16,  29),
            S(  8,  -7),   S( 36, -22),   S( 50, -43),   S( 38,  47),   S(  2,  26),   S(  9,   5),   S(  0,  63),   S(-20,  17),
            S(-195, -37),  S( 29, -26),   S(-64, -87),   S(-34, -18),   S(-29,  20),   S( 50,  62),   S( 28,  39),   S(-65,   9),

            /* knights: bucket 2 */
            S(-68,  14),   S(-39,  26),   S(-19,   1),   S(-25,  20),   S(-19,  18),   S(-56, -10),   S(-28,  -3),   S(-20, -42),
            S(-37,  22),   S(-14,  37),   S(-33,  23),   S(-11,  23),   S(-29,  27),   S(-12,   2),   S(  6,  -1),   S(-38,  -6),
            S(-35,  35),   S(-23,  25),   S(-22,  31),   S(-27,  72),   S(-18,  53),   S(-21,  15),   S(-19,  10),   S(-13,   7),
            S(-15,  68),   S(-32,  54),   S(-20,  75),   S(-24,  86),   S(-22,  66),   S( -5,  62),   S(  7,  37),   S(-14,  47),
            S(-16,  62),   S( -5,  62),   S( -1,  70),   S( 14,  57),   S(-20,  80),   S( 24,  75),   S(-17,  71),   S( 34, -26),
            S(-39,  55),   S(-24,  55),   S(-26,  83),   S( 35,  27),   S( 36,  30),   S(129,   4),   S( 94,   9),   S(  2, -25),
            S( 20,  50),   S(-16,  45),   S( 50,   2),   S(  8,  16),   S(-11,  43),   S(-28, -13),   S( 16,  10),   S(  9, -24),
            S(-76,  28),   S( 17,  60),   S(-12,  76),   S(-25, -61),   S(-26, -22),   S(-52, -59),   S( 22,  -8),   S(-113, -79),

            /* knights: bucket 3 */
            S(-69,   7),   S(-14, -41),   S(  1, -12),   S(  2,  -4),   S(  0,  -3),   S( -5, -21),   S(-13, -29),   S(-21, -54),
            S(-15, -36),   S(  2,  10),   S(  8,  -5),   S(  4,   4),   S( -1,   2),   S( 31, -18),   S( 24, -25),   S( 22, -54),
            S(-10,  -4),   S( -7,   9),   S( 11,  23),   S(  3,  48),   S( 19,  31),   S(  6,  22),   S( 22,  -1),   S( 10,  -8),
            S(  6,   4),   S(  5,  43),   S( 20,  53),   S(  8,  61),   S( 17,  78),   S( 27,  68),   S( 43,  40),   S( 14,  39),
            S( -5,  40),   S( 17,  42),   S( 30,  60),   S( 37,  79),   S( 24,  81),   S( 38,  96),   S( -7,  90),   S( 51,  75),
            S(-42,  42),   S( -3,  57),   S( 31,  58),   S( 19,  79),   S( 61,  65),   S(132,  75),   S( 62,  86),   S( 21,  83),
            S( -1,  40),   S( -6,  49),   S(-34,  80),   S( 17,  68),   S( 43,  54),   S( 80,  46),   S( 15, -22),   S( 33,   5),
            S(-175,  45),  S(-46,  83),   S(-30,  63),   S( 73,  39),   S( 64,  35),   S(-49,  68),   S(-16, -42),   S(-65, -109),

            /* knights: bucket 4 */
            S( 13,  26),   S(-10, -13),   S(-46,  -4),   S(-28,   0),   S(-25,  39),   S(-33,  -3),   S( -3, -51),   S(-25, -15),
            S( 14,   5),   S( -5, -28),   S( -5,  -7),   S( -5,   8),   S(-24,  -7),   S( -6, -21),   S(-11,   2),   S(-76,   2),
            S(  8,  10),   S( 28, -19),   S( 57,   6),   S( 46,  15),   S( 48,  16),   S( 42, -32),   S(-22, -21),   S( -3, -42),
            S(-30, -32),   S(  7,   0),   S( 42,   7),   S( 62,  -2),   S( -5,  34),   S( -4,  25),   S(-34,  43),   S(  4,  11),
            S(  8, -31),   S( 45,   7),   S( 69,  11),   S( 42,  45),   S( 71,  15),   S( 31,  33),   S(  8,   5),   S(-17,  46),
            S( -6, -32),   S(-17, -43),   S( 31,  -1),   S( 56,  23),   S(  3,   4),   S(-21,  54),   S(-27,  26),   S( 26,  17),
            S(-15, -35),   S(-47, -34),   S(  1,   8),   S( 33,  22),   S( 24,   1),   S( 17,  52),   S(-11,  18),   S(-35, -23),
            S(  3,   1),   S(-10, -44),   S(-12, -44),   S(  4, -11),   S( 10,  11),   S( -2,  17),   S( -6,  21),   S(-19,  -9),

            /* knights: bucket 5 */
            S(  9,  35),   S( 23,  17),   S(-54,  40),   S(-26,  34),   S( -2,  36),   S( 15,  14),   S( -2,  28),   S( -7,   1),
            S( 29,  32),   S( 46,  21),   S(-11,   9),   S(-19,  11),   S( 54, -18),   S(-16,  -2),   S(-20,  42),   S(-26,  34),
            S(-40,  -3),   S(  6,   1),   S( 25,  15),   S( 17,  15),   S( 21,  18),   S(  8,  18),   S( 16,  17),   S(-31,  37),
            S( 27,   5),   S( 22, -16),   S( 70,   0),   S( 96, -19),   S(112,   3),   S( 78,  10),   S( -5,  18),   S(  2,  53),
            S( 58,  -3),   S( 33, -22),   S( 92, -19),   S(114, -19),   S(100, -20),   S( 43,  29),   S( 19,   6),   S( 38,  25),
            S(-12, -45),   S( 52, -19),   S( -1, -28),   S( 11,   5),   S( 26,   1),   S( 38, -12),   S(-17,  21),   S( 33,  33),
            S( 13,  18),   S(-42, -50),   S( 17, -59),   S(-17,  -3),   S( -2, -44),   S( 17,  -5),   S( 15,  46),   S(  4,  23),
            S(-17, -26),   S(-39, -102),  S( 19,  -8),   S(-27, -24),   S(  9,  14),   S( -1,  10),   S( 20,  26),   S( -3,  24),

            /* knights: bucket 6 */
            S(  1,  12),   S(-27,  35),   S(-38,  33),   S(-25,  31),   S(-62,  49),   S( -8,  53),   S(-18,  34),   S(-22, -26),
            S( 46,  -2),   S(  2,  40),   S(  0,  -6),   S( 35,  10),   S( 38,  -1),   S(-11,  22),   S( 18,  41),   S(-12,  72),
            S(-22,  22),   S( 31,   4),   S( 15,  30),   S( 57,  28),   S( 10,  35),   S(-42,  41),   S(  2,  36),   S(-25,  14),
            S( 25,  62),   S( 70,   5),   S( 34,  24),   S( 70,  21),   S( 87,  -4),   S( 73,  24),   S( 24,  -2),   S(-13,  40),
            S(-32,  47),   S( 31,   9),   S( 92,   9),   S(128,  -3),   S( 88, -21),   S( 52,  28),   S(140, -24),   S( -3,  15),
            S( 32,  24),   S( 35,   6),   S(  5,  28),   S( 11,  20),   S( 55, -11),   S( 45,  -4),   S( 14, -25),   S( 19, -40),
            S(  7,  44),   S(  1,  43),   S( 35,  37),   S(  3,   2),   S( 25,  14),   S( 47, -42),   S(  0, -16),   S( 23,  54),
            S( 11,  36),   S(  7,  30),   S( 21,  30),   S( -3,  31),   S( 19, -18),   S( -6,  -4),   S(  2,  -3),   S(-24, -53),

            /* knights: bucket 7 */
            S(  0, -17),   S(-58, -41),   S(-34, -14),   S(-63,  10),   S(-19, -26),   S(-61,  29),   S(-24, -11),   S(-28,  15),
            S(-48, -78),   S( 45, -23),   S(-37,   8),   S(-33,   7),   S( 12,  -7),   S(  6,  13),   S( -9,  16),   S(-83,  24),
            S( -8, -33),   S(-28, -22),   S( 20, -11),   S( 23,  19),   S( 50,   7),   S( 53,  11),   S(  8,  16),   S(-13,  44),
            S(-20,  -1),   S( 26,  -5),   S( 66, -12),   S( 73,   4),   S(116, -19),   S( 86,  14),   S( 65,  -3),   S( 72,  10),
            S( 13,   9),   S(-11,  16),   S( 11,  28),   S( 78,  -1),   S( 97,  12),   S(156, -19),   S(191,  -7),   S( 41, -11),
            S(-12,  -2),   S( 37,  21),   S(  5,  23),   S( 53,  17),   S( 63,   9),   S( 78,   7),   S( 21, -26),   S(-11, -40),
            S(-36,   6),   S( -5,  20),   S( 18, -11),   S( 24,  38),   S( 52,  40),   S( 42,   3),   S(-37, -47),   S(-13, -30),
            S(-39, -38),   S( -1,  24),   S(  8,   8),   S(-15,  21),   S( -8,  -2),   S( 24,  11),   S(  1, -16),   S( -8, -22),

            /* knights: bucket 8 */
            S( -7,  -5),   S( -5, -19),   S( -8,   1),   S(-15, -57),   S( -7, -16),   S(  1, -24),   S(  9,  14),   S( -5, -25),
            S(-10, -39),   S(-15, -45),   S( -6, -29),   S(-16,  -9),   S(-13,  36),   S( 11,   0),   S(  2,  -9),   S(  6,  11),
            S( -8, -41),   S(-18, -33),   S( -4, -75),   S( 20,  22),   S(-11, -15),   S( -4,   7),   S( -3, -16),   S(  4,  -8),
            S(-20, -71),   S(-10, -20),   S( 10,   0),   S( -9,  14),   S(-23, -29),   S(-16,  -7),   S(-19, -22),   S(-20, -51),
            S(-16, -62),   S( -5, -35),   S(  4, -18),   S(  6,  32),   S(-19, -16),   S(-22, -28),   S(  7, -12),   S(  4,  11),
            S( -5, -10),   S( 19,  -3),   S(  6,  13),   S( -6, -11),   S(  1,  18),   S( 10, -15),   S(-12, -44),   S( -4,  -1),
            S(  1,  20),   S( -4, -34),   S(-13, -25),   S( 11,  31),   S( -4, -24),   S( -1,  -8),   S( -1,   8),   S(  3,   2),
            S(  2,   7),   S( -1,   6),   S( -5,  -5),   S(  9,  22),   S(  1,  11),   S(  4,  17),   S(  2,   5),   S( -6,  -9),

            /* knights: bucket 9 */
            S(-23, -65),   S( -8, -21),   S( -5, -74),   S( -7, -44),   S(-29, -10),   S(-28,  -9),   S( 15,  24),   S(  6,   3),
            S(-10, -45),   S(-17, -50),   S( -4, -87),   S(-26, -49),   S(-23, -45),   S(-24, -67),   S(-11,   5),   S(-14,  21),
            S(-15, -47),   S(-11, -49),   S(-13, -42),   S(-24, -77),   S( -7,  -3),   S(  8,   2),   S(-17,  -1),   S(  6,  11),
            S(-15, -38),   S(-22, -63),   S( -5, -26),   S( -1, -41),   S(-21, -39),   S(-13, -10),   S(-14, -53),   S( -2,  -2),
            S(  8,  21),   S( -1, -18),   S( 10, -14),   S(  3, -26),   S(-32, -60),   S(-12,   1),   S( -4,  -2),   S(  2,  10),
            S(-21, -21),   S(-13, -42),   S( -6, -13),   S(  4,  -2),   S(  4,  22),   S(  0,  14),   S( -9, -16),   S( -3,   0),
            S(-14, -19),   S(  0,  -5),   S(-12,   8),   S(-29, -27),   S( 10,  20),   S( -3,  22),   S( -7,  25),   S(  2,  24),
            S(  1,  -5),   S(  2,  -2),   S(  5,  14),   S( -8, -14),   S(  2,  30),   S( -8,  -9),   S(  1,   7),   S( -3,  14),

            /* knights: bucket 10 */
            S( -7,  -5),   S( -6,   6),   S(  0,  13),   S(-17,  -9),   S( -4, -56),   S(  2, -60),   S( -4,  25),   S( -4,  16),
            S( -1, -15),   S( 18,   8),   S(-11, -32),   S( -8, -59),   S(  0, -18),   S(-16, -25),   S( -2,  10),   S(  6,  24),
            S( -2,  -9),   S(  6,   7),   S( -8,   6),   S( 10, -42),   S(-27, -58),   S(-23, -26),   S(-11, -13),   S( -7, -11),
            S( -9, -23),   S(-14, -17),   S(  6, -16),   S(  2, -12),   S(  3, -15),   S( -9,   3),   S(-11, -56),   S( -7,  -1),
            S(-14,  -1),   S( -3,  -3),   S( -1, -13),   S(-18, -22),   S( 18, -17),   S(  2, -42),   S( -1, -32),   S(-12, -22),
            S(  8,  31),   S(-20,  -1),   S(  6,  16),   S( -6,  29),   S(-10, -10),   S(-21,  -3),   S( -8,  10),   S(-22,  -2),
            S( -2,  -5),   S( -4,  12),   S(-14, -36),   S(  4, -48),   S( -7,  21),   S(-24, -31),   S( -2,  -2),   S(  2,   1),
            S(  2,   1),   S(  9,  29),   S( -4,  15),   S( -1, -11),   S( -7,   7),   S(  0,   7),   S(  5,  10),   S( -2,  -4),

            /* knights: bucket 11 */
            S( -9, -34),   S(-10,  -1),   S(  0, -22),   S( 18,  43),   S(-39, -12),   S( -9,   0),   S(-12, -25),   S( 15,  44),
            S( -1, -18),   S(-24, -42),   S(  1, -36),   S( 23, -19),   S( 20,  35),   S(  5, -14),   S(-16, -51),   S( -3, -22),
            S( -6, -30),   S(-19, -21),   S(  5,  -3),   S(  6,  -8),   S( -6,  38),   S( 20,  16),   S( -3, -34),   S( -9, -14),
            S(-18,  -9),   S( 12,  -3),   S( 12, -18),   S( -3,  15),   S( 22,   6),   S( 16, -16),   S(  1,  13),   S( -5,   0),
            S(-25,  -7),   S( -2, -37),   S(-24,  24),   S( -5,   1),   S( 46,  11),   S(  6,  21),   S(-18, -85),   S(-18, -33),
            S(  1,  -2),   S(  2, -27),   S( -1, -13),   S( 16,   1),   S(  6,  44),   S( -7, -23),   S( -5, -30),   S(  1,  15),
            S( -1,   2),   S(  8,  51),   S(-12, -28),   S( -3, -19),   S( 16,   2),   S( 11,  27),   S(  1,  -6),   S(-13,  -6),
            S( -2, -13),   S( -1,  -4),   S( -7, -26),   S(  8,  27),   S( -4,   3),   S(  0, -18),   S(  1,   0),   S(  0,  -2),

            /* knights: bucket 12 */
            S(-20, -63),   S(  0,  -3),   S( -4, -38),   S(  0,   6),   S( -3,  19),   S( -8, -25),   S(  3,  16),   S( -2,  -4),
            S( -1,  -3),   S(  0,   6),   S(  0,  -1),   S(  3,  14),   S( 11,   2),   S( -4,   1),   S(  2,  -2),   S( -1, -14),
            S( -5, -15),   S(-14, -31),   S(-10, -20),   S(-12, -24),   S(  7,  19),   S(  1,  27),   S(  0,  -8),   S( -4, -15),
            S(  2,   4),   S( -6, -41),   S(  2,  27),   S( 12,  31),   S(  5,  10),   S( -6,  -1),   S(  6,  12),   S(  3,  15),
            S( -5,  -8),   S( -1,  11),   S( -6, -36),   S( -8,   1),   S( 14,  36),   S( -5,   1),   S( -6, -12),   S(-17, -11),
            S(  0,   6),   S(  2,  -9),   S( -6, -16),   S( -8, -21),   S( -7, -13),   S( -7, -13),   S( 11,  10),   S(  0,  -3),
            S( -8, -20),   S( -8, -17),   S( -6,  -6),   S( -5, -16),   S( -1,  12),   S(-19, -20),   S( -1, -14),   S( -3,  -8),
            S(  0,   0),   S(  1,   0),   S(  1,   2),   S(  3,   0),   S(  3,   6),   S(  5,   6),   S( -2,  -7),   S(  0,   0),

            /* knights: bucket 13 */
            S( -5, -23),   S( -5, -11),   S( -8, -41),   S( -4,  -4),   S( -9, -23),   S( -1,  14),   S( -9, -15),   S(  2,   9),
            S(  1,  16),   S( -2,  -5),   S(  0,   6),   S( -4, -13),   S( -4,  -6),   S(  6,  32),   S(  0,   0),   S( -6, -18),
            S(  0,  -2),   S(  1,  -9),   S(  1,  -4),   S(  7,   6),   S( 12,  39),   S( -4,  20),   S(  5,  12),   S( -3,  -8),
            S(  4,  17),   S(  0,   0),   S( -2,  15),   S(  3,  42),   S( -6,  -8),   S(-10,  -7),   S(  1,   5),   S(  9,   7),
            S(  5,  34),   S( -7, -23),   S(  5,  41),   S(-17,   4),   S(-21,   4),   S(-10,  20),   S(-17, -28),   S( -5,  -7),
            S( -6, -14),   S(  5,  11),   S( -9,  -1),   S( -7,  -7),   S(  3,  33),   S( -2,   9),   S(  9,  31),   S( -3,  -6),
            S(  0,   3),   S(  5,   9),   S( -3,   6),   S(-10,  -7),   S( -6, -11),   S( -2,  -6),   S(  1,   3),   S(  1,   4),
            S(  3,   9),   S(  3,   8),   S( -2,  -6),   S( -3,  -4),   S( -1,   3),   S(  2,   3),   S( -3,  -5),   S(  0,   1),

            /* knights: bucket 14 */
            S(  1,  -4),   S( -2,   0),   S(  7,  21),   S( -2,   6),   S(  0, -36),   S( -9,  -6),   S(  3,   4),   S(  0,   4),
            S(  0,  -8),   S( -6, -16),   S( -3, -11),   S( -9,  -6),   S(  3, -13),   S( -2,  -1),   S(-11,  -4),   S(  5,  70),
            S( -4,  -8),   S( -2, -45),   S(  8,  27),   S( -8, -32),   S( -8,  -7),   S(  8,  21),   S(  0,   3),   S(  4,  21),
            S( -2, -14),   S( -3, -25),   S(-29, -34),   S( -5,  44),   S( -4,  14),   S( -4,   4),   S(  2,   9),   S( -3,  26),
            S(  2,   3),   S(-10, -30),   S( -8,   1),   S( -4,  11),   S(  5,  54),   S( -9,   4),   S(  5,  -2),   S(  5,  15),
            S( -1,   0),   S(  1,  -9),   S(  9,  14),   S(  1,   2),   S( -3,   5),   S(  5,  21),   S( -2,  15),   S( -4, -18),
            S( -1,   0),   S( -3, -12),   S( -2,   5),   S(  1,  -5),   S(  5,  16),   S( -3,  -9),   S( -2,   7),   S(  6,   6),
            S(  0,   2),   S( -1,  -1),   S( -2,  -6),   S( -1,  -1),   S(  0,   0),   S(  0,  -1),   S(  2,   6),   S(  0,   0),

            /* knights: bucket 15 */
            S( -4, -13),   S( -2,  -1),   S(  4,  22),   S( -6,  -9),   S( -8, -25),   S( -5, -22),   S(  0,  -9),   S( -1,  -1),
            S(  0, -12),   S(  8,  10),   S( -6, -14),   S( 14,  46),   S(  7,  18),   S(-12, -53),   S( -3,  -3),   S( -2,  -6),
            S(  3,   4),   S(-12, -33),   S( -5, -24),   S(  8,  17),   S(-23, -30),   S(  1,  -4),   S(  1,   0),   S( -3,  -6),
            S( -4, -16),   S( -7, -15),   S( -1,  -8),   S(  0,  17),   S(  1,  19),   S( -7,  30),   S(  8,  20),   S(  1,   9),
            S( -4, -17),   S(  8,  24),   S(  1,  19),   S(  4,   8),   S( 24,  37),   S( -6,  11),   S(  6,  -9),   S(  8,  33),
            S( -1,   0),   S( -3,  -6),   S(  0,  12),   S( -5, -17),   S( -7,  -7),   S(  4,  17),   S(  0,  14),   S(  4,   9),
            S( -1,   5),   S( -9, -24),   S(  8,  26),   S(  4,   7),   S(  0,  13),   S(  2, -11),   S(  2,  14),   S(  4,   3),
            S(  0,   0),   S(  1,   7),   S( -1,  -2),   S(  0,   6),   S(  1,   9),   S(  0,   0),   S( -2,  -5),   S(  0,   0),

            /* bishops: bucket 0 */
            S( 25,  25),   S(-34,   5),   S(-22,  10),   S(-14, -21),   S(-12,  -4),   S( -1,  18),   S( 76, -72),   S( 23,   0),
            S(-51,  26),   S(  8, -16),   S(-28,  46),   S(  5,  27),   S(  8,  20),   S( 51,  -8),   S( 38,  46),   S( 37,  -1),
            S(  7,  33),   S(  2,  38),   S(  6,  28),   S( -6,  43),   S( 41,  14),   S( 40,  52),   S( 32,  13),   S(  9,   0),
            S( 21, -19),   S( 43, -58),   S( 10,  35),   S( 55,  10),   S( 67,  37),   S( 49,  55),   S(  7,  30),   S( 38,  23),
            S( 57, -46),   S( 45,   0),   S( 63,  31),   S( 61,  63),   S( 84,  34),   S( 19,  33),   S( 42,  45),   S(-10,  25),
            S( 69, -23),   S( 86,  31),   S( 87,  38),   S( 67,  20),   S(-11,  61),   S(  5,  73),   S( 57,  37),   S(-15,   7),
            S(-71, -102),  S( 83,  51),   S(102,  55),   S(  6,   6),   S(  6, -23),   S( 11,  41),   S(-14,  43),   S(-18,  40),
            S(-11, -39),   S(  8,  13),   S(-12, -26),   S(-14, -16),   S( -2, -45),   S( -8,  26),   S(-12,  31),   S(-24, -19),

            /* bishops: bucket 1 */
            S(-44,  17),   S(-20,  -7),   S(-27,  39),   S(  5,  -9),   S(-25,  22),   S( 16,   5),   S( 16,  -2),   S( 46, -49),
            S(  9, -28),   S(-10,   1),   S( -6,  -3),   S(-10,  19),   S( 30, -11),   S(  9,  -1),   S( 62, -13),   S(-10,  -7),
            S(-28,   7),   S( 28,  -2),   S(-16,  18),   S( 28,   4),   S(  9,  -3),   S( 34,   4),   S(  3,  -2),   S( 67, -16),
            S( 27, -43),   S( 46,  -1),   S( 25,   6),   S( 11,  18),   S( 53,  -1),   S(  2,  15),   S( 56,  -6),   S( -7,  12),
            S(-18,  -8),   S( 48,  -4),   S( 28,  -5),   S(104, -10),   S( 35,  21),   S( 38,  26),   S( -7,  26),   S( 28,  20),
            S( 64, -28),   S( 17,   0),   S( 99, -28),   S( 34,  -2),   S(102, -27),   S(-39,  43),   S(-33,  79),   S(-21,  20),
            S(  3, -53),   S( 48, -29),   S(-32, -23),   S( 15,  29),   S( 17,  39),   S(-25,  48),   S(  0,   9),   S(-31,  41),
            S( 10, -29),   S(-12,  14),   S(-15, -21),   S(-37,  -1),   S(-13,   2),   S( 26, -27),   S( 52,  -8),   S(-19,  41),

            /* bishops: bucket 2 */
            S( 14, -13),   S(-11, -14),   S(  2,   9),   S(-20,   9),   S( 16,   5),   S(-14,   9),   S( 10, -10),   S( 18,   4),
            S( 28, -51),   S( 17,  -8),   S( -5,  -3),   S( 12,  11),   S(-10,  20),   S(  9,   8),   S(  3,   0),   S( 20, -73),
            S( 43, -18),   S( 29, -10),   S(  1,  28),   S( -6,   9),   S(  3,  33),   S( -7,  -5),   S( -3, -16),   S( -7, -11),
            S( -4,  -7),   S( 41,  11),   S(  5,   6),   S( 28,  29),   S(  0,  20),   S( -8,  33),   S(-14,   3),   S( 12,   4),
            S(-12,  24),   S( -1,  20),   S( 50,  22),   S( 40,   6),   S( 12,  33),   S(  0,  25),   S( -1,  42),   S( 47, -19),
            S(-44,  27),   S(-20,  54),   S( -7,   7),   S( 56,   4),   S( 76,   1),   S( 53,  46),   S( 66,   2),   S(  6, -34),
            S(-32,  58),   S( -6,   0),   S(-10,  32),   S( 41,  11),   S(-67, -27),   S(-43,   7),   S(-44,  29),   S(-12, -45),
            S(-73,  -1),   S( -4,  16),   S(  1,  -2),   S(-11,  18),   S( -9, -37),   S( -7,  -2),   S(-19, -47),   S(-59, -18),

            /* bishops: bucket 3 */
            S( 35,   5),   S( 52, -29),   S( 16, -14),   S(  4,  10),   S( 16,  22),   S(  4,  35),   S(-16,  53),   S(  2, -20),
            S( 43,  -5),   S( 33,  -3),   S( 21,  12),   S( 25,  12),   S( 26,  21),   S( 28,  18),   S( 18,  10),   S( 44, -59),
            S(  5,   0),   S( 47,  28),   S( 23,  44),   S( 24,  34),   S( 23,  39),   S( 17,  31),   S( 21,   0),   S( 16,  -4),
            S( -9,  13),   S( 14,  38),   S( 32,  51),   S( 39,  50),   S( 42,  36),   S( 28,  15),   S( 29,  -3),   S( 46, -32),
            S(  9,  32),   S(  8,  56),   S( 10,  63),   S( 67,  48),   S( 49,  47),   S( 59,   7),   S( 18,  35),   S(  0,  20),
            S(  4,  44),   S( 29,  59),   S( -4,  52),   S( 10,  56),   S( 44,  61),   S( 74,  55),   S( 64,  55),   S( 32,  66),
            S(-20,  73),   S(  1,  48),   S( 27,  26),   S( 16,  54),   S( 13,  47),   S( 29,  86),   S(-28,  -6),   S( 34, -62),
            S(-50,  39),   S(-22,  67),   S(-69,  49),   S(-19,  49),   S(  9,  20),   S(-57,  41),   S(  5, -28),   S( 45,  17),

            /* bishops: bucket 4 */
            S(-41,   9),   S(-40,   0),   S(-26,  32),   S(-48,  29),   S(-26,  -5),   S(-32,  -5),   S( -4, -22),   S(-20, -45),
            S( -7, -10),   S(-16,  23),   S( 77,  -9),   S(-26,  29),   S(-40,   7),   S(  2, -26),   S( -6,  -7),   S(-36, -32),
            S( 55,  12),   S(-13, -30),   S( 16,   6),   S(  6,  -8),   S( 21, -15),   S(-55,  51),   S(  3, -21),   S(-86,  15),
            S( 21, -20),   S( 61, -26),   S( 44, -10),   S( 12,  27),   S(-29,  42),   S( 36,  13),   S(-73,  48),   S(-25, -37),
            S( 22,  -3),   S(-10, -38),   S(-10,  -9),   S( 11,  25),   S(-20,  43),   S( 16,  17),   S(-20,  56),   S(-33,  15),
            S(-75, -101),  S(-48, -35),   S(-14,   1),   S(  7,   6),   S(-29,  43),   S( 28,   8),   S(  2,  18),   S( 10,  26),
            S(  3,  -3),   S(-27, -12),   S( -4,  10),   S(-27, -35),   S(  7, -21),   S( 46, -13),   S( 23,  19),   S( 41,  47),
            S( -8, -17),   S(  4, -29),   S(-14,   0),   S( -8, -37),   S(-26, -16),   S( -5,  21),   S(  6,  32),   S( 28,  37),

            /* bishops: bucket 5 */
            S(-21, -13),   S( 14, -17),   S(-32,  23),   S(-48,  11),   S(-29,  18),   S(-90,  13),   S(-29,  17),   S(-31, -17),
            S(-14, -22),   S(-13,  10),   S( 25, -13),   S(-35,  25),   S(-53,  20),   S(-33,  23),   S(-43,   1),   S( -6, -28),
            S( 10,  17),   S(-30,  -9),   S( 11,   4),   S(-12,   2),   S(-15,  19),   S(-63,  18),   S( -3,   7),   S(-10,  20),
            S(  8, -24),   S(-15,   8),   S( 79, -30),   S(  1,  23),   S( 23,  -3),   S( 18,   6),   S(-63,  25),   S(-39,   3),
            S( 17, -17),   S( 23, -13),   S(-18, -21),   S(-29, -13),   S( 34, -18),   S( -8,   6),   S( 31,  13),   S(-37,   7),
            S( 18, -12),   S(-22,  18),   S( 39, -35),   S(-24, -56),   S(-24,  23),   S(-33,  12),   S(-32,  17),   S(-40,  56),
            S(-20, -13),   S(-35, -10),   S(  3, -30),   S( -7,   6),   S( 13,  -3),   S(-13,  22),   S(  1,  34),   S(-40,  21),
            S(-19,  12),   S(-18, -21),   S(-11, -24),   S(  0, -12),   S(-52,  24),   S( 13,   6),   S( -4,  -6),   S( 22,   7),

            /* bishops: bucket 6 */
            S(-20, -40),   S(-12, -13),   S(-22,   5),   S(-30,  23),   S(-53,  35),   S(-35,  22),   S(-61,  36),   S(-73,   1),
            S(-51,  14),   S(-54,   4),   S(-101,  44),  S(-38,  28),   S(-65,  37),   S(-43,  26),   S(-40,  21),   S(-50,  15),
            S(  1,   1),   S(-36,   0),   S(-10,  17),   S(-39,  28),   S(-25,  32),   S(-24,   5),   S(-28, -14),   S(-46,  15),
            S(-34,  25),   S(-71,  32),   S(-20,   8),   S( 39,  17),   S(-13,  30),   S(  1,  16),   S( 15, -20),   S(-24,   9),
            S(-39,  30),   S(-50,  31),   S(  0,  11),   S( 48,  -5),   S(-22, -11),   S(-24, -15),   S( 34,  -6),   S(-31,  -4),
            S(-57,  56),   S(-20,  24),   S(-49,  18),   S( -4,  11),   S( -8,  -7),   S(-18, -21),   S(-23,  20),   S(-17, -10),
            S(-12,  30),   S(-72,  49),   S(-40,  27),   S(-74,  34),   S( -5,  -4),   S( 24,  -9),   S( -4,  -6),   S( -8, -17),
            S( -7,  17),   S(-23,  24),   S(-39,  26),   S(  1,  11),   S(-44,  20),   S( 30, -25),   S(-25,  17),   S(-14,   2),

            /* bishops: bucket 7 */
            S( -6, -23),   S(-92, -17),   S(-24, -35),   S(-26,   2),   S(-42, -19),   S(-54,   3),   S(-68, -39),   S(-73, -23),
            S(-15,  -9),   S( -9, -17),   S( 27, -30),   S(-26, -16),   S(-45,  21),   S(-55,  18),   S(-16, -17),   S(-14, -10),
            S(-64, -32),   S(-40,  12),   S(  1,   7),   S( 39, -17),   S(  2,  -1),   S( -8,   0),   S(-27,  -6),   S(-59,   3),
            S(-39,   6),   S(-40,  10),   S(-12,  12),   S(-40,  34),   S( 79,   3),   S(-21,  13),   S( 82, -22),   S(-26, -22),
            S(-13,   0),   S(  7,  12),   S(-41,  18),   S( 30,   0),   S( 71, -22),   S( 61,  -5),   S(-36,  -1),   S(-41, -32),
            S(-108,  58),  S(-48,  65),   S( 11,  19),   S(-78,  31),   S(-33,  37),   S( 31,   1),   S(-17,  22),   S(-45, -86),
            S( -8, -12),   S( -1,  25),   S(-47,  47),   S( 12,  25),   S( 23, -11),   S( 30, -25),   S( 37, -18),   S(-18, -34),
            S(-12,  -2),   S(-22, -10),   S(-20,  15),   S(-22,   1),   S(  7, -19),   S( 16, -15),   S( 34,   7),   S( -4,   2),

            /* bishops: bucket 8 */
            S( 41,  71),   S(  0, -43),   S( -4,  -7),   S( -2,  44),   S(-16,   6),   S(-10, -40),   S(-27, -50),   S(-15, -22),
            S(-10,   1),   S( 22,  44),   S( 17, -20),   S(  1,  14),   S( -2,  -6),   S( -6,  -5),   S(-24, -70),   S( -3,   6),
            S(-11,  -4),   S(-24, -37),   S( 32,  15),   S( 13,   2),   S( 11,  14),   S( 11,  21),   S(-18, -27),   S(-31, -17),
            S( -7, -10),   S( 32,  43),   S(-28,  14),   S( 12, -13),   S(  3,  31),   S( 17,  40),   S(-25, -15),   S( -6,  -3),
            S( 11,   5),   S( 56,  76),   S( 21,  10),   S( -2,   7),   S( 23,  60),   S(-26,  33),   S( 10, -52),   S(  7,  38),
            S(-17,  -6),   S( -7,  12),   S(  6,  54),   S( 19,  -6),   S( 21,  14),   S( 40,  -2),   S( -9,  34),   S(  9,  47),
            S(-10,  11),   S(-15, -25),   S( 26,  11),   S( 33,  22),   S(  1,   0),   S( 19,  38),   S( 33,  76),   S( -7,  18),
            S( -4,   8),   S(  8,   6),   S(  1,  16),   S(  3,  34),   S( 27, -10),   S( 26,   5),   S(  6,  24),   S( 47,  37),

            /* bishops: bucket 9 */
            S( 16,  39),   S(  4,  -6),   S(-30,  27),   S(-47, -29),   S(-25,  -9),   S(-26, -32),   S( -3, -25),   S( -9, -17),
            S(-20, -31),   S( 21, -39),   S(-10,  10),   S(-27,   1),   S(-48,   7),   S(-18, -41),   S(-37,  10),   S(-14,  -9),
            S(-20, -24),   S( 10, -19),   S(-11, -31),   S(  1, -13),   S(  7,  -5),   S(-15, -33),   S( -4,  -7),   S(-12,  -8),
            S( -9,  23),   S(  0,  -7),   S(  8, -15),   S( 17,  11),   S(-22,   8),   S( -4,  14),   S( 11,  40),   S(  3,  32),
            S( 24,  22),   S(  1, -41),   S(  4,  24),   S( 21, -33),   S( -6,  27),   S( -1,   6),   S( 14,  37),   S(-17, -26),
            S( 11, -16),   S(-21,  17),   S( -1,   6),   S( 17,   7),   S( 44, -54),   S(  2,  12),   S( 27,  18),   S(  9,  43),
            S( 15, -18),   S( -2,  24),   S(  1,   1),   S( 22,   7),   S( 23,  -7),   S( 29,  -9),   S( 11,  28),   S( 15,  31),
            S( 19,  40),   S( -7, -61),   S(  0,   4),   S(  7,  -6),   S(  7,  19),   S( 16,   7),   S( 11,  -6),   S( 21,  26),

            /* bishops: bucket 10 */
            S( 12,  29),   S( 15,  19),   S(-27, -47),   S(-26, -34),   S(-82, -39),   S(-53, -58),   S( -2, -23),   S(-10,  20),
            S( -6,  11),   S(-11, -23),   S( -3, -54),   S(-25, -76),   S(-37,   0),   S(-31, -31),   S(-34, -22),   S(-11,  11),
            S( -8, -18),   S(-30,  23),   S(  4, -15),   S(-35,  22),   S(-34, -29),   S(-18, -37),   S( -9,   3),   S( 12,   5),
            S(-25,  -4),   S(-30,  28),   S(-37, -34),   S(-22,  29),   S(-34,  19),   S( 11,  -8),   S( 43,  13),   S( -8, -15),
            S(  4,   3),   S(-38,  30),   S( 10, -10),   S(-18,   9),   S( 36, -37),   S( 13,  32),   S(  9, -47),   S(  9,  13),
            S( -2,  -5),   S( 21, -13),   S(  2,   2),   S( 10,   9),   S( 23, -36),   S( -4,  22),   S( 12, -16),   S( 16,  10),
            S( 11,  34),   S( -9, -16),   S( 30, -16),   S( 10,   5),   S( -6,  16),   S(-15, -42),   S(  7,  -5),   S(  8,  22),
            S( 10,  18),   S( 15,  57),   S( 42, -19),   S(  1,  45),   S( 14,  47),   S(  6,  23),   S( 12,  30),   S(  2, -11),

            /* bishops: bucket 11 */
            S(  8, -13),   S(-14, -16),   S(-23, -12),   S(-16, -39),   S(-33, -31),   S(-11, -28),   S( -5,  -9),   S( -5,  32),
            S(  2, -27),   S( 18,  -3),   S(-11,   8),   S( 11, -36),   S(-37, -15),   S(-47, -28),   S(-21,  11),   S( 11, -13),
            S(  6, -26),   S(  5, -26),   S( -5,   7),   S(-39,  33),   S(-25, -21),   S( 17,  29),   S( -2, -18),   S( -7, -20),
            S( 20,   2),   S(  9, -27),   S( 19,  15),   S(-20, -29),   S( 10,   5),   S( 36,  54),   S( 31,  11),   S(-14, -36),
            S(-15, -11),   S( -7, -50),   S(-42,  54),   S(-37,  66),   S(-24,  36),   S( 28,  18),   S( 26, -19),   S(  9,   4),
            S( -2,  10),   S( -5, -18),   S(  5,  26),   S( 10,  11),   S( -3,  23),   S( 34, -11),   S(  5,   2),   S( -5, -17),
            S(-11, -19),   S( 35,  64),   S( 35,  34),   S( 19,  11),   S( 11,  -6),   S( -6, -11),   S(-18, -19),   S(  1,  -3),
            S( 35,  24),   S( -5, -41),   S( 26,  63),   S( 22, -27),   S( 21,  52),   S(  5,   6),   S(-12,   4),   S(  3, -14),

            /* bishops: bucket 12 */
            S( -4, -14),   S(  0,  -8),   S( -4,  -8),   S(  6,  14),   S( -4,   1),   S( -6, -12),   S( -1,  12),   S( -5,  -8),
            S( -2, -14),   S(  5,   2),   S(-12, -20),   S( 14,  39),   S( -6,  -1),   S(  8,  10),   S(-20, -25),   S( -1,  -7),
            S( 10,  -4),   S( 12,  14),   S( 28,  21),   S(  6,   7),   S(-11,  13),   S( -5,  -2),   S( -3,  -8),   S(  2,  10),
            S( 13,  14),   S( 20,   4),   S(  2,  -1),   S( 16,  35),   S(  5,   8),   S( 10,  23),   S( -1,  17),   S(  2,  10),
            S( 10,  -7),   S(  8,   0),   S(  0,  34),   S( 25,  -1),   S( 23,  41),   S( 21,  26),   S(  6,   1),   S( -1,  -2),
            S(  2,  -8),   S(-14, -19),   S(  7,  33),   S(  3, -14),   S( 20,  19),   S( 10,   5),   S(-11, -17),   S( -6, -12),
            S( -5, -11),   S(  9,  21),   S(  4,   6),   S(  6,  -9),   S( 12,   3),   S( 13,   7),   S( 21,  22),   S(  3,   3),
            S(  4,   8),   S( -3,  -5),   S(  4,   8),   S( -1,  -2),   S(  2,  11),   S(  8, -14),   S(  9,  -2),   S(  7,   4),

            /* bishops: bucket 13 */
            S( -6, -22),   S(  0,   1),   S( -9, -25),   S( -3, -17),   S(  0,   1),   S( -6, -12),   S(-19, -20),   S( -1,  -2),
            S( -5,   5),   S(  1, -14),   S(  2,   1),   S( 19,  -4),   S( -9, -22),   S(  3,  20),   S(  4,  -9),   S(  3,   0),
            S( 10,  -6),   S( 39,  25),   S( 24,   7),   S( 13,  19),   S( -6,  15),   S( 14,  39),   S( -8, -13),   S( -3,   9),
            S( 28,  42),   S( 40,  -1),   S( 18,  11),   S(-21,   8),   S( 28,  60),   S(-11,  -4),   S( 19,   4),   S(  8,  21),
            S(  5,  10),   S( 20,  31),   S(  8, -23),   S( 16,  11),   S( 10,  -1),   S( 12,  43),   S(  8,  13),   S( -4,   0),
            S( -6, -11),   S(  2,  15),   S(  9, -13),   S( 22,   9),   S(  9,   0),   S(  9,  -1),   S( -1, -19),   S(  3, -15),
            S(  6,   6),   S( -1,  -8),   S( -4, -11),   S(  9,  19),   S(  3,  17),   S( 21,   8),   S( 16,  -1),   S(  8,  -1),
            S(  3,   2),   S(  0,  -8),   S(  1,   8),   S(  4,  15),   S(  8,  22),   S(  3, -15),   S( 14,  -1),   S( 14,  -4),

            /* bishops: bucket 14 */
            S( -9, -14),   S(  5,  20),   S( 19,  17),   S( 13,  53),   S( -8,  15),   S( -7, -15),   S(  2,  14),   S( -2,  22),
            S( -2,  12),   S(  0, -20),   S( -1,   7),   S( -5, -15),   S( 12,  -9),   S( -1,   9),   S(  8,  42),   S(  0,   7),
            S( -7, -24),   S(  5,  15),   S(-18, -10),   S( 12,  30),   S(  9,  20),   S( 10,  21),   S(  2,  24),   S( 10,  20),
            S(  7,  40),   S(  6,  -7),   S(  5,  13),   S( -2,  37),   S( 20,  30),   S( 13,  -7),   S( 25,  12),   S(  2, -30),
            S( 11,   8),   S(  3,  10),   S(  7,  -8),   S( 24, -14),   S(-11, -11),   S( -2,  -2),   S( 24,  -3),   S(  3, -19),
            S( -3,  -8),   S( 22,  30),   S( 19,   0),   S( 15,  34),   S( 11,  -5),   S(  0, -13),   S( -4,  13),   S(  9, -12),
            S( 17,  41),   S( 14,  15),   S( 14,   4),   S(  7,   3),   S( 10,   6),   S( -2,  -3),   S( -5,  -7),   S(  6,   4),
            S( 13,  -3),   S( 11,  23),   S(  8,  30),   S(  8,   1),   S( -6,  -6),   S( -2, -27),   S( 11,  15),   S(  6,  21),

            /* bishops: bucket 15 */
            S( -3,  -2),   S(  7,  13),   S(-17, -37),   S( -2,   9),   S( -4,  -1),   S(-14, -32),   S(-12, -32),   S( -6, -10),
            S(  8,  22),   S( -4,  -1),   S( -4,   0),   S( 10,  11),   S(  8,   4),   S( -5,  -9),   S(  1,  -9),   S( -2,  -5),
            S(  8,   3),   S( -2,   3),   S(  7,  -9),   S( 19,  48),   S( 15,  34),   S( 10,  12),   S( 11,  20),   S(  4,   1),
            S(  2,  -8),   S( 13,  22),   S(  4,  25),   S(-14,  13),   S( 23,   6),   S( 27,   8),   S( 30,  36),   S( 12,  19),
            S( -7, -15),   S( -3,   8),   S( 16,  68),   S( 22,  51),   S( 19,  21),   S(  8,  15),   S( 11,   8),   S( -2,  -7),
            S(  4,  25),   S( 10,  10),   S(  7,  53),   S( 22,  28),   S( 32,  11),   S( 16,  24),   S( -4,   1),   S( -1,  -8),
            S(  2,  -6),   S( 11,  26),   S(  4,  26),   S( 20,  26),   S(  8,  18),   S( -7,  -8),   S(  4, -10),   S(  2,   2),
            S(  0,  -7),   S( 13,   7),   S( 13,  12),   S(  5,  -5),   S(  5,  18),   S(  1,   4),   S(  4,  13),   S(  4,  -7),

            /* rooks: bucket 0 */
            S(-11,   7),   S( 19,  -8),   S(  3,  -7),   S(  1,   5),   S(-22,  64),   S( -8,  39),   S(-37,  45),   S(-64,  59),
            S( -6,   3),   S(-13,  32),   S(-25,   9),   S(  1,  31),   S(  5,  45),   S( -9,  24),   S(-20,   7),   S(-33,  45),
            S( 23, -14),   S( 19, -23),   S(-15,   5),   S( 10, -24),   S(-25,  53),   S(-13,  27),   S(-17,  46),   S(-15,  37),
            S( -2, -16),   S( 31,   8),   S(-45,  49),   S( 19,   4),   S( 18,  50),   S(-24,  46),   S(-34,  73),   S(-23,  38),
            S( 75, -71),   S( 52,  -7),   S( 22,  16),   S( 46,   7),   S( 38,  23),   S( 22,  78),   S( 56,  27),   S(-12,  78),
            S( 60, -14),   S( 68,  16),   S( 87,  -8),   S(111,   4),   S( 20,  57),   S( 32,  59),   S( 19,  75),   S(-48,  90),
            S( 12,  39),   S( 73,  69),   S(100,  18),   S( 77,  16),   S( 58,  47),   S( 20,  72),   S(-37, 105),   S(-32,  86),
            S( -6, -32),   S( 25,  13),   S( 52,  46),   S( 48, -25),   S( 57,  45),   S( 29,  26),   S( 60,  18),   S( 53, -30),

            /* rooks: bucket 1 */
            S(-53,  50),   S(-21,   3),   S( -2,  13),   S(-34,  22),   S(-29,  35),   S(-32,  50),   S(-46,  67),   S(-73,  69),
            S(-45,  32),   S(-18, -12),   S(-34,  34),   S(-21,  28),   S(-32,  16),   S(-44,  36),   S(-27,  27),   S(-35,  48),
            S(-34,  30),   S( -1, -16),   S( -8,   3),   S(-47,  40),   S(-35,  19),   S(-40,  37),   S(-85,  77),   S(-30,  64),
            S(-51,  59),   S( 21,  -7),   S( -4,  26),   S( -8,  12),   S(-48,  37),   S(-62,  77),   S(-41,  73),   S(-56,  84),
            S(-42,  58),   S(  4,  -4),   S( 20,  23),   S( 50,  -3),   S(-11,  27),   S( 12,  52),   S(-23,  58),   S( -9,  85),
            S( 50,  21),   S( 59,  -3),   S( 66,  -2),   S( 13,  19),   S(-13,  10),   S( 18,  54),   S( 21,  40),   S( 13,  69),
            S( 56,  48),   S( 49, -15),   S( 26,  20),   S( 46,  27),   S( 64,   6),   S(  1,  57),   S( 37,  66),   S( 32,  85),
            S( 43, -19),   S( 30, -32),   S( -5, -25),   S( -3, -19),   S( 65, -14),   S( 47,  12),   S( 67,  22),   S( 90,  20),

            /* rooks: bucket 2 */
            S(-61,  70),   S(-46,  56),   S(-40,  54),   S(-39,  18),   S(-23,  28),   S(-37,  32),   S(-35,  16),   S(-74,  55),
            S(-57,  58),   S(-67,  61),   S(-40,  51),   S(-48,  35),   S(-42,  35),   S(-45,  24),   S(-27,   4),   S(-34,  13),
            S(-55,  64),   S(-40,  61),   S(-41,  36),   S(-37,  34),   S(-40,  33),   S(-26,  30),   S(-16,   4),   S(-23,  34),
            S(-42,  83),   S(-32,  73),   S(-53,  75),   S(-58,  42),   S(-43,  43),   S(-19,  32),   S(-28,  36),   S(-22,  43),
            S(-32,  90),   S(-29,  72),   S(-23,  73),   S(-30,  39),   S(-37,  59),   S(  5,  41),   S(-22,  42),   S(-10,  63),
            S( 10,  78),   S( -2,  83),   S( 18,  59),   S(-22,  51),   S( 69,  -7),   S( 25,  55),   S( 79,   4),   S( 24,  72),
            S( 61,  69),   S( 10,  79),   S( 26,  56),   S( 48,  18),   S( -9,   4),   S( 47,  62),   S(-39,  95),   S( 18,  79),
            S( 38,  28),   S( 53,  38),   S( 33,  21),   S( 18,  -4),   S( 11,   6),   S( 33,  19),   S( 26,   3),   S(  6,  47),

            /* rooks: bucket 3 */
            S(-16,  79),   S( -6,  70),   S(-15, 103),   S( -6,  81),   S(  6,  51),   S( 10,  47),   S( 25,  12),   S( -6,   1),
            S( -3,  62),   S(-21,  82),   S(-15, 104),   S( -6,  98),   S( -5,  64),   S( 16,  18),   S( 37,   7),   S( 19,  17),
            S(  9,  60),   S(-11,  90),   S(-18,  94),   S(-14, 100),   S( 13,  42),   S( 15,  36),   S( 39,  20),   S( 27,  13),
            S(  2,  94),   S( -3, 116),   S(-27, 129),   S( -4, 104),   S(-12,  77),   S( -1,  69),   S( 30,  55),   S( 15,  19),
            S( -3, 109),   S(-23, 129),   S(  5, 119),   S(  2, 116),   S(  0,  98),   S( 33,  67),   S( 66,  51),   S( 32,  56),
            S(  2, 122),   S( 13, 114),   S( 24, 114),   S( 24, 105),   S( 95,  43),   S(116,  36),   S( 89,  62),   S( 45,  36),
            S( 18, 122),   S( -7, 131),   S( 10, 138),   S(  6, 133),   S( 17, 117),   S( 91,  64),   S(143, 104),   S(187,  69),
            S(115, -18),   S( 50,  58),   S( 23, 104),   S( 44,  81),   S( 27,  65),   S( 80,  59),   S( 63,  51),   S( 92,  19),

            /* rooks: bucket 4 */
            S(-26, -24),   S( 15, -16),   S(-19, -13),   S( -8, -18),   S(-32,   9),   S(-14,  39),   S(-35, -14),   S(-111,  51),
            S(-39, -45),   S(-56, -15),   S( -9, -16),   S( 29, -46),   S(  3,  25),   S(-52,  37),   S(-31, -11),   S(-34,  14),
            S(-13, -37),   S(-19, -46),   S(-57,   5),   S( -9, -51),   S(-43,  10),   S(-61,  20),   S(-29,  12),   S(-61,  34),
            S(-37, -53),   S(  4,  -3),   S(-18, -18),   S(  0, -19),   S( 16,  10),   S( -7,   9),   S( -2,  15),   S(-21,  -3),
            S( -7, -27),   S(  5, -34),   S( 58,  -4),   S( 44,  -3),   S( 86, -10),   S( 62,  10),   S( 21,   7),   S( 17,  23),
            S(-11, -42),   S( 10,  -5),   S( 18,  16),   S( 16,   7),   S( 29,  22),   S(  3,  50),   S( 51,   6),   S( 39,  46),
            S(-47, -18),   S( 23,  22),   S( 58,  -3),   S( 30, -21),   S( 70,  -5),   S( 11,  -3),   S(-11,  -4),   S(-16,  18),
            S( 19, -13),   S(  7,  27),   S( 35, -16),   S(  2, -24),   S( 57,  14),   S(  6,  -7),   S(  2,  -6),   S( 12,   8),

            /* rooks: bucket 5 */
            S(-40,  34),   S(-45,  24),   S( -7,  11),   S( 14,  -1),   S(-14,  19),   S(-17,  25),   S(-39,  63),   S(-49,  53),
            S(-14,  -6),   S(-25, -26),   S( 47, -52),   S( 10, -12),   S( 12, -11),   S(-21,   0),   S(-28,  11),   S(-22,  34),
            S(-73,  34),   S(-33,  -5),   S( 25, -46),   S(  8, -24),   S(  6, -28),   S( -2,   5),   S(-57,  40),   S(-36,  32),
            S(-51,  22),   S( -8,  -4),   S( 81, -55),   S( 51, -21),   S( 11,  -9),   S( -2,  27),   S( -8,  41),   S( -8,  46),
            S( -4,  30),   S(  6,  -2),   S( -7,   9),   S(  5, -28),   S( 15,  10),   S( 49,  18),   S( 30,  22),   S( 38,  29),
            S( -7,  31),   S(  2,  24),   S( 20,   3),   S(-16,  -8),   S( 24, -15),   S(  3,  27),   S( 74,  16),   S( 43,  37),
            S( 37, -20),   S( 60, -34),   S( 18, -14),   S( 48,  10),   S( 68, -38),   S( 81, -29),   S( 79, -23),   S( 61,  -7),
            S(-20,  47),   S( 44, -18),   S( 36,  -8),   S( 15,  22),   S( 23,  20),   S(  7,  29),   S( 13,  38),   S( 49,  34),

            /* rooks: bucket 6 */
            S(-51,  45),   S(-12,  21),   S(-10,  14),   S(-32,  13),   S(  6,   1),   S( -5,  11),   S( 11, -11),   S(-16,  -1),
            S(-48,  32),   S( 13,  15),   S(-16,  12),   S( -7,   3),   S( 52, -32),   S(-24,   2),   S(-14, -14),   S( -9,   8),
            S(-63,  41),   S( -9,  28),   S(-14,  10),   S(  9,  -8),   S(-12,   2),   S( 44, -10),   S(  3, -32),   S(-12,  -2),
            S(-72,  69),   S( -1,  40),   S(  9,  12),   S( 51, -20),   S( 41, -22),   S( 11,   1),   S( 34, -25),   S( -4,  26),
            S(-17,  58),   S( 23,  26),   S( 68,  18),   S( 68, -19),   S( 50, -33),   S( 16,  13),   S( 49, -12),   S(103,  -8),
            S( 92,  13),   S( 88,  10),   S(109,   4),   S( 71, -11),   S( 16, -11),   S( 12,  19),   S( 31, -27),   S(117, -10),
            S( 37,  12),   S(135, -35),   S( 82, -18),   S( 99, -50),   S( 45, -16),   S( 25,  -9),   S( 61, -28),   S( 76, -28),
            S( 92, -22),   S( 57,  11),   S(-35,  41),   S( 58, -13),   S( 45,   2),   S( 41,  26),   S( 86,   5),   S( 42,  25),

            /* rooks: bucket 7 */
            S(-94,  41),   S(-82,  40),   S(-54,  39),   S(-57,  32),   S(-34,   3),   S( -6, -29),   S(-37,  15),   S(-51, -26),
            S(-72,  23),   S(-38,  15),   S(-63,  42),   S(-75,  44),   S(-39,  -5),   S(-12, -11),   S( 15,   3),   S(-37, -47),
            S(-82,  35),   S(-82,  42),   S(-24,  10),   S(-59,  28),   S(-27,  -7),   S(-26,   1),   S( 50, -40),   S( 26, -67),
            S(-74,  37),   S( -5,  15),   S(  6,   6),   S( 47, -26),   S(  0,  -8),   S( 81, -39),   S( 54, -19),   S( 17, -33),
            S(  9,  35),   S( 12,  22),   S( 44,  13),   S( 87, -12),   S(138, -54),   S(146, -71),   S(104, -16),   S(-49, -34),
            S( 22,  35),   S( 40,  18),   S(101,  -2),   S(102, -18),   S(108, -16),   S( 38,  13),   S( 21,  41),   S(  2, -13),
            S( 20,  -7),   S( 36, -24),   S( 65, -11),   S(131, -51),   S(138, -51),   S(123, -45),   S( 63,  -8),   S( 38, -37),
            S( -5, -18),   S(-18,  19),   S( 30,   9),   S( 20,  13),   S( -5,   3),   S( 47,  -4),   S( 48,  13),   S( 49, -18),

            /* rooks: bucket 8 */
            S(-11, -71),   S( 24, -29),   S(-20,  -1),   S( 28,  18),   S(-24, -38),   S(-23,  -1),   S( -8, -52),   S(-17,  20),
            S(-18, -103),  S(  1, -38),   S(-26,  12),   S(-43, -83),   S(-15, -40),   S( -1, -10),   S(-14, -19),   S(-46, -32),
            S( 23,   0),   S( -1, -18),   S( 20,  18),   S(-30,  20),   S( -9,  75),   S( 17,  30),   S(  1,  74),   S(-31,  12),
            S(-10, -26),   S(  0,  -2),   S( -4,   7),   S( 16,  26),   S(  6,  21),   S( 29,  40),   S(  7,  29),   S( -8, -10),
            S( -2, -40),   S( 13,  11),   S( 20,  26),   S( 18,  35),   S(  1, -20),   S(-13,  17),   S( 13,  52),   S(  5,  38),
            S(-25,   7),   S( -1, -11),   S(-19, -15),   S(  1, -29),   S(  8,  47),   S(-20,  12),   S(  0,  -1),   S( -2,  28),
            S(-28,  40),   S( -2,  27),   S( 23,   5),   S( 26,  15),   S( 43,  42),   S( 17,  52),   S( -3,   0),   S( 10,  53),
            S( -1,  15),   S(-10,   5),   S(-42,  41),   S( 43,  51),   S(-13,  18),   S( 27,  52),   S( -8,  22),   S(  3,   7),

            /* rooks: bucket 9 */
            S(-36, -67),   S( -8, -85),   S( 16, -101),  S( -5, -43),   S(-19, -53),   S( -7, -36),   S( -7, -27),   S( -5,  -5),
            S(-58, -27),   S(-31, -84),   S(-40, -94),   S(-40, -82),   S(-33, -68),   S(-44, -18),   S(-13, -59),   S(-24, -32),
            S( -9,  -5),   S(-28, -23),   S( -8, -15),   S(-16, -47),   S(-18, -16),   S(-23,  -5),   S( 14,  46),   S( 12,  30),
            S( -2,  16),   S( -4, -21),   S(  4, -13),   S( -1,  -1),   S(-17, -35),   S(-14, -10),   S( -2, -29),   S( 26,  -1),
            S(  2, -20),   S(-13, -26),   S(-13, -66),   S(-12,  -4),   S(-33, -47),   S(-32, -22),   S(-23, -19),   S( -8, -28),
            S(  8,  -7),   S(-33, -38),   S(-15, -31),   S( -2,  37),   S(-19,  12),   S(-18,   3),   S(-12,  -4),   S(-22, -12),
            S( -5,  33),   S(  2,   0),   S( 11, -29),   S( -8,  20),   S( -8, -20),   S( 22,  -1),   S( 28,  38),   S(-30, -19),
            S( -4,   7),   S(-17,  34),   S(  2,  -1),   S( -2,  29),   S(-25,  15),   S( 18,  49),   S( 11,  16),   S(  8,  49),

            /* rooks: bucket 10 */
            S(-14, -20),   S(-35, -13),   S(-15, -26),   S( -8, -58),   S(  2, -54),   S( -7, -73),   S( -9, -81),   S(-24, -19),
            S(-30, -24),   S(-35, -49),   S(-32, -31),   S(-58, -50),   S(-39, -45),   S(-18, -63),   S(-28, -62),   S(-36, -57),
            S( -8,   6),   S(-22,  -2),   S(-15, -32),   S(-64, -64),   S(-10, -27),   S(  3, -17),   S(-18, -34),   S(-11, -22),
            S(-26,   8),   S(-32, -18),   S( -9, -40),   S(-11,  24),   S( -1,   3),   S(  1,  15),   S( -7, -64),   S( 17, -34),
            S(-28, -10),   S( -1, -16),   S(-10, -31),   S(-18, -57),   S( 18,   2),   S( -1,  25),   S(-14, -32),   S(-16, -67),
            S(-24,   0),   S( -5,   5),   S( -2, -16),   S(-11, -35),   S( 18, -31),   S( -7, -26),   S(-36, -46),   S(  1, -29),
            S(  4,  -2),   S( 24, -22),   S(  9, -34),   S(  3, -19),   S( 37,  -5),   S(  4,   2),   S( -4, -20),   S( -8, -23),
            S(-11,   5),   S( 13,  49),   S( -3,  32),   S(-22,  13),   S(-17,  37),   S(-22,  13),   S(-24,  16),   S( -3,  22),

            /* rooks: bucket 11 */
            S(-51, -13),   S(-58, -14),   S( -2,   4),   S(-27,  18),   S(-47, -21),   S(-30, -18),   S(-20, -24),   S(-49, -59),
            S( -1, -20),   S(-14,  -9),   S(-72, -22),   S(-58, -16),   S(-34, -35),   S( -2, -14),   S(-53, -74),   S(-49, -89),
            S(-33,   7),   S(-17,   3),   S(-13,  28),   S(-37,  10),   S( -5, -45),   S(-17,   0),   S(  7, -30),   S(-17,   2),
            S(-18,  -2),   S( -4, -10),   S(-17,   0),   S( 13,  23),   S( 26,  25),   S(-22, -17),   S(  3,  15),   S(-12, -41),
            S(-20,  -1),   S( 21, -18),   S( 18, -22),   S(  6,  10),   S( 34,  -7),   S(  6, -15),   S( 25,  43),   S( -9, -75),
            S(-11, -34),   S(-16,  -9),   S(  6,  -9),   S( 26, -11),   S(-19, -38),   S( -1,   3),   S(-20,  25),   S(-15, -28),
            S( -3,  10),   S(-28,   0),   S(-10,  14),   S(-14, -11),   S(  7,  -6),   S(  5,  -3),   S(  5,  -8),   S(-10, -23),
            S(-16,   5),   S( 28,  21),   S( 23,  59),   S( 33,  23),   S(  5,  21),   S(-14,  29),   S( 23,  32),   S(-24,  35),

            /* rooks: bucket 12 */
            S(-30, -103),  S( -6,  -2),   S( -1, -40),   S(-18, -39),   S( -4,   3),   S(  8, -23),   S(-10, -28),   S(-24, -74),
            S(  4, -10),   S(  1,  14),   S( 14,  21),   S( 14,  31),   S( 13,  22),   S( 15, -11),   S(-12,  -7),   S(-21, -38),
            S( -9, -21),   S(  4,  36),   S( 11,  31),   S( 19,  11),   S(  0, -15),   S(  8,   4),   S( 11,  63),   S( -4,  21),
            S( -9,  -4),   S(  1,   2),   S( 17,  36),   S( 17,  25),   S( 18,  25),   S(  3,  18),   S( 11,  23),   S( -3,  11),
            S( 18,  10),   S(  8,  16),   S(  6,  50),   S(  0, -15),   S( 10,  32),   S(-11, -24),   S( -1,   4),   S(  5,  12),
            S( -1,  19),   S(-14, -35),   S( 22,  53),   S(  1,   8),   S( 13,  19),   S( -5, -46),   S( 15,  35),   S( -1,   7),
            S(-34, -29),   S( -7,  39),   S( 12,  33),   S(  8,  26),   S( -4,  -7),   S( 17,  28),   S( -1,  18),   S(  4,  41),
            S( 13,  21),   S(-22,  29),   S( 10,  28),   S( 13,  24),   S(  9,  16),   S(  6,  26),   S( -2, -10),   S( -1,  15),

            /* rooks: bucket 13 */
            S(-29, -54),   S(-30, -77),   S(-17, -42),   S(  2,  -1),   S(-24, -68),   S( -3,   3),   S(-24, -32),   S(-23, -46),
            S(-10, -24),   S(-12, -34),   S(  1,   9),   S( 11,   9),   S( 19,  20),   S( 11,   0),   S( 17, -18),   S(  0, -14),
            S(-29,  -7),   S(-16, -11),   S( -5, -16),   S( 18,  27),   S( 20,  25),   S(  3, -21),   S(  9,  29),   S( -1, -33),
            S(  3, -23),   S( -6, -25),   S(  8,  21),   S( 12,  27),   S( 11,  30),   S(-11,   0),   S(  1,  -3),   S(  6,  37),
            S( -3,   5),   S( -6,   0),   S(  4,  10),   S(  8,   4),   S(  4,  28),   S(  3, -17),   S(  1,  22),   S( -7, -17),
            S(  1,  15),   S(-16,  -9),   S( -9,  -5),   S( -9,  29),   S(-18, -37),   S( 13,  23),   S( -7,  12),   S( -7,  -5),
            S(  2, -16),   S(-12, -26),   S(-12, -40),   S( -4, -13),   S(-14,  -8),   S( -3,  -9),   S( -2,  -1),   S(  5, -31),
            S( 10,   7),   S(-10,   1),   S(-15,   4),   S(  3,  44),   S(-20, -12),   S(  3,  19),   S( -1,  37),   S( 11,   4),

            /* rooks: bucket 14 */
            S( -4, -19),   S(-36, -49),   S(-10,  -4),   S( -8, -50),   S(-21, -64),   S(-13, -28),   S(-41, -71),   S(-17, -36),
            S( -8,  57),   S(  8,  24),   S( 12,   8),   S(  2, -13),   S( -3,  -6),   S( -2,   2),   S( -3,  -1),   S( -5, -28),
            S(  3,  24),   S( -6,  12),   S(-17, -22),   S( -2, -16),   S(  7,  17),   S( -8, -16),   S( -4,  25),   S(-30, -76),
            S(  3,  15),   S( 12,  22),   S(  5,  15),   S( 12,   2),   S(-16,  -7),   S( -8, -32),   S(  2,  -4),   S(-14, -24),
            S( 15,  23),   S( 24,  35),   S(  0,   6),   S(  2,  17),   S(  8, -13),   S( 19,  36),   S( -8,   1),   S( -4,  -3),
            S(  5,  14),   S(  9,  20),   S(  8,  32),   S( -1, -12),   S( -3,  24),   S(-24, -10),   S( -4,  18),   S( -5, -13),
            S(  1,  -7),   S( -1,   9),   S(-11, -16),   S(-17, -27),   S( -1,  29),   S(  6,  -1),   S(-11, -13),   S(-12,  -7),
            S(  1, -14),   S( -1,   4),   S(  9,   5),   S( -1, -39),   S(-17, -24),   S(-12, -78),   S( -1, -25),   S(  5,  22),

            /* rooks: bucket 15 */
            S(-21, -53),   S(-12, -41),   S(-39, -60),   S(-11, -40),   S(  3, -15),   S(-13, -35),   S(-11, -15),   S(-25, -61),
            S(  6,  49),   S(-14, -10),   S( -6, -43),   S( -7, -22),   S( -5, -16),   S(  6, -10),   S( 11,  11),   S(  8,  16),
            S( 14,  22),   S( -6, -13),   S( 10,  13),   S( 14, -11),   S( 18,  16),   S( -7, -33),   S( 10,  25),   S(  6,  17),
            S(  9,   7),   S(  0,  -5),   S( 19,  20),   S( -7,  -8),   S(  8,   7),   S(  7, -10),   S( -6,  -6),   S(  1,  -4),
            S(  2,  -2),   S(  8,  16),   S( 11,  -1),   S(  7,  23),   S( 11,  26),   S( 16,  21),   S( -6,  -4),   S( -5, -16),
            S( 20,  39),   S(  4,   1),   S(  8,  -3),   S(  0, -10),   S( -5, -13),   S(-10,  13),   S(  4,  35),   S(  2,  24),
            S(  3,   2),   S(  5,  25),   S(  1,  28),   S(  7,  29),   S(  3,  24),   S(  3,   8),   S(-11,  -6),   S(  0, -24),
            S(  2,   9),   S(  1,  13),   S(  5,   1),   S(  2,  30),   S(  1,  18),   S(-10, -23),   S( -7,  -5),   S( -8,  -1),

            /* queens: bucket 0 */
            S( -9, -28),   S(-30, -85),   S(-25, -82),   S(  0, -122),  S(-17, -47),   S( -4, -55),   S(-65, -29),   S(-12, -27),
            S(-25, -27),   S(  9, -84),   S( 15, -93),   S(-15, -43),   S(  9, -49),   S( -8, -51),   S(-32, -35),   S(-29, -35),
            S(-25,  38),   S( -3, -13),   S( 16, -56),   S(  0, -24),   S( -3,  -8),   S( -3, -24),   S(-27,  10),   S(-71, -52),
            S(-36,  56),   S(  8, -19),   S( -3,  15),   S(-19,  53),   S(  7,  59),   S(-29,  27),   S(-40,  -7),   S(-15, -48),
            S(-32,   3),   S(-18, 103),   S( 11,  43),   S( 13,  47),   S( 11,  66),   S(-13,  67),   S(-46,  39),   S(-49,   8),
            S(-11,  12),   S( 30,  61),   S( 41,  44),   S( -4,  85),   S(-30,  55),   S(-64,  79),   S(-103,   7),  S(-40, -14),
            S(  0,   0),   S(  0,   0),   S( 36,   5),   S(-18,  67),   S(-46,  49),   S(-79,  91),   S(-84,  54),   S(-101,  31),
            S(  0,   0),   S(  0,   0),   S( 23,  20),   S(  1,   3),   S(-47,  55),   S(-60,  21),   S(-41,  28),   S(-65,  -4),

            /* queens: bucket 1 */
            S(  3, -25),   S(  4, -10),   S( 10, -68),   S( 32, -95),   S( 25, -68),   S( 15, -43),   S(-16, -31),   S(-22,  -9),
            S(-36,  10),   S( 26,  -4),   S( 38, -54),   S( 26, -14),   S( 42, -18),   S(  7,   9),   S(-25,  34),   S(-44,   0),
            S( 37,  15),   S( 29, -13),   S( 20,  28),   S( 26,  54),   S(-12,  53),   S( 35,  -2),   S(-11,  51),   S( -3,  37),
            S( 43,  17),   S( 22,  52),   S( 14,  79),   S( 14,  89),   S( 23,  60),   S( 14,  24),   S( 28,  34),   S(-22,  80),
            S( 39,   2),   S( 50,  73),   S( 42, 131),   S( 17,  71),   S( 34, 113),   S( 59,  30),   S(-12,  72),   S( 11,  86),
            S( 49,  37),   S( 92,  24),   S(101,  88),   S(123,  67),   S( 67,  87),   S(-17, 125),   S( 30,  75),   S(-16,  42),
            S( 83,  34),   S( 73,  28),   S(  0,   0),   S(  0,   0),   S( 15,  60),   S(-10,  68),   S(-14,  80),   S(-50,  54),
            S(101, -19),   S( 72,   4),   S(  0,   0),   S(  0,   0),   S( 19,  68),   S( 64,  64),   S( 90,  10),   S(-13,  35),

            /* queens: bucket 2 */
            S( 18, -49),   S( 18, -15),   S( 24,  -7),   S( 41, -31),   S( 41, -59),   S( 12, -28),   S(-24, -55),   S( 33,   9),
            S( -3,  22),   S(-12,  40),   S( 36,  -5),   S( 43,  -1),   S( 50, -12),   S( 15, -11),   S( 22,  24),   S( 20,  28),
            S( 44,  17),   S( 27,  31),   S( 14,  72),   S( 12,  67),   S( 29,  30),   S( 14,  63),   S( 34,  17),   S( 21,  39),
            S( 25,  38),   S( 28,  82),   S( 15,  92),   S(  6, 118),   S( 36,  89),   S( 15,  92),   S( 25,  74),   S( 41,  84),
            S(-10,  77),   S( 26,  16),   S( -1, 122),   S( 29, 122),   S( 48, 121),   S( 74,  86),   S( 62, 104),   S( 72,  57),
            S(-36,  78),   S(-42, 112),   S(  9, 123),   S( 79, 102),   S( 61,  81),   S(114, 132),   S(122,  70),   S(  4, 183),
            S(-38, 126),   S(-17, 106),   S(-15, 127),   S( 74,  85),   S(  0,   0),   S(  0,   0),   S(  4, 136),   S( 41,  88),
            S(-23,  61),   S( 24,  31),   S( 74,  30),   S( 44,  89),   S(  0,   0),   S(  0,   0),   S( 82,  59),   S( 34,  85),

            /* queens: bucket 3 */
            S(-65,  28),   S(-44,  38),   S(-35,  29),   S(-18,  38),   S(-38,  14),   S(-19, -35),   S(-33, -62),   S(-63,  16),
            S(-62,  11),   S(-47,  39),   S(-30,  47),   S(-21,  49),   S(-20,  51),   S(-16,   7),   S( 12, -18),   S( 17, -61),
            S(-59,  49),   S(-42,  81),   S(-37,  91),   S(-43, 128),   S(-37,  87),   S(-33,  82),   S(-19,  51),   S(-25,  17),
            S(-43,  46),   S(-56,  94),   S(-53, 140),   S(-39, 157),   S(-44, 149),   S(-31, 105),   S(-17,  87),   S(-24,  83),
            S(-58,  86),   S(-48, 112),   S(-54, 132),   S(-36, 164),   S(-20, 148),   S(-18, 172),   S(-26, 139),   S(-30, 112),
            S(-82,  91),   S(-70, 133),   S(-77, 165),   S(-59, 176),   S(-52, 177),   S( 20, 124),   S(-27, 171),   S(-29, 129),
            S(-103, 118),  S(-104, 145),  S(-88, 179),   S(-86, 145),   S(-93, 184),   S(-13, 108),   S(  0,   0),   S(  0,   0),
            S(-142, 133),  S(-97, 111),   S(-78, 107),   S(-89, 135),   S(-52, 121),   S(-21,  96),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-27,  14),   S(-69, -65),   S(-19, -26),   S(  0, -10),   S(-17, -13),   S(-18, -12),   S(-47, -35),   S(  2,   6),
            S( -2,  -1),   S( -1,  25),   S(-29,  11),   S(-47,  14),   S(-45,  -7),   S( -7,  14),   S(-47, -25),   S(  2, -12),
            S( -3,  39),   S( 30, -13),   S( 44, -31),   S( 43,   5),   S( 29,   8),   S( -5,  23),   S(-49, -52),   S( 35,  30),
            S( 18,  26),   S( 21, -13),   S( -3,  -4),   S( 14,  35),   S( 64,  30),   S( 13,  33),   S(-37,   2),   S(-32, -13),
            S(  0,   0),   S(  0,   0),   S( 43,  35),   S( 71,  61),   S( 16,  76),   S(-14,  30),   S(-15,   6),   S(-17,   0),
            S(  0,   0),   S(  0,   0),   S( 31,  27),   S( 55,  59),   S( 54,  62),   S( 23,  30),   S(-10, -10),   S( -3,  16),
            S(  2,   1),   S( 25,  14),   S( 69,  43),   S( 55,  62),   S( 72,  21),   S( 17,  23),   S(  1,   5),   S(-51,   5),
            S( 55,  43),   S( -7,   4),   S( 57,  31),   S( 53,  27),   S( 17,  -2),   S(-23, -21),   S(-38, -39),   S(  3,  -2),

            /* queens: bucket 5 */
            S( 30,  32),   S( 51,  21),   S(  1,   3),   S(-12,  34),   S( 42,  -9),   S( 32,  21),   S( 34,  11),   S( 20,   6),
            S( 48,  34),   S( 30,   8),   S(-13,  -8),   S( -4,  43),   S( -9,  56),   S(-36,   5),   S( 50,  26),   S( 18,  22),
            S( 19,   4),   S( 47, -19),   S( 15,  35),   S(-44,  15),   S( 17,  25),   S( 24,  10),   S( 21,  49),   S( 19,  14),
            S( 14,  10),   S( 39,  27),   S( 21,  -7),   S( 59,  55),   S( 49,   3),   S( 57,  39),   S( 18,  31),   S(-22,  13),
            S( 51,  37),   S( 57,   2),   S(  0,   0),   S(  0,   0),   S( 28,  42),   S( 39,  57),   S( 66,  71),   S( -8,  11),
            S( 55,  56),   S( 58,  47),   S(  0,   0),   S(  0,   0),   S( 40,  30),   S( 91,  70),   S( 37,  38),   S( 62,  46),
            S( 85,   7),   S( 95,  60),   S( 60,  62),   S( 25,  36),   S(106,  73),   S(104,  59),   S( 65,  80),   S( 29,  13),
            S( 43,  44),   S( 83,  56),   S( 92,  35),   S( 77,  46),   S( 68,  77),   S( 71,  62),   S( 53,  49),   S( 31,  -7),

            /* queens: bucket 6 */
            S( 59,  72),   S(-19,  -6),   S( 36,  16),   S( 14,  -7),   S( -3,  25),   S(-23,  11),   S(  0,  20),   S( 18,  18),
            S( 22,  30),   S( 46,  29),   S( 55,  57),   S( 41,  38),   S( 52,  38),   S( 26,  33),   S(-18,  16),   S( 48,  33),
            S( 18,  56),   S( 77,  37),   S( 63,  53),   S( 44,  30),   S(  9,  29),   S( 54,   9),   S(100,  47),   S( 77,  71),
            S(-10,  27),   S(-32,  31),   S( 61,  47),   S( 95,  45),   S( 60,  35),   S( 29,  20),   S(102,  54),   S(105,  69),
            S(  7,  39),   S(  3,  44),   S( 75,  75),   S( 81,  66),   S(  0,   0),   S(  0,   0),   S( 81,  60),   S(127,  78),
            S( 17,  44),   S( 67,  75),   S( 63,  85),   S( 49,  48),   S(  0,   0),   S(  0,   0),   S(106,  58),   S(128,  71),
            S( 65,  14),   S( 24,  60),   S( 81,  61),   S( 79,  74),   S( 68,  75),   S( 87,  94),   S(123,  66),   S(168,  33),
            S( -3,  30),   S( 66,  35),   S( 74,  68),   S( 89,  71),   S(118,  64),   S(135,  70),   S(141,  46),   S(110,  56),

            /* queens: bucket 7 */
            S(-17,  -8),   S(  1, -16),   S(-12,  26),   S(-15,  16),   S( 10,  17),   S(-21,  20),   S(  7,  25),   S(-24, -27),
            S(-25,   9),   S(-77,  40),   S(  6,  37),   S( -2,  45),   S( -8,  29),   S(  6,  25),   S(  8,  16),   S(-46,  20),
            S(-28,  16),   S(-22,  17),   S(-12,  69),   S( 32,  42),   S( 40,  37),   S( 43,   3),   S( 41, -28),   S( 38,  14),
            S(-47,  19),   S(-12,  18),   S( 14,  46),   S( 59,  47),   S( 85,  76),   S( 73,  22),   S( 72,  11),   S( 69,  26),
            S(  7,   3),   S(-25,  41),   S( 29,  69),   S( 24,  75),   S(124,  22),   S( 94,  33),   S(  0,   0),   S(  0,   0),
            S(-16,  25),   S( 23,  40),   S( -9,  83),   S( 15,  72),   S( 70,  59),   S( 74,  78),   S(  0,   0),   S(  0,   0),
            S(-22,  42),   S(-41,  21),   S(-18,  41),   S( 46,  54),   S( 83,  41),   S(111,  34),   S( 60,  25),   S( 85,  62),
            S( 13,  -7),   S( 25, -11),   S( 36,  21),   S( 37,  18),   S( 74,  50),   S( 51,  40),   S(-27,  67),   S( 97,  43),

            /* queens: bucket 8 */
            S( -9, -22),   S(  6, -10),   S(-21, -42),   S(  3,   0),   S(-10, -24),   S(  5, -10),   S(  5,  -1),   S( -4,  -6),
            S(-19, -29),   S(  9,  -3),   S( 25,   8),   S(  2,   3),   S( 21,  22),   S(-10, -11),   S(-21, -22),   S( -4,  -7),
            S(  0,   0),   S(  0,   0),   S( 15,  10),   S(-16, -40),   S( 19,  19),   S( 10,   6),   S(-12, -11),   S( -3,   0),
            S(  0,   0),   S(  0,   0),   S(-10, -15),   S( 26,  20),   S( 22,  43),   S( -5,  -5),   S( 10,  20),   S( -7, -25),
            S(  6,   4),   S( 14,  25),   S( 17,  18),   S(  9,  14),   S( 10,   8),   S(  4,   6),   S( 13,  12),   S(-23, -21),
            S(  2, -27),   S(  0, -25),   S( 14,  19),   S( -2, -20),   S(  2,  -6),   S( 24,  28),   S( 21,  -2),   S( -4, -11),
            S(-14, -37),   S(  7, -12),   S( 25,  10),   S( 20,  30),   S( 13,   2),   S(  5,  29),   S(  2,  -6),   S(  6,  -2),
            S( 24,  24),   S( 27,   1),   S(  3,  -2),   S( 11,   7),   S( 14,   6),   S(-27, -38),   S( -4,   0),   S(-35, -50),

            /* queens: bucket 9 */
            S( 22,  13),   S( -8,  -8),   S( 22,  19),   S( 26,   5),   S( 16,  -9),   S( -5, -25),   S(-13, -21),   S( -1,  -8),
            S( -7,  -6),   S(  8,   4),   S(  5,  -3),   S( 14,  16),   S(-14, -26),   S(-20, -57),   S( -9,  -6),   S( -6, -23),
            S(-17, -44),   S( -8, -11),   S(  0,   0),   S(  0,   0),   S( 12,  12),   S( 17,  33),   S(-14, -17),   S( 12,   5),
            S( 22,  18),   S(-11, -22),   S(  0,   0),   S(  0,   0),   S(  5,   4),   S( 25,  25),   S( 16,  10),   S(  1,  11),
            S(  0, -15),   S(  4,  13),   S( -1,  -6),   S(-13, -10),   S(-14, -23),   S( 11,  36),   S( 26,  25),   S( 21,   9),
            S( 23,  28),   S(  8,   6),   S( 12,   2),   S( -4, -14),   S( 11,  24),   S( -4,  -9),   S(  6,  -3),   S( -9, -28),
            S(  5, -10),   S( 12,   2),   S( -6,  25),   S( -4,  14),   S(  4,  -3),   S( 39,  38),   S( -2,  -2),   S( 21,  -9),
            S( 18,  -7),   S( 15,  -8),   S(-10, -18),   S(  9,   2),   S( -1,   2),   S( 11,  28),   S(-24, -48),   S( 21,   7),

            /* queens: bucket 10 */
            S( 21,  10),   S( 13,  21),   S( 11,   9),   S( -7, -20),   S(  3, -20),   S( 10,  14),   S( -7, -24),   S(  1, -24),
            S(  5,  -2),   S(  3,  -1),   S( 15,   5),   S(-15, -41),   S(  4,  10),   S( 20,  19),   S(-12, -25),   S(-15, -28),
            S( -3,  -3),   S( -7, -12),   S(  0,  -2),   S(  7,   4),   S(  0,   0),   S(  0,   0),   S(  2,  -5),   S(-11, -17),
            S(-23, -36),   S( 15,  22),   S( 13,  16),   S(  8,  10),   S(  0,   0),   S(  0,   0),   S(  2,  -7),   S( -5, -10),
            S(  9,  22),   S( 30,  36),   S( 15,  16),   S( 39,  61),   S(  6,   2),   S(  9,  16),   S(  3, -27),   S( 42,   9),
            S( -5,  -4),   S(  7,  -2),   S( 33,  34),   S(  1,   7),   S(  0, -10),   S( 20,  26),   S( 29,  28),   S(  3, -11),
            S( 22,  25),   S(  9,  14),   S( 22,  46),   S( 45,  47),   S( 18,  30),   S( 31,  25),   S( 16,  -2),   S( 16,   1),
            S(-20, -49),   S( 30,  48),   S( 18,  13),   S(-36, -33),   S( 14,  10),   S(-10, -13),   S(  5,  15),   S(  9,  10),

            /* queens: bucket 11 */
            S(  0,  11),   S( 12,  11),   S( -6, -14),   S( -9,  -6),   S(  8,  10),   S(-10, -29),   S( -9, -33),   S( -7,  -3),
            S(  1,   6),   S( -6, -17),   S( -7,   8),   S( 13,  32),   S( 41,  38),   S(  9,  -6),   S( 33,  33),   S(  6,   6),
            S( -8,  -7),   S(  1,  -7),   S(-19, -10),   S( -1,   4),   S( 11,   7),   S( -6,  -8),   S(  0,   0),   S(  0,   0),
            S(-27, -26),   S(-10,   0),   S(-12,  21),   S(  5,  13),   S(  1, -11),   S( -4,  -3),   S(  0,   0),   S(  0,   0),
            S( -3,  20),   S( 13,  11),   S(  4,  17),   S(  9,  35),   S( 48,  61),   S( 31,  59),   S( 12,   6),   S(-15, -32),
            S( -5,   1),   S(-14, -17),   S( -8,   6),   S(  5,  25),   S( 26,  42),   S(-14, -11),   S( 20, -11),   S( 12,  -7),
            S(  0,   8),   S( -4,  -8),   S(  2,  -4),   S(  0,   0),   S( 32,  67),   S(  1,  19),   S( -7,   0),   S( 22,  12),
            S(-30, -66),   S(-10, -22),   S(-12, -45),   S(-16, -13),   S( 25,  31),   S(  9,   6),   S( -3,   5),   S( -9, -20),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 11,  22),   S(-27, -22),   S( -1,   8),   S( -7, -22),   S(  7,   4),   S( -1,  -2),
            S(  0,   0),   S(  0,   0),   S(  8,  -3),   S(  3,  -2),   S( 11,  21),   S( -2,  -6),   S( -1,  -6),   S(  3,   1),
            S( -1,  -1),   S( 12,  17),   S( -7,  -6),   S(-16, -36),   S( 32,  61),   S( -9,   8),   S( -3,   1),   S(  2,  -2),
            S( -5, -18),   S(  0,   8),   S(  7,  29),   S( 13,  33),   S( -8,  -2),   S( -5,  -6),   S(  0,  -1),   S( -8, -13),
            S(-18, -27),   S( -5,  -3),   S(  3, -13),   S( -4, -20),   S(  5,  16),   S(-13, -35),   S( -5, -17),   S(-10, -17),
            S( 14,  17),   S( -8, -14),   S(  9,  14),   S( -2, -12),   S(-18, -37),   S(-21, -12),   S(-17, -14),   S(-10, -17),
            S(-17, -35),   S(  0,  -2),   S(-11, -10),   S(  4,   2),   S( -4,  -6),   S(-17, -24),   S(  5,  10),   S( -8, -28),
            S(  7,   5),   S(  1,  -8),   S( 15,   5),   S( -7,  -5),   S(-11, -22),   S( -8, -12),   S(-14,  -5),   S( -5, -12),

            /* queens: bucket 13 */
            S(-11, -28),   S(-12, -31),   S(  0,   0),   S(  0,   0),   S(-12,  -8),   S( -8, -22),   S(  7,  10),   S( -6, -21),
            S(-14, -45),   S( -1, -11),   S(  0,   0),   S(  0,   0),   S(-15, -26),   S(-14, -18),   S(  0,  -8),   S(  2,   1),
            S(-14, -22),   S(  0,   6),   S(  2,   6),   S(  0,  -9),   S(-22, -44),   S(-10, -14),   S( -5,   6),   S(  6,  13),
            S( -7, -27),   S( -3,  -6),   S(  6,   9),   S( 18,  24),   S( 16,  20),   S( 10,  21),   S( 12,  23),   S( -8,  -7),
            S(  1, -21),   S( -1, -22),   S(  0,  -1),   S( 24,  48),   S( -8,  -9),   S( -6, -12),   S(-17, -34),   S( -5, -29),
            S( -9, -12),   S(-20, -30),   S( 12,   1),   S( 17,  32),   S(  2,  14),   S( -5,  -7),   S(-13, -25),   S(-15, -32),
            S( -6,  -6),   S(  1, -10),   S( 22,  43),   S(  1,  -5),   S( -8, -25),   S(  9,  -9),   S(-13, -24),   S( -9, -14),
            S(-12,  -7),   S(-13, -25),   S( -4,  -9),   S( -4, -17),   S( -5, -22),   S( -5,   1),   S( -3, -15),   S(-30, -47),

            /* queens: bucket 14 */
            S(  1,   4),   S(  3,   8),   S(  0,  -3),   S(  4,  -2),   S(  0,   0),   S(  0,   0),   S(  5,   8),   S( -7, -14),
            S(-11, -28),   S(-11, -23),   S(  1, -12),   S(  4,  -8),   S(  0,   0),   S(  0,   0),   S(  0,   2),   S(-10, -20),
            S(  1,   5),   S(  0, -16),   S( -6, -13),   S( -3,  -8),   S(  5,  13),   S( -1,   2),   S( -7,  -9),   S(  1, -10),
            S( -7,   0),   S( -1,  -2),   S( -2,   3),   S(-18, -25),   S(-11, -10),   S(-20, -24),   S(-10, -27),   S(  0,  -7),
            S(-10, -10),   S( -3,   1),   S(-17, -21),   S(  2,  12),   S(  4,  18),   S(-16, -31),   S(  2,  14),   S(-15, -24),
            S(-10, -22),   S(  7,   8),   S(-13, -15),   S(-12, -19),   S(  1,   5),   S( -8, -14),   S( -5, -12),   S(-23, -37),
            S(-11, -11),   S( -8, -21),   S( -3,  -4),   S(  7,  10),   S(  0,  -4),   S(  8,   7),   S(-14, -26),   S(-13, -25),
            S(-19, -28),   S( 17,   3),   S(-11, -11),   S( -5, -14),   S(  4,  -2),   S( -6,  -4),   S( -4,   2),   S(  6,  -1),

            /* queens: bucket 15 */
            S(  0,   0),   S(-11, -20),   S(  6,   7),   S(-15, -14),   S(  4,   2),   S( -9,  -5),   S(  0,   0),   S(  0,   0),
            S( -4,  -3),   S( -3,  -5),   S(-13, -15),   S( 10,  15),   S(  1,   2),   S(  1,   3),   S(  0,   0),   S(  0,   0),
            S( -3,   2),   S( -4, -16),   S( -9,   0),   S( -1,   3),   S( -1,  -9),   S( 11,  19),   S(  1,   4),   S( -1,  -4),
            S( -5, -10),   S(-13, -20),   S( -7, -13),   S(  9,  17),   S(  4,  14),   S( 20,  62),   S( -7,   0),   S( -1,   5),
            S(  2,   2),   S(  4,  13),   S(-15, -24),   S(-12,  -7),   S( 17,  45),   S( -9,  -9),   S(  2,   6),   S( -2,  -5),
            S( -6, -16),   S( -4,  -2),   S( -6,  -8),   S(-26, -42),   S(  0,  13),   S(-17, -14),   S(-12, -18),   S( -6,  -3),
            S( -6, -19),   S(  4,   8),   S(-12, -12),   S(  5,  -2),   S( -6,  -7),   S( -4,  -1),   S(  3,   3),   S( -9, -11),
            S( -8, -12),   S(-13, -30),   S( -3,  -8),   S(  1,  -1),   S( -7,  -3),   S( -8, -13),   S(  0,  -8),   S( -2,   7),

            /* kings: bucket 0 */
            S( -6, -21),   S( 17,  10),   S(  3,  -1),   S(-19,   2),   S( -7, -13),   S( 19, -12),   S( -6,  13),   S(  9, -59),
            S( -8,  21),   S( -2,  -8),   S(  9,  -1),   S(-51,  27),   S(-31,  36),   S(-22,  25),   S(-24,  50),   S(  4,  29),
            S(  2,  -3),   S( 75, -31),   S(-15,  -1),   S(-13,  -3),   S(-12,  -1),   S( 15,  -7),   S(-24,  16),   S( 31, -37),
            S(-34, -18),   S(  5, -28),   S(  6, -27),   S(-32,  17),   S(-65,  45),   S(-48,  16),   S(-26,  37),   S(-21,  16),
            S(-44, -136),  S( -2, -36),   S(-34,  -8),   S( 20, -35),   S(-55, -13),   S(-31,   7),   S( -6,   1),   S( -9,   2),
            S(  0, -116),  S(  3, -11),   S( -4, -45),   S(-27, -14),   S(  1, -11),   S(-40,  13),   S(  9,  19),   S( -1,  16),
            S(  0,   0),   S(  0,   0),   S(  2, -43),   S( -6, -23),   S(-22,  18),   S(-20,  -5),   S( -9,  22),   S(-12,  -5),
            S(  0,   0),   S(  0,   0),   S(-15, -22),   S(  0, -21),   S( -2, -25),   S(  3,  27),   S(  7,   6),   S(  5,  -9),

            /* kings: bucket 1 */
            S( 16, -30),   S( 33, -17),   S( 11, -12),   S( 13,   6),   S( -9,   4),   S( 26, -12),   S( -3,  14),   S( 14, -21),
            S( 11,  -2),   S( -3,  20),   S( 24, -17),   S(-51,  23),   S(-18,  14),   S( -3,   7),   S(  6,  10),   S( 12,  -1),
            S(-10,   3),   S( 16, -13),   S(  1, -19),   S( 15, -20),   S(-36,  -1),   S( 18, -21),   S( 47, -21),   S( 36, -10),
            S(  1, -23),   S( 27, -28),   S( -6,  -1),   S(  0,   8),   S( 18,  11),   S(  4,   1),   S( -2,   6),   S(-26,  31),
            S(-11, -42),   S( -9, -57),   S(-18, -53),   S(-39, -26),   S(  0, -14),   S(-23, -27),   S(-14,  -9),   S( -3,   9),
            S(-32,  -6),   S(-92,   9),   S(-32,  23),   S( 10,   8),   S(-40,   4),   S(-32,   5),   S( 15,   9),   S( -2, -18),
            S(-33, -14),   S(-18, -15),   S(  0,   0),   S(  0,   0),   S(-34,  27),   S(-56,  11),   S(-16,  48),   S(-11, -35),
            S(-36, -94),   S(-29, -25),   S(  0,   0),   S(  0,   0),   S( -4,  11),   S(-13,  25),   S( -5,  14),   S(  1, -46),

            /* kings: bucket 2 */
            S( 17, -50),   S( 14,  -2),   S(  9, -20),   S( 21,  -8),   S(-12,  11),   S( 38, -26),   S( -6,  24),   S( 19, -31),
            S( 18, -24),   S( -9,  27),   S( -8,   9),   S(-18,   9),   S(-19,  14),   S(-15,   4),   S( 11,  -1),   S( 11,  -2),
            S(-38, -14),   S( -5, -11),   S( -3, -12),   S(-31,  -9),   S( -7,   2),   S( 11, -24),   S( 37, -17),   S( 22, -18),
            S(  5,  25),   S(-23,  16),   S(  6,   3),   S(-31,  20),   S( 25,  -4),   S( -3,  -6),   S( 37, -26),   S( 38, -14),
            S( -1,   5),   S(  8,   4),   S( 47, -50),   S(  1, -18),   S( 23, -47),   S(-18, -49),   S( 31, -57),   S( 30, -55),
            S(  4,  -7),   S(-10, -10),   S(-21, -16),   S(-43,  -2),   S( -4,  13),   S(-15,  42),   S(-69,   8),   S(-19, -13),
            S(-14, -19),   S( -9,  21),   S(-80,  -2),   S( -6,  26),   S(  0,   0),   S(  0,   0),   S( -7,  16),   S(-46, -35),
            S( -3, -40),   S(-18, -25),   S(-37, -46),   S(  6,  16),   S(  0,   0),   S(  0,   0),   S(-10, -28),   S(-36, -131),

            /* kings: bucket 3 */
            S(  6, -67),   S( 17,  -2),   S( 17, -18),   S( -2,  -9),   S( -6, -10),   S( 41, -30),   S( -3,  20),   S(  4, -31),
            S( 13,   8),   S(-20,  36),   S( -1,   2),   S(-38,  18),   S(-45,  29),   S(  4,  -2),   S( -4,  16),   S( 13,   6),
            S( 28, -35),   S(  9,  -4),   S(-11,  -4),   S(-47,   5),   S(-11,  14),   S( 33, -20),   S( 58, -24),   S( 61, -17),
            S( 11,  10),   S(-114,  56),  S(-46,  13),   S(-30,   9),   S(-57,  19),   S(-34, -11),   S(-21,  -6),   S(-42, -12),
            S( -1,  20),   S( -8,  -9),   S(-24, -20),   S(-24,  -9),   S( 31, -47),   S( 53, -59),   S( 23, -65),   S(-31, -70),
            S(-11, -16),   S( 25,  -2),   S( -9,  -1),   S( 13, -31),   S( 30, -31),   S( 82, -49),   S( 69, -12),   S( 56, -121),
            S(-22,  11),   S( 37,  10),   S( 19,  -7),   S( 23, -36),   S( 50, -54),   S( 45, -42),   S(  0,   0),   S(  0,   0),
            S( -6, -27),   S(  8, -11),   S(  3,  42),   S(  0, -13),   S( -7, -61),   S( -5,  -2),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-55,  16),   S( 11,  32),   S(-13,  40),   S(  0,  -1),   S( -1,  10),   S(-12,   7),   S(  5,  -3),   S( 15, -20),
            S(-36,  12),   S( 28,  20),   S(-28,  26),   S(  6,  -4),   S( 47, -14),   S( 28, -19),   S( 48, -17),   S(  0,  12),
            S( -8,  23),   S(  0, -19),   S( 17,  -5),   S( -7,   3),   S( -1,   9),   S( 16, -28),   S(-45,  15),   S( 31, -10),
            S( 10, -20),   S( -3,  16),   S(  3,   4),   S( -1,  10),   S(  5,  13),   S(-25,  22),   S( 31,  -3),   S( 14,   7),
            S(  0,   0),   S(  0,   0),   S( -5,   4),   S(-41,   8),   S(-69,  28),   S(-18, -12),   S(-21,  -5),   S( 12, -20),
            S(  0,   0),   S(  0,   0),   S( -5,   3),   S(-16,  38),   S(-13,  24),   S( -9,   3),   S( -3,  10),   S(-24,  19),
            S(  0,  -3),   S(  3, -17),   S( -4, -21),   S(-16,  23),   S( -2,   6),   S(-20, -21),   S(  0,  29),   S(  7,   7),
            S( -9, -28),   S(  6,  -1),   S(-13, -48),   S(  0,  13),   S( 12,   5),   S(-11, -13),   S(-18, -15),   S(  0,   2),

            /* kings: bucket 5 */
            S( 48,  -4),   S( -1,  -2),   S(-17,  14),   S(-33,  22),   S(-15,  19),   S(  0,   5),   S( 39,  -3),   S( 37, -23),
            S( -5,  -3),   S( 12,  20),   S( 16,   4),   S( 19,  -4),   S( 34, -11),   S( 39, -14),   S( 21,   8),   S( 39, -10),
            S(  1,   7),   S( -4,  -9),   S(  5, -11),   S(  0,  -8),   S( -7,   9),   S(-40,   1),   S(-23,  11),   S( 20,  -4),
            S( -9,   1),   S(  0, -10),   S(  2,   2),   S( -6,  17),   S( 37,   6),   S(-17,  11),   S( 35,   4),   S( 18,   4),
            S(-16, -30),   S(-20, -48),   S(  0,   0),   S(  0,   0),   S( -1, -12),   S(-24,  -8),   S(  9, -11),   S( -5, -14),
            S(-20, -37),   S(-45, -26),   S(  0,   0),   S(  0,   0),   S(-21,  30),   S(-43,  -1),   S(-28,   6),   S(-20,   1),
            S(-13, -31),   S(-29,  21),   S(  4,   7),   S(  6,   4),   S(-24,  37),   S(-49,  17),   S(-15,  14),   S( 10,  -2),
            S(-14, -99),   S(-10,  32),   S(-17, -27),   S(  3, -15),   S( -6,  -6),   S( -4,   2),   S(-10,  -5),   S( -1,  11),

            /* kings: bucket 6 */
            S( 10, -18),   S( 26, -14),   S(  4,  -5),   S(-26,  24),   S(-24,  26),   S(-22,  14),   S(  3,  18),   S( 21,  -6),
            S( 34, -20),   S( 39,  -2),   S( 18,  -6),   S(  0,  -2),   S( 24,  -5),   S( -9,   5),   S( 22,  -4),   S( 11,  -4),
            S(  6, -23),   S(-45,  10),   S(-18,  -8),   S(-35,  -1),   S( 15,  -7),   S(-55,   3),   S(  9,  -1),   S( -2,   7),
            S( -1,  18),   S( 45, -19),   S( 30, -12),   S( 54,  -7),   S( 79,  -8),   S(-38,  11),   S(-10,  11),   S(  8,   0),
            S( 23,  -9),   S( 24, -34),   S(-39,  -9),   S( 11, -15),   S(  0,   0),   S(  0,   0),   S(-59, -20),   S(-63, -16),
            S(-25, -17),   S( -9,   3),   S(-25,  -9),   S(-22, -25),   S(  0,   0),   S(  0,   0),   S(-25, -22),   S(-37, -29),
            S(-16, -14),   S(-26,  18),   S(-41,  13),   S(-17,   7),   S(-10,  18),   S( -2, -37),   S(-36, -24),   S(-11, -46),
            S( -1,  -1),   S(  9,  10),   S( -3,  27),   S(-14, -27),   S(  5, -23),   S( -5, -20),   S(  1,  11),   S(  3, -62),

            /* kings: bucket 7 */
            S( 19, -15),   S( -6, -19),   S( -4, -25),   S(  2,   2),   S(-17,  10),   S(-35,  30),   S(-46,  40),   S(-42,  23),
            S(  5,  16),   S( 23, -17),   S(-35,  20),   S(-20,   0),   S(-28,   8),   S(-54,  28),   S(  0,  -4),   S(  0,   8),
            S( 48, -46),   S(-40,   8),   S(-13,  -3),   S(-25,  -3),   S(-52,  16),   S(-54,  21),   S(  1,   1),   S(-29,  11),
            S(-30,  16),   S( -8,  14),   S(-25,  -2),   S( 15,  -7),   S( 27, -10),   S( 72, -40),   S( 13,  -8),   S( 16,  -6),
            S(-31,  16),   S(-26,   9),   S(-10, -24),   S(  9, -18),   S( 22, -27),   S( 27, -20),   S(  0,   0),   S(  0,   0),
            S(-13, -20),   S(-13,   2),   S( 26,  -1),   S(  0, -19),   S( 14, -20),   S( 11, -15),   S(  0,   0),   S(  0,   0),
            S( 12,  24),   S(  0, -15),   S( -1,  22),   S(-25, -40),   S(  3, -21),   S(-11, -33),   S(  4,  -7),   S(-10,   5),
            S( 12,  14),   S(-10,   2),   S( 20,  24),   S( -2, -16),   S( 13,  35),   S(-19, -47),   S(  0, -26),   S( -6, -57),

            /* kings: bucket 8 */
            S( 29, 117),   S(-24,  87),   S( 22,  57),   S(-12,   0),   S( -4,  14),   S(-27, -23),   S( 18, -16),   S( -6,  -7),
            S( 17,  80),   S( 23,  20),   S( 47,  51),   S( 86,  -9),   S( -4,  36),   S( 33, -21),   S( -7,  10),   S(-11,  20),
            S(  0,   0),   S(  0,   0),   S( 33,  63),   S( 65,  12),   S( 23,   6),   S( -4,  -9),   S( 17,   9),   S( 16, -28),
            S(  0,   0),   S(  0,   0),   S(  8,  83),   S( -7, -16),   S(-24,  41),   S(-11,  22),   S( 18,   2),   S( 32,  11),
            S( -8, -27),   S(  3,  25),   S(  5,  -1),   S(  3,  23),   S( -2,   7),   S( 17, -17),   S(-11,  22),   S(  0, -19),
            S(  4,   5),   S(  1,  -3),   S( -3,   1),   S(-17, -25),   S(-16,  -7),   S(-10, -28),   S(-13, -18),   S(  0,  -4),
            S( -9, -35),   S( -9, -36),   S( -3,   2),   S(  0,   1),   S( -5, -31),   S( -5,  17),   S( -3,  -7),   S(  6, -29),
            S( -7, -19),   S( -9, -32),   S( -1,  -7),   S( -5, -20),   S( 13,  28),   S( -6,   9),   S( -4,   1),   S(  7,  10),

            /* kings: bucket 9 */
            S(  4,   4),   S( -8,  29),   S(-15,  49),   S( 11,  10),   S(-19,  42),   S(-36,  21),   S( 62,   0),   S( 27,  11),
            S( -8,  47),   S( 56,  20),   S( 21,  -2),   S( 62,   6),   S( 51,  23),   S( 15,   4),   S(-12,  37),   S(-31,  10),
            S( -2,  -2),   S( 39,   9),   S(  0,   0),   S(  0,   0),   S( 42,  27),   S(  8,  -3),   S( 30,  -4),   S(-13,  21),
            S( 18, -40),   S( 11, -22),   S(  0,   0),   S(  0,   0),   S(-10,  37),   S( 35,   3),   S(-27,   9),   S(-17,  41),
            S(  5, -12),   S( 14,  15),   S(  7,   8),   S( -1,  29),   S( -7,  13),   S(-38,  17),   S(-26,  23),   S(  3, -23),
            S( -5,   2),   S( -2,  -3),   S( 13,  -4),   S(-18, -33),   S(-10, -18),   S(-20,  26),   S(-34,  -7),   S( -3,  30),
            S(  6,  18),   S( -2, -42),   S(  4,   8),   S(  4, -26),   S(  9, -24),   S( 18,  50),   S(-13, -25),   S( 10,   9),
            S(  8,  12),   S(-19, -39),   S( 11, -25),   S( -4, -28),   S( -7, -28),   S(-16,  -9),   S( -9,  16),   S(  9,  13),

            /* kings: bucket 10 */
            S( 36,   4),   S(-11,   3),   S( 25,  10),   S(  1,  23),   S( 10,  18),   S(-84,  43),   S(-27,  43),   S(-85,  79),
            S( 21, -11),   S( 78,   7),   S(  7,  -3),   S( 59,   5),   S( 48,   6),   S( 41,   3),   S( 10,  28),   S(-96,  48),
            S( 30,  -7),   S( 19,  -7),   S( 28,  -8),   S( 10,  20),   S(  0,   0),   S(  0,   0),   S(-26,  36),   S(-57,  36),
            S( 24,  -8),   S( 38, -22),   S( 28, -11),   S( 23,  -8),   S(  0,   0),   S(  0,   0),   S(  7,  11),   S(  5,  -4),
            S(  1,  -5),   S( 10,  21),   S( 45, -15),   S( 17, -37),   S(  3,  -5),   S( 10,  21),   S( 10,  10),   S(  9,   4),
            S(  0,  18),   S(  0,  10),   S(  9,  -4),   S(  3,  18),   S(  4,   0),   S(-16, -17),   S(-10,  -1),   S( -7,  -5),
            S( -5, -73),   S( -3,  10),   S( 13, -17),   S( 10, -22),   S( 18,  -9),   S(-15, -29),   S( 12, -56),   S(  7,   8),
            S(  0,   6),   S( 17, -23),   S(  5, -11),   S(-12,  -2),   S(  6,  -8),   S( -2, -19),   S( -4,  -4),   S(  8,  11),

            /* kings: bucket 11 */
            S(-21, -24),   S( 16, -11),   S( 11, -31),   S(-10,  18),   S(-13,  10),   S(-97,  63),   S(-39,  72),   S(-132, 147),
            S( -1, -13),   S( 13,  10),   S(-24, -17),   S( 28,  37),   S( 76,   1),   S( 59,  45),   S( 35,  20),   S( 28,  34),
            S( 13, -42),   S(  1,  13),   S( -4,  -4),   S( 30,   5),   S( 89,   6),   S( 43,  68),   S(  0,   0),   S(  0,   0),
            S(-10,  27),   S( 31,  -2),   S(  4,   3),   S( 13,  13),   S( 26,  -5),   S( 25,  17),   S(  0,   0),   S(  0,   0),
            S( 15,  38),   S(-20,  -5),   S( 24,  -7),   S(  8,   1),   S( -1,  13),   S( 10,  15),   S(  1,  13),   S( -2, -14),
            S(  8,   3),   S( 23, -19),   S( 12, -25),   S( 17,   9),   S( -1, -22),   S( -9, -13),   S(  0,   3),   S(  0,  -2),
            S( -2, -34),   S( 12,   8),   S( 30,  12),   S( -1, -27),   S(  4, -28),   S( -5,  10),   S( -9, -19),   S( -8, -17),
            S(  1,  -2),   S(  3, -13),   S(-17, -14),   S(  2, -11),   S(-16, -36),   S( -2, -11),   S(  1,   0),   S(  4,  10),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 17,  33),   S( 11,  -1),   S( -5, -14),   S(  9,   6),   S(  1, -31),   S( -8,  40),
            S(  0,   0),   S(  0,   0),   S( 65, 110),   S( 21,   4),   S( 11,  50),   S( 29,  18),   S(  4, -14),   S(-16,  21),
            S( -3,   4),   S(  0,   3),   S( 21,  65),   S( 52,  30),   S(  2, -20),   S( 13,   3),   S( 20, -26),   S(-33, -16),
            S( -3,   9),   S(  4,  33),   S( -8, -17),   S( -1,   0),   S(  1, -13),   S( 18,  16),   S(  1,  27),   S( -3,  -8),
            S( 15,  44),   S(  1,  12),   S(  9,   6),   S( -2,  54),   S(  2,  36),   S( -2,  23),   S( -6,  14),   S(-14,   1),
            S(  8,  18),   S( 11,  47),   S( -4,  -8),   S(  4,  -2),   S(  2,  -1),   S(-12,  25),   S( -4,  17),   S( 11,   9),
            S( -2,  -2),   S(  3,  10),   S(  2,   1),   S( -4,  17),   S( -3,  -9),   S( -7,  -6),   S( 12,  35),   S(-16, -51),
            S( -5,  -9),   S(  4,   1),   S(  0,   0),   S( -2,  -2),   S( -3, -13),   S( -1, -21),   S( -5, -11),   S( -6, -21),

            /* kings: bucket 13 */
            S(  1,  45),   S(  0,  17),   S(  0,   0),   S(  0,   0),   S( 43,  13),   S( 13,  -3),   S(-10, -22),   S( -6,  29),
            S( 15,  27),   S(  3,  20),   S(  0,   0),   S(  0,   0),   S( 47,   2),   S( 16,  -5),   S(-14,   3),   S(-15,   7),
            S(  4,  -4),   S( 24,  23),   S( -3, -20),   S( 17,  14),   S( 42, -31),   S( 25,  -4),   S( -2, -11),   S( 25,   9),
            S( -8,  -4),   S( 13,  19),   S(  5,  18),   S(-14,  18),   S( -4,  15),   S( 12,   1),   S( -9,  15),   S( -7,  -2),
            S(  2,  -5),   S( -1, -16),   S( 12,  53),   S(-25,   4),   S( -4,  16),   S(  2,  21),   S(-12, -16),   S(  5,  34),
            S(  8,  12),   S( -1,  32),   S(  0,  18),   S( -2,  -4),   S( -7,   4),   S( -8,  11),   S(-16,  21),   S(  8,   2),
            S(  5,  10),   S( -4, -11),   S(-10, -51),   S( 10,  36),   S(-10,  -7),   S( -2,   7),   S(-10, -28),   S(  3,  19),
            S(  1,  -6),   S(  4,  -9),   S(  2,  17),   S( -2,  -2),   S( -1,   2),   S(-18, -11),   S( -5,  12),   S(  7,  21),

            /* kings: bucket 14 */
            S( 24,  31),   S(  8,  13),   S( 15,  -4),   S( 10,  -7),   S(  0,   0),   S(  0,   0),   S( 11,  65),   S(-48,  37),
            S(-13, -26),   S( 23, -32),   S( 49, -45),   S( 34, -16),   S(  0,   0),   S(  0,   0),   S(  4,  31),   S(-50,  29),
            S( 13,  35),   S( 10,  -4),   S( 32, -28),   S( 40, -19),   S(  8, -18),   S(  7,  23),   S( 21,  65),   S(-20,   3),
            S( -8,  -6),   S(  7,  -2),   S(-13, -21),   S( 24,  18),   S(-18,  -3),   S( 14,  58),   S(  8,  13),   S(  5, -16),
            S( -5, -23),   S(  9,   3),   S(  0,   6),   S(-13, -19),   S(  9,  27),   S(  0,  19),   S( 20,  78),   S( 10,   5),
            S(  3,  10),   S( -4, -13),   S( -2,   5),   S(  9,  18),   S(-14,  15),   S( -3,   5),   S(-11, -29),   S(  0,   8),
            S( 11,  30),   S(-11, -32),   S( 10,   7),   S( 22,  -2),   S(  1, -17),   S( -5,  32),   S(-23, -12),   S( 10,  34),
            S(  7,  25),   S(  7,  27),   S( -1,  -8),   S(  1,  11),   S(  5, -13),   S( -1,  -3),   S(-15, -21),   S( -2, -20),

            /* kings: bucket 15 */
            S( 10,  33),   S(  2,  14),   S( 23, -10),   S( -8,   2),   S(-21, -11),   S(  3,  97),   S(  0,   0),   S(  0,   0),
            S( -2, -26),   S(  0, -35),   S(-13, -32),   S( 30,  53),   S( 35, -18),   S( 79, 112),   S(  0,   0),   S(  0,   0),
            S(-17, -14),   S( 19, -12),   S( 14, -24),   S( -1,   0),   S(  5,  -5),   S( 20, 100),   S( 11,  29),   S(-16,   1),
            S(  4,  -7),   S(  6,  10),   S( 23,  30),   S(-20, -24),   S( -2,  -4),   S( 20,  28),   S( 17,  66),   S(  1, -12),
            S( 11,  -8),   S(  1,  37),   S(  0,   3),   S(-11, -49),   S(-13, -14),   S( -9,  33),   S(  8, -10),   S(  2,  22),
            S( -1,  27),   S(-17, -31),   S(  2,   9),   S( 13,  26),   S( -8,  -8),   S(  8,  31),   S( -5,   3),   S(  7,  11),
            S(  8,  27),   S(  0,  24),   S( -3, -21),   S(  6, -11),   S(  8,  13),   S(  7,   1),   S( -6,  -6),   S(  7,   4),
            S(  1,  -3),   S(  7,   2),   S( -5,   2),   S(  7,   7),   S(  7,  -3),   S(  7,   8),   S( -2,  -5),   S(  1,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-21, -153),  S(  3,   6),   S( 15,  76),   S( 22, 113),   S( 29, 134),   S( 32, 154),   S( 38, 152),   S( 45, 141),
            S( 58, 118),

            /* bishop mobility */
            S(  3, -46),   S(  7,  28),   S( 17,  68),   S( 23, 101),   S( 27, 128),   S( 29, 149),   S( 32, 156),   S( 35, 157),
            S( 39, 158),   S( 45, 154),   S( 51, 142),   S( 74, 133),   S( 77, 130),   S( 61, 121),

            /* rook mobility */
            S(-13,  35),   S( -6, 101),   S( -7, 135),   S( -3, 146),   S( -5, 168),   S(  0, 175),   S(  2, 186),   S(  6, 185),
            S(  6, 195),   S(  8, 201),   S( 16, 203),   S( 27, 199),   S( 33, 200),   S( 45, 197),   S( 75, 179),

            /* queen mobility */
            S( 77, -22),   S( 79, -52),   S( 68, 114),   S( 63, 180),   S( 64, 221),   S( 64, 258),   S( 63, 300),   S( 63, 327),
            S( 65, 344),   S( 65, 361),   S( 62, 385),   S( 58, 406),   S( 60, 415),   S( 57, 427),   S( 58, 434),   S( 59, 443),
            S( 56, 443),   S( 62, 440),   S( 75, 429),   S( 85, 415),   S(125, 384),   S(135, 372),   S(158, 348),   S(142, 350),
            S(177, 327),   S(157, 325),   S(141, 250),   S(118, 213),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  19),   S(-21,  59),   S(-35,  51),   S(-42,  74),   S( 16,  22),   S( -5,  22),   S(  6,  70),   S( 30,  35),
            S( 16,  38),   S(  4,  51),   S(-19,  57),   S( -9,  46),   S( 11,  36),   S(-26,  51),   S(-27,  74),   S( 39,  35),
            S( 23,  71),   S( 13,  78),   S(  4,  57),   S( 20,  50),   S(  3,  53),   S(-34,  75),   S(-28, 105),   S(-10,  85),
            S( 30, 112),   S( 41, 131),   S( 25,  84),   S(  8,  57),   S(  5,  68),   S( -2, 103),   S(-46, 137),   S(-85, 158),
            S( 37, 156),   S( 61, 192),   S( 56, 142),   S( 38, 114),   S(-47, 108),   S( 34, 112),   S(-41, 188),   S(-66, 181),
            S( 88, 238),   S( 90, 271),   S(134, 261),   S(135, 258),   S(139, 268),   S(141, 247),   S(129, 266),   S(130, 266),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,   6),   S(  6,  -3),   S( 11,  -5),   S(  9,  25),   S( 26,  38),   S(  2, -24),   S(-13,  33),   S( 11, -32),
            S( -9,  30),   S( 35,  -2),   S( -3,  48),   S( 20,  45),   S( 37,  -2),   S( -2,  44),   S( 18,  -4),   S( 11,   9),
            S( -8,  22),   S( 16,  10),   S(  0,  56),   S( 16,  75),   S( 24,  32),   S( 25,  32),   S( 31,   0),   S( -3,  27),
            S( 24,  34),   S( 13,  51),   S( 36,  99),   S( 19, 102),   S( 65,  61),   S( 72,  62),   S( 11,  62),   S( 39,  24),
            S( 64,  60),   S(123, 108),   S( 98, 158),   S(127, 174),   S(140, 142),   S(130, 161),   S(158, 138),   S( 87,  65),
            S( 80, 205),   S(111, 292),   S( 89, 242),   S(100, 195),   S( 68, 135),   S( 57, 159),   S( 53, 167),   S( 20,  89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 23,  29),   S( 27,  30),   S( 44,  49),   S( 40,  43),   S( 29,  35),   S( 32,  38),   S(  8,  26),   S( 49,  13),
            S( -4,  27),   S( 16,  42),   S( 13,  43),   S(  9,  59),   S( 26,  22),   S(  7,  27),   S( 26,  26),   S(  2,  12),
            S(  0,  15),   S( 20,  41),   S( 48,  56),   S( 37,  46),   S( 37,  55),   S( 69,  11),   S( 21,  21),   S( 23,   2),
            S( 54,  66),   S( 93,  61),   S(116, 108),   S(139, 128),   S(140, 111),   S( 69, 132),   S( 63,  54),   S( 89,  -6),
            S( 48, 128),   S( 88, 140),   S(145, 219),   S(130, 256),   S(108, 281),   S( 82, 246),   S(155, 213),   S(-53, 196),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  28),   S(  9,  20),   S( 13,  34),   S( -7,  46),   S( 65,  20),   S( 19,   7),   S(  2,  -5),   S( 29,   7),
            S(  2,  15),   S(  8,   7),   S( 18,  17),   S( 15,  35),   S( 13,  20),   S( -2,  11),   S(  3,  10),   S( 29,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -15),   S( -8,  -7),   S(-18, -17),   S(-15, -35),   S(-13, -20),   S(  2, -11),   S( -3, -10),   S(-29,   4),
            S(-25, -28),   S( -9, -20),   S(-13, -34),   S(  7, -46),   S(-65, -20),   S(-19,  -7),   S( -2,   5),   S(-29,  -7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -30),   S(  1, -30),   S( -4, -35),   S(-54, -14),   S(-13, -26),   S(-15, -32),   S(  5, -33),   S(-14, -40),
            S(-15, -10),   S( -9,  -9),   S(-24,   7),   S(  5, -21),   S(-30, -19),   S(-26,  -4),   S(-34,  -6),   S( -6, -22),
            S(-17, -17),   S( -6, -23),   S(-25,  -1),   S(-29,  -4),   S(-23, -33),   S(-27, -12),   S(-11, -15),   S(-43, -24),
            S( -4, -31),   S( 22, -47),   S( 12, -17),   S(  9, -33),   S(  2, -23),   S( 61, -44),   S( 51, -52),   S( -9, -51),
            S(  9, -40),   S( 44, -71),   S( 53, -38),   S( 61, -23),   S( 65, -41),   S( 53, -15),   S(120, -82),   S( 34, -76),
            S( 93, -94),   S(111, -105),  S( 89, -58),   S( 70, -21),   S( 88, -31),   S(130, -14),   S( 96, -39),   S( 33, -72),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   5),        // attacks to squares 1 from king
            S( 14,   6),        // attacks to squares 2 from king

            /* castling available */
            S( 63, -75),        // king-side castling available
            S( 22,  50),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 43, -73),   S( 49, -77),   S( 41, -87),   S( 29, -74),   S( 22, -67),   S( 10, -63),   S(-11, -52),   S(-20, -44),
            S( -7, -56),   S( 19, -55),   S( 38, -59),   S( 11, -46),   S( 67, -68),

            /* orthogonal lines */
            S(-25, -132),  S(-82, -97),   S(-108, -82),  S(-123, -81),  S(-128, -89),  S(-132, -93),  S(-133, -99),  S(-128, -104),
            S(-144, -98),  S(-158, -101), S(-173, -115), S(-138, -138), S(-136, -141), S(-50, -175),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 16, 222),

            /* passed pawn can advance */
            S(-10,  35),   S( -6,  60),   S(  0, 109),   S( 33, 188),

            /* blocked passed pawn */
            S(  0,   0),   S( 40, -19),   S( 34,  -8),   S( 27,  35),   S( 32,  62),   S( 26,  40),   S( 97,  70),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 48, -61),   S( 48,  15),   S( 27,  25),   S( 18,  65),   S( 24, 101),   S(141, 121),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-43,  -9),   S(-11, -32),   S(  5, -36),   S(-31, -12),   S(-27,   9),   S(101,  10),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 48, -59),   S( 29, -19),   S(  8, -18),   S( 14, -49),   S( -2, -127),  S(-57, -201),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 25,  48),   S( 38,  32),   S(126,  32),   S( 39,  20),   S(189, 115),   S(126, 113),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 19,  54),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-48, 142),

            /* bad bishop pawn */
            S( -7, -15),

            /* rook on open file */
            S( 41,   7),

            /* rook on half-open file */
            S( 10,  41),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 25,  37),   S( 25,   3),   S( 37,  22),   S( 28,  -1),   S( 28, -18),

            /* pawn threats */
            S(  0,   0),   S( 67, 107),   S( 53, 126),   S( 69,  95),   S( 56,  46),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 33,  64),   S( 49,  51),   S( 71,  44),   S( 47,  69),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 21,  60),   S( 24,  59),   S( -4,  42),   S( 69,  49),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 15,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
