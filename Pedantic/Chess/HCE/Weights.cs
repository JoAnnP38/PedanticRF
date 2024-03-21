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
        public const int MAX_WEIGHTS = 12787;
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
        public const int TEMPO = 12786;             // tempo bonus for side moving

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

        // Solution sample size: 6000014, generated on Thu, 21 Mar 2024 02:57:04 GMT
        // Solution K: 0.003850, error: 0.084077, accuracy: 0.5063
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 76, 228),   S(382, 670),   S(413, 660),   S(534, 1085),  S(1370, 1816), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(101, -123),  S(155, -95),   S( 39, -35),   S( -4,   5),   S(-30,  17),   S(-17,  -5),   S(-47,   1),   S(-35, -17),
            S(134, -133),  S(112, -104),  S( 11, -62),   S(-18, -44),   S(-19, -16),   S(-15, -27),   S(-37, -22),   S(-26, -43),
            S(112, -102),  S( 74, -50),   S( 17, -61),   S( 16, -71),   S(  0, -62),   S( 13, -66),   S( -8, -62),   S(  5, -58),
            S( 69, -33),   S( 68, -70),   S( 39, -70),   S( 28, -76),   S( -8, -53),   S(-14, -56),   S(-16, -52),   S(-10, -34),
            S( 79,  32),   S( 43, -24),   S( 44, -10),   S( 74, -93),   S( 35, -64),   S(-14, -40),   S(-41,  11),   S(-31,  48),
            S( 84,  45),   S( 53,  52),   S( 15,  -2),   S( 45, -42),   S(-36,  12),   S( 44,  24),   S(  0,  13),   S( -2,  64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 31, -31),   S( 28, -35),   S( 55, -23),   S(  5,  22),   S( -8,  -3),   S(  8, -14),   S(-36,  -3),   S(-26,  16),
            S( 42, -44),   S( 20, -40),   S( 25, -48),   S(  2, -41),   S( -8, -18),   S( -2, -29),   S(-34, -15),   S(-31, -15),
            S( 27, -39),   S( 12, -27),   S( 20, -47),   S( 17, -55),   S(-16, -25),   S( 21, -48),   S( -6, -37),   S(  7, -31),
            S( 43, -17),   S( 31, -58),   S( 31, -57),   S( 18, -50),   S( -7, -28),   S(  5, -40),   S(-19, -35),   S( -4,  -1),
            S( 12,  62),   S(-45,  -4),   S(  2, -28),   S(  7, -24),   S( 58, -57),   S( -5,  -7),   S(-23,  21),   S(-14,  48),
            S( 79,  58),   S( 39, -20),   S(-50, -10),   S(-70,  46),   S(-10, -10),   S(-56, -27),   S(-76,  21),   S(-28,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16,  -2),   S(-22,   0),   S( -8,   6),   S( -1,   4),   S( 12,  -7),   S( 37, -20),   S(  8, -45),   S( -2, -20),
            S(  1, -26),   S(-29, -11),   S(-16, -36),   S(-17, -34),   S( 12, -31),   S( 12, -33),   S( -3, -41),   S(-13, -33),
            S( -8, -25),   S(-21, -26),   S( -8, -54),   S( -2, -54),   S(  1, -31),   S( 31, -43),   S(  0, -39),   S( 16, -33),
            S(-18,  -5),   S(-12, -48),   S(-17, -50),   S(  0, -58),   S( 14, -52),   S( -2, -24),   S( -1, -25),   S(  2,  -7),
            S( -7,  40),   S(-45, -10),   S(-40, -37),   S(-37, -46),   S( 13,  -4),   S( -6,   0),   S(-28,  20),   S(-23,  78),
            S(-55,  93),   S(-88,  66),   S(-79, -22),   S(-89, -19),   S(-15,  -8),   S(-27,  33),   S(-22,  21),   S(-42,  73),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -20),   S(-25,  -3),   S(-21,   4),   S(  8, -36),   S( -2,  -5),   S( 47, -18),   S( 96, -74),   S( 74, -87),
            S( -1, -45),   S(-25, -30),   S(-20, -40),   S(-14, -34),   S( -7, -24),   S( 24, -49),   S( 72, -78),   S( 70, -81),
            S( -2, -49),   S( -2, -60),   S( -2, -67),   S(  3, -69),   S( -1, -54),   S( 37, -59),   S( 42, -68),   S( 89, -80),
            S( -1, -33),   S(  3, -79),   S(  1, -80),   S( 15, -80),   S( 30, -84),   S( 20, -60),   S( 35, -54),   S( 67, -33),
            S( 25,   8),   S( -2, -49),   S( 15, -83),   S( 34, -88),   S( 81, -63),   S( 72, -44),   S( 79,  -7),   S( 62,  57),
            S(-31, 110),   S(-30,  28),   S(-14, -49),   S( 18, -84),   S( 56, -82),   S( 72, -20),   S( 87,  -3),   S( 70,  62),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-98,  25),   S(-25,  -6),   S(-50,  25),   S(-24,  -2),   S(  2, -25),   S(-51,  26),   S(-68,  16),   S(-53,   2),
            S(-35,  16),   S( 32,  -8),   S( 33, -37),   S( 20, -29),   S( -2, -27),   S(-57, -13),   S( 22, -51),   S(  4, -31),
            S( 10,   1),   S( 35,  -5),   S(-15,   9),   S( 10, -33),   S(-50, -29),   S(-16, -36),   S(-35, -31),   S( 21, -46),
            S(  9,  36),   S(-34,  42),   S( 37,   1),   S( -4,   5),   S( 13, -34),   S(-44, -20),   S(  1, -46),   S( 32, -23),
            S(-18,  89),   S(-23,  84),   S(-16,  36),   S( -5,  -9),   S(-18,  14),   S(-56,  30),   S(-45, -46),   S( 54,  17),
            S( 57,  69),   S( 39, 125),   S( -3,  16),   S(  0, -21),   S( -5, -12),   S(-11, -26),   S( 16,   1),   S(-17,  -7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-101,  56),  S(-69,  36),   S(  9,   8),   S(-16,  26),   S(-15,  31),   S(-18,  21),   S(-54,  14),   S(-30,  25),
            S(-59,  24),   S(-72,  24),   S(  4,  -3),   S(-10,  17),   S( 10,  -5),   S(-23, -10),   S(-22, -13),   S(-24,   4),
            S(-49,  35),   S(-61,  32),   S( 47, -29),   S( 14, -28),   S( 26, -17),   S(-26, -16),   S( -7, -16),   S( 12, -10),
            S(-55,  61),   S(-79,  48),   S(  6,   1),   S( 48,   4),   S(-14,  -2),   S(-44,   3),   S( 10, -19),   S( 21,   5),
            S( 64,  44),   S( 39,  39),   S( -2,  59),   S( 16,  40),   S( 17,  20),   S( 34,  -2),   S(  5,   2),   S( 21,  33),
            S( 73,  34),   S( 24,  35),   S( 34,   9),   S( 49, -22),   S( 58, -26),   S( -2, -11),   S(  2, -18),   S(  4,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42,  25),   S(-62,  26),   S(-39,  26),   S(-29,   5),   S( 40, -24),   S(-40,  15),   S(-69,   5),   S(-57,  21),
            S(-35,   4),   S(-11, -20),   S(-14, -30),   S( -7,  -6),   S( 27, -14),   S( 10, -22),   S(-36, -10),   S(-58,   3),
            S(-12,  -6),   S(-24,  -9),   S(-25, -20),   S(-38,  -3),   S( 17, -14),   S( 64, -40),   S(-11, -19),   S(-15,   3),
            S(-15,  13),   S(-90,  15),   S( 10, -34),   S(-29,   1),   S( 12,  -2),   S( 26,  -8),   S(  9, -10),   S( 44,  -2),
            S( 21,  12),   S(-72,  19),   S(  6, -25),   S(-28,   4),   S( 18,  26),   S( 67,  25),   S( 17,  11),   S( 74,  15),
            S( 58,  16),   S( 11,  -2),   S( -9, -46),   S( 18, -50),   S( 43,   3),   S( 23,  17),   S( 22,   2),   S( 69,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -28),   S(-57, -12),   S( -8, -16),   S(-71,  18),   S(-31, -10),   S( 10, -15),   S( -8, -44),   S(-47, -26),
            S(-33, -44),   S(-34, -44),   S(-44, -33),   S(-25, -44),   S(  2, -36),   S( 59, -57),   S( 57, -55),   S(-12, -38),
            S(-38, -42),   S(-49, -42),   S(-34, -56),   S( -9, -53),   S(-13, -26),   S( 39, -41),   S( 47, -58),   S( 58, -50),
            S(-25, -41),   S(-47, -57),   S(-77, -45),   S(-59, -18),   S(  9, -39),   S( 14, -16),   S( 31, -20),   S( 67, -26),
            S(-27, -15),   S( -3, -61),   S( -4, -52),   S( -1, -73),   S(  5,  -2),   S( 45, -10),   S( 86,  43),   S( 96,  34),
            S(-20,  -7),   S(-16, -43),   S(  1, -33),   S(  6, -61),   S( -5,  -8),   S(  3, -20),   S( 55,  38),   S( 79,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-62,  67),   S(-47,  79),   S( 38,  19),   S( -2,  15),   S( 15,   2),   S( 11,  10),   S(-55,   8),   S(-60,  27),
            S(-108,  81),  S(-35,  55),   S(-47,  23),   S(-16,  18),   S(  3,  -1),   S(-12, -25),   S(-39, -14),   S( -3,  -9),
            S(-71, 112),   S(-36, 118),   S( -8,  71),   S(-29,  55),   S( 20, -19),   S(-69, -15),   S(-66, -11),   S(-75,   4),
            S(-43, 153),   S( 38, 136),   S( 21, 112),   S( 26,  55),   S(-12,  15),   S(-31, -28),   S(-16, -23),   S(-30,   0),
            S(-13, 176),   S( 39, 164),   S( 23, 149),   S( 56, 103),   S( 17, -19),   S(  0,  18),   S(  1, -31),   S( -5,   7),
            S( 32, 215),   S( 76, 201),   S( 85, 212),   S( 50,  74),   S( -2,  11),   S(-16,  -6),   S(-14, -27),   S( -3,   6),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-77,  82),   S(-59,  53),   S(  5,   6),   S( 27,  53),   S( 13,  -5),   S(-35,  12),   S(-79,  17),   S(-61,  33),
            S(-61,  41),   S(-72,  34),   S(-60,  31),   S( 11,  53),   S(-42,  -6),   S(-26,  -8),   S(-98,   1),   S(-23,   4),
            S(-79,  67),   S(-113, 101),  S(-49,  95),   S(-94, 106),   S(-66,  54),   S(-106,  15),  S(-47, -19),   S(-36,   0),
            S(-98, 120),   S(-46, 125),   S( -6, 132),   S( 38, 139),   S(-24,  66),   S(-46,  21),   S( 17,  -3),   S(-39,  20),
            S( 33, 110),   S( 27, 139),   S( 13, 164),   S( 30, 191),   S( 22, 147),   S( -9,  41),   S( -2,  -8),   S( -3, -10),
            S( 21,  72),   S( 26, 139),   S( 79, 170),   S( 57, 214),   S( 20, 104),   S( -7, -15),   S( -4,  23),   S(-26,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-75,   9),   S(-52, -19),   S( -8,   8),   S( -5,   2),   S( -8,  21),   S(-66,  30),   S(-119,  23),  S(-72,  35),
            S(-84,   2),   S(-103,  11),  S( -8, -15),   S(  8, -21),   S(-31,  33),   S(-56,  28),   S(-119,  36),  S(-83,  23),
            S(-35,  -5),   S(-78,   5),   S(-16, -12),   S(-87,  71),   S(-95,  97),   S(-22,  48),   S(-115,  45),  S(-82,  40),
            S(-97,  36),   S(-110,  48),  S( -4,  13),   S(-54,  91),   S( 43,  94),   S(-25,  75),   S(-53,  55),   S( 28,  22),
            S(-17,  45),   S(-31,  27),   S( -2,  52),   S( 45, 111),   S( 88, 112),   S( 51,  77),   S( -5,  87),   S( 35,  48),
            S( -2,  10),   S( -4,  29),   S( 25,  50),   S( 57, 130),   S( -9, 145),   S( 41,  87),   S(  6,  76),   S( 23, 135),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47, -24),   S(-88,  23),   S( 45, -45),   S(  2,  13),   S( -7,   7),   S(-63,  43),   S(-53,  36),   S(-48,  30),
            S(-105, -14),  S(-90, -16),   S(-59, -14),   S(-69,  15),   S(-14,  13),   S(-26,  26),   S(-78,  65),   S(-137,  58),
            S(-51, -35),   S(-57, -36),   S(-13, -31),   S(-16,   0),   S(-62,  30),   S(-23,  66),   S(-86,  89),   S(-72,  76),
            S(-90,  13),   S(-75, -25),   S(-14, -33),   S(-91,  46),   S(-10,  49),   S( 14,  72),   S( 36, 114),   S( 91,  68),
            S(-34,  25),   S(-44,  11),   S(-10, -16),   S(-29,  24),   S( 41,  93),   S( -3, 117),   S( 90, 132),   S( 39, 121),
            S(-47,  27),   S(-18, -27),   S(  4, -43),   S(  2,  15),   S( 13,  57),   S( 30, 119),   S( 55, 191),   S( 45, 175),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23,  13),   S(-17,  20),   S(-25, -14),   S(  5,  18),   S(-12, -26),   S(-21,  -9),   S(-11, -24),   S(-11,  13),
            S(-37, -11),   S(-14,  28),   S( -4,  -8),   S(  7,  15),   S( -1,  -5),   S(  6, -18),   S(-17, -28),   S(-25, -50),
            S(-16,  31),   S(-20,  91),   S( 28,  35),   S( 12,  33),   S(-14,   2),   S(-39, -21),   S(-52, -59),   S(-32, -57),
            S(-34, 105),   S(-14, 130),   S( 40, 139),   S( 15, 107),   S(-36, -34),   S(-18, -74),   S(-14, -23),   S(-48, -39),
            S( 30, 100),   S( 36, 234),   S( 67, 131),   S( 20,  78),   S(  2,  -4),   S(  5, -24),   S( -3, -20),   S(-11, -73),
            S( 48, 133),   S( 70, 233),   S(107, 213),   S( 36,  96),   S(  2,   8),   S(-16, -17),   S( -4,   1),   S(-25, -34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -14),   S(-39,   2),   S(-13,   4),   S(  0,   8),   S( -2,  -1),   S(-42,  10),   S(-45, -46),   S(-25,   3),
            S(-59, -17),   S(-60,  33),   S(-36,  45),   S( 14,  38),   S(-51,  33),   S( -1, -29),   S(-75, -33),   S(-72,  20),
            S(-63,  50),   S(-60,  44),   S(-56,  67),   S(-20, 110),   S(-10,  54),   S(-18, -31),   S(-56, -36),   S(-104, -17),
            S(-89,  94),   S(  2, 139),   S( -1, 151),   S( -4,  94),   S( 11,  72),   S(-34,  16),   S(-26, -31),   S(-39, -18),
            S(  7, 116),   S( 50, 165),   S( 75, 202),   S( 53, 251),   S( 32, 146),   S(-29, -21),   S(  1, -50),   S(-27, -18),
            S( 30,  63),   S( 75, 181),   S( 84, 208),   S( 96, 290),   S( 41, 151),   S(  8,  43),   S( -6, -25),   S( -3,  -1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57, -52),   S(-30, -20),   S(-11, -18),   S( -4,  -8),   S(-10, -12),   S(-35,  -3),   S(-42, -13),   S(-20,  48),
            S(-46,   1),   S(-77,   6),   S(-55, -34),   S(  7,   1),   S(-47,  70),   S(  2,   6),   S(-52,  15),   S(-43,  30),
            S(-58, -24),   S(-54,   7),   S(-36, -22),   S(-18,  20),   S(-26,  85),   S(-41,  23),   S(-22,   4),   S(-50,  42),
            S(-46, -22),   S(-28,  52),   S(-24,  26),   S(-17,  74),   S( 13, 143),   S(-54,  85),   S(-25,  48),   S(-27,  56),
            S(-40, -46),   S( 12,   5),   S( 15,  79),   S( 29, 120),   S( 70, 223),   S( 53, 182),   S( 19,  92),   S( 35,  69),
            S( -2,  23),   S( 10,  22),   S( 32, 123),   S( 30, 131),   S( 62, 209),   S( 53, 121),   S( 34, 113),   S( 25,  44),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -32),   S(-21, -29),   S(-23, -64),   S(  1,   3),   S(  9,  23),   S( -3,  46),   S( -6,  -2),   S( 12,  16),
            S(-47, -23),   S(-18, -27),   S(-13, -56),   S( 42, -25),   S(-17,  23),   S(  6,  34),   S( 10,  18),   S( -5,   4),
            S(  1, -63),   S(-44, -71),   S(-11, -44),   S( 15, -14),   S( 27,  39),   S(-17,  79),   S( -9,  79),   S(-19,  91),
            S(-33, -67),   S(-51, -38),   S(-42, -23),   S(  4,  19),   S(-15,  75),   S( 11,  77),   S(-31, 144),   S(-28,  63),
            S(-34, -41),   S(-32, -29),   S(-27,   1),   S(  2,   7),   S( 47, 134),   S( 61, 161),   S( 64, 242),   S( 43,  72),
            S( -3,  41),   S( -1,   9),   S( -2,   5),   S( 10,  25),   S( 24,  75),   S( 91, 218),   S( 30, 194),   S( 25,  78),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-65, -22),   S(  3,  26),   S(-49,   1),   S(-25, -24),   S(-35,  -3),   S(-17, -41),   S(-58, -43),   S(-29, -27),
            S(-23,  45),   S( 42, -42),   S(-22,  -5),   S( 10, -14),   S( -3, -31),   S(-14, -31),   S(-43, -24),   S(-79, -12),
            S(  4,  71),   S( -5, -14),   S( 14, -18),   S(-20,  49),   S(  7,  30),   S(-26,  -2),   S( -3, -47),   S(-42, -53),
            S( 22, -24),   S( 25,  -2),   S( 22,  38),   S( 26,  34),   S(  8,  16),   S( -3,  15),   S( -1, -34),   S(  7, -13),
            S( -6, -19),   S( 48,  -1),   S(  4,  22),   S( 83, -16),   S( 35,  12),   S( 44,   7),   S( 21,  -4),   S(-78,  21),
            S(-17,  -9),   S( 19,   2),   S( 22,  29),   S( 33,   4),   S( 50, -50),   S( 37, -18),   S(-15, -10),   S(  4, -43),
            S(-15, -40),   S( 17, -59),   S( 28, -23),   S( 43, -30),   S( 10, -29),   S( -3, -67),   S(  4, -53),   S(-29, -24),
            S(-54, -28),   S(-19,  -6),   S( -5, -19),   S(-17, -68),   S(-20, -24),   S( 26,  26),   S( -7,   0),   S( 12,   0),

            /* knights: bucket 1 */
            S(-52,  -4),   S(-53,  85),   S(  1,  40),   S(-14,  56),   S(-11,  42),   S(-20,  19),   S(-30,  53),   S(-41, -17),
            S( 30,  11),   S( 10,  34),   S( -3,  19),   S( -5,  40),   S( -7,  22),   S(  9,  20),   S( 20, -25),   S(-15, -27),
            S(-30,  31),   S(  0,  26),   S( 13,  10),   S( 37,  21),   S( 11,  33),   S(-20,  32),   S(  1,  -2),   S(-23,  10),
            S( -6,  43),   S( 59,  28),   S( 40,  43),   S( 17,  44),   S( 14,  35),   S(  0,  33),   S( -6,   3),   S( -9,  38),
            S( -6,  42),   S( 29,  25),   S( 33,  21),   S( 34,  23),   S( 38,  34),   S( 23,  39),   S( 24,  32),   S( 24,  18),
            S( 23,   8),   S( 24, -10),   S( 50,  38),   S( 44,  11),   S(  1,  20),   S( 41,  27),   S( 35,  -6),   S( -2,  11),
            S( 29,  -2),   S( 17,   8),   S(-12, -26),   S( 55,  35),   S(  1,   7),   S( 61, -19),   S(-40, -12),   S(-13, -11),
            S(-103, -68),  S(-35, -26),   S(-13,  -7),   S( -6,  11),   S(-13,  -4),   S(-25, -33),   S(-13, -24),   S(-62, -48),

            /* knights: bucket 2 */
            S(-42,   5),   S(  1,  21),   S(-47,  47),   S(-28,  51),   S(-50,  59),   S(-40,  54),   S(-14,  45),   S(-40,  16),
            S(-15, -13),   S(-23,  21),   S(-10,  18),   S( -7,  23),   S( -2,  13),   S( -5,  42),   S(-25,  51),   S(-39,  96),
            S(-19,  23),   S( -4,   5),   S(-11,  40),   S( 25,  17),   S(  4,  36),   S(  6,   9),   S(  2,  35),   S(-31,  37),
            S( -9,  36),   S(-32,  40),   S( 11,  47),   S(  7,  48),   S(  3,  50),   S( -2,  54),   S( 13,  28),   S(-11,  32),
            S(  4,  37),   S(-10,  31),   S(-16,  52),   S(-24,  59),   S( -5,  58),   S(  1,  46),   S( -6,  49),   S( -8,  18),
            S(-30,  25),   S(-17,  33),   S(-10,  53),   S(-29,  52),   S(-39,  54),   S( -8,  57),   S(-31,  28),   S( 33, -22),
            S(-15,  23),   S(-32,   1),   S(-29,  29),   S(-34,  18),   S(-16,  -3),   S( -3,  24),   S(-50,  25),   S(-44, -11),
            S(-197,  27),  S( 19,   8),   S(-77,  36),   S( -9, -12),   S(  8,   0),   S(-79, -39),   S( 15, -36),   S(-215, -79),

            /* knights: bucket 3 */
            S(-46, -28),   S( 15, -34),   S(-39,   4),   S( -9,   7),   S(  2,  -4),   S( -2,   0),   S( 24, -15),   S(-33,  -5),
            S( -3,  -1),   S(-31,   6),   S(-10,  -9),   S( 18,   7),   S( 23,  -1),   S( -1, -12),   S(  1,   0),   S( -5,  44),
            S( -2, -24),   S(  7,  -5),   S(  7,   9),   S( 27,  16),   S( 28,  22),   S( 30,   1),   S( 21,   9),   S(  8,  38),
            S(  3,  -5),   S(  3,  21),   S( 27,  39),   S( 26,  39),   S( 37,  38),   S( 33,  39),   S( 46,  19),   S( 22,  17),
            S( 24,  14),   S( 17,  14),   S( 30,  17),   S( 34,  40),   S( 30,  49),   S( 35,  52),   S( 43,  42),   S( 18,  13),
            S( 23, -12),   S( 26, -15),   S( 48,   7),   S( 68,   3),   S( 61, -10),   S( 74,   1),   S( 38,   6),   S(  1,  42),
            S( 16,  -6),   S( 31, -17),   S( 55, -23),   S( 51, -10),   S( 53, -13),   S( 49, -20),   S( 56, -70),   S( 45, -13),
            S(-132, -10),  S(-10, -12),   S(-27,   1),   S( -2,  12),   S( 41,  -5),   S(-20,  -1),   S( -2, -15),   S(-94, -55),

            /* knights: bucket 4 */
            S(  8,  17),   S(-64,  10),   S(  4,  23),   S( 16,   0),   S(-26, -10),   S(-48,  -5),   S( -5, -47),   S(-36, -50),
            S( 56,  11),   S(-50,  10),   S( 13, -47),   S(  3,   6),   S( 10,  -7),   S(-17, -34),   S( 18, -48),   S(-30, -70),
            S(-16,  14),   S( 30,  44),   S( 26,   4),   S( 24,  10),   S( -1,  -1),   S(-43,  25),   S(-65, -41),   S(  4, -60),
            S(-10,  63),   S( 37, -17),   S( 27,  38),   S( 37,  13),   S( 10,   4),   S(103, -14),   S( 21, -22),   S( 10,  26),
            S( 38,  16),   S(-47,  68),   S( 72,  38),   S( 60,  24),   S( 23,  38),   S(-45,  34),   S( -1, -32),   S(  1, -14),
            S(  3,  41),   S(-34,  22),   S( 82,  -4),   S(  2, -20),   S( -2,  29),   S( 37,  24),   S( 10,  46),   S(-17, -45),
            S( -5,   4),   S( -5,   2),   S(-14, -36),   S(  9,  79),   S( 38,  56),   S( 22, -12),   S( 16,   7),   S(-22, -14),
            S( -9,   8),   S( -5, -18),   S( 12,  31),   S( -4,  19),   S(  1,  -5),   S( 20,  37),   S( -3,  10),   S(  3, -14),

            /* knights: bucket 5 */
            S( 26,   9),   S(-20,  51),   S( 38,  44),   S( 24,  37),   S( 24,  20),   S( 11,   3),   S(-14,  37),   S(-12,   6),
            S(  3, -12),   S( 57,  30),   S( 14,  29),   S(-24,  51),   S( 24,  37),   S( -6,  40),   S( 19,  33),   S( -1, -44),
            S( -9,  44),   S(-40,  49),   S( 43,  30),   S( 31,  41),   S(-42,  56),   S( -1,  35),   S(-10,  37),   S(-14, -20),
            S( 52,  35),   S( 40,  26),   S( 34,  41),   S( 15,  49),   S(-11,  56),   S( 10,  46),   S( 32,  52),   S( 14,  32),
            S( 35,  56),   S( 51,  23),   S( 35,  57),   S( 62,  49),   S( 71,  43),   S( 49,  24),   S( 49,  49),   S( 30,  27),
            S(  1,  32),   S(  0,  33),   S( 12,  36),   S( 36,  50),   S( 44,  40),   S( 34,  50),   S(  9,  14),   S( -6,  33),
            S( -3,  27),   S(-35,  77),   S( 14,  35),   S(  8,  44),   S( 19,  61),   S(  4,  57),   S( 12,  43),   S( -4, -30),
            S( 13,  18),   S( -1,   9),   S( 12,  46),   S(  0,  22),   S( 17,  29),   S(  4,  45),   S(  7,  44),   S(-35, -43),

            /* knights: bucket 6 */
            S(  6, -54),   S(-32,  -1),   S( 39,  14),   S(-30,  40),   S(-21,  37),   S(  1,  53),   S(-11,  22),   S(-11,   0),
            S( -8, -29),   S( 35,  10),   S(  5,  22),   S(-66,  50),   S(-49,  64),   S( 21,  50),   S( 24,  41),   S( 18,  23),
            S( 10, -22),   S( -5,  -5),   S(  0,  31),   S(  4,  42),   S( -4,  55),   S(-41,  55),   S(-12,  69),   S(-16,  46),
            S(  8,  25),   S( 61,   8),   S( 35,  44),   S( 75,  32),   S( 27,  39),   S( 12,  64),   S(  8,  62),   S( -3,  71),
            S( -9,  33),   S( 66,   1),   S( 66,  26),   S( 51,  29),   S( 80,  49),   S( 92,  35),   S( -4,  64),   S(  1,  38),
            S( 21,  14),   S( 21,   3),   S( 56,  26),   S( 62,  37),   S( 72,  47),   S( 70,  34),   S( 30,  30),   S( 46,  42),
            S(-22,  21),   S( -4,  50),   S(-24,  39),   S( 22,  29),   S(  4,  64),   S( 20,  50),   S(  7,  61),   S(-36, -19),
            S(-64, -46),   S( 15,  27),   S( 28,  36),   S(-13,  33),   S( 24,  16),   S(  0,  37),   S( 29,  72),   S( 15,  36),

            /* knights: bucket 7 */
            S(-22, -48),   S(-226, -45),  S(-54, -45),   S(-71, -19),   S(-35,  -5),   S(-53, -17),   S(-19,  -9),   S(-28, -17),
            S(-34, -109),  S(-61, -36),   S(-71, -26),   S(-15, -21),   S(-41,   2),   S( 15, -18),   S( 14,  49),   S(-11,   7),
            S(-74, -69),   S(-62, -23),   S(-46,   8),   S( 20,  -6),   S( -6,   6),   S( 27,  -6),   S(-15,  62),   S(  9,  45),
            S(-87, -23),   S( 31, -36),   S( -7,  21),   S( 57,  -8),   S( 64,  -2),   S( 12,  18),   S(  4,  23),   S(-24,  47),
            S(-53, -10),   S(-24, -24),   S( 54, -17),   S( 77, -22),   S(100,   1),   S( 45,  32),   S(113,  -6),   S( 75,   3),
            S( -7, -46),   S( -4, -41),   S( -8,  -1),   S( 51,  -3),   S( 87, -11),   S( 98,   1),   S( 47,  -5),   S( 15,  19),
            S(-40, -16),   S(-104, -29),  S(-10,  -7),   S(  9,   8),   S(  7,  33),   S( 35,   6),   S(-21,  35),   S( 20,  15),
            S(-52, -44),   S(-16, -22),   S(-37, -22),   S(  7,  -4),   S( 22,  15),   S( 11,  -2),   S( -8,  -1),   S(-17, -11),

            /* knights: bucket 8 */
            S( -1,  -5),   S( -5,  -1),   S(  7,  19),   S( -8, -17),   S(-11, -36),   S( -5, -49),   S( -2, -19),   S( -7, -21),
            S(  3,   2),   S( -3,   2),   S( -9, -16),   S(-28, -72),   S(-20, -10),   S(-26, -96),   S(-11, -50),   S(-11, -25),
            S(  0,  11),   S( -5, -35),   S(  5,   6),   S( -6, -10),   S( 11, -26),   S( -6, -11),   S( -9, -25),   S(-10, -38),
            S(-16,   3),   S( -5, -20),   S(  3,   4),   S(  2,  14),   S( 18,  17),   S(  6,   2),   S( -6, -43),   S( -4, -29),
            S( 41,  87),   S( -7, -23),   S(  6,  32),   S( 38,  18),   S(  1,   8),   S(-19, -21),   S( -4, -11),   S(  3,  11),
            S( 12,  52),   S(  8,  15),   S( 28,  17),   S( 33,  33),   S(  0,  12),   S( -9,  -3),   S( -8, -38),   S( -7,  -7),
            S(  9,   2),   S(  4,   5),   S( -4, -29),   S( 15,  12),   S(  7,  -3),   S(  5,   0),   S(  2,   8),   S( -5, -18),
            S( -1,  -1),   S(  8,  17),   S( -2,   2),   S( 12,  12),   S(  6,  28),   S( -4, -18),   S(  1,   4),   S(  0,   5),

            /* knights: bucket 9 */
            S( -9, -19),   S(-32, -60),   S(-10, -32),   S( -2, -11),   S(-23, -62),   S(-22, -55),   S( -8, -22),   S( -2, -31),
            S(-11, -32),   S(-12,  -3),   S(-19, -48),   S(-25, -25),   S(  1,  15),   S(-12, -39),   S( -7, -18),   S(-19, -42),
            S(  9,  24),   S(-17,  -6),   S( -9,  -3),   S( 15,   1),   S(  5,  23),   S(-37,  12),   S(-11, -19),   S(  7,   4),
            S(-13,   2),   S( -7,   6),   S( 15,   0),   S( 27,  40),   S( 29,   8),   S(  7,  20),   S(-23, -46),   S( -1,  15),
            S(  8,  15),   S(  3,  -6),   S( 10,  25),   S(  0,  42),   S( 16,  14),   S( 16,   1),   S(  7,   8),   S( -2,  -8),
            S( -6,  -4),   S( 15,  50),   S( 15,  18),   S(-22,  19),   S( 44,  65),   S( 10,  -4),   S( 13,  10),   S(-14, -30),
            S(  3,  -6),   S(  0,   7),   S( 26,  58),   S( -4, -23),   S( 10,  34),   S(-10, -33),   S(  1,   2),   S( -6,  -7),
            S( -1,  -8),   S(  6,  15),   S( 10,  22),   S( 22,  46),   S( -4, -34),   S(  2,  23),   S(  5,   4),   S( -1,   4),

            /* knights: bucket 10 */
            S(-14, -24),   S(-17, -73),   S(-24, -65),   S(-16, -31),   S( -2,   8),   S( -5, -50),   S( -3,  12),   S(  6,  31),
            S(-12, -39),   S(-10, -25),   S(  4,  13),   S(-12, -16),   S(-14, -27),   S( -8, -17),   S( -8,  -7),   S( -2,  -7),
            S( -8, -44),   S(-19, -36),   S(-10, -21),   S( -7, -12),   S(  6,  -4),   S(-18,  -5),   S(-22, -41),   S(-10,  -1),
            S( -9, -32),   S( -5, -45),   S( 11, -25),   S( 24,  20),   S(  1,  37),   S( 44,  43),   S(  8,  24),   S( 19,  51),
            S(-16, -58),   S(-20, -37),   S( 16,  24),   S( 32,  38),   S( 16,  57),   S(  1,  36),   S( 25,  20),   S( 26,  39),
            S(-10, -49),   S(-10, -22),   S( -3,  17),   S(  6,  43),   S( 13,  51),   S( 23,  71),   S( 29,  65),   S( 23,  57),
            S(  1,   8),   S( -8, -18),   S(  5,   8),   S( 25,  34),   S( 12,  16),   S(  7,  35),   S( 12,  35),   S(  9,  27),
            S( -5, -10),   S(  1,  -3),   S( -6,   0),   S(-10, -18),   S( 15,  55),   S(  4,  22),   S(  6,  18),   S( -2, -10),

            /* knights: bucket 11 */
            S( -1,  -4),   S(-25,  -7),   S(-13, -71),   S(-18, -52),   S(-28, -56),   S( -7, -21),   S( -8,  -6),   S( -6, -11),
            S(-16, -31),   S(-18, -31),   S(-20, -99),   S(-16, -21),   S(-24, -21),   S(-37, -41),   S(-16, -27),   S( -6, -16),
            S(-18, -80),   S(-19, -67),   S( -7,   4),   S(  9,  12),   S(-19,   9),   S(-26,  26),   S( 12,   4),   S(  2,  21),
            S(-22, -42),   S(-12, -40),   S(-31, -13),   S(  1,   5),   S( 25,  28),   S( 19,  16),   S( -2,  11),   S(  2,   1),
            S(-14, -37),   S(-14, -40),   S( 29, -12),   S(-13,  12),   S( 50,  32),   S( 49,  69),   S(-12, -12),   S( 23,  65),
            S(-15, -28),   S( -5, -26),   S( -3, -10),   S( 45,  52),   S(  7,  15),   S( 57,  30),   S(  6,  29),   S( 17,  47),
            S(  9,  27),   S( -3,  -6),   S(  2, -13),   S( -6, -35),   S( 14,  11),   S( -3,   7),   S( 20,  42),   S( 17,  61),
            S( -6, -11),   S( -2, -10),   S( 20,  36),   S(  1,  11),   S(  9,  30),   S( -5,  -5),   S(  8,  17),   S(  0,  15),

            /* knights: bucket 12 */
            S( -4, -17),   S( -2,   0),   S(  0, -22),   S( -2,  -5),   S( -6, -13),   S( -5, -14),   S( -2,  -9),   S( -2, -12),
            S( -1,  -2),   S(  1,   2),   S(  3,  10),   S(  2, -11),   S(  4,  27),   S( -2, -19),   S( -3, -23),   S( -3,  -2),
            S( -3, -16),   S(  4,   8),   S( -9, -24),   S( -6,  -6),   S(  0, -14),   S(  5,   1),   S(  2,   0),   S( -3, -18),
            S( -1,   4),   S(  0,  -7),   S( -8, -11),   S( 14,  31),   S( -1,   3),   S( -2,   9),   S( -5, -18),   S( -1, -11),
            S(  6,   5),   S(  3,  11),   S(  4, -12),   S( -2,  17),   S(  6, -16),   S(  3,  18),   S( -5, -22),   S(  2,   7),
            S( -5, -22),   S( -4, -21),   S(  9,   3),   S( 12,   3),   S(  1,   2),   S( -4, -14),   S(  5,  11),   S(  1,  11),
            S(  2,  10),   S(-14, -16),   S( -5,  17),   S(  6,  -8),   S( -2,   1),   S( -6, -18),   S( -1,   3),   S(  0,   0),
            S(  7,  12),   S(  7,  22),   S( -3,  -2),   S(  3,  13),   S( -4,  -9),   S( -3, -12),   S( -1,  -2),   S( -1,   2),

            /* knights: bucket 13 */
            S( -6, -18),   S( -2, -11),   S(  0,  -4),   S( -5, -18),   S( -8, -19),   S( -3, -18),   S( -6, -17),   S( -2,  -8),
            S( -4, -13),   S(  1,   4),   S( -1, -16),   S(-12, -14),   S( -9, -33),   S( -3, -15),   S( -2,  -3),   S(  4,  11),
            S( -4, -20),   S(-10, -50),   S(  6,  17),   S(  4,  25),   S( -7, -18),   S( -7, -31),   S( -5, -12),   S( -6, -27),
            S( -8,  -9),   S(  9,  27),   S( -1,  -4),   S( -7, -11),   S( -6, -16),   S(  5,   4),   S(  1,  -2),   S(-11, -26),
            S( -3,  -4),   S( -3,   1),   S( 12,  16),   S( 19,  35),   S(  1,   4),   S( -1, -19),   S( -2, -48),   S(  2,  -4),
            S(  1,   5),   S( 20,  23),   S( 10,  13),   S(-17, -11),   S(  2,   7),   S(-12, -58),   S(  1,   0),   S( -5,  -8),
            S(  4,  10),   S(  7,  19),   S(  9,  11),   S( -1,  11),   S( 18,  38),   S( -1,  -4),   S(  3,   9),   S( -1,  -2),
            S(  1,   4),   S(  2,   2),   S(  3,   5),   S( -2, -12),   S( -1,  -7),   S(  3,   3),   S( -1,  -6),   S(  1,   5),

            /* knights: bucket 14 */
            S(  0, -11),   S( -2, -24),   S( -3,  -2),   S(  0,  14),   S( -8, -23),   S( -3, -21),   S( -3,  -8),   S(  1,   5),
            S(  0,  -4),   S( -3,  -1),   S(-11, -47),   S( -3, -18),   S(  7,  17),   S(  0,   6),   S(  3,  11),   S(  0,   4),
            S(-10, -15),   S( -7, -26),   S(-13, -64),   S(  1,  -7),   S(  0, -21),   S( -3, -27),   S( -3, -10),   S(  0,   6),
            S(  5,  19),   S( -5, -24),   S( -7, -28),   S(-12, -33),   S( 14,  -4),   S( -5, -28),   S( -8, -22),   S( -5,   0),
            S(  0,   7),   S( -7, -39),   S(  8,  57),   S( -4, -30),   S(  1,   9),   S(  6,  32),   S(  4,  10),   S( -4,  -4),
            S( -2,  -2),   S(  4,   9),   S(-13, -43),   S( -3,  -3),   S( 11,  25),   S( -4,  17),   S( -2,   3),   S(  1,   5),
            S(  2,   1),   S( -4, -15),   S(  9, -12),   S( -2,  15),   S( -8,   6),   S(-10, -39),   S( -7,  -5),   S(  0,   6),
            S(  0,  -2),   S(  6,  11),   S( -3, -20),   S(  8,  13),   S(  4,  16),   S(  1,   2),   S( -7, -14),   S(  1,   1),

            /* knights: bucket 15 */
            S( -4, -19),   S( -5, -24),   S(  1, -15),   S( -9,  -9),   S( -4,  -5),   S(  3,   2),   S( -1,   2),   S(  0,   6),
            S( -2,  -9),   S( -2, -13),   S( -2,  -6),   S( -3, -12),   S(  3,  10),   S(  2,  10),   S(  0,   3),   S( -1,  -3),
            S(-11, -19),   S(  0,  -2),   S(-11, -30),   S(-11, -32),   S( -2, -12),   S( -3,   2),   S(  0,   5),   S( -3,  -2),
            S( -1,  -4),   S( -9, -36),   S(  8,   1),   S(  2,   6),   S(  1, -21),   S(  7,  17),   S(  1,  -1),   S( -3,  11),
            S( -4, -17),   S( -1,  -2),   S(  3,  -3),   S( -1,  -3),   S( -6,   8),   S( 10,  24),   S(-10, -16),   S(  1,   6),
            S(  2,   4),   S( -3, -12),   S( -5, -37),   S( -7, -11),   S( -8, -25),   S( -4,  14),   S( -3, -12),   S( -2,   3),
            S( -8, -21),   S( -5, -16),   S(  0, -13),   S(  1,   9),   S( -8,  -1),   S( -3,   8),   S( -4, -14),   S( -1,  10),
            S( -1,  -2),   S(  2,   0),   S( -5,  -9),   S( -5, -11),   S( -4,  -8),   S( -7,  -9),   S( -2,  -4),   S( -4,   3),

            /* bishops: bucket 0 */
            S( 42,  51),   S(  9,  -3),   S( 44,  18),   S(  8,  26),   S(  3, -12),   S( -7,  14),   S( -6, -20),   S( -2, -23),
            S( 31, -26),   S( 91,  11),   S( 34,   9),   S( 27, -19),   S(-11,  35),   S( 19, -21),   S(-21,   9),   S( 26, -60),
            S( -4,  80),   S( 47,  14),   S( 46,  37),   S(  9,  55),   S( 22,  20),   S(-17,  39),   S( 10, -20),   S( 11, -43),
            S( 11,  13),   S( 66,   0),   S( 39,  35),   S( 42,  29),   S( 12,  15),   S( 42,  -7),   S(  5, -17),   S( -4,  21),
            S(  1,   6),   S( 36,  10),   S( 32,  10),   S( 64,   2),   S( 70,  -1),   S( 14,   9),   S(  8,  -8),   S(-45,  22),
            S(-21,  43),   S( 23,  -8),   S( 64,   6),   S( 74, -30),   S( 55,  43),   S(-35,  38),   S(-21,  24),   S(  1, -14),
            S(  5,   9),   S( 24,  20),   S( 59,  -5),   S( -7,  41),   S(-35, -12),   S( 32,  -7),   S( 42,  -3),   S(-18, -31),
            S(-23, -29),   S( 31,  17),   S(-17,  -4),   S(  2,   0),   S( 14,  14),   S( 41,  30),   S( -2,  38),   S(-26,  21),

            /* bishops: bucket 1 */
            S( 16,  32),   S(-30,  40),   S( -6,  41),   S( 15,   7),   S( -2,  18),   S( -8,  25),   S(-15,   6),   S(-37,   8),
            S( 33, -30),   S( 42,  -1),   S( 39,  12),   S( 27,  20),   S( -2,   4),   S( 27,  -6),   S(-28,  24),   S(-22,  10),
            S( 38, -12),   S( 18,  11),   S( 53,  18),   S( 17,  28),   S( 23,  18),   S( -1,  20),   S( 33,  -4),   S( -2, -30),
            S( 47,   9),   S( -7,  25),   S( 15,  19),   S( 30,  22),   S(-18,  53),   S( 22,   9),   S(-16,  12),   S( 15, -13),
            S( 54,  14),   S( 19,  17),   S( 18,  31),   S(  0,  32),   S( 13,  21),   S( -2,  27),   S( 22, -10),   S(-18,  20),
            S( -7,  28),   S( 39,  14),   S( 27,  45),   S( 48,  11),   S( 18,  36),   S( 29,  27),   S( -8,  16),   S( 27,   3),
            S(-51,  68),   S(  6,  24),   S( -7,  43),   S( 61,   2),   S( 37,   1),   S(-13,  36),   S( 42, -21),   S(-48,  41),
            S( 11,  11),   S( 23,  -3),   S( 12,   8),   S(-34,  21),   S( 19,  28),   S( -9,   1),   S(  2,   1),   S(-25,   4),

            /* bishops: bucket 2 */
            S(  3,  -2),   S( 11,   7),   S( -1,  13),   S(-30,  46),   S(-17,  29),   S(-23,  18),   S( -3, -41),   S(-31,  28),
            S(-14,   4),   S( 16,   1),   S( 18,  11),   S(  3,  17),   S( -7,  32),   S( 30, -13),   S(  2, -12),   S( -3, -52),
            S( -7,  -9),   S( -8,   9),   S( 22,  20),   S( -1,  40),   S(  9,  30),   S( 10,  35),   S( 13,  16),   S(-21, -29),
            S( -1,   0),   S(-16,  22),   S(-10,  37),   S(  3,  35),   S( -2,  38),   S(  7,  16),   S(  7,  20),   S(  3, -15),
            S(  2,  -4),   S(-10,  18),   S(-15,  20),   S(-25,  35),   S(-29,  43),   S(-10,  35),   S( -1,  17),   S(-35,  20),
            S(  6,  19),   S( -4,  18),   S(  3,  39),   S(-32,  36),   S( 25,   3),   S(-13,  40),   S( 28,  41),   S(  6,   4),
            S(-18,  27),   S(-28,  34),   S(-44,  54),   S( 19, -12),   S( -6,  14),   S(-23,  12),   S(-52,  29),   S(-15,  19),
            S(-66,  69),   S(-49,  49),   S(-52,  42),   S(-29,  13),   S(-36,  11),   S(-74,  14),   S(  5,  22),   S(-89,  20),

            /* bishops: bucket 3 */
            S( 16,  -7),   S( 31,   2),   S( 21,  21),   S( 16,  10),   S( 15,  10),   S( 50, -13),   S( 46, -25),   S( 56, -67),
            S( 22,  -5),   S( 20,  15),   S( 35,  -7),   S( 14,  31),   S( 25,   7),   S( 29,  28),   S( 56,   9),   S( 26,   4),
            S( 26,  -4),   S(  9,  22),   S( 28,  37),   S( 29,  24),   S( 28,  52),   S( 37,  29),   S( 40,  31),   S( 45, -21),
            S( 36, -18),   S( 20,  13),   S( 24,  35),   S( 32,  45),   S( 27,  43),   S( 36,  25),   S( 24,  31),   S( 21, -10),
            S( 18,   1),   S( 34,   9),   S( 36,  15),   S( 35,  45),   S( 19,  42),   S( 42,  28),   S( 16,  31),   S(  9,  45),
            S( 29,   0),   S( 28,  27),   S( 42,  30),   S( 38,  12),   S( 33,  13),   S( 55,  23),   S( 50,   5),   S(  2,  63),
            S( 34,   4),   S( 17,  19),   S( 26,  24),   S(  9,  19),   S( -3,  25),   S( 18,   6),   S( 13,  45),   S( 16,  56),
            S(-35,  81),   S(  0,   2),   S( 79,   0),   S(  3,  44),   S( 11,  26),   S( 13,  38),   S( 48,   7),   S(101, -21),

            /* bishops: bucket 4 */
            S(-10, -13),   S(-35,  18),   S(-43,  21),   S(-51,  19),   S(-55,  42),   S(-61,  40),   S(-33,  -5),   S(-10,   4),
            S(-32, -18),   S( 31,  28),   S(-36,  61),   S(-20,  18),   S( -8,  -8),   S( 38,  -1),   S(-11,  14),   S(  3,  15),
            S(  1,  10),   S(-25,  20),   S( 65, -10),   S(  2,  11),   S(  5,  31),   S( 32,  20),   S(  5, -23),   S(-76, -28),
            S(  9,  54),   S(-18,  19),   S( 48,  19),   S( 66,  25),   S( 16,  19),   S( 54, -32),   S( 47,   4),   S(-13, -33),
            S( -1,   6),   S(-44,  56),   S( -7,  56),   S( -2,  38),   S( 26,  -6),   S( 18, -22),   S(-45, -24),   S(-10,  -9),
            S(-27,  32),   S(-15,  51),   S(  8,  53),   S( 20,  34),   S( 32,  12),   S( 11,  -8),   S(  5, -58),   S(  0,  -2),
            S(-36, -42),   S(  7,   1),   S( 15,  30),   S( 35,  26),   S(  8, -19),   S(  0,  -3),   S(  1,  -6),   S(  1, -26),
            S( 10,  -7),   S(-15, -47),   S(  1,  -5),   S(-16, -28),   S(  5,  -4),   S( -8,   7),   S( -1, -22),   S( 12,   6),

            /* bishops: bucket 5 */
            S(  3,  -5),   S( -9,  40),   S(-53,  29),   S(-53,  32),   S(-26,  36),   S(-57,  24),   S(-11,  18),   S(  8,  43),
            S(-39,  47),   S(-41,  25),   S(-30,  41),   S(-11,  29),   S(-32,  42),   S(-17,  23),   S(-17,  -3),   S( 12, -14),
            S( -3,  16),   S( -1,  33),   S( -1,  40),   S(-18,  47),   S(-20,  50),   S(-31,  24),   S(-22,  28),   S(-25,   8),
            S( 23,  18),   S( 30,  21),   S(-65,  79),   S( 27,  22),   S( 22,  34),   S( -2,  27),   S( 36,  -8),   S( 24,  18),
            S( 42,  35),   S(  4,  33),   S( 57,  35),   S( 95,  24),   S( 74,  -4),   S(  5,  43),   S( 55,  -2),   S( 10, -11),
            S( 34,  24),   S( 17,  52),   S( 25,  68),   S( 34,  19),   S(  6,  41),   S( 17,  -2),   S(-24,  54),   S( 16,  12),
            S( 12,  16),   S(-36,  31),   S(  1,  20),   S( 19,  49),   S( 27,  10),   S( 45,  44),   S(  7,  51),   S( -6,  28),
            S(  6,  13),   S( 15,  42),   S(  8,  18),   S(-30,  38),   S(-20,  57),   S(  9,  13),   S( 32,  58),   S( -8,  19),

            /* bishops: bucket 6 */
            S(-10,  24),   S(-16,  35),   S(-60,  40),   S(-59,  37),   S(-62,  29),   S(-32,  24),   S(-19,  66),   S( 15,   0),
            S( 51, -22),   S(-13,  15),   S( -8,  30),   S(-23,  34),   S(-27,  40),   S(-46,  28),   S(-124,  65),  S( 34,  38),
            S( 28, -22),   S( 12,  11),   S( 32,  24),   S( 43,  20),   S( 21,  28),   S( -5,  36),   S(  4,  37),   S(-46,  16),
            S( 17,  23),   S( 22,   1),   S( 43,  19),   S( 23,  25),   S( 34,  29),   S( 14,  30),   S( 31,  34),   S(-26,  11),
            S(-23,  23),   S( 77,  -9),   S( 34,  17),   S( 30,  22),   S( 94,  29),   S( 68,  15),   S( 39,  34),   S(-32,  48),
            S( 41,   7),   S(-31,  46),   S( 10,  40),   S( 33,  24),   S( 19,  31),   S( 68,  31),   S( -9,  57),   S(-17,  48),
            S(-38,  28),   S(-32,  50),   S(  6,  32),   S( -1,  30),   S( 16,  20),   S( 15,  24),   S( 20,  49),   S(  6,  53),
            S(  3,  58),   S( 17,  10),   S( 39,  24),   S( -6,  36),   S(  4,  26),   S( 36,  16),   S( 10,  20),   S(  9,  30),

            /* bishops: bucket 7 */
            S( 22, -27),   S( 16,   2),   S(-49, -20),   S(-39,  -3),   S(-28, -10),   S(-84,   3),   S(-52, -49),   S(-64,   7),
            S(-64, -30),   S(-42, -27),   S( -3,  -9),   S( 19, -27),   S(-30,  23),   S(-46,  17),   S(-65,   7),   S(-44, -29),
            S(-45, -23),   S( 41, -29),   S( 12,   1),   S( 25,   5),   S(-26,  12),   S( -3,  -2),   S(-44,  27),   S(-51,  35),
            S(-70,  23),   S( 49, -22),   S( 77, -23),   S( 47,   1),   S(108, -15),   S(-18,  35),   S( 10,  39),   S( -2,  37),
            S( -4, -48),   S(-22,  -7),   S( 58, -36),   S( 86, -31),   S( 80,  16),   S( 85,   4),   S( 13,  42),   S( 29, -15),
            S(-33, -23),   S( -2,  -7),   S( 60, -33),   S( 24,   0),   S( 32,   3),   S( 85,  16),   S( 76,  16),   S( 25, -11),
            S(-16, -38),   S(-52,  26),   S( 23, -19),   S( 12, -19),   S( 26, -38),   S( 38, -20),   S( 44,  24),   S(-10,  -6),
            S( 13,  -6),   S(-36,  -1),   S(-26,  24),   S( 14, -15),   S(  7, -11),   S( 26,  -7),   S(  9,  -3),   S( 28,  25),

            /* bishops: bucket 8 */
            S( -1,  -7),   S(-11, -49),   S(-65, -26),   S(-11, -23),   S(  0,  43),   S( -5, -10),   S(  0, -19),   S(-10, -22),
            S(  3,   0),   S(-48, -26),   S(-18, -19),   S(-18, -11),   S( -1, -16),   S(-37, -28),   S(  3, -51),   S( -6,  -3),
            S(  5,  17),   S(-11,  21),   S(-21,  -3),   S(-14,  -7),   S( -1,  14),   S( 20, -13),   S( -8, -44),   S(-27, -29),
            S(  7,  49),   S( -2,  37),   S(-12,  25),   S(-27,  12),   S( 24,   7),   S(-15,  -6),   S( 12, -52),   S(-10, -16),
            S(  0,  24),   S( 15,  59),   S(-17,  37),   S( 41,  42),   S( -1,  -9),   S( 20,  15),   S(  8, -26),   S(-17, -29),
            S( -1,  14),   S( 13,  11),   S(  4,  29),   S(-21,   9),   S( 35,   6),   S( -5, -28),   S(-18, -18),   S(-18, -30),
            S(-12, -10),   S( 30,  49),   S( 12,  33),   S( -1,  -4),   S( -7,  27),   S( 11,  26),   S( -9, -12),   S( -8, -34),
            S( -6, -26),   S(  1, -20),   S(  0, -31),   S(  5, -19),   S(-30, -12),   S( -8, -13),   S( -3,  -4),   S( -3,  18),

            /* bishops: bucket 9 */
            S(-17,   3),   S(-16, -33),   S(-24,  -4),   S(-15, -49),   S(-47, -18),   S(-26, -50),   S(-16, -12),   S(  8,   8),
            S(-17, -44),   S(-19,  -1),   S(-39, -39),   S(  3,  10),   S(-33,  26),   S(-17, -12),   S(-12, -13),   S(  1, -19),
            S( 17,  12),   S( 16, -13),   S(  1,  -2),   S(-26,   9),   S(  5,  32),   S(  9,   4),   S(-16, -33),   S( 12,  38),
            S(-17,  -3),   S( 32,  24),   S(-27,  25),   S(-15,  32),   S( 24,   6),   S( 29, -15),   S(  1,  -9),   S( -9,  -7),
            S(-10,  12),   S( 25,  11),   S(  7,  51),   S( -1,  60),   S(  6,  13),   S(  6,  21),   S(  7,   7),   S( -3,   2),
            S(-10, -22),   S( 15,  65),   S( 10,  16),   S(-16,  48),   S(  8,  34),   S( 12,  26),   S( -4,  29),   S(-18,  -2),
            S( -1,  -1),   S( 44,  27),   S( -5,   6),   S( -5,  63),   S( 29,  32),   S( 12,  10),   S( -4,   1),   S(-10, -20),
            S( -6, -28),   S( -4,  18),   S( -8,   9),   S(-23, -43),   S(-17,  -5),   S( 16,  34),   S( -6, -12),   S( -1,   2),

            /* bishops: bucket 10 */
            S(-24,  -1),   S(  5, -35),   S(-36, -10),   S(-41,   1),   S( -3,  -3),   S(-32, -11),   S(-11, -43),   S(-21, -12),
            S( 11, -40),   S(-12,  -5),   S( -5, -12),   S(-35,  17),   S(-24,  14),   S(-19,  35),   S(-13, -40),   S( -5, -14),
            S( 16, -22),   S( -6, -26),   S(-29, -28),   S( -1,  14),   S(-26,  14),   S(-41,  65),   S(-20,  31),   S(  2,  17),
            S(-13, -48),   S(  0,   4),   S( 23, -18),   S(  3,  18),   S( 49,  24),   S(  1,  23),   S(  3,  38),   S( 14,  46),
            S(-10,  -2),   S(-16,  16),   S(-20,  -3),   S( 47,  24),   S(-13,  55),   S( 27,  51),   S(  8,  53),   S(-17, -52),
            S(-11, -43),   S(-44, -12),   S(-24, -22),   S(-33,  14),   S( 16,   3),   S( 52,  44),   S(  4,  22),   S( 12,  18),
            S(-28, -31),   S(-10, -24),   S(-21,  -7),   S( 12, -35),   S( -7,   7),   S( 14,  24),   S( 26,  32),   S( 21,  33),
            S(  0,  -1),   S(  1,   8),   S(  6, -17),   S( -3, -17),   S( 10,  14),   S(-10, -10),   S( -1,  11),   S(  5,  36),

            /* bishops: bucket 11 */
            S(-21,  14),   S(-23, -22),   S(-43, -32),   S(-29, -10),   S(-19, -28),   S(-58, -51),   S(-14, -24),   S(-27, -26),
            S(-19,   2),   S( 20, -17),   S(-12, -31),   S(-25, -42),   S(-46, -33),   S(-23, -34),   S(-12, -27),   S(-19, -17),
            S(-14, -60),   S( -5, -45),   S(  3,  -4),   S(  1, -17),   S(-12, -26),   S(-19,  36),   S(-10,  16),   S(  5,  26),
            S(-29, -20),   S(  3, -34),   S(  9, -34),   S( -4, -35),   S( 15,  17),   S( 25,  40),   S( 18,  63),   S( 29,  27),
            S(-10, -23),   S(-12, -49),   S(-48,  34),   S( 34,   7),   S( 23,  34),   S(-24,  64),   S( 17,  48),   S( 11,  12),
            S(-14, -31),   S(-37,  -1),   S( -4, -41),   S( 33,  19),   S(  8,   3),   S( 28,  49),   S( 40,  43),   S(  1,   7),
            S(-12, -18),   S(-15, -25),   S(-21, -11),   S(  0,  -8),   S( 13,  12),   S( 35,  -6),   S(  2,  14),   S(  8,  16),
            S(-19, -20),   S(-17, -23),   S( -8,  -2),   S(  8,   3),   S( 21,  27),   S(-21, -39),   S( -3,  13),   S( 10,  -2),

            /* bishops: bucket 12 */
            S(  3,  13),   S( -4,  -2),   S(-12, -48),   S(  6,  -6),   S( -6,   5),   S(  3,   4),   S( -2,   4),   S( -5,   3),
            S( -6,   3),   S( -6, -21),   S(  5,  24),   S(-13, -20),   S( -6,   0),   S( -2,   9),   S( -3,  19),   S( -6, -10),
            S(  2,   1),   S(-25,  -5),   S(-12, -17),   S(-10, -24),   S( -2,   0),   S( -2,   5),   S( -7, -33),   S( -1,   2),
            S( -1,  -4),   S( -9, -23),   S(-13, -34),   S( -3,   3),   S(  3,   8),   S( 14,  27),   S(  0,  -5),   S( -2,   8),
            S( -8, -31),   S(-13, -17),   S( -8,  40),   S(-12, -13),   S(-10, -20),   S( -8,  -2),   S(  2,  -8),   S(  2,  18),
            S(-21, -38),   S( -5,  45),   S(-21,  12),   S(-21, -14),   S(  1,  -2),   S(  0,   6),   S(  6,  17),   S( -1,   6),
            S( -3,  -8),   S( -8,   6),   S( -3,  14),   S(-10,  -3),   S( -7,  -6),   S( 14,  29),   S( -7, -29),   S( -6,  -3),
            S(  0,   5),   S( -7, -22),   S( -4,   0),   S( 14,  19),   S(  0,  10),   S( -2,   6),   S( -9,  -4),   S(  2,   2),

            /* bishops: bucket 13 */
            S(-14, -62),   S(-14, -27),   S(-10, -25),   S(-12,  16),   S(-22, -37),   S(-20, -20),   S(-10, -17),   S(  1,  13),
            S( -2,   1),   S( -5,  -7),   S( -8, -26),   S(-18, -16),   S(-19,  -1),   S(-13, -10),   S(  2,   5),   S(  2, -12),
            S(-10,  -6),   S(  9,  23),   S(-16,  10),   S(-11,  21),   S(-19, -28),   S(  3,  -6),   S(-10, -46),   S(  9,  33),
            S( -4,   3),   S( -5, -19),   S(-11, -11),   S(-33,  31),   S( 11,   9),   S(  5,  28),   S( -3,   8),   S(-10, -16),
            S( -1,   4),   S(-14,  -6),   S(-17, -24),   S( 18,  19),   S(-11,  11),   S(  0,   5),   S( -5, -13),   S(  1,  -2),
            S( -3,  -7),   S(  2,  26),   S( -2,  -5),   S(  8,  40),   S(  2,   8),   S( 13,  -4),   S(  3,  32),   S(-10, -23),
            S( -4,  -4),   S(  0,  20),   S(  7,  37),   S(-13,  12),   S(-10,  13),   S(  5,   0),   S(-10,  -8),   S(  0,   0),
            S( -2,  -9),   S( -1,   5),   S( -6,  -2),   S(  5,   1),   S( -4,   0),   S(-12, -24),   S( -6,  -9),   S( -7, -40),

            /* bishops: bucket 14 */
            S( -4,  -3),   S(-16, -14),   S(-23, -25),   S(-15, -50),   S(-13, -39),   S( -9, -27),   S(-11, -27),   S(-11, -15),
            S(-11, -22),   S(  6, -17),   S(  2,  -8),   S(-36, -58),   S(-16, -21),   S(-25, -27),   S(-14,   9),   S( -1, -18),
            S(-10, -27),   S(-13, -22),   S(-21, -46),   S(-11, -15),   S(-39, -38),   S(-32, -68),   S( -5,  12),   S( -9,  -9),
            S( -7, -23),   S( -1,   0),   S(  0, -18),   S(-29,  17),   S( -4, -17),   S( -6,  15),   S(-18,   1),   S( -9, -28),
            S( -6, -11),   S(-11,  31),   S( -3, -13),   S(  0, -20),   S(  1,  -3),   S( -9,  -8),   S( 10,  20),   S( -4, -27),
            S(  0,  14),   S(-11,   3),   S(-17, -25),   S(-16, -30),   S( -7,  14),   S(  0,  46),   S(-17,  12),   S( -9, -21),
            S( -6, -28),   S( -4,   6),   S( -2,  12),   S(  8,  15),   S(-21, -15),   S(-11,  -4),   S( -8, -21),   S( -3,  -8),
            S( -1, -10),   S( -2,  -4),   S( -8, -20),   S(-13, -18),   S(-10, -19),   S( -6, -12),   S(  2, -13),   S( -1,  -4),

            /* bishops: bucket 15 */
            S( -5,  -2),   S( 12,  10),   S(-10, -17),   S(  0,  -5),   S(-16, -21),   S( -6,  -6),   S( -5, -10),   S( -4, -17),
            S(  1,   2),   S(  0,   5),   S(  7, -10),   S(-10,  -6),   S(-18, -35),   S(-10, -23),   S( -2,   0),   S( -6,  -3),
            S(-13, -36),   S(-14, -22),   S(-19, -20),   S(-19, -17),   S(-37, -38),   S(-24, -24),   S( -9, -10),   S( -3,   5),
            S( -4,  -8),   S(-17, -20),   S( -2, -15),   S(-22, -65),   S(-17,  -3),   S( -5, -12),   S(  6,  30),   S( -4, -27),
            S( -6, -22),   S( -5,  -4),   S( -7,  24),   S(-16, -58),   S(  4, -41),   S( -7,  19),   S(  2,   1),   S(-10, -24),
            S(-12, -26),   S(-16,  -7),   S(-12, -20),   S(-22, -14),   S(-15,  -9),   S(-17,  -8),   S(  2,  27),   S( -2,   5),
            S( -2,   0),   S(  1, -24),   S(  0,   2),   S(-12, -17),   S(-16, -28),   S( -6, -12),   S(-17,   1),   S(  8,  10),
            S( -5,  -7),   S( -3,   7),   S( -7,  -6),   S( -2,   2),   S(-10, -15),   S(-26, -38),   S( -3, -18),   S(  2,   5),

            /* rooks: bucket 0 */
            S(-22,  -5),   S( -1,  -4),   S(-12, -11),   S(  9, -10),   S( -4,   9),   S(  3,  -6),   S( -5,  13),   S(  7,  14),
            S( 30, -75),   S( 25, -31),   S(  7, -21),   S( -4,  -8),   S( 22,  -3),   S( 21, -26),   S(-16,   1),   S(-48,  35),
            S( -1, -15),   S(  3,  32),   S( 17,   7),   S( 19,  -9),   S( -6,  31),   S(  5,   4),   S(-16,  -8),   S(-49,  34),
            S( 62, -45),   S( 65,   2),   S( 37,  25),   S( 35,  12),   S( 22,  21),   S(-11,  23),   S(-38,  45),   S(-39,  34),
            S( 48, -32),   S( 67, -11),   S( 84,  -7),   S( 49, -19),   S( 35,   9),   S( 14,   8),   S(  3,  33),   S(-39,  44),
            S( 78, -49),   S(120, -67),   S( 40,  -6),   S( 42,   3),   S( 44,  -3),   S(-38,  34),   S( 56,  -3),   S(-67,  46),
            S( 34, -21),   S( 63,   8),   S( 13,  24),   S( 14,  16),   S(  0,  10),   S(  2,  20),   S( 11,  17),   S(-29,  32),
            S( 63,  22),   S(  0,  47),   S( -7,  29),   S( 20,  31),   S(-12,  18),   S( 34, -15),   S(-33,  45),   S( 32,  18),

            /* rooks: bucket 1 */
            S(-78,  41),   S(-49,  11),   S(-53,   1),   S(-36, -10),   S(-23, -25),   S(-22, -16),   S(-27,  -6),   S(-37,  21),
            S(-33,  -3),   S(-58,  14),   S( -7, -33),   S(-34, -15),   S(-23, -17),   S(-21, -33),   S(-34, -24),   S(-78,  24),
            S(  7,   3),   S(-27,  27),   S(-16,   3),   S(-30,   9),   S(-20,   2),   S(-26,  17),   S(-26,   3),   S(-47,  33),
            S(-64,  71),   S(-77,  62),   S(-20,  39),   S(-21,  31),   S(-34,  32),   S(-62,  59),   S(-32,  44),   S(-32,  22),
            S( 26,  39),   S( 24,  39),   S( 13,  18),   S(-29,  52),   S(-16,  41),   S( 29,  17),   S(-17,  33),   S(-12,   9),
            S( 41,  15),   S(-10,  54),   S( 12,  17),   S(-42,  36),   S( 22,   6),   S(-32,  50),   S( 35,  19),   S(-15,  26),
            S(-14,  41),   S(  0,  42),   S( 20,  39),   S(-90,  79),   S( 21,  30),   S(-38,  70),   S(-50,  53),   S(-56,  42),
            S( 37,  24),   S( 27,  44),   S( -3,  55),   S(-37,  51),   S(  2,  19),   S( 36,  20),   S(-35,  56),   S(  5,   4),

            /* rooks: bucket 2 */
            S(-69,  40),   S(-51,  23),   S(-46,  19),   S(-55,  16),   S(-60,  16),   S(-50,  -8),   S(-43, -25),   S(-53,  27),
            S(-86,  42),   S(-46,  20),   S(-54,  22),   S(-69,  13),   S(-46,  -1),   S(-66,  -6),   S(-67,  21),   S(-62,   9),
            S(-71,  58),   S(-65,  63),   S(-56,  48),   S(-35,   1),   S(-48,  27),   S(-30,  16),   S(-30,  15),   S(-38,  28),
            S(-72,  68),   S(-42,  59),   S(-44,  58),   S(-37,  47),   S(-28,  30),   S( -1,  26),   S(-19,  41),   S(-31,  38),
            S(-37,  54),   S(-49,  68),   S(-46,  57),   S(-15,  35),   S( 25,  21),   S( -8,  45),   S( -6,  35),   S(-46,  55),
            S(-56,  52),   S(-36,  48),   S(-11,  34),   S(-13,  27),   S(  6,  33),   S( 47,  19),   S( 13,  33),   S(-17,  31),
            S(-73,  53),   S(-81,  84),   S(-44,  61),   S( -1,  47),   S(-31,  54),   S( 48,  12),   S(-64,  73),   S(-34,  52),
            S(-63,  85),   S(-35,  58),   S(-68,  56),   S(-51,  57),   S(-88,  84),   S(-16,  53),   S(-49,  81),   S(-31,  48),

            /* rooks: bucket 3 */
            S( -1,  71),   S( -2,  73),   S(  8,  64),   S( 15,  55),   S( 11,  49),   S( -5,  68),   S( -6,  81),   S(-15,  41),
            S(-33,  91),   S( -5,  59),   S(  3,  61),   S(  3,  60),   S( 13,  55),   S( 10,  55),   S( 48, -12),   S( 28, -49),
            S(-31,  88),   S(-10,  88),   S( -7,  79),   S(  5,  69),   S( 10,  80),   S( 25,  65),   S( 31,  59),   S( 16,  44),
            S(-22,  98),   S(-10,  74),   S( 21,  73),   S( 25,  69),   S( 26,  68),   S(  7, 100),   S( 61,  78),   S( 14,  76),
            S(-24, 109),   S( 26,  82),   S( 18,  71),   S( 44,  76),   S( 54,  65),   S( 50,  68),   S( 97,  52),   S( 53,  55),
            S(-20,  93),   S(  8,  80),   S(  8,  83),   S( 10,  76),   S( 28,  62),   S( 50,  51),   S( 77,  45),   S( 86,  24),
            S(-47, 112),   S(-20, 112),   S( -7, 102),   S( 25,  82),   S( 14,  78),   S( 31,  70),   S( 72,  62),   S(111,  37),
            S(-76, 160),   S(  6, 104),   S( 34,  64),   S( 52,  64),   S( 65,  53),   S( 52,  59),   S(138,  45),   S(115,  50),

            /* rooks: bucket 4 */
            S(-70,  13),   S(-23,   5),   S(-13,  -8),   S( -7,  15),   S(-44,  -1),   S(  2, -39),   S( 25, -41),   S( -5, -37),
            S(-28, -17),   S(-64,  -2),   S(-77,  42),   S(-12,   7),   S(  2, -38),   S(-17, -28),   S(  1, -14),   S(-16, -18),
            S( -5, -19),   S( -8, -17),   S(-26,  -2),   S( -1,  -6),   S(  3,  -2),   S( -2,  -7),   S( 18,  -8),   S(-57,  -9),
            S(-49,  -9),   S(  2,  19),   S(-27,   6),   S( 25,  13),   S( 17,  -6),   S(  6,  -7),   S(-26,  40),   S( -4,  20),
            S( -9, -31),   S( -5,  -2),   S( -2,  13),   S( 65,  20),   S( -2,   3),   S( 13,  26),   S( 28,  40),   S( 32,  12),
            S( 13, -16),   S( 49, -12),   S( 44,  23),   S( 12,  21),   S( 17,  22),   S( 18,  28),   S( 20,  21),   S( -3,  43),
            S(  2,  -2),   S( 16,  33),   S( 21,  23),   S( 44,   3),   S( 58,  20),   S( 18,   3),   S( 24,  43),   S( 52,  41),
            S( 46, -66),   S( 50,  56),   S( 28,  31),   S(-13,   5),   S( 38, -13),   S( 31,  40),   S(  7,  21),   S(  2,   4),

            /* rooks: bucket 5 */
            S(-31,  36),   S(-62,  65),   S(-33,  37),   S(-60,  34),   S(-18,  18),   S(-31,  33),   S(-27,  38),   S(-39,  50),
            S(-39,  49),   S(-58,  38),   S(-95,  61),   S(-20,  10),   S(-24,  16),   S( -8,  10),   S( 10,  10),   S(-65,  42),
            S(-12,  43),   S(-77,  68),   S(-69,  41),   S(-65,  38),   S(-54,  45),   S( 19,  17),   S(  8,  38),   S(-39,  51),
            S(-43,  86),   S(-37,  41),   S(-22,  49),   S( -2,  35),   S( -2,  49),   S(-15,  69),   S( -8,  70),   S( 22,  31),
            S( -1,  71),   S(-25,  77),   S( 22,  53),   S( 21,  53),   S( 44,  45),   S( 30,  62),   S( 55,  61),   S( 27,  48),
            S( 17,  71),   S( 11,  74),   S( 71,  46),   S( 38,  70),   S( 53,  55),   S( 36,  55),   S( 37,  63),   S( 38,  66),
            S( 29,  58),   S(  5,  68),   S( 55,  66),   S( 66,  49),   S( 61,  40),   S( 84,  54),   S( 91,  53),   S( 66,  55),
            S( 64,  45),   S( 88,  18),   S( 17,  63),   S( 20,  45),   S( 71,  48),   S( 49,  44),   S( 33,  50),   S( 37,  54),

            /* rooks: bucket 6 */
            S(-63,  36),   S(-38,  34),   S(-14,  26),   S(-14,  13),   S(-32,   9),   S(-99,  53),   S(-61,  55),   S(-36,  53),
            S(-45,  44),   S(-54,  48),   S(  4,  19),   S(-49,  20),   S(-45,  36),   S(-93,  68),   S(-102,  64),  S( 46,   6),
            S(-74,  72),   S(-39,  52),   S( -8,  30),   S(-56,  42),   S(-15,  17),   S(-69,  68),   S(-55,  78),   S( 30,  31),
            S(-70,  81),   S( 20,  44),   S(-11,  57),   S(-12,  45),   S(-37,  49),   S(-22,  59),   S(-50,  62),   S(-15,  56),
            S(-12,  83),   S( 20,  68),   S( 52,  43),   S( 22,  44),   S(  9,  61),   S( 13,  54),   S(  9,  58),   S( -3,  54),
            S( 13,  62),   S( 50,  54),   S( 86,  34),   S( 43,  29),   S( 32,  50),   S( 53,  54),   S( 44,  63),   S( 71,  43),
            S( 50,  59),   S( 57,  51),   S( 77,  42),   S(102,  17),   S( 99,  38),   S( 46,  69),   S( 84,  54),   S( 74,  48),
            S( 41,  85),   S( 37,  63),   S( 43,  47),   S( 14,  60),   S( 65,  40),   S( 79,  51),   S( 54,  71),   S( 16,  61),

            /* rooks: bucket 7 */
            S(-58, -23),   S(-46, -10),   S(-33, -22),   S(-30,  -3),   S(-14, -12),   S(-51,  27),   S(-44,  -3),   S(  6, -26),
            S(-102,  35),  S(-59,  21),   S(-29,   6),   S(-14, -19),   S( -9,   6),   S( -5,   9),   S(-48,   4),   S(-35,  -4),
            S(-97,  52),   S(-52,  29),   S(-16,   7),   S(-13,  -9),   S(  0,  -5),   S(-15, -11),   S( -2, -31),   S( 16,   3),
            S(-55,  42),   S(  9,   9),   S( 32,   2),   S( 55,  -9),   S( 40, -18),   S( 17,  -1),   S( 25, -20),   S( 21,  -8),
            S(-10,  30),   S(-10,  22),   S( 45, -18),   S( 40,  -1),   S( 98, -16),   S(113, -25),   S( 69,  -9),   S( 34, -12),
            S( -2,  22),   S(-16,  17),   S( 78, -26),   S(103, -26),   S(117, -20),   S( 94,   0),   S( 67,  22),   S(  6,   0),
            S(-10,  33),   S( 34,  24),   S( 92,  -3),   S( 74,  -7),   S(107, -12),   S( 92,   8),   S( 29,  46),   S( 19,  11),
            S(-16,  71),   S(-10,  33),   S( 27,   3),   S( 70, -15),   S( 31,   5),   S( -4,  29),   S( 74,   4),   S( 63,   1),

            /* rooks: bucket 8 */
            S(-63, -66),   S(-23,  16),   S(-20,   8),   S( -5, -11),   S( -3, -74),   S(-30, -75),   S(-30, -38),   S(  0, -20),
            S( -8, -18),   S(-23, -11),   S(  4, -13),   S(  9, -53),   S(-13, -16),   S(-11, -15),   S( -9, -32),   S(-24, -100),
            S( 16,  22),   S(  2, -15),   S( 29,  -6),   S(-23,  20),   S(-16, -19),   S( -1, -54),   S(  7,  24),   S(  6,  -5),
            S(-13, -49),   S(-10,  13),   S(  1,  -2),   S( 19, -22),   S( 10,  10),   S(  2, -17),   S( 10, -22),   S(-27, -31),
            S(-14, -24),   S( 14,  18),   S(  0,   3),   S( -6,  15),   S(-15, -27),   S( 30, -20),   S( 19,   9),   S(  2, -39),
            S( 10,  47),   S( -8,   8),   S( 22,  35),   S( 16, -21),   S(-13, -12),   S(  3,  -8),   S( -4, -15),   S( 14,  27),
            S( -1, -19),   S( 36, -27),   S( 36,  22),   S( 49,   6),   S( 37,  13),   S( 25, -40),   S( 25,  -8),   S( 27,   3),
            S(  3, -139),  S(  6, -16),   S( 34,  16),   S(  2, -21),   S(  5, -14),   S(  9,  -6),   S( -3,  -4),   S( 33,  39),

            /* rooks: bucket 9 */
            S(-59, -23),   S(  8,  -9),   S(-19, -16),   S(-46,  -4),   S(-25,  14),   S(-10,  -2),   S(  8, -25),   S(-44, -24),
            S( 31, -11),   S( -1, -25),   S(-17, -34),   S(-34,  -5),   S(-17, -24),   S( 15,  19),   S( 18, -25),   S(-24, -50),
            S( -8,  -3),   S( 31,   5),   S( -4,   8),   S(  2,   7),   S(  6, -24),   S( 29,  -4),   S( 11,  42),   S(  6,  23),
            S(  7,  16),   S(  9,   4),   S( 16,  38),   S( 10,  -9),   S( 14,  26),   S( 12, -23),   S(  4,  10),   S( 19,  -7),
            S( -4,  32),   S(  6,  17),   S(  1,  17),   S( 29,  38),   S( 31,  22),   S( 19,  15),   S( 12,  -8),   S(  6, -10),
            S(  9,  51),   S( -2,  -1),   S(  3,  27),   S(-17,   0),   S( 11, -25),   S( 24,  11),   S(  5,  48),   S( 22,  15),
            S( 59,  13),   S( 59,  18),   S( 57,  25),   S( 59, -15),   S( 26, -19),   S( 46, -21),   S( 36,   8),   S( 56,  17),
            S( 75, -102),  S( 35, -19),   S( 30,  17),   S( 21,  45),   S( 24,  43),   S( 21,  26),   S( 29,   9),   S( 23,   2),

            /* rooks: bucket 10 */
            S(-29, -74),   S(-16, -70),   S(-48, -12),   S(-28,  10),   S(-54, -28),   S(-58, -34),   S( 25, -12),   S(-15, -26),
            S(  2,   2),   S( 18, -15),   S(-14, -40),   S(-13,  13),   S( 16, -22),   S( -9,   8),   S( 34, -17),   S(-15,  13),
            S(-19, -27),   S( -1, -27),   S(-10, -14),   S( 12, -22),   S(-39,  17),   S( -4, -14),   S( 29,  23),   S( -2, -11),
            S(-12,   8),   S( 21, -22),   S(-16,  -1),   S( 10,  10),   S( 10, -26),   S( 19, -10),   S( 20,  42),   S( -4, -37),
            S(  9,   7),   S( 31, -14),   S( 17,  18),   S( -8, -23),   S( -9,   1),   S( 16, -13),   S( 38,  39),   S( 14,  33),
            S( 36,  10),   S( 26,  56),   S( 21,  35),   S( 20, -12),   S(-10, -14),   S(  4,  11),   S( 35,  21),   S( 30,  31),
            S( 84,  10),   S( 76,  11),   S( 74, -11),   S( 63, -30),   S( 63, -16),   S( 48,   2),   S( 25, -23),   S( 20,  -5),
            S( 67,  18),   S( 19, -13),   S( 37,  -8),   S( 38,   1),   S( 38,  39),   S( 15,   7),   S(  5,  -9),   S( 16, -17),

            /* rooks: bucket 11 */
            S(-47, -43),   S(-16, -37),   S(-27,  -6),   S(-34, -61),   S(-10, -27),   S( -8,  -9),   S(-31,  -7),   S(-47, -13),
            S(-21, -44),   S( 11, -47),   S(-21, -10),   S( -4, -29),   S(-13, -49),   S(-15, -23),   S( -1, -12),   S(-12,   3),
            S( 10, -24),   S( -3, -25),   S( 11, -39),   S(  4, -30),   S( 21, -33),   S(-16,   1),   S(-30, -25),   S(-41, -68),
            S( -5,  11),   S( -3,  -6),   S(  4,   4),   S( 12,   3),   S( 22,  -4),   S(  7,  20),   S( 23, -44),   S(  5, -14),
            S( 26,   9),   S( 19, -27),   S( 22,  -4),   S( 22, -10),   S( 34, -18),   S( 21,  -5),   S( -2, -10),   S(  2,   1),
            S( 12,  31),   S( 44,   9),   S( 24,  -9),   S( 42,   4),   S( 54,  14),   S( 47,  11),   S( -2, -35),   S( 22,   1),
            S( 55,  46),   S( 67,   6),   S( 55, -28),   S( 70,  -3),   S( 51, -27),   S( 36,  15),   S( 29,  10),   S( 43, -22),
            S( 62,  11),   S( 21,  11),   S( 12,  -9),   S( 11, -12),   S( 14, -15),   S(  9,  17),   S(  1,  -1),   S( 25, -26),

            /* rooks: bucket 12 */
            S(  1, -33),   S(-17, -36),   S(-24, -78),   S(-15, -34),   S( -7,  -5),   S( -3, -49),   S(-36, -79),   S(-15, -35),
            S( 10,  -5),   S(  6,  -8),   S( -1,  -7),   S(  6,   3),   S( -5,   5),   S(  0,  14),   S( -4, -14),   S( -2, -16),
            S(  0,   7),   S(-13, -35),   S( -2, -15),   S(-11,  10),   S( -5, -29),   S(  4,  -1),   S( -3,   5),   S(  3, -13),
            S( -5,   0),   S(  2,  -3),   S( -4,  21),   S(  8, -15),   S( 15,  10),   S(-16, -42),   S(-15, -18),   S(-11, -41),
            S(  2,  -6),   S( -5, -36),   S(  4,  -2),   S( -5, -29),   S( -6, -40),   S( 11,  10),   S(-12, -25),   S( -4, -19),
            S( -7,   0),   S(  4, -16),   S( -4,   3),   S(  5,  -4),   S( -3, -20),   S(  2, -19),   S(  6, -25),   S( -3, -15),
            S(-13, -23),   S(  5, -16),   S(-11, -69),   S(  4, -24),   S( 10,  -7),   S(-10, -35),   S( -2, -29),   S(  2, -19),
            S( -9, -56),   S(  4,   4),   S( 14,  25),   S(  9,  12),   S( -4, -24),   S(  0, -15),   S(-19, -50),   S( 15,  16),

            /* rooks: bucket 13 */
            S( -8, -34),   S(-11, -32),   S( -4, -45),   S( -3,  10),   S( 10,  -5),   S( -9,  -8),   S( 13,  -7),   S(-31, -60),
            S( -5, -12),   S( -1, -16),   S( -8,  -5),   S( -6,  22),   S(  0,   4),   S(-11, -27),   S( -1, -23),   S( -8, -30),
            S(-15, -47),   S(-18, -46),   S(  6, -13),   S(  2, -15),   S(  4,  -3),   S(  1,  19),   S(  4,  -9),   S( -5, -44),
            S( -7, -45),   S(  4,  18),   S( -9,  -5),   S(  2, -13),   S( 25,  13),   S(-12, -30),   S(  1, -21),   S(  0, -21),
            S( 13, -21),   S(  3, -16),   S(  8,  30),   S( -2, -15),   S( -3,  -8),   S(  4, -16),   S( -8, -49),   S( 13, -29),
            S(-11, -13),   S(  7, -42),   S(-26, -35),   S( 13,  -8),   S(  8,  -6),   S( 10,  13),   S(  8, -38),   S( -2,   2),
            S(  3,  -6),   S( 17,  25),   S( 14,  -4),   S( -7, -31),   S(  9, -59),   S( 21, -24),   S(  0, -43),   S( -2, -48),
            S(-17, -145),  S(-10, -49),   S( 16,   9),   S(-10, -21),   S(  6,  45),   S(-10, -25),   S( -9, -25),   S(  4,   1),

            /* rooks: bucket 14 */
            S(  5,  -5),   S(-26, -64),   S( -3,  -1),   S( -9, -47),   S(  5,   8),   S(-15, -13),   S( 12,  -1),   S( -5, -18),
            S(-25, -72),   S(-14, -35),   S(  2,  16),   S( -1, -14),   S(-11,   5),   S( -6, -44),   S(  2,  11),   S( 10,   3),
            S( -4, -43),   S(  1,   5),   S( -3, -14),   S(-12,  -5),   S( -6, -15),   S( -6, -12),   S(  1,  23),   S(  2, -15),
            S( 11,   5),   S(-14, -24),   S(  7,  -1),   S( -8,  -6),   S( -9, -13),   S(  2, -12),   S(  2,   1),   S(-14, -45),
            S( -7, -48),   S(  4, -16),   S( -5, -11),   S( -6, -33),   S(  1,  15),   S(-23, -34),   S(  8,   4),   S(  8,  -1),
            S(  5,  -1),   S( -4, -22),   S(  8, -24),   S(  2, -14),   S(-17, -17),   S( -8,  10),   S(  0, -27),   S( -2,  -2),
            S( 16, -14),   S(  1, -52),   S( -3, -31),   S( 10,  -8),   S(  8, -49),   S( -3, -16),   S( 15,   8),   S(  2,  -3),
            S(  0, -32),   S(  4, -23),   S(-16, -29),   S(  3,  15),   S(-23, -34),   S(  6,  -7),   S(  8,  14),   S( 14,   5),

            /* rooks: bucket 15 */
            S(  6, -54),   S( -1, -25),   S(  2, -23),   S(-16, -47),   S(  0,  -4),   S( -2, -21),   S( -1, -27),   S( -6, -31),
            S(-11, -14),   S(-15, -30),   S(  5,   8),   S(-16, -44),   S(-12, -22),   S(  8, -14),   S(-22, -56),   S(  4, -17),
            S( -7, -16),   S( -9, -22),   S( -4, -10),   S( -5,  12),   S(  5, -26),   S( 14,   0),   S( -3, -17),   S( -3, -20),
            S( -3, -40),   S( -5, -33),   S(-10,  -5),   S(-11, -27),   S(-16, -20),   S(  2, -26),   S( -3, -38),   S( -9,  -1),
            S( -6, -26),   S( -2,  20),   S( 19,   1),   S( -1, -29),   S(  0,   7),   S( 12,  10),   S(  0,  18),   S( 10,  38),
            S(  8,   2),   S( -1, -23),   S(  0, -25),   S(  1,  -3),   S( -1,  -1),   S(-16,  -1),   S(  8,   1),   S(-19,  -6),
            S(  5,  -6),   S( -3, -23),   S( 15, -39),   S( -6, -35),   S( -4, -19),   S( 10,  35),   S( -6,   0),   S(-11,  -1),
            S(  8,  13),   S(  3, -17),   S(  0, -23),   S( -5, -22),   S( -3, -20),   S(  5, -29),   S( -4, -23),   S( -2,  -9),

            /* queens: bucket 0 */
            S(-48, -15),   S(-32, -57),   S( 44, -78),   S( 57, -38),   S( 31, -32),   S( 15,  -8),   S( 40,  10),   S( 13,  42),
            S( -2, -30),   S( 35, -63),   S( 34,  -3),   S( 34,  -7),   S( 30,  20),   S( 16,  19),   S( 18,  44),   S( 54, -21),
            S(  8,  37),   S( 30,  53),   S( 17,  15),   S( 18,  40),   S( 26,  20),   S( 15,  34),   S( -7,  50),   S( 38,  25),
            S( 19,  19),   S( 14,  68),   S(  9,  68),   S( 22,  51),   S( 20,  59),   S( 17,  25),   S( 36,  12),   S( 13,  27),
            S( 30,  30),   S( 20,  55),   S(  5,  59),   S( 15,  76),   S( -5,  29),   S( 10,  10),   S( 28,  11),   S( 33,  11),
            S( 25,  79),   S( 39,  44),   S( 25,  56),   S( -4,  13),   S( 74,  21),   S( 25,  32),   S( 26,  15),   S( 10,  12),
            S( 38,  73),   S( 24,  54),   S(  6,  50),   S( 72,  65),   S( 14,   4),   S(-15,  -4),   S( 14,  25),   S( 14,  36),
            S( 32,  38),   S( 18,  36),   S( 55,  26),   S( 56,  54),   S( 74,  51),   S(  0,  14),   S( 58,  32),   S( 78,  44),

            /* queens: bucket 1 */
            S(-15, -30),   S(-72, -19),   S(-67, -23),   S(-13, -78),   S(  6, -52),   S(-10, -44),   S(  7, -37),   S(  3,  22),
            S(-37, -15),   S(  0, -64),   S( 14, -54),   S( -5,  47),   S(  2, -10),   S(  2,  22),   S( 32, -62),   S( 14,  -6),
            S(-26,  62),   S( 13,  -2),   S(  5,  23),   S(  7,  10),   S(  6,  36),   S( -7,  41),   S( 26, -23),   S( 26,  32),
            S(  5,  -4),   S(-13,  13),   S(-12,  54),   S( 34,  22),   S( 10,  59),   S( 23,  20),   S( -6,   3),   S( 16, -10),
            S( 30, -39),   S(  1,  33),   S(  1,  78),   S( -2,  55),   S( -2,  45),   S( -8,  35),   S(-20,  22),   S( -3,  33),
            S(-18,  37),   S( 18,  41),   S( 41,  66),   S(-26,  52),   S( 12,  35),   S(-13,  57),   S( 42,  27),   S(  9,  23),
            S( -1,  51),   S(-14,  86),   S( -8,  32),   S(-30,  72),   S(-21,  83),   S( 28,  37),   S(-16,  34),   S(-56,  53),
            S(-28,  11),   S( -8,  24),   S( 41,  39),   S(-29,  13),   S( 19,  42),   S(-17, -13),   S(-23,   4),   S(  8,  44),

            /* queens: bucket 2 */
            S( 15,  13),   S( 19, -44),   S( 15, -53),   S( -2, -15),   S(-15,  11),   S(-22, -24),   S(-17, -21),   S(  7,  32),
            S(  7,   9),   S( 20,  30),   S( 15, -12),   S( 21, -22),   S( 11, -15),   S( 14, -29),   S(  7, -26),   S( 40, -31),
            S( 19,   0),   S( 21,  -1),   S( 13,  36),   S( 12,  39),   S( 20,  56),   S( 17,  58),   S( 14,   0),   S( 29,   2),
            S(  6,  -7),   S(-11,  51),   S( -6,  65),   S( 14,  59),   S(  0,  73),   S( 15,  70),   S( 13,  43),   S( -6,  60),
            S( -3,  21),   S(  9,  12),   S(-22,  72),   S(-43, 133),   S(-18,  97),   S(-31, 115),   S(-23, 128),   S( -3,  95),
            S( 21,  11),   S( -9,  44),   S(-21,  96),   S(-15,  75),   S(-39, 114),   S(-16, 115),   S(-11, 100),   S(  5,  96),
            S(-27,  59),   S(-35, 102),   S(-42,  86),   S( 18,  52),   S(-11,  81),   S(  0,  59),   S(-36,  61),   S(-17,  56),
            S(-78, 113),   S( 39,  38),   S( 47,  33),   S( 39,  40),   S( 23,  58),   S( 46,  36),   S( 24,  34),   S(-13,  35),

            /* queens: bucket 3 */
            S( 79, 106),   S( 60,  74),   S( 49, 108),   S( 42,  97),   S( 65,  46),   S( 43,  24),   S(  3,  29),   S( 55,  62),
            S( 46, 136),   S( 60, 121),   S( 47, 111),   S( 46,  94),   S( 44,  86),   S( 60,  67),   S( 62,  14),   S( 22,  77),
            S( 63,  89),   S( 53, 109),   S( 56,  85),   S( 54,  73),   S( 62,  96),   S( 56, 113),   S( 58, 109),   S( 57,  62),
            S( 37, 134),   S( 47,  89),   S( 56,  98),   S( 50, 109),   S( 40, 119),   S( 40, 147),   S( 63, 107),   S( 32, 149),
            S( 45, 120),   S( 58, 108),   S( 46, 102),   S( 30, 107),   S( 36, 138),   S( 23, 150),   S( 28, 166),   S( 41, 173),
            S( 36, 134),   S( 34, 132),   S( 54, 119),   S( 18, 125),   S( 28, 153),   S( 53, 145),   S( 53, 166),   S( 10, 216),
            S( 41, 147),   S( 46, 125),   S( 77,  94),   S( 69, 103),   S( 31, 132),   S( 37, 141),   S( 60, 178),   S(144,  87),
            S( 81,  95),   S( 80, 115),   S( 61, 113),   S( 93,  85),   S( 27, 139),   S( 96,  77),   S(132,  76),   S(125,  91),

            /* queens: bucket 4 */
            S( -6, -11),   S(  6,   3),   S(-10,  -6),   S(-16,  14),   S(  9,   5),   S( 54,  13),   S(-26,  -8),   S(-22,  12),
            S(-41, -30),   S(-31, -10),   S(-11, -28),   S(-38,  -3),   S( -1,  -8),   S( 22, -20),   S(  1,  12),   S(-48, -36),
            S( 11,   4),   S( 18,   2),   S( 16,  49),   S(  9,  57),   S(  6,  -3),   S(-18,  -6),   S( -1,  -6),   S(-25,   1),
            S( -1,   7),   S( 14,   7),   S(  5,  28),   S(  1,  31),   S( 52,  63),   S(  3,  21),   S(  9, -15),   S( -7, -14),
            S( -7, -12),   S( 16,  23),   S(  9,  37),   S( 79,  57),   S( 17,  37),   S(  9, -12),   S(  1,  22),   S( -4, -31),
            S( 16,  24),   S( 28,   6),   S( 10,  70),   S( 37,  46),   S( 20,  50),   S(-35, -26),   S(-26, -19),   S(-23,  -3),
            S(-10, -25),   S( 15,  41),   S( 33,  53),   S( 36,  46),   S(  9,  11),   S(  3,   2),   S(  3, -18),   S(  2,  -6),
            S(-32, -38),   S(-17, -19),   S( 31,  52),   S(  8,  37),   S( -3,   3),   S( -6,   1),   S(-14, -19),   S(  2, -11),

            /* queens: bucket 5 */
            S(-41, -43),   S(-43, -17),   S(-60, -45),   S(-52, -33),   S(-10, -25),   S( 14, -21),   S( -8, -15),   S( -1, -13),
            S(-23,  -6),   S(-43,  -4),   S(-81,  -9),   S(-68,  -2),   S(  1, -26),   S(-46, -14),   S(-65, -32),   S(-52,  -7),
            S(-21,   9),   S(-80, -45),   S(-86, -29),   S( -6,  25),   S( 34,  61),   S(  7, -22),   S(-14, -11),   S(  5,   8),
            S(-50, -24),   S(-61, -11),   S( 13,  37),   S( 30,  86),   S( 18,  30),   S( -7,  16),   S( 13, -37),   S(-11,   6),
            S(-52, -26),   S(-11,  40),   S( -2,  56),   S( 11,  43),   S( 54,  59),   S(  1,  21),   S( 22,  24),   S(-26, -12),
            S(-21,  12),   S( 17,  69),   S(-13,  27),   S( 38,  63),   S( 33,  48),   S( 25,  21),   S( 13,  24),   S( -1,  -4),
            S(-40, -28),   S(  5,  52),   S( 17,  89),   S( -3,  44),   S(  8,  54),   S( 23,  42),   S(  9,  22),   S(-22, -16),
            S(  9,  22),   S( -1, -13),   S(-10,   1),   S( 13,  56),   S(  6,  36),   S(  6,  21),   S( 15,  -6),   S(-25, -29),

            /* queens: bucket 6 */
            S(-50, -26),   S(-73, -22),   S(-76, -47),   S(-100, -70),  S(-96, -60),   S(-86, -48),   S(-60, -38),   S(-37,  -1),
            S(-85, -22),   S(-67,  -5),   S(-34,   2),   S(-35,  -4),   S(-78,  49),   S(-82,   3),   S(-115, -55),  S( 21,  36),
            S(-49,  14),   S( -5,  25),   S(-26,  21),   S(-105,  83),  S(-52,  69),   S( -4,  -7),   S(-27, -16),   S( 12,  19),
            S(-24,  24),   S(  8,  23),   S(-33,  49),   S(-63,  63),   S( -9,  70),   S(  5,  65),   S(-21,  51),   S( 19,  19),
            S(-41,  27),   S(-13,  28),   S(  0,  47),   S(  3,  31),   S( 32,  81),   S( 77,  30),   S( -6,  28),   S(  2,  52),
            S(-40,  39),   S(-16,  24),   S( 26,  39),   S( 54,  18),   S( -1,  50),   S( 69, 107),   S(-30, -14),   S( -9,  14),
            S(  5,  30),   S( -7,  22),   S(  0,  52),   S(  0,  32),   S( 40,  61),   S( 52,  72),   S(-19,  14),   S(-36,   7),
            S( 14,   6),   S( 15,  23),   S( 36,  28),   S( -1,  32),   S( 25,  44),   S( 24,  53),   S(-17,  33),   S( -9,   0),

            /* queens: bucket 7 */
            S(-16,  -1),   S(-15,   8),   S(-59,  34),   S(  1, -41),   S( -1, -38),   S(-72, -11),   S(-58, -39),   S(-26, -28),
            S(-34, -27),   S(-44,   3),   S( -9,  19),   S( -8,  21),   S(-11,  14),   S(-76,  44),   S(-44,  25),   S(-33, -17),
            S(-52,  -4),   S(-78,  34),   S(  2,  35),   S(  1,  27),   S( 19,  39),   S( -1,  30),   S( -6,  25),   S(-51,  -7),
            S(-51,   6),   S(-10,  37),   S(-13,  36),   S(-16,  69),   S( 32,  27),   S( 53,  29),   S( -1,  64),   S(-15,  18),
            S(-10,  27),   S(-54,  54),   S(  6,  31),   S( 29,   8),   S( 63,  13),   S( 92,   8),   S( 57,  18),   S( 52, -19),
            S( 13,  41),   S(-30,  30),   S(-15,   7),   S( 50,   1),   S( 20,  70),   S( 97,  31),   S( 63,  16),   S( 42,   7),
            S( 39,  -2),   S( -7,  14),   S( -1,  26),   S( 23,  33),   S( 16,  15),   S( 71,  55),   S( 59,  34),   S( 52,  46),
            S( 18,  14),   S( 26,  30),   S( 49,  29),   S( 29,  29),   S( 35,  26),   S( 29,  46),   S( -2,  28),   S( 64,  55),

            /* queens: bucket 8 */
            S(-10, -27),   S( -2,   9),   S(-13,   1),   S( 19,  18),   S( -4,  -1),   S(  0, -16),   S(-26, -34),   S( -3,   3),
            S( -6,  15),   S( -8,  -6),   S( -8,  -3),   S( 27,  25),   S( -1,  -5),   S( -3, -11),   S(-23, -45),   S( -8,  -3),
            S(  6,  10),   S( -8,  -2),   S(-14,  -9),   S( -6, -32),   S(-30, -26),   S(-15, -16),   S(-18, -17),   S( -1, -10),
            S(  1,  10),   S( -3,  -2),   S( -4,   4),   S( 26,  26),   S( 14,  22),   S( -1,  13),   S( 10,  14),   S( -5, -25),
            S( 16,  32),   S(  3,  30),   S( 22,  24),   S( 17,  30),   S( 16,  41),   S( 13,  10),   S(-12,  -7),   S(-21, -28),
            S(  8,  15),   S( 17,  30),   S(-22,  10),   S(  9,  40),   S(-16, -14),   S( -9,   2),   S( 13,  17),   S(  3,   8),
            S(-13, -19),   S(-21, -17),   S( 36,  50),   S( 14,  14),   S( -9,  18),   S(  8,  29),   S( -4, -25),   S( -6, -15),
            S( -8,  -7),   S(  9,  15),   S( -2, -26),   S( -1,  12),   S( -5, -39),   S(  4,   5),   S(  3, -11),   S( -2,   1),

            /* queens: bucket 9 */
            S( -2,  -4),   S(-16, -21),   S(  4,   4),   S(-40, -52),   S(-30, -41),   S(-20, -29),   S(-13, -18),   S(-21, -31),
            S(  2,  16),   S(-13, -16),   S(-34, -36),   S(-25, -32),   S(-14, -13),   S(-27, -33),   S(  0,  -4),   S( -1,   4),
            S(  8,   7),   S( -3,   1),   S(-15,  18),   S(-32, -26),   S(-12,  -5),   S(-12,  -4),   S(  3,  -9),   S( -5,   1),
            S(-12, -14),   S(  4,  19),   S(  7,  31),   S(  6,  31),   S( 18,  20),   S(  9,   4),   S( -6, -10),   S(-14, -23),
            S( -2,   4),   S( 16,  39),   S( 22,  43),   S( 25,  56),   S( 11,  16),   S( -1,  19),   S(  0,   1),   S(  0,  -8),
            S(-36, -42),   S( -3,  14),   S( -3,  22),   S( 11,  35),   S( -5,  -1),   S(-13,  -3),   S( -5,   6),   S( 13,  19),
            S( -4, -16),   S( 10,  14),   S(-16,  -3),   S( 20,  36),   S( 20,   9),   S(  7,   6),   S( 14,  15),   S( -9, -20),
            S( -1, -11),   S( -2, -44),   S( 17,   2),   S( 22,  42),   S(  1, -19),   S(  7,  15),   S( 13,   8),   S(  3, -19),

            /* queens: bucket 10 */
            S( -1,  -9),   S( -9, -13),   S(-17, -36),   S(-32, -30),   S(-14, -20),   S(  5,   3),   S(  5,  -6),   S(  5,  -1),
            S(-21, -32),   S(-21, -27),   S(-12,  -9),   S(-17, -10),   S( 11,   8),   S( -8,  -7),   S(  5,  -8),   S(-11, -12),
            S(  6, -12),   S( -4,  -2),   S(-15, -16),   S( 13,   6),   S(  0,   3),   S(-23,   1),   S( -3,  -7),   S(-10, -14),
            S(  6,   6),   S( -1, -13),   S( -4,  -5),   S(-24, -13),   S( 19,  19),   S(-28, -10),   S(  0,  -1),   S(-17, -28),
            S( -5, -11),   S( -5, -10),   S(-14,  -1),   S( 34,  48),   S(-13, -15),   S(  9,  10),   S( 11,  15),   S(-11,  -9),
            S( -4,   3),   S(-14, -19),   S(-12, -16),   S( -6,  13),   S( 10,  20),   S(  3,  31),   S( 11,   9),   S( -8, -14),
            S( -6, -27),   S(-23, -37),   S( 17,  37),   S( -6,   8),   S( -5,   0),   S( 12,   3),   S( -9, -19),   S(-11, -13),
            S( 17,  15),   S( -9, -26),   S( -2, -26),   S( 29,  43),   S( 26,  46),   S(  8,  15),   S( 13,   1),   S(  0,  -7),

            /* queens: bucket 11 */
            S( -1,   0),   S(-10, -20),   S(-15, -13),   S(-15, -24),   S(-11, -21),   S( -3,  -7),   S(-12, -13),   S(-26, -45),
            S( -3, -12),   S(  6,  12),   S(-51, -51),   S(-36, -22),   S(-10,   0),   S( -8,   0),   S(  8,  10),   S(-20, -19),
            S(-11, -22),   S( -5, -20),   S(  0, -34),   S(-12, -12),   S(  4,   5),   S( -7,   9),   S( 28,  29),   S(-12,   2),
            S(  2, -17),   S(-27, -24),   S(  5,  -5),   S( 10,  31),   S( 15, -10),   S(  1,  12),   S( 26,  35),   S(  5,   5),
            S(  0,   7),   S(  2,  -6),   S(-19, -32),   S( 19,  14),   S( 18,  12),   S( 30,  75),   S( 32,  59),   S(  0,   9),
            S( -9, -31),   S( 25,  31),   S( -7, -12),   S( 21,  17),   S( 33,  19),   S( 70,  77),   S( 21,   6),   S(-16, -22),
            S( -2, -13),   S( -2,   0),   S(  2,  20),   S( -9,  -3),   S(  5,   6),   S( 31,  31),   S( 28,  26),   S(-12, -22),
            S(-12, -34),   S(  0,  -8),   S( -5,  -7),   S( 13,   4),   S( 14,  28),   S(-28, -33),   S( 27,  25),   S(-10, -38),

            /* queens: bucket 12 */
            S(  6,   4),   S(  4,  10),   S(  2,   4),   S( -8,   3),   S( -9, -12),   S(  0,  -2),   S(  3,  10),   S(  0,  -4),
            S(  4,   9),   S(-12, -27),   S( -6,  -4),   S( -8, -13),   S( -9, -17),   S(  1,   2),   S( -4, -21),   S( -5, -15),
            S(  0,  -4),   S( -5, -10),   S( 16,  22),   S( -3, -13),   S( -3,  -1),   S( -9, -17),   S( -7, -20),   S(-13,  -9),
            S(  4,  13),   S(  8,  15),   S( 11,  10),   S(  0,   2),   S( 11,  22),   S( -5, -13),   S(  4,   2),   S( -2,  -5),
            S(  7,   4),   S( 13,  29),   S( 31,  57),   S( -4,  15),   S( -2,  15),   S(  7,  16),   S(-13, -31),   S(  3, -10),
            S( 10,  24),   S( -8, -10),   S( 49,  70),   S(  5,  15),   S(  5,  21),   S( -2,  -8),   S(  0,  -6),   S( -6, -18),
            S(  5,  -1),   S( -1,  18),   S( 17,  29),   S( 17,  21),   S(  7,  13),   S(  5,  18),   S(  4,   5),   S( -7,  -5),
            S( -2, -30),   S(-24, -39),   S( -7,   2),   S(-17, -42),   S(  7,  -5),   S( -7, -16),   S( -5, -12),   S( -8, -10),

            /* queens: bucket 13 */
            S( -6, -10),   S( -4, -20),   S(  4,  -4),   S( -6,  -7),   S( -7, -16),   S( -4,  -9),   S( -8, -11),   S(  0,  -2),
            S( -2,   1),   S(  5,  17),   S(  8,  20),   S( -4,   2),   S(-10,  -8),   S(  1,   5),   S( -2,   4),   S(-10, -18),
            S( -2,  -5),   S( -4,  -9),   S(  9,  29),   S( -6,   1),   S( -3, -12),   S( -9, -10),   S( -8, -22),   S(-13, -20),
            S( -7, -15),   S( -4,   0),   S( 11,  15),   S( 20,  32),   S( 25,  49),   S(-15, -27),   S( -7, -18),   S(-10, -18),
            S(-13, -22),   S( 20,  34),   S( 16,  61),   S( 20,  41),   S( 33,  61),   S( -6, -21),   S( -3,  -9),   S( -3,  -8),
            S( -5, -10),   S( -3,   2),   S( 48,  78),   S( 16,  38),   S(  7,  19),   S( -2,  -5),   S(  8,  17),   S( -7, -13),
            S( -6,  -9),   S( 11,   7),   S( 15,  25),   S( 21,  35),   S(  3,  13),   S(-10, -22),   S( -2, -18),   S(  5,  10),
            S(-22, -47),   S( -3, -19),   S( -1, -25),   S(-12, -16),   S(  4,   7),   S( -2,   7),   S(-11,  -7),   S( -6,  -9),

            /* queens: bucket 14 */
            S(  1,   8),   S(  2,   1),   S(  2,  -2),   S( -6, -12),   S(  3,  -3),   S(  1,   7),   S( -1,  -9),   S(-11, -19),
            S( -9, -21),   S( -4,   2),   S(  1, -12),   S( -1,  -6),   S( -7,  -8),   S(-11, -26),   S( 10,  27),   S( -6, -10),
            S( -1,   6),   S(-14, -14),   S( 10,  19),   S( -9,  -7),   S(  0,  -7),   S(  5,   4),   S(  7,   6),   S(  1,  -5),
            S( -8, -14),   S(  3,   1),   S( -7,   3),   S( 16,  27),   S( 11,  12),   S( -2,   6),   S( 14,  37),   S(  8,  10),
            S( -2,  -1),   S( -1,  -3),   S(-12,   1),   S( 30,  51),   S( 10,  33),   S( 22,  30),   S(  7,   4),   S(  4,   6),
            S( -1,  -4),   S( -6,  -3),   S( 13,  29),   S( 11,  14),   S(  6,  22),   S( 23,  61),   S(  6,  17),   S( -3,  -7),
            S(  4,   6),   S(  8,  14),   S(  3,  12),   S( 14,  21),   S( 11,  18),   S( 11,  20),   S( 22,  22),   S(  8,  16),
            S(  1,   7),   S(  5,  20),   S(-10,  -5),   S( 20,  12),   S( -3,   1),   S(  3,   0),   S( -2,   0),   S( -9, -20),

            /* queens: bucket 15 */
            S( -1,   0),   S(-10, -30),   S( -1,  -1),   S( -1,  -4),   S( -5,  -9),   S( -8, -17),   S(-10, -14),   S(  6,   1),
            S(  2,   4),   S( -3,  -5),   S(-15, -30),   S( -1,  -3),   S( -8,  -2),   S( -6, -12),   S( 10,  20),   S(  5,   0),
            S(  5,  -6),   S( -5, -12),   S( -2,  -1),   S( -3, -10),   S( -2,  -5),   S(  7,  26),   S( -1,  -2),   S( -1,  -7),
            S( -3,  -3),   S(  0,  -4),   S(-13, -13),   S( 14,  20),   S( -1,   9),   S( -2,   2),   S( 12,  21),   S(  7,  13),
            S(  0,   4),   S(  0,  -1),   S( -6, -16),   S(  8,  25),   S( 14,  24),   S(  6,  -3),   S( -4, -10),   S( -6, -12),
            S(  0,   7),   S( -1,  -4),   S(  3,   5),   S(  5,  13),   S( -3,   6),   S( 25,  42),   S(  5, -13),   S(  2,  -4),
            S( -3,  -5),   S(  1, -13),   S( 10,  24),   S(  5,   4),   S( 12,  25),   S( 17,  40),   S( 17,  25),   S(  5,   9),
            S(  4,  -3),   S( -5,  -1),   S( -1,  -5),   S( 23,  36),   S( 10,  12),   S( -2,  -7),   S(  3,  -3),   S(-16, -39),

            /* kings: bucket 0 */
            S( 69,  -8),   S( 46,  50),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 50,  29),   S(130,  54),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 37,  15),   S(-12,  27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 80,  42),   S( 64,  48),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-16,  43),   S( -5,  28),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60,  62),   S( 63,  48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-13,  53),   S(-29,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 21,  87),   S(-42,  60),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26, -68),   S( 62, -24),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -19),   S( 24,  16),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 59, -19),   S( 19,  -3),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 25,  31),   S( 18,  30),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 63,  16),   S( 30,  14),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 26,  47),   S(  6,  52),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 71,  33),   S( 41, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 48,  71),   S( -7,  31),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -128),  S( -4, -52),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71, -99),   S(-110,   1),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 23, -51),   S(-41, -45),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-30, -35),   S(-73, -11),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-13, -32),   S(-31, -29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-39, -14),   S(-95,  21),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -1, -34),   S(-22, -102),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-80,   7),   S(-18, -79),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -117),  S(-68, -36),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -215),  S(-21, -108),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-54, -57),   S(  8, -63),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-66, -82),   S(-30, -115),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -5, -53),   S(-106, -14),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 16, -114),  S(-83, -79),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-116,  -3),  S(-50, -106),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-57, -66),   S(-20, -232),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -4, -32),   S(-29,  15),   S( 12,  -5),   S( -5,  33),   S( 33, -19),   S( 49,   9),   S( 48, -13),   S( 57,  -2),
            S(-15, -16),   S(-37,   6),   S( -5,  -9),   S(  4,  -8),   S( 17,   4),   S(  6,  10),   S( 35,  -9),   S( 29,  17),
            S( 10, -29),   S(  1, -29),   S( 39, -29),   S( 11, -18),   S( 23,  -4),   S( 11,  28),   S(  5,  45),   S( 36,  15),
            S( 20, -19),   S( 24,  13),   S( 59, -33),   S( 44, -10),   S( 21,  45),   S(-17,  93),   S( 15,  80),   S( 65,  58),
            S(101, -57),   S(116, -30),   S( 98, -17),   S( 28,  40),   S( 62, 118),   S( -3, 136),   S(  9, 172),   S( 72, 112),
            S(-219, -88),  S(-120, -149), S(-15, -181),  S( 26,  35),   S( 77, 226),   S( 75, 177),   S(107, 175),   S( 78, 137),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39,  17),   S(-45,  29),   S(-17,   4),   S(-45,  65),   S(-10,  -2),   S( 22,   1),   S( 13,   1),   S( 18,  23),
            S(-58,  20),   S(-40,  13),   S(-32,   7),   S(-12,   6),   S(  8,   8),   S( -9,   0),   S( -2,   0),   S( -9,  17),
            S(-43,  24),   S(-13,  16),   S(-18,   6),   S( 15, -16),   S(  5,  16),   S(-13,  15),   S(-28,  24),   S(-10,  24),
            S(-27,  43),   S(  9,  23),   S( -8,  19),   S(  7,  34),   S( 12,  28),   S(-25,  42),   S(  2,  35),   S( 34,  54),
            S( 14,  41),   S( 75,  -1),   S(102, -32),   S( 78, -20),   S( 23,  26),   S(-18,  51),   S(-14,  73),   S( 38,  85),
            S( 65,  40),   S(-23, -36),   S(  0, -100),  S(-15, -100),  S(-41, -80),   S(-10,  20),   S( 67, 180),   S( 72, 220),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43,  41),   S(-35,  25),   S(-17,   6),   S( -9,  21),   S(-29,  30),   S(-12,  12),   S(  1, -13),   S(-10,  24),
            S(-56,  35),   S(-43,  28),   S(-30,   6),   S(-24,  23),   S(-25,  19),   S(-32,   5),   S(-14, -12),   S(-42,  17),
            S(-42,  48),   S(-39,  54),   S( -8,  19),   S(-11,  22),   S(-26,  27),   S(-25,   6),   S(-29,   6),   S(-33,  14),
            S(-27,  86),   S(-37,  75),   S( -4,  39),   S(  7,  40),   S( -5,  36),   S(-20,  11),   S(  4,  23),   S( 18,  14),
            S(-20, 130),   S(-52, 126),   S(-18,  21),   S( 39, -32),   S( 77,   2),   S( 86,  -3),   S( 66,   0),   S( 50,   6),
            S( 15, 246),   S( 16, 199),   S( -3,  86),   S(  8, -92),   S(-34, -157),  S(-71, -156),  S(-13, -74),   S( 41,   7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  21),   S( -5,  16),   S( 10,   7),   S(  4,  31),   S( -1,  46),   S( 31,  25),   S( 18,   0),   S( 10, -14),
            S( -5,  25),   S( -3,  26),   S(  0,  10),   S(  2,  18),   S( 15,  18),   S( 15,   3),   S(  9, -12),   S(-19,  -7),
            S(  5,  39),   S(-10,  56),   S( 14,  19),   S(  9,   7),   S( 23,  -8),   S( 17,  -9),   S(  4, -21),   S(-16, -10),
            S(  5,  94),   S(-11, 106),   S( 18,  66),   S( 20,  36),   S( 37,   1),   S( 39, -30),   S( 25,   0),   S( 40, -20),
            S( 13, 152),   S(-13, 176),   S(-23, 157),   S(  3, 114),   S( 47,  41),   S( 79,  -6),   S(119, -35),   S( 93, -38),
            S( 98, 112),   S( 40, 244),   S( 50, 252),   S(-23, 227),   S(-29, 103),   S(  1, -178),  S(-75, -237),  S(-162, -180),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 90, -24),   S( 10,  14),   S(-40,   8),   S( -6, -10),   S( -9,   7),   S(  8, -13),   S(-15,   1),   S(-64,  45),
            S( 35,  -2),   S( -6,  24),   S( 18,  -9),   S(-33,  16),   S(-30, -11),   S(  5, -26),   S(-35, -22),   S(-48,   6),
            S( 67,  -8),   S(122, -33),   S( 16, -12),   S(-15,  -4),   S(-70,  15),   S( -5,   2),   S(-70,  23),   S(-63,  28),
            S(-126, -55),  S(-81, -76),   S( 46, -60),   S(-29,  11),   S(-12,  17),   S(-95,  75),   S(-25,  49),   S(-65,  80),
            S(-38, -68),   S(-65, -109),  S(-47, -79),   S( 54,   9),   S( 41,  98),   S( 25,  85),   S( 30,  57),   S( 22,  95),
            S(  2, -60),   S(-10, -52),   S(  2, -59),   S(  5,  46),   S( 66, 104),   S( 74, 141),   S( 24, 147),   S( 27, 131),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-72,  53),   S(-65,  54),   S(-23,  23),   S( 53,  25),   S( 78, -11),   S( 23,  -3),   S(-18,   4),   S(-72,  54),
            S(-71,  40),   S(-49,  42),   S(-11,  14),   S( -9,  21),   S(-17,  17),   S(-15,   1),   S(-55,   4),   S(-72,  31),
            S(-41,  32),   S(-32,  52),   S( 15,  32),   S(  7,  41),   S(-40,  55),   S(-61,  32),   S(-60,  30),   S(-65,  45),
            S(-46,  50),   S( -2,   9),   S(-54, -21),   S( -2, -21),   S( -2,   3),   S(-67,  34),   S(  6,  27),   S(-14,  55),
            S( 58,   8),   S(-13, -31),   S( 24, -96),   S( 24, -83),   S( 18, -33),   S(-20,  31),   S(-24,  73),   S(-71, 118),
            S( 26,  50),   S( 26, -10),   S(-36, -64),   S(-16, -49),   S(-16, -40),   S( 30,  35),   S( 46, 141),   S( 64, 150),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-92,  43),   S(-72,  23),   S(-16,   1),   S( -2,  15),   S(-24,  46),   S(  9,  15),   S(  9,   7),   S(  0,  28),
            S(-67,  19),   S(-65,  19),   S(-35,   4),   S( 26,  10),   S(-16,  28),   S(-12,  10),   S(-14,  11),   S( -8,  14),
            S(-60,  38),   S(-85,  45),   S(-58,  40),   S(-20,  40),   S(-15,  49),   S( 25,  15),   S(  9,  12),   S( -7,  18),
            S(-111, 103),  S(-50,  54),   S(-16,  29),   S(-13,  23),   S( -9, -25),   S(  2, -37),   S( -8,   0),   S( 31,   3),
            S(-31, 112),   S(-51,  66),   S( 46,   1),   S(-21, -27),   S(  8, -77),   S(-46, -67),   S(-10, -31),   S( 78,  -7),
            S( 83,  81),   S( 94,  80),   S( 54,  28),   S( 76, -100),  S(-10, -103),  S(-45, -38),   S(-12, -46),   S( 72,   8),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,   8),   S(-38, -20),   S(  5, -32),   S(-31,  28),   S( 12,  16),   S( 77, -22),   S( 52, -24),   S( 67,  -6),
            S(-54,   6),   S(-67,   5),   S(-34, -13),   S(-20,  -3),   S( 16,  -3),   S( 41, -24),   S( 25, -13),   S( 51, -16),
            S(-49,  24),   S(-91,  42),   S(-33,   5),   S(-39,   6),   S( -2,   4),   S( 26, -14),   S( 55, -18),   S( 53, -16),
            S(-41,  62),   S(-86,  78),   S(-32,  47),   S(-32,  38),   S(  1,  -2),   S( 48, -63),   S( -5, -60),   S( 14, -97),
            S( -9,  69),   S(-45, 141),   S(  6, 116),   S( 11,  89),   S( 24,  14),   S( -6, -71),   S(-52, -128),  S(-29, -100),
            S(120, 105),   S( 58, 147),   S( 82, 132),   S( 87,  72),   S( 73,  10),   S( -6, -128),  S(-29, -94),   S( -9, -173),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24,  14),   S( 14,  24),   S( 39,  13),   S(-26, -55),   S(-42, -115),  S(-33, -13),   S( 18, -61),   S( 51, -38),
            S(-18, -47),   S(  0,  -7),   S(-72, -54),   S(-72, -33),   S(-42, -54),   S( 74, -73),   S(  0, -62),   S(-23, -40),
            S( 36, -95),   S( 14, -70),   S(-16, -54),   S(-64, -38),   S(-16, -23),   S( 18, -37),   S(-39, -23),   S( 14, -33),
            S( 25, -17),   S(-51, -47),   S( 30, -24),   S( 11,  -7),   S(-36,  30),   S( -7,  18),   S(-13,  27),   S( -3,  24),
            S( 11, -13),   S( -5, -36),   S( -2,  20),   S( 19,  97),   S( 65, 125),   S( 36, 117),   S( 15, 100),   S(-11, 114),
            S( 16,  48),   S(  6,  38),   S( 17,  56),   S( 46, 107),   S( 39,  44),   S( 54, 184),   S( 42, 104),   S(-35, 114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 37,  -2),   S( 58,  21),   S(  6,   4),   S(  1,  27),   S( 11, -20),   S( 13, -23),   S( 43, -54),   S( 10, -24),
            S( 42, -54),   S( 27, -53),   S( -1, -50),   S(-39, -25),   S(-32, -18),   S(-39, -30),   S(-31, -34),   S( -4, -34),
            S(-29, -36),   S(-48, -35),   S(-26, -74),   S(-66, -16),   S(  8, -43),   S(-14, -42),   S(-95, -16),   S( 19, -29),
            S(-71,  16),   S(-32, -57),   S(-16, -60),   S(-41, -28),   S(  1, -37),   S(-12, -16),   S( -9,  -4),   S( -6,  12),
            S( 11,  10),   S(  0, -18),   S(-23,  19),   S( 15,  20),   S( 10,  73),   S( 22,  53),   S(  7,  65),   S( -7,  57),
            S( 21,  43),   S( 26,  47),   S( 17, 108),   S( 19,  57),   S( 32,  92),   S( 10, 110),   S(  6,  85),   S( 33,  86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26, -55),   S( 22, -54),   S( -3, -20),   S(-18, -29),   S( 36,  17),   S( 63,  14),   S(  9,   7),   S( 12,  -8),
            S(-17, -50),   S(-96, -33),   S(-14, -50),   S( 34, -58),   S( -3, -14),   S( -6, -22),   S( 15, -43),   S( 19, -45),
            S(-17, -51),   S(-101, -24),  S(-51, -37),   S( -8, -29),   S( -8, -48),   S(  4, -59),   S( -8, -66),   S( 45, -60),
            S(-26,  -4),   S(-14,  -7),   S(-55, -18),   S(-59, -40),   S(-16, -65),   S(-53, -53),   S( -2, -47),   S(-13, -41),
            S( -4,  16),   S( 22,   8),   S( 20,  29),   S(-14, -12),   S( 28,  21),   S( 24,  25),   S(-32,  20),   S( 33,  -5),
            S(-21,  31),   S( 11,  61),   S( 20,  70),   S( 17,  70),   S( 19,  94),   S( 16,  54),   S(-10,  37),   S( 20,  49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -52),   S( 10, -52),   S(-16, -47),   S( -1, -13),   S(  3, -10),   S( 80,   2),   S( 37,  -9),   S( 51,   1),
            S( -3, -67),   S(-56, -56),   S(-29, -66),   S(-13, -58),   S(-35, -31),   S(  4, -43),   S( 32, -47),   S( 52, -75),
            S(-48, -24),   S(-76,  -4),   S( -9, -27),   S(-36, -14),   S(-53, -43),   S( 16, -55),   S( 22, -109),  S( 77, -106),
            S(-68,  39),   S(-79,  36),   S(  4,  38),   S( 26, -20),   S(-23, -13),   S( -8, -46),   S(-68, -60),   S( 59, -105),
            S(-20,  19),   S(-19,  72),   S( -4,  89),   S( 12,  56),   S( 29,  62),   S(  2,   7),   S( 19,  29),   S( -8, -16),
            S(  5, 101),   S( 37,  60),   S(  8,  66),   S( 16,  77),   S(  2,  42),   S( 47,  95),   S( 10,  34),   S( 36,  45),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -88),   S( 23, -69),   S( 22,   3),   S( -2, -12),   S(-11, -35),   S(-28, -77),   S(  8, -42),   S(  3, -55),
            S( 41, -93),   S( 18, -75),   S(-32, -73),   S(-26, -70),   S(-16, -96),   S(-32, -64),   S(  0, -86),   S(-14, -45),
            S(-12, -67),   S(  8, -58),   S(-29, -109),  S(-31, -78),   S(-12, -24),   S(-25, -53),   S(-49, -62),   S(-56, -59),
            S(-23, -72),   S( 11,  19),   S( -9,  -4),   S(  2,   7),   S( -1,  37),   S(  2,  47),   S( 11,   3),   S( -5,  11),
            S( 23,  38),   S( -5,   0),   S( 14,  47),   S(  8,  31),   S( 35,  83),   S( 15,  83),   S(  2,  80),   S(  6,  67),
            S(  8,  17),   S( -2,  34),   S( 14,  57),   S( 14,  58),   S( 30, 106),   S( 31, 106),   S(-12,  -1),   S(-26, -23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-18, -81),   S( 30, -72),   S( 13, -11),   S(  3,  -3),   S( 25,  19),   S(-40, -33),   S( 11, -79),   S(-22, -66),
            S( 21, -129),  S( 44, -84),   S( -6, -93),   S( 11, -33),   S(-14, -24),   S( -8, -82),   S(  9, -109),  S(-12, -65),
            S( 20, -81),   S(-14, -59),   S(  5, -93),   S( 12, -90),   S(-28, -53),   S( 38, -74),   S(  2, -80),   S( 54, -79),
            S( -2, -58),   S( 10, -26),   S( 10, -27),   S( -3,  42),   S( 26,  35),   S(-25,   0),   S( -4, -32),   S( 21, -48),
            S(-14,  18),   S(  6,  33),   S( -9,  -3),   S( 21,  50),   S( 27,  46),   S( 22,  70),   S( 27, 105),   S(-11,  69),
            S(  7, 108),   S( 22,  37),   S( -4,  19),   S( 23,  59),   S( 22,  74),   S( 13,  53),   S(-11,  37),   S(-15,  64),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -100),  S( -2, -65),   S( -6, -43),   S(  8,  13),   S( -3,  -3),   S(  1,   4),   S( 20, -64),   S( -9, -42),
            S( 20, -112),  S(-35, -107),  S(-29, -78),   S(-34, -81),   S(-12, -62),   S( 13, -57),   S( 15, -53),   S( 12, -106),
            S( 14, -87),   S(-25, -69),   S(-17, -78),   S( 13, -68),   S( -8, -42),   S( 15, -85),   S(-14, -87),   S( 36, -68),
            S(-13, -39),   S(-13, -86),   S(-22,  -7),   S(-29,   0),   S( 18, -84),   S( -2, -27),   S( -5, -50),   S( 10, -11),
            S(-19, -31),   S(  9,  15),   S(  2,  54),   S( -4,  24),   S( 25,  80),   S(  7,   8),   S( 13,  31),   S( 10,  56),
            S(  0,  24),   S( 11,  74),   S( 22,  52),   S( 16,  76),   S(  4,  38),   S(  8,  50),   S( 28,  99),   S( 29,  99),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -44),   S( -9, -75),   S(-10, -34),   S( -5, -18),   S(-18, -33),   S(-27, -41),   S( 10, -37),   S(-10, -109),
            S(-22, -73),   S(-16, -84),   S(-31, -80),   S(-16, -24),   S( -9,   1),   S( -5, -24),   S( -3, -21),   S( 24, -142),
            S(-43, -25),   S(-23, -61),   S(-66, -80),   S( -7, -70),   S(-33, -30),   S( 10, -54),   S(  2, -52),   S(  5, -46),
            S( 15, -37),   S(-36, -11),   S( -7,  21),   S(-23,  18),   S( 22,   7),   S(-11, -19),   S( -6, -32),   S( -7,  32),
            S( 11,  69),   S(-11,  56),   S(  2,  86),   S( 22,  80),   S( 26,  67),   S( 11,  65),   S(  3,  10),   S(  4,  18),
            S(-27,  20),   S( -3,  -6),   S(  7,  55),   S( 25,  52),   S( 18,  64),   S( 11,  53),   S( 13,  56),   S( 18,  50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-66, -18),   S(-30, -24),   S(-36,  27),   S( 11,   8),   S(-33,   1),   S(-27,  13),   S( -6, -25),   S(-82, -57),
            S( -6, -42),   S(-35,  -2),   S(-13, -43),   S( -4, -17),   S( -7,  18),   S(-25,  16),   S(-20, -64),   S(-25, -40),
            S( -9, -39),   S( 20, -22),   S(  7,   1),   S( 33,  26),   S(  9,   1),   S( 11,   4),   S(-38,  32),   S(-30, -27),
            S( 13,   8),   S( 51,  48),   S( 18,  41),   S( 60,  11),   S( 29,  25),   S( 15,  16),   S( 70, -29),   S( -7,  -5),
            S( 37,  45),   S(-17,  69),   S( 45,  92),   S( 72,  37),   S( 75,  37),   S( 21,  33),   S( 28, -10),   S( 14, -43),
            S( 90, -73),   S( -2,  69),   S(142,   4),   S( 91,  19),   S( 65,  61),   S(-15,  79),   S( 37, -19),   S(-20,  12),
            S( 32,  -2),   S(-12, -44),   S( 18,   4),   S( 95,  69),   S( 38,  11),   S( 34,  14),   S(-59,  22),   S(-46,  14),
            S(-127, -151), S( -1,   1),   S(  8,  29),   S( 20,  26),   S( 18,  24),   S( 17,  23),   S(-25,  -1),   S(-15,  24),

            /* knights: bucket 1 */
            S( 51,   5),   S(-67,  26),   S(-24,  14),   S(-40,  30),   S(-44,  58),   S(-25, -19),   S(-27, -25),   S(-26, -20),
            S(-29,  16),   S(-52,  75),   S(-49,  51),   S(-16,  36),   S(-17,  26),   S(-14,  23),   S(-25,  -2),   S(-17, -56),
            S(-39,  16),   S(  8,  -1),   S(-20,  17),   S(-13,  46),   S(-10,  49),   S( -8,  20),   S(-45,  31),   S(  4,   9),
            S(-23,  96),   S( 35,  10),   S(-19,  63),   S( -8,  49),   S( -1,  58),   S( -8,  58),   S(-11,   7),   S(-23,  32),
            S( 42, -15),   S( -5,  16),   S( 22,  61),   S( 16,  62),   S( 34,  43),   S( 10,  54),   S( -7,  43),   S(-24,  73),
            S( 26,  28),   S( 70,   8),   S( 95,  29),   S( 93,  42),   S( 43,  44),   S(-24,  63),   S( 39,   6),   S( 32,  18),
            S(  3,   1),   S( 10, -22),   S( 37, -26),   S(  5,  58),   S( -5,  39),   S(  9,   6),   S( 13,  60),   S(-36,  28),
            S(-194, -43),  S( 25, -23),   S(-45, -86),   S(-30,  -3),   S( -5,  25),   S( 28,  25),   S(  3,  33),   S(-74,   3),

            /* knights: bucket 2 */
            S(-61,   3),   S(-43,  21),   S(-36,  16),   S(-18,  19),   S(-22,   9),   S(-46, -10),   S(-33, -10),   S(-38, -24),
            S(-32, -10),   S( -2,  35),   S(-25,  18),   S(-13,  22),   S(-25,  27),   S(-25,  11),   S(  0,   3),   S(-43, -12),
            S(-38,  56),   S(-20,  20),   S(-20,  16),   S(-24,  56),   S(-16,  40),   S(-23,  14),   S(-29,  12),   S( -3,  -7),
            S(-15,  70),   S( -4,  54),   S(-24,  78),   S(-20,  81),   S(-29,  67),   S( -2,  43),   S( -9,  33),   S( -8,  33),
            S(-15,  55),   S( -8,  73),   S(  8,  61),   S( 22,  59),   S(-22,  74),   S( 21,  57),   S(-11,  58),   S( 22,  -7),
            S(-55,  71),   S(-26,  57),   S(-47,  90),   S( 39,  22),   S( 41,  27),   S(118,  -9),   S( 56,  16),   S( -5,  -4),
            S(  8,  42),   S(-61,  60),   S( 48,   5),   S( 56, -19),   S(-33,  42),   S( -9,  -9),   S( 54,  -2),   S( 28, -27),
            S(-64,  24),   S( 11,  50),   S( -8,  45),   S(-30, -60),   S( -5, -38),   S(-73, -72),   S( 48,  11),   S(-159, -49),

            /* knights: bucket 3 */
            S(-52,   2),   S(-17, -40),   S(  2, -20),   S( -1,  -1),   S(  4,  -7),   S( -7, -13),   S(-26, -32),   S(-25, -70),
            S(-26, -28),   S( 11, -12),   S( 10,  -3),   S(  4,   1),   S(  4,   1),   S( 20, -20),   S( 29, -50),   S( 17, -60),
            S( -7,  -9),   S( -3,   6),   S( 10,  18),   S( 14,  39),   S( 22,  29),   S(  4,  19),   S( 12, -11),   S( 21, -37),
            S(  7,   5),   S( 25,  29),   S( 20,  49),   S( 14,  58),   S( 20,  70),   S( 29,  60),   S( 35,  52),   S( 17,  41),
            S( -7,  40),   S( 15,  40),   S( 34,  63),   S( 42,  86),   S( 23,  75),   S( 39,  89),   S( -7,  86),   S( 50,  83),
            S(-23,  36),   S( 14,  52),   S( 42,  48),   S( 13,  79),   S( 57,  70),   S(120,  71),   S( 45,  81),   S( 26, 101),
            S( -6,  30),   S(-36,  58),   S(-30,  69),   S( 15,  65),   S( 41,  46),   S( 92,  39),   S( 46, -25),   S( 54,  -9),
            S(-162,  47),  S(  0,  82),   S(-53,  87),   S( 47,  41),   S( 60,  67),   S(-35,  51),   S(-34, -57),   S(-82, -130),

            /* knights: bucket 4 */
            S( 16,  14),   S(-11, -12),   S(-80, -35),   S(-51,  -7),   S( -1,   5),   S(-18, -22),   S( 11, -24),   S(-28, -31),
            S( 20,  28),   S( -3, -22),   S(-25,   2),   S(-51,  17),   S(  8, -20),   S( 21, -52),   S( 11,   7),   S(-27,  14),
            S( -5, -15),   S(  3, -20),   S( 65,  16),   S( 75,   4),   S( 35,  28),   S( 69, -30),   S( -2, -15),   S(-18, -13),
            S(-14, -14),   S( 36, -25),   S( 39, -10),   S( 74,  -4),   S( 17,  36),   S(-15,  43),   S(-30,  19),   S(  4,   7),
            S( 23, -61),   S( 12, -20),   S( 87,  -4),   S( 20,  43),   S( 64,   9),   S( -7,  36),   S( 30, -14),   S(-43,  36),
            S( -5, -33),   S( -3, -26),   S( 34,   6),   S( 45,  25),   S(  0,  38),   S( 17,  50),   S( -4,  42),   S( 16, -25),
            S(-22, -36),   S(-36, -45),   S( 10, -17),   S( 24,  35),   S( 50,  50),   S(  0,  23),   S( 16,  52),   S(-39, -42),
            S( -1,   9),   S(-15, -51),   S( -6, -45),   S(  6, -18),   S( 16,  26),   S(-25,  10),   S( -1,  23),   S( -1,   7),

            /* knights: bucket 5 */
            S(  3,   6),   S( 15,  39),   S(-28,  31),   S(  6,   1),   S(-17,  32),   S(  8,  28),   S(-18,  20),   S( -9,  -6),
            S( 34,   8),   S( 37,  21),   S( 15,  -4),   S( -1,   4),   S( 27,  -7),   S( -3,   1),   S(-24,  55),   S(-60, -12),
            S(-18,  22),   S(-33,  15),   S( 12,   5),   S( 11,  26),   S( -2,  18),   S(  6,  15),   S( -1,   5),   S(-67,  32),
            S( 49,   7),   S( 18, -14),   S( 93,   6),   S(106, -17),   S(108,  -9),   S( 88,   0),   S(-15,  37),   S(  1,  53),
            S( 54,  -7),   S( 41,  -4),   S( 94,  -8),   S(155, -12),   S( 73, -17),   S( 49,  20),   S( -3,  18),   S(  6,  34),
            S(-14, -29),   S( 26, -38),   S(  5, -34),   S( 14, -16),   S( 31, -10),   S( 73, -16),   S(-12,  11),   S( 31,  32),
            S( -6,  -2),   S(-37, -76),   S( -6, -61),   S(-35, -33),   S(  4, -35),   S( 13,  13),   S(-20,  41),   S(  1,  27),
            S(-39, -53),   S(-29, -77),   S(  9,  -9),   S(-24, -31),   S(  6, -10),   S(-12,  12),   S(  9,  31),   S(-27,  21),

            /* knights: bucket 6 */
            S(-27, -38),   S(-39,  25),   S( -3,  -4),   S(-28,  45),   S(-26,  37),   S(  1,  28),   S(-32,  49),   S(-23,  19),
            S( 23, -16),   S(-33,  50),   S(-41,  12),   S( 18,  18),   S( 12,  18),   S(-15,  18),   S( 23,  29),   S(-42,  37),
            S(  5,   1),   S(  5,  25),   S(  8,  20),   S( 26,  31),   S( 15,  38),   S(-28,  30),   S( 27,  19),   S(-16,  28),
            S( 29,  40),   S( 73,   9),   S( 47,  22),   S( 51,  22),   S( 98,  -8),   S( 82,   4),   S( 35,   2),   S(-16,  41),
            S(-48,  53),   S( 67,  -2),   S( 84,  25),   S( 78,  21),   S(118, -25),   S( 10,  42),   S(128, -12),   S( 25,  24),
            S( 23,  19),   S( 25,   6),   S( 37,  21),   S( 33,  19),   S( 36, -20),   S( 23,   8),   S(-16, -12),   S( 40, -23),
            S(  3,  26),   S( 12,  35),   S( 31,  42),   S(  5,  -4),   S( 24,   8),   S( 13, -48),   S( -9, -11),   S( -7,  33),
            S( 19,  35),   S(  4,  30),   S( 11,  23),   S(  5,  29),   S( 25,  -9),   S( -1,  -7),   S(  4,   7),   S(-53, -67),

            /* knights: bucket 7 */
            S(-31, -25),   S(  0, -39),   S( 15,  -4),   S(-45,  27),   S( -2, -10),   S(-40,   6),   S( 15, -17),   S(-29,  22),
            S( 25, -63),   S( 11, -32),   S( -4, -11),   S( 13, -22),   S( 19,  -3),   S( 17,  21),   S(  0,  24),   S(-26,  17),
            S(-26, -25),   S(-52,  12),   S( 12,  -7),   S( 23,   7),   S( 42,  15),   S( 34,  10),   S( 20,   6),   S(-11,  38),
            S(-68,  16),   S(-13,   6),   S( 52,  -9),   S( 90,  -4),   S(112,  -7),   S( 92,  15),   S( 46,  17),   S( 65,  20),
            S(-19,  32),   S( 34,  -4),   S( 44,  15),   S( 73,  11),   S( 96,  14),   S(144, -13),   S(182, -14),   S( 44, -12),
            S(-23,  17),   S( 10,  34),   S(  7,   8),   S( 34,  22),   S(114, -23),   S(123, -20),   S(  1, -21),   S( -7, -46),
            S(-35, -22),   S(  5,  -9),   S(  2,   7),   S( 47,  32),   S( 35,  15),   S( 27,  10),   S(-24, -36),   S(-21, -33),
            S(-29, -19),   S(-18, -10),   S(  1,  38),   S(-13,  46),   S(  3,  -5),   S( 26,  12),   S( 17,  11),   S( -1, -22),

            /* knights: bucket 8 */
            S( -6, -14),   S(  4,   7),   S(  2,  11),   S(-29, -62),   S(  2,  40),   S(-18, -57),   S(  9,  28),   S(  1,  -7),
            S(-12, -50),   S(-11, -43),   S(  3, -27),   S(-12,   0),   S(-20,  13),   S(  0, -10),   S( -1, -14),   S(  2,  11),
            S(-17, -53),   S( -5, -24),   S( -7, -42),   S( 18,  11),   S( -8, -14),   S(-13,   2),   S( -3, -37),   S(  3, -19),
            S(-15, -69),   S( -7, -29),   S(  9,  30),   S(  5,  21),   S(-18,  -8),   S( -3,  25),   S(-23, -51),   S(-12, -32),
            S( -3, -25),   S(  2,  -9),   S( -3, -30),   S(-11,  12),   S(-26,   2),   S(-14, -34),   S( -3,  -3),   S( 13,  22),
            S(  2,  10),   S(  7, -10),   S( -9, -13),   S(  1, -33),   S(-16,   3),   S( -3,   2),   S( -2,   6),   S( -6,  -5),
            S(  0,  14),   S(  6, -17),   S(-10,   4),   S( -2,  -1),   S( -3, -16),   S( 13,  25),   S( -2,  10),   S(  4,  -7),
            S(  0,  -3),   S( -7, -11),   S( -7,   6),   S(  8,  15),   S( -6, -10),   S( -1,   8),   S(  1,   7),   S( -6,  -1),

            /* knights: bucket 9 */
            S(-16, -45),   S( -1,  -5),   S( -8, -50),   S(-12, -57),   S(-24, -25),   S(-18,  -8),   S( 13,  32),   S(  0, -13),
            S(-11, -28),   S(-12, -45),   S(-19, -97),   S(-17, -47),   S( -3, -25),   S(-22, -51),   S(  0,  20),   S( -6,   1),
            S(-14, -49),   S(-14, -40),   S(-24, -51),   S( -9, -53),   S(-15,  10),   S(  9, -14),   S( -4,  25),   S( -4,  -5),
            S(-29, -42),   S(-22, -54),   S( -5, -48),   S(-10, -53),   S(-17, -48),   S(-18,   2),   S(-23, -57),   S(  2,  47),
            S(  5,  34),   S( -8,  -7),   S( -6, -33),   S(-21, -41),   S(-12, -38),   S(-10,  10),   S(-18,  -8),   S( -7,  12),
            S(-13, -41),   S(-38, -65),   S(-23, -37),   S( 17,   9),   S( -4,   8),   S(  2, -13),   S(  4, -10),   S( -9,  -5),
            S( -5,  -6),   S( -1,   3),   S(-13, -20),   S(-19,   8),   S( -4,   6),   S( -3,  18),   S( -6,  27),   S( -7,  -3),
            S(  1,   9),   S(  1,   4),   S( -2,  25),   S( -3,   0),   S( -7,   7),   S( -3,   5),   S(  2,   2),   S( -2,  14),

            /* knights: bucket 10 */
            S( -3, -13),   S(-10, -15),   S(-13, -33),   S(-22,  -2),   S( -8, -51),   S(  5, -48),   S(  1,   7),   S( -2,  15),
            S( -7, -15),   S( 27,   7),   S(-15, -15),   S( -5, -47),   S( -3,  -4),   S(-33, -57),   S(-15,   8),   S(  4,  39),
            S(  4,  18),   S( -4, -25),   S(  2,  14),   S( 18, -31),   S(-20, -54),   S(  0, -12),   S( -1, -16),   S(-11,  11),
            S(  1,   1),   S(-15, -37),   S(  5, -16),   S( -2,  -2),   S(-20,  -5),   S(-12, -20),   S( -4, -55),   S(-10, -24),
            S(-13, -14),   S( -7, -30),   S(-18,  15),   S( -6, -45),   S(  8,  10),   S(  0, -48),   S( 16,  25),   S(  9,  16),
            S(  3,  26),   S(-11, -16),   S(  0,   7),   S(-20,  15),   S(-14, -26),   S(-22, -26),   S(-15,  14),   S(-11,   4),
            S(  5,  16),   S( -1, -20),   S(-17, -43),   S(  2, -38),   S(-11,  14),   S(-18, -32),   S(-14,   0),   S( -5, -15),
            S(  4,   2),   S(  3,  14),   S(  4,  30),   S(  2,  -4),   S(  2,  19),   S( -1,  16),   S(  1,  -7),   S(  5,  15),

            /* knights: bucket 11 */
            S(-10, -44),   S(-23, -23),   S(  3,   6),   S( 24,  47),   S(-59, -79),   S( -1,  -8),   S( -4, -15),   S( 24,  56),
            S(  2,  -3),   S(-26, -39),   S( -5, -46),   S( 25, -12),   S( -2,  29),   S(-11, -32),   S(-10, -19),   S( -1,  -3),
            S(-13, -31),   S( -9, -16),   S(-20, -18),   S(  3,   9),   S(-18,  48),   S( 28,  17),   S(-12,  -9),   S(-13, -32),
            S(-27, -24),   S( 17, -21),   S( 14, -18),   S( -5,  17),   S( 17, -11),   S(  6, -13),   S( 11,  20),   S( -8, -19),
            S(-16,  -8),   S( -6, -56),   S(-26,  20),   S(-13,  -3),   S( 50,  55),   S( 24,  44),   S(-14, -74),   S(-18, -44),
            S(-12, -39),   S(-21, -54),   S(  8,  14),   S( 14,   7),   S(  4,  45),   S(-11, -18),   S(  1, -21),   S(  2,  34),
            S( -9, -24),   S( 10,  51),   S(-10, -22),   S(  4,  -4),   S(  2,  -1),   S( -4, -58),   S( 10, -32),   S( -3,  -2),
            S( -3, -12),   S(  6,  11),   S(  1,   9),   S( -3,  13),   S( -8, -12),   S(  0, -17),   S(  8,  21),   S( -2,  -4),

            /* knights: bucket 12 */
            S(-16, -43),   S( -9, -28),   S( -4, -38),   S(  0,   7),   S(  2,  24),   S( -4,  -8),   S(  0,   9),   S( -2,  -3),
            S( -1,  -3),   S(  1,   2),   S(  2, -31),   S( -6,   0),   S( -2,   9),   S(  1,  18),   S( -4, -11),   S(  1,  -4),
            S( -2, -11),   S( -9, -28),   S( -8, -19),   S(  2,   3),   S( -7, -14),   S(  4,  30),   S( -6, -11),   S( -5, -12),
            S( -1,  -2),   S(  2, -18),   S( -5,  31),   S(  6,  17),   S( -4,  -7),   S(  4,  18),   S( -4,   3),   S(  4,   7),
            S( -2,   3),   S( -8, -33),   S( -7, -30),   S(  0,   1),   S(  6,  31),   S( -7, -16),   S( -9, -12),   S(-12,  -2),
            S( -4,  -2),   S(  4,  -3),   S( -2, -12),   S(  4,  -5),   S(  3,  15),   S( -8, -17),   S( 13,  13),   S( -3,   1),
            S( -2,  -6),   S( -1,   3),   S(-11,  -4),   S( -3,   4),   S(  5,  26),   S( -7,  -4),   S( -5, -17),   S( -1,  -2),
            S(  0,   0),   S(  0,  -1),   S( -1,  -1),   S( -1,  -2),   S(  2,   2),   S(  5,   7),   S(  0,  -3),   S(  1,   5),

            /* knights: bucket 13 */
            S(  2,  -1),   S( -3, -15),   S( -7, -27),   S( -2, -10),   S( -8, -19),   S(  1,   5),   S( -8,  -6),   S(  2,   9),
            S( -3,   5),   S( -3, -11),   S(  4,  13),   S( -6, -14),   S( -9, -17),   S( -3,   2),   S(  2,  15),   S( -5, -14),
            S(  9,  13),   S(  0,   2),   S(  5,  24),   S(  4,   7),   S(  1,  31),   S( -6,   3),   S(  0, -12),   S(  4,  23),
            S( -3,   5),   S( -1,   0),   S( -6,   0),   S(  7,  37),   S( -2,   3),   S(  0,  41),   S(  4,  22),   S( 11,  25),
            S( -1,  10),   S(-14, -29),   S( -4,  27),   S( -8,  14),   S( -8,  16),   S( -3,  22),   S(  2,  21),   S( -6,  -5),
            S( -4, -10),   S(  3,   6),   S( -8,   1),   S(  8,  11),   S(-14,  -2),   S(-15, -14),   S(  2,  17),   S( -3, -11),
            S(  2,   6),   S(  9,  22),   S(  1,  26),   S( -5,   5),   S(  2,  19),   S(  0,  13),   S(  3,   5),   S( -1,   1),
            S(  1,   4),   S( -3,  -4),   S( -4, -12),   S( -3,  -3),   S( -3,  -5),   S(  3,   5),   S( -4,  -7),   S(  0,   1),

            /* knights: bucket 14 */
            S(  0,  -7),   S( -6,  -8),   S(  7,  27),   S( -4,  -7),   S( -9, -46),   S( -3,   4),   S(  4,  11),   S( -1,  -5),
            S( -2, -17),   S(-13, -25),   S( -3, -24),   S(  1,  22),   S(  3,  -7),   S( -1,  -5),   S(-10,  -2),   S(  8,  71),
            S(  0,  -7),   S(  1, -37),   S(  3,  19),   S( -5, -33),   S( -8,  -5),   S( 10,  18),   S(  0,  10),   S(  2,  10),
            S( -3,  -8),   S( -7, -31),   S(-22,  -3),   S(-12,  23),   S( -7,  33),   S( -8, -13),   S(  2,   3),   S(  0,  41),
            S( 11,  22),   S(-20, -45),   S(-12, -18),   S( -8,   0),   S( -5,  13),   S(  0,  11),   S(  3,  21),   S(  7,  13),
            S( -2,   4),   S( 10,  14),   S(  6,  12),   S(  4,  22),   S(  0,  15),   S(  6,  25),   S(  1,  23),   S( -1,   0),
            S( -1,   0),   S( -4, -10),   S(  7,  23),   S(  7,   1),   S(  4,  20),   S( -8, -37),   S(  3,   8),   S(  7,   7),
            S(  0,  -1),   S(  0,  -2),   S( -2,   4),   S(  2,   6),   S( -2,   1),   S(  0,  -1),   S(  1,   2),   S(  0,   0),

            /* knights: bucket 15 */
            S( -5, -28),   S(  1,  14),   S(  8,  32),   S( -5,   3),   S( -2, -14),   S( -5, -27),   S( -1,   1),   S( -5, -15),
            S(  2,  -4),   S(  5,   8),   S( -7,  -3),   S(  0,  22),   S(  8,  19),   S( -3, -28),   S( -2,  -9),   S(  0,  -5),
            S(  4,  11),   S( -7, -19),   S(  5,   0),   S( -4,  -1),   S(-11, -39),   S(  1, -25),   S( -1,   6),   S(  0,   5),
            S( -1, -12),   S( -5,   0),   S(  5,  -6),   S(  0,  27),   S( -4,  44),   S( -6,  44),   S(  3,  10),   S( -1,  -2),
            S(  2,   5),   S( 15,  23),   S(  0,  28),   S(-15, -11),   S( 24,  52),   S( 16,  47),   S( -2, -31),   S(  5,  17),
            S(  6,  22),   S( -5,  -4),   S( -5, -14),   S(-17, -42),   S(-12, -14),   S( 10,  40),   S(  3,  14),   S(  6,  22),
            S(  0,   1),   S(  0,   5),   S(  6,  23),   S( -1,  -9),   S(  7,  31),   S(  2,   1),   S(  2,  12),   S(  1,   2),
            S(  0,   2),   S( -1,   0),   S( -2,  -6),   S(  0,  11),   S(  2,   8),   S(  1,   3),   S( -1,  -4),   S(  0,   0),

            /* bishops: bucket 0 */
            S( 42,  -4),   S( -6,  54),   S(-31,  40),   S(-32,  21),   S(-17,  14),   S(  0,  28),   S( 54, -47),   S( 17,  20),
            S( -9, -41),   S( -2,  16),   S(-24,  42),   S(  4,  28),   S( 14,  13),   S( 29,  11),   S( 41,  39),   S( 32, -15),
            S( 13,  15),   S( 10,  16),   S( 10,  27),   S(  2,  28),   S( 21,  29),   S( 43,  35),   S( 49, -13),   S( 16,  -6),
            S(  1,  -7),   S( 54, -40),   S( 10,  21),   S( 49,  17),   S( 84,  22),   S( 29,  65),   S( 28,   3),   S(  2,  21),
            S( 51, -19),   S( 39,   8),   S( 78,   2),   S( 83,  46),   S(108,  30),   S(  7,  43),   S( 34,  44),   S( -9,  51),
            S( 45,   5),   S( 78,  44),   S(118,  23),   S( 62,   7),   S(-10,  62),   S( 31,  53),   S( 53,   6),   S( 14,   1),
            S(-50, -102),  S( 64, -27),   S( 73,  86),   S(-26,  33),   S( 29, -30),   S( 45,  19),   S( 14,  37),   S(-31,  47),
            S( -9, -39),   S(  3, -24),   S(  2, -31),   S(-38, -25),   S(-25,  -5),   S( 12,  15),   S(-16,  74),   S(  1,  15),

            /* bishops: bucket 1 */
            S(-45,  46),   S( -5,  -9),   S(-29,  46),   S( 18,  -4),   S(-33,  27),   S( 10,  20),   S( 50, -27),   S( 55, -32),
            S( 21, -19),   S(-13,   3),   S( -3,  -7),   S(-11,  27),   S( 40, -12),   S(  1,   4),   S( 60, -12),   S(-13,  -7),
            S(-12,  -9),   S( 24,  -7),   S(-19,  29),   S( 17,   8),   S(  9, -11),   S( 45, -10),   S( -1,  -4),   S( 73, -10),
            S( 23, -24),   S( 68, -15),   S( 20,   2),   S( 20,   6),   S( 64,  -4),   S( 11,   5),   S( 73,  -9),   S(  0,   2),
            S( 20, -34),   S( 58, -17),   S(  4,  10),   S(104, -10),   S( 55,   3),   S( 51,  12),   S( -5,  31),   S( 43, -20),
            S( 94, -52),   S( 32, -12),   S( 61,  -6),   S( 33,   6),   S(103, -19),   S(-47,  28),   S(-33,  69),   S(-38,  29),
            S(  1, -73),   S(  1, -16),   S(-23,  -8),   S( 20,  28),   S( 10,  44),   S( -1,  26),   S(  9,  16),   S(-37,  22),
            S( -9, -21),   S(-19, -20),   S(-15, -20),   S(-38, -42),   S(-31, -10),   S( 38, -41),   S( 12, -12),   S(-57,  18),

            /* bishops: bucket 2 */
            S(  6, -13),   S( -3, -24),   S(  0,  14),   S(-23,  11),   S( 13,   8),   S(-18,  11),   S( 13,  -9),   S(  0,   8),
            S(  8, -20),   S( 10,  -2),   S(-17,   1),   S( 12,  11),   S( -6,  11),   S( -6,   7),   S(  5,  -4),   S( 17, -60),
            S( 32,   1),   S( 21,  -4),   S(  0,  19),   S( -8,   4),   S( -5,  29),   S( -7,  -4),   S( -3, -29),   S(-14,  -3),
            S( -5, -14),   S( 45,  -3),   S( -3,   6),   S( 24,  39),   S( -3,  26),   S( -7,  25),   S(-10,  -5),   S( -4,   2),
            S(  2,  18),   S( -2,  28),   S( 47,  16),   S( 29,  16),   S( 18,  29),   S( 10,  20),   S(  7,  20),   S( 47, -26),
            S(-39,  37),   S(-17,  43),   S(-25,  -3),   S( 82, -17),   S( 53,   4),   S( 68,  39),   S( 63,  -3),   S( 28, -39),
            S(-38,  67),   S( -7,  13),   S(-11,  24),   S(-12,  31),   S(-43, -41),   S(-47,  14),   S(-52,  38),   S(-46, -31),
            S(-62, -29),   S(  2,  30),   S( 15, -14),   S(-22,  14),   S(-33, -15),   S(-53,  -8),   S(  4, -41),   S(-90,  -5),

            /* bishops: bucket 3 */
            S( 28,  25),   S( 40, -33),   S( 16, -19),   S(  8,  12),   S( 15,  23),   S(  2,  38),   S(-28,  64),   S(  9, -16),
            S( 30,  13),   S( 29,   1),   S( 20,  10),   S( 28,   6),   S( 29,  19),   S( 11,  23),   S( 19,  13),   S( 44, -44),
            S(  8, -10),   S( 42,  28),   S( 24,  36),   S( 24,  33),   S( 17,  38),   S( 14,  35),   S( 24, -10),   S( 17,  17),
            S(-16,  23),   S( 17,  39),   S( 27,  54),   S( 31,  58),   S( 45,  29),   S( 30,  15),   S( 39, -12),   S( 33, -45),
            S(  6,  30),   S( 11,  54),   S(  3,  62),   S( 53,  61),   S( 55,  45),   S( 54,  24),   S( 26,  20),   S( 16,  13),
            S(  1,  44),   S( 22,  58),   S( -3,  47),   S( 12,  52),   S( 41,  46),   S( 76,  48),   S( 41,  74),   S( 37,  62),
            S(-50,  90),   S( -4,  68),   S( 26,  27),   S( 16,  45),   S( 11,  37),   S( 38,  72),   S(-14,  -6),   S( 18, -27),
            S(-67,  68),   S(-33,  70),   S(-70,  47),   S(-26,  32),   S( 25,  -2),   S(-66,  41),   S( -9, -32),   S( 25,  14),

            /* bishops: bucket 4 */
            S(-14,  21),   S(-31,   5),   S(-81,  47),   S(-46,  31),   S(-40, -17),   S(-21, -14),   S(-19, -47),   S(-19, -50),
            S(  1,  -7),   S( 28, -11),   S( 63, -38),   S(-60,  23),   S(-56,  44),   S( 44, -37),   S(-22,  -6),   S( -6, -22),
            S( 15,  54),   S(-11, -34),   S( 41,  -8),   S(  2,   0),   S( 47, -16),   S(-59,  29),   S( -2, -11),   S(-51, -16),
            S( 55, -11),   S( 63,  -9),   S( 41,  -2),   S(-13,  22),   S( 10,   0),   S( 59,  14),   S(-44,   4),   S( 18,  -7),
            S(  7, -33),   S(-26, -25),   S( 27, -20),   S( 21,  -1),   S(  5,  20),   S( 27,  29),   S(-17,  77),   S(-83,  -1),
            S(-51, -104),  S(-39, -29),   S(-12,  14),   S( 21,  26),   S(-54,  54),   S( 46,  31),   S(-19,  29),   S(  0,  19),
            S( -4,   0),   S(-20,  12),   S(-20, -23),   S(-18, -51),   S(  0,  -8),   S( 21, -14),   S(  6,   1),   S( 54,  60),
            S(-12, -34),   S(  8, -29),   S( -1,   6),   S( -2, -26),   S(-26, -14),   S(  9, -16),   S( 10,  53),   S( 13,  10),

            /* bishops: bucket 5 */
            S(-42, -14),   S( 24, -21),   S(-55,  17),   S(-48,  26),   S(-13,   2),   S(-54,  11),   S(-33, -13),   S(-48, -19),
            S(-26, -15),   S(-21,  18),   S(  5,   5),   S(  9,  -7),   S(-68,  27),   S(-10,   2),   S(-25,  -2),   S(  1, -21),
            S(  5,  25),   S( -6, -12),   S( 48, -14),   S( -8,   2),   S(-30,  25),   S(-29,  -2),   S(-16,  11),   S(-53,  26),
            S( 14, -13),   S( -9,  -2),   S( 65, -32),   S( 26,  18),   S(-10,   6),   S( 16,   9),   S(-86,  39),   S(-30,  28),
            S( -2, -16),   S( 24, -18),   S(-59, -12),   S(-15, -28),   S(-16,  -6),   S(-22,  10),   S( -8,  25),   S(-73,  24),
            S( 22, -40),   S(-42,  13),   S(  7, -16),   S( -9, -65),   S(-36,  16),   S(-29,  12),   S(-24,   2),   S( -9,  52),
            S(-29, -35),   S(-20, -12),   S(-30, -12),   S( -4,   7),   S( 14, -23),   S(-24,  23),   S( 11,  32),   S(-26,  20),
            S(-24,  -7),   S( -7, -38),   S( 16, -39),   S(-38,  14),   S(-20,  15),   S(-16,  12),   S(-28,  -2),   S( 13,  35),

            /* bishops: bucket 6 */
            S(-15, -16),   S(-19,  -3),   S(-41,   5),   S(-25,  10),   S(-65,  38),   S(-21,  16),   S(-55,  23),   S(-21,  13),
            S(-17,  20),   S(-15, -21),   S(-75,  43),   S(-38,  28),   S(-73,  39),   S(-32,  16),   S(-59,  29),   S(-59,   1),
            S( 23,  -2),   S(-60,  33),   S(  4,  -4),   S(-50,  34),   S(-31,  32),   S(-38,  14),   S(-12, -22),   S( -4,   4),
            S(-66,  36),   S(-97,  38),   S(  7,  -3),   S( 55,  14),   S( 10,  29),   S( 27,   2),   S( -7,  -2),   S(-19,  26),
            S(-16,  19),   S(-53,  37),   S( 18,   5),   S( 39,   8),   S(-25, -14),   S(-13,  11),   S( 32,  -9),   S(-50,  -1),
            S(-60,  51),   S(-32,  22),   S(-40,  22),   S(-12,  12),   S( 17, -29),   S(-13, -23),   S( 16,   8),   S(-24,  -5),
            S(-21,  27),   S(-84,  57),   S(-32,   9),   S( 12, -32),   S(-11,  17),   S( -9, -12),   S(  5, -14),   S(-19, -26),
            S( -8,  14),   S(-31,  16),   S(-18,   4),   S( -3,  10),   S(-15,  12),   S( 44, -31),   S(-27, -23),   S( -1,  13),

            /* bishops: bucket 7 */
            S(-17, -59),   S(-101,  15),  S(-20, -45),   S(-47,  16),   S(-40,   3),   S(-64,  11),   S(-44, -47),   S(-55,  -2),
            S(-12, -34),   S( -1, -42),   S( 14, -26),   S(-26, -12),   S(-26,  -2),   S(-45,   1),   S(-32, -20),   S(-18,  -2),
            S(-26, -17),   S(-45,  25),   S( 25,  -7),   S( 38, -20),   S(-18,   3),   S(-24,  10),   S(-64,  18),   S(-66,  18),
            S( 16, -45),   S(-57,  27),   S(-22,  13),   S(-10,  26),   S( 60,  11),   S(-22,  14),   S( 39, -24),   S(-22, -11),
            S(-59,  23),   S(  6,  20),   S(-46,  39),   S(  2,  18),   S( 43,  -2),   S( 20,  25),   S(-31,  17),   S(-27, -15),
            S(-95,  17),   S(-38,  51),   S( 20,   5),   S(-95,  50),   S(-24,  17),   S( 37,  -6),   S(-12,  15),   S(-45, -81),
            S(-23, -13),   S(-25,  42),   S(-39,  26),   S( 12,   7),   S(-19,  -4),   S( 64, -47),   S( 22, -24),   S( 11,   3),
            S( -4,   4),   S(-12,  31),   S( -7, -15),   S(-24,  -2),   S( 35, -15),   S(-32,  11),   S( 34, -31),   S(  3,  10),

            /* bishops: bucket 8 */
            S( 42,  85),   S( -1, -35),   S( -1,   9),   S(-10,   2),   S(  9,  21),   S(-25,  -6),   S(-11, -14),   S(-13,  -8),
            S(-12, -14),   S( 33,  63),   S( 25,  -5),   S(  3,   0),   S( -4,  -9),   S(  1,   7),   S(-14, -40),   S( -9,   8),
            S(-12, -12),   S(-12, -22),   S( 22,  42),   S(  5,   6),   S(  0,  41),   S( 37,  55),   S(-37, -19),   S(-22, -12),
            S( -5, -29),   S( 21,   8),   S( -8,  32),   S( 26,  -3),   S(  9,  30),   S( 19,  45),   S(-18,  23),   S(  5, -16),
            S(  5, -12),   S( 57,  67),   S( 30,  35),   S(-20,  13),   S( 10,  26),   S(-24,  32),   S( 11, -20),   S(  1,  22),
            S( -5,   2),   S(  5,  26),   S( 18,  20),   S( 22,  32),   S(  3,  23),   S( 63,  32),   S(-13,  54),   S( -3,  32),
            S( -1,   9),   S(-16, -27),   S( 20,  24),   S( 34, -12),   S( 10,  13),   S( 26,  33),   S( 31,  49),   S(-19,  -9),
            S( -8,  13),   S(  5,  -6),   S(  1,  17),   S( 14,  18),   S( 21,  13),   S( 26,  31),   S( 11,  39),   S( 47,  36),

            /* bishops: bucket 9 */
            S( 11,  36),   S(  4, -13),   S(-12,  15),   S(-35, -19),   S(-12, -43),   S(-12, -27),   S( -5,  -2),   S(-15, -21),
            S(-17, -38),   S(-27, -25),   S( 12,  13),   S(-41, -14),   S(-44,  -2),   S( -6, -36),   S(-32,  -5),   S(-20, -19),
            S( -5, -27),   S( 22, -11),   S(-10, -21),   S( -4,  22),   S( 24,  -7),   S(  2, -12),   S(  5,  12),   S(-22,  -5),
            S( -4,  64),   S(  0, -15),   S( -5,   5),   S( 19,  -7),   S(-19,  -3),   S(-11,  25),   S( 12,  24),   S(  4,  55),
            S( 22,   3),   S(  1, -22),   S( 22,  38),   S( 19, -36),   S( -2,  11),   S( 10,  19),   S(-24,  11),   S(-15, -22),
            S( 23,   4),   S(-26,   8),   S(  8, -16),   S(  5,  40),   S( 33, -51),   S(-12,  31),   S( 24,  27),   S(  2,  48),
            S(  1, -59),   S(-12,  16),   S( 10,  17),   S( 19,  -2),   S(  7, -23),   S( 30,  16),   S( 35,  35),   S(  9,  70),
            S( 14,  49),   S(  8, -16),   S( -1,  -2),   S( 11,  -1),   S( 12,  54),   S(  0,  -3),   S( 21,   2),   S( 37,  22),

            /* bishops: bucket 10 */
            S(  8,   0),   S( 10,  32),   S(-18, -55),   S(-40, -32),   S(-62,  -9),   S(-14, -82),   S( -7,   4),   S( -2,  22),
            S(-14,   2),   S( 17,   1),   S(-17, -38),   S(-32, -44),   S(-63,   6),   S(-47, -19),   S(-44, -13),   S(  5,  20),
            S(-11, -18),   S(-18, -35),   S(-14, -20),   S(  0,  -2),   S(-43, -22),   S(-17, -26),   S(  4,   2),   S(-19, -28),
            S(-20,  35),   S(-32,  18),   S(-37, -17),   S(-17,   0),   S(-27,  -5),   S( 36, -25),   S( 22,  57),   S(  3, -54),
            S( -2,  17),   S(-17,  28),   S( 11, -17),   S(-31,  16),   S( 40, -17),   S( 19,  43),   S( 15, -42),   S(  8,   6),
            S( 18,   2),   S(  3,   1),   S(  6,  15),   S( 16, -11),   S( 36, -12),   S( 21,  15),   S( 12,  11),   S( 11,  -7),
            S( 23,  26),   S(  1,  18),   S( 31,  -4),   S(  7,  36),   S(-11,  22),   S(  1, -61),   S(  4, -27),   S( 16,  -9),
            S( 18,  61),   S(  5,  17),   S( 42,  13),   S(  7,  34),   S( 11,  57),   S( -2,  13),   S( 19,  46),   S(  0, -15),

            /* bishops: bucket 11 */
            S( 18, -20),   S(  2,   1),   S(-14, -18),   S( -5,  -5),   S(-17, -18),   S(-11, -24),   S(-22, -27),   S(-10,  11),
            S( -4, -17),   S( 29,  14),   S( -9,   2),   S(  2, -37),   S( -2,   5),   S(-38, -38),   S(-27,  27),   S( 11, -24),
            S(-19, -49),   S(  7,  11),   S( 19,   6),   S(-53, -10),   S(-11, -14),   S(  3,  33),   S(  0,   4),   S(-14, -28),
            S( -4, -32),   S(-21, -43),   S( 18,  -5),   S(-66,  -5),   S( 11,   3),   S( 31,  33),   S( 42,   0),   S( -1,  -9),
            S(-24, -30),   S(-21,  -7),   S(-55,  69),   S(-13,  54),   S(-31,  15),   S( 29, -24),   S( 30,  -2),   S( -1,  -3),
            S( -7,   7),   S(  0,  14),   S(  4,  11),   S( -6,  22),   S( 23,  31),   S( 31,   1),   S( 18,  12),   S(-18, -31),
            S(  3,   7),   S( 48,  60),   S( 17,  50),   S( 29,  32),   S( 19,  -2),   S(-18, -15),   S( -4, -20),   S( -6,  -5),
            S( 36,  34),   S(  5,   0),   S( 34,  49),   S( 37, -16),   S(  6,  39),   S(  4,  25),   S(  3,   2),   S(  5,   3),

            /* bishops: bucket 12 */
            S( -7, -19),   S( -3,  -8),   S( -5,   9),   S(-11, -19),   S(-13,   1),   S(-13, -16),   S( -6, -10),   S(  0,  -2),
            S( -5, -10),   S(  7,   1),   S(  4,  11),   S( -5,  -4),   S(-14,  -6),   S(  7,  10),   S( -8,  -8),   S( -2,  -8),
            S(  7,  -2),   S( 10, -15),   S( 20,   1),   S( 20,  -5),   S(  2,   3),   S(-14, -11),   S( 10,   6),   S(-11, -11),
            S( 12,   8),   S( 15,   3),   S( 12,  28),   S( 13,  22),   S(  6,  20),   S( -3,  12),   S(  5,  26),   S( 10,  19),
            S(  5,  -7),   S(  4,  -5),   S( -1,   1),   S( 23,   5),   S( 12,   8),   S( 24,  26),   S(  2, -12),   S(  6,   5),
            S(  7,  -2),   S(-14, -28),   S( -3,  18),   S( -1, -14),   S( 18,  11),   S( 12,  12),   S( -6, -10),   S( -4,  -6),
            S( -2,   2),   S(  3,   7),   S(  2,  15),   S(  1,   1),   S(  7, -19),   S( 24,  24),   S(  7,  30),   S(  5,  10),
            S( -2,  -1),   S(  1,   1),   S(  3,   8),   S(  0,   3),   S( -1,   2),   S(  3, -10),   S( 15,  16),   S(  4,   4),

            /* bishops: bucket 13 */
            S(-12, -34),   S(  0,  -6),   S(  0,  -4),   S(  0,  -1),   S(  3,   1),   S(-27, -30),   S(-21, -36),   S( -5,  -9),
            S( -8, -19),   S( -6, -21),   S(  0,  10),   S( 14,  -6),   S(-19, -22),   S( 14,  33),   S(  0,  -4),   S(  1,   0),
            S(  4, -16),   S( 26,  24),   S(  3, -33),   S( 22,  20),   S( -1,  -9),   S( 10,  22),   S(  2, -10),   S( -6,  -2),
            S( 24,  34),   S( 39,  19),   S( 18,  12),   S( -3, -30),   S( 20,  76),   S( -7,   2),   S( 12,  22),   S( -1, -10),
            S( 17,  10),   S( 11, -10),   S( 16,   6),   S( 12,  -8),   S( 14,  13),   S(  7,  13),   S( 19,   7),   S(  3,   6),
            S(  2,   9),   S(  6,  14),   S( -1,  -3),   S( 16,   8),   S(  8,  45),   S(  2,  17),   S(  1, -11),   S(  5,  -9),
            S(  5,   0),   S(-13, -19),   S(  3, -13),   S( 19,  22),   S( 12,  36),   S( 19,  27),   S( 16,   0),   S(  6,   5),
            S(  2,   2),   S( -1,  -7),   S( -2,   1),   S(  9,  10),   S(  5,   8),   S(  1,  -5),   S( 12, -11),   S(  8, -26),

            /* bishops: bucket 14 */
            S(-12, -27),   S(  4,  28),   S( 22,  23),   S(  4,   1),   S(-16,  13),   S( -9, -20),   S( -8,  -3),   S( -8,  28),
            S( -6,  -5),   S( -3, -11),   S( 10,   0),   S( -6,   8),   S(  8,  -7),   S( -1,  -8),   S( -4,  28),   S(  5,  18),
            S( -3, -11),   S(  0,   3),   S( -7,  24),   S( 27,  47),   S( 17,  65),   S( 11,  16),   S( -4,  25),   S( 11,  33),
            S(  4,  47),   S( 16,  -5),   S( -9, -10),   S( -3,  10),   S(  3,  13),   S( 11, -34),   S( 31,  38),   S(  6,   1),
            S( 12,  12),   S( -6,  -3),   S( 14,  -9),   S( 16,   4),   S( -4,  10),   S(  5,   7),   S( 35,   9),   S(  5, -24),
            S( -1,  -2),   S( 26,  56),   S(  8,  36),   S( 13,   9),   S( 10,   7),   S( -6,   6),   S( -1,  21),   S( 16,  -5),
            S( 16,  31),   S( 20,  13),   S( 12,  17),   S( 11,  12),   S(  4,  -9),   S( 13,  41),   S( 10,  13),   S(  4,   4),
            S( 12,  -9),   S( 11,   4),   S(  3,  -2),   S(  7,  16),   S( -5,  -8),   S( -2,  -6),   S( 10,   2),   S(  3,  -4),

            /* bishops: bucket 15 */
            S( -2,  -8),   S(  1,   0),   S(-14, -29),   S( -5,  11),   S( -5, -21),   S( -1, -10),   S( -7, -18),   S( -6, -17),
            S( 11,  24),   S(  3,   3),   S(  7,   6),   S(  6,  -2),   S( 13,   8),   S( -3,  -7),   S(  0, -19),   S( -3,  -5),
            S(  7,  -3),   S(  9,   0),   S(  4, -23),   S( 28,  35),   S( 17,   6),   S(  9,  29),   S( 16,  38),   S(  0,  -2),
            S( -1,  -9),   S( 24,  24),   S(  2,  32),   S(-10,  -1),   S( 15,  18),   S( 26,  -2),   S( 18,  20),   S(  8,   8),
            S(  2,   2),   S( -6,  11),   S(  8,  41),   S( 23,  49),   S( 13,   3),   S( 17,  12),   S(  3,  -7),   S( -3,   9),
            S( -1,  12),   S(  3,  27),   S( 16,  28),   S( -5,   5),   S( 30,  25),   S( 10, -14),   S( -8,  -1),   S( -1,  -4),
            S(  6,  -8),   S( 10,  18),   S(  5,  23),   S(  8,   4),   S(  6,  27),   S(  0,   7),   S( -4, -12),   S( -2,  -3),
            S(  3,  -4),   S(  9,   8),   S( 11,  15),   S(  4,  -2),   S(  6,  15),   S( -1, -11),   S(  5,   7),   S(  5,  -7),

            /* rooks: bucket 0 */
            S(-15,  14),   S( 21, -17),   S(  3,  -6),   S(  4,   3),   S(-27,  75),   S(-10,  38),   S(-42,  78),   S(-53,  44),
            S(  2, -46),   S( -1,  23),   S(-30,  27),   S(  3,  33),   S(  1,  58),   S(  8,  21),   S(-18,  12),   S(-26,  37),
            S( 26, -27),   S( 13,  -3),   S( -9,  26),   S( -5,  11),   S(-44,  75),   S(-27,  34),   S(-18,  46),   S(  7,  13),
            S( 17, -41),   S( 43,  10),   S(-35,  42),   S( 23,  15),   S( -8,  66),   S(-18,  40),   S(-30,  58),   S(-37,  58),
            S( 77, -81),   S( 23,  33),   S( 23,   5),   S( 27,  38),   S( 28,  31),   S( 13,  69),   S( 46,  42),   S( 18,  38),
            S( 64, -34),   S( 82,  38),   S( 97,  -4),   S( 85,  13),   S(-15,  75),   S( 31,  51),   S(  6,  74),   S(-31,  85),
            S( 27,  24),   S( 66,  72),   S( 92,  45),   S( 94,   7),   S( 96,  40),   S( -5,  77),   S(-18,  85),   S(-12,  68),
            S(  1, -22),   S( 19,  -3),   S( 53,  44),   S( 47,  -6),   S( 55,  48),   S( 69,  33),   S( 37,  35),   S( 46, -34),

            /* rooks: bucket 1 */
            S(-48,  42),   S(-13,   1),   S(  2,   7),   S(-37,  31),   S(-35,  48),   S(-40,  55),   S(-39,  61),   S(-76,  70),
            S(-45,  27),   S(-15, -16),   S(-25,  31),   S(-24,  26),   S(-29,   4),   S(-59,  56),   S(-29,  14),   S(-51,  59),
            S(-34,  24),   S( -7,  -9),   S(  6,  -4),   S(-31,  25),   S(-30,  17),   S(-17,  19),   S(-59,  53),   S(-33,  62),
            S(-61,  59),   S( 19,  -1),   S(-17,  32),   S(-10,   4),   S(-30,  44),   S(-54,  73),   S(-61,  71),   S(-88,  92),
            S(-23,  48),   S( 24, -19),   S( 58,  -2),   S( 26,  -3),   S( 25,   4),   S(-12,  81),   S(-21,  57),   S(-30,  83),
            S( 57,  30),   S( 72, -16),   S( 46,   7),   S( 16,  28),   S( 38,  12),   S(  3,  58),   S( -2,  53),   S( 20,  70),
            S( 46,  61),   S( 42,  -1),   S( 40,  49),   S( 19, -22),   S( 89,  21),   S( 28,  45),   S( 46,  64),   S( 39,  88),
            S( 48, -13),   S( 22, -27),   S(-26,   5),   S(-26,  -1),   S( 43,  10),   S( 23,  11),   S( 37,  31),   S( 75,  25),

            /* rooks: bucket 2 */
            S(-55,  63),   S(-36,  50),   S(-38,  49),   S(-32,  17),   S(-16,  23),   S(-34,  27),   S(-26,  17),   S(-71,  65),
            S(-50,  57),   S(-62,  62),   S(-41,  57),   S(-37,  26),   S(-52,  39),   S(-45,  26),   S(-25,  12),   S(-38,  26),
            S(-55,  63),   S(-42,  57),   S(-42,  47),   S(-34,  41),   S(-35,  27),   S(-27,  31),   S(-13,   5),   S( -8,  26),
            S(-47,  79),   S(-57,  85),   S(-53,  76),   S(-50,  43),   S(-35,  44),   S(-23,  43),   S(-23,  26),   S(-20,  45),
            S(-18,  81),   S(-30,  76),   S(-17,  77),   S(-22,  42),   S(-31,  48),   S( 56,  17),   S( -9,  52),   S(-11,  63),
            S(  6,  91),   S( 11,  73),   S( 44,  52),   S(-13,  46),   S( 73,  -9),   S( 55,  35),   S(113, -12),   S( 52,  57),
            S( 52,  75),   S(  7,  79),   S( 57,  45),   S( 54,  20),   S( 43, -23),   S( 32,  53),   S(-46,  98),   S( 10,  85),
            S( 33,  34),   S( 49,  37),   S( 86,   6),   S(-13,   9),   S(-44,  26),   S( 19,  -1),   S(  0,  16),   S( -8,  56),

            /* rooks: bucket 3 */
            S(-15,  77),   S( -7,  73),   S(-14, 101),   S( -8,  88),   S(  5,  55),   S(  5,  49),   S( 25,  20),   S( -5,   4),
            S( -3,  60),   S(-18,  86),   S(-16, 109),   S(  0,  97),   S(  2,  59),   S( 16,  20),   S( 51,  -3),   S( 16,  10),
            S(  8,  62),   S(-13,  87),   S( -6,  90),   S( -4,  95),   S( 13,  49),   S(  7,  34),   S( 44,  21),   S( 27,   2),
            S( -8,  96),   S(-18, 124),   S(-29, 133),   S( -3, 104),   S(-12,  82),   S(  7,  69),   S( 39,  36),   S(  5,  29),
            S(  5, 107),   S(-21, 125),   S(  9, 121),   S(  4, 114),   S(  8,  87),   S( 36,  68),   S( 58,  55),   S( 37,  52),
            S(  3, 127),   S( 22, 106),   S( 22, 121),   S( 47,  94),   S( 92,  47),   S(122,  38),   S(109,  43),   S( 35,  53),
            S( 20, 117),   S(  3, 119),   S( 11, 136),   S( 15, 130),   S( 34, 105),   S(120,  37),   S(127, 153),   S(173,  66),
            S(123, -24),   S( 55,  53),   S( 23, 121),   S( 25,  80),   S( 34,  76),   S(106,  43),   S( 74,  41),   S(136,   5),

            /* rooks: bucket 4 */
            S(-11, -31),   S(-19,   1),   S( -5, -24),   S(-50,  20),   S(-46,   0),   S(-36,  21),   S(-28,   4),   S(-82,  29),
            S(-69, -30),   S(-71,  14),   S( 29, -51),   S(-16, -31),   S( 16,   5),   S(  3,  25),   S(-52,  12),   S(-26,  24),
            S(-12, -30),   S(-46, -22),   S(-26,  13),   S(  0, -43),   S(-21, -10),   S(-31,  14),   S(-30, -23),   S(-63,  27),
            S(-73, -43),   S( 32, -13),   S(-10, -33),   S(-21, -30),   S( 33,  -3),   S(  0,  11),   S(-22, -18),   S( -9,  -7),
            S(-18, -41),   S( 24, -62),   S( 47, -18),   S( 33, -16),   S( 69,   7),   S( 31,  21),   S( 11,  16),   S( 29,   5),
            S(-24, -58),   S(  7,  -7),   S( -2, -31),   S( 28,  27),   S( 31,  36),   S( 13,  18),   S( 50,  13),   S(  0,  41),
            S(-29, -29),   S( 31,  52),   S( 79, -12),   S( 56, -22),   S( 43,   4),   S( 12, -16),   S( 19, -40),   S( 34,   4),
            S( 22, -16),   S( 14,  29),   S( 43,   2),   S(  2,   0),   S( 62,   0),   S( -4,  -7),   S(-15,   3),   S( -2,   5),

            /* rooks: bucket 5 */
            S(-20,  30),   S(-21,   0),   S(  3,  -1),   S( 26,   3),   S(  5,  17),   S( 14,  17),   S(-17,  57),   S(-64,  66),
            S( -9,  -1),   S(-25, -13),   S( 42, -48),   S( 33, -14),   S( 11, -10),   S(-16,  16),   S(-39,  49),   S(  0,  15),
            S(-59,  26),   S(-21,   2),   S(  4, -23),   S( 22, -17),   S(-13, -12),   S( 23,  -3),   S(-61,  47),   S(-36,  31),
            S(-27,  23),   S( -5,   2),   S( 67, -39),   S( 14,  -3),   S( 29,  -9),   S(-22,  42),   S( -2,  20),   S(  9,  31),
            S( 15,  28),   S( 24,   7),   S( 31,  14),   S( -7, -14),   S( 10,  14),   S( 92,   6),   S( 23,  45),   S( 25,  46),
            S( -5,  28),   S( -1,  13),   S(  5,   5),   S(-18, -28),   S( 20,  15),   S( 35,  24),   S( 98,  13),   S( 69,  28),
            S( 13,   5),   S(  8,  -7),   S( -1,  -8),   S( 55,  15),   S( 53, -23),   S( 21, -18),   S( 79, -27),   S( 36,  -6),
            S(-26,  46),   S( 21,  -6),   S( 21,   7),   S(  8,  21),   S( 33,  13),   S( 15,  36),   S( -1,  52),   S( 45,  56),

            /* rooks: bucket 6 */
            S(-75,  60),   S(-42,  41),   S(-26,  25),   S( -7,  10),   S( 20, -12),   S( 11,  -2),   S(  8,  -2),   S( -2,  -5),
            S(-45,  18),   S( 20,   5),   S(-13,  16),   S(  2,  11),   S( 54, -35),   S(  0,  -8),   S(-37,   0),   S(-25,  21),
            S(-65,  52),   S(-14,  20),   S(-12,  20),   S(-19,   9),   S(-13,   6),   S( 64, -22),   S(  3, -32),   S( -8, -10),
            S(-46,  53),   S(-13,  48),   S( 22,  12),   S( 82, -25),   S( 62, -21),   S( 10,  12),   S(-12,  -3),   S( 52,  10),
            S(-43,  63),   S( 23,  34),   S( 91,  16),   S( 70, -11),   S( 30, -28),   S( 15,  25),   S( 66, -10),   S( 99, -10),
            S( 78,  20),   S( 96,   4),   S(117,  -9),   S( 42,  -5),   S( -1, -49),   S( 23,  29),   S( 56, -15),   S( 71,  17),
            S(  2,  24),   S( 96, -18),   S( 94, -33),   S( 89, -39),   S( -8, -10),   S( 41,  -5),   S( 62, -36),   S( 58, -26),
            S( 68,  -8),   S( 41,  13),   S( 12,  22),   S( 66, -22),   S( 69,  -6),   S( 32,  25),   S( 39,  10),   S( 52,  14),

            /* rooks: bucket 7 */
            S(-96,  34),   S(-87,  45),   S(-78,  45),   S(-57,  43),   S(-35,   9),   S(-34,  -5),   S(-35,   8),   S(-60, -19),
            S(-89,  36),   S(-36,  15),   S(-55,  29),   S(-68,  45),   S(-27, -20),   S(-37,   1),   S(  7,   6),   S(  3, -65),
            S(-66,  32),   S(-72,  26),   S(-34,  19),   S(-50,  34),   S(-16, -10),   S(-41,  19),   S( 31, -27),   S(-14, -50),
            S(-72,  29),   S(-23,  25),   S( 14,   5),   S( 49, -13),   S( 27,  -9),   S( 37, -14),   S( 26,   0),   S(  5, -22),
            S( 19,  29),   S( 40,  16),   S( 34,  25),   S(100, -18),   S(135, -47),   S(132, -43),   S(113, -33),   S(-57, -14),
            S( -1,  47),   S( 13,  21),   S( 91,   7),   S(106, -16),   S( 87,  -8),   S( 29,  21),   S(  5,  15),   S( -6, -28),
            S( -9,   6),   S( 25, -17),   S( 35,  -5),   S(117, -39),   S(111, -42),   S(118, -44),   S( 63, -16),   S( 17, -40),
            S(-30, -13),   S(  0,  10),   S( 21,   1),   S( 12,  14),   S( 31,  -3),   S( 70, -22),   S( 55,  11),   S( 14,  -9),

            /* rooks: bucket 8 */
            S( -4, -88),   S(  2, -25),   S(  7,   5),   S( 31,  -9),   S(-45, -42),   S(  2, -21),   S(-29, -45),   S(-26,  15),
            S(-35, -92),   S(-19, -55),   S(-39,  -2),   S(-19, -89),   S(-27, -56),   S(-53, -56),   S( -5, -18),   S(-37, -33),
            S( -5,  -1),   S( -5, -15),   S( -1, -46),   S(-20,  -8),   S( -8,  41),   S( -3,  24),   S( -8,  33),   S(-26,   1),
            S(  0, -11),   S(  1,  -3),   S( -2,  10),   S( 11,  21),   S( 15,  57),   S( 22,  31),   S( -2,  14),   S(-11, -40),
            S( -9, -44),   S( 16,  26),   S( 13,  21),   S( 17,  26),   S(  2,  30),   S( -6,   8),   S(  0,  35),   S( 16,  53),
            S(-17, -11),   S(  1,   9),   S(-29,   3),   S(-10,  -9),   S(  5,  32),   S(-25,   2),   S(-11,   8),   S( -3,  25),
            S(-13,  24),   S(  7,  25),   S(  7, -14),   S( 11,  25),   S( 15,  11),   S( 20,  46),   S( -1,  23),   S(  9,  51),
            S(-11,  21),   S( 11,  23),   S(-37,  40),   S( 13,  24),   S( -1,  38),   S( 14,  34),   S(  9,  49),   S( 13,  54),

            /* rooks: bucket 9 */
            S(-20, -56),   S(  8, -77),   S( 24, -87),   S( 34, -53),   S(-20, -49),   S( -9, -38),   S( -3, -18),   S(-18, -31),
            S(-71, -56),   S(-35, -61),   S(-38, -77),   S(-45, -64),   S(-53, -32),   S(-49, -12),   S(-34, -71),   S(-17, -37),
            S(-18, -27),   S(-23,  -3),   S(-11, -13),   S( -4, -50),   S( -5, -17),   S(-19,   5),   S(  6,  42),   S( -6,  22),
            S( -8,  -7),   S( -5, -29),   S( 10,  19),   S( -5,   4),   S(-12, -50),   S(  4,  17),   S(  5,  14),   S(  0, -19),
            S(-27, -34),   S(  4, -33),   S( -8, -47),   S(-18,   7),   S(-31, -27),   S(-21,  -9),   S(-11, -24),   S(-16,  -6),
            S(-14,   8),   S(-50, -30),   S(-18, -22),   S( -7,  25),   S( -3,   1),   S( -3,   8),   S(-11, -13),   S(-18, -14),
            S(  1,  23),   S(  9, -16),   S( 15, -30),   S(-12,  23),   S(  9, -11),   S(  8,   1),   S( 12,  21),   S(-11,  12),
            S(-19,   3),   S(  3,  23),   S( -2,  20),   S( -8,  42),   S(  0,  25),   S(  0,  63),   S(-11,  -8),   S( 29,  35),

            /* rooks: bucket 10 */
            S(-22, -10),   S(-51, -11),   S(-10, -29),   S(-21, -77),   S(-13, -56),   S(-12, -70),   S( 20, -67),   S( -7, -16),
            S(-31, -13),   S(-45, -51),   S(-39, -30),   S(-48, -65),   S(-50, -58),   S(-11, -48),   S(-37, -81),   S(-62, -73),
            S(  2, -13),   S(-19,  -6),   S(-38, -13),   S(-59, -42),   S(-18, -29),   S(  6, -39),   S(-14, -43),   S(-22, -38),
            S(-32, -11),   S(-31,  -9),   S(  0, -27),   S( -6, -18),   S(  0,  -9),   S(  2,   3),   S(-10, -54),   S( 26, -25),
            S( -5,  -9),   S(-26,  -2),   S(-11, -31),   S( -4, -78),   S(  3,   3),   S(-10, -12),   S( -3, -45),   S(-21, -41),
            S(-30,  -6),   S( -6, -17),   S(-18, -15),   S(-31, -45),   S(  2, -17),   S( -9, -11),   S(-21, -52),   S(-21, -10),
            S(-13, -15),   S( 11, -17),   S( -2, -27),   S(-19, -16),   S( 24, -26),   S(-18, -18),   S(-19, -42),   S( 14, -28),
            S(  5,   1),   S(  4,  37),   S( -1,  29),   S(-40,  -1),   S(-13,  52),   S(-11,   9),   S(-41,   1),   S( -4,   9),

            /* rooks: bucket 11 */
            S(-34, -14),   S(-55,   7),   S(-44,  -7),   S(-47, -12),   S(-25, -48),   S(-28, -26),   S(-26, -18),   S(-52, -57),
            S(  1,  -4),   S(-12, -22),   S(-56, -17),   S(-62, -38),   S(-20, -30),   S( 12, -12),   S(-14, -29),   S(-36, -73),
            S(-52,  17),   S(-18,  19),   S(  3,   2),   S(-37,   8),   S( 15, -12),   S( -8,  13),   S( 10,  -9),   S(-22,  10),
            S( -4, -13),   S(-11, -40),   S(-18,  20),   S( 22,  18),   S( 13,  15),   S(-33, -68),   S(  5,  14),   S( -7, -36),
            S(-30,  -7),   S( -3,  -2),   S(-18,  14),   S( 13,   0),   S( 44,  -7),   S( -2, -25),   S( 19,  48),   S( -7, -39),
            S(-17,  -1),   S(-18,  -5),   S(-11, -19),   S( 14,   8),   S( -4, -30),   S(  8,  -4),   S(-22,  34),   S( -6, -21),
            S(  3,  13),   S(-24, -36),   S(  4,  11),   S( -9,   2),   S( -3,  -1),   S(  7,   6),   S( 13,   0),   S(-11,   0),
            S(  6,  10),   S( 11,  45),   S( -2,  42),   S( 21,  31),   S(-11,  -3),   S(-15,  23),   S( 27,  29),   S(-17,  24),

            /* rooks: bucket 12 */
            S(-39, -119),  S( -3,  -4),   S(  0, -20),   S(-19, -36),   S( -6,  -8),   S( 22, -16),   S(-19, -55),   S(-27, -53),
            S(  6,  13),   S( -1,   5),   S(  3,  15),   S( 21,  42),   S( 12,  22),   S( 15, -17),   S( 18,   8),   S(-16, -43),
            S( -3,  -6),   S(  7,  43),   S( 10,  20),   S( 16,  13),   S( -6, -30),   S( 21,  30),   S( 13,  34),   S( -5,  25),
            S( 11,  45),   S( -5,  -2),   S( 11,  40),   S( 12,  13),   S( 13, -11),   S(  7,   3),   S(  8,  27),   S( -1,  17),
            S( 17,  19),   S(  8,  25),   S( -6,  37),   S( -3, -14),   S( 12,  28),   S( -1,  -1),   S( -5, -11),   S(  1,  -3),
            S(  2, -10),   S(-12,  -4),   S( -8,  -4),   S( -2,  16),   S(  7,  18),   S( -5, -20),   S( 11,  39),   S(  6,  10),
            S( -5, -25),   S( -9,  37),   S(  2,  29),   S(  1,  24),   S(  3,   3),   S(  9,  14),   S(  3,  25),   S( -4,  13),
            S(  0,  11),   S( -8,  47),   S( -1,   3),   S( 14,  30),   S(  5,  11),   S(  5,  34),   S(  4,   9),   S(  5,  17),

            /* rooks: bucket 13 */
            S(-41, -59),   S(-28, -56),   S(-24, -64),   S( -4, -22),   S(-22, -53),   S( -4,  -5),   S(-34, -47),   S(-36, -61),
            S(-16, -34),   S(-10, -22),   S( -3,   0),   S( -2,   3),   S( 20,  48),   S(  6,   2),   S(  8, -31),   S( -1, -13),
            S(-18, -21),   S(-17, -14),   S(-10,  -6),   S( -1,   6),   S( 17,  16),   S(  6,  -6),   S( 15,  41),   S( -5, -25),
            S(  3,   4),   S( -5, -13),   S(  4,  14),   S( -5,   6),   S( 14,  31),   S( -4,  -3),   S(  6,  13),   S( -3,  18),
            S(  4,  24),   S(  1,  -4),   S( -7, -15),   S(  4,  20),   S(  0,  37),   S(  1, -11),   S( 14,  29),   S(  8,  32),
            S( -7,  21),   S( -6, -10),   S(-12,  24),   S(-20, -13),   S(-19, -11),   S( 12,  22),   S( -8,  -9),   S( -3,  11),
            S(  0,  -2),   S( -7, -21),   S(  0, -21),   S(  1,  19),   S( -9, -43),   S( 10,  20),   S( 13,  15),   S(  0, -11),
            S( 11,   8),   S(-11,  -8),   S(-17, -39),   S( 18,  38),   S( -8,   1),   S(  7,  29),   S(  3,  32),   S( -3,   4),

            /* rooks: bucket 14 */
            S( -6, -29),   S(-29, -45),   S(-24, -43),   S(-17, -52),   S(-15, -25),   S(-11, -43),   S(-39, -76),   S(-24, -53),
            S(  2,  41),   S(  8,  20),   S(  7,  18),   S(  4, -25),   S(  4,  -7),   S(  3,   8),   S(  1,  -3),   S( -5,  -8),
            S( -8,   4),   S(  4,  39),   S(-17, -16),   S( -7, -22),   S(  0,   7),   S(  2,   5),   S( 11,  15),   S(-14, -47),
            S(  0,  40),   S( 13,  40),   S( -5,  -6),   S( 12,  13),   S(-18, -16),   S( -6, -36),   S( 16,  25),   S(-17, -46),
            S( 13,  44),   S( 25,  32),   S(  8,   7),   S(  5,  32),   S(  8, -24),   S( 12,   9),   S( -7,  -5),   S(-16, -44),
            S( 18,  47),   S( 11,  40),   S(  8,  21),   S( -6,  -5),   S( -9,   8),   S(-27,   1),   S(  2,   5),   S( -8, -18),
            S(-10, -31),   S( 23,  40),   S(-15, -43),   S(-19, -52),   S(  2,  19),   S( 16,  31),   S(-15, -48),   S(-12, -34),
            S( -5,  -7),   S( -2,  12),   S( -2, -14),   S(  8,   9),   S(-19, -33),   S(-17, -46),   S(  3, -11),   S( -1,  19),

            /* rooks: bucket 15 */
            S(-21, -56),   S(-16, -35),   S(-34, -57),   S(-23, -28),   S(-16, -31),   S(-15, -39),   S(  1,   8),   S(-19, -64),
            S( 15,  69),   S(-12,  -9),   S(-14, -29),   S(-17, -13),   S(-12, -29),   S( -6, -18),   S(  8,   8),   S(  2,   3),
            S(  5,  29),   S(-11, -21),   S( 19,  14),   S( 29,  44),   S(  5, -17),   S( -8, -26),   S(  5,  26),   S( 12,  29),
            S(  4,   7),   S(  1,  -2),   S( 18,  25),   S( -1,  -4),   S(  2,  12),   S(  9,  -5),   S( 18,  36),   S( -7, -26),
            S(  9,  10),   S(  7,  20),   S(  5,  14),   S(  1,  16),   S(  7,  20),   S(  1,  -9),   S(  5,  28),   S( 14,  18),
            S(  3,  -3),   S( -2, -38),   S( 11,   4),   S( -6, -11),   S( -2,   3),   S( -6,  41),   S( -2,   2),   S(  0,   4),
            S( 11,  -3),   S(  5,  32),   S(  5,  12),   S(  6,  -4),   S(  3,  20),   S( 11,   1),   S(-29,  22),   S(-19, -39),
            S( -1,  -1),   S(  2,  22),   S( 13,  21),   S(  2,  27),   S( -2,  19),   S( -4,  -3),   S( -7,   0),   S(-13, -27),

            /* queens: bucket 0 */
            S(-22,  -8),   S(-29, -53),   S(-37, -66),   S( -1, -103),  S(  6, -89),   S(  7, -74),   S(-59, -25),   S(-17, -23),
            S(-23, -38),   S(  7, -60),   S( 11, -82),   S(-17, -14),   S(  4, -19),   S( -8, -38),   S(-41, -49),   S(-64, -33),
            S(-17,  21),   S( -5, -30),   S( 29, -64),   S( -2, -39),   S(-12,  -9),   S(  0, -20),   S(-20, -27),   S(-88, -44),
            S(-22,  26),   S( 34, -24),   S( -6,  15),   S(-21,  63),   S( -7,  49),   S(-29,  27),   S(-38, -30),   S(-19, -44),
            S(-17,  -1),   S(  1,  57),   S( 19,  71),   S(-23,  74),   S( 19,  43),   S(-30,  63),   S(-39,  37),   S(-46,   5),
            S(-23,  21),   S( 24,  56),   S( 33,  52),   S(-11,  94),   S(-60,  71),   S(-57,  78),   S(-84,  11),   S(-63, -11),
            S(  0,   0),   S(  0,   0),   S( 43,   9),   S(-20,  46),   S(-12,  29),   S(-84,  79),   S(-81,  73),   S(-115,  12),
            S(  0,   0),   S(  0,   0),   S( 20,  17),   S(  8,   8),   S(-42,  42),   S(-47,  20),   S(-63,   8),   S(-43, -29),

            /* queens: bucket 1 */
            S( 26, -24),   S(  3, -31),   S(  9, -63),   S( 26, -105),  S( 20, -38),   S(  3, -36),   S( 18, -14),   S(-16,  15),
            S(-52,  60),   S( 22, -17),   S( 33, -53),   S( 24, -16),   S( 50, -46),   S( -6,  -1),   S(-19,  19),   S(-15,  -6),
            S( 46, -26),   S( 14, -17),   S( 17,   5),   S(  6,  71),   S(-17,  43),   S( 29,  22),   S(-20,  50),   S(  5, -20),
            S( 24,  19),   S( 12,  55),   S(  0,  56),   S( 23,  74),   S( 14, 102),   S(  5,  34),   S( 17,  57),   S(-31,  81),
            S( 38,  23),   S( 33,  76),   S( 47,  74),   S( 41,  50),   S( 48,  95),   S( 67,  38),   S(  6,  69),   S( -5,  81),
            S( 46,  39),   S( 75,  49),   S(105,  97),   S(130,  87),   S( 36, 117),   S( 21,  81),   S( 60,  42),   S(-18,  41),
            S( 74,  42),   S( 74,  57),   S(  0,   0),   S(  0,   0),   S(-15,  62),   S(-10,  49),   S(-38,  97),   S(-37,  49),
            S(110,  16),   S( 58,  23),   S(  0,   0),   S(  0,   0),   S( 31,  45),   S( 41,  64),   S( 95,  11),   S(-21,  29),

            /* queens: bucket 2 */
            S( 15, -33),   S( 29, -27),   S(  8,  34),   S( 39, -41),   S( 37, -45),   S( 23, -51),   S(-14, -21),   S( 38,  11),
            S( 16,  26),   S(  9,  21),   S( 36, -15),   S( 40,  15),   S( 53, -11),   S( 12,  21),   S( 34, -17),   S( -1,  54),
            S( 10,  53),   S( 19,  54),   S( 16,  85),   S( 16,  53),   S( 18,  44),   S( 19,  54),   S( 37,   1),   S( 41,  15),
            S( 30,  38),   S( 25, 103),   S( 15,  82),   S(  6, 119),   S( 27,  96),   S( 10, 105),   S( 15,  85),   S( 34, 104),
            S( -2,  78),   S( 20,  55),   S( 13, 126),   S( 30, 118),   S( 25, 137),   S( 96,  62),   S( 68, 102),   S( 71,  72),
            S(-34,  89),   S(-28,  97),   S( -9, 116),   S( 67,  94),   S( 96,  43),   S(103, 131),   S(116,  74),   S( 17, 152),
            S(-30,  92),   S(-37, 118),   S(-16, 108),   S( 93,  66),   S(  0,   0),   S(  0,   0),   S( 26, 131),   S( 24, 121),
            S(-16,  66),   S(  8,  47),   S( 57,  27),   S( 59,  67),   S(  0,   0),   S(  0,   0),   S( 73,  79),   S( 13,  88),

            /* queens: bucket 3 */
            S(-62,  16),   S(-45,  41),   S(-33,  22),   S(-18,  32),   S(-27, -10),   S(-25, -26),   S(-16, -58),   S(-66,  24),
            S(-54,  24),   S(-43,  24),   S(-28,  38),   S(-19,  60),   S(-16,  47),   S(-18,   9),   S(  8, -24),   S( 18, -71),
            S(-63,  52),   S(-44,  70),   S(-34,  98),   S(-45, 125),   S(-44,  96),   S(-33,  79),   S(-16,  53),   S(-22,  22),
            S(-40,  43),   S(-54,  99),   S(-55, 135),   S(-38, 142),   S(-39, 129),   S(-28,  90),   S(-18,  78),   S(-24,  88),
            S(-45,  67),   S(-51, 117),   S(-51, 134),   S(-39, 172),   S(-34, 150),   S(-17, 152),   S(-29, 159),   S(-30,  88),
            S(-69,  92),   S(-58, 128),   S(-85, 161),   S(-58, 183),   S(-56, 173),   S( 13, 135),   S(-38, 181),   S(-33, 169),
            S(-89,  84),   S(-94, 132),   S(-96, 186),   S(-93, 172),   S(-86, 199),   S(-14, 100),   S(  0,   0),   S(  0,   0),
            S(-147, 145),  S(-101, 123),  S(-53,  83),   S(-79, 128),   S(-62, 131),   S( -7,  83),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-37,  13),   S(-51, -23),   S( 10,   9),   S( -7, -34),   S(-14,  12),   S( -4,  27),   S(-74, -48),   S( 10,  21),
            S(-14, -14),   S(-24,   5),   S( -2,   3),   S(-34, -15),   S(-35,  19),   S(-15,  17),   S(-22, -11),   S( -7, -36),
            S( 11,  32),   S(-13, -41),   S( 27,   3),   S( 11,  19),   S( 62,  33),   S( -4,   8),   S(-15, -41),   S(  0,   6),
            S(  2, -14),   S( -4, -24),   S( 20,  47),   S( 14,  48),   S( 42,  41),   S( -6,  18),   S( -9,  -3),   S(-25,  -1),
            S(  0,   0),   S(  0,   0),   S( 49,  31),   S( 66,  48),   S( 11,  37),   S(  1,  30),   S(  3,  18),   S(  7,  11),
            S(  0,   0),   S(  0,   0),   S( 48,  40),   S( 62,  40),   S( 19,  33),   S( 32,  70),   S(  0,  10),   S(  8, -15),
            S( 28,   2),   S( 22,  17),   S( 80,  66),   S( 69,  42),   S( 77,  28),   S(  3,  17),   S(  8,  -8),   S(-54, -29),
            S( 48,  24),   S( -5, -21),   S( 53,  27),   S( 43,  24),   S( -3, -23),   S(-20, -29),   S(-42, -24),   S( 19,   7),

            /* queens: bucket 5 */
            S( 31,  23),   S( 29,   4),   S(-14, -11),   S(-47,  14),   S( 15, -34),   S( 50,  56),   S( 10,  10),   S(  7,   0),
            S( -3,   0),   S( -9,  22),   S(  6,  -3),   S(  8,  31),   S(  9,  48),   S( -9,  20),   S( -1,   4),   S( 20,   6),
            S( 61,  48),   S( 63,  38),   S(  1,  23),   S(-15,  29),   S( 23,  34),   S( 67,  36),   S(  1,  36),   S( 11,  13),
            S( 32,  -5),   S( 55,  45),   S( 20,  13),   S( 70,  53),   S( 73,  36),   S( 43,  28),   S( 44,  71),   S( -9,  15),
            S( 58,  32),   S( 42,  -5),   S(  0,   0),   S(  0,   0),   S( 34,  30),   S( 24,  42),   S( 42,  52),   S( 12,  26),
            S( 63,  46),   S( 82,  81),   S(  0,   0),   S(  0,   0),   S( 57,  53),   S( 76,  73),   S( 60,  18),   S( 52,  42),
            S( 91,  24),   S( 81,  45),   S( 82,  75),   S( 29,  50),   S( 63,  47),   S(120,  71),   S( 76,  66),   S( 15,   6),
            S( 62,  44),   S( 59,  26),   S( 85,  47),   S( 61,  58),   S( 62,  50),   S( 62,  70),   S( 61,  60),   S( 65,  30),

            /* queens: bucket 6 */
            S( 46,  45),   S(-15,  -6),   S( 39,  10),   S( 44,  17),   S( 37,  10),   S(-18,   4),   S(-29,  -4),   S( 17,  34),
            S( 40,  33),   S( 37,  17),   S( 42,  48),   S( 49,  18),   S( 46,  32),   S( -8,  17),   S(-18,  25),   S( 31,  41),
            S( 11,  54),   S( 33,  20),   S( 47,  37),   S( 50,  26),   S( 44,  45),   S( 27,   7),   S( 77,  47),   S( 63,  72),
            S( -6,  30),   S(-12,  36),   S( 67,  24),   S(101,  62),   S( 73,  36),   S( 62,  41),   S(110,  39),   S(119,  61),
            S( -8,  18),   S( 34,  78),   S( 62,  84),   S( 76,  72),   S(  0,   0),   S(  0,   0),   S( 80,  57),   S(114,  81),
            S( 44,  53),   S( 54,  76),   S( 77,  84),   S( 51,  57),   S(  0,   0),   S(  0,   0),   S(106,  85),   S(110,  44),
            S( 73,  18),   S(  2,  76),   S( 95,  47),   S( 89,  49),   S( 49,  43),   S(101, 103),   S(117,  69),   S(144,  49),
            S( 17,  25),   S( 69,  12),   S( 91,  62),   S(131,  74),   S(159,  79),   S(116,  67),   S(116,  60),   S(113,  32),

            /* queens: bucket 7 */
            S( -8,   0),   S(-14, -33),   S(-19, -14),   S(-22,  27),   S( 24, -19),   S(  0,  29),   S( -6,   3),   S(-12,  -7),
            S(-29,  -9),   S(-45,  17),   S(-36,  74),   S( -2,  54),   S(-27,  59),   S( -7,  45),   S(  5,  21),   S(-16,  25),
            S( -6,   9),   S( 10,  12),   S( 17,  50),   S( 53,  24),   S( 38,  42),   S( 21,  27),   S( 56,  -3),   S( 79,  47),
            S(-38,   7),   S(-15,  25),   S( 10,  74),   S( 73,  49),   S( 92,  41),   S( 91,  25),   S( 84,  -6),   S( 50,  -2),
            S( -2,   4),   S(-32,  53),   S( -9,  87),   S( 46,  75),   S(113,  28),   S( 95,  58),   S(  0,   0),   S(  0,   0),
            S( -1,  40),   S(-10,  62),   S(  9,  73),   S( 29,  72),   S( 62,  57),   S(106,  86),   S(  0,   0),   S(  0,   0),
            S(-64,  69),   S(-35,  23),   S(-23,  60),   S( 53,  53),   S( 61,  95),   S(113,  58),   S( 65,  23),   S( 86,  68),
            S(-12,  30),   S( 25,   1),   S( 51,  26),   S( 51,  14),   S( 54,  55),   S( 61,  21),   S(-13,  53),   S( 75,  46),

            /* queens: bucket 8 */
            S(-17, -21),   S( 10, -16),   S( -7, -24),   S(  9,  18),   S(  7,  -6),   S(  9,  -6),   S(  5,  -9),   S( -2,   0),
            S( -7, -17),   S( -1,  -9),   S(  0, -18),   S( -1,   8),   S( 29,  34),   S( -5, -15),   S(  8,  14),   S(  3,   3),
            S(  0,   0),   S(  0,   0),   S(  3, -11),   S(-17, -30),   S( 20,   9),   S( 17,  -1),   S( -7,   5),   S( -9,  -6),
            S(  0,   0),   S(  0,   0),   S( 14,  21),   S(  3, -13),   S( -2,   9),   S(  4, -11),   S(  7,  15),   S(-11, -22),
            S( -1,  -4),   S( 14,  23),   S( 15,  28),   S( 14,  21),   S( -7, -16),   S(  3, -11),   S(  3,   3),   S( -4, -10),
            S(  4, -16),   S( -2, -20),   S( 34,  44),   S( -8, -36),   S( 20,  16),   S( 12,   3),   S( -7, -25),   S( -9, -12),
            S(-15, -23),   S( -4, -11),   S(  0,   8),   S( 25,  42),   S( 13,   1),   S(  5,  21),   S(  5,   5),   S(  2,  -8),
            S( 10,   7),   S(  7,  -6),   S( 22,  10),   S( 19,  11),   S( 17,  26),   S(-15, -33),   S(  0,  -3),   S(-13, -27),

            /* queens: bucket 9 */
            S(  1, -13),   S(-21, -30),   S( -6, -24),   S( 48,  47),   S(  5,   3),   S( -1,  -8),   S(-10, -21),   S( -4, -26),
            S( 11,  28),   S( 11,   5),   S(  9,  -8),   S( 14,  13),   S(-24, -46),   S(  0, -21),   S( -3,  -2),   S( -5, -21),
            S(  5,   0),   S(-10, -13),   S(  0,   0),   S(  0,   0),   S(  8,  -2),   S( 23,  28),   S(  4,  -5),   S( 10,   7),
            S( 12,  13),   S( -7, -21),   S(  0,   0),   S(  0,   0),   S(  4,   9),   S( 28,  25),   S( 22,  19),   S(-13,  -6),
            S( -1, -28),   S(  7,   2),   S(  3,  -2),   S(-16, -15),   S( -5, -27),   S( 35,  61),   S( 11,  -9),   S(  2,  -5),
            S( 10,  12),   S( 10,   3),   S(  8,  -4),   S( -1, -21),   S(-10, -16),   S( -6, -24),   S( -6, -16),   S(  1, -16),
            S( 17,   0),   S( 33,  18),   S( -1,  -6),   S( 11,  27),   S(  9,  -8),   S( 30,  38),   S(  6,  27),   S(  5, -20),
            S( 18, -31),   S( 25,   4),   S( -5,   1),   S(  5,   1),   S( 37,  34),   S( -6,  -5),   S( 24,  -7),   S(  5,  -5),

            /* queens: bucket 10 */
            S( 16,  13),   S( 24,  20),   S(  3,  -1),   S(  1, -26),   S(  1, -14),   S( 17,  27),   S(-21, -45),   S( 11,   0),
            S(  0, -11),   S(-22, -19),   S( 13,   2),   S( -1, -31),   S( -8,   2),   S( 15,  -7),   S(-18, -33),   S(  4,   0),
            S( -4,  -4),   S( 14,  18),   S( 12,  23),   S(  8,  -2),   S(  0,   0),   S(  0,   0),   S(  7,   5),   S( -9, -12),
            S( -5, -10),   S( 19,  26),   S(  6,   2),   S(  6,   2),   S(  0,   0),   S(  0,   0),   S( -2,   2),   S( 13, -12),
            S( -4,  -5),   S( 22,  20),   S(  3,  12),   S( 54,  62),   S( 15,  35),   S( -4,   3),   S(-13, -30),   S( 11, -23),
            S(-12, -12),   S(  1, -10),   S( 24,  36),   S( 16,  17),   S( 12,  21),   S( 13,  28),   S(  9,  14),   S(  1, -20),
            S( 12,   2),   S( 15,  20),   S( 25,  50),   S( 33,  40),   S(  6,  11),   S( 34,  28),   S( 19,  14),   S(  7,  -1),
            S(-14, -36),   S(  7,  10),   S( 20,  16),   S(  3,   4),   S( 20,  23),   S( 20,  25),   S(  7,   1),   S( 22,  10),

            /* queens: bucket 11 */
            S( -9,  -6),   S(  3,   7),   S( -1,  -7),   S(  2,  11),   S( 11,  13),   S(-35, -42),   S( 10,   6),   S( -6,   1),
            S(-13,   5),   S( 15,   6),   S(-21, -15),   S(-13,  -1),   S( 29,  25),   S( 10,  20),   S( 30,  29),   S( 10,  11),
            S( 10,   7),   S( 10,  15),   S(-39,   1),   S(-24, -31),   S( 16,  15),   S( -6, -23),   S(  0,   0),   S(  0,   0),
            S(  4,   5),   S(-17,  -6),   S(-15, -32),   S( -2,   0),   S( 10,  -2),   S(  0,   8),   S(  0,   0),   S(  0,   0),
            S( -5,   6),   S( 13,  11),   S(  3,  12),   S( -1,   0),   S( 49,  54),   S( 15,  51),   S( 11,  12),   S( -4, -19),
            S( 12,   8),   S(-18, -23),   S(-12,   5),   S( -4,  -3),   S(  8,   7),   S(  7,   6),   S( 19,  25),   S( 16,   8),
            S(-16,  -7),   S(  4,  18),   S( 12,  26),   S( -2,  -8),   S( 12,  54),   S(-25, -15),   S(-28, -28),   S( 33,  25),
            S(-27, -68),   S(  5,   3),   S(-23, -28),   S( -4,  44),   S( 17,  14),   S( 19,   3),   S(-18, -31),   S(  0,  -8),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 10,  18),   S(-13, -22),   S(  0,   2),   S(-10, -23),   S( 12,  18),   S( -3,   0),
            S(  0,   0),   S(  0,   0),   S(  5,  -2),   S(-20, -30),   S( -7, -12),   S( -4, -16),   S(-10, -22),   S(  3,   1),
            S( -1,  -1),   S(  4,   5),   S( -8,  -3),   S(  8,   4),   S( 17,  32),   S( -3,  17),   S( -6, -10),   S(  9,  11),
            S(  4,  -8),   S( 10,   8),   S( 11,  11),   S(  2,  22),   S(  9,   4),   S(-11,   5),   S( -1,  -6),   S( -4,  -7),
            S( -9, -15),   S( -3,  -4),   S( -2, -11),   S(  7,  -6),   S(  0,  11),   S(-27, -61),   S(-18, -35),   S( -3,  -4),
            S(  9,   9),   S(  3,   0),   S( -7, -14),   S( -3,   3),   S( -8,  -6),   S(-13,   2),   S(-11,  -5),   S( -6, -15),
            S( -7, -18),   S(  0,  -4),   S( -5,  -5),   S(  8,   8),   S(  2,  -2),   S( -8, -10),   S(  6,   8),   S( -8, -41),
            S( 13,  23),   S(  1,  -3),   S(  4,  -4),   S( -4,  -6),   S( -4, -11),   S(-26, -32),   S(-18, -21),   S(-15, -32),

            /* queens: bucket 13 */
            S(-15, -19),   S( -5,  -8),   S(  0,   0),   S(  0,   0),   S( -7, -16),   S( -6, -14),   S(  4,  18),   S( -2,  -6),
            S( -9, -33),   S( -4, -12),   S(  0,   0),   S(  0,   0),   S( -9, -27),   S( -4,  -5),   S( -9, -24),   S(-10, -21),
            S(-13, -43),   S( -4,   5),   S(  3,  15),   S(  7,   7),   S(-16, -31),   S( -4,   5),   S( -1,  -6),   S( -3,   2),
            S(  9,   9),   S(-18, -19),   S(  4,   1),   S(  7,   6),   S( 25,  41),   S( 19,  38),   S(  0, -14),   S(-11, -12),
            S( 14,  -2),   S(  4,  -7),   S( -2,  -1),   S( 11,  27),   S(  5,  11),   S(  6,   0),   S( -1,  -3),   S(  8,   7),
            S(-11, -14),   S(-20, -38),   S( -5, -13),   S(  2,   4),   S( 11,   9),   S(  8,  20),   S(-19, -34),   S( -6, -21),
            S( -8, -16),   S( -1,  -3),   S(  4,  10),   S(-13, -16),   S(-12, -34),   S(  5, -11),   S(-23, -45),   S( -6,   1),
            S(-17, -29),   S(  1,  -1),   S( -5,  -8),   S(  4,   8),   S( -6, -18),   S(-12,  -4),   S(  2,  -2),   S(-13, -24),

            /* queens: bucket 14 */
            S( -1, -11),   S( 20,  18),   S( -4,  -9),   S( -6, -13),   S(  0,   0),   S(  0,   0),   S( -1,  -7),   S(  4,  -9),
            S(-12, -25),   S( -9, -11),   S(-12, -20),   S(  1,  -7),   S(  0,   0),   S(  0,   0),   S( -6, -15),   S(  2, -21),
            S( -1, -10),   S(  3, -16),   S( -3,  -1),   S( -4, -10),   S( -1,  11),   S(  7,  12),   S(-11,  -8),   S( -8, -20),
            S( -7,  -7),   S(-11, -22),   S( 10,  16),   S(-16, -22),   S(  8,  10),   S(-24, -44),   S( 11,   0),   S(-10, -20),
            S(  0,  17),   S( -3,  -8),   S(-12, -29),   S( 23,  26),   S(  5,  23),   S(-14, -17),   S( -3,  18),   S(  4,  10),
            S( -8, -19),   S( -9, -24),   S(-12, -19),   S(-22, -47),   S(  8,  16),   S( -1,  -7),   S( -1, -13),   S(-17, -19),
            S(-13, -17),   S(-12, -29),   S( -3,   1),   S(  3,   4),   S( -4,  -3),   S(  5,   2),   S(-15, -35),   S( -3, -14),
            S(-17, -32),   S( -3, -20),   S(-12, -14),   S( -2,  -4),   S(  2,  -7),   S( -9, -17),   S( -6,  -5),   S( -1, -10),

            /* queens: bucket 15 */
            S( -1,  -3),   S( -5, -15),   S( 10,  11),   S( -7, -18),   S( 12,  12),   S(-11, -18),   S(  0,   0),   S(  0,   0),
            S( -3,  -9),   S(-13, -19),   S( -9,  -7),   S(  0,  -2),   S(  7,   6),   S(  4,   3),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S( 11,   6),   S( -6,  -3),   S( -6,  -5),   S( -5, -23),   S(  7,   9),   S( -2,  -1),   S( -2,  -7),
            S( -1,  -2),   S(-11, -20),   S( -2,   2),   S(  7,   6),   S( 12,  30),   S(  4,  23),   S( -2,   5),   S( -4, -25),
            S( -7, -18),   S( -2,  -8),   S(-17, -25),   S( 11,  17),   S( 18,  49),   S(  5,  21),   S( -5,  -3),   S(-17, -37),
            S(  1,   2),   S( -8, -15),   S( -8, -19),   S(-17, -22),   S(  5,  28),   S(-11,  -6),   S( -2,  -3),   S( -4,  -6),
            S( -2, -11),   S( -1,   2),   S( -4,   1),   S( -6, -16),   S(-11, -15),   S(  2,   7),   S(  2,  -2),   S(  3,   1),
            S(-14, -32),   S(-21, -54),   S( -6, -27),   S(  0,  -2),   S(-24,  -9),   S(-13, -14),   S(  3,   3),   S( -9, -12),

            /* kings: bucket 0 */
            S(-20,   4),   S( 40, -32),   S(  1,   0),   S(-24,  21),   S(-22,  11),   S( 29, -29),   S( -4,   4),   S(  0, -47),
            S(-21,  41),   S(  0, -13),   S(  6,  -4),   S(-35,  18),   S(-52,  55),   S(  2,  11),   S(-17,  38),   S( -6,  32),
            S( 13,   0),   S( 67, -27),   S(  0,  -3),   S( -3, -12),   S( -1,  -9),   S( 30, -16),   S(-66,  30),   S( 48, -44),
            S( -8, -33),   S( 13, -18),   S( 23, -32),   S(-27,   5),   S(-58,  44),   S(-68,  35),   S(-44,  40),   S( -4,  32),
            S(-32, -110),  S(  3, -44),   S(  4, -40),   S(  0, -21),   S(-66,   6),   S(-67,  11),   S(-10,  21),   S(  8, -12),
            S(-17, -118),  S(  5,  12),   S(-11, -46),   S(-37, -20),   S(  2, -24),   S(-26,  40),   S( 22,  26),   S(-20,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -54),   S(  2, -15),   S(-10, -30),   S(-24,  -1),   S(-36,  11),   S(-19,  18),
            S(  0,   0),   S(  0,   0),   S(-13, -16),   S(  9,  -3),   S( 14,  10),   S( -2,   2),   S(  8,  -7),   S( 10,   7),

            /* kings: bucket 1 */
            S( 11, -24),   S( 26, -12),   S( 15, -19),   S( 18,   5),   S( -7,  -2),   S( 20,  -9),   S( -3,   8),   S( 10, -19),
            S(  8,  -6),   S(  5,   7),   S( 16, -17),   S(-52,  32),   S(-18,  16),   S(-13,  16),   S( -2,  23),   S(  2,   7),
            S( -4, -21),   S(  3,  -3),   S(-19,  -5),   S( 23, -21),   S(-26,  -3),   S( -1, -12),   S( 60, -30),   S( 58, -12),
            S( 11,  -3),   S( 16, -23),   S(  6,  -5),   S( -4,   0),   S( 23,  13),   S( -9,  10),   S(  0,   2),   S(-48,  41),
            S( -9, -63),   S(-20, -41),   S( -4, -56),   S(-24, -30),   S( -9, -13),   S( -9, -29),   S(  3, -11),   S(-11,  -8),
            S(-29,  11),   S(-113,   3),  S(-26,  38),   S(  3,  -1),   S(-29, -13),   S(-45,  24),   S( 24,  -5),   S( 10,  14),
            S(-20, -59),   S(-20,   4),   S(  0,   0),   S(  0,   0),   S(-28,  32),   S(-55,  22),   S(  3,  52),   S(  7, -26),
            S(-31, -132),  S(  1,   7),   S(  0,   0),   S(  0,   0),   S( -8,  10),   S(-15,  29),   S( -8,  28),   S( -7, -61),

            /* kings: bucket 2 */
            S(  9, -53),   S(  7,  -3),   S( 15, -22),   S( 26, -13),   S( -8,  11),   S( 32, -22),   S( -9,  22),   S( 15, -27),
            S( 50, -34),   S(-11,  27),   S( -5,   5),   S(-22,  10),   S(-25,  15),   S( -6,   1),   S(  6,  -1),   S(  0,   0),
            S(-39,  -1),   S(-15,  -9),   S(-13,  -8),   S( -3, -19),   S(-10,  -6),   S( 13, -24),   S( 27, -15),   S( 20, -18),
            S(  8,  32),   S(-43,  14),   S( -8,   8),   S(-32,  11),   S( 45,  -4),   S( -7, -11),   S( 57, -34),   S( 50, -10),
            S( -4, -15),   S( 23,  -6),   S( 12, -46),   S( 14, -21),   S( 14, -43),   S( -6, -45),   S( 28, -54),   S( 21, -46),
            S(  2, -13),   S(-21,  -8),   S(-23,  -6),   S(-25,  -9),   S( 12,  10),   S( -7,  15),   S(-85,   4),   S(-11, -19),
            S( -7, -23),   S(-22,  35),   S(-60,  -5),   S(-16,  19),   S(  0,   0),   S(  0,   0),   S( -8,  16),   S(-46, -26),
            S( -4, -27),   S(-11, -14),   S(-33, -24),   S( -5,  18),   S(  0,   0),   S(  0,   0),   S( -6,  -9),   S(-45, -130),

            /* kings: bucket 3 */
            S( 13, -69),   S( 22, -15),   S( 29, -28),   S( -1, -10),   S( -2, -16),   S( 40, -23),   S( -2,  18),   S(  4, -33),
            S(  5,  18),   S(-17,  45),   S( -4,   1),   S(-37,  18),   S(-46,  30),   S( 10,  -5),   S( -3,  18),   S(  8,  10),
            S( 28, -34),   S(-14,  16),   S(-10,  -8),   S(-33,   0),   S(-14,  10),   S( 41, -28),   S( 56, -20),   S( 52, -15),
            S(-39,  25),   S(-67,  38),   S(-62,  21),   S(-34,   6),   S(-38,  18),   S(  7, -30),   S(-46,   4),   S(-20, -13),
            S(-20,  17),   S(  3,   3),   S(-35,  -8),   S(-37,  -7),   S( 37, -39),   S( 71, -69),   S( 24, -64),   S(-15, -82),
            S(-21,  -1),   S(  4,   2),   S( 32,   1),   S( 13, -31),   S( 51, -44),   S( 64, -51),   S( 79,   9),   S( 39, -111),
            S(-28,  -8),   S( 32,   1),   S( 22, -11),   S( 24, -17),   S( 27, -44),   S( 39, -68),   S(  0,   0),   S(  0,   0),
            S( -4, -13),   S(  2, -14),   S(-13,  13),   S(  2, -19),   S(  0, -69),   S(  2,   9),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-63,  -6),   S( -7,  33),   S(-16,  41),   S( 42,   3),   S( 24,  -4),   S( 16, -20),   S(  7,   6),   S(  9, -30),
            S(-37,  28),   S( 19,  22),   S(-36,  14),   S(-16,  -6),   S( 44, -12),   S(  6,   2),   S( 58, -28),   S( 16,   4),
            S( 20,  28),   S( 13, -23),   S( 36,  -7),   S(-22,   6),   S( -3,   9),   S( -2, -14),   S(-55,  10),   S( 28,  -9),
            S( -2, -15),   S(-31,  10),   S( 12,   4),   S( -4,  12),   S(-20,  12),   S(-30,  26),   S( 28,   6),   S( 26,   4),
            S(  0,   0),   S(  0,   0),   S( -9,  -4),   S(-23,  18),   S(-42,  16),   S(-23, -12),   S(-15, -12),   S(  7,  10),
            S(  0,   0),   S(  0,   0),   S(  1,   9),   S(-14,  14),   S( -7,  36),   S(-28, -25),   S( -3,   3),   S(  6,   2),
            S( -8, -38),   S( -3, -10),   S( -2, -23),   S( 10,  35),   S(  5,  23),   S(-27,  -6),   S( -8,  26),   S(  5,  13),
            S( -9,  -4),   S(  1,  -2),   S( -8, -24),   S( -3,   8),   S(  6,   4),   S( -9,  -7),   S( -9,  -9),   S(  5,   4),

            /* kings: bucket 5 */
            S( 33,   3),   S(-24,  10),   S(-24,  22),   S(-61,  41),   S(-38,  31),   S( -8,  10),   S( 26,   4),   S( 19,  -1),
            S(-34,  19),   S( 30,  10),   S( 37, -12),   S( 34, -12),   S( 37, -12),   S( 61, -16),   S( 23,   5),   S( 72, -30),
            S(-21,   6),   S( -2, -12),   S( -7,  -4),   S( -2,  -3),   S( 26,  -9),   S(-28,  -1),   S(-14,  15),   S( 28,  -6),
            S(-11,   6),   S( 12, -14),   S(  3,  -6),   S(  5,  12),   S( -6,  29),   S( -1,  -1),   S(  1,   7),   S( 20, -14),
            S(  7, -30),   S(-24, -46),   S(  0,   0),   S(  0,   0),   S( -6,  -6),   S(-35,   1),   S( -1, -12),   S(-15,  -5),
            S(-15, -21),   S(-37, -22),   S(  0,   0),   S(  0,   0),   S(-28,  24),   S(-35,  -8),   S( -3,  -7),   S(-16,   5),
            S(-13, -16),   S(-46,  13),   S( -6,  38),   S(  8,   3),   S(-21,  38),   S(-43,   4),   S(-12,  27),   S(  6,  -6),
            S(-12, -87),   S(-26, -23),   S( -8, -35),   S(-10, -50),   S(-16, -16),   S(-19,  29),   S(-10, -26),   S(  2,  19),

            /* kings: bucket 6 */
            S( 31, -27),   S( -2,   8),   S(-19,  -1),   S( -3,  14),   S(-27,  25),   S(-31,  26),   S(-16,  29),   S( -1,  -2),
            S( 53, -41),   S( 16,   5),   S( 12,  -8),   S( 20, -10),   S( 29,  -9),   S(-17,  12),   S( 14,   1),   S( -6,   7),
            S(  1, -15),   S( -9,  -2),   S( -9, -14),   S(-35,   6),   S(-16,  -4),   S(-43,   3),   S(  5,   2),   S(-14,  12),
            S( 49,  -9),   S( 14,  -5),   S( 31, -15),   S( 24,   1),   S( 75,  -3),   S(-15,   2),   S(-14,   5),   S( -6,  12),
            S(  6, -33),   S(  0, -13),   S(-27, -19),   S( 12, -11),   S(  0,   0),   S(  0,   0),   S(-37, -19),   S(-28, -21),
            S(-10, -13),   S( 12,  -8),   S(-17,  -1),   S( -5, -15),   S(  0,   0),   S(  0,   0),   S(-46, -28),   S(-47,  -7),
            S(  2, -33),   S(-11,  10),   S(-48,  25),   S(-20,  -6),   S( -4,   3),   S( -3, -22),   S(-39,   9),   S( 11, -60),
            S(  5,   1),   S(  5, -14),   S(-16,  -6),   S(-11, -28),   S( -5, -35),   S( -9, -53),   S(  3,  35),   S(  3, -56),

            /* kings: bucket 7 */
            S( 25, -17),   S(  1, -18),   S(-14,  -9),   S(-46,  23),   S(-33,  14),   S(-64,  44),   S(-27,  26),   S(-35,  15),
            S( 22,   1),   S( 24, -27),   S( -5,  -7),   S( -8,  -5),   S(-13,  -2),   S(-40,  22),   S( -3,  -2),   S( -5,  11),
            S( 43, -34),   S(-29,  -3),   S( -2,  -8),   S(-43,   4),   S(-52,   2),   S(-28,   9),   S( 38, -10),   S(-60,  22),
            S(-51,  32),   S( 11,  -4),   S(-20,   8),   S( 31,  -8),   S( 44,  -9),   S( 63, -31),   S( 21,  -9),   S(  4,  -6),
            S(-32,  27),   S(-10,  -1),   S( 11, -31),   S( 25, -26),   S( 22, -37),   S( 11,  -3),   S(  0,   0),   S(  0,   0),
            S( -8, -28),   S( -7,  -6),   S( 14, -19),   S(-17,   3),   S(  8, -12),   S( 28, -12),   S(  0,   0),   S(  0,   0),
            S(  6,  22),   S(  8,   1),   S(  1,  19),   S(-24, -30),   S( -4, -10),   S(  4, -23),   S(  5, -31),   S(-11,  25),
            S(  5,  10),   S(-19, -15),   S( 13,  38),   S(-10,  -9),   S( 10,   6),   S(-13, -50),   S(  0, -30),   S(-15, -66),

            /* kings: bucket 8 */
            S(  1,  92),   S(-24,  84),   S( 41,  39),   S( -2,   4),   S(-16,  28),   S(-15,   6),   S( 44, -34),   S(-27,   2),
            S( 24,  72),   S( 39,  17),   S( 55,  50),   S( 78,  -1),   S( 13,  23),   S( 19, -15),   S(  6,   9),   S(  8,   2),
            S(  0,   0),   S(  0,   0),   S( 23,  42),   S( 33,  13),   S( 16,   9),   S( -3,  -4),   S(  7,  15),   S( 17, -27),
            S(  0,   0),   S(  0,   0),   S(  1,  64),   S( 10,   3),   S( -3,  37),   S(-20,  26),   S( 28,  -9),   S(  6,  29),
            S( -6, -32),   S( -2,  47),   S(  3,  38),   S(-18,  52),   S( -5, -19),   S(  5, -10),   S( 18,   5),   S(-25, -16),
            S( 10,  12),   S( -3, -27),   S(-14, -47),   S(-24,  12),   S(-22,   7),   S( -7, -14),   S(-17,   1),   S( 15,  -1),
            S( -4, -14),   S(  0, -13),   S(  5,  -7),   S(  6,  11),   S(  1, -42),   S( -6,   6),   S( -5,  -7),   S( -8, -80),
            S( -4,  -6),   S(-12, -37),   S(  0,  -1),   S(-14, -30),   S(  6,  19),   S(-10,  -5),   S( 13,  16),   S(  9,  11),

            /* kings: bucket 9 */
            S( 15,  21),   S(  6,  30),   S(-23,  43),   S( 17,  12),   S(-36,  37),   S( -6,  23),   S( 50,  -8),   S(  8,   2),
            S(-15,  41),   S( 39,  34),   S( -8,   6),   S( 64,   6),   S( 54,  17),   S(  6,  11),   S( -8,  31),   S(-21,  18),
            S(  3,  30),   S( 31,  11),   S(  0,   0),   S(  0,   0),   S( 23,  16),   S( -8,   8),   S( 24,   7),   S( -6,  26),
            S(  4, -54),   S(  8, -26),   S(  0,   0),   S(  0,   0),   S(-10,  29),   S( 34,  -4),   S(-21,  16),   S(-31,  27),
            S( 11, -35),   S( 22, -11),   S( -5,   0),   S( -9,  27),   S(-13,  32),   S(-24,  14),   S(-33,  22),   S( 13,  -4),
            S(  3,  -5),   S( 17,  -6),   S( 15, -24),   S(  0,  -4),   S( -4,   0),   S(-20,  22),   S(-37, -13),   S( 12,  33),
            S(  1, -13),   S(  0, -21),   S(  1, -13),   S( -2, -37),   S( 11, -13),   S( 18,  47),   S( -3, -24),   S( 12,   9),
            S(  2,   5),   S(  3, -11),   S(  6, -26),   S( -2, -24),   S( -7, -44),   S( -6,  11),   S(-13,   9),   S(  4,   3),

            /* kings: bucket 10 */
            S( 38, -26),   S( 15,   9),   S( 14,   3),   S(  3,  20),   S(-16,  16),   S(-88,  47),   S(-60,  52),   S(-48,  71),
            S(  9,  -6),   S( 94, -12),   S( 24,  -8),   S( 49,  16),   S( 77,  14),   S( 16,   8),   S(-16,  30),   S(-100,  57),
            S( 13,  -2),   S( 22,  13),   S( 25,  -6),   S( 19,   1),   S(  0,   0),   S(  0,   0),   S(-14,  13),   S(-71,  31),
            S( 25,  -8),   S( 41, -19),   S( 32, -21),   S( 28,  19),   S(  0,   0),   S(  0,   0),   S( 18,  14),   S(  3,   9),
            S(  8,  17),   S( 20,  18),   S( 53, -30),   S( 21, -41),   S( 12,  -7),   S( 10,  19),   S(  3,   9),   S( -5,  19),
            S( -5,   4),   S(  7,  -3),   S(-12,   0),   S(  0,   4),   S(  4,   1),   S(-25, -12),   S(-16,  -6),   S( -8,  -4),
            S(  2, -38),   S(  5, -21),   S( 13, -24),   S( 19, -21),   S( 11, -24),   S( -1, -15),   S( 10, -32),   S( -1,  17),
            S(  2,   3),   S(  6,  -1),   S( -3, -25),   S( -7,  -1),   S(  9, -22),   S( -3, -28),   S(-12, -26),   S(  7,  -2),

            /* kings: bucket 11 */
            S(  9, -25),   S( -5, -11),   S( -7,  -5),   S(-10,   7),   S(  0,   4),   S(-73,  60),   S(-49,  76),   S(-150, 140),
            S(  3, -30),   S( -7,  27),   S(-33,  -7),   S( 24,  14),   S( 55,   6),   S( 62,  45),   S( 54,   6),   S( 26,  36),
            S( -4, -40),   S( -7,  18),   S(  4, -18),   S( 31,   3),   S( 74,  -4),   S( 33,  61),   S(  0,   0),   S(  0,   0),
            S(-12,  -1),   S( 28,  20),   S( -2,  16),   S(  2,  20),   S( 27, -17),   S( 21,  38),   S(  0,   0),   S(  0,   0),
            S(  9,  22),   S( -2, -15),   S( 12, -25),   S( 10, -12),   S( 19, -12),   S(  8,  -3),   S(  3,   8),   S(  1,   4),
            S(  2,   5),   S(  1,   0),   S( 11,  -8),   S( 12,   8),   S( -9,  15),   S(  9, -18),   S(  4,   2),   S( -8, -13),
            S(-12,  14),   S(  7, -17),   S( 10,   9),   S( -4, -27),   S( 11,  -1),   S( -8,   1),   S(  2,  -8),   S(-10,  -1),
            S(  2,  -2),   S( 16,  14),   S(-16, -26),   S(  7, -13),   S(-11, -22),   S( -3, -29),   S( -2, -24),   S(  2,   7),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 21,  48),   S( -2, -10),   S(  1,   2),   S(  4, -13),   S( 11, -28),   S( -7,  28),
            S(  0,   0),   S(  0,   0),   S( 51, 140),   S(  0,   1),   S( 24,  35),   S( 16,   0),   S( 17,   6),   S(-25,  28),
            S(  1,   9),   S(  0,  -8),   S( 25,  69),   S( 41,   6),   S(  3, -16),   S( 10,   1),   S( 15, -10),   S(-16, -16),
            S(  4,  20),   S( 13,  34),   S( 13,  24),   S( 18,  16),   S( -7,  23),   S( -6,  27),   S( -2,   9),   S(  3,  -1),
            S(  3,  20),   S(  8,  28),   S(  4,  15),   S(  3,  37),   S(  7,  48),   S( -6, -26),   S( -6,  10),   S( -5,  -7),
            S(  5,   5),   S(  3,  17),   S( -4,   1),   S( -8, -14),   S(  0,  -8),   S(-16,  42),   S(-15, -17),   S( 13,   6),
            S(  8,  11),   S( -8, -15),   S(  2,  18),   S(-10,   0),   S( -3,   3),   S(  1, -20),   S(  4,  15),   S( -4, -24),
            S(  0,   6),   S( -1,  -3),   S(  0,  13),   S( -6,   4),   S(  7,  24),   S(  3,   0),   S( -8, -20),   S(-10, -33),

            /* kings: bucket 13 */
            S( -4,  51),   S( 16,  57),   S(  0,   0),   S(  0,   0),   S( 34,  26),   S( 15, -17),   S( -4, -14),   S(  3,  50),
            S( -9, -10),   S(  0, -10),   S(  0,   0),   S(  0,   0),   S( 32,  -6),   S( 35,  -7),   S(-37,  15),   S(  2, -14),
            S(  0,  -3),   S(  4,   8),   S( -1, -10),   S( 13,  39),   S( 59,  13),   S( 26,  -1),   S(  2,   0),   S( 20, -17),
            S(-14,   2),   S( 13,  -4),   S(  6,  25),   S( -1,  -9),   S(  3,  -2),   S(  7,   4),   S(  7,  23),   S( -8, -24),
            S(  8,  -4),   S( -2,  18),   S( 24,  77),   S(-16,   3),   S(  0,  36),   S(  4,   4),   S( -4,   5),   S( 18,  28),
            S(  8,  -2),   S(-10,  16),   S( -8,  14),   S( -3,   3),   S(-20, -10),   S( -5,  41),   S(-13,  28),   S(  0,  -3),
            S( 14,  13),   S( -5, -26),   S(-16, -59),   S(  1,  21),   S(-11,  -1),   S(-16,  21),   S(-17, -46),   S(-10,  -7),
            S( -1,  -7),   S(  6, -15),   S(  6,  16),   S(  9,  23),   S( -1,  19),   S(-10,  13),   S( -6,   4),   S( 10,  23),

            /* kings: bucket 14 */
            S( 11,  48),   S(  0,  -7),   S( 15, -24),   S( 11, -25),   S(  0,   0),   S(  0,   0),   S( 11,  85),   S(-27,  57),
            S( -2, -13),   S( 12,  -8),   S( 44, -49),   S( 36,   6),   S(  0,   0),   S(  0,   0),   S(  9,  20),   S(-60,  15),
            S( -7,  -6),   S( 11,  -7),   S( 38, -21),   S( 39,   2),   S( 11,  14),   S( 12,  36),   S( 18,  62),   S(-34,  12),
            S( 10,   0),   S( 11,  -2),   S(  2, -29),   S( 19,  -7),   S(-20,  -7),   S( 17,  62),   S( 20,  17),   S(  1, -15),
            S( 11,  26),   S( -2, -29),   S(  1,  17),   S(-12, -26),   S( -3,   3),   S( -4,  68),   S(  6,  51),   S( 14,   8),
            S(-10,   5),   S( 10,  21),   S( -1,   8),   S( -3,  -1),   S( -6,  12),   S( -5,   4),   S(  0,   8),   S(-10, -18),
            S(  3,  16),   S(-16, -26),   S(  5, -32),   S( 20, -22),   S(  4,  -8),   S( -6,  29),   S(-27, -11),   S( 13,  26),
            S( -1,   4),   S(  6,   0),   S( 11,   2),   S(  2,   0),   S(  6,  -9),   S( -4, -12),   S(-12, -19),   S(  3,  -1),

            /* kings: bucket 15 */
            S( 16,  26),   S( 10,  -4),   S( 10,  -2),   S(-14, -27),   S(  7, -20),   S( -6,  55),   S(  0,   0),   S(  0,   0),
            S(  6, -27),   S(  6, -25),   S( -7, -16),   S( 16,  45),   S( 37,   3),   S( 67, 113),   S(  0,   0),   S(  0,   0),
            S(-13, -28),   S( 20, -11),   S(  8, -17),   S( -4,  11),   S( 16,  14),   S(  9,  83),   S(  3,  25),   S( -6,  26),
            S(  4, -17),   S( -4,  22),   S(  4,   4),   S(  5, -14),   S(-18, -44),   S( 15,  19),   S( 25,  71),   S( -2, -19),
            S( 17,  14),   S(-25,  -3),   S( -7,  13),   S(-12, -56),   S(  3, -15),   S(  5,  61),   S( 10,  18),   S( -9, -21),
            S(  4,  34),   S( -5,  18),   S(  4,  10),   S( 15,  38),   S( -7,  -7),   S( -6,   3),   S(  3,  11),   S(  3,  17),
            S( 13,  30),   S(  2,  26),   S( 14,  33),   S( -3,  -3),   S(  4,   1),   S(  3,   4),   S( -5,  -8),   S(  2,   3),
            S(  0,   2),   S(  2,   4),   S(  6,   2),   S(  5,  -6),   S(  7,  -1),   S(  9,   0),   S(  1,   3),   S(  2,   2),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-21, -154),  S(  2,   8),   S( 12,  78),   S( 21, 114),   S( 28, 135),   S( 32, 155),   S( 37, 154),   S( 46, 143),
            S( 60, 118),

            /* bishop mobility */
            S(  3, -43),   S(  7,  26),   S( 17,  66),   S( 23, 102),   S( 27, 127),   S( 28, 148),   S( 33, 156),   S( 36, 157),
            S( 38, 161),   S( 43, 156),   S( 48, 147),   S( 75, 135),   S( 65, 140),   S( 55, 126),

            /* rook mobility */
            S(-13,  33),   S( -6, 105),   S( -7, 140),   S( -3, 151),   S( -5, 172),   S( -2, 182),   S(  1, 191),   S(  5, 190),
            S(  4, 201),   S(  9, 205),   S( 15, 206),   S( 24, 203),   S( 31, 205),   S( 49, 198),   S( 70, 184),

            /* queen mobility */
            S( 83, -31),   S( 80, -72),   S( 69, 100),   S( 63, 182),   S( 66, 215),   S( 65, 252),   S( 65, 294),   S( 64, 326),
            S( 66, 342),   S( 65, 363),   S( 62, 386),   S( 59, 406),   S( 60, 417),   S( 58, 431),   S( 56, 440),   S( 57, 443),
            S( 65, 439),   S( 64, 440),   S( 85, 422),   S( 90, 418),   S(106, 398),   S(143, 371),   S(132, 370),   S(182, 338),
            S(217, 312),   S(207, 312),   S(156, 267),   S(149, 200),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  7,  14),   S(-20,  48),   S(-37,  53),   S(-36,  74),   S( 20,  15),   S(-13,  20),   S(  1,  58),   S( 24,  35),
            S( 18,  33),   S(  2,  43),   S(-24,  51),   S(-16,  42),   S(  1,  36),   S(-21,  42),   S(-27,  64),   S( 30,  35),
            S( 25,  69),   S( 18,  69),   S(  9,  52),   S( 24,  43),   S(  3,  53),   S(-35,  72),   S(-33, 104),   S( -9,  81),
            S( 35, 107),   S( 45, 117),   S( 25,  81),   S( 10,  55),   S(  6,  63),   S( -2,  90),   S(-53, 133),   S(-79, 156),
            S( 25, 153),   S( 50, 193),   S( 61, 132),   S( 14, 117),   S(-45, 104),   S( 33, 109),   S(-55, 169),   S(-81, 174),
            S( 88, 234),   S( 82, 275),   S(114, 250),   S(129, 256),   S(133, 262),   S(149, 244),   S(122, 269),   S(121, 276),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,   7),   S( -8, -28),   S( -1, -17),   S( -5,   9),   S( 16,  10),   S(-13, -39),   S(-24,   6),   S( -3, -46),
            S(-23,  20),   S( 26, -26),   S( -7,  39),   S( 12,  14),   S( 34, -13),   S(-10,  20),   S( 19, -17),   S( -1,  -8),
            S(-18,  30),   S( 22,  -9),   S( -4,  54),   S( 17,  49),   S( 24,  34),   S( 28,  13),   S( 36,   1),   S( -9,  20),
            S( 12,  23),   S( 24,  45),   S( 32, 100),   S(  8,  96),   S( 71,  67),   S( 62,  57),   S( 25,  58),   S( 17,  27),
            S( 61,  79),   S(111, 121),   S(109, 143),   S(130, 155),   S(156, 182),   S(120, 153),   S(112, 117),   S( 83, 105),
            S( 66, 174),   S(123, 277),   S(127, 252),   S(108, 206),   S( 62, 136),   S( 48, 127),   S( 49, 154),   S( 22,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  21),   S( 18,  20),   S( 36,  29),   S( 30,  21),   S( 21,  17),   S( 21,  27),   S(  3,  10),   S( 40,  -4),
            S( -3,  18),   S( 14,  35),   S( 13,  33),   S(  8,  40),   S( 27,  10),   S(  8,  20),   S( 28,  21),   S( -1,  10),
            S( -4,  26),   S( 30,  51),   S( 49,  62),   S( 42,  51),   S( 43,  52),   S( 73,  20),   S( 29,  39),   S( 23,   6),
            S( 62,  75),   S(111,  52),   S(115, 131),   S(145, 137),   S(143, 120),   S( 70, 140),   S( 75,  59),   S( 82,  15),
            S( 43,  94),   S(102, 154),   S(137, 185),   S(105, 278),   S(118, 242),   S( 63, 254),   S(171, 220),   S(-58, 186),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 33,  30),   S( 17,  13),   S( 11,  35),   S( -6,  69),   S( 60,  35),   S( 14,  13),   S(  3,   4),   S( 28,  16),
            S(  1,  15),   S(  5,  11),   S( 19,  18),   S( 16,  35),   S( 14,  18),   S( -1,  11),   S(  5,   8),   S( 27,  -2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -15),   S( -5, -11),   S(-19, -18),   S(-16, -35),   S(-14, -18),   S(  1, -11),   S( -5,  -8),   S(-27,   2),
            S(-33, -30),   S(-17, -13),   S(-11, -35),   S(  6, -69),   S(-60, -35),   S(-14, -13),   S( -3,  -4),   S(-28, -16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -37),   S(-13, -42),   S(-12, -53),   S(-66, -35),   S(-21, -53),   S(-27, -49),   S( -8, -50),   S(-23, -64),
            S(-28, -25),   S(-19, -32),   S(-32, -16),   S( -6, -40),   S(-38, -38),   S(-33, -25),   S(-43, -20),   S(-15, -43),
            S(-20, -21),   S( -6, -36),   S(-25, -11),   S(-31, -24),   S(-21, -49),   S(-23, -25),   S(-14, -23),   S(-40, -30),
            S( -5, -37),   S( 19, -47),   S(  8, -18),   S(  8, -32),   S(  8, -29),   S( 57, -42),   S( 38, -42),   S(-13, -56),
            S( 13, -48),   S( 39, -77),   S( 53, -26),   S( 58, -29),   S( 67, -43),   S( 79, -38),   S(128, -90),   S( 29, -71),
            S(102, -98),   S(128, -116),  S( 84, -46),   S( 85, -32),   S( 68, -26),   S(135, -22),   S( 71, -45),   S( 31, -84),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-12, -24),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 15,   5),        // attacks to squares 1 from king
            S( 15,   6),        // attacks to squares 2 from king

            /* castling available */
            S( 68, -88),        // king-side castling available
            S( 19,  48),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 38, -72),   S( 44, -71),   S( 36, -83),   S( 25, -72),   S( 16, -66),   S(  3, -60),   S(-17, -50),   S(-23, -46),
            S( -6, -59),   S( 11, -56),   S( 31, -59),   S( 30, -56),   S( 62, -67),

            /* orthogonal lines */
            S(-12, -144),  S(-84, -90),   S(-104, -81),  S(-119, -80),  S(-125, -87),  S(-129, -90),  S(-128, -98),  S(-125, -102),
            S(-139, -96),  S(-153, -101), S(-165, -115), S(-138, -135), S(-118, -147), S(-35, -180),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 33, 237),

            /* passed pawn can advance */
            S( -9,  34),   S( -5,  61),   S(  5, 106),   S( 55, 171),

            /* blocked passed pawn */
            S(  0,   0),   S( 44, -17),   S( 33, -11),   S( 32,  32),   S( 29,  60),   S( 37,  28),   S(102,  62),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 25, -31),   S( 26,  27),   S( 29,  24),   S( 22,  57),   S( 29, 101),   S(132, 131),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(  5, -37),   S(-25, -22),   S( 10, -39),   S(-35,  -7),   S(-38,  12),   S( 90,  18),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 44, -34),   S( 19, -25),   S(  2,  -2),   S( 12, -44),   S(  1, -132),  S(-57, -201),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 33,  70),   S( 22,  26),   S(106,  44),   S( 40,  28),   S(184, 117),   S( 95, 137),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 17,  57),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-52, 147),

            /* bad bishop pawn */
            S( -8, -14),

            /* rook on open file */
            S( 40,   8),

            /* rook on half-open file */
            S( 11,  40),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 24,  40),   S( 25,   3),   S( 34,  28),   S( 28,   2),   S( 31, -17),

            /* pawn threats */
            S(  0,   0),   S( 64, 115),   S( 54, 124),   S( 76,  88),   S( 56,  58),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  65),   S( 51,  44),   S( 71,  47),   S( 48,  66),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 20,  56),   S( 22,  61),   S( -5,  37),   S( 65,  59),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 15,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
