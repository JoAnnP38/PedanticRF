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
        public const int MAX_WEIGHTS = 12795;
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
        public const int BISHOP_LONG_DIAG = 12763;  // bishop on long diagonal
        public const int PAWN_PUSH_THREAT = 12764;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12770;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12776;      // minor piece threat
        public const int ROOK_THREAT = 12782;       // rook threat
        public const int CHECK_THREAT = 12788;      // check threat against enemy king
        public const int TEMPO = 12794;             // tempo bonus for side moving

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

        public Score BishopLongDiagonal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[BISHOP_LONG_DIAG];
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

        // Solution sample size: 16000000, generated on Wed, 24 Apr 2024 14:05:36 GMT
        // Solution K: 0.003850, error: 0.082255, accuracy: 0.5140
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 73, 226),   S(386, 670),   S(412, 661),   S(542, 1074),  S(1381, 1800), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(105, -122),  S(147, -90),   S( 41, -43),   S(-23,  24),   S(-30,  12),   S(-22,  -2),   S(-49,   2),   S(-26, -18),
            S(123, -126),  S(105, -105),  S(  9, -63),   S(-11, -54),   S(-18, -20),   S(-20, -28),   S(-35, -24),   S(-21, -43),
            S(111, -102),  S( 63, -61),   S( 14, -64),   S( 13, -68),   S( -9, -60),   S(  7, -59),   S(-10, -52),   S(  9, -55),
            S( 72, -39),   S( 51, -56),   S( 27, -60),   S( 18, -82),   S(-13, -44),   S(-13, -55),   S(-17, -42),   S( -3, -27),
            S( 76,  40),   S( 31,  -7),   S( 38, -28),   S( 52, -72),   S( 23, -46),   S( -5, -43),   S(-21,  -7),   S(-28,  50),
            S( 63,  62),   S( 49,  80),   S(  4,   9),   S( 17, -17),   S(-42,  -2),   S(  7,   2),   S( -3,  20),   S( 16,  47),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 34, -30),   S( 33, -38),   S( 53, -23),   S(  3,  21),   S(-14,  -4),   S(  5, -11),   S(-42,   3),   S(-28,  21),
            S( 36, -45),   S( 25, -45),   S( 12, -46),   S( -3, -41),   S( -9, -23),   S( -8, -28),   S(-33, -16),   S(-37, -11),
            S( 32, -40),   S( 11, -29),   S( 15, -53),   S( 13, -55),   S(-22, -24),   S( 15, -48),   S( -8, -33),   S(  6, -26),
            S( 46, -23),   S( 20, -51),   S( 25, -55),   S(  5, -50),   S(-13, -22),   S( 14, -45),   S(-23, -26),   S( -4,   1),
            S( 28,  45),   S(-31,   0),   S( -6, -36),   S( 11, -49),   S( 39, -38),   S( -5,  -8),   S(-24,  20),   S(-20,  68),
            S( 56,  59),   S( 15,   2),   S(-48, -18),   S(-21,  25),   S(-20,  -7),   S(-58,  26),   S(-46,  29),   S(-35,  86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  -4),   S(-21,   1),   S( -6,  -2),   S( -7,  12),   S(  7,  -5),   S( 36, -18),   S(  9, -43),   S(  1, -18),
            S( -3, -28),   S(-25, -17),   S(-18, -36),   S(-14, -35),   S(  7, -33),   S(  9, -31),   S(  0, -41),   S(-18, -27),
            S( -4, -27),   S(-19, -30),   S( -7, -55),   S(  1, -55),   S( -4, -30),   S( 24, -44),   S(  5, -40),   S( 14, -32),
            S( -7, -12),   S( -9, -49),   S(-11, -54),   S( -4, -56),   S(  8, -47),   S(  5, -31),   S(  2, -24),   S(  7,  -9),
            S(  2,  31),   S(-41, -11),   S(-41, -44),   S(-41, -36),   S( 13,  -8),   S( -9,   2),   S(-20,  22),   S(-14,  75),
            S(-48,  75),   S(-88,  53),   S(-90,  -7),   S(-67, -21),   S(-38,   6),   S(-17,  19),   S(-10,  -2),   S(-17,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -20),   S(-25,  -5),   S(-21,  -7),   S(  6, -39),   S(  0,  -6),   S( 50, -25),   S( 90, -70),   S( 75, -85),
            S( -6, -45),   S(-24, -32),   S(-19, -46),   S(-14, -34),   S( -4, -32),   S( 16, -41),   S( 61, -75),   S( 64, -77),
            S(  1, -51),   S( -3, -60),   S(  0, -69),   S(  3, -69),   S(  3, -57),   S( 29, -60),   S( 39, -67),   S( 82, -76),
            S(  4, -37),   S(  6, -77),   S(  4, -80),   S(  5, -76),   S( 23, -75),   S( 24, -66),   S( 32, -51),   S( 73, -35),
            S( 31,   3),   S( -4, -39),   S( 13, -80),   S( 17, -74),   S( 91, -70),   S( 77, -45),   S( 60,   5),   S( 59,  61),
            S(-28, 101),   S(-17,   9),   S(  1, -54),   S(  0, -73),   S( 72, -81),   S( 69, -27),   S( 61,   3),   S( 67,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-99,  30),   S(-12,  -9),   S(-38,  15),   S(-11,  23),   S(-11, -23),   S(-49,  24),   S(-45,  -2),   S(-43,   5),
            S(-25,  14),   S( 40, -14),   S( 25, -35),   S(  9, -23),   S( -6, -25),   S(-48, -21),   S(  4, -45),   S(  4, -30),
            S( 33, -11),   S( 38, -10),   S(-18,  10),   S(  1, -27),   S(-30, -33),   S(-11, -39),   S(-15, -46),   S( 26, -41),
            S(  6,  32),   S(-21,  41),   S( 33,   4),   S(  1,   0),   S( 18, -42),   S(-31, -30),   S( 11, -46),   S( 57, -34),
            S(-25,  96),   S(-29,  91),   S(-19,  27),   S(-22,   4),   S(  1,  14),   S(-19,  -2),   S(-31, -35),   S( 39,  20),
            S( 65,  82),   S( 52, 105),   S(  8,  39),   S( 18,  21),   S( 12, -19),   S(  0, -14),   S(  7,  -1),   S(-13,  10),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-102,  58),  S(-79,  45),   S(-14,  17),   S( -9,  19),   S(-22,  33),   S(-35,  23),   S(-46,  13),   S(-24,  27),
            S(-56,  19),   S(-62,  21),   S( 26, -12),   S( 12,   8),   S( 14,  -9),   S(-20, -15),   S(-25, -11),   S(-28,   9),
            S(-50,  37),   S(-59,  30),   S( 45, -24),   S(  5, -20),   S( 28, -15),   S(-18, -20),   S( -7, -13),   S( 18, -12),
            S(-59,  56),   S(-57,  35),   S( -6,   6),   S( 17,  11),   S(-17,   6),   S(-50,  -2),   S(  7, -16),   S( 15,  11),
            S( 22,  62),   S( 28,  37),   S( 20,  43),   S( 19,  25),   S(-14,  33),   S( 56,  -6),   S( 12,   7),   S( 48,  26),
            S( 59,  47),   S( 56,  19),   S( 37,  -2),   S( 34,   1),   S( 43, -12),   S( 19,  -3),   S(  9,   5),   S(  6,  49),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-54,  30),   S(-44,  19),   S(-36,  18),   S(-31,  18),   S( 32, -20),   S(-33,  14),   S(-63,   7),   S(-59,  21),
            S(-41,   1),   S(-15, -19),   S(-19, -31),   S( -2,  -7),   S( 35, -17),   S( 19, -20),   S(-35,  -8),   S(-65,   8),
            S(-20,  -6),   S(-19,  -9),   S(-18, -21),   S(-31,  -4),   S( 12, -10),   S( 61, -39),   S( -5, -18),   S(-16,   6),
            S(-31,  16),   S(-75,   8),   S( -1, -27),   S(-17,  -7),   S( 11,   1),   S( 31, -13),   S( 19, -10),   S( 37,   4),
            S(  5,  22),   S(-54,  12),   S(  5, -29),   S( -8, -12),   S( 43,  26),   S( 65,  22),   S( 36,   9),   S( 63,  31),
            S( 58,  25),   S( 18,  -2),   S(  1, -37),   S(  5, -37),   S( 18,   2),   S( 21,   5),   S( 38,  -8),   S( 38,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -23),   S(-49, -15),   S(-28,  -7),   S(-53,  10),   S(-15, -23),   S( 24, -22),   S(  2, -50),   S(-40, -23),
            S(-36, -43),   S(-36, -43),   S(-43, -40),   S(-21, -46),   S( -8, -37),   S( 52, -57),   S( 55, -58),   S( -6, -33),
            S(-36, -45),   S(-53, -39),   S(-38, -50),   S(-14, -45),   S( -5, -29),   S( 42, -42),   S( 48, -58),   S( 54, -45),
            S(-11, -48),   S(-46, -53),   S(-74, -47),   S(-45, -28),   S(  0, -31),   S( 24, -22),   S( 26, -17),   S( 74, -27),
            S( 12, -38),   S(  6, -62),   S(-23, -57),   S(  1, -69),   S( 25,  -7),   S( 33,  -3),   S( 65,  43),   S(100,  35),
            S(-15,   2),   S(-30, -35),   S(  3, -56),   S( -4, -56),   S( -3, -17),   S( 27, -23),   S( 49,  39),   S( 89,  59),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-64,  81),   S(-46,  65),   S( 11,  26),   S(-11,  -1),   S( 11,   6),   S( -2,   4),   S(-41,   3),   S(-46,  26),
            S(-67,  71),   S(-63,  61),   S(-33,  44),   S(-14,  13),   S( -7, -13),   S(-34, -17),   S(-49, -10),   S(  4,  -8),
            S(-69, 110),   S(-13, 108),   S(-11,  67),   S(-24,  36),   S( 17, -16),   S(-94, -10),   S(-69, -20),   S(-38,  -7),
            S(-36, 149),   S(  7, 159),   S( 13, 113),   S( 10,  53),   S(-28,   9),   S(-28, -26),   S(-26,  -8),   S(-49,   7),
            S(-16, 178),   S( 42, 163),   S( 24, 166),   S( 56, 100),   S( 19,   6),   S(  1,  -1),   S(-18, -17),   S( -6,  16),
            S( 54, 201),   S( 70, 218),   S( 87, 206),   S( 49,  77),   S(  6,  33),   S(-13,   2),   S(-10, -27),   S(  2,  10),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-104,  81),  S(-75,  55),   S(  4,  15),   S( 11,  31),   S(  8,   7),   S(-49,  20),   S(-79,  16),   S(-78,  32),
            S(-65,  43),   S(-62,  39),   S(-51,  38),   S(  1,  52),   S(-53,   4),   S(-31,  -7),   S(-74,  -5),   S(-30,   8),
            S(-96,  74),   S(-122, 105),  S(-58,  90),   S(-112, 100),  S(-63,  56),   S(-86,  10),   S(-45, -22),   S(-43,   2),
            S(-77, 113),   S(-40, 122),   S( -1, 131),   S( 40, 135),   S(-31,  62),   S(-41,  16),   S( 12,  -3),   S(-44,  19),
            S( 11, 129),   S( 21, 148),   S( 20, 162),   S( 43, 179),   S( 19, 133),   S( -8,  37),   S( -1,  -3),   S( -1,  -1),
            S( 25,  76),   S( 21, 129),   S( 65, 146),   S( 70, 188),   S( 29, 112),   S( -9,  -1),   S(-15,  -8),   S(-19, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-93,  15),   S(-68,  -1),   S(-10,   0),   S(  2,  19),   S(-11,   3),   S(-70,  36),   S(-111,  29),  S(-65,  37),
            S(-100,   6),  S(-83,   6),   S(-17, -14),   S(-25,  -8),   S(-25,  28),   S(-44,  31),   S(-125,  36),  S(-87,  21),
            S(-24, -13),   S(-85,  14),   S(-32,   6),   S(-88,  74),   S(-85,  91),   S(-20,  47),   S(-118,  50),  S(-88,  45),
            S(-100,  31),  S(-79,  26),   S(-14,  14),   S(-43,  83),   S( 14, 102),   S(-56,  88),   S(-32,  51),   S(  1,  28),
            S(-29,  43),   S(-35,  17),   S(  6,  52),   S( 24, 129),   S(100, 115),   S( 46,  72),   S( -9,  89),   S( 27,  48),
            S( -2,  14),   S(-22,  -4),   S( 19,  22),   S( 49, 119),   S( 13, 135),   S( 26,  64),   S( -7,  79),   S( 23, 102),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-75,  -4),   S(-77,  17),   S( 38, -19),   S( -2,  17),   S( -1,  34),   S(-87,  56),   S(-57,  39),   S(-73,  51),
            S(-72, -21),   S(-80, -21),   S(-30, -41),   S(-48,  13),   S(-36,  11),   S(-27,  28),   S(-95,  64),   S(-98,  52),
            S(-36, -34),   S(-59, -34),   S(-48,  -9),   S(-25,   6),   S(-46,  34),   S(-10,  57),   S(-79,  88),   S(-48,  73),
            S(-52,   4),   S(-90, -14),   S(-27, -30),   S(-52,  14),   S(  8,  44),   S( -6,  76),   S( 15, 119),   S( 69,  81),
            S(-22,  21),   S(-48,  -9),   S( -9,  -6),   S(-10,  21),   S( 58,  95),   S(-11, 129),   S( 94, 127),   S( 84, 110),
            S(-34,  44),   S(-20,   1),   S(  8, -19),   S(  2,  -1),   S( 20,  70),   S( 32, 156),   S( 66, 189),   S( 33, 181),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15,  15),   S(-18,  10),   S(-19,   0),   S(  2,   6),   S( -4, -11),   S(-10,  10),   S(-16, -22),   S(-18,  -6),
            S(-38, -20),   S( -7,  21),   S(  9,  20),   S( -1,   4),   S(  0,  29),   S( -6, -16),   S(-36, -35),   S(-27, -44),
            S(-17,  45),   S(-36, 101),   S( 19,  69),   S( 20,  39),   S(-14,   0),   S(-46, -19),   S(-43, -51),   S(-43, -62),
            S(-42,  98),   S(-46, 130),   S( 40, 118),   S( 23,  96),   S(-18, -35),   S(-42, -38),   S( -8, -19),   S(-61, -54),
            S( 33, 105),   S( 38, 220),   S( 49, 155),   S( 18,  57),   S( -1,  11),   S( -3, -25),   S( -1,   2),   S(-20, -51),
            S( 47, 117),   S( 54, 224),   S(118, 225),   S( 47,  98),   S( -7,   3),   S(-10, -10),   S(-11, -30),   S(-23, -39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -16),   S(-21,  14),   S( -6,  10),   S( -2,   5),   S( -9, -10),   S(-30,   4),   S(-36, -45),   S(-24,  -7),
            S(-39,  -9),   S(-57,  49),   S(-23,  37),   S( 21,  23),   S(-45,  23),   S(-14, -14),   S(-81, -27),   S(-61,   6),
            S(-60,  51),   S(-52,  51),   S(-37,  82),   S( -9, 100),   S(  3,  34),   S(-40, -31),   S(-62, -32),   S(-78, -29),
            S(-78,  97),   S( -8, 125),   S( -5, 146),   S(  8, 129),   S(  2,  62),   S(-43,  27),   S(-18, -18),   S(-37, -44),
            S(  1, 101),   S( 53, 176),   S( 66, 201),   S( 49, 254),   S( 23, 153),   S(-12,  14),   S( -4, -67),   S(-26, -43),
            S( 41,  72),   S( 73, 175),   S( 85, 198),   S( 85, 258),   S( 40, 111),   S(  3,  11),   S( -1,   0),   S( -6,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -58),   S(-39, -23),   S( -8, -27),   S( -3,  -2),   S( -5,  -2),   S(-31,  14),   S(-36,  -3),   S( -5,  47),
            S(-54,  11),   S(-55,   8),   S(-53, -29),   S(  1,  10),   S(-38,  67),   S(-16,  21),   S(-41,  19),   S(-57,  15),
            S(-61, -25),   S(-60,   5),   S(-35, -18),   S(-21,  43),   S(-18,  77),   S(-51,  40),   S(-33,   3),   S(-64,  45),
            S(-50,  10),   S(-24,  52),   S(-25,  31),   S(  9, 100),   S( -3, 139),   S(-29,  90),   S(-37,  39),   S(-35,  61),
            S(-22, -24),   S(  9,  15),   S( 13,  79),   S( 36, 138),   S( 47, 222),   S( 41, 178),   S( 10,  88),   S( 25,  44),
            S( -3,  19),   S( 18,  32),   S( 30, 116),   S( 36, 138),   S( 65, 220),   S( 57, 124),   S( 31,  96),   S( 20,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -34),   S(-33, -23),   S(-11, -34),   S(  1,  -3),   S( 17,  21),   S(  1,  47),   S(-10, -21),   S( 10,  26),
            S(-43, -31),   S(-33, -14),   S(-14, -42),   S( 24,  -8),   S(-14,  -2),   S(  7,  47),   S(  5,  30),   S(  0,   3),
            S(-17, -75),   S(-33, -61),   S(-19, -54),   S(  3, -10),   S( 12,  32),   S(-13,  58),   S( -2,  72),   S(-23,  69),
            S(-26, -24),   S(-44, -32),   S(-31,  -1),   S( 11,  17),   S(-10,  52),   S(  6,  95),   S(-28, 146),   S( -6,  61),
            S(-28, -45),   S(-32, -33),   S(-14,  11),   S(  0,  -2),   S( 36, 116),   S( 64, 166),   S( 56, 230),   S( 74,  78),
            S( -9,   5),   S( -4,   9),   S(  1,   7),   S(  7,  23),   S( 26,  82),   S( 84, 195),   S( 34, 181),   S( 42,  43),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-35,   8),   S( -1,  12),   S(-51,  13),   S(-33,  -7),   S(-38,  -9),   S( -4, -28),   S(-49, -44),   S(-28, -14),
            S(-36,  62),   S( 17, -37),   S(-45,  14),   S(  5, -23),   S(-10, -20),   S(-24, -16),   S(-33, -21),   S(-69, -21),
            S(  7,  67),   S(  0,  -8),   S(  2,  -9),   S(-25,  33),   S(  7,   7),   S(-35,   0),   S(-10, -30),   S(-39, -48),
            S( 13,  -9),   S( 43,   9),   S( 10,  27),   S( 22,  28),   S(  8,   4),   S(  0,   2),   S( -4, -15),   S( -1,  -3),
            S( 21, -28),   S( 39,  12),   S( 16,   6),   S( 69,  -9),   S( 45,  -9),   S( 31,  19),   S( 26, -14),   S(-57, -10),
            S( 23, -12),   S( 15,  10),   S( 33,  10),   S( 57, -17),   S( 37, -48),   S( 18,  11),   S( 13, -22),   S( -2,  -8),
            S( 19, -25),   S( 17, -35),   S( 20, -25),   S( 36, -30),   S( 24, -16),   S( -5, -27),   S( -9, -40),   S(-19, -28),
            S(-63, -50),   S( -5,   3),   S( -4, -16),   S(  5, -41),   S(-18, -21),   S( 24,  14),   S( -5,   5),   S( 19,   1),

            /* knights: bucket 1 */
            S(-41,  33),   S(-55,  84),   S(  8,  42),   S(-42,  64),   S(-27,  45),   S(-30,  27),   S(-39,  48),   S(-15,  -6),
            S( 36,  30),   S(-13,  40),   S(-10,  26),   S(-10,  42),   S( -9,  23),   S(-17,  12),   S( 14, -15),   S(-27,  16),
            S(-31,  32),   S( 15,  12),   S( -4,  13),   S( 13,  27),   S(  3,  30),   S(-31,  28),   S(-16,   5),   S(-32,  22),
            S( -6,  42),   S( 55,  26),   S( 16,  43),   S( 19,  30),   S(  9,  29),   S( -1,  27),   S( 15,  12),   S( 13,  15),
            S(  5,  50),   S( 21,  24),   S( 28,  26),   S( 41,  26),   S( 39,  25),   S( 34,  18),   S( 30,  14),   S( 18,  20),
            S( 12,  20),   S( 23,   9),   S( 20,  29),   S( 47,  11),   S( 12,  18),   S( 39,  29),   S( 29,   6),   S( 19,  -6),
            S( 42,  10),   S( 28,  19),   S(-12, -18),   S( 15,  30),   S( 32,  -6),   S( 31,  -5),   S(-29,  12),   S( -4, -17),
            S(-85, -55),   S(-21,  -9),   S( -4,  18),   S(  5,  30),   S(-11,   5),   S(-23, -18),   S( -2,  -2),   S(-32, -27),

            /* knights: bucket 2 */
            S(-55,  13),   S( -8,  20),   S(-37,  50),   S(-37,  51),   S(-49,  60),   S(-43,  65),   S(-22,  30),   S(-23,  25),
            S(-14, -13),   S(-24,  13),   S(-14,  14),   S(-12,  29),   S(-10,  24),   S(-16,  49),   S(-40,  58),   S(-38,  73),
            S(-17,  26),   S( -5,  10),   S(-12,  27),   S( 12,  20),   S(  3,  30),   S(  2,  11),   S( -4,  40),   S(-28,  33),
            S(-11,  41),   S(-21,  37),   S(  0,  40),   S(  7,  46),   S( -3,  46),   S( -3,  35),   S(  1,  40),   S( -4,  44),
            S( 20,  23),   S(-16,  34),   S( -5,  44),   S(-17,  53),   S(  5,  45),   S( -7,  40),   S(  7,  32),   S( -2,  24),
            S(-19,  34),   S(  4,  31),   S(-24,  48),   S(-15,  42),   S(-26,  42),   S(  8,  20),   S(-27,  12),   S( 19,   4),
            S(-12,  25),   S(-28,  17),   S(-28,  17),   S(-35,  34),   S(-10,  14),   S(  6,  20),   S(-45,  40),   S(-28,  15),
            S(-133,  34),  S( -2,   3),   S(-77,  34),   S(-26,  14),   S(  0,  12),   S(-56,   5),   S(  0,   5),   S(-166, -42),

            /* knights: bucket 3 */
            S(-46,  -2),   S(  9, -26),   S(-30,  -4),   S( -3, -10),   S(  0,  -5),   S(-18,   6),   S( 23, -19),   S( -7, -16),
            S(-10,   3),   S(-28,  -6),   S(-16, -12),   S(  8,   7),   S( 18,  -5),   S( -5, -10),   S( -1, -13),   S(-16,  64),
            S(  2, -31),   S(  6,  -5),   S(  2,  -3),   S( 15,   5),   S( 21,  17),   S( 23,   1),   S( 18,   2),   S( 13,  30),
            S(  2,   2),   S( 13,   9),   S( 20,  29),   S( 26,  29),   S( 30,  30),   S( 27,  31),   S( 30,  19),   S( 26,  22),
            S( 29,   7),   S(  9,  18),   S( 37,   9),   S( 34,  38),   S( 32,  34),   S( 36,  43),   S( 46,  37),   S( 22,  14),
            S(  7,  11),   S( 34, -12),   S( 51,  -6),   S( 63,   0),   S( 74, -22),   S( 81, -16),   S( 17,   9),   S( 17,  44),
            S( 33,  -2),   S( 17,  10),   S( 49, -23),   S( 53,  -8),   S( 71, -32),   S( 68, -37),   S( 69, -63),   S( 56, -17),
            S(-99,  26),   S(-23,  12),   S(-24,   5),   S( 10,  17),   S( 41,  -8),   S(  2, -10),   S( -3, -15),   S(-55, -33),

            /* knights: bucket 4 */
            S( 14,  21),   S(-53,   6),   S( 15,  29),   S( -7,  -8),   S(-25, -16),   S(-33, -26),   S(-13, -53),   S(-28, -41),
            S( 33,  27),   S(-26,  38),   S(  8, -26),   S(  2,  -8),   S( 13, -21),   S(-11, -47),   S(  8,  -3),   S(  1, -47),
            S( -7,  30),   S(  4,  38),   S(  5,   7),   S( 13,  11),   S( -8,  -2),   S(-45,  12),   S(-47, -33),   S(-31, -55),
            S(  0,  67),   S( 36, -20),   S( 51,  20),   S( 30,  19),   S( 22,  10),   S( 99, -16),   S( 26, -30),   S(  1, -19),
            S( 65,  36),   S(-12,  45),   S( 51,  44),   S( 47,  20),   S( 43,  35),   S(-11,  25),   S( -2, -25),   S(-11,  -8),
            S( 10,  22),   S(-26,   2),   S( 84,  16),   S( 11,   6),   S( 11,  16),   S( 23,  17),   S( 11,  27),   S(-10, -20),
            S( -5,   9),   S(-15,   9),   S( 13,  -1),   S(  5,  37),   S(  8,   9),   S(  7, -16),   S(  4,  -9),   S(-15,  -3),
            S(-10,  -4),   S(  0,  -2),   S(  9,  10),   S(  1,   6),   S( -6,  -9),   S( 10,  22),   S( -1,   7),   S( -3, -18),

            /* knights: bucket 5 */
            S( 12,   8),   S(-44,  51),   S( 25,  38),   S( 11,  50),   S( 27,  22),   S(  6,  -1),   S( -2,  15),   S(-19, -12),
            S( 14,   7),   S( 29,  51),   S( 10,  24),   S(-20,  41),   S( 24,  34),   S( -4,  34),   S( 17,  28),   S(-14, -25),
            S(  0,  30),   S(-17,  40),   S( 56,  19),   S( 42,  40),   S(-19,  48),   S( -6,  25),   S(-20,  18),   S(  7,  -2),
            S( 36,  50),   S( 12,  48),   S( 38,  40),   S(  7,  55),   S( 24,  44),   S( 19,  41),   S( 26,  44),   S( 14,  37),
            S( 24,  55),   S( 36,  34),   S( 52,  50),   S( 71,  41),   S( 88,  45),   S( 33,  42),   S( 41,  37),   S( 40,  32),
            S(  5,  35),   S(  1,  52),   S( 25,  29),   S( 18,  54),   S( 42,  41),   S( 16,  53),   S( 23,  15),   S( -4,  33),
            S( 19,  59),   S( -7,  67),   S( 30,  46),   S( 16,  63),   S(  6,  53),   S(  7,  45),   S( 23,  69),   S(  3,   4),
            S(  1,  17),   S(  0,  19),   S(  9,  43),   S( -3,   8),   S( 10,  44),   S(  2,  37),   S(  9,  43),   S(-15, -11),

            /* knights: bucket 6 */
            S(  2, -34),   S(-29,  -2),   S( 22,  29),   S(-36,  42),   S(-42,  55),   S(  2,  43),   S(-13,  39),   S(-12,  39),
            S( -5, -25),   S( 44,   3),   S(  6,  12),   S(-43,  40),   S(-68,  68),   S( 19,  50),   S( 14,  52),   S(  0,  18),
            S(-31, -15),   S( -1,   2),   S(-10,  23),   S( 21,  30),   S(-19,  58),   S(-43,  57),   S(  4,  49),   S( -6,  45),
            S( 35,   9),   S( 36,  13),   S( 53,  27),   S( 80,  23),   S( 28,  45),   S( 21,  50),   S( 15,  58),   S(-18,  78),
            S(  1,  39),   S( 68,  -6),   S( 60,  34),   S( 82,  28),   S( 96,  34),   S( 89,  33),   S( 20,  60),   S( 20,  58),
            S( 25,  27),   S( 13,  15),   S( 69,  17),   S( 54,  40),   S( 61,  46),   S( 36,  33),   S( 23,  42),   S( 40,  45),
            S(-23,  26),   S( -1,  36),   S(-30,  37),   S( 28,  31),   S(  1,  58),   S( 20,  43),   S( 19,  73),   S( -7,  34),
            S(-38,   9),   S( 15,  45),   S( 29,  41),   S( 10,  41),   S( 23,  37),   S( 11,  61),   S( 22,  64),   S( 13,  32),

            /* knights: bucket 7 */
            S(-32, -49),   S(-199, -44),  S(-78, -48),   S(-64, -15),   S(-48,  -8),   S(-40, -12),   S(-14,   9),   S(-14,  11),
            S(-49, -74),   S(-45, -47),   S(-40, -34),   S(-60,   4),   S(-53,  12),   S( -5, -12),   S(-22,  50),   S(  1,  32),
            S(-85, -64),   S(-59, -36),   S(-55,  -3),   S( 20, -21),   S(-21,   6),   S(  3,   7),   S(-17,  56),   S( 42,  57),
            S(-61, -19),   S( 17, -24),   S( -2,   7),   S( 36,  -3),   S( 51,  -1),   S( 20,  12),   S( 16,  15),   S(-19,  35),
            S(-58, -20),   S(-22, -27),   S( 50, -23),   S( 86, -19),   S(108,  -6),   S( 72,  20),   S( 95,   2),   S( 81,  26),
            S( -7, -36),   S( 14, -38),   S(-18,  -5),   S( 33,  -4),   S( 70,   6),   S( 79,   5),   S( 59, -14),   S(  0,  16),
            S(-34, -31),   S(-67, -19),   S(  5, -15),   S( 32,  17),   S( 36,  21),   S( 40,   0),   S(-18,  24),   S(  5,  10),
            S(-34, -19),   S( -7,  -4),   S(-26, -10),   S(  8,  13),   S( 12,   7),   S( 23,  21),   S( -3,  -7),   S(  0,   0),

            /* knights: bucket 8 */
            S( -1,  -7),   S( -9, -10),   S( -3,  -3),   S(-10, -31),   S(-10, -40),   S(-10, -51),   S( -2,  -2),   S( -5, -21),
            S(  2,   1),   S( -6, -10),   S( -8, -31),   S(-19, -44),   S(-31, -31),   S(-17, -70),   S(-13, -58),   S(-17, -37),
            S(  5,  18),   S(-23, -19),   S( 19,   1),   S(  4,  -6),   S(  2, -34),   S(-17, -16),   S(-13, -39),   S( -7, -40),
            S(-17,   0),   S(  0,  -5),   S( -1,  10),   S(  4,  28),   S(  6,  -6),   S(  7,   6),   S(-13, -50),   S( -2, -16),
            S( 27,  56),   S( 12,   9),   S( 15,  33),   S( 35,  15),   S( 11,  29),   S( -5,  -4),   S(  4, -21),   S( -7,  -7),
            S( 13,  38),   S( 10,   6),   S( 27,  22),   S( 33,  15),   S(  2,  -1),   S( -2, -10),   S( -7, -29),   S( -6,  -9),
            S(  4,  17),   S(  1,   5),   S(  6,   9),   S( 11,   9),   S(  6,   7),   S(  5,  20),   S(  2,  12),   S( -1,   3),
            S(  2,   3),   S( 11,  34),   S(  5,  17),   S( -1,   1),   S(  3,  13),   S( -5, -18),   S(  3,   5),   S( -3,  -4),

            /* knights: bucket 9 */
            S( -9, -25),   S(-20, -34),   S(-18, -46),   S( -4, -14),   S(-24, -53),   S(-15, -41),   S( -4, -17),   S( -3, -23),
            S(-11, -36),   S(-12,   3),   S(-11, -50),   S(-14,  -7),   S( -6, -16),   S( -7, -34),   S( -6,  -3),   S(-15, -42),
            S(  5,   9),   S(-11, -14),   S(  2, -20),   S(  2,   0),   S(  3,  13),   S(-33,  -6),   S(-13, -13),   S( -8, -16),
            S(-14,  -1),   S( -6,  -7),   S(  5,  24),   S( 18,  27),   S( 28,  19),   S(  9,  20),   S(-11, -35),   S( -3,  -1),
            S(  0,  24),   S( 18,   7),   S( 19,  34),   S(  2,  37),   S( 10,  14),   S( 12,  -5),   S(  2, -28),   S(  5,  11),
            S(  0,   4),   S(  7,  32),   S( 15,  28),   S( -6,  14),   S( 35,  32),   S( 17,  10),   S(  8,  15),   S( -6, -21),
            S(  1,   1),   S( -1,  23),   S( 18,  37),   S( 12,   3),   S( 14,  41),   S( -2, -18),   S(  4,  18),   S( -2,   1),
            S(  2,   2),   S(  4,  10),   S( 13,  29),   S( 16,  32),   S(  9,  11),   S(  0,   6),   S(  3,   5),   S(  0,  -3),

            /* knights: bucket 10 */
            S(-17, -45),   S(-17, -53),   S(-13, -26),   S(-18, -19),   S(-12, -10),   S(-15, -43),   S( -3,  15),   S(  5,  23),
            S( -6, -24),   S( -6, -12),   S( -1, -16),   S(-20, -34),   S(-25, -36),   S( -8, -40),   S( -8,  -4),   S( -5, -11),
            S(-17, -48),   S(-19, -62),   S(-10, -19),   S(-16, -19),   S( 14,  -3),   S(-14,  -6),   S( -6,   3),   S( -7,   8),
            S( -8, -16),   S( -5, -43),   S(  4, -37),   S( 18,   9),   S( 11,  34),   S( 19,  19),   S(  5,  15),   S( 11,  44),
            S( -8, -44),   S(-13, -28),   S( 15,   8),   S( 24,  26),   S( 20,  47),   S(  1,  23),   S( 20,  14),   S( 23,  51),
            S(-10, -39),   S( -5, -22),   S( -3, -13),   S( 13,  36),   S( 36,  55),   S( 31,  35),   S( 27,  59),   S( 18,  56),
            S(  0,   0),   S( -9, -30),   S(  1,  -7),   S( 27,  26),   S( 17,  26),   S(  9,  33),   S(  1,   1),   S( 10,  29),
            S( -3, -14),   S(  3,  11),   S( -7, -17),   S(  4,   2),   S( 12,  39),   S(  5,  27),   S(  3,  15),   S(  0,   0),

            /* knights: bucket 11 */
            S(  0,   2),   S(-20, -29),   S( -9, -46),   S(-10, -24),   S(-20, -48),   S(-12, -17),   S( -6,  -3),   S( -4,  -5),
            S( -7,  -6),   S(-13, -20),   S(-15, -77),   S(-30, -28),   S( -9,  -4),   S(-29, -36),   S(-16, -28),   S( -7,  -8),
            S(-15, -52),   S(-24, -59),   S(-27, -37),   S( -1,  -2),   S(-15,   0),   S(-19,  13),   S(  8,  -5),   S( -1,  16),
            S(-12, -27),   S( -8, -29),   S(-26,  -6),   S( 25,  28),   S( 15,  15),   S( 18,   6),   S( 15,  24),   S( 15,  31),
            S( -3, -23),   S(-18, -58),   S(  6, -21),   S(  0,   5),   S( 15,  19),   S( 34,  52),   S(  7,  -1),   S( 25,  66),
            S( -7,  -8),   S( -6, -28),   S(  0,  -7),   S( 40,  32),   S( 17,  21),   S( 48,  44),   S( 22,  22),   S( 14,  27),
            S(  9,  27),   S( -2,  -6),   S(  7, -13),   S( 12, -16),   S( 20,  29),   S( -1,   4),   S( 16,  41),   S( 20,  57),
            S( -3,   1),   S( -2, -16),   S(  9,  14),   S(  2,   8),   S(  2,  14),   S(  3,   7),   S(  4,   6),   S(  2,  13),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   2),   S( -2, -20),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   5),   S( -2, -13),
            S(  0,   0),   S(  1,   2),   S(  2,   6),   S( -4, -12),   S( -2,   5),   S( -4, -21),   S( -2, -12),   S(  1,  10),
            S( -5, -12),   S(  4,   4),   S( -6, -12),   S( -6, -24),   S(  0,   2),   S( -5, -18),   S(  2,  -5),   S( -7, -31),
            S( -7, -12),   S( -1,   1),   S( -9, -24),   S(  4,  13),   S( -6,  -7),   S(  0,   5),   S( -1,  -5),   S( -1,  -7),
            S(  9,  17),   S(  5,   3),   S( -6, -13),   S(  0,   3),   S( -6, -27),   S(  0,   4),   S( -1, -13),   S( -1,   2),
            S(  1,  -8),   S( -2, -20),   S(  1,  -1),   S( -1,  -5),   S(  5,  11),   S( -5, -19),   S( -1,  -7),   S(  0,   3),
            S(  2,   8),   S( -8,  -8),   S(  0,  11),   S(  2,  -8),   S( -5,  -8),   S( -5, -21),   S( -2,  -1),   S(  0,  -2),
            S(  2,   4),   S(  2,  13),   S( -2,  -4),   S(  2,  -1),   S( -2,  -4),   S( -2,  -9),   S( -3,  -9),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -1,  -5),   S( -2,  -2),   S( -8, -13),   S( -1,   1),   S( -3, -12),   S(  1,  -1),
            S( -2,  -6),   S(  1,   4),   S( -2, -23),   S(-10, -21),   S( -6, -31),   S( -4, -24),   S(  0,   0),   S(  0,  -1),
            S( -3,  -9),   S( -8, -30),   S(  6,  16),   S(  0,  -1),   S(-13, -40),   S(-10, -25),   S( -2, -11),   S( -6, -28),
            S( -8, -14),   S(  5,  13),   S(  1,   0),   S(-11, -28),   S( -2, -10),   S(  6,  10),   S(  0, -13),   S( -4,  -9),
            S(  3,  11),   S( -1,  -1),   S(  2,  -9),   S( 10,  17),   S(  5, -13),   S( -3, -10),   S(  2, -12),   S(  1,   1),
            S( -3,  -8),   S( 14,  14),   S(  7,  21),   S(-12,  10),   S(  5,   4),   S( -9, -33),   S(  4,   6),   S( -3,   3),
            S(  1,   7),   S(  2,   4),   S( 10,  11),   S(  7,   9),   S( 14,  21),   S( -5, -21),   S( -2,  -1),   S( -5,  -3),
            S( -1,   2),   S( -1,  -5),   S(  0,   1),   S(  1,  -8),   S( -1,   0),   S(  3,  -2),   S(  0,  -1),   S( -1,   1),

            /* knights: bucket 14 */
            S( -3, -23),   S( -5, -24),   S( -1,  -1),   S( -3,   4),   S( -8, -23),   S( -2, -15),   S( -1,  -5),   S(  0,   2),
            S(  0,  -2),   S( -4,  -8),   S(-15, -60),   S( -8, -37),   S( -1,  -9),   S(  1,   6),   S(  1,  -2),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-11, -55),   S(  1,   1),   S( -4, -21),   S( -4, -10),   S(  0,  -1),   S(  2,  11),
            S(  0,   5),   S( -6, -32),   S(-16, -41),   S(-11, -37),   S( -3, -21),   S(  2,  -1),   S( -2, -15),   S( -7, -11),
            S( -2,  -4),   S( -2, -15),   S(  0,  21),   S( -8, -34),   S( -9,  -9),   S(  3,  22),   S(  3,   6),   S( -4,  -4),
            S( -4,  -8),   S(  4,  -1),   S( -9, -31),   S(  4,   0),   S( 15,  20),   S(  4,   9),   S( -3,   0),   S(  1,  -2),
            S(  0,  -3),   S( -2, -10),   S(  7,  -4),   S(  0, -10),   S( -7, -10),   S( -3,  -9),   S( -6,  -4),   S(  1,   8),
            S(  0,  -2),   S(  2,   4),   S( -1,  -9),   S(  7,  -1),   S(  5,  18),   S(  1,   3),   S( -2,  -6),   S( -1,  -5),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -1, -13),   S( -1, -13),   S( -7, -13),   S( -2,  -1),   S( -1,  -4),   S(  1,   0),   S(  0,  15),
            S( -2,  -5),   S(  0,  -2),   S( -5, -19),   S( -6, -25),   S( -2,  -6),   S( -1,  -8),   S(  0,   0),   S( -1,  -3),
            S( -6, -15),   S( -7, -15),   S( -3, -12),   S(-15, -41),   S( -6, -25),   S( -2,  -4),   S( -1,   0),   S( -2,   2),
            S( -6, -16),   S( -6, -31),   S( -6, -19),   S(  0,  -9),   S(  0, -17),   S(  7,  22),   S(  5,  10),   S( -3,   0),
            S(  0,  -1),   S( -2,  -6),   S( -1, -16),   S( -7, -11),   S(  3,  17),   S(  4,  10),   S( -6,  -7),   S( -1,   3),
            S( -2,  -3),   S( -2,  -4),   S( -2, -21),   S( -3,   6),   S( -5, -14),   S( -7,  11),   S( -2,   5),   S(  2,   9),
            S( -3, -12),   S( -2,  -6),   S( -1,  -9),   S( -4,  -7),   S(-10, -13),   S( -4,  16),   S( -2,  -7),   S(  3,  13),
            S(  0,  -3),   S(  0,  -1),   S( -3,  -9),   S( -2,  -8),   S( -2,  -4),   S(-10,  -4),   S(  6,  17),   S( -2,   2),

            /* bishops: bucket 0 */
            S( 21,  11),   S( 20, -13),   S( 39,  17),   S(  6,  20),   S( -5,  -3),   S( 18,  -6),   S( 25, -38),   S(  5, -34),
            S( 56, -40),   S( 80,  -2),   S( 40,   6),   S( 17,   2),   S(-12,  34),   S(  0, -21),   S(-37,  -5),   S( 15, -46),
            S( 28,  45),   S( 46,   8),   S( 26,   3),   S( 14,  50),   S( 20,  11),   S(-30,  21),   S(  9, -26),   S( 18, -42),
            S( 22,  10),   S( 69,  -9),   S( 39,  13),   S( 37,  35),   S(  4,  33),   S( 29,   1),   S( -2, -10),   S(  3,   3),
            S( 18,   4),   S( 33,  26),   S(  6,  41),   S( 59,  16),   S( 65,  -1),   S( 19,  -3),   S( 24, -16),   S(-30,   2),
            S(-37,  63),   S( -1,  21),   S( 65, -23),   S( 90, -23),   S( 42,  32),   S( -3,  -1),   S(  2,  13),   S(  3,  14),
            S(-11,  16),   S(  9,  -3),   S( 43,  -6),   S(  2,  35),   S(-32,  -3),   S( 28,  27),   S(  2, -13),   S( -9,  -9),
            S(-32, -41),   S( 11,   3),   S(  1,   8),   S(  8,  -9),   S( 22,  27),   S( 35,  11),   S(  1,  47),   S(-20,   3),

            /* bishops: bucket 1 */
            S( 42,  12),   S( -5,  34),   S(  5,  41),   S( 11,  29),   S( -7,  29),   S( -2,  30),   S(-10,   3),   S(-41,  -2),
            S( 15,  -8),   S( 35, -18),   S( 54,   6),   S( 29,  28),   S( -8,  15),   S(  7,  -3),   S(-33,  -6),   S( 18, -12),
            S( 44,  -2),   S( 12,   7),   S( 34, -10),   S( 19,  24),   S( 21,  25),   S(-21,   0),   S( 25,  -7),   S(  9, -29),
            S( 40,   7),   S( 20,  19),   S( 12,  15),   S( 35,  23),   S(  5,  25),   S( 24,   6),   S( -3,   7),   S( 15, -10),
            S( 39,  32),   S( 11,  24),   S( 21,  25),   S( -1,  34),   S( 28,  12),   S(  2,  17),   S( 32, -15),   S( -8,  14),
            S( -2,  24),   S( 30,  38),   S( 32,   1),   S( 56,  -7),   S( 18,  18),   S( 36, -18),   S(  1,  31),   S( 52, -13),
            S( -7,  49),   S(-27,  19),   S( 19,  28),   S( 36,  24),   S( 40,  25),   S(-19,  22),   S( 34, -21),   S(-15,  41),
            S( 15,   3),   S(  9,   6),   S(  3,  13),   S(-22,  22),   S( 22,  17),   S( -7,   6),   S( 13,   7),   S( -3,  13),

            /* bishops: bucket 2 */
            S( 19, -12),   S(  7,  17),   S( -6,  15),   S(-29,  51),   S(-13,  35),   S(-31,  30),   S(-19,  -2),   S(-45,  24),
            S(-20,  27),   S(  2, -17),   S( 22,   9),   S( -2,  24),   S( -2,  33),   S( 16,   6),   S(-11, -20),   S(  9, -25),
            S( -6,   7),   S( -4,  10),   S(  4,   5),   S( -3,  46),   S(  5,  36),   S( -2,  15),   S( 14,  12),   S(-15,   3),
            S(  3,  11),   S( -8,  14),   S(-13,  37),   S(  7,  37),   S(  0,  40),   S(  8,  24),   S(  7,  17),   S(  3,   9),
            S( 12,   5),   S(-15,  34),   S( -8,  27),   S(-30,  47),   S( -9,  38),   S( -8,  46),   S(  4,  23),   S(-28,  36),
            S(  7,  29),   S( -4,  16),   S(-23,  26),   S(-16,  25),   S( 11,  12),   S( -4,   5),   S( -1,  56),   S(  2,  25),
            S(  4,  21),   S(-22,   9),   S(-26,  53),   S( 19,   1),   S( -1,   2),   S(-17,   9),   S(-67,   9),   S(-31,  37),
            S(-53,  30),   S(-37,  45),   S(-24,  28),   S(-40,  25),   S(-48,  38),   S(-34,  18),   S(  7,  13),   S(-67,   7),

            /* bishops: bucket 3 */
            S(  3,   7),   S( 34,  -6),   S( 27,  17),   S( 17,  18),   S( 19,   6),   S( 41,  -8),   S( 43, -26),   S( 48, -67),
            S( 14,   8),   S(  6,  -1),   S( 31,  -5),   S( 12,  30),   S( 23,   7),   S( 23,  20),   S( 44, -10),   S( 46,  -3),
            S( 23,  10),   S( 14,  15),   S( 11,  13),   S( 27,  20),   S( 24,  49),   S( 23,   6),   S( 39,  23),   S( 50,  -9),
            S( 33,  -6),   S( 27,   9),   S( 19,  32),   S( 27,  45),   S( 33,  35),   S( 32,  33),   S( 30,  21),   S( 26,  -2),
            S( 21,   4),   S( 27,  13),   S( 44,  14),   S( 29,  45),   S( 25,  46),   S( 37,  27),   S( 20,  34),   S( 27,  33),
            S( 30,   4),   S( 36,  22),   S( 26,  10),   S( 43,  14),   S( 26,  19),   S( 56,   0),   S( 51,  13),   S(  8,  69),
            S( 20,   9),   S( -7,  14),   S( 43,  23),   S( 24,  16),   S( 16,  15),   S( 24,   0),   S(  2,  24),   S( 21,  37),
            S(-32,  59),   S(  0,  35),   S( 58,  11),   S( 28,  17),   S( -9,  34),   S(  4,  33),   S( 32,  -1),   S( 66, -29),

            /* bishops: bucket 4 */
            S(-23, -25),   S(-21,  10),   S(-35,  -1),   S(-23,  21),   S(-25,  26),   S(-51,  26),   S(  1, -11),   S(-12, -12),
            S( -4,  15),   S(  7,   5),   S(-10,  36),   S(-31,  17),   S(-20,  -5),   S( 39,  -7),   S(-30, -11),   S( 13,  -3),
            S( -8,   5),   S(-32,  35),   S( 14, -20),   S(-25,  14),   S(  2,  26),   S( 22, -24),   S(-30, -11),   S(-52,  -2),
            S(-33,  28),   S(  0,  33),   S( 50,  28),   S( 31,  35),   S( 12,  23),   S( 52, -10),   S( 48,  -8),   S(-11, -36),
            S( 11,  21),   S(  3,  47),   S(-15,  53),   S( 21,  42),   S( 36,  10),   S( 34, -18),   S(-10, -21),   S( 13, -10),
            S( -6,  36),   S( 22,  17),   S(-12,  28),   S( 21,  13),   S( 41,  10),   S(  7, -12),   S( 19, -35),   S(  5,  -5),
            S(-16,  10),   S( 28,  13),   S( 13,  18),   S( 25,  19),   S( 11,  -2),   S(  3,  19),   S( -1,   2),   S(  7, -24),
            S( 10, -19),   S(-11, -36),   S(  2,  -2),   S( -4,   0),   S(  7,  -9),   S(  2,  10),   S(  1,  -3),   S( -5,   2),

            /* bishops: bucket 5 */
            S(-11,  -3),   S(-13,  43),   S(-51,  35),   S(-29,  33),   S(-42,  34),   S(-11,  20),   S( -4,  17),   S(-23,  14),
            S(-24,  39),   S(-15,   8),   S(-34,  60),   S(  0,  29),   S(-31,  38),   S(-28,  26),   S(-36, -15),   S(-10,   1),
            S(  1,  16),   S(  0,  39),   S( 19,  13),   S(-20,  50),   S( -3,  37),   S(-34,  -2),   S(-30,  32),   S(-19,   6),
            S( 32,  14),   S( 26,  28),   S(-11,  57),   S( 29,  30),   S( 31,  35),   S( 18,  30),   S( 17,  -6),   S( 10,  26),
            S( 34,  47),   S( 37,  16),   S( 53,  30),   S( 81,  32),   S( 50,  21),   S( 44,  19),   S( 37,  13),   S( -7,   6),
            S( 23,  45),   S( 31,  46),   S( 34,  21),   S( 30,  34),   S(  0,  34),   S( 19, -18),   S(-23,  48),   S(  0,  33),
            S(  4,  43),   S(-34,  12),   S( 10,  41),   S(  5,  50),   S( 27,  28),   S( 31,  39),   S( -1,  20),   S(  1,  36),
            S( -2, -11),   S( 15,  34),   S( 15,  14),   S(  5,  38),   S(  2,  56),   S( 14,  24),   S( 30,  56),   S( -5,   4),

            /* bishops: bucket 6 */
            S( -8,  16),   S(  0,  30),   S(-41,  38),   S(-41,  38),   S(-41,  25),   S(-46,  34),   S(-20,  57),   S(-12,  18),
            S( 23,   9),   S( -4, -11),   S(-23,  30),   S( -7,  33),   S(-31,  45),   S(-19,  26),   S(-103,  33),  S( 17,  27),
            S( 23,   0),   S(  6,   8),   S( 24,  -2),   S( 19,  28),   S( 39,  22),   S( 13,   8),   S(  2,  33),   S(-37,  22),
            S(-13,  42),   S( 17,  15),   S( 34,  21),   S( 31,  34),   S( 40,  32),   S( 36,  27),   S( 35,  32),   S(-13,   0),
            S(-13,  23),   S( 53,   6),   S( 26,  27),   S( 52,  24),   S( 97,  27),   S( 59,  26),   S( 38,  31),   S(-24,  51),
            S(  4,  12),   S(-44,  49),   S( 10,  18),   S( 16,  40),   S( 37,  29),   S( 27,  26),   S(  5,  49),   S( -8,  51),
            S(-22,  36),   S(-31,  26),   S(  1,  40),   S(-11,  33),   S( 44,  21),   S( 21,  28),   S(-12,  31),   S( -1,  37),
            S(  8,  52),   S( 14,  37),   S(  8,  42),   S(  1,  47),   S(-17,  38),   S( 32,  17),   S( 11,  23),   S( 12,   6),

            /* bishops: bucket 7 */
            S(-15, -36),   S( -3,   3),   S(-39, -26),   S(-55,  12),   S(-32,  -7),   S(-76,  19),   S(-71, -31),   S(-63,   7),
            S(-32, -26),   S(-60, -41),   S(-22,  -7),   S( -1, -15),   S(-35,   3),   S(-45,  17),   S(-51, -12),   S(-31,  12),
            S(-34, -19),   S(  3, -18),   S( 19, -40),   S( 18,  -2),   S(-33,  18),   S(-19, -19),   S(-33,  45),   S(-23,  30),
            S(-40,  17),   S( 55, -36),   S( 74, -22),   S( 57,   4),   S( 81,   1),   S(  4,  23),   S( 26,  30),   S( -3,  28),
            S( 24, -47),   S(-12, -19),   S( 65, -36),   S(103, -27),   S( 70,  25),   S( 73,  15),   S( -4,  42),   S( 26,  11),
            S(-27, -11),   S(-27,   4),   S( 29, -45),   S( 23,  -3),   S( 51, -12),   S( 53,   3),   S( 54,  16),   S( 24,   3),
            S( -2, -12),   S(-42,  -9),   S(  8,  -1),   S( 13,  -5),   S( 14, -21),   S( 35,  -9),   S(  9,  -4),   S( 14,  14),
            S(-14,  -7),   S( -8,  20),   S(-29,  13),   S(  5,  -3),   S( 10,  -3),   S( 19,  -4),   S( 25,   9),   S(  5,   6),

            /* bishops: bucket 8 */
            S(-10,  -8),   S(-12, -32),   S(-44,  -4),   S( -3, -24),   S( -5,  22),   S(-24,  -1),   S(  6,  23),   S( -6, -10),
            S( -7,  -1),   S(-33, -49),   S(-13, -23),   S(-15,  -4),   S( 10,  -9),   S(-17, -28),   S(-20, -55),   S( -5,  -8),
            S(  2,   1),   S(-11,  10),   S(-24,   7),   S(-10,  17),   S( -7,  11),   S( -9, -41),   S(  5, -44),   S(-31, -37),
            S(  8,  35),   S( -5,  43),   S(  8,  40),   S( -3,  14),   S( 19,  21),   S( -3,   7),   S(  4, -18),   S( -5, -17),
            S( 16,  38),   S( 13,  66),   S( -8,  31),   S( 49,  44),   S(  3,  22),   S( 17,   9),   S(  7, -31),   S(-10, -15),
            S( -1,  10),   S( 12,  36),   S(  9,  18),   S(-14,  18),   S( 28,  10),   S( -9, -17),   S(-14, -14),   S(-18, -21),
            S( -3,   7),   S(  8,  24),   S( 10,  23),   S(  1,   3),   S(  4,  11),   S( -1,  20),   S(-13, -15),   S( -9, -27),
            S( -9, -13),   S(  1, -27),   S(  0,  -6),   S(  0, -13),   S(-18,  -8),   S( -5,  -5),   S( -1,  14),   S( -8,   7),

            /* bishops: bucket 9 */
            S(-26, -32),   S( -6,   4),   S(-21,   4),   S(-11, -24),   S(-35, -28),   S(-19, -35),   S(-16,  -7),   S(  9,   1),
            S(-14, -16),   S(-39, -33),   S( -9,  -7),   S(-16,  15),   S(-47,  28),   S(-19, -15),   S(-17, -17),   S( -5,  -5),
            S(  9,   2),   S( 17,  13),   S(-27, -20),   S(-14,  22),   S(  1,  12),   S(-10, -24),   S(-16, -28),   S( -5,  24),
            S(-14,   9),   S( 16,  18),   S( -8,  25),   S(  9,  21),   S( 19,  25),   S( 11,   3),   S(  3,  -3),   S(-16, -24),
            S( -2,  19),   S( 23,  26),   S(  7,  39),   S( 11,  51),   S(-14,  16),   S(  4,  31),   S( -5,  34),   S( -7,  -6),
            S(-13,   5),   S( 20,  51),   S(  2,  15),   S( 23,  18),   S( 13,  33),   S( -7,  -6),   S(-18,   3),   S(-12, -12),
            S(  6,  18),   S( 18,  12),   S(  5,   8),   S(  2,  44),   S( 20,  38),   S(  8,   5),   S( -8, -16),   S( -5,  -2),
            S( -2, -23),   S( -7,  21),   S( -5,  17),   S(-18, -13),   S(-13,  -3),   S(  8,  29),   S(  2,   6),   S(-13, -17),

            /* bishops: bucket 10 */
            S(-20,  -8),   S(  5, -22),   S(-34, -27),   S(-19, -22),   S(-23, -10),   S(-24, -20),   S(-12, -22),   S(-18, -26),
            S(  6, -16),   S(-28, -39),   S( -6,  -8),   S(-41,   4),   S(-39,   6),   S(-20,  21),   S(-31, -58),   S(-11, -15),
            S(  9, -13),   S(  1, -11),   S(-38, -49),   S(  3,   7),   S(-37,  30),   S(-39,  11),   S(-21,  27),   S(  6,  20),
            S(-10, -21),   S(  4,   8),   S( 12,  -5),   S( 15,   8),   S( 11,  27),   S(-10,  56),   S(  4,  30),   S( 16,  27),
            S(-18,   0),   S(  2,   1),   S( -3,  16),   S( 35,  26),   S(  2,  62),   S( 22,  49),   S(  9,  41),   S(  1, -15),
            S(  3, -26),   S(-24,  -1),   S(-24, -13),   S(-11,  26),   S( 26,  35),   S( 35,  22),   S( 10,  53),   S(  2,  10),
            S(-21,  -7),   S(-12, -48),   S( -9,  -8),   S( 22,  15),   S( -4,  -4),   S( 18,  37),   S( 16,  37),   S( 14,  17),
            S( -7, -30),   S( -8,   8),   S(  7,  21),   S(-10,   2),   S( -8,  14),   S(-10,  -8),   S( 10,   3),   S(  6,  25),

            /* bishops: bucket 11 */
            S(-21,   0),   S(-31, -12),   S(-51, -45),   S(-22, -29),   S(-21, -10),   S(-65, -47),   S( -9, -11),   S(-23, -22),
            S(-12, -17),   S( -4, -41),   S( -8,  -7),   S(-24, -35),   S(-44,  -9),   S(-29, -27),   S(-26, -44),   S(-22, -35),
            S(-10, -48),   S(  1, -47),   S(-30, -27),   S(  0,  -8),   S( -3,  -7),   S(-36,   9),   S( -9,  24),   S( -1,  19),
            S(-16, -36),   S(-12, -34),   S(  6,  -9),   S(  4,  -7),   S( 13,  24),   S( -1,  60),   S(  8,  48),   S( 19,  28),
            S( -9, -22),   S(-17, -44),   S(-15,  21),   S( 50,   3),   S( 37,  37),   S(  3,  58),   S( 19,  56),   S( 14,  26),
            S(-18, -50),   S(-31,  -3),   S(-13, -38),   S(  9,  13),   S(  3,  33),   S( 18,  28),   S( 29,  38),   S( -1,   3),
            S( -8,  -7),   S(-21, -46),   S(-20,  -1),   S( -6, -17),   S( 10,   0),   S( 35,  16),   S( -9,   1),   S( 16,  31),
            S(-19, -14),   S(-21,   0),   S( -6,  12),   S( 10,   5),   S( 12,   3),   S(-17, -23),   S(  4,   8),   S( -2, -22),

            /* bishops: bucket 12 */
            S(  0,   2),   S( -7, -13),   S(-12, -28),   S( -7, -26),   S( -9, -19),   S(-11, -20),   S(  0,  12),   S( -6,   1),
            S( -7,  -7),   S(-13, -33),   S( -7, -13),   S( -6, -11),   S(-14, -23),   S( -2,  14),   S( -3,  -1),   S( -1,  -9),
            S( -1,  -3),   S(-15,  -2),   S(-12, -19),   S( -9,  -5),   S( -5,   7),   S( -6, -13),   S(-10, -43),   S( -3,  -2),
            S( -1,   4),   S(  5,   1),   S(-17, -29),   S( -3,  11),   S(  1,   7),   S(  6,  25),   S( -5,  -8),   S( -6,  -4),
            S( -1,  -3),   S(  3,  17),   S( -4,  20),   S( -8,   1),   S( -2,  -4),   S( -4,   3),   S(  4,   5),   S( -7,  -2),
            S(-12, -12),   S(  5,  60),   S(-28,   1),   S( -9,  -4),   S(  7, -15),   S( -4,   0),   S( -1,   5),   S( -1,  -5),
            S( -2,  -4),   S( -6,  10),   S(  3,  14),   S( -7,   7),   S( -1,   9),   S(  8,  16),   S( -8, -18),   S( -1,   4),
            S( -2,  -3),   S(  0,  -6),   S( -6,  -1),   S(  6,   8),   S(  1,   8),   S(  0,   3),   S(-10,   0),   S(  0,  -3),

            /* bishops: bucket 13 */
            S( -8, -43),   S(-13, -28),   S(-13, -15),   S(-15, -18),   S(-16, -19),   S( -8,   3),   S( -1,  -5),   S( -8, -10),
            S( -4,  -7),   S(-11, -13),   S(-13, -28),   S(-19,  -9),   S(-14,   8),   S( -8,   1),   S( -2, -11),   S(  2,  -2),
            S( -9, -10),   S( -6,  -7),   S( -8,   9),   S(-21,  -2),   S(-12, -23),   S( -3, -13),   S( -3, -30),   S(  5,  21),
            S( -2,   4),   S(-12,  -4),   S(-13,   4),   S(-23,  11),   S(  1,  20),   S(  3,  -7),   S( -1,   4),   S( -6,  -6),
            S( -3,   9),   S(-16,   6),   S(-16,  -2),   S( 20,   0),   S( -6,   4),   S( -5,   5),   S(-10, -15),   S( -2,  -6),
            S( -3,  -4),   S( -8,   2),   S(-20, -15),   S( 12,  17),   S(  3,   8),   S( -3,  -6),   S(  6,  19),   S( -3,  -6),
            S( -6,  -9),   S( -9,  -4),   S(  6,  24),   S( -7,   9),   S( -8,  -1),   S(  1,   0),   S(-15, -25),   S(  0,   7),
            S( -8, -18),   S(  0,   9),   S( -1,  -2),   S(  5,   1),   S(  0,   6),   S( -8,  -6),   S(  1,  10),   S( -2, -14),

            /* bishops: bucket 14 */
            S( -7, -17),   S(-12, -16),   S(-18, -26),   S(-18, -45),   S(-14, -36),   S( -6, -27),   S(-10, -13),   S(-10, -16),
            S(-10, -25),   S( -2, -23),   S( -8, -14),   S(-26, -41),   S(-10, -12),   S(-18, -11),   S(-15, -22),   S(  1, -13),
            S( -8, -11),   S( -9, -31),   S(-22, -30),   S(-13, -19),   S(-26,  -3),   S(-22, -30),   S( -7,   2),   S( -3,  -2),
            S( -8, -22),   S( -8,  -5),   S(-10,  -5),   S(-22,  20),   S(  1,   7),   S(-21,  13),   S(-19, -15),   S( -5, -11),
            S( -8,  -4),   S( -7,  26),   S( -6, -19),   S( -5, -20),   S(-12,  10),   S( -6,  -6),   S(  7,  22),   S(  2,  -5),
            S(  0,   4),   S( -8,   7),   S(-22, -12),   S( -8, -17),   S(  7,  10),   S(-10,  17),   S( -2,  34),   S( -7, -21),
            S( -6, -22),   S( -1,  -1),   S( -7,   1),   S(  3,  15),   S(-10,  -7),   S( -1,   1),   S( -3, -13),   S( -4,  -7),
            S( -7,  -7),   S( -4,  -6),   S( -3,  -5),   S( -2,   5),   S(-10, -18),   S(  1,   9),   S(  7, -10),   S(  1,   3),

            /* bishops: bucket 15 */
            S(  4,   8),   S(  6,   6),   S(-19, -27),   S(  0,  -9),   S(-11, -16),   S(-12, -23),   S( -6, -12),   S( -2, -10),
            S(  2,   4),   S( -1,  -7),   S(  3,  -1),   S( -9, -11),   S(-14, -21),   S( -6,  -7),   S( -8, -17),   S(  0,   0),
            S( -7, -13),   S(  0,  -2),   S(-13, -10),   S(-10,  -7),   S(-19, -19),   S(-17, -22),   S( -7, -10),   S(  2,  16),
            S( -4,  -7),   S(-16, -18),   S(  7, -12),   S(-23, -29),   S( -4,   7),   S( -9, -14),   S(  4,  15),   S( -1,  -9),
            S( -1, -10),   S(-12, -16),   S(-13, -10),   S(-18, -45),   S( -1, -22),   S(-13,  22),   S(  4,  19),   S(-10, -16),
            S( -9, -32),   S(-12, -12),   S(-18, -35),   S(-20, -11),   S( -4,  -2),   S( -9, -26),   S(  8,  40),   S(  1,  13),
            S( -3,   2),   S( -2, -17),   S( -2, -13),   S( -4,   3),   S(-11, -16),   S( -1,   8),   S(-12,  -2),   S(  3,   5),
            S( -4,  -1),   S( -1,   1),   S( -4,  -1),   S( -6,  -4),   S( -8,  -5),   S(-16, -20),   S( -9, -24),   S(  0,   0),

            /* rooks: bucket 0 */
            S(-23,   7),   S(-11,   1),   S(-15, -11),   S( -9,  -5),   S(-14,  10),   S( -9,  -7),   S(-15,  22),   S( -3,  19),
            S( 10, -63),   S( 25, -14),   S(  1,  -2),   S( -5,   3),   S( 10,  -2),   S( -2,  -9),   S(-34,  20),   S(-45,  29),
            S(  3, -25),   S( 10,  26),   S( 20,   6),   S(  9,   9),   S(-18,  39),   S( -1,   7),   S(-30,  17),   S(-41,  15),
            S( 26, -21),   S( 61,   0),   S( 42,  26),   S( 41,   5),   S( 14,   8),   S( -2,  14),   S(-14,  23),   S(-36,  34),
            S( 61, -23),   S( 88, -16),   S( 67,  -2),   S( 39,  -8),   S( 46,   7),   S( 25,   9),   S( -5,  37),   S(-17,  33),
            S( 70, -43),   S(106, -34),   S( 52,   7),   S( 16,  21),   S( 42,  10),   S(-39,  33),   S( 34,  18),   S(-37,  41),
            S( 44,  -8),   S( 70,  -2),   S( 24,   8),   S(  9,  28),   S( -7,  29),   S( -3,  15),   S(-12,  35),   S(-12,  27),
            S( 32,  20),   S( 17,  48),   S( 18,  29),   S( -3,  38),   S(  5,  19),   S(  9,   2),   S( -3,  30),   S(  3,  28),

            /* rooks: bucket 1 */
            S(-77,  35),   S(-55,   7),   S(-58,  -3),   S(-43, -12),   S(-29, -22),   S(-29, -21),   S(-35, -10),   S(-39,  20),
            S(-43,  12),   S(-57,  18),   S(-17, -13),   S(-27, -32),   S(-30, -13),   S(-40, -14),   S(-42, -20),   S(-60,  14),
            S(  1,   7),   S(-26,  30),   S(-19,  10),   S(-39,  19),   S(-44,  26),   S( -4,  -4),   S(-24,   5),   S(-46,  18),
            S(-50,  52),   S(-34,  31),   S(  6,  16),   S(-12,  19),   S(-23,  29),   S(-41,  39),   S(-34,  37),   S(-31,  14),
            S( 48,  15),   S( 33,  33),   S( 25,   3),   S(-36,  39),   S(-20,  38),   S( 17,  19),   S( -1,  17),   S(-41,  24),
            S( 42,  11),   S(  8,  29),   S(  7,  24),   S(-32,  27),   S( 12,  11),   S(-28,  42),   S(-10,  24),   S(-43,  32),
            S(-16,  30),   S(  8,  27),   S( 22,  26),   S(-46,  48),   S(-24,  33),   S(  4,  33),   S(-36,  27),   S(-54,  34),
            S( 26,  31),   S( 32,  35),   S(  0,  28),   S(-45,  54),   S(  0,  15),   S( 24,  13),   S(-18,  35),   S( -6,  15),

            /* rooks: bucket 2 */
            S(-69,  38),   S(-48,  18),   S(-48,  15),   S(-58,  14),   S(-62,  12),   S(-50,   7),   S(-34, -22),   S(-50,  28),
            S(-80,  46),   S(-63,  36),   S(-46,  25),   S(-54,   9),   S(-42,  -3),   S(-55,   0),   S(-69,  19),   S(-63,  16),
            S(-74,  61),   S(-59,  49),   S(-56,  50),   S(-33,   8),   S(-45,  23),   S(-27,  20),   S(-17,  12),   S(-35,  21),
            S(-72,  63),   S(-56,  65),   S(-38,  60),   S(-33,  46),   S(-26,  31),   S(  5,  31),   S(-35,  51),   S(-20,  32),
            S(-25,  50),   S(-48,  66),   S(-44,  56),   S(-18,  39),   S( 26,  24),   S( 20,  28),   S(-25,  49),   S(-40,  46),
            S(-38,  45),   S(-33,  46),   S(-15,  28),   S( -4,  21),   S( 20,  25),   S( 48,  15),   S( 23,  19),   S(-18,  29),
            S(-52,  40),   S(-67,  68),   S(-34,  52),   S(-11,  47),   S( 12,  26),   S( 25,  19),   S(-52,  60),   S(-34,  47),
            S(-35,  67),   S(-13,  47),   S(-69,  64),   S(-27,  42),   S(-48,  56),   S(-27,  60),   S(-50,  74),   S(-22,  46),

            /* rooks: bucket 3 */
            S( -7,  73),   S( -7,  65),   S( -3,  57),   S(  4,  45),   S(  1,  46),   S(-18,  68),   S( -9,  77),   S( -5,  39),
            S(-33,  84),   S(-12,  64),   S(  2,  56),   S(  6,  50),   S( 16,  44),   S(  8,  55),   S( 39,   3),   S( 19, -35),
            S(-35,  80),   S(-15,  80),   S( -2,  72),   S( 14,  53),   S(  8,  73),   S( 24,  65),   S( 29,  66),   S(  4,  50),
            S(-24,  89),   S(-17,  83),   S( 20,  70),   S( 27,  64),   S( 24,  68),   S( -2, 107),   S( 60,  60),   S( 20,  70),
            S(-14,  98),   S( 23,  78),   S( 18,  68),   S( 39,  66),   S( 42,  66),   S( 48,  66),   S( 90,  52),   S( 56,  45),
            S(-13,  90),   S( 12,  74),   S( 12,  69),   S( 18,  67),   S( 29,  51),   S( 51,  47),   S( 87,  31),   S( 94,  14),
            S(-35, 100),   S(-17,  99),   S( -8,  91),   S( 26,  76),   S( 18,  72),   S( 32,  69),   S( 62,  63),   S(110,  27),
            S(-77, 151),   S( -8, 102),   S( 10,  76),   S( 40,  64),   S( 52,  54),   S( 56,  66),   S(119,  48),   S(103,  47),

            /* rooks: bucket 4 */
            S(-88,  27),   S(-13,   0),   S(-45,   6),   S(-28,  18),   S(-32, -19),   S(  8, -51),   S( -6, -22),   S(-13, -35),
            S(-35,   2),   S(-43,   6),   S(-46,  16),   S(-40,  23),   S(-12, -10),   S(-16, -24),   S(  4, -36),   S(-20, -22),
            S(  0,  13),   S(-25, -17),   S(-14,  11),   S(-14,  -9),   S( -3,  -6),   S( -6,  -8),   S( 33, -16),   S(-45,   0),
            S(-32, -11),   S(  2,   6),   S(-24,  17),   S( 27,   1),   S( 19,   3),   S( 17,  -2),   S( 15,  10),   S( -7,  12),
            S(-17, -10),   S( -3,  31),   S(-10,  21),   S( 75,   6),   S( 23,  21),   S(  3,  16),   S( 40,  29),   S( 31,   1),
            S( 25,  10),   S( 25,  13),   S( 53,  15),   S( 42,  13),   S( 33,  16),   S(  4,  35),   S(  7,  27),   S( 25,  31),
            S(  3,  -3),   S( 38,  30),   S( 31,  29),   S( 38,  22),   S( 53,   9),   S( 10,   2),   S( 32,  18),   S( 28,  22),
            S( 38, -54),   S( 40,  45),   S( 16,  29),   S( 12,  18),   S( 17,   4),   S(  9,  24),   S( 14,   5),   S( 15,  18),

            /* rooks: bucket 5 */
            S(-50,  34),   S(-59,  53),   S(-67,  52),   S(-59,  37),   S(-47,  25),   S(-45,  40),   S(-12,  25),   S(-42,  44),
            S(-42,  36),   S(-37,  30),   S(-85,  67),   S(-56,  37),   S(-42,  23),   S(-22,  16),   S(  5,  14),   S(-36,  23),
            S( -8,  48),   S(-47,  60),   S(-56,  61),   S(-63,  59),   S(-34,  29),   S(-12,  30),   S(-10,  41),   S(-13,  42),
            S(-33,  74),   S( -5,  46),   S(-25,  65),   S(-13,  42),   S(-13,  54),   S(  4,  59),   S( -4,  53),   S(  3,  35),
            S( 13,  61),   S(  5,  63),   S( 41,  44),   S( 33,  58),   S( 38,  52),   S( 16,  72),   S( 65,  59),   S( 28,  40),
            S( 60,  56),   S( 35,  63),   S( 57,  52),   S( 28,  69),   S( 58,  48),   S( 53,  57),   S( 51,  47),   S( 42,  42),
            S( 45,  39),   S( 22,  64),   S( 43,  53),   S( 62,  41),   S( 36,  48),   S( 44,  56),   S( 64,  48),   S( 65,  44),
            S( 88,  31),   S( 70,  32),   S( 35,  55),   S( 20,  36),   S( 48,  45),   S( 48,  48),   S( 45,  42),   S( 21,  48),

            /* rooks: bucket 6 */
            S(-59,  27),   S(-59,  40),   S(-40,  31),   S(-41,  26),   S(-66,  38),   S(-88,  68),   S(-56,  55),   S(-47,  54),
            S(-45,  35),   S(-31,  32),   S(-29,  31),   S(-48,  25),   S(-55,  45),   S(-75,  63),   S(-67,  57),   S( 12,  16),
            S(-41,  59),   S(-29,  38),   S(-13,  40),   S(-45,  43),   S( -3,  30),   S(-39,  62),   S(-29,  74),   S(  3,  41),
            S(-40,  71),   S( 26,  45),   S( -6,  58),   S(  9,  38),   S(  6,  41),   S(  0,  54),   S(-39,  61),   S(-18,  57),
            S(  1,  70),   S( 37,  58),   S( 55,  45),   S( 37,  41),   S( 24,  59),   S( 42,  49),   S( 42,  45),   S( 13,  56),
            S(  8,  62),   S( 58,  50),   S( 80,  31),   S( 45,  31),   S( 34,  44),   S( 49,  57),   S( 56,  47),   S( 62,  48),
            S( 33,  59),   S( 69,  41),   S( 76,  32),   S( 91,  17),   S( 97,  24),   S( 48,  53),   S( 51,  51),   S( 48,  47),
            S( 53,  72),   S( 28,  63),   S( 34,  48),   S( 43,  40),   S( 66,  45),   S( 55,  63),   S( 59,  60),   S( 21,  65),

            /* rooks: bucket 7 */
            S(-77, -11),   S(-52,  -8),   S(-47, -17),   S(-35,  -9),   S(-26,  -6),   S(-60,  35),   S(-50,  22),   S( -7, -13),
            S(-75,  21),   S(-46,   5),   S(-49,   4),   S(-19, -11),   S(-22,  11),   S( -9,  11),   S(-19,  -1),   S(-55,  11),
            S(-90,  50),   S(-43,  16),   S(-19,   8),   S(-10, -11),   S(-10,   5),   S(-19,  -6),   S(-17, -10),   S( 14,   8),
            S(-65,  41),   S( -8,  21),   S(  5,  12),   S( 20,   9),   S( 32,  -3),   S( 32,   4),   S( 35,   1),   S( -7,   8),
            S(-24,  39),   S( 12,  11),   S( 50, -14),   S( 56,  -8),   S( 76,  -4),   S(103,   0),   S( 80,   3),   S( 49, -14),
            S(-18,  32),   S( 15,  12),   S( 81, -27),   S( 98, -25),   S( 76,  -8),   S( 77,  16),   S( 76,  18),   S( 27,   4),
            S(-12,  33),   S( 20,  16),   S( 49,   1),   S( 66,  -1),   S( 94,  -8),   S( 93,  -7),   S( 42,  27),   S( 18,  10),
            S(  7,  60),   S(-25,  42),   S( 34,   1),   S( 78, -24),   S( 27,   4),   S( 20,  16),   S( 46,   7),   S( 63,  -5),

            /* rooks: bucket 8 */
            S(-48, -44),   S(-12, -10),   S(  2,   3),   S( -3, -15),   S(-13, -42),   S(-12, -54),   S(-16, -25),   S( -7, -17),
            S( -3, -18),   S( -5,  -6),   S(  1, -11),   S(  8, -13),   S( -7, -29),   S(-10, -24),   S( -7, -44),   S(-17, -64),
            S(  7,  17),   S(  8, -19),   S(  3,   5),   S(  9,   9),   S(-15, -33),   S( -5, -34),   S( 12,  20),   S( -2,   0),
            S( -7, -18),   S( -3,  26),   S( -7,   5),   S( 21,   4),   S(  7,  13),   S( -5, -13),   S(  9, -17),   S(  3,   0),
            S( -5, -11),   S(  1,  16),   S( -1,  28),   S( 16,   9),   S(  2,   5),   S( 21,   2),   S( 11, -14),   S( 13, -36),
            S(  8,  30),   S( -4,   9),   S( 34,  40),   S( 29,  -8),   S(  2,  -5),   S(  7, -14),   S(  4,  -1),   S( 12,  41),
            S(  5, -10),   S( 14, -15),   S( 26,   4),   S( 21, -19),   S( 30,   7),   S( 21, -21),   S( 20, -14),   S( 18,  -6),
            S(  6, -146),  S( 12, -10),   S( 23,   8),   S( -1,  -9),   S(  3,   1),   S(  4, -13),   S(  7,  -8),   S( 22,  -3),

            /* rooks: bucket 9 */
            S(-50, -15),   S(-12, -21),   S(-24, -26),   S(-39,  -2),   S(-20,   4),   S( -8,  -4),   S( 10, -42),   S(-39, -33),
            S( 29, -19),   S(  3, -17),   S(-17, -18),   S(-19,  -3),   S(-18, -11),   S( 18,   6),   S(  3, -29),   S(-13, -28),
            S( 10, -16),   S( 18,  -5),   S(  3,   5),   S( -9,  -1),   S( -8, -20),   S( 26,  -6),   S( 14,  20),   S( -3,   0),
            S(  3,  10),   S( 10,   4),   S( 13,  20),   S( -2,   4),   S(  7,  17),   S( 23,  -4),   S( 16,  35),   S( 14,   0),
            S( 12,   9),   S(  5,  12),   S(  6,  28),   S( 15,  21),   S( 35,  27),   S( 26,  27),   S( 11,  -1),   S( 14,  -8),
            S( 17,  45),   S( -6,  11),   S( 12,   4),   S(-12,   1),   S( 15,   4),   S( 32,   8),   S(  9,  35),   S( 16,  13),
            S( 64,  18),   S( 61,   6),   S( 33,  29),   S( 54,  11),   S( 32,  -8),   S( 31,   6),   S( 39,   1),   S( 46,  25),
            S( 68, -70),   S( 41, -28),   S( 22,  29),   S( 33,  27),   S( 12,  38),   S( 27,  17),   S( 26,  10),   S( 32,   8),

            /* rooks: bucket 10 */
            S(-60, -78),   S(-18, -48),   S(-49, -25),   S(-34,  -3),   S(-36,  -2),   S(-31,  -9),   S(  8, -12),   S(-36, -17),
            S( -4, -15),   S(  9, -27),   S( -4, -25),   S( -5, -15),   S(  2, -17),   S( -9,  -2),   S( 36,   4),   S(  8,   0),
            S(-15, -17),   S(-13, -21),   S(  3, -17),   S( 19,  -4),   S(-17,  19),   S( -3,  -6),   S( 25,  26),   S(  6,  -4),
            S(  4,   1),   S(  7, -12),   S(  2,  -7),   S(  6,  14),   S( 29,  -5),   S(  3,  -6),   S( 26,  25),   S( -1,  -9),
            S(  8,  13),   S( 32,   8),   S( 13,   9),   S( 20, -21),   S( -3,  -4),   S( 15,  10),   S( 31,  31),   S(  9,  28),
            S( 40,  30),   S( 31,  42),   S( 23,  10),   S( 22,   7),   S(  3,  -9),   S( 19,  10),   S( 36,  20),   S( 11,  38),
            S( 75,  12),   S( 80,   2),   S( 78,  -6),   S( 72, -18),   S( 55, -16),   S( 39,  15),   S( 27,   7),   S( 33,   6),
            S( 60,  15),   S(  9,  -2),   S( 40,   0),   S( 23,   7),   S( 36,  -1),   S( 30,  15),   S( 15,   3),   S( 20, -11),

            /* rooks: bucket 11 */
            S(-42, -45),   S(-31, -25),   S(-20, -28),   S(-30, -55),   S(  0, -22),   S( -6,   5),   S(-26, -29),   S(-55, -15),
            S(-17, -28),   S( -6, -43),   S( -2, -29),   S( -2, -28),   S( -5, -23),   S(-17, -16),   S( -1, -31),   S(-22,   3),
            S(  2, -31),   S( 18, -13),   S( 21, -15),   S( 11, -21),   S( 11, -10),   S( -9,  11),   S(-24, -26),   S(-10, -52),
            S( -2,  27),   S( -2, -10),   S( -2,  10),   S( 14,   6),   S(  4,  -5),   S( 15,  29),   S( 28,  -9),   S(  2, -23),
            S( 12,  11),   S( 19, -11),   S( 29,  -1),   S( 24,  -9),   S( 27,  -5),   S( 33, -10),   S(  9,   8),   S(  0,  -9),
            S( 27,  33),   S( 45,   6),   S( 28, -11),   S( 50,  20),   S( 52,  20),   S( 43,   9),   S( -2,   5),   S( 18,  27),
            S( 64,  36),   S( 62,   2),   S( 70, -14),   S( 76, -15),   S( 48, -10),   S( 54,  12),   S( 36,  34),   S( 58,  -1),
            S( 45,  33),   S( 14,  28),   S( 23,   6),   S( 11,  -8),   S( -7,  -3),   S( 20,  19),   S( 15,  13),   S( 34,  10),

            /* rooks: bucket 12 */
            S( -3,  -8),   S( -9, -30),   S(-13, -52),   S( -5, -10),   S(  0,  -5),   S( -4, -34),   S(-22, -62),   S(-24, -52),
            S(  8,   7),   S( -6, -22),   S(-12, -18),   S( -7, -18),   S(-10,  -7),   S( -8, -16),   S(  1,  -2),   S(-11, -32),
            S(  3,   1),   S( -6, -19),   S( -8, -25),   S(-13,  -9),   S( -5, -22),   S(  6,  -7),   S( -7, -10),   S(  5,  -9),
            S( -7,  -8),   S( -1, -13),   S(  3,  12),   S(  8, -11),   S(  1,  -8),   S(-10, -39),   S( -8, -11),   S( -4, -39),
            S( -3,  -8),   S( -2, -19),   S( 12,   4),   S(  9,   7),   S( -8, -37),   S(  6, -19),   S( -6,  -9),   S(  1, -16),
            S( -2,  -7),   S( -3,  -7),   S( 19,  32),   S(  9,  -6),   S( -4,  -7),   S( -6, -21),   S(  1, -25),   S(  4,   8),
            S( -3,  -3),   S(  4, -27),   S(  4, -41),   S( 13,   2),   S(  7,  -4),   S( -6, -40),   S( -3,  -9),   S(  9, -17),
            S( -4, -40),   S(  8,  23),   S(  4, -20),   S(  2,   2),   S( -3, -25),   S(-11, -49),   S(-14, -30),   S(  8,  -4),

            /* rooks: bucket 13 */
            S(-14, -40),   S( -6, -24),   S( -4, -17),   S(  0,  11),   S(  5,  -4),   S(-13, -39),   S(  1, -23),   S(-18, -33),
            S( -2, -33),   S( -2, -13),   S(-12,  -6),   S( -8,  -3),   S(-10, -18),   S( -1, -12),   S(  4,   1),   S( -4, -21),
            S( -5, -29),   S( -7, -26),   S( -5, -37),   S( -2, -23),   S( 10,  11),   S(  2,  -6),   S(  1, -23),   S(  2, -32),
            S( -7, -52),   S(  2,  -5),   S( -9, -43),   S( -5, -11),   S( 13,  12),   S( -9, -38),   S( -2, -28),   S(  2, -19),
            S( 12, -20),   S(  9, -18),   S( 17,  25),   S( -5,  -9),   S(-10, -29),   S(  4, -15),   S( -6, -39),   S(  9,  -9),
            S( -7, -39),   S( 11, -27),   S( -7, -10),   S( 15,  -7),   S(  7, -13),   S( 11,  16),   S(  9,  -3),   S(  4,   8),
            S(  6,  -2),   S(  9,  20),   S( 11,  11),   S(  2, -16),   S( 12, -27),   S( 21,   6),   S(  4, -14),   S(  3, -18),
            S(-14, -122),  S(-16, -67),   S(  6,   6),   S(  1,   0),   S( -3,  15),   S( -3, -29),   S(-10, -27),   S(  5,   2),

            /* rooks: bucket 14 */
            S( -8, -36),   S(-16, -47),   S( -2,  -8),   S( -3, -35),   S(  3, -22),   S( -9, -21),   S( 10,  -5),   S( -7, -22),
            S(-21, -43),   S(-13, -54),   S(-10,   4),   S(-14, -36),   S(-11, -17),   S(  1, -32),   S(  6,  23),   S(  6, -11),
            S( -2, -24),   S( -8, -19),   S( -4, -18),   S( -6, -11),   S(-14, -27),   S( -7, -22),   S(  7,  22),   S( -2, -27),
            S( 12,   5),   S( -7, -34),   S( -4, -19),   S( -5,   6),   S(  3, -13),   S(  4, -13),   S( -4, -35),   S( -3, -22),
            S(  1, -14),   S(  3, -25),   S( -7, -29),   S( -9, -25),   S( -6, -18),   S( -4, -19),   S(  3,   6),   S(  9,   2),
            S(  3, -14),   S(  0, -24),   S(  1, -18),   S(  2, -21),   S(-11, -19),   S( -8,   6),   S(  7,   8),   S(  0,  -6),
            S( 19,   0),   S(  2, -35),   S(  4, -21),   S(  2, -29),   S(  7, -43),   S(  7,   2),   S(  9,  12),   S( 10,   8),
            S( -2, -26),   S(  4, -16),   S( -9, -29),   S( 10,  12),   S( -9, -17),   S(  3,   8),   S(  6,  17),   S(  0, -17),

            /* rooks: bucket 15 */
            S( -2, -54),   S(-13, -42),   S( -1, -27),   S( -7, -28),   S(  0, -16),   S( -4,  -8),   S(-17, -52),   S( -9, -15),
            S(-14, -20),   S(-14, -27),   S(  2,  -2),   S( -6, -23),   S(-10, -27),   S(  6, -26),   S(-11, -41),   S(  7,   5),
            S( -9, -23),   S(-11, -23),   S( -2, -25),   S(  2,   0),   S(  9, -30),   S( -3,  -8),   S( -3,   4),   S( -4, -12),
            S(  2, -30),   S( -4, -26),   S(-11, -18),   S( -5, -19),   S(-11, -19),   S(  3, -18),   S(  0, -18),   S( -9,  -2),
            S(  0, -11),   S( -5, -12),   S( 10,  -7),   S(  0, -13),   S(  1,  -2),   S(  2,  -2),   S( -1,   7),   S(  0,  16),
            S(  7,  19),   S(  2,   1),   S(  1, -12),   S(  0, -10),   S( -6,  -9),   S(  1,  12),   S(  6,  -9),   S( -7, -13),
            S( 12,  19),   S( 11,  -7),   S(  8, -34),   S( -4, -33),   S(  1, -21),   S( 12,  36),   S(  3,  -1),   S(  0,  13),
            S(  2, -17),   S( -7, -19),   S(  2,  -6),   S(  1, -11),   S( -6, -13),   S(  0, -25),   S(  1, -17),   S(  2,  -3),

            /* queens: bucket 0 */
            S(-17, -10),   S(-17, -53),   S( 47, -84),   S( 55, -58),   S( 33, -37),   S( 19,  -3),   S( 55,   9),   S( 21,  19),
            S( -9, -11),   S( 30, -58),   S( 38, -16),   S( 21,   5),   S( 20,  31),   S( 22,  21),   S(  8,  62),   S( 38,  20),
            S( 25,   5),   S( 37,  16),   S( 20,  29),   S( 17,  38),   S( 17,  18),   S(  9,  19),   S(  8,  31),   S( 35,  32),
            S( 19,  23),   S( 25,  46),   S(  7,  47),   S( 11,  45),   S(  9,  55),   S( 15,  34),   S( 17,  26),   S( 21,  28),
            S( 41,  52),   S( 27,  43),   S( 18,  40),   S( 25,  52),   S( -4,  29),   S( -4,  13),   S( 34,  20),   S( 45,  -2),
            S( 29,  61),   S( 26,  53),   S( 14,  38),   S( 22,  14),   S( 47,  -9),   S(  7,  36),   S( 29,  19),   S( 27, -19),
            S( 50,  51),   S( 53,  42),   S( 33,  37),   S( 51,  25),   S( 22,   7),   S( -6,  -9),   S( 33,  24),   S( 34,  11),
            S( 48,  30),   S( 23,  37),   S( 43,  18),   S( 37,  35),   S( 46,  32),   S(-12,   3),   S( 52,  28),   S( 48,  29),

            /* queens: bucket 1 */
            S(  0, -16),   S(-74, -23),   S(-51, -28),   S(-14, -68),   S(-10, -25),   S(-20, -48),   S( 13, -29),   S( 10,  27),
            S(-17, -25),   S(-13, -44),   S(  7, -51),   S( -6,   4),   S(-11,   1),   S(  4,  -3),   S( 19, -40),   S( -1,  23),
            S(-32,  42),   S( -2,  -7),   S(  1,  15),   S( -8,  10),   S( -5,  32),   S(-17,  33),   S( 13,   9),   S( 17,  22),
            S(  9, -21),   S(-11,  31),   S(-15,  37),   S( 10,  43),   S( -7,  54),   S(  2,  29),   S(  3,  -4),   S( 18,  17),
            S( 13,   8),   S(  6,  23),   S( -5,  62),   S(-19,  60),   S(-15,  53),   S(  0,  15),   S( -8,  16),   S(  1,  36),
            S( 12,  25),   S( 13,  52),   S( 15,  58),   S(-36,  54),   S(-16,  47),   S(-33,  44),   S( 28,  23),   S( 19,  45),
            S(  4,  33),   S(-11,  68),   S(-19,  32),   S(-25,  68),   S(-26,  47),   S( 16,  25),   S( -8,  38),   S(-25,  47),
            S( -5,   6),   S(  5,  16),   S( 15,  26),   S( -9,  10),   S( -5,  13),   S(  6,  12),   S( 10,  27),   S( -9,  30),

            /* queens: bucket 2 */
            S(  7,  18),   S( 13, -36),   S(  5, -19),   S( -5, -10),   S(-23,   6),   S(-28, -12),   S(-28, -21),   S( 16,  11),
            S( 14,  13),   S( 10,  34),   S( 16, -13),   S( 16, -23),   S( 11, -29),   S( 14, -49),   S( 10,  -9),   S( 34, -24),
            S( 14,  10),   S( 16,   6),   S(  1,  46),   S(  6,  35),   S(  0,  57),   S( 13,  45),   S( 10,  19),   S( 30,  12),
            S(  7,  20),   S( -2,  51),   S( -2,  42),   S(  4,  56),   S(-19,  82),   S( -3,  81),   S( 16,  15),   S(  5,  65),
            S( 15,   6),   S( -7,  53),   S(-13,  56),   S(-34,  97),   S(-41, 109),   S(-15,  77),   S( -9, 102),   S( -6, 106),
            S( 12,  24),   S(  0,  41),   S(-28,  75),   S(-10,  52),   S(-30,  89),   S(-12,  93),   S( -3,  94),   S( 11,  76),
            S(-18,  52),   S(-35,  76),   S(-16,  59),   S(  6,  60),   S(-21,  74),   S( 25,  41),   S(-20,  44),   S( -9,  76),
            S(-64,  75),   S(  5,  33),   S( 30,  35),   S( 33,  29),   S(  2,  61),   S( 19,  32),   S( 15,  25),   S(-15,  41),

            /* queens: bucket 3 */
            S( 82,  92),   S( 55,  97),   S( 48, 101),   S( 41,  83),   S( 67,  32),   S( 45,  25),   S( 20,  26),   S( 47,  64),
            S( 66, 114),   S( 60, 108),   S( 44, 112),   S( 46,  87),   S( 46,  77),   S( 60,  48),   S( 63,  11),   S( 38,  54),
            S( 60,  89),   S( 52, 105),   S( 55,  83),   S( 52,  78),   S( 48,  92),   S( 52,  96),   S( 60,  96),   S( 63,  75),
            S( 48, 123),   S( 61,  83),   S( 49,  94),   S( 42,  94),   S( 45,  93),   S( 40, 129),   S( 59, 102),   S( 51, 136),
            S( 64,  91),   S( 58, 103),   S( 52,  88),   S( 39,  94),   S( 35, 114),   S( 27, 127),   S( 39, 165),   S( 54, 156),
            S( 49, 122),   S( 58,  98),   S( 53,  93),   S( 26, 116),   S( 35, 130),   S( 75, 100),   S( 65, 136),   S( 36, 191),
            S( 63, 115),   S( 61, 102),   S( 71,  83),   S( 61,  92),   S( 35, 109),   S( 61, 111),   S( 94, 126),   S(161,  72),
            S( 79,  90),   S(105,  74),   S( 78,  83),   S( 83,  76),   S( 44, 103),   S(113,  50),   S(138,  56),   S(146,  56),

            /* queens: bucket 4 */
            S(-12, -23),   S(-19, -20),   S(-22,  -8),   S(-11,  -9),   S( 10, -17),   S( 35,  -2),   S(-34,  -9),   S(-25,  -3),
            S(-28, -17),   S(-26,  -4),   S( 14,  -8),   S(-41,  23),   S( -1,  -6),   S(  1, -14),   S( -8, -11),   S(-35, -16),
            S(  3,   2),   S( 13,  -1),   S(  3,  29),   S( -2,  31),   S( 21,  16),   S(  4,  -8),   S(  7, -20),   S(-25, -25),
            S(-14,   4),   S( -8,  14),   S(  2,  37),   S( -2,  31),   S( 14,  34),   S( 19,  18),   S(  3, -14),   S( -4,  -8),
            S( -8,   0),   S( 18,  14),   S( 17,  29),   S( 29,  42),   S( 22,  29),   S( 19,  -1),   S(-21, -18),   S( -8, -29),
            S(  5,  14),   S( 35,  14),   S( 25,  54),   S( 22,  43),   S( 11,   7),   S(  2,   3),   S(-16, -14),   S(-14,  -9),
            S(-10, -17),   S( -5,  18),   S(  3,  26),   S( 31,  32),   S(  9,  10),   S(-12,  -4),   S(-22, -42),   S(-21, -25),
            S( -2, -16),   S( -1,  -2),   S( 30,  36),   S(  4,  18),   S(-17, -18),   S( -7, -11),   S(-19, -33),   S( -8, -18),

            /* queens: bucket 5 */
            S(-37, -15),   S(-26, -31),   S(-32, -29),   S(-45, -28),   S(-57, -30),   S(  7, -16),   S(-10,  -6),   S( -6,  -7),
            S(-29,  -5),   S(-39, -13),   S(-70, -20),   S(-67,  -3),   S(-15,  -3),   S(-42, -16),   S(-48, -17),   S(-50, -16),
            S(-35,   4),   S(-58, -11),   S(-65,   4),   S(-31,  33),   S( 15,  52),   S(-12,  22),   S( -3,  -1),   S( 10,  20),
            S(-54, -10),   S(-52,  -3),   S( -1,  36),   S( -5,  52),   S( 12,  29),   S( -3,  13),   S( -5,  -8),   S( -8,  14),
            S(-33,  -6),   S(-21,  20),   S(-11,  48),   S( -7,  46),   S( 27,  49),   S(  0,  17),   S(  0,   8),   S(-30, -29),
            S(-17,  17),   S(  6,  37),   S(-12,  42),   S(  1,  45),   S( 39,  50),   S(  3,  13),   S(  1,   2),   S(-11, -12),
            S( -9,   9),   S(-10,  13),   S(  5,  59),   S( -3,  33),   S(  0,  37),   S( 21,  32),   S( 11,   9),   S(-21, -16),
            S(  8,  26),   S( 11,  11),   S(  3,  18),   S( 11,  49),   S( 16,  29),   S(  3,  20),   S( -1, -24),   S(-18, -16),

            /* queens: bucket 6 */
            S(-32,   4),   S(-53, -26),   S(-67, -29),   S(-88, -58),   S(-92, -51),   S(-71, -45),   S(-51, -43),   S(-28,   4),
            S(-63, -13),   S(-47,  -1),   S(-56,  14),   S(-63,  11),   S(-79,  17),   S(-92,  -2),   S(-85, -19),   S(  7,  18),
            S(-46,  10),   S(-23,  11),   S(-57,  40),   S(-100,  87),  S(-38,  50),   S(-36,   4),   S(-48, -13),   S(  3,   7),
            S(-44,  12),   S(-27,   9),   S(-29,  62),   S(-49,  70),   S(  4,  44),   S( 12,  50),   S(-10,  36),   S( 11,  -7),
            S(-53,  19),   S( -6,  36),   S(-28,  52),   S(  8,  30),   S( 29,  55),   S( 59,  38),   S( 25,  33),   S( -6,  18),
            S(-25,  41),   S(-12,  18),   S( 22,  21),   S( 22,  45),   S(  7,  53),   S( 62,  66),   S( -5,  -6),   S(-15,  11),
            S( -7,   6),   S(  1,   1),   S(-14,  40),   S(-11,  35),   S( 28,  50),   S( 17,  58),   S( -9,  22),   S(-38,  -1),
            S( -1,   6),   S( 17,  11),   S( 11,  29),   S( -4,  22),   S( 30,  38),   S( 19,  27),   S( -3,  14),   S(  3,   7),

            /* queens: bucket 7 */
            S(-10, -14),   S(-36,  11),   S(-51,  19),   S(-38,   9),   S(-32, -10),   S(-35, -24),   S(-29,  -5),   S(-14, -10),
            S(-35, -10),   S(-53,   3),   S(-31,   9),   S(-25,  36),   S(-33,  32),   S(-46,  39),   S(-43,  22),   S(-31, -12),
            S(-36, -24),   S(-56,  31),   S(-22,  32),   S(-15,  29),   S(  4,  22),   S(  1,  30),   S( -7,  16),   S(-13,   3),
            S(-63,   1),   S(  6,   0),   S(-20,  24),   S( -6,  40),   S( 31,  22),   S( 33,  24),   S( 14,  34),   S(  2,  22),
            S(-30,  18),   S(-52,  23),   S(  9,  18),   S( 45,  -4),   S( 63, -12),   S( 83, -16),   S( 37,  12),   S( 44,  -7),
            S(-16,  11),   S(-17,   6),   S(  4,  -1),   S( 15, -10),   S( 37,  35),   S( 82,  20),   S( 65,   3),   S( 49,  12),
            S(  8, -20),   S(  1,   9),   S(  1,  -7),   S(  5,  12),   S( 39,  16),   S( 55,  37),   S( 52,  18),   S( 54,  28),
            S( 15,   3),   S( 20,   3),   S( 21,   7),   S( 21,  15),   S( 41,  23),   S( 24,  19),   S( 16,   6),   S( 40,  43),

            /* queens: bucket 8 */
            S( -5, -10),   S( -2,   3),   S(-13,  -5),   S( -9,  -7),   S( -4,   0),   S( -1, -16),   S(-20, -24),   S( -4,   4),
            S( -7,   0),   S(-11, -15),   S( -4,   5),   S(-13,  -3),   S( -5,  -5),   S(-17, -20),   S(-18, -39),   S( -5,  -9),
            S( -1,   0),   S( -6,   2),   S( -6,   2),   S( -6, -11),   S( -5,   4),   S(-12, -12),   S(-11, -26),   S(-15, -28),
            S( -3,   3),   S(  9,  19),   S( 12,  18),   S(  6,  12),   S( -2,   0),   S( -6,  -1),   S( -1,  -3),   S( -7, -21),
            S( 16,  28),   S(  3,  28),   S( 12,  14),   S( 12,  20),   S( 13,  30),   S(  3,   0),   S( -8, -11),   S(-10, -17),
            S(  8,  20),   S( 13,  22),   S(-17,  16),   S( 15,  35),   S( -8, -14),   S( -5, -11),   S(  4,   2),   S(  3,  12),
            S( -6, -12),   S(-16, -25),   S( 22,  36),   S( 15,  16),   S(  2,  17),   S(  3,  18),   S( -2,  -7),   S( -6, -16),
            S(-14, -28),   S( 14,  12),   S(-15, -47),   S( -9,  -6),   S(-11, -30),   S( -1,  -6),   S( -3, -17),   S( -5,  -7),

            /* queens: bucket 9 */
            S(  5,   7),   S(-13, -28),   S(  2,  -2),   S(-30, -31),   S(-23, -38),   S(-18, -30),   S(-13, -21),   S(-13, -18),
            S( -3,  -5),   S( -9,  -7),   S(-19, -24),   S( -4,   0),   S(-17,  -8),   S(-15, -18),   S(  2,  -1),   S( -3,  -6),
            S(  4,   6),   S(  4,   9),   S( -8,  21),   S( -4,  -5),   S( -4,   9),   S(  1,  -2),   S(  3,   2),   S(  3,  -1),
            S( -4,  -9),   S( -5,   5),   S( 13,  41),   S(  8,  23),   S( 18,  31),   S(  5,  13),   S( -9, -16),   S(  0, -10),
            S(  6,  11),   S(  9,  32),   S( 12,  33),   S( 18,  52),   S( 21,  34),   S( 10,  19),   S( -3,   4),   S(-11, -14),
            S(-18, -20),   S(-15,  -2),   S(  5,  22),   S( 16,  35),   S( -4,   2),   S( -1,  10),   S( -9,  -6),   S( -5,  -6),
            S( -6, -16),   S(-10, -25),   S( -7,  24),   S( 12,  30),   S( 17,  21),   S(  8,  -5),   S(  6,  -4),   S(-12, -25),
            S(  0,  -1),   S( -3, -22),   S( 12,  -1),   S(  1,  15),   S( 14,   2),   S( -2,   0),   S( 11,   2),   S(  2, -16),

            /* queens: bucket 10 */
            S(  3,  -1),   S( -3,   4),   S(-11, -18),   S(-22, -26),   S(-12, -14),   S( -6,  -5),   S(  2, -12),   S( -6,  -9),
            S( -8, -12),   S( -8, -15),   S(-15, -25),   S( -9, -12),   S( -5,  -7),   S(-18, -13),   S(  1,  -8),   S(-17, -19),
            S( -2, -13),   S( -9, -14),   S( -8,  -9),   S( -1,   3),   S( -6,   3),   S( -7,   3),   S(  1,   2),   S(  2,   6),
            S(  0,  -2),   S(  1,  -4),   S( -3,  -6),   S(  0,  31),   S( 15,  25),   S( -6,   6),   S( -3,  -5),   S(-14, -19),
            S( -5,  -7),   S(  5,  -6),   S( -5,   5),   S( 22,  48),   S(  1,  -2),   S( 17,  30),   S( 12,  14),   S(  1,   6),
            S( -3,  -5),   S(-20, -31),   S( -4,   0),   S(  2,  14),   S(  5,  16),   S(  5,  20),   S( 12,   9),   S( -4, -11),
            S( -6,  -6),   S(-18, -28),   S(  8,  22),   S( -7,  -8),   S(  7,   6),   S(  4,  10),   S( -3,  -7),   S( -8,  -5),
            S(  5,  -1),   S( -3, -16),   S(  6,  -4),   S(  7,  -5),   S( 18,  16),   S(  5,   7),   S( 16,  14),   S(  2,  -8),

            /* queens: bucket 11 */
            S(-10, -14),   S( -7, -20),   S(-22, -21),   S(-11, -28),   S(-12, -19),   S( -9, -11),   S( -6,  -6),   S(-12, -23),
            S(-18, -32),   S( -8,  -8),   S(-41, -36),   S(-11,  -9),   S(-12, -10),   S( -9,  -6),   S( -5,  -9),   S( -6,  -4),
            S(-18, -23),   S(-17, -35),   S(  3, -21),   S( -9, -18),   S( -8, -13),   S( -2,   6),   S(  8,  20),   S(-12,  -8),
            S(-16, -28),   S(-25, -25),   S( -8, -24),   S( 15,  28),   S( 10,   1),   S(-11,  -6),   S( 24,  25),   S( -2,  -1),
            S(-14, -13),   S( -6, -18),   S(-22, -27),   S( 24,  19),   S( 15,  15),   S( 27,  51),   S( 22,  42),   S(  3,  12),
            S(-14, -30),   S(  3,   2),   S(-17, -19),   S( 15,  11),   S( 24,   5),   S( 46,  37),   S( 11,   0),   S( -7,  -5),
            S( -9,  -4),   S(-15, -23),   S(  8,  15),   S(-14,  -5),   S(  6,   5),   S( 23,  25),   S( 38,  39),   S( -2, -16),
            S(-11, -22),   S(-10, -25),   S( -8, -22),   S(  3, -15),   S(  2,   9),   S( -2,  -7),   S( 20,  10),   S(  0, -31),

            /* queens: bucket 12 */
            S(  6,   0),   S( -1,  -2),   S(  2,   1),   S( -8,  -5),   S(-10, -13),   S( -2,  -3),   S(  0,  -2),   S( -4, -10),
            S( -4,  -3),   S( -8, -14),   S( -9, -12),   S( -5, -11),   S( -3,  -3),   S( -6,  -2),   S( -1,  -9),   S( -6,  -9),
            S( -2,  -5),   S( -5,  -9),   S( 11,  13),   S( -5,  -5),   S( -2,  -5),   S( -8, -14),   S(-13, -25),   S( -9,  -8),
            S(  2,   7),   S( -1,   3),   S(  4,   6),   S(  0,   9),   S(  7,  13),   S(  0,  -4),   S(  0,  -4),   S( -4, -12),
            S(  2,  -2),   S( 11,  13),   S( 33,  57),   S(  2,  16),   S( -5,   7),   S(  0,   5),   S(-13, -31),   S( -2, -15),
            S(  8,  19),   S( 14,  25),   S( 35,  43),   S( -2,   8),   S(  0,   5),   S(  2,   2),   S(  5,   5),   S( -5, -15),
            S(  2,   1),   S(  3,   8),   S( 17,  14),   S( 12,   9),   S(  5,   9),   S( -3,   4),   S(  8,   5),   S( -4,  -4),
            S( -3, -26),   S( -8, -24),   S(-12, -19),   S( -9, -27),   S( 11,  -7),   S(  1,  -2),   S(  2,  -5),   S( -6, -11),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -5, -15),   S(  0,  -5),   S( -3,  -8),   S( -3, -11),   S( -3,  -3),   S( -7, -11),   S( -6,  -8),
            S(  4,   9),   S(  5,  13),   S(  4,  10),   S( -4,  -4),   S( -6,  -6),   S(  2,  10),   S(  1,   6),   S(-11, -19),
            S( -3,  -7),   S(  0,   0),   S(  3,  15),   S(  2,  10),   S( -2,  -2),   S( -6,  -9),   S( -5, -10),   S(-12, -17),
            S( -3,  -5),   S(  2,   2),   S( 11,  12),   S( 18,  27),   S( 15,  33),   S( -4,  -7),   S( -5, -13),   S( -5,  -7),
            S( -3,  -1),   S(  6,  17),   S( 16,  41),   S( 13,  38),   S( 23,  45),   S(  0,  -8),   S( -5,  -7),   S( -7, -14),
            S(  0,   0),   S( 12,  32),   S( 39,  74),   S( 19,  41),   S(  1,  16),   S(  1,   7),   S(  6,  14),   S( -5, -14),
            S( -1,   1),   S( 19,  32),   S(  9,  28),   S( 13,  24),   S( -1,  10),   S(  1,  -8),   S( -2, -10),   S(  6,   8),
            S(-11, -16),   S(  5,  -1),   S( -1,  -7),   S( -8, -11),   S(  7,   2),   S(  5,   8),   S( -8,  -7),   S( -6, -13),

            /* queens: bucket 14 */
            S( -1,  -2),   S(  0,   1),   S( -2,  -8),   S( -9,  -9),   S(  4,   6),   S( -2,  -5),   S( -2,  -9),   S( -5, -11),
            S( -5,  -7),   S(  5,  16),   S( -2,  -4),   S( -1,  -7),   S( -9, -12),   S( -7, -15),   S( -5,  -4),   S( -3,  -8),
            S( -2,  -2),   S(-10, -13),   S( -6, -14),   S( -1,  -2),   S(  1,   0),   S(  1,  -5),   S(  3,   5),   S( -6, -14),
            S( -8,  -9),   S(  8,   8),   S( -6,  -4),   S( 23,  41),   S( 14,  15),   S( -1,   6),   S( 11,  24),   S(  1,  -4),
            S(  4,  13),   S(  4,   0),   S(-14,  -8),   S( 16,  27),   S( 14,  32),   S( 17,  25),   S( 10,  18),   S( -4,  -9),
            S( -2,  -5),   S(  4,  13),   S( 14,  24),   S( 12,  20),   S( 18,  42),   S( 15,  46),   S(  8,  16),   S( -2,  -8),
            S(  3,   7),   S(  8,   9),   S( 16,  36),   S( 19,  32),   S( 15,  33),   S( 14,  27),   S( 16,  29),   S(  2,   5),
            S( -3,  -1),   S(  0,   0),   S( -9, -14),   S( 12,  19),   S(  1,   4),   S(  3,   1),   S(  3,   7),   S(-10, -17),

            /* queens: bucket 15 */
            S( -1,  -2),   S(  0,  -6),   S( -5,  -8),   S( -2, -10),   S( -6, -11),   S( -6, -12),   S(-11, -25),   S(  0,  -7),
            S( -1,  -4),   S( -4,  -9),   S( -5, -13),   S( -4, -11),   S(  0,   8),   S( -3,  -8),   S( 11,  13),   S(  2,   1),
            S(  0,  -9),   S( -3, -12),   S( -1,  -2),   S( -4, -12),   S( -4, -11),   S(  6,  16),   S( -2,  -5),   S(  0,  -8),
            S( -5,  -8),   S(  3,   3),   S( -4,  -3),   S(  4,   2),   S(  2,  10),   S(  0,   6),   S(  6,   5),   S(  4,   3),
            S( -2,  -7),   S( -2,  -5),   S( -8, -13),   S( -4,  -5),   S(  5,  10),   S(  8,   6),   S( -3,  -6),   S(  0,  -7),
            S( -3,  -7),   S( -2,  -6),   S( -1,   2),   S(  1,   1),   S( -1,  -6),   S( 20,  32),   S(  4,  -1),   S(  0,  -8),
            S( -6, -13),   S(  4,  -6),   S(  6,   7),   S(  7,   6),   S(  6,   8),   S( 22,  37),   S( 11,  21),   S(  4,   5),
            S(  1,  -4),   S( -5,  -6),   S( -2,  -5),   S( 10,  12),   S(  8,   2),   S(  4,  -3),   S( -2,  -6),   S( -6, -20),

            /* kings: bucket 0 */
            S( 69,  -2),   S( 50,  47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 42,  35),   S(108,  62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
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
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -2,  49),   S(-22,  16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  86),   S(-34,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 21, -62),   S( 72, -27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -12),   S( 14,  19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 41, -15),   S( 19,  -1),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11,  32),   S( -6,  29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 46,  16),   S( 12,  14),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  7,  49),   S( -9,  48),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 58,  32),   S( 18, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 30,  66),   S( -9,  25),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -124),  S(  7, -59),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -106),  S(-98, -17),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -52),   S(-38, -44),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-40, -31),   S(-49, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-16, -38),   S(-23, -33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-37, -23),   S(-93,   9),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-28, -36),   S(-37, -107),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-90,  -5),   S(-13, -88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -110),  S(-77, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -227),  S(-15, -99),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-68, -56),   S( 22, -66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-49, -78),   S(-19, -105),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-12, -56),   S(-115, -14),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23, -118),  S(-63, -68),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-127,   1),  S(-35, -114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-46, -72),   S(  0, -225),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -22),   S(-26,  14),   S( 11,  -3),   S( -8,  22),   S( 20,   0),   S( 43,   7),   S( 48,  -8),   S( 48,   1),
            S(-12, -28),   S(-30,   1),   S(  0, -12),   S( -1, -13),   S( 15,   1),   S( -1,  14),   S( 27,  -2),   S( 21,  24),
            S(  3, -27),   S(  1, -22),   S( 33, -34),   S( 13, -17),   S( 20,  -8),   S( 10,  27),   S( -3,  46),   S( 31,  22),
            S( 11, -19),   S( 31,   2),   S( 51, -28),   S( 36,  -4),   S( 17,  45),   S(-17,  86),   S(  7,  85),   S( 56,  64),
            S( 92, -53),   S(124, -17),   S( 89, -25),   S( 43,  15),   S( 45, 137),   S( -6, 138),   S( 12, 156),   S( 64, 133),
            S(-224, -66),  S(-117, -132), S( 13, -164),  S( 37,  43),   S( 83, 197),   S( 66, 187),   S(109, 168),   S(100, 146),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  20),   S(-44,  25),   S(-18,  10),   S(-41,  56),   S(-14,   1),   S( 15,   7),   S( 15,   0),   S( 15,  26),
            S(-55,  16),   S(-49,  18),   S(-32,   8),   S(-19,   5),   S(  0,   5),   S(-15,   9),   S( -6,   1),   S(-17,  21),
            S(-45,  24),   S(-19,  20),   S(-24,   5),   S(  9,  -9),   S(  1,  19),   S(-21,  18),   S(-30,  30),   S(-12,  28),
            S(-34,  43),   S(  8,  26),   S(-18,  26),   S( 13,  26),   S(  4,  27),   S(-35,  47),   S( -2,  39),   S( 27,  55),
            S(  4,  36),   S( 62,  -3),   S( 92, -26),   S( 87, -23),   S( 33,  29),   S(  1,  36),   S(-26,  80),   S( 36,  92),
            S( 45,  44),   S(-36, -16),   S(-10, -100),  S(-12, -97),   S(-38, -65),   S( -2,  44),   S( 52, 185),   S( 68, 213),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44,  42),   S(-33,  25),   S(-20,  12),   S(-14,  19),   S(-33,  37),   S(-11,  11),   S(  2,  -9),   S( -7,  21),
            S(-55,  33),   S(-40,  28),   S(-32,   8),   S(-27,  17),   S(-25,  16),   S(-34,   7),   S(-13, -12),   S(-39,  12),
            S(-45,  50),   S(-39,  53),   S(-12,  16),   S(-15,  19),   S(-17,  21),   S(-24,   4),   S(-27,   7),   S(-29,  10),
            S(-32,  89),   S(-42,  76),   S(-15,  44),   S(  0,  37),   S( -6,  34),   S(-22,  18),   S(  5,  17),   S( 23,  12),
            S(-30, 132),   S(-46, 118),   S( -8,  23),   S( 24, -25),   S( 99, -11),   S( 92,  -9),   S( 72, -16),   S( 51,   3),
            S(-11, 248),   S( 32, 177),   S( 15,  68),   S( 26, -90),   S(-13, -172),  S(-77, -132),  S(-59, -54),   S( 17,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  16),   S( -1,  15),   S(  8,  11),   S(  0,  34),   S( -8,  51),   S( 31,  21),   S( 22,  -4),   S(  8, -13),
            S( -3,  18),   S( -3,  25),   S( -1,   7),   S( -1,   7),   S( 11,  14),   S( 15,   0),   S( 10, -14),   S(-19,  -6),
            S(  3,  36),   S( -9,  57),   S(  8,  19),   S(  8,   1),   S( 28, -12),   S( 17, -14),   S(  5, -23),   S(-13, -12),
            S(  2,  91),   S(-17, 104),   S(  7,  68),   S( 18,  34),   S( 22,   2),   S( 33, -25),   S( 23,   2),   S( 35, -21),
            S( -2, 157),   S(-15, 167),   S(-29, 166),   S( -9, 113),   S( 33,  51),   S( 87, -17),   S(115, -36),   S( 98, -40),
            S(104, 124),   S( 48, 237),   S( 27, 251),   S(  9, 208),   S(-22,  92),   S( 30, -174),  S(-56, -242),  S(-164, -173),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 69,   1),   S( 22,   6),   S( -5,  -7),   S(-12, -10),   S(  4, -14),   S(  3, -10),   S(  0, -10),   S(-57,  42),
            S( 38,  -3),   S(  5,  19),   S(  3,  -1),   S(-14,  -4),   S(-23, -22),   S(-16, -17),   S(-32, -21),   S(-43,   3),
            S( 64, -14),   S(107, -29),   S( 34, -17),   S(-30,  -2),   S(-70,   9),   S(-13,   4),   S(-72,  21),   S(-62,  31),
            S(-86, -73),   S(-16, -92),   S( 68, -58),   S(-29,   6),   S(-27,  18),   S(-64,  67),   S(-38,  56),   S(-53,  78),
            S(-29, -74),   S(-63, -111),  S( -8, -91),   S( 55,   5),   S( 72,  88),   S( -7, 101),   S( 14,  76),   S( -3, 101),
            S(  5, -57),   S(-15, -75),   S(  2, -63),   S(  3,  50),   S( 57,  88),   S( 65, 151),   S( 44, 156),   S( 58, 124),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  46),   S(-46,  44),   S( -8,  24),   S( 52,   7),   S( 63,  -3),   S(  9,   3),   S(-18,  10),   S(-53,  46),
            S(-75,  39),   S(-39,  40),   S(-23,  24),   S( -7,  23),   S(-19,  21),   S(-30,   9),   S(-52,   4),   S(-74,  32),
            S(-34,  29),   S(-34,  54),   S( 23,  28),   S(  6,  40),   S(-28,  42),   S(-65,  33),   S(-61,  32),   S(-62,  43),
            S(-26,  38),   S(-12,  10),   S(-32, -37),   S(  1, -27),   S( -6,  -6),   S(-53,  33),   S(-10,  29),   S(-30,  56),
            S( 57,   8),   S( -9, -32),   S( 30, -95),   S(  5, -72),   S( 49, -41),   S( 16,  22),   S(-19,  69),   S(-44, 117),
            S( 49,  33),   S( 18, -12),   S(-29, -66),   S(-17, -59),   S(-30, -57),   S( 46,  39),   S( 63, 135),   S( 40, 147),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87,  42),   S(-58,  20),   S(-20,   5),   S(  6,   4),   S(  7,  24),   S( 15,  12),   S( 13,   6),   S(  2,  26),
            S(-79,  24),   S(-59,  16),   S(-50,  10),   S( 15,  11),   S(-14,  26),   S(-12,  13),   S(-17,  11),   S( -9,  12),
            S(-61,  36),   S(-74,  43),   S(-51,  31),   S(-42,  44),   S(  0,  41),   S( 11,  18),   S(  4,  20),   S(-13,  18),
            S(-92,  90),   S(-62,  60),   S(-33,  35),   S(-18,  18),   S(-12, -34),   S(-20, -29),   S(-26,   5),   S( 31,  -2),
            S(-18, 104),   S(-51,  73),   S( 23,  10),   S(-12, -30),   S(  4, -72),   S(-41, -66),   S( -6, -33),   S( 79,  -5),
            S( 78,  77),   S( 68,  90),   S( 45,  24),   S( 39, -77),   S( -6, -102),  S(-41, -50),   S( -6, -45),   S( 75,   2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52,   4),   S(-41, -14),   S( -3, -22),   S(-65,  45),   S( 20,   5),   S( 66, -18),   S( 58, -27),   S( 68, -11),
            S(-60,   4),   S(-64,   5),   S(-35, -20),   S(-38,   4),   S(  0,  -2),   S( 42, -27),   S( 28, -15),   S( 50, -17),
            S(-58,  26),   S(-78,  40),   S(-40,   6),   S(-44,   2),   S( -1,  -2),   S( 22, -15),   S( 57, -16),   S( 54, -19),
            S(-56,  63),   S(-91,  82),   S(-59,  63),   S(-35,  37),   S(-16,  -1),   S( 37, -58),   S( 17, -73),   S( 21, -108),
            S( 13,  63),   S(-65, 136),   S(-11, 119),   S(-14,  88),   S(  9,  21),   S( 12, -78),   S(-46, -131),  S(-14, -99),
            S(130,  84),   S( 81, 123),   S( 88, 107),   S( 57,  96),   S( 32,   4),   S(  3, -100),  S(-26, -89),   S( -7, -180),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 28,   6),   S( 11,   9),   S( 50,   0),   S( -8, -35),   S(-29, -61),   S(-19, -25),   S( 15, -57),   S( 39, -41),
            S( 16, -61),   S( 13, -15),   S(-37, -56),   S(-54, -35),   S(-30, -58),   S( 31, -62),   S( 10, -67),   S( -4, -52),
            S( 36, -97),   S(  9, -55),   S( -3, -63),   S(-44, -50),   S(-29, -28),   S( 10, -41),   S(-40, -21),   S( -1, -29),
            S(  4, -25),   S(-24, -37),   S( 17, -16),   S(-12,   0),   S(-23,  10),   S(  1,  23),   S( -7,  24),   S(-12,  24),
            S( 24,   7),   S( -2, -31),   S(  8,  45),   S( 33,  93),   S( 53, 120),   S( 29, 121),   S( 13,  96),   S(-31, 106),
            S( 20,  35),   S(  5,  57),   S( 25,  71),   S( 32,  99),   S( 46,  97),   S( 51, 151),   S( 40, 101),   S(-21,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 32,   9),   S( 34,  21),   S( 23,  19),   S(  4,  29),   S( 20,   2),   S( 15, -16),   S( 33, -47),   S(-17, -19),
            S( 59, -58),   S( 17, -49),   S( 12, -58),   S(-13, -43),   S(-28, -26),   S(-48, -28),   S(-46, -35),   S( 18, -45),
            S( -9, -42),   S(-27, -42),   S(-19, -72),   S(-59, -41),   S( -1, -35),   S(-13, -46),   S(-56, -34),   S( 16, -32),
            S(-42,   1),   S(-44, -49),   S( -2, -67),   S(-37, -28),   S(  0, -41),   S( -3, -23),   S( 12,  -8),   S(  1,   9),
            S(  4,  13),   S( -8, -19),   S(-18,   4),   S( 20,  30),   S( 17,  61),   S( 19,  54),   S( -1,  67),   S(  0,  64),
            S( -8,  69),   S( 26,  62),   S( -2,  58),   S( 22,  57),   S( 26, 109),   S( 16,  86),   S( 17,  81),   S( 17,  82),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24, -51),   S( -2, -48),   S( -2, -17),   S( -2, -11),   S( 35,  16),   S( 70,  11),   S( 21,   4),   S( 14, -18),
            S( -7, -60),   S(-63, -41),   S(-12, -50),   S( 20, -37),   S( -1, -29),   S(-11, -22),   S( 18, -39),   S( 17, -43),
            S(-20, -45),   S(-85, -22),   S(-64, -38),   S(-11, -30),   S(-19, -45),   S(-12, -61),   S(-24, -62),   S( 64, -66),
            S(-36,   1),   S(-19,  -6),   S(-24, -31),   S(-54, -37),   S(  4, -67),   S(-47, -50),   S(-24, -51),   S( 20, -49),
            S( 11,  17),   S( 31,  16),   S( 15,  12),   S(-19,  -1),   S(  9,  21),   S( 12,  14),   S(-31,  10),   S( 41,  -1),
            S(  8,  26),   S(  2,  49),   S( 25,  56),   S(  7,  58),   S( 24,  84),   S(  1,  45),   S(-13,  23),   S( 26,  40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -45),   S( -1, -46),   S(-33, -42),   S(  5,  -3),   S(  1, -19),   S( 65,  11),   S( 53,  -9),   S( 58,  -9),
            S(-36, -61),   S(-49, -62),   S(-37, -71),   S( -4, -61),   S(-23, -31),   S( 12, -46),   S( 26, -46),   S( 41, -71),
            S(-24, -38),   S(-87,  -4),   S(-31, -22),   S(-11, -25),   S(-67, -41),   S( 39, -65),   S( 27, -120),  S( 87, -102),
            S(-50,  23),   S(-70,  32),   S(  3,  28),   S( 16,  -8),   S(-31, -11),   S(-22, -42),   S(-35, -53),   S( 40, -94),
            S(-16,  22),   S(-18,  67),   S(-11,  96),   S( 17,  60),   S( 27,  63),   S(-10,   8),   S( -1,   8),   S(  8, -23),
            S( 16,  70),   S( 27,  57),   S( 30,  83),   S( 26,  84),   S( 13,  66),   S( 34,  83),   S( 12,  31),   S( 28,  39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -106),  S( 28, -51),   S( -2, -28),   S(  0,  -1),   S( -6, -30),   S(-38, -71),   S( 16, -47),   S(  4, -45),
            S( 39, -86),   S( 28, -46),   S(-22, -74),   S(-32, -58),   S(-33, -86),   S(-12, -60),   S(-15, -89),   S(-21, -67),
            S( -7, -60),   S( -9, -77),   S(-23, -95),   S(-26, -83),   S(-11, -54),   S( -7, -46),   S(-38, -59),   S(-10, -76),
            S(-12, -38),   S( -4, -16),   S(-19, -17),   S( -2,   2),   S( 17,  57),   S(  4,  41),   S(  4,  10),   S( -6,  -4),
            S( 11,  22),   S(  1,  18),   S(  3,  24),   S( 20,  62),   S( 30,  77),   S( 24,  87),   S( 13,  80),   S( 19,  51),
            S( 12,  30),   S(  1,  38),   S( 13,  53),   S( 12,  60),   S( 25, 100),   S( 24,  93),   S(-21, -24),   S(-13,   5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -63),   S( 28, -82),   S( 19,   3),   S( -2, -12),   S(  5, -20),   S(-30, -40),   S(-10, -74),   S(-15, -68),
            S( 29, -132),  S( 20, -100),  S(  0, -86),   S( 12,  -8),   S(-25, -53),   S(  1, -79),   S( -1, -92),   S(  1, -88),
            S( 33, -88),   S(-10, -76),   S( -3, -90),   S(  6, -60),   S(-44, -27),   S( 21, -74),   S( -8, -74),   S( 58, -88),
            S( 16, -26),   S( -1, -34),   S(  2, -29),   S( -4,  25),   S( 14,   6),   S(-17,  10),   S(-13, -17),   S(  8, -21),
            S( -3,  41),   S(  7,  24),   S( -2,   6),   S( 22,  56),   S( 39,  78),   S( 26,  89),   S( 12,  97),   S( -7,  58),
            S( 12, 103),   S( 29,  52),   S(  3,  35),   S( 12,  45),   S( 20,  63),   S( 10,  51),   S( -4,  40),   S(  2,  67),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -116),  S(  4, -70),   S( -4, -41),   S(  3,   3),   S( -6, -14),   S( -1,  -1),   S( 20, -70),   S( -8, -42),
            S( 17, -113),  S(-38, -106),  S( -5, -80),   S(-28, -86),   S(-10, -56),   S( 17, -50),   S(  1, -65),   S( 25, -85),
            S( 17, -93),   S(-21, -78),   S(-14, -61),   S(  3, -74),   S(-23, -51),   S(  4, -90),   S( -1, -101),  S( 36, -58),
            S(  5, -30),   S(-22, -38),   S( -5,  -4),   S(-21,  -8),   S( 12, -51),   S( -5, -28),   S( 12, -30),   S( 12,  -4),
            S(-14, -13),   S(  6,  38),   S( 11,  51),   S( -8,  16),   S( 19,  71),   S(  2,  19),   S( 17,  46),   S( 23,  67),
            S( -5,  35),   S(  7,  49),   S( 27,  74),   S( 22,  70),   S( 16,  59),   S(  1,  35),   S( 23,  83),   S( 23,  91),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -27),   S(  2, -58),   S(-26, -56),   S(-10, -26),   S(-12, -28),   S(-15, -37),   S( -9, -56),   S(  3, -88),
            S(-23, -65),   S(-22, -100),  S(-17, -104),  S(-10, -35),   S(-20, -23),   S( -7, -27),   S( 12, -53),   S( 12, -106),
            S(-28, -45),   S(-33, -63),   S(-44, -51),   S(  6, -37),   S(-32, -37),   S( -8, -71),   S(  3, -47),   S(  6, -44),
            S( 10, -35),   S(-26, -17),   S( -2,  43),   S(-19,  14),   S( 10,   9),   S( -9, -17),   S( -7, -11),   S( -8,  34),
            S(  6,  46),   S(  1,  52),   S(  1,  71),   S( 12,  61),   S( 24,  82),   S( 12,  65),   S( 18,  60),   S( 10,  23),
            S(-22,   7),   S( -7,   4),   S( 11,  75),   S( 21,  56),   S( 21,  71),   S( 18,  58),   S( 11,  34),   S( 16,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-72, -28),   S(-32, -27),   S(-22,  -6),   S(-19,  20),   S(-28, -26),   S(-31,  -7),   S( -9, -24),   S(-75, -39),
            S( 14, -36),   S( -3,   2),   S(-26, -33),   S( -7, -12),   S(-14,  -8),   S(-13, -21),   S(-35, -48),   S(-21, -39),
            S(-18, -22),   S( 17, -31),   S( -1,   4),   S( 26,  22),   S(-14,   8),   S(  3,  -5),   S(-33,  21),   S(-26, -33),
            S( 11,  20),   S( 37,  46),   S( 25,  29),   S( 43,  15),   S( 27,  16),   S( 12,  23),   S( 35, -18),   S( -9, -16),
            S( 61,  42),   S( 23,  53),   S( 54,  60),   S( 63,  37),   S( 66,  29),   S( 11,  24),   S( 15,  -6),   S(  4,   2),
            S(103, -31),   S( -7,  53),   S(144,  -1),   S( 71,  38),   S( 52,  39),   S(-38,  58),   S( 34, -10),   S(-19,   5),
            S( 57,  -1),   S( -2, -21),   S( 51,  19),   S( 85,  65),   S( 41,  24),   S(  1,  31),   S(-13,   8),   S(-46,   6),
            S(-108, -118), S( -1,   1),   S(  7,   2),   S( 19,  25),   S(  5,  33),   S( 20,  15),   S(-30,   0),   S( -3,  16),

            /* knights: bucket 1 */
            S( 21,   3),   S(-61,  17),   S(-30,   9),   S(-48,  28),   S(-28,  33),   S(-26, -24),   S(-31,  -4),   S(  5, -15),
            S(-35,  38),   S(-50,  56),   S(-29,  24),   S(-14,  18),   S(-24,  18),   S( -9,  22),   S(-11,  -4),   S(-12, -49),
            S(-37,  29),   S( -3,   0),   S(-22,  15),   S(-13,  48),   S(-18,  34),   S(-10,   6),   S(-43,  30),   S(-13,  25),
            S(-18,  72),   S( 30,  30),   S( -7,  50),   S( -5,  58),   S( -7,  53),   S(-10,  53),   S( -1,  19),   S(-25,  53),
            S( 67,  -1),   S(  8,  22),   S( 43,  56),   S( 21,  48),   S( 40,  47),   S( -6,  66),   S(-10,  49),   S( -3,  62),
            S( 31,  24),   S( 63, -12),   S( 82,  17),   S( 95,  28),   S( 71,  22),   S(-33,  72),   S( 19,  27),   S(  3,  41),
            S( 24,   1),   S( 38,  -8),   S( 34, -16),   S( 26,  48),   S( 16,  35),   S(  4,  19),   S( 17,  72),   S(-28,  49),
            S(-140, -22),  S( 16, -17),   S(-33, -59),   S(-17,  11),   S( -3,  10),   S( 41,  48),   S( 19,  50),   S(-56,  34),

            /* knights: bucket 2 */
            S(-61,  17),   S(-38,  26),   S(-29,   1),   S(-23,  16),   S(-22,  12),   S(-55,  -2),   S(-29,   3),   S(-14, -17),
            S(-15,  10),   S( -4,  35),   S(-22,   6),   S(-19,  15),   S(-29,  22),   S(-19,   5),   S(  8,   7),   S(-29,   7),
            S(-32,  51),   S(-21,  20),   S(-25,  17),   S(-25,  54),   S(-24,  42),   S(-23,   8),   S(-26,  13),   S( -2,  -7),
            S(-11,  57),   S( -5,  42),   S(-26,  73),   S(-18,  74),   S(-37,  70),   S(  2,  47),   S(  7,  30),   S( -4,  36),
            S( -8,  65),   S(-17,  66),   S(  6,  64),   S( 17,  58),   S(  2,  65),   S( 20,  65),   S( -5,  60),   S( 23,  16),
            S(-39,  70),   S(-20,  49),   S(-15,  79),   S( 44,  26),   S( 48,  27),   S(130,  -7),   S( 69,  10),   S( 31,  -6),
            S( 36,  41),   S(-38,  58),   S( 49,  18),   S( 33,   5),   S( -1,  40),   S( 18, -10),   S( 39,  22),   S( 32,  -4),
            S(-48,  42),   S( 33,  67),   S(-12,  67),   S( -8, -24),   S(-19, -11),   S(-27, -42),   S( 18,   1),   S(-110, -41),

            /* knights: bucket 3 */
            S(-51,  29),   S(-14, -49),   S( -1, -23),   S( -2, -17),   S(  1, -17),   S( -9, -32),   S(-20, -29),   S(-22, -69),
            S(-13, -24),   S( -1,   0),   S(  8, -13),   S( -3,  -2),   S( -6,  -3),   S( 20, -21),   S( 22, -35),   S( 23, -52),
            S(-11,  -1),   S(-11,   6),   S(  0,  17),   S(  3,  38),   S( 10,  26),   S( -1,  13),   S(  9,   1),   S( 20, -34),
            S( 10,   3),   S( 16,  27),   S( 17,  43),   S( 10,  53),   S( 13,  66),   S( 28,  52),   S( 32,  46),   S( 16,  37),
            S( -2,  45),   S( 22,  32),   S( 25,  51),   S( 28,  75),   S( 31,  70),   S( 35,  78),   S(  8,  90),   S( 63,  79),
            S(-10,  33),   S(  7,  41),   S( 11,  52),   S( 20,  67),   S( 60,  67),   S(143,  62),   S( 61,  82),   S( 24, 104),
            S(-21,  46),   S(-17,  53),   S(-18,  62),   S( 30,  57),   S( 45,  60),   S(106,  39),   S( 12,  -2),   S( 92,  21),
            S(-137,  51),  S(-30,  82),   S(-46,  86),   S( 37,  48),   S( 65,  73),   S(-46,  69),   S(-24, -40),   S(-53, -100),

            /* knights: bucket 4 */
            S( 10,  12),   S(-11, -12),   S(-61,  16),   S(-37, -15),   S(-39,  19),   S(-22, -16),   S( 19, -29),   S(-18, -13),
            S( 21,  39),   S(  3, -22),   S( -5,   3),   S(-20,   4),   S( -9, -16),   S(  9, -47),   S(-12,  10),   S(-43,  -2),
            S( -3, -19),   S( 31,  -8),   S( 53,  -1),   S( 65,   1),   S( 13,  16),   S( 33, -32),   S(-12, -26),   S( -7, -31),
            S(-17, -24),   S( 28,   0),   S( 50, -17),   S( 71,  -3),   S( 35,   7),   S( -3,  25),   S(-34,  27),   S( -5,  12),
            S(  8, -37),   S( 35,  -9),   S( 67,  10),   S( 36,  36),   S( 53,   1),   S( 15,  18),   S( 24,  -7),   S(-29,  45),
            S( -5, -24),   S(  2,  -1),   S( 40, -24),   S( 58,  16),   S(  4,  16),   S(-22,  35),   S(-19,   5),   S( 21,   6),
            S(-16, -28),   S(-20,  -7),   S(  4,  -5),   S( 23,  19),   S( 25,   9),   S( -3,  10),   S( 11,  39),   S(-33,  -9),
            S(  4,  16),   S(-11, -35),   S( -6, -30),   S( 17,   4),   S( 14,  21),   S( -4,  18),   S( -4,  19),   S(-15, -12),

            /* knights: bucket 5 */
            S( 21,  33),   S(  9,  29),   S(-38,  33),   S(-19,  22),   S(-20,  30),   S(  5,  16),   S(-16,  18),   S( 11,  32),
            S( 20,  31),   S( 28,  29),   S(  1,   4),   S(-24,  12),   S( 27, -11),   S(-32,  15),   S(-12,  41),   S(-50,  21),
            S(-29,  27),   S( -8,   2),   S( 33,   4),   S( 37,   9),   S( 22,  14),   S(-20,  21),   S( -7,  13),   S(-48,  20),
            S( 34,  17),   S( 33, -19),   S( 58,  -5),   S( 89, -20),   S( 85,   1),   S( 76,   5),   S( -5,  21),   S( 13,  36),
            S( 43,   2),   S( 40, -10),   S( 95, -15),   S(134, -11),   S( 93, -18),   S( 45,  14),   S(  3,   7),   S( 16,  26),
            S(  0, -23),   S( 39, -30),   S(  5, -28),   S( 14,  15),   S( 26,  -3),   S( 48,  -4),   S( -6,  10),   S( 26,  35),
            S(  2,   7),   S(-25, -55),   S(  3, -48),   S(-10, -18),   S( -6, -36),   S(  5,   4),   S( -4,  43),   S( 19,  39),
            S(-19, -26),   S(-25, -62),   S( 10,  -8),   S(-23, -27),   S(  7,   1),   S(  1,  33),   S( 22,  42),   S(  1,  28),

            /* knights: bucket 6 */
            S( -4,  -2),   S(-54,  26),   S(-30,   6),   S(-49,  36),   S(-53,  34),   S(-24,  36),   S(-16,  42),   S(-31,  16),
            S(  7,  -9),   S(-18,  49),   S(-22,   2),   S( 13,   8),   S( 12,  16),   S(-44,  36),   S(-22,  50),   S(-38,  72),
            S(-10,  20),   S( 11,  16),   S(  3,  21),   S( 27,  29),   S( 33,  25),   S(-28,  32),   S( 19,  27),   S(-21,  43),
            S(  8,  51),   S( 54,   5),   S( 41,  22),   S( 70,   9),   S( 89, -11),   S( 77,   6),   S( 27,  15),   S(-11,  49),
            S( -8,  43),   S( 37,  11),   S( 90,   3),   S(120,  -3),   S(112, -19),   S( 72,  20),   S(128, -20),   S( 20,  28),
            S( 10,  21),   S( 24,   6),   S( 58,  11),   S( 44,   1),   S( 54,  -6),   S( 40,  -4),   S( 15, -13),   S( 29,   1),
            S(  4,  34),   S( 13,  36),   S( 36,  34),   S( -1,  -7),   S( 28, -14),   S( 22, -38),   S( -6,  -3),   S( 11,  42),
            S( 20,  44),   S(  2,  33),   S( 17,  36),   S(  5,  20),   S( 10,  -6),   S( -7,  -2),   S( 11,  27),   S(-19, -27),

            /* knights: bucket 7 */
            S(-32, -36),   S(-24, -44),   S( -6, -15),   S(-49,  19),   S(-12,  -3),   S(-44,   8),   S(-17, -10),   S(-13,  29),
            S(-32, -51),   S(-12, -26),   S(-44,  -6),   S(-40,  -2),   S( -7,   8),   S( -3,  21),   S( -9,  15),   S(-63,  41),
            S( -6, -35),   S(-45, -19),   S(  3, -17),   S(  0,  18),   S( 45,  12),   S( 39,   4),   S( 26,  12),   S( -3,  31),
            S(-40,  16),   S(  6,  -4),   S( 53, -19),   S( 84,  -2),   S(107, -12),   S( 87,  11),   S( 73,   1),   S( 67,   6),
            S(  2,   7),   S( -8,  12),   S( 16,  15),   S( 79,  -5),   S(106,  -2),   S(155, -28),   S(194, -17),   S( 40,  -4),
            S(-19,  15),   S( 22,   8),   S( -6,   6),   S( 52,  11),   S( 99,  -7),   S( 97, -11),   S( 18,  -9),   S(  6, -40),
            S(-20,   6),   S(-10,   5),   S( -6,  14),   S( 25,  20),   S( 55,  13),   S( 30,  21),   S(-12, -33),   S(-15, -36),
            S(-27, -33),   S( -9,  11),   S( -2,  25),   S(  4,  20),   S( 14,  11),   S( 19,  13),   S(  5,  -5),   S(  2,  -7),

            /* knights: bucket 8 */
            S( -1,   6),   S( 12,  29),   S( 12,  27),   S( -9, -30),   S( -2,  23),   S( -5, -18),   S( 13,  24),   S( -2, -12),
            S( -6, -23),   S( -5, -23),   S( -9, -40),   S(-11,   2),   S( -7,  33),   S( -1,  -5),   S(  0,  -5),   S( -3,  -3),
            S(-11, -40),   S( -8, -27),   S(  1, -50),   S(  3,   6),   S( -9, -23),   S( 12,   6),   S( -3,  -3),   S( -1, -13),
            S(-17, -53),   S( -9, -32),   S(  9,  14),   S(  1,   7),   S(-18, -17),   S(-24, -13),   S(-20, -27),   S(-16, -35),
            S( -7, -24),   S(  5, -18),   S( -1, -21),   S( -1,  -7),   S(-16,   1),   S(-11, -13),   S(  4,   1),   S( -1, -10),
            S( -3,   8),   S( 12,  -3),   S( -2,   4),   S( -6, -13),   S( -8,  -4),   S( -6, -11),   S( -9,  -4),   S( -6, -18),
            S(  0,  16),   S( -1, -25),   S(-12, -20),   S(  5,  14),   S(  2,   3),   S( -1,   0),   S( -4,   5),   S( -2, -17),
            S(  1,   2),   S( -3,   7),   S( -5,   3),   S(  3,   0),   S( -1,   8),   S( -1,  -4),   S(  0,   5),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-18, -58),   S( -5,  -1),   S( -2, -32),   S( -4, -34),   S(-16,  -9),   S(-12,  11),   S(  5,  22),   S(  2,  -6),
            S( -4,   6),   S(-14, -42),   S(-21, -108),  S(-26, -67),   S(-10, -35),   S(-22, -56),   S(-11,  -1),   S(-12,   2),
            S( -9, -20),   S(-16, -42),   S(-12, -41),   S( -4, -60),   S(-21, -13),   S( 10,   5),   S(-13,  -6),   S( -3,   6),
            S(-18, -47),   S(-11, -44),   S( -8, -27),   S( -9, -39),   S(-14, -31),   S(  4,  -2),   S(-17, -39),   S(  3,  13),
            S(  3,  24),   S( -8, -25),   S( -2, -23),   S( -1, -32),   S(-10, -26),   S( -4,  10),   S( -9, -11),   S( -4,   1),
            S(-13, -21),   S(-18, -33),   S(-11, -24),   S( -4, -18),   S(  0,  15),   S( -8,   0),   S( -3,  19),   S(  0,  11),
            S(-10, -16),   S( -1,  19),   S(-12,  -6),   S(-23, -19),   S(  2,   5),   S(  1,  23),   S( -7,  18),   S( -6,   4),
            S(  4,   2),   S(  4,   3),   S( -1,  12),   S(  0,   8),   S(-10,  -5),   S( -4,   4),   S(  3,  11),   S(  0,  16),

            /* knights: bucket 10 */
            S( -8, -28),   S( -5,  12),   S(-10,  -9),   S(-11,  17),   S(-20, -44),   S(  9, -16),   S( -3,  12),   S( -1,  20),
            S( -3, -15),   S(  7,   3),   S(-14, -23),   S(-10, -46),   S( -8, -32),   S(-25, -57),   S( -7,  16),   S(  3,  31),
            S( -3,  -2),   S( -4,  -9),   S( -7, -20),   S(  6, -50),   S(-26, -44),   S( -3, -23),   S( -9, -34),   S( -9,  12),
            S( -9, -14),   S(-11, -23),   S( -6, -16),   S( -2, -24),   S(-10, -17),   S( -4,  -5),   S( -8, -48),   S( -3,  -2),
            S(-12, -18),   S(-12, -29),   S( -9,  -5),   S( -5, -14),   S(  5,  -6),   S( -7, -38),   S( -3,  -8),   S(  5,   9),
            S( -1,  15),   S(-12,   2),   S(-10,   7),   S(-13,  17),   S(-15, -19),   S(-19, -17),   S(-13,  -3),   S(-17,  -4),
            S(  3,  11),   S( -3,  -2),   S( -6, -26),   S( 12, -20),   S( -6,   3),   S(-17, -46),   S( -8,   8),   S( -9, -10),
            S( -1,   4),   S( -1,  11),   S( -1,  18),   S( -4,   6),   S( -4,   6),   S( -6,  -9),   S(  6,  13),   S(  2,  10),

            /* knights: bucket 11 */
            S( -3, -15),   S(-26, -26),   S( -4,  -5),   S(  4,  21),   S(-37, -31),   S(  1,  15),   S( -5,   9),   S(  9,  36),
            S( -6, -14),   S(-28, -42),   S(-13, -42),   S( 15,  -3),   S(  8,  18),   S( -2, -27),   S(-13, -22),   S( -8, -11),
            S(-13, -39),   S(-19, -19),   S( -2, -15),   S(  2,  -7),   S( -7,  14),   S( 17,  -6),   S(  0, -15),   S( -4,  -5),
            S(-14, -10),   S(  6, -20),   S( -2, -25),   S( 12,  -1),   S( 29,  -7),   S(  1, -19),   S( 14,  20),   S(  0,  -5),
            S(-16,   3),   S(  3, -37),   S(-19,  -2),   S(  2, -16),   S( 32,   9),   S(  5,  16),   S( -8, -66),   S( -9, -10),
            S( -8, -22),   S( -7, -43),   S(  3,   7),   S(  9,  -2),   S(  8,  31),   S( -7, -13),   S( -4, -28),   S( -1,  20),
            S(  0,  -6),   S( -8,  19),   S(-11, -10),   S(  6,  -3),   S( 13,  -3),   S(  3, -17),   S(  1, -16),   S( -4,   2),
            S( -3, -15),   S(  2,   7),   S( -3,  -8),   S(  2,  18),   S( -4,  -8),   S(  0,  -6),   S(  5,  17),   S( -1,  -3),

            /* knights: bucket 12 */
            S(-14, -40),   S( -3, -10),   S( -1, -19),   S(  0,   8),   S( -5,   6),   S( -5, -11),   S( -1,   6),   S( -1,   1),
            S( -3,  -9),   S(  0,   2),   S(  0, -17),   S( -3,   4),   S( -4, -10),   S(  0,   4),   S(  1,  -1),   S(  0,  -7),
            S( -3, -10),   S( -6, -23),   S( -6, -21),   S(-15, -25),   S( -8,  -5),   S( -3,  25),   S( -3,   0),   S( -5,  -9),
            S(  2,   9),   S( -1, -35),   S( -7,  25),   S(  3,  13),   S( -4, -12),   S(  3,  22),   S(  5,  12),   S(  2,   8),
            S(  0,   2),   S( -4,  -8),   S( -4, -20),   S( -4,  -9),   S(  0,   5),   S( -3,   5),   S( -6,  -3),   S( -8,  -8),
            S( -5,  -2),   S( -1,  -3),   S( -3, -14),   S( -3, -10),   S( -3,   0),   S( -7, -20),   S(  7,   7),   S( -1,   8),
            S( -4,  -8),   S( -2,   0),   S(-10,  -1),   S( -2,  -6),   S(  0,   9),   S( -9,  -8),   S( -5, -18),   S( -3,  -2),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -6),   S(  1,   3),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -7),   S( -4, -13),   S( -3, -16),   S( -2,  -6),   S( -3, -12),   S( -2,   7),   S( -6,  -5),   S(  3,  11),
            S( -2,   9),   S( -2,  -2),   S(  3,   9),   S( -4,  -3),   S( -6, -11),   S( -1,   8),   S(  1,  19),   S( -3,  -5),
            S(  5,  -2),   S(  5,   8),   S(  5,   1),   S( -4, -25),   S(  4,  22),   S( -5,   8),   S(  7,   4),   S( -2,  -1),
            S(  0,  13),   S(  0,   4),   S( -6,  -4),   S(  1,  26),   S(  0,  10),   S( -2,  28),   S(  1,   7),   S( 10,  19),
            S(  1,  21),   S( -2, -17),   S( -4,  10),   S( -7,   7),   S(-16,   0),   S( -3,  24),   S( -8, -23),   S( -3,  -3),
            S( -3,  -4),   S(  2,   1),   S( -4,  10),   S(  3,  12),   S( -8,   6),   S( -8,   3),   S(  3,  21),   S(  1,   3),
            S(  1,   4),   S(  3,   8),   S( -6,  -4),   S( -4,   2),   S( -2,   7),   S( -3,  -7),   S(  2,   6),   S( -1,   1),
            S(  2,   6),   S(  1,   2),   S( -2,  -3),   S(  2,   4),   S(  0,   1),   S(  1,   2),   S( -1,  -2),   S(  0,   2),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   4),   S(  5,  17),   S( -2,   0),   S( -6, -24),   S( -1,  18),   S(  2,   3),   S(  0,   3),
            S( -2,  -9),   S( -8, -15),   S(  2,  -4),   S( -1,  -2),   S(  3,   1),   S(  0,   4),   S( -7,   5),   S(  6,  58),
            S( -1,   0),   S( -5, -33),   S(  6,  16),   S(-10, -32),   S( -3,  -2),   S(  1,   7),   S( -1,   8),   S(  3,  17),
            S( -1,  -3),   S( -3, -17),   S(-22, -15),   S( -2,  41),   S(  3,  41),   S( -4,  -9),   S(  0,   5),   S(  1,  35),
            S(  6,  16),   S(-17, -34),   S( -9,  -9),   S( -8,   3),   S(  0,  31),   S(-11,   3),   S( -3,   0),   S(  4,  13),
            S( -1,   3),   S(  5,   5),   S(  3,  -4),   S( -3,  13),   S(  1,  17),   S(  1,  13),   S(  1,   8),   S( -5, -11),
            S(  0,   4),   S( -3,  -1),   S(  3,  17),   S(  6,   5),   S(  4,  13),   S( -4, -10),   S(  2,   7),   S(  4,   5),
            S(  0,  -1),   S(  0,   1),   S( -1,  -2),   S(  2,   4),   S( -1,   1),   S(  0,  -2),   S(  1,   0),   S(  1,   2),

            /* knights: bucket 15 */
            S( -2, -13),   S( -1,   4),   S(  4,  24),   S( -2,   4),   S( -4, -16),   S(-10, -35),   S( -4, -15),   S( -1, -10),
            S(  2,  -1),   S(  4,   5),   S( -6,  -6),   S(  9,  43),   S(  0,  12),   S( -8, -35),   S( -3,  -3),   S(  1,   2),
            S(  0,  -5),   S( -5, -19),   S(  1, -10),   S(  5,   8),   S(-17, -29),   S( -1,  -4),   S( -2,  -6),   S( -2,  -1),
            S(  0,  -7),   S( -3,   4),   S( -5, -14),   S( -5,   4),   S( -7,   4),   S( -9,  25),   S(  5,   5),   S( -1,   1),
            S( -1,  -1),   S(  9,  21),   S( -5,   6),   S( -7,   5),   S( 18,  33),   S(  0,  15),   S(  6,  -4),   S(  4,  18),
            S(  1,   4),   S( -4,  -9),   S( -2,   0),   S( -9, -18),   S( -6,  -9),   S(  1,  16),   S(  0,   9),   S(  5,  13),
            S( -1,   1),   S( -2,  -7),   S(  4,  16),   S(  3,   4),   S(  3,  13),   S(  6,   9),   S(  1,   8),   S(  4,   9),
            S(  1,   4),   S( -1,  -5),   S(  0,   0),   S( -1,   1),   S(  2,  10),   S(  1,   1),   S(  0,   0),   S(  1,   3),

            /* bishops: bucket 0 */
            S( 25,  -2),   S( -8,  39),   S(-16,  12),   S(-24,  -6),   S( -6,   1),   S( -4,  13),   S( 66, -39),   S( 22, -10),
            S(-27,  -7),   S(-12, -21),   S(-24,  34),   S(  0,  11),   S( -1,  19),   S( 47,  -6),   S( 29,  24),   S( 44, -11),
            S( 15,  14),   S(  1,  24),   S(  2,  -8),   S(  6,  10),   S( 20,  21),   S( 28,  19),   S( 33,   5),   S( 25,   4),
            S( 17, -25),   S( 36, -39),   S( 14,  15),   S( 35,  15),   S( 65,  35),   S( 30,  47),   S( 13,  20),   S(  9,  28),
            S( 40, -14),   S( 44, -16),   S( 55,   8),   S( 80,  43),   S( 89,  24),   S( 17,  45),   S( 29,  48),   S( -4,  19),
            S( 53,  19),   S( 55,  44),   S(100,   3),   S( 56,  -2),   S( 16,  43),   S(  9,  35),   S( 37,  32),   S( -3,  14),
            S(-40, -75),   S( 73,  33),   S( 85,  80),   S( 20,  -1),   S( 12,  -7),   S( 23,  31),   S(-30,  21),   S(-14,  55),
            S(-20, -37),   S( -5,  -8),   S( 13, -25),   S(-14, -12),   S(-12, -14),   S(-18,   9),   S(-17,  22),   S(-31, -34),

            /* bishops: bucket 1 */
            S(-61,  17),   S( -5,   1),   S(-24,  39),   S( 15,  -5),   S(-20,  24),   S(  3,   6),   S( 34, -10),   S( 26, -29),
            S(  3, -29),   S(-25, -12),   S( -7,  -4),   S(-17,  17),   S( 25,  -7),   S(  2,   4),   S( 42, -36),   S( 16,  -7),
            S(-10,   5),   S( 23, -10),   S(-26,  -9),   S( 15,   6),   S(  0,   0),   S( 23, -30),   S( 10,  -2),   S( 63,   1),
            S( 23, -12),   S( 51, -16),   S( 25,   3),   S( 22,  11),   S( 37,   1),   S(  9,  14),   S( 50,  -3),   S(  1,  18),
            S( 28,  -9),   S( 55, -13),   S( 17,   6),   S( 94, -16),   S( 50,  21),   S( 39,  21),   S( -3,  25),   S( 30,   6),
            S( 61, -40),   S( 48,   9),   S( 59, -27),   S( 70, -13),   S( 75,   2),   S(-41,  11),   S(-28,  55),   S(-29,  22),
            S( 18, -61),   S( -1, -51),   S( -8,   1),   S( 25,  45),   S( 26,  34),   S(-15,  30),   S(-24,   4),   S(-26,  39),
            S( -6, -21),   S(-14,   8),   S( -7, -21),   S(-48,  -1),   S(-22,  20),   S( 19,   3),   S( 28,   4),   S(-56,  -1),

            /* bishops: bucket 2 */
            S(  0, -18),   S(-12,  -3),   S(  0,  17),   S(-22,   8),   S( 10,  14),   S(-21,   8),   S( 16,  -6),   S( -2, -21),
            S( 25, -17),   S(  1, -33),   S( -6,  -6),   S(  6,  14),   S(-13,  13),   S(  5,   6),   S( -4, -32),   S( 18, -48),
            S( 45,   6),   S( 19,  -1),   S( -9,  -5),   S( -9,   9),   S( -5,  30),   S(-19, -32),   S(  3, -23),   S(-10,   0),
            S(-14,   8),   S( 44,  16),   S( -5,  19),   S( 24,  30),   S( -1,  16),   S( -4,  21),   S(-13,   0),   S(  7,  15),
            S(  0,  22),   S(-31,  42),   S( 48,  22),   S( 20,  29),   S( 21,  28),   S( 24,   7),   S( 11,  32),   S( 42,  -6),
            S(-32,  40),   S( -7,  40),   S(-35, -12),   S( 88,  -1),   S( 51,  13),   S( 99, -19),   S( 76,  12),   S( 42, -43),
            S(-36,  67),   S(-42,  -1),   S( -8,  22),   S(  6,  14),   S(-43,  -8),   S(-35,  13),   S(-38,  -3),   S(  1, -37),
            S(-82, -21),   S(-13,  27),   S(  2,  11),   S(-18,  30),   S(-29, -10),   S(-31,  11),   S(  1, -11),   S(-55, -18),

            /* bishops: bucket 3 */
            S( 36, -15),   S( 39, -16),   S( 20, -23),   S( 11,  -1),   S( 16,  11),   S( -3,  30),   S(-11,  51),   S( -1, -18),
            S( 41,   7),   S( 22, -30),   S( 20,   0),   S( 22,   4),   S( 19,  19),   S( 22,   9),   S(  7, -20),   S( 35, -40),
            S( 17,  -3),   S( 33,  34),   S( 17,   5),   S( 17,  27),   S( 16,  30),   S(  7,  -5),   S( 20,  -9),   S( 15,  14),
            S( -5,  15),   S( 11,  43),   S( 24,  51),   S( 35,  46),   S( 36,  20),   S( 28,   6),   S( 28,  -2),   S( 38, -33),
            S(  8,  35),   S( 16,  52),   S(  4,  57),   S( 53,  47),   S( 48,  44),   S( 50,  20),   S( 29,  16),   S(  4,  14),
            S(  6,  36),   S( 22,  56),   S(  2,  13),   S( 16,  41),   S( 52,  39),   S( 78,  38),   S( 48,  40),   S( 46,  73),
            S(-24,  78),   S( -6,  24),   S( 11,  29),   S( -4,  56),   S( 24,  34),   S( 55,  48),   S(-29,  23),   S( 26, -21),
            S(-44,  11),   S(-32,  56),   S(-50,  44),   S(-32,  52),   S( 17,  12),   S(-58,  32),   S( 20,   8),   S( 15,  12),

            /* bishops: bucket 4 */
            S(-34,   5),   S(-30,   9),   S(-39,  17),   S(-56,  15),   S(-30, -10),   S(-20,  -5),   S(-12, -19),   S(-37, -36),
            S( -6,   6),   S( -7, -18),   S( 59, -31),   S(-34,  16),   S(-54,  25),   S(-10, -29),   S(-28, -31),   S(-28, -19),
            S(  9,  23),   S( -9, -17),   S(  0,  -5),   S( -5,   5),   S( 14,  -8),   S(-67,   1),   S(-20, -29),   S(-52, -12),
            S( 31,   0),   S( 53, -15),   S( 33,  12),   S( 12,  29),   S( -9,  24),   S( 23,   2),   S(-44,   8),   S( -7, -18),
            S( 16, -12),   S( -8, -18),   S( 42, -10),   S( 19,   5),   S( -2,  29),   S( 21,  11),   S(-23,  39),   S(-55,   6),
            S(-51, -80),   S(-46,  -2),   S( -9,   3),   S(  8,   6),   S(-45,  45),   S(  6,   5),   S(-14,  30),   S( -4,  32),
            S(  0,   4),   S(-27,  -3),   S(  2, -17),   S(-27, -11),   S(  2, -20),   S( 35,   4),   S( -7, -12),   S( 18,  35),
            S( -6,  -5),   S(  1, -17),   S(-13,  -7),   S(  2, -15),   S(-17,   6),   S(  4,  20),   S(  4,  47),   S(  6,   3),

            /* bishops: bucket 5 */
            S(-46,  -6),   S( 19,  -3),   S(-48,  24),   S(-56,  25),   S(-18,   7),   S(-60,  21),   S(-34,  24),   S(-48, -15),
            S( -8,  -2),   S(-30,  -6),   S( 19,   0),   S(-18,  19),   S(-59,  34),   S(-27,  24),   S(-34,  -6),   S(  8,  -6),
            S( 14,  32),   S(-18,   3),   S( 16, -23),   S(  0,  11),   S(-11,  23),   S(-66,   2),   S(-18,  20),   S(-17,  31),
            S( 20,  10),   S(  7,  15),   S( 70, -15),   S( 39,  16),   S( -8,  28),   S(  8,  23),   S(-57,  36),   S(-21,  24),
            S( 15,  -2),   S( 35,  -2),   S( -5,  12),   S( -4,   2),   S(  1,  11),   S( -5,  16),   S(  9,  24),   S(-48,  24),
            S(  5,  -9),   S(-35,  20),   S( 19, -22),   S( -9, -14),   S(-10,  10),   S(-17, -11),   S(-24,  23),   S(-35,  53),
            S(-20,  -8),   S( -7, -18),   S(-15,  -1),   S(  5,  24),   S( 17,   6),   S( -9,  28),   S( -4,   6),   S(-20,  38),
            S(-14,  -6),   S( -9, -16),   S(  1, -15),   S(-18,  -1),   S(-22,  30),   S(  9,  38),   S(-17,  28),   S( 11,   5),

            /* bishops: bucket 6 */
            S(-11, -29),   S(-11,  10),   S(-38,  25),   S(-22,  15),   S(-62,  37),   S(-35,  22),   S(-39,  33),   S(-55,  -3),
            S(-35,  20),   S(-32, -26),   S(-58,  43),   S(-46,  34),   S(-47,  31),   S(-48,  23),   S(-40,   6),   S(-30,  16),
            S(  2,   9),   S(-37,  19),   S(-10, -13),   S(-34,  34),   S(-24,  36),   S(-28, -14),   S( -9,  -7),   S( -5,  26),
            S(-64,  33),   S(-51,  33),   S(-11,  20),   S( 20,  40),   S( 13,  36),   S( 20,  15),   S( 24,   5),   S(-10,  24),
            S(-44,  28),   S(-28,  33),   S(  9,  13),   S( 61,  15),   S(-11,  20),   S( -7,  10),   S( 16,  15),   S(-18,   1),
            S(-48,  46),   S(-18,  24),   S(-48,  -2),   S(-14,  16),   S( 18,  13),   S( -4,  -6),   S( -1,  21),   S(-25,   2),
            S(-14,  40),   S(-80,  31),   S(-26,  19),   S(-19,  26),   S( -5,   7),   S(  7,   6),   S( 11, -13),   S(-22,  16),
            S(-16,   3),   S(-24,  39),   S( -8,  35),   S( 29,  13),   S(-24,  24),   S( 20, -13),   S(-10,  13),   S(-12,  11),

            /* bishops: bucket 7 */
            S(-14, -51),   S(-51,  -7),   S(-39, -17),   S(-15, -11),   S(-40,  -3),   S(-39,  -6),   S(-63, -17),   S(-48, -11),
            S( -8, -45),   S(-12, -49),   S( 14, -21),   S(-27, -11),   S(-33,   2),   S(-45,   4),   S(-39, -31),   S( -3, -12),
            S(-44, -20),   S(-29,   3),   S(-19, -25),   S(  9,  -7),   S(  1,  -3),   S( -8, -39),   S(-50,   4),   S(-51,  10),
            S(-21, -21),   S(-62,  29),   S(-27,  13),   S(-13,  22),   S( 89,  -1),   S( -2,  13),   S( 41, -29),   S(-10,  -6),
            S(-23,   3),   S( 17,  -9),   S(-45,  30),   S( 11,   4),   S( 55,  -7),   S( 50,   8),   S(-16,  14),   S(-30,  -7),
            S(-71,  37),   S(-39,  52),   S(-20,  -9),   S(-79,  31),   S(-27,  19),   S( 16, -13),   S(  6,  38),   S(-47, -76),
            S(-10,  -2),   S(-37,  -1),   S(-46,  21),   S( -5,   9),   S(  3,   0),   S( 25, -24),   S(  8, -27),   S(  4,  -8),
            S(-22, -31),   S( -4,   9),   S( -9,  14),   S(  0,   9),   S( -7,   3),   S( 13, -13),   S( 30, -27),   S(  0,  -5),

            /* bishops: bucket 8 */
            S( 33,  56),   S( -1, -36),   S( -1,   0),   S(-10,  42),   S(  1,  20),   S( -7, -38),   S(-16, -26),   S(-11, -19),
            S(  1,  -1),   S( 14,  26),   S( 22,   6),   S(  8,  20),   S(  1, -17),   S(  2,   1),   S(-34, -50),   S(-10,   2),
            S( -6,  -6),   S(-14, -14),   S( 22,  24),   S( 11,  11),   S(  7,  15),   S( -5,  -2),   S(-25, -13),   S(-33, -27),
            S( -5, -12),   S( 30,  23),   S( -1,  26),   S( 25,  10),   S(  6,  36),   S( 12,  28),   S(-12,   9),   S(  3, -17),
            S( 15,  18),   S( 49,  57),   S( 20,  -2),   S( -6,  23),   S( 11,  24),   S(-22,  24),   S( -6, -24),   S(  5,  20),
            S( -8,  -6),   S(  2,   7),   S(  7,  18),   S( 25,  14),   S( 13,  32),   S( 25,   1),   S( -7,  61),   S( -2,  33),
            S(  3,  15),   S(-18, -45),   S( 26,  -2),   S( 25,   1),   S( 10,   0),   S( 22,  46),   S( 16,  22),   S(-12,  -1),
            S( -6,  -4),   S(  5,   4),   S(  2,  19),   S(  2,  11),   S( 30,   4),   S( 23,  13),   S( 15,  39),   S( 35,  24),

            /* bishops: bucket 9 */
            S(  7,  28),   S(  5,  14),   S( -1,  -1),   S(-30, -27),   S(-19,  -9),   S( -7,  -6),   S( -1,   0),   S( -8,  -5),
            S(  0,  -2),   S(  5, -14),   S(  5,  14),   S(-31,   4),   S(-27,  13),   S(-10, -12),   S(-38, -18),   S(-16, -27),
            S( -8,   4),   S( 17,   7),   S( -5, -25),   S(  4,  23),   S( 12,  15),   S(-30, -22),   S( -2,   8),   S(-10,  -4),
            S( -1,  24),   S(  0, -10),   S( 29,   3),   S( 27,   6),   S( -1,  25),   S( -9,  16),   S(  4,  22),   S( -4,  17),
            S( 26,  18),   S( 19,  13),   S( 28,  22),   S( 18, -18),   S( 13,  31),   S( -1,  34),   S(  7,  36),   S(-15, -16),
            S( 18,  22),   S( -6,  29),   S(  8, -19),   S( 13,  18),   S( 41, -40),   S( -8,  10),   S( 16,  34),   S( 12,  30),
            S( 13,  12),   S(-13,   6),   S(  8,  11),   S( 20,  -1),   S( 23,   1),   S( 33,  18),   S( 15,  26),   S( 18,  57),
            S( 10,  34),   S(  2, -20),   S(  3,  25),   S( 11,  19),   S(  8,  43),   S( 19,  -2),   S( 26,   0),   S( 27,  19),

            /* bishops: bucket 10 */
            S( -1, -31),   S( 12,  11),   S( -2, -20),   S(-24, -20),   S(-66, -18),   S(-32, -59),   S(  8,  -6),   S( -4,  12),
            S( -9,  19),   S( -6, -56),   S( -8, -18),   S(-21, -36),   S(-47,   6),   S(-29, -22),   S(-32, -19),   S(  1,   2),
            S(-10, -33),   S(-19, -18),   S(-19, -33),   S( -5,  27),   S(-14,   7),   S(-13, -35),   S( -7,   3),   S( -5, -19),
            S(-16,  13),   S(-23,   2),   S(-26, -27),   S(  9,   3),   S(-18,  53),   S( 31,  14),   S( 35,  27),   S( -5, -34),
            S( 12,   8),   S(-36,  26),   S( -1,   9),   S(  5,  37),   S( 39,  -8),   S( 25,  40),   S( 22, -14),   S( 17,  10),
            S(  7,  11),   S( 11,  15),   S(-11,  -4),   S( 27,  13),   S( 14, -12),   S(  0,  -9),   S( 11,   9),   S( 25,  14),
            S( 21,  38),   S( -6,  -2),   S( 30, -14),   S( 12,  30),   S( -1,  14),   S( -7, -23),   S(  0, -17),   S( 22,  30),
            S( 10,  24),   S( 21,  32),   S( 44,  17),   S(  8,  21),   S( -4,  26),   S(  6,  15),   S( 13,  20),   S(  0, -17),

            /* bishops: bucket 11 */
            S( 12, -16),   S( -7, -14),   S( -7,  -8),   S(  1,  -5),   S(-21, -17),   S( -3,  -4),   S(-21, -27),   S(-10,  -2),
            S( -7, -12),   S(  0, -26),   S(-11,   8),   S(  2, -14),   S(-15,  12),   S(-42,  -9),   S(-36, -17),   S(  8,   2),
            S(-11, -47),   S( -3, -16),   S(-15, -39),   S(-31,   5),   S( -6, -10),   S(  6,  20),   S( -2, -10),   S( -2, -17),
            S(  4,   0),   S( -2, -30),   S(  5,  -2),   S(-32, -18),   S( 13,   3),   S( 21,  49),   S( 42,  15),   S( -7, -25),
            S( -9, -13),   S(-13,  -9),   S(-37,  40),   S(-25,  34),   S(-21,  32),   S( 39,   6),   S( 30,  -9),   S(  8,   6),
            S( -6,   8),   S(-10,  -8),   S( -9, -10),   S(  2,  21),   S( 25,  20),   S(  8, -26),   S(  3, -11),   S( -1, -15),
            S( -1,  -6),   S( 13,  22),   S( 19,  48),   S( 32,  23),   S( 18,  -6),   S( -7,  -2),   S(-19, -31),   S( -7, -14),
            S( 27,  13),   S(  5,   3),   S( 29,  46),   S( 29, -18),   S( 17,  17),   S(  4,   7),   S( -6, -12),   S(  5,  -5),

            /* bishops: bucket 12 */
            S( -5, -12),   S( -4, -13),   S( -6,  -1),   S(  7,  19),   S( -9,  -8),   S( -8,  -5),   S( -1,   0),   S( -2,   1),
            S(  0,  -6),   S(  6,   1),   S( -1,  -3),   S(  1,  14),   S(  0,   9),   S(  9,   6),   S(-14, -22),   S( -1,  -5),
            S(  8,   5),   S( 11,  -3),   S( 21,  16),   S( 20,  15),   S( -2,  12),   S( -8,  -9),   S(  2,   5),   S( -5,  -2),
            S( 10,   2),   S( 16,   6),   S( 20,   6),   S( 18,  40),   S( 11,   7),   S(  5,  23),   S(  3,  13),   S(  3,   7),
            S( 11,  10),   S( 10,  10),   S( -2,  19),   S( 21,   8),   S( 19,  29),   S(  9,  29),   S(  8,  12),   S(  3,  11),
            S(  2,   1),   S( -8,  -8),   S( -6,  12),   S(  2,  -3),   S( 30,  30),   S(  9,   6),   S( -9,  -9),   S( -4, -10),
            S( -3,  -3),   S(  4,  10),   S(  3,  11),   S(  5,  -5),   S( 12,   2),   S( 20,  24),   S( 11,  25),   S( -1,  -3),
            S(  0,   4),   S( -1,  -4),   S(  1,  -2),   S(  0,  -5),   S(  2,   8),   S(  3, -10),   S( 14,   6),   S(  6,   4),

            /* bishops: bucket 13 */
            S( -5, -19),   S(  0,  -2),   S( -5, -14),   S( -6, -11),   S( 16,  14),   S( -8, -11),   S(-15, -20),   S( -2,  -3),
            S( -5,  -2),   S( -8, -13),   S( -1,   3),   S( 16,   1),   S( -6, -15),   S(  4,  12),   S( -1,  -7),   S(  0,  -3),
            S(  8, -12),   S( 30,  17),   S( 10,  -1),   S( 18,  27),   S(  3,  24),   S(  7,  18),   S( -8,   3),   S( -7,  -5),
            S( 25,  29),   S( 46,  15),   S( 22,  26),   S(-16,   9),   S( 17,  68),   S(  3,  13),   S(  9,   6),   S(  2,  10),
            S( 21,  21),   S( 16,  15),   S( 11,   4),   S(  9,  -7),   S( 11,  -4),   S( 11,  21),   S( 13,  15),   S(  3,  10),
            S(  6,   5),   S(  1,   7),   S( -4, -11),   S( 17,  -3),   S(  6,  15),   S( -7, -20),   S(  3,  -3),   S( 12,   0),
            S(  7,   8),   S(-10, -20),   S( -2, -17),   S(  4,   6),   S(  5,  17),   S( 17,  10),   S(  7,  -6),   S(  9,  12),
            S(  1,  -2),   S( -2,  -1),   S(  0,  13),   S(  2,  10),   S(  7,  15),   S(  3, -11),   S( 13,  -2),   S( 11, -10),

            /* bishops: bucket 14 */
            S(-13, -24),   S(  5,  22),   S( 16,  13),   S(  5,  22),   S(-12,  -3),   S( -8,  -7),   S( -5,   3),   S( -8,  13),
            S( -1,   1),   S( -2,  -6),   S(  2,  11),   S( -2,  -8),   S( 12,   2),   S(  2,   8),   S( -6,  15),   S(  3,  28),
            S(  1,  -3),   S( -2, -13),   S( -9, -15),   S( 20,  33),   S( 23,  44),   S( 10,  19),   S(  5,  36),   S(  3,  28),
            S(  4,  32),   S(  8, -12),   S( -2,   0),   S(  2,  30),   S( 10,  21),   S( 21,   8),   S( 21,  17),   S(  9, -18),
            S( 10,   7),   S(  7,  15),   S( 12,   5),   S( 20,  10),   S( -3,   6),   S(  5,  14),   S( 22,   0),   S( 15,  12),
            S(  2, -11),   S( 23,  37),   S(  2,   6),   S( 15,   8),   S(  9,   1),   S( -6,   2),   S( -2,  20),   S( 16,   1),
            S( 16,  35),   S(  6,   8),   S( 12,  19),   S(  7,  11),   S(  7,   1),   S(  4,  12),   S(  0, -11),   S(  2,   1),
            S( 13,   2),   S( 13,  18),   S(  4,  11),   S(  5,   2),   S( -4,  -3),   S(  2,  -3),   S(  8,  11),   S(  3,   3),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -1),   S( -7, -15),   S( -2,  -1),   S( -6, -22),   S( -3,  -7),   S( -5, -14),   S( -4,  -5),
            S(  8,  13),   S( -4, -10),   S(  6,   4),   S(  4,   4),   S(  8,  -2),   S( -1,  -4),   S( -2, -10),   S( -3,  -5),
            S(  3,  -4),   S(  2,   0),   S(  0,  -8),   S( 14,  16),   S( 12,  29),   S(  8,  25),   S( 16,  21),   S(  4,   3),
            S(  1,  -8),   S( 13,  14),   S( 12,  30),   S(-17,  -4),   S(  4,   7),   S( 17,   5),   S( 14,   2),   S(  9,  16),
            S( -2,  -8),   S( -1,  12),   S( -3,  20),   S( 21,  54),   S( 20,  24),   S( 11,  -1),   S(  8,   2),   S( -3,   2),
            S( -2,  20),   S(  6,  12),   S(  4,  24),   S(  7,  11),   S( 23,  20),   S(  7, -12),   S(  3,  10),   S(  2,  -2),
            S(  5,  -3),   S(  2,  17),   S(  8,  29),   S( 13,  17),   S( 10,  16),   S( -2,   8),   S( -1,  -9),   S(  0,   0),
            S(  3,  -3),   S( 10,  13),   S(  7,  -2),   S(  9,  11),   S(  5,  17),   S(  1,  -1),   S(  5,  11),   S(  5,   0),

            /* rooks: bucket 0 */
            S(-20,  13),   S(  8, -11),   S(-11,   2),   S(-12,  17),   S(-33,  61),   S(-20,  36),   S(-51,  63),   S(-58,  47),
            S(  0, -22),   S( -4,  15),   S(-32,  22),   S( -6,  29),   S( -8,  43),   S(-11,  25),   S(-22,  12),   S(-29,  43),
            S( 22, -34),   S(  9, -15),   S(-13,  10),   S( -7,  13),   S(-36,  56),   S(-17,  16),   S(-22,  40),   S( -8,  16),
            S(  9, -22),   S( 37,  -5),   S(-32,  29),   S( 15,  15),   S(  8,  48),   S(-19,  45),   S(-24,  52),   S(-19,  33),
            S( 53, -62),   S( 44,  -3),   S( 22,  23),   S( 35,  21),   S( 41,  18),   S( 22,  66),   S( 33,  49),   S(  6,  58),
            S( 58, -30),   S( 63,  16),   S(111, -21),   S(104,  22),   S( 27,  53),   S( 31,  59),   S(  7,  68),   S(-42,  80),
            S( 24,  17),   S( 53,  43),   S( 97,  29),   S( 66,  11),   S( 61,  46),   S( 16,  61),   S(-10,  73),   S(-18,  67),
            S(  3, -21),   S( 28,  23),   S( 26,  22),   S( 42,  -4),   S( 23,  44),   S( 42,  13),   S( 34,  15),   S( 51, -44),

            /* rooks: bucket 1 */
            S(-56,  50),   S(-21,   6),   S(-15,  14),   S(-45,  32),   S(-42,  45),   S(-47,  47),   S(-54,  68),   S(-78,  74),
            S(-47,  35),   S(-23,  -9),   S(-24,  16),   S(-30,  24),   S(-34,  17),   S(-46,  42),   S(-27,  20),   S(-37,  53),
            S(-33,  25),   S(-12,  -6),   S(-13,  -1),   S(-25,  14),   S(-32,  19),   S(-52,  35),   S(-62,  62),   S(-29,  59),
            S(-42,  46),   S( -4,   9),   S(-15,  25),   S(-27,  14),   S(-37,  36),   S(-50,  65),   S(-34,  61),   S(-71,  88),
            S(-14,  42),   S( 18,  -9),   S( 32,  10),   S( 29,   0),   S(  4,  23),   S( -7,  77),   S(  6,  60),   S(-10,  83),
            S( 47,  32),   S( 72,  -5),   S( 39,  12),   S(  0,  29),   S( 11,  20),   S( 11,  56),   S( 38,  43),   S(  9,  78),
            S( 15,  65),   S( 35,   2),   S(  6,  31),   S( 17,  15),   S( 49,  12),   S(  3,  51),   S( 27,  62),   S( 32,  80),
            S( 51, -11),   S( 14,  -8),   S( -2, -11),   S(-19, -10),   S( 23,   4),   S( 17,  16),   S( 33,  29),   S( 49,  34),

            /* rooks: bucket 2 */
            S(-62,  69),   S(-51,  60),   S(-43,  53),   S(-39,  23),   S(-28,  24),   S(-41,  27),   S(-31,  16),   S(-69,  58),
            S(-57,  64),   S(-54,  55),   S(-49,  55),   S(-47,  32),   S(-49,  38),   S(-46,  19),   S(-20,   4),   S(-51,  38),
            S(-47,  65),   S(-33,  54),   S(-42,  41),   S(-36,  36),   S(-32,  23),   S(-31,  20),   S(-14,   6),   S(-12,  28),
            S(-37,  76),   S(-30,  69),   S(-47,  64),   S(-60,  51),   S(-45,  45),   S(-24,  29),   S( -9,  26),   S(-21,  45),
            S(-13,  85),   S(-20,  81),   S(  5,  67),   S(-13,  41),   S(-29,  52),   S( 28,  22),   S(  7,  39),   S( -1,  64),
            S( 21,  84),   S( 21,  72),   S( 37,  62),   S(-11,  48),   S( 52,  14),   S( 42,  47),   S(111,  -3),   S( 58,  60),
            S( 50,  63),   S( -6,  76),   S( 18,  52),   S( 33,  20),   S(  2,   6),   S( 24,  70),   S(-39,  88),   S( 36,  67),
            S( 14,  41),   S( 21,  47),   S( 26,  31),   S(-26,  23),   S(-31,  10),   S( 16,  10),   S( 14,  21),   S( -5,  51),

            /* rooks: bucket 3 */
            S(-18,  72),   S(-13,  69),   S(-15,  91),   S(-12,  82),   S( -1,  47),   S(  2,  41),   S( 20,  12),   S( -9,   4),
            S( -1,  60),   S(-15,  73),   S(-15,  94),   S( -5,  85),   S( -3,  51),   S( 14,  13),   S( 46, -13),   S( 18,   5),
            S( 12,  56),   S( -7,  82),   S(-11,  81),   S( -7,  88),   S( 15,  39),   S(  6,  30),   S( 37,   8),   S( 32,   5),
            S(  2,  88),   S( -7, 107),   S(-17, 109),   S( -7,  96),   S( -3,  67),   S( 16,  48),   S( 36,  29),   S(  7,  26),
            S(  4, 105),   S(-10, 118),   S( 19, 111),   S( 19, 101),   S( 15,  84),   S( 45,  57),   S( 67,  32),   S( 42,  42),
            S(  7, 123),   S( 24, 107),   S( 34, 113),   S( 49,  95),   S(103,  43),   S(134,  24),   S( 90,  37),   S( 50,  37),
            S( 20, 110),   S( 15, 109),   S( 28, 116),   S( 25, 110),   S( 33,  91),   S(102,  42),   S(106,  93),   S(141,  63),
            S(115, -32),   S( 50,  41),   S( 13,  96),   S( 15,  79),   S( 17,  67),   S( 70,  55),   S( 39,  31),   S( 97,   9),

            /* rooks: bucket 4 */
            S(-22, -25),   S( 16, -18),   S(-21,  -7),   S(-40,  17),   S(-55,  18),   S(-39,  48),   S(-41,   5),   S(-85,  38),
            S(-25, -46),   S(-48,  -3),   S(-18, -18),   S(  2, -31),   S( 16, -15),   S( -9,   5),   S(-27,   0),   S( -2,  16),
            S(-13, -21),   S(-38, -23),   S(-38,  -7),   S( -9, -36),   S(-32,  -5),   S(-45,  18),   S(-22,  18),   S(-63,  22),
            S(-54, -35),   S( 10,   2),   S( 10, -22),   S( 13, -19),   S( 41,   3),   S( -7,  15),   S(-11,  -3),   S(-15,  12),
            S(-15, -34),   S( 27, -38),   S( 25,   2),   S( 50, -15),   S( 64,  -3),   S( 56,  27),   S( 17,  13),   S( 14,  27),
            S(-14, -36),   S( 10,  11),   S(  8,  -2),   S( 20,  10),   S( 30,  23),   S( 14,  14),   S( 33,  18),   S( 33,  39),
            S(-19, -22),   S( 34,  24),   S( 45,  -3),   S( 55, -10),   S( 62,  -2),   S(-11,  14),   S( 15, -12),   S( 22,   8),
            S( 17, -28),   S(  8,  14),   S( 33, -11),   S( 27, -11),   S( 53,   4),   S( 13,   5),   S(  5,  10),   S(  4,  14),

            /* rooks: bucket 5 */
            S(-25,  26),   S(-15,   5),   S(  1,  -5),   S( 24,  -5),   S(-11,  19),   S(-14,  31),   S(-32,  54),   S(-33,  36),
            S(-10,  -3),   S(-25, -14),   S( 42, -56),   S( 29, -22),   S(-20,   6),   S(-26,  13),   S(-41,  31),   S(-14,  30),
            S(-36,  23),   S( -4,  -6),   S(  6, -25),   S( -3, -16),   S(-17,  -5),   S( 31, -16),   S(-44,  33),   S(-26,  20),
            S(-23,  24),   S(  6,   3),   S( 53, -30),   S( 35,  -8),   S( 40, -10),   S( -8,  38),   S( 12,  31),   S(  6,  45),
            S( 40,  18),   S( 25,   5),   S( 17,  18),   S( 10,  -4),   S( -4,  19),   S( 72,   8),   S( 30,  34),   S( 49,  35),
            S(  0,  31),   S( -5,  11),   S(  5,   5),   S(-12, -12),   S( 22,  12),   S( 19,  26),   S( 58,  16),   S( 50,  32),
            S( 49,   3),   S( 48, -10),   S(  0,  -1),   S( 37,   7),   S( 58,  -7),   S( 57, -13),   S( 85, -13),   S( 45,  15),
            S( 20,  30),   S( 16,   6),   S( 53,  -8),   S(  5,  15),   S( 43,  17),   S( 23,  30),   S( 32,  40),   S( 57,  40),

            /* rooks: bucket 6 */
            S(-49,  47),   S(-37,  39),   S(-31,  30),   S(-31,  23),   S( -1,   7),   S(  5,  -2),   S( 22,  -8),   S(-36,  21),
            S(-44,  29),   S(  7,   9),   S(-11,  12),   S( -6,   2),   S( 17, -18),   S(-26,  -3),   S(-29,   0),   S(-10,  12),
            S(-55,  39),   S( -4,  18),   S( -1,   6),   S(  1,   1),   S(-11,   5),   S( 35, -17),   S(  5, -19),   S( -9,  -4),
            S(-33,  52),   S( -6,  38),   S( 18,  14),   S( 60, -11),   S( 41, -16),   S( 21,   0),   S( 15,  -2),   S( 18,  28),
            S( -5,  52),   S( 58,  25),   S( 90,  17),   S( 72,  -8),   S( 30,  -9),   S( 33,  14),   S( 72, -10),   S( 90,   1),
            S( 79,  13),   S( 84,  -1),   S( 84,  -1),   S( 44, -16),   S(  4, -14),   S( 23,  31),   S( 35,  -6),   S( 59,  15),
            S( 59,  11),   S(130, -23),   S(108, -25),   S(100, -38),   S( 37, -16),   S( 52,  -5),   S( 68, -13),   S( 87, -23),
            S( 77,  -8),   S( 51,  16),   S(  6,  30),   S( 63,  -8),   S( 61,   0),   S( 32,  24),   S( 87,   4),   S( 57,  19),

            /* rooks: bucket 7 */
            S(-99,  35),   S(-80,  35),   S(-71,  37),   S(-62,  34),   S(-34,  -2),   S(-27, -18),   S(-34,   4),   S(-70, -16),
            S(-86,  34),   S(-34,  10),   S(-56,  21),   S(-65,  31),   S(-33, -14),   S(-17, -15),   S(  5,  -8),   S( -4, -58),
            S(-84,  36),   S(-67,  29),   S(-34,   7),   S(-43,  21),   S(-40,   0),   S(-24,   6),   S( 44, -35),   S(  2, -53),
            S(-73,  36),   S(-15,  15),   S( -3,  12),   S( 63, -23),   S( 16,  -8),   S( 71, -32),   S( 53, -11),   S( 28, -29),
            S(  0,  27),   S( 30,  21),   S( 59,  10),   S( 88, -14),   S(143, -51),   S(123, -56),   S( 88, -25),   S(-41, -37),
            S( 30,  15),   S( 30,   2),   S( 93,  -6),   S( 86, -23),   S( 78, -14),   S( 39,   8),   S( 23,  33),   S( -2, -28),
            S(  8,  -2),   S( 40, -14),   S( 75, -16),   S(111, -44),   S(122, -46),   S(116, -46),   S( 50,   5),   S( 22, -31),
            S(-22, -16),   S(  8,   5),   S( 40,  -2),   S( 31,   0),   S( 51, -16),   S( 72, -13),   S( 36,  10),   S( 29, -22),

            /* rooks: bucket 8 */
            S(-14, -81),   S(-14, -37),   S( -7, -14),   S( 18,   7),   S(-24, -27),   S(-21,   5),   S(-12, -30),   S(-20,   9),
            S(-32, -81),   S(-15, -44),   S(-22,   0),   S(-27, -66),   S(-25, -39),   S(-16, -19),   S(-10,  -6),   S(-38, -33),
            S(  1, -10),   S( -3, -14),   S( 12,  -5),   S(-11,  17),   S( -8,  48),   S( 13,  27),   S(  5,  50),   S(-18,   4),
            S( -5, -21),   S( -1,   5),   S( -1,  -2),   S( 15,  24),   S(  4,  42),   S( 32,  41),   S(  0,  21),   S( -9, -11),
            S( -9, -41),   S( 11,  21),   S(  9,  18),   S( 17,  37),   S(  8,  23),   S( -2,   5),   S( 13,  46),   S(  0,  22),
            S(-24,   9),   S(  4,  12),   S(-16,   8),   S( -6, -16),   S(  5,  35),   S(-15,  32),   S(  0,   4),   S(  2,  23),
            S(  3,  34),   S(  2,  26),   S(  3,   6),   S( 20,  13),   S( 14,  11),   S( 10,  32),   S(  5,  26),   S(  3,  47),
            S(-11,  14),   S(  3,  14),   S(-19,  31),   S( 34,  50),   S( -6,  24),   S( 12,  42),   S(  2,  25),   S(  7,  42),

            /* rooks: bucket 9 */
            S(-31, -68),   S( -9, -66),   S( -5, -99),   S(-10, -44),   S(-15, -48),   S(  1, -32),   S( -6, -19),   S( -4, -30),
            S(-59, -50),   S(-29, -72),   S(-27, -66),   S(-41, -49),   S(-35, -55),   S(-26,   3),   S(-21, -52),   S(-30, -30),
            S(-11, -14),   S(-21, -15),   S(  3,  -7),   S( -7, -32),   S( -5, -17),   S(  6,  19),   S(  3,   7),   S(  3,  17),
            S( -5,   2),   S(  4,  -3),   S(  3,   2),   S( -1,   5),   S(-12, -32),   S(  5,   0),   S( -7,  -1),   S(  5, -22),
            S( -4,   0),   S( -8, -15),   S( -7, -48),   S( -8,   3),   S(-17, -19),   S(-11,   2),   S(-13, -13),   S( -7, -10),
            S( -9,   3),   S(-29, -19),   S(-12, -20),   S( -1,  18),   S( -4,  -3),   S( -8,  11),   S( -5,   0),   S(-13,  10),
            S(  5,  33),   S(  7,   2),   S(  5, -34),   S(  2,  15),   S(  7, -16),   S( 20,   6),   S(  4,  13),   S( -4,  -9),
            S(-18,  16),   S(-17,  27),   S( -7,  16),   S( -6,  35),   S( -8,  30),   S(  5,  55),   S(  3,  22),   S( 11,  30),

            /* rooks: bucket 10 */
            S(-23, -26),   S(-55,  -8),   S(-31, -40),   S( -5, -51),   S(-13, -47),   S(  2, -79),   S(  6, -64),   S(-19, -42),
            S(-42, -13),   S(-31, -32),   S(-42, -26),   S(-37, -52),   S(-41, -46),   S(-23, -47),   S(-10, -38),   S(-45, -75),
            S( -8, -11),   S(-22, -15),   S(-28, -18),   S(-38, -45),   S( -9, -16),   S(  3, -16),   S( -8, -30),   S(-13, -15),
            S(-25,  -7),   S(-35, -32),   S( -4, -34),   S( -8,   1),   S(  6,   4),   S(  6,  12),   S(-10, -35),   S(  1, -36),
            S(  8,  -8),   S(  2,  -8),   S(-13, -16),   S( -9, -36),   S(  7,  11),   S( -3,   0),   S( -5, -26),   S( -8, -36),
            S(-10,   5),   S( 12,   0),   S( -4, -17),   S(  0, -31),   S(  2,  -8),   S( -7,  -7),   S(-18, -33),   S( -4, -18),
            S(-10,  -5),   S(  8, -25),   S( -1, -19),   S( -4, -13),   S( 13, -18),   S(-10,  -8),   S(-13, -31),   S( -8, -13),
            S( -7,   1),   S(  7,  32),   S(  0,  35),   S(-11,  12),   S( -8,  33),   S(-27,   7),   S(-29,  15),   S( -4,  14),

            /* rooks: bucket 11 */
            S(-60, -15),   S(-39,   0),   S(-51,  -7),   S(-27,  -5),   S(-46, -17),   S(-18, -18),   S(-17, -34),   S(-36, -65),
            S(-18, -13),   S(-22, -19),   S(-57, -10),   S(-53, -19),   S(-15, -25),   S(-10, -12),   S(-25, -31),   S(-43, -61),
            S(-30,  25),   S(-22,  13),   S( -7,  32),   S(-19,  19),   S(  7, -22),   S( -6,  -4),   S(  6, -21),   S(-10,  12),
            S(-24,  -5),   S( -9, -16),   S(-12,  13),   S(  8,  17),   S( 21,  12),   S(-19, -34),   S(  6,  16),   S( -8, -21),
            S( -8,  -7),   S(  6,  -3),   S(  5,   7),   S(  4,   8),   S( 35,  -8),   S(  1,  -6),   S( 18,  34),   S(-15, -44),
            S(  4, -13),   S(-12,  -3),   S( 14,  -9),   S( 19,  -3),   S(-12, -16),   S(  5,   4),   S(  7,  31),   S( -4, -10),
            S( -6,  10),   S(-22, -25),   S( -4,   1),   S( -1,   3),   S(  8,  -2),   S(  5,   7),   S(  2,  15),   S(-12,  -6),
            S( -8,   7),   S( 16,  36),   S(  3,  28),   S( 19,  25),   S(-12,   8),   S( -3,  25),   S( 14,  13),   S(-21,  24),

            /* rooks: bucket 12 */
            S(-33, -98),   S( -9, -13),   S(-19, -55),   S(-19, -36),   S(-12, -26),   S(  9,  -8),   S(-16, -40),   S(-18, -40),
            S(  3,   3),   S(  1,   4),   S(  8,  20),   S(  4,  14),   S(  8,   7),   S( 10,  -6),   S(  6,   9),   S(-18, -22),
            S( -4, -11),   S(  8,  35),   S( 11,  23),   S( 24,  23),   S(  6,  -7),   S( 16,  25),   S(  6,  33),   S( -2,  27),
            S(  6,  22),   S(  8,   2),   S( 14,  33),   S( 10,  20),   S( 13,   9),   S(  6,   8),   S(  6,  20),   S( -2,   6),
            S( 12,  16),   S( 14,  30),   S(  8,  47),   S(  2,   0),   S(  9,  28),   S( -2, -13),   S(  5,  16),   S(  6,  14),
            S( -2,   1),   S( -3,  -6),   S(  0,  17),   S( -5,   3),   S(  8,  25),   S(  0, -20),   S( 10,  26),   S(  5,  10),
            S(-15, -10),   S(-12,  19),   S(  7,  40),   S(  0,  21),   S( -2,   2),   S( 12,  18),   S(  3,  24),   S(  0,  24),
            S(  3,   4),   S( -8,  29),   S(  5,  31),   S( 14,  23),   S(  2,   7),   S(  1,  21),   S(  2,  10),   S(  2,  14),

            /* rooks: bucket 13 */
            S(-25, -24),   S(-24, -51),   S(-24, -51),   S(-15, -35),   S(-27, -51),   S( -3,  -4),   S(-25, -48),   S(-23, -37),
            S(-14, -11),   S( -8, -18),   S(  2,   6),   S( -2,  -3),   S( 18,  35),   S(  4,  13),   S(  8,   1),   S(-10, -12),
            S(-14,  -2),   S(-14,   3),   S( -4,  -8),   S(  8,  10),   S(  6,  25),   S( 14,  -1),   S( 12,  44),   S(-12, -25),
            S(  8,  15),   S( -2,   6),   S( -2,   9),   S(  5,  18),   S( 11,  21),   S(  0,   7),   S(  6,  15),   S(  1,  21),
            S(  7,  21),   S(  3,  -8),   S( -5, -21),   S(  3,   5),   S( -5,  24),   S(  1,  -2),   S(  6,   8),   S( -1,  -2),
            S(  1,  15),   S( -4,  -5),   S(-10,  -9),   S(-13,   0),   S(-12, -14),   S(  3,  -2),   S( -7,   9),   S(  1,   5),
            S(  3,  -8),   S(  7,   6),   S( -9, -28),   S(  3,  17),   S( -9,  -3),   S(  7,  11),   S(  2,   5),   S(  0, -13),
            S(  2,  23),   S(-10,  13),   S( -4,   7),   S( 10,  27),   S( -3,  18),   S(  7,  24),   S(  0,  24),   S(  4,   6),

            /* rooks: bucket 14 */
            S( -5, -26),   S(-30, -29),   S(-17, -16),   S(-18, -53),   S(-11, -37),   S( -5, -21),   S(-32, -57),   S(-25, -36),
            S( -7,  27),   S(  4,  27),   S(  6,   8),   S( -1, -19),   S(  0,  -8),   S( -3,  -4),   S( -1,   5),   S( -4,  -5),
            S(  4,  31),   S( -2,  27),   S(  1,   2),   S(  2,   1),   S(  4,   9),   S(  0,  -5),   S(  2,  20),   S(-19, -49),
            S( -4,  14),   S( 15,  21),   S(  6,  16),   S(  9,   5),   S( -8,  -6),   S(  1, -10),   S(  9,  12),   S(-12, -18),
            S(  9,  19),   S( 19,  24),   S( -2,  -2),   S(  2,   7),   S(  3, -11),   S( 17,  31),   S(  0,   4),   S( -3, -14),
            S(  6,  15),   S(  7,  15),   S(  7,  18),   S(  2,   4),   S( -4,   8),   S(-15,   7),   S( -8,  -7),   S( -6,  -5),
            S( -5,  -8),   S(  9,  18),   S( -7, -17),   S(-18, -33),   S( -4,   7),   S(  0,   3),   S(-12, -12),   S( -8,  -7),
            S(  0,   2),   S(  4,   8),   S( -4, -13),   S(  6,  -9),   S(-10, -15),   S(-14, -41),   S(  2,  -5),   S(  1,  32),

            /* rooks: bucket 15 */
            S(-24, -44),   S(-17, -47),   S(-39, -48),   S(-23, -50),   S( -2, -22),   S(-13, -20),   S( -3, -10),   S(-20, -51),
            S(  7,  32),   S(-11,   2),   S(-11,  -8),   S( -6,  -9),   S( -6, -18),   S(  4,  -1),   S(  7,  11),   S(  3,   5),
            S(  6,   8),   S( -6, -15),   S( 11,  24),   S(  8,  -2),   S(  6,  -3),   S( -4, -17),   S(  6,  23),   S(  3,   8),
            S(  2,  10),   S( -2,  -5),   S( 18,  33),   S( -3, -11),   S(  4,  17),   S(  2,   8),   S(  7,  14),   S(  3, -12),
            S(  7,  17),   S(  6,  11),   S(  6,  -8),   S(  4,  14),   S(  6,  15),   S(  3,   0),   S( -2,  27),   S(  5, -10),
            S(  7,  19),   S(  8,   4),   S(  9,   2),   S(  5,   7),   S( -5, -11),   S( -4,  38),   S(  2,  23),   S(  5,   2),
            S(  4,  -2),   S( -2,   8),   S(  9,  20),   S(  5,  13),   S(  2,  17),   S(  5,  14),   S(-13,  10),   S(-10, -30),
            S(  1,  25),   S(  0,  28),   S(  9,  25),   S(  2,  29),   S(  0,   4),   S( -5, -24),   S( -5,  14),   S(-15, -11),

            /* queens: bucket 0 */
            S( -3,  -7),   S(-25, -48),   S(-32, -58),   S(  1, -96),   S( -7, -54),   S(  9, -58),   S(-56, -27),   S(-15, -10),
            S(-13, -28),   S( 12, -75),   S(  3, -66),   S(-10, -18),   S(  2, -20),   S( -9, -35),   S(-26, -26),   S(-37,  -9),
            S( -3,   9),   S( -4, -22),   S( 27, -51),   S(-12,   8),   S( -9,  25),   S( -5,   2),   S(-35,   0),   S(-78, -40),
            S(-24,  27),   S( 16, -26),   S( -6,  18),   S(-12,  65),   S( -7,  64),   S(-25,  37),   S(-44,  25),   S(-17, -25),
            S(-18, -16),   S(  3,  66),   S(  6,  30),   S( -2,  41),   S(  3,  63),   S(-28, 107),   S(-58,  68),   S(-41,   2),
            S(-12,  12),   S( 19,  34),   S( 38,  36),   S(-23,  71),   S(-30,  66),   S(-64,  97),   S(-65,  25),   S(-41,   4),
            S(  0,   0),   S(  0,   0),   S( 19,   2),   S(-32,  30),   S(-36,  24),   S(-64,  79),   S(-89,  61),   S(-101,  25),
            S(  0,   0),   S(  0,   0),   S(  8,  -6),   S(-10, -13),   S(-31,  22),   S(-37,   3),   S(-52,  -5),   S(-64, -27),

            /* queens: bucket 1 */
            S( 16,  -4),   S(  6,   2),   S( 13, -47),   S( 31, -87),   S( 37, -45),   S( 11, -25),   S( 14,  -5),   S(  2,  17),
            S(-23,  32),   S( 21,  16),   S( 37, -39),   S( 26,   3),   S( 40,  11),   S(  1,  20),   S(-21,  35),   S(-19,  10),
            S( 45,  -3),   S( 22,   1),   S( 15,  32),   S( 16,  71),   S( -7,  80),   S( 30,  43),   S( -7,  36),   S( 15,  -9),
            S( 36,   5),   S( 18,  39),   S( 18,  48),   S( 44,  67),   S( 17,  82),   S(  4,  58),   S(  8,  38),   S(-11,  54),
            S( 45,  -2),   S( 52,  15),   S( 51,  40),   S( 25,  34),   S( 46,  67),   S( 34,  24),   S(-10,  71),   S(  1,  91),
            S( 63,  -1),   S(102,  11),   S( 83,  44),   S( 84,  56),   S( 56,  38),   S( 16,  62),   S( 39,  52),   S(  1,  51),
            S(101, -21),   S( 58, -19),   S(  0,   0),   S(  0,   0),   S(  6,  43),   S(-10,  23),   S( -8,  51),   S(-41,  36),
            S( 81,  -4),   S( 60,  -9),   S(  0,   0),   S(  0,   0),   S( 19,  23),   S( 42,  27),   S( 79,   0),   S(-18,  34),

            /* queens: bucket 2 */
            S( 33, -14),   S( 27,  10),   S( 30,  21),   S( 44, -23),   S( 43, -29),   S( 29, -22),   S( -1, -20),   S( 35,  29),
            S( 21,   4),   S(  5,  49),   S( 36,  20),   S( 42,  32),   S( 51,   7),   S( 20,  23),   S( 23,  18),   S( 16,  48),
            S( 34,  11),   S( 28,  35),   S( 18, 100),   S( 15,  82),   S( 24,  77),   S( 22,  68),   S( 31,  45),   S( 30,  60),
            S(  4,  59),   S( 21,  81),   S( 19,  81),   S( 12, 122),   S( 28,  96),   S( 22,  91),   S( 37,  60),   S( 33,  81),
            S(  1,  85),   S( -6,  72),   S(  2,  93),   S( 32,  76),   S( 24,  99),   S( 90,  44),   S( 73,  57),   S( 67,  54),
            S(-16,  88),   S( -7,  79),   S(  2,  75),   S( 78,  35),   S( 40,  56),   S( 98,  72),   S(119,  38),   S( 44, 108),
            S( -3,  54),   S(-12,  48),   S( -8,  70),   S( 50,  28),   S(  0,   0),   S(  0,   0),   S( 22,  78),   S( 47,  72),
            S( -1,  36),   S( 36,  -4),   S( 49, -12),   S( 32,  35),   S(  0,   0),   S(  0,   0),   S( 53,  42),   S( 16,  64),

            /* queens: bucket 3 */
            S(-44,  37),   S(-31,  43),   S(-24,  41),   S(-13,  50),   S(-27,  33),   S(-16, -14),   S(-17, -36),   S(-41,  25),
            S(-59,  57),   S(-38,  46),   S(-25,  63),   S(-17,  81),   S(-15,  71),   S(-16,  35),   S( 15, -14),   S( 15, -26),
            S(-53,  76),   S(-39,  85),   S(-34, 113),   S(-41, 142),   S(-30, 122),   S(-25,  93),   S(-11,  52),   S(-12,  20),
            S(-42,  77),   S(-61, 135),   S(-52, 158),   S(-36, 171),   S(-39, 160),   S(-18,  95),   S( -3,  74),   S(-15,  66),
            S(-54, 120),   S(-47, 154),   S(-55, 174),   S(-45, 186),   S(-26, 152),   S( -1, 128),   S(-12, 121),   S(-17,  80),
            S(-59, 110),   S(-60, 156),   S(-63, 179),   S(-60, 189),   S(-51, 163),   S( 19,  92),   S(-21, 121),   S(-23, 120),
            S(-96, 122),   S(-95, 142),   S(-79, 180),   S(-72, 157),   S(-75, 162),   S(-16,  82),   S(  0,   0),   S(  0,   0),
            S(-125, 138),  S(-82, 101),   S(-70, 102),   S(-65, 108),   S(-52,  99),   S(-13,  57),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-34,  -4),   S(-49, -35),   S( -9,  -1),   S( -9, -19),   S( -7,  -6),   S( -9,  11),   S(-33, -26),   S( 13,  21),
            S( -1, -11),   S( -8,   6),   S( -4,   1),   S(-18, -13),   S(-44,  19),   S(-17,  12),   S(-46,  -9),   S(  1, -15),
            S( 10,  19),   S( 22, -29),   S( 16, -17),   S( 16,   9),   S( 40,  11),   S( 13,  22),   S(-22, -18),   S( 33,  23),
            S(-10, -21),   S( 21, -19),   S(  8,   2),   S( -9,  18),   S( 44,  29),   S(  0,  59),   S(-26,   6),   S(-11,  19),
            S(  0,   0),   S(  0,   0),   S( 17,  -7),   S( 56,  36),   S( 25,  56),   S( 31,  51),   S(  9,  16),   S( 11,  21),
            S(  0,   0),   S(  0,   0),   S( 18,  10),   S( 35,  19),   S( 42,  47),   S( 30,  49),   S( 18,  24),   S(  0,   6),
            S( 16,  -3),   S( 20,   9),   S( 64,  38),   S( 62,  36),   S( 57,  13),   S( 18,  27),   S(  5,  22),   S(-12,  21),
            S( 29,  -7),   S(-18, -31),   S( 25,   7),   S( 46,  17),   S( 16,   6),   S(  9,  21),   S( -1,   2),   S( 20,   7),

            /* queens: bucket 5 */
            S( 35,  22),   S( 23,   6),   S( 14,   6),   S(-13,  26),   S( 32,  -6),   S( 39,  46),   S( 11,  -1),   S( 20,   2),
            S( 18,  15),   S( 15,  -2),   S( 13,  -1),   S(  9,  14),   S( 10,  42),   S(-14, -12),   S( 27,  15),   S( 13,   5),
            S( 21,   3),   S( 47,  -4),   S( 23,   0),   S(  9,  16),   S( 19,   7),   S( 31,  17),   S( 25,  40),   S( 12,  13),
            S( 10, -31),   S( 38,   4),   S( 26, -15),   S( 34,  16),   S( 63,   9),   S( 33,  14),   S( 35,  47),   S(  4,  30),
            S( 40,  -6),   S( 28, -41),   S(  0,   0),   S(  0,   0),   S( 11,  10),   S( 31,  15),   S( 39,  51),   S( 16,  33),
            S( 38,  16),   S( 36,   6),   S(  0,   0),   S(  0,   0),   S( 30,  20),   S( 63,  34),   S( 44,  37),   S( 52,  40),
            S( 75,   6),   S( 73,  11),   S( 51,  40),   S( 25,  25),   S( 53,  21),   S( 95,  44),   S( 66,  55),   S( 50,  29),
            S( 43,  30),   S( 55,  13),   S( 66,  20),   S( 44,  -2),   S( 55,  18),   S( 63,  37),   S( 69,  46),   S( 59,  30),

            /* queens: bucket 6 */
            S( 48,  50),   S(  1,   3),   S( 33,  14),   S( 34,  20),   S( 23,  14),   S( -8,   0),   S(  0,  11),   S(  8,  20),
            S( 26,  18),   S( 26,  32),   S( 55,  42),   S( 51,  28),   S( 38,  24),   S( 16,  13),   S(-12,  26),   S( 26,  32),
            S(-14,  45),   S( 34,  35),   S( 28,  37),   S( 50,  14),   S( 35,  13),   S( 47,   0),   S( 63,  26),   S( 69,  59),
            S( 25,  36),   S(  6,  26),   S( 50,  10),   S( 96,  19),   S( 47,  -8),   S( 48,  10),   S( 85,   9),   S(101,  45),
            S( 30,  52),   S( 33,  36),   S( 55,  38),   S( 52,  32),   S(  0,   0),   S(  0,   0),   S( 67,  22),   S(115,  56),
            S( 42,  49),   S( 57,  48),   S( 49,  56),   S( 28,   9),   S(  0,   0),   S(  0,   0),   S( 82,  49),   S(117,  47),
            S( 60,  36),   S( 27,  23),   S( 75,  17),   S( 61,  18),   S( 43,  38),   S( 70,  46),   S(132,  26),   S(144,  10),
            S( 38,  38),   S( 66,  23),   S( 74,  15),   S( 82,  34),   S(104,  13),   S(100,  13),   S(113,  13),   S(101,  28),

            /* queens: bucket 7 */
            S( -8,  23),   S( -8,  -1),   S(-23,  21),   S( -8,  23),   S( 11,   2),   S(-14,   3),   S( -5,  16),   S(-13,  -6),
            S(-11,  25),   S(-50,  27),   S( -9,  54),   S(-15,  79),   S(-14,  42),   S(  5,  24),   S(  6,   3),   S(-31,  -1),
            S(  1,  24),   S(-20,  37),   S(-24,  91),   S( 32,  50),   S( 46,  29),   S( 29,   6),   S( 53, -28),   S( 54,  -2),
            S(-17,  22),   S( 15,  43),   S( 14,  69),   S( 41,  70),   S( 76,  44),   S( 74,  -5),   S( 84, -34),   S( 52,  -6),
            S( 13,  23),   S(-14,  60),   S( 14, 101),   S( 49,  80),   S( 87,  18),   S( 71,  -3),   S(  0,   0),   S(  0,   0),
            S(  1,  46),   S(-10,  86),   S(  8,  90),   S(  0,  85),   S( 63,  35),   S( 98,  51),   S(  0,   0),   S(  0,   0),
            S(-37,  59),   S(-22,  39),   S( 11,  55),   S( 36,  56),   S( 67,  35),   S( 85,  15),   S( 70,  24),   S( 67,  33),
            S( 33,  16),   S( 47,  29),   S( 52,  52),   S( 52,  19),   S( 55,  35),   S( 31,   2),   S( -9,   7),   S( 74, -10),

            /* queens: bucket 8 */
            S(-18, -36),   S(  0, -23),   S(-16, -41),   S( -3,  -9),   S(-15, -29),   S(  8,  -4),   S( -1, -11),   S(  1,   5),
            S(-20, -31),   S( -5, -14),   S(  3, -15),   S( -5, -11),   S(  9,  -3),   S( -4, -10),   S( -3,   3),   S( -1,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -16),   S( -9, -44),   S(  5,   3),   S(  9,  -4),   S( -8,  -9),   S(  3,   6),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S( -1, -12),   S( -1,   3),   S(  4,   0),   S( 12,  20),   S(  6,   3),
            S( -2, -11),   S(  7,  10),   S(  7,   2),   S( 12,  -7),   S(  7,  -9),   S( 12,  13),   S( 14,  13),   S(-10,  -9),
            S(  1, -15),   S(  4, -16),   S( 15,  14),   S(  3, -20),   S( 12,   8),   S( 26,  34),   S(  8,  -4),   S( -2,  -4),
            S(-17, -37),   S(  1, -11),   S( 12,   8),   S( 25,  37),   S( 12,  11),   S( 17,  39),   S(  4,   6),   S(  5,   1),
            S(  1,   0),   S(  3,  -7),   S( 13,   7),   S(  8,  -3),   S( 17,  18),   S( -3,  -5),   S(  3,  10),   S(-17, -28),

            /* queens: bucket 9 */
            S( 10, -10),   S(-18, -34),   S(-13, -32),   S( 13,  -8),   S( -6, -35),   S( -1,  -9),   S( -5,  -9),   S( -2, -14),
            S( -1,  -7),   S(-10, -20),   S(-10, -26),   S(  2, -15),   S(-21, -50),   S(-12, -29),   S(  6,  -3),   S(  1,  -9),
            S(-17, -44),   S(-13, -27),   S(  0,   0),   S(  0,   0),   S(  5,  -8),   S( 11,  -8),   S( -4,  -8),   S(  6,  -3),
            S(  2,  -6),   S(-11, -30),   S(  0,   0),   S(  0,   0),   S(  0,  -4),   S( 11,   3),   S( 11,  12),   S( -1,   2),
            S( -8, -28),   S(  1, -14),   S(  0,  -7),   S(-10,  -9),   S( -5, -28),   S( 12,  17),   S(  5,  -8),   S(  0, -15),
            S( 11,  10),   S( -2, -29),   S(  5, -10),   S( -4, -19),   S(  0, -10),   S(  6,   5),   S( -3, -12),   S( -2, -12),
            S(  8,   5),   S(  7,  -5),   S( -5,  -3),   S(  1,   9),   S( 22,  24),   S( 25,  29),   S(  7,  20),   S(  7, -11),
            S( 16, -11),   S( 25,  15),   S( -2,  -8),   S( 20,  12),   S( 20,  17),   S(  5,  13),   S(  1, -17),   S( 13,   3),

            /* queens: bucket 10 */
            S( 15,   8),   S( 12,   9),   S(  0,  -9),   S( -5, -26),   S( -9, -29),   S( -8, -16),   S( -4, -26),   S( -4, -14),
            S(  6,   3),   S(-14, -21),   S( -5, -23),   S(-18, -53),   S( -3,  -9),   S( 11,   0),   S(-10, -27),   S( -5,  -6),
            S( -2,   1),   S(  3,   4),   S( -1,  -3),   S( -7, -17),   S(  0,   0),   S(  0,   0),   S(  3,  -3),   S(-11, -21),
            S( -3,  -9),   S(  4,   5),   S(  4,   3),   S(  9,   2),   S(  0,   0),   S(  0,   0),   S( -5, -14),   S(  1, -17),
            S( 11,  15),   S( 15,   4),   S(  3,  -5),   S( 31,  33),   S(  0,   2),   S( -1,  -1),   S(  2, -11),   S( 11, -25),
            S( -6, -10),   S(  7,   7),   S( 23,  26),   S( 11,  13),   S( 15,  14),   S( 15,  22),   S( 16,   9),   S( -3, -23),
            S(  9,   6),   S( 19,  28),   S( 19,  26),   S( 20,  16),   S(  9,  16),   S( 25,  13),   S( 14,   8),   S(  5,  -5),
            S(-11, -30),   S(  3,   6),   S( 21,   5),   S( -6,  -2),   S( 14,  14),   S(  2,   2),   S( 13,   8),   S(  8, -10),

            /* queens: bucket 11 */
            S(-11,  -4),   S( -3,  -1),   S( -8,  -9),   S(-19, -19),   S( -4, -14),   S(-19, -32),   S( -6, -29),   S( -7, -14),
            S( -6,   0),   S(  1,   8),   S(-24, -11),   S( -6,   3),   S( 21,   1),   S( -9, -24),   S(  8,  -1),   S( -5, -12),
            S(  3,   7),   S(  6,   2),   S(-19,  13),   S( -2,   3),   S( -2, -19),   S(-22, -29),   S(  0,   0),   S(  0,   0),
            S(  0,   3),   S( -7,  11),   S( -2,  12),   S( -1,   4),   S(  1,  -8),   S( -3,   6),   S(  0,   0),   S(  0,   0),
            S(  1,  12),   S( 15,  15),   S( 17,  24),   S(  4,  24),   S( 42,  46),   S( 18,  27),   S(  8,   0),   S(-10, -28),
            S(  1,   4),   S(  1,   0),   S( -2,  11),   S( 12,  28),   S( 15,  19),   S(  1,   3),   S(  5, -10),   S(  6, -21),
            S(  3,   4),   S(  9,  12),   S( 16,  23),   S(  2,  13),   S( 18,  56),   S( 15,  11),   S(  5,   1),   S( 11,  -3),
            S(-17, -57),   S(  9,  12),   S( -7,  -7),   S(  3,  36),   S( 14,  29),   S( 11,   0),   S( -6,  -3),   S( 11,  -1),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  2,   2),   S(-14, -19),   S( -6,  -6),   S(-12, -21),   S( -2,  -4),   S( -3,  -3),
            S(  0,   0),   S(  0,   0),   S(  6,   3),   S( -9, -18),   S( -7,  -8),   S(-10, -21),   S( -9, -17),   S(  1,  -1),
            S( -6,  -9),   S(  5,   7),   S( -5,  -7),   S(-11, -35),   S( 16,  32),   S(  0,  15),   S( -2,  -7),   S(  8,   9),
            S( -9, -18),   S(  5,   3),   S(  8,  14),   S(  3,  12),   S(  1,   4),   S( -2,  10),   S( -3,  -2),   S( -3,  -8),
            S(-17, -29),   S(  3,   9),   S(  5,   2),   S(  6,   6),   S(  7,  30),   S( -5, -19),   S( -8, -16),   S( -1,   0),
            S(  1,  -6),   S( -4, -11),   S(  0, -13),   S(  4,   8),   S( -5,  -8),   S( -9,   0),   S(-11, -10),   S( -3,  -8),
            S( -9, -13),   S(  3,   5),   S( -6, -11),   S( 13,  11),   S( -1,   0),   S( -9, -15),   S(  0,  -1),   S( -7, -26),
            S(  6,  12),   S(  0,  -3),   S(  1,  -6),   S(  0,   3),   S( -6,  -7),   S(-14, -13),   S( -6,   9),   S( -9, -15),

            /* queens: bucket 13 */
            S(-22, -34),   S(-15, -28),   S(  0,   0),   S(  0,   0),   S(-17, -30),   S(-12, -34),   S(  0,  -2),   S( -4, -10),
            S(-16, -45),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(-16, -36),   S(-22, -43),   S(-12, -21),   S( -4,  -6),
            S(-21, -38),   S( -4, -14),   S( -4,  -5),   S( -2, -13),   S(-21, -40),   S(-10, -15),   S( -8,  -6),   S( -1,  -4),
            S( -8, -19),   S(-19, -31),   S(  0,  -8),   S( -5, -17),   S( 11,   6),   S( 18,  32),   S( -4, -15),   S( -8, -10),
            S(  5, -10),   S(  0, -24),   S( -7, -20),   S( 12,  24),   S( -6, -11),   S( -1, -15),   S( -2,  -5),   S(  2, -10),
            S( -2,  -4),   S(-14, -19),   S(  4,   3),   S( 10,  23),   S(  1,  -9),   S( -5,  -5),   S(-12, -22),   S(-10, -23),
            S(  0,   0),   S( -4, -10),   S( 11,  25),   S( -2,  -2),   S(  3,   2),   S(  8,   0),   S(-14, -25),   S( -7, -11),
            S( -8,  -6),   S( -3,  -8),   S( -6, -13),   S(  0,  -7),   S(  3,  -2),   S( -1,  -3),   S( -1,  -8),   S(-13, -20),

            /* queens: bucket 14 */
            S( -6, -15),   S(  0,  -9),   S( -9, -19),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S( -3,  -7),   S( -7, -22),
            S( -7, -23),   S(-26, -47),   S(-11, -25),   S( -3, -15),   S(  0,   0),   S(  0,   0),   S( -9, -24),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -6, -20),   S(-13, -25),   S( -3,  -4),   S(  2,   4),   S(-10, -14),   S(-17, -32),
            S( -9, -11),   S( -2,   0),   S(  0,   0),   S(-15, -21),   S( -7, -14),   S(-19, -27),   S( -2, -22),   S(  1,   1),
            S( -6, -12),   S( -5, -11),   S( -4, -15),   S(  5,   9),   S(  6,  18),   S(-10, -25),   S( -9,  -5),   S( -1,  -2),
            S( -6, -13),   S(  3,  -3),   S(-12, -20),   S(-13, -22),   S(  6,  10),   S(  1,   5),   S( -1,  -5),   S(-10, -11),
            S(-10, -16),   S( -2,  -8),   S(  0,   0),   S(  3,   6),   S(  2,   4),   S(  3,   5),   S( -8, -20),   S( -3,  -8),
            S(-11, -17),   S(  5,  -5),   S(-10, -14),   S( -4,  -9),   S(  3,   1),   S( -3,  -3),   S( -4,  -2),   S(  2,  -7),

            /* queens: bucket 15 */
            S(  1,   3),   S( -7, -18),   S(  4,   0),   S(-11, -18),   S(  4,   6),   S(-10, -10),   S(  0,   0),   S(  0,   0),
            S( -5,  -5),   S(  1,   6),   S(-12, -16),   S( -8, -16),   S(  0,  -6),   S(  2,   6),   S(  0,   0),   S(  0,   0),
            S( -1,  -1),   S(  1,   0),   S(-11,  -3),   S( -6,  -6),   S( -9, -21),   S(  5,   5),   S( -1,   2),   S( -1,  -4),
            S( -2,  -4),   S(-10, -14),   S( -3,  -5),   S(  1,   7),   S(  9,  27),   S(  8,  28),   S( -3,   5),   S( -4, -16),
            S(  1,   3),   S(  1,   1),   S( -4,  -8),   S(  0,  -1),   S( 11,  52),   S(  4,  20),   S(  3,  11),   S( -6, -16),
            S( -1,  -3),   S( -3,  -1),   S( -3,  -8),   S( -6,  -1),   S( -2,   5),   S( -9,  -8),   S(  2,  12),   S( -8,  -6),
            S( -5, -12),   S(  0,   0),   S( -5,   4),   S(  3,   3),   S( -7,  -9),   S(  1,   6),   S(  5,  10),   S( -5, -10),
            S( -8, -18),   S(-13, -31),   S( -2, -10),   S(  2,   2),   S(-14,  -3),   S( -3,  -1),   S(  1,  -1),   S( -3,   5),

            /* kings: bucket 0 */
            S(-11, -20),   S( 30, -10),   S( 16,  -4),   S(-27,  14),   S( -8,  14),   S( 31, -26),   S(  4,   1),   S( 10, -49),
            S(-18,  32),   S( -2,  -1),   S( -3,   6),   S(-45,  25),   S(-43,  43),   S(-15,  22),   S(-14,  37),   S( -4,  27),
            S( 13,   3),   S( 65, -29),   S(  0,  -1),   S(-20,   2),   S(-33,   5),   S(  0,  -5),   S(-31,  17),   S( 29, -29),
            S(-27, -26),   S(  5, -28),   S(  4, -25),   S(-23,   6),   S(-48,  33),   S(-48,  27),   S(-40,  39),   S(-16,  31),
            S(-50, -123),  S( -5, -45),   S( -4, -31),   S( 14, -25),   S(-48,  -5),   S(-29,   9),   S(-22,  14),   S(  2,  -9),
            S(-10, -120),  S(  1,  10),   S(-10, -52),   S(-13,  -7),   S( -2, -12),   S(-26,  21),   S( 15,  24),   S(-19,   6),
            S(  0,   0),   S(  0,   0),   S(  0, -49),   S(  4, -35),   S(-19,  -3),   S(-11, -14),   S(-27,   6),   S( -9,  -4),
            S(  0,   0),   S(  0,   0),   S(-12, -10),   S(  1, -10),   S(  9,  -2),   S( -5,  13),   S(  7,   3),   S(  9,   0),

            /* kings: bucket 1 */
            S(  7, -26),   S( 32, -23),   S( 15, -16),   S( 28,  -3),   S(  0,  -1),   S( 33, -20),   S(  7,   3),   S( 19, -23),
            S( 10,  -2),   S(  5,  10),   S( -2,  -7),   S(-45,  27),   S(-32,  22),   S(-13,  15),   S( -5,  18),   S(  3,   9),
            S( -8, -16),   S(  0, -14),   S(  6, -17),   S( 14, -19),   S(-34,   1),   S( 18, -19),   S( 23, -11),   S( 38, -13),
            S( -1,  -1),   S(  0,  -9),   S(  2,  -3),   S( -6,   6),   S(  5,  12),   S(-10,   1),   S( 31,  -6),   S(-19,  26),
            S(-18, -55),   S(-18, -43),   S(-11, -52),   S(-17, -41),   S( -3, -22),   S( -2, -30),   S( -8,  -3),   S( -6,  -4),
            S(-31,   0),   S(-103,   5),  S(-33,  28),   S(  2,  20),   S(-42,   7),   S(-26,  14),   S( 16,   4),   S( -6,  -8),
            S(-34, -51),   S(-23,   6),   S(  0,   0),   S(  0,   0),   S(-41,  15),   S(-54,  30),   S( -6,  29),   S( -3, -31),
            S(-29, -109),  S(-13, -14),   S(  0,   0),   S(  0,   0),   S( -8,   8),   S(-13,  16),   S( -2,  21),   S( -4, -47),

            /* kings: bucket 2 */
            S( 15, -58),   S(  9,  -4),   S( 17, -20),   S( 16, -10),   S(  0,   6),   S( 35, -23),   S( -2,  14),   S( 20, -27),
            S( 34, -36),   S(-17,  31),   S(-16,   7),   S(-17,   8),   S(-25,  15),   S(-12,   5),   S(  3,   2),   S(  2,   0),
            S(-31,  -5),   S(-19, -12),   S( -6, -13),   S(-11, -17),   S( -8,  -5),   S(  6, -19),   S( 28, -18),   S( 26, -17),
            S( 15,  11),   S(-20,  16),   S(  6,   0),   S(-30,  14),   S( 27,  -5),   S(-16,  -9),   S( 31, -27),   S( 31, -10),
            S( -3, -12),   S( 15, -16),   S( 26, -40),   S(  3, -27),   S( 30, -48),   S(-23, -40),   S( 20, -48),   S(  9, -46),
            S(  3,   8),   S( -9,  -7),   S(-36,  -2),   S(-39, -10),   S(  3,   1),   S(-11,  24),   S(-83,  10),   S(-17, -20),
            S( -8, -11),   S( -8,  21),   S(-75,  12),   S(-17,  11),   S(  0,   0),   S(  0,   0),   S(-13,  18),   S(-37, -34),
            S( -7, -39),   S(-19, -28),   S(-30, -33),   S( -6,  10),   S(  0,   0),   S(  0,   0),   S(-10, -13),   S(-33, -119),

            /* kings: bucket 3 */
            S( -4, -53),   S( 17, -10),   S( 28, -24),   S( -5,  -7),   S( -1, -13),   S( 35, -25),   S(  2,  13),   S(  6, -28),
            S(  2,  17),   S(-21,  39),   S(-19,   6),   S(-38,  18),   S(-54,  32),   S( -1,   0),   S( -7,  19),   S(  1,  13),
            S( 16, -26),   S(  1,  -3),   S( -3,  -9),   S(-35,   1),   S(-13,   8),   S( 22, -19),   S( 49, -19),   S( 54, -17),
            S(-18,  30),   S(-91,  45),   S(-55,  16),   S(-50,  15),   S(-35,  11),   S(-15, -21),   S(-37,  -3),   S(-33, -15),
            S(-13,   7),   S(-11,  -5),   S(-34, -12),   S(-24, -15),   S( 33, -46),   S( 50, -65),   S( 33, -69),   S( 10, -81),
            S(-10, -14),   S( 18,   6),   S( 21, -11),   S(  0, -22),   S( 48, -34),   S( 56, -48),   S( 71, -20),   S( 52, -115),
            S(-20, -10),   S( 25,   9),   S( 15, -14),   S( 30, -22),   S( 31, -29),   S( 26, -54),   S(  0,   0),   S(  0,   0),
            S( -3, -11),   S(  6,   9),   S( -2,  18),   S( 13, -11),   S( 10, -72),   S( -3,  10),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-56,   6),   S(  7,  35),   S( 10,  21),   S( 12,   0),   S( -7,   7),   S(  9, -11),   S(  5,   7),   S( 21, -36),
            S(-35,  22),   S( 24,  19),   S( -6,  18),   S( -7,   3),   S( 31,  -2),   S( 23,  -4),   S( 52, -14),   S( 14,  -3),
            S(  0,  26),   S( 11, -13),   S( 19,  -5),   S( -9,   3),   S(-20,   9),   S( 20, -21),   S(-38,   8),   S( 14, -12),
            S(  0, -22),   S(-12,   9),   S(  5,  16),   S(  7,   4),   S(-19,  10),   S(-13,  18),   S( 16,  10),   S( 10,   7),
            S(  0,   0),   S(  0,   0),   S( -1,   1),   S(-27,  13),   S(-36,  14),   S(-27, -14),   S(-20,   2),   S( -3,  -1),
            S(  0,   0),   S(  0,   0),   S(  6, -16),   S( -3,  25),   S(-12,  27),   S(-29, -10),   S(  4, -14),   S( -1,  17),
            S( -3, -20),   S( -4,  -7),   S( -4, -22),   S(  0,  22),   S( -5,  25),   S(-29,  -7),   S(-11,  21),   S(  4,  -4),
            S( -5, -22),   S(  3, -12),   S(-10, -20),   S( -8,   3),   S(  6,  11),   S( -7, -11),   S( -5,   1),   S(  4,  11),

            /* kings: bucket 5 */
            S( 33,  -5),   S(-10,  12),   S(-33,  23),   S(-42,  28),   S(-18,  26),   S(  0,  12),   S( 37,  -3),   S( 29,  -9),
            S(  0,   0),   S( 16,  10),   S( 28,  -5),   S( 24,  -6),   S( 18,  -4),   S( 39, -12),   S( 29,   4),   S( 47, -17),
            S(-12,   9),   S( -6,  -8),   S(-12,  -6),   S( -3,  -7),   S(  8,  -3),   S(-40,   1),   S( -3,   2),   S( 18,  -4),
            S( -3, -12),   S(  0,  -7),   S(  8,  -5),   S(  6,  17),   S(  4,  20),   S(  6,   4),   S( 14,   6),   S(  7,   5),
            S( -5, -29),   S(-31, -44),   S(  0,   0),   S(  0,   0),   S( -8,  -4),   S(-22, -13),   S(  2, -13),   S( -9,   6),
            S( -7, -39),   S(-25, -28),   S(  0,   0),   S(  0,   0),   S(-22,  37),   S(-57,  13),   S(-19,  -3),   S( -6,  -4),
            S(-16, -33),   S(-32,  21),   S(  1,  12),   S( -1, -17),   S(-28,  29),   S(-41,  19),   S( -1,  10),   S(  9,  17),
            S(-10, -101),  S( -8,  12),   S(-10, -26),   S( -3, -36),   S(-10, -18),   S( -6,   6),   S( -3, -16),   S(  0,   5),

            /* kings: bucket 6 */
            S( 38, -36),   S( 30, -14),   S( -2,   2),   S(-20,  22),   S( -7,  19),   S(-21,  20),   S(  1,  20),   S(  9,   2),
            S( 48, -28),   S( 12,  17),   S( 16,  -7),   S( 24,  -9),   S( 24,  -5),   S( -9,  12),   S( 16,   1),   S(  5,   2),
            S( 18, -19),   S(-24,   3),   S(-15,  -9),   S( -3,  -6),   S( 16, -13),   S(-45,   6),   S( 11,  -2),   S(-17,  13),
            S( 12,   6),   S( 27,  -5),   S( 17, -13),   S( 24,   5),   S( 58,   0),   S(-27,   5),   S( -6,   7),   S(  7,  -1),
            S(  8, -20),   S( 16, -29),   S(-22, -12),   S(  1, -18),   S(  0,   0),   S(  0,   0),   S(-46, -19),   S(-41, -17),
            S(-16,   0),   S(  2,   0),   S(-31,  -2),   S( -9, -22),   S(  0,   0),   S(  0,   0),   S(-27, -13),   S(-31, -21),
            S( -1,  -9),   S(-10,   7),   S(-40,  10),   S(-16,  -2),   S(  3,   3),   S(-10, -32),   S(-29, -12),   S( -9, -38),
            S( -1,  -6),   S(  1,  -7),   S( -4,   9),   S(-15, -28),   S( -8, -37),   S( -5, -25),   S( -7,  -2),   S( -1, -59),

            /* kings: bucket 7 */
            S( 31, -34),   S( -6,  -4),   S(-27,  -3),   S(-13,   9),   S(-26,  11),   S(-41,  35),   S(-27,  33),   S(-37,  21),
            S( 13,  -2),   S( 23, -21),   S( -3,  -8),   S(-32,   7),   S(-12,   6),   S(-37,  23),   S(  3,  -3),   S( -4,  14),
            S( 29, -29),   S(-16,  -8),   S(-31,  -2),   S(-34,   1),   S(-43,   7),   S(-33,  14),   S( 13,  -3),   S(-46,  21),
            S(-23,  17),   S(  8,   8),   S( -4,  -2),   S( 35,  -7),   S( 35, -11),   S( 47, -26),   S( 15,  -9),   S( 20, -11),
            S(-15,  15),   S( -4,   0),   S(  4, -26),   S(  8, -18),   S( 17, -27),   S(  9, -21),   S(  0,   0),   S(  0,   0),
            S(-10, -32),   S(  0,  -8),   S( 15, -11),   S( 12,  -6),   S( 26, -11),   S( 17, -11),   S(  0,   0),   S(  0,   0),
            S( 14,  17),   S( -2, -20),   S(  2,   5),   S(-13, -12),   S(  8, -18),   S( -6, -28),   S(  4, -16),   S(-12,  12),
            S(  7,   7),   S( -7,  -9),   S( 11,  20),   S( -3,  -5),   S(  8,  16),   S(-17, -49),   S(  9, -11),   S(-10, -57),

            /* kings: bucket 8 */
            S( 16, 116),   S( -5,  84),   S( 41,  37),   S( -2,  -1),   S(-14,  12),   S(-15,  -4),   S( 31, -15),   S(-14, -19),
            S( 30,  69),   S( 23,  13),   S( 48,  59),   S( 83,  -3),   S( 17,  25),   S(  6,  -6),   S( -4,  11),   S(  2,  27),
            S(  0,   0),   S(  0,   0),   S( 29,  63),   S( 40,   6),   S( 19,   5),   S( -9,  -6),   S( -3,  14),   S(  9, -16),
            S(  0,   0),   S(  0,   0),   S(  4,  71),   S( -5,   2),   S(-15,  34),   S( -6,  19),   S( 14,  10),   S(  9,  16),
            S( -3, -26),   S( -1,  26),   S(  3,  11),   S(-14,  25),   S(-16,  -4),   S(  5, -14),   S(  0,  12),   S(-14, -26),
            S(  5,  14),   S( -1, -14),   S( -3, -11),   S( -7,   3),   S(-13,   0),   S(-11,  -1),   S( -9,  -2),   S(  8,  -7),
            S( -5, -14),   S( -8, -12),   S(  4,   8),   S( -1, -10),   S( -2, -34),   S(-10,   7),   S( -2,  -1),   S(  5, -46),
            S( -7,  -9),   S(-12, -26),   S( -2, -12),   S( -6, -22),   S(  7,   8),   S( -5,   1),   S(  1,  -4),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  7,  25),   S(-12,  34),   S(-18,  57),   S( 19,   8),   S(-15,  32),   S(-26,  29),   S( 40,   4),   S( 21,  13),
            S(-18,  33),   S( 36,  23),   S(  6,   0),   S( 50,   2),   S( 60,  16),   S( 23,   8),   S( -5,  29),   S(-16,  13),
            S( -6,  12),   S( 22,  14),   S(  0,   0),   S(  0,   0),   S( 45,  17),   S( -3,   5),   S(  7,   1),   S(-19,  22),
            S( -2, -29),   S( 12, -22),   S(  0,   0),   S(  0,   0),   S(  8,  32),   S( 13,   2),   S(-12,  11),   S(-16,  30),
            S(  4, -20),   S( 12,  -3),   S(  4,  18),   S(  1,  11),   S(-13,  15),   S(-21,  17),   S(-10,  14),   S( -1, -14),
            S(  5,   2),   S(  1,  -6),   S(  6,  -8),   S(-11, -21),   S(-11,  10),   S( -1,  10),   S(-33,   2),   S(  5,  31),
            S(  2,  -6),   S( -4, -20),   S( -2,  -7),   S(  2, -32),   S( 14, -27),   S( 13,  16),   S(-17,  -8),   S(  4,   4),
            S(  6,   5),   S( -2, -22),   S( 10, -23),   S( -4, -23),   S( -1, -19),   S(  3,   8),   S( -6,  13),   S(  9,  -2),

            /* kings: bucket 10 */
            S( 34,  -3),   S(  2,  -8),   S(  6,   9),   S(  8,  22),   S(-11,  18),   S(-91,  50),   S(-31,  46),   S(-84,  81),
            S(  4,  -1),   S( 64,  -1),   S( 25,  -5),   S( 34,   8),   S( 60,  10),   S( 46,   6),   S( 11,  28),   S(-85,  49),
            S( 16,   6),   S( 28,  -1),   S( 28, -11),   S( 15,  11),   S(  0,   0),   S(  0,   0),   S( -9,  23),   S(-59,  29),
            S( 16,   6),   S( 44, -28),   S( 36, -31),   S( 31,   1),   S(  0,   0),   S(  0,   0),   S( -1,  14),   S(  4,   3),
            S(  4,   6),   S( 27,   5),   S( 30, -20),   S(  9, -31),   S(  4, -17),   S(  7,  26),   S(  9,   9),   S(-10,  16),
            S(  3,  14),   S(  2,  -6),   S( -4,   8),   S( 10,  -8),   S(  7,  -3),   S(-17,  -6),   S(-13,   6),   S( -1,  -8),
            S(  0, -43),   S( -4, -15),   S(  9,  -9),   S( 14,   2),   S( 12,  -3),   S(-10, -16),   S(  4, -27),   S(  4,   3),
            S(  4,   6),   S( 11,  -9),   S( -2, -17),   S(  1,   1),   S(  5, -16),   S(  0, -29),   S( -5,  -7),   S(  9,   3),

            /* kings: bucket 11 */
            S( -6, -19),   S(  9,   7),   S(  7,  -9),   S( -7,  14),   S( -7,   7),   S(-67,  57),   S(-71,  80),   S(-123, 148),
            S( -2, -27),   S( 22,   3),   S(-11, -16),   S( 15,  20),   S( 83,   1),   S( 58,  41),   S( 10,  21),   S( 25,  40),
            S(  3, -51),   S( -2,  18),   S( -1, -11),   S( 23,  10),   S( 63,   1),   S( 26,  61),   S(  0,   0),   S(  0,   0),
            S(  0,  19),   S( 18,  13),   S( -3,   4),   S( 12,  13),   S( 32, -10),   S( 23,  20),   S(  0,   0),   S(  0,   0),
            S(  1,  33),   S(  2,  -5),   S(  9,  -7),   S( 15, -19),   S( 17,   2),   S( -1,  -2),   S(  9,   9),   S(  6,   2),
            S( 11,  10),   S(  1, -15),   S( 17, -12),   S(  0,   4),   S( -5,  -7),   S(  3, -17),   S( -4,  -8),   S(-11,  -3),
            S(  6,  12),   S(  8,  -6),   S( 18,  23),   S(  1, -26),   S( 18, -18),   S(  4,   2),   S(-11, -14),   S( -8, -14),
            S(  4,   7),   S(  5,   1),   S(-11, -22),   S(  5,  -7),   S( -4, -21),   S( -8, -19),   S(  0, -20),   S(  5,  12),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 18,  55),   S(  7,  -8),   S(  0,  -3),   S(  7,  15),   S(  7,  -1),   S(-20,   7),
            S(  0,   0),   S(  0,   0),   S( 46, 106),   S( 29,  14),   S( 22,  42),   S( 13,  -1),   S( 22,  -6),   S(-19,   2),
            S( -1,   8),   S(  3,  12),   S( 23,  68),   S( 40,  21),   S(  8,  -9),   S( 11,   3),   S(  0, -10),   S(-10,  -2),
            S( -2,  10),   S( 10,  31),   S( -1,  17),   S(  5,  -4),   S( -8,   1),   S( -1,  20),   S( -3,  10),   S(  1,   8),
            S( 10,  18),   S(  6,  23),   S( 10,  18),   S( -2,  40),   S( -3,  36),   S(  0,   4),   S( -9,  15),   S(-12, -12),
            S(  6,   5),   S( 10,  15),   S( -2,  -1),   S( -9, -15),   S( -1,   4),   S( -7,  18),   S( -9, -15),   S(  6,  -2),
            S(  3,   8),   S( -7, -13),   S( -1,   5),   S( -1,   1),   S( -5,  -9),   S(  4,  10),   S(  8,  43),   S( -1, -29),
            S( -2,   2),   S(  5,   3),   S( -3,   7),   S(  0,   3),   S( -1,  -6),   S(  3,   8),   S(-11, -23),   S( -3, -22),

            /* kings: bucket 13 */
            S( -1,  51),   S(  7,  33),   S(  0,   0),   S(  0,   0),   S( 43,  14),   S( 14, -11),   S( -4,  -5),   S(-18,  25),
            S(  3,  21),   S( -1,   0),   S(  0,   0),   S(  0,   0),   S( 47,   5),   S( 27,  -7),   S(-21,   7),   S(-15,   5),
            S( -3,   2),   S( 19,  22),   S(  2,  -7),   S( 14,  40),   S( 50,  11),   S( 23,  -6),   S(  2,   6),   S( 12, -10),
            S(-10,  -5),   S( 15,  -3),   S(  1,  21),   S( -5,  14),   S( -3,  15),   S(  4, -11),   S(  4,  22),   S(-16, -26),
            S(  6,  12),   S( -1,   7),   S(  5,  45),   S( -4,  24),   S( -9,  10),   S(  5,  19),   S(-11,   1),   S(  7,  10),
            S(  4,   0),   S( -6,  17),   S( -2,  17),   S( -4,  -1),   S(-12, -15),   S( -5,   9),   S( -9,  20),   S(  1,   1),
            S(  9,  11),   S( -9, -22),   S(-11, -43),   S(  3,  19),   S(-10, -12),   S(-10,  15),   S(-14, -25),   S(  6,  13),
            S(  1,  -2),   S(  5,  -4),   S(  4,  20),   S(  3,   5),   S(  0,  17),   S(-11, -16),   S( -3,   8),   S(  8,  14),

            /* kings: bucket 14 */
            S( 18,  33),   S(  0,  -8),   S( 11, -44),   S( 16,   0),   S(  0,   0),   S(  0,   0),   S(  6,  72),   S(-40,  36),
            S(-10, -11),   S( 18,  -8),   S( 48, -34),   S( 42,   9),   S(  0,   0),   S(  0,   0),   S( 13,  33),   S(-44,   6),
            S(  4,   4),   S( 16,  -6),   S( 34, -31),   S( 40,   4),   S( 10,  -3),   S( 14,  37),   S( 26,  56),   S(-28,   2),
            S(  7,  -6),   S(  8, -10),   S( -1, -10),   S( 12,  -2),   S(-19,  -1),   S( 15,  56),   S(  4,  23),   S(  6,  -2),
            S(  7,  19),   S(  8,   0),   S( -9,   4),   S(-18,  11),   S(  1,  29),   S(  5,  55),   S(  2,  39),   S(  5,  13),
            S( -6,  -6),   S(  1,   5),   S( -2,  -2),   S( -1,  10),   S( -6, -20),   S( -6,  -2),   S(-16,  -5),   S( -1,   8),
            S(  5,   8),   S(-10, -13),   S( 10,  -4),   S( 16,   3),   S(  3,  -4),   S( -7,  18),   S(-26, -21),   S(  8,  17),
            S(  1,  12),   S(  5,  -9),   S(  9,   2),   S( -4,  -6),   S(  7,  -9),   S( -3,  -6),   S(-13, -26),   S(  0,  -8),

            /* kings: bucket 15 */
            S( 11,  31),   S(  5,  -2),   S( 11,  -7),   S( -8,  -2),   S(-10, -12),   S( -1,  56),   S(  0,   0),   S(  0,   0),
            S( -4, -22),   S(  7, -13),   S( -7, -14),   S( 20,  47),   S( 40,  -2),   S( 62, 108),   S(  0,   0),   S(  0,   0),
            S(-10, -23),   S( 16,  -9),   S(  7, -14),   S( -3,  12),   S( 10,  -5),   S( 26,  71),   S(  8,  43),   S(-14,  -1),
            S( -1, -12),   S(  3,  15),   S(  3,  15),   S(-12, -29),   S(-12,  -2),   S( 21,  46),   S( 17,  46),   S( -3, -15),
            S( 10,   8),   S( -8,  24),   S(  0,  -3),   S( -6, -37),   S( -3,   9),   S(  2,  35),   S(  5,   7),   S(  4,   3),
            S(  5,  27),   S(-15,  -4),   S(  8,  16),   S(  8,  17),   S( -9, -24),   S( -2,   7),   S(  1,   7),   S(  5,  18),
            S(  8,  12),   S( -3,  25),   S( -2, -12),   S(  3,   6),   S(  9,   7),   S(  9,  13),   S( -5,  -3),   S(  2,   1),
            S( -2,  -8),   S(  4,   1),   S( -2, -10),   S(  4,   4),   S(  5,   5),   S( 10,  13),   S(  1,  -7),   S(  3,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-28,  65),   S(-36, -60),   S( -4,  28),   S( 11,  88),   S( 22, 114),   S( 27, 148),   S( 38, 151),   S( 49, 156),
            S( 66, 139),

            /* bishop mobility */
            S(-60,  90),   S(-37, -44),   S(  1,   8),   S(  8,  76),   S( 21, 106),   S( 30, 128),   S( 33, 147),   S( 40, 153),
            S( 42, 159),   S( 52, 156),   S( 59, 153),   S( 83, 133),   S(100, 129),   S( 70, 126),

            /* rook mobility */
            S(-63,   7),   S(-145, 118),  S(-18,  34),   S(-11, 106),   S(-10, 137),   S( -9, 160),   S( -7, 177),   S(  0, 183),
            S(  8, 185),   S( 14, 195),   S( 19, 200),   S( 28, 201),   S( 42, 202),   S( 54, 199),   S( 96, 173),

            /* queen mobility */
            S(113, 165),   S( 37, 310),   S( 26, 261),   S( 10, 213),   S( 49,  67),   S( 57,  46),   S( 58, 146),   S( 57, 210),
            S( 55, 263),   S( 55, 290),   S( 55, 318),   S( 59, 332),   S( 60, 351),   S( 64, 356),   S( 64, 367),   S( 65, 372),
            S( 66, 374),   S( 65, 377),   S( 70, 369),   S( 77, 358),   S( 88, 342),   S(121, 304),   S(132, 289),   S(152, 257),
            S(186, 230),   S(192, 205),   S(139, 202),   S(114, 146),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,  14),   S(-20,  48),   S(-30,  46),   S(-39,  61),   S(  7,  14),   S(-15,  18),   S(-10,  65),   S( 22,  30),
            S( 15,  34),   S( -3,  47),   S(-19,  47),   S(-22,  40),   S( -6,  38),   S(-32,  46),   S(-37,  63),   S( 29,  30),
            S( 21,  70),   S( 11,  74),   S(  5,  56),   S( 20,  47),   S( -5,  53),   S(-30,  69),   S(-40, 103),   S( -9,  80),
            S( 30, 109),   S( 40, 121),   S( 23,  80),   S( 11,  61),   S(  7,  65),   S( -2,  91),   S(-48, 128),   S(-76, 153),
            S( 23, 153),   S( 52, 185),   S( 61, 133),   S( 33, 115),   S(-54, 107),   S( 20, 110),   S(-57, 173),   S(-86, 172),
            S( 93, 233),   S( 79, 271),   S(127, 243),   S(127, 253),   S(134, 263),   S(152, 243),   S(128, 254),   S(134, 264),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,   4),   S( -7, -26),   S( -2,  -9),   S( -3,   5),   S( 13,  15),   S(-14, -40),   S(-22,   9),   S( -6, -46),
            S(-17,  17),   S( 19, -17),   S(  0,  27),   S(  9,  26),   S( 29,  -7),   S( -4,  15),   S( 24, -15),   S( -5,  -7),
            S(-15,  16),   S( 17,   6),   S(  2,  44),   S( 16,  54),   S( 23,  30),   S( 33,  18),   S( 27,   2),   S(  1,  13),
            S( 15,  35),   S( 14,  53),   S( 42,  91),   S( 13, 102),   S( 68,  68),   S( 70,  56),   S( 17,  62),   S( 24,  22),
            S( 49,  95),   S( 88, 118),   S(102, 140),   S(139, 166),   S(136, 135),   S(134, 149),   S(129, 127),   S( 50,  62),
            S( 72, 195),   S(117, 279),   S(102, 222),   S( 96, 198),   S( 66, 152),   S( 48, 141),   S( 41, 143),   S( 16,  78),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  21),   S( 17,  22),   S( 32,  35),   S( 32,  25),   S( 20,  21),   S( 27,  22),   S(  6,  11),   S( 42,  -3),
            S( -4,  21),   S( 17,  35),   S( 11,  36),   S(  9,  41),   S( 24,  12),   S(  9,  22),   S( 33,  19),   S(  0,  12),
            S(  0,  22),   S( 29,  49),   S( 54,  57),   S( 39,  60),   S( 44,  55),   S( 71,  19),   S( 30,  36),   S( 20,   6),
            S( 57,  73),   S(103,  59),   S(123, 124),   S(146, 129),   S(138, 120),   S( 79, 132),   S( 71,  60),   S( 75,  10),
            S( 43, 126),   S( 89, 145),   S(153, 212),   S(104, 253),   S(132, 266),   S( 82, 242),   S(158, 210),   S(-53, 172),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26,  32),   S( 11,  18),   S( 11,  33),   S(-13,  62),   S( 67,  22),   S( 21,   9),   S( -1,   2),   S( 30,  12),
            S(  2,  14),   S(  7,   8),   S( 17,  18),   S( 14,  30),   S(  9,  18),   S(  0,   9),   S(  7,   6),   S( 28,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -14),   S( -7,  -8),   S(-17, -18),   S(-14, -30),   S( -9, -18),   S(  0,  -9),   S( -7,  -6),   S(-28,   4),
            S(-26, -32),   S(-11, -18),   S(-11, -33),   S( 13, -62),   S(-67, -22),   S(-21,  -9),   S(  1,  -2),   S(-30, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -38),   S(-14, -42),   S(-16, -48),   S(-62, -35),   S(-27, -45),   S(-29, -47),   S( -8, -49),   S(-25, -61),
            S(-26, -22),   S(-20, -30),   S(-33, -13),   S( -9, -34),   S(-42, -34),   S(-25, -28),   S(-39, -20),   S(-13, -42),
            S(-20, -19),   S( -8, -36),   S(-27, -12),   S(-32, -25),   S(-21, -43),   S(-25, -21),   S(-10, -23),   S(-41, -31),
            S( -9, -32),   S( 18, -46),   S( 13, -19),   S(  9, -30),   S( 10, -30),   S( 57, -44),   S( 41, -46),   S(-12, -54),
            S( 11, -48),   S( 39, -73),   S( 44, -27),   S( 59, -32),   S( 73, -48),   S( 79, -36),   S(133, -92),   S( 32, -80),
            S( 93, -98),   S(123, -108),  S( 86, -47),   S( 70, -30),   S( 62, -28),   S(113, -41),   S( 98, -45),   S( 44, -83),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-13, -24),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  4,   2),        // attacks to squares 1 from king
            S(  8,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 66, -64),        // king-side castling available
            S( 17,  59),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 29, -88),   S( 34, -74),   S( 27, -85),   S( 26, -74),   S( 17, -65),   S( 13, -59),   S(  3, -50),   S( -5, -44),
            S(  4, -43),   S( 16, -40),   S( 46, -45),   S( 42, -41),   S( 95, -50),

            /* orthogonal lines */
            S(-64, -139),  S(-91, -120),  S(-111, -99),  S(-125, -93),  S(-132, -91),  S(-139, -92),  S(-138, -98),  S(-134, -103),
            S(-149, -92),  S(-159, -90),  S(-165, -96),  S(-136, -122), S(-94, -140),  S(-37, -157),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 28, 231),

            /* passed pawn can advance */
            S(-10,  34),   S( -3,  61),   S( 15, 104),   S( 82, 170),

            /* blocked passed pawn */
            S(  0,   0),   S( 52, -26),   S( 29,  -5),   S( 33,  32),   S( 32,  62),   S( 23,  40),   S( 68,  82),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 48, -50),   S( 39,  17),   S( 21,  28),   S( 20,  60),   S( 32,  98),   S(136, 130),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-13, -16),   S( -6, -31),   S(  7, -26),   S(-18,  -2),   S(-22,  30),   S(126,  18),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 29, -16),   S( 29, -18),   S( 12,  -5),   S( 12, -38),   S( -6, -107),  S(-28, -193),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 24,  51),   S( 50,  25),   S(106,  43),   S( 35,  25),   S(177, 113),   S(110, 127),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 16,  57),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-44, 114),

            /* bad bishop pawn */
            S( -8, -17),

            /* rook on open file */
            S( 30,   6),

            /* rook on half-open file */
            S(  8,  44),

            /* pawn shields minor piece */
            S( 12,  13),

            /* bishop on long diagonal */
            S( 28,  52),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 21,  32),   S( 23,   2),   S( 34,  21),   S( 27,  -2),   S( 34, -20),

            /* pawn threats */
            S(  0,   0),   S( 67, 106),   S( 51, 118),   S( 73,  89),   S( 60,  41),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  65),   S( 52,  49),   S( 76,  43),   S( 49,  69),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 27,  53),   S( 27,  49),   S(-15,  41),   S( 65,  72),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 21,  10),   S( 21,  33),   S( 36,  13),   S(  9,  29),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  15),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
