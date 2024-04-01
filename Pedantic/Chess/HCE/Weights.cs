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

        // Solution sample size: 6000006, generated on Sun, 31 Mar 2024 18:26:13 GMT
        // Solution K: 0.003850, error: 0.083843, accuracy: 0.5069
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 77, 230),   S(384, 675),   S(415, 669),   S(536, 1084),  S(1368, 1820), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(104, -132),  S(151, -101),  S( 40, -46),   S(-32,  18),   S(-42,  31),   S(-23,  -1),   S(-42,  -5),   S(-39, -11),
            S(123, -137),  S(106, -107),  S( 14, -70),   S( -6, -52),   S(-18, -11),   S(-19, -25),   S(-28, -28),   S(-28, -40),
            S(114, -107),  S( 66, -58),   S( 22, -72),   S( 17, -68),   S( -1, -72),   S( 13, -55),   S( -5, -58),   S(  1, -58),
            S( 76, -48),   S( 67, -59),   S( 42, -68),   S( 27, -88),   S( -9, -49),   S(-23, -46),   S( -9, -53),   S( -4, -29),
            S( 60,  48),   S( 34,   4),   S( 31, -22),   S( 71, -85),   S( 48, -63),   S( -3, -48),   S(-15, -24),   S(-37,  62),
            S( 81,  66),   S( 42,  65),   S(  4,  44),   S(-19, -22),   S(-52,  14),   S( 16,  24),   S( 36,   6),   S(  3,  69),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 35, -33),   S( 38, -39),   S( 49, -27),   S(  3,  24),   S( -6, -15),   S( 10, -14),   S(-29,  -7),   S(-33,  23),
            S( 32, -40),   S( 25, -48),   S( 14, -45),   S(  0, -35),   S(-10, -22),   S( -5, -28),   S(-23, -19),   S(-36, -12),
            S( 29, -39),   S( 19, -27),   S( 20, -54),   S( 15, -51),   S(-14, -32),   S( 21, -52),   S(  0, -39),   S(  8, -32),
            S( 46, -11),   S( 31, -50),   S( 31, -58),   S( 21, -54),   S( -4, -25),   S( 14, -43),   S(-11, -33),   S( -2,  -1),
            S( 20,  53),   S(-10,  -8),   S( 21, -44),   S( 23, -52),   S( 47, -40),   S( -7, -20),   S(-24,  18),   S(-26,  67),
            S( 99,  60),   S( 14,  10),   S(-42,  -7),   S(-23,  46),   S(-20,  34),   S(-51,  17),   S(-45,  41),   S(-39,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9,  -2),   S(-18,   0),   S( -7,   1),   S( -3,   5),   S( 13,  -7),   S( 38, -20),   S( 12, -47),   S( -6, -13),
            S( -1, -25),   S(-23, -19),   S(-19, -35),   S(-15, -28),   S( 10, -33),   S( 17, -34),   S(  0, -42),   S(-16, -28),
            S(  0, -28),   S(-15, -26),   S( -5, -55),   S(  3, -60),   S(  3, -33),   S( 27, -46),   S( -1, -36),   S( 12, -33),
            S( -5,  -8),   S( -4, -45),   S(-10, -51),   S(  1, -55),   S( 19, -51),   S( 10, -34),   S(  3, -20),   S(  4,  -5),
            S(  1,  34),   S(-46,  -6),   S(-54, -37),   S(-38, -50),   S(  8,   6),   S( -3,  -6),   S(-42,  23),   S(-23,  78),
            S(-55,  86),   S(-77,  50),   S(-96, -12),   S(-65, -23),   S(-14, -21),   S(-23,  18),   S(-24,  18),   S(-26,  73),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -15),   S(-25,  -6),   S(-21,  -7),   S( 10, -39),   S(  2,  -4),   S( 48, -24),   S( 95, -78),   S( 70, -83),
            S( -6, -43),   S(-22, -35),   S(-21, -42),   S(-13, -33),   S( -7, -28),   S( 20, -44),   S( 69, -81),   S( 64, -79),
            S( -2, -49),   S(  1, -58),   S(  2, -73),   S(  9, -71),   S(  2, -56),   S( 33, -62),   S( 39, -69),   S( 83, -79),
            S(  2, -29),   S( 11, -80),   S(  5, -79),   S( 13, -76),   S( 33, -79),   S( 23, -64),   S( 33, -52),   S( 72, -32),
            S( 24,  10),   S(  5, -46),   S(  3, -71),   S( 11, -78),   S( 99, -72),   S( 80, -46),   S( 63,   0),   S( 56,  61),
            S(-24,  96),   S(-19,  14),   S( -4, -57),   S(  5, -81),   S( 89, -92),   S( 78, -47),   S( 75, -19),   S( 71,  52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-119,  36),  S(  1, -19),   S(-58,  23),   S( -1,  21),   S(-38, -18),   S(-61,  33),   S(-66,  15),   S(-36,   1),
            S(-29,  21),   S( 40, -19),   S( 15, -28),   S( 45, -34),   S( -5, -20),   S(-52, -16),   S( 26, -58),   S( 16, -31),
            S( 36, -14),   S( 50, -14),   S(-30,   3),   S( -7, -32),   S(-36, -36),   S( -3, -47),   S(-18, -51),   S( 16, -37),
            S(-25,  44),   S(-34,  45),   S( 24,  11),   S(  0,  -3),   S( 28, -53),   S(-27, -28),   S( 16, -51),   S( 79, -46),
            S(-14, 100),   S(-27,  96),   S(-16,  27),   S(-15,  -7),   S(-19,  26),   S(  5, -12),   S(-37, -38),   S( 61,  22),
            S( 84,  76),   S( 64, 132),   S(  7,  28),   S( 14,  -4),   S(-16, -55),   S(  7,   3),   S(  0,  12),   S(-31,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  53),   S(-79,  47),   S( -6,  14),   S( 22,  18),   S(-21,  41),   S(-35,  25),   S(-74,  25),   S(-14,  27),
            S(-45,  18),   S(-77,  24),   S( 14,  -1),   S(-18,  28),   S(  7,  -3),   S(-26, -14),   S(-22, -14),   S(-27,  10),
            S(-36,  31),   S(-62,  34),   S( 45, -22),   S( 10, -23),   S( 14,  -9),   S(-22, -13),   S(  3, -15),   S( 24, -14),
            S(-64,  65),   S(-58,  33),   S( -3,  10),   S( -6,  27),   S( -6,   5),   S(-48,  -4),   S(  8, -15),   S( 15,   9),
            S( 29,  59),   S( 45,  35),   S( 45,  39),   S(  6,  33),   S( 18,  15),   S( 33,  -7),   S(-11,   5),   S( 49,  13),
            S( 56,  75),   S( 32,  27),   S( 57,  -4),   S( 28, -15),   S( 21, -23),   S( 12, -25),   S(-11,   9),   S( 30,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  28),   S(-50,  22),   S(-40,  23),   S(-40,  19),   S( 48, -24),   S(-30,  12),   S(-70,  11),   S(-72,  30),
            S(-34,   0),   S(-10, -21),   S(-15, -32),   S(-19,   6),   S( 35, -17),   S( 23, -20),   S(-39,  -5),   S(-66,   9),
            S(-21,  -2),   S(-15,  -6),   S(-39, -12),   S(-50,   2),   S( 17, -10),   S( 64, -38),   S( -5, -17),   S(-15,   4),
            S(-25,  19),   S(-72,  12),   S( 16, -31),   S(-31,   3),   S(  6,   4),   S( 28, -14),   S( 22,  -9),   S( 47,   4),
            S(  9,  13),   S(-63,  15),   S( 40, -35),   S(-38,  -8),   S( 69,  23),   S( 57,  36),   S( 14,  14),   S( 57,  31),
            S( 71,  28),   S( 30,   9),   S( -2, -45),   S( 20, -62),   S( 29,  -4),   S( 43,  14),   S( 37,  -8),   S( 20,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -29),   S(-46, -15),   S(-36,  -4),   S(-78,   6),   S(-34, -10),   S( 21, -22),   S( -8, -48),   S(-44, -23),
            S(-34, -47),   S(-44, -42),   S(-47, -45),   S(-25, -35),   S(  3, -46),   S( 47, -58),   S( 54, -60),   S( -8, -35),
            S(-32, -48),   S(-46, -41),   S(-48, -44),   S(-16, -43),   S( -8, -33),   S( 35, -40),   S( 51, -62),   S( 55, -52),
            S( -9, -45),   S(-35, -63),   S(-79, -40),   S(-42, -28),   S( 11, -45),   S( 27, -26),   S( 29, -16),   S( 79, -28),
            S( 18, -47),   S( 19, -65),   S(-25, -57),   S( -7, -67),   S( 42,  -8),   S( 33, -19),   S( 84,  39),   S( 81,  39),
            S(-10, -14),   S(-21, -45),   S( -5, -48),   S(  9, -25),   S(-12, -17),   S( 34, -33),   S( 71,  45),   S(110,  68),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42,  77),   S(-45,  75),   S( 11,   8),   S( -2,  16),   S( 23,  21),   S( 14,  -5),   S(-31,   3),   S(-35,  18),
            S(-80,  58),   S(-46,  55),   S(-48,  57),   S( -5, -15),   S(-19,  -8),   S(-58, -17),   S(-67, -12),   S(  9, -16),
            S(-98, 121),   S(-30, 116),   S( 16,  55),   S(-44,  47),   S( 31, -19),   S(-104, -13),  S(-64, -26),   S(-52,  -4),
            S(-31, 145),   S( 10, 161),   S( 18, 116),   S( 33,  56),   S(-45,  21),   S(-27, -28),   S(-33, -11),   S(-36,  -5),
            S(-39, 182),   S( 49, 161),   S( 28, 165),   S( 48, 145),   S( 20,  -7),   S(  9,  23),   S(-13, -21),   S(  2,  15),
            S( 45, 192),   S( 76, 197),   S( 95, 198),   S( 40,  80),   S(  5,  44),   S( -6,   1),   S(-13, -14),   S( -9, -20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-117,  90),  S(-92,  54),   S( 36,  24),   S( 11,  40),   S(  3, -17),   S(-62,  23),   S(-119,  23),  S(-91,  41),
            S(-78,  48),   S(-68,  34),   S(-55,  45),   S( -5,  45),   S(-37,   6),   S(  5, -14),   S(-77,  -9),   S(-18,   1),
            S(-108,  77),  S(-123, 101),  S(-67,  85),   S(-127, 100),  S(-104,  70),  S(-95,  12),   S(-26, -27),   S(-38,  -8),
            S(-71, 118),   S(-15, 127),   S(-15, 137),   S( 50, 130),   S(-47,  66),   S(-21,   6),   S(-10,   6),   S(-60,  36),
            S( 13, 126),   S( 33, 148),   S( 17, 153),   S( 52, 172),   S( 26, 139),   S(-21,  34),   S( 11,  -4),   S( 23, -13),
            S( 10,  87),   S( 25, 110),   S( 70, 143),   S( 78, 191),   S( 24,  91),   S( -4,  -4),   S(-14,   1),   S(-27, -34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-99,  24),   S(-75,   5),   S(  5,   5),   S(-11, -12),   S(  1,   6),   S(-78,  35),   S(-149,  44),  S(-56,  25),
            S(-71,  -2),   S(-46,  -7),   S(-16,  -3),   S(-16, -21),   S(-17,  33),   S(-79,  45),   S(-128,  37),  S(-76,  19),
            S(-23, -11),   S(-61,  10),   S(-27,  10),   S(-80,  70),   S(-108, 101),  S(-23,  51),   S(-129,  52),  S(-109,  53),
            S(-88,  25),   S(-89,  31),   S(  5,   6),   S(-64,  96),   S( 17, 100),   S(-48,  81),   S(-31,  56),   S( -9,  41),
            S(-12,  27),   S(-46,  40),   S(  9,  45),   S( 29, 127),   S( 73, 134),   S( 35,  79),   S(-41,  99),   S( 20,  57),
            S( 12,  13),   S(-14,   1),   S(  8,  14),   S( 62, 126),   S( 16, 135),   S( 13,  99),   S(-20,  89),   S( 29, 106),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-59, -10),   S(-74,  17),   S( 44, -22),   S( -4,  -8),   S(  8,  22),   S(-114,  71),  S(-61,  35),   S(-46,  43),
            S(-76, -17),   S(-82, -17),   S(-18, -52),   S(-49,  47),   S(-29,   5),   S(-22,  26),   S(-82,  61),   S(-107,  57),
            S(-69, -34),   S(-63, -26),   S(-31,  13),   S(-31,   4),   S(-38,  28),   S( 13,  52),   S(-99,  91),   S(-54,  81),
            S(-89,  15),   S(-103, -18),  S(-51, -13),   S(-78,  28),   S( -9,  45),   S(  4,  74),   S( 17, 108),   S( 80,  84),
            S(-12,  36),   S(-62,   2),   S( -6,  -2),   S( -3,  28),   S( 42,  98),   S(  2, 112),   S(106, 136),   S( 84,  99),
            S(-58,  44),   S(-13,  -1),   S( 22,  10),   S(-20, -21),   S( 23,  92),   S( 29, 155),   S( 79, 179),   S( 48, 175),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35,  12),   S(-19, -20),   S(-16,   0),   S(  2,   8),   S(-12, -28),   S(-18,   9),   S(-18, -21),   S(-10, -14),
            S(-16, -19),   S(-10,  39),   S( -8,   6),   S( 17,  13),   S( -8,  42),   S( -2, -29),   S(-29, -37),   S(-32, -54),
            S(-27,  49),   S(-23,  80),   S( 26,  50),   S(  8,  31),   S(-10,  -1),   S(-33,  13),   S(-51, -41),   S(-44, -48),
            S(-51,  96),   S(-41, 130),   S( 54, 148),   S( 25, 111),   S(-15, -36),   S(-33, -34),   S( -8, -12),   S(-65, -67),
            S( 28, 105),   S( 41, 221),   S( 34, 136),   S( 28,  58),   S( 11,  10),   S(  8, -39),   S(-25, -24),   S(-24, -57),
            S( 39, 144),   S( 50, 247),   S(102, 208),   S( 49, 112),   S( -3,   8),   S( -9, -20),   S( -4,  -9),   S(-15, -23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -19),   S(-26,   8),   S(  0,  25),   S( -3,   3),   S(-10,  -7),   S(-48,  -2),   S(-42, -48),   S(-30,  -3),
            S(-61,   0),   S(-86,  42),   S(-21,  36),   S(  9,   9),   S(-55,  18),   S( -1,  12),   S(-93, -30),   S(-54,  28),
            S(-54,  63),   S(-86,  47),   S(-47,  79),   S(-32,  91),   S(  5,   4),   S(-35, -46),   S(-54, -32),   S(-81, -42),
            S(-86,  97),   S( -7, 125),   S(-27, 152),   S(  5, 103),   S(  0,  78),   S(-41,  22),   S(-14, -11),   S(-34, -58),
            S(  1, 104),   S( 52, 147),   S( 74, 207),   S( 51, 244),   S( 26, 159),   S(-18,  -3),   S(  0, -79),   S(-14, -35),
            S( 35,  55),   S( 66, 191),   S( 93, 207),   S( 98, 267),   S( 36, 143),   S( -8,  -5),   S( -1,  -5),   S(-13,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -68),   S(-35,  -6),   S(-10, -21),   S( -4,   1),   S( -8, -17),   S(-40,  38),   S(-39,  11),   S( -1,  50),
            S(-47,  16),   S(-48,  -9),   S(-47, -42),   S(  9,  11),   S(-12,  47),   S( -7,   8),   S(-41,  11),   S(-48,  44),
            S(-71,  -7),   S(-66, -14),   S(-23, -13),   S( -7,  39),   S(-30,  81),   S(-61,  32),   S(-44,  -4),   S(-61,  17),
            S(-51,  -1),   S(-33,  40),   S(-21,  12),   S( -9,  98),   S(-20, 157),   S(-52,  83),   S(-44,  24),   S(-39,  94),
            S(-29, -43),   S(  8,  25),   S( 12, 100),   S( 43, 134),   S( 42, 227),   S( 45, 171),   S( 25,  72),   S( 30,  70),
            S(-23,   0),   S( 29,  36),   S( 24, 105),   S( 38, 174),   S( 59, 208),   S( 50, 133),   S( 31,  86),   S(  9,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -45),   S(-37, -14),   S( -5,  -5),   S( -1,   1),   S( 16,  21),   S(  5,  54),   S(-16, -17),   S( 23,  37),
            S(-46, -17),   S(-45, -32),   S(-14, -39),   S( 24,  -9),   S( -9,  17),   S( -7,  33),   S(  4,  25),   S(-26,  -1),
            S(-30, -78),   S(-29, -51),   S(-26, -48),   S( -9, -19),   S( 18,  32),   S(-14,  69),   S( 16,  74),   S(-14,  88),
            S(-41, -26),   S(-51, -42),   S(-21,  -1),   S( 22,  -4),   S(-16,  52),   S(  2,  92),   S(-22, 158),   S(-11,  55),
            S(-36, -33),   S(-22,  -4),   S(-17,  -4),   S(  4,  12),   S( 37,  99),   S( 43, 179),   S( 46, 267),   S( 65,  69),
            S(-15,  18),   S(  1,  14),   S( -1,  10),   S(  3,   8),   S( 32,  96),   S( 90, 188),   S( 28, 172),   S( 30,  48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-31,  -6),   S(-14,  14),   S(-23,  -8),   S(-26, -13),   S(-31, -31),   S(  9, -25),   S(-54, -58),   S(-55, -27),
            S(-36,  72),   S( 31, -27),   S(-38,  33),   S(  3, -14),   S( -7,  -8),   S(-17, -36),   S(-21, -12),   S(-75, -49),
            S(-23,  89),   S( -6,  -7),   S( 19, -20),   S(-28,  50),   S(  1,  24),   S(-31,  13),   S( -5, -41),   S(-45, -23),
            S( 17, -44),   S( 37,  21),   S( 17,  33),   S( 24,  37),   S( 15,  -4),   S( -6,  17),   S( -4, -19),   S(-17,  -4),
            S( 37, -35),   S( 65, -21),   S( 16,  19),   S( 74, -25),   S( 41,  -4),   S( 18,  38),   S(  5,  -5),   S(-69, -20),
            S( 33, -16),   S(-20,  17),   S( 21,  24),   S( 52,   7),   S( 62, -45),   S(  5,  22),   S(  7, -16),   S(-14, -11),
            S(-34, -42),   S( 37, -74),   S( 19, -22),   S(  5, -35),   S( 38, -27),   S( -9, -13),   S(  5, -33),   S(-39, -29),
            S(-92, -56),   S(-29, -31),   S(-14, -40),   S(-17, -66),   S(-20, -11),   S(  7,  -2),   S(  2,  18),   S(  2, -18),

            /* knights: bucket 1 */
            S(-55,  10),   S(-52,  92),   S( 20,  52),   S(-36,  70),   S(-16,  42),   S(-21,  28),   S(-30,  25),   S(-28, -17),
            S( 14,  16),   S( 20,  22),   S(-13,  33),   S(-15,  67),   S( -3,  27),   S(  2,  10),   S(-23,  -1),   S(-28, -16),
            S(-27,  23),   S(  9,  15),   S(  5,  10),   S( 20,  31),   S(  2,  42),   S(-20,  29),   S(-10,  -1),   S(-20,  -8),
            S(-18,  38),   S( 39,  33),   S( 21,  50),   S( 13,  43),   S( 11,  39),   S( -8,  37),   S( 21,   1),   S( 23,  10),
            S( 11,  31),   S( 29,  28),   S( 45,  24),   S( 34,  29),   S( 26,  33),   S( 12,  42),   S( 19,  18),   S( 17,  14),
            S( -9,  28),   S( 42, -20),   S( 19,  22),   S( 28,  31),   S( 27,  35),   S( 48,  16),   S( 32,  22),   S( 15,  -9),
            S( 40,  26),   S(  5,  34),   S(-14, -18),   S( 34,  24),   S( -8,   3),   S( 77, -17),   S(-40,  18),   S(-20, -16),
            S(-98, -68),   S( -3,  16),   S(-24,  -3),   S(  3,  20),   S(-20, -18),   S( -6, -19),   S(-17, -33),   S(-83, -80),

            /* knights: bucket 2 */
            S(-57,  11),   S( -1,  16),   S(-22,  42),   S(-13,  46),   S(-50,  72),   S(-34,  65),   S(-22,  52),   S(-35,  23),
            S(-23,  -4),   S(-33,  28),   S( -5,  16),   S( -5,  34),   S(  3,  25),   S( -1,  48),   S(-24,  49),   S(-53,  51),
            S(-18,  22),   S(  6,  13),   S( -4,  29),   S( 24,  14),   S( 11,  30),   S(  9,  20),   S( -7,  35),   S(-27,  29),
            S(-15,  42),   S( -5,  41),   S( 10,  43),   S(  5,  53),   S( 11,  44),   S( -5,  44),   S(  8,  47),   S(  4,  35),
            S( 20,  31),   S(-10,  33),   S( -1,  54),   S(-23,  66),   S( -1,  53),   S( -3,  43),   S(  5,  44),   S(  3,  18),
            S(-28,  13),   S(-12,  40),   S( -5,  39),   S( 10,  35),   S(-31,  67),   S(-12,  36),   S(-19,  17),   S( 58, -35),
            S( 24,   7),   S(-46,   7),   S(-41,  18),   S(-14,  24),   S(-31,  27),   S( 12,  17),   S(-27,  24),   S(-51,   4),
            S(-172,  10),  S(-14, -18),   S(-81,  35),   S(  0,  13),   S( -5,  12),   S(-84,  -6),   S(  6, -16),   S(-184, -73),

            /* knights: bucket 3 */
            S(-50, -38),   S(  6, -40),   S(-32,  -7),   S( -3,   6),   S( -4,   0),   S( -8,   4),   S( 10, -17),   S(-21, -26),
            S(-14,  -6),   S(-31,  -7),   S(-18,  -9),   S( 17,   2),   S( 19,   2),   S( -6, -10),   S(  5,  -9),   S(-21,  49),
            S(  4, -31),   S(  8,   4),   S(  7,   1),   S( 26,   6),   S( 28,  20),   S( 28,   9),   S(  3,  11),   S(  4,  37),
            S( -6,   1),   S( 10,  23),   S( 21,  39),   S( 22,  40),   S( 37,  38),   S( 24,  38),   S( 36,  18),   S( 29,   9),
            S( 25,  13),   S(  9,  25),   S( 37,  11),   S( 30,  50),   S( 26,  42),   S( 33,  55),   S( 42,  42),   S( 14,  23),
            S( -5,   3),   S( 16,   1),   S( 34,   8),   S( 71,  -4),   S( 85, -10),   S( 84, -17),   S( 42,  14),   S( 12,  37),
            S( 19,   1),   S(  7,   3),   S( 48, -30),   S( 31,   9),   S( 45, -22),   S( 37, -31),   S( 49, -63),   S( 63, -17),
            S(-123,  -6),  S(-27,  -3),   S(-15,   2),   S( 37,   2),   S( 50, -22),   S(-23, -13),   S( 19, -20),   S(-57, -40),

            /* knights: bucket 4 */
            S( 24,  32),   S(-69,  -4),   S( -5,  39),   S(-39, -38),   S(-47, -24),   S(-43, -34),   S(-20, -50),   S(-27, -41),
            S( 45,   9),   S(-44,  32),   S( 35, -32),   S( -3, -11),   S( 39, -34),   S( 22, -36),   S( 16, -25),   S(-13, -53),
            S(-21,  11),   S( 19,  53),   S( 18,   1),   S( 45,   8),   S( 37,  -6),   S(-30,  20),   S(-43, -36),   S(-32, -62),
            S(-15,  65),   S( 45, -13),   S( 36,  27),   S( 39,  17),   S( 48,  -4),   S(125, -27),   S( 23, -44),   S(  4,  11),
            S( 43,  44),   S(-40,  63),   S( 39,  53),   S( 52,  28),   S( 47,  34),   S( -4,  37),   S(  2, -49),   S(-22, -14),
            S(  0,  42),   S(-10,   5),   S( 54,  42),   S( 24,  25),   S(-11,  35),   S( 37,  41),   S( -2,  21),   S(-20, -36),
            S( -9, -21),   S( -7,  16),   S(  4, -10),   S( -9,  27),   S(  9,  31),   S( 27, -24),   S( -6, -13),   S(-17,   2),
            S(-17, -14),   S( -9, -20),   S( 21,  32),   S(  2,   7),   S(  5,  18),   S(  8,  27),   S( -8,  -3),   S( -2, -20),

            /* knights: bucket 5 */
            S( 17,  -3),   S( -8,  40),   S( 26,  31),   S( 26,  41),   S( 26,  12),   S(-12,  -8),   S( -7,  59),   S(-24, -14),
            S( 22,  -5),   S( 39,  60),   S(-15,  47),   S(-38,  57),   S( 29,  43),   S( 15,  31),   S( 30,  12),   S( -4, -36),
            S( -4,  37),   S(-19,  29),   S( 36,  30),   S( 59,  37),   S( -7,  51),   S( 11,  22),   S(  6,  20),   S( 12,  17),
            S( 51,  32),   S(  5,  56),   S( 48,  48),   S( 40,  59),   S(  1,  61),   S(  3,  57),   S( 16,  49),   S(  0,  41),
            S(-18,  49),   S( 16,  42),   S( 56,  62),   S( 69,  45),   S(108,  37),   S( 56,  45),   S( 46,  48),   S( 21,  48),
            S(  3,  28),   S(  5,  41),   S( 40,  33),   S( 30,  48),   S( 38,  43),   S( -6,  58),   S( 28,  36),   S(-22,  -5),
            S(-15,  45),   S(-14,  57),   S( 39,  30),   S(  1,  63),   S( 10,  47),   S( -1,  51),   S( 25,  67),   S( -4, -11),
            S( 11,  42),   S(-11,  -3),   S( 19,  44),   S( -9,  14),   S(  8,  46),   S( -5,  -9),   S(  1,  33),   S(-22, -13),

            /* knights: bucket 6 */
            S(-17, -84),   S( 26, -34),   S( 27,  21),   S(-43,  47),   S(-26,  49),   S( 12,  51),   S(-27,   4),   S(-23,   6),
            S(-19, -36),   S( 64,   9),   S( 41,  16),   S(-57,  55),   S(-57,  73),   S( 37,  49),   S( 14,  63),   S(-13,  -1),
            S( -2, -30),   S( -5,  10),   S(  4,  23),   S( 31,  29),   S( 12,  48),   S(-24,  63),   S( -2,  63),   S(  5,  25),
            S(-16,  31),   S( 43,  13),   S( 48,  40),   S( 88,  27),   S( 19,  65),   S( 39,  57),   S( 11,  72),   S(-22,  73),
            S( -9,  36),   S( 74,  -4),   S( 41,  53),   S( 78,  42),   S(111,  38),   S( 93,  45),   S( 15,  60),   S( 18,  69),
            S( 12,  19),   S( 21,  19),   S( 49,  32),   S( 53,  37),   S( 22,  60),   S( 41,  34),   S(  6,  35),   S( 33,  39),
            S(-22,  37),   S( 16,  30),   S(-33,  47),   S( 35,  33),   S(  4,  54),   S(  9,  38),   S( 35,  90),   S(-25,  25),
            S(-63, -11),   S( 25,  57),   S( 29,  28),   S( 21,  34),   S( 27,  29),   S( 19,  48),   S( 17,  66),   S( 15,  41),

            /* knights: bucket 7 */
            S(-42, -67),   S(-210, -42),  S(-56, -47),   S(-66, -18),   S(-33,  -7),   S(-51, -23),   S(  4,  -8),   S(-28, -15),
            S(-35, -76),   S(-52, -48),   S(-33, -26),   S(-38,  -4),   S(-57,  29),   S( -6,   7),   S(  3,  72),   S( 45,   9),
            S(-87, -66),   S(-60, -32),   S(-66,  12),   S( 26, -22),   S( -8,  -7),   S(  1,  19),   S(  1,  51),   S( 63,  53),
            S(-81, -47),   S( 28, -42),   S( -9,  19),   S( 12,  15),   S( 49,  13),   S( 18,  21),   S( 18,   3),   S(-16,  45),
            S(-50, -24),   S(-54,  -9),   S( 29,  -6),   S( 80, -15),   S(127, -16),   S( 60,  32),   S( 95,  -1),   S( 62,  30),
            S(  0, -40),   S( 36, -55),   S( -2, -10),   S( -9,  13),   S( 48,  31),   S( 67,   2),   S( 61, -31),   S( -8,  19),
            S(-45, -30),   S(-68, -15),   S(  2, -11),   S( 37,  25),   S( 34,   9),   S( 56,  -7),   S(-15,  28),   S( -2, -23),
            S(-30, -11),   S(-14, -35),   S(-41, -16),   S( 12,  -4),   S( 14,   0),   S( 24,  21),   S(-11, -33),   S( -6, -24),

            /* knights: bucket 8 */
            S(  0, -11),   S( -7,   1),   S( -1,  -1),   S(  3,   7),   S(-14, -48),   S( -8, -46),   S(  5,  -7),   S( -5, -30),
            S(  6,  21),   S( -8, -26),   S( -6, -34),   S(-12, -46),   S(-45, -34),   S(-26, -82),   S(-14, -59),   S(-15, -29),
            S(  0,  17),   S(-15, -27),   S( 19,  19),   S( -4,  -4),   S( 14,  20),   S(-26, -24),   S( -5, -27),   S(  0, -17),
            S(-21,   9),   S( -6,   4),   S(  0,  23),   S(  5,  40),   S( 21,  32),   S( 15,  14),   S( -7, -23),   S(  5,   6),
            S( 34,  75),   S( 11,  14),   S( 26,  40),   S( 34,  31),   S( 10,  46),   S( -2,  15),   S(  1, -17),   S( -8, -17),
            S( 11,  25),   S( 17,  19),   S( 50,  14),   S( 29,  19),   S(  3,  12),   S( -2, -11),   S( -9, -40),   S(-13, -12),
            S( -1, -11),   S( 11,   9),   S(-13,  -6),   S( 13,  17),   S( 12,  26),   S( 17,  48),   S(  0,   7),   S( -6,   2),
            S( -1,  -6),   S( 16,  39),   S( -2,  14),   S( -3,  -1),   S(  0,  11),   S( -3,  -1),   S(  1,  -4),   S( -5,  -7),

            /* knights: bucket 9 */
            S( -8, -27),   S(-20, -41),   S(-18, -41),   S(  0,  -8),   S(-28, -68),   S(-19, -50),   S(  1,  -1),   S( -2, -26),
            S(-12, -38),   S( -8,  18),   S( -5, -47),   S( -9,   3),   S(  2, -15),   S(-12, -62),   S( -8,   2),   S(-17, -56),
            S(  8,  23),   S(-14,  16),   S( -1,   5),   S( 10,   5),   S( -2,  20),   S(-49,  16),   S( -1,  -7),   S( -4, -15),
            S(-15,   0),   S( -1,   6),   S( 14,  34),   S( 44,  18),   S( 26,  19),   S( 18,  38),   S(-22, -56),   S(-10, -18),
            S( -5,  34),   S(  1,  19),   S( 15,  35),   S( 26,  61),   S( 22,  27),   S( 12, -29),   S(  4, -14),   S( -1, -29),
            S( -7, -15),   S( 20,  24),   S( 30,  39),   S(-23,  30),   S( 32,  59),   S( 18,   3),   S(  6,   9),   S( -5, -16),
            S( -1, -16),   S(-11,  -7),   S( 32,  50),   S( 21,  16),   S( 14,  28),   S(  3,  13),   S( -7, -14),   S(  1,   2),
            S(  0,  -4),   S(  8,  23),   S( 12,  19),   S( 16,  23),   S(  6,  -1),   S(  4,  25),   S( -1,  -6),   S( -2, -11),

            /* knights: bucket 10 */
            S(-13, -38),   S(-12, -60),   S(-22, -39),   S(-19, -23),   S( -5,   8),   S(-10, -44),   S( -4,  12),   S(  7,  25),
            S( -9, -30),   S(-14, -27),   S(  4, -19),   S(-25,  -6),   S(-13, -51),   S(-15, -50),   S( -6, -19),   S( -9,  11),
            S(-16, -48),   S(-25, -54),   S(  7,  -5),   S( -8, -21),   S( 13,  -5),   S(-23, -14),   S( -8,  13),   S( -8,   5),
            S(-12, -26),   S( -6, -51),   S(  0, -11),   S( 21,  16),   S(  7,  47),   S( 23,  36),   S( 18,  24),   S( 21,  42),
            S(-10, -48),   S(-23, -47),   S( 23,  11),   S( 20,  40),   S( 14,  60),   S( 17,  32),   S( 26, -15),   S( 18,  30),
            S(-19, -57),   S( -5, -20),   S( -9,   9),   S(  3,  35),   S( 24,  50),   S( 32,  72),   S( 21,  47),   S(  5,  42),
            S(  0,  -6),   S(-13, -24),   S(  6,  -8),   S( 10,   5),   S( 12,  19),   S(  9,  52),   S( -7, -28),   S(  5,   4),
            S( -3, -22),   S(  2,   7),   S(-13, -15),   S(  5,  -4),   S( 14,  39),   S( -4,  24),   S( -3,  -5),   S( -2, -12),

            /* knights: bucket 11 */
            S( -2,   8),   S(-17,  17),   S( -8, -51),   S(-16, -31),   S(-26, -60),   S(-14, -26),   S( -5,  -9),   S( -4,   3),
            S(-13, -25),   S(-21, -58),   S(  4, -53),   S(-53, -49),   S(-34, -29),   S(-39,  -6),   S(-15, -39),   S( -6, -11),
            S(-18, -64),   S(-37, -60),   S(-11, -10),   S( -2,  -6),   S(-14,   0),   S(-34,  32),   S( 28,  -5),   S( -1,  -5),
            S(-11, -35),   S( -2, -24),   S(-27,   3),   S(  0,  32),   S( 31,  48),   S( 15,  29),   S( -5,  -2),   S( 18,  20),
            S(-19, -58),   S(-28, -81),   S(  8, -29),   S(  9,  35),   S( 25,  31),   S( 39,  52),   S( 14,  33),   S( 26,  95),
            S(  1,   4),   S( -3, -12),   S( 11,   4),   S( 52,  51),   S( -7,   5),   S( 36,  79),   S( 21,  18),   S( 10,  24),
            S(  6,  21),   S( -8, -13),   S(  6,  -5),   S( 30,  10),   S( 12,  26),   S( -9, -17),   S(  3,  16),   S( 12,  33),
            S(  0,  12),   S( -1, -20),   S( 10,   9),   S( -2,   0),   S( -4,   4),   S(  1,   4),   S(  8,  13),   S( -6,  -6),

            /* knights: bucket 12 */
            S( -4, -16),   S( -4,  -5),   S( -1, -25),   S( -1,   2),   S( -8, -17),   S( -3,  -7),   S(  4,  -4),   S( -2,  -9),
            S( -1,   1),   S( -3,  -9),   S( -1,   3),   S( -3, -18),   S( -3,   0),   S( -2, -26),   S(  1,  -7),   S(  3,  15),
            S( -5,  -7),   S(  2,   7),   S(-15,  -9),   S( -1, -14),   S(  2,  -2),   S(  3,   5),   S(  0, -13),   S( -6, -23),
            S( -2,   3),   S(  3,   2),   S( -7, -22),   S( 15,  37),   S(  0,   7),   S(  1,  -4),   S( -7, -23),   S( -4, -22),
            S(  5,   5),   S( -5, -16),   S(-16, -28),   S( -8, -23),   S(  6, -15),   S(  9,  36),   S( -2,  -5),   S(  0,  -6),
            S( -3,  -9),   S( -3, -12),   S(  2,   6),   S( 10,  10),   S( -4, -21),   S( -4, -19),   S( -2, -13),   S(  2,   7),
            S(  0,   8),   S(-14, -24),   S(  0,  11),   S(  2, -11),   S( -6,  -7),   S( -3, -24),   S( -1,   1),   S(  0,   2),
            S(  2,   4),   S(  3,   4),   S(  3,  -1),   S( -1,  -5),   S( -4,  -6),   S( -4, -13),   S( -2,  -7),   S(  0,   3),

            /* knights: bucket 13 */
            S( -8, -26),   S( -4,  -9),   S(  0, -10),   S( -3, -10),   S(-13,  -9),   S( -1,  -1),   S( -4,  -8),   S( -1,  -9),
            S( -4, -18),   S(  3,  10),   S( -5, -21),   S( -3,   0),   S( -7, -33),   S( -2, -21),   S(  0,   5),   S(  0,  -2),
            S( -1,  -3),   S(-10, -19),   S(  4,  16),   S(  5,  31),   S(-20, -55),   S( -9, -20),   S(  2,   8),   S(-12, -48),
            S( -5,   4),   S(  7,  27),   S(-11, -29),   S( -5,  -7),   S(  0, -14),   S( 11,  31),   S(  2,  -5),   S(-10, -20),
            S( -4, -10),   S( -2,   2),   S( 20,  22),   S( 13,  28),   S(  4, -29),   S(  3, -23),   S( -1, -23),   S(  0,  11),
            S( -2,  -7),   S( 27,  41),   S( 23,  19),   S( -2,  28),   S( -1,  16),   S( -9, -38),   S(  7,  14),   S( -2,   5),
            S(  0,   7),   S( -4, -11),   S(  9,   9),   S(  2, -17),   S( 16,  39),   S(  0,  -7),   S( -1,   5),   S( -3,   3),
            S(  0,   0),   S( -2,  -5),   S(  2,   3),   S(  7,  -4),   S( -8, -20),   S(  2,   0),   S(  0,   0),   S( -2,  -2),

            /* knights: bucket 14 */
            S( -2, -13),   S(-11, -51),   S( -5, -10),   S( -4,   0),   S(-11,  -7),   S( -2, -14),   S( -2,  -6),   S( -2,  -3),
            S(  0,  -1),   S( -4, -15),   S(-18, -66),   S( -5, -29),   S(  2,  -9),   S( -1,  10),   S(  4,   3),   S(  0,   4),
            S(-10, -22),   S( -2, -17),   S(-12, -54),   S(  6,  26),   S( 10,  16),   S( -4, -10),   S(  4,   8),   S(  1,   6),
            S( -2,  -1),   S( -9, -39),   S( -3, -10),   S(-16, -33),   S( -6, -18),   S(  2,  -6),   S(  0,  -1),   S( -9, -11),
            S(  0,   4),   S(  3,  -9),   S( 11,  41),   S(  0, -21),   S(-18,   4),   S(  8,   9),   S(  0,  -3),   S( -9, -25),
            S(  0,  -1),   S( -2, -18),   S(-10, -42),   S(  4,   3),   S( 19,   1),   S( 14,  28),   S(  1,   0),   S(  3,   4),
            S(  3,  -2),   S( -2, -10),   S(  7, -14),   S(  6,  10),   S( -2,  -7),   S( -4, -15),   S( -6,  -4),   S(  0,   5),
            S(  0,   0),   S(  1,   2),   S( -5, -26),   S(  9,   2),   S( -1,  14),   S(  3,  11),   S( -1,  -7),   S(  0,  -4),

            /* knights: bucket 15 */
            S( -3,  -7),   S( -1, -21),   S(  2, -16),   S( -8, -13),   S( -3,  -1),   S( -4,  -4),   S( -2,  -5),   S(  1,  19),
            S( -4, -11),   S(  1,  -2),   S( -4, -14),   S( -5, -23),   S(  0,  -1),   S(  2,   9),   S( -1,   3),   S( -1,  -2),
            S( -9, -24),   S(-10, -18),   S( -5, -19),   S(-14, -30),   S(  6,  -5),   S( -5, -17),   S(  2,   6),   S( -4,  -6),
            S( -6, -21),   S(-11, -46),   S(-12, -21),   S( -3,  -4),   S(-12, -67),   S( 12,  31),   S( -5, -17),   S( -9, -16),
            S( -1,  -8),   S(  3,   2),   S( -1, -13),   S( -4, -22),   S(  4,  19),   S( 10,  14),   S( -5,  -9),   S(  0,   1),
            S( -1,  -3),   S(  1,   8),   S( -5, -32),   S( -9,   1),   S(  4, -15),   S( -6,  26),   S( -4,  -8),   S(  0,   9),
            S( -2, -13),   S( -2,  -9),   S( -1,  -8),   S( -5,  -4),   S(-16, -39),   S(  0,  18),   S(-15, -26),   S(  1,  10),
            S( -1,  -2),   S(  0,  -1),   S( -2,  -5),   S( -7, -18),   S( -3,  -4),   S(-15, -14),   S(  6,  14),   S( -3,   5),

            /* bishops: bucket 0 */
            S( 39,  20),   S( 29, -23),   S( 50,  14),   S( 12,   8),   S(-16, -12),   S( -3,   3),   S(  6, -33),   S(-17, -13),
            S( 32, -37),   S( 91,  15),   S( 32,   5),   S( 20, -10),   S( -8,  33),   S( 16, -46),   S(-24,  -5),   S( 16, -59),
            S( 26,  34),   S( 54,  12),   S( 41,  32),   S(  9,  56),   S( 19,   9),   S(-18,  51),   S( 22, -34),   S( 24, -40),
            S( 31,   0),   S( 63,  12),   S( 57,   9),   S( 34,  39),   S( 11,  33),   S( 39,  -1),   S(  9, -25),   S( -3,  -3),
            S( 12, -24),   S( 44,   1),   S( 29,  19),   S( 56,  21),   S( 60, -23),   S(  2,  10),   S( 19, -24),   S(-41,  24),
            S(-25,  47),   S(-19,  27),   S( 73,   7),   S( 72, -30),   S( 83,  22),   S( 22,   3),   S( -1,   3),   S(  5,  -1),
            S(-47,  35),   S( -6,  25),   S( 52, -21),   S( 10,  30),   S(-47,   5),   S( 28,  18),   S( 60, -47),   S(-18, -34),
            S(-30, -58),   S( 13,  24),   S( 14,   4),   S( -1, -18),   S( 27,  -5),   S( 33,   9),   S(-12,  22),   S(-37,   8),

            /* bishops: bucket 1 */
            S( 51,  20),   S( -9,  19),   S( 12,  17),   S(-12,  30),   S( 10,  12),   S( -3,  19),   S( -7, -10),   S(-28,  14),
            S( -1, -17),   S( 46,   3),   S( 44,   8),   S( 19,  35),   S(  1,   8),   S(  6,  -9),   S(-25,  11),   S(  0,  -3),
            S( 36, -10),   S( 34, -13),   S( 48,  23),   S( 18,  21),   S( 20,  19),   S( -7,  14),   S( 29,  -9),   S( 13, -37),
            S( 31,   6),   S( 31,  14),   S( 22,   2),   S( 36,  28),   S(  6,  32),   S( 28,   2),   S( -8,  13),   S(  1,   2),
            S( 45,  18),   S( 15,   1),   S(  7,  36),   S( 13,  14),   S( 29,   5),   S(-10,  19),   S( 23, -16),   S(  0,  14),
            S( -5,  18),   S( 36,  25),   S( 42,  35),   S( 47, -15),   S( 10,  25),   S( -7,  46),   S( -7,  36),   S( 34, -22),
            S(-29,  64),   S(  7,  34),   S(  6,  23),   S( 68,  15),   S( 22,  28),   S( -6,  19),   S( 35,   2),   S( -9,  10),
            S( 16,  14),   S(-10,   9),   S(-17,   2),   S(-12,  26),   S( 10,  11),   S( -2,  -2),   S( 14,  36),   S( 17,  36),

            /* bishops: bucket 2 */
            S( 35,  -9),   S( 11,  11),   S(  3,  15),   S(-21,  40),   S(  3,  20),   S(-17,  17),   S(-27,  -7),   S(-48,  11),
            S(-22,  25),   S( 19,   1),   S( 20,   6),   S(  4,  18),   S(  2,  32),   S(  9,   7),   S(  6,  -9),   S(  8, -45),
            S(  1, -17),   S(  3,  12),   S( 17,  36),   S(  1,  35),   S(  8,  32),   S( 12,  32),   S( 16,   5),   S(-20, -10),
            S(-13,  18),   S( -7,   5),   S(-14,  39),   S(  5,  40),   S( -4,  53),   S( 13,  19),   S(  2,  26),   S(  4, -21),
            S( -7,   1),   S(-16,  25),   S(  6,  25),   S(-25,  47),   S( -2,  31),   S( -9,  51),   S(  3,  19),   S(-24,  26),
            S(-25,  43),   S(-12,   5),   S(  6,  51),   S(-16,  33),   S(  2,  19),   S( 19,  36),   S( 12,  50),   S(  5,  15),
            S(-22,  17),   S( -7,  39),   S(-37,  56),   S(  3,  -3),   S( 22,  -8),   S(-18,   8),   S(-74,  46),   S(-34,  41),
            S(-58,  46),   S(-55,  41),   S(-66,  33),   S(-41,  29),   S(-66,  38),   S(-58,  26),   S( -8,  21),   S(-60,  11),

            /* bishops: bucket 3 */
            S(  5,  25),   S( 23,  -2),   S( 19,  16),   S( 13,  10),   S(  9,  20),   S( 48,  -9),   S( 31, -37),   S( 39, -50),
            S(  4,   6),   S( 13,  26),   S( 27, -13),   S( 14,  31),   S( 22,   8),   S( 15,  29),   S( 54,  15),   S( 25, -11),
            S( 18,  -1),   S( 17,  11),   S( 26,  38),   S( 28,  17),   S( 21,  53),   S( 29,  29),   S( 38,  31),   S( 41, -16),
            S( 17,   2),   S( 27,   9),   S( 21,  35),   S( 28,  51),   S( 31,  42),   S( 39,  26),   S( 22,  24),   S( 21,   6),
            S( 14,  -2),   S( 26,  10),   S( 49,  13),   S( 37,  39),   S( 32,  42),   S( 33,  39),   S( 16,  31),   S( 26,  31),
            S( 15,   4),   S( 24,  22),   S( 36,  37),   S( 41,  24),   S( 32,  16),   S( 50,  23),   S( 50,   9),   S(  0,  64),
            S( 13, -15),   S( 17,  28),   S( 47,  15),   S( -4,  19),   S( 35,  -1),   S( 37, -11),   S(  4,  54),   S( 18,  20),
            S(-29,  75),   S( -9,  45),   S( 58,  -1),   S( 17,  24),   S( 14,  -7),   S( -3,  32),   S(  0,  17),   S( 59,  -9),

            /* bishops: bucket 4 */
            S(-27, -30),   S( -8,   5),   S(-38, -10),   S(-18,  27),   S(-52,  50),   S(-61,  20),   S( -6, -40),   S(-11,  11),
            S(-23,  -9),   S( 17,   6),   S(-17,  49),   S(-40,  19),   S(  4, -19),   S( 67, -10),   S(-34,  30),   S( 29,  -3),
            S( 19,  43),   S(-39,  44),   S( 44,  13),   S(-15,  11),   S( 14,  27),   S( 46,   0),   S(-53,  -2),   S(-50,   7),
            S(-30,  38),   S(  8,  27),   S( 54,  35),   S( 49,  14),   S( -9,  21),   S( 48, -12),   S( 47,  -6),   S(-22, -33),
            S(  9,  38),   S( 15,  47),   S(-57,  69),   S( 28,  43),   S( -4,  11),   S( 23, -21),   S(-30, -19),   S( 27,  -2),
            S(-12,  60),   S(  7,  30),   S(-53,  59),   S( 25,  22),   S( 33,   6),   S( 11,  35),   S( 14, -26),   S(-26, -52),
            S(-15,  24),   S( 32,  25),   S(-11,  30),   S( 38,   8),   S(  5,   0),   S( -2,   4),   S( 13,   2),   S( 13, -33),
            S( 31,   1),   S(-26, -39),   S(  2, -12),   S( -9, -19),   S( -1,  -2),   S(-10,   0),   S(  6, -12),   S(  6,  18),

            /* bishops: bucket 5 */
            S(-25, -17),   S( -4,  21),   S(-55,  38),   S(-30,  25),   S(-50,  37),   S(-49,  11),   S(-11,  14),   S(-16,  15),
            S(-34,  33),   S(-41,  33),   S(-13,  43),   S( 37,   7),   S(-18,  23),   S(-52,  35),   S(-27,  14),   S(-31, -18),
            S(-13,   7),   S(-24,  52),   S(  3,  38),   S( -7,  40),   S( 10,  29),   S(-32,  29),   S(-44,  39),   S(-16, -14),
            S( 28,  11),   S(  6,  20),   S(-52,  80),   S( 33,  28),   S( 47,  32),   S( 24,  35),   S( 34,   5),   S( 21,   3),
            S( 59,  31),   S( 35,  19),   S( 87,  14),   S( 73,  33),   S( 20,  20),   S( 26,   2),   S( 49,   7),   S(-16,   2),
            S( 39,  27),   S( 28,  32),   S( 61,  34),   S( 18,  32),   S(  5,  39),   S( 55, -26),   S(-36,  55),   S(  2,  31),
            S(-12,  40),   S(-27,  15),   S( -2,  32),   S( -6,  51),   S( 39,  19),   S(  5,  41),   S(  9,  35),   S( 12,  17),
            S(  8,   1),   S( 12,  28),   S(-14,  11),   S(  5,  58),   S( -3,  44),   S( 12,  36),   S( 37,  37),   S(-15,  11),

            /* bishops: bucket 6 */
            S(-18,  36),   S(-23,  25),   S(-72,  42),   S(-45,  19),   S(-41,  22),   S(-74,  46),   S( -6,  54),   S( -8,   4),
            S( -5,   6),   S(  4,  12),   S(  4,  16),   S(  0,  23),   S(-35,  43),   S(-17,  24),   S(-115,  59),  S( 46,  11),
            S(  4,   0),   S(  8,   6),   S( 35,  22),   S( 24,  37),   S( 44,  26),   S( 50,  14),   S( 24,  18),   S(-78,  30),
            S(-12,  38),   S(  0,  24),   S( 26,  32),   S( 43,  35),   S( 22,  48),   S( 16,  38),   S( 22,  43),   S(-37,  30),
            S(-14,  17),   S( 56,  -3),   S( 11,  36),   S( 53,  28),   S( 83,  37),   S( 72,  34),   S( 24,  36),   S( -4,  44),
            S(  1,   9),   S(-34,  36),   S( 26,  32),   S( 56,  15),   S( 28,  35),   S( 28,  45),   S(  8,  55),   S(-36,  56),
            S(-27,  33),   S(-32,  50),   S(-12,  39),   S(-33,  36),   S( 48,  13),   S( -6,  55),   S( 15,  51),   S( -8,  37),
            S( -8,  63),   S(  6,  42),   S( 29,  19),   S( -5,  52),   S( -5,  24),   S( 13,  22),   S( 32,   4),   S( 21,  13),

            /* bishops: bucket 7 */
            S(  3, -21),   S(  6,  -7),   S(-38, -28),   S(-38,  -9),   S(-25, -26),   S(-98,  19),   S(-66, -18),   S(-63,  16),
            S(-22, -52),   S(-59, -16),   S(-12, -18),   S( -4, -21),   S(-29,  11),   S(-32,   0),   S(-26, -20),   S(-41, -27),
            S(-12, -55),   S( 10, -19),   S( 45, -20),   S( 32,   2),   S(-11,   8),   S(-20,  11),   S(-27,  58),   S(-27,  14),
            S(-49,   9),   S( 36, -20),   S( 72,  -8),   S( 64,  -2),   S(106,  -9),   S( -9,  22),   S( 19,  34),   S(-28,  52),
            S( -8, -43),   S(-39, -11),   S( 47, -30),   S( 60,   4),   S( 75,  21),   S( 87,  -4),   S( -6,  49),   S( 31,   4),
            S(-36, -26),   S(-17,  -6),   S( 38, -22),   S( 11,  -3),   S( 68, -42),   S( 79,  -3),   S( 70,  19),   S( 32, -14),
            S( -8, -28),   S(-33,  24),   S(  9,  -1),   S( 29, -20),   S( 31, -40),   S( 53, -13),   S( 24,  19),   S(  0,  15),
            S( -9,   3),   S(-40,  23),   S(-42,   4),   S( 21, -23),   S( 27, -27),   S( 17, -28),   S( 19,   8),   S( 34,  42),

            /* bishops: bucket 8 */
            S(-14, -11),   S( -5, -14),   S(-33, -12),   S(-17, -40),   S( -8,   5),   S( -4,  28),   S(  5,  18),   S(  2,   4),
            S( -5,  -6),   S(-37, -22),   S(-34, -30),   S(  7,  16),   S(  8, -27),   S(-27, -14),   S( -5, -14),   S( -4,  -3),
            S(  9,  11),   S(-31,  32),   S(-17,  25),   S(-16,   4),   S(-10,  12),   S(-20,  24),   S(-15, -80),   S(-28, -40),
            S(  9,  24),   S(-12,  47),   S(-10,  18),   S( -3,  -5),   S( 15,  59),   S(-12,  10),   S( 17, -15),   S( -6, -20),
            S( 17,  33),   S(  2, 100),   S(  2,  17),   S( 69,  51),   S( 14,   4),   S( 14,  -5),   S( 15, -25),   S( -2, -26),
            S(-16,  -8),   S( 12,  36),   S( 40,  48),   S( -8,  32),   S( 36,   0),   S(  2,   4),   S(-25, -40),   S(-11, -14),
            S( -6,  22),   S( 26,  50),   S( 14,  27),   S( -4,   5),   S( -2,   0),   S( -3,   2),   S( -9,   0),   S(-10, -42),
            S( -8, -19),   S( -6, -16),   S(  1, -10),   S(  0, -14),   S(-27,   1),   S( -9,  -9),   S( -2,  13),   S( -4,  17),

            /* bishops: bucket 9 */
            S( -9,  -1),   S(  1,  15),   S(  1,  14),   S(-14, -27),   S(-31, -40),   S(-20, -31),   S(-18,   4),   S( 18,   0),
            S( -4, -18),   S(-33, -13),   S(-22, -25),   S(-11,   7),   S(-56,   2),   S(-14, -24),   S(-18,   1),   S(  1,  -8),
            S( -3, -19),   S( 10, -10),   S(-19,   8),   S(-28,  18),   S(-14,  12),   S( -9,   3),   S(-23, -48),   S(-17,  28),
            S( -2,   4),   S(  8,  22),   S(-20,  29),   S( 21,  20),   S( 11,  37),   S(  7,   4),   S(-15,   2),   S(-23, -44),
            S( -2,  11),   S( 30,   8),   S( 15,  57),   S( 23,  44),   S(-28,  21),   S(-14,  35),   S(  8,  32),   S(-11, -22),
            S(-21,  -2),   S( 33,  64),   S( 18,  52),   S( 31,  24),   S( 20,  45),   S(  9,  30),   S(-20, -23),   S(-12, -13),
            S(  0, -18),   S( 21,  24),   S(  5,   1),   S( -1,  40),   S( 28,  29),   S(  3,   5),   S(  2,   7),   S(-11, -20),
            S( -4, -31),   S( 10,  49),   S( -6,  37),   S(-22, -11),   S(-10, -39),   S( 19,  26),   S( -9, -12),   S( -8, -17),

            /* bishops: bucket 10 */
            S(-25, -19),   S( -6, -35),   S(-38, -24),   S(-14,   7),   S(-16, -30),   S(-26, -12),   S(-23, -51),   S(-24, -11),
            S(  4, -28),   S(-31, -15),   S(-11, -11),   S(-45,  -9),   S(-31,  13),   S(-32,  25),   S(-22, -52),   S(  2,  11),
            S( -2, -21),   S(  3,  -7),   S(-41,  -9),   S( -1,  28),   S(-19,  25),   S(-10,  32),   S(-21,  46),   S(  8,  18),
            S(-17, -46),   S(  7,  17),   S(  8,  -5),   S( 25, -14),   S( 17,  11),   S(  1,  28),   S(  8,  32),   S( 23,  32),
            S(-13, -26),   S(-12,  -2),   S(-27,   6),   S( 53,  11),   S( 12,  68),   S( 44,  44),   S( 22,  43),   S(  0, -29),
            S( -2, -13),   S(-22, -30),   S(  3, -26),   S(-22,  24),   S(  1,  22),   S( 41,  35),   S( -8,  17),   S(  6,  35),
            S(-16, -14),   S( -2, -44),   S( -7,  12),   S(  8,  -3),   S(-12,  -2),   S( 17,  15),   S( 26,  64),   S( 13,  26),
            S( -4, -11),   S(-14, -37),   S(  7,  -2),   S(-24,  -2),   S( -4,   9),   S(-29,  -9),   S( 12,   0),   S(  6,  23),

            /* bishops: bucket 11 */
            S(-13,   7),   S(-36,  -7),   S(-46, -50),   S(-33, -19),   S(-17, -42),   S(-84, -77),   S(-12, -17),   S(-24, -32),
            S(-15,  -5),   S(  9, -24),   S(-26, -10),   S(-21, -39),   S(-24,  -3),   S(-25, -64),   S(-16, -45),   S(-30, -40),
            S(-15, -45),   S( 12, -56),   S( -4, -28),   S(  6,   1),   S(-17, -18),   S(-37,  26),   S(-16,  50),   S( -4,  30),
            S(-25, -18),   S( -8, -52),   S( -6, -11),   S(-25,  -8),   S( 38, -10),   S(  0,  68),   S( 13,  74),   S( 19,   9),
            S( -8, -35),   S(-24, -10),   S(-22,  34),   S( 52, -22),   S( 66,  34),   S( 14,  76),   S(  2,  35),   S(  3,  25),
            S(-17, -69),   S(-27, -12),   S( -3, -36),   S(  6,  31),   S( 19,  36),   S( 34,  60),   S( 34,  55),   S(-18,   5),
            S( -6, -17),   S( -8, -32),   S(-18, -25),   S( -4, -32),   S(  5,  17),   S( 32,  -1),   S( 19,   0),   S( -2,  -1),
            S(-11,  -6),   S(-27, -48),   S(-12,   9),   S(  3,   1),   S( 14,  30),   S(-31, -52),   S(  2,  25),   S( 15,  18),

            /* bishops: bucket 12 */
            S(  3,  11),   S(-12, -24),   S( -8, -30),   S(-12, -33),   S( -6,  -7),   S(-10, -29),   S(  1,  24),   S(  8,  31),
            S( -6,  -9),   S( -9, -33),   S( -4,  10),   S( -5,  -2),   S( -9, -23),   S( -4,  13),   S( -1,  13),   S(  0,  -1),
            S( -4,   0),   S(-20,  22),   S(-13, -16),   S( -1,  10),   S(  1,  18),   S( -5,  -8),   S(-12, -50),   S( -7, -20),
            S(-10, -19),   S(  1, -12),   S(-17, -44),   S(-13,  -5),   S( 14,  32),   S(  0,  15),   S( -5, -24),   S(-10, -26),
            S(  3,  -7),   S( -4,  20),   S(  6,  42),   S(-15,  -3),   S(  0, -11),   S( -9,  -9),   S(  6, -11),   S( -2,   7),
            S(-15, -17),   S( -8,  35),   S(-21,  11),   S( -8,  11),   S(  8, -40),   S(  5,  23),   S( -4,  -3),   S(  1,  -2),
            S( -4, -11),   S(  2,  22),   S(  9,  32),   S( -7,  13),   S(  2,  11),   S( 11,  22),   S(-11, -24),   S( -3,   4),
            S( -1,   0),   S(  4,   1),   S(  0,  12),   S(  4,   8),   S(  0,  16),   S( -1,   1),   S(-16,  -5),   S( -2,  -1),

            /* bishops: bucket 13 */
            S(-14, -41),   S(-18, -33),   S(-15,  -1),   S(-12,  -9),   S(-18, -28),   S(-18, -11),   S(-10,  -5),   S( -8, -14),
            S( -4, -14),   S( -5, -16),   S(-13, -26),   S(-17, -14),   S( -7,  16),   S(-10,   9),   S(  3,  -4),   S(  2,   8),
            S( -8,   5),   S(  0,  -9),   S( -3,   9),   S(-19,  27),   S(-11, -28),   S(  6,  -6),   S( -9, -31),   S(  6,  24),
            S(  2,   8),   S(-27, -23),   S( -5,   3),   S(-23, -12),   S(-13,  -8),   S( -6,  19),   S( -3, -10),   S(-12, -11),
            S(  0,  27),   S(-25,   9),   S(-17,  41),   S( 29,   3),   S(  4,   8),   S( -1,  -9),   S(-21, -33),   S( -6, -19),
            S( -3, -10),   S( -4,  20),   S(-12, -18),   S(  9,   4),   S( 11,  22),   S( 19,  35),   S(  7,  26),   S( -7,  -4),
            S( -8, -12),   S( -8,   3),   S(  8,  18),   S(-11,  14),   S(-21, -19),   S(  3, -10),   S(-15, -31),   S( -2,  -4),
            S( -9, -14),   S(  1,  14),   S( -1,  -3),   S( -6, -42),   S(  0,  -2),   S( -4,  19),   S( -1,  20),   S( -3, -13),

            /* bishops: bucket 14 */
            S( -9, -14),   S(-22, -25),   S(-18, -45),   S(-22, -35),   S(-13, -47),   S( -8, -20),   S(-18, -32),   S( -7, -12),
            S(-13, -17),   S(  1,  -2),   S( -1,  16),   S(-37, -49),   S(-13, -30),   S(-21, -12),   S(-18, -12),   S(  7,  -8),
            S( -5, -17),   S( -6, -40),   S(-19, -15),   S(-13, -41),   S(-33, -22),   S(-16, -14),   S( -7,   6),   S( -6,  -7),
            S( -9, -34),   S( -3,  13),   S( -4, -12),   S(-20,  25),   S(-10,  -5),   S(-15,  14),   S(-27, -26),   S( -6, -13),
            S( -8,  -2),   S( -5,  35),   S( -3,   8),   S(  0, -22),   S(-16,  -3),   S( -5, -13),   S(  8,   7),   S( -2,   1),
            S(  0,  -3),   S(-10,   0),   S(-21, -12),   S( -9, -13),   S( -2,   8),   S( -7,  15),   S(-17,  -5),   S( -7, -11),
            S(-10, -26),   S( -8, -15),   S( -8,   4),   S( -5,  22),   S( -7,  17),   S(  6,  16),   S( -7, -21),   S( -8,  -9),
            S( -6, -15),   S( -5,  -2),   S( -9, -15),   S( -1,   2),   S(-16, -30),   S( -3,  -8),   S( -9, -12),   S(  0,   5),

            /* bishops: bucket 15 */
            S(  5,  22),   S(  2,  -5),   S(-18, -19),   S( -1, -26),   S(-17, -16),   S(-15, -30),   S( -4, -10),   S( -1, -11),
            S(  3,  -5),   S(  0,  -5),   S(  9,  -7),   S( -7,  18),   S( -8, -14),   S( -5,   1),   S( -8, -30),   S( -2,   3),
            S( -7,  -9),   S( -3, -12),   S(-22, -24),   S(-16,  -3),   S(-29, -14),   S(-11,  -8),   S(-10, -17),   S(  3,  15),
            S( -7, -22),   S(-13, -16),   S(  2,  -9),   S(-25, -37),   S(-12,  14),   S( -1,  13),   S( -1,   1),   S( -4, -26),
            S( -7, -15),   S(-10,  -4),   S( -7,  -9),   S(-21, -56),   S( 11,  -3),   S( -3,  27),   S(  2,  11),   S( -6,  -5),
            S( -7, -33),   S(-16, -12),   S( -4,  16),   S(-20,   0),   S(-17, -23),   S( -8, -22),   S( 16,  44),   S(  1,   2),
            S( -4,  -1),   S(  8,   6),   S( -1,  -5),   S( -3,  -6),   S(-13, -25),   S( -9,   7),   S(-10,  19),   S(  3,   0),
            S(  0,   7),   S( -3,   0),   S( -8, -19),   S( -9,  -8),   S( -1,  14),   S(-21, -37),   S(-10, -26),   S(  2,   2),

            /* rooks: bucket 0 */
            S(-20, -26),   S(-18,  -4),   S( -2, -31),   S(  7, -21),   S( -4,  16),   S(-10,   4),   S(  4,   3),   S(  3,  16),
            S( 31, -73),   S( 41, -52),   S( 16, -27),   S( -3,  -4),   S( 25,   3),   S(  3, -18),   S(-31,  13),   S(-51,  25),
            S( 11, -22),   S( 31, -10),   S( 38,  -2),   S(  9,  20),   S(-10,  37),   S( 21, -13),   S(-42,  48),   S(-54,  29),
            S( 23, -25),   S( 69,  -1),   S( 41,   8),   S( 37,  19),   S( 34,   5),   S( -7,  14),   S(-14,  14),   S(-48,  50),
            S( 66, -39),   S(105,  -6),   S( 71,   3),   S( 36,  -6),   S( 39,  29),   S( 27,   5),   S(  8,  20),   S(-37,  41),
            S( 65, -55),   S( 84, -49),   S( 53,  10),   S(-18,  53),   S( 14,  38),   S(-28,  43),   S( 35,  26),   S(-53,  53),
            S( 51, -12),   S(107, -25),   S( 51,  -6),   S(-16,  50),   S(-28,  54),   S( -1,  25),   S(-16,  22),   S(-50,  30),
            S( 53,  26),   S(  0,  48),   S(  7,  49),   S( -9,  46),   S( 24,  35),   S( 17,   5),   S( -5,  41),   S(-15,  43),

            /* rooks: bucket 1 */
            S(-80,  28),   S(-52,  12),   S(-60,  -3),   S(-43, -15),   S(-27, -24),   S(-30, -17),   S(-35,  -9),   S(-46,  26),
            S(-49,  -8),   S(-61,  -3),   S(-31,  -9),   S(-24, -36),   S(-28, -15),   S(-32, -27),   S(-57, -20),   S(-65,  -1),
            S( -6,   7),   S(-43,  41),   S(-31,   7),   S(-42,  14),   S(-42,  24),   S(-19,  -3),   S(-26,   8),   S(-47,  13),
            S(-31,  41),   S(-57,  39),   S( -5,  18),   S(-43,  33),   S(-34,  33),   S(-23,  32),   S(-28,  40),   S(-48,  21),
            S( 38,  21),   S( 28,  24),   S(  2,   3),   S(-40,  43),   S(-12,  26),   S( -6,  35),   S(  0,  16),   S(-55,  18),
            S( 24,  16),   S( -1,  44),   S(-21,  30),   S(-18,   3),   S( 11,   5),   S(-46,  43),   S( -3,  15),   S(-56,  32),
            S(-23,  35),   S(-34,  54),   S( 30,  30),   S(-63,  71),   S(-50,  44),   S( 33,  18),   S(-41,  28),   S(-58,  31),
            S( 32,  36),   S(  1,  41),   S(-22,  32),   S(-49,  48),   S( 22,  14),   S( 18,  46),   S(-49,  38),   S(  4,   9),

            /* rooks: bucket 2 */
            S(-70,  44),   S(-48,  20),   S(-46,  20),   S(-59,  17),   S(-51,   7),   S(-54,   5),   S(-42, -21),   S(-53,  28),
            S(-88,  47),   S(-50,  25),   S(-51,  27),   S(-55,   6),   S(-50,   4),   S(-51,   0),   S(-75,  -2),   S(-65,  15),
            S(-67,  61),   S(-61,  60),   S(-43,  52),   S(-37,  12),   S(-50,  35),   S(-39,  22),   S(-20,  13),   S(-44,  35),
            S(-83,  68),   S(-72,  76),   S(-43,  57),   S(-28,  50),   S(-29,  39),   S(-13,  36),   S(-44,  57),   S(-28,  38),
            S(-43,  59),   S(-48,  66),   S(-38,  48),   S(-25,  33),   S( 34,  25),   S( 12,  26),   S(-24,  48),   S(-44,  48),
            S(-27,  49),   S(-39,  55),   S(-15,  26),   S(  2,  18),   S( 23,  26),   S( 74,   4),   S(  5,  36),   S(-43,  49),
            S(-69,  51),   S(-66,  70),   S(-34,  55),   S(  6,  40),   S( 41,  18),   S( 24,   9),   S(-60,  74),   S(-24,  47),
            S(-45,  79),   S(-20,  50),   S(-69,  63),   S(-22,  55),   S(-38,  51),   S(-39,  65),   S(-54,  75),   S(-27,  44),

            /* rooks: bucket 3 */
            S( -3,  80),   S( -1,  71),   S(  6,  60),   S( 11,  49),   S( 10,  49),   S( -9,  61),   S( -7,  78),   S(-12,  41),
            S(-48, 100),   S(-14,  74),   S( -8,  73),   S(  8,  55),   S( 15,  52),   S( 18,  52),   S( 35,  -2),   S( 13, -22),
            S(-33,  78),   S(-11,  83),   S(  3,  82),   S(  7,  59),   S( 13,  81),   S( 25,  69),   S( 30,  61),   S( -4,  53),
            S(-39, 103),   S(-29,  89),   S( 18,  74),   S( 30,  69),   S( 18,  82),   S(  2, 110),   S( 56,  70),   S( 12,  72),
            S(-30, 108),   S( 19,  89),   S(  5,  74),   S( 23,  80),   S( 36,  79),   S( 49,  62),   S( 80,  58),   S( 55,  53),
            S(-22,  99),   S(  2,  86),   S( 20,  66),   S(  6,  74),   S( 26,  57),   S( 36,  53),   S( 88,  34),   S( 69,  36),
            S(-41, 105),   S(-30, 105),   S( -8, 100),   S( 33,  75),   S(  3,  93),   S( 20,  87),   S( 54,  73),   S( 98,  26),
            S(-66, 153),   S(  2, 100),   S( 20,  77),   S( 45,  68),   S( 63,  49),   S( 65,  49),   S(120,  51),   S( 75,  58),

            /* rooks: bucket 4 */
            S(-86,  38),   S(-20,  19),   S(-67,  21),   S(  4,  11),   S(-33,  -8),   S(  3, -48),   S( 26, -51),   S( 15, -33),
            S(-37, -15),   S(-23,  -3),   S(-35,  24),   S(-63,  51),   S( -1,  -7),   S( 11, -13),   S( 14, -23),   S(-26,  -6),
            S(  3,  -3),   S(-33, -45),   S( -7,  11),   S( 15, -22),   S(  7,  -8),   S(-21,  11),   S( 52,  -5),   S(-47, -11),
            S(-43, -16),   S( 10,   4),   S(-28,   9),   S(  0,   6),   S( 26,  22),   S( 12, -38),   S(-27,   3),   S( -3,  17),
            S(-33, -38),   S(-12,  24),   S( 12,  25),   S( 75,  15),   S( 36,  18),   S( -8,  10),   S( 41,  28),   S( 18,   8),
            S( 17,  24),   S( 43,  -5),   S( 43,  23),   S( 35,  17),   S( 54,  15),   S( 25,  41),   S( 21,   2),   S( 36,  56),
            S(-17,  10),   S( 51,  34),   S(  5,  37),   S( 30,  28),   S( 53,  13),   S( 20,  19),   S( 41,   9),   S( 41,  43),
            S( 47, -69),   S( 64,  42),   S( 33,  23),   S( 31,   2),   S( 22,   6),   S(  3,  22),   S( 39,   1),   S( -7,  29),

            /* rooks: bucket 5 */
            S(-37,  35),   S(-46,  63),   S(-39,  28),   S(-54,  38),   S(-23,  24),   S(-35,  41),   S( -1,  31),   S(-32,  38),
            S( -7,  33),   S(-65,  63),   S(-63,  69),   S(-47,  36),   S(  7,  11),   S( -1,   3),   S( -8,  34),   S( -9,  21),
            S(-34,  55),   S(-69,  61),   S(-64,  62),   S(-57,  56),   S(-18,  29),   S(-27,  38),   S(-10,  38),   S( -8,  40),
            S(-48,  77),   S(-17,  64),   S(-19,  45),   S( -6,  43),   S(-19,  66),   S(-29,  74),   S(  7,  36),   S( 13,  25),
            S( -1,  64),   S(  7,  67),   S( 29,  37),   S( 14,  59),   S( 46,  60),   S( 28,  76),   S( 76,  42),   S( 37,  41),
            S( 73,  58),   S( 14,  80),   S( 67,  56),   S( 11,  79),   S( 52,  57),   S( 42,  71),   S( 40,  52),   S( 31,  56),
            S( 64,  36),   S( 32,  56),   S( 30,  67),   S( 69,  34),   S( 43,  36),   S( 34,  56),   S( 74,  45),   S( 84,  36),
            S(102,  35),   S( 83,  32),   S( 50,  67),   S(  6,  52),   S( 46,  42),   S( 79,  27),   S( 29,  42),   S( 46,  35),

            /* rooks: bucket 6 */
            S(-44,  27),   S(-39,  39),   S(-24,  33),   S(-16,  13),   S(-42,  28),   S(-85,  64),   S(-25,  51),   S(-11,  46),
            S(-37,  30),   S(-28,  44),   S( -8,  32),   S(-46,  34),   S(-75,  61),   S(-93,  78),   S(-46,  43),   S( 39,   6),
            S(-34,  60),   S(-59,  52),   S(-11,  51),   S(-70,  54),   S(-15,  44),   S(-34,  61),   S(-55,  75),   S( 34,  35),
            S(-50,  80),   S( 41,  42),   S(-51,  77),   S(  7,  53),   S(  9,  40),   S(-32,  70),   S(-30,  75),   S(-15,  53),
            S(-13,  82),   S( 19,  79),   S( 27,  53),   S( 47,  38),   S(  8,  69),   S( 22,  61),   S( 27,  60),   S( 28,  61),
            S( -8,  75),   S( 51,  59),   S( 76,  31),   S( 38,  40),   S( 15,  51),   S( 35,  59),   S( 59,  62),   S( 55,  58),
            S( 49,  59),   S( 47,  53),   S( 65,  48),   S( 85,  23),   S(118,  19),   S( 69,  49),   S( 24,  58),   S( 57,  52),
            S( 22, 101),   S( 24,  62),   S( 46,  48),   S(  6,  66),   S( 33,  58),   S( 72,  56),   S( 62,  61),   S( 24,  70),

            /* rooks: bucket 7 */
            S(-67, -17),   S(-28, -25),   S(-45, -20),   S(-22, -17),   S(-17, -13),   S(-48,  25),   S(-43,  -7),   S(-13,  -2),
            S(-99,  30),   S(-52,   6),   S(-42,  10),   S( -4, -28),   S(-26,  16),   S(-13,  12),   S(-34,   4),   S(-71,  17),
            S(-106,  69),  S(-52,  20),   S(  4,   3),   S( -6, -12),   S(  4,   4),   S(-29, -13),   S(-20,  -2),   S( 24,  -7),
            S(-41,  34),   S( -2,  23),   S(  7,  15),   S( 41,   2),   S(  9,   8),   S( 31,   8),   S(  4,   1),   S( -8,  -7),
            S(  5,  28),   S( 34,  11),   S( 25, -12),   S( 48,  -4),   S( 59,  11),   S( 99, -11),   S( 62,  21),   S( 39, -19),
            S(-42,  42),   S( 15,  15),   S( 69, -24),   S( 94, -30),   S( 71, -10),   S( 81,   9),   S( 92,  12),   S( 35,  -9),
            S(  2,  33),   S( 35,   3),   S( 57,  -5),   S( 63,   3),   S(117, -13),   S(108, -22),   S( 43,  24),   S( 25,   8),
            S(-21,  71),   S(-28,  39),   S( 59,  -7),   S( 98, -38),   S( 53,   1),   S( 22,  12),   S( 52,   6),   S( 71,  -3),

            /* rooks: bucket 8 */
            S(-41, -25),   S(-22,  -9),   S(-14,   2),   S( -8, -22),   S(-14, -38),   S(-20, -45),   S( -8, -50),   S( 13,  -6),
            S( -6, -31),   S(-15, -18),   S(  0, -18),   S( 18, -33),   S( -9, -44),   S(-23, -26),   S( -6, -48),   S(-13, -64),
            S( 27,  27),   S(  1, -13),   S(  6, -10),   S( 17,   5),   S( -5, -37),   S(  8, -18),   S(  4,  -1),   S( -8, -21),
            S( -3, -15),   S(-20,  21),   S( -3,  -8),   S( 19,  15),   S(  4,  22),   S(-13, -10),   S( 23, -13),   S(  2,   6),
            S( -4, -21),   S( 16,  14),   S( -4,  43),   S( 26,   5),   S(  8,   6),   S( 15,   3),   S(  3, -22),   S(  9, -46),
            S(-11,   8),   S( -6,   9),   S( 26,  43),   S( 34, -27),   S( -5, -42),   S(  3, -34),   S( 17,  21),   S(  1,  30),
            S( 17, -21),   S( -5, -38),   S( 36,  13),   S( 21,  -2),   S( 46,   7),   S( 14, -15),   S( 26, -14),   S( 20, -25),
            S(  1, -183),  S( 21, -12),   S( 35,  13),   S( 14,  -9),   S( 11,  -4),   S(  9, -29),   S(  2,   0),   S( 25,  -8),

            /* rooks: bucket 9 */
            S(-60,   3),   S(-19, -31),   S(-22, -11),   S(-54, -17),   S(-47,  22),   S( -2, -13),   S(  7, -23),   S(-47, -45),
            S(  4, -19),   S( -1, -23),   S(-24, -31),   S( -9, -20),   S( -7, -15),   S( 15,  -1),   S( 14, -18),   S(-11, -44),
            S( 14,   1),   S( 19,  -9),   S( 27,   3),   S(-14,   2),   S(  0, -15),   S( 28,  -9),   S( 13,  28),   S(  5, -20),
            S( -3,  16),   S(  9,   7),   S( 26,  26),   S( 14,  14),   S(  1,  -9),   S( 22,  -4),   S(  9,  32),   S(  7, -26),
            S( 23,  21),   S( 15,  11),   S( -5,   3),   S( 11,  12),   S( 31,  13),   S( 26,  51),   S( 14, -23),   S( 23, -14),
            S( 20,  43),   S(-13,  22),   S( -7,   9),   S(-14,  18),   S( 18,  -9),   S( 42,   1),   S(  2,  33),   S( 14,  -1),
            S( 87,  14),   S( 57,  15),   S( 28,  46),   S( 50,   4),   S( 41, -14),   S( 18,   6),   S( 38,   3),   S( 43,  32),
            S( 65, -76),   S( 38, -36),   S( 18,  13),   S( 36,  27),   S( 12,  58),   S( 19, -10),   S( 36,  13),   S( 30,  26),

            /* rooks: bucket 10 */
            S(-64, -81),   S(-16, -37),   S(-41, -10),   S(-26,  -6),   S(-42,  -6),   S(-60,  -9),   S( 24, -26),   S( -9,  -9),
            S( 17, -13),   S( 10, -33),   S( 20, -37),   S(  0,  17),   S( -9, -12),   S(  5, -29),   S( 39,  -5),   S(  3,  19),
            S(-10,  -9),   S(-12, -15),   S( -1,   7),   S( 25,  -9),   S(-49,  26),   S( -3,  -5),   S( 29,  32),   S(  2,  -3),
            S(-13,   6),   S( 30,   3),   S( 14,  -2),   S(  5,  17),   S( 25, -20),   S( 26, -21),   S( 27,  27),   S(  3,   0),
            S( -5,  10),   S( 23, -21),   S(  6,  -4),   S( 16, -24),   S(-15, -27),   S(  8,  -8),   S( 31,  51),   S(  9,  23),
            S( 39,  38),   S( 47,  43),   S( 19,  34),   S( 34,  16),   S( -6, -14),   S( 21,   2),   S( 30,  10),   S( 28,  43),
            S( 60,  23),   S( 81,  -5),   S( 94,  -2),   S( 71, -15),   S( 47, -15),   S( 45,  -5),   S( 19, -14),   S( 20,  16),
            S( 73,  30),   S(  4, -20),   S( 26,  -9),   S( 26,   6),   S( 45,  10),   S( 48,  25),   S( -5,  -4),   S( 14,  -4),

            /* rooks: bucket 11 */
            S(-44, -49),   S( -8, -26),   S(-22, -18),   S(-19, -52),   S( -3, -17),   S(  4,  -5),   S(-17, -18),   S(-44,   1),
            S(-17, -40),   S(-11, -54),   S( -6, -30),   S(-31, -40),   S(-16, -44),   S(-15, -30),   S(-12, -19),   S( -9,  22),
            S( -1, -27),   S( 31, -12),   S( 19,   4),   S(  9, -18),   S( -2, -31),   S(-21,  18),   S(-39, -42),   S(-17, -65),
            S(  3,  17),   S( -9, -20),   S(  1,  24),   S(  4, -19),   S(-15, -14),   S(  6,  17),   S( 30,  -3),   S( -2, -12),
            S( 13,   1),   S( 34, -23),   S( 25,  22),   S( 19,   1),   S( 39,  -4),   S( 42, -12),   S( 10,  14),   S(-18, -32),
            S( 35,  33),   S( 51,  -3),   S( 27,   3),   S( 40,  18),   S( 74, -14),   S( 55,   5),   S(-13,  10),   S(  6,  11),
            S( 60,  30),   S( 72,  15),   S( 84, -24),   S( 90,  -6),   S( 63,  12),   S( 47,  21),   S( 41,  60),   S( 55,   4),
            S( 58,  29),   S( 19,  30),   S( 24,   0),   S( 26,   3),   S(-17, -14),   S( 23,  22),   S( 11,  -9),   S( 40,  -4),

            /* rooks: bucket 12 */
            S(  4,  -3),   S(-21, -53),   S( -6, -41),   S(  0,  -7),   S(  6,  -6),   S( -4, -54),   S(-34, -78),   S(-23, -44),
            S( 23,  11),   S(-11, -32),   S( -6, -10),   S(-21, -50),   S(  2,  11),   S(  1,  -2),   S(  2,   8),   S( -8, -29),
            S( 11,  24),   S(-12, -26),   S(-11, -35),   S(-20, -18),   S( -8, -32),   S(  4, -27),   S(  1,  -1),   S( -2, -28),
            S( -6, -16),   S(-16, -27),   S(  3,  34),   S(  4, -40),   S( -3, -20),   S( -3, -16),   S( -3,   8),   S( -2, -32),
            S( -1,   2),   S(  3, -29),   S( 22,  21),   S( 10,  23),   S( -8, -37),   S(  6, -18),   S(-13, -21),   S( -5, -26),
            S(  1,  -5),   S(  7, -11),   S( 33,  61),   S( 10,  -6),   S( -7, -22),   S( -4, -19),   S(  8, -32),   S(  4,  10),
            S(-15, -24),   S( 10, -11),   S(  0, -42),   S( 17,   5),   S( 20,  14),   S( -7, -29),   S( -3, -22),   S(  5, -14),
            S(-10, -44),   S( -2, -12),   S( 21,  14),   S( -1, -30),   S(  2, -40),   S(  0, -31),   S( -4, -19),   S( 18,  13),

            /* rooks: bucket 13 */
            S(-12, -38),   S(-22, -75),   S( -2, -17),   S(  2,  18),   S( 10,  10),   S(-14, -27),   S(  3, -15),   S(-28, -63),
            S( -1,  -3),   S( -6, -11),   S(-10,   1),   S( -2,  27),   S(-18, -53),   S( -4,  -8),   S(  6, -10),   S( -6, -29),
            S(-16, -85),   S( -9, -21),   S(  0, -33),   S(  4, -35),   S( -3, -14),   S(  6,  15),   S(  9, -28),   S( -4, -43),
            S(-16, -66),   S( 15,  25),   S(-13, -24),   S(  0, -19),   S( 11,   1),   S(-17, -35),   S( -3, -45),   S( -3, -20),
            S( 21, -14),   S( 14,  -2),   S( 29,  41),   S(-10, -22),   S(-23, -46),   S( -4,  -1),   S(  1, -34),   S(  0, -23),
            S( -1, -38),   S( 14, -15),   S(-36, -38),   S(  9, -13),   S( 14,  10),   S(  2,  18),   S(  4, -21),   S( -1,   8),
            S(  7,  -8),   S(  8,  20),   S( -3,  -6),   S(  6,  -2),   S(  5, -55),   S( 24,   5),   S(  5, -20),   S(  3, -31),
            S(-18, -137),  S(-19, -92),   S(  9,  15),   S(-11, -11),   S( -3,   4),   S(  6, -17),   S( -8,  -9),   S( -2,  25),

            /* rooks: bucket 14 */
            S(-19, -52),   S(-25, -34),   S(  0, -14),   S(  1, -20),   S(  5, -33),   S( -8,   4),   S( 11, -18),   S(-16, -28),
            S(-20, -46),   S(-28, -63),   S(  1,   6),   S(-11, -18),   S(-10, -20),   S(-17, -42),   S(  8,  25),   S(  9,  -6),
            S( -4, -25),   S(-20, -32),   S( -7, -11),   S(-16,  -8),   S(  0,   1),   S( -2,  -3),   S(  9,  42),   S(  7,   3),
            S( 21,  19),   S( -8, -30),   S( -3, -28),   S(-12,   0),   S( -3, -24),   S(  3, -11),   S(-12, -34),   S( -5, -41),
            S( -3,  -5),   S( -2, -52),   S( -6, -39),   S(-11, -32),   S( -9, -30),   S(-11, -46),   S( -3, -22),   S(  9,  -1),
            S( 11,   4),   S(  0, -35),   S( -4, -10),   S( 14,  -5),   S(-12,  -5),   S(-14,  -4),   S(  6,   5),   S( -8,  -4),
            S( 15, -21),   S( -2, -61),   S(-10, -17),   S( -3, -26),   S(  9, -56),   S( -1, -12),   S( 22,  32),   S(  7,   6),
            S( -1, -42),   S( 12,   6),   S(-14, -18),   S(  9,   7),   S(-14, -31),   S( 11,  29),   S(  2,  21),   S(  0, -14),

            /* rooks: bucket 15 */
            S(-22, -79),   S( -9, -36),   S(-18, -44),   S(  2, -15),   S(  2,  -6),   S(  9,  -2),   S(-20, -47),   S( -5, -11),
            S(-20, -39),   S(-12, -22),   S( 11,  27),   S( -3, -35),   S( -7, -23),   S(  1, -28),   S( -9, -32),   S( 13,  36),
            S( -8, -13),   S( -4,   5),   S(  0, -17),   S(  2,   4),   S(  7, -11),   S(-13, -22),   S(  0,   7),   S( -7,  -5),
            S(  4, -26),   S(-10, -37),   S( -6,   0),   S(-11, -26),   S(-14, -44),   S( -5, -19),   S( -1, -22),   S(-11,   1),
            S(  9,  11),   S( -8,  -7),   S(  8,  -4),   S(  1, -21),   S( -2, -14),   S(  9,  15),   S( -4,   4),   S(  7,  29),
            S( 10,  13),   S(  1, -12),   S(  7,  -2),   S(-16, -48),   S(-16,  -8),   S( -2,  20),   S(  1, -20),   S(-16, -48),
            S(  5,  27),   S( 13,  10),   S( 13, -46),   S( -3, -39),   S( -6, -21),   S( 12,  50),   S( 18,   2),   S(  4,  17),
            S(  2, -39),   S( -6, -10),   S(  3, -33),   S(  8,  -1),   S( -4, -12),   S(-12, -49),   S(-13, -35),   S( -3, -23),

            /* queens: bucket 0 */
            S(-24, -12),   S(-19, -58),   S( 45, -106),  S( 41, -36),   S( 18, -13),   S( -6, -14),   S( 46, -14),   S( 13,  30),
            S(-12, -29),   S( 53, -79),   S( 26,  12),   S( 10,  24),   S( 17,  36),   S(  5,  34),   S( 22,  43),   S( 37,  -3),
            S( 17,  -6),   S( 29,  21),   S( 29,   5),   S(  4,  63),   S( 17,   7),   S(  0,  39),   S(  5,  42),   S( 23,  40),
            S(  7,  -2),   S( 23,  17),   S( -5,  43),   S( 15,  39),   S( 12,  59),   S(  4,  40),   S( 10,  40),   S( 16,  49),
            S( 37,  56),   S( 34,  27),   S( 38,  28),   S(  2,  85),   S( 11,  10),   S( -2,  12),   S( 20,   5),   S( 45, -25),
            S( -5,  74),   S( 15,  54),   S(  1,  14),   S( 33,   6),   S( 58, -22),   S( 20,  31),   S( 43,  15),   S( 13, -23),
            S( 19,  37),   S( 35,  63),   S( 16,  73),   S( 54,  42),   S( 14,   2),   S( -7, -36),   S(  5,  31),   S( 19,  34),
            S( 40,  23),   S( 30,  33),   S( 67,  33),   S( 71,  34),   S( 60,  15),   S(-34, -22),   S( 64,  56),   S( 53,  28),

            /* queens: bucket 1 */
            S(  2, -34),   S(-84,  -2),   S(-28, -21),   S(-19, -86),   S(  0, -43),   S(-26, -25),   S( 23, -32),   S( -9,  38),
            S(-16, -52),   S(-28, -49),   S(  3, -44),   S(-10,  31),   S(-12,  11),   S(  3,  11),   S( 26, -51),   S(-19,  53),
            S(-25,  57),   S( -5, -27),   S(  2,   8),   S( -2,   7),   S( -7,  62),   S(-26,  62),   S( 14,   8),   S( 26,  30),
            S( 13, -20),   S( 13,  31),   S(-25,  29),   S( 29,  29),   S(-16,  72),   S(  1,  27),   S(-10,  12),   S(  8,  35),
            S( 22, -19),   S(  0,  41),   S(-17,  70),   S( -3,  52),   S( -9,  50),   S( -5,  33),   S(-30,  30),   S(-14,  34),
            S(  4,  29),   S( 15,  38),   S( 31,  34),   S(-28,  65),   S( -2,  63),   S(-38,  48),   S(  6,  20),   S( 18,  17),
            S( -9,  59),   S(-18,  95),   S(  2,  41),   S(-21,  37),   S(-41,  29),   S( 35,  16),   S( -3,  39),   S( -3,  20),
            S(-28,  11),   S( -3,  -3),   S( 23,  29),   S( -6,  18),   S(  4,  30),   S(  1,   6),   S( 16,  25),   S(-15,  31),

            /* queens: bucket 2 */
            S( -7,  55),   S( 24, -53),   S(  6, -10),   S( -2,  -8),   S(-27,  25),   S(-36, -15),   S(-33, -17),   S( 13,   1),
            S( 13,  -3),   S( 22,  27),   S( 19,  -1),   S( 17, -32),   S( 21, -35),   S(  7, -25),   S( 17, -17),   S( 48, -38),
            S( 21,  10),   S( 15,  12),   S( 12,  50),   S(  8,  42),   S(  9,  65),   S( 14,  53),   S( 14,  14),   S( 21,  49),
            S(  8,  21),   S( -8,  63),   S( -5,  42),   S(  9,  51),   S(-17,  98),   S(  2,  51),   S( 19,  22),   S(  7,  54),
            S(  6,  14),   S( -4,  54),   S(-25,  78),   S(-46, 120),   S(-42, 123),   S( -8,  87),   S(-24, 132),   S( -3,  84),
            S(  8,  59),   S(-13,  69),   S(-26,  78),   S(  7,  40),   S(-17,  83),   S(-29, 120),   S(-17, 122),   S( 18,  52),
            S(-24,  66),   S(-39,  87),   S(-17,  74),   S(-25,  64),   S(-10,  83),   S( 55,  62),   S(-27,  52),   S(-12,  55),
            S(-57,  93),   S( 21,  41),   S( 43,  34),   S( 49,  17),   S( 38,  68),   S(  9,  47),   S( 23,  26),   S(  5,  19),

            /* queens: bucket 3 */
            S( 84,  80),   S( 57, 107),   S( 45, 101),   S( 42,  94),   S( 69,  28),   S( 40,  26),   S(  6,  27),   S( 47,  22),
            S( 62, 124),   S( 67, 107),   S( 47, 113),   S( 47,  85),   S( 49,  83),   S( 62,  53),   S( 73,  13),   S( 28,  40),
            S( 64,  93),   S( 50,  99),   S( 64,  73),   S( 56,  69),   S( 57,  95),   S( 58,  96),   S( 63,  92),   S( 68,  54),
            S( 48, 131),   S( 60,  90),   S( 43, 101),   S( 54,  89),   S( 44,  99),   S( 37, 133),   S( 66,  99),   S( 50, 128),
            S( 58,  95),   S( 61, 103),   S( 49,  88),   S( 31,  89),   S( 39, 117),   S( 27, 138),   S( 31, 167),   S( 45, 148),
            S( 57, 117),   S( 56, 107),   S( 40, 104),   S( 25, 110),   S( 28, 129),   S( 72,  99),   S( 65, 147),   S( 29, 200),
            S( 36, 122),   S( 52, 110),   S( 75,  90),   S( 57,  89),   S( 44, 108),   S( 47, 122),   S( 83, 140),   S(150,  67),
            S( 81,  96),   S( 86,  88),   S( 54, 116),   S( 74,  82),   S( 28, 116),   S(106,  50),   S(131,  77),   S(132,  69),

            /* queens: bucket 4 */
            S(  9, -13),   S(-32, -23),   S(-45,   0),   S(-26,  -7),   S(  5,  13),   S( 45,   0),   S(-13,   4),   S(-37,   8),
            S(-12,   3),   S(-28, -13),   S( 29, -13),   S( -6,  32),   S( -6,   3),   S(  7, -29),   S(  0,   8),   S(-39,   5),
            S( -2,  -4),   S(  0, -10),   S( 10,  36),   S( 15,  38),   S( 17,  -9),   S( 21, -11),   S( 14, -18),   S(-22, -22),
            S(-48, -18),   S(-11,   9),   S(  4,  29),   S( -1,  28),   S( 18,  14),   S( 40,  43),   S(-34, -36),   S(-10,  -8),
            S(-17,  12),   S( 27,  12),   S( 42,  42),   S( 61,  52),   S( 20,  44),   S(  9,  -1),   S(-11, -10),   S(  0, -28),
            S( 20,  17),   S( 36,  23),   S( 30,  64),   S( 50,  53),   S(  8,  -8),   S(  0,  -2),   S(-15, -23),   S(-18,   2),
            S(-13, -35),   S(  1,  26),   S(-27,  18),   S( 48,  61),   S(  2,  11),   S(-10, -16),   S(-29, -47),   S(-36, -29),
            S( -7, -14),   S( 26,  16),   S( -7,  21),   S( 18,  37),   S( -9,  -1),   S( 10,  12),   S(-21, -45),   S( 15,   7),

            /* queens: bucket 5 */
            S(-62, -36),   S(-35, -37),   S(-12, -29),   S(-32, -28),   S(-38,  -8),   S( 26,  -5),   S( -4, -19),   S(  4,  -3),
            S(-31,   9),   S(-45, -26),   S(-84, -32),   S(-63,   0),   S(-38, -32),   S(-28, -12),   S(-48, -11),   S(-36,  -9),
            S(-47,   2),   S(-90,  -2),   S(-61,   3),   S(-26,  30),   S( 17,  80),   S(-44,   1),   S( -7, -11),   S( -2,  13),
            S(-52, -20),   S(-78, -31),   S(  8,  70),   S( 13,  82),   S( 16,  18),   S(  9,  -2),   S(  2,  -3),   S(-12,  31),
            S(-23,  -5),   S(  2,  34),   S(-18,  66),   S( 22,  54),   S( 46,  50),   S(-26,  21),   S(  5,  -3),   S(  8,  -2),
            S(-31,  -3),   S( 23,  52),   S(-24,  21),   S(-26,  30),   S( 40,  53),   S( 23,   8),   S( 16,  -6),   S(  9,  15),
            S(  9,  16),   S( 12,  29),   S(-21,  44),   S( -8,  33),   S(-16,  45),   S( 20,  38),   S( 15,   7),   S(-27, -35),
            S( -8,  32),   S( 27,   7),   S( 11,  18),   S(-12,  22),   S(  5,  36),   S(  0,  15),   S( 15,   3),   S(-39, -39),

            /* queens: bucket 6 */
            S(-23,  -5),   S(-37, -30),   S(-32, -15),   S(-92, -57),   S(-92, -41),   S(-80, -50),   S(-74, -29),   S(-11,  26),
            S(-66,   4),   S(-37,  17),   S(-39,  15),   S(-53,  29),   S(-73,  22),   S(-112,   2),  S(-72, -30),   S( -5,  32),
            S(-25,  25),   S(  3, -20),   S(-68,  44),   S(-77,  74),   S(-51,  57),   S(-10,   6),   S(-59, -14),   S( -3,  19),
            S( -9,   7),   S( -7,  42),   S(  2,  48),   S(-64,  97),   S(  1,  52),   S( 17,  59),   S(-11,  34),   S(  6, -33),
            S(-35,  50),   S(-15,  -3),   S(-41,  35),   S( 15,  28),   S( 29,  62),   S( 54,  23),   S( 17,  51),   S(  6,  49),
            S(-42,  37),   S( 14,  37),   S( 15,  21),   S( 39,  43),   S(  8,  67),   S( 35,  67),   S( 11,  -7),   S(-24,  11),
            S( 15,  13),   S( -3, -17),   S( 15,  49),   S(-16,  30),   S( 52,  66),   S( 14,  39),   S(  2,  12),   S(-30,   1),
            S( 17,  22),   S(-11,   8),   S( 27,  34),   S(-20,   6),   S( 50,  28),   S( 12,  38),   S(-15,  18),   S( 19,  11),

            /* queens: bucket 7 */
            S( -5,  -8),   S(-36,   4),   S(-45,  35),   S(-32,  -4),   S(-35, -36),   S(-55, -24),   S(-35, -27),   S( -1, -32),
            S(-63,  30),   S(-75,  33),   S(-34,  20),   S( -1,  15),   S(-49,  50),   S(-46,  19),   S(-49,  47),   S(-17, -42),
            S(-52,  -5),   S(-52,  33),   S(-31,  47),   S(  2,  11),   S( 10,  33),   S(  4,   5),   S( -7,  20),   S( -6,   4),
            S(-85, -26),   S(-12,  -3),   S( -9,  12),   S(-34,  51),   S( 59,   1),   S( 47,   6),   S(-17,  70),   S(-16,   3),
            S(-48,  22),   S(-76,  40),   S(  6,   5),   S( 39, -13),   S( 39,   3),   S(105, -29),   S( 39,  -6),   S( 35, -28),
            S(-30,   4),   S(  0,  13),   S(  1, -19),   S(  5, -13),   S( 53,  40),   S( 77,  -4),   S( 81,   8),   S( 22,  22),
            S( 10, -17),   S( -7,  -6),   S(  7,   4),   S( 34,   7),   S( 45,  11),   S( 53,  43),   S( 61, -14),   S( 30,  19),
            S( 33,  -5),   S( -2,   9),   S( 14,  16),   S(  8,  23),   S( 36,  13),   S( 17,  28),   S(  3,   6),   S( 60,  71),

            /* queens: bucket 8 */
            S( -6, -14),   S( -2,   5),   S(-28, -17),   S(  4,   9),   S(-13, -21),   S(  4, -10),   S(-24, -41),   S( -7,   2),
            S(-11,   4),   S( -9, -13),   S( -2,   5),   S(-27, -25),   S( 10,   4),   S( -4,  -9),   S(-23, -39),   S(  0,   1),
            S(  1,  11),   S(-20, -13),   S(  0,  24),   S(  0,  -6),   S(-23, -10),   S(  9,  17),   S(-14, -28),   S(-15, -35),
            S(  0,  15),   S( 17,  19),   S( 12,  38),   S( 34,  29),   S(  0,   4),   S(-11,   5),   S(  0,   6),   S(  3,  -5),
            S( 17,  32),   S( 16,  44),   S( 16,  18),   S(  9,  19),   S( 17,  42),   S( 20,  25),   S(  0,  13),   S( -3,   0),
            S( 10,  23),   S( -9, -14),   S(-17,  14),   S( 17,  39),   S( -6,   3),   S( -3,  -8),   S(  0,   2),   S( -4,   4),
            S( -9, -25),   S(-42, -55),   S( 36,  55),   S( 15,  20),   S( -2,  28),   S( -9,   6),   S( -6, -22),   S(-10, -27),
            S(-14, -23),   S(  0,   7),   S( -7, -44),   S(-14, -13),   S(-14, -36),   S(  3,   3),   S(  1,  -8),   S( -4, -11),

            /* queens: bucket 9 */
            S( 12,  22),   S( -9, -16),   S(  9,  12),   S(-28, -37),   S(-20, -29),   S(-28, -45),   S(-13, -25),   S( -9, -16),
            S(  1,  -2),   S(-11,  -2),   S(-23, -34),   S(-11,  -5),   S(-29, -29),   S( -1,  -6),   S( -5,  -7),   S( -5,   0),
            S(  2,   0),   S(  9,  24),   S( -3,  29),   S( -1, -18),   S(-17, -11),   S( -2,   3),   S( -1,  -5),   S(-14, -12),
            S( -3,  -1),   S(  2,   9),   S( 21,  47),   S(  4,  22),   S( 25,  38),   S(  9,  15),   S( -8, -21),   S(  0,   0),
            S( -8,  -5),   S(  5,  23),   S(  6,  38),   S( 40,  72),   S( 12,  26),   S( 20,  30),   S( 13,  33),   S( -4,  -4),
            S(-19, -20),   S(-13,  -6),   S(  2,  40),   S(  4,  21),   S( -6,   0),   S(  6,  25),   S(-16, -13),   S( -9, -21),
            S(  0,  -8),   S( -7, -18),   S(-20,   9),   S(  9,  20),   S( 15,  26),   S(  7,  -9),   S( -5, -15),   S(-10, -40),
            S( -8,  -2),   S( 13,  -3),   S(-11, -22),   S(  5,  20),   S( 21,  29),   S(-11,   3),   S(  6,   5),   S( -3, -23),

            /* queens: bucket 10 */
            S(  8,   6),   S( -4,   2),   S( -5,  -6),   S(-22, -32),   S(  2,   4),   S( -8,  -6),   S(  6,  11),   S( -3,  -3),
            S( -8, -16),   S(-13, -23),   S(-13, -24),   S(  7,  10),   S(  2,   8),   S(-12, -16),   S( -7, -14),   S(-28, -35),
            S( -7, -24),   S(-13, -16),   S( -2,  17),   S(-13, -10),   S( -4,  -6),   S(-15,   6),   S(-10, -19),   S(-12, -12),
            S( -4, -11),   S(  8,   2),   S( 12,  17),   S( -6,   9),   S( 22,  34),   S(  4,  20),   S(-13, -27),   S(-24, -38),
            S(-17, -18),   S(-16, -33),   S( -7,  19),   S( 34,  51),   S( -1, -19),   S(  7,  30),   S(  1,   0),   S(  3,   8),
            S( -4,  -5),   S(-40, -53),   S( -7, -12),   S( -1,  33),   S( -4,   1),   S( 13,  18),   S(  6,   7),   S(-16, -26),
            S(  8,  -3),   S(  0, -14),   S( 16,  35),   S( -4,  -9),   S( -8, -24),   S( -9, -15),   S( -4,   2),   S(  1,   1),
            S( -3,   1),   S(  6,  -4),   S(  4,  -8),   S(  9,  -1),   S( 12,  17),   S( 14,  20),   S(  9,   9),   S( -2, -15),

            /* queens: bucket 11 */
            S(-14, -13),   S(-15, -29),   S(-16, -13),   S( -1, -17),   S(-16, -22),   S(-20, -28),   S( -7,  -6),   S(-22, -34),
            S(-23, -27),   S(-17, -17),   S(-41, -35),   S(  4,   6),   S(-17,  -6),   S(  7,  21),   S(  0,  -3),   S( -6,   8),
            S(-24, -32),   S( -5, -20),   S( -8, -26),   S(  9,  11),   S( -8, -16),   S(  1,  21),   S( 22,  29),   S(  0,  -4),
            S(-23, -31),   S(-31, -40),   S( -6, -19),   S( 21,  31),   S( -8, -12),   S( -7,  17),   S( 23,   8),   S(  0,  10),
            S( -9,   2),   S( -6, -30),   S(-12,   2),   S( 13,   7),   S( 15,  -1),   S( 16,  42),   S( 27,  40),   S( -8,  11),
            S( -8, -20),   S( -3,   7),   S( -6, -16),   S(  2,  14),   S( 32,   4),   S( 54,  37),   S( -6, -17),   S(  4,  11),
            S(-20, -45),   S(-33, -45),   S( 19,  19),   S(-23,  -8),   S(-10, -17),   S( 20,  20),   S( 50,  53),   S( -7, -24),
            S(-21, -39),   S(-33, -68),   S( -6,  -2),   S(-14, -23),   S( -7,   1),   S( 11,   6),   S( 20,  18),   S( -2, -31),

            /* queens: bucket 12 */
            S(  8,   1),   S(  1,   1),   S(-11, -16),   S(-13,   1),   S( -6,  -5),   S(  7,  13),   S(  3,   2),   S( -1,  -5),
            S(  1,  -3),   S( -8, -17),   S(-17, -19),   S( -4,   1),   S( -8, -10),   S( -3,   2),   S( -6, -19),   S( -8, -22),
            S(  0,   3),   S( -3,  -4),   S( 27,  36),   S( 10,  13),   S( -1,  -3),   S(-17, -34),   S(-16, -33),   S( -8,  -8),
            S(  5,   6),   S(  4,  14),   S( -1,  11),   S(  5,  16),   S(  8,  10),   S(  2,  -4),   S( -5,  -7),   S( -8, -17),
            S(  0,  -4),   S( 14,  23),   S( 28,  48),   S(  0,  14),   S(  3,  23),   S( -4,   9),   S(-14, -33),   S( -8, -23),
            S( 10,  18),   S(  7,   5),   S( 45,  48),   S( -7,  -7),   S(  0,  -4),   S( -1,  -4),   S(  5,   8),   S(-10, -25),
            S(  0,  -2),   S( 18,  27),   S(  8,  -2),   S( 30,  26),   S(  2,  12),   S( -4,   1),   S(  9,   7),   S( -8,  -6),
            S(-11, -47),   S(-16, -35),   S(-16, -29),   S(-12, -29),   S( -4, -28),   S(  5,   0),   S(  7,   1),   S( -6, -13),

            /* queens: bucket 13 */
            S( -2,  -3),   S( -1,  -7),   S(  6,   5),   S( -6,  -7),   S(  1,  -3),   S( -4,  -9),   S( -7,  -8),   S(-12, -16),
            S(  1,  10),   S(  7,  17),   S(  7,   7),   S( -5,  -1),   S(-15, -20),   S(  5,   3),   S( -4,  -4),   S( -5,  -8),
            S(  8,  15),   S(  4,  11),   S( -4,   6),   S(  6,  30),   S(-13, -20),   S( -5,  -7),   S( -8, -16),   S(-18, -22),
            S(-11, -13),   S(  1,   9),   S( 20,  12),   S( 21,  31),   S( 10,  16),   S( -7, -14),   S(-10, -19),   S(-14, -27),
            S(  1,   6),   S( -3,   5),   S( 14,  52),   S( 21,  44),   S( 25,  39),   S( -1,  -2),   S( -7, -12),   S(-14, -28),
            S( -4,  -5),   S( 17,  38),   S( 57,  92),   S( 29,  56),   S(  3,   9),   S( 10,   7),   S( 11,  22),   S( -5, -25),
            S(  3,   5),   S( 13,  11),   S( 12,  24),   S(  1,  16),   S(  3,   9),   S( 11,  -6),   S( -1, -11),   S(  6,   7),
            S(-30, -32),   S(  3,   5),   S(  0, -15),   S( -9, -16),   S( 10,  11),   S(  5,  11),   S(-11, -15),   S( -2,   1),

            /* queens: bucket 14 */
            S( -1,  -1),   S( -5, -15),   S(  1,  -7),   S(-10,  -3),   S( 13,  18),   S( -3,  -8),   S(  0,  -4),   S(-11, -27),
            S( -7, -10),   S(  7,  25),   S( -5,  -5),   S(  3,   0),   S( -4,   6),   S(-12, -26),   S(  1,   1),   S(  2,   1),
            S(  8,  16),   S( -6,  -5),   S(-13, -30),   S(  0, -10),   S(  9,   8),   S( -4,  -5),   S(  6,  14),   S( -6, -15),
            S( -3,  -6),   S(  4,  -9),   S( -3,   2),   S( 20,  39),   S( 16,   6),   S( 10,  25),   S( 16,  37),   S(  7,   4),
            S(  0,   9),   S(  5,  -1),   S( -9,   0),   S( 21,  31),   S( 24,  30),   S( 10,  20),   S( 12,  19),   S( -1,  -5),
            S( -8, -16),   S(  0,  -6),   S( 28,  46),   S(  7,  19),   S( 11,  29),   S( 10,  34),   S( 17,  40),   S( -5,  -8),
            S(  5,  15),   S( 12,  19),   S( 13,  28),   S( 22,  30),   S(  2,   3),   S( 18,  31),   S( 16,  28),   S(  5,   6),
            S( -4,   3),   S(  0,   2),   S( -7, -19),   S(-11, -19),   S( -7, -14),   S(  1,  -2),   S( -5, -13),   S( -6,  -1),

            /* queens: bucket 15 */
            S( -2,  -6),   S( 12,  17),   S( -2, -10),   S( -3, -11),   S( -5,  -9),   S( -6, -10),   S( -9, -22),   S( -2,  -8),
            S( 10,  -4),   S( -2,  -5),   S( -6, -21),   S( -6, -11),   S(  0,   9),   S( -1,  -1),   S( 10,  19),   S( -6, -20),
            S(  1, -10),   S( -4, -11),   S( -3,   4),   S( -5, -16),   S(  1,  -3),   S(  0,   6),   S( -8, -13),   S(  5,  -1),
            S( -7,   1),   S(  2,  11),   S(  2,   7),   S(  5,   5),   S(  4,   9),   S( -6,  -1),   S( 13,  24),   S( 11,  10),
            S( -3,  -8),   S( -8, -13),   S(-17, -28),   S( -6,  -5),   S(  1,   5),   S(  6,  -2),   S( -9,  -5),   S(  6,   4),
            S(  2,   0),   S(  0,  -2),   S(  5,   9),   S(-10, -14),   S(  2,   3),   S( 19,  24),   S(  4, -11),   S(  6,   2),
            S( -9, -14),   S(  5,  -5),   S(  8,   8),   S( -4, -12),   S(  2,  10),   S( 24,  35),   S( 20,  29),   S(  2,   0),
            S( -7, -17),   S( -2,  -1),   S(  5,  11),   S( 15,  19),   S(  1, -12),   S(  3,  -5),   S( -7, -17),   S(-14, -31),

            /* kings: bucket 0 */
            S( 82, -19),   S( 59,  47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 56,  32),   S(131,  54),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 45,  21),   S( -2,  41),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 83,  50),   S( 65,  62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-20,  36),   S(-13,  30),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 38,  65),   S( 41,  52),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -3,  47),   S(-18,  10),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 20,  81),   S(-37,  52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 28, -63),   S( 79, -31),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -25),   S(  5,  22),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 53, -18),   S( 28,  -1),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 22,  36),   S( -6,  36),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 50,  13),   S( 18,   9),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  6,  50),   S( -7,  45),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 80,  32),   S( 47, -16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 37,  75),   S( 10,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-70, -126),  S( 13, -57),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49, -108),  S(-99, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( -3, -49),   S(-34, -41),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-41, -27),   S(-43, -18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-19, -32),   S( -8, -34),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-44, -19),   S(-94,  12),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-35, -32),   S(-44, -107),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-88,   0),   S(-22, -87),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -95),   S(-69, -33),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-48, -250),  S(  0, -110),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-101, -31),  S( 55, -73),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-69, -73),   S(-19, -107),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-40, -38),   S(-123,  -6),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 26, -117),  S(-72, -67),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-134,   2),  S(-34, -121),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-52, -64),   S(-13, -221),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -31),   S(-27,  13),   S(  9,  -3),   S(-11,  24),   S( 25,   1),   S( 46,   3),   S( 44, -12),   S( 50,   1),
            S(-10, -28),   S(-30,   2),   S( -8, -14),   S(  2, -11),   S( 21,   6),   S(  1,  11),   S( 27,  -4),   S( 28,  20),
            S(  9, -35),   S( -8, -14),   S( 33, -36),   S( 12, -20),   S( 19, -11),   S(  7,  33),   S(  4,  42),   S( 36,  16),
            S( 12, -24),   S( 28,   2),   S( 58, -29),   S( 45, -13),   S( 30,  36),   S(-10,  77),   S(  8,  91),   S( 58,  60),
            S( 84, -47),   S(122,  -7),   S( 74,  -8),   S( 50,  -4),   S( 49, 152),   S(-49, 161),   S( 20, 148),   S( 69, 132),
            S(-216, -67),  S(-97, -156),  S( 49, -196),  S( 54,  51),   S( 99, 204),   S(116, 166),   S(118, 162),   S(112, 148),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46,  21),   S(-44,  20),   S(-23,  16),   S(-44,  49),   S(-17,   1),   S( 20,   7),   S( 10,  -3),   S( 13,  26),
            S(-52,  19),   S(-45,  12),   S(-34,   7),   S(-14,  12),   S(  4,  10),   S(-13,  12),   S( -4,  -3),   S(-13,  23),
            S(-51,  28),   S(-23,  21),   S(-19,   8),   S(  5,  -3),   S(  3,  14),   S(-16,  16),   S(-29,  27),   S( -8,  27),
            S(-44,  49),   S( 16,  21),   S(-20,  30),   S( 23,  28),   S(  7,  31),   S(-22,  42),   S(  9,  37),   S( 32,  56),
            S( -8,  43),   S( 62,   2),   S(120, -30),   S( 91, -17),   S( 66,   0),   S( -8,  39),   S(-41,  76),   S( 43,  98),
            S( 31,  60),   S(-33, -12),   S( 34, -120),  S(-17, -94),   S(-41, -51),   S( -2,  57),   S( 63, 182),   S( 68, 214),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,  40),   S(-32,  18),   S(-20,  12),   S(-11,  30),   S(-30,  27),   S(-13,  12),   S( -7,  -7),   S( -7,  20),
            S(-50,  32),   S(-45,  32),   S(-31,   7),   S(-25,  24),   S(-24,  18),   S(-39,  11),   S(-15, -15),   S(-35,  12),
            S(-43,  49),   S(-39,  51),   S(-11,  16),   S(-16,  18),   S(-23,  26),   S(-23,   7),   S(-27,   5),   S(-29,  10),
            S(-35,  91),   S(-36,  71),   S(-13,  46),   S(  5,  42),   S( -8,  41),   S(-25,  16),   S(  9,  17),   S( 23,  14),
            S(-33, 133),   S(-37, 120),   S(  4,  18),   S( 15, -25),   S(105, -18),   S(104, -26),   S( 62, -10),   S( 47,  11),
            S(-10, 245),   S( 34, 181),   S( 24,  70),   S( 20, -81),   S(-39, -164),  S(-80, -145),  S(-37, -42),   S( 27,  27),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1,  16),   S( -4,  12),   S(  8,  15),   S( -1,  41),   S( -8,  46),   S( 28,  26),   S( 14,   0),   S(  9, -17),
            S( -1,  22),   S( -4,  22),   S(  0,   7),   S(  2,  11),   S( 12,  18),   S( 13,   1),   S(  8, -16),   S(-17,  -6),
            S(  4,  38),   S(-10,  55),   S( 12,  22),   S(  3,   4),   S( 21, -10),   S( 18, -12),   S(  6, -22),   S(-14, -12),
            S(  1,  88),   S(-20, 108),   S(  9,  74),   S( 21,  37),   S( 29,   3),   S( 39, -30),   S( 26,   0),   S( 34, -21),
            S(  2, 158),   S( -9, 177),   S(-20, 164),   S(-10, 116),   S( 44,  44),   S( 66, -13),   S(126, -43),   S(102, -42),
            S(105, 132),   S( 34, 251),   S(  9, 272),   S(  2, 219),   S(-45,  99),   S(  6, -187),  S(-73, -244),  S(-146, -181),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 58,   7),   S(  8,   6),   S(-10,  -6),   S( -4,   8),   S(  6, -17),   S( 23, -20),   S(  8, -13),   S(-83,  57),
            S( 57,  -3),   S( 17,  11),   S(-19,   3),   S(-21,  -6),   S(-18, -21),   S(-30,  -7),   S(-41, -21),   S(-37,  -2),
            S( 71, -13),   S(111, -32),   S( 32, -23),   S(-28,  -4),   S(-69,  15),   S(  1,  -1),   S(-66,  16),   S(-60,  28),
            S(-88, -64),   S( -5, -102),  S( 60, -51),   S(-43,  23),   S(-14,  16),   S(-64,  65),   S( -1,  41),   S(-65,  79),
            S( -4, -74),   S(-97, -103),  S(-14, -75),   S( 48, -16),   S( 44,  89),   S( 18,  95),   S( 28,  71),   S(-16, 100),
            S( 14, -49),   S(-29, -66),   S(-17, -66),   S( -8,  36),   S( 63,  85),   S( 52, 143),   S( 46, 137),   S( 48, 112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-67,  49),   S(-35,  38),   S(  4,  20),   S( 63,  14),   S( 51,   2),   S( -1,  13),   S(-26,   7),   S(-55,  48),
            S(-92,  48),   S(-33,  39),   S(-24,  24),   S(-25,  32),   S(-15,  22),   S(-18,  10),   S(-57,   5),   S(-65,  31),
            S(-23,  28),   S(-31,  60),   S( 13,  35),   S( 30,  36),   S(-42,  52),   S(-54,  33),   S(-72,  34),   S(-64,  47),
            S(-44,  49),   S(-17,  14),   S(-36, -25),   S( 14, -27),   S(-14,   2),   S(-69,  36),   S(-32,  40),   S(-31,  60),
            S( 42,  17),   S(  6, -28),   S( 40, -96),   S( 13, -78),   S( 70, -52),   S( 39,  14),   S(-43,  86),   S(-66, 127),
            S( 63,  30),   S( 37, -12),   S(-33, -70),   S(-26, -56),   S(-22, -41),   S( 40,  42),   S( 49, 111),   S( 65, 144),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-107,  50),  S(-70,  23),   S( -8,   5),   S( -3,  26),   S( 12,  26),   S(  6,  20),   S(  7,   7),   S( -8,  30),
            S(-86,  30),   S(-65,  18),   S(-45,   5),   S( 11,  16),   S(-34,  41),   S(-20,  16),   S( -9,   7),   S(-14,  17),
            S(-62,  39),   S(-84,  45),   S(-47,  35),   S(-46,  48),   S(  4,  42),   S( 24,  16),   S( 28,   8),   S(-21,  22),
            S(-110, 100),  S(-56,  64),   S(-10,  28),   S(-31,  27),   S(-18, -29),   S(-20, -28),   S(-48,  22),   S( 25,   7),
            S(-37, 117),   S(-30,  73),   S( 26,   9),   S( -8, -43),   S(  6, -72),   S(-47, -68),   S(-32, -29),   S( 44,   5),
            S( 83,  80),   S( 87,  90),   S( 43,  17),   S( 30, -81),   S( -6, -116),  S(-72, -34),   S(-13, -57),   S( 74,  12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49,  -1),   S(-47, -17),   S( -4, -26),   S(-42,  39),   S( 36,  -9),   S( 63, -17),   S( 56, -25),   S( 60,  -7),
            S(-52,   2),   S(-68,   5),   S(-39, -16),   S(-30,  -2),   S(  8,  -4),   S( 34, -24),   S( 35, -22),   S( 49, -12),
            S(-68,  35),   S(-92,  39),   S(-39,   9),   S(-61,   7),   S(-13,   1),   S( 18, -13),   S( 65, -16),   S( 49, -13),
            S(-47,  61),   S(-99,  87),   S(-42,  57),   S(-35,  46),   S( -1,  -5),   S( 30, -58),   S( 26, -72),   S( 15, -105),
            S( 11,  61),   S(-55, 139),   S( 10, 121),   S(-32,  95),   S( 47,  -5),   S(  9, -87),   S(-47, -132),  S( 17, -112),
            S(151,  78),   S( 72, 146),   S( 70, 113),   S( 79,  82),   S( 47,  26),   S(  5, -131),  S(-14, -58),   S(-29, -169),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   5),   S(  9,  22),   S( 22,  25),   S(-18, -55),   S(-31, -59),   S( -8, -19),   S( 12, -52),   S( 46, -46),
            S(  0, -59),   S(  0,  -6),   S(-31, -52),   S(-67, -10),   S(-34, -48),   S( 34, -68),   S( 14, -75),   S(-35, -57),
            S( 26, -91),   S( 26, -55),   S(-28, -51),   S(-52, -58),   S(-27, -31),   S( 33, -59),   S(-52, -24),   S(-17, -37),
            S( 28, -33),   S(-39, -31),   S( 27, -16),   S( -3, -20),   S(-24,  13),   S( -3,  11),   S( -4,  18),   S( -7,  36),
            S( 39,   4),   S(  4, -25),   S( -1,  34),   S( 44,  99),   S( 37, 128),   S( 44, 113),   S( 25, 101),   S(-18, 109),
            S( 21,  29),   S(  6,  44),   S( 26,  82),   S( 25,  77),   S( 58, 108),   S( 64, 159),   S( 36, 108),   S(-44,  69),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 44,   2),   S( 24,  30),   S( 28,   9),   S(-10,   9),   S(  3,   0),   S(-13, -11),   S( 31, -44),   S(-17, -27),
            S( 64, -58),   S( 34, -52),   S( 29, -57),   S(-25, -29),   S(-30, -23),   S(-32, -33),   S(-26, -45),   S(  5, -43),
            S(-16, -37),   S(-12, -51),   S( -7, -75),   S(-61, -39),   S( 24, -38),   S(-35, -41),   S(-63, -40),   S(  2, -34),
            S(-51,   6),   S(-18, -56),   S(-19, -66),   S(-47, -22),   S(-16, -18),   S(-19, -20),   S( -9,  -2),   S( 25,   3),
            S( -1,  22),   S( -6, -11),   S(-31,  -4),   S( 27,   1),   S(  1,  55),   S( 20,  55),   S( -6,  74),   S( -6,  51),
            S(-28, 108),   S( 18,  35),   S(  7,  88),   S( 29,  74),   S( 31, 109),   S( 28, 107),   S(  8,  79),   S(  1,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -64),   S(-15, -44),   S( -7, -14),   S( 11, -13),   S( 48,   9),   S( 89,  12),   S( 50,  -1),   S(-12,  -5),
            S(-40, -47),   S(-52, -45),   S(-29, -41),   S( 46, -38),   S(  6, -17),   S(  0, -23),   S(  3, -37),   S(  5, -39),
            S(-25, -44),   S(-79, -26),   S(-43, -42),   S( -8, -28),   S( 26, -61),   S(-28, -44),   S(-29, -51),   S( 50, -65),
            S(-38,   1),   S(-21,  -1),   S(-31, -24),   S(-51, -45),   S(-12, -53),   S(-45, -61),   S( -6, -64),   S( 17, -55),
            S( -3,  11),   S( 45,  21),   S( 12,  27),   S(-12, -11),   S( 29,  40),   S( 13,  20),   S(-35,  12),   S( 41,  -3),
            S( -1,  25),   S(  3,  73),   S( 24,  74),   S( -2,  41),   S( 22,  54),   S( -7,  23),   S(  6,  29),   S( 19,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-53, -47),   S( -7, -48),   S(-38, -35),   S(  8, -16),   S(-11, -20),   S( 57,  11),   S( 54, -11),   S( 57,  -9),
            S(-24, -69),   S(-53, -58),   S(-33, -64),   S( 27, -61),   S(-41, -28),   S(  6, -43),   S( 27, -45),   S( 45, -60),
            S(-38, -38),   S(-67, -15),   S(-37, -16),   S( -9, -34),   S(-81, -26),   S( 59, -67),   S( 40, -129),  S( 99, -107),
            S(-65,  21),   S(-50,  35),   S(-14,  42),   S( 30,  -8),   S(-11, -11),   S( -5, -60),   S(-45, -56),   S( 28, -95),
            S(  6,  -3),   S(-22,  70),   S(-16,  89),   S( 13,  56),   S(  3,  70),   S(  3,   1),   S( -3,  29),   S( 16, -15),
            S( 19,  88),   S( 41,  58),   S( 24,  86),   S( 47,  99),   S( -6,  40),   S( 41,  84),   S( 22,  62),   S( 17,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5, -119),  S( 25, -56),   S(  7, -26),   S( -5, -11),   S( -1, -16),   S(-32, -62),   S( 41, -44),   S( -2, -34),
            S( 34, -101),  S( 28, -54),   S(-36, -95),   S(-28, -66),   S(-29, -113),  S(  9, -54),   S(  0, -77),   S(-15, -83),
            S(-21, -68),   S( -9, -48),   S(-29, -105),  S(-25, -83),   S( -5, -54),   S(-14, -26),   S(-58, -63),   S(-21, -71),
            S( -4,   5),   S( 16, -31),   S(-18, -38),   S( -1,  30),   S( 19,  49),   S( 10,  87),   S(  2, -10),   S(-16, -23),
            S(  2, -12),   S(  6,  15),   S(  0,  27),   S( 13,  88),   S( 39, 102),   S( 28,  75),   S(  3,  93),   S( 29,  68),
            S( -3,  12),   S(  7,  57),   S(  8,  53),   S( 12,  50),   S( 14,  82),   S( 28,  94),   S(-26, -30),   S(-17,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0, -53),   S( 32, -73),   S( 21,  22),   S( -3, -10),   S(  8, -26),   S(-22, -60),   S(-19, -104),  S(-20, -62),
            S( 43, -131),  S( 27, -101),  S( -4, -82),   S( 12,   4),   S( -1, -47),   S( 13, -89),   S( -4, -89),   S(-11, -72),
            S( -1, -70),   S( -7, -62),   S(  2, -87),   S( 17, -80),   S(-26, -31),   S( 17, -63),   S( -1, -61),   S( 60, -105),
            S(  4, -44),   S(-10, -11),   S(  7, -44),   S( -2,  20),   S( 23,   1),   S(-25,  14),   S(-12, -45),   S( 25, -36),
            S( -6,  25),   S(  7,   5),   S( -4,   9),   S( 10,  32),   S( 40,  81),   S( 24,  55),   S(  4,  74),   S( -6,  56),
            S(  7,  85),   S( 27,  52),   S( -6,   4),   S( 25,  73),   S( 25,  63),   S( 21,  80),   S( -2,  31),   S( -8,  43),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -1, -107),  S( 10, -52),   S(  5, -40),   S(  2,   2),   S( -4, -10),   S(  2,  -5),   S( 25, -58),   S( 20, -47),
            S( 20, -117),  S(-48, -117),  S( -5, -78),   S(-24, -98),   S(  6, -53),   S( 18, -55),   S(  0, -72),   S( 24, -84),
            S( 16, -81),   S(-16, -91),   S(-24, -75),   S( -9, -76),   S(-25, -83),   S( -2, -103),  S(  3, -85),   S( 23, -75),
            S(  4, -24),   S(-19, -52),   S( -7,  39),   S( -4,   5),   S( 19, -44),   S(  6, -56),   S(  4, -27),   S( 20,  17),
            S(-14,   8),   S( 13,  35),   S( -2,  36),   S(  8,  62),   S( 21,  78),   S(  4,  35),   S( 18,  41),   S( 16,  48),
            S(  3,  29),   S( 13,  34),   S( 25,  68),   S( 18,  60),   S( 19,  70),   S( 10,  51),   S( 26,  85),   S( 21, 108),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11, -19),   S(  8, -75),   S(-23, -57),   S(-17, -47),   S(-11, -17),   S( -3, -51),   S( 16, -35),   S( 16, -59),
            S(-16, -65),   S(-20, -123),  S(-13, -89),   S(-26, -30),   S(  3, -27),   S( -4, -47),   S( 35, -63),   S(  6, -105),
            S(-42, -49),   S(-31, -59),   S(-35, -48),   S( 17, -26),   S(-32, -30),   S(  0, -83),   S( -2, -53),   S( 17, -45),
            S(  1, -37),   S(-25, -33),   S(  5,  28),   S(-35,  15),   S( 11,   0),   S( -6, -11),   S(  0,  32),   S(  7,  62),
            S( 10,  44),   S(  1,  29),   S( -2,  58),   S( 15,  92),   S( 21,  93),   S( 14,  68),   S( 14,  37),   S(  0,  13),
            S(-22,  -4),   S(-11, -18),   S( 10,  70),   S( 11,  56),   S( 26,  75),   S( 19,  73),   S( 15,  64),   S( 17,  34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-77, -49),   S(-15, -28),   S( -1,   5),   S(-19,  77),   S(-27, -19),   S(-22,   0),   S(-18, -32),   S(-90, -48),
            S(  7, -31),   S(-16,   1),   S( -7, -18),   S( -1, -10),   S(-18,   4),   S(-10,  -3),   S(-33, -44),   S(-31, -14),
            S(  3, -57),   S( 21, -17),   S(  2,  17),   S( 38,  27),   S(  6,   0),   S(  0,  23),   S(-32,  50),   S(-17, -40),
            S(  3,  33),   S( 55,  61),   S( 15,  45),   S( 49,  22),   S( 33,  16),   S( 28,  28),   S( 35, -18),   S( -7, -26),
            S( 52,  33),   S( 15,  55),   S( 61,  54),   S( 73,  39),   S( 59,  48),   S( 12,  20),   S( 16,   4),   S( 11,   6),
            S( 97, -52),   S(-12,  59),   S(152,  10),   S( 83,  40),   S( 47,  35),   S(-35,  67),   S( 39,  -2),   S(-28,  18),
            S( 70,  -4),   S(-33, -41),   S( 29, -12),   S(102,  79),   S( 52,  32),   S(  5,  46),   S(-39,   8),   S(-38,  15),
            S(-82, -115),  S(  3,   4),   S( 12,  -3),   S( 29,  37),   S( -5,  43),   S(  5,   7),   S(-36,  -5),   S(  2,  12),

            /* knights: bucket 1 */
            S( 25,  -7),   S(-59,  32),   S(-43,  22),   S(-42,  24),   S(-43,  51),   S(-23, -40),   S(-36,  -7),   S(-22,   5),
            S(-58,  28),   S(-43,  56),   S(-28,  33),   S(-10,  30),   S(-13,  29),   S(  1,  24),   S(-21,  31),   S(  7, -89),
            S(-42,  25),   S( -5,  16),   S(-19,  27),   S( -2,  52),   S(-10,  46),   S( -8,  13),   S(-40,  39),   S( -2,   6),
            S( -2,  58),   S( 33,  31),   S( -6,  62),   S( -2,  59),   S(-10,  67),   S(-10,  55),   S( -8,  51),   S(-32,  46),
            S( 42,  -1),   S( -3,  34),   S( 42,  55),   S( 20,  65),   S( 49,  40),   S( -3,  75),   S( -4,  48),   S(-15,  78),
            S( -5,  25),   S( 78,  -8),   S( 66,  21),   S( 88,  42),   S( 69,  23),   S(-39,  77),   S( 12,  17),   S( 13,  25),
            S( 14, -17),   S( 23, -15),   S( 59, -41),   S( 57,  41),   S( 17,  32),   S( 23,   2),   S( -4,  71),   S(-43,  47),
            S(-179, -44),  S(  7, -35),   S(-33, -69),   S(-25,  -5),   S( -5,  -5),   S( 50,  51),   S(  3,  40),   S(-58,   5),

            /* knights: bucket 2 */
            S(-59,  18),   S(-34,  33),   S(-37,   5),   S(-29,  25),   S(-20,  12),   S(-44,   0),   S(-26,   9),   S(-36, -23),
            S(-24,  16),   S( -1,  32),   S(-12,   6),   S( -9,  16),   S(-21,  19),   S(-10,  -4),   S(  0,  -9),   S(-30, -15),
            S(-36,  39),   S(-20,  21),   S(-21,  27),   S(-21,  63),   S( -9,  45),   S(-21,  13),   S(-13,  11),   S( -9,   5),
            S( -8,  51),   S(-30,  54),   S(-19,  81),   S(-13,  79),   S(-39,  79),   S(  1,  54),   S(  6,  32),   S(-17,  38),
            S(-22,  54),   S(-25,  78),   S(  1,  76),   S( 13,  77),   S( -3,  67),   S( 23,  71),   S( -7,  60),   S( 17,  10),
            S(-42,  80),   S(-11,  42),   S( -3,  79),   S( 44,  39),   S( 50,  35),   S(116,  -6),   S( 62,   2),   S( 31, -22),
            S(  9,  46),   S(-47,  61),   S( 36,  34),   S( 22,   6),   S(-24,  64),   S(  6,  -3),   S( 43,  10),   S( 31, -33),
            S(-86,  45),   S( 25,  62),   S(  8,  77),   S(-29, -63),   S(-32, -16),   S(-25, -59),   S( 17, -16),   S(-154, -99),

            /* knights: bucket 3 */
            S(-56, -13),   S(-12, -43),   S( -3, -22),   S( -9,  -6),   S(  4, -13),   S( -3, -26),   S(-12, -36),   S(-26, -97),
            S(-19, -36),   S(  7,  -9),   S( 12,   2),   S(  1,  -1),   S(  1,  -9),   S( 25, -16),   S( 21, -37),   S( 27, -65),
            S(-18, -11),   S( -9,   2),   S(  5,  23),   S( 13,  41),   S( 19,  34),   S(  0,  19),   S( 24, -16),   S( 16, -44),
            S(  8,   0),   S( 16,  23),   S( 21,  50),   S( 14,  58),   S( 10,  76),   S( 32,  62),   S( 33,  45),   S(  7,  32),
            S( -9,  25),   S( 14,  41),   S( 25,  65),   S( 24,  81),   S( 29,  86),   S( 33,  94),   S(  6,  98),   S( 53,  71),
            S(  5,  11),   S( 15,  38),   S( 27,  57),   S( 13,  86),   S( 50,  66),   S(133,  79),   S( 34,  79),   S( 22, 100),
            S(-40,  32),   S(-14,  46),   S(-30,  60),   S( 12,  66),   S( 42,  69),   S(100,  41),   S( 15, -23),   S( 26,   3),
            S(-168,  41),  S(-29,  78),   S(-65,  84),   S( 43,  23),   S( 75,  51),   S(-88,  52),   S(  7, -33),   S(-82, -100),

            /* knights: bucket 4 */
            S( 10,  29),   S(-11, -26),   S(-39,  14),   S(-29, -32),   S(-31,  -4),   S(-59, -23),   S(  8,  -7),   S(-19,  -2),
            S( 39,  45),   S(-16, -32),   S( -4,  16),   S(-24,   5),   S( -2, -23),   S( 11, -37),   S(  4,  21),   S(-43,   6),
            S(  8,  -9),   S( 26,  11),   S( 78,  13),   S( 68,   2),   S( 33,  16),   S( 52, -13),   S(-22, -16),   S(-22, -15),
            S(-44, -64),   S( 29,  -3),   S( 20, -10),   S( 91,   1),   S( 65,  11),   S(-24,  37),   S(-16,  32),   S( 11, -13),
            S(-14, -51),   S( 34,   9),   S( 67,  30),   S( 44,  40),   S( 27,   2),   S( 19,  35),   S(-13, -13),   S(-36,  26),
            S( -3, -34),   S(  9,  10),   S( 51,  10),   S( 63,  43),   S(-23,  33),   S(-18,  49),   S(-14,  -2),   S( 10, -18),
            S(-29, -39),   S(-25,  -8),   S( -4, -20),   S( 20,  23),   S( 29,   1),   S( -5,  14),   S( 16,  29),   S(-46, -21),
            S( -1,   5),   S(-14, -41),   S(-11, -62),   S( 18,   4),   S( -4,   0),   S(-14,  11),   S(  0,  22),   S(-18, -19),

            /* knights: bucket 5 */
            S( 14,  11),   S( 10,  26),   S(-12,  40),   S(-15,  22),   S(-19,  27),   S( 27,  23),   S( -9,  11),   S( 10,  14),
            S( 12,   9),   S( 39,  28),   S(  0,  13),   S(-36,  18),   S( 33, -17),   S(-18,  17),   S(-20,  33),   S(-70, -11),
            S(-42,  15),   S(  0,  12),   S( 24,  18),   S( 15,  31),   S( 36,  23),   S(  3,  27),   S(  2,  17),   S(-53,  21),
            S( 16,  31),   S( 31, -14),   S( 56,  -5),   S( 98, -11),   S( 78,   2),   S( 81,  11),   S(-19,  23),   S(  6,  32),
            S( 45, -11),   S( 26, -28),   S( 62,  -1),   S(145, -14),   S( 81,  -7),   S( 46,  14),   S( 10,  14),   S( 13,  21),
            S(-20, -21),   S(  5, -30),   S( 13, -28),   S( -5,  11),   S( 33, -10),   S( 26,  16),   S( 21,   1),   S( 15,   3),
            S( -5, -20),   S(-25, -64),   S( 11, -62),   S(-14, -27),   S( -6, -52),   S(  5,  27),   S(  1,  33),   S( 26,   9),
            S(-45, -58),   S(-30, -84),   S( 16,  -9),   S(-20, -15),   S( 15,   5),   S( -7,  37),   S( -4,  10),   S( -1,  35),

            /* knights: bucket 6 */
            S(-10, -28),   S(-73,  20),   S( -7,   8),   S(-49,  39),   S(-61,  45),   S(  7,  35),   S(-16,  32),   S(-37,   4),
            S( 11,  -3),   S( -7,  40),   S(-28,  18),   S( 21,   6),   S( -7,  26),   S(-27,  31),   S( 17,  41),   S(-43,  56),
            S( -7,  36),   S( 20,  12),   S(  9,  31),   S( 32,  40),   S( 44,  43),   S(-26,  36),   S( 10,  36),   S( 17,  11),
            S(-24,  56),   S( 37,  15),   S( 56,  25),   S( 78,  18),   S(113, -17),   S( 90,  -3),   S( 36,  11),   S( -1,  50),
            S(-26,  28),   S( 33,  17),   S(106,   3),   S( 87,   8),   S(152, -41),   S( 62,  44),   S(151, -21),   S( -6,  15),
            S(  5,  22),   S( 11,  12),   S(  6,  41),   S( 53,  15),   S( 14,  18),   S( 25,  30),   S( 20, -15),   S( 56, -13),
            S( -7,  15),   S( 22,  27),   S( 25,  42),   S(-15, -12),   S(  0, -13),   S( 33, -22),   S(-10, -13),   S( -5,  44),
            S( 19,  35),   S( -3,  22),   S( 19,  23),   S( 24,   7),   S( 10, -11),   S( -4,   5),   S( 12,  17),   S(-36, -47),

            /* knights: bucket 7 */
            S(-34, -29),   S( 20, -25),   S( 22, -16),   S(-30,  26),   S( -8, -14),   S(-43,  26),   S(-45,   5),   S(-26,  11),
            S(-19, -31),   S( -8, -33),   S(  4,   7),   S(-44,  15),   S(  6,  -7),   S( -1,  19),   S(  6,  16),   S(-45,  31),
            S(-18, -30),   S(-46, -20),   S( 40, -14),   S( 40,  15),   S( 42,  25),   S( 24,  16),   S( 42,  10),   S( -1,  30),
            S(-46,  41),   S( -1,   0),   S( 59,  -8),   S( 62,  18),   S(133, -19),   S( 80,  19),   S( 65,   4),   S( 71,  -8),
            S( 13,  -4),   S(  9,  18),   S( 21,  29),   S( 90,  -7),   S(111,   0),   S(157, -25),   S(181, -13),   S( 47,   0),
            S(-25,  13),   S( 48,  20),   S( -4,  11),   S( 35,  38),   S( 81,  -1),   S(114, -13),   S( 17,  -4),   S(-37, -64),
            S(-30, -16),   S(  0, -24),   S( -1,  27),   S( 26,  29),   S( 51,  26),   S( 58,  22),   S(-10, -26),   S(-14, -31),
            S(-39, -58),   S(-17,   2),   S(-23,  26),   S( 11,  -1),   S(  8,  18),   S( 19,  17),   S( 10, -16),   S( -1, -16),

            /* knights: bucket 8 */
            S( -6,  -6),   S( -6, -43),   S( -5,   4),   S(-21, -64),   S( -6,  18),   S(-15, -23),   S(  8,  24),   S( -6, -32),
            S( -8, -42),   S(-11, -35),   S(-12, -59),   S(-24,  -7),   S(-39,  35),   S(-12, -26),   S( 11,  17),   S( -7, -20),
            S(-13, -54),   S(-15, -23),   S( 12, -38),   S( 13,   0),   S( -2,  -8),   S( 12,  13),   S(-10, -39),   S(  5, -11),
            S(-19, -68),   S( -4,  -8),   S( -5,  -2),   S(  1,   1),   S(-32,  -6),   S(-21,  -6),   S(-24, -24),   S(-12, -32),
            S(-10, -41),   S( -7, -37),   S(  3,  -8),   S(  3,  -3),   S( -6,  -1),   S(-16,  17),   S( -4,   6),   S( -2,  -2),
            S( -1,   5),   S( 13,  25),   S( -7,  -1),   S( -6, -26),   S( -1,   8),   S(  2, -14),   S( -4,  -2),   S(-13, -21),
            S(  8,  34),   S( -2, -29),   S(-21, -39),   S( 11,  30),   S(-13, -38),   S(  7,  22),   S(  8,  18),   S( -1, -15),
            S(  5,  14),   S( -3, -15),   S(-10,  -6),   S( 12,  28),   S(  4,  23),   S( -5,   6),   S(  2,   3),   S( -6, -12),

            /* knights: bucket 9 */
            S(-14, -56),   S( -8, -19),   S(-11, -56),   S(  4, -45),   S(-24, -11),   S(-24, -16),   S(  3,   3),   S( -1, -23),
            S( -3, -22),   S(-14, -55),   S(-24, -101),  S( -7, -38),   S(-13, -46),   S(-22, -63),   S( -7,  -4),   S(-19, -20),
            S(-10, -15),   S(-11, -27),   S(-10, -55),   S(  1, -45),   S(-22, -18),   S( 19,   8),   S(-19,   8),   S(  3,  13),
            S(-29, -56),   S(-15, -51),   S( -7,  -2),   S(-22, -32),   S(-28, -23),   S( -6,   5),   S(-18, -67),   S( 15,   7),
            S(  9,  35),   S( -6, -35),   S( -8, -28),   S( 15,  -8),   S(-11, -25),   S(  7,  39),   S( -3, -10),   S( -5,  35),
            S(-10, -16),   S(-16, -14),   S(-11, -18),   S(  9, -11),   S( 14,  42),   S( -3,   5),   S( -6,  -2),   S( -6,  11),
            S(-16, -28),   S( -4,  -7),   S(  2,  28),   S(-17,   0),   S( -9,  17),   S( -5,  19),   S(-17, -10),   S(-10, -15),
            S( -2,  -9),   S(  2,   3),   S(-11,  -9),   S(  3, -22),   S( -8, -20),   S( -7, -16),   S(  1,  -3),   S(  0,   7),

            /* knights: bucket 10 */
            S( -7, -17),   S( -9, -13),   S(-10, -22),   S(-18,  23),   S(-15, -44),   S(  8, -19),   S(  6,   9),   S(  3,  32),
            S( -9, -15),   S( 15,  23),   S( -4,  -6),   S( -7, -24),   S(  9, -11),   S(-11, -50),   S(-17,  10),   S(  3,  40),
            S( -4,   9),   S(-17,  -8),   S(-17,   5),   S( 18, -37),   S(-10, -57),   S( 14,  -8),   S( 12,  11),   S(-18,  -9),
            S(-17, -35),   S( -9, -31),   S( -1, -12),   S( -2, -35),   S(  4,   6),   S(-13,  -2),   S(-19, -82),   S( -8,   2),
            S(-17, -16),   S( -9, -31),   S(  0,  -5),   S(-13, -16),   S(  7,   2),   S(  1, -63),   S( -4,  -4),   S(-12, -11),
            S( -3,   9),   S(-24, -29),   S(  4,  27),   S(-21,  14),   S(-13, -18),   S(-15,  14),   S( -6,   5),   S(-10,   2),
            S( -4, -13),   S(  4,  12),   S( -7,  -2),   S( -5, -29),   S(-11,  10),   S(-23, -51),   S( -5,  -3),   S( -6, -16),
            S(  0,   0),   S( -1,  13),   S(  1,  30),   S( -6, -10),   S(  5,  19),   S(-11, -17),   S(  3, -10),   S( -6, -14),

            /* knights: bucket 11 */
            S( -5, -18),   S(-38, -41),   S( -4, -21),   S( 23,  23),   S(-37, -38),   S(-16, -34),   S( -5, -19),   S( 10,  32),
            S(  0, -14),   S(-20, -28),   S(-11, -51),   S(  7,  -8),   S( 20,  35),   S(  2, -35),   S(  6, -17),   S(-16, -28),
            S(-10, -36),   S(-27, -17),   S(  6,  -2),   S( 22, -12),   S(-10,  31),   S( 16,   1),   S( -3, -37),   S( -6, -11),
            S(-21, -12),   S( 40,  31),   S(  2, -18),   S(-15,  23),   S( 30,  -4),   S(  4, -23),   S( 22,   9),   S(  5,   8),
            S(-15,  11),   S(  8, -48),   S(-19,  21),   S(-11, -15),   S( 44,  26),   S( 13,  24),   S(-16, -67),   S( -4, -15),
            S( -4, -19),   S(  2, -44),   S( 14,   3),   S( 24,  -1),   S( 14,  54),   S(-10, -21),   S( -6, -26),   S( -1,  17),
            S(  2,  12),   S( -4,  27),   S( -7,  11),   S( -9, -25),   S(  5, -13),   S(  6, -21),   S( -2, -19),   S( -6,   2),
            S( -1,  -6),   S(  5,  13),   S(  1,  12),   S(  4,  20),   S( -4, -10),   S(  1, -21),   S(  5,  16),   S( -4,  -9),

            /* knights: bucket 12 */
            S(-18, -71),   S(  0,   1),   S( -4, -38),   S( -5,  -7),   S( -3,   5),   S(  2,   4),   S(  2,  10),   S( -2,  -5),
            S( -6, -18),   S( -2,  -5),   S(-10,  -9),   S(  2,   2),   S( -9, -35),   S(  3,  21),   S( -5, -11),   S(  0,   2),
            S(  6,   7),   S( -9, -28),   S( -8, -25),   S(-17, -25),   S(  1,  14),   S(-10,  22),   S(-11,  -6),   S( -2,   9),
            S(  4,  20),   S(  1, -34),   S( -2,  19),   S(  3,  46),   S(  8, -14),   S(  2,  40),   S( 10,  24),   S(  3,   9),
            S(  4,  24),   S( -4, -10),   S(  6, -12),   S(  7,  29),   S( 15,  37),   S( -3,  -1),   S(-11, -23),   S(-11, -15),
            S( -9, -15),   S(  7,   3),   S( -4, -24),   S( -2, -25),   S( -5, -10),   S( -8,  -6),   S( 14,  14),   S(  0,  18),
            S(  0,   4),   S( -6, -15),   S(-12, -17),   S( -1,  -6),   S(  0,  12),   S( -6, -16),   S( -8, -15),   S( -5, -10),
            S(  0,   0),   S(  2,   6),   S(  0,   0),   S(  1, -11),   S(  1,   1),   S(  5,   2),   S( -1,  -7),   S( -2,  -4),

            /* knights: bucket 13 */
            S( -5, -20),   S( -2, -16),   S( -5, -26),   S( -1, -20),   S( -4, -25),   S( -1,   8),   S(-11, -15),   S(  4,  17),
            S( -1,   5),   S( -1,  -1),   S(  4,  16),   S( -2,  -5),   S(  1,   3),   S(  0,  -4),   S( -6,   1),   S( -2,  -4),
            S(  1,  -7),   S(  5,  16),   S( -7, -22),   S( -4, -19),   S(  2,  25),   S( -7,   2),   S( 11,  12),   S(  1,  20),
            S( -3,  -1),   S( -1, -23),   S(  0,  15),   S(  6,  34),   S( -1,  27),   S( 12,  46),   S(  0,  13),   S( 12,  21),
            S(  4,  36),   S(-16, -41),   S( -5,  31),   S( -4,  15),   S(-13,   5),   S(  4,  22),   S( -6, -20),   S( -6,  -3),
            S( -1,  -4),   S(  3,   4),   S(-13, -32),   S(  9,  32),   S( -9,   6),   S(-12, -17),   S(  4,  25),   S(  1,   7),
            S(  0,   6),   S( 10,  26),   S( -9, -13),   S( -3,   1),   S( -3,   3),   S(  3,   8),   S( -1,   1),   S(  2,   7),
            S(  2,   5),   S(  3,   7),   S( -2,  -9),   S(  0,   6),   S( -5,  -9),   S(  5,  11),   S( -2,  -5),   S(  0,   2),

            /* knights: bucket 14 */
            S( -1,  -4),   S( -3,   3),   S(  5,  18),   S( -2,   5),   S( -8, -33),   S( -1,  25),   S(  3,  12),   S( -3,  -4),
            S( -4, -17),   S(-14, -32),   S(  2,  -9),   S( -2,   3),   S(  3,   3),   S(  2,  -7),   S( -5,  11),   S( 10,  59),
            S(  1,  11),   S( -5, -25),   S(  4,  12),   S( -5, -34),   S(  5,  21),   S(  7,  14),   S(  3,  16),   S(  5,  12),
            S( -1,  -6),   S( -3, -30),   S(-23,  -9),   S( -5,  53),   S(  0,  47),   S(  1, -11),   S( -6, -25),   S(  0,  23),
            S(  8,  22),   S(-20, -39),   S(  1,  10),   S( -8,  -7),   S( -7,  21),   S(-12,  -1),   S(  1,  18),   S( -2,  -2),
            S( -4,  -3),   S( -2, -25),   S(  3,   9),   S(  2,   9),   S(  9,  25),   S(  3,  27),   S(  6,  21),   S( -6, -17),
            S( -1,   6),   S( -8, -16),   S(  5,  17),   S(  5,  -2),   S(  5,  20),   S( -6, -11),   S(  1,   9),   S(  5,   1),
            S(  3,   4),   S( -1,  -6),   S( -2,  -8),   S(  7,  15),   S(  0,  -2),   S(  0,  -2),   S( -1,  -5),   S(  0,   1),

            /* knights: bucket 15 */
            S( -3, -22),   S(  1,   7),   S(  8,  31),   S( -3,  -4),   S( -7, -14),   S( -8, -29),   S( -1,   4),   S( -4, -12),
            S(  0,  -2),   S(  4,   8),   S( -8, -25),   S(  9,  62),   S( 14,  13),   S( -4, -24),   S( -1,  -2),   S(  1,   1),
            S( -3,  -8),   S( -8, -18),   S(  4, -11),   S(  9,  10),   S(-17, -50),   S(  0, -16),   S(  3,   3),   S( -2,  -9),
            S(  3,   2),   S( -6, -13),   S( -2,   7),   S( -6,  10),   S( -7,  -1),   S(  8,  78),   S(  3,   3),   S(  0,  11),
            S( -1,  -4),   S( 14,  37),   S(-12, -23),   S( -5,   7),   S( 17,  23),   S(  7,  16),   S(  1, -16),   S(  2,   6),
            S(  2,   7),   S(  1,   5),   S(  0,   2),   S( -9, -21),   S(-11,  -5),   S(  7,  15),   S(  0,   5),   S(  2,  10),
            S( -5,  -9),   S( -5, -16),   S(  4,  15),   S(  3,   3),   S(  3,  22),   S(  6,   2),   S(  1,   6),   S(  4,   9),
            S(  4,  14),   S( -4, -17),   S(  0,   1),   S(  0,  -1),   S( -1,   1),   S( -1,   1),   S(  2,   3),   S(  1,   3),

            /* bishops: bucket 0 */
            S( 44, -33),   S(-37,  50),   S(-26,  20),   S(-23,  -9),   S( -8,  -7),   S(  5,   4),   S( 66, -27),   S( 30,  12),
            S(-66,  12),   S( -1,  -6),   S(-13,  43),   S(  6,  11),   S(  8,  17),   S( 39,  17),   S( 43,  38),   S( 46, -13),
            S(  4,  30),   S( 17,  15),   S(  0,  38),   S( 12,  12),   S( 31,  21),   S( 47,  43),   S( 46,  -5),   S( 29, -15),
            S( 12, -31),   S( 28, -42),   S(  1,  15),   S( 42,  10),   S( 74,  32),   S( 24,  36),   S( 31,  -3),   S(-28,  52),
            S( 35,  -9),   S( 32, -12),   S( 45,  27),   S( 92,  53),   S( 94,  11),   S( 29,  26),   S( 31,  44),   S(  2, -13),
            S( 47,  63),   S( 63,  23),   S( 98,  14),   S( 43,   8),   S(  3,  48),   S( 21,  77),   S( 29,  16),   S( -3,  27),
            S(-95, -98),   S(103,  28),   S( 92,  84),   S( 14,  -1),   S( 28, -32),   S(  8,  26),   S( -7,  31),   S(-15,  60),
            S( -2, -24),   S(-12,  -4),   S( 14, -59),   S( -5, -27),   S(-32, -37),   S(-36,  25),   S(-26,  59),   S(-15,  -2),

            /* bishops: bucket 1 */
            S(-63,  36),   S( 21, -12),   S(-32,  43),   S(  4,  -5),   S(-36,  38),   S(  7,   2),   S( 35,  -2),   S( 25, -23),
            S(  3, -30),   S(-20,   6),   S(-11,  -1),   S(-19,  20),   S( 30,  -7),   S(  6,  -1),   S( 59, -27),   S(  3, -13),
            S(-15,   2),   S( 22, -12),   S(-14,  17),   S( 11,   9),   S(  4,  -3),   S( 34,   4),   S( -4,   7),   S( 73, -15),
            S( 20, -16),   S( 53, -19),   S( 13,   2),   S( 13,  13),   S( 44,   3),   S(  6,  16),   S( 65,  -1),   S(  3, -10),
            S( 19, -20),   S( 51,  -4),   S( 30,   2),   S( 93, -11),   S( 57,  11),   S( 52,  30),   S( -8,  28),   S( 35,  -3),
            S( 55, -34),   S( 70,  -6),   S( 56,   5),   S( 56, -12),   S(115, -17),   S(-25,  26),   S(-36,  51),   S(-30,  20),
            S( 16, -71),   S(  1, -39),   S( -6,   1),   S( 12,  49),   S( 34,  44),   S(  0,  19),   S(  2,   4),   S(-65,  29),
            S(  7, -35),   S(-24,  13),   S(-32, -30),   S(-25,   3),   S(-15,  10),   S( 27, -16),   S( 38,  18),   S(-84,  -5),

            /* bishops: bucket 2 */
            S(-15,   2),   S(-23,   0),   S( -1,   4),   S(-32,  14),   S( 14,   6),   S(-19,   4),   S( 16,  -7),   S( -3,  -2),
            S( 23, -35),   S( 10, -12),   S( -8,  -2),   S( 10,  13),   S( -7,   8),   S(  7,   9),   S(  6, -12),   S( 28, -64),
            S( 43,  -2),   S(  9,  -5),   S( -3,  19),   S( -6,   5),   S(  0,  24),   S( -9,  -2),   S(  2, -24),   S(-13, -14),
            S(-15, -11),   S( 48,  12),   S( -5,  10),   S( 30,  27),   S(  0,  12),   S( -9,  30),   S(-12,  -1),   S(  1,  -9),
            S(  2,  11),   S(-28,  42),   S( 40,  30),   S( 27,  23),   S( 12,  29),   S( 28,   4),   S(  6,  29),   S( 37, -15),
            S(-27,  32),   S( -6,  41),   S(-19,   0),   S( 79,   2),   S( 64,  -2),   S( 96,  17),   S( 66,  17),   S( 33, -43),
            S(-53,  72),   S(-37,  19),   S(-14,  25),   S( 10,  12),   S(-16,  -9),   S(-44,  19),   S(-47,  26),   S( -6, -64),
            S(-93,  -6),   S(-45,  30),   S(  8,   1),   S(-42,  25),   S(-46, -34),   S(-32,  -9),   S(-27, -18),   S(-55,  11),

            /* bishops: bucket 3 */
            S( 33,  -8),   S( 42, -37),   S( 15, -30),   S(  3,  11),   S( 12,   5),   S(  0,  27),   S( -8,  48),   S( 11, -28),
            S( 37,  -7),   S( 31, -13),   S( 23,  -5),   S( 23,   7),   S( 26,  13),   S( 24,   5),   S( 17,   1),   S( 40, -38),
            S( 12,  -8),   S( 23,  37),   S( 25,  30),   S( 18,  24),   S( 20,  27),   S( 18,  28),   S( 18, -12),   S( 14,   7),
            S( -5,  10),   S( 10,  40),   S( 20,  50),   S( 35,  46),   S( 33,  21),   S( 20,  10),   S( 31,  -2),   S( 28, -44),
            S( 12,  26),   S( 12,  56),   S( -6,  60),   S( 43,  51),   S( 47,  44),   S( 45,  18),   S( 26,  19),   S(-12,  10),
            S(  5,  34),   S( 19,  66),   S( -8,  45),   S( 20,  36),   S( 53,  37),   S( 77,  66),   S( 45,  42),   S( 44,  66),
            S(-29,  91),   S( -8,  54),   S( 18,  24),   S( 13,  56),   S( 11,  24),   S( 55,  42),   S( -7,  29),   S( 25, -51),
            S(-50,  32),   S(-48,  60),   S(-52,  53),   S(-13,  35),   S( 32,  11),   S(-62,  57),   S( 25,  -3),   S( 25,  21),

            /* bishops: bucket 4 */
            S(-10,  29),   S(-57,  -8),   S(-38,   2),   S(-47,   3),   S(-11, -32),   S(-29, -16),   S( -8, -24),   S(-28, -19),
            S(-15, -22),   S( 23, -12),   S( 52, -26),   S(-50,  25),   S(-50,  27),   S(-26, -34),   S(-23,  -7),   S(-61, -34),
            S(-10,   4),   S( 30, -16),   S( 15, -10),   S(-10,   9),   S( 14, -21),   S(-61,  55),   S(-20, -48),   S(-52, -19),
            S( 23,  -6),   S( 47, -23),   S( 42,  27),   S(  0,  26),   S(  1,  16),   S( 43,   7),   S(-55,   6),   S( 13, -34),
            S( 20,   0),   S(-14, -22),   S( 53, -11),   S( 42,  11),   S(-24,  50),   S( 26,   9),   S( -8,  42),   S(-45, -13),
            S(-59, -99),   S(-27, -11),   S( 19,  21),   S( 41,  19),   S(-72,  62),   S( 25,  15),   S( -7,  22),   S( 10,  47),
            S(  4,   5),   S(-11,  -6),   S(-13, -25),   S(-17, -23),   S(  9, -37),   S( 52,  -2),   S( 22,  15),   S( 56,  -1),
            S(  0,  -2),   S(  9,  -7),   S(-28,   2),   S(  2, -37),   S(-28,   3),   S(-17,  12),   S( -6,  37),   S( 11,  22),

            /* bishops: bucket 5 */
            S(-45, -12),   S( 46, -23),   S( -7,   4),   S(-36,  11),   S(-34,  12),   S(-75,  19),   S(-37,  -1),   S(-43,   3),
            S(-29, -19),   S(-61,  28),   S( -5,   8),   S(-11,  25),   S(-37,  21),   S( -3,   7),   S(-21,  16),   S(-18, -37),
            S( 10,  33),   S(-26,   4),   S( 35,   1),   S(  5,   8),   S(-14,  21),   S(-36,  19),   S(-60,  44),   S(-27,  43),
            S( 13,   6),   S(-14,  28),   S( 80, -18),   S( 78,  -8),   S(  9,  21),   S( 25,  16),   S(-80,  21),   S(-29,  33),
            S( 33, -32),   S( 27,   5),   S( 13,   0),   S(  5,  -8),   S(  8,   9),   S(-28,  20),   S(-25,  32),   S(-46,  17),
            S(  7, -23),   S(-61,  15),   S( 59,  -8),   S(-27, -12),   S(-12,   9),   S(-23,  26),   S(-22,   0),   S(-34,  62),
            S(-28, -26),   S( 11,  -7),   S(-42,  -7),   S( -2,  13),   S( 21,  12),   S(-22,  29),   S( -3,  38),   S(-22,  22),
            S(-20,  16),   S(-27, -16),   S(  7, -22),   S(-18, -14),   S( -9,   8),   S(  3,  33),   S(-18,   7),   S( 13,   6),

            /* bishops: bucket 6 */
            S(-44, -19),   S(-13,  -2),   S(-34,  22),   S(-15,  18),   S(-33,  19),   S(-51,  26),   S(-52,  38),   S(-34, -17),
            S(-47,  18),   S(-13, -12),   S(-53,  30),   S(-22,  21),   S(-54,  28),   S(-49,  33),   S(-63,  39),   S(-34,  22),
            S( 43, -22),   S(-53,  23),   S(-10,  12),   S(-44,  33),   S(-19,  33),   S( -8,   5),   S( -4, -14),   S(-27,  22),
            S(-59,  42),   S(-58,  45),   S(-15,  26),   S( 29,  39),   S(  7,  35),   S( 43,  11),   S( 18,  -6),   S(-17,  39),
            S(-49,  22),   S(-33,  44),   S( 24,   4),   S(107, -10),   S( -8,  20),   S(-24,  12),   S( 30,   6),   S(-29,  -5),
            S(-51,  41),   S(-30,  28),   S(-23,  10),   S(  2,   6),   S( 41,   3),   S(  8,  -1),   S( -4,   1),   S(-35,  -6),
            S(-38,  36),   S(-80,  47),   S(-33,  24),   S(-29,  31),   S(  0,  -6),   S( 18, -13),   S(  5,   6),   S(-28,  -5),
            S( -9,  20),   S(-25,  24),   S(  0,  29),   S( 19,  10),   S(-29,  21),   S( -3, -13),   S(-20,  11),   S(  8,  24),

            /* bishops: bucket 7 */
            S( 12, -43),   S(-51,  -2),   S(-71, -16),   S(-36,  -3),   S(-36,   3),   S(-52,  13),   S(-76, -25),   S(-90,  11),
            S(-19, -59),   S(  7, -34),   S( 27, -22),   S(-14,  -7),   S(-47,   4),   S(-38,   1),   S(-44, -10),   S( -2, -19),
            S(-40,  -7),   S(-27,  -6),   S( -3,   1),   S( 35, -12),   S(  0,   1),   S( -3,  -6),   S(-50, -13),   S(-68,  28),
            S(-20, -26),   S(-80,  49),   S(-47,  23),   S(-39,  36),   S( 63,  19),   S(  4,  18),   S( 34, -14),   S(-31, -27),
            S(-50,  -7),   S( 27,  -3),   S(-49,  40),   S(  3,   8),   S( 43,  -8),   S( 50,   7),   S(-37,   3),   S(-21, -25),
            S(-99,  35),   S(-40,  48),   S( -2,  14),   S(-87,  44),   S(-49,  27),   S( -9,  15),   S( 27,  43),   S(-84, -87),
            S(-31,  -5),   S(-21,  29),   S(-39,  13),   S(-21,  21),   S(-12,   2),   S( 17, -12),   S( 35, -39),   S( -3, -19),
            S(-34, -29),   S(-17,   6),   S(-27,  14),   S(-27,   9),   S( 18, -24),   S( -4, -12),   S( 20, -48),   S(  5,   0),

            /* bishops: bucket 8 */
            S( 40,  86),   S( -3, -46),   S(  2,   7),   S( -7,  42),   S(-21,  11),   S(-14, -38),   S(-27, -33),   S(-10,  -7),
            S( 15,  13),   S( 25,  53),   S( 23,  -1),   S( 14,   5),   S( 16,  -5),   S( -5,  10),   S(  0, -21),   S(  1,  16),
            S(-13, -10),   S( -7,  -2),   S( 36,  15),   S( 13,   8),   S( 12,  39),   S(  4,  28),   S(-23, -11),   S(-39, -12),
            S( -7, -15),   S( 26,  27),   S(  1,  17),   S( 43, -20),   S(  7,  62),   S( 18,  28),   S(-15,  14),   S(  4, -12),
            S(  2,  16),   S( 56,  68),   S(  6,  -4),   S(-15,  30),   S( 27,  34),   S(-29,  25),   S(-18, -54),   S(  6,  15),
            S( -7,  -4),   S( -6,  -3),   S( 25,  45),   S( 24,  -4),   S( 29,  30),   S( 63,  23),   S(-16,  22),   S( -4,  27),
            S( 12,  45),   S( -8,  -4),   S( 33,   5),   S( 39,   1),   S( 11,  21),   S( 22,  42),   S( 35,  43),   S(-15,   1),
            S( -1,   6),   S(  3,   2),   S( -6,  -3),   S( -5,  15),   S( 30, -12),   S( 21,   7),   S(  4,  14),   S( 44,  26),

            /* bishops: bucket 9 */
            S( 22,  66),   S(  0, -15),   S(  1,  11),   S(-16, -37),   S(-17, -24),   S(-11, -10),   S(  2,   6),   S( -3,   6),
            S(-13, -18),   S( -3,   6),   S( 12,  26),   S(-39,  -7),   S(-20,   5),   S(-22,  -9),   S(-24, -19),   S(-16, -49),
            S( -5, -10),   S( 18,  15),   S(  4,   4),   S(  2,  14),   S(  3,  17),   S(-17,   2),   S(-12, -12),   S(-31, -17),
            S( -7,  13),   S( -4, -11),   S( 16,  12),   S( 30, -15),   S( -9,  21),   S( -6,  23),   S( -6,  22),   S(  0,  14),
            S( 23,  21),   S( 10,  28),   S( 22,  73),   S( 41, -19),   S( 14,  41),   S(  7,  29),   S(  6,  43),   S( -6,   5),
            S( 23,  34),   S(-24,  39),   S( 24,  -5),   S( 13,  28),   S( 46, -53),   S( 15,  32),   S(  1,  30),   S( 10,  18),
            S( 13,  -2),   S(-19,  17),   S(  4,  14),   S( 22,  -5),   S( 12, -16),   S( 18,  24),   S( 41,  51),   S( 10,  15),
            S( 19,  50),   S( -8, -33),   S(  4,  21),   S( -3,  25),   S(  7,  44),   S(  8, -19),   S( 26,   9),   S( 39,  48),

            /* bishops: bucket 10 */
            S(  3, -32),   S(  8,  23),   S(  5, -11),   S(-30, -27),   S(-69, -30),   S(-33, -76),   S( 10, -12),   S( -1,  29),
            S( -1,  15),   S(-12, -41),   S( -9,   3),   S(-13, -35),   S(-52,  -6),   S(-34, -46),   S(-37,  -5),   S(  6,   6),
            S( -2, -14),   S(-24, -24),   S(-19,   1),   S( -5,  65),   S(-11,  20),   S(-16, -30),   S(  0,  26),   S( -8, -50),
            S(-19,   4),   S(-21,  -6),   S(-14, -49),   S(  9,  -9),   S(-19,  73),   S( 32,   0),   S( 34,  28),   S(-14, -30),
            S( 16,   0),   S(-26,  27),   S(-19,  19),   S( -8,  39),   S( 52, -17),   S( 17,  12),   S( 31, -31),   S( 24,   1),
            S(  6, -10),   S( 22,   3),   S( -7,  27),   S( 32,  13),   S( -3, -11),   S( 11,  33),   S( 11,  -2),   S( 16,   4),
            S( 19,  23),   S(  7,  22),   S( 28, -58),   S( -3,  18),   S( -8,   3),   S(  4, -15),   S( -4,   3),   S( 13,  46),
            S( 14,  40),   S( 15,  33),   S( 25,   2),   S(  2,  38),   S(  4,  27),   S( 12,  16),   S( 13,  43),   S(  2,   0),

            /* bishops: bucket 11 */
            S( 12, -50),   S(-10,  -5),   S(  0,  14),   S( -8, -37),   S(-10, -10),   S(-20,  -4),   S(-50, -62),   S(-12,  10),
            S(-12, -35),   S(  8,   5),   S(-20,  -4),   S(  8, -22),   S( -6,   5),   S(-45, -23),   S(-36, -11),   S( 24,  24),
            S(-15, -54),   S(-19, -54),   S( 35,  -7),   S(-35,  12),   S(-23, -24),   S( 13,  28),   S( -7,  12),   S(  7, -10),
            S( -8,  -4),   S(  8, -33),   S( -3,   7),   S(-49,  -1),   S( 17, -37),   S( 25,  58),   S( 39,   6),   S( -8, -20),
            S(  3, -11),   S( -2, -42),   S(-40,  47),   S(-40,  43),   S(-44,  40),   S( 39,  23),   S( 31, -28),   S(  8,  13),
            S(-15, -23),   S( -8,   4),   S( -4,  31),   S( -3,  35),   S( 57,  19),   S( 29,  -9),   S(  5, -20),   S(-13, -31),
            S( -7, -30),   S( 32,  55),   S(  3,  56),   S( 14,  -6),   S( 24,  13),   S(-13, -25),   S(-16, -26),   S( -5,  -8),
            S( 36,   9),   S(  8,  -6),   S( 26,  48),   S( 21, -15),   S( 21,  39),   S(  7,  20),   S( -5,  -7),   S( 13,   9),

            /* bishops: bucket 12 */
            S( -5, -13),   S( -2,  -7),   S( -6, -14),   S(  8,  -2),   S(-13, -10),   S(-10,  -9),   S( -1, -13),   S( -4,  -8),
            S( -1,  -7),   S(  7,   7),   S( -6, -13),   S( -6,  12),   S( -9,   4),   S(  5,  13),   S(-12, -12),   S( -1,  -2),
            S(  5,   3),   S( 24,  16),   S( 34,  25),   S( 20,   6),   S(-10,  -1),   S( -7,   5),   S(  5,  -2),   S( -4,  -4),
            S(  9,  -1),   S(  5,  -6),   S( 23,  10),   S( 15,  13),   S( -2,  -4),   S(-15,  -5),   S( 12,  24),   S( -3,  -6),
            S( 12,  13),   S(  8,  17),   S(  2,  37),   S( 16,   0),   S( 23,  24),   S( 20,  50),   S( -5,  -7),   S( 11,  14),
            S(  2,  -5),   S(-22, -12),   S(  3,  26),   S( -4,  -2),   S( 18,  24),   S( 23,  24),   S( -1,   5),   S( -5, -18),
            S( -3,  -3),   S(  1,   8),   S(  4,  15),   S(  2,  -1),   S( 11, -11),   S( 17,  44),   S( 18,  26),   S( -1,   7),
            S(  2,   6),   S(  4,   4),   S( -1,   2),   S( -2, -13),   S( -3,   0),   S(  0, -25),   S( 20,  20),   S(  7,   8),

            /* bishops: bucket 13 */
            S( -3, -19),   S(  0,   1),   S( -3, -16),   S( -2,   1),   S(  5,  -2),   S( -3, -15),   S(-17, -24),   S( -3,  -3),
            S( -4,   3),   S( -5, -19),   S( -9, -12),   S( 18, -11),   S(-14, -35),   S(  8,  -8),   S( -4,  -4),   S(  4,  -8),
            S(  1, -33),   S( 32,  32),   S( 14,   3),   S( 22,  27),   S( -5,   8),   S( 18,  31),   S( -6,   2),   S( -7,  10),
            S( 12,  25),   S( 47,   8),   S( 26,  36),   S(-13, -14),   S( 21,  88),   S(  7,  39),   S(  9,  20),   S(  5,  13),
            S( 14,  14),   S(  6,   4),   S( 13, -31),   S( -2, -30),   S( 14,   9),   S( 10,   7),   S( 17,   4),   S(  0,  46),
            S( -4,   2),   S(  0,  -4),   S( -5, -18),   S( 16,   7),   S(  3,   4),   S(  2, -22),   S( -7,  -7),   S( 10,  -2),
            S( 11,  10),   S( -5, -22),   S( -2, -16),   S(  7,  10),   S(  9,  25),   S( 11, -11),   S(  9,  -7),   S(  8,   3),
            S(  3,  -5),   S(  0,  -2),   S( -1,   4),   S(  3,   0),   S(  4,   7),   S(  4, -16),   S(  6, -13),   S( 13, -12),

            /* bishops: bucket 14 */
            S( -5,  -5),   S(  3,  11),   S( 18,  12),   S( -2,   8),   S(-17, -11),   S( -4,   3),   S( -3,  -5),   S( -5,  17),
            S(  3,   3),   S(  7,  10),   S(  1,  -5),   S(  1,  -3),   S( 15,   9),   S(  3,   3),   S(-13,   0),   S(  7,  38),
            S(  0,   0),   S(  6,  -3),   S(-11,   2),   S( 28,  42),   S( 10,  35),   S( 10,   9),   S(  6,  22),   S(  1,  24),
            S(  5,  34),   S( 17,   4),   S(  2,  17),   S(  2,  26),   S( 15,  28),   S( 19,  24),   S( 19,  27),   S(  2, -12),
            S(  8, -11),   S( -1,  -1),   S( 23,  26),   S( 17,  -7),   S( -6, -37),   S(  4,   6),   S( 25,  -3),   S( 21,  18),
            S( -2, -20),   S( 19,  16),   S( 12,  -2),   S(  7,  10),   S(  7, -11),   S(  1,  16),   S(  3,  39),   S(  6, -22),
            S( 17,  37),   S( 11,  19),   S( 13,   9),   S( 11,   9),   S(  4,  -5),   S(  7,  16),   S( -5, -15),   S(  6,   3),
            S(  9, -21),   S( 14,  30),   S( -2,   2),   S(  8,  13),   S( -3,  -6),   S(  0, -17),   S(  6,  10),   S(  2,   0),

            /* bishops: bucket 15 */
            S(  1,  -4),   S(  4,   6),   S( -8, -22),   S( -8,  -3),   S(-10, -35),   S( -9, -20),   S( -7, -12),   S( -7, -17),
            S( 10,  27),   S(  2,   6),   S( -4,  -5),   S(  5,   7),   S( 15,   2),   S( -1,  -4),   S(  0,  -5),   S( -8, -19),
            S(  4,  -7),   S(  8,   0),   S(  5, -14),   S( 13,  15),   S( 13,  29),   S(  3,  18),   S( 19,  27),   S(  4,  -2),
            S(  1,  -5),   S(  6,   6),   S(  4,   9),   S(-12,  20),   S( 15,   9),   S( 24,   0),   S(  9,  21),   S( 12,  -1),
            S( -3, -12),   S( -8,  12),   S(  0,  31),   S( 32,  52),   S( 31,  -1),   S( 11,  13),   S( 15,  23),   S(  3,  15),
            S( -6,   5),   S(  2,   1),   S( 17,  61),   S( 17,   5),   S( 19,  15),   S(  8, -16),   S( -1,  12),   S(  2,   4),
            S(  5,   2),   S(  8,  27),   S( 12,  66),   S(  7,  -3),   S(  1,  14),   S( -4, -12),   S( -2,  -7),   S( -3,  -5),
            S(  3,   7),   S(  5,  -1),   S(  7,   3),   S(  8,   1),   S(  8,  14),   S( -5, -18),   S( -1,  -5),   S(  6,   7),

            /* rooks: bucket 0 */
            S(-13,   9),   S(  9,  -1),   S( -3,  10),   S(  3,  13),   S(-22,  61),   S(-14,  50),   S(-39,  58),   S(-68,  58),
            S( -6,  -6),   S( -1,   5),   S(-32,  21),   S( -9,  37),   S( -9,  47),   S(  0,  26),   S(-25,  22),   S(-16,  41),
            S(  3, -16),   S(  0, -17),   S(-16,  15),   S( -3,  13),   S(-41,  66),   S(-20,  17),   S(-13,  24),   S( -3,  14),
            S( -3,  -9),   S(  5,  24),   S(-53,  62),   S(  5,  31),   S( -9,  60),   S(-22,  37),   S(-25,  42),   S(-27,  42),
            S( 72, -74),   S( 61, -15),   S(  9,  48),   S( 29,  27),   S( 46,   9),   S( -1,  93),   S( 12,  62),   S( -9,  64),
            S( 55, -24),   S( 54,  32),   S( 94,  -1),   S(111,  27),   S( 22,  50),   S( 37,  67),   S(  3,  65),   S(-60, 107),
            S( 41,  10),   S( 58,  32),   S( 88,  40),   S( 66,   7),   S( 89,  30),   S( 26,  66),   S(-32,  93),   S( -9,  83),
            S(  9, -40),   S( 10, -14),   S( 15,  43),   S(  4, -23),   S( 26,  44),   S( 68,   8),   S( 36,  20),   S( 43,   7),

            /* rooks: bucket 1 */
            S(-54,  53),   S(-21,   6),   S( -5,  17),   S(-35,  34),   S(-36,  53),   S(-37,  48),   S(-50,  65),   S(-80,  68),
            S(-66,  51),   S(-26, -10),   S(-31,  29),   S(-25,  24),   S(-26,  12),   S(-48,  44),   S(-12,   2),   S(-46,  61),
            S(-34,  33),   S(  4, -14),   S(-13,  -1),   S(-24,  19),   S(-52,  33),   S(-53,  35),   S(-63,  71),   S(-34,  60),
            S(-44,  38),   S(-25,  16),   S(  1,  31),   S(-14,  12),   S(-44,  45),   S(-40,  62),   S(-35,  77),   S(-84,  91),
            S(-32,  50),   S( 31, -28),   S( 34,  11),   S(  2,   4),   S( -6,  19),   S(-22,  87),   S( 11,  43),   S(-14,  77),
            S( 43,  35),   S( 61,  -3),   S( 36,  24),   S(-16,  41),   S(  0,  30),   S( 30,  42),   S( 54,  28),   S( 15,  74),
            S( 13,  63),   S( 18,  18),   S( 17,  50),   S( 39,  18),   S( 38,  15),   S(  0,  45),   S( -1,  85),   S( 48,  60),
            S( 66, -26),   S( 16, -22),   S(  6, -26),   S( -1,  12),   S( 37, -17),   S(  8,  28),   S( 50,  20),   S( 47,  33),

            /* rooks: bucket 2 */
            S(-61,  66),   S(-49,  60),   S(-37,  55),   S(-33,  23),   S(-24,  30),   S(-32,  27),   S(-33,  16),   S(-73,  57),
            S(-49,  58),   S(-50,  49),   S(-42,  55),   S(-45,  31),   S(-49,  34),   S(-50,  17),   S(-18,  -2),   S(-60,  43),
            S(-49,  60),   S(-35,  60),   S(-60,  44),   S(-21,  27),   S(-44,  27),   S(-30,  28),   S(-24,  13),   S( -5,  25),
            S(-35,  80),   S(-41,  71),   S(-60,  79),   S(-62,  50),   S(-61,  55),   S(-45,  43),   S(-22,  23),   S(-29,  43),
            S(-34,  90),   S(-46,  83),   S( -4,  75),   S(-15,  35),   S(-41,  56),   S( 33,  30),   S( -1,  27),   S(-15,  66),
            S( 10,  84),   S( 15,  69),   S( 20,  64),   S(-16,  47),   S( 54,   7),   S( 39,  43),   S(100,   3),   S( 47,  59),
            S( 65,  47),   S(  3,  67),   S( 14,  48),   S( 32,  20),   S(  6,  -3),   S( 51,  68),   S(-26,  65),   S( 19,  73),
            S( 16,  31),   S(  9,  43),   S( 22,  31),   S(-21,  18),   S(  5,   0),   S( 46,   9),   S(  5,  34),   S( 20,  30),

            /* rooks: bucket 3 */
            S(-15,  72),   S( -8,  66),   S(-12, 100),   S( -4,  88),   S(  6,  46),   S( 11,  44),   S( 27,   8),   S( -4,  -7),
            S( 13,  50),   S(-15,  81),   S( -9,  93),   S( -5,  91),   S(  2,  54),   S(  7,  23),   S( 52,  -4),   S( 20,  -1),
            S(  9,  60),   S(-10,  85),   S(-17,  87),   S( -1,  92),   S( 11,  40),   S(  9,  39),   S( 33,   9),   S( 32,   9),
            S(  6,  90),   S( -6, 112),   S(-19, 122),   S( -9,  98),   S( -4,  66),   S( 12,  53),   S( 36,  28),   S(  7,  25),
            S(  9, 106),   S(-13, 118),   S( 14, 124),   S( 22,  97),   S(  3,  88),   S( 32,  74),   S( 64,  29),   S( 41,  40),
            S(  6, 120),   S( 22, 103),   S( 16, 122),   S( 45,  95),   S(106,  39),   S(133,  28),   S( 85,  34),   S( 49,  24),
            S( 25, 109),   S(  8, 117),   S( 18, 122),   S( 17, 119),   S( 33,  91),   S(101,  37),   S(111, 135),   S(148,  52),
            S( 99, -28),   S( 49,  46),   S(  3, 105),   S( 16,  76),   S( 17,  64),   S( 85,  54),   S( 30,  44),   S( 98,  13),

            /* rooks: bucket 4 */
            S(-19, -13),   S( 24, -34),   S(-12, -12),   S(-12,   6),   S(-35,   3),   S(-60,  59),   S(  3, -17),   S(-80,  12),
            S(-29, -34),   S(-53, -17),   S(-35, -17),   S(  2, -52),   S( 41, -13),   S(  3,  15),   S(-35,  22),   S( 10,   8),
            S( -9, -27),   S(-32, -19),   S(-36,   0),   S( -9, -38),   S(-63,  12),   S(-62,  26),   S(-45, -10),   S(-52,  -3),
            S(-49, -36),   S( 13,  -8),   S(-13, -22),   S( -1, -26),   S( 21,  -5),   S(  7,  16),   S(-11,   5),   S(-24,  -3),
            S(-20, -22),   S( 25, -31),   S( 16,   5),   S( 66,   4),   S( 71,   4),   S( 65,  23),   S(  8,  33),   S( -8,  41),
            S( -2, -31),   S( 11,   1),   S( 15,  24),   S( -2,  35),   S( 55,   7),   S( 14,   1),   S( 50,  25),   S( 11,  36),
            S(-43,  -9),   S( 25,  49),   S( 52, -13),   S( 61,   4),   S( 43,   1),   S( -2,   9),   S(-13, -15),   S( -3,  13),
            S(  1, -35),   S(  7,  17),   S( 38, -18),   S( 31, -35),   S( 46,   0),   S(  0, -20),   S( -4,  -8),   S(  3,   2),

            /* rooks: bucket 5 */
            S(-31,  42),   S(-20,  15),   S( -2,  11),   S( 22,  11),   S(  0,  16),   S(-20,  41),   S(-16,  53),   S(-54,  48),
            S(  8,   8),   S(-30, -13),   S( 47, -54),   S( 28, -18),   S( -9,   9),   S(-15,  19),   S(-60,  33),   S(-25,  54),
            S(-35,  23),   S(-36,  17),   S( 41, -44),   S( 12,  -3),   S( 24, -20),   S( 17,  -3),   S(-37,  16),   S(-48,  24),
            S(-28,  30),   S( -2,  11),   S( 68, -32),   S( 31,  -2),   S( 41, -10),   S( 23,  35),   S(  9,  36),   S(-12,  49),
            S( 28,  23),   S( 27,   8),   S(  2,  17),   S( -9, -21),   S(  2,  27),   S( 97,  19),   S( 24,  23),   S( 51,  38),
            S(-12,  29),   S(-18,   1),   S( -1,  -8),   S(-12, -15),   S(  4,  16),   S( 25,  10),   S( 77,  20),   S( 31,  30),
            S( 28,   9),   S( 50, -12),   S(-13,  -2),   S( 42,  14),   S( 45,  -5),   S( 48, -12),   S( 97, -15),   S( 67,   4),
            S( -2,  30),   S( 12,  11),   S( 57, -11),   S(  9,  18),   S( 30,  31),   S(  3,  31),   S( 34,  27),   S( 37,  35),

            /* rooks: bucket 6 */
            S(-61,  57),   S(-28,  38),   S(-16,  26),   S( -1,  12),   S( 20,  -9),   S( 31, -13),   S( 41, -30),   S(-40,  31),
            S(-56,  32),   S(  8,   7),   S( 30,  -6),   S( 17,  -8),   S( 25, -26),   S(  6, -21),   S(-41,  13),   S(-23,  18),
            S(-36,  39),   S(  2,  13),   S( -3,   5),   S( -6,  11),   S( -9,   2),   S( 46, -16),   S( -5,  -3),   S( -4, -16),
            S(-36,  60),   S(-17,  45),   S( 58,   7),   S( 48,  -9),   S( 61, -21),   S(  5,   6),   S( 26, -11),   S( 23,  22),
            S( -1,  50),   S( 66,  18),   S(105,  16),   S( 75, -12),   S( 16, -22),   S(  4,  20),   S( 61,  -7),   S(110, -10),
            S( 49,  13),   S( 72,  -1),   S( 98,  -2),   S( 47, -30),   S( 12, -32),   S( 31,  49),   S( 15, -33),   S( 39,  15),
            S( 51,   9),   S(147, -30),   S(101, -27),   S( 82, -30),   S( 44, -13),   S( 30,  15),   S( 95, -26),   S( 62, -16),
            S( 94, -27),   S( 44,  28),   S(  3,  29),   S( 74, -16),   S( 77,  -5),   S( 42,  20),   S( 64,  10),   S( 38,  29),

            /* rooks: bucket 7 */
            S(-85,  24),   S(-81,  40),   S(-76,  58),   S(-64,  51),   S(-22,  -2),   S(-14, -13),   S(-32,   2),   S(-51, -18),
            S(-81,  32),   S(-27,   3),   S(-42,  26),   S(-67,  37),   S(-28, -15),   S(-27,  -8),   S( 18,   9),   S( 16, -68),
            S(-85,  36),   S(-93,  42),   S( -7,  -5),   S(-50,  30),   S(-17, -13),   S(-24,   6),   S( 49, -43),   S( 12, -64),
            S(-108,  53),  S( -5,  23),   S( -4,  16),   S( 39, -12),   S(  7,  -9),   S( 46, -15),   S( 11,  -4),   S(  7, -13),
            S( -4,  28),   S( 17,  27),   S( 56,  24),   S( 78,  -2),   S(168, -69),   S(136, -66),   S(103, -38),   S(-60, -39),
            S( -1,  25),   S( 35,  -6),   S( 99,  -1),   S( 49,  -7),   S( 41,  -3),   S( 36,  12),   S( 15,  32),   S(-10, -49),
            S(  4,  -5),   S( 59, -22),   S( 66, -12),   S(128, -49),   S(121, -48),   S(134, -47),   S( 62,   7),   S( 30, -23),
            S(-10, -18),   S( -9,  14),   S( 84, -23),   S(-12,   9),   S( 58, -16),   S(102, -13),   S( 11,  30),   S( 38, -20),

            /* rooks: bucket 8 */
            S(-54, -76),   S(-22, -41),   S( -3,  -4),   S( 13,  -2),   S( -4, -20),   S(-29,   5),   S(-17, -21),   S(-28,   7),
            S(-40, -105),  S(-26, -47),   S(-26,  29),   S(-32, -82),   S(-40, -45),   S( -6, -15),   S( -9,  -9),   S(-46, -41),
            S(  8, -13),   S(-14, -29),   S( 28,   6),   S(-38, -25),   S(-26,  55),   S( 10,  44),   S(  7,  63),   S(-20,   1),
            S( -1, -12),   S(  3,   6),   S(-15, -19),   S( 16,   8),   S( 10,  33),   S( 13,  23),   S(  5,  42),   S(  9, -22),
            S(-24, -48),   S( 24,  33),   S(  8,  11),   S( 13,  60),   S(  6,  17),   S(-27, -20),   S( 15,  68),   S( -9,  38),
            S(-16,  -9),   S(  0,   6),   S(-20,   0),   S( -4, -14),   S( -1,  40),   S(-32,   5),   S(  9,  -6),   S(  8,  23),
            S( 11,  48),   S(-11,  50),   S( 12,   1),   S( 17,   6),   S( 25,  31),   S(  7,  36),   S( 28,  23),   S(  0,  19),
            S(-16,  30),   S(  7,  -5),   S(-26,  39),   S( 41,  79),   S( -4,  53),   S(  8,  29),   S( -3,   3),   S( 11,  34),

            /* rooks: bucket 9 */
            S(-46, -65),   S( -4, -81),   S( -9, -98),   S(  7, -63),   S(-44, -42),   S(-24, -35),   S(-12, -42),   S( -6, -46),
            S(-74, -48),   S(-37, -72),   S(-17, -50),   S(-46, -43),   S(-34, -40),   S(-28, -30),   S(-35, -62),   S(-26, -21),
            S(-16, -29),   S(-24, -13),   S( 14,  13),   S( -2, -24),   S( 11, -18),   S(  1,  13),   S(  9,  38),   S(  4,  18),
            S( -9,  19),   S( -1, -43),   S(  0,  -9),   S(  4,  17),   S(-12, -33),   S(  0,   6),   S( -8,  11),   S( 21, -31),
            S(  1,  -9),   S( -6, -22),   S(-22, -46),   S(-14,  -3),   S(-38, -23),   S(-28,   3),   S(-12, -20),   S(  0,   0),
            S( -6,  -3),   S(-30, -12),   S(-26,  -4),   S(  8,  26),   S(-12,   6),   S( -4,  -5),   S(-19,  -4),   S(-16,  29),
            S(  8,   5),   S( 18, -15),   S( 17, -24),   S(-13,  12),   S( 27, -16),   S( 18,  16),   S(  8,  23),   S(-16,  -1),
            S( -6,  10),   S(  6,  23),   S( 10,  33),   S( -2,  15),   S(  3,  24),   S(  4,  49),   S(  6,  18),   S( -8,  23),

            /* rooks: bucket 10 */
            S(-11, -31),   S(-33,   2),   S(-10, -36),   S(  5, -46),   S(-14, -38),   S(-26, -56),   S( 21, -51),   S(-17, -30),
            S(-57, -13),   S(-27, -41),   S(-54, -33),   S(-71, -43),   S(-41, -50),   S(-12, -52),   S( -6, -28),   S(-30, -49),
            S(  3, -31),   S(-28, -14),   S(-10,  -3),   S(-53, -49),   S(-15, -39),   S(-10, -16),   S(-17, -14),   S(-33, -29),
            S(-25, -27),   S(-39, -35),   S( -4, -20),   S(-16,  -8),   S( -3,  -9),   S( -2,  16),   S(-17, -50),   S( -9, -46),
            S(  9, -20),   S( 14,   4),   S(-11, -44),   S(  2, -54),   S( -3,  14),   S( -2,   5),   S( 10, -21),   S( -6, -37),
            S( -1,   6),   S(  2,  -8),   S( -7, -21),   S( -5, -52),   S( -3,  -6),   S(-17,   6),   S(-10, -16),   S(-13, -12),
            S( -3, -13),   S( -4, -33),   S( -1, -24),   S(-23,  -4),   S( 17, -29),   S(-13, -18),   S( -2, -36),   S(-14, -16),
            S( -1,  13),   S( 21,  36),   S(  4,  38),   S(  5, -15),   S(-20,  23),   S(-26,  51),   S(-34,  14),   S( -4,  18),

            /* rooks: bucket 11 */
            S(-30,  -9),   S(-54,  -2),   S(-48,  -3),   S(-37,  18),   S(-86,  -2),   S( -8, -31),   S(  4, -31),   S(-41, -53),
            S(-25, -17),   S(-34, -15),   S(-55, -28),   S(-31,  14),   S(-30, -20),   S(  6, -21),   S(-31, -76),   S(-36, -60),
            S(-46,  46),   S(-14,  20),   S(-18,  31),   S(-17,  19),   S( 26,  -7),   S( -4, -23),   S(  5,  -4),   S(-22,  -4),
            S(-12,  -4),   S(  4,  -1),   S(-26,   6),   S( 10,  16),   S(  6,  -8),   S(-24, -32),   S( 14,  18),   S( -5, -27),
            S(-18,  18),   S( 22,  -3),   S(-14,  16),   S(-12,  23),   S( 43, -11),   S(  1, -13),   S( 28,  53),   S(-21, -68),
            S(  0, -12),   S( -6, -17),   S( 19, -12),   S( 43,  -5),   S( -2, -27),   S(  3,  12),   S(  4,  38),   S(-11,   6),
            S(-26,   8),   S(-41, -26),   S( 11,  -3),   S(  5,   9),   S(  7,  20),   S( -5,   8),   S(-27,  -9),   S( -8,   3),
            S(-20,   4),   S( 40,  29),   S( 21,  36),   S( 41,  38),   S(-11,  10),   S( 10,  30),   S( 26,  15),   S(-26,  25),

            /* rooks: bucket 12 */
            S(-46, -119),  S( -4,  -5),   S(-10, -30),   S(-19, -37),   S( -3, -10),   S(  7, -24),   S(-32, -65),   S(-20, -30),
            S(  2,   1),   S(  2,   9),   S( 14,  26),   S( -2,  -2),   S(  2,   7),   S( -7, -38),   S( -2,  12),   S( -7,  -7),
            S(-13, -26),   S( 13,  37),   S( 20,  36),   S( 10, -17),   S( -3, -15),   S( 19,  24),   S( 11,  53),   S(  2,  41),
            S( -2,  11),   S( 12,   3),   S( 15,  29),   S( 15,  25),   S( 19,  13),   S( 11,  21),   S(  0,  13),   S(  2,  32),
            S( 18,  27),   S( 14,  36),   S( -5,  31),   S(  7,   6),   S( 27,  59),   S(  4, -10),   S(  1,  13),   S( -3, -11),
            S(-11, -23),   S( 11,  37),   S( -2,  13),   S(-10,   5),   S(  2,  24),   S(  7, -13),   S(  4,  24),   S(  7,  11),
            S(-13,  16),   S( -8,  37),   S( 10,  38),   S(  1,  19),   S(  7,   5),   S( 18,  25),   S( -5,   7),   S( -8,  16),
            S(  1,  -1),   S(-20,   6),   S( -5,  -1),   S( 11,  26),   S( 13,  33),   S(  2,  15),   S( -1, -10),   S(  9,  30),

            /* rooks: bucket 13 */
            S(-36, -32),   S(-22, -47),   S(-35, -79),   S(-19, -43),   S( -1, -56),   S( -4,  -2),   S(-19, -39),   S(-10, -35),
            S(-12,   1),   S(-10, -31),   S(  1,  11),   S( -1,  -6),   S( 24,  34),   S(  9,  10),   S( 21,   7),   S( -5,  -5),
            S(-11,   4),   S(-20, -13),   S( -7,   8),   S( 13,  24),   S( 13,  30),   S(  2,  -9),   S( 13,  27),   S( -6,  -7),
            S(  7,  12),   S(  5,  13),   S( -7,   0),   S( 16,  18),   S(  7,  14),   S(-14, -12),   S( 11,  25),   S( -5,   4),
            S(  8,  15),   S(  2, -15),   S(  0,  -4),   S(  1,  -4),   S( -5,  30),   S(  8, -11),   S(  3,   6),   S( -3,   0),
            S(  6,  34),   S( -5,  10),   S( -9,   0),   S( -6,  33),   S(-10, -12),   S( 10,  20),   S( -1,  10),   S(  0,   6),
            S(  4, -32),   S(  1,  -8),   S(-10, -29),   S(  0,  38),   S( -7,  -6),   S( -1, -10),   S( -2, -11),   S( -2,  -4),
            S(  3,  13),   S(-12,  29),   S(-10,   0),   S(  8,  50),   S( -8, -11),   S(  2,  11),   S(  5,  35),   S( 13,  10),

            /* rooks: bucket 14 */
            S(  5, -34),   S(-35, -66),   S(-14, -25),   S(-21, -61),   S(-21, -49),   S( -2, -25),   S(-41, -68),   S(  0, -17),
            S(-12,   4),   S( 11,  27),   S( 10,   9),   S(-16, -35),   S(  1,  -2),   S(  0,   3),   S( -1,   2),   S( -8, -10),
            S( 13,  62),   S(-15,  17),   S( -6,  -1),   S(  2,  12),   S( 12,  21),   S(  5,   9),   S(  4,  22),   S(-25, -67),
            S( -4,  12),   S( 19,  22),   S(  3,  12),   S(  4,  -7),   S(-12, -23),   S(-10, -16),   S(  8,   2),   S(-14, -12),
            S(  5,  26),   S( 24,  23),   S( -2,   1),   S( -6,  -3),   S( -7, -34),   S( 15,   9),   S(-15, -41),   S(  6, -11),
            S( 18,  44),   S(  9,  24),   S( 11,  29),   S(-19, -40),   S( -3,  -9),   S(-11,  25),   S( -6,  10),   S(-12, -14),
            S( -2,  10),   S(  3,   5),   S(-10, -27),   S( -9, -26),   S( -1,  24),   S( 10,  20),   S(-17,  -2),   S(  4,  15),
            S(  7,  12),   S( -2,  -5),   S( 15,  10),   S(  9,  -4),   S( -8,  -3),   S(-13, -65),   S( -1,   0),   S( -8,  38),

            /* rooks: bucket 15 */
            S(-15, -35),   S(-18, -36),   S(-43, -85),   S(-25, -63),   S( -4, -18),   S(-11, -50),   S( -9, -19),   S(-18, -54),
            S(  8,  16),   S(-16, -19),   S( -1,  12),   S( 13,   4),   S(-11, -31),   S( -2,  -8),   S(  9,   5),   S( -3,   1),
            S(  7,   6),   S( -2, -16),   S( 16,  12),   S( 10,  12),   S( 10,  12),   S( -5, -23),   S(  5,  25),   S(  4,  22),
            S(  1, -13),   S(  7,   7),   S( 14,  29),   S(  8, -11),   S(  9,  33),   S( -3,   7),   S( -3,  -5),   S(  6,   2),
            S(  9,  16),   S(  3,  -4),   S(  1, -27),   S( -4,  16),   S(  3,  17),   S( -3,  -5),   S( -2,  20),   S(  8, -29),
            S( 20,  49),   S( 15,  16),   S(  3,   3),   S(  0,   9),   S(  3,   4),   S(-26,   9),   S( -3,  21),   S( 12,  14),
            S(  8,   8),   S( -2,  16),   S( 23,  46),   S(  5,  19),   S(  9,  19),   S(  8,   9),   S( -2,  15),   S( -4, -24),
            S(  8,  45),   S(  3,  30),   S(  9,  39),   S(  8,  38),   S(  0,   0),   S( -4,  -6),   S(  6,  38),   S(-19,  -3),

            /* queens: bucket 0 */
            S(  7, -25),   S(-15, -44),   S(-19, -46),   S( 16, -99),   S(  6, -36),   S( 11, -28),   S(-53, -33),   S(-11, -24),
            S( -7, -47),   S( 26, -88),   S( 26, -94),   S(  4, -19),   S( 22, -40),   S( -7, -22),   S(-23, -34),   S(-31,  -2),
            S(-11,  26),   S( 13, -31),   S( 29, -42),   S(  6,   6),   S(  1,  22),   S(  0,   3),   S( -9, -25),   S(-90, -40),
            S(-15,  -9),   S( 34, -23),   S( 10,  38),   S( -5,  46),   S(-12,  86),   S(-10,  51),   S(-52,  40),   S(-34,   4),
            S(-34,  19),   S( 14,  46),   S( 14,  16),   S(  9,  68),   S(  3,  67),   S(-26,  96),   S(-79, 124),   S(-39,  -1),
            S(-18, -14),   S( 35,  31),   S( 67,  28),   S(-15,  53),   S(-36,  68),   S(-44,  86),   S(-70,  21),   S(-48, -12),
            S(  0,   0),   S(  0,   0),   S(  4,  -2),   S(-43,  38),   S(-44,  15),   S(-78,  74),   S(-90,  74),   S(-133,  36),
            S(  0,   0),   S(  0,   0),   S( -6, -19),   S(-12, -22),   S(-30,  29),   S(-64,   3),   S(-71,  32),   S(-74, -16),

            /* queens: bucket 1 */
            S( 20,  -5),   S(  1, -12),   S( 24, -64),   S( 33, -73),   S( 40, -26),   S( 22, -46),   S( -8, -39),   S( -2,  29),
            S(-27,  54),   S( 16,  29),   S( 42, -25),   S( 26,  20),   S( 47,   3),   S(  0,  27),   S(-20,  26),   S(-18,   2),
            S( 20,  23),   S( 28,  -1),   S( 18,  42),   S( 18,  91),   S( -8,  96),   S( 32,  66),   S( -2,  47),   S( 24, -29),
            S( 43, -40),   S(  7,  56),   S( 27,  60),   S( 38,  88),   S( 37,  66),   S( 27,  55),   S( -1,  50),   S(-24,  95),
            S( 52,  -1),   S( 49,  25),   S( 63,  44),   S( 26,  23),   S( 62,  55),   S( 25,  27),   S( -5,  97),   S( 11,  94),
            S( 78, -20),   S( 98,  -8),   S( 73,  64),   S(101,  48),   S( 48,  44),   S(  2,  69),   S( 54,  37),   S(  9,  11),
            S( 91,  -7),   S( 40, -12),   S(  0,   0),   S(  0,   0),   S( 27,  28),   S( -3, -10),   S( -7,  73),   S(-42,   2),
            S( 95, -31),   S( 58, -33),   S(  0,   0),   S(  0,   0),   S( 22,  14),   S( 41,  20),   S( 94,  20),   S(-13,  20),

            /* queens: bucket 2 */
            S( 40, -48),   S( 17,  26),   S( 28,  40),   S( 41,  -8),   S( 48, -32),   S( 24, -16),   S( -9, -23),   S( 15,  32),
            S( 15,   5),   S(  3,  57),   S( 33,  25),   S( 41,  47),   S( 48,  19),   S( 11,  47),   S( 10,  56),   S( 27,  76),
            S( 30,  17),   S( 25,  56),   S( 14, 105),   S( 13,  91),   S( 21,  81),   S( 17,  82),   S( 29,  38),   S( 20,  47),
            S(  4,  51),   S( 13, 102),   S( 19,  82),   S(  9, 125),   S( 39,  85),   S( 16, 108),   S( 27,  69),   S( 29,  61),
            S( -4,  87),   S( -8,  71),   S(  9,  76),   S( 31,  88),   S( 31,  89),   S(102,  27),   S( 75,  42),   S( 66,  47),
            S(-55, 107),   S(-33,  87),   S( 13,  68),   S( 74,  26),   S( 44,  45),   S( 99,  58),   S( 96,  45),   S( 21, 111),
            S(-31,  82),   S(-19,  57),   S(  7,  31),   S( 83,  14),   S(  0,   0),   S(  0,   0),   S( 41,  31),   S( 49,  23),
            S(-10,  26),   S( 34, -21),   S( 37, -16),   S( 33,  23),   S(  0,   0),   S(  0,   0),   S( 72,  -7),   S(  7,  51),

            /* queens: bucket 3 */
            S(-63,  58),   S(-39,  40),   S(-25,  47),   S(-16,  59),   S(-31,  38),   S(-21, -12),   S(-25, -33),   S(-50,   2),
            S(-58,  45),   S(-47,  49),   S(-27,  66),   S(-16,  91),   S(-18,  83),   S(-15,  34),   S(  6, -17),   S( 24, -41),
            S(-60,  59),   S(-39,  94),   S(-41, 130),   S(-42, 151),   S(-36, 119),   S(-29, 102),   S(-18,  69),   S(-21,  17),
            S(-49,  73),   S(-64, 136),   S(-45, 160),   S(-40, 172),   S(-38, 154),   S(-18, 104),   S(-15,  78),   S(-17,  47),
            S(-48, 100),   S(-57, 153),   S(-65, 188),   S(-31, 178),   S(-26, 146),   S(-16, 134),   S(-19, 124),   S(-18,  59),
            S(-82, 120),   S(-71, 154),   S(-78, 206),   S(-66, 192),   S(-39, 151),   S( 20,  88),   S(-28, 112),   S(-36, 106),
            S(-84, 102),   S(-95, 137),   S(-96, 178),   S(-63, 140),   S(-91, 159),   S(-36,  92),   S(  0,   0),   S(  0,   0),
            S(-143, 124),  S(-119, 121),  S(-93,  98),   S(-63, 107),   S(-45,  84),   S(  4,  34),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-39,  17),   S(-50, -34),   S(  0,  18),   S( 11,  -9),   S(-12,   2),   S(-28, -15),   S(-58, -40),   S( 28,  32),
            S( -2,  -5),   S(  1,   3),   S( -5,  37),   S(-11,   1),   S( -9,  34),   S( -5,  23),   S(-45, -26),   S(-32, -34),
            S( 12,  38),   S(-13, -44),   S( 32, -19),   S( 23,   5),   S( 25,  16),   S( 13,  11),   S(-16,  -4),   S( 34,  25),
            S(-45, -33),   S( 19, -24),   S( -1,  -5),   S(-12,  17),   S( 27,  51),   S(  9,  40),   S(-27,  -3),   S( -8,  -1),
            S(  0,   0),   S(  0,   0),   S( 19,  -8),   S( 48,  29),   S( 30,  48),   S(  7,  32),   S(  2,   2),   S( 13,  29),
            S(  0,   0),   S(  0,   0),   S( 16,  -9),   S( 32,  32),   S( 43,  30),   S( 31,  54),   S( 31,  57),   S(  7,   6),
            S(-13, -36),   S( 20,  13),   S( 71,  47),   S( 64,  48),   S( 56,  -5),   S( 14,   2),   S( 17,  21),   S( -9,  19),
            S( 33,  -5),   S(-32, -44),   S( 19, -10),   S( 35,  -7),   S(-17, -16),   S( 14,  38),   S( -8,  11),   S( 29,  15),

            /* queens: bucket 5 */
            S( 21,  21),   S( 47,  24),   S( 25,  19),   S( -8,  48),   S( 39, -19),   S( 47,  59),   S( 20,  18),   S(  6,   3),
            S( 20,  16),   S( 16,  -6),   S( 15,   9),   S( 15,  10),   S( -8,  72),   S(-12,  10),   S( 59,  41),   S( -5,  -5),
            S( 36,  21),   S( 51,  -6),   S( 30,  -5),   S(  2, -29),   S( 23,   0),   S( 28,  15),   S(  3,  33),   S( 26,  32),
            S(  1, -14),   S( 27, -21),   S(  6, -26),   S( 25,  27),   S( 83,  10),   S( 37,  31),   S( 20,  23),   S( 20,  33),
            S( 19, -20),   S( 22, -54),   S(  0,   0),   S(  0,   0),   S(  3,  22),   S(  4,  15),   S( 54,  46),   S( 24,  41),
            S( 51,   7),   S( 27,   9),   S(  0,   0),   S(  0,   0),   S( 26,   5),   S( 47,  41),   S( 37,  29),   S( 64,  55),
            S( 81,   4),   S( 74,  12),   S( 53,  56),   S( 17,  10),   S( 58,   7),   S(115,  10),   S( 53,  65),   S( 65,  34),
            S( 59,  57),   S( 62,  21),   S( 78,  16),   S( 40,  -2),   S( 52,  25),   S( 43,  27),   S( 61,  45),   S( 56,  26),

            /* queens: bucket 6 */
            S( 61,  66),   S( 28,  26),   S( 30,   7),   S( 40,  17),   S( 49,   8),   S( -6,   2),   S( 17,   9),   S( 15,  23),
            S( 12,  39),   S( 44,  39),   S( 54,  78),   S( 60,  29),   S( 39,  46),   S( 18, -10),   S(-22,   8),   S( 25,  34),
            S( 17,  46),   S( 16,  28),   S( 22,  35),   S( 61,  23),   S( 34,   6),   S( 48,   0),   S( 63,  37),   S( 85,  67),
            S( 16,  31),   S( 14,  12),   S( 69,   4),   S(103,   3),   S( 45,  -4),   S( 40,   6),   S( 91,  22),   S(105,  50),
            S( 18,  51),   S( 25,  45),   S( 46,  55),   S( 35,  20),   S(  0,   0),   S(  0,   0),   S( 39,   2),   S( 96,  45),
            S( 37,  51),   S( 24,  47),   S( 58,  31),   S( 16,   7),   S(  0,   0),   S(  0,   0),   S( 77,  45),   S( 90,  43),
            S( 53,  35),   S(  9,  29),   S( 66,   8),   S( 40,   6),   S( 30,  16),   S( 92,  71),   S(111,  30),   S(134,  11),
            S( 23,  37),   S( 56,  19),   S( 57,  21),   S( 69,  26),   S(117,  -4),   S( 66,   2),   S( 92,   0),   S(116,  17),

            /* queens: bucket 7 */
            S( -9,  18),   S( -1,   7),   S( 10,  41),   S( -3,  26),   S(  2,  35),   S(-27,  51),   S( -4,  11),   S( 10, -10),
            S(-31,  37),   S(-51,  28),   S(-19,  84),   S( -7,  98),   S( 15,  37),   S( 22,  39),   S( 25, -19),   S(-16,  -2),
            S( 10,  19),   S( -1,  45),   S(  3,  79),   S( 55,  47),   S( 34,  37),   S( 68, -14),   S( 56, -59),   S( 40,  13),
            S( -6,  39),   S( 22,  44),   S( 23,  73),   S( 62,  72),   S( 87,  42),   S( 89, -11),   S( 84, -29),   S( 36, -18),
            S( 11,  37),   S(  8,  52),   S(  3, 142),   S( 45,  92),   S( 76,  22),   S( 77,  24),   S(  0,   0),   S(  0,   0),
            S(-40,  61),   S( 28, 101),   S( 12,  98),   S(  8,  84),   S( 77,  29),   S( 74,  50),   S(  0,   0),   S(  0,   0),
            S(-24,  62),   S(-36,  47),   S( -2,  69),   S( 29,  68),   S( 49,  53),   S( 75,  25),   S( 54,   1),   S( 48,  19),
            S( 11,  33),   S( 49,  29),   S( 70,  41),   S( 70,  18),   S( 56,  33),   S( 35,  10),   S(-24,  -8),   S( 76,  11),

            /* queens: bucket 8 */
            S(-20, -41),   S( -1, -34),   S(-19, -44),   S( 15,  10),   S(-23, -42),   S(  8,  -9),   S(  0,   2),   S(  5,  13),
            S( -9, -24),   S(-20, -34),   S(-15, -49),   S( -6, -17),   S( -8, -34),   S( -6, -16),   S( -1,  -3),   S( -2,   2),
            S(  0,   0),   S(  0,   0),   S( -8, -26),   S(-13, -43),   S( -1,  -2),   S( -1,  -2),   S(-14, -15),   S(  4,   7),
            S(  0,   0),   S(  0,   0),   S(  8,   7),   S( -8, -24),   S(-13,   0),   S( -4,  -5),   S( 19,  32),   S( 11,  -1),
            S(  1, -14),   S(  6,   4),   S( 20,  17),   S( 23, -11),   S(  1, -31),   S( 25,  42),   S( 23,  34),   S( -3,   1),
            S( -9, -30),   S( 11, -15),   S( 33,  23),   S( -3, -48),   S( 22,  25),   S( 27,  22),   S( 17,   4),   S( -6, -13),
            S(-17, -40),   S(  3, -13),   S( 26,  31),   S( 39,  60),   S( 14,  22),   S(  2,  14),   S(  2,   3),   S( -1,  -2),
            S( -6,  -6),   S( 22,   6),   S( 34,  34),   S( -8, -14),   S(  9,   7),   S( -6,   7),   S( 13,  34),   S(-17, -31),

            /* queens: bucket 9 */
            S( 17,   3),   S(-40, -66),   S(-15, -38),   S( 14,   3),   S(  2, -20),   S(-14, -24),   S( -3,  -3),   S(  0, -19),
            S( -5, -13),   S( -6, -31),   S(-19, -51),   S( -4, -30),   S(-32, -67),   S(-10, -27),   S(  7,  -6),   S(  3, -10),
            S(-16, -45),   S(-11, -27),   S(  0,   0),   S(  0,   0),   S( 15,  13),   S(  1, -12),   S(-12, -32),   S(  4,  -5),
            S(  4, -15),   S(-11, -35),   S(  0,   0),   S(  0,   0),   S(-11, -16),   S(  1, -11),   S( 14,  17),   S(  4,  14),
            S( -5, -21),   S( -3,   2),   S( 12,  15),   S(-12, -12),   S(  1, -22),   S(  8,  30),   S(  5, -21),   S(  6, -11),
            S( 30,  35),   S( 14, -18),   S(  5,   1),   S( -1, -22),   S( 13,   7),   S( 19,   8),   S(  9,   4),   S(  3,  10),
            S(  9,  10),   S( 30,  -1),   S(  0,  -4),   S(-21,   1),   S( 31,  34),   S( 62,  68),   S(  7,  31),   S( 14,  -3),
            S( -2, -31),   S( 24,   4),   S(  5,   0),   S(  3,  -6),   S( 25,  25),   S(  0,   6),   S( 11, -18),   S( 16,  -4),

            /* queens: bucket 10 */
            S( 22,  16),   S(  9,   5),   S(  1, -18),   S(-22, -58),   S( -9, -32),   S( -3, -17),   S( -7, -39),   S( -3, -10),
            S(  4,   0),   S(-16, -25),   S(  1, -28),   S(-30, -68),   S( -9, -13),   S(  4, -14),   S( -4, -24),   S(  7,   3),
            S(  8,  12),   S(  0,  -4),   S(  3,   6),   S(  7,  -1),   S(  0,   0),   S(  0,   0),   S(  6,   7),   S(-17, -26),
            S( -7, -18),   S(  9,   7),   S(  5,   3),   S( 12,  -1),   S(  0,   0),   S(  0,   0),   S(  5,  -1),   S( -3, -10),
            S( 10,  13),   S( 13,   8),   S( -4,  -3),   S( 36,  45),   S(  9,  19),   S(  4,   0),   S( -8, -23),   S( 27, -11),
            S( -1, -12),   S(  1,  -4),   S( 20,  16),   S( 19,  31),   S(  9,  21),   S( 24,  34),   S( 19,  -3),   S(  7,  -6),
            S(  9,   8),   S( 26,  46),   S( 28,  38),   S( 29,  41),   S( 21,  37),   S( 20,  11),   S( 11,   4),   S( -4, -25),
            S(-17, -35),   S( 24,  34),   S( 28,  26),   S( -6,   8),   S( 37,  44),   S( 27,  34),   S(  9,  31),   S( 11,   0),

            /* queens: bucket 11 */
            S( -4,   4),   S(-14, -23),   S( -2,  -8),   S(-45, -45),   S(-15, -28),   S(-41, -68),   S(  7,  -8),   S(-18, -26),
            S(  4,  17),   S(  1,   5),   S(-24, -25),   S(  0,  17),   S( 14,   9),   S( 18,   6),   S(  6, -17),   S(-15, -20),
            S(  1,   2),   S( 22,  13),   S(-16,  20),   S(  7,   4),   S( -4, -28),   S( -3, -22),   S(  0,   0),   S(  0,   0),
            S(-12,  -3),   S(  2,  23),   S(  0,  16),   S( 16,  25),   S(-10, -37),   S(  7,  20),   S(  0,   0),   S(  0,   0),
            S(  2,  19),   S(  8,   1),   S( 15,  42),   S( 16,  40),   S( 37,  61),   S( 29,  30),   S( -8, -19),   S(-11, -32),
            S( -3,   8),   S(  5,  14),   S(-12,  -4),   S( 20,  43),   S( 13,  18),   S( 11,   7),   S(  3,  -5),   S(  5, -17),
            S(  0,   7),   S(  5,   8),   S( 25,  48),   S(  4,   8),   S( 11,  63),   S(  7, -29),   S(  6,  10),   S( 14,  13),
            S(-20, -39),   S( -2,   6),   S( -8,   1),   S(  1,  35),   S( 28,  44),   S( 17,  15),   S(  5,   3),   S( 35,  25),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  3,  10),   S(-22, -18),   S( -2,   6),   S(-16, -26),   S( -5, -11),   S( -6, -15),
            S(  0,   0),   S(  0,   0),   S(  8,   6),   S( -6,  -6),   S(-19, -12),   S(-10, -31),   S( -4, -15),   S(  4,   5),
            S( -6,  -8),   S(  1,   1),   S( -1,   3),   S( -5, -33),   S( 17,  32),   S( -5,  10),   S(  7,   9),   S(  9,  14),
            S(-11, -20),   S(  3,  -6),   S( -6,   8),   S(  6,   9),   S( 16,  30),   S( -1,   1),   S( -2,   0),   S( -4,  -6),
            S(-12, -19),   S(-12, -21),   S(  7,   0),   S(  4,  12),   S( 15,  40),   S(-16, -49),   S(-10, -22),   S(  1,   1),
            S( 16,   8),   S( -7, -14),   S(  3,   4),   S(-15,  -3),   S(  7,  13),   S( -9,  -9),   S(-16, -20),   S( -1, -14),
            S( -8, -14),   S( -4,  -5),   S(-14, -15),   S( 10,   2),   S(-13, -35),   S(-12, -13),   S(  5,   0),   S(  0, -29),
            S( 10,  22),   S( -3,  -9),   S(  8,  22),   S(  1,   4),   S(-16, -15),   S( -9,  -3),   S(-12,   5),   S( -7, -13),

            /* queens: bucket 13 */
            S(-24, -45),   S(-17, -38),   S(  0,   0),   S(  0,   0),   S(-17, -22),   S(-15, -31),   S(  4,   8),   S(-10, -25),
            S( -5, -37),   S( -9, -22),   S(  0,   0),   S(  0,   0),   S(-15, -27),   S(-26, -38),   S( -3,  -8),   S( -5,  -8),
            S(-17, -38),   S(-11, -20),   S( -5,  -4),   S( -1,  -4),   S(-30, -60),   S(  0,   2),   S(-19, -22),   S( -5,  -3),
            S( 11,  10),   S(-28, -39),   S(  9,  21),   S( 12,   7),   S( 23,  36),   S( 17,  33),   S(-21, -37),   S(-15, -21),
            S(  3,   0),   S(  6,  -2),   S(-17, -29),   S( 22,  42),   S( 11,  17),   S(  8,  -4),   S(  1,   5),   S( -3, -15),
            S( -3,   1),   S(-17, -28),   S( -5, -18),   S( 18,  39),   S(  4, -23),   S(  9,  17),   S( -2,  -3),   S( -7, -19),
            S(  3,  -3),   S(  0,  -4),   S( 11,  35),   S( -3,   0),   S( -4, -13),   S( 12,   8),   S(-22, -41),   S( -9, -11),
            S(-22, -25),   S( -6,  -7),   S( -1,  -5),   S( -1,  -2),   S( -2,  -1),   S( -2,   3),   S(  0, -10),   S(-15, -15),

            /* queens: bucket 14 */
            S( -3, -18),   S( 10,  13),   S( -9, -25),   S(  2,  -9),   S(  0,   0),   S(  0,   0),   S( -2,  -9),   S( -9, -19),
            S(-17, -37),   S(-16, -24),   S(-11, -31),   S(-10, -14),   S(  0,   0),   S(  0,   0),   S( -8, -18),   S(-17, -36),
            S( -8, -13),   S( -7, -32),   S( -3,  -7),   S(-15, -24),   S(  6,  11),   S( -7,  -3),   S(-15, -18),   S(-19, -29),
            S( -7,   0),   S(  0,   0),   S(  3,  11),   S(-12, -29),   S( -9, -25),   S( -6, -14),   S( -2, -23),   S( -7, -11),
            S( -8,  -8),   S( -6, -14),   S(-21, -41),   S( 13,  15),   S(  4,  17),   S( -7, -12),   S(  2,  35),   S(  8,  15),
            S( -2,  -9),   S( -1, -22),   S(-11, -15),   S(-12, -26),   S( -1,   0),   S(-12, -11),   S( -7, -25),   S(-15, -17),
            S( -7, -10),   S( -2, -10),   S(  7,  13),   S( -1,  -5),   S(  9,  12),   S( 12,  24),   S( -1,  -4),   S(-10, -16),
            S(-10, -25),   S( 10,   2),   S(-10,  -9),   S(  4,   2),   S(  9,  17),   S( -3,   5),   S( -1,  14),   S(  9,   0),

            /* queens: bucket 15 */
            S(  0,   0),   S( -3,  -5),   S(  3,  -9),   S(-17, -25),   S(  9,  23),   S(-10,  -7),   S(  0,   0),   S(  0,   0),
            S( -4,   0),   S(  0,   5),   S(-22, -26),   S(-12, -20),   S( -4,  -8),   S(  2,   3),   S(  0,   0),   S(  0,   0),
            S(  0,  -2),   S(  5,   5),   S(-14, -18),   S(-11, -13),   S( -9, -21),   S(  5,   3),   S(  0,   5),   S(  0,  -5),
            S( -2,  -2),   S(-13, -17),   S(  0,   4),   S( 12,  29),   S( -4,   4),   S( -1,  23),   S( -3,  10),   S( -6, -12),
            S( -3, -11),   S( -3,  -8),   S( -5, -17),   S( 10,  15),   S(  4,  34),   S( 20,  37),   S(  5,  15),   S( -8, -22),
            S( -1,  -7),   S( -3,   0),   S( -6, -18),   S( -7, -10),   S(  1,   9),   S(-14, -13),   S( 17,  39),   S( -1,   4),
            S( -5, -15),   S( -1,   0),   S( -9,   3),   S(  3,   0),   S(-12, -10),   S( -6,  -5),   S(  8,  10),   S( -6, -18),
            S(  3,   0),   S( -9, -25),   S( -7, -17),   S(  6,   3),   S(-16,  -4),   S(  3,   9),   S(  3,   2),   S(-15,  -6),

            /* kings: bucket 0 */
            S( -2, -31),   S( 25,  -5),   S( 12,  -4),   S(-27,   1),   S(-29,  19),   S( 10, -21),   S( -9,   6),   S(  5, -54),
            S(-15,  38),   S( -8,   0),   S(-15,   9),   S(-62,  39),   S(-51,  45),   S(  0,   6),   S(-13,  39),   S(  7,  18),
            S( 10,  -7),   S( 60, -27),   S( -7,   2),   S(-15,  -1),   S(-27,   2),   S( -8,  -6),   S(-35,  16),   S( 36, -32),
            S( -9, -34),   S( -9, -23),   S( 10, -27),   S(-28,  -6),   S(-55,  25),   S(-52,  24),   S(-24,  29),   S(-13,  21),
            S(-46, -123),  S(  2, -56),   S( -1, -26),   S( 11, -27),   S(-50,   8),   S(-31,   3),   S(-19,  21),   S( 21,   8),
            S(  2, -110),  S(  3, -11),   S( -6, -30),   S(-41, -27),   S(  4, -32),   S(-35,  24),   S( 10,  11),   S(-23,   9),
            S(  0,   0),   S(  0,   0),   S( -3, -33),   S( 18, -25),   S(-21,  18),   S( -9,   1),   S(-30,  27),   S(-14,  14),
            S(  0,   0),   S(  0,   0),   S(-10,   0),   S(  1,  -4),   S( 13,   5),   S(  4,  25),   S( 13,   9),   S( 12,   3),

            /* kings: bucket 1 */
            S( 27, -30),   S( 35, -26),   S( 21, -21),   S( 24,  -7),   S(-12,   1),   S(  6,  -7),   S(  0,   4),   S( 19, -23),
            S( 15,  -4),   S( 14,  13),   S(  5,  -4),   S(-53,  33),   S(-25,  17),   S(-13,  15),   S(  2,   7),   S(  5,   6),
            S( -1,  -5),   S(  9, -24),   S(  2, -15),   S( 15, -10),   S(-49,   5),   S( -5, -12),   S( 34, -20),   S( 39,  -6),
            S( 10,  -9),   S(-14, -11),   S(-15,  -3),   S(-18,   7),   S( 10,  11),   S(-11,   5),   S( 35,  -8),   S(-26,  32),
            S(-31, -42),   S(-11, -58),   S( -9, -48),   S(-36, -29),   S( 17, -17),   S( -2, -40),   S( -6,   5),   S( 17,  -5),
            S(-32,   8),   S(-98,  -5),   S(-30,  19),   S(  3,   5),   S(-39, -12),   S(-19,  14),   S( 13,   1),   S( -2,   0),
            S(-26, -46),   S(-19, -13),   S(  0,   0),   S(  0,   0),   S(-42,  -1),   S(-51,  26),   S( -7,  52),   S( -3, -45),
            S(-37, -116),  S(-14,  -5),   S(  0,   0),   S(  0,   0),   S(-10,   2),   S(-15,  32),   S(  2,  33),   S(-10, -75),

            /* kings: bucket 2 */
            S( 21, -48),   S( 14,   1),   S( 24, -18),   S( 21, -16),   S( -4,   8),   S( 25, -20),   S( -1,  17),   S( 26, -31),
            S( 60, -42),   S( -9,  26),   S( -5,   4),   S( -1,   6),   S(-22,  16),   S( -1,  -1),   S( 15,  -2),   S( 16,  -1),
            S(-41,  -6),   S(-19, -16),   S(  1, -20),   S(-23,  -6),   S(-14,  -5),   S( 13, -22),   S( 42, -17),   S( 32, -18),
            S(-11,  21),   S(-14,  20),   S( 12,   6),   S( -7,   8),   S( 28,  -4),   S( -5, -10),   S( 25, -27),   S( 42, -10),
            S(  0, -13),   S( 42, -26),   S(  2, -36),   S(  9, -30),   S( 31, -55),   S(-22, -41),   S( 14, -41),   S( 24, -61),
            S(  8,   5),   S( -4,  -9),   S(-26,  -2),   S(-51,   0),   S( -1,  -7),   S(-16,  50),   S(-60,  13),   S(-26, -28),
            S(-14,   2),   S(  0,  36),   S(-69,  -2),   S(-11,  13),   S(  0,   0),   S(  0,   0),   S(-23,  20),   S(-34, -34),
            S(-19, -63),   S(-19, -22),   S(-21, -39),   S( -3,  24),   S(  0,   0),   S(  0,   0),   S(-11, -20),   S(-35, -108),

            /* kings: bucket 3 */
            S(  5, -46),   S( 18,  -8),   S( 29, -20),   S(-11,  -4),   S( -8, -12),   S( 25, -19),   S( -3,  19),   S(  5, -30),
            S( 14,   7),   S(-25,  43),   S(-10,   0),   S(-40,  24),   S(-45,  28),   S(  2,  -1),   S( -3,  17),   S( 13,   7),
            S( 30, -45),   S( -4,   8),   S(  5, -17),   S(-37,   4),   S( -3,   6),   S( 16, -17),   S( 50, -17),   S( 50, -13),
            S(-14,  20),   S(-86,  46),   S(-41,  -3),   S(-38,  23),   S(-48,  15),   S( -7, -23),   S(-41,   0),   S(-29, -16),
            S(-22,  39),   S(-13, -14),   S(-31,  -9),   S(-37,  -7),   S( 14, -39),   S( 75, -72),   S( 47, -73),   S(-44, -58),
            S( -8, -25),   S( 40,   2),   S( -1, -15),   S( -9, -16),   S( 50, -35),   S( 57, -57),   S( 65,  10),   S( 62, -128),
            S(-25,   0),   S( 52,   6),   S( 15,  -9),   S( 31, -29),   S( 19, -30),   S( 43, -44),   S(  0,   0),   S(  0,   0),
            S(  4,   5),   S(  1, -17),   S(-12,  23),   S( 14,  -7),   S( -1, -76),   S(  4,  10),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-55,   0),   S( 12,  46),   S( 15,  18),   S(  2,  23),   S( 32,  -8),   S(  6,  -7),   S(-17,  22),   S( 56, -49),
            S(-46,  29),   S( 33,  16),   S(-14,  15),   S( -6,  -4),   S( 39,  -5),   S(  7,   2),   S( 57, -24),   S(-20,  17),
            S( -2,  33),   S( 23, -27),   S( 21,  -4),   S(-12,   2),   S(-23,   6),   S( 30, -29),   S(-43,  14),   S( 62, -43),
            S( -2,  -9),   S(-18,   3),   S(-22,  28),   S( -3,   7),   S(-20,  12),   S(-11,  24),   S( 16,  22),   S( 29,  -8),
            S(  0,   0),   S(  0,   0),   S( -8,  15),   S(-19,   3),   S(-38,  12),   S(-37,   0),   S(-31,  11),   S(-13,  -9),
            S(  0,   0),   S(  0,   0),   S(  3, -21),   S(  2,  32),   S(-27,  20),   S(-31,   4),   S(  7, -33),   S(  3,  18),
            S( -2, -25),   S( -9, -16),   S( -6, -32),   S( 13,  21),   S( -2,  29),   S(-40, -34),   S(-16,  24),   S( -3, -13),
            S( -2, -26),   S( -3,   1),   S(-13, -18),   S( -7,  -8),   S(  9,  22),   S( -7,   8),   S(-14,   8),   S(  9,  29),

            /* kings: bucket 5 */
            S( 52,  11),   S(-12,  16),   S(-51,  38),   S(-40,   8),   S(-39,  26),   S( -7,  15),   S( 27,   3),   S( 23,   3),
            S(-34,   3),   S( 33,   3),   S( 66, -17),   S( 31,  -2),   S( 31, -14),   S( 29,  -9),   S( 14,  10),   S( 49, -27),
            S(  8,  -5),   S( -4, -12),   S(  3,  -4),   S(  6, -11),   S( 20,  -8),   S(-49,  -3),   S( -6,   8),   S( 51, -14),
            S( 10,  -6),   S(  3,  -8),   S(  1,  -1),   S(-10,  16),   S(  9,  20),   S(-10,  13),   S(  0,   5),   S( 19,  -3),
            S( -3, -21),   S(-42, -45),   S(  0,   0),   S(  0,   0),   S(-33,   4),   S(-29,  -4),   S(-16,  -5),   S(-20, -13),
            S( -9, -37),   S(-39, -31),   S(  0,   0),   S(  0,   0),   S(-25,  20),   S(-39,   8),   S(-12,  -9),   S(-10,   9),
            S(-12, -27),   S(-55,  30),   S(  1,  36),   S(  1, -11),   S(-20,  44),   S(-46,  22),   S(  5,   6),   S( 13,  26),
            S(-12, -120),  S( -1,  18),   S(-11, -29),   S( -6, -29),   S(-13, -51),   S( -6, -13),   S(-14, -12),   S( -7, -14),

            /* kings: bucket 6 */
            S( 25, -33),   S( 51, -11),   S( 17,  -3),   S(-10,  10),   S(-19,  26),   S(-23,  22),   S(  2,  18),   S( -8,   7),
            S( 40, -11),   S( 25,   8),   S( 20,  -6),   S( 28,  -9),   S( 20,  -5),   S( -7,   8),   S( 19,  -1),   S( 19,   0),
            S( -7, -14),   S(-26,   3),   S(-18,  -4),   S(-14,   0),   S( 11, -10),   S(-53,   7),   S( 12,  -2),   S(-20,  17),
            S( 15,   9),   S( 42, -10),   S( 29, -15),   S( 37,  -4),   S( 58,  -1),   S(-13,   0),   S(-19,   4),   S( 14,  -3),
            S( 11, -12),   S( 15, -37),   S(-34, -20),   S(  4, -18),   S(  0,   0),   S(  0,   0),   S(-35, -22),   S(-58, -22),
            S(-16,  -4),   S(  3,   7),   S(-43,   9),   S(  5, -14),   S(  0,   0),   S(  0,   0),   S(-29, -12),   S(-37, -22),
            S(  2,  -4),   S(-13,  18),   S(-56,  12),   S( -7,   1),   S(-11,  10),   S(-11, -44),   S(-33, -36),   S(  0, -48),
            S( -1,   5),   S( -5, -21),   S( -5,  43),   S(-13, -42),   S( -1, -21),   S(-12, -44),   S(-18,  -4),   S(  0, -38),

            /* kings: bucket 7 */
            S( 36, -29),   S(-11,   0),   S(-42,  -5),   S(  8,   0),   S(-29,  13),   S(-20,  20),   S(-34,  34),   S(-41,  17),
            S( 14,   2),   S(  0,  -9),   S(-31,  11),   S(-19,  -1),   S(-49,  21),   S(-42,  23),   S(  7,  -3),   S( 11,   4),
            S( 26, -34),   S(-38,   6),   S(-49,   9),   S(-15,  -5),   S(-47,   8),   S(-33,  14),   S( 26,  -5),   S(-50,  23),
            S(-42,  34),   S(-10,   9),   S( -1, -10),   S(  2,   8),   S( 44,  -6),   S( 50, -32),   S(  2, -10),   S( 25, -10),
            S(-11,  -3),   S(  3,  -3),   S( 12, -34),   S(  7, -17),   S( 21, -28),   S( 10, -16),   S(  0,   0),   S(  0,   0),
            S( -8, -42),   S(-27,  10),   S( 17, -20),   S( 27, -19),   S( 26, -15),   S( 12, -16),   S(  0,   0),   S(  0,   0),
            S( 17,   4),   S(  2, -13),   S( -8,  36),   S(-20, -20),   S(  2, -29),   S(-18, -23),   S(  2, -19),   S(-14,  -8),
            S( 10,  27),   S(-12, -13),   S( 19,  35),   S( -6, -15),   S( 14,  24),   S(-15, -52),   S(  7, -40),   S( -8, -45),

            /* kings: bucket 8 */
            S( 14, 125),   S(-29,  64),   S( 39,  26),   S(  8,   2),   S(-41,  13),   S(-24,   8),   S( 39, -24),   S(-17, -13),
            S( 29,  98),   S( 27,  14),   S( 43,  66),   S( 82,   6),   S( 31,  18),   S(-14,   5),   S( -7,   8),   S(-14,  26),
            S(  0,   0),   S(  0,   0),   S( 27,  59),   S( 40,   3),   S( 27,   8),   S(  2, -10),   S( -1,   8),   S( 22, -17),
            S(  0,   0),   S(  0,   0),   S(  5,  75),   S( 21,   0),   S(-19,  33),   S(-23,  20),   S( 30,   6),   S( -9,  40),
            S( -1, -31),   S( 12,  43),   S( -3, -12),   S( 10,   1),   S(-13,  -3),   S(  4,  -5),   S(  9,   1),   S( -9, -39),
            S( -6,   6),   S( -4, -18),   S( 10, -10),   S( -5,  17),   S(-13,  11),   S( -4, -22),   S(-26, -10),   S( 15,  17),
            S( -9, -15),   S( -6, -12),   S( 14,  -4),   S( -8, -22),   S( -4, -32),   S( -6,  -1),   S(-10, -21),   S(  4, -33),
            S( -1,  -6),   S( -9, -11),   S( -2, -22),   S( -4, -12),   S(  9,  24),   S( -9,  -9),   S( 14,   9),   S(  7,   0),

            /* kings: bucket 9 */
            S(  9,  24),   S(  1,  34),   S(-14,  69),   S(  7,   8),   S( -7,  24),   S(-12,  16),   S( 74,  -8),   S( 32,   5),
            S(-10,  27),   S( 36,  24),   S(  4,  -2),   S( 75,  -4),   S( 46,  19),   S( 33,  15),   S(-13,  33),   S(-13,  12),
            S(  2,  10),   S( 25,  19),   S(  0,   0),   S(  0,   0),   S( 27,  20),   S( 30,  -2),   S( -2,  12),   S(-17,  23),
            S(  1, -43),   S( -7, -18),   S(  0,   0),   S(  0,   0),   S(  1,  23),   S(  9,   7),   S(-23,  10),   S(-22,  47),
            S(-20, -31),   S( -3,  37),   S( -6,   1),   S(  7,  12),   S(-18,  24),   S(-15,  -3),   S(-22,  11),   S(  6, -16),
            S(  5,   9),   S(  6, -12),   S(  4,   5),   S( -5, -18),   S(-17,  11),   S(-12,  19),   S(-29,   3),   S(  6,  50),
            S(  1, -23),   S( -8, -21),   S( -9,  16),   S( 15, -12),   S( 15, -29),   S( 11,  15),   S(-25, -17),   S(  6, -14),
            S(  1,  -3),   S(  3, -33),   S(  5, -47),   S(-11, -22),   S(-10, -36),   S( -1,  11),   S( -4,  11),   S(  8,   3),

            /* kings: bucket 10 */
            S( 36,   0),   S( -4,   0),   S(  2,  11),   S(-21,  30),   S( -4,  11),   S(-64,  47),   S( -7,  39),   S(-96,  86),
            S( 20, -17),   S( 60,  13),   S( 12,   4),   S( 48,  11),   S( 38,  13),   S( 39,   8),   S(-12,  36),   S(-83,  53),
            S( 29,  12),   S( 34, -11),   S( 20, -12),   S( 24,  17),   S(  0,   0),   S(  0,   0),   S(-12,  32),   S(-75,  27),
            S( 21,  -7),   S( 49, -28),   S( 45, -27),   S( 39,  -6),   S(  0,   0),   S(  0,   0),   S( 14,  18),   S(  7,  -2),
            S( 11,  -8),   S( 29,  23),   S( 43, -33),   S( 11, -36),   S(  3,  19),   S( 10,  13),   S(  8, -18),   S( -6,  24),
            S(-16,   0),   S(  1,   4),   S( -4,  10),   S( 13,  10),   S( -3,  -2),   S(-10, -29),   S(-22, -10),   S( -8,  -3),
            S( -2, -35),   S( -4, -14),   S(  1, -28),   S(  9, -25),   S(  7,  -8),   S(-16,  -4),   S(  3, -31),   S( 14,  20),
            S(  1, -14),   S( 17, -33),   S( -1,   2),   S( -2, -15),   S(  6, -21),   S(  1,   0),   S( -3,   9),   S(  1, -15),

            /* kings: bucket 11 */
            S(-25, -42),   S(  8,  13),   S( -5,  -8),   S(  7,  -2),   S(  5,  -1),   S(-65,  59),   S(-65,  83),   S(-92, 124),
            S( -4, -21),   S( 33,   0),   S(-23, -17),   S( -1,  24),   S( 84,   6),   S( 21,  50),   S( 21,  16),   S( 12,  35),
            S(  3, -38),   S(  8,  12),   S( 20,   0),   S( 29,  10),   S( 62,   4),   S( 37,  60),   S(  0,   0),   S(  0,   0),
            S( -4,  31),   S( 28,   8),   S( 10, -10),   S( -1,  17),   S( 43,  -7),   S( 16,  21),   S(  0,   0),   S(  0,   0),
            S(  0,  32),   S(  2,  -1),   S( -2,  -7),   S(  3, -18),   S( 25,  -5),   S(  2,  10),   S(  9,  28),   S( 13,   7),
            S( 11,  26),   S( -1, -26),   S( 32, -31),   S(  7,   8),   S(  2,  12),   S( -2, -10),   S( -5,  -4),   S(-18, -31),
            S(-10, -10),   S(  4, -18),   S( 13,  21),   S(  0, -11),   S( 10, -25),   S(  6,   1),   S( -2, -20),   S(-10,  -3),
            S(  8,  18),   S( 21,   7),   S(-19, -29),   S( 10,  10),   S( -9, -33),   S(-12, -30),   S(  0, -15),   S(  8,  16),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 15,  33),   S( 11,  31),   S( -3, -15),   S(  2,   2),   S(  3,  -7),   S(-20,  25),
            S(  0,   0),   S(  0,   0),   S( 53, 116),   S( 17,   1),   S( 32,  50),   S( 21,   6),   S( 12, -10),   S(-29,  -2),
            S(  0,   7),   S(  4,  24),   S( 25,  66),   S( 48,  21),   S( 13, -13),   S( 18,   4),   S(  5, -22),   S(-20,   5),
            S(  1,   7),   S( 10,  29),   S(  0,  13),   S(  6,   5),   S(-14,  13),   S(  0,  -4),   S( -1,  21),   S( -2,  -9),
            S(  6,  14),   S(  4,  12),   S( 17,   9),   S(  0,  62),   S(  1,  39),   S( -5,   5),   S(-17,   3),   S( -4,   6),
            S( 15,  18),   S( -1,  -1),   S( -1,  21),   S(-12, -13),   S(  2,  34),   S(-10,  18),   S( -4,   9),   S(  3, -14),
            S(  0,   5),   S( -5, -14),   S( -1,  12),   S( -3,  -4),   S( -4,  -2),   S(  4,  18),   S(  6,  17),   S( -6, -31),
            S( -2,  -3),   S(  7,  11),   S( -7,  -2),   S(  1,   3),   S(  1, -18),   S(  1,   5),   S( -6, -10),   S( -4, -20),

            /* kings: bucket 13 */
            S( -3,  41),   S( -2,   1),   S(  0,   0),   S(  0,   0),   S( 34,  38),   S(  9, -15),   S(  0, -28),   S(-11,  35),
            S(  2,  15),   S( -2,   5),   S(  0,   0),   S(  0,   0),   S( 39,   9),   S( 23, -25),   S(-21,  12),   S(-17,   7),
            S(-13,   8),   S( 22,  44),   S( -5, -24),   S( 12,  20),   S( 58, -22),   S( 24,  -5),   S( 20,  12),   S(  7, -34),
            S(-15, -21),   S( 18,   4),   S( 10,  38),   S( -2,  -2),   S( -4,   6),   S(  2,   6),   S( 11,  24),   S(-23, -39),
            S(  7,  13),   S( -6,  20),   S(  3,  33),   S( -5,  36),   S( -4, -15),   S(  3,  22),   S( -6, -13),   S(  5,   7),
            S(  3,  -8),   S( -6,  35),   S(  0,  14),   S(  3, -18),   S( -4,  29),   S( -2,  30),   S( -6,  28),   S(  2, -12),
            S( 14,  32),   S( -6, -27),   S(-11, -48),   S(  2,  25),   S( -7,   4),   S(  1,  40),   S( -8,  -6),   S( -3,  18),
            S( -1,  -9),   S(  4,  -5),   S(  3,  22),   S(  6,   2),   S(  3,  33),   S( -9, -20),   S( -6, -13),   S(  5,  13),

            /* kings: bucket 14 */
            S( 16,  25),   S( -9,  -8),   S( -1, -33),   S( 16,  21),   S(  0,   0),   S(  0,   0),   S( 14,  68),   S(-39,  17),
            S(-20, -44),   S( 27, -21),   S( 57, -43),   S( 45,  -4),   S(  0,   0),   S(  0,   0),   S( 14,  38),   S(-52,  20),
            S( -1,  -2),   S( 13, -22),   S( 40, -28),   S( 43, -11),   S( 14,  12),   S(  7,  29),   S( 25,  49),   S(-25,   9),
            S( 12, -13),   S( 16,  12),   S( -4,   7),   S(  6,  28),   S( -2,   2),   S( 16,  55),   S(  9,   6),   S(  8,  17),
            S(  7,   2),   S( -1,   5),   S( -1,  42),   S(-14, -13),   S( -4, -15),   S( 14,  68),   S(  3,  54),   S( 15,   9),
            S(-12, -11),   S( 16,  22),   S( -5,   4),   S(  6,   5),   S( -4, -28),   S( -3,   5),   S(-15,  13),   S(  1,  10),
            S(  2,   3),   S(-14, -24),   S(  2, -17),   S( 23,  10),   S(  9,  18),   S(-15,   4),   S(-19,  16),   S( 13,  13),
            S(  2,  14),   S(  8,   4),   S( -2, -24),   S(-11, -26),   S( 12, -11),   S( -6, -10),   S(-10, -39),   S( -2, -21),

            /* kings: bucket 15 */
            S( -3,   0),   S( 14,   3),   S( 11,  -3),   S( -5,   7),   S(  8, -13),   S( -5,  56),   S(  0,   0),   S(  0,   0),
            S(  6, -23),   S(  3, -33),   S( -2, -25),   S( 13,  50),   S( 30, -14),   S( 70, 112),   S(  0,   0),   S(  0,   0),
            S( -8, -15),   S( 20, -17),   S( 17, -16),   S( -9,  20),   S( 14,   8),   S( 24,  79),   S(  5,  28),   S(-13,  26),
            S(  6, -13),   S(  6,  -3),   S(  1,  10),   S( -6, -20),   S(-21,   0),   S( 33,  39),   S( 25,  68),   S( -5, -11),
            S(  9,  12),   S(-17,   8),   S( 16,  28),   S( -5, -21),   S(  7,  30),   S(  1,  29),   S(  0,  12),   S( -1,  -6),
            S(  9,  28),   S(-15,  -2),   S( -1,  -6),   S(  5,  17),   S(-11, -30),   S(-11, -10),   S(  0,  -8),   S(  6,  24),
            S( 10,  27),   S(  0,  30),   S( -1,  -8),   S(  2, -14),   S(  2, -21),   S(  4,  16),   S(  0, -13),   S(  0,  -2),
            S( -1,  -5),   S(  2,  -9),   S(  1,  -6),   S(  8,  16),   S(  6,  26),   S(  7,   9),   S( -4,   3),   S(  4,  12),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-31,  44),   S(  5,  13),   S( 15,  81),   S( 23, 118),   S( 31, 136),   S( 34, 159),   S( 40, 155),   S( 50, 142),
            S( 63, 115),

            /* bishop mobility */
            S( -4,  35),   S(  5,  48),   S( 17,  79),   S( 23, 112),   S( 26, 138),   S( 29, 155),   S( 33, 162),   S( 37, 163),
            S( 41, 163),   S( 51, 153),   S( 61, 141),   S( 89, 128),   S( 83, 132),   S( 76, 123),

            /* rook mobility */
            S(-13,  19),   S( -6, 113),   S( -7, 150),   S( -3, 161),   S( -5, 183),   S(  0, 192),   S(  3, 201),   S(  5, 203),
            S(  5, 211),   S(  9, 214),   S( 17, 215),   S( 26, 213),   S( 28, 215),   S( 48, 209),   S( 67, 195),

            /* queen mobility */
            S( 59, 153),   S( 56, 248),   S( 54, 184),   S( 53, 184),   S( 54, 229),   S( 55, 260),   S( 54, 308),   S( 54, 326),
            S( 57, 336),   S( 58, 351),   S( 56, 368),   S( 56, 380),   S( 57, 385),   S( 55, 395),   S( 58, 396),   S( 58, 399),
            S( 69, 377),   S( 74, 375),   S( 86, 357),   S(121, 324),   S(128, 305),   S(172, 267),   S(154, 262),   S(193, 221),
            S(200, 209),   S(170, 205),   S(106, 149),   S( 94, 120),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  12),   S(-20,  51),   S(-34,  43),   S(-35,  52),   S( 18,  19),   S(-21,  22),   S( -6,  62),   S( 29,  29),
            S( 13,  31),   S( -4,  49),   S(-17,  49),   S(-16,  36),   S( -3,  35),   S(-39,  49),   S(-36,  67),   S( 31,  29),
            S( 22,  69),   S( 17,  69),   S(  9,  55),   S( 27,  40),   S(  5,  50),   S(-25,  64),   S(-37, 103),   S(  0,  76),
            S( 30, 110),   S( 48, 114),   S( 27,  83),   S( 12,  55),   S( 10,  59),   S( -9, 101),   S(-39, 127),   S(-73, 153),
            S( 29, 152),   S( 45, 184),   S( 59, 136),   S( 41, 118),   S(-52, 103),   S( 34, 113),   S(-45, 180),   S(-83, 178),
            S( 98, 236),   S( 91, 274),   S(126, 243),   S(126, 254),   S(127, 266),   S(169, 242),   S(149, 260),   S(121, 265),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,   5),   S( -4, -29),   S( -2, -14),   S( -1, -10),   S( 13,  10),   S( -9, -43),   S(-26,   9),   S(  1, -50),
            S(-17,   2),   S( 23, -12),   S( -4,  30),   S( 11,  21),   S( 37, -13),   S(-10,  22),   S( 21, -16),   S( -4,  -9),
            S(-18,  16),   S( 19,   2),   S( -3,  44),   S( 15,  60),   S( 23,  29),   S( 28,  18),   S( 30,  -1),   S( -6,  19),
            S( 15,  28),   S( 14,  50),   S( 36,  97),   S(  3, 105),   S( 73,  68),   S( 64,  59),   S( 18,  72),   S( 22,   8),
            S( 46,  97),   S(116, 121),   S(117, 127),   S(125, 158),   S(153, 144),   S(133, 141),   S(124, 122),   S( 69,  70),
            S( 63, 182),   S(103, 276),   S(102, 242),   S(108, 211),   S( 67, 148),   S( 48, 142),   S( 50, 156),   S( 19,  85),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  21),   S( 20,  21),   S( 35,  34),   S( 31,  21),   S( 21,  19),   S( 25,  23),   S(  3,  13),   S( 40,  -6),
            S( -6,  21),   S( 15,  34),   S( 10,  34),   S(  8,  44),   S( 25,  17),   S(  8,  22),   S( 27,  20),   S( -4,  16),
            S( -1,  19),   S( 29,  54),   S( 50,  57),   S( 38,  56),   S( 43,  55),   S( 64,  29),   S( 27,  36),   S( 19,   5),
            S( 59,  79),   S(116,  55),   S(127, 120),   S(149, 140),   S(132, 136),   S( 82, 135),   S( 65,  63),   S( 84,  -2),
            S( 33, 125),   S( 78, 158),   S(161, 231),   S(102, 266),   S(133, 270),   S( 58, 233),   S(148, 233),   S(-66, 170),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 27,  29),   S( 16,  21),   S( 10,  36),   S(-14,  66),   S( 64,  32),   S( 13,   7),   S( -2,  -2),   S( 31,  11),
            S(  0,  16),   S(  6,  12),   S( 17,  21),   S( 16,  36),   S( 14,  21),   S( -2,   9),   S(  6,   8),   S( 28,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0, -16),   S( -6, -12),   S(-17, -21),   S(-16, -36),   S(-14, -21),   S(  2,  -9),   S( -6,  -8),   S(-28,   4),
            S(-27, -29),   S(-16, -21),   S(-10, -36),   S( 14, -66),   S(-64, -32),   S(-13,  -7),   S(  2,   2),   S(-31, -11),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -41),   S(-12, -42),   S(-15, -49),   S(-64, -30),   S(-23, -46),   S(-28, -51),   S( -6, -49),   S(-23, -64),
            S(-26, -27),   S(-20, -29),   S(-34, -16),   S( -4, -43),   S(-36, -39),   S(-28, -28),   S(-43, -18),   S(-16, -44),
            S(-16, -25),   S( -6, -37),   S(-27, -14),   S(-31, -22),   S(-21, -43),   S(-20, -23),   S(-12, -22),   S(-41, -30),
            S( -3, -38),   S( 21, -45),   S( 12, -26),   S(  5, -29),   S(  7, -25),   S( 59, -41),   S( 45, -49),   S(-12, -59),
            S( 22, -49),   S( 34, -72),   S( 57, -34),   S( 64, -27),   S( 72, -40),   S( 69, -27),   S(125, -104),  S( 34, -80),
            S(100, -110),  S(123, -112),  S(101, -46),   S( 71, -19),   S( 88, -29),   S(155, -36),   S( 84, -46),   S( 52, -86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-12, -25),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  4,   3),        // attacks to squares 1 from king
            S( 11,   4),        // attacks to squares 2 from king

            /* castling available */
            S( 64, -76),        // king-side castling available
            S( 16,  49),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 40, -90),   S( 48, -84),   S( 41, -90),   S( 34, -77),   S( 30, -67),   S( 19, -58),   S(  7, -50),   S(  2, -41),
            S( 20, -52),   S( 38, -47),   S( 64, -48),   S( 22, -25),   S( 94, -54),

            /* orthogonal lines */
            S(-41, -147),  S(-101, -104), S(-122, -93),  S(-136, -88),  S(-140, -93),  S(-144, -94),  S(-140, -101), S(-137, -101),
            S(-149, -94),  S(-156, -98),  S(-151, -115), S(-96, -145),  S(-88, -145),  S(-11, -174),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 23, 229),

            /* passed pawn can advance */
            S(-19,  40),   S( -5,  62),   S(  0, 106),   S( 80, 170),

            /* blocked passed pawn */
            S(  0,   0),   S( 69, -19),   S( 36,  -9),   S( 31,  39),   S( 28,  68),   S( 30,  35),   S( 83,  67),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 53, -57),   S( 39,  19),   S( 42,  15),   S( 20,  60),   S( 33,  96),   S(123, 139),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-16, -13),   S(-20, -36),   S(  1, -36),   S(-26, -13),   S(-37,  18),   S(100,  15),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 47, -35),   S( 19,  -9),   S(  6,   5),   S( 17, -39),   S( -9, -122),  S(-38, -221),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 24,  53),   S( 66,  19),   S(122,  38),   S( 31,  28),   S(182, 119),   S(111, 126),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 21,  57),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-46, 125),

            /* bad bishop pawn */
            S( -8, -15),

            /* rook on open file */
            S( 39,   5),

            /* rook on half-open file */
            S( 11,  40),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 25,  39),   S( 24,   5),   S( 37,  22),   S( 27,   1),   S( 30, -19),

            /* pawn threats */
            S(  0,   0),   S( 65, 111),   S( 51, 132),   S( 71,  97),   S( 61,  28),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  58),   S( 50,  51),   S( 77,  42),   S( 48,  72),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 22,  46),   S( 33,  37),   S(-16,  42),   S( 72,  58),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 19,  12),   S( 20,  35),   S( 38,  10),   S(  8,  31),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 16,  17),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
