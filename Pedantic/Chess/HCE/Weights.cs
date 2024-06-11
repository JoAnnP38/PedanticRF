// <copyright file="Weights.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess.HCE
{
    using System.Runtime.CompilerServices;
    using System.Text;
    using Pedantic.Utilities;

    // Coefficients or weights for the HCE evaluation function
    public sealed class Weights : IInitialize
    {
        #region Feature Identifiers/Constants

        public static readonly Guid HCE_WEIGHTS_VERSION = new("a7a8fc5d-b4b9-4ffd-81d7-c5eac4c2b91e");

        // 6 (piece weights) + (6x64x16x2) 6 Piece Types X 64 Squares X 16 King Buckets X 2 Both Kings
        public const int MAX_WEIGHTS = 12816;
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
        public const int DOUBLED_PAWN = 12681;      // doubled pawn
        public const int KING_ATTACK_1 = 12682;     // attacks 1 square from king
        public const int KING_ATTACK_2 = 12691;     // attacks 2 square from king
        public const int CAN_CASTLE_KS = 12700;     // can castle king-side
        public const int CAN_CASTLE_QS = 12701;     // can castle queen-side
        public const int KS_DIAG_MOBILITY = 12702;  // open diagonal line attacks against king
        public const int KS_ORTH_MOBILITY = 12715;  // open orthogonal line attacks against king
        public const int PAWNLESS_FLANK = 12729;    // king is on pawnless flank
        public const int KING_OUTSIDE_PP_SQUARE = 12730;    // king cannot stop promotion
        public const int PP_CAN_ADVANCE = 12731;    // passed pawn can safely advance
        public const int BLOCKED_PASSED_PAWN = 12735;       // blocked passed pawn
        public const int ROOK_BEHIND_PASSER = 12775;// rook behine passed pawn
        public const int BISHOP_PAIR = 12776;       // bonus for having both bishops
        public const int BAD_BISHOP_PAWN = 12777;   // bad bishop pawn
        public const int ROOK_ON_OPEN_FILE = 12778; // rook on open file
        public const int ROOK_ON_HALF_OPEN_FILE = 12779;    // rook on half-open file
        public const int PAWN_SHIELDS_MINOR = 12780;// pawn shields minor piece
        public const int BISHOP_LONG_DIAG = 12781;  // bishop on long diagonal
        public const int MINOR_OUTPOST = 12782;     // minor piece outpost (knight or bishop)
        public const int ROOK_ON_BLOCKED_FILE = 12784;  // rook on file with blocked friendly pawn
        public const int PAWN_PUSH_THREAT = 12785;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12791;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12797;      // minor piece threat
        public const int ROOK_THREAT = 12803;       // rook threat
        public const int CHECK_THREAT = 12809;      // check threat against enemy king
        public const int TEMPO = 12815;             // tempo bonus for side moving

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

        public Score DoubledPawn
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[DOUBLED_PAWN];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score KingAttack1(int count)
        {
            count = Math.Clamp(count, 0, 8);
            return weights[KING_ATTACK_1 + count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score KingAttack2(int count)
        {
            count = Math.Clamp(count, 0, 8);
            return weights[KING_ATTACK_2 + count];
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

        public Score PawnlessFlank
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[PAWNLESS_FLANK];
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

        public Score RookOnBlockedFile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[ROOK_ON_BLOCKED_FILE];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score MinorOutpost(Piece piece)
        {
            Util.Assert(piece >= Piece.Knight && piece <= Piece.Bishop);
            return weights[MINOR_OUTPOST + (piece - Piece.Knight)];
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

        // Solution sample size: 16000000, generated on Mon, 10 Jun 2024 01:28:33 GMT
        // Solution K: 0.003850, error: 0.081916, accuracy: 0.5153
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 80, 232),   S(389, 668),   S(414, 662),   S(556, 1088),  S(1402, 1803), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(105, -126),  S(155, -85),   S( 45, -41),   S(-27,  32),   S(-24,  15),   S(-23,   5),   S(-47,  12),   S(-31, -18),
            S(125, -130),  S(111, -103),  S( 11, -61),   S(-12, -51),   S(-22, -14),   S(-20, -25),   S(-33, -18),   S(-24, -44),
            S(111, -107),  S( 67, -59),   S( 13, -63),   S( 12, -73),   S(-11, -63),   S(  4, -57),   S( -6, -48),   S(  3, -58),
            S( 66, -41),   S( 53, -51),   S( 27, -60),   S( 18, -85),   S(-17, -44),   S( -9, -56),   S(-12, -39),   S( -6, -29),
            S( 75,  37),   S( 37,  -5),   S( 39, -25),   S( 52, -71),   S( 19, -43),   S(-11, -39),   S(-21,  -4),   S(-32,  51),
            S( 63,  57),   S( 52,  80),   S(  5,  11),   S( 19, -16),   S(-40,   0),   S(  6,   4),   S( -5,  24),   S( 13,  48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 33, -32),   S( 39, -31),   S( 56, -21),   S(  3,  23),   S( -4,  -6),   S(  6,  -8),   S(-38,  11),   S(-31,  19),
            S( 36, -46),   S( 29, -41),   S( 17, -47),   S( -4, -41),   S(-13, -21),   S( -5, -27),   S(-30,  -9),   S(-36, -13),
            S( 29, -44),   S( 14, -26),   S( 20, -56),   S( 13, -60),   S(-21, -28),   S( 14, -49),   S( -4, -29),   S(  1, -29),
            S( 41, -23),   S( 23, -46),   S( 27, -56),   S(  8, -52),   S(-13, -22),   S( 19, -46),   S(-16, -22),   S( -7,   0),
            S( 28,  45),   S(-29,   5),   S( -2, -34),   S( 11, -47),   S( 35, -35),   S( -9,  -6),   S(-23,  24),   S(-25,  70),
            S( 56,  58),   S( 16,   4),   S(-43, -18),   S(-22,  25),   S(-21,  -7),   S(-57,  27),   S(-47,  33),   S(-40,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  -4),   S(-14,   8),   S( -3,   1),   S(  3,   6),   S( 19, -12),   S( 35, -15),   S( 13, -36),   S( -2, -20),
            S( -1, -30),   S(-22, -11),   S(-17, -34),   S(-15, -34),   S(  9, -34),   S( 14, -32),   S(  1, -34),   S(-17, -31),
            S( -9, -28),   S(-18, -25),   S( -8, -56),   S( -1, -57),   S( -2, -34),   S( 27, -47),   S(  9, -37),   S( 11, -36),
            S(-13, -10),   S( -5, -45),   S(-10, -55),   S( -1, -58),   S( 11, -49),   S( 13, -32),   S( 10, -22),   S(  6, -12),
            S( -2,  35),   S(-37,  -5),   S(-37, -42),   S(-44, -31),   S( 14,  -8),   S( -5,   4),   S(-18,  25),   S(-19,  74),
            S(-46,  78),   S(-87,  59),   S(-92,  -3),   S(-69, -17),   S(-41,   7),   S(-17,  21),   S( -4,  -1),   S(-16,  76),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9, -19),   S(-21,   3),   S(-21,  -3),   S( 16, -47),   S(  0,  -1),   S( 54, -23),   S( 97, -64),   S( 72, -89),
            S( -7, -46),   S(-22, -27),   S(-18, -44),   S(-16, -27),   S( -6, -30),   S( 22, -41),   S( 66, -70),   S( 66, -82),
            S( -7, -52),   S( -4, -56),   S( -3, -70),   S(  0, -72),   S(  1, -61),   S( 26, -58),   S( 47, -65),   S( 81, -81),
            S( -4, -36),   S(  7, -72),   S(  3, -81),   S(  6, -77),   S( 26, -82),   S( 33, -68),   S( 39, -50),   S( 71, -37),
            S( 25,   6),   S( -5, -33),   S( 12, -76),   S( 14, -68),   S( 89, -69),   S( 75, -40),   S( 62,  10),   S( 52,  61),
            S(-29, 101),   S(-19,  13),   S( -3, -50),   S( -6, -65),   S( 64, -76),   S( 64, -20),   S( 66,   7),   S( 70,  52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87,  19),   S( -7,  -7),   S(-31,  14),   S( -8,  25),   S( -7, -19),   S(-41,  26),   S(-41,   6),   S(-45,   3),
            S(-14,   2),   S( 39, -17),   S( 18, -34),   S( 15, -27),   S(-10, -20),   S(-48, -18),   S(  4, -37),   S(  5, -32),
            S( 37, -23),   S( 32, -10),   S(-23,   7),   S( -7, -31),   S(-45, -29),   S(-23, -34),   S(-25, -37),   S( 20, -43),
            S( 12,  23),   S(-10,  37),   S( 32,   1),   S( -5,   0),   S( 12, -38),   S(-41, -24),   S(  9, -39),   S( 52, -35),
            S(-22,  89),   S(-19,  87),   S(-19,  27),   S(-18,   3),   S(  2,  17),   S(-20,   3),   S(-29, -31),   S( 38,  18),
            S( 65,  76),   S( 53, 102),   S(  8,  38),   S( 17,  22),   S( 13, -16),   S(  1, -11),   S(  7,   1),   S(-15,  13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-97,  54),   S(-72,  50),   S( -6,  12),   S( -7,  17),   S(-12,  31),   S(-27,  21),   S(-41,  21),   S(-27,  26),
            S(-57,  16),   S(-61,  24),   S( 19, -13),   S( 15,   4),   S(  9,  -8),   S(-21, -15),   S(-27,  -3),   S(-28,   7),
            S(-55,  33),   S(-62,  34),   S( 35, -27),   S( -9, -23),   S( 16, -16),   S(-23, -21),   S(-14,  -5),   S( 11, -13),
            S(-57,  53),   S(-51,  37),   S( -1,   0),   S( 22,   4),   S(-18,   4),   S(-47,  -3),   S(  2,  -7),   S( 10,  11),
            S( 32,  58),   S( 32,  38),   S( 28,  40),   S( 28,  20),   S(-10,  32),   S( 65,  -9),   S( 13,  12),   S( 50,  25),
            S( 62,  43),   S( 58,  18),   S( 40,  -6),   S( 38,  -4),   S( 44, -14),   S( 22,  -6),   S( 11,   9),   S(  6,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49,  28),   S(-32,  24),   S(-31,  18),   S(-25,  17),   S( 30, -19),   S(-29,  10),   S(-61,  14),   S(-59,  17),
            S(-34,  -2),   S(-10, -15),   S(-22, -30),   S( -7,  -7),   S( 29, -17),   S( 12, -22),   S(-40,  -1),   S(-72,   6),
            S(-20,  -9),   S(-19,  -5),   S(-22, -23),   S(-41,  -6),   S( -6,  -9),   S( 48, -40),   S(-14, -11),   S(-21,   1),
            S(-32,  15),   S(-72,  13),   S(  7, -32),   S(-16,  -9),   S( 12,  -3),   S( 41, -18),   S( 21,  -6),   S( 40,   0),
            S( 11,  22),   S(-43,  14),   S( 15, -30),   S( -4, -12),   S( 42,  23),   S( 71,  18),   S( 37,  11),   S( 68,  26),
            S( 62,  26),   S( 25,   0),   S(  6, -36),   S( 11, -38),   S( 21,  -1),   S( 25,   3),   S( 43, -10),   S( 44,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -24),   S(-41, -10),   S(-24,  -4),   S(-48,  13),   S(-11, -19),   S( 26, -20),   S( -4, -42),   S(-40, -30),
            S(-36, -44),   S(-33, -39),   S(-41, -38),   S(-23, -43),   S(-11, -36),   S( 44, -53),   S( 48, -55),   S( -9, -39),
            S(-40, -47),   S(-53, -35),   S(-44, -48),   S(-23, -45),   S(-22, -26),   S( 25, -37),   S( 34, -52),   S( 47, -52),
            S(-19, -46),   S(-46, -49),   S(-78, -46),   S(-51, -25),   S(-12, -27),   S( 21, -20),   S( 27, -15),   S( 76, -33),
            S(  9, -35),   S( 10, -58),   S(-20, -53),   S(  0, -65),   S( 18,  -4),   S( 29,   1),   S( 65,  44),   S(100,  30),
            S( -9,   1),   S(-25, -33),   S(  4, -51),   S( -6, -51),   S( -2, -16),   S( 27, -21),   S( 47,  41),   S( 86,  57),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  71),   S(-34,  66),   S( 17,  27),   S(-10,  -1),   S( 14,  10),   S(  5,   7),   S(-35,  12),   S(-44,  23),
            S(-61,  60),   S(-58,  61),   S(-33,  42),   S(-15,  13),   S(-17,  -8),   S(-37, -13),   S(-50,  -1),   S( -2,  -8),
            S(-62,  95),   S(-14, 106),   S(-14,  62),   S(-33,  34),   S( 11, -15),   S(-102,  -6),  S(-72, -12),   S(-45,  -8),
            S(-31, 138),   S(  4, 156),   S(  8, 107),   S( 10,  48),   S(-35,  15),   S(-34, -21),   S(-30,   0),   S(-55,   7),
            S(-11, 168),   S( 42, 159),   S( 26, 163),   S( 55, 100),   S( 18,  11),   S(  1,   3),   S(-17, -14),   S( -8,  17),
            S( 52, 191),   S( 69, 210),   S( 86, 200),   S( 49,  73),   S(  6,  36),   S(-12,   6),   S(-10, -25),   S(  0,  11),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94,  76),   S(-60,  57),   S( 13,  12),   S( 12,  31),   S( 13,  11),   S(-35,  16),   S(-69,  26),   S(-76,  30),
            S(-55,  37),   S(-54,  38),   S(-44,  28),   S(  5,  46),   S(-49,   1),   S(-25, -10),   S(-70,   3),   S(-30,   8),
            S(-90,  66),   S(-112, 101),  S(-54,  78),   S(-109,  87),  S(-68,  53),   S(-93,   9),   S(-49, -13),   S(-49,   3),
            S(-71, 107),   S(-35, 121),   S(  0, 120),   S( 41, 124),   S(-32,  58),   S(-44,  14),   S(  8,   7),   S(-52,  21),
            S( 16, 123),   S( 23, 146),   S( 21, 156),   S( 44, 174),   S( 18, 130),   S( -5,  35),   S( -3,   3),   S( -4,   1),
            S( 20,  74),   S( 21, 125),   S( 64, 137),   S( 69, 180),   S( 28, 108),   S( -8,  -8),   S(-14,  -7),   S(-20, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-89,  14),   S(-55,   6),   S( -5,   1),   S(  4,  21),   S( -9,   5),   S(-45,  26),   S(-98,  33),   S(-56,  31),
            S(-98,   6),   S(-80,  12),   S(-10, -17),   S(-23,  -8),   S(-20,  22),   S(-41,  24),   S(-119,  38),  S(-83,  16),
            S(-29, -13),   S(-84,  19),   S(-32,   1),   S(-84,  67),   S(-86,  82),   S(-27,  41),   S(-119,  53),  S(-88,  39),
            S(-103,  32),  S(-79,  34),   S(-11,   9),   S(-43,  78),   S( 14,  95),   S(-58,  80),   S(-31,  54),   S( -2,  25),
            S(-27,  45),   S(-34,  22),   S(  7,  50),   S( 26, 125),   S(102, 110),   S( 51,  65),   S(-14,  92),   S( 31,  44),
            S( -2,  14),   S(-21,  -2),   S( 19,  17),   S( 50, 115),   S( 10, 129),   S( 27,  55),   S( -6,  72),   S( 24,  95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71,  -5),   S(-70,  25),   S( 43, -14),   S( -1,  18),   S(  2,  39),   S(-74,  55),   S(-43,  41),   S(-62,  40),
            S(-68, -23),   S(-81, -13),   S(-34, -37),   S(-51,  17),   S(-36,  13),   S(-33,  29),   S(-94,  67),   S(-98,  44),
            S(-41, -34),   S(-61, -28),   S(-60,  -5),   S(-36,   8),   S(-58,  36),   S(-22,  59),   S(-89,  89),   S(-49,  63),
            S(-58,   6),   S(-89,  -8),   S(-30, -29),   S(-56,  18),   S(  3,  43),   S(-13,  76),   S( 14, 116),   S( 77,  69),
            S(-21,  23),   S(-44,  -5),   S( -8,  -2),   S( -8,  24),   S( 57,  95),   S( -4, 125),   S( 98, 122),   S( 90, 103),
            S(-34,  44),   S(-19,   5),   S(  9, -20),   S(  2,   1),   S( 20,  69),   S( 31, 151),   S( 67, 181),   S( 35, 172),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13,  13),   S(-15,  16),   S(-17,   3),   S(  2,   5),   S( -3, -10),   S(-10,  16),   S(-11, -13),   S(-18,  -7),
            S(-38, -26),   S( -6,  23),   S(  9,  20),   S( -1,   4),   S( -3,  32),   S( -9, -12),   S(-35, -26),   S(-30, -45),
            S(-19,  36),   S(-37,  99),   S( 18,  63),   S( 17,  38),   S(-19,  -1),   S(-49, -18),   S(-47, -44),   S(-49, -63),
            S(-46,  89),   S(-46, 127),   S( 38, 116),   S( 22,  96),   S(-21, -31),   S(-42, -35),   S(-10, -12),   S(-62, -54),
            S( 33,  94),   S( 37, 215),   S( 47, 150),   S( 18,  56),   S( -2,  15),   S( -3, -21),   S( -1,   2),   S(-22, -46),
            S( 44, 108),   S( 53, 218),   S(117, 220),   S( 46,  99),   S( -7,   4),   S(-10,  -8),   S(-11, -31),   S(-23, -40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -19),   S(-17,  19),   S( -4,  11),   S( -2,   4),   S( -8,  -9),   S(-28,   6),   S(-32, -35),   S(-24,  -9),
            S(-41, -11),   S(-55,  50),   S(-24,  34),   S( 20,  23),   S(-45,  24),   S(-15, -14),   S(-81, -18),   S(-62,   7),
            S(-60,  46),   S(-49,  54),   S(-39,  78),   S(-13,  93),   S( -1,  32),   S(-44, -31),   S(-66, -24),   S(-82, -28),
            S(-77,  90),   S( -9, 124),   S( -4, 138),   S(  4, 123),   S( -3,  62),   S(-46,  25),   S(-19,  -8),   S(-43, -41),
            S(  1,  96),   S( 54, 170),   S( 66, 195),   S( 47, 248),   S( 21, 149),   S(-11,  15),   S( -4, -62),   S(-26, -37),
            S( 41,  69),   S( 72, 171),   S( 84, 191),   S( 83, 251),   S( 38, 106),   S(  2,  10),   S( -1,   1),   S( -6,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -57),   S(-32, -17),   S( -8, -25),   S( -2,  -1),   S( -5,  -1),   S(-30,  10),   S(-32,   4),   S( -3,  44),
            S(-54,  11),   S(-56,  12),   S(-54, -28),   S( -2,  10),   S(-39,  63),   S(-17,  18),   S(-41,  23),   S(-58,  11),
            S(-64, -26),   S(-60,   9),   S(-38, -20),   S(-24,  41),   S(-22,  70),   S(-55,  35),   S(-35,   7),   S(-68,  40),
            S(-52,  12),   S(-24,  56),   S(-28,  29),   S(  9,  96),   S( -5, 132),   S(-30,  84),   S(-37,  47),   S(-37,  57),
            S(-21, -23),   S( 11,  18),   S( 13,  78),   S( 35, 134),   S( 46, 214),   S( 43, 170),   S( 11,  83),   S( 25,  40),
            S( -3,  23),   S( 19,  37),   S( 30, 112),   S( 35, 135),   S( 64, 212),   S( 57, 113),   S( 30,  93),   S( 21,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -31),   S(-29, -15),   S(-10, -30),   S(  0,  -3),   S( 17,  22),   S(  2,  48),   S(-10, -19),   S(  8,  19),
            S(-45, -34),   S(-33, -11),   S(-14, -41),   S( 22,  -7),   S(-14,   1),   S(  6,  46),   S(  5,  35),   S( -2,  -3),
            S(-20, -75),   S(-33, -56),   S(-21, -51),   S(  0, -10),   S( 10,  33),   S(-18,  57),   S( -2,  75),   S(-24,  60),
            S(-29, -24),   S(-44, -27),   S(-32,   1),   S(  9,  19),   S(-13,  51),   S(  5,  93),   S(-25, 142),   S( -9,  52),
            S(-27, -42),   S(-31, -31),   S(-14,  15),   S(  0,   1),   S( 35, 115),   S( 66, 165),   S( 59, 224),   S( 74,  67),
            S( -8,   5),   S( -4,  10),   S(  2,   9),   S(  7,  23),   S( 26,  81),   S( 84, 189),   S( 34, 175),   S( 43,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-29,   6),   S(  3,  12),   S(-44,  17),   S(-20,  -5),   S(-27,  -5),   S(  4, -30),   S(-42, -45),   S(-35, -16),
            S(-35,  55),   S( 26, -40),   S(-40,  16),   S( 11, -18),   S(-10, -16),   S(-23, -14),   S(-27, -23),   S(-69, -21),
            S(  3,  65),   S( -2,  -6),   S(  6,  -7),   S(-23,  37),   S( 11,  11),   S(-34,   5),   S(-11, -26),   S(-38, -49),
            S( 17, -24),   S( 41,   6),   S( 12,  19),   S( 27,  23),   S(  3,   2),   S( -3,   1),   S(-12, -16),   S( -3,  -6),
            S( 17, -31),   S( 34,   8),   S( 12,   7),   S( 64, -10),   S( 41,  -9),   S( 26,  19),   S( 20, -17),   S(-60, -15),
            S( 18, -15),   S(  9,   7),   S( 24,  13),   S( 53, -16),   S( 32, -44),   S( 16,  10),   S(  9, -25),   S( -4, -12),
            S( 14, -30),   S( 15, -39),   S( 15, -25),   S( 32, -29),   S( 20, -18),   S(-10, -26),   S(-10, -42),   S(-20, -33),
            S(-69, -56),   S( -7,   0),   S( -5, -17),   S(  0, -44),   S(-21, -22),   S( 20,   9),   S( -6,   4),   S( 15,  -3),

            /* knights: bucket 1 */
            S(-43,  24),   S(-52,  89),   S( 28,  40),   S(-25,  68),   S(-17,  52),   S(-20,  28),   S(-31,  53),   S(-17, -12),
            S( 36,  23),   S( -4,  38),   S( -3,  28),   S( -5,  45),   S( -7,  30),   S(-13,  18),   S( 19, -15),   S(-25,  12),
            S(-32,  29),   S( 14,  12),   S(  2,  14),   S( 15,  30),   S(  5,  34),   S(-28,  32),   S(-18,   8),   S(-26,  21),
            S(  1,  36),   S( 53,  23),   S( 19,  39),   S( 24,  19),   S(  7,  27),   S( -4,  25),   S( 16,   8),   S( 16,  11),
            S(  0,  47),   S( 16,  22),   S( 27,  24),   S( 38,  24),   S( 35,  24),   S( 28,  19),   S( 25,  12),   S( 19,  16),
            S(  8,  17),   S( 16,  10),   S( 15,  30),   S( 43,  14),   S( 11,  22),   S( 32,  31),   S( 24,   5),   S( 15, -10),
            S( 35,   4),   S( 27,  16),   S(-22, -17),   S( 15,  33),   S( 28,  -4),   S( 24,  -3),   S(-32,  12),   S(-11, -21),
            S(-101, -67),  S(-22, -12),   S( -4,  17),   S(  2,  27),   S(-13,   3),   S(-25, -21),   S( -5,  -6),   S(-39, -39),

            /* knights: bucket 2 */
            S(-56,   6),   S(  2,  21),   S(-32,  55),   S(-23,  58),   S(-32,  60),   S(-36,  75),   S(-16,  30),   S(-19,  13),
            S(-14, -18),   S(-17,  12),   S(-12,  20),   S(-11,  37),   S( -5,  25),   S(-14,  56),   S(-33,  57),   S(-38,  65),
            S(-11,  21),   S( -5,  11),   S(-10,  32),   S( 14,  24),   S(  2,  36),   S(  5,  15),   S( -8,  43),   S(-19,  25),
            S( -7,  34),   S(-26,  38),   S(  5,  35),   S(  1,  45),   S( -3,  40),   S( -9,  34),   S(  3,  36),   S( -2,  37),
            S( 16,  22),   S(-16,  31),   S( -6,  43),   S(-18,  51),   S(  0,  45),   S( -9,  40),   S(  2,  30),   S(  0,  19),
            S(-25,  32),   S(  0,  30),   S(-27,  51),   S(-20,  49),   S(-27,  45),   S( -4,  27),   S(-31,   9),   S( 14,   1),
            S(-21,  23),   S(-31,  17),   S(-34,  20),   S(-38,  38),   S(-15,  18),   S(  1,  24),   S(-51,  36),   S(-34,  11),
            S(-147,  19),  S( -4,  -1),   S(-80,  32),   S(-28,  13),   S( -4,  11),   S(-59,   2),   S( -3,   0),   S(-179, -56),

            /* knights: bucket 3 */
            S(-48, -12),   S( 17, -30),   S(-20,  -5),   S( 10,  -3),   S( 13,  -8),   S(-11,  10),   S( 25, -17),   S( -3, -25),
            S(-11,  -2),   S(-22,  -9),   S(-15,  -6),   S( 10,  11),   S( 22,  -2),   S(  1,  -8),   S( -1,  -9),   S(-16,  55),
            S(  5, -34),   S(  5,  -2),   S(  4,   2),   S( 17,  10),   S( 21,  24),   S( 30,   1),   S( 16,   2),   S( 12,  32),
            S(  2,  -4),   S( 12,   8),   S( 17,  29),   S( 21,  26),   S( 31,  30),   S( 27,  25),   S( 30,  15),   S( 25,  12),
            S( 28,   2),   S(  7,  13),   S( 36,   6),   S( 30,  37),   S( 28,  36),   S( 36,  40),   S( 42,  32),   S( 18,  11),
            S(  4,   7),   S( 31, -15),   S( 46,  -2),   S( 59,   3),   S( 70, -17),   S( 73,  -9),   S( 14,   6),   S( 12,  38),
            S( 28,  -6),   S( 15,   6),   S( 44, -20),   S( 51,  -8),   S( 66, -31),   S( 60, -35),   S( 63, -66),   S( 47, -23),
            S(-105,   9),  S(-24,   6),   S(-28,   3),   S(  2,  16),   S( 32,  -7),   S( -8, -12),   S(-11, -23),   S(-74, -47),

            /* knights: bucket 4 */
            S( 13,  17),   S(-46,   6),   S( 14,  27),   S( -1,  -5),   S(-20, -11),   S(-28, -23),   S( -8, -52),   S(-31, -46),
            S( 32,  24),   S(-21,  37),   S( 17, -22),   S( 16,  -5),   S( 21, -14),   S( -4, -40),   S( 11,  -3),   S(  2, -47),
            S(-10,  29),   S( 10,  38),   S( 11,  10),   S( 21,  16),   S( -4,   3),   S(-43,  18),   S(-47, -28),   S(-31, -55),
            S( -2,  64),   S( 31, -24),   S( 44,  25),   S( 24,  17),   S( 12,  13),   S( 93, -17),   S( 27, -31),   S(  0, -19),
            S( 56,  28),   S(-20,  43),   S( 40,  46),   S( 41,  17),   S( 40,  34),   S(-15,  26),   S( -5, -29),   S(-10,  -9),
            S(  7,  16),   S(-33,  -4),   S( 77,  15),   S(  6,   7),   S( 11,  19),   S( 20,  19),   S( 13,  29),   S(-11, -22),
            S( -7,   7),   S(-16,   8),   S( 14,  -1),   S(  4,  37),   S(  9,  11),   S(  6, -15),   S(  4,  -6),   S(-14,  -2),
            S(-11,  -8),   S( -2,  -6),   S( 10,  11),   S(  2,   5),   S( -5,  -8),   S(  9,  21),   S( -2,   5),   S( -3, -19),

            /* knights: bucket 5 */
            S( 12,  -2),   S(-36,  46),   S( 30,  36),   S( 23,  48),   S( 38,  25),   S( 14,   2),   S(  2,  18),   S(-21, -21),
            S( 12,  -1),   S( 32,  46),   S( 21,  25),   S( -7,  44),   S( 36,  38),   S(  5,  37),   S( 23,  27),   S(-14, -26),
            S(  5,  25),   S( -9,  41),   S( 62,  23),   S( 38,  47),   S(-16,  53),   S( -2,  30),   S(-17,  19),   S(  8,  -3),
            S( 29,  46),   S(  9,  49),   S( 31,  44),   S( -6,  62),   S( 11,  51),   S(  4,  44),   S( 24,  41),   S( 11,  34),
            S( 21,  50),   S( 25,  36),   S( 41,  52),   S( 52,  46),   S( 74,  48),   S( 24,  43),   S( 40,  34),   S( 35,  30),
            S(  4,  30),   S( -7,  50),   S( 17,  30),   S(  9,  55),   S( 33,  45),   S( 13,  52),   S( 17,  15),   S( -6,  31),
            S( 19,  55),   S( -5,  66),   S( 31,  48),   S( 14,  66),   S(  7,  55),   S(  9,  48),   S( 23,  68),   S(  4,   1),
            S( -4,   7),   S(  0,  13),   S(  9,  41),   S( -3,   6),   S( 10,  42),   S(  2,  35),   S(  8,  38),   S(-17, -17),

            /* knights: bucket 6 */
            S(  2, -41),   S(-18,  -4),   S( 32,  29),   S(-23,  41),   S(-23,  49),   S( 20,  40),   S( -8,  33),   S(-12,  25),
            S( -4, -30),   S( 53,   1),   S( 18,  13),   S(-30,  41),   S(-58,  71),   S( 29,  51),   S( 21,  49),   S( -1,   8),
            S(-22, -18),   S( -1,   3),   S( -7,  28),   S( 18,  37),   S(-22,  65),   S(-37,  61),   S( 13,  47),   S(  2,  43),
            S( 32,   5),   S( 32,  12),   S( 40,  31),   S( 70,  30),   S( 16,  53),   S(  7,  55),   S(  9,  62),   S(-24,  69),
            S(  4,  35),   S( 62,  -7),   S( 51,  36),   S( 62,  35),   S( 74,  39),   S( 73,  39),   S( 16,  57),   S( 19,  51),
            S( 24,  26),   S( 10,  15),   S( 64,  22),   S( 43,  44),   S( 51,  48),   S( 28,  30),   S( 14,  40),   S( 36,  37),
            S(-22,  21),   S(  3,  35),   S(-24,  37),   S( 30,  33),   S(  3,  60),   S( 22,  42),   S( 21,  71),   S( -7,  28),
            S(-41,  -1),   S( 16,  40),   S( 28,  38),   S( 11,  40),   S( 22,  35),   S( 11,  59),   S( 20,  57),   S( 11,  24),

            /* knights: bucket 7 */
            S(-33, -57),   S(-188, -44),  S(-69, -44),   S(-56, -15),   S(-39,  -8),   S(-31, -16),   S( -8,   2),   S(-17,   5),
            S(-49, -76),   S(-37, -46),   S(-35, -30),   S(-47,   5),   S(-44,  12),   S(  6, -11),   S(-14,  47),   S(  5,  28),
            S(-79, -65),   S(-55, -35),   S(-49,   1),   S( 19, -15),   S(-18,  10),   S(  2,  13),   S(-14,  58),   S( 45,  54),
            S(-60, -24),   S(  8, -24),   S(-13,  11),   S( 27,  -2),   S( 39,  -1),   S(  7,  16),   S(  8,  14),   S(-23,  33),
            S(-60, -24),   S(-22, -29),   S( 47, -22),   S( 73, -15),   S( 99,  -6),   S( 57,  22),   S( 84,   0),   S( 72,  18),
            S( -7, -40),   S( 17, -40),   S(-24,   0),   S( 24,   2),   S( 58,  11),   S( 68,   6),   S( 48, -20),   S( -6,   7),
            S(-33, -33),   S(-64, -20),   S(  5, -13),   S( 32,  17),   S( 34,  22),   S( 39,   2),   S(-20,  23),   S(  0,   4),
            S(-37, -29),   S( -9, -10),   S(-27, -14),   S(  8,  13),   S( 10,   4),   S( 22,  18),   S( -3, -10),   S( -4,  -7),

            /* knights: bucket 8 */
            S( -1,  -8),   S( -8,  -8),   S( -3,  -4),   S( -8, -30),   S(-10, -39),   S(-10, -51),   S( -2,  -1),   S( -5, -23),
            S(  2,   1),   S( -6, -11),   S( -7, -29),   S(-18, -43),   S(-28, -27),   S(-17, -70),   S(-13, -58),   S(-16, -36),
            S(  4,  17),   S(-20, -18),   S( 25,   9),   S(  4,   0),   S(  4, -30),   S(-15, -10),   S(-12, -36),   S( -8, -41),
            S(-18,  -1),   S( -1,  -2),   S( -3,  15),   S(  4,  33),   S(  8,  -2),   S(  6,   7),   S(-14, -52),   S( -3, -17),
            S( 26,  52),   S( 11,   8),   S( 13,  35),   S( 33,  18),   S( 10,  29),   S( -4,  -6),   S(  4, -21),   S( -7,  -9),
            S( 13,  36),   S(  8,   5),   S( 29,  23),   S( 31,  14),   S(  2,  -1),   S( -1,  -7),   S( -7, -28),   S( -6,  -9),
            S(  3,  12),   S(  1,   3),   S(  6,  10),   S( 11,  11),   S(  7,   8),   S(  5,  21),   S(  2,  12),   S( -1,   4),
            S(  1,   0),   S( 11,  32),   S(  5,  15),   S( -2,   0),   S(  2,  11),   S( -5, -20),   S(  3,   5),   S( -3,  -4),

            /* knights: bucket 9 */
            S(-10, -30),   S(-20, -36),   S(-18, -47),   S( -3, -14),   S(-22, -53),   S(-14, -38),   S( -3, -13),   S( -4, -27),
            S(-11, -38),   S(-12,  -1),   S(-10, -50),   S(-11,  -7),   S( -3, -13),   S( -7, -33),   S( -5,  -2),   S(-15, -42),
            S(  5,   6),   S( -9, -14),   S(  6, -14),   S(  4,   5),   S(  4,  20),   S(-31,   1),   S(-12,  -9),   S( -8, -18),
            S(-14,  -3),   S( -6,  -8),   S(  5,  31),   S( 14,  35),   S( 28,  26),   S(  9,  23),   S(-12, -35),   S( -3,  -1),
            S(  0,  21),   S( 20,   8),   S( 18,  41),   S( -2,  45),   S(  7,  17),   S( 13,  -7),   S(  2, -29),   S(  5,   8),
            S(  0,   0),   S(  7,  32),   S( 14,  35),   S(-10,  22),   S( 33,  39),   S( 14,   9),   S(  6,  11),   S( -7, -24),
            S(  1,   0),   S( -1,  21),   S( 18,  38),   S( 11,   5),   S( 13,  42),   S( -2, -15),   S(  4,  17),   S( -2,  -1),
            S(  1,   0),   S(  3,   8),   S( 12,  27),   S( 15,  30),   S(  9,  10),   S(  0,   4),   S(  3,   4),   S(  0,  -4),

            /* knights: bucket 10 */
            S(-18, -50),   S(-16, -53),   S(-12, -26),   S(-17, -21),   S(-12, -11),   S(-13, -45),   S( -3,  15),   S(  4,  20),
            S( -6, -24),   S( -7, -14),   S(  1, -17),   S(-18, -34),   S(-23, -36),   S( -8, -40),   S( -8,  -8),   S( -5, -13),
            S(-16, -50),   S(-17, -61),   S( -7, -10),   S(-13, -13),   S( 15,   4),   S(-11,  -1),   S( -6,   3),   S( -7,   5),
            S( -8, -19),   S( -6, -44),   S(  3, -33),   S( 16,  16),   S(  7,  41),   S( 15,  25),   S(  6,  17),   S( 11,  43),
            S( -6, -46),   S(-13, -31),   S( 15,  12),   S( 21,  34),   S( 16,  54),   S( -1,  28),   S( 18,  12),   S( 23,  51),
            S(-11, -40),   S( -4, -21),   S( -4,  -9),   S( 12,  45),   S( 33,  65),   S( 30,  43),   S( 27,  58),   S( 16,  52),
            S(  0,  -3),   S(-10, -32),   S(  2,  -7),   S( 27,  27),   S( 18,  30),   S(  9,  33),   S(  0,  -3),   S(  9,  24),
            S( -3, -17),   S(  3,  11),   S( -7, -18),   S(  4,  -4),   S( 12,  39),   S(  5,  25),   S(  2,  12),   S( -1,  -3),

            /* knights: bucket 11 */
            S( -1,  -1),   S(-18, -28),   S( -7, -43),   S( -9, -25),   S(-20, -50),   S(-12, -17),   S( -6,  -5),   S( -4,  -6),
            S( -7,  -8),   S(-12, -20),   S(-14, -76),   S(-27, -23),   S( -7,   0),   S(-29, -37),   S(-16, -30),   S( -8, -10),
            S(-14, -53),   S(-22, -61),   S(-23, -32),   S(  1,   5),   S(-14,   7),   S(-16,  20),   S(  9,  -5),   S(  0,  15),
            S(-13, -30),   S( -7, -31),   S(-26,  -5),   S( 26,  31),   S( 16,  20),   S( 14,   9),   S( 12,  25),   S( 16,  29),
            S( -3, -24),   S(-19, -58),   S(  6, -19),   S(  0,   8),   S( 13,  21),   S( 29,  54),   S(  4,  -3),   S( 25,  65),
            S( -7, -11),   S( -6, -25),   S(  1,  -5),   S( 38,  33),   S( 17,  22),   S( 49,  49),   S( 22,  23),   S( 13,  26),
            S(  9,  27),   S( -2,  -6),   S(  8, -10),   S( 13, -15),   S( 21,  32),   S(  1,   6),   S( 16,  38),   S( 20,  54),
            S( -3,   1),   S( -2, -19),   S(  8,  12),   S(  1,   4),   S(  2,  12),   S(  2,   4),   S(  3,   5),   S(  2,  11),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   3),   S( -2, -20),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   4),   S( -2, -13),
            S(  0,   0),   S(  1,   2),   S(  2,   6),   S( -3, -11),   S( -1,   5),   S( -4, -20),   S( -2, -11),   S(  1,   9),
            S( -5, -12),   S(  5,   4),   S( -6, -12),   S( -6, -21),   S(  0,   3),   S( -5, -17),   S(  2,  -4),   S( -7, -30),
            S( -7, -12),   S( -1,   1),   S( -8, -22),   S(  5,  14),   S( -4,  -3),   S(  0,   6),   S( -2,  -7),   S( -1, -10),
            S(  9,  16),   S(  5,   3),   S( -6, -12),   S(  0,   3),   S( -5, -26),   S(  0,   3),   S( -1, -13),   S(  0,   1),
            S(  1,  -9),   S( -5, -23),   S(  1,   0),   S( -1,  -6),   S(  4,  10),   S( -5, -17),   S( -1,  -7),   S(  0,   3),
            S(  2,   6),   S( -8, -11),   S( -1,  10),   S(  2,  -9),   S( -5,  -7),   S( -5, -21),   S( -2,  -1),   S(  0,  -1),
            S(  2,   3),   S(  2,  13),   S( -2,  -3),   S(  2,  -2),   S( -2,  -5),   S( -2,  -9),   S( -3,  -9),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -2,  -6),   S( -2,  -1),   S( -8, -13),   S( -1,   1),   S( -3, -12),   S(  1,  -2),
            S( -2,  -7),   S(  1,   5),   S( -2, -23),   S(-10, -21),   S( -6, -30),   S( -4, -25),   S(  0,   1),   S(  1,  -1),
            S( -4, -10),   S( -8, -31),   S(  7,  18),   S(  0,  -1),   S(-12, -38),   S( -9, -23),   S( -2, -14),   S( -6, -28),
            S( -8, -14),   S(  5,  12),   S(  1,   2),   S(-11, -25),   S( -2,  -7),   S(  5,  12),   S(  0, -13),   S( -5, -11),
            S(  3,  11),   S( -2,  -3),   S(  2,  -8),   S( 11,  22),   S(  4, -11),   S( -4,  -8),   S(  2, -13),   S(  1,   1),
            S( -3,  -9),   S( 14,  14),   S(  7,  23),   S(-13,  11),   S(  5,   5),   S(-10, -33),   S(  4,   5),   S( -4,   2),
            S(  1,   7),   S(  2,   5),   S(  9,  12),   S(  8,  12),   S( 14,  23),   S( -4, -21),   S( -2,  -2),   S( -5,  -3),
            S( -1,   1),   S( -1,  -6),   S( -1,   1),   S(  1,  -8),   S( -1,  -1),   S(  3,  -1),   S(  0,  -1),   S( -1,   0),

            /* knights: bucket 14 */
            S( -3, -24),   S( -5, -25),   S( -1,  -2),   S( -3,   3),   S( -8, -24),   S( -2, -14),   S( -1,  -5),   S(  0,   1),
            S(  0,  -2),   S( -3, -10),   S(-15, -60),   S( -8, -36),   S( -1,  -9),   S(  1,   6),   S(  1,  -4),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-10, -53),   S(  2,   2),   S( -4, -19),   S( -4,  -9),   S(  0,  -1),   S(  1,   8),
            S(  1,   5),   S( -6, -32),   S(-15, -39),   S(-12, -36),   S( -2, -21),   S(  2,   0),   S( -3, -16),   S( -7, -11),
            S( -2,  -4),   S( -2, -17),   S(  1,  23),   S( -7, -30),   S( -9,  -7),   S(  1,  22),   S(  2,   4),   S( -3,  -6),
            S( -4,  -8),   S(  3,  -3),   S( -9, -30),   S(  4,   2),   S( 14,  26),   S(  3,   8),   S( -3,  -1),   S(  0,  -4),
            S(  0,  -3),   S( -2, -10),   S(  7,  -3),   S(  0,  -7),   S( -7,  -9),   S( -2,  -8),   S( -4,  -2),   S(  1,   7),
            S(  0,  -2),   S(  2,   4),   S( -1, -10),   S(  7,  -1),   S(  5,  19),   S(  1,   3),   S( -2,  -8),   S( -1,  -5),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -1, -14),   S( -1, -14),   S( -7, -13),   S( -3,  -1),   S( -2,  -5),   S(  1,   0),   S(  0,  14),
            S( -2,  -6),   S(  0,  -2),   S( -4, -18),   S( -6, -26),   S( -2,  -3),   S( -1,  -9),   S(  0,   0),   S( -1,  -3),
            S( -6, -16),   S( -7, -16),   S( -3, -11),   S(-13, -39),   S( -6, -24),   S( -1,  -3),   S( -1,  -1),   S( -2,   0),
            S( -6, -17),   S( -6, -32),   S( -7, -20),   S(  0,  -8),   S(  0, -18),   S(  6,  23),   S(  5,  10),   S( -3,  -1),
            S(  0,  -2),   S( -2,  -5),   S( -2, -16),   S( -8, -12),   S(  3,  18),   S(  3,  10),   S( -6,  -7),   S( -2,   2),
            S( -3,  -4),   S( -2,  -5),   S( -2, -21),   S( -3,   7),   S( -5, -13),   S( -6,  13),   S( -3,   3),   S(  2,   7),
            S( -3, -13),   S( -1,  -6),   S( -1,  -8),   S( -4,  -8),   S(-10, -13),   S( -4,  16),   S( -2,  -8),   S(  3,  13),
            S(  0,  -3),   S(  0,  -1),   S( -3,  -9),   S( -2,  -9),   S( -2,  -4),   S( -9,  -6),   S(  7,  18),   S( -2,   1),

            /* bishops: bucket 0 */
            S( 16,  18),   S( 24, -10),   S( 43,  20),   S(  8,  24),   S( -2,  -1),   S( 18,  -2),   S( 27, -38),   S(  2, -34),
            S( 48, -43),   S( 80,   4),   S( 37,  12),   S( 18,   5),   S(-13,  37),   S(  2, -18),   S(-36,   2),   S( 11, -48),
            S( 26,  41),   S( 50,  11),   S( 29,   6),   S( 13,  56),   S( 20,  14),   S(-31,  28),   S( 10, -22),   S( 12, -39),
            S( 16,  11),   S( 64, -12),   S( 37,   9),   S( 37,  30),   S(  2,  30),   S( 28,   0),   S(-10, -10),   S(  1,  -1),
            S( 16,   4),   S( 28,  22),   S(  5,  36),   S( 54,  12),   S( 58,   0),   S( 16,  -6),   S( 20, -20),   S(-36,  -2),
            S(-37,  63),   S(  1,  17),   S( 54, -20),   S( 84, -22),   S( 32,  32),   S(-10,   0),   S( -1,  11),   S(  0,  10),
            S(-13,  14),   S( 10,   1),   S( 43,  -7),   S(  2,  37),   S(-29,  -3),   S( 23,  28),   S(  2, -11),   S(-15,  -8),
            S(-34, -43),   S( 11,   4),   S(  3,   8),   S(  5, -11),   S( 19,  27),   S( 34,  12),   S(  0,  44),   S(-22,   1),

            /* bishops: bucket 1 */
            S( 38,   9),   S( -6,  31),   S(  9,  39),   S( 17,  30),   S( -6,  31),   S(  3,  35),   S( -5,   4),   S(-45,  -5),
            S( 10, -14),   S( 35, -12),   S( 52,  10),   S( 28,  33),   S( -8,  19),   S(  9,   3),   S(-32,  -3),   S( 15, -16),
            S( 40,  -2),   S( 22,   6),   S( 40,  -7),   S( 22,  26),   S( 21,  29),   S(-20,   6),   S( 29,  -2),   S(  6, -29),
            S( 40,   3),   S( 17,  16),   S( 13,  13),   S( 36,  18),   S(  4,  22),   S( 21,   4),   S( -9,   6),   S( 16, -12),
            S( 35,  29),   S(  8,  20),   S( 21,  22),   S(  0,  29),   S( 24,   9),   S( -2,  16),   S( 28, -19),   S( -8,  10),
            S(  0,  22),   S( 33,  33),   S( 29,   3),   S( 54,  -5),   S( 17,  17),   S( 32, -16),   S(  0,  27),   S( 47, -15),
            S( -8,  46),   S(-25,  22),   S( 17,  30),   S( 37,  27),   S( 41,  28),   S(-18,  24),   S( 38, -18),   S(-18,  40),
            S( 13,   4),   S( 11,   7),   S(  4,  14),   S(-19,  23),   S( 21,  18),   S( -8,   6),   S( 13,   6),   S( -3,  12),

            /* bishops: bucket 2 */
            S( 18, -15),   S(  9,  19),   S( -1,  22),   S(-27,  53),   S( -8,  38),   S(-27,  33),   S(-16,  -4),   S(-45,  18),
            S(-21,  23),   S(  6, -15),   S( 22,  14),   S( -3,  31),   S( -1,  38),   S( 17,  11),   S( -8, -19),   S(  5, -33),
            S( -1,   3),   S(  0,  13),   S(  6,   9),   S( -1,  46),   S(  8,  36),   S( -1,  20),   S( 18,  15),   S(-12,  -4),
            S(  3,  10),   S(-12,  12),   S(-12,  34),   S(  1,  34),   S( -8,  39),   S(  6,  21),   S(  4,  15),   S(  9,   1),
            S(  8,   5),   S(-18,  31),   S( -8,  25),   S(-32,  43),   S(-11,  34),   S(-11,  44),   S(  2,  19),   S(-27,  32),
            S(  5,  27),   S( -5,  12),   S(-27,  28),   S(-18,  25),   S( 10,  14),   S(-11,  10),   S( -5,  54),   S( -1,  26),
            S(  1,  21),   S(-23,  12),   S(-26,  55),   S( 21,   3),   S( -3,   5),   S(-18,  11),   S(-68,  15),   S(-36,  37),
            S(-54,  31),   S(-35,  45),   S(-25,  28),   S(-39,  27),   S(-48,  38),   S(-32,  18),   S(  5,  16),   S(-70,   7),

            /* bishops: bucket 3 */
            S(  0,   3),   S( 37,  -8),   S( 32,  20),   S( 18,  21),   S( 22,  14),   S( 43,  -3),   S( 47, -21),   S( 42, -61),
            S( 11,   4),   S(  8,  -1),   S( 31,  -2),   S( 10,  35),   S( 22,  11),   S( 23,  26),   S( 45,  -2),   S( 39,  -4),
            S( 24,   7),   S( 15,  19),   S( 12,  17),   S( 25,  25),   S( 25,  55),   S( 28,   8),   S( 42,  26),   S( 48, -11),
            S( 32,  -7),   S( 23,   6),   S( 17,  30),   S( 22,  42),   S( 28,  36),   S( 31,  27),   S( 26,  19),   S( 26,  -5),
            S( 20,   2),   S( 23,  12),   S( 42,  11),   S( 26,  42),   S( 23,  43),   S( 35,  26),   S( 16,  30),   S( 24,  32),
            S( 30,   4),   S( 33,  19),   S( 22,  11),   S( 40,  13),   S( 24,  20),   S( 50,   4),   S( 51,  12),   S(  5,  68),
            S( 19,   9),   S( -7,  16),   S( 42,  24),   S( 21,  20),   S( 12,  18),   S( 16,   5),   S( -2,  29),   S( 21,  34),
            S(-34,  59),   S(  0,  33),   S( 57,  10),   S( 23,  17),   S(-16,  35),   S( -3,  36),   S( 24,   3),   S( 55, -26),

            /* bishops: bucket 4 */
            S(-23, -26),   S(-19,   9),   S(-30,  -2),   S(-19,  19),   S(-21,  30),   S(-43,  30),   S(  1,  -8),   S(-11, -10),
            S( -6,   9),   S(  4,   8),   S( -5,  35),   S(-26,  20),   S(-16,  -2),   S( 39,  -2),   S(-21,  -7),   S( 14,  -2),
            S(-11,  -2),   S(-35,  39),   S( 14, -13),   S(-23,  19),   S(  4,  30),   S( 31, -20),   S(-24,  -5),   S(-53,   0),
            S(-36,  26),   S( -6,  36),   S( 49,  30),   S( 25,  33),   S( 11,  19),   S( 50,  -9),   S( 46,  -9),   S(-13, -36),
            S(  2,  19),   S( -3,  44),   S(-23,  53),   S( 13,  39),   S( 29,   7),   S( 32, -21),   S(-12, -24),   S( 16, -10),
            S( -7,  35),   S( 16,  14),   S(-19,  25),   S( 13,  12),   S( 39,   8),   S(  6, -16),   S( 16, -37),   S(  4,  -6),
            S(-15,  10),   S( 30,  18),   S( 18,  21),   S( 25,  18),   S( 11,  -4),   S(  4,  18),   S(  2,   5),   S(  7, -27),
            S( 12, -17),   S( -9, -36),   S(  2,  -2),   S( -3,   1),   S(  7, -11),   S(  1,  10),   S( -1,  -8),   S( -5,   1),

            /* bishops: bucket 5 */
            S(-15, -13),   S(-10,  36),   S(-31,  31),   S( -9,  31),   S(-30,  33),   S(  8,  13),   S( -6,  18),   S(-22,  13),
            S(-24,  38),   S(-12,   6),   S(-24,  57),   S(  7,  29),   S(-20,  35),   S(-25,  29),   S(-32, -11),   S( -9,   0),
            S(  3,  18),   S(  2,  40),   S( 25,  14),   S(-18,  54),   S(  5,  38),   S(-25,   0),   S(-22,  35),   S(-17,   8),
            S( 29,  15),   S( 22,  30),   S(-16,  60),   S( 28,  31),   S( 28,  36),   S( 14,  30),   S( 17,  -6),   S( 13,  27),
            S( 25,  45),   S( 29,  15),   S( 47,  31),   S( 71,  33),   S( 43,  19),   S( 37,  18),   S( 35,  11),   S( -4,   4),
            S( 21,  39),   S( 20,  46),   S( 30,  21),   S( 18,  36),   S( -5,  35),   S( 14, -18),   S(-23,  44),   S(  4,  33),
            S(  2,  39),   S(-26,  16),   S( 16,  43),   S(  9,  55),   S( 31,  30),   S( 35,  41),   S(  0,  21),   S(  0,  30),
            S( -1,  -7),   S( 16,  37),   S( 17,  14),   S(  7,  38),   S(  4,  58),   S( 15,  26),   S( 31,  56),   S( -7,  -1),

            /* bishops: bucket 6 */
            S(-11,  16),   S(  6,  28),   S(-18,  28),   S(-24,  33),   S(-23,  24),   S(-27,  30),   S(-17,  52),   S(-18,   8),
            S( 23,   8),   S(  7, -12),   S(-15,  31),   S(  5,  28),   S(-23,  44),   S(-10,  26),   S(-92,  29),   S( 17,  24),
            S( 22,   1),   S( 17,   9),   S( 37,  -2),   S( 28,  29),   S( 43,  26),   S( 21,  10),   S( 11,  31),   S(-35,  20),
            S( -8,  39),   S( 15,  13),   S( 30,  21),   S( 24,  36),   S( 42,  32),   S( 34,  28),   S( 30,  32),   S(-16,   0),
            S( -2,  20),   S( 53,   3),   S( 22,  24),   S( 44,  23),   S( 89,  26),   S( 52,  26),   S( 32,  28),   S(-29,  46),
            S( 13,  11),   S(-46,  46),   S(  8,  17),   S(  6,  41),   S( 29,  30),   S( 21,  26),   S( -4,  46),   S(-12,  46),
            S(-20,  31),   S(-27,  26),   S(  7,  40),   S( -7,  35),   S( 47,  24),   S( 25,  32),   S( -7,  35),   S( -2,  35),
            S(  7,  45),   S( 15,  34),   S( 13,  41),   S(  4,  47),   S(-13,  40),   S( 36,  19),   S( 13,  25),   S( 13,   9),

            /* bishops: bucket 7 */
            S(-14, -37),   S( -4,   7),   S(-30, -26),   S(-47,  12),   S(-26, -10),   S(-73,  19),   S(-68, -27),   S(-64,   6),
            S(-30, -28),   S(-54, -39),   S(-16,  -3),   S(  3, -13),   S(-25,   2),   S(-37,  14),   S(-45, -12),   S(-32,   7),
            S(-30, -19),   S(  8, -16),   S( 28, -38),   S( 26,   0),   S(-30,  22),   S(-20, -12),   S(-36,  47),   S(-30,  27),
            S(-36,  15),   S( 52, -38),   S( 74, -25),   S( 51,   0),   S( 75,  -3),   S(  5,  20),   S( 21,  32),   S(-15,  28),
            S( 28, -50),   S( -9, -25),   S( 58, -38),   S( 93, -30),   S( 58,  21),   S( 61,  15),   S(-15,  42),   S( 20,   9),
            S(-23, -13),   S(-28,  -2),   S( 30, -47),   S( 14,  -4),   S( 39,  -9),   S( 40,   1),   S( 42,  13),   S( 21,   2),
            S(  4, -16),   S(-37,  -7),   S( 13,  -3),   S( 14,  -6),   S( 18, -19),   S( 34,  -5),   S( 11,   1),   S( 13,  14),
            S(-12,  -5),   S( -8,  14),   S(-28,  10),   S(  7,  -4),   S( 11,  -2),   S( 20,  -4),   S( 26,  10),   S(  7,  12),

            /* bishops: bucket 8 */
            S(-10,  -7),   S(-12, -33),   S(-39,  -4),   S( -3, -26),   S( -6,  19),   S(-22,  -2),   S(  7,  23),   S( -4,  -8),
            S( -6,  -2),   S(-31, -45),   S(-12, -21),   S(-14,  -4),   S( 12,  -9),   S(-17, -26),   S(-18, -53),   S( -4,  -7),
            S(  2,   2),   S( -8,  13),   S(-23,   8),   S( -8,  20),   S( -2,  15),   S( -6, -39),   S(  7, -40),   S(-31, -35),
            S(  5,  33),   S( -7,  44),   S(  6,  42),   S( -5,  13),   S( 13,  18),   S( -4,   8),   S(  2, -19),   S( -4, -16),
            S( 15,  37),   S( 12,  66),   S(-13,  33),   S( 41,  42),   S( -1,  18),   S( 13,   6),   S(  7, -32),   S(-10, -15),
            S( -3,   5),   S( 12,  36),   S(  7,  19),   S(-16,  18),   S( 25,   9),   S(-10, -17),   S(-16, -16),   S(-18, -20),
            S( -4,   5),   S( 11,  26),   S( 10,  23),   S(  1,   2),   S(  5,  13),   S( -1,  22),   S(-12, -14),   S( -9, -27),
            S( -8, -12),   S(  1, -26),   S(  0,  -5),   S( -1, -13),   S(-18,  -9),   S( -5,  -4),   S(  0,  16),   S( -7,   8),

            /* bishops: bucket 9 */
            S(-24, -32),   S( -5,   2),   S(-16,   4),   S( -8, -22),   S(-31, -27),   S(-17, -36),   S(-16, -10),   S(  8,  -4),
            S(-15, -17),   S(-36, -30),   S( -8,  -9),   S(-11,  16),   S(-43,  28),   S(-17, -15),   S(-14, -20),   S( -4,  -5),
            S(  9,  -1),   S( 20,  12),   S(-25, -17),   S(-13,  24),   S(  6,  15),   S( -7, -23),   S(-12, -21),   S( -3,  26),
            S(-15,   9),   S( 14,  17),   S(-12,  29),   S(  5,  23),   S( 18,  24),   S(  8,   3),   S(  3,  -5),   S(-15, -22),
            S( -1,  20),   S( 22,  24),   S(  5,  42),   S( 10,  53),   S(-18,  16),   S(  0,  29),   S( -9,  32),   S( -6,  -5),
            S(-12,   1),   S( 18,  46),   S(  2,  19),   S( 21,  24),   S( 10,  36),   S( -9,  -6),   S(-20,  -1),   S(-12, -11),
            S(  4,  14),   S( 20,  11),   S(  6,  11),   S(  3,  47),   S( 20,  41),   S(  8,   6),   S( -7, -14),   S( -5,  -1),
            S( -4, -27),   S( -7,  22),   S( -3,  19),   S(-18, -11),   S(-13,  -2),   S(  6,  26),   S(  1,   3),   S(-13, -18),

            /* bishops: bucket 10 */
            S(-23, -13),   S(  5, -26),   S(-31, -26),   S(-15, -22),   S(-19, -10),   S(-21, -20),   S(-11, -21),   S(-18, -27),
            S(  7, -16),   S(-26, -37),   S( -3,  -8),   S(-37,   4),   S(-34,   8),   S(-18,  21),   S(-29, -55),   S(-11, -17),
            S( 11, -10),   S(  5,  -8),   S(-34, -46),   S(  7,   9),   S(-34,  31),   S(-37,  15),   S(-21,  29),   S(  5,  16),
            S(-10, -20),   S(  3,   7),   S(  8,  -7),   S( 18,   3),   S(  8,  28),   S(-12,  57),   S(  4,  31),   S( 15,  26),
            S(-17,   1),   S(  0,  -1),   S( -8,  16),   S( 26,  29),   S(  0,  61),   S( 20,  50),   S(  7,  39),   S(  2, -16),
            S(  4, -25),   S(-27,  -3),   S(-25, -14),   S(-12,  30),   S( 24,  41),   S( 34,  25),   S(  7,  48),   S(  1,   8),
            S(-21,  -8),   S(-10, -45),   S( -8,  -8),   S( 23,  14),   S( -3,  -3),   S( 19,  39),   S( 16,  36),   S( 12,  13),
            S( -5, -28),   S( -9,   3),   S(  6,  18),   S( -9,   5),   S( -8,  18),   S( -8,  -6),   S( 11,   1),   S(  5,  22),

            /* bishops: bucket 11 */
            S(-19,   3),   S(-31, -11),   S(-46, -44),   S(-21, -29),   S(-17, -10),   S(-62, -45),   S( -9, -12),   S(-22, -22),
            S(-11, -16),   S(  0, -37),   S( -5,  -3),   S(-24, -33),   S(-44,  -7),   S(-29, -28),   S(-23, -42),   S(-22, -34),
            S( -9, -46),   S(  3, -43),   S(-27, -22),   S(  1,  -6),   S(  0,  -2),   S(-33,  12),   S( -8,  27),   S( -1,  20),
            S(-16, -38),   S(-15, -38),   S(  4, -12),   S(  0,  -8),   S( 10,  19),   S( -2,  58),   S(  6,  50),   S( 16,  27),
            S( -9, -25),   S(-19, -45),   S(-19,  18),   S( 46,  -1),   S( 27,  36),   S( -6,  59),   S( 16,  55),   S( 13,  25),
            S(-17, -50),   S(-33,  -4),   S(-15, -41),   S(  3,  11),   S(  2,  31),   S( 13,  29),   S( 25,  39),   S( -5,  -2),
            S( -7,  -6),   S(-18, -41),   S(-19,   4),   S( -5, -14),   S( 11,   0),   S( 36,  20),   S( -7,   6),   S( 14,  28),
            S(-18, -14),   S(-21,  -2),   S( -6,  13),   S( 10,   5),   S( 12,   2),   S(-16, -23),   S(  5,   7),   S( -1, -18),

            /* bishops: bucket 12 */
            S(  0,   3),   S( -8, -13),   S(-12, -29),   S( -6, -26),   S( -9, -18),   S(-11, -20),   S( -1,  11),   S( -5,   0),
            S( -7,  -7),   S(-13, -32),   S( -7, -13),   S( -5, -11),   S(-13, -21),   S( -2,  14),   S( -3,   0),   S( -1,  -8),
            S( -1,  -3),   S(-15,  -1),   S(-12, -17),   S( -8,  -3),   S( -5,   9),   S( -5, -13),   S( -9, -43),   S( -3,  -5),
            S( -1,   4),   S(  4,   0),   S(-17, -28),   S( -4,  12),   S(  1,   5),   S(  5,  23),   S( -5,  -7),   S( -6,  -3),
            S( -1,  -3),   S(  2,  17),   S( -5,  20),   S( -9,   1),   S( -4,  -5),   S( -6,   2),   S(  3,   5),   S( -7,  -1),
            S(-14, -14),   S(  3,  57),   S(-29,   2),   S(-10,  -5),   S(  6, -15),   S( -5,   1),   S(  0,   5),   S( -1,  -5),
            S( -2,  -5),   S( -4,  13),   S(  5,  20),   S( -6,   6),   S( -1,   9),   S(  8,  17),   S( -7, -16),   S( -1,   5),
            S( -2,  -3),   S(  0,  -6),   S( -5,   2),   S(  6,   8),   S(  2,  10),   S(  0,   3),   S(-10,  -1),   S(  0,  -3),

            /* bishops: bucket 13 */
            S( -8, -42),   S(-13, -28),   S(-13, -17),   S(-15, -18),   S(-15, -19),   S( -8,   1),   S( -2,  -5),   S( -7,  -9),
            S( -4,  -6),   S(-11, -12),   S(-12, -28),   S(-18,  -9),   S(-13,   7),   S( -8,   1),   S( -1, -11),   S(  3,  -2),
            S( -9, -10),   S( -5,  -5),   S( -7,  10),   S(-21,  -2),   S(-12, -22),   S( -3, -11),   S( -2, -28),   S(  5,  21),
            S( -2,   4),   S(-13,  -4),   S(-14,   4),   S(-27,   8),   S( -1,  16),   S(  3,  -6),   S( -2,   3),   S( -7,  -6),
            S( -3,   8),   S(-17,   3),   S(-17,  -4),   S( 16,  -2),   S(-10,   0),   S( -7,   6),   S(-11, -20),   S( -2,  -8),
            S( -3,  -6),   S( -9,   1),   S(-20, -13),   S( 10,  15),   S(  3,  10),   S( -4,  -7),   S(  6,  18),   S( -3,  -6),
            S( -6,  -9),   S( -9,  -2),   S(  7,  29),   S( -6,   8),   S( -6,   1),   S(  2,   2),   S(-15, -25),   S(  0,   7),
            S( -8, -17),   S( -1,   7),   S( -1,   0),   S(  5,   1),   S( -1,   5),   S( -8,  -7),   S(  1,  10),   S( -2, -14),

            /* bishops: bucket 14 */
            S( -7, -16),   S(-12, -16),   S(-18, -28),   S(-17, -43),   S(-14, -36),   S( -5, -26),   S(-10, -14),   S(-10, -16),
            S(-10, -27),   S( -1, -21),   S( -7, -13),   S(-26, -41),   S(-10, -11),   S(-17, -10),   S(-14, -22),   S(  1, -13),
            S( -8, -12),   S( -8, -30),   S(-21, -28),   S(-13, -18),   S(-25,  -4),   S(-22, -31),   S( -6,   2),   S( -3,  -2),
            S( -7, -21),   S( -9,  -6),   S(-11,  -4),   S(-24,  18),   S( -2,   7),   S(-23,  10),   S(-19, -15),   S( -5, -11),
            S( -9,  -5),   S( -9,  25),   S( -7, -19),   S( -7, -22),   S(-15,   7),   S( -8,  -6),   S(  4,  20),   S(  2,  -6),
            S( -1,   4),   S(-10,   7),   S(-22, -12),   S( -8, -16),   S(  5,   6),   S(-10,  17),   S( -3,  33),   S( -7, -21),
            S( -6, -22),   S(  0,  -1),   S( -7,   2),   S(  4,  20),   S( -9,  -1),   S( -1,   3),   S( -3, -12),   S( -4,  -6),
            S( -7,  -8),   S( -4,  -8),   S( -3,  -6),   S( -2,   6),   S(-10, -18),   S(  1,  10),   S(  6, -13),   S(  1,   4),

            /* bishops: bucket 15 */
            S(  4,   8),   S(  6,   4),   S(-19, -28),   S(  0, -11),   S( -9, -13),   S(-12, -23),   S( -5, -13),   S( -2, -10),
            S(  3,   6),   S( -1,  -7),   S(  3,  -2),   S( -8, -11),   S(-14, -21),   S( -6,  -7),   S( -8, -17),   S( -1,   0),
            S( -7, -13),   S(  0,  -1),   S(-12,  -9),   S(-10,  -6),   S(-19, -18),   S(-17, -20),   S( -6,  -9),   S(  2,  17),
            S( -4,  -7),   S(-17, -17),   S(  6, -13),   S(-23, -30),   S( -5,   5),   S( -8, -12),   S(  3,  15),   S( -1,  -8),
            S( -1,  -9),   S(-13, -18),   S(-14, -10),   S(-21, -47),   S( -3, -24),   S(-15,  20),   S(  2,  19),   S(-10, -16),
            S( -9, -32),   S(-12, -12),   S(-18, -34),   S(-22, -12),   S( -5,  -2),   S(-11, -28),   S(  7,  38),   S(  1,  11),
            S( -3,   2),   S( -1, -16),   S( -2, -13),   S( -4,   4),   S(-11, -15),   S(  0,  12),   S(-11,   0),   S(  4,   6),
            S( -3,  -1),   S( -1,   2),   S( -3,   1),   S( -6,  -3),   S( -8,  -5),   S(-16, -19),   S( -9, -23),   S(  1,   1),

            /* rooks: bucket 0 */
            S(-21,   6),   S( -9,  -1),   S(-18, -11),   S( -9,  -8),   S(-13,  10),   S( -9,  -8),   S(-15,  22),   S( -1,  16),
            S( 11, -60),   S( 26, -15),   S(  7,  -3),   S(  1,   3),   S( 16,   0),   S( -3,  -3),   S(-32,  21),   S(-44,  33),
            S(  2, -24),   S( 17,  29),   S( 22,  11),   S(  8,  13),   S(-16,  42),   S( -4,  12),   S(-30,  18),   S(-40,  16),
            S( 26, -22),   S( 57,   1),   S( 39,  30),   S( 36,   7),   S( 10,  10),   S( -5,  15),   S(-15,  21),   S(-40,  35),
            S( 56, -23),   S( 85, -16),   S( 60,  -1),   S( 33,  -7),   S( 42,   7),   S( 21,  10),   S(-14,  39),   S(-25,  36),
            S( 60, -42),   S( 98, -33),   S( 45,   8),   S( 12,  22),   S( 40,  11),   S(-47,  35),   S( 23,  23),   S(-44,  44),
            S( 36, -11),   S( 62,  -3),   S( 19,   7),   S(  3,  29),   S(-15,  32),   S( -9,  16),   S(-21,  35),   S(-17,  26),
            S( 27,  17),   S(  8,  45),   S( 12,  26),   S( -8,  39),   S(  0,  20),   S(  5,   1),   S( -4,  28),   S(  7,  22),

            /* rooks: bucket 1 */
            S(-76,  34),   S(-53,   4),   S(-56,  -7),   S(-43, -15),   S(-30, -21),   S(-29, -20),   S(-34, -10),   S(-37,  18),
            S(-27,   5),   S(-52,  17),   S(-13, -13),   S(-22, -31),   S(-27, -11),   S(-38, -11),   S(-40, -16),   S(-57,  17),
            S(  6,  10),   S(-21,  33),   S(-15,  14),   S(-41,  24),   S(-46,  32),   S( -6,   5),   S(-25,   9),   S(-48,  25),
            S(-54,  55),   S(-39,  33),   S(  4,  18),   S(-18,  21),   S(-31,  34),   S(-46,  44),   S(-38,  41),   S(-35,  16),
            S( 44,  17),   S( 21,  36),   S( 22,   3),   S(-41,  42),   S(-27,  42),   S( 11,  22),   S( -5,  19),   S(-44,  26),
            S( 39,  12),   S(  4,  29),   S(  2,  26),   S(-38,  30),   S(  3,  15),   S(-33,  44),   S(-20,  28),   S(-51,  35),
            S(-18,  30),   S(  1,  29),   S( 16,  28),   S(-50,  51),   S(-30,  35),   S( -2,  34),   S(-41,  30),   S(-59,  35),
            S( 29,  27),   S( 32,  34),   S( -3,  28),   S(-48,  53),   S( -4,  16),   S( 22,  13),   S(-14,  35),   S( -3,  12),

            /* rooks: bucket 2 */
            S(-65,  36),   S(-47,  19),   S(-46,  14),   S(-56,  13),   S(-59,   9),   S(-43,   2),   S(-31, -23),   S(-49,  27),
            S(-73,  47),   S(-59,  38),   S(-43,  27),   S(-52,  12),   S(-40,  -2),   S(-51,   1),   S(-60,  19),   S(-58,  20),
            S(-71,  66),   S(-57,  55),   S(-53,  55),   S(-32,  14),   S(-43,  28),   S(-18,  25),   S(-15,  17),   S(-34,  28),
            S(-72,  65),   S(-58,  67),   S(-42,  63),   S(-41,  52),   S(-33,  36),   S(  1,  34),   S(-39,  55),   S(-21,  36),
            S(-25,  52),   S(-48,  67),   S(-47,  59),   S(-26,  44),   S( 16,  31),   S( 15,  31),   S(-30,  53),   S(-42,  48),
            S(-38,  46),   S(-34,  49),   S(-21,  34),   S(-10,  25),   S( 12,  32),   S( 42,  18),   S( 20,  21),   S(-21,  30),
            S(-54,  43),   S(-68,  70),   S(-36,  56),   S(-18,  53),   S(  4,  31),   S( 15,  24),   S(-51,  59),   S(-39,  47),
            S(-28,  63),   S( -6,  45),   S(-70,  66),   S(-36,  47),   S(-55,  59),   S(-33,  63),   S(-53,  73),   S(-24,  45),

            /* rooks: bucket 3 */
            S( -7,  74),   S( -8,  67),   S( -5,  60),   S(  3,  49),   S( -1,  45),   S(-18,  65),   S(-12,  75),   S( -5,  38),
            S(-31,  89),   S(-13,  70),   S(  3,  63),   S(  8,  58),   S( 18,  49),   S( 14,  54),   S( 40,   2),   S( 21, -37),
            S(-34,  85),   S(-15,  85),   S( -2,  78),   S( 12,  61),   S(  8,  79),   S( 27,  72),   S( 35,  71),   S(  8,  51),
            S(-28,  92),   S(-19,  85),   S( 16,  74),   S( 20,  70),   S( 16,  74),   S( -5, 109),   S( 56,  64),   S( 17,  71),
            S(-17,  99),   S( 20,  79),   S( 13,  71),   S( 33,  71),   S( 36,  69),   S( 43,  66),   S( 84,  52),   S( 51,  46),
            S(-16,  91),   S(  8,  76),   S(  6,  72),   S( 13,  72),   S( 21,  55),   S( 42,  52),   S( 78,  35),   S( 86,  15),
            S(-38, 100),   S(-20, 100),   S(-12,  95),   S( 18,  82),   S(  9,  77),   S( 20,  72),   S( 45,  67),   S( 90,  30),
            S(-74, 147),   S( -7, 101),   S(  5,  78),   S( 35,  67),   S( 40,  59),   S( 42,  69),   S(102,  52),   S( 82,  50),

            /* rooks: bucket 4 */
            S(-85,  24),   S( -8,  -2),   S(-41,   5),   S(-26,  18),   S(-27, -18),   S(  9, -48),   S( -2, -20),   S( -6, -35),
            S(-31,  -2),   S(-41,   5),   S(-41,  15),   S(-40,  24),   S( -4,  -8),   S(-17, -20),   S(  5, -33),   S(-12, -23),
            S(  0,   8),   S(-28, -19),   S(-15,  11),   S( -8, -10),   S(  2,  -3),   S(  2,  -5),   S( 35, -13),   S(-36,   1),
            S(-30, -13),   S(  5,   5),   S(-26,  19),   S( 28,   2),   S( 21,   6),   S( 15,   1),   S( 17,  11),   S( -6,  15),
            S(-18, -10),   S( -5,  31),   S(-12,  23),   S( 67,   8),   S( 21,  23),   S( -2,  18),   S( 36,  29),   S( 31,   1),
            S( 21,   8),   S( 24,  12),   S( 49,  15),   S( 36,  13),   S( 34,  17),   S(  4,  34),   S(  7,  27),   S( 21,  32),
            S( -1,  -7),   S( 31,  28),   S( 25,  29),   S( 37,  21),   S( 53,  12),   S( 12,   1),   S( 32,  18),   S( 27,  22),
            S( 33, -57),   S( 36,  44),   S( 13,  27),   S( 11,  19),   S( 15,   5),   S( 11,  25),   S( 13,   5),   S( 17,  17),

            /* rooks: bucket 5 */
            S(-34,  32),   S(-49,  51),   S(-60,  47),   S(-52,  32),   S(-40,  23),   S(-35,  40),   S( -5,  27),   S(-31,  43),
            S(-26,  32),   S(-31,  29),   S(-78,  66),   S(-50,  38),   S(-40,  26),   S(-15,  18),   S( 10,  17),   S(-19,  20),
            S( 14,  40),   S(-37,  56),   S(-52,  55),   S(-58,  57),   S(-32,  31),   S( -4,  33),   S( -2,  42),   S(  0,  42),
            S(-22,  69),   S( -4,  47),   S(-25,  67),   S(-14,  43),   S(-18,  58),   S(  9,  61),   S( -2,  55),   S(  8,  36),
            S( 16,  60),   S(  0,  66),   S( 37,  46),   S( 28,  60),   S( 30,  56),   S( 14,  75),   S( 62,  60),   S( 28,  42),
            S( 62,  54),   S( 25,  66),   S( 51,  52),   S( 21,  71),   S( 55,  49),   S( 49,  57),   S( 53,  47),   S( 47,  41),
            S( 46,  39),   S( 20,  64),   S( 43,  53),   S( 59,  43),   S( 33,  51),   S( 43,  56),   S( 66,  48),   S( 69,  43),
            S( 92,  28),   S( 70,  32),   S( 31,  56),   S( 17,  37),   S( 46,  47),   S( 48,  48),   S( 45,  41),   S( 24,  47),

            /* rooks: bucket 6 */
            S(-40,  22),   S(-41,  35),   S(-25,  28),   S(-38,  26),   S(-58,  33),   S(-71,  60),   S(-48,  54),   S(-34,  53),
            S(-19,  28),   S(-15,  30),   S(-13,  29),   S(-44,  28),   S(-51,  46),   S(-63,  60),   S(-63,  57),   S( 27,  13),
            S(-14,  51),   S(-10,  35),   S(  4,  35),   S(-36,  41),   S( -6,  30),   S(-36,  61),   S(-29,  74),   S( 19,  37),
            S(-24,  67),   S( 32,  44),   S(  2,  58),   S(  7,  42),   S(  1,  46),   S(  0,  57),   S(-42,  64),   S(-11,  57),
            S( 11,  69),   S( 42,  58),   S( 56,  47),   S( 29,  46),   S( 13,  65),   S( 37,  53),   S( 34,  49),   S( 15,  58),
            S( 16,  61),   S( 60,  51),   S( 82,  33),   S( 35,  37),   S( 25,  48),   S( 43,  58),   S( 50,  49),   S( 66,  47),
            S( 40,  58),   S( 75,  42),   S( 81,  34),   S( 88,  21),   S( 91,  28),   S( 46,  53),   S( 49,  51),   S( 49,  47),
            S( 59,  70),   S( 27,  66),   S( 33,  51),   S( 38,  43),   S( 59,  49),   S( 52,  65),   S( 55,  61),   S( 21,  65),

            /* rooks: bucket 7 */
            S(-61, -16),   S(-39, -12),   S(-35, -19),   S(-25, -10),   S(-23,  -6),   S(-59,  35),   S(-46,  19),   S( -7, -16),
            S(-54,  15),   S(-31,   1),   S(-37,   1),   S( -5, -13),   S(-19,  11),   S(-10,  13),   S(-16,  -2),   S(-55,   8),
            S(-72,  45),   S(-28,  14),   S( -2,   4),   S(  2, -12),   S( -3,   5),   S(-26,  -3),   S(-25, -13),   S(  8,   6),
            S(-55,  40),   S(  0,  19),   S( 10,  13),   S( 20,  12),   S( 29,   1),   S( 28,   9),   S( 35,   2),   S( -9,   8),
            S(-23,  39),   S( 13,  12),   S( 51, -12),   S( 56,  -5),   S( 65,   2),   S( 90,   6),   S( 72,   6),   S( 41,  -9),
            S(-13,  31),   S( 14,  14),   S( 79, -24),   S( 98, -22),   S( 69,  -4),   S( 63,  20),   S( 63,  21),   S( 21,   4),
            S(-14,  34),   S( 16,  19),   S( 45,   4),   S( 61,   3),   S( 86,  -5),   S( 84,  -6),   S( 35,  28),   S(  8,   9),
            S(  4,  60),   S(-27,  44),   S( 32,   4),   S( 76, -22),   S( 18,   8),   S( 10,  17),   S( 42,   7),   S( 52,  -5),

            /* rooks: bucket 8 */
            S(-41, -47),   S( -9, -11),   S(  4,   3),   S(  4, -14),   S( -9, -41),   S( -9, -55),   S(-14, -24),   S( -3, -17),
            S( -2, -19),   S( -4,  -6),   S(  1, -13),   S( 11, -13),   S( -3, -25),   S( -9, -21),   S( -6, -42),   S(-17, -63),
            S(  6,  16),   S( 11, -15),   S(  6,   6),   S( 13,  10),   S(-13, -31),   S( -1, -31),   S( 13,  23),   S( -1,  -1),
            S( -7, -20),   S( -1,  25),   S( -3,   8),   S( 21,   3),   S(  6,  15),   S( -4, -12),   S( 10, -16),   S(  3,   1),
            S( -6, -11),   S(  1,  17),   S( -1,  29),   S( 16,   9),   S(  4,   7),   S( 22,   8),   S( 11,  -9),   S( 13, -32),
            S(  7,  27),   S( -6,   7),   S( 32,  41),   S( 27,  -7),   S(  0,  -5),   S(  5, -14),   S(  3,   0),   S( 10,  43),
            S(  1, -12),   S( 12, -16),   S( 23,   3),   S( 18, -18),   S( 28,   7),   S( 20, -19),   S( 19, -14),   S( 17,  -7),
            S(  3, -144),  S(  9, -16),   S( 21,   8),   S(  0,  -9),   S(  4,   1),   S(  4, -13),   S(  7,  -7),   S( 22,  -2),

            /* rooks: bucket 9 */
            S(-39, -16),   S( -6, -23),   S(-18, -28),   S(-33,  -1),   S(-16,   5),   S( -2,  -2),   S( 16, -44),   S(-32, -30),
            S( 35, -20),   S(  6, -19),   S(-12, -17),   S(-17,  -4),   S(-14, -10),   S( 22,   7),   S(  7, -28),   S( -9, -27),
            S( 15, -18),   S( 21,  -7),   S(  5,   5),   S( -7,   0),   S( -8, -20),   S( 30,  -4),   S( 15,  19),   S(  1,   0),
            S(  5,   9),   S( 10,   5),   S( 14,  20),   S( -1,   6),   S(  5,  21),   S( 25,  -2),   S( 16,  37),   S( 15,   2),
            S( 15,   5),   S(  5,  10),   S(  4,  27),   S( 15,  23),   S( 35,  32),   S( 25,  31),   S( 12,  -1),   S( 14,  -7),
            S( 15,  42),   S( -6,  10),   S( 10,   1),   S(-14,   2),   S( 12,   8),   S( 30,   7),   S(  8,  35),   S( 15,  13),
            S( 64,  17),   S( 59,   6),   S( 29,  26),   S( 53,  11),   S( 29,  -8),   S( 29,   6),   S( 38,   1),   S( 44,  24),
            S( 67, -70),   S( 41, -31),   S( 19,  25),   S( 32,  27),   S( 11,  35),   S( 27,  16),   S( 24,  11),   S( 32,   8),

            /* rooks: bucket 10 */
            S(-48, -79),   S(-13, -49),   S(-38, -28),   S(-29,  -4),   S(-31,  -5),   S(-26, -11),   S( 14, -15),   S(-29, -19),
            S(  2, -18),   S( 15, -26),   S(  3, -25),   S(  2, -15),   S(  7, -18),   S( -5,  -4),   S( 36,   5),   S( 13,  -1),
            S(-10, -18),   S( -8, -22),   S(  8, -17),   S( 25,  -4),   S(-15,  18),   S( -1,  -7),   S( 27,  26),   S( 11,  -6),
            S(  8,   0),   S(  8, -13),   S(  4,  -6),   S(  8,  15),   S( 26,  -3),   S(  1,  -4),   S( 25,  26),   S(  1,  -7),
            S( 10,  12),   S( 33,   7),   S( 16,   9),   S( 19, -20),   S( -4,  -3),   S( 14,  10),   S( 31,  29),   S(  9,  28),
            S( 37,  30),   S( 31,  41),   S( 22,  10),   S( 18,   8),   S( -1,  -7),   S( 17,  10),   S( 34,  18),   S(  8,  36),
            S( 74,  12),   S( 81,   1),   S( 77,  -5),   S( 69, -17),   S( 53, -16),   S( 37,  13),   S( 24,   7),   S( 30,   6),
            S( 61,  15),   S(  9,  -2),   S( 40,  -1),   S( 21,   7),   S( 34,  -3),   S( 28,  12),   S( 15,   2),   S( 21, -12),

            /* rooks: bucket 11 */
            S(-33, -49),   S(-24, -27),   S(-15, -27),   S(-25, -53),   S(  4, -21),   S( -4,   2),   S(-23, -32),   S(-50, -19),
            S(-12, -27),   S( -5, -43),   S(  0, -28),   S(  2, -26),   S( -1, -22),   S(-16, -17),   S( -2, -32),   S(-19,   1),
            S(  6, -30),   S( 17, -14),   S( 23, -14),   S( 15, -19),   S( 13,  -9),   S( -8,   9),   S(-21, -24),   S(-10, -54),
            S(  1,  27),   S( -1, -11),   S(  0,  13),   S( 15,   9),   S(  4,  -2),   S( 13,  33),   S( 29, -10),   S(  2, -25),
            S( 13,  11),   S( 20,  -9),   S( 29,   2),   S( 24,  -8),   S( 26,  -4),   S( 29,  -5),   S( 10,  11),   S( -1, -12),
            S( 27,  34),   S( 45,   9),   S( 28,  -9),   S( 50,  20),   S( 51,  22),   S( 40,  11),   S( -4,   6),   S( 16,  26),
            S( 60,  35),   S( 59,   3),   S( 68, -12),   S( 76, -15),   S( 46,  -9),   S( 48,  12),   S( 32,  33),   S( 52,  -3),
            S( 44,  33),   S( 14,  27),   S( 22,   8),   S( 10,  -7),   S( -5,  -2),   S( 18,  18),   S( 14,  10),   S( 32,   6),

            /* rooks: bucket 12 */
            S( -2,  -9),   S( -8, -30),   S(-12, -53),   S( -3,  -9),   S(  1,  -3),   S( -4, -34),   S(-22, -65),   S(-24, -52),
            S(  7,   5),   S( -6, -23),   S(-12, -20),   S( -7, -19),   S( -8,  -5),   S( -7, -15),   S(  1,  -1),   S(-11, -32),
            S(  3,   0),   S( -5, -19),   S( -9, -23),   S(-12,  -7),   S( -4, -21),   S(  6,  -7),   S( -7, -10),   S(  5,  -9),
            S( -6,  -8),   S(  0,  -9),   S(  2,  12),   S(  9, -10),   S(  1,  -8),   S( -9, -37),   S( -7, -11),   S( -3, -38),
            S( -3, -10),   S( -1, -19),   S( 12,   4),   S(  9,   8),   S( -8, -35),   S(  7, -17),   S( -5,  -8),   S(  1, -16),
            S( -4,  -9),   S( -5,  -8),   S( 19,  34),   S(  8,  -5),   S( -4,  -6),   S( -6, -20),   S(  1, -24),   S(  5,   9),
            S( -5,  -6),   S(  0, -27),   S(  1, -39),   S( 11,   1),   S(  7,  -2),   S( -6, -38),   S( -3,  -8),   S(  8, -18),
            S( -6, -41),   S(  7,  23),   S(  3, -20),   S(  1,   2),   S( -4, -24),   S(-12, -49),   S(-14, -31),   S(  7,  -6),

            /* rooks: bucket 13 */
            S(-12, -41),   S( -6, -26),   S( -2, -18),   S(  2,  10),   S(  7,  -4),   S(-11, -37),   S(  2, -22),   S(-17, -32),
            S( -1, -32),   S( -2, -15),   S(-12,  -6),   S( -6,  -2),   S(-10, -20),   S(  0, -11),   S(  6,   3),   S( -4, -21),
            S( -4, -28),   S( -7, -28),   S( -4, -35),   S( -1, -22),   S( 11,  14),   S(  2,  -5),   S(  2, -21),   S(  1, -33),
            S( -6, -51),   S(  4,  -3),   S( -8, -42),   S( -4,  -9),   S( 14,  14),   S( -6, -34),   S( -2, -27),   S(  3, -18),
            S( 12, -21),   S( 10, -18),   S( 17,  25),   S( -5,  -7),   S( -9, -27),   S(  4, -13),   S( -6, -38),   S(  9,  -7),
            S( -7, -39),   S( 10, -27),   S( -9, -12),   S( 13,  -7),   S(  6, -11),   S( 10,  17),   S(  8,  -2),   S(  4,   9),
            S(  6,  -7),   S(  9,  15),   S(  8,   8),   S(  1, -17),   S( 11, -27),   S( 20,   6),   S(  4, -12),   S(  2, -18),
            S(-15, -119),  S(-17, -70),   S(  5,   6),   S(  1,  -1),   S( -4,  14),   S( -4, -31),   S(-10, -27),   S(  5,   1),

            /* rooks: bucket 14 */
            S( -6, -32),   S(-15, -49),   S( -2,  -8),   S( -1, -34),   S(  4, -23),   S(-10, -23),   S(  9,  -5),   S( -6, -19),
            S(-21, -45),   S(-13, -54),   S( -8,   4),   S(-12, -39),   S(-10, -16),   S(  1, -31),   S(  7,  24),   S(  6, -11),
            S( -2, -23),   S( -8, -18),   S( -3, -17),   S( -6, -13),   S(-13, -25),   S( -7, -22),   S(  7,  21),   S( -1, -26),
            S( 12,   5),   S( -7, -33),   S( -3, -18),   S( -5,   7),   S(  4, -13),   S(  4, -12),   S( -4, -34),   S( -2, -22),
            S(  2, -13),   S(  4, -25),   S( -6, -28),   S( -9, -23),   S( -6, -16),   S( -4, -18),   S(  3,   9),   S(  8,   5),
            S(  4, -14),   S(  0, -22),   S(  1, -15),   S(  2, -18),   S(-12, -19),   S(-10,   9),   S(  6,  11),   S(  0,  -5),
            S( 18,  -2),   S(  2, -37),   S(  4, -23),   S(  2, -30),   S(  6, -44),   S(  6,   3),   S(  7,   9),   S(  9,   6),
            S( -1, -23),   S(  3, -15),   S( -9, -29),   S( 10,  12),   S(-10, -20),   S(  2,   8),   S(  4,  15),   S( -1, -16),

            /* rooks: bucket 15 */
            S( -2, -55),   S(-13, -44),   S( -1, -27),   S( -6, -27),   S(  1, -15),   S( -3,  -9),   S(-16, -54),   S( -9, -15),
            S(-14, -21),   S(-14, -28),   S(  4,  -1),   S( -7, -24),   S(-10, -27),   S(  6, -28),   S(-12, -41),   S(  7,   2),
            S( -8, -23),   S(-10, -23),   S( -2, -24),   S(  2,   1),   S( 10, -27),   S( -3, -10),   S( -3,   6),   S( -4, -12),
            S(  3, -30),   S( -4, -25),   S(-11, -17),   S( -5, -19),   S(-10, -18),   S(  3, -17),   S(  0, -17),   S( -9,  -1),
            S(  0, -11),   S( -5, -11),   S( 11,  -6),   S(  1, -10),   S(  1,   0),   S(  2,   2),   S(  0,   9),   S(  0,  15),
            S(  7,  16),   S(  2,   1),   S(  0, -13),   S(  0, -10),   S( -6,  -9),   S(  1,  16),   S(  5,  -7),   S( -9, -15),
            S( 11,  19),   S( 10,  -5),   S(  8, -34),   S( -4, -32),   S(  1, -21),   S( 10,  34),   S(  1,  -1),   S(  0,  11),
            S(  1, -19),   S( -7, -18),   S(  2,  -6),   S(  1, -10),   S( -6, -14),   S( -1, -25),   S(  1, -16),   S(  2,  -4),

            /* queens: bucket 0 */
            S(-21, -13),   S(-19, -55),   S( 50, -87),   S( 59, -57),   S( 33, -36),   S( 20,  -1),   S( 55,   9),   S( 22,  18),
            S(-10, -14),   S( 35, -62),   S( 41, -16),   S( 23,  10),   S( 26,  32),   S( 26,  21),   S(  9,  63),   S( 36,  22),
            S( 28,   4),   S( 44,  15),   S( 24,  27),   S( 20,  35),   S( 19,  19),   S( 11,  18),   S( 10,  29),   S( 37,  30),
            S( 20,  20),   S( 26,  46),   S(  6,  48),   S(  4,  51),   S(  6,  58),   S( 13,  35),   S( 14,  28),   S( 18,  30),
            S( 40,  51),   S( 30,  42),   S( 19,  41),   S( 17,  58),   S( -8,  31),   S( -7,  12),   S( 32,  23),   S( 45,  -5),
            S( 26,  59),   S( 23,  55),   S( 10,  38),   S( 17,  15),   S( 41,  -9),   S( -1,  36),   S( 23,  21),   S( 21, -21),
            S( 45,  48),   S( 50,  42),   S( 29,  37),   S( 46,  25),   S( 20,   6),   S(-12,  -9),   S( 27,  23),   S( 29,  10),
            S( 43,  28),   S( 20,  36),   S( 39,  15),   S( 34,  34),   S( 42,  29),   S(-17,   1),   S( 47,  26),   S( 42,  25),

            /* queens: bucket 1 */
            S(  1, -17),   S(-74, -24),   S(-51, -29),   S(-13, -69),   S( -9, -26),   S(-17, -46),   S( 15, -30),   S( 11,  26),
            S(-15, -26),   S(-10, -43),   S( 11, -47),   S( -2,  -1),   S( -6,   2),   S(  7,  -1),   S( 21, -40),   S(  1,  20),
            S(-26,  45),   S(  3,  -4),   S(  5,  12),   S( -4,   5),   S( -4,  33),   S(-15,  33),   S( 15,  12),   S( 23,  18),
            S(  8, -17),   S(-11,  32),   S(-15,  34),   S(  2,  47),   S( -8,  50),   S(  1,  30),   S(  1,  -1),   S( 18,  18),
            S( 14,  13),   S(  7,  27),   S( -1,  62),   S(-25,  67),   S(-17,  53),   S(  0,  15),   S( -8,  18),   S(  3,  35),
            S(  8,  27),   S( 14,  53),   S( 14,  61),   S(-40,  59),   S(-21,  48),   S(-36,  47),   S( 19,  27),   S( 15,  40),
            S(  2,  38),   S(-13,  71),   S(-22,  35),   S(-25,  70),   S(-29,  48),   S( 10,  29),   S( -9,  40),   S(-28,  45),
            S( -5,   8),   S(  7,  17),   S( 14,  26),   S(-10,  13),   S( -4,  15),   S(  6,  15),   S( 12,  26),   S( -7,  31),

            /* queens: bucket 2 */
            S( 10,  19),   S( 16, -34),   S( 10, -20),   S( -1, -15),   S(-16,  -3),   S(-22, -18),   S(-24, -22),   S( 16,  10),
            S( 20,  13),   S( 13,  40),   S( 19, -11),   S( 20, -20),   S( 17, -29),   S( 16, -47),   S( 11,  -5),   S( 35, -26),
            S( 27,   4),   S( 19,  15),   S(  3,  47),   S(  9,  37),   S(  4,  57),   S( 14,  51),   S( 12,  21),   S( 34,  13),
            S(  8,  28),   S(  1,  56),   S( -2,  44),   S(  3,  56),   S(-21,  81),   S( -3,  84),   S( 12,  20),   S(  5,  71),
            S( 16,  10),   S( -6,  60),   S( -7,  59),   S(-30,  96),   S(-35, 109),   S(-14,  79),   S( -6, 103),   S( -5, 106),
            S( 11,  25),   S(  0,  45),   S(-33,  84),   S( -6,  53),   S(-27,  90),   S(-14,  99),   S( -7,  98),   S(  9,  75),
            S(-22,  55),   S(-37,  81),   S(-16,  66),   S(  6,  64),   S(-20,  76),   S( 24,  44),   S(-16,  46),   S(-16,  81),
            S(-68,  78),   S(  0,  39),   S( 29,  40),   S( 28,  35),   S(  1,  65),   S( 17,  34),   S( 12,  28),   S(-12,  40),

            /* queens: bucket 3 */
            S( 84,  88),   S( 59,  91),   S( 50,  98),   S( 45,  80),   S( 68,  33),   S( 47,  19),   S( 21,  19),   S( 44,  54),
            S( 68, 114),   S( 61, 110),   S( 46, 114),   S( 50,  89),   S( 50,  80),   S( 65,  46),   S( 68,   6),   S( 42,  42),
            S( 68,  83),   S( 55, 106),   S( 55,  83),   S( 55,  78),   S( 52,  91),   S( 60,  95),   S( 64, 101),   S( 68,  72),
            S( 49, 121),   S( 61,  85),   S( 48,  95),   S( 38,  99),   S( 39,  96),   S( 38, 130),   S( 60, 100),   S( 54, 130),
            S( 66,  90),   S( 58, 105),   S( 54,  88),   S( 37,  97),   S( 31, 116),   S( 28, 124),   S( 41, 162),   S( 55, 152),
            S( 48, 120),   S( 56,  99),   S( 49,  93),   S( 25, 116),   S( 29, 133),   S( 67, 106),   S( 64, 135),   S( 36, 181),
            S( 60, 114),   S( 59, 102),   S( 68,  85),   S( 56,  96),   S( 29, 112),   S( 55, 112),   S( 87, 125),   S(155,  68),
            S( 73,  89),   S( 95,  78),   S( 67,  89),   S( 69,  85),   S( 30, 110),   S( 99,  57),   S(127,  59),   S(134,  58),

            /* queens: bucket 4 */
            S(-12, -23),   S(-16, -18),   S(-25,  -9),   S( -4,  -7),   S( 14, -13),   S( 37,   1),   S(-31,  -8),   S(-24,  -2),
            S(-32, -20),   S(-30,  -5),   S( 13,  -7),   S(-39,  24),   S(  5,  -5),   S(  1, -12),   S( -5,  -9),   S(-31, -14),
            S(  1,   0),   S( 11,  -1),   S( -2,  28),   S( -2,  33),   S( 26,  16),   S(  7,  -6),   S(  8, -18),   S(-24, -23),
            S(-16,   2),   S( -5,  14),   S(  4,  35),   S( -6,  29),   S( 15,  34),   S( 22,  20),   S(  4, -11),   S( -2,  -6),
            S(-10,  -3),   S( 14,  11),   S( 15,  27),   S( 27,  42),   S( 23,  28),   S( 21,   3),   S(-17, -13),   S( -7, -28),
            S(  2,  12),   S( 34,  12),   S( 25,  54),   S( 23,  45),   S( 12,   9),   S(  4,   6),   S(-15, -13),   S( -9,  -5),
            S(-12, -20),   S( -6,  16),   S(  2,  25),   S( 30,  34),   S(  9,  11),   S(-12,  -2),   S(-18, -40),   S(-19, -25),
            S( -4, -17),   S( -3,  -5),   S( 28,  37),   S(  4,  20),   S(-18, -18),   S( -6, -10),   S(-20, -33),   S( -7, -17),

            /* queens: bucket 5 */
            S(-35, -13),   S(-23, -29),   S(-27, -27),   S(-41, -28),   S(-52, -29),   S( 11, -14),   S( -4,  -3),   S(  1,  -5),
            S(-27,  -4),   S(-40, -15),   S(-65, -22),   S(-64,  -4),   S(-14,  -5),   S(-41, -15),   S(-43, -15),   S(-48, -12),
            S(-33,   3),   S(-61, -14),   S(-65,   1),   S(-37,  27),   S( 15,  50),   S( -5,  25),   S( -1,   0),   S( 16,  23),
            S(-51, -12),   S(-50,  -6),   S( -1,  36),   S( -4,  49),   S( 13,  25),   S( -2,  13),   S(  2,  -5),   S( -3,  17),
            S(-30,  -4),   S(-25,  17),   S(-11,  46),   S( -5,  43),   S( 29,  48),   S(  1,  19),   S(  2,  13),   S(-24, -26),
            S(-14,  17),   S( 10,  36),   S(-12,  41),   S(  2,  44),   S( 40,  50),   S(  5,  14),   S(  4,   4),   S(-10,  -9),
            S( -6,  12),   S( -7,  14),   S(  8,  60),   S( -1,  34),   S(  1,  39),   S( 25,  37),   S( 13,  11),   S(-16, -13),
            S( 10,  26),   S( 12,  14),   S(  4,  19),   S( 12,  50),   S( 17,  32),   S(  6,  23),   S(  2, -23),   S(-17, -13),

            /* queens: bucket 6 */
            S(-22,   9),   S(-47, -19),   S(-62, -25),   S(-73, -59),   S(-87, -50),   S(-70, -43),   S(-48, -42),   S(-26,   4),
            S(-60, -10),   S(-37,   0),   S(-47,  12),   S(-61,  10),   S(-76,  15),   S(-82,  -1),   S(-84, -19),   S(  7,  18),
            S(-36,  13),   S(-12,  13),   S(-48,  39),   S(-95,  85),   S(-42,  49),   S(-40,   0),   S(-48, -14),   S(  2,   6),
            S(-35,  14),   S(-21,  13),   S(-23,  63),   S(-44,  65),   S(  5,  44),   S( 15,  49),   S(-11,  35),   S( 11,  -7),
            S(-47,  24),   S(  0,  40),   S(-23,  54),   S( 11,  30),   S( 31,  54),   S( 61,  36),   S( 23,  32),   S( -4,  20),
            S(-18,  44),   S( -7,  21),   S( 27,  23),   S( 22,  48),   S(  9,  51),   S( 61,  68),   S( -8,  -7),   S(-11,  13),
            S( -4,   7),   S(  7,   6),   S( -6,  45),   S( -8,  37),   S( 30,  53),   S( 21,  64),   S( -7,  24),   S(-35,   1),
            S(  1,   8),   S( 19,  14),   S( 14,  35),   S(  0,  26),   S( 30,  41),   S( 21,  30),   S(  0,  19),   S(  6,  13),

            /* queens: bucket 7 */
            S( -1,  -5),   S(-28,  16),   S(-44,  25),   S(-27,  12),   S(-24,  -9),   S(-29, -24),   S(-29,  -7),   S(-18, -12),
            S(-26,  -7),   S(-39,   5),   S(-17,   6),   S(-16,  36),   S(-26,  32),   S(-39,  37),   S(-41,  22),   S(-38, -14),
            S(-27, -18),   S(-37,  30),   S(-13,  33),   S( -5,  29),   S( 10,  18),   S(  2,  25),   S(-15,  12),   S(-24,  -4),
            S(-53,   1),   S( 15,   4),   S(-13,  25),   S(  0,  38),   S( 34,  19),   S( 32,  24),   S(  8,  37),   S( -4,  18),
            S(-22,  21),   S(-47,  28),   S( 14,  19),   S( 54,  -8),   S( 61, -10),   S( 82, -16),   S( 34,  12),   S( 35,  -8),
            S(-11,  14),   S(-12,   9),   S(  9,   0),   S( 17,  -9),   S( 36,  36),   S( 76,  21),   S( 63,   2),   S( 37,  10),
            S( 12, -17),   S(  7,  11),   S(  5,  -6),   S(  5,  14),   S( 35,  19),   S( 51,  38),   S( 51,  19),   S( 48,  23),
            S( 16,   4),   S( 18,   4),   S( 19,   9),   S( 17,  17),   S( 37,  26),   S( 20,  20),   S( 13,   5),   S( 35,  43),

            /* queens: bucket 8 */
            S( -5, -10),   S( -1,   4),   S(-13,  -4),   S( -8,  -6),   S( -4,   1),   S( -1, -15),   S(-19, -23),   S( -3,   6),
            S( -6,   1),   S(-11, -15),   S( -3,   5),   S(-11,  -1),   S( -3,  -2),   S(-16, -19),   S(-18, -38),   S( -3,  -8),
            S( -2,  -1),   S( -5,   2),   S( -7,   2),   S( -4,  -9),   S( -2,   6),   S( -9, -10),   S(-11, -25),   S(-14, -26),
            S( -4,   2),   S(  9,  17),   S( 12,  19),   S(  5,  11),   S( -2,   2),   S( -5,   0),   S(  0,  -2),   S( -6, -20),
            S( 15,  27),   S(  2,  26),   S( 10,  13),   S(  9,  17),   S( 13,  33),   S(  5,   1),   S( -7,  -8),   S(-10, -19),
            S(  7,  18),   S( 10,  20),   S(-19,  12),   S( 14,  33),   S( -9, -14),   S( -5, -11),   S(  4,   3),   S(  4,  13),
            S( -6, -13),   S(-18, -26),   S( 21,  33),   S( 14,  15),   S(  1,  17),   S(  2,  17),   S( -3,  -7),   S( -6, -15),
            S(-15, -29),   S( 14,  10),   S(-16, -48),   S(-10,  -6),   S(-12, -29),   S( -1,  -5),   S( -3, -17),   S( -4,  -6),

            /* queens: bucket 9 */
            S(  6,   8),   S(-12, -26),   S(  2,  -1),   S(-28, -32),   S(-22, -37),   S(-16, -29),   S(-12, -21),   S(-12, -17),
            S( -2,  -6),   S( -8,  -7),   S(-17, -22),   S( -2,   1),   S(-15,  -7),   S(-15, -19),   S(  2,  -2),   S( -3,  -8),
            S(  5,   6),   S(  4,   8),   S( -7,  22),   S( -3,  -5),   S( -5,   8),   S(  3,   0),   S(  5,   4),   S(  5,   2),
            S( -4,  -9),   S( -5,   5),   S( 13,  40),   S(  9,  22),   S( 19,  31),   S(  4,  11),   S( -7, -15),   S(  2,  -8),
            S(  5,   9),   S(  8,  30),   S( 11,  30),   S( 17,  50),   S( 22,  34),   S( 11,  20),   S( -3,   6),   S(-10, -11),
            S(-18, -20),   S(-17,  -6),   S(  5,  20),   S( 15,  34),   S( -4,   2),   S( -1,  11),   S( -8,  -5),   S( -5,  -5),
            S( -5, -16),   S(-10, -26),   S(-10,  21),   S( 10,  28),   S( 16,  22),   S(  6,  -6),   S(  7,  -3),   S(-11, -25),
            S(  1,   0),   S( -3, -23),   S( 11,  -3),   S(  1,  15),   S( 13,   1),   S( -2,   0),   S( 12,   3),   S(  3, -14),

            /* queens: bucket 10 */
            S(  3,   0),   S( -2,   4),   S(-10, -18),   S(-21, -23),   S(-11, -14),   S( -5,  -5),   S(  3, -10),   S( -4,  -8),
            S( -7, -11),   S( -8, -15),   S(-12, -23),   S( -7, -12),   S( -5,  -7),   S(-18, -13),   S(  1,  -8),   S(-16, -17),
            S(  0, -11),   S( -8, -13),   S( -6,  -7),   S( -1,   2),   S( -6,   1),   S( -6,   4),   S(  2,   2),   S(  3,   6),
            S(  0,  -2),   S(  4,  -3),   S( -1,  -6),   S(  1,  30),   S( 15,  25),   S( -5,   5),   S( -2,  -6),   S(-13, -17),
            S( -5,  -7),   S(  7,  -5),   S( -4,   4),   S( 21,  47),   S(  0,  -4),   S( 17,  29),   S( 12,  13),   S(  1,   5),
            S( -3,  -4),   S(-19, -32),   S( -4,  -2),   S(  1,  11),   S(  5,  16),   S(  5,  21),   S( 10,   6),   S( -4, -11),
            S( -4,  -4),   S(-16, -27),   S(  8,  22),   S( -6,  -8),   S(  7,   6),   S(  3,   8),   S( -3,  -8),   S( -8,  -6),
            S(  7,   1),   S( -1, -16),   S(  8,  -3),   S(  8,  -5),   S( 17,  14),   S(  5,   5),   S( 15,  16),   S(  2,  -8),

            /* queens: bucket 11 */
            S(-10, -14),   S( -6, -19),   S(-21, -19),   S(-10, -27),   S(-12, -18),   S( -9, -10),   S( -4,  -5),   S(-12, -22),
            S(-15, -32),   S( -8,  -7),   S(-39, -34),   S(-10,  -9),   S(-13, -10),   S(-10,  -6),   S( -5,  -9),   S( -6,  -3),
            S(-16, -21),   S(-14, -33),   S(  4, -20),   S( -7, -15),   S( -9, -15),   S( -4,   4),   S(  6,  19),   S(-11,  -7),
            S(-14, -26),   S(-24, -24),   S( -6, -23),   S( 16,  26),   S( 12,   1),   S(-12,  -5),   S( 23,  24),   S( -2,   1),
            S(-13, -12),   S( -4, -15),   S(-20, -24),   S( 24,  24),   S( 15,  14),   S( 27,  49),   S( 20,  39),   S(  2,  10),
            S(-13, -29),   S(  3,   3),   S(-16, -17),   S( 15,  11),   S( 24,   5),   S( 43,  33),   S(  8,  -1),   S( -8,  -7),
            S( -7,  -3),   S(-13, -21),   S(  9,  16),   S(-12,  -4),   S(  5,   5),   S( 22,  23),   S( 36,  37),   S( -3, -17),
            S(-10, -21),   S( -9, -23),   S( -6, -20),   S(  4, -14),   S(  2,  10),   S( -3,  -8),   S( 17,   5),   S( -2, -33),

            /* queens: bucket 12 */
            S(  6,   0),   S( -1,  -1),   S(  2,   0),   S( -8,  -5),   S(-10, -12),   S( -1,  -3),   S(  0,  -2),   S( -4,  -9),
            S( -3,  -2),   S( -9, -14),   S( -9, -12),   S( -5, -10),   S( -2,  -2),   S( -6,  -2),   S( -1,  -9),   S( -5,  -9),
            S( -2,  -5),   S( -6, -10),   S( 12,  14),   S( -4,  -4),   S( -2,  -5),   S( -8, -13),   S(-12, -24),   S( -8,  -7),
            S(  2,   6),   S( -1,   2),   S(  4,   6),   S(  0,   7),   S(  8,  15),   S(  1,  -3),   S(  0,  -4),   S( -4, -10),
            S(  1,  -4),   S( 10,  12),   S( 32,  56),   S(  1,  16),   S( -5,   7),   S(  1,   7),   S(-12, -29),   S( -2, -14),
            S(  7,  17),   S( 13,  24),   S( 33,  42),   S( -3,   7),   S(  0,   5),   S(  2,   2),   S(  5,   5),   S( -4, -15),
            S(  3,   1),   S(  2,   5),   S( 17,  15),   S( 11,   8),   S(  5,   9),   S( -3,   4),   S(  9,   6),   S( -4,  -4),
            S( -5, -29),   S(-10, -26),   S(-11, -20),   S(-10, -27),   S( 10,  -7),   S(  1,  -1),   S(  1,  -6),   S( -7, -12),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -4, -14),   S(  1,  -4),   S( -2,  -7),   S( -3, -10),   S( -2,  -1),   S( -7,  -9),   S( -6,  -8),
            S(  4,  10),   S(  5,  14),   S(  5,  11),   S( -3,  -2),   S( -6,  -6),   S(  2,  11),   S(  1,   6),   S(-10, -19),
            S( -2,  -7),   S(  1,   0),   S(  3,  17),   S(  4,  12),   S( -1,   0),   S( -6,  -8),   S( -4, -11),   S(-12, -16),
            S( -3,  -4),   S(  2,   2),   S( 13,  13),   S( 19,  28),   S( 15,  33),   S( -3,  -4),   S( -5, -13),   S( -5,  -6),
            S( -3,  -5),   S(  6,  17),   S( 15,  40),   S( 12,  36),   S( 23,  43),   S(  0,  -8),   S( -4,  -6),   S( -7, -14),
            S(  0,   0),   S( 12,  31),   S( 37,  73),   S( 18,  40),   S(  0,  16),   S(  1,   7),   S(  6,  15),   S( -5, -14),
            S( -1,   0),   S( 18,  30),   S(  9,  26),   S( 13,  25),   S( -2,   7),   S(  0,  -8),   S( -1,  -9),   S(  5,   7),
            S(-12, -18),   S(  3,  -3),   S( -2,  -8),   S( -9, -12),   S(  6,   1),   S(  4,   7),   S( -7,  -6),   S( -6, -13),

            /* queens: bucket 14 */
            S( -1,  -1),   S(  0,   2),   S( -2,  -7),   S( -9,  -8),   S(  4,   7),   S( -2,  -4),   S( -1,  -8),   S( -4, -10),
            S( -5,  -7),   S(  6,  16),   S( -1,  -2),   S(  0,  -6),   S( -8, -10),   S( -6, -14),   S( -5,  -3),   S( -2,  -6),
            S( -1,  -2),   S( -9, -12),   S( -5, -10),   S(  1,   0),   S(  2,   0),   S(  1,  -4),   S(  3,   6),   S( -6, -14),
            S( -7,  -8),   S(  8,  10),   S( -4,  -3),   S( 23,  41),   S( 15,  15),   S( -1,   6),   S( 11,  24),   S(  1,  -3),
            S(  4,  12),   S(  4,   1),   S(-12,  -7),   S( 16,  26),   S( 13,  33),   S( 17,  24),   S(  9,  18),   S( -4,  -9),
            S( -2,  -5),   S(  5,  15),   S( 14,  24),   S( 11,  20),   S( 17,  40),   S( 13,  44),   S(  7,  15),   S( -3,  -9),
            S(  3,   7),   S(  8,   9),   S( 15,  35),   S( 19,  32),   S( 15,  32),   S( 13,  26),   S( 16,  28),   S(  1,   5),
            S( -3,  -1),   S( -1,  -1),   S( -9, -14),   S( 12,  19),   S(  0,   2),   S(  2,  -1),   S(  1,   4),   S(-10, -17),

            /* queens: bucket 15 */
            S( -1,  -2),   S(  1,  -5),   S( -5,  -8),   S( -3, -10),   S( -5, -10),   S( -5, -12),   S(-11, -24),   S(  0,  -7),
            S( -1,  -5),   S( -4,  -9),   S( -5, -11),   S( -4, -11),   S(  1,   7),   S( -3,  -7),   S( 11,  14),   S(  3,   1),
            S(  0,  -8),   S( -3, -11),   S(  0,  -1),   S( -4, -11),   S( -4, -11),   S(  6,  17),   S( -1,  -4),   S(  0,  -8),
            S( -5,  -8),   S(  4,   5),   S( -3,  -2),   S(  3,   2),   S(  1,  10),   S(  0,   6),   S(  5,   5),   S(  3,   4),
            S( -2,  -7),   S( -2,  -5),   S( -8, -12),   S( -4,  -4),   S(  5,  11),   S(  8,   6),   S( -4,  -7),   S(  0,  -8),
            S( -3,  -6),   S( -2,  -6),   S( -1,   2),   S(  0,   0),   S( -2,  -6),   S( 19,  29),   S(  3,  -2),   S(  0,  -9),
            S( -6, -13),   S(  3,  -5),   S(  5,   8),   S(  7,   7),   S(  6,   9),   S( 21,  37),   S( 11,  19),   S(  4,   5),
            S(  1,  -4),   S( -5,  -5),   S( -2,  -4),   S( 10,  12),   S(  7,   2),   S(  3,  -5),   S( -3,  -8),   S( -7, -22),

            /* kings: bucket 0 */
            S( 50,  13),   S( 38,  60),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 31,  46),   S(112,  67),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 40,  25),   S(-11,  42),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 88,  49),   S( 69,  60),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-12,  43),   S( -6,  33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 61,  63),   S( 62,  47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10,  60),   S(-36,  28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 19,  91),   S(-45,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  7, -53),   S( 82, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39,  -6),   S( 40,  15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 42, -10),   S( 29,   5),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 34,  31),   S( 25,  27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 53,  17),   S( 16,  14),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 41,  45),   S( 22,  42),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 75,  29),   S( 11,  -7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 61,  58),   S( -8,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61, -120),  S( 17, -63),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -105),  S(-94, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  9, -55),   S(-30, -47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-42, -34),   S(-53, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-13, -39),   S(-13, -40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-45, -22),   S(-86,   2),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -9, -44),   S(-47, -106),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-79, -12),   S(-19, -89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -106),  S(-78, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -224),  S(-18, -96),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-64, -59),   S( 24, -66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-55, -75),   S(-27, -99),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-12, -55),   S(-107, -20),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 17, -114),  S(-66, -69),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-123,  -1),  S(-35, -114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-47, -73),   S( -3, -227),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -22),   S(-19,  23),   S( 13,   1),   S(-11,  31),   S( 24,   5),   S( 47,   9),   S( 58,  -3),   S( 49,   0),
            S( -9, -28),   S(-24,   9),   S(  2, -10),   S( -1, -10),   S( 20,   3),   S(  8,  14),   S( 33,   4),   S( 22,  24),
            S(  4, -31),   S(  2, -18),   S( 30, -35),   S( 10, -17),   S( 19,  -9),   S(  9,  29),   S( -1,  52),   S( 25,  20),
            S(  9, -20),   S( 28,   5),   S( 50, -29),   S( 32,  -4),   S( 18,  43),   S( -9,  86),   S( 15,  91),   S( 53,  63),
            S( 92, -54),   S(119, -16),   S( 85, -23),   S( 46,  17),   S( 49, 135),   S( 10, 136),   S( 23, 155),   S( 66, 132),
            S(-218, -75),  S(-128, -134), S( 11, -170),  S( 34,  44),   S( 88, 197),   S( 75, 188),   S(113, 168),   S( 98, 148),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  19),   S(-35,  33),   S(-14,  15),   S(-37,  62),   S(-10,   8),   S( 20,  12),   S( 26,   7),   S( 15,  25),
            S(-51,  15),   S(-42,  25),   S(-27,  11),   S(-19,  11),   S(  4,   8),   S( -8,  11),   S(  0,   7),   S(-15,  20),
            S(-47,  19),   S(-17,  22),   S(-25,   4),   S(  3, -10),   S( -2,  17),   S(-24,  19),   S(-29,  35),   S(-18,  26),
            S(-39,  40),   S(  8,  30),   S(-22,  25),   S(  5,  26),   S(  0,  27),   S(-32,  46),   S(  5,  42),   S( 25,  54),
            S(  7,  33),   S( 56,   4),   S( 87, -25),   S( 79, -20),   S( 36,  30),   S( 10,  36),   S(-20,  81),   S( 40,  89),
            S( 40,  42),   S(-37, -18),   S(-16, -102),  S(-20, -99),   S(-39, -67),   S( -3,  47),   S( 49, 186),   S( 68, 212),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  38),   S(-19,  29),   S(-16,  17),   S(  1,   9),   S(-22,  37),   S( -6,  15),   S( 13,  -2),   S( -6,  19),
            S(-47,  29),   S(-31,  32),   S(-26,  10),   S(-25,  20),   S(-20,  19),   S(-26,   9),   S( -8,  -4),   S(-36,  10),
            S(-45,  45),   S(-31,  53),   S(-12,  15),   S(-14,  14),   S(-19,  17),   S(-26,   3),   S(-28,  11),   S(-36,   7),
            S(-31,  86),   S(-33,  76),   S(-13,  41),   S(  0,  33),   S(-10,  30),   S(-25,  17),   S(  5,  22),   S( 16,  11),
            S(-24, 131),   S(-36, 117),   S(  2,  23),   S( 24, -20),   S( 90,  -9),   S( 87,  -5),   S( 72, -14),   S( 42,   5),
            S( -7, 245),   S( 42, 176),   S( 15,  72),   S( 27, -90),   S(-24, -170),  S(-87, -132),  S(-28, -63),   S(  9,  21),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  13),   S(  9,  20),   S( 13,  15),   S(  3,  35),   S(  0,  52),   S( 37,  22),   S( 31,   5),   S(  5, -13),
            S(  1,  15),   S(  7,  29),   S(  3,  10),   S(  0,  11),   S( 13,  16),   S( 21,   1),   S( 15,  -4),   S(-20,  -5),
            S(  2,  32),   S( -3,  59),   S( 10,  19),   S(  8,  -1),   S( 26, -13),   S( 13, -13),   S(  6, -17),   S(-19, -14),
            S(  2,  89),   S( -9, 107),   S( 15,  64),   S( 21,  28),   S( 26,  -1),   S( 33, -25),   S( 19,   8),   S( 31, -20),
            S(  3, 155),   S( -3, 167),   S(-18, 166),   S( -2, 111),   S( 37,  52),   S( 83, -11),   S(105, -29),   S( 96, -38),
            S(103, 127),   S( 52, 240),   S( 31, 251),   S( 13, 207),   S(-22,  94),   S( 27, -174),  S(-76, -235),  S(-156, -179),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 75,  -4),   S( 32,   9),   S( 11, -12),   S(-10,  -8),   S(  6,  -8),   S( 10,  -9),   S(  7,  -2),   S(-56,  41),
            S( 37,  -5),   S(-11,  28),   S( -6,  -1),   S(-23,  -3),   S(-25, -20),   S(-12, -14),   S(-26, -15),   S(-44,   2),
            S( 54, -14),   S( 73, -16),   S( 14, -13),   S(-42,   0),   S(-72,  10),   S( -7,   3),   S(-66,  26),   S(-65,  28),
            S(-93, -73),   S(-18, -89),   S( 67, -60),   S(-34,   5),   S(-24,  15),   S(-47,  61),   S(-18,  55),   S(-49,  73),
            S(-41, -75),   S(-67, -114),  S(-14, -94),   S( 54,   6),   S( 77,  87),   S(  9,  99),   S( 26,  76),   S(  6,  97),
            S(  1, -64),   S(-18, -78),   S( -1, -68),   S(  2,  48),   S( 57,  85),   S( 70, 149),   S( 46, 155),   S( 60, 125),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-57,  45),   S(-36,  49),   S(  1,  22),   S( 58,   7),   S( 68,   0),   S( 14,   5),   S(-12,  18),   S(-57,  47),
            S(-88,  41),   S(-53,  49),   S(-42,  29),   S(-30,  28),   S(-41,  27),   S(-37,  13),   S(-57,  14),   S(-76,  33),
            S(-56,  31),   S(-60,  65),   S(-13,  36),   S(-29,  47),   S(-56,  49),   S(-78,  37),   S(-67,  39),   S(-65,  41),
            S(-48,  41),   S(-31,  19),   S(-47, -36),   S(-10, -27),   S(-15,  -6),   S(-57,  34),   S( -3,  33),   S(-28,  53),
            S( 46,   9),   S(-20, -28),   S( 11, -92),   S( -9, -72),   S( 39, -41),   S( 24,  21),   S(-13,  70),   S(-33, 113),
            S( 46,  31),   S( 19, -13),   S(-32, -69),   S(-23, -63),   S(-32, -58),   S( 47,  39),   S( 68, 135),   S( 40, 147),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-83,  39),   S(-44,  24),   S(-11,   6),   S( 20,   4),   S( 14,  27),   S( 23,  10),   S( 21,  13),   S(  1,  24),
            S(-73,  22),   S(-53,  20),   S(-53,  12),   S( -3,  16),   S(-36,  31),   S(-31,  18),   S(-37,  23),   S(-22,  13),
            S(-60,  31),   S(-71,  45),   S(-63,  33),   S(-63,  47),   S(-29,  45),   S(-24,  26),   S(-24,  33),   S(-35,  19),
            S(-85,  85),   S(-54,  60),   S(-38,  34),   S(-20,  14),   S(-22, -34),   S(-28, -27),   S(-37,  14),   S( 11,   0),
            S( -7, 100),   S(-40,  72),   S( 22,  12),   S(-24, -28),   S(-13, -72),   S(-54, -66),   S(-20, -28),   S( 70,  -5),
            S( 81,  76),   S( 72,  89),   S( 46,  23),   S( 35, -78),   S( -9, -104),  S(-42, -54),   S( -7, -45),   S( 75,   1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49,   1),   S(-30,  -9),   S(  5, -21),   S(-59,  49),   S( 25,   8),   S( 72, -19),   S( 63, -19),   S( 70, -13),
            S(-55,   1),   S(-53,   8),   S(-30, -19),   S(-39,   4),   S( -9,   0),   S( 27, -24),   S(  6,  -3),   S( 40, -16),
            S(-57,  22),   S(-69,  41),   S(-37,   6),   S(-45,   0),   S(-15,   1),   S(  1, -11),   S( 22,   0),   S( 37, -17),
            S(-54,  60),   S(-81,  82),   S(-48,  57),   S(-33,  33),   S(-17,  -3),   S( 38, -58),   S(  5, -64),   S( 20, -109),
            S( 17,  60),   S(-50, 134),   S(  6, 115),   S( -6,  86),   S(  7,  21),   S(  6, -79),   S(-49, -130),  S(-31, -98),
            S(129,  83),   S( 84, 125),   S( 97, 104),   S( 63,  93),   S( 33,   3),   S(  1, -104),  S(-29, -94),   S(-12, -184),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 17,   7),   S( -7,  15),   S( 46,  -3),   S( -9, -37),   S(-28, -59),   S(-17, -24),   S( 17, -49),   S( 34, -40),
            S(  8, -59),   S( -1,  -8),   S(-40, -57),   S(-59, -35),   S(-24, -58),   S( 41, -62),   S( 13, -59),   S( -3, -53),
            S( 17, -97),   S(  7, -56),   S(-15, -66),   S(-41, -56),   S(-28, -31),   S( 16, -43),   S(-34, -16),   S( -2, -32),
            S( -1, -29),   S(-28, -36),   S( 12, -23),   S(-14,  -6),   S(-19,   5),   S(  7,  19),   S(  3,  27),   S( -7,  20),
            S( 26,   3),   S(  1, -30),   S(  8,  42),   S( 34,  91),   S( 54, 119),   S( 32, 118),   S( 16,  96),   S(-30, 102),
            S( 18,  32),   S(  4,  54),   S( 24,  68),   S( 31, 100),   S( 46,  94),   S( 51, 147),   S( 40, 101),   S(-21,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 14,  10),   S( 15,  31),   S( 11,  15),   S(  1,  24),   S( 15,   2),   S(  0, -14),   S( 30, -37),   S(-22, -18),
            S( 41, -55),   S( -2, -40),   S( -7, -55),   S(-30, -44),   S(-36, -25),   S(-53, -26),   S(-50, -25),   S( 18, -45),
            S(-21, -44),   S(-44, -36),   S(-40, -74),   S(-76, -43),   S(-15, -36),   S(-19, -46),   S(-51, -29),   S( 15, -35),
            S(-49,  -1),   S(-49, -47),   S(-13, -71),   S(-46, -32),   S( -4, -46),   S( -1, -26),   S( 18,  -5),   S(  1,   5),
            S(  3,  11),   S( -5, -19),   S(-16,   4),   S( 20,  27),   S( 17,  60),   S( 21,  51),   S(  6,  67),   S(  2,  61),
            S(-10,  66),   S( 27,  61),   S( -2,  56),   S( 21,  60),   S( 26, 108),   S( 17,  83),   S( 17,  79),   S( 16,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -50),   S( -6, -39),   S( -9, -16),   S( -5, -14),   S( 28,  15),   S( 46,  11),   S(  6,  14),   S( -6, -17),
            S( -9, -60),   S(-66, -34),   S(-19, -49),   S( 12, -37),   S(-27, -26),   S(-27, -20),   S( -2, -27),   S(  5, -42),
            S(-20, -48),   S(-89, -17),   S(-70, -39),   S(-30, -29),   S(-34, -48),   S(-32, -63),   S(-39, -53),   S( 52, -70),
            S(-39,  -2),   S(-18,  -3),   S(-28, -34),   S(-58, -42),   S( -6, -71),   S(-51, -56),   S(-19, -51),   S( 16, -53),
            S( 10,  15),   S( 32,  17),   S( 19,  11),   S(-19,  -3),   S( 10,  18),   S( 13,  12),   S(-26,   8),   S( 42,  -6),
            S(  8,  24),   S(  2,  49),   S( 25,  54),   S(  8,  57),   S( 24,  81),   S(  1,  43),   S(-13,  18),   S( 27,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -46),   S(  4, -39),   S(-29, -40),   S(  6,  -2),   S(  2, -21),   S( 61,   9),   S( 33,   0),   S( 49, -11),
            S(-35, -62),   S(-45, -56),   S(-28, -72),   S(  7, -63),   S(-28, -29),   S( 13, -47),   S(  9, -35),   S( 36, -71),
            S(-18, -43),   S(-84,  -1),   S(-25, -25),   S(-10, -30),   S(-61, -47),   S( 30, -67),   S( 23, -119),  S( 67, -103),
            S(-50,  19),   S(-67,  36),   S(  6,  23),   S( 18, -12),   S(-28, -18),   S(-23, -49),   S(-35, -53),   S( 35, -101),
            S(-14,  17),   S(-14,  67),   S( -9,  92),   S( 20,  59),   S( 27,  57),   S( -6,   2),   S(  1,   7),   S(  7, -28),
            S( 15,  67),   S( 27,  55),   S( 32,  78),   S( 25,  80),   S( 12,  60),   S( 33,  78),   S( 12,  30),   S( 26,  34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -104),  S( 28, -49),   S( -3, -28),   S(  0,  -2),   S( -5, -29),   S(-32, -71),   S( 19, -39),   S(  6, -47),
            S( 37, -89),   S( 27, -46),   S(-23, -78),   S(-32, -62),   S(-31, -89),   S( -7, -62),   S( -6, -85),   S(-20, -69),
            S( -8, -65),   S( -8, -78),   S(-22, -96),   S(-25, -87),   S(-10, -57),   S( -4, -48),   S(-34, -53),   S( -7, -80),
            S(-13, -38),   S( -3, -14),   S(-20, -22),   S( -3,  -2),   S( 17,  54),   S(  5,  40),   S(  7,  15),   S( -6,  -8),
            S( 11,  22),   S(  1,  16),   S(  3,  24),   S( 20,  60),   S( 31,  75),   S( 27,  87),   S( 14,  81),   S( 20,  51),
            S( 12,  29),   S(  1,  35),   S( 12,  51),   S( 12,  60),   S( 25, 101),   S( 24,  92),   S(-20, -23),   S(-13,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -62),   S( 22, -76),   S( 18,   5),   S( -2, -12),   S(  5, -21),   S(-33, -37),   S( -9, -66),   S(-15, -69),
            S( 25, -133),  S( 18, -101),  S( -3, -87),   S(  8, -14),   S(-26, -55),   S(  2, -81),   S(  2, -86),   S(  3, -89),
            S( 29, -89),   S( -7, -73),   S( -3, -91),   S(  6, -61),   S(-43, -29),   S( 23, -75),   S(  0, -70),   S( 59, -91),
            S( 16, -27),   S(  2, -31),   S(  2, -31),   S( -4,  22),   S( 14,   6),   S(-13,   7),   S(-10, -12),   S(  7, -24),
            S( -4,  40),   S(  9,  26),   S( -2,   4),   S( 22,  54),   S( 37,  78),   S( 27,  88),   S( 13,  94),   S( -7,  55),
            S( 11, 102),   S( 30,  51),   S(  4,  35),   S( 12,  44),   S( 20,  63),   S( 10,  51),   S( -3,  38),   S(  2,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -119),  S(  5, -65),   S( -6, -40),   S(  2,   2),   S( -6, -14),   S( -4,  -1),   S( 15, -62),   S(-14, -41),
            S( 18, -115),  S(-34, -102),  S( -6, -83),   S(-29, -89),   S(-11, -60),   S( 15, -57),   S(  4, -62),   S( 21, -89),
            S( 16, -97),   S(-18, -73),   S(-14, -66),   S(  4, -77),   S(-22, -52),   S(  3, -95),   S(  4, -97),   S( 37, -65),
            S(  4, -35),   S(-19, -37),   S( -5,  -6),   S(-20, -13),   S( 13, -56),   S( -3, -28),   S( 14, -26),   S( 13,  -9),
            S(-15, -16),   S(  5,  44),   S( 11,  51),   S( -8,  15),   S( 19,  69),   S(  2,  14),   S( 18,  47),   S( 22,  62),
            S( -5,  29),   S(  7,  47),   S( 26,  73),   S( 20,  71),   S( 16,  57),   S(  2,  33),   S( 24,  82),   S( 22,  91),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -28),   S(  7, -54),   S(-24, -52),   S(-10, -26),   S(-12, -29),   S(-16, -41),   S( -8, -54),   S(  1, -86),
            S(-22, -67),   S(-18, -93),   S(-14, -105),  S( -9, -37),   S(-20, -26),   S( -6, -36),   S(  8, -54),   S(  8, -111),
            S(-26, -49),   S(-30, -58),   S(-43, -53),   S(  8, -43),   S(-31, -40),   S( -7, -75),   S(  5, -48),   S(  4, -49),
            S(  9, -38),   S(-25, -13),   S( -2,  40),   S(-20,  10),   S( 11,   5),   S( -9, -22),   S( -5, -13),   S( -7,  29),
            S(  6,  44),   S(  3,  52),   S(  1,  69),   S( 12,  60),   S( 25,  80),   S( 12,  62),   S( 17,  56),   S(  8,  20),
            S(-22,   3),   S( -7,   4),   S( 10,  71),   S( 20,  54),   S( 21,  68),   S( 18,  56),   S( 11,  33),   S( 15,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-75, -30),   S(-26, -27),   S(-18,  -6),   S( -8,  24),   S(-15, -24),   S(-25,   1),   S( -4, -28),   S(-74, -41),
            S( 14, -36),   S(  1,   0),   S(-21, -31),   S( -8, -10),   S( -9,  -6),   S( -7, -23),   S(-33, -47),   S(-29, -36),
            S(-18, -21),   S( 21, -33),   S(  0,   7),   S( 34,  22),   S( -9,  10),   S(  8,  -6),   S(-29,  21),   S(-25, -36),
            S( 12,  21),   S( 33,  49),   S( 29,  31),   S( 36,  18),   S( 28,  16),   S( 11,  21),   S( 36, -20),   S(-11, -25),
            S( 58,  39),   S( 21,  52),   S( 62,  59),   S( 56,  41),   S( 65,  29),   S( 16,  20),   S( 18, -10),   S(  5,  -4),
            S( 91, -41),   S( -3,  44),   S(131,  -5),   S( 70,  41),   S( 53,  46),   S(-38,  63),   S( 35, -12),   S(-22,   4),
            S( 48,  -8),   S( -3, -21),   S( 41,  22),   S( 83,  70),   S( 39,  27),   S( 10,  32),   S( -9,   6),   S(-44,   1),
            S(-112, -123), S( -2,   0),   S(  7,   4),   S( 19,  23),   S(  3,  32),   S( 19,  12),   S(-33,  -5),   S(-10,  10),

            /* knights: bucket 1 */
            S( 17,  -4),   S(-52,  19),   S(-20,   9),   S(-31,  31),   S(-14,  34),   S(-16, -21),   S(-24,  -7),   S(  8, -20),
            S(-36,  31),   S(-45,  54),   S(-25,  25),   S(-14,  21),   S(-18,  18),   S( -2,  21),   S(-10,  -4),   S(-16, -53),
            S(-31,  25),   S(  1,  -1),   S(-18,  17),   S( -6,  50),   S(-11,  35),   S( -4,   7),   S(-39,  28),   S(-11,  21),
            S(-11,  65),   S( 28,  33),   S( -5,  51),   S( -9,  62),   S( -5,  54),   S( -5,  51),   S(  1,  21),   S(-24,  47),
            S( 61,  -4),   S( 12,  17),   S( 46,  57),   S( 17,  51),   S( 40,  44),   S( -1,  62),   S( -6,  46),   S( -4,  58),
            S( 23,  23),   S( 51, -17),   S( 72,  17),   S( 84,  24),   S( 69,  22),   S(-28,  74),   S( 16,  28),   S(  1,  39),
            S( 13,   1),   S( 32,  -5),   S( 28, -10),   S( 25,  50),   S(  9,  36),   S( -3,  24),   S( 17,  73),   S(-29,  43),
            S(-156, -35),  S( 15, -17),   S(-36, -61),   S(-19,  10),   S( -4,  12),   S( 39,  49),   S( 17,  45),   S(-69,  22),

            /* knights: bucket 2 */
            S(-59,   9),   S(-33,  30),   S(-21,   2),   S(-10,  18),   S(-11,  12),   S(-48,   3),   S(-22,   5),   S(-14, -28),
            S(-16,   6),   S( -1,  32),   S(-20,   9),   S(-17,  18),   S(-24,  23),   S(-13,   6),   S( 11,   4),   S(-29,   1),
            S(-27,  45),   S(-21,  22),   S(-18,  17),   S(-14,  54),   S(-16,  43),   S(-16,   8),   S(-21,  14),   S(  3,  -7),
            S( -3,  47),   S( -4,  38),   S(-22,  71),   S(-11,  71),   S(-29,  70),   S(  7,  45),   S( 13,  31),   S( -1,  35),
            S( -7,  59),   S(-17,  66),   S(  5,  65),   S( 19,  57),   S(  3,  65),   S( 19,  68),   S( -2,  56),   S( 20,  13),
            S(-41,  66),   S(-20,  50),   S(-11,  83),   S( 38,  26),   S( 38,  26),   S(114,  -5),   S( 64,   2),   S( 29, -12),
            S( 32,  35),   S(-41,  59),   S( 43,  26),   S( 27,  11),   S( -8,  50),   S( 13,  -4),   S( 24,  24),   S( 20,  -4),
            S(-56,  29),   S( 28,  62),   S(-14,  70),   S(-11, -23),   S(-24,  -8),   S(-34, -46),   S( 17,  -3),   S(-123, -55),

            /* knights: bucket 3 */
            S(-48,  17),   S( -8, -52),   S(  4, -20),   S(  6, -11),   S(  8, -16),   S( -4, -25),   S(-14, -25),   S(-24, -74),
            S(-13, -33),   S(  4,  -7),   S( 11, -13),   S( -1,  -3),   S( -2,  -2),   S( 22, -19),   S( 27, -40),   S( 25, -59),
            S( -9,  -6),   S(-10,   5),   S(  6,  15),   S( 11,  40),   S( 16,  26),   S(  2,  14),   S( 15,  -2),   S( 25, -36),
            S( 12,  -4),   S( 17,  21),   S( 19,  41),   S( 14,  47),   S( 16,  68),   S( 31,  56),   S( 37,  45),   S( 19,  33),
            S( -2,  40),   S( 24,  29),   S( 26,  49),   S( 29,  75),   S( 29,  74),   S( 41,  81),   S(  8,  90),   S( 66,  77),
            S(-10,  27),   S(  5,  41),   S( 14,  56),   S( 25,  71),   S( 59,  73),   S(131,  62),   S( 63,  73),   S( 23,  90),
            S(-22,  37),   S(-12,  46),   S(-10,  61),   S( 31,  63),   S( 48,  64),   S( 94,  46),   S( 14,   2),   S( 81,  19),
            S(-149,  36),  S(-29,  74),   S(-44,  84),   S( 35,  50),   S( 59,  79),   S(-51,  71),   S(-28, -42),   S(-62, -107),

            /* knights: bucket 4 */
            S( 10,  11),   S( -8,  -8),   S(-47,  16),   S(-28, -10),   S(-28,  23),   S(-14, -11),   S( 20, -27),   S(-17, -16),
            S( 21,  36),   S(  7, -21),   S( -5,  12),   S( -5,   5),   S( -1, -10),   S( 18, -42),   S( -7,  12),   S(-44,  -5),
            S( -5, -17),   S( 13,  -1),   S( 50,   5),   S( 60,   6),   S( 14,  19),   S( 43, -31),   S( -7, -25),   S(-10, -33),
            S(-33, -35),   S( 23,  -4),   S( 34, -20),   S( 63,  -3),   S( 28,   9),   S( -8,  23),   S(-30,  23),   S( -5,   7),
            S(-13, -50),   S( 16, -16),   S( 44,   8),   S( 27,  42),   S( 46,   4),   S( 13,  15),   S( 30, -12),   S(-30,  38),
            S( -5, -26),   S( -6,  -5),   S( 32, -26),   S( 53,  22),   S(  3,  20),   S(-21,  36),   S(-21,  -1),   S( 20,   1),
            S(-18, -30),   S(-23,  -9),   S(  4,  -3),   S( 23,  20),   S( 26,  12),   S(  0,  11),   S( 14,  35),   S(-34, -13),
            S(  3,  13),   S(-12, -36),   S( -7, -30),   S( 15,   1),   S( 13,  18),   S( -4,  14),   S( -5,  17),   S(-17, -16),

            /* knights: bucket 5 */
            S( 20,  22),   S( 20,  27),   S(-24,  35),   S( -4,  24),   S(  0,  30),   S( 18,  16),   S(-10,  18),   S( 10,  22),
            S( 23,  28),   S( 38,  25),   S(  8,  10),   S(-12,  17),   S( 44,  -8),   S(-20,  15),   S( -3,  42),   S(-43,  17),
            S(-30,  25),   S( -8,   5),   S( 22,  15),   S( 22,  21),   S( 21,  18),   S(-16,  25),   S( -5,  15),   S(-45,  16),
            S( 25,  15),   S( 18, -20),   S( 33,  -1),   S( 68, -16),   S( 72,   2),   S( 65,   6),   S( -5,  18),   S( 20,  30),
            S( 36,   5),   S( 11,  -9),   S( 71, -14),   S(106, -12),   S( 70, -18),   S( 34,  19),   S(  1,   6),   S( 16,  22),
            S( -5, -20),   S( 28, -28),   S( -3, -22),   S(  1,  11),   S( 19,  -1),   S( 41,   3),   S(-16,  15),   S( 26,  31),
            S(  0,   4),   S(-29, -58),   S( -6, -47),   S(-11, -14),   S( -9, -37),   S(  5,   5),   S( -3,  41),   S( 20,  32),
            S(-22, -38),   S(-27, -66),   S(  9,  -9),   S(-24, -27),   S(  6,  -5),   S(  0,  29),   S( 20,  36),   S( -2,  18),

            /* knights: bucket 6 */
            S( -5, -10),   S(-36,  23),   S(-14,   5),   S(-29,  37),   S(-29,  31),   S( -5,  33),   S( -7,  42),   S(-33,   7),
            S(  9, -16),   S( -6,  45),   S( -6,   2),   S( 31,   9),   S( 20,  21),   S(-38,  40),   S(-13,  48),   S(-36,  66),
            S( -2,  15),   S( 19,  15),   S(  5,  26),   S( 24,  36),   S( 16,  36),   S(-51,  45),   S( 19,  30),   S(-12,  40),
            S( 11,  44),   S( 46,   6),   S( 27,  27),   S( 57,  10),   S( 67,  -6),   S( 49,  12),   S( 12,  13),   S(-23,  45),
            S( -5,  37),   S( 23,  15),   S( 76,  10),   S( 92,   2),   S( 79, -16),   S( 41,  22),   S( 92, -21),   S( 17,  27),
            S( 15,  16),   S( 15,  10),   S( 39,  23),   S( 25,  11),   S( 34,  -2),   S( 28,  -5),   S( -4, -11),   S( 18,   3),
            S(  3,  28),   S( 18,  33),   S( 33,  37),   S( -3,  -7),   S( 23, -11),   S( 17, -36),   S( -9,  -7),   S( 11,  39),
            S( 13,  29),   S(  1,  26),   S( 15,  32),   S(  0,  15),   S(  6,  -6),   S( -9,  -3),   S(  8,  22),   S(-25, -37),

            /* knights: bucket 7 */
            S(-33, -43),   S(-15, -43),   S(  4, -16),   S(-35,  19),   S(  0,  -3),   S(-32,   5),   S(-13,  -7),   S(-15,  20),
            S(-32, -53),   S( -2, -29),   S(-31,  -7),   S(-27,  -2),   S(  6,   8),   S(  1,  26),   S( -5,  12),   S(-57,  37),
            S(  5, -40),   S(-29, -24),   S( 12, -17),   S( -1,  23),   S( 40,  18),   S( 29,  13),   S(  3,  22),   S( -6,  30),
            S(-34,  11),   S(  9, -10),   S( 54, -22),   S( 72,   3),   S( 96,  -9),   S( 68,  12),   S( 53,   0),   S( 55,  -2),
            S(  3,   2),   S( -1,   5),   S( 18,  14),   S( 64,   2),   S( 89,   6),   S(115, -23),   S(175, -22),   S( 14, -18),
            S(-18,  10),   S( 23,   6),   S( -8,   8),   S( 34,  23),   S( 83,   4),   S( 80,  -9),   S( 10, -15),   S( -6, -47),
            S(-20,   1),   S( -3,   1),   S(  0,  12),   S( 24,  23),   S( 56,  18),   S( 25,  21),   S(-17, -35),   S(-18, -40),
            S(-31, -41),   S( -9,   6),   S( -4,  19),   S(  5,  17),   S( 11,   8),   S( 17,  10),   S(  4,  -5),   S(  0, -10),

            /* knights: bucket 8 */
            S( -2,   3),   S(  9,  25),   S( 11,  26),   S( -9, -29),   S( -1,  24),   S( -3, -18),   S( 13,  24),   S( -3, -14),
            S( -6, -22),   S( -5, -22),   S( -8, -36),   S(-11,   7),   S( -5,  36),   S(  1,  -5),   S(  0,  -6),   S( -3,  -4),
            S(-11, -38),   S( -8, -23),   S(  0, -42),   S(  3,  14),   S(-11, -17),   S( 11,  10),   S( -1,  -6),   S( -1, -15),
            S(-19, -55),   S(-10, -31),   S(  4,  19),   S( -3,  12),   S(-19, -14),   S(-27, -16),   S(-20, -34),   S(-15, -36),
            S( -7, -24),   S(  3, -20),   S( -1, -16),   S(  0,  -4),   S(-18,   0),   S(-12, -17),   S(  3,  -4),   S( -1, -13),
            S( -2,  11),   S( 12,   2),   S( -1,   9),   S( -3,  -9),   S( -5,  -1),   S( -4, -11),   S( -9,  -7),   S( -7, -21),
            S(  1,  18),   S( -1, -25),   S(-12, -18),   S(  5,  13),   S(  3,   1),   S(  0,  -2),   S( -3,   2),   S( -3, -16),
            S(  0,   1),   S( -4,   5),   S( -5,   1),   S(  2,  -4),   S( -1,   5),   S( -2,  -8),   S(  0,   4),   S( -3,  -4),

            /* knights: bucket 9 */
            S(-20, -65),   S( -5,  -3),   S( -5, -33),   S( -4, -34),   S(-15,  -8),   S(-12,  10),   S(  6,  19),   S(  1, -10),
            S( -5,   2),   S(-15, -47),   S(-20, -102),  S(-27, -58),   S(-11, -33),   S(-22, -57),   S(-11,  -2),   S(-12,   0),
            S( -9, -19),   S(-17, -38),   S(-14, -29),   S( -5, -47),   S(-23,  -6),   S( 10,  13),   S(-13,  -3),   S( -3,   2),
            S(-16, -44),   S(-13, -45),   S(-11, -24),   S(-14, -40),   S(-19, -30),   S(  0,   2),   S(-18, -41),   S(  2,   6),
            S(  4,  27),   S(-10, -26),   S( -5, -20),   S( -5, -32),   S(-11, -26),   S( -6,   8),   S( -9, -13),   S( -4,  -1),
            S(-13, -19),   S(-19, -33),   S(-11, -16),   S( -4, -11),   S(  1,  20),   S( -6,   3),   S( -4,  19),   S( -1,   6),
            S(-10, -15),   S( -1,  20),   S(-11,  -2),   S(-21, -16),   S(  1,   3),   S(  3,  22),   S( -8,  13),   S( -6,   0),
            S(  4,   0),   S(  4,   1),   S( -1,  10),   S(  0,   5),   S(-10,  -8),   S( -5,  -1),   S(  3,   7),   S( -1,  11),

            /* knights: bucket 10 */
            S(-10, -33),   S( -6,   9),   S(-10,  -8),   S(-11,  16),   S(-21, -46),   S(  8, -18),   S( -3,  11),   S( -3,  13),
            S( -4, -18),   S(  9,   2),   S(-14, -24),   S(-10, -46),   S( -8, -26),   S(-25, -51),   S( -9,  13),   S(  2,  26),
            S( -3,  -5),   S( -4,  -7),   S( -7, -10),   S(  5, -44),   S(-26, -35),   S( -7, -12),   S(-12, -30),   S(-10,  10),
            S(-10, -19),   S(-11, -21),   S( -9, -11),   S( -8, -18),   S(-13, -19),   S( -8,  -3),   S(-11, -48),   S( -4,  -2),
            S(-12, -22),   S(-12, -27),   S( -9,  -1),   S( -7, -13),   S( -1,  -6),   S(-10, -38),   S( -5, -14),   S(  5,  10),
            S( -2,   7),   S(-12,   0),   S(-10,  10),   S(-13,  21),   S(-14, -13),   S(-19, -12),   S(-13,  -2),   S(-17,  -6),
            S(  3,   8),   S( -2,  -4),   S( -6, -28),   S( 13, -20),   S( -5,   5),   S(-16, -42),   S( -8,   7),   S(-11, -13),
            S( -1,   1),   S( -2,   7),   S( -1,  14),   S( -4,   3),   S( -4,   2),   S( -7, -14),   S(  5,   8),   S(  1,   6),

            /* knights: bucket 11 */
            S( -3, -16),   S(-25, -26),   S( -4,  -6),   S(  4,  20),   S(-38, -33),   S( -1,  11),   S( -7,   6),   S(  8,  31),
            S( -8, -18),   S(-26, -41),   S(-10, -42),   S( 16,   1),   S(  6,  21),   S( -3, -25),   S(-14, -22),   S( -8, -11),
            S(-13, -42),   S(-18, -22),   S( -1, -10),   S( -1,   1),   S( -9,  25),   S( 14,   1),   S( -2, -11),   S( -4,  -2),
            S(-16, -14),   S(  5, -21),   S( -4, -24),   S(  6,   5),   S( 25,   2),   S( -7, -17),   S( 11,  18),   S( -3,  -8),
            S(-15,   0),   S(  2, -41),   S(-20,  -3),   S(  0, -14),   S( 31,  10),   S(  4,  21),   S(-13, -69),   S(-10, -12),
            S( -9, -26),   S( -7, -46),   S(  3,   6),   S(  8,   0),   S(  8,  34),   S( -7,  -7),   S( -3, -21),   S( -2,  21),
            S( -1,  -8),   S( -8,  15),   S(-11, -13),   S(  6,  -6),   S( 13,  -3),   S(  4, -15),   S(  1, -15),   S( -4,   2),
            S( -3, -17),   S(  2,   5),   S( -3, -12),   S(  1,  14),   S( -4, -10),   S( -1, -10),   S(  5,  15),   S( -1,  -5),

            /* knights: bucket 12 */
            S(-14, -41),   S( -3,  -9),   S( -1, -17),   S(  0,   9),   S( -3,   8),   S( -5, -11),   S( -1,   6),   S( -1,   1),
            S( -3,  -8),   S(  0,   2),   S(  0, -12),   S( -3,   9),   S( -4,  -7),   S(  1,   4),   S(  2,   0),   S(  0,  -8),
            S( -2, -10),   S( -6, -20),   S( -6, -19),   S(-14, -21),   S( -8,  -2),   S( -2,  26),   S( -4,   0),   S( -4,  -9),
            S(  3,  10),   S( -1, -32),   S( -7,  27),   S(  2,  17),   S( -4, -12),   S(  3,  22),   S(  5,  12),   S(  2,   8),
            S(  0,   4),   S( -3,  -4),   S( -4, -19),   S( -4,  -9),   S(  1,   5),   S( -3,   5),   S( -6,  -4),   S( -8,  -8),
            S( -4,  -3),   S( -1,  -3),   S( -3, -13),   S( -1,  -8),   S( -3,  -2),   S( -7, -20),   S(  7,   8),   S( -1,   7),
            S( -4,  -9),   S( -2,  -1),   S( -9,  -2),   S( -3,  -8),   S(  0,   9),   S( -8,  -7),   S( -5, -19),   S( -4,  -3),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -7),   S(  1,   2),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -6),   S( -4, -12),   S( -3, -16),   S( -2,  -6),   S( -4, -10),   S( -2,   7),   S( -5,   0),   S(  3,  10),
            S( -2,   9),   S( -2,  -1),   S(  4,  11),   S( -3,  -1),   S( -6,  -7),   S(  0,  12),   S(  2,  22),   S( -4,  -6),
            S(  4,  -2),   S(  5,  10),   S(  5,   5),   S( -4, -21),   S(  4,  24),   S( -5,  10),   S(  6,   4),   S( -3,  -3),
            S(  0,  14),   S(  0,   4),   S( -6,  -1),   S(  0,  28),   S(  0,  11),   S( -3,  26),   S(  0,   6),   S( 10,  19),
            S(  1,  22),   S( -1, -14),   S( -3,  15),   S( -7,   9),   S(-15,   1),   S( -4,  24),   S( -8, -23),   S( -3,  -3),
            S( -3,  -5),   S(  2,   2),   S( -3,   8),   S(  2,  10),   S( -8,   7),   S( -8,   3),   S(  2,  19),   S(  0,   2),
            S(  1,   4),   S(  3,   8),   S( -6,  -5),   S( -5,  -1),   S( -2,   6),   S( -4,  -8),   S(  2,   6),   S( -1,   1),
            S(  2,   6),   S(  0,   2),   S( -2,  -3),   S(  2,   4),   S(  0,   1),   S(  1,   2),   S(  0,  -2),   S(  0,   1),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   4),   S(  5,  18),   S( -2,   0),   S( -6, -23),   S( -1,  19),   S(  2,   2),   S( -1,   3),
            S( -2, -11),   S( -8, -15),   S(  2,  -3),   S( -1,   2),   S(  3,   2),   S(  0,   6),   S( -8,   7),   S(  6,  57),
            S( -1,  -1),   S( -4, -34),   S(  7,  18),   S(-11, -36),   S( -3,   1),   S(  2,  12),   S( -1,  10),   S(  3,  18),
            S(  0,  -4),   S( -4, -19),   S(-22, -11),   S( -2,  44),   S(  1,  42),   S( -4,  -2),   S(  0,   6),   S(  0,  36),
            S(  6,  15),   S(-17, -35),   S( -8,  -7),   S( -8,   5),   S(  0,  35),   S(-10,   4),   S( -3,   0),   S(  3,  12),
            S( -1,   2),   S(  4,   4),   S(  3,  -5),   S( -3,  13),   S(  2,  17),   S(  1,  14),   S(  1,   8),   S( -5, -11),
            S(  0,   4),   S( -2,  -2),   S(  3,  16),   S(  6,   4),   S(  3,  10),   S( -5, -12),   S(  2,   6),   S(  3,   5),
            S(  0,  -1),   S(  0,   1),   S( -1,  -2),   S(  2,   4),   S( -1,   1),   S( -1,  -2),   S(  0,  -1),   S(  1,   2),

            /* knights: bucket 15 */
            S( -3, -14),   S( -1,   3),   S(  4,  23),   S( -2,   7),   S( -3, -15),   S(-10, -35),   S( -4, -15),   S( -2, -12),
            S(  2,  -1),   S(  5,   6),   S( -6,  -7),   S( 10,  45),   S(  1,  19),   S( -8, -31),   S( -3,  -3),   S(  2,   3),
            S(  0,  -5),   S( -5, -21),   S(  2,  -8),   S(  6,  11),   S(-18, -27),   S( -1,  -4),   S( -2,  -5),   S( -1,  -1),
            S(  0,  -9),   S( -3,   3),   S( -6, -16),   S( -5,   7),   S( -8,   7),   S(-10,  26),   S(  4,   7),   S( -1,   0),
            S( -1,  -3),   S(  8,  20),   S( -5,   4),   S( -6,   5),   S( 18,  35),   S(  0,  17),   S(  7,  -1),   S(  4,  19),
            S(  1,   2),   S( -4, -11),   S( -2,   0),   S( -9, -18),   S( -6, -10),   S(  2,  18),   S(  0,   8),   S(  5,  11),
            S( -1,   0),   S( -2,  -7),   S(  4,  15),   S(  3,   3),   S(  4,  13),   S(  5,   7),   S(  1,   7),   S(  3,   8),
            S(  1,   4),   S( -1,  -6),   S(  0,  -1),   S( -1,   0),   S(  2,  10),   S(  1,   1),   S(  0,   0),   S(  1,   2),

            /* bishops: bucket 0 */
            S( 23,  -7),   S( -9,  38),   S(-11,  15),   S(-21,  -8),   S( -1,   1),   S(  3,  12),   S( 66, -40),   S( 19, -16),
            S(-30, -11),   S( -7, -21),   S(-23,  35),   S(  2,  12),   S(  4,  19),   S( 51,  -7),   S( 31,  23),   S( 41, -15),
            S( 14,  10),   S(  6,  24),   S(  6,  -8),   S(  7,  10),   S( 26,  20),   S( 33,  21),   S( 40,   6),   S( 26,   7),
            S( 20, -28),   S( 36, -39),   S( 15,  14),   S( 27,  16),   S( 63,  35),   S( 30,  45),   S( 17,  16),   S(  4,  29),
            S( 34, -11),   S( 47, -21),   S( 59,   4),   S( 75,  42),   S( 86,  22),   S( 24,  42),   S( 32,  43),   S( -7,  15),
            S( 54,  16),   S( 55,  37),   S( 95,   4),   S( 56,  -3),   S( 23,  43),   S( 10,  33),   S( 36,  29),   S( -8,  13),
            S(-45, -78),   S( 68,  35),   S( 88,  82),   S( 21,   2),   S( 21,  -7),   S( 31,  29),   S(-24,  18),   S(-15,  53),
            S(-21, -39),   S( -4,  -8),   S( 14, -25),   S(-14, -15),   S(-10, -15),   S(-17,   8),   S(-16,  17),   S(-32, -36),

            /* bishops: bucket 1 */
            S(-59,  14),   S(  0,  -3),   S(-15,  39),   S( 22,  -8),   S(-13,  21),   S( 12,   5),   S( 41, -12),   S( 24, -33),
            S(  1, -31),   S(-19, -13),   S( -4,  -5),   S(-14,  16),   S( 30,  -6),   S(  7,   3),   S( 47, -35),   S( 13, -10),
            S( -7,   3),   S( 33,  -8),   S(-19,  -9),   S( 19,   7),   S(  7,   0),   S( 31, -28),   S( 19,   1),   S( 63,  -2),
            S( 25, -14),   S( 48, -12),   S( 28,   4),   S( 21,  13),   S( 40,   0),   S( 11,  14),   S( 50,  -2),   S(  0,  18),
            S( 22,  -6),   S( 56, -14),   S( 15,   8),   S( 93, -18),   S( 49,  19),   S( 41,  22),   S( -2,  25),   S( 32,   4),
            S( 65, -42),   S( 49,   8),   S( 52, -24),   S( 57, -10),   S( 74,   4),   S(-39,  12),   S(-21,  53),   S(-30,  19),
            S( 14, -62),   S( -5, -46),   S( -9,   8),   S( 24,  48),   S( 29,  35),   S(-12,  33),   S(-20,   7),   S(-28,  32),
            S(-12, -29),   S(-20,  12),   S( -6, -18),   S(-47,   1),   S(-25,  20),   S( 19,   6),   S( 28,   5),   S(-51,   0),

            /* bishops: bucket 2 */
            S( -1, -21),   S( -6,  -7),   S(  7,  17),   S(-16,   6),   S( 18,  11),   S(-14,   7),   S( 22, -11),   S( -3, -20),
            S( 22, -22),   S(  4, -30),   S( -3,  -6),   S(  7,  14),   S( -9,  11),   S(  8,   5),   S(  0, -31),   S( 18, -53),
            S( 50,   0),   S( 29,   1),   S( -4,  -2),   S( -7,  10),   S(  0,  31),   S( -9, -33),   S( 14, -21),   S( -6,  -6),
            S(-17,   8),   S( 45,  16),   S( -1,  18),   S( 30,  28),   S(  6,  13),   S(  3,  19),   S(-12,   1),   S( 10,  11),
            S(  2,  19),   S(-35,  44),   S( 54,  22),   S( 22,  27),   S( 24,  25),   S( 23,   7),   S( 11,  30),   S( 34,  -5),
            S(-30,  36),   S(  0,  37),   S(-36,  -8),   S( 93,  -2),   S( 47,  13),   S( 87, -15),   S( 72,  13),   S( 40, -43),
            S(-31,  59),   S(-37,   0),   S( -3,  24),   S(  1,  20),   S(-52,  -2),   S(-49,  22),   S(-35,   3),   S( -3, -40),
            S(-83, -22),   S(-11,  27),   S(  0,  11),   S(-19,  31),   S(-29,  -7),   S(-33,  12),   S(-11,  -7),   S(-60, -17),

            /* bishops: bucket 3 */
            S( 35, -19),   S( 41, -19),   S( 26, -23),   S( 17,  -3),   S( 21,  10),   S(  2,  29),   S(-10,  51),   S(  0, -23),
            S( 38,   1),   S( 24, -28),   S( 22,  -1),   S( 24,   4),   S( 21,  16),   S( 23,   8),   S( 11, -20),   S( 31, -40),
            S( 17,   0),   S( 38,  37),   S( 20,   9),   S( 19,  28),   S( 20,  31),   S( 13,  -4),   S( 27,  -8),   S( 18,   8),
            S( -8,  16),   S( 12,  42),   S( 25,  50),   S( 36,  46),   S( 36,  22),   S( 29,   7),   S( 28,  -2),   S( 44, -38),
            S(  5,  31),   S( 17,  49),   S(  9,  57),   S( 53,  46),   S( 47,  46),   S( 52,  21),   S( 31,  15),   S(  5,  11),
            S(  2,  34),   S( 19,  54),   S(  6,  11),   S( 21,  41),   S( 52,  41),   S( 72,  45),   S( 47,  38),   S( 49,  71),
            S(-24,  75),   S( -1,  23),   S( 15,  26),   S(  3,  55),   S( 29,  38),   S( 56,  53),   S(-39,  29),   S( 20, -22),
            S(-41,   8),   S(-24,  49),   S(-46,  40),   S(-30,  53),   S( 14,  17),   S(-59,  37),   S( 16,   7),   S( -2,   6),

            /* bishops: bucket 4 */
            S(-34,   4),   S(-26,   7),   S(-32,  20),   S(-50,  17),   S(-31,  -5),   S(-23,  -1),   S(-10, -18),   S(-41, -36),
            S( -8,   4),   S( -8, -16),   S( 62, -27),   S(-36,  21),   S(-56,  29),   S(-10, -24),   S(-30, -28),   S(-26, -20),
            S(  9,  24),   S( -7, -13),   S(  8,  -5),   S( -2,   6),   S( 13,  -4),   S(-63,   5),   S(-16, -27),   S(-51, -14),
            S( 30,   0),   S( 48, -18),   S( 32,   9),   S( 11,  24),   S(-11,  25),   S( 29,  -2),   S(-44,   7),   S( -7, -21),
            S( 15, -12),   S(-20, -24),   S( 30, -14),   S( 11,   4),   S( -6,  29),   S( 18,   9),   S(-18,  31),   S(-53,   2),
            S(-54, -85),   S(-57, -15),   S(-19,  -4),   S(  2,   7),   S(-43,  48),   S( 10,   1),   S(-14,  24),   S( -6,  27),
            S( -2,   1),   S(-26,   0),   S(  2, -18),   S(-27,  -8),   S(  3, -17),   S( 40,   4),   S( -5, -12),   S( 19,  33),
            S( -8,  -7),   S( -2, -21),   S(-10,  -6),   S(  2, -15),   S(-17,   7),   S(  5,  20),   S(  7,  43),   S(  6,   1),

            /* bishops: bucket 5 */
            S(-44,  -7),   S( 24,  -6),   S(-28,  20),   S(-40,  22),   S( -7,   7),   S(-55,  22),   S(-35,  23),   S(-48, -17),
            S(-11,  -5),   S(-24,  -5),   S( 26,   0),   S(-17,  23),   S(-54,  35),   S(-31,  29),   S(-37,  -1),   S(  6,  -9),
            S(  6,  30),   S(-16,   7),   S( 19, -19),   S(  5,  14),   S( -9,  25),   S(-65,   7),   S(-20,  25),   S(-21,  29),
            S( 12,  13),   S( -4,  17),   S( 58, -12),   S( 28,  15),   S(-14,  27),   S(  4,  22),   S(-65,  38),   S(-20,  23),
            S(  9,   0),   S( 21,   0),   S(-29,  12),   S(-30,   2),   S(-13,   9),   S(-18,  19),   S(  8,  23),   S(-42,  19),
            S(  2,  -3),   S(-51,  22),   S(  0, -25),   S(-32, -23),   S(-29,  13),   S(-20,  -7),   S(-24,  25),   S(-32,  48),
            S(-24,  -6),   S( -8, -11),   S(-17,   2),   S(  4,  29),   S( 15,   9),   S(-12,  34),   S( -1,  10),   S(-17,  35),
            S(-15,  -3),   S(-10, -15),   S(  0, -11),   S(-17,   5),   S(-23,  34),   S(  9,  41),   S(-17,  30),   S( 13,   8),

            /* bishops: bucket 6 */
            S(-17, -32),   S(-15,   9),   S(-27,  22),   S(-11,  14),   S(-48,  33),   S(-22,  19),   S(-38,  32),   S(-61,  -3),
            S(-38,  15),   S(-29, -24),   S(-55,  43),   S(-38,  32),   S(-43,  33),   S(-42,  24),   S(-40,  10),   S(-35,  12),
            S(  3,   7),   S(-39,  25),   S( -3, -11),   S(-33,  36),   S(-25,  42),   S(-32,  -6),   S(-10,  -2),   S(-16,  24),
            S(-61,  31),   S(-59,  35),   S(-23,  24),   S( 15,  37),   S( -1,  37),   S(  2,  19),   S( 14,   8),   S(-17,  25),
            S(-40,  24),   S(-30,  32),   S( -7,  18),   S( 42,  17),   S(-40,  19),   S(-34,  12),   S( -4,  17),   S(-25,   3),
            S(-43,  40),   S(-21,  27),   S(-58,   5),   S(-41,  22),   S( -9,   9),   S(-24, -11),   S(-23,  23),   S(-34,   6),
            S(-14,  37),   S(-72,  33),   S(-30,  24),   S(-18,  30),   S( -8,  11),   S(  2,  11),   S( 10,  -8),   S(-30,  18),
            S(-16,   5),   S(-26,  40),   S( -7,  35),   S( 24,  19),   S(-26,  29),   S( 18,  -9),   S(-11,  14),   S(-13,  14),

            /* bishops: bucket 7 */
            S(-19, -49),   S(-53,  -7),   S(-35, -18),   S(-14,  -9),   S(-34,  -3),   S(-30,  -7),   S(-61, -20),   S(-48, -15),
            S( -5, -47),   S( -7, -46),   S( 15, -20),   S(-26, -10),   S(-33,   4),   S(-38,   4),   S(-35, -31),   S( -9, -15),
            S(-43, -22),   S(-24,   5),   S(-12, -23),   S(  8,  -4),   S(  6,  -2),   S( -5, -36),   S(-52,  10),   S(-62,  14),
            S(-17, -26),   S(-63,  26),   S(-29,  10),   S(-17,  22),   S( 83,  -3),   S(-11,  13),   S( 30, -28),   S(-22,  -3),
            S(-22,  -3),   S( 21, -18),   S(-46,  27),   S(  2,   3),   S( 38,  -5),   S( 33,   8),   S(-48,  11),   S(-38, -13),
            S(-68,  29),   S(-35,  45),   S(-16, -11),   S(-88,  37),   S(-39,  23),   S( -9, -12),   S(-13,  24),   S(-62, -86),
            S( -4,  -6),   S(-24,  -4),   S(-39,  22),   S( -1,  11),   S(  1,   5),   S( 20, -20),   S(  5, -24),   S(  2, -12),
            S(-20, -32),   S(  0,   7),   S( -8,  17),   S( -1,  12),   S(-11,   6),   S( 11, -13),   S( 30, -26),   S( -3,  -7),

            /* bishops: bucket 8 */
            S( 34,  58),   S( -1, -34),   S( -1,   0),   S( -8,  44),   S(  3,  23),   S( -6, -33),   S(-16, -25),   S(-11, -17),
            S(  1,  -1),   S( 15,  28),   S( 22,  11),   S(  8,  25),   S(  2, -10),   S(  4,   2),   S(-33, -47),   S( -9,   0),
            S( -7,  -6),   S(-13, -10),   S( 22,  30),   S( 12,  19),   S(  7,  18),   S( -2,  -2),   S(-25, -14),   S(-34, -28),
            S( -6, -13),   S( 27,  20),   S( -7,  24),   S( 19,   7),   S(  3,  34),   S( 12,  24),   S(-12,   4),   S(  3, -20),
            S( 14,  18),   S( 42,  51),   S( 16,  -5),   S( -9,  18),   S(  9,  22),   S(-23,  18),   S( -9, -30),   S(  5,  17),
            S( -8,  -6),   S( -1,   5),   S(  6,  20),   S( 24,  17),   S( 13,  34),   S( 25,   1),   S( -7,  54),   S( -2,  31),
            S(  3,  15),   S(-16, -42),   S( 29,   2),   S( 25,   5),   S( 11,   2),   S( 24,  50),   S( 19,  26),   S(-12,  -3),
            S( -6,  -4),   S(  5,   3),   S(  2,  16),   S(  4,  12),   S( 30,   6),   S( 24,  13),   S( 16,  42),   S( 36,  27),

            /* bishops: bucket 9 */
            S(  6,  28),   S(  6,  16),   S( -1,   1),   S(-28, -21),   S(-20,  -5),   S( -7,  -2),   S( -2,   1),   S( -9,  -5),
            S(  1,   0),   S(  7, -11),   S(  5,  18),   S(-34,   9),   S(-28,  18),   S(-10,  -5),   S(-36, -12),   S(-16, -29),
            S( -7,   7),   S( 18,   9),   S( -5, -17),   S(  2,  33),   S(  9,  19),   S(-31, -19),   S(  0,  12),   S( -9,  -6),
            S( -2,  28),   S( -7, -11),   S( 22,  -1),   S( 16,  -4),   S( -9,  20),   S(-11,  15),   S(  3,  24),   S( -4,   9),
            S( 25,  17),   S( 14,  12),   S( 21,  17),   S(  7, -26),   S( 10,  22),   S( -3,  30),   S(  4,  32),   S(-14, -20),
            S( 18,  24),   S( -8,  27),   S(  6, -16),   S( 11,  19),   S( 39, -41),   S( -6,  11),   S( 15,  33),   S( 13,  29),
            S( 13,  12),   S(-11,  13),   S( 11,  15),   S( 24,   1),   S( 26,   2),   S( 34,  20),   S( 15,  30),   S( 19,  58),
            S( 11,  37),   S(  2, -23),   S(  4,  21),   S( 13,  15),   S( 11,  39),   S( 20,  -2),   S( 27,   1),   S( 29,  22),

            /* bishops: bucket 10 */
            S( -2, -31),   S( 13,  13),   S( -3, -16),   S(-25, -16),   S(-66, -10),   S(-29, -52),   S(  9,  -2),   S( -4,  13),
            S( -9,  17),   S( -5, -53),   S( -6, -12),   S(-23, -31),   S(-52,  12),   S(-31, -17),   S(-30, -12),   S(  2,   4),
            S( -9, -34),   S(-17, -14),   S(-21, -28),   S( -5,  31),   S(-17,  20),   S(-13, -27),   S( -6,   7),   S( -5, -15),
            S(-16,   9),   S(-24,   2),   S(-29, -27),   S( -1,   1),   S(-30,  44),   S( 22,   8),   S( 29,  26),   S( -6, -30),
            S( 12,   5),   S(-39,  23),   S( -5,   7),   S(  2,  32),   S( 29, -16),   S( 16,  35),   S( 17, -19),   S( 15,  10),
            S(  9,   8),   S(  7,  17),   S(-12,  -2),   S( 25,  13),   S( 14, -16),   S( -2,  -9),   S(  9,   9),   S( 25,  16),
            S( 20,  39),   S( -3,   3),   S( 33, -12),   S( 15,  30),   S(  2,  18),   S( -4, -20),   S(  1, -14),   S( 22,  29),
            S( 12,  26),   S( 21,  32),   S( 44,  18),   S( 10,  20),   S( -2,  21),   S(  7,  12),   S( 14,  17),   S(  1, -13),

            /* bishops: bucket 11 */
            S( 10, -18),   S( -7, -13),   S( -7,  -5),   S(  2,   0),   S(-18, -13),   S( -3,  -1),   S(-21, -24),   S(-11,   2),
            S( -5, -11),   S(  3, -19),   S(-10,  11),   S(  1, -10),   S(-14,  15),   S(-40,  -3),   S(-35, -12),   S( 10,   5),
            S(-10, -49),   S(  0, -18),   S( -9, -37),   S(-30,   8),   S( -8,  -2),   S(  6,  26),   S( -1,  -3),   S( -2, -15),
            S(  2,  -5),   S( -2, -37),   S(  6,  -7),   S(-31, -21),   S(  7,   0),   S( 11,  46),   S( 35,  11),   S( -9, -27),
            S(-12, -17),   S(-14, -17),   S(-36,  33),   S(-28,  32),   S(-26,  31),   S( 34,   5),   S( 22, -16),   S(  8,   6),
            S( -6,   6),   S( -9, -11),   S( -8,  -9),   S(  3,  24),   S( 23,  20),   S(  6, -28),   S(  1, -15),   S( -2, -16),
            S( -2,  -6),   S( 17,  26),   S( 21,  50),   S( 33,  25),   S( 20,  -6),   S( -6,  -3),   S(-17, -29),   S( -7, -12),
            S( 28,  19),   S(  6,   4),   S( 30,  47),   S( 31, -17),   S( 18,  18),   S(  4,   4),   S( -6, -13),   S(  5,  -5),

            /* bishops: bucket 12 */
            S( -5, -11),   S( -4, -13),   S( -5,   0),   S(  8,  20),   S( -9,  -9),   S( -7,  -3),   S(  0,   2),   S( -1,   1),
            S(  0,  -5),   S(  6,   3),   S(  0,  -2),   S(  1,  15),   S(  0,  10),   S(  9,   7),   S(-14, -22),   S( -1,  -5),
            S(  8,   5),   S( 11,  -2),   S( 21,  16),   S( 21,  18),   S(  0,  13),   S( -7,  -9),   S(  2,   5),   S( -5,  -3),
            S( 10,   4),   S( 14,   2),   S( 19,   6),   S( 16,  39),   S( 10,   5),   S(  4,  20),   S(  3,  12),   S(  3,   7),
            S( 11,   9),   S( 10,  10),   S( -3,  16),   S( 21,   7),   S( 19,  25),   S(  8,  28),   S(  7,  10),   S(  4,  12),
            S(  2,   1),   S( -9, -10),   S( -6,  12),   S(  2,  -4),   S( 31,  31),   S( 10,   9),   S( -8,  -7),   S( -4, -10),
            S( -3,  -3),   S(  4,   9),   S(  3,  10),   S(  5,  -6),   S( 13,   1),   S( 21,  25),   S( 12,  25),   S( -1,  -2),
            S(  0,   5),   S( -1,  -4),   S(  0,  -4),   S(  0,  -5),   S(  2,   8),   S(  4, -10),   S( 15,   6),   S(  7,   5),

            /* bishops: bucket 13 */
            S( -5, -17),   S( -1,  -2),   S( -4, -13),   S( -5,  -9),   S( 17,  16),   S( -6, -10),   S(-15, -20),   S( -3,  -4),
            S( -5,  -2),   S( -8, -11),   S(  0,   4),   S( 17,   2),   S( -5, -13),   S(  3,  12),   S( -1,  -6),   S(  1,  -3),
            S(  8, -10),   S( 31,  19),   S( 10,   1),   S( 19,  31),   S(  2,  24),   S(  8,  20),   S( -8,   5),   S( -6,  -4),
            S( 25,  30),   S( 46,  16),   S( 20,  27),   S(-20,   8),   S( 15,  67),   S(  2,  12),   S(  8,   6),   S(  2,   9),
            S( 22,  23),   S( 16,  17),   S( 13,   0),   S(  7,  -9),   S(  9,  -6),   S( 10,  20),   S( 12,  16),   S(  3,  11),
            S(  7,   6),   S(  1,   7),   S( -4, -13),   S( 17,  -5),   S(  8,  14),   S( -5, -19),   S(  2,  -2),   S( 12,   1),
            S(  7,   8),   S(-10, -21),   S( -1, -18),   S(  4,   1),   S(  6,  18),   S( 18,  11),   S(  9,  -2),   S( 10,  13),
            S(  1,  -2),   S( -2,  -2),   S( -1,  11),   S(  2,   9),   S(  7,  13),   S(  3, -11),   S( 13,  -4),   S( 11, -10),

            /* bishops: bucket 14 */
            S(-12, -24),   S(  5,  21),   S( 15,  12),   S(  5,  22),   S(-12,  -1),   S( -7,  -5),   S( -5,   3),   S( -8,  13),
            S( -1,   1),   S( -1,  -5),   S(  2,  12),   S( -2,  -8),   S( 13,   5),   S(  3,  10),   S( -5,  19),   S(  4,  29),
            S(  1,  -4),   S( -2, -13),   S( -8, -14),   S( 19,  33),   S( 22,  47),   S( 11,  23),   S(  6,  39),   S(  3,  30),
            S(  5,  32),   S(  8, -12),   S( -3,   0),   S(  2,  27),   S(  7,  13),   S( 20,   9),   S( 20,  16),   S(  9, -16),
            S( 10,   7),   S(  7,  16),   S( 10,   9),   S( 18,  10),   S( -3,   0),   S(  5,  13),   S( 22,   0),   S( 16,  11),
            S(  2, -11),   S( 23,  37),   S(  3,   7),   S( 14,   5),   S(  9,  -1),   S( -7,   0),   S( -2,  19),   S( 17,   3),
            S( 17,  37),   S(  8,  11),   S( 13,  18),   S(  7,  10),   S(  7,  -3),   S(  3,  10),   S(  0, -11),   S(  2,   0),
            S( 14,   4),   S( 13,  18),   S(  4,   9),   S(  5,   1),   S( -4,  -4),   S(  1,  -5),   S(  7,  10),   S(  3,   3),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -2),   S( -7, -16),   S( -2,  -1),   S( -6, -19),   S( -4,  -8),   S( -5, -13),   S( -3,  -5),
            S(  9,  13),   S( -4, -11),   S(  5,   3),   S(  4,   5),   S(  8,  -1),   S( -1,  -2),   S( -1, -10),   S( -3,  -6),
            S(  3,  -6),   S(  3,   1),   S(  0,  -7),   S( 13,  16),   S( 13,  30),   S(  8,  26),   S( 16,  22),   S(  4,   5),
            S(  1,  -9),   S( 12,  12),   S( 11,  29),   S(-18,  -5),   S(  2,   6),   S( 16,   5),   S( 13,   1),   S(  9,  19),
            S( -1,  -8),   S( -2,  11),   S( -4,  21),   S( 20,  56),   S( 20,  24),   S( 12,  -1),   S(  8,   1),   S( -3,   1),
            S( -2,  18),   S(  6,  11),   S(  4,  25),   S(  7,  13),   S( 23,  20),   S(  7, -13),   S(  3,   8),   S(  1,  -2),
            S(  5,  -2),   S(  2,  18),   S(  9,  31),   S( 15,  18),   S( 10,  15),   S( -2,   7),   S( -1,  -8),   S(  0,   0),
            S(  3,  -3),   S( 11,  14),   S(  8,   0),   S(  9,  10),   S(  5,  16),   S(  1,  -2),   S(  4,  10),   S(  4,  -1),

            /* rooks: bucket 0 */
            S(-21,  11),   S(  6, -13),   S(-14,   0),   S(-10,  15),   S(-28,  58),   S(-18,  34),   S(-48,  61),   S(-54,  44),
            S(  2, -20),   S( -6,  16),   S(-33,  23),   S(  1,  28),   S(  2,  39),   S( -2,  21),   S(-14,   9),   S(-20,  41),
            S( 24, -30),   S( 12, -10),   S(-11,  12),   S( -2,  15),   S(-25,  53),   S( -8,  12),   S(-16,  36),   S( -1,  11),
            S( 13, -17),   S( 35,   0),   S(-35,  34),   S( 19,  16),   S( 19,  40),   S(-10,  38),   S(-19,  46),   S(-14,  28),
            S( 52, -55),   S( 40,   5),   S( 15,  33),   S( 39,  26),   S( 48,  18),   S( 32,  62),   S( 38,  49),   S( 12,  57),
            S( 43, -29),   S( 50,  20),   S( 89, -12),   S(100,  28),   S( 34,  52),   S( 36,  57),   S( 13,  67),   S(-37,  80),
            S( 13,  12),   S( 52,  45),   S( 88,  33),   S( 67,  16),   S( 71,  48),   S( 26,  62),   S( -6,  77),   S(-11,  67),
            S( -7, -26),   S( 26,  21),   S( 19,  19),   S( 41,  -1),   S( 27,  46),   S( 44,  14),   S( 35,  16),   S( 54, -44),

            /* rooks: bucket 1 */
            S(-52,  46),   S(-19,   3),   S(-11,  11),   S(-40,  26),   S(-36,  42),   S(-42,  45),   S(-47,  62),   S(-71,  70),
            S(-37,  32),   S(-23,  -6),   S(-19,  18),   S(-24,  25),   S(-23,  15),   S(-36,  41),   S(-16,  16),   S(-24,  47),
            S(-21,  22),   S( -8,  -5),   S( -8,   3),   S(-23,  17),   S(-24,  19),   S(-42,  33),   S(-54,  60),   S(-17,  54),
            S(-30,  42),   S( -7,  14),   S( -8,  27),   S(-27,  17),   S(-31,  35),   S(-40,  61),   S(-22,  52),   S(-59,  81),
            S(-13,  46),   S( 13,  -2),   S( 28,  17),   S( 24,   8),   S(  4,  29),   S( -1,  79),   S( 11,  60),   S( -5,  81),
            S( 38,  39),   S( 47,   7),   S( 22,  17),   S(-20,  35),   S(  1,  26),   S(  7,  61),   S( 37,  45),   S( 12,  79),
            S( 10,  70),   S( 17,  11),   S(  0,  32),   S(  8,  14),   S( 42,  17),   S(  5,  55),   S( 28,  64),   S( 37,  81),
            S( 42,  -6),   S( -5,   0),   S(-15, -12),   S(-30, -16),   S( 13,  10),   S( 13,  18),   S( 34,  29),   S( 50,  36),

            /* rooks: bucket 2 */
            S(-56,  65),   S(-43,  55),   S(-35,  49),   S(-36,  19),   S(-25,  20),   S(-38,  25),   S(-30,  16),   S(-67,  60),
            S(-42,  56),   S(-41,  47),   S(-38,  52),   S(-42,  33),   S(-45,  38),   S(-38,  19),   S(-19,   5),   S(-45,  39),
            S(-34,  58),   S(-21,  48),   S(-30,  38),   S(-32,  37),   S(-30,  26),   S(-27,  23),   S(-14,  10),   S( -9,  31),
            S(-24,  67),   S(-16,  58),   S(-35,  58),   S(-57,  51),   S(-42,  45),   S(-15,  27),   S( -7,  27),   S(-15,  45),
            S( -5,  82),   S(-14,  78),   S( 11,  66),   S(-17,  48),   S(-36,  59),   S( 31,  27),   S(  3,  47),   S( -2,  69),
            S( 21,  87),   S( 22,  74),   S( 32,  68),   S(-32,  61),   S( 19,  26),   S( 26,  51),   S( 81,  10),   S( 38,  72),
            S( 50,  66),   S( -3,  77),   S( 14,  57),   S( 20,  32),   S(-18,  11),   S( 15,  67),   S(-44,  88),   S( 32,  72),
            S(  8,  46),   S( 11,  51),   S( 14,  39),   S(-56,  37),   S(-68,  10),   S(  2,   8),   S( -6,  28),   S(-16,  57),

            /* rooks: bucket 3 */
            S(-16,  69),   S(-11,  66),   S(-11,  88),   S( -9,  79),   S(  1,  46),   S(  2,  36),   S( 18,  12),   S( -9,   4),
            S(  5,  57),   S( -9,  72),   S( -8,  93),   S(  3,  85),   S(  2,  54),   S( 15,  14),   S( 48, -11),   S( 21,   8),
            S( 18,  54),   S(  0,  79),   S( -3,  79),   S(  1,  86),   S( 20,  42),   S(  6,  35),   S( 40,  13),   S( 35,   9),
            S(  8,  84),   S( -2, 105),   S(-10, 105),   S(  2,  91),   S(  2,  69),   S( 16,  53),   S( 37,  33),   S( 10,  31),
            S(  8, 105),   S( -8, 118),   S( 24, 110),   S( 26, 100),   S( 19,  88),   S( 42,  66),   S( 68,  38),   S( 42,  50),
            S(  7, 125),   S( 26, 108),   S( 38, 115),   S( 53,  96),   S(100,  50),   S(111,  36),   S( 76,  46),   S( 35,  44),
            S( 22, 117),   S( 17, 115),   S( 34, 122),   S( 33, 115),   S( 35,  99),   S( 88,  54),   S( 98,  97),   S(113,  61),
            S(115, -26),   S( 50,  44),   S( 17,  98),   S( 20,  81),   S( 16,  72),   S( 55,  63),   S( 29,  33),   S( 57,  11),

            /* rooks: bucket 4 */
            S(-24, -26),   S( 11, -13),   S(-33,  -4),   S(-36,  18),   S(-41,  10),   S(-29,  42),   S(-29,   0),   S(-74,  34),
            S(-28, -44),   S(-51,   3),   S(-23, -16),   S(  8, -29),   S( 29, -20),   S(  1,   2),   S(-17,  -7),   S(  8,  11),
            S(-17, -14),   S(-38, -16),   S(-39,  -2),   S( -5, -29),   S(-22,  -9),   S(-36,  17),   S(-15,  15),   S(-53,  17),
            S(-70, -26),   S(  8,   8),   S( -2, -14),   S(  7, -13),   S( 42,   3),   S( -2,  15),   S( -7,  -3),   S(-12,  11),
            S(-31, -39),   S( 21, -40),   S( 13,   2),   S( 46, -10),   S( 65,  -4),   S( 57,  25),   S( 19,  10),   S( 22,  23),
            S(-17, -38),   S( 10,  11),   S(  6,  -2),   S( 18,  13),   S( 31,  22),   S( 12,  13),   S( 32,  17),   S( 36,  34),
            S(-29, -19),   S( 30,  26),   S( 35,   0),   S( 49,  -5),   S( 61,  -3),   S( -9,  11),   S( 15, -13),   S( 26,   6),
            S(  7, -22),   S(  2,  17),   S( 25,  -5),   S( 29,  -5),   S( 55,   2),   S( 15,   0),   S(  9,   5),   S(  8,  12),

            /* rooks: bucket 5 */
            S( -2,  20),   S(-10,   6),   S(  4,  -6),   S( 30,  -7),   S(-12,  22),   S(  5,  28),   S(-15,  48),   S(-10,  29),
            S( 14,  -7),   S(-23, -11),   S( 44, -52),   S( 37, -20),   S(-14,   8),   S(-14,  15),   S(-28,  29),   S(  6,  24),
            S(-20,  26),   S( -4,   4),   S(  0, -14),   S( -2,  -9),   S(-21,   5),   S( 46, -10),   S(-31,  35),   S(-12,  18),
            S(-25,  32),   S(-13,  19),   S( 30, -17),   S( 16,   2),   S( 22,   4),   S( -7,  46),   S( 15,  32),   S( 10,  46),
            S( 32,  24),   S(  0,  18),   S( -5,  21),   S( -7,  -6),   S(-26,  27),   S( 62,  15),   S( 30,  33),   S( 53,  34),
            S( -8,  35),   S(-13,  19),   S(  1,   5),   S(-16, -14),   S( 15,  18),   S( 13,  31),   S( 60,  15),   S( 53,  29),
            S( 34,  13),   S( 23,   5),   S(-15,   7),   S( 26,  12),   S( 44,   5),   S( 45,  -6),   S( 82, -10),   S( 43,  17),
            S( 31,  31),   S( 14,  14),   S( 44,  -1),   S(  0,  19),   S( 42,  23),   S( 31,  33),   S( 43,  37),   S( 69,  38),

            /* rooks: bucket 6 */
            S(-23,  38),   S( -9,  26),   S(-11,  25),   S(-29,  24),   S( -5,  10),   S( 13,  -3),   S( 23,  -7),   S(-18,  18),
            S(-13,  17),   S( 30,   0),   S( 15,   7),   S( -1,   5),   S( 16, -14),   S(-14,  -3),   S(-27,   1),   S(  7,  11),
            S(-31,  33),   S( 24,  11),   S( 24,   5),   S( -1,  11),   S(-16,  12),   S( 42, -11),   S( -2,  -8),   S(  8,   0),
            S(-26,  54),   S(  3,  39),   S( 18,  22),   S( 33,   7),   S(  7,   3),   S(  8,  10),   S( -8,  12),   S( 14,  37),
            S( -1,  52),   S( 60,  25),   S( 84,  24),   S( 37,   9),   S(  1,  -1),   S( 13,  19),   S( 40,   3),   S( 74,  11),
            S( 86,   9),   S( 87,  -3),   S( 76,   3),   S( 30,  -8),   S( -1, -16),   S( 19,  32),   S( 27,   2),   S( 53,  19),
            S( 53,  15),   S(119, -18),   S( 94, -16),   S( 71, -21),   S( 12,  -7),   S( 38,   5),   S( 44,   2),   S( 66,  -9),
            S( 92,  -9),   S( 66,  12),   S( 15,  34),   S( 58,   3),   S( 46,   9),   S( 24,  31),   S( 75,  16),   S( 73,  22),

            /* rooks: bucket 7 */
            S(-83,  28),   S(-64,  27),   S(-54,  27),   S(-45,  25),   S(-26,  -3),   S(-30, -17),   S(-36,   6),   S(-74, -15),
            S(-65,  25),   S(-16,  -1),   S(-33,  10),   S(-42,  21),   S(-21, -14),   S(-23, -11),   S(  2,  -3),   S(-10, -56),
            S(-65,  28),   S(-52,  23),   S(-13,   0),   S(-16,  10),   S(-32,   3),   S(-27,  13),   S( 41, -29),   S( -5, -45),
            S(-62,  34),   S( -5,  13),   S(  7,  10),   S( 71, -25),   S(  4,   3),   S( 47, -18),   S( 38,  -1),   S(  1, -15),
            S( 12,  23),   S( 39,  18),   S( 71,   5),   S( 92, -16),   S(134, -44),   S( 90, -47),   S( 71, -18),   S(-72, -34),
            S( 39,  11),   S( 39,  -3),   S(101,  -9),   S( 91, -26),   S( 72,  -9),   S( 32,  11),   S( 20,  33),   S( -9, -31),
            S( 10,  -2),   S( 41, -15),   S( 74, -16),   S(111, -44),   S(109, -39),   S( 93, -35),   S( 38,   9),   S( -1, -25),
            S(-20, -15),   S( 14,   2),   S( 41,  -2),   S( 36,   0),   S( 49, -10),   S( 54,  -3),   S( 22,  17),   S(  7,  -9),

            /* rooks: bucket 8 */
            S(-23, -78),   S(-18, -38),   S(-13, -11),   S( 19,   7),   S(-24, -31),   S(-20,  -1),   S(-10, -35),   S(-20,   4),
            S(-34, -77),   S(-16, -43),   S(-25,   6),   S(-28, -62),   S(-26, -37),   S(-16, -19),   S(-10,  -6),   S(-38, -34),
            S( -1, -14),   S( -5, -13),   S(  9,  -2),   S(-12,  19),   S( -8,  48),   S( 12,  27),   S(  2,  47),   S(-18,   5),
            S( -6, -22),   S( -2,   3),   S( -1,   1),   S( 15,  27),   S(  2,  40),   S( 32,  42),   S(  0,  19),   S( -9, -12),
            S(-11, -38),   S( 11,  25),   S(  9,  22),   S( 17,  41),   S(  8,  24),   S( -3,   5),   S( 12,  44),   S(  1,  21),
            S(-27,  11),   S(  3,  14),   S(-15,  12),   S( -5, -14),   S(  5,  32),   S(-15,  28),   S(  0,  -1),   S(  3,  19),
            S(  1,  34),   S(  4,  24),   S(  4,   6),   S( 22,  12),   S( 13,   4),   S( 10,  22),   S(  6,  15),   S(  2,  35),
            S(-14,  14),   S(  3,  12),   S(-18,  33),   S( 36,  49),   S( -6,  15),   S( 11,  33),   S(  2,  16),   S(  7,  32),

            /* rooks: bucket 9 */
            S(-27, -63),   S(-13, -61),   S(-12, -98),   S(-20, -43),   S(-21, -42),   S(  3, -29),   S( -5, -22),   S( -3, -32),
            S(-58, -41),   S(-33, -64),   S(-30, -61),   S(-45, -43),   S(-41, -45),   S(-26,  11),   S(-23, -51),   S(-31, -28),
            S(-15,  -9),   S(-28, -11),   S( -2,  -9),   S(-11, -34),   S(-14, -12),   S(  2,  22),   S(  0,   6),   S(  2,  14),
            S( -4,   7),   S(  3,  -1),   S(  1,   2),   S( -2,   6),   S(-14, -28),   S(  3,   3),   S( -8,  -4),   S(  5, -26),
            S( -3,   4),   S( -9,  -5),   S( -8, -45),   S( -9,   6),   S(-22, -12),   S(-12,   7),   S(-13, -15),   S( -6, -11),
            S( -4,   6),   S(-29, -11),   S(-15, -19),   S( -3,  19),   S( -5,   2),   S( -4,  12),   S( -3,   0),   S(-11,   8),
            S( 10,  25),   S(  7,   1),   S(  3, -38),   S(  1,   8),   S(  8, -17),   S( 22,   1),   S(  7,   2),   S( -2, -21),
            S(-12,   7),   S(-17,  26),   S( -6,  11),   S( -4,  29),   S( -7,  28),   S(  7,  49),   S(  5,  10),   S( 15,  16),

            /* rooks: bucket 10 */
            S(-18, -29),   S(-53, -10),   S(-31, -37),   S(-13, -47),   S(-21, -45),   S( -3, -78),   S(  1, -62),   S(-14, -37),
            S(-44,  -8),   S(-31, -30),   S(-42, -19),   S(-46, -42),   S(-46, -42),   S(-28, -42),   S(-16, -27),   S(-46, -69),
            S(-11, -13),   S(-27, -16),   S(-33, -15),   S(-44, -42),   S(-15, -18),   S( -1, -15),   S(-14, -27),   S(-18, -13),
            S(-24, -11),   S(-34, -35),   S( -6, -34),   S(-10,   6),   S(  3,   4),   S(  3,  11),   S(-10, -29),   S( -1, -34),
            S( 10,  -8),   S(  5, -12),   S(-14, -11),   S(-14, -29),   S(  5,  14),   S( -6,   7),   S(-10, -18),   S(-10, -31),
            S( -7,   3),   S( 15,  -1),   S(  0, -13),   S( -2, -25),   S( -1,  -6),   S( -7,  -6),   S(-20, -27),   S(  2, -14),
            S( -7, -19),   S( 11, -36),   S(  1, -24),   S( -3, -15),   S( 12, -22),   S( -7, -13),   S(-14, -31),   S( -3, -19),
            S( -3, -11),   S( 11,  17),   S(  4,  30),   S(-10,  12),   S( -8,  29),   S(-25,   2),   S(-27,  12),   S(  0,   8),

            /* rooks: bucket 11 */
            S(-60, -16),   S(-38,  -3),   S(-48, -12),   S(-27,  -9),   S(-46, -11),   S(-28, -13),   S(-18, -34),   S(-40, -63),
            S(-18, -11),   S(-25, -18),   S(-57, -10),   S(-55, -18),   S(-16, -19),   S(-17,  -6),   S(-27, -26),   S(-47, -60),
            S(-33,  25),   S(-23,  12),   S( -7,  30),   S(-22,  17),   S(  2, -20),   S(-12,  -2),   S(  4, -19),   S(-15,  10),
            S(-24,  -7),   S(-12, -17),   S(-11,  11),   S(  7,  16),   S( 19,  14),   S(-21, -30),   S(  6,  17),   S( -8, -21),
            S( -5,  -9),   S(  9,  -5),   S(  5,   6),   S(  4,   7),   S( 35,  -6),   S( -3,   0),   S( 16,  38),   S(-16, -41),
            S(  6, -20),   S( -9,  -8),   S( 16, -13),   S( 21,  -5),   S(-10, -14),   S(  1,   7),   S(  4,  34),   S( -6,  -7),
            S( -3,  -1),   S(-19, -36),   S( -2,  -8),   S(  0,  -5),   S( 10,  -4),   S(  2,   8),   S(  2,  15),   S(-12,  -7),
            S( -6,  -3),   S( 17,  24),   S(  4,  18),   S( 19,  17),   S( -8,   4),   S( -1,  23),   S( 14,  16),   S(-20,  24),

            /* rooks: bucket 12 */
            S(-35, -97),   S( -9, -13),   S(-21, -55),   S(-19, -37),   S(-12, -24),   S(  8,  -9),   S(-16, -37),   S(-19, -39),
            S(  2,   1),   S(  1,   4),   S(  8,  20),   S(  3,  13),   S(  7,   8),   S(  8,  -6),   S(  6,  10),   S(-18, -24),
            S( -6, -12),   S(  7,  35),   S( 10,  23),   S( 26,  25),   S(  7,  -5),   S( 15,  27),   S(  7,  33),   S( -3,  27),
            S(  6,  23),   S(  8,   4),   S( 14,  33),   S( 11,  22),   S( 12,   8),   S(  6,   8),   S(  6,  19),   S( -3,   5),
            S( 12,  17),   S( 14,  31),   S(  8,  47),   S(  3,  -1),   S(  8,  24),   S( -3, -16),   S(  5,  13),   S(  5,  11),
            S( -2,   0),   S( -4,  -5),   S(  0,  17),   S( -5,   2),   S(  7,  21),   S( -1, -23),   S( 10,  22),   S(  5,   7),
            S(-16, -10),   S(-11,  20),   S(  8,  38),   S( -1,  19),   S( -3,  -2),   S( 12,  14),   S(  3,  20),   S(  0,  20),
            S(  4,   3),   S( -8,  30),   S(  5,  31),   S( 13,  21),   S(  2,   5),   S(  1,  17),   S(  2,   9),   S(  2,  12),

            /* rooks: bucket 13 */
            S(-27, -20),   S(-28, -50),   S(-26, -51),   S(-19, -36),   S(-30, -49),   S( -4,  -1),   S(-27, -47),   S(-23, -36),
            S(-15,  -9),   S( -8, -18),   S(  2,   7),   S( -2,  -3),   S( 17,  37),   S(  3,  14),   S(  8,   3),   S(-11, -12),
            S(-12,   1),   S(-13,   6),   S( -4,  -7),   S(  6,  10),   S(  6,  28),   S( 13,   0),   S( 12,  45),   S(-13, -25),
            S(  9,  17),   S( -1,   8),   S( -3,   9),   S(  5,  19),   S(  9,  23),   S(  0,   8),   S(  5,  14),   S(  2,  22),
            S(  7,  18),   S(  3,  -8),   S( -5, -22),   S(  3,   5),   S( -3,  24),   S(  1,  -4),   S(  5,   5),   S( -1,  -4),
            S(  0,  11),   S( -4,  -5),   S(-10, -10),   S(-14,  -3),   S(-12, -14),   S(  2,  -5),   S( -8,   3),   S(  0,   0),
            S(  4, -13),   S(  8,   5),   S( -9, -31),   S(  3,  16),   S( -8,  -4),   S(  6,   8),   S(  1,   1),   S(  0, -16),
            S(  3,  17),   S(-10,  13),   S( -5,   4),   S(  9,  23),   S( -2,  12),   S(  7,  21),   S(  0,  20),   S(  3,  -1),

            /* rooks: bucket 14 */
            S( -6, -23),   S(-32, -26),   S(-19, -15),   S(-21, -53),   S(-14, -38),   S( -8, -22),   S(-33, -55),   S(-26, -30),
            S( -7,  27),   S(  4,  28),   S(  6,  12),   S(  0, -17),   S(  0,  -7),   S( -3,  -3),   S( -2,   5),   S( -5,  -2),
            S(  5,  31),   S( -2,  30),   S(  1,   5),   S(  2,   5),   S(  3,   8),   S( -1,  -3),   S(  2,  26),   S(-19, -43),
            S( -4,  14),   S( 15,  22),   S(  7,  21),   S( 10,   7),   S( -9,  -5),   S(  1,  -9),   S(  8,  14),   S(-12, -16),
            S(  9,  14),   S( 19,  18),   S( -2,  -5),   S(  2,   6),   S(  3, -13),   S( 17,  29),   S(  0,  -1),   S( -3, -18),
            S(  6,   8),   S(  6,   9),   S(  7,  16),   S(  2,   5),   S( -3,   6),   S(-15,   4),   S( -9,  -8),   S( -6,  -8),
            S( -7, -14),   S(  8,  12),   S( -8, -21),   S(-17, -34),   S( -5,   6),   S(  1,  -2),   S(-13, -13),   S( -8,  -8),
            S( -1,  -7),   S(  4,   4),   S( -4, -14),   S(  6,  -8),   S(-11, -16),   S(-15, -43),   S(  3,  -5),   S(  1,  28),

            /* rooks: bucket 15 */
            S(-25, -44),   S(-16, -46),   S(-40, -47),   S(-25, -48),   S( -3, -22),   S(-14, -19),   S( -3,  -7),   S(-21, -50),
            S(  6,  32),   S(-11,   1),   S(-12,  -6),   S( -6,  -8),   S( -6, -18),   S(  4,   0),   S(  7,  11),   S(  3,   5),
            S(  6,  10),   S( -6, -13),   S( 12,  25),   S(  9,   1),   S(  6,   0),   S( -4, -14),   S(  6,  25),   S(  2,   6),
            S(  3,  10),   S( -1,  -5),   S( 18,  34),   S( -4, -10),   S(  4,  18),   S(  1,  10),   S(  7,  18),   S(  3, -10),
            S(  6,  13),   S(  5,   6),   S(  7,  -9),   S(  2,  10),   S(  6,  13),   S(  4,   3),   S( -1,  28),   S(  4, -10),
            S(  6,  14),   S(  7,  -3),   S(  8,  -2),   S(  4,   2),   S( -5, -14),   S( -4,  38),   S(  2,  25),   S(  4,   3),
            S(  4,  -6),   S( -3,   2),   S(  8,  16),   S(  4,   9),   S(  2,  15),   S(  5,  14),   S(-13,  13),   S( -8, -28),
            S(  1,  20),   S( -1,  23),   S(  8,  20),   S(  1,  24),   S(  0,   3),   S( -6, -22),   S( -4,  16),   S(-14, -10),

            /* queens: bucket 0 */
            S( -1,  -7),   S(-22, -46),   S(-31, -54),   S(  0, -98),   S( -6, -53),   S( 13, -60),   S(-52, -27),   S(-13,  -9),
            S(-14, -29),   S( 16, -76),   S(  4, -68),   S( -8, -18),   S(  4, -18),   S( -4, -35),   S(-22, -26),   S(-33, -10),
            S( -1,   6),   S( -1, -21),   S( 29, -50),   S( -9,   8),   S( -3,  24),   S(  2,   0),   S(-29,   0),   S(-73, -40),
            S(-17,  20),   S( 18, -22),   S( -8,  21),   S( -8,  70),   S( -4,  64),   S(-18,  38),   S(-38,  30),   S(-16, -24),
            S(-22, -21),   S(  4,  64),   S(  2,  33),   S( -1,  49),   S(  6,  71),   S(-15, 108),   S(-54,  71),   S(-41,   4),
            S(-15,   5),   S( 16,  33),   S( 13,  37),   S(-17,  71),   S(-15,  67),   S(-54,  98),   S(-60,  27),   S(-42,   8),
            S(  0,   0),   S(  0,   0),   S( 17,   2),   S(-31,  33),   S(-29,  29),   S(-58,  85),   S(-82,  65),   S(-95,  26),
            S(  0,   0),   S(  0,   0),   S(  2, -11),   S(-18, -13),   S(-30,  25),   S(-31,  10),   S(-47,  -2),   S(-58, -23),

            /* queens: bucket 1 */
            S( 22,   0),   S( 12,   0),   S( 17, -45),   S( 31, -85),   S( 39, -43),   S( 17, -25),   S( 18,  -4),   S(  5,  17),
            S(-17,  35),   S( 27,  17),   S( 40, -33),   S( 30,   6),   S( 43,  14),   S(  7,  20),   S(-14,  40),   S(-13,  10),
            S( 51,  -2),   S( 29,   3),   S( 22,  32),   S( 17,  74),   S( -1,  80),   S( 38,  45),   S(  1,  40),   S( 21,  -9),
            S( 46,   1),   S( 20,  42),   S( 23,  49),   S( 41,  69),   S( 22,  83),   S( 11,  61),   S( 11,  41),   S( -5,  59),
            S( 48,   1),   S( 55,  12),   S( 51,  34),   S( 21,  31),   S( 48,  65),   S( 33,  32),   S( -6,  76),   S(  8,  91),
            S( 63,  -2),   S( 95,  11),   S( 85,  43),   S( 71,  53),   S( 43,  39),   S( 19,  64),   S( 45,  55),   S(  1,  58),
            S(102, -26),   S( 50, -21),   S(  0,   0),   S(  0,   0),   S( -1,  39),   S(-11,  20),   S( -9,  57),   S(-36,  38),
            S( 70, -13),   S( 41, -21),   S(  0,   0),   S(  0,   0),   S( 10,  16),   S( 31,  21),   S( 78,   0),   S(-15,  36),

            /* queens: bucket 2 */
            S( 39, -12),   S( 32,  12),   S( 34,  20),   S( 45, -26),   S( 47, -30),   S( 34, -20),   S(  4, -19),   S( 38,  32),
            S( 27,   4),   S( 13,  51),   S( 39,  27),   S( 43,  36),   S( 54,   9),   S( 23,  28),   S( 27,  20),   S( 20,  48),
            S( 45,   8),   S( 31,  44),   S( 23, 104),   S( 17,  83),   S( 27,  81),   S( 29,  73),   S( 37,  48),   S( 35,  60),
            S(  3,  75),   S( 25,  84),   S( 27,  81),   S( 16, 122),   S( 35,  93),   S( 26,  93),   S( 38,  63),   S( 37,  83),
            S(  7,  86),   S( -9,  85),   S(  5,  97),   S( 38,  73),   S( 26,  91),   S( 94,  38),   S( 71,  54),   S( 65,  60),
            S(-11,  87),   S(  0,  82),   S(  2,  80),   S( 69,  34),   S( 29,  53),   S( 96,  69),   S(101,  42),   S( 45, 104),
            S(  1,  51),   S( -1,  40),   S( -5,  67),   S( 42,  28),   S(  0,   0),   S(  0,   0),   S( 27,  69),   S( 38,  67),
            S(  1,  35),   S( 33,  -1),   S( 38, -10),   S( 16,  35),   S(  0,   0),   S(  0,   0),   S( 38,  29),   S(  7,  50),

            /* queens: bucket 3 */
            S(-42,  32),   S(-27,  38),   S(-21,  36),   S(-13,  43),   S(-26,  32),   S(-12, -18),   S(-12, -39),   S(-35,  19),
            S(-56,  55),   S(-36,  48),   S(-22,  65),   S(-15,  83),   S(-14,  73),   S(-14,  36),   S( 18, -13),   S( 16, -27),
            S(-48,  77),   S(-37,  89),   S(-29, 113),   S(-38, 144),   S(-26, 124),   S(-19,  94),   S( -5,  55),   S( -7,  19),
            S(-41,  82),   S(-57, 142),   S(-49, 163),   S(-33, 174),   S(-34, 165),   S(-18, 101),   S( -1,  80),   S(-11,  64),
            S(-55, 121),   S(-45, 156),   S(-46, 177),   S(-40, 194),   S(-20, 158),   S(  1, 131),   S(-13, 123),   S(-14,  72),
            S(-63, 114),   S(-58, 159),   S(-55, 182),   S(-50, 194),   S(-48, 169),   S( -4, 106),   S(-26, 123),   S(-24, 104),
            S(-95, 130),   S(-92, 151),   S(-72, 186),   S(-59, 160),   S(-67, 162),   S(-16,  80),   S(  0,   0),   S(  0,   0),
            S(-123, 142),  S(-76, 104),   S(-62, 104),   S(-64, 111),   S(-62, 102),   S(-24,  56),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-30,  -2),   S(-43, -34),   S( -6,   1),   S( -7, -17),   S( -6,  -6),   S( -6,  12),   S(-32, -24),   S( 13,  21),
            S( -1, -10),   S( -8,   6),   S( -4,   1),   S(-10, -13),   S(-39,  18),   S(-16,  12),   S(-44, -10),   S( -2, -17),
            S(  6,  17),   S( 20, -30),   S( 12, -17),   S( 15,   8),   S( 45,  10),   S( 19,  20),   S(-14, -17),   S( 34,  24),
            S(-17, -24),   S( 14, -21),   S(  0,  -1),   S(-10,  17),   S( 43,  31),   S(  8,  58),   S(-24,   6),   S(-14,  17),
            S(  0,   0),   S(  0,   0),   S( 13,  -9),   S( 50,  34),   S( 24,  57),   S( 33,  52),   S( 13,  16),   S( 15,  22),
            S(  0,   0),   S(  0,   0),   S( 15,  10),   S( 29,  17),   S( 42,  48),   S( 32,  49),   S( 22,  26),   S(  2,   6),
            S( 11,  -6),   S( 18,   8),   S( 59,  37),   S( 52,  35),   S( 54,  16),   S( 23,  31),   S(  7,  25),   S( -9,  23),
            S( 16, -12),   S(-20, -33),   S( 20,   6),   S( 46,  22),   S( 16,   9),   S(  9,  24),   S(  2,   5),   S( 21,   8),

            /* queens: bucket 5 */
            S( 38,  25),   S( 28,  12),   S( 20,   9),   S(  1,  27),   S( 38,  -5),   S( 42,  48),   S( 10,  -1),   S( 21,   3),
            S( 23,  18),   S( 17,   1),   S( 15,  -1),   S( 15,  15),   S( 14,  43),   S( -6,  -8),   S( 29,  15),   S( 10,   5),
            S( 20,   5),   S( 43,  -3),   S( 22,  -1),   S(  4,  16),   S( 18,   8),   S( 32,  19),   S( 28,  42),   S( 20,  17),
            S(  6, -30),   S( 31,   1),   S( 18, -20),   S( 25,  10),   S( 55,   3),   S( 32,  15),   S( 36,  49),   S(  6,  32),
            S( 36,  -9),   S( 20, -42),   S(  0,   0),   S(  0,   0),   S(  1,   6),   S( 26,  14),   S( 42,  56),   S( 16,  36),
            S( 32,  13),   S( 31,   3),   S(  0,   0),   S(  0,   0),   S( 28,  19),   S( 58,  32),   S( 46,  38),   S( 53,  41),
            S( 60,   1),   S( 63,   6),   S( 50,  39),   S( 25,  26),   S( 41,  15),   S( 83,  43),   S( 61,  58),   S( 52,  33),
            S( 40,  29),   S( 48,  11),   S( 58,  17),   S( 38,  -5),   S( 53,  21),   S( 64,  40),   S( 69,  50),   S( 60,  34),

            /* queens: bucket 6 */
            S( 52,  52),   S(  5,   8),   S( 42,  19),   S( 36,  24),   S( 29,  15),   S(  0,   3),   S(  4,  13),   S(  8,  19),
            S( 32,  21),   S( 36,  30),   S( 72,  41),   S( 60,  29),   S( 42,  23),   S( 19,  17),   S(-12,  29),   S( 26,  33),
            S( -2,  49),   S( 45,  38),   S( 32,  39),   S( 52,  14),   S( 27,  11),   S( 44,   1),   S( 57,  27),   S( 69,  61),
            S( 28,  37),   S(  9,  31),   S( 49,  14),   S( 84,  14),   S( 30, -11),   S( 35,   7),   S( 71,   4),   S( 95,  45),
            S( 33,  55),   S( 34,  38),   S( 51,  37),   S( 42,  28),   S(  0,   0),   S(  0,   0),   S( 58,  18),   S( 98,  49),
            S( 44,  50),   S( 58,  46),   S( 47,  55),   S( 24,   6),   S(  0,   0),   S(  0,   0),   S( 68,  41),   S(103,  42),
            S( 59,  37),   S( 19,  29),   S( 61,  15),   S( 50,  15),   S( 41,  37),   S( 66,  48),   S(119,  22),   S(126,   7),
            S( 41,  43),   S( 66,  28),   S( 72,  20),   S( 77,  37),   S( 94,   8),   S( 92,  12),   S(102,  13),   S( 97,  31),

            /* queens: bucket 7 */
            S( -6,  26),   S( -5,   2),   S(-19,  23),   S(  0,  24),   S( 16,   3),   S(-10,   7),   S(  0,  15),   S(-14,  -7),
            S( -3,  25),   S(-40,  28),   S(  4,  48),   S( -6,  75),   S( -7,  42),   S(  6,  25),   S(  9,   3),   S(-27,  -5),
            S(  6,  25),   S( -8,  35),   S(-10,  88),   S( 43,  44),   S( 46,  32),   S( 20,  14),   S( 44, -24),   S( 46,  -6),
            S(-11,  22),   S( 23,  43),   S( 21,  70),   S( 46,  73),   S( 76,  47),   S( 58,  -3),   S( 69, -32),   S( 31,  -8),
            S( 19,  24),   S( -6,  58),   S( 22, 102),   S( 51,  82),   S( 79,  21),   S( 60,  -5),   S(  0,   0),   S(  0,   0),
            S(  5,  46),   S( -5,  88),   S( 16,  88),   S(  0,  87),   S( 53,  37),   S( 84,  48),   S(  0,   0),   S(  0,   0),
            S(-33,  62),   S(-19,  43),   S( 13,  59),   S( 34,  61),   S( 50,  41),   S( 66,  19),   S( 66,  22),   S( 57,  25),
            S( 35,  19),   S( 43,  33),   S( 50,  59),   S( 47,  26),   S( 50,  44),   S( 21,   6),   S(-19,   7),   S( 52,  -9),

            /* queens: bucket 8 */
            S(-19, -37),   S(  1, -22),   S(-16, -42),   S( -3,  -7),   S(-17, -30),   S(  9,  -3),   S( -1, -12),   S(  1,   4),
            S(-21, -36),   S( -6, -15),   S(  2, -14),   S( -6, -10),   S(  9,  -2),   S( -4, -10),   S( -3,   2),   S( -1,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -16),   S(-11, -46),   S(  7,   3),   S(  6,  -7),   S( -7,  -8),   S(  3,   5),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S( -4, -13),   S( -2,   0),   S(  4,  -1),   S( 12,  18),   S(  7,   3),
            S( -3, -12),   S(  8,  12),   S(  8,  -1),   S( 10,  -5),   S(  8, -11),   S( 13,  12),   S( 13,  11),   S(-10, -10),
            S(  1, -13),   S(  4, -16),   S( 15,  12),   S(  3, -19),   S( 13,   8),   S( 28,  35),   S(  8,  -5),   S( -2,  -5),
            S(-16, -36),   S(  2, -10),   S( 14,  11),   S( 25,  39),   S( 12,  12),   S( 18,  41),   S(  4,   7),   S(  5,   0),
            S(  3,   0),   S(  5,  -5),   S( 15,  10),   S(  9,  -2),   S( 18,  19),   S( -2,  -4),   S(  4,  10),   S(-16, -28),

            /* queens: bucket 9 */
            S(  9,  -9),   S(-19, -33),   S(-16, -36),   S( 11, -11),   S( -8, -36),   S( -2,  -8),   S( -4,  -9),   S( -1, -14),
            S( -2,  -7),   S(-13, -22),   S(-11, -27),   S(  0, -16),   S(-25, -53),   S(-13, -31),   S(  7,  -2),   S(  1,  -9),
            S(-17, -44),   S(-14, -26),   S(  0,   0),   S(  0,   0),   S(  4,  -9),   S(  8, -10),   S( -6,  -9),   S(  5,  -4),
            S(  1,  -8),   S(-12, -32),   S(  0,   0),   S(  0,   0),   S( -1,  -5),   S(  9,   0),   S( 10,   9),   S( -3,   2),
            S( -8, -25),   S(  0, -14),   S( -1,  -6),   S(-11, -10),   S( -6, -28),   S( 11,  17),   S(  6,  -7),   S(  0, -15),
            S( 11,  11),   S( -2, -26),   S(  3, -10),   S( -3, -18),   S( -1, -11),   S(  6,   6),   S( -1, -10),   S( -2, -13),
            S(  9,   7),   S(  9,  -4),   S( -5,  -2),   S(  1,   9),   S( 25,  28),   S( 25,  29),   S(  8,  21),   S(  7, -11),
            S( 17, -10),   S( 27,  19),   S( -1,  -7),   S( 20,  14),   S( 21,  18),   S(  7,  15),   S(  1, -18),   S( 14,   2),

            /* queens: bucket 10 */
            S( 16,  10),   S( 13,   9),   S( -1,  -9),   S( -6, -26),   S(-10, -30),   S(-10, -20),   S( -4, -27),   S( -5, -14),
            S(  6,   4),   S(-13, -20),   S( -8, -25),   S(-19, -52),   S( -5, -11),   S( 10,  -1),   S(-12, -27),   S( -6,  -6),
            S( -1,   1),   S(  2,   3),   S( -3,  -5),   S( -8, -19),   S(  0,   0),   S(  0,   0),   S(  1,  -7),   S(-13, -23),
            S( -4, -10),   S(  3,   4),   S(  3,   3),   S(  8,   1),   S(  0,   0),   S(  0,   0),   S( -6, -15),   S( -1, -17),
            S( 12,  16),   S( 15,   6),   S(  3,  -4),   S( 28,  31),   S( -1,   1),   S( -2,  -1),   S(  1, -11),   S( 10, -26),
            S( -5,  -8),   S(  6,   7),   S( 24,  29),   S( 11,  13),   S( 14,  14),   S( 15,  22),   S( 16,  10),   S( -3, -22),
            S(  8,   8),   S( 18,  25),   S( 19,  26),   S( 23,  20),   S( 11,  16),   S( 24,  12),   S( 15,  10),   S(  6,  -5),
            S(-11, -31),   S(  5,   6),   S( 22,   7),   S( -5,   1),   S( 14,  14),   S(  3,   1),   S( 15,  10),   S( 10,  -7),

            /* queens: bucket 11 */
            S(-10,  -3),   S( -3,  -1),   S( -7, -10),   S(-18, -20),   S( -5, -13),   S(-23, -34),   S( -8, -32),   S(-10, -16),
            S( -5,  -1),   S(  1,   8),   S(-22, -10),   S( -7,   4),   S( 20,  -1),   S(-11, -27),   S(  6,  -5),   S( -6, -12),
            S(  3,   7),   S(  5,   0),   S(-20,  12),   S( -2,   2),   S( -5, -21),   S(-25, -31),   S(  0,   0),   S(  0,   0),
            S( -1,   1),   S( -7,   9),   S( -2,  11),   S(  0,   3),   S(  0,  -9),   S( -3,   6),   S(  0,   0),   S(  0,   0),
            S(  2,  11),   S( 16,  15),   S( 17,  24),   S(  4,  22),   S( 41,  47),   S( 16,  27),   S(  9,   0),   S(-11, -29),
            S(  2,   4),   S(  1,   0),   S(  0,  14),   S( 12,  28),   S( 15,  20),   S(  2,   5),   S(  5,  -9),   S(  3, -24),
            S(  4,   4),   S( 10,  12),   S( 16,  25),   S(  2,  11),   S( 20,  59),   S( 16,  13),   S(  7,   5),   S( 10,  -3),
            S(-16, -57),   S( 11,  14),   S( -5,  -5),   S(  7,  39),   S( 18,  33),   S( 11,   1),   S( -5,  -1),   S( 11,   1),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  1,   2),   S(-17, -20),   S( -7,  -5),   S(-12, -19),   S( -1,  -3),   S( -2,  -3),
            S(  0,   0),   S(  0,   0),   S(  5,   3),   S( -9, -17),   S( -8,  -8),   S(-11, -23),   S( -8, -16),   S(  2,   0),
            S( -6,  -9),   S(  5,   7),   S( -6,  -7),   S(-12, -35),   S( 16,  30),   S( -1,  12),   S( -1,  -7),   S(  8,   9),
            S( -9, -18),   S(  6,   3),   S(  6,  13),   S(  4,  13),   S(  1,   3),   S( -1,   9),   S( -3,  -3),   S( -3,  -9),
            S(-17, -28),   S(  3,   9),   S(  7,   3),   S(  6,   5),   S(  6,  27),   S( -5, -20),   S( -8, -17),   S( -2,  -2),
            S(  2,  -5),   S( -4, -11),   S(  0, -13),   S(  6,   9),   S( -5, -10),   S( -8,   1),   S(-11, -10),   S( -2,  -7),
            S( -8, -10),   S(  4,   7),   S( -6, -11),   S( 13,  10),   S(  0,   0),   S( -9, -14),   S(  1,   1),   S( -7, -26),
            S(  7,  12),   S(  0,  -3),   S(  2,  -6),   S(  0,   2),   S( -6,  -7),   S(-12, -13),   S( -3,  11),   S( -8, -13),

            /* queens: bucket 13 */
            S(-23, -34),   S(-15, -30),   S(  0,   0),   S(  0,   0),   S(-17, -28),   S(-13, -34),   S( -1,  -2),   S( -4,  -9),
            S(-16, -45),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(-16, -35),   S(-22, -43),   S(-12, -21),   S( -4,  -6),
            S(-22, -38),   S( -5, -14),   S( -4,  -4),   S( -2, -14),   S(-22, -41),   S(-11, -16),   S( -8,  -7),   S( -1,  -5),
            S( -8, -18),   S(-19, -29),   S(  0, -11),   S( -7, -19),   S( 10,   5),   S( 18,  32),   S( -4, -15),   S( -8, -11),
            S(  6,  -7),   S(  3, -20),   S( -7, -20),   S( 12,  23),   S( -7,  -9),   S( -1, -16),   S( -3,  -5),   S(  2, -11),
            S( -1,  -1),   S(-13, -18),   S(  5,   3),   S( 11,  22),   S(  0, -11),   S( -5,  -7),   S(-12, -23),   S( -9, -21),
            S(  0,   0),   S( -3,  -9),   S( 11,  24),   S( -2,  -2),   S(  4,   2),   S(  8,   0),   S(-13, -25),   S( -7, -11),
            S( -7,  -6),   S( -2,  -5),   S( -5, -13),   S(  1,  -6),   S(  4,  -1),   S( -1,  -3),   S(  0,  -8),   S(-12, -20),

            /* queens: bucket 14 */
            S( -6, -15),   S( -1, -10),   S(-10, -19),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S( -4,  -7),   S( -8, -23),
            S( -7, -24),   S(-26, -47),   S(-11, -22),   S( -3, -14),   S(  0,   0),   S(  0,   0),   S( -8, -23),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -7, -21),   S(-14, -25),   S( -3,  -4),   S(  1,   3),   S(-11, -17),   S(-17, -33),
            S( -9, -11),   S( -2,  -1),   S(  0,   0),   S(-15, -20),   S( -9, -16),   S(-20, -29),   S( -2, -21),   S(  1,   2),
            S( -6, -12),   S( -4, -12),   S( -4, -15),   S(  6,  11),   S(  5,  19),   S(-10, -25),   S( -9,  -3),   S( -1,  -4),
            S( -5, -12),   S(  3,  -4),   S(-12, -20),   S(-12, -22),   S(  6,  10),   S(  2,   5),   S( -1,  -5),   S( -9, -11),
            S( -9, -16),   S( -2,  -9),   S(  0,   0),   S(  3,   6),   S(  3,   4),   S(  4,   5),   S( -8, -21),   S( -3,  -9),
            S(-10, -17),   S(  5,  -5),   S(-10, -15),   S( -3,  -8),   S(  3,   1),   S( -3,  -3),   S( -4,  -3),   S(  2,  -8),

            /* queens: bucket 15 */
            S(  1,   3),   S( -6, -18),   S(  5,   1),   S(-11, -18),   S(  4,   7),   S(-10, -11),   S(  0,   0),   S(  0,   0),
            S( -4,  -4),   S(  1,   6),   S(-13, -17),   S( -8, -17),   S(  0,  -7),   S(  2,   7),   S(  0,   0),   S(  0,   0),
            S( -1,   0),   S(  1,  -1),   S(-12,  -4),   S( -6,  -5),   S(-10, -22),   S(  3,   4),   S( -1,   2),   S( -1,  -4),
            S( -3,  -5),   S(-10, -15),   S( -3,  -5),   S(  2,   8),   S( 10,  29),   S(  6,  27),   S( -3,   5),   S( -4, -13),
            S(  1,   2),   S(  1,   0),   S( -3,  -9),   S( -1,  -1),   S( 11,  51),   S(  5,  21),   S(  4,  12),   S( -6, -15),
            S( -1,  -3),   S( -3,  -2),   S( -3,  -8),   S( -6,  -2),   S( -1,   3),   S( -9,  -8),   S(  2,  11),   S( -7,  -5),
            S( -5, -11),   S(  0,  -1),   S( -5,   4),   S(  4,   4),   S( -7, -11),   S(  1,   6),   S(  5,  10),   S( -5, -10),
            S( -8, -18),   S(-13, -30),   S( -1, -10),   S(  2,   2),   S(-13,  -3),   S( -3,  -1),   S(  1,   0),   S( -3,   4),

            /* kings: bucket 0 */
            S( -9, -20),   S( 29,  -8),   S( 16,  -3),   S(-26,  15),   S(-11,  14),   S( 30, -25),   S(  2,   3),   S(  9, -48),
            S(-17,  29),   S( -4,   1),   S( -1,   2),   S(-42,  23),   S(-40,  41),   S(-10,  19),   S(-13,  34),   S(  1,  23),
            S(  9,   5),   S( 69, -31),   S(  4,  -5),   S(-24,   1),   S(-21,   3),   S(  4,  -9),   S(-23,  13),   S( 34, -30),
            S(-27, -26),   S( 11, -29),   S( 17, -29),   S(-22,   8),   S(-38,  31),   S(-44,  25),   S(-33,  35),   S(-14,  30),
            S(-52, -121),  S( -3, -46),   S( -1, -34),   S( 14, -23),   S(-46,  -6),   S(-31,   9),   S(-19,  10),   S(  0,  -9),
            S(-10, -120),  S(  1,   7),   S( -9, -54),   S(-13,  -8),   S(  0, -14),   S(-24,  17),   S( 18,  21),   S(-20,   7),
            S(  0,   0),   S(  0,   0),   S(  0, -49),   S(  4, -33),   S(-18,  -4),   S(-11, -16),   S(-29,   4),   S(-10,  -3),
            S(  0,   0),   S(  0,   0),   S(-12, -11),   S(  1, -11),   S(  8,  -3),   S( -6,  11),   S(  7,   4),   S(  9,   0),

            /* kings: bucket 1 */
            S(  6, -25),   S( 30, -21),   S( 17, -16),   S( 31,  -4),   S( -2,  -2),   S( 34, -21),   S(  3,   5),   S( 16, -23),
            S( 10,  -2),   S(  3,  10),   S( -3,  -8),   S(-49,  28),   S(-31,  20),   S(-11,  14),   S( -7,  16),   S(  5,   7),
            S(-12, -15),   S( -5, -12),   S(  2, -17),   S(  7, -20),   S(-37,   2),   S( 14, -19),   S( 24, -13),   S( 35, -11),
            S( -3,  -1),   S(  7, -12),   S( 14,  -7),   S(  1,   5),   S( 22,   6),   S( -3,  -1),   S( 38,  -9),   S(-19,  26),
            S(-20, -55),   S(-15, -45),   S( -6, -54),   S(-14, -42),   S(  0, -25),   S( -1, -29),   S(-10,  -4),   S( -7,  -3),
            S(-34,   0),   S(-102,   4),  S(-33,  27),   S(  2,  22),   S(-43,   5),   S(-25,  13),   S( 16,   3),   S( -8,  -9),
            S(-35, -51),   S(-24,   4),   S(  0,   0),   S(  0,   0),   S(-40,  13),   S(-52,  26),   S( -5,  27),   S( -5, -32),
            S(-30, -111),  S(-13, -14),   S(  0,   0),   S(  0,   0),   S( -8,   8),   S(-14,  14),   S( -4,  20),   S( -5, -47),

            /* kings: bucket 2 */
            S( 10, -54),   S(  6,  -1),   S( 18, -18),   S( 17,  -9),   S(  1,   6),   S( 37, -24),   S( -5,  17),   S( 16, -24),
            S( 35, -36),   S(-16,  31),   S(-17,   8),   S(-21,  10),   S(-28,  15),   S(-15,   6),   S(  1,   0),   S(  1,   0),
            S(-34,  -3),   S(-17, -13),   S(-11, -11),   S(-18, -18),   S( -9,  -3),   S(  0, -18),   S( 27, -18),   S( 23, -16),
            S( 13,  13),   S( -6,  12),   S( 14,   0),   S(-14,   9),   S( 41,  -9),   S( -8, -11),   S( 38, -29),   S( 32, -10),
            S( -6, -10),   S( 17, -15),   S( 27, -38),   S(  9, -29),   S( 33, -49),   S(-20, -42),   S( 24, -50),   S(  2, -44),
            S(  1,   6),   S(-11,  -6),   S(-41,   2),   S(-39, -12),   S(  2,   0),   S(-11,  26),   S(-83,  10),   S(-20, -18),
            S( -8, -10),   S( -9,  21),   S(-75,  13),   S(-17,  10),   S(  0,   0),   S(  0,   0),   S(-10,  17),   S(-38, -37),
            S( -8, -39),   S(-20, -27),   S(-32, -31),   S( -7,   9),   S(  0,   0),   S(  0,   0),   S(-10, -11),   S(-34, -120),

            /* kings: bucket 3 */
            S( -5, -52),   S( 14,  -6),   S( 27, -21),   S( -4,  -5),   S(  1, -13),   S( 38, -26),   S(  1,  15),   S(  8, -28),
            S(  4,  17),   S(-20,  39),   S(-15,   5),   S(-33,  16),   S(-52,  30),   S(  1,  -1),   S( -8,  18),   S(  4,  11),
            S( 18, -26),   S(  5,  -5),   S( -1,  -9),   S(-32,  -4),   S( -9,   8),   S( 20, -20),   S( 52, -22),   S( 54, -16),
            S(-17,  32),   S(-82,  41),   S(-49,  16),   S(-34,  12),   S(-17,   7),   S(  1, -26),   S(-21,  -8),   S(-26, -16),
            S(-15,   9),   S( -9,  -5),   S(-34, -11),   S(-16, -16),   S( 35, -45),   S( 59, -68),   S( 44, -72),   S(  6, -80),
            S(-13, -13),   S( 23,   5),   S( 21, -12),   S(  2, -24),   S( 45, -32),   S( 60, -50),   S( 72, -22),   S( 51, -116),
            S(-23, -11),   S( 27,  10),   S( 16, -13),   S( 31, -22),   S( 33, -30),   S( 29, -53),   S(  0,   0),   S(  0,   0),
            S( -5, -11),   S(  5,   9),   S( -4,  19),   S( 10,  -9),   S(  8, -69),   S( -1,  10),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-62,   7),   S(  3,  36),   S(  2,  23),   S(  6,   2),   S(-16,   7),   S(  1,  -8),   S(  1,   8),   S( 12, -31),
            S(-38,  22),   S( 32,  17),   S(  5,  15),   S( -1,   1),   S( 34,  -3),   S( 26,  -5),   S( 50, -16),   S( 13,  -3),
            S( -3,  26),   S( 14, -13),   S( 19,  -5),   S( -6,   0),   S(-18,  11),   S( 21, -23),   S(-35,   6),   S( 18, -12),
            S( -2, -22),   S( -9,   9),   S(  4,  16),   S(  9,   4),   S(-15,   8),   S( -9,  16),   S( 17,   9),   S( 10,   6),
            S(  0,   0),   S(  0,   0),   S( -1,   3),   S(-27,  12),   S(-35,  14),   S(-26, -15),   S(-21,   1),   S( -4,  -2),
            S(  0,   0),   S(  0,   0),   S(  6, -12),   S( -4,  24),   S(-10,  26),   S(-26, -13),   S(  8, -17),   S( -2,  16),
            S( -3, -20),   S( -4,  -7),   S( -4, -22),   S(  0,  21),   S( -6,  24),   S(-27,  -9),   S(-12,  19),   S(  3,  -4),
            S( -5, -23),   S(  2, -14),   S(-10, -19),   S( -8,   3),   S(  6,  11),   S( -7, -10),   S( -6,   0),   S(  5,  11),

            /* kings: bucket 5 */
            S( 27,  -2),   S(-18,  16),   S(-43,  27),   S(-49,  31),   S(-25,  28),   S(-10,  14),   S( 28,   1),   S( 14,  -2),
            S( -4,   1),   S( 20,  10),   S( 33,  -6),   S( 30,  -7),   S( 23,  -6),   S( 40, -13),   S( 29,   1),   S( 44, -16),
            S(-16,  10),   S( -4,  -7),   S(-15,  -4),   S( -4,  -8),   S(  9,  -1),   S(-38,   0),   S( -8,   4),   S( 12,   0),
            S( -3, -12),   S( -2,  -5),   S(  8,  -5),   S( 10,  17),   S(  4,  20),   S( 10,   2),   S( 16,   5),   S(  6,   5),
            S( -4, -29),   S(-30, -45),   S(  0,   0),   S(  0,   0),   S( -7,  -3),   S(-19, -13),   S(  7, -14),   S(-10,   4),
            S( -6, -39),   S(-25, -29),   S(  0,   0),   S(  0,   0),   S(-23,  39),   S(-54,  11),   S(-16,  -3),   S( -7,  -3),
            S(-16, -31),   S(-31,  21),   S(  2,   8),   S(  0, -16),   S(-27,  29),   S(-39,  19),   S(  0,   9),   S( 10,  19),
            S(-10, -100),  S( -9,  12),   S(-10, -25),   S( -2, -33),   S(-10, -17),   S( -6,  10),   S( -2, -15),   S(  0,   7),

            /* kings: bucket 6 */
            S( 30, -34),   S( 21, -10),   S(-10,   4),   S(-27,  25),   S(-18,  22),   S(-31,  23),   S(-10,  25),   S(-10,   9),
            S( 40, -25),   S(  8,  17),   S( 11,  -6),   S( 27,  -9),   S( 28,  -7),   S( -5,  10),   S( 18,  -2),   S(  1,   2),
            S( 13, -17),   S(-27,   3),   S(-19,  -8),   S( -6,  -8),   S( 12, -10),   S(-45,   5),   S(  8,  -2),   S(-25,  16),
            S( 10,   5),   S( 24,  -3),   S( 13, -12),   S( 28,   4),   S( 63,  -2),   S(-30,   5),   S( -8,   7),   S(  2,   0),
            S(  6, -19),   S( 20, -30),   S(-24, -11),   S(  1, -17),   S(  0,   0),   S(  0,   0),   S(-46, -21),   S(-42, -18),
            S(-17,   1),   S(  5,  -1),   S(-31,  -1),   S(-11, -20),   S(  0,   0),   S(  0,   0),   S(-27, -15),   S(-29, -21),
            S(  0, -10),   S( -9,   8),   S(-39,  10),   S(-16,  -2),   S(  4,   7),   S( -9, -31),   S(-28, -12),   S( -8, -38),
            S( -1,  -6),   S(  2,  -6),   S( -3,  11),   S(-15, -29),   S( -7, -37),   S( -4, -25),   S( -6,  -1),   S( -2, -57),

            /* kings: bucket 7 */
            S( 24, -31),   S(-13,   0),   S(-33,   0),   S(-18,  11),   S(-35,  13),   S(-52,  38),   S(-37,  38),   S(-54,  29),
            S(  6,   1),   S( 20, -20),   S( -4,  -8),   S(-24,   5),   S( -6,   4),   S(-27,  20),   S( 14,  -9),   S( -2,  11),
            S( 28, -28),   S(-18,  -8),   S(-33,  -2),   S(-31,  -3),   S(-41,   8),   S(-28,  11),   S( 19,  -6),   S(-52,  23),
            S(-25,  19),   S(  5,   9),   S( -4,  -1),   S( 42,  -8),   S( 35, -10),   S( 55, -29),   S( 21, -11),   S( 13,  -8),
            S(-17,  16),   S( -4,   1),   S(  0, -23),   S( 12, -17),   S( 16, -26),   S( 10, -21),   S(  0,   0),   S(  0,   0),
            S(-11, -31),   S( -1,  -7),   S( 15, -11),   S( 12,  -7),   S( 22,  -9),   S( 16, -10),   S(  0,   0),   S(  0,   0),
            S( 13,  18),   S( -4, -18),   S(  2,   5),   S(-13, -12),   S(  7, -18),   S( -5, -26),   S(  5, -17),   S(-11,  11),
            S(  7,   7),   S( -8,  -8),   S( 10,  18),   S( -3,  -4),   S(  6,  13),   S(-19, -50),   S(  9, -13),   S(-11, -60),

            /* kings: bucket 8 */
            S( 13, 118),   S( -3,  81),   S( 40,  40),   S( -3,  -1),   S(-12,  13),   S(-15,  -4),   S( 29, -14),   S(-16, -18),
            S( 29,  70),   S( 27,  14),   S( 50,  60),   S( 84,  -3),   S( 18,  23),   S(  7,  -6),   S( -3,   9),   S(  3,  26),
            S(  0,   0),   S(  0,   0),   S( 29,  66),   S( 39,   2),   S( 19,   8),   S( -9,  -9),   S( -1,  13),   S(  9, -16),
            S(  0,   0),   S(  0,   0),   S(  4,  75),   S( -8,   0),   S(-19,  36),   S( -5,  17),   S( 14,  10),   S( 10,  14),
            S( -3, -26),   S( -1,  27),   S(  4,  14),   S(-17,  26),   S(-16,  -3),   S(  4, -16),   S(  2,  10),   S(-15, -27),
            S(  5,  13),   S( -1, -14),   S( -2, -14),   S( -7,   2),   S(-12,   0),   S(-10,  -3),   S( -7,  -3),   S(  8,  -8),
            S( -5, -14),   S( -9, -12),   S(  4,  10),   S( -1, -11),   S( -3, -32),   S(-11,   7),   S( -3,   1),   S(  6, -46),
            S( -6,  -9),   S(-12, -25),   S( -2, -11),   S( -6, -22),   S(  7,   7),   S( -6,   3),   S(  0,  -4),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  5,  28),   S(-14,  33),   S(-19,  56),   S( 18,   9),   S(-19,  33),   S(-29,  28),   S( 34,   6),   S( 18,  15),
            S(-19,  35),   S( 36,  23),   S(  3,   0),   S( 47,   1),   S( 61,  16),   S( 27,   5),   S( -7,  27),   S(-17,  13),
            S( -5,  11),   S( 22,  13),   S(  0,   0),   S(  0,   0),   S( 48,  18),   S(  0,   2),   S( 10,  -2),   S(-19,  22),
            S( -2, -31),   S( 12, -23),   S(  0,   0),   S(  0,   0),   S(  6,  33),   S( 13,  -1),   S(-14,  11),   S(-17,  31),
            S(  4, -19),   S( 12,  -2),   S(  4,  18),   S(  0,  13),   S(-14,  17),   S(-21,  15),   S( -8,  12),   S(  1, -16),
            S(  5,   4),   S(  1,  -9),   S(  8,  -9),   S(-11, -22),   S(-12,  11),   S( -1,   9),   S(-31,   1),   S(  5,  31),
            S(  2,  -6),   S( -4, -21),   S( -2,  -8),   S(  3, -30),   S( 14, -31),   S( 13,  17),   S(-17,  -9),   S(  4,   4),
            S(  7,   6),   S( -2, -23),   S( 10, -24),   S( -5, -21),   S( -2, -15),   S(  2,   8),   S( -7,  10),   S(  8,  -1),

            /* kings: bucket 10 */
            S( 34,  -1),   S(  2,  -7),   S(  5,   9),   S(  6,  25),   S(-15,  20),   S(-93,  49),   S(-34,  48),   S(-92,  87),
            S(  3,   1),   S( 61,   1),   S( 25,  -5),   S( 31,  11),   S( 57,  12),   S( 47,   3),   S( 17,  24),   S(-91,  49),
            S( 14,   7),   S( 26,   1),   S( 27, -12),   S( 14,  10),   S(  0,   0),   S(  0,   0),   S( -7,  22),   S(-60,  28),
            S( 15,   6),   S( 42, -26),   S( 34, -32),   S( 29,   4),   S(  0,   0),   S(  0,   0),   S( -1,  14),   S(  6,   2),
            S(  3,   6),   S( 27,   6),   S( 29, -19),   S( 10, -29),   S(  3, -16),   S(  7,  23),   S(  8,   8),   S( -9,  15),
            S(  3,  15),   S(  1,  -4),   S( -1,   4),   S( 11,  -7),   S(  8,  -2),   S(-17,  -6),   S(-12,   6),   S(  0,  -8),
            S( -1, -41),   S( -4, -14),   S(  9, -10),   S( 13,   0),   S( 11,  -2),   S(-10, -19),   S(  5, -27),   S(  5,   5),
            S(  4,   4),   S( 11, -11),   S( -1, -14),   S(  0,   4),   S(  5, -13),   S( -1, -29),   S( -4,  -7),   S(  9,   3),

            /* kings: bucket 11 */
            S( -5, -18),   S(  8,   8),   S(  6, -10),   S( -6,  15),   S( -9,   6),   S(-70,  57),   S(-73,  82),   S(-132, 153),
            S( -2, -25),   S( 20,   4),   S(-12, -16),   S( 20,  22),   S( 88,  -1),   S( 63,  40),   S( 17,  19),   S( 23,  40),
            S(  4, -49),   S( -3,  19),   S(  0, -10),   S( 24,   9),   S( 68,   0),   S( 28,  61),   S(  0,   0),   S(  0,   0),
            S(  1,  22),   S( 18,  12),   S( -7,   3),   S(  8,  15),   S( 25,  -9),   S( 22,  24),   S(  0,   0),   S(  0,   0),
            S(  0,  35),   S(  1,  -3),   S(  8,  -7),   S( 13, -14),   S( 16,   0),   S(  0,   1),   S(  8,  11),   S(  7,   0),
            S( 11,  10),   S(  0, -15),   S( 14, -11),   S( -1,   4),   S( -6,  -7),   S(  3, -16),   S( -5,  -8),   S(-10,  -3),
            S(  6,  14),   S(  7,  -7),   S( 17,  23),   S(  0, -26),   S( 17, -17),   S(  3,   3),   S(-10, -12),   S( -8, -16),
            S(  4,   8),   S(  4,   0),   S(-12, -23),   S(  4,  -6),   S( -4, -20),   S( -8, -18),   S(  0, -20),   S(  5,  11),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 20,  58),   S(  7,  -6),   S(  1,  -2),   S(  8,  14),   S(  8,   0),   S(-20,   8),
            S(  0,   0),   S(  0,   0),   S( 46, 108),   S( 29,  13),   S( 22,  43),   S( 13,  -3),   S( 24,  -6),   S(-17,  -1),
            S( -1,  10),   S(  3,  13),   S( 24,  71),   S( 39,  18),   S(  8,  -7),   S( 11,   2),   S(  2, -13),   S( -8,  -1),
            S( -2,   9),   S(  9,  31),   S( -1,  16),   S(  3,  -6),   S( -8,   0),   S( -2,  18),   S( -4,   9),   S(  1,   7),
            S( 10,  17),   S(  6,  23),   S( 10,  20),   S( -2,  40),   S( -3,  38),   S(  0,   3),   S( -8,  14),   S(-12, -12),
            S(  7,   5),   S( 10,  14),   S( -2,  -1),   S(-10, -15),   S(  0,   5),   S( -7,  16),   S( -9, -16),   S(  7,  -1),
            S(  3,   8),   S( -6, -13),   S( -2,   6),   S( -1,   0),   S( -5,  -9),   S(  4,   7),   S(  8,  44),   S( -1, -29),
            S( -3,   2),   S(  5,   3),   S( -4,   6),   S(  0,   3),   S( -1,  -4),   S(  3,   5),   S(-11, -22),   S( -3, -22),

            /* kings: bucket 13 */
            S( -1,  53),   S(  7,  35),   S(  0,   0),   S(  0,   0),   S( 44,  18),   S( 13, -12),   S( -4,  -8),   S(-18,  26),
            S(  2,  20),   S( -1,  -3),   S(  0,   0),   S(  0,   0),   S( 47,   2),   S( 28,  -9),   S(-18,   4),   S(-14,   6),
            S( -3,   3),   S( 19,  22),   S(  2,  -7),   S( 14,  38),   S( 52,  13),   S( 22,  -6),   S(  2,   6),   S( 14,  -8),
            S(-10,  -6),   S( 15,  -2),   S(  0,  20),   S( -7,  16),   S( -5,  14),   S(  3, -12),   S(  4,  21),   S(-16, -27),
            S(  6,  11),   S(  0,   6),   S(  5,  42),   S( -5,  24),   S( -8,  10),   S(  5,  18),   S(-10,   1),   S(  8,  10),
            S(  4,   0),   S( -5,  17),   S( -2,  17),   S( -4,  -1),   S(-12, -16),   S( -5,   9),   S( -8,  19),   S(  1,   1),
            S(  9,  11),   S( -8, -21),   S(-11, -43),   S(  4,  19),   S(-11, -10),   S(-10,  14),   S(-14, -24),   S(  6,  14),
            S(  1,  -1),   S(  5,  -4),   S(  4,  20),   S(  3,   5),   S(  0,  18),   S(-11, -17),   S( -3,   8),   S(  8,  15),

            /* kings: bucket 14 */
            S( 18,  34),   S(  0,  -7),   S( 11, -41),   S( 16,   0),   S(  0,   0),   S(  0,   0),   S(  5,  71),   S(-44,  41),
            S( -9,  -6),   S( 19,  -8),   S( 46, -34),   S( 41,  13),   S(  0,   0),   S(  0,   0),   S( 14,  29),   S(-43,   4),
            S(  5,   3),   S( 15,  -5),   S( 35, -34),   S( 40,   3),   S( 11,  -2),   S( 14,  33),   S( 28,  55),   S(-27,   3),
            S(  8,  -5),   S(  7,  -9),   S( -2, -11),   S( 10,   1),   S(-22,  -1),   S( 14,  55),   S(  4,  23),   S(  6,  -1),
            S(  7,  19),   S(  9,  -2),   S( -9,   2),   S(-17,  10),   S(  1,  30),   S(  5,  54),   S(  2,  37),   S(  5,  13),
            S( -5,  -7),   S(  2,   6),   S( -1,  -3),   S(  0,  11),   S( -6, -20),   S( -6,  -3),   S(-15,  -7),   S( -1,   8),
            S(  4,   6),   S(-10, -13),   S( 11,  -6),   S( 16,   4),   S(  3,  -4),   S( -6,  17),   S(-26, -22),   S(  8,  18),
            S(  1,  14),   S(  5,  -6),   S(  9,   3),   S( -5,  -6),   S(  7,  -9),   S( -3,  -5),   S(-13, -25),   S(  0, -11),

            /* kings: bucket 15 */
            S( 12,  32),   S(  7,  -2),   S( 11,  -7),   S( -7,   0),   S(-11, -10),   S(  0,  57),   S(  0,   0),   S(  0,   0),
            S( -1, -23),   S(  7, -11),   S( -7, -14),   S( 21,  50),   S( 40,  -1),   S( 62, 108),   S(  0,   0),   S(  0,   0),
            S( -8, -22),   S( 16, -10),   S(  7, -18),   S( -3,  12),   S( 11,  -6),   S( 26,  69),   S(  8,  43),   S(-14,  -1),
            S( -1, -11),   S(  3,  15),   S(  2,  15),   S(-13, -29),   S(-14,  -2),   S( 20,  47),   S( 16,  47),   S( -3, -10),
            S( 10,   6),   S( -8,  24),   S(  1,  -4),   S( -5, -35),   S( -4,   9),   S(  1,  34),   S(  4,   7),   S(  4,   3),
            S(  5,  27),   S(-14,  -3),   S(  8,  16),   S(  8,  19),   S(-10, -24),   S( -3,   7),   S(  1,   8),   S(  4,  18),
            S(  8,  12),   S( -4,  23),   S( -1, -12),   S(  4,   6),   S(  8,   7),   S(  9,  15),   S( -5,  -1),   S(  2,   1),
            S( -2,  -7),   S(  5,   2),   S( -2, -11),   S(  4,   4),   S(  4,   4),   S( 10,  13),   S(  0,  -7),   S(  3,   7),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-43,  47),   S(-18, -19),   S(  2,  63),   S( 15,  97),   S( 25, 119),   S( 33, 143),   S( 42, 147),   S( 55, 141),
            S( 68, 122),

            /* bishop mobility */
            S(-36,  22),   S(-13,  -3),   S(  5,  45),   S( 14,  86),   S( 24, 110),   S( 29, 130),   S( 33, 142),   S( 38, 146),
            S( 43, 148),   S( 52, 145),   S( 64, 136),   S( 85, 129),   S( 94, 129),   S( 64, 132),

            /* rook mobility */
            S(-110,  10),  S(-32,  10),   S(-15,  88),   S(-12, 118),   S(-12, 149),   S( -7, 160),   S(  0, 171),   S(  8, 172),
            S( 14, 184),   S( 21, 188),   S( 23, 195),   S( 32, 194),   S( 42, 196),   S( 49, 200),   S( 87, 172),

            /* queen mobility */
            S( 89, 163),   S(-24, 328),   S( 26, 209),   S( 40, 118),   S( 50, 131),   S( 50, 190),   S( 52, 229),   S( 55, 263),
            S( 56, 293),   S( 58, 318),   S( 61, 334),   S( 64, 345),   S( 65, 353),   S( 65, 367),   S( 67, 369),   S( 68, 371),
            S( 70, 371),   S( 74, 364),   S( 81, 351),   S( 96, 335),   S(106, 318),   S(148, 277),   S(151, 268),   S(176, 233),
            S(187, 219),   S(181, 201),   S(123, 193),   S(109, 145),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   2),   S(-24,  32),   S(-32,  28),   S(-43,  49),   S(  3,  -1),   S(-13,  -1),   S( -4,  43),   S( 14,  16),
            S(  7,  22),   S( -6,  32),   S(-20,  33),   S(-23,  28),   S( -6,  25),   S(-30,  28),   S(-29,  42),   S( 23,  17),
            S( 18,  57),   S( 10,  59),   S(  6,  43),   S( 20,  39),   S( -3,  43),   S(-25,  52),   S(-32,  82),   S(-12,  66),
            S( 29,  94),   S( 38, 106),   S( 18,  72),   S(  7,  56),   S(  3,  60),   S(-13,  80),   S(-57, 115),   S(-81, 139),
            S( 18, 142),   S( 48, 179),   S( 51, 124),   S( 25, 109),   S(-58, 102),   S( 12, 103),   S(-63, 171),   S(-89, 160),
            S( 90, 221),   S( 84, 268),   S(128, 234),   S(122, 247),   S(127, 259),   S(152, 237),   S(136, 252),   S(131, 252),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,  -5),   S( -6, -27),   S( -5, -18),   S(  4,  -9),   S( 13,   4),   S(-15, -45),   S(-23,   8),   S(  0, -55),
            S(-19,  16),   S( 24, -23),   S( -2,  27),   S( 15,  20),   S( 32, -11),   S( -6,  16),   S( 25, -23),   S( -5,  -6),
            S(-11,  18),   S( 14,   6),   S(  4,  41),   S( 16,  54),   S( 25,  30),   S( 32,  17),   S( 30,   0),   S( -3,  18),
            S( 14,  38),   S( 16,  50),   S( 33,  96),   S( 14, 102),   S( 63,  71),   S( 64,  58),   S( 24,  57),   S( 17,  28),
            S( 51,  94),   S( 87, 114),   S(100, 139),   S(139, 163),   S(136, 133),   S(137, 145),   S(131, 121),   S( 50,  61),
            S( 73, 193),   S(117, 277),   S(102, 221),   S( 97, 198),   S( 67, 152),   S( 48, 139),   S( 40, 143),   S( 16,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  17),   S( 16,  18),   S( 28,  29),   S( 32,  16),   S( 20,  16),   S( 20,  17),   S(  4,   7),   S( 37,  -7),
            S( -5,  24),   S( 17,  36),   S( 13,  35),   S(  9,  44),   S( 23,  16),   S( 15,  21),   S( 31,  18),   S(  1,  14),
            S( -2,  25),   S( 28,  53),   S( 51,  60),   S( 37,  64),   S( 42,  58),   S( 68,  21),   S( 31,  37),   S( 17,  10),
            S( 52,  73),   S(102,  56),   S(119, 123),   S(142, 130),   S(131, 121),   S( 69, 130),   S( 66,  58),   S( 64,  13),
            S( 44, 122),   S( 89, 138),   S(153, 208),   S(108, 249),   S(133, 258),   S( 80, 236),   S(148, 200),   S(-56, 167),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  33),   S( 11,  13),   S( 12,  30),   S(-13,  61),   S( 64,  18),   S( 19,   6),   S( -4,  -3),   S( 28,  14),
            S( -2,  16),   S(  5,   8),   S( 17,  16),   S( 12,  29),   S(  7,  17),   S(  3,   7),   S(  9,   5),   S( 26,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2, -16),   S( -5,  -8),   S(-17, -16),   S(-12, -29),   S( -7, -17),   S( -3,  -7),   S( -9,  -5),   S(-26,   3),
            S(-25, -33),   S(-11, -13),   S(-12, -30),   S( 13, -61),   S(-64, -18),   S(-19,  -6),   S(  4,   3),   S(-28, -14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15, -29),   S(-13, -43),   S(-12, -43),   S(-55, -32),   S(-21, -43),   S(-27, -41),   S( -7, -51),   S( -9, -50),
            S(-15, -13),   S(-19, -30),   S(-31, -10),   S( -2, -37),   S(-34, -34),   S(-27, -21),   S(-35, -22),   S(  1, -33),
            S( -8,  -8),   S( -8, -34),   S(-23,  -7),   S(-27, -21),   S(-15, -37),   S(-19, -14),   S(-10, -21),   S(-27, -17),
            S(  4, -23),   S( 17, -46),   S( 13, -14),   S( 10, -28),   S( 10, -26),   S( 55, -37),   S( 34, -44),   S(  0, -42),
            S( 22, -42),   S( 38, -78),   S( 47, -28),   S( 63, -35),   S( 77, -51),   S( 79, -35),   S(129, -97),   S( 44, -73),
            S(103, -93),   S(120, -114),  S( 94, -49),   S( 73, -32),   S( 70, -32),   S(120, -45),   S( 96, -54),   S( 54, -80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-10, -26),

            /* doubled pawn */
            S(-12, -33),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(-44,  18),        // attacks to squares 1 from king
            S(-43,   5),   S(-15,   1),   S( 25,  -8),   S( 76, -22),   S(129, -34),   S(142, -17),   S(192, -33),   S(238, -20),

            S(-38,  11),        // attacks to squares 2 from king
            S(-38,   8),   S(-27,   9),   S( -6,   3),   S( 16,   0),   S( 39,  -4),   S( 62, -11),   S( 91, -20),   S(147, -29),

            /* castling available */
            S( 69, -61),        // king-side castling available
            S( 14,  65),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 41, -92),   S( 45, -82),   S( 39, -91),   S( 30, -77),   S( 23, -66),   S( 16, -60),   S( -1, -48),   S( -3, -44),
            S( 11, -47),   S( 29, -48),   S( 57, -47),   S( 21, -31),   S( 97, -53),

            /* orthogonal lines */
            S(-44, -144),  S(-97, -109),  S(-119, -89),  S(-135, -82),  S(-142, -85),  S(-148, -86),  S(-147, -92),  S(-142, -98),
            S(-156, -88),  S(-172, -83),  S(-169, -97),  S(-122, -129), S(-91, -142),  S(-39, -152),

            /* pawnless flank */
            S( 52, -34),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 26, 225),

            /* passed pawn can advance */
            S(-11,  33),   S( -3,  59),   S( 18, 100),   S( 91, 166),

            /* blocked passed pawn */
            S(  0,   0),   S( 56, -17),   S( 30,   9),   S( 27,  46),   S( 24,  63),   S( 15,  35),   S( 65,  63),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 52, -44),   S( 47,  26),   S( 21,  36),   S( 10,  59),   S( 25,  95),   S(129, 125),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-16, -16),   S(-10, -34),   S( -5, -34),   S(-30, -11),   S(-38,  18),   S(110,   8),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 26, -12),   S( 26, -12),   S(  1,   0),   S( -1, -38),   S(-20, -116),  S(-43, -211),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 26,  50),   S( 62,  21),   S( 96,  44),   S( 30,  23),   S(170, 112),   S( 99, 116),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 12,  52),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-29, 122),

            /* bad bishop pawn */
            S( -6, -15),

            /* rook on open file */
            S( 22,  -3),

            /* rook on half-open file */
            S(  1,  38),

            /* pawn shields minor piece */
            S( 12,  12),

            /* bishop on long diagonal */
            S( 25,  50),

            /* minor outpost */
            S(  6,  34),   S( 18,  29),

            /* rook on blocked file */
            S(-11, -10),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 20,  23),   S( 20,  -4),   S( 31,  18),   S( 26,  -4),   S( 36, -21),

            /* pawn threats */
            S(  0,   0),   S( 65,  97),   S( 51, 116),   S( 73,  88),   S( 61,  44),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  64),   S( 52,  49),   S( 78,  45),   S( 51,  68),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 25,  47),   S( 30,  41),   S(-18,  44),   S( 68,  64),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 22,  11),   S( 20,  34),   S( 31,  14),   S(  5,  32),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
