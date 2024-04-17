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

        // Solution sample size: 16000000, generated on Wed, 17 Apr 2024 11:52:30 GMT
        // Solution K: 0.003850, error: 0.082311, accuracy: 0.5137
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 73, 226),   S(386, 671),   S(417, 664),   S(541, 1074),  S(1379, 1801), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(105, -122),  S(147, -91),   S( 41, -42),   S(-20,  22),   S(-30,  12),   S(-21,  -2),   S(-49,   2),   S(-25, -19),
            S(123, -125),  S(104, -105),  S(  7, -62),   S(-11, -55),   S(-19, -19),   S(-19, -29),   S(-35, -24),   S(-20, -44),
            S(111, -101),  S( 63, -61),   S( 13, -64),   S( 11, -69),   S( -9, -62),   S(  7, -59),   S( -9, -52),   S(  9, -56),
            S( 72, -39),   S( 51, -57),   S( 26, -59),   S( 20, -83),   S( -9, -45),   S(-12, -55),   S(-17, -42),   S( -2, -27),
            S( 75,  40),   S( 31,  -6),   S( 39, -28),   S( 52, -71),   S( 23, -46),   S( -5, -43),   S(-21,  -7),   S(-27,  50),
            S( 63,  62),   S( 49,  80),   S(  4,   9),   S( 17, -17),   S(-42,  -4),   S(  7,   2),   S( -4,  20),   S( 17,  47),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 34, -30),   S( 33, -38),   S( 53, -23),   S(  6,  20),   S(-14,  -3),   S(  5, -11),   S(-42,   3),   S(-27,  20),
            S( 36, -44),   S( 25, -46),   S( 10, -45),   S( -2, -42),   S(-10, -23),   S( -7, -28),   S(-33, -16),   S(-36, -12),
            S( 31, -40),   S( 11, -29),   S( 15, -53),   S( 11, -56),   S(-22, -26),   S( 15, -49),   S( -8, -33),   S(  6, -26),
            S( 46, -23),   S( 20, -51),   S( 24, -55),   S(  7, -50),   S( -9, -23),   S( 14, -45),   S(-23, -25),   S( -4,   1),
            S( 27,  45),   S(-32,   0),   S( -6, -36),   S( 10, -49),   S( 39, -37),   S( -4,  -8),   S(-24,  20),   S(-19,  68),
            S( 56,  58),   S( 14,   2),   S(-47, -18),   S(-22,  26),   S(-20,  -8),   S(-57,  26),   S(-47,  29),   S(-35,  86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12,  -3),   S(-22,   1),   S( -6,  -2),   S( -6,  12),   S(  6,  -4),   S( 36, -17),   S(  9, -43),   S(  2, -18),
            S( -3, -28),   S(-26, -17),   S(-20, -35),   S(-14, -36),   S(  7, -33),   S(  9, -32),   S(  0, -41),   S(-17, -27),
            S( -4, -27),   S(-20, -29),   S( -7, -56),   S( -1, -56),   S( -7, -31),   S( 24, -45),   S(  5, -41),   S( 14, -32),
            S( -8, -12),   S(-10, -49),   S(-12, -54),   S( -4, -55),   S( 11, -47),   S(  4, -30),   S(  2, -23),   S(  8,  -9),
            S(  1,  32),   S(-40, -11),   S(-41, -44),   S(-42, -36),   S( 13,  -8),   S( -9,   2),   S(-20,  22),   S(-15,  75),
            S(-48,  75),   S(-88,  54),   S(-91,  -7),   S(-67, -21),   S(-39,   6),   S(-19,  19),   S(-11,  -2),   S(-16,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -19),   S(-25,  -5),   S(-21,  -7),   S(  7, -40),   S(  3,  -9),   S( 50, -24),   S( 90, -70),   S( 75, -85),
            S( -5, -45),   S(-24, -32),   S(-20, -45),   S(-13, -35),   S( -4, -33),   S( 15, -41),   S( 61, -75),   S( 64, -78),
            S(  0, -51),   S( -3, -60),   S(  0, -69),   S(  1, -69),   S( -4, -56),   S( 29, -61),   S( 39, -67),   S( 82, -76),
            S(  3, -37),   S(  5, -76),   S(  4, -79),   S(  7, -77),   S( 26, -76),   S( 23, -65),   S( 31, -51),   S( 73, -35),
            S( 30,   4),   S( -4, -39),   S( 13, -80),   S( 17, -73),   S( 91, -69),   S( 77, -45),   S( 59,   5),   S( 59,  61),
            S(-27, 100),   S(-17,   9),   S(  1, -54),   S( -1, -72),   S( 71, -81),   S( 68, -26),   S( 61,   4),   S( 67,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-99,  31),   S(-12,  -9),   S(-38,  15),   S(-11,  23),   S(-12, -22),   S(-49,  24),   S(-45,  -2),   S(-43,   5),
            S(-24,  14),   S( 40, -14),   S( 24, -34),   S(  9, -24),   S( -6, -25),   S(-47, -22),   S(  6, -46),   S(  5, -30),
            S( 32, -11),   S( 38, -10),   S(-19,  11),   S( -3, -27),   S(-31, -33),   S(-12, -39),   S(-15, -46),   S( 27, -42),
            S(  6,  32),   S(-20,  41),   S( 32,   5),   S( -1,   1),   S( 22, -43),   S(-31, -30),   S( 11, -46),   S( 57, -35),
            S(-25,  96),   S(-29,  92),   S(-20,  27),   S(-22,   5),   S(  1,  14),   S(-18,  -2),   S(-32, -35),   S( 39,  19),
            S( 65,  82),   S( 52, 105),   S(  8,  39),   S( 18,  21),   S( 12, -18),   S(  0, -15),   S(  7,   0),   S(-14,  10),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-101,  58),  S(-78,  45),   S(-14,  18),   S( -8,  19),   S(-22,  33),   S(-34,  23),   S(-46,  13),   S(-24,  27),
            S(-55,  20),   S(-62,  21),   S( 26, -12),   S( 10,   9),   S( 14,  -9),   S(-19, -16),   S(-24, -11),   S(-27,   8),
            S(-50,  37),   S(-59,  30),   S( 44, -24),   S(  2, -21),   S( 25, -14),   S(-17, -20),   S( -6, -13),   S( 18, -12),
            S(-59,  56),   S(-57,  35),   S( -5,   6),   S( 15,  11),   S(-15,   5),   S(-50,  -2),   S(  7, -16),   S( 15,  11),
            S( 23,  62),   S( 28,  36),   S( 20,  43),   S( 21,  26),   S(-12,  33),   S( 56,  -6),   S( 13,   7),   S( 47,  27),
            S( 59,  47),   S( 57,  18),   S( 37,  -2),   S( 35,   1),   S( 43, -12),   S( 20,  -3),   S(  8,   5),   S(  5,  50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  30),   S(-43,  19),   S(-35,  18),   S(-30,  18),   S( 33, -20),   S(-34,  15),   S(-63,   7),   S(-59,  22),
            S(-41,   2),   S(-15, -19),   S(-20, -30),   S( -3,  -8),   S( 34, -17),   S( 20, -21),   S(-36,  -7),   S(-64,   8),
            S(-19,  -6),   S(-19,  -9),   S(-17, -21),   S(-35,  -3),   S( 12, -10),   S( 60, -39),   S( -5, -18),   S(-17,   6),
            S(-30,  15),   S(-74,   8),   S( -1, -27),   S(-18,  -7),   S( 13,   0),   S( 32, -13),   S( 19, -10),   S( 37,   4),
            S(  4,  22),   S(-54,  12),   S(  4, -28),   S( -7, -12),   S( 44,  26),   S( 65,  22),   S( 36,   9),   S( 64,  31),
            S( 59,  25),   S( 17,  -2),   S(  1, -36),   S(  6, -37),   S( 19,   2),   S( 21,   4),   S( 39,  -8),   S( 38,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -23),   S(-49, -15),   S(-28,  -7),   S(-51,   9),   S(-15, -23),   S( 24, -22),   S(  1, -49),   S(-40, -22),
            S(-35, -43),   S(-35, -44),   S(-43, -40),   S(-20, -46),   S( -7, -37),   S( 53, -58),   S( 55, -58),   S( -6, -33),
            S(-36, -45),   S(-53, -39),   S(-39, -49),   S(-17, -45),   S( -9, -29),   S( 42, -42),   S( 48, -58),   S( 54, -45),
            S(-11, -48),   S(-46, -53),   S(-74, -46),   S(-44, -28),   S(  1, -32),   S( 24, -22),   S( 25, -17),   S( 73, -26),
            S( 12, -38),   S(  6, -62),   S(-21, -57),   S(  2, -69),   S( 26,  -7),   S( 33,  -3),   S( 66,  43),   S(100,  35),
            S(-15,   3),   S(-30, -34),   S(  4, -56),   S( -3, -56),   S( -2, -17),   S( 27, -23),   S( 49,  39),   S( 90,  58),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-63,  80),   S(-45,  65),   S( 11,  26),   S(-10,  -1),   S( 11,   7),   S( -3,   4),   S(-40,   3),   S(-46,  26),
            S(-66,  71),   S(-63,  61),   S(-32,  44),   S(-15,  13),   S( -9, -12),   S(-34, -18),   S(-49, -10),   S(  5,  -8),
            S(-69, 110),   S(-12, 108),   S(-11,  67),   S(-25,  36),   S( 17, -17),   S(-94, -11),   S(-68, -20),   S(-38,  -7),
            S(-36, 149),   S(  7, 160),   S( 11, 114),   S(  7,  53),   S(-28,   9),   S(-29, -25),   S(-25,  -8),   S(-50,   7),
            S(-17, 178),   S( 41, 163),   S( 24, 167),   S( 56, 100),   S( 19,   6),   S(  1,  -2),   S(-19, -16),   S( -5,  17),
            S( 51, 200),   S( 71, 218),   S( 87, 206),   S( 49,  76),   S(  7,  33),   S(-13,   1),   S(-10, -27),   S(  2,   9),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-105,  82),  S(-74,  55),   S(  5,  15),   S( 12,  31),   S(  9,   8),   S(-49,  20),   S(-78,  16),   S(-78,  32),
            S(-64,  43),   S(-63,  39),   S(-52,  38),   S(  2,  51),   S(-53,   4),   S(-29,  -8),   S(-74,  -5),   S(-28,   8),
            S(-96,  74),   S(-122, 105),  S(-59,  89),   S(-112, 100),  S(-62,  56),   S(-86,  10),   S(-44, -22),   S(-42,   2),
            S(-77, 113),   S(-40, 122),   S( -2, 131),   S( 39, 135),   S(-31,  62),   S(-40,  15),   S( 13,  -3),   S(-44,  19),
            S( 12, 129),   S( 21, 148),   S( 20, 162),   S( 43, 179),   S( 19, 133),   S( -7,  37),   S( -1,  -3),   S(  0,  -1),
            S( 24,  76),   S( 22, 129),   S( 65, 146),   S( 70, 189),   S( 29, 112),   S( -8,  -1),   S(-14,  -8),   S(-21, -21),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-93,  15),   S(-68,  -1),   S( -9,   0),   S(  2,  19),   S(-11,   3),   S(-69,  36),   S(-110,  29),  S(-65,  37),
            S(-100,   7),  S(-82,   6),   S(-17, -14),   S(-25,  -8),   S(-24,  28),   S(-43,  30),   S(-125,  36),  S(-86,  20),
            S(-24, -13),   S(-85,  14),   S(-32,   6),   S(-89,  74),   S(-85,  91),   S(-21,  48),   S(-118,  49),  S(-88,  45),
            S(-100,  31),  S(-79,  26),   S(-14,  14),   S(-45,  84),   S( 15, 101),   S(-57,  88),   S(-32,  51),   S(  1,  28),
            S(-28,  43),   S(-36,  17),   S(  6,  52),   S( 24, 128),   S(100, 115),   S( 46,  73),   S( -9,  89),   S( 28,  48),
            S( -2,  13),   S(-21,  -4),   S( 19,  22),   S( 49, 119),   S( 13, 135),   S( 27,  64),   S( -7,  79),   S( 23, 101),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-75,  -4),   S(-76,  17),   S( 38, -19),   S( -2,  17),   S(  0,  34),   S(-86,  56),   S(-57,  39),   S(-73,  51),
            S(-72, -20),   S(-79, -21),   S(-30, -41),   S(-48,  12),   S(-36,  11),   S(-26,  27),   S(-95,  65),   S(-98,  52),
            S(-36, -34),   S(-59, -33),   S(-48,  -8),   S(-26,   5),   S(-46,  34),   S(-10,  57),   S(-80,  88),   S(-48,  73),
            S(-52,   4),   S(-89, -14),   S(-27, -30),   S(-52,  13),   S(  6,  43),   S( -7,  77),   S( 15, 119),   S( 70,  81),
            S(-22,  22),   S(-48, -10),   S( -8,  -6),   S(-10,  21),   S( 58,  96),   S(-10, 129),   S( 94, 126),   S( 83, 111),
            S(-35,  43),   S(-20,   1),   S(  8, -19),   S(  2,  -1),   S( 20,  70),   S( 32, 156),   S( 67, 188),   S( 32, 181),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15,  15),   S(-17,  11),   S(-19,   0),   S(  2,   6),   S( -3, -11),   S(-11,   9),   S(-14, -21),   S(-18,  -6),
            S(-38, -20),   S( -7,  21),   S(  9,  20),   S( -1,   4),   S(  0,  29),   S( -6, -17),   S(-36, -35),   S(-27, -44),
            S(-17,  44),   S(-36, 101),   S( 20,  68),   S( 20,  39),   S(-13,   1),   S(-46, -19),   S(-44, -51),   S(-43, -61),
            S(-42,  97),   S(-45, 130),   S( 40, 118),   S( 23,  96),   S(-18, -35),   S(-42, -38),   S( -8, -19),   S(-61, -54),
            S( 35, 104),   S( 38, 219),   S( 48, 155),   S( 18,  57),   S( -1,  11),   S( -3, -25),   S( -2,   2),   S(-19, -51),
            S( 47, 117),   S( 54, 224),   S(118, 225),   S( 47,  98),   S( -7,   3),   S(-10,  -9),   S(-11, -29),   S(-23, -39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -16),   S(-21,  16),   S( -6,  10),   S( -2,   5),   S( -8, -10),   S(-30,   4),   S(-35, -45),   S(-24,  -7),
            S(-39,  -8),   S(-57,  49),   S(-24,  37),   S( 21,  22),   S(-45,  23),   S(-14, -14),   S(-81, -28),   S(-62,   6),
            S(-59,  51),   S(-52,  51),   S(-37,  82),   S(-10, 101),   S(  3,  34),   S(-41, -31),   S(-62, -32),   S(-78, -28),
            S(-79,  97),   S( -7, 125),   S( -5, 146),   S(  7, 130),   S(  2,  62),   S(-43,  27),   S(-17, -17),   S(-37, -44),
            S(  2, 101),   S( 52, 176),   S( 66, 202),   S( 49, 255),   S( 23, 153),   S(-11,  15),   S( -4, -67),   S(-25, -43),
            S( 40,  71),   S( 73, 175),   S( 85, 198),   S( 85, 258),   S( 40, 112),   S(  3,  11),   S( -1,   0),   S( -7,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -58),   S(-38, -21),   S( -9, -27),   S( -2,  -2),   S( -5,  -2),   S(-31,  14),   S(-35,  -2),   S( -5,  47),
            S(-53,  11),   S(-56,   8),   S(-54, -29),   S(  1,  11),   S(-38,  67),   S(-16,  20),   S(-41,  19),   S(-56,  15),
            S(-61, -26),   S(-61,   5),   S(-35, -18),   S(-21,  43),   S(-18,  77),   S(-51,  40),   S(-33,   3),   S(-64,  44),
            S(-50,  10),   S(-23,  52),   S(-25,  31),   S(  9, 100),   S( -4, 139),   S(-30,  90),   S(-36,  40),   S(-35,  61),
            S(-21, -24),   S(  8,  14),   S( 14,  78),   S( 36, 138),   S( 47, 222),   S( 41, 179),   S(  7,  90),   S( 25,  44),
            S( -4,  19),   S( 18,  32),   S( 30, 116),   S( 36, 138),   S( 65, 219),   S( 57, 122),   S( 31,  97),   S( 18,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -34),   S(-32, -22),   S(-12, -34),   S(  1,  -3),   S( 17,  21),   S(  1,  47),   S(-10, -21),   S( 10,  26),
            S(-43, -31),   S(-33, -15),   S(-15, -42),   S( 24,  -8),   S(-14,  -1),   S(  7,  46),   S(  5,  30),   S( -1,   3),
            S(-18, -75),   S(-33, -61),   S(-19, -54),   S(  2, -10),   S( 12,  32),   S(-13,  58),   S( -1,  71),   S(-23,  69),
            S(-26, -24),   S(-44, -32),   S(-31,  -3),   S( 11,  17),   S(-11,  51),   S(  6,  94),   S(-28, 145),   S( -6,  61),
            S(-28, -45),   S(-32, -33),   S(-14,  11),   S(  0,  -3),   S( 36, 116),   S( 64, 166),   S( 56, 229),   S( 74,  78),
            S( -9,   5),   S( -4,   9),   S(  1,   6),   S(  7,  23),   S( 26,  82),   S( 85, 194),   S( 34, 181),   S( 42,  42),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-36,   8),   S( -1,  12),   S(-51,  12),   S(-34,  -6),   S(-37,  -9),   S( -5, -28),   S(-49, -43),   S(-28, -14),
            S(-37,  61),   S( 17, -36),   S(-46,  14),   S(  5, -23),   S( -9, -22),   S(-24, -17),   S(-33, -21),   S(-68, -21),
            S(  7,  67),   S( -1,  -8),   S(  2,  -9),   S(-25,  33),   S(  7,   6),   S(-36,   0),   S(-10, -30),   S(-39, -48),
            S( 12,  -9),   S( 42,  10),   S( 10,  27),   S( 21,  28),   S(  8,   4),   S(  0,   2),   S( -5, -15),   S( -2,  -1),
            S( 20, -27),   S( 38,  13),   S( 16,   6),   S( 70, -10),   S( 44,  -8),   S( 31,  18),   S( 25, -13),   S(-58, -10),
            S( 22, -12),   S( 14,  10),   S( 32,  10),   S( 56, -17),   S( 37, -49),   S( 18,  11),   S( 12, -21),   S( -1,  -9),
            S( 19, -25),   S( 17, -35),   S( 20, -26),   S( 37, -30),   S( 24, -16),   S( -5, -27),   S( -9, -40),   S(-18, -28),
            S(-63, -51),   S( -5,   2),   S( -4, -16),   S(  5, -41),   S(-18, -21),   S( 23,  13),   S( -5,   5),   S( 18,   2),

            /* knights: bucket 1 */
            S(-41,  33),   S(-54,  84),   S(  7,  42),   S(-42,  64),   S(-27,  45),   S(-31,  26),   S(-39,  47),   S(-15,  -6),
            S( 36,  31),   S(-13,  40),   S(-10,  26),   S(-10,  42),   S( -9,  22),   S(-17,  12),   S( 14, -14),   S(-27,  17),
            S(-31,  32),   S( 15,  12),   S( -4,  12),   S( 14,  26),   S(  3,  29),   S(-31,  29),   S(-16,   5),   S(-32,  22),
            S( -6,  42),   S( 55,  26),   S( 16,  44),   S( 19,  30),   S(  9,  29),   S( -1,  27),   S( 15,  11),   S( 13,  15),
            S(  4,  51),   S( 21,  24),   S( 28,  26),   S( 41,  26),   S( 39,  26),   S( 34,  18),   S( 30,  14),   S( 18,  21),
            S( 11,  21),   S( 22,   9),   S( 21,  28),   S( 46,  11),   S( 12,  17),   S( 40,  29),   S( 28,   5),   S( 19,  -6),
            S( 43,  10),   S( 28,  19),   S(-12, -18),   S( 14,  30),   S( 32,  -6),   S( 30,  -5),   S(-30,  12),   S( -3, -17),
            S(-85, -54),   S(-22,  -9),   S( -4,  18),   S(  4,  31),   S(-11,   5),   S(-23, -18),   S( -3,  -2),   S(-32, -27),

            /* knights: bucket 2 */
            S(-56,  13),   S( -8,  20),   S(-37,  50),   S(-37,  51),   S(-49,  60),   S(-42,  65),   S(-22,  30),   S(-22,  23),
            S(-14, -13),   S(-25,  14),   S(-15,  14),   S(-12,  29),   S( -9,  24),   S(-16,  49),   S(-39,  58),   S(-38,  74),
            S(-17,  25),   S( -5,   9),   S(-12,  27),   S( 12,  19),   S(  3,  30),   S(  1,  11),   S( -4,  40),   S(-27,  32),
            S(-12,  41),   S(-21,  37),   S(  1,  39),   S(  8,  46),   S( -3,  46),   S( -3,  35),   S(  2,  40),   S( -4,  44),
            S( 20,  23),   S(-17,  34),   S( -5,  44),   S(-17,  53),   S(  6,  45),   S( -7,  40),   S(  6,  32),   S( -1,  23),
            S(-20,  34),   S(  4,  31),   S(-23,  47),   S(-15,  42),   S(-27,  43),   S(  8,  20),   S(-28,  12),   S( 18,   4),
            S(-12,  26),   S(-28,  17),   S(-28,  17),   S(-35,  34),   S(-11,  14),   S(  6,  21),   S(-45,  40),   S(-29,  15),
            S(-133,  34),  S( -3,   3),   S(-77,  34),   S(-27,  14),   S(  0,  12),   S(-56,   5),   S(  0,   5),   S(-166, -42),

            /* knights: bucket 3 */
            S(-46,  -2),   S( 10, -27),   S(-31,  -4),   S( -3, -10),   S( -1,  -5),   S(-17,   5),   S( 22, -18),   S( -6, -16),
            S(-10,   3),   S(-28,  -6),   S(-16, -13),   S(  8,   7),   S( 18,  -5),   S( -5,  -9),   S(  0, -14),   S(-16,  64),
            S(  2, -31),   S(  6,  -5),   S(  2,  -3),   S( 15,   4),   S( 21,  17),   S( 23,   1),   S( 18,   2),   S( 12,  31),
            S(  2,   2),   S( 13,   9),   S( 20,  28),   S( 27,  29),   S( 31,  29),   S( 28,  31),   S( 31,  18),   S( 25,  23),
            S( 29,   6),   S(  9,  18),   S( 37,   8),   S( 33,  38),   S( 32,  34),   S( 35,  43),   S( 45,  37),   S( 22,  14),
            S(  6,  11),   S( 34, -12),   S( 51,  -6),   S( 62,   0),   S( 74, -22),   S( 81, -15),   S( 16,   8),   S( 16,  44),
            S( 33,  -3),   S( 17,  10),   S( 48, -23),   S( 52,  -8),   S( 69, -31),   S( 67, -37),   S( 68, -62),   S( 54, -15),
            S(-99,  26),   S(-24,  12),   S(-25,   6),   S(  9,  18),   S( 39,  -7),   S(  2, -10),   S( -3, -15),   S(-55, -33),

            /* knights: bucket 4 */
            S( 14,  21),   S(-52,   6),   S( 15,  28),   S( -6,  -7),   S(-25, -16),   S(-34, -26),   S(-13, -53),   S(-29, -41),
            S( 33,  27),   S(-26,  38),   S(  8, -26),   S(  1,  -7),   S( 12, -21),   S(-12, -46),   S(  8,  -3),   S(  1, -47),
            S( -8,  31),   S(  4,  38),   S(  4,   7),   S( 13,  10),   S( -8,  -2),   S(-44,  12),   S(-47, -33),   S(-31, -55),
            S(  0,  66),   S( 37, -20),   S( 50,  20),   S( 31,  19),   S( 22,  11),   S(100, -17),   S( 26, -30),   S(  1, -19),
            S( 64,  36),   S(-11,  45),   S( 51,  44),   S( 47,  20),   S( 43,  36),   S(-11,  25),   S( -2, -25),   S(-11,  -9),
            S( 10,  22),   S(-27,   3),   S( 84,  16),   S( 11,   6),   S( 11,  16),   S( 23,  17),   S( 10,  27),   S(-10, -20),
            S( -5,   9),   S(-15,   9),   S( 12,  -1),   S(  4,  37),   S(  8,   9),   S(  8, -16),   S(  4,  -9),   S(-15,  -3),
            S(-10,  -4),   S(  0,  -2),   S(  9,  10),   S(  1,   6),   S( -6,  -8),   S( 10,  22),   S( -1,   7),   S( -3, -18),

            /* knights: bucket 5 */
            S( 13,   8),   S(-44,  50),   S( 25,  38),   S( 11,  50),   S( 27,  21),   S(  6,  -1),   S( -2,  15),   S(-19, -12),
            S( 14,   7),   S( 29,  51),   S( 11,  23),   S(-20,  41),   S( 23,  34),   S( -4,  33),   S( 17,  28),   S(-13, -25),
            S( -1,  30),   S(-17,  39),   S( 58,  18),   S( 43,  39),   S(-19,  48),   S( -5,  25),   S(-20,  18),   S(  7,  -2),
            S( 36,  50),   S( 12,  47),   S( 38,  39),   S(  7,  55),   S( 24,  44),   S( 20,  41),   S( 26,  44),   S( 14,  37),
            S( 24,  55),   S( 36,  33),   S( 52,  49),   S( 72,  41),   S( 88,  44),   S( 33,  42),   S( 41,  37),   S( 40,  32),
            S(  5,  35),   S(  1,  51),   S( 26,  29),   S( 18,  54),   S( 43,  41),   S( 17,  53),   S( 23,  14),   S( -4,  33),
            S( 18,  59),   S( -7,  67),   S( 29,  46),   S( 15,  62),   S(  6,  53),   S(  7,  45),   S( 22,  69),   S(  3,   4),
            S(  1,  18),   S(  0,  19),   S(  9,  43),   S( -3,   8),   S( 10,  44),   S(  2,  37),   S(  9,  43),   S(-15, -10),

            /* knights: bucket 6 */
            S(  2, -34),   S(-29,  -2),   S( 22,  29),   S(-36,  42),   S(-41,  55),   S(  2,  44),   S(-13,  39),   S(-12,  39),
            S( -5, -26),   S( 44,   3),   S(  6,  12),   S(-43,  39),   S(-67,  68),   S( 19,  50),   S( 14,  52),   S(  0,  18),
            S(-32, -15),   S( -2,   2),   S(-10,  23),   S( 21,  30),   S(-17,  57),   S(-42,  56),   S(  4,  49),   S( -6,  45),
            S( 35,   8),   S( 36,  12),   S( 53,  27),   S( 80,  23),   S( 29,  45),   S( 21,  50),   S( 15,  58),   S(-18,  77),
            S(  1,  39),   S( 68,  -6),   S( 61,  33),   S( 82,  28),   S( 97,  34),   S( 88,  33),   S( 21,  59),   S( 20,  58),
            S( 26,  27),   S( 12,  14),   S( 68,  17),   S( 55,  39),   S( 61,  46),   S( 37,  33),   S( 23,  42),   S( 40,  45),
            S(-23,  26),   S( -1,  36),   S(-29,  37),   S( 27,  31),   S(  0,  58),   S( 19,  43),   S( 19,  73),   S( -7,  33),
            S(-38,  10),   S( 16,  46),   S( 29,  41),   S( 10,  41),   S( 23,  37),   S( 11,  62),   S( 22,  64),   S( 13,  32),

            /* knights: bucket 7 */
            S(-32, -48),   S(-200, -44),  S(-78, -48),   S(-65, -15),   S(-48,  -8),   S(-40, -12),   S(-15,   9),   S(-15,  11),
            S(-49, -74),   S(-46, -47),   S(-40, -33),   S(-61,   4),   S(-53,  12),   S( -6, -12),   S(-22,  50),   S(  1,  33),
            S(-85, -64),   S(-59, -36),   S(-54,  -3),   S( 20, -21),   S(-20,   5),   S(  4,   6),   S(-18,  57),   S( 41,  57),
            S(-61, -19),   S( 17, -24),   S( -2,   7),   S( 37,  -3),   S( 52,  -1),   S( 21,  12),   S( 17,  15),   S(-19,  35),
            S(-59, -20),   S(-22, -26),   S( 50, -23),   S( 88, -19),   S(108,  -6),   S( 71,  21),   S( 96,   1),   S( 81,  26),
            S( -6, -36),   S( 14, -38),   S(-17,  -5),   S( 33,  -4),   S( 70,   6),   S( 79,   5),   S( 59, -14),   S(  0,  16),
            S(-34, -31),   S(-67, -18),   S(  5, -15),   S( 32,  16),   S( 36,  21),   S( 40,   0),   S(-18,  25),   S(  5,  10),
            S(-34, -18),   S( -7,  -5),   S(-26, -10),   S(  8,  13),   S( 12,   7),   S( 23,  21),   S( -3,  -7),   S(  0,   0),

            /* knights: bucket 8 */
            S( -1,  -6),   S( -9, -11),   S( -4,  -3),   S(-10, -31),   S(-11, -40),   S(-10, -51),   S( -2,  -2),   S( -5, -21),
            S(  2,   1),   S( -6, -10),   S( -8, -31),   S(-18, -44),   S(-31, -31),   S(-18, -71),   S(-13, -58),   S(-17, -37),
            S(  5,  18),   S(-23, -19),   S( 19,   1),   S(  4,  -6),   S(  2, -35),   S(-17, -16),   S(-13, -38),   S( -7, -40),
            S(-17,   0),   S( -1,  -5),   S( -1,  10),   S(  4,  28),   S(  7,  -5),   S(  7,   5),   S(-13, -50),   S( -2, -16),
            S( 27,  56),   S( 11,   9),   S( 15,  33),   S( 35,  15),   S( 11,  29),   S( -4,  -4),   S(  5, -21),   S( -7,  -7),
            S( 13,  37),   S( 10,   6),   S( 27,  22),   S( 33,  14),   S(  2,  -1),   S( -2, -10),   S( -7, -30),   S( -6,  -9),
            S(  5,  18),   S(  1,   5),   S(  6,   9),   S( 11,   9),   S(  6,   7),   S(  5,  20),   S(  2,  12),   S( -1,   3),
            S(  2,   3),   S( 11,  33),   S(  5,  18),   S( -1,   1),   S(  3,  13),   S( -5, -18),   S(  3,   5),   S( -3,  -4),

            /* knights: bucket 9 */
            S( -9, -26),   S(-20, -34),   S(-18, -46),   S( -4, -15),   S(-24, -52),   S(-15, -41),   S( -4, -17),   S( -3, -23),
            S(-12, -36),   S(-11,   3),   S(-11, -51),   S(-14,  -7),   S( -6, -16),   S( -7, -34),   S( -6,  -3),   S(-15, -42),
            S(  5,   9),   S(-10, -14),   S(  1, -21),   S(  2,  -1),   S(  3,  13),   S(-33,  -6),   S(-13, -13),   S( -8, -16),
            S(-14,  -2),   S( -6,  -7),   S(  5,  24),   S( 19,  26),   S( 28,  19),   S(  9,  20),   S(-11, -35),   S( -3,  -1),
            S(  0,  24),   S( 17,   7),   S( 19,  34),   S(  2,  36),   S( 11,  13),   S( 12,  -6),   S(  2, -27),   S(  5,  11),
            S(  0,   4),   S(  7,  32),   S( 15,  27),   S( -7,  13),   S( 35,  31),   S( 16,   9),   S(  8,  14),   S( -6, -21),
            S(  1,   1),   S( -1,  23),   S( 18,  37),   S( 11,   3),   S( 14,  41),   S( -2, -19),   S(  4,  18),   S( -2,   1),
            S(  2,   2),   S(  3,   9),   S( 13,  29),   S( 16,  32),   S(  9,  11),   S(  0,   6),   S(  3,   5),   S(  0,  -3),

            /* knights: bucket 10 */
            S(-17, -45),   S(-17, -54),   S(-13, -25),   S(-18, -19),   S(-12, -10),   S(-15, -43),   S( -3,  15),   S(  5,  23),
            S( -6, -24),   S( -6, -12),   S( -1, -17),   S(-20, -34),   S(-25, -36),   S( -8, -40),   S( -8,  -4),   S( -5, -12),
            S(-16, -48),   S(-19, -63),   S(-10, -20),   S(-16, -20),   S( 14,  -3),   S(-15,  -7),   S( -6,   2),   S( -7,   7),
            S( -8, -16),   S( -5, -44),   S(  3, -37),   S( 18,   8),   S( 11,  34),   S( 19,  19),   S(  5,  14),   S( 10,  44),
            S( -8, -44),   S(-13, -28),   S( 15,   8),   S( 24,  26),   S( 20,  47),   S(  1,  24),   S( 20,  13),   S( 23,  52),
            S(-10, -40),   S( -5, -23),   S( -3, -13),   S( 13,  36),   S( 36,  55),   S( 31,  35),   S( 27,  59),   S( 18,  56),
            S(  0,   0),   S( -9, -28),   S(  1,  -8),   S( 28,  26),   S( 17,  26),   S(  9,  33),   S(  2,   1),   S( 10,  29),
            S( -3, -14),   S(  3,  11),   S( -7, -17),   S(  4,   1),   S( 12,  39),   S(  5,  28),   S(  3,  15),   S(  0,   0),

            /* knights: bucket 11 */
            S(  0,   1),   S(-19, -28),   S(-10, -46),   S(-10, -25),   S(-20, -48),   S(-12, -16),   S( -6,  -3),   S( -4,  -5),
            S( -7,  -6),   S(-13, -20),   S(-15, -77),   S(-31, -28),   S( -9,  -4),   S(-29, -36),   S(-16, -28),   S( -7,  -8),
            S(-15, -52),   S(-24, -59),   S(-27, -37),   S(  0,  -2),   S(-15,   0),   S(-19,  14),   S(  8,  -6),   S( -1,  16),
            S(-12, -27),   S( -8, -30),   S(-26,  -7),   S( 25,  28),   S( 15,  15),   S( 17,   7),   S( 14,  24),   S( 15,  32),
            S( -3, -23),   S(-18, -58),   S(  6, -21),   S(  1,   4),   S( 15,  18),   S( 34,  52),   S(  7,  -2),   S( 25,  66),
            S( -7,  -8),   S( -6, -28),   S(  0,  -7),   S( 40,  32),   S( 17,  20),   S( 48,  44),   S( 21,  22),   S( 14,  27),
            S(  9,  26),   S( -2,  -6),   S(  7, -13),   S( 12, -16),   S( 20,  29),   S( -1,   4),   S( 16,  41),   S( 20,  57),
            S( -3,   2),   S( -2, -17),   S(  9,  14),   S(  2,   8),   S(  2,  14),   S(  3,   7),   S(  4,   6),   S(  2,  13),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   2),   S( -2, -19),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   5),   S( -2, -13),
            S(  0,   0),   S(  1,   2),   S(  2,   6),   S( -4, -11),   S( -2,   6),   S( -4, -21),   S( -2, -12),   S(  1,  10),
            S( -5, -13),   S(  4,   4),   S( -5, -12),   S( -6, -24),   S(  0,   2),   S( -6, -18),   S(  1,  -5),   S( -7, -30),
            S( -7, -12),   S( -1,   1),   S( -9, -24),   S(  4,  15),   S( -6,  -6),   S(  0,   5),   S( -1,  -5),   S( -1,  -7),
            S(  9,  17),   S(  5,   3),   S( -6, -12),   S(  0,   3),   S( -5, -26),   S(  0,   4),   S( -1, -13),   S( -1,   2),
            S(  1,  -8),   S( -2, -20),   S(  1,  -1),   S( -1,  -5),   S(  5,  11),   S( -5, -18),   S( -1,  -7),   S(  0,   3),
            S(  2,   8),   S( -9,  -9),   S(  0,  10),   S(  2,  -8),   S( -5,  -8),   S( -5, -21),   S( -2,  -1),   S(  0,  -2),
            S(  2,   4),   S(  2,  13),   S( -2,  -4),   S(  2,  -1),   S( -2,  -4),   S( -2,  -9),   S( -3,  -8),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -1,  -5),   S( -2,  -3),   S( -8, -13),   S( -1,   1),   S( -3, -12),   S(  1,  -1),
            S( -2,  -6),   S(  1,   5),   S( -2, -23),   S(-10, -21),   S( -6, -30),   S( -4, -24),   S(  0,   0),   S(  1,  -1),
            S( -3,  -9),   S( -8, -30),   S(  7,  16),   S( -1,  -2),   S(-13, -41),   S(-10, -25),   S( -2, -11),   S( -6, -28),
            S( -9, -14),   S(  6,  13),   S(  1,   0),   S(-11, -29),   S( -2,  -9),   S(  6,  10),   S(  0, -13),   S( -4,  -9),
            S(  3,  11),   S( -1,  -2),   S(  2,  -9),   S( 10,  17),   S(  5, -13),   S( -3, -10),   S(  2, -12),   S(  1,   1),
            S( -3,  -8),   S( 14,  13),   S(  6,  21),   S(-13,  10),   S(  5,   4),   S( -9, -33),   S(  4,   6),   S( -3,   3),
            S(  1,   7),   S(  1,   4),   S( 10,  11),   S(  7,   8),   S( 14,  20),   S( -5, -21),   S( -2,  -1),   S( -5,  -3),
            S( -1,   2),   S( -1,  -5),   S(  0,   1),   S(  1,  -8),   S( -1,   0),   S(  3,  -2),   S(  0,  -1),   S( -1,   1),

            /* knights: bucket 14 */
            S( -3, -23),   S( -5, -24),   S( -1,  -2),   S( -3,   4),   S( -8, -23),   S( -2, -15),   S( -1,  -5),   S(  0,   2),
            S(  0,  -2),   S( -4,  -8),   S(-15, -59),   S( -8, -37),   S( -1,  -8),   S(  1,   5),   S(  1,  -2),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-11, -55),   S(  1,   1),   S( -4, -21),   S( -4, -10),   S(  0,  -1),   S(  2,  11),
            S(  0,   5),   S( -6, -33),   S(-15, -41),   S(-11, -39),   S( -3, -21),   S(  2,  -1),   S( -2, -15),   S( -7, -11),
            S( -2,  -4),   S( -2, -15),   S(  0,  21),   S( -8, -34),   S( -9,  -8),   S(  3,  22),   S(  3,   7),   S( -4,  -5),
            S( -4,  -8),   S(  3,  -2),   S( -9, -31),   S(  4,   0),   S( 15,  20),   S(  4,   9),   S( -3,  -1),   S(  1,  -2),
            S(  0,  -3),   S( -2, -10),   S(  7,  -5),   S(  0,  -9),   S( -7, -10),   S( -3,  -9),   S( -6,  -5),   S(  1,   8),
            S(  0,  -2),   S(  2,   4),   S( -1,  -9),   S(  7,  -1),   S(  5,  18),   S(  1,   3),   S( -2,  -6),   S( -1,  -4),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -1, -12),   S( -1, -12),   S( -7, -14),   S( -2,  -1),   S( -1,  -4),   S(  1,   0),   S(  0,  15),
            S( -2,  -5),   S(  0,  -2),   S( -5, -19),   S( -6, -25),   S( -2,  -5),   S( -1,  -8),   S(  0,   0),   S( -1,  -3),
            S( -6, -15),   S( -7, -15),   S( -3, -11),   S(-15, -41),   S( -5, -25),   S( -2,  -4),   S( -1,   0),   S( -2,   2),
            S( -6, -16),   S( -6, -31),   S( -6, -19),   S(  0, -10),   S(  0, -17),   S(  7,  23),   S(  5,  10),   S( -3,   0),
            S(  0,  -1),   S( -2,  -6),   S( -1, -16),   S( -7, -11),   S(  4,  18),   S(  4,  11),   S( -6,  -7),   S( -1,   3),
            S( -2,  -3),   S( -2,  -4),   S( -2, -21),   S( -3,   6),   S( -5, -13),   S( -7,  11),   S( -2,   6),   S(  2,   9),
            S( -3, -12),   S( -2,  -9),   S( -1,  -9),   S( -4,  -7),   S(-11, -13),   S( -4,  16),   S( -2,  -8),   S(  3,  13),
            S(  0,  -3),   S(  0,  -1),   S( -3,  -9),   S( -2,  -8),   S( -2,  -4),   S(-10,  -4),   S(  6,  17),   S( -2,   3),

            /* bishops: bucket 0 */
            S( 31,  23),   S( 18, -15),   S( 40,  13),   S(  5,  17),   S( -5,  -6),   S( 18, -10),   S( 23, -40),   S( 11, -22),
            S( 55, -42),   S( 85,  18),   S( 38,   2),   S( 16,  -3),   S(-12,  30),   S( -2, -26),   S(-33,  12),   S( 14, -48),
            S( 27,  43),   S( 45,   3),   S( 33,  29),   S( 12,  46),   S( 17,   7),   S(-25,  41),   S(  8, -30),   S( 18, -45),
            S( 21,   7),   S( 67, -14),   S( 38,   8),   S( 34,  31),   S(  3,  28),   S( 28,  -5),   S( -3, -14),   S(  1,   0),
            S( 18,   1),   S( 31,  21),   S(  4,  36),   S( 58,  11),   S( 63,  -6),   S( 17,  -8),   S( 22, -20),   S(-31,  -2),
            S(-39,  60),   S( -4,  17),   S( 62,  -2),   S( 87, -28),   S( 39,  27),   S(  5,  16),   S( -1,  10),   S(  0,  11),
            S(-13,  14),   S( 18,  16),   S( 41, -11),   S( -2,  31),   S(-34,  -7),   S( 25,  24),   S( 12,   5),   S(-11, -11),
            S(-23, -27),   S(  9,   0),   S( -1,   6),   S(  6, -12),   S( 21,  24),   S( 33,   8),   S( -1,  44),   S(-11,  16),

            /* bishops: bucket 1 */
            S( 50,  26),   S( -6,  32),   S(  5,  39),   S(  9,  27),   S( -8,  27),   S( -2,  26),   S(-11,   1),   S(-35,  14),
            S( 14, -11),   S( 42,  -3),   S( 52,   3),   S( 29,  24),   S( -9,  11),   S(  6,  -7),   S(-28,  12),   S( 18, -14),
            S( 43,  -5),   S( 11,   3),   S( 42,  10),   S( 18,  20),   S( 20,  22),   S(-12,  20),   S( 24, -11),   S(  8, -32),
            S( 40,   4),   S( 19,  15),   S( 10,  10),   S( 33,  19),   S(  3,  20),   S( 22,   1),   S( -6,   3),   S( 14, -13),
            S( 38,  30),   S(  9,  19),   S( 19,  20),   S( -3,  30),   S( 26,   8),   S(  0,  12),   S( 30, -19),   S( -9,  11),
            S( -3,  21),   S( 27,  34),   S( 37,  22),   S( 53, -12),   S( 14,  14),   S( 41,   1),   S( -2,  28),   S( 51, -16),
            S( -9,  46),   S(-18,  37),   S( 18,  23),   S( 34,  19),   S( 38,  19),   S(-21,  18),   S( 44,  -2),   S(-18,  39),
            S( 26,  23),   S(  8,   2),   S(  2,  10),   S(-23,  19),   S( 21,  13),   S( -8,   2),   S( 12,   4),   S(  8,  31),

            /* bishops: bucket 2 */
            S( 27,   5),   S(  6,  14),   S( -7,  13),   S(-29,  48),   S(-13,  33),   S(-32,  27),   S(-19,  -5),   S(-39,  39),
            S(-21,  25),   S( 10,  -2),   S( 21,   5),   S( -2,  20),   S( -3,  29),   S( 16,   2),   S( -4,  -6),   S(  9, -27),
            S( -7,   3),   S( -6,   7),   S( 14,  23),   S( -5,  42),   S(  4,  31),   S(  6,  35),   S( 13,   9),   S(-17,   1),
            S(  1,   8),   S(-10,   9),   S(-15,  32),   S(  4,  32),   S( -3,  36),   S(  6,  19),   S(  5,  14),   S(  2,   7),
            S( 12,   1),   S(-17,  31),   S(-11,  23),   S(-34,  42),   S(-12,  34),   S(-11,  41),   S(  2,  18),   S(-29,  33),
            S(  6,  27),   S( -6,  13),   S(-17,  46),   S(-20,  21),   S(  8,   7),   S( -1,  26),   S( -3,  51),   S(  1,  22),
            S(  4,  19),   S(-13,  27),   S(-29,  49),   S( 17,  -3),   S( -3,  -3),   S(-20,   4),   S(-57,  30),   S(-32,  34),
            S(-44,  50),   S(-39,  42),   S(-25,  26),   S(-41,  22),   S(-50,  35),   S(-36,  15),   S(  5,  10),   S(-57,  31),

            /* bishops: bucket 3 */
            S( 11,  24),   S( 33,  -9),   S( 26,  14),   S( 17,  14),   S( 18,   3),   S( 42, -12),   S( 42, -29),   S( 54, -56),
            S( 13,   5),   S( 11,  17),   S( 30, -10),   S( 11,  27),   S( 22,   3),   S( 22,  16),   S( 51,   8),   S( 45,  -5),
            S( 22,   6),   S( 13,  10),   S( 20,  33),   S( 25,  14),   S( 23,  44),   S( 30,  30),   S( 38,  19),   S( 49, -11),
            S( 32, -10),   S( 25,   5),   S( 18,  26),   S( 24,  41),   S( 28,  32),   S( 31,  27),   S( 28,  16),   S( 25,  -5),
            S( 19,   2),   S( 25,   9),   S( 42,   9),   S( 26,  40),   S( 23,  41),   S( 34,  22),   S( 18,  30),   S( 26,  29),
            S( 29,   1),   S( 34,  18),   S( 33,  30),   S( 40,   9),   S( 23,  14),   S( 57,  23),   S( 48,   9),   S(  6,  65),
            S( 19,   6),   S(  3,  35),   S( 41,  19),   S( 22,  12),   S( 14,  11),   S( 22,  -6),   S( 10,  44),   S( 18,  35),
            S(-25,  83),   S( -1,  32),   S( 56,   8),   S( 27,  14),   S(-11,  31),   S(  2,  30),   S( 30,  -5),   S( 80,  -6),

            /* bishops: bucket 4 */
            S(-15, -11),   S(-23,   8),   S(-37,  -4),   S(-23,  19),   S(-25,  24),   S(-52,  23),   S(  0, -13),   S( -3,   6),
            S( -3,  14),   S( 13,  20),   S(-12,  34),   S(-31,  14),   S(-21,  -8),   S( 38, -11),   S(-25,   8),   S( 13,  -5),
            S( -8,   3),   S(-34,  31),   S( 16,   0),   S(-26,   9),   S(  1,  21),   S( 30,  -4),   S(-31, -15),   S(-53,  -4),
            S(-33,  26),   S( -1,  29),   S( 48,  23),   S( 27,  31),   S( 11,  17),   S( 50, -15),   S( 46, -13),   S(-11, -38),
            S( 11,  18),   S(  1,  42),   S(-16,  48),   S( 19,  38),   S( 34,   5),   S( 31, -23),   S(-12, -26),   S( 12, -12),
            S( -7,  32),   S( 19,  12),   S( -8,  46),   S( 20,   9),   S( 39,   5),   S( 16,   4),   S( 17, -38),   S(  3,  -8),
            S(-17,   7),   S( 36,  30),   S( 11,  14),   S( 22,  14),   S( 10,  -5),   S(  1,  15),   S(  7,  16),   S(  6, -26),
            S( 16,  -7),   S(-12, -38),   S(  1,  -4),   S( -6,  -3),   S(  6, -12),   S(  1,   8),   S(  0,  -6),   S(  0,  11),

            /* bishops: bucket 5 */
            S( -6,  15),   S(-13,  41),   S(-52,  33),   S(-30,  30),   S(-43,  32),   S(-10,  17),   S( -6,  14),   S(-17,  33),
            S(-25,  37),   S(-18,  28),   S(-32,  56),   S(  0,  25),   S(-30,  34),   S(-28,  22),   S(-33,   4),   S(-10,  -2),
            S(  0,  13),   S(  0,  35),   S( 14,  33),   S(-19,  45),   S( -3,  32),   S(-29,  18),   S(-31,  28),   S(-21,   3),
            S( 31,  11),   S( 25,  23),   S( -9,  51),   S( 28,  25),   S( 30,  30),   S( 16,  25),   S( 16, -10),   S(  9,  22),
            S( 34,  44),   S( 36,  12),   S( 52,  25),   S( 80,  27),   S( 49,  16),   S( 43,  15),   S( 35,   9),   S( -9,   3),
            S( 23,  43),   S( 30,  41),   S( 37,  38),   S( 28,  28),   S( -2,  30),   S( 23,  -1),   S(-24,  44),   S( -2,  31),
            S(  3,  40),   S(-27,  32),   S(  8,  36),   S(  4,  45),   S( 25,  23),   S( 30,  35),   S(  5,  39),   S(  1,  33),
            S(  7,  12),   S( 13,  31),   S( 14,  11),   S(  3,  35),   S(  1,  53),   S( 12,  21),   S( 29,  53),   S(  1,  21),

            /* bishops: bucket 6 */
            S( -2,  35),   S(  0,  27),   S(-40,  35),   S(-42,  35),   S(-42,  21),   S(-47,  32),   S(-21,  54),   S( -6,  34),
            S( 22,   6),   S( -2,   7),   S(-23,  26),   S( -6,  29),   S(-30,  41),   S(-18,  22),   S(-105,  51),  S( 17,  24),
            S( 23,  -4),   S(  4,   4),   S( 31,  17),   S( 19,  23),   S( 40,  17),   S( 11,  28),   S(  3,  28),   S(-38,  19),
            S(-14,  39),   S( 14,  10),   S( 34,  15),   S( 30,  29),   S( 41,  26),   S( 37,  22),   S( 34,  28),   S(-14,  -3),
            S(-14,  20),   S( 51,   2),   S( 25,  22),   S( 52,  18),   S( 95,  22),   S( 59,  21),   S( 38,  27),   S(-24,  48),
            S(  2,   9),   S(-45,  45),   S( 17,  35),   S( 14,  36),   S( 34,  23),   S( 29,  44),   S(  3,  45),   S( -9,  49),
            S(-23,  34),   S(-22,  45),   S( -1,  36),   S(-13,  29),   S( 42,  16),   S( 19,  24),   S( -3,  50),   S( -2,  34),
            S( 18,  72),   S( 13,  34),   S(  7,  39),   S(  0,  44),   S(-18,  35),   S( 30,  14),   S(  9,  19),   S( 21,  29),

            /* bishops: bucket 7 */
            S( -6, -17),   S( -3,   0),   S(-39, -30),   S(-55,   9),   S(-33, -10),   S(-76,  17),   S(-71, -33),   S(-57,  20),
            S(-34, -28),   S(-58, -21),   S(-22, -10),   S( -2, -19),   S(-34,  -1),   S(-46,  14),   S(-46,   3),   S(-32,  10),
            S(-36, -22),   S(  2, -22),   S( 27, -20),   S( 18,  -7),   S(-34,  13),   S(-12,   1),   S(-34,  40),   S(-23,  27),
            S(-42,  14),   S( 52, -41),   S( 73, -27),   S( 56,  -2),   S( 79,  -4),   S(  3,  17),   S( 25,  25),   S( -3,  26),
            S( 20, -50),   S(-14, -23),   S( 63, -41),   S(101, -32),   S( 69,  19),   S( 72,  10),   S( -5,  38),   S( 25,   8),
            S(-29, -13),   S(-31,   0),   S( 39, -28),   S( 21,  -8),   S( 49, -16),   S( 56,  18),   S( 53,  11),   S( 21,   0),
            S( -4, -15),   S(-31,  10),   S(  6,  -5),   S( 11,  -9),   S( 12, -26),   S( 33, -13),   S( 18,  15),   S( 12,  11),
            S( -4,  15),   S( -9,  17),   S(-30,  10),   S(  3,  -6),   S(  8,  -6),   S( 17,  -8),   S( 23,   6),   S( 16,  27),

            /* bishops: bucket 8 */
            S( -7,  -1),   S(-12, -32),   S(-45,  -7),   S( -5, -27),   S( -6,  19),   S(-25,  -4),   S(  6,  21),   S( -1,   3),
            S( -7,  -2),   S(-28, -36),   S(-15, -27),   S(-17,  -7),   S(  9, -13),   S(-18, -31),   S(-15, -41),   S( -6, -10),
            S(  2,   0),   S(-13,   6),   S(-16,  24),   S(-11,  12),   S( -9,   6),   S(  1, -22),   S(  3, -47),   S(-32, -39),
            S(  7,  33),   S( -6,  40),   S(  6,  35),   S( -5,  10),   S( 16,  16),   S( -5,   3),   S(  2, -20),   S( -6, -18),
            S( 15,  37),   S( 11,  61),   S(-11,  27),   S( 45,  40),   S(  1,  19),   S( 15,   6),   S(  5, -33),   S(-11, -17),
            S( -1,   8),   S( 10,  33),   S( 17,  35),   S(-17,  15),   S( 26,   6),   S( -3,  -7),   S(-16, -17),   S(-18, -22),
            S( -3,   6),   S( 15,  39),   S(  8,  20),   S(  0,   0),   S(  3,   8),   S( -3,  17),   S( -9,  -8),   S(-10, -28),
            S( -5,  -5),   S(  0, -28),   S( -1,  -8),   S(  0, -15),   S(-19, -10),   S( -6,  -7),   S( -1,  12),   S( -6,  10),

            /* bishops: bucket 9 */
            S(-19, -19),   S( -8,   1),   S(-22,   1),   S(-12, -27),   S(-37, -32),   S(-20, -39),   S(-17,  -9),   S( 13,  13),
            S(-15, -17),   S(-31, -16),   S(-11, -11),   S(-18,  11),   S(-49,  23),   S(-20, -19),   S( -9,   1),   S( -6,  -6),
            S(  9,   1),   S( 16,  10),   S(-17,   1),   S(-16,  16),   S( -1,   8),   S(  0,  -2),   S(-17, -32),   S( -7,  21),
            S(-15,   6),   S( 15,  15),   S( -9,  20),   S(  5,  16),   S( 17,  20),   S(  9,  -2),   S(  1,  -7),   S(-17, -27),
            S( -3,  16),   S( 21,  21),   S(  5,  34),   S(  8,  46),   S(-17,  11),   S(  1,  25),   S( -8,  31),   S( -7,  -9),
            S(-14,   2),   S( 19,  48),   S( 12,  35),   S( 22,  13),   S( 11,  29),   S(  2,  11),   S(-20,   0),   S(-13, -14),
            S(  5,  16),   S( 25,  31),   S(  2,   4),   S(  1,  40),   S( 20,  35),   S(  6,   2),   S( -2,  -5),   S( -5,  -3),
            S(  0, -16),   S( -8,  20),   S( -7,  14),   S(-19, -16),   S(-14,  -5),   S(  7,  28),   S(  1,   5),   S( -9, -10),

            /* bishops: bucket 10 */
            S(-13,   9),   S(  4, -25),   S(-36, -30),   S(-21, -24),   S(-24, -13),   S(-25, -23),   S(-13, -24),   S(-13, -16),
            S(  5, -19),   S(-20, -19),   S( -8, -11),   S(-43,  -1),   S(-41,   1),   S(-22,  17),   S(-23, -41),   S(-11, -16),
            S(  7, -15),   S( -1, -15),   S(-28, -29),   S(  1,   2),   S(-40,  25),   S(-30,  30),   S(-23,  23),   S(  5,  18),
            S(-12, -24),   S(  2,   3),   S( 10, -11),   S( 12,   3),   S(  9,  22),   S(-12,  51),   S(  3,  27),   S( 15,  25),
            S(-19,  -3),   S(  0,  -3),   S( -6,  10),   S( 32,  21),   S( -1,  57),   S( 21,  44),   S(  7,  37),   S(  0, -18),
            S(  2, -29),   S(-27,  -5),   S(-16,   6),   S(-13,  22),   S( 24,  30),   S( 47,  43),   S(  9,  49),   S(  1,   7),
            S(-22,  -8),   S( -4, -32),   S(-10, -11),   S( 21,  11),   S( -6,  -8),   S( 16,  33),   S( 23,  51),   S( 13,  16),
            S( -2, -19),   S( -8,   7),   S(  7,  19),   S(-11,   0),   S(-10,  11),   S(-12, -10),   S(  9,   1),   S(  9,  33),

            /* bishops: bucket 11 */
            S(-14,  17),   S(-32, -14),   S(-52, -48),   S(-23, -31),   S(-22, -13),   S(-66, -49),   S(-10, -13),   S(-21, -18),
            S(-13, -19),   S(  5, -24),   S( -9, -11),   S(-25, -38),   S(-46, -12),   S(-31, -31),   S(-18, -30),   S(-23, -36),
            S(-11, -50),   S( -1, -50),   S(-20,  -8),   S( -2, -13),   S( -6, -12),   S(-28,  27),   S(-11,  20),   S( -2,  17),
            S(-17, -37),   S(-14, -38),   S(  3, -14),   S(  1, -12),   S( 11,  19),   S( -4,  55),   S(  6,  44),   S( 18,  27),
            S(-10, -24),   S(-20, -48),   S(-18,  16),   S( 46,  -2),   S( 34,  32),   S(  0,  53),   S( 16,  51),   S( 13,  24),
            S(-19, -51),   S(-33,  -7),   S( -4, -23),   S(  7,   8),   S(  1,  28),   S( 24,  45),   S( 28,  34),   S( -1,   3),
            S( -8,  -9),   S(-13, -32),   S(-22,  -5),   S( -8, -20),   S(  8,  -3),   S( 34,  13),   S( -3,  17),   S( 15,  30),
            S(-15,  -7),   S(-21,  -3),   S( -7,  10),   S(  9,   4),   S( 11,   2),   S(-18, -26),   S(  3,   7),   S(  2, -10),

            /* bishops: bucket 12 */
            S(  1,   4),   S( -7, -14),   S(-12, -29),   S( -7, -27),   S( -9, -19),   S(-12, -22),   S( -1,  11),   S( -4,   6),
            S( -7,  -7),   S(-12, -30),   S( -8, -14),   S( -7, -13),   S(-15, -25),   S( -3,  13),   S( -1,   5),   S( -1, -10),
            S( -1,  -3),   S(-16,  -4),   S( -9, -13),   S(-10,  -7),   S( -6,   5),   S( -2,  -6),   S(-10, -44),   S( -4,  -3),
            S( -2,   3),   S(  4,   0),   S(-18, -31),   S( -4,  10),   S(  0,   5),   S(  5,  23),   S( -5,  -9),   S( -7,  -4),
            S( -1,  -4),   S(  2,  15),   S( -5,  18),   S( -9,  -1),   S( -3,  -6),   S( -5,   2),   S(  4,   4),   S( -7,  -3),
            S(-12, -13),   S(  4,  58),   S(-21,  12),   S(-10,  -6),   S(  6, -17),   S( -1,   3),   S( -1,   4),   S( -1,  -5),
            S( -2,  -5),   S( -1,  18),   S(  2,  11),   S( -8,   6),   S( -1,   8),   S(  7,  15),   S( -7, -18),   S( -1,   4),
            S( -2,  -2),   S(  0,  -6),   S( -6,  -2),   S(  6,   7),   S(  1,   8),   S(  0,   3),   S(-10,  -1),   S(  1,  -2),

            /* bishops: bucket 13 */
            S( -6, -39),   S(-13, -29),   S(-13, -17),   S(-16, -19),   S(-16, -19),   S( -9,   1),   S( -2,  -5),   S( -6,  -4),
            S( -4,  -8),   S( -9,  -8),   S(-14, -30),   S(-19, -10),   S(-14,   6),   S( -9,  -1),   S(  1,  -5),   S(  2,  -2),
            S( -9, -11),   S( -6,  -8),   S( -3,  18),   S(-22,  -5),   S(-14, -26),   S(  2,  -4),   S( -4, -30),   S(  5,  21),
            S( -2,   3),   S(-13,  -6),   S(-14,   1),   S(-25,   9),   S(  0,  18),   S(  2,  -9),   S( -2,   3),   S( -7,  -7),
            S( -3,   9),   S(-16,   4),   S(-18,  -6),   S( 18,  -3),   S( -8,   1),   S( -6,   3),   S(-11, -17),   S( -2,  -7),
            S( -3,  -5),   S( -9,   1),   S(-12,  -1),   S(  9,  14),   S(  1,   8),   S(  2,   0),   S(  6,  18),   S( -3,  -6),
            S( -6,  -9),   S( -6,   2),   S(  5,  24),   S( -8,   7),   S( -9,  -2),   S(  1,  -1),   S(-13, -21),   S(  0,   7),
            S( -7, -16),   S(  0,   9),   S( -1,  -3),   S(  5,   0),   S( -1,   5),   S( -8,  -7),   S(  1,   9),   S( -1, -13),

            /* bishops: bucket 14 */
            S( -5, -11),   S(-13, -17),   S(-18, -28),   S(-18, -46),   S(-15, -36),   S( -6, -28),   S(-10, -14),   S( -9, -13),
            S(-10, -25),   S(  1, -18),   S( -8, -15),   S(-27, -42),   S(-11, -14),   S(-19, -13),   S(-13, -19),   S(  1, -13),
            S( -9, -12),   S( -9, -32),   S(-17, -22),   S(-15, -20),   S(-28,  -6),   S(-19, -23),   S( -8,   1),   S( -3,  -2),
            S( -8, -23),   S( -9,  -6),   S(-12,  -7),   S(-23,  17),   S(  0,   5),   S(-23,  10),   S(-20, -17),   S( -5, -11),
            S( -8,  -4),   S( -8,  24),   S( -7, -21),   S( -7, -22),   S(-14,   7),   S( -8,  -9),   S(  6,  21),   S(  2,  -6),
            S( -1,   3),   S( -9,   5),   S(-17,  -4),   S( -9, -20),   S(  5,   6),   S( -3,  28),   S( -3,  33),   S( -8, -21),
            S( -6, -22),   S(  2,   4),   S( -8,   0),   S(  2,  13),   S(-11,  -9),   S( -2,  -2),   S( -1,  -7),   S( -4,  -7),
            S( -6,  -6),   S( -4,  -7),   S( -4,  -6),   S( -2,   4),   S(-10, -19),   S(  1,   9),   S(  7, -10),   S(  1,   5),

            /* bishops: bucket 15 */
            S(  7,  13),   S(  5,   5),   S(-19, -28),   S( -1, -10),   S(-11, -17),   S(-12, -24),   S( -6, -13),   S( -2,  -9),
            S(  2,   4),   S(  1,   0),   S(  3,  -2),   S(-10, -13),   S(-15, -22),   S( -7,  -8),   S( -7, -14),   S( -1,   0),
            S( -7, -14),   S( -1,  -3),   S( -9,  -3),   S(-11,  -9),   S(-20, -21),   S(-14, -15),   S( -7, -11),   S(  2,  15),
            S( -4,  -7),   S(-17, -18),   S(  6, -15),   S(-24, -32),   S( -5,   5),   S(-10, -16),   S(  3,  14),   S( -1,  -9),
            S( -1, -10),   S(-12, -18),   S(-14, -11),   S(-20, -46),   S( -2, -24),   S(-14,  19),   S(  3,  17),   S(-10, -17),
            S( -9, -32),   S(-12, -13),   S(-16, -31),   S(-21, -13),   S( -5,  -4),   S( -3, -16),   S(  7,  38),   S(  1,  13),
            S( -3,   1),   S(  0, -14),   S( -3, -13),   S( -4,   2),   S(-12, -14),   S( -2,   6),   S( -8,   8),   S(  3,   5),
            S( -3,   1),   S( -1,   1),   S( -5,  -1),   S( -7,  -4),   S( -8,  -5),   S(-17, -21),   S( -9, -25),   S(  1,   1),

            /* rooks: bucket 0 */
            S(-23,   7),   S(-10,   1),   S(-14, -11),   S( -9,  -5),   S(-14,  10),   S( -9,  -8),   S(-15,  23),   S( -4,  19),
            S( 10, -62),   S( 25, -13),   S(  0,  -2),   S( -5,   3),   S( 10,  -2),   S( -2,  -9),   S(-34,  20),   S(-45,  30),
            S(  3, -25),   S( 11,  25),   S( 20,   7),   S(  9,  10),   S(-18,  39),   S( -1,   6),   S(-30,  18),   S(-41,  15),
            S( 26, -21),   S( 61,   0),   S( 42,  26),   S( 42,   5),   S( 14,   9),   S( -2,  14),   S(-15,  23),   S(-37,  34),
            S( 62, -23),   S( 88, -15),   S( 67,  -1),   S( 40,  -9),   S( 46,   7),   S( 25,   9),   S( -5,  37),   S(-16,  33),
            S( 70, -43),   S(106, -34),   S( 53,   7),   S( 16,  20),   S( 42,  10),   S(-39,  33),   S( 35,  18),   S(-37,  41),
            S( 44,  -8),   S( 70,  -2),   S( 23,   9),   S( 10,  28),   S( -7,  28),   S( -3,  15),   S(-13,  35),   S(-11,  27),
            S( 32,  21),   S( 17,  48),   S( 18,  30),   S( -4,  38),   S(  6,  19),   S(  9,   2),   S( -3,  30),   S(  3,  28),

            /* rooks: bucket 1 */
            S(-77,  35),   S(-55,   7),   S(-58,  -3),   S(-43, -13),   S(-30, -22),   S(-30, -21),   S(-35,  -9),   S(-39,  21),
            S(-43,  12),   S(-58,  19),   S(-17, -13),   S(-27, -32),   S(-30, -14),   S(-40, -14),   S(-42, -19),   S(-60,  14),
            S(  2,   7),   S(-27,  31),   S(-19,  11),   S(-39,  19),   S(-44,  25),   S( -5,  -4),   S(-24,   5),   S(-46,  19),
            S(-51,  52),   S(-34,  31),   S(  6,  16),   S(-11,  19),   S(-23,  29),   S(-41,  39),   S(-34,  37),   S(-31,  14),
            S( 48,  16),   S( 32,  33),   S( 26,   3),   S(-35,  40),   S(-19,  38),   S( 17,  20),   S( -1,  18),   S(-41,  24),
            S( 42,  11),   S(  8,  28),   S(  8,  23),   S(-32,  27),   S( 12,  11),   S(-28,  42),   S( -9,  23),   S(-44,  32),
            S(-16,  31),   S(  8,  27),   S( 22,  26),   S(-45,  48),   S(-25,  33),   S(  4,  33),   S(-37,  28),   S(-53,  34),
            S( 25,  32),   S( 31,  36),   S(  0,  28),   S(-45,  54),   S(  0,  15),   S( 24,  13),   S(-17,  35),   S( -7,  15),

            /* rooks: bucket 2 */
            S(-69,  38),   S(-48,  18),   S(-48,  15),   S(-58,  14),   S(-62,  12),   S(-50,   7),   S(-34, -22),   S(-50,  29),
            S(-81,  46),   S(-63,  35),   S(-46,  25),   S(-55,  10),   S(-42,  -3),   S(-56,   0),   S(-68,  19),   S(-63,  16),
            S(-75,  62),   S(-60,  49),   S(-56,  50),   S(-34,   8),   S(-45,  23),   S(-27,  20),   S(-17,  12),   S(-36,  22),
            S(-72,  63),   S(-56,  65),   S(-39,  60),   S(-32,  46),   S(-25,  31),   S(  4,  31),   S(-35,  51),   S(-20,  32),
            S(-25,  50),   S(-48,  66),   S(-43,  56),   S(-18,  39),   S( 27,  24),   S( 20,  28),   S(-25,  49),   S(-39,  46),
            S(-38,  45),   S(-33,  47),   S(-15,  28),   S( -5,  21),   S( 21,  25),   S( 47,  15),   S( 23,  19),   S(-17,  29),
            S(-52,  40),   S(-67,  68),   S(-34,  52),   S(-12,  47),   S( 12,  26),   S( 25,  19),   S(-52,  60),   S(-34,  47),
            S(-35,  67),   S(-13,  47),   S(-70,  64),   S(-28,  42),   S(-48,  56),   S(-27,  60),   S(-50,  74),   S(-22,  46),

            /* rooks: bucket 3 */
            S( -7,  74),   S( -7,  65),   S( -3,  58),   S(  4,  45),   S(  0,  46),   S(-18,  68),   S( -9,  76),   S( -5,  38),
            S(-33,  84),   S(-12,  64),   S(  2,  56),   S(  6,  50),   S( 15,  44),   S(  8,  55),   S( 40,   2),   S( 20, -36),
            S(-35,  80),   S(-15,  80),   S( -2,  72),   S( 14,  53),   S(  8,  73),   S( 24,  65),   S( 29,  66),   S(  4,  50),
            S(-25,  90),   S(-17,  83),   S( 20,  71),   S( 28,  64),   S( 24,  68),   S(  0, 106),   S( 60,  60),   S( 20,  69),
            S(-14,  98),   S( 23,  78),   S( 17,  69),   S( 40,  66),   S( 43,  66),   S( 48,  66),   S( 89,  52),   S( 56,  45),
            S(-13,  90),   S( 12,  74),   S( 11,  69),   S( 17,  68),   S( 28,  51),   S( 50,  47),   S( 87,  32),   S( 94,  14),
            S(-35, 100),   S(-17,  99),   S( -8,  92),   S( 25,  76),   S( 18,  72),   S( 31,  69),   S( 61,  63),   S(109,  27),
            S(-77, 151),   S( -8, 102),   S( 10,  76),   S( 39,  64),   S( 51,  54),   S( 55,  66),   S(118,  48),   S(103,  47),

            /* rooks: bucket 4 */
            S(-87,  26),   S(-13,   0),   S(-46,   6),   S(-28,  18),   S(-32, -19),   S(  8, -51),   S( -6, -22),   S(-13, -35),
            S(-35,   2),   S(-43,   6),   S(-46,  16),   S(-41,  24),   S(-13, -10),   S(-15, -25),   S(  3, -36),   S(-21, -22),
            S(  0,  12),   S(-25, -16),   S(-14,  11),   S(-14,  -9),   S( -3,  -7),   S( -6,  -7),   S( 33, -17),   S(-46,   0),
            S(-32, -11),   S(  2,   5),   S(-24,  17),   S( 28,   1),   S( 20,   3),   S( 16,  -2),   S( 15,  10),   S( -7,  12),
            S(-18, -10),   S( -3,  31),   S(-11,  22),   S( 75,   7),   S( 23,  22),   S(  4,  16),   S( 40,  29),   S( 31,   1),
            S( 25,  11),   S( 25,  13),   S( 54,  15),   S( 41,  13),   S( 33,  16),   S(  5,  35),   S(  7,  27),   S( 25,  31),
            S(  3,  -3),   S( 38,  30),   S( 30,  29),   S( 38,  22),   S( 54,  10),   S( 10,   2),   S( 32,  18),   S( 28,  22),
            S( 37, -54),   S( 40,  45),   S( 15,  28),   S( 12,  18),   S( 17,   4),   S(  8,  24),   S( 14,   5),   S( 16,  17),

            /* rooks: bucket 5 */
            S(-49,  34),   S(-59,  53),   S(-67,  52),   S(-60,  38),   S(-47,  25),   S(-45,  40),   S(-11,  25),   S(-43,  44),
            S(-41,  35),   S(-37,  30),   S(-86,  67),   S(-56,  37),   S(-42,  23),   S(-22,  15),   S(  5,  14),   S(-36,  22),
            S( -9,  48),   S(-47,  59),   S(-56,  61),   S(-62,  59),   S(-33,  28),   S(-12,  30),   S(-10,  41),   S(-13,  41),
            S(-34,  74),   S( -5,  46),   S(-25,  65),   S(-13,  42),   S(-13,  53),   S(  3,  60),   S( -4,  53),   S(  3,  35),
            S( 12,  61),   S(  5,  63),   S( 42,  44),   S( 33,  57),   S( 39,  52),   S( 17,  72),   S( 65,  59),   S( 28,  40),
            S( 60,  55),   S( 35,  63),   S( 57,  52),   S( 28,  69),   S( 58,  48),   S( 53,  57),   S( 51,  47),   S( 42,  42),
            S( 44,  39),   S( 22,  64),   S( 43,  53),   S( 62,  40),   S( 36,  48),   S( 45,  56),   S( 64,  48),   S( 66,  44),
            S( 89,  31),   S( 69,  32),   S( 35,  55),   S( 19,  36),   S( 48,  45),   S( 47,  48),   S( 44,  42),   S( 21,  48),

            /* rooks: bucket 6 */
            S(-59,  27),   S(-60,  41),   S(-39,  31),   S(-41,  25),   S(-67,  39),   S(-88,  68),   S(-56,  55),   S(-47,  54),
            S(-45,  35),   S(-31,  32),   S(-29,  31),   S(-48,  25),   S(-55,  45),   S(-76,  63),   S(-67,  57),   S( 12,  16),
            S(-42,  59),   S(-29,  39),   S(-13,  40),   S(-45,  42),   S( -3,  30),   S(-39,  62),   S(-29,  74),   S(  4,  41),
            S(-40,  70),   S( 26,  45),   S( -6,  58),   S(  9,  38),   S(  6,  41),   S(  0,  54),   S(-39,  60),   S(-18,  57),
            S(  1,  70),   S( 37,  58),   S( 56,  45),   S( 38,  40),   S( 25,  59),   S( 41,  49),   S( 42,  45),   S( 13,  56),
            S(  9,  62),   S( 58,  50),   S( 81,  31),   S( 45,  31),   S( 33,  45),   S( 50,  57),   S( 55,  47),   S( 61,  48),
            S( 34,  58),   S( 69,  41),   S( 76,  32),   S( 92,  16),   S( 97,  24),   S( 48,  53),   S( 51,  51),   S( 48,  47),
            S( 53,  71),   S( 28,  63),   S( 34,  48),   S( 44,  39),   S( 66,  45),   S( 55,  63),   S( 58,  60),   S( 20,  66),

            /* rooks: bucket 7 */
            S(-77, -12),   S(-51,  -8),   S(-46, -17),   S(-34,  -9),   S(-26,  -6),   S(-61,  35),   S(-51,  22),   S( -7, -13),
            S(-74,  22),   S(-46,   5),   S(-49,   4),   S(-19, -11),   S(-22,  11),   S( -9,  10),   S(-19,  -1),   S(-55,  11),
            S(-90,  50),   S(-43,  16),   S(-18,   7),   S( -9, -11),   S(-10,   5),   S(-18,  -7),   S(-18, -10),   S( 15,   8),
            S(-65,  41),   S( -8,  21),   S(  5,  13),   S( 20,   9),   S( 32,  -3),   S( 31,   5),   S( 36,   1),   S( -8,   9),
            S(-24,  39),   S( 12,  11),   S( 52, -14),   S( 58,  -9),   S( 76,  -4),   S(103,   0),   S( 80,   3),   S( 49, -13),
            S(-18,  32),   S( 15,  12),   S( 82, -27),   S( 98, -25),   S( 75,  -7),   S( 76,  16),   S( 77,  17),   S( 27,   4),
            S(-12,  33),   S( 20,  16),   S( 48,   1),   S( 65,   0),   S( 95,  -9),   S( 92,  -7),   S( 42,  27),   S( 17,  10),
            S(  7,  60),   S(-24,  42),   S( 34,   1),   S( 78, -24),   S( 27,   4),   S( 20,  16),   S( 46,   7),   S( 63,  -5),

            /* rooks: bucket 8 */
            S(-48, -44),   S(-11,  -9),   S(  2,   3),   S( -2, -15),   S(-13, -42),   S(-12, -54),   S(-16, -24),   S( -7, -18),
            S( -3, -18),   S( -5,  -6),   S(  1, -12),   S(  8, -13),   S( -7, -29),   S(-10, -24),   S( -7, -44),   S(-17, -64),
            S(  7,  17),   S(  8, -19),   S(  2,   5),   S( 10,   9),   S(-16, -33),   S( -5, -34),   S( 12,  21),   S( -2,  -1),
            S( -7, -18),   S( -3,  26),   S( -7,   5),   S( 21,   4),   S(  7,  13),   S( -5, -13),   S(  8, -17),   S(  2,   0),
            S( -5,  -9),   S(  2,  16),   S(  0,  28),   S( 16,   8),   S(  2,   4),   S( 21,   2),   S( 11, -13),   S( 13, -36),
            S(  9,  30),   S( -3,   8),   S( 34,  40),   S( 29,  -9),   S(  2,  -5),   S(  6, -14),   S(  4,  -1),   S( 11,  41),
            S(  5,  -9),   S( 14, -15),   S( 26,   4),   S( 21, -19),   S( 30,   7),   S( 22, -21),   S( 20, -14),   S( 18,  -6),
            S(  6, -145),  S( 12, -10),   S( 23,   9),   S( -1,  -9),   S(  3,   1),   S(  4, -13),   S(  7,  -8),   S( 22,  -2),

            /* rooks: bucket 9 */
            S(-50, -15),   S(-13, -20),   S(-24, -27),   S(-40,  -1),   S(-20,   4),   S(-10,  -3),   S( 10, -42),   S(-39, -33),
            S( 29, -19),   S(  4, -17),   S(-17, -18),   S(-19,  -4),   S(-18, -10),   S( 18,   6),   S(  3, -28),   S(-13, -28),
            S( 11, -16),   S( 19,  -6),   S(  4,   5),   S(-10,  -1),   S( -8, -20),   S( 27,  -6),   S( 14,  20),   S( -4,   0),
            S(  3,  10),   S( 11,   4),   S( 13,  21),   S( -2,   3),   S(  6,  17),   S( 24,  -5),   S( 16,  35),   S( 14,   1),
            S( 12,   9),   S(  5,  12),   S(  6,  28),   S( 15,  22),   S( 35,  27),   S( 26,  27),   S( 10,   0),   S( 14,  -8),
            S( 17,  45),   S( -6,  11),   S( 12,   4),   S(-12,   1),   S( 15,   4),   S( 33,   8),   S(  9,  35),   S( 16,  14),
            S( 65,  18),   S( 61,   6),   S( 33,  28),   S( 54,  11),   S( 32,  -8),   S( 31,   6),   S( 39,   1),   S( 46,  25),
            S( 68, -70),   S( 42, -29),   S( 22,  29),   S( 33,  27),   S( 12,  37),   S( 26,  17),   S( 26,  10),   S( 31,   9),

            /* rooks: bucket 10 */
            S(-59, -79),   S(-18, -48),   S(-49, -25),   S(-34,  -3),   S(-36,  -2),   S(-30,  -9),   S(  8, -12),   S(-36, -17),
            S( -4, -15),   S(  9, -27),   S( -4, -25),   S( -6, -14),   S(  2, -17),   S( -9,  -2),   S( 35,   4),   S(  9,   0),
            S(-15, -17),   S(-13, -21),   S(  3, -17),   S( 19,  -4),   S(-18,  19),   S( -3,  -6),   S( 26,  26),   S(  7,  -4),
            S(  4,   1),   S(  7, -12),   S(  2,  -6),   S(  5,  14),   S( 29,  -4),   S(  3,  -6),   S( 26,  25),   S(  0,  -9),
            S(  8,  13),   S( 32,   8),   S( 13,   9),   S( 20, -21),   S( -3,  -4),   S( 15,  10),   S( 31,  31),   S(  9,  28),
            S( 39,  30),   S( 32,  42),   S( 23,  10),   S( 22,   7),   S(  3,  -9),   S( 19,  11),   S( 36,  20),   S( 11,  38),
            S( 76,  12),   S( 81,   2),   S( 77,  -5),   S( 72, -18),   S( 55, -16),   S( 39,  15),   S( 27,   7),   S( 34,   5),
            S( 60,  15),   S(  8,  -1),   S( 40,   0),   S( 23,   7),   S( 35,   0),   S( 30,  15),   S( 15,   3),   S( 21, -10),

            /* rooks: bucket 11 */
            S(-42, -45),   S(-30, -25),   S(-19, -28),   S(-30, -55),   S(  0, -21),   S( -6,   5),   S(-25, -29),   S(-54, -15),
            S(-16, -28),   S( -7, -43),   S( -2, -28),   S( -2, -28),   S( -5, -23),   S(-17, -16),   S( -1, -31),   S(-21,   3),
            S(  2, -31),   S( 19, -14),   S( 21, -15),   S( 11, -22),   S( 11, -10),   S( -9,  11),   S(-23, -25),   S(-10, -52),
            S( -2,  27),   S( -1, -10),   S( -2,  10),   S( 14,   6),   S(  4,  -5),   S( 15,  29),   S( 28, -10),   S(  2, -23),
            S( 12,  11),   S( 19, -11),   S( 30,  -1),   S( 24,  -9),   S( 28,  -5),   S( 34, -11),   S(  9,   7),   S(  0,  -9),
            S( 27,  33),   S( 45,   6),   S( 28, -11),   S( 50,  20),   S( 52,  20),   S( 43,  10),   S( -2,   6),   S( 18,  28),
            S( 64,  36),   S( 62,   2),   S( 71, -14),   S( 76, -15),   S( 49, -10),   S( 54,  12),   S( 36,  34),   S( 57,  -1),
            S( 45,  33),   S( 14,  28),   S( 22,   6),   S( 11,  -8),   S( -6,  -3),   S( 20,  18),   S( 15,  13),   S( 34,  11),

            /* rooks: bucket 12 */
            S( -3,  -9),   S( -9, -30),   S(-13, -52),   S( -4, -10),   S(  0,  -4),   S( -4, -34),   S(-22, -62),   S(-25, -52),
            S(  8,   7),   S( -6, -22),   S(-12, -18),   S( -7, -18),   S(-10,  -7),   S( -8, -16),   S(  1,  -1),   S(-11, -32),
            S(  3,   0),   S( -6, -19),   S( -8, -25),   S(-13,  -9),   S( -5, -22),   S(  6,  -7),   S( -7, -10),   S(  4, -10),
            S( -6,  -8),   S( -1, -13),   S(  3,  12),   S(  8, -11),   S(  1,  -8),   S(-10, -39),   S( -8, -11),   S( -4, -39),
            S( -3,  -7),   S( -2, -19),   S( 12,   4),   S( 10,   8),   S( -8, -37),   S(  6, -19),   S( -6,  -9),   S(  1, -16),
            S( -2,  -7),   S( -3,  -8),   S( 19,  32),   S(  9,  -5),   S( -4,  -7),   S( -6, -21),   S(  1, -25),   S(  4,   8),
            S( -3,  -3),   S(  4, -27),   S(  4, -41),   S( 13,   2),   S(  7,  -4),   S( -6, -38),   S( -3,  -9),   S(  9, -17),
            S( -4, -40),   S(  8,  23),   S(  4, -20),   S(  2,   3),   S( -3, -25),   S(-11, -49),   S(-14, -30),   S(  8,  -4),

            /* rooks: bucket 13 */
            S(-14, -40),   S( -6, -24),   S( -4, -17),   S(  0,  11),   S(  5,  -4),   S(-13, -39),   S(  1, -24),   S(-18, -33),
            S( -2, -33),   S( -2, -12),   S(-12,  -7),   S( -7,  -3),   S(-10, -18),   S( -2, -13),   S(  4,   1),   S( -4, -21),
            S( -5, -29),   S( -7, -27),   S( -5, -37),   S( -2, -23),   S( 10,  11),   S(  2,  -6),   S(  1, -23),   S(  2, -32),
            S( -7, -51),   S(  2,  -4),   S( -9, -43),   S( -5, -11),   S( 13,  12),   S( -8, -38),   S( -2, -28),   S(  2, -19),
            S( 12, -20),   S(  9, -18),   S( 17,  24),   S( -5,  -9),   S(-10, -29),   S(  4, -15),   S( -6, -39),   S(  9,  -8),
            S( -7, -39),   S( 10, -27),   S( -7, -10),   S( 15,  -7),   S(  7, -13),   S( 11,  16),   S(  9,  -3),   S(  4,   8),
            S(  6,  -2),   S( 10,  20),   S( 11,   9),   S(  2, -15),   S( 12, -28),   S( 21,   5),   S(  4, -14),   S(  2, -18),
            S(-14, -121),  S(-16, -67),   S(  7,   6),   S(  2,   1),   S( -3,  15),   S( -3, -29),   S(-10, -28),   S(  5,   1),

            /* rooks: bucket 14 */
            S( -8, -36),   S(-16, -47),   S( -2,  -8),   S( -3, -34),   S(  3, -22),   S(-10, -22),   S( 10,  -5),   S( -7, -21),
            S(-21, -44),   S(-14, -54),   S(-10,   4),   S(-14, -36),   S(-11, -17),   S(  1, -32),   S(  6,  23),   S(  6, -10),
            S( -2, -24),   S( -8, -18),   S( -4, -19),   S( -6, -11),   S(-14, -26),   S( -7, -22),   S(  7,  22),   S( -1, -27),
            S( 12,   6),   S( -7, -34),   S( -4, -19),   S( -5,   6),   S(  3, -13),   S(  4, -13),   S( -4, -35),   S( -2, -21),
            S(  1, -14),   S(  3, -25),   S( -7, -29),   S( -9, -25),   S( -6, -18),   S( -4, -19),   S(  3,   6),   S(  9,   2),
            S(  3, -14),   S(  0, -24),   S(  1, -20),   S(  2, -21),   S(-11, -20),   S( -9,   6),   S(  7,   8),   S(  0,  -7),
            S( 19,   0),   S(  2, -35),   S(  3, -20),   S(  2, -29),   S(  7, -41),   S(  7,   2),   S(  9,  12),   S( 10,   9),
            S( -2, -26),   S(  3, -17),   S( -9, -28),   S( 10,  12),   S( -9, -17),   S(  3,   8),   S(  6,  17),   S(  0, -17),

            /* rooks: bucket 15 */
            S( -2, -54),   S(-13, -42),   S( -1, -28),   S( -7, -29),   S( -1, -16),   S( -4,  -8),   S(-17, -52),   S( -9, -15),
            S(-14, -20),   S(-14, -27),   S(  2,  -2),   S( -6, -23),   S(-10, -27),   S(  7, -25),   S(-11, -41),   S(  7,   4),
            S( -8, -22),   S(-11, -24),   S( -2, -25),   S(  2,   0),   S(  9, -30),   S( -3,  -8),   S( -3,   4),   S( -4, -12),
            S(  2, -31),   S( -4, -26),   S(-11, -18),   S( -5, -18),   S(-11, -19),   S(  3, -18),   S(  0, -16),   S( -9,  -1),
            S(  0, -11),   S( -5, -12),   S( 11,  -8),   S(  0, -13),   S(  1,  -2),   S(  2,   0),   S( -1,   8),   S(  1,  16),
            S(  7,  19),   S(  2,   1),   S(  1, -12),   S(  0, -11),   S( -6,  -9),   S(  2,  11),   S(  5,  -8),   S( -7, -13),
            S( 12,  19),   S( 11,  -7),   S(  8, -34),   S( -4, -33),   S(  1, -21),   S( 12,  36),   S(  3,  -1),   S(  0,  13),
            S(  2, -16),   S( -7, -19),   S(  2,  -6),   S(  1, -11),   S( -6, -13),   S(  0, -25),   S(  1, -17),   S(  2,  -3),

            /* queens: bucket 0 */
            S(-17, -11),   S(-17, -52),   S( 48, -85),   S( 55, -58),   S( 33, -38),   S( 19,  -3),   S( 55,   9),   S( 20,  19),
            S( -9, -11),   S( 31, -59),   S( 37, -16),   S( 21,   6),   S( 20,  31),   S( 22,  22),   S(  8,  61),   S( 37,  21),
            S( 24,   6),   S( 37,  15),   S( 20,  28),   S( 17,  38),   S( 17,  18),   S(  9,  19),   S(  8,  31),   S( 35,  32),
            S( 19,  24),   S( 25,  46),   S(  8,  47),   S( 12,  44),   S( 10,  55),   S( 15,  33),   S( 17,  26),   S( 21,  27),
            S( 40,  52),   S( 28,  43),   S( 18,  40),   S( 24,  53),   S( -4,  28),   S( -4,  12),   S( 34,  19),   S( 45,  -2),
            S( 29,  62),   S( 26,  53),   S( 13,  38),   S( 21,  14),   S( 47,  -9),   S(  6,  36),   S( 29,  19),   S( 28, -19),
            S( 49,  51),   S( 53,  42),   S( 33,  36),   S( 51,  25),   S( 21,   7),   S( -6,  -9),   S( 33,  24),   S( 34,  11),
            S( 48,  30),   S( 22,  36),   S( 43,  18),   S( 36,  35),   S( 46,  32),   S(-12,   3),   S( 52,  29),   S( 48,  30),

            /* queens: bucket 1 */
            S(  0, -15),   S(-74, -23),   S(-51, -28),   S(-14, -68),   S(-11, -25),   S(-20, -48),   S( 13, -29),   S( 10,  27),
            S(-17, -25),   S(-13, -44),   S(  7, -51),   S( -7,   5),   S(-11,   1),   S(  4,  -3),   S( 19, -40),   S( -1,  23),
            S(-32,  42),   S( -2,  -8),   S(  1,  14),   S( -8,  10),   S( -5,  32),   S(-17,  33),   S( 13,   9),   S( 16,  21),
            S( 10, -22),   S(-11,  31),   S(-15,  37),   S( 10,  43),   S( -7,  54),   S(  1,  29),   S(  2,  -3),   S( 18,  17),
            S( 14,   8),   S(  6,  23),   S( -5,  62),   S(-19,  60),   S(-15,  53),   S(  0,  15),   S( -9,  17),   S(  1,  35),
            S( 11,  25),   S( 13,  52),   S( 15,  57),   S(-36,  54),   S(-17,  48),   S(-34,  44),   S( 28,  23),   S( 20,  43),
            S(  4,  33),   S(-11,  68),   S(-19,  31),   S(-25,  67),   S(-27,  47),   S( 16,  25),   S( -9,  40),   S(-25,  47),
            S( -5,   6),   S(  5,  16),   S( 14,  26),   S(-10,  10),   S( -5,  13),   S(  6,  12),   S( 10,  26),   S( -9,  30),

            /* queens: bucket 2 */
            S(  8,  18),   S( 13, -36),   S(  5, -19),   S( -5, -10),   S(-23,   6),   S(-28, -12),   S(-28, -21),   S( 16,  11),
            S( 14,  12),   S( 10,  34),   S( 16, -13),   S( 15, -23),   S( 11, -29),   S( 13, -49),   S( 10,  -9),   S( 34, -24),
            S( 14,  10),   S( 16,   6),   S(  1,  45),   S(  5,  35),   S(  0,  56),   S( 13,  44),   S(  9,  19),   S( 29,  12),
            S(  7,  20),   S( -2,  51),   S( -2,  42),   S(  4,  56),   S(-19,  82),   S( -3,  82),   S( 15,  15),   S(  5,  65),
            S( 15,   6),   S( -6,  53),   S(-13,  55),   S(-34,  97),   S(-40, 109),   S(-16,  77),   S( -9, 102),   S( -7, 106),
            S( 12,  24),   S(  0,  42),   S(-28,  75),   S(-10,  51),   S(-31,  90),   S(-12,  93),   S( -3,  94),   S( 12,  75),
            S(-18,  51),   S(-34,  76),   S(-16,  58),   S(  5,  60),   S(-22,  74),   S( 24,  40),   S(-20,  44),   S( -9,  76),
            S(-65,  75),   S(  5,  33),   S( 29,  36),   S( 33,  28),   S(  1,  61),   S( 18,  32),   S( 14,  25),   S(-14,  41),

            /* queens: bucket 3 */
            S( 83,  91),   S( 55,  97),   S( 48, 101),   S( 41,  84),   S( 66,  32),   S( 46,  24),   S( 21,  24),   S( 48,  63),
            S( 66, 114),   S( 59, 108),   S( 43, 112),   S( 45,  87),   S( 46,  77),   S( 60,  48),   S( 64,  10),   S( 38,  52),
            S( 60,  89),   S( 52, 105),   S( 54,  83),   S( 51,  78),   S( 48,  91),   S( 52,  95),   S( 60,  96),   S( 62,  75),
            S( 48, 123),   S( 61,  82),   S( 49,  94),   S( 43,  93),   S( 44,  92),   S( 40, 129),   S( 59, 101),   S( 50, 136),
            S( 65,  91),   S( 58, 103),   S( 52,  88),   S( 40,  94),   S( 36, 114),   S( 27, 126),   S( 40, 164),   S( 54, 155),
            S( 49, 122),   S( 58,  98),   S( 52,  93),   S( 25, 117),   S( 33, 131),   S( 75, 101),   S( 64, 137),   S( 35, 191),
            S( 63, 115),   S( 61, 101),   S( 71,  83),   S( 61,  92),   S( 34, 109),   S( 60, 110),   S( 94, 125),   S(160,  73),
            S( 79,  89),   S(105,  74),   S( 78,  83),   S( 82,  77),   S( 43, 103),   S(112,  50),   S(137,  57),   S(146,  56),

            /* queens: bucket 4 */
            S(-13, -23),   S(-19, -20),   S(-22,  -9),   S(-11,  -9),   S( 10, -16),   S( 34,  -2),   S(-34,  -9),   S(-26,  -3),
            S(-28, -18),   S(-26,  -4),   S( 14,  -8),   S(-41,  23),   S( -3,  -6),   S(  1, -15),   S( -8, -11),   S(-35, -16),
            S(  2,   2),   S( 12,  -1),   S(  3,  29),   S( -2,  31),   S( 20,  16),   S(  4,  -8),   S(  7, -20),   S(-25, -24),
            S(-14,   4),   S( -8,  14),   S(  1,  37),   S( -2,  31),   S( 15,  35),   S( 19,  18),   S(  3, -14),   S( -4,  -8),
            S( -8,   0),   S( 18,  14),   S( 17,  28),   S( 30,  41),   S( 22,  29),   S( 19,  -1),   S(-22, -18),   S( -7, -29),
            S(  5,  14),   S( 35,  14),   S( 25,  54),   S( 22,  43),   S( 11,   7),   S(  2,   3),   S(-16, -14),   S(-15,  -9),
            S(-10, -18),   S( -5,  18),   S(  2,  26),   S( 31,  32),   S(  9,  10),   S(-12,  -4),   S(-22, -42),   S(-21, -25),
            S( -2, -15),   S( -1,  -2),   S( 30,  36),   S(  4,  17),   S(-17, -18),   S( -7, -11),   S(-20, -34),   S( -8, -18),

            /* queens: bucket 5 */
            S(-37, -15),   S(-27, -32),   S(-32, -29),   S(-45, -28),   S(-57, -30),   S(  8, -16),   S(-10,  -6),   S( -6,  -7),
            S(-29,  -5),   S(-38, -13),   S(-70, -21),   S(-67,  -3),   S(-16,  -3),   S(-43, -17),   S(-48, -17),   S(-50, -16),
            S(-35,   4),   S(-58, -11),   S(-65,   4),   S(-31,  32),   S( 15,  53),   S(-11,  21),   S( -3,  -1),   S( 10,  20),
            S(-54, -10),   S(-51,  -3),   S( -1,  36),   S( -5,  52),   S( 11,  29),   S( -3,  13),   S( -5,  -8),   S( -9,  13),
            S(-33,  -6),   S(-21,  20),   S(-11,  48),   S( -6,  45),   S( 27,  49),   S(  1,  18),   S(  0,   8),   S(-30, -29),
            S(-18,  16),   S(  7,  37),   S(-12,  42),   S(  1,  45),   S( 40,  50),   S(  4,  13),   S(  1,   1),   S(-11, -12),
            S( -9,   9),   S(-10,  13),   S(  5,  59),   S( -3,  33),   S(  0,  38),   S( 21,  32),   S( 11,   9),   S(-21, -16),
            S(  9,  26),   S( 11,  11),   S(  2,  18),   S( 11,  49),   S( 16,  29),   S(  3,  20),   S( -1, -24),   S(-18, -16),

            /* queens: bucket 6 */
            S(-31,   4),   S(-53, -26),   S(-66, -28),   S(-87, -59),   S(-92, -51),   S(-72, -45),   S(-51, -43),   S(-28,   4),
            S(-62, -13),   S(-46,  -1),   S(-55,  13),   S(-63,  11),   S(-79,  16),   S(-91,  -2),   S(-85, -19),   S(  7,  18),
            S(-46,  10),   S(-23,  11),   S(-57,  40),   S(-100,  87),  S(-38,  50),   S(-36,   3),   S(-48, -13),   S(  4,   6),
            S(-44,  12),   S(-27,   9),   S(-29,  62),   S(-49,  70),   S(  4,  44),   S( 11,  49),   S(-10,  35),   S( 10,  -8),
            S(-53,  19),   S( -6,  36),   S(-28,  52),   S(  8,  30),   S( 30,  55),   S( 59,  38),   S( 26,  32),   S( -6,  18),
            S(-25,  41),   S(-12,  17),   S( 23,  21),   S( 21,  45),   S(  7,  53),   S( 62,  66),   S( -5,  -7),   S(-16,  11),
            S( -7,   6),   S(  1,   1),   S(-14,  40),   S(-11,  34),   S( 27,  50),   S( 17,  58),   S( -9,  22),   S(-37,   0),
            S(  0,   7),   S( 17,  11),   S( 11,  28),   S( -4,  22),   S( 30,  38),   S( 19,  27),   S( -3,  14),   S(  3,   7),

            /* queens: bucket 7 */
            S(-10, -14),   S(-37,  12),   S(-50,  18),   S(-38,   8),   S(-31, -11),   S(-35, -24),   S(-30,  -5),   S(-13,  -9),
            S(-35, -10),   S(-53,   3),   S(-31,   9),   S(-26,  37),   S(-33,  33),   S(-46,  39),   S(-43,  22),   S(-32, -13),
            S(-37, -23),   S(-55,  30),   S(-22,  32),   S(-15,  29),   S(  3,  22),   S(  0,  30),   S( -8,  16),   S(-13,   3),
            S(-63,   1),   S(  6,   0),   S(-20,  23),   S( -6,  40),   S( 31,  22),   S( 32,  24),   S( 14,  33),   S(  2,  22),
            S(-30,  18),   S(-53,  24),   S(  9,  18),   S( 46,  -5),   S( 64, -12),   S( 84, -16),   S( 37,  12),   S( 44,  -8),
            S(-17,  11),   S(-17,   7),   S(  4,  -1),   S( 15, -10),   S( 36,  35),   S( 81,  20),   S( 65,   2),   S( 49,  13),
            S(  7, -20),   S(  1,   9),   S(  2,  -8),   S(  4,  12),   S( 38,  16),   S( 54,  37),   S( 52,  18),   S( 54,  27),
            S( 14,   3),   S( 19,   3),   S( 21,   7),   S( 21,  15),   S( 41,  23),   S( 24,  19),   S( 17,   6),   S( 40,  43),

            /* queens: bucket 8 */
            S( -5, -10),   S( -1,   3),   S(-13,  -5),   S( -9,  -7),   S( -4,   0),   S( -1, -16),   S(-20, -25),   S( -4,   4),
            S( -7,   0),   S(-11, -15),   S( -4,   5),   S(-13,  -3),   S( -5,  -5),   S(-17, -20),   S(-18, -39),   S( -5,  -9),
            S( -1,   0),   S( -6,   2),   S( -6,   3),   S( -6, -10),   S( -5,   4),   S(-12, -12),   S(-11, -27),   S(-16, -28),
            S( -3,   3),   S(  9,  19),   S( 12,  18),   S(  6,  12),   S( -2,   1),   S( -6,  -1),   S( -2,  -3),   S( -7, -21),
            S( 16,  28),   S(  3,  28),   S( 12,  14),   S( 12,  20),   S( 13,  30),   S(  3,  -1),   S( -8, -11),   S(-10, -17),
            S(  8,  20),   S( 13,  23),   S(-18,  15),   S( 15,  34),   S( -8, -14),   S( -5, -11),   S(  3,   2),   S(  3,  12),
            S( -6, -12),   S(-16, -24),   S( 22,  36),   S( 14,  15),   S(  2,  17),   S(  3,  17),   S( -2,  -7),   S( -7, -16),
            S(-14, -28),   S( 14,  12),   S(-15, -47),   S( -9,  -6),   S(-11, -30),   S( -1,  -6),   S( -3, -17),   S( -5,  -7),

            /* queens: bucket 9 */
            S(  5,   7),   S(-13, -27),   S(  1,  -2),   S(-30, -32),   S(-23, -38),   S(-18, -30),   S(-13, -20),   S(-13, -18),
            S( -3,  -5),   S( -9,  -7),   S(-19, -24),   S( -4,   0),   S(-17,  -8),   S(-15, -18),   S(  2,  -1),   S( -3,  -7),
            S(  4,   6),   S(  3,   9),   S( -8,  21),   S( -4,  -4),   S( -4,   9),   S(  1,  -2),   S(  3,   3),   S(  3,  -1),
            S( -5,  -9),   S( -5,   5),   S( 13,  41),   S(  8,  23),   S( 18,  31),   S(  5,  13),   S( -9, -16),   S(  0, -10),
            S(  6,  11),   S(  9,  32),   S( 12,  33),   S( 17,  52),   S( 21,  34),   S( 10,  19),   S( -3,   5),   S(-11, -14),
            S(-18, -20),   S(-15,  -1),   S(  5,  22),   S( 15,  35),   S( -4,   2),   S( -1,  10),   S( -9,  -7),   S( -5,  -6),
            S( -6, -17),   S(-10, -24),   S( -7,  24),   S( 12,  30),   S( 16,  20),   S(  7,  -5),   S(  6,  -4),   S(-12, -25),
            S(  0,  -1),   S( -3, -22),   S( 12,  -1),   S(  1,  15),   S( 13,   1),   S( -2,   0),   S( 11,   2),   S(  2, -16),

            /* queens: bucket 10 */
            S(  3,  -1),   S( -3,   2),   S(-11, -18),   S(-22, -26),   S(-12, -14),   S( -6,  -5),   S(  2, -12),   S( -6,  -9),
            S( -8, -12),   S( -8, -15),   S(-15, -25),   S( -9, -14),   S( -5,  -7),   S(-18, -13),   S(  1,  -8),   S(-17, -19),
            S( -2, -13),   S( -9, -14),   S( -8,  -9),   S( -1,   3),   S( -6,   3),   S( -7,   3),   S(  1,   2),   S(  2,   6),
            S(  0,  -2),   S(  1,  -4),   S( -2,  -6),   S(  0,  31),   S( 15,  25),   S( -6,   6),   S( -3,  -5),   S(-14, -19),
            S( -5,  -6),   S(  5,  -7),   S( -5,   5),   S( 22,  48),   S(  1,  -2),   S( 17,  30),   S( 12,  14),   S(  1,   6),
            S( -3,  -5),   S(-20, -32),   S( -4,   0),   S(  2,  14),   S(  5,  16),   S(  5,  20),   S( 12,   9),   S( -4, -11),
            S( -6,  -6),   S(-18, -28),   S(  8,  22),   S( -7,  -8),   S(  7,   6),   S(  4,  10),   S( -3,  -7),   S( -8,  -5),
            S(  5,  -1),   S( -3, -16),   S(  6,  -4),   S(  7,  -5),   S( 18,  16),   S(  5,   7),   S( 16,  15),   S(  1,  -8),

            /* queens: bucket 11 */
            S(-10, -14),   S( -7, -20),   S(-22, -21),   S(-11, -28),   S(-13, -19),   S( -9, -11),   S( -5,  -6),   S(-12, -23),
            S(-18, -32),   S( -9,  -8),   S(-41, -36),   S(-11, -10),   S(-12, -10),   S( -9,  -6),   S( -5,  -9),   S( -6,  -3),
            S(-18, -23),   S(-17, -36),   S(  3, -21),   S( -9, -18),   S( -8, -13),   S( -2,   6),   S(  8,  20),   S(-12,  -8),
            S(-16, -28),   S(-25, -25),   S( -8, -25),   S( 15,  28),   S( 10,   1),   S(-12,  -6),   S( 24,  25),   S( -2,  -1),
            S(-14, -13),   S( -6, -18),   S(-22, -27),   S( 24,  19),   S( 16,  15),   S( 27,  51),   S( 22,  42),   S(  3,  12),
            S(-14, -30),   S(  2,   2),   S(-17, -19),   S( 14,  10),   S( 24,   5),   S( 46,  37),   S( 11,   0),   S( -7,  -5),
            S( -9,  -4),   S(-14, -23),   S(  8,  15),   S(-14,  -5),   S(  6,   6),   S( 23,  25),   S( 38,  39),   S( -2, -16),
            S(-12, -23),   S(-10, -25),   S( -8, -22),   S(  3, -15),   S(  1,   9),   S( -2,  -8),   S( 20,  10),   S(  0, -31),

            /* queens: bucket 12 */
            S(  6,   0),   S( -1,  -2),   S(  2,   1),   S( -8,  -5),   S( -9, -12),   S( -2,  -3),   S(  0,  -2),   S( -4, -10),
            S( -4,  -3),   S( -8, -14),   S( -9, -12),   S( -5, -11),   S( -3,  -3),   S( -6,  -2),   S( -1,  -9),   S( -6,  -9),
            S( -2,  -5),   S( -5,  -9),   S( 12,  13),   S( -5,  -5),   S( -2,  -5),   S( -8, -14),   S(-13, -25),   S( -9,  -8),
            S(  2,   7),   S( -1,   3),   S(  3,   6),   S(  0,   9),   S(  7,  14),   S(  0,  -4),   S(  0,  -4),   S( -4, -12),
            S(  2,  -2),   S( 11,  13),   S( 32,  57),   S(  1,  15),   S( -5,   7),   S(  0,   5),   S(-13, -31),   S( -2, -15),
            S(  8,  19),   S( 14,  25),   S( 34,  43),   S( -2,   7),   S(  0,   5),   S(  2,   2),   S(  5,   4),   S( -5, -15),
            S(  2,   1),   S(  3,   8),   S( 17,  14),   S( 12,   9),   S(  5,   9),   S( -4,   3),   S(  8,   5),   S( -4,  -4),
            S( -3, -26),   S( -8, -24),   S(-12, -17),   S(-10, -28),   S( 11,  -7),   S(  1,  -2),   S(  2,  -5),   S( -6, -11),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -5, -15),   S(  0,  -5),   S( -3,  -8),   S( -3, -11),   S( -3,  -3),   S( -7, -11),   S( -6,  -8),
            S(  4,   9),   S(  5,  13),   S(  4,  10),   S( -4,  -4),   S( -6,  -6),   S(  2,  10),   S(  1,   6),   S(-11, -19),
            S( -3,  -7),   S(  0,   0),   S(  3,  15),   S(  2,  10),   S( -2,  -2),   S( -6,  -9),   S( -5, -12),   S(-12, -17),
            S( -3,  -6),   S(  2,   2),   S( 11,  12),   S( 18,  28),   S( 15,  33),   S( -4,  -7),   S( -5, -14),   S( -5,  -6),
            S( -3,  -1),   S(  6,  17),   S( 16,  41),   S( 13,  38),   S( 24,  45),   S(  0,  -9),   S( -5,  -8),   S( -7, -14),
            S(  0,   0),   S( 12,  32),   S( 39,  74),   S( 19,  41),   S(  1,  16),   S(  1,   7),   S(  6,  14),   S( -5, -14),
            S( -1,   1),   S( 19,  32),   S(  9,  28),   S( 13,  24),   S( -1,  10),   S(  1,  -8),   S( -2, -10),   S(  6,   8),
            S(-11, -16),   S(  5,  -1),   S( -2,  -7),   S( -8, -10),   S(  7,   2),   S(  5,   8),   S( -8,  -7),   S( -6, -13),

            /* queens: bucket 14 */
            S( -2,  -2),   S(  0,   1),   S( -2,  -8),   S( -9,  -9),   S(  4,   6),   S( -2,  -5),   S( -2,  -9),   S( -5, -11),
            S( -5,  -7),   S(  5,  16),   S( -2,  -4),   S( -1,  -7),   S( -9, -12),   S( -7, -15),   S( -5,  -4),   S( -3,  -8),
            S( -2,  -2),   S(-10, -12),   S( -6, -14),   S( -1,  -2),   S(  1,   0),   S(  1,  -4),   S(  3,   5),   S( -6, -14),
            S( -8,  -9),   S(  8,   8),   S( -6,  -4),   S( 23,  41),   S( 14,  15),   S( -1,   6),   S( 11,  24),   S(  1,  -4),
            S(  4,  13),   S(  4,   0),   S(-14,  -8),   S( 16,  27),   S( 14,  34),   S( 17,  25),   S( 10,  18),   S( -4,  -9),
            S( -2,  -5),   S(  4,  13),   S( 14,  24),   S( 12,  21),   S( 17,  42),   S( 15,  46),   S(  8,  16),   S( -2,  -8),
            S(  3,   7),   S(  8,   9),   S( 16,  36),   S( 19,  32),   S( 15,  33),   S( 14,  27),   S( 16,  29),   S(  2,   5),
            S( -3,  -1),   S(  0,   0),   S( -9, -14),   S( 12,  19),   S(  1,   5),   S(  3,   1),   S(  3,   5),   S(-10, -17),

            /* queens: bucket 15 */
            S( -1,  -3),   S(  1,  -6),   S( -5,  -8),   S( -2, -10),   S( -6, -11),   S( -5, -12),   S(-11, -25),   S(  0,  -6),
            S( -1,  -4),   S( -4,  -9),   S( -5, -13),   S( -4, -11),   S(  0,   8),   S( -3,  -8),   S( 11,  13),   S(  2,   1),
            S(  0,  -9),   S( -3, -12),   S( -1,  -2),   S( -4, -12),   S( -4, -11),   S(  5,  16),   S( -2,  -5),   S(  0,  -8),
            S( -5,  -8),   S(  3,   3),   S( -4,  -3),   S(  4,   2),   S(  2,  10),   S(  0,   6),   S(  6,   5),   S(  4,   3),
            S( -2,  -7),   S( -2,  -5),   S( -8, -13),   S( -4,  -5),   S(  5,  10),   S(  8,   6),   S( -3,  -6),   S(  0,  -7),
            S( -3,  -7),   S( -2,  -6),   S( -1,   2),   S(  1,   1),   S( -1,  -6),   S( 20,  31),   S(  4,  -1),   S(  0,  -8),
            S( -6, -13),   S(  4,  -6),   S(  6,   7),   S(  7,   6),   S(  6,   8),   S( 22,  37),   S( 11,  20),   S(  4,   5),
            S(  1,  -4),   S( -5,  -6),   S( -2,  -5),   S( 10,  12),   S(  7,   2),   S(  4,  -3),   S( -2,  -6),   S( -6, -20),

            /* kings: bucket 0 */
            S( 68,  -2),   S( 49,  47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 41,  35),   S(109,  62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 48,  17),   S( -4,  35),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 75,  49),   S( 54,  60),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -8,  38),   S(  0,  28),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 43,  66),   S( 49,  50),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -3,  49),   S(-23,  15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  84),   S(-36,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 21, -62),   S( 73, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -12),   S( 14,  19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 42, -15),   S( 20,  -1),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 12,  32),   S( -3,  29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 46,  16),   S( 12,  13),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 10,  49),   S( -8,  48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 58,  32),   S( 17, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 30,  66),   S(-10,  25),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -124),  S(  8, -59),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -107),  S(-100, -17),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  6, -52),   S(-36, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-40, -30),   S(-49, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-14, -38),   S(-23, -33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-37, -23),   S(-93,   9),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-26, -36),   S(-38, -107),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-90,  -5),   S(-14, -88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -109),  S(-76, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -227),  S(-15, -99),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-68, -57),   S( 22, -67),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-50, -78),   S(-20, -105),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-12, -56),   S(-114, -15),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23, -118),  S(-64, -67),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-126,   1),  S(-35, -114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-47, -72),   S(  0, -224),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -22),   S(-26,  14),   S( 11,  -2),   S( -5,  21),   S( 20,  -1),   S( 43,   7),   S( 49,  -8),   S( 49,   1),
            S(-12, -28),   S(-29,   1),   S( -1, -12),   S(  0, -14),   S( 14,   2),   S( -1,  13),   S( 28,  -3),   S( 22,  23),
            S(  4, -27),   S(  1, -22),   S( 32, -34),   S(  9, -17),   S( 17,  -9),   S( 10,  26),   S( -3,  46),   S( 32,  21),
            S( 11, -19),   S( 31,   2),   S( 51, -28),   S( 38,  -4),   S( 19,  44),   S(-17,  86),   S(  8,  84),   S( 56,  64),
            S( 91, -53),   S(125, -17),   S( 89, -25),   S( 43,  15),   S( 46, 137),   S( -6, 138),   S( 13, 155),   S( 64, 133),
            S(-223, -66),  S(-117, -132), S( 13, -164),  S( 37,  43),   S( 84, 197),   S( 66, 187),   S(109, 168),   S(100, 146),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  20),   S(-44,  25),   S(-18,  10),   S(-39,  56),   S(-14,   2),   S( 16,   8),   S( 15,   0),   S( 15,  26),
            S(-54,  16),   S(-49,  18),   S(-33,   9),   S(-19,   5),   S( -1,   6),   S(-16,   8),   S( -5,   0),   S(-16,  21),
            S(-46,  24),   S(-19,  20),   S(-24,   5),   S(  7, -10),   S( -2,  18),   S(-21,  18),   S(-30,  30),   S(-11,  27),
            S(-34,  43),   S(  8,  26),   S(-19,  26),   S( 15,  27),   S(  5,  27),   S(-35,  47),   S( -2,  39),   S( 27,  55),
            S(  3,  37),   S( 62,  -3),   S( 93, -26),   S( 87, -23),   S( 32,  30),   S(  1,  35),   S(-26,  80),   S( 36,  92),
            S( 45,  44),   S(-37, -16),   S( -9, -100),  S(-12, -97),   S(-39, -65),   S( -3,  44),   S( 52, 185),   S( 69, 213),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44,  43),   S(-33,  25),   S(-20,  13),   S(-13,  19),   S(-32,  37),   S(-11,  11),   S(  2,  -9),   S( -7,  21),
            S(-55,  34),   S(-40,  27),   S(-33,   9),   S(-27,  16),   S(-25,  16),   S(-34,   6),   S(-13, -13),   S(-38,  11),
            S(-45,  50),   S(-40,  53),   S(-11,  15),   S(-16,  17),   S(-20,  20),   S(-23,   4),   S(-28,   7),   S(-28,  10),
            S(-32,  89),   S(-43,  76),   S(-15,  44),   S(  2,  37),   S( -5,  34),   S(-23,  18),   S(  5,  17),   S( 23,  12),
            S(-30, 133),   S(-46, 118),   S( -8,  23),   S( 24, -24),   S( 98, -10),   S( 92,  -8),   S( 73, -16),   S( 51,   3),
            S(-10, 248),   S( 32, 178),   S( 14,  69),   S( 27, -91),   S(-14, -171),  S(-79, -132),  S(-59, -54),   S( 17,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  16),   S( -1,  15),   S(  8,  11),   S(  1,  33),   S( -7,  50),   S( 31,  21),   S( 22,  -4),   S(  8, -13),
            S( -3,  18),   S( -3,  25),   S( -2,   9),   S( -1,   6),   S( 11,  14),   S( 15,  -1),   S( 10, -14),   S(-18,  -7),
            S(  3,  37),   S(-10,  58),   S(  8,  18),   S(  5,   1),   S( 23, -12),   S( 18, -14),   S(  5, -22),   S(-13, -12),
            S(  2,  91),   S(-18, 104),   S(  6,  69),   S( 19,  34),   S( 28,   0),   S( 32, -25),   S( 22,   2),   S( 35, -21),
            S( -2, 157),   S(-16, 168),   S(-29, 166),   S( -9, 114),   S( 34,  51),   S( 87, -16),   S(115, -36),   S( 97, -40),
            S(105, 125),   S( 48, 237),   S( 27, 252),   S(  8, 209),   S(-23,  93),   S( 29, -174),  S(-56, -243),  S(-163, -173),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 69,   1),   S( 22,   6),   S( -5,  -7),   S(-10,  -9),   S(  4, -14),   S(  4, -10),   S(  0,  -9),   S(-58,  42),
            S( 39,  -3),   S(  5,  19),   S(  3,  -1),   S(-15,  -4),   S(-23, -22),   S(-15, -18),   S(-31, -22),   S(-43,   3),
            S( 65, -14),   S(107, -29),   S( 34, -17),   S(-32,  -3),   S(-74,   9),   S(-14,   4),   S(-73,  21),   S(-61,  31),
            S(-87, -73),   S(-16, -92),   S( 68, -58),   S(-28,   7),   S(-26,  17),   S(-64,  67),   S(-37,  56),   S(-52,  78),
            S(-29, -73),   S(-64, -112),  S( -8, -91),   S( 55,   6),   S( 73,  89),   S( -6, 101),   S( 13,  76),   S( -3, 102),
            S(  4, -58),   S(-15, -74),   S(  2, -63),   S(  3,  50),   S( 57,  88),   S( 65, 151),   S( 45, 156),   S( 57, 124),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  47),   S(-46,  45),   S( -8,  24),   S( 55,   7),   S( 61,  -2),   S(  9,   4),   S(-18,  10),   S(-53,  46),
            S(-75,  40),   S(-38,  40),   S(-24,  25),   S( -8,  23),   S(-20,  21),   S(-29,   8),   S(-51,   3),   S(-74,  32),
            S(-34,  29),   S(-35,  55),   S( 23,  28),   S(  4,  39),   S(-32,  42),   S(-64,  33),   S(-61,  31),   S(-61,  43),
            S(-25,  38),   S(-10,  10),   S(-32, -37),   S(  0, -25),   S( -6,  -6),   S(-53,  33),   S(-10,  29),   S(-30,  56),
            S( 56,   8),   S( -9, -32),   S( 29, -95),   S(  6, -72),   S( 50, -41),   S( 17,  21),   S(-20,  69),   S(-44, 118),
            S( 49,  33),   S( 18, -12),   S(-28, -66),   S(-16, -59),   S(-30, -57),   S( 46,  40),   S( 62, 135),   S( 41, 148),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87,  42),   S(-58,  20),   S(-20,   5),   S(  8,   4),   S(  7,  24),   S( 15,  12),   S( 13,   7),   S(  2,  26),
            S(-78,  24),   S(-59,  16),   S(-51,  10),   S( 15,  11),   S(-15,  26),   S(-12,  12),   S(-16,  10),   S( -8,  12),
            S(-61,  36),   S(-73,  42),   S(-52,  32),   S(-45,  44),   S( -2,  40),   S( 11,  18),   S(  3,  21),   S(-13,  18),
            S(-91,  90),   S(-61,  59),   S(-33,  35),   S(-19,  19),   S(-13, -33),   S(-20, -29),   S(-26,   5),   S( 31,  -2),
            S(-18, 104),   S(-52,  74),   S( 23,  10),   S(-12, -30),   S(  4, -72),   S(-40, -66),   S( -6, -32),   S( 78,  -4),
            S( 78,  78),   S( 69,  89),   S( 44,  24),   S( 40, -77),   S( -5, -103),  S(-41, -50),   S( -6, -44),   S( 74,   3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52,   4),   S(-40, -14),   S( -4, -22),   S(-64,  45),   S( 19,   6),   S( 66, -17),   S( 58, -27),   S( 68, -11),
            S(-59,   4),   S(-63,   5),   S(-36, -20),   S(-38,   4),   S( -1,  -2),   S( 43, -28),   S( 28, -16),   S( 50, -18),
            S(-57,  26),   S(-77,  39),   S(-41,   7),   S(-47,   2),   S( -2,  -3),   S( 21, -16),   S( 57, -16),   S( 54, -19),
            S(-55,  63),   S(-91,  81),   S(-59,  63),   S(-36,  38),   S(-14,  -1),   S( 38, -58),   S( 16, -73),   S( 22, -108),
            S( 14,  62),   S(-65, 136),   S(-10, 119),   S(-13,  88),   S(  8,  22),   S( 13, -79),   S(-46, -131),  S(-14, -99),
            S(131,  84),   S( 81, 123),   S( 88, 106),   S( 57,  95),   S( 33,   4),   S(  4, -100),  S(-25, -89),   S( -8, -179),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 28,   7),   S( 12,   9),   S( 52,   0),   S( -8, -34),   S(-29, -62),   S(-20, -25),   S( 16, -57),   S( 39, -41),
            S( 16, -60),   S( 13, -15),   S(-38, -55),   S(-54, -36),   S(-31, -58),   S( 32, -63),   S( 10, -67),   S( -4, -52),
            S( 35, -96),   S(  9, -55),   S( -2, -63),   S(-45, -51),   S(-31, -28),   S( 10, -41),   S(-39, -21),   S( -1, -29),
            S(  3, -25),   S(-23, -37),   S( 16, -16),   S(-12,   0),   S(-23,  10),   S(  1,  23),   S( -6,  24),   S(-12,  24),
            S( 25,   7),   S( -2, -32),   S(  8,  45),   S( 33,  93),   S( 54, 120),   S( 29, 121),   S( 13,  96),   S(-30, 106),
            S( 19,  34),   S(  5,  57),   S( 25,  71),   S( 32,  99),   S( 46,  97),   S( 51, 151),   S( 39, 102),   S(-21,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 32,   9),   S( 35,  21),   S( 23,  19),   S(  5,  29),   S( 19,   1),   S( 15, -16),   S( 33, -46),   S(-17, -19),
            S( 60, -58),   S( 17, -49),   S( 12, -58),   S(-13, -43),   S(-29, -25),   S(-47, -28),   S(-46, -35),   S( 17, -45),
            S( -9, -42),   S(-27, -42),   S(-18, -72),   S(-60, -42),   S( -4, -35),   S(-13, -46),   S(-55, -34),   S( 16, -32),
            S(-43,   1),   S(-43, -49),   S( -1, -68),   S(-39, -27),   S(  0, -41),   S( -4, -23),   S( 12,  -8),   S(  2,   9),
            S(  4,  13),   S( -9, -19),   S(-18,   4),   S( 20,  30),   S( 17,  61),   S( 19,  54),   S(  0,  68),   S(  1,  64),
            S( -9,  69),   S( 27,  62),   S( -2,  58),   S( 22,  57),   S( 27, 109),   S( 16,  86),   S( 17,  81),   S( 16,  82),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-25, -51),   S( -1, -48),   S( -1, -17),   S( -2, -12),   S( 36,  16),   S( 69,  12),   S( 21,   4),   S( 13, -17),
            S( -8, -60),   S(-62, -41),   S(-12, -50),   S( 20, -38),   S( -1, -30),   S(-10, -23),   S( 18, -39),   S( 17, -43),
            S(-19, -45),   S(-85, -22),   S(-64, -38),   S(-13, -30),   S(-20, -46),   S(-12, -61),   S(-24, -62),   S( 63, -67),
            S(-36,   1),   S(-19,  -6),   S(-24, -31),   S(-55, -36),   S(  2, -67),   S(-46, -50),   S(-22, -51),   S( 18, -48),
            S( 12,  16),   S( 31,  16),   S( 15,  13),   S(-19,  -1),   S(  9,  21),   S( 13,  14),   S(-31,  11),   S( 42,  -1),
            S(  7,  26),   S(  2,  49),   S( 25,  56),   S(  7,  58),   S( 24,  84),   S(  1,  45),   S(-12,  23),   S( 25,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-34, -45),   S( -1, -46),   S(-32, -41),   S(  6,  -2),   S(  2, -19),   S( 65,  11),   S( 53,  -8),   S( 56,  -8),
            S(-35, -61),   S(-49, -62),   S(-37, -71),   S( -4, -61),   S(-23, -31),   S( 12, -47),   S( 27, -47),   S( 40, -71),
            S(-24, -38),   S(-86,  -5),   S(-32, -22),   S(-13, -25),   S(-68, -41),   S( 40, -65),   S( 27, -120),  S( 88, -102),
            S(-51,  23),   S(-70,  32),   S(  3,  28),   S( 16,  -7),   S(-31, -11),   S(-22, -42),   S(-34, -53),   S( 40, -94),
            S(-15,  21),   S(-19,  67),   S(-10,  96),   S( 17,  60),   S( 27,  64),   S(-10,   8),   S( -2,   8),   S(  9, -23),
            S( 15,  71),   S( 28,  57),   S( 30,  83),   S( 26,  84),   S( 12,  66),   S( 34,  83),   S( 12,  31),   S( 28,  39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -106),  S( 30, -49),   S( -3, -29),   S(  0,  -1),   S( -6, -30),   S(-38, -72),   S( 17, -47),   S(  4, -45),
            S( 40, -85),   S( 28, -45),   S(-22, -73),   S(-32, -58),   S(-33, -86),   S(-12, -60),   S(-15, -89),   S(-20, -66),
            S( -7, -61),   S( -9, -77),   S(-23, -96),   S(-25, -83),   S(-11, -54),   S( -8, -46),   S(-37, -60),   S(-10, -76),
            S(-12, -38),   S( -4, -16),   S(-19, -17),   S( -2,   3),   S( 17,  57),   S(  4,  41),   S(  4,  10),   S( -6,  -3),
            S( 11,  22),   S(  1,  18),   S(  3,  24),   S( 20,  62),   S( 30,  77),   S( 24,  87),   S( 12,  79),   S( 20,  51),
            S( 12,  29),   S(  2,  39),   S( 13,  53),   S( 12,  60),   S( 25, 100),   S( 24,  91),   S(-21, -23),   S(-14,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -63),   S( 30, -82),   S( 20,   3),   S( -2, -11),   S(  5, -20),   S(-30, -40),   S(-10, -74),   S(-15, -68),
            S( 30, -132),  S( 19, -100),  S(  0, -85),   S( 12,  -9),   S(-25, -53),   S(  1, -80),   S(  0, -92),   S(  0, -88),
            S( 32, -88),   S(-10, -76),   S( -3, -90),   S(  6, -60),   S(-44, -28),   S( 21, -74),   S( -8, -74),   S( 59, -88),
            S( 16, -25),   S( -1, -33),   S(  2, -29),   S( -4,  26),   S( 14,   6),   S(-17,  10),   S(-13, -17),   S(  8, -21),
            S( -3,  42),   S(  7,  23),   S( -2,   6),   S( 22,  56),   S( 39,  78),   S( 27,  89),   S( 11,  96),   S( -6,  59),
            S( 11, 102),   S( 29,  52),   S(  3,  35),   S( 12,  45),   S( 20,  63),   S( 10,  52),   S( -4,  40),   S(  1,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -116),  S(  5, -70),   S( -5, -41),   S(  3,   4),   S( -6, -14),   S( -1,  -1),   S( 20, -69),   S( -9, -42),
            S( 17, -112),  S(-38, -106),  S( -5, -80),   S(-28, -86),   S( -9, -56),   S( 16, -50),   S(  1, -65),   S( 25, -85),
            S( 18, -92),   S(-21, -77),   S(-14, -61),   S(  3, -74),   S(-24, -52),   S(  3, -89),   S(  0, -101),  S( 35, -58),
            S(  5, -30),   S(-21, -38),   S( -5,  -4),   S(-21,  -8),   S( 12, -50),   S( -5, -28),   S( 12, -30),   S( 11,  -4),
            S(-13, -13),   S(  6,  37),   S( 11,  51),   S( -8,  16),   S( 20,  71),   S(  2,  18),   S( 17,  46),   S( 23,  67),
            S( -6,  34),   S(  7,  50),   S( 27,  74),   S( 22,  71),   S( 16,  59),   S(  1,  36),   S( 24,  83),   S( 23,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -26),   S(  3, -58),   S(-26, -56),   S(-10, -26),   S(-12, -27),   S(-14, -38),   S( -8, -55),   S(  3, -88),
            S(-23, -64),   S(-22, -100),  S(-17, -105),  S(-10, -35),   S(-20, -23),   S( -7, -28),   S( 11, -54),   S( 12, -106),
            S(-27, -45),   S(-32, -63),   S(-45, -52),   S(  6, -37),   S(-32, -38),   S( -7, -71),   S(  3, -48),   S(  6, -44),
            S( 10, -36),   S(-26, -17),   S( -2,  42),   S(-20,  15),   S( 11,   9),   S( -9, -18),   S( -6, -11),   S( -7,  34),
            S(  7,  46),   S(  1,  51),   S(  1,  71),   S( 12,  62),   S( 24,  82),   S( 12,  64),   S( 17,  60),   S( 10,  23),
            S(-23,   4),   S( -7,   3),   S( 11,  75),   S( 21,  57),   S( 21,  71),   S( 18,  58),   S( 11,  35),   S( 16,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-73, -28),   S(-31, -28),   S(-22,  -6),   S(-19,  20),   S(-28, -26),   S(-32,  -7),   S( -9, -25),   S(-75, -38),
            S( 14, -37),   S( -4,   3),   S(-27, -32),   S( -7, -12),   S(-14,  -8),   S(-13, -22),   S(-34, -49),   S(-21, -38),
            S(-18, -22),   S( 17, -31),   S( -1,   4),   S( 26,  22),   S(-13,   8),   S(  3,  -4),   S(-33,  21),   S(-26, -34),
            S( 10,  20),   S( 37,  46),   S( 25,  29),   S( 43,  14),   S( 28,  16),   S( 13,  22),   S( 36, -18),   S( -8, -17),
            S( 60,  44),   S( 22,  53),   S( 54,  60),   S( 63,  37),   S( 66,  29),   S( 12,  23),   S( 15,  -6),   S(  6,   2),
            S(102, -31),   S( -8,  53),   S(143,  -1),   S( 71,  38),   S( 52,  39),   S(-36,  58),   S( 34, -10),   S(-19,   5),
            S( 57,  -1),   S( -1, -21),   S( 51,  18),   S( 84,  65),   S( 40,  24),   S(  1,  32),   S(-13,   9),   S(-47,   6),
            S(-108, -118), S( -1,   0),   S(  7,   2),   S( 20,  25),   S(  4,  33),   S( 20,  15),   S(-30,   0),   S( -4,  16),

            /* knights: bucket 1 */
            S( 22,   3),   S(-61,  17),   S(-31,   9),   S(-48,  27),   S(-28,  33),   S(-25, -25),   S(-31,  -4),   S(  5, -15),
            S(-35,  39),   S(-50,  56),   S(-29,  23),   S(-13,  18),   S(-24,  18),   S(-10,  22),   S(-10,  -4),   S(-12, -49),
            S(-37,  29),   S( -3,  -1),   S(-22,  14),   S(-13,  48),   S(-17,  33),   S(-10,   6),   S(-43,  29),   S(-13,  24),
            S(-18,  72),   S( 30,  30),   S( -7,  50),   S( -4,  57),   S( -6,  52),   S(-10,  53),   S( -1,  19),   S(-25,  53),
            S( 66,  -1),   S(  7,  21),   S( 43,  56),   S( 20,  48),   S( 41,  47),   S( -6,  66),   S( -9,  49),   S( -3,  62),
            S( 31,  23),   S( 62, -12),   S( 81,  17),   S( 96,  28),   S( 70,  22),   S(-33,  72),   S( 18,  27),   S(  3,  41),
            S( 23,   1),   S( 38,  -7),   S( 34, -16),   S( 26,  48),   S( 15,  35),   S(  3,  19),   S( 17,  72),   S(-28,  49),
            S(-140, -23),  S( 16, -17),   S(-33, -59),   S(-17,  11),   S( -4,  10),   S( 41,  48),   S( 20,  50),   S(-56,  34),

            /* knights: bucket 2 */
            S(-63,  17),   S(-38,  26),   S(-29,   1),   S(-24,  16),   S(-23,  13),   S(-54,  -3),   S(-29,   4),   S(-13, -18),
            S(-15,  11),   S( -5,  35),   S(-23,   7),   S(-18,  15),   S(-29,  22),   S(-19,   5),   S(  8,   6),   S(-29,   8),
            S(-32,  52),   S(-21,  19),   S(-24,  16),   S(-25,  54),   S(-23,  41),   S(-23,   7),   S(-26,  13),   S( -3,  -6),
            S(-11,  58),   S( -5,  41),   S(-27,  73),   S(-17,  74),   S(-36,  70),   S(  2,  47),   S(  8,  30),   S( -4,  37),
            S( -8,  65),   S(-18,  66),   S(  6,  64),   S( 17,  58),   S(  2,  65),   S( 19,  66),   S( -6,  60),   S( 23,  15),
            S(-39,  70),   S(-20,  49),   S(-14,  78),   S( 43,  26),   S( 47,  27),   S(130,  -7),   S( 69,  10),   S( 31,  -6),
            S( 36,  42),   S(-38,  58),   S( 48,  18),   S( 33,   5),   S( -1,  40),   S( 17, -10),   S( 39,  22),   S( 32,  -4),
            S(-48,  42),   S( 32,  67),   S(-12,  68),   S( -8, -23),   S(-19, -11),   S(-28, -42),   S( 18,   1),   S(-110, -41),

            /* knights: bucket 3 */
            S(-51,  28),   S(-14, -49),   S( -1, -24),   S( -2, -17),   S(  1, -17),   S( -8, -33),   S(-21, -28),   S(-22, -70),
            S(-13, -25),   S( -1,  -1),   S(  8, -14),   S( -4,  -3),   S( -6,  -3),   S( 19, -21),   S( 23, -35),   S( 23, -52),
            S(-11,  -1),   S(-11,   5),   S(  0,  17),   S(  4,  38),   S( 10,  25),   S( -1,  13),   S(  9,   0),   S( 20, -34),
            S(  9,   2),   S( 16,  26),   S( 18,  42),   S( 11,  52),   S( 13,  66),   S( 28,  52),   S( 32,  46),   S( 15,  38),
            S( -2,  45),   S( 23,  31),   S( 25,  51),   S( 29,  74),   S( 31,  70),   S( 34,  79),   S(  8,  90),   S( 63,  78),
            S( -9,  33),   S(  7,  41),   S( 12,  52),   S( 20,  68),   S( 59,  67),   S(141,  62),   S( 59,  83),   S( 23, 105),
            S(-21,  46),   S(-16,  53),   S(-19,  62),   S( 29,  57),   S( 43,  61),   S(104,  39),   S( 11,  -1),   S( 91,  21),
            S(-136,  51),  S(-30,  83),   S(-46,  86),   S( 36,  48),   S( 64,  74),   S(-46,  69),   S(-25, -40),   S(-53, -100),

            /* knights: bucket 4 */
            S(  9,  11),   S(-11, -11),   S(-62,  16),   S(-37, -15),   S(-40,  20),   S(-21, -16),   S( 18, -29),   S(-18, -13),
            S( 21,  39),   S(  3, -21),   S( -5,   3),   S(-21,   5),   S( -9, -16),   S(  8, -47),   S(-12,  10),   S(-43,  -2),
            S( -3, -19),   S( 30,  -8),   S( 52,  -1),   S( 65,   1),   S( 12,  16),   S( 33, -32),   S(-11, -26),   S( -7, -31),
            S(-17, -24),   S( 29,   0),   S( 50, -17),   S( 71,  -3),   S( 35,   8),   S( -3,  25),   S(-32,  26),   S( -6,  12),
            S(  6, -37),   S( 35,  -9),   S( 66,  10),   S( 35,  37),   S( 53,   1),   S( 16,  17),   S( 24,  -7),   S(-29,  45),
            S( -5, -24),   S(  2,  -1),   S( 40, -24),   S( 57,  15),   S(  4,  16),   S(-21,  35),   S(-18,   4),   S( 22,   6),
            S(-16, -28),   S(-20,  -7),   S(  4,  -5),   S( 23,  19),   S( 25,   9),   S( -3,  10),   S( 11,  40),   S(-33,  -9),
            S(  4,  16),   S(-11, -35),   S( -6, -29),   S( 17,   4),   S( 14,  21),   S( -3,  18),   S( -4,  20),   S(-16, -12),

            /* knights: bucket 5 */
            S( 21,  33),   S(  9,  28),   S(-37,  33),   S(-20,  22),   S(-20,  29),   S(  5,  16),   S(-16,  18),   S( 11,  32),
            S( 19,  30),   S( 28,  28),   S(  1,   4),   S(-24,  12),   S( 27, -11),   S(-32,  14),   S(-13,  41),   S(-49,  21),
            S(-29,  27),   S( -8,   2),   S( 33,   4),   S( 37,   9),   S( 21,  14),   S(-19,  21),   S( -8,  12),   S(-48,  19),
            S( 34,  17),   S( 34, -20),   S( 58,  -5),   S( 88, -20),   S( 85,   1),   S( 77,   5),   S( -4,  21),   S( 12,  36),
            S( 43,   2),   S( 40, -10),   S( 95, -15),   S(134, -12),   S( 92, -17),   S( 44,  14),   S(  5,   7),   S( 17,  26),
            S( -1, -24),   S( 38, -31),   S(  5, -28),   S( 14,  14),   S( 26,  -3),   S( 48,  -4),   S( -6,  10),   S( 27,  34),
            S(  2,   7),   S(-26, -55),   S(  2, -48),   S(-10, -18),   S( -6, -36),   S(  5,   4),   S( -4,  43),   S( 19,  39),
            S(-19, -26),   S(-26, -62),   S( 10,  -8),   S(-23, -27),   S(  7,   1),   S(  0,  33),   S( 22,  42),   S(  1,  29),

            /* knights: bucket 6 */
            S( -5,  -2),   S(-53,  26),   S(-29,   7),   S(-49,  36),   S(-52,  34),   S(-24,  36),   S(-15,  42),   S(-31,  16),
            S(  7,  -9),   S(-18,  49),   S(-21,   2),   S( 13,   8),   S( 12,  16),   S(-44,  35),   S(-21,  50),   S(-37,  72),
            S(-10,  20),   S( 11,  16),   S(  4,  21),   S( 27,  30),   S( 33,  25),   S(-29,  32),   S( 19,  27),   S(-21,  43),
            S(  8,  51),   S( 54,   5),   S( 42,  21),   S( 70,   9),   S( 89, -11),   S( 76,   6),   S( 28,  14),   S(-11,  49),
            S( -8,  44),   S( 37,  11),   S( 90,   3),   S(121,  -3),   S(112, -19),   S( 71,  20),   S(127, -20),   S( 20,  28),
            S( 10,  21),   S( 24,   5),   S( 58,  11),   S( 43,   1),   S( 54,  -6),   S( 40,  -5),   S( 14, -13),   S( 29,   1),
            S(  4,  34),   S( 13,  35),   S( 35,  35),   S( -1,  -7),   S( 28, -14),   S( 22, -38),   S( -6,  -3),   S( 11,  42),
            S( 20,  44),   S(  2,  33),   S( 17,  36),   S(  5,  21),   S(  9,  -6),   S( -6,  -2),   S( 11,  26),   S(-20, -27),

            /* knights: bucket 7 */
            S(-32, -36),   S(-24, -44),   S( -6, -15),   S(-49,  19),   S(-12,  -3),   S(-45,   8),   S(-16, -10),   S(-13,  29),
            S(-32, -50),   S(-12, -26),   S(-44,  -6),   S(-40,  -2),   S( -8,   8),   S( -3,  21),   S(-11,  15),   S(-62,  40),
            S( -6, -35),   S(-46, -19),   S(  3, -17),   S(  0,  18),   S( 45,  12),   S( 39,   4),   S( 25,  12),   S( -2,  31),
            S(-40,  16),   S(  5,  -4),   S( 53, -20),   S( 84,  -2),   S(107, -12),   S( 87,  11),   S( 72,   1),   S( 67,   6),
            S(  1,   8),   S( -7,  12),   S( 16,  15),   S( 79,  -6),   S(106,  -1),   S(155, -29),   S(192, -17),   S( 39,  -4),
            S(-19,  15),   S( 23,   8),   S( -6,   6),   S( 51,  11),   S( 99,  -7),   S( 98, -11),   S( 18,  -9),   S(  5, -40),
            S(-19,   6),   S(-11,   6),   S( -7,  13),   S( 25,  20),   S( 55,  13),   S( 30,  21),   S(-12, -33),   S(-15, -36),
            S(-27, -33),   S( -9,  10),   S( -2,  25),   S(  4,  21),   S( 14,  11),   S( 19,  13),   S(  5,  -5),   S(  2,  -7),

            /* knights: bucket 8 */
            S( -1,   6),   S( 12,  29),   S( 12,  27),   S(-10, -31),   S( -2,  22),   S( -5, -19),   S( 13,  24),   S( -2, -12),
            S( -6, -23),   S( -5, -23),   S( -8, -40),   S(-11,   2),   S( -7,  32),   S( -1,  -5),   S(  0,  -5),   S( -3,  -3),
            S(-11, -40),   S( -8, -27),   S(  1, -51),   S(  3,   6),   S( -9, -24),   S( 12,   7),   S( -3,  -3),   S( -1, -13),
            S(-17, -52),   S( -9, -32),   S(  9,  14),   S(  1,   7),   S(-18, -17),   S(-24, -12),   S(-21, -28),   S(-15, -35),
            S( -6, -24),   S(  5, -18),   S( -1, -21),   S( -1,  -9),   S(-16,   1),   S(-11, -13),   S(  4,   0),   S( -1, -10),
            S( -3,   8),   S( 12,  -3),   S( -2,   3),   S( -6, -13),   S( -8,  -4),   S( -5, -12),   S( -9,  -4),   S( -6, -17),
            S(  0,  16),   S( -1, -25),   S(-12, -20),   S(  5,  14),   S(  2,   3),   S(  0,   0),   S( -3,   5),   S( -2, -16),
            S(  1,   2),   S( -3,   7),   S( -5,   3),   S(  3,   0),   S( -1,   8),   S( -1,  -4),   S(  0,   5),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-18, -58),   S( -5,  -1),   S( -2, -32),   S( -5, -34),   S(-16,  -9),   S(-12,  11),   S(  5,  21),   S(  2,  -6),
            S( -4,   6),   S(-14, -43),   S(-21, -108),  S(-26, -66),   S(-10, -35),   S(-22, -56),   S(-11,  -1),   S(-12,   2),
            S( -9, -19),   S(-16, -42),   S(-12, -41),   S( -4, -60),   S(-21, -13),   S( 10,   5),   S(-12,  -5),   S( -3,   7),
            S(-18, -48),   S(-11, -44),   S( -8, -28),   S( -9, -40),   S(-14, -32),   S(  4,  -2),   S(-16, -39),   S(  3,  13),
            S(  3,  25),   S( -8, -25),   S( -2, -23),   S( -1, -32),   S(-10, -26),   S( -3,  10),   S( -9, -11),   S( -4,   2),
            S(-13, -21),   S(-18, -33),   S(-11, -24),   S( -4, -18),   S(  0,  15),   S( -8,   0),   S( -3,  19),   S(  0,  10),
            S(-10, -15),   S(  0,  17),   S(-12,  -6),   S(-23, -19),   S(  2,   5),   S(  1,  23),   S( -7,  18),   S( -6,   5),
            S(  4,   3),   S(  4,   3),   S( -1,  11),   S(  0,   8),   S(-10,  -5),   S( -4,   4),   S(  3,  11),   S(  0,  16),

            /* knights: bucket 10 */
            S( -8, -28),   S( -5,  12),   S(-10, -10),   S(-11,  17),   S(-20, -44),   S(  9, -16),   S( -3,  12),   S( -1,  20),
            S( -4, -16),   S(  6,   3),   S(-14, -23),   S(-10, -46),   S( -8, -32),   S(-25, -57),   S( -7,  16),   S(  3,  31),
            S( -3,  -3),   S( -4,  -9),   S( -7, -20),   S(  6, -50),   S(-26, -44),   S( -3, -23),   S( -9, -34),   S( -9,  12),
            S( -9, -14),   S(-11, -23),   S( -7, -16),   S( -2, -24),   S(-10, -17),   S( -4,  -5),   S( -8, -48),   S( -3,  -3),
            S(-12, -18),   S(-12, -30),   S( -9,  -5),   S( -6, -14),   S(  5,  -5),   S( -7, -38),   S( -3,  -8),   S(  5,   9),
            S( -1,  15),   S(-12,   2),   S(-10,   8),   S(-13,  16),   S(-15, -19),   S(-19, -16),   S(-13,  -4),   S(-17,  -4),
            S(  3,  11),   S( -3,  -1),   S( -6, -26),   S( 12, -20),   S( -6,   3),   S(-17, -46),   S( -8,   8),   S( -9, -10),
            S( -1,   4),   S( -1,  11),   S( -1,  18),   S( -4,   6),   S( -4,   6),   S( -6,  -9),   S(  6,  12),   S(  2,  10),

            /* knights: bucket 11 */
            S( -3, -15),   S(-26, -26),   S( -4,  -5),   S(  4,  21),   S(-38, -32),   S(  0,  15),   S( -6,   8),   S(  9,  34),
            S( -6, -14),   S(-28, -42),   S(-13, -42),   S( 15,  -3),   S(  8,  18),   S( -3, -27),   S(-13, -22),   S( -8, -11),
            S(-13, -38),   S(-20, -20),   S( -2, -15),   S(  2,  -8),   S( -8,  14),   S( 17,  -6),   S(  0, -14),   S( -4,  -5),
            S(-14, -10),   S(  5, -20),   S( -2, -25),   S( 12,  -1),   S( 29,  -7),   S(  1, -20),   S( 14,  20),   S(  0,  -5),
            S(-16,   3),   S(  2, -38),   S(-18,  -2),   S(  2, -16),   S( 32,   9),   S(  5,  17),   S( -8, -66),   S(-10, -10),
            S( -8, -21),   S( -7, -43),   S(  3,   7),   S(  9,  -2),   S(  9,  31),   S( -8, -13),   S( -4, -28),   S( -1,  20),
            S(  0,  -6),   S( -8,  19),   S(-11, -10),   S(  6,  -3),   S( 13,  -3),   S(  3, -17),   S(  1, -16),   S( -4,   2),
            S( -3, -15),   S(  2,   6),   S( -3,  -8),   S(  2,  17),   S( -4,  -8),   S(  0,  -6),   S(  5,  17),   S( -1,  -3),

            /* knights: bucket 12 */
            S(-14, -40),   S( -3, -10),   S( -1, -18),   S(  0,   8),   S( -4,   7),   S( -5, -11),   S( -1,   6),   S( -1,   1),
            S( -3,  -9),   S(  0,   3),   S(  0, -17),   S( -3,   4),   S( -4,  -9),   S(  0,   4),   S(  1,  -1),   S(  0,  -7),
            S( -2, -10),   S( -6, -23),   S( -6, -20),   S(-15, -25),   S( -8,  -5),   S( -3,  25),   S( -3,   1),   S( -5,  -9),
            S(  2,  10),   S( -2, -36),   S( -7,  25),   S(  3,  12),   S( -4, -11),   S(  3,  22),   S(  5,  12),   S(  2,   8),
            S(  0,   2),   S( -4,  -8),   S( -5, -21),   S( -4,  -7),   S(  0,   5),   S( -3,   4),   S( -6,  -3),   S( -8,  -7),
            S( -5,  -2),   S( -1,  -3),   S( -3, -14),   S( -3, -11),   S( -3,  -1),   S( -7, -20),   S(  7,   7),   S( -1,   8),
            S( -4,  -8),   S( -2,  -1),   S(-10,  -1),   S( -2,  -6),   S(  0,   9),   S( -9,  -8),   S( -5, -18),   S( -3,  -2),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -6),   S(  1,   3),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -7),   S( -4, -13),   S( -3, -16),   S( -2,  -5),   S( -3, -12),   S( -2,   7),   S( -6,  -5),   S(  3,  11),
            S( -2,   8),   S( -2,  -1),   S(  3,   9),   S( -4,  -2),   S( -6, -11),   S( -1,   7),   S(  1,  19),   S( -3,  -5),
            S(  5,  -2),   S(  5,   8),   S(  5,   1),   S( -4, -25),   S(  4,  22),   S( -6,   7),   S(  7,   4),   S( -2,  -1),
            S(  0,  12),   S(  0,   4),   S( -6,  -4),   S(  1,  27),   S(  0,  10),   S( -2,  27),   S(  1,   7),   S( 10,  19),
            S(  1,  21),   S( -2, -16),   S( -4,   9),   S( -7,   7),   S(-16,  -1),   S( -3,  25),   S( -8, -24),   S( -3,  -2),
            S( -4,  -5),   S(  2,   1),   S( -4,  10),   S(  3,  12),   S( -8,   6),   S( -8,   4),   S(  3,  21),   S(  1,   3),
            S(  1,   4),   S(  3,   8),   S( -6,  -4),   S( -4,   2),   S( -2,   7),   S( -3,  -7),   S(  2,   7),   S( -1,   1),
            S(  2,   6),   S(  1,   2),   S( -2,  -3),   S(  2,   4),   S(  0,   1),   S(  1,   2),   S( -1,  -2),   S(  0,   2),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   3),   S(  5,  17),   S( -2,   0),   S( -6, -24),   S( -1,  19),   S(  2,   3),   S(  0,   3),
            S( -2,  -9),   S( -8, -15),   S(  2,  -4),   S( -1,  -2),   S(  3,   1),   S(  0,   4),   S( -7,   5),   S(  6,  57),
            S( -1,   0),   S( -5, -33),   S(  6,  16),   S(-10, -34),   S( -3,  -3),   S(  1,   7),   S( -1,   8),   S(  3,  17),
            S( -1,  -3),   S( -3, -17),   S(-22, -15),   S( -2,  43),   S(  3,  41),   S( -5, -10),   S(  0,   5),   S(  1,  35),
            S(  6,  16),   S(-17, -34),   S( -9,  -9),   S( -8,   4),   S(  0,  31),   S(-11,   4),   S( -3,   0),   S(  4,  13),
            S( -1,   3),   S(  5,   6),   S(  3,  -4),   S( -3,  13),   S(  1,  16),   S(  1,  13),   S(  1,   8),   S( -5, -11),
            S(  0,   4),   S( -3,  -1),   S(  3,  17),   S(  6,   4),   S(  4,  13),   S( -4, -10),   S(  2,   7),   S(  4,   6),
            S(  0,  -1),   S(  0,   1),   S( -1,  -1),   S(  2,   4),   S( -1,   1),   S(  0,  -2),   S(  1,   0),   S(  1,   2),

            /* knights: bucket 15 */
            S( -2, -13),   S( -1,   4),   S(  4,  24),   S( -2,   4),   S( -4, -16),   S(-10, -35),   S( -4, -15),   S( -1, -10),
            S(  2,  -1),   S(  4,   5),   S( -6,  -7),   S(  9,  42),   S(  0,  12),   S( -8, -34),   S( -3,  -2),   S(  1,   2),
            S(  0,  -5),   S( -5, -20),   S(  1, -11),   S(  5,   8),   S(-17, -29),   S( -1,  -3),   S( -2,  -6),   S( -2,  -1),
            S(  0,  -8),   S( -3,   4),   S( -5, -13),   S( -5,   5),   S( -7,   3),   S( -9,  25),   S(  5,   5),   S( -1,   1),
            S( -1,  -1),   S(  9,  21),   S( -5,   6),   S( -7,   5),   S( 18,  33),   S(  0,  15),   S(  6,  -4),   S(  4,  17),
            S(  1,   4),   S( -4,  -8),   S( -1,   0),   S( -9, -18),   S( -6, -10),   S(  1,  16),   S(  0,   9),   S(  5,  13),
            S( -1,   1),   S( -3,  -7),   S(  4,  17),   S(  3,   4),   S(  3,  13),   S(  6,   9),   S(  1,   8),   S(  4,   9),
            S(  1,   4),   S( -1,  -5),   S(  0,   0),   S( -1,   0),   S(  2,  10),   S(  1,   1),   S(  0,   0),   S(  1,   3),

            /* bishops: bucket 0 */
            S( 38,  14),   S( -9,  37),   S(-17,   9),   S(-25,  -8),   S( -7,  -1),   S( -4,  10),   S( 65, -42),   S( 31,   5),
            S(-28,  -9),   S( -3,  -2),   S(-25,  31),   S( -1,   8),   S( -1,  16),   S( 47, -10),   S( 35,  39),   S( 44, -14),
            S( 14,  11),   S(  0,  20),   S( 11,  12),   S(  4,   6),   S( 19,  16),   S( 34,  36),   S( 32,   0),   S( 25,   1),
            S( 16, -27),   S( 34, -42),   S( 13,  10),   S( 33,  10),   S( 62,  31),   S( 30,  41),   S( 12,  15),   S(  8,  24),
            S( 39, -16),   S( 42, -20),   S( 54,   3),   S( 78,  37),   S( 87,  18),   S( 15,  40),   S( 28,  43),   S( -3,  15),
            S( 51,  16),   S( 53,  40),   S(110,  21),   S( 54,  -6),   S( 15,  37),   S( 12,  54),   S( 35,  28),   S( -4,  12),
            S(-42, -76),   S( 85,  45),   S( 83,  78),   S( 18,  -4),   S( 10, -11),   S( 21,  28),   S(-22,  38),   S(-16,  53),
            S(-18, -35),   S( -5,  -9),   S( 12, -27),   S(-16, -15),   S(-14, -16),   S(-19,   6),   S(-18,  20),   S(-22, -20),

            /* bishops: bucket 1 */
            S(-52,  34),   S( -6,  -1),   S(-24,  36),   S( 14,  -8),   S(-21,  21),   S(  2,   4),   S( 34, -13),   S( 35,  -9),
            S(  3, -31),   S(-16,   4),   S( -8,  -8),   S(-18,  14),   S( 24, -10),   S(  2,   0),   S( 50, -18),   S( 15,  -9),
            S(-11,   2),   S( 22, -13),   S(-18,  10),   S( 14,   2),   S( -1,  -4),   S( 30, -10),   S(  9,  -5),   S( 63,  -2),
            S( 22, -15),   S( 49, -20),   S( 24,  -2),   S( 20,   7),   S( 34,  -4),   S(  7,   9),   S( 49,  -7),   S(  0,  15),
            S( 26, -12),   S( 53, -17),   S( 16,   1),   S( 91, -20),   S( 48,  16),   S( 36,  16),   S( -5,  21),   S( 29,   3),
            S( 59, -43),   S( 45,   6),   S( 66, -11),   S( 66, -16),   S( 71,  -3),   S(-38,  29),   S(-30,  51),   S(-31,  20),
            S( 16, -63),   S(  7, -36),   S(-10,  -3),   S( 23,  43),   S( 24,  30),   S(-17,  26),   S(-17,  22),   S(-27,  37),
            S(  2,  -5),   S(-14,   5),   S( -7, -22),   S(-49,  -3),   S(-24,  17),   S( 18,   0),   S( 27,   1),   S(-46,  20),

            /* bishops: bucket 2 */
            S(  7,   4),   S(-13,  -6),   S(  0,  14),   S(-22,   5),   S( 10,  11),   S(-21,   5),   S( 16,  -9),   S(  4,  -2),
            S( 24, -20),   S(  8, -12),   S( -7, -10),   S(  5,  10),   S(-14,   9),   S(  5,   1),   S(  3, -15),   S( 17, -50),
            S( 43,   4),   S( 19,  -5),   S( -4,  17),   S(-10,   5),   S( -6,  25),   S(-14, -11),   S(  2, -27),   S(-11,  -2),
            S(-14,   4),   S( 43,  11),   S( -5,  13),   S( 23,  25),   S( -3,  11),   S( -5,  17),   S(-13,  -4),   S(  6,  12),
            S(  0,  19),   S(-33,  37),   S( 46,  17),   S( 18,  24),   S( 19,  24),   S( 23,   3),   S(  9,  28),   S( 41,  -9),
            S(-33,  37),   S( -9,  36),   S(-29,   7),   S( 85,  -6),   S( 48,   9),   S(101,  -1),   S( 73,   8),   S( 41, -46),
            S(-37,  64),   S(-32,  17),   S( -9,  17),   S(  3,  11),   S(-44, -11),   S(-36,   9),   S(-28,  11),   S(  0, -40),
            S(-73,   3),   S(-13,  25),   S(  0,   9),   S(-19,  27),   S(-31, -12),   S(-32,   9),   S(  0, -14),   S(-48,   0),

            /* bishops: bucket 3 */
            S( 44,   3),   S( 39, -19),   S( 19, -25),   S( 10,  -5),   S( 16,   8),   S( -3,  26),   S(-12,  48),   S(  7,  -2),
            S( 41,   5),   S( 28, -13),   S( 19,  -5),   S( 21,   1),   S( 17,  15),   S( 21,   5),   S( 13,  -3),   S( 35, -42),
            S( 16,  -7),   S( 32,  30),   S( 23,  26),   S( 16,  23),   S( 15,  25),   S( 12,  16),   S( 19, -13),   S( 14,  11),
            S( -6,  12),   S( 10,  38),   S( 23,  45),   S( 32,  42),   S( 33,  16),   S( 27,   0),   S( 27,  -6),   S( 36, -36),
            S(  8,  31),   S( 14,  48),   S(  2,  51),   S( 51,  42),   S( 46,  39),   S( 49,  16),   S( 28,  12),   S(  3,  11),
            S(  5,  32),   S( 21,  51),   S(  4,  35),   S( 13,  36),   S( 50,  34),   S( 83,  54),   S( 46,  37),   S( 44,  71),
            S(-25,  75),   S(  3,  44),   S( 10,  24),   S( -5,  52),   S( 22,  31),   S( 53,  44),   S(-18,  35),   S( 24, -22),
            S(-33,  34),   S(-33,  52),   S(-50,  41),   S(-33,  49),   S( 16,  10),   S(-60,  30),   S( 18,   6),   S( 26,  21),

            /* bishops: bucket 4 */
            S(-27,  19),   S(-31,   7),   S(-39,  14),   S(-57,  13),   S(-32, -12),   S(-19,  -7),   S(-13, -20),   S(-31, -28),
            S( -7,   4),   S( -1,  -3),   S( 58, -34),   S(-34,  13),   S(-55,  22),   S(-11, -32),   S(-20, -15),   S(-29, -21),
            S(  8,  21),   S(-11, -21),   S( 10,  12),   S( -5,   0),   S( 12, -12),   S(-59,  20),   S(-21, -33),   S(-53, -14),
            S( 29,  -2),   S( 50, -20),   S( 31,   7),   S(  9,  24),   S(-11,  20),   S( 21,  -3),   S(-46,   3),   S( -7, -21),
            S( 15, -12),   S(-10, -22),   S( 40, -14),   S( 16,   2),   S( -5,  25),   S( 20,   6),   S(-25,  35),   S(-56,   3),
            S(-51, -81),   S(-47,  -5),   S( -1,  17),   S(  6,   1),   S(-48,  40),   S( 12,  24),   S(-15,  26),   S( -5,  30),
            S( -1,   3),   S(-19,  12),   S( -1, -21),   S(-29, -15),   S(  0, -25),   S( 34,   0),   S(  1,   6),   S( 17,  33),
            S( -2,   3),   S(  0, -18),   S(-14, -10),   S(  1, -18),   S(-18,   3),   S(  3,  17),   S(  3,  44),   S( 11,  14),

            /* bishops: bucket 5 */
            S(-40,  14),   S( 17,  -6),   S(-48,  21),   S(-56,  22),   S(-18,   4),   S(-60,  17),   S(-35,  21),   S(-40,   5),
            S( -9,  -5),   S(-30,  15),   S( 19,  -5),   S(-17,  15),   S(-59,  29),   S(-28,  20),   S(-33,  14),   S(  7,  -8),
            S( 13,  29),   S(-19,  -1),   S( 24,  -2),   S(  0,   6),   S(-11,  17),   S(-59,  22),   S(-18,  17),   S(-17,  28),
            S( 18,   6),   S(  5,  11),   S( 68, -21),   S( 38,  10),   S(-11,  23),   S(  6,  17),   S(-59,  31),   S(-22,  21),
            S( 14,  -6),   S( 33,  -6),   S( -7,   8),   S( -7,  -2),   S( -1,   7),   S( -6,  11),   S(  6,  19),   S(-49,  21),
            S(  4, -12),   S(-36,  15),   S( 20,  -8),   S(-11, -17),   S(-13,   4),   S(-11,   8),   S(-25,  18),   S(-37,  50),
            S(-20, -11),   S( -3,   1),   S(-17,  -6),   S(  3,  20),   S( 15,   2),   S(-11,  24),   S(  2,  27),   S(-21,  35),
            S( -8,  15),   S(-10, -19),   S( -1, -18),   S(-20,  -4),   S(-24,  27),   S(  7,  35),   S(-19,  25),   S( 20,  29),

            /* bishops: bucket 6 */
            S( -4,  -9),   S(-11,   7),   S(-38,  22),   S(-22,  12),   S(-63,  34),   S(-35,  19),   S(-41,  30),   S(-51,  16),
            S(-36,  17),   S(-31,  -5),   S(-59,  40),   S(-45,  30),   S(-47,  26),   S(-48,  18),   S(-39,  26),   S(-31,  14),
            S(  1,   6),   S(-39,  15),   S( -5,   9),   S(-34,  29),   S(-25,  31),   S(-19,   8),   S(-10, -11),   S( -5,  24),
            S(-65,  30),   S(-52,  28),   S(-11,  14),   S( 18,  35),   S( 12,  30),   S( 18,   9),   S( 22,   1),   S(-12,  21),
            S(-44,  25),   S(-30,  28),   S(  7,   8),   S( 59,  10),   S(-13,  15),   S( -9,   6),   S( 14,  11),   S(-19,  -2),
            S(-49,  43),   S(-20,  20),   S(-45,  18),   S(-16,  11),   S( 14,  10),   S( -5,   5),   S( -2,  17),   S(-26,  -1),
            S(-15,  37),   S(-73,  52),   S(-29,  15),   S(-20,  21),   S( -7,   3),   S(  5,   1),   S( 13,   6),   S(-23,  13),
            S( -6,  28),   S(-26,  36),   S( -9,  32),   S( 27,   9),   S(-26,  21),   S( 18, -16),   S(-11,  10),   S( -4,  32),

            /* bishops: bucket 7 */
            S( -4, -33),   S(-51,  -9),   S(-39, -20),   S(-15, -14),   S(-40,  -6),   S(-40,  -9),   S(-64, -20),   S(-40,   1),
            S( -9, -48),   S( -6, -30),   S( 13, -25),   S(-26, -15),   S(-32,  -3),   S(-46,   0),   S(-37, -15),   S( -3, -15),
            S(-45, -23),   S(-32,  -1),   S(-13,  -5),   S(  9, -13),   S(  0,  -8),   S( -1, -21),   S(-51,   0),   S(-52,   8),
            S(-22, -24),   S(-63,  25),   S(-28,   7),   S(-14,  17),   S( 87,  -6),   S( -4,   8),   S( 40, -34),   S(-11,  -9),
            S(-23,   0),   S( 16, -14),   S(-46,  25),   S( 10,  -1),   S( 53, -12),   S( 49,   4),   S(-19,  10),   S(-32,  -9),
            S(-72,  34),   S(-42,  48),   S(-18,  12),   S(-80,  27),   S(-29,  14),   S( 20,  -2),   S(  4,  35),   S(-48, -77),
            S(-12,  -5),   S(-26,  19),   S(-48,  17),   S( -7,   5),   S(  1,  -5),   S( 22, -27),   S( 18, -13),   S(  4, -10),
            S(-10,  -9),   S( -5,   6),   S(-11,  11),   S( -2,   6),   S( -9,   0),   S( 11, -17),   S( 26, -30),   S(  7,   9),

            /* bishops: bucket 8 */
            S( 40,  70),   S( -2, -39),   S( -3,  -3),   S(-11,  39),   S(  0,  18),   S( -8, -40),   S(-16, -27),   S(-10, -16),
            S(  0,  -3),   S( 20,  40),   S( 20,   4),   S(  6,  17),   S( -1, -21),   S(  1,  -1),   S(-27, -37),   S(-10,   2),
            S( -7,  -6),   S(-15, -15),   S( 30,  37),   S(  9,   7),   S(  5,  11),   S(  5,  16),   S(-26, -15),   S(-34, -28),
            S( -5, -12),   S( 29,  22),   S( -3,  22),   S( 23,   6),   S(  4,  31),   S( 10,  25),   S(-13,   6),   S(  3, -18),
            S( 14,  17),   S( 47,  54),   S( 17,  -6),   S( -8,  19),   S(  8,  19),   S(-25,  19),   S( -8, -27),   S(  4,  18),
            S( -8,  -7),   S(  0,   4),   S( 16,  36),   S( 23,  10),   S( 10,  28),   S( 34,  20),   S( -9,  58),   S( -2,  32),
            S(  2,  15),   S(-13, -35),   S( 25,  -4),   S( 23,  -3),   S(  8,  -3),   S( 20,  42),   S( 25,  40),   S(-13,  -3),
            S( -5,   0),   S(  5,   4),   S(  1,  18),   S(  1,  10),   S( 28,   1),   S( 22,  11),   S( 14,  36),   S( 40,  35),

            /* bishops: bucket 9 */
            S( 11,  40),   S(  4,  10),   S( -3,  -5),   S(-31, -31),   S(-21, -12),   S( -8,  -9),   S( -2,  -2),   S( -5,   4),
            S( -1,  -5),   S( 10,   3),   S(  4,   9),   S(-33,   0),   S(-29,   8),   S(-12, -16),   S(-31,  -2),   S(-17, -28),
            S( -9,   1),   S( 15,   3),   S(  3, -10),   S(  2,  18),   S( 10,  10),   S(-23,  -1),   S( -4,   4),   S(-11,  -7),
            S( -2,  21),   S( -2, -14),   S( 27,  -1),   S( 24,   1),   S( -4,  19),   S(-12,  10),   S(  2,  17),   S( -5,  15),
            S( 25,  15),   S( 17,   9),   S( 25,  18),   S( 15, -22),   S( 10,  27),   S( -3,  29),   S(  4,  31),   S(-16, -19),
            S( 17,  19),   S( -8,  25),   S( 15,   0),   S( 10,  13),   S( 40, -45),   S(  1,  29),   S( 14,  29),   S( 11,  27),
            S( 12,  10),   S( -6,  26),   S(  5,   8),   S( 18,  -5),   S( 21,  -3),   S( 31,  13),   S( 23,  46),   S( 17,  55),
            S( 16,  48),   S(  1, -22),   S(  2,  22),   S( 10,  17),   S(  7,  40),   S( 17,  -5),   S( 25,  -3),   S( 34,  35),

            /* bishops: bucket 10 */
            S(  4, -20),   S( 11,   9),   S( -3, -24),   S(-26, -24),   S(-67, -21),   S(-34, -62),   S(  7,  -9),   S(  0,  25),
            S(-10,  18),   S(  1, -39),   S(-10, -21),   S(-23, -40),   S(-49,   1),   S(-31, -26),   S(-26,  -3),   S(  1,   0),
            S(-11, -35),   S(-21, -23),   S( -9, -13),   S( -7,  22),   S(-17,   3),   S( -6, -17),   S( -9,  -1),   S( -7, -22),
            S(-17,  11),   S(-25,  -2),   S(-28, -33),   S(  7,  -2),   S(-20,  47),   S( 29,   9),   S( 33,  23),   S( -6, -37),
            S( 10,   5),   S(-38,  21),   S( -3,   3),   S(  2,  32),   S( 36, -13),   S( 22,  35),   S( 20, -19),   S( 15,   6),
            S(  6,   8),   S(  9,  10),   S( -3,  16),   S( 25,   8),   S( 11, -18),   S(  7,   9),   S(  9,   5),   S( 25,  12),
            S( 19,  36),   S(  3,  19),   S( 29, -19),   S( 10,  25),   S( -3,  10),   S( -9, -27),   S(  7,   0),   S( 21,  28),
            S( 17,  42),   S( 20,  29),   S( 43,  13),   S(  7,  18),   S( -6,  23),   S(  5,  12),   S( 12,  17),   S(  5,  -5),

            /* bishops: bucket 11 */
            S( 15,  -9),   S( -8, -16),   S( -9, -10),   S(  0,  -7),   S(-21, -19),   S( -4,  -8),   S(-22, -30),   S( -5,  12),
            S( -7, -12),   S(  6, -15),   S(-14,   4),   S(  0, -18),   S(-17,   8),   S(-44, -13),   S(-30,  -2),   S(  8,   0),
            S(-11, -48),   S( -5, -19),   S( -7, -22),   S(-33,   1),   S( -8, -15),   S( 16,  38),   S( -4, -13),   S( -3, -17),
            S(  3,  -2),   S( -4, -34),   S(  2,  -7),   S(-35, -23),   S( 11,  -2),   S( 18,  44),   S( 40,  14),   S( -8, -26),
            S(-11, -14),   S(-15, -13),   S(-39,  36),   S(-27,  30),   S(-24,  27),   S( 37,   2),   S( 28, -12),   S(  7,   5),
            S( -6,   6),   S(-12, -11),   S(  1,  10),   S(  0,  16),   S( 22,  15),   S( 18,  -9),   S(  1, -14),   S( -2, -17),
            S( -2,  -7),   S( 23,  42),   S( 16,  44),   S( 30,  19),   S( 16, -10),   S( -8,  -5),   S(-14, -20),   S( -8, -15),
            S( 33,  27),   S(  3,   0),   S( 27,  43),   S( 28, -20),   S( 16,  15),   S(  4,   6),   S( -6, -13),   S(  6,  -1),

            /* bishops: bucket 12 */
            S( -5, -10),   S( -4, -13),   S( -6,  -2),   S(  6,  18),   S( -9,  -9),   S( -8,  -5),   S( -1,   0),   S( -1,   1),
            S(  0,  -6),   S(  7,   3),   S( -2,  -4),   S(  0,  12),   S( -1,   7),   S(  8,   6),   S(-12, -19),   S( -2,  -5),
            S(  7,   4),   S( 11,  -4),   S( 25,  23),   S( 19,  13),   S( -3,  10),   S( -4,  -2),   S(  2,   4),   S( -5,  -3),
            S(  9,   1),   S( 16,   4),   S( 18,   3),   S( 16,  37),   S( 10,   5),   S(  4,  21),   S(  3,  13),   S(  3,   7),
            S( 11,   9),   S( 10,   9),   S( -3,  17),   S( 19,   6),   S( 18,  27),   S(  8,  27),   S(  7,  11),   S(  3,  11),
            S(  2,   0),   S( -8,  -8),   S( -3,  17),   S(  0,  -4),   S( 29,  28),   S( 14,  14),   S( -9, -10),   S( -4, -10),
            S( -3,  -3),   S(  5,  12),   S(  3,  11),   S(  5,  -6),   S( 12,   1),   S( 19,  22),   S( 14,  29),   S( -1,  -3),
            S(  0,   5),   S( -1,  -4),   S(  1,  -2),   S(  0,  -5),   S(  2,   8),   S(  3, -11),   S( 14,   5),   S(  8,   6),

            /* bishops: bucket 13 */
            S( -2, -11),   S( -1,  -3),   S( -5, -15),   S( -7, -11),   S( 15,  12),   S( -8, -12),   S(-16, -20),   S( -2,  -3),
            S( -6,  -2),   S( -5,  -8),   S( -1,   3),   S( 15,   0),   S( -6, -17),   S(  2,  10),   S(  3,  -1),   S(  0,  -3),
            S(  7, -13),   S( 29,  16),   S( 16,   6),   S( 17,  25),   S(  2,  21),   S( 13,  31),   S( -9,   1),   S( -7,  -5),
            S( 24,  28),   S( 45,  13),   S( 20,  24),   S(-18,   7),   S( 16,  66),   S(  2,  10),   S(  8,   4),   S(  2,   8),
            S( 20,  20),   S( 15,  13),   S( 10,   2),   S(  7,  -9),   S(  8,  -7),   S(  9,  17),   S( 12,  13),   S(  2,   9),
            S(  6,   4),   S(  1,   6),   S(  1,  -2),   S( 16,  -5),   S(  5,  12),   S(  1,  -9),   S(  1,  -5),   S( 11,  -1),
            S(  7,   8),   S( -7, -14),   S( -2, -18),   S(  3,   4),   S(  4,  15),   S( 16,   9),   S( 13,   3),   S(  9,  12),
            S(  2,  -1),   S( -2,  -2),   S( -1,  12),   S(  1,   9),   S(  7,  15),   S(  3, -12),   S( 13,  -2),   S( 14,  -6),

            /* bishops: bucket 14 */
            S(-11, -21),   S(  5,  21),   S( 16,  12),   S(  4,  21),   S(-13,  -4),   S( -8,  -7),   S( -5,   2),   S( -6,  18),
            S( -1,   1),   S(  1,   0),   S(  1,   9),   S( -2,  -9),   S( 12,   1),   S(  2,   7),   S( -3,  23),   S(  3,  27),
            S(  1,  -4),   S( -3, -15),   S( -2,  -4),   S( 19,  30),   S( 22,  42),   S( 15,  26),   S(  4,  34),   S(  2,  26),
            S(  4,  31),   S(  8, -14),   S( -4,  -3),   S(  1,  28),   S(  8,  18),   S( 19,   5),   S( 20,  16),   S(  9, -17),
            S( 10,   6),   S(  6,  13),   S( 10,   1),   S( 18,   6),   S( -5,   3),   S(  4,  12),   S( 21,  -1),   S( 15,  11),
            S(  1, -12),   S( 21,  35),   S( 10,  19),   S( 13,   5),   S(  8,  -2),   S( -1,  10),   S( -3,  18),   S( 16,   0),
            S( 16,  35),   S( 12,  17),   S( 11,  18),   S(  6,   9),   S(  7,   0),   S(  3,  11),   S(  2,  -7),   S(  2,   1),
            S( 16,   6),   S( 12,  18),   S(  3,  10),   S(  4,   1),   S( -4,  -4),   S(  1,  -4),   S(  8,  11),   S(  4,   6),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -1),   S( -7, -16),   S( -2,  -2),   S( -7, -23),   S( -4,  -9),   S( -5, -14),   S( -3,  -3),
            S(  8,  13),   S( -2,  -7),   S(  5,   4),   S(  3,   4),   S(  7,  -3),   S( -1,  -5),   S( -1,  -9),   S( -3,  -5),
            S(  2,  -5),   S(  2,   0),   S(  4,   0),   S( 13,  12),   S( 11,  27),   S( 11,  30),   S( 16,  20),   S(  4,   3),
            S(  1,  -8),   S( 12,  13),   S( 11,  28),   S(-18,  -6),   S(  3,   7),   S( 16,   3),   S( 13,   1),   S(  9,  15),
            S( -2,  -9),   S( -2,  10),   S( -5,  18),   S( 20,  52),   S( 18,  22),   S( 11,  -3),   S(  8,   1),   S( -3,   2),
            S( -2,  19),   S(  5,  10),   S(  8,  32),   S(  6,   8),   S( 22,  19),   S( 11,  -7),   S(  3,   9),   S(  1,  -2),
            S(  5,  -3),   S(  5,  22),   S(  6,  27),   S( 12,  16),   S(  9,  15),   S( -2,   7),   S(  0,  -7),   S(  0,   0),
            S(  4,   0),   S( 10,  12),   S(  7,  -3),   S(  9,  10),   S(  4,  16),   S(  1,  -2),   S(  4,  10),   S(  5,   0),

            /* rooks: bucket 0 */
            S(-20,  13),   S(  7, -11),   S(-11,   2),   S(-12,  17),   S(-33,  61),   S(-20,  36),   S(-51,  63),   S(-58,  47),
            S(  0, -22),   S( -4,  15),   S(-33,  22),   S( -6,  30),   S( -8,  43),   S(-11,  25),   S(-21,  11),   S(-28,  42),
            S( 22, -35),   S( 10, -15),   S(-14,  10),   S( -7,  14),   S(-36,  57),   S(-17,  16),   S(-22,  40),   S( -8,  16),
            S(  9, -21),   S( 37,  -6),   S(-32,  30),   S( 15,  15),   S(  7,  49),   S(-20,  46),   S(-24,  52),   S(-19,  33),
            S( 54, -62),   S( 44,  -3),   S( 21,  23),   S( 35,  21),   S( 42,  18),   S( 22,  67),   S( 32,  50),   S(  6,  58),
            S( 57, -29),   S( 62,  15),   S(111, -21),   S(104,  21),   S( 27,  54),   S( 31,  59),   S(  7,  69),   S(-41,  80),
            S( 24,  17),   S( 52,  45),   S( 96,  28),   S( 66,  11),   S( 62,  46),   S( 16,  61),   S(-10,  73),   S(-18,  67),
            S(  4, -21),   S( 28,  23),   S( 26,  22),   S( 42,  -4),   S( 23,  44),   S( 42,  13),   S( 34,  15),   S( 51, -43),

            /* rooks: bucket 1 */
            S(-56,  49),   S(-21,   6),   S(-14,  13),   S(-45,  31),   S(-42,  45),   S(-46,  47),   S(-53,  68),   S(-77,  74),
            S(-47,  35),   S(-22,  -9),   S(-24,  16),   S(-30,  24),   S(-34,  17),   S(-46,  41),   S(-26,  20),   S(-37,  52),
            S(-33,  25),   S(-11,  -6),   S(-13,  -1),   S(-25,  14),   S(-32,  19),   S(-51,  34),   S(-60,  61),   S(-29,  59),
            S(-42,  46),   S( -4,   9),   S(-16,  26),   S(-27,  14),   S(-37,  36),   S(-50,  65),   S(-33,  61),   S(-70,  88),
            S(-14,  42),   S( 18,  -9),   S( 33,   9),   S( 30,   0),   S(  4,  23),   S( -8,  78),   S(  6,  60),   S(-10,  83),
            S( 48,  32),   S( 73,  -5),   S( 39,  12),   S( -1,  29),   S( 12,  19),   S( 11,  55),   S( 38,  43),   S( 10,  78),
            S( 16,  64),   S( 35,   1),   S(  6,  31),   S( 17,  15),   S( 49,  11),   S(  3,  51),   S( 26,  62),   S( 33,  80),
            S( 51, -11),   S( 15,  -8),   S( -2, -11),   S(-20, -10),   S( 22,   4),   S( 16,  15),   S( 33,  29),   S( 48,  34),

            /* rooks: bucket 2 */
            S(-62,  69),   S(-51,  60),   S(-43,  54),   S(-40,  23),   S(-28,  24),   S(-41,  27),   S(-31,  16),   S(-69,  59),
            S(-58,  65),   S(-53,  55),   S(-50,  55),   S(-47,  32),   S(-50,  38),   S(-46,  19),   S(-19,   4),   S(-51,  38),
            S(-47,  65),   S(-33,  55),   S(-42,  41),   S(-36,  35),   S(-33,  23),   S(-31,  20),   S(-15,   6),   S(-12,  27),
            S(-37,  77),   S(-29,  68),   S(-47,  64),   S(-61,  51),   S(-46,  45),   S(-24,  29),   S( -9,  26),   S(-21,  45),
            S(-12,  85),   S(-19,  81),   S(  6,  67),   S(-12,  41),   S(-28,  52),   S( 28,  22),   S(  7,  39),   S( -1,  64),
            S( 22,  84),   S( 22,  72),   S( 36,  62),   S(-12,  48),   S( 51,  14),   S( 42,  47),   S(112,  -4),   S( 58,  60),
            S( 50,  63),   S( -6,  76),   S( 17,  52),   S( 34,  19),   S(  3,   6),   S( 24,  70),   S(-39,  88),   S( 36,  67),
            S( 14,  41),   S( 20,  48),   S( 26,  31),   S(-27,  23),   S(-33,   9),   S( 16,  10),   S( 13,  21),   S( -6,  51),

            /* rooks: bucket 3 */
            S(-18,  72),   S(-13,  68),   S(-15,  91),   S(-12,  82),   S( -1,  47),   S(  1,  41),   S( 19,  12),   S( -9,   4),
            S( -2,  60),   S(-15,  73),   S(-16,  94),   S( -5,  85),   S( -4,  51),   S( 13,  13),   S( 46, -13),   S( 18,   5),
            S( 12,  57),   S( -7,  82),   S(-11,  81),   S( -8,  88),   S( 15,  39),   S(  5,  30),   S( 36,   8),   S( 32,   6),
            S(  1,  88),   S( -7, 107),   S(-17, 109),   S( -6,  96),   S( -3,  67),   S( 16,  47),   S( 34,  30),   S(  6,  26),
            S(  4, 105),   S(-10, 118),   S( 19, 111),   S( 20, 100),   S( 17,  83),   S( 45,  57),   S( 67,  32),   S( 42,  41),
            S(  6, 123),   S( 24, 107),   S( 34, 113),   S( 48,  95),   S(103,  43),   S(133,  24),   S( 90,  37),   S( 50,  37),
            S( 20, 110),   S( 14, 109),   S( 28, 116),   S( 25, 111),   S( 31,  92),   S(101,  42),   S(106,  93),   S(140,  63),
            S(114, -31),   S( 49,  41),   S( 12,  96),   S( 14,  79),   S( 17,  67),   S( 69,  55),   S( 38,  30),   S( 96,   9),

            /* rooks: bucket 4 */
            S(-21, -26),   S( 15, -19),   S(-21,  -7),   S(-40,  17),   S(-56,  18),   S(-39,  48),   S(-41,   5),   S(-85,  39),
            S(-25, -46),   S(-50,  -2),   S(-19, -18),   S(  2, -31),   S( 16, -15),   S( -9,   6),   S(-26,   0),   S( -1,  15),
            S(-13, -21),   S(-38, -22),   S(-37,  -7),   S( -9, -35),   S(-32,  -5),   S(-45,  18),   S(-22,  18),   S(-62,  22),
            S(-54, -35),   S(  9,   2),   S( 10, -22),   S( 14, -19),   S( 40,   3),   S( -7,  15),   S(-10,  -3),   S(-14,  12),
            S(-16, -34),   S( 27, -36),   S( 26,   2),   S( 51, -15),   S( 64,  -3),   S( 57,  27),   S( 17,  13),   S( 15,  27),
            S(-14, -36),   S( 10,  11),   S(  9,  -2),   S( 21,  10),   S( 30,  23),   S( 13,  15),   S( 32,  19),   S( 33,  39),
            S(-20, -22),   S( 34,  24),   S( 45,  -3),   S( 56, -10),   S( 62,  -3),   S(-11,  14),   S( 15, -11),   S( 23,   8),
            S( 18, -29),   S(  7,  14),   S( 33, -11),   S( 26, -11),   S( 53,   4),   S( 13,   5),   S(  5,  10),   S(  4,  14),

            /* rooks: bucket 5 */
            S(-24,  26),   S(-15,   6),   S(  0,  -5),   S( 25,  -4),   S(-12,  20),   S(-12,  31),   S(-32,  55),   S(-32,  36),
            S(-10,  -3),   S(-25, -14),   S( 42, -55),   S( 29, -22),   S(-20,   6),   S(-25,  13),   S(-41,  32),   S(-13,  30),
            S(-36,  23),   S( -3,  -6),   S(  7, -25),   S( -3, -15),   S(-17,  -5),   S( 32, -16),   S(-44,  33),   S(-26,  20),
            S(-23,  24),   S(  6,   3),   S( 53, -30),   S( 37,  -8),   S( 42, -10),   S( -7,  38),   S( 12,  31),   S(  6,  45),
            S( 40,  18),   S( 24,   5),   S( 18,  18),   S( 10,  -3),   S( -3,  19),   S( 71,   8),   S( 29,  34),   S( 48,  35),
            S(  0,  31),   S( -5,  11),   S(  5,   5),   S(-12, -12),   S( 21,  13),   S( 20,  26),   S( 58,  16),   S( 50,  32),
            S( 49,   3),   S( 47, -10),   S(  0,  -1),   S( 36,   7),   S( 59,  -7),   S( 56, -14),   S( 85, -13),   S( 45,  15),
            S( 20,  30),   S( 17,   5),   S( 53,  -8),   S(  5,  15),   S( 43,  17),   S( 23,  30),   S( 33,  40),   S( 57,  40),

            /* rooks: bucket 6 */
            S(-49,  48),   S(-37,  39),   S(-31,  30),   S(-31,  23),   S( -1,   7),   S(  5,  -2),   S( 21,  -8),   S(-36,  21),
            S(-44,  30),   S(  8,   9),   S(-11,  12),   S( -6,   2),   S( 16, -17),   S(-27,  -2),   S(-30,   0),   S( -9,  12),
            S(-55,  39),   S( -3,  18),   S( -1,   6),   S(  2,   1),   S(-11,   5),   S( 35, -17),   S(  5, -19),   S( -9,  -3),
            S(-33,  52),   S( -5,  38),   S( 19,  14),   S( 62, -12),   S( 42, -15),   S( 21,   1),   S( 14,  -2),   S( 18,  28),
            S( -6,  52),   S( 58,  25),   S( 91,  17),   S( 73,  -8),   S( 31,  -9),   S( 33,  14),   S( 72, -10),   S( 89,   2),
            S( 80,  12),   S( 84,  -1),   S( 85,   0),   S( 44, -16),   S(  4, -15),   S( 23,  32),   S( 35,  -6),   S( 58,  15),
            S( 59,  11),   S(130, -23),   S(107, -24),   S(100, -38),   S( 36, -15),   S( 51,  -5),   S( 67, -13),   S( 86, -23),
            S( 78,  -8),   S( 51,  16),   S(  6,  30),   S( 63,  -8),   S( 61,   1),   S( 31,  24),   S( 87,   4),   S( 57,  19),

            /* rooks: bucket 7 */
            S(-99,  35),   S(-80,  36),   S(-70,  36),   S(-62,  35),   S(-33,  -2),   S(-27, -18),   S(-34,   4),   S(-71, -15),
            S(-85,  34),   S(-34,  10),   S(-56,  21),   S(-65,  31),   S(-33, -13),   S(-18, -15),   S(  3,  -7),   S( -5, -58),
            S(-84,  36),   S(-67,  29),   S(-33,   7),   S(-42,  21),   S(-39,   0),   S(-25,   6),   S( 43, -34),   S(  1, -53),
            S(-74,  37),   S(-14,  15),   S( -3,  12),   S( 63, -23),   S( 16,  -8),   S( 71, -32),   S( 52, -11),   S( 27, -28),
            S(  0,  28),   S( 31,  21),   S( 60,   9),   S( 88, -14),   S(144, -51),   S(123, -56),   S( 88, -26),   S(-41, -37),
            S( 29,  16),   S( 30,   2),   S( 94,  -6),   S( 87, -23),   S( 78, -14),   S( 40,   7),   S( 23,  33),   S( -2, -28),
            S(  8,  -1),   S( 40, -14),   S( 74, -16),   S(111, -44),   S(122, -46),   S(117, -46),   S( 50,   5),   S( 23, -32),
            S(-23, -15),   S(  8,   5),   S( 41,  -3),   S( 31,   0),   S( 51, -17),   S( 72, -13),   S( 35,  10),   S( 29, -22),

            /* rooks: bucket 8 */
            S(-14, -81),   S(-15, -38),   S( -6, -14),   S( 18,   6),   S(-24, -27),   S(-21,   4),   S(-12, -30),   S(-20,   8),
            S(-31, -81),   S(-15, -44),   S(-22,   0),   S(-27, -65),   S(-25, -40),   S(-15, -21),   S(-10,  -7),   S(-38, -33),
            S(  1, -10),   S( -3, -14),   S( 12,  -5),   S(-11,  17),   S( -8,  48),   S( 13,  27),   S(  4,  49),   S(-18,   4),
            S( -5, -21),   S( -1,   5),   S( -1,  -2),   S( 15,  24),   S(  4,  42),   S( 32,  42),   S(  0,  20),   S( -9, -10),
            S( -9, -41),   S( 11,  21),   S(  9,  18),   S( 17,  37),   S(  8,  23),   S( -1,   5),   S( 12,  46),   S(  0,  22),
            S(-24,   9),   S(  3,  12),   S(-17,   8),   S( -5, -16),   S(  5,  36),   S(-15,  32),   S( -1,   4),   S(  3,  23),
            S(  3,  33),   S(  2,  26),   S(  3,   6),   S( 21,  13),   S( 15,  12),   S( 10,  32),   S(  6,  26),   S(  3,  47),
            S(-11,  14),   S(  3,  14),   S(-19,  31),   S( 34,  50),   S( -6,  24),   S( 12,  43),   S(  2,  25),   S(  7,  42),

            /* rooks: bucket 9 */
            S(-30, -68),   S( -9, -66),   S( -5, -98),   S(-10, -44),   S(-15, -48),   S(  1, -32),   S( -6, -19),   S( -4, -29),
            S(-60, -49),   S(-29, -73),   S(-27, -66),   S(-41, -49),   S(-35, -55),   S(-26,   3),   S(-21, -52),   S(-30, -30),
            S(-11, -14),   S(-21, -15),   S(  3,  -7),   S( -7, -33),   S( -5, -17),   S(  7,  18),   S(  3,   7),   S(  3,  16),
            S( -5,   2),   S(  4,  -3),   S(  3,   2),   S( -1,   5),   S(-12, -31),   S(  5,   0),   S( -7,  -1),   S(  5, -22),
            S( -4,   0),   S( -8, -15),   S( -7, -48),   S( -8,   3),   S(-17, -19),   S(-11,   2),   S(-13, -13),   S( -7, -10),
            S( -9,   3),   S(-29, -19),   S(-12, -20),   S( -1,  18),   S( -4,  -3),   S( -8,  11),   S( -5,   1),   S(-13,  10),
            S(  5,  33),   S(  7,   3),   S(  5, -34),   S(  2,  15),   S(  7, -15),   S( 20,   6),   S(  4,  14),   S( -4,  -9),
            S(-18,  16),   S(-17,  27),   S( -7,  17),   S( -6,  35),   S( -8,  30),   S(  5,  55),   S(  3,  22),   S( 12,  30),

            /* rooks: bucket 10 */
            S(-21, -27),   S(-55,  -8),   S(-30, -40),   S( -5, -50),   S(-13, -47),   S(  1, -79),   S(  6, -64),   S(-19, -42),
            S(-42, -12),   S(-31, -32),   S(-41, -25),   S(-37, -52),   S(-41, -46),   S(-23, -48),   S(-11, -36),   S(-46, -75),
            S( -7, -12),   S(-23, -14),   S(-29, -18),   S(-38, -45),   S( -9, -17),   S(  3, -15),   S( -8, -29),   S(-13, -15),
            S(-25,  -7),   S(-35, -32),   S( -4, -34),   S( -8,   0),   S(  6,   4),   S(  6,  12),   S(-10, -35),   S(  1, -36),
            S(  8,  -8),   S(  3,  -8),   S(-13, -16),   S( -9, -36),   S(  7,  11),   S( -3,   0),   S( -5, -26),   S( -8, -36),
            S(-10,   5),   S( 12,   1),   S( -3, -17),   S(  0, -31),   S(  1,  -8),   S( -7,  -7),   S(-18, -33),   S( -3, -18),
            S( -9,  -5),   S(  8, -24),   S( -1, -19),   S( -4, -13),   S( 13, -18),   S(-10,  -7),   S(-13, -31),   S( -8, -12),
            S( -6,   1),   S(  8,  32),   S(  0,  35),   S(-11,  13),   S( -9,  34),   S(-27,   7),   S(-29,  15),   S( -4,  14),

            /* rooks: bucket 11 */
            S(-58, -16),   S(-39,   1),   S(-51,  -7),   S(-27,  -5),   S(-46, -17),   S(-19, -17),   S(-16, -34),   S(-37, -65),
            S(-17, -14),   S(-22, -19),   S(-58, -10),   S(-53, -19),   S(-14, -25),   S(-11, -11),   S(-25, -31),   S(-43, -61),
            S(-30,  25),   S(-22,  13),   S( -8,  32),   S(-19,  19),   S(  7, -22),   S( -7,  -3),   S(  7, -21),   S(-10,  13),
            S(-25,  -5),   S(-10, -16),   S(-12,  13),   S(  8,  17),   S( 21,  12),   S(-19, -35),   S(  6,  16),   S( -8, -21),
            S( -8,  -6),   S(  6,  -3),   S(  5,   7),   S(  4,   8),   S( 34,  -8),   S(  1,  -5),   S( 18,  34),   S(-15, -43),
            S(  4, -13),   S(-12,  -4),   S( 14,  -9),   S( 19,  -3),   S(-11, -16),   S(  5,   4),   S(  7,  31),   S( -4, -10),
            S( -6,  10),   S(-22, -25),   S( -3,   0),   S( -1,   3),   S(  8,  -2),   S(  5,   7),   S(  2,  15),   S(-12,  -6),
            S( -7,   7),   S( 16,  36),   S(  3,  28),   S( 18,  26),   S(-11,   8),   S( -3,  25),   S( 14,  13),   S(-20,  23),

            /* rooks: bucket 12 */
            S(-33, -98),   S( -9, -14),   S(-19, -55),   S(-19, -36),   S(-11, -26),   S(  9,  -8),   S(-16, -39),   S(-18, -40),
            S(  3,   3),   S(  1,   4),   S(  8,  20),   S(  4,  13),   S(  8,   7),   S( 10,  -6),   S(  6,   9),   S(-18, -23),
            S( -4, -11),   S(  7,  35),   S( 11,  23),   S( 24,  23),   S(  7,  -7),   S( 16,  24),   S(  7,  33),   S( -2,  27),
            S(  6,  22),   S(  8,   2),   S( 14,  33),   S( 10,  20),   S( 12,   9),   S(  5,   8),   S(  6,  20),   S( -3,   6),
            S( 12,  17),   S( 14,  30),   S(  8,  47),   S(  2,   0),   S(  9,  28),   S( -2, -13),   S(  5,  16),   S(  6,  14),
            S( -2,   1),   S( -3,  -6),   S(  0,  17),   S( -5,   3),   S(  8,  25),   S(  0, -20),   S( 10,  26),   S(  5,  10),
            S(-15, -10),   S(-12,  19),   S(  7,  40),   S(  0,  21),   S( -2,   2),   S( 12,  18),   S(  3,  24),   S(  0,  24),
            S(  3,   4),   S( -8,  29),   S(  5,  31),   S( 14,  24),   S(  2,   6),   S(  1,  21),   S(  2,  11),   S(  2,  15),

            /* rooks: bucket 13 */
            S(-25, -24),   S(-25, -51),   S(-24, -51),   S(-15, -35),   S(-27, -52),   S( -3,  -5),   S(-25, -48),   S(-23, -37),
            S(-15, -11),   S( -8, -18),   S(  2,   7),   S( -2,  -3),   S( 18,  34),   S(  4,  13),   S(  8,   2),   S(-10, -12),
            S(-13,  -3),   S(-14,   2),   S( -4,  -8),   S(  8,  10),   S(  6,  26),   S( 14,  -1),   S( 12,  44),   S(-12, -25),
            S(  8,  15),   S( -1,   6),   S( -3,   8),   S(  5,  17),   S( 10,  21),   S(  0,   7),   S(  5,  14),   S(  1,  21),
            S(  7,  21),   S(  3,  -8),   S( -4, -21),   S(  3,   5),   S( -5,  24),   S(  1,  -2),   S(  6,   8),   S( -1,  -2),
            S(  1,  15),   S( -4,  -4),   S( -9,  -9),   S(-13,   0),   S(-12, -14),   S(  3,  -2),   S( -7,   9),   S(  1,   5),
            S(  3,  -8),   S(  7,   7),   S( -9, -28),   S(  3,  17),   S( -9,  -3),   S(  7,  12),   S(  1,   5),   S(  0, -14),
            S(  2,  23),   S(-10,  13),   S( -4,   7),   S( 10,  27),   S( -2,  18),   S(  7,  25),   S(  0,  24),   S(  4,   8),

            /* rooks: bucket 14 */
            S( -5, -26),   S(-30, -28),   S(-17, -17),   S(-18, -53),   S(-11, -37),   S( -5, -21),   S(-32, -56),   S(-24, -38),
            S( -7,  27),   S(  4,  27),   S(  6,   9),   S( -1, -20),   S(  0,  -7),   S( -3,  -4),   S( -1,   5),   S( -4,  -4),
            S(  4,  31),   S( -2,  27),   S(  0,   2),   S(  2,   0),   S(  4,   9),   S(  0,  -4),   S(  1,  19),   S(-18, -49),
            S( -4,  14),   S( 15,  21),   S(  6,  16),   S(  9,   5),   S( -8,  -6),   S(  0, -10),   S(  9,  12),   S(-11, -18),
            S(  9,  19),   S( 19,  24),   S( -2,  -2),   S(  2,   7),   S(  3, -11),   S( 17,  30),   S(  0,   4),   S( -3, -14),
            S(  6,  15),   S(  7,  15),   S(  8,  18),   S(  2,   5),   S( -3,   7),   S(-16,   6),   S( -8,  -6),   S( -6,  -5),
            S( -5,  -7),   S(  9,  18),   S( -7, -17),   S(-19, -33),   S( -4,   7),   S(  0,   3),   S(-12, -12),   S( -8,  -7),
            S(  0,   2),   S(  4,  10),   S( -4, -13),   S(  6,  -9),   S(-10, -15),   S(-15, -41),   S(  2,  -4),   S(  1,  32),

            /* rooks: bucket 15 */
            S(-24, -43),   S(-17, -47),   S(-39, -49),   S(-23, -50),   S( -2, -22),   S(-12, -22),   S( -3, -10),   S(-20, -51),
            S(  7,  32),   S(-11,   3),   S(-11,  -8),   S( -6,  -9),   S( -6, -18),   S(  4,  -1),   S(  7,  10),   S(  3,   6),
            S(  6,   8),   S( -6, -14),   S( 11,  24),   S(  8,  -2),   S(  6,  -3),   S( -4, -17),   S(  6,  24),   S(  3,   8),
            S(  2,  10),   S( -2,  -5),   S( 18,  33),   S( -3, -11),   S(  4,  17),   S(  2,   8),   S(  7,  14),   S(  3, -12),
            S(  7,  17),   S(  6,  11),   S(  6,  -8),   S(  4,  15),   S(  6,  14),   S(  3,   0),   S( -3,  29),   S(  5, -10),
            S(  7,  19),   S(  8,   4),   S(  9,   2),   S(  5,   7),   S( -5, -12),   S( -4,  38),   S(  2,  23),   S(  5,   2),
            S(  4,  -2),   S( -2,   8),   S(  9,  19),   S(  5,  13),   S(  2,  17),   S(  4,  16),   S(-13,  10),   S( -9, -30),
            S(  1,  26),   S(  0,  26),   S(  9,  25),   S(  2,  29),   S(  0,   4),   S( -5, -24),   S( -5,  14),   S(-15, -11),

            /* queens: bucket 0 */
            S( -3,  -6),   S(-25, -48),   S(-32, -58),   S(  1, -97),   S( -7, -54),   S(  9, -58),   S(-56, -27),   S(-15, -10),
            S(-13, -29),   S( 12, -75),   S(  4, -66),   S(-11, -17),   S(  2, -21),   S(-10, -35),   S(-25, -27),   S(-37,  -9),
            S( -3,   9),   S( -4, -21),   S( 27, -51),   S(-11,   7),   S( -9,  25),   S( -4,   2),   S(-35,   0),   S(-78, -40),
            S(-25,  28),   S( 17, -26),   S( -6,  18),   S(-13,  65),   S( -6,  63),   S(-25,  37),   S(-43,  25),   S(-17, -24),
            S(-19, -16),   S(  4,  66),   S(  6,  30),   S(  0,  39),   S(  4,  63),   S(-28, 107),   S(-57,  67),   S(-40,   2),
            S(-14,  12),   S( 18,  35),   S( 37,  36),   S(-23,  71),   S(-30,  66),   S(-64,  97),   S(-65,  24),   S(-41,   5),
            S(  0,   0),   S(  0,   0),   S( 18,   2),   S(-31,  29),   S(-35,  24),   S(-64,  79),   S(-89,  60),   S(-100,  25),
            S(  0,   0),   S(  0,   0),   S(  8,  -6),   S(-11, -13),   S(-31,  22),   S(-37,   3),   S(-53,  -5),   S(-63, -27),

            /* queens: bucket 1 */
            S( 16,  -4),   S(  6,   2),   S( 14, -48),   S( 31, -88),   S( 37, -44),   S( 12, -25),   S( 14,  -5),   S(  2,  18),
            S(-23,  33),   S( 22,  16),   S( 37, -38),   S( 26,   3),   S( 40,  12),   S(  0,  21),   S(-21,  35),   S(-19,  10),
            S( 45,  -3),   S( 22,   2),   S( 15,  32),   S( 16,  71),   S( -6,  80),   S( 31,  43),   S( -7,  36),   S( 15,  -9),
            S( 35,   4),   S( 18,  39),   S( 18,  46),   S( 43,  66),   S( 17,  82),   S(  4,  58),   S(  8,  38),   S(-11,  54),
            S( 45,  -2),   S( 52,  14),   S( 51,  40),   S( 26,  34),   S( 46,  67),   S( 33,  25),   S(-10,  71),   S(  2,  91),
            S( 62,  -1),   S(102,  10),   S( 83,  44),   S( 84,  55),   S( 55,  38),   S( 16,  62),   S( 39,  52),   S(  1,  50),
            S(100, -21),   S( 58, -19),   S(  0,   0),   S(  0,   0),   S(  6,  42),   S(-10,  22),   S( -9,  52),   S(-41,  36),
            S( 81,  -4),   S( 60,  -9),   S(  0,   0),   S(  0,   0),   S( 18,  23),   S( 42,  27),   S( 79,   0),   S(-19,  34),

            /* queens: bucket 2 */
            S( 33, -13),   S( 26,  11),   S( 30,  21),   S( 44, -22),   S( 43, -29),   S( 29, -22),   S( -1, -20),   S( 34,  29),
            S( 21,   3),   S(  6,  48),   S( 36,  19),   S( 42,  31),   S( 51,   6),   S( 20,  23),   S( 24,  17),   S( 16,  48),
            S( 33,  11),   S( 28,  34),   S( 18, 101),   S( 15,  82),   S( 25,  76),   S( 22,  67),   S( 32,  44),   S( 30,  59),
            S(  4,  59),   S( 21,  82),   S( 19,  81),   S( 12, 122),   S( 28,  96),   S( 22,  90),   S( 37,  60),   S( 33,  80),
            S(  1,  85),   S( -5,  72),   S(  2,  93),   S( 32,  76),   S( 24, 100),   S( 90,  44),   S( 73,  57),   S( 68,  53),
            S(-15,  88),   S( -7,  79),   S(  2,  75),   S( 77,  35),   S( 38,  57),   S( 97,  73),   S(119,  36),   S( 45, 107),
            S( -3,  54),   S(-11,  48),   S( -8,  70),   S( 49,  27),   S(  0,   0),   S(  0,   0),   S( 22,  79),   S( 47,  72),
            S( -1,  35),   S( 36,  -5),   S( 48, -12),   S( 31,  35),   S(  0,   0),   S(  0,   0),   S( 52,  42),   S( 15,  65),

            /* queens: bucket 3 */
            S(-44,  36),   S(-32,  43),   S(-24,  40),   S(-13,  50),   S(-28,  34),   S(-16, -14),   S(-17, -36),   S(-41,  24),
            S(-59,  57),   S(-39,  46),   S(-26,  63),   S(-18,  81),   S(-16,  72),   S(-17,  35),   S( 15, -15),   S( 15, -27),
            S(-53,  76),   S(-39,  85),   S(-33, 112),   S(-42, 142),   S(-30, 121),   S(-25,  92),   S(-11,  52),   S(-13,  20),
            S(-42,  77),   S(-61, 135),   S(-52, 158),   S(-35, 169),   S(-39, 159),   S(-19,  95),   S( -4,  75),   S(-16,  66),
            S(-54, 119),   S(-47, 154),   S(-55, 174),   S(-45, 186),   S(-24, 151),   S( -1, 127),   S(-12, 120),   S(-18,  80),
            S(-60, 110),   S(-59, 155),   S(-63, 179),   S(-60, 189),   S(-51, 163),   S( 18,  92),   S(-23, 122),   S(-24, 120),
            S(-95, 122),   S(-95, 142),   S(-79, 180),   S(-72, 157),   S(-77, 162),   S(-17,  81),   S(  0,   0),   S(  0,   0),
            S(-126, 138),  S(-83, 101),   S(-71, 102),   S(-66, 108),   S(-52,  99),   S(-14,  56),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-34,  -3),   S(-49, -37),   S(-10,  -1),   S( -9, -19),   S( -7,  -6),   S(-10,  11),   S(-33, -26),   S( 13,  22),
            S(  0, -11),   S( -8,   6),   S( -4,   1),   S(-19, -13),   S(-43,  19),   S(-17,  12),   S(-46, -10),   S(  0, -16),
            S( 11,  19),   S( 22, -29),   S( 15, -17),   S( 17,   9),   S( 41,  11),   S( 13,  22),   S(-21, -18),   S( 32,  23),
            S(-10, -21),   S( 21, -19),   S(  8,   1),   S( -8,  18),   S( 44,  29),   S(  1,  59),   S(-26,   6),   S(-11,  19),
            S(  0,   0),   S(  0,   0),   S( 17,  -8),   S( 57,  36),   S( 26,  55),   S( 31,  51),   S( 10,  16),   S( 12,  22),
            S(  0,   0),   S(  0,   0),   S( 18,  10),   S( 34,  19),   S( 41,  47),   S( 30,  49),   S( 19,  24),   S(  0,   6),
            S( 15,  -4),   S( 20,   9),   S( 65,  38),   S( 62,  36),   S( 56,  13),   S( 18,  27),   S(  5,  22),   S(-12,  21),
            S( 29,  -7),   S(-18, -31),   S( 25,   7),   S( 46,  17),   S( 16,   6),   S(  9,  21),   S(  0,   2),   S( 21,   7),

            /* queens: bucket 5 */
            S( 35,  22),   S( 24,   7),   S( 13,   6),   S(-13,  26),   S( 31,  -6),   S( 39,  46),   S( 11,  -1),   S( 19,   2),
            S( 18,  15),   S( 15,  -2),   S( 14,  -1),   S(  9,  12),   S(  9,  42),   S(-14, -12),   S( 27,  14),   S( 14,   5),
            S( 21,   3),   S( 48,  -5),   S( 23,   0),   S(  8,  16),   S( 19,   7),   S( 31,  17),   S( 24,  40),   S( 12,  13),
            S(  9, -31),   S( 38,   4),   S( 26, -15),   S( 34,  16),   S( 63,   9),   S( 33,  14),   S( 35,  46),   S(  4,  30),
            S( 40,  -6),   S( 27, -41),   S(  0,   0),   S(  0,   0),   S( 12,  10),   S( 31,  15),   S( 39,  52),   S( 16,  33),
            S( 37,  16),   S( 36,   6),   S(  0,   0),   S(  0,   0),   S( 30,  20),   S( 63,  34),   S( 44,  37),   S( 52,  40),
            S( 75,   6),   S( 73,  11),   S( 51,  39),   S( 25,  25),   S( 53,  20),   S( 95,  43),   S( 65,  55),   S( 49,  29),
            S( 43,  30),   S( 54,  13),   S( 66,  20),   S( 44,  -2),   S( 55,  18),   S( 63,  37),   S( 69,  46),   S( 60,  30),

            /* queens: bucket 6 */
            S( 49,  50),   S(  0,   3),   S( 34,  15),   S( 33,  19),   S( 23,  14),   S( -7,   0),   S(  1,  11),   S(  9,  20),
            S( 27,  18),   S( 26,  32),   S( 55,  42),   S( 51,  29),   S( 38,  24),   S( 16,  13),   S(-12,  26),   S( 26,  31),
            S(-14,  45),   S( 34,  35),   S( 28,  37),   S( 50,  15),   S( 35,  13),   S( 47,  -1),   S( 63,  27),   S( 69,  59),
            S( 25,  36),   S(  6,  26),   S( 51,  10),   S( 96,  19),   S( 46,  -8),   S( 47,   9),   S( 84,   9),   S(101,  44),
            S( 30,  52),   S( 33,  36),   S( 55,  38),   S( 52,  31),   S(  0,   0),   S(  0,   0),   S( 66,  21),   S(114,  56),
            S( 42,  49),   S( 57,  48),   S( 49,  56),   S( 27,   8),   S(  0,   0),   S(  0,   0),   S( 81,  49),   S(117,  47),
            S( 61,  36),   S( 27,  23),   S( 75,  17),   S( 61,  18),   S( 43,  38),   S( 70,  46),   S(131,  25),   S(143,  10),
            S( 38,  38),   S( 66,  24),   S( 74,  15),   S( 82,  34),   S(103,  13),   S( 99,  13),   S(112,  13),   S(102,  28),

            /* queens: bucket 7 */
            S( -8,  23),   S( -8,  -1),   S(-22,  20),   S( -8,  23),   S( 12,   2),   S(-14,   3),   S( -4,  16),   S(-12,  -6),
            S(-12,  25),   S(-50,  27),   S(-10,  54),   S(-16,  79),   S(-14,  42),   S(  4,  24),   S(  5,   4),   S(-31,  -1),
            S(  1,  24),   S(-19,  37),   S(-24,  91),   S( 32,  50),   S( 45,  30),   S( 29,   6),   S( 53, -28),   S( 54,  -2),
            S(-17,  22),   S( 14,  43),   S( 14,  69),   S( 42,  70),   S( 76,  44),   S( 73,  -5),   S( 84, -34),   S( 51,  -6),
            S( 13,  23),   S(-14,  59),   S( 15, 101),   S( 49,  80),   S( 89,  17),   S( 71,  -2),   S(  0,   0),   S(  0,   0),
            S( -1,  46),   S(-10,  87),   S(  7,  90),   S(  0,  85),   S( 62,  34),   S( 98,  51),   S(  0,   0),   S(  0,   0),
            S(-37,  58),   S(-22,  39),   S( 10,  55),   S( 36,  56),   S( 67,  35),   S( 84,  15),   S( 71,  24),   S( 67,  32),
            S( 33,  17),   S( 47,  29),   S( 52,  52),   S( 52,  18),   S( 55,  35),   S( 31,   2),   S( -9,   7),   S( 74, -10),

            /* queens: bucket 8 */
            S(-18, -36),   S(  0, -23),   S(-16, -41),   S( -3,  -9),   S(-15, -29),   S(  8,  -4),   S( -1, -12),   S(  1,   5),
            S(-20, -31),   S( -5, -14),   S(  3, -15),   S( -6, -11),   S(  9,  -3),   S( -4, -10),   S( -3,   3),   S( -1,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -16),   S( -9, -44),   S(  5,   3),   S(  9,  -4),   S( -8,  -9),   S(  3,   6),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S( -1, -12),   S( -1,   3),   S(  4,   0),   S( 12,  19),   S(  6,   3),
            S( -2, -11),   S(  7,  10),   S(  7,   2),   S( 12,  -6),   S(  8,  -9),   S( 12,  13),   S( 14,  13),   S( -9,  -8),
            S(  1, -15),   S(  4, -16),   S( 15,  14),   S(  3, -20),   S( 12,   8),   S( 26,  34),   S(  8,  -3),   S( -2,  -4),
            S(-18, -37),   S(  1, -11),   S( 12,   7),   S( 25,  37),   S( 12,  11),   S( 17,  39),   S(  4,   6),   S(  5,   1),
            S(  1,   1),   S(  4,  -7),   S( 13,   7),   S(  8,  -3),   S( 17,  18),   S( -3,  -5),   S(  3,  10),   S(-17, -28),

            /* queens: bucket 9 */
            S( 10, -10),   S(-18, -34),   S(-13, -32),   S( 13,  -8),   S( -6, -35),   S( -2,  -9),   S( -5,  -9),   S( -2, -14),
            S( -1,  -7),   S(-10, -20),   S(-10, -26),   S(  2, -15),   S(-22, -50),   S(-12, -29),   S(  6,  -3),   S(  1, -10),
            S(-16, -44),   S(-13, -27),   S(  0,   0),   S(  0,   0),   S(  5,  -8),   S( 11,  -7),   S( -4,  -8),   S(  5,  -3),
            S(  2,  -7),   S(-11, -30),   S(  0,   0),   S(  0,   0),   S(  0,  -4),   S( 11,   2),   S( 11,  12),   S( -2,   2),
            S( -8, -28),   S(  1, -13),   S(  0,  -7),   S(-10,  -9),   S( -5, -28),   S( 12,  17),   S(  5,  -8),   S(  0, -15),
            S( 11,  10),   S( -2, -29),   S(  5, -10),   S( -4, -19),   S(  0, -10),   S(  6,   6),   S( -3, -12),   S( -2, -12),
            S(  8,   5),   S(  7,  -5),   S( -5,  -3),   S(  1,   9),   S( 23,  24),   S( 25,  30),   S(  7,  20),   S(  7, -11),
            S( 16, -11),   S( 24,  15),   S( -1,  -7),   S( 19,  12),   S( 20,  17),   S(  5,  14),   S(  1, -19),   S( 13,   3),

            /* queens: bucket 10 */
            S( 15,   8),   S( 12,   8),   S(  0,  -8),   S( -5, -26),   S( -9, -29),   S( -8, -16),   S( -4, -26),   S( -4, -14),
            S(  6,   3),   S(-14, -21),   S( -5, -23),   S(-18, -53),   S( -3,  -9),   S( 11,   0),   S(-10, -27),   S( -5,  -7),
            S( -2,   2),   S(  3,   4),   S( -1,  -3),   S( -7, -17),   S(  0,   0),   S(  0,   0),   S(  3,  -3),   S(-11, -21),
            S( -3,  -9),   S(  4,   5),   S(  4,   3),   S(  9,   2),   S(  0,   0),   S(  0,   0),   S( -5, -14),   S(  1, -17),
            S( 11,  15),   S( 14,   5),   S(  3,  -5),   S( 31,  33),   S(  0,   2),   S( -1,  -1),   S(  2, -11),   S( 11, -25),
            S( -6, -10),   S(  7,   7),   S( 23,  26),   S( 11,  12),   S( 15,  14),   S( 15,  22),   S( 16,   9),   S( -3, -23),
            S(  9,   6),   S( 19,  27),   S( 19,  26),   S( 20,  16),   S( 10,  16),   S( 25,  13),   S( 14,   8),   S(  5,  -5),
            S(-11, -30),   S(  3,   6),   S( 21,   5),   S( -6,  -2),   S( 14,  14),   S(  2,   2),   S( 13,   8),   S(  8, -10),

            /* queens: bucket 11 */
            S(-11,  -4),   S( -3,  -1),   S( -8,  -9),   S(-19, -19),   S( -4, -15),   S(-19, -32),   S( -7, -31),   S( -7, -14),
            S( -6,   0),   S(  1,   8),   S(-24, -11),   S( -6,   3),   S( 21,   1),   S( -8, -24),   S(  8,  -1),   S( -5, -12),
            S(  3,   7),   S(  6,   2),   S(-19,  13),   S( -2,   3),   S( -1, -19),   S(-22, -29),   S(  0,   0),   S(  0,   0),
            S(  0,   2),   S( -7,  11),   S( -2,  12),   S( -1,   4),   S(  1,  -8),   S( -3,   6),   S(  0,   0),   S(  0,   0),
            S(  1,  12),   S( 15,  16),   S( 17,  25),   S(  4,  23),   S( 42,  46),   S( 18,  27),   S(  8,  -1),   S(-10, -28),
            S(  1,   4),   S(  1,   0),   S( -2,  11),   S( 12,  28),   S( 15,  19),   S(  1,   2),   S(  5, -10),   S(  6, -21),
            S(  3,   3),   S(  9,  12),   S( 16,  23),   S(  2,  13),   S( 18,  56),   S( 15,  11),   S(  4,   1),   S( 10,  -4),
            S(-17, -57),   S(  9,  12),   S( -7,  -7),   S(  4,  36),   S( 14,  29),   S( 11,   0),   S( -6,  -3),   S( 11,  -1),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  2,   2),   S(-14, -19),   S( -6,  -5),   S(-12, -21),   S( -2,  -4),   S( -3,  -3),
            S(  0,   0),   S(  0,   0),   S(  6,   3),   S( -9, -18),   S( -7,  -8),   S(-10, -21),   S( -9, -17),   S(  1,  -1),
            S( -6,  -9),   S(  5,   7),   S( -5,  -7),   S(-11, -35),   S( 17,  32),   S(  0,  15),   S( -2,  -6),   S(  8,   9),
            S( -8, -20),   S(  5,   3),   S(  8,  14),   S(  3,  13),   S(  1,   4),   S( -2,  10),   S( -3,  -2),   S( -3,  -8),
            S(-17, -29),   S(  3,   9),   S(  5,   2),   S(  6,   6),   S(  7,  30),   S( -5, -19),   S( -8, -16),   S( -1,   0),
            S(  1,  -6),   S( -4, -11),   S(  0, -12),   S(  4,   8),   S( -5,  -8),   S( -9,   0),   S(-11, -10),   S( -3,  -8),
            S( -9, -13),   S(  3,   5),   S( -6, -11),   S( 13,  11),   S( -1,   0),   S(-10, -15),   S(  0,   0),   S( -7, -26),
            S(  6,  12),   S(  0,  -3),   S(  2,  -6),   S(  0,   3),   S( -6,  -7),   S(-14, -13),   S( -6,   9),   S( -9, -15),

            /* queens: bucket 13 */
            S(-22, -34),   S(-15, -28),   S(  0,   0),   S(  0,   0),   S(-17, -30),   S(-12, -34),   S(  0,  -2),   S( -4, -10),
            S(-17, -45),   S( -6, -16),   S(  0,   0),   S(  0,   0),   S(-16, -36),   S(-22, -43),   S(-12, -21),   S( -4,  -6),
            S(-21, -38),   S( -4, -14),   S( -4,  -5),   S( -2, -13),   S(-21, -40),   S(-10, -15),   S( -8,  -6),   S( -1,  -4),
            S( -8, -19),   S(-19, -32),   S(  0,  -8),   S( -5, -17),   S( 11,   6),   S( 18,  32),   S( -4, -15),   S( -8, -10),
            S(  5, -10),   S(  0, -24),   S( -7, -20),   S( 12,  24),   S( -6, -10),   S( -1, -15),   S( -2,  -5),   S(  2, -12),
            S( -2,  -4),   S(-14, -19),   S(  4,   2),   S( 10,  23),   S(  1, -10),   S( -5,  -5),   S(-12, -22),   S(-10, -23),
            S(  0,   0),   S( -4, -10),   S( 11,  25),   S( -2,  -2),   S(  3,   2),   S(  8,   0),   S(-14, -25),   S( -7, -11),
            S( -8,  -6),   S( -2,  -7),   S( -6, -11),   S(  0,  -8),   S(  3,  -2),   S( -1,  -3),   S( -1,  -8),   S(-13, -20),

            /* queens: bucket 14 */
            S( -6, -15),   S(  0,  -9),   S( -9, -19),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S( -3,  -7),   S( -7, -22),
            S( -7, -23),   S(-26, -47),   S(-11, -25),   S( -3, -15),   S(  0,   0),   S(  0,   0),   S( -9, -23),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -6, -20),   S(-13, -25),   S( -3,  -4),   S(  2,   4),   S(-10, -16),   S(-17, -32),
            S( -9, -11),   S( -2,   0),   S(  0,   0),   S(-15, -21),   S( -7, -14),   S(-19, -28),   S( -3, -22),   S(  1,   1),
            S( -6, -12),   S( -5, -11),   S( -4, -15),   S(  5,   9),   S(  6,  18),   S(-10, -25),   S( -9,  -5),   S( -1,  -2),
            S( -6, -13),   S(  3,  -3),   S(-12, -20),   S(-13, -22),   S(  6,  10),   S(  1,   5),   S( -1,  -5),   S(-10, -11),
            S(-10, -16),   S( -2,  -8),   S(  0,   0),   S(  3,   6),   S(  2,   4),   S(  3,   5),   S( -8, -20),   S( -3,  -8),
            S(-11, -17),   S(  5,  -5),   S(-10, -14),   S( -4,  -9),   S(  3,   2),   S( -3,  -3),   S( -4,  -2),   S(  2,  -7),

            /* queens: bucket 15 */
            S(  1,   3),   S( -7, -18),   S(  3,  -1),   S(-11, -18),   S(  4,   6),   S(-10, -10),   S(  0,   0),   S(  0,   0),
            S( -5,  -5),   S(  1,   6),   S(-13, -16),   S( -8, -16),   S(  0,  -6),   S(  2,   6),   S(  0,   0),   S(  0,   0),
            S( -1,  -1),   S(  1,   0),   S(-12,  -3),   S( -6,  -6),   S( -9, -21),   S(  5,   5),   S( -1,   2),   S( -1,  -4),
            S( -2,  -4),   S(-10, -14),   S( -3,  -5),   S(  1,   7),   S(  9,  27),   S(  8,  28),   S( -3,   5),   S( -4, -16),
            S(  1,   3),   S(  1,   1),   S( -4,  -8),   S(  0,  -1),   S( 11,  52),   S(  4,  20),   S(  3,  11),   S( -6, -16),
            S( -1,  -3),   S( -3,  -1),   S( -3,  -8),   S( -6,  -1),   S( -2,   5),   S( -9,  -8),   S(  2,  12),   S( -8,  -6),
            S( -5, -12),   S(  0,   0),   S( -5,   4),   S(  3,   3),   S( -7,  -9),   S(  1,   6),   S(  5,  12),   S( -5, -10),
            S( -8, -18),   S(-14, -31),   S( -1, -10),   S(  2,   2),   S(-13,  -3),   S( -3,  -1),   S(  1,  -1),   S( -3,   5),

            /* kings: bucket 0 */
            S(-11, -20),   S( 29,  -9),   S( 16,  -4),   S(-27,  14),   S( -8,  14),   S( 30, -26),   S(  4,   1),   S( 10, -49),
            S(-19,  32),   S( -1,  -1),   S( -3,   5),   S(-44,  24),   S(-43,  43),   S(-15,  22),   S(-14,  37),   S( -5,  28),
            S( 13,   3),   S( 65, -30),   S(  1,  -1),   S(-20,   2),   S(-32,   5),   S(  1,  -5),   S(-31,  17),   S( 30, -29),
            S(-26, -26),   S(  5, -28),   S(  4, -25),   S(-23,   6),   S(-49,  33),   S(-48,  28),   S(-39,  38),   S(-16,  31),
            S(-49, -123),  S( -4, -46),   S( -4, -31),   S( 14, -25),   S(-48,  -5),   S(-28,   9),   S(-23,  13),   S(  2,  -9),
            S(-10, -120),  S(  1,  10),   S(-10, -52),   S(-14,  -7),   S( -2, -11),   S(-26,  21),   S( 16,  24),   S(-19,   7),
            S(  0,   0),   S(  0,   0),   S(  0, -49),   S(  4, -35),   S(-19,  -4),   S(-12, -14),   S(-27,   6),   S( -9,  -3),
            S(  0,   0),   S(  0,   0),   S(-12, -10),   S(  1, -10),   S(  9,  -2),   S( -5,  15),   S(  7,   4),   S(  9,   0),

            /* kings: bucket 1 */
            S(  7, -26),   S( 32, -23),   S( 15, -16),   S( 27,  -3),   S(  0,  -1),   S( 33, -20),   S(  6,   3),   S( 19, -24),
            S( 10,  -1),   S(  6,  10),   S( -2,  -7),   S(-44,  26),   S(-32,  22),   S(-13,  15),   S( -6,  18),   S(  3,   9),
            S( -8, -16),   S(  0, -14),   S(  6, -17),   S( 14, -19),   S(-34,   1),   S( 18, -19),   S( 24, -12),   S( 38, -13),
            S( -1,  -1),   S( -1,  -9),   S(  3,  -4),   S( -6,   7),   S(  5,  12),   S(-11,   1),   S( 31,  -6),   S(-21,  27),
            S(-18, -55),   S(-18, -42),   S(-11, -52),   S(-16, -41),   S( -3, -22),   S( -2, -30),   S( -8,  -4),   S( -6,  -4),
            S(-31,   0),   S(-104,   6),  S(-33,  28),   S(  2,  20),   S(-42,   6),   S(-25,  14),   S( 15,   3),   S( -6,  -8),
            S(-35, -50),   S(-23,   5),   S(  0,   0),   S(  0,   0),   S(-42,  15),   S(-53,  29),   S( -5,  29),   S( -3, -32),
            S(-29, -109),  S(-13, -15),   S(  0,   0),   S(  0,   0),   S( -8,   8),   S(-13,  16),   S( -2,  21),   S( -4, -47),

            /* kings: bucket 2 */
            S( 15, -58),   S(  9,  -4),   S( 17, -21),   S( 16, -10),   S(  0,   7),   S( 35, -24),   S( -2,  14),   S( 21, -28),
            S( 33, -36),   S(-17,  31),   S(-16,   8),   S(-17,   8),   S(-25,  15),   S(-12,   5),   S(  3,   2),   S(  2,   0),
            S(-31,  -5),   S(-19, -12),   S( -6, -13),   S(-11, -17),   S( -9,  -5),   S(  6, -19),   S( 28, -18),   S( 26, -17),
            S( 15,  11),   S(-20,  16),   S(  5,   0),   S(-29,  14),   S( 27,  -5),   S(-15,  -9),   S( 31, -27),   S( 32, -10),
            S( -3, -12),   S( 16, -16),   S( 26, -40),   S(  3, -27),   S( 30, -48),   S(-24, -41),   S( 20, -48),   S(  9, -46),
            S(  3,   8),   S( -9,  -6),   S(-35,  -2),   S(-39, -10),   S(  3,   1),   S(-12,  24),   S(-83,  10),   S(-17, -20),
            S( -8, -11),   S( -8,  21),   S(-75,  12),   S(-17,  11),   S(  0,   0),   S(  0,   0),   S(-13,  19),   S(-37, -34),
            S( -7, -39),   S(-19, -28),   S(-30, -34),   S( -6,  10),   S(  0,   0),   S(  0,   0),   S(-10, -12),   S(-33, -119),

            /* kings: bucket 3 */
            S( -4, -53),   S( 18, -10),   S( 29, -24),   S( -5,  -7),   S( -1, -12),   S( 35, -25),   S(  1,  13),   S(  6, -28),
            S(  2,  17),   S(-20,  39),   S(-19,   6),   S(-37,  18),   S(-54,  32),   S(  0,   0),   S( -7,  19),   S(  1,  13),
            S( 16, -27),   S(  1,  -3),   S( -1,  -9),   S(-35,   1),   S(-13,   8),   S( 22, -19),   S( 49, -20),   S( 54, -17),
            S(-18,  30),   S(-91,  44),   S(-54,  16),   S(-49,  15),   S(-35,  11),   S(-14, -21),   S(-37,  -3),   S(-33, -15),
            S(-13,   7),   S(-11,  -5),   S(-34, -12),   S(-24, -15),   S( 34, -47),   S( 51, -65),   S( 33, -69),   S( 10, -81),
            S(-10, -14),   S( 18,   6),   S( 21, -11),   S(  0, -22),   S( 48, -34),   S( 56, -48),   S( 71, -20),   S( 51, -114),
            S(-20, -10),   S( 25,   8),   S( 15, -14),   S( 30, -23),   S( 31, -29),   S( 26, -54),   S(  0,   0),   S(  0,   0),
            S( -3, -11),   S(  6,   9),   S( -2,  18),   S( 13, -11),   S( 10, -72),   S( -2,  11),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-57,   6),   S(  7,  35),   S( 10,  21),   S( 12,   1),   S( -8,   7),   S(  9, -11),   S(  4,   7),   S( 21, -36),
            S(-35,  22),   S( 23,  19),   S( -7,  18),   S( -7,   3),   S( 32,  -2),   S( 23,  -3),   S( 53, -14),   S( 13,  -3),
            S(  0,  26),   S( 11, -13),   S( 19,  -4),   S( -9,   3),   S(-20,  10),   S( 20, -20),   S(-38,   8),   S( 15, -12),
            S(  0, -22),   S(-11,   9),   S(  5,  16),   S(  7,   4),   S(-19,  10),   S(-13,  18),   S( 17,  10),   S( 10,   7),
            S(  0,   0),   S(  0,   0),   S( -1,   1),   S(-27,  13),   S(-36,  15),   S(-27, -14),   S(-20,   2),   S( -3,  -1),
            S(  0,   0),   S(  0,   0),   S(  6, -15),   S( -3,  25),   S(-12,  27),   S(-29, -10),   S(  4, -14),   S(  0,  16),
            S( -3, -20),   S( -4,  -7),   S( -4, -23),   S(  0,  22),   S( -5,  25),   S(-29,  -7),   S(-11,  21),   S(  4,  -4),
            S( -5, -21),   S(  3, -13),   S(-10, -20),   S( -8,   3),   S(  6,  11),   S( -7, -11),   S( -6,   1),   S(  4,  10),

            /* kings: bucket 5 */
            S( 33,  -4),   S(-10,  12),   S(-34,  23),   S(-43,  28),   S(-18,  26),   S(  0,  12),   S( 35,  -3),   S( 29,  -9),
            S(  0,   0),   S( 15,  10),   S( 28,  -6),   S( 24,  -6),   S( 17,  -4),   S( 39, -12),   S( 28,   4),   S( 47, -17),
            S(-12,   8),   S( -7,  -7),   S(-12,  -6),   S( -3,  -7),   S(  7,  -3),   S(-40,   1),   S( -4,   2),   S( 18,  -4),
            S( -4, -12),   S(  0,  -8),   S(  9,  -5),   S(  7,  17),   S(  5,  20),   S(  6,   4),   S( 14,   7),   S(  7,   4),
            S( -4, -29),   S(-31, -44),   S(  0,   0),   S(  0,   0),   S( -7,  -4),   S(-23, -13),   S(  2, -14),   S( -9,   5),
            S( -7, -38),   S(-26, -27),   S(  0,   0),   S(  0,   0),   S(-22,  36),   S(-57,  13),   S(-18,  -4),   S( -6,  -4),
            S(-16, -32),   S(-32,  21),   S(  1,  11),   S( -1, -17),   S(-28,  29),   S(-41,  20),   S( -2,  10),   S(  9,  17),
            S(-10, -100),  S( -8,  11),   S(-10, -26),   S( -2, -36),   S(-10, -18),   S( -7,   6),   S( -3, -16),   S(  0,   5),

            /* kings: bucket 6 */
            S( 38, -36),   S( 30, -14),   S( -3,   2),   S(-20,  22),   S( -7,  19),   S(-21,  20),   S(  0,  20),   S(  8,   2),
            S( 48, -28),   S( 12,  16),   S( 16,  -7),   S( 25,  -9),   S( 25,  -5),   S( -9,  12),   S( 15,   1),   S(  5,   2),
            S( 17, -19),   S(-24,   2),   S(-15,  -8),   S( -2,  -6),   S( 16, -13),   S(-45,   6),   S(  9,  -2),   S(-17,  13),
            S( 13,   5),   S( 26,  -5),   S( 16, -13),   S( 26,   5),   S( 59,   0),   S(-28,   5),   S( -6,   7),   S(  6,  -1),
            S(  8, -19),   S( 17, -30),   S(-23, -12),   S(  2, -18),   S(  0,   0),   S(  0,   0),   S(-46, -19),   S(-41, -17),
            S(-16,   0),   S(  1,   0),   S(-31,  -2),   S( -9, -22),   S(  0,   0),   S(  0,   0),   S(-27, -13),   S(-31, -21),
            S( -1,  -9),   S(-10,   7),   S(-41,  11),   S(-16,  -2),   S(  3,   3),   S(-11, -30),   S(-29, -13),   S( -9, -38),
            S( -1,  -7),   S(  2,  -8),   S( -4,   9),   S(-15, -27),   S( -8, -37),   S( -5, -25),   S( -6,  -2),   S( -1, -58),

            /* kings: bucket 7 */
            S( 31, -34),   S( -6,  -4),   S(-26,  -3),   S(-13,   9),   S(-26,  11),   S(-42,  35),   S(-28,  33),   S(-37,  21),
            S( 12,  -1),   S( 22, -21),   S( -3,  -8),   S(-32,   7),   S(-12,   6),   S(-36,  23),   S(  3,  -2),   S( -4,  14),
            S( 29, -29),   S(-17,  -8),   S(-31,  -2),   S(-34,   1),   S(-43,   7),   S(-32,  14),   S( 14,  -3),   S(-46,  21),
            S(-24,  17),   S(  7,   8),   S( -4,  -2),   S( 36,  -7),   S( 37, -11),   S( 47, -26),   S( 16,  -9),   S( 19, -11),
            S(-15,  15),   S( -4,   0),   S(  4, -26),   S(  9, -18),   S( 18, -27),   S(  9, -21),   S(  0,   0),   S(  0,   0),
            S(-10, -32),   S(  0,  -8),   S( 15, -11),   S( 13,  -6),   S( 26, -11),   S( 17, -10),   S(  0,   0),   S(  0,   0),
            S( 14,  17),   S( -2, -20),   S(  1,   5),   S(-13, -12),   S(  9, -18),   S( -5, -29),   S(  4, -16),   S(-11,  12),
            S(  8,   7),   S( -7,  -9),   S( 11,  21),   S( -3,  -5),   S(  8,  16),   S(-17, -49),   S(  9, -12),   S(-10, -56),

            /* kings: bucket 8 */
            S( 16, 115),   S( -5,  84),   S( 41,  37),   S( -2,  -1),   S(-14,  12),   S(-15,  -4),   S( 31, -15),   S(-14, -19),
            S( 30,  70),   S( 23,  14),   S( 48,  59),   S( 84,  -2),   S( 17,  24),   S(  5,  -6),   S( -4,  11),   S(  2,  26),
            S(  0,   0),   S(  0,   0),   S( 29,  63),   S( 40,   6),   S( 20,   5),   S( -9,  -5),   S( -2,  15),   S(  9, -16),
            S(  0,   0),   S(  0,   0),   S(  3,  71),   S( -5,   2),   S(-15,  34),   S( -6,  19),   S( 14,  10),   S( 10,  16),
            S( -3, -26),   S( -1,  26),   S(  3,  11),   S(-14,  25),   S(-16,  -4),   S(  4, -14),   S(  1,  11),   S(-14, -26),
            S(  5,  14),   S( -1, -15),   S( -4, -12),   S( -7,   3),   S(-13,   0),   S(-11,  -1),   S( -9,  -2),   S(  8,  -7),
            S( -5, -14),   S( -8, -11),   S(  4,   8),   S( -1, -10),   S( -2, -33),   S(-10,   7),   S( -2,  -1),   S(  5, -46),
            S( -7,  -9),   S(-13, -26),   S( -2, -11),   S( -6, -21),   S(  7,   8),   S( -5,   1),   S(  1,  -5),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  7,  24),   S(-13,  34),   S(-18,  57),   S( 19,   8),   S(-15,  31),   S(-27,  30),   S( 40,   4),   S( 20,  13),
            S(-18,  33),   S( 36,  23),   S(  6,  -1),   S( 50,   2),   S( 59,  16),   S( 23,   8),   S( -5,  29),   S(-16,  13),
            S( -6,  12),   S( 23,  14),   S(  0,   0),   S(  0,   0),   S( 45,  17),   S( -3,   5),   S(  6,   1),   S(-20,  22),
            S( -2, -29),   S( 12, -22),   S(  0,   0),   S(  0,   0),   S(  8,  32),   S( 13,   2),   S(-11,  11),   S(-16,  30),
            S(  4, -20),   S( 12,  -3),   S(  4,  18),   S(  1,  11),   S(-14,  15),   S(-21,  17),   S( -9,  14),   S( -1, -14),
            S(  5,   2),   S(  1,  -7),   S(  6,  -8),   S(-11, -21),   S(-11,  10),   S( -1,  10),   S(-33,   2),   S(  5,  31),
            S(  2,  -6),   S( -3, -20),   S( -2,  -7),   S(  2, -32),   S( 14, -27),   S( 14,  16),   S(-17,  -8),   S(  4,   4),
            S(  6,   5),   S( -2, -22),   S( 10, -23),   S( -5, -22),   S( -1, -20),   S(  3,   8),   S( -6,  12),   S(  8,  -1),

            /* kings: bucket 10 */
            S( 34,  -3),   S(  1,  -7),   S(  5,  10),   S(  7,  22),   S(-11,  18),   S(-92,  50),   S(-32,  46),   S(-84,  81),
            S(  5,  -2),   S( 63,  -1),   S( 25,  -5),   S( 33,   8),   S( 61,  10),   S( 46,   6),   S( 11,  28),   S(-85,  49),
            S( 17,   5),   S( 28,  -1),   S( 28, -10),   S( 16,  11),   S(  0,   0),   S(  0,   0),   S(-10,  23),   S(-60,  29),
            S( 16,   6),   S( 44, -28),   S( 35, -31),   S( 31,   1),   S(  0,   0),   S(  0,   0),   S( -1,  14),   S(  4,   3),
            S(  3,   6),   S( 27,   5),   S( 30, -20),   S( 10, -32),   S(  4, -18),   S(  6,  27),   S(  9,   9),   S(-10,  16),
            S(  3,  14),   S(  3,  -7),   S( -4,   8),   S( 10,  -7),   S(  7,  -3),   S(-17,  -6),   S(-14,   6),   S( -1,  -8),
            S(  0, -43),   S( -4, -15),   S(  9,  -9),   S( 13,   1),   S( 12,  -4),   S(-10, -16),   S(  5, -28),   S(  5,   3),
            S(  4,   6),   S( 11,  -9),   S( -1, -17),   S(  1,   2),   S(  5, -15),   S(  0, -29),   S( -5,  -7),   S(  9,   3),

            /* kings: bucket 11 */
            S( -6, -19),   S(  9,   8),   S(  6,  -8),   S( -6,  14),   S( -7,   7),   S(-67,  57),   S(-71,  81),   S(-123, 147),
            S( -2, -27),   S( 22,   3),   S(-10, -16),   S( 15,  20),   S( 83,   1),   S( 59,  40),   S(  9,  22),   S( 26,  40),
            S(  3, -51),   S( -2,  18),   S( -1, -10),   S( 24,  10),   S( 62,   1),   S( 26,  61),   S(  0,   0),   S(  0,   0),
            S(  0,  18),   S( 18,  13),   S( -3,   4),   S( 11,  13),   S( 32, -10),   S( 23,  20),   S(  0,   0),   S(  0,   0),
            S(  0,  33),   S(  2,  -5),   S(  8,  -7),   S( 15, -19),   S( 17,   2),   S( -1,  -2),   S(  9,   9),   S(  6,   2),
            S( 11,  10),   S(  0, -15),   S( 17, -11),   S(  0,   4),   S( -5,  -7),   S(  3, -17),   S( -4,  -8),   S(-11,  -3),
            S(  6,  12),   S(  8,  -7),   S( 18,  24),   S(  1, -26),   S( 18, -18),   S(  4,   2),   S(-11, -14),   S( -8, -14),
            S(  5,   7),   S(  5,   0),   S(-11, -22),   S(  5,  -9),   S( -4, -20),   S( -8, -19),   S(  0, -20),   S(  5,  12),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 19,  56),   S(  7,  -7),   S(  1,  -3),   S(  7,  15),   S(  7,  -1),   S(-20,   7),
            S(  0,   0),   S(  0,   0),   S( 46, 107),   S( 29,  14),   S( 22,  42),   S( 13,   0),   S( 23,  -6),   S(-19,   2),
            S( -1,   8),   S(  3,  12),   S( 22,  68),   S( 39,  21),   S(  8,  -9),   S( 11,   3),   S(  0,  -9),   S(-10,  -2),
            S( -2,  10),   S( 10,  31),   S( -1,  17),   S(  5,  -4),   S( -8,   1),   S( -1,  20),   S( -4,  10),   S(  1,   8),
            S( 10,  18),   S(  6,  23),   S( 10,  18),   S( -2,  40),   S( -3,  36),   S(  0,   4),   S( -9,  15),   S(-12, -12),
            S(  6,   5),   S( 10,  15),   S( -2,  -1),   S(-10, -15),   S( -1,   4),   S( -7,  18),   S( -9, -14),   S(  6,  -2),
            S(  4,   8),   S( -7, -13),   S( -2,   5),   S( -1,   1),   S( -5,  -9),   S(  4,   8),   S(  8,  41),   S( -1, -29),
            S( -2,   2),   S(  5,   3),   S( -3,   7),   S(  0,   2),   S( -1,  -6),   S(  3,   7),   S(-11, -22),   S( -2, -22),

            /* kings: bucket 13 */
            S(  0,  51),   S(  7,  32),   S(  0,   0),   S(  0,   0),   S( 42,  15),   S( 14, -11),   S( -4,  -6),   S(-18,  25),
            S(  3,  21),   S( -1,   0),   S(  0,   0),   S(  0,   0),   S( 46,   5),   S( 28,  -7),   S(-21,   8),   S(-15,   5),
            S( -3,   2),   S( 19,  23),   S(  2,  -7),   S( 14,  40),   S( 51,  11),   S( 23,  -6),   S(  3,   6),   S( 12, -10),
            S(-10,  -5),   S( 15,  -2),   S(  1,  21),   S( -5,  14),   S( -3,  14),   S(  4, -11),   S(  4,  20),   S(-16, -26),
            S(  6,  12),   S( -1,   7),   S(  5,  43),   S( -4,  24),   S( -9,   9),   S(  5,  19),   S(-11,   2),   S(  7,  10),
            S(  4,   0),   S( -5,  17),   S( -2,  17),   S( -4,  -1),   S(-12, -15),   S( -5,   9),   S( -9,  20),   S(  1,   2),
            S(  9,  11),   S( -9, -21),   S(-11, -43),   S(  3,  19),   S(-11, -10),   S(-10,  15),   S(-14, -24),   S(  6,  13),
            S(  1,  -2),   S(  5,  -2),   S(  4,  20),   S(  4,   5),   S(  0,  17),   S(-11, -16),   S( -3,   9),   S(  8,  14),

            /* kings: bucket 14 */
            S( 19,  34),   S(  0,  -7),   S( 11, -43),   S( 15,   0),   S(  0,   0),   S(  0,   0),   S(  6,  72),   S(-39,  36),
            S(-10, -11),   S( 19,  -8),   S( 49, -34),   S( 41,   9),   S(  0,   0),   S(  0,   0),   S( 13,  33),   S(-44,   5),
            S(  4,   4),   S( 16,  -6),   S( 34, -31),   S( 40,   4),   S( 10,  -3),   S( 14,  36),   S( 26,  56),   S(-29,   2),
            S(  7,  -5),   S(  8,  -9),   S( -1,  -9),   S( 12,  -2),   S(-19,  -1),   S( 15,  55),   S(  3,  24),   S(  5,  -2),
            S(  7,  19),   S(  8,   0),   S( -9,   3),   S(-18,  13),   S(  1,  29),   S(  5,  55),   S(  2,  39),   S(  5,  13),
            S( -6,  -7),   S(  2,   6),   S( -2,  -2),   S( -1,  10),   S( -6, -19),   S( -6,  -2),   S(-15,  -5),   S( -2,   8),
            S(  4,   9),   S(-10, -13),   S( 11,  -6),   S( 16,   3),   S(  3,  -4),   S( -7,  18),   S(-27, -21),   S(  8,  17),
            S(  1,  12),   S(  5, -10),   S(  9,   2),   S( -4,  -6),   S(  7,  -9),   S( -3,  -6),   S(-13, -26),   S(  0,  -8),

            /* kings: bucket 15 */
            S( 12,  32),   S(  5,  -3),   S( 11,  -7),   S( -8,  -2),   S(-10, -13),   S(  0,  56),   S(  0,   0),   S(  0,   0),
            S( -3, -22),   S(  7, -13),   S( -7, -14),   S( 19,  48),   S( 41,  -2),   S( 62, 108),   S(  0,   0),   S(  0,   0),
            S(-10, -23),   S( 17,  -9),   S(  7, -16),   S( -3,  12),   S( 10,  -5),   S( 26,  71),   S(  8,  43),   S(-14,  -1),
            S( -1, -12),   S(  3,  16),   S(  4,  15),   S(-12, -29),   S(-12,  -2),   S( 21,  46),   S( 17,  45),   S( -3, -15),
            S( 10,   8),   S( -8,  25),   S(  1,  -3),   S( -6, -37),   S( -3,   9),   S(  2,  35),   S(  4,   7),   S(  4,   4),
            S(  5,  26),   S(-15,  -4),   S(  8,  16),   S(  8,  18),   S( -9, -24),   S( -2,   8),   S(  1,   8),   S(  4,  18),
            S(  8,  12),   S( -4,  24),   S( -2, -12),   S(  3,   6),   S(  9,   7),   S(  9,  13),   S( -5,  -3),   S(  2,   1),
            S( -2,  -8),   S(  4,   3),   S( -2, -10),   S(  4,   4),   S(  5,   5),   S( 10,  13),   S(  1,  -6),   S(  3,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-28,  64),   S(-36, -61),   S( -4,  27),   S( 11,  87),   S( 22, 113),   S( 27, 148),   S( 38, 151),   S( 48, 156),
            S( 65, 139),

            /* bishop mobility */
            S(-62,  95),   S(-39, -49),   S(  1,   7),   S(  7,  75),   S( 22, 105),   S( 31, 130),   S( 35, 150),   S( 43, 157),
            S( 45, 165),   S( 55, 163),   S( 61, 161),   S( 85, 141),   S(102, 135),   S( 70, 133),

            /* rook mobility */
            S(-63,   8),   S(-146, 119),  S(-18,  35),   S(-12, 106),   S(-11, 138),   S( -9, 161),   S( -7, 178),   S(  0, 184),
            S(  8, 186),   S( 14, 196),   S( 19, 201),   S( 28, 202),   S( 42, 203),   S( 55, 199),   S( 96, 173),

            /* queen mobility */
            S(111, 165),   S( 36, 309),   S( 25, 260),   S( 10, 212),   S( 49,  67),   S( 56,  44),   S( 58, 144),   S( 56, 209),
            S( 54, 262),   S( 54, 289),   S( 54, 316),   S( 58, 331),   S( 59, 351),   S( 63, 355),   S( 64, 366),   S( 64, 371),
            S( 65, 373),   S( 64, 376),   S( 69, 369),   S( 76, 358),   S( 87, 342),   S(120, 303),   S(131, 288),   S(151, 257),
            S(185, 229),   S(190, 205),   S(138, 201),   S(113, 145),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,  14),   S(-20,  48),   S(-32,  47),   S(-38,  61),   S(  7,  15),   S(-17,  19),   S(-11,  65),   S( 23,  30),
            S( 15,  34),   S( -3,  47),   S(-18,  47),   S(-20,  39),   S( -6,  37),   S(-34,  47),   S(-37,  63),   S( 29,  30),
            S( 21,  70),   S( 11,  74),   S(  3,  57),   S( 21,  47),   S( -5,  54),   S(-33,  70),   S(-41, 103),   S( -9,  80),
            S( 30, 109),   S( 40, 121),   S( 23,  80),   S( 10,  60),   S(  3,  67),   S( -2,  91),   S(-48, 128),   S(-76, 153),
            S( 23, 153),   S( 52, 185),   S( 61, 133),   S( 32, 115),   S(-54, 107),   S( 20, 110),   S(-58, 174),   S(-87, 172),
            S( 91, 233),   S( 80, 270),   S(127, 243),   S(129, 253),   S(135, 263),   S(152, 243),   S(128, 254),   S(132, 264),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,   3),   S( -7, -26),   S( -3,  -9),   S( -3,   2),   S( 17,  12),   S(-15, -40),   S(-22,   9),   S( -7, -46),
            S(-17,  16),   S( 18, -16),   S( -3,  28),   S( 11,  24),   S( 30,  -7),   S( -7,  17),   S( 20, -13),   S( -4,  -7),
            S(-18,  17),   S( 21,   5),   S( -1,  45),   S( 16,  55),   S( 25,  28),   S( 28,  21),   S( 31,   0),   S( -3,  16),
            S( 15,  35),   S( 14,  54),   S( 41,  91),   S( 15, 100),   S( 69,  68),   S( 68,  56),   S( 18,  62),   S( 24,  22),
            S( 49,  96),   S( 88, 118),   S(102, 140),   S(139, 165),   S(135, 135),   S(134, 149),   S(129, 127),   S( 51,  62),
            S( 72, 193),   S(117, 278),   S(102, 222),   S( 95, 197),   S( 66, 152),   S( 48, 141),   S( 41, 143),   S( 16,  78),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 13,  21),   S( 17,  21),   S( 33,  33),   S( 31,  25),   S( 20,  21),   S( 26,  22),   S(  5,  11),   S( 41,  -3),
            S( -4,  22),   S( 15,  35),   S( 11,  36),   S( 10,  42),   S( 25,  16),   S(  9,  23),   S( 31,  20),   S( -1,  13),
            S(  0,  22),   S( 30,  49),   S( 52,  57),   S( 41,  58),   S( 46,  54),   S( 69,  20),   S( 31,  36),   S( 20,   6),
            S( 58,  73),   S(102,  59),   S(121, 125),   S(148, 128),   S(140, 118),   S( 77, 132),   S( 71,  60),   S( 76,   9),
            S( 44, 125),   S( 89, 145),   S(153, 212),   S(104, 253),   S(133, 266),   S( 82, 242),   S(158, 211),   S(-54, 174),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26,  32),   S( 11,  18),   S( 10,  33),   S(-12,  62),   S( 67,  22),   S( 21,  10),   S(  0,   2),   S( 30,  12),
            S(  2,  14),   S(  7,   8),   S( 17,  18),   S( 15,  30),   S( 12,  17),   S(  0,   9),   S(  6,   6),   S( 28,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -14),   S( -7,  -8),   S(-17, -18),   S(-15, -30),   S(-12, -17),   S(  0,  -9),   S( -6,  -6),   S(-28,   4),
            S(-26, -32),   S(-11, -18),   S(-10, -33),   S( 12, -62),   S(-67, -22),   S(-21, -10),   S(  0,  -2),   S(-30, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -39),   S(-14, -42),   S(-16, -48),   S(-63, -35),   S(-26, -46),   S(-28, -48),   S( -8, -49),   S(-25, -61),
            S(-28, -22),   S(-20, -30),   S(-33, -14),   S( -9, -33),   S(-41, -33),   S(-29, -26),   S(-39, -20),   S(-15, -41),
            S(-20, -19),   S( -8, -36),   S(-26, -12),   S(-33, -24),   S(-22, -42),   S(-24, -21),   S(-10, -23),   S(-42, -30),
            S( -9, -32),   S( 19, -46),   S( 13, -20),   S(  9, -31),   S( 10, -30),   S( 58, -44),   S( 42, -46),   S(-13, -54),
            S( 12, -48),   S( 39, -73),   S( 45, -28),   S( 59, -31),   S( 74, -49),   S( 80, -36),   S(133, -92),   S( 32, -81),
            S( 93, -98),   S(124, -108),  S( 86, -47),   S( 69, -30),   S( 63, -29),   S(115, -41),   S(100, -46),   S( 43, -84),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-13, -24),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  5,   1),        // attacks to squares 1 from king
            S(  9,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 66, -64),        // king-side castling available
            S( 16,  60),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 28, -88),   S( 34, -75),   S( 27, -86),   S( 26, -75),   S( 17, -65),   S( 12, -59),   S(  1, -51),   S( -7, -44),
            S(  2, -44),   S( 13, -41),   S( 43, -46),   S( 39, -41),   S( 91, -50),

            /* orthogonal lines */
            S(-65, -138),  S(-92, -120),  S(-113, -98),  S(-125, -93),  S(-132, -90),  S(-139, -91),  S(-138, -97),  S(-134, -103),
            S(-149, -92),  S(-160, -89),  S(-165, -96),  S(-136, -122), S(-94, -139),  S(-37, -157),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 29, 231),

            /* passed pawn can advance */
            S(-10,  34),   S( -3,  61),   S( 15, 104),   S( 82, 170),

            /* blocked passed pawn */
            S(  0,   0),   S( 52, -26),   S( 29,  -5),   S( 33,  32),   S( 32,  63),   S( 23,  40),   S( 68,  82),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 49, -48),   S( 38,  16),   S( 22,  27),   S( 19,  59),   S( 30,  98),   S(134, 133),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-13, -16),   S( -6, -31),   S(  7, -25),   S(-17,  -2),   S(-22,  31),   S(126,  18),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 28, -16),   S( 28, -18),   S( 12,  -5),   S( 12, -36),   S( -6, -107),  S(-28, -193),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 24,  50),   S( 50,  25),   S(106,  43),   S( 34,  25),   S(177, 113),   S(110, 127),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 16,  57),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-51, 115),

            /* bad bishop pawn */
            S( -9, -18),

            /* rook on open file */
            S( 30,   6),

            /* rook on half-open file */
            S(  8,  44),

            /* pawn shields minor piece */
            S( 12,  14),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 21,  32),   S( 23,   1),   S( 34,  21),   S( 28,  -2),   S( 35, -21),

            /* pawn threats */
            S(  0,   0),   S( 67, 106),   S( 50, 119),   S( 73,  89),   S( 61,  41),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 35,  67),   S( 51,  50),   S( 76,  43),   S( 49,  71),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 27,  53),   S( 28,  49),   S(-16,  42),   S( 65,  73),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 21,  10),   S( 20,  33),   S( 35,  13),   S(  8,  29),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  15),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
