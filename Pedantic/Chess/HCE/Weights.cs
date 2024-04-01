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

        // Solution sample size: 6000013, generated on Sun, 31 Mar 2024 23:04:33 GMT
        // Solution K: 0.003850, error: 0.083821, accuracy: 0.5070
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 77, 229),   S(385, 676),   S(412, 672),   S(541, 1085),  S(1368, 1822), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(113, -139),  S(150, -99),   S( 43, -45),   S(-39,  44),   S(-12,  -9),   S(-21,  -5),   S(-48,   7),   S(-30, -18),
            S(135, -134),  S(101, -102),  S(  9, -61),   S(-11, -56),   S( -8, -25),   S(-20, -26),   S(-23, -23),   S(-18, -49),
            S(120, -110),  S( 57, -55),   S( 17, -71),   S(  7, -63),   S(  0, -62),   S( 12, -61),   S(-11, -51),   S( 11, -64),
            S( 84, -55),   S( 60, -64),   S( 33, -57),   S( 27, -84),   S( -5, -46),   S( -5, -67),   S(-13, -45),   S(  2, -37),
            S( 87,  37),   S( 56, -12),   S( 52,  -4),   S( 58, -65),   S( 45, -45),   S( 10, -57),   S( -5, -13),   S(-29,  58),
            S( 83,  74),   S( 51,  84),   S(-17,  22),   S( 52, -28),   S( -9,  -1),   S( 32,  36),   S(-17,  58),   S(  3,  58),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 36, -31),   S( 38, -39),   S( 53, -20),   S(  7,  30),   S(-13,   6),   S(  8, -13),   S(-34,   0),   S(-34,  26),
            S( 40, -40),   S( 27, -46),   S( 14, -42),   S(  5, -42),   S( -6, -24),   S( -4, -31),   S(-18, -22),   S(-32, -12),
            S( 28, -39),   S( 19, -35),   S( 23, -55),   S( 20, -60),   S(-11, -32),   S( 24, -48),   S( -7, -39),   S(  7, -30),
            S( 58, -27),   S( 30, -44),   S( 39, -67),   S( 21, -60),   S( -8, -28),   S( 22, -42),   S(-15, -29),   S( -4,   6),
            S(  7,  55),   S(-36,   6),   S(  8, -30),   S( 16, -48),   S( 39, -34),   S( -3,   5),   S(-25,  24),   S(-19,  76),
            S( 30,  61),   S( -5,  -6),   S(-30,   7),   S( -3,  20),   S(-54,   5),   S(-60,  28),   S(-75,  41),   S(-37,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  -4),   S(-18,   1),   S( -7,   0),   S( -1,   7),   S( 10,  -3),   S( 34, -17),   S( 11, -45),   S( -3, -15),
            S(  1, -25),   S(-27, -13),   S(-22, -31),   S(-14, -37),   S(  9, -32),   S(  8, -31),   S(  3, -43),   S(-12, -29),
            S( -8, -28),   S(-19, -31),   S( -5, -60),   S(  1, -62),   S( -1, -29),   S( 25, -41),   S( -1, -41),   S( 17, -34),
            S( -5, -16),   S( -5, -48),   S(-10, -58),   S(  1, -52),   S( 10, -44),   S(  6, -30),   S(  9, -25),   S(  6,  -4),
            S(  0,  37),   S(-37, -13),   S(-35, -46),   S(-26, -48),   S( 19,  -8),   S( 20, -22),   S(-12,  15),   S(-19,  80),
            S(-70,  86),   S(-81,  50),   S(-89, -24),   S(-91, -12),   S(-25,  20),   S(-22,  22),   S(-26,  20),   S(-46, 111),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9, -18),   S(-26,  -3),   S(-20,  -5),   S( 14, -50),   S( -1,  -8),   S( 47, -22),   S( 92, -75),   S( 71, -85),
            S( -3, -43),   S(-26, -28),   S(-21, -38),   S(-12, -36),   S( -6, -28),   S( 14, -43),   S( 67, -81),   S( 67, -79),
            S( -4, -50),   S( -4, -62),   S(  3, -72),   S(  4, -70),   S( -1, -56),   S( 31, -58),   S( 37, -70),   S( 86, -77),
            S(  1, -35),   S(  9, -76),   S(  7, -84),   S( 12, -79),   S( 26, -75),   S( 24, -61),   S( 44, -58),   S( 76, -38),
            S( 27,  10),   S( -7, -36),   S( 16, -75),   S( 21, -75),   S( 92, -60),   S( 79, -45),   S( 64,  -2),   S( 54,  68),
            S(-34,  90),   S( -5,  27),   S( -9, -41),   S(-10, -71),   S( 45, -83),   S( 57, -18),   S( 79,  -6),   S( 90,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-82,  24),   S(  3,  -9),   S(-34,  14),   S( -8,  12),   S(  7, -18),   S(-55,  32),   S(-51,   7),   S(-46,   7),
            S(-38,  22),   S( 74, -31),   S( 43, -40),   S(  6, -18),   S( -6, -15),   S(-37, -30),   S(  3, -48),   S(  2, -24),
            S( 39, -13),   S( 40,  -2),   S(-29,  12),   S(-10, -14),   S(-30, -27),   S( -7, -38),   S(-31, -32),   S( 42, -50),
            S(  4,  30),   S(-21,  43),   S( 12,  16),   S(-12,  12),   S( 14, -41),   S(-34, -34),   S(  4, -42),   S( 32, -23),
            S(-18,  94),   S(-12,  86),   S(-64,  55),   S( -7,  -1),   S( 18,  35),   S(-19,  21),   S(  6, -18),   S( 41,  12),
            S( 90, 106),   S( 47,  99),   S( 19,  57),   S( 14,  12),   S( 32,   2),   S(  9, -16),   S( 26,  21),   S(-36,  11),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-101,  60),  S(-92,  55),   S(-31,  22),   S( 31,  11),   S( 27,  17),   S(-45,  29),   S(-54,  16),   S(-28,  31),
            S(-56,  26),   S(-84,  28),   S( 24,  -8),   S( -4,  15),   S(  0,  -5),   S(-25, -14),   S(-34,  -6),   S(-38,  13),
            S(-45,  37),   S(-69,  33),   S( 57, -30),   S( -6, -22),   S( 18,  -8),   S(-25, -16),   S(-17,  -8),   S(  9,  -6),
            S(-55,  56),   S(-75,  50),   S( -8,   4),   S( 15,  15),   S(-24,   4),   S(-75,   8),   S(  3, -13),   S( 10,  15),
            S( 71,  56),   S( 18,  48),   S( 38,  39),   S(-34,  48),   S(-21,  34),   S( 57,  -8),   S(  0,   5),   S( 54,  31),
            S( 50,  48),   S( 72,  17),   S( 35,   4),   S( 12,  11),   S( 53,  -5),   S(  4,   2),   S( -3,   9),   S( 10,  45),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43,  32),   S(-59,  28),   S(-39,  24),   S(-44,  17),   S( 16, -14),   S(-50,  19),   S(-76,  13),   S(-62,  23),
            S(-32,   4),   S(-16, -19),   S(-26, -25),   S( -7,  -2),   S( 31, -15),   S(  6, -15),   S(-39,  -8),   S(-65,   7),
            S(-14,  -5),   S(-22,  -7),   S(-15, -24),   S(-41,  -2),   S( 11, -10),   S( 56, -36),   S(-15, -19),   S(-15,   3),
            S( -8,   5),   S(-76,  12),   S(-15, -23),   S(-28,   4),   S( -6,   6),   S( 20, -12),   S( 37, -18),   S( 28,   8),
            S(  8,  30),   S(-57,  18),   S(-10, -28),   S(-16,  -2),   S( 39,  28),   S( 41,  42),   S( 53,   5),   S( 67,  33),
            S( 52,  29),   S( 36, -10),   S(-14, -31),   S( 29, -63),   S( 22, -16),   S( 46,  22),   S( 46, -20),   S( 35,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -26),   S(-48, -16),   S(-17,  -6),   S(-79,  28),   S(-23, -19),   S( 27, -23),   S(-12, -46),   S(-37, -24),
            S(-38, -35),   S(-42, -39),   S(-44, -39),   S(  1, -65),   S( -6, -37),   S( 54, -56),   S( 66, -67),   S(  5, -37),
            S(-36, -45),   S(-54, -40),   S(-36, -53),   S( -4, -56),   S(-12, -30),   S( 43, -45),   S( 42, -58),   S( 60, -46),
            S(-24, -37),   S(-28, -63),   S(-77, -57),   S(-35, -25),   S( -4, -31),   S( 21, -21),   S( 33, -14),   S( 76, -24),
            S( 26, -40),   S( 34, -78),   S(-29, -49),   S( 15, -77),   S( 47, -12),   S( 15,  -3),   S(102,  25),   S(115,  27),
            S( -4, -12),   S(-40, -28),   S(  9, -57),   S(-14, -41),   S( 23, -52),   S( 28, -13),   S( 72,  29),   S(106,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94,  70),   S(-27,  78),   S(  0,  28),   S(-16,  -9),   S( 13,   3),   S( 18, -10),   S(-56,  -6),   S(-40,  29),
            S(-77,  78),   S(-37,  42),   S(-22,  21),   S(-19,  18),   S(-12,  -4),   S(-34, -21),   S(-54, -12),   S( 32, -16),
            S(-54, 115),   S(-18, 113),   S(-25,  60),   S(-49,  54),   S( 16, -23),   S(-127,   7),  S(-70, -16),   S(-70,  -5),
            S(-13, 144),   S( 10, 175),   S( 10, 104),   S(  0,  70),   S(-40,   7),   S(-41, -25),   S(-18, -16),   S(-30,  10),
            S(-15, 169),   S( 64, 157),   S(  9, 165),   S( 61, 117),   S( 12, -12),   S(  9,   2),   S(-13,  -1),   S( -8,  11),
            S( 49, 205),   S( 75, 214),   S( 91, 206),   S( 41,  84),   S(-14,  19),   S(-21,   5),   S(-10, -13),   S(  5,  39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-107,  82),  S(-91,  64),   S( 22,  13),   S( 20,  29),   S( 18,  11),   S(-26,   9),   S(-51,  13),   S(-103,  39),
            S(-41,  41),   S(-58,  39),   S(-65,  34),   S(-38,  62),   S(-57,  -1),   S( -2, -19),   S(-114,   8),  S(-25,  -2),
            S(-84,  75),   S(-110,  95),  S(-62,  92),   S(-127,  98),  S(-57,  50),   S(-87,   9),   S(-19, -35),   S(-14,  -5),
            S(-78, 123),   S(-27, 117),   S(  3, 117),   S( 27, 145),   S(-52,  67),   S(-51,  13),   S(  9,  -5),   S(-60,  22),
            S(  9, 126),   S( 11, 144),   S( 19, 162),   S( 53, 171),   S( 38, 131),   S(  9,  36),   S(-16,   7),   S(-14,  -6),
            S( 24,  62),   S( 13, 134),   S( 56, 156),   S( 77, 189),   S( 28, 113),   S(-25,  -3),   S( -5, -26),   S(-33, -38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  36),   S(-53,  -8),   S(-12,  -8),   S( 16,  26),   S( -2,   9),   S(-98,  45),   S(-108,  25),  S(-30,  15),
            S(-90,   7),   S(-89,   0),   S(-22, -17),   S( -8, -19),   S(-20,  20),   S(-11,  12),   S(-118,  37),  S(-106,  19),
            S(-23, -10),   S(-87,   9),   S(-46,  11),   S(-96,  77),   S(-118, 108),  S(-26,  55),   S(-100,  40),  S(-97,  44),
            S(-116,  40),  S(-53,  15),   S(-30,  18),   S(-70, 104),   S( 29, 102),   S(-55,  86),   S(-59,  59),   S(-33,  44),
            S(-12,  32),   S(-31,  16),   S( 26,  28),   S( 35, 137),   S(111, 125),   S( 18,  72),   S(-11,  91),   S( 36,  42),
            S(  3,  15),   S(-27,   5),   S( 14,   8),   S( 57, 113),   S(  5, 148),   S( 25,  73),   S(-11,  80),   S( 10, 103),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-83, -14),   S(-79,  14),   S( 40, -22),   S( -2,  35),   S(  3,  -2),   S(-110,  56),  S(-43,  30),   S(-34,  24),
            S(-42, -19),   S(-92, -15),   S(-45, -34),   S(-30,   3),   S(-62,  34),   S(-15,  21),   S(-106,  68),  S(-97,  51),
            S(-42, -36),   S(-45, -49),   S(-29, -17),   S(-20,  15),   S(-79,  46),   S(-33,  71),   S(-98,  89),   S(-53,  74),
            S(-64,   8),   S(-84, -22),   S(-38, -23),   S(-60,  24),   S( 18,  25),   S( 16,  72),   S(  3, 120),   S( 72,  82),
            S(-20,  36),   S(-53,  -7),   S(-15,   2),   S(-16,  12),   S( 71,  68),   S(-16, 119),   S( 80, 128),   S( 64, 117),
            S(-39,  45),   S(-21, -12),   S( 17,  14),   S(-14, -32),   S( 21,  66),   S( 35, 144),   S( 70, 166),   S( 62, 188),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -18),   S(-33,  -5),   S(-18, -18),   S(  1,   3),   S( -7, -16),   S(-18,  33),   S( -6, -50),   S(-13, -29),
            S(-40, -16),   S( -1,  25),   S( 12,  34),   S(  8,  23),   S(-15,  27),   S(  3, -19),   S(-43, -18),   S(-24, -47),
            S(-28,  50),   S(-24,  88),   S(  9,  75),   S( 21,  38),   S(  2,   4),   S(-54, -12),   S(-42, -75),   S(-49, -69),
            S(-24,  90),   S(-37, 117),   S( 63, 143),   S( 13,  84),   S(-11, -15),   S(-51, -18),   S(-33, -27),   S(-59, -75),
            S( 49,  97),   S( 41, 235),   S( 37, 136),   S( 10,  46),   S(  6,  18),   S( -1, -38),   S( -4, -23),   S(-22, -27),
            S( 42, 114),   S( 50, 228),   S(115, 191),   S( 53, 120),   S( -1,  18),   S( -9,  -9),   S(-11, -31),   S(-15, -36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -13),   S(-28, -22),   S(  5,  16),   S(  0,  10),   S( -7, -22),   S(-42, -13),   S(-26, -28),   S(-44,   5),
            S(-40, -38),   S(-50,  37),   S(-22,  20),   S( 18,  56),   S(-55,  23),   S(-23, -13),   S(-72, -43),   S(-73,   5),
            S(-57,  53),   S(-55,  58),   S(-66,  78),   S( -1, 100),   S( -5,  27),   S(-48, -30),   S(-64, -24),   S(-70, -37),
            S(-97, 103),   S(-11, 115),   S(-13, 154),   S( -5, 102),   S( 12,  68),   S(-32,   1),   S(-12, -19),   S(-30, -21),
            S(-16,  76),   S( 61, 161),   S( 62, 177),   S( 56, 236),   S( 28, 150),   S( -9,  12),   S( 15, -58),   S(-39, -53),
            S( 53,  46),   S( 66, 192),   S( 86, 200),   S( 75, 251),   S( 43, 129),   S( -7,  14),   S( -4,   3),   S(  2, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -63),   S(-40, -14),   S(  4, -28),   S( -4,   0),   S( -3,  -2),   S(-46,  31),   S(-44, -32),   S( -8,  31),
            S(-72,  17),   S(-54,  -4),   S(-52, -36),   S( 14,   1),   S(-37,  77),   S(  0,  21),   S(-41,  11),   S(-50,  16),
            S(-47, -28),   S(-69,  -1),   S(-37, -38),   S( -8,  50),   S(-14,  86),   S(-35,  31),   S(-26,  -3),   S(-48,  45),
            S(-51,  -4),   S(-26,  75),   S(-26,  42),   S( -4,  96),   S( -3, 156),   S(-22,  85),   S(-36,  49),   S(-52,  42),
            S(-26, -31),   S(  8,   6),   S( 15,  91),   S( 40, 154),   S( 51, 240),   S( 46, 173),   S(  4,  93),   S( 21,  47),
            S(  4,  16),   S(  1, -28),   S( 23, 111),   S( 27, 154),   S( 61, 205),   S( 69,  97),   S( 35, 112),   S( 28,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -40),   S(-37, -39),   S(-19, -39),   S( -2,   2),   S( 15,  29),   S(  5,  46),   S(-15, -18),   S( 16,  60),
            S(-51, -43),   S(-47, -14),   S(-16, -52),   S( 36,   1),   S(-23,   7),   S( -8,  28),   S( -5,  39),   S(  5,  30),
            S(-31, -65),   S(-25, -32),   S(-24, -44),   S(  8,   1),   S( 33,  13),   S( -3,  50),   S( -6,  91),   S(-20,  70),
            S(-34, -37),   S(-45, -48),   S(-30,  28),   S( -2,   1),   S(-17,  40),   S( 15,  75),   S(-45, 146),   S(  0,  70),
            S(-29, -49),   S(-29, -24),   S(-19,  13),   S(  2,  10),   S( 45, 138),   S( 61, 154),   S( 36, 243),   S( 75,  72),
            S( -3,  37),   S( -1,   6),   S(  0,   9),   S(  7,  35),   S( 30, 101),   S( 84, 178),   S( 25, 156),   S( 27,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-26,  -6),   S(  4,  -3),   S(-41,   2),   S(-32,  -2),   S(-54,  13),   S(-15, -13),   S(-53, -40),   S(-18, -19),
            S(-39,  42),   S(  6, -52),   S(-43,  24),   S( -6, -27),   S( -9, -19),   S(-13, -20),   S( -1, -29),   S(-92,  -3),
            S(  6,  70),   S( -2, -16),   S( 13, -12),   S(-19,  47),   S(  8,  12),   S(-31,  10),   S( -3,  -9),   S(-31, -43),
            S( 18, -51),   S( 46, -26),   S( 19,  25),   S( 22,  38),   S( 13,  22),   S(-11,  29),   S( -2,  -5),   S( -7,  -7),
            S( 13, -21),   S( 41,  15),   S( 18,  22),   S( 65,  -9),   S( 36,   5),   S( 18,  42),   S( 14, -11),   S(-73,  22),
            S( 35,  -4),   S( 10,  -3),   S( 35,  22),   S( 67, -12),   S( 30, -50),   S( 20,  -2),   S(  3, -26),   S(-11, -11),
            S(-27, -36),   S( -8, -59),   S( 26, -20),   S( 26, -31),   S( 39, -40),   S(  8, -44),   S( -6, -38),   S( -6, -46),
            S(-63, -66),   S(-24,  -2),   S(-10, -12),   S(  3, -64),   S(-23, -21),   S( 33,  35),   S( -9, -12),   S(  6,  -2),

            /* knights: bucket 1 */
            S(-68,  25),   S(-66,  80),   S( -1,  54),   S(-20,  56),   S(-29,  36),   S(-18,  32),   S(-48,  61),   S(-40, -30),
            S( 14,  10),   S( -7,  46),   S(-25,  31),   S(-18,  57),   S( -6,  27),   S(  5,   9),   S(-10,  -6),   S(-16, -13),
            S(-22,  26),   S(  4,  16),   S(  7,  11),   S( 26,  34),   S( 17,  34),   S(-24,  34),   S( -4,   9),   S(-26,   9),
            S(  3,  32),   S( 49,  39),   S( 24,  47),   S( 14,  37),   S( 20,  37),   S( -2,  41),   S( 37,  -2),   S( 12,  15),
            S( -2,  54),   S( 10,  36),   S( 48,  22),   S( 38,  28),   S( 38,  32),   S( 30,  18),   S( 28,  18),   S( 14,  45),
            S( 21,   1),   S(  9,  17),   S( 53,  31),   S( 54,  16),   S( 36,  17),   S( 43,  30),   S(  6,   8),   S(  4,  -6),
            S( 50,  -8),   S( 46,  16),   S(-38,  -3),   S(-13,  42),   S(  3,  14),   S( 42,  -8),   S(-63,  30),   S( 10, -24),
            S(-119, -82),  S(-24, -14),   S(-47, -18),   S(  5,  16),   S(  5,  12),   S(-24, -39),   S(-10, -21),   S(-60, -65),

            /* knights: bucket 2 */
            S(-42, -19),   S( -6,  31),   S(-20,  48),   S(-22,  45),   S(-27,  53),   S(-34,  69),   S(-25,  42),   S(-40, -14),
            S( -9, -21),   S(-23,  10),   S(-10,  21),   S(-13,  34),   S( -1,  25),   S( -9,  57),   S(-29,  49),   S(-54,  72),
            S(-18,  17),   S(  5,   9),   S( -9,  35),   S( 23,  26),   S(  4,  32),   S(  7,  14),   S(  7,  30),   S(-27,  20),
            S( -9,  35),   S(-30,  41),   S( -2,  53),   S( 10,  45),   S(  7,  49),   S(  5,  37),   S(  5,  41),   S( -4,  41),
            S( 16,  26),   S( -9,  37),   S( -6,  48),   S(-20,  51),   S(  5,  49),   S( -7,  44),   S(  2,  46),   S( -7,   7),
            S( -3,  29),   S(  1,  23),   S(-14,  54),   S( -8,  38),   S(-31,  65),   S( 10,  35),   S(-14,  14),   S( 36, -11),
            S(-31,  14),   S(-33,   1),   S(-32,  28),   S(-47,  32),   S( 14,  -1),   S( -8,  41),   S(-24,  43),   S(-74,  26),
            S(-153,  23),  S( -1,  -9),   S(-105,  36),  S(-29, -17),   S( 16,  15),   S(-60, -13),   S(  4,   6),   S(-194, -25),

            /* knights: bucket 3 */
            S(-61,  -1),   S(  2, -22),   S(-41,  12),   S( -7,  -1),   S( -8,   3),   S(-14,   2),   S( 17, -40),   S(-26, -21),
            S(-24,  15),   S(-21,  -5),   S(-17,  -1),   S(  9,  11),   S( 21,  -9),   S( -6,  -4),   S( -1,  -5),   S(-11,  39),
            S( -1, -32),   S( 11, -10),   S(  5,   4),   S( 28,  11),   S( 22,  18),   S( 27,   4),   S( 18,   2),   S(  6,  27),
            S( -1,   0),   S( 15,   3),   S( 17,  42),   S( 25,  33),   S( 33,  35),   S( 32,  34),   S( 41,  17),   S( 28,  21),
            S( 29,   8),   S(  9,  26),   S( 42,  10),   S( 26,  47),   S( 29,  48),   S( 24,  58),   S( 43,  44),   S( 20,  10),
            S(  8,   4),   S( 37, -24),   S( 62,  -6),   S( 69,   6),   S( 75,  -7),   S( 88, -13),   S( 29,  -3),   S( 22,  36),
            S( 22, -13),   S( -4,   4),   S( 70, -36),   S( 64,  -9),   S( 64, -28),   S( 56, -45),   S( 31, -62),   S( 51, -24),
            S(-159,  21),  S(-43, -19),   S(  1, -38),   S( 13,  -1),   S( 26,  -1),   S(-26,  -6),   S(-10, -34),   S(-85, -70),

            /* knights: bucket 4 */
            S(  1,  -2),   S(-68,  25),   S( 27,  30),   S(-15,  20),   S( -7,  -5),   S(-43, -16),   S(-13, -37),   S(-37, -53),
            S( 22,  16),   S(-31,  30),   S(  5, -35),   S( 21, -17),   S( 70, -24),   S( -5, -44),   S(  7, -11),   S(  1, -60),
            S( -6,  31),   S( 10,  60),   S( 32,  -4),   S( 41,  15),   S(-13,  -1),   S(-47,  26),   S(-30, -46),   S(-17, -57),
            S(-10,  60),   S( 37,  12),   S( 48,  45),   S( 43,  10),   S( 25,  11),   S( 80,  -1),   S( 34, -22),   S( -4, -10),
            S( 65,  35),   S(-15,  25),   S( 38,  50),   S( 76,  11),   S( 65,  48),   S( -6,  25),   S(  2, -74),   S(-44, -15),
            S(-19,  11),   S(-25, -24),   S(102,  21),   S( 22,  29),   S( -1,  20),   S( 21,  57),   S( 30,  68),   S(-17, -28),
            S(  6,   0),   S(-23, -18),   S( -1,  -7),   S(-13,  31),   S(  3,  10),   S( 17, -23),   S(-11,   4),   S(-21,  -7),
            S( -6,  -8),   S( -6, -14),   S(  6, -10),   S(-13,  -7),   S(  1,   5),   S(  8,  15),   S( -2,  14),   S( -7, -20),

            /* knights: bucket 5 */
            S(  6,  19),   S(-41,  15),   S( 10,  38),   S( 11,  42),   S( 47,  14),   S( 11, -13),   S( -6,   1),   S(  5,  18),
            S( 23,  17),   S( 44,  59),   S( 10,  30),   S( 26,  26),   S( 47,  25),   S( 16,  36),   S( -2,  34),   S(-23, -32),
            S( 21,  40),   S(-49,  53),   S( 60,  14),   S( 59,  43),   S(-25,  52),   S( 17,  24),   S( -7,  21),   S( -4,  -6),
            S( 43,  35),   S( 21,  46),   S( 60,  36),   S(  3,  61),   S( 24,  61),   S( 14,  59),   S( 34,  66),   S(  0,  51),
            S(  8,  69),   S( 31,  46),   S( 42,  62),   S( 64,  41),   S(131,  40),   S( 11,  48),   S( 42,  55),   S( 29,  10),
            S( 15,  37),   S( 12,  48),   S(-12,  52),   S( -6,  60),   S( 54,  44),   S(  0,  51),   S( 13,  21),   S(-13,   7),
            S( 21,  70),   S(-10,  60),   S( 45,  73),   S( 21,  64),   S(  6,  63),   S(  2,  43),   S( 27,  70),   S( -6, -13),
            S( -3,  -7),   S( 10,   2),   S(  3,  35),   S(-12,   8),   S( 22,  45),   S( -5,  18),   S(  5,  41),   S(-22, -41),

            /* knights: bucket 6 */
            S( -4, -80),   S(  4, -14),   S( 20,  22),   S(-13,  26),   S(-51,  45),   S(-27,  52),   S(-45,  28),   S( -3,  34),
            S(  0, -44),   S( 64,   5),   S( 25,  10),   S(-29,  29),   S(-81,  81),   S( 27,  47),   S( 14,  52),   S( -5,   2),
            S(-18, -40),   S( -1,  -6),   S( 13,  16),   S( 12,  42),   S(  1,  55),   S(-28,  55),   S( 34,  38),   S( 16,  60),
            S( 27,  11),   S( 67,   0),   S( 57,  38),   S( 83,  35),   S( 52,  37),   S( 25,  60),   S( 22,  55),   S(  8,  69),
            S(  7,  26),   S( 74,   0),   S( 46,  37),   S( 75,  29),   S( 95,  39),   S(119,  35),   S( 58,  44),   S( 38,  37),
            S( 19,  18),   S( 42,  -1),   S( 61,  27),   S( 44,  46),   S( 59,  53),   S( 66,  25),   S( 12,  35),   S( 37,  51),
            S(-21,  31),   S(-13,  19),   S(-35,  36),   S( 37,  36),   S( 12,  66),   S( 25,  45),   S(  3,  70),   S(-17,  31),
            S(-44,  -7),   S( 26,  71),   S( 35,  37),   S(  0,  24),   S( 35,  24),   S(  3,  46),   S( 17,  52),   S(  1,   6),

            /* knights: bucket 7 */
            S(-48, -53),   S(-209, -39),  S(-94, -21),   S(-67, -19),   S(-31, -11),   S(-60,  -8),   S( -8,   4),   S(-37, -15),
            S(-35, -97),   S(-37, -55),   S(-27, -32),   S(-23, -17),   S(-79,  41),   S(  9, -10),   S( -2,  69),   S( -5,   6),
            S(-97, -68),   S(-45, -24),   S(-39,   0),   S(  7,  -7),   S(  0,  -7),   S( 30,  -1),   S(-15,  70),   S( 17,  74),
            S(-69, -22),   S(  3, -21),   S(-32,  30),   S( 33,   6),   S( 38,   8),   S( 28,   7),   S( -2,  25),   S(-16,  31),
            S(-62, -25),   S(-14, -24),   S( 28,   6),   S( 82, -13),   S(115,  -4),   S( 95,  14),   S( 81,  14),   S( 76,  14),
            S(-22, -33),   S( 12, -41),   S(-20,  -5),   S( 42,   1),   S( 72,  14),   S( 80,  34),   S( 73, -28),   S(-16,  51),
            S(-49, -18),   S(-56, -23),   S(-13,   2),   S( 21,   9),   S( 17,  32),   S( 58,  -2),   S(-11,  -7),   S( -8, -38),
            S(-48, -34),   S(-17, -19),   S(-39, -21),   S( 17,  15),   S(  9,   7),   S( 15,  12),   S( -8, -26),   S( -1, -13),

            /* knights: bucket 8 */
            S( -3, -12),   S(-14, -13),   S( -5, -15),   S( -2, -11),   S( -2,  -1),   S(-14, -59),   S( -1,   8),   S( -3, -15),
            S(  4,  -1),   S(-10, -25),   S(-13, -29),   S(-15, -45),   S(-34, -47),   S(-17, -64),   S( -5, -46),   S(-22, -47),
            S(  1,  19),   S(-11, -29),   S( 15,   3),   S(-21, -12),   S( 17,   1),   S(-19, -11),   S(-27, -49),   S( -1, -38),
            S(-23,  18),   S( -5, -14),   S( -5,   4),   S( -4,  35),   S( -2,  -5),   S( 14,  32),   S( -7, -47),   S( -4,  -7),
            S( 30,  56),   S(  8, -12),   S( 12,  55),   S( 41,  36),   S( -7,  42),   S( -3,  13),   S(  1, -19),   S(-12, -15),
            S(  9,  38),   S(  7,  49),   S(  9,  21),   S( 34,  22),   S(  8,   2),   S(  2,   2),   S(-14, -47),   S(-12,  -8),
            S(  4,   4),   S(  8,  19),   S( 11,  19),   S( 22,  40),   S( -7,  -4),   S( 13,  36),   S(  4,   0),   S(-13, -26),
            S(  1,  -2),   S( 10,  18),   S( -2, -19),   S(  3,  -4),   S(  4,   8),   S( -2, -10),   S(  7,   9),   S( -3,  -8),

            /* knights: bucket 9 */
            S( -9, -21),   S( -7,   3),   S(-12,  -6),   S(  5,  -1),   S(-28, -54),   S(-30, -56),   S(  1,  -5),   S( -6, -34),
            S(-17, -68),   S(-13,  15),   S( -8, -58),   S( -8,  22),   S(  4,   8),   S(-11, -42),   S( -7, -16),   S(-12, -40),
            S(  7,  -8),   S( -8,  -6),   S(  7,   7),   S(  7,   9),   S(  2,  25),   S(-31,  18),   S(-17,  -6),   S( -7, -14),
            S(-12,  22),   S(-16,  11),   S( 11,  20),   S( 49,  26),   S( 29,  15),   S( 11,  28),   S(-30, -46),   S( -3,  -4),
            S(  1,  39),   S(  4,  18),   S( -1,  46),   S( 18,  50),   S(  4,  22),   S(  9,  16),   S( 14,  -5),   S(  2,  -4),
            S(  0, -11),   S( 19,  53),   S(  7,  53),   S(-18,  14),   S( 41,  43),   S( 17,  18),   S( 11,  15),   S(-13, -37),
            S( -2, -16),   S( -4,  13),   S( 27,  35),   S( 20,  14),   S( 15,  50),   S(  4,  13),   S( -2,   2),   S(  2,   4),
            S(  0,  -2),   S(  0,  11),   S(  6,   8),   S( 14,  17),   S(  6, -13),   S(  4,  12),   S(  5,   7),   S( -4,  -8),

            /* knights: bucket 10 */
            S(-18, -54),   S(-23, -51),   S(-16, -42),   S(-27, -37),   S(-20, -38),   S(-17, -64),   S( -1,  16),   S(  6,  34),
            S( -7, -23),   S(-17, -37),   S( -7, -43),   S(-17, -27),   S(-17, -28),   S(-12, -45),   S(-21, -39),   S( -4,   7),
            S( -9, -59),   S(-10, -51),   S(-15,   2),   S(-15, -19),   S( 10, -10),   S(-32, -14),   S( -5,  -5),   S( -9,  20),
            S( -8, -15),   S(-12, -53),   S( -7, -32),   S( 17,  32),   S( -4,  55),   S( 22,  40),   S(  4, -11),   S( 19,  33),
            S(-10, -72),   S(-22, -25),   S(  7,   1),   S( 27,  51),   S( 42,  43),   S( 17,  41),   S( 21,  17),   S( 22,  49),
            S(-15, -55),   S(-21, -56),   S(-19, -19),   S( 33,  54),   S( 45,  66),   S( 22,  40),   S( 28,  77),   S( 17,  47),
            S( -2, -13),   S( -5, -35),   S(  4, -20),   S( 30,  43),   S( 23,  18),   S( 17,  65),   S( -5,   5),   S( 13,  33),
            S(  1,   1),   S(  1,   5),   S( -2,  10),   S( -7, -36),   S(  5,  26),   S(  5,   9),   S(  1,  17),   S( -3, -16),

            /* knights: bucket 11 */
            S(  0,  -4),   S(-17,  -5),   S( -1, -40),   S(-13, -39),   S(-21, -43),   S( -7,  -7),   S( -3,  -9),   S( -5,  -8),
            S(-12,   1),   S(-22, -46),   S(-18, -59),   S(-19, -10),   S(-11, -10),   S(-38, -48),   S(-17, -18),   S( -5, -18),
            S(-11, -25),   S(-20, -74),   S(-19, -29),   S( -4,  -6),   S(-25,  -7),   S(-15,  46),   S( -5, -24),   S(-13, -19),
            S(-15, -40),   S(-18, -39),   S(-20,  -6),   S( 25,  36),   S( 16,  41),   S(  2,  25),   S( -9,  11),   S( 17,   7),
            S(  0, -26),   S(-19, -77),   S( 22,  -9),   S( -4,   0),   S( 31,  15),   S( 36,  62),   S(  6, -11),   S( 23,  61),
            S(-12, -26),   S(  5, -11),   S( 17,  26),   S( 30,  21),   S( 34,  44),   S( 50,  47),   S( 18,  33),   S(  9,   0),
            S( 15,  44),   S(  4,   0),   S(  2, -25),   S( 10, -20),   S( 10,  14),   S(  2,  -3),   S(  5,  28),   S( 14,  51),
            S( -6,  -6),   S(  2, -18),   S( 19,  34),   S( 10,  12),   S( -5,  11),   S(  1,   5),   S(  8,  18),   S(  3,  18),

            /* knights: bucket 12 */
            S( -4, -17),   S( -4,  -5),   S(  2, -17),   S(  2,  18),   S( -3, -10),   S(  2,   0),   S(  2,  -3),   S( -3, -15),
            S(  1,   3),   S( -1,  -3),   S(  3,  12),   S( -1, -18),   S(-16, -14),   S( -4, -24),   S( -1, -16),   S(  6,  26),
            S( -4, -17),   S(  2,   8),   S( -3,  -5),   S( -7, -30),   S(  2,  11),   S( -7, -18),   S(  1,  -5),   S( -3, -15),
            S( -9, -23),   S(  2,   8),   S(-11, -26),   S(  1,  41),   S(  0,  10),   S(  1,  11),   S( -1,  -5),   S( -3, -13),
            S( 10,  17),   S(  4,   6),   S(  0,  14),   S( 14,  11),   S( -6, -41),   S( -1,  18),   S(  1,  -6),   S(  5,  14),
            S( -5, -28),   S(-12,  -6),   S(  9,  13),   S(  2,   3),   S( -5,  -5),   S( -6, -13),   S( -4, -11),   S( -1,  -6),
            S(  1,   6),   S(  3,  19),   S( -4,  -9),   S( -1, -31),   S( -9, -10),   S( -9, -32),   S(  0,   4),   S(  0,  -4),
            S(  4,   5),   S(  4,  10),   S(  0,  -3),   S( -1,   3),   S( -3,  -8),   S( -5, -15),   S( -4, -15),   S(  0,   1),

            /* knights: bucket 13 */
            S( -7, -16),   S( -3,  -1),   S( -2, -10),   S( -5, -10),   S( -9, -15),   S( -2,  -5),   S( -2,  -8),   S( -3, -16),
            S( -3, -11),   S( -1,  -4),   S(  4,   0),   S( -8,  -1),   S( -7, -30),   S( -4, -21),   S( -3, -10),   S( -1, -10),
            S(-11, -37),   S( -4, -14),   S(  7,  15),   S(  2,  15),   S(-11, -29),   S(-10, -34),   S( -9, -18),   S( -7, -35),
            S( -8, -12),   S(  7,  11),   S(  3,  16),   S(-15, -29),   S( -4, -42),   S( 11,  27),   S(  0, -28),   S( -3,   1),
            S(  5,  13),   S( -5,   6),   S( 13,  10),   S( 11,   6),   S(  7,  -2),   S(  7,  19),   S(-10, -38),   S(  1,  -1),
            S(  1,   0),   S( 11,   1),   S(  7,  19),   S(-10,   7),   S( -8, -22),   S( -9, -20),   S(  5,  15),   S( -6, -11),
            S(  2,   5),   S( -7, -10),   S(  9,  17),   S( -2, -26),   S( 25,  39),   S( -8, -24),   S(  0,   8),   S( -2,   3),
            S( -2,   4),   S( -1, -12),   S(  2,   5),   S(  4,   4),   S( -2,  -4),   S(  1,  -5),   S( -1,  -8),   S( -2,  -3),

            /* knights: bucket 14 */
            S(  0, -13),   S(-11, -40),   S( -3, -12),   S( -2,  10),   S(-14, -26),   S(  2,   2),   S( -1,  -2),   S(  3,   5),
            S( -1,  -9),   S( -6, -14),   S(-20, -82),   S( -7, -37),   S(  2, -15),   S(  0,   3),   S( -2,  -3),   S(  0,   2),
            S( -4, -11),   S( -2, -17),   S(-13, -60),   S(  1,  -8),   S(  1, -10),   S( -1, -17),   S(  1,  -1),   S(  2,   4),
            S( -1,   0),   S( -2, -16),   S( -8, -33),   S( -6, -34),   S(-10, -27),   S(  6,  -2),   S(  2, -17),   S(-17, -35),
            S( -4, -10),   S( -1, -13),   S(  6,  39),   S( -5, -25),   S( -9, -25),   S(  8,  21),   S(  5,  16),   S( -1,   1),
            S( -4, -20),   S(  3,  -7),   S( -4, -12),   S(  7,  28),   S( 18,   2),   S(  1,  14),   S(  1,   3),   S(  0,  -6),
            S(  2,   1),   S( -1,  -7),   S(  5,  -1),   S(  8,   2),   S( -5, -14),   S( -4, -15),   S( -2,   8),   S( -1,  -2),
            S( -1,  -2),   S(  5,   9),   S(  0, -10),   S( 11,   6),   S(  1,   7),   S( -1,   2),   S( -5, -22),   S(  0,  -5),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -1, -13),   S( -3, -31),   S(-11, -22),   S( -1,   1),   S( -1,  -3),   S(  0,  -1),   S( -2,   2),
            S( -2, -15),   S(  0,  -6),   S( -5, -10),   S(  5,   7),   S(  4,   7),   S(  3,  11),   S( -2,  -1),   S( -1,  -2),
            S( -8, -23),   S(  3,  -5),   S( -3, -15),   S(-19, -49),   S(  1,   5),   S( -5,  -5),   S(  5,  13),   S( -5, -11),
            S( -6, -22),   S( -7, -21),   S( -7, -23),   S(-13, -40),   S( 14,   3),   S(  5,  36),   S(  2,   9),   S( -2,  11),
            S(  0,   5),   S( -1,   0),   S(  7,  -1),   S( -5, -16),   S(  1,  18),   S( 10,  32),   S( -5,  -2),   S( -3,  -9),
            S( -4,  -6),   S( -3,   2),   S( -3, -22),   S( -5,   8),   S( -3, -16),   S( -8,  16),   S( -2,   1),   S( -3,   4),
            S( -2, -14),   S( -5, -19),   S( -2, -14),   S( -4,   4),   S(-10,  -7),   S(  0,  24),   S( -1,   1),   S(  3,  15),
            S(  0,   0),   S( -1,  -2),   S( -4, -12),   S( -1,  -4),   S( -4,  -2),   S(-18, -15),   S(  7,  23),   S( -2,   2),

            /* bishops: bucket 0 */
            S( 10,   3),   S( 14,   1),   S( 44,  16),   S(-21,  32),   S(-17,   6),   S( -6, -12),   S(  5, -53),   S( -2, -28),
            S( 47, -66),   S( 76,  26),   S( 43,   2),   S( 16,   1),   S( -2,  20),   S(  0,  -6),   S(-37,  26),   S( 11, -48),
            S( 15,  49),   S( 41,  14),   S( 46,  22),   S(  0,  58),   S( 20,  15),   S(-16,  45),   S(  6,  -2),   S( -5, -35),
            S( 14,  35),   S( 62, -11),   S( 46,   6),   S( 38,  48),   S( 15,  20),   S( 46, -20),   S( 23, -46),   S(-10,  36),
            S( 27, -24),   S( 19,  30),   S( 21,  34),   S( 52,  18),   S( 64,  -5),   S(  3, -10),   S( -1,   4),   S(-32,   3),
            S( -1,  39),   S( 17,  -5),   S( 63,  -8),   S( 82, -14),   S( 53,  17),   S( 44,   4),   S(-29,  36),   S( 11,  -3),
            S(-38,   0),   S(-25,  36),   S( 63, -13),   S(-20,  38),   S(-60,  -3),   S(  1,  13),   S( 34,  -9),   S( -8,   7),
            S(-34, -21),   S( 32,   0),   S( -4, -17),   S(-14, -17),   S( 34,  29),   S( 28,  -6),   S( -3,  66),   S( -8,   4),

            /* bishops: bucket 1 */
            S( 51,  17),   S(-17,  28),   S(  4,  20),   S(  1,  21),   S(  4,  16),   S(-13,  20),   S( -2,  -9),   S(-24, -19),
            S(  6, -18),   S( 39, -11),   S( 46,   5),   S( 31,  14),   S( -7,  14),   S( 10,  -4),   S(-31,  22),   S( 19, -42),
            S( 41, -19),   S( 19,  -3),   S( 57,   7),   S( 16,  23),   S( 17,  23),   S(-17,  26),   S( 35, -10),   S(  5, -27),
            S( 19,   2),   S( 13,  30),   S(  9,  18),   S( 50,  15),   S( -6,  36),   S( 26,  -5),   S(  1,   9),   S( 19,  -6),
            S( 48,   2),   S( 16,  13),   S(  5,  43),   S( 11,  31),   S( 29,   5),   S( -4,  26),   S( 11, -10),   S(  8, -15),
            S(-40,  40),   S(  3,  48),   S( 21,  38),   S( 45,  -9),   S(  2,   9),   S( 48,  11),   S( 53,   3),   S( 36, -19),
            S(  0,  23),   S( 23,  31),   S(-11,  54),   S( 13,  21),   S( 44,  27),   S( 15,  14),   S( 45,  -5),   S(-14,  12),
            S( 37, -21),   S( 21,  -8),   S( 11,   4),   S(-15,  29),   S( -5,  23),   S( 17,  23),   S(  4,  15),   S( 12,  26),

            /* bishops: bucket 2 */
            S( 26,  -6),   S(  5,   7),   S(  2,  10),   S(-33,  46),   S(-10,  20),   S(-29,  23),   S(-23,  17),   S(-29,  42),
            S(-15,   3),   S( 18,  -7),   S( 13,  10),   S(  3,  21),   S( -2,  29),   S( 14,  -6),   S(  1, -18),   S(  0, -43),
            S( -4,  -2),   S(-12,  22),   S( 19,  26),   S( -6,  42),   S(  5,  29),   S(  2,  33),   S( 23,   0),   S(-20,  -7),
            S( -1,  12),   S( -6,  13),   S(-17,  33),   S( 10,  39),   S(  0,  34),   S( 11,  16),   S(  8,   7),   S(  6, -18),
            S(  7,  -4),   S(-24,  33),   S(-13,  22),   S(-26,  47),   S(-15,  42),   S( -5,  36),   S( -3,  -3),   S(-41,  25),
            S(  4,  22),   S( -1,  16),   S(-10,  51),   S(-26,  29),   S( 15,  -5),   S(-14,  42),   S( -2,  55),   S(  3,  21),
            S( -4,   9),   S(  7,  21),   S(-23,  39),   S( 38, -16),   S(  6,   8),   S( -4,  -1),   S(-39,  20),   S(-14,  28),
            S(-68,  49),   S(  5,  43),   S(-57,  24),   S(-34,  22),   S(-38,  23),   S(-45,  10),   S(  4,  12),   S(-73,  34),

            /* bishops: bucket 3 */
            S(  9,  20),   S( 22,   6),   S( 16,  16),   S(  5,  19),   S(  8,  15),   S( 43, -15),   S( 37, -28),   S( 32, -44),
            S(  9,  -3),   S( 10,  20),   S( 28,  -3),   S( 15,  25),   S( 26,   1),   S( 13,  19),   S( 52,   9),   S( 22, -20),
            S( 17,   1),   S( 10,  25),   S( 28,  37),   S( 26,  19),   S( 18,  56),   S( 28,  28),   S( 39,  21),   S( 45, -19),
            S( 33, -13),   S( 16,  20),   S( 19,  40),   S( 32,  49),   S( 31,  32),   S( 32,  30),   S( 25,  16),   S( 23,  -2),
            S( 16,   5),   S( 22,  19),   S( 44,  12),   S( 36,  43),   S( 18,  49),   S( 39,  33),   S( 12,  25),   S( 23,  36),
            S( 34,  -1),   S( 43,  11),   S( 29,  32),   S( 38,  20),   S( 16,  26),   S( 60,  29),   S( 53,   9),   S( -1,  56),
            S(  1,   2),   S( -6,  45),   S( 45,  22),   S( 34,  13),   S( 16,   6),   S( 35,   1),   S(  0,  65),   S( 24,  15),
            S(-23,  94),   S(-15,  14),   S( 65,   4),   S( 16,  29),   S( 15,  21),   S(-10,  28),   S( 28,  -9),   S( 76, -11),

            /* bishops: bucket 4 */
            S( -5, -16),   S(-39,  18),   S(-33,  -2),   S(-30,   9),   S(-27,  20),   S(-87,  22),   S( 12, -26),   S(-11,   5),
            S(-24, -20),   S( 23,  18),   S( -8,  21),   S(-27,  22),   S(-18, -13),   S( 28, -10),   S(-15,  15),   S( 15,  -3),
            S(-21,  -6),   S(-62,  45),   S( -3,  28),   S(-13,  12),   S( 40,  20),   S( 36, -18),   S(-31, -15),   S(-47,  -9),
            S(-41,  20),   S( -3,  21),   S( 28,  45),   S( 34,  42),   S(  4,  26),   S( 41,  -7),   S( 43,  -7),   S(-12, -55),
            S(-26,  40),   S( 20,  38),   S(-28,  57),   S( 31,  34),   S( 51, -18),   S( 17, -34),   S(-31, -12),   S( 23,  12),
            S(  8,  29),   S( 35,  42),   S(  5,  45),   S( -2,  16),   S( 68,  -5),   S( 13,   4),   S( 18, -43),   S( 21,   1),
            S(-32,  -5),   S( 34,  40),   S(  8,   6),   S( 19,  24),   S(-10,  -7),   S(-17,   3),   S(  6, -12),   S( -1, -38),
            S( 19,  -8),   S( -2, -36),   S(  6,  -5),   S(-13,  -5),   S(-16,  -6),   S( -4,  20),   S( -4, -18),   S(  0,  -3),

            /* bishops: bucket 5 */
            S( -2,   3),   S(-38,  25),   S(-52,  36),   S(-53,  38),   S(-42,  23),   S(-21,  14),   S( -9,  35),   S(-22,  55),
            S(-33,  22),   S( -9,  13),   S(-32,  46),   S( 17,  16),   S(-39,  38),   S(  1,  10),   S(-52,  10),   S(-19, -12),
            S( -4,  19),   S(  4,  30),   S( 16,  37),   S(-30,  60),   S(-28,  48),   S(-19,   3),   S( -2,  23),   S(-27,  -5),
            S( 40,  23),   S( 22,  28),   S(-19,  63),   S( 10,  35),   S( 23,  36),   S( 37,  23),   S( 18,  11),   S(  8,  45),
            S( 27,  28),   S(-11,  28),   S( 72,  26),   S( 83,  23),   S( 34,  23),   S( 68,   7),   S( 43,   7),   S( -1,   6),
            S( 25,  29),   S( 49,  49),   S( 25,  62),   S( 71,  22),   S( -4,  28),   S(  0,  21),   S(-28,  50),   S( -1,  30),
            S(  4,  29),   S(  0,  11),   S( 12,  28),   S(  0,  43),   S( 41,   7),   S( 24,  46),   S(-13,  26),   S( -2,  39),
            S( 11,   7),   S( 10,  40),   S(  4,   9),   S(  8,  28),   S( 20,  38),   S(  8,  18),   S( 27,  56),   S(-14, -12),

            /* bishops: bucket 6 */
            S(  4,  20),   S( 30,   2),   S(-65,  39),   S(-60,  31),   S(-52,  15),   S(-62,  39),   S(-21,  60),   S(-15,   8),
            S( -7,  25),   S(-17,   6),   S(-12,  29),   S(-15,  22),   S(-22,  32),   S(-13,   5),   S(-110,  52),  S( 18,  16),
            S( 29, -22),   S( -8,  13),   S( 41,  12),   S( 13,  36),   S( 44,  16),   S( 19,  22),   S(  2,  30),   S(-35,  18),
            S(-43,  61),   S( 21,  10),   S( 32,  21),   S( 47,  29),   S( 40,  26),   S( 44,  13),   S(  8,  29),   S(-18,  -2),
            S( 23,   2),   S( 37,  21),   S( 16,  18),   S( 68,  19),   S(108,  21),   S(110,  14),   S( 31,  16),   S(-65,  55),
            S(-13,  20),   S(-35,  44),   S(  2,  35),   S( 64,  17),   S( 44,  17),   S( 31,  34),   S(  0,  45),   S(-30,  48),
            S(-10,   6),   S(-10,  43),   S( -2,  31),   S(-14,  16),   S( 16,  41),   S(  4,  47),   S( 19,  43),   S(-19,  15),
            S(  2,  53),   S(  0,  41),   S(  5,  30),   S(  4,  35),   S(  5,  29),   S( 50,   8),   S( -9,  35),   S( 17,  11),

            /* bishops: bucket 7 */
            S(-36,  -5),   S( -6,  -9),   S(-32, -35),   S(-68,  30),   S(-22, -27),   S(-78,  20),   S(-78, -29),   S(-66,  20),
            S(-47, -46),   S(-50, -29),   S(-11,  -1),   S(-15, -15),   S(-31,   4),   S(-16,  -4),   S(-47,   6),   S(-30,   9),
            S(-45, -23),   S( 17, -28),   S( 23,  -2),   S( 42, -16),   S(-44,  28),   S( -3,  -5),   S(-34,  41),   S(-68,  37),
            S(-94,  34),   S( 55, -22),   S( 84, -23),   S( 55,  21),   S( 94,  -8),   S(  9,  10),   S( 16,  38),   S(  8,  33),
            S( 11, -53),   S( -7, -27),   S( 66, -35),   S(110, -22),   S( 72,  32),   S( 72,  10),   S(  5,  38),   S(  6,  35),
            S(-27, -13),   S(-20,   7),   S( 79, -38),   S( -9,   6),   S( 34,  -3),   S( 72,  25),   S( 93,   0),   S( 18,   1),
            S( -4, -46),   S(-38,  27),   S(-15,   3),   S(  9, -13),   S(  1, -20),   S( 50, -25),   S( 42,  20),   S( 47,   5),
            S( -9,   3),   S(-35,  14),   S(-40,  12),   S(  2,  -1),   S(  3,   0),   S( 51, -23),   S( 22,  -3),   S( 41,  48),

            /* bishops: bucket 8 */
            S(-16, -21),   S(-18, -27),   S(-54, -17),   S( -9, -22),   S( -3,  54),   S(-10, -36),   S(  5,   8),   S( -5, -13),
            S(-16,  -6),   S(-32, -44),   S(-16, -27),   S(-17,   8),   S(  7, -12),   S(-28,   0),   S( -3, -33),   S( -9, -11),
            S( 10,  14),   S(-20,  22),   S( -7,  31),   S(-26,  41),   S( 10,  17),   S(  9,   7),   S( 18, -22),   S(-48, -62),
            S( 16,  20),   S( -7,  61),   S( 19,   5),   S(  3,   9),   S( 11,   3),   S( 23,  20),   S(  1, -26),   S( -3, -26),
            S( 22,  62),   S( 14,  77),   S( -9,  67),   S( 54,  27),   S( -3,   8),   S( 36,  32),   S( 13,  -9),   S( -9, -35),
            S(-10,   6),   S(  1,   0),   S( 44,  23),   S(-25,  41),   S( 36,  20),   S(-20, -14),   S(-23, -36),   S(-10, -18),
            S( -4,  -3),   S( 29,  38),   S( 15,  14),   S( -7, -11),   S(  2, -11),   S(  8,  25),   S( -7,  -2),   S(-17, -54),
            S( -3,  -9),   S( -5, -21),   S(-11, -45),   S( -7, -38),   S(-10, -22),   S(  0,   5),   S( -3,   6),   S( -7,   4),

            /* bishops: bucket 9 */
            S(-11, -13),   S( -8,   6),   S(-10,  18),   S(-12, -45),   S(-45, -30),   S(-47, -32),   S(-26, -21),   S(  6,   5),
            S(-11, -22),   S(-35, -19),   S(-22, -26),   S(-26,  16),   S(-49,  26),   S(-13, -31),   S(-14,  22),   S(  1,  -9),
            S(  4,   4),   S(  0,  18),   S(-43,  33),   S( -6,  18),   S( 16, -10),   S( 15, -17),   S(  4, -13),   S( -3,  19),
            S(-28, -22),   S( 33,  34),   S(-35,  43),   S( -4,  16),   S( 11,  25),   S( 11,  -3),   S( -6,  11),   S(-24, -29),
            S( -1,  -1),   S( 60,  22),   S( 15,  28),   S(  1,  28),   S(  8,  10),   S( 10,  22),   S( -4,   2),   S( -3, -33),
            S(-23, -15),   S( 26,  35),   S( -5,  32),   S( 13,  13),   S( 23,  33),   S(  5,  47),   S(-23,  31),   S( -7,  -3),
            S(  8,  16),   S( 22,  38),   S(  2,   7),   S(-13,  71),   S(  8,  19),   S( -3, -26),   S( -1, -23),   S( -1,   5),
            S(  8,   0),   S(  3,  -1),   S(  6,  42),   S(-12,  -2),   S(-27, -41),   S( 14,  22),   S( -3,  -4),   S(-14, -26),

            /* bishops: bucket 10 */
            S(-33,  -7),   S( -3, -31),   S(-49, -20),   S(-27, -29),   S(-19, -21),   S(-21, -18),   S(-15, -36),   S(-22, -24),
            S( 10,  -1),   S(-21, -49),   S(-14,   5),   S(-35,  29),   S(-53,   8),   S(-17,  13),   S(-19, -39),   S(-12, -30),
            S(  3, -19),   S( -8, -16),   S(-52,  -9),   S( -7,  -1),   S(-28,  21),   S(-45,  43),   S(-17,  26),   S(  1,  37),
            S(-18, -25),   S( 19,  -1),   S( 49,  -4),   S( 42, -15),   S( 46,  38),   S(-35,  45),   S(-12,  42),   S( 12,  17),
            S( -9,   4),   S(-27,   8),   S( -9,   0),   S( 42,  19),   S( 24,  48),   S( 20,  51),   S( 34,  51),   S(  9, -12),
            S( -5, -26),   S(-22,  14),   S( -7,  10),   S(-22,  36),   S( 21,  39),   S( 42,  56),   S(  5,  56),   S( 12,  34),
            S(-20,   8),   S(  1, -48),   S(-22, -16),   S( 26, -22),   S( -1,   6),   S( 24,  41),   S( 27,  48),   S( 13,  20),
            S( -4, -25),   S(-13,  -6),   S( 19,  17),   S(-10,   3),   S(  2,   6),   S( -7, -21),   S(  1,  -7),   S(  9,  41),

            /* bishops: bucket 11 */
            S( -5,  11),   S(-37, -32),   S(-61, -52),   S(-15, -38),   S(-14, -27),   S(-57, -33),   S(-12, -15),   S(-22, -19),
            S(-11,   7),   S(  9,   2),   S( -1, -10),   S(-18, -74),   S(-38,  -8),   S(-24, -31),   S( -4, -55),   S(-30, -44),
            S( -3, -75),   S( 15, -59),   S(-22, -14),   S(-15,   1),   S(-10,  -6),   S(-28,  38),   S(-15,  22),   S(-18, -23),
            S(-12, -45),   S(-19, -48),   S(  5,  -8),   S(  6,   5),   S( 27,   1),   S( -3,  86),   S( 13,  64),   S( 28,  24),
            S( -7, -11),   S(-27, -23),   S(-13,  29),   S( 27,  24),   S( 43,   8),   S(  1,  80),   S( 16,  80),   S(  7,   5),
            S(-31, -37),   S(-23,  14),   S(-11,  -9),   S( -9,  26),   S( -2,  23),   S(  4,  69),   S( 34,  53),   S( -8, -25),
            S( -6, -13),   S(-21, -54),   S(-13,   3),   S(-19, -20),   S( 15,  21),   S( 42,  -6),   S( 20,  38),   S(  8,  28),
            S(-17,  -9),   S(-19,  -2),   S(  0,  -4),   S(  5, -28),   S( 24,  27),   S(-22, -47),   S(  2,  25),   S(  6,  -3),

            /* bishops: bucket 12 */
            S( -5,  -4),   S( -8,  -9),   S(-12, -59),   S( -2, -17),   S(-12, -11),   S(-10, -24),   S(  1,  24),   S( -9,   8),
            S( -7,  -5),   S( -4, -13),   S( -5,   9),   S( -9, -17),   S(-14, -35),   S( -4,   6),   S( -3,  11),   S( -2,  -8),
            S(  1,   5),   S(-15, -17),   S( -2,  -5),   S(  0, -16),   S(-10,  15),   S(  6,  12),   S(-14, -55),   S(  3,  -9),
            S( -1, -15),   S(  0, -14),   S(-19, -43),   S(  5,  26),   S(  7,   2),   S( -1,   6),   S( -1,  -5),   S(-10, -18),
            S( -6, -12),   S( -7,   2),   S(  7,  15),   S(-11,  14),   S(  0,   2),   S(  2,   8),   S(  8,  10),   S( -7,   1),
            S(-20, -16),   S(  1,  57),   S(-10,   0),   S(-17, -16),   S(  1,  -9),   S( -4,  10),   S(  1,   6),   S( -2,  -6),
            S( -5,  -9),   S(  6,  38),   S(  5,  11),   S( -7,  13),   S( -7,   6),   S( 19,  43),   S( -2, -17),   S( -7, -10),
            S(  0,   2),   S( -4, -15),   S(-15,   0),   S( 14,  35),   S(  3,  13),   S( -3,  -6),   S( -6,  -5),   S(  0,  -2),

            /* bishops: bucket 13 */
            S( -7, -38),   S(-10, -10),   S( -9,  -5),   S(-17, -13),   S(-18, -28),   S(-17,   5),   S( -6, -19),   S(-10, -13),
            S( -5,   0),   S( -9, -15),   S(-11, -56),   S(-19,  -9),   S(-15,  24),   S( -7,  31),   S( -3, -22),   S(  0,   6),
            S( -3,  -1),   S(  2,   4),   S(-21,  -2),   S(-25,  -4),   S(-10, -32),   S( 13,   1),   S( -8, -39),   S(  9,  30),
            S(  0,  -4),   S(-10,  -4),   S(-23, -11),   S(-26, -10),   S(  4,  30),   S(  2,   6),   S(  0,   0),   S(-10, -23),
            S( -4,  12),   S(-17,   2),   S(-21,  13),   S( 33,  23),   S( -2,  26),   S(-13,  -4),   S( -8, -12),   S( -6, -31),
            S( -3,   3),   S(-13, -19),   S( -8, -17),   S( 16,  27),   S( -6,   8),   S( 10,  11),   S( -2,   9),   S(-10,  -9),
            S( -8, -15),   S( -5, -10),   S(  1,  25),   S(  4,  38),   S(-11,  -6),   S(  5,  13),   S(-12, -40),   S(  0,   3),
            S( -3,  -3),   S( -1,   8),   S(-11, -24),   S(  6,  -6),   S( -2, -11),   S( -5,   2),   S( -2,   5),   S( -2, -12),

            /* bishops: bucket 14 */
            S( -8,  -9),   S(-11, -23),   S(-23, -51),   S(-26, -62),   S(-12, -27),   S( -7, -19),   S(-10, -12),   S( -6, -21),
            S( -9, -25),   S( -5, -14),   S(-10,  16),   S(-26, -45),   S( -8,  -7),   S(-19,   1),   S(-15, -20),   S(  4,  -7),
            S( -7,  -8),   S(-13, -40),   S(-12, -33),   S(  0, -16),   S(-25, -37),   S(-12, -32),   S( -9,   4),   S( -7,  -7),
            S( -7, -10),   S( -6,  -6),   S( -5, -13),   S(-21,  35),   S( -6,  22),   S(-20,  31),   S(-23,   1),   S( -9, -21),
            S( -8,  -1),   S(-18,  16),   S(  0,  -8),   S( 10,  -5),   S( -3,   6),   S(  6, -17),   S(  6,  18),   S(  3,  -9),
            S(  0,  16),   S(-14,   8),   S(-12,  26),   S(-19, -29),   S(  8,  23),   S( 10,  50),   S(-21,   3),   S(-10, -25),
            S( -6, -30),   S(  4,  -7),   S( -5,   9),   S(  4,  -3),   S( -9, -13),   S(-10,  -9),   S( -4, -15),   S( -5,  -2),
            S( -3,  -9),   S( -6, -12),   S(  0,   6),   S( -9, -10),   S( -9, -17),   S(  1,   8),   S(  0,   4),   S(  0,   4),

            /* bishops: bucket 15 */
            S(  3,  16),   S(  7,  -3),   S(-16, -32),   S( -4, -31),   S( -7,   2),   S(-13, -16),   S( -4,  -4),   S(  0,  -7),
            S(  5,   6),   S(  1,  22),   S( -1,  -9),   S( -4,  -8),   S(-22, -37),   S( -3, -13),   S( -4, -17),   S( -2,   6),
            S( -8, -24),   S( -8, -30),   S( -5,  16),   S( -4,   0),   S(-26, -18),   S(  0,  11),   S(-11,  -7),   S(  2,  21),
            S( -4,  -9),   S(-15, -22),   S(  0, -13),   S(-20, -27),   S(-14,  -4),   S(  3,  -7),   S(  9,  30),   S( -5, -20),
            S( -3,  -5),   S( -5, -17),   S(-20, -15),   S(-20, -49),   S( -3, -24),   S(-12,  15),   S( 11,  47),   S( -5, -24),
            S( -7, -39),   S(-15,  -6),   S( -6, -11),   S(-20,   5),   S(-15,   5),   S(-25, -50),   S(  8,  19),   S( -6,  -3),
            S( -2,   1),   S( -3, -26),   S(  1, -10),   S( -7,  -3),   S(-11,   3),   S( -1,  22),   S(-20,  -5),   S( 10,  15),
            S( -6, -12),   S( -6,   1),   S(-10,  -8),   S(-11, -22),   S( -7, -19),   S(-18, -16),   S( -9, -22),   S(  0,  -4),

            /* rooks: bucket 0 */
            S(-35,  -1),   S( -9,   7),   S(-10, -27),   S( 12, -15),   S(  7,   4),   S(  0,   5),   S( -4,  15),   S( 11,  14),
            S( 17, -71),   S( 32, -31),   S(  0,  14),   S( -2,  -9),   S( 25,  -9),   S( -1,   6),   S(-20,  16),   S(-35,  34),
            S( 13, -34),   S( 10,  47),   S( 45,  -2),   S( 16,   2),   S(  4,  26),   S(  1,   9),   S(-37,  39),   S(-52,  25),
            S( 59, -46),   S( 65,  -2),   S( 41,  31),   S( 17,  24),   S( 15,   9),   S(-10,  38),   S( -9,  29),   S(-45,  42),
            S( 69, -29),   S( 76,   3),   S( 52,  22),   S( 20,   6),   S( -4,  45),   S( 18,   6),   S( 15,  33),   S(-22,  43),
            S( 99, -65),   S(102, -46),   S( 96,  -7),   S( -9,  34),   S( 53,  33),   S(-27,   5),   S( 38,  27),   S(-34,  36),
            S( 41,  -1),   S( 59,   8),   S( 15,  17),   S( 22,  22),   S(-15,  35),   S( 16,  14),   S( 10,  30),   S(-17,  29),
            S( 66,   7),   S( 21,  47),   S(  9,  37),   S( -3,  35),   S( 18,   9),   S( 20,  15),   S( -2,  21),   S( 21,  30),

            /* rooks: bucket 1 */
            S(-76,  32),   S(-35,  -6),   S(-60,   7),   S(-42,  -5),   S(-16, -30),   S(-29,  -4),   S(-43,   7),   S(-36,  23),
            S(-44, -10),   S(-77,  14),   S(-11, -15),   S(-25, -42),   S(-28, -23),   S(-33,  -9),   S(-40, -22),   S(-86,  30),
            S(  5,   3),   S(-23,  28),   S(-18,   1),   S(-38,  23),   S(-46,  34),   S( -9,  -2),   S(-21,  -8),   S(-62,  25),
            S(-62,  69),   S(-39,  24),   S( 21,  17),   S(-28,  27),   S(-12,  20),   S(-66,  56),   S(-46,  62),   S(-42,  21),
            S( 58,   7),   S( 38,  34),   S( 11,  10),   S(-51,  50),   S(-18,  33),   S( 50,   8),   S(  6,  17),   S(-61,  26),
            S( 65,  -2),   S(-13,  40),   S(  8,  29),   S(-49,  41),   S( 18,   2),   S(-27,  36),   S(-10,  20),   S(-51,  36),
            S(-11,  37),   S( -3,  39),   S( 17,  35),   S(-72,  72),   S(-13,  28),   S(-27,  54),   S(-63,  35),   S(-18,  24),
            S( -5,  38),   S( 14,  44),   S(  9,  24),   S(-63,  51),   S( 30,   6),   S( 41,  11),   S(-16,  44),   S(  5,  18),

            /* rooks: bucket 2 */
            S(-72,  44),   S(-51,  29),   S(-48,  17),   S(-55,  17),   S(-59,   8),   S(-49,   0),   S(-47, -31),   S(-56,  25),
            S(-77,  41),   S(-64,  37),   S(-49,  30),   S(-69,  19),   S(-39,  -8),   S(-63,  -4),   S(-83,  12),   S(-59,  -4),
            S(-74,  63),   S(-48,  55),   S(-50,  57),   S(-29,  11),   S(-52,  26),   S(-34,  21),   S(-21,   8),   S(-41,  19),
            S(-89,  72),   S(-49,  60),   S(-41,  65),   S(-41,  54),   S(-34,  44),   S(  3,  27),   S(-36,  58),   S(-18,  27),
            S(-41,  57),   S(-61,  75),   S(-62,  74),   S(-23,  45),   S( 30,  11),   S(  0,  38),   S(-21,  34),   S(-30,  38),
            S(-35,  45),   S(-29,  42),   S(-16,  33),   S( -7,  18),   S(-14,  29),   S( 11,  35),   S( 21,  17),   S(-36,  44),
            S(-55,  45),   S(-58,  71),   S(-34,  55),   S(-11,  51),   S(-24,  47),   S( 15,  27),   S(-59,  72),   S(-16,  43),
            S(-56,  71),   S(  0,  39),   S(-60,  52),   S(-25,  36),   S(-44,  67),   S(-17,  54),   S(-61,  80),   S(-28,  46),

            /* rooks: bucket 3 */
            S( -2,  69),   S( -3,  68),   S(  5,  55),   S( 15,  47),   S( 13,  42),   S(-11,  63),   S( -8,  68),   S(-10,  40),
            S(-37,  88),   S(-16,  67),   S(  1,  60),   S( -3,  64),   S(  9,  47),   S(  8,  56),   S( 44, -14),   S( 20, -43),
            S(-38,  87),   S( -5,  79),   S(  8,  75),   S( 16,  52),   S( 11,  69),   S( 18,  67),   S( 24,  68),   S(  7,  33),
            S(-31,  91),   S(-26,  89),   S(  9,  79),   S( 19,  66),   S( 22,  72),   S( -3, 110),   S( 58,  63),   S( 10,  64),
            S(-19,  95),   S( 17,  78),   S( 14,  68),   S( 35,  69),   S( 46,  58),   S( 47,  65),   S( 70,  68),   S( 50,  51),
            S(-16,  93),   S( 15,  72),   S(  5,  72),   S( 10,  67),   S( 20,  55),   S( 49,  47),   S( 87,  30),   S( 86,  25),
            S(-29,  98),   S(-27, 110),   S(-19, 101),   S( 30,  79),   S( 22,  67),   S( 47,  63),   S( 61,  78),   S( 89,  32),
            S(-77, 157),   S(-13, 110),   S(  6,  64),   S( 38,  59),   S( 50,  67),   S( 32,  70),   S(136,  24),   S( 94,  40),

            /* rooks: bucket 4 */
            S(-79,  17),   S(-22,  27),   S(-33,  15),   S( 21,  -7),   S(-37,  -2),   S( 22, -50),   S(  9, -33),   S( 15, -55),
            S(-33,  16),   S(-24, -21),   S(-28,  -2),   S( 10,  -5),   S(-33,  10),   S( -5, -29),   S( -9, -12),   S(-40,  -4),
            S( 17,   7),   S(-38, -50),   S(-47,  27),   S(  3, -19),   S(  2,   5),   S( -3,  -3),   S( 43, -22),   S(-50,   7),
            S(-42, -21),   S( 11,  -6),   S(-34,  17),   S( 31,  -3),   S( 49,  -4),   S( 24,  -6),   S( -9,  27),   S(  8,  18),
            S(  5, -25),   S(-17,  15),   S(  5,  22),   S( 62,  18),   S( 32,  13),   S( 22,  18),   S( 23,  42),   S( 45,  -8),
            S(  9,   5),   S(  4,  30),   S( 38,  18),   S( -3,  16),   S( 34,  16),   S( 18,  48),   S(  0,  33),   S( 20,  31),
            S( 10,  -5),   S( 16,  36),   S( 25,  23),   S( 40,  21),   S( 58,  26),   S( 13,  -2),   S( 27,  28),   S( 17,  29),
            S( 47, -58),   S( 46,  45),   S( 21,  42),   S( 26,  15),   S( 33,  35),   S(  8,  25),   S( 14,  18),   S( 48,  11),

            /* rooks: bucket 5 */
            S(-35,  39),   S(-41,  59),   S(-37,  39),   S(-61,  40),   S(-21,  13),   S(-21,  29),   S(-18,  38),   S(-44,  65),
            S(  0,  26),   S(-39,  36),   S(-88,  72),   S(-62,  39),   S(-39,  36),   S(-15,  12),   S( 12,  25),   S(-61,  44),
            S( 18,  32),   S(-73,  77),   S(-65,  64),   S(-82,  56),   S(-43,  26),   S(-23,  38),   S( -2,  45),   S( -4,  26),
            S(-43,  75),   S(  0,  48),   S(-24,  48),   S( -5,  34),   S(-19,  69),   S(  2,  59),   S(  8,  61),   S( 44,  28),
            S( -8,  68),   S(-18,  71),   S( 16,  56),   S( 11,  66),   S( 33,  50),   S( 40,  54),   S( 77,  80),   S( 30,  56),
            S( 33,  76),   S( 21,  73),   S( 43,  60),   S(  2,  65),   S( 57,  59),   S( 55,  50),   S( 58,  49),   S( 21,  66),
            S( 52,  48),   S( 14,  63),   S( 55,  48),   S( 56,  56),   S( 37,  52),   S( 42,  53),   S( 77,  53),   S( 90,  36),
            S( 99,  26),   S( 66,  39),   S( 39,  49),   S( 30,  28),   S( 47,  49),   S( 44,  47),   S( 63,  38),   S( 20,  58),

            /* rooks: bucket 6 */
            S(-46,  28),   S(-29,  29),   S(-21,  21),   S(  3,   0),   S(-53,  39),   S(-85,  61),   S(-52,  63),   S(  0,  30),
            S(-25,  26),   S(-24,  39),   S(-41,  52),   S(-50,  28),   S(-37,  34),   S(-75,  62),   S(-85,  59),   S( 36,  -2),
            S(-66,  70),   S(-45,  49),   S(-36,  54),   S(-51,  44),   S( -5,  28),   S(-26,  60),   S(-45,  69),   S( 10,  41),
            S(-78,  86),   S(  8,  50),   S(-19,  59),   S( 18,  28),   S( 17,  31),   S(  8,  57),   S(-46,  52),   S(-11,  53),
            S(-15,  72),   S(  8,  66),   S( 51,  47),   S( 24,  43),   S( 13,  58),   S( 17,  65),   S( 19,  56),   S(-17,  62),
            S( -9,  71),   S( 32,  58),   S( 80,  32),   S( 54,  25),   S( 17,  34),   S( 21,  77),   S( 32,  58),   S( 33,  51),
            S( 56,  54),   S( 86,  42),   S(104,  25),   S( 66,  33),   S(115,  21),   S( 61,  41),   S( 28,  65),   S( 26,  66),
            S( 63,  73),   S( 33,  62),   S( 20,  49),   S( 46,  49),   S( 71,  41),   S( 51,  69),   S( 67,  50),   S( 28,  51),

            /* rooks: bucket 7 */
            S(-78,  -6),   S(-31, -22),   S(-31, -22),   S(-27, -10),   S( -2, -25),   S(-45,  26),   S(-51,  22),   S(-10,   2),
            S(-101,  43),  S(-13, -12),   S( -6, -22),   S(-14,  -3),   S(-10,   4),   S( -4,  25),   S(-12,  -2),   S(-44,   6),
            S(-87,  58),   S(-38,  15),   S(-18,  17),   S( 24, -14),   S(  2,   8),   S(-21,  -7),   S( -4, -15),   S(  0,  22),
            S(-62,  41),   S(-41,  32),   S( 19,  10),   S( 21,  21),   S( 14,  12),   S( 36,   6),   S(  5,  12),   S(-38,  25),
            S(-20,  37),   S( 14,  14),   S( 53, -20),   S( 53,  -2),   S( 76,  -5),   S( 94,   4),   S( 55,  23),   S( 26,  -8),
            S(-31,  42),   S(-15,  28),   S( 79, -21),   S( 96, -18),   S( 65,  -1),   S( 93,   9),   S( 69,   8),   S( 31,  -5),
            S(-12,  37),   S( 52,  16),   S( 20,  18),   S( 85,  -3),   S( 96, -10),   S( 80,  11),   S( 58,  14),   S( 41,   1),
            S( 20,  61),   S(-19,  49),   S(  4,  14),   S( 72, -21),   S( -9,  33),   S( -5,  15),   S( 69,  -9),   S( 58,   1),

            /* rooks: bucket 8 */
            S(-41, -63),   S( -2,   8),   S(  8,  -7),   S(-10, -29),   S(-13, -29),   S(-31, -63),   S(  5, -30),   S(-18, -29),
            S(  7,   1),   S(-24, -30),   S( -5, -22),   S( 16,  -4),   S(-18, -28),   S( -8,  -9),   S(  1, -40),   S(-30, -73),
            S( -1,  19),   S( 16,  18),   S(  9,  -9),   S( -3,   7),   S(-26, -60),   S(  4, -27),   S( 11,  15),   S(  0,  -1),
            S(-18, -15),   S(  3,  21),   S( 11,   0),   S( 30,  21),   S( -4,  18),   S(  1, -25),   S( 12, -16),   S( -1, -13),
            S( -8, -38),   S(-16,   8),   S( 11,  32),   S(  0, -10),   S( 14,  10),   S( 25,  10),   S(  5,  -9),   S( 15, -36),
            S( -1,  53),   S(-21, -11),   S( 35,  42),   S( 18, -19),   S( -8, -10),   S(  6, -12),   S( -8,   7),   S(  7,  32),
            S(  1, -18),   S( 15, -27),   S( 31,   7),   S( 35, -43),   S( 38,  11),   S( 27, -28),   S( 35,  -7),   S( 10,  -1),
            S( 12, -138),  S( 27, -24),   S( 17,  17),   S( 22,  16),   S( 10,   3),   S( 16,  19),   S( 11, -30),   S( 20,   4),

            /* rooks: bucket 9 */
            S(-52, -14),   S( -9, -28),   S(-26, -23),   S(-61,  14),   S(-39,  28),   S(-13, -11),   S( 27, -57),   S(-37, -20),
            S( 34, -11),   S(  2, -22),   S( -1, -24),   S(-18,  -6),   S( -4, -14),   S( 22,  26),   S( -3, -26),   S( -4, -26),
            S( 12, -13),   S(  8,  -3),   S( -7,  23),   S( -9, -17),   S(  7, -29),   S( 33,  -4),   S(  5,  22),   S(-10, -11),
            S(  4,  24),   S( 16,  11),   S( 10,  12),   S(  9,   0),   S(  8,   1),   S( 28,  -2),   S( 30,  23),   S(  3,  -6),
            S( -1,  18),   S( -2,  -4),   S(  7,  32),   S( 17,  15),   S( 24,   3),   S( 14,  13),   S( 15,   5),   S(-10, -12),
            S( 27,  29),   S( -2,   8),   S( 18,   3),   S(-15,   2),   S( 11,  25),   S( 29, -10),   S( 14,  25),   S( 20,   2),
            S( 44,  19),   S( 75, -22),   S( 45,  20),   S( 50,   4),   S( 27,   9),   S( 29,   2),   S( 48,   1),   S( 53,  28),
            S( 80, -76),   S( 43, -49),   S( 27,  22),   S( 39,  43),   S( 13,  26),   S(  7,  17),   S( 23,   3),   S( 49,   5),

            /* rooks: bucket 10 */
            S(-39, -74),   S(-25, -52),   S(-66, -25),   S(-28,  -4),   S(-36,  10),   S(-57,  -9),   S(  8, -11),   S(-32,  -9),
            S( -2, -15),   S( 17, -15),   S(-10, -52),   S( 16, -12),   S( -5,  -7),   S(  4, -18),   S( 50, -10),   S( -7, -10),
            S(-23, -33),   S(-15,  -9),   S(  9, -33),   S( -6,  11),   S(-32,   2),   S( -2,   3),   S( 16,  10),   S( 15,  13),
            S(-15,   0),   S( 15,  -2),   S( -6, -13),   S( 13,  12),   S( 18,  13),   S( 13,  -3),   S( 42,  44),   S( 16,   1),
            S(  9,  20),   S( 44,  -1),   S(  6,   9),   S( 17, -10),   S( -1,  13),   S( 17,  -1),   S( 28,  40),   S( 22,  38),
            S( 40,  33),   S( 31,  54),   S( 13,   4),   S( 28,  21),   S(  1, -11),   S( 10,  15),   S( 41,  14),   S(  8,  32),
            S( 82,  25),   S( 74,   3),   S( 85, -25),   S( 72, -14),   S( 37, -15),   S( 60,  18),   S( 14,   2),   S( 27,  12),
            S( 51,  13),   S( 11, -13),   S( 34,  -5),   S( 36,  22),   S( 36, -30),   S( 33,   1),   S( 11,   3),   S( 17, -21),

            /* rooks: bucket 11 */
            S(-28, -43),   S(-30, -41),   S( -7,  -5),   S(-35, -68),   S(-19,  -9),   S( -4, -12),   S(-19, -17),   S(-54,   8),
            S(-36, -43),   S(-17, -35),   S( 16, -28),   S( 10, -54),   S(-34, -31),   S( -6, -19),   S(  6, -35),   S(-19,  -6),
            S(-21, -38),   S( 13, -27),   S( 27, -13),   S( 13,   5),   S( 15, -23),   S(-15, -12),   S( -8, -17),   S(  0, -46),
            S(  9,  31),   S( -4,   3),   S(  2,   3),   S( 31,  14),   S( 12, -30),   S(  7,  29),   S( 20, -11),   S(  9,   6),
            S( 14,   7),   S( 16, -10),   S( 14,  -5),   S( 30, -28),   S( 29,   8),   S( 28, -10),   S(  4,  16),   S(  3, -13),
            S( 28,  30),   S( 54,   2),   S( 26,   2),   S( 76,   6),   S( 56,  19),   S( 26,  25),   S(-27, -19),   S( 27,  38),
            S( 64,  54),   S( 68,  12),   S( 73, -33),   S( 94,   3),   S( 54, -17),   S( 48,  22),   S( 49,  77),   S( 63,  -1),
            S( 50,  38),   S( 24,  32),   S( 22,   9),   S( 18,  12),   S(-20,  -8),   S( 32,  23),   S( 15,   6),   S( 27, -12),

            /* rooks: bucket 12 */
            S(  1,   3),   S( -7, -29),   S( -5, -29),   S( -3,  12),   S(-17, -30),   S(  1, -39),   S(-34, -70),   S( -6,   6),
            S(  9,  -3),   S( -9, -32),   S( -8, -25),   S(  3, -11),   S(-10,  -3),   S( -8, -37),   S(-12, -23),   S( -5,  -9),
            S( 17,  24),   S(-15, -38),   S( -8, -27),   S(-17, -10),   S(  0, -13),   S( 16,   2),   S(  0,  -6),   S(  3, -15),
            S(  0,  -4),   S(  4, -32),   S(  0, -22),   S(  5, -20),   S(  5,   2),   S( -8, -33),   S(-18, -22),   S(  6, -21),
            S(-17, -33),   S(  2, -41),   S( 15,  -3),   S(  9,   9),   S(-10, -38),   S(  4, -24),   S( -3,  -5),   S(  8,  -7),
            S(-11, -32),   S(  8,  -9),   S( 10,  48),   S(  2,  -9),   S( -6, -18),   S( -8, -20),   S( 13,   0),   S(  7,  13),
            S(-17, -39),   S(-11, -51),   S(-16, -53),   S( 12, -18),   S( -2, -19),   S( -2, -31),   S( -2,   6),   S(  4, -37),
            S( -3, -36),   S(  1,   4),   S(  6, -26),   S(  8,  23),   S( -2, -29),   S( -8, -46),   S(-11, -17),   S(  8,  -9),

            /* rooks: bucket 13 */
            S(-10, -19),   S(  3,  -5),   S(  9, -11),   S( -8,  17),   S( -5, -18),   S(-12, -28),   S( 12,  -5),   S(-16, -46),
            S( -2,  15),   S(  1, -11),   S( -9, -16),   S( -9,  -9),   S(-23, -43),   S(-13, -30),   S(  6,  15),   S( -2, -33),
            S( -9, -42),   S(-10, -26),   S( -2, -40),   S( -6, -46),   S(  6,   6),   S( -5,   2),   S(  7,  -6),   S( -1, -44),
            S( -8, -55),   S( 16,  41),   S( -1, -20),   S( -5, -14),   S( 15,   7),   S(-19, -35),   S( -2, -29),   S(  3, -29),
            S(  9,  -7),   S(  7,   0),   S( 27,  26),   S(-11, -36),   S(  1,  -9),   S(  1, -21),   S(  0, -32),   S(  9, -27),
            S(-12, -51),   S( 13, -18),   S(-22, -31),   S(  0, -29),   S( 13, -10),   S( 10,   7),   S(  8,  -5),   S( -3,  12),
            S(-10, -33),   S( -5,  23),   S(  4,   1),   S(  0,   1),   S( 17, -18),   S( 15,  11),   S(-12, -49),   S( -8, -17),
            S(-25, -154),  S(-15, -61),   S(  4,  -7),   S( -3,  -5),   S( -4,   2),   S( -1, -13),   S(-10, -33),   S(  4,   4),

            /* rooks: bucket 14 */
            S( -7, -22),   S(-25, -52),   S( 10,  24),   S(-10, -27),   S( -6, -31),   S(  3,  16),   S(  6,   0),   S(-17, -64),
            S(-30, -50),   S(-10, -45),   S( -9,  18),   S(-24, -40),   S( -6,   9),   S(-10, -66),   S(  6,  15),   S( 11,  18),
            S(  0, -14),   S(  1,  -9),   S( -1,  -9),   S(-16,  -2),   S( -5, -20),   S( -3, -25),   S( 14,  39),   S(  8, -12),
            S( 18,  14),   S(-13, -45),   S(  5,   8),   S(-13, -12),   S( 12, -22),   S(  6,  16),   S(-15, -75),   S( -5, -26),
            S(  2,  -8),   S( -2, -30),   S(  1, -30),   S( -2, -28),   S(-11, -41),   S(-15, -16),   S( -4, -40),   S(  3, -23),
            S( 10, -10),   S(  0, -14),   S( -5, -24),   S( 12,  11),   S( -9, -29),   S(-13,  -4),   S( 16,  24),   S( -5,  13),
            S( 22,  16),   S(  3, -54),   S(  2, -37),   S( 11, -17),   S(  6, -48),   S( 10,  -5),   S( -2,   5),   S( 17,   4),
            S( -4, -40),   S(  1, -16),   S(-11, -35),   S(  3, -22),   S(-20, -44),   S( 12,   5),   S(  5,  10),   S(  7, -18),

            /* rooks: bucket 15 */
            S( -4, -50),   S(-11, -35),   S( -7, -38),   S(-12, -38),   S(  7,   5),   S(  5, -14),   S(-25, -43),   S(-10, -15),
            S(-13, -24),   S(-26, -39),   S( -3,  13),   S(-29, -59),   S( -9, -31),   S( 18,   0),   S(-15, -45),   S( 13,  23),
            S( -1,  -8),   S( -9, -36),   S(-11, -30),   S( -9,   2),   S( 30,  -7),   S(  9,  -2),   S( -1,  10),   S( -5, -24),
            S(  3, -29),   S( -7, -32),   S(-12, -18),   S(-11, -18),   S(-17, -47),   S( 13,  -8),   S( -4, -39),   S(-14,   2),
            S( -8, -14),   S( -3,   9),   S( 14, -12),   S( -9, -31),   S( -4, -14),   S(  7,  18),   S(  0, -11),   S(-12,  -5),
            S(  9,   9),   S( -2, -14),   S(  3,  -5),   S( -2, -17),   S( -6,  19),   S(  6,  -5),   S( 11, -10),   S(-17, -17),
            S(  1,  11),   S(  3,  -7),   S( 14, -17),   S(  2, -27),   S(  2, -24),   S( -2,  16),   S( -2,  -1),   S( -7,  15),
            S(  7, -25),   S( -4, -19),   S( -6, -24),   S( -1, -15),   S(  0, -11),   S(  1,  -5),   S(  5, -21),   S(  4, -12),

            /* queens: bucket 0 */
            S(-37, -18),   S(-47, -89),   S( 26, -82),   S( 48, -29),   S( 28, -21),   S( 34,  -9),   S( 56,  16),   S(-15,  47),
            S(-21,  -9),   S( 29, -71),   S( 37, -13),   S( 22,  17),   S( 21,  27),   S( 28,   4),   S( 23,  34),   S( 50,   4),
            S( 15,  16),   S( 32,  20),   S( 10,  59),   S( 14,  38),   S( 17,  38),   S( 16,  24),   S(  6,  19),   S( 43,  32),
            S(  6,   7),   S( 38,  38),   S(-13,  69),   S( 16,  51),   S( 11,  61),   S( 23,  12),   S(  7,  49),   S( 21,  53),
            S( 32,  29),   S( 29,  48),   S( 13,  40),   S( 15,  69),   S(-17,  27),   S( 23, -17),   S( 14,  32),   S( 53, -22),
            S(  4,  55),   S( 15,  64),   S(  4,  38),   S( 22,  -7),   S( 54,  -7),   S(-16,  11),   S( 30,  44),   S( 33, -28),
            S( 50,  13),   S( 36,  49),   S( 21,  12),   S( 60,  32),   S(-22,  13),   S( -4, -39),   S( 12,  37),   S( 48,  30),
            S( 47,  32),   S( -8,  53),   S( 51,  44),   S( 20,  54),   S( 51,  37),   S(-33,   1),   S( 70,  43),   S( 49,  50),

            /* queens: bucket 1 */
            S(-18,  -6),   S(-103,  -5),  S(-79, -20),   S(-14, -45),   S( 14, -86),   S(-11, -38),   S(  5, -46),   S( -7,   6),
            S(-23, -40),   S(  4, -66),   S( 11, -65),   S( -6,  29),   S( -5,  20),   S(  5,   2),   S( 20, -39),   S(  2,  18),
            S(-35,  39),   S(  2,   2),   S(  6,  10),   S(  7,  -6),   S( -6,  41),   S(-18,  55),   S( 19,  -4),   S(  7,  51),
            S(  9, -37),   S(  8, -16),   S(-20,  47),   S( 34,  23),   S( -9,  57),   S( 10,   2),   S(-18,  44),   S( 22,   0),
            S( 16,  34),   S( 33,  11),   S(-18,  73),   S( -3,  46),   S(-28,  46),   S( -8,  55),   S( -5,  22),   S( -7,  39),
            S(  9,  26),   S( 23,  46),   S( 40,  66),   S(-19,  56),   S(-42,  62),   S(-14,  35),   S( 17,  41),   S(  9,  47),
            S( -8,  34),   S(-14,  79),   S(-29,  43),   S(-43,  49),   S(-14,  30),   S(  5,   7),   S( -4,  23),   S(-31,  40),
            S( -3,  13),   S(  9,  35),   S( 46,  42),   S(-11,  22),   S( 10,  41),   S(-17,  -9),   S(-10,  26),   S(  9,  34),

            /* queens: bucket 2 */
            S( 19,  13),   S( 27, -56),   S( 13, -31),   S( -1, -25),   S(-40,  46),   S(-56,  -4),   S(-33, -33),   S( 36,  -5),
            S( 12,  -3),   S( 19,  24),   S( 15, -16),   S( 22,  -8),   S( 14, -29),   S(  7, -40),   S( -4,  28),   S( 51,  -7),
            S( 18,   3),   S( 14,   9),   S(  3,  46),   S( 12,  40),   S(  8,  59),   S( 18,  44),   S(  2,   9),   S( 30,  15),
            S(  6,   4),   S(-10,  68),   S( -8,  51),   S(  8,  51),   S(-27, 102),   S( -4,  86),   S( 14,  38),   S(  0,  77),
            S( 13,  15),   S(-10,  60),   S(-10,  61),   S(-32,  88),   S(-17,  80),   S(-12,  82),   S(-17, 107),   S( -8,  97),
            S( 12,  48),   S( -3,  60),   S(-32,  93),   S(-13,  45),   S(-32,  93),   S(-42, 121),   S(-20, 103),   S( 20,  65),
            S(-30,  54),   S(-42,  88),   S(-37,  63),   S( 32,  77),   S(-16,  75),   S(  9,  51),   S(-26,  51),   S(-21,  58),
            S(-61,  92),   S( -6,  33),   S( 44,  13),   S( 38,  25),   S( -6,  69),   S( 34,  26),   S(  9,  38),   S(  5,  17),

            /* queens: bucket 3 */
            S( 82, 107),   S( 56, 105),   S( 47, 103),   S( 38,  96),   S( 63,  36),   S( 42,   6),   S( 16,  12),   S( 34,  35),
            S( 58, 129),   S( 57, 107),   S( 41, 120),   S( 48,  95),   S( 45,  83),   S( 65,  52),   S( 62,   5),   S( 35,  45),
            S( 63,  77),   S( 47, 109),   S( 54,  83),   S( 54,  77),   S( 52,  92),   S( 52,  99),   S( 53,  97),   S( 60,  52),
            S( 38, 129),   S( 45, 101),   S( 50,  83),   S( 45,  94),   S( 43,  84),   S( 35, 126),   S( 52, 103),   S( 45, 136),
            S( 51, 100),   S( 54, 116),   S( 57,  87),   S( 44,  71),   S( 34, 115),   S( 31, 132),   S( 39, 161),   S( 41, 157),
            S( 52, 119),   S( 52, 118),   S( 45, 106),   S( 11, 120),   S( 33, 133),   S( 76,  94),   S( 64, 143),   S( 30, 201),
            S( 54, 113),   S( 46, 100),   S( 76,  82),   S( 42, 104),   S( 44, 115),   S( 84,  80),   S( 95, 127),   S(149,  64),
            S( 88,  93),   S( 94,  72),   S( 83,  75),   S( 78,  80),   S( 28, 113),   S(108,  56),   S(131,  57),   S(103,  74),

            /* queens: bucket 4 */
            S(-29, -21),   S(-32, -32),   S(-41, -41),   S(-17, -18),   S( 14,   2),   S( 56,  25),   S(-40, -36),   S(-49,  -8),
            S(-43, -24),   S(-31, -18),   S( -8, -17),   S(-35,  36),   S( 11,  -6),   S( 53, -18),   S(  4, -21),   S(-34, -22),
            S(  5,  14),   S( -8,  -8),   S( 12,  65),   S( 15,  26),   S(-10,  -9),   S( 33,  -1),   S( 10, -25),   S(  0,  -5),
            S(  2,  20),   S( -3,   7),   S(-24,  23),   S(-13,  29),   S(  5,  19),   S( 14,  21),   S( -3, -31),   S(-15, -22),
            S(  1,  14),   S( 27,  19),   S( 30,  23),   S( 57,  57),   S(  1,  42),   S(  3, -29),   S(-44, -35),   S(  5, -37),
            S(  5,  10),   S( 24,   3),   S( 31,  80),   S( 23,  31),   S(  8,   6),   S( -9, -13),   S(-11, -15),   S(-25, -11),
            S(-25, -27),   S(-49,  31),   S(  7,  26),   S( 54,  40),   S( 10,   5),   S( -3,  -7),   S( -8, -13),   S( -2,  -5),
            S(-20, -43),   S( -2, -11),   S( 41,  65),   S(-11,  27),   S(-26, -21),   S(-16, -26),   S(-27, -37),   S( -9, -16),

            /* queens: bucket 5 */
            S(-46,  -5),   S(-33, -40),   S(-18,   3),   S(-45, -35),   S(-50, -32),   S( 10, -17),   S(-33,  -2),   S( -7,  -5),
            S( -9,   6),   S( -9,  -9),   S(-49, -20),   S(-73,  -1),   S(-37,   3),   S(-38,  -7),   S(-45, -25),   S(-39,   0),
            S(-37,   0),   S(-49, -13),   S(-75, -20),   S(-32,  38),   S( 48,  78),   S(-16,   2),   S(  6,  13),   S( 18,  30),
            S(-61,   4),   S(-33,  27),   S(-32,  28),   S( -2,  68),   S( 22,  14),   S(-16,   2),   S(-21, -14),   S( -4,   8),
            S(-40, -14),   S(  8,  26),   S( -2,  52),   S( -5,  54),   S( 20,  47),   S(-32,   1),   S( 12,  10),   S(-20, -23),
            S(-31,  23),   S(  0,  39),   S( -7,  43),   S( 20,  52),   S( 31,  49),   S( 16,  20),   S(  0,   9),   S(-13,   2),
            S( 11,   4),   S(  3,  28),   S( 28,  83),   S( 10,  37),   S( 25,  47),   S( 17,  46),   S( 21,   5),   S(-42, -37),
            S( -9,  27),   S( 10,  11),   S( 21,  62),   S( 10,  48),   S( 12,  26),   S(  7,  29),   S(  1, -30),   S(-28, -35),

            /* queens: bucket 6 */
            S(-34,  -1),   S(-31,   6),   S(-47, -29),   S(-101, -51),  S(-78, -54),   S(-61, -33),   S( -1, -10),   S( -7,  17),
            S(-81, -22),   S(-54,   4),   S(-33,  14),   S(-43,   1),   S(-71,  20),   S(-96, -21),   S(-84, -12),   S(-30,  26),
            S(-20,  32),   S(-17,  10),   S(-58,  40),   S(-90,  89),   S(-11,  56),   S(-45,   8),   S(-28,  11),   S(  3,  -7),
            S(-49,   8),   S(-35,  21),   S(-33,  57),   S(-56,  86),   S(-12,  55),   S( 11,  54),   S(  8,  14),   S(-14, -10),
            S(-55,  30),   S(-10,  31),   S(-19,  54),   S( 16,  30),   S( 27,  79),   S( 47,  23),   S(  4,   4),   S(-11,  24),
            S(-28,  52),   S( 10,  -1),   S( 17,  19),   S( 37,  41),   S( 13,  45),   S( 70,  81),   S( 16, -18),   S(-11,  25),
            S( 24,  36),   S(-13, -13),   S(-14,  39),   S(-11,  31),   S( 26,  75),   S( 43,  88),   S(-21,  22),   S(-25,  11),
            S(-13,   9),   S( 21,  38),   S( -2,  21),   S(  7,  26),   S( 31,  41),   S( 23,  36),   S(-28,  -6),   S(  0,   1),

            /* queens: bucket 7 */
            S(-11,  18),   S(-32,  14),   S(-83,  43),   S(-28, -14),   S(-34, -23),   S(-34, -21),   S(-48, -29),   S(-25, -26),
            S(-25,  -3),   S(-76,  22),   S(-21,  43),   S(-17,  35),   S(-30,  26),   S(-38,  48),   S(-48,  32),   S(-70, -16),
            S(-37, -23),   S(-28,  30),   S( -1,  13),   S(  1,  32),   S(  7,  23),   S( 12,  13),   S(  8,  16),   S(-28, -10),
            S(-74,  22),   S( -5,  11),   S(-25,  22),   S(  4,  28),   S( 45,  17),   S( 43,  20),   S( 20,  39),   S( -1,   3),
            S(-23,  29),   S(-64,  20),   S( 11,  32),   S( 25,   6),   S( 75,  -9),   S( 83, -17),   S( 38,  24),   S( 39, -21),
            S(  0,  48),   S(  8,  29),   S(  0,   2),   S( 10,  -1),   S( 56,  25),   S( 83,  28),   S( 80, -18),   S( 50,  -2),
            S( -3,  -5),   S( 15,  14),   S( 31, -14),   S(  7,  17),   S( 50,  11),   S( 54,  32),   S( 26,  18),   S( 41,  23),
            S(  6,   4),   S( 22, -12),   S( 38,  -3),   S( 19,  27),   S( 41,  20),   S( 38,  25),   S( 33,  -4),   S( 36,  63),

            /* queens: bucket 8 */
            S( -5, -14),   S( -2,   2),   S(-18,  -5),   S( -7, -11),   S( -7,  -1),   S(-17, -41),   S(-26, -39),   S(  1,   1),
            S(  6,  16),   S( -6,   0),   S( -6,   1),   S(-16,  -8),   S(  4,   6),   S(-25, -16),   S(-16, -27),   S( -2,   3),
            S( -1,  -2),   S(-20, -15),   S(  2,   2),   S(  5,  -4),   S(-24,  -7),   S(  1,  -2),   S(-12, -34),   S(-16, -28),
            S(  1,   9),   S(  6,  17),   S( 19,  27),   S( 22,  11),   S( -2,  -5),   S(-10, -15),   S(  7,  11),   S( -2, -21),
            S( 14,  25),   S(  9,  55),   S( 17,  35),   S( 31,  38),   S( 25,  42),   S( -3, -10),   S( -4,  -6),   S(-20, -27),
            S( 10,  15),   S( 12,  22),   S(-26,   8),   S(  9,  32),   S( -8, -13),   S(  0,   1),   S(  1,   1),   S( -3,   5),
            S(-21, -18),   S(-21, -30),   S( 23,  43),   S(  7, -11),   S( 14,  28),   S( -3,   2),   S( -8, -19),   S( -7, -22),
            S(-16, -39),   S( 12,   6),   S( -6, -35),   S( -2,  -5),   S(-28, -55),   S( -4, -12),   S(  9,   4),   S( -4,  -4),

            /* queens: bucket 9 */
            S( 10,  15),   S(-14, -23),   S(  5,   6),   S(-26, -30),   S(-19, -26),   S(-20, -34),   S(-21, -39),   S(-16, -39),
            S(  0,   3),   S(-12,  -9),   S( -4,  -6),   S(-22, -20),   S( -8,  -1),   S( -3,  -2),   S(  8,   5),   S( -5,  -9),
            S(  7,  15),   S( -2,   8),   S(-10,  13),   S(  0,  -2),   S(-19, -26),   S(  3,   6),   S(  8,   7),   S( -3,  -4),
            S( -1,  -2),   S( -2,  12),   S( 27,  65),   S( 10,  37),   S( 27,  33),   S( -5,   1),   S(-12, -28),   S(-11, -12),
            S( -1,  11),   S(  0,  16),   S(  6,  31),   S(  5,  28),   S(  7,  22),   S( 31,  35),   S( 15,  31),   S(-12, -13),
            S(-16, -17),   S(-16,  -4),   S(  7,  35),   S( 17,  38),   S( -7,  -8),   S( 14,  41),   S(-10,  -6),   S( -8, -13),
            S(-10, -11),   S( -6, -13),   S( -4,  18),   S( -7,   0),   S(  4,  -5),   S( -8, -14),   S( 12,  14),   S(-12, -23),
            S(-20, -17),   S(-14, -40),   S(  5, -16),   S( -9,  -8),   S( 23,  22),   S( -9,  -6),   S( 12,   2),   S(  3,  -7),

            /* queens: bucket 10 */
            S( -4,   0),   S(  0,  -3),   S( -6,  -3),   S(-20, -22),   S(-18, -17),   S( -8,   2),   S(  9,   4),   S(  2,   3),
            S(-11, -27),   S(  8,   3),   S(  3,  -7),   S(-15, -18),   S( -2,  -3),   S(-18,  -7),   S(  8,  -4),   S(-12, -22),
            S(  2, -18),   S(-22, -34),   S(-24, -16),   S( 13,   4),   S(  3,  15),   S(  6,  25),   S(-21, -36),   S( -8, -10),
            S(  7,   2),   S(  2, -16),   S( 14,  17),   S(  6,  25),   S( 16,  17),   S( 18,  39),   S( -9, -19),   S( -3, -21),
            S(  1,  14),   S(  1, -16),   S( -3,  10),   S( 23,  50),   S(  4,   1),   S(  3,  13),   S(  5,   1),   S(  2,   1),
            S( -1,  -2),   S( -4,  -9),   S(  0, -11),   S(  6,  20),   S( 30,  39),   S( 15,  22),   S( -5, -17),   S( -9, -25),
            S( -2,  -3),   S(-16, -33),   S(  2,   8),   S( -6, -17),   S(  1,   2),   S( 17,  28),   S( -9, -22),   S(  1,  -1),
            S(  8,   1),   S( -6, -20),   S(-12, -33),   S( 17,  -1),   S(  9,  20),   S(  0,  -2),   S(  5,   9),   S( 13,  -2),

            /* queens: bucket 11 */
            S(-26, -44),   S(  7, -10),   S( -2,  -6),   S( -5, -13),   S(  0,  -9),   S(-21, -21),   S(-19, -28),   S( -9, -17),
            S(-18, -17),   S(-16,  -6),   S(-47, -33),   S(-18, -12),   S( -8,   3),   S( -7,   9),   S(  8,  -7),   S(-16, -17),
            S( -9, -11),   S( -9, -27),   S(  3, -20),   S(  0,  -7),   S(-13, -12),   S( 14,  12),   S( 18,  27),   S(-11,  -8),
            S(-14, -27),   S(-30, -41),   S( 12,   7),   S( 32,  52),   S(  4,  -1),   S(-13,  25),   S( 22,  32),   S(  0,   7),
            S( -6,   2),   S( -8, -31),   S(-10,   5),   S( 29,  24),   S( 28,   4),   S( 26,  51),   S( -3,  26),   S( -8,   3),
            S( -3, -16),   S( 10,  15),   S(-23, -29),   S( 11,   4),   S( 39,   3),   S( 47,  60),   S(  9,   0),   S(-21, -20),
            S(-13, -11),   S( -9, -16),   S(-10,  -5),   S( -8,  -5),   S(  5,  18),   S( 25,  10),   S( 48,  40),   S(  2, -12),
            S(-20, -23),   S(  1, -20),   S(-15, -39),   S(  8, -16),   S(  2,   2),   S(-18,  -9),   S( 37,  18),   S( -6, -30),

            /* queens: bucket 12 */
            S(  7,  10),   S(  4,   1),   S( -2,  -6),   S( -3,  -1),   S(-14, -19),   S( -1,  -4),   S( -2,  -7),   S(  1,  -3),
            S( -2,  -5),   S( -6, -12),   S( -7,  -4),   S(  1,   9),   S(  1,   1),   S( -8,  -9),   S(-11, -24),   S( -2,  -5),
            S( -5,  -7),   S( -9, -19),   S( 24,  36),   S( -3,  -5),   S(-16, -26),   S( -5,  -9),   S(-14, -23),   S(-13,  -9),
            S( -2,   0),   S( -7,  -4),   S( 10,  19),   S( 10,  14),   S( 17,  31),   S( -7, -10),   S( 11,  12),   S(  0,  -6),
            S( -6, -11),   S( 13,  30),   S( 23,  37),   S(  2,   8),   S(  0,  21),   S(  9,  15),   S(-12, -29),   S( -1, -12),
            S( 10,  25),   S( -8, -10),   S( 26,   2),   S(  4,  -5),   S( -5,   8),   S( -1,   5),   S(  3,  -6),   S(-11, -25),
            S(  8,   7),   S( -4, -21),   S( 11,   2),   S(  7,  -4),   S(  8,  15),   S(  1,   3),   S( 12,  10),   S(-15, -21),
            S( -8, -27),   S( -2, -13),   S(-17, -28),   S( -4, -17),   S(  4, -12),   S( -1, -12),   S( -6,  -5),   S( -4, -11),

            /* queens: bucket 13 */
            S( -1,  -7),   S( -1,  -5),   S(  3,   9),   S( -3,  -9),   S( -2, -15),   S(  1,  -1),   S( -8, -11),   S( -7,  -9),
            S(  2,   7),   S(  4,  17),   S( 12,  20),   S( -4, -10),   S( -6,  -9),   S(  3,  14),   S( -4,  -6),   S( -5, -13),
            S( -2,  -2),   S(  3,   4),   S(  1,   8),   S( -1,  10),   S(  2,   5),   S(-11, -13),   S(  0,  -4),   S(-11, -21),
            S( -2,  -3),   S(  0,  10),   S(  4,   5),   S( 26,  42),   S( 18,  29),   S(-12,  -4),   S(-16, -34),   S(-12, -21),
            S(-10, -11),   S( -1,   9),   S( 12,  40),   S( 27,  66),   S( 14,  34),   S(-12, -27),   S( -4,  -3),   S( -4, -13),
            S( 10,  12),   S( 11,  34),   S( 36,  81),   S( 28,  54),   S( -2,   9),   S(  5,  12),   S(  4,   5),   S(-11, -17),
            S( -8,  -8),   S( 26,  40),   S( 10,  26),   S( 12,  15),   S( 10,  33),   S(  3,  -8),   S(  3,  -3),   S(  4,   8),
            S( -6, -12),   S(  0, -10),   S(  2,  -2),   S( -4,  -3),   S( -3,  -4),   S(  8,  19),   S(-12,  -1),   S(  0,   0),

            /* queens: bucket 14 */
            S(  3,   6),   S( -3, -11),   S(  0,  -7),   S( -1,  -5),   S( 11,  18),   S(  2,   7),   S( -6, -10),   S(-13, -26),
            S( -3,  -9),   S(  9,  25),   S(  1,  -2),   S(  5,   7),   S( -5,  -3),   S( -8, -21),   S( -2,  -2),   S(  1,   7),
            S( -4,  -5),   S(-17, -23),   S( -7, -19),   S( -7, -10),   S(  9,  12),   S( -5, -24),   S(  6,   9),   S( -8, -19),
            S( -5,   4),   S( 11,  13),   S(-13,  -9),   S( 23,  33),   S(  5,  -5),   S( -1,   9),   S( 18,  42),   S( 10,  12),
            S(  3,  18),   S( -3,  -2),   S(-27, -25),   S( 23,  34),   S( 18,  32),   S( 23,  34),   S(  2,  -1),   S(  0,  -6),
            S( -5, -13),   S(  3,  17),   S(  9,  16),   S( 16,  26),   S(  7,  41),   S( 22,  50),   S( 13,  32),   S( -8, -16),
            S(  3,  11),   S(  9,  14),   S( 12,  44),   S( 30,  44),   S(  6,  19),   S(  3,   7),   S( 13,  24),   S( 11,  20),
            S(  2,  11),   S( -2,  -7),   S(-17, -32),   S(  3, -12),   S(  6,  17),   S(  6,   2),   S( -8, -10),   S(-16, -27),

            /* queens: bucket 15 */
            S(  2,  -6),   S(  9,  10),   S( -8, -13),   S(  1,  -1),   S( -4,  -6),   S(-13, -23),   S( -5,  -1),   S(  3, -14),
            S(  7,  -2),   S(  0,  -4),   S(-12, -31),   S( -3,  -5),   S(  3,  20),   S( -8, -22),   S( 11,  13),   S(  5,   6),
            S( -1,  -5),   S(  2, -10),   S( -2,   1),   S( -1,  -5),   S( -3,  -8),   S( 10,  36),   S(  0,   0),   S(  1,  -5),
            S( -1,  -8),   S(  1,   1),   S( -8,  -9),   S(  5,   3),   S(  4,   7),   S( -2,  10),   S( 21,  28),   S( -1,   0),
            S( -2,  -2),   S( -3,  -5),   S( -4,  -8),   S(  2,  10),   S( 11,  28),   S( -9, -24),   S(  1,   5),   S(  0,  -8),
            S( -6, -11),   S( -2,  -3),   S(  4,   2),   S( -5,  -4),   S( -2, -15),   S( 13,   9),   S( 14,  23),   S(  0, -22),
            S( -3,  -8),   S(  3,  -9),   S( -1,  -5),   S( 16,  23),   S( 17,  20),   S( 30,  53),   S(  4,   4),   S( -1,  -1),
            S( -2,   3),   S( -3,  -4),   S(  0,  -7),   S(  4,   2),   S( 13,   6),   S(  7,   8),   S(  3,  -1),   S( -9, -35),

            /* kings: bucket 0 */
            S( 72,  -3),   S( 67,  44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 53,  32),   S(126,  57),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 51,  16),   S( -6,  34),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 77,  46),   S( 61,  58),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-18,  41),   S(-10,  30),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 45,  66),   S( 51,  52),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -1,  51),   S(-17,  15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 21,  85),   S(-34,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11, -63),   S( 58, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -19),   S( -6,  27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 40, -16),   S( 22,  -6),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 10,  29),   S( -4,  24),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 51,  18),   S( 15,  18),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 10,  52),   S( 12,  46),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 50,  30),   S( 13, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 21,  70),   S(-14,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57, -130),  S( -4, -59),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -103),  S(-105, -12),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 22, -46),   S(-21, -36),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-37, -21),   S(-31,  -4),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-16, -34),   S( -1, -40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-13, -23),   S(-98,  21),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-25, -44),   S(-49, -105),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-88, -13),   S(  5, -99),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -126),  S(-64, -40),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -241),  S(-31, -106),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-82, -32),   S(  8, -52),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-70, -92),   S( -7, -104),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-11, -64),   S(-118, -20),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 17, -117),  S(-76, -67),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-116, -10),  S(-32, -117),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-74, -68),   S(-16, -217),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -25),   S(-19,   2),   S(  5,  -5),   S( 10,  38),   S( 30,   6),   S( 47,   9),   S( 49, -17),   S( 58,  -8),
            S(-10, -29),   S(-30,   2),   S( -5,  -6),   S(  6, -10),   S( 14,  10),   S( 12,   5),   S( 24,  -1),   S( 29,  21),
            S( 11, -25),   S(  8, -24),   S( 36, -44),   S( 18, -15),   S( 22,  -7),   S( 14,  26),   S(  7,  38),   S( 35,  15),
            S( 13, -17),   S( 34,   3),   S( 64, -31),   S( 44,  -1),   S( 18,  41),   S(-23,  84),   S(  9,  85),   S( 53,  69),
            S( 81, -42),   S(128, -10),   S(104, -18),   S( 51,   7),   S( 57, 129),   S(  4, 127),   S( 38, 151),   S( 78, 127),
            S(-215, -64),  S(-123, -130), S( 10, -162),  S( 41,  18),   S(103, 214),   S( 71, 167),   S( 84, 153),   S( 62, 182),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  24),   S(-47,  27),   S(-12,   4),   S(-52,  78),   S( -9,   0),   S( 20,   7),   S( 17,  -3),   S( 15,  20),
            S(-53,  20),   S(-45,  20),   S(-22,   4),   S(-16,   8),   S(  6,   8),   S( -6,   5),   S( -6,  -3),   S(-15,  18),
            S(-50,  32),   S(-15,  18),   S(-16,   3),   S( 14,  -8),   S(  7,  16),   S(-14,  18),   S(-24,  29),   S(-17,  26),
            S(-38,  48),   S( 22,  18),   S(-13,  22),   S( 17,  31),   S(  8,  26),   S(-30,  46),   S( -5,  37),   S( 25,  62),
            S( 17,  33),   S( 67,   0),   S(102, -17),   S( 92, -19),   S( 51,  14),   S(  0,  40),   S(-40,  80),   S( 30, 102),
            S( 24,  65),   S(-11, -31),   S(  1, -105),  S(  7, -87),   S(-41, -58),   S(-12,  53),   S( -1, 212),   S(102, 204),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,  44),   S(-35,  23),   S(-22,  11),   S(-14,  29),   S(-26,  32),   S(-16,  14),   S(  1, -13),   S( -6,  21),
            S(-53,  34),   S(-43,  26),   S(-30,   5),   S(-27,  27),   S(-25,  21),   S(-33,   7),   S(-19, -10),   S(-38,  16),
            S(-42,  52),   S(-40,  57),   S(-16,  20),   S(-12,  21),   S(-22,  25),   S(-27,  10),   S(-23,   7),   S(-32,  13),
            S(-30,  90),   S(-38,  78),   S(-15,  48),   S(  3,  41),   S( -2,  33),   S(-23,  19),   S(  4,  14),   S( 19,  17),
            S( -9, 123),   S(-47, 131),   S(  5,  14),   S( 17, -24),   S(100,  -1),   S( 87,  -3),   S( 55,  -4),   S( 42,   8),
            S( -5, 253),   S( 57, 182),   S(-14,  85),   S( 47, -99),   S(  5, -192),  S(-93, -153),  S(  2, -99),   S( 31,  -1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  3,  15),   S( -6,  18),   S(  8,  10),   S( -3,  48),   S( -3,  45),   S( 30,  23),   S( 20,  -5),   S( 10, -14),
            S( -5,  22),   S( -5,  26),   S( -1,   6),   S( -3,  17),   S( 10,  22),   S( 20,  -4),   S(  5,  -9),   S(-20,  -5),
            S(  7,  37),   S( -8,  60),   S(  6,  24),   S(  6,   8),   S( 23,  -9),   S( 16, -11),   S(  9, -21),   S(-16, -13),
            S(  6,  88),   S(-15, 103),   S(  4,  72),   S( 21,  34),   S( 34,  -1),   S( 32, -26),   S( 21,   2),   S( 31, -15),
            S(  5, 159),   S(-12, 171),   S(-26, 163),   S(-17, 120),   S( 52,  46),   S( 86,  -8),   S(115, -29),   S( 88, -28),
            S(107, 130),   S( 32, 254),   S( 22, 267),   S( -4, 229),   S(-37, 101),   S( 15, -189),  S(-70, -247),  S(-153, -176),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 76, -11),   S( 32,   4),   S(-32,  12),   S( 18,  -6),   S( 22, -10),   S(  0, -12),   S(  8, -15),   S(-75,  52),
            S( 46,   2),   S(  8,  22),   S(-21,  15),   S(-36,   5),   S(-44, -11),   S(-20, -10),   S(-26, -24),   S(-80,  22),
            S( 79, -16),   S(115, -27),   S( 20,  -9),   S(-24,   5),   S(-70,   8),   S( -5,   1),   S(-76,  28),   S(-51,  27),
            S(-91, -70),   S(-42, -88),   S( 46, -46),   S(-36,  10),   S(-36,  26),   S(-75,  74),   S(-26,  49),   S(-79,  89),
            S(  1, -83),   S(-71, -115),  S(-57, -77),   S( 49,  22),   S( 90,  86),   S( -3, 103),   S( 53,  57),   S(  2,  97),
            S( -2, -55),   S(-20, -77),   S(-12, -68),   S( 10,  27),   S( 51,  84),   S( 45, 164),   S( 47, 153),   S( 54, 113),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43,  42),   S(-63,  56),   S( -9,  17),   S( 59,   4),   S( 66,  -4),   S( 16,   0),   S(-36,  11),   S(-53,  44),
            S(-70,  38),   S(-47,  45),   S(-28,  24),   S( -3,  30),   S(  1,  20),   S(-22,   4),   S(-77,  12),   S(-82,  36),
            S(-36,  35),   S(-29,  59),   S( 24,  32),   S(  8,  40),   S(-35,  52),   S(-67,  32),   S(-75,  37),   S(-75,  48),
            S(-16,  40),   S(  0,   5),   S(-50, -30),   S( -5, -15),   S(-14,  -5),   S(-52,  34),   S(-20,  30),   S(-23,  52),
            S( 67,  -3),   S( 25, -46),   S( 47, -93),   S(  3, -81),   S( 68, -45),   S(  8,  23),   S( 20,  51),   S(-59, 126),
            S( 18,  39),   S( 13,  12),   S(-47, -49),   S(-25, -44),   S(-17, -42),   S( 50,  60),   S( 88, 131),   S( 74, 159),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71,  34),   S(-66,  21),   S(-29,   4),   S( -1,   6),   S(  5,  29),   S( 13,  14),   S( 12,   6),   S( -3,  28),
            S(-76,  27),   S(-73,  22),   S(-52,  13),   S( 13,  20),   S(-24,  36),   S(-11,  10),   S(-22,  12),   S(-13,  15),
            S(-62,  40),   S(-79,  46),   S(-53,  38),   S(-45,  51),   S(  2,  44),   S(  8,  22),   S(  8,  18),   S(-15,  18),
            S(-101,  99),  S(-59,  60),   S(-34,  40),   S(-20,  27),   S(-25, -25),   S(-12, -29),   S(-21,  11),   S( 35,   0),
            S(-12, 106),   S(-58,  84),   S(-17,  36),   S( 12, -44),   S( 12, -61),   S(-21, -68),   S( -8, -19),   S( 80,  -8),
            S(105,  61),   S( 77,  87),   S( 43,  16),   S( 35, -67),   S( 20, -94),   S(-38, -54),   S(-18, -47),   S( 52,   5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50,   3),   S(-42, -16),   S( -7, -28),   S(-40,  38),   S( 17,  -2),   S( 62, -17),   S( 65, -32),   S( 64,  -7),
            S(-61,  11),   S(-63,   2),   S(-45, -19),   S(-40,  15),   S(  2,  -2),   S( 40, -28),   S( 38, -17),   S( 48, -14),
            S(-50,  24),   S(-78,  40),   S(-25,  -1),   S(-38,  -4),   S( -9,   4),   S( 23, -14),   S( 75, -18),   S( 59, -21),
            S(-41,  64),   S(-87,  80),   S(-53,  60),   S(-33,  37),   S(  2,  -8),   S( 54, -76),   S( -9, -61),   S( 18, -104),
            S(  3,  68),   S(-39, 130),   S(  7, 123),   S( -4,  78),   S( -6,  36),   S( 15, -81),   S(-44, -123),  S( 16, -111),
            S(145,  73),   S( 78, 130),   S(121,  74),   S( 43,  98),   S( 21,  17),   S( 25, -138),  S(-10, -102),  S(-14, -169),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -5,  11),   S(  1,  23),   S( 55,  -2),   S(-23, -31),   S(-16, -68),   S(-19, -22),   S(  4, -50),   S( 36, -42),
            S(  6, -64),   S( 22,  -2),   S(-35, -49),   S(-17, -39),   S(-63, -45),   S( 45, -62),   S( 41, -81),   S(  1, -54),
            S( 49, -92),   S( 15, -68),   S(-13, -68),   S(-56, -58),   S(  0, -30),   S(-12, -26),   S(-53, -17),   S(  7, -43),
            S( 21,  -8),   S(-24, -39),   S( 15, -15),   S(  6,  15),   S(-16,  12),   S( 12,  -1),   S(-16,  25),   S(-19,  33),
            S( 18,   2),   S(  0, -24),   S( 13,  29),   S( 27,  99),   S( 37, 129),   S( 21, 121),   S( 30, 107),   S(-20, 109),
            S( 27,  49),   S(  0,  47),   S( 18,  70),   S( 19, 117),   S( 36,  76),   S( 56, 202),   S( 30, 105),   S(-18,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 21,  12),   S( 30,  23),   S( 49,   3),   S(  8,  22),   S(  8,   2),   S( 13, -10),   S( 11, -48),   S(-32, -12),
            S( 65, -53),   S( 24, -40),   S( 11, -62),   S( -1, -29),   S(-37, -22),   S(-56, -20),   S(-23, -42),   S( 30, -48),
            S(-20, -44),   S(-45, -47),   S(  8, -82),   S(-74, -45),   S( 10, -43),   S( -6, -42),   S(-70, -32),   S( 20, -26),
            S(-41,   0),   S(-31, -69),   S( 22, -72),   S(-29, -39),   S(-23, -26),   S(-27, -41),   S( 15, -13),   S(  7,  19),
            S( 26,  19),   S(-12, -16),   S(-20,  -8),   S( 30,  12),   S( 18,  83),   S( 18,  60),   S(  9,  72),   S( -9,  54),
            S(-30,  65),   S( 12,  49),   S( 11,  76),   S( 20,  48),   S(  7,  98),   S( 26,  99),   S( 15, 117),   S( 19,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -45),   S( -6, -49),   S(  2, -22),   S( -1, -10),   S( 45,  -6),   S( 62,  13),   S( 36,  -1),   S( 10, -12),
            S(-20, -42),   S(-69, -38),   S( -6, -52),   S( 36, -43),   S( 15, -15),   S(  4, -30),   S( 13, -37),   S( 44, -45),
            S( -3, -50),   S(-84, -24),   S(-46, -41),   S(-31, -29),   S(-16, -39),   S(-28, -59),   S(-10, -66),   S( 44, -59),
            S(-24,   0),   S(  8,  -8),   S(-40, -26),   S(-85, -22),   S(-42, -55),   S(-39, -53),   S(-23, -46),   S( 11, -46),
            S( 17,  19),   S( 37,  14),   S(  8,   0),   S(-22,  -1),   S( 14,  11),   S( 10,  24),   S(-39,  25),   S( 25,  15),
            S( -6,  23),   S( 11,  41),   S( 22,  60),   S( -4,  65),   S( 29,  76),   S(  3,  36),   S(-11,   5),   S( 15,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-51, -47),   S(  7, -53),   S(-10, -48),   S(  2, -11),   S(-12, -17),   S( 77,  16),   S( 51, -11),   S( 39, -12),
            S( -1, -74),   S(-53, -71),   S(-26, -76),   S( 28, -65),   S(-19, -34),   S( -4, -41),   S( 37, -55),   S( 78, -83),
            S(-36, -26),   S(-89,   1),   S(-43, -23),   S(-40,  -4),   S(-76, -45),   S( 25, -60),   S( 12, -115),  S(121, -118),
            S(-49,  25),   S(-76,  24),   S(  0,  22),   S( -1,  16),   S(-50,  -6),   S(-27, -38),   S(-31, -51),   S( 24, -98),
            S(-19,  23),   S(-20,  78),   S(-15, 100),   S( 22,  76),   S( 41,  67),   S(-15, -10),   S( -1,  29),   S( -1, -39),
            S( 12,  78),   S( 29,  44),   S( 34,  95),   S( 36,  71),   S(  5,  57),   S( 39,  90),   S( 12,  41),   S(  5,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-21, -132),  S( 32, -87),   S( -5,  -9),   S( -3,  -7),   S(  9, -16),   S(-45, -80),   S( 16, -45),   S(  5, -45),
            S( 40, -94),   S( 49, -48),   S(-24, -47),   S(-27, -62),   S(-35, -97),   S(-29, -71),   S(-10, -93),   S(-26, -83),
            S( -1, -60),   S(-16, -91),   S(-26, -127),  S(-32, -106),  S(  4, -38),   S(-17, -33),   S(-59, -65),   S( -8, -84),
            S(-20, -69),   S( 16,  18),   S(-20, -10),   S( -7, -19),   S( 11,  65),   S(  5,  50),   S( 15,  -8),   S( -4, -12),
            S(  4,  -1),   S( 10,  39),   S( 16,  52),   S( 31,  73),   S( 38, 108),   S( 29,  66),   S( 15,  91),   S( 16,  54),
            S( 13,  51),   S( -9,  26),   S( 10,  63),   S(  8,  42),   S( 25, 108),   S( 30, 106),   S(-20, -59),   S( -7,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -73),   S( 25, -78),   S( 25,  17),   S( -3, -10),   S( 10, -11),   S(-54, -32),   S(  5, -80),   S(-16, -62),
            S( 50, -136),  S( 16, -118),  S(  0, -72),   S( 24, -17),   S(-35, -69),   S( 16, -88),   S(-11, -96),   S(  2, -74),
            S( 10, -102),  S(-36, -54),   S(  2, -97),   S( 16, -48),   S(-37, -25),   S( 17, -78),   S(-14, -62),   S( 71, -97),
            S(  2, -42),   S( -1, -11),   S(  0, -37),   S( -3,  26),   S( 12,  -5),   S(-18,   9),   S( -1, -27),   S( 17, -38),
            S(-16,  48),   S(  5,   5),   S( -3,   2),   S( 16,  35),   S( 43,  78),   S( 23,  73),   S( 15,  96),   S( -1,  45),
            S( 25,  78),   S( 36,  49),   S( -7,  23),   S(  8,  31),   S( 16,  63),   S( 10,  59),   S(  3,  80),   S( -1,  74),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5, -101),  S(  9, -73),   S(-17, -33),   S(  9,   3),   S( -4, -23),   S(  8,   7),   S( 29, -63),   S( -6, -61),
            S( 18, -121),  S(-45, -114),  S(-16, -107),  S(-32, -101),  S( -8, -36),   S(  2, -65),   S( -7, -58),   S( 10, -102),
            S( -3, -93),   S(-17, -85),   S(-17, -64),   S( -2, -65),   S(-22, -70),   S( 10, -90),   S(-18, -101),  S( 46, -62),
            S( -1, -50),   S( -3, -47),   S(  2, -17),   S(-31,   2),   S( 22, -18),   S( -7,  -8),   S( 12, -42),   S(  4,  -6),
            S( -9,  -8),   S( 15,   9),   S( -7,  25),   S(  3,  42),   S( 19,  64),   S( -3,  20),   S(  9,  -1),   S( 20,  66),
            S(-13,  44),   S(  6,  55),   S( 28,  73),   S( 18,  61),   S( 25,  82),   S(  6,  49),   S( 26,  87),   S( 14,  79),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-14, -41),   S(  9, -51),   S(-29, -48),   S(-11, -35),   S(-19, -52),   S(-26, -39),   S(  3, -76),   S( 23, -77),
            S(-14, -56),   S(  2, -118),  S(-13, -106),  S(-17, -44),   S( -7, -45),   S(-11, -36),   S( 10, -53),   S(  3, -105),
            S(-38, -45),   S(-32, -74),   S(-40, -48),   S( 14, -31),   S(-47, -26),   S(  1, -76),   S(-19, -83),   S( 11, -68),
            S(  3, -43),   S(-39, -23),   S( -5,  73),   S(-20,   5),   S( 23,  12),   S(-14, -17),   S(  0,   6),   S(  1,  21),
            S( -4,  26),   S( -5,  15),   S(-10,  60),   S( 22,  65),   S( 23,  85),   S( 11,  56),   S( 11,  43),   S(  6,  38),
            S(-25, -22),   S(-14, -41),   S( 14,  78),   S( 28,  57),   S( 26,  74),   S( 20,  42),   S( 20,  66),   S( 17,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-71, -46),   S(-17, -22),   S(-24,  -8),   S(  3,  15),   S(-32,  18),   S(-36,  11),   S(  3, -11),   S(-71, -39),
            S( 10, -25),   S( 19, -30),   S(-34, -27),   S(  0,  -8),   S(-16,  10),   S(-13,  18),   S(-41, -36),   S(-31, -57),
            S(  2, -75),   S( 11, -16),   S(  4,  10),   S( 29,  29),   S( -9,  20),   S(  9,   0),   S(-34,  20),   S(-27, -20),
            S(  7,  -4),   S( 43,  45),   S( 41,  22),   S( 47,  28),   S( 24,  26),   S(  8,  30),   S( 21,   0),   S(-23,  14),
            S( 48,  49),   S( 15,  68),   S( 46,  90),   S( 65,  45),   S( 63,  41),   S( 24,  29),   S( 14,  17),   S( -6,  10),
            S( 87, -78),   S( 10,  59),   S(137,  -1),   S(100,  31),   S( 36,  50),   S(-37,  70),   S( 18, -16),   S(-49,   7),
            S( 27,  -8),   S(-29, -21),   S( 44,  21),   S( 68,  67),   S( 41,  19),   S( 29,  17),   S(-78,  23),   S(-11,  12),
            S(-97, -118),  S( -2,   1),   S(-21, -24),   S( 40,  -7),   S( 36,  34),   S( 18,  16),   S(-45, -27),   S( -4,  37),

            /* knights: bucket 1 */
            S(-21,   0),   S(-66,  25),   S(-16,  11),   S(-49,  29),   S(-29,  51),   S(-10, -32),   S(-48,  -4),   S( -8, -28),
            S(-20,  15),   S(-71,  70),   S(-37,  44),   S( -6,  31),   S(-17,  21),   S(-17,  42),   S(-19,  -3),   S(-28, -60),
            S(-41,  13),   S(  4,  -6),   S(-18,  20),   S( -5,  48),   S( -9,  43),   S( -7,  16),   S(-37,  33),   S(-18,  25),
            S(-13,  56),   S( 32,  47),   S( -1,  56),   S( -5,  67),   S( -1,  51),   S(-23,  81),   S(-12,  30),   S(-23,  34),
            S( 62,   2),   S(  6,  22),   S( 46,  64),   S( 20,  61),   S( 42,  52),   S(  1,  77),   S( -1,  38),   S(-13,  79),
            S( 50,  15),   S( 77,  -7),   S( 55,  30),   S( 92,  28),   S( 95,  12),   S(-41,  80),   S( 11,  25),   S(-10,  21),
            S( 23, -23),   S( 44, -17),   S( 43, -18),   S( 46,  34),   S( 16,  27),   S(-11,  28),   S(-15,  80),   S(-31,  35),
            S(-190, -82),  S(  5, -41),   S(-65, -93),   S(-16,   2),   S(-27,  -7),   S( 29,  37),   S(  8,  37),   S(-86,  27),

            /* knights: bucket 2 */
            S(-61, -30),   S(-39,  20),   S(-38,   0),   S(-18,  15),   S(-14,  15),   S(-57,   4),   S(-30,  15),   S(-18, -30),
            S(-34,  19),   S(-11,  28),   S(-20,  16),   S( -4,  14),   S(-21,  19),   S(-17,  11),   S( -1,  -2),   S(-34,  12),
            S(-37,  40),   S(-18,  24),   S(-19,  21),   S(-22,  54),   S(-15,  48),   S(-19,  14),   S(-29,  16),   S(  1, -17),
            S(-13,  55),   S(  3,  41),   S(-22,  84),   S(-14,  81),   S(-35,  72),   S( -3,  56),   S(  3,  32),   S( -6,  16),
            S(-25,  64),   S(-23,  76),   S(  3,  73),   S( 17,  68),   S( -3,  65),   S( 27,  70),   S( -3,  58),   S( 24,  -3),
            S(-50,  58),   S( -9,  46),   S(-26,  96),   S( 42,  25),   S( 46,  32),   S(105,  -6),   S( 56,  22),   S( 28, -30),
            S( 31,  12),   S(-56,  70),   S( 56,   3),   S( 28,   0),   S(-18,  53),   S( 26, -19),   S( 42,  19),   S( 10, -20),
            S(-72,  21),   S(  7,  61),   S(-11,  54),   S(-39, -69),   S(-22,  -2),   S(-47, -62),   S( 48,  15),   S(-154, -78),

            /* knights: bucket 3 */
            S(-57,  14),   S(-16, -45),   S( -6, -18),   S( -4,  -4),   S(  7,  -8),   S( -5, -22),   S(-21, -14),   S(-24, -62),
            S(-18, -36),   S( -7,  -4),   S(  9,  -8),   S(  9,  -9),   S( -2,   2),   S( 23, -15),   S( 22, -51),   S( 11, -54),
            S(-15,   2),   S(-10,  12),   S(  6,  21),   S(  1,  43),   S( 20,  32),   S(  4,  18),   S( 11,  -2),   S( 19, -35),
            S(  6,   4),   S( 17,  37),   S( 28,  48),   S( 12,  58),   S( 16,  72),   S( 27,  65),   S( 30,  43),   S( 10,  29),
            S(-10,  24),   S( 17,  39),   S( 23,  66),   S( 29,  87),   S( 30,  76),   S( 46,  80),   S(  9,  89),   S( 60,  58),
            S(-14,  17),   S(  1,  53),   S(  4,  69),   S( 20,  77),   S( 59,  73),   S(130,  74),   S( 57,  84),   S(  9,  92),
            S(-40,  49),   S(-11,  53),   S(-60,  81),   S(  6,  77),   S( 50,  42),   S( 90,  58),   S( 33,   4),   S( 59, -10),
            S(-172,  75),  S(-16,  80),   S(-17,  69),   S( 40,  47),   S( 64,  55),   S(-67,  75),   S(-35, -74),   S(-89, -143),

            /* knights: bucket 4 */
            S( 12,  21),   S(-15,  11),   S(-73,  13),   S(-48, -11),   S(-14,   4),   S(-40, -16),   S( 15, -34),   S(-31, -31),
            S( 21,  23),   S( 13, -11),   S(-17,  21),   S(-13,  -2),   S(-11, -11),   S( 25, -49),   S( 14,   7),   S(-53, -11),
            S(  1, -11),   S( 20,  -8),   S( 77,  -9),   S( 74,   1),   S(  7,  44),   S( 42, -35),   S( 27,  -3),   S( -8, -12),
            S( -5, -29),   S( 17,   5),   S( 16,  10),   S( 88,  10),   S( 37,  14),   S(  1,  42),   S(-48,  23),   S(-19, -10),
            S( 19, -26),   S( 27,  18),   S( 45,  23),   S( 38,  29),   S( 64,   8),   S( -6,  36),   S(-12,  10),   S(-40,  42),
            S(  0, -24),   S( -7,  -3),   S( 46,   4),   S( 49,  17),   S(-18,  16),   S(-13,  44),   S(-15,  42),   S( 40,  -1),
            S( -4,  -3),   S(-40, -45),   S(  5, -11),   S( 22,  25),   S( 42,  26),   S( -7,   4),   S(  8,  40),   S(-48, -13),
            S(  2,  20),   S(-17, -57),   S( -9, -30),   S(  7, -20),   S(  2, -10),   S( -5,  26),   S( -3,  13),   S(  5, -14),

            /* knights: bucket 5 */
            S( 19,  18),   S(  7,  30),   S(-23,  44),   S(-10,  30),   S(-25,  32),   S( 14,  23),   S(-12,  31),   S(  1,  10),
            S( 22,  37),   S( 35,  16),   S( 29,  11),   S(-43,  30),   S( 56, -14),   S( 25,   2),   S(-27,  65),   S(-30,  24),
            S(-21,   3),   S( -9,  10),   S( 24,  15),   S( 37,  15),   S( 12,  30),   S(-18,  34),   S(  8,   0),   S(-10,  28),
            S( 51,  12),   S( 31, -19),   S( 29,   9),   S( 78, -12),   S( 95,  11),   S( 83,   2),   S( 27,  10),   S( 16,  20),
            S( 41,   2),   S( 53, -24),   S( 96,  -6),   S(124,  -1),   S(108, -24),   S( 60,  14),   S( -1,  14),   S(  8,  34),
            S(-11, -18),   S( 44, -29),   S( 11, -26),   S(  0,  12),   S( 27,  12),   S( 66,   9),   S( 11,  19),   S( 13,  21),
            S(-10,   3),   S(-25, -41),   S(  1, -73),   S(-25, -39),   S(-18, -46),   S( 14, -14),   S(  0,  33),   S( 10,  24),
            S(-32, -45),   S(-14, -67),   S(  7,   0),   S(-15, -24),   S( 22,  14),   S(  2,  27),   S( 22,  58),   S(-13, -21),

            /* knights: bucket 6 */
            S( -8,   9),   S(-65,  38),   S(-18,   8),   S(-33,  44),   S(-44,  44),   S(-26,  39),   S( -9,  26),   S(-44,  17),
            S(  4, -20),   S(  2,  49),   S(-47,   9),   S(  7,  26),   S( 34,   7),   S(-49,  34),   S( 11,  30),   S(-31,  40),
            S(-14,  19),   S( 32,  16),   S( 13,  32),   S( 34,  31),   S( 40,  40),   S(-37,  44),   S(-13,  31),   S(-37,  32),
            S( -5,  58),   S( 37,  20),   S( 35,  33),   S( 48,  29),   S( 88,  -7),   S( 88,  -1),   S( 30,   0),   S(-23,  50),
            S(-17,  34),   S( 43,   7),   S(112,   6),   S(115,  13),   S(112, -22),   S( 31,  46),   S(135, -26),   S(  5,  32),
            S(  3,  14),   S( 36,  11),   S( 77,  11),   S( 47,   9),   S( 59,  -3),   S( 46,  17),   S( 14, -10),   S( 22,  -8),
            S( 12,  26),   S( 19,  36),   S( 22,  43),   S(  0,  -9),   S( 32,  -9),   S( 28,  -7),   S( -9, -28),   S(  2,  23),
            S(  3,  12),   S( -3,  32),   S(  9,  20),   S(-10,  16),   S( 15,  -9),   S(  4, -15),   S(  5,   9),   S(-29, -41),

            /* knights: bucket 7 */
            S(-47, -49),   S(-44, -42),   S(-25,   5),   S(-45,  31),   S(-24,  -1),   S(-34,   7),   S(-52,  19),   S(-12,  19),
            S(-46, -77),   S(-11, -47),   S(-13,   0),   S(-42,  26),   S(-25,   4),   S(  0,  17),   S(-11,   1),   S(-65,  10),
            S(  3, -42),   S(-26, -32),   S( 19, -15),   S(  8,  22),   S( 46,  24),   S( 62,  -1),   S( 44,   9),   S( -5,  23),
            S(-40,  14),   S( 24,  12),   S( 55, -13),   S( 86,   3),   S(114,  -9),   S( 62,  26),   S(104, -17),   S(106, -17),
            S(  2,  -1),   S(  7,  -4),   S( 17,  28),   S(106,  -7),   S(107,   4),   S(129, -24),   S(216,  -7),   S( 51, -14),
            S( -8,  28),   S(  0,  12),   S( 26,  -7),   S( 43,  36),   S(114, -10),   S(128,  -4),   S( 37, -18),   S( 20, -42),
            S(-35,   9),   S(  4, -16),   S(  9,  16),   S( 35,  33),   S( 36,   8),   S( 40,  16),   S(-18, -46),   S(-26, -28),
            S(-23, -22),   S(-14,  -4),   S(-23,  18),   S(  7,   8),   S(  6,  11),   S( 11,  -7),   S(  1,   6),   S( -3,  -2),

            /* knights: bucket 8 */
            S( -2, -14),   S( 15,  20),   S( 13,  10),   S( -6, -44),   S( -7,  11),   S(-15, -51),   S( 14,  15),   S( -6, -25),
            S(-15, -52),   S(-10, -30),   S( -8, -47),   S(-24,  10),   S(-17,  25),   S( 22,  23),   S( -5,  -6),   S( -3,  -5),
            S( -9, -38),   S( -3, -13),   S( 18, -43),   S(  5,  10),   S( -5, -16),   S( 16,  12),   S( -9, -20),   S(  1, -20),
            S(-18, -54),   S(-14, -37),   S( -1,  16),   S(  8,  17),   S(-11,  -7),   S( -6,  11),   S(-14, -12),   S(-10, -19),
            S( -6, -30),   S( -2, -17),   S( -2, -27),   S( -6,   1),   S( -2,  22),   S( -7,   6),   S( -7,   0),   S(  1, -11),
            S( -2,  15),   S( 18,   6),   S( 10,  38),   S( -7,  -5),   S( -2,   4),   S( -5, -35),   S(-10, -15),   S( -8, -16),
            S(  5,  30),   S(  0, -40),   S( -8, -20),   S(  5,  25),   S( -2,  -6),   S(  8,   6),   S(-13, -23),   S( -3, -19),
            S( -3,  -2),   S( -4,   3),   S( -4,  -5),   S(  3,   2),   S(  0,  10),   S(-10, -15),   S(  3,  11),   S(  0,   2),

            /* knights: bucket 9 */
            S(-14, -54),   S( -9,  -4),   S( -4, -37),   S(  5, -12),   S(-24,   3),   S(-16,  25),   S(  3,   4),   S(  4,   3),
            S( -5, -14),   S(-10, -49),   S(-23, -120),  S(-19, -50),   S(  4, -14),   S(-27, -72),   S( -5,  19),   S(-15, -11),
            S( -2,   4),   S(-16, -25),   S(-11, -42),   S( 13, -31),   S( -6,  -6),   S( 14,  11),   S( -9,   2),   S( -4,  -6),
            S(-30, -57),   S(-16, -72),   S(  0, -14),   S(-15, -46),   S(-24, -31),   S(  7,   7),   S(-30, -71),   S( -4,  16),
            S(  4,  16),   S(-18, -33),   S(  8, -31),   S( 14, -33),   S( -5, -35),   S(  2,  17),   S( -9, -16),   S(  2,   2),
            S(-10, -34),   S(-22, -53),   S(-13, -22),   S( -1,   1),   S( 10,   8),   S( -2,   3),   S( -4,  17),   S( -7,  -1),
            S(-12, -20),   S( -7,  19),   S(-16,   2),   S(-22, -17),   S( -4, -17),   S( 16,  61),   S( -1,  37),   S( -9, -24),
            S( -1, -15),   S(  7,  15),   S( -3,   1),   S( -7,  -7),   S(-14, -13),   S(-11, -22),   S(  0,  -3),   S( -2,   5),

            /* knights: bucket 10 */
            S( -6, -24),   S( -9,   2),   S(-15,  -8),   S(-26,  -1),   S(-18, -42),   S( -9, -23),   S(  0,   8),   S( -7,  10),
            S( -4, -30),   S(  5,  15),   S(-15, -27),   S( -8, -51),   S(  3, -31),   S(-37, -91),   S(-17,  -2),   S( 10,  37),
            S(  4,  13),   S( -3,   2),   S(-19, -30),   S(  1, -53),   S(-14, -39),   S( -1, -14),   S( -3, -50),   S(-15,   3),
            S(-14, -15),   S(-13, -38),   S( -9, -18),   S(  9, -35),   S(-14, -19),   S(  4,  -4),   S(-17, -78),   S(  3,  13),
            S(-12, -27),   S( -7, -22),   S(-19, -10),   S( -6, -28),   S( 28,  12),   S( -5, -44),   S( -2, -13),   S( -3,  10),
            S( -5,   3),   S(-14,  -5),   S( -9,  11),   S(  0,  20),   S( -3,   5),   S(-10,   4),   S(  1,  -5),   S(-17, -22),
            S(  4,  11),   S( -2, -14),   S(  2,  -6),   S(  8, -25),   S( -3,  28),   S( -8, -37),   S( -8,  13),   S(-11, -19),
            S(  2,   6),   S( -8, -12),   S(  3,  38),   S( -1,  -6),   S(  2,  13),   S( -7, -24),   S(  1,  -3),   S(  0,   0),

            /* knights: bucket 11 */
            S( -9, -35),   S(-21, -38),   S(-11, -20),   S( 15,   3),   S(-47, -58),   S(-27, -18),   S(-13,  -5),   S( 11,  32),
            S(  1,  -4),   S(-35, -82),   S(  1, -19),   S( 10,  12),   S( 19,  23),   S(-12, -41),   S( -6,  -4),   S( -8,  -4),
            S(-12, -32),   S(-20, -12),   S(  0,  -5),   S( 20,  19),   S(-18,   0),   S(  8,  -5),   S( 16, -21),   S(-19, -36),
            S(-14,   3),   S( 11,   6),   S( -4, -16),   S( 14,   1),   S( 31, -11),   S( -6,  -8),   S( 15,  11),   S(  8,  -2),
            S(-15,  12),   S( -4, -41),   S(-25,  25),   S(-15,  -6),   S( 22,  -9),   S( 13,  27),   S(-12, -82),   S(-19, -26),
            S(-13, -45),   S(-16, -68),   S(  5,   1),   S( 21,  16),   S(  5,  29),   S(-21, -31),   S( -6, -40),   S( -2,  26),
            S(  2, -13),   S(  5,  14),   S( -4,  -1),   S( 14,  12),   S( 13,  -6),   S( -2, -29),   S(  3, -10),   S( -3,  15),
            S(  1,  -5),   S(  0,  20),   S( -6, -18),   S(  1,  20),   S(  5,  -7),   S(  4,   2),   S(  1,  18),   S(  0,  -4),

            /* knights: bucket 12 */
            S(-15, -54),   S(  0,  -8),   S(  4,  -2),   S( -6,  -6),   S( -7,  -4),   S( -2,  -2),   S( -2,   1),   S(  0,   1),
            S( -1,  -2),   S(  2,   1),   S( -1, -15),   S( -5, -23),   S( -6, -28),   S( -2,  -2),   S(  3,   4),   S( -1, -11),
            S( -3, -17),   S( -4, -15),   S( -2,   2),   S(-15, -28),   S(-10, -31),   S( -9,  -3),   S( -7,  -1),   S( -6,  -1),
            S(  0,   8),   S( -7, -53),   S( -4,  28),   S(  8,  39),   S(  6,  10),   S( -5,  15),   S( 10,  24),   S( -3,  -9),
            S(  1,  14),   S( -4, -12),   S(  0,   3),   S(-10, -32),   S(  8,  19),   S( -6,   9),   S(-11, -14),   S(-14, -21),
            S( -5,  -5),   S(  2,   2),   S( -8, -18),   S(  4,   5),   S( -2,   1),   S(  3,   4),   S( 12,  17),   S( -2,   3),
            S( -7, -29),   S( -4,  -6),   S( -3,  10),   S(  0,  -5),   S(  0,  13),   S( -8,  -7),   S(  0,  -3),   S( -5,  -5),
            S(  0,   0),   S(  1,   0),   S( -2,  -4),   S(  1,  -4),   S(  1,  -2),   S(  6,   9),   S( -2,  -6),   S( -2,  -5),

            /* knights: bucket 13 */
            S( -2, -21),   S( -6,  -7),   S( -6, -34),   S( -1, -12),   S( -8, -35),   S(  0,  16),   S( -5,  -2),   S(  3,  15),
            S( -1,   3),   S(  1,  -3),   S( -1,  -9),   S(  4,  -1),   S( -9,  -8),   S(  2,  -1),   S(  2,  25),   S( -2,   0),
            S(  5,  -2),   S(  6,  17),   S(  3,   1),   S( -4, -30),   S( 12,  44),   S(  2,   7),   S(  9,   6),   S(  3,  18),
            S(  0,  -8),   S( -1,   0),   S(  0,  10),   S( -7,  24),   S(  3,  17),   S( 16,  54),   S(  5,  28),   S( 13,  33),
            S(  1,  19),   S( -6, -16),   S( -4,  16),   S( -4,  13),   S( -6,  25),   S(  3,  33),   S(  5,  13),   S( -3, -10),
            S( -4, -11),   S( -3,   2),   S( -3,  14),   S( -2,   5),   S(  1,  23),   S( -6,  -1),   S( -6,   9),   S(  2,  11),
            S(  2,   8),   S(  4,   8),   S( -9, -12),   S(  0,  18),   S(  4,  23),   S( -6,  -2),   S( -2,  -2),   S(  0,   5),
            S(  3,   8),   S( -3,  -6),   S( -1,  -7),   S(  0,   6),   S( -1,   6),   S(  0,   5),   S(  0,  -4),   S(  0,   1),

            /* knights: bucket 14 */
            S(  2,   9),   S( -3,  -9),   S( 14,  48),   S( -2,   5),   S( -1,  -6),   S(  0,  28),   S( -1,  -1),   S( -3, -11),
            S( -4, -14),   S( -8, -22),   S(  5, -12),   S( -3, -10),   S(  4,   7),   S(  6,   2),   S(-13, -11),   S(  5,  57),
            S( -5,  -7),   S( -2, -40),   S(  9,  26),   S(-11, -27),   S(-11, -22),   S(  6,   3),   S( -2,   8),   S(  3,  19),
            S( -3, -15),   S( -1, -18),   S(-24, -18),   S(-12,  20),   S( 10,  52),   S(  0,  13),   S(  1,  15),   S( -2,  28),
            S( 15,  37),   S(-20, -41),   S(-11,  -2),   S( -4,   5),   S( -6,   2),   S( -5,  39),   S( -9, -12),   S(  2,   9),
            S(  0,   8),   S(  8,   4),   S( 12,  23),   S(  7,  19),   S(  1,  16),   S( -2,  14),   S( -3,   8),   S( -5,  -5),
            S(  1,   8),   S( -2,  -5),   S(  4,  21),   S(  2,  -7),   S(  5,  27),   S( -9, -26),   S(  4,   9),   S(  5,  -1),
            S(  0,   0),   S(  2,   5),   S( -1,  -1),   S(  5,   9),   S( -1,   3),   S(  1,  -3),   S(  0,   1),   S( -1,  -1),

            /* knights: bucket 15 */
            S( -3, -11),   S( -1,   5),   S(  3,  25),   S( -1,   7),   S(-11, -39),   S(-15, -40),   S( -5, -24),   S( -5, -24),
            S(  1,  -4),   S(  6,  18),   S(-12, -23),   S(  8,  46),   S( -3,  17),   S( -6, -16),   S( -5,  -4),   S(  4,   2),
            S( -3,  -6),   S(-10, -34),   S( -6, -29),   S( -2,  -5),   S(-21, -31),   S(  2,  21),   S(  0,  -2),   S( -1,  -4),
            S( -1,  -6),   S( -4,   1),   S( -7, -24),   S( -4,  -6),   S( -8,  14),   S( -8,  44),   S(  3,  10),   S(  3,  13),
            S(  0,   3),   S( 12,  28),   S(  1,  27),   S( -8,   8),   S( 29,  36),   S( 17,  39),   S(  2, -12),   S(  5,  23),
            S(  3,  13),   S( -7, -23),   S(  3,   5),   S(-14, -18),   S(-12, -25),   S( 10,  49),   S(  6,  24),   S(  2,  13),
            S( -5,  -5),   S( -6, -22),   S(  4,  15),   S( -1,  -6),   S(  4,  18),   S(  5,  16),   S(  2,  17),   S(  3,   5),
            S( -1,  -4),   S( -3, -12),   S(  2,   4),   S( -1,  -3),   S(  2,  11),   S(  1,   0),   S(  0,  -1),   S(  0,   0),

            /* bishops: bucket 0 */
            S( 28, -23),   S(-23,   7),   S(-25,  28),   S(-25,  22),   S(  0,  -6),   S(  2,  10),   S( 54, -37),   S( 31,  -1),
            S(-51, -19),   S(  2,  -4),   S(-45,  43),   S( -4,  20),   S( -1,  25),   S( 57,   0),   S( 36,  50),   S( 30, -31),
            S( 28,   3),   S( -1,  52),   S( -2,  41),   S(  3,  12),   S( 23,  13),   S( 41,  49),   S( 35,  -8),   S( 40, -23),
            S(  0,  -7),   S( 51, -38),   S( 10,  16),   S( 58,   2),   S( 59,  55),   S( 31,  54),   S( 13,  19),   S(  5,  49),
            S( 31, -13),   S( 35, -17),   S( 78,  -9),   S( 80,  42),   S(104,  16),   S( 11,  39),   S( 40,  62),   S( 14, -11),
            S( 24,  72),   S( 41,  49),   S( 92,  23),   S( 55,   3),   S(  6,  41),   S(  1,  67),   S( 56,  22),   S( -8,  -7),
            S(-56, -108),  S(128,  51),   S( 73,  62),   S(  0, -12),   S( 34, -25),   S(  9,  30),   S(-14,  23),   S(-19,  48),
            S(-14, -43),   S( -2,  24),   S( 12, -12),   S(-28,   0),   S(-32, -15),   S( -8,   0),   S(-37,  19),   S(-28, -32),

            /* bishops: bucket 1 */
            S(-61,   4),   S( -7, -18),   S(-33,  48),   S( -4,   7),   S(-40,  36),   S(  6,   7),   S( 31, -43),   S( 23, -16),
            S(-13, -18),   S(-21,  14),   S( -2, -18),   S(-22,  27),   S( 30, -10),   S( -3,   1),   S( 61, -35),   S( 14, -22),
            S(-33,   9),   S( 38, -27),   S(-19,  15),   S( 20,   3),   S( -1,  -2),   S( 35,  -2),   S(  2,   9),   S( 59,  -1),
            S( 33, -37),   S( 34,  -4),   S( 27, -15),   S( -1,  23),   S( 55,  -4),   S( 11,  21),   S( 70,  -1),   S(-21,   0),
            S( 44, -23),   S( 52, -18),   S(  7,  19),   S(100, -13),   S( 74,   3),   S( 56,  21),   S(  8,  28),   S( 47, -23),
            S( 68, -59),   S( 39,   6),   S( 71,  -9),   S( 78, -19),   S( 98,   5),   S(-40,  27),   S(-40,  71),   S(-18,  -4),
            S( 12, -63),   S( -1, -54),   S( 31, -31),   S( 15,  31),   S(  5,  75),   S(-44,  34),   S(  7,   6),   S(-30,  21),
            S( -5, -48),   S(-25,  19),   S(-20, -57),   S(-15,  -1),   S( -7,   4),   S( 20, -11),   S(  0,   8),   S(-68,  33),

            /* bishops: bucket 2 */
            S(  9, -12),   S(-19, -13),   S( -2,   8),   S(-24,   2),   S(  7,  20),   S(-21,   8),   S( -5,   9),   S( -7,  -6),
            S( 17, -42),   S(  8,  -6),   S(-10, -14),   S(  5,  13),   S(-10,  13),   S( -5,  22),   S(  8, -21),   S(  7, -57),
            S( 43, -14),   S( 17,  -8),   S( -2,  24),   S(-10,   0),   S( -2,  26),   S( -3,  -8),   S( -3, -25),   S(-12, -13),
            S(-23,  -3),   S( 39,  15),   S(  1,   5),   S( 33,  17),   S( -2,  19),   S( -5,  25),   S(-23,   3),   S( -5, -17),
            S( 11,   4),   S(-26,  42),   S( 64,  22),   S( 13,  27),   S( 24,  23),   S( 15,  12),   S( 12,  41),   S( 31,  -9),
            S(-22,  15),   S( -9,  41),   S(-33,  16),   S(100,  -7),   S( 59,   6),   S(112,  -3),   S(102,   3),   S( 33, -57),
            S(-39,  64),   S(-41,  14),   S( -2,  15),   S( -3,  22),   S(-13, -40),   S( -2, -30),   S(-22,   2),   S(  1, -70),
            S(-80,   0),   S(-38,  27),   S(  2,   6),   S(-40,  34),   S(-15, -17),   S(-31,  -3),   S( 23, -36),   S(-59, -22),

            /* bishops: bucket 3 */
            S( 22,   5),   S( 31, -18),   S( 16, -31),   S( 10,   5),   S( 17,   8),   S( -6,  35),   S(-21,  54),   S(  4, -32),
            S( 24,   6),   S( 28,  -7),   S( 13,  -1),   S( 18,   5),   S( 19,  21),   S( 23,  11),   S(  9,   5),   S( 29, -34),
            S( 10, -11),   S( 28,  32),   S( 16,  40),   S( 15,  29),   S( 17,  31),   S( 19,  31),   S( 17,  -8),   S(  9,   2),
            S(-22,  23),   S( 20,  33),   S( 25,  41),   S( 30,  47),   S( 34,  25),   S( 28,   7),   S( 30,  -6),   S( 29, -50),
            S(  8,  24),   S( 14,  52),   S( -2,  67),   S( 45,  50),   S( 57,  47),   S( 50,  16),   S( 29,  24),   S( -2,  12),
            S(-10,  47),   S( 11,  57),   S( 10,  37),   S( 14,  53),   S( 50,  45),   S( 88,  67),   S( 35,  34),   S( 45,  82),
            S(-16,  63),   S(  8,  54),   S( 11,  25),   S(-12,  56),   S( 24,  32),   S( 42,  55),   S(-18,   3),   S( 11, -11),
            S(-60,  46),   S( -6,  37),   S(-80,  48),   S(-36,  32),   S(  9,   7),   S(-43,  29),   S( 26,   5),   S( 31,  15),

            /* bishops: bucket 4 */
            S(-10,  27),   S(-25,   2),   S(-56,   7),   S(-59,  16),   S( -5, -19),   S(-35,   7),   S( -2, -16),   S(-26, -60),
            S(  8,  -4),   S(  3,  14),   S( 68, -24),   S(-29,  -3),   S(-38,  13),   S( 33, -47),   S( -1, -21),   S(-32, -35),
            S( 24,   6),   S(  6, -28),   S(  2,  15),   S(-17,   4),   S( -7,   4),   S(-42,  30),   S(-25,  -4),   S(-107,   0),
            S(  6,  -1),   S( 63,  -7),   S( 54,   4),   S( 26,  54),   S(  8,  -3),   S( 10,  14),   S(-53,  20),   S( 23, -12),
            S( 16, -25),   S( -6,  -5),   S( 22, -33),   S( 59,  15),   S(-12,  50),   S( 45,   7),   S(-26,  82),   S(-55,  44),
            S(-42, -66),   S(-30,   0),   S(  3,  32),   S( -5,  12),   S(-72,  45),   S( 12,  27),   S(-17,  30),   S(-10,  -9),
            S(  3,  -7),   S(-22,  27),   S( -1,   1),   S(-15, -22),   S(-28,   5),   S( 22,   7),   S(-11,   5),   S( 39,  66),
            S(  7, -14),   S( -4, -32),   S(-21, -34),   S( -7, -39),   S(-13,  -9),   S( -3,  -1),   S( -1,  46),   S( 14,  23),

            /* bishops: bucket 5 */
            S(-39,  15),   S( 12, -16),   S(-61,  19),   S(-60,  20),   S(-60,  18),   S(-42,   1),   S(-28,   9),   S(-39,   0),
            S(-32,  -3),   S(-33,   5),   S(  4,  -5),   S(  0,   0),   S(-55,  27),   S(-34,  25),   S(-63,  12),   S(  8, -15),
            S( 10,  18),   S(-49,   5),   S( 38,  -3),   S(-18,  15),   S(-16,  21),   S(-47,  19),   S( -2,   4),   S(-17,  27),
            S( 11,   1),   S(  5,  19),   S( 57, -13),   S( 67,  -8),   S( 19,  13),   S( 20,  20),   S(-95,  42),   S(-35,   4),
            S(  1, -15),   S( 30,  -9),   S( -7,  10),   S( 10,  -7),   S(  3,  15),   S( -6,  13),   S(  0,  37),   S(-64,  20),
            S( -8, -16),   S(-19,  15),   S( 45, -20),   S( -7,   9),   S(-27,  23),   S(-41,  24),   S(-36,  17),   S(-60,  48),
            S(-26, -18),   S(-12,   3),   S(-18,   3),   S(  3,  19),   S( 14, -15),   S( 25,  23),   S(-14,  34),   S( -1,  12),
            S(  0,  15),   S( 10, -47),   S( -6, -32),   S(-18,  -8),   S(-25,  25),   S( 12,  15),   S(-19,  24),   S( 31,   0),

            /* bishops: bucket 6 */
            S(-11, -22),   S(-27,   4),   S(-28,   3),   S(-47,  19),   S(-42,  29),   S(-66,  23),   S(-44,  31),   S(-56,   1),
            S(-43,  24),   S(  0, -29),   S(-44,  24),   S(-43,  32),   S(-55,  28),   S(-49,  30),   S(-52,  29),   S(-50,  -3),
            S(-10,   8),   S(-49,  19),   S(  2,   4),   S(-42,  35),   S(-20,  30),   S(-41,  20),   S(-22,   3),   S(-28,  27),
            S(-84,  32),   S(-59,  30),   S(-20,  23),   S( 47,  20),   S( 14,  42),   S( 38,  14),   S( 36, -14),   S( -1,   8),
            S(-27,  24),   S(-31,  18),   S( 16,   8),   S(105, -18),   S(-23,  20),   S( -5,  -2),   S( -8,  32),   S(-46,  -3),
            S(-29,  12),   S( 22,   3),   S(-29,  16),   S(-68,  29),   S( 12,  14),   S( -7,  -2),   S( -8,   6),   S(-12, -20),
            S( -1,  34),   S(-70,  40),   S(-44,  37),   S(  0,  18),   S(  5,  -4),   S(  7,  10),   S( -1,   4),   S(-18,  14),
            S(  2,  22),   S(-16,  13),   S(-11,  28),   S( 34,  -7),   S(-29,  11),   S( 13, -23),   S(-22,  26),   S(  8,  27),

            /* bishops: bucket 7 */
            S( 13, -63),   S(-81, -25),   S(-48, -27),   S(-47,  -6),   S(-17, -10),   S(-74,  16),   S(-60, -52),   S(-82,   3),
            S( -5, -41),   S(  3, -34),   S( 15, -27),   S(-29,  -5),   S(-52,  14),   S(-38,   7),   S(-51, -20),   S(  7, -28),
            S(-31, -19),   S(-49,  11),   S(-11,  -1),   S( 26, -26),   S( 12, -14),   S( -2,  -6),   S(-27,  -3),   S(-65,  12),
            S(-25, -24),   S(-47,   8),   S(-47,  32),   S(-19,  24),   S( 90,  10),   S( 31,  -8),   S( 31, -34),   S(  2, -41),
            S(-16,   5),   S( 17, -14),   S(-54,  39),   S( -2,   6),   S( 62, -13),   S( 73,  -9),   S(-17,  23),   S(-31, -20),
            S(-65,  28),   S(-45,  48),   S(  4,  -7),   S(-64,  25),   S( -3,  15),   S( 22,   0),   S( 37,  58),   S(-61, -73),
            S(-36, -16),   S(-23,  34),   S(-60,  26),   S(-29,  36),   S(-12,  -7),   S( 31, -40),   S( 48, -30),   S(  1, -39),
            S(-15,  -2),   S( 10,   2),   S( -1,  16),   S(-20,  -4),   S( 10, -26),   S( 31, -38),   S( 31, -18),   S( -4, -10),

            /* bishops: bucket 8 */
            S( 27,  81),   S(  5, -14),   S( -9, -24),   S(-16,  27),   S( -8,  28),   S(-22, -46),   S(-24, -31),   S( -9,  -6),
            S( -3, -18),   S( 13,  29),   S( 31, -24),   S(  9,  20),   S( -8, -40),   S(  5,  -9),   S(-21, -26),   S( -4,  21),
            S(-16, -11),   S( -8,  -1),   S( 18,  29),   S( 19,  17),   S( 11,  20),   S(  5,  22),   S(-29, -21),   S(-33, -32),
            S( -8,  -8),   S( 33,  26),   S( -2,  21),   S( 24,   8),   S( 10,  29),   S( 10,  30),   S(-16,  11),   S(  3,  -7),
            S( 11,   8),   S( 26,  75),   S( 21,   6),   S(-11,  41),   S( 13,  12),   S(-22,  39),   S(-14, -30),   S(  6,  33),
            S( -6, -14),   S( 15,   7),   S( 13,  25),   S( 14, -17),   S( 12,  11),   S( 43,  36),   S(-11,  44),   S( -3,  37),
            S( -9,  -3),   S(-20, -50),   S( 21,  12),   S( 13,   8),   S(  1, -19),   S( 24,  29),   S( 28,  50),   S( -9,  11),
            S( -3,   2),   S(  1,  -9),   S(  0,  13),   S(  4,   4),   S( 28,  -1),   S( 23,  23),   S( 19,  55),   S( 45,  33),

            /* bishops: bucket 9 */
            S(  9,  46),   S( 10,  -3),   S(  2,  10),   S(-48, -43),   S(-32, -14),   S(-13, -11),   S(-11,  -9),   S(-10,   5),
            S(-13, -41),   S(  4, -12),   S( 15,  37),   S(-33,  -9),   S(-22,  -4),   S(-20,  -2),   S(-27,   4),   S(-17, -13),
            S(-21, -18),   S( 24,  13),   S(  4,  -3),   S( 13,  23),   S( -8,   2),   S( -6,  10),   S(  9,  18),   S(-13,   4),
            S(-18,   3),   S(  1, -11),   S( 31,   8),   S( 39,   7),   S(-24,  35),   S(-11,  13),   S(  3,  39),   S( -1,  23),
            S( 24,  28),   S( -4,  33),   S( 25,  55),   S( 20, -25),   S( 22,  13),   S( 26,  20),   S(  2,  32),   S(-13, -39),
            S( 11,   1),   S( -4,  12),   S(  6,  19),   S( 10,  -6),   S( 36, -40),   S(-12,  75),   S( 20,  24),   S(  8,  57),
            S(  9, -17),   S(-18,   6),   S( -8, -10),   S( 16, -11),   S(-14,  18),   S( 36,  29),   S( 34,  20),   S( 18,  34),
            S( 10,  40),   S( -4, -48),   S( 13,   0),   S(  9,   3),   S( 13,  41),   S( 21,  21),   S( 11, -10),   S( 44,  50),

            /* bishops: bucket 10 */
            S( -5, -26),   S( 13,   3),   S(-17, -25),   S(-27, -30),   S(-69,  -2),   S(-36, -63),   S(  5, -10),   S( -6,  36),
            S( -5,  11),   S(-14, -41),   S(-21,   0),   S( -9, -49),   S(-34,  -8),   S(-30, -42),   S(-44,   4),   S( -9, -20),
            S( -1, -21),   S(-41, -37),   S(  1, -29),   S( -4,  43),   S( -5,   2),   S(  4,  -2),   S(  6, -10),   S( -8, -40),
            S(-20,  16),   S(-27,  -1),   S(-17, -33),   S( -1,  14),   S(-21,  61),   S( 28, -16),   S( 38,   5),   S(-15, -28),
            S( 12,  33),   S(-46,  42),   S(  7,  10),   S(  2,  52),   S( 50, -31),   S( 10,  60),   S( 15, -10),   S( 13,  -7),
            S( -4,   4),   S( 13,  17),   S(  8,  12),   S( 10,  24),   S( 20, -33),   S( 16,  17),   S(  1,   4),   S( 31,  21),
            S( 13,  20),   S( 26,  14),   S( 27, -32),   S(  6,  27),   S(-22,  41),   S( -6, -31),   S( 22,   0),   S( 13,  14),
            S( 16,  69),   S( 10,  26),   S( 42,   1),   S( 12,  32),   S(  5,  -3),   S( -1,  20),   S(  7,  38),   S(  2, -11),

            /* bishops: bucket 11 */
            S( 16, -23),   S(-14,   1),   S(  1, -19),   S(-12,  -7),   S(-26, -44),   S(-21, -30),   S(-18, -15),   S( -5,  15),
            S(  1,  -2),   S( 17,  30),   S(  3,  19),   S( 13, -15),   S(  4,  11),   S(-35, -15),   S(-32,  -8),   S(  2, -13),
            S(-14, -18),   S( -3,  10),   S(-11,  -4),   S(-33, -10),   S(-17, -13),   S(  7, -10),   S( -5, -24),   S( -7, -28),
            S( -4,  -1),   S(-14, -10),   S( -2,  13),   S(-47, -32),   S(  1, -23),   S( 25,  50),   S( 45,  10),   S( -9, -33),
            S(-29, -26),   S(-21, -35),   S(-48,  43),   S(-27,  23),   S(-40,  48),   S( 45, -20),   S( 15, -55),   S(  8,  11),
            S(-23,  10),   S(  2,   6),   S( -6,  31),   S( -3,  36),   S( 39,   3),   S( 17,   5),   S(  6, -29),   S( -2,   6),
            S( -4, -30),   S( 26,  38),   S( 29,  51),   S( 42,   1),   S( 15, -11),   S( -1,   2),   S(  6,  -4),   S( -7, -16),
            S( 28,  12),   S(  5,  24),   S( 31,  46),   S( 18, -21),   S( 23,  11),   S(  9,  19),   S(-14, -29),   S( 12,   8),

            /* bishops: bucket 12 */
            S( -5, -12),   S( -4, -17),   S(-12,  -5),   S(-12, -14),   S(-13, -16),   S(-15, -24),   S( -2,  -5),   S( -3,  -1),
            S(  2,  -3),   S( 10,   9),   S( -6,  -9),   S(  4,  14),   S( -5,  11),   S(  7,  12),   S(-11, -17),   S( -2,  -8),
            S( 11,   2),   S(  9, -19),   S( 26,   2),   S( 22,  30),   S( -1, -10),   S( -7,  -5),   S(  6,   2),   S( -8,   0),
            S( 17,   8),   S( 12,   1),   S( 18,   4),   S( 28,  61),   S(  8,   1),   S( -6,   6),   S( -7,   8),   S(  5,  15),
            S(  9,   1),   S( 11,   8),   S( -9,  21),   S( 17,  34),   S( 12,  34),   S( 14,  44),   S( -2,  -6),   S(  6,  11),
            S( -2, -10),   S(-20, -24),   S( -2,  11),   S( -3, -13),   S( 19,  22),   S( 20,   8),   S( -9,  -2),   S( -8,  -8),
            S( -1,  -3),   S( 11,  16),   S(  2,  11),   S(  4, -17),   S(  6,   7),   S( 30,  51),   S( 16,  26),   S(  4,   7),
            S( -2,   6),   S(  1,  -4),   S(  3,   8),   S( -2,  -6),   S(  9,  13),   S(  4, -15),   S(  8,   3),   S(  7,  -4),

            /* bishops: bucket 13 */
            S(-14,  -3),   S(  6,  20),   S( -3,  -5),   S( -2, -28),   S(  2,  -2),   S( -5, -18),   S(  1,   0),   S( -1,   0),
            S( -1,   1),   S( -3, -25),   S( -1,   9),   S(  5, -24),   S(-13, -25),   S( 11,  13),   S(  8,  10),   S(  2,   4),
            S(  2, -29),   S( 25,  -2),   S( 23,  21),   S( 20,  31),   S(  4,  42),   S( 12,  17),   S( 10,  20),   S(-14, -14),
            S( 25,  26),   S( 41,  39),   S( 14,  24),   S(-19,   4),   S( 22,  58),   S(  8,  27),   S( 16,  21),   S(  5,  20),
            S( 18,  17),   S( 16,  33),   S( 17, -13),   S( 15,  26),   S(  5,  -6),   S( 14,  18),   S(  9, -18),   S( -4,   5),
            S(  6,  13),   S(  0,  14),   S(  9,   9),   S( 16,  11),   S( -5,   3),   S( -3,   3),   S(  5, -10),   S( 17,   0),
            S( 13,  17),   S( -7, -19),   S( -3, -11),   S(  6,  10),   S(  9,  16),   S( 22,  26),   S( 14,   3),   S(  9,  -6),
            S(  2,  -6),   S( -3,  -7),   S( -1,   5),   S(  7,  21),   S( 13,  21),   S(  9,   7),   S(  9,   1),   S(  7, -16),

            /* bishops: bucket 14 */
            S( -4,  -9),   S(  8,  37),   S( 18,  21),   S(  4,  22),   S( -6,   5),   S( -3,  -8),   S( -8,   8),   S(  2,  35),
            S( -7,  -6),   S( -4, -13),   S( -7,   8),   S( -3, -10),   S( 12,  16),   S( -1,  -2),   S( -9,  19),   S(  3,  27),
            S(  2,   9),   S(  5, -11),   S( 11,  10),   S( 14,  48),   S(  8,  29),   S( 15,  24),   S( 16,  36),   S(  2,  23),
            S(  5,  43),   S( 15, -10),   S( -2,  13),   S(  6,  46),   S(  6,  16),   S( 22,  -1),   S( 20,  33),   S(  7,  -4),
            S( 10,  10),   S( 11,  21),   S(  9,  -1),   S( 33, -16),   S(  0, -14),   S( 16,  11),   S( 20,   3),   S( 11,   6),
            S(  8,  14),   S( 20,  33),   S( -2,  32),   S(  9,  10),   S(  9,  10),   S( -5, -18),   S( -5,   9),   S( 11,  -4),
            S( 11,  34),   S( 13,  22),   S(  5,  24),   S(  5,  -7),   S(  2, -13),   S(  2,  10),   S(  1,   0),   S(  3,  10),
            S( 19,   7),   S( 12,  16),   S(  2,  -1),   S(  4,   4),   S( -7,  -6),   S(  2,  -4),   S(  4,   8),   S(  1,  -2),

            /* bishops: bucket 15 */
            S( -1, -10),   S(  4,   8),   S(-17, -39),   S( -9, -13),   S( -5, -23),   S( -5, -17),   S( -5,  -3),   S( -7, -16),
            S(  8,   8),   S( -5, -10),   S( 11,   1),   S( 11,  18),   S(  6, -13),   S( -4, -23),   S( -3, -13),   S( -5, -15),
            S( -1, -18),   S( -5, -12),   S(  6,  16),   S( 12,  15),   S( 20,  45),   S( 19,  17),   S( 24,  26),   S( 11,   9),
            S(  0,  -7),   S(  2,  -4),   S(  3,  26),   S( -8,  32),   S(  5, -17),   S( 22,   2),   S( 15,  23),   S( 12,  17),
            S( -5, -10),   S(-21,  -2),   S(  8,  30),   S( 20,  26),   S( 20,  19),   S(  7,   6),   S(  5,  -7),   S( -5,  -3),
            S( -9,  -1),   S(  7,  12),   S( 15,  50),   S( 21,  21),   S( 18,  45),   S( 17,   0),   S( -4,  10),   S(  0,  -7),
            S(  5,   2),   S( 12,  22),   S(  5,  25),   S( 14,  23),   S( 12,  18),   S( -8, -13),   S(  1,  -2),   S( -2,  -4),
            S(  5,  -5),   S( 16,  23),   S(  5,  11),   S(  8,  11),   S(  9,  36),   S( -1, -13),   S(  3,  13),   S(  5,   5),

            /* rooks: bucket 0 */
            S(-10,  15),   S( 18, -11),   S(  7,   1),   S(  0,  12),   S(-24,  61),   S( -7,  29),   S(-36,  55),   S(-59,  47),
            S(  9, -35),   S(-17,  28),   S(-28,  25),   S( 21,  21),   S( -7,  68),   S(  5,  14),   S(-34,  48),   S(-38,  53),
            S( 18, -24),   S( 20, -21),   S(-16,  16),   S(  7,   5),   S(-25,  68),   S( 10,  -2),   S(-13,  33),   S( 22, -16),
            S( -7,   7),   S( 31,   6),   S(-32,  34),   S( 39,  14),   S(  8,  54),   S(-20,  53),   S(-35,  46),   S(-16,  41),
            S( 72, -66),   S( 34,  34),   S( 26,  27),   S( 12,  40),   S( 53,   4),   S( 34,  52),   S( 29,  46),   S( 22,  43),
            S( 53, -17),   S( 31,  23),   S(104, -11),   S(122,  19),   S( 31,  53),   S( 52,  40),   S(  8,  70),   S(-31,  82),
            S( 50,   8),   S( 61,  40),   S( 87,  57),   S( 79,   9),   S( 39,  57),   S( 37,  60),   S( -6,  70),   S( 13,  57),
            S(-23, -52),   S(  0,   2),   S(  5,  24),   S( 52,  -1),   S( 42,  68),   S( 53,  34),   S( 45,  -5),   S( 48, -48),

            /* rooks: bucket 1 */
            S(-52,  52),   S(-24,  17),   S(-13,  33),   S(-34,  30),   S(-38,  50),   S(-37,  54),   S(-48,  77),   S(-78,  77),
            S(-34,  28),   S(-15,  -9),   S(-17,  15),   S(-20,  36),   S(-30,  20),   S(-59,  51),   S(-25,  16),   S(-35,  45),
            S(-23,  33),   S(-11, -21),   S( -2,  -2),   S(-27,  25),   S(-39,  31),   S(-37,  30),   S(-57,  60),   S(-27,  65),
            S(-50,  44),   S( -6,  19),   S(-21,  46),   S(-37,  24),   S(-24,  30),   S(-41,  77),   S(-52,  77),   S(-58,  83),
            S(  7,  36),   S( 24, -15),   S( 41,   3),   S( 26,   3),   S( -8,  23),   S(-15,  66),   S(  4,  63),   S( -4,  77),
            S( 58,  22),   S( 68,  -1),   S( 37,  -1),   S(-14,  39),   S( 16,  16),   S( -4,  72),   S( 19,  50),   S( 23,  72),
            S( 11,  55),   S( 22,  -1),   S( 34,  73),   S( 28,  31),   S( 42,  24),   S(-23,  52),   S( 24,  64),   S( 28,  85),
            S( 65, -20),   S( 19, -24),   S(-30, -40),   S(-25, -34),   S( 36,   4),   S( 23,  18),   S( 19,  28),   S( 70,  25),

            /* rooks: bucket 2 */
            S(-61,  70),   S(-51,  64),   S(-42,  62),   S(-35,  21),   S(-26,  29),   S(-35,  29),   S(-26,  11),   S(-77,  60),
            S(-59,  66),   S(-51,  51),   S(-49,  58),   S(-34,  30),   S(-42,  40),   S(-42,  14),   S(-16,   8),   S(-52,  35),
            S(-48,  63),   S(-47,  58),   S(-44,  40),   S(-31,  34),   S(-41,  33),   S(-32,  20),   S(-14,  16),   S(-14,  37),
            S(-38,  83),   S(-40,  73),   S(-51,  77),   S(-54,  53),   S(-45,  52),   S(-31,  39),   S(-28,  26),   S(-23,  37),
            S(-25,  99),   S(-29,  83),   S(-13,  75),   S(-18,  37),   S(-21,  52),   S( 32,  23),   S( -4,  29),   S(-11,  60),
            S( 19,  78),   S( 24,  72),   S( 18,  66),   S(-33,  59),   S( 57,  14),   S( 28,  35),   S(109,  -9),   S( 32,  71),
            S( 45,  56),   S(-20,  71),   S( 36,  39),   S( 18,  24),   S( -9,   5),   S( 32,  68),   S(-56,  85),   S( 28,  62),
            S(-10,  47),   S( 26,  41),   S( 51,  29),   S( -6,  22),   S(-16, -12),   S( 17,   4),   S( 48,   8),   S( -8,  48),

            /* rooks: bucket 3 */
            S(-15,  72),   S( -7,  68),   S(-12, 100),   S( -8,  84),   S(  1,  51),   S(  9,  42),   S( 33,  11),   S( -8,   2),
            S(  0,  54),   S( -9,  73),   S(-18,  98),   S(  3,  80),   S(  2,  54),   S( 20,  10),   S( 47,  -5),   S( 17,  11),
            S( 14,  55),   S(-21,  86),   S(-22,  86),   S(-13,  96),   S( 12,  43),   S( 10,  36),   S( 44,  -3),   S( 30,   8),
            S(  1,  91),   S( -7, 111),   S(-17, 117),   S(  1,  94),   S( -4,  64),   S( 13,  53),   S( 25,  35),   S(  9,  24),
            S( -2, 113),   S(-18, 124),   S( 13, 118),   S( 12, 104),   S( -3, 102),   S( 41,  66),   S( 65,  28),   S( 50,  33),
            S(  4, 118),   S( 12, 111),   S( 30, 114),   S( 40,  96),   S( 98,  50),   S(121,  34),   S( 91,  29),   S( 17,  60),
            S( 14, 112),   S( 12, 105),   S( 30, 110),   S( 12, 116),   S( 28,  99),   S( 95,  47),   S(101, 130),   S(169,  55),
            S(113, -35),   S( 50,  39),   S( 26,  99),   S( 37,  70),   S( 34,  50),   S( 59,  63),   S( 77,  14),   S(111,  28),

            /* rooks: bucket 4 */
            S( -2, -25),   S(  6,  -8),   S(-27,   1),   S(-13,   5),   S(-30,  14),   S(-33,  44),   S(-59,  19),   S(-84,  43),
            S(-24, -58),   S(-68,  -7),   S( -3, -19),   S(-35, -17),   S( 30, -24),   S(-28,  13),   S(-28,   2),   S( -2,   1),
            S(-31, -26),   S(-32, -21),   S(-33, -13),   S( -8, -40),   S(-28, -11),   S(-70,  26),   S(-52,  29),   S(-56,   6),
            S(-56, -33),   S( -4, -18),   S( 20, -41),   S(  4, -23),   S( 53,  -6),   S( 14,   5),   S( 20, -25),   S(-23,   6),
            S( -5, -26),   S(  8, -56),   S( 10, -14),   S( 31, -20),   S( 67,  -3),   S( 69,  19),   S( 15,  16),   S( 19,  22),
            S(-14, -45),   S( 13,  14),   S(  1, -13),   S(  7,   8),   S( -4,  23),   S( 13,  13),   S( 18,  13),   S( 30,  18),
            S(-33,   3),   S( 28,  34),   S( 19,   1),   S( 38,   0),   S( 32,  -6),   S(-37,   2),   S(  8, -17),   S( 17,  -5),
            S( 33, -26),   S(-13,  21),   S( 31,  -2),   S( 23,  -8),   S( 61,   4),   S( 35,  11),   S(  6,  16),   S( 12,  16),

            /* rooks: bucket 5 */
            S( -7,  13),   S(-13,  -3),   S( -5,   7),   S( 26,  -4),   S(-12,  18),   S(  3,  31),   S(-24,  61),   S(-69,  62),
            S(-22,  16),   S( -8, -32),   S( 64, -71),   S( 41, -32),   S( -2,   1),   S(-11,  -8),   S(-21,  24),   S(-23,  38),
            S(-55,  37),   S( 18, -26),   S( 15, -37),   S(-15,  -6),   S( -6,   2),   S( 29,  -5),   S(-30,  15),   S(-57,  27),
            S(-36,  26),   S( 15,  -8),   S( 63, -33),   S( 30,  -7),   S( 59, -22),   S( -3,  40),   S( 10,  22),   S(  8,  47),
            S( 36,  29),   S(  8,  19),   S( 11,  20),   S(-18,  -3),   S(-17,  22),   S( 80,   4),   S( 13,  24),   S( 42,  44),
            S(-26,  32),   S(-30,   6),   S( 17,  21),   S(-20,   9),   S( 18,   8),   S( 14,  37),   S( 59,  20),   S( 36,  43),
            S( 84, -23),   S( 41,  -5),   S(  6,   1),   S( 17,  22),   S( 76,  -8),   S( 85, -30),   S( 92, -21),   S( 37,  12),
            S(  2,  36),   S( 71,  -4),   S( 69, -15),   S( 15,  17),   S( 21,  17),   S( 35,  25),   S( 40,  32),   S( 63,  37),

            /* rooks: bucket 6 */
            S(-48,  52),   S( -6,  16),   S( -4,  18),   S(-19,  16),   S( 24,  -2),   S( 27,  -9),   S( 27,   0),   S(-34,  24),
            S(-58,  34),   S(  2,  18),   S( -8,   5),   S(  5,   4),   S( 18, -12),   S(-41,   4),   S(-22,  -1),   S( -4,  22),
            S(-44,  34),   S(-21,  24),   S(-29,  20),   S( 11,  -2),   S( -2,   3),   S( 10,  -7),   S(  1, -26),   S( 17, -13),
            S(-60,  66),   S(  0,  40),   S( 44,  14),   S( 44,  -6),   S( 25, -12),   S(  4,   6),   S(  1,   8),   S( -8,  37),
            S(-18,  63),   S( 49,  35),   S( 63,  19),   S( 67,  -1),   S( 38,  -5),   S( 28,  11),   S( 42,  -7),   S( 70,   3),
            S( 86,   7),   S( 76,  -1),   S( 83, -11),   S( 47, -38),   S(  6, -10),   S( 17,  -5),   S( 33, -24),   S( 45,   9),
            S( 32,  10),   S(140, -32),   S( 75, -11),   S(150, -62),   S( 52,  -8),   S( 46, -15),   S( 66, -22),   S( 86, -26),
            S( 89, -16),   S( 30,  15),   S(-34,  56),   S( 55,  -4),   S( 48,  -6),   S( 11,  25),   S( 92,   1),   S( 44,  20),

            /* rooks: bucket 7 */
            S(-93,  38),   S(-84,  41),   S(-58,  35),   S(-52,  28),   S(-22,  -9),   S(-28, -11),   S(-49,  10),   S(-49, -27),
            S(-73,  23),   S(-19,  -9),   S(-46,  13),   S(-59,  22),   S(-27, -13),   S( 17, -34),   S( 10,  -2),   S(-19, -52),
            S(-80,  32),   S(-71,  21),   S(-20,   4),   S(-45,  18),   S(-30,  -6),   S(-23,   8),   S( 30, -23),   S(-31, -36),
            S(-73,  38),   S(-25,  20),   S( -9,  16),   S( 30,  -9),   S( 14,  -9),   S( 43, -18),   S( 25,  -3),   S( 26, -40),
            S( -5,  36),   S( 10,  25),   S( 69,   9),   S( 81, -13),   S(138, -39),   S( 84, -46),   S( 99, -35),   S(-94, -29),
            S( 15,  15),   S(  3,  15),   S(101,  -6),   S( 58, -10),   S( 53,  -2),   S( 16,  -3),   S( 25,  61),   S(-13, -39),
            S( 25, -21),   S( 55, -27),   S( 68, -21),   S(127, -56),   S(130, -51),   S(115, -48),   S( 43,   6),   S( 15, -32),
            S( 12, -38),   S(  5,   0),   S( 55,  -6),   S( 22,  18),   S( 44, -35),   S( 52,  -7),   S( 12,  20),   S( 45, -20),

            /* rooks: bucket 8 */
            S(-62, -68),   S(-15, -33),   S( 10,  -6),   S(  5,  -9),   S(-11, -48),   S(-15,  28),   S( -5, -36),   S(  0,  10),
            S(-42, -108),  S( -7, -30),   S(-31, -10),   S(-37, -68),   S(-29, -47),   S(-25, -21),   S(  3, -16),   S(-45, -44),
            S( -2,  -9),   S(  3, -11),   S( 18, -15),   S(-26,  10),   S(-17,  25),   S(  3,  -2),   S( -4,  62),   S(-24, -22),
            S( -5, -21),   S( -1,   9),   S(  6,  21),   S( 18,  48),   S( 11,  63),   S( 33,  33),   S( 14,  47),   S(-12,   0),
            S(-17, -56),   S( -1,   1),   S(  7,  29),   S( 17,  49),   S( 14,   9),   S( -6,  -1),   S( 13,  69),   S( -1,  44),
            S(-26,   7),   S(  3,   9),   S(-13,  12),   S( -5, -11),   S(  5,  45),   S(-33, -18),   S(  0,   3),   S(  7,  30),
            S(  5,  31),   S( -3,  17),   S( 10,   8),   S( 20,  37),   S( 21,  31),   S( 17,  42),   S( 17,  51),   S(  6,  42),
            S( 12,  37),   S( 11,  25),   S(-25,  54),   S( 40,  73),   S(-21,  26),   S( 13,  41),   S( 27,  46),   S( 22,  49),

            /* rooks: bucket 9 */
            S(-27, -64),   S(-10, -70),   S( 19, -127),  S(  8, -36),   S(-15, -30),   S(  7, -40),   S( 14, -27),   S( 15,  -2),
            S(-55, -27),   S(-45, -66),   S(-38, -65),   S(-43, -41),   S(-46, -29),   S(-45,   5),   S(-18, -39),   S(-24, -20),
            S( -9,  -1),   S(-14,  -7),   S( 12,  -1),   S(-15, -36),   S(  7, -24),   S( 11,   1),   S(  4,  29),   S(  7,  37),
            S(  6,  16),   S(  9,   3),   S(  0,  -1),   S( -4,   4),   S(-16, -42),   S( 24,  18),   S( -7,  13),   S( 13,   4),
            S( -1,   1),   S( -5,  -3),   S(  8, -33),   S(-18, -11),   S(-19, -29),   S(-13,  -6),   S( -6,  -6),   S( -9,  -6),
            S(-11,   8),   S(-41,  -5),   S(-26, -17),   S(  3,  33),   S(-14,  12),   S(-12,   3),   S(  7,   1),   S(-24,  -4),
            S(  5,  37),   S(  2,   5),   S( 15, -37),   S(-25,  23),   S( 20, -13),   S( 27, -12),   S( 21,  11),   S(-11, -29),
            S(-28,  -4),   S(-11,  38),   S( -7,   1),   S( -9,  33),   S(  4,  29),   S(  6,  55),   S(  4,  10),   S(  8,  16),

            /* rooks: bucket 10 */
            S(-24, -22),   S(-38,   7),   S( -7, -37),   S( 22, -56),   S(-18, -35),   S(-28, -62),   S(-16, -43),   S(-22, -56),
            S(-59,   8),   S(-31, -35),   S(-32, -22),   S(-60, -58),   S(-53, -59),   S(-19, -44),   S(-25, -63),   S(-35, -47),
            S( 10, -31),   S(-27, -27),   S(-57,   2),   S(-40, -34),   S(-11,  -6),   S( 16,  17),   S( 19, -28),   S(-20, -11),
            S(-25, -13),   S(-38, -23),   S( -9, -28),   S( 12,  29),   S( -5, -14),   S(  9,  21),   S(  7, -25),   S( 14, -46),
            S(-23, -24),   S( -1,  11),   S(  2, -23),   S( -1, -58),   S( -4,  21),   S(  4,  13),   S(-10, -35),   S(-14, -31),
            S( -1,  14),   S( 21,  -1),   S( 10,  16),   S(-10, -24),   S(  1,   3),   S(  3,  -8),   S(-34, -21),   S(-13, -15),
            S(-18,   5),   S( 14,  -9),   S( 12, -20),   S( 15,  -8),   S( 29, -18),   S(-33,  -6),   S(-21, -22),   S( 15,   5),
            S( -7,  -4),   S( 16,  36),   S(  4,  23),   S( -6,  -1),   S(-16,  35),   S(-14,   6),   S(-37,  32),   S( 16,  17),

            /* rooks: bucket 11 */
            S(-57, -27),   S(-35, -12),   S(-29,   0),   S(-13,  10),   S(-38, -20),   S(-33,  -9),   S(-17, -58),   S(-29, -70),
            S( -2, -10),   S(-11, -16),   S(-55, -14),   S(-52,  -8),   S(-52, -11),   S(  6, -17),   S(-49, -68),   S(-35, -57),
            S(-26,  21),   S(-32,  25),   S( -8,  14),   S(-19,  17),   S( 25, -12),   S(-17,   7),   S( 10, -36),   S(-13,  10),
            S(-34,   7),   S( -8, -11),   S(-16,  14),   S( 10,   0),   S(  6,  -6),   S(-18, -27),   S(  6,  14),   S(  2,  -4),
            S(-20,  -2),   S(  9, -22),   S( -9,   5),   S( -2,  -4),   S( 20,  -4),   S(-10, -17),   S( 23,  58),   S(-13, -58),
            S(-13, -33),   S(  3, -24),   S( 20,   5),   S( 13, -23),   S( -8, -21),   S(  1,  27),   S(  1,   8),   S(-21,   0),
            S( -6,   0),   S(-29, -13),   S( -6,   9),   S( -1, -20),   S(  1, -32),   S(  3,  26),   S( -2,  25),   S(-26,  -6),
            S(  2,  14),   S( 27,  33),   S(  9,  30),   S(  6,  19),   S(-11,  33),   S(-25,  18),   S( 22,  17),   S(-23,  38),

            /* rooks: bucket 12 */
            S(-33, -88),   S(  7,  12),   S(-15, -35),   S(-18, -45),   S( -4, -11),   S( 13, -19),   S(-14, -26),   S( -7, -10),
            S(  1,  11),   S(  0,   2),   S(  8,  20),   S(  0,  -4),   S(  3,  -9),   S( 13,  -7),   S( -3,  -1),   S(-13, -18),
            S( -8, -17),   S(  2,  36),   S( 12,  43),   S( 26,  27),   S( -5, -40),   S( 32,  54),   S(  4,  31),   S( -1,  36),
            S( -5,   6),   S( 12,  -2),   S(  6,  -8),   S(  4,  15),   S(  8,   4),   S(  3,  11),   S(  2,  10),   S( -4,   1),
            S(  0, -11),   S(  6,   8),   S( -4,  39),   S( -2, -19),   S(  2,  23),   S( 12,   7),   S( -5,   6),   S(  7,   9),
            S(  1,   2),   S( -8,   0),   S(  4,  21),   S( -2,  23),   S( 14,  34),   S(  0, -12),   S( 19,  42),   S(  4,  16),
            S(-23,   1),   S( -6,  28),   S( -2,   8),   S(  6,  43),   S(-12, -24),   S( 15,  23),   S(  7,  39),   S( -6,  18),
            S(  1,  15),   S( -8,  25),   S( -4,  27),   S( 12,  32),   S(  2,  14),   S(  2,  29),   S( -4,   6),   S(  1,  -8),

            /* rooks: bucket 13 */
            S(-26, -47),   S(-21, -53),   S(-23, -44),   S(-18, -33),   S(-46, -84),   S( -8, -26),   S(-23, -46),   S(-30, -69),
            S(-13,  -8),   S(-12, -32),   S(  2,   5),   S(  1,  -7),   S(  5,  24),   S(  1,  32),   S(  9,   5),   S(-20, -30),
            S(-22, -14),   S(-17,   4),   S(  0,  -6),   S(  6,   3),   S( 20,  24),   S(  6, -25),   S( 19,  58),   S( -9, -12),
            S( 10,  -6),   S( -8,   8),   S( -4,  -2),   S(  5,   7),   S(  7,   0),   S(  1,  11),   S(  2,   7),   S(  6,  17),
            S( 16,   8),   S(  7,  -2),   S(-15, -22),   S( -2, -24),   S( -1,  12),   S( 10,  17),   S(  7,  26),   S(  2,   6),
            S(  9,  25),   S( -1,   1),   S(-16,  -7),   S(-22, -26),   S( -8,   1),   S( 23,  42),   S(-11, -14),   S(  7,  24),
            S(  3,  -1),   S(  1,  -7),   S(-12, -37),   S( 15,  40),   S(-10,  14),   S( -1, -10),   S(  1,  -7),   S(-11, -16),
            S(  8,  11),   S(-12,   7),   S(-23, -18),   S( 23,  72),   S(  1,  13),   S(  8,  38),   S(  4,  35),   S(  4,   0),

            /* rooks: bucket 14 */
            S( 11, -14),   S(-24, -33),   S(-23, -39),   S(-14, -38),   S(-16, -49),   S(  4, -14),   S(-52, -69),   S( -9, -42),
            S(  2,  45),   S( -2,   5),   S( 12,  14),   S(  1, -10),   S( -5, -20),   S( -4,  -2),   S( -1,  -9),   S(-11, -13),
            S( 11,  37),   S(-10,  78),   S(  6,  -5),   S(  0,  -5),   S(  1,  -1),   S(  4,   9),   S( -5,  -4),   S(-32, -59),
            S( -6,   2),   S( 22,  31),   S(  0,  -3),   S( 19,  -4),   S( -2,  10),   S( -4, -19),   S(  3, -18),   S(-10,  -1),
            S(  6,  21),   S( 16,  23),   S( 13,  25),   S( 13,  51),   S( 12, -27),   S( 14,  25),   S( -7,  -5),   S(-13, -30),
            S( 12,  16),   S(  2,   3),   S( -1, -13),   S(-14, -22),   S(-16, -16),   S( -6,  31),   S(  2,  11),   S(  5,  17),
            S( -7,  -2),   S(  7,  13),   S(-21, -52),   S(-23, -30),   S(  5,  25),   S( 16,  30),   S(-21,  -1),   S(-10,   7),
            S(  6,   9),   S(  7,  16),   S( -5, -10),   S(  2, -17),   S( -9, -19),   S(-17, -42),   S( -9, -36),   S(-13,  27),

            /* rooks: bucket 15 */
            S(-16, -33),   S(-14, -28),   S(-44, -43),   S(-26, -50),   S( -3, -21),   S(-10, -51),   S( -3, -15),   S(-29, -61),
            S( 15,  36),   S(-15, -21),   S( -1,   9),   S(-16, -45),   S( -7, -35),   S( 11,  10),   S(  7,  12),   S(  5,  16),
            S( 11,  27),   S(  1, -11),   S( 16,  36),   S(  4,  -8),   S(  9, -17),   S(  0, -13),   S( 15,  52),   S(  7,  10),
            S(  8,  23),   S(  0,  -7),   S( 15,  29),   S(-17, -24),   S( 18,  39),   S(  3,  21),   S(  4,   9),   S(  2,  -7),
            S( -2, -33),   S( -2,  -6),   S(  5,   9),   S( -2,  14),   S(  8,  16),   S(  5,  -6),   S(  9,  55),   S(  0, -13),
            S(  9,  23),   S( 13,  20),   S( 11,  -1),   S(  9,  14),   S( -2,  -5),   S(-10,  14),   S( -2,  39),   S(  7,  -5),
            S(  3,  12),   S( -2,  10),   S(  6,   7),   S(  2,   2),   S(  0,  11),   S( 17,  35),   S( -9,   6),   S(  0,  12),
            S(  1,  24),   S( -1,  30),   S(  2,   7),   S(  0,  27),   S( -9,  -3),   S(  5,   3),   S(-15, -24),   S(-21, -44),

            /* queens: bucket 0 */
            S(-10,   2),   S(-14, -64),   S(-19, -58),   S( 10, -102),  S(  6, -70),   S( 25, -59),   S(-61, -35),   S(-56, -23),
            S(  9, -62),   S( 21, -92),   S( 18, -66),   S( 14, -54),   S( 14, -14),   S( -6, -22),   S(-28, -13),   S(-45, -13),
            S(  7,  -1),   S( 17, -45),   S( 39, -41),   S(  7,  -9),   S( -1,  21),   S( 16, -21),   S(-15,  -9),   S(-77, -83),
            S(-19,  50),   S( 29, -32),   S( -3,  29),   S(  1,  57),   S(  6,  81),   S(-13,  53),   S(-40,  51),   S(-17,  -8),
            S(-35, -23),   S(  9,  64),   S(  8,  26),   S( 25,  40),   S( -3,  59),   S(-28,  76),   S(-63,  81),   S(-33,  25),
            S(-31,  -3),   S( 21,  46),   S( 27,  23),   S(-30,  83),   S(-36,  92),   S(-60, 106),   S(-81,  28),   S(-53, -15),
            S(  0,   0),   S(  0,   0),   S( 27, -23),   S(-54,  29),   S(-20,  22),   S(-67,  67),   S(-96,  58),   S(-95,  12),
            S(  0,   0),   S(  0,   0),   S( 26,  -5),   S(  7, -27),   S(-46,  38),   S(-61,  26),   S(-64,  28),   S(-82, -10),

            /* queens: bucket 1 */
            S(  7, -28),   S(  3,   2),   S( 15, -51),   S( 36, -84),   S( 32, -18),   S( -2, -24),   S( 15, -23),   S( -2,  29),
            S(-39,  43),   S( 18,  55),   S( 45, -54),   S( 31,  30),   S( 44,  27),   S( -5,  23),   S(-16,  48),   S(-35,  23),
            S( 61,   3),   S( 27,   1),   S( 26,  26),   S(  9,  85),   S( -5,  96),   S( 18,  83),   S(  6,  53),   S( 10,  13),
            S( 34,   4),   S( 21,  33),   S( 20,  59),   S( 33,  72),   S( 34,  78),   S( 18,  50),   S( 26,  45),   S(-13,  71),
            S( 39, -14),   S( 38,  25),   S( 58,  38),   S( 31,  15),   S( 45,  76),   S( 74,  13),   S(-12,  86),   S( -5,  98),
            S( 50,   1),   S(110,   6),   S( 62,  55),   S( 79,  54),   S( 66,  29),   S(  5,  80),   S( 32,  65),   S(  1,  32),
            S( 93, -17),   S( 43, -29),   S(  0,   0),   S(  0,   0),   S(  9,  39),   S(  0, -11),   S( -9,  35),   S(-43,  15),
            S( 89, -31),   S( 53, -13),   S(  0,   0),   S(  0,   0),   S( 29,  24),   S( 31,  16),   S( 87, -20),   S( -5,  27),

            /* queens: bucket 2 */
            S( 10,  -1),   S(  8,  46),   S( 20,  47),   S( 40,  -9),   S( 36, -31),   S( 38, -26),   S( -6, -44),   S( 38,  22),
            S( -1,  15),   S( 10,  52),   S( 33,  44),   S( 40,  27),   S( 52,  13),   S( 10,  35),   S( 25,   7),   S( -8,  78),
            S( 28,  30),   S( 26,  29),   S( 28, 102),   S( 11,  93),   S( 24,  84),   S( 19,  69),   S( 35,  46),   S( 31,  52),
            S(  3,  67),   S( 36,  94),   S(  7, 107),   S(  9, 132),   S( 36,  87),   S( 28,  93),   S( 27,  90),   S( 33,  83),
            S(-16,  95),   S(-18,  62),   S( -6, 119),   S( 27,  99),   S( 39, 106),   S( 86,  27),   S( 70,  39),   S( 66,  41),
            S(-34,  94),   S(-28,  65),   S( 10,  75),   S( 82,  32),   S( 33,  37),   S(103,  64),   S(128,  40),   S( 23, 104),
            S( 10,  42),   S(-11,  52),   S(-10,  59),   S( 66,  -7),   S(  0,   0),   S(  0,   0),   S(  7,  84),   S( 20,  65),
            S(-30,  14),   S( 52, -31),   S( 53, -33),   S( 26,  15),   S(  0,   0),   S(  0,   0),   S( 69,  26),   S(  9,  59),

            /* queens: bucket 3 */
            S(-63,  32),   S(-40,  41),   S(-30,  51),   S(-12,  56),   S(-27,  35),   S(-23,  -2),   S(-28, -30),   S(-49,   8),
            S(-57,  34),   S(-39,  57),   S(-22,  61),   S(-17,  79),   S(-15,  82),   S(-20,  38),   S( 11,  -5),   S( 11, -37),
            S(-62,  92),   S(-35,  91),   S(-31, 117),   S(-43, 151),   S(-33, 124),   S(-20,  90),   S( -7,  58),   S(-20,  24),
            S(-41,  76),   S(-56, 132),   S(-55, 176),   S(-32, 168),   S(-36, 171),   S(-20, 109),   S( -4,  84),   S(-17,  47),
            S(-55, 121),   S(-50, 144),   S(-55, 174),   S(-49, 201),   S(-21, 155),   S(-21, 154),   S(-22, 124),   S(-17,  63),
            S(-79, 131),   S(-67, 147),   S(-67, 175),   S(-49, 179),   S(-41, 150),   S( 10,  97),   S(-18, 113),   S(-37, 106),
            S(-88, 105),   S(-93, 152),   S(-87, 180),   S(-63, 145),   S(-87, 152),   S(  5,  51),   S(  0,   0),   S(  0,   0),
            S(-161, 153),  S(-89, 107),   S(-57,  89),   S(-86, 123),   S(-58,  99),   S(-21,  32),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-32,  16),   S(-46, -22),   S( -8,  -8),   S( -5,   1),   S( -5, -16),   S( -2,  11),   S(-59, -34),   S( 20,  26),
            S( 12,   4),   S(-23,  -6),   S( -4,  15),   S(-48,   5),   S(-27,  23),   S( 31,  42),   S(-42, -19),   S( -2, -23),
            S( 17,  28),   S( 13, -43),   S(  2,   7),   S( 36,  -1),   S( 40,   0),   S( 24,  35),   S(-14, -21),   S( 32,  14),
            S(-34, -30),   S( 45,  12),   S( -9,   8),   S( 36,  29),   S( 42,   4),   S( -5,  64),   S(-34, -16),   S(  2,  22),
            S(  0,   0),   S(  0,   0),   S( 43,   3),   S( 78,  28),   S( 14,  35),   S( 39,  61),   S( -1,  10),   S(-10,  18),
            S(  0,   0),   S(  0,   0),   S( 20,  14),   S( 51,  51),   S( 62,  42),   S( 31,  57),   S( 16,  22),   S(  9,  10),
            S( 11,  -8),   S( 28,   8),   S( 69,  34),   S( 38,  18),   S( 49,  27),   S( 31,  49),   S( 25,  30),   S(  0,  21),
            S( 28,   5),   S(-14, -31),   S( 28,  -1),   S( 50,  32),   S( 15,   8),   S(  2,  -8),   S(-13,   9),   S(  3,  22),

            /* queens: bucket 5 */
            S( 36,  22),   S( 29,  31),   S( 16,  -7),   S(-27,   8),   S( 37, -23),   S( 30,  39),   S( 18,   8),   S( 27,  17),
            S( 22,  17),   S( 25,  14),   S( 31,   6),   S( -6,  25),   S( 15,  47),   S(  0,  -7),   S( 48,  23),   S( 22,  14),
            S( 32,   1),   S( 39, -18),   S(-21,  -8),   S( 10,  15),   S( 11,   4),   S( 46,   5),   S( 31,  40),   S(  7,  26),
            S( 13, -35),   S( 38,   1),   S( 10,  -8),   S( 37,  31),   S( 69,   0),   S(  5, -12),   S( 21,  41),   S(-31,  14),
            S( 28, -10),   S( 42, -37),   S(  0,   0),   S(  0,   0),   S( 19,  26),   S(  6,   1),   S( 29,  48),   S(  6,  35),
            S( 71,  29),   S( 43,  19),   S(  0,   0),   S(  0,   0),   S( 43,  39),   S( 63,  58),   S( 68,  47),   S( 41,  30),
            S( 42, -14),   S( 58,  15),   S( 53,  44),   S( 18,  14),   S( 49,  32),   S( 98,  19),   S( 59,  58),   S( 29,  21),
            S( 32,  25),   S( 47,   7),   S( 75,  28),   S( 33, -11),   S( 37,  18),   S( 70,  24),   S( 59,  51),   S( 71,  40),

            /* queens: bucket 6 */
            S( 42,  51),   S( 17,  -3),   S( 17,  52),   S( 21,  30),   S( 51,  13),   S( 12,  13),   S( 15,   8),   S( 12,  14),
            S( 19,  29),   S( 41,  20),   S( 42,  54),   S( 44,  41),   S( 55,  21),   S( 36,   0),   S( -2,  16),   S( 30,  35),
            S( 10,  67),   S( 30,  58),   S( 20,  40),   S( 67,   9),   S( 63,   7),   S( 51,   2),   S( 27,  27),   S( 87,  56),
            S( 22,  20),   S(-23,  18),   S( 51,  30),   S( 99,  33),   S( 10,  -6),   S( 35,   4),   S( 79,  44),   S(113,  45),
            S( 37,  56),   S( 49,  52),   S( 43,  53),   S( 70,  39),   S(  0,   0),   S(  0,   0),   S( 69,  34),   S(105,  13),
            S( 35,  54),   S( 76,  59),   S( 66,  60),   S( 21,  -2),   S(  0,   0),   S(  0,   0),   S( 73,  45),   S(107,  18),
            S( 29,  40),   S(  3,  45),   S( 51,  41),   S( 68,  16),   S( 55,  42),   S( 56,  72),   S(119,  33),   S(153,  17),
            S( 47,  19),   S( 57,  40),   S( 94,  22),   S( 90,  48),   S( 85, -14),   S(100,  16),   S(107,  17),   S(110,  27),

            /* queens: bucket 7 */
            S(-19,  15),   S(-10, -45),   S(-29,  39),   S(-11,  36),   S(-12,  11),   S(-11,  10),   S( 10,  32),   S(-20, -16),
            S(-22,  26),   S(-44,   5),   S(-22,  68),   S(-22,  85),   S(-18,  46),   S( 31,  11),   S( 15,  22),   S( -8,   8),
            S(  3,  23),   S(-31,  48),   S( -4,  86),   S( 46,  27),   S( 22,  39),   S( 50,  -1),   S( 66, -49),   S( 51,  -7),
            S( -5,  55),   S(  2,  48),   S( 16,  79),   S( 50,  70),   S( 63,  54),   S( 72,  -9),   S( 88, -23),   S( 42,  -9),
            S( 14,  55),   S(-11,  47),   S( 31, 118),   S( 33,  90),   S( 79,  23),   S( 57, -10),   S(  0,   0),   S(  0,   0),
            S( 22,  31),   S(-14,  91),   S( -3,  93),   S(  7,  92),   S( 55,  32),   S(115,  60),   S(  0,   0),   S(  0,   0),
            S(-19,  68),   S(-35,  56),   S( 13,  27),   S( 45,  56),   S( 68,  22),   S( 81,  23),   S( 51,   5),   S( 83,  67),
            S( 23,  13),   S(  8,  46),   S( 39,  53),   S( 38,  17),   S( 58,  33),   S( 37,   3),   S(-13,  -6),   S( 75,  -2),

            /* queens: bucket 8 */
            S( -4, -26),   S(  5, -24),   S(-23, -55),   S(  5,  -7),   S(-15, -21),   S(  5, -20),   S( -5, -20),   S(  0,   0),
            S(-22, -33),   S( -9, -19),   S( -1, -20),   S(  0,  10),   S( -1, -20),   S(  1,  -3),   S( -4,   4),   S( -4,  -9),
            S(  0,   0),   S(  0,   0),   S( -2, -30),   S( -8, -46),   S(  5,   6),   S( 11,  -9),   S( -4, -13),   S( -3,  -1),
            S(  0,   0),   S(  0,   0),   S( -1,   1),   S( -2, -22),   S(  0,   1),   S(  5,  -4),   S(  7,  28),   S(  0,  -2),
            S( -9, -18),   S( 11,  18),   S(-14, -44),   S( 15, -10),   S(  0, -21),   S(  6,  -4),   S( 11,   5),   S(  8,  11),
            S(  3, -13),   S(  5,  -3),   S( 16,  30),   S(  0, -31),   S( -9, -31),   S( 42,  57),   S( 21,  11),   S( -1,   4),
            S( -7, -13),   S( -7, -20),   S(  1,  -1),   S( 34,  62),   S( 19,  18),   S( 28,  53),   S(  9,  14),   S(  1,  -7),
            S( 18,  22),   S(  8,  13),   S( 13,  10),   S(  9,  -1),   S( 33,  41),   S(-26, -35),   S(  6,  16),   S(-28, -45),

            /* queens: bucket 9 */
            S( 19,   7),   S(-19, -39),   S(-11, -27),   S( 21,   7),   S( -4, -26),   S( -7, -21),   S(-17, -18),   S( -4, -28),
            S(  3,   9),   S( -4, -20),   S(-19, -40),   S(  1, -15),   S(-18, -47),   S(-16, -38),   S( 20,  18),   S( 12,   6),
            S(-22, -63),   S(-12, -20),   S(  0,   0),   S(  0,   0),   S( -9, -29),   S( 17,  11),   S(-11, -12),   S(  7,   4),
            S( 13,  10),   S(-26, -46),   S(  0,   0),   S(  0,   0),   S( -4,  -7),   S(  7,  -6),   S( 13,   7),   S(-16,   2),
            S(  8, -10),   S(  3,  -5),   S( 13,  11),   S( -9, -11),   S(-13, -44),   S( 10,  25),   S( -5, -16),   S(  3, -15),
            S( 20,  14),   S(  0, -26),   S(  9, -25),   S(  7,  -9),   S(  4,   6),   S( 10,  11),   S( -9, -25),   S(  3, -17),
            S( 17,  17),   S( 21,   9),   S( -5, -11),   S(  4,   6),   S( 28,  38),   S( 34,  34),   S( 14,  31),   S(  2, -19),
            S( 26,   7),   S( 33,   4),   S(-17, -11),   S( 19,  19),   S( 19,  17),   S(  2,  22),   S( 21,  -7),   S(  0,  -5),

            /* queens: bucket 10 */
            S( 13,  10),   S(  4,  -4),   S(  5,  -4),   S(-21, -48),   S( -4, -23),   S(-11, -29),   S( -7, -35),   S(  6,  -9),
            S( 11,   4),   S(-18, -23),   S(  5,  -5),   S(-29, -65),   S( -9, -13),   S( 10,  -8),   S(-13, -31),   S(-20, -33),
            S( -1,   0),   S( -4,  -5),   S(  1,   1),   S(-11, -21),   S(  0,   0),   S(  0,   0),   S( -1, -11),   S(-15, -24),
            S( -6, -20),   S(  6,  13),   S(  6,  -7),   S(  3,  -4),   S(  0,   0),   S(  0,   0),   S(-15, -24),   S(  1, -11),
            S(  9,  27),   S( 19,  24),   S( -6,  -8),   S( 37,  45),   S( 13,  24),   S(  4,   7),   S(  3, -19),   S( 29, -18),
            S( -2,  13),   S( 16,  28),   S( 26,  38),   S( 12,   2),   S(-15, -10),   S(  2,   5),   S( 26,  25),   S( -6, -11),
            S( 14,   1),   S( 17,  24),   S( 20,  41),   S( 36,  36),   S( 10,  15),   S( 21,   8),   S(  8, -11),   S( 14,  17),
            S( -5, -14),   S(  2,  13),   S( 36,  20),   S( -9, -14),   S(  8,   8),   S( 19,  26),   S(  9,  24),   S(  5,   2),

            /* queens: bucket 11 */
            S( -6,   2),   S( -7,   1),   S( -5, -20),   S(-13,  -2),   S(  2, -15),   S(-36, -48),   S(-12, -33),   S( -7, -16),
            S(  5,  12),   S( 21,  22),   S(-31, -19),   S(  9,  36),   S( 21,  11),   S(  2, -23),   S( -7, -26),   S( -8, -28),
            S( -3,  -3),   S(  0, -10),   S( -8,  24),   S( -6,  -1),   S(  5, -16),   S(-16, -27),   S(  0,   0),   S(  0,   0),
            S(  0,  14),   S(-12,  18),   S(-11, -17),   S(  7,  -2),   S(-11, -25),   S( -9,  -2),   S(  0,   0),   S(  0,   0),
            S(  6,  18),   S( 11,   8),   S( 11,  15),   S( 14,  38),   S( 43,  65),   S(  4,  17),   S(  4,  -4),   S( -8, -23),
            S(-14,  -6),   S( 19,  24),   S(  0,  21),   S( 13,  17),   S( 31,  44),   S(  5, -11),   S( -8,   0),   S( 21,   8),
            S( -4,   7),   S(-13, -22),   S( 27,  34),   S( 21,  38),   S( 26,  61),   S(  7,  -4),   S( -5, -10),   S(  7,   0),
            S(-27, -66),   S(  5,  10),   S( -5, -17),   S( -3,  25),   S(  5,  29),   S( 15,   4),   S(-20, -29),   S(  7, -15),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  4,   6),   S(-11, -12),   S(  0,   3),   S( -7, -12),   S( -2,  -9),   S( -2,  -5),
            S(  0,   0),   S(  0,   0),   S( 14,  10),   S( -7, -15),   S( -7,  -3),   S(-11, -29),   S(-12, -27),   S(  1,  -3),
            S( -6,  -9),   S( -1,  -7),   S( -9, -16),   S(-20, -33),   S( 21,  35),   S(  4,  30),   S(  2,  -2),   S( 12,  22),
            S( -5, -18),   S(  0,  -8),   S(  4,  12),   S( 15,  31),   S(  0,  -3),   S(  0,  13),   S(  6,  13),   S( -4, -11),
            S(-16, -31),   S(  4,  11),   S( 11,  21),   S(  2, -19),   S(  9,  31),   S(  5,   0),   S(-11, -22),   S( -2,   0),
            S( -4,  -8),   S( -5,  -2),   S( -2, -10),   S(  1,  -3),   S(  2,  15),   S(  1,  14),   S(-14, -12),   S( -3, -10),
            S(-19, -29),   S(  5,   8),   S( -4,  -7),   S( 13,  13),   S(  2,   9),   S( -8, -12),   S(  6,  15),   S( -7, -28),
            S( 13,  28),   S(  1,  -3),   S( -1,  -1),   S(  7,  16),   S( -4,  -4),   S(-12,   5),   S(-17,  -1),   S(-11, -26),

            /* queens: bucket 13 */
            S(-25, -47),   S(-11, -22),   S(  0,   0),   S(  0,   0),   S(-25, -35),   S(-12, -27),   S( 11,   4),   S(-12, -28),
            S(-16, -33),   S( -9, -17),   S(  0,   0),   S(  0,   0),   S(-12, -29),   S(-23, -46),   S( -9, -20),   S( -6,  -4),
            S(-20, -33),   S( -4, -10),   S( -1,  11),   S( 10,   1),   S(-33, -59),   S(  0,  -7),   S( -6, -11),   S( -3,  -7),
            S(-23, -41),   S(-15, -26),   S( -1, -27),   S(  0, -20),   S( 27,  16),   S( 33,  55),   S( -3,  -8),   S( -6,  -5),
            S(  8,  -2),   S(-16, -52),   S( -1, -15),   S( 17,  31),   S( -6,  -7),   S(  1, -16),   S(  0,  -5),   S(-12, -38),
            S( 19,  35),   S(-17, -20),   S(-10, -26),   S( 17,  22),   S( -1,  -6),   S(-11, -11),   S(-12, -25),   S( -6, -21),
            S(  3,   8),   S(  1,   7),   S( 17,  27),   S( -1,  -1),   S(  4,   8),   S(  2, -10),   S( -7, -10),   S(-12, -13),
            S(-17, -17),   S( -6, -11),   S( -5, -18),   S(  2,  -2),   S(  8,   8),   S(  3,   9),   S(  1,   3),   S( -7,  -9),

            /* queens: bucket 14 */
            S( -1,  -5),   S(  2,  -8),   S(-14, -22),   S(  0,  -8),   S(  0,   0),   S(  0,   0),   S( -8, -21),   S(-17, -32),
            S(-21, -46),   S(-25, -48),   S( -7, -26),   S( -7, -13),   S(  0,   0),   S(  0,   0),   S(-21, -39),   S(  3, -14),
            S(-11, -19),   S( -4, -29),   S( -5, -16),   S( -4,  -5),   S(  2,   8),   S(  4,   9),   S(-13, -14),   S( -5, -20),
            S(-12,  -7),   S(  0,   3),   S(  5,   7),   S(-26, -37),   S( -8, -24),   S(-12, -32),   S(  7, -17),   S(  1, -13),
            S( -8,  -2),   S(  9,  11),   S( -7, -15),   S(  5,   0),   S( 11,  30),   S(-20, -41),   S( -8,   1),   S(  0,  -6),
            S(  0,  -3),   S( -5, -11),   S( -1,   4),   S(-14, -25),   S(  7,   9),   S( -6, -14),   S( -3,  -4),   S( -9, -19),
            S( -6,  -3),   S( -1, -12),   S( -2,  -2),   S( -1,   3),   S(  7,  13),   S(  3,   3),   S( -5,  -8),   S( -9, -21),
            S(-15, -19),   S(  3,  -6),   S( -3,   4),   S(  2,   1),   S( 11,  18),   S( -6,  -5),   S( -8,  -7),   S(  6,   3),

            /* queens: bucket 15 */
            S(  1,   9),   S( -2, -10),   S( 13,   5),   S(-17, -26),   S(  7,  16),   S(-10,  -4),   S(  0,   0),   S(  0,   0),
            S( -2,  -1),   S(  0,  10),   S(-10, -13),   S( -2, -18),   S(  7,  16),   S(  0,   1),   S(  0,   0),   S(  0,   0),
            S( -2,  -1),   S( 10,  14),   S(-24, -17),   S(  4,   0),   S(  5,  -4),   S( -2,   1),   S( -2,   4),   S( -1,  -5),
            S( -1,  -3),   S(-16, -23),   S( -4,  -1),   S( 16,  35),   S( 10,  27),   S( -1,  12),   S(-17, -27),   S(  2, -11),
            S(  1,   4),   S(  2,   6),   S( -6,  -8),   S(  5,   2),   S( -1,  15),   S(  3, -12),   S( -2,   6),   S( -6, -12),
            S(  1,   0),   S(-13, -21),   S( -3, -16),   S(-13,  -5),   S( -7,  10),   S(-10,  -5),   S(  1,   7),   S( -1,  -9),
            S( -9, -20),   S( -5,  -1),   S(-12, -12),   S( -2,  -6),   S( -9,  -6),   S( -1,   6),   S(  3,   7),   S( -7, -13),
            S( -7, -18),   S(-12, -33),   S( -1,  -9),   S(  2,   1),   S(-19,   1),   S( -7,  -7),   S(  5,   4),   S(  1,  10),

            /* kings: bucket 0 */
            S( -4, -29),   S( 19,  -6),   S(  5,   5),   S(-28,   4),   S(-24,  21),   S( 11, -12),   S( -2,   7),   S(  7, -48),
            S(-24,  41),   S(  9,  -4),   S( -3,  12),   S(-47,  25),   S(-35,  39),   S( -2,   8),   S(-16,  37),   S(  1,  26),
            S( 14,   5),   S( 82, -26),   S( 20, -11),   S(-15,   2),   S(-16,   0),   S(  8,   1),   S(-44,  19),   S(  7, -18),
            S(-32, -33),   S( 11, -34),   S(-11, -20),   S(-39,  16),   S(-59,  33),   S(-41,  11),   S(-36,  34),   S( 12,  25),
            S(-61, -116),  S(-15, -45),   S(-12, -28),   S( 11, -12),   S(-47,   0),   S(-30,  13),   S(-16,  24),   S( -4,  -5),
            S( -2, -136),  S(  4,  17),   S( -9, -43),   S(-15, -17),   S( 11, -19),   S(-22,  31),   S( 16,   4),   S(-15,  10),
            S(  0,   0),   S(  0,   0),   S( -5, -50),   S(-21, -46),   S(-28, -12),   S( -2,  -7),   S(-29,  17),   S(-23, -20),
            S(  0,   0),   S(  0,   0),   S( -4,  17),   S(  1, -25),   S( 13,   3),   S(  2,  14),   S( 10,  16),   S(  1, -11),

            /* kings: bucket 1 */
            S( 19, -41),   S( 23, -17),   S(  8,  -5),   S(  5,   3),   S(-13,   8),   S( 25, -15),   S(  4,   7),   S( 24, -30),
            S( 20,  -3),   S( 11,  14),   S( 26, -20),   S(-41,  22),   S(-16,  14),   S(-14,  18),   S( 10,  12),   S( 13,  13),
            S(-11, -12),   S(  6, -24),   S( -1, -22),   S( -8,  -7),   S(-11,  -6),   S( 22, -25),   S( 47, -28),   S( 47, -24),
            S(  5,  -2),   S( -9, -10),   S( 36, -12),   S(  7,   5),   S( 24,  13),   S(-42,  11),   S( 40,  -1),   S(-31,  37),
            S(-25, -49),   S(-35, -35),   S(-34, -55),   S(-23, -45),   S( -9, -15),   S(  9, -35),   S(-14,  10),   S(-19, -13),
            S(-42,  17),   S(-105,  -1),  S(-29,  45),   S( 16,  30),   S(-28,   5),   S(-35,  19),   S( 18,  -6),   S(  0, -21),
            S(-27, -34),   S(-36,  -1),   S(  0,   0),   S(  0,   0),   S(-38,  23),   S(-54,  18),   S(-19,  34),   S( -3, -26),
            S(-34, -105),  S(-17, -18),   S(  0,   0),   S(  0,   0),   S(-17,   3),   S(-14,  36),   S(-18,   3),   S( -3, -50),

            /* kings: bucket 2 */
            S( 30, -57),   S(  2,   8),   S( 15, -21),   S( 18, -14),   S( -6,  12),   S( 31, -25),   S(  0,  17),   S( 22, -27),
            S( 46, -41),   S( -7,  21),   S(-10,   9),   S( -6,   3),   S(-20,  15),   S( -8,   2),   S( 22,  -8),   S( 16,  -2),
            S(-70,  -3),   S(-17,  -5),   S( -8, -16),   S(  0, -21),   S(  0,  -8),   S(  8, -21),   S( 46, -22),   S( 30, -25),
            S(  8,  13),   S(-18,  11),   S(  7,  -1),   S(-32,  14),   S( 39,  -4),   S(-27,  -3),   S( 25, -27),   S( 19,  -6),
            S( 10,   6),   S( 17,  -6),   S( 13, -41),   S(  0, -25),   S( 20, -42),   S(-10, -43),   S( 34, -44),   S(  0, -46),
            S(  8,   8),   S( -6, -29),   S(-13,   0),   S(-29, -19),   S(  2,  12),   S(-13,  20),   S(-85,  12),   S( -8, -27),
            S(-17, -39),   S(-10,  20),   S(-100,  -3),  S( -9,  11),   S(  0,   0),   S(  0,   0),   S(  1,  32),   S(-47, -60),
            S(-10, -35),   S(-14,  -4),   S(-24, -24),   S(  0,   5),   S(  0,   0),   S(  0,   0),   S(-15,  -3),   S(-34, -111),

            /* kings: bucket 3 */
            S(  9, -55),   S( 11, -10),   S( 23, -32),   S(-14,  -6),   S( -9, -12),   S( 29, -22),   S( -2,  15),   S(  5, -26),
            S( 14,  10),   S(-22,  40),   S(-19,  10),   S(-40,  22),   S(-48,  30),   S( -3,   1),   S( -1,  14),   S(  9,  12),
            S( 16, -20),   S(  5,  -8),   S(-18,   6),   S(-34,   3),   S(-15,  10),   S( 16, -18),   S( 53, -21),   S( 53, -17),
            S(-33,  32),   S(-113,  54),  S(-56,  13),   S(-41,   7),   S( -8,  -4),   S(-22, -18),   S(-49,  -1),   S(-46, -11),
            S(-18,  10),   S(-15,   8),   S(-17, -14),   S(-44,  -5),   S( 24, -43),   S( 68, -74),   S( 23, -55),   S(  5, -84),
            S( -5, -13),   S( 24,  -5),   S( 10,   0),   S( -3, -38),   S( 46, -42),   S( 73, -52),   S( 83, -29),   S( 41, -119),
            S( -7,  -6),   S( 28,  14),   S( 31, -13),   S( 36, -39),   S( 43, -37),   S( 38, -61),   S(  0,   0),   S(  0,   0),
            S( -7, -24),   S(  4,  -3),   S( -8,  42),   S( 19,   6),   S( 20, -68),   S( -5,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-57,   6),   S( -7,  32),   S( 10,  33),   S( 17,  -8),   S( 17,  -9),   S(  9,  -1),   S(  6,   3),   S( 41, -43),
            S(-37,  29),   S( 20,  22),   S(  2,  18),   S(-20,   6),   S( 67, -10),   S(  6,   6),   S( 54, -17),   S( 24,  -2),
            S( 11,  25),   S(  9, -16),   S( 20,  -3),   S( -7,   7),   S( -5,  14),   S(  7, -12),   S(-42,  11),   S( 20, -19),
            S(  3, -10),   S(-23,   1),   S(-12,  19),   S(  9,  11),   S(-34,   9),   S(-20,  22),   S( 22,   8),   S( 15,  -2),
            S(  0,   0),   S(  0,   0),   S( -8, -23),   S(-30,  14),   S(-34,  16),   S(-44,  -3),   S(-26,  -6),   S(  1,  19),
            S(  0,   0),   S(  0,   0),   S( -1, -25),   S(-18,  53),   S( -5,  19),   S(-17,  -6),   S( -1, -19),   S(  5,  -6),
            S( -6, -24),   S(  2,   3),   S(-11, -20),   S( 23,   8),   S( -6,   8),   S(-13,   7),   S(  0,   8),   S( -5, -16),
            S( -2, -13),   S(  3, -22),   S(-17, -34),   S( -6,  12),   S( -2,   7),   S( -1,   6),   S(-11,  17),   S(  5,   9),

            /* kings: bucket 5 */
            S( 21,  -4),   S(  0,  -6),   S(-36,  23),   S(-44,  19),   S(-28,  30),   S(  4,   8),   S( 39,  -6),   S( 22, -15),
            S(  9,   2),   S( 15,  18),   S( 39,  -5),   S(  5,   2),   S( -2,   4),   S( 55, -12),   S( 25,   7),   S( 62, -20),
            S(  5,   4),   S(-18,  -4),   S( 12, -12),   S(  6,  -8),   S( -3,   2),   S(-25,  -1),   S(-27,   8),   S( 14,   6),
            S(  9, -26),   S( -7, -10),   S(-12,  -3),   S( -6,  22),   S( -4,  19),   S( 14,   1),   S( 30,  -3),   S( 12,  -1),
            S( 10, -26),   S(-37, -56),   S(  0,   0),   S(  0,   0),   S(  5,   4),   S(-39,   0),   S( 30, -14),   S( -7,  -6),
            S(-12, -31),   S(-39, -47),   S(  0,   0),   S(  0,   0),   S(-25,  39),   S(-58,   8),   S(-34,  -1),   S(  3, -13),
            S(  0, -31),   S(-24,  16),   S( -9,  11),   S(-10, -12),   S(-21,  33),   S(-44,  23),   S(-11,  26),   S( 13,  47),
            S( -2, -101),  S(-12,  15),   S(-15, -31),   S(  2, -29),   S(-10, -22),   S( -8,  19),   S( -8, -18),   S( -4,  17),

            /* kings: bucket 6 */
            S( 41, -28),   S( 26, -22),   S( 15, -14),   S(-25,  23),   S(-11,  18),   S(-19,  21),   S(  4,  17),   S( 12,   5),
            S( 31, -13),   S( 11,  16),   S(  0,   3),   S( 12,  -2),   S( 17,  -4),   S(-11,   8),   S(  0,   8),   S(  6,   4),
            S(  8, -12),   S(-29,  -2),   S(-26, -10),   S(-26,   1),   S( 17, -13),   S(-30,   1),   S( -3,   1),   S( -8,   6),
            S( 29,  -3),   S( 45, -16),   S( 30, -17),   S( 45,   1),   S( 36,  10),   S(-22,  -1),   S(  9,   1),   S(  2,   6),
            S(  8, -13),   S( 15, -21),   S(-12, -16),   S( -7,  -9),   S(  0,   0),   S(  0,   0),   S(-46, -18),   S(-52, -19),
            S(-12, -10),   S( -1, -11),   S(-21, -11),   S(-24, -33),   S(  0,   0),   S(  0,   0),   S(-25, -14),   S(-22, -23),
            S( -2, -10),   S(-18,   9),   S(-34, -13),   S(-34,   8),   S( -6,   6),   S(-11, -15),   S(-28, -19),   S(  8, -40),
            S(  1,  -1),   S( -1,  -7),   S(-11,  28),   S(-14, -46),   S( -8, -48),   S(  4, -22),   S( -2,  -1),   S(  0, -66),

            /* kings: bucket 7 */
            S( 20, -15),   S( 13, -24),   S(-37,  -4),   S(-18,  16),   S( -2,   0),   S(-43,  39),   S(-24,  31),   S(-29,  18),
            S(  0,   4),   S( 32, -28),   S(-26,   7),   S(-27,   0),   S(-40,  18),   S(-39,  26),   S(  4,  -1),   S(  5,  12),
            S( 23, -33),   S(-29,  -4),   S(-30,  -6),   S(  8, -11),   S(-57,  13),   S(-49,  20),   S( 35, -10),   S(-72,  30),
            S(-16,  25),   S(  8,  14),   S(-29,  12),   S( 24,  -4),   S( 48,  -9),   S( 57, -36),   S(  2,  -3),   S( 39, -21),
            S(-15, -10),   S(  6,   8),   S( 10, -43),   S( 25, -24),   S( 19, -23),   S( 13, -38),   S(  0,   0),   S(  0,   0),
            S(  3, -39),   S(  2,  -5),   S( 17,  -4),   S( 23,  -9),   S( 30,  -5),   S( 10,  -8),   S(  0,   0),   S(  0,   0),
            S( 20,  29),   S(  7,   5),   S(  6,  23),   S(-16,  -8),   S( 15,  -6),   S(-18, -27),   S( 16,   7),   S(-19,  -8),
            S(  7,   5),   S(-17, -27),   S( 12,   8),   S( -5,  -8),   S( 12,  21),   S(-16, -43),   S(  0, -38),   S(-11, -64),

            /* kings: bucket 8 */
            S( 21, 116),   S( -9,  84),   S( 33,   8),   S( 17,  -5),   S(-21,   1),   S(-20,  -8),   S( 36, -22),   S(-26,  -7),
            S( 36,  67),   S( 28,  19),   S( 65,  58),   S( 92,   7),   S(  0,  31),   S( 11,  -2),   S( 14,  -3),   S(-12,  29),
            S(  0,   0),   S(  0,   0),   S( 29,  96),   S( 33, -12),   S( 32,   8),   S(-16,  -1),   S(-17,  26),   S( -6,   4),
            S(  0,   0),   S(  0,   0),   S(  6,  85),   S(  9,  -7),   S(-17,  41),   S(-10,  10),   S( 11,   2),   S( 16,  12),
            S( -4, -24),   S(  6,  20),   S( 15,  38),   S(  6,  -2),   S( -9, -13),   S(  3,  -1),   S(  6,   1),   S(-16, -37),
            S( -3,  21),   S(  1, -17),   S(  2, -19),   S(-15,   5),   S( -9,   7),   S(-12, -22),   S(-22,   9),   S( 21,  -2),
            S( -5, -14),   S(-20, -38),   S( 10,  19),   S(  4,  11),   S(  1, -25),   S(-23,   4),   S( -5, -28),   S( 10, -25),
            S( -4,  -4),   S(-15, -19),   S(-10, -32),   S(  2,  -7),   S(  2,  19),   S(-16, -20),   S(  6,   6),   S( -1, -17),

            /* kings: bucket 9 */
            S(  0,  19),   S( 17,  26),   S(-26,  59),   S( 32,   5),   S(-44,  40),   S(-13,  25),   S( 34,  11),   S( 54,  -7),
            S(-33,  35),   S( 40,  19),   S(  3,  -4),   S( 61, -10),   S( 65,  19),   S( 18,   5),   S(  4,  25),   S(-34,  24),
            S( -8,  10),   S( 44,   5),   S(  0,   0),   S(  0,   0),   S( 29,  20),   S(  0,  12),   S( 19,  -7),   S(-17,  22),
            S( -1, -20),   S( 21, -20),   S(  0,   0),   S(  0,   0),   S(  7,  33),   S( 29,   4),   S(-47,  31),   S(-32,  42),
            S(-12, -39),   S( 13,   7),   S(  3,  20),   S(  0,  14),   S(-11,  30),   S( -4,   2),   S(-33,  25),   S(  2, -13),
            S(  7,   9),   S(-16,   0),   S(  0, -15),   S( -4, -19),   S(-22,  -1),   S(-18,  26),   S(-10,  -6),   S(-11,  55),
            S( -8, -26),   S( -5, -52),   S( -7, -14),   S( -8, -33),   S(  6, -38),   S( 20,  22),   S(-12, -10),   S(  7, -42),
            S(  2,   4),   S( -3, -21),   S(  1, -42),   S(-10, -32),   S( -2, -33),   S(  2,  -4),   S(-13,  25),   S(  4,  15),

            /* kings: bucket 10 */
            S( 25,  -3),   S(  5, -16),   S(  8,   0),   S( 13,  29),   S(  2,  17),   S(-74,  42),   S(-22,  50),   S(-89,  84),
            S(-10,  -8),   S( 68,   3),   S(  0, -11),   S( 40,  11),   S( 52,  19),   S( 22,  -1),   S(  0,  30),   S(-101,  57),
            S( 19,   1),   S( 23,   6),   S( 43,  -5),   S( 15,  10),   S(  0,   0),   S(  0,   0),   S(-27,  30),   S(-46,  18),
            S( 13,  -3),   S( 45, -31),   S( 43, -32),   S( 19, -23),   S(  0,   0),   S(  0,   0),   S(  7,  22),   S( -4,   4),
            S(  0, -20),   S( 17,   6),   S( 42, -19),   S( 11, -54),   S( 12, -13),   S( -1,   8),   S( 24,   5),   S(-10,   7),
            S(  0,  19),   S( 11,  24),   S(-10,  18),   S( 13,  -2),   S(  6,  10),   S(-16,  -5),   S( -7,   2),   S( -8, -17),
            S(-11, -51),   S( 11,   0),   S(  1, -10),   S( 29,  29),   S(  4,   0),   S(-32, -28),   S(  3, -17),   S( 14,   5),
            S( -4, -12),   S( 13, -15),   S(  7,  -9),   S( -7,   1),   S(  2,   0),   S( -7, -24),   S(-10, -27),   S( 16,  13),

            /* kings: bucket 11 */
            S(-14,  -3),   S( 27,  18),   S( 34,   8),   S(-14,  22),   S( -4,   6),   S(-96,  56),   S(-78,  82),   S(-118, 150),
            S(  2, -52),   S(  3,   5),   S(-16, -24),   S( 11,  22),   S( 94,  -2),   S( 63,  46),   S( 30,  13),   S( 21,  44),
            S( 11, -81),   S(-15,  30),   S( -6,  10),   S( 11,  19),   S( 42,   9),   S( 38,  60),   S(  0,   0),   S(  0,   0),
            S( 16,  39),   S( 11,  27),   S(  0,  -7),   S(  2,  11),   S( 26, -13),   S( 39,  17),   S(  0,   0),   S(  0,   0),
            S( -4,  37),   S(  8, -21),   S( 19,  -9),   S( 22, -25),   S(  2,  10),   S(  7,   1),   S(  3,  -6),   S(  6,  -4),
            S( 18,  12),   S(-10,   2),   S( -2, -29),   S( 12,   2),   S( -4,   4),   S( -3, -13),   S(  5,  26),   S(-14, -16),
            S(  1,   1),   S( 10, -15),   S( 18,  13),   S( -3, -30),   S( 19, -34),   S(  9,   8),   S( -4, -25),   S(-14,  -4),
            S(  4,  12),   S(  7,  15),   S(-13, -29),   S( 10,  11),   S(-11, -34),   S( -5, -15),   S(  1, -14),   S(  7,  15),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 19,  42),   S(  9,   9),   S(  6,  -1),   S(  2,   7),   S( 33,  -9),   S(-11,  26),
            S(  0,   0),   S(  0,   0),   S( 49, 122),   S( 36, -17),   S( 22,  51),   S( 21,  -1),   S(-17,   7),   S(-23,  -7),
            S(  0,   2),   S(  0,  11),   S(  6,  47),   S( 31,  25),   S( 23,  12),   S(  2, -23),   S( -9, -30),   S(-10,  -5),
            S( -9,   8),   S( 12,  35),   S( -1,  12),   S(  1,  21),   S(-13,  -9),   S(  8,  29),   S(  5,  29),   S( -3,  -7),
            S( 12,  14),   S( 14,  33),   S( 11,   2),   S( 13,  75),   S(  1,  24),   S( -9,   0),   S(-12,  -1),   S(  0,  -4),
            S(  2,   2),   S( 16,  25),   S( -1,  11),   S( -8,   5),   S(  1,  11),   S( -2,  44),   S(-15,  -9),   S(  5,   2),
            S(  0,   5),   S( -2,   1),   S(  4,  13),   S(-12,   0),   S(  2,   9),   S( -4, -11),   S(  0,  17),   S( -1, -23),
            S(  1,  -2),   S(  1,  -3),   S( -1,   6),   S( -5,  -4),   S(  6,   6),   S(  1,  18),   S( -3, -18),   S( -3, -13),

            /* kings: bucket 13 */
            S( -4,  43),   S( 20,  37),   S(  0,   0),   S(  0,   0),   S( 47,  32),   S( 14,   4),   S( -7,  -8),   S(-17,  13),
            S(  9,  27),   S(  4,  -1),   S(  0,   0),   S(  0,   0),   S( 43, -17),   S( 34,  -7),   S(-52,  17),   S( -2, -15),
            S(  3,  32),   S( 11,  21),   S(  1, -19),   S( 15,  29),   S( 43, -13),   S( 31,  11),   S( -1,  19),   S( 23,  -7),
            S(-15, -16),   S( 12,  -4),   S( 16,  30),   S(  0,  20),   S( -2,  25),   S( 21,   0),   S( -2,   2),   S(-16, -30),
            S(  9,   2),   S( -5, -10),   S(  6,  59),   S( -1,  33),   S(-10,   9),   S(  5,  17),   S(-11, -22),   S( 12,  31),
            S( -2, -17),   S( -9,  34),   S( 13,  30),   S(  6,  -2),   S(-11, -11),   S(-15, -26),   S(-17,  21),   S(  5,   6),
            S( 11,   7),   S( -4, -13),   S(-12, -39),   S(  8,  33),   S(-11,  -4),   S( -1,  17),   S(-19, -38),   S(  5,  32),
            S(  3,  -1),   S(  4,  -8),   S(  5,   4),   S( -1,   2),   S( -7,   3),   S(-17, -32),   S(  1,   1),   S(  3,  10),

            /* kings: bucket 14 */
            S(  8,  28),   S( -5,  12),   S(  7, -36),   S( 16,  -4),   S(  0,   0),   S(  0,   0),   S( 10,  91),   S(-52,  28),
            S( -8, -19),   S( 19, -16),   S( 55, -54),   S( 44,  16),   S(  0,   0),   S(  0,   0),   S( 16,  49),   S(-69,   1),
            S( -2,  -1),   S( 15, -10),   S( 25, -32),   S( 49,   5),   S( 13, -11),   S(  5,  26),   S( 27,  59),   S(-34, -17),
            S( -1, -20),   S( 10,   2),   S( -3, -11),   S( 11, -17),   S(-22,   0),   S( 25,  62),   S( -1,   6),   S( 14,   6),
            S( 13,  24),   S( 13, -24),   S( -5,  16),   S(-13,   6),   S( 10,  10),   S( 22,  38),   S( -4,  50),   S( 12,   5),
            S(  0,  -4),   S(  9,  25),   S( -7,  -8),   S( -2,  39),   S(  3,  -9),   S( -4,  12),   S(-18,   9),   S( -2,  11),
            S(  2,  18),   S(-11, -37),   S( 14,  25),   S( 17,   5),   S(  3,  -2),   S( -6,  21),   S(-23, -12),   S( 21,  48),
            S(  2,   9),   S(  1, -12),   S(  6,  -2),   S( -2, -12),   S( 12,   0),   S( -9, -19),   S(-15, -32),   S(  1, -11),

            /* kings: bucket 15 */
            S( 16,  28),   S(  3,   4),   S( 28,  15),   S( -2,  -7),   S( -5, -13),   S(  5,  47),   S(  0,   0),   S(  0,   0),
            S( 14, -17),   S(  8, -19),   S(-17, -24),   S( 33,  50),   S( 27, -10),   S( 67, 117),   S(  0,   0),   S(  0,   0),
            S( -3, -21),   S(  3,  -8),   S( 15, -31),   S( -2,  -3),   S(-10,  -8),   S( 23,  85),   S(  7,  46),   S( -5,  13),
            S( 12,  -9),   S( -1,  20),   S(  3,  -6),   S( -6, -35),   S(-12,   8),   S( 22,  39),   S( 22,  59),   S(-10, -15),
            S( 11,  26),   S( -8,  26),   S(  5,  28),   S( -1, -27),   S( -9,  -9),   S( -2,  69),   S( -3, -12),   S(  5,  19),
            S(  0,  37),   S(-13, -25),   S( -1,   4),   S( 10,   9),   S(-16, -57),   S(  4,  22),   S(  2, -20),   S(  6,  41),
            S(  6,  26),   S( -3,  26),   S(  1,  10),   S(  5,  -7),   S(  1, -11),   S(  4,   1),   S( -5, -14),   S(  0,  -4),
            S( -8, -20),   S( 12,   5),   S( -2, -17),   S(  6,   8),   S(  6,  11),   S(  5,   5),   S(  1,   8),   S(  3,  10),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-30,  42),   S(  5,   8),   S( 15,  82),   S( 23, 120),   S( 31, 137),   S( 34, 158),   S( 39, 156),   S( 51, 143),
            S( 60, 120),

            /* bishop mobility */
            S( -6,  31),   S(  4,  44),   S( 15,  77),   S( 22, 110),   S( 26, 137),   S( 28, 154),   S( 32, 162),   S( 36, 160),
            S( 39, 162),   S( 50, 152),   S( 59, 142),   S( 82, 129),   S( 92, 120),   S( 83, 122),

            /* rook mobility */
            S( -9,   8),   S( -3, 112),   S( -6, 151),   S( -3, 161),   S( -5, 183),   S( -1, 192),   S(  2, 200),   S(  5, 202),
            S(  5, 210),   S(  9, 214),   S( 14, 216),   S( 25, 212),   S( 29, 214),   S( 40, 214),   S( 74, 192),

            /* queen mobility */
            S( 50, 121),   S( 53, 261),   S( 55, 186),   S( 53, 199),   S( 56, 234),   S( 55, 270),   S( 56, 297),   S( 56, 321),
            S( 57, 340),   S( 58, 352),   S( 56, 371),   S( 53, 386),   S( 54, 394),   S( 53, 399),   S( 55, 400),   S( 59, 397),
            S( 66, 387),   S( 74, 373),   S( 90, 354),   S(117, 328),   S(129, 307),   S(176, 261),   S(153, 268),   S(179, 229),
            S(206, 218),   S(211, 178),   S( 96, 164),   S(101, 122),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  17),   S(-23,  47),   S(-33,  44),   S(-36,  68),   S( 17,  19),   S(-17,  18),   S(  3,  60),   S( 28,  27),
            S( 16,  32),   S( -1,  45),   S(-15,  48),   S(-16,  42),   S(  2,  35),   S(-30,  47),   S(-39,  68),   S( 22,  33),
            S( 22,  69),   S( 15,  70),   S(  4,  60),   S( 22,  47),   S( -2,  56),   S(-39,  73),   S(-45, 107),   S(-20,  85),
            S( 30, 109),   S( 42, 118),   S( 29,  80),   S(  8,  61),   S(  3,  68),   S(  7,  91),   S(-49, 135),   S(-76, 153),
            S( 22, 153),   S( 52, 184),   S( 63, 129),   S( 38, 114),   S(-63, 104),   S( 26, 111),   S(-50, 170),   S(-82, 178),
            S( 87, 234),   S( 78, 263),   S(114, 254),   S(119, 252),   S(139, 272),   S(155, 250),   S(130, 262),   S(131, 265),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,  -2),   S( -8, -24),   S( -3, -12),   S( -3,  -9),   S( 12,  14),   S(-12, -38),   S(-26,  10),   S( -4, -48),
            S(-21,  10),   S( 27, -20),   S( -6,  29),   S( 11,  24),   S( 33, -10),   S(-13,  21),   S( 23, -15),   S( -6,  -8),
            S(-17,  22),   S( 18,  -3),   S( -4,  52),   S( 17,  56),   S( 23,  25),   S( 28,  20),   S( 29,   0),   S(  0,  17),
            S(  0,  20),   S( 25,  64),   S( 28,  93),   S( 13, 105),   S( 63,  71),   S( 68,  68),   S( 19,  53),   S( 21,  24),
            S( 15,  74),   S( 86, 130),   S(125, 159),   S(133, 157),   S(147, 173),   S(118, 140),   S(129, 117),   S( 81,  97),
            S( 82, 203),   S(128, 284),   S(115, 241),   S(104, 213),   S( 65, 154),   S( 59, 152),   S( 47, 155),   S( 13,  87),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 12,  16),   S( 17,  22),   S( 35,  29),   S( 29,  22),   S( 19,  19),   S( 24,  27),   S(  3,  11),   S( 38,  -3),
            S( -6,  22),   S( 17,  34),   S( 12,  35),   S(  6,  43),   S( 25,  13),   S(  8,  20),   S( 30,  21),   S( -1,  14),
            S( -2,  24),   S( 27,  55),   S( 52,  65),   S( 40,  52),   S( 44,  56),   S( 69,  23),   S( 31,  36),   S( 17,  11),
            S( 67,  68),   S(104,  67),   S(117, 128),   S(150, 141),   S(136, 123),   S( 75, 136),   S( 63,  65),   S( 74,   8),
            S( 59, 116),   S(106, 136),   S(168, 204),   S( 84, 265),   S(114, 289),   S( 59, 239),   S(139, 210),   S(-75, 154),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 30,  31),   S( 14,  19),   S( 12,  37),   S( -9,  61),   S( 67,  34),   S( 21,  11),   S( -1,   5),   S( 27,  13),
            S(  3,  13),   S(  7,  10),   S( 17,  22),   S( 16,  33),   S( 13,  18),   S( -1,  11),   S(  7,   6),   S( 28,  -2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -3, -13),   S( -7, -10),   S(-17, -22),   S(-16, -33),   S(-13, -18),   S(  1, -11),   S( -7,  -6),   S(-28,   2),
            S(-30, -31),   S(-14, -19),   S(-12, -37),   S(  9, -61),   S(-67, -34),   S(-21, -11),   S(  1,  -5),   S(-27, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26, -39),   S(-14, -43),   S(-16, -47),   S(-64, -32),   S(-23, -49),   S(-30, -48),   S(-12, -46),   S(-22, -64),
            S(-27, -28),   S(-20, -30),   S(-36, -16),   S( -5, -42),   S(-39, -40),   S(-31, -25),   S(-42, -18),   S(-12, -45),
            S(-20, -19),   S( -2, -39),   S(-23, -14),   S(-32, -24),   S(-20, -48),   S(-21, -25),   S( -2, -24),   S(-41, -30),
            S( -6, -32),   S( 17, -43),   S( 11, -14),   S( 11, -39),   S( 14, -30),   S( 63, -42),   S( 41, -48),   S( -6, -60),
            S( 14, -47),   S( 57, -78),   S( 54, -30),   S( 58, -24),   S( 70, -45),   S( 71, -31),   S(116, -86),   S( 32, -83),
            S( 90, -87),   S(124, -100),  S( 77, -41),   S( 78, -28),   S( 99, -52),   S(121, -38),   S( 99, -40),   S( 32, -80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-13, -24),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  4,   3),        // attacks to squares 1 from king
            S( 11,   4),        // attacks to squares 2 from king

            /* castling available */
            S( 73, -92),        // king-side castling available
            S( 18,  49),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 41, -90),   S( 47, -83),   S( 41, -89),   S( 33, -74),   S( 33, -70),   S( 23, -61),   S(  7, -50),   S( -3, -40),
            S( 19, -49),   S( 37, -50),   S( 56, -46),   S( 42, -37),   S( 98, -55),

            /* orthogonal lines */
            S(-47, -143),  S(-100, -106), S(-119, -93),  S(-134, -91),  S(-138, -94),  S(-142, -94),  S(-141, -98),  S(-137, -101),
            S(-147, -95),  S(-160, -96),  S(-152, -113), S(-116, -137), S(-90, -145),  S( 13, -183),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 27, 231),

            /* passed pawn can advance */
            S(-10,  35),   S( -4,  63),   S(  4, 108),   S( 73, 175),

            /* blocked passed pawn */
            S(  0,   0),   S( 65, -25),   S( 40,  -5),   S( 33,  37),   S( 35,  65),   S( 29,  43),   S( 48,  69),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 53, -41),   S( 42,  16),   S( 23,  34),   S( 25,  57),   S( 42,  96),   S(122, 142),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-20,  -7),   S( -5, -34),   S( -2, -28),   S(-22, -10),   S(-37,  23),   S( 82,  19),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 39, -41),   S( 42, -33),   S(  6,   5),   S( 21, -53),   S(-10, -112),  S(-70, -211),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 14,  27),   S( 93,  13),   S(128,  38),   S( 28,  29),   S(175, 122),   S(104, 128),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 22,  56),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-34, 121),

            /* bad bishop pawn */
            S( -6, -15),

            /* rook on open file */
            S( 38,   5),

            /* rook on half-open file */
            S( 11,  40),

            /* pawn shields minor piece */
            S( 13,  11),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 24,  36),   S( 24,   2),   S( 35,  23),   S( 26,   5),   S( 31, -19),

            /* pawn threats */
            S(  0,   0),   S( 66, 108),   S( 54, 128),   S( 84,  83),   S( 57,  54),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  57),   S( 48,  49),   S( 74,  42),   S( 48,  70),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 21,  46),   S( 26,  41),   S(-14,  38),   S( 69,  61),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 20,  11),   S( 21,  34),   S( 40,  10),   S(  9,  31),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 16,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
