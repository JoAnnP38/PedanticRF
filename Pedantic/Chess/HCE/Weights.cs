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

        public static readonly Guid HCE_WEIGHTS_VERSION = new("e3afd1d2-468e-4531-a73b-01f904fc06b7");

        // 6 (piece weights) + (6x64x16x2) 6 Piece Types X 64 Squares X 16 King Buckets X 2 Both Kings
        public const int MAX_WEIGHTS = 12796;
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
        public const int PAWNLESS_FLANK = 12712;    // king is on pawnless flank
        public const int KING_OUTSIDE_PP_SQUARE = 12713;    // king cannot stop promotion
        public const int PP_CAN_ADVANCE = 12714;    // passed pawn can safely advance
        public const int BLOCKED_PASSED_PAWN = 12718;       // blocked passed pawn
        public const int ROOK_BEHIND_PASSER = 12758;// rook behine passed pawn
        public const int BISHOP_PAIR = 12759;       // bonus for having both bishops
        public const int BAD_BISHOP_PAWN = 12760;   // bad bishop pawn
        public const int ROOK_ON_OPEN_FILE = 12761; // rook on open file
        public const int ROOK_ON_HALF_OPEN_FILE = 12762;    // rook on half-open file
        public const int PAWN_SHIELDS_MINOR = 12763;// pawn shields minor piece
        public const int BISHOP_LONG_DIAG = 12764;  // bishop on long diagonal
        public const int PAWN_PUSH_THREAT = 12765;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12771;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12777;      // minor piece threat
        public const int ROOK_THREAT = 12783;       // rook threat
        public const int CHECK_THREAT = 12789;      // check threat against enemy king
        public const int TEMPO = 12795;             // tempo bonus for side moving

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

        // Solution sample size: 16000000, generated on Thu, 25 Apr 2024 10:32:13 GMT
        // Solution K: 0.003850, error: 0.082255, accuracy: 0.5140
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 75, 224),   S(386, 669),   S(412, 660),   S(541, 1074),  S(1380, 1799), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(108, -126),  S(149, -94),   S( 41, -44),   S(-23,  23),   S(-32,  14),   S(-24,   1),   S(-52,   5),   S(-29, -14),
            S(126, -130),  S(107, -109),  S(  9, -65),   S(-11, -55),   S(-19, -17),   S(-22, -24),   S(-38, -21),   S(-24, -39),
            S(114, -105),  S( 66, -65),   S( 14, -65),   S( 13, -69),   S(-10, -58),   S(  5, -55),   S(-11, -49),   S(  6, -51),
            S( 75, -43),   S( 53, -61),   S( 27, -61),   S( 17, -82),   S(-15, -41),   S(-16, -50),   S(-19, -39),   S( -6, -22),
            S( 79,  35),   S( 33, -11),   S( 39, -30),   S( 52, -71),   S( 19, -41),   S(-10, -37),   S(-25,  -3),   S(-32,  55),
            S( 66,  56),   S( 50,  75),   S(  5,   8),   S( 18, -17),   S(-43,   0),   S(  5,   6),   S( -4,  23),   S( 12,  51),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 35, -30),   S( 34, -39),   S( 54, -25),   S(  3,  19),   S(-14,  -4),   S(  4, -12),   S(-44,   5),   S(-31,  24),
            S( 36, -45),   S( 25, -46),   S( 13, -48),   S( -2, -43),   S( -9, -23),   S( -8, -28),   S(-35, -13),   S(-40,  -7),
            S( 32, -41),   S( 12, -30),   S( 16, -55),   S( 14, -56),   S(-22, -24),   S( 14, -49),   S(-10, -31),   S(  3, -23),
            S( 46, -23),   S( 21, -51),   S( 25, -56),   S(  6, -51),   S(-13, -22),   S( 12, -44),   S(-25, -23),   S( -7,   5),
            S( 27,  45),   S(-31,   1),   S( -5, -38),   S( 12, -51),   S( 37, -37),   S( -7,  -7),   S(-28,  24),   S(-24,  72),
            S( 57,  57),   S( 15,   1),   S(-46, -20),   S(-20,  23),   S(-21,  -8),   S(-59,  27),   S(-48,  33),   S(-39,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13,  -1),   S(-22,   3),   S( -7,  -2),   S( -8,  12),   S(  6,  -5),   S( 36, -19),   S(  9, -43),   S(  0, -17),
            S( -5, -25),   S(-27, -14),   S(-19, -35),   S(-15, -35),   S(  7, -33),   S( 10, -32),   S(  0, -41),   S(-18, -26),
            S( -6, -24),   S(-21, -27),   S( -8, -54),   S(  0, -53),   S( -4, -31),   S( 24, -45),   S(  5, -41),   S( 14, -31),
            S(-10,  -8),   S(-12, -46),   S(-13, -52),   S( -5, -55),   S(  7, -47),   S(  4, -31),   S(  2, -24),   S(  7,  -8),
            S( -2,  36),   S(-44,  -6),   S(-43, -41),   S(-43, -34),   S( 12,  -8),   S(-11,   2),   S(-22,  22),   S(-16,  76),
            S(-51,  80),   S(-90,  58),   S(-91,  -5),   S(-67, -19),   S(-38,   4),   S(-18,  18),   S(-12,  -1),   S(-17,  74),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9, -17),   S(-27,  -3),   S(-23,  -5),   S(  4, -37),   S( -1,  -5),   S( 50, -24),   S( 90, -71),   S( 74, -86),
            S( -8, -42),   S(-26, -30),   S(-20, -43),   S(-15, -32),   S( -5, -31),   S( 16, -40),   S( 62, -76),   S( 64, -78),
            S( -2, -48),   S( -5, -57),   S( -2, -66),   S(  2, -67),   S(  2, -56),   S( 28, -59),   S( 40, -69),   S( 83, -77),
            S(  1, -33),   S(  4, -73),   S(  2, -76),   S(  3, -73),   S( 21, -73),   S( 23, -65),   S( 32, -53),   S( 73, -36),
            S( 28,   7),   S( -8, -35),   S(  9, -75),   S( 14, -69),   S( 88, -67),   S( 75, -44),   S( 59,   4),   S( 58,  60),
            S(-31, 105),   S(-20,  13),   S( -3, -48),   S( -4, -67),   S( 69, -78),   S( 67, -24),   S( 61,   3),   S( 69,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-90,  22),   S( -6, -15),   S(-33,  11),   S(-11,  22),   S(-12, -21),   S(-54,  29),   S(-49,   2),   S(-47,   9),
            S(-15,   5),   S( 46, -21),   S( 30, -40),   S( 11, -26),   S(-10, -21),   S(-53, -15),   S(  0, -41),   S(  0, -25),
            S( 42, -19),   S( 46, -18),   S(-12,   5),   S(  5, -31),   S(-34, -28),   S(-16, -33),   S(-20, -41),   S( 22, -37),
            S( 15,  24),   S(-12,  33),   S( 39,  -1),   S(  4,  -3),   S( 15, -37),   S(-37, -24),   S(  7, -42),   S( 52, -30),
            S(-18,  89),   S(-23,  84),   S(-16,  23),   S(-20,   3),   S( -1,  18),   S(-21,   4),   S(-32, -32),   S( 36,  23),
            S( 67,  76),   S( 52,  99),   S(  9,  36),   S( 19,  20),   S( 12, -15),   S(  1, -10),   S(  7,   2),   S(-13,  14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-98,  56),   S(-75,  43),   S( -6,  11),   S( -7,  16),   S(-20,  31),   S(-30,  20),   S(-50,  16),   S(-28,  31),
            S(-52,  18),   S(-58,  19),   S( 33, -17),   S( 20,   2),   S( 17, -10),   S(-15, -18),   S(-29,  -7),   S(-32,  13),
            S(-46,  35),   S(-55,  28),   S( 54, -30),   S( 13, -26),   S( 32, -17),   S(-13, -23),   S(-11,  -9),   S( 14,  -7),
            S(-55,  54),   S(-53,  33),   S(  3,   0),   S( 25,   5),   S(-13,   3),   S(-47,  -4),   S(  2, -12),   S(  9,  16),
            S( 26,  60),   S( 32,  34),   S( 26,  38),   S( 26,  20),   S(-11,  32),   S( 58,  -8),   S( 10,  10),   S( 44,  30),
            S( 62,  44),   S( 59,  16),   S( 39,  -6),   S( 37,  -3),   S( 44, -14),   S( 20,  -4),   S(  9,   8),   S(  5,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  33),   S(-44,  21),   S(-32,  16),   S(-28,  16),   S( 36, -23),   S(-27,  10),   S(-63,   7),   S(-58,  22),
            S(-43,   5),   S(-17, -17),   S(-15, -33),   S(  1,  -9),   S( 38, -19),   S( 27, -26),   S(-35,  -7),   S(-64,   8),
            S(-22,  -2),   S(-21,  -6),   S(-13, -23),   S(-26,  -6),   S( 18, -13),   S( 68, -44),   S( -4, -18),   S(-15,   5),
            S(-34,  20),   S(-77,  12),   S(  3, -29),   S(-13,  -9),   S( 17,  -3),   S( 38, -18),   S( 20, -11),   S( 38,   3),
            S(  2,  26),   S(-56,  15),   S(  9, -30),   S( -4, -13),   S( 49,  22),   S( 71,  18),   S( 37,   8),   S( 66,  29),
            S( 57,  29),   S( 18,   2),   S(  3, -36),   S(  8, -37),   S( 20,  -1),   S( 22,   2),   S( 40, -10),   S( 39,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -20),   S(-51, -13),   S(-30,  -4),   S(-53,  11),   S(-14, -23),   S( 26, -23),   S(  5, -52),   S(-36, -26),
            S(-39, -40),   S(-38, -41),   S(-45, -36),   S(-22, -43),   S( -7, -37),   S( 54, -58),   S( 58, -62),   S( -2, -36),
            S(-38, -41),   S(-55, -36),   S(-41, -46),   S(-16, -42),   S( -4, -29),   S( 44, -43),   S( 53, -62),   S( 58, -49),
            S(-14, -44),   S(-48, -50),   S(-77, -42),   S(-47, -24),   S(  0, -31),   S( 26, -23),   S( 30, -21),   S( 78, -30),
            S(  9, -34),   S(  4, -58),   S(-25, -51),   S( -2, -63),   S( 25,  -6),   S( 34,  -4),   S( 70,  38),   S(104,  31),
            S(-15,   5),   S(-30, -32),   S(  3, -51),   S( -4, -51),   S( -2, -17),   S( 28, -23),   S( 50,  35),   S( 91,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61,  74),   S(-44,  61),   S( 11,  25),   S(-11,  -1),   S( 12,   9),   S( -4,   8),   S(-42,   6),   S(-46,  30),
            S(-63,  64),   S(-60,  56),   S(-32,  42),   S(-15,  12),   S(-11,  -8),   S(-37, -12),   S(-52,  -6),   S(  2,  -3),
            S(-62, 100),   S( -7, 101),   S( -8,  63),   S(-25,  35),   S( 12, -10),   S(-99,  -3),   S(-71, -15),   S(-41,  -2),
            S(-31, 141),   S( 11, 150),   S( 17, 108),   S( 11,  51),   S(-31,  15),   S(-30, -20),   S(-27,  -4),   S(-51,  12),
            S(-14, 170),   S( 44, 155),   S( 27, 162),   S( 56,  99),   S( 20,  11),   S(  2,   5),   S(-18, -13),   S( -5,  19),
            S( 55, 193),   S( 70, 210),   S( 87, 202),   S( 49,  75),   S(  7,  35),   S(-12,   4),   S(-10, -24),   S(  3,  13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-102,  79),  S(-74,  53),   S(  6,  10),   S( 11,  29),   S(  8,   6),   S(-47,  16),   S(-80,  20),   S(-79,  36),
            S(-63,  40),   S(-60,  37),   S(-47,  31),   S(  3,  46),   S(-53,   3),   S(-30,  -9),   S(-77,  -2),   S(-33,  13),
            S(-94,  71),   S(-119, 102),  S(-51,  81),   S(-106,  92),  S(-62,  54),   S(-85,   9),   S(-50, -16),   S(-47,   8),
            S(-72, 108),   S(-38, 119),   S(  4, 123),   S( 44, 127),   S(-28,  60),   S(-39,  14),   S(  8,   3),   S(-47,  25),
            S( 13, 124),   S( 22, 143),   S( 23, 155),   S( 45, 172),   S( 22, 129),   S( -6,  34),   S( -1,   0),   S( -1,   2),
            S( 25,  71),   S( 21, 125),   S( 65, 140),   S( 70, 182),   S( 28, 109),   S( -8,  -6),   S(-14,  -6),   S(-19, -18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-93,  17),   S(-69,   1),   S(-11,  -2),   S(  2,  18),   S(-12,   0),   S(-66,  30),   S(-111,  29),  S(-64,  37),
            S(-103,  10),  S(-86,   9),   S(-16, -16),   S(-26,  -8),   S(-21,  23),   S(-40,  25),   S(-126,  36),  S(-87,  21),
            S(-27,  -9),   S(-87,  17),   S(-31,   4),   S(-87,  71),   S(-79,  85),   S(-13,  41),   S(-119,  50),  S(-88,  46),
            S(-100,  34),  S(-80,  30),   S(-11,  11),   S(-40,  80),   S( 18,  97),   S(-50,  80),   S(-31,  50),   S(  2,  28),
            S(-29,  47),   S(-34,  20),   S(  7,  49),   S( 26, 124),   S(104, 109),   S( 51,  64),   S( -8,  87),   S( 30,  45),
            S(  0,  17),   S(-21,   0),   S( 19,  19),   S( 50, 114),   S( 12, 129),   S( 27,  57),   S( -7,  75),   S( 24,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-75,  -1),   S(-79,  20),   S( 36, -16),   S( -2,  17),   S( -1,  34),   S(-88,  55),   S(-56,  37),   S(-71,  47),
            S(-73, -18),   S(-81, -18),   S(-33, -37),   S(-50,  17),   S(-37,  12),   S(-27,  28),   S(-93,  62),   S(-95,  49),
            S(-37, -30),   S(-59, -31),   S(-52,  -4),   S(-29,  10),   S(-47,  35),   S( -8,  56),   S(-76,  84),   S(-42,  68),
            S(-53,   8),   S(-90, -11),   S(-28, -26),   S(-54,  18),   S(  8,  44),   S( -3,  74),   S( 21, 112),   S( 74,  76),
            S(-21,  25),   S(-47,  -5),   S( -8,  -2),   S( -9,  25),   S( 59,  96),   S( -9, 127),   S( 99, 120),   S( 88, 105),
            S(-32,  48),   S(-19,   5),   S(  9, -15),   S(  3,   3),   S( 20,  71),   S( 32, 154),   S( 67, 182),   S( 34, 176),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16,  11),   S(-19,   9),   S(-19,   0),   S(  2,   6),   S( -3, -11),   S(-10,  15),   S(-15, -20),   S(-17,  -2),
            S(-38, -25),   S( -7,  18),   S(  8,  20),   S( -1,   3),   S(  0,  32),   S( -5, -12),   S(-36, -31),   S(-27, -40),
            S(-17,  39),   S(-36,  97),   S( 19,  67),   S( 20,  39),   S(-14,   4),   S(-46, -14),   S(-44, -47),   S(-44, -58),
            S(-41,  89),   S(-45, 123),   S( 40, 115),   S( 23,  96),   S(-18, -29),   S(-40, -33),   S( -8, -14),   S(-60, -50),
            S( 35,  96),   S( 39, 212),   S( 49, 151),   S( 18,  56),   S( -1,  15),   S( -2, -21),   S( -1,   4),   S(-19, -47),
            S( 47, 109),   S( 53, 217),   S(118, 222),   S( 47,  97),   S( -6,   6),   S( -9,  -7),   S(-11, -31),   S(-23, -39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -17),   S(-21,  13),   S( -6,   8),   S( -2,   5),   S( -9, -10),   S(-30,   4),   S(-36, -42),   S(-23,  -4),
            S(-39, -10),   S(-57,  47),   S(-24,  34),   S( 20,  21),   S(-45,  23),   S(-15, -14),   S(-82, -24),   S(-62,  11),
            S(-59,  49),   S(-52,  50),   S(-38,  79),   S(-10,  96),   S(  3,  34),   S(-40, -31),   S(-63, -28),   S(-79, -24),
            S(-77,  93),   S( -7, 122),   S( -4, 140),   S(  7, 124),   S(  2,  63),   S(-43,  25),   S(-18, -11),   S(-37, -39),
            S(  2,  97),   S( 54, 172),   S( 66, 196),   S( 49, 248),   S( 23, 150),   S(-11,  14),   S( -3, -63),   S(-25, -39),
            S( 41,  69),   S( 73, 172),   S( 84, 194),   S( 83, 253),   S( 40, 110),   S(  3,  10),   S(  0,   3),   S( -5,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -56),   S(-38, -22),   S( -9, -28),   S( -3,  -2),   S( -5,  -3),   S(-32,  10),   S(-36,  -3),   S( -4,  48),
            S(-53,  14),   S(-56,  10),   S(-54, -29),   S(  1,  12),   S(-39,  65),   S(-17,  17),   S(-40,  19),   S(-56,  15),
            S(-62, -22),   S(-60,   8),   S(-36, -18),   S(-22,  43),   S(-19,  73),   S(-50,  35),   S(-34,   5),   S(-64,  45),
            S(-49,  14),   S(-23,  55),   S(-26,  30),   S(  9,  98),   S( -3, 134),   S(-28,  85),   S(-37,  42),   S(-35,  61),
            S(-21, -21),   S( 10,  17),   S( 14,  78),   S( 36, 135),   S( 47, 215),   S( 42, 172),   S( 10,  87),   S( 26,  42),
            S( -2,  23),   S( 19,  35),   S( 30, 114),   S( 36, 136),   S( 65, 214),   S( 57, 117),   S( 30,  95),   S( 19,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -32),   S(-32, -22),   S(-11, -32),   S(  1,  -3),   S( 17,  21),   S(  1,  47),   S(-11, -23),   S( 10,  24),
            S(-42, -30),   S(-33, -13),   S(-14, -41),   S( 24,  -6),   S(-14,  -1),   S(  7,  47),   S(  5,  29),   S( -1,   0),
            S(-17, -73),   S(-32, -59),   S(-19, -51),   S(  3,  -7),   S( 12,  32),   S(-14,  58),   S( -1,  69),   S(-24,  65),
            S(-26, -21),   S(-43, -29),   S(-30,   2),   S( 12,  21),   S(-10,  52),   S(  7,  93),   S(-27, 141),   S( -5,  55),
            S(-27, -41),   S(-31, -30),   S(-13,  15),   S(  1,   0),   S( 36, 115),   S( 64, 164),   S( 58, 223),   S( 77,  70),
            S( -7,   7),   S( -4,  11),   S(  2,   8),   S(  7,  25),   S( 27,  82),   S( 84, 192),   S( 34, 175),   S( 42,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-35,   8),   S( -1,  12),   S(-51,  12),   S(-33,  -7),   S(-38,  -9),   S( -4, -28),   S(-50, -43),   S(-28, -13),
            S(-36,  61),   S( 17, -38),   S(-45,  14),   S(  5, -23),   S(-10, -20),   S(-25, -16),   S(-34, -21),   S(-69, -20),
            S(  7,  66),   S(  0,  -8),   S(  2,  -9),   S(-25,  33),   S(  7,   7),   S(-36,   0),   S(-10, -30),   S(-39, -48),
            S( 13,  -9),   S( 43,   9),   S( 10,  27),   S( 22,  28),   S(  7,   4),   S(  0,   2),   S( -5, -15),   S( -1,  -3),
            S( 21, -28),   S( 39,  12),   S( 16,   6),   S( 69,  -9),   S( 45,  -9),   S( 31,  19),   S( 25, -13),   S(-58, -10),
            S( 23, -12),   S( 14,  10),   S( 33,  10),   S( 57, -18),   S( 37, -48),   S( 18,  11),   S( 13, -22),   S( -2,  -9),
            S( 19, -25),   S( 17, -35),   S( 20, -25),   S( 37, -29),   S( 24, -17),   S( -5, -26),   S( -9, -39),   S(-19, -28),
            S(-63, -50),   S( -5,   3),   S( -4, -16),   S(  5, -41),   S(-18, -20),   S( 24,  13),   S( -5,   5),   S( 19,   2),

            /* knights: bucket 1 */
            S(-41,  32),   S(-54,  84),   S(  9,  41),   S(-42,  64),   S(-27,  45),   S(-31,  27),   S(-39,  49),   S(-15,  -6),
            S( 37,  30),   S(-13,  40),   S( -9,  25),   S(-10,  42),   S( -9,  23),   S(-17,  13),   S( 14, -14),   S(-27,  16),
            S(-31,  32),   S( 16,  12),   S( -4,  12),   S( 13,  27),   S(  3,  30),   S(-31,  28),   S(-16,   6),   S(-32,  22),
            S( -6,  42),   S( 55,  26),   S( 16,  43),   S( 20,  29),   S(  9,  28),   S( -1,  27),   S( 15,  12),   S( 13,  15),
            S(  5,  50),   S( 21,  24),   S( 28,  26),   S( 41,  25),   S( 40,  25),   S( 34,  19),   S( 30,  14),   S( 17,  21),
            S( 12,  20),   S( 23,   9),   S( 21,  28),   S( 48,  10),   S( 12,  17),   S( 39,  29),   S( 29,   6),   S( 19,  -7),
            S( 42,  10),   S( 28,  19),   S(-12, -19),   S( 15,  30),   S( 32,  -6),   S( 31,  -6),   S(-30,  13),   S( -4, -17),
            S(-85, -55),   S(-21,  -9),   S( -4,  18),   S(  5,  31),   S(-11,   5),   S(-23, -18),   S( -2,  -1),   S(-31, -27),

            /* knights: bucket 2 */
            S(-55,  14),   S( -8,  21),   S(-37,  50),   S(-37,  51),   S(-49,  60),   S(-43,  65),   S(-22,  30),   S(-22,  23),
            S(-14, -13),   S(-24,  14),   S(-15,  14),   S(-12,  30),   S(-10,  24),   S(-16,  49),   S(-40,  58),   S(-38,  73),
            S(-18,  26),   S( -5,  10),   S(-12,  27),   S( 12,  20),   S(  3,  30),   S(  2,  11),   S( -4,  40),   S(-27,  33),
            S(-11,  42),   S(-21,  37),   S(  0,  40),   S(  7,  46),   S( -3,  45),   S( -3,  35),   S(  1,  40),   S( -4,  44),
            S( 20,  23),   S(-16,  34),   S( -5,  44),   S(-17,  53),   S(  5,  45),   S( -6,  40),   S(  7,  32),   S( -2,  23),
            S(-19,  35),   S(  4,  31),   S(-24,  48),   S(-15,  42),   S(-27,  42),   S(  8,  20),   S(-27,  12),   S( 19,   4),
            S(-12,  25),   S(-28,  17),   S(-28,  17),   S(-34,  34),   S(-10,  14),   S(  7,  20),   S(-45,  40),   S(-27,  15),
            S(-133,  33),  S( -2,   4),   S(-77,  35),   S(-26,  14),   S(  0,  12),   S(-56,   5),   S(  0,   5),   S(-166, -42),

            /* knights: bucket 3 */
            S(-46,  -1),   S(  9, -26),   S(-30,  -4),   S( -3,  -9),   S(  0,  -5),   S(-18,   6),   S( 23, -19),   S( -7, -15),
            S(-10,   4),   S(-28,  -6),   S(-16, -12),   S(  8,   8),   S( 18,  -5),   S( -5, -10),   S(  0, -13),   S(-16,  63),
            S(  2, -31),   S(  6,  -5),   S(  2,  -3),   S( 15,   5),   S( 22,  17),   S( 23,   1),   S( 18,   2),   S( 13,  29),
            S(  2,   2),   S( 13,  10),   S( 20,  29),   S( 26,  29),   S( 30,  29),   S( 27,  31),   S( 30,  19),   S( 26,  22),
            S( 29,   7),   S(  9,  18),   S( 37,   9),   S( 33,  38),   S( 32,  34),   S( 36,  42),   S( 46,  37),   S( 22,  14),
            S(  7,  12),   S( 34, -12),   S( 51,  -6),   S( 63,   0),   S( 75, -22),   S( 81, -16),   S( 17,   9),   S( 17,  44),
            S( 33,  -2),   S( 17,  10),   S( 49, -23),   S( 53,  -8),   S( 71, -32),   S( 68, -37),   S( 69, -63),   S( 56, -17),
            S(-99,  26),   S(-23,  13),   S(-24,   6),   S( 10,  18),   S( 41,  -7),   S(  2, -10),   S( -3, -16),   S(-55, -32),

            /* knights: bucket 4 */
            S( 14,  21),   S(-53,   6),   S( 15,  29),   S( -7,  -7),   S(-24, -15),   S(-33, -26),   S(-13, -53),   S(-28, -40),
            S( 34,  27),   S(-26,  38),   S(  8, -25),   S(  2,  -8),   S( 12, -20),   S(-11, -47),   S(  8,  -3),   S(  1, -47),
            S( -7,  30),   S(  4,  38),   S(  5,   7),   S( 12,  11),   S( -8,  -2),   S(-46,  12),   S(-47, -33),   S(-31, -54),
            S(  0,  66),   S( 36, -20),   S( 52,  19),   S( 30,  20),   S( 22,  10),   S( 98, -16),   S( 26, -30),   S(  1, -19),
            S( 65,  36),   S(-12,  45),   S( 52,  44),   S( 47,  20),   S( 42,  36),   S(-11,  25),   S( -2, -24),   S(-11,  -8),
            S( 10,  21),   S(-26,   2),   S( 84,  15),   S( 11,   6),   S( 11,  16),   S( 23,  18),   S( 10,  28),   S(-10, -19),
            S( -5,   9),   S(-15,   9),   S( 13,  -2),   S(  5,  37),   S(  8,   8),   S(  7, -15),   S(  3,  -9),   S(-15,  -3),
            S(-10,  -4),   S(  0,  -2),   S(  9,  10),   S(  1,   6),   S( -6,  -8),   S( 10,  22),   S( -1,   6),   S( -3, -17),

            /* knights: bucket 5 */
            S( 13,   9),   S(-44,  51),   S( 25,  38),   S( 11,  50),   S( 27,  22),   S(  6,  -1),   S( -2,  15),   S(-19, -12),
            S( 14,   7),   S( 29,  52),   S( 10,  24),   S(-20,  41),   S( 23,  35),   S( -5,  34),   S( 17,  29),   S(-14, -25),
            S(  0,  30),   S(-17,  41),   S( 55,  19),   S( 41,  40),   S(-21,  49),   S( -7,  26),   S(-21,  18),   S(  6,  -2),
            S( 36,  50),   S( 11,  48),   S( 38,  40),   S(  6,  56),   S( 22,  45),   S( 19,  42),   S( 25,  45),   S( 13,  38),
            S( 25,  55),   S( 36,  34),   S( 53,  50),   S( 71,  41),   S( 88,  45),   S( 32,  43),   S( 41,  37),   S( 40,  33),
            S(  5,  35),   S(  1,  51),   S( 25,  29),   S( 18,  54),   S( 42,  41),   S( 16,  53),   S( 22,  15),   S( -4,  34),
            S( 20,  59),   S( -6,  66),   S( 29,  46),   S( 16,  63),   S(  6,  52),   S(  7,  45),   S( 22,  70),   S(  3,   4),
            S(  1,  17),   S(  0,  19),   S(  9,  43),   S( -3,   8),   S( 10,  44),   S(  2,  37),   S(  9,  43),   S(-16, -11),

            /* knights: bucket 6 */
            S(  2, -34),   S(-28,  -2),   S( 22,  29),   S(-36,  42),   S(-42,  56),   S(  2,  43),   S(-14,  39),   S(-12,  38),
            S( -4, -25),   S( 44,   3),   S(  6,  12),   S(-43,  40),   S(-68,  69),   S( 18,  50),   S( 13,  52),   S(  0,  17),
            S(-31, -14),   S( -1,   2),   S(-10,  24),   S( 20,  31),   S(-20,  59),   S(-43,  57),   S(  4,  49),   S( -6,  45),
            S( 35,   9),   S( 36,  13),   S( 53,  27),   S( 80,  24),   S( 28,  46),   S( 21,  50),   S( 15,  59),   S(-18,  78),
            S(  1,  39),   S( 69,  -6),   S( 60,  34),   S( 81,  29),   S( 95,  34),   S( 89,  33),   S( 20,  60),   S( 20,  57),
            S( 25,  28),   S( 13,  15),   S( 69,  18),   S( 54,  40),   S( 61,  46),   S( 36,  33),   S( 23,  42),   S( 41,  45),
            S(-23,  26),   S( -2,  37),   S(-30,  37),   S( 28,  31),   S(  1,  57),   S( 20,  42),   S( 19,  73),   S( -7,  34),
            S(-38,   9),   S( 15,  45),   S( 29,  41),   S( 10,  40),   S( 23,  37),   S( 11,  62),   S( 22,  64),   S( 13,  32),

            /* knights: bucket 7 */
            S(-31, -49),   S(-199, -45),  S(-78, -48),   S(-64, -15),   S(-47,  -8),   S(-40, -12),   S(-14,   9),   S(-14,  12),
            S(-49, -74),   S(-45, -46),   S(-40, -34),   S(-60,   4),   S(-53,  12),   S( -5, -11),   S(-22,  49),   S(  1,  32),
            S(-85, -64),   S(-60, -36),   S(-55,  -3),   S( 21, -21),   S(-21,   5),   S(  3,   6),   S(-17,  57),   S( 42,  56),
            S(-60, -19),   S( 18, -24),   S( -3,   8),   S( 36,  -3),   S( 51,  -1),   S( 21,  12),   S( 17,  14),   S(-18,  34),
            S(-58, -21),   S(-22, -26),   S( 50, -23),   S( 86, -19),   S(108,  -6),   S( 72,  20),   S( 95,   1),   S( 82,  26),
            S( -7, -36),   S( 14, -38),   S(-18,  -5),   S( 32,  -3),   S( 70,   6),   S( 79,   5),   S( 59, -14),   S(  0,  16),
            S(-34, -31),   S(-67, -18),   S(  4, -15),   S( 32,  17),   S( 36,  21),   S( 40,   0),   S(-18,  24),   S(  4,   9),
            S(-34, -19),   S( -8,  -5),   S(-26, -10),   S(  8,  13),   S( 12,   7),   S( 23,  21),   S( -3,  -7),   S(  0,   0),

            /* knights: bucket 8 */
            S( -1,  -6),   S( -9, -10),   S( -3,  -3),   S(-10, -31),   S(-11, -40),   S(-10, -51),   S( -2,  -1),   S( -5, -21),
            S(  2,   1),   S( -6, -10),   S( -8, -31),   S(-19, -44),   S(-31, -30),   S(-17, -70),   S(-13, -59),   S(-17, -37),
            S(  4,  17),   S(-22, -19),   S( 19,   2),   S(  4,  -6),   S(  2, -34),   S(-17, -18),   S(-13, -38),   S( -7, -39),
            S(-17,   0),   S(  0,  -4),   S( -1,  10),   S(  4,  29),   S(  6,  -6),   S(  7,   6),   S(-13, -50),   S( -2, -15),
            S( 27,  55),   S( 11,   9),   S( 15,  32),   S( 35,  15),   S( 11,  29),   S( -5,  -3),   S(  5, -20),   S( -7,  -7),
            S( 13,  37),   S( 10,   7),   S( 28,  21),   S( 33,  14),   S(  2,  -1),   S( -2,  -9),   S( -7, -30),   S( -6,  -9),
            S(  5,  18),   S(  1,   5),   S(  6,   9),   S( 11,   8),   S(  6,   7),   S(  5,  20),   S(  2,  13),   S( -1,   3),
            S(  2,   3),   S( 11,  34),   S(  5,  17),   S( -1,   1),   S(  3,  12),   S( -5, -18),   S(  3,   5),   S( -3,  -4),

            /* knights: bucket 9 */
            S( -9, -25),   S(-20, -36),   S(-18, -46),   S( -4, -16),   S(-24, -51),   S(-15, -41),   S( -4, -17),   S( -3, -23),
            S(-12, -36),   S(-12,   2),   S(-11, -50),   S(-14,  -8),   S( -6, -16),   S( -7, -34),   S( -6,  -4),   S(-15, -41),
            S(  5,   8),   S(-10, -13),   S(  1, -21),   S(  2,   1),   S(  2,  13),   S(-33,  -6),   S(-13, -12),   S( -8, -15),
            S(-14,  -1),   S( -6,  -7),   S(  5,  24),   S( 17,  28),   S( 28,  20),   S(  9,  21),   S(-12, -35),   S( -3,   0),
            S( -1,  24),   S( 18,   8),   S( 19,  35),   S(  2,  38),   S( 11,  14),   S( 12,  -5),   S(  2, -28),   S(  5,  11),
            S(  1,   4),   S(  7,  31),   S( 15,  28),   S( -6,  15),   S( 35,  32),   S( 17,  10),   S(  8,  15),   S( -6, -21),
            S(  1,   2),   S( -1,  24),   S( 18,  37),   S( 12,   3),   S( 14,  41),   S( -2, -17),   S(  4,  18),   S( -2,   1),
            S(  2,   2),   S(  4,   9),   S( 13,  28),   S( 16,  33),   S(  9,  11),   S(  1,   6),   S(  3,   4),   S(  0,  -3),

            /* knights: bucket 10 */
            S(-17, -45),   S(-17, -53),   S(-13, -26),   S(-18, -20),   S(-12, -10),   S(-15, -43),   S( -3,  15),   S(  4,  22),
            S( -6, -23),   S( -6, -12),   S( -1, -16),   S(-20, -34),   S(-25, -35),   S( -8, -40),   S( -8,  -5),   S( -5, -12),
            S(-17, -48),   S(-19, -62),   S(-11, -18),   S(-16, -19),   S( 13,  -2),   S(-15,  -6),   S( -6,   2),   S( -7,   7),
            S( -8, -17),   S( -6, -44),   S(  3, -37),   S( 18,   9),   S( 11,  35),   S( 19,  19),   S(  6,  15),   S( 11,  45),
            S( -8, -44),   S(-12, -27),   S( 15,   8),   S( 24,  27),   S( 20,  48),   S(  1,  23),   S( 20,  14),   S( 23,  51),
            S(-10, -39),   S( -5, -21),   S( -4, -13),   S( 13,  36),   S( 36,  56),   S( 31,  36),   S( 27,  58),   S( 18,  57),
            S(  0,  -1),   S( -9, -30),   S(  1,  -7),   S( 27,  26),   S( 17,  28),   S(  9,  32),   S(  1,   0),   S( 10,  28),
            S( -3, -14),   S(  3,  11),   S( -7, -17),   S(  4,   1),   S( 12,  39),   S(  5,  27),   S(  3,  15),   S(  0,   0),

            /* knights: bucket 11 */
            S(  0,   2),   S(-20, -29),   S( -9, -46),   S(-10, -25),   S(-21, -48),   S(-12, -17),   S( -6,  -4),   S( -4,  -5),
            S( -7,  -6),   S(-13, -21),   S(-15, -78),   S(-31, -27),   S(-10,  -5),   S(-29, -37),   S(-16, -28),   S( -7,  -8),
            S(-15, -52),   S(-24, -60),   S(-27, -37),   S( -1,  -2),   S(-15,   0),   S(-19,  13),   S(  8,  -6),   S( -1,  16),
            S(-12, -27),   S( -7, -29),   S(-26,  -7),   S( 26,  27),   S( 15,  15),   S( 18,   7),   S( 15,  24),   S( 15,  32),
            S( -3, -23),   S(-18, -57),   S(  6, -21),   S(  1,   5),   S( 15,  19),   S( 34,  52),   S(  7,  -2),   S( 25,  66),
            S( -7,  -9),   S( -7, -28),   S(  0,  -7),   S( 40,  32),   S( 17,  21),   S( 48,  44),   S( 22,  22),   S( 14,  28),
            S(  9,  27),   S( -2,  -6),   S(  7, -13),   S( 12, -16),   S( 20,  30),   S( -1,   3),   S( 16,  40),   S( 20,  56),
            S( -3,   1),   S( -2, -17),   S(  9,  14),   S(  2,   7),   S(  2,  13),   S(  3,   6),   S(  4,   6),   S(  2,  13),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   2),   S( -2, -20),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   5),   S( -2, -13),
            S(  0,   0),   S(  1,   2),   S(  2,   6),   S( -4, -12),   S( -2,   6),   S( -4, -21),   S( -2, -11),   S(  1,  10),
            S( -5, -13),   S(  4,   4),   S( -6, -13),   S( -6, -23),   S(  0,   2),   S( -6, -18),   S(  2,  -5),   S( -7, -30),
            S( -7, -13),   S( -1,   1),   S( -9, -24),   S(  4,  15),   S( -6,  -7),   S(  0,   5),   S( -1,  -6),   S( -1,  -7),
            S(  9,  17),   S(  5,   3),   S( -6, -13),   S(  0,   3),   S( -6, -27),   S(  0,   3),   S( -1, -13),   S( -1,   1),
            S(  1,  -8),   S( -2, -21),   S(  1,  -1),   S( -1,  -6),   S(  5,  11),   S( -5, -18),   S( -1,  -7),   S(  0,   3),
            S(  2,   8),   S( -9,  -9),   S(  0,  10),   S(  2,  -8),   S( -5,  -8),   S( -5, -21),   S( -2,  -1),   S(  0,  -2),
            S(  2,   4),   S(  2,  13),   S( -2,  -4),   S(  2,  -1),   S( -2,  -4),   S( -2,  -9),   S( -3,  -8),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -1,  -5),   S( -2,  -3),   S( -8, -14),   S( -1,   1),   S( -3, -12),   S(  1,  -1),
            S( -2,  -6),   S(  1,   4),   S( -2, -23),   S(-11, -22),   S( -6, -31),   S( -4, -24),   S(  0,   0),   S(  1,  -1),
            S( -3,  -9),   S( -8, -31),   S(  6,  15),   S(  0,  -1),   S(-13, -40),   S(-10, -25),   S( -2, -11),   S( -6, -28),
            S( -8, -15),   S(  5,  13),   S(  1,   0),   S(-11, -28),   S( -2,  -9),   S(  6,  11),   S(  0, -13),   S( -4, -10),
            S(  3,  11),   S( -1,  -2),   S(  2,  -8),   S( 10,  18),   S(  5, -13),   S( -3,  -9),   S(  2, -12),   S(  1,   0),
            S( -3,  -8),   S( 14,  14),   S(  7,  22),   S(-13,  10),   S(  5,   4),   S( -9, -33),   S(  4,   6),   S( -3,   3),
            S(  1,   7),   S(  2,   4),   S( 10,  11),   S(  8,  10),   S( 14,  22),   S( -5, -21),   S( -2,  -1),   S( -5,  -3),
            S( -1,   2),   S( -1,  -5),   S(  0,   1),   S(  1,  -8),   S( -1,   0),   S(  3,  -1),   S(  0,  -1),   S( -1,   1),

            /* knights: bucket 14 */
            S( -3, -23),   S( -5, -24),   S( -1,  -1),   S( -3,   4),   S( -8, -24),   S( -2, -15),   S( -1,  -5),   S(  0,   2),
            S(  0,  -2),   S( -3,  -8),   S(-15, -59),   S( -8, -36),   S( -1,  -9),   S(  1,   6),   S(  1,  -3),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-11, -56),   S(  1,   1),   S( -4, -20),   S( -4, -10),   S(  0,  -1),   S(  2,  10),
            S(  0,   5),   S( -6, -32),   S(-15, -41),   S(-11, -39),   S( -3, -21),   S(  2,  -1),   S( -3, -15),   S( -7, -11),
            S( -2,  -4),   S( -2, -15),   S(  0,  20),   S( -7, -34),   S( -9,  -8),   S(  3,  22),   S(  3,   6),   S( -4,  -4),
            S( -4,  -8),   S(  4,  -2),   S( -9, -32),   S(  4,   0),   S( 15,  22),   S(  4,   8),   S( -3,   0),   S(  1,  -2),
            S(  0,  -3),   S( -2, -10),   S(  7,  -4),   S(  0,  -9),   S( -7, -10),   S( -3,  -9),   S( -6,  -4),   S(  1,   8),
            S(  0,  -2),   S(  2,   4),   S( -1,  -9),   S(  7,  -1),   S(  5,  18),   S(  1,   3),   S( -2,  -6),   S( -1,  -5),

            /* knights: bucket 15 */
            S( -2,  -7),   S( -1, -12),   S( -1, -13),   S( -7, -13),   S( -2,  -1),   S( -1,  -4),   S(  1,   0),   S(  0,  15),
            S( -2,  -5),   S(  0,  -2),   S( -5, -19),   S( -6, -25),   S( -2,  -5),   S( -1,  -8),   S(  0,   0),   S( -1,  -3),
            S( -6, -16),   S( -7, -16),   S( -3, -12),   S(-15, -41),   S( -6, -25),   S( -2,  -4),   S( -1,   0),   S( -2,   2),
            S( -6, -16),   S( -6, -31),   S( -6, -20),   S( -1, -10),   S(  0, -18),   S(  7,  22),   S(  5,  10),   S( -3,  -1),
            S(  0,  -1),   S( -2,  -5),   S( -1, -17),   S( -7, -12),   S(  3,  17),   S(  4,  10),   S( -6,  -7),   S( -1,   3),
            S( -3,  -3),   S( -2,  -4),   S( -2, -22),   S( -3,   6),   S( -5, -14),   S( -7,   9),   S( -3,   5),   S(  2,   9),
            S( -3, -12),   S( -2,  -6),   S( -1,  -8),   S( -4,  -8),   S(-10, -13),   S( -4,  15),   S( -2,  -7),   S(  3,  13),
            S(  0,  -3),   S(  0,  -1),   S( -3,  -9),   S( -2,  -8),   S( -2,  -4),   S( -9,  -4),   S(  6,  17),   S( -2,   2),

            /* bishops: bucket 0 */
            S( 21,  11),   S( 19, -12),   S( 39,  17),   S(  6,  20),   S( -5,  -3),   S( 18,  -7),   S( 25, -37),   S(  5, -34),
            S( 56, -40),   S( 80,  -3),   S( 40,   5),   S( 17,   2),   S(-12,  34),   S( -1, -21),   S(-37,  -5),   S( 14, -46),
            S( 28,  45),   S( 46,   7),   S( 26,   3),   S( 14,  50),   S( 20,  11),   S(-30,  20),   S(  9, -25),   S( 18, -41),
            S( 22,  10),   S( 69,  -9),   S( 40,  13),   S( 37,  35),   S(  4,  33),   S( 28,   1),   S( -2, -10),   S(  3,   2),
            S( 18,   4),   S( 33,  25),   S(  6,  41),   S( 59,  16),   S( 65,  -1),   S( 19,  -3),   S( 23, -16),   S(-30,   1),
            S(-37,  62),   S( -1,  20),   S( 65, -24),   S( 90, -23),   S( 41,  32),   S( -4,   0),   S(  2,  13),   S(  2,  13),
            S(-11,  16),   S(  9,  -3),   S( 43,  -7),   S(  2,  35),   S(-32,  -3),   S( 28,  27),   S(  2, -12),   S(-10,  -9),
            S(-32, -41),   S( 11,   2),   S(  1,   8),   S(  8, -10),   S( 22,  26),   S( 35,  11),   S(  0,  47),   S(-20,   3),

            /* bishops: bucket 1 */
            S( 42,  13),   S( -5,  33),   S(  5,  41),   S( 11,  29),   S( -7,  29),   S( -2,  30),   S(-10,   3),   S(-41,  -3),
            S( 15,  -9),   S( 35, -17),   S( 54,   5),   S( 29,  28),   S( -8,  15),   S(  7,  -3),   S(-33,  -5),   S( 19, -12),
            S( 44,  -2),   S( 12,   7),   S( 34, -10),   S( 19,  24),   S( 22,  24),   S(-21,   0),   S( 25,  -7),   S(  9, -29),
            S( 41,   6),   S( 21,  18),   S( 12,  14),   S( 36,  23),   S(  5,  24),   S( 24,   6),   S( -3,   7),   S( 15, -10),
            S( 39,  32),   S( 11,  23),   S( 21,  25),   S(  0,  33),   S( 28,  12),   S(  2,  17),   S( 31, -15),   S( -8,  14),
            S( -2,  23),   S( 30,  37),   S( 33,   0),   S( 56,  -7),   S( 18,  18),   S( 36, -17),   S(  1,  30),   S( 52, -13),
            S( -7,  48),   S(-28,  19),   S( 19,  27),   S( 35,  24),   S( 40,  24),   S(-19,  22),   S( 34, -21),   S(-15,  40),
            S( 15,   3),   S(  9,   6),   S(  3,  13),   S(-21,  21),   S( 22,  17),   S( -8,   6),   S( 13,   7),   S( -3,  13),

            /* bishops: bucket 2 */
            S( 19, -12),   S(  7,  17),   S( -6,  15),   S(-29,  51),   S(-13,  36),   S(-31,  30),   S(-19,  -2),   S(-44,  23),
            S(-20,  27),   S(  2, -17),   S( 21,   9),   S( -2,  24),   S( -2,  33),   S( 17,   6),   S(-11, -20),   S(  9, -25),
            S( -6,   7),   S( -4,  11),   S(  4,   5),   S( -3,  46),   S(  5,  36),   S( -2,  14),   S( 14,  12),   S(-15,   3),
            S(  3,  11),   S( -8,  14),   S(-13,  37),   S(  7,  37),   S(  0,  40),   S(  8,  24),   S(  7,  16),   S(  3,   9),
            S( 12,   5),   S(-15,  35),   S( -8,  27),   S(-30,  46),   S( -9,  38),   S( -8,  46),   S(  3,  23),   S(-28,  35),
            S(  7,  29),   S( -4,  16),   S(-23,  26),   S(-16,  25),   S( 11,  12),   S( -4,   4),   S( -1,  55),   S(  2,  25),
            S(  4,  21),   S(-22,   9),   S(-26,  53),   S( 19,   1),   S( -1,   2),   S(-17,   8),   S(-67,   9),   S(-30,  36),
            S(-53,  30),   S(-37,  45),   S(-23,  28),   S(-41,  26),   S(-48,  38),   S(-34,  18),   S(  7,  14),   S(-67,   7),

            /* bishops: bucket 3 */
            S(  3,   6),   S( 34,  -5),   S( 27,  17),   S( 17,  18),   S( 19,   6),   S( 41,  -8),   S( 43, -26),   S( 48, -67),
            S( 14,   9),   S(  6,  -1),   S( 31,  -5),   S( 12,  30),   S( 23,   7),   S( 23,  20),   S( 44, -10),   S( 46,  -3),
            S( 23,  10),   S( 14,  15),   S( 11,  13),   S( 27,  20),   S( 24,  49),   S( 23,   6),   S( 39,  23),   S( 50,  -9),
            S( 33,  -6),   S( 27,   9),   S( 19,  32),   S( 27,  45),   S( 33,  35),   S( 32,  32),   S( 30,  21),   S( 26,  -2),
            S( 20,   5),   S( 27,  14),   S( 44,  14),   S( 29,  45),   S( 25,  46),   S( 37,  26),   S( 20,  34),   S( 27,  33),
            S( 30,   4),   S( 36,  22),   S( 26,  10),   S( 43,  14),   S( 26,  19),   S( 56,   0),   S( 51,  13),   S(  8,  69),
            S( 20,   9),   S( -7,  14),   S( 43,  23),   S( 23,  16),   S( 16,  15),   S( 24,   0),   S(  2,  23),   S( 21,  37),
            S(-32,  60),   S(  0,  35),   S( 57,  11),   S( 29,  16),   S( -9,  34),   S(  5,  33),   S( 33,  -2),   S( 66, -30),

            /* bishops: bucket 4 */
            S(-23, -24),   S(-22,  10),   S(-36,  -1),   S(-23,  21),   S(-25,  27),   S(-50,  26),   S(  1, -11),   S(-12, -12),
            S( -4,  15),   S(  6,   6),   S(-10,  37),   S(-31,  17),   S(-19,  -6),   S( 39,  -7),   S(-30, -11),   S( 13,  -3),
            S( -8,   5),   S(-33,  35),   S( 14, -20),   S(-25,  14),   S(  1,  26),   S( 22, -24),   S(-29, -11),   S(-53,  -1),
            S(-32,  28),   S(  0,  33),   S( 50,  28),   S( 30,  35),   S( 12,  22),   S( 52,  -9),   S( 47,  -7),   S(-11, -36),
            S( 11,  20),   S(  4,  45),   S(-14,  53),   S( 21,  42),   S( 36,  11),   S( 33, -18),   S(-10, -20),   S( 13, -10),
            S( -6,  36),   S( 22,  16),   S(-12,  27),   S( 21,  14),   S( 41,   9),   S(  7, -12),   S( 19, -35),   S(  5,  -5),
            S(-16,   9),   S( 27,  13),   S( 13,  18),   S( 25,  18),   S( 11,  -3),   S(  3,  18),   S( -1,   4),   S(  7, -23),
            S( 10, -18),   S(-10, -36),   S(  2,  -3),   S( -4,   0),   S(  7, -10),   S(  2,  11),   S(  1,  -3),   S( -5,   2),

            /* bishops: bucket 5 */
            S(-10,  -4),   S(-13,  43),   S(-51,  35),   S(-29,  33),   S(-42,  34),   S(-10,  20),   S( -4,  16),   S(-23,  14),
            S(-25,  40),   S(-15,   8),   S(-34,  60),   S( -1,  30),   S(-31,  38),   S(-29,  26),   S(-36, -15),   S(-10,   1),
            S(  1,  16),   S(  1,  38),   S( 19,  13),   S(-20,  50),   S( -5,  38),   S(-35,  -2),   S(-30,  32),   S(-19,   7),
            S( 32,  14),   S( 25,  28),   S(-10,  56),   S( 29,  30),   S( 30,  36),   S( 18,  31),   S( 16,  -5),   S(  9,  27),
            S( 34,  47),   S( 38,  15),   S( 54,  30),   S( 81,  31),   S( 50,  21),   S( 44,  19),   S( 36,  14),   S( -7,   6),
            S( 23,  45),   S( 30,  46),   S( 34,  21),   S( 29,  34),   S(  0,  34),   S( 18, -17),   S(-24,  48),   S( -1,  34),
            S(  3,  43),   S(-33,  12),   S( 11,  40),   S(  5,  50),   S( 27,  28),   S( 32,  38),   S( -1,  20),   S(  1,  35),
            S( -2, -11),   S( 15,  34),   S( 15,  13),   S(  6,  37),   S(  2,  56),   S( 13,  24),   S( 30,  56),   S( -5,   5),

            /* bishops: bucket 6 */
            S( -8,  16),   S(  0,  29),   S(-39,  37),   S(-41,  38),   S(-42,  25),   S(-46,  35),   S(-20,  57),   S(-13,  17),
            S( 23,   9),   S( -3, -12),   S(-23,  30),   S( -7,  33),   S(-31,  45),   S(-19,  26),   S(-103,  32),  S( 17,  27),
            S( 24,   0),   S(  6,   8),   S( 24,  -3),   S( 19,  28),   S( 39,  22),   S( 13,   8),   S(  3,  32),   S(-37,  22),
            S(-13,  42),   S( 17,  14),   S( 34,  21),   S( 30,  34),   S( 40,  32),   S( 37,  27),   S( 35,  31),   S(-13,   1),
            S(-12,  23),   S( 53,   6),   S( 26,  26),   S( 53,  23),   S( 97,  27),   S( 60,  25),   S( 39,  30),   S(-24,  50),
            S(  4,  12),   S(-43,  49),   S( 10,  18),   S( 16,  40),   S( 37,  29),   S( 26,  26),   S(  5,  48),   S( -9,  51),
            S(-22,  36),   S(-30,  25),   S(  0,  40),   S(-11,  32),   S( 44,  21),   S( 21,  28),   S(-12,  31),   S( -1,  37),
            S(  9,  51),   S( 14,  37),   S(  9,  41),   S(  1,  47),   S(-17,  38),   S( 32,  18),   S( 11,  23),   S( 12,   6),

            /* bishops: bucket 7 */
            S(-15, -37),   S( -3,   3),   S(-38, -27),   S(-55,  11),   S(-32,  -7),   S(-76,  20),   S(-71, -31),   S(-63,   7),
            S(-32, -26),   S(-59, -41),   S(-21,  -7),   S( -1, -15),   S(-34,   2),   S(-45,  16),   S(-51, -12),   S(-31,  12),
            S(-34, -19),   S(  3, -18),   S( 19, -40),   S( 18,  -2),   S(-33,  18),   S(-19, -20),   S(-34,  45),   S(-23,  30),
            S(-40,  17),   S( 55, -36),   S( 75, -22),   S( 57,   4),   S( 80,   1),   S(  5,  22),   S( 26,  29),   S( -3,  29),
            S( 24, -48),   S(-12, -18),   S( 65, -36),   S(102, -27),   S( 70,  24),   S( 73,  15),   S( -3,  41),   S( 27,  11),
            S(-28, -11),   S(-27,   4),   S( 29, -45),   S( 23,  -3),   S( 51, -11),   S( 52,   3),   S( 53,  16),   S( 24,   3),
            S( -2, -13),   S(-42,  -9),   S(  8,  -1),   S( 13,  -6),   S( 14, -22),   S( 35,  -9),   S(  9,  -4),   S( 14,  14),
            S(-14,  -7),   S( -8,  19),   S(-29,  12),   S(  5,  -3),   S( 10,  -3),   S( 19,  -5),   S( 25,   9),   S(  5,   7),

            /* bishops: bucket 8 */
            S(-10,  -8),   S(-12, -32),   S(-44,  -5),   S( -4, -25),   S( -5,  20),   S(-24,  -2),   S(  6,  23),   S( -5, -10),
            S( -7,  -1),   S(-33, -49),   S(-13, -23),   S(-16,  -3),   S( 10,  -8),   S(-17, -28),   S(-20, -55),   S( -5,  -8),
            S(  2,   2),   S(-11,   9),   S(-25,   7),   S(-10,  17),   S( -7,  11),   S( -8, -42),   S(  5, -44),   S(-31, -37),
            S(  8,  35),   S( -5,  44),   S(  8,  40),   S( -3,  15),   S( 19,  21),   S( -2,   7),   S(  4, -17),   S( -5, -17),
            S( 16,  38),   S( 12,  65),   S( -9,  32),   S( 48,  44),   S(  3,  23),   S( 17,  10),   S(  7, -30),   S(-10, -15),
            S( -1,  10),   S( 12,  36),   S(  9,  19),   S(-14,  19),   S( 28,  10),   S( -9, -17),   S(-14, -13),   S(-18, -20),
            S( -3,   7),   S(  8,  24),   S( 10,  23),   S(  2,   2),   S(  5,  11),   S( -2,  20),   S(-13, -15),   S( -9, -27),
            S( -9, -13),   S(  1, -27),   S(  0,  -4),   S(  0, -13),   S(-18,  -8),   S( -5,  -5),   S(  0,  14),   S( -8,   8),

            /* bishops: bucket 9 */
            S(-25, -32),   S( -6,   3),   S(-21,   3),   S(-11, -23),   S(-36, -28),   S(-19, -36),   S(-16,  -8),   S(  9,   0),
            S(-14, -15),   S(-39, -34),   S( -9,  -8),   S(-16,  16),   S(-48,  29),   S(-19, -15),   S(-17, -18),   S( -5,  -4),
            S(  9,   1),   S( 17,  13),   S(-27, -20),   S(-15,  22),   S(  1,  13),   S(-10, -23),   S(-15, -28),   S( -5,  25),
            S(-14,   9),   S( 16,  18),   S( -9,  26),   S(  8,  21),   S( 19,  26),   S( 11,   4),   S(  4,  -2),   S(-16, -23),
            S( -2,  20),   S( 23,  26),   S(  7,  39),   S( 10,  52),   S(-13,  16),   S(  4,  32),   S( -5,  36),   S( -7,  -6),
            S(-12,   5),   S( 21,  51),   S(  1,  16),   S( 23,  19),   S( 13,  35),   S( -8,  -4),   S(-18,   3),   S(-12, -11),
            S(  6,  18),   S( 18,  12),   S(  4,   8),   S(  2,  45),   S( 21,  39),   S(  8,   6),   S( -8, -17),   S( -5,  -2),
            S( -2, -23),   S( -7,  22),   S( -5,  17),   S(-19, -12),   S(-13,  -2),   S(  8,  29),   S(  2,   5),   S(-13, -18),

            /* bishops: bucket 10 */
            S(-21,  -8),   S(  5, -23),   S(-34, -26),   S(-18, -22),   S(-23,  -9),   S(-24, -20),   S(-12, -21),   S(-18, -26),
            S(  6, -16),   S(-28, -38),   S( -6,  -8),   S(-41,   4),   S(-39,   6),   S(-20,  21),   S(-31, -58),   S(-11, -14),
            S(  9, -12),   S(  2, -11),   S(-38, -49),   S(  2,   8),   S(-37,  30),   S(-40,  11),   S(-21,  28),   S(  6,  19),
            S(-10, -21),   S(  4,   8),   S( 12,  -5),   S( 15,   7),   S( 10,  28),   S(-10,  56),   S(  4,  31),   S( 16,  28),
            S(-18,   0),   S(  2,   2),   S( -3,  16),   S( 35,  27),   S(  2,  63),   S( 22,  50),   S(  9,  41),   S(  1, -15),
            S(  3, -25),   S(-24,  -1),   S(-24, -13),   S(-11,  28),   S( 26,  35),   S( 35,  23),   S( 10,  53),   S(  2,  10),
            S(-21,  -6),   S(-13, -49),   S( -9,  -8),   S( 22,  15),   S( -4,  -4),   S( 18,  36),   S( 16,  37),   S( 14,  17),
            S( -7, -30),   S( -8,   8),   S(  7,  20),   S(-11,   3),   S( -8,  14),   S(-10,  -9),   S( 10,   2),   S(  6,  25),

            /* bishops: bucket 11 */
            S(-21,   0),   S(-31, -12),   S(-51, -45),   S(-22, -29),   S(-21, -10),   S(-66, -46),   S( -9, -11),   S(-23, -22),
            S(-12, -18),   S( -3, -41),   S( -8,  -8),   S(-24, -36),   S(-45, -10),   S(-29, -27),   S(-26, -44),   S(-22, -35),
            S( -9, -48),   S(  2, -48),   S(-30, -26),   S(  1,  -9),   S( -4,  -7),   S(-37,   9),   S( -9,  24),   S( -1,  19),
            S(-16, -36),   S(-11, -35),   S(  6,  -9),   S(  5,  -7),   S( 14,  24),   S( -2,  60),   S(  7,  49),   S( 19,  27),
            S( -9, -22),   S(-17, -43),   S(-15,  22),   S( 49,   4),   S( 36,  39),   S(  2,  59),   S( 19,  55),   S( 14,  26),
            S(-18, -49),   S(-31,  -2),   S(-13, -38),   S(  9,  14),   S(  3,  34),   S( 18,  28),   S( 30,  37),   S( -1,   3),
            S( -8,  -7),   S(-20, -46),   S(-20,   0),   S( -6, -16),   S( 10,   0),   S( 35,  16),   S( -9,   2),   S( 16,  31),
            S(-19, -13),   S(-20,  -1),   S( -6,  12),   S( 10,   6),   S( 12,   3),   S(-17, -23),   S(  4,   8),   S( -2, -21),

            /* bishops: bucket 12 */
            S(  0,   3),   S( -7, -13),   S(-12, -28),   S( -7, -26),   S( -9, -18),   S(-11, -21),   S( -1,  10),   S( -5,   1),
            S( -7,  -7),   S(-13, -33),   S( -7, -13),   S( -6, -12),   S(-14, -23),   S( -2,  14),   S( -3,  -3),   S( -1,  -9),
            S( -1,  -4),   S(-15,  -2),   S(-12, -18),   S( -8,  -5),   S( -5,   8),   S( -6, -14),   S(-10, -43),   S( -3,  -5),
            S( -2,   4),   S(  5,   1),   S(-17, -30),   S( -3,  12),   S(  1,   6),   S(  6,  25),   S( -4,  -7),   S( -6,  -3),
            S( -1,  -4),   S(  3,  17),   S( -4,  19),   S( -8,   1),   S( -2,  -3),   S( -4,   4),   S(  4,   6),   S( -7,  -2),
            S(-12, -12),   S(  5,  59),   S(-27,   1),   S( -9,  -3),   S(  7, -15),   S( -4,   3),   S(  0,   5),   S( -1,  -5),
            S( -2,  -5),   S( -6,   9),   S(  3,  14),   S( -7,   5),   S( -1,   8),   S(  8,  16),   S( -7, -18),   S( -1,   5),
            S( -2,  -4),   S(  0,  -6),   S( -6,  -1),   S(  6,   8),   S(  1,   9),   S(  0,   3),   S(-10,  -1),   S(  0,  -3),

            /* bishops: bucket 13 */
            S( -8, -42),   S(-13, -29),   S(-13, -15),   S(-15, -18),   S(-16, -18),   S( -9,   2),   S( -1,  -5),   S( -8, -10),
            S( -4,  -7),   S(-12, -13),   S(-13, -28),   S(-19,  -9),   S(-14,   9),   S( -9,   0),   S( -2, -12),   S(  2,  -2),
            S( -9, -10),   S( -6,  -7),   S( -8,   9),   S(-21,  -2),   S(-12, -23),   S( -3, -13),   S( -3, -30),   S(  5,  21),
            S( -2,   4),   S(-12,  -3),   S(-13,   4),   S(-23,  11),   S(  2,  20),   S(  4,  -6),   S( -1,   4),   S( -6,  -6),
            S( -3,   9),   S(-16,   7),   S(-16,  -2),   S( 20,   0),   S( -6,   4),   S( -5,   6),   S(-10, -15),   S( -2,  -6),
            S( -3,  -4),   S( -8,   2),   S(-20, -16),   S( 11,  16),   S(  3,   9),   S( -3,  -6),   S(  7,  20),   S( -3,  -5),
            S( -6,  -9),   S(-10,  -4),   S(  7,  26),   S( -7,   9),   S( -8,   0),   S(  1,   0),   S(-15, -25),   S(  0,   7),
            S( -8, -18),   S(  0,   9),   S( -1,   0),   S(  5,   1),   S(  0,   6),   S( -8,  -6),   S(  1,  10),   S( -3, -14),

            /* bishops: bucket 14 */
            S( -8, -17),   S(-12, -16),   S(-18, -27),   S(-18, -44),   S(-14, -35),   S( -6, -26),   S(-10, -14),   S(-10, -17),
            S(-10, -26),   S( -2, -23),   S( -8, -14),   S(-26, -41),   S(-10, -11),   S(-18, -10),   S(-15, -22),   S(  1, -13),
            S( -8, -11),   S( -9, -32),   S(-22, -30),   S(-13, -18),   S(-26,  -4),   S(-22, -30),   S( -7,   1),   S( -3,  -2),
            S( -8, -22),   S( -8,  -5),   S(-10,  -4),   S(-22,  20),   S(  1,   7),   S(-21,  13),   S(-19, -15),   S( -5, -11),
            S( -8,  -4),   S( -7,  26),   S( -7, -19),   S( -5, -19),   S(-12,  11),   S( -6,  -5),   S(  7,  23),   S(  2,  -4),
            S( -1,   4),   S( -8,   7),   S(-21, -11),   S( -8, -16),   S(  7,  10),   S(-10,  16),   S( -2,  35),   S( -7, -20),
            S( -6, -22),   S( -1,  -1),   S( -7,   1),   S(  3,  16),   S(-10,  -7),   S( -1,   1),   S( -3, -12),   S( -4,  -6),
            S( -7,  -7),   S( -4,  -6),   S( -3,  -4),   S( -2,   5),   S(-10, -17),   S(  1,   9),   S(  7, -10),   S(  1,   4),

            /* bishops: bucket 15 */
            S(  4,   8),   S(  6,   5),   S(-19, -27),   S(  0,  -9),   S(-11, -15),   S(-12, -23),   S( -6, -13),   S( -2, -10),
            S(  2,   4),   S( -1,  -8),   S(  3,  -1),   S( -9, -11),   S(-14, -21),   S( -6,  -7),   S( -8, -17),   S( -1,   0),
            S( -7, -14),   S(  0,  -2),   S(-13, -10),   S(-11,  -7),   S(-19, -19),   S(-17, -21),   S( -7, -10),   S(  2,  16),
            S( -4,  -7),   S(-16, -17),   S(  7, -12),   S(-22, -29),   S( -4,   8),   S( -9, -14),   S(  4,  16),   S( -1,  -8),
            S( -1,  -9),   S(-12, -16),   S(-13,  -9),   S(-18, -45),   S( -1, -22),   S(-13,  22),   S(  4,  20),   S(-10, -16),
            S( -9, -32),   S(-12, -12),   S(-18, -35),   S(-20, -11),   S( -4,  -3),   S( -9, -26),   S(  8,  39),   S(  1,  13),
            S( -3,   2),   S( -2, -17),   S( -2, -15),   S( -4,   3),   S(-11, -13),   S( -1,   7),   S(-12,  -4),   S(  3,   5),
            S( -3,  -1),   S( -1,   2),   S( -4,   0),   S( -6,  -4),   S( -8,  -5),   S(-16, -19),   S( -9, -24),   S(  1,   0),

            /* rooks: bucket 0 */
            S(-22,   6),   S(-10,   0),   S(-15, -12),   S( -9,  -6),   S(-14,  10),   S( -9,  -7),   S(-15,  22),   S( -3,  19),
            S( 10, -63),   S( 25, -15),   S(  1,  -3),   S( -5,   2),   S( 10,  -2),   S( -2,  -9),   S(-34,  21),   S(-45,  30),
            S(  3, -25),   S( 10,  26),   S( 21,   5),   S(  9,   9),   S(-18,  39),   S( -1,   6),   S(-30,  17),   S(-41,  15),
            S( 26, -21),   S( 62,  -1),   S( 43,  25),   S( 42,   5),   S( 14,   8),   S( -3,  14),   S(-15,  22),   S(-36,  34),
            S( 61, -23),   S( 88, -16),   S( 67,  -3),   S( 39,  -9),   S( 46,   6),   S( 25,   9),   S( -6,  37),   S(-17,  33),
            S( 69, -43),   S(106, -35),   S( 53,   6),   S( 16,  20),   S( 42,  10),   S(-39,  33),   S( 34,  19),   S(-38,  41),
            S( 44,  -9),   S( 70,  -2),   S( 25,   7),   S(  9,  27),   S( -7,  29),   S( -3,  15),   S(-13,  35),   S(-12,  27),
            S( 33,  19),   S( 16,  47),   S( 19,  28),   S( -4,  38),   S(  6,  19),   S(  8,   2),   S( -3,  31),   S(  3,  27),

            /* rooks: bucket 1 */
            S(-77,  35),   S(-55,   6),   S(-57,  -4),   S(-43, -13),   S(-29, -22),   S(-29, -21),   S(-35,  -9),   S(-39,  21),
            S(-43,  12),   S(-57,  18),   S(-17, -13),   S(-26, -32),   S(-30, -14),   S(-40, -14),   S(-42, -19),   S(-60,  14),
            S(  2,   6),   S(-26,  30),   S(-19,  10),   S(-39,  19),   S(-44,  25),   S( -4,  -4),   S(-24,   5),   S(-46,  19),
            S(-51,  52),   S(-34,  31),   S(  6,  16),   S(-12,  19),   S(-23,  29),   S(-41,  39),   S(-35,  37),   S(-31,  14),
            S( 48,  15),   S( 33,  32),   S( 25,   3),   S(-35,  39),   S(-20,  38),   S( 17,  19),   S( -2,  18),   S(-41,  24),
            S( 43,  10),   S(  8,  28),   S(  8,  23),   S(-32,  27),   S( 12,  11),   S(-28,  41),   S(-11,  24),   S(-44,  32),
            S(-16,  30),   S(  8,  26),   S( 22,  26),   S(-46,  48),   S(-24,  33),   S(  4,  32),   S(-36,  27),   S(-54,  34),
            S( 26,  30),   S( 31,  35),   S( -1,  28),   S(-45,  53),   S(  0,  14),   S( 23,  14),   S(-18,  36),   S( -6,  15),

            /* rooks: bucket 2 */
            S(-69,  38),   S(-48,  18),   S(-48,  15),   S(-57,  13),   S(-62,  12),   S(-50,   6),   S(-33, -23),   S(-50,  28),
            S(-80,  46),   S(-63,  36),   S(-46,  25),   S(-54,  10),   S(-42,  -3),   S(-55,   0),   S(-68,  19),   S(-63,  16),
            S(-74,  61),   S(-60,  50),   S(-56,  50),   S(-33,   9),   S(-45,  23),   S(-27,  20),   S(-17,  12),   S(-35,  21),
            S(-72,  63),   S(-57,  65),   S(-38,  60),   S(-33,  46),   S(-26,  31),   S(  4,  30),   S(-34,  51),   S(-20,  32),
            S(-25,  51),   S(-48,  66),   S(-44,  56),   S(-18,  38),   S( 26,  24),   S( 20,  27),   S(-24,  49),   S(-39,  45),
            S(-38,  45),   S(-33,  47),   S(-15,  28),   S( -4,  20),   S( 21,  25),   S( 47,  15),   S( 23,  19),   S(-18,  28),
            S(-52,  41),   S(-67,  68),   S(-34,  52),   S(-11,  47),   S( 12,  26),   S( 25,  19),   S(-52,  59),   S(-34,  46),
            S(-36,  67),   S(-13,  47),   S(-69,  64),   S(-27,  42),   S(-49,  57),   S(-28,  61),   S(-51,  74),   S(-23,  46),

            /* rooks: bucket 3 */
            S( -7,  73),   S( -7,  65),   S( -3,  57),   S(  5,  45),   S(  1,  46),   S(-18,  67),   S( -9,  76),   S( -4,  38),
            S(-33,  84),   S(-12,  64),   S(  2,  56),   S(  6,  51),   S( 16,  44),   S(  9,  54),   S( 39,   3),   S( 19, -36),
            S(-35,  80),   S(-15,  80),   S( -2,  72),   S( 14,  53),   S(  8,  73),   S( 24,  64),   S( 29,  66),   S(  5,  49),
            S(-24,  89),   S(-16,  83),   S( 20,  70),   S( 27,  64),   S( 25,  68),   S( -1, 106),   S( 61,  59),   S( 20,  69),
            S(-14,  98),   S( 23,  78),   S( 18,  68),   S( 39,  66),   S( 42,  66),   S( 49,  65),   S( 90,  51),   S( 56,  45),
            S(-13,  90),   S( 12,  74),   S( 12,  69),   S( 18,  67),   S( 29,  51),   S( 51,  47),   S( 87,  31),   S( 95,  13),
            S(-35, 100),   S(-17,  99),   S( -8,  92),   S( 26,  76),   S( 18,  72),   S( 32,  69),   S( 62,  63),   S(110,  27),
            S(-77, 151),   S( -8, 103),   S( 10,  76),   S( 40,  63),   S( 51,  54),   S( 57,  64),   S(119,  48),   S(103,  46),

            /* rooks: bucket 4 */
            S(-87,  26),   S(-12,  -1),   S(-45,   5),   S(-27,  18),   S(-33, -18),   S(  7, -50),   S( -7, -21),   S(-13, -35),
            S(-35,   2),   S(-43,   6),   S(-45,  15),   S(-39,  23),   S(-13, -10),   S(-16, -24),   S(  3, -35),   S(-21, -22),
            S(  0,  12),   S(-25, -18),   S(-13,  10),   S(-14,  -9),   S( -4,  -6),   S( -6,  -7),   S( 32, -16),   S(-45,   0),
            S(-32, -11),   S(  2,   5),   S(-24,  17),   S( 27,   1),   S( 19,   4),   S( 16,  -1),   S( 15,  10),   S( -8,  13),
            S(-17, -11),   S( -3,  30),   S(-10,  21),   S( 75,   6),   S( 22,  21),   S(  2,  17),   S( 39,  29),   S( 31,   1),
            S( 25,  10),   S( 26,  12),   S( 54,  14),   S( 42,  13),   S( 32,  17),   S(  4,  35),   S(  7,  27),   S( 24,  31),
            S(  4,  -4),   S( 38,  30),   S( 31,  28),   S( 38,  21),   S( 53,   9),   S( 10,   2),   S( 32,  18),   S( 28,  21),
            S( 38, -54),   S( 40,  44),   S( 15,  29),   S( 11,  18),   S( 17,   4),   S(  8,  25),   S( 13,   6),   S( 15,  18),

            /* rooks: bucket 5 */
            S(-50,  34),   S(-59,  53),   S(-67,  52),   S(-58,  36),   S(-46,  24),   S(-44,  39),   S(-13,  25),   S(-43,  44),
            S(-42,  35),   S(-37,  30),   S(-85,  67),   S(-56,  37),   S(-42,  23),   S(-22,  15),   S(  4,  14),   S(-36,  23),
            S( -8,  48),   S(-48,  60),   S(-57,  61),   S(-62,  58),   S(-35,  29),   S(-12,  30),   S(-11,  41),   S(-14,  41),
            S(-33,  73),   S( -5,  46),   S(-25,  65),   S(-13,  42),   S(-14,  54),   S(  3,  59),   S( -4,  53),   S(  3,  35),
            S( 12,  61),   S(  5,  63),   S( 42,  44),   S( 33,  57),   S( 38,  52),   S( 16,  72),   S( 64,  59),   S( 27,  40),
            S( 60,  55),   S( 34,  63),   S( 57,  51),   S( 28,  69),   S( 57,  48),   S( 52,  57),   S( 50,  47),   S( 41,  42),
            S( 45,  39),   S( 22,  63),   S( 43,  53),   S( 62,  40),   S( 36,  48),   S( 44,  55),   S( 63,  48),   S( 65,  43),
            S( 87,  31),   S( 69,  32),   S( 35,  55),   S( 20,  35),   S( 48,  44),   S( 48,  47),   S( 44,  42),   S( 20,  48),

            /* rooks: bucket 6 */
            S(-60,  27),   S(-60,  41),   S(-40,  31),   S(-41,  25),   S(-66,  39),   S(-87,  67),   S(-56,  55),   S(-47,  54),
            S(-47,  35),   S(-32,  32),   S(-29,  31),   S(-48,  25),   S(-55,  45),   S(-75,  63),   S(-67,  57),   S( 12,  16),
            S(-42,  59),   S(-30,  39),   S(-14,  41),   S(-46,  43),   S( -4,  31),   S(-38,  62),   S(-29,  74),   S(  3,  41),
            S(-40,  71),   S( 25,  46),   S( -7,  59),   S(  8,  38),   S(  6,  41),   S(  0,  54),   S(-40,  60),   S(-18,  57),
            S(  1,  71),   S( 37,  58),   S( 55,  45),   S( 37,  41),   S( 23,  59),   S( 42,  48),   S( 41,  45),   S( 13,  56),
            S(  8,  62),   S( 57,  50),   S( 81,  31),   S( 45,  30),   S( 33,  44),   S( 49,  57),   S( 55,  47),   S( 61,  48),
            S( 33,  59),   S( 69,  41),   S( 76,  32),   S( 91,  17),   S( 97,  24),   S( 48,  52),   S( 51,  51),   S( 48,  47),
            S( 52,  72),   S( 26,  64),   S( 33,  49),   S( 43,  39),   S( 66,  45),   S( 55,  63),   S( 58,  60),   S( 21,  65),

            /* rooks: bucket 7 */
            S(-77, -11),   S(-52,  -8),   S(-47, -16),   S(-34,  -9),   S(-26,  -6),   S(-60,  34),   S(-49,  21),   S( -6, -14),
            S(-74,  21),   S(-46,   6),   S(-49,   4),   S(-19, -11),   S(-22,  11),   S( -9,  10),   S(-18,  -2),   S(-54,  10),
            S(-90,  50),   S(-43,  17),   S(-19,   8),   S(-10, -10),   S(-10,   6),   S(-19,  -7),   S(-17, -11),   S( 15,   8),
            S(-65,  41),   S( -9,  21),   S(  5,  13),   S( 19,   9),   S( 32,  -3),   S( 32,   3),   S( 36,   0),   S( -7,   8),
            S(-25,  39),   S( 11,  11),   S( 50, -13),   S( 56,  -8),   S( 75,  -5),   S(104,  -1),   S( 81,   2),   S( 49, -14),
            S(-18,  32),   S( 15,  12),   S( 81, -26),   S( 98, -25),   S( 76,  -8),   S( 77,  16),   S( 76,  17),   S( 27,   4),
            S(-12,  34),   S( 20,  17),   S( 48,   2),   S( 65,   0),   S( 94,  -8),   S( 93,  -7),   S( 41,  27),   S( 17,  10),
            S(  6,  61),   S(-26,  43),   S( 33,   2),   S( 78, -24),   S( 26,   5),   S( 21,  16),   S( 46,   7),   S( 63,  -5),

            /* rooks: bucket 8 */
            S(-48, -44),   S(-12, -10),   S(  2,   3),   S( -3, -15),   S(-13, -42),   S(-11, -54),   S(-16, -24),   S( -8, -17),
            S( -3, -18),   S( -5,  -6),   S(  0, -11),   S(  8, -14),   S( -7, -29),   S(-10, -23),   S( -7, -43),   S(-17, -64),
            S(  7,  18),   S(  8, -19),   S(  3,   5),   S(  9,   9),   S(-15, -33),   S( -5, -33),   S( 13,  21),   S( -2,   0),
            S( -7, -18),   S( -3,  26),   S( -6,   5),   S( 21,   4),   S(  7,  13),   S( -5, -12),   S(  9, -17),   S(  3,   1),
            S( -5,  -9),   S(  1,  16),   S(  0,  28),   S( 17,   9),   S(  3,   5),   S( 21,   4),   S( 11, -12),   S( 13, -34),
            S(  9,  30),   S( -4,   8),   S( 34,  41),   S( 29,  -8),   S(  2,  -5),   S(  7, -13),   S(  4,   0),   S( 12,  42),
            S(  5, -10),   S( 14, -15),   S( 27,   4),   S( 21, -19),   S( 30,   8),   S( 22, -21),   S( 20, -13),   S( 18,  -5),
            S(  5, -146),  S( 12, -10),   S( 23,   8),   S( -1,  -9),   S(  3,   1),   S(  4, -11),   S(  7,  -7),   S( 22,  -1),

            /* rooks: bucket 9 */
            S(-49, -15),   S(-12, -21),   S(-24, -27),   S(-39,  -2),   S(-20,   3),   S( -8,  -4),   S( 10, -42),   S(-40, -32),
            S( 29, -19),   S(  3, -16),   S(-17, -19),   S(-19,  -4),   S(-18, -11),   S( 18,   5),   S(  3, -29),   S(-13, -28),
            S( 11, -16),   S( 18,  -5),   S(  3,   5),   S(-10,   0),   S( -8, -20),   S( 26,  -6),   S( 13,  20),   S( -3,   0),
            S(  3,  11),   S( 10,   5),   S( 12,  21),   S( -2,   4),   S(  7,  18),   S( 23,  -4),   S( 16,  37),   S( 14,   2),
            S( 12,   9),   S(  5,  12),   S(  6,  29),   S( 15,  22),   S( 35,  28),   S( 26,  28),   S( 12,  -1),   S( 14,  -7),
            S( 18,  44),   S( -6,  12),   S( 12,   3),   S(-12,   2),   S( 16,   4),   S( 32,   8),   S(  9,  35),   S( 16,  13),
            S( 64,  18),   S( 61,   6),   S( 34,  28),   S( 54,  11),   S( 32,  -9),   S( 31,   6),   S( 39,   1),   S( 46,  25),
            S( 67, -70),   S( 41, -28),   S( 21,  28),   S( 33,  27),   S( 12,  37),   S( 26,  17),   S( 25,  11),   S( 31,  10),

            /* rooks: bucket 10 */
            S(-60, -78),   S(-18, -48),   S(-49, -25),   S(-35,  -3),   S(-36,  -2),   S(-31, -10),   S(  8, -13),   S(-36, -18),
            S( -4, -15),   S(  9, -27),   S( -4, -25),   S( -5, -15),   S(  2, -18),   S( -9,  -3),   S( 35,   4),   S(  8,   0),
            S(-14, -17),   S(-13, -20),   S(  2, -17),   S( 19,  -3),   S(-17,  19),   S( -4,  -6),   S( 25,  26),   S(  6,  -4),
            S(  4,   2),   S(  7, -11),   S(  1,  -5),   S(  5,  15),   S( 29,  -4),   S(  3,  -6),   S( 25,  26),   S(  0,  -8),
            S(  8,  14),   S( 32,   9),   S( 13,   9),   S( 20, -20),   S( -3,  -4),   S( 15,   9),   S( 31,  31),   S( 10,  28),
            S( 39,  31),   S( 31,  42),   S( 22,  10),   S( 22,   7),   S(  3,  -9),   S( 20,  10),   S( 36,  20),   S( 10,  38),
            S( 75,  13),   S( 81,   2),   S( 78,  -5),   S( 72, -18),   S( 56, -17),   S( 39,  15),   S( 27,   7),   S( 33,   6),
            S( 60,  16),   S(  8,  -2),   S( 40,   0),   S( 23,   7),   S( 36,  -2),   S( 30,  14),   S( 15,   3),   S( 20, -10),

            /* rooks: bucket 11 */
            S(-43, -44),   S(-31, -25),   S(-20, -28),   S(-31, -56),   S(  0, -21),   S( -6,   5),   S(-27, -30),   S(-55, -16),
            S(-17, -28),   S( -7, -43),   S( -2, -28),   S( -2, -29),   S( -5, -23),   S(-17, -17),   S( -2, -32),   S(-22,   2),
            S(  2, -30),   S( 18, -13),   S( 20, -14),   S( 11, -22),   S( 11,  -9),   S( -9,  11),   S(-24, -26),   S(-10, -52),
            S( -2,  28),   S( -1, -10),   S( -2,  10),   S( 14,   6),   S(  5,  -5),   S( 15,  29),   S( 28,  -9),   S(  2, -23),
            S( 13,  11),   S( 18, -10),   S( 30,  -1),   S( 24,  -9),   S( 28,  -5),   S( 34, -11),   S( 10,   7),   S(  0,  -9),
            S( 28,  33),   S( 46,   7),   S( 28, -10),   S( 51,  19),   S( 53,  20),   S( 45,   8),   S( -2,   4),   S( 18,  26),
            S( 64,  37),   S( 62,   2),   S( 71, -14),   S( 76, -15),   S( 49, -10),   S( 55,  12),   S( 36,  35),   S( 57,  -1),
            S( 45,  34),   S( 14,  29),   S( 23,   6),   S( 11,  -8),   S( -6,  -3),   S( 20,  19),   S( 15,  13),   S( 34,  10),

            /* rooks: bucket 12 */
            S( -4,  -9),   S( -9, -30),   S(-13, -51),   S( -5, -11),   S(  0,  -4),   S( -4, -34),   S(-22, -62),   S(-24, -52),
            S(  8,   6),   S( -6, -23),   S(-12, -19),   S( -7, -18),   S(-10,  -8),   S( -8, -15),   S(  1,  -2),   S(-11, -32),
            S(  3,   0),   S( -6, -19),   S( -9, -25),   S(-13,  -9),   S( -5, -22),   S(  6,  -7),   S( -7, -10),   S(  5,  -9),
            S( -6,  -8),   S(  0, -13),   S(  3,  12),   S(  8, -12),   S(  1,  -8),   S(-10, -39),   S( -8, -12),   S( -3, -39),
            S( -2, -10),   S( -2, -18),   S( 12,   4),   S(  9,   7),   S( -8, -36),   S(  6, -19),   S( -5,  -9),   S(  1, -15),
            S( -2,  -7),   S( -3,  -8),   S( 19,  32),   S(  9,  -5),   S( -4,  -7),   S( -6, -20),   S(  1, -25),   S(  5,   8),
            S( -4,  -3),   S(  3, -27),   S(  4, -41),   S( 13,   1),   S(  7,  -3),   S( -6, -40),   S( -3,  -9),   S(  9, -17),
            S( -4, -41),   S(  8,  22),   S(  4, -20),   S(  2,   2),   S( -3, -25),   S(-11, -49),   S(-14, -29),   S(  8,  -4),

            /* rooks: bucket 13 */
            S(-15, -40),   S( -6, -24),   S( -4, -18),   S(  0,  10),   S(  5,  -4),   S(-13, -39),   S(  1, -23),   S(-18, -32),
            S( -2, -34),   S( -2, -13),   S(-12,  -7),   S( -8,  -3),   S(-10, -18),   S( -2, -13),   S(  5,   1),   S( -4, -21),
            S( -5, -28),   S( -7, -27),   S( -5, -36),   S( -3, -24),   S( 10,  11),   S(  1,  -6),   S(  1, -22),   S(  2, -32),
            S( -7, -52),   S(  2,  -4),   S( -9, -42),   S( -5, -10),   S( 13,  12),   S( -9, -38),   S( -2, -28),   S(  2, -18),
            S( 11, -20),   S(  9, -18),   S( 17,  25),   S( -5,  -9),   S(-10, -29),   S(  4, -15),   S( -6, -39),   S(  9,  -8),
            S( -6, -39),   S( 11, -27),   S( -7, -10),   S( 15,  -7),   S(  7, -13),   S( 11,  15),   S(  9,  -3),   S(  5,   8),
            S(  6,  -3),   S(  9,  19),   S( 11,  10),   S(  2, -15),   S( 12, -27),   S( 21,   7),   S(  4, -13),   S(  3, -18),
            S(-14, -121),  S(-16, -68),   S(  6,   6),   S(  1,   0),   S( -3,  14),   S( -3, -30),   S(-10, -27),   S(  5,   1),

            /* rooks: bucket 14 */
            S( -8, -33),   S(-16, -48),   S( -3,  -8),   S( -3, -34),   S(  2, -24),   S( -9, -22),   S( 10,  -5),   S( -7, -22),
            S(-21, -43),   S(-14, -54),   S(-10,   3),   S(-14, -36),   S(-11, -17),   S(  1, -32),   S(  6,  24),   S(  6, -12),
            S( -2, -24),   S( -8, -19),   S( -4, -19),   S( -6, -12),   S(-14, -26),   S( -7, -22),   S(  7,  22),   S( -2, -27),
            S( 12,   5),   S( -7, -34),   S( -4, -19),   S( -5,   6),   S(  3, -14),   S(  4, -13),   S( -4, -35),   S( -2, -21),
            S(  1, -14),   S(  3, -25),   S( -7, -28),   S( -9, -24),   S( -6, -17),   S( -4, -19),   S(  3,   7),   S(  9,   2),
            S(  4, -15),   S(  0, -24),   S(  1, -19),   S(  3, -21),   S(-11, -20),   S( -8,   6),   S(  7,   8),   S(  0,  -5),
            S( 19,  -1),   S(  2, -35),   S(  3, -20),   S(  2, -28),   S(  7, -42),   S(  7,   2),   S(  9,  12),   S( 10,   8),
            S( -2, -23),   S(  4, -16),   S( -8, -28),   S( 10,  12),   S( -9, -18),   S(  3,   7),   S(  6,  16),   S(  0, -16),

            /* rooks: bucket 15 */
            S( -2, -54),   S(-13, -42),   S( -1, -28),   S( -7, -28),   S(  0, -16),   S( -4,  -8),   S(-17, -53),   S( -9, -14),
            S(-14, -21),   S(-14, -27),   S(  2,  -2),   S( -6, -23),   S(-10, -28),   S(  6, -26),   S(-11, -41),   S(  7,   5),
            S( -9, -23),   S(-11, -23),   S( -2, -24),   S(  2,   0),   S(  9, -30),   S( -3,  -8),   S( -4,   4),   S( -4, -12),
            S(  2, -31),   S( -4, -25),   S(-11, -18),   S( -5, -19),   S(-11, -20),   S(  3, -18),   S(  0, -18),   S( -9,  -2),
            S(  1, -11),   S( -5, -12),   S( 11,  -9),   S(  0, -13),   S(  1,  -2),   S(  2,   0),   S( -1,   7),   S(  0,  16),
            S(  8,  18),   S(  3,   0),   S(  1, -12),   S(  0, -10),   S( -5,  -9),   S(  2,  11),   S(  6,  -8),   S( -7, -13),
            S( 12,  19),   S( 11,  -6),   S(  9, -34),   S( -4, -33),   S(  1, -22),   S( 13,  37),   S(  3,  -2),   S(  0,  12),
            S(  2, -16),   S( -7, -18),   S(  2,  -6),   S(  1, -11),   S( -6, -13),   S(  0, -25),   S(  1, -19),   S(  2,  -3),

            /* queens: bucket 0 */
            S(-17, -10),   S(-18, -53),   S( 47, -84),   S( 55, -58),   S( 33, -37),   S( 19,  -3),   S( 55,   9),   S( 21,  19),
            S(-10, -12),   S( 30, -60),   S( 38, -17),   S( 21,   5),   S( 20,  31),   S( 22,  21),   S(  8,  62),   S( 37,  21),
            S( 25,   5),   S( 37,  15),   S( 20,  28),   S( 17,  37),   S( 17,  18),   S(  9,  19),   S(  8,  32),   S( 35,  32),
            S( 20,  23),   S( 25,  45),   S(  8,  47),   S( 11,  44),   S(  9,  55),   S( 14,  34),   S( 17,  26),   S( 21,  28),
            S( 41,  52),   S( 28,  43),   S( 18,  40),   S( 25,  52),   S( -4,  28),   S( -5,  13),   S( 33,  20),   S( 45,  -2),
            S( 29,  61),   S( 26,  53),   S( 14,  38),   S( 22,  13),   S( 47,  -9),   S(  7,  36),   S( 29,  19),   S( 27, -19),
            S( 50,  51),   S( 53,  42),   S( 34,  36),   S( 51,  25),   S( 22,   7),   S( -6,  -9),   S( 32,  24),   S( 34,  11),
            S( 48,  30),   S( 23,  37),   S( 44,  18),   S( 37,  35),   S( 46,  32),   S(-12,   3),   S( 52,  28),   S( 48,  29),

            /* queens: bucket 1 */
            S(  0, -17),   S(-75, -23),   S(-52, -27),   S(-15, -67),   S(-11, -25),   S(-20, -47),   S( 12, -28),   S( 10,  27),
            S(-17, -25),   S(-13, -44),   S(  7, -51),   S( -7,   5),   S(-12,   2),   S(  3,  -3),   S( 18, -39),   S( -1,  23),
            S(-32,  42),   S( -2,  -7),   S(  1,  15),   S( -8,  10),   S( -5,  32),   S(-17,  33),   S( 13,   9),   S( 17,  22),
            S(  9, -20),   S(-11,  30),   S(-15,  36),   S(  9,  43),   S( -7,  53),   S(  1,  29),   S(  3,  -3),   S( 18,  17),
            S( 13,   8),   S(  6,  22),   S( -5,  62),   S(-19,  59),   S(-15,  52),   S(  0,  14),   S( -8,  16),   S(  1,  36),
            S( 11,  25),   S( 13,  52),   S( 15,  57),   S(-36,  54),   S(-16,  47),   S(-34,  43),   S( 28,  23),   S( 19,  45),
            S(  4,  33),   S(-11,  67),   S(-19,  32),   S(-25,  68),   S(-26,  46),   S( 16,  25),   S( -9,  40),   S(-25,  47),
            S( -5,   6),   S(  5,  16),   S( 14,  25),   S( -9,  10),   S( -5,  13),   S(  6,  11),   S( 10,  26),   S( -9,  30),

            /* queens: bucket 2 */
            S(  7,  19),   S( 13, -36),   S(  5, -19),   S( -5, -10),   S(-23,   6),   S(-28, -12),   S(-27, -21),   S( 16,  11),
            S( 14,  13),   S( 10,  34),   S( 16, -13),   S( 16, -23),   S( 11, -29),   S( 14, -50),   S( 11, -10),   S( 34, -24),
            S( 13,  10),   S( 16,   6),   S(  1,  45),   S(  6,  35),   S(  0,  57),   S( 13,  44),   S( 10,  19),   S( 30,  12),
            S(  7,  20),   S( -2,  50),   S( -2,  42),   S(  4,  56),   S(-19,  82),   S( -3,  81),   S( 16,  15),   S(  5,  65),
            S( 15,   6),   S( -7,  53),   S(-13,  56),   S(-34,  96),   S(-41, 109),   S(-15,  76),   S( -9, 102),   S( -6, 105),
            S( 12,  24),   S(  0,  42),   S(-28,  75),   S(-10,  52),   S(-31,  90),   S(-12,  93),   S( -3,  94),   S( 11,  76),
            S(-18,  52),   S(-35,  76),   S(-16,  58),   S(  6,  59),   S(-21,  74),   S( 25,  40),   S(-20,  44),   S( -9,  76),
            S(-64,  75),   S(  5,  33),   S( 30,  35),   S( 33,  29),   S(  2,  61),   S( 19,  31),   S( 14,  25),   S(-15,  41),

            /* queens: bucket 3 */
            S( 82,  92),   S( 55,  97),   S( 48, 100),   S( 42,  82),   S( 67,  32),   S( 45,  24),   S( 21,  25),   S( 47,  63),
            S( 66, 114),   S( 60, 108),   S( 44, 112),   S( 46,  87),   S( 46,  77),   S( 61,  47),   S( 64,  10),   S( 38,  52),
            S( 61,  89),   S( 52, 105),   S( 55,  83),   S( 52,  78),   S( 48,  91),   S( 52,  96),   S( 60,  96),   S( 63,  75),
            S( 48, 123),   S( 61,  83),   S( 49,  94),   S( 42,  94),   S( 45,  93),   S( 40, 129),   S( 59, 101),   S( 51, 136),
            S( 64,  92),   S( 58, 103),   S( 52,  88),   S( 39,  94),   S( 35, 114),   S( 27, 126),   S( 39, 165),   S( 55, 156),
            S( 49, 122),   S( 58,  98),   S( 52,  94),   S( 27, 116),   S( 35, 130),   S( 75, 100),   S( 64, 137),   S( 36, 191),
            S( 63, 115),   S( 61, 102),   S( 71,  83),   S( 61,  93),   S( 35, 109),   S( 61, 110),   S( 94, 125),   S(162,  72),
            S( 79,  90),   S(105,  74),   S( 79,  83),   S( 83,  76),   S( 44, 103),   S(113,  50),   S(138,  57),   S(146,  56),

            /* queens: bucket 4 */
            S(-12, -24),   S(-19, -20),   S(-22,  -9),   S(-11,  -9),   S( 10, -17),   S( 34,  -2),   S(-35,  -9),   S(-25,  -2),
            S(-28, -17),   S(-26,  -4),   S( 14,  -8),   S(-41,  23),   S( -1,  -6),   S(  1, -14),   S( -8, -11),   S(-36, -16),
            S(  3,   2),   S( 13,  -1),   S(  3,  29),   S( -3,  31),   S( 20,  16),   S(  4,  -7),   S(  6, -20),   S(-25, -24),
            S(-14,   4),   S( -7,  14),   S(  2,  37),   S( -3,  31),   S( 14,  34),   S( 19,  18),   S(  3, -14),   S( -3,  -8),
            S( -8,   0),   S( 18,  13),   S( 17,  28),   S( 30,  40),   S( 22,  29),   S( 19,  -1),   S(-21, -18),   S( -8, -29),
            S(  5,  14),   S( 35,  14),   S( 25,  53),   S( 22,  43),   S( 10,   7),   S(  2,   3),   S(-16, -15),   S(-14,  -9),
            S( -9, -17),   S( -5,  18),   S(  3,  26),   S( 31,  32),   S(  9,  10),   S(-12,  -3),   S(-22, -43),   S(-21, -26),
            S( -2, -16),   S( -1,  -2),   S( 30,  36),   S(  4,  18),   S(-17, -18),   S( -7, -11),   S(-19, -33),   S( -8, -18),

            /* queens: bucket 5 */
            S(-37, -13),   S(-26, -31),   S(-32, -29),   S(-45, -28),   S(-58, -30),   S(  7, -16),   S(-10,  -6),   S( -7,  -8),
            S(-29,  -5),   S(-39, -13),   S(-70, -20),   S(-67,  -3),   S(-16,  -3),   S(-43, -16),   S(-49, -17),   S(-50, -16),
            S(-35,   3),   S(-58, -11),   S(-65,   4),   S(-31,  33),   S( 14,  52),   S(-12,  22),   S( -4,  -2),   S( 10,  20),
            S(-54, -10),   S(-52,  -3),   S( -1,  36),   S( -5,  52),   S( 11,  29),   S( -4,  13),   S( -6,  -8),   S( -9,  14),
            S(-33,  -6),   S(-21,  20),   S(-11,  48),   S( -7,  46),   S( 27,  48),   S(  0,  17),   S( -1,   7),   S(-30, -29),
            S(-17,  16),   S(  6,  37),   S(-12,  42),   S(  1,  45),   S( 39,  49),   S(  3,  12),   S(  0,   1),   S(-11, -10),
            S( -9,   9),   S(-10,  13),   S(  5,  59),   S( -3,  33),   S(  0,  37),   S( 21,  31),   S( 11,   8),   S(-21, -16),
            S(  8,  25),   S( 11,  10),   S(  3,  18),   S( 11,  49),   S( 16,  28),   S(  3,  20),   S( -1, -24),   S(-18, -16),

            /* queens: bucket 6 */
            S(-32,   4),   S(-54, -26),   S(-67, -29),   S(-87, -59),   S(-92, -51),   S(-71, -45),   S(-51, -43),   S(-28,   3),
            S(-64, -14),   S(-48,  -1),   S(-57,  14),   S(-64,  11),   S(-79,  16),   S(-91,  -2),   S(-85, -19),   S(  7,  18),
            S(-46,  10),   S(-24,  11),   S(-57,  39),   S(-101,  87),  S(-39,  50),   S(-36,   3),   S(-48, -13),   S(  3,   6),
            S(-44,  12),   S(-28,   9),   S(-29,  62),   S(-49,  70),   S(  3,  44),   S( 12,  49),   S(-11,  36),   S( 11,  -7),
            S(-53,  19),   S( -7,  36),   S(-28,  52),   S(  8,  29),   S( 29,  55),   S( 59,  38),   S( 25,  32),   S( -6,  18),
            S(-25,  41),   S(-12,  18),   S( 22,  20),   S( 21,  45),   S(  6,  53),   S( 62,  66),   S( -5,  -8),   S(-15,  10),
            S( -7,   6),   S(  1,   1),   S(-14,  40),   S(-11,  34),   S( 27,  49),   S( 17,  58),   S(-10,  21),   S(-38,  -1),
            S( -1,   6),   S( 17,  11),   S( 11,  28),   S( -4,  21),   S( 30,  38),   S( 19,  27),   S( -3,  14),   S(  3,   7),

            /* queens: bucket 7 */
            S(-10, -14),   S(-36,  11),   S(-51,  19),   S(-38,   8),   S(-31, -11),   S(-35, -24),   S(-29,  -5),   S(-14, -10),
            S(-35, -10),   S(-53,   3),   S(-31,   8),   S(-24,  36),   S(-32,  31),   S(-45,  39),   S(-42,  21),   S(-30, -13),
            S(-36, -24),   S(-56,  31),   S(-23,  32),   S(-15,  29),   S(  4,  21),   S(  1,  29),   S( -7,  16),   S(-13,   3),
            S(-63,   1),   S(  6,   1),   S(-20,  24),   S( -7,  40),   S( 31,  22),   S( 33,  24),   S( 14,  33),   S(  2,  21),
            S(-30,  17),   S(-53,  23),   S(  9,  18),   S( 45,  -4),   S( 63, -12),   S( 84, -17),   S( 37,  11),   S( 44,  -7),
            S(-16,  11),   S(-18,   7),   S(  4,  -1),   S( 14, -10),   S( 37,  35),   S( 82,  20),   S( 65,   2),   S( 49,  12),
            S(  8, -20),   S(  1,  10),   S(  1,  -7),   S(  4,  12),   S( 38,  15),   S( 54,  37),   S( 52,  18),   S( 54,  27),
            S( 14,   3),   S( 19,   3),   S( 21,   7),   S( 20,  15),   S( 41,  24),   S( 24,  19),   S( 16,   6),   S( 40,  44),

            /* queens: bucket 8 */
            S( -5, -10),   S( -1,   4),   S(-13,  -5),   S( -9,  -7),   S( -4,   0),   S( -1, -16),   S(-20, -24),   S( -4,   5),
            S( -7,   0),   S(-11, -15),   S( -4,   5),   S(-13,  -3),   S( -5,  -4),   S(-17, -20),   S(-18, -39),   S( -4,  -9),
            S( -1,   0),   S( -6,   2),   S( -6,   3),   S( -5, -10),   S( -5,   4),   S(-11, -12),   S(-11, -26),   S(-15, -27),
            S( -3,   3),   S( 10,  19),   S( 12,  18),   S(  6,  12),   S( -2,   1),   S( -6,   0),   S( -1,  -3),   S( -7, -21),
            S( 16,  28),   S(  3,  28),   S( 12,  14),   S( 12,  21),   S( 13,  31),   S(  4,   0),   S( -8, -10),   S(-10, -17),
            S(  8,  20),   S( 13,  22),   S(-17,  16),   S( 15,  36),   S( -8, -14),   S( -5, -10),   S(  4,   2),   S(  3,  12),
            S( -6, -12),   S(-16, -24),   S( 22,  36),   S( 15,  16),   S(  3,  18),   S(  3,  18),   S( -2,  -7),   S( -6, -15),
            S(-14, -28),   S( 14,  11),   S(-15, -47),   S( -8,  -5),   S(-11, -29),   S( -1,  -6),   S( -2, -16),   S( -5,  -7),

            /* queens: bucket 9 */
            S(  5,   7),   S(-13, -27),   S(  1,  -2),   S(-30, -32),   S(-23, -38),   S(-18, -30),   S(-13, -20),   S(-13, -18),
            S( -3,  -5),   S( -9,  -7),   S(-19, -24),   S( -4,   0),   S(-17,  -8),   S(-15, -19),   S(  2,  -1),   S( -3,  -7),
            S(  4,   6),   S(  4,   9),   S( -8,  21),   S( -4,  -4),   S( -4,   9),   S(  1,  -2),   S(  3,   2),   S(  3,   0),
            S( -4,  -9),   S( -5,   5),   S( 13,  41),   S(  8,  23),   S( 19,  32),   S(  5,  13),   S( -9, -16),   S(  0, -10),
            S(  6,  11),   S(  9,  32),   S( 12,  33),   S( 18,  52),   S( 21,  35),   S( 10,  19),   S( -3,   5),   S(-11, -14),
            S(-17, -20),   S(-15,  -2),   S(  5,  22),   S( 16,  36),   S( -4,   3),   S( -1,  11),   S( -8,  -6),   S( -5,  -6),
            S( -6, -16),   S(-10, -25),   S( -7,  24),   S( 12,  31),   S( 16,  21),   S(  7,  -5),   S(  6,  -4),   S(-12, -25),
            S(  0,  -1),   S( -3, -22),   S( 12,  -1),   S(  1,  16),   S( 13,   1),   S( -2,   0),   S( 11,   2),   S(  2, -16),

            /* queens: bucket 10 */
            S(  3,   0),   S( -3,   4),   S(-11, -18),   S(-22, -26),   S(-12, -14),   S( -6,  -5),   S(  1, -12),   S( -6,  -9),
            S( -8, -11),   S( -8, -15),   S(-15, -25),   S( -9, -14),   S( -5,  -6),   S(-18, -13),   S(  1,  -8),   S(-17, -19),
            S( -2, -13),   S( -9, -14),   S( -8,  -9),   S( -1,   4),   S( -6,   1),   S( -7,   3),   S(  2,   2),   S(  3,   7),
            S(  0,  -2),   S(  1,  -4),   S( -3,  -6),   S(  1,  32),   S( 15,  26),   S( -6,   6),   S( -2,  -5),   S(-14, -19),
            S( -5,  -7),   S(  5,  -6),   S( -5,   5),   S( 22,  49),   S(  1,  -2),   S( 17,  30),   S( 12,  14),   S(  1,   6),
            S( -3,  -5),   S(-20, -32),   S( -4,   0),   S(  2,  14),   S(  5,  16),   S(  5,  20),   S( 12,   9),   S( -4, -11),
            S( -6,  -6),   S(-18, -28),   S(  8,  22),   S( -6,  -7),   S(  7,   7),   S(  4,  11),   S( -3,  -8),   S( -8,  -5),
            S(  5,  -1),   S( -3, -16),   S(  6,  -4),   S(  7,  -6),   S( 18,  16),   S(  5,   7),   S( 16,  14),   S(  2,  -8),

            /* queens: bucket 11 */
            S(-10, -14),   S( -7, -20),   S(-22, -20),   S(-11, -28),   S(-12, -19),   S( -9, -11),   S( -6,  -6),   S(-12, -22),
            S(-17, -32),   S( -8,  -7),   S(-41, -35),   S(-11,  -9),   S(-12, -10),   S( -9,  -6),   S( -5,  -9),   S( -6,  -4),
            S(-17, -23),   S(-17, -35),   S(  3, -20),   S( -9, -17),   S( -8, -13),   S( -2,   6),   S(  8,  20),   S(-12,  -8),
            S(-16, -28),   S(-25, -25),   S( -7, -25),   S( 15,  30),   S( 10,   2),   S(-11,  -6),   S( 24,  26),   S( -2,  -1),
            S(-14, -13),   S( -6, -18),   S(-22, -26),   S( 24,  19),   S( 16,  15),   S( 28,  52),   S( 22,  42),   S(  3,  12),
            S(-14, -30),   S(  3,   3),   S(-16, -18),   S( 15,  12),   S( 24,   6),   S( 47,  38),   S( 11,   0),   S( -7,  -5),
            S( -9,  -4),   S(-14, -23),   S(  8,  15),   S(-13,  -5),   S(  6,   7),   S( 24,  25),   S( 38,  39),   S( -2, -15),
            S(-11, -22),   S( -9, -25),   S( -8, -21),   S(  4, -15),   S(  2,   9),   S( -2,  -7),   S( 20,  10),   S(  0, -31),

            /* queens: bucket 12 */
            S(  6,   0),   S( -1,  -2),   S(  2,   1),   S( -8,  -5),   S(-10, -13),   S( -2,  -3),   S(  0,  -2),   S( -4, -10),
            S( -4,  -2),   S( -8, -14),   S( -9, -12),   S( -5, -10),   S( -3,  -3),   S( -6,  -2),   S( -1,  -9),   S( -6,  -9),
            S( -2,  -5),   S( -5,  -9),   S( 12,  13),   S( -5,  -5),   S( -2,  -5),   S( -8, -14),   S(-13, -25),   S( -8,  -7),
            S(  2,   7),   S( -1,   3),   S(  4,   6),   S(  1,   8),   S(  7,  14),   S(  0,  -4),   S(  0,  -3),   S( -4, -11),
            S(  1,  -3),   S( 11,  13),   S( 32,  57),   S(  2,  16),   S( -5,   7),   S(  0,   5),   S(-13, -30),   S( -2, -14),
            S(  8,  19),   S( 14,  25),   S( 34,  43),   S( -1,   8),   S(  0,   5),   S(  2,   1),   S(  5,   5),   S( -5, -15),
            S(  2,   1),   S(  3,   8),   S( 17,  14),   S( 12,   9),   S(  5,   9),   S( -3,   4),   S(  8,   6),   S( -4,  -4),
            S( -3, -26),   S( -8, -24),   S(-12, -17),   S( -9, -27),   S( 11,  -7),   S(  1,  -2),   S(  2,  -4),   S( -6, -10),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -5, -15),   S(  0,  -5),   S( -3,  -7),   S( -3, -10),   S( -2,  -2),   S( -7, -10),   S( -6,  -8),
            S(  4,   9),   S(  5,  13),   S(  4,  10),   S( -4,  -4),   S( -6,  -6),   S(  2,  10),   S(  1,   5),   S(-11, -19),
            S( -3,  -7),   S(  0,   0),   S(  3,  15),   S(  2,  11),   S( -2,  -2),   S( -6,  -9),   S( -5, -12),   S(-12, -17),
            S( -3,  -7),   S(  2,   2),   S( 11,  12),   S( 18,  28),   S( 15,  33),   S( -4,  -5),   S( -5, -13),   S( -5,  -6),
            S( -3,  -1),   S(  6,  17),   S( 16,  41),   S( 12,  39),   S( 23,  45),   S(  0,  -8),   S( -4,  -7),   S( -7, -14),
            S(  0,   0),   S( 12,  32),   S( 38,  74),   S( 19,  41),   S(  1,  16),   S(  1,   8),   S(  6,  14),   S( -5, -14),
            S(  0,   1),   S( 19,  32),   S(  9,  28),   S( 13,  25),   S( -1,  10),   S(  1,  -7),   S( -2,  -9),   S(  6,   8),
            S(-11, -16),   S(  5,  -1),   S( -1,  -6),   S( -8, -10),   S(  7,   2),   S(  5,   8),   S( -8,  -6),   S( -6, -13),

            /* queens: bucket 14 */
            S( -1,  -2),   S(  0,   1),   S( -2,  -8),   S( -9,  -9),   S(  4,   6),   S( -2,  -5),   S( -2,  -9),   S( -5, -11),
            S( -5,  -7),   S(  5,  16),   S( -2,  -4),   S( -1,  -6),   S( -9, -12),   S( -7, -15),   S( -5,  -4),   S( -3,  -7),
            S( -2,  -2),   S(-10, -12),   S( -6, -14),   S( -1,  -2),   S(  1,   0),   S(  1,  -4),   S(  3,   5),   S( -6, -14),
            S( -8,  -9),   S(  8,   9),   S( -6,  -4),   S( 23,  41),   S( 14,  15),   S( -1,   6),   S( 11,  24),   S(  1,  -3),
            S(  4,  13),   S(  4,   1),   S(-13,  -8),   S( 16,  28),   S( 14,  34),   S( 17,  25),   S( 10,  18),   S( -4,  -9),
            S( -2,  -5),   S(  4,  14),   S( 14,  24),   S( 12,  21),   S( 18,  43),   S( 15,  46),   S(  8,  16),   S( -2,  -8),
            S(  3,   7),   S(  8,   9),   S( 16,  37),   S( 19,  33),   S( 15,  34),   S( 14,  27),   S( 16,  29),   S(  2,   5),
            S( -3,   0),   S(  0,   0),   S( -9, -14),   S( 12,  19),   S(  1,   4),   S(  3,   1),   S(  2,   7),   S(-10, -17),

            /* queens: bucket 15 */
            S( -1,  -3),   S(  1,  -5),   S( -5,  -8),   S( -2, -10),   S( -6, -10),   S( -6, -12),   S(-11, -24),   S(  0,  -6),
            S( -1,  -4),   S( -4,  -9),   S( -5, -13),   S( -4, -11),   S(  0,   8),   S( -3,  -8),   S( 11,  13),   S(  2,   1),
            S(  0,  -9),   S( -3, -11),   S( -1,  -2),   S( -4, -12),   S( -4, -11),   S(  6,  16),   S( -2,  -5),   S(  0,  -8),
            S( -5,  -8),   S(  3,   3),   S( -4,  -3),   S(  4,   2),   S(  2,  10),   S(  0,   6),   S(  6,   5),   S(  4,   3),
            S( -2,  -7),   S( -2,  -5),   S( -8, -12),   S( -4,  -5),   S(  5,  10),   S(  8,   6),   S( -3,  -6),   S(  0,  -7),
            S( -3,  -7),   S( -2,  -6),   S( -1,   2),   S(  1,   1),   S( -1,  -6),   S( 20,  31),   S(  4,  -1),   S(  0,  -8),
            S( -6, -13),   S(  4,  -6),   S(  6,   8),   S(  7,   7),   S(  6,   8),   S( 22,  37),   S( 12,  21),   S(  4,   5),
            S(  1,  -4),   S( -5,  -6),   S( -2,  -4),   S( 10,  12),   S(  7,   2),   S(  4,  -3),   S( -2,  -6),   S( -6, -20),

            /* kings: bucket 0 */
            S( 67,   2),   S( 49,  50),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 40,  38),   S(107,  65),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 46,  20),   S( -6,  38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 72,  52),   S( 51,  63),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10,  41),   S(  0,  29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 40,  69),   S( 49,  50),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -2,  49),   S(-22,  16),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  85),   S(-34,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 20, -60),   S( 72, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -10),   S( 15,  19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 38, -12),   S( 17,   1),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  9,  34),   S( -7,  31),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 41,  20),   S( 13,  12),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  3,  52),   S( -6,  47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 59,  30),   S( 18, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 32,  65),   S( -8,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -126),  S( 10, -62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -110),  S(-97, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  7, -52),   S(-39, -43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-39, -32),   S(-48, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-20, -36),   S(-18, -38),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-41, -21),   S(-88,   4),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-23, -41),   S(-34, -112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-86, -10),   S( -8, -94),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -112),  S(-75, -28),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40, -229),  S(-14, -100),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-66, -59),   S( 23, -67),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-50, -79),   S(-19, -104),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-15, -55),   S(-111, -19),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 24, -118),  S(-61, -72),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-125,  -3),  S(-30, -118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-45, -76),   S(  2, -229),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -21),   S(-26,  14),   S( 10,  -2),   S( -8,  23),   S( 19,   2),   S( 41,   9),   S( 47,  -7),   S( 47,   3),
            S(-13, -28),   S(-30,   2),   S( -1, -11),   S( -2, -12),   S( 14,   3),   S( -3,  16),   S( 25,  -1),   S( 20,  26),
            S(  3, -26),   S(  1, -21),   S( 32, -33),   S( 12, -16),   S( 19,  -7),   S(  9,  27),   S( -5,  48),   S( 29,  24),
            S( 11, -18),   S( 31,   2),   S( 51, -28),   S( 36,  -3),   S( 16,  46),   S(-17,  86),   S(  6,  86),   S( 54,  66),
            S( 93, -55),   S(125, -19),   S( 89, -25),   S( 42,  16),   S( 44, 138),   S( -6, 139),   S( 12, 156),   S( 63, 134),
            S(-220, -71),  S(-117, -134), S( 13, -166),  S( 37,  43),   S( 82, 197),   S( 65, 188),   S(110, 167),   S(100, 147),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41,  21),   S(-44,  25),   S(-18,  10),   S(-41,  57),   S(-15,   2),   S( 14,   9),   S( 14,   2),   S( 13,  28),
            S(-56,  17),   S(-50,  19),   S(-33,   9),   S(-20,   6),   S( -2,   7),   S(-17,  11),   S( -7,   2),   S(-19,  24),
            S(-46,  25),   S(-20,  20),   S(-25,   6),   S(  8,  -8),   S(  0,  20),   S(-22,  19),   S(-32,  32),   S(-14,  30),
            S(-34,  43),   S(  8,  26),   S(-18,  25),   S( 13,  26),   S(  3,  28),   S(-36,  47),   S( -3,  40),   S( 26,  57),
            S(  5,  36),   S( 62,  -2),   S( 94, -27),   S( 87, -23),   S( 32,  30),   S(  0,  37),   S(-26,  80),   S( 36,  93),
            S( 46,  43),   S(-33, -19),   S( -8, -101),  S(-10, -97),   S(-37, -67),   S( -3,  45),   S( 52, 185),   S( 67, 214),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45,  43),   S(-33,  25),   S(-21,  14),   S(-15,  21),   S(-34,  38),   S(-12,  12),   S(  0,  -7),   S( -9,  23),
            S(-55,  34),   S(-41,  28),   S(-33,  10),   S(-28,  18),   S(-26,  18),   S(-36,   8),   S(-15, -10),   S(-41,  15),
            S(-45,  50),   S(-39,  53),   S(-12,  16),   S(-16,  19),   S(-18,  22),   S(-25,   5),   S(-29,   9),   S(-31,  12),
            S(-31,  89),   S(-42,  76),   S(-15,  44),   S(  0,  37),   S( -7,  35),   S(-23,  18),   S(  4,  19),   S( 21,  14),
            S(-30, 132),   S(-46, 118),   S( -9,  24),   S( 23, -24),   S( 99, -11),   S( 91,  -7),   S( 72, -15),   S( 50,   4),
            S(-12, 248),   S( 32, 178),   S( 14,  69),   S( 28, -91),   S(-13, -171),  S(-78, -131),  S(-58, -55),   S( 16,  25),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  3,  17),   S( -2,  16),   S(  7,  12),   S( -1,  36),   S( -9,  53),   S( 30,  23),   S( 20,  -2),   S(  6, -11),
            S( -4,  19),   S( -4,  26),   S( -2,   9),   S( -3,   9),   S(  9,  16),   S( 13,   1),   S(  8, -12),   S(-21,  -4),
            S(  3,  37),   S( -9,  58),   S(  8,  20),   S(  7,   2),   S( 26, -10),   S( 16, -12),   S(  3, -20),   S(-15, -10),
            S(  2,  91),   S(-17, 104),   S(  7,  67),   S( 17,  34),   S( 21,   4),   S( 32, -24),   S( 22,   4),   S( 34, -18),
            S( -2, 157),   S(-16, 167),   S(-30, 167),   S(-10, 114),   S( 32,  52),   S( 86, -15),   S(115, -36),   S( 97, -39),
            S(103, 125),   S( 47, 238),   S( 25, 252),   S(  8, 209),   S(-24,  94),   S( 31, -176),  S(-57, -242),  S(-160, -176),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 70,   0),   S( 24,   4),   S( -2, -10),   S(-11, -10),   S(  5, -14),   S(  5, -10),   S(  0,  -9),   S(-56,  43),
            S( 39,  -3),   S(  6,  19),   S(  6,  -3),   S(-12,  -5),   S(-22, -21),   S(-15, -16),   S(-32, -20),   S(-43,   4),
            S( 64, -14),   S(108, -30),   S( 35, -18),   S(-28,  -4),   S(-70,  10),   S(-11,   3),   S(-72,  22),   S(-62,  32),
            S(-84, -74),   S(-14, -93),   S( 70, -59),   S(-27,   5),   S(-25,  18),   S(-62,  66),   S(-37,  57),   S(-52,  79),
            S(-28, -75),   S(-62, -113),  S( -6, -92),   S( 56,   5),   S( 73,  88),   S( -5, 100),   S( 15,  77),   S( -3, 102),
            S(  6, -59),   S(-15, -77),   S(  2, -63),   S(  3,  50),   S( 57,  86),   S( 65, 150),   S( 44, 156),   S( 58, 124),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  46),   S(-44,  44),   S( -4,  22),   S( 54,   6),   S( 64,  -3),   S( 13,   2),   S(-18,  11),   S(-52,  47),
            S(-73,  39),   S(-38,  40),   S(-19,  22),   S( -4,  21),   S(-18,  21),   S(-26,   7),   S(-52,   5),   S(-74,  34),
            S(-32,  28),   S(-32,  53),   S( 26,  27),   S(  8,  39),   S(-26,  42),   S(-60,  30),   S(-61,  32),   S(-61,  44),
            S(-24,  37),   S(-11,   9),   S(-29, -39),   S(  3, -27),   S( -4,  -6),   S(-48,  30),   S( -9,  29),   S(-28,  56),
            S( 59,   7),   S( -6, -34),   S( 32, -95),   S(  8, -73),   S( 52, -42),   S( 19,  20),   S(-18,  69),   S(-43, 118),
            S( 51,  32),   S( 19, -12),   S(-27, -66),   S(-17, -60),   S(-30, -56),   S( 46,  40),   S( 63, 136),   S( 41, 148),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  42),   S(-58,  20),   S(-17,   4),   S(  8,   4),   S( 10,  23),   S( 19,  10),   S( 14,   7),   S(  3,  27),
            S(-78,  25),   S(-59,  16),   S(-47,   9),   S( 17,  11),   S(-12,  25),   S( -9,  11),   S(-17,  12),   S( -8,  13),
            S(-61,  36),   S(-72,  42),   S(-47,  29),   S(-38,  42),   S(  1,  40),   S( 15,  17),   S(  3,  22),   S(-12,  19),
            S(-90,  89),   S(-60,  58),   S(-28,  32),   S(-15,  15),   S(-11, -34),   S(-17, -30),   S(-25,   6),   S( 32,  -1),
            S(-17, 104),   S(-50,  73),   S( 26,   9),   S( -8, -33),   S(  6, -72),   S(-38, -67),   S( -5, -33),   S( 81,  -5),
            S( 79,  77),   S( 68,  90),   S( 46,  24),   S( 41, -78),   S( -4, -103),  S(-39, -53),   S( -5, -45),   S( 76,   3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52,   4),   S(-41, -13),   S( -2, -22),   S(-64,  44),   S( 21,   5),   S( 68, -19),   S( 59, -27),   S( 69, -10),
            S(-60,   4),   S(-64,   6),   S(-35, -20),   S(-38,   5),   S(  1,  -2),   S( 45, -28),   S( 28, -15),   S( 50, -16),
            S(-58,  26),   S(-78,  40),   S(-39,   6),   S(-43,   2),   S(  0,  -2),   S( 24, -17),   S( 56, -15),   S( 53, -17),
            S(-55,  63),   S(-90,  81),   S(-57,  61),   S(-34,  36),   S(-14,  -2),   S( 40, -59),   S( 15, -71),   S( 20, -106),
            S( 14,  62),   S(-65, 136),   S( -9, 118),   S(-11,  86),   S( 11,  20),   S( 17, -81),   S(-43, -134),  S(-13, -99),
            S(130,  85),   S( 82, 123),   S( 89, 105),   S( 58,  95),   S( 34,   4),   S(  5, -102),  S(-25, -92),   S( -6, -181),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 27,   5),   S( 11,   8),   S( 51,  -3),   S( -9, -37),   S(-28, -61),   S(-15, -27),   S( 16, -56),   S( 41, -40),
            S( 16, -61),   S( 15, -17),   S(-36, -58),   S(-53, -38),   S(-26, -59),   S( 35, -64),   S( 12, -66),   S( -2, -51),
            S( 34, -96),   S( 11, -57),   S(  0, -66),   S(-41, -54),   S(-25, -30),   S( 14, -43),   S(-39, -20),   S(  1, -29),
            S(  5, -25),   S(-24, -38),   S( 18, -19),   S(-11,  -3),   S(-22,   9),   S(  1,  21),   S( -8,  25),   S(-11,  25),
            S( 24,   7),   S( -1, -32),   S(  8,  44),   S( 33,  92),   S( 53, 119),   S( 29, 118),   S( 12,  96),   S(-31, 105),
            S( 20,  34),   S(  4,  56),   S( 25,  70),   S( 32,  98),   S( 46,  95),   S( 50, 148),   S( 39, 101),   S(-21,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 31,   8),   S( 33,  21),   S( 25,  15),   S(  4,  27),   S( 20,   1),   S( 20, -19),   S( 32, -45),   S(-16, -18),
            S( 58, -59),   S( 19, -51),   S( 12, -59),   S(-13, -45),   S(-23, -30),   S(-43, -31),   S(-44, -35),   S( 20, -44),
            S( -9, -43),   S(-26, -43),   S(-18, -75),   S(-58, -43),   S(  1, -36),   S( -7, -49),   S(-53, -35),   S( 17, -32),
            S(-42,   0),   S(-42, -50),   S( -2, -69),   S(-37, -29),   S(  1, -42),   S( -3, -25),   S( 13,  -8),   S(  3,   9),
            S(  4,  13),   S( -8, -20),   S(-17,   2),   S( 20,  29),   S( 17,  60),   S( 18,  53),   S( -1,  67),   S(  0,  64),
            S( -9,  68),   S( 27,  61),   S( -2,  58),   S( 22,  57),   S( 26, 107),   S( 15,  84),   S( 16,  80),   S( 16,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-23, -52),   S( -1, -48),   S(  0, -20),   S( -2, -12),   S( 35,  14),   S( 75,   7),   S( 21,   3),   S( 14, -19),
            S( -6, -61),   S(-62, -42),   S( -8, -54),   S( 22, -40),   S(  1, -30),   S( -8, -24),   S( 20, -40),   S( 18, -44),
            S(-18, -46),   S(-84, -23),   S(-60, -42),   S( -8, -33),   S(-16, -48),   S( -9, -64),   S(-22, -63),   S( 66, -69),
            S(-35,  -1),   S(-19,  -7),   S(-22, -35),   S(-52, -39),   S(  6, -68),   S(-46, -52),   S(-22, -52),   S( 20, -50),
            S( 11,  16),   S( 31,  15),   S( 16,  10),   S(-19,  -3),   S( 10,  19),   S( 13,  12),   S(-31,  10),   S( 41,  -3),
            S(  7,  25),   S(  1,  48),   S( 25,  54),   S(  7,  56),   S( 24,  82),   S(  1,  43),   S(-13,  21),   S( 25,  39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31, -46),   S(  1, -46),   S(-29, -44),   S(  5,  -4),   S(  2, -22),   S( 71,   6),   S( 55, -10),   S( 59, -10),
            S(-35, -61),   S(-49, -61),   S(-33, -74),   S(  0, -64),   S(-18, -34),   S( 16, -49),   S( 28, -47),   S( 41, -70),
            S(-22, -39),   S(-86,  -5),   S(-26, -26),   S( -5, -29),   S(-61, -45),   S( 44, -69),   S( 29, -121),  S( 87, -102),
            S(-50,  22),   S(-69,  31),   S(  6,  24),   S( 19, -11),   S(-27, -15),   S(-20, -46),   S(-33, -53),   S( 41, -94),
            S(-16,  21),   S(-19,  66),   S(-10,  92),   S( 17,  57),   S( 27,  60),   S(-10,   5),   S( -1,   6),   S(  8, -25),
            S( 16,  69),   S( 27,  56),   S( 30,  80),   S( 25,  80),   S( 12,  63),   S( 33,  80),   S( 12,  30),   S( 28,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -104),  S( 28, -53),   S( -3, -29),   S(  0,  -2),   S( -6, -31),   S(-36, -72),   S( 17, -46),   S(  5, -45),
            S( 40, -85),   S( 28, -46),   S(-22, -76),   S(-32, -60),   S(-31, -88),   S(-11, -62),   S(-13, -89),   S(-20, -67),
            S( -7, -61),   S( -9, -78),   S(-23, -98),   S(-25, -84),   S(-10, -55),   S( -7, -48),   S(-38, -59),   S( -9, -76),
            S(-12, -37),   S( -4, -17),   S(-19, -19),   S( -3,   1),   S( 17,  56),   S(  3,  39),   S(  3,  10),   S( -7,  -2),
            S( 11,  22),   S(  1,  16),   S(  2,  23),   S( 19,  62),   S( 30,  76),   S( 24,  86),   S( 13,  79),   S( 18,  51),
            S( 12,  30),   S(  1,  37),   S( 12,  52),   S( 12,  60),   S( 25,  99),   S( 24,  92),   S(-21, -24),   S(-13,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -62),   S( 27, -83),   S( 19,   3),   S( -2, -13),   S(  5, -21),   S(-28, -42),   S( -9, -75),   S(-14, -68),
            S( 29, -132),  S( 20, -101),  S(  0, -87),   S( 12,  -9),   S(-24, -53),   S(  2, -80),   S(  1, -92),   S(  3, -88),
            S( 32, -88),   S(-10, -76),   S( -3, -91),   S(  7, -60),   S(-42, -28),   S( 22, -76),   S( -7, -75),   S( 59, -88),
            S( 17, -26),   S( -2, -35),   S(  1, -30),   S( -4,  25),   S( 13,   5),   S(-17,   7),   S(-13, -17),   S(  8, -21),
            S( -3,  42),   S(  7,  24),   S( -2,   5),   S( 22,  55),   S( 38,  78),   S( 26,  88),   S( 11,  96),   S( -7,  56),
            S( 12, 104),   S( 29,  52),   S(  3,  34),   S( 12,  45),   S( 20,  63),   S( 10,  50),   S( -4,  39),   S(  2,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -117),  S(  4, -71),   S( -5, -43),   S(  3,   3),   S( -6, -15),   S(  0,  -3),   S( 21, -71),   S( -9, -42),
            S( 18, -113),  S(-37, -107),  S( -4, -82),   S(-28, -88),   S( -9, -57),   S( 18, -51),   S(  2, -65),   S( 26, -85),
            S( 18, -94),   S(-20, -78),   S(-13, -63),   S(  4, -75),   S(-23, -52),   S(  4, -91),   S(  0, -101),  S( 36, -59),
            S(  5, -32),   S(-22, -40),   S( -5,  -6),   S(-21, -10),   S( 13, -52),   S( -5, -30),   S( 12, -30),   S( 12,  -5),
            S(-14, -14),   S(  6,  36),   S( 11,  49),   S( -8,  15),   S( 19,  70),   S(  1,  16),   S( 17,  45),   S( 23,  66),
            S( -4,  34),   S(  7,  49),   S( 27,  73),   S( 22,  70),   S( 16,  59),   S(  1,  34),   S( 23,  82),   S( 23,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -27),   S(  3, -59),   S(-26, -58),   S(-10, -27),   S(-12, -28),   S(-13, -42),   S( -9, -58),   S(  3, -86),
            S(-23, -66),   S(-21, -100),  S(-17, -107),  S(-11, -38),   S(-19, -25),   S( -7, -30),   S( 12, -54),   S( 12, -105),
            S(-27, -46),   S(-33, -63),   S(-44, -54),   S(  6, -39),   S(-32, -40),   S( -8, -75),   S(  4, -49),   S(  6, -44),
            S(  9, -36),   S(-26, -17),   S( -2,  40),   S(-19,  12),   S( 10,   6),   S(-10, -20),   S( -7, -13),   S( -8,  34),
            S(  6,  46),   S(  1,  51),   S(  0,  68),   S( 11,  59),   S( 24,  80),   S( 11,  62),   S( 17,  57),   S( 10,  23),
            S(-21,   5),   S( -7,   1),   S( 10,  73),   S( 21,  55),   S( 21,  70),   S( 18,  56),   S( 11,  35),   S( 16,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-72, -27),   S(-32, -27),   S(-22,  -5),   S(-19,  20),   S(-28, -25),   S(-31,  -7),   S( -9, -25),   S(-75, -39),
            S( 14, -36),   S( -4,   2),   S(-26, -32),   S( -7, -12),   S(-14,  -8),   S(-13, -21),   S(-35, -49),   S(-21, -39),
            S(-18, -21),   S( 17, -31),   S( -2,   5),   S( 26,  23),   S(-14,   8),   S(  3,  -5),   S(-33,  21),   S(-26, -33),
            S( 10,  20),   S( 38,  46),   S( 25,  29),   S( 43,  14),   S( 27,  16),   S( 12,  23),   S( 36, -19),   S( -9, -16),
            S( 61,  43),   S( 22,  54),   S( 54,  60),   S( 63,  37),   S( 65,  29),   S( 11,  23),   S( 15,  -7),   S(  5,   1),
            S(103, -31),   S( -7,  53),   S(143,   0),   S( 71,  38),   S( 52,  40),   S(-38,  58),   S( 34, -10),   S(-19,   5),
            S( 56,  -1),   S( -2, -21),   S( 50,  19),   S( 85,  64),   S( 41,  24),   S(  1,  31),   S(-13,   7),   S(-46,   6),
            S(-108, -118), S( -1,   0),   S(  7,   3),   S( 19,  25),   S(  4,  32),   S( 20,  14),   S(-30,   0),   S( -3,  16),

            /* knights: bucket 1 */
            S( 21,   3),   S(-61,  17),   S(-31,   9),   S(-48,  27),   S(-28,  32),   S(-25, -24),   S(-31,  -5),   S(  5, -14),
            S(-35,  38),   S(-50,  56),   S(-28,  23),   S(-13,  17),   S(-24,  17),   S( -9,  21),   S(-11,  -5),   S(-12, -50),
            S(-37,  29),   S( -3,  -1),   S(-22,  14),   S(-13,  48),   S(-18,  34),   S(-10,   6),   S(-43,  29),   S(-13,  24),
            S(-18,  71),   S( 30,  31),   S( -7,  50),   S( -5,  58),   S( -7,  53),   S(-10,  53),   S( -1,  19),   S(-25,  52),
            S( 66,  -1),   S(  8,  22),   S( 43,  56),   S( 21,  48),   S( 40,  47),   S( -6,  65),   S( -9,  49),   S( -3,  62),
            S( 31,  24),   S( 63, -12),   S( 81,  18),   S( 95,  28),   S( 71,  22),   S(-33,  72),   S( 19,  27),   S(  3,  41),
            S( 24,   2),   S( 38,  -7),   S( 34, -15),   S( 26,  48),   S( 16,  35),   S(  4,  19),   S( 17,  71),   S(-28,  48),
            S(-140, -22),  S( 16, -17),   S(-33, -58),   S(-17,  12),   S( -3,  11),   S( 41,  48),   S( 19,  50),   S(-55,  34),

            /* knights: bucket 2 */
            S(-61,  17),   S(-38,  26),   S(-29,   1),   S(-23,  15),   S(-22,  12),   S(-55,  -2),   S(-29,   3),   S(-14, -17),
            S(-15,  10),   S( -4,  35),   S(-22,   6),   S(-19,  15),   S(-29,  22),   S(-19,   5),   S(  8,   6),   S(-29,   7),
            S(-32,  51),   S(-21,  19),   S(-25,  16),   S(-25,  54),   S(-24,  42),   S(-23,   7),   S(-26,  13),   S( -2,  -6),
            S(-11,  57),   S( -5,  42),   S(-26,  72),   S(-18,  74),   S(-37,  71),   S(  2,  47),   S(  7,  31),   S( -4,  37),
            S( -8,  65),   S(-17,  66),   S(  6,  63),   S( 17,  59),   S(  2,  65),   S( 20,  66),   S( -5,  60),   S( 23,  16),
            S(-39,  70),   S(-20,  49),   S(-15,  79),   S( 44,  26),   S( 49,  27),   S(130,  -6),   S( 70,  10),   S( 31,  -6),
            S( 36,  41),   S(-38,  58),   S( 49,  19),   S( 33,   5),   S( -1,  41),   S( 18,  -9),   S( 39,  24),   S( 32,  -4),
            S(-47,  43),   S( 33,  67),   S(-12,  68),   S( -8, -24),   S(-19, -10),   S(-27, -42),   S( 18,   1),   S(-110, -41),

            /* knights: bucket 3 */
            S(-51,  29),   S(-14, -49),   S( -1, -23),   S( -2, -18),   S(  1, -17),   S( -9, -32),   S(-20, -28),   S(-22, -69),
            S(-13, -25),   S( -1,  -1),   S(  8, -14),   S( -3,  -3),   S( -5,  -3),   S( 20, -21),   S( 22, -34),   S( 23, -51),
            S(-11,  -1),   S(-11,   5),   S(  0,  17),   S(  3,  39),   S( 10,  26),   S( -1,  13),   S(  9,   1),   S( 20, -34),
            S( 10,   2),   S( 16,  26),   S( 17,  42),   S( 10,  52),   S( 13,  66),   S( 28,  53),   S( 32,  46),   S( 16,  37),
            S( -1,  44),   S( 22,  32),   S( 25,  51),   S( 29,  75),   S( 31,  71),   S( 34,  79),   S(  8,  90),   S( 63,  79),
            S( -9,  33),   S(  8,  41),   S( 11,  51),   S( 20,  68),   S( 59,  68),   S(143,  63),   S( 61,  83),   S( 24, 105),
            S(-20,  45),   S(-16,  53),   S(-18,  62),   S( 30,  57),   S( 46,  60),   S(106,  39),   S( 12,  -2),   S( 92,  22),
            S(-136,  51),  S(-30,  82),   S(-46,  86),   S( 37,  48),   S( 65,  74),   S(-46,  70),   S(-24, -40),   S(-53, -100),

            /* knights: bucket 4 */
            S( 10,  13),   S(-11, -11),   S(-61,  17),   S(-38, -16),   S(-39,  20),   S(-22, -16),   S( 19, -29),   S(-18, -13),
            S( 22,  40),   S(  3, -21),   S( -6,   4),   S(-21,   5),   S( -9, -16),   S(  9, -47),   S(-12,  10),   S(-43,  -2),
            S( -3, -18),   S( 30,  -6),   S( 53,   0),   S( 65,   1),   S( 13,  16),   S( 33, -32),   S(-12, -26),   S( -7, -32),
            S(-17, -24),   S( 29,   1),   S( 50, -17),   S( 71,  -3),   S( 35,   8),   S( -2,  25),   S(-33,  26),   S( -5,  12),
            S(  8, -36),   S( 35,  -8),   S( 67,  11),   S( 35,  38),   S( 53,   1),   S( 15,  17),   S( 24,  -8),   S(-29,  44),
            S( -5, -25),   S(  2,   0),   S( 40, -25),   S( 58,  16),   S(  4,  15),   S(-22,  35),   S(-18,   3),   S( 21,   5),
            S(-16, -29),   S(-20,  -6),   S(  4,  -3),   S( 23,  20),   S( 25,   9),   S( -3,  10),   S( 11,  39),   S(-33,  -9),
            S(  4,  16),   S(-11, -34),   S( -6, -30),   S( 16,   4),   S( 14,  21),   S( -4,  17),   S( -4,  19),   S(-15, -12),

            /* knights: bucket 5 */
            S( 21,  32),   S(  9,  28),   S(-38,  33),   S(-20,  22),   S(-19,  30),   S(  5,  15),   S(-16,  18),   S( 11,  31),
            S( 20,  30),   S( 28,  28),   S(  0,   4),   S(-24,  12),   S( 27, -11),   S(-31,  14),   S(-11,  40),   S(-49,  20),
            S(-29,  27),   S( -8,   2),   S( 33,   4),   S( 36,  10),   S( 21,  14),   S(-19,  21),   S( -8,  14),   S(-48,  19),
            S( 34,  17),   S( 33, -19),   S( 58,  -6),   S( 89, -20),   S( 85,   2),   S( 77,   5),   S( -4,  20),   S( 14,  35),
            S( 43,   3),   S( 40, -10),   S( 94, -15),   S(134, -11),   S( 93, -17),   S( 46,  13),   S(  4,   7),   S( 17,  26),
            S(  0, -23),   S( 39, -29),   S(  5, -27),   S( 14,  14),   S( 26,  -2),   S( 48,  -4),   S( -6,   9),   S( 27,  34),
            S(  2,   7),   S(-25, -54),   S(  3, -49),   S(-10, -18),   S( -6, -36),   S(  5,   4),   S( -4,  42),   S( 19,  38),
            S(-19, -26),   S(-25, -62),   S( 10, -10),   S(-23, -27),   S(  7,   0),   S(  1,  33),   S( 22,  41),   S(  1,  28),

            /* knights: bucket 6 */
            S( -4,  -1),   S(-54,  26),   S(-29,   6),   S(-49,  37),   S(-53,  34),   S(-24,  36),   S(-15,  42),   S(-31,  16),
            S(  7, -10),   S(-18,  49),   S(-21,   2),   S( 13,   8),   S( 12,  16),   S(-45,  36),   S(-23,  51),   S(-37,  72),
            S(-10,  20),   S( 11,  15),   S(  2,  21),   S( 27,  30),   S( 33,  25),   S(-28,  32),   S( 19,  27),   S(-21,  43),
            S(  9,  51),   S( 54,   5),   S( 41,  22),   S( 70,   9),   S( 89, -11),   S( 76,   7),   S( 27,  15),   S(-11,  50),
            S( -7,  43),   S( 37,  11),   S( 91,   3),   S(120,  -3),   S(112, -19),   S( 71,  21),   S(128, -20),   S( 20,  29),
            S( 11,  21),   S( 24,   5),   S( 58,  11),   S( 44,   2),   S( 54,  -6),   S( 40,  -4),   S( 15, -12),   S( 29,   2),
            S(  4,  34),   S( 14,  35),   S( 36,  35),   S( -1,  -6),   S( 28, -14),   S( 22, -38),   S( -6,  -3),   S( 12,  42),
            S( 20,  43),   S(  2,  33),   S( 18,  35),   S(  5,  20),   S( 10,  -6),   S( -6,  -2),   S( 11,  27),   S(-19, -27),

            /* knights: bucket 7 */
            S(-32, -37),   S(-24, -44),   S( -6, -15),   S(-49,  19),   S(-12,  -3),   S(-45,   8),   S(-17,  -9),   S(-13,  29),
            S(-32, -51),   S(-13, -26),   S(-44,  -6),   S(-41,  -1),   S( -8,   8),   S( -4,  22),   S(-10,  15),   S(-63,  41),
            S( -6, -35),   S(-45, -20),   S(  2, -17),   S(  0,  18),   S( 44,  13),   S( 38,   5),   S( 25,  13),   S( -3,  32),
            S(-40,  16),   S(  6,  -5),   S( 53, -19),   S( 83,  -1),   S(107, -11),   S( 87,  12),   S( 72,   2),   S( 66,   6),
            S(  2,   7),   S( -8,  12),   S( 17,  15),   S( 79,  -5),   S(105,  -1),   S(154, -27),   S(193, -16),   S( 39,  -3),
            S(-19,  15),   S( 22,   8),   S( -6,   5),   S( 52,  11),   S( 98,  -6),   S( 97, -11),   S( 18,  -8),   S(  6, -40),
            S(-20,   6),   S(-10,   5),   S( -6,  13),   S( 25,  20),   S( 55,  14),   S( 30,  21),   S(-12, -32),   S(-15, -35),
            S(-27, -33),   S( -9,  10),   S( -2,  25),   S(  4,  20),   S( 14,  11),   S( 19,  13),   S(  5,  -4),   S(  2,  -8),

            /* knights: bucket 8 */
            S( -1,   7),   S( 12,  31),   S( 12,  27),   S( -9, -29),   S( -2,  21),   S( -5, -18),   S( 13,  24),   S( -2, -12),
            S( -6, -23),   S( -5, -23),   S( -8, -39),   S(-11,   3),   S( -7,  33),   S( -2,  -6),   S(  0,  -6),   S( -3,  -3),
            S(-11, -40),   S( -8, -25),   S(  1, -51),   S(  3,   6),   S(-10, -23),   S( 12,   6),   S( -3,  -4),   S( -1, -14),
            S(-17, -52),   S( -9, -30),   S(  9,  15),   S(  0,   8),   S(-19, -18),   S(-24, -14),   S(-21, -28),   S(-15, -35),
            S( -6, -24),   S(  5, -17),   S( -1, -19),   S( -1,  -6),   S(-17,   0),   S(-12, -14),   S(  4,   0),   S( -1, -11),
            S( -3,   9),   S( 12,  -2),   S( -2,   4),   S( -7, -13),   S( -8,  -4),   S( -6, -12),   S( -9,  -5),   S( -7, -18),
            S(  0,  17),   S( -1, -25),   S(-12, -20),   S(  5,  13),   S(  2,   4),   S( -1,  -1),   S( -4,   5),   S( -3, -17),
            S(  1,   2),   S( -3,   7),   S( -5,   3),   S(  3,  -1),   S( -1,   7),   S( -1,  -5),   S(  0,   5),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-18, -57),   S( -5,  -2),   S( -2, -32),   S( -4, -34),   S(-16,  -9),   S(-12,  11),   S(  5,  21),   S(  2,  -6),
            S( -4,   6),   S(-14, -43),   S(-21, -107),  S(-26, -67),   S(-10, -35),   S(-22, -56),   S(-11,  -2),   S(-12,   2),
            S( -9, -21),   S(-16, -43),   S(-12, -41),   S( -4, -59),   S(-22, -13),   S( 10,   4),   S(-13,  -6),   S( -3,   6),
            S(-17, -47),   S(-11, -43),   S( -7, -27),   S( -8, -38),   S(-13, -32),   S(  4,  -2),   S(-17, -40),   S(  2,  11),
            S(  3,  25),   S( -8, -25),   S( -1, -22),   S( -1, -32),   S(-10, -25),   S( -4,  10),   S( -9, -11),   S( -5,   1),
            S(-12, -19),   S(-18, -34),   S(-11, -22),   S( -4, -18),   S(  1,  15),   S( -8,   0),   S( -3,  19),   S(  0,  10),
            S(-10, -15),   S(  0,  20),   S(-12,  -6),   S(-23, -19),   S(  2,   5),   S(  1,  23),   S( -7,  18),   S( -6,   4),
            S(  4,   2),   S(  4,   3),   S( -1,  12),   S(  0,   8),   S(-10,  -5),   S( -4,   4),   S(  3,  10),   S(  0,  15),

            /* knights: bucket 10 */
            S( -8, -28),   S( -5,  11),   S(-10,  -9),   S(-11,  18),   S(-20, -44),   S(  9, -15),   S( -3,  11),   S( -1,  21),
            S( -4, -16),   S(  7,   3),   S(-14, -24),   S(-10, -45),   S( -8, -32),   S(-25, -56),   S( -7,  15),   S(  3,  31),
            S( -3,  -3),   S( -4, -10),   S( -7, -20),   S(  6, -50),   S(-26, -44),   S( -3, -23),   S( -9, -32),   S( -9,  11),
            S( -9, -14),   S(-11, -23),   S( -7, -17),   S( -2, -24),   S(-10, -16),   S( -4,  -4),   S( -8, -48),   S( -3,  -2),
            S(-13, -19),   S(-12, -29),   S( -9,  -5),   S( -6, -15),   S(  5,  -6),   S( -7, -38),   S( -3,  -8),   S(  5,   9),
            S( -1,  14),   S(-11,   1),   S(-10,   7),   S(-13,  17),   S(-15, -20),   S(-19, -17),   S(-13,  -4),   S(-17,  -4),
            S(  3,  10),   S( -3,  -2),   S( -6, -28),   S( 12, -20),   S( -6,   3),   S(-16, -45),   S( -8,   8),   S(-10, -10),
            S( -1,   4),   S( -1,  10),   S( -1,  18),   S( -4,   5),   S( -4,   6),   S( -6, -10),   S(  6,  12),   S(  2,  10),

            /* knights: bucket 11 */
            S( -3, -14),   S(-26, -27),   S( -4,  -5),   S(  4,  21),   S(-38, -32),   S(  0,  15),   S( -5,   9),   S(  9,  34),
            S( -6, -14),   S(-27, -42),   S(-13, -42),   S( 15,  -3),   S(  8,  18),   S( -3, -27),   S(-13, -21),   S( -8, -11),
            S(-13, -40),   S(-19, -20),   S( -2, -15),   S(  2,  -8),   S( -8,  15),   S( 17,  -6),   S(  0, -13),   S( -3,  -3),
            S(-14, -11),   S(  5, -20),   S( -3, -25),   S( 11,  -2),   S( 29,  -7),   S(  1, -19),   S( 14,  21),   S(  0,  -4),
            S(-16,   2),   S(  3, -37),   S(-19,  -2),   S(  1, -16),   S( 32,   8),   S(  5,  17),   S( -8, -65),   S(-10, -10),
            S( -8, -22),   S( -7, -43),   S(  3,   6),   S(  9,  -3),   S(  8,  32),   S( -8, -13),   S( -4, -27),   S( -1,  20),
            S(  0,  -6),   S( -8,  18),   S(-11, -10),   S(  6,  -3),   S( 13,  -3),   S(  4, -18),   S(  1, -15),   S( -4,   2),
            S( -3, -16),   S(  2,   7),   S( -3,  -8),   S(  2,  17),   S( -4,  -8),   S(  0,  -7),   S(  5,  17),   S( -1,  -4),

            /* knights: bucket 12 */
            S(-14, -39),   S( -3, -10),   S( -1, -19),   S(  0,   8),   S( -5,   7),   S( -5, -10),   S( -1,   6),   S( -1,   1),
            S( -3,  -9),   S(  0,   3),   S(  0, -15),   S( -3,   5),   S( -4,  -8),   S(  0,   3),   S(  1,  -1),   S(  0,  -8),
            S( -2, -10),   S( -6, -21),   S( -6, -20),   S(-15, -25),   S( -9,  -5),   S( -3,  25),   S( -3,   1),   S( -5,  -9),
            S(  2,   9),   S( -1, -35),   S( -7,  26),   S(  3,  13),   S( -4, -12),   S(  3,  21),   S(  5,  12),   S(  2,   8),
            S(  0,   2),   S( -4,  -6),   S( -5, -21),   S( -4,  -9),   S(  0,   4),   S( -3,   5),   S( -6,  -3),   S( -9,  -8),
            S( -4,  -2),   S( -1,  -3),   S( -3, -14),   S( -3, -11),   S( -3,  -1),   S( -7, -20),   S(  7,   8),   S( -1,   8),
            S( -4,  -8),   S( -2,  -1),   S(-10,  -1),   S( -2,  -6),   S(  0,   9),   S( -9,  -8),   S( -5, -19),   S( -3,  -2),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -6),   S(  1,   3),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -7),   S( -4, -12),   S( -3, -16),   S( -2,  -6),   S( -3, -11),   S( -2,   7),   S( -6,  -5),   S(  3,  10),
            S( -2,  10),   S( -2,  -2),   S(  3,   9),   S( -4,  -2),   S( -6, -11),   S( -1,   9),   S(  2,  20),   S( -3,  -5),
            S(  5,  -1),   S(  5,   9),   S(  5,   1),   S( -4, -25),   S(  4,  22),   S( -5,   7),   S(  7,   4),   S( -3,  -2),
            S(  0,  14),   S(  0,   4),   S( -6,  -3),   S(  1,  26),   S(  0,  10),   S( -2,  28),   S(  0,   7),   S( 10,  20),
            S(  1,  21),   S( -2, -16),   S( -4,  11),   S( -7,   7),   S(-16,  -3),   S( -3,  24),   S( -8, -22),   S( -3,  -3),
            S( -3,  -4),   S(  2,   2),   S( -4,   9),   S(  3,  12),   S( -9,   6),   S( -8,   3),   S(  3,  21),   S(  0,   3),
            S(  1,   4),   S(  3,   8),   S( -6,  -4),   S( -4,   2),   S( -2,   7),   S( -3,  -7),   S(  2,   7),   S( -1,   1),
            S(  2,   6),   S(  1,   2),   S( -2,  -2),   S(  2,   5),   S(  0,   1),   S(  1,   2),   S( -1,  -2),   S(  0,   2),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   4),   S(  5,  18),   S( -2,   1),   S( -6, -24),   S( -2,  18),   S(  2,   3),   S(  0,   3),
            S( -2,  -9),   S( -8, -16),   S(  2,  -5),   S( -1,  -1),   S(  3,   2),   S(  1,   4),   S( -7,   6),   S(  6,  57),
            S( -1,   0),   S( -5, -33),   S(  6,  16),   S(-10, -32),   S( -3,  -2),   S(  1,   8),   S( -1,   9),   S(  3,  18),
            S( -1,  -4),   S( -3, -17),   S(-22, -15),   S( -2,  43),   S(  2,  40),   S( -4,  -9),   S(  0,   6),   S(  1,  36),
            S(  6,  15),   S(-17, -35),   S( -9, -10),   S( -8,   2),   S(  0,  31),   S(-11,   3),   S( -3,   0),   S(  4,  12),
            S( -1,   3),   S(  4,   5),   S(  2,  -5),   S( -3,  12),   S(  1,  16),   S(  1,  13),   S(  1,   7),   S( -5, -11),
            S(  0,   4),   S( -3,  -2),   S(  3,  16),   S(  6,   4),   S(  3,  12),   S( -5, -11),   S(  2,   7),   S(  4,   5),
            S(  0,  -1),   S(  0,   1),   S( -1,  -1),   S(  2,   4),   S( -1,   1),   S(  0,  -2),   S(  1,   0),   S(  1,   2),

            /* knights: bucket 15 */
            S( -2, -13),   S( -1,   4),   S(  4,  24),   S( -2,   5),   S( -4, -16),   S(-10, -34),   S( -4, -15),   S( -1, -10),
            S(  2,  -1),   S(  4,   5),   S( -6,  -7),   S(  9,  43),   S(  0,  13),   S( -8, -34),   S( -3,  -3),   S(  1,   2),
            S(  0,  -5),   S( -5, -20),   S(  1, -11),   S(  5,   7),   S(-17, -28),   S( -1,  -5),   S( -2,  -6),   S( -2,  -1),
            S(  0,  -7),   S( -3,   3),   S( -5, -14),   S( -6,   5),   S( -7,   4),   S(-10,  25),   S(  4,   5),   S( -1,   1),
            S( -1,  -2),   S(  9,  21),   S( -5,   5),   S( -7,   4),   S( 18,  33),   S(  0,  15),   S(  6,  -4),   S(  4,  17),
            S(  1,   4),   S( -4,  -9),   S( -1,   0),   S( -9, -18),   S( -6, -10),   S(  1,  15),   S(  0,   8),   S(  5,  12),
            S( -1,   1),   S( -2,  -7),   S(  4,  16),   S(  3,   4),   S(  3,  13),   S(  5,   8),   S(  1,   7),   S(  4,   9),
            S(  1,   4),   S( -1,  -5),   S(  0,   0),   S( -1,   1),   S(  2,  10),   S(  1,   1),   S(  0,   0),   S(  1,   3),

            /* bishops: bucket 0 */
            S( 25,  -3),   S( -8,  39),   S(-16,  11),   S(-24,  -6),   S( -6,   1),   S( -4,  13),   S( 66, -40),   S( 22, -10),
            S(-27,  -7),   S(-12, -21),   S(-23,  34),   S(  1,  11),   S( -1,  19),   S( 48,  -6),   S( 29,  24),   S( 44, -12),
            S( 15,  14),   S(  1,  24),   S(  2,  -8),   S(  6,  10),   S( 20,  20),   S( 28,  18),   S( 33,   4),   S( 26,   4),
            S( 17, -25),   S( 36, -38),   S( 14,  15),   S( 35,  14),   S( 66,  34),   S( 31,  46),   S( 14,  20),   S(  9,  27),
            S( 39, -13),   S( 44, -16),   S( 55,   8),   S( 80,  43),   S( 89,  23),   S( 17,  44),   S( 30,  47),   S( -3,  18),
            S( 52,  21),   S( 56,  43),   S(100,   3),   S( 56,  -2),   S( 16,  43),   S(  9,  35),   S( 37,  32),   S( -3,  14),
            S(-40, -75),   S( 73,  33),   S( 85,  80),   S( 20,   0),   S( 12,  -8),   S( 23,  32),   S(-31,  21),   S(-13,  55),
            S(-20, -38),   S( -4,  -8),   S( 13, -24),   S(-14, -13),   S(-12, -13),   S(-18,   9),   S(-17,  22),   S(-31, -34),

            /* bishops: bucket 1 */
            S(-61,  16),   S( -5,   1),   S(-23,  39),   S( 15,  -6),   S(-20,  24),   S(  3,   6),   S( 35, -10),   S( 27, -30),
            S(  4, -29),   S(-25, -13),   S( -6,  -5),   S(-17,  16),   S( 25,  -7),   S(  2,   4),   S( 43, -36),   S( 16,  -7),
            S(-10,   5),   S( 24, -10),   S(-26,  -9),   S( 16,   5),   S(  0,   0),   S( 23, -31),   S( 10,  -2),   S( 64,   0),
            S( 23, -13),   S( 50, -15),   S( 25,   3),   S( 22,  11),   S( 37,   1),   S(  9,  14),   S( 51,  -3),   S(  2,  17),
            S( 28,  -8),   S( 55, -12),   S( 17,   7),   S( 94, -16),   S( 50,  20),   S( 39,  21),   S( -2,  25),   S( 31,   6),
            S( 61, -40),   S( 49,   9),   S( 59, -27),   S( 70, -13),   S( 75,   2),   S(-41,  11),   S(-27,  55),   S(-29,  22),
            S( 17, -60),   S( -1, -52),   S( -7,   0),   S( 25,  45),   S( 27,  33),   S(-14,  30),   S(-24,   3),   S(-26,  39),
            S( -6, -23),   S(-13,   8),   S( -7, -21),   S(-48,  -1),   S(-22,  20),   S( 19,   4),   S( 28,   5),   S(-56,  -1),

            /* bishops: bucket 2 */
            S(  0, -18),   S(-12,  -4),   S(  0,  16),   S(-22,   8),   S( 10,  14),   S(-20,   8),   S( 16,  -6),   S( -2, -21),
            S( 25, -17),   S(  2, -33),   S( -6,  -7),   S(  6,  13),   S(-13,  13),   S(  5,   6),   S( -3, -32),   S( 18, -48),
            S( 45,   6),   S( 20,  -1),   S( -9,  -5),   S( -9,   9),   S( -5,  30),   S(-19, -32),   S(  3, -23),   S(-10,   0),
            S(-14,   7),   S( 45,  15),   S( -4,  18),   S( 24,  30),   S( -1,  16),   S( -4,  22),   S(-13,   0),   S(  7,  15),
            S(  1,  22),   S(-31,  42),   S( 48,  22),   S( 20,  29),   S( 21,  29),   S( 24,   8),   S( 11,  32),   S( 41,  -5),
            S(-32,  40),   S( -7,  40),   S(-35, -12),   S( 88,  -2),   S( 52,  13),   S( 98, -18),   S( 76,  13),   S( 42, -43),
            S(-35,  67),   S(-42,  -2),   S( -8,  22),   S(  6,  14),   S(-42,  -8),   S(-34,  13),   S(-38,  -2),   S(  1, -37),
            S(-82, -21),   S(-12,  28),   S(  3,  11),   S(-17,  29),   S(-29, -10),   S(-31,  11),   S(  1, -11),   S(-55, -19),

            /* bishops: bucket 3 */
            S( 37, -15),   S( 39, -16),   S( 20, -23),   S( 11,  -2),   S( 16,  10),   S( -3,  29),   S(-11,  51),   S( -1, -18),
            S( 41,   7),   S( 22, -30),   S( 20,  -1),   S( 22,   4),   S( 19,  19),   S( 22,   9),   S(  8, -20),   S( 35, -40),
            S( 17,  -3),   S( 33,  33),   S( 18,   5),   S( 17,  27),   S( 16,  30),   S(  7,  -5),   S( 21,  -9),   S( 16,  13),
            S( -5,  15),   S( 11,  43),   S( 24,  50),   S( 35,  46),   S( 36,  20),   S( 28,   6),   S( 28,  -2),   S( 38, -33),
            S(  9,  34),   S( 16,  52),   S(  4,  57),   S( 53,  47),   S( 48,  44),   S( 50,  21),   S( 30,  17),   S(  4,  15),
            S(  7,  35),   S( 22,  55),   S(  2,  13),   S( 16,  41),   S( 52,  39),   S( 78,  38),   S( 48,  40),   S( 46,  74),
            S(-24,  78),   S( -6,  24),   S( 11,  29),   S( -4,  56),   S( 24,  35),   S( 55,  48),   S(-29,  23),   S( 27, -21),
            S(-44,  11),   S(-32,  56),   S(-49,  44),   S(-32,  52),   S( 17,  13),   S(-58,  32),   S( 20,   8),   S( 15,  12),

            /* bishops: bucket 4 */
            S(-35,   6),   S(-30,   8),   S(-40,  17),   S(-56,  16),   S(-31,  -9),   S(-20,  -5),   S(-12, -19),   S(-37, -36),
            S( -6,   7),   S( -8, -17),   S( 59, -30),   S(-34,  16),   S(-55,  25),   S(-10, -29),   S(-27, -32),   S(-28, -20),
            S(  9,  24),   S(-10, -15),   S(  0,  -5),   S( -5,   5),   S( 14,  -9),   S(-67,   1),   S(-19, -30),   S(-52, -12),
            S( 31,   1),   S( 53, -15),   S( 33,  12),   S( 12,  28),   S( -9,  25),   S( 24,   2),   S(-44,   8),   S( -6, -19),
            S( 16, -11),   S( -8, -18),   S( 43,  -9),   S( 18,   6),   S( -2,  30),   S( 21,  11),   S(-22,  38),   S(-55,   6),
            S(-51, -80),   S(-45,  -3),   S( -9,   2),   S(  8,   7),   S(-45,  45),   S(  6,   5),   S(-14,  29),   S( -4,  32),
            S(  0,   4),   S(-27,  -5),   S(  1, -17),   S(-28, -11),   S(  1, -20),   S( 35,   4),   S( -8, -11),   S( 18,  35),
            S( -6,  -5),   S(  1, -17),   S(-13,  -8),   S(  2, -16),   S(-17,   5),   S(  4,  21),   S(  4,  47),   S(  6,   3),

            /* bishops: bucket 5 */
            S(-46,  -6),   S( 18,  -3),   S(-48,  24),   S(-55,  25),   S(-18,   7),   S(-61,  21),   S(-34,  24),   S(-48, -16),
            S( -8,  -1),   S(-30,  -6),   S( 19,   0),   S(-18,  19),   S(-59,  34),   S(-27,  24),   S(-34,  -5),   S(  8,  -6),
            S( 14,  32),   S(-17,   3),   S( 15, -23),   S( -1,  12),   S(-12,  23),   S(-66,   2),   S(-18,  20),   S(-16,  30),
            S( 20,  10),   S(  7,  15),   S( 70, -15),   S( 40,  15),   S( -8,  28),   S(  8,  23),   S(-57,  36),   S(-21,  24),
            S( 15,  -2),   S( 35,  -1),   S( -5,  13),   S( -4,   2),   S(  0,  13),   S( -5,  17),   S(  8,  24),   S(-47,  24),
            S(  5,  -8),   S(-34,  20),   S( 19, -22),   S( -9, -14),   S(-10,  10),   S(-17, -11),   S(-24,  23),   S(-35,  53),
            S(-19,  -7),   S( -7, -17),   S(-15,  -1),   S(  5,  24),   S( 17,   6),   S( -8,  29),   S( -4,   7),   S(-20,  38),
            S(-14,  -5),   S( -9, -15),   S(  1, -15),   S(-18,   0),   S(-22,  31),   S(  9,  38),   S(-17,  27),   S( 11,   5),

            /* bishops: bucket 6 */
            S(-10, -30),   S(-11,  10),   S(-39,  25),   S(-23,  15),   S(-63,  37),   S(-35,  22),   S(-40,  33),   S(-55,  -3),
            S(-34,  20),   S(-32, -26),   S(-59,  44),   S(-46,  34),   S(-48,  31),   S(-48,  22),   S(-40,   6),   S(-30,  17),
            S(  2,   9),   S(-38,  20),   S(-10, -13),   S(-35,  35),   S(-25,  37),   S(-28, -13),   S( -9,  -7),   S( -5,  26),
            S(-64,  33),   S(-51,  33),   S(-12,  21),   S( 20,  40),   S( 11,  37),   S( 20,  15),   S( 23,   6),   S(-11,  25),
            S(-44,  28),   S(-28,  33),   S(  9,  13),   S( 60,  16),   S(-11,  20),   S( -6,  10),   S( 14,  17),   S(-18,   1),
            S(-48,  46),   S(-18,  24),   S(-48,  -2),   S(-14,  16),   S( 17,  13),   S( -4,  -6),   S(  0,  22),   S(-26,   3),
            S(-14,  40),   S(-80,  32),   S(-27,  19),   S(-19,  26),   S( -5,   8),   S(  7,   6),   S( 11, -13),   S(-22,  17),
            S(-16,   3),   S(-24,  40),   S( -8,  35),   S( 29,  13),   S(-24,  24),   S( 20, -12),   S( -9,  13),   S(-12,  11),

            /* bishops: bucket 7 */
            S(-14, -52),   S(-51,  -7),   S(-39, -17),   S(-15, -10),   S(-40,  -3),   S(-39,  -6),   S(-64, -17),   S(-48, -10),
            S( -8, -46),   S(-11, -49),   S( 14, -22),   S(-27, -11),   S(-33,   2),   S(-45,   4),   S(-40, -31),   S( -3, -12),
            S(-44, -21),   S(-29,   2),   S(-19, -25),   S(  9,  -7),   S(  0,  -3),   S( -9, -38),   S(-52,   5),   S(-52,  11),
            S(-20, -21),   S(-62,  29),   S(-27,  13),   S(-13,  22),   S( 89,  -1),   S( -3,  14),   S( 40, -28),   S(-11,  -5),
            S(-23,   3),   S( 18, -10),   S(-45,  30),   S( 11,   4),   S( 55,  -7),   S( 50,   9),   S(-17,  14),   S(-30,  -7),
            S(-71,  36),   S(-40,  52),   S(-20,  -9),   S(-79,  32),   S(-27,  19),   S( 15, -12),   S(  6,  38),   S(-47, -76),
            S(-10,  -2),   S(-38,  -1),   S(-46,  22),   S( -4,   9),   S(  2,   0),   S( 24, -23),   S(  8, -28),   S(  4,  -9),
            S(-21, -31),   S( -4,   9),   S( -9,  15),   S( -1,   9),   S( -8,   3),   S( 12, -13),   S( 30, -26),   S(  0,  -4),

            /* bishops: bucket 8 */
            S( 33,  57),   S( -1, -35),   S( -1,  -1),   S(-10,  42),   S(  1,  20),   S( -6, -38),   S(-16, -26),   S(-11, -18),
            S(  1,  -1),   S( 14,  26),   S( 21,   7),   S(  8,  20),   S(  1, -16),   S(  2,   1),   S(-33, -51),   S(-10,   2),
            S( -6,  -6),   S(-14, -14),   S( 23,  25),   S( 11,  11),   S(  7,  15),   S( -5,  -4),   S(-25, -14),   S(-33, -26),
            S( -5, -12),   S( 31,  24),   S( -1,  26),   S( 25,  10),   S(  6,  35),   S( 11,  28),   S(-12,   9),   S(  3, -17),
            S( 15,  18),   S( 50,  57),   S( 20,  -2),   S( -5,  22),   S( 11,  23),   S(-24,  23),   S( -6, -25),   S(  5,  19),
            S( -8,  -6),   S(  2,   7),   S(  7,  19),   S( 25,  15),   S( 13,  32),   S( 25,   1),   S( -8,  60),   S( -2,  32),
            S(  3,  15),   S(-18, -45),   S( 26,  -2),   S( 24,   2),   S(  9,   0),   S( 22,  45),   S( 16,  22),   S(-13,  -2),
            S( -7,  -4),   S(  5,   4),   S(  1,  18),   S(  2,  11),   S( 29,   5),   S( 23,  13),   S( 15,  38),   S( 35,  25),

            /* bishops: bucket 9 */
            S(  6,  28),   S(  5,  14),   S( -2,  -1),   S(-31, -27),   S(-20, -10),   S( -7,  -5),   S( -2,  -2),   S( -9,  -7),
            S(  0,  -3),   S(  5, -13),   S(  5,  14),   S(-32,   4),   S(-27,  13),   S(-10, -13),   S(-38, -18),   S(-16, -28),
            S( -9,   4),   S( 17,   6),   S( -5, -24),   S(  4,  23),   S( 13,  14),   S(-31, -23),   S( -2,   7),   S(-11,  -4),
            S( -1,  24),   S(  0,  -9),   S( 30,   3),   S( 28,   6),   S( -1,  24),   S( -9,  16),   S(  4,  22),   S( -4,  16),
            S( 26,  19),   S( 19,  13),   S( 29,  22),   S( 18, -18),   S( 13,  32),   S(  0,  33),   S(  6,  35),   S(-15, -16),
            S( 18,  22),   S( -5,  29),   S(  8, -19),   S( 13,  19),   S( 41, -40),   S( -8,  11),   S( 16,  33),   S( 12,  29),
            S( 13,  10),   S(-13,   7),   S(  7,  12),   S( 20,  -1),   S( 23,   2),   S( 33,  17),   S( 14,  27),   S( 18,  57),
            S( 10,  35),   S(  2, -21),   S(  3,  24),   S( 11,  20),   S(  8,  44),   S( 19,  -2),   S( 26,  -1),   S( 27,  20),

            /* bishops: bucket 10 */
            S( -1, -32),   S( 12,  11),   S( -2, -19),   S(-24, -20),   S(-66, -18),   S(-33, -59),   S(  8,  -6),   S( -4,  13),
            S( -9,  19),   S( -6, -57),   S( -8, -18),   S(-22, -35),   S(-48,   6),   S(-30, -21),   S(-32, -19),   S(  1,   1),
            S(-10, -33),   S(-19, -18),   S(-19, -32),   S( -5,  27),   S(-14,  10),   S(-13, -34),   S( -7,   3),   S( -5, -19),
            S(-16,  14),   S(-23,   2),   S(-26, -27),   S(  9,   4),   S(-18,  53),   S( 31,  14),   S( 35,  29),   S( -5, -33),
            S( 11,   7),   S(-36,  24),   S( -1,   9),   S(  6,  37),   S( 40,  -8),   S( 26,  39),   S( 22, -14),   S( 17,  10),
            S(  7,  10),   S( 10,  16),   S(-11,  -4),   S( 27,  14),   S( 14, -13),   S(  0,  -7),   S( 11,   9),   S( 25,  14),
            S( 20,  38),   S( -6,  -1),   S( 31, -15),   S( 12,  29),   S( -1,  13),   S( -7, -23),   S(  0, -17),   S( 22,  29),
            S( 10,  24),   S( 21,  31),   S( 44,  17),   S(  8,  21),   S( -5,  25),   S(  6,  15),   S( 12,  18),   S(  0, -16),

            /* bishops: bucket 11 */
            S( 11, -17),   S( -8, -15),   S( -7,  -7),   S(  1,  -5),   S(-21, -16),   S( -4,  -4),   S(-22, -27),   S(-10,  -2),
            S( -7, -12),   S(  0, -26),   S(-12,   7),   S(  2, -14),   S(-15,  11),   S(-43,  -9),   S(-37, -16),   S(  8,   4),
            S(-10, -47),   S( -3, -17),   S(-16, -40),   S(-31,   5),   S( -6,  -9),   S(  6,  21),   S( -1,  -8),   S( -2, -16),
            S(  4,   0),   S( -2, -31),   S(  4,  -3),   S(-33, -18),   S( 13,   2),   S( 21,  50),   S( 42,  16),   S( -8, -26),
            S(-10, -13),   S(-13, -10),   S(-37,  39),   S(-26,  35),   S(-22,  33),   S( 38,   7),   S( 30, -10),   S(  8,   5),
            S( -6,   9),   S(-10,  -9),   S(-10, -10),   S(  2,  22),   S( 24,  20),   S(  7, -26),   S(  3, -12),   S( -1, -15),
            S( -1,  -6),   S( 13,  22),   S( 18,  48),   S( 32,  23),   S( 18,  -7),   S( -7,  -3),   S(-19, -31),   S( -7, -15),
            S( 27,  15),   S(  5,   3),   S( 28,  47),   S( 29, -18),   S( 17,  17),   S(  4,   7),   S( -6, -12),   S(  4,  -6),

            /* bishops: bucket 12 */
            S( -5, -11),   S( -4, -12),   S( -6,  -2),   S(  7,  20),   S( -9,  -9),   S( -8,  -5),   S( -1,   2),   S( -2,   1),
            S(  0,  -5),   S(  6,   2),   S( -1,  -3),   S(  1,  14),   S(  0,   8),   S(  9,   6),   S(-14, -22),   S( -1,  -4),
            S(  8,   5),   S( 11,  -3),   S( 21,  16),   S( 20,  15),   S( -3,  11),   S( -8,  -9),   S(  2,   5),   S( -5,  -3),
            S( 10,   2),   S( 17,   6),   S( 20,   7),   S( 18,  40),   S( 11,   7),   S(  5,  22),   S(  3,  13),   S(  3,   7),
            S( 11,  10),   S( 10,  10),   S( -2,  18),   S( 21,   8),   S( 19,  29),   S(  9,  29),   S(  7,  12),   S(  3,  11),
            S(  2,   0),   S( -8,  -9),   S( -6,  13),   S(  2,  -3),   S( 30,  30),   S(  9,   8),   S( -9,  -8),   S( -4, -10),
            S( -3,  -4),   S(  4,  10),   S(  3,  11),   S(  5,  -6),   S( 12,   2),   S( 20,  24),   S( 11,  24),   S( -1,  -2),
            S(  0,   4),   S( -1,  -4),   S(  1,  -3),   S(  0,  -5),   S(  2,   8),   S(  3, -10),   S( 14,   6),   S(  7,   4),

            /* bishops: bucket 13 */
            S( -5, -18),   S( -1,  -3),   S( -5, -14),   S( -6, -10),   S( 16,  13),   S( -8, -11),   S(-15, -20),   S( -2,  -4),
            S( -5,  -2),   S( -8, -14),   S( -1,   3),   S( 16,   1),   S( -6, -15),   S(  4,  12),   S( -1,  -7),   S(  0,  -3),
            S(  8, -11),   S( 30,  17),   S( 10,  -1),   S( 18,  28),   S(  3,  23),   S(  6,  18),   S( -8,   3),   S( -7,  -5),
            S( 25,  29),   S( 46,  15),   S( 22,  27),   S(-16,  11),   S( 17,  68),   S(  3,  13),   S(  9,   6),   S(  2,   9),
            S( 21,  22),   S( 16,  14),   S( 11,   4),   S(  9,  -7),   S( 11,  -4),   S( 11,  21),   S( 13,  16),   S(  3,  10),
            S(  6,   5),   S(  1,   7),   S( -4, -12),   S( 17,  -3),   S(  6,  14),   S( -7, -21),   S(  2,  -4),   S( 12,   0),
            S(  7,   7),   S(-10, -20),   S( -2, -17),   S(  4,   4),   S(  5,  18),   S( 17,  10),   S(  7,  -6),   S(  9,  12),
            S(  1,  -1),   S( -2,  -2),   S(  0,  12),   S(  2,   9),   S(  7,  15),   S(  3, -12),   S( 13,  -2),   S( 11, -10),

            /* bishops: bucket 14 */
            S(-13, -24),   S(  5,  21),   S( 16,  12),   S(  4,  21),   S(-12,  -3),   S( -8,  -6),   S( -5,   3),   S( -8,  14),
            S( -1,   1),   S( -2,  -4),   S(  2,  11),   S( -2,  -9),   S( 12,   2),   S(  2,   8),   S( -6,  16),   S(  3,  29),
            S(  1,  -4),   S( -2, -13),   S( -9, -16),   S( 19,  32),   S( 23,  44),   S( 11,  18),   S(  5,  36),   S(  3,  29),
            S(  4,  32),   S(  8, -13),   S( -3,  -1),   S(  2,  30),   S( 10,  20),   S( 21,   8),   S( 21,  17),   S(  9, -16),
            S( 10,   7),   S(  6,  14),   S( 12,   4),   S( 20,  10),   S( -3,   5),   S(  5,  14),   S( 22,   0),   S( 15,  11),
            S(  2, -11),   S( 23,  37),   S(  2,   7),   S( 15,   7),   S(  9,   1),   S( -6,   2),   S( -2,  20),   S( 16,   1),
            S( 16,  35),   S(  6,   7),   S( 12,  17),   S(  7,  11),   S(  7,   0),   S(  3,  12),   S(  0, -11),   S(  2,   1),
            S( 13,   0),   S( 13,  18),   S(  4,  10),   S(  5,   2),   S( -4,  -3),   S(  2,  -4),   S(  8,  11),   S(  3,   3),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -1),   S( -7, -15),   S( -2,  -1),   S( -7, -22),   S( -3,  -7),   S( -5, -14),   S( -4,  -5),
            S(  8,  12),   S( -4, -10),   S(  5,   4),   S(  4,   4),   S(  8,  -1),   S( -1,  -4),   S( -2, -10),   S( -3,  -5),
            S(  3,  -4),   S(  2,   0),   S(  0,  -8),   S( 13,  16),   S( 12,  29),   S(  8,  25),   S( 16,  21),   S(  4,   4),
            S(  1,  -8),   S( 13,  14),   S( 12,  31),   S(-17,  -4),   S(  3,  10),   S( 17,   6),   S( 14,   2),   S(  9,  17),
            S( -2,  -8),   S( -1,  13),   S( -4,  20),   S( 21,  54),   S( 20,  24),   S( 12,  -1),   S(  9,   2),   S( -3,   2),
            S( -2,  20),   S(  6,  12),   S(  4,  24),   S(  7,  11),   S( 23,  20),   S(  7, -12),   S(  3,   9),   S(  2,  -2),
            S(  5,  -2),   S(  2,  17),   S(  7,  30),   S( 13,  18),   S( 10,  15),   S( -2,   8),   S( -1,  -9),   S(  0,   0),
            S(  3,  -2),   S( 10,  13),   S(  7,  -1),   S(  9,  11),   S(  5,  17),   S(  1,  -1),   S(  4,  11),   S(  4,   0),

            /* rooks: bucket 0 */
            S(-20,  13),   S(  8, -11),   S(-11,   2),   S(-12,  17),   S(-33,  60),   S(-20,  36),   S(-51,  62),   S(-58,  47),
            S(  0, -22),   S( -4,  14),   S(-32,  21),   S( -5,  29),   S( -8,  42),   S(-11,  24),   S(-21,  12),   S(-28,  43),
            S( 22, -34),   S( 10, -15),   S(-13,  10),   S( -7,  13),   S(-36,  56),   S(-17,  15),   S(-22,  39),   S( -8,  16),
            S(  9, -21),   S( 37,  -5),   S(-32,  31),   S( 15,  15),   S(  8,  48),   S(-19,  46),   S(-24,  52),   S(-19,  32),
            S( 53, -61),   S( 44,  -2),   S( 21,  25),   S( 35,  22),   S( 41,  18),   S( 22,  66),   S( 33,  50),   S(  6,  58),
            S( 57, -28),   S( 64,  16),   S(111, -20),   S(104,  23),   S( 27,  54),   S( 31,  59),   S(  7,  68),   S(-41,  80),
            S( 24,  18),   S( 54,  46),   S( 97,  30),   S( 66,  11),   S( 61,  46),   S( 16,  61),   S(-10,  73),   S(-18,  67),
            S(  4, -20),   S( 28,  23),   S( 26,  22),   S( 43,  -4),   S( 23,  45),   S( 42,  13),   S( 34,  15),   S( 51, -43),

            /* rooks: bucket 1 */
            S(-56,  49),   S(-21,   6),   S(-15,  14),   S(-45,  31),   S(-42,  44),   S(-46,  47),   S(-53,  68),   S(-78,  74),
            S(-47,  35),   S(-23,  -9),   S(-24,  16),   S(-30,  24),   S(-34,  17),   S(-46,  42),   S(-27,  20),   S(-37,  53),
            S(-33,  26),   S(-12,  -6),   S(-13,  -1),   S(-25,  13),   S(-32,  18),   S(-52,  34),   S(-62,  62),   S(-29,  59),
            S(-42,  47),   S( -5,  10),   S(-16,  26),   S(-27,  15),   S(-37,  36),   S(-50,  66),   S(-34,  60),   S(-70,  88),
            S(-15,  43),   S( 18,  -8),   S( 33,  10),   S( 30,   1),   S(  4,  23),   S( -8,  78),   S(  5,  60),   S(-10,  83),
            S( 47,  33),   S( 72,  -4),   S( 39,  13),   S(  0,  30),   S( 12,  20),   S( 10,  57),   S( 38,  44),   S(  9,  79),
            S( 14,  66),   S( 34,   3),   S(  6,  32),   S( 17,  15),   S( 49,  12),   S(  2,  52),   S( 26,  62),   S( 32,  81),
            S( 50, -10),   S( 13,  -7),   S( -2,  -9),   S(-19, -10),   S( 22,   5),   S( 17,  16),   S( 32,  30),   S( 49,  34),

            /* rooks: bucket 2 */
            S(-61,  68),   S(-50,  60),   S(-42,  53),   S(-39,  23),   S(-28,  24),   S(-40,  26),   S(-31,  16),   S(-69,  58),
            S(-57,  64),   S(-53,  54),   S(-49,  55),   S(-47,  32),   S(-49,  38),   S(-46,  19),   S(-20,   5),   S(-51,  38),
            S(-46,  65),   S(-32,  54),   S(-42,  41),   S(-36,  36),   S(-32,  23),   S(-31,  20),   S(-15,   6),   S(-12,  27),
            S(-37,  76),   S(-30,  68),   S(-47,  64),   S(-60,  51),   S(-45,  45),   S(-24,  29),   S( -9,  26),   S(-21,  45),
            S(-13,  85),   S(-19,  81),   S(  5,  67),   S(-13,  41),   S(-29,  53),   S( 29,  22),   S(  8,  39),   S( -1,  64),
            S( 21,  84),   S( 21,  73),   S( 36,  63),   S(-11,  49),   S( 53,  14),   S( 43,  47),   S(112,  -3),   S( 58,  61),
            S( 50,  64),   S( -6,  77),   S( 17,  52),   S( 33,  21),   S(  3,   7),   S( 25,  70),   S(-40,  89),   S( 36,  68),
            S( 14,  41),   S( 21,  47),   S( 27,  31),   S(-27,  23),   S(-31,  10),   S( 17,  10),   S( 14,  22),   S( -5,  51),

            /* rooks: bucket 3 */
            S(-17,  72),   S(-12,  68),   S(-14,  91),   S(-12,  82),   S( -1,  46),   S(  2,  41),   S( 20,  12),   S( -9,   4),
            S( -1,  60),   S(-14,  73),   S(-15,  94),   S( -4,  85),   S( -3,  51),   S( 14,  13),   S( 47, -13),   S( 18,   6),
            S( 13,  56),   S( -7,  81),   S(-10,  80),   S( -7,  88),   S( 15,  39),   S(  6,  30),   S( 37,   8),   S( 33,   5),
            S(  2,  88),   S( -6, 107),   S(-17, 108),   S( -6,  96),   S( -3,  67),   S( 16,  49),   S( 36,  30),   S(  7,  27),
            S(  5, 105),   S( -9, 118),   S( 20, 111),   S( 19, 101),   S( 15,  85),   S( 45,  58),   S( 68,  32),   S( 42,  43),
            S(  7, 123),   S( 25, 107),   S( 35, 113),   S( 49,  96),   S(104,  43),   S(134,  24),   S( 91,  37),   S( 51,  38),
            S( 20, 110),   S( 15, 108),   S( 29, 116),   S( 25, 111),   S( 33,  92),   S(102,  43),   S(107,  93),   S(141,  64),
            S(115, -31),   S( 50,  41),   S( 13,  96),   S( 15,  80),   S( 18,  67),   S( 70,  55),   S( 39,  31),   S( 97,  10),

            /* rooks: bucket 4 */
            S(-22, -25),   S( 15, -18),   S(-22,  -7),   S(-41,  17),   S(-55,  17),   S(-39,  47),   S(-41,   5),   S(-85,  37),
            S(-26, -45),   S(-49,  -1),   S(-18, -18),   S(  1, -31),   S( 16, -15),   S( -9,   5),   S(-26,   0),   S( -1,  15),
            S(-14, -20),   S(-38, -23),   S(-38,  -7),   S(-10, -35),   S(-32,  -5),   S(-45,  18),   S(-22,  18),   S(-63,  22),
            S(-55, -34),   S( 10,   2),   S( 10, -21),   S( 13, -19),   S( 41,   2),   S( -7,  15),   S(-11,  -3),   S(-15,  12),
            S(-15, -33),   S( 27, -36),   S( 25,   3),   S( 51, -16),   S( 65,  -3),   S( 56,  26),   S( 17,  13),   S( 14,  26),
            S(-14, -37),   S( 10,  10),   S(  8,  -3),   S( 21,  10),   S( 30,  23),   S( 14,  14),   S( 33,  17),   S( 33,  38),
            S(-19, -21),   S( 34,  24),   S( 45,  -3),   S( 55, -10),   S( 63,  -3),   S(-11,  14),   S( 15, -12),   S( 22,   7),
            S( 17, -28),   S(  7,  14),   S( 33, -11),   S( 26, -11),   S( 53,   4),   S( 12,   5),   S(  5,  10),   S(  3,  14),

            /* rooks: bucket 5 */
            S(-25,  26),   S(-16,   6),   S(  1,  -6),   S( 24,  -5),   S(-11,  19),   S(-13,  30),   S(-32,  54),   S(-32,  36),
            S(-11,  -3),   S(-26, -14),   S( 42, -55),   S( 29, -22),   S(-20,   5),   S(-25,  13),   S(-41,  31),   S(-13,  30),
            S(-37,  23),   S( -5,  -5),   S(  5, -24),   S( -3, -16),   S(-17,  -5),   S( 31, -16),   S(-44,  33),   S(-25,  20),
            S(-24,  25),   S(  5,   4),   S( 53, -30),   S( 35,  -8),   S( 41, -10),   S( -9,  39),   S( 13,  31),   S(  6,  45),
            S( 39,  18),   S( 24,   6),   S( 17,  19),   S(  9,  -3),   S( -4,  20),   S( 72,   9),   S( 29,  34),   S( 49,  35),
            S(  0,  31),   S( -5,  12),   S(  5,   4),   S(-13, -13),   S( 21,  14),   S( 20,  26),   S( 58,  16),   S( 50,  33),
            S( 49,   3),   S( 48,  -9),   S(  0,  -1),   S( 37,   6),   S( 59,  -7),   S( 57, -13),   S( 85, -12),   S( 46,  14),
            S( 19,  30),   S( 15,   7),   S( 53,  -8),   S(  6,  14),   S( 42,  17),   S( 23,  30),   S( 32,  40),   S( 57,  40),

            /* rooks: bucket 6 */
            S(-49,  48),   S(-37,  38),   S(-31,  30),   S(-32,  24),   S( -1,   7),   S(  5,  -2),   S( 22,  -9),   S(-36,  20),
            S(-44,  29),   S(  8,   8),   S(-11,  12),   S( -6,   2),   S( 17, -18),   S(-25,  -3),   S(-30,   0),   S(-10,  12),
            S(-54,  38),   S( -4,  18),   S( -2,   7),   S(  1,   1),   S(-12,   5),   S( 36, -17),   S(  5, -18),   S( -8,  -3),
            S(-32,  52),   S( -5,  38),   S( 18,  14),   S( 61, -11),   S( 40, -14),   S( 22,   0),   S( 15,  -2),   S( 17,  28),
            S( -5,  51),   S( 58,  25),   S( 90,  17),   S( 71,  -7),   S( 30,  -8),   S( 34,  15),   S( 72,  -9),   S( 90,   2),
            S( 80,  12),   S( 84,  -1),   S( 85,   0),   S( 44, -15),   S(  4, -13),   S( 23,  30),   S( 34,  -4),   S( 59,  16),
            S( 60,  11),   S(130, -23),   S(107, -24),   S(100, -37),   S( 37, -15),   S( 53,  -6),   S( 69, -13),   S( 87, -23),
            S( 77,  -8),   S( 51,  16),   S(  5,  30),   S( 63,  -8),   S( 63,   0),   S( 33,  24),   S( 87,   4),   S( 57,  19),

            /* rooks: bucket 7 */
            S(-99,  35),   S(-80,  36),   S(-71,  37),   S(-62,  34),   S(-34,  -2),   S(-27, -18),   S(-34,   4),   S(-70, -15),
            S(-86,  34),   S(-34,  10),   S(-56,  21),   S(-65,  31),   S(-33, -14),   S(-18, -14),   S(  5,  -8),   S( -5, -58),
            S(-85,  36),   S(-68,  29),   S(-34,   7),   S(-43,  21),   S(-40,   0),   S(-24,   7),   S( 44, -35),   S(  1, -52),
            S(-74,  36),   S(-15,  15),   S( -3,  12),   S( 62, -23),   S( 16,  -8),   S( 72, -32),   S( 52, -11),   S( 28, -28),
            S(  1,  26),   S( 31,  21),   S( 60,   9),   S( 88, -13),   S(144, -51),   S(123, -56),   S( 88, -25),   S(-41, -36),
            S( 31,  15),   S( 30,   1),   S( 94,  -7),   S( 87, -23),   S( 78, -14),   S( 40,   7),   S( 23,  32),   S( -2, -28),
            S(  9,  -2),   S( 41, -15),   S( 76, -16),   S(112, -44),   S(123, -46),   S(117, -46),   S( 50,   5),   S( 22, -30),
            S(-22, -16),   S(  8,   5),   S( 41,  -3),   S( 31,   0),   S( 51, -17),   S( 72, -13),   S( 35,  10),   S( 29, -21),

            /* rooks: bucket 8 */
            S(-14, -81),   S(-14, -38),   S( -7, -14),   S( 18,   6),   S(-25, -28),   S(-22,   4),   S(-13, -30),   S(-20,   8),
            S(-32, -82),   S(-15, -44),   S(-21,   0),   S(-27, -66),   S(-26, -40),   S(-16, -21),   S(-10,  -6),   S(-38, -34),
            S(  1, -10),   S( -3, -14),   S( 12,  -5),   S(-11,  16),   S( -8,  49),   S( 12,  27),   S(  4,  50),   S(-18,   4),
            S( -5, -21),   S( -1,   5),   S( -1,  -2),   S( 15,  24),   S(  3,  41),   S( 31,  41),   S(  0,  19),   S( -9, -11),
            S( -9, -41),   S( 10,  20),   S(  9,  17),   S( 17,  37),   S(  7,  22),   S( -2,   4),   S( 12,  45),   S( -1,  21),
            S(-25,   9),   S(  3,  11),   S(-16,   7),   S( -6, -17),   S(  5,  35),   S(-16,  31),   S( -1,   3),   S(  2,  22),
            S(  3,  33),   S(  2,  25),   S(  3,   6),   S( 20,  12),   S( 14,  10),   S(  9,  31),   S(  5,  25),   S(  3,  46),
            S(-12,  14),   S(  3,  13),   S(-19,  30),   S( 34,  50),   S( -6,  23),   S( 11,  42),   S(  2,  25),   S(  7,  41),

            /* rooks: bucket 9 */
            S(-32, -68),   S(-11, -64),   S( -6, -99),   S(-11, -45),   S(-16, -49),   S(  0, -32),   S( -7, -20),   S( -5, -30),
            S(-59, -50),   S(-30, -72),   S(-27, -66),   S(-42, -50),   S(-35, -55),   S(-26,   3),   S(-22, -53),   S(-30, -31),
            S(-11, -15),   S(-21, -14),   S(  3,  -7),   S( -7, -33),   S( -6, -16),   S(  6,  18),   S(  2,   7),   S(  3,  17),
            S( -5,   2),   S(  4,  -3),   S(  3,   2),   S( -2,   5),   S(-11, -31),   S(  5,   1),   S( -7,  -1),   S(  4, -23),
            S( -4,  -1),   S( -8, -15),   S( -7, -48),   S( -8,   1),   S(-17, -19),   S(-10,   2),   S(-13, -14),   S( -7, -10),
            S( -9,   3),   S(-29, -18),   S(-12, -20),   S( -1,  15),   S( -4,  -4),   S( -8,   9),   S( -5,   0),   S(-13,   9),
            S(  4,  32),   S(  6,   3),   S(  6, -35),   S(  2,  13),   S(  7, -16),   S( 20,   5),   S(  4,  13),   S( -4, -10),
            S(-19,  17),   S(-18,  28),   S( -7,  15),   S( -6,  32),   S( -9,  30),   S(  4,  55),   S(  2,  22),   S( 11,  30),

            /* rooks: bucket 10 */
            S(-23, -27),   S(-56,  -8),   S(-31, -40),   S( -4, -52),   S(-13, -47),   S(  1, -79),   S(  5, -64),   S(-20, -41),
            S(-43, -13),   S(-31, -33),   S(-42, -26),   S(-37, -52),   S(-41, -46),   S(-22, -49),   S(-11, -36),   S(-46, -76),
            S( -8, -12),   S(-23, -15),   S(-29, -18),   S(-38, -44),   S( -8, -18),   S(  3, -16),   S( -8, -30),   S(-14, -15),
            S(-25,  -8),   S(-35, -33),   S( -4, -34),   S( -7,   2),   S(  5,   3),   S(  6,  10),   S( -9, -34),   S(  1, -35),
            S(  8,  -9),   S(  3,  -9),   S(-13, -16),   S( -9, -36),   S(  7,  10),   S( -3,  -1),   S( -5, -25),   S( -7, -35),
            S(-10,   3),   S( 11,   0),   S( -4, -17),   S(  0, -31),   S(  2, -10),   S( -6,  -8),   S(-18, -32),   S( -4, -17),
            S(-10,  -6),   S(  7, -25),   S( -1, -19),   S( -4, -14),   S( 13, -19),   S( -9,  -8),   S(-13, -31),   S( -9, -12),
            S( -7,   0),   S(  7,  31),   S(  0,  35),   S(-12,  13),   S( -8,  32),   S(-27,   5),   S(-30,  14),   S( -5,  14),

            /* rooks: bucket 11 */
            S(-60, -15),   S(-39,   0),   S(-52,  -7),   S(-28,  -4),   S(-46, -17),   S(-18, -19),   S(-17, -35),   S(-37, -64),
            S(-19, -13),   S(-23, -18),   S(-58,  -9),   S(-53, -19),   S(-15, -24),   S(-10, -12),   S(-25, -31),   S(-43, -62),
            S(-31,  25),   S(-23,  14),   S( -8,  32),   S(-20,  19),   S(  7, -22),   S( -6,  -4),   S(  7, -23),   S(-10,  13),
            S(-25,  -6),   S(-10, -16),   S(-12,  13),   S(  8,  16),   S( 21,  12),   S(-20, -35),   S(  6,  16),   S( -8, -21),
            S( -9,  -7),   S(  6,  -4),   S(  5,   6),   S(  4,   7),   S( 34,  -8),   S(  0,  -6),   S( 18,  33),   S(-15, -45),
            S(  4, -14),   S(-12,  -4),   S( 14, -10),   S( 19,  -4),   S(-12, -17),   S(  4,   3),   S(  6,  31),   S( -4, -11),
            S( -7,   9),   S(-22, -26),   S( -4,   1),   S( -2,   3),   S(  8,  -2),   S(  5,   6),   S(  2,  14),   S(-12,  -6),
            S( -8,   7),   S( 15,  36),   S(  3,  27),   S( 18,  25),   S(-12,   7),   S( -3,  24),   S( 13,  12),   S(-22,  24),

            /* rooks: bucket 12 */
            S(-33, -98),   S( -9, -14),   S(-19, -55),   S(-19, -36),   S(-12, -26),   S(  9,  -7),   S(-16, -39),   S(-18, -40),
            S(  3,   1),   S(  1,   4),   S(  9,  20),   S(  4,  13),   S(  8,   7),   S( 10,  -6),   S(  6,   9),   S(-18, -22),
            S( -4, -11),   S(  8,  35),   S( 11,  23),   S( 24,  23),   S(  6,  -7),   S( 16,  26),   S(  6,  34),   S( -3,  27),
            S(  6,  21),   S(  8,   2),   S( 14,  32),   S( 10,  20),   S( 12,   8),   S(  5,   8),   S(  6,  19),   S( -2,   6),
            S( 12,  17),   S( 14,  30),   S(  7,  46),   S(  2,   0),   S(  9,  27),   S( -2, -13),   S(  5,  16),   S(  6,  14),
            S( -2,   1),   S( -3,  -6),   S(  0,  17),   S( -5,   3),   S(  8,  25),   S(  0, -19),   S( 10,  26),   S(  5,  10),
            S(-16, -10),   S(-12,  19),   S(  6,  40),   S( -1,  21),   S( -2,   2),   S( 12,  18),   S(  3,  24),   S(  0,  23),
            S(  2,   5),   S( -8,  29),   S(  5,  31),   S( 13,  23),   S(  2,   6),   S(  1,  21),   S(  2,  11),   S(  2,  14),

            /* rooks: bucket 13 */
            S(-25, -24),   S(-24, -50),   S(-24, -50),   S(-16, -35),   S(-27, -52),   S( -3,  -4),   S(-25, -48),   S(-23, -36),
            S(-14, -10),   S( -8, -18),   S(  2,   6),   S( -2,  -2),   S( 18,  36),   S(  4,  12),   S(  8,   1),   S(-11, -12),
            S(-13,  -3),   S(-13,   2),   S( -4,  -8),   S(  8,  10),   S(  6,  26),   S( 14,  -1),   S( 12,  43),   S(-12, -25),
            S(  8,  15),   S( -2,   6),   S( -2,   9),   S(  5,  17),   S( 11,  21),   S(  0,   7),   S(  6,  14),   S(  1,  21),
            S(  7,  21),   S(  3,  -7),   S( -5, -22),   S(  3,   4),   S( -4,  24),   S(  1,  -2),   S(  5,   7),   S( -1,  -2),
            S(  1,  16),   S( -3,  -3),   S(-10,  -8),   S(-13,  -2),   S(-12, -14),   S(  3,  -2),   S( -7,   9),   S(  1,   5),
            S(  3,  -8),   S(  7,   6),   S( -9, -29),   S(  3,  16),   S( -9,  -3),   S(  7,  12),   S(  2,   5),   S(  0, -13),
            S(  2,  23),   S(-10,  14),   S( -4,   7),   S(  9,  25),   S( -3,  17),   S(  7,  25),   S(  0,  24),   S(  4,   5),

            /* rooks: bucket 14 */
            S( -5, -25),   S(-30, -30),   S(-17, -18),   S(-18, -53),   S(-11, -37),   S( -5, -21),   S(-31, -56),   S(-24, -37),
            S( -7,  26),   S(  4,  27),   S(  6,   9),   S(  0, -19),   S(  0,  -8),   S( -3,  -3),   S( -1,   6),   S( -4,  -4),
            S(  4,  31),   S( -2,  27),   S(  1,   3),   S(  3,   2),   S(  4,   8),   S(  0,  -5),   S(  2,  22),   S(-18, -48),
            S( -4,  14),   S( 15,  21),   S(  6,  16),   S(  9,   5),   S( -8,  -7),   S(  1, -11),   S(  9,  12),   S(-12, -17),
            S(  9,  18),   S( 19,  23),   S( -2,  -3),   S(  2,   7),   S(  3, -12),   S( 18,  31),   S(  0,   4),   S( -3, -14),
            S(  6,  15),   S(  7,  14),   S(  8,  18),   S(  2,   6),   S( -4,   7),   S(-15,   6),   S( -9,  -7),   S( -6,  -5),
            S( -6,  -9),   S(  9,  18),   S( -7, -17),   S(-18, -32),   S( -5,   6),   S(  0,   1),   S(-12, -13),   S( -8,  -6),
            S( -1,   1),   S(  4,   9),   S( -4, -11),   S(  6,  -7),   S(-11, -16),   S(-15, -41),   S(  2,  -4),   S(  1,  31),

            /* rooks: bucket 15 */
            S(-24, -44),   S(-17, -48),   S(-39, -48),   S(-24, -50),   S( -3, -22),   S(-13, -20),   S( -3, -10),   S(-20, -51),
            S(  7,  31),   S(-11,   3),   S(-11,  -7),   S( -6,  -9),   S( -6, -16),   S(  4,   0),   S(  7,  11),   S(  3,   5),
            S(  6,   8),   S( -6, -14),   S( 11,  24),   S(  8,  -1),   S(  6,  -2),   S( -4, -16),   S(  6,  24),   S(  3,   7),
            S(  2,  10),   S( -2,  -5),   S( 18,  33),   S( -3, -11),   S(  4,  17),   S(  2,   8),   S(  7,  15),   S(  3, -11),
            S(  7,  18),   S(  6,  10),   S(  6,  -8),   S(  3,  14),   S(  6,  15),   S(  3,   1),   S( -3,  28),   S(  5, -10),
            S(  7,  18),   S(  8,   3),   S(  9,   2),   S(  4,   6),   S( -5, -12),   S( -4,  38),   S(  1,  23),   S(  5,   3),
            S(  4,  -2),   S( -2,   7),   S(  9,  20),   S(  5,  14),   S(  2,  18),   S(  4,  16),   S(-13,  11),   S(-10, -30),
            S(  1,  25),   S(  0,  27),   S(  8,  24),   S(  1,  29),   S(  0,   3),   S( -6, -23),   S( -6,  14),   S(-15, -11),

            /* queens: bucket 0 */
            S( -3,  -6),   S(-25, -48),   S(-32, -58),   S(  0, -96),   S( -7, -54),   S(  9, -58),   S(-56, -27),   S(-15, -10),
            S(-14, -28),   S( 12, -75),   S(  3, -65),   S(-10, -18),   S(  2, -21),   S( -9, -36),   S(-26, -27),   S(-38,  -9),
            S( -3,  10),   S( -4, -21),   S( 27, -51),   S(-12,   8),   S( -9,  25),   S( -4,   2),   S(-35,   0),   S(-78, -41),
            S(-25,  28),   S( 16, -25),   S( -6,  18),   S(-13,  65),   S( -6,  63),   S(-25,  36),   S(-43,  25),   S(-17, -25),
            S(-19, -15),   S(  3,  67),   S(  6,  31),   S( -1,  40),   S(  3,  64),   S(-27, 107),   S(-58,  68),   S(-41,   2),
            S(-13,  13),   S( 19,  35),   S( 38,  37),   S(-23,  72),   S(-30,  66),   S(-64,  97),   S(-65,  25),   S(-41,   4),
            S(  0,   0),   S(  0,   0),   S( 19,   2),   S(-31,  30),   S(-36,  25),   S(-64,  79),   S(-89,  60),   S(-100,  24),
            S(  0,   0),   S(  0,   0),   S(  8,  -6),   S(-10, -13),   S(-31,  22),   S(-37,   4),   S(-52,  -5),   S(-64, -27),

            /* queens: bucket 1 */
            S( 16,  -4),   S(  6,   2),   S( 13, -48),   S( 31, -88),   S( 37, -45),   S( 11, -25),   S( 14,  -5),   S(  3,  17),
            S(-23,  33),   S( 21,  16),   S( 37, -39),   S( 27,   3),   S( 40,  10),   S(  1,  19),   S(-21,  34),   S(-19,   9),
            S( 45,  -2),   S( 22,   2),   S( 15,  32),   S( 16,  71),   S( -7,  80),   S( 31,  43),   S( -7,  35),   S( 15, -10),
            S( 36,   5),   S( 18,  40),   S( 18,  47),   S( 44,  67),   S( 17,  83),   S(  4,  58),   S(  8,  37),   S(-11,  54),
            S( 45,  -2),   S( 52,  15),   S( 51,  41),   S( 25,  35),   S( 46,  68),   S( 34,  25),   S(-10,  70),   S(  2,  90),
            S( 62,   0),   S(102,  11),   S( 83,  45),   S( 84,  56),   S( 56,  38),   S( 16,  62),   S( 39,  52),   S(  1,  51),
            S(100, -20),   S( 57, -18),   S(  0,   0),   S(  0,   0),   S(  7,  43),   S(-10,  23),   S( -8,  52),   S(-40,  36),
            S( 81,  -4),   S( 60,  -9),   S(  0,   0),   S(  0,   0),   S( 19,  23),   S( 42,  27),   S( 79,   0),   S(-18,  34),

            /* queens: bucket 2 */
            S( 33, -14),   S( 27,  10),   S( 30,  20),   S( 44, -23),   S( 43, -30),   S( 29, -22),   S( -1, -20),   S( 35,  30),
            S( 21,   4),   S(  6,  48),   S( 36,  20),   S( 42,  32),   S( 51,   7),   S( 20,  23),   S( 23,  18),   S( 15,  48),
            S( 34,  11),   S( 28,  35),   S( 18, 100),   S( 15,  82),   S( 24,  77),   S( 22,  68),   S( 31,  45),   S( 30,  60),
            S(  5,  59),   S( 21,  81),   S( 19,  82),   S( 12, 122),   S( 28,  97),   S( 22,  91),   S( 36,  60),   S( 33,  81),
            S(  1,  85),   S( -6,  72),   S(  2,  93),   S( 32,  77),   S( 25, 100),   S( 90,  44),   S( 73,  58),   S( 67,  55),
            S(-16,  88),   S( -7,  79),   S(  2,  76),   S( 78,  35),   S( 40,  57),   S( 98,  73),   S(119,  38),   S( 44, 108),
            S( -3,  54),   S(-12,  48),   S( -8,  71),   S( 50,  28),   S(  0,   0),   S(  0,   0),   S( 22,  79),   S( 47,  72),
            S(  0,  36),   S( 36,  -4),   S( 49, -11),   S( 32,  35),   S(  0,   0),   S(  0,   0),   S( 53,  43),   S( 16,  65),

            /* queens: bucket 3 */
            S(-44,  37),   S(-31,  43),   S(-24,  41),   S(-13,  50),   S(-27,  34),   S(-16, -14),   S(-17, -36),   S(-41,  25),
            S(-59,  57),   S(-38,  46),   S(-25,  64),   S(-17,  82),   S(-15,  72),   S(-16,  36),   S( 15, -14),   S( 15, -26),
            S(-53,  77),   S(-39,  86),   S(-34, 113),   S(-41, 143),   S(-30, 122),   S(-24,  93),   S(-11,  53),   S(-12,  21),
            S(-42,  77),   S(-61, 135),   S(-52, 159),   S(-36, 171),   S(-39, 160),   S(-18,  96),   S( -3,  75),   S(-15,  67),
            S(-54, 120),   S(-47, 154),   S(-55, 175),   S(-45, 186),   S(-25, 152),   S( -1, 129),   S(-12, 122),   S(-18,  82),
            S(-59, 110),   S(-60, 156),   S(-63, 179),   S(-60, 190),   S(-51, 164),   S( 19,  92),   S(-21, 122),   S(-23, 121),
            S(-95, 123),   S(-94, 142),   S(-79, 181),   S(-71, 158),   S(-75, 163),   S(-16,  83),   S(  0,   0),   S(  0,   0),
            S(-125, 139),  S(-82, 101),   S(-70, 102),   S(-65, 109),   S(-51, 100),   S(-12,  57),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-34,  -3),   S(-50, -35),   S( -9,  -1),   S(-10, -19),   S( -6,  -6),   S( -9,  11),   S(-33, -26),   S( 13,  21),
            S( -1, -11),   S( -8,   6),   S( -3,   1),   S(-18, -14),   S(-44,  19),   S(-17,  12),   S(-46, -10),   S(  1, -15),
            S( 10,  20),   S( 22, -29),   S( 15, -17),   S( 17,   9),   S( 40,  11),   S( 13,  22),   S(-22, -19),   S( 33,  23),
            S(-10, -20),   S( 21, -19),   S(  8,   2),   S( -9,  19),   S( 44,  29),   S(  0,  58),   S(-25,   6),   S(-11,  19),
            S(  0,   0),   S(  0,   0),   S( 17,  -7),   S( 56,  36),   S( 25,  56),   S( 31,  51),   S( 10,  16),   S( 12,  21),
            S(  0,   0),   S(  0,   0),   S( 18,  10),   S( 35,  19),   S( 41,  49),   S( 30,  49),   S( 18,  24),   S(  0,   6),
            S( 16,  -3),   S( 20,   9),   S( 64,  38),   S( 61,  37),   S( 57,  13),   S( 18,  27),   S(  5,  22),   S(-12,  21),
            S( 28,  -7),   S(-17, -31),   S( 25,   7),   S( 46,  18),   S( 16,   5),   S(  9,  21),   S( -1,   2),   S( 20,   6),

            /* queens: bucket 5 */
            S( 35,  21),   S( 23,   7),   S( 14,   6),   S(-12,  25),   S( 33,  -6),   S( 39,  46),   S( 11,  -1),   S( 20,   2),
            S( 18,  15),   S( 15,  -2),   S( 14,  -2),   S( 10,  13),   S( 10,  42),   S(-13, -11),   S( 27,  15),   S( 14,   5),
            S( 21,   3),   S( 47,  -4),   S( 23,   1),   S(  9,  17),   S( 20,   7),   S( 33,  17),   S( 25,  40),   S( 13,  13),
            S( 10, -31),   S( 38,   4),   S( 26, -15),   S( 33,  16),   S( 63,   9),   S( 34,  14),   S( 36,  47),   S(  4,  30),
            S( 40,  -6),   S( 28, -41),   S(  0,   0),   S(  0,   0),   S( 12,  11),   S( 32,  15),   S( 39,  52),   S( 16,  33),
            S( 38,  16),   S( 36,   7),   S(  0,   0),   S(  0,   0),   S( 30,  20),   S( 64,  35),   S( 44,  37),   S( 52,  40),
            S( 75,   6),   S( 73,  11),   S( 51,  40),   S( 24,  25),   S( 53,  21),   S( 95,  44),   S( 66,  55),   S( 50,  29),
            S( 43,  30),   S( 55,  13),   S( 66,  20),   S( 44,  -2),   S( 55,  18),   S( 63,  37),   S( 69,  47),   S( 60,  30),

            /* queens: bucket 6 */
            S( 49,  50),   S(  1,   3),   S( 33,  14),   S( 34,  20),   S( 24,  14),   S( -7,   0),   S(  0,  12),   S(  8,  20),
            S( 26,  18),   S( 27,  32),   S( 56,  41),   S( 52,  28),   S( 39,  24),   S( 17,  13),   S(-12,  26),   S( 27,  32),
            S(-13,  45),   S( 35,  34),   S( 28,  37),   S( 50,  15),   S( 35,  14),   S( 46,   0),   S( 63,  27),   S( 69,  60),
            S( 25,  35),   S(  6,  26),   S( 50,  11),   S( 96,  19),   S( 46,  -7),   S( 48,  10),   S( 85,   9),   S(101,  45),
            S( 30,  52),   S( 33,  36),   S( 55,  38),   S( 52,  32),   S(  0,   0),   S(  0,   0),   S( 67,  22),   S(115,  56),
            S( 42,  49),   S( 57,  48),   S( 49,  56),   S( 28,   9),   S(  0,   0),   S(  0,   0),   S( 82,  49),   S(118,  47),
            S( 61,  36),   S( 27,  24),   S( 75,  17),   S( 61,  19),   S( 43,  38),   S( 70,  47),   S(132,  26),   S(144,  11),
            S( 38,  38),   S( 66,  24),   S( 75,  15),   S( 83,  34),   S(104,  14),   S(100,  13),   S(113,  13),   S(101,  28),

            /* queens: bucket 7 */
            S( -8,  23),   S( -8,  -1),   S(-24,  21),   S( -9,  23),   S( 11,   3),   S(-14,   4),   S( -5,  16),   S(-13,  -6),
            S(-11,  25),   S(-50,  27),   S(-10,  54),   S(-16,  79),   S(-14,  43),   S(  5,  24),   S(  6,   4),   S(-32,  -1),
            S(  1,  24),   S(-20,  37),   S(-24,  91),   S( 31,  51),   S( 45,  30),   S( 28,   8),   S( 52, -27),   S( 54,  -2),
            S(-18,  22),   S( 14,  43),   S( 14,  69),   S( 41,  71),   S( 76,  45),   S( 74,  -4),   S( 84, -34),   S( 51,  -5),
            S( 13,  24),   S(-14,  59),   S( 15, 101),   S( 50,  80),   S( 88,  18),   S( 71,  -3),   S(  0,   0),   S(  0,   0),
            S(  1,  46),   S(-10,  86),   S(  8,  90),   S(  1,  85),   S( 63,  35),   S( 98,  52),   S(  0,   0),   S(  0,   0),
            S(-37,  58),   S(-22,  39),   S( 11,  55),   S( 36,  56),   S( 68,  36),   S( 85,  15),   S( 71,  24),   S( 68,  33),
            S( 33,  16),   S( 47,  29),   S( 52,  52),   S( 52,  19),   S( 55,  36),   S( 32,   2),   S( -8,   7),   S( 75, -10),

            /* queens: bucket 8 */
            S(-18, -36),   S(  0, -23),   S(-16, -42),   S( -3,  -9),   S(-16, -29),   S(  8,  -4),   S( -1, -12),   S(  1,   5),
            S(-20, -31),   S( -5, -15),   S(  3, -15),   S( -6, -11),   S(  9,  -4),   S( -4, -11),   S( -3,   3),   S( -1,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -17),   S( -9, -45),   S(  5,   3),   S(  8,  -5),   S( -8,  -9),   S(  3,   6),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S( -1, -12),   S( -1,   3),   S(  3,   0),   S( 12,  20),   S(  6,   3),
            S( -2, -12),   S(  7,   9),   S(  7,   2),   S( 12,  -7),   S(  7, -10),   S( 11,  12),   S( 14,  12),   S(-10,  -9),
            S(  2, -15),   S(  4, -17),   S( 14,  13),   S(  3, -20),   S( 12,   7),   S( 26,  32),   S(  8,  -5),   S( -2,  -4),
            S(-17, -38),   S(  1, -11),   S( 11,   8),   S( 25,  37),   S( 12,  11),   S( 17,  38),   S(  4,   6),   S(  5,   1),
            S(  1,   0),   S(  3,  -7),   S( 13,   7),   S(  8,  -4),   S( 16,  18),   S( -4,  -5),   S(  3,  10),   S(-17, -28),

            /* queens: bucket 9 */
            S(  9, -10),   S(-18, -34),   S(-13, -33),   S( 13,  -8),   S( -6, -35),   S( -1,  -9),   S( -5,  -9),   S( -2, -14),
            S( -1,  -7),   S(-10, -20),   S(-10, -27),   S(  2, -15),   S(-21, -50),   S(-12, -29),   S(  6,  -3),   S(  1, -10),
            S(-17, -44),   S(-13, -27),   S(  0,   0),   S(  0,   0),   S(  5,  -8),   S( 11,  -8),   S( -4,  -8),   S(  6,  -4),
            S(  2,  -6),   S(-11, -30),   S(  0,   0),   S(  0,   0),   S(  1,  -3),   S( 11,   2),   S( 11,  11),   S( -2,   2),
            S( -8, -28),   S(  1, -14),   S(  0,  -7),   S(-10, -10),   S( -5, -28),   S( 12,  17),   S(  5,  -8),   S(  0, -16),
            S( 11,  10),   S( -2, -29),   S(  5, -10),   S( -4, -19),   S(  0, -10),   S(  5,   5),   S( -3, -12),   S( -2, -13),
            S(  8,   5),   S(  7,  -5),   S( -5,  -4),   S(  0,   9),   S( 22,  23),   S( 25,  29),   S(  7,  19),   S(  7, -11),
            S( 15, -12),   S( 24,  15),   S( -2,  -8),   S( 19,  12),   S( 20,  16),   S(  5,  13),   S(  1, -18),   S( 13,   3),

            /* queens: bucket 10 */
            S( 15,  11),   S( 12,   8),   S(  0,  -9),   S( -6, -27),   S( -9, -29),   S( -9, -17),   S( -4, -26),   S( -4, -15),
            S(  6,   3),   S(-14, -21),   S( -5, -23),   S(-18, -52),   S( -3,  -9),   S( 11,   0),   S(-10, -27),   S( -6,  -6),
            S( -2,   1),   S(  3,   4),   S( -1,  -3),   S( -7, -18),   S(  0,   0),   S(  0,   0),   S(  3,  -3),   S(-11, -21),
            S( -3,  -9),   S(  3,   4),   S(  4,   3),   S(  9,   2),   S(  0,   0),   S(  0,   0),   S( -5, -14),   S(  1, -17),
            S( 11,  15),   S( 15,   4),   S(  3,  -5),   S( 31,  33),   S(  0,   2),   S( -1,  -1),   S(  2, -11),   S( 11, -25),
            S( -6, -10),   S(  6,   6),   S( 22,  26),   S( 11,  12),   S( 14,  14),   S( 15,  22),   S( 16,   8),   S( -3, -23),
            S(  8,   6),   S( 19,  27),   S( 19,  26),   S( 20,  15),   S(  9,  15),   S( 24,  13),   S( 14,   8),   S(  5,  -6),
            S(-11, -30),   S(  3,   6),   S( 20,   4),   S( -6,  -2),   S( 14,  14),   S(  2,   2),   S( 13,   7),   S(  8, -11),

            /* queens: bucket 11 */
            S(-11,  -4),   S( -4,  -2),   S( -8,  -9),   S(-19, -19),   S( -5, -14),   S(-19, -33),   S( -7, -31),   S( -7, -15),
            S( -6,   0),   S(  1,   8),   S(-24, -11),   S( -7,   2),   S( 21,   1),   S( -9, -25),   S(  8,  -1),   S( -5, -12),
            S(  3,   7),   S(  6,   2),   S(-19,  13),   S( -2,   3),   S( -2, -20),   S(-22, -29),   S(  0,   0),   S(  0,   0),
            S(  0,   2),   S( -7,  10),   S( -2,  11),   S( -1,   4),   S(  1,  -9),   S( -3,   6),   S(  0,   0),   S(  0,   0),
            S(  1,  12),   S( 14,  15),   S( 16,  24),   S(  4,  23),   S( 42,  46),   S( 17,  27),   S(  8,  -1),   S(-10, -28),
            S(  1,   4),   S(  1,  -2),   S( -2,  12),   S( 12,  28),   S( 15,  19),   S(  1,   2),   S(  5, -11),   S(  6, -21),
            S(  3,   3),   S(  9,  12),   S( 16,  23),   S(  2,  13),   S( 18,  56),   S( 15,  11),   S(  4,   1),   S( 10,  -4),
            S(-17, -57),   S(  9,  12),   S( -7,  -7),   S(  3,  36),   S( 14,  29),   S( 11,  -1),   S( -6,  -3),   S( 11,  -1),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  2,   2),   S(-14, -19),   S( -6,  -6),   S(-12, -21),   S( -2,  -4),   S( -3,  -3),
            S(  0,   0),   S(  0,   0),   S(  6,   3),   S( -9, -18),   S( -7,  -8),   S(-10, -21),   S( -9, -17),   S(  1,  -1),
            S( -6,  -9),   S(  5,   7),   S( -5,  -7),   S(-11, -35),   S( 16,  31),   S(  0,  14),   S( -2,  -7),   S(  8,   9),
            S( -8, -20),   S(  5,   2),   S(  8,  14),   S(  3,  12),   S(  1,   3),   S( -2,  10),   S( -3,  -2),   S( -3,  -8),
            S(-17, -29),   S(  3,   9),   S(  5,   2),   S(  6,   6),   S(  7,  30),   S( -5, -19),   S( -8, -16),   S( -1,   0),
            S(  1,  -7),   S( -4, -11),   S(  0, -13),   S(  4,  10),   S( -5,  -8),   S( -9,   0),   S(-11, -10),   S( -2,  -8),
            S( -9, -13),   S(  3,   5),   S( -6, -11),   S( 13,  11),   S( -1,   0),   S( -9, -15),   S(  0,  -1),   S( -7, -26),
            S(  6,  12),   S(  0,  -3),   S(  1,  -6),   S(  0,   3),   S( -6,  -7),   S(-14, -13),   S( -6,   9),   S( -9, -15),

            /* queens: bucket 13 */
            S(-22, -34),   S(-15, -28),   S(  0,   0),   S(  0,   0),   S(-17, -29),   S(-12, -34),   S(  0,  -1),   S( -4, -10),
            S(-16, -45),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(-16, -35),   S(-22, -44),   S(-12, -21),   S( -4,  -6),
            S(-21, -38),   S( -4, -14),   S( -4,  -5),   S( -2, -13),   S(-21, -40),   S(-10, -15),   S( -8,  -6),   S( -1,  -4),
            S( -7, -19),   S(-19, -31),   S(  0, -10),   S( -5, -17),   S( 11,   5),   S( 18,  32),   S( -4, -14),   S( -8, -10),
            S(  4, -10),   S(  0, -24),   S( -7, -20),   S( 11,  24),   S( -7, -10),   S( -1, -16),   S( -3,  -5),   S(  2, -10),
            S( -2,  -4),   S(-14, -19),   S(  4,   2),   S( 10,  22),   S(  1, -10),   S( -5,  -6),   S(-12, -22),   S(-10, -23),
            S(  0,   0),   S( -4, -10),   S( 11,  24),   S( -2,  -2),   S(  3,   2),   S(  8,   0),   S(-14, -25),   S( -7, -11),
            S( -8,  -6),   S( -3,  -8),   S( -6, -11),   S(  0,  -8),   S(  3,  -2),   S( -1,  -3),   S( -1,  -8),   S(-12, -20),

            /* queens: bucket 14 */
            S( -6, -15),   S(  0, -10),   S( -9, -19),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S( -3,  -7),   S( -7, -22),
            S( -7, -23),   S(-26, -47),   S(-12, -25),   S( -3, -15),   S(  0,   0),   S(  0,   0),   S( -9, -24),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -6, -20),   S(-13, -25),   S( -3,  -4),   S(  2,   3),   S(-10, -16),   S(-17, -32),
            S( -9, -11),   S( -2,  -1),   S(  0,  -1),   S(-15, -21),   S( -7, -15),   S(-19, -28),   S( -3, -22),   S(  1,   1),
            S( -6, -12),   S( -5, -12),   S( -4, -16),   S(  5,   9),   S(  6,  18),   S(-10, -25),   S( -9,  -6),   S( -2,  -3),
            S( -6, -13),   S(  3,  -4),   S(-13, -20),   S(-13, -23),   S(  6,  10),   S(  1,   4),   S( -2,  -6),   S(-10, -11),
            S(-10, -17),   S( -2,  -9),   S(  0,   0),   S(  3,   6),   S(  2,   4),   S(  3,   5),   S( -8, -21),   S( -3,  -9),
            S(-11, -17),   S(  5,  -5),   S(-10, -14),   S( -4,  -9),   S(  3,   1),   S( -3,  -3),   S( -4,  -2),   S(  2,  -7),

            /* queens: bucket 15 */
            S(  1,   3),   S( -7, -18),   S(  4,   0),   S(-11, -18),   S(  4,   6),   S(-10, -10),   S(  0,   0),   S(  0,   0),
            S( -5,  -4),   S(  1,   6),   S(-13, -16),   S( -8, -17),   S(  0,  -6),   S(  2,   7),   S(  0,   0),   S(  0,   0),
            S( -1,  -1),   S(  1,   0),   S(-12,  -3),   S( -6,  -6),   S( -9, -24),   S(  4,   4),   S( -1,   2),   S( -1,  -4),
            S( -2,  -5),   S(-10, -15),   S( -3,  -5),   S(  0,   6),   S(  9,  27),   S(  7,  28),   S( -3,   5),   S( -4, -16),
            S(  1,   3),   S(  1,   1),   S( -4,  -9),   S(  0,  -1),   S( 11,  52),   S(  3,  19),   S(  3,  12),   S( -6, -16),
            S( -1,  -3),   S( -3,  -2),   S( -4,  -8),   S( -6,  -1),   S( -2,   5),   S( -9,  -8),   S(  1,  12),   S( -8,  -6),
            S( -5, -13),   S(  0,  -1),   S( -5,   4),   S(  3,   3),   S( -7,  -9),   S(  1,   6),   S(  5,  12),   S( -5, -10),
            S( -8, -18),   S(-14, -31),   S( -2, -10),   S(  2,   3),   S(-14,  -3),   S( -3,  -1),   S(  1,  -1),   S( -3,   4),

            /* kings: bucket 0 */
            S(-11, -21),   S( 30,  -9),   S( 16,  -4),   S(-27,  14),   S( -8,  13),   S( 30, -25),   S(  4,   1),   S( 10, -49),
            S(-18,  32),   S( -2,   0),   S( -2,   5),   S(-45,  25),   S(-42,  42),   S(-16,  22),   S(-14,  36),   S( -5,  28),
            S( 12,   3),   S( 64, -28),   S(  1,  -3),   S(-22,   5),   S(-32,   4),   S( -1,  -5),   S(-31,  17),   S( 29, -29),
            S(-25, -27),   S(  7, -30),   S(  7, -28),   S(-24,   7),   S(-48,  32),   S(-49,  28),   S(-41,  38),   S(-16,  31),
            S(-48, -124),  S( -3, -48),   S( -1, -36),   S( 14, -22),   S(-47,  -6),   S(-30,  10),   S(-22,  13),   S(  2,  -9),
            S(-10, -122),  S(  1,   8),   S(-10, -56),   S(-12,  -6),   S( -2, -12),   S(-25,  21),   S( 15,  23),   S(-19,   7),
            S(  0,   0),   S(  0,   0),   S(  0, -51),   S(  5, -36),   S(-19,  -6),   S(-11, -14),   S(-28,   6),   S( -9,  -4),
            S(  0,   0),   S(  0,   0),   S(-13, -11),   S(  2, -10),   S(  9,  -2),   S( -5,  13),   S(  7,   3),   S(  9,   0),

            /* kings: bucket 1 */
            S(  7, -27),   S( 32, -22),   S( 15, -15),   S( 28,  -3),   S(  0,  -2),   S( 32, -19),   S(  6,   3),   S( 19, -24),
            S( 10,  -1),   S(  5,  10),   S( -2,  -7),   S(-45,  27),   S(-31,  21),   S(-14,  16),   S( -6,  17),   S(  3,   9),
            S( -8, -16),   S(  0, -14),   S(  5, -17),   S( 13, -18),   S(-33,  -1),   S( 16, -17),   S( 23, -11),   S( 38, -12),
            S( -1,  -1),   S(  1, -11),   S(  4,  -4),   S( -4,   5),   S(  8,   9),   S(-12,   3),   S( 30,  -7),   S(-19,  26),
            S(-18, -55),   S(-15, -46),   S( -7, -54),   S(-15, -42),   S( -2, -25),   S( -3, -29),   S( -8,  -4),   S( -7,  -4),
            S(-30,  -1),   S(-102,   4),  S(-31,  27),   S(  3,  20),   S(-42,   5),   S(-24,  15),   S( 16,   3),   S( -6,  -8),
            S(-33, -54),   S(-23,   5),   S(  0,   0),   S(  0,   0),   S(-42,  13),   S(-52,  29),   S( -5,  27),   S( -3, -33),
            S(-29, -111),  S(-13, -17),   S(  0,   0),   S(  0,   0),   S( -8,   8),   S(-13,  15),   S( -2,  20),   S( -4, -46),

            /* kings: bucket 2 */
            S( 14, -58),   S(  9,  -3),   S( 16, -20),   S( 17, -10),   S(  0,   6),   S( 35, -23),   S( -2,  14),   S( 20, -28),
            S( 33, -36),   S(-17,  31),   S(-16,   8),   S(-17,   8),   S(-24,  14),   S(-13,   6),   S(  3,   1),   S(  1,   0),
            S(-31,  -4),   S(-18, -13),   S( -8, -11),   S( -9, -18),   S( -8,  -5),   S(  5, -19),   S( 28, -18),   S( 26, -17),
            S( 15,  11),   S(-19,  16),   S(  2,   3),   S(-26,  12),   S( 28,  -6),   S(-15,  -9),   S( 32, -29),   S( 31, -10),
            S( -3, -11),   S( 14, -16),   S( 23, -37),   S(  7, -31),   S( 31, -49),   S(-20, -43),   S( 23, -50),   S( 10, -47),
            S(  3,   8),   S( -9,  -7),   S(-37,   1),   S(-37, -13),   S(  3,   0),   S(-10,  24),   S(-82,   9),   S(-16, -21),
            S( -8, -11),   S( -8,  21),   S(-74,  14),   S(-17,   8),   S(  0,   0),   S(  0,   0),   S(-13,  16),   S(-36, -36),
            S( -7, -38),   S(-19, -28),   S(-30, -32),   S( -7,   7),   S(  0,   0),   S(  0,   0),   S(-10, -14),   S(-32, -122),

            /* kings: bucket 3 */
            S( -5, -53),   S( 17, -10),   S( 28, -24),   S( -5,  -7),   S( -1, -13),   S( 35, -24),   S(  2,  13),   S(  7, -28),
            S(  2,  17),   S(-21,  39),   S(-19,   6),   S(-37,  18),   S(-53,  31),   S( -1,   0),   S( -7,  19),   S(  1,  13),
            S( 17, -27),   S(  1,  -3),   S( -3,  -8),   S(-34,   0),   S(-13,   9),   S( 22, -19),   S( 49, -19),   S( 55, -17),
            S(-19,  30),   S(-93,  46),   S(-57,  19),   S(-50,  15),   S(-36,  13),   S(-15, -21),   S(-37,  -4),   S(-33, -15),
            S(-13,   8),   S(-12,  -4),   S(-35, -10),   S(-24, -16),   S( 29, -44),   S( 54, -67),   S( 36, -71),   S( 13, -83),
            S(-11, -13),   S( 17,   7),   S( 19,  -9),   S(  0, -23),   S( 46, -32),   S( 58, -50),   S( 73, -23),   S( 53, -118),
            S(-20,  -9),   S( 25,   9),   S( 14, -12),   S( 30, -24),   S( 31, -29),   S( 27, -56),   S(  0,   0),   S(  0,   0),
            S( -3, -11),   S(  5,   9),   S( -2,  18),   S( 13, -11),   S( 11, -72),   S( -3,  10),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-56,   6),   S(  7,  35),   S( 10,  21),   S( 11,   1),   S( -8,   7),   S( 10, -11),   S(  5,   7),   S( 22, -36),
            S(-37,  23),   S( 23,  19),   S( -6,  18),   S( -7,   3),   S( 30,  -2),   S( 23,  -4),   S( 53, -15),   S( 13,  -3),
            S( -1,  26),   S( 11, -13),   S( 19,  -4),   S( -9,   3),   S(-20,   9),   S( 21, -21),   S(-37,   7),   S( 14, -12),
            S(  0, -21),   S(-11,   8),   S(  5,  16),   S(  6,   5),   S(-20,  11),   S(-13,  17),   S( 16,   9),   S( 10,   7),
            S(  0,   0),   S(  0,   0),   S( -1,   1),   S(-29,  13),   S(-36,  14),   S(-27, -15),   S(-20,   1),   S( -3,  -2),
            S(  0,   0),   S(  0,   0),   S(  6, -15),   S( -3,  24),   S(-12,  27),   S(-28, -11),   S(  4, -15),   S( -1,  16),
            S( -3, -20),   S( -4,  -6),   S( -4, -23),   S(  0,  21),   S( -5,  24),   S(-28, -10),   S(-11,  20),   S(  4,  -3),
            S( -5, -23),   S(  3, -13),   S(-10, -20),   S( -8,   3),   S(  6,  11),   S( -7, -11),   S( -5,   1),   S(  5,  11),

            /* kings: bucket 5 */
            S( 33,  -4),   S(-10,  12),   S(-34,  22),   S(-43,  28),   S(-20,  26),   S(  1,  11),   S( 37,  -4),   S( 31, -10),
            S( -1,   0),   S( 16,  10),   S( 27,  -4),   S( 24,  -6),   S( 16,  -4),   S( 40, -13),   S( 29,   4),   S( 47, -17),
            S(-12,   8),   S( -6,  -7),   S(-13,  -5),   S( -3,  -7),   S(  7,  -2),   S(-38,   0),   S( -3,   2),   S( 18,  -4),
            S( -2, -12),   S(  0,  -8),   S(  9,  -6),   S(  7,  17),   S(  3,  21),   S(  9,   2),   S( 15,   5),   S(  8,   4),
            S( -4, -29),   S(-31, -46),   S(  0,   0),   S(  0,   0),   S( -8,  -4),   S(-20, -14),   S(  3, -14),   S( -9,   5),
            S( -6, -41),   S(-26, -29),   S(  0,   0),   S(  0,   0),   S(-22,  37),   S(-55,  11),   S(-18,  -4),   S( -6,  -5),
            S(-16, -34),   S(-31,  20),   S(  1,  10),   S( -1, -18),   S(-28,  28),   S(-40,  19),   S( -1,   9),   S( 10,  17),
            S(-10, -101),  S( -8,  10),   S(-10, -27),   S( -2, -35),   S(-10, -18),   S( -6,   7),   S( -3, -17),   S(  0,   7),

            /* kings: bucket 6 */
            S( 38, -36),   S( 32, -15),   S( -2,   1),   S(-21,  22),   S( -9,  20),   S(-20,  20),   S(  1,  20),   S(  9,   1),
            S( 48, -27),   S( 12,  17),   S( 15,  -7),   S( 25,  -8),   S( 22,  -4),   S( -7,  11),   S( 15,   1),   S(  6,   2),
            S( 17, -19),   S(-24,   3),   S(-16,  -8),   S( -2,  -6),   S( 13, -12),   S(-43,   5),   S( 10,  -2),   S(-16,  13),
            S( 12,   6),   S( 27,  -5),   S( 15, -12),   S( 24,   5),   S( 56,   1),   S(-25,   4),   S( -6,   7),   S(  7,  -1),
            S(  8, -19),   S( 16, -29),   S(-23, -11),   S( -1, -17),   S(  0,   0),   S(  0,   0),   S(-46, -20),   S(-40, -18),
            S(-16,   0),   S(  3,  -1),   S(-30,  -2),   S( -9, -23),   S(  0,   0),   S(  0,   0),   S(-27, -15),   S(-29, -23),
            S(  0,  -9),   S( -9,   6),   S(-39,  10),   S(-16,  -3),   S(  3,   4),   S(-10, -30),   S(-28, -14),   S( -8, -38),
            S( -1,  -6),   S(  2,  -8),   S( -3,  10),   S(-15, -29),   S( -8, -37),   S( -5, -26),   S( -6,  -3),   S( -1, -59),

            /* kings: bucket 7 */
            S( 31, -33),   S( -5,  -4),   S(-27,  -3),   S(-14,  10),   S(-28,  12),   S(-41,  35),   S(-27,  33),   S(-37,  21),
            S( 13,  -2),   S( 23, -21),   S( -2,  -8),   S(-32,   7),   S(-13,   7),   S(-36,  22),   S(  3,  -3),   S( -4,  14),
            S( 29, -29),   S(-16,  -8),   S(-30,  -2),   S(-33,   1),   S(-45,   8),   S(-30,  13),   S( 13,  -3),   S(-46,  21),
            S(-23,  18),   S(  7,   9),   S( -6,   0),   S( 34,  -5),   S( 32,  -9),   S( 49, -27),   S( 16, -10),   S( 20, -11),
            S(-15,  15),   S( -5,   1),   S(  2, -24),   S(  6, -16),   S( 14, -24),   S(  9, -21),   S(  0,   0),   S(  0,   0),
            S(-10, -31),   S(  0,  -8),   S( 15, -10),   S( 12,  -6),   S( 25, -10),   S( 17, -13),   S(  0,   0),   S(  0,   0),
            S( 14,  18),   S( -2, -19),   S(  2,   5),   S(-12, -12),   S(  7, -18),   S( -5, -29),   S(  5, -16),   S(-11,   9),
            S(  7,   7),   S( -7, -10),   S( 11,  20),   S( -3,  -5),   S(  8,  16),   S(-17, -50),   S(  9, -13),   S(-11, -59),

            /* kings: bucket 8 */
            S( 15, 119),   S( -7,  87),   S( 39,  41),   S( -3,  -2),   S(-14,  13),   S(-15,  -5),   S( 32, -16),   S(-14, -19),
            S( 30,  70),   S( 22,  16),   S( 45,  63),   S( 82,  -3),   S( 17,  26),   S(  7,  -7),   S( -3,  10),   S(  3,  26),
            S(  0,   0),   S(  0,   0),   S( 28,  66),   S( 39,   5),   S( 18,   7),   S( -8,  -7),   S( -2,  14),   S(  9, -16),
            S(  0,   0),   S(  0,   0),   S(  4,  76),   S( -6,   1),   S(-16,  36),   S( -6,  18),   S( 14,  10),   S(  9,  15),
            S( -3, -26),   S( -1,  27),   S(  3,  13),   S(-15,  25),   S(-18,  -3),   S(  4, -16),   S(  0,  11),   S(-14, -26),
            S(  5,  13),   S( -1, -14),   S( -4, -12),   S( -7,   2),   S(-13,   1),   S(-11,  -2),   S( -9,  -2),   S(  8,  -7),
            S( -5, -14),   S( -8, -13),   S(  4,   9),   S( -1, -10),   S( -2, -32),   S(-11,   6),   S( -3,   1),   S(  5, -46),
            S( -6,  -9),   S(-12, -25),   S( -2, -11),   S( -6, -21),   S(  7,   8),   S( -5,   2),   S(  1,  -3),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  7,  26),   S(-14,  35),   S(-20,  58),   S( 17,   9),   S(-18,  34),   S(-26,  28),   S( 41,   3),   S( 21,  13),
            S(-19,  34),   S( 34,  24),   S(  4,   1),   S( 48,   3),   S( 57,  18),   S( 25,   6),   S( -4,  28),   S(-16,  13),
            S( -7,  13),   S( 23,  13),   S(  0,   0),   S(  0,   0),   S( 45,  17),   S( -1,   3),   S(  8,   0),   S(-19,  22),
            S( -2, -31),   S( 12, -22),   S(  0,   0),   S(  0,   0),   S(  7,  35),   S( 14,   0),   S(-11,  10),   S(-15,  29),
            S(  5, -21),   S( 12,  -3),   S(  4,  17),   S(  0,  12),   S(-14,  18),   S(-21,  14),   S( -9,  13),   S(  0, -15),
            S(  5,   2),   S(  1,  -6),   S(  6,  -9),   S(-11, -20),   S(-12,  11),   S(  0,   8),   S(-33,   2),   S(  5,  31),
            S(  2,  -6),   S( -3, -21),   S( -2,  -8),   S(  2, -30),   S( 14, -27),   S( 14,  16),   S(-17,  -8),   S(  4,   4),
            S(  7,   6),   S( -1, -22),   S( 10, -23),   S( -4, -20),   S( -1, -19),   S(  3,   7),   S( -6,  13),   S(  9,  -1),

            /* kings: bucket 10 */
            S( 34,  -2),   S(  2,  -8),   S(  5,   8),   S(  7,  24),   S(-15,  22),   S(-92,  50),   S(-31,  46),   S(-85,  82),
            S(  5,  -2),   S( 63,  -1),   S( 25,  -6),   S( 32,  11),   S( 56,  13),   S( 49,   4),   S( 12,  27),   S(-86,  49),
            S( 16,   7),   S( 28,   0),   S( 27, -11),   S( 14,  13),   S(  0,   0),   S(  0,   0),   S( -7,  22),   S(-59,  28),
            S( 15,   6),   S( 43, -27),   S( 36, -33),   S( 30,   4),   S(  0,   0),   S(  0,   0),   S(  0,  14),   S(  5,   4),
            S(  4,   7),   S( 27,   5),   S( 30, -21),   S(  9, -30),   S(  4, -17),   S(  7,  25),   S(  9,   9),   S( -9,  16),
            S(  3,  14),   S(  3,  -7),   S( -3,   5),   S( 10,  -5),   S(  7,  -2),   S(-17,  -6),   S(-13,   6),   S(  0,  -8),
            S(  0, -42),   S( -3, -15),   S(  9, -10),   S( 14,   2),   S( 12,  -1),   S(-10, -18),   S(  5, -28),   S(  5,   4),
            S(  4,   6),   S( 11,  -9),   S( -1, -17),   S(  1,   2),   S(  5, -15),   S(  0, -30),   S( -5,  -7),   S(  9,   3),

            /* kings: bucket 11 */
            S( -6, -20),   S(  9,   8),   S(  7, -10),   S( -6,  16),   S( -9,   8),   S(-68,  58),   S(-75,  83),   S(-125, 152),
            S( -2, -27),   S( 23,   3),   S(-10, -17),   S( 15,  22),   S( 81,   2),   S( 57,  42),   S(  8,  23),   S( 25,  40),
            S(  3, -50),   S( -2,  19),   S( -1, -10),   S( 23,  12),   S( 61,   1),   S( 26,  62),   S(  0,   0),   S(  0,   0),
            S(  0,  20),   S( 18,  13),   S( -3,   3),   S( 10,  15),   S( 31, -10),   S( 23,  23),   S(  0,   0),   S(  0,   0),
            S(  1,  33),   S(  2,  -5),   S(  9,  -8),   S( 13, -16),   S( 16,   2),   S( -1,  -1),   S(  9,  11),   S(  6,   1),
            S( 11,  10),   S(  1, -15),   S( 16, -12),   S( -1,   5),   S( -5,  -7),   S(  3, -17),   S( -4,  -8),   S(-11,  -4),
            S(  6,  12),   S(  8,  -7),   S( 18,  22),   S(  1, -25),   S( 17, -18),   S(  4,   3),   S(-11, -13),   S( -8, -14),
            S(  5,   8),   S(  6,   1),   S(-11, -22),   S(  5,  -6),   S( -4, -21),   S( -8, -18),   S(  0, -19),   S(  5,  11),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 18,  58),   S(  6,  -6),   S(  1,  -3),   S(  7,  14),   S(  7,  -1),   S(-20,   6),
            S(  0,   0),   S(  0,   0),   S( 45, 111),   S( 27,  15),   S( 22,  44),   S( 13,  -2),   S( 23,  -6),   S(-18,   1),
            S( -1,   8),   S(  3,  13),   S( 23,  72),   S( 39,  20),   S(  8,  -7),   S( 11,   2),   S(  1, -11),   S(-10,  -2),
            S( -2,   9),   S(  9,  32),   S( -1,  18),   S(  5,  -4),   S( -8,   2),   S( -2,  19),   S( -4,  11),   S(  1,   8),
            S( 10,  17),   S(  6,  23),   S( 10,  20),   S( -2,  40),   S( -3,  37),   S(  0,   3),   S( -9,  14),   S(-12, -11),
            S(  6,   5),   S( 10,  17),   S( -2,  -1),   S(-10, -15),   S( -1,   5),   S( -8,  16),   S( -9, -15),   S(  6,  -2),
            S(  3,   8),   S( -6, -13),   S( -1,   6),   S( -1,   1),   S( -5,  -9),   S(  4,   7),   S(  8,  43),   S(  0, -29),
            S( -2,   2),   S(  5,   3),   S( -4,   7),   S(  0,   1),   S( -1,  -5),   S(  3,   8),   S(-11, -23),   S( -2, -22),

            /* kings: bucket 13 */
            S( -1,  53),   S(  6,  32),   S(  0,   0),   S(  0,   0),   S( 43,  16),   S( 14, -12),   S( -2,  -6),   S(-17,  24),
            S(  3,  22),   S( -2,   1),   S(  0,   0),   S(  0,   0),   S( 47,   6),   S( 27,  -7),   S(-20,   7),   S(-15,   5),
            S( -3,   3),   S( 19,  22),   S(  2,  -7),   S( 14,  40),   S( 50,  13),   S( 22,  -6),   S(  2,   7),   S( 12, -10),
            S(-10,  -6),   S( 15,  -2),   S(  1,  21),   S( -5,  15),   S( -3,  15),   S(  4, -11),   S(  4,  22),   S(-16, -26),
            S(  6,  12),   S( -1,   6),   S(  5,  42),   S( -4,  24),   S( -9,  11),   S(  5,  19),   S(-11,   1),   S(  7,  10),
            S(  4,  -1),   S( -5,  17),   S( -2,  17),   S( -5,  -1),   S(-13, -15),   S( -6,   8),   S( -9,  20),   S(  1,   0),
            S(  9,  11),   S( -8, -22),   S(-11, -44),   S(  3,  20),   S(-11, -10),   S(-10,  15),   S(-14, -24),   S(  6,  14),
            S(  1,  -2),   S(  5,  -2),   S(  4,  20),   S(  3,   5),   S(  0,  17),   S(-10, -16),   S( -3,   8),   S(  8,  14),

            /* kings: bucket 14 */
            S( 19,  34),   S( -1,  -7),   S( 11, -43),   S( 15,   1),   S(  0,   0),   S(  0,   0),   S(  6,  72),   S(-40,  39),
            S(-10, -10),   S( 18,  -7),   S( 47, -34),   S( 41,  11),   S(  0,   0),   S(  0,   0),   S( 13,  34),   S(-45,   7),
            S(  4,   4),   S( 15,  -6),   S( 33, -31),   S( 40,   6),   S( 10,  -2),   S( 14,  36),   S( 27,  57),   S(-29,   3),
            S(  7,  -4),   S(  8,  -8),   S( -1, -11),   S( 12,   0),   S(-19,  -1),   S( 16,  54),   S(  3,  24),   S(  6,  -3),
            S(  7,  19),   S(  9,   0),   S( -9,   3),   S(-18,  12),   S(  1,  29),   S(  5,  54),   S(  2,  38),   S(  5,  12),
            S( -5,  -6),   S(  1,   5),   S( -2,  -3),   S( -1,  10),   S( -6, -20),   S( -6,  -2),   S(-15,  -5),   S( -1,   8),
            S(  4,  10),   S(-10, -13),   S( 11,  -7),   S( 16,   5),   S(  3,  -4),   S( -7,  18),   S(-26, -22),   S(  8,  17),
            S(  1,  12),   S(  5,  -9),   S(  9,   2),   S( -4,  -6),   S(  7,  -9),   S( -3,  -6),   S(-13, -25),   S(  0, -11),

            /* kings: bucket 15 */
            S( 11,  31),   S(  6,  -2),   S( 11,  -7),   S( -8,  -1),   S(-10, -11),   S( -1,  59),   S(  0,   0),   S(  0,   0),
            S( -3, -22),   S(  7, -12),   S( -8, -16),   S( 19,  51),   S( 39,   0),   S( 61, 111),   S(  0,   0),   S(  0,   0),
            S(-10, -23),   S( 17,  -9),   S(  7, -16),   S( -3,  15),   S(  9,  -5),   S( 25,  73),   S(  8,  44),   S(-14,  -1),
            S( -1, -12),   S(  3,  14),   S(  3,  13),   S(-12, -28),   S(-13,  -2),   S( 21,  48),   S( 17,  48),   S( -3, -14),
            S( 10,   6),   S( -8,  24),   S(  0,  -5),   S( -6, -35),   S( -3,   8),   S(  1,  36),   S(  4,   7),   S(  3,   3),
            S(  5,  27),   S(-15,  -4),   S(  8,  15),   S(  8,  19),   S(-10, -23),   S( -2,   8),   S(  1,   6),   S(  5,  19),
            S(  8,  11),   S( -4,  25),   S( -2, -13),   S(  3,   7),   S(  8,   8),   S(  8,  16),   S( -5,  -2),   S(  2,   2),
            S( -2,  -7),   S(  4,   1),   S( -2, -11),   S(  4,   5),   S(  5,   5),   S( 10,  14),   S(  1,  -7),   S(  3,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-28,  65),   S(-36, -60),   S( -4,  28),   S( 11,  88),   S( 22, 114),   S( 27, 149),   S( 38, 151),   S( 49, 157),
            S( 66, 139),

            /* bishop mobility */
            S(-61,  90),   S(-38, -44),   S(  1,   8),   S(  8,  76),   S( 21, 106),   S( 30, 128),   S( 33, 147),   S( 40, 153),
            S( 42, 159),   S( 52, 156),   S( 59, 153),   S( 83, 132),   S(100, 129),   S( 70, 126),

            /* rook mobility */
            S(-63,   7),   S(-145, 118),  S(-18,  34),   S(-11, 106),   S(-10, 137),   S( -9, 160),   S( -7, 177),   S(  0, 183),
            S(  8, 185),   S( 14, 195),   S( 19, 200),   S( 28, 201),   S( 42, 202),   S( 54, 199),   S( 97, 172),

            /* queen mobility */
            S(113, 165),   S( 36, 309),   S( 26, 261),   S( 10, 212),   S( 49,  67),   S( 57,  46),   S( 58, 146),   S( 56, 211),
            S( 54, 263),   S( 54, 290),   S( 55, 317),   S( 58, 332),   S( 60, 351),   S( 63, 356),   S( 64, 367),   S( 64, 372),
            S( 66, 374),   S( 65, 377),   S( 70, 369),   S( 77, 358),   S( 88, 343),   S(121, 304),   S(132, 288),   S(153, 256),
            S(186, 229),   S(193, 205),   S(139, 201),   S(114, 144),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,  13),   S(-19,  46),   S(-29,  43),   S(-38,  58),   S(  9,  10),   S( -9,  12),   S(  1,  56),   S( 25,  27),
            S( 15,  33),   S( -2,  45),   S(-17,  45),   S(-21,  38),   S( -4,  34),   S(-25,  39),   S(-28,  54),   S( 32,  26),
            S( 22,  68),   S( 12,  72),   S(  6,  53),   S( 21,  45),   S( -3,  50),   S(-25,  63),   S(-33,  95),   S( -6,  76),
            S( 31, 106),   S( 41, 119),   S( 25,  77),   S( 12,  58),   S(  8,  62),   S(  2,  85),   S(-43, 122),   S(-73, 148),
            S( 25, 151),   S( 54, 183),   S( 63, 129),   S( 35, 112),   S(-52, 105),   S( 24, 106),   S(-54, 171),   S(-85, 170),
            S( 94, 232),   S( 80, 269),   S(129, 240),   S(129, 250),   S(135, 262),   S(154, 241),   S(128, 253),   S(135, 264),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  2,   4),   S( -6, -27),   S( -2,  -9),   S( -3,   5),   S( 13,  15),   S(-14, -40),   S(-22,   9),   S( -6, -47),
            S(-17,  18),   S( 20, -18),   S(  0,  27),   S(  9,  26),   S( 29,  -6),   S( -4,  14),   S( 24, -16),   S( -5,  -7),
            S(-15,  17),   S( 17,   5),   S(  2,  44),   S( 16,  54),   S( 23,  30),   S( 33,  18),   S( 27,   2),   S(  1,  13),
            S( 15,  36),   S( 14,  52),   S( 42,  92),   S( 13, 101),   S( 68,  68),   S( 70,  56),   S( 18,  60),   S( 24,  23),
            S( 49,  96),   S( 88, 117),   S(102, 140),   S(139, 164),   S(136, 135),   S(135, 148),   S(130, 124),   S( 51,  61),
            S( 72, 192),   S(117, 277),   S(102, 222),   S( 96, 197),   S( 66, 152),   S( 48, 140),   S( 41, 142),   S( 16,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  21),   S( 17,  21),   S( 32,  34),   S( 32,  24),   S( 20,  21),   S( 27,  22),   S(  6,  11),   S( 42,  -3),
            S( -4,  21),   S( 17,  35),   S( 11,  35),   S(  9,  41),   S( 24,  12),   S(  9,  22),   S( 33,  18),   S(  0,  12),
            S(  0,  22),   S( 29,  49),   S( 54,  57),   S( 39,  60),   S( 44,  56),   S( 71,  19),   S( 30,  36),   S( 20,   6),
            S( 57,  73),   S(103,  58),   S(123, 124),   S(146, 129),   S(138, 120),   S( 79, 132),   S( 72,  59),   S( 74,  10),
            S( 44, 124),   S( 90, 143),   S(153, 211),   S(105, 251),   S(133, 264),   S( 83, 240),   S(160, 208),   S(-54, 172),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26,  32),   S( 10,  18),   S( 10,  33),   S(-13,  62),   S( 67,  22),   S( 21,   9),   S( -1,   1),   S( 30,  12),
            S(  2,  14),   S(  7,   8),   S( 17,  18),   S( 14,  30),   S(  9,  18),   S(  0,   9),   S(  7,   6),   S( 28,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -2, -14),   S( -7,  -8),   S(-17, -18),   S(-14, -30),   S( -9, -18),   S(  0,  -9),   S( -7,  -6),   S(-28,   4),
            S(-26, -32),   S(-10, -18),   S(-10, -33),   S( 13, -62),   S(-67, -22),   S(-21,  -9),   S(  1,  -1),   S(-30, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -39),   S(-14, -42),   S(-16, -48),   S(-62, -36),   S(-27, -46),   S(-29, -48),   S( -8, -49),   S(-24, -62),
            S(-26, -23),   S(-20, -31),   S(-33, -14),   S( -9, -35),   S(-41, -36),   S(-24, -29),   S(-38, -21),   S(-13, -44),
            S(-20, -20),   S( -8, -36),   S(-27, -12),   S(-32, -25),   S(-20, -44),   S(-24, -22),   S( -9, -24),   S(-40, -32),
            S( -8, -33),   S( 18, -47),   S( 13, -20),   S(  9, -31),   S( 12, -32),   S( 58, -46),   S( 42, -47),   S(-11, -56),
            S( 12, -50),   S( 39, -75),   S( 46, -30),   S( 60, -34),   S( 77, -53),   S( 83, -40),   S(137, -96),   S( 34, -84),
            S( 95, -101),  S(126, -110),  S( 90, -51),   S( 72, -33),   S( 67, -34),   S(122, -48),   S(103, -50),   S( 46, -89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-13, -24),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  4,   1),        // attacks to squares 1 from king
            S(  9,   3),        // attacks to squares 2 from king

            /* castling available */
            S( 67, -64),        // king-side castling available
            S( 17,  59),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 30, -88),   S( 35, -74),   S( 27, -85),   S( 26, -74),   S( 17, -65),   S( 14, -59),   S(  3, -50),   S( -5, -44),
            S(  4, -43),   S( 16, -41),   S( 44, -44),   S( 42, -40),   S( 92, -47),

            /* orthogonal lines */
            S(-64, -137),  S(-89, -120),  S(-109, -98),  S(-123, -93),  S(-130, -91),  S(-137, -92),  S(-136, -98),  S(-132, -104),
            S(-146, -93),  S(-158, -90),  S(-164, -96),  S(-137, -120), S(-91, -141),  S(-50, -145),

            /* pawnless flank */
            S( 41, -36),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 28, 230),

            /* passed pawn can advance */
            S(-10,  34),   S( -3,  61),   S( 15, 104),   S( 84, 170),

            /* blocked passed pawn */
            S(  0,   0),   S( 52, -27),   S( 30,  -5),   S( 34,  31),   S( 33,  61),   S( 23,  39),   S( 69,  81),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 49, -51),   S( 39,  16),   S( 22,  27),   S( 21,  58),   S( 32,  97),   S(136, 129),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-13, -16),   S( -5, -32),   S(  7, -26),   S(-17,  -3),   S(-21,  30),   S(127,  18),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 29, -16),   S( 30, -20),   S( 13,  -7),   S( 13, -40),   S( -5, -108),  S(-28, -193),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 23,  53),   S( 43,  29),   S( 99,  46),   S( 32,  27),   S(175, 115),   S(109, 129),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 16,  57),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-44, 115),

            /* bad bishop pawn */
            S( -8, -17),

            /* rook on open file */
            S( 30,   6),

            /* rook on half-open file */
            S(  8,  44),

            /* pawn shields minor piece */
            S( 12,  14),

            /* bishop on long diagonal */
            S( 28,  52),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 21,  32),   S( 23,   2),   S( 34,  21),   S( 27,  -2),   S( 35, -21),

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
