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
        public const int MAX_WEIGHTS = 12793;
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
        public const int BACKWARD_PAWN = 12680;     // backward pawn
        public const int KING_ATTACK = 12681;       // attacks near king
        public const int CAN_CASTLE_KS = 12683;     // can castle king-side
        public const int CAN_CASTLE_QS = 12684;     // can castle queen-side
        public const int KS_DIAG_MOBILITY = 12685;  // open diagonal line attacks against king
        public const int KS_ORTH_MOBILITY = 12698;  // open orthogonal line attacks against king
        public const int KING_OUTSIDE_PP_SQUARE = 12712;    // king cannot stop promotion
        public const int PP_CAN_ADVANCE = 12713;    // passed pawn can safely advance
        public const int BLOCKED_PASSED_PAWN = 12717;       // blocked passed pawn
        public const int ROOK_BEHIND_PASSER = 12757;
        public const int BISHOP_PAIR = 12758;       // bonus for having both bishops
        public const int BAD_BISHOP_PAWN = 12759;   // bad bishop pawn
        public const int ROOK_ON_OPEN_FILE = 12760; // rook on open file
        public const int ROOK_ON_HALF_OPEN_FILE = 12761;    // rook on half-open file
        public const int PAWN_PUSH_THREAT = 12762;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12768;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12774;      // minor piece threat
        public const int ROOK_THREAT = 12780;       // rook threat
        public const int CHECK_THREAT = 12786;      // check threat against enemy king
        public const int TEMPO = 12792;             // tempo bonus for side moving

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

        public Score BackwardPawn
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[BACKWARD_PAWN];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score CheckThreat(Piece checker)
        {
            return weights[CHECK_THREAT + (int)checker];
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

        // Solution sample size: 6000001, generated on Wed, 27 Mar 2024 21:25:51 GMT
        // Solution K: 0.003850, error: 0.083853, accuracy: 0.5070
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 77, 230),   S(383, 672),   S(412, 669),   S(532, 1087),  S(1361, 1824), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(109, -132),  S(154, -97),   S( 37, -45),   S(-23,  27),   S(-24,   9),   S(-29,  11),   S(-49,   4),   S(-36, -11),
            S(132, -135),  S(113, -111),  S( 14, -66),   S( -7, -54),   S(-14, -18),   S(-17, -31),   S(-27, -30),   S(-23, -41),
            S(107, -101),  S( 71, -69),   S( 22, -77),   S( 16, -78),   S(-10, -65),   S(  6, -61),   S( -7, -50),   S(  8, -56),
            S( 85, -43),   S( 72, -67),   S( 25, -54),   S( 29, -83),   S(-14, -48),   S(-13, -54),   S(-17, -43),   S( -6, -26),
            S( 61,  49),   S( 35,  -4),   S( 27, -16),   S( 47, -64),   S( 42, -59),   S( -2, -57),   S(-18,   1),   S(-12,  52),
            S( 57,  78),   S( 68,  76),   S( 12,  -7),   S( 13,  12),   S(-59,  18),   S( 18,  10),   S(-16,  33),   S( 24,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 39, -34),   S( 35, -39),   S( 53, -28),   S( -3,  31),   S(  2, -10),   S(  3, -11),   S(-41,   4),   S(-25,  20),
            S( 47, -50),   S( 23, -43),   S( 19, -49),   S(  6, -44),   S( -4, -22),   S( -1, -33),   S(-26, -21),   S(-28, -14),
            S( 38, -45),   S( 19, -30),   S( 23, -52),   S( 18, -56),   S(-21, -30),   S( 17, -46),   S( -2, -40),   S( 10, -29),
            S( 48, -21),   S( 31, -55),   S( 30, -62),   S( 20, -57),   S(-10, -25),   S(  9, -38),   S(-25, -20),   S( -2,  -3),
            S( 46,  42),   S(-38,  -5),   S(  0, -21),   S( 10, -45),   S( 58, -48),   S(-22,  -5),   S(-27,   7),   S( -1,  63),
            S( 81,  45),   S( 14, -16),   S(-47,   3),   S(-18,  40),   S(-67,   9),   S(-53, -13),   S(-41,  19),   S(-43,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12,  -6),   S(-20,   1),   S(-11,   6),   S( -8,  12),   S( 12,   2),   S( 37, -20),   S(  8, -44),   S( -3, -20),
            S(  2, -26),   S(-23, -19),   S(-19, -35),   S(-12, -36),   S( 11, -26),   S( 15, -31),   S( -1, -43),   S(-14, -30),
            S( -5, -26),   S(-17, -30),   S( -6, -56),   S( -3, -57),   S( -3, -32),   S( 25, -46),   S(  6, -41),   S( 14, -33),
            S(-10,  -9),   S( -6, -48),   S(-11, -52),   S(  2, -61),   S( 14, -52),   S(  6, -34),   S( -2, -25),   S(  9, -10),
            S( 17,  32),   S(-38, -18),   S(-52, -42),   S(-49, -24),   S( 34, -13),   S( 11,  -2),   S( -6,  11),   S(-13,  82),
            S(-87,  89),   S(-112,  76),  S(-94,   7),   S(-82, -15),   S(-25,   1),   S( -9,  28),   S( 17, -13),   S(-28,  84),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -18),   S(-25,  -6),   S(-22,  -8),   S(  3, -43),   S( -1,  -2),   S( 47, -21),   S( 91, -71),   S( 72, -87),
            S( -4, -43),   S(-23, -36),   S(-20, -47),   S(-13, -34),   S( -5, -28),   S( 21, -41),   S( 67, -78),   S( 69, -80),
            S( -4, -51),   S( -2, -60),   S(  2, -73),   S(  3, -70),   S( -4, -60),   S( 29, -61),   S( 44, -69),   S( 86, -78),
            S( -1, -33),   S(  7, -80),   S(  6, -85),   S( 14, -86),   S( 29, -78),   S( 23, -61),   S( 31, -51),   S( 74, -33),
            S( 36,   2),   S(  8, -58),   S( 19, -84),   S( 15, -72),   S( 93, -65),   S( 69, -45),   S( 72,   1),   S( 61,  63),
            S(-37, 104),   S(-30,  16),   S(-10, -57),   S( 12, -77),   S( 74, -92),   S( 88, -27),   S( 74, -17),   S( 92,  57),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-102,  28),  S(-18,  -6),   S(-42,  14),   S( -7,  15),   S(-25,  -7),   S(-67,  34),   S(-58,   0),   S(-68,   6),
            S(-19,   8),   S( 25,  -6),   S( 13, -22),   S( 11, -21),   S( -1, -22),   S(-43, -28),   S( -9, -45),   S( -4, -30),
            S( 37, -15),   S( 34,  -8),   S(-21,  -1),   S( -1, -25),   S(-23, -36),   S( -1, -47),   S(-52, -39),   S( 33, -51),
            S( -7,  44),   S(-31,  44),   S( 26,  11),   S(-12,   4),   S( 14, -34),   S(-24, -36),   S(-18, -37),   S( 69, -48),
            S(-18,  95),   S(-31,  91),   S(-17,  34),   S(-16,  -7),   S(  4,  16),   S(-38,   4),   S(-12, -43),   S( 53,  24),
            S( 89,  76),   S( 22, 108),   S( 11,  17),   S(  9,  13),   S( 25, -13),   S(  5,  22),   S( 12,   7),   S(-24,  25),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-98,  57),   S(-83,  46),   S( -3,  13),   S( 26,  28),   S(-44,  38),   S(-35,  25),   S(-47,  14),   S(-38,  35),
            S(-51,  21),   S(-58,  15),   S( 22,  -7),   S(-26,  18),   S(  8,  -3),   S(-12, -18),   S(-20, -13),   S(-42,  15),
            S(-53,  41),   S(-55,  28),   S( 46, -24),   S( 10, -27),   S( 25, -15),   S(-26, -16),   S( -9,  -9),   S(  3,  -3),
            S(-55,  59),   S(-63,  41),   S(  1,   2),   S(  5,  15),   S( 11,  -5),   S(-35,  -9),   S( 14, -23),   S( -2,  17),
            S( 39,  62),   S( -1,  52),   S( 13,  52),   S( 12,  37),   S(-20,  42),   S( 43,   2),   S(  0,   7),   S( 67,  20),
            S(102,  32),   S( 72,  25),   S( 30, -10),   S( 43,  -4),   S( 71,   3),   S( 16,  14),   S( -8,  14),   S( 13,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-58,  31),   S(-46,  19),   S(-64,  34),   S(-21, -10),   S( 47, -33),   S(-46,  17),   S(-64,   4),   S(-68,  24),
            S(-40,   3),   S(-10, -23),   S(-30, -27),   S(  5,  -7),   S( 33, -13),   S( 24, -23),   S(-36,  -8),   S(-58,   5),
            S(-15,  -6),   S(-25,  -5),   S(-20, -22),   S(-24,  -9),   S( -3, -10),   S( 62, -41),   S(-16, -16),   S(-27,   6),
            S( -4,   1),   S(-94,  16),   S(  5, -31),   S(-17,  -5),   S(  3,   4),   S( 29, -13),   S( 30, -19),   S( 40,   0),
            S(  3,  27),   S(-87,  26),   S(-20, -26),   S( 26, -25),   S( 59,  25),   S( 82,  19),   S(  9,  20),   S( 87,  39),
            S( 62,  37),   S( 21,   0),   S(-25, -29),   S( 32, -47),   S( 23,  16),   S( 23,  21),   S( 23, -10),   S( 46,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -28),   S(-56, -12),   S(-15, -13),   S(-63,   0),   S(-36, -11),   S( 15, -20),   S( -9, -44),   S(-43, -23),
            S(-30, -42),   S(-33, -51),   S(-44, -45),   S(-17, -49),   S( 12, -47),   S( 51, -52),   S( 62, -58),   S(  2, -38),
            S(-30, -49),   S(-44, -50),   S(-41, -53),   S(-17, -51),   S( -7, -33),   S( 41, -43),   S( 49, -59),   S( 61, -50),
            S(-18, -43),   S(-52, -57),   S(-87, -46),   S(-51, -22),   S(  0, -33),   S( 26, -21),   S( 17, -13),   S( 76, -26),
            S(  7, -22),   S( 22, -76),   S(-20, -54),   S(-20, -69),   S(  8,  10),   S( 20,   9),   S( 50,  65),   S( 99,  42),
            S(-12,   2),   S( -6, -59),   S( 36, -63),   S( -4, -46),   S( -9, -42),   S( 52, -33),   S( 27,  61),   S( 79,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-63,  68),   S(-41,  74),   S( 17,   2),   S(-13,   2),   S(  9, -14),   S(-23,  12),   S(-43,   5),   S(-45,  33),
            S(-80,  60),   S(-60,  48),   S(-26,  37),   S(-10,  33),   S(-32,  -7),   S(-32, -12),   S(-56, -18),   S( 11, -12),
            S(-63, 112),   S(-20, 105),   S(-10,  65),   S(-47,  57),   S( 53, -42),   S(-89, -17),   S(-52, -30),   S(-30, -18),
            S(-49, 147),   S( 41, 144),   S( 26, 100),   S( 16,  58),   S(-39,  14),   S(-19, -35),   S(-51, -18),   S(-76,  17),
            S(  7, 183),   S( 59, 165),   S( 63, 160),   S( 68, 107),   S(  3, -25),   S( 25,  -2),   S( -4,  -2),   S( -1,   4),
            S( 54, 200),   S( 92, 210),   S( 85, 217),   S( 41,  76),   S( 12,  38),   S( -9,  16),   S(-11, -34),   S( -6,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-90,  76),   S(-67,  61),   S(  4,  25),   S( 21,  41),   S(  5,  11),   S(-17,   8),   S(-100,  34),  S(-117,  43),
            S(-47,  50),   S(-68,  39),   S(-22,  32),   S(-10,  61),   S(-60,  12),   S(-34,  -6),   S(-93,  -4),   S(-15,   0),
            S(-89,  74),   S(-118,  99),  S(-50,  84),   S(-127, 112),  S(-89,  71),   S(-79,  -1),   S(-24, -24),   S(-32,  -3),
            S(-81, 121),   S(-49, 127),   S(  1, 127),   S( 36, 149),   S(-40,  72),   S(-41,  17),   S(  7,  -4),   S(-62,  32),
            S( 40, 109),   S(-10, 145),   S( 45, 159),   S( 52, 189),   S( 32, 154),   S(  1,  46),   S(  5,  12),   S(  3,  10),
            S( -3,  85),   S( 18, 145),   S( 62, 144),   S( 60, 181),   S( 31, 103),   S(-12,  17),   S(-13,  -6),   S(-30, -44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-101,  23),  S(-63,  -8),   S( -5,   4),   S( 10,  52),   S(  6,   8),   S(-80,  37),   S(-116,  31),  S(-69,  40),
            S(-90,  -1),   S(-78,   0),   S(-25, -16),   S(-27,  -2),   S(-14,  23),   S(-69,  41),   S(-128,  38),  S(-65,  18),
            S(-31,  -8),   S(-96,  12),   S(-26,   5),   S(-86,  71),   S(-95,  97),   S( -9,  47),   S(-153,  54),  S(-109,  52),
            S(-94,  25),   S(-104,  28),  S( 21,  13),   S(-45, 106),   S( 13, 112),   S(-69, 100),   S(-41,  63),   S( 31,  21),
            S(-25,  42),   S(-44,   9),   S( 10,  37),   S( 41, 134),   S(105, 120),   S( 47,  92),   S( -5, 101),   S(  0,  64),
            S(  3, -14),   S(-14,  -9),   S( 17,   6),   S( 58, 130),   S( 26, 140),   S(  7,  63),   S( -1, 111),   S( 20, 115),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-65,  -8),   S(-100,  26),  S( 49, -29),   S(  9,  32),   S( -2,  35),   S(-76,  59),   S(-41,  36),   S(-27,  38),
            S(-84, -12),   S(-93, -18),   S(-40, -39),   S(-41,  11),   S(-42,  24),   S(-24,  17),   S(-97,  74),   S(-77,  51),
            S(-31, -40),   S(-47, -35),   S(-46, -15),   S(-28,  -4),   S(-19,  28),   S(-38,  69),   S(-79,  95),   S(-63,  79),
            S(-49,  -2),   S(-78, -19),   S(-51, -17),   S(-40,  27),   S( 10,  28),   S(-38,  84),   S( 23, 112),   S( 72,  82),
            S(-26,  50),   S(-57,   6),   S(-14,  -2),   S(-16,  31),   S( 42, 104),   S( 10, 115),   S( 85, 135),   S( 92, 134),
            S(-35,   8),   S(-10, -16),   S(  2, -21),   S(-18, -44),   S( 24,  76),   S( 32, 154),   S( 72, 169),   S( 36, 173),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23,   3),   S(-23,  -6),   S(-11,   5),   S( -4,  -2),   S( -7, -20),   S( -4,  13),   S(-30, -61),   S(-17,  -5),
            S(-37, -22),   S(-18,  35),   S( 20,  40),   S(  9,   5),   S(  1,  29),   S(-29,  -8),   S( -5, -40),   S(-54, -31),
            S(-27,  40),   S(-23,  85),   S( 21,  56),   S( 31,  27),   S( -7,  -3),   S(-31,   0),   S(-59, -49),   S(-38, -74),
            S(-41,  86),   S(-52, 140),   S( 60, 150),   S( 32, 108),   S(-12, -34),   S(-33, -28),   S(  0,   5),   S(-66, -85),
            S( 43, 112),   S( 46, 231),   S( 55, 163),   S( 19,  74),   S( -8,  10),   S(-16, -47),   S( -5, -48),   S(-19, -83),
            S( 45, 113),   S( 55, 239),   S(118, 231),   S( 41,  89),   S( -5,  16),   S(-12,  -7),   S(-10, -30),   S(-15, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52, -31),   S(-13,  -2),   S(  4,  46),   S( -1,   2),   S( -9, -13),   S(-24,  19),   S(-43, -37),   S(-30,  13),
            S(-31,  -5),   S(-72,  29),   S(-34,  40),   S( 17,  26),   S(-49,  15),   S(-26, -42),   S(-98, -66),   S(-81,  29),
            S(-51,  64),   S(-30,  31),   S(-50,  78),   S(  5, 112),   S( -4,  32),   S(-18, -26),   S(-78, -38),   S(-63, -36),
            S(-56, 116),   S( -8, 107),   S(-32, 158),   S(-20, 108),   S( 16,  77),   S(-47,  33),   S( -8, -55),   S(-42, -39),
            S( 11, 113),   S( 53, 187),   S( 64, 201),   S( 33, 220),   S( 34, 152),   S( -2,  15),   S(  3, -67),   S(-24, -23),
            S( 34,  65),   S( 82, 172),   S( 84, 195),   S( 86, 276),   S( 41, 128),   S( -9,  -2),   S(  6,  25),   S(-10, -16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34, -43),   S(-30, -18),   S(-16, -46),   S(  0,  -3),   S( -7,   4),   S(-26,  -6),   S(-43, -35),   S(  3,   6),
            S(-45,  23),   S(-34,   3),   S(-69, -41),   S(  7,  -9),   S(-39,  49),   S(-12,  29),   S(-44,  28),   S(-62,  26),
            S(-43,  -5),   S(-68,   1),   S(-32, -22),   S(-12,  70),   S(-33,  73),   S(-67,  42),   S(-45,  -1),   S(-87,  52),
            S(-38, -11),   S(-32,  69),   S(-15,  22),   S( -5, 103),   S( -9, 151),   S(-52,  80),   S(-32,  33),   S(-20,  55),
            S(-27, -11),   S( -2,  13),   S( 26,  93),   S( 29, 113),   S( 61, 207),   S( 46, 166),   S( 13,  77),   S( 14,  43),
            S( 10,  58),   S( 12,  19),   S( 42, 133),   S( 20, 134),   S( 52, 186),   S( 40,  99),   S( 28,  95),   S( 24,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -11),   S(-34, -37),   S(-21, -67),   S(  1,   6),   S( 24,  49),   S(  7,  44),   S(-15,   6),   S( 13,  38),
            S(-61, -16),   S(-29,  -3),   S(-24, -33),   S( 38,  -2),   S(-23,  -5),   S(  8,  72),   S(  0,  38),   S(-21,   9),
            S(-18, -71),   S(-33, -37),   S(-10, -53),   S(  2, -25),   S( 14,  38),   S(-14,  84),   S(-20,  68),   S(-48,  54),
            S(-27, -33),   S(-54, -44),   S(-27,   5),   S(  6,  29),   S(-10,  78),   S(  9,  91),   S(-28, 128),   S( -8,  64),
            S(-35, -42),   S(-31, -22),   S( -8,  32),   S(  3, -11),   S( 25,  95),   S( 50, 156),   S( 52, 235),   S( 79,  95),
            S(-13, -19),   S(  0,   9),   S(  3,  25),   S(  5,  28),   S( 24,  70),   S( 88, 180),   S( 22, 154),   S( 33,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-40, -35),   S( 13,   2),   S(-37,  -2),   S(-41,  10),   S(-60,  15),   S(-10, -36),   S(-60, -37),   S(-36,  -5),
            S(-27,  40),   S( 40, -57),   S(-52,  16),   S(  7, -16),   S(  6, -36),   S(-18, -23),   S(-51,   9),   S(-82, -47),
            S(  6,  42),   S(  4, -30),   S( 16, -18),   S(-26,  48),   S( 21,   8),   S(-30,  -5),   S( -9, -25),   S(-34, -44),
            S( 23, -16),   S( 48,  21),   S( 23,  16),   S( 17,  50),   S(  3,   6),   S(  1,  16),   S(-16,  -6),   S(-18, -13),
            S( -1, -32),   S( 46,  -3),   S( 19,  16),   S( 77, -29),   S( 51,  -4),   S( 39,  31),   S( 31, -12),   S(-51,  -6),
            S( 19, -36),   S( 12,   6),   S( 28,  24),   S( 64,   2),   S( 40, -54),   S( 24,  -2),   S( 15, -16),   S(-34,  -9),
            S( -6, -12),   S( 21,   2),   S( 38, -33),   S( 32, -47),   S( 44, -39),   S(-44, -48),   S(-30, -44),   S(-31,  -9),
            S(-74, -77),   S( -3,  -7),   S(-33, -46),   S( -8, -39),   S(-34, -28),   S(  7, -22),   S(-20,  -9),   S( 13,   4),

            /* knights: bucket 1 */
            S(-62,  29),   S(-54,  97),   S( 13,  39),   S( -4,  50),   S(-26,  49),   S(-45,  40),   S(-36,  47),   S(-46,   4),
            S( 15,  29),   S( 16,  28),   S(-12,  19),   S(-19,  53),   S(  1,  17),   S(  4,   9),   S(-25,  -1),   S(-29,  14),
            S(-30,  22),   S( -1,  22),   S(  9,  12),   S( 22,  25),   S(  8,  39),   S(-26,  32),   S( -3,   1),   S(-37,  17),
            S( -2,  17),   S( 50,  27),   S( 25,  48),   S( 13,  39),   S( 11,  45),   S(-10,  40),   S( 26,  -8),   S(  4,  32),
            S(-12,  47),   S( 34,  20),   S( 37,  22),   S( 44,  25),   S( 24,  49),   S( 19,  37),   S( 22,  14),   S(  3,  25),
            S( 27,   2),   S( 15,  20),   S( 18,  44),   S( 76,   4),   S(  6,  18),   S( 34,  51),   S( 41,  -9),   S( 10, -12),
            S( 35,  21),   S( 13,  11),   S(  3, -21),   S(-14,  27),   S(  9,   5),   S( 72, -14),   S(-51,   4),   S(  5, -17),
            S(-128, -79),  S(-14,   1),   S( -6,   8),   S( -5,  18),   S(-12, -10),   S(-13,  -8),   S(  2,   4),   S(-33, -53),

            /* knights: bucket 2 */
            S(-41,  14),   S( -5,  32),   S(-42,  58),   S(-30,  57),   S(-23,  56),   S(-33,  61),   S(-16,  22),   S(-37,  26),
            S(-30,  -9),   S( -7,   6),   S( -7,   8),   S( -9,  27),   S(  2,  12),   S(-17,  48),   S(-38,  67),   S(-61,  87),
            S(-21,  20),   S( -3,   8),   S( -9,  30),   S( 13,  29),   S(  6,  37),   S(  4,  12),   S( -3,  22),   S(-29,  14),
            S(-10,  44),   S(-13,  51),   S(  9,  49),   S(  0,  55),   S(  5,  41),   S( -8,  32),   S( -4,  56),   S( -3,  27),
            S( 22,   6),   S(-14,  34),   S(  0,  49),   S(-21,  59),   S(  3,  53),   S(-11,  44),   S(  5,  43),   S( -5,  14),
            S(-36,  20),   S(-10,  30),   S(-29,  51),   S( -8,  47),   S(-20,  58),   S(-18,  48),   S(-21,  12),   S( 48, -16),
            S(-36,  33),   S(-18,  -7),   S(-56,  24),   S( -8,  35),   S( -7,  -4),   S(  1,  17),   S(-26,  19),   S(-35,  -4),
            S(-142,  -3),  S( 21,  15),   S(-59,  34),   S( -4,  -9),   S(  8,   0),   S(-66,  -7),   S( 16,   1),   S(-171, -55),

            /* knights: bucket 3 */
            S(-52, -17),   S(  5, -29),   S(-45,  13),   S(  1,  14),   S( -4,   4),   S( -7,   3),   S( 28, -24),   S( -4,   4),
            S( -1,  -4),   S(-34,  -4),   S(-14,  -4),   S( 16,   3),   S( 24,  -8),   S( -6, -18),   S(  1, -10),   S(-15,  47),
            S( -3, -23),   S(  9,  -6),   S(  8,   3),   S( 26,   8),   S( 23,  21),   S( 28,   0),   S(  8,   6),   S( 10,  34),
            S(  5,  -4),   S( 21,  18),   S( 23,  43),   S( 25,  41),   S( 37,  29),   S( 27,  35),   S( 35,  29),   S( 27,  18),
            S( 18,  10),   S(  9,  26),   S( 43,  13),   S( 29,  49),   S( 35,  40),   S( 29,  55),   S( 47,  45),   S( 21,  10),
            S( -7,  -1),   S( 31,  -6),   S( 41,  13),   S( 82,   7),   S( 61,   1),   S( 84,  -9),   S( 16,   8),   S( 15,  23),
            S( 29,   3),   S( -7,   4),   S( 49, -23),   S( 47, -14),   S( 64, -41),   S( 55, -22),   S( 35, -65),   S( 32, -27),
            S(-133,   6),  S(-16,   5),   S(-14,  -8),   S( 19,  21),   S( 14, -15),   S(  9, -27),   S( -5, -53),   S(-88, -47),

            /* knights: bucket 4 */
            S( -1,  10),   S(-48,  16),   S( 15,  33),   S( -4,   1),   S(-11, -16),   S(-31, -49),   S( 11, -59),   S(-31, -38),
            S( 41,  33),   S(-55,  23),   S( 12, -20),   S(  8,  14),   S( 17,  -1),   S( -7, -34),   S( 14, -28),   S( -3, -44),
            S(  0,  42),   S( 44,  13),   S(  0,  21),   S( 30,  30),   S( -9,  13),   S(-21,  27),   S(-29, -38),   S(-29, -41),
            S(-11,  71),   S( -3,  -7),   S( 35,  31),   S( 28,  16),   S( 35,  14),   S(119,   1),   S( 21, -44),   S( -8, -19),
            S( 73,  20),   S(-21,  52),   S( 32,  45),   S( 45,  34),   S( 31,  26),   S( 30,  24),   S(-13, -50),   S(-12, -31),
            S( 10,   5),   S(-33,  11),   S(115,  43),   S( 27, -16),   S( 23,  35),   S( 22,  27),   S(  5,  32),   S(  3, -27),
            S(  6,   0),   S(-10,   6),   S( -4, -10),   S(  7,  56),   S(  6,  -3),   S(  8, -29),   S(  7,  -5),   S(-12,  15),
            S(-15,  -4),   S( -8, -21),   S( 14,  15),   S(  5,   7),   S( -9, -16),   S( 14,  37),   S(  2,  14),   S( -3, -15),

            /* knights: bucket 5 */
            S( 21,   7),   S(-17,  -2),   S( 28,  21),   S(  9,  63),   S( 36,  22),   S( 11,   1),   S(-34, -12),   S(-21, -23),
            S(  0, -15),   S( 12,  51),   S( 42,  14),   S( -4,  38),   S( 38,  38),   S(  7,  40),   S( 22,  24),   S(-15,   1),
            S( -6,  37),   S(-30,  43),   S( 46,  27),   S( 37,  52),   S( -5,  50),   S(-17,  36),   S(-22,  31),   S( 22,  -4),
            S( 37,  42),   S( 20,  46),   S( 47,  45),   S( -5,  65),   S(  5,  55),   S( 18,  47),   S( 30,  45),   S(  7,  48),
            S( 12,  42),   S( 32,  34),   S( 45,  63),   S( 50,  49),   S(108,  45),   S( 52,  30),   S( 40,  55),   S( 17,  20),
            S(  4,  14),   S(  8,  47),   S( 20,  51),   S( -5,  71),   S( 33,  34),   S( 22,  66),   S( 33,   0),   S(-11,  24),
            S(  9,  13),   S(-27,  69),   S( 46,  52),   S( 20,  61),   S( -7,  45),   S( 19,  56),   S( 15,  55),   S(  3,   9),
            S( -4,  12),   S( -5,   3),   S( 12,  37),   S( -4,   5),   S( 19,  57),   S(-10,  -9),   S(  6,  29),   S(-43, -66),

            /* knights: bucket 6 */
            S( 16, -49),   S( -1, -10),   S( 29,  30),   S(-40,  38),   S(-42,  37),   S( 25,  46),   S(-19,  34),   S(-25,  15),
            S(-16, -39),   S( 75, -12),   S( 13,  12),   S(-28,  39),   S(-89,  83),   S( 29,  53),   S( -8,  46),   S(-13,   2),
            S(  7, -28),   S(-11,  15),   S( 21,  12),   S( 31,  26),   S( -3,  54),   S(-42,  62),   S( 13,  56),   S(  3,  51),
            S(  5,   6),   S( 32,  25),   S( 65,  33),   S( 69,  32),   S( 14,  45),   S( 13,  52),   S(  3,  59),   S(-20,  70),
            S(-11,  32),   S( 63,   2),   S( 63,  41),   S( 65,  34),   S( 80,  48),   S( 53,  55),   S( 32,  42),   S( 44,  29),
            S( 24,  22),   S( 38,  14),   S( 56,  31),   S( 47,  57),   S( 83,  44),   S( 34,  33),   S(  0,  38),   S( 49,  45),
            S(-16,  34),   S( 13,  17),   S(-40,  39),   S( 31,  39),   S(  5,  48),   S( 27,  66),   S( 19,  59),   S(-30,  12),
            S(-43, -17),   S( 16,  45),   S( 12,  17),   S(-17,  25),   S( 21,  40),   S( 12,  52),   S(  8,  39),   S( 17,  25),

            /* knights: bucket 7 */
            S(-43, -61),   S(-233, -65),  S(-92, -40),   S(-102,  14),  S(-47,   4),   S(-25, -24),   S(-39,   3),   S(-18,  -7),
            S(-36, -88),   S(  7, -48),   S(-41, -43),   S(-32,  -6),   S(-74,  22),   S( -9, -19),   S( 37,  25),   S( 12,   7),
            S(-76, -56),   S(-49, -24),   S(-57,  14),   S( 17, -15),   S( -7,  -5),   S( 17,  -3),   S(-20,  46),   S( 70,  32),
            S(-77, -11),   S(-18, -16),   S(  0,  20),   S( 38,  -1),   S( 48,   4),   S( 13,   6),   S(  7,  16),   S(-31,  19),
            S(-70, -30),   S(-34, -20),   S( 47, -20),   S( 73,   3),   S(111,  -1),   S( 63,  28),   S( 67,  15),   S( 47,  18),
            S(-30, -41),   S( -3, -38),   S(-13,  -3),   S( 70,   4),   S( 50,  28),   S( 78,  12),   S( 23,  21),   S( -7,  34),
            S(-47, -27),   S(-47,  -3),   S( 14, -16),   S( 61,  24),   S( -5,  24),   S( 52, -23),   S(-19,  10),   S( 21,  -4),
            S(-36, -18),   S(  1, -16),   S(-18,  -8),   S( 10,  25),   S(  4,  -1),   S( 33,  32),   S( -2, -16),   S(-23, -22),

            /* knights: bucket 8 */
            S( -4, -17),   S(  2,  13),   S( -1,   8),   S( -3, -11),   S(-18, -68),   S( -2, -34),   S( -1,  -3),   S( -5, -20),
            S(  1, -13),   S( -4, -20),   S(-14, -12),   S(-16, -44),   S(-29, -25),   S(-31, -96),   S(-18, -57),   S(-21, -40),
            S( -7,  11),   S( -2,  -8),   S( 21,  -2),   S(  8,  -1),   S( -5, -49),   S( -3,  -2),   S(-14, -40),   S(-13, -35),
            S(-15,  -5),   S( -3, -33),   S(-20,   9),   S(  7,   8),   S(  4,   8),   S( 13,  15),   S( -5, -15),   S(  7,  -9),
            S( 28,  64),   S( 10,   6),   S(  7,  32),   S( 24,  18),   S( 11,  48),   S(-14,  18),   S( 16, -18),   S(-14, -11),
            S( 19,  25),   S( -7, -31),   S( 29,  20),   S( 29,  39),   S(-10, -30),   S( -6,   5),   S( -3, -31),   S( -5,  -6),
            S(  4,  23),   S( 11,  -3),   S( -2, -16),   S(  1,  29),   S( 16,  29),   S(  4,  14),   S(  2,  13),   S(-11, -35),
            S(  4,   6),   S(  7,  20),   S( -3,  11),   S( -3, -32),   S( 12,  31),   S( -9, -38),   S(  1, -18),   S( -1,  -3),

            /* knights: bucket 9 */
            S( -7, -16),   S(-25, -33),   S(-10, -47),   S( -8, -41),   S(-27, -79),   S(-24, -55),   S( -5, -18),   S( -3, -15),
            S(-13, -22),   S(-11, -11),   S(-16, -58),   S(-16,  -6),   S(  6,   2),   S(  3, -22),   S( -2,  -3),   S(-14, -31),
            S(  3,   5),   S( -1,  13),   S(  4, -13),   S(  7,  16),   S( -2,  25),   S(-40, -17),   S(-13,   4),   S(-11, -21),
            S( -7, -21),   S( -1,  17),   S(  8,  30),   S( 17,  29),   S( 31,  30),   S( 20,  59),   S(-21, -47),   S( -3,   5),
            S(  4,  53),   S( 17,   5),   S( -6,  43),   S(  4,  51),   S( 11,  13),   S( 16, -13),   S(  3,  -7),   S( 11,  22),
            S(  4,  32),   S( 30,  54),   S( 29,  18),   S(-10,   7),   S( 39,  37),   S(  4,  -7),   S( 13,   6),   S( -5, -21),
            S(  0,  -8),   S( -8,  -2),   S( 10,   9),   S( 13,  -9),   S( 17,  44),   S(-14, -37),   S(  1,  33),   S( -8, -22),
            S(  0,  -6),   S(  5,   4),   S(  1,   6),   S( 24,  34),   S( 10, -17),   S(  0,  11),   S(  8,  23),   S(  0,  -1),

            /* knights: bucket 10 */
            S(-17, -39),   S(-28, -97),   S( -4, -34),   S(-23, -33),   S(-19,   5),   S(-11, -47),   S(  2,  18),   S( -1,   8),
            S(-11, -30),   S( -2,   8),   S( -1, -10),   S(-23, -43),   S(-38, -57),   S( -6, -49),   S(-11, -19),   S( -6,  -2),
            S(-17, -36),   S( -5, -37),   S(-20,  -1),   S(-17, -11),   S( 15,   0),   S(-24, -12),   S( -8,   4),   S(-15,  -1),
            S( -1,  -1),   S(-15, -57),   S(-12, -40),   S( 20,  24),   S( -3,  45),   S( 37,  37),   S( 22,  31),   S( 13,  44),
            S(-20, -76),   S(-17, -35),   S( 31,  41),   S( 17,  29),   S( 18,  38),   S( 10,  -1),   S(  8, -11),   S( 20,  44),
            S( -7, -42),   S(  0, -28),   S(-20, -43),   S(  4,  37),   S( 31,  60),   S( 27,  35),   S( 18,  47),   S( 18,  49),
            S(  1,  12),   S(-13, -42),   S(  7,  12),   S( 23,  17),   S( 19,  18),   S(  4,  37),   S( -3,  -4),   S(  8,  12),
            S(  1,  -9),   S(  0,   0),   S(-13, -10),   S(  2, -15),   S( 13,  39),   S( -2,  27),   S(  6,  30),   S( -1,   1),

            /* knights: bucket 11 */
            S( -1,  -1),   S(-11, -31),   S(  1, -39),   S( -4, -33),   S(-25, -50),   S( -9, -18),   S(  8,   5),   S( -4,   9),
            S(-24, -47),   S(-19, -42),   S(-10, -66),   S(-12, -27),   S( -4,   5),   S(-22, -29),   S(-18, -24),   S( -6,  -7),
            S(-11, -80),   S(-23, -70),   S(-24, -37),   S(  8,   7),   S(-19,  10),   S(-20,  11),   S( 21,  -3),   S( -2,   7),
            S(-15, -34),   S( -3, -43),   S(-34,  -1),   S(  8,  41),   S( 21,  24),   S( 12,  35),   S(  7,  -8),   S( 22,  13),
            S( 16, -11),   S(-30, -56),   S( -6, -41),   S( -5,   0),   S(  2,  30),   S( 45,  60),   S( -3,  -4),   S( 35,  81),
            S( -3,  -7),   S(-11, -39),   S(  3,   0),   S( 42,  61),   S( 15,  37),   S( 74,  69),   S( 10,  28),   S( 15,   5),
            S(  5,  24),   S(  7,  19),   S( 17,  15),   S( -5, -42),   S( 21,  30),   S(  3,  21),   S( 13,  32),   S( 18,  39),
            S( -5,  -3),   S( -3, -24),   S( 11,  16),   S( 10,  31),   S( -2,  13),   S( -3,  -9),   S(  5,  22),   S( -1,  20),

            /* knights: bucket 12 */
            S( -2,  -8),   S(  0,   0),   S( -3, -34),   S( -1,   5),   S( -7, -11),   S( -2,  -4),   S(  4,   6),   S( -5, -23),
            S( -2,  -5),   S(  0,  -3),   S(  3,  19),   S(  5,  14),   S( -9,  10),   S( -2, -23),   S(  0,  -7),   S(  2,  12),
            S( -3,  -9),   S(  5,   2),   S( -2,  -3),   S( -4,  -2),   S(  4,   7),   S(  4,  -4),   S(  1,  -7),   S( -9, -33),
            S(-14, -28),   S(  1,  -7),   S(  5,  15),   S(  4,   9),   S( -5,  -4),   S( -4,  -4),   S(  1,  -7),   S( -1, -15),
            S(  8,  20),   S(  3,   9),   S(  1,   9),   S(  6,  16),   S( -9, -40),   S(  7,  20),   S(  0, -12),   S( -3,  -6),
            S(  4,  -1),   S(-10, -18),   S(  9,  16),   S( -4, -12),   S(  0, -10),   S( -4, -17),   S(  0,   0),   S( -1,   0),
            S(  2,   1),   S(-10, -13),   S(-10,  14),   S( -8, -23),   S( -2,  -8),   S(-11, -36),   S( -2,  -3),   S( -1,  -4),
            S(  5,   5),   S(  6,  12),   S( -6,  -9),   S(  4,   9),   S(  0,   3),   S( -3,  -8),   S( -3,  -8),   S( -1,   0),

            /* knights: bucket 13 */
            S( -7, -22),   S( -3, -17),   S(  1,  -4),   S( -6, -20),   S( -7,  -3),   S(  1,   4),   S( -4, -16),   S(  4,   8),
            S( -2,  -7),   S( -1,  -4),   S(  2,  -6),   S( -5, -12),   S( -7, -34),   S(  3,   3),   S(  2,  16),   S(  0,  -1),
            S( -2, -23),   S( -3, -14),   S(  1,   5),   S( -8,   0),   S( -4, -27),   S( -9, -24),   S( -3, -13),   S(-10, -48),
            S( -5,  -3),   S(  7,  30),   S(  4, -11),   S(-12, -22),   S(  3,   4),   S( 15,  26),   S(  3,   8),   S( -1,  -4),
            S(  2,  -3),   S(  4,   4),   S(  8,  14),   S( 12,  38),   S( 11,   0),   S(  6,  -6),   S( -1, -24),   S(  0,  -3),
            S(  1,   6),   S(  7,  -4),   S( 22,  44),   S(-21,  12),   S(  3,  17),   S(-14, -46),   S(  1,  -4),   S( -6, -13),
            S(  0,   4),   S(  1,   7),   S(  9,   5),   S(  0,  -3),   S( 11,  30),   S( -1,  -8),   S( -4,  -1),   S( -8,   2),
            S( -2,   4),   S( -6,  -9),   S( -6,  -9),   S( -1, -23),   S(  0,  -2),   S(  0,  -3),   S(  0,  -1),   S(  0,   2),

            /* knights: bucket 14 */
            S( -2, -18),   S( -5, -18),   S( -5, -14),   S(  0,  13),   S(-12, -32),   S( -5, -31),   S( -1,  -9),   S(  0,  -4),
            S( -3, -13),   S( -2,  -3),   S(-16, -64),   S( -4, -24),   S(  2,  -4),   S( -3, -16),   S(  1,   8),   S(  1,   5),
            S( -4,   1),   S( -3,  -6),   S( -9, -37),   S( -2, -14),   S(  1, -22),   S(-13, -40),   S(  2,   3),   S(  2,   2),
            S(  1,  16),   S( -7, -34),   S(-12, -28),   S(-12, -51),   S( -3,   0),   S(  5,   7),   S(  2, -15),   S( -9,  -6),
            S(  0,   2),   S(  2,  -5),   S(  8,  35),   S(  0, -18),   S( -6,  20),   S(  2, -13),   S(  7,  25),   S( -3,  -7),
            S( -3,  -7),   S(  4,   3),   S( -5, -24),   S( 12,  17),   S( 10,  19),   S(  8,  12),   S(  0,  14),   S( -2, -11),
            S(  1,  -3),   S( -1,  -6),   S( 11,  -7),   S( -3, -18),   S( -8, -21),   S(-11,  -9),   S( -2,  -2),   S(  4,  14),
            S(  0,  -2),   S(  1,  -2),   S( -5, -19),   S(  6,  -5),   S(  1,  14),   S(  1,   6),   S(  2,   3),   S(  0,  -4),

            /* knights: bucket 15 */
            S(  0,   4),   S( -1, -21),   S( -5, -25),   S( -6,  -5),   S( -2,  -1),   S(  3,   8),   S( -2,  -7),   S(  0,   6),
            S( -3, -11),   S(  0,  -1),   S(  2,   0),   S( -6, -29),   S( -1,   4),   S(  1,   8),   S(  1,   7),   S( -1,  -5),
            S(  2,  -1),   S( -3, -12),   S( -4,   0),   S(-17, -45),   S(  2,   2),   S( -3,  -4),   S( -1,  -3),   S(  0,   9),
            S(-10, -32),   S( -6, -20),   S(-13, -29),   S(  2,  -8),   S(  9,   7),   S(  8,  27),   S( 10,  32),   S( -3,   8),
            S( -2, -11),   S( -6,  -6),   S(-10, -30),   S(-13, -35),   S(  7,  40),   S( 10,  27),   S(-13, -13),   S( -3,   1),
            S( -4,  -3),   S( -6, -12),   S(-11, -41),   S( -6,   1),   S( -6, -27),   S( -5,  28),   S(  3,  14),   S( -2,   9),
            S( -4, -21),   S( -2,  -4),   S( -3, -19),   S( -5,  -5),   S( -8,  -6),   S(  8,  48),   S( -3, -22),   S(  4,  15),
            S(  0,   0),   S( -1,  -4),   S( -6, -21),   S(  0,  -4),   S( -4,  -3),   S(-15, -19),   S(  7,  15),   S( -2,  -3),

            /* bishops: bucket 0 */
            S( 25,  57),   S( -8,  12),   S( 55,   4),   S( -6,  21),   S(-22,   0),   S( -3,  19),   S(  0, -56),   S(  9, -14),
            S( 51, -45),   S( 84,  16),   S( 35,  13),   S( 15,   4),   S( -2,  27),   S( -5, -30),   S(-36,  15),   S( 15, -39),
            S(  7,  40),   S( 37,  11),   S( 38,  31),   S(  5,  64),   S( 21,  17),   S( -5,  44),   S( -5, -12),   S(  8, -44),
            S( 53, -44),   S( 82,  -3),   S( 43,  13),   S( 45,  34),   S( -5,  49),   S( 40, -11),   S( 15, -24),   S(-18,  -2),
            S( 22,  -5),   S( 37,  30),   S( 13,  38),   S( 85,   1),   S( 60,  -8),   S(  5,  -5),   S( 22,  -8),   S(-62,  19),
            S(-18,  43),   S( 20,  10),   S( 57,  -1),   S(106, -40),   S( 58,  32),   S(  0,  27),   S( 20,   5),   S( 28, -17),
            S(-30,  -9),   S( 58,  11),   S( 47, -34),   S( -3,  26),   S(-38,   5),   S( 61,  20),   S( 66,  -2),   S( -9, -42),
            S(-42, -33),   S( 28, -11),   S(-17,  12),   S( 31,  30),   S(  9,  -4),   S( 17,  -8),   S(-22,  37),   S(-14,   2),

            /* bishops: bucket 1 */
            S( 24,  43),   S(-23,   0),   S( -2,  36),   S(  6,  12),   S( -5,  22),   S( -6,  21),   S(  1,  -2),   S(-52,   4),
            S(  3, -14),   S( 40,  -6),   S( 51,   7),   S( 21,  15),   S( -7,  12),   S(  7, -13),   S(-27,  17),   S( 18, -16),
            S( 39,  -1),   S( 11,  13),   S( 45,   7),   S( 25,  17),   S( 18,  22),   S(-13,  30),   S( 35, -18),   S(  0, -51),
            S( 15,  -5),   S( 32,  19),   S(  9,  27),   S( 29,  21),   S(  1,  34),   S( 15,   8),   S( 18, -19),   S(  9, -12),
            S( 38,  12),   S( 13,  17),   S( 38,  19),   S(  8,  27),   S( 19,  21),   S(  6,   6),   S( 14,  -7),   S(  0,  -3),
            S( -7,  15),   S( 24,  49),   S( 20,  34),   S( 76, -30),   S(  8,  29),   S( 34,  13),   S(-23,  39),   S( 25,  -2),
            S(-23,  56),   S( 24,  40),   S( 20,  50),   S( 40,  15),   S( 65,  18),   S( -4,  25),   S( 17,   1),   S( -2,   2),
            S( 49,  12),   S( 11,   6),   S(-28,  21),   S(-18,  13),   S(  0,  19),   S( 19,  20),   S(  5,  -4),   S( 12,  28),

            /* bishops: bucket 2 */
            S( 26,  -5),   S( 16,   3),   S(  1,  14),   S(-21,  42),   S(-10,  19),   S(-21,  26),   S(-15, -28),   S(-40,  26),
            S( -9,  12),   S( 12,   2),   S( 21,   7),   S(  6,  13),   S(  0,  24),   S(  3,   3),   S(  6, -14),   S( 10, -38),
            S(-10,   4),   S( -9,  18),   S( 11,  27),   S(  0,  41),   S(  6,  26),   S( 14,  31),   S(  8,  18),   S(-15,  -7),
            S( -4,  20),   S(-20,  25),   S(-13,  39),   S( 12,  29),   S(  1,  46),   S(  5,  25),   S( -5,  27),   S(-10, -12),
            S( -2,   7),   S(-10,  27),   S(-12,  26),   S(-33,  47),   S( -4,  28),   S(-14,  39),   S(  1,  19),   S(-13,  10),
            S( -9,  30),   S( 10,  -3),   S( -7,  34),   S(-16,  28),   S( -1,  30),   S(  2,  13),   S( 15,  42),   S(  7,  16),
            S( -2,  19),   S( -8,  34),   S(-17,  47),   S( 22,  -9),   S(  6, -10),   S(-27,   8),   S(-49,  42),   S(-38,  37),
            S(-72,  60),   S(-59,  56),   S(-25,   6),   S(-19,   8),   S(-44,  25),   S(-64,  13),   S( 24,   9),   S(-84,   8),

            /* bishops: bucket 3 */
            S( -4,  25),   S( 25,  -5),   S( 18,  16),   S(  8,  15),   S( 12,  10),   S( 47,  -9),   S( 56, -38),   S( 43, -61),
            S( 10,   2),   S( 12,  21),   S( 27, -10),   S( 14,  31),   S( 24,  12),   S( 15,  16),   S( 58,   8),   S( 32,   2),
            S( 20,  13),   S( 10,  16),   S( 21,  36),   S( 31,  17),   S( 21,  51),   S( 42,  21),   S( 30,  27),   S( 39, -14),
            S( 17, -13),   S( 17,  19),   S( 19,  37),   S( 29,  61),   S( 34,  39),   S( 32,  27),   S( 26,  29),   S( 14,  -7),
            S( 17,  -1),   S( 27,  13),   S( 34,  21),   S( 32,  39),   S( 32,  40),   S( 41,  36),   S( 14,  42),   S( 29,  27),
            S( 16,   8),   S( 40,  17),   S( 35,  32),   S( 44,  16),   S( 29,  20),   S( 59,  24),   S( 59,   1),   S( 17,  61),
            S( 15,   4),   S(  1,  34),   S( 39,  33),   S( 13,   7),   S( 24,  25),   S( 31, -10),   S(  4,  58),   S(  5,  39),
            S(-24,  76),   S(  9,  37),   S( 43,  13),   S( 12,  19),   S(  5,  17),   S(  0,  37),   S( 21,   6),   S( 81,  -4),

            /* bishops: bucket 4 */
            S(-10, -37),   S(-37,   4),   S(-49,   7),   S(-20,  14),   S(-26,  33),   S(-62,  18),   S(  5,  -3),   S( -8,  20),
            S( -9,   3),   S( 15,   0),   S( -2,  36),   S(-30,  10),   S(-27,  -9),   S( 54,   0),   S(-22,  24),   S(  0,  -7),
            S(-30,  11),   S(-17,  23),   S( 32,  12),   S( -8,   6),   S( 26,  28),   S( 34,  19),   S(-12,  -4),   S(-85,  14),
            S(-17,  42),   S( 20,  23),   S( 55,   9),   S( 53,  33),   S( 60,  -7),   S( 72, -11),   S( 52,   2),   S(  0, -10),
            S( -2,   5),   S(  6,  26),   S( -2,  57),   S(  5,  55),   S( 17,  22),   S( 35, -33),   S(-16,  -2),   S(  7, -20),
            S( 10,  39),   S( 21,  31),   S(-26,  26),   S(-13,  36),   S( 34,  -7),   S( 25,  25),   S( 21, -43),   S(-12, -14),
            S(-32, -20),   S( 34,  16),   S( -9,  22),   S( 46,  29),   S( 11, -20),   S(  6,   2),   S( -1,  -5),   S(  5, -27),
            S( 13, -10),   S(-34, -58),   S( 27,  12),   S( -9, -11),   S(-17,  -9),   S(  9,  13),   S(  3,   7),   S( 13,  15),

            /* bishops: bucket 5 */
            S(  1,   5),   S(-14,  28),   S(-47,  32),   S(-32,  30),   S(-49,  22),   S(-45,   6),   S(-16,  32),   S(-22,  43),
            S(-56,  27),   S(-45,  32),   S(-12,  42),   S(  2,  27),   S(-16,  30),   S(-27,  27),   S(-77,  26),   S(-16,  -7),
            S(  4,   1),   S(-31,  51),   S( 15,  38),   S(-32,  47),   S(  5,  28),   S(-31,  35),   S(  4,  33),   S( -8,  -8),
            S( 13,  14),   S( 28,  36),   S( -5,  62),   S(  0,  40),   S( 43,  24),   S( 21,  32),   S( 12,  -4),   S(  8,  35),
            S( 47,  51),   S( 61,  -8),   S( 34,  35),   S( 93,  22),   S( 68,  13),   S( 39,  20),   S( 33,   8),   S( 11, -17),
            S( 28,  43),   S( 12,  53),   S( 60,  39),   S( 10,  38),   S(  0,  24),   S(  5,  -2),   S(-39,  40),   S(-13,  35),
            S( -8,  35),   S(-49,  36),   S( -6,  47),   S(  0,  46),   S( 18,  36),   S( 16,  49),   S(-18,  37),   S( -9,  28),
            S(  3,   8),   S( 15,  49),   S(  9,   3),   S(-12,  23),   S(  8,  46),   S(  7,  10),   S( 37,  45),   S(-12,   8),

            /* bishops: bucket 6 */
            S( -5,  16),   S(-29,  42),   S(-48,  30),   S(-43,  27),   S(-11,   6),   S(-64,  39),   S(-36,  51),   S(  2,  16),
            S( 20,  -1),   S(  7,   1),   S(-43,  40),   S(-11,  24),   S(-24,  40),   S(  9,   2),   S(-132,  62),  S( 43,   3),
            S( 26, -19),   S(  1,   2),   S( 30,  24),   S( 14,  36),   S( 26,  20),   S(-17,  38),   S(  3,  37),   S(-33,  11),
            S(  4,  31),   S( -7,  24),   S( 31,  26),   S( 32,  32),   S( 44,  20),   S( 11,  32),   S(  9,  28),   S(-37,   3),
            S(  9,   3),   S( 57,   9),   S( 27,  18),   S( 16,  38),   S( 75,  41),   S( 53,  27),   S( 43,  20),   S(-31,  46),
            S( 28,   7),   S(-53,  49),   S(-21,  50),   S( 30,  29),   S( 42,  31),   S( 39,  43),   S(-11,  54),   S(-14,  52),
            S(-28,  18),   S(-29,  57),   S(-13,  34),   S(-19,  25),   S( 21,  21),   S(  4,  39),   S( 25,  34),   S(  5,  27),
            S(  0,  57),   S( 12,  35),   S( 23,  30),   S(  9,  22),   S( -5,  51),   S( 17,  14),   S(  5,  11),   S( 23,  11),

            /* bishops: bucket 7 */
            S(-22, -18),   S(-16,  10),   S(-39, -29),   S(-45,   0),   S(-40, -15),   S(-64,   0),   S(-55, -38),   S(-60,  15),
            S(-37, -32),   S(-40, -26),   S(-17, -11),   S(  8, -29),   S(-33,  11),   S(-15, -23),   S(-61,   0),   S(-58, -26),
            S(-49,  -2),   S( -1, -34),   S( 28,  -8),   S( 32,   4),   S(-10,  -1),   S(  8,  -4),   S( -1,  26),   S(-44,  18),
            S(-67,  11),   S( 45, -23),   S( 85, -32),   S( 53,   6),   S(103,  -9),   S(-16,  29),   S( -1,  41),   S( 19,   5),
            S(-15, -18),   S(-20, -22),   S( 68, -53),   S( 99, -31),   S( 76,  13),   S( 82,  14),   S( 10,  28),   S( 10,   5),
            S(-51, -21),   S(-24,   2),   S( 49, -23),   S(-23,  15),   S( 22,   2),   S( 74,  -1),   S( 62,  22),   S(-15,  17),
            S(-18, -35),   S(-26,   9),   S(-26, -10),   S(  4, -15),   S( 12, -33),   S( 28, -14),   S(  8,  28),   S( 22,   8),
            S( -6,  -1),   S(-25,  16),   S(-46,  27),   S( 31, -18),   S( 20, -14),   S( 12, -20),   S( 19,  -3),   S( 27,  35),

            /* bishops: bucket 8 */
            S( -6, -10),   S( -8, -37),   S(-41, -33),   S( -1,   2),   S( -5,  42),   S(-13, -26),   S( -8,  14),   S(  1, -42),
            S( -6,  -1),   S(-39, -46),   S(-22, -44),   S(  3,  12),   S( -8, -10),   S(-17, -20),   S( -4, -50),   S( -8, -14),
            S(  4,   2),   S(-12,  18),   S(-17, -12),   S(  6,  19),   S( -2,  17),   S(-13, -36),   S(-16, -27),   S(-33, -34),
            S( 10,  43),   S(-13,  26),   S( -9,  39),   S(  7,  22),   S(  2,  32),   S(-10,  35),   S(  8,  -3),   S( -3,   2),
            S( 18,  29),   S( 10,  66),   S(-19,  37),   S( 53,  42),   S( 20,  10),   S( 17,  -2),   S( 15,   9),   S(-10,   3),
            S(-25, -37),   S( 10,  23),   S( 46,  30),   S(-20,  11),   S( 37,  44),   S(  6,   7),   S(-23, -10),   S(-19, -23),
            S( -5,   1),   S( 15,  36),   S( -3,  17),   S(  0,  17),   S(  3,  -1),   S(  9,  14),   S( -6, -18),   S( -2, -25),
            S( -2, -12),   S(  6, -13),   S(  6,  -7),   S( -2, -37),   S(-13,   3),   S(-15,  -8),   S( -3,   1),   S( -3,  14),

            /* bishops: bucket 9 */
            S(-26,  -9),   S(-10, -25),   S(-23,   3),   S(-16, -27),   S(-41, -18),   S( -9, -58),   S(-26, -26),   S( 10,   8),
            S(-28, -58),   S(-23, -13),   S(-11, -30),   S(-19,   6),   S(-53,   7),   S(-12, -11),   S( -7,  -9),   S(  6,   6),
            S( 12,  -6),   S( 16,   3),   S(-22,  20),   S(-17,  25),   S( 19,  13),   S( 16,   4),   S(-12, -10),   S( -9,  29),
            S(-11,  11),   S( 39,  20),   S( -9,  45),   S(  0,  18),   S(  0,  17),   S(  2,  20),   S( 11,  -2),   S(-10, -20),
            S( -3,   5),   S( 26,  30),   S( -1,  38),   S( -3,  38),   S( -5,  17),   S(  4,  25),   S(  1,  40),   S( -8, -20),
            S( -8,  19),   S( 15,  52),   S( 20,  36),   S(  9,  33),   S( 16, -15),   S( -3,  16),   S( -1,  22),   S(-29, -19),
            S( 13,  46),   S( 41,  41),   S( -3,  10),   S( -5,  20),   S(  7,  29),   S(  5, -14),   S(  4, -18),   S(-17, -30),
            S(  6, -24),   S( 10,  18),   S(-10,   7),   S(-21,   5),   S(-32, -21),   S(  4,  41),   S(  7,  34),   S( -8, -19),

            /* bishops: bucket 10 */
            S(-14,  19),   S( -9, -57),   S(-30, -21),   S(-10,  11),   S( -7,   6),   S(-38, -40),   S(-12, -30),   S( -6,   0),
            S( 23,  -7),   S(-17,  -7),   S(-10,  19),   S(-37,   4),   S(-37,  -2),   S(-29,  42),   S(-14, -39),   S(-16, -45),
            S(  8, -38),   S(  4,  -9),   S(-18, -12),   S(-26,  21),   S(-31,  34),   S(-34,  40),   S(  0,  50),   S( 16,  38),
            S( -1, -11),   S(-13,   1),   S(  0,   6),   S( 29,  -6),   S( 29,  24),   S(-12,  36),   S( -6,  -8),   S( 15,  22),
            S(-17, -15),   S( -2,  20),   S(-22,  16),   S( 28,  31),   S(-10,  62),   S( 18,  44),   S( 22,  51),   S(  9, -37),
            S( -1, -17),   S(-36,  -3),   S( -4,  42),   S(-33,  18),   S( 18,  28),   S( 37,  32),   S( 10,  21),   S( -3,   3),
            S(-11,   3),   S( -6, -18),   S(-13, -15),   S( 14,   4),   S(-10, -35),   S(  8,   3),   S( 22,  69),   S( 12,  15),
            S(  2,   4),   S(-20, -23),   S(  5,   0),   S(-14,  15),   S( -5,  10),   S(-15, -30),   S(  4,  -4),   S( 10,  36),

            /* bishops: bucket 11 */
            S( -7,  23),   S(-52, -24),   S(-48, -75),   S(-29, -50),   S(-21, -42),   S(-59, -32),   S( -7,  -5),   S(-27, -24),
            S( -8, -10),   S( 10,  -6),   S(-30, -18),   S( -7, -38),   S(-59,  21),   S(-39, -53),   S(-19, -37),   S(-16, -32),
            S(-19, -62),   S(  3, -62),   S(-14, -14),   S(  0,  18),   S(-17,   5),   S(-37,   8),   S(-22, -14),   S(  3,  22),
            S( -7,   1),   S(-13, -56),   S( -5, -14),   S( 19, -30),   S( 30,   4),   S(  6,  52),   S( 28,  45),   S( 15,  26),
            S(-29, -32),   S(-17, -47),   S(-16,  35),   S( 45,  13),   S( 30,  33),   S(-33,  76),   S( 23,  35),   S(  3,  -6),
            S(-13, -60),   S(-38, -12),   S( -7,   5),   S( 28,  33),   S( 28,   8),   S( 29,  43),   S( 19,  51),   S(-28,  -8),
            S( -5,  -9),   S(-23, -35),   S(-23,  10),   S(-12,  -4),   S( -5, -12),   S( 42,   1),   S( -3,  16),   S( -8,   0),
            S( -9,   4),   S(-23, -26),   S( -4,  -7),   S( 12,  15),   S( 13,   0),   S(-15, -17),   S(  6,  13),   S(  4,  12),

            /* bishops: bucket 12 */
            S( -1,  -4),   S( -9, -22),   S( -7, -47),   S( -6, -23),   S( -8, -21),   S(-23, -65),   S( -2,  10),   S( -6,   7),
            S( -3,  -3),   S( -4,  -4),   S( -5,  -8),   S(  1,   3),   S(-11, -29),   S( -7,  18),   S(  2,  20),   S( -4, -13),
            S(  4,   0),   S(-19, -13),   S(-18, -36),   S(-10, -14),   S(  2,  10),   S( -4,  -8),   S(-13, -30),   S( -4, -12),
            S( -9,  -6),   S(  4,  -6),   S(-16, -38),   S( -4, -12),   S( -7,  14),   S(  2,  11),   S( -9,   5),   S( -3,   1),
            S(  2,  -4),   S(  3,  11),   S( -6,  23),   S( -7, -18),   S(  1, -24),   S(  2,  17),   S( -3, -27),   S( -6,  -4),
            S(-24, -32),   S( -9,  27),   S(-25,  38),   S( -2,   4),   S(  7, -17),   S( -7,   0),   S( -1,   5),   S(-10, -21),
            S(  0,   9),   S( -8,  14),   S( -6,  -4),   S(  2,  20),   S(  1,   3),   S(  6,  23),   S( -8, -27),   S(  0,  12),
            S( -3,  -8),   S(  0,  -7),   S( -7,  21),   S(  0,  -7),   S(  7,  19),   S( -1,  -2),   S( -3,   5),   S(  0,  -4),

            /* bishops: bucket 13 */
            S( -9, -46),   S(-10, -17),   S(-16, -14),   S(-24, -24),   S(-13, -22),   S(-10, -13),   S(-10, -25),   S( -3,  15),
            S(  0,   0),   S( -6,   9),   S( -5, -36),   S(-14,  -1),   S(-16,   0),   S( -2,  28),   S( -5, -12),   S(  1,  -6),
            S( -9, -14),   S(  7,  -4),   S(  7,  28),   S(-32,  -4),   S(-15, -37),   S(  3,  14),   S(-10, -36),   S(  0,  21),
            S( -3,  12),   S( -3,   2),   S(-15, -16),   S(-29,   4),   S( 11,  13),   S(  3, -15),   S( -8, -10),   S( -9, -28),
            S( -4,  10),   S(-24,  -4),   S(  2, -26),   S( 36,  -8),   S(  5,   1),   S(  1,  17),   S(-12, -21),   S( -4, -10),
            S( -5, -14),   S(-15, -25),   S(-11,  20),   S( 14,  22),   S(-10,   7),   S(  5,  10),   S(  3,  23),   S( -1, -13),
            S( -4,   3),   S( -6,  21),   S(  5,  11),   S( -1,  16),   S(-15,  -9),   S(  8,  -8),   S(-11, -22),   S(  1,   2),
            S( -6, -27),   S( -4,  11),   S( -3, -10),   S(  9,  15),   S( -8,   4),   S( -4,   8),   S( -4,  14),   S( -5, -17),

            /* bishops: bucket 14 */
            S( -8, -15),   S( -9, -17),   S(-14, -12),   S(-25, -48),   S(-14, -46),   S( -6, -17),   S(-14, -18),   S(-14, -37),
            S(-10, -49),   S( -8, -29),   S(  1,  -5),   S(-26, -40),   S(-13, -14),   S(-12,   7),   S(-16, -22),   S( -1, -20),
            S( -9, -16),   S(-14, -52),   S( -8, -22),   S(-13, -35),   S(-29, -17),   S(-23, -27),   S( -9,   4),   S( -1,  10),
            S(-14, -43),   S(-14,   2),   S( -6,   5),   S(-18,  18),   S( -3,  21),   S( -5,  39),   S(-29, -11),   S( -2,  -7),
            S( -8,  -4),   S( -9,  44),   S(-13, -37),   S( -3, -17),   S( -4,   3),   S( -3,  -6),   S(  4,  16),   S( -2,   8),
            S( -3,  -4),   S(  0,  21),   S( -9,  24),   S( -6,  13),   S( -2,  12),   S( 15,  39),   S( -9,   4),   S( -5,  -8),
            S( -8, -29),   S( -4, -12),   S( -9,   8),   S(  3,   7),   S(-13,  -6),   S(-10, -34),   S( -9, -25),   S( -1,  -3),
            S( -4,  -9),   S( -2,  -5),   S( -1,  -5),   S(-13, -15),   S( -6,  -1),   S(  3,  19),   S(  3,  -7),   S(  3,   7),

            /* bishops: bucket 15 */
            S(  7,  21),   S( 15,   7),   S(-19, -14),   S( -2, -14),   S( -4,  -4),   S(-14, -27),   S( -1,  -6),   S( -1,  -9),
            S(  3,  18),   S(  5,  13),   S(  0,  -7),   S( -8, -13),   S(-18, -37),   S( -2,  15),   S( -1, -21),   S( -4,   0),
            S( -5, -13),   S( -3,  -3),   S(  4,  20),   S(-13, -22),   S(-17,  11),   S(-14,  -5),   S(-12, -15),   S( -2,   7),
            S( -9,  -6),   S(-13,  -8),   S(  0, -27),   S(-27, -52),   S( -4,   8),   S( -3, -18),   S(  5,  25),   S( -1, -16),
            S( -2, -16),   S(-19, -17),   S( -6,  10),   S(-18, -61),   S(  5, -33),   S(-10,  28),   S( -7,   1),   S( -2,  -4),
            S(-10, -34),   S(-24, -26),   S(-13, -14),   S(-29, -21),   S(-14,   0),   S( -7, -12),   S( 10,  39),   S(  5,  17),
            S( -4,  -1),   S( -2, -12),   S( -2, -18),   S( -6,   0),   S(-18, -29),   S(-11,  12),   S(-19,   8),   S(  8,  14),
            S(  2, -11),   S( -2,   0),   S( -2,  18),   S( -6,  -2),   S(-11, -16),   S(-20, -28),   S(-12, -17),   S(  1,   0),

            /* rooks: bucket 0 */
            S(-32,   5),   S( -4, -26),   S( -8,  -3),   S(  1, -10),   S( -4,  11),   S( -6,   9),   S( -8,  17),   S(  2,  15),
            S( 27, -73),   S( 43, -37),   S( 13,  -7),   S(-11,  10),   S( 24,  -7),   S( 11, -15),   S(-32,  27),   S(-47,  45),
            S(  4, -34),   S(  8,  35),   S(  8,  19),   S(  6,   4),   S(-19,  54),   S(  9,  -8),   S(-40,  33),   S(-59,  37),
            S(  3,  -8),   S( 62,  -6),   S( 37,  21),   S( 35,  10),   S(  3,  26),   S( -4,  34),   S(-28,  47),   S(-63,  59),
            S( 50, -13),   S( 96,  -6),   S( 73,   8),   S( 28,  -5),   S( 36,  11),   S(  8,  22),   S(  8,  31),   S(-37,  51),
            S( 65, -49),   S( 84, -33),   S( 60,   6),   S(  4,  29),   S( 10,   5),   S(-30,  36),   S( 32,  17),   S(-51,  58),
            S( 45, -11),   S( 89, -13),   S( -7,  26),   S(  2,  46),   S(  9,  33),   S(  1,  -1),   S(-18,  55),   S(-25,  28),
            S( 37,  22),   S( 13,  48),   S( 22,  38),   S(  5,  48),   S( 50,   3),   S( 17,  -5),   S(-17,  27),   S(-19,  35),

            /* rooks: bucket 1 */
            S(-80,  31),   S(-63,  19),   S(-59,  -3),   S(-41, -13),   S(-20, -27),   S(-26, -19),   S(-36,  -1),   S(-35,   9),
            S(-60,  24),   S(-70,   7),   S(-28, -15),   S(-38, -24),   S(-27, -25),   S(-48, -11),   S(-40, -19),   S(-84,  17),
            S( -1,  19),   S(-52,  52),   S(-31,  18),   S(-55,  34),   S(-39,  18),   S( -3, -15),   S(-20,  -6),   S(-38,  13),
            S(-38,  51),   S(-26,  37),   S( -3,  24),   S(-30,  31),   S(-51,  47),   S(-58,  67),   S(-42,  35),   S(-28,  20),
            S( 27,  44),   S( 19,  42),   S(  5,   7),   S(-58,  55),   S( -5,  26),   S( -2,  34),   S(  8,  19),   S(-54,  29),
            S( 62,   5),   S(-15,  44),   S(  5,  28),   S(-32,  24),   S( 11,   8),   S(-37,  43),   S(  8,  18),   S(-48,  47),
            S(-38,  45),   S( 25,  22),   S( 36,  18),   S(-80,  63),   S(-34,  38),   S(-39,  53),   S(-67,  38),   S(-67,  37),
            S( 16,  37),   S( 47,  15),   S(  0,  36),   S(-43,  60),   S( 19,  19),   S( 20,  27),   S(-20,  42),   S(-10,  15),

            /* rooks: bucket 2 */
            S(-71,  44),   S(-48,  22),   S(-49,  22),   S(-60,  19),   S(-64,  21),   S(-55,  13),   S(-41, -28),   S(-54,  18),
            S(-78,  39),   S(-70,  43),   S(-58,  30),   S(-54,   7),   S(-43,  -2),   S(-57, -15),   S(-71,   9),   S(-71,  22),
            S(-71,  66),   S(-62,  56),   S(-49,  45),   S(-36,  10),   S(-45,  26),   S(-35,  26),   S(-19,  18),   S(-35,  20),
            S(-70,  60),   S(-77,  72),   S(-40,  55),   S(-27,  46),   S(-33,  36),   S(-11,  37),   S(-49,  47),   S(-15,  21),
            S(-26,  53),   S(-64,  70),   S(-42,  51),   S(-23,  34),   S(  2,  27),   S(-15,  45),   S(-32,  46),   S(-49,  49),
            S(-42,  43),   S(-40,  55),   S( -8,  24),   S( -3,  23),   S( 21,  25),   S( 14,  27),   S(-17,  40),   S(-43,  32),
            S(-61,  47),   S(-82,  83),   S( -5,  41),   S( -4,  50),   S(  0,  34),   S( 53,   7),   S(-53,  63),   S(-37,  40),
            S(-39,  68),   S(-15,  59),   S(-60,  57),   S(-48,  55),   S(-61,  72),   S(-26,  57),   S(-33,  72),   S( 24,  22),

            /* rooks: bucket 3 */
            S( -2,  72),   S( -1,  67),   S(  4,  64),   S( 13,  47),   S(  7,  51),   S(-10,  68),   S( -6,  76),   S( -9,  31),
            S(-33,  92),   S( -7,  61),   S(  1,  62),   S(  4,  57),   S( 14,  54),   S(  8,  62),   S( 41,  -9),   S( 11, -40),
            S(-34,  89),   S(-10,  80),   S(-10,  88),   S( 14,  54),   S( 17,  75),   S( 18,  71),   S( 28,  64),   S(  5,  42),
            S(-30,  86),   S(-22,  87),   S( 11,  74),   S( 14,  81),   S( 19,  72),   S(  4, 102),   S( 42,  67),   S( 13,  72),
            S(-10,  96),   S( 11,  85),   S( 27,  56),   S( 28,  69),   S( 46,  67),   S( 50,  66),   S( 89,  59),   S( 56,  41),
            S(-21,  95),   S( 10,  69),   S(  8,  73),   S( 17,  69),   S( 27,  56),   S( 53,  48),   S( 93,  33),   S( 89,  15),
            S(-43, 107),   S(-22, 105),   S(  1,  93),   S( 31,  81),   S( 17,  71),   S( 29,  82),   S( 70,  67),   S(117,  29),
            S(-76, 152),   S( -6, 109),   S( 20,  78),   S( 31,  77),   S( 57,  51),   S( 69,  59),   S( 99,  49),   S(108,  52),

            /* rooks: bucket 4 */
            S(-84,  35),   S(-26, -10),   S(-34,  18),   S( -5,  17),   S(-38,   0),   S( 27, -72),   S( 35, -31),   S(-18, -23),
            S(-28,  13),   S(-66,  14),   S(-14,   3),   S(-23,  31),   S(-21,   0),   S( -2, -24),   S(-13, -25),   S(-15, -23),
            S(  4,  18),   S(-15, -17),   S(-10,   4),   S(-17,  -9),   S( 24,  -1),   S(-22, -14),   S( 40, -17),   S(-49, -16),
            S(-55,  -1),   S(-19,  10),   S( -4,   6),   S( 33,  -1),   S( 35,  15),   S( 25,   5),   S( 18,  13),   S(  0,  10),
            S(-10, -20),   S(-24,  44),   S(  4,  33),   S( 83,   2),   S( -8,  33),   S(-23,  17),   S( 49,  25),   S( 52, -10),
            S( 28,  -8),   S( 25,   2),   S( 52,  45),   S( 28,  22),   S( 44,  21),   S(  6,  39),   S(-13,  26),   S( 12,  32),
            S( 35,  -1),   S( 50,  42),   S(  8,  29),   S( 26,  36),   S( 29,   7),   S( 17,  21),   S( 32,  33),   S( 28,  48),
            S( 33, -43),   S( 32,  47),   S( 26,  36),   S( 18,  -2),   S(  3,  13),   S( -3,  12),   S( -2,  36),   S( 30,  14),

            /* rooks: bucket 5 */
            S(-46,  44),   S(-55,  67),   S(-43,  49),   S(-40,  20),   S(-47,  39),   S(-32,  40),   S( 11,  29),   S(-30,  46),
            S(-24,  40),   S(-27,  48),   S(-94,  77),   S(-52,  45),   S(-34,  39),   S(  8,  18),   S( 23,  12),   S(-13,  13),
            S(  6,  47),   S(-22,  52),   S(-34,  61),   S(-48,  63),   S(-47,  34),   S( 36,   5),   S(-15,  58),   S(-14,  44),
            S(-47,  72),   S( 12,  28),   S(-15,  56),   S( -8,  36),   S( -3,  63),   S( 14,  70),   S( -6,  67),   S( -3,  34),
            S( 28,  69),   S( 10,  71),   S( 34,  40),   S( 28,  51),   S( 61,  48),   S( 13,  68),   S( 47,  76),   S( 39,  51),
            S( 68,  67),   S( 69,  52),   S( 55,  72),   S(  7,  57),   S( 52,  50),   S( 50,  75),   S( 33,  52),   S( 32,  53),
            S( 26,  61),   S( 37,  63),   S( 44,  58),   S( 41,  39),   S( 43,  66),   S( 55,  64),   S( 33,  69),   S( 88,  37),
            S( 99,  36),   S( 56,  35),   S( 31,  60),   S( 29,  41),   S( 36,  54),   S( 53,  43),   S( 54,  55),   S( 26,  55),

            /* rooks: bucket 6 */
            S(-36,  19),   S(-35,  25),   S(-13,  21),   S( -6,   6),   S(-64,  43),   S(-79,  64),   S(-22,  41),   S(-29,  50),
            S(-41,  41),   S(-29,  39),   S(  1,  17),   S(-38,  20),   S(-54,  51),   S(-98,  73),   S(-100,  74),  S( 15,  11),
            S(-78,  86),   S(-15,  37),   S(  5,  35),   S(-36,  40),   S(-16,  32),   S(-40,  49),   S(-67,  92),   S( 23,  24),
            S(-47,  65),   S( -4,  50),   S(-26,  67),   S( 14,  42),   S(-11,  38),   S( 23,  40),   S(-25,  37),   S(-46,  56),
            S(-15,  76),   S( 27,  62),   S( 34,  49),   S( 30,  38),   S( -6,  58),   S( 47,  52),   S( 47,  48),   S( 18,  49),
            S(-20,  71),   S( 22,  66),   S( 69,  41),   S( 30,  34),   S( 42,  48),   S( 59,  52),   S( 52,  49),   S( 61,  50),
            S( 57,  53),   S( 42,  53),   S( 88,  43),   S(109,   9),   S(109,  27),   S( 52,  62),   S( 63,  56),   S( 55,  44),
            S( 47,  78),   S( 24,  67),   S( 55,  47),   S( 20,  58),   S( 75,  35),   S( 62,  48),   S( 61,  66),   S( 22,  59),

            /* rooks: bucket 7 */
            S(-66, -14),   S(-49, -10),   S(-41, -18),   S(-19, -12),   S(  8, -30),   S(-44,  36),   S(-35,   5),   S(  5, -18),
            S(-75,  33),   S(-28,   8),   S(-56,   7),   S(  7, -33),   S(-10,  13),   S(  0,  21),   S(-16,  11),   S(-68,  24),
            S(-91,  47),   S(-45,  16),   S(-17,  10),   S(  5,  -9),   S(  5,   4),   S(-25,  -1),   S( 39, -30),   S(-11,  10),
            S(-42,  21),   S(-12,  16),   S(-26,  26),   S( 15,  16),   S( 11,  14),   S( 34,   2),   S( 26, -22),   S(-22,   7),
            S(-47,  48),   S(  4,  15),   S( 19,  -7),   S( 37,   2),   S( 76,  -9),   S(120,  -7),   S( 44,  12),   S( 14,  -5),
            S(  3,  23),   S(  7,  17),   S( 96, -33),   S( 96, -22),   S( 87, -29),   S( 68,  14),   S( 77,  18),   S( 12,  -5),
            S( -8,  36),   S( 25,  19),   S( 47,  13),   S( 55,  13),   S(119, -22),   S( 90,  10),   S( 48,   3),   S( 47,  -7),
            S( 22,  51),   S(  3,  42),   S( 50,   2),   S(122, -36),   S( 23,   5),   S(  6,   4),   S( 68, -12),   S( 32,  13),

            /* rooks: bucket 8 */
            S(-29, -49),   S(-24,   5),   S(  1, -28),   S(  2, -34),   S( -2, -34),   S(-24, -57),   S(-24, -33),   S( -5,   3),
            S( -4, -24),   S(-20,  -8),   S( -8,   4),   S( 23,  -8),   S( -1, -21),   S(-11,  -5),   S(  3, -47),   S(-12, -71),
            S( 19,  26),   S( 21,  -2),   S( 15, -10),   S(-13,  -5),   S( -4, -50),   S( -1, -26),   S( 13,  38),   S(  3,  -3),
            S(  1, -16),   S(  3,  29),   S(  5,   0),   S( 24,  -5),   S( 16,  25),   S(  1, -14),   S(  8,  -8),   S( -8,  -1),
            S( -9, -16),   S(  8,  41),   S( 12,   4),   S(  7,  -6),   S(  5,  33),   S( 36,   0),   S( 19,   3),   S( 11, -71),
            S(  8,  41),   S(-18,   5),   S( 27,  17),   S( 37, -11),   S(  2, -20),   S(  1, -16),   S( -2, -17),   S( 10,  40),
            S( -2, -13),   S(  6, -37),   S( 17,   0),   S( 10, -40),   S( 18,  -9),   S( 18, -41),   S( 22, -17),   S( 12,  23),
            S(  7, -128),  S( 12, -25),   S(  9,  -7),   S(  5, -17),   S(  3,  13),   S(  6, -35),   S(  6, -13),   S( 25, -24),

            /* rooks: bucket 9 */
            S(-44,  -8),   S( -2, -32),   S(-32, -20),   S(-47,  -4),   S(-14,  11),   S( 14,  -7),   S(-15, -44),   S(-63, -32),
            S( 28, -20),   S( -4, -28),   S( -1,  -2),   S(  1, -25),   S(-22, -17),   S(  9,  21),   S(  2, -43),   S( -8, -35),
            S( 22, -22),   S( 15, -17),   S( 19,   5),   S( -4,  -8),   S(-11, -31),   S( 25,   5),   S( 12,   4),   S(-11,   3),
            S( -6,   1),   S( 12,   9),   S( 22,  14),   S(  3,  -1),   S( -1,   0),   S( 15, -14),   S( 33,  19),   S( 11,  -2),
            S( 22,  16),   S( -7,  18),   S( -9,  18),   S( 25,  11),   S( 30,  19),   S( 23,  27),   S( 21, -11),   S( 18, -20),
            S( 18,  36),   S(-10,   3),   S(  8,  -7),   S( -4,  -4),   S( 27,  16),   S( 32,  28),   S( 10,  40),   S( 33,  20),
            S( 63,  10),   S( 35,   3),   S( 34,  38),   S( 57,   7),   S( 39,   6),   S( 27,  11),   S( 23,   0),   S( 59,  -3),
            S( 61, -63),   S( 50, -41),   S( 17,  17),   S( 32,  16),   S( 19,  56),   S( 24,  52),   S( 29,  -7),   S( 35,  22),

            /* rooks: bucket 10 */
            S(-50, -90),   S(  0, -56),   S(-34, -23),   S(-34, -18),   S(-45,  -2),   S(-37,   4),   S( 21,  -5),   S(-43,  -7),
            S(-25, -12),   S( 17, -15),   S(-19, -46),   S(  7, -10),   S(  4, -26),   S( -6,  -9),   S( 21,   0),   S(  1,   6),
            S(-38,  -7),   S(-23, -32),   S(  1,  -8),   S( 10, -10),   S(-19,  28),   S(  5,   8),   S( 22,  14),   S( 18,  40),
            S(  1,  -2),   S(  8, -19),   S( 15,   2),   S(  2,  25),   S( 17,  -7),   S( -6,  -9),   S( 30,  17),   S( -7, -17),
            S(  0,  14),   S( 21,  -7),   S( 15,  -6),   S( 20, -31),   S(-14, -33),   S( 13, -24),   S( 35,  38),   S(  0,  28),
            S( 42,  36),   S( 26,  30),   S( 31,  28),   S( 12,   5),   S( -3, -15),   S( 13,  19),   S( 32,  32),   S(  5,  32),
            S( 74,  16),   S( 79, -16),   S( 75,   1),   S( 61, -18),   S( 22, -23),   S( 34, -11),   S( 20,  17),   S( 46,  -3),
            S( 57,   6),   S( 20,   4),   S( 34,   8),   S( 32,  25),   S( 20,   9),   S( 39,  14),   S(  3,   6),   S(  8, -23),

            /* rooks: bucket 11 */
            S(-40, -46),   S(-33, -36),   S(-37, -34),   S(-50, -68),   S( 16, -48),   S(-15,  22),   S(-34, -24),   S(-58,   1),
            S(-19, -50),   S( -6, -40),   S( -7, -32),   S(-32, -23),   S(-22, -37),   S(-15,  -8),   S(-13, -31),   S(-12,  -2),
            S(-10, -45),   S( 19,  -3),   S( 19, -32),   S( 24, -17),   S(  3, -12),   S( -9,  24),   S(-15, -33),   S(-21, -64),
            S(  7,  20),   S( 15, -23),   S(  2,  12),   S( 44,   2),   S(  7,  17),   S( -2,  25),   S( 34, -19),   S( 10,  -7),
            S(  8,  24),   S( 11, -27),   S( 18,  18),   S( 29,  -8),   S( 29, -10),   S( 22,  -6),   S( 14,   1),   S(  1, -23),
            S( 32,  20),   S( 65,   4),   S( 41, -27),   S( 48,  11),   S( 59,  -6),   S( 32,  26),   S( -7,  -8),   S( 27,  37),
            S( 81,  19),   S( 65,  16),   S( 81, -48),   S( 80,  18),   S( 50,  -8),   S( 51,  16),   S( 31,  57),   S( 54,  -1),
            S( 54,  31),   S(  9,  23),   S( 33, -25),   S(  7, -13),   S(  1, -29),   S(  7,  16),   S( 14,   6),   S( 33,  23),

            /* rooks: bucket 12 */
            S( 10,   3),   S(-16, -26),   S(-11, -58),   S(-16,  -9),   S( -1,  -1),   S(-14, -45),   S(-30, -57),   S(-13, -18),
            S( 15,  22),   S(  5, -11),   S( -5,  -5),   S(-11, -25),   S( -4,  10),   S( -4, -17),   S(  6,   7),   S( -8, -27),
            S( 10,  12),   S(-17, -36),   S( -4,  -9),   S(-21, -19),   S(-10, -26),   S(  9,  -2),   S( -2,  -4),   S( 10, -12),
            S( -9,  17),   S( 12,  -4),   S(  1,   5),   S( -6, -43),   S(  1,  -8),   S( -7, -34),   S(-15, -21),   S(  0, -23),
            S(-21, -36),   S(  3, -28),   S( 21,  41),   S( 10,   1),   S( -3, -33),   S(  4, -23),   S(-12, -42),   S(  5, -28),
            S( -5,  16),   S(-14, -42),   S( 24,  26),   S( -5, -10),   S(-14, -25),   S( -3, -12),   S( -3, -29),   S(  3,  -2),
            S(  2,   6),   S( -5, -52),   S( -4, -48),   S( 12, -12),   S(  8, -17),   S(-17, -62),   S( -2, -16),   S( -3,  -6),
            S( -9, -55),   S( -1,   1),   S( 17,   2),   S( -8, -12),   S(  8, -17),   S(  5,  -9),   S(-13, -21),   S( 20,  27),

            /* rooks: bucket 13 */
            S(-28, -49),   S( -4, -23),   S(  9,  -5),   S(  5,  14),   S(  2, -12),   S(-16, -40),   S(-10, -64),   S(-14, -18),
            S(-23, -56),   S(-11,  -6),   S(-11, -14),   S( -1, -13),   S( -8, -29),   S( -1, -25),   S( 11,  23),   S( 13,   3),
            S( -7, -47),   S( -4, -17),   S(  2, -38),   S(  4, -32),   S( 17,   5),   S( -4,  -3),   S(  4, -23),   S(  3, -42),
            S(-17, -27),   S( 10,  28),   S(-14, -30),   S(-15, -29),   S( 20,  18),   S(-15, -47),   S( -2, -42),   S(  7, -21),
            S( 16,   9),   S(  5, -24),   S( 21,  22),   S(-14, -23),   S(-13, -36),   S( -9, -24),   S( -9, -37),   S( 11,   3),
            S(-10, -24),   S( 13, -22),   S(-10, -17),   S( 20, -19),   S( 14, -17),   S(  6,  14),   S( 10,  -5),   S(  7,   0),
            S( -2,  -8),   S( 34,  59),   S(  9,  20),   S(-14, -26),   S( 19, -27),   S( 28,  17),   S(  2, -21),   S(  8,  -8),
            S(-16, -108),  S(-19, -89),   S( 12,   5),   S( -6, -19),   S( -1,  22),   S(-17, -26),   S( -4, -24),   S( -5,  -4),

            /* rooks: bucket 14 */
            S( -4, -24),   S(-15, -37),   S(  3, -21),   S(-10, -40),   S(  6,  -9),   S(  4, -15),   S(  8, -19),   S(  3,   6),
            S(-21, -40),   S(-23, -67),   S(-12,  11),   S(-17, -18),   S( -9, -31),   S( -2, -35),   S(  5,  21),   S( -7, -31),
            S(-10, -51),   S( -4,  -7),   S(  4,  -5),   S(-18,   0),   S( -4, -10),   S( -8, -14),   S( 13,  39),   S(  4, -17),
            S( 10,  -9),   S(  1, -28),   S(  0, -10),   S( -3,  18),   S( -4, -35),   S(  5, -37),   S( -3, -17),   S( -6, -25),
            S( -6, -40),   S(  0, -23),   S(  0,  -8),   S( -8, -19),   S( -3,  -8),   S( -6, -12),   S(  0, -12),   S( 13,  32),
            S(  8, -13),   S(  2, -11),   S( -7, -29),   S( 13, -11),   S( -2,   1),   S(-14,  -2),   S(  0, -14),   S( -2,  22),
            S( 18,  18),   S(  8, -24),   S( 10, -19),   S(  6, -35),   S( -1, -56),   S( 15,  16),   S(  6,   0),   S(  2,   8),
            S(  2, -31),   S( 15,   6),   S(-20, -37),   S( 20,  18),   S(-17, -26),   S(  6,   4),   S(  1,   6),   S(  1,  -4),

            /* rooks: bucket 15 */
            S(-15, -76),   S(-11, -34),   S(  2, -33),   S(  4, -14),   S(  4, -19),   S(-10, -39),   S(-12, -60),   S(  0,   0),
            S(-17, -32),   S( -8, -21),   S( -1,  -7),   S( -9, -39),   S(-15, -38),   S(  3, -56),   S(-15, -41),   S( 14,  10),
            S(-10, -34),   S(-13, -18),   S(-15, -49),   S( -3,   2),   S( -1, -24),   S( 11,   3),   S(  0,  30),   S(-15, -34),
            S(  3,  -2),   S( -8, -20),   S(  3,  24),   S( -6, -24),   S(-15, -43),   S(-10, -39),   S(  2, -16),   S( -9, -14),
            S( -7, -18),   S(-14, -12),   S( 12,  -1),   S(  6,   1),   S(  0, -10),   S( 14,  32),   S(  2,   8),   S(  2,  23),
            S( -2,   8),   S( 13,  14),   S(  9,  -1),   S( -6,  -7),   S( -7,  -7),   S(  3, -14),   S(  0, -22),   S( -8, -47),
            S(  2,  11),   S( -4, -35),   S(  8, -43),   S( -3, -35),   S(  0, -14),   S(-13,  -1),   S(  2,  -9),   S(  2,   9),
            S(  6, -31),   S(-16, -31),   S( -1, -34),   S(  4,   1),   S( -4, -13),   S(  5, -14),   S(  1, -14),   S(  1,  -9),

            /* queens: bucket 0 */
            S(-41,   3),   S(-20, -67),   S( 39, -92),   S( 51, -48),   S( 29, -24),   S( 24, -28),   S( 52,  15),   S( 16,  38),
            S(-26,  -5),   S( 45, -57),   S( 44, -28),   S( 26,   5),   S( 30,  15),   S( 15,  36),   S(  8,  61),   S( 22,  38),
            S( 17,  24),   S( 29,  28),   S( 24,  35),   S( 17,  35),   S( 21,   3),   S(  5,  55),   S(  1,  46),   S( 39,  54),
            S( 17,   5),   S( 39,  43),   S(  7,  57),   S(  7,  77),   S( 14,  34),   S( 22,  40),   S( 13,  38),   S( 19,  25),
            S( 46,  -7),   S( 27,  62),   S( 39,  32),   S( 38,  84),   S(  3,  24),   S(  4,   0),   S( 22,  18),   S( 39, -14),
            S(  5,  67),   S(  4,  98),   S( 46,  45),   S( 31,  19),   S( 72, -37),   S( 39,  15),   S( 43,  30),   S( 22, -28),
            S( 48,  60),   S( 76,  27),   S( 16,  68),   S( 40,  36),   S( 23,  -7),   S( -7, -14),   S( 48,  11),   S( 17,  39),
            S( 64,  14),   S( 18,  26),   S( 61,  34),   S( 34,  36),   S( 48,  38),   S(-37,   0),   S( 60,  26),   S( 45,  34),

            /* queens: bucket 1 */
            S( 23, -14),   S(-54, -20),   S(-59,  -5),   S(-25, -86),   S(-10, -53),   S(-24, -65),   S( 19, -62),   S(  9,  14),
            S(-25, -33),   S( -7, -71),   S(  8, -45),   S( -6,  13),   S( -8,  17),   S(  4,   7),   S( 33, -69),   S( -8,  -5),
            S(-36,  66),   S(-12,   6),   S(  3,  27),   S(-10,  17),   S( -7,  32),   S( -9,  21),   S( 14,  21),   S( 19,  31),
            S( -1,  -4),   S( -9,  24),   S(-17,  55),   S(  6,  58),   S(-20,  66),   S( 13,   6),   S(-16,  28),   S( 20,   5),
            S( 15, -30),   S(-15,  59),   S( -3,  51),   S(-20,  76),   S(-13,  73),   S(-18,  39),   S(  1,   4),   S(  4,  34),
            S( 35,  13),   S( 41,  28),   S( 21,  73),   S(-15,  73),   S(-13,  53),   S(-36,  51),   S( 23,   3),   S( 21,  16),
            S(-28,  46),   S(-36,  98),   S(-36,  49),   S(-27,  85),   S(-39,  81),   S( -2,  49),   S(-26,  58),   S( -5,  13),
            S( -8,   0),   S( 44,  39),   S( 24,  40),   S(-32,  17),   S( 13,  35),   S( 11,  18),   S( 10,  21),   S(  5,  16),

            /* queens: bucket 2 */
            S( -7,   9),   S( 16, -54),   S( -3, -30),   S( -6, -27),   S(-50,  40),   S(-45, -11),   S(-39, -45),   S( -3,  24),
            S( 10,  52),   S( 16,   9),   S( 17,  -6),   S( 15,  -5),   S( 18, -40),   S( 12, -51),   S( 16, -38),   S( 28, -29),
            S(  5,  25),   S( 17,   9),   S( 12,  21),   S(  7,  25),   S(  2,  58),   S( 19,  37),   S( 16,  16),   S( 32,  22),
            S(  6,  -7),   S(  2,  53),   S( -3,  47),   S( 10,  61),   S(-12,  73),   S( -1,  92),   S( 21,  12),   S( -2,  51),
            S(  4,  11),   S(  1,  44),   S(-13,  72),   S(-30, 109),   S(-19,  91),   S(-18, 104),   S(-21, 123),   S( -7, 110),
            S(  1,  19),   S(-14,  73),   S(-26,  72),   S( -9,  61),   S(-35,  91),   S(-27, 104),   S( -8, 102),   S(  8,  68),
            S(-26,  48),   S(-39,  85),   S(-39,  88),   S(  7,  65),   S(-20,  96),   S( 17,  34),   S(-36,  60),   S(-36,  82),
            S(-51,  73),   S( 18,  45),   S( 62,  23),   S( 59,  44),   S( -1,  63),   S( 24,  39),   S( 30,  45),   S(-18,  18),

            /* queens: bucket 3 */
            S( 75,  72),   S( 49, 103),   S( 39, 105),   S( 36,  91),   S( 64,  27),   S( 36,  31),   S( 25,  20),   S( 55,  48),
            S( 55, 135),   S( 57, 123),   S( 44, 119),   S( 47,  90),   S( 47,  79),   S( 66,  48),   S( 58,  -2),   S( 30,  34),
            S( 55, 105),   S( 53, 105),   S( 61,  84),   S( 49,  78),   S( 47,  95),   S( 58,  95),   S( 62,  94),   S( 68,  60),
            S( 43, 128),   S( 62,  87),   S( 53,  97),   S( 51,  95),   S( 41,  91),   S( 45, 139),   S( 63, 102),   S( 39, 124),
            S( 43, 110),   S( 57,  98),   S( 44,  96),   S( 47,  91),   S( 48, 121),   S( 21, 125),   S( 34, 163),   S( 44, 165),
            S( 41, 121),   S( 60, 106),   S( 43, 112),   S( 19, 137),   S( 36, 133),   S( 74, 112),   S( 60, 151),   S( 28, 183),
            S( 49, 121),   S( 43, 119),   S( 62, 109),   S( 44, 113),   S( 37, 116),   S( 61, 113),   S( 90, 126),   S(151,  62),
            S( 74,  94),   S( 85,  77),   S( 70,  86),   S( 80,  90),   S( 42, 109),   S(121,  46),   S(159,  47),   S(120,  73),

            /* queens: bucket 4 */
            S( 24,  -4),   S( -8,  -8),   S(-32,  -9),   S( -8, -30),   S(  1, -36),   S( 21, -13),   S(-34, -11),   S(-10,  24),
            S(-32, -24),   S(-41, -20),   S( 18, -32),   S(-67,  27),   S( -7,   5),   S( 20, -15),   S(-14,  -7),   S(-11, -12),
            S(-17, -16),   S(-13, -28),   S( -5,  29),   S( 15,  14),   S( 17,   8),   S( 16,  -1),   S( -2, -17),   S(-23, -23),
            S(-30,   0),   S( -6,  -2),   S(-20,  29),   S( 25,  35),   S( 38,  23),   S( 11,  29),   S(-13, -28),   S( 10,   6),
            S(-12,  10),   S(  7,  -6),   S( 16,  23),   S( 19,  41),   S( 10,  37),   S( 21,  -3),   S(-16, -17),   S(-33, -43),
            S( -3,  13),   S(  9,   0),   S( 19,  53),   S( 46,  40),   S( 28,  30),   S(  1,  15),   S(-23, -21),   S(-16, -12),
            S(-63, -62),   S(-14,  -5),   S(  2,  31),   S( 43,  33),   S( 25,  31),   S(  2,   6),   S(-21, -38),   S(-28, -28),
            S( -7, -16),   S( -9, -10),   S( 47,  62),   S( 12,  34),   S(-32, -21),   S( -9,  -9),   S(-23, -42),   S(-10, -17),

            /* queens: bucket 5 */
            S(-47, -33),   S(-56, -17),   S(  6,   2),   S(-25, -29),   S(-51, -49),   S( 32,  -7),   S(-21, -24),   S(-23,   4),
            S(-18, -10),   S(-42,  14),   S(-79, -33),   S(-45,  19),   S(-39, -25),   S(-63, -10),   S(-39,  -6),   S(-64, -21),
            S(-32, -20),   S(-58,   4),   S(-75,  -9),   S(-33,  40),   S( 40,  33),   S( -9,  15),   S(-17, -16),   S(-13,  26),
            S(-70, -19),   S(-61, -20),   S(-14,  64),   S(-13,  64),   S(  8,  39),   S( -7,  23),   S(-10, -25),   S(-42,   2),
            S( -7,   2),   S(-26,  31),   S(-23,  63),   S( -9,  39),   S( 26,  46),   S( 24,  36),   S( 12,   7),   S(-20,  -8),
            S( -4,  24),   S(  0,  53),   S(-14,  46),   S( 15,  50),   S( 41,  62),   S( -5,  17),   S(  0,  -8),   S(  2,  -2),
            S(-32,  -8),   S( 13,  51),   S( -6,  62),   S( -2,  46),   S( -7,  38),   S( 12,  27),   S( 16,   8),   S(  0,   7),
            S( 35,  34),   S( 17,  -2),   S(  6,  13),   S(  8,  60),   S( 20,  33),   S(  0,  -2),   S(-18, -39),   S(-15, -13),

            /* queens: bucket 6 */
            S(-32,  -8),   S(-50, -30),   S(-41, -37),   S(-66, -49),   S(-79, -24),   S(-78, -50),   S(-49, -44),   S(-53,  -6),
            S(-66, -22),   S(-56,  14),   S(-39,   3),   S(-46,  30),   S(-93,  36),   S(-119,   2),  S(-100, -17),  S( -4,  33),
            S(-43,  32),   S(-40,  10),   S(-34,  43),   S(-92,  71),   S(-37,  46),   S(-59, -17),   S(-60,  -8),   S(  5,  22),
            S(-10,  34),   S(-47,  12),   S(-16,  47),   S(-64,  79),   S( 16,  46),   S( -4,  32),   S( -9,  38),   S(  6,  -3),
            S(-75,  13),   S( 13,  28),   S( -6,  46),   S( 35,  33),   S( 32,  50),   S( 80,  26),   S( 29,  39),   S(-21,   3),
            S(-26,  28),   S(-17,  12),   S( 21,  22),   S( 31,  51),   S( 11,  67),   S( 69,  89),   S(-16,   3),   S(-26,  -8),
            S( -3,   3),   S(  4,   9),   S( 19,  47),   S( -2,  35),   S( 40,  61),   S( 27,  75),   S(-10,  35),   S(-28,   7),
            S( -1,  28),   S( -1,   0),   S( -4,  23),   S( -2,  31),   S( 60,  36),   S( 18,  47),   S(-25,   5),   S( 15,  10),

            /* queens: bucket 7 */
            S(-49, -17),   S(-46,  41),   S(-30, -15),   S(-33,   9),   S(-30, -14),   S(-19, -21),   S(-33, -15),   S(-13, -22),
            S(-73, -12),   S(-32,   7),   S(-39,  24),   S(-19,  33),   S(-29,  30),   S(-62,  66),   S(-26,  12),   S(-40, -14),
            S(-53, -35),   S(-35,   6),   S(-31,  28),   S(  0,  17),   S( 28,   9),   S( 10,  18),   S(-17,  14),   S(-31, -32),
            S(-87,  -4),   S(-30,  13),   S(-15,  26),   S(-13,  41),   S( 58,  13),   S( 33,  28),   S(  5,  41),   S(  7,  -2),
            S(-29,  12),   S(-99,  38),   S(  8,  29),   S( 34,  13),   S( 63,  -3),   S( 89, -15),   S( 68,  14),   S( 63, -23),
            S(-56,  42),   S(  4,   2),   S( 13,  -7),   S( 22,  -6),   S( 42,  30),   S( 76,  38),   S( 76,  12),   S( 44,  28),
            S(  9,  -5),   S( -6,   6),   S(  2,  11),   S(  6,  26),   S( 39,  27),   S( 52,  31),   S( 43,  17),   S( 30,   6),
            S( 10,  17),   S( 18, -29),   S( 15,  -2),   S( 30,  33),   S( 47,  23),   S( 27,   8),   S( 12,  -6),   S( 50,  75),

            /* queens: bucket 8 */
            S( -5, -10),   S(  6,  20),   S(-31, -30),   S( -5,  -5),   S(-11,  -5),   S( -7, -19),   S(-20, -21),   S( -8,  -1),
            S( -7,   6),   S( -6,  -1),   S(-15,   0),   S(  4,  13),   S( -2,   4),   S(-18, -21),   S(-20, -45),   S( -8,  -8),
            S(  3,   7),   S( -4,   6),   S(  3,  16),   S(  2,   3),   S(-27, -29),   S( -7, -10),   S( 11,   2),   S(-14, -28),
            S(  2,   9),   S(  2,  12),   S( 40,  44),   S( 23,  30),   S( 25,  28),   S(-15,   5),   S(-13,  -8),   S( -4, -16),
            S( 13,  26),   S( 18,  42),   S( 14,  29),   S( 24,  15),   S( 16,  31),   S( 21,  25),   S( -4, -13),   S( -1, -11),
            S( 13,  26),   S(  1, -11),   S(-31,   6),   S( 16,  34),   S(-11, -26),   S(-12, -15),   S(  7,   1),   S( -1,   3),
            S( -6, -13),   S( -9,  -7),   S( 27,  36),   S(  9,  13),   S(-10,   4),   S( -9,  -4),   S( -7, -11),   S( -1,   1),
            S(-30, -36),   S( 12,  14),   S(-18, -67),   S( -6,  -5),   S(-13, -24),   S(  3,   5),   S(  7,   7),   S(-11, -14),

            /* queens: bucket 9 */
            S(  7,  12),   S(-12, -26),   S(  3,   0),   S(-12, -11),   S(-25, -42),   S( -9, -22),   S(-17, -30),   S(-23, -44),
            S(  3,   3),   S(-11, -13),   S( -4,  -5),   S(-12, -14),   S(-18,   4),   S(-13, -32),   S(  4,  -7),   S( -7, -17),
            S( 10,  11),   S( -5,  -1),   S( -9,  23),   S( -9,  -7),   S( -3,  -1),   S( -1,   7),   S(  2,   1),   S( -2,   4),
            S(  3,  12),   S(-12,   3),   S(  9,  27),   S( -8, -19),   S( 11,  29),   S(  8,   7),   S(  4,   3),   S( -9, -12),
            S(  0,  16),   S( -8,  15),   S( 16,  54),   S( 29,  73),   S( 26,  41),   S(  5,  -6),   S( 17,  40),   S( -5,  -9),
            S(-21, -26),   S( -5,  12),   S( -1,  14),   S( 19,  46),   S( 10,  17),   S( -4,  15),   S(-18, -18),   S( -9, -12),
            S(-13, -25),   S(  5, -11),   S(-36, -27),   S( 30,  45),   S( 10,  19),   S(  8,  -7),   S( 16,  13),   S(-22, -46),
            S(  2,  11),   S( 16,  12),   S(  1, -29),   S(  9,  33),   S( 19,   6),   S( -5,   3),   S( 15,  15),   S(-10, -28),

            /* queens: bucket 10 */
            S(  3,  -3),   S( 12,  19),   S( -7, -21),   S( -3,  -5),   S( -1,   8),   S(  0,   2),   S( -7, -21),   S(-10, -19),
            S(-18, -36),   S( -7, -28),   S(  4,   1),   S( -9, -10),   S(  1,  10),   S(-20, -22),   S(  2,   2),   S(-21, -25),
            S(  2, -14),   S( -7,  11),   S(-16,  -8),   S( 11,  13),   S( -5,  15),   S( -3,   7),   S(-10, -18),   S( -6, -14),
            S( -2, -13),   S( 11,   3),   S(-11, -15),   S(  4,  28),   S( 21,  28),   S(-18,  12),   S(  2,   1),   S(-10, -16),
            S(-15, -26),   S( 16,  10),   S(  3,  22),   S(  2,  31),   S(-10, -25),   S(  9,  20),   S( 14,  24),   S(  2,  16),
            S( -2,  -8),   S(-10, -22),   S(-10,  -4),   S(  2,  24),   S(  9,  21),   S(  5,  30),   S( 18,  23),   S(  0,  -7),
            S(  4,  -1),   S(-15, -24),   S( 12,  25),   S( -3, -14),   S(  9,  13),   S( -8,  -9),   S( -8, -15),   S( -3,   7),
            S(  6,   1),   S(-11, -37),   S(  8,  -4),   S(  5,  -4),   S( 13,  14),   S( -5,  -4),   S( 27,  39),   S(  5, -12),

            /* queens: bucket 11 */
            S(  1,  -1),   S(-13, -26),   S(-23, -32),   S(-19, -25),   S( -4,  -8),   S(-11, -13),   S( -6,  -4),   S(-19, -32),
            S(-23, -28),   S( -9, -12),   S(-39, -47),   S(-20, -22),   S( -5,  -7),   S(-27, -22),   S(  6,  15),   S( -9, -18),
            S(-34, -49),   S(-18, -34),   S( 13,  -8),   S(  3,  -3),   S(-12,   2),   S(  7,  24),   S(  8,  27),   S(-17,   0),
            S(-11, -33),   S(-25, -18),   S(  9,  -4),   S( 14,  28),   S( 32,  27),   S(-13,  -6),   S( 23,  27),   S( 18,  27),
            S(-15, -11),   S( -7, -28),   S( -7, -20),   S( 27,  26),   S(  9,   4),   S( 17,  30),   S(  7,  25),   S( -8,  -7),
            S(-16, -35),   S(  2,  -1),   S(-25, -26),   S( 17,   3),   S( 38,  -1),   S( 47,  36),   S( 14,   3),   S( -4,  -2),
            S( -3,   3),   S(-15, -29),   S( 32,  35),   S(-12, -13),   S( 14,  18),   S( 39,  49),   S( 34,  28),   S( -6,  -9),
            S(-21, -46),   S(-16, -35),   S(  1,  -3),   S(  9,   3),   S( 10,  17),   S( 13, -11),   S(  4, -11),   S(  4,  -9),

            /* queens: bucket 12 */
            S( 11,  10),   S( -2,  -3),   S( -1,   3),   S(-12, -10),   S(-10, -12),   S(  0,  -1),   S(  0,  -3),   S( -2,  -5),
            S(  1,  -2),   S( -3, -13),   S( -8,  -8),   S( -6, -13),   S( -8, -15),   S( -3,  -3),   S(  0, -13),   S( -3,  -4),
            S(  0,  -2),   S( -2, -11),   S( 21,  23),   S( -5,  -3),   S( -8, -15),   S( -6,  -9),   S(-11, -23),   S(-10,  -6),
            S(  0,   5),   S(  1,   2),   S(  8,  15),   S(-11, -13),   S( 18,  25),   S( -1,  -9),   S( -6, -12),   S( -5, -16),
            S(  3,   8),   S( 17,  27),   S( 37,  67),   S(  5,  26),   S(  7,  15),   S(  4,  17),   S(-19, -37),   S( -7, -21),
            S(  0,   2),   S(-11, -12),   S( 49,  51),   S( -7,   7),   S( -8,  -5),   S(  1,   3),   S(  8,  16),   S( -1,  -7),
            S(  4,   6),   S( -4,  -4),   S( 11,   4),   S( 26,  11),   S( 11,  19),   S( 11,  19),   S(  6,   8),   S( -4,  -9),
            S( -6, -41),   S( -8, -31),   S(-27, -42),   S(-16, -41),   S(  9,   2),   S(  0,  -7),   S(-12, -17),   S( -1,   0),

            /* queens: bucket 13 */
            S( -3,  -9),   S( -1, -13),   S(  9,   9),   S(  1,  -7),   S( -6, -23),   S( -3,  -2),   S( -5,  -5),   S( -2,  -3),
            S( -1,   5),   S( -3,  -6),   S( 10,  13),   S( -4,  -6),   S(  1,   8),   S(  6,  13),   S( -1,   1),   S( -7, -11),
            S( -6,  -9),   S(  6,  11),   S( 16,  42),   S(  6,  21),   S(  2,   8),   S(-15, -24),   S( -8, -12),   S(-14, -24),
            S( -7,  -7),   S( -6,  -3),   S(  6,   2),   S( 30,  55),   S( 23,  40),   S( -4,  -2),   S(-12, -21),   S(  0,  -2),
            S( -6,  -9),   S( 13,  26),   S( 10,  42),   S( 11,  50),   S( 36,  58),   S( -4, -19),   S(-13, -26),   S( -7, -12),
            S(  2,   0),   S( 17,  37),   S( 38,  84),   S( 19,  36),   S(  4,   1),   S(  5,  15),   S(  5,  15),   S( -6, -19),
            S(  1,  -3),   S( 21,  25),   S(  5,  15),   S( -6, -15),   S( -1,  14),   S(  4,  -1),   S( -5, -12),   S(  4,   7),
            S(-17, -15),   S( -6, -18),   S( -8, -29),   S( -6,  -2),   S( -2, -16),   S(  4,  -1),   S(-11, -13),   S(  1,  -1),

            /* queens: bucket 14 */
            S( -2,   5),   S( -2,  -2),   S(  2,  -1),   S(  6,   9),   S(  0,  -2),   S( -7, -15),   S( -9, -18),   S( -8, -18),
            S( -4, -10),   S(  5,  19),   S( -3, -11),   S(  6,   1),   S( -8,  -4),   S(-13, -28),   S( -2,   1),   S(  4,   8),
            S(  0,  -4),   S( -3,  -5),   S( -9, -13),   S( -3,  -6),   S(  1,  -1),   S( -9, -23),   S(  6,   5),   S(  0,  -2),
            S(-11, -14),   S( 11,   7),   S(-13, -12),   S( 31,  49),   S( 19,  16),   S( -2,   2),   S( 13,  36),   S(  2,  -7),
            S(  2,  13),   S(  7,  10),   S(  0,   8),   S( 13,  15),   S( 19,  43),   S( 21,  48),   S(  8,  25),   S( -2, -12),
            S(  0,  -2),   S(  4,  15),   S( 15,  14),   S( 22,  23),   S(  7,  11),   S(  6,  23),   S(  6,  12),   S(  3,  -5),
            S(  9,  21),   S( 12,  17),   S(  7,  13),   S( 11,  22),   S( 12,  21),   S( 13,  25),   S( 20,  37),   S( 12,  21),
            S(  4,  15),   S( -1,  14),   S( -1,  -6),   S(  2,   2),   S(-12,  -7),   S(  6,  10),   S( -1,   9),   S(-16, -23),

            /* queens: bucket 15 */
            S( -1,  -2),   S(  5,   0),   S( -1,  -3),   S(  2,  -8),   S( -9, -16),   S( -5, -12),   S(-10, -19),   S(  1, -10),
            S(  6,  -8),   S(  0,  -9),   S( -5,  -8),   S(  1,  -4),   S( -1,   8),   S(  2,   4),   S(  8,  15),   S(  7,   1),
            S(  6,   3),   S( -1,  -3),   S( -3,  -8),   S( -8, -18),   S( -5, -12),   S(  2,  15),   S( -1,  -8),   S(  4,   0),
            S(  1,   0),   S( -5, -14),   S(  7,  12),   S(  2,  -4),   S(  1,   4),   S( -5, -15),   S( 14,  19),   S(  3,   5),
            S(  6,  19),   S( -4, -10),   S( -8, -16),   S(  5,  16),   S(  2,   2),   S(  3,   5),   S(  1,  -7),   S( -4, -12),
            S(  0,  -2),   S( -3, -11),   S( -6,  -8),   S( -7, -10),   S(  3,   4),   S( 28,  47),   S( 16,   1),   S( -5, -12),
            S( -2,  -5),   S( -8, -26),   S(  7,  11),   S(  7,   7),   S(  4,   5),   S( 23,  42),   S( -5, -16),   S(  6,  10),
            S(  1,   0),   S(  0,  -2),   S( -1,  -4),   S( 18,  30),   S(  6,   1),   S( -1, -19),   S( -5,  -9),   S( -1, -17),

            /* kings: bucket 0 */
            S( 77,  -8),   S( 59,  41),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 39,  33),   S(119,  55),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 46,  18),   S( -7,  34),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 83,  43),   S( 65,  56),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-17,  43),   S(-10,  35),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 51,  64),   S( 50,  52),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -9,  48),   S(-20,  13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18,  82),   S(-37,  52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 24, -63),   S( 64, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-22, -17),   S( 13,  23),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 41, -23),   S(  9,  -3),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11,  25),   S(-15,  30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 59,  16),   S( 20,  13),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  1,  53),   S(  8,  50),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 71,  32),   S( 37, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 29,  70),   S(  4,  31),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -134),  S( 10, -51),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -109),  S(-90, -10),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  8, -49),   S(-17, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-45, -26),   S(-47, -11),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-13, -36),   S(-27, -36),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-39, -24),   S(-112,  27),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -7, -39),   S(-36, -108),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-83,  -5),   S(-27, -83),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -115),  S(-67, -49),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34, -220),  S(-34, -90),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-57, -51),   S(  1, -58),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-59, -66),   S( -4, -116),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-14, -57),   S(-118,  -3),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 30, -122),  S(-73, -64),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-122,   1),  S(-41, -118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-42, -70),   S(-30, -246),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -26),   S(-31,  14),   S(  8,   4),   S( 13, -13),   S( 17,  -8),   S( 47,  11),   S( 42,  -6),   S( 47,   8),
            S(-11, -23),   S(-34,   4),   S( -6, -17),   S(  4, -19),   S( 17,  -5),   S(  3,   9),   S( 27,  -3),   S( 17,  29),
            S(  4, -26),   S( -5, -24),   S( 33, -37),   S( 17, -21),   S( 25,  -9),   S( 19,  21),   S( -1,  46),   S( 35,  14),
            S( 19, -28),   S( 33,  -8),   S( 65, -36),   S( 37,   2),   S( 25,  50),   S(-16,  94),   S( 16,  77),   S( 52,  67),
            S(104, -48),   S(104,  -8),   S( 88,  -2),   S( 54,  10),   S( 53, 111),   S(-24, 135),   S( 24, 150),   S( 72, 141),
            S(-213, -61),  S(-128, -142), S( -1, -128),  S( 34,  44),   S( 82, 201),   S( 88, 166),   S(102, 170),   S( 88, 164),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,  22),   S(-45,  21),   S(-16,   7),   S(-40,  63),   S(-14,   0),   S( 21,   6),   S( 15,  -1),   S( 16,  25),
            S(-58,  21),   S(-42,  15),   S(-34,  15),   S(-17,  11),   S(  4,   8),   S(-13,   6),   S( -2,  -2),   S(-17,  19),
            S(-43,  26),   S(-13,  17),   S(-19,   0),   S( 12, -14),   S(  5,  21),   S(-11,  15),   S(-29,  25),   S( -8,  27),
            S(-28,  40),   S( 11,  25),   S( -9,  20),   S( 14,  30),   S( 10,  31),   S(-30,  47),   S( -3,  40),   S( 24,  55),
            S( 13,  42),   S( 55,   0),   S(113, -31),   S( 78, -18),   S( 34,  20),   S( -4,  37),   S(-38,  75),   S( 46,  93),
            S( 63,  40),   S(-11, -32),   S( -8, -86),   S( -9, -94),   S(-71, -38),   S( 21,  35),   S( 31, 178),   S( 73, 228),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  40),   S(-32,  17),   S(-20,  11),   S( -5,  35),   S(-29,  30),   S(-12,  13),   S( -2, -12),   S(-12,  21),
            S(-52,  34),   S(-44,  29),   S(-32,  11),   S(-27,  19),   S(-25,  18),   S(-40,  10),   S(-15, -14),   S(-45,  14),
            S(-43,  54),   S(-39,  53),   S(-10,  17),   S(-10,  21),   S(-16,  24),   S(-20,   6),   S(-31,   8),   S(-32,   9),
            S(-24,  87),   S(-38,  69),   S(-12,  42),   S(  3,  42),   S( -3,  37),   S(-24,  19),   S(  5,  20),   S( 11,  18),
            S(-26, 143),   S(-42, 122),   S(-16,  35),   S( 35, -28),   S( 90, -10),   S( 94, -11),   S( 65,  -9),   S( 41,  11),
            S(-10, 263),   S( 34, 195),   S( -1,  96),   S( 50, -103),  S(-29, -157),  S(-92, -150),  S(-29, -47),   S( 11,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  3,  15),   S( -3,  11),   S(  8,  12),   S(  6,  49),   S( -3,  44),   S( 31,  24),   S( 20,  -5),   S(  6, -13),
            S( -2,  22),   S( -4,  25),   S( -1,  13),   S(  0,  12),   S( 12,  16),   S( 10,   2),   S( 10, -16),   S(-24,  -3),
            S(  5,  39),   S( -9,  60),   S( 11,  19),   S( 10,   3),   S( 28, -11),   S( 21, -13),   S(  3, -24),   S(-16, -11),
            S(  4,  92),   S(-17, 106),   S( 10,  69),   S( 19,  37),   S( 34,   0),   S( 33, -25),   S( 25,   0),   S( 34, -21),
            S(  4, 158),   S(-26, 182),   S(-22, 165),   S( -6, 108),   S( 32,  43),   S(103, -32),   S(116, -43),   S(107, -40),
            S(117, 120),   S( 46, 248),   S( 36, 262),   S(-11, 226),   S(-25, 100),   S( 13, -172),  S(-67, -243),  S(-143, -185),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 58,  -4),   S( 31,  -7),   S(-35,   7),   S(-47,  16),   S( -1,  -2),   S(-25,   8),   S(-14,  -9),   S(-67,  43),
            S( 21,  15),   S( 17,  11),   S(-15,  12),   S(-10, -15),   S(-28, -19),   S(-29, -10),   S(-41, -24),   S(-33,  -6),
            S( 55, -11),   S(106, -21),   S(  7,  -8),   S(-28,   3),   S(-63,   7),   S( 10, -10),   S(-90,  28),   S(-55,  23),
            S(-78, -75),   S(-49, -84),   S( 70, -43),   S(-23,   4),   S(-12,  16),   S(-74,  69),   S(-35,  61),   S(-39,  68),
            S(  0, -72),   S(-59, -107),  S(-21, -78),   S( 27,  -3),   S( 66,  80),   S( 15, 102),   S( 27,  91),   S( 30,  95),
            S(  2, -70),   S(-15, -62),   S( -4, -117),  S( -5,  13),   S( 45,  81),   S( 57, 131),   S( 52, 165),   S( 34, 123),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-53,  41),   S(-44,  41),   S(-18,  27),   S( 26,  21),   S( 53,  -1),   S( 10,   5),   S(-35,  13),   S(-66,  51),
            S(-70,  36),   S(-37,  40),   S(-22,  26),   S( 12,  19),   S(-11,  20),   S(-29,   8),   S(-66,   5),   S(-86,  39),
            S(-17,  22),   S(-24,  50),   S( 18,  33),   S(  4,  40),   S(-34,  49),   S(-57,  30),   S(-72,  32),   S(-63,  45),
            S(-22,  36),   S( -2,   7),   S(-44, -29),   S( 10, -23),   S(-27,   2),   S(-100,  50),  S( -7,  33),   S(-33,  60),
            S( 57,  17),   S(  7, -49),   S( 86, -101),  S(  1, -76),   S( 40, -39),   S(  6,  22),   S(-22,  64),   S(-24, 119),
            S( 79,  31),   S(  7,   0),   S(-25, -69),   S(-31, -37),   S(-23, -37),   S( 58,  55),   S( 69, 111),   S( 55, 144),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-88,  40),   S(-59,  15),   S(-14,   1),   S( 21,   4),   S(-23,  42),   S( 19,  11),   S(  2,  10),   S( 17,  19),
            S(-64,  18),   S(-63,  22),   S(-53,  14),   S( -1,  23),   S(-21,  31),   S(-20,  15),   S(-13,   9),   S(-27,  19),
            S(-60,  38),   S(-74,  43),   S(-34,  25),   S(-32,  45),   S(-14,  49),   S( 27,  15),   S(  7,  19),   S(-20,  18),
            S(-97,  97),   S(-56,  59),   S(-39,  37),   S(-22,  26),   S(-13, -26),   S( -9, -32),   S(-31,  12),   S( 33,   1),
            S(-24, 112),   S(-29,  68),   S( 10,  26),   S(-21, -25),   S( 11, -75),   S(-79, -58),   S(-35, -23),   S( 51,   9),
            S(111,  70),   S( 74,  92),   S( 50,  22),   S( 73, -97),   S( -7, -79),   S(-30, -41),   S(-18, -32),   S( 67,  -2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,   3),   S(-46, -15),   S(  1, -24),   S(-36,  50),   S( 22,   4),   S( 70, -14),   S( 52, -24),   S( 82, -14),
            S(-60,   2),   S(-58,   0),   S(-24, -21),   S(-25,   2),   S(  6,  -5),   S( 32, -24),   S( 17, -11),   S( 54, -15),
            S(-51,  22),   S(-73,  32),   S(-33,   6),   S(-47,   4),   S( -4,   6),   S( 23, -12),   S( 51, -12),   S( 69, -25),
            S(-39,  57),   S(-90,  84),   S(-52,  63),   S(-36,  45),   S( -7,  -5),   S( 48, -58),   S(  9, -67),   S( 39, -110),
            S( 11,  65),   S(-77, 147),   S( -9, 129),   S(-12,  82),   S( 42,   7),   S( 28, -85),   S(-62, -132),  S(-22, -97),
            S(102,  87),   S(100, 127),   S( 99,  81),   S( 62, 109),   S( 97, -21),   S( -4, -91),   S(-45, -92),   S(  7, -203),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-19,   5),   S(  6,   3),   S( 48,  33),   S(-18, -45),   S(-15, -47),   S(-37, -26),   S( 23, -51),   S( 18, -44),
            S( 39, -82),   S( 30, -22),   S(-17, -48),   S(-47, -25),   S(-37, -49),   S( 43, -68),   S( 14, -66),   S(  0, -55),
            S( 40, -94),   S( 10, -70),   S(  8, -76),   S(-47, -41),   S(-23, -24),   S(-22, -37),   S(-43, -17),   S(-25, -27),
            S(-18, -24),   S(-43, -32),   S( 15, -10),   S( 14,  11),   S(-44,  25),   S(  7,  21),   S(  6,  11),   S( -7,  26),
            S( 43,  25),   S(-15, -21),   S(  2,   9),   S( 48, 100),   S( 56, 112),   S( 29, 136),   S( 40,  98),   S(-18,  94),
            S( 27,  50),   S(  6,  35),   S(  6,  53),   S( 22,  84),   S( 49,  90),   S( 40, 169),   S( 36,  83),   S(-37,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 32,  13),   S( 41,  22),   S( 41,  12),   S( 11,  42),   S( 21,  -5),   S(  2, -13),   S( 30, -53),   S(-33, -14),
            S( 75, -59),   S( 14, -49),   S( 41, -64),   S( -8, -18),   S(-44, -16),   S(-27, -29),   S(-54, -32),   S( 32, -46),
            S(-46, -25),   S( 10, -54),   S( -7, -71),   S(-61, -31),   S( -5, -31),   S(-23, -43),   S(-72, -27),   S( 17, -34),
            S(-46,   7),   S(-56, -47),   S(  1, -41),   S(-20, -28),   S(-28, -25),   S(-23, -10),   S( 11,  -5),   S( -4,  20),
            S( 28,   4),   S(-14, -37),   S(-12,  -6),   S( 28,  18),   S( 12,  71),   S( 17,  59),   S(  1,  73),   S( 10,  60),
            S(-14,  69),   S( 30,  54),   S(  1,  77),   S( 20,  48),   S( 24,  78),   S( 11, 109),   S( 21,  83),   S( 22,  97),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-19, -48),   S( -6, -45),   S(-22, -17),   S( -4, -28),   S( 33,  24),   S( 59,  15),   S( -5,  14),   S(  9, -12),
            S(  4, -65),   S(-52, -50),   S( 10, -64),   S( 39, -37),   S( 11, -18),   S(  4, -31),   S(  3, -40),   S( 17, -47),
            S( -3, -42),   S(-110,  -9),  S(-54, -45),   S(-24, -14),   S(-20, -41),   S(-21, -57),   S(-29, -68),   S( 50, -64),
            S(-48,  -2),   S(-36,  15),   S(-36, -37),   S(-36, -42),   S( -4, -61),   S(-59, -38),   S(-35, -53),   S(-11, -42),
            S(  2,  15),   S( 18,  16),   S( 17,  22),   S(-30, -10),   S(  5,  36),   S( 16,  15),   S(-15,  42),   S( 50,  10),
            S(-19,  42),   S( -8,  35),   S( 29,  68),   S(  6,  80),   S( 25,  85),   S(  0,  34),   S( -5,   9),   S( 19,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-51, -33),   S(-20, -45),   S(-36, -45),   S(  9, -10),   S( -6, -11),   S( 59,  12),   S( 61,  -8),   S( 55, -11),
            S( -2, -62),   S(-56, -61),   S(-51, -57),   S( -4, -49),   S( -3, -39),   S( 22, -52),   S( 42, -56),   S( 95, -83),
            S(-25, -33),   S(-76, -11),   S(-54, -15),   S(-36, -24),   S(-79, -49),   S( 40, -69),   S(  7, -108),  S( 79, -103),
            S(-29,  18),   S(-67,  37),   S( 15,  10),   S( -5,  12),   S(-12, -14),   S(-21, -62),   S(-57, -48),   S( 56, -104),
            S(-14,  24),   S(-17,  83),   S(-29, 107),   S(  2,  52),   S( 36,  75),   S(-11,  17),   S( -1,  24),   S( -3, -24),
            S( -9,  83),   S( 33,  24),   S( 40,  82),   S( 18,  82),   S(  6,  66),   S( 49,  81),   S( 17,  52),   S( 38,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1, -113),  S( 18, -60),   S( -4, -32),   S(  6,   6),   S(  0, -21),   S(-25, -65),   S(  0, -52),   S(  1, -27),
            S( 20, -89),   S( 16, -67),   S(-38, -68),   S(-36, -38),   S(-43, -77),   S(-14, -68),   S(-13, -76),   S(-17, -70),
            S(  8, -74),   S(-17, -85),   S(-22, -105),  S(-52, -106),  S( -6, -47),   S(-26, -29),   S(-13, -70),   S(-17, -88),
            S( -7, -16),   S( 15, -11),   S(-22, -29),   S(  2, -11),   S( 11,  58),   S( 13,  36),   S( -3,  20),   S( -3, -20),
            S( 12,  27),   S( -4,  14),   S( 12,  43),   S( 20,  67),   S( 37,  95),   S( 22,  62),   S( 20,  97),   S( 24,  76),
            S(  3,  15),   S(  0,  46),   S( 17,  73),   S( 17,  66),   S( 30, 105),   S( 29, 116),   S(-22, -17),   S( -3,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -71),   S( 15, -21),   S(  2, -47),   S( -4, -12),   S( 10,  -5),   S(-35, -30),   S(-23, -71),   S(-11, -54),
            S(  1, -113),  S( 36, -116),  S(-22, -118),  S( 19, -26),   S(-20, -45),   S( 29, -67),   S( -4, -107),  S( -4, -77),
            S( 17, -93),   S(-29, -63),   S( 15, -109),  S( 12, -64),   S(-44, -53),   S(  9, -79),   S( -2, -82),   S( 35, -70),
            S( 16, -30),   S( -3, -37),   S(  6, -19),   S(  1,  28),   S( 15,  21),   S(-29,   6),   S(-25, -47),   S( 29, -49),
            S( -3,  29),   S( 14,  36),   S(  3,  -1),   S( 21,  68),   S( 56,  98),   S( 33, 105),   S( 12,  69),   S( 10,  83),
            S(  2,  81),   S( 28,  79),   S( -8,  16),   S( 11,  53),   S( 18,  65),   S( 11,  57),   S( -7,  20),   S( -2,  51),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -123),  S( -1, -68),   S(  3, -55),   S(  0,   3),   S( -5, -22),   S(  1, -21),   S( 35, -82),   S(  3, -61),
            S( 16, -103),  S(-48, -92),   S( -8, -73),   S(-26, -81),   S(  2, -38),   S(  6, -43),   S(  5, -66),   S( 39, -83),
            S( 22, -72),   S( -3, -87),   S(-10, -81),   S(  9, -61),   S(-22, -76),   S(  5, -81),   S(-18, -93),   S( 35, -65),
            S( -4, -27),   S(-25, -59),   S( -3,  -1),   S(-27,   1),   S( 12, -63),   S( 14, -25),   S(  2, -93),   S(  6,   3),
            S( -5,  11),   S(  6,   7),   S(  8,  50),   S(  7,  41),   S( 21,  55),   S(  5,   9),   S( 21,  27),   S( 27,  56),
            S(-16,  14),   S( -4,  51),   S( 14,  52),   S( 14,  68),   S( 15,  59),   S(  3,  48),   S( 29, 103),   S( 14, 102),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2, -18),   S( 17, -56),   S(-24, -46),   S(-13, -25),   S( -6, -27),   S( -3, -57),   S(  6, -44),   S( -2, -71),
            S(-18, -63),   S(-37, -108),  S(-26, -113),  S(-23, -52),   S(-10, -23),   S(  6, -31),   S( 21, -72),   S( 19, -112),
            S(-42, -62),   S(-27, -69),   S(-43, -50),   S(  7, -53),   S(-47, -64),   S(-18, -47),   S(  1, -61),   S( 15, -61),
            S( 12, -40),   S(-14, -40),   S( -7,  43),   S(-15,  33),   S( 24,  28),   S(  0, -31),   S(  0,  11),   S( -2,  29),
            S( 13,  19),   S( -8,  21),   S(  1,  65),   S( 17,  65),   S( 31,  94),   S( 11,  64),   S( 18,  48),   S(  7,  36),
            S(-30, -14),   S(  2, -13),   S( 14,  70),   S( 16,  54),   S( 21,  42),   S( 18,  37),   S( 19,  59),   S(  6,  12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-76, -32),   S(-35, -22),   S( -5,   1),   S( 15,   7),   S(-27, -24),   S(-37,  41),   S(  0, -34),   S(-87, -36),
            S( 35, -54),   S(  2, -23),   S(-12, -47),   S(  3, -11),   S( -7,   2),   S(-10,   7),   S(-20, -65),   S(-15, -29),
            S( -3, -71),   S( 20, -34),   S(  7,  -7),   S( 46,   9),   S(  5,   9),   S( 14,  -5),   S(-28,  19),   S(-26, -35),
            S(  8,  10),   S( 19,  72),   S( 16,  40),   S( 46,  26),   S( 32,  13),   S( 22,  22),   S( 32,  -1),   S(-14, -30),
            S( 55,  53),   S( 25,  50),   S( 58,  72),   S( 62,  36),   S( 79,  13),   S( 11,  16),   S( 24,   4),   S(  0,   0),
            S(107, -40),   S(-18,  45),   S(140,   7),   S(109,  -7),   S( 86,  27),   S(-24,  56),   S( 14,  14),   S( -2,  26),
            S( 43,   6),   S( -4,   5),   S( 39, -10),   S( 68,  70),   S( 54,  25),   S( 24,  -2),   S(-14,  10),   S(-46, -10),
            S(-101, -144), S(  5,  13),   S( -4,  12),   S( 38,  15),   S( -6,  37),   S( 21,  -3),   S(-28,   3),   S(-15,   9),

            /* knights: bucket 1 */
            S( 19,   9),   S(-57,   1),   S(-42,  11),   S(-44,  32),   S(-25,  39),   S(-23, -23),   S(-51, -12),   S(  8, -12),
            S(-31,   1),   S(-62,  49),   S(-28,  37),   S( -6,  29),   S(-24,  24),   S(-22,  47),   S(-24,  17),   S(-17, -55),
            S(-46,  28),   S(  3,   8),   S(-14,  16),   S(  2,  49),   S(-12,  42),   S( -4,  16),   S(-33,  21),   S(-36,  45),
            S(-21,  67),   S( 16,  32),   S(-12,  51),   S( -8,  62),   S( -3,  57),   S( -9,  61),   S( -2,  27),   S(-35,  68),
            S( 38,  20),   S( -1,  26),   S( 38,  67),   S( 36,  54),   S( 35,  50),   S(  4,  69),   S(-18,  44),   S( -9,  51),
            S( 28,  10),   S( 52,   0),   S( 57,  51),   S( 85,  35),   S( 66,  29),   S(-34,  87),   S( 36,  27),   S(  6,  10),
            S( -2, -10),   S(  4, -14),   S( 31, -33),   S( 25,  62),   S(-26,  26),   S( -5,  32),   S( 24,  62),   S( -7,  55),
            S(-175, -66),  S( 31, -17),   S(-31, -66),   S(-15,  24),   S( -3,  25),   S( 30,  22),   S( 25,  31),   S(-99,  23),

            /* knights: bucket 2 */
            S(-73,  12),   S(-35,  -1),   S(-37,   4),   S(-23,  26),   S(-20,   9),   S(-50,   4),   S(-34,   1),   S(-12,  -8),
            S(-35,  12),   S(-15,  42),   S(-31,  32),   S(-13,  21),   S(-30,  28),   S(-12,   3),   S( -9,  13),   S(-32,   5),
            S(-35,  49),   S(-22,  27),   S(-21,  19),   S(-24,  63),   S(-17,  47),   S(-19,  10),   S(-24,  10),   S( -3,  -5),
            S(-19,  60),   S( -9,  36),   S(-29,  78),   S(-14,  78),   S(-36,  73),   S(  3,  58),   S( -2,  44),   S(-14,  46),
            S(-22,  63),   S(-31,  70),   S(  5,  67),   S( 16,  66),   S( -2,  72),   S( 18,  70),   S(-12,  61),   S(  5,   9),
            S(-45,  64),   S(-27,  52),   S(-10,  84),   S( 46,  26),   S( 43,  33),   S(101,   0),   S( 53,  12),   S( 29, -20),
            S(  4,  58),   S(-28,  62),   S( 53,  16),   S( 13,   9),   S(-19,  33),   S(-18,  20),   S( 30,  36),   S( -5, -24),
            S(-56,  53),   S( 24,  40),   S(-15,  65),   S(-36, -43),   S(-44,  -8),   S(-63, -43),   S( 17,  -2),   S(-127, -87),

            /* knights: bucket 3 */
            S(-50,   8),   S(-14, -54),   S(  6, -32),   S(-15,  -9),   S(  2, -14),   S( -3, -23),   S(-21, -29),   S(-58, -48),
            S(-33, -20),   S(  2,  -5),   S( 10,  -6),   S(  2,  -1),   S( -4,   3),   S( 24, -17),   S( 20, -31),   S( 20, -67),
            S( -9,  -7),   S( -7,   3),   S(  5,  19),   S( 10,  42),   S( 21,  27),   S(  3,  15),   S( 17,  -7),   S( 12, -24),
            S( -1,  -2),   S(  1,  36),   S( 20,  40),   S( 10,  59),   S( 12,  77),   S( 28,  60),   S( 26,  51),   S( 10,  37),
            S( -2,  33),   S( 19,  28),   S( 19,  61),   S( 24,  82),   S( 25,  78),   S( 42,  88),   S(  7,  88),   S( 54,  81),
            S( -8,  28),   S(  9,  35),   S( 22,  50),   S(  2,  75),   S( 75,  56),   S(133,  70),   S( 59,  68),   S( 10,  88),
            S(-30,  33),   S(-17,  51),   S(-26,  53),   S( 36,  54),   S( 51,  60),   S( 85,  33),   S(  6, -21),   S( 93,   0),
            S(-169,  47),  S( -3,  63),   S(-36,  60),   S( 44,  22),   S( 73,  63),   S(-56,  55),   S( 11, -27),   S(-89, -140),

            /* knights: bucket 4 */
            S(  6,  15),   S(-24,  -7),   S(-62,   2),   S(-48, -19),   S(-40,  21),   S(-34,  -5),   S(  3, -51),   S(-23, -22),
            S(  4,  27),   S(-13, -41),   S(  6, -10),   S(-15,  19),   S(-13, -12),   S(  4, -52),   S( 13,  17),   S(-66, -14),
            S( 30,  11),   S( 54,  -9),   S( 43,   9),   S( 64,   3),   S( 12,  19),   S( 11, -12),   S(-32, -20),   S( 22, -31),
            S(  2, -33),   S(  6, -18),   S( 67, -21),   S( 82,   0),   S( 17,  22),   S(-22,  35),   S(-41,  44),   S(-12, -11),
            S( 17, -47),   S( 23, -34),   S( 58,  19),   S( 18,  34),   S( 43,  10),   S(  6,  18),   S( 11,  -2),   S(-35,  29),
            S( -3, -21),   S(-14, -20),   S( 26,   1),   S( 93,  19),   S( 12,  48),   S(-11,  38),   S(-27,  15),   S( 18, -14),
            S(-15, -26),   S(-30, -13),   S(  8,  10),   S( 15,  -4),   S( 17,  11),   S( 30,  30),   S(  6,  52),   S(-46, -23),
            S(  5,  10),   S(-13, -37),   S( -7, -49),   S( 22,  16),   S( -1,   2),   S( -3,  33),   S( -1,  35),   S(  3,   0),

            /* knights: bucket 5 */
            S( 19,   3),   S( 10,  24),   S(-56,  40),   S(-23,  13),   S(-17,  44),   S( 21,  16),   S(-26,   3),   S( 14,   6),
            S( 24,  12),   S( 50,  29),   S( -2,  12),   S( -7,   9),   S( 46, -25),   S(-11,   5),   S(-19,  42),   S(-70,  22),
            S(-18,  -6),   S( -3,  -5),   S( 55,  10),   S( 43,  16),   S( 25,  24),   S( -6,  29),   S(-17,  19),   S(-55,  10),
            S( 38,  21),   S( 30, -14),   S( 62,  -7),   S( 64,  -8),   S( 77,  17),   S( 99,   6),   S(  0,  17),   S( 11,  36),
            S( 61,  14),   S( 31, -13),   S(107, -11),   S(133,  -2),   S( 77,  -3),   S( 74,  19),   S( 11,  10),   S( 33,  30),
            S(  5, -31),   S( 40, -20),   S( 17, -54),   S( 16,  21),   S(-12,  -6),   S( 48,  -4),   S(-16,  22),   S( 16,  24),
            S( -5,  25),   S(-40, -69),   S(  6, -50),   S(-11, -27),   S(-11, -58),   S( 30,   8),   S(-21,  49),   S( -7,  39),
            S(-30, -37),   S(-18, -69),   S( 10, -27),   S(-25, -19),   S( 10,   1),   S( 13,  10),   S(  9,  11),   S(  3,  11),

            /* knights: bucket 6 */
            S(  7,   6),   S(-36,  27),   S(-21,   4),   S(-30,  37),   S(-51,  29),   S( 24,  13),   S(-14,  51),   S(-42,  13),
            S( -3, -21),   S(  7,  34),   S(-12,   3),   S( 15,  21),   S( 11,  20),   S(-29,  35),   S(  6,  50),   S(-44,  73),
            S(-18,  15),   S( 44,   1),   S( 34,  16),   S( 18,  45),   S( 29,  35),   S(-38,  47),   S( 29,  22),   S(-15,  31),
            S(  5,  40),   S( 56,  -1),   S( 37,  25),   S( 57,  24),   S( 93,   5),   S( 87,  12),   S( 10,  20),   S( 10,  53),
            S( -4,  41),   S( 62,   3),   S( 63,  12),   S( 84,   6),   S(116, -24),   S( 63,  28),   S(145, -25),   S( -7,  15),
            S(  5,  37),   S( 14,  17),   S( 58,  19),   S( 39,  11),   S( 19,   4),   S( 25,   7),   S( 19,  -8),   S( 38, -20),
            S( 19,  15),   S( 31,  30),   S( 31,  39),   S( 20,   3),   S( 13, -30),   S(  4, -39),   S( 12,   6),   S( 10,  28),
            S( 18,  35),   S(  0,  28),   S(  4,  25),   S( 15,   8),   S( 16,  -6),   S( 12, -16),   S(  3,  13),   S(-14, -44),

            /* knights: bucket 7 */
            S(-41, -57),   S(-16, -25),   S(-17,  -3),   S(-86,  39),   S(-11,  -6),   S(-13,  -4),   S(  4, -22),   S(-19,  23),
            S(-20, -60),   S( 29, -68),   S(-25,  -3),   S( -4, -18),   S( 14,  -9),   S( 42,  12),   S(-13,  25),   S(-59,  38),
            S(-28, -44),   S(-61,  -1),   S( 11,  -7),   S( -4,  34),   S( 45,  19),   S( 39,   9),   S( 35,  15),   S( 38,  15),
            S(-32,   4),   S( 11,  -1),   S( 33,  -2),   S( 61,  12),   S(113,  -5),   S( 69,  27),   S( 86,  -5),   S( 59,   1),
            S(-17,  10),   S( -5,   6),   S( 22,  20),   S( 64,   9),   S(118,   0),   S(120, -12),   S(176,   2),   S( 49, -13),
            S(-33,  -3),   S( 32,   2),   S(-11,  20),   S( 88,   6),   S(108, -11),   S(110,   4),   S( 15,  -1),   S(-11, -53),
            S(-20, -16),   S(  7,  -2),   S(  4,  14),   S( 37,  17),   S( 76,  35),   S( 49,  21),   S(-39, -68),   S(-23, -51),
            S(-47, -62),   S(-25, -34),   S(-26,  -2),   S( -8,   7),   S(  5,  -4),   S( 24,  21),   S(  0,  -7),   S(  3,   2),

            /* knights: bucket 8 */
            S(  2,   1),   S( -4,  -8),   S(  8,   4),   S( -9, -28),   S( -7,   8),   S( -3, -13),   S( 16,  23),   S( -7, -25),
            S(-15, -43),   S(-10, -32),   S( -2, -18),   S(-10,  27),   S( -8,  32),   S(-17, -46),   S( -3,  -8),   S( -6,  -2),
            S(-12, -48),   S(-13, -42),   S(  4, -54),   S( 10,  11),   S(  0, -26),   S( 24,  10),   S( -5, -14),   S(  0, -31),
            S(-23, -63),   S( -6, -20),   S(  7,  33),   S(  4,   1),   S(-17, -20),   S( -9,   0),   S( -4,   9),   S(-24, -47),
            S( -7, -36),   S( 10,  -2),   S( -4, -13),   S( -4,  -1),   S(-22,  20),   S(-25,  -6),   S(  5, -21),   S(  1,   7),
            S( -3, -12),   S( 18, -14),   S( -5,   5),   S( -5, -25),   S(-17,  -4),   S(  7,  -1),   S( -8, -13),   S(  4,  17),
            S( -6,  16),   S( -2, -33),   S(-15,  -6),   S(  1, -18),   S( -1,  -4),   S(  4,  10),   S( -6,  -3),   S(  1, -20),
            S( -2,  -8),   S( -2,  14),   S(  0,  13),   S(  8,  12),   S( -8, -23),   S(  1,  -4),   S( -4,  -1),   S( -7,  -9),

            /* knights: bucket 9 */
            S(-18, -57),   S( -8,  -9),   S(  7, -48),   S(  2, -19),   S(-18, -11),   S(-11,   2),   S(  5,  19),   S(  2,   1),
            S( -2,   0),   S( -9, -48),   S(-31, -122),  S(-15, -63),   S( -9, -17),   S(-27, -60),   S(-14,   0),   S(-10,   6),
            S(-11, -19),   S(-16, -26),   S(-32, -52),   S(-12, -71),   S(-18,  -6),   S(  6, -11),   S( -1,  14),   S(  2,   8),
            S(-28, -41),   S(  0, -36),   S(  1,   1),   S(-12, -49),   S(-10, -50),   S(  5,   9),   S(-11, -52),   S(  9,   5),
            S(-13,  31),   S(-11, -18),   S(  6, -41),   S( 18,   1),   S(-13, -31),   S( -1,  13),   S(-11,  -6),   S( -3, -10),
            S(-18, -34),   S(-32, -31),   S( -9, -15),   S( -5, -10),   S(  1,  27),   S(  1,  24),   S(-11,   4),   S( -6, -10),
            S( -9,   2),   S( 10,  36),   S( -7, -17),   S(-25, -21),   S(  6,   7),   S(  9,  38),   S(-28, -41),   S( -1,  -1),
            S(  2,  -2),   S(  3, -12),   S( -2,   5),   S(  0,  -1),   S( -6,   7),   S( -9, -23),   S( -4,  -4),   S(  0,  22),

            /* knights: bucket 10 */
            S(-10, -26),   S(  0,   5),   S( -2,  -1),   S(-15,   5),   S(-29, -47),   S( -2, -59),   S(  1,   3),   S( -7,  10),
            S(  0, -17),   S( 31,  49),   S(-11, -17),   S( 13, -33),   S( -8, -20),   S(-29, -71),   S(-26,   9),   S(  5,  21),
            S( 10,  22),   S( 12,  15),   S(-16,  19),   S( 15, -29),   S(-14, -45),   S(  1,  -4),   S(-10, -35),   S( -7,   0),
            S( -3,  -7),   S(-26, -32),   S(  8,  -7),   S( -7, -19),   S(  2,  14),   S(  6,  28),   S(  7, -21),   S(  4,   2),
            S(-23, -54),   S(-15, -24),   S(  9,   5),   S(-14,  -7),   S( 15,  35),   S(  5, -40),   S( -6,  -4),   S(  8,  12),
            S(  0,  21),   S(-13,  -6),   S( -2,  -1),   S(-16,  51),   S(-13,   0),   S( -6,  11),   S(-12,  -6),   S( -5,  -2),
            S( 12,  12),   S( -3, -11),   S( -1,  -8),   S( 18, -19),   S(-14,  12),   S(-26, -52),   S(-16,   5),   S(-12, -24),
            S(  3,  13),   S(  5,  26),   S( -2,   9),   S( -4,   8),   S( -2,  13),   S(-12, -20),   S(  7,  23),   S(  3,  12),

            /* knights: bucket 11 */
            S( -9, -33),   S(-32, -29),   S(-10, -20),   S( 26,  38),   S(-45, -52),   S( -8,  -3),   S( -5,  10),   S( 15,  37),
            S( -7,  -9),   S(-22, -63),   S(-16, -47),   S(  5,   4),   S( 18,  11),   S(  2,  -4),   S(-16, -29),   S( -5,  -8),
            S( -2, -17),   S(-13,   5),   S(  3,  12),   S(  0, -11),   S(-15,  28),   S( 22,  -9),   S(  1,  -1),   S( -9, -30),
            S(-16, -11),   S( 19, -10),   S( 17,   4),   S(  6,  17),   S( 23,  -1),   S( 10,  -9),   S( 14,  15),   S( -6, -27),
            S(-19,  -9),   S(  1, -34),   S(-19,  27),   S( -8,   0),   S( 47,  20),   S( 24,  57),   S(-20, -81),   S( -8, -23),
            S( -8, -27),   S(  7, -64),   S(  3,   9),   S(  8,  25),   S(-11,  37),   S(-20, -18),   S( -4, -57),   S( -5,  24),
            S(  8,   7),   S( -4,  17),   S(-21, -38),   S( -3,  -3),   S( 15, -16),   S( -3,  -7),   S(  1, -24),   S(  3,  29),
            S( -4, -20),   S( -3,  13),   S( -9, -29),   S( -6,  -9),   S(  4,  -9),   S( -3, -27),   S(  2,  10),   S( -1,   0),

            /* knights: bucket 12 */
            S(-12, -42),   S( -3, -14),   S( -1, -15),   S(  0,   7),   S( -8,   3),   S(-14, -26),   S( -2,   1),   S( -3,  -5),
            S( -7, -19),   S(  0,  -2),   S(  4, -14),   S( -9, -19),   S( -3, -20),   S( -5,   9),   S(  1,  -2),   S(  2,  -1),
            S( -6,  -9),   S( -5, -18),   S( -7, -10),   S(-17, -17),   S( -2,  20),   S( -6,   0),   S( -9,  -3),   S( -6,  -4),
            S( -2,  -6),   S( -1, -48),   S( -8,  19),   S( -9,  11),   S(  3,  15),   S( -7,  11),   S(  3,  12),   S(  1,   3),
            S( -6, -19),   S(  2,  -2),   S(  5,  13),   S( -3,  -9),   S(  2,  34),   S(-21, -16),   S( -9,  -9),   S(-10, -15),
            S( -4,  -2),   S(  3,  11),   S(  2,   9),   S(  5,   2),   S( -2,   9),   S(-13, -31),   S( 13,  18),   S( -3,  -9),
            S( -7, -18),   S( -2,  -5),   S(-10,  10),   S( -5,  -3),   S(  0,   2),   S(-11, -35),   S( -1,  -3),   S( -5,  -6),
            S(  0,   0),   S(  2,   7),   S(  0,  -4),   S(  1,  -4),   S(  0,  -4),   S(  3,  15),   S( -2, -11),   S( -2,  -6),

            /* knights: bucket 13 */
            S(  2, -10),   S( -7, -16),   S( -6, -24),   S(  6,  17),   S(  1,   4),   S(  5,  19),   S( -3,  -4),   S(  4,  15),
            S(  0,  11),   S( -4,  -8),   S(  5,  14),   S( -6,   0),   S(-19, -32),   S(  5,  12),   S(  5,  31),   S(  5,  18),
            S(  5,   4),   S(  3,  16),   S(  0,   5),   S(  4,   8),   S(  6,  31),   S(-14,  -3),   S( 10,   6),   S( -4,  -3),
            S(  2,   3),   S(  2,  12),   S( -3,  -1),   S( -8,  24),   S(  7,  26),   S(  3,  52),   S(  2,  -9),   S( 18,  41),
            S(  4,  31),   S(  0,  -6),   S(  3,  22),   S( -5,   2),   S(-17,  17),   S( -6,  19),   S( -8, -11),   S( -9, -14),
            S( -7, -17),   S(  1,  -5),   S( -5,   6),   S(  8,  34),   S(-17,   0),   S(-12,  -9),   S(  5,  34),   S(  1,  12),
            S(  1,   4),   S(  5,   9),   S( -4,   7),   S( -9,  -6),   S(  5,  21),   S( -6, -16),   S(  1,   7),   S(  1,   5),
            S(  1,   3),   S( -2,  -8),   S( -3,  -7),   S(  0,   6),   S(  1,   7),   S(  1,   5),   S( -2,  -6),   S(  0,   0),

            /* knights: bucket 14 */
            S( -1,  -6),   S( -2,  -1),   S( 10,  23),   S( -5,  -4),   S( -1, -10),   S( -2,  11),   S(  7,   6),   S(  1,  10),
            S( -3, -16),   S( -3,  -9),   S(  4,  -4),   S(  1,  -7),   S(  4,  10),   S(  2,  10),   S(-12,   4),   S(  2,  39),
            S( -3,   5),   S(-10, -37),   S(  5,  27),   S(-18, -59),   S( -2,  16),   S( -1, -21),   S( -2,  12),   S(  3,   8),
            S(  2,   5),   S( -1, -16),   S(-20, -19),   S( -3,  42),   S(  3,  39),   S( -8,  -3),   S( -4, -18),   S(  4,  40),
            S(  5,   5),   S(-14, -22),   S(-14, -23),   S( -7,  17),   S(  3,  21),   S( -4,  16),   S( -9,   2),   S(  3,   5),
            S(  0,  11),   S(  6,  11),   S( 11,  29),   S( 14,  33),   S(  2,  13),   S(  4,  26),   S( -3,   9),   S( -5, -10),
            S( -3,  -4),   S(  1,   1),   S(  2,   5),   S(  8,  17),   S(  1,   4),   S( -3, -18),   S( -1,   8),   S( -3,  -7),
            S( -1,  -2),   S( -1,   1),   S(  1,   4),   S(  4,  11),   S( -2,  -9),   S( -2,  -5),   S(  2,   7),   S(  0,   0),

            /* knights: bucket 15 */
            S( -2, -12),   S( -1,   1),   S( 13,  48),   S(  2,  12),   S( -9, -27),   S(-13, -46),   S( -1,  -2),   S( -4,  -9),
            S(  1,  -3),   S(  6,   8),   S( -5,   2),   S(  9,  33),   S( 12,  27),   S(-15, -51),   S( -1,  -4),   S(  0,  -2),
            S(  0,  -3),   S( -3,  -7),   S( -2, -14),   S( 12,  40),   S(-29, -66),   S( -5, -14),   S( -2,  -4),   S( -1,  -3),
            S( -1, -10),   S(  1,  24),   S( -6, -16),   S( -2,   8),   S( -8,   2),   S(-13,  35),   S(  5,  -7),   S( -1,  -5),
            S( -2, -10),   S( 10,  26),   S(-11, -19),   S( -9, -28),   S( 26,  33),   S( 10,   6),   S(  3,   8),   S(  5,  20),
            S(  0,  -2),   S(  0, -11),   S(  2,  -6),   S(  2,  -7),   S(-13, -14),   S(  4,  22),   S( -2,  11),   S(  2,   8),
            S( -4, -10),   S( -2, -11),   S(  7,  27),   S(  1, -15),   S(  3,   8),   S(  6,  12),   S( -1,   3),   S(  0,   3),
            S(  0,   3),   S( -2,  -6),   S(  0,   0),   S( -2,  -1),   S(  2,   9),   S( -2,  -2),   S(  0,  -1),   S(  0,   2),

            /* bishops: bucket 0 */
            S( 11,   7),   S(-29,  47),   S(-26,  21),   S(-25,   1),   S( -7,   6),   S( 11, -11),   S( 71, -52),   S( 47, -13),
            S(-28,  -8),   S(  4, -11),   S(  3,  -4),   S(  4,  15),   S(  9,   4),   S( 47,   1),   S( 34,  58),   S(  7,  -6),
            S( 14,  20),   S( 19,  19),   S( 19,  13),   S( 10,   8),   S( 33,   8),   S( 42,  40),   S( 48,   1),   S( 22,   4),
            S( -6,  -7),   S( 33,  -9),   S(  7,  11),   S( 55,   1),   S( 63,  37),   S( 42,  47),   S( 24,  16),   S( 15,  33),
            S( 39, -39),   S( 42, -21),   S( 45,  17),   S( 90,  39),   S( 95,  34),   S(  6,  55),   S( 41,  30),   S(  9,  13),
            S( 39,  57),   S( 59,  34),   S( 92,  35),   S( 39,  29),   S( 20,  16),   S( 18,  36),   S( 55,  24),   S(-37,   8),
            S(-38, -81),   S(105,  27),   S( 90,  92),   S( -8, -15),   S( 17, -31),   S(-28,  36),   S(-24,  35),   S(-46,  68),
            S(-20, -41),   S( 17,   8),   S(  7, -25),   S(-24, -34),   S(-23, -34),   S(-24,  -7),   S(-50,   5),   S( -7, -20),

            /* bishops: bucket 1 */
            S(-47,  32),   S(  4, -14),   S(-27,  37),   S( 25, -16),   S(-30,  40),   S( 13,  -6),   S( 27, -24),   S( 20,   4),
            S(  5, -37),   S( -8,   2),   S(  5, -19),   S(-12,  25),   S( 34,  -8),   S( 12,   2),   S( 65, -21),   S( 17, -30),
            S(-18,  -4),   S( 21, -12),   S( -6,   4),   S( 19,   1),   S( 10,  -9),   S( 37, -19),   S( 18,  -7),   S( 85, -25),
            S( 26, -32),   S( 59, -14),   S( 28, -16),   S( 19,  11),   S( 46,  -2),   S( 19,  19),   S( 84, -27),   S(  6,   7),
            S( 32,  -7),   S( 56, -10),   S( 29,   2),   S(111, -33),   S( 54,  19),   S( 47,  25),   S( 10,  27),   S( 38,  -2),
            S( 58, -29),   S( 42,   9),   S( 62, -22),   S( 89, -17),   S( 93,   3),   S(-23,  32),   S(-33,  58),   S(-32,   2),
            S( -2, -75),   S(  0, -31),   S(-39,  -8),   S( -1,  61),   S( 46,  43),   S( -8,   1),   S( -5,  31),   S(-30,   8),
            S(  7, -17),   S(-39,  28),   S(-20,  -7),   S(-45,  -5),   S(-22,   5),   S( 49, -18),   S( 39,  -6),   S(-59,   7),

            /* bishops: bucket 2 */
            S( -5, -14),   S(-12, -12),   S( -4,   6),   S(-21,   0),   S( -1,  17),   S(-20,   2),   S(  5,  -2),   S(  9, -12),
            S(  7, -24),   S( 10,  -4),   S( -9,  -4),   S(  4,  12),   S(-11,   9),   S(  1,  13),   S(  3, -13),   S( 15, -64),
            S( 42,   2),   S( 12,   4),   S(  6,  18),   S(-12,   0),   S( -3,  29),   S(-21,  -1),   S(  4, -28),   S(-21,  15),
            S( -1,  -4),   S( 53,   1),   S( -4,  10),   S( 22,  29),   S( -8,  15),   S( -5,  24),   S(-11,  -8),   S( 11,   5),
            S( -6,  20),   S( -1,  32),   S( 54,  29),   S( 16,  34),   S(  6,  35),   S( -6,  25),   S(  5,  41),   S( 40,  -4),
            S(-24,  25),   S(-18,  35),   S( -3,   2),   S( 86,  -1),   S( 44,  18),   S( 75,  24),   S( 91,  14),   S( 20, -51),
            S(-62,  74),   S(-24,   8),   S(-28,  18),   S( 12,  24),   S(-58, -10),   S( -9,   1),   S(-12,  -5),   S( -2, -63),
            S(-86, -29),   S( -1, -18),   S( 10,   3),   S(-24,  14),   S(-47,   6),   S(-34,  -3),   S( -5, -35),   S(-28, -35),

            /* bishops: bucket 3 */
            S( 43,   0),   S( 39, -40),   S( 16, -30),   S(  3,  10),   S( 10,  17),   S(  0,  24),   S(-19,  35),   S(  3,  -3),
            S( 32,  -9),   S( 32,  -6),   S( 16,   5),   S( 23,   4),   S( 23,  15),   S( 23,  13),   S( 13,   5),   S( 35, -61),
            S( 10,  -9),   S( 26,  40),   S( 25,  40),   S( 14,  25),   S( 19,  27),   S(  2,  34),   S( 25, -12),   S( 12,   0),
            S( -6,  19),   S( 17,  32),   S( 19,  56),   S( 31,  49),   S( 27,  30),   S( 28,   8),   S( 24, -13),   S( 36, -37),
            S(  2,  39),   S(  9,  53),   S( 10,  55),   S( 49,  57),   S( 46,  51),   S( 39,  23),   S( 30,   5),   S(-14,  22),
            S(  9,  30),   S( 10,  54),   S( -8,  47),   S( 18,  32),   S( 51,  32),   S( 90,  67),   S( 32,  45),   S( 30,  72),
            S(-30,  76),   S(  4,  46),   S( 14,  25),   S( -6,  68),   S(  8,  25),   S( 71,  35),   S(-15,  27),   S( 37, -39),
            S(-39,  36),   S(-54,  44),   S(-62,  36),   S(-25,  31),   S( 21, -19),   S(-78,  44),   S( 32,  11),   S( -8,   9),

            /* bishops: bucket 4 */
            S(-33,  20),   S(-44,   0),   S(-32,  35),   S(-46,  27),   S(-12, -25),   S( 10, -18),   S( -3, -16),   S(-58, -61),
            S(-16, -39),   S(  8,   6),   S( 50, -26),   S(-15,  -3),   S(-32,  12),   S( -2, -10),   S(-24, -10),   S(-24, -24),
            S( -8,  49),   S(-21, -26),   S(-16,  31),   S( -3,  13),   S( 10,  -1),   S(-75,  42),   S( -3,   1),   S(-41,   5),
            S( 10,  13),   S( 40, -14),   S( 21,   3),   S( 48,  17),   S(  4,  -4),   S( 18,  17),   S(-82,  22),   S(-28, -38),
            S( 20,   4),   S(  3, -29),   S( 29, -22),   S( 26,  -3),   S( -4,  10),   S( 20,  -7),   S(-32,  52),   S(-39, -13),
            S(-42, -93),   S(-31,   3),   S(  8,  26),   S( 24,  34),   S(-40,  72),   S( 43,  39),   S( -7,  18),   S( -6,  28),
            S( -6,  -6),   S(-29,  -9),   S( -5, -24),   S(-38,  -2),   S(  4, -33),   S( 30,  12),   S(  4,  19),   S( 34,  40),
            S( -5, -11),   S( -1, -24),   S(-15, -26),   S(  2, -33),   S(-15,  -2),   S(-19,  14),   S(  6,  57),   S(  9,   8),

            /* bishops: bucket 5 */
            S(-38,  -2),   S(-11,  -8),   S(-40,  20),   S(-47,  12),   S( -1, -13),   S(-88,  28),   S(-48,   3),   S(-24,  14),
            S(  1, -25),   S(-70,  34),   S(  7,  -2),   S( -6,  13),   S(-67,  31),   S( -3,  10),   S(-35,  17),   S( 30, -43),
            S(  1,  30),   S(-14,   1),   S( 42,  -8),   S(  6,   9),   S(  0,  18),   S(-26,  14),   S(-20,  18),   S(-23,  13),
            S( 11, -11),   S( 11,  18),   S( 76, -29),   S( 54,   5),   S(  3,  30),   S( 22,  18),   S(-65,  37),   S(-17,  10),
            S(-13,  -4),   S( 30,  -3),   S(-21,  34),   S(-21,  11),   S( 13,  -3),   S(-14,  13),   S( -4,  27),   S(-46,  13),
            S( -5, -31),   S( -6,  -6),   S( 36,   3),   S( 12, -39),   S(-67,  33),   S( -4,  19),   S(-35,  22),   S(-38,  43),
            S( -8, -20),   S(-31,  12),   S(-13, -14),   S(  1,   8),   S( 17,   3),   S( -2,  25),   S( -8,  32),   S(-31,  33),
            S( -7,  17),   S( -5, -20),   S(-23, -27),   S(-20,  11),   S(-28,   8),   S(  6,  34),   S(-35,  23),   S( 26,  15),

            /* bishops: bucket 6 */
            S(-10,   8),   S(-36,  -2),   S(-39,  12),   S(-27,  13),   S(-51,  36),   S(-55,  17),   S(-53,  22),   S(-68,  17),
            S(-49,  24),   S(  4, -16),   S(-66,  26),   S(-34,  27),   S(-75,  42),   S(-28,  11),   S(-61,  38),   S(-30,  -2),
            S( 11,  -4),   S(-42,  27),   S(  7,  11),   S(-19,  21),   S(-13,  28),   S(  5,  -2),   S(-13,  -6),   S(-10,  15),
            S(-60,  32),   S(-70,  38),   S(-17,  26),   S( 42,  38),   S( 11,  32),   S(-25,  40),   S( 21,   9),   S(  6,  33),
            S(-45,  12),   S(-20,  23),   S( -3,  16),   S( 46,  16),   S( -4,  12),   S( -5,   4),   S( 19,  18),   S(-35,   3),
            S(-51,  34),   S( -9,  10),   S(-29,  14),   S(-46,  27),   S( 10,  24),   S( -9,   4),   S(  3,   6),   S(-24, -15),
            S(  1,  35),   S(-76,  56),   S(-56,  17),   S( -4,  16),   S( 10,  12),   S( 15,  -1),   S( -5,  11),   S( -5,  11),
            S(  1,  20),   S(-22,  27),   S(-25,  32),   S( 51,  -8),   S(  9,  -1),   S( 17, -34),   S(-17,  20),   S( -4,  23),

            /* bishops: bucket 7 */
            S(-19, -35),   S(-81,  20),   S(-52, -19),   S(-28, -12),   S(-35,   1),   S(-50,  -2),   S(-55, -44),   S(-54, -14),
            S(-11, -40),   S(-17, -37),   S( 15, -16),   S( -8, -29),   S(-25,  -5),   S(-47,   0),   S(-32, -13),   S(-19, -18),
            S(-58, -37),   S(-57,  24),   S(-15,  12),   S( 14, -13),   S(-20,  23),   S(-20,  -7),   S(-67,  19),   S(-93,  36),
            S(-15, -41),   S(-54,  22),   S(-25,  15),   S(-22,  22),   S(125, -26),   S(  3,  10),   S( 17, -19),   S(-41,   0),
            S(-19, -10),   S(  5,   1),   S(-48,  43),   S(-11,  16),   S( 54, -21),   S( 72,  10),   S(-43,   4),   S(-37, -19),
            S(-78,  32),   S(-68,  74),   S( -5,   8),   S(-95,  38),   S( -1, -15),   S( 18,  -1),   S( -3,  55),   S(-33, -78),
            S(-33,  -8),   S( -4,  32),   S(-37,  21),   S(  4,   4),   S( -5,  -4),   S( 53, -25),   S( 18, -30),   S(-19, -33),
            S(-14, -25),   S(  8,   2),   S( 10,  12),   S(  5,   6),   S(-24,  15),   S(-15, -30),   S( 49, -21),   S(  0,  18),

            /* bishops: bucket 8 */
            S( 46,  59),   S(-19, -82),   S(  6, -18),   S(  7,  41),   S( -5,  15),   S(-17, -59),   S(-14, -29),   S( -9, -13),
            S(  8,  16),   S( 25,  62),   S( 19,   4),   S( -1,   6),   S( -2, -20),   S(  9,   4),   S(-37, -82),   S( -5,  16),
            S(-16, -13),   S(-20, -23),   S( 20,  37),   S( 27,  -2),   S(  3,  26),   S( 24,  20),   S(-32, -17),   S(-32, -33),
            S(  3,  -3),   S( 28,  34),   S(  8,  26),   S( 20, -10),   S( 13,  35),   S( 13,  44),   S(-25,  13),   S(  0,   1),
            S( 13,  -1),   S( 44,  96),   S(  3, -15),   S(-11,   8),   S( 21,  33),   S(-30,  53),   S(-13,  -9),   S(  0,  16),
            S(-14, -19),   S( -3,  -5),   S(  2,  73),   S( 18,  22),   S(  4,  37),   S( 53,   7),   S( -8,  46),   S(  6,  43),
            S(  2,  29),   S(-12, -17),   S( 12, -19),   S( 30,   3),   S(  5, -11),   S( 14,  42),   S( 29,  18),   S( -5,   4),
            S( -9,  -5),   S(  9,  14),   S(  2,  27),   S(  4,  23),   S( 41,  -3),   S( 32,  11),   S(  1,  49),   S( 27,   6),

            /* bishops: bucket 9 */
            S( 14,  42),   S(  4,  -8),   S( -2,  -8),   S(-61, -34),   S(-35, -27),   S(-10, -15),   S(  5,  14),   S( -2,  10),
            S( -8, -30),   S(  6,  -7),   S( -7,  11),   S(-33,  11),   S(-33,  15),   S( -9, -15),   S(-33,  -3),   S(-27, -39),
            S(  4, -20),   S( 11,  15),   S(  0,   7),   S( -5,  29),   S(  4,   8),   S(-13,  -4),   S( -2,   2),   S(-31, -34),
            S(-12,  23),   S( 18, -32),   S( 24,   8),   S( 18,  -8),   S( -6,  32),   S(-17,   8),   S( -2,  35),   S( -3,  18),
            S( 20, -12),   S( 22,  -8),   S( 27,  35),   S( 18, -44),   S( 30,  38),   S( 13,  36),   S(  4,  45),   S( -9, -32),
            S( 13, -10),   S( -5,  34),   S( -2,  35),   S(  4,   0),   S( 35, -72),   S( 11,  37),   S( 19,  36),   S(  4,  14),
            S( 12,  -6),   S( -1,  17),   S(  8,  12),   S( 20,  -9),   S( 21, -18),   S( 17,  55),   S( 34,  37),   S( 21,  35),
            S( 15,  45),   S( -5, -17),   S( -7,  28),   S( 17,  37),   S(  0,  41),   S(  4,  -4),   S( 27,  25),   S( 39,  73),

            /* bishops: bucket 10 */
            S( -2, -23),   S( 10,  25),   S( -6, -33),   S(-15,   0),   S(-59, -11),   S(-33, -80),   S(  6, -17),   S(  7,  41),
            S(-11,  13),   S(-10, -38),   S(-16, -17),   S(-13, -28),   S(-66,  -3),   S(-33, -13),   S(-33,   1),   S( 12,  41),
            S(-24, -49),   S( -5, -41),   S( -3, -35),   S(  0,  32),   S(-13,  -5),   S( -2,  -2),   S( 17,   7),   S(  3,   4),
            S(-24,   0),   S(-33,  29),   S(-15, -19),   S( -6,  10),   S(  0,  44),   S( 34, -14),   S( 35,  16),   S(-19, -36),
            S(  4,  18),   S(-27,  34),   S( 13,  -5),   S( -1,  21),   S( 51,  -4),   S( 24,  32),   S( 34,  -6),   S( 22,   4),
            S(  2,   1),   S( 13, -16),   S( 12,  39),   S( 11,   7),   S( 20, -23),   S(-21,  17),   S(  0,   4),   S( 22,  12),
            S(  0,  20),   S( 15,  28),   S( 21,  -7),   S( 32,  12),   S(-14,  -9),   S(  8, -37),   S( -2,  -6),   S( 16,  21),
            S( 11,  41),   S(  6,  28),   S( 40,  22),   S( 11,  38),   S( -7,  56),   S(  0,  25),   S(  7,  -4),   S( 11,   7),

            /* bishops: bucket 11 */
            S( 14,   5),   S( -6, -14),   S(  6,  11),   S(-17, -34),   S(-16, -21),   S( -7, -19),   S(-27, -41),   S(-11,   1),
            S( -4,  -7),   S( 14, -10),   S(-17,   7),   S( -6, -17),   S(-21,  11),   S(-50,   0),   S(-43, -24),   S( -6, -18),
            S(  4, -34),   S( 13, -22),   S( 24,   2),   S(-42,  21),   S( -7, -27),   S( 22,  32),   S( -9,  -7),   S( -7, -12),
            S( 13,  32),   S( -2, -25),   S( 10,   6),   S(-40, -32),   S( 19,  11),   S(  9,  63),   S( 46,  -4),   S( -8, -17),
            S( -5,   0),   S(-14, -17),   S(-28,  20),   S( -6,   9),   S(-19,  43),   S( 35, -16),   S( 25, -18),   S(  8,  -4),
            S( -5,  -6),   S(-23,   4),   S( -1,  14),   S(-11,  28),   S( 30,  41),   S( 25, -11),   S( -3,  -4),   S( -7,  -9),
            S( -1, -20),   S( 46,  69),   S(  9,  81),   S( 31,  23),   S( -3,  -8),   S(-34, -18),   S(-16, -12),   S( -5,  -3),
            S( 36,  60),   S( -6,  -5),   S( 35,  42),   S( 30, -10),   S( -1,  -1),   S( 12,  31),   S( -4,  -5),   S( 12,  13),

            /* bishops: bucket 12 */
            S(-10, -16),   S(  0, -12),   S( -6,   5),   S(  1,   8),   S( -2,   5),   S( -2,  12),   S(  1,   8),   S( -6, -11),
            S(  5,  -2),   S(  2,   2),   S( -3,  -7),   S( -6,  -4),   S( -9, -14),   S(  7,  -6),   S( -8, -14),   S( -2,  -6),
            S( 11,   3),   S(  9,  -9),   S( 17,  11),   S( 24,  20),   S(  4,  18),   S( -5,  -3),   S(  3,  19),   S(-10,  -5),
            S( 13,   9),   S( 11,  13),   S( 14,   8),   S( 26,  48),   S(  9,  26),   S( -5,  -4),   S(  2,  16),   S(  3,  18),
            S(  7,  -4),   S(  1,  -8),   S( -8,   7),   S( 25,  17),   S( 13,  11),   S( 18,  23),   S(  2, -11),   S(  7,  13),
            S(  4,  -6),   S(-14, -18),   S( -6,  18),   S( -4, -23),   S( 20,  20),   S(  7, -11),   S( -9, -11),   S( -5,  -3),
            S( -2,   0),   S(  1,   0),   S( -1,   3),   S(  3, -13),   S( 11,   4),   S( 22,  16),   S( 12,  33),   S( -1,  -9),
            S(  0,   2),   S(  4,   4),   S(  3,   9),   S(  4,   3),   S( -1,   5),   S(  4,   2),   S( 14,  10),   S( 10,   1),

            /* bishops: bucket 13 */
            S(-10,  -7),   S(  5,  16),   S( -7, -26),   S( -4, -14),   S( 12,   5),   S(-18, -12),   S(-19, -22),   S( -6,  -9),
            S( -6,  -8),   S(-10, -19),   S( -8,   4),   S( 17,  16),   S( -1,  -9),   S( -2,   2),   S(  5,  -5),   S(  3,  -4),
            S(  9,  -9),   S( 27,  12),   S( 14,   1),   S( 34,  27),   S( -1,  19),   S(  8,  23),   S(-14,   1),   S( -7,  -8),
            S( 25,   8),   S( 43,  26),   S( 19,  19),   S(-18,  -1),   S( 22,  73),   S( 10,  10),   S( 12, -10),   S(  1,   0),
            S( 19,  13),   S(  1,  18),   S(  1, -23),   S(  2, -23),   S(  0,  -8),   S(  8,  22),   S( 14,   1),   S( -2,   2),
            S(  8,  12),   S( -2,   2),   S(  5,   1),   S( 20,  26),   S(  3,  -3),   S( -9,   1),   S( -1, -29),   S(  4, -14),
            S(  5,   8),   S( -2, -31),   S( -1,  -9),   S(  7,   3),   S(  2,  12),   S( 17, -17),   S( 12,   8),   S(  5,  -1),
            S(  3,   0),   S(  1,  -3),   S(  2,  22),   S( -1,   7),   S(  9,  32),   S( -2,  -3),   S( 17, -11),   S( 11,  -8),

            /* bishops: bucket 14 */
            S( -4,  -4),   S(  3,   7),   S(  8,  -5),   S(  9,  27),   S(-13, -13),   S( -3, -16),   S( -1,  10),   S( -7,  18),
            S( -1,   4),   S(  4,  -3),   S( 16,  38),   S(  7,   5),   S( 12,  -4),   S( -4,  -4),   S( -4,  39),   S( -2,  28),
            S( -1,  -4),   S( -6, -29),   S( -5,  -4),   S( 20,  46),   S( 21,  69),   S( 21,  17),   S( 11,  27),   S(  1,  12),
            S(  0,  26),   S(  5, -30),   S( -8,  19),   S( -5,  39),   S( 14, -18),   S( 21,   5),   S( 31,  28),   S(  4, -31),
            S( 11,   0),   S(  0,  -3),   S( 17,  16),   S( 30,   6),   S(-15, -31),   S( 10,  -3),   S( 25,   4),   S( 10,  -6),
            S( -6, -36),   S( 11,  23),   S(  7,  33),   S(  7,  12),   S( 12,  12),   S(  0,   7),   S( 10,  16),   S( 10,   5),
            S( 12,  50),   S( 10,  33),   S(  7,   9),   S(  3,  -2),   S(  3,   3),   S(  1,  -4),   S(  4,   8),   S(  4,  -8),
            S( 13,  -6),   S( 11,  15),   S(  4,   0),   S( -2,  -9),   S(  2,  -9),   S(  5,   9),   S(  4,   8),   S(  4,   6),

            /* bishops: bucket 15 */
            S( -1,  -1),   S( -1,  -6),   S( -8, -19),   S( -9, -19),   S(-12, -39),   S(-17, -22),   S( -4, -21),   S( -3,  -7),
            S( 13,  15),   S( -2,  10),   S(  3,  -3),   S( -2,   1),   S(  9,  12),   S( -4,  -2),   S( -1,  -6),   S( -2, -14),
            S( -4, -16),   S( -1, -13),   S(  7,  -6),   S(  1,  27),   S( 12,  30),   S( 15,  27),   S(  6,  27),   S(  1,   3),
            S(  2,  -8),   S( 10,  10),   S( 12,  36),   S(-19,   1),   S( 14,  21),   S( 21,   9),   S( 16,  24),   S(  6,  15),
            S( -3,  -2),   S(  0,  14),   S(  3,  24),   S( 35,  64),   S( 21,   7),   S( 11,   9),   S( 11,   9),   S(-10,  -4),
            S(  1,  26),   S(  7,  10),   S(  7,  43),   S( 11,  23),   S( 23,  28),   S( 13, -11),   S( -2,   6),   S(  3,  11),
            S(  7,  11),   S( 10,  25),   S(  4,   4),   S( 15,  11),   S(  8,   0),   S( -4, -13),   S( -3, -28),   S( -1,  -2),
            S(  7,   4),   S(  2,   2),   S(  7,  -6),   S(  8,   3),   S(  7,  32),   S( -3,  -6),   S(  5,  21),   S(  8,  10),

            /* rooks: bucket 0 */
            S(-14,   9),   S( 14,  -1),   S(  2,   0),   S(  0,   6),   S(-29,  65),   S(-17,  35),   S(-45,  63),   S(-67,  63),
            S(-23,  -1),   S(-20,  35),   S(-27,  11),   S( 12,   8),   S(-12,  49),   S( -4,  10),   S(-31,  34),   S(-44,  70),
            S(  6, -24),   S(  9,   0),   S(  6,   0),   S( -6,  10),   S(-38,  60),   S(-18,  14),   S(-15,  15),   S( -4,  11),
            S( 16, -30),   S( 35,   3),   S(-39,  33),   S(  5,  18),   S( 25,  33),   S(-12,  17),   S(-42,  55),   S(-37,  51),
            S( 46, -59),   S( 30,   9),   S(  7,  31),   S( 32,  19),   S( 62,  -9),   S(  8,  66),   S( 46,  29),   S(  7,  53),
            S( 28,  -8),   S( 82,  24),   S(103, -19),   S(119,  11),   S( 10,  60),   S(  1,  69),   S(-24,  71),   S(-64,  99),
            S( 19,  22),   S( 87,  72),   S(121,  44),   S( 55,  24),   S( 46,  56),   S( 13,  56),   S(-10,  74),   S(-16,  78),
            S( 23, -18),   S( 28,  12),   S( -3,   8),   S( 21,  11),   S( 29,  39),   S( 83,  25),   S( 86,  15),   S( 48, -28),

            /* rooks: bucket 1 */
            S(-58,  55),   S(-19,   8),   S( -2,  16),   S(-35,  33),   S(-37,  47),   S(-41,  54),   S(-50,  71),   S(-76,  84),
            S(-48,  27),   S(-23,  -5),   S(-27,  24),   S(-24,  23),   S(-42,  26),   S(-38,  23),   S(-27,  19),   S(-38,  49),
            S(-52,  35),   S(-19,   4),   S(-36,  28),   S(-32,  30),   S(-38,  26),   S(-30,  30),   S(-54,  46),   S(-36,  57),
            S(-37,  44),   S(-17,  26),   S(-10,  41),   S(-33,  24),   S(-50,  42),   S(-53,  71),   S( -1,  28),   S(-81,  88),
            S(-27,  47),   S( 29, -17),   S( 44,   5),   S( 57,  -3),   S(  9,  14),   S( -1,  65),   S( 19,  26),   S( -4,  64),
            S( 52,  21),   S( 79, -29),   S( 41,   5),   S(-24,  39),   S( 23,   3),   S( 24,  53),   S( 37,  47),   S(-10,  80),
            S( 28,  61),   S( 25,   5),   S(-19,  57),   S(  6,  22),   S( 78,  -2),   S( -5,  59),   S( 55,  74),   S( 45,  77),
            S( 49,  -7),   S( 18, -16),   S( 15, -25),   S( -3,  -1),   S( 23,  12),   S( 21,  20),   S( 40,  33),   S( 68,  22),

            /* rooks: bucket 2 */
            S(-58,  65),   S(-51,  64),   S(-39,  53),   S(-34,  23),   S(-18,  24),   S(-35,  31),   S(-32,  16),   S(-77,  65),
            S(-53,  47),   S(-55,  49),   S(-46,  55),   S(-51,  38),   S(-50,  44),   S(-41,  13),   S(-33,   0),   S(-28,  17),
            S(-41,  47),   S(-31,  51),   S(-40,  42),   S(-29,  32),   S(-44,  41),   S(-24,  21),   S(-20,   3),   S( -8,  32),
            S(-31,  79),   S(-38,  69),   S(-50,  69),   S(-58,  47),   S(-44,  51),   S(-20,  22),   S(-10,  32),   S(-35,  49),
            S(-35,  90),   S(-25,  80),   S(-14,  74),   S(-22,  43),   S(-30,  57),   S( 39,  17),   S( -5,  26),   S( 13,  54),
            S( 31,  75),   S( 13,  73),   S( 30,  55),   S( -9,  40),   S( 62,   7),   S(  0,  53),   S( 95,  12),   S( 37,  69),
            S( 44,  70),   S(-17,  94),   S( 14,  59),   S( 41,  34),   S(-23,  11),   S( 38,  69),   S(-73, 111),   S( 23,  83),
            S( 11,  47),   S( 41,  33),   S( 65,  15),   S( -3,  16),   S(-27,  30),   S( 34,   8),   S( 28,  30),   S( -7,  55),

            /* rooks: bucket 3 */
            S(-14,  68),   S(-11,  73),   S(-11,  92),   S( -7,  84),   S(  6,  42),   S(  9,  33),   S( 22,  17),   S(-10,   7),
            S( -5,  53),   S(-16,  78),   S(-15,  93),   S( -4,  89),   S(  2,  43),   S( 15,   5),   S( 46, -11),   S( 26,  -5),
            S(  4,  60),   S(-15,  79),   S( -1,  70),   S(-14,  97),   S(  6,  36),   S( 10,  27),   S( 34,   9),   S( 27,   0),
            S( -3,  89),   S(-10, 109),   S(-14, 108),   S(  0,  86),   S( -7,  65),   S( 11,  55),   S( 46,  32),   S( -2,  38),
            S( -5, 108),   S( -9, 116),   S( -1, 127),   S( 11, 104),   S(  8,  84),   S( 33,  61),   S( 50,  51),   S( 30,  53),
            S(  3, 118),   S( 11, 112),   S( 23, 116),   S( 37,  93),   S( 85,  42),   S(130,  26),   S( 73,  47),   S( 44,  29),
            S( 16, 114),   S(  6, 111),   S( 10, 122),   S( 20, 113),   S( 29,  97),   S( 82,  43),   S( 84, 142),   S(145,  64),
            S(114, -31),   S( 41,  41),   S( 21, 101),   S( 19,  84),   S( 33,  72),   S( 49,  59),   S(  9,  40),   S(100,  13),

            /* rooks: bucket 4 */
            S(-24, -25),   S( 12, -11),   S( -8, -10),   S(-23,  11),   S(-36,  19),   S( -2,  21),   S(-31,   4),   S(-69,  42),
            S(-64, -32),   S(-30, -21),   S( -2, -16),   S(  5, -25),   S(  9,  -7),   S(-12,   9),   S(-18, -11),   S( -9,  26),
            S(-19, -11),   S(-44, -38),   S(-27,  10),   S(  6, -46),   S(-41,   3),   S(-55,  25),   S(-21,  23),   S(-81,  54),
            S(-56, -19),   S(  3,  12),   S(-12, -12),   S( 19, -19),   S( 22,  -8),   S( -5,  20),   S(  5,  -5),   S( -8,   7),
            S(-23, -68),   S( 11, -24),   S( 48,   1),   S( 38, -13),   S( 49,  -7),   S( 43,  38),   S( 23,  21),   S(  6,  37),
            S( -7, -29),   S( -2, -15),   S( 19,  12),   S( 19,  23),   S( 60,  12),   S(-14,  40),   S( 46,   9),   S( 18,  48),
            S(-17, -23),   S( 23,   2),   S( 51, -20),   S( 61, -17),   S( 73, -17),   S( 10,  -8),   S( 20, -15),   S( 35,   9),
            S( 14, -23),   S(  7,  12),   S( 34,  -7),   S( 24, -17),   S( 35, -11),   S(  7, -12),   S( 10,   2),   S( 25,  20),

            /* rooks: bucket 5 */
            S(-27,  31),   S(  3,  -1),   S( -5,   8),   S( 17,  15),   S( -1,  15),   S(-10,  38),   S( -2,  57),   S(-30,  42),
            S(-11,   2),   S(-15, -12),   S( 44, -52),   S( 50, -23),   S( 18, -15),   S(-43,  25),   S(-69,  46),   S(-10,  52),
            S(-50,  20),   S(-12,   2),   S( 41, -46),   S(  5, -12),   S( -9,  -7),   S( 35, -15),   S(-47,  44),   S(-29,  23),
            S(-40,  25),   S( 26,  -5),   S( 70, -29),   S( 23,  12),   S( 43, -14),   S( -9,  35),   S(-13,  44),   S( -8,  47),
            S( 36,  20),   S(-22,  33),   S( 20,  14),   S(-25,  -6),   S(-43,  35),   S( 92,  12),   S( 44,  27),   S( 66,  29),
            S(  0,  57),   S(-27,  12),   S( 14,  13),   S( -9,   1),   S( 40,  16),   S( 24,  22),   S( 89,  22),   S( 40,  50),
            S( 55,   1),   S( 31, -10),   S(-21,   7),   S( 20,   4),   S( 40, -11),   S( 67, -39),   S(109, -23),   S( 59,   4),
            S(-13,  38),   S( 12,   5),   S( 38, -11),   S( 15,  25),   S( 68,   7),   S( 24,  30),   S( 21,  38),   S( 56,  38),

            /* rooks: bucket 6 */
            S(-64,  55),   S( -8,  18),   S(-17,  14),   S( -6,   7),   S( -3,   6),   S( -8,   5),   S( 35, -19),   S(-26,  10),
            S(-31,  15),   S( 28,  -2),   S( -9,  19),   S(  4,  -2),   S( 22, -22),   S(-39,  -7),   S(-25,  -7),   S(-18,  20),
            S(-50,  31),   S( -1,  17),   S(-19,   6),   S( -1,   2),   S(-28,  13),   S( 41, -15),   S(  2, -13),   S(  0, -23),
            S(-41,  60),   S( 15,  37),   S( 15,  17),   S( 39,  -5),   S( 20,  -7),   S( 28,   1),   S(-27,  15),   S(  8,  34),
            S(-14,  49),   S( 49,  28),   S( 89,  22),   S( 55,  -1),   S( 19,  -7),   S( 11,   5),   S( 62, -16),   S( 75,   6),
            S( 83,  21),   S( 83,   3),   S( 83,  11),   S( 40,  -3),   S(-17, -25),   S( 32,  27),   S( 31, -13),   S( 86,  21),
            S( 41,   9),   S(142, -33),   S(135, -51),   S( 90, -35),   S( 35, -16),   S( 55,  -7),   S( 45,  -6),   S( 93, -26),
            S( 93, -24),   S( 42,  14),   S( -1,  23),   S( 78, -15),   S( 88,  -4),   S( 42,  27),   S( 56,   9),   S( 25,  25),

            /* rooks: bucket 7 */
            S(-89,  24),   S(-86,  44),   S(-66,  39),   S(-45,  29),   S(-25,  -2),   S(-15, -20),   S(-15,  -6),   S(-56,  -9),
            S(-70,  32),   S(-23,   1),   S(-45,  16),   S(-61,  29),   S(-29, -22),   S(  8, -28),   S( 12,   5),   S( -3, -44),
            S(-103,  42),  S(-67,  19),   S(-34,  13),   S(-47,  27),   S( -4, -27),   S(-32,  12),   S( 38, -23),   S( 11, -56),
            S(-85,  43),   S(-29,  28),   S(-10,  15),   S( 82, -29),   S(-16,  -2),   S( 35, -11),   S( 51,  -1),   S( 18, -27),
            S( 13,  34),   S( 23,  21),   S( 56,  27),   S( 93, -10),   S(155, -44),   S(106, -62),   S( 70, -24),   S(-50, -30),
            S( 15,  28),   S( 28,   9),   S(113,  -4),   S( 85,  -6),   S( 93, -13),   S( 47,   3),   S(  5,   7),   S(  3, -30),
            S( -2,   1),   S( 57, -19),   S( 89, -32),   S( 96, -35),   S(121, -50),   S(101, -46),   S( 44,  16),   S( 34, -53),
            S(-12, -22),   S(  4, -17),   S( 64, -11),   S(  6,   9),   S( 13,   2),   S( 57,   1),   S( 19,  42),   S( 39, -12),

            /* rooks: bucket 8 */
            S(-27, -58),   S(-15, -38),   S(-20,  -1),   S( 15,  -6),   S(-26, -13),   S(-24,  11),   S(-23, -56),   S(-35,  20),
            S(-37, -91),   S( -9, -32),   S(-31,  21),   S(-30, -62),   S(  1, -29),   S( -8, -21),   S( -5,   8),   S(-51, -28),
            S( 19,  26),   S( -6, -12),   S( 21, -13),   S(-39,  11),   S( 24,  49),   S( 33,  32),   S( -3,  60),   S(-25,  25),
            S( -1, -16),   S( -4,  -1),   S( -7,  11),   S(  0, -10),   S(  0,  73),   S( 41,  52),   S( -6,  24),   S(-12, -41),
            S( -4, -26),   S(  4,   3),   S(  3,  10),   S( 16,  23),   S( -3,  -9),   S(-24,  -2),   S( 20,  45),   S( -9,  38),
            S(  2,  33),   S( -5,  19),   S(-18,   7),   S(-14, -41),   S( -7,  49),   S(-18,  18),   S(-24, -17),   S( -5,  -1),
            S( 13,  32),   S(  1,  47),   S( 13,  27),   S( 12,  14),   S( 19,  -4),   S( 14,  52),   S(  2,  22),   S(  3,  65),
            S(-11,  39),   S( 13,  28),   S(-24,  28),   S( 28,   8),   S(-18,  -2),   S(  8,  37),   S(  8,  28),   S( -8,  34),

            /* rooks: bucket 9 */
            S(-40, -66),   S(-32, -65),   S( 13, -79),   S( 18, -50),   S(-13, -43),   S(  0, -50),   S(  3, -30),   S( 23, -25),
            S(-65, -25),   S(-32, -53),   S(-32, -47),   S(-50, -66),   S(-55, -38),   S(-38,  17),   S(-34, -47),   S(-17, -39),
            S( -9, -11),   S(-17, -12),   S( 21,  44),   S(-12, -45),   S( 13,   0),   S(-12, -19),   S( 12,  17),   S( 12,  23),
            S(-25,  -1),   S( 12,  -2),   S( 10,  25),   S( -3,  -7),   S(-25, -39),   S(  1,   9),   S(-12, -12),   S( -3, -25),
            S(  1,   8),   S(-26, -19),   S(-12, -30),   S(-20, -11),   S(-24, -24),   S(-24,  11),   S(-17, -13),   S( -8,   7),
            S(-29, -28),   S(-50, -25),   S( -8, -28),   S( 12,  30),   S( -9,  18),   S(-18,   3),   S( -2,   4),   S(  2,   7),
            S( 18,  37),   S(  7, -10),   S( 18, -44),   S(  7,   3),   S(  5,  -4),   S( 10,  -5),   S(  9,   9),   S(-13, -34),
            S(-15,  17),   S(-12,  32),   S( 17,  13),   S(  6,  40),   S(  0,  26),   S( -8,  47),   S( -9,  15),   S(  9,  28),

            /* rooks: bucket 10 */
            S(-20, -34),   S(-64, -24),   S( -7, -53),   S(-24, -41),   S( -2, -48),   S(  7, -60),   S( 15, -97),   S(-34, -46),
            S(-32, -22),   S(-52, -23),   S(-50, -39),   S(-57, -61),   S(-28, -63),   S(-36, -60),   S( -4, -49),   S(-30, -70),
            S( -3, -23),   S(-27, -22),   S(-56, -14),   S(-33, -35),   S(-12, -16),   S( 11,   8),   S( -1, -11),   S(-30, -27),
            S(-12,  14),   S(-32, -10),   S( -4, -35),   S( -5,  14),   S(  3,  -7),   S(  5,   8),   S(-11, -45),   S( 18, -16),
            S( -6,  -6),   S( 12, -14),   S( 10, -10),   S( -2, -49),   S( 12,  17),   S(  4,  33),   S( -4, -21),   S(-17, -46),
            S(  2,   0),   S( 22,   3),   S(  1, -26),   S( -4, -36),   S(  6, -28),   S(  7, -10),   S( -9, -33),   S(-18,  -5),
            S(  5,   0),   S( 29, -13),   S( -4, -21),   S( -6,  -7),   S( 22,  -1),   S(-18,  -1),   S(-11, -41),   S(-10,  -6),
            S(-17, -11),   S( 15,  42),   S( -2,  27),   S(-23,  27),   S( 13,  26),   S(-24,  16),   S( -2,  28),   S(  4,   4),

            /* rooks: bucket 11 */
            S(-43, -24),   S(-38,  -2),   S(-36, -24),   S( -2,   1),   S(-51, -24),   S(-29, -10),   S(-19, -61),   S(-40, -62),
            S(-15,  -3),   S(-42, -17),   S(-55, -14),   S(-46,   2),   S(-40,   0),   S(-12,   0),   S(-21, -43),   S(-58, -78),
            S(-12,  13),   S( -4,  29),   S( 19,  38),   S(-16,  22),   S( 20,  -8),   S(-14,  11),   S( 16,  12),   S(-18,   3),
            S(-33,  13),   S(-25,  -2),   S(-14,  -4),   S( 17,  26),   S( 19,  25),   S(-29, -54),   S( 11,  17),   S( -7, -19),
            S(-20,   0),   S( -5, -14),   S(  2, -11),   S(  2,  26),   S( 24,  -2),   S(  3,   0),   S( 18,  38),   S(-15, -57),
            S( 15, -31),   S(-13, -13),   S( 18, -21),   S( 15, -17),   S(  6, -21),   S(  4,  19),   S(-11,  36),   S(  8, -17),
            S(-38,  17),   S(-34, -32),   S(  3,  10),   S(-12, -24),   S( 10,   0),   S(-10,  14),   S(-16,  17),   S(-12,   9),
            S( -7,   6),   S( 23,  32),   S( 11,  35),   S( 29,  21),   S(-18,   0),   S(-15,  22),   S(  1,  -7),   S(-30,  43),

            /* rooks: bucket 12 */
            S(-31, -90),   S( -2,   3),   S(-14, -47),   S(-35, -61),   S( -8,  -3),   S( 16, -12),   S(-20, -21),   S( -9, -19),
            S(  6,   9),   S(  5,  16),   S(  6,  20),   S( 10,  15),   S( -1, -19),   S( 22,  28),   S( 12,  24),   S(-27, -29),
            S( -9, -25),   S(  2,  38),   S(  6,  12),   S( 20,  -4),   S( -7, -18),   S( 14,  28),   S( 11,  21),   S( -4,   9),
            S( 12,  28),   S(  7, -15),   S( 19,  42),   S(  4,  10),   S( 20,  18),   S(  6,  12),   S(  6,  29),   S(  2,  18),
            S( 10,   0),   S( 21,  36),   S(  3,  10),   S( -5, -29),   S(  1,   8),   S( -8, -21),   S(  2,  18),   S(  1,   1),
            S(-12, -35),   S(  1,  -5),   S( -5, -17),   S(-19,  -4),   S(  2,  10),   S(  8,  -2),   S( -1,  12),   S(  1,  17),
            S( -6,   2),   S( -6,  19),   S( 15,  50),   S( 18,  39),   S( -8, -14),   S( 16,   9),   S(  2,  23),   S(  3,  28),
            S( -2,   5),   S(  1,  51),   S( -8,  23),   S( 17,  36),   S( -4,  -5),   S(  2,  45),   S( -4,   0),   S(  9,  36),

            /* rooks: bucket 13 */
            S(-33, -27),   S(-25, -68),   S(-22, -48),   S(-22, -53),   S(-18, -44),   S( -4,   8),   S(-27, -30),   S(-20, -53),
            S(-12, -31),   S(-15, -26),   S(  0,  -1),   S( -2, -11),   S( 14,  41),   S( 12,  25),   S(  1,  -3),   S(-10,  -2),
            S(-13, -25),   S( -3,  13),   S(  1,   2),   S(  0,   2),   S(  8,   6),   S( 18,  -8),   S( 11,  39),   S(  1, -16),
            S( -6, -13),   S(  1,  14),   S( -5,   3),   S( 11,  28),   S( -1,   7),   S(  8,  17),   S(  5,   1),   S( -4,  19),
            S(  0,  10),   S(  2,  20),   S( -2, -13),   S( 13,  36),   S( -1,  27),   S( -5,   2),   S( 11,  25),   S( -8, -23),
            S(  7,  31),   S( -7,  19),   S( -4,  27),   S(-20,  -2),   S(-31, -56),   S( 12,   9),   S(-16,  -8),   S(  0,  12),
            S( 16,   7),   S( -3,  -3),   S(-14, -41),   S( -6,  24),   S( -5,  21),   S( -1, -17),   S(-10, -21),   S(  2, -29),
            S(  5,  25),   S(-23, -22),   S( -4,  13),   S(  0,  31),   S( -1,  39),   S( 10,  10),   S(  1,  14),   S( 13,   2),

            /* rooks: bucket 14 */
            S(  0, -26),   S(-24, -30),   S(-20, -11),   S(-17, -60),   S(-35, -46),   S(  0, -17),   S(-37, -42),   S(-21, -25),
            S(  7,  55),   S(  0,  22),   S(  1,  10),   S( -5, -19),   S( -1, -10),   S( -3,   5),   S( -2,  -5),   S( -5, -18),
            S( 14,  41),   S( 14,  49),   S(-11, -11),   S( -6, -28),   S(  7,  12),   S( -2,  -8),   S(  7,  25),   S(-28, -61),
            S( -9,   1),   S( 10,  -7),   S(  5,  -6),   S(  8,   6),   S(  2,  12),   S(  2, -18),   S(  7,  -6),   S(-14, -35),
            S( 14,  50),   S( 21,  40),   S(  1,  12),   S( -9,  -1),   S(  7,   4),   S( 11,  17),   S(  4,  16),   S(  3,   6),
            S(  2,  13),   S(  4,  10),   S( 11,  35),   S( -2,  -1),   S(  0,  21),   S(-17,  11),   S( -1,  -5),   S(-16, -29),
            S(-13, -25),   S( -3,  -3),   S( -5, -34),   S(-24, -26),   S( -4,  17),   S( -2,  21),   S(-21, -11),   S(-12,  -5),
            S(  5,   2),   S( -6,  -4),   S(  2,   4),   S(  9,  -7),   S( -8,  -4),   S(-15, -64),   S( -4, -29),   S(  1,  24),

            /* rooks: bucket 15 */
            S( -6, -56),   S(-13, -51),   S(-47, -45),   S(-30, -45),   S(-11, -21),   S(-15, -32),   S(-14, -44),   S(-24, -52),
            S( 11,  20),   S(-13, -11),   S( -8, -14),   S(  6,   5),   S(-10, -15),   S(  7,  -1),   S(  6,  10),   S(  0,   1),
            S( -1,  18),   S(  3, -17),   S(  4, -12),   S( 21,  30),   S(  8,  -8),   S(  4,   4),   S( 16,  31),   S(  5,   9),
            S(  7,  10),   S(  4,  -1),   S( 25,  46),   S( -3,  -9),   S( 15,  39),   S(  0, -11),   S(  7,   1),   S(  4, -14),
            S( 12,  34),   S(  1,   3),   S(  2,  -9),   S(  0,   9),   S(  9,  30),   S( 11,  25),   S( -3,  28),   S(  1,   1),
            S(  9,  38),   S(  3,   3),   S( 13,  14),   S(  1,   4),   S(  3,  14),   S(-10,  35),   S(  4,  38),   S( 10,   4),
            S(  5,  -4),   S( -5,  10),   S(  9,  12),   S(  2,   6),   S( -4,  -9),   S( 18,  47),   S(-20,  -1),   S( -6, -26),
            S( 13,  26),   S(  6,  35),   S(  6,  30),   S(  3,  27),   S(  4,  38),   S( -1, -12),   S( -5,  33),   S(-19, -25),

            /* queens: bucket 0 */
            S( -4, -15),   S(-26, -76),   S(-26, -72),   S(  1, -78),   S(  1, -72),   S( 14, -72),   S(-60, -41),   S(-41,  -9),
            S(-13, -32),   S( 21, -103),  S( 15, -62),   S( -5, -35),   S(  3,  -3),   S( -4, -36),   S(-15, -44),   S(-25,  27),
            S(  4,  23),   S(  1,  -7),   S( 28, -55),   S( -6,  25),   S(-13,  30),   S(  7, -14),   S(-30,  -6),   S(-91, -31),
            S(-15,  22),   S( 25, -23),   S( -6,  34),   S( -1,  43),   S(  4,  81),   S(-13,  35),   S(-43,  19),   S(-15, -33),
            S(-20,   3),   S(  1,  85),   S(  9,  32),   S( -7,  52),   S(  5,  62),   S(-18,  95),   S(-53,  68),   S(-34,  -1),
            S(-30,  22),   S( 26,  15),   S( 24,  52),   S(-11,  73),   S(-38,  54),   S(-59,  87),   S(-74,  -7),   S(-50,  -5),
            S(  0,   0),   S(  0,   0),   S(-12,   0),   S(-53,  42),   S(-37,  39),   S(-79,  70),   S(-88,  50),   S(-111,   0),
            S(  0,   0),   S(  0,   0),   S(  6,   5),   S(-22, -12),   S(-27,  23),   S(-58,  22),   S(-60, -14),   S(-60,  -8),

            /* queens: bucket 1 */
            S( 10,   0),   S(  4,  10),   S( 21, -61),   S( 32, -85),   S( 35, -27),   S( 14, -30),   S( -4, -51),   S(  0,  31),
            S(-40,  39),   S( 30,  -4),   S( 42, -45),   S( 32,  14),   S( 47, -16),   S( -2,   7),   S(-14,  27),   S(-40, -21),
            S( 13,  40),   S( 22,  14),   S( 17,  38),   S( 19,  86),   S( -7,  68),   S( 36,  42),   S(-21,  74),   S(  6,  -9),
            S( 29,  22),   S( 30,  15),   S( 37,  13),   S( 38,  79),   S( 17,  79),   S(  1,  56),   S( 18,  43),   S( -6,  70),
            S( 42,   1),   S( 34,  66),   S( 47,  51),   S( 29,  44),   S( 57,  76),   S( 70,  31),   S( -3,  52),   S( 15,  76),
            S( 63, -28),   S( 92,  31),   S( 89,  34),   S(112, 102),   S( 79,  36),   S( 17,  69),   S( 35,  49),   S(  2,   9),
            S( 67, -22),   S( 53,  -3),   S(  0,   0),   S(  0,   0),   S(  7,  79),   S(  4,  34),   S(-13,  54),   S(-49,  33),
            S(127, -24),   S( 39,  -9),   S(  0,   0),   S(  0,   0),   S(  4,   3),   S( 29,  23),   S( 94,  -5),   S(-24,  44),

            /* queens: bucket 2 */
            S( 39, -12),   S( 22,  -7),   S( 31,  32),   S( 41, -11),   S( 33, -26),   S( 23, -12),   S(-31, -30),   S( 30,  17),
            S( -3,  36),   S(  6,  54),   S( 31,  21),   S( 43,  25),   S( 43,  26),   S( 11,  22),   S( 30,  -8),   S( 10,  54),
            S( 30,  60),   S( 16,  53),   S( 14, 109),   S( 21,  73),   S( 32,  64),   S( 16,  64),   S( 26,  49),   S( 17,  26),
            S( 18,  57),   S( 17, 108),   S( 16, 104),   S( 12, 117),   S( 43,  78),   S( 12,  96),   S( 24,  67),   S( 39,  63),
            S( -9,  46),   S( 14,  46),   S(  6, 127),   S( 19,  80),   S( 33,  91),   S( 94,  63),   S( 65,  61),   S( 75,  40),
            S(-37, 109),   S(-39,  85),   S( 13,  55),   S( 72,  35),   S( 74,  45),   S( 78,  93),   S(116,  31),   S( 30, 112),
            S(-13,  74),   S(-25,  75),   S(-25,  81),   S( 58,  57),   S(  0,   0),   S(  0,   0),   S( 13, 104),   S( 29,  81),
            S( 31,  11),   S( 58, -24),   S( 98, -31),   S( 56,  42),   S(  0,   0),   S(  0,   0),   S( 70,   9),   S( 36,  33),

            /* queens: bucket 3 */
            S(-61,  69),   S(-38,  41),   S(-26,  46),   S(-14,  53),   S(-32,  37),   S(-22, -28),   S(-20, -63),   S(-73,  22),
            S(-60,  51),   S(-45,  49),   S(-30,  58),   S(-23,  81),   S(-22,  78),   S(-25,  40),   S( 18, -13),   S( 23, -57),
            S(-57,  58),   S(-46,  89),   S(-45, 125),   S(-41, 141),   S(-31, 112),   S(-32,  92),   S(-19,  65),   S(-26,   4),
            S(-47,  52),   S(-71, 124),   S(-57, 154),   S(-43, 177),   S(-40, 164),   S(-27,  85),   S(-18,  81),   S(-17,  71),
            S(-56, 112),   S(-60, 155),   S(-59, 157),   S(-51, 175),   S(-38, 157),   S( -8, 142),   S(-21, 125),   S(-28,  63),
            S(-64,  87),   S(-80, 147),   S(-74, 167),   S(-65, 167),   S(-52, 163),   S( 11,  84),   S(-46, 157),   S(-30,  97),
            S(-93, 101),   S(-96, 144),   S(-96, 184),   S(-84, 155),   S(-100, 174),  S(-50, 108),   S(  0,   0),   S(  0,   0),
            S(-152, 169),  S(-85, 114),   S(-74, 111),   S(-67, 110),   S(-57, 100),   S(-18,  45),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-28,  -8),   S(-63, -28),   S( 17,   7),   S(-38, -16),   S(-20, -24),   S( -8,  30),   S(-35, -38),   S( 22,  25),
            S( -9,   5),   S( -6,  -5),   S( -8,  -8),   S(-41, -12),   S(-40,  36),   S(-38,   1),   S(-60, -25),   S(-15, -20),
            S( 18,  20),   S( 12, -33),   S( 19, -27),   S( 28,  -8),   S( 52,  23),   S(  5,  21),   S(  1,  -1),   S( 57,  32),
            S( -3,  -8),   S( 10, -19),   S( -1,  13),   S( 16,  53),   S( 51,  29),   S( 23,  61),   S(-24,   7),   S(-32,  19),
            S(  0,   0),   S(  0,   0),   S( 16, -14),   S( 93,  72),   S(  2,  35),   S( 22,  49),   S( -8,  21),   S( 11,  37),
            S(  0,   0),   S(  0,   0),   S( 30,  26),   S( 60,  56),   S( 49,  52),   S( 35,  40),   S( 30,  34),   S( -3,  -9),
            S(  1, -16),   S( 24,  10),   S( 43,  26),   S( 61,  23),   S( 74,   7),   S( 19,  16),   S( -8,  -7),   S(-28,  28),
            S( 52,  10),   S(-38, -35),   S( 35,   8),   S( 78,  57),   S( 10,  -5),   S( 12, -16),   S( 10,   0),   S(  3,   1),

            /* queens: bucket 5 */
            S( 36,   5),   S( 49,  38),   S(-12,   0),   S( -4,  22),   S( 56,   5),   S( 51,  55),   S( 57,  27),   S(  8, -15),
            S(  8,   2),   S( 22,  10),   S( 22, -23),   S( 24,   7),   S( 27,  44),   S(-42, -17),   S( 32,  27),   S( 14,  25),
            S( 33,  14),   S( 50,  -9),   S( 36,  14),   S(  1,  30),   S( 29,  22),   S( 50,  27),   S( 32,  40),   S( 32,  21),
            S(  9, -27),   S( 36,  -1),   S( 33,  -3),   S( 41,  39),   S( 81,   1),   S( 26,   1),   S( 28,  38),   S(-10,   7),
            S( 28, -18),   S( 14, -53),   S(  0,   0),   S(  0,   0),   S( 14,   5),   S( 26,  -7),   S( 60,  37),   S(-22,  40),
            S( 33,  21),   S( 48,  24),   S(  0,   0),   S(  0,   0),   S( 26,   9),   S( 74,  41),   S( 72,  40),   S( 54,  51),
            S( 54,   9),   S( 59,  19),   S( 58,  56),   S( 24,  27),   S( 57,  33),   S( 71,  27),   S( 66,  39),   S( 37,  22),
            S( 24,  17),   S( 80,  35),   S( 57,  -4),   S( 30,  16),   S( 62,  33),   S( 64,  39),   S( 61,  59),   S( 52,  31),

            /* queens: bucket 6 */
            S( 51,  67),   S(-12,  16),   S( 58,  26),   S( 47,  25),   S( 24,   7),   S(-50, -27),   S( 25,  29),   S( 34,  21),
            S(  0,  19),   S( 62,  48),   S( 44,  38),   S( 51,  40),   S( 47,  18),   S( 20,  27),   S(-20,  29),   S( -9,  13),
            S(-30,  51),   S( 48,  29),   S( 42,  58),   S( 59,  45),   S( 30,   6),   S( 72, -12),   S( 44,  -1),   S( 66,  53),
            S(  9,  25),   S( 13,  37),   S( 45,  36),   S( 87,   7),   S( 63, -13),   S( 34,  10),   S(105,  23),   S(119,  42),
            S( 12,  49),   S( 34,  46),   S( 52,  44),   S( 37,  36),   S(  0,   0),   S(  0,   0),   S( 88,  43),   S(112,  39),
            S( 36,  49),   S( 75,  66),   S( 47,  47),   S( 32,  20),   S(  0,   0),   S(  0,   0),   S( 96,  59),   S(123,  40),
            S( 54,  23),   S( 34,  42),   S( 66,  12),   S( 56,  32),   S( 41,  45),   S( 90,  66),   S(117,  34),   S(161,  -3),
            S( 42,  62),   S( 88,  35),   S( 82,  39),   S( 65,  21),   S(104,  22),   S(124,  32),   S( 88,   5),   S( 91,  50),

            /* queens: bucket 7 */
            S(  3,  19),   S(-16, -12),   S(-29,  32),   S( 14, -12),   S( 15,  19),   S( 13,   7),   S(-39,  14),   S(-26, -21),
            S(-26,  33),   S(-62,  28),   S( -8,  75),   S(-30, 103),   S( 13,  24),   S( 16,  22),   S( 19,  -6),   S(-23,   3),
            S(-17,   5),   S( -9,  34),   S(  5,  70),   S( 72,  39),   S( 68,  35),   S( 48,   0),   S( 42, -34),   S( 42,  36),
            S(-36,  62),   S(  5,  52),   S( 18,  86),   S( 30,  82),   S(109,  68),   S( 57,  15),   S( 67, -18),   S( 33,  11),
            S( 16,   9),   S(-27,  78),   S( 38,  91),   S( 62,  80),   S(125, -18),   S( 45,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  61),   S(-12, 104),   S( 28, 116),   S( 14,  91),   S( 73,  56),   S(107,  83),   S(  0,   0),   S(  0,   0),
            S(-42,  86),   S(-32,  42),   S( -5,  63),   S( 42,  51),   S( 72,  31),   S(122,  12),   S( 47,  10),   S( 85,  70),
            S( 15,  24),   S( 43,  21),   S( 58,  53),   S( 30,  34),   S( 88,  33),   S( 55,  12),   S(-18,   2),   S( 86,  -1),

            /* queens: bucket 8 */
            S(-29, -40),   S( 10,  -5),   S(-25, -67),   S( 21,  22),   S( -9,  -7),   S(  5, -11),   S(  4,  -8),   S( -1,   3),
            S(-19, -31),   S(  2,  -9),   S( 13,   2),   S( -6, -14),   S(  8,   2),   S(  4,   0),   S( -4,  -5),   S( -6,  -3),
            S(  0,   0),   S(  0,   0),   S(-12, -36),   S(-34, -72),   S(-14, -22),   S( 11,   5),   S( -1, -11),   S(  7,  16),
            S(  0,   0),   S(  0,   0),   S(  2,  -1),   S( 23,   8),   S( -2,   4),   S(  9,   0),   S(  8,  18),   S(  4,  -5),
            S( -7,  -9),   S(  7,   6),   S( 17,  22),   S( 38,  25),   S(  5,   3),   S( 13,  29),   S( 24,  13),   S(-18, -15),
            S(-12, -30),   S( 15, -26),   S( 16,  24),   S( -2, -22),   S(  5,  -3),   S( 18,  24),   S(  4, -21),   S(  0,  -4),
            S(-25, -25),   S( 10, -18),   S( 23,  22),   S( 30,  40),   S( 15,  32),   S(  3,  34),   S( 15,  13),   S( 13,  11),
            S( 18,  20),   S(  4, -11),   S( 14,  13),   S( -2, -20),   S( 10,   6),   S( -8, -11),   S( -5,   9),   S(-19, -39),

            /* queens: bucket 9 */
            S(  6,  -8),   S( -6, -29),   S( -5, -37),   S( 12,   0),   S( -2, -22),   S(-13, -24),   S(-16, -24),   S( -6, -18),
            S( -4, -26),   S( -7, -22),   S( -3, -13),   S( 23,  19),   S(-17, -41),   S( -4, -28),   S( -9,  -9),   S(  4, -14),
            S(-13, -63),   S(-12, -28),   S(  0,   0),   S(  0,   0),   S( -2, -17),   S( 10,  -8),   S( -3, -12),   S( 11,   0),
            S(  0, -18),   S(-22, -47),   S(  0,   0),   S(  0,   0),   S(-10, -11),   S( 24,  23),   S(  8,   5),   S( -6,  -4),
            S(  1,  -4),   S(  6, -13),   S( 11,  11),   S(-21, -25),   S(  4, -23),   S(  9,  21),   S( 20,  13),   S( -8, -24),
            S( 20,  12),   S(-11, -27),   S( 20,  12),   S( -3, -26),   S( -2, -15),   S(  5,  26),   S(  7,  -4),   S(  3,   3),
            S(  0,  -6),   S( 17, -11),   S( -3,  12),   S( -4,  12),   S( 31,  29),   S( 37,  44),   S(  5,  12),   S( 16,  -1),
            S(  7, -25),   S( 23,   6),   S(-14, -19),   S(  8,   3),   S( 33,  38),   S( -2,   5),   S( 23,  10),   S(  7,   7),

            /* queens: bucket 10 */
            S(  8,   1),   S( 13,   5),   S( 11,   4),   S( -9, -35),   S(-16, -43),   S(  2,  -5),   S( -9, -29),   S( -5, -22),
            S( 14,   3),   S(-14, -14),   S(-17, -53),   S( -9, -40),   S(-11, -16),   S( 13,   0),   S( -3, -25),   S(-14, -17),
            S(  1,  -1),   S(  8,  18),   S(-12, -18),   S(-18, -22),   S(  0,   0),   S(  0,   0),   S(  5,  -2),   S( -8, -11),
            S(-10, -25),   S( -2, -14),   S( 20,  20),   S( -1,  -9),   S(  0,   0),   S(  0,   0),   S(  2, -13),   S(  2, -25),
            S(  9,  24),   S( 11,  11),   S(  8,   1),   S( 40,  49),   S( 18,  27),   S(  1,  -4),   S( -1, -15),   S( 21, -14),
            S(  0,  -3),   S( 23,  12),   S( 30,  40),   S(  0,  -1),   S( 20,   1),   S( 14,  26),   S( 16,   9),   S(  4, -12),
            S( 10,  -8),   S( 21,  21),   S( 33,  43),   S( 22,  15),   S(  6,  13),   S( 35,  18),   S(  9,   5),   S(  2, -11),
            S(-27, -46),   S( -5,  -7),   S(  2, -20),   S( 15,  30),   S( 29,  34),   S( -8, -14),   S(  4,  -5),   S(  1, -21),

            /* queens: bucket 11 */
            S( -8,  -4),   S(  2,  20),   S(-15, -21),   S(-14,  -4),   S(-17, -32),   S(-16, -27),   S(  9, -28),   S(-11, -15),
            S( -1,  -4),   S(-18, -11),   S(-14, -13),   S(-23, -16),   S( 25,  -4),   S( -6, -10),   S( 10,   9),   S( -4, -14),
            S( -4,   1),   S(  7,   2),   S( -1,  41),   S(-23,  -6),   S(-10, -25),   S(-18, -26),   S(  0,   0),   S(  0,   0),
            S( -9,  10),   S(-29,  -3),   S( -3,   0),   S(  2,   2),   S(-11, -40),   S( -8,  -2),   S(  0,   0),   S(  0,   0),
            S( -3,   3),   S( 12,   9),   S( 27,  38),   S(  3,  29),   S( 45,  65),   S( 35,  36),   S( 16,   9),   S(-10, -25),
            S( -6,   8),   S( -3,  -5),   S(-23,  -2),   S(  9,  22),   S( 17,  29),   S(  8,  15),   S( -3,  -3),   S( 23,  -3),
            S(  3,  10),   S( 12,   8),   S( 24,  38),   S( 26,  29),   S( 26,  69),   S( 26,  24),   S( 14,  22),   S(  5,   9),
            S(-33, -64),   S(  5,   7),   S( -8, -31),   S( 16,  54),   S( 18,  44),   S( -8,  -6),   S(-13,   0),   S( 23,  17),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  4,   5),   S(-26, -34),   S(-12, -16),   S( -7, -14),   S(  1,  -1),   S(  3,   6),
            S(  0,   0),   S(  0,   0),   S(  7,   3),   S( -6,  -4),   S( -6, -11),   S(-16, -35),   S(-13, -27),   S(  6,   7),
            S( -9, -14),   S( -1,  -5),   S(  0,   3),   S(-12, -40),   S( 21,  36),   S( -4,  17),   S(  1,   7),   S(  5,   9),
            S(  5,  -3),   S( 15,  11),   S(  0,  13),   S(  5,  21),   S(  1,   0),   S(  0,  22),   S(  4,  15),   S( -6, -10),
            S(-14, -32),   S(  5,  12),   S( 27,  42),   S(  8,   8),   S( 10,  37),   S(-12, -36),   S( -7, -13),   S( -2,  -2),
            S(  2,   1),   S( -2,  -8),   S( -3, -19),   S( 10,   9),   S( -4,  -9),   S(  0,   8),   S( -9,  -8),   S( -7, -17),
            S( -7,  -9),   S(  1,   5),   S( -6,  -6),   S( 10,  -1),   S( -5,  -6),   S(-15, -16),   S(  6,  12),   S( -9, -34),
            S(  8,  17),   S( -9, -19),   S(  4,  -1),   S(  9,  18),   S(-11,  -4),   S( -3,  -5),   S(-13,   7),   S(-10, -14),

            /* queens: bucket 13 */
            S(-14, -37),   S( -8, -20),   S(  0,   0),   S(  0,   0),   S(-18, -27),   S(-17, -39),   S(-12, -20),   S( -1,  -7),
            S(-12, -44),   S( -6, -17),   S(  0,   0),   S(  0,   0),   S(-13, -31),   S(-18, -22),   S(-11, -14),   S( -6,  -8),
            S(-22, -39),   S( -6,  -4),   S( -5, -10),   S(  2,   0),   S(-31, -50),   S(-15, -27),   S(-13,  -9),   S(  4,   3),
            S(-17, -39),   S(-25, -33),   S(  5,   1),   S( -6, -24),   S( 29,  30),   S( 17,  29),   S( -3, -13),   S( -5,  -3),
            S( 10,  -6),   S( -3, -16),   S(-10, -21),   S( 20,  34),   S(  0,   0),   S( 10,   7),   S(-11, -19),   S(  3,  -6),
            S(  1,  -6),   S(-16, -24),   S(  2,  -1),   S( 19,  39),   S(  7,  -3),   S( -5,   6),   S(-11, -23),   S(-15, -28),
            S( -1,  -2),   S(  1, -11),   S( 22,  35),   S(  0,   6),   S(  2,  -6),   S( -1, -13),   S(-11, -24),   S(-13, -14),
            S( -9,  -2),   S(-10, -18),   S( -7, -25),   S(  2,  -4),   S(  0,  -2),   S(  0,  -8),   S(  3,   9),   S( -9,  -5),

            /* queens: bucket 14 */
            S( -5, -25),   S( 13,   5),   S(-19, -29),   S( -2, -17),   S(  0,   0),   S(  0,   0),   S( -6, -12),   S( -6, -24),
            S(-12, -31),   S(-17, -30),   S( -4,  -2),   S( -3,  -7),   S(  0,   0),   S(  0,   0),   S( -2, -10),   S( -3, -16),
            S( -5, -17),   S( -5, -28),   S( -1, -15),   S( -7, -29),   S(  3,   6),   S(  0,   9),   S(-13, -27),   S(-21, -40),
            S( -8, -13),   S( -1,   0),   S(  9,   4),   S(-14, -23),   S( -1, -10),   S(-26, -54),   S( -2, -18),   S(  3,  -4),
            S( -9, -14),   S(  0,   0),   S(-10, -25),   S(  5,  16),   S( 14,  30),   S( -7, -22),   S(-18,  -9),   S( 11,  22),
            S( -8, -12),   S(-14, -24),   S(-29, -36),   S(-11, -15),   S( -2,   6),   S(-10, -13),   S(  2,   2),   S(-18, -21),
            S( -6,  -9),   S(  2,   2),   S( -5, -13),   S( -3,  -9),   S(  4,   2),   S(  5,  14),   S(-15, -27),   S( -7,  -7),
            S(-17, -22),   S( 15,  13),   S(-10,  -9),   S(  3,   1),   S(  1,   0),   S( -1,   2),   S( -5,  -6),   S( -1, -13),

            /* queens: bucket 15 */
            S( -4,  -3),   S(-11, -23),   S( 13,  11),   S(-11, -21),   S( -3,  -9),   S( -6, -10),   S(  0,   0),   S(  0,   0),
            S(  1,   8),   S( -8,  -8),   S( -8,  -4),   S( -6,  -3),   S(  6,  17),   S(  3,   7),   S(  0,   0),   S(  0,   0),
            S( -5,  -7),   S(  5,   0),   S( -7,   6),   S( -8, -11),   S(-22, -46),   S(  4,  12),   S( -4,  -5),   S( -4,  -8),
            S( -5,  -9),   S(-12, -19),   S( -8, -14),   S(  1,  14),   S( 18,  55),   S( 12,  33),   S(  0,   4),   S( -2, -11),
            S( -7, -12),   S( -6, -13),   S(-10,   0),   S(  5,  11),   S(  0,  35),   S(  2,   9),   S(  2,  21),   S( -4, -11),
            S( -1,  -9),   S( -7, -12),   S( -1,  -7),   S( -2,   6),   S(  5,  14),   S(  0,   9),   S( -2,  11),   S( -3,   6),
            S( -8, -20),   S( -1,  -1),   S( -5, -10),   S(  6,   4),   S( -3,  -4),   S( -7,   1),   S(  9,  23),   S( -7, -10),
            S( -9, -26),   S(-20, -44),   S(  2,   4),   S(  3,   7),   S(-26, -18),   S( -2,   4),   S(  4,   1),   S(  1,  16),

            /* kings: bucket 0 */
            S(-14, -11),   S( 11, -13),   S( 15,   3),   S(-21,  17),   S(-24,  15),   S( 15, -16),   S(  0,   7),   S( 10, -50),
            S(  4,  26),   S(  0,  -2),   S( 12,  -6),   S(-45,  30),   S(-35,  41),   S(-10,  19),   S(-28,  44),   S(  9,  23),
            S( 33,  -1),   S( 67, -28),   S( -4,  -3),   S( -4,  -2),   S( 12, -22),   S( -8,  12),   S(-35,  22),   S( 40, -36),
            S(-31, -20),   S(-14, -19),   S(-15,  -4),   S(  8, -11),   S(-51,  34),   S(-53,  30),   S(-36,  28),   S(-16,  30),
            S(-52, -101),  S( -2, -44),   S(-17, -30),   S(  2, -13),   S(-43, -13),   S(-61,  18),   S(-17,  27),   S( 12,  -8),
            S( -8, -137),  S( 12,  11),   S( -9, -54),   S(-31,  -9),   S(  0,  -9),   S(-46,  27),   S( 11,  11),   S(-19,   1),
            S(  0,   0),   S(  0,   0),   S( -2, -49),   S( 11,  -4),   S(-16, -13),   S( -7, -18),   S(-22,  12),   S(-14,   7),
            S(  0,   0),   S(  0,   0),   S(-11, -20),   S(  1, -13),   S(  4, -14),   S(-12,  19),   S( -1, -24),   S( 13,  19),

            /* kings: bucket 1 */
            S(  2, -40),   S( 32, -33),   S(  9,  -3),   S( 13,   4),   S( -8,  -3),   S( 22, -16),   S(  1,   4),   S( 11, -22),
            S(-10,  15),   S( 10,  14),   S( 12, -13),   S(-36,  13),   S(-44,  26),   S(-12,  20),   S( -7,  19),   S(  5,  14),
            S(  6, -22),   S(  2,  -6),   S(  6, -10),   S( 13, -21),   S(-45,   8),   S( 40, -33),   S( 26, -17),   S( 57, -18),
            S(-10,   6),   S(-14, -19),   S(  9,  -3),   S(  2,  -3),   S( -4,  18),   S( -6,   0),   S( 17,  11),   S(-27,  30),
            S(  2, -64),   S(-15, -41),   S(-28, -46),   S(-12, -40),   S(  2, -17),   S(  1, -31),   S(-12,   5),   S( -9,   1),
            S( -5,  19),   S(-112,  -9),  S(-38,  29),   S(  5,   5),   S(-50,  -5),   S(-18,  11),   S( 26,  -5),   S(  6, -12),
            S(-45, -51),   S(-24,   0),   S(  0,   0),   S(  0,   0),   S(-38,  20),   S(-60,   8),   S( 14,  50),   S(-11, -45),
            S(-26, -85),   S(-13, -14),   S(  0,   0),   S(  0,   0),   S( -9,   4),   S( -4,  48),   S(  4,  24),   S(-12, -66),

            /* kings: bucket 2 */
            S( 16, -42),   S( 12, -12),   S( 24, -17),   S( 13, -14),   S( -8,  13),   S( 36, -23),   S( -2,  20),   S( 21, -27),
            S( 40, -34),   S( -2,  16),   S(  8,   1),   S(-11,   4),   S(-20,  10),   S(-11,   4),   S(  9,  -2),   S( 17, -11),
            S(-68,   4),   S( 19, -19),   S(  2, -17),   S( -4, -20),   S( -3,  -9),   S( 15, -18),   S( 45, -23),   S( 41, -23),
            S( 27,   1),   S(-25,   8),   S( -4,  11),   S(-22,  11),   S( 28,  -3),   S(-10, -14),   S( 10,  -9),   S( 46, -20),
            S( -1, -20),   S( 21,  -6),   S( 19, -45),   S(-29, -15),   S( 20, -50),   S(-14, -48),   S( 23, -48),   S( 10, -40),
            S( -5,  -8),   S( 10, -19),   S(-23,  -9),   S(-34, -11),   S( 12,  -1),   S(-26,  34),   S(-88,  17),   S(-44, -17),
            S(-13, -21),   S(-14,  32),   S(-66,  -1),   S(-19,   8),   S(  0,   0),   S(  0,   0),   S(-18,   0),   S(-39, -47),
            S(-15, -28),   S(-22, -27),   S(-43, -38),   S(  3,  19),   S(  0,   0),   S(  0,   0),   S( -4,  10),   S(-43, -100),

            /* kings: bucket 3 */
            S(  2, -52),   S(  6,  -2),   S( 28, -30),   S(  2, -10),   S( -9,  -7),   S( 32, -22),   S(  0,  16),   S(  5, -27),
            S( 23,   3),   S(-18,  33),   S(-13,   7),   S(-36,  17),   S(-47,  29),   S(  7,  -2),   S( -5,  15),   S(  9,  12),
            S( 30, -25),   S( 18,  -8),   S(-11,  -2),   S(-45,   4),   S(-28,  19),   S( 18, -17),   S( 48, -17),   S( 48, -14),
            S(-49,  29),   S(-75,  42),   S(-24,   4),   S(-66,  19),   S(-23,  10),   S( -8, -27),   S(-15,  -4),   S(-19, -20),
            S(-22,  21),   S(-12, -23),   S(-18, -20),   S(-19, -13),   S( 23, -41),   S( 41, -64),   S( 45, -70),   S( -7, -68),
            S(-19,   9),   S( 36,  -2),   S( 51, -20),   S(  1, -20),   S( 52, -24),   S( 52, -62),   S( 79,  -7),   S( 34, -112),
            S(-17, -16),   S( 35,  17),   S( -2,  -6),   S( 40, -18),   S( 17, -30),   S( 29, -36),   S(  0,   0),   S(  0,   0),
            S(  7, -15),   S( -6,  -8),   S( -6,   1),   S( 19,  27),   S(  3, -81),   S(  6,  16),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-55,  15),   S( 30,  11),   S( -7,  36),   S( 34,  -2),   S(  7,  13),   S(-12,  14),   S( -2,  15),   S( 13, -31),
            S(-51,  33),   S( 16,  11),   S(-28,  18),   S( 18, -10),   S( 42, -22),   S( 12,   2),   S( 38,  -7),   S( 25, -16),
            S(  2,  21),   S(  4,  -5),   S( 11,   2),   S(  1,  -8),   S( -3,  -2),   S( 27, -27),   S(-26,  -2),   S( 46, -36),
            S(  9, -19),   S(-15,   3),   S(  3,  18),   S( -4,  11),   S(-30,  14),   S( -4,  25),   S( 20,   8),   S(  3,  14),
            S(  0,   0),   S(  0,   0),   S( -5,  11),   S(-12,   9),   S(-42,  14),   S(-46, -18),   S(-24,  21),   S(-13,  -3),
            S(  0,   0),   S(  0,   0),   S(  3,   2),   S(  0,  13),   S( -2,  32),   S(-31,   9),   S( -7, -22),   S( -3,  23),
            S(  0,   0),   S( -9, -36),   S( -5,  -5),   S( -8,  17),   S( 14,  36),   S(-19,  -4),   S(-20,  11),   S(  0, -13),
            S( -3, -24),   S(  7,   3),   S(-13, -28),   S(-10, -14),   S( 12,  -3),   S(-13, -18),   S(-12,   1),   S(  7,  22),

            /* kings: bucket 5 */
            S( 12,  14),   S(-17,   7),   S(-36,  20),   S(-38,  28),   S(-23,  28),   S( -6,   9),   S( 13,   4),   S( 36,  -9),
            S( -1,  -6),   S( 21,   5),   S( 20,  -1),   S( 18,  -3),   S(  7,   1),   S( 46, -17),   S( 29,   2),   S( 74, -28),
            S(-12,   7),   S( -8,   1),   S(  5,  -5),   S( -8,  -8),   S( 14,  -2),   S(-27,  -1),   S( -9,  12),   S( 36,  -6),
            S(  6, -19),   S(  3, -11),   S( 16,  -8),   S(-13,  20),   S( -9,  24),   S( -3,   2),   S( 11,   7),   S( 22,  -3),
            S(  7, -44),   S(-34, -35),   S(  0,   0),   S(  0,   0),   S( 22,  -9),   S(-18, -18),   S( 11, -18),   S(-10,  -3),
            S(-21, -29),   S(-26, -13),   S(  0,   0),   S(  0,   0),   S(-14,  30),   S(-67,  18),   S( -2,  -4),   S(  3,  -2),
            S( -9, -15),   S(-23,  28),   S(  8,  19),   S( -3, -15),   S(-26,  29),   S(-50,   7),   S( -9,  21),   S(  8,  26),
            S( -5, -103),  S(-21,   7),   S(-13, -30),   S( -8, -41),   S( -5,   0),   S( -7,  19),   S( -4, -24),   S( -2,   2),

            /* kings: bucket 6 */
            S( 39, -44),   S( 20,  -3),   S( -3,   0),   S(-34,  26),   S(  2,  10),   S(-42,  27),   S(  4,  15),   S( 13,  -2),
            S( 44, -31),   S( -3,  21),   S( 18,  -1),   S( 28, -14),   S( 18,   1),   S( -9,  10),   S( 11,   3),   S(  5,   3),
            S( 24, -15),   S(-51,   9),   S( -6, -13),   S(-13,  -4),   S( 11, -13),   S(-46,   7),   S( -1,   1),   S(-11,  12),
            S( 17,  -1),   S( 23,  -3),   S( 36, -16),   S( 10,   7),   S( 71,  -5),   S(-34,   8),   S( -8,  16),   S(-17,   7),
            S( 27, -24),   S( 16, -24),   S( -5, -17),   S( 10, -19),   S(  0,   0),   S(  0,   0),   S(-42,  -6),   S(-25, -29),
            S(  7, -10),   S(  1,   1),   S(-49,  11),   S(-20, -32),   S(  0,   0),   S(  0,   0),   S(-32, -27),   S(-32, -19),
            S(  0,  -6),   S(-10,  14),   S(-42,  -1),   S(-25,  -4),   S( -5, -16),   S( -3, -36),   S(-32, -23),   S(  8, -47),
            S(  1, -12),   S(  8,  -6),   S( -7,  28),   S(-11, -30),   S(  2, -21),   S(-15, -56),   S(-13,   9),   S( -1, -49),

            /* kings: bucket 7 */
            S( 56, -36),   S(-32,   2),   S(  0, -23),   S(-32,  17),   S(-33,   9),   S(-53,  39),   S(-41,  35),   S(-43,  21),
            S( 17,   6),   S(  8, -19),   S(-11,  -5),   S(-33,   7),   S(-23,   8),   S(-35,  21),   S( -1,  -2),   S( -3,   9),
            S( 19, -22),   S(-40,   0),   S(-32,  -1),   S(-30,  -1),   S(-55,   6),   S(-35,  16),   S( 13,  -1),   S(-58,  25),
            S(-30,  42),   S(  9,  -1),   S(-33,   5),   S( 36, -13),   S( 68, -26),   S( 57, -29),   S( 11,  -5),   S( 38, -23),
            S(-32,  17),   S(-12,   5),   S( 18, -29),   S( 11, -28),   S( 10, -35),   S( 17,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -53),   S(-12,   6),   S(  9,  -8),   S( 17, -11),   S( 19, -11),   S( 33, -18),   S(  0,   0),   S(  0,   0),
            S( 22,  21),   S( 24, -19),   S( 10,  29),   S(-29, -20),   S( 17, -11),   S( -7, -39),   S(  3, -37),   S(-12,  28),
            S(  2,   8),   S( -7, -22),   S( 30,  45),   S( -9, -30),   S(  8,   0),   S(-25, -48),   S( 12, -12),   S(-15, -60),

            /* kings: bucket 8 */
            S(  5, 124),   S( -7,  72),   S( 18,  47),   S(  5,  -7),   S(-14,  12),   S(  2,  11),   S( 22, -17),   S(-22,  -7),
            S( 25,  79),   S( 28,  -2),   S( 34,  67),   S( 76,  -9),   S(  4,  26),   S(-19,   3),   S( 28,   9),   S(-13,  13),
            S(  0,   0),   S(  0,   0),   S( 32,  49),   S( 28,  11),   S(  8,   8),   S(-27,   5),   S( 18,   4),   S( 27, -21),
            S(  0,   0),   S(  0,   0),   S( -2,  54),   S( 15,  10),   S(-26,  23),   S( -6,  22),   S( 10,   9),   S(  9,  34),
            S( -3, -26),   S(  0,  11),   S(  9,   9),   S( 11,  29),   S(-18, -12),   S( 16,  -8),   S( -1,  15),   S(-21, -17),
            S(  6,  22),   S( -3,  -7),   S(-18, -23),   S(-16,  17),   S(-15,   0),   S( -8, -19),   S(-17,   4),   S( 17, -37),
            S( -5, -10),   S(-15, -27),   S(  2, -15),   S(  0,   4),   S( -2, -21),   S( -1,  25),   S(  0, -15),   S(  5, -60),
            S( -4,  -1),   S(-10, -10),   S( -7,  -5),   S( -4, -17),   S(  8,   7),   S( -7, -12),   S(  8,  18),   S(  3, -16),

            /* kings: bucket 9 */
            S( 20,  16),   S( 21,  27),   S(-16,  51),   S( 16,   9),   S(-17,  27),   S(  0,  31),   S( 38,   0),   S( 20,  15),
            S(-32,  29),   S( 45,  33),   S(  4,   3),   S( 68,   6),   S( 67,  19),   S( 15,   7),   S(-19,  29),   S(-51,  27),
            S( -1,  22),   S(  9,  15),   S(  0,   0),   S(  0,   0),   S( 39,  25),   S(  8,   1),   S(  7,   6),   S(-27,  23),
            S(  5, -35),   S(  2, -38),   S(  0,   0),   S(  0,   0),   S( -5,  40),   S( 20,  -6),   S( -9,  20),   S(-26,  30),
            S(  2,   1),   S(  6,  -6),   S(  7,  17),   S(  0,  26),   S(-20,  13),   S(-11,   3),   S(-12,   8),   S( 11, -33),
            S(  4,  -8),   S(  1, -19),   S(  6, -35),   S(-13,  -8),   S( -5,  27),   S(-23,   9),   S(-18, -14),   S( -5,  46),
            S(-11, -23),   S( -9,  -2),   S(  1, -22),   S(  1, -15),   S( 15, -23),   S(  3,  22),   S(-23,   2),   S(  0, -12),
            S(  8,   1),   S(  0, -23),   S( 16, -31),   S(  2, -15),   S(-15, -46),   S(  3,   5),   S( -4,  17),   S(  8,  -3),

            /* kings: bucket 10 */
            S( 32, -27),   S( 12,  26),   S( 20,   6),   S( -2,  26),   S(  3,  17),   S(-103,  47),  S(-18,  43),   S(-71,  71),
            S( 17,  -5),   S( 89, -17),   S( 25,  -2),   S( 22,  14),   S( 66,  20),   S( 44,   5),   S( 13,  41),   S(-92,  42),
            S(  3,  -6),   S( 37,  -2),   S( 23,  -6),   S(  2,  13),   S(  0,   0),   S(  0,   0),   S(-19,  21),   S(-75,  34),
            S( 25, -15),   S( 56, -15),   S( 23, -21),   S( 31,  -4),   S(  0,   0),   S(  0,   0),   S( 15,  15),   S(  6,  -5),
            S( -1,  13),   S( 31,   3),   S( 30, -13),   S( 17, -47),   S( -5, -23),   S( 15,  30),   S(  7,   4),   S(-22,   1),
            S(  0,  19),   S( -6,   3),   S( -2,  -3),   S( 12,  11),   S( -4, -11),   S( -6,   3),   S( -9,  12),   S( -3, -10),
            S( -6, -85),   S(-11, -17),   S(  3, -36),   S( 20,  12),   S( 21,  -7),   S(-18, -20),   S( 17, -11),   S(  3,   7),
            S(  9,  16),   S(  9,   7),   S(  9,  26),   S(  4, -13),   S(  9,   9),   S( -7, -36),   S(-10, -32),   S(  1, -27),

            /* kings: bucket 11 */
            S(-10, -32),   S( 20,  19),   S(  0,   4),   S(-12,  12),   S( -2,  15),   S(-69,  42),   S(-80,  78),   S(-90, 132),
            S(  1, -38),   S(  1,  19),   S(-20, -20),   S( 19,  16),   S(110,  -2),   S( 56,  37),   S( 23,  10),   S( -1,  43),
            S(  9, -52),   S(  6,  16),   S(-11,   1),   S(  6,  20),   S( 63,   7),   S( 25,  47),   S(  0,   0),   S(  0,   0),
            S(  5,  15),   S( 27,   2),   S(  3,  -2),   S(  1,   9),   S( 23,  -3),   S( 19,  31),   S(  0,   0),   S(  0,   0),
            S(  4,  29),   S( 16,   2),   S( -3,   7),   S( 24, -39),   S( 19,  -3),   S( 14,  -5),   S(  7,   9),   S(  8,  -2),
            S( 16,  12),   S(-14,  -8),   S(  8,  20),   S( -4,   6),   S( -9,   4),   S(  3,  -2),   S( -8,  -5),   S( -7,  -2),
            S(  4,   8),   S(  4,  -4),   S( 26,  38),   S(  2, -23),   S(  2, -37),   S(  7,  -2),   S(-11, -24),   S(-11,  -1),
            S( -2,   1),   S(  3,  -3),   S(-19,  -9),   S(  9,   3),   S( -4, -17),   S(-10, -22),   S( -1, -29),   S(  6,   4),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 18,  31),   S( 10, -11),   S(  2, -11),   S( 18,  18),   S( -9, -21),   S( -3,  26),
            S(  0,   0),   S(  0,   0),   S( 56, 109),   S( 23,  19),   S( 22,  24),   S( 22,   9),   S( 18, -11),   S(-26,  27),
            S( -2,  -1),   S( -2,  10),   S( 14,  45),   S( 44,  32),   S(  8, -12),   S( 10,  16),   S(-11,  -2),   S(-22,  14),
            S( -2,   4),   S( 13,  46),   S(  0, -13),   S( -1,  22),   S(-12, -27),   S( -5,  31),   S( -9,   2),   S(  0,   0),
            S( 13,  21),   S(  1,   4),   S(  8,   9),   S( -5,  28),   S(  2,  44),   S( -2, -18),   S( -1,   8),   S( -4,   3),
            S(  7,   7),   S( 13,  16),   S(  5,  10),   S(  4,   0),   S( -3,  11),   S(  3,  45),   S( -7,  14),   S(  2, -26),
            S(  4,   9),   S( -8, -24),   S(  0,  15),   S( -2,  -5),   S( -4,   2),   S(  3,  13),   S(  0,  25),   S( -1, -29),
            S(  1,   2),   S(  3,  14),   S( -7,   5),   S( -4,   8),   S(  1,  -2),   S(  6,   7),   S( -3,   0),   S( -7, -25),

            /* kings: bucket 13 */
            S(  6,  51),   S( -1,   0),   S(  0,   0),   S(  0,   0),   S( 38,  36),   S( 21, -26),   S( -4, -29),   S(-19,  22),
            S( -7,  23),   S(  3,  12),   S(  0,   0),   S(  0,   0),   S( 44,   6),   S( 25,  -2),   S(-10,  -5),   S(-18,  14),
            S(  6,  25),   S( 23,  37),   S( -5,  -7),   S( 19,  29),   S( 52,  -2),   S( 26, -19),   S(  5, -16),   S(  2,  -4),
            S( -7,  -4),   S( 15, -29),   S( 15,  47),   S(-14,  -4),   S(  3,  49),   S(  4, -21),   S(  4,  14),   S(-14, -19),
            S(  6,   6),   S( -8, -16),   S(  2,  55),   S(-14,  17),   S( -2,  31),   S( -6,   9),   S( -3, -27),   S(  2,  17),
            S(  4,  -5),   S(  6,  48),   S( -7,   7),   S( -1,   4),   S(-18, -38),   S(-10,  10),   S(-26,  -4),   S(  8,   4),
            S( 15,  18),   S(-12, -33),   S( -7, -36),   S(  1,  21),   S(-18,  -5),   S( -7,  38),   S(-12,   1),   S(  8,  44),
            S(  1,   4),   S(  7, -11),   S(  2,   9),   S(  4,   6),   S(  2,  23),   S( -7, -14),   S( -1,  -5),   S(  8,  22),

            /* kings: bucket 14 */
            S( 25,  47),   S(  1, -11),   S( -2, -30),   S( 25,  -4),   S(  0,   0),   S(  0,   0),   S( 22,  78),   S(-49,  14),
            S(-14, -11),   S( 19,   1),   S( 49, -44),   S( 40,  -2),   S(  0,   0),   S(  0,   0),   S(  3,  32),   S(-29,  11),
            S(-10,  -1),   S(  8, -33),   S( 39, -49),   S( 37,  -8),   S( 16,  27),   S( -1,  17),   S( 37,  70),   S(-33,  22),
            S( -1, -23),   S( 10,  45),   S( -7, -21),   S( 19,  25),   S(-12,  28),   S( 16,  56),   S(  4,   1),   S(  0,   3),
            S( -4, -14),   S( 19,  35),   S(  0,  -3),   S( -5,  26),   S(  1,  32),   S(  9,  59),   S( -5,  18),   S( 18,  22),
            S( -4,   1),   S( -8, -22),   S( -6,  22),   S(  5,  -3),   S( -4, -12),   S(-10, -27),   S(-14,   0),   S(  6,  38),
            S(  5,  17),   S(-11, -39),   S( 12,   1),   S(  7, -20),   S( -3,  -8),   S( -9,  11),   S(-25,  -8),   S( 11,  13),
            S( -4,  -5),   S(  4,   8),   S( 13,  -1),   S( -2, -23),   S( 10,  -2),   S(  2,  -2),   S(-16, -37),   S( -2, -28),

            /* kings: bucket 15 */
            S(  6,  11),   S( 24,  14),   S(  7, -14),   S( -8,  -5),   S(  8, -18),   S(  5,  55),   S(  0,   0),   S(  0,   0),
            S( -9, -22),   S(  3, -17),   S( -7, -22),   S( 13,  70),   S( 33,  -2),   S( 59, 104),   S(  0,   0),   S(  0,   0),
            S( -6, -37),   S(  6, -22),   S( 16,  -8),   S(  4,  15),   S( 10, -15),   S( 24,  93),   S(  9,  41),   S(-18,  -6),
            S( 14,  18),   S( 11,  21),   S( -1,   3),   S(-13, -37),   S(-28, -27),   S( 34,  56),   S( 21,  48),   S( -1,  -2),
            S(  2,  -1),   S(-14,  46),   S( -6,  -1),   S( -4, -30),   S( -3,  17),   S( -3,  55),   S(  2,  11),   S(  2,  -9),
            S(  8,  28),   S(-13,   2),   S( 13,   1),   S( 15,  27),   S(-13, -35),   S(  8,  25),   S(  6,  24),   S(  6,  24),
            S( 11,  25),   S(  4,  35),   S( -1,  -9),   S(  3, -19),   S( 10,   3),   S(  4,  31),   S( -7,  -5),   S(  1,  -6),
            S( -6, -15),   S(  3, -17),   S( -5, -33),   S(  1,  -2),   S(  5,   8),   S( 12,  19),   S(  1,   4),   S(  5,   7),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-20, -145),  S(  3,  14),   S( 14,  82),   S( 22, 120),   S( 28, 142),   S( 32, 164),   S( 38, 161),   S( 47, 150),
            S( 55, 128),

            /* bishop mobility */
            S(  0, -29),   S(  6,  38),   S( 16,  81),   S( 22, 112),   S( 26, 138),   S( 28, 158),   S( 32, 165),   S( 35, 166),
            S( 39, 169),   S( 46, 160),   S( 57, 149),   S( 82, 136),   S( 96, 133),   S( 71, 128),

            /* rook mobility */
            S(-12,  57),   S( -7, 127),   S( -7, 153),   S( -3, 161),   S( -5, 185),   S( -1, 192),   S(  1, 201),   S(  5, 201),
            S(  5, 211),   S(  9, 214),   S( 15, 216),   S( 24, 212),   S( 31, 213),   S( 41, 211),   S( 67, 194),

            /* queen mobility */
            S( 61, -11),   S( 69, -38),   S( 61, 106),   S( 54, 182),   S( 56, 226),   S( 54, 272),   S( 54, 307),   S( 55, 323),
            S( 54, 350),   S( 55, 365),   S( 52, 384),   S( 52, 395),   S( 51, 407),   S( 51, 413),   S( 52, 415),   S( 52, 418),
            S( 61, 403),   S( 66, 397),   S( 80, 378),   S(100, 359),   S(129, 327),   S(164, 291),   S(177, 263),   S(209, 239),
            S(198, 248),   S(192, 221),   S(110, 176),   S(110, 121),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  18),   S(-13,  45),   S(-37,  56),   S(-30,  59),   S(  1,  24),   S( -4,  11),   S(-12,  64),   S( 25,  33),
            S( 11,  35),   S(  0,  46),   S(-21,  51),   S(-19,  43),   S(  2,  34),   S(-33,  49),   S(-35,  63),   S( 38,  30),
            S( 18,  73),   S( 14,  72),   S(  8,  54),   S( 23,  49),   S(  1,  52),   S(-38,  76),   S(-40, 107),   S(-10,  85),
            S( 28, 109),   S( 47, 122),   S( 28,  81),   S(  7,  61),   S(  7,  63),   S(  2,  90),   S(-42, 125),   S(-82, 156),
            S( 17, 153),   S( 57, 187),   S( 54, 135),   S( 40, 108),   S(-48, 105),   S( 28, 109),   S(-42, 167),   S(-85, 169),
            S( 94, 226),   S( 89, 268),   S(120, 245),   S(119, 257),   S(135, 264),   S(167, 240),   S(123, 256),   S(130, 267),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,   8),   S( -6, -30),   S(  0, -11),   S( -2,   5),   S( 12,  13),   S(-12, -43),   S(-25,  12),   S( -1, -52),
            S(-18,   6),   S( 23, -16),   S( -5,  31),   S( 11,  20),   S( 35, -10),   S( -8,  15),   S( 21, -17),   S( -3,  -8),
            S(-14,  20),   S( 17,  -1),   S( -1,  50),   S( 12,  61),   S( 24,  27),   S( 27,  24),   S( 33,  -5),   S( -7,  21),
            S( 16,  53),   S( 11,  34),   S( 44, 120),   S( -1, 100),   S( 82,  68),   S( 52,  53),   S( 36,  66),   S( -3,  21),
            S( 26,  92),   S( 93,  93),   S(112, 146),   S(100, 185),   S(126, 120),   S(143, 144),   S(117, 129),   S( 37,  51),
            S( 64, 188),   S( 98, 276),   S( 90, 210),   S( 88, 178),   S( 67, 163),   S( 61, 147),   S( 55, 171),   S( 27, 118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  22),   S( 18,  23),   S( 36,  31),   S( 30,  23),   S( 20,  20),   S( 26,  20),   S(  2,  13),   S( 41,  -6),
            S( -3,  19),   S( 14,  31),   S( 11,  36),   S(  6,  41),   S( 24,  17),   S(  8,  22),   S( 28,  19),   S( -3,  13),
            S( -4,  20),   S( 28,  48),   S( 49,  62),   S( 42,  49),   S( 42,  56),   S( 72,  17),   S( 30,  36),   S( 21,   5),
            S( 66,  69),   S(115,  54),   S(118, 123),   S(137, 147),   S(158, 110),   S( 73, 136),   S( 70,  63),   S( 61,  13),
            S( 65, 118),   S( 77, 167),   S(146, 208),   S(109, 297),   S(109, 265),   S( 93, 265),   S(138, 214),   S(-55, 160),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 35,  31),   S( 14,  14),   S( 15,  33),   S(-11,  69),   S( 61,  35),   S( 22,  11),   S( -2,   5),   S( 38,  15),
            S(  1,  15),   S(  5,   9),   S( 17,  21),   S( 17,  33),   S( 14,  19),   S(  0,   9),   S(  4,   8),   S( 29,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -15),   S( -5,  -9),   S(-17, -21),   S(-17, -33),   S(-14, -19),   S(  0,  -9),   S( -4,  -8),   S(-29,   4),
            S(-35, -31),   S(-14, -14),   S(-15, -33),   S( 11, -69),   S(-61, -35),   S(-22, -11),   S(  2,  -5),   S(-38, -15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -39),   S(-14, -38),   S(-11, -53),   S(-64, -34),   S(-24, -52),   S(-31, -48),   S(  3, -54),   S(-24, -65),
            S(-28, -25),   S(-20, -28),   S(-34, -20),   S( -4, -41),   S(-40, -36),   S(-26, -30),   S(-40, -19),   S(-14, -46),
            S(-17, -25),   S(-10, -34),   S(-26, -11),   S(-34, -21),   S(-23, -40),   S(-19, -25),   S( -8, -24),   S(-44, -30),
            S( -9, -34),   S( 18, -47),   S( 14, -19),   S( 11, -36),   S( 11, -32),   S( 59, -44),   S( 36, -44),   S( -7, -60),
            S(  6, -50),   S( 41, -72),   S( 59, -37),   S( 56, -23),   S( 86, -55),   S( 71, -28),   S(121, -85),   S( 35, -88),
            S( 97, -95),   S(129, -109),  S( 96, -46),   S( 64, -28),   S( 73, -35),   S(102, -26),   S( 91, -38),   S( 24, -75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-12, -24),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  3,   1),        // attacks to squares 1 from king
            S( 11,   4),        // attacks to squares 2 from king

            /* castling available */
            S( 68, -87),        // king-side castling available
            S( 19,  37),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 37, -85),   S( 46, -79),   S( 41, -88),   S( 33, -75),   S( 30, -65),   S( 18, -57),   S(  9, -47),   S(  3, -41),
            S( 19, -48),   S( 44, -50),   S( 58, -43),   S( 49, -37),   S(101, -41),

            /* orthogonal lines */
            S(-48, -144),  S(-98, -105),  S(-117, -93),  S(-132, -88),  S(-138, -89),  S(-140, -92),  S(-139, -97),  S(-134, -97),
            S(-146, -90),  S(-153, -92),  S(-146, -109), S(-109, -135), S(-76, -146),  S(  3, -172),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 27, 243),

            /* passed pawn can advance */
            S( -7,  34),   S( -6,  63),   S( 11, 105),   S( 53, 186),

            /* blocked passed pawn */
            S(  0,   0),   S( 55, -26),   S( 46, -18),   S( 27,  41),   S( 30,  63),   S( 34,  38),   S( 77,  74),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 67, -58),   S( 37,  16),   S( 25,  33),   S( 11,  72),   S( 37,  92),   S(143, 129),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-16, -15),   S( -6, -35),   S(  1, -30),   S(-29,  -7),   S(-45,  23),   S( 93,  19),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 48, -48),   S( 21, -25),   S( 11, -11),   S( 21, -60),   S( -2, -128),  S(-34, -228),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 31,  58),   S( 38,  38),   S(157,  29),   S( 30,  27),   S(176, 124),   S(106, 129),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 19,  60),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-43, 122),

            /* bad bishop pawn */
            S( -7, -15),

            /* rook on open file */
            S( 38,   6),

            /* rook on half-open file */
            S( 12,  40),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 24,  38),   S( 25,   4),   S( 35,  22),   S( 26,   4),   S( 33, -21),

            /* pawn threats */
            S(  0,   0),   S( 68, 107),   S( 53, 136),   S( 79,  84),   S( 56,  63),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 31,  62),   S( 51,  48),   S( 78,  38),   S( 49,  58),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 23,  55),   S( 28,  53),   S(-14,  46),   S( 70,  59),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 20,   9),   S( 22,  34),   S( 41,  10),   S(  9,  28),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 16,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
