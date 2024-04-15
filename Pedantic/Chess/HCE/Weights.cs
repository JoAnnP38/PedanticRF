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
        public const int MAX_WEIGHTS = 12794;
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
        public const int ROOK_BEHIND_PASSER = 12757;// rook behine passed pawn
        public const int BISHOP_PAIR = 12758;       // bonus for having both bishops
        public const int BAD_BISHOP_PAWN = 12759;   // bad bishop pawn
        public const int ROOK_ON_OPEN_FILE = 12760; // rook on open file
        public const int ROOK_ON_HALF_OPEN_FILE = 12761;    // rook on half-open file
        public const int PAWN_SHIELDS_MINOR = 12762;// pawn shields minor piece
        public const int PAWN_PUSH_THREAT = 12763;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12769;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12775;      // minor piece threat
        public const int ROOK_THREAT = 12781;       // rook threat
        public const int CHECK_THREAT = 12787;      // check threat against enemy king
        public const int TEMPO = 12793;             // tempo bonus for side moving

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

        public Score PawnShieldsMinor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[PAWN_SHIELDS_MINOR];
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

        // Solution sample size: 6000012, generated on Sun, 14 Apr 2024 21:44:56 GMT
        // Solution K: 0.003850, error: 0.083583, accuracy: 0.5085
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 71, 231),   S(386, 685),   S(415, 672),   S(539, 1088),  S(1385, 1816), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(109, -131),  S(154, -100),  S( 46, -53),   S(-14,  35),   S(-22,  35),   S(-24,   2),   S(-46,  -2),   S(-22, -18),
            S(132, -136),  S(109, -108),  S( 14, -64),   S(-11, -39),   S(-12, -17),   S(-18, -36),   S(-31, -23),   S(-25, -37),
            S(120, -114),  S( 67, -64),   S( 19, -74),   S( 13, -71),   S( -6, -60),   S( 10, -65),   S( -8, -55),   S(  5, -51),
            S( 77, -43),   S( 51, -63),   S( 36, -67),   S( 21, -80),   S(-20, -45),   S(-13, -64),   S(-20, -31),   S( -9, -29),
            S( 70,  36),   S( 25, -11),   S( 31,   0),   S( 50, -91),   S(-12,  -6),   S(-17, -45),   S(-34,  -1),   S(-26,  48),
            S( 72,  58),   S( 54,  78),   S(-17, -22),   S( 18,  -9),   S(-56, -13),   S(  2,   5),   S( 14,  33),   S( 16,  44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 33, -32),   S( 35, -36),   S( 53, -31),   S(  3,  14),   S(-18,  -4),   S( 10, -14),   S(-35,   1),   S(-26,  20),
            S( 32, -43),   S( 24, -45),   S( 19, -52),   S( -2, -42),   S( -7, -22),   S( -8, -30),   S(-21, -20),   S(-30, -14),
            S( 28, -40),   S( 16, -29),   S( 15, -57),   S( 13, -57),   S(-20, -28),   S( 18, -51),   S( -4, -42),   S(  9, -32),
            S( 34, -14),   S( 31, -59),   S( 34, -62),   S(  6, -52),   S(-11, -26),   S( 16, -54),   S(-21, -21),   S( -9,  11),
            S(  4,  59),   S(-44,  -5),   S(-19, -23),   S(  3, -50),   S( 18, -18),   S(-16, -13),   S(-48,  31),   S(-29,  73),
            S( 72,  64),   S( 46, -31),   S(-32,  -5),   S(-20,  18),   S(-19,  25),   S(-56,  22),   S(-72,  37),   S(-78, 110),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12,  -4),   S(-23,   1),   S( -6,  -1),   S( -8,  10),   S( 11,  -7),   S( 34, -16),   S( 12, -43),   S(  2, -20),
            S( -6, -24),   S(-29, -17),   S(-17, -37),   S(-10, -38),   S( 12, -29),   S( 10, -33),   S(  3, -42),   S(-14, -28),
            S( -6, -28),   S(-21, -29),   S( -7, -58),   S( -2, -57),   S( -6, -30),   S( 23, -43),   S(  1, -40),   S( 16, -31),
            S(-13, -10),   S(-11, -51),   S( -6, -58),   S( -7, -51),   S(  7, -47),   S(  8, -36),   S(  0, -27),   S(  3,  -9),
            S( -9,  37),   S(-46, -12),   S(-43, -42),   S(-60, -31),   S( 21, -20),   S( -5,  -5),   S(-12,  22),   S(-12,  79),
            S(-43,  69),   S(-93,  70),   S(-97,   4),   S(-76, -39),   S(-46,  25),   S( -9,  27),   S(-16,  36),   S(-13,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -16),   S(-26,  -4),   S(-21,  -8),   S(  2, -32),   S(  3, -11),   S( 49, -23),   S( 93, -72),   S( 73, -86),
            S( -5, -43),   S(-27, -34),   S(-19, -47),   S(-12, -36),   S( -2, -29),   S( 17, -46),   S( 65, -77),   S( 63, -78),
            S( -1, -51),   S( -4, -59),   S(  0, -71),   S( -1, -71),   S( -6, -54),   S( 31, -61),   S( 38, -71),   S( 83, -78),
            S(  0, -38),   S(  7, -80),   S(  4, -76),   S(  8, -78),   S( 21, -77),   S( 24, -69),   S( 29, -51),   S( 72, -38),
            S( 17,  12),   S( -6, -45),   S( 19, -80),   S(  9, -80),   S( 89, -72),   S( 73, -49),   S( 58,  10),   S( 55,  60),
            S(-32, 100),   S(-30,  35),   S( -4, -62),   S( 11, -68),   S( 50, -62),   S( 53, -28),   S( 75,   0),   S( 72,  28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-85,  25),   S(-13, -12),   S(-56,  23),   S( 21,  27),   S(  4, -33),   S(-62,  26),   S(-37, -11),   S(-48,   1),
            S( -7,  11),   S(  4,   5),   S( 18, -29),   S( 30, -31),   S( -3, -24),   S(-39, -30),   S(  3, -38),   S( 12, -28),
            S( 48, -16),   S( 23,   6),   S(-36,  15),   S( -9, -23),   S(-29, -33),   S( -3, -46),   S(-40, -44),   S(  9, -32),
            S( 15,  37),   S(-31,  45),   S( 42, -14),   S(  1,  -1),   S( 16, -37),   S(-22, -44),   S(-29, -49),   S( 62, -28),
            S(  8,  88),   S(-49,  84),   S(-49,  38),   S( -8, -15),   S(  6,  12),   S( -8,   6),   S(-16, -16),   S( 28,  43),
            S( 43, 101),   S( 44, 116),   S(  4,  24),   S(  2,  -5),   S( 24, -14),   S( 26,  10),   S(  6,  -2),   S(-35,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-113,  65),  S(-71,  46),   S(-25,  24),   S(  4,  15),   S(-46,  41),   S(-43,  29),   S(-52,  18),   S(-29,  30),
            S(-70,  25),   S(-63,  24),   S( 48, -15),   S(  0,  16),   S( 25,  -6),   S(-15, -20),   S(-31,  -5),   S(-23,   9),
            S(-61,  41),   S(-51,  29),   S( 55, -32),   S(  9, -27),   S( 10,  -8),   S(-16, -21),   S( -8, -15),   S( 13,  -9),
            S(-72,  60),   S(-60,  36),   S( -6,   5),   S( 15,  16),   S(-24,   8),   S(-33,  -8),   S( -7,  -9),   S( 43,  -5),
            S(  9,  60),   S( -1,  56),   S( 20,  44),   S(  3,  33),   S(  5,  18),   S( 50, -18),   S( 41,  -5),   S( 20,  38),
            S( 53,  57),   S( 32,  17),   S( 24,   8),   S( 44,  -4),   S( 50, -10),   S(  3, -24),   S( 13,  20),   S( 21,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50,  32),   S(-59,  24),   S(-31,  24),   S(-17,   7),   S( 40, -31),   S(-32,  18),   S(-62,   9),   S(-64,  22),
            S(-36,   1),   S(-17, -22),   S(-18, -28),   S( -8,  -4),   S( 33, -14),   S( 17, -16),   S(-32,  -6),   S(-71,  11),
            S(-23,  -4),   S(-15,  -8),   S(-24, -17),   S(-45,   1),   S(  9,  -7),   S( 63, -35),   S(  1, -23),   S(-25,   9),
            S(-16,  10),   S(-85,  15),   S( -2, -24),   S(-23,   4),   S(  6,   3),   S( 23, -13),   S( 32, -16),   S( 30,   6),
            S( 22,  18),   S(-48,  11),   S(  2, -29),   S(  3, -26),   S( 26,  35),   S( 14,  37),   S( 51,   4),   S( 46,  36),
            S( 59,  39),   S( 28,  17),   S(-29, -13),   S( -5, -33),   S( 27,  -2),   S( 25,   4),   S( 44,  -8),   S( 59,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -24),   S(-51, -12),   S(-18,  -5),   S(-79,   9),   S(  3, -34),   S( 21, -15),   S(  2, -50),   S(-25, -31),
            S(-39, -38),   S(-44, -45),   S(-40, -44),   S( 13, -58),   S(  6, -42),   S( 56, -57),   S( 71, -60),   S( 10, -40),
            S(-18, -54),   S(-51, -39),   S(-30, -54),   S(  3, -55),   S( -2, -35),   S( 46, -41),   S( 51, -61),   S( 56, -46),
            S(-15, -49),   S(-30, -66),   S(-65, -51),   S(-50, -18),   S(  1, -30),   S( 22, -22),   S( 27, -16),   S( 73, -28),
            S(  8, -41),   S(  2, -67),   S(-21, -52),   S( -1, -67),   S( 58, -19),   S( 23,  -6),   S( 74,  52),   S(106,  24),
            S(-12, -22),   S(-13, -26),   S( 25, -70),   S(  1, -35),   S( -5,  -3),   S( 35, -22),   S( 19,  49),   S( 37,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50,  83),   S(-12,  56),   S( 13,  22),   S(-20, -24),   S( 18,  33),   S( 35,   0),   S(-55,  15),   S(-68,  33),
            S(-61,  70),   S(-37,  38),   S(-26,  19),   S( -6,  11),   S(-42,  -5),   S(-30,  -5),   S(-42, -14),   S(  5, -19),
            S(-83, 125),   S(-27, 102),   S( -3,  55),   S(-40,  33),   S( 29, -12),   S(-83, -23),   S(-55, -34),   S(-23,  -6),
            S(-29, 145),   S( 12, 165),   S(-21, 108),   S( 13,  46),   S(  3,  -5),   S(-21, -41),   S(-16,  -4),   S(-28,   6),
            S(-29, 187),   S( 32, 166),   S( 28, 156),   S( 90, 121),   S( 12, -28),   S( 19,   1),   S( -6, -30),   S(  0,  -5),
            S( 51, 184),   S( 76, 224),   S(101, 219),   S( 36,  99),   S( 17,  32),   S(-15,   5),   S( -8, -28),   S(  1,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-97,  76),   S(-80,  70),   S( 30, -12),   S( 16,  44),   S( 26,  15),   S(-26,  10),   S(-78,   5),   S(-106,  46),
            S(-82,  48),   S(-54,  32),   S(-80,  44),   S(-19,  69),   S(-67,  10),   S(-26, -13),   S(-63, -14),   S( 10,  -3),
            S(-107,  79),  S(-95,  95),   S(-62,  81),   S(-123,  96),  S(-76,  63),   S(-90,  13),   S(-40, -28),   S(-30,  -5),
            S(-65, 102),   S(-47, 134),   S(-25, 137),   S( 20, 147),   S(-44,  66),   S(-23,   9),   S( 13,   1),   S(-66,  32),
            S( 32, 119),   S( 32, 138),   S( 16, 166),   S( 38, 176),   S( 26, 135),   S(  6,  47),   S( -7,  -6),   S(-11, -15),
            S( 31,  66),   S( 31, 134),   S( 66, 146),   S( 65, 182),   S( 40, 117),   S(-13, -15),   S(-14, -14),   S(-26, -41),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-104,  24),  S(-78,   7),   S( 22,   9),   S(  4,  31),   S(-14,   4),   S(-62,  28),   S(-134,  35),  S(-58,  36),
            S(-81,   4),   S(-77,  -2),   S(-15, -20),   S(  6,  -9),   S(-15,  26),   S(-28,  21),   S(-132,  37),  S(-83,  14),
            S(-17,  -9),   S(-89,   9),   S(-23,   0),   S(-82,  77),   S(-113, 102),  S(-25,  60),   S(-113,  36),  S(-87,  43),
            S(-64,  10),   S(-94,  54),   S(-23,  13),   S(-52,  98),   S( 43,  85),   S(-59,  88),   S(-32,  49),   S( 11,  25),
            S(-37,  43),   S(-40, -11),   S(  0,  50),   S( 44, 121),   S( 89, 122),   S( 59,  74),   S( -8,  88),   S( 22,  54),
            S(  6,  21),   S(-14,   0),   S( 21,  44),   S( 60, 113),   S( 15, 140),   S( 35,  88),   S(-13,  83),   S( 31, 108),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71, -14),   S(-69,  15),   S( 59, -38),   S(  7,  12),   S( -4,  34),   S(-108,  72),  S(-66,  40),   S(-51,  36),
            S(-76, -15),   S(-90,  -9),   S(-23, -34),   S(-47,  11),   S(-34,  26),   S(  5,  15),   S(-120,  79),  S(-126,  63),
            S(-33, -43),   S(-55, -33),   S(-34, -10),   S(-21, -12),   S(-95,  51),   S( -2,  60),   S(-87,  94),   S(-65,  79),
            S(-75,   4),   S(-98, -19),   S( -5, -33),   S(-44,  26),   S( -8,  48),   S(-31,  85),   S( 30, 118),   S( 92,  82),
            S(-21,  23),   S(-42,  30),   S(  2,   6),   S(-16,  21),   S( 61,  99),   S(-16, 139),   S(108, 145),   S( 84, 117),
            S(-58,  54),   S(-12, -10),   S(  2, -42),   S(-10,  -5),   S( 31,  48),   S( 36, 156),   S( 66, 194),   S( 56, 185),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-18,   3),   S(-17,  64),   S(-26,   4),   S(  7,  12),   S(  0,  -1),   S( -5,  33),   S( -4, -19),   S(-29, -28),
            S(-23,   1),   S( -1,   1),   S( -1,  32),   S(  2,  -9),   S(  6,  36),   S( 17,  12),   S(-28, -30),   S(-25, -66),
            S(-32,  48),   S(-26, 102),   S( 10,  52),   S( 18,  46),   S(  7,  24),   S(-43,  -9),   S(-42, -75),   S(-52, -60),
            S(-29,  87),   S(-37, 144),   S( 40, 132),   S( 34, 102),   S(-19, -25),   S(-42,  -3),   S(-17, -10),   S(-59, -50),
            S( 47,  95),   S( 56, 226),   S( 49, 144),   S( 29,  64),   S(  7,  38),   S(  1, -17),   S( -6,   7),   S(-18, -59),
            S( 37, 105),   S( 63, 220),   S(126, 225),   S( 48, 112),   S( -5, -11),   S(-11, -11),   S( -6, -16),   S(-26, -37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49, -36),   S(-38, -34),   S( -9,   9),   S(  0,   8),   S(-14, -18),   S(-33, -14),   S(-29, -33),   S(-19,   0),
            S(-45,  -7),   S(-56,  50),   S(-25,  49),   S( 17,  37),   S(-55,  21),   S(-13, -23),   S(-75, -46),   S(-66,   2),
            S(-54,  56),   S(-57,  63),   S(-58, 102),   S(  8,  90),   S(  9,  24),   S(-48, -33),   S(-50, -30),   S(-59, -34),
            S(-77, 109),   S( -1, 125),   S(-28, 149),   S(  4,  97),   S(  7,  83),   S(-25,  37),   S( -7,   0),   S(-41, -14),
            S(  9,  90),   S( 49, 165),   S( 71, 210),   S( 49, 242),   S( 35, 164),   S(-12,   4),   S(  8, -76),   S(-32, -65),
            S( 54,  65),   S( 79, 184),   S( 89, 198),   S( 90, 271),   S( 50, 134),   S(  3,  17),   S(  6,   5),   S( -2, -32),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -51),   S(-30, -30),   S( -9, -37),   S(  0,  12),   S( -9,  -2),   S(-31,  11),   S(-47, -18),   S( -4,  75),
            S(-54,  25),   S(-78,  13),   S(-43, -11),   S( 14,  27),   S(-38,  58),   S(-22, -17),   S(-34,   9),   S(-35,  25),
            S(-49, -43),   S(-58,   2),   S(-26, -12),   S(-25,  83),   S(-11,  60),   S(-64,  42),   S(-38,   4),   S(-53,  37),
            S(-49, -21),   S(-16,  42),   S(-33,  42),   S( -1,  78),   S(-22, 150),   S(-25,  88),   S(-33,  34),   S(-36,  71),
            S(-37, -31),   S(  1,  -5),   S(  7,  94),   S( 39, 140),   S( 61, 222),   S( 41, 160),   S( 16,  97),   S(  9,  48),
            S( -8,  32),   S( 10,   0),   S( 36, 115),   S( 28, 139),   S( 57, 186),   S( 48, 105),   S( 34,  84),   S( 27,  57),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -21),   S(-32, -35),   S( -4, -18),   S( -1,  -3),   S( 11,   3),   S( 14,  66),   S(-27, -30),   S( 10,  27),
            S(-58, -30),   S(-41,  -9),   S(-20, -61),   S( 28, -13),   S(-24,  -4),   S(-13,  50),   S(-10,  16),   S(-16,  33),
            S(-16, -76),   S(-28, -45),   S(-23, -73),   S( 10,  -5),   S( 10,  20),   S( -5,  88),   S(-22,  76),   S(-37,  82),
            S(-29, -50),   S(-48, -41),   S(-51, -31),   S(  5,  33),   S(-19,  44),   S(  2,  82),   S(-14, 148),   S( -4,  33),
            S(-30, -20),   S(-24, -14),   S( -3,  24),   S( 12,  17),   S( 48, 114),   S( 57, 154),   S( 53, 241),   S( 71,  80),
            S(  0,  46),   S(-19,  -9),   S(  8,  37),   S(  2,  20),   S( 21,  77),   S( 88, 199),   S( 29, 147),   S( 44,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-28,  -2),   S( 31,   6),   S(-36,   7),   S(-41,  15),   S(-27, -12),   S(-10, -29),   S(-54, -32),   S(-41, -18),
            S(-18,  51),   S( 22, -22),   S(-47,  -1),   S(  4, -25),   S( -8, -20),   S(-36,  -7),   S(-44,  -7),   S(-55, -35),
            S(  2,  59),   S(  6, -19),   S(  3, -12),   S(-17,  29),   S(  3,   7),   S(-35,  -5),   S( -7, -28),   S(-52, -42),
            S( 22, -35),   S( 21,  17),   S( -3,  46),   S( 21,  33),   S( 21,  -8),   S(  6,   2),   S( -6, -14),   S(  0, -14),
            S( 20, -19),   S( 43,  -7),   S( 22, -11),   S( 71, -11),   S( 24,   2),   S( 23,  30),   S( 30, -23),   S(-57,  -3),
            S( 19, -27),   S( -5,  16),   S( 33,  25),   S( 47, -17),   S( 42, -59),   S( 40,   6),   S( -9,   3),   S(-43, -43),
            S(-12, -29),   S( 52, -48),   S( 28, -33),   S( 12, -26),   S( 11,  12),   S( -1, -43),   S( -7, -47),   S( 28,  11),
            S(-53, -34),   S( -8,  15),   S( 14,  -3),   S(  7, -23),   S(-34, -14),   S( 13,  13),   S(  3,  15),   S( 23,  -3),

            /* knights: bucket 1 */
            S(-31,  57),   S(-64,  77),   S( 11,  35),   S(-43,  62),   S(-36,  49),   S(-14,  20),   S(-39,  63),   S(-26,  -2),
            S( 37,  36),   S(  7,  33),   S(-22,  27),   S(-13,  50),   S(-12,  13),   S(-17,  -4),   S( -6, -23),   S(-12,  11),
            S(-39,  47),   S( 17,   8),   S(  0,  10),   S(  8,  35),   S( -7,  30),   S(-36,  34),   S(-23,  11),   S(-22,  -1),
            S(-13,  33),   S( 42,  26),   S( 18,  46),   S( 22,  33),   S(  3,  31),   S(  5,  12),   S( 23,   4),   S( 14,  25),
            S(  3,  53),   S( 25,  18),   S( 30,  21),   S( 54,  14),   S( 29,  29),   S( 23,  23),   S( 34,   5),   S( 27,  27),
            S(-20,  16),   S( 22,  -7),   S( 10,  46),   S( 38,  28),   S( 13,   0),   S( 42,  28),   S( 43,   2),   S( 38,   0),
            S( 10,  37),   S( 41,  19),   S(-25, -21),   S(  1,  36),   S( 23, -33),   S( 32, -23),   S(-42,  27),   S( 42, -21),
            S(-78, -53),   S(-11,  -4),   S( -5,  -7),   S(-19,  -3),   S( 18,   6),   S(-40,  -8),   S(-13,   1),   S(-68, -42),

            /* knights: bucket 2 */
            S(-43,  15),   S( -4,  19),   S(-34,  45),   S(-40,  40),   S(-52,  68),   S(-45,  76),   S(-19,  31),   S(-54,  60),
            S( -6, -14),   S( -5,  -1),   S(-12,  13),   S( -9,  22),   S( -6,  18),   S(-11,  39),   S(-32,  60),   S(-36,  84),
            S(-17,  25),   S( -1,  14),   S(-10,  32),   S( 24,  17),   S(  6,  30),   S(  3,  12),   S( -7,  42),   S(-28,  41),
            S(-12,  44),   S(-15,  37),   S(  4,  38),   S( 11,  46),   S( -2,  45),   S( -3,  45),   S(  3,  41),   S(  4,  45),
            S( 19,  30),   S(-12,  27),   S( -1,  42),   S(-24,  58),   S( 11,  38),   S(  0,  42),   S( 12,  32),   S(-15,  23),
            S(-18,  27),   S(  7,  16),   S(-19,  43),   S(-20,  30),   S(-26,  60),   S( -1,  28),   S(-33,  16),   S( 34,   0),
            S( 22,  12),   S(-73,  28),   S(-42,  28),   S(  2,  25),   S(  4,  -8),   S(-37,  39),   S(-44,  41),   S(-23,  17),
            S(-158,  24),  S(-22,  19),   S(-73,  33),   S(  7, -16),   S( 12,  27),   S(-64,  11),   S( 15,  43),   S(-158,  -1),

            /* knights: bucket 3 */
            S(-32, -15),   S(  8, -27),   S(-29, -10),   S(  4, -21),   S( -7,  -4),   S(-20,   6),   S( 20, -24),   S( 10, -26),
            S(-19,  34),   S(-28,  -6),   S(-24,   1),   S(  5,   6),   S( 18,  -2),   S( -8,  -8),   S( -4,  -1),   S(-18,  61),
            S( -3, -22),   S(  7,   9),   S(  1,   0),   S( 23,  -1),   S( 21,  12),   S( 20,   2),   S( 12,  -1),   S(  5,  43),
            S( -2,   3),   S( 18,  -2),   S( 16,  31),   S( 25,  32),   S( 31,  29),   S( 27,  26),   S( 43,   8),   S( 31,  19),
            S( 31,  10),   S( 10,  16),   S( 31,  10),   S( 32,  37),   S( 29,  33),   S( 35,  50),   S( 42,  43),   S(  9,  26),
            S( 25, -11),   S( 43, -24),   S( 51,  -4),   S( 47,  11),   S( 70, -16),   S( 80, -15),   S( 19,  -4),   S( 34,  52),
            S( 27,   7),   S(-10,  11),   S( 63, -31),   S( 27,   5),   S( 69, -33),   S( 55, -30),   S( 59, -40),   S( 71, -34),
            S(-137,  59),  S(-27,  19),   S( -4, -16),   S( 30,  -2),   S(  7,  16),   S(-16, -18),   S( 27, -27),   S(-12, -51),

            /* knights: bucket 4 */
            S( 20,  34),   S(-27,  22),   S(-20,  35),   S(-12, -28),   S(-23, -17),   S(-38, -41),   S(  1, -24),   S(-29, -44),
            S( 37,  55),   S(-45,  26),   S(  0, -57),   S(  3,   9),   S(  1, -14),   S(  3, -66),   S( -5, -28),   S(  9, -46),
            S(-15,  44),   S(  0,  33),   S(-10,  22),   S( 24,  11),   S(  3,  -4),   S(-57,  -8),   S(-23, -22),   S( 14, -50),
            S( -7,  81),   S( 42,  -2),   S( 56,  15),   S( 23,   5),   S(  9,   9),   S(109, -25),   S( 13, -15),   S( -2, -22),
            S( 74,  42),   S(-25,  33),   S( 63,  38),   S( 82,   4),   S( 45,  40),   S( -2,   5),   S( 11, -45),   S(  9,   6),
            S(  4,   0),   S( -2,  33),   S(100,  -8),   S( 20,  -1),   S( -4,  22),   S( 27,  28),   S( 13,  32),   S( -8, -35),
            S( -6, -12),   S(-13,   6),   S( 30,  -2),   S(  2,  50),   S(  5,  -6),   S(  6, -17),   S( 21,   9),   S(-13,   5),
            S( -6,   8),   S(  8,  20),   S(  7,   7),   S( -1,   8),   S(  2,   3),   S( 19,  25),   S( -3,   9),   S( -8, -31),

            /* knights: bucket 5 */
            S( -9,   3),   S(-11,  36),   S( 36,  43),   S(  7,  55),   S( -1,  23),   S(  3, -11),   S( 15,  33),   S(-19, -34),
            S( 12,  -5),   S( 48,  67),   S( 37,   5),   S(-19,  35),   S( 24,  40),   S( -3,  28),   S( 10,  12),   S( -5, -19),
            S(  8,  30),   S(-33,  44),   S( 82,  12),   S( 31,  34),   S(-44,  52),   S(-30,  36),   S(-21,  20),   S(  2,   5),
            S( 25,  45),   S( 10,  38),   S( 69,  25),   S( -6,  59),   S( 13,  44),   S(-13,  54),   S( 19,  37),   S( 28,  44),
            S( 14,  37),   S( 66,  12),   S( 35,  60),   S( 18,  60),   S( 97,  34),   S( 16,  47),   S( 41,  51),   S( 17,  34),
            S( -8,  34),   S(-15,  35),   S( 16,  38),   S( 13,  58),   S( 42,  45),   S( 21,  64),   S( 15,  16),   S( -4,  22),
            S( 23,  43),   S(-25,  71),   S(  5,  44),   S( 18,  70),   S( 11,  33),   S( -3,  30),   S( 11,  56),   S(  5,   3),
            S(  2,  42),   S(  4,  -7),   S( 13,  50),   S( -5,   7),   S(  9,  56),   S(  8,  61),   S( -7,   4),   S(-18, -18),

            /* knights: bucket 6 */
            S( 13, -44),   S(-14, -28),   S( 45,  30),   S(-38,  55),   S(-21,  47),   S( 32,  58),   S(-20,  36),   S(-19,  20),
            S( -2, -42),   S( 44,  15),   S(  4,  17),   S(-20,  21),   S(-73,  72),   S(  5,  52),   S(  4,  58),   S(-23,  11),
            S( -9, -25),   S(-15,   9),   S( 15,  12),   S( 20,  27),   S( 17,  41),   S(-23,  51),   S(-11,  64),   S(-22,  45),
            S( 24,  16),   S( 73,  14),   S( 45,  28),   S( 62,  30),   S( 34,  42),   S( 33,  48),   S( 31,  57),   S(-46,  81),
            S(-25,  40),   S( 50,   2),   S( 60,  29),   S( 83,  30),   S(106,  33),   S( 83,  39),   S(-11,  65),   S( 25,  50),
            S( 21,  25),   S( 12,  14),   S( 78,  15),   S( 79,  26),   S( 83,  42),   S( 57,  29),   S( -2,  44),   S( 30,  45),
            S( -9,  29),   S( -4,  48),   S(-31,  34),   S( 22,  27),   S( 29,  59),   S(  9,  33),   S( 16,  73),   S(-18,  41),
            S(-25,  18),   S( 13,  57),   S( 10,  33),   S(  2,  36),   S( 33,  47),   S( 19,  64),   S( 26,  72),   S( 14,  36),

            /* knights: bucket 7 */
            S(-48, -50),   S(-204, -22),  S(-91, -15),   S(-73, -29),   S(-39,  -6),   S(-41,   3),   S(-17,  14),   S(-21, -18),
            S(-20, -87),   S(-54, -48),   S(-43, -33),   S(-54,  -7),   S(-57,  14),   S(  2,  -9),   S( 20,  47),   S( -7,  41),
            S(-72, -55),   S(-30, -61),   S(-64,  10),   S(  3, -24),   S(-14,  -1),   S( -3,   7),   S(-17,  68),   S( 25,  66),
            S(-32, -25),   S( 15,  -7),   S(  6,   1),   S( 38,  12),   S( 61,  -6),   S( 18,   5),   S( 10,   9),   S(-64,  58),
            S(-77, -36),   S(-35, -24),   S( 49, -35),   S( 97, -17),   S(107,  -6),   S( 57,  20),   S(101,  -3),   S(103,  24),
            S( 15, -33),   S( 10, -50),   S(-19,  -1),   S( 22,  -2),   S( 74,   5),   S( 95,  -4),   S( 66, -25),   S( 22,  26),
            S(-30,  -7),   S(-89, -27),   S(  8,  -1),   S( 43,   9),   S( 17,  12),   S( 68,  10),   S(-14,  26),   S(  0,  10),
            S(-48, -25),   S( -4,  -5),   S(-40, -17),   S( 23,  31),   S(  4,   4),   S( 22,   2),   S(  9,  10),   S(-12,  -3),

            /* knights: bucket 8 */
            S(  0,   1),   S( -8,   3),   S(  1,  10),   S(-11, -18),   S( -9, -40),   S( -4, -56),   S(  2, -21),   S( -8, -27),
            S( -2, -25),   S( -6, -20),   S( -6, -40),   S(-35, -61),   S(-30, -27),   S(-12, -73),   S( -6, -55),   S(-24, -46),
            S(  4,  37),   S(-26, -45),   S( 12, -15),   S( -7, -13),   S(  4, -32),   S(-14, -31),   S(-10, -50),   S( -8, -24),
            S(-22, -10),   S(-16, -23),   S( 12,   6),   S( -1,  29),   S(  5,   3),   S(  8,  44),   S( -3, -18),   S(  3,   8),
            S( 31,  77),   S( -1,  -5),   S( 23,  24),   S( 13,   5),   S( 21,  11),   S( -8,  -3),   S(  6, -31),   S(-11, -16),
            S(  9,  25),   S( 15,  51),   S( 53,  24),   S( 49,  13),   S(  4,   6),   S( -5, -16),   S( -4, -22),   S( -1,  16),
            S(  5,  -4),   S(  0,  -8),   S( -8,  -3),   S( 13,  24),   S(  5,  15),   S( 17,  50),   S(  1,   0),   S( -1,   3),
            S(  2,   2),   S( 17,  44),   S(  3,   1),   S(  9,  17),   S(  6,  17),   S( -7, -26),   S(  4,  -9),   S(  0,  -2),

            /* knights: bucket 9 */
            S( -8, -10),   S(-22, -42),   S(-24, -46),   S( -6, -19),   S(-16, -34),   S(-21, -47),   S(  3,   7),   S( -2, -13),
            S(-15, -48),   S( -7,  10),   S( -9, -53),   S(-10, -11),   S( -8, -21),   S( -2, -36),   S( -7,   1),   S(-19, -52),
            S(  4,  10),   S( -2, -23),   S(  4, -22),   S( -7,   7),   S(-17,  14),   S(-38,   3),   S( -9, -11),   S( -5, -23),
            S(-14,   6),   S(-17, -36),   S( -9,  12),   S( 27,  19),   S( 30,  19),   S( 16,  12),   S(-28, -61),   S( -7, -15),
            S( -1,  51),   S(  2,  21),   S( -7,  35),   S( 17,  43),   S( 18,   6),   S( -8,  -9),   S( 13,   7),   S(  5,  13),
            S(  0,   2),   S(  6,  35),   S( 22,  32),   S(-20,  23),   S( 34,  24),   S( 21,  -2),   S(  2,  14),   S( -4, -30),
            S(  1,   0),   S(  0,  10),   S( 26,  44),   S( 13,  -9),   S( 16,  53),   S( -5, -20),   S(  8,  23),   S( -3,  -7),
            S(  5,   5),   S( 11,  27),   S( 16,  31),   S( 21,  44),   S( 11,   8),   S(  7,  17),   S(  5,   6),   S( -4, -19),

            /* knights: bucket 10 */
            S(-24, -68),   S(-20, -58),   S(-18, -54),   S(-31, -37),   S( -8, -18),   S(-15, -45),   S(  5,  22),   S(  5,  28),
            S(-11, -37),   S( -5,   0),   S( -1, -13),   S(-31, -40),   S(-19, -35),   S(-23, -38),   S( -7,  -5),   S( -8,  -6),
            S(-12, -49),   S(-20, -66),   S( -8, -17),   S(-13, -27),   S( 25,  -6),   S(-25,  -3),   S(-20, -26),   S( -8,  20),
            S( -1,   5),   S(  0, -38),   S(  2, -28),   S( 11,  13),   S( 11,  34),   S( 34,  45),   S( -2,  20),   S( 10,  42),
            S( -7, -45),   S( -9, -21),   S( 20,   9),   S( 33,  21),   S( 12,  36),   S( 19,  16),   S(  1,   4),   S( 23,  42),
            S( -5, -20),   S(-18, -31),   S( -8, -27),   S(  9,  25),   S( 39,  45),   S( 30,  22),   S( 30,  54),   S( 17,  75),
            S(  9,  20),   S( -7, -21),   S(  8,  -1),   S( 31,  23),   S( 14,  32),   S( 14,  52),   S( 11,  31),   S( 11,  43),
            S(  0, -18),   S( -1,   9),   S(-13,   1),   S(  9,  12),   S( 14,  49),   S(  4,  14),   S( 12,  30),   S(  0,   2),

            /* knights: bucket 11 */
            S(  3,   6),   S(-17, -13),   S( -9, -34),   S(-15, -22),   S(-20, -43),   S(-14, -20),   S(  0,  -6),   S( -2,  11),
            S(-13,  -9),   S(-25, -39),   S(-25, -72),   S(-14, -33),   S( -1,  -9),   S(-29, -20),   S(-26, -42),   S( -6,   5),
            S(-22, -54),   S(-23, -79),   S(-32, -28),   S( -4, -28),   S(-25,  15),   S( -8,  24),   S( 23,   7),   S( -3,  30),
            S(-31, -63),   S( -9, -32),   S(-11,  15),   S( 27,  28),   S( 30,  13),   S(  3,  11),   S(  6,   2),   S( 12,  -2),
            S( -4, -19),   S( -7, -38),   S(  6, -26),   S(  5,  12),   S( 13,  14),   S( 35,  41),   S(  8,  -8),   S(  8,  61),
            S( -4,   0),   S(  3, -15),   S( 14,   1),   S( 45,  33),   S( 15,  26),   S( 28,  37),   S( 37,  42),   S( 11,  52),
            S( 10,  31),   S(  3,   6),   S(  3, -28),   S( 16,   3),   S( 23,   9),   S( -7,  -6),   S( 18,  47),   S( 21,  69),
            S( -6,   3),   S( -8, -29),   S( 15,  26),   S( -1,  -8),   S(  2,  -1),   S( -2,   3),   S(  3,  10),   S(  0,   9),

            /* knights: bucket 12 */
            S( -3, -11),   S( -1,  -3),   S(  0, -20),   S( -4, -10),   S( -6, -14),   S( -2,  -4),   S( -1,  -2),   S( -3, -13),
            S(  0,   0),   S(  1,  -3),   S( -5, -18),   S( -8, -14),   S(  5,  15),   S( -2, -20),   S( -1,  -9),   S(  1,   3),
            S( -2, -10),   S( -2, -11),   S( -7, -20),   S( -3, -13),   S( -3,  -4),   S( -9, -32),   S( -1,   1),   S( -4, -10),
            S( -7,  -6),   S(  4,  15),   S(-16, -44),   S(  0,  30),   S(  4,  14),   S(  5,   9),   S( -2,   5),   S(  1,  -6),
            S(  5,   9),   S(  3,  10),   S(-11,  -9),   S( -6,  -1),   S( -5, -22),   S( 10,  13),   S(  2,  -3),   S(  2,   3),
            S(  1,   0),   S( -4,  -6),   S(  5,  15),   S(  6,  -4),   S( 10,  22),   S( -8, -28),   S(  0,  -9),   S( -1,  -5),
            S(  1,   3),   S( -3,   4),   S(  5,  16),   S(  1,  -4),   S( -2,  -1),   S( -7, -26),   S( -3,  -4),   S(  0,  -3),
            S(  5,  13),   S(  3,   8),   S( -1,  -3),   S(  1,   3),   S( -5,  -9),   S( -1,  -4),   S( -2,  -6),   S( -1,   0),

            /* knights: bucket 13 */
            S( -3, -12),   S( -2,  -7),   S( -3,  -9),   S(  2,  10),   S( -9, -21),   S( -6, -25),   S( -4, -17),   S( -2,  -6),
            S(  0,  -4),   S(  3,  12),   S( -1,  -9),   S( -7,  -7),   S(-11, -35),   S( -7, -38),   S(  0,  13),   S(  4,   7),
            S( -2, -11),   S( -6, -33),   S(  4,   5),   S(  3,   8),   S(-16, -37),   S( -6, -16),   S( -3,  -9),   S( -9, -38),
            S(-11, -21),   S(  9,  38),   S(  4,  10),   S(-11, -31),   S( -6, -37),   S(  1,   1),   S(  5,   1),   S( -5,  -8),
            S( -4,  -5),   S(  0,   4),   S( 14,   2),   S(  9,   5),   S(  8,   2),   S(-10,  -4),   S(  0, -26),   S( -1,   0),
            S(  2,   0),   S( 17,  22),   S( 19,  26),   S(-14,   6),   S( 13,   8),   S( -5, -34),   S(  3,   3),   S( -3,   1),
            S( -1,  -3),   S(  1,  -1),   S( -5, -11),   S(  7,   8),   S( 19,  35),   S( -8, -30),   S( -2,  -5),   S( -8,  -4),
            S(  0,   0),   S(  9,  18),   S( -1,   6),   S( -4, -22),   S( -4,  -9),   S(  3,   1),   S( -1,  -4),   S( -1,  -1),

            /* knights: bucket 14 */
            S( -2, -10),   S( -7, -24),   S(  1,   9),   S( -5, -12),   S( -8, -14),   S( -2, -13),   S(  0,  -5),   S( -1,  -9),
            S( -3,  -5),   S( -5, -14),   S(-17, -67),   S(-12, -35),   S(  6,  -1),   S( -1,  -1),   S(  3,  14),   S(  1,  10),
            S( -8, -14),   S( -3, -20),   S(-18, -75),   S( -4, -24),   S(  1,  -6),   S(  0, -10),   S( -7, -36),   S(  1,   5),
            S(  2,  21),   S( -5, -36),   S(-21, -54),   S(-16, -57),   S(  2,   2),   S(  4,  -5),   S(  0, -14),   S( -8, -13),
            S( -1, -10),   S( -7, -27),   S(  5,  39),   S( -1, -19),   S(-11,   5),   S(  3,  28),   S(  1,   9),   S( -5,  -8),
            S( -3, -10),   S(  6,  -4),   S( -6, -36),   S(  0, -22),   S( -2,  18),   S(  5,  37),   S(  5,  10),   S( -3, -11),
            S(  2,   1),   S( -4, -13),   S(  4, -13),   S( -8,  -8),   S(  4,  14),   S(  0,   4),   S( -6,  10),   S(  1,   9),
            S(  0,  -2),   S(  0,  -1),   S( -2, -12),   S(  8,   7),   S(  2,   6),   S(  1,   2),   S(  7,  24),   S(  0,  -4),

            /* knights: bucket 15 */
            S( -1,  -5),   S(  1, -12),   S( -3, -22),   S( -8, -10),   S( -3,   0),   S(  0,   9),   S(  2,   1),   S( -2,  12),
            S( -1,  -2),   S( -3, -18),   S(-11, -33),   S(  0,  10),   S( -2,  10),   S(  1,   8),   S(  1,   0),   S(  0,  -1),
            S( -8, -23),   S(  0,   2),   S( -6, -27),   S(-14, -43),   S( -7, -27),   S( -7, -19),   S( -2,  -1),   S( -3,  -1),
            S( -7, -16),   S( -8, -35),   S(-11, -29),   S( -4, -32),   S(  6,  -3),   S( 14,  29),   S( -5,  -7),   S( -5,  -4),
            S( -1,   3),   S( -4,  -3),   S( -5, -16),   S( -6, -10),   S(  0,  30),   S( 18,  40),   S( -8, -11),   S( -1,  -6),
            S(  3,   8),   S( -3,  -7),   S( -7, -29),   S(  5,  27),   S(-15, -30),   S( -1,   5),   S(  0,  13),   S(  1,   3),
            S( -2, -12),   S(  0,  -1),   S( -4, -16),   S( -3,  -8),   S(-12, -15),   S(  5,  35),   S( -9, -30),   S(  0,   9),
            S( -1,  -9),   S(  2,   0),   S( -5, -11),   S(  2,  -4),   S( -1,  11),   S(-13,  -9),   S( 12,  31),   S( -5,   0),

            /* bishops: bucket 0 */
            S( 47,  35),   S( 35, -23),   S( 44,  13),   S(-19,  40),   S(  3, -10),   S(  9,  -5),   S( 35, -44),   S(-10,   1),
            S( 39, -27),   S( 86,  26),   S( 58, -15),   S( 14,  -7),   S(-19,  40),   S(  1, -35),   S(-29,  -3),   S( 26, -70),
            S(  2,  74),   S( 50,  -2),   S( 38,  18),   S(  9,  55),   S( 13,  27),   S(-26,  49),   S(  7, -45),   S( 12, -35),
            S( 29,   5),   S( 54,   8),   S( 38,  15),   S( 41,  34),   S( 10,  13),   S( 41,  -2),   S( 20, -29),   S(  9,  21),
            S( 16,   5),   S( 34,  19),   S( 20,  38),   S( 43,  21),   S( 63, -11),   S( 26,  -5),   S(  5,   2),   S(-39,  14),
            S(-46,  83),   S(  9,  15),   S( 40,   6),   S( 96, -28),   S( 31,  12),   S( -3,   5),   S(  3,   6),   S( -9,  18),
            S(-42,  36),   S( 11,  13),   S( 22,  -5),   S(-21,  38),   S(-30,  -1),   S( 21,  17),   S( 19,  16),   S(-26,  -7),
            S(  7,  -2),   S( 24, -20),   S( -3,   4),   S( 15,  20),   S(  4,  51),   S( 34,  40),   S( -3,  27),   S( -9,  17),

            /* bishops: bucket 1 */
            S( 65,  41),   S(-31,  60),   S( 19,  32),   S( -4,  23),   S(  3,  24),   S(  0,  24),   S(  4, -26),   S(-28,  32),
            S( 26, -29),   S( 48,  -7),   S( 51,   3),   S( 27,  27),   S( -7,  14),   S( 21,  -9),   S(-26,  20),   S( -4,  18),
            S( 43, -11),   S( 16,   1),   S( 51,  -5),   S( 21,  22),   S( 21,  25),   S(-11,  20),   S( 31, -20),   S(  5, -25),
            S( 36,  11),   S( 22,  17),   S( 20,  -2),   S( 41,  12),   S( 12,  13),   S( 27,  11),   S( 25, -28),   S( 21, -19),
            S( 46,  11),   S(  4,  11),   S(  4,  30),   S(-12,  34),   S( 23,  18),   S( -3,   8),   S( 23, -14),   S( -3,  29),
            S( -4,  11),   S( 17,  54),   S( 12,  47),   S( 58,   1),   S( -2,  30),   S( 53,  -4),   S(  2,  15),   S( 44, -27),
            S( 18,  24),   S( 21,  20),   S(-10,  49),   S( 40,  16),   S( 86, -23),   S(-12,  24),   S( 39,  -5),   S(-32,  37),
            S( 33,  30),   S(-20,   3),   S( -9,  12),   S( -3,   4),   S( 23,  15),   S( 17, -36),   S( -6, -23),   S( 22,  34),

            /* bishops: bucket 2 */
            S( 34,   7),   S( 16,  13),   S( -2,   8),   S(-29,  51),   S(-25,  33),   S(-33,  32),   S(-23,  -3),   S(-26,  27),
            S(-25,  28),   S( 12,   3),   S( 27,   5),   S(  1,  19),   S( -1,  35),   S(  8,   5),   S(  1,   1),   S(  2, -35),
            S(  4,  -4),   S(  1,  14),   S( 19,  20),   S( -2,  44),   S(  8,  30),   S( 11,  35),   S(  6,  12),   S(  2,  -8),
            S( -1,  13),   S(-13,  12),   S(-11,  33),   S( 10,  31),   S(-11,  51),   S(  9,  19),   S( 16,  16),   S(  0,  14),
            S( -4,   4),   S( -7,  23),   S( -8,  18),   S(-40,  48),   S(-11,  37),   S( -4,  36),   S( -1,   3),   S( -4,  11),
            S(  4,  18),   S(  9,   2),   S(-12,  57),   S(-28,  34),   S(  1,  16),   S(-17,  34),   S( 16,  49),   S( 10,  17),
            S(-13,  13),   S(-29,  41),   S(-31,  55),   S( 29,  -9),   S( -8,   5),   S( -9,   5),   S(-54,  25),   S(-16,  38),
            S(-39,  58),   S(-41,  61),   S(-23,  35),   S(-34,  18),   S(-43,  22),   S(-22,   8),   S( 22,  25),   S(-61,  35),

            /* bishops: bucket 3 */
            S( 18,  27),   S( 49, -25),   S( 26,  14),   S( 19,  13),   S(  8,   5),   S( 35,  -9),   S( 33, -22),   S( 64, -79),
            S( 15,  -6),   S( 13,  22),   S( 34, -15),   S( 11,  28),   S( 23,   1),   S( 15,  19),   S( 53,  11),   S( 46, -19),
            S( 23,  10),   S( 13,   7),   S( 19,  33),   S( 24,  11),   S( 24,  46),   S( 34,  27),   S( 32,  22),   S( 49,  -8),
            S( 34, -12),   S(  7,  19),   S( 19,  28),   S( 24,  39),   S( 32,  26),   S( 30,  25),   S( 32,  19),   S( 22,  -4),
            S( 14,   3),   S( 30,   3),   S( 33,  13),   S( 25,  35),   S( 26,  38),   S( 32,  28),   S( 12,  29),   S( 32,  22),
            S( 30, -13),   S( 27,  31),   S( 42,  25),   S( 32,  27),   S( 28,  17),   S( 61,  22),   S( 39,  18),   S( 16,  47),
            S( 22,  -3),   S(-11,  49),   S( 52,   8),   S( 34,  -4),   S(  1,  29),   S( 20,  -3),   S( -9,  63),   S( 22,  33),
            S(-34,  88),   S( -6,  18),   S( 66,   1),   S( 18,  23),   S( 17,  21),   S( -2,  58),   S( 31,   0),   S( 75,  -3),

            /* bishops: bucket 4 */
            S(-26, -25),   S(-51,   3),   S(-57,  22),   S(-38,  22),   S(-19,  28),   S(-56,  15),   S( 28,  -1),   S(-12,   5),
            S( -7,  19),   S( 22,  39),   S( -9,  28),   S(-29,  28),   S(-23, -10),   S( 45,   4),   S(-38,  19),   S( 18,  -8),
            S(-19,  26),   S(-32,  26),   S(-18,  23),   S( -5,   1),   S( -6,  16),   S( 41, -18),   S(-51,   5),   S(-42, -15),
            S( -3,  27),   S(  8,   5),   S( 67,   9),   S(  4,  45),   S(-27,  32),   S( 28,   4),   S( 23, -17),   S(-10, -30),
            S( 13,  12),   S(-21,  65),   S(  1,  53),   S( 36,  31),   S( 53, -17),   S( 20, -22),   S(-17, -33),   S( -7, -10),
            S(-15,  12),   S( -1,  35),   S(-40,  42),   S( 11,  32),   S( 34,   1),   S( 38,  21),   S(  6, -35),   S(  4,  -9),
            S( -5, -10),   S( 49,  32),   S( 14,  -1),   S( 49,   6),   S(  6,  13),   S( 11,  30),   S(  7,  -1),   S( -2, -16),
            S( 23, -17),   S(-33, -45),   S( -4,  13),   S(-26,  -4),   S(  4,  -7),   S( 11,  14),   S(-18, -41),   S(  2,   3),

            /* bishops: bucket 5 */
            S( -3,  26),   S(-20,  63),   S(-25,  16),   S(-11,  20),   S(-69,  32),   S( -2,  -1),   S(-37,  30),   S(-15,  36),
            S(-28,  42),   S(-15,  35),   S(-28,  54),   S(  7,  24),   S(-44,  40),   S(-18,  23),   S(-33,  -3),   S(  1, -11),
            S(-24,  29),   S(-11,  32),   S( 30,  20),   S(-37,  43),   S(-24,  43),   S(-38,  21),   S( -6,  20),   S(-14,   0),
            S( 43,  16),   S( 22,  27),   S(-41,  74),   S( 29,  20),   S( 37,  25),   S( 54,   8),   S(-24,   9),   S(-12,  29),
            S( 34,  43),   S( 54,  11),   S( 43,  33),   S( 64,  26),   S( 49,  21),   S( 36,  11),   S( 57,  -4),   S( 14, -10),
            S( 21,  40),   S( 21,  47),   S( 53,  30),   S( -6,  49),   S(  3,  16),   S( 28,   6),   S(-28,  39),   S(-15,  40),
            S( -6,  43),   S(-11,  13),   S(  7,  25),   S( 28,  21),   S(  7,  20),   S( 13,  47),   S(  4,  40),   S(  3,  18),
            S( 15,  21),   S(  2,  47),   S(  8,   9),   S( 11,  48),   S( -5,  55),   S(  6,  29),   S( 35,  65),   S( -4,  30),

            /* bishops: bucket 6 */
            S(  7,  34),   S(-22,  36),   S(-64,  42),   S(-51,  39),   S(-70,  29),   S(-47,  33),   S(-19,  72),   S( 20,  38),
            S( -4,  -3),   S( 15,   2),   S(-35,  41),   S( -1,  24),   S(-23,  40),   S(-10,  26),   S(-91,  46),   S( 23,  26),
            S( 20,   4),   S( 16,  -4),   S( 33,  13),   S( 20,  27),   S( 29,  26),   S( 28,  28),   S( 23,  16),   S(-13,   3),
            S(  0,  17),   S( 32,   1),   S( 58,   1),   S( 19,  39),   S( 28,  31),   S( 31,  24),   S( 42,  36),   S(-28,  -8),
            S(  2,  21),   S( 55,  -8),   S( 33,  14),   S( 32,  26),   S(108,  20),   S( 58,  20),   S( 19,  30),   S(-16,  47),
            S( 29, -20),   S(-73,  54),   S( 17,  38),   S( 31,  35),   S( 37,  30),   S( 33,  31),   S(-21,  51),   S(-55,  63),
            S(-22,  27),   S(-41,  59),   S(  7,  18),   S(-26,  30),   S( 18,  17),   S( 18,  38),   S( 17,  44),   S(-11,  45),
            S( 17,  60),   S( 22,  28),   S( 21,  40),   S( -4,  34),   S(-20,  43),   S( 11,  21),   S(  9,  30),   S(  7,  22),

            /* bishops: bucket 7 */
            S( 23, -16),   S(-13,   1),   S(-52, -24),   S(-69,  23),   S(-21, -35),   S(-70,  18),   S(-94, -33),   S(-64,   7),
            S(-34, -37),   S(-57, -19),   S(-33,  -4),   S( -2, -21),   S(-51,  12),   S(-50,  11),   S(-48,  -6),   S(-31,  -2),
            S(-34, -10),   S( 10, -22),   S(  2,  -2),   S( 46, -21),   S(-48,  21),   S(-14,   6),   S(-43,  51),   S(-50,  20),
            S(-44,   6),   S( 91, -56),   S( 57,  -7),   S( 43,  21),   S( 63,   0),   S( 15,   1),   S( 23,  37),   S( 17,  40),
            S( 29, -49),   S(-24, -30),   S( 57, -36),   S(120, -50),   S( 37,  39),   S( 58,  31),   S( -2,  32),   S( 34,   5),
            S(-34, -25),   S(  0,   0),   S( 38, -22),   S( 49, -15),   S( 51,  -5),   S( 68,   0),   S( 66,   7),   S( 39,  14),
            S( -6,  -6),   S(-59,  20),   S(  2,   3),   S( 20, -21),   S( 31, -29),   S( 42, -23),   S( 27,  23),   S( 48,  16),
            S(  2,  13),   S(-25,  12),   S(-43,  23),   S( 14, -12),   S(  6, -10),   S( 31, -25),   S( 31,  17),   S( 34,  23),

            /* bishops: bucket 8 */
            S(-12, -36),   S( -8, -35),   S(-52, -28),   S(-20, -43),   S( 12,  51),   S( -2,  15),   S(  2,  24),   S( -7,   0),
            S( -9, -19),   S(-38, -58),   S(-23, -28),   S(-10,  10),   S(  2,  -3),   S(-24, -16),   S(-12, -56),   S(  5,  16),
            S( -8, -27),   S(-27,  14),   S(  5,  35),   S(-22,  17),   S( -1,  -1),   S( -7,  14),   S(  1, -54),   S(-31, -31),
            S( 14,  48),   S( -9,  28),   S( -5,  41),   S(-17,  18),   S( 20,  17),   S(-21,  29),   S(  4, -27),   S( -3,  -6),
            S( -4,  10),   S( 11,  55),   S(-31,  50),   S( 62,  34),   S( -6,  17),   S(  0,  15),   S(  3, -39),   S( -8, -30),
            S(  1,  19),   S( 24,  14),   S( 36,  40),   S( -5, -29),   S( 18,   7),   S(-11, -12),   S(-19,  -9),   S( -9, -12),
            S(  5,   4),   S( 23,  43),   S( 16,  29),   S( -3,  20),   S( -5, -20),   S(  6,  18),   S( -8, -35),   S(-11, -49),
            S(-10, -28),   S( 11, -19),   S( -1, -18),   S( -1, -19),   S(-27, -12),   S( -8,  16),   S( -5,   4),   S( -2,  16),

            /* bishops: bucket 9 */
            S(-28, -12),   S( -5,  -9),   S(-20,  34),   S( -5, -33),   S(-35, -52),   S(-12, -31),   S(-20,  -1),   S( 18,  35),
            S(-16, -24),   S(-28,   3),   S(-21, -18),   S(-29,  -5),   S(-33,  25),   S(-14, -40),   S( -3,  15),   S(  0,   7),
            S( 13,  11),   S(  0,  38),   S(-23,   2),   S(-18,  16),   S( 12,   1),   S( -1,  -2),   S(-17, -14),   S(-17,   6),
            S(-14,  -3),   S( 42,  20),   S(-12,  41),   S(  8,  22),   S( 28,   6),   S(  2, -20),   S(-18, -24),   S(-14, -53),
            S(-11,   0),   S(  8,  30),   S( 13,  19),   S( 20,  24),   S(-37,  20),   S( 16,  11),   S(  5,  36),   S( -6, -21),
            S( -9,  37),   S( 30,  37),   S( 31,  51),   S( 33,  20),   S(  9,  16),   S(  1,  38),   S(-18,  11),   S( -4,   7),
            S(  2,  11),   S( 39,  34),   S( -3,   1),   S( -5,  43),   S( 14,  15),   S(  1,   2),   S( -2,   9),   S( -5,  -4),
            S(  8,  -5),   S(  8,  20),   S( -9,  29),   S(-28,  -7),   S(-22,  -8),   S(  5,   5),   S( -6, -11),   S(-12, -10),

            /* bishops: bucket 10 */
            S(-18,   9),   S(  0, -39),   S(-35, -33),   S(-25,  -7),   S(-16, -41),   S(-28, -20),   S(-15, -60),   S( -9, -22),
            S(  5, -12),   S(-37, -28),   S(-10, -13),   S(-53,   4),   S(-60,  -4),   S(-31,  15),   S(-20, -55),   S(-16, -42),
            S( 26, -15),   S(  5,  -7),   S(-19, -40),   S(-16,   5),   S(-31,  16),   S(-41,  30),   S(-23,   4),   S( 13,  22),
            S(-23, -36),   S( -7,  10),   S(  4, -23),   S( 34, -10),   S( 29,  27),   S(-33,  25),   S(-15,  16),   S( 17,  42),
            S(-15,  15),   S(-15,  -6),   S(-32,  32),   S( 44,   6),   S(-10,  64),   S( 39,  34),   S( 30,  10),   S( -7,  -2),
            S( -9, -32),   S(-24,   5),   S(-14, -11),   S(-31,  19),   S( 33,  19),   S( 37,  58),   S(-10,  46),   S(  8,  12),
            S(-19,   5),   S(  2, -38),   S( -1, -18),   S( 28,  26),   S( -4,   0),   S( -5,  29),   S( 17,  55),   S( 13,  13),
            S( -1, -36),   S( -2,   2),   S(  5,  26),   S(-16, -20),   S(-11,   1),   S(-24, -16),   S( -2,  13),   S(  3,  14),

            /* bishops: bucket 11 */
            S( -7,  17),   S(-40,  13),   S(-43, -35),   S(-17, -28),   S(-13, -14),   S(-66, -69),   S( -4, -27),   S(-21, -29),
            S( -7, -25),   S( -7, -30),   S(-14, -13),   S(-21, -20),   S(-41, -39),   S(-44, -52),   S(-12, -39),   S(-20, -28),
            S( -9, -51),   S(  7, -61),   S(-29,  12),   S(-19,  -5),   S(-24, -30),   S(-22,  25),   S(-25,  11),   S( 14,  22),
            S(-20, -27),   S(-36, -66),   S(  1,   4),   S(-20, -11),   S( 24,  10),   S( 14,  51),   S( 15,  71),   S( 25,  45),
            S( -1, -27),   S(-36, -27),   S(-28,  32),   S( 38,  24),   S( 74,  34),   S( -3,  66),   S( 21,  68),   S( 10,  33),
            S(-26, -51),   S(-32, -27),   S( -7, -26),   S( 16,   4),   S(  6,  21),   S( 18,  54),   S( 40,  49),   S( -7,  -2),
            S( -7,  -1),   S(-18, -21),   S(-31,   0),   S(-16, -17),   S( -9, -28),   S( 30,  -1),   S(  0,   8),   S(  8,  -1),
            S(-20, -21),   S(-22,  -7),   S( -4,  -1),   S( 11, -12),   S( 18,  19),   S(-19, -38),   S( -5,   4),   S(  3,  -5),

            /* bishops: bucket 12 */
            S(  2,  12),   S(-11, -19),   S(-11, -29),   S( -5, -21),   S( -5, -15),   S(-14, -17),   S(  0,  12),   S( -6,   9),
            S( -4,  -1),   S(-15, -48),   S( -6,  16),   S( -1, -12),   S(-19, -35),   S(  3,  32),   S( -3,  -2),   S(  3,  -3),
            S(  1,  -2),   S(-23, -23),   S(  3,  12),   S( -9,   9),   S( -9,   5),   S( -1,  -9),   S( -3, -19),   S(  1,  -1),
            S(  1,   1),   S(  1,  -6),   S(-17, -67),   S( -9,   6),   S(  6,  19),   S(  2,  21),   S( -7,  -2),   S(  1,   2),
            S( -5,  -7),   S( -2,  29),   S(-11,  16),   S(-10,  -4),   S( -4,  10),   S( -1,  -7),   S(  2, -13),   S( -5, -10),
            S(-18,  -3),   S( -8,  40),   S(-14,  11),   S(-18, -11),   S(  1, -24),   S(  4,  10),   S( -3,  -5),   S(  3,   4),
            S(  0,  -8),   S( -2,  21),   S( 18,  43),   S(-17,  21),   S( 11,   8),   S(  7,   7),   S( -8, -15),   S( -6, -11),
            S(  0,   6),   S(  1,  -4),   S( -5, -19),   S( -1,  -9),   S(  4,  12),   S(  2,  14),   S( -7,   0),   S(  0,   0),

            /* bishops: bucket 13 */
            S( -8, -27),   S(-11, -12),   S(-10, -17),   S(-13, -10),   S(-13,  -9),   S(-18,   6),   S( -4, -12),   S( -3,   8),
            S( -4,   0),   S( -6,   2),   S( -8, -22),   S(-21, -23),   S(-14, -11),   S(-11,   7),   S(  7,   2),   S( -4, -10),
            S( -9,  -5),   S( -4, -30),   S(  0,  22),   S(-23,  -6),   S( -7, -24),   S(  3,   3),   S( -6, -24),   S(  2,  20),
            S( -9, -16),   S( -6,  -9),   S(-17, -41),   S(-27,  -9),   S( -4,  26),   S( -1, -17),   S( -4,   6),   S(-14, -24),
            S( -3,  13),   S(-14,  33),   S(-28, -24),   S( 18, -18),   S(  6,  -9),   S( -8,  15),   S( -1,   4),   S( -4, -10),
            S( -1,   6),   S(-18,  -2),   S( 10,  21),   S( 19,   8),   S(  3,  14),   S(  5,   7),   S( 10,  17),   S( -4,  -2),
            S( -4,  -2),   S( -9, -29),   S(  8,  37),   S( -5,  19),   S(  1,  17),   S(  3,  -4),   S(-11, -22),   S(  1,  17),
            S( -7, -21),   S(  5,   6),   S( -7,  -8),   S(  3, -12),   S( -2,   0),   S(-10, -11),   S( -1,  14),   S( -1, -15),

            /* bishops: bucket 14 */
            S( -4, -18),   S(-17, -30),   S( -8,   3),   S(-12, -35),   S( -9, -32),   S( -7, -21),   S( -8, -15),   S(-10, -32),
            S(  0, -12),   S( -6, -41),   S(-10, -11),   S(-16, -10),   S(-24, -21),   S(-21, -33),   S(-16, -17),   S(  4,  -7),
            S(-11, -26),   S(-15, -24),   S(-17,   6),   S( -6,  -9),   S(-22, -17),   S(-20, -52),   S( -6,   6),   S(  0,   5),
            S( -8, -19),   S( -9,   4),   S(  0,  17),   S(-18,  46),   S( 12,   4),   S(-20,   7),   S(-17,   7),   S( -8, -12),
            S( -8,   8),   S(-12,  35),   S( -9, -29),   S(  3,   2),   S(-15,   0),   S( -5, -56),   S( -2, -18),   S( -2, -13),
            S( -1,  12),   S(-13,   7),   S(-14,  -7),   S(-22, -21),   S( 17,  -1),   S( -4,  22),   S( -6,  26),   S( -6,  -4),
            S( -8, -19),   S( -2,   1),   S(-14,   2),   S(  4,  14),   S(-16,  -9),   S( -3, -21),   S( -3,  -7),   S( -4,  -6),
            S( -1,   3),   S( -2, -13),   S( -2, -15),   S( -7,   0),   S( -5, -12),   S(  2,  14),   S(  1,  -3),   S( -1,  -1),

            /* bishops: bucket 15 */
            S( 12,  34),   S(  7,   0),   S(-16, -21),   S( -4, -21),   S(-12, -31),   S(-12, -15),   S( -3,   1),   S( -1, -11),
            S(  6,   9),   S(  4,   9),   S(  9,  10),   S(-11, -44),   S(-18, -25),   S(-10, -22),   S( -1,  -4),   S( -3,   4),
            S( -7, -31),   S( -5, -22),   S( -8,   5),   S(-12, -17),   S(-15,   0),   S(-20,  -7),   S( -9, -14),   S(  2,  11),
            S( -3,   3),   S(-18, -32),   S( 10,   4),   S(-20, -39),   S( -4,  11),   S( -5, -25),   S(  9,  25),   S( -3, -11),
            S( -3, -14),   S( -9,  -3),   S(-11,  -7),   S(-24, -61),   S(-10, -40),   S( -8,  34),   S(  0,  24),   S(-10, -13),
            S(-11, -37),   S(-14, -11),   S(-19, -37),   S(-28, -18),   S(-11, -19),   S(-12, -17),   S( 11,  39),   S( -3,   4),
            S( -7,  -6),   S(  3, -18),   S(  1,   4),   S( -7,   3),   S( -7, -19),   S( -5, -18),   S( -7,  33),   S(  2,   2),
            S( -7, -16),   S(  0,   5),   S( -3,  10),   S( -8, -11),   S(-16,  -8),   S(-13, -13),   S(-12, -35),   S(  2,   0),

            /* rooks: bucket 0 */
            S(-25,   3),   S(-21,  -2),   S(-11,  -9),   S(-12,   1),   S(-16,  15),   S( -9,  -2),   S(-13,  24),   S( -3,  16),
            S( 57, -76),   S( 12, -17),   S(  3, -10),   S( -7,   5),   S( 17,  -3),   S(  8, -17),   S(-31,  24),   S(-49,  41),
            S( 14, -17),   S( -4,  57),   S( 21,   5),   S(  8,   9),   S(-15,  43),   S( -8,  16),   S(-19,  23),   S(-53,   5),
            S( 29,  -3),   S( 82, -38),   S( 49,  22),   S( 50,   5),   S( -3,  26),   S( -7,  19),   S(-21,  17),   S(-36,  21),
            S( 60, -27),   S( 76, -16),   S( 69,  -2),   S( 32,   1),   S( 18,  37),   S( 43,  -9),   S( 27,  18),   S(-14,  28),
            S( 89, -77),   S( 78, -48),   S( 46,  16),   S( -1,  33),   S( 64,  26),   S(-28,  13),   S( 44,  20),   S(-61,  58),
            S( 48, -20),   S( 80,  -5),   S( 31,   1),   S( 29,  30),   S(-10,  30),   S( 23,   3),   S( 14,  14),   S(-44,  30),
            S( 16,  19),   S( 14,  61),   S( 30,  17),   S(-23,  46),   S( 20,   8),   S(  9, -18),   S(  5,  24),   S( 17,  35),

            /* rooks: bucket 1 */
            S(-80,  39),   S(-44,   1),   S(-73,  11),   S(-49,  -5),   S(-28, -32),   S(-23, -20),   S(-38,  -4),   S(-37,  19),
            S(-30,  17),   S(-64,  26),   S(-15, -11),   S(-30, -33),   S(-10, -36),   S(-47, -10),   S(-35, -29),   S(-63,  24),
            S(  1,   4),   S(-21,  42),   S(-39,  35),   S(-53,  23),   S(-43,  33),   S(-24,  13),   S(-23,  13),   S(-52,  31),
            S(-62,  53),   S(-76,  62),   S(  1,  26),   S(-13,  16),   S(-30,  43),   S(-38,  39),   S(-27,  42),   S(-29,  23),
            S( 71,   1),   S( 35,  38),   S( 38,   6),   S(-41,  42),   S(-29,  40),   S(  1,  20),   S( -1,  26),   S(-42,  13),
            S( 71,  -1),   S(  6,  45),   S( 28,  25),   S(-40,  33),   S(  8,   8),   S(-48,  62),   S( 22,   4),   S(-65,  59),
            S(-19,  40),   S(-11,  36),   S( 52,   3),   S(-69,  68),   S(-12,  28),   S( -3,  47),   S(-37,  36),   S(-45,  37),
            S( 14,  34),   S( 38,  45),   S(  7,  25),   S(-46,  50),   S(  7,  13),   S( 23,  16),   S(-11,  29),   S(-27,  31),

            /* rooks: bucket 2 */
            S(-71,  43),   S(-52,  28),   S(-45,  12),   S(-60,  20),   S(-60,  12),   S(-41,   2),   S(-27, -22),   S(-47,  27),
            S(-71,  49),   S(-70,  43),   S(-47,  24),   S(-62,  18),   S(-35,  -5),   S(-42,   5),   S(-56,  11),   S(-56,  15),
            S(-77,  77),   S(-57,  56),   S(-56,  63),   S(-39,   8),   S(-55,  42),   S(-18,  23),   S(-11,   5),   S(-42,  40),
            S(-80,  71),   S(-74,  71),   S(-33,  59),   S(-27,  46),   S(-24,  41),   S( -5,  37),   S(-49,  59),   S(-25,  36),
            S(-19,  45),   S(-53,  80),   S(-45,  62),   S(-33,  46),   S( 41,  14),   S( 16,  37),   S(-11,  47),   S(-12,  39),
            S(-43,  53),   S(-28,  51),   S(  7,  21),   S(  7,  19),   S(  8,  28),   S( 16,  34),   S(-16,  39),   S(-34,  31),
            S(-44,  35),   S(-53,  62),   S(-41,  54),   S(-40,  66),   S( 24,  26),   S( 31,  17),   S(-56,  70),   S(-45,  51),
            S(-39,  69),   S( -1,  42),   S(-61,  60),   S(-29,  47),   S(-63,  76),   S( -5,  65),   S(-31,  78),   S( -3,  43),

            /* rooks: bucket 3 */
            S( -9,  74),   S( -9,  69),   S(  0,  54),   S(  4,  47),   S(  1,  42),   S(-19,  72),   S(-10,  82),   S( -2,  32),
            S(-36, 100),   S(-11,  72),   S(  3,  51),   S(  7,  51),   S( 19,  42),   S(  8,  60),   S( 41,   1),   S( 29, -37),
            S(-33,  83),   S(-12,  82),   S(  2,  73),   S(  7,  49),   S(  5,  74),   S( 28,  62),   S( 29,  58),   S( -3,  56),
            S(-23,  80),   S(-27,  82),   S( 20,  67),   S( 24,  63),   S( 26,  75),   S( 11,  97),   S( 52,  74),   S( 20,  67),
            S(-16,  97),   S( 30,  82),   S( 19,  69),   S( 42,  58),   S( 43,  69),   S( 49,  63),   S( 92,  55),   S( 67,  38),
            S(-17,  92),   S( 15,  72),   S( 26,  64),   S( 11,  67),   S( 25,  49),   S( 30,  54),   S( 79,  37),   S(117,   8),
            S(-29,  92),   S(-17,  99),   S(-13,  88),   S( 28,  74),   S( 20,  72),   S( 51,  66),   S( 71,  67),   S( 94,  35),
            S(-68, 157),   S(-19, 111),   S( 11,  77),   S( 43,  60),   S( 44,  68),   S( 54,  70),   S(105,  51),   S(113,  49),

            /* rooks: bucket 4 */
            S(-82,  13),   S(-26,  12),   S(-44,  -6),   S(-49,  27),   S(-36, -14),   S( 19, -74),   S(-22, -20),   S(  4, -54),
            S(-32,   1),   S(-47,  14),   S(-50,  18),   S(-28,  16),   S(-39, -12),   S(-28,  -3),   S( 15, -30),   S(-33, -33),
            S( 20,  17),   S(-24, -43),   S(-12,   3),   S(-21,  -5),   S(  5,  -3),   S( -4, -23),   S( 29,  -1),   S(-45,   7),
            S(-49,  -9),   S( 12,   0),   S(-38,  11),   S( 18,   7),   S( 20,  26),   S( 49,   6),   S(-13,  29),   S( 29,  -4),
            S(  5, -13),   S( 13,  43),   S(  4,  13),   S( 55,  25),   S( 34,  -3),   S(-13,  47),   S( 38,  37),   S( 35,   9),
            S( 21,  -4),   S( 33,   5),   S( 22,  22),   S( 30,  28),   S( 29,  14),   S(  7,  15),   S( -1,   3),   S( 13,  33),
            S(-10,  -4),   S( 29,  39),   S( 23,  51),   S( 44,  22),   S( 55,  -5),   S( -5,   1),   S( 58,  32),   S( 74,   3),
            S( 46, -71),   S( 42,  53),   S(  6,  31),   S(  6,  12),   S( 21, -10),   S(  4,  22),   S( 16,  41),   S( 22,   1),

            /* rooks: bucket 5 */
            S(-64,  42),   S(-70,  61),   S(-62,  48),   S(-50,  26),   S(-43,  22),   S(-33,  39),   S(-28,  39),   S(-54,  45),
            S(-33,  39),   S(-63,  41),   S(-88,  59),   S(-56,  30),   S(-49,  17),   S(-16,  -5),   S(-11,  10),   S(-42,  28),
            S(-45,  56),   S(-36,  52),   S(-51,  67),   S(-41,  35),   S(-38,  37),   S(  8,  20),   S( -5,  33),   S(-10,  44),
            S(-39,  82),   S( 48,  19),   S(-54,  71),   S( -7,  56),   S( -4,  55),   S(  3,  57),   S( 17,  40),   S( -4,  36),
            S(  1,  64),   S( 13,  69),   S( 31,  65),   S( 46,  44),   S( 30,  45),   S( 37,  59),   S( 51,  73),   S( 17,  41),
            S( 57,  60),   S( 39,  70),   S( 45,  54),   S( 44,  67),   S( 63,  35),   S( 43,  52),   S( 59,  45),   S( 41,  43),
            S( 18,  50),   S( 23,  54),   S( 58,  39),   S( 62,  50),   S( 42,  57),   S( 63,  32),   S( 53,  46),   S( 85,  32),
            S( 85,  27),   S(101,  13),   S( 21,  74),   S( 20,  38),   S( 41,  38),   S( 53,  41),   S( 51,  29),   S(  8,  55),

            /* rooks: bucket 6 */
            S(-66,  32),   S(-30,  25),   S(-37,  29),   S(-25,  10),   S(-57,  24),   S(-85,  67),   S(-75,  68),   S(-35,  44),
            S(-46,  44),   S(-30,  34),   S(-26,  32),   S(-42,  23),   S(-67,  48),   S(-62,  54),   S(-93,  65),   S( 18,  25),
            S(-55,  63),   S(-51,  57),   S(  1,  37),   S(-54,  42),   S( -1,  36),   S(-59,  54),   S(-30,  62),   S( 37,  24),
            S(-63,  82),   S( 23,  43),   S(-31,  71),   S( 25,  37),   S( 14,  43),   S( -8,  71),   S(-23,  62),   S(-19,  53),
            S(-29,  84),   S( 34,  68),   S( 50,  50),   S( 43,  34),   S( 41,  47),   S( 61,  50),   S( 42,  41),   S(  0,  64),
            S(-37,  82),   S( 44,  59),   S( 93,  32),   S( 51,  27),   S( 40,  37),   S( 67,  47),   S( 37,  68),   S( 53,  64),
            S( 55,  50),   S( 69,  44),   S( 83,  30),   S( 53,  31),   S(106,  23),   S( 55,  53),   S( 50,  51),   S( 39,  45),
            S( 40,  81),   S( 41,  54),   S( 43,  53),   S( 24,  44),   S( 78,  37),   S( 75,  54),   S( 61,  66),   S(  1,  64),

            /* rooks: bucket 7 */
            S(-88,  -8),   S(-67,   4),   S(-66,   1),   S(-31, -12),   S(-12, -17),   S(-61,  37),   S(-36,   3),   S(-13, -20),
            S(-95,  40),   S(-47,  12),   S(-48,   6),   S(-23,  -9),   S(-35,   8),   S(-30,  34),   S(-30,   8),   S(-46,  17),
            S(-119,  66),  S(-61,  36),   S( -9,   3),   S(-17, -14),   S( -7,   4),   S(-22,  -7),   S( 17, -22),   S( 32,   7),
            S(-46,  35),   S( 20,   8),   S( 29,   5),   S( 40,  -2),   S( 41,  -2),   S( 56,  -1),   S( 53,  -8),   S( -8,   1),
            S(-19,  32),   S( -3,  25),   S( 68, -13),   S( 59, -14),   S( 90,  -2),   S( 86,   5),   S( 54,  21),   S( 21,   5),
            S(-11,  36),   S(  5,  14),   S( 87, -20),   S(105, -29),   S( 65,  -7),   S( 68,  14),   S(102,  25),   S( 36, -10),
            S(-14,  33),   S( 11,  16),   S( 31,  -3),   S( 84,  -2),   S( 88, -10),   S( 94,  10),   S( 59,  19),   S( 38,   8),
            S(  3,  67),   S(-32,  45),   S( 39,   4),   S(101, -28),   S(  5,  25),   S(-11,  28),   S( 36,  12),   S( 64,   6),

            /* rooks: bucket 8 */
            S(-13, -39),   S(-18,  22),   S( -5,  23),   S(-17,  -7),   S(-30, -41),   S(-16, -79),   S(-18, -31),   S( -7, -29),
            S( -7, -23),   S( -8, -15),   S( -1,  -6),   S( 14,  -8),   S( 12,   8),   S(  0, -13),   S(  6, -35),   S(-10, -80),
            S(  7,  25),   S(  3, -15),   S(  6, -31),   S( 12,  37),   S(-15, -19),   S(-23, -58),   S( -8,  14),   S( 11, -14),
            S(-12, -35),   S( -9,  33),   S(  3,   2),   S( 15,  -1),   S(  1,  -2),   S(-16, -33),   S(  5, -14),   S(  1, -10),
            S( -6,   4),   S( -4, -17),   S( 19,  27),   S( 20,  10),   S( -6,   1),   S( 25,   8),   S( 19,   1),   S(  6, -36),
            S(  9,  48),   S(  1,  19),   S( 22,  58),   S( 32, -14),   S(  8,   6),   S( 11, -24),   S(  0,  21),   S( 18,  46),
            S(  3,  19),   S( 16,  12),   S( 41,  -5),   S( 17, -16),   S( 14, -34),   S( 30, -23),   S( 38, -14),   S(  7,  -9),
            S( 15, -150),  S( 17, -14),   S( 18,  -2),   S(  8,  -7),   S( -7, -12),   S( 17, -23),   S( 17, -18),   S( 30,  16),

            /* rooks: bucket 9 */
            S(-37, -22),   S( -5, -21),   S(-19,  -9),   S(-29,  29),   S(-47,  14),   S( -7, -24),   S( 15, -50),   S(-49, -25),
            S( 35,  -1),   S( -5,  -8),   S(-29, -11),   S(-25,  -9),   S( -4,   0),   S( 39, -15),   S(  0, -58),   S( -7, -28),
            S(  8,  -5),   S( 11,  -9),   S( 18,   7),   S( -6,  -1),   S(-16, -17),   S( 24, -20),   S( 15,  34),   S( -1,   1),
            S(-13,  20),   S( 11,  12),   S( 10,  19),   S(  4,  -8),   S(  5,   8),   S( 25,   2),   S( 13,  34),   S(  4,  20),
            S( 17, -20),   S( -7,   9),   S(  2,  17),   S( 23,   6),   S( 28,  -3),   S( 22,  30),   S( 19,   5),   S(  8, -21),
            S( 15,  50),   S( -5,   7),   S(  4,  14),   S( -3,  30),   S( 11,  23),   S( 29,  15),   S( -2,   2),   S( 21,  12),
            S( 69,   7),   S( 62,  11),   S( 45,  31),   S( 51,   3),   S( 13, -11),   S( 39,   5),   S( 34,  16),   S( 56,  25),
            S( 73, -76),   S( 37, -40),   S( 28,   8),   S( 38,  45),   S( 11,  62),   S( 31,  24),   S( 26,  38),   S( 41, -14),

            /* rooks: bucket 10 */
            S(-30, -106),  S( -7, -62),   S(-25, -16),   S(-35,  -7),   S(-18,   2),   S(-55,   9),   S(  7,  -8),   S(-32,   7),
            S(  6, -39),   S(  6, -39),   S(  8, -36),   S( -3, -18),   S(  2,  -4),   S( -4,   1),   S( 31,   4),   S(  1,  -6),
            S(-28, -33),   S(-22,  -3),   S( -5, -20),   S( 13,  37),   S(-54,  11),   S(-25,  13),   S( 20,  13),   S( 33,  -5),
            S( -7,   3),   S( -5,  -3),   S(  8,  -1),   S( 17,  12),   S( 23, -24),   S(  8, -16),   S( 33,  38),   S(-13,  -3),
            S( 23,   2),   S( 32,   2),   S( 13,  16),   S( 34, -28),   S(-12,  22),   S( 16,   8),   S( 22,  58),   S( 14,  31),
            S( 49,  30),   S( 41,  50),   S( 22,  50),   S( 24,  17),   S( -5, -21),   S( 32,   7),   S( 44,  32),   S(  7,  39),
            S( 52,  15),   S( 79,   4),   S( 78, -12),   S( 72, -44),   S( 48, -25),   S( 41,   5),   S( 13,  10),   S( 18,   2),
            S( 60,  10),   S(  1,   1),   S( 37,  11),   S( 34,  15),   S( 44,   5),   S( 44,  32),   S(  1,  -3),   S( 21, -16),

            /* rooks: bucket 11 */
            S(-31, -44),   S(-31, -17),   S( -3, -38),   S(-45, -52),   S( -3,   4),   S( -8,   4),   S(-20, -38),   S(-73, -35),
            S(-15, -37),   S(  0, -37),   S(  1, -19),   S( -4, -45),   S(-21, -22),   S( -4,  -3),   S( 12,  -4),   S( -9,  18),
            S(  5, -36),   S( 26, -21),   S(  9, -24),   S(-13, -12),   S(  4,  -1),   S( -8,  29),   S(-14, -45),   S(-15, -71),
            S( -3,  51),   S(  9,  -1),   S( -9,  29),   S( 10,  13),   S( -5,   0),   S( -2,  26),   S( 26,  -1),   S( 15, -18),
            S(  3,  19),   S( 14,  -6),   S( 19,   9),   S( 30,   1),   S( 37,   3),   S( 24,  -6),   S( 12, -11),   S( -7,  -2),
            S( 57,  32),   S( 41,   5),   S( 22, -45),   S( 60,  -3),   S( 56,   8),   S( 37,   5),   S(  6,  -8),   S( 33,  10),
            S( 51,  56),   S( 74,   5),   S( 91,  -7),   S( 67,  -5),   S( 69,   8),   S( 36,  27),   S( 17,  10),   S( 77,  16),
            S( 53,  44),   S( 12,  17),   S( 16, -19),   S( 13,  12),   S( -3, -13),   S( 33,   5),   S( 13,  -2),   S( 42,  -5),

            /* rooks: bucket 12 */
            S(  6,   8),   S(-25, -44),   S(-16, -42),   S(-18, -13),   S(-16, -20),   S( -6, -42),   S(-31, -75),   S(-20, -40),
            S(  7,   7),   S(  8, -13),   S(-22,  -8),   S(-11,  -5),   S(-17,   3),   S(  2,  -5),   S(  2,  -9),   S( -4, -17),
            S(  8, -15),   S( -7, -16),   S(  0,  -8),   S(  2,   4),   S(-10, -38),   S( 10,   6),   S(  2,  11),   S(  5,  -5),
            S(-10, -17),   S( -2, -11),   S(  5,  10),   S(  9,  -8),   S( -1, -18),   S( -7, -35),   S(-16, -19),   S(  4, -24),
            S(-11, -13),   S( -1, -24),   S(  7,  -2),   S( 15,   8),   S( -6, -42),   S(  3, -11),   S(-13, -20),   S( -4, -11),
            S(  1,  -6),   S( -3, -21),   S( 14,  20),   S(  8,  -4),   S(  2,   6),   S(-12, -44),   S(  6, -11),   S( -3,  -3),
            S(  3,  19),   S(-18, -49),   S(-11, -54),   S( 18, -11),   S(  9,  22),   S( -8, -53),   S( -4, -28),   S(  8,  -1),
            S( -9, -42),   S( 11,  28),   S( 12,  -8),   S(  2,  16),   S(  1, -11),   S(-11, -34),   S(-25, -63),   S(  3, -13),

            /* rooks: bucket 13 */
            S(-20, -41),   S(-14, -35),   S( -3, -11),   S( -6,  -2),   S( -2, -13),   S(-25, -58),   S( -3, -25),   S(-17, -23),
            S(-22, -53),   S(  5, -18),   S(  1,   0),   S(  4,  25),   S( -8,  -9),   S(-11, -48),   S(  8,  -1),   S( -3, -19),
            S( -6, -46),   S(-15, -41),   S( -1, -31),   S( -5, -51),   S(  5,   0),   S(  0,  -3),   S( -6, -50),   S( -7, -47),
            S( -6, -43),   S( 14,  20),   S(-15, -34),   S( -6,  -2),   S(  9,  -8),   S(-18, -52),   S( 12, -12),   S( -2, -32),
            S( 18, -25),   S(  4, -51),   S( 18,  36),   S( -9, -21),   S(-24, -38),   S( -1,  -8),   S(-12, -33),   S( 18,   9),
            S(-17, -52),   S(  3, -40),   S(-14,  -3),   S( 16,   8),   S(  0, -17),   S( 16,  32),   S( 11,   2),   S( -4,  12),
            S(  7,  -4),   S( 12,  29),   S( 23,  33),   S( 16,   6),   S( 11, -21),   S( 36,   8),   S(-10,  -6),   S( 10, -17),
            S(-14, -114),  S(-26, -74),   S(  8,   5),   S( -5, -11),   S( -4,  -1),   S(  5, -27),   S( -7,  -1),   S( 12,   4),

            /* rooks: bucket 14 */
            S( -3, -17),   S(-13, -52),   S( -2,   7),   S(  2, -26),   S( -6, -11),   S(  9,   0),   S( -2, -40),   S(-10, -21),
            S(-19, -35),   S(-30, -84),   S( -8,   8),   S(-15, -23),   S(-13,  -5),   S(-12, -47),   S( 10,  31),   S( -4, -13),
            S(  8,  -9),   S( -5, -27),   S( -4, -37),   S(-15, -29),   S(-24, -57),   S( -7, -14),   S(  9,  16),   S(  8,  13),
            S(  4,  -6),   S( -4, -17),   S(  0, -15),   S(-19,  -1),   S( -5, -38),   S(  9,   4),   S(-15, -63),   S( -1,  -3),
            S(  8,   7),   S(  6, -33),   S( -3, -15),   S( -4, -20),   S(  0,   7),   S( -9, -18),   S(  3, -12),   S(  4, -17),
            S( -1, -25),   S(  6, -13),   S(  3, -14),   S(-22, -35),   S(-23, -39),   S(-12,   7),   S(  3,  -1),   S( -4,  -9),
            S( 28,  20),   S(  6, -34),   S(  3, -10),   S(  3, -27),   S(  6, -27),   S(  8,  14),   S( 11,  33),   S( 10,  17),
            S(  6, -22),   S(  6, -29),   S(-13, -38),   S(  0, -24),   S( -8, -26),   S(  6,  11),   S(  4,  17),   S(  3, -14),

            /* rooks: bucket 15 */
            S(  0, -40),   S(-11, -40),   S(-10, -53),   S( -9, -40),   S(  3, -13),   S( -7, -10),   S( -5, -33),   S( -9,  -9),
            S(-12, -21),   S( -9,  -8),   S(  2,   3),   S(  0, -25),   S(-11, -36),   S( -1, -27),   S(-15, -32),   S(  2, -20),
            S(-10, -26),   S(-14, -24),   S( -4, -28),   S( -2,   1),   S( 14, -14),   S( -6,   1),   S(-10,   6),   S(-12, -27),
            S(  3, -20),   S(  2,  -4),   S( -5,   7),   S( -9, -29),   S(-20, -35),   S( -2, -19),   S(  6, -16),   S( -9,   7),
            S(  0,   0),   S(-20, -32),   S(  3, -23),   S( -5, -18),   S( -4, -17),   S(  9,  19),   S( -7,  -6),   S(  1,   6),
            S(  2,   9),   S(  4,   0),   S(  1,   1),   S( -1, -14),   S( -1,  12),   S( 10,  -6),   S( 12, -24),   S(-15, -17),
            S( 14,  43),   S(  7,  -6),   S( 12, -72),   S( -2, -29),   S( -1, -30),   S(  7,  38),   S(  0,  -3),   S( -4,   8),
            S( -2, -21),   S(-14, -23),   S(  7,   2),   S(  9,   3),   S(-11,  -3),   S(  5, -16),   S(  1, -36),   S(  0, -20),

            /* queens: bucket 0 */
            S(-28, -14),   S(-31, -49),   S( 46, -89),   S( 65, -65),   S( 31, -73),   S( 28, -37),   S( 76,  14),   S( 36,  24),
            S(-17, -22),   S( 36, -37),   S( 31,   0),   S( 24,  -3),   S( 21,  27),   S( 10,  24),   S( -1,  65),   S( 48,   6),
            S( 25,  43),   S( 27,  42),   S( 49, -10),   S( 21,  31),   S( 18,  33),   S( 11,  32),   S( 16,   8),   S( 40,  20),
            S( 17,  13),   S( 32,  52),   S(-11,  89),   S( 17,  49),   S(  9,  45),   S( 26,  16),   S( 12,  39),   S(  4,  33),
            S( 39,  40),   S( 20,  22),   S( 13,  51),   S(  9,  87),   S( -1,  27),   S( 10, -30),   S( 15,  44),   S( 38,   3),
            S( 27,  44),   S( 13,  66),   S( 12,  38),   S( 18,  20),   S( 47, -44),   S(  5,  34),   S( 14,  29),   S(  5,   1),
            S( 47,  34),   S( 61,  39),   S( 21,  34),   S( 46,  15),   S( 21,  13),   S(-29, -20),   S( 22,  51),   S( 16,  11),
            S(105,  33),   S( 20,  39),   S( 29,  35),   S( 34,  21),   S( 46,  42),   S(-56,  -5),   S( 66,  29),   S( 53,  31),

            /* queens: bucket 1 */
            S(  4, -32),   S(-68,   1),   S(-41, -28),   S(-33, -65),   S( -3, -22),   S( -7, -84),   S(  3, -20),   S(  0,  30),
            S(-13, -47),   S(-20, -48),   S( 10, -52),   S( -5,   7),   S( -7,   5),   S( -3,  16),   S( 30, -59),   S( -7,  27),
            S(-30,  42),   S(-11,   1),   S( 10,  19),   S( -7,  21),   S( -3,  45),   S(-23,  41),   S( 12,   7),   S( 15,  33),
            S(  8,   4),   S(-10,  28),   S( -2,  30),   S( 21,  42),   S(  3,  52),   S(  7,  55),   S(  7,  -2),   S(  3,  36),
            S( 27, -24),   S(  4,   5),   S( -2,  60),   S( -1,  51),   S( -8,  65),   S( -7,  16),   S( -8,  25),   S( 20,  48),
            S( 25,  38),   S(  4,  69),   S( 16,  61),   S(-46,  45),   S(-38,  58),   S(-40,  44),   S( 38,  16),   S( 32,  34),
            S(-10,  15),   S(-19,  80),   S(-10,  16),   S(-30,  45),   S(-40,  41),   S( 12,  30),   S( 13,  19),   S(-38,  29),
            S(-18,  -3),   S( 34,  20),   S( 13,  36),   S( -8,  34),   S(-13,  16),   S(-11,   2),   S(  2,  18),   S( 13,  43),

            /* queens: bucket 2 */
            S( -1,  44),   S( 39, -57),   S(  6, -16),   S( -6,  -6),   S(-37,  43),   S(-38,  21),   S(-10, -38),   S( 22,  14),
            S(  2,  33),   S( 15,  27),   S( 20, -22),   S( 19, -32),   S( 16, -35),   S( 13, -38),   S( 11, -28),   S( 32, -31),
            S( 11,  24),   S( 17,   9),   S(  9,  52),   S(  4,  31),   S(  7,  47),   S( 13,  59),   S( 13,   2),   S( 24,  19),
            S( 10,   9),   S( -7,  62),   S(  4,  42),   S(  3,  50),   S(-16,  93),   S( -7, 100),   S( 15,  10),   S( 11,  36),
            S( 15,  12),   S( -5,  37),   S(-14,  50),   S(-47, 118),   S(-28,  87),   S(-21,  70),   S(-15,  88),   S( -1, 115),
            S( 15,  21),   S( 29,  20),   S(-18,  62),   S( -8,  46),   S(-39,  90),   S(-27, 117),   S( -9,  73),   S( 11,  73),
            S(-28,  71),   S(-35,  80),   S(-32,  59),   S( 16,  61),   S(-29,  80),   S( 40,  41),   S( -7,  44),   S(-35,  96),
            S(-81, 100),   S(-11,  44),   S( 45,  35),   S( 45,  24),   S( 28,  69),   S( 26,  31),   S(-14,  62),   S(  6,  22),

            /* queens: bucket 3 */
            S( 90,  97),   S( 71,  89),   S( 53,  97),   S( 40,  79),   S( 60,  32),   S( 43,  31),   S( 19,  38),   S( 46,  53),
            S( 64, 117),   S( 63, 104),   S( 45, 116),   S( 46,  91),   S( 46,  76),   S( 55,  52),   S( 61,  16),   S( 48,  61),
            S( 61,  97),   S( 55, 104),   S( 55,  99),   S( 50,  75),   S( 58,  89),   S( 54,  94),   S( 60,  92),   S( 59,  73),
            S( 47, 125),   S( 61,  76),   S( 59,  95),   S( 47,  86),   S( 36, 105),   S( 43, 122),   S( 65,  94),   S( 43, 150),
            S( 60,  97),   S( 57,  98),   S( 59,  85),   S( 35,  94),   S( 37, 113),   S( 33, 125),   S( 40, 169),   S( 61, 157),
            S( 51, 125),   S( 58,  97),   S( 60,  86),   S( 16, 120),   S( 27, 148),   S( 94,  84),   S( 59, 137),   S( 26, 204),
            S( 64, 112),   S( 51, 119),   S( 73,  77),   S( 72,  93),   S( 42, 103),   S( 70,  94),   S(100, 127),   S(147,  61),
            S( 76,  87),   S(113,  74),   S( 82,  73),   S( 84,  83),   S( 44, 101),   S(108,  64),   S(147,  43),   S(135,  64),

            /* queens: bucket 4 */
            S(-26, -28),   S(-15,   2),   S(-10,   9),   S(  4, -12),   S( 22, -13),   S( 48,  12),   S(-62, -19),   S(-39, -15),
            S(-16,  -1),   S(-49, -14),   S(-17,  18),   S( -8,  16),   S( 12, -14),   S( 30, -23),   S( -7, -37),   S(-63, -28),
            S( 18,  17),   S( 11, -30),   S( 20,  29),   S(  1,  34),   S(  0,   9),   S( 14,   6),   S( 13,   0),   S(-15, -24),
            S(-24,  -7),   S( 23,  29),   S(-19,  41),   S(-17,  34),   S( 16,  26),   S( 24,  18),   S( 12,   0),   S(  9,   8),
            S( 14,   9),   S( 46,  40),   S( -1,  49),   S( 42,  47),   S(-12,  26),   S( 44,  -6),   S( -3,  18),   S( -1, -34),
            S(  1,   8),   S( 13,   9),   S( 21,  75),   S( 35,  40),   S( 21, -12),   S( -1,  14),   S(-35, -20),   S(-26, -15),
            S(-29, -18),   S(  6,  30),   S(-14,  21),   S( 19,  36),   S(  1,  -2),   S(-29, -26),   S(-24, -39),   S(-27, -30),
            S(  5, -13),   S( -6, -13),   S( 39,  55),   S( -4,  22),   S(-25,   4),   S(-21,  -5),   S(-17, -10),   S(-12, -24),

            /* queens: bucket 5 */
            S(-30, -16),   S(-29, -29),   S(-18, -17),   S(-36, -30),   S(-66, -19),   S( 40,  -7),   S(-33, -16),   S(-14, -12),
            S(-19,   2),   S(-34, -24),   S(-81, -37),   S(-80, -21),   S(-19,  -4),   S(-21, -13),   S(-73, -30),   S(-73, -37),
            S(-11,  23),   S(-54, -39),   S(-62,  -2),   S(-19,  64),   S( 17,  74),   S(-13,  24),   S(-45, -19),   S( 17,  31),
            S(-58, -13),   S(-70, -29),   S(-30,  60),   S(  2,  62),   S( 27,  42),   S(-45,  24),   S( 10, -13),   S(  3,   3),
            S(-45,  -3),   S(-15,  25),   S(-36,  32),   S( 10,  30),   S( 26,  54),   S( 14,  26),   S( -3,  -9),   S(-31, -19),
            S(  1,  10),   S( 28,  62),   S(-14,  49),   S( 46,  46),   S(  7,  23),   S(  3,   4),   S( -5,   8),   S(-12, -21),
            S( -2,  15),   S(  2,  16),   S( 18,  80),   S( -9,  48),   S( 24,  51),   S( 28,  28),   S(  4,  11),   S( -8,  -1),
            S( 26,  73),   S(  9,  -9),   S(  5,  29),   S( 15,  70),   S( 14,  36),   S(  9,  33),   S(  9, -24),   S(-11,  -1),

            /* queens: bucket 6 */
            S(-51, -11),   S(-56, -36),   S(-65, -34),   S(-91, -36),   S(-65, -41),   S(-70, -49),   S(-61, -25),   S(-51,  -6),
            S(-57, -18),   S(-61,   8),   S(-65,   2),   S(-23,   0),   S(-105,  16),  S(-103, -20),  S(-85, -15),   S(-20,  14),
            S(-48,  -4),   S( -9,  20),   S(-75,  47),   S(-96,  69),   S(-52,  62),   S(-36,  18),   S(-36,  -3),   S(  3,  -1),
            S(-50,  12),   S(-17,  28),   S(-38,  67),   S(-51,  75),   S( -7,  40),   S( -9,  63),   S(-17,  40),   S( 40,  16),
            S(-70, -17),   S(-12,  43),   S(-27,  48),   S( 15,  33),   S( 40,  66),   S( 41,  34),   S( 31,  47),   S(-19,  28),
            S(-33,  33),   S(  8,  41),   S( 31,  18),   S( 50,  31),   S( 22,  55),   S( 67,  78),   S(-38, -16),   S( -9,  11),
            S(-15,  -7),   S( -2,  -5),   S( -7,  64),   S( -7,  33),   S( 42,  42),   S( 14,  48),   S(-22,  32),   S(-27,   4),
            S(  9,  10),   S( 22,  38),   S( 28,  42),   S(  6,  29),   S( 28,  35),   S( 13,  24),   S( -1,   1),   S( 15,   3),

            /* queens: bucket 7 */
            S(-18, -20),   S(-24,  10),   S(-64,  34),   S(-26, -15),   S(-14, -40),   S(-51, -51),   S(-31, -40),   S(-14, -29),
            S(-33, -25),   S(-72,  23),   S(-26,   3),   S(-37,  52),   S(-37,  28),   S(-54,  45),   S(-45,  26),   S(-30,  -5),
            S(-52, -22),   S(-51,  15),   S(-29,  29),   S( -7,  11),   S(-12,  37),   S( -5,  31),   S(-18,  -5),   S(-18,   5),
            S(-47,  19),   S(-12,  19),   S( 14,  -2),   S(-15,  42),   S( 33,  33),   S( 43,  17),   S(-10,  47),   S( 15,  30),
            S(-35,  33),   S(-76,  -7),   S( 10,  17),   S( 41,  10),   S( 54,   1),   S( 70,  -1),   S( 43,  13),   S( 54, -23),
            S(-26,  27),   S( -3,  -2),   S( 10, -19),   S( 35, -27),   S( 32,  36),   S( 74,  17),   S( 71,  -1),   S( 67,   9),
            S(-19, -22),   S(  2,  -6),   S(  2,  -3),   S( 33,  -8),   S( 47,   7),   S( 52,  31),   S( 27, -17),   S( 72,  33),
            S(  8,   2),   S( 31,   2),   S( 22,  13),   S( 49,  17),   S( 25,  10),   S( -1,  17),   S( 32,  32),   S(  8,  45),

            /* queens: bucket 8 */
            S( -6, -16),   S(-14, -13),   S(-19,  -7),   S(  3,   9),   S( -6,  -4),   S( -6, -30),   S(-32, -48),   S( -4,   5),
            S( -5,  -1),   S(-11, -19),   S( -5,   9),   S(-19,  -1),   S( -5,   4),   S( -4,  -9),   S( -4, -30),   S(  1,  -3),
            S(  9,  22),   S( -2,  11),   S(-14, -18),   S(  6,   4),   S(-17, -11),   S(-11, -20),   S(-25, -36),   S(-13, -26),
            S(  2,  12),   S(  9,  17),   S( -3,  13),   S(  7,  -6),   S( -4,   6),   S( -6,  -5),   S( -5,  11),   S(  0, -21),
            S(  9,  17),   S(  9,  46),   S( 37,  30),   S( 14,  23),   S( 28,  49),   S(  1,   2),   S( -8,  -7),   S(-16, -16),
            S(  6,  15),   S( 23,  28),   S(-13,  25),   S( 22,  46),   S(-12, -23),   S(-20, -31),   S(  7,  21),   S(  0,   5),
            S( -6, -15),   S(-11, -17),   S( 20,  30),   S( 24,  42),   S(  6,  21),   S(  0,  22),   S(  3,   5),   S(-13, -23),
            S( -4,  -6),   S(  6,  -1),   S( -3, -19),   S( -4, -18),   S(  0, -23),   S( -1,  -8),   S(  3,   4),   S( -6, -16),

            /* queens: bucket 9 */
            S( -2,  -7),   S(-16, -29),   S(  2,  -4),   S(-32, -41),   S(-18, -29),   S(-19, -22),   S( -9,  -9),   S(-23, -35),
            S( -1,  -2),   S( -6,   1),   S( -8, -15),   S(  4,  12),   S(-10,  -6),   S( -7,  -7),   S(  8,  13),   S( -6,  -4),
            S(  2,   2),   S( -3,   2),   S(-13,  37),   S( -5, -16),   S(  2,  17),   S( 16,  27),   S( 22,  34),   S(  0,   0),
            S( -6,  -5),   S( -6,   6),   S( 13,  53),   S( 10,  31),   S(  4,   1),   S( -6, -10),   S(-16, -15),   S(  6,  -2),
            S(  1,   6),   S( 16,  56),   S( 19,  54),   S( 16,  48),   S( 16,  25),   S( 14,  40),   S(  6,  22),   S(-17, -10),
            S(-24, -28),   S(-24, -15),   S( 15,  36),   S(  7,  42),   S( 11,  11),   S(  4,  21),   S( -1,   4),   S( -7,  -1),
            S( -4, -12),   S( -4, -14),   S(-19,   4),   S(  6,  17),   S(  1, -13),   S( 17,  17),   S(  0, -14),   S(-19, -31),
            S( -6,  10),   S(  6, -18),   S( 32,  28),   S( 10,   7),   S( 22,   6),   S( -5, -10),   S( 10,   5),   S(-13, -25),

            /* queens: bucket 10 */
            S(  5,  -2),   S(-14, -20),   S(-13, -16),   S(-19,  -5),   S(-16, -25),   S(  2,   0),   S( -1, -10),   S(  5,  -2),
            S(-21, -32),   S( -7, -12),   S( -7, -15),   S( -1,  -5),   S( -8,  -3),   S(-10, -14),   S(  3,  -3),   S(-13, -20),
            S( -1, -12),   S( -2, -12),   S(-29, -35),   S( -5, -14),   S(  9,  15),   S(-21,  -7),   S(-11,  -8),   S( 10,  21),
            S(  0,  -5),   S(  6,   1),   S(  0,   9),   S( -1,  18),   S( 24,  23),   S( -2,  27),   S( -3,  -6),   S(-15, -25),
            S(-12, -22),   S( 19,   6),   S(-23,  -9),   S( 10,  66),   S( -8, -25),   S( 16,  22),   S(  5,  -7),   S( -3,  -7),
            S( -7,  -9),   S(-26, -35),   S( -7,  -7),   S( 17,  51),   S( -5,   5),   S( -1,   2),   S( 12,  13),   S(-11, -21),
            S( -7,   4),   S(-14, -31),   S(  3,  11),   S(-16, -28),   S( 19,   9),   S( 14,  24),   S( -6, -13),   S(  7,  13),
            S( 10,   1),   S( -1, -17),   S(  6, -13),   S( 21,  -2),   S( 34,  58),   S( -3,   9),   S( 18,  17),   S( -6, -24),

            /* queens: bucket 11 */
            S(  3,  10),   S(-29, -51),   S(-29, -30),   S( -6, -27),   S(-19, -31),   S( -7,  -3),   S( -8,  -2),   S(-19, -25),
            S(-25, -43),   S(-16, -13),   S(-18, -11),   S(-14,   5),   S(  1, -13),   S( -9, -19),   S(  6,  17),   S(  6,  20),
            S(-28, -45),   S(-25, -44),   S( -6, -27),   S( -6, -17),   S(-11,  -9),   S( -2,   1),   S( 14,  24),   S(-30, -21),
            S(-26, -36),   S(-20, -18),   S( -7, -20),   S( 22,  40),   S(  3,  -5),   S(-10,  -9),   S( 15,  20),   S( -6,  -2),
            S(-15,  -5),   S(  6,  -5),   S(  2,   5),   S(  5,  -8),   S( 19,  17),   S( 34,  83),   S( 34,  46),   S( -2,   1),
            S(-12, -35),   S(  3, -12),   S(-25, -32),   S(  6,  16),   S( 60,  35),   S( 38,  24),   S(  7,   9),   S(-10, -10),
            S( -7,  18),   S(-17, -25),   S( 18,  10),   S( -5,  12),   S( 10,  12),   S( 24,  28),   S( 45,  55),   S( -3, -10),
            S( -2, -15),   S(-13, -16),   S(-20, -44),   S(-13, -37),   S(-17, -16),   S( 14,  -4),   S(  9,  -3),   S( 10,   2),

            /* queens: bucket 12 */
            S(  7,  -4),   S(  3,   3),   S( -2,  -9),   S(-19,  -9),   S(-11, -14),   S(  0,   1),   S( -5, -11),   S( -3, -12),
            S( -1,  -7),   S(-14, -27),   S(-19, -28),   S(  2,   6),   S( -5,  -7),   S(  0,   3),   S( -7, -21),   S(  4,   4),
            S(  1,  -1),   S( -9, -10),   S( 14,  22),   S( -7,   1),   S( -5, -10),   S( -6, -11),   S(-15, -33),   S(-14, -19),
            S(  2,   3),   S( -7,  -7),   S(  3,   6),   S(  7,  14),   S( 12,  21),   S(  7,   4),   S( -3, -10),   S( -4,  -3),
            S(  2,   2),   S( 25,  42),   S( 27,  38),   S( 13,  33),   S(  4,  30),   S( 10,  21),   S(-13, -31),   S(  7,  -8),
            S( 10,  25),   S(  6,  22),   S( 30,  24),   S(-11,  10),   S( -1,   1),   S(  4,   3),   S(  8, -10),   S( -5, -14),
            S(  8,  11),   S(  7,  33),   S( 29,  27),   S( 22,  33),   S( 12,  21),   S(  5,   5),   S(  4,   1),   S( -3,  -3),
            S(  8,  -5),   S(-12, -34),   S(-23, -35),   S( -8, -36),   S( 13, -11),   S(  5,   1),   S(  3,  -6),   S( -9, -13),

            /* queens: bucket 13 */
            S(-11, -23),   S( -6, -12),   S(  2,  -7),   S( -8, -12),   S(  0,  -9),   S(  0,  -1),   S(-11, -17),   S( -6, -13),
            S(  4,   8),   S( -3,  -1),   S(  5,  18),   S( -1,   3),   S(-12, -19),   S(  8,  16),   S(  0,   0),   S( -5,  -5),
            S(  3,   2),   S( -5,   1),   S( -4,  13),   S(  4,  15),   S(  2,   0),   S( -7, -17),   S( -8,  -9),   S(-13, -10),
            S( -5,  -7),   S(  0,   4),   S(  4,  -1),   S( 21,  34),   S( 28,  51),   S(-19, -16),   S( -3,  -5),   S( -9, -12),
            S( -3, -11),   S( 13,  23),   S( 17,  53),   S(  5,  29),   S( 34,  68),   S(  7,   4),   S( -3,  -6),   S( -5, -14),
            S(  8,   5),   S(  8,  28),   S( 35,  74),   S(  6,  34),   S( -1,  -4),   S(  6,   9),   S( 11,   8),   S( -9, -14),
            S(  1,   3),   S(  8,  15),   S( 14,  28),   S( 12,  33),   S(  3,  15),   S( -2,  -5),   S(  8,   9),   S(  9,  16),
            S(-21, -20),   S(  2,  -2),   S(-16, -34),   S( -8, -20),   S(  0,  -5),   S(  6,   9),   S(  3,   2),   S(  2,   5),

            /* queens: bucket 14 */
            S(  5,  17),   S( -1,   0),   S(  1,  -5),   S( -7,  -6),   S(  5,   8),   S( -5, -14),   S( -7, -12),   S(-11, -25),
            S( -5, -12),   S(  4,  15),   S( -1,  -7),   S( -1,  -2),   S( -8, -14),   S( -6, -19),   S( -3,  -4),   S( -2, -10),
            S( -2,   1),   S(-12, -19),   S(-13, -20),   S(-12, -17),   S( -1,  -5),   S( -1, -12),   S( 18,  29),   S( -7, -20),
            S( -8, -12),   S( 10,  12),   S( -8,  -1),   S( 20,  40),   S( 10,  22),   S( -4,   1),   S( 11,  26),   S(  2,  -6),
            S( 10,  23),   S(  8,   1),   S(-25, -30),   S( 25,  23),   S( 11,  24),   S( 24,  28),   S( 13,  21),   S( -5, -17),
            S( -1,  -2),   S(  2,  10),   S( 18,  33),   S(  5,  11),   S( 12,  39),   S( 16,  42),   S(  4,   6),   S( -1,  -2),
            S( 11,  18),   S( 11,  11),   S( 18,  38),   S( 11,  25),   S( 16,  36),   S(  1,   2),   S( 10,  19),   S(  8,  13),
            S(  0,   6),   S(  6,  12),   S(  5,   5),   S(  9,  18),   S( -9,  -7),   S(  2,  -3),   S(  1,   9),   S(-16, -30),

            /* queens: bucket 15 */
            S( -2,  -9),   S(  3,  -4),   S(  2,   0),   S( -8, -12),   S( -3, -10),   S( -1,  -4),   S(-10, -21),   S(  5,  -4),
            S(  6,  -5),   S(  3,   2),   S(-11, -16),   S( -4, -11),   S( -1,   4),   S(  1,  -7),   S( 15,  26),   S( -1,  -6),
            S( -3, -13),   S(  3,  -8),   S(  2,  14),   S(-10, -21),   S( -8, -18),   S( -1,   8),   S( -2,  -6),   S(  3,  -5),
            S( -3,  -4),   S( -1,  -1),   S(  5,  10),   S(  0,  -8),   S(-12, -18),   S(  8,   9),   S( 12,  18),   S( 11,  11),
            S( -4,  -9),   S( -1,  -5),   S( -4,  -6),   S( -1,   3),   S(  3,  16),   S( -2,  -5),   S( -7, -10),   S( -2,  -3),
            S( -6,  -8),   S( -4, -15),   S( -2, -13),   S( -3, -13),   S( -1,   0),   S( 19,  27),   S( 11,  -1),   S(  5,  -6),
            S( -3,  -5),   S(  1,  -2),   S(  3,   3),   S(  6,  -1),   S(  5,   0),   S( 22,  38),   S( 22,  26),   S(  6,   9),
            S(  2,  -1),   S( -7,  -9),   S(-12, -12),   S( 16,  18),   S(  2,  -8),   S( -2,  -5),   S( -4, -19),   S( -5, -25),

            /* kings: bucket 0 */
            S( 70,  -1),   S( 61,  45),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 50,  36),   S(112,  65),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 48,  14),   S( -1,  32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 61,  50),   S( 56,  60),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-14,  34),   S( -7,  28),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 39,  67),   S( 43,  48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  1,  47),   S(-16,  10),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23,  84),   S(-34,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 21, -65),   S( 63, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -18),   S( 11,  24),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 36, -15),   S( 24,  -3),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 19,  32),   S(-10,  30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 36,  15),   S( 20,  13),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19,  47),   S(-16,  50),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 53,  32),   S( 17, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 21,  72),   S( 10,  17),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -119),  S(-15, -46),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -122),  S(-85,  -9),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -2, -41),   S(-37, -40),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-29, -27),   S(-71,  -8),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-16, -35),   S(-31, -29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-43, -17),   S(-93,  17),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10, -46),   S(-48, -109),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-69, -15),   S(-21, -93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55, -112),  S(-63, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36, -240),  S(  0, -98),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-63, -50),   S( 40, -75),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-62, -87),   S(-17, -106),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0, -65),   S(-97, -16),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  4, -111),  S(-84, -51),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-127,   7),  S(-41, -107),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-61, -51),   S(  7, -229),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0, -24),   S(-31,  16),   S( 15, -11),   S(-10,  22),   S( 17,  -3),   S( 55,   2),   S( 48, -10),   S( 56,   3),
            S( -6, -20),   S(-33,   3),   S(  1, -16),   S(  9, -19),   S( 16,  11),   S( 11,   4),   S( 28,  -1),   S( 31,  21),
            S( 12, -32),   S(  0, -20),   S( 30, -38),   S( 10, -18),   S( 19,  -3),   S( 10,  29),   S(  2,  42),   S( 41,  12),
            S( 22, -28),   S( 34,   5),   S( 56, -29),   S( 34,   3),   S( 15,  41),   S(-15,  84),   S( 11,  92),   S( 67,  65),
            S( 96, -39),   S(131,  -9),   S( 94,  -3),   S( 40,  21),   S( 38, 138),   S(-21, 145),   S( 25, 148),   S( 63, 137),
            S(-239, -47),  S(-106, -157), S( 41, -158),  S( 32,  55),   S( 79, 169),   S( 64, 212),   S(127, 183),   S( 88, 151),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38,  19),   S(-45,  20),   S(-23,  13),   S(-26,  35),   S(-12,  -2),   S( 20,   5),   S( 18,  -7),   S( 15,  26),
            S(-50,  19),   S(-52,  17),   S(-30,   8),   S(-13,   7),   S(  5,   7),   S(-10,   4),   S( -2,  -3),   S(-14,  19),
            S(-49,  30),   S(-16,  16),   S(-26,   7),   S(  6,  -8),   S( -1,  20),   S(-22,  18),   S(-27,  27),   S(-10,  24),
            S(-39,  46),   S(  8,  28),   S(-13,  23),   S( 15,  22),   S(  6,  25),   S(-33,  45),   S(  5,  33),   S( 34,  48),
            S(  4,  37),   S( 77, -24),   S( 76, -21),   S( 82, -12),   S( 17,  33),   S( -5,  41),   S( -9,  58),   S( 22, 102),
            S( 47,  55),   S(-66, -11),   S( 44, -129),  S( -7, -99),   S(-44, -51),   S( -5,  33),   S( 38, 176),   S( 62, 212),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  41),   S(-35,  20),   S(-18,   7),   S(-11,  26),   S(-28,  32),   S(-12,  12),   S( -1, -12),   S( -9,  22),
            S(-54,  37),   S(-41,  26),   S(-33,  10),   S(-22,  18),   S(-23,  17),   S(-33,   3),   S(-17, -14),   S(-42,  13),
            S(-46,  51),   S(-43,  51),   S(-12,  12),   S(-13,  18),   S(-21,  20),   S(-22,   4),   S(-34,   9),   S(-32,  12),
            S(-31,  92),   S(-48,  82),   S(-19,  45),   S(  2,  37),   S( -8,  38),   S(-27,  22),   S( -4,  22),   S( 21,  18),
            S(-25, 132),   S(-56, 127),   S( -6,  20),   S( 23, -19),   S(104,   0),   S( 88, -13),   S( 70, -16),   S( 24,   7),
            S(-17, 262),   S( 33, 182),   S(-16,  89),   S(  7, -84),   S(-45, -169),  S(-86, -146),  S(-72, -34),   S(  7,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  14),   S( -3,  12),   S( 10,   8),   S(  7,  44),   S( -5,  48),   S( 33,  21),   S( 21,  -6),   S( 10, -12),
            S( -1,  21),   S( -5,  25),   S(  0,   9),   S(  4,  12),   S( 12,  15),   S( 19,  -6),   S( 11, -17),   S(-18,  -7),
            S(  4,  40),   S(-11,  57),   S(  7,  19),   S(  7,   0),   S( 23, -11),   S( 16, -15),   S(  8, -25),   S(-14, -12),
            S(  5,  93),   S(-21, 106),   S(  3,  69),   S( 18,  33),   S( 29,   0),   S( 31, -26),   S( 22,   6),   S( 38, -21),
            S(  0, 153),   S(-21, 174),   S(-33, 173),   S( -1, 120),   S( 41,  44),   S( 87, -25),   S(122, -36),   S( 87, -44),
            S(122, 113),   S( 30, 259),   S( -2, 272),   S( -1, 208),   S(-27,  88),   S( 33, -172),  S(-70, -244),  S(-163, -188),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 68,  -4),   S(  6,  16),   S( -7,  -3),   S( -4,  17),   S( 10,   4),   S( -1,  -4),   S(-22,   0),   S(-73,  54),
            S( 35,   6),   S(  3,  18),   S(  9,  -6),   S(-26,   2),   S(-37, -12),   S(-35,  -7),   S(-39, -28),   S(-34,   5),
            S( 61, -12),   S(114, -31),   S( 34, -17),   S(-62,  10),   S(-80,  12),   S(  7,   3),   S(-80,  20),   S(-65,  40),
            S(-88, -71),   S(-24, -86),   S( 68, -51),   S(-28,  -2),   S(-21,   9),   S(-55,  62),   S(-15,  46),   S(-54,  81),
            S(-60, -72),   S(-54, -143),  S(-38, -79),   S( 68, -11),   S( 82,  95),   S( -7, 102),   S( 18,  86),   S(-14,  82),
            S(  5, -66),   S(-15, -77),   S( -1, -67),   S(  5,  33),   S( 79, 128),   S( 78, 171),   S( 51, 167),   S( 46, 132),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  47),   S(-49,  50),   S(-24,  28),   S( 52,  24),   S( 72,  -6),   S( 16,   1),   S(-27,   8),   S(-63,  55),
            S(-74,  41),   S(-56,  46),   S(-38,  24),   S(  2,  14),   S(-13,  21),   S(-19,   4),   S(-72,  11),   S(-76,  31),
            S(-24,  27),   S(-39,  56),   S( 14,  33),   S( 25,  31),   S(-28,  41),   S(-59,  36),   S(-76,  36),   S(-68,  44),
            S(-16,  35),   S(  2,  11),   S(-19, -42),   S(-13, -24),   S( -9,  -4),   S(-50,  25),   S(-14,  36),   S(-13,  55),
            S( 47,  10),   S(-11, -30),   S( 24, -94),   S(-11, -72),   S( 59, -45),   S( 19,  26),   S(-30,  63),   S(-43, 116),
            S( 44,  33),   S( 33, -30),   S(-62, -68),   S( -3, -40),   S(-17, -53),   S( 58,  41),   S( 82, 151),   S( 63, 130),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-85,  41),   S(-77,  27),   S(-14,   6),   S( -4,  12),   S( -1,  30),   S( 17,  16),   S( 16,   4),   S( -3,  33),
            S(-75,  24),   S(-65,  19),   S(-41,   8),   S( 30,  11),   S(-12,  24),   S( -5,  10),   S( -9,   7),   S(-14,  16),
            S(-67,  40),   S(-77,  44),   S(-49,  33),   S(-52,  48),   S(-11,  46),   S( 10,  21),   S(  9,  16),   S(-18,  23),
            S(-98,  91),   S(-74,  72),   S(-27,  30),   S(-13,  14),   S(-25, -32),   S( -4, -30),   S( -8,   3),   S( 34,   3),
            S(-19, 110),   S(-64,  81),   S( 39,  13),   S( 25, -49),   S(  6, -80),   S(-68, -55),   S(  3, -41),   S( 54,   4),
            S( 80,  83),   S(104,  75),   S( 38,  33),   S( 46, -92),   S( -4, -98),   S(-48, -47),   S( -8, -56),   S( 49,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,   4),   S(-44, -16),   S(  5, -27),   S(-54,  72),   S( 29,   6),   S( 73, -20),   S( 61, -31),   S( 83, -16),
            S(-60,  10),   S(-65,   7),   S(-18, -23),   S(-41,  11),   S(  7,  -3),   S( 47, -32),   S( 35, -20),   S( 59, -20),
            S(-53,  27),   S(-73,  31),   S(-30,  -1),   S(-39,  -2),   S(  4,  -6),   S( 21, -12),   S( 63, -18),   S( 56, -17),
            S(-52,  63),   S(-103,  89),  S(-68,  67),   S(-30,  30),   S(-17,   2),   S( 46, -60),   S( 35, -77),   S(  3, -96),
            S( -1,  73),   S(-74, 147),   S( -6, 136),   S(-42, 108),   S( 13,  24),   S( -5, -82),   S(-46, -137),  S(  7, -110),
            S(124,  88),   S( 82, 133),   S(126,  95),   S( 61,  93),   S( 51,  11),   S(-16, -123),  S(-49, -106),  S( -4, -183),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,   0),   S( 28,  -2),   S( 59,  -7),   S(-19, -44),   S(-32, -86),   S(-40, -14),   S( 34, -70),   S( 52, -43),
            S(  2, -50),   S( 32, -20),   S(-51, -71),   S(-72, -38),   S(-48, -40),   S( 45, -67),   S( 29, -75),   S(  3, -57),
            S( 33, -97),   S( -2, -36),   S( 10, -64),   S(-66, -56),   S(-40, -21),   S(  7, -34),   S(-35, -25),   S( -9, -22),
            S(  7, -32),   S(-33, -41),   S(  9, -23),   S( -7,   2),   S(-14,   9),   S( 13,  24),   S(-33,  42),   S(-10,  31),
            S( 11,   1),   S(-12, -29),   S(  7,  43),   S( 34,  92),   S( 57, 152),   S( 48, 127),   S( 11, 103),   S(-37, 139),
            S( 17,  36),   S(  8,  57),   S( 27,  82),   S( 44, 113),   S( 43, 110),   S( 53, 168),   S( 36,  93),   S(-27,  97),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 34,  -4),   S( 21,  27),   S( 11,  32),   S(  2,  25),   S( 28,  -6),   S(  1, -11),   S( 32, -49),   S(-19, -17),
            S( 45, -56),   S(-10, -35),   S(-11, -47),   S( -6, -43),   S(-15, -17),   S(-62, -29),   S(-45, -32),   S( 16, -39),
            S(-22, -43),   S(-28, -42),   S( -8, -80),   S(-54, -39),   S(-19, -18),   S(  5, -49),   S(-73, -29),   S( 32, -39),
            S(-25, -11),   S(  9, -81),   S(  5, -70),   S(-32, -21),   S(-26, -24),   S(-23, -15),   S(  4, -11),   S( 30,   0),
            S( 17,  12),   S(-18, -12),   S( -4,  -1),   S( 31,  41),   S(  1,  78),   S( 12,  45),   S( -7,  66),   S( 15,  56),
            S(-13,  56),   S( 36,  82),   S(  5,  57),   S( 12,  41),   S( 35, 116),   S( 24, 113),   S( 23,  86),   S( 19,  96),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26, -43),   S(  7, -56),   S( 10, -16),   S(-16, -41),   S( 37,  13),   S( 75,  11),   S( 15,  -1),   S( 18, -15),
            S(-39, -50),   S(-75, -35),   S( 11, -55),   S( 37, -33),   S( 15, -30),   S( 39, -42),   S(  4, -47),   S( 15, -49),
            S( -2, -48),   S(-97, -29),   S(-43, -59),   S(-14, -30),   S(-28, -35),   S(-35, -53),   S(-30, -73),   S( 56, -63),
            S(-32,   1),   S(-26,   6),   S(-35, -28),   S(-77, -19),   S(-25, -52),   S(-48, -51),   S(-16, -50),   S(  1, -51),
            S( 21,  18),   S( 48,   5),   S( 32,  17),   S(-23, -16),   S(  9,  34),   S( 28,   8),   S(-42,  22),   S( 37,   8),
            S(  9,  28),   S(  0,  49),   S( 23,  77),   S(  2,  78),   S( 14,  69),   S( -3,  34),   S(-14,  10),   S( 25,  59),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -39),   S(-29, -44),   S(-34, -49),   S( -7, -35),   S( -4, -13),   S( 56,  14),   S( 33,   4),   S( 77,  -8),
            S(  9, -72),   S(-33, -67),   S(-17, -83),   S( -9, -51),   S( -6, -44),   S( 15, -45),   S( 45, -50),   S( 71, -76),
            S(-46, -27),   S(-62, -19),   S(-25, -32),   S(-41, -24),   S(-91, -23),   S( 24, -63),   S( 31, -133),  S(100, -112),
            S(-37,  12),   S(-61,  33),   S(-17,  34),   S( 17, -25),   S(-49,   4),   S(-36, -40),   S(-52, -60),   S( 34, -95),
            S( -7,  36),   S(-36,  72),   S(-12, 114),   S( 14,  49),   S( 21,  38),   S(-12,  -4),   S( 12,  28),   S(-17, -28),
            S( 10,  88),   S( 28,  30),   S( 36, 105),   S( 33,  98),   S( 19,  70),   S( 35,  71),   S( 11,  30),   S( 34,  44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -102),  S( 26, -46),   S( 22, -23),   S(  2,  -9),   S(  1,  -6),   S(-33, -44),   S( 20, -39),   S(-12, -61),
            S( 28, -74),   S( 33, -60),   S(-27, -44),   S(-23, -58),   S(-33, -61),   S(-15, -56),   S(-20, -76),   S(-19, -75),
            S(-17, -68),   S(  7, -44),   S(-22, -111),  S(-26, -116),  S(-11, -50),   S(-14, -25),   S(-37, -78),   S(-34, -74),
            S(-21, -39),   S(-10, -31),   S(-17, -17),   S(  4,   1),   S( 19,  59),   S( 20,   8),   S(  9,  20),   S( -8,   4),
            S(  1,   7),   S(-12, -11),   S(  9,  24),   S( 16,  54),   S( 26,  60),   S( 23,  86),   S( 14, 104),   S( 32,  50),
            S( 16,  36),   S(  5,  58),   S( 14,  56),   S(  8,  51),   S( 23,  95),   S( 34,  95),   S(-12, -11),   S(-12,   2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -56),   S( 51, -79),   S( 14, -13),   S(  7,   8),   S(  3, -29),   S(-11, -34),   S(-13, -49),   S(-28, -74),
            S( 22, -144),  S( 24, -117),  S(  7, -74),   S(  8,  -1),   S(-20, -40),   S(-15, -61),   S( -1, -94),   S(-12, -80),
            S( 26, -104),  S(-11, -73),   S(-15, -86),   S( 16, -33),   S(-31, -16),   S( 27, -75),   S( -6, -75),   S( 46, -88),
            S(  6, -57),   S(-13, -32),   S(-13, -80),   S(-14,  30),   S( 14, -11),   S(-15, -12),   S(  3, -25),   S(  3, -38),
            S( -1,  43),   S(  4,  33),   S(  1,  14),   S( 13,  38),   S( 49,  86),   S( 29,  76),   S( 27, 103),   S( -4,  45),
            S(  3,  86),   S( 38,  81),   S(  0,  14),   S(  3,  45),   S( 20,  75),   S( 14,  36),   S( -9,  35),   S(-18,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -108),  S( 27, -85),   S(  8, -44),   S(  7,   3),   S( -6,   6),   S( -6,  13),   S( 33, -69),   S(-19, -38),
            S( 24, -122),  S(-32, -108),  S(  2, -92),   S(-31, -79),   S( -6, -65),   S( 12, -42),   S(  0, -59),   S( 22, -80),
            S( 26, -105),  S( -6, -82),   S(-14, -46),   S(  9, -66),   S(-20, -67),   S( 17, -82),   S(  1, -117),  S( 34, -72),
            S( -8, -40),   S(-19, -37),   S(-20,  -2),   S(-20,  22),   S( 26, -28),   S(  1, -26),   S( -9, -35),   S(  3,  20),
            S(-15, -22),   S( -2,  -2),   S(-18,  17),   S(-12,   8),   S( 30,  81),   S( -3,  23),   S( 27,  75),   S( 33,  73),
            S( -7,  25),   S(  8,  65),   S( 16,  40),   S( 22,  74),   S( 20,  80),   S( -2,  30),   S( 31, 112),   S( 33, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -30),   S(  7, -29),   S(-32, -63),   S( -4, -13),   S(-13, -25),   S(-17, -45),   S(  2, -52),   S(  6, -110),
            S(-41, -54),   S(-39, -122),  S(-27, -107),  S( -2, -32),   S( -7, -16),   S(-19, -50),   S( 26, -49),   S(  2, -103),
            S(-21, -42),   S(-27, -45),   S(-57, -62),   S( 10, -39),   S(-33, -50),   S(  6, -72),   S( 12, -61),   S( 29, -32),
            S( 11, -57),   S(-19, -18),   S(  2,  55),   S( -1,  64),   S(  9,   6),   S( -2,  -4),   S( -9,   8),   S(  3,  24),
            S( -1,  41),   S( -3,  77),   S(  4,  82),   S( 13,  46),   S( 25,  82),   S(  9,  69),   S( 31,  80),   S( -2,  -5),
            S(-14,   1),   S(-17, -47),   S(  6,  69),   S( 16,  50),   S( 18,  37),   S( 18,  53),   S(  5,  29),   S( 32,  52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-54, -50),   S(-16, -51),   S( -6, -22),   S(-26,  19),   S(-22,  -7),   S(-29,  -6),   S( -7, -14),   S(-71, -42),
            S( 19, -37),   S(-29,  31),   S(-26, -43),   S( -8, -12),   S(-18,  -9),   S(-24, -12),   S(-27, -39),   S(-22, -40),
            S(-25, -18),   S(  4, -27),   S(  1,  -3),   S( 29,  16),   S( -7,   7),   S(  5,  -2),   S(-36,  15),   S(-29, -33),
            S(  9,  27),   S( 41,  64),   S( 19,  45),   S( 47,  16),   S( 32,  14),   S( 11,  31),   S( 52, -53),   S(-12, -24),
            S( 56,  69),   S( 12,  69),   S( 65,  43),   S( 50,  50),   S( 67,  26),   S( -1,  11),   S( 25, -13),   S( 34,  -9),
            S(146, -16),   S(-18,  78),   S(137, -11),   S( 82,  37),   S( 52,  33),   S(-17,  56),   S(  8, -19),   S(  7,  14),
            S( 60,  23),   S( -7, -23),   S( 40, -10),   S( 76,  63),   S( 35,  17),   S( 11,  38),   S(-28,   6),   S( -3,  -3),
            S(-92, -93),   S(  4,   3),   S( 11,  18),   S( 47,  28),   S( -5,  19),   S( 25,  11),   S(-22,  10),   S(  2,  22),

            /* knights: bucket 1 */
            S( 28,   9),   S(-56,  20),   S(-32,  -7),   S(-30,   7),   S(-29,  34),   S(-20, -52),   S(-26, -27),   S(-42, -41),
            S(-45,  31),   S(-62,  55),   S(-22,   6),   S(-14,  20),   S(-23,  19),   S(-13,  16),   S(-34,  -3),   S(-23, -47),
            S(-48,  50),   S( -7,  -7),   S(-22,   5),   S(-33,  53),   S(-24,  41),   S(-10,  -3),   S(-45,  30),   S(-18,  54),
            S(-10,  69),   S( 19,  33),   S( -4,  41),   S( -6,  55),   S( -3,  45),   S(-22,  64),   S(-17,  28),   S(-39,  62),
            S( 33,  20),   S( -1,  26),   S( 50,  47),   S( 23,  45),   S( 43,  52),   S(-12,  68),   S( -2,  42),   S(  2,  67),
            S( 31,   5),   S( 66, -24),   S( 69,   7),   S( 82,  56),   S( 35,  53),   S(-45,  88),   S( 11,  36),   S( 27,  13),
            S( 34,  -7),   S( 15,   1),   S( 28, -38),   S( 31,  45),   S( 33,  23),   S(-19,  31),   S(  6,  61),   S(  2,  46),
            S(-167, -12),  S( 25, -11),   S(-17, -43),   S( -2,  24),   S(-10,  34),   S( 22,  24),   S( 24,  57),   S(-45,  35),

            /* knights: bucket 2 */
            S(-65,  15),   S(-33,  20),   S(-36,   7),   S(-31,  22),   S(-29,  26),   S(-52,  -5),   S(-25,  -1),   S(-33,   0),
            S(-19,  -4),   S( -4,  32),   S(-18,  14),   S(-16,  19),   S(-29,  20),   S(-19,   5),   S(  7,  -5),   S(-36,   8),
            S(-29,  40),   S(-21,  10),   S(-23,   7),   S(-36,  60),   S(-24,  44),   S(-19,   7),   S(-21,  12),   S(  2,  -9),
            S( -4,  57),   S(  1,  34),   S(-23,  79),   S(-12,  62),   S(-35,  74),   S(  2,  44),   S(  0,  41),   S( -8,  38),
            S( -7,  49),   S(-19,  70),   S( 10,  62),   S( 18,  56),   S(  2,  70),   S( 19,  60),   S( -7,  64),   S( 27,  19),
            S(-48,  91),   S(-19,  50),   S(-16,  74),   S( 65,  17),   S( 36,  21),   S(140, -21),   S( 80,   5),   S( 27,  -4),
            S( 17,  54),   S(-46,  63),   S( 42,  18),   S( 21,   5),   S( -2,  45),   S( 10,  -9),   S( 52,  33),   S( 11,  -8),
            S(-73,  67),   S( 29,  76),   S( -9,  62),   S( -5, -44),   S(-40,  -1),   S(-35, -31),   S( 36,  17),   S(-116, -68),

            /* knights: bucket 3 */
            S(-71,  29),   S(-11, -55),   S( -3, -22),   S( -9, -15),   S(  3, -16),   S(-14, -30),   S(-22, -16),   S(-38, -85),
            S(-13, -47),   S( -9,  17),   S( 13, -19),   S( -3,  -2),   S( -8,  -2),   S( 18, -21),   S( 22, -49),   S( 22, -61),
            S( -7,  -4),   S(-14,  -5),   S( -1,   9),   S( -8,  38),   S(  7,  30),   S( -1,  12),   S( 13,   2),   S( 21, -42),
            S( 10,   2),   S( 10,  35),   S( 19,  36),   S(  9,  47),   S( 11,  64),   S( 27,  52),   S( 22,  47),   S(  8,  48),
            S( -9,  45),   S( 17,  36),   S( 29,  48),   S( 30,  74),   S( 30,  70),   S( 29,  78),   S(  3,  94),   S( 67,  81),
            S(-25,  41),   S(  1,  36),   S( 11,  45),   S( 26,  60),   S( 50,  68),   S(147,  57),   S( 60,  88),   S(  2, 128),
            S(-17,  39),   S(-12,  63),   S(-39,  77),   S( 24,  62),   S( 48,  55),   S(107,  28),   S( 21,   4),   S( 93,   0),
            S(-142,  47),  S(-59,  97),   S(-50,  83),   S( 63,  25),   S( 54,  59),   S(-32,  78),   S( -9, -38),   S(-50, -104),

            /* knights: bucket 4 */
            S( 22,  54),   S(-12, -30),   S(-76,   5),   S(-36, -24),   S(-40,  -3),   S(-24, -15),   S(-13, -27),   S(-13,  -6),
            S( 47,  46),   S(  7, -16),   S(-12,  -4),   S(-42,  13),   S(-32,  17),   S( -3, -50),   S(  2,  10),   S(-42,  -4),
            S(  9, -28),   S( 44,  -6),   S( 68,  12),   S( 96, -15),   S( 11,  -2),   S(  6, -29),   S(-12, -15),   S(-24,  -6),
            S( -6, -41),   S( 23,  -3),   S( 62, -19),   S( 87,  -2),   S( 16,  24),   S(-45,  60),   S(-50,  28),   S( 15,   9),
            S(  6, -47),   S( 16,  10),   S( 67,   4),   S( 41,  28),   S( 45,   2),   S( 49,  10),   S( 19, -25),   S(-34,  12),
            S(-22, -53),   S(  6,  12),   S( 47, -27),   S( 61,  32),   S( -4,  20),   S(  0,  57),   S( -5,  14),   S( 23, -12),
            S(-22, -14),   S(-34, -29),   S( 15,   0),   S( 26,   1),   S( 41,  24),   S( -4,   9),   S(  5,  48),   S(-36, -11),
            S(  3,  13),   S(  0, -11),   S(-14, -45),   S(  0, -21),   S(  2,  19),   S(-18,  17),   S(  3,  29),   S(-10,  -5),

            /* knights: bucket 5 */
            S( 30,  29),   S( 18,  26),   S(-40,  36),   S(-18,  28),   S(-61,  52),   S( 11,  12),   S(-15,  11),   S( 17,  39),
            S( 29,  44),   S( 49,  36),   S( -5,  15),   S(-33,  16),   S( 28, -16),   S(-13,  -6),   S(-10,  34),   S(-59,  25),
            S(-16,   9),   S( 29, -16),   S( 31,  -4),   S( 36,  10),   S( 43,  11),   S(-49,  34),   S(-16,  15),   S(-25,  29),
            S(  6,  18),   S( 19, -17),   S( 65,   4),   S( 66, -13),   S(107,  -1),   S( 90,  -2),   S( 27,  18),   S( 33,  39),
            S( 64,  10),   S( 62, -20),   S( 96, -19),   S(134, -20),   S(111, -24),   S( 46,  12),   S( -4,  18),   S(-22,  36),
            S(-12, -17),   S( 20, -45),   S( 32, -51),   S(  5,  -2),   S( 32, -16),   S( 72, -21),   S(-25,  30),   S( 35,  39),
            S(  2,   0),   S(-14, -23),   S( -2, -60),   S(-24,  15),   S( -3, -50),   S(  6,  20),   S(  2,  36),   S( 28,  39),
            S(-29, -52),   S(-19, -36),   S( 14, -23),   S(-19, -18),   S( 12,   3),   S(-18,  23),   S( 14,  19),   S(-10,  46),

            /* knights: bucket 6 */
            S(  7,  14),   S(-23,  36),   S(-13,   0),   S(-54,  40),   S(-39,  32),   S( 23,  25),   S( 33,  53),   S(-18,  43),
            S( 18, -15),   S(  2,  51),   S(-61,   9),   S( 19,  12),   S( 39,   1),   S(-46,  47),   S(-47,  49),   S(-30,  57),
            S( -3,  27),   S( 23,   4),   S( 19,  14),   S( 45,  23),   S( 19,  30),   S(-26,  34),   S(  6,  32),   S(-38,  37),
            S( 11,  54),   S( 65,   9),   S( 35,  25),   S( 53,  13),   S( 84,  -6),   S( 78,   0),   S(  9,   8),   S(-25,  60),
            S(-20,  59),   S( 41,   6),   S( 74,   4),   S(120, -10),   S( 84,  -6),   S( 36,  24),   S(112, -14),   S( 16,  18),
            S( 20,  29),   S( 48,  -2),   S( 67,  10),   S( 35,  -4),   S( 81, -29),   S( 38,   8),   S( 13, -10),   S( 19,   6),
            S( 12,  20),   S(  1,  32),   S( 57,  46),   S(  6,  -3),   S( 34,  -8),   S( 19, -46),   S(-14,  -2),   S( 22,  56),
            S( 37,  71),   S(  3,  39),   S(  0,  17),   S( -1,  14),   S( 32,  -5),   S(-11,   4),   S( 23,  41),   S(-19, -43),

            /* knights: bucket 7 */
            S(-31, -56),   S(-41, -15),   S( -7,  19),   S(-86,  25),   S( -2, -36),   S(-54,  12),   S(-27,   0),   S( -7,  31),
            S(-33, -45),   S( 23, -45),   S(-41,   5),   S(-26, -14),   S(  2,  -8),   S(  4,  23),   S(-17,  21),   S(-58,  28),
            S(-36, -27),   S(-50, -10),   S(  8, -19),   S( -1,  19),   S( 38,  25),   S( 39,   7),   S( 36,   7),   S( -6,  32),
            S(-63,  27),   S( 26, -13),   S( 49, -19),   S( 77,  -2),   S(112,  -9),   S(105, -11),   S( 87,   0),   S( 55,   5),
            S(  2,   8),   S(  6,  -4),   S(  5,   7),   S( 89,  -9),   S(121,  -1),   S(124, -26),   S(202, -21),   S( 20,  25),
            S(-25,  22),   S(  8,  11),   S(-13,  26),   S( 14,  28),   S(133, -29),   S( 98,   0),   S( 30,  -7),   S( 17, -46),
            S(-29,  20),   S(-23,   9),   S(  2,  21),   S( 34,  21),   S( 47,  29),   S( 26,  24),   S(-19, -18),   S( -3,   0),
            S(-35, -25),   S( -9,  -6),   S(  8,  35),   S( -9,  34),   S( 27,  19),   S( 15,   8),   S( 10,  10),   S( -3,  -9),

            /* knights: bucket 8 */
            S( -5, -10),   S( 14,  50),   S( 13,  29),   S(-26, -79),   S( -9,   9),   S( -6, -16),   S( 20,  44),   S( -7, -23),
            S(-11, -48),   S(-10, -43),   S(-16, -54),   S(-15,  22),   S( -2,  32),   S( -1,   2),   S(  0,  -6),   S( -4, -15),
            S(-11, -30),   S(-17, -45),   S(  1, -48),   S( 18,  18),   S(-17, -32),   S( -2,  25),   S( -7, -28),   S( -5, -17),
            S(-14, -59),   S( -9, -21),   S( -5,   3),   S(-14,  28),   S(-22,  -4),   S(-19, -24),   S(-33, -18),   S(-19, -66),
            S( -6, -24),   S(  6,   0),   S(  1, -23),   S(  2,   3),   S( -8, -15),   S(-15, -14),   S( 12,   8),   S(  7,   5),
            S(-10, -12),   S( 31,  26),   S(  9,  21),   S( -7, -17),   S(  3,  39),   S(-22, -52),   S(-11,   0),   S( -8, -20),
            S(  0,   7),   S( -7, -38),   S( -9, -26),   S( -1,  20),   S( -6, -30),   S(  1,  -6),   S( -3,  -6),   S(  0,  -2),
            S(  3,   7),   S(  2,  11),   S( -4,  -8),   S(  4,   0),   S(  0,  15),   S( -7, -23),   S( -4,  -3),   S( -2,   1),

            /* knights: bucket 9 */
            S(-20, -60),   S( -7, -13),   S(  7, -48),   S(  1, -38),   S(-27, -13),   S(-14,  10),   S(  5,  31),   S(  7,  -5),
            S( -8,  -1),   S(-22, -87),   S(-12, -112),  S(-12, -47),   S(-17, -50),   S(-12, -46),   S( -5,  13),   S(-10,  14),
            S( -7, -24),   S(-12, -42),   S(-18, -41),   S( -7, -64),   S(-15,   9),   S(  8,  -6),   S(-10,  30),   S( -3,   2),
            S(-22, -67),   S(-10, -54),   S( -1, -19),   S( -3, -58),   S(-12, -31),   S(  7,   6),   S(-17, -44),   S(  9,  42),
            S(  3,  20),   S(-10, -23),   S(-22, -25),   S( -4, -39),   S( -9, -33),   S( -5,  16),   S(-14, -16),   S(  3,  16),
            S(-17, -57),   S(-16, -23),   S( -3, -11),   S( 10,  21),   S(  0,  -8),   S( -7, -20),   S(  3,  13),   S( -6,  20),
            S( -9, -11),   S(-10, -12),   S(-16, -21),   S(-15,  -4),   S( 20,  20),   S( -3,  15),   S( -7,  25),   S( -5, -15),
            S(  5,   6),   S(  2,  -1),   S( -1,   6),   S(  3,  -2),   S( -9,   7),   S(  3,  31),   S( 13,  26),   S( -2,  -1),

            /* knights: bucket 10 */
            S( -3,  -9),   S( -2,   7),   S( -8,  22),   S( -6,  28),   S( -6, -19),   S(  4,  -2),   S( -4,  20),   S( -4,   7),
            S( -4,  -5),   S( 18,  37),   S(-12, -34),   S(-11, -88),   S(-15, -35),   S(-24, -45),   S(-12,   1),   S( 13,  53),
            S(  2,  -9),   S( -4,  -2),   S(-11, -12),   S(  0, -46),   S(-15, -46),   S(  6, -28),   S(-13, -46),   S( -5,  16),
            S(-12, -16),   S(-10, -32),   S( 12, -36),   S( -7, -35),   S(-10,  -6),   S( -2, -25),   S(-13, -63),   S(-11, -29),
            S(-25, -59),   S(-25, -47),   S(-22,   7),   S(-13, -46),   S(  9,   9),   S( -6, -38),   S( 10,   1),   S( -1,   1),
            S( -3,   2),   S(-12,  14),   S(  0,  -5),   S(-17,   6),   S(-11,   9),   S(-23,  -3),   S(-11,   1),   S(-17,  -5),
            S( 11,  27),   S( 10,  41),   S(-11, -31),   S( 18, -34),   S(-26, -20),   S(-24, -46),   S(-10,  21),   S( -6,   8),
            S( -4,  -8),   S(  8,  21),   S(  6,  40),   S(  1,   9),   S( -2,  13),   S( -2,   2),   S(  5,  16),   S( -1,   1),

            /* knights: bucket 11 */
            S( -9, -22),   S(-32, -35),   S( -7, -21),   S( 10,  21),   S(-45, -63),   S(-10,  13),   S( -6,  13),   S(  5,  17),
            S(  2,   2),   S(-29, -63),   S(-15, -59),   S( 18,   8),   S(  4,  37),   S( -9, -37),   S(  4,  -1),   S( -2,  12),
            S(-12, -22),   S(-15, -10),   S(-14,   0),   S(  8, -15),   S(-15,  -4),   S(  0, -13),   S(-10, -27),   S(-11,  -1),
            S(-15,  -5),   S( 10,  -5),   S(-12,  -9),   S( -8,  11),   S( 35, -23),   S( 17, -15),   S( 26,  35),   S(  1,   5),
            S(-15,  12),   S( 23, -41),   S( -4,   0),   S( -5,  -6),   S( 30,  10),   S( 21,  36),   S(-14, -77),   S( -7, -16),
            S(  4,  -7),   S( -9, -53),   S( 11, -21),   S(-17, -26),   S(  0,  21),   S(  2,   3),   S( -4, -26),   S(  2,  22),
            S( -2, -20),   S( -1,  14),   S(-23, -37),   S( -3,   7),   S( -1, -25),   S(  5,  -3),   S(  8,  -5),   S(  1,  28),
            S(  0,   0),   S(  0,  -1),   S( -8, -23),   S( -1,  25),   S(  6,  10),   S(  7,   3),   S(  4,  23),   S(  1,   4),

            /* knights: bucket 12 */
            S(-13, -42),   S( -5,  -4),   S( -2, -15),   S(  1,   7),   S(  0,  18),   S( -2,  -7),   S(  4,  21),   S(  0,   3),
            S(  0,   4),   S(  1,   5),   S( -4, -20),   S( -7,  -6),   S( -1, -11),   S( -8,   1),   S( -4,  -7),   S(  3,   4),
            S( -2,   3),   S( -6, -23),   S(-16, -43),   S(-31, -53),   S( -5,  -1),   S(-10,   3),   S( -6,  -6),   S( -5, -20),
            S(  2,  11),   S( -3, -43),   S( -8,  10),   S(  1,  18),   S( 14,  -3),   S(  5,  35),   S(  2,   7),   S(  0,  -4),
            S( -3,   7),   S( -9, -17),   S( -6, -28),   S(  0,   2),   S( -1,  12),   S( -6,  23),   S( -8,  -3),   S(-10,  -4),
            S( -1,   5),   S( -3, -17),   S( -4, -11),   S( -1, -14),   S( -1, -16),   S( -4,  -8),   S( 17,  22),   S( -3,  11),
            S( -3, -11),   S( -4,  -8),   S( -7,  11),   S(  1,  15),   S(  1,  16),   S(-15, -13),   S( -6, -20),   S( -3,  -4),
            S(  0,   0),   S(  0,   1),   S( -1,   2),   S(  2,   4),   S(  1,   1),   S(  5,   3),   S(  1,   2),   S( -2,  -5),

            /* knights: bucket 13 */
            S(  5,  -1),   S( -1,  -6),   S( -5, -38),   S(  6,  -5),   S(  4,   8),   S(  0,  19),   S( -3,  -1),   S(  3,   8),
            S( -6,  -7),   S( -4,  -8),   S(  1,   0),   S( -9,  -3),   S(-14, -18),   S(  1,   1),   S(  5,  32),   S( -2, -10),
            S(  4,   3),   S(  3,  11),   S(  6,   2),   S(  4, -14),   S( 11,  34),   S( -8,  -4),   S( 16,  24),   S(  3,  23),
            S( -1,   2),   S( -3,  16),   S(-11, -26),   S(  1,  25),   S(  1, -13),   S(  7,  46),   S(  8,  13),   S(  7,  11),
            S(  1,  16),   S( -6, -35),   S(-14,  36),   S( -6,  37),   S(-17,   2),   S(  7,  35),   S( -3, -10),   S( -5, -17),
            S( -6,  -6),   S(  5,   4),   S( -5,  27),   S(  2, -10),   S(-16,   1),   S(-12,  -4),   S(  3,  17),   S( -5,  -9),
            S(  0,  -1),   S(  1,   0),   S( -7,   9),   S( -1,   0),   S(  4,  23),   S(  0,  15),   S( 10,  19),   S(  1,   5),
            S(  1,   3),   S( -3,  -5),   S( -5, -14),   S( -3,  -1),   S(  1,   5),   S( -3,  -5),   S( -1,   0),   S(  1,   1),

            /* knights: bucket 14 */
            S(  0,  -1),   S( -1,   0),   S( 15,  49),   S( -4,  -4),   S( -5, -12),   S(-10,   5),   S(  5,   6),   S( -2,  -3),
            S(  0,  -3),   S(-10, -21),   S( -2,  -6),   S(  5,  -8),   S(  6,  23),   S(  7,   8),   S( -6,  18),   S(  7,  49),
            S( -3,   3),   S( -3, -31),   S(  4,  16),   S( -9, -46),   S( -1, -17),   S(  3,  12),   S( -2,   4),   S(  3,  25),
            S(  2,   5),   S( -6, -18),   S(-19, -18),   S( -3,  43),   S(  6,  40),   S( -7, -14),   S( -1, -11),   S( -1,  41),
            S( 11,  25),   S(-21, -49),   S(-10,   0),   S(  2,   4),   S( -4,  31),   S( -2,   9),   S( -3,  -4),   S(  1,   5),
            S( -3,  -3),   S(  2,   0),   S( 14,  32),   S( -5,  17),   S(  3,  10),   S(-11,   2),   S( -6,  -9),   S( -4,  -6),
            S( -2,  -5),   S(  1,   0),   S(  6,  13),   S(  0,   1),   S(  8,  28),   S( -3, -11),   S(  3,  13),   S(  6,   0),
            S(  0,   1),   S(  2,   3),   S(  1,   1),   S(  3,   7),   S(  2,   3),   S( -1,  -2),   S(  2,   5),   S(  0,   1),

            /* knights: bucket 15 */
            S( -3, -16),   S(  1,   5),   S(  3,  23),   S(  4,  14),   S( -5, -17),   S( -7, -30),   S( -5, -26),   S(  0, -13),
            S(  0,  -7),   S(  5,   7),   S( -4, -11),   S(  6,  22),   S( 15,  35),   S(-14, -68),   S( -2,  -4),   S(  0,   4),
            S(  2,   4),   S(-14, -35),   S(  5,  -1),   S(  5,  -6),   S(-23, -46),   S( -2,  -2),   S( -4, -10),   S(  0,  -1),
            S( -1,  -9),   S( -7, -16),   S( -1,  -8),   S(  2,  20),   S( -2,  12),   S( -3,  47),   S( 10,  17),   S(  0,   2),
            S( -1,   2),   S(  7,  16),   S( -3,  12),   S( -9,  -2),   S(  8,  10),   S( 14,  28),   S( -3, -16),   S(  5,  20),
            S(  1,   5),   S(  3,   0),   S(  7,  27),   S( -7, -14),   S( -4,  11),   S( -2,  15),   S(  1,  16),   S(  6,   9),
            S( -3,  -6),   S( -4, -13),   S(  5,  21),   S(  0,  -1),   S(  4,  20),   S(  8,  12),   S(  0,   3),   S(  4,   9),
            S(  3,  11),   S( -1,   0),   S(  2,   2),   S( -1,   2),   S(  3,  15),   S(  1,   4),   S( -1,  -5),   S(  1,   4),

            /* bishops: bucket 0 */
            S( 22,  -7),   S(-15,  30),   S(-14,  16),   S(-29,   1),   S(  8,   0),   S( 10,  -5),   S( 59, -43),   S( 35, -12),
            S(-15,  19),   S( -2, -24),   S(-34,  64),   S( -2,   7),   S(  7,  18),   S( 46,  -3),   S( 36,  56),   S( 47, -11),
            S( -2,  47),   S( -9,  21),   S( 12,  19),   S(  7,  15),   S( 14,  26),   S( 47,  31),   S( 35,   6),   S( 24,   6),
            S( 23, -23),   S( 53, -61),   S( 11,  25),   S( 42,   0),   S( 73,  29),   S( 19,  67),   S( 11,  25),   S( 16,  22),
            S( 45, -13),   S( 42,  -3),   S( 50,   6),   S( 79,  36),   S( 95,   8),   S( 10,  33),   S( 38,  44),   S(  4,  14),
            S( 72,  21),   S( 46,  45),   S(125,  22),   S( 50,   5),   S( 36,  17),   S( 16,  61),   S( 15,  28),   S( -2, -10),
            S(-53, -93),   S( 84,  30),   S( 87,  33),   S( -3,   0),   S( 11,  13),   S( 20,  14),   S(-19,  35),   S(-27,  59),
            S( -7, -50),   S(  0,  18),   S(-11, -39),   S(-23,   4),   S(-60, -48),   S(-26,  -3),   S(-33,  39),   S(-12,  -3),

            /* bishops: bucket 1 */
            S(-35,  32),   S(  1, -19),   S(-23,  43),   S( 14,  -6),   S(-35,  38),   S(  5,   9),   S( 32,  -2),   S( 41, -36),
            S( 15, -14),   S(-13,  -7),   S(-22,   3),   S(-15,  12),   S( 27, -19),   S( 10,  -8),   S( 55, -26),   S( 21, -28),
            S(-18,   6),   S( 23, -22),   S( -6,  -4),   S( 12,   1),   S(  2,  -5),   S( 31,  -7),   S(  6,   4),   S( 73,   2),
            S( 39, -15),   S( 44,  -9),   S( 15,   8),   S( 23,  11),   S( 53, -27),   S(  6,  13),   S( 50,  -7),   S(-21,  36),
            S( 36, -13),   S( 44,  -3),   S( 19,   1),   S(107, -37),   S( 40,  25),   S( 30,  30),   S(  1,  30),   S( 20,   3),
            S( 46, -41),   S( 75, -22),   S( 90, -16),   S( 81, -32),   S( 43,  28),   S(-44,  35),   S(-45,  84),   S(-46,  29),
            S( 23, -73),   S( -3, -50),   S( -8,  -3),   S( 23,  42),   S( 30,  34),   S(-14,   9),   S(  0,  11),   S(-24,  32),
            S(  8, -17),   S(-21,  -1),   S(  4, -21),   S(-31,   0),   S(  1,  10),   S( 38,  -7),   S( 43, -28),   S(-54,  -3),

            /* bishops: bucket 2 */
            S( 25, -16),   S(-28,   7),   S(  5,  17),   S(-28,   3),   S( 23,  10),   S(-10,   0),   S(  8,  -3),   S( 29, -39),
            S( 38, -21),   S(  7,  -9),   S(-11, -11),   S( 10,   5),   S( -8,  -1),   S( 15,  -4),   S(  6, -19),   S( 15, -53),
            S( 49,  -3),   S( 14,  -6),   S( -3,  23),   S( -8,  -1),   S( -5,  22),   S(-10, -13),   S(  3, -30),   S(-15,  -1),
            S( -4,  -9),   S( 54,  -1),   S( -2,   2),   S( 16,  34),   S( -6,  14),   S( -3,  17),   S( -5, -16),   S(  0,  39),
            S(  8,  33),   S(-31,  46),   S( 61,  11),   S( 29,  19),   S( 19,  28),   S( 18,  13),   S( 18,  26),   S( 59, -26),
            S(-35,  54),   S(-13,  36),   S(-41,   7),   S( 91, -10),   S( 53,  -5),   S(110, -11),   S( 85,  -2),   S( 28, -37),
            S(-33,  72),   S(-31,  16),   S(  3,  13),   S( 17,   9),   S(-19, -35),   S(-45,  -1),   S( -4,  -7),   S( -3, -45),
            S(-57,   0),   S(-12,  21),   S( 12,   1),   S(-51,  43),   S(-21, -11),   S(-17,   5),   S(-27, -24),   S(-45,  23),

            /* bishops: bucket 3 */
            S( 32,  14),   S( 18,  -3),   S( 20, -22),   S(  9,  -3),   S( 24,  13),   S(  0,  29),   S(-12,  60),   S( -3,  25),
            S( 37,  17),   S( 26, -11),   S( 11,   1),   S( 21,   4),   S( 19,  16),   S( 25,   1),   S( 11,  -4),   S( 32, -43),
            S( 15, -11),   S( 27,  34),   S( 21,  35),   S( 13,  32),   S( 12,  27),   S(  8,  23),   S( 21, -17),   S( 12,  11),
            S(-15,  16),   S( 24,  30),   S( 16,  53),   S( 29,  50),   S( 25,  20),   S( 25,   2),   S( 24,  -5),   S( 34, -37),
            S( 18,  26),   S(  6,  56),   S(  5,  55),   S( 48,  54),   S( 46,  39),   S( 50,  12),   S( 30,  20),   S(-10,  27),
            S(  2,  47),   S( 14,  51),   S(-12,  46),   S( 28,  18),   S( 45,  38),   S( 71,  80),   S( 50,  42),   S( 39,  77),
            S(-34,  91),   S( 14,  34),   S(  2,  38),   S(-12,  54),   S( 21,  33),   S( 39,  53),   S(-12,  28),   S( 16, -69),
            S(-34,  36),   S(-49,  59),   S(-62,  38),   S(-26,  44),   S( 19,  17),   S(-54,  33),   S( 29,  22),   S( 31,   7),

            /* bishops: bucket 4 */
            S(-38,  17),   S(-20,   9),   S(-12, -12),   S(-63,  18),   S(-24, -24),   S(-20,  31),   S(  1,   9),   S(-32, -18),
            S( -9,   2),   S( 30, -32),   S( 74, -42),   S(-33,   3),   S(-59,  19),   S( -3, -51),   S(  0, -18),   S(-34, -34),
            S(-19,  22),   S(  6, -27),   S( 13,  13),   S( -2,  -7),   S( 25, -27),   S(-86,  44),   S(  7, -46),   S(-50,  -7),
            S( 18,   0),   S( 45, -30),   S( 36,  -4),   S( 19, -11),   S(-25,  43),   S( 39,  -3),   S(-50,  13),   S( -2, -24),
            S(  9, -32),   S(-17, -35),   S( 33,   2),   S( 45, -10),   S(-19,   5),   S( 14,  24),   S(-62,  43),   S(-41,   2),
            S(-65, -104),  S(-55, -23),   S( 11, -10),   S(  1,  -4),   S(-64,  63),   S( 35,  -1),   S(-16, -10),   S(  5,  62),
            S(  4,   3),   S(-26,  11),   S( 24,   5),   S(-14, -17),   S( -7,  -4),   S( 31, -26),   S( -5,   4),   S( 19,  41),
            S(  7,  23),   S(  1,  -7),   S(-13, -14),   S( -7, -41),   S(-30,   8),   S( -9,  23),   S(  4,  41),   S( 14,  30),

            /* bishops: bucket 5 */
            S(-15,  11),   S( 16, -14),   S(-82,  37),   S(-61,  26),   S(-48,  12),   S(-48,  22),   S(-22,  31),   S(-44,  10),
            S(-29,  19),   S(-45,  20),   S( 19,  -1),   S(-39,  22),   S(-58,  21),   S(-38,  29),   S(-52,  16),   S( 17,  -5),
            S( -3,  26),   S( -9,  -3),   S(  5,   6),   S(  7,  -1),   S(-18,  12),   S(-44,  15),   S(-15,   7),   S( 24,  -9),
            S( 22,  -2),   S( 34,  -8),   S( 68, -30),   S( 37,   0),   S( -8,  20),   S( -3,  23),   S(-79,  29),   S(-17,  17),
            S( 20, -23),   S( 55, -16),   S(-23,  13),   S(-10, -11),   S( 23, -20),   S(-30,  19),   S( -3,  27),   S(-23,  12),
            S( 12, -20),   S(-47,   9),   S( 17, -16),   S(-35, -39),   S(-30,  12),   S(-15,  19),   S(-46,  26),   S(-25,  46),
            S(-42, -23),   S( 15, -10),   S(-18,   8),   S( 26,  -1),   S( 22,  -1),   S(  4,  13),   S(  0,  23),   S(-31,  54),
            S( -4,  16),   S(  1,  12),   S(-10, -12),   S(-13, -24),   S(-24,  21),   S(  9,  35),   S(-15,  16),   S( 36,  35),

            /* bishops: bucket 6 */
            S( 13,   0),   S(-23,  19),   S(-47,  33),   S(-30,  20),   S(-40,  44),   S(-50,  22),   S(-38,  19),   S(-38,  36),
            S(-55,  10),   S(-20,  -8),   S(-77,  42),   S( -8,  15),   S(-81,  42),   S(-60,  28),   S(-46,  25),   S(-27,  24),
            S(-26,  19),   S(-57,  20),   S(-20,  21),   S(-50,  37),   S(-11,  23),   S(-10,   2),   S(-45,   5),   S(-22,  37),
            S(-47,  26),   S(-80,  41),   S(-10,  15),   S( 21,  32),   S( 12,  31),   S( 20,   4),   S( 30, -11),   S(-10,  11),
            S(-60,  43),   S(-35,  38),   S(-11,  19),   S( 46,  19),   S(-42,  24),   S(-14,   5),   S( 35,  -5),   S(-22,   6),
            S(-43,  51),   S(-24,  16),   S(-33,   8),   S(-21,   5),   S( 19,  -1),   S(-11,   8),   S( 22,  -6),   S( -1, -14),
            S(  0,  41),   S(-106,  62),  S(-44,  24),   S(-17,  24),   S( -3,  11),   S( 41,  -1),   S(  7,  32),   S(  0,   3),
            S(  8,  36),   S(-15,  30),   S(-26,  45),   S( 18,  17),   S(-19,  14),   S( 25, -12),   S( -7,  18),   S(  3,  22),

            /* bishops: bucket 7 */
            S( 16, -30),   S(-71, -23),   S(-52, -15),   S(-44, -15),   S(-54,   4),   S(-70,  14),   S(-63, -47),   S(-41,   0),
            S(-11, -57),   S( -9, -30),   S( 26, -43),   S(-47,   4),   S(-33,  -1),   S(-45,   2),   S( 10, -53),   S(  7, -28),
            S(-11, -62),   S(-25,   2),   S( -6,  -2),   S( 28, -13),   S(-23,   0),   S( -5, -19),   S(-66,   7),   S(-56,  12),
            S( -7, -40),   S(-71,  32),   S(-46,  13),   S(-24,  23),   S(101,  -5),   S( 22,  -9),   S( 52, -50),   S(  1, -12),
            S(-12, -10),   S(-10,  10),   S(-78,  50),   S( -3,   1),   S( 73, -22),   S( 61,   3),   S(-17,  -4),   S(-19,   9),
            S(-87,  46),   S(-55,  70),   S(-31,  18),   S(-108,  45),  S(-29,  -1),   S( 49,  -4),   S(  6,  34),   S(-65, -69),
            S( -6,   9),   S(-22,  30),   S(-83,  46),   S(  8,  16),   S( -1, -19),   S(  4,  -5),   S( -1,   2),   S(-10, -28),
            S(-14,  -5),   S(-16,   0),   S(-11,  33),   S( 23, -19),   S(-17,  -7),   S( -2, -16),   S( 21, -31),   S(-10,  17),

            /* bishops: bucket 8 */
            S( 41,  89),   S(  0, -64),   S( -8,  -4),   S(  5,  45),   S(-17,  -6),   S(-19, -26),   S(-27, -29),   S(-11,  -1),
            S(  8,  -1),   S( 12,  30),   S( 18,  11),   S(  1,  29),   S(-22, -24),   S( -6,   6),   S(-22,  -5),   S(-13,   5),
            S( -4,  11),   S( -8, -17),   S( 28,  58),   S(  9,  11),   S( 23,  18),   S( 13,  33),   S(-35, -12),   S(-27, -14),
            S( -8,  -1),   S( 32,  10),   S(  3,  -2),   S( 22, -18),   S( 22,  36),   S( 10,   9),   S( -7,  19),   S( -4, -17),
            S( 12,  16),   S( 54,  67),   S(  9, -20),   S(  1,   6),   S( 13,  22),   S(-14,  11),   S(-12, -22),   S(  9,  21),
            S(-12, -43),   S(  8,  25),   S(  0,  54),   S( 21,  19),   S( 18,  45),   S( 44,  34),   S( -2,  75),   S(  9,  59),
            S(  3,  32),   S(-22, -49),   S( 30, -11),   S( 26,  12),   S(  3, -15),   S(  2,  44),   S( 15,  23),   S(-13,  -7),
            S( -4,  11),   S(  5,  -9),   S( -7,  -1),   S(  2,  11),   S( 42,  14),   S( 25,   6),   S(  9,  72),   S( 38,  10),

            /* bishops: bucket 9 */
            S( 27,  81),   S(  3,  -7),   S( -8,   9),   S(-45,  -4),   S(-28, -18),   S(-10,  -7),   S( -6,   1),   S(-10, -18),
            S( -7, -14),   S(  5,  24),   S( 10,  -1),   S(-23,  -5),   S(-32,  34),   S(-18, -27),   S(-37, -16),   S(-15, -23),
            S(  1,   4),   S( 22,   0),   S( 15,   8),   S( -3,  34),   S(  9,   8),   S( -8,  -5),   S( -4,   9),   S(-18, -17),
            S(  2,  30),   S(  9,  -5),   S( 13, -12),   S( 38, -12),   S(-25,  15),   S(-18,  22),   S(-16,  12),   S(  5,  21),
            S( 10,  31),   S( 21,  19),   S( 15,  23),   S( 12, -24),   S( 19,  21),   S(  5,   8),   S(-15,  30),   S(-18, -11),
            S( 25,   7),   S( -8,  16),   S( 17,   4),   S( 20,   7),   S( 43, -63),   S(-18,  50),   S( 12,  30),   S( 10,  26),
            S( 21,   9),   S( -7,  29),   S(  6,  11),   S( 19,  -3),   S( 21, -10),   S( 33,   5),   S( 12,  28),   S( 27,  35),
            S( 16,  35),   S(-10, -52),   S( 11,  29),   S(  8,   9),   S(  1,  58),   S( 13,  10),   S( 28, -15),   S( 38,  53),

            /* bishops: bucket 10 */
            S(  3, -35),   S( 17,  24),   S(-17, -31),   S(-25,   5),   S(-52, -18),   S(-47, -53),   S(  6, -21),   S(  6,  65),
            S(-24, -28),   S(  8, -29),   S( -8, -33),   S( -7, -46),   S(-63,   7),   S(-23, -16),   S(-30,  18),   S(  1,  32),
            S(  6, -16),   S(-31, -31),   S(  2, -34),   S( 27,  19),   S( -7,  24),   S( -7,   1),   S( -2,  -1),   S( 15, -38),
            S(-20,  -9),   S(-33,  16),   S( -8, -39),   S(-15,  23),   S(  1,  34),   S( 21,   6),   S( 25,   9),   S( -1, -24),
            S(  7,  17),   S(-25,  38),   S(-14,  -7),   S(-31,  51),   S( 44,  -5),   S( 19,  29),   S( 29, -17),   S( 26,  14),
            S( -3,  22),   S( 17,  46),   S(  4,  13),   S( 27,   8),   S( 35, -48),   S(  7,   4),   S( 26,  28),   S( 21,  -1),
            S( 20,  38),   S( -9,  12),   S( 38,  -2),   S( -4,  24),   S(-21,  22),   S( -4, -28),   S(  5,  16),   S( 21,  31),
            S( 19,  45),   S( 20,  21),   S( 54,  -2),   S(  5,  19),   S( -6,  37),   S(  2,  14),   S( -1,  14),   S(  1, -13),

            /* bishops: bucket 11 */
            S( 25,  13),   S( -2,  23),   S( 15, -15),   S( 15, -16),   S(-37, -15),   S( -3, -24),   S(-18, -29),   S(  2,   0),
            S(  4,  -7),   S(  6,  -7),   S( -2, -14),   S( -1, -21),   S(-12,  -7),   S(-71, -34),   S(-31,   4),   S(  7, -25),
            S(-13, -24),   S(  8,   5),   S( -6,  -4),   S(-58,  -8),   S( -7,  -7),   S(  4,  20),   S( 10,  11),   S( -4, -22),
            S(  5,   7),   S( -8, -44),   S( 33, -27),   S(-50,  20),   S(  7,  -6),   S( 33,  38),   S( 21,   4),   S(-12, -45),
            S(-18, -15),   S(-15, -23),   S(-33,  26),   S(-26,  13),   S(-38,  33),   S( 46, -17),   S( 10, -29),   S(  6,  -7),
            S( -9,  -6),   S(-12, -19),   S( -3,  27),   S(  5,  47),   S( 18,  31),   S( 22,   6),   S( -3, -24),   S(-14, -11),
            S(-10, -43),   S( 39,  46),   S( 16,  45),   S( 38,  -1),   S( 14, -15),   S( -9,  -5),   S( -8, -11),   S(-11,  -4),
            S( 30,  21),   S(  4,  -9),   S( 34,  43),   S( 21, -40),   S( 23,  24),   S( 13,  31),   S( -7, -25),   S( -1, -10),

            /* bishops: bucket 12 */
            S( -5,  -7),   S(  2,   4),   S( -3,  11),   S(  1, -12),   S( -7,  -5),   S( -1,  -2),   S( -5, -16),   S( -4,  -6),
            S(  1,  -5),   S(  7,   9),   S( 10,  10),   S(  1,  18),   S( -3,  -3),   S( 10,  11),   S(-16, -32),   S(  0,   1),
            S(  3,  -8),   S(  3, -10),   S( 25,   9),   S( 28,  13),   S( -4,  30),   S( -6, -20),   S( -5,  -3),   S( -1,   6),
            S( 10,   1),   S( 11,   5),   S( 20,  26),   S( 22,  46),   S(-11, -12),   S( -7,  -3),   S(  6,  17),   S(  3,  19),
            S( 13,   4),   S(  7,   1),   S( -5,  22),   S( 15,  -7),   S( 22,  48),   S( 16,  38),   S(  1, -11),   S(  1,   2),
            S(  1,  -7),   S(-10, -18),   S( -5,  24),   S(  1,   1),   S( 23,   7),   S( 11,  -1),   S(-11,  -4),   S( -6, -15),
            S( -3,  -7),   S( 10,  25),   S(  2,  13),   S(  1,  -7),   S( 14, -11),   S( 16,  29),   S( 15,  43),   S(  3,   5),
            S(  0,   2),   S( -5, -10),   S(  1,  -1),   S( -3, -17),   S(  2,   5),   S(  4, -18),   S( 10,  -8),   S(  5,   3),

            /* bishops: bucket 13 */
            S( -6, -37),   S( -3,   3),   S( -4, -15),   S( -8, -21),   S(  9,  -8),   S(-29, -36),   S(-22, -28),   S( -6, -11),
            S(-10, -29),   S(  0,   4),   S(  0,   6),   S( 14,  14),   S( -9, -26),   S(  8,  -5),   S(-15, -26),   S( -1,  -4),
            S(  7, -26),   S( 33,   4),   S( 20,   2),   S( 11,  14),   S( -7,  15),   S( -5,  19),   S( -8, -18),   S(-20, -28),
            S( 19,  14),   S( 40,  11),   S( 31,  41),   S(-28,   5),   S( 15,  84),   S(  4,  19),   S( 29,  45),   S(  5,  14),
            S( 12,  -6),   S(  8,  -8),   S(  5, -22),   S( 13,  -8),   S( 11,  11),   S(  9, -11),   S( 17,   5),   S(  0,   1),
            S(  5,   9),   S( -5,   2),   S(  2,  -3),   S( 10,  -2),   S(  5,   0),   S(-11, -36),   S( -8, -19),   S( 13,   7),
            S(  9,   3),   S( -2,  -7),   S( -1,  -6),   S(  4,   2),   S( -1,  10),   S( 13,  -1),   S( 12,   7),   S(  9,  12),
            S(  1,  12),   S( -3,  -4),   S(  5,  23),   S(  2,  12),   S( 11,  20),   S( -2, -24),   S(  5, -16),   S( 11, -13),

            /* bishops: bucket 14 */
            S( -5, -11),   S(  2,   9),   S( 19,  12),   S(  4,  23),   S(-21, -21),   S( -5,  -6),   S(-12, -14),   S(-12,  11),
            S( -4,  -5),   S(  3,  15),   S( 12,  20),   S(-13, -20),   S( 12,   6),   S(  9,  21),   S( -1,  43),   S(  7,  40),
            S( -3, -13),   S(-10, -35),   S(  2,  12),   S(  3,  26),   S(  8,  38),   S( 25,  26),   S( 11,  36),   S(  7,  37),
            S(  6,  45),   S(  6, -17),   S( -7,  11),   S(  0,  39),   S( 12,  36),   S( 26, -19),   S( 27,  45),   S(  7,  -3),
            S(  9,   2),   S(  3,  12),   S( 13,  15),   S(  7,  -6),   S(-12,   1),   S(  8,  14),   S( 29, -20),   S(  2, -12),
            S(  3,  -5),   S( 23,  36),   S( 13,  18),   S( 14,  35),   S( 10,  15),   S(  0,  -8),   S( -6,  23),   S( 21,   5),
            S( 14,  37),   S( 17,  17),   S( 10,   8),   S( 11,  32),   S(  7, -23),   S(  9,  48),   S(  7,   8),   S( 10,  10),
            S( 14, -19),   S( 10,  15),   S(  4,  14),   S(  8,  15),   S( -1,   2),   S(  6,  16),   S(  5,   9),   S(  4,   9),

            /* bishops: bucket 15 */
            S( -2,  -3),   S(  4,   3),   S( -7, -13),   S(  1,   1),   S( -6, -42),   S(-14, -33),   S( -5,   1),   S(  1,   6),
            S(  8,  19),   S( -8, -12),   S( 11,   8),   S(  4,  18),   S(  7,  -9),   S( -3, -12),   S(  0, -14),   S( -3, -11),
            S(  5,   9),   S(  4,  -7),   S(  1,   2),   S(  8,  23),   S(  9,  17),   S( 16,  41),   S( 12,  13),   S( 10,   2),
            S(  4,  -7),   S( 17,   8),   S(  7,  31),   S(-14,   2),   S( 14,  13),   S( 19, -17),   S( 12,   0),   S(  8,  14),
            S( -9, -16),   S(-11,   6),   S(  8,  38),   S( 29,  59),   S( 22,  24),   S( 10,  12),   S(  4,  -3),   S( -8,  -9),
            S( -1,  24),   S(  2,  15),   S( 16,  66),   S(  9,  13),   S( 16,   4),   S( 11,  17),   S(  0,  10),   S(  1,  -3),
            S(  6,   3),   S( 15,  45),   S(  0,  24),   S( 13,  14),   S(  6,   0),   S( -4,  -6),   S(  0,  -4),   S(  0,   0),
            S(  5,  -1),   S( 10,  12),   S(  5,   4),   S( 11,  10),   S(  5,  19),   S( -3, -13),   S(  1,   1),   S(  3,  -7),

            /* rooks: bucket 0 */
            S(-19,  16),   S(  8, -14),   S(-13,  -6),   S(-15,  15),   S(-36,  68),   S(-22,  43),   S(-50,  63),   S(-66,  63),
            S(-15, -16),   S(-22,  40),   S(-41,  22),   S(-10,  29),   S(-18,  56),   S( -4,   5),   S(-28,  14),   S(-30,  68),
            S(  3,   1),   S( -5, -22),   S( -8,  -5),   S( -8,  19),   S(-44,  65),   S(-23,  19),   S(-10,   5),   S(  0,  22),
            S( -8,   0),   S( 46,   0),   S(-39,  38),   S( 29,  -2),   S( 19,  45),   S(-29,  43),   S(-27,  63),   S(-25,  43),
            S( 40, -49),   S( 47, -10),   S( 37,  -2),   S( 27,  19),   S( 40,   9),   S( 16,  71),   S( 54,  32),   S(  0,  66),
            S( 63, -50),   S( 85,  33),   S(114, -39),   S( 95,  21),   S( 27,  70),   S( 26,  55),   S( 13,  78),   S(-47,  95),
            S( -2,  13),   S( 63,  49),   S(122,   6),   S( 45,   9),   S( 64,  41),   S( -1,  73),   S(-29,  82),   S(-11,  71),
            S(  0, -24),   S(  3, -22),   S( 13,  47),   S( 24,  12),   S( 50,  43),   S( 50,  24),   S( 47,  -6),   S( 49, -55),

            /* rooks: bucket 1 */
            S(-52,  52),   S(-28,  13),   S(-23,  21),   S(-51,  35),   S(-45,  49),   S(-43,  44),   S(-58,  77),   S(-74,  75),
            S(-43,  17),   S(-24, -16),   S(-29,  33),   S(-26,  23),   S(-44,  25),   S(-52,  42),   S(-25,  23),   S(-41,  51),
            S(-21,  11),   S(-16, -14),   S(-36,   4),   S(-30,  19),   S(-29,  10),   S(-48,  33),   S(-52,  60),   S(-14,  31),
            S(-35,  57),   S( 20,  -1),   S( -5,  25),   S(-24,  13),   S(-46,  46),   S(-55,  76),   S(-41,  72),   S(-71,  94),
            S(-22,  38),   S( 11,  -1),   S( 26,  13),   S( 70, -23),   S(  6,  18),   S( -3,  80),   S(  2,  64),   S(-22,  95),
            S( 75,  25),   S( 55,   3),   S( 48,  21),   S(-15,  37),   S( 26,   3),   S(  6,  62),   S(  8,  46),   S( 49,  60),
            S(  5,  62),   S( 14,  30),   S( 22,  34),   S( 14,  37),   S( 41,  24),   S( -2,  43),   S( 54,  53),   S( 64,  68),
            S( 41, -12),   S( 29,  -9),   S( -1, -11),   S(-35, -23),   S(  8,  -4),   S( 27,  28),   S( 22,  36),   S( 70,   2),

            /* rooks: bucket 2 */
            S(-57,  71),   S(-46,  60),   S(-47,  61),   S(-38,  24),   S(-31,  32),   S(-38,  19),   S(-39,  19),   S(-72,  66),
            S(-57,  57),   S(-68,  61),   S(-46,  48),   S(-48,  35),   S(-47,  38),   S(-41,  19),   S(-23,  12),   S(-64,  55),
            S(-43,  64),   S(-25,  55),   S(-45,  40),   S(-27,  38),   S(-40,  23),   S(-48,  40),   S( -8,   8),   S(-20,  36),
            S(-36,  83),   S(-18,  73),   S(-47,  65),   S(-48,  41),   S(-60,  49),   S(-17,  17),   S( -8,  16),   S(-12,  41),
            S( -9,  81),   S( -4,  65),   S( 10,  67),   S(-22,  50),   S(-19,  52),   S( 36,  21),   S( -2,  33),   S( -6,  71),
            S( 13,  93),   S( 30,  73),   S( 34,  64),   S(-15,  49),   S( 53,   9),   S( 35,  62),   S(122,  -4),   S( 73,  59),
            S( 58,  63),   S( 15,  66),   S( 17,  56),   S( 13,  37),   S( 27,   6),   S( 45,  44),   S(-35,  83),   S( 35,  71),
            S( 10,  43),   S( 27,  42),   S( 35,  24),   S(-35,  30),   S(-19,  21),   S( 20,  15),   S( 38,  10),   S(-19,  66),

            /* rooks: bucket 3 */
            S(-16,  76),   S(-10,  66),   S(-20, 102),   S(-15,  86),   S( -3,  53),   S(  0,  41),   S( 19,   9),   S(-10,  10),
            S( -2,  53),   S(-17,  73),   S(-16,  98),   S( -8,  87),   S( -8,  60),   S( 15,   8),   S( 42, -14),   S( 16,  -4),
            S( 12,  55),   S( -8,  80),   S(-18,  85),   S( -4,  93),   S( 19,  36),   S(  1,  33),   S( 39,   9),   S( 35,   3),
            S( -2,  93),   S(  0, 114),   S(-23, 116),   S( -6,  99),   S( -6,  64),   S(  9,  52),   S( 39,  27),   S( 11,  25),
            S(  7, 110),   S(-23, 124),   S( 15, 113),   S( 17, 110),   S( 15,  87),   S( 40,  58),   S( 56,  27),   S( 28,  49),
            S(  9, 121),   S( 20, 115),   S( 21, 119),   S( 43, 107),   S( 98,  50),   S(129,  41),   S(105,  29),   S( 42,  36),
            S( 21, 115),   S( 14, 110),   S( 26, 121),   S( 22, 116),   S( 26,  99),   S( 91,  31),   S( 96,  87),   S(167,  72),
            S( 98, -25),   S( 58,  43),   S( 15,  93),   S( -1,  86),   S(  5,  63),   S( 84,  53),   S( 47,  42),   S( 96,  16),

            /* rooks: bucket 4 */
            S(-29, -22),   S( -9, -11),   S(-21,  -5),   S(-34,   5),   S(-40,  -7),   S(-46,  46),   S(-33, -13),   S(-60,  27),
            S(-22, -50),   S(-44, -15),   S( -8, -20),   S( 22, -46),   S( 24, -27),   S(-33,  10),   S(-38,   1),   S( 13,  12),
            S(-18, -13),   S(-28, -22),   S(-34,  -8),   S(-15, -37),   S(-62,  -3),   S(-64,  30),   S(-16,  16),   S(-59,  15),
            S(-60, -43),   S( 11, -19),   S(-16,   1),   S(  4,   1),   S( 30, -16),   S(  7,   9),   S(-28,   5),   S( 16, -10),
            S( -5, -51),   S(  9, -57),   S( 27,  -3),   S( 56, -20),   S( 48,  14),   S( 94,  31),   S( 21,  17),   S( 18,  25),
            S(-15, -41),   S( 18,  17),   S( 18,  -4),   S( 46,  15),   S( 32,  22),   S(  1,  32),   S( 25,  16),   S( 12,  46),
            S(-18, -15),   S( 34,  13),   S( 49,   5),   S( 62, -19),   S( 49, -22),   S(-23,  12),   S( -2,  -4),   S(  8,   9),
            S( 15, -29),   S(-12,  10),   S( 40, -20),   S( 31,  -4),   S( 49,   6),   S( 12,   3),   S(  8,  17),   S( -6,  33),

            /* rooks: bucket 5 */
            S(-37,  32),   S(-11, -11),   S(-22,  11),   S( 12,   3),   S( -4,  18),   S( -1,  18),   S(-45,  67),   S(-40,  39),
            S(-14,  -7),   S(-47, -14),   S( 46, -64),   S( 25, -20),   S(-21,  13),   S(-31,  14),   S(-29,  19),   S(-16,  27),
            S(-79,  32),   S(-50,  10),   S( 29, -41),   S(-14,  -7),   S(-29,  -8),   S( 49, -21),   S(-71,  43),   S(-53,  18),
            S(-25,  34),   S(-15,  23),   S( 52, -29),   S( 42, -11),   S( 63, -14),   S( -8,  44),   S( 20,  22),   S(-20,  53),
            S( 58,  11),   S( 11,  10),   S( 16,  14),   S( 21,   3),   S( -7,  27),   S( 66,   8),   S( 35,  19),   S( 57,  32),
            S( -8,  33),   S(-17,  15),   S( 11,   4),   S( -4,  -5),   S(  1,   8),   S( 20,  37),   S( 84,  13),   S( 34,  31),
            S( 50,   4),   S( 64, -16),   S(  5,   7),   S( 42,  19),   S( 68, -18),   S( 92, -31),   S(114, -21),   S( 57,  23),
            S( -3,  45),   S( 55,   2),   S( 49,   0),   S( 12,  11),   S( 27,  22),   S( 42,  18),   S( 56,  30),   S( 37,  42),

            /* rooks: bucket 6 */
            S(-71,  69),   S(-41,  36),   S(  4,   3),   S(-29,  21),   S( -7,   7),   S( 13, -15),   S( 26, -19),   S( -8,   2),
            S(-45,  18),   S( 27, -10),   S( -8,   8),   S(  8,  -4),   S(  5,  -6),   S( -1, -13),   S(-34,   1),   S(-40,  28),
            S(-90,  46),   S(-24,  22),   S(  3,  -3),   S(  8,  -1),   S(-12,  -9),   S( 14, -15),   S(-15,  -7),   S( 31, -17),
            S(-65,  68),   S(-18,  45),   S( 34,   4),   S( 59,  -8),   S( 44, -23),   S( 35,  -8),   S( 34, -10),   S( 14,  28),
            S(  1,  52),   S( 45,  25),   S(107,  12),   S( 64,   0),   S( 40, -12),   S( 18,  25),   S( 52,  -4),   S(105,  -1),
            S( 93,   8),   S( 47,  13),   S( 81,   3),   S( 50, -23),   S(  1, -15),   S( 23,  38),   S( 46,  -1),   S( 27,  29),
            S( 57,  16),   S(152, -31),   S( 99, -17),   S(105, -39),   S( 42,  -6),   S( 41,  -3),   S( 48, -16),   S( 77, -13),
            S( 89, -22),   S( 28,  36),   S(-19,  34),   S( 84, -17),   S( 70,  -4),   S( 38,  30),   S( 76,  14),   S( 90,  -2),

            /* rooks: bucket 7 */
            S(-105,  35),  S(-86,  39),   S(-71,  49),   S(-84,  57),   S(-36,  12),   S(-30, -11),   S(-43,   9),   S(-58, -18),
            S(-88,  35),   S(-40,  17),   S(-69,  35),   S(-70,  40),   S( -7, -27),   S(-14, -24),   S( 19,  -4),   S( 13, -70),
            S(-64,  26),   S(-69,  28),   S(-24,  11),   S(-49,  31),   S(-23,  -2),   S(-17,  -3),   S( 66, -48),   S( 20, -71),
            S(-55,  37),   S(-10,  17),   S(-16,  23),   S( 68, -23),   S(-19,   3),   S( 73, -32),   S( 26,  -9),   S( 16, -11),
            S( -2,  44),   S( 46,   8),   S( 50,  10),   S( 97,  -9),   S(161, -58),   S( 95, -47),   S(108, -37),   S(-18, -43),
            S(  4,  25),   S( 18,  11),   S( 86,  -6),   S( 95, -17),   S( 68, -16),   S( 31,   0),   S( 21,  18),   S( -3, -43),
            S(  8,   4),   S( 14,  -1),   S( 74,  -6),   S(113, -51),   S(145, -61),   S(142, -55),   S( 67,   1),   S(  1, -30),
            S(-36, -11),   S( 15,  10),   S( 89, -14),   S( 42,  -7),   S( 59, -20),   S( 63, -12),   S( 10,  22),   S( 26, -20),

            /* rooks: bucket 8 */
            S(-32, -78),   S(-27, -34),   S( -2,  -6),   S( 11,  26),   S( -9, -51),   S(-34,  19),   S( -8, -49),   S(-37, -20),
            S(-28, -80),   S(-21, -38),   S(-20,   1),   S(-40, -58),   S(-20, -45),   S(-13,  -9),   S(  1,   3),   S(-29, -49),
            S( 10,  -7),   S( -7, -12),   S( 27,  17),   S(-22,   8),   S(-20,  55),   S(  3,  24),   S(  3,  45),   S(-11,  18),
            S( -5, -27),   S( -6,  -3),   S(  8,  21),   S( 16,  35),   S( -4,  25),   S( 39,  26),   S(  6,  15),   S(-12, -32),
            S(-11, -57),   S( 15,  36),   S( 27,  34),   S( 25,  64),   S( -2,  15),   S( -7,  -5),   S(  3,  41),   S(  4,  39),
            S(-22,  12),   S(  6,  13),   S(-11,  30),   S( -2, -19),   S( 10,  30),   S(-20,  17),   S( -6,   2),   S(  0,  19),
            S( 20,  50),   S(-13,  31),   S(  8,  10),   S( 15,  46),   S( 14,  16),   S( 23,  48),   S( 17,  45),   S( -4,  39),
            S(  5,  -2),   S(  9,  -7),   S(-23,  54),   S( 35,  47),   S(  3,  33),   S( 10,  37),   S( -1,  47),   S( 11,  66),

            /* rooks: bucket 9 */
            S(-20, -67),   S( -4, -68),   S( 21, -105),  S(-15, -64),   S(-12, -61),   S(-18, -37),   S(-31, -43),   S( -7, -24),
            S(-46, -35),   S(-26, -68),   S(-12, -58),   S(-47, -79),   S(-41, -40),   S(-26,  24),   S(-32, -68),   S(-25, -18),
            S(-19, -10),   S(-21,  -6),   S( 17,  15),   S(  3,   8),   S(-10, -16),   S( -5, -10),   S(  5,   6),   S( -8,  28),
            S(-11,   0),   S(  8,  -1),   S(  4,   7),   S(  8,  30),   S(-24, -49),   S( 17,   8),   S( -2,   8),   S( 17, -13),
            S( -4,  -4),   S( -5, -13),   S(-14, -50),   S(-18, -13),   S(-32, -23),   S( -3,  -1),   S(-23, -18),   S( -4,  15),
            S( -8,   8),   S(-19, -22),   S(-14,  -3),   S( -6,  36),   S(-10,   0),   S(-22,  -5),   S( 14,  21),   S( -8,  17),
            S( -4,  33),   S(  6,   9),   S(  6, -34),   S(  1,  21),   S(  7, -14),   S( 31,  -7),   S(  1,   0),   S( -6, -31),
            S( -3,  37),   S( -9,  21),   S(  4,   9),   S(-22,  36),   S(-19,  40),   S( 13,  56),   S(  2,  18),   S( 17,  28),

            /* rooks: bucket 10 */
            S(-34, -28),   S(-66, -14),   S( -8, -46),   S(  2, -39),   S(-33, -34),   S(  1, -96),   S(  6, -51),   S(-30, -29),
            S(-38,  -8),   S(-53,  -3),   S(-37, -27),   S(-25, -58),   S(-31, -42),   S(-34, -63),   S(  4, -44),   S(-28, -67),
            S(  3, -30),   S(-21, -12),   S(-16, -13),   S(-53, -54),   S(-12,  -1),   S( -1,  -2),   S( -9, -30),   S(-23, -16),
            S(-50,  -4),   S(-36, -18),   S( -1, -34),   S(-18,  -7),   S( -1, -12),   S( 11,  20),   S( -7, -38),   S(  4, -35),
            S(  7,   0),   S(-13,  -7),   S(-10,  -5),   S( -7, -43),   S( 13,  19),   S( 19,  37),   S( -8, -32),   S(-19, -51),
            S( -7,  19),   S( 19,  -5),   S(  1, -19),   S(  3, -32),   S( 12,   6),   S(  8, -14),   S(-34, -24),   S( -3,  10),
            S(-32,   0),   S(  8, -25),   S(  9, -29),   S( 11, -18),   S(  2,  -4),   S( -4,  -5),   S(-20, -38),   S( -6,  -6),
            S( 10, -12),   S( 14,  38),   S(  4,  23),   S(  6,  -4),   S(-16,  42),   S(-26,  14),   S(-20,  13),   S(  4,  17),

            /* rooks: bucket 11 */
            S(-58, -33),   S(-48,  17),   S(-19,   8),   S(-17,   4),   S(-60,  -3),   S(-24, -22),   S(  7, -55),   S(-28, -49),
            S( -8, -20),   S(-22, -23),   S(-57, -13),   S(-81,  16),   S(-13,  -8),   S( 10,  -6),   S(-35, -35),   S(-33, -70),
            S(-41,  29),   S(-12,  10),   S(-24,  27),   S(-34,  10),   S( -7, -11),   S(  0,  -2),   S( 24, -42),   S(-22,  22),
            S(-34,   6),   S(  4, -16),   S(-21,  15),   S(  6,  11),   S( 38,  25),   S(-18, -13),   S(  5,  10),   S( -5, -17),
            S(-13,  -4),   S( 10, -24),   S(  6,  -9),   S( -7,   4),   S( 34, -10),   S(-10,  -5),   S( 17,  39),   S(-18, -49),
            S( -8, -28),   S( -6, -24),   S( 27,   7),   S( 11, -24),   S( -6, -37),   S( 16,   2),   S( -7,  33),   S(-19,  -8),
            S(  3,  21),   S(-16, -12),   S( 19,  -5),   S(-13, -29),   S( -4,  12),   S( 14,  19),   S( -6,  21),   S(-23, -15),
            S(-11,  23),   S( 19,  54),   S(  3,  14),   S( 16,  50),   S(-10,  29),   S(  5,  13),   S( -1,  12),   S(-19,  12),

            /* rooks: bucket 12 */
            S(-42, -106),  S(  8,  10),   S(-12, -51),   S(-24, -49),   S( -4, -28),   S( 17,   3),   S(-18, -37),   S(-19, -57),
            S(  7,   4),   S(  3,  10),   S(  7,  28),   S(  7,  15),   S(  9,   6),   S( 23,  15),   S(  3,  11),   S(-25, -45),
            S( -8, -13),   S( 14,  51),   S(  2,   1),   S( 13,   6),   S(  3,  -9),   S( 10,  -3),   S(  5,  27),   S(  3,  44),
            S( -8,   5),   S(  5, -17),   S(  8,  24),   S(  3,  12),   S( 14,  -9),   S(  4,  14),   S( 12,  26),   S( -1,  11),
            S( 16,  -5),   S( 11,  18),   S( 19,  63),   S( -2, -16),   S( 10,  27),   S(-12, -35),   S(  8,  23),   S(  3,  -2),
            S(  3,  -3),   S( -7, -31),   S(  4,  41),   S(  4,  26),   S(  8,  30),   S(-10, -60),   S( 17,  44),   S(  0,   3),
            S( -2,  27),   S(-23,  11),   S(  9,  13),   S(  2,  24),   S( -7,   2),   S(  7,   3),   S(  8,  28),   S( -5,  15),
            S(  3,   8),   S( -8,  39),   S(  0,  35),   S( 11,  22),   S(  8,  10),   S( -1,  17),   S( -6,  -1),   S(  2,   7),

            /* rooks: bucket 13 */
            S(-35, -37),   S(-29, -64),   S(-18, -36),   S(-14, -26),   S(-34, -57),   S( 12,  27),   S(-31, -15),   S(-33, -62),
            S( -5,   0),   S( -6,  -9),   S(  0,   0),   S(  1,   0),   S( 18,  34),   S(  2,  12),   S( 22,  16),   S( -6,  -9),
            S(  2, -10),   S(-22,  -4),   S( -6,   6),   S(  4,  12),   S(  5,  30),   S( 22,  13),   S( 16,  52),   S( -5, -24),
            S( 12,  11),   S(  0,  17),   S(  3,  19),   S( 14,  -1),   S( 10,  23),   S(  0,   7),   S( 18,  22),   S(  0,  22),
            S(  4,  25),   S( -7,   5),   S( -5,  12),   S(  7,  -5),   S(-15,  10),   S(  7,  11),   S( 15,  37),   S(  3,   1),
            S(  4,  15),   S(  0,  -3),   S(-13,   1),   S(-25, -22),   S(-13, -17),   S( 15,  -3),   S(-10,   5),   S( -5, -15),
            S( -2,  -8),   S( -2,  -2),   S( -7, -42),   S(  3,  30),   S(-13, -10),   S( 11,  33),   S(  5,   9),   S( -1, -33),
            S(  4,  26),   S(-10,  -4),   S(-12,  -5),   S( 23,  46),   S(-10,  -1),   S(  9,  41),   S( -1,  45),   S( -2,  -8),

            /* rooks: bucket 14 */
            S(  1, -37),   S(-32, -31),   S(-25, -31),   S(-11, -30),   S(-17, -47),   S( -3, -30),   S(-40, -64),   S(-22, -41),
            S(  1,  52),   S(  7,   6),   S(  5,  12),   S(  4, -15),   S(  4,  -5),   S( -3,   3),   S( -5,  11),   S(  0,   5),
            S(  7,  24),   S( -9,  43),   S(  4, -11),   S( -7,  -3),   S(  4,   2),   S(  5,  -1),   S(  6,  11),   S(-14, -44),
            S( -1,  27),   S( 19,  37),   S( -3, -13),   S(  5,  -6),   S(-18, -11),   S( -8, -23),   S( 12,  27),   S( -8, -16),
            S( 16,  22),   S( 30,  43),   S(  0,   4),   S( -8,   0),   S( -3, -23),   S(  7,  36),   S( -4,  35),   S(-22, -34),
            S(  5,   8),   S(  3,   8),   S(  8,   8),   S(  1, -11),   S(  2,  25),   S(-21, -13),   S(-14, -24),   S( -5,  -4),
            S(  0, -12),   S(  5,  24),   S(-12, -62),   S(-27, -51),   S(  3,   9),   S(  7,  15),   S( -5,  -4),   S(  2,  45),
            S(  9,   0),   S( -1,  16),   S(-12, -20),   S(  4, -15),   S(-13,  -4),   S( -6, -70),   S( -7, -31),   S( -2,  25),

            /* rooks: bucket 15 */
            S( -8, -24),   S(-13, -26),   S(-49, -61),   S(-29, -84),   S(-11, -48),   S(-19, -45),   S(  0,  -6),   S(-21, -56),
            S( 23,  59),   S( -7,  16),   S( -9, -10),   S( 11,  18),   S(-17, -43),   S(  7,  -8),   S(  9,  18),   S(  3,   6),
            S( 13,  32),   S( 14,  -2),   S( 10,  19),   S(  9, -15),   S( 13,  22),   S(  0,   4),   S(  5,  24),   S(  6,  14),
            S( -1,   0),   S(  6,   3),   S( 29,  48),   S( -3,  -1),   S( 21,  56),   S(  0,  -9),   S( -5,  12),   S(  4, -12),
            S(  8,  20),   S(  8,  12),   S( -1,   3),   S( -2,   2),   S(  8,  22),   S(  7,   5),   S(  5,  37),   S( -1, -30),
            S(  9,  13),   S(  8,   2),   S(  6, -16),   S(  0,  13),   S(-11, -14),   S(-19,  19),   S( -5,   5),   S(  3,  23),
            S(  6,  -7),   S(-10,   9),   S(  7,  24),   S( 10,  21),   S(  4,  -4),   S(  9,  36),   S( -8,  15),   S( -8, -35),
            S(  0,  11),   S( -3,  26),   S( 11,  24),   S(  9,  38),   S(-12, -19),   S(-10,  -4),   S( -1,  12),   S( -3,   2),

            /* queens: bucket 0 */
            S( -3, -14),   S(-26, -103),  S(-19, -91),   S(  9, -102),  S(  3, -44),   S( 10, -64),   S(-58, -45),   S(-22, -28),
            S( -7, -35),   S( 20, -102),  S(  8, -70),   S(-12, -15),   S(  1,  -6),   S( -3, -34),   S(-23, -11),   S(-33,  -2),
            S(-21,  40),   S(  3, -14),   S( 18, -50),   S( -5,  -8),   S(-16,  32),   S(  0,  -4),   S(-32,   7),   S(-76, -55),
            S(-25,  32),   S( 26, -15),   S(-15,  23),   S( -8,  59),   S(  3,  70),   S(-24,  42),   S(-41,  42),   S( -9,   7),
            S(-17,  -9),   S( 13,  86),   S(  6,  62),   S(-10,  59),   S( -8,  52),   S(-18, 105),   S(-42,  73),   S(-41,  42),
            S(-23,   9),   S( 59,  44),   S( 49,  14),   S(-27,  64),   S( -1,  40),   S(-54,  98),   S(-71,  -5),   S(-43, -25),
            S(  0,   0),   S(  0,   0),   S( 59,   7),   S(-19,   5),   S(-49,  26),   S(-59,  98),   S(-91,  65),   S(-103,   8),
            S(  0,   0),   S(  0,   0),   S(-20, -19),   S( 10, -26),   S(-37,  25),   S(-50,  25),   S(-74, -10),   S(-67, -12),

            /* queens: bucket 1 */
            S( -1, -12),   S( -1, -36),   S( 24, -57),   S( 36, -86),   S( 31, -20),   S( 18, -30),   S( 37,  -9),   S(-27,  22),
            S(-19,  27),   S( 22,  -3),   S( 32, -23),   S( 25,  12),   S( 33,  24),   S( -3,  39),   S(-32,  31),   S(-26,  -8),
            S( 56,  -9),   S( 34, -38),   S( 11,  29),   S( 26,  54),   S(-16,  80),   S( 27,  69),   S(  0,  35),   S( -7,  19),
            S( 48, -17),   S( 25,  53),   S(  6,  75),   S( 54,  57),   S( 27,  62),   S( -7,  81),   S( 22,  14),   S( -3,  55),
            S( 44, -10),   S( 77,  20),   S( 56,  52),   S( 30,  17),   S( 41,  59),   S( 61,   8),   S(-34, 119),   S(  6, 100),
            S( 72,  -6),   S( 74,   5),   S( 71,  48),   S(105,  70),   S( 39,  32),   S(  1,  85),   S( 36,  44),   S( -5,  52),
            S(105, -23),   S( 74, -11),   S(  0,   0),   S(  0,   0),   S( -2,  37),   S(-17,  -7),   S(-28,  58),   S(-23,  68),
            S( 86,  -4),   S( 27, -41),   S(  0,   0),   S(  0,   0),   S( 15,  26),   S(  0,  21),   S( 77,   0),   S(-45,  44),

            /* queens: bucket 2 */
            S( 19,  -4),   S( 11,  11),   S( 24,  36),   S( 46, -21),   S( 47, -30),   S( 31, -54),   S( -8, -30),   S( 22,  34),
            S( 26,  16),   S(  2,  59),   S( 33,  29),   S( 37,  23),   S( 47,  15),   S( 10,  41),   S( 28, -11),   S(  2,  75),
            S( 28,  27),   S( 23,  33),   S( 13,  78),   S( 12,  98),   S( 16,  81),   S( 17,  68),   S( 32,  42),   S( 36,  48),
            S(  6,  64),   S( 24,  79),   S(  6,  94),   S( 11, 118),   S( 22,  99),   S(  4, 110),   S( 29,  78),   S( 35,  90),
            S(  3,  75),   S(-19, 107),   S(  7,  74),   S( 36,  59),   S(  4, 113),   S( 87,  43),   S( 90,  32),   S( 74,  24),
            S(-23,  91),   S( -9,  86),   S( -9,  84),   S( 71,  33),   S( 21,  65),   S( 78,  81),   S(124,  45),   S( 39, 117),
            S( -5,  68),   S( -8,  52),   S(-19,  91),   S( 86,  19),   S(  0,   0),   S(  0,   0),   S( 22,  59),   S( 64,  70),
            S(  6,  41),   S( 31,  -2),   S( 35,   8),   S( 66,  15),   S(  0,   0),   S(  0,   0),   S( 91,  44),   S( 11,  68),

            /* queens: bucket 3 */
            S(-53,  54),   S(-49,  60),   S(-29,  44),   S( -9,  57),   S(-20,  30),   S(-19,  -8),   S(-21, -38),   S(-34,  31),
            S(-48,  33),   S(-44,  62),   S(-28,  62),   S(-19,  85),   S(-19,  85),   S(-11,  35),   S( 18, -20),   S( 10, -33),
            S(-51,  69),   S(-40,  85),   S(-38, 113),   S(-42, 152),   S(-40, 130),   S(-27, 100),   S(-14,  68),   S(-10,  12),
            S(-40,  89),   S(-61, 145),   S(-62, 164),   S(-35, 177),   S(-33, 158),   S(-17,  99),   S(-12,  98),   S(-10,  62),
            S(-54, 128),   S(-45, 167),   S(-63, 182),   S(-41, 190),   S(-34, 160),   S(-12, 146),   S(-18, 129),   S(-21,  90),
            S(-60, 100),   S(-63, 159),   S(-81, 200),   S(-51, 193),   S(-57, 169),   S( 28,  78),   S(-26, 159),   S(-23, 126),
            S(-102, 135),  S(-99, 153),   S(-88, 204),   S(-83, 156),   S(-81, 171),   S(-16,  80),   S(  0,   0),   S(  0,   0),
            S(-123, 151),  S(-72,  91),   S(-38,  70),   S(-73, 105),   S(-59, 101),   S(  0,  44),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S( -8,  11),   S(-16, -50),   S(  6,  36),   S(-34, -11),   S(-20,   0),   S(-14,   3),   S(-46, -33),   S( 15,  15),
            S(  6,  -9),   S( -2,   1),   S( 11,  -2),   S(-15, -13),   S(-27, -14),   S(-16,  19),   S(-32, -14),   S(-12, -19),
            S( 14,  25),   S( 38, -11),   S(-43, -50),   S( 28,  15),   S( 48,  11),   S( 11,  23),   S(-16, -28),   S( 46,  20),
            S(-36, -43),   S( 22, -23),   S( 10,  22),   S(  5,  39),   S( 44,  53),   S( 12,  40),   S(-26,   5),   S( -9,  27),
            S(  0,   0),   S(  0,   0),   S( 24,  -5),   S( 25,  30),   S( 52,  72),   S( 31,  51),   S( 15,  37),   S(  1,  15),
            S(  0,   0),   S(  0,   0),   S( 27,   4),   S( 50,  31),   S( 44,  43),   S( 42,  37),   S( -1,  21),   S( -5,  11),
            S(-12, -35),   S( 32,   9),   S( 41,  18),   S( 51,  31),   S( 61,   0),   S(  0,  13),   S( 12,   3),   S(-19,  -3),
            S( 45,  19),   S( -9,  -9),   S( 49,  17),   S( 50,  25),   S(  2, -17),   S(-12,   2),   S( -1,   0),   S( 21,  10),

            /* queens: bucket 5 */
            S( 47,  25),   S( 32, -19),   S( 24,  19),   S(-41,  10),   S( 13, -30),   S( 72,  44),   S(  4,   3),   S( 19,   0),
            S(  2,   4),   S( -2, -22),   S( 26,   0),   S( 11,   7),   S( 30,  56),   S(-22, -19),   S( 15,  16),   S(  6,  -4),
            S( 18, -13),   S( 54, -15),   S( 64,  15),   S( -2,  29),   S( 28,   1),   S( 37,   6),   S(-35,  12),   S( 21,  33),
            S( -1, -41),   S( 41,   4),   S( 23,   5),   S( 50,   7),   S( 72,  10),   S( 27,  25),   S( 30,  67),   S( 19,  37),
            S( 30, -28),   S( 28, -35),   S(  0,   0),   S(  0,   0),   S( 13,   4),   S( 35,  -3),   S( 69,  74),   S( -8,  30),
            S( 49,  17),   S( 36,  22),   S(  0,   0),   S(  0,   0),   S( 18,   9),   S( 81,  36),   S( 52,  32),   S( 54,  39),
            S( 94,  35),   S( 72,   1),   S( 52,  40),   S( 19,  17),   S( 69,  25),   S( 86,  54),   S( 78,  97),   S( 16,  29),
            S( 55,  48),   S( 80,  48),   S( 77,  33),   S( 40,  18),   S( 59,  21),   S( 59,  41),   S( 45,  44),   S( 67,  18),

            /* queens: bucket 6 */
            S( 25,  35),   S(  1,  -1),   S( 26,   5),   S( 53,  14),   S( 18,  24),   S(-29, -19),   S( -3,  36),   S( 26,  26),
            S( 43,  31),   S( 10,  32),   S( 82,  42),   S( 82,  21),   S( 65,   6),   S( 15,   9),   S(-30,  21),   S( 18,   6),
            S( -7,  52),   S( 16,   4),   S( 35,  30),   S( 57,  -5),   S( 46,  27),   S( 27,   9),   S( 76,  18),   S( 61,  63),
            S( 45,  39),   S( 14,  30),   S( 30,   5),   S( 66,  19),   S( 26, -23),   S( 51,   0),   S( 94, -12),   S(117,  61),
            S( 30,  49),   S( 39,  51),   S( 37,  46),   S( 85,  28),   S(  0,   0),   S(  0,   0),   S( 72,  39),   S(129,  35),
            S( 51,  56),   S( 83,  56),   S( 76,  46),   S( 32, -21),   S(  0,   0),   S(  0,   0),   S( 72,  49),   S(117,  33),
            S( 58,  53),   S(  4,  37),   S( 89,   5),   S( 64,  40),   S( 50,  50),   S( 56,  26),   S(129,  18),   S(112,  39),
            S( 55,  61),   S( 72,  30),   S( 93,  47),   S( 84,  38),   S(127,  14),   S(102,  30),   S(112,  32),   S( 79,   4),

            /* queens: bucket 7 */
            S(-21,  27),   S(-13,  -2),   S(-22,  37),   S(-18,  65),   S( 32, -16),   S( -4,  10),   S(-46,  -3),   S(-23, -26),
            S(-34,  38),   S(-84,  69),   S(-38,  83),   S(-18,  71),   S(  9,  34),   S( 28,   6),   S(  2,  23),   S(-31,  10),
            S( -3,  44),   S(-32,  45),   S( -4,  66),   S( 44,  46),   S( 36,  37),   S( 57,  -7),   S( 82, -19),   S( 57,  20),
            S(-43,  36),   S(  3,  56),   S( 56,  64),   S( 56,  71),   S( 60,  56),   S( 56,   6),   S( 82, -46),   S( 33, -21),
            S( 10,  17),   S( 22,  45),   S( 20, 111),   S( 29,  96),   S( 87,  16),   S( 57,  -9),   S(  0,   0),   S(  0,   0),
            S( 10,  41),   S(  4,  84),   S( 27,  85),   S( 19,  89),   S( 68,  40),   S( 98,  65),   S(  0,   0),   S(  0,   0),
            S(-37,  76),   S(-21,  49),   S( 18,  56),   S( 26,  51),   S( 92,   6),   S(119,  10),   S( 48,  -3),   S( 66,  39),
            S( 21,  23),   S( 48,  28),   S( 40,  44),   S( 58,  18),   S( 40,  28),   S( 18,  19),   S(-15, -14),   S(105, -20),

            /* queens: bucket 8 */
            S( -6, -36),   S(  8,  -7),   S(-12, -40),   S(-11,  -5),   S(-21, -33),   S( 18,   4),   S(  2,  -2),   S( -2,  -2),
            S(-33, -52),   S(  1,  -6),   S( -2, -21),   S( -6, -18),   S( -1, -35),   S( -8,  -5),   S( -9,  -5),   S(  5,   8),
            S(  0,   0),   S(  0,   0),   S( 12,   5),   S(-27, -60),   S(  5,  10),   S(  8, -12),   S( -7,   4),   S(  4,   1),
            S(  0,   0),   S(  0,   0),   S(  2,   6),   S(-10, -31),   S( 12,  26),   S( 12,  12),   S( 17,  40),   S(  5,   6),
            S( -3, -12),   S(  5,   2),   S( 15,   4),   S(  9,  15),   S( 23,  14),   S(  7,   7),   S(  5,   2),   S(  6,  13),
            S(  0, -21),   S( -3, -38),   S( -5, -10),   S(  4, -13),   S( 10,   4),   S( 11,  15),   S(  0, -30),   S( 10,   9),
            S(-30, -45),   S(  0, -27),   S( 14,  -4),   S( 24,  39),   S(  9,   0),   S(  0,  11),   S( 10,   6),   S(  9,   6),
            S( 12,   1),   S( -3,  -2),   S( 17,  18),   S( 12,   6),   S( 13,  17),   S(-14, -20),   S( 10,  15),   S(-18, -43),

            /* queens: bucket 9 */
            S( 13,   0),   S(-24, -36),   S(  4, -23),   S(  8, -11),   S(-17, -58),   S( 23,  31),   S( -5,  -4),   S( -2, -25),
            S(-20, -39),   S( -1, -15),   S(-14, -26),   S( 11,  -5),   S(-21, -57),   S(-12, -37),   S( 19,  13),   S(  6,  -1),
            S( -2, -42),   S( -8, -26),   S(  0,   0),   S(  0,   0),   S( -7, -15),   S( 11,   3),   S( -9, -22),   S( -7, -15),
            S( -2,  -3),   S(-17, -38),   S(  0,   0),   S(  0,   0),   S(-13, -24),   S( 13, -10),   S( 15,  29),   S( -3,   3),
            S(-25, -52),   S(-13, -23),   S(  7,   0),   S(-10,  -9),   S(  5, -12),   S( 16,  31),   S( -2, -27),   S(  7, -18),
            S(  9,  10),   S( -5, -34),   S( 12,  -5),   S( -4, -29),   S( -3, -15),   S( -5, -26),   S( -4, -21),   S(-14, -13),
            S(  9,   8),   S( 19,  27),   S(-22, -27),   S( -5,  11),   S( 28,  25),   S( 42,  45),   S( 13,  29),   S( 15, -14),
            S( 25,  -3),   S( 23,   3),   S( -9,  -3),   S( 28,  23),   S( 31,  24),   S( -1,  -9),   S( 15,  -5),   S( 10,  -4),

            /* queens: bucket 10 */
            S( 17,   4),   S( 20,  14),   S( -5,  -4),   S( -6, -23),   S(  1, -19),   S( 11,   5),   S(-18, -50),   S( -7, -29),
            S(  8,   3),   S(-19, -23),   S(  7,  -3),   S(-12, -43),   S(-11, -18),   S( 20,  14),   S( -9, -36),   S( -9,  -7),
            S( -1,   8),   S( -2,  -6),   S(  9,  20),   S(-11, -13),   S(  0,   0),   S(  0,   0),   S(  2,  -2),   S(-17, -26),
            S(-10, -22),   S( 10,   3),   S( -1, -19),   S( 11,  15),   S(  0,   0),   S(  0,   0),   S(-15, -30),   S( 10,   2),
            S( 17,  23),   S( 11,  15),   S( -3,  -8),   S( 34,  47),   S(  0,   0),   S( 10,  12),   S(  3, -12),   S( 25, -22),
            S(-20, -18),   S( -4,  -6),   S( 23,  23),   S( 16,  12),   S( 13,   5),   S(  8,  10),   S( 21,  13),   S( -9, -21),
            S(  6,   5),   S( 23,  29),   S( 21,  52),   S( 38,  46),   S(  2,   8),   S(  3, -10),   S( 13,   8),   S( -8, -27),
            S(-16, -36),   S(  1,  -9),   S( 14,   3),   S(  6,  -9),   S( -1,  -2),   S(  7,  10),   S(  7,   8),   S( 26,  13),

            /* queens: bucket 11 */
            S(-23, -14),   S( -4,  -8),   S(-21, -44),   S(-37, -40),   S( -7, -23),   S(-30, -61),   S( -2, -42),   S(-14, -32),
            S(  8,   7),   S(  2,  -1),   S(-40, -27),   S( 18,  28),   S( 21,   7),   S( 19,   4),   S( 10, -12),   S( -1, -13),
            S( -3,   8),   S( 19,  20),   S(-18,  22),   S( -9,  -3),   S( -6, -14),   S(-30, -36),   S(  0,   0),   S(  0,   0),
            S( -1,   5),   S(-11,   4),   S( 10,  27),   S(  1,  19),   S( -1, -17),   S(  8,  17),   S(  0,   0),   S(  0,   0),
            S(  6,  14),   S( -1, -19),   S( 25,  19),   S(  7,  19),   S( 44,  43),   S( 21,  36),   S(  8,   6),   S( -9, -32),
            S( -3,   5),   S( -9,  -7),   S( -5,  -4),   S(  5,  10),   S( 10,  10),   S( 11,   1),   S(  6, -12),   S( -5, -31),
            S(-11,  -6),   S(  6,   2),   S(  8,   9),   S(  5,  14),   S( 30,  63),   S( 25,  27),   S( 17,   4),   S( 25,  23),
            S(-12, -44),   S( 22,  14),   S(-20, -16),   S(-19,   7),   S( 42,  64),   S( -7, -14),   S(  9,  17),   S( 17,  20),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  9,  13),   S(-17, -19),   S( -5,  -2),   S(-11, -20),   S(  3,   7),   S( -1,   0),
            S(  0,   0),   S(  0,   0),   S(  0,  -8),   S(-15, -24),   S(-14, -25),   S(-18, -42),   S( -8, -20),   S(  4,   3),
            S(  1,   4),   S(  3,   0),   S(  0,   5),   S(-14, -45),   S( 29,  43),   S(  8,  37),   S( -1,  -4),   S(  6,  10),
            S(  6,  -2),   S( -1,  -4),   S( 17,  15),   S(  9,  21),   S( -3,   2),   S( -8,   3),   S( -3, -13),   S( -9, -16),
            S(-31, -46),   S(  6,   1),   S( -1,  -1),   S( -2,   5),   S( 17,  46),   S( -6, -22),   S( -7, -15),   S( -4,  -9),
            S(  4,   1),   S( -5,  -6),   S( -7, -23),   S( -2,  11),   S( -7,  -9),   S( -9,   6),   S(-25, -13),   S( -7, -15),
            S(-23, -34),   S( -2,  -1),   S(  1,   0),   S( 13,  13),   S(  0,  -8),   S( -8, -13),   S(  8,  15),   S(  2,  -8),
            S(  8,  19),   S(  8,   8),   S(  7,   2),   S( 12,  19),   S(-16,  -9),   S(-17, -18),   S(-11,  12),   S(-11, -24),

            /* queens: bucket 13 */
            S(-25, -56),   S(  7,   5),   S(  0,   0),   S(  0,   0),   S(-19, -30),   S(-21, -39),   S(  1,   0),   S( -9, -21),
            S(-16, -47),   S( -8, -15),   S(  0,   0),   S(  0,   0),   S(-16, -39),   S(-21, -37),   S( -6, -23),   S( -2,  -2),
            S( -7, -13),   S( -5,  -4),   S( -1,  16),   S(  6,  -5),   S(-21, -44),   S( -3,  -9),   S(-16, -13),   S(  3,  -2),
            S(-12, -21),   S(-28, -30),   S( -2, -15),   S( -6,  -5),   S( 12,   4),   S(  9,  13),   S( -8, -16),   S(-12, -12),
            S( 13,  -1),   S(  7, -34),   S( -8, -15),   S( 11,  22),   S(-17, -35),   S(  6, -24),   S( -4, -15),   S( -3, -18),
            S( -2,  -5),   S(-12, -11),   S( 14,  13),   S(  1,  14),   S(  8, -13),   S( -3,   8),   S(-15, -26),   S(  0,   2),
            S( -3, -14),   S(  0, -12),   S(  7,  13),   S( -2,  -9),   S(  0,  -6),   S( 10,  -4),   S( -8, -12),   S(-12, -15),
            S(-13,  10),   S( -2,  -2),   S(  2,   5),   S( -1,  -7),   S(  2,   4),   S( -1,   6),   S(  6,  11),   S(-12, -14),

            /* queens: bucket 14 */
            S( -3, -18),   S( 13,  -7),   S(-11, -23),   S( -9, -11),   S(  0,   0),   S(  0,   0),   S( -3, -11),   S(  1, -17),
            S(-11, -29),   S(-28, -45),   S( -8, -17),   S(  2,   0),   S(  0,   0),   S(  0,   0),   S(  0,  -4),   S( -6, -29),
            S(-10, -21),   S(  5, -20),   S(-25, -41),   S(-10, -25),   S(  0,   5),   S(  0,   1),   S(-12, -22),   S(-14, -32),
            S( -7,  -1),   S(  6,  11),   S( -2,  -5),   S(-18, -32),   S( -9, -16),   S(-22, -46),   S(  3,  -9),   S( -3,  -2),
            S( -5,  -5),   S(  2,  -4),   S( -7, -30),   S( 10,  23),   S( 10,  26),   S( -8, -15),   S( -7,  13),   S( 12,  10),
            S( -1,  -7),   S( -1,  -5),   S( -4,  -8),   S( -6,  -4),   S(  3,  13),   S( -7,  -2),   S( -3,   1),   S( -8, -16),
            S( -4,  -1),   S( -2,  -2),   S(  0,  -4),   S( -1,   0),   S(  6,   8),   S(  9,  10),   S(-21, -39),   S( -4, -11),
            S(-13, -13),   S( 10,  -1),   S( -5, -13),   S(-10, -14),   S(  7,   4),   S( 10,  17),   S( -2,   5),   S(  1,  -5),

            /* queens: bucket 15 */
            S( -3,  -7),   S(  2,  -5),   S(  9,   2),   S(-17, -29),   S(  4,   8),   S(-15, -11),   S(  0,   0),   S(  0,   0),
            S(  1,   9),   S(  2,   0),   S(-18, -23),   S( -4, -14),   S( -9,  -4),   S(  0,   6),   S(  0,   0),   S(  0,   0),
            S( -1,  -2),   S( -1,   4),   S( -8,  -7),   S(-17, -18),   S(-11, -28),   S(  6,  11),   S(  0,   2),   S( -5, -11),
            S( -1,  -1),   S(-13, -16),   S( -3,  -3),   S(-12, -18),   S(  8,  33),   S(  7,  37),   S( -1,   1),   S( -6, -20),
            S( -2,  -5),   S(  8,  15),   S(-11, -19),   S(  8,  20),   S( 15,  48),   S( 15,  40),   S(-15, -19),   S(-14, -17),
            S(  1,  -1),   S(  1,  -4),   S( -2, -16),   S( -6,   2),   S(  7,  15),   S(-11,  -4),   S(  4,  14),   S( -8,  -5),
            S( -1,  -5),   S(  0,  10),   S(-15, -16),   S(  8,   8),   S(  1,   8),   S(  7,  17),   S(  8,  19),   S( -7, -16),
            S(  1, -15),   S(-17, -44),   S( -1,   0),   S(  5,  10),   S(-21,   8),   S( -2,  -3),   S(  7,   5),   S( -2,   5),

            /* kings: bucket 0 */
            S(  3, -23),   S( 30, -12),   S(  8,  10),   S(-27,  13),   S(-17,  19),   S( 30, -34),   S(  0,   2),   S(  9, -54),
            S(-15,  27),   S(-21,   9),   S(-11,  -6),   S(-47,  30),   S(-54,  45),   S( -5,  13),   S(-17,  42),   S(  0,  21),
            S( 22,  -6),   S( 45, -25),   S( 11,  -3),   S(-35,   0),   S(-48,   6),   S( 20,  -9),   S(-24,  17),   S( 44, -26),
            S(-29, -35),   S( 49, -31),   S( -6,  -7),   S(-17,   9),   S(-92,  58),   S(-42,  20),   S(-42,  21),   S(-32,  45),
            S(-58, -131),  S( -8, -28),   S( -8, -30),   S( 19, -29),   S(-38,   1),   S(-37,  17),   S(-12,   3),   S( -9,  -5),
            S( -5, -127),  S(  5,   5),   S( -4, -52),   S(  5,   4),   S( -1, -14),   S(-44,  11),   S( 19,   1),   S(-13,   8),
            S(  0,   0),   S(  0,   0),   S(  0, -57),   S(  4, -45),   S(-23,  10),   S(-13, -38),   S(-17,  20),   S(-23, -18),
            S(  0,   0),   S(  0,   0),   S(-14, -13),   S(  2, -22),   S( 17,   7),   S(  3,  32),   S( 18,  35),   S(  9,  -9),

            /* kings: bucket 1 */
            S( 18, -14),   S( 35, -28),   S( 13,  -9),   S( 22,  -6),   S( -2,   0),   S( 29, -12),   S(  9,   2),   S( 12, -11),
            S(  9,  -6),   S( 13,   2),   S( -2,   0),   S(-35,  15),   S(-27,  12),   S(-22,  22),   S(  3,  14),   S( 10,   8),
            S( -5, -15),   S(  3, -18),   S( 11, -22),   S( 12, -21),   S(-39,  11),   S( 14, -23),   S( 24, -11),   S( 36,  -8),
            S( -8,   8),   S( 19, -20),   S( -2,  -2),   S( -4,  12),   S(  2,  12),   S(-14,   9),   S( 37,  -9),   S(-23,  29),
            S(-15, -68),   S(-10, -40),   S(-33, -43),   S( -4, -37),   S( -7, -13),   S(  3, -40),   S(  5,  -5),   S(-15,  -3),
            S( -8,  20),   S(-120,   5),  S(-43,  36),   S(  1,  -1),   S(-38,   4),   S(-14,   5),   S( 22,   7),   S(-28,   5),
            S(-36, -46),   S(-18,   3),   S(  0,   0),   S(  0,   0),   S(-26,  31),   S(-43,  13),   S(-24,  40),   S(  0, -22),
            S(-33, -118),  S( -7,  -5),   S(  0,   0),   S(  0,   0),   S(-11,  11),   S( -7,  12),   S( -9,  -5),   S( -4, -55),

            /* kings: bucket 2 */
            S( 11, -56),   S( 15,  -2),   S( 24, -22),   S( 13, -11),   S(  2,  10),   S( 38, -27),   S( -1,  17),   S( 20, -25),
            S( 30, -32),   S( -4,  33),   S( -9,   6),   S(-19,   8),   S(-23,  12),   S(-19,   9),   S(  9,   0),   S(  7,   0),
            S(-37,   7),   S( -8, -11),   S(  1, -16),   S(-22, -14),   S( -5,   1),   S( -9, -15),   S( 46, -23),   S( 38, -24),
            S( 15,  15),   S(-34,  16),   S( 17,   4),   S(-28,  13),   S( 53, -18),   S( -1, -10),   S(  4, -22),   S( 30,  -3),
            S( -8,  -7),   S( 23,  -6),   S( 40, -51),   S( -3, -19),   S( 16, -44),   S(-20, -40),   S( 21, -50),   S( 13, -56),
            S( -1, -10),   S( 18, -13),   S(  2, -16),   S(-43,  -1),   S( -6,  14),   S(-18,  21),   S(-71,   9),   S(-38,  -9),
            S( -8, -32),   S(-23,  31),   S(-71,  -8),   S(-20,  47),   S(  0,   0),   S(  0,   0),   S(-12,  24),   S(-24, -39),
            S( -3, -23),   S(-13, -10),   S(-37, -41),   S( -6, -15),   S(  0,   0),   S(  0,   0),   S(-10, -19),   S(-28, -121),

            /* kings: bucket 3 */
            S(  7, -52),   S( 17, -11),   S( 31, -30),   S(-14,  -6),   S( -3, -13),   S( 35, -27),   S(  3,  13),   S(  6, -28),
            S( -1,  20),   S(-18,  38),   S( -9,  -2),   S(-48,  24),   S(-56,  31),   S(  0,  -1),   S(-10,  20),   S(  4,  12),
            S( 15, -19),   S( 10,  -5),   S(-10,  -5),   S(-36,  -1),   S(-11,  14),   S( 29, -26),   S( 58, -23),   S( 54, -18),
            S(-39,  30),   S(-97,  46),   S(-56,  17),   S(-51,  18),   S(-51,  17),   S(-15, -21),   S(-36,  -4),   S(-40, -10),
            S(-19,  13),   S(-28,   7),   S(-48, -14),   S(-38,  -4),   S( 10, -44),   S( 63, -70),   S( 18, -64),   S( -3, -75),
            S( -7, -25),   S( 17,   4),   S( 37, -15),   S(  5, -24),   S( 50, -33),   S( 59, -46),   S( 86, -26),   S( 54, -122),
            S(-23,   1),   S( 32,   3),   S( 14, -27),   S( 41, -25),   S( 28, -16),   S( 40, -39),   S(  0,   0),   S(  0,   0),
            S(  3, -10),   S( -2,  -3),   S( -1,  33),   S(  9,  15),   S( -1, -76),   S(-12,  19),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-59,  13),   S(-21,  44),   S( -9,  32),   S(  5,   6),   S(  0,   3),   S(  9, -20),   S( 18,  11),   S(  8, -40),
            S(-39,  23),   S( 30,  16),   S(-13,  13),   S(  7,  -4),   S( 44, -10),   S( 11,   2),   S( 73, -19),   S(  9,  -5),
            S(  0,  17),   S( -2,  -9),   S( 33,  -9),   S( -3,   0),   S(  5,   0),   S( 29, -31),   S(-68,  18),   S( 47, -13),
            S( 12,  -9),   S( -9,   2),   S(  9,  11),   S( -6,  10),   S( -4,  14),   S(-32,  19),   S( 18,  17),   S( 43, -13),
            S(  0,   0),   S(  0,   0),   S( -7,  10),   S(-28,   9),   S(-52,  26),   S(-53,  -3),   S(-10, -14),   S( -9,  -4),
            S(  0,   0),   S(  0,   0),   S(  6,  15),   S(-10,  45),   S(-20,  11),   S(-24,  -7),   S( -3,  -1),   S(  1,  11),
            S( -2, -19),   S(  3,   4),   S( -5, -27),   S( 18,  22),   S(-14,  27),   S(-30, -16),   S(-11,  28),   S( 14,  15),
            S( -6, -22),   S(  2, -22),   S( -8, -25),   S( -6, -20),   S( -3, -13),   S( 10,  21),   S(-21,   0),   S( -1, -10),

            /* kings: bucket 5 */
            S( 21,  -8),   S(  6,  -7),   S(-55,  31),   S( -8,  11),   S(-12,  27),   S( 10,   3),   S( 55,  -6),   S( 27,  -8),
            S( -3,   5),   S( 23,  11),   S( 23,  -1),   S( 18,  -2),   S( 10,  -3),   S( 25,  -8),   S( 27,   5),   S( 42, -21),
            S(-22,   8),   S(-24,  -1),   S( -7,   1),   S(-13,  -8),   S( 16,  -4),   S(-45,  -1),   S( -6,   8),   S( 23,  -8),
            S(  4, -12),   S( 10,  -8),   S( 13,  -6),   S(  7,  14),   S( 10,  22),   S(-16,  14),   S(  3,  11),   S( 22,  -8),
            S( -4, -27),   S(-32, -56),   S(  0,   0),   S(  0,   0),   S( -8, -12),   S(-26,  -8),   S(-14,  -8),   S(-14,   4),
            S( -5, -29),   S(-29, -30),   S(  0,   0),   S(  0,   0),   S(-27,  21),   S(-56,  19),   S(-18,  -8),   S( -7, -14),
            S( -5, -10),   S(-35,  11),   S( -2,  28),   S( -5, -44),   S(-18,  31),   S(-32,  19),   S(  6,  12),   S( -2,   4),
            S(-12, -119),  S( -3,  22),   S( -3,   3),   S( -3, -31),   S( -7, -17),   S(-16,  16),   S( -1, -21),   S(  4,  22),

            /* kings: bucket 6 */
            S( 70, -50),   S( 26, -14),   S( 12,  -9),   S( -5,  10),   S(-30,  31),   S(-21,  16),   S(  1,  19),   S( 17,   1),
            S( 77, -36),   S(-10,  22),   S( -1,   1),   S( 28, -10),   S( 25,  -9),   S(-13,  13),   S(  8,   3),   S( 11,  -1),
            S(  7, -10),   S(-45,   7),   S(-13,  -6),   S( -8,  -8),   S( 11, -11),   S(-56,   8),   S( 24,  -3),   S(-40,  20),
            S( 31,  -3),   S( 11,   3),   S( 24, -16),   S( 35,  -3),   S( 69,  -4),   S(-22,   4),   S(  2,   2),   S( -3,   2),
            S(  7, -26),   S(  4, -20),   S(-14, -18),   S(-15,  -3),   S(  0,   0),   S(  0,   0),   S(-47, -21),   S(-40, -29),
            S( -3,  14),   S( -2, -17),   S(-19,   0),   S(-16, -25),   S(  0,   0),   S(  0,   0),   S(-45, -15),   S(-18,  -9),
            S( -6, -25),   S( -2, -24),   S(-47,   3),   S(-14,  15),   S( -5,  -3),   S( -6,  -6),   S(-42, -13),   S( -8, -32),
            S(  4,  15),   S(  4,  11),   S(-11, -15),   S(-11,   5),   S(  8, -33),   S(-11, -35),   S( -4,  10),   S(  0, -70),

            /* kings: bucket 7 */
            S( 37, -33),   S(-10,  -1),   S( -6,  -9),   S(-19,   7),   S(-32,  16),   S(-54,  39),   S(-28,  34),   S(-36,  19),
            S(  2,   4),   S( 26, -29),   S(-12,  -4),   S(-38,   8),   S( -2,  -4),   S(-34,  25),   S( -6,   1),   S(  2,  12),
            S( 43, -43),   S(-29,  -2),   S(-58,  11),   S( -8, -13),   S(-33,   4),   S(-49,  20),   S( 12,   3),   S(-55,  19),
            S(-47,  18),   S( -4,  19),   S(  7,  -2),   S( 20,   1),   S( 49, -16),   S( 51, -29),   S( 14,  -8),   S( 32, -16),
            S( -5,  16),   S( 10,   0),   S( 15, -30),   S( 20, -21),   S( 36, -26),   S(  6, -25),   S(  0,   0),   S(  0,   0),
            S( -3, -42),   S(  5,  -6),   S(-10,  -8),   S(  7,   3),   S( 39, -13),   S(  7,  -3),   S(  0,   0),   S(  0,   0),
            S(  5,   5),   S( 23, -13),   S(  4,  25),   S(-16, -33),   S( 14, -13),   S( -5, -19),   S( 11, -15),   S(-13,  13),
            S( -3,   7),   S( -5, -38),   S( 13,  12),   S( -4,   0),   S( 11,  39),   S(-28, -53),   S(  0, -17),   S(-15, -56),

            /* kings: bucket 8 */
            S(  1, 107),   S(  2,  93),   S( 17,  29),   S( 20,  -3),   S(-28,  -7),   S(-14,   9),   S(  8, -13),   S( -3, -19),
            S( 35,  69),   S( 30,  19),   S( 35,  68),   S( 83, -10),   S( -2,  31),   S(  9,   1),   S( 17,  -1),   S( 15,  31),
            S(  0,   0),   S(  0,   0),   S( 28,  76),   S( 23,   9),   S(  6,  20),   S(  5, -14),   S(-18,  18),   S(-12, -21),
            S(  0,   0),   S(  0,   0),   S(  5,  67),   S( 12,  -7),   S(  7,  23),   S(-24,  24),   S(  7,   1),   S( 16,  34),
            S( -8, -29),   S(  2,  27),   S( -2,  22),   S( -9,  19),   S( -8, -32),   S( -5, -12),   S(  4, -14),   S( -5, -43),
            S( -2,  14),   S(  8, -12),   S( -9, -24),   S(-11,  -3),   S( -7,  -2),   S( -7,   3),   S(-11,  27),   S( 13,  -5),
            S( -8, -21),   S( -6,   9),   S(  6,   0),   S( -2, -16),   S(  5, -38),   S(-18,  20),   S( -3, -15),   S( -2, -63),
            S( -1,   3),   S(-15, -27),   S( -5,  -4),   S( -1, -16),   S(  8,   9),   S( -4,   3),   S(  3,  -1),   S(  5,  -4),

            /* kings: bucket 9 */
            S( -3,  13),   S(-11,  29),   S(-26,  61),   S( 20,  -5),   S(-31,  36),   S(-43,  33),   S( 54,   0),   S( 31,  -6),
            S(-31,  48),   S( 33,  17),   S( 30,   6),   S( 52,  -1),   S( 57,  13),   S( 17,  10),   S( -9,  34),   S(-29,  24),
            S(  8,   4),   S( 26, -22),   S(  0,   0),   S(  0,   0),   S( 29,  16),   S( 15,   5),   S( 16,  -2),   S(-23,  32),
            S( -3, -38),   S( 10, -23),   S(  0,   0),   S(  0,   0),   S( 10,  14),   S(  8,  11),   S( -9,   7),   S(-18,  22),
            S(  4, -16),   S(  7,   2),   S( -1,  16),   S(  7,   2),   S( -4,  13),   S(-24,  15),   S( -7,   5),   S( 20,  -3),
            S( 10,  21),   S(  9, -21),   S( 12,  -2),   S(-18, -17),   S(-24,  13),   S( 10,  13),   S(-34,  -6),   S( 20,  55),
            S( -3, -14),   S( -4, -18),   S(  4,  22),   S(  6, -30),   S( 12, -15),   S(  9,  25),   S(-11, -34),   S( 13,  20),
            S(  7,  -1),   S(  1,  -1),   S( 10, -26),   S( -8, -45),   S(-11, -40),   S(  1,  -8),   S(-10,   5),   S(  7, -11),

            /* kings: bucket 10 */
            S( 21,  -3),   S(  3, -22),   S( 23,  -3),   S(-10,  32),   S( -1,  16),   S(-79,  41),   S(-45,  51),   S(-64,  79),
            S( 33,  -6),   S( 64,  15),   S( 13,  -2),   S( 29,  18),   S( 53,  15),   S( 56,   2),   S( 13,  25),   S(-87,  49),
            S( 20,  17),   S( 52,  -3),   S( 40,  -8),   S( 27,   8),   S(  0,   0),   S(  0,   0),   S(-36,  25),   S(-55,  25),
            S( 29, -15),   S( 50, -25),   S( 25, -30),   S( 26,  12),   S(  0,   0),   S(  0,   0),   S(  4,  14),   S(  1,   2),
            S( 13,   8),   S( 33,   5),   S( 26, -22),   S( 19, -42),   S(  4, -25),   S( 17,  21),   S(  4,   7),   S(-14,  23),
            S( -4,  21),   S(-16,   9),   S( -7,   7),   S(  4,   5),   S(  3,  14),   S(-24, -10),   S(-11,  -3),   S( -4, -18),
            S(  2, -31),   S( -2,   3),   S(  8,  -3),   S( 10, -16),   S( 11, -31),   S( -1, -19),   S( 10, -30),   S(  0, -13),
            S( -3, -16),   S( 13, -29),   S( -5, -16),   S(  3,   4),   S(-11,   1),   S(-12, -45),   S( -9, -15),   S( 12,  22),

            /* kings: bucket 11 */
            S( -8, -18),   S(  5,  13),   S( 11,  -7),   S( -4,  24),   S(  1,   6),   S(-84,  64),   S(-60,  76),   S(-128, 149),
            S( -4, -21),   S( 21,  17),   S(-25, -33),   S( 33,  13),   S( 82,   4),   S( 72,  35),   S( 40,  20),   S(  1,  44),
            S( 10, -35),   S( 12,  25),   S(-13,  -6),   S( 39,   8),   S( 61,   6),   S( 31,  57),   S(  0,   0),   S(  0,   0),
            S(  5,  12),   S( -6,   8),   S( 11, -12),   S( 16,  35),   S( 33,  -6),   S( 25,  18),   S(  0,   0),   S(  0,   0),
            S(  1,  26),   S(-18,  -1),   S(  7,  -3),   S(  3, -23),   S( 18,   4),   S(  3, -10),   S(  4,  19),   S( -3,   6),
            S( 20,  17),   S( -5,  -7),   S( -1, -16),   S( -7, -10),   S( -6,   2),   S(  9,  -6),   S(  6,  -3),   S( -7, -22),
            S(  6,  15),   S( -9, -20),   S( 26,  27),   S( -1, -22),   S( 21, -42),   S( -1, -11),   S(-21,  -6),   S(-17, -25),
            S(  5,   6),   S(  8,   2),   S(-12, -19),   S(  9,  22),   S( -6, -31),   S(-12, -10),   S( -1, -11),   S(  7,  20),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 23,  52),   S( -2,   1),   S( -8, -34),   S( -3,  12),   S(-17, -10),   S(  6,  24),
            S(  0,   0),   S(  0,   0),   S( 51,  99),   S( 21,  18),   S( 50,  58),   S(  6,  -7),   S( 17,  -7),   S(-18,   3),
            S(  0,   7),   S(  3,   8),   S( 18,  72),   S( 55,  40),   S(  6, -26),   S( 17,  14),   S(  7,  -8),   S(-34,   0),
            S(  0,  16),   S(  1,  29),   S( -9,  19),   S( -9, -31),   S(-18,  24),   S( -5,  12),   S( -4,  36),   S( 11,  12),
            S(  7,   4),   S( 12,  28),   S(  9,   9),   S(  6,  43),   S(  5,  48),   S(-11,  -6),   S( -5,  15),   S( -6,  -3),
            S(  2,  -8),   S( 10,  11),   S( -4,  -5),   S(-12, -15),   S(  2,  20),   S( -7,  12),   S( -3, -16),   S(  3,   1),
            S( 11,  21),   S( -9, -22),   S( -7, -13),   S(  3,  23),   S(  1,   6),   S(  3,  -4),   S(  0,  21),   S(  1, -15),
            S(  1,   3),   S( -2,  -2),   S(  1,  11),   S( -3,  -5),   S( -3, -17),   S(  7,  15),   S(-13, -13),   S(  0,  -9),

            /* kings: bucket 13 */
            S( -8,  62),   S( 15,  52),   S(  0,   0),   S(  0,   0),   S( 38,  18),   S( 26,  -2),   S(  7,  -7),   S(-20,  10),
            S(  1,  35),   S( -1, -13),   S(  0,   0),   S(  0,   0),   S( 40,  10),   S( 33, -10),   S(-37,   7),   S(-13,  -2),
            S( -8,  11),   S(  7,  36),   S( -7, -39),   S( 15,  34),   S( 39,  -9),   S( 27,  -3),   S(  0,  -2),   S( 10,  -3),
            S( -2,   5),   S(  4,  -4),   S(  6,  44),   S( -2,   5),   S( -7,   9),   S( 25,  -5),   S(  5,  14),   S(-13, -14),
            S(  5,   3),   S( -7,   2),   S(  5,  33),   S(-13,  21),   S(-10,   9),   S(  7,  19),   S(-11,  -6),   S( 14,  14),
            S(  3, -11),   S(  2,  41),   S(  7,  33),   S(-11,  -8),   S( -8,   6),   S( -7,  -3),   S(-21,  11),   S( -4, -26),
            S(  1,   7),   S( -3, -11),   S( -7, -37),   S(  2,  22),   S( -8, -15),   S( -3,  25),   S(-14, -14),   S(  6,  17),
            S(  3,  -6),   S(  5,  -5),   S(  0,   9),   S(  5,   6),   S(  0,  36),   S(-14, -23),   S( -1,  11),   S(  8,  15),

            /* kings: bucket 14 */
            S( 20,  47),   S( -8, -17),   S(  0, -52),   S( 17,  27),   S(  0,   0),   S(  0,   0),   S( 18,  57),   S(-38,  17),
            S(-20, -32),   S( 25,  -7),   S( 44, -25),   S( 28, -14),   S(  0,   0),   S(  0,   0),   S( 16,  36),   S(-50,   4),
            S(-10, -18),   S( 26, -16),   S( 37, -37),   S( 40,  10),   S( 10,   1),   S(  4,  36),   S( 20,  46),   S(-18,  -7),
            S(  9,   0),   S( 13,  16),   S(-16, -38),   S( 11,  15),   S(-23, -10),   S( 24,  48),   S(  8,  25),   S(  0, -17),
            S( 12,  44),   S(  8, -22),   S(  5,  26),   S(-15,  13),   S( 16,  37),   S( 11,  56),   S( 15,  78),   S(  3,   6),
            S(-13, -34),   S(  2,  18),   S(-10,  -6),   S(  9,   4),   S( -3, -17),   S(-12,  16),   S(-15, -13),   S( -5,  19),
            S(  4,  14),   S(-17, -13),   S(  5,   0),   S( 17, -11),   S(  4,  -1),   S(-11,  17),   S(-18,  -2),   S( 13,  37),
            S( -3,  -4),   S(  7,   3),   S(  9,   5),   S( -7, -17),   S( 10,  -2),   S( -4, -14),   S( -6, -22),   S(  2,  -7),

            /* kings: bucket 15 */
            S(  0,  26),   S( 14,   1),   S( 10,  -5),   S(-13, -15),   S(  7, -10),   S(-18,  63),   S(  0,   0),   S(  0,   0),
            S( -4, -31),   S(  2, -24),   S(  6, -13),   S( 35,  68),   S( 31,   9),   S( 53,  96),   S(  0,   0),   S(  0,   0),
            S(-13, -31),   S( 22,   6),   S(  0, -15),   S(  1,  18),   S( 14, -16),   S( 28,  82),   S( 12,  48),   S( -9,   0),
            S(  1, -13),   S(  9,   5),   S(  6,  23),   S(-14, -43),   S( -6,  -6),   S( 18,  45),   S( 25,  63),   S(-12, -37),
            S( 14,  36),   S(-19,  22),   S(  4,  24),   S( -8, -35),   S(-11,  -1),   S(  6,  68),   S( 14,  -3),   S(  2,   4),
            S(  9,  27),   S( -8,   0),   S(  4,   3),   S(  2,  29),   S(-14, -50),   S(  5,  18),   S( 10,  10),   S(  7,  12),
            S(  5,   4),   S( -2,  19),   S(  0,  -6),   S(  2,   4),   S( -3, -28),   S( -1,  12),   S( -3,  -7),   S(  1,  -3),
            S(  2,   2),   S(  7, -10),   S( -2, -20),   S(  1,  -6),   S(  4,  -1),   S(  9,  12),   S( -1,   4),   S(  3,   6),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-28,  64),   S(-32, -61),   S( -3,  32),   S( 12,  88),   S( 22, 117),   S( 26, 153),   S( 36, 158),   S( 47, 163),
            S( 65, 143),

            /* bishop mobility */
            S(-57,  95),   S(-37, -47),   S(  3,   6),   S(  7,  79),   S( 22, 110),   S( 31, 137),   S( 35, 156),   S( 42, 163),
            S( 45, 172),   S( 53, 170),   S( 62, 167),   S( 86, 146),   S(105, 142),   S( 74, 143),

            /* rook mobility */
            S(-61,   0),   S(-147, 126),  S(-18,  38),   S(-12, 112),   S(-11, 145),   S( -9, 167),   S( -7, 185),   S( -1, 192),
            S(  8, 194),   S( 14, 203),   S( 18, 211),   S( 29, 209),   S( 41, 211),   S( 52, 210),   S(102, 178),

            /* queen mobility */
            S(112, 187),   S( 65, 293),   S(  7, 285),   S( 29, 204),   S( 47,  81),   S( 62,  31),   S( 62, 144),   S( 63, 206),
            S( 59, 268),   S( 59, 300),   S( 58, 331),   S( 63, 342),   S( 64, 363),   S( 67, 370),   S( 67, 381),   S( 68, 386),
            S( 71, 388),   S( 71, 389),   S( 74, 386),   S( 79, 376),   S( 88, 360),   S(126, 321),   S(127, 303),   S(172, 262),
            S(195, 245),   S(198, 219),   S(145, 202),   S(111, 146),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  17),   S(-22,  46),   S(-41,  49),   S(-43,  63),   S( 18,  15),   S(-24,  21),   S(-14,  64),   S( 19,  34),
            S( 16,  32),   S( -2,  47),   S(-27,  49),   S(-15,  34),   S( -3,  37),   S(-26,  44),   S(-40,  64),   S( 26,  33),
            S( 21,  72),   S( 10,  74),   S(  4,  58),   S( 19,  52),   S( -4,  56),   S(-32,  67),   S(-40, 106),   S(-14,  82),
            S( 28, 112),   S( 45, 114),   S( 26,  83),   S(  5,  62),   S(  5,  65),   S(-15,  99),   S(-45, 124),   S(-83, 159),
            S( 27, 154),   S( 55, 186),   S( 56, 146),   S( 31, 115),   S(-55, 107),   S( 23, 116),   S(-57, 173),   S(-80, 189),
            S( 88, 237),   S( 87, 274),   S(126, 254),   S(119, 258),   S(138, 273),   S(157, 247),   S(116, 265),   S(113, 279),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,   8),   S( -5, -30),   S( -2,  -8),   S(  1, -11),   S( 13,  19),   S(-12, -43),   S(-23,   9),   S( -5, -49),
            S(-21,  18),   S( 23, -22),   S( -5,  34),   S(  6,  26),   S( 31,  -5),   S(-14,  20),   S( 24, -16),   S( -8,  -7),
            S(-15,  17),   S( 18,   7),   S(  2,  45),   S( 13,  56),   S( 26,  33),   S( 26,  18),   S( 33,   6),   S( -4,  13),
            S( 16,  38),   S( 16,  52),   S( 37, 103),   S( 10, 101),   S( 65,  82),   S( 75,  51),   S( 22,  60),   S( 18,  21),
            S( 38, 116),   S( 92, 126),   S(112, 138),   S(133, 165),   S(130, 127),   S(135, 164),   S(124, 131),   S( 35,  48),
            S( 77, 202),   S(124, 292),   S(104, 229),   S( 90, 183),   S( 55, 136),   S( 46, 135),   S( 39, 125),   S( 10,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 14,  18),   S( 19,  21),   S( 33,  34),   S( 29,  27),   S( 19,  17),   S( 24,  26),   S(  3,  10),   S( 42,  -1),
            S( -4,  22),   S( 17,  32),   S( 10,  41),   S(  8,  43),   S( 24,  16),   S(  8,  23),   S( 31,  23),   S(  0,  13),
            S(  0,  23),   S( 30,  54),   S( 52,  63),   S( 39,  59),   S( 45,  55),   S( 70,  24),   S( 32,  36),   S( 23,   6),
            S( 58,  86),   S(114,  54),   S(119, 123),   S(156, 143),   S(136, 134),   S( 80, 141),   S( 59,  71),   S( 80,   2),
            S( 50, 124),   S(118, 125),   S(147, 195),   S(122, 228),   S(126, 264),   S( 75, 244),   S(158, 228),   S(-37, 188),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 21,  37),   S( 13,  16),   S( 15,  30),   S( -9,  58),   S( 69,  19),   S( 20,  10),   S(  0,   2),   S( 24,   9),
            S(  2,  14),   S(  8,  10),   S( 17,  20),   S( 14,  33),   S( 12,  19),   S( -1,  11),   S(  7,   9),   S( 29,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -14),   S( -8, -10),   S(-17, -20),   S(-14, -33),   S(-12, -19),   S(  1, -11),   S( -7,  -9),   S(-29,   5),
            S(-21, -37),   S(-13, -16),   S(-15, -30),   S(  9, -58),   S(-69, -19),   S(-20, -10),   S(  0,  -2),   S(-24,  -9),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -41),   S(-13, -40),   S(-15, -49),   S(-57, -36),   S(-22, -46),   S(-29, -51),   S( -4, -48),   S(-19, -66),
            S(-29, -28),   S(-19, -29),   S(-35, -15),   S(-11, -35),   S(-40, -38),   S(-34, -21),   S(-39, -21),   S( -8, -46),
            S(-24, -19),   S( -6, -36),   S(-23, -11),   S(-30, -24),   S(-21, -44),   S(-29, -21),   S(-15, -20),   S(-36, -33),
            S(-11, -33),   S( 20, -44),   S( 11, -16),   S(  8, -29),   S(  6, -27),   S( 56, -36),   S( 37, -42),   S( -7, -62),
            S( 11, -50),   S( 55, -77),   S( 51, -43),   S( 57, -26),   S( 77, -47),   S( 81, -31),   S(138, -97),   S( 37, -91),
            S(101, -94),   S(127, -111),  S(111, -68),   S( 57, -16),   S( 69, -31),   S( 99, -32),   S( 96, -43),   S( 40, -84),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-12, -25),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  4,   2),        // attacks to squares 1 from king
            S(  8,   4),        // attacks to squares 2 from king

            /* castling available */
            S( 70, -74),        // king-side castling available
            S( 15,  64),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 32, -93),   S( 39, -80),   S( 32, -93),   S( 30, -80),   S( 21, -71),   S( 16, -64),   S(  3, -55),   S( -6, -48),
            S(  6, -49),   S( 21, -49),   S( 44, -50),   S( 43, -45),   S(131, -66),

            /* orthogonal lines */
            S(-76, -128),  S(-93, -129),  S(-114, -107), S(-128, -101), S(-136, -97),  S(-142, -99),  S(-142, -106), S(-139, -110),
            S(-155, -98),  S(-164, -97),  S(-164, -105), S(-139, -130), S(-98, -150),  S(-31, -169),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 31, 240),

            /* passed pawn can advance */
            S(-12,  37),   S( -2,  62),   S( 14, 104),   S( 76, 175),

            /* blocked passed pawn */
            S(  0,   0),   S( 30, -28),   S( 32, -14),   S( 31,  36),   S( 28,  70),   S( 33,  46),   S( 68,  84),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 58, -57),   S( 37,   8),   S( 18,  30),   S( 16,  61),   S( 37, 102),   S(128, 140),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-12, -29),   S( -4, -36),   S(  9, -31),   S(-21,   3),   S(-27,  38),   S(120,  28),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 37, -61),   S( 32, -25),   S(  7,   2),   S( 17, -45),   S(  0, -115),  S(-55, -169),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 18,  48),   S( 29,  30),   S(126,  41),   S( 24,  27),   S(171, 126),   S( 98, 134),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 15,  59),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-52, 121),

            /* bad bishop pawn */
            S( -8, -18),

            /* rook on open file */
            S( 29,   6),

            /* rook on half-open file */
            S(  7,  46),

            /* pawn shields minor piece */
            S( 12,  14),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 22,  32),   S( 23,   3),   S( 36,  18),   S( 29,  -2),   S( 33, -19),

            /* pawn threats */
            S(  0,   0),   S( 66, 104),   S( 52, 121),   S( 72,  86),   S( 62,  37),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 36,  67),   S( 49,  52),   S( 76,  44),   S( 47,  68),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 27,  51),   S( 26,  52),   S(-19,  46),   S( 62,  83),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 21,  10),   S( 21,  33),   S( 35,  13),   S(  9,  30),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 15,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
