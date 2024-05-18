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

        // Solution sample size: 16000000, generated on Sat, 18 May 2024 01:25:48 GMT
        // Solution K: 0.003850, error: 0.082075, accuracy: 0.5149
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 80, 223),   S(385, 668),   S(407, 664),   S(544, 1077),  S(1389, 1801), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(104, -126),  S(148, -95),   S( 44, -45),   S(-24,  27),   S(-27,  14),   S(-23,   2),   S(-50,   6),   S(-28, -15),
            S(125, -130),  S(108, -110),  S( 11, -65),   S(-10, -53),   S(-19, -15),   S(-20, -25),   S(-36, -21),   S(-21, -40),
            S(112, -106),  S( 63, -64),   S( 13, -65),   S( 11, -68),   S( -9, -59),   S(  3, -54),   S(-13, -49),   S(  5, -52),
            S( 71, -42),   S( 51, -59),   S( 27, -62),   S( 17, -81),   S(-16, -40),   S(-15, -51),   S(-20, -39),   S( -6, -23),
            S( 77,  35),   S( 33, -10),   S( 40, -29),   S( 52, -70),   S( 19, -40),   S( -9, -37),   S(-25,  -3),   S(-31,  55),
            S( 64,  56),   S( 49,  76),   S(  4,   8),   S( 17, -18),   S(-42,   0),   S(  4,   6),   S( -4,  22),   S( 13,  50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 33, -30),   S( 33, -38),   S( 54, -25),   S(  3,  21),   S( -4,  -9),   S(  5, -11),   S(-41,   4),   S(-27,  22),
            S( 37, -44),   S( 26, -45),   S( 15, -48),   S(  0, -43),   S(-10, -22),   S( -6, -28),   S(-33, -13),   S(-34,  -9),
            S( 30, -41),   S( 10, -30),   S( 16, -55),   S( 12, -55),   S(-22, -25),   S( 12, -47),   S(-10, -31),   S(  2, -23),
            S( 44, -23),   S( 19, -51),   S( 26, -57),   S(  6, -50),   S(-12, -21),   S( 13, -44),   S(-24, -23),   S( -7,   5),
            S( 28,  46),   S(-33,   3),   S( -3, -36),   S( 12, -48),   S( 36, -35),   S( -8,  -6),   S(-28,  24),   S(-23,  73),
            S( 55,  58),   S( 14,   2),   S(-46, -19),   S(-23,  24),   S(-21,  -7),   S(-60,  27),   S(-46,  31),   S(-39,  89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  -1),   S(-19,   3),   S( -6,  -1),   S(  0,   5),   S( 16, -14),   S( 35, -19),   S( 12, -44),   S(  3, -18),
            S(  0, -27),   S(-25, -13),   S(-18, -33),   S(-13, -33),   S( 10, -33),   S( 13, -33),   S( -1, -39),   S(-12, -28),
            S( -8, -24),   S(-21, -28),   S( -9, -53),   S(  0, -54),   S( -3, -32),   S( 24, -45),   S(  4, -40),   S( 13, -31),
            S(-12,  -8),   S( -9, -48),   S(-12, -52),   S( -4, -55),   S(  9, -46),   S(  4, -30),   S(  2, -23),   S(  6,  -8),
            S( -3,  38),   S(-42,  -5),   S(-39, -40),   S(-44, -32),   S( 13,  -7),   S(-11,   5),   S(-22,  23),   S(-19,  77),
            S(-52,  81),   S(-90,  58),   S(-94,  -2),   S(-70, -17),   S(-40,   6),   S(-21,  21),   S( -7,  -4),   S(-18,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -17),   S(-25,  -3),   S(-22,  -3),   S( 14, -48),   S( -1,  -4),   S( 53, -26),   S( 91, -73),   S( 71, -86),
            S( -5, -43),   S(-24, -29),   S(-18, -42),   S(-15, -28),   S( -5, -29),   S( 20, -43),   S( 64, -77),   S( 64, -78),
            S( -5, -48),   S( -6, -58),   S( -4, -65),   S(  2, -67),   S(  0, -55),   S( 26, -58),   S( 39, -68),   S( 80, -76),
            S( -2, -33),   S(  3, -74),   S(  2, -77),   S(  4, -73),   S( 23, -75),   S( 23, -65),   S( 32, -52),   S( 70, -34),
            S( 26,   9),   S( -7, -33),   S( 11, -73),   S( 15, -67),   S( 86, -65),   S( 72, -41),   S( 56,   7),   S( 56,  62),
            S(-30, 104),   S(-22,  14),   S( -3, -47),   S( -6, -65),   S( 63, -75),   S( 61, -21),   S( 62,   2),   S( 67,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-91,  22),   S(-11, -13),   S(-33,  11),   S(-10,  23),   S( -9, -19),   S(-46,  26),   S(-45,   1),   S(-43,   7),
            S(-17,   5),   S( 42, -22),   S( 28, -39),   S( 14, -27),   S( -7, -20),   S(-48, -17),   S(  2, -41),   S(  6, -27),
            S( 39, -20),   S( 39, -17),   S(-17,   6),   S(  2, -30),   S(-36, -29),   S(-17, -33),   S(-24, -40),   S( 22, -37),
            S( 11,  24),   S(-12,  32),   S( 36,  -1),   S(  1,  -2),   S( 15, -38),   S(-37, -25),   S(  6, -42),   S( 52, -31),
            S(-18,  88),   S(-23,  85),   S(-16,  24),   S(-17,   3),   S(  1,  19),   S(-19,   4),   S(-31, -31),   S( 38,  22),
            S( 65,  75),   S( 52,  99),   S(  8,  35),   S( 18,  19),   S( 11, -16),   S(  1, -10),   S(  7,   1),   S(-14,  15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-96,  55),   S(-77,  44),   S( -8,  11),   S( -6,  14),   S(-18,  30),   S(-28,  19),   S(-47,  16),   S(-24,  30),
            S(-51,  17),   S(-60,  20),   S( 32, -17),   S( 20,   1),   S( 17, -10),   S(-14, -18),   S(-27,  -7),   S(-28,  12),
            S(-49,  35),   S(-62,  29),   S( 50, -29),   S(  6, -25),   S( 27, -16),   S(-18, -21),   S(-14,  -8),   S( 14,  -8),
            S(-57,  54),   S(-55,  33),   S(  4,  -1),   S( 26,   4),   S(-14,   3),   S(-50,  -4),   S(  0, -11),   S( 11,  15),
            S( 29,  59),   S( 32,  35),   S( 31,  39),   S( 29,  20),   S( -9,  32),   S( 63,  -8),   S(  9,  10),   S( 47,  29),
            S( 60,  44),   S( 57,  17),   S( 39,  -6),   S( 37,  -4),   S( 45, -15),   S( 21,  -4),   S(  9,   8),   S(  6,  54),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52,  31),   S(-39,  20),   S(-31,  16),   S(-27,  16),   S( 32, -22),   S(-30,  10),   S(-64,   7),   S(-57,  21),
            S(-39,   3),   S(-13, -17),   S(-15, -32),   S(  0,  -9),   S( 35, -19),   S( 24, -26),   S(-37,  -7),   S(-65,   9),
            S(-23,  -3),   S(-22,  -7),   S(-18, -22),   S(-32,  -5),   S( 13, -12),   S( 63, -42),   S( -8, -17),   S(-16,   5),
            S(-33,  19),   S(-77,  11),   S(  5, -31),   S(-14,  -9),   S( 17,  -4),   S( 39, -19),   S( 18, -10),   S( 38,   3),
            S(  8,  24),   S(-50,  15),   S( 13, -29),   S( -2, -13),   S( 49,  22),   S( 74,  17),   S( 37,   8),   S( 70,  27),
            S( 59,  27),   S( 21,   1),   S(  4, -37),   S(  8, -38),   S( 21,  -1),   S( 25,   2),   S( 40, -11),   S( 42,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -21),   S(-46, -14),   S(-27,  -4),   S(-48,  12),   S(-13, -22),   S( 26, -23),   S( -4, -49),   S(-42, -25),
            S(-37, -40),   S(-35, -41),   S(-43, -36),   S(-19, -44),   S( -7, -37),   S( 52, -58),   S( 52, -62),   S( -7, -36),
            S(-40, -42),   S(-56, -37),   S(-43, -45),   S(-18, -43),   S( -9, -27),   S( 36, -41),   S( 46, -60),   S( 54, -48),
            S(-17, -44),   S(-49, -51),   S(-77, -43),   S(-48, -25),   S( -4, -29),   S( 25, -23),   S( 28, -20),   S( 77, -30),
            S(  9, -34),   S(  7, -59),   S(-21, -51),   S(  1, -64),   S( 23,  -5),   S( 33,  -3),   S( 69,  39),   S(105,  31),
            S(-14,   4),   S(-29, -32),   S(  5, -51),   S( -3, -52),   S( -2, -16),   S( 28, -23),   S( 48,  37),   S( 88,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-59,  73),   S(-42,  61),   S( 14,  25),   S(-10,  -1),   S( 12,   8),   S(  0,   8),   S(-39,   6),   S(-44,  28),
            S(-63,  63),   S(-63,  57),   S(-33,  42),   S(-17,  12),   S(-12,  -8),   S(-37, -13),   S(-51,  -6),   S(  1,  -4),
            S(-65, 100),   S(-11, 100),   S(-11,  62),   S(-29,  35),   S( 12, -12),   S(-98,  -4),   S(-71, -17),   S(-41,  -3),
            S(-30, 139),   S(  9, 149),   S( 15, 106),   S( 12,  49),   S(-30,  14),   S(-31, -20),   S(-27,  -4),   S(-50,  10),
            S(-12, 169),   S( 44, 155),   S( 29, 161),   S( 56, 100),   S( 20,  12),   S(  2,   3),   S(-17, -15),   S( -6,  19),
            S( 53, 192),   S( 70, 209),   S( 87, 200),   S( 49,  74),   S(  7,  35),   S(-11,   3),   S(-10, -25),   S(  2,  14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-99,  78),   S(-71,  53),   S(  8,  10),   S( 12,  30),   S( 10,   8),   S(-43,  16),   S(-77,  20),   S(-75,  34),
            S(-61,  40),   S(-59,  36),   S(-46,  31),   S(  2,  45),   S(-52,   3),   S(-27, -10),   S(-75,  -2),   S(-31,  13),
            S(-93,  70),   S(-119, 100),  S(-48,  79),   S(-105,  90),  S(-63,  54),   S(-87,   9),   S(-48, -17),   S(-46,   7),
            S(-73, 109),   S(-38, 117),   S(  7, 120),   S( 46, 124),   S(-25,  58),   S(-39,  13),   S(  9,   3),   S(-47,  23),
            S( 14, 123),   S( 23, 143),   S( 24, 155),   S( 47, 171),   S( 23, 128),   S( -5,  35),   S( -1,   1),   S( -1,   3),
            S( 23,  71),   S( 21, 123),   S( 65, 139),   S( 71, 179),   S( 29, 108),   S( -8,  -7),   S(-14,  -8),   S(-19, -17),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-91,  16),   S(-64,   1),   S( -9,  -1),   S(  3,  20),   S(-12,   2),   S(-61,  29),   S(-108,  29),  S(-60,  35),
            S(-99,   9),   S(-83,   8),   S(-15, -16),   S(-25,  -8),   S(-21,  23),   S(-40,  25),   S(-125,  35),  S(-85,  21),
            S(-28, -10),   S(-87,  16),   S(-30,   3),   S(-86,  70),   S(-79,  84),   S(-15,  41),   S(-121,  50),  S(-88,  45),
            S(-100,  34),  S(-78,  28),   S(-10,  10),   S(-39,  78),   S( 22,  95),   S(-47,  78),   S(-32,  51),   S(  1,  27),
            S(-27,  46),   S(-35,  21),   S(  7,  50),   S( 28, 124),   S(105, 108),   S( 53,  64),   S( -8,  87),   S( 30,  45),
            S( -1,  16),   S(-21,  -2),   S( 19,  18),   S( 48, 116),   S( 11, 129),   S( 27,  55),   S( -6,  72),   S( 24,  97),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-72,  -3),   S(-76,  20),   S( 41, -16),   S( -2,  18),   S(  1,  36),   S(-85,  55),   S(-54,  38),   S(-67,  45),
            S(-69, -19),   S(-81, -18),   S(-32, -37),   S(-50,  17),   S(-37,  13),   S(-30,  28),   S(-100,  63),  S(-99,  48),
            S(-37, -31),   S(-60, -32),   S(-54,  -4),   S(-33,  10),   S(-53,  36),   S(-15,  58),   S(-81,  85),   S(-47,  68),
            S(-54,   7),   S(-90, -12),   S(-29, -27),   S(-53,  18),   S(  7,  43),   S( -5,  73),   S( 21, 111),   S( 76,  73),
            S(-22,  25),   S(-46,  -6),   S( -7,  -1),   S( -8,  24),   S( 60,  94),   S( -6, 125),   S(100, 118),   S( 91, 103),
            S(-33,  47),   S(-19,   5),   S( 10, -18),   S(  2,   3),   S( 21,  70),   S( 32, 152),   S( 66, 180),   S( 34, 174),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-14,  10),   S(-18,   9),   S(-19,   1),   S(  2,   6),   S( -3, -10),   S( -9,  14),   S(-14, -19),   S(-17,  -3),
            S(-38, -25),   S( -7,  19),   S(  9,  20),   S( -1,   4),   S(  0,  33),   S( -6, -11),   S(-36, -31),   S(-27, -41),
            S(-17,  38),   S(-38,  95),   S( 20,  65),   S( 19,  38),   S(-15,   2),   S(-46, -14),   S(-45, -48),   S(-44, -58),
            S(-43,  90),   S(-47, 121),   S( 40, 113),   S( 23,  96),   S(-19, -29),   S(-40, -34),   S( -9, -14),   S(-60, -50),
            S( 34,  96),   S( 41, 211),   S( 49, 151),   S( 19,  57),   S(  0,  16),   S( -2, -21),   S( -1,   2),   S(-19, -46),
            S( 46, 111),   S( 56, 218),   S(118, 222),   S( 47,  98),   S( -6,   5),   S( -9,  -7),   S(-11, -28),   S(-22, -39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -19),   S(-20,  13),   S( -6,  11),   S( -2,   4),   S( -9, -10),   S(-29,   4),   S(-36, -41),   S(-23,  -5),
            S(-39, -10),   S(-57,  48),   S(-24,  34),   S( 20,  22),   S(-44,  25),   S(-14, -14),   S(-81, -24),   S(-61,  10),
            S(-59,  48),   S(-51,  49),   S(-38,  77),   S(-11,  95),   S(  2,  34),   S(-40, -30),   S(-64, -27),   S(-78, -25),
            S(-77,  92),   S( -9, 120),   S( -4, 139),   S(  7, 123),   S(  0,  65),   S(-44,  25),   S(-17, -12),   S(-38, -39),
            S(  2,  99),   S( 55, 171),   S( 68, 196),   S( 49, 248),   S( 24, 150),   S(-10,  15),   S( -3, -63),   S(-25, -36),
            S( 40,  71),   S( 74, 172),   S( 84, 194),   S( 85, 252),   S( 40, 110),   S(  3,  10),   S(  0,   2),   S( -5,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -56),   S(-37, -22),   S( -8, -28),   S( -3,  -1),   S( -5,  -2),   S(-31,   8),   S(-35,  -2),   S( -4,  47),
            S(-52,  13),   S(-55,   9),   S(-54, -29),   S(  0,  11),   S(-39,  64),   S(-16,  17),   S(-42,  19),   S(-54,  13),
            S(-61, -23),   S(-61,   8),   S(-35, -19),   S(-22,  41),   S(-20,  71),   S(-53,  36),   S(-35,   5),   S(-64,  44),
            S(-50,  14),   S(-24,  54),   S(-27,  29),   S(  9,  98),   S( -4, 132),   S(-27,  83),   S(-37,  41),   S(-36,  60),
            S(-21, -21),   S( 11,  18),   S( 14,  78),   S( 36, 135),   S( 48, 215),   S( 44, 171),   S( 10,  85),   S( 27,  42),
            S( -2,  25),   S( 19,  37),   S( 30, 115),   S( 36, 140),   S( 65, 214),   S( 58, 115),   S( 29,  96),   S( 21,  32),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -31),   S(-31, -20),   S(-10, -32),   S(  0,  -3),   S( 17,  21),   S(  1,  47),   S(-10, -24),   S(  9,  22),
            S(-42, -31),   S(-32, -13),   S(-14, -40),   S( 24,  -6),   S(-14,   1),   S(  7,  46),   S(  4,  29),   S(  0,  -2),
            S(-17, -73),   S(-33, -59),   S(-19, -51),   S(  2,  -8),   S( 12,  33),   S(-15,  58),   S( -3,  70),   S(-23,  64),
            S(-26, -23),   S(-44, -30),   S(-31,   2),   S( 12,  20),   S(-11,  51),   S(  5,  92),   S(-27, 138),   S( -6,  54),
            S(-27, -42),   S(-31, -31),   S(-13,  15),   S(  1,   2),   S( 36, 116),   S( 66, 164),   S( 58, 222),   S( 77,  69),
            S( -8,   7),   S( -3,  10),   S(  2,   9),   S(  7,  25),   S( 27,  81),   S( 85, 192),   S( 32, 182),   S( 43,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-32,   4),   S( -2,  12),   S(-46,  15),   S(-21,  -8),   S(-31,  -5),   S(  1, -31),   S(-43, -47),   S(-33, -17),
            S(-37,  55),   S( 19, -40),   S(-41,  14),   S(  9, -20),   S(-11, -18),   S(-23, -16),   S(-30, -24),   S(-71, -22),
            S(  5,  63),   S( -4,  -9),   S(  3,  -8),   S(-24,  34),   S( 11,   9),   S(-33,   3),   S(-11, -27),   S(-41, -49),
            S( 18, -17),   S( 42,  10),   S( 10,  28),   S( 28,  28),   S(  7,   7),   S( -1,   7),   S( -7, -13),   S( -1,  -4),
            S( 16, -29),   S( 36,  14),   S( 14,  10),   S( 67,  -6),   S( 44,  -5),   S( 29,  23),   S( 23, -13),   S(-60, -12),
            S( 17, -14),   S( 11,  10),   S( 32,  13),   S( 56, -13),   S( 37, -43),   S( 20,  15),   S( 12, -22),   S( -3, -13),
            S( 13, -31),   S( 13, -39),   S( 18, -26),   S( 35, -31),   S( 23, -19),   S( -8, -27),   S(-10, -43),   S(-22, -34),
            S(-70, -56),   S( -6,   0),   S( -5, -19),   S(  0, -46),   S(-20, -24),   S( 20,   9),   S( -7,   2),   S( 15,  -3),

            /* knights: bucket 1 */
            S(-43,  22),   S(-53,  86),   S( 20,  39),   S(-27,  66),   S(-19,  50),   S(-23,  27),   S(-33,  51),   S(-19, -13),
            S( 34,  21),   S( -5,  35),   S( -5,  28),   S( -7,  45),   S( -9,  29),   S(-15,  15),   S( 15, -15),   S(-28,  11),
            S(-32,  26),   S( 12,  11),   S( -1,  14),   S( 14,  30),   S(  5,  32),   S(-27,  31),   S(-18,   6),   S(-30,  20),
            S(  1,  39),   S( 55,  26),   S( 17,  45),   S( 24,  28),   S(  8,  32),   S( -2,  32),   S( 17,  13),   S( 15,  16),
            S(  1,  49),   S( 17,  27),   S( 27,  30),   S( 38,  28),   S( 37,  28),   S( 30,  25),   S( 26,  17),   S( 16,  17),
            S(  8,  15),   S( 19,  11),   S( 18,  34),   S( 46,  16),   S( 12,  23),   S( 35,  34),   S( 27,   7),   S( 16, -11),
            S( 34,   2),   S( 25,  15),   S(-17, -19),   S( 14,  32),   S( 31,  -7),   S( 26,  -5),   S(-32,   9),   S(-10, -24),
            S(-99, -67),   S(-23, -14),   S( -5,  13),   S(  2,  25),   S(-14,   0),   S(-26, -22),   S( -5,  -7),   S(-39, -39),

            /* knights: bucket 2 */
            S(-59,   4),   S( -1,  21),   S(-33,  53),   S(-25,  56),   S(-38,  61),   S(-37,  70),   S(-18,  29),   S(-22,  11),
            S(-15, -20),   S(-21,  10),   S(-14,  18),   S(-12,  35),   S( -6,  25),   S(-14,  53),   S(-36,  55),   S(-39,  63),
            S(-18,  23),   S( -5,   9),   S(-10,  30),   S( 13,  23),   S(  2,  35),   S(  4,  13),   S( -8,  40),   S(-26,  26),
            S( -9,  39),   S(-22,  39),   S(  5,  40),   S(  3,  52),   S( -2,  47),   S( -5,  38),   S(  4,  40),   S( -2,  43),
            S( 16,  23),   S(-17,  36),   S( -5,  48),   S(-17,  55),   S(  2,  49),   S( -9,  45),   S(  2,  35),   S( -3,  22),
            S(-24,  32),   S(  3,  32),   S(-26,  54),   S(-16,  48),   S(-26,  47),   S(  4,  28),   S(-28,  11),   S( 14,   1),
            S(-20,  22),   S(-32,  15),   S(-33,  18),   S(-37,  34),   S(-13,  14),   S(  1,  22),   S(-50,  34),   S(-35,   9),
            S(-144,  16),  S( -4,  -3),   S(-80,  30),   S(-30,  10),   S( -3,   8),   S(-59,  -1),   S( -3,  -2),   S(-178, -55),

            /* knights: bucket 3 */
            S(-48, -12),   S( 15, -30),   S(-22,  -6),   S(  9,  -5),   S( 11, -11),   S(-13,   9),   S( 21, -17),   S( -7, -30),
            S(-12,  -2),   S(-23, -10),   S(-16,  -9),   S(  9,   8),   S( 22,  -5),   S(  0,  -9),   S( -1, -12),   S(-17,  55),
            S(  2, -33),   S(  5,  -3),   S(  5,   0),   S( 19,   7),   S( 22,  20),   S( 22,   4),   S( 16,   0),   S( 12,  28),
            S(  2,   2),   S( 14,  11),   S( 20,  34),   S( 25,  33),   S( 33,  32),   S( 27,  34),   S( 30,  22),   S( 27,  17),
            S( 29,   3),   S(  9,  20),   S( 38,  12),   S( 33,  40),   S( 29,  39),   S( 35,  45),   S( 40,  40),   S( 19,  12),
            S(  6,   7),   S( 33, -12),   S( 50,   0),   S( 62,   3),   S( 72, -16),   S( 77, -10),   S( 15,   7),   S( 10,  39),
            S( 28,  -8),   S( 16,   5),   S( 45, -23),   S( 53, -10),   S( 67, -33),   S( 62, -38),   S( 63, -68),   S( 48, -25),
            S(-106,   8),  S(-24,   5),   S(-28,   1),   S(  4,  13),   S( 34, -12),   S( -8, -15),   S(-12, -25),   S(-70, -49),

            /* knights: bucket 4 */
            S( 13,  17),   S(-51,   3),   S( 13,  25),   S( -3,  -7),   S(-21, -13),   S(-32, -25),   S( -9, -52),   S(-30, -45),
            S( 33,  22),   S(-23,  35),   S( 14, -24),   S(  9,  -7),   S( 16, -16),   S( -6, -42),   S( 11,  -2),   S(  0, -48),
            S(-10,  27),   S(  8,  38),   S(  8,  10),   S( 13,  15),   S( -6,   2),   S(-44,  17),   S(-48, -31),   S(-33, -57),
            S( -2,  64),   S( 34, -21),   S( 46,  25),   S( 27,  24),   S( 20,  16),   S( 97, -12),   S( 28, -28),   S( -1, -19),
            S( 60,  29),   S(-14,  47),   S( 47,  48),   S( 46,  24),   S( 42,  39),   S(-10,  30),   S( -3, -24),   S(-10,  -9),
            S(  8,  17),   S(-27,   2),   S( 83,  19),   S(  9,  11),   S( 12,  21),   S( 23,  22),   S( 11,  28),   S(-11, -22),
            S( -7,   5),   S(-16,   7),   S( 12,  -1),   S(  4,  36),   S(  8,   8),   S(  6, -16),   S(  3, -10),   S(-15,  -4),
            S(-11,  -8),   S( -1,  -6),   S(  9,   9),   S(  0,   2),   S( -7, -10),   S(  9,  19),   S( -2,   4),   S( -4, -20),

            /* knights: bucket 5 */
            S(  9,  -3),   S(-41,  46),   S( 29,  35),   S( 19,  49),   S( 33,  25),   S( 11,   0),   S(  0,  16),   S(-22, -22),
            S( 11,   0),   S( 29,  47),   S( 17,  25),   S(-12,  43),   S( 30,  38),   S( -1,  36),   S( 20,  27),   S(-15, -28),
            S(  1,  26),   S(-13,  40),   S( 59,  23),   S( 40,  45),   S(-18,  53),   S( -4,  30),   S(-21,  19),   S(  6,  -4),
            S( 33,  47),   S( 11,  49),   S( 36,  45),   S(  6,  61),   S( 19,  50),   S( 13,  48),   S( 26,  47),   S( 14,  37),
            S( 21,  52),   S( 35,  36),   S( 50,  54),   S( 65,  47),   S( 82,  50),   S( 29,  48),   S( 42,  39),   S( 37,  32),
            S(  5,  31),   S(  1,  51),   S( 24,  33),   S( 17,  59),   S( 40,  47),   S( 15,  58),   S( 21,  17),   S( -6,  30),
            S( 18,  54),   S( -7,  63),   S( 30,  46),   S( 15,  63),   S(  6,  52),   S(  8,  46),   S( 22,  66),   S(  2,  -1),
            S( -5,   5),   S( -1,  12),   S(  7,  38),   S( -4,   3),   S(  9,  40),   S(  1,  31),   S(  8,  37),   S(-18, -19),

            /* knights: bucket 6 */
            S(  0, -43),   S(-20,  -3),   S( 28,  28),   S(-27,  42),   S(-31,  50),   S( 12,  41),   S(-12,  33),   S(-15,  25),
            S( -6, -31),   S( 50,   0),   S( 12,  12),   S(-36,  42),   S(-61,  71),   S( 25,  50),   S( 14,  49),   S( -3,   7),
            S(-27, -18),   S( -2,   3),   S(-10,  28),   S( 19,  35),   S(-21,  63),   S(-39,  60),   S(  8,  47),   S( -2,  42),
            S( 36,   7),   S( 38,  13),   S( 48,  33),   S( 78,  28),   S( 25,  51),   S( 12,  58),   S( 14,  61),   S(-23,  72),
            S(  4,  36),   S( 69,  -5),   S( 57,  39),   S( 75,  35),   S( 86,  41),   S( 85,  39),   S( 18,  61),   S( 20,  55),
            S( 23,  25),   S( 13,  16),   S( 68,  24),   S( 50,  46),   S( 58,  51),   S( 34,  38),   S( 22,  43),   S( 38,  39),
            S(-23,  21),   S( -1,  34),   S(-29,  37),   S( 29,  31),   S( -1,  57),   S( 21,  42),   S( 18,  68),   S( -9,  28),
            S(-43,  -2),   S( 14,  38),   S( 27,  36),   S(  9,  36),   S( 21,  32),   S(  9,  56),   S( 19,  57),   S( 10,  22),

            /* knights: bucket 7 */
            S(-35, -57),   S(-191, -46),  S(-71, -45),   S(-60, -16),   S(-42, -10),   S(-36, -17),   S(-13,   2),   S(-17,   5),
            S(-52, -79),   S(-38, -47),   S(-38, -31),   S(-52,   5),   S(-46,  10),   S(  0, -12),   S(-19,  47),   S(  0,  27),
            S(-83, -67),   S(-58, -34),   S(-52,   2),   S( 19, -16),   S(-22,  10),   S(  3,  11),   S(-12,  56),   S( 43,  52),
            S(-60, -22),   S( 15, -23),   S( -7,  15),   S( 33,   3),   S( 44,   4),   S( 12,  18),   S( 15,  15),   S(-23,  34),
            S(-60, -23),   S(-20, -25),   S( 49, -18),   S( 81, -12),   S(105,   0),   S( 66,  25),   S( 92,   4),   S( 76,  20),
            S( -7, -41),   S( 17, -38),   S(-20,   2),   S( 31,   3),   S( 65,  13),   S( 75,  10),   S( 56, -14),   S( -4,   9),
            S(-35, -34),   S(-66, -22),   S(  3, -15),   S( 30,  16),   S( 34,  20),   S( 40,   0),   S(-20,  21),   S(  0,   2),
            S(-38, -31),   S(-10, -11),   S(-28, -16),   S(  7,  10),   S(  9,   2),   S( 21,  16),   S( -5, -12),   S( -4,  -9),

            /* knights: bucket 8 */
            S( -1,  -8),   S( -9, -10),   S( -3,  -4),   S( -9, -31),   S(-10, -41),   S(-10, -51),   S( -2,  -2),   S( -5, -24),
            S(  2,   0),   S( -6, -12),   S( -7, -29),   S(-18, -44),   S(-30, -27),   S(-17, -70),   S(-13, -59),   S(-17, -37),
            S(  4,  16),   S(-22, -20),   S( 21,   7),   S(  5,  -1),   S(  3, -30),   S(-15, -12),   S(-13, -38),   S( -8, -42),
            S(-19,  -3),   S(  1,  -4),   S( -1,  15),   S(  5,  34),   S(  8,  -1),   S(  8,  11),   S(-13, -50),   S( -3, -17),
            S( 26,  52),   S( 10,   9),   S( 15,  37),   S( 35,  19),   S( 12,  34),   S( -4,   1),   S(  5, -19),   S( -7,  -8),
            S( 13,  35),   S( 10,   6),   S( 29,  24),   S( 32,  17),   S(  3,   1),   S( -1,  -6),   S( -7, -28),   S( -6, -10),
            S(  2,  12),   S(  0,   1),   S(  6,   9),   S( 10,   8),   S(  6,   7),   S(  5,  20),   S(  2,  11),   S( -1,   1),
            S(  1,   0),   S( 11,  32),   S(  5,  15),   S( -2,  -1),   S(  2,  10),   S( -5, -20),   S(  3,   4),   S( -4,  -5),

            /* knights: bucket 9 */
            S(-10, -30),   S(-20, -37),   S(-18, -48),   S( -3, -15),   S(-23, -55),   S(-15, -40),   S( -3, -15),   S( -4, -28),
            S(-12, -39),   S(-12,  -1),   S(-11, -50),   S(-13,  -7),   S( -4, -14),   S( -7, -33),   S( -5,  -3),   S(-15, -42),
            S(  5,   5),   S(-10, -14),   S(  3, -14),   S(  3,   4),   S(  4,  19),   S(-32,   0),   S(-13, -11),   S( -8, -18),
            S(-15,  -4),   S( -6,  -8),   S(  5,  30),   S( 18,  32),   S( 28,  26),   S( 10,  26),   S(-11, -35),   S( -2,  -1),
            S(  0,  21),   S( 19,   7),   S( 19,  41),   S(  2,  43),   S( 10,  19),   S( 13,   0),   S(  3, -27),   S(  5,   9),
            S(  1,   0),   S(  7,  31),   S( 16,  34),   S( -6,  21),   S( 35,  38),   S( 17,  14),   S(  7,  15),   S( -7, -24),
            S(  1,  -1),   S( -2,  19),   S( 18,  36),   S( 11,   4),   S( 12,  39),   S( -3, -18),   S(  3,  16),   S( -3,  -1),
            S(  1,  -1),   S(  3,   7),   S( 11,  26),   S( 14,  28),   S(  8,   7),   S(  0,   4),   S(  2,   3),   S( -1,  -5),

            /* knights: bucket 10 */
            S(-18, -51),   S(-17, -55),   S(-13, -27),   S(-18, -21),   S(-12, -12),   S(-15, -44),   S( -3,  15),   S(  4,  20),
            S( -7, -25),   S( -7, -15),   S(  0, -17),   S(-20, -34),   S(-24, -35),   S( -8, -40),   S( -9,  -8),   S( -5, -13),
            S(-17, -52),   S(-18, -61),   S( -9, -11),   S(-15, -14),   S( 13,   3),   S(-13,  -1),   S( -6,   4),   S( -7,   4),
            S( -8, -18),   S( -5, -44),   S(  4, -32),   S( 19,  15),   S( 10,  40),   S( 18,  25),   S(  6,  16),   S( 10,  42),
            S( -8, -46),   S(-12, -28),   S( 16,  14),   S( 23,  33),   S( 20,  54),   S(  1,  29),   S( 20,  14),   S( 22,  51),
            S(-11, -41),   S( -5, -21),   S( -3,  -7),   S( 13,  43),   S( 36,  63),   S( 31,  43),   S( 28,  58),   S( 17,  53),
            S( -1,  -3),   S(-11, -33),   S(  1,  -8),   S( 26,  26),   S( 17,  28),   S(  9,  31),   S(  1,  -3),   S(  8,  23),
            S( -4, -17),   S(  3,   9),   S( -8, -20),   S(  3,  -4),   S( 11,  35),   S(  5,  24),   S(  2,  12),   S( -1,  -3),

            /* knights: bucket 11 */
            S( -1,  -1),   S(-19, -29),   S( -8, -45),   S(-10, -26),   S(-21, -50),   S(-12, -19),   S( -6,  -6),   S( -4,  -6),
            S( -8, -11),   S(-13, -21),   S(-15, -78),   S(-28, -25),   S( -9,  -3),   S(-29, -38),   S(-17, -30),   S( -8, -10),
            S(-15, -54),   S(-23, -61),   S(-26, -32),   S(  0,   4),   S(-14,   6),   S(-18,  18),   S(  9,  -5),   S( -1,  14),
            S(-13, -30),   S( -7, -28),   S(-25,  -1),   S( 27,  33),   S( 16,  21),   S( 17,  12),   S( 12,  25),   S( 14,  27),
            S( -3, -24),   S(-18, -56),   S(  8, -15),   S(  1,   9),   S( 15,  24),   S( 34,  57),   S(  7,  -2),   S( 24,  63),
            S( -8, -11),   S( -6, -26),   S(  1,  -3),   S( 40,  38),   S( 17,  25),   S( 49,  48),   S( 22,  23),   S( 14,  26),
            S(  8,  26),   S( -2,  -7),   S(  7, -11),   S( 12, -17),   S( 20,  29),   S(  0,   4),   S( 15,  37),   S( 19,  52),
            S( -4,   0),   S( -2, -19),   S(  8,  11),   S(  1,   5),   S(  1,  11),   S(  2,   3),   S(  3,   4),   S(  1,  10),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   2),   S( -2, -20),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   4),   S( -2, -14),
            S(  0,   0),   S(  1,   2),   S(  2,   6),   S( -3, -12),   S( -2,   6),   S( -4, -21),   S( -2, -12),   S(  1,   9),
            S( -5, -14),   S(  5,   4),   S( -5, -11),   S( -6, -21),   S(  0,   4),   S( -5, -17),   S(  1,  -5),   S( -7, -30),
            S( -7, -13),   S( -1,   1),   S( -8, -22),   S(  5,  17),   S( -5,  -4),   S(  0,   7),   S( -1,  -6),   S( -1,  -8),
            S(  9,  16),   S(  4,   2),   S( -6, -11),   S(  0,   4),   S( -5, -25),   S(  0,   4),   S( -1, -13),   S( -1,   1),
            S(  1,  -9),   S( -3, -22),   S(  2,   1),   S( -1,  -6),   S(  5,  11),   S( -5, -17),   S( -1,  -7),   S(  0,   3),
            S(  2,   6),   S( -9, -12),   S( -1,   9),   S(  2,  -9),   S( -5,  -8),   S( -5, -21),   S( -2,  -2),   S(  0,  -2),
            S(  2,   3),   S(  1,  12),   S( -2,  -4),   S(  2,  -2),   S( -2,  -4),   S( -2, -10),   S( -3,  -9),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -2,  -5),   S( -2,  -2),   S( -8, -13),   S( -1,   1),   S( -3, -12),   S(  1,  -2),
            S( -2,  -7),   S(  1,   5),   S( -2, -23),   S(-10, -22),   S( -6, -31),   S( -4, -25),   S(  0,   1),   S(  0,  -1),
            S( -4, -10),   S( -8, -29),   S(  7,  18),   S(  0,   0),   S(-12, -37),   S( -9, -23),   S( -3, -15),   S( -6, -28),
            S( -8, -15),   S(  5,  13),   S(  1,   2),   S(-10, -26),   S( -1,  -7),   S(  6,  13),   S(  0, -13),   S( -5, -10),
            S(  3,  10),   S( -1,  -2),   S(  3,  -7),   S( 10,  22),   S(  5, -10),   S( -3,  -7),   S(  2, -12),   S(  1,   1),
            S( -3,  -9),   S( 14,  14),   S(  7,  24),   S(-13,  12),   S(  5,   5),   S( -9, -33),   S(  4,   6),   S( -4,   2),
            S(  1,   6),   S(  2,   4),   S(  9,  11),   S(  7,  10),   S( 14,  22),   S( -5, -22),   S( -3,  -2),   S( -5,  -3),
            S( -1,   1),   S( -1,  -7),   S( -1,   1),   S(  1,  -9),   S( -2,  -1),   S(  3,  -1),   S(  0,  -2),   S( -2,   0),

            /* knights: bucket 14 */
            S( -3, -23),   S( -5, -25),   S( -2,  -2),   S( -3,   3),   S( -8, -24),   S( -2, -15),   S( -1,  -5),   S(  0,   2),
            S(  0,  -3),   S( -3,  -8),   S(-15, -60),   S( -8, -36),   S( -1, -10),   S(  1,   6),   S(  1,  -3),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-10, -53),   S(  1,   3),   S( -4, -19),   S( -4,  -8),   S(  0,  -1),   S(  2,   9),
            S(  0,   5),   S( -6, -33),   S(-15, -39),   S(-11, -38),   S( -2, -20),   S(  3,   2),   S( -3, -16),   S( -7, -11),
            S( -2,  -4),   S( -2, -14),   S(  1,  23),   S( -7, -31),   S( -9,  -7),   S(  2,  23),   S(  3,   5),   S( -4,  -7),
            S( -4,  -8),   S(  3,  -2),   S( -9, -30),   S(  5,   2),   S( 15,  26),   S(  4,   9),   S( -3,   0),   S(  0,  -3),
            S(  0,  -3),   S( -2, -11),   S(  7,  -4),   S( -1,  -8),   S( -7, -10),   S( -3,  -9),   S( -6,  -4),   S(  1,   7),
            S( -1,  -2),   S(  2,   4),   S( -2, -11),   S(  7,  -1),   S(  4,  17),   S(  1,   2),   S( -2,  -8),   S( -1,  -5),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -1, -14),   S( -1, -13),   S( -7, -14),   S( -2,  -1),   S( -2,  -5),   S(  1,   0),   S(  0,  14),
            S( -2,  -6),   S(  0,  -2),   S( -4, -18),   S( -6, -26),   S( -2,  -5),   S( -1,  -8),   S(  0,   0),   S( -1,  -3),
            S( -6, -16),   S( -7, -16),   S( -3, -11),   S(-14, -39),   S( -5, -24),   S( -1,  -3),   S( -1,  -1),   S( -2,   1),
            S( -6, -17),   S( -6, -32),   S( -6, -19),   S(  0,  -7),   S(  1, -16),   S(  7,  24),   S(  5,  10),   S( -4,  -1),
            S(  0,  -2),   S( -2,  -5),   S( -1, -15),   S( -7, -10),   S(  3,  18),   S(  3,  10),   S( -6,  -8),   S( -2,   1),
            S( -3,  -4),   S( -2,  -4),   S( -2, -20),   S( -3,   8),   S( -5, -14),   S( -6,  11),   S( -3,   4),   S(  2,   8),
            S( -3, -12),   S( -2,  -6),   S( -1,  -9),   S( -4,  -8),   S(-11, -13),   S( -4,  15),   S( -2,  -8),   S(  3,  12),
            S(  0,  -3),   S(  0,  -1),   S( -4, -10),   S( -2,  -9),   S( -2,  -5),   S(-10,  -5),   S(  6,  17),   S( -2,   1),

            /* bishops: bucket 0 */
            S( 14,  16),   S( 18, -13),   S( 39,  17),   S(  8,  21),   S( -2,  -5),   S( 17,  -5),   S( 25, -41),   S(  0, -35),
            S( 45, -44),   S( 78,   1),   S( 37,   8),   S( 17,   3),   S(-14,  35),   S(  0, -20),   S(-37,  -1),   S(  8, -49),
            S( 23,  41),   S( 49,   8),   S( 27,   3),   S( 11,  53),   S( 20,  12),   S(-30,  26),   S( 10, -23),   S( 11, -41),
            S( 20,   9),   S( 69,  -8),   S( 37,  15),   S( 37,  35),   S(  7,  34),   S( 29,   4),   S( -2,  -9),   S(  1,  -1),
            S( 16,   2),   S( 33,  27),   S(  7,  43),   S( 60,  18),   S( 65,   1),   S( 19,  -2),   S( 25, -16),   S(-34,  -1),
            S(-39,  63),   S( -1,  21),   S( 62, -19),   S( 89, -20),   S( 43,  33),   S( -3,   1),   S(  2,  14),   S(  1,  11),
            S(-13,  12),   S( 10,   0),   S( 43,  -7),   S(  1,  35),   S(-29,  -3),   S( 26,  27),   S(  2, -12),   S(-14, -10),
            S(-34, -44),   S( 10,   2),   S(  2,   6),   S(  4, -13),   S( 19,  24),   S( 33,   9),   S( -2,  44),   S(-23,   0),

            /* bishops: bucket 1 */
            S( 33,   8),   S( -8,  29),   S(  7,  36),   S( 13,  28),   S( -5,  27),   S(  1,  33),   S( -6,   1),   S(-46,  -7),
            S( 10, -16),   S( 34, -14),   S( 52,   7),   S( 27,  30),   S( -9,  17),   S(  9,   0),   S(-32,  -4),   S( 14, -19),
            S( 37,  -5),   S( 19,   5),   S( 37,  -9),   S( 19,  24),   S( 21,  26),   S(-20,   4),   S( 30,  -4),   S(  5, -31),
            S( 41,   2),   S( 20,  18),   S( 12,  16),   S( 36,  25),   S(  5,  26),   S( 22,   7),   S( -3,   8),   S( 16, -12),
            S( 37,  26),   S( 10,  24),   S( 22,  27),   S(  3,  35),   S( 29,  13),   S(  3,  18),   S( 31, -17),   S( -8,   9),
            S( -2,  21),   S( 31,  37),   S( 34,   4),   S( 58,  -4),   S( 21,  19),   S( 37, -14),   S(  3,  30),   S( 47, -16),
            S( -9,  44),   S(-27,  20),   S( 17,  29),   S( 35,  25),   S( 41,  25),   S(-18,  23),   S( 36, -20),   S(-19,  36),
            S( 13,   1),   S(  8,   4),   S(  2,  11),   S(-23,  20),   S( 18,  15),   S(-10,   4),   S( 11,   4),   S( -6,   9),

            /* bishops: bucket 2 */
            S( 16, -18),   S(  7,  17),   S( -3,  20),   S(-28,  51),   S(-13,  36),   S(-28,  29),   S(-20,  -7),   S(-46,  16),
            S(-23,  22),   S(  4, -16),   S( 21,  11),   S( -4,  28),   S( -2,  35),   S( 17,   8),   S(-10, -20),   S(  3, -35),
            S( -6,   2),   S( -2,  12),   S(  4,   7),   S( -3,  44),   S(  6,  34),   S(  0,  16),   S( 16,  13),   S(-16,  -6),
            S(  1,  11),   S( -9,  15),   S(-11,  36),   S(  6,  38),   S( -3,  44),   S(  8,  25),   S(  8,  16),   S(  8,   0),
            S(  7,   4),   S(-17,  35),   S( -5,  27),   S(-29,  49),   S( -9,  40),   S( -6,  48),   S(  4,  22),   S(-28,  29),
            S(  6,  24),   S( -3,  15),   S(-23,  30),   S(-14,  26),   S( 14,  14),   S( -6,  10),   S( -2,  57),   S( -2,  24),
            S( -2,  18),   S(-22,  10),   S(-26,  53),   S( 19,   2),   S( -3,   3),   S(-18,  10),   S(-70,  13),   S(-37,  34),
            S(-55,  28),   S(-37,  43),   S(-28,  26),   S(-44,  25),   S(-50,  35),   S(-37,  16),   S(  4,  12),   S(-71,   5),

            /* bishops: bucket 3 */
            S( -1,   1),   S( 36, -10),   S( 30,  19),   S( 17,  18),   S( 21,  10),   S( 41,  -6),   S( 41, -24),   S( 41, -65),
            S(  9,   5),   S(  6,  -2),   S( 31,  -5),   S(  9,  32),   S( 22,  10),   S( 22,  23),   S( 44,  -6),   S( 37,  -7),
            S( 21,   6),   S( 14,  18),   S( 12,  16),   S( 26,  22),   S( 24,  51),   S( 24,   7),   S( 41,  23),   S( 45, -12),
            S( 31,  -6),   S( 28,   9),   S( 20,  34),   S( 27,  47),   S( 31,  39),   S( 32,  34),   S( 27,  24),   S( 27,  -6),
            S( 20,   2),   S( 26,  17),   S( 47,  13),   S( 31,  46),   S( 26,  48),   S( 38,  28),   S( 19,  35),   S( 23,  31),
            S( 30,   3),   S( 37,  22),   S( 27,  12),   S( 45,  14),   S( 28,  21),   S( 55,   3),   S( 50,  15),   S(  6,  66),
            S( 17,   7),   S( -7,  15),   S( 41,  23),   S( 23,  17),   S( 13,  16),   S( 18,   3),   S( -2,  27),   S( 16,  35),
            S(-35,  56),   S( -3,  32),   S( 56,   9),   S( 24,  14),   S(-15,  33),   S( -3,  33),   S( 25,  -1),   S( 56, -29),

            /* bishops: bucket 4 */
            S(-24, -27),   S(-24,   6),   S(-36,  -4),   S(-26,  18),   S(-23,  27),   S(-46,  27),   S( -1, -11),   S(-14, -12),
            S(-10,   7),   S(  4,   5),   S(-11,  33),   S(-29,  18),   S(-19,  -5),   S( 37,  -4),   S(-27,  -8),   S( 11,  -4),
            S(-13,  -2),   S(-35,  36),   S( 13, -15),   S(-25,  17),   S(  3,  29),   S( 26, -20),   S(-24,  -7),   S(-54,  -3),
            S(-35,  25),   S( -3,  35),   S( 47,  29),   S( 30,  39),   S( 16,  24),   S( 54,  -6),   S( 49,  -5),   S(-11, -35),
            S(  5,  17),   S(  5,  47),   S(-17,  55),   S( 20,  44),   S( 36,  12),   S( 34, -16),   S( -8, -19),   S( 13, -10),
            S( -6,  34),   S( 24,  18),   S(-11,  30),   S( 19,  15),   S( 42,  10),   S(  8, -10),   S( 19, -34),   S(  2,  -8),
            S(-17,   8),   S( 30,  17),   S( 15,  20),   S( 24,  17),   S(  9,  -4),   S(  1,  17),   S( -1,   4),   S(  6, -26),
            S( 11, -18),   S(-11, -38),   S(  1,  -6),   S( -5,  -2),   S(  6, -13),   S(  0,   7),   S( -2,  -9),   S( -5,   0),

            /* bishops: bucket 5 */
            S(-18, -15),   S(-15,  35),   S(-40,  30),   S(-22,  33),   S(-40,  34),   S( -1,  14),   S( -5,  15),   S(-26,  12),
            S(-27,  35),   S(-15,   6),   S(-32,  57),   S(  0,  28),   S(-30,  36),   S(-30,  28),   S(-34, -13),   S(-11,  -3),
            S( -1,  15),   S(  2,  38),   S( 20,  14),   S(-24,  53),   S(  1,  37),   S(-31,   1),   S(-26,  35),   S(-20,   5),
            S( 31,  14),   S( 24,  30),   S(-14,  60),   S( 28,  33),   S( 28,  39),   S( 19,  33),   S( 20,  -4),   S( 10,  26),
            S( 27,  44),   S( 35,  15),   S( 53,  33),   S( 80,  35),   S( 46,  23),   S( 42,  21),   S( 38,  16),   S( -8,   4),
            S( 19,  39),   S( 30,  46),   S( 36,  24),   S( 31,  36),   S( -2,  38),   S( 18, -16),   S(-22,  47),   S( -1,  32),
            S(  1,  38),   S(-30,  14),   S( 12,  41),   S(  5,  52),   S( 29,  29),   S( 32,  38),   S( -3,  18),   S( -3,  28),
            S( -3, -11),   S( 14,  34),   S( 14,  12),   S(  5,  36),   S(  1,  55),   S( 12,  23),   S( 28,  53),   S( -9,  -3),

            /* bishops: bucket 6 */
            S(-13,  13),   S(  2,  26),   S(-26,  30),   S(-37,  35),   S(-36,  24),   S(-38,  31),   S(-22,  52),   S(-20,   6),
            S( 19,   7),   S(  2, -12),   S(-21,  31),   S( -4,  29),   S(-31,  44),   S(-19,  26),   S(-101,  30),  S( 15,  23),
            S( 19,   0),   S( 11,  10),   S( 29,   0),   S( 22,  29),   S( 37,  26),   S( 11,  12),   S(  7,  32),   S(-40,  20),
            S(-11,  41),   S( 21,  16),   S( 36,  23),   S( 30,  37),   S( 39,  35),   S( 36,  30),   S( 30,  33),   S(-15,   1),
            S( -8,  21),   S( 58,   7),   S( 24,  28),   S( 49,  26),   S( 96,  30),   S( 59,  29),   S( 34,  32),   S(-29,  46),
            S(  9,  10),   S(-39,  47),   S( 11,  19),   S( 15,  42),   S( 38,  31),   S( 29,  29),   S(  4,  49),   S(-13,  45),
            S(-24,  30),   S(-29,  24),   S(  3,  40),   S(-10,  33),   S( 45,  22),   S( 23,  30),   S(-10,  34),   S( -4,  32),
            S(  4,  43),   S( 12,  32),   S(  8,  39),   S(  0,  45),   S(-18,  37),   S( 31,  16),   S( 11,  22),   S( 12,   7),

            /* bishops: bucket 7 */
            S(-15, -40),   S( -4,   2),   S(-32, -28),   S(-51,  10),   S(-32, -11),   S(-77,  18),   S(-72, -32),   S(-65,   4),
            S(-33, -29),   S(-56, -40),   S(-21,  -4),   S( -1, -14),   S(-30,   1),   S(-45,  14),   S(-51, -12),   S(-32,   5),
            S(-35, -21),   S(  5, -16),   S( 23, -37),   S( 20,   1),   S(-31,  20),   S(-21, -14),   S(-38,  46),   S(-30,  25),
            S(-37,  15),   S( 57, -34),   S( 78, -20),   S( 60,   5),   S( 83,   3),   S(  1,  24),   S( 20,  32),   S(-15,  28),
            S( 24, -49),   S( -4, -20),   S( 65, -33),   S(103, -25),   S( 67,  27),   S( 67,  18),   S( -5,  44),   S( 22,   8),
            S(-26, -13),   S(-22,   3),   S( 33, -43),   S( 22,  -2),   S( 47,  -8),   S( 51,   6),   S( 55,  18),   S( 22,   1),
            S( -2, -16),   S(-39,  -8),   S(  9,  -3),   S( 13,  -8),   S( 15, -21),   S( 34,  -7),   S( 11,   0),   S( 13,  12),
            S(-14,  -6),   S(-11,  12),   S(-33,   8),   S(  4,  -6),   S(  9,  -5),   S( 18,  -5),   S( 24,   8),   S(  5,   7),

            /* bishops: bucket 8 */
            S(-10,  -9),   S(-12, -33),   S(-43,  -6),   S( -3, -27),   S( -6,  19),   S(-24,  -4),   S(  6,  21),   S( -5,  -9),
            S( -7,  -3),   S(-33, -48),   S(-12, -22),   S(-16,  -4),   S( 10,  -9),   S(-17, -27),   S(-18, -53),   S( -5,  -8),
            S(  2,   1),   S(-10,  11),   S(-24,   9),   S( -9,  19),   S( -6,  13),   S( -7, -38),   S(  7, -41),   S(-31, -37),
            S(  6,  32),   S( -5,  46),   S(  7,  42),   S( -2,  17),   S( 18,  23),   S( -1,  11),   S(  5, -15),   S( -5, -17),
            S( 15,  36),   S( 13,  68),   S( -9,  34),   S( 47,  46),   S(  4,  24),   S( 18,  12),   S(  8, -27),   S(-10, -15),
            S( -2,   6),   S( 13,  37),   S(  9,  20),   S(-14,  21),   S( 29,  13),   S( -8, -14),   S(-13, -14),   S(-18, -21),
            S( -4,   4),   S( 10,  24),   S(  8,  22),   S(  0,   1),   S(  5,  12),   S( -1,  22),   S(-13, -15),   S( -9, -28),
            S( -8, -13),   S(  1, -28),   S( -1,  -7),   S( -1, -15),   S(-19, -11),   S( -6,  -6),   S(  0,  13),   S( -8,   7),

            /* bishops: bucket 9 */
            S(-25, -32),   S( -6,   2),   S(-19,   2),   S( -9, -23),   S(-33, -28),   S(-18, -37),   S(-17, -12),   S(  7,  -5),
            S(-16, -18),   S(-37, -31),   S( -8,  -8),   S(-14,  15),   S(-46,  29),   S(-18, -15),   S(-17, -19),   S( -5,  -6),
            S(  8,  -2),   S( 18,  11),   S(-26, -17),   S(-13,  26),   S(  3,  15),   S( -9, -23),   S(-14, -24),   S( -5,  25),
            S(-15,   7),   S( 17,  18),   S( -8,  29),   S(  9,  25),   S( 19,  28),   S( 11,   6),   S(  6,   0),   S(-15, -22),
            S( -1,  19),   S( 24,  27),   S(  7,  43),   S( 12,  54),   S(-13,  18),   S(  5,  35),   S( -4,  40),   S( -6,  -5),
            S(-12,   3),   S( 20,  50),   S(  1,  20),   S( 22,  22),   S( 11,  36),   S( -6,   0),   S(-17,   5),   S(-12, -11),
            S(  3,  12),   S( 18,  10),   S(  4,   9),   S(  2,  45),   S( 19,  38),   S(  7,   5),   S( -8, -15),   S( -5,  -3),
            S( -4, -27),   S( -8,  18),   S( -5,  17),   S(-19, -14),   S(-14,  -4),   S(  5,  25),   S(  0,   3),   S(-13, -19),

            /* bishops: bucket 10 */
            S(-24, -14),   S(  4, -26),   S(-33, -27),   S(-18, -23),   S(-22, -10),   S(-23, -21),   S(-11, -21),   S(-18, -27),
            S(  5, -18),   S(-28, -38),   S( -5,  -8),   S(-40,   4),   S(-37,   7),   S(-20,  22),   S(-29, -55),   S(-12, -17),
            S(  9, -12),   S(  4,  -9),   S(-36, -46),   S(  4,  10),   S(-36,  33),   S(-38,  15),   S(-21,  28),   S(  4,  16),
            S(-10, -19),   S(  6,  11),   S( 14,  -3),   S( 19,   8),   S( 13,  31),   S( -9,  59),   S(  6,  31),   S( 15,  27),
            S(-18,   0),   S(  4,   5),   S( -4,  20),   S( 34,  31),   S(  3,  66),   S( 23,  53),   S( 11,  43),   S(  1, -17),
            S(  3, -25),   S(-23,   1),   S(-22, -10),   S(-13,  30),   S( 25,  39),   S( 35,  26),   S(  9,  53),   S(  2,  10),
            S(-21,  -9),   S(-11, -47),   S( -9,  -9),   S( 21,  13),   S( -4,  -4),   S( 18,  37),   S( 15,  35),   S( 12,  13),
            S( -7, -29),   S(-10,   3),   S(  4,  17),   S(-11,   3),   S( -9,  15),   S(-11,  -8),   S(  9,  -1),   S(  5,  21),

            /* bishops: bucket 11 */
            S(-20,   1),   S(-32, -14),   S(-50, -47),   S(-23, -30),   S(-20, -10),   S(-65, -46),   S(-10, -12),   S(-23, -23),
            S(-12, -18),   S( -1, -39),   S( -8,  -6),   S(-25, -35),   S(-44,  -9),   S(-29, -28),   S(-26, -43),   S(-23, -34),
            S(-10, -47),   S(  3, -46),   S(-27, -22),   S(  0,  -7),   S( -2,  -4),   S(-36,  11),   S( -9,  25),   S( -2,  18),
            S(-16, -37),   S(-10, -34),   S(  7,  -7),   S(  4,  -3),   S( 14,  25),   S( -1,  61),   S(  8,  51),   S( 17,  26),
            S( -9, -23),   S(-16, -40),   S(-14,  23),   S( 49,   7),   S( 34,  43),   S( -1,  63),   S( 19,  58),   S( 14,  25),
            S(-18, -51),   S(-30,  -1),   S(-12, -36),   S(  9,  16),   S(  5,  33),   S( 17,  30),   S( 27,  38),   S( -5,  -2),
            S( -8,  -7),   S(-19, -43),   S(-19,   3),   S( -6, -16),   S(  9,  -1),   S( 35,  17),   S( -9,   4),   S( 13,  26),
            S(-19, -15),   S(-21,  -3),   S( -7,  12),   S(  9,   2),   S( 11,   1),   S(-18, -26),   S(  3,   6),   S( -2, -21),

            /* bishops: bucket 12 */
            S(  0,   3),   S( -7, -13),   S(-12, -29),   S( -7, -26),   S( -9, -19),   S(-11, -20),   S( -1,  11),   S( -5,   1),
            S( -7,  -7),   S(-13, -32),   S( -7, -13),   S( -6, -13),   S(-14, -23),   S( -2,  14),   S( -3,  -1),   S( -1,  -9),
            S( -1,  -4),   S(-15,  -2),   S(-12, -19),   S( -8,  -4),   S( -4,   9),   S( -5, -13),   S(-10, -43),   S( -3,  -6),
            S( -2,   3),   S(  5,   2),   S(-17, -28),   S( -2,  13),   S(  2,   7),   S(  6,  26),   S( -4,  -6),   S( -6,  -3),
            S( -1,  -4),   S(  4,  18),   S( -4,  21),   S( -9,   1),   S( -3,  -3),   S( -4,   5),   S(  5,   7),   S( -7,  -2),
            S(-13, -14),   S(  5,  61),   S(-27,   3),   S( -9,  -3),   S(  7, -15),   S( -4,   3),   S(  0,   5),   S( -1,  -5),
            S( -2,  -5),   S( -5,  12),   S(  4,  17),   S( -7,   6),   S( -1,   9),   S(  8,  17),   S( -7, -17),   S( -1,   5),
            S( -2,  -3),   S(  0,  -6),   S( -6,   0),   S(  6,   7),   S(  1,   9),   S(  0,   3),   S(-10,  -1),   S(  0,  -3),

            /* bishops: bucket 13 */
            S( -8, -42),   S(-13, -29),   S(-13, -17),   S(-15, -18),   S(-16, -19),   S( -8,   1),   S( -2,  -5),   S( -8,  -9),
            S( -4,  -7),   S(-11, -12),   S(-13, -29),   S(-18,  -9),   S(-13,   7),   S( -8,   1),   S( -1, -11),   S(  2,  -2),
            S( -9, -11),   S( -5,  -7),   S( -7,  11),   S(-21,   1),   S(-12, -21),   S( -3, -11),   S( -3, -29),   S(  4,  20),
            S( -2,   3),   S(-11,  -3),   S(-13,   5),   S(-23,  12),   S(  2,  19),   S(  4,  -5),   S(  0,   6),   S( -7,  -6),
            S( -3,   9),   S(-16,   5),   S(-15,  -1),   S( 19,   1),   S( -7,   3),   S( -5,   8),   S( -9, -15),   S( -2,  -8),
            S( -3,  -6),   S( -8,   3),   S(-19, -12),   S( 11,  17),   S(  3,  10),   S( -2,  -5),   S(  7,  20),   S( -3,  -6),
            S( -6,  -8),   S( -9,  -4),   S(  7,  27),   S( -7,   9),   S( -8,  -1),   S(  2,   1),   S(-15, -25),   S(  0,   7),
            S( -8, -17),   S( -2,   7),   S( -2,  -4),   S(  4,   0),   S( -1,   6),   S( -8,  -7),   S(  1,   9),   S( -2, -14),

            /* bishops: bucket 14 */
            S( -8, -17),   S(-12, -16),   S(-18, -28),   S(-17, -45),   S(-14, -36),   S( -6, -27),   S(-10, -14),   S(-10, -16),
            S(-10, -27),   S( -2, -22),   S( -7, -13),   S(-26, -41),   S(-10, -11),   S(-18, -11),   S(-15, -22),   S(  1, -13),
            S( -9, -12),   S( -8, -31),   S(-22, -30),   S(-13, -18),   S(-26,  -2),   S(-22, -30),   S( -7,   2),   S( -3,  -3),
            S( -8, -23),   S( -8,  -5),   S(-10,  -3),   S(-22,  22),   S(  1,   9),   S(-21,  14),   S(-18, -15),   S( -5, -11),
            S( -9,  -5),   S( -7,  26),   S( -7, -18),   S( -5, -19),   S(-13,  12),   S( -6,  -4),   S(  7,  23),   S(  2,  -5),
            S( -1,   4),   S( -8,   9),   S(-21, -11),   S( -8, -16),   S(  6,  10),   S( -9,  20),   S( -2,  34),   S( -7, -21),
            S( -6, -22),   S( -1,   0),   S( -7,   1),   S(  3,  18),   S( -9,  -3),   S( -1,   2),   S( -3, -12),   S( -4,  -7),
            S( -8,  -8),   S( -4,  -8),   S( -3,  -7),   S( -2,   6),   S(-10, -18),   S(  0,   8),   S(  6, -12),   S(  0,   3),

            /* bishops: bucket 15 */
            S(  4,   7),   S(  5,   5),   S(-19, -29),   S( -1, -10),   S(-10, -15),   S(-12, -23),   S( -6, -12),   S( -2, -10),
            S(  2,   4),   S( -1,  -8),   S(  3,  -1),   S( -9, -11),   S(-14, -20),   S( -6,  -7),   S( -8, -17),   S( -1,   0),
            S( -7, -14),   S(  0,  -1),   S(-12, -10),   S(-10,  -6),   S(-19, -18),   S(-17, -21),   S( -7, -10),   S(  2,  17),
            S( -4,  -7),   S(-16, -16),   S(  7, -10),   S(-22, -28),   S( -4,   7),   S( -8, -13),   S(  4,  17),   S( -1,  -9),
            S( -1,  -9),   S(-12, -17),   S(-13,  -8),   S(-18, -44),   S( -1, -22),   S(-14,  21),   S(  4,  20),   S(-10, -17),
            S( -9, -32),   S(-11, -11),   S(-18, -33),   S(-20, -11),   S( -4,  -3),   S(-10, -26),   S(  9,  38),   S(  1,  11),
            S( -3,   1),   S( -1, -16),   S( -2, -14),   S( -4,   4),   S(-11, -15),   S( -1,   9),   S(-11,  -2),   S(  4,   5),
            S( -4,  -1),   S( -1,   1),   S( -4,   0),   S( -6,  -4),   S( -8,  -5),   S(-17, -21),   S( -9, -24),   S(  1,   0),

            /* rooks: bucket 0 */
            S(-25,  10),   S( -9,  -1),   S(-16, -13),   S( -9,  -7),   S(-14,  12),   S( -9,  -7),   S(-14,  22),   S( -2,  18),
            S( 12, -61),   S( 27, -15),   S(  7,  -5),   S(  0,   3),   S( 14,   1),   S( -1,  -5),   S(-31,  21),   S(-43,  33),
            S(  2, -23),   S( 14,  28),   S( 22,   7),   S(  8,  13),   S(-15,  42),   S( -1,  10),   S(-29,  19),   S(-37,  16),
            S( 25, -21),   S( 60,   0),   S( 41,  27),   S( 38,   6),   S( 14,   9),   S( -3,  15),   S(-14,  23),   S(-36,  35),
            S( 58, -21),   S( 87, -15),   S( 64,  -2),   S( 36,  -8),   S( 45,   7),   S( 24,  10),   S( -7,  38),   S(-20,  35),
            S( 64, -42),   S(103, -35),   S( 50,   6),   S( 13,  21),   S( 41,  10),   S(-42,  34),   S( 29,  21),   S(-40,  43),
            S( 40,  -9),   S( 66,  -3),   S( 21,   7),   S(  5,  29),   S(-11,  30),   S( -5,  15),   S(-17,  35),   S(-15,  27),
            S( 29,  19),   S( 11,  47),   S( 16,  27),   S( -5,  38),   S(  3,  20),   S(  7,   2),   S( -2,  29),   S(  5,  26),

            /* rooks: bucket 1 */
            S(-74,  37),   S(-54,   7),   S(-56,  -5),   S(-43, -15),   S(-31, -21),   S(-29, -20),   S(-34,  -8),   S(-37,  21),
            S(-34,   7),   S(-53,  16),   S(-15, -12),   S(-22, -32),   S(-27, -12),   S(-39, -12),   S(-40, -16),   S(-57,  17),
            S(  4,  10),   S(-23,  33),   S(-15,  13),   S(-39,  24),   S(-44,  31),   S( -5,   2),   S(-23,   9),   S(-44,  23),
            S(-51,  54),   S(-36,  34),   S(  5,  18),   S(-15,  20),   S(-29,  32),   S(-44,  43),   S(-35,  40),   S(-32,  16),
            S( 47,  17),   S( 28,  35),   S( 24,   3),   S(-39,  40),   S(-24,  41),   S( 14,  22),   S( -2,  20),   S(-41,  25),
            S( 41,  12),   S(  5,  29),   S(  5,  25),   S(-36,  29),   S(  7,  12),   S(-30,  43),   S(-13,  26),   S(-47,  34),
            S(-18,  31),   S(  4,  28),   S( 17,  27),   S(-50,  50),   S(-26,  33),   S(  1,  33),   S(-38,  29),   S(-55,  34),
            S( 27,  30),   S( 28,  37),   S( -4,  29),   S(-47,  54),   S( -3,  16),   S( 21,  14),   S(-17,  36),   S( -5,  15),

            /* rooks: bucket 2 */
            S(-66,  38),   S(-48,  20),   S(-47,  15),   S(-57,  13),   S(-59,  10),   S(-48,   6),   S(-33, -22),   S(-48,  29),
            S(-74,  46),   S(-62,  38),   S(-44,  27),   S(-52,  10),   S(-40,  -2),   S(-51,   0),   S(-62,  18),   S(-58,  16),
            S(-71,  65),   S(-56,  53),   S(-53,  55),   S(-30,  12),   S(-40,  26),   S(-21,  24),   S(-17,  16),   S(-32,  25),
            S(-71,  65),   S(-56,  67),   S(-41,  63),   S(-37,  49),   S(-29,  33),   S(  3,  33),   S(-37,  53),   S(-19,  35),
            S(-24,  52),   S(-48,  67),   S(-45,  58),   S(-23,  42),   S( 22,  27),   S( 19,  29),   S(-25,  50),   S(-39,  47),
            S(-38,  46),   S(-32,  48),   S(-20,  32),   S( -8,  23),   S( 16,  28),   S( 46,  16),   S( 23,  19),   S(-19,  29),
            S(-53,  42),   S(-70,  70),   S(-36,  55),   S(-16,  50),   S(  7,  29),   S( 18,  22),   S(-56,  61),   S(-35,  47),
            S(-34,  67),   S(-11,  47),   S(-71,  66),   S(-32,  44),   S(-52,  58),   S(-32,  62),   S(-53,  74),   S(-20,  45),

            /* rooks: bucket 3 */
            S( -5,  74),   S( -6,  66),   S( -3,  59),   S(  5,  47),   S(  1,  44),   S(-18,  67),   S( -9,  74),   S( -7,  41),
            S(-30,  87),   S(-11,  69),   S(  5,  62),   S( 10,  56),   S( 19,  48),   S( 14,  54),   S( 43,   1),   S( 22, -39),
            S(-31,  83),   S(-12,  84),   S(  2,  77),   S( 16,  59),   S( 10,  77),   S( 26,  69),   S( 32,  69),   S(  4,  53),
            S(-24,  91),   S(-16,  84),   S( 19,  73),   S( 26,  67),   S( 20,  71),   S( -3, 108),   S( 59,  61),   S( 17,  71),
            S(-13,  99),   S( 24,  79),   S( 17,  70),   S( 38,  68),   S( 39,  68),   S( 46,  66),   S( 90,  50),   S( 54,  46),
            S(-13,  91),   S( 11,  75),   S(  9,  71),   S( 17,  69),   S( 25,  53),   S( 44,  50),   S( 83,  33),   S( 90,  15),
            S(-34,  99),   S(-18,  98),   S( -9,  93),   S( 23,  78),   S( 13,  74),   S( 24,  71),   S( 54,  65),   S( 99,  30),
            S(-74, 149),   S( -7, 101),   S(  9,  77),   S( 39,  64),   S( 45,  56),   S( 48,  67),   S(109,  50),   S( 92,  49),

            /* rooks: bucket 4 */
            S(-87,  26),   S(-10,  -4),   S(-43,   4),   S(-27,  18),   S(-31, -17),   S(  7, -49),   S( -5, -19),   S( -9, -35),
            S(-34,   1),   S(-42,   7),   S(-43,  15),   S(-38,  24),   S( -8,  -8),   S(-17, -20),   S(  4, -33),   S(-17, -21),
            S(  1,  11),   S(-28, -17),   S(-12,   9),   S(-10,  -9),   S( -1,  -3),   S( -3,  -4),   S( 35, -14),   S(-40,   2),
            S(-31, -12),   S(  4,   3),   S(-25,  16),   S( 28,   0),   S( 18,   6),   S( 16,   0),   S( 16,  11),   S( -5,  14),
            S(-17, -11),   S( -5,  30),   S(-11,  21),   S( 70,   7),   S( 21,  23),   S(  1,  18),   S( 37,  29),   S( 31,   1),
            S( 22,   9),   S( 22,  11),   S( 50,  14),   S( 39,  13),   S( 33,  18),   S(  4,  35),   S(  7,  27),   S( 23,  31),
            S(  1,  -6),   S( 33,  28),   S( 28,  28),   S( 36,  21),   S( 52,  11),   S( 11,   2),   S( 32,  18),   S( 27,  22),
            S( 35, -56),   S( 36,  44),   S( 14,  27),   S( 11,  18),   S( 16,   5),   S(  9,  25),   S( 12,   6),   S( 15,  18),

            /* rooks: bucket 5 */
            S(-45,  35),   S(-54,  53),   S(-64,  50),   S(-57,  36),   S(-44,  24),   S(-42,  41),   S(-10,  27),   S(-38,  45),
            S(-35,  35),   S(-35,  31),   S(-81,  68),   S(-53,  40),   S(-37,  23),   S(-17,  17),   S(  6,  18),   S(-30,  24),
            S(  0,  45),   S(-45,  59),   S(-54,  60),   S(-61,  59),   S(-34,  30),   S( -6,  31),   S( -7,  43),   S( -9,  44),
            S(-29,  73),   S( -4,  47),   S(-24,  65),   S(-13,  42),   S(-17,  56),   S(  6,  60),   S( -4,  55),   S(  7,  36),
            S( 12,  62),   S(  3,  65),   S( 39,  45),   S( 29,  59),   S( 35,  53),   S( 15,  75),   S( 64,  61),   S( 28,  42),
            S( 59,  56),   S( 30,  65),   S( 54,  52),   S( 25,  70),   S( 54,  49),   S( 51,  58),   S( 50,  48),   S( 42,  42),
            S( 42,  41),   S( 20,  64),   S( 42,  54),   S( 61,  40),   S( 33,  49),   S( 42,  57),   S( 63,  48),   S( 65,  44),
            S( 89,  30),   S( 67,  32),   S( 33,  55),   S( 18,  36),   S( 46,  45),   S( 47,  48),   S( 43,  42),   S( 21,  48),

            /* rooks: bucket 6 */
            S(-52,  27),   S(-53,  40),   S(-34,  31),   S(-41,  27),   S(-64,  37),   S(-82,  65),   S(-53,  56),   S(-42,  54),
            S(-34,  34),   S(-24,  33),   S(-22,  32),   S(-44,  26),   S(-52,  48),   S(-71,  65),   S(-66,  59),   S( 16,  16),
            S(-31,  58),   S(-21,  39),   S( -4,  38),   S(-42,  42),   S( -3,  30),   S(-38,  64),   S(-30,  75),   S(  9,  40),
            S(-34,  71),   S( 27,  47),   S( -5,  60),   S(  6,  40),   S(  6,  42),   S(  1,  54),   S(-41,  62),   S(-17,  58),
            S(  4,  72),   S( 38,  60),   S( 55,  46),   S( 33,  44),   S( 20,  60),   S( 39,  50),   S( 39,  47),   S( 12,  57),
            S(  9,  63),   S( 56,  52),   S( 79,  33),   S( 41,  33),   S( 30,  46),   S( 45,  57),   S( 53,  48),   S( 60,  49),
            S( 32,  60),   S( 68,  43),   S( 75,  35),   S( 86,  19),   S( 94,  25),   S( 47,  52),   S( 49,  51),   S( 48,  47),
            S( 53,  72),   S( 26,  65),   S( 31,  50),   S( 39,  41),   S( 63,  47),   S( 52,  63),   S( 56,  61),   S( 19,  66),

            /* rooks: bucket 7 */
            S(-70, -12),   S(-46,  -9),   S(-41, -17),   S(-31,  -9),   S(-25,  -5),   S(-60,  34),   S(-47,  19),   S( -9, -14),
            S(-64,  19),   S(-39,   5),   S(-44,   4),   S(-13, -10),   S(-21,  12),   S(-10,  12),   S(-18,   0),   S(-57,  11),
            S(-80,  49),   S(-37,  17),   S( -9,   7),   S( -2, -11),   S( -6,   5),   S(-21,  -6),   S(-21, -12),   S( 10,   8),
            S(-61,  41),   S( -5,  21),   S(  8,  14),   S( 19,  11),   S( 30,  -2),   S( 30,   5),   S( 34,  -1),   S(-10,   7),
            S(-23,  40),   S( 13,  12),   S( 51, -13),   S( 58,  -8),   S( 72,  -2),   S( 99,   1),   S( 77,   1),   S( 45, -13),
            S(-16,  32),   S( 12,  14),   S( 79, -24),   S( 98, -23),   S( 74,  -7),   S( 71,  17),   S( 70,  17),   S( 25,   3),
            S(-15,  35),   S( 15,  19),   S( 43,   4),   S( 62,   1),   S( 89,  -7),   S( 88,  -7),   S( 37,  26),   S( 11,   9),
            S(  4,  61),   S(-28,  44),   S( 31,   4),   S( 78, -23),   S( 22,   6),   S( 13,  17),   S( 42,   6),   S( 57,  -6),

            /* rooks: bucket 8 */
            S(-45, -45),   S(-11, -10),   S(  3,   4),   S( -1, -14),   S(-12, -41),   S(-10, -55),   S(-16, -25),   S( -5, -17),
            S( -2, -17),   S( -5,  -5),   S(  1, -12),   S(  9, -13),   S( -5, -26),   S( -9, -21),   S( -5, -42),   S(-16, -62),
            S(  7,  17),   S(  9, -16),   S(  5,   5),   S( 11,  10),   S(-13, -32),   S( -3, -32),   S( 13,  21),   S( -1,   0),
            S( -8, -19),   S( -4,  24),   S( -5,   5),   S( 21,   4),   S(  6,  13),   S( -4, -12),   S(  9, -16),   S(  2,   2),
            S( -7, -10),   S(  0,  15),   S( -1,  28),   S( 16,   9),   S(  3,   6),   S( 22,   6),   S( 10,  -9),   S( 12, -33),
            S(  8,  27),   S( -6,   5),   S( 33,  41),   S( 28,  -7),   S(  0,  -6),   S(  7, -13),   S(  4,  -1),   S( 13,  42),
            S(  3, -12),   S( 13, -17),   S( 24,   2),   S( 19, -20),   S( 30,   7),   S( 21, -20),   S( 19, -14),   S( 17,  -6),
            S(  3, -146),  S(  9, -14),   S( 21,   7),   S( -2,  -8),   S(  2,   1),   S(  3, -12),   S(  7,  -7),   S( 22,   0),

            /* rooks: bucket 9 */
            S(-43, -15),   S(-10, -21),   S(-21, -27),   S(-35,  -2),   S(-18,   4),   S( -7,  -2),   S( 13, -42),   S(-37, -30),
            S( 31, -18),   S(  4, -17),   S(-14, -17),   S(-16,  -3),   S(-16, -11),   S( 20,   7),   S(  5, -27),   S(-10, -28),
            S( 12, -16),   S( 20,  -5),   S(  4,   7),   S( -7,   0),   S( -7, -21),   S( 28,  -5),   S( 15,  21),   S( -1,   0),
            S(  4,  10),   S(  9,   5),   S( 13,  20),   S( -1,   5),   S(  7,  18),   S( 25,  -3),   S( 16,  37),   S( 13,   3),
            S( 13,   8),   S(  4,  12),   S(  5,  28),   S( 14,  23),   S( 34,  29),   S( 26,  30),   S( 12,   1),   S( 14,  -6),
            S( 16,  44),   S( -7,  11),   S( 11,   2),   S(-13,   0),   S( 13,   4),   S( 30,   8),   S(  8,  34),   S( 15,  14),
            S( 63,  19),   S( 61,   6),   S( 31,  28),   S( 53,  11),   S( 31,  -8),   S( 30,   6),   S( 38,   1),   S( 44,  26),
            S( 67, -69),   S( 40, -29),   S( 20,  26),   S( 31,  26),   S( 12,  36),   S( 26,  16),   S( 23,  12),   S( 32,  10),

            /* rooks: bucket 10 */
            S(-54, -78),   S(-16, -48),   S(-44, -26),   S(-32,  -4),   S(-34,  -2),   S(-28, -10),   S( 11, -14),   S(-31, -19),
            S( -1, -17),   S( 12, -27),   S(  0, -25),   S( -3, -15),   S(  4, -18),   S( -7,  -3),   S( 35,   4),   S( 10,  -1),
            S(-13, -17),   S(-11, -21),   S(  3, -16),   S( 21,  -5),   S(-16,  18),   S( -1,  -6),   S( 25,  27),   S(  8,  -6),
            S(  6,   1),   S(  8, -12),   S(  1,  -5),   S(  6,  15),   S( 28,  -4),   S(  3,  -6),   S( 25,  25),   S(  1,  -8),
            S(  8,  14),   S( 32,   8),   S( 12,   9),   S( 18, -21),   S( -4,  -5),   S( 14,   8),   S( 30,  31),   S(  9,  28),
            S( 37,  31),   S( 30,  42),   S( 22,  10),   S( 19,   8),   S(  2, -10),   S( 16,   9),   S( 34,  20),   S(  8,  38),
            S( 73,  13),   S( 80,   2),   S( 76,  -4),   S( 70, -17),   S( 55, -17),   S( 37,  12),   S( 25,   6),   S( 31,   7),
            S( 60,  16),   S(  8,  -2),   S( 40,  -1),   S( 21,   7),   S( 35,  -3),   S( 28,  12),   S( 14,   3),   S( 20, -11),

            /* rooks: bucket 11 */
            S(-40, -44),   S(-28, -25),   S(-19, -28),   S(-28, -54),   S(  0, -21),   S( -5,   4),   S(-25, -30),   S(-53, -16),
            S(-14, -28),   S( -6, -43),   S( -1, -28),   S(  0, -28),   S( -3, -23),   S(-16, -17),   S( -2, -31),   S(-20,   2),
            S(  3, -30),   S( 16, -14),   S( 21, -14),   S( 13, -21),   S( 13, -10),   S( -8,   9),   S(-22, -25),   S(-11, -52),
            S(  0,  27),   S( -2, -10),   S( -1,  11),   S( 15,   6),   S(  4,  -5),   S( 13,  30),   S( 28, -10),   S(  2, -24),
            S( 11,  11),   S( 20, -10),   S( 30,   1),   S( 21,  -8),   S( 26,  -5),   S( 32, -10),   S(  9,   7),   S( -1, -11),
            S( 27,  33),   S( 46,   8),   S( 27, -10),   S( 51,  20),   S( 51,  19),   S( 41,   8),   S( -3,   2),   S( 17,  25),
            S( 62,  36),   S( 60,   3),   S( 70, -13),   S( 76, -15),   S( 47,  -9),   S( 50,  12),   S( 34,  34),   S( 55,  -3),
            S( 46,  33),   S( 15,  28),   S( 22,   6),   S( 11,  -7),   S( -7,  -2),   S( 19,  17),   S( 13,   9),   S( 33,   7),

            /* rooks: bucket 12 */
            S( -3,  -8),   S( -8, -29),   S(-12, -52),   S( -4, -10),   S(  1,  -3),   S( -4, -33),   S(-21, -62),   S(-23, -54),
            S(  7,   5),   S( -6, -22),   S(-12, -18),   S( -7, -18),   S(-10,  -6),   S( -7, -15),   S(  1,  -2),   S(-10, -32),
            S(  4,   0),   S( -6, -19),   S( -8, -25),   S(-13,  -7),   S( -5, -22),   S(  6,  -7),   S( -7,  -9),   S(  5,  -8),
            S( -7,  -8),   S(  0, -11),   S(  2,  11),   S(  8, -11),   S(  1,  -8),   S(-10, -38),   S( -7, -12),   S( -4, -38),
            S( -3, -11),   S( -2, -21),   S( 11,   3),   S(  9,   7),   S( -8, -36),   S(  6, -18),   S( -5,  -7),   S(  1, -16),
            S( -3,  -8),   S( -4, -10),   S( 20,  32),   S(  8,  -6),   S( -4,  -6),   S( -7, -21),   S(  1, -26),   S(  5,   7),
            S( -5,  -4),   S(  1, -30),   S(  2, -42),   S( 11,  -1),   S(  8,  -3),   S( -5, -39),   S( -3,  -9),   S( 10, -17),
            S( -5, -43),   S(  7,  22),   S(  3, -21),   S(  1,   1),   S( -4, -26),   S(-11, -49),   S(-14, -30),   S(  8,  -4),

            /* rooks: bucket 13 */
            S(-13, -40),   S( -6, -24),   S( -4, -18),   S(  1,  11),   S(  6,  -4),   S(-12, -37),   S(  2, -22),   S(-18, -31),
            S( -2, -31),   S( -2, -14),   S(-11,  -7),   S( -7,  -2),   S(-10, -18),   S( -1, -12),   S(  5,   1),   S( -4, -21),
            S( -5, -29),   S( -7, -27),   S( -4, -35),   S( -2, -23),   S( 10,  13),   S(  1,  -5),   S(  1, -21),   S(  1, -32),
            S( -6, -51),   S(  3,  -5),   S( -9, -42),   S( -5,  -9),   S( 13,  13),   S( -7, -36),   S( -2, -27),   S(  3, -17),
            S( 12, -20),   S(  8, -19),   S( 16,  24),   S( -5,  -9),   S( -9, -28),   S(  4, -13),   S( -6, -39),   S( 10,  -7),
            S( -7, -40),   S( 10, -27),   S( -8, -13),   S( 14,  -8),   S(  5, -12),   S( 10,  16),   S(  8,  -5),   S(  5,   8),
            S(  5,  -4),   S(  9,  19),   S(  9,   8),   S(  2, -16),   S( 10, -27),   S( 21,   6),   S(  3, -13),   S(  3, -17),
            S(-14, -121),  S(-17, -69),   S(  5,   5),   S(  1,  -1),   S( -4,  13),   S( -3, -30),   S(-11, -27),   S(  5,   0),

            /* rooks: bucket 14 */
            S( -7, -30),   S(-15, -48),   S( -2,  -6),   S( -2, -34),   S(  2, -23),   S( -9, -22),   S( 10,  -8),   S( -6, -22),
            S(-22, -44),   S(-13, -54),   S( -9,   5),   S(-13, -38),   S(-11, -16),   S(  1, -32),   S(  7,  25),   S(  5, -10),
            S( -3, -23),   S( -8, -20),   S( -4, -17),   S( -6, -13),   S(-13, -25),   S( -7, -22),   S(  7,  22),   S( -1, -27),
            S( 12,   6),   S( -8, -32),   S( -3, -19),   S( -5,   7),   S(  3, -11),   S(  4, -13),   S( -4, -34),   S( -3, -22),
            S(  1, -13),   S(  3, -25),   S( -7, -28),   S( -9, -24),   S( -6, -17),   S( -5, -19),   S(  2,   8),   S(  8,   4),
            S(  3, -15),   S(  0, -24),   S(  1, -18),   S(  1, -20),   S(-11, -19),   S( -9,   4),   S(  5,   9),   S(  0,  -5),
            S( 19,  -1),   S(  1, -36),   S(  3, -20),   S(  2, -29),   S(  5, -45),   S(  6,   0),   S(  8,  10),   S(  9,   7),
            S( -2, -23),   S(  3, -16),   S( -9, -28),   S( 10,  11),   S(-10, -19),   S(  3,   8),   S(  4,  16),   S( -1, -15),

            /* rooks: bucket 15 */
            S( -2, -53),   S(-13, -41),   S( -1, -27),   S( -7, -28),   S(  0, -16),   S( -4,  -9),   S(-17, -53),   S( -9, -13),
            S(-14, -20),   S(-13, -27),   S(  2,  -1),   S( -7, -24),   S(-10, -29),   S(  6, -27),   S(-11, -41),   S(  7,   5),
            S( -8, -23),   S(-10, -23),   S( -3, -24),   S(  2,   1),   S(  9, -27),   S( -4,  -9),   S( -3,   4),   S( -4, -14),
            S(  2, -31),   S( -4, -25),   S(-11, -17),   S( -5, -17),   S(-11, -19),   S(  2, -19),   S(  0, -18),   S( -9,  -2),
            S(  0, -10),   S( -5, -12),   S( 11,  -4),   S( -1, -11),   S(  1,  -1),   S(  2,  -2),   S( -2,   7),   S(  0,  17),
            S(  7,  17),   S(  2,   0),   S(  1, -14),   S(  0, -11),   S( -6,  -9),   S(  1,  13),   S(  5, -10),   S( -8, -14),
            S( 10,  19),   S( 11,  -6),   S(  8, -32),   S( -4, -34),   S(  1, -21),   S( 12,  34),   S(  2,  -4),   S(  0,  11),
            S(  1, -18),   S( -7, -18),   S(  2,  -6),   S(  1, -12),   S( -7, -15),   S( -1, -26),   S(  0, -18),   S(  1,  -4),

            /* queens: bucket 0 */
            S(-21, -12),   S(-22, -57),   S( 47, -89),   S( 56, -57),   S( 32, -35),   S( 19,  -1),   S( 56,   8),   S( 22,  19),
            S(-13, -14),   S( 32, -65),   S( 40, -17),   S( 21,   7),   S( 24,  30),   S( 25,  20),   S(  9,  62),   S( 36,  22),
            S( 25,   3),   S( 40,  15),   S( 21,  27),   S( 19,  35),   S( 20,  18),   S( 13,  17),   S( 10,  31),   S( 36,  33),
            S( 20,  19),   S( 25,  45),   S(  7,  46),   S(  5,  49),   S(  8,  58),   S( 14,  34),   S( 17,  28),   S( 20,  30),
            S( 40,  51),   S( 30,  43),   S( 19,  41),   S( 20,  56),   S( -5,  28),   S( -6,  13),   S( 33,  22),   S( 45,  -3),
            S( 27,  59),   S( 24,  54),   S( 14,  37),   S( 20,  16),   S( 46,  -9),   S(  5,  36),   S( 28,  21),   S( 25, -21),
            S( 47,  49),   S( 52,  43),   S( 32,  36),   S( 50,  27),   S( 23,   7),   S( -6,  -9),   S( 32,  24),   S( 31,  11),
            S( 45,  28),   S( 23,  36),   S( 43,  17),   S( 35,  36),   S( 46,  30),   S(-13,   3),   S( 48,  27),   S( 46,  27),

            /* queens: bucket 1 */
            S( -4, -17),   S(-77, -26),   S(-55, -29),   S(-17, -68),   S(-11, -25),   S(-19, -46),   S( 14, -30),   S( 11,  26),
            S(-19, -29),   S(-12, -46),   S(  9, -51),   S( -6,   3),   S( -8,   0),   S(  5,  -3),   S( 20, -38),   S( -1,  21),
            S(-30,  44),   S( -1,  -4),   S(  4,  11),   S( -6,   7),   S( -5,  32),   S(-14,  31),   S( 15,  12),   S( 19,  21),
            S(  8, -17),   S(-11,  31),   S(-15,  35),   S(  3,  47),   S(-10,  51),   S(  2,  29),   S(  2,  -1),   S( 18,  17),
            S( 15,   8),   S(  6,  25),   S( -2,  62),   S(-22,  63),   S(-16,  52),   S(  1,  14),   S( -8,  16),   S(  2,  35),
            S( 10,  26),   S( 13,  53),   S( 15,  59),   S(-37,  57),   S(-16,  47),   S(-33,  45),   S( 25,  24),   S( 19,  39),
            S(  2,  36),   S(-12,  70),   S(-19,  33),   S(-24,  69),   S(-25,  48),   S( 14,  28),   S( -6,  39),   S(-26,  45),
            S( -4,   7),   S(  6,  17),   S( 15,  27),   S( -9,  11),   S( -2,  15),   S(  6,  13),   S( 10,  26),   S( -8,  29),

            /* queens: bucket 2 */
            S(  8,  17),   S( 14, -36),   S(  8, -23),   S( -3, -17),   S(-25,   4),   S(-27, -17),   S(-29, -25),   S( 12,   8),
            S( 16,  10),   S( 11,  36),   S( 17, -12),   S( 17, -21),   S( 12, -27),   S( 14, -50),   S(  9,  -8),   S( 32, -29),
            S( 17,  11),   S( 17,  12),   S(  3,  45),   S(  8,  35),   S(  1,  57),   S( 13,  48),   S(  8,  23),   S( 29,  14),
            S(  6,  26),   S(  0,  54),   S( -3,  44),   S(  2,  56),   S(-21,  81),   S( -4,  85),   S( 12,  20),   S(  3,  68),
            S( 14,   9),   S( -8,  58),   S( -8,  56),   S(-30,  93),   S(-37, 108),   S(-14,  77),   S( -8, 103),   S( -6, 105),
            S( 10,  24),   S(  0,  44),   S(-31,  80),   S( -7,  51),   S(-28,  90),   S(-14,  97),   S( -4,  95),   S( 10,  73),
            S(-23,  53),   S(-36,  78),   S(-14,  61),   S(  6,  61),   S(-19,  74),   S( 25,  42),   S(-20,  47),   S(-13,  77),
            S(-68,  77),   S(  2,  37),   S( 29,  39),   S( 29,  33),   S(  1,  64),   S( 19,  32),   S( 14,  26),   S(-16,  41),

            /* queens: bucket 3 */
            S( 85,  88),   S( 59,  91),   S( 50, 100),   S( 44,  82),   S( 69,  29),   S( 46,  19),   S( 19,  19),   S( 40,  54),
            S( 68, 113),   S( 62, 108),   S( 48, 113),   S( 49,  88),   S( 50,  77),   S( 63,  45),   S( 67,   4),   S( 40,  42),
            S( 66,  87),   S( 56, 107),   S( 58,  82),   S( 56,  76),   S( 52,  88),   S( 55,  96),   S( 62,  98),   S( 66,  70),
            S( 49, 124),   S( 63,  84),   S( 49,  96),   S( 41,  96),   S( 40,  95),   S( 39, 129),   S( 59, 102),   S( 51, 134),
            S( 65,  92),   S( 60, 104),   S( 56,  85),   S( 39,  96),   S( 34, 115),   S( 29, 123),   S( 41, 164),   S( 53, 155),
            S( 50, 120),   S( 59,  99),   S( 52,  94),   S( 29, 114),   S( 33, 132),   S( 73, 100),   S( 66, 134),   S( 37, 184),
            S( 62, 113),   S( 63,  99),   S( 73,  82),   S( 62,  93),   S( 36, 108),   S( 62, 107),   S( 93, 123),   S(157,  69),
            S( 76,  88),   S(101,  76),   S( 75,  85),   S( 78,  81),   S( 38, 107),   S(107,  52),   S(134,  56),   S(140,  56),

            /* queens: bucket 4 */
            S(-13, -24),   S(-19, -20),   S(-25,  -8),   S( -9,  -7),   S( 11, -14),   S( 35,   1),   S(-33,  -9),   S(-24,  -1),
            S(-30, -18),   S(-29,  -6),   S( 13,  -9),   S(-39,  23),   S(  3,  -4),   S(  1, -13),   S( -8, -10),   S(-35, -15),
            S(  2,   1),   S(  9,  -3),   S(  1,  28),   S( -1,  31),   S( 25,  16),   S(  6,  -7),   S(  8, -18),   S(-24, -22),
            S(-16,   2),   S( -7,  14),   S(  2,  35),   S( -5,  30),   S( 15,  35),   S( 21,  20),   S(  2, -14),   S( -2,  -6),
            S( -8,  -2),   S( 16,  12),   S( 16,  28),   S( 29,  41),   S( 23,  28),   S( 21,   1),   S(-20, -16),   S( -9, -28),
            S(  3,  12),   S( 35,  13),   S( 25,  54),   S( 24,  45),   S( 11,   8),   S(  3,   4),   S(-14, -13),   S(-11,  -6),
            S(-12, -20),   S( -6,  18),   S(  2,  25),   S( 31,  34),   S( 10,  11),   S(-12,  -3),   S(-21, -41),   S(-20, -25),
            S( -4, -17),   S( -2,  -3),   S( 29,  36),   S(  5,  20),   S(-17, -16),   S( -7, -10),   S(-20, -35),   S( -9, -19),

            /* queens: bucket 5 */
            S(-37, -14),   S(-26, -30),   S(-31, -29),   S(-45, -28),   S(-56, -29),   S(  9, -15),   S( -7,  -4),   S( -3,  -5),
            S(-28,  -4),   S(-40, -13),   S(-66, -20),   S(-66,  -3),   S(-15,  -3),   S(-41, -16),   S(-47, -15),   S(-50, -15),
            S(-34,   3),   S(-59, -13),   S(-65,   4),   S(-34,  31),   S( 17,  51),   S( -9,  22),   S( -3,   0),   S( 13,  22),
            S(-53, -10),   S(-50,  -4),   S(  0,  36),   S( -5,  51),   S( 12,  27),   S( -3,  13),   S( -3,  -7),   S( -7,  15),
            S(-32,  -5),   S(-24,  18),   S(-11,  47),   S( -6,  45),   S( 28,  47),   S(  1,  17),   S(  1,  10),   S(-29, -28),
            S(-16,  16),   S(  8,  37),   S(-12,  42),   S(  2,  45),   S( 40,  49),   S(  4,  14),   S(  2,   2),   S(-11,  -9),
            S( -9,   8),   S( -9,  14),   S(  6,  60),   S( -2,  34),   S(  0,  38),   S( 22,  34),   S( 11,   9),   S(-19, -15),
            S(  8,  26),   S( 12,  12),   S(  3,  19),   S( 12,  49),   S( 16,  29),   S(  5,  21),   S( -1, -24),   S(-18, -15),

            /* queens: bucket 6 */
            S(-28,   7),   S(-49, -21),   S(-66, -27),   S(-82, -59),   S(-92, -51),   S(-71, -46),   S(-52, -43),   S(-27,   3),
            S(-62, -12),   S(-44,   0),   S(-51,  13),   S(-64,  13),   S(-78,  17),   S(-88,   0),   S(-85, -19),   S(  7,  19),
            S(-42,  11),   S(-18,  11),   S(-53,  39),   S(-97,  86),   S(-41,  51),   S(-37,   2),   S(-47, -15),   S( -1,   5),
            S(-39,  12),   S(-23,  11),   S(-27,  63),   S(-48,  67),   S(  4,  44),   S( 15,  49),   S(-12,  34),   S( 10,  -8),
            S(-52,  21),   S( -3,  38),   S(-28,  53),   S(  9,  29),   S( 31,  53),   S( 60,  36),   S( 25,  31),   S( -5,  19),
            S(-23,  42),   S(-10,  20),   S( 24,  22),   S( 20,  46),   S(  8,  51),   S( 62,  67),   S( -6,  -7),   S(-15,  11),
            S( -7,   6),   S(  3,   3),   S(-11,  42),   S(-12,  36),   S( 28,  50),   S( 19,  60),   S(-10,  22),   S(-37,  -1),
            S( -2,   6),   S( 18,  12),   S( 13,  31),   S( -4,  24),   S( 30,  39),   S( 19,  27),   S( -1,  16),   S(  3,   9),

            /* queens: bucket 7 */
            S( -5, -10),   S(-33,  15),   S(-49,  24),   S(-33,  13),   S(-30,  -9),   S(-34, -25),   S(-32,  -8),   S(-18, -12),
            S(-32,  -8),   S(-48,   6),   S(-23,   8),   S(-22,  37),   S(-28,  30),   S(-45,  37),   S(-45,  24),   S(-36, -14),
            S(-33, -21),   S(-46,  29),   S(-16,  31),   S( -9,  29),   S( 10,  17),   S(  0,  25),   S(-16,  12),   S(-20,  -2),
            S(-58,   1),   S(  9,   3),   S(-17,  25),   S( -3,  38),   S( 34,  17),   S( 32,  22),   S( 11,  32),   S( -4,  16),
            S(-27,  21),   S(-50,  27),   S( 11,  18),   S( 50,  -9),   S( 63, -12),   S( 84, -18),   S( 36,   9),   S( 37,  -9),
            S(-14,  12),   S(-16,   9),   S(  6,   0),   S( 16, -10),   S( 36,  35),   S( 79,  19),   S( 63,   2),   S( 41,   9),
            S(  9, -19),   S(  3,  11),   S(  1,  -5),   S(  5,  14),   S( 35,  18),   S( 53,  38),   S( 52,  18),   S( 48,  24),
            S( 15,   4),   S( 19,   3),   S( 21,   8),   S( 19,  16),   S( 39,  26),   S( 22,  19),   S( 14,   4),   S( 36,  43),

            /* queens: bucket 8 */
            S( -5, -10),   S( -1,   4),   S(-13,  -4),   S( -8,  -5),   S( -4,   0),   S( -1, -15),   S(-19, -24),   S( -3,   6),
            S( -7,   0),   S(-11, -15),   S( -3,   6),   S(-12,  -2),   S( -4,  -3),   S(-16, -19),   S(-18, -38),   S( -3,  -8),
            S( -2,  -1),   S( -6,   2),   S( -7,   2),   S( -4,  -9),   S( -4,   5),   S(-10, -11),   S(-11, -26),   S(-14, -26),
            S( -3,   3),   S( 10,  19),   S( 11,  19),   S(  5,  11),   S( -2,   0),   S( -6,   0),   S( -1,  -3),   S( -6, -20),
            S( 16,  28),   S(  2,  27),   S( 10,  13),   S( 10,  17),   S( 13,  32),   S(  4,   1),   S( -7,  -8),   S(-10, -17),
            S(  8,  19),   S( 11,  20),   S(-19,  14),   S( 14,  34),   S( -8, -14),   S( -5, -10),   S(  4,   2),   S(  3,  13),
            S( -7, -13),   S(-18, -26),   S( 21,  34),   S( 13,  15),   S(  2,  18),   S(  3,  18),   S( -2,  -7),   S( -6, -15),
            S(-14, -28),   S( 12,   9),   S(-16, -48),   S( -9,  -5),   S(-11, -29),   S( -1,  -6),   S( -2, -16),   S( -5,  -6),

            /* queens: bucket 9 */
            S(  5,   7),   S(-13, -26),   S(  2,  -1),   S(-29, -31),   S(-22, -37),   S(-17, -30),   S(-12, -21),   S(-12, -18),
            S( -3,  -4),   S( -8,  -6),   S(-18, -22),   S( -3,   0),   S(-16,  -7),   S(-15, -19),   S(  2,  -1),   S( -3,  -8),
            S(  5,   6),   S(  4,   9),   S( -8,  20),   S( -4,  -6),   S( -6,   7),   S(  1,  -2),   S(  4,   3),   S(  4,   1),
            S( -5,  -9),   S( -5,   4),   S( 14,  41),   S(  8,  22),   S( 19,  31),   S(  4,  11),   S( -8, -16),   S(  1,  -9),
            S(  5,  10),   S(  8,  31),   S( 12,  33),   S( 17,  50),   S( 20,  33),   S( 10,  19),   S( -3,   5),   S(-11, -11),
            S(-18, -19),   S(-16,  -4),   S(  5,  22),   S( 15,  35),   S( -5,   2),   S( -1,  10),   S( -9,  -6),   S( -5,  -8),
            S( -6, -17),   S(-10, -25),   S( -9,  22),   S( 10,  29),   S( 16,  20),   S(  6,  -6),   S(  6,  -4),   S(-11, -24),
            S( -1,  -1),   S( -4, -22),   S( 11,  -3),   S(  0,  15),   S( 13,   1),   S( -3,  -1),   S( 11,   3),   S(  3, -15),

            /* queens: bucket 10 */
            S(  3,   0),   S( -3,   2),   S(-10, -17),   S(-21, -24),   S(-11, -14),   S( -6,  -5),   S(  2, -10),   S( -4,  -9),
            S( -7, -11),   S( -8, -15),   S(-14, -23),   S( -8, -12),   S( -4,  -6),   S(-18, -12),   S(  1,  -8),   S(-16, -17),
            S( -1, -12),   S( -9, -14),   S( -7,  -8),   S( -2,   2),   S( -7,   1),   S( -7,   4),   S(  2,   2),   S(  3,   7),
            S(  0,  -2),   S(  2,  -3),   S( -2,  -7),   S(  1,  31),   S( 15,  25),   S( -6,   5),   S( -3,  -6),   S(-13, -18),
            S( -5,  -7),   S(  6,  -6),   S( -5,   4),   S( 21,  47),   S(  1,  -1),   S( 17,  28),   S( 12,  13),   S(  0,   6),
            S( -3,  -5),   S(-20, -32),   S( -4,   0),   S(  1,  12),   S(  5,  16),   S(  5,  21),   S( 11,   7),   S( -5, -11),
            S( -6,  -7),   S(-17, -27),   S(  8,  22),   S( -7,  -8),   S(  6,   5),   S(  3,   6),   S( -3,  -8),   S( -9,  -6),
            S(  6,   0),   S( -2, -17),   S(  6,  -3),   S(  7,  -7),   S( 16,  14),   S(  4,   6),   S( 16,  14),   S(  2,  -8),

            /* queens: bucket 11 */
            S(-10, -14),   S( -7, -19),   S(-21, -19),   S(-10, -27),   S(-12, -18),   S( -9, -11),   S( -5,  -6),   S(-12, -22),
            S(-16, -32),   S( -8,  -6),   S(-40, -34),   S(-10,  -8),   S(-12,  -9),   S( -9,  -6),   S( -5,  -9),   S( -6,  -3),
            S(-17, -22),   S(-16, -34),   S(  3, -20),   S( -9, -16),   S( -8, -13),   S( -3,   5),   S(  7,  19),   S(-12,  -7),
            S(-16, -28),   S(-24, -24),   S( -7, -25),   S( 14,  26),   S( 11,   0),   S(-11,  -6),   S( 23,  24),   S( -2,   1),
            S(-14, -13),   S( -4, -16),   S(-21, -24),   S( 25,  21),   S( 15,  14),   S( 27,  50),   S( 20,  40),   S(  2,  11),
            S(-14, -30),   S(  3,   3),   S(-16, -17),   S( 15,  12),   S( 24,   5),   S( 45,  35),   S(  8,  -3),   S( -8,  -7),
            S( -8,  -3),   S(-14, -22),   S(  9,  16),   S(-13,  -4),   S(  5,   6),   S( 22,  23),   S( 36,  37),   S( -3, -17),
            S(-11, -21),   S( -9, -23),   S( -7, -20),   S(  5, -13),   S(  2,  10),   S( -3, -10),   S( 18,   6),   S( -2, -31),

            /* queens: bucket 12 */
            S(  6,   0),   S( -1,  -1),   S(  2,   1),   S( -8,  -5),   S( -9, -12),   S( -1,  -3),   S(  0,  -2),   S( -4, -10),
            S( -3,  -2),   S( -8, -14),   S( -9, -11),   S( -5, -10),   S( -2,  -2),   S( -6,  -2),   S( -1,  -9),   S( -5,  -9),
            S( -2,  -5),   S( -6, -10),   S( 11,  13),   S( -5,  -4),   S( -2,  -5),   S( -8, -13),   S(-12, -24),   S( -8,  -7),
            S(  2,   7),   S( -1,   2),   S(  4,   6),   S(  1,   7),   S(  8,  14),   S(  0,  -3),   S(  0,  -4),   S( -4, -11),
            S(  1,  -4),   S( 11,  12),   S( 32,  56),   S(  1,  15),   S( -5,   7),   S(  1,   7),   S(-13, -30),   S( -2, -14),
            S(  8,  17),   S( 13,  24),   S( 33,  42),   S( -2,   7),   S( -1,   5),   S(  2,   2),   S(  5,   5),   S( -5, -15),
            S(  3,   1),   S(  2,   5),   S( 17,  16),   S( 11,   8),   S(  5,   9),   S( -4,   4),   S(  9,   6),   S( -4,  -4),
            S( -4, -28),   S(-10, -27),   S(-12, -21),   S(-10, -29),   S( 10,  -7),   S(  1,  -1),   S(  2,  -5),   S( -6, -11),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -4, -14),   S(  1,  -4),   S( -2,  -7),   S( -3, -10),   S( -2,  -2),   S( -7,  -9),   S( -6,  -8),
            S(  4,  10),   S(  5,  14),   S(  4,  11),   S( -4,  -3),   S( -6,  -6),   S(  2,  11),   S(  1,   6),   S(-11, -19),
            S( -2,  -7),   S(  0,   0),   S(  3,  16),   S(  3,  12),   S( -2,  -1),   S( -5,  -8),   S( -4, -11),   S(-12, -16),
            S( -3,  -4),   S(  2,   3),   S( 12,  13),   S( 19,  28),   S( 15,  32),   S( -4,  -7),   S( -5, -13),   S( -5,  -6),
            S( -3,  -5),   S(  6,  19),   S( 15,  41),   S( 12,  37),   S( 22,  42),   S(  0,  -8),   S( -4,  -6),   S( -7, -14),
            S(  0,   0),   S( 12,  32),   S( 38,  74),   S( 18,  40),   S(  0,  15),   S(  1,   7),   S(  6,  15),   S( -5, -14),
            S( -1,   0),   S( 19,  32),   S(  9,  27),   S( 13,  25),   S( -2,  10),   S(  1,  -8),   S( -1,  -9),   S(  5,   7),
            S(-12, -17),   S(  4,  -3),   S( -2,  -8),   S( -9, -12),   S(  6,   1),   S(  5,   7),   S( -8,  -7),   S( -6, -13),

            /* queens: bucket 14 */
            S( -1,  -2),   S(  0,   2),   S( -1,  -7),   S( -9,  -8),   S(  4,   7),   S( -2,  -4),   S( -1,  -8),   S( -4, -10),
            S( -5,  -7),   S(  6,  17),   S( -1,  -3),   S(  0,  -6),   S( -9, -11),   S( -7, -14),   S( -5,  -4),   S( -3,  -7),
            S( -1,  -2),   S( -9, -12),   S( -5, -12),   S(  0,  -1),   S(  2,   1),   S(  2,  -4),   S(  3,   6),   S( -6, -14),
            S( -8,  -8),   S(  8,  10),   S( -5,  -3),   S( 22,  40),   S( 14,  15),   S( -1,   6),   S( 11,  25),   S(  1,  -3),
            S(  4,  13),   S(  5,   1),   S(-13,  -8),   S( 16,  26),   S( 13,  33),   S( 17,  24),   S(  9,  18),   S( -4, -10),
            S( -2,  -5),   S(  5,  15),   S( 14,  24),   S( 12,  20),   S( 17,  41),   S( 13,  44),   S(  7,  16),   S( -3,  -9),
            S(  3,   7),   S(  8,  10),   S( 16,  36),   S( 19,  33),   S( 15,  33),   S( 13,  24),   S( 16,  27),   S(  1,   5),
            S( -3,  -1),   S(  0,   1),   S(-10, -14),   S( 12,  18),   S(  0,   3),   S(  2,  -1),   S(  1,   6),   S(-10, -17),

            /* queens: bucket 15 */
            S( -1,  -2),   S(  1,  -5),   S( -5,  -8),   S( -2, -10),   S( -5, -10),   S( -5, -12),   S(-11, -24),   S(  0,  -7),
            S( -1,  -3),   S( -4,  -8),   S( -5, -12),   S( -4, -11),   S(  1,   9),   S( -3,  -7),   S( 11,  14),   S(  3,   1),
            S(  0,  -8),   S( -3, -11),   S(  0,  -1),   S( -4, -11),   S( -3, -11),   S(  6,  17),   S( -1,  -4),   S(  0,  -8),
            S( -5,  -8),   S(  4,   5),   S( -3,  -3),   S(  4,   2),   S(  2,  10),   S(  0,   6),   S(  5,   5),   S(  4,   3),
            S( -2,  -7),   S( -2,  -5),   S( -8, -12),   S( -4,  -5),   S(  5,  11),   S(  7,   5),   S( -4,  -7),   S(  0,  -8),
            S( -3,  -7),   S( -2,  -6),   S( -1,   2),   S(  0,   0),   S( -2,  -6),   S( 19,  29),   S(  3,  -2),   S(  0,  -9),
            S( -6, -13),   S(  4,  -6),   S(  6,   8),   S(  7,   7),   S(  6,   8),   S( 22,  37),   S( 11,  19),   S(  4,   5),
            S(  1,  -4),   S( -5,  -6),   S( -2,  -4),   S( 10,  12),   S(  7,   2),   S(  3,  -5),   S( -3,  -8),   S( -7, -22),

            /* kings: bucket 0 */
            S( 61,   4),   S( 44,  54),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 35,  40),   S(107,  65),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 45,  21),   S( -6,  38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 77,  51),   S( 55,  63),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-11,  42),   S( -1,  29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 44,  67),   S( 52,  49),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -6,  52),   S(-28,  19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 16,  86),   S(-41,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 20, -58),   S( 77, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-36,  -9),   S( 22,  18),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 44, -12),   S( 24,   2),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 15,  34),   S(  0,  31),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 49,  19),   S( 18,  12),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 11,  51),   S(  1,  46),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 68,  29),   S( 21, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 38,  64),   S( -7,  25),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52, -123),  S( 12, -62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -108),  S(-94, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  6, -51),   S(-37, -43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-37, -33),   S(-47, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-18, -36),   S(-17, -38),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-40, -22),   S(-86,   3),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-18, -42),   S(-36, -110),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-83, -11),   S( -8, -93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -112),  S(-74, -29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -229),  S(-13, -101),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-64, -60),   S( 22, -66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-53, -78),   S(-21, -102),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-16, -54),   S(-110, -20),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 23, -117),  S(-63, -71),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-125,  -2),  S(-29, -117),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-48, -74),   S( -2, -229),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -6, -23),   S(-26,  14),   S(  9,  -1),   S( -9,  26),   S( 20,   1),   S( 43,   7),   S( 49,  -8),   S( 47,   3),
            S(-10, -28),   S(-31,   3),   S( -2, -10),   S( -1, -11),   S( 15,   4),   S(  1,  14),   S( 26,   0),   S( 20,  26),
            S(  3, -27),   S( -3, -20),   S( 29, -32),   S(  8, -15),   S( 16,  -7),   S(  4,  29),   S( -9,  48),   S( 26,  24),
            S(  9, -18),   S( 29,   2),   S( 51, -27),   S( 35,  -5),   S( 14,  46),   S(-19,  86),   S(  3,  87),   S( 52,  66),
            S( 91, -54),   S(125, -18),   S( 89, -22),   S( 44,  16),   S( 44, 137),   S( -3, 138),   S( 12, 155),   S( 64, 134),
            S(-220, -73),  S(-121, -134), S( 14, -168),  S( 35,  43),   S( 85, 197),   S( 67, 187),   S(109, 167),   S( 98, 147),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-39,  20),   S(-42,  25),   S(-18,  11),   S(-37,  56),   S(-13,   2),   S( 15,   9),   S( 16,   0),   S( 13,  28),
            S(-52,  17),   S(-49,  20),   S(-32,  10),   S(-20,   9),   S(  1,   8),   S(-15,  11),   S( -7,   3),   S(-17,  23),
            S(-48,  24),   S(-21,  21),   S(-27,   7),   S(  4,  -6),   S( -2,  20),   S(-27,  21),   S(-35,  32),   S(-18,  30),
            S(-37,  43),   S(  9,  25),   S(-18,  25),   S( 12,  27),   S(  2,  29),   S(-35,  45),   S( -4,  40),   S( 23,  57),
            S(  5,  36),   S( 63,  -2),   S( 95, -24),   S( 87, -20),   S( 35,  30),   S(  5,  35),   S(-28,  81),   S( 37,  93),
            S( 46,  43),   S(-33, -20),   S( -7, -101),  S(-11, -96),   S(-39, -67),   S( -4,  48),   S( 48, 185),   S( 66, 214),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42,  42),   S(-29,  24),   S(-20,  14),   S(  0,   6),   S(-25,  33),   S(-11,  12),   S(  4,  -9),   S( -7,  23),
            S(-51,  33),   S(-40,  29),   S(-30,   9),   S(-28,  19),   S(-24,  19),   S(-33,   8),   S(-15,  -9),   S(-38,  14),
            S(-48,  50),   S(-40,  52),   S(-15,  18),   S(-17,  21),   S(-20,  22),   S(-27,   6),   S(-32,   9),   S(-34,  13),
            S(-34,  90),   S(-40,  74),   S(-15,  43),   S(  1,  36),   S( -7,  35),   S(-23,  18),   S(  4,  19),   S( 19,  14),
            S(-29, 133),   S(-44, 118),   S( -2,  24),   S( 25, -23),   S( 97,  -8),   S( 91,  -6),   S( 75, -16),   S( 52,   4),
            S(-10, 248),   S( 37, 176),   S( 14,  72),   S( 27, -90),   S(-17, -169),  S(-77, -131),  S(-31, -64),   S( 16,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  4,  15),   S(  0,  16),   S(  9,  12),   S(  2,  32),   S( -4,  49),   S( 32,  22),   S( 21,  -3),   S(  8, -12),
            S( -2,  18),   S( -2,  26),   S( -1,  10),   S( -2,  10),   S( 10,  17),   S( 15,   1),   S(  7, -10),   S(-19,  -4),
            S(  0,  37),   S(-12,  57),   S(  6,  20),   S(  5,   2),   S( 23, -10),   S( 13, -11),   S( -1, -19),   S(-16, -10),
            S( -1,  92),   S(-18, 103),   S(  8,  65),   S( 17,  31),   S( 23,   2),   S( 31, -23),   S( 20,   5),   S( 32, -18),
            S(  0, 157),   S(-14, 167),   S(-25, 167),   S( -7, 113),   S( 35,  53),   S( 84, -13),   S(111, -33),   S( 96, -38),
            S(103, 127),   S( 45, 240),   S( 24, 254),   S(  8, 208),   S(-25,  95),   S( 26, -175),  S(-67, -239),  S(-158, -178),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 71,   0),   S( 25,   5),   S(  2, -10),   S(-13,  -9),   S(  4, -13),   S(  7, -11),   S(  1, -10),   S(-57,  43),
            S( 44,  -4),   S( 11,  19),   S(  7,  -3),   S(-11,  -6),   S(-23, -21),   S(-15, -16),   S(-31, -20),   S(-44,   5),
            S( 65, -15),   S(107, -29),   S( 32, -17),   S(-29,  -3),   S(-72,  11),   S(-13,   4),   S(-76,  23),   S(-65,  33),
            S(-84, -73),   S(-10, -92),   S( 72, -60),   S(-26,   3),   S(-25,  17),   S(-61,  64),   S(-39,  56),   S(-51,  77),
            S(-30, -74),   S(-63, -112),  S( -6, -91),   S( 56,   6),   S( 76,  88),   S( -3, 100),   S( 16,  76),   S(  0, 101),
            S(  3, -61),   S(-16, -77),   S(  0, -66),   S(  2,  48),   S( 57,  87),   S( 65, 150),   S( 43, 154),   S( 58, 125),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  46),   S(-41,  43),   S( -2,  22),   S( 56,   7),   S( 63,  -1),   S( 12,   3),   S(-21,  11),   S(-55,  49),
            S(-74,  39),   S(-36,  40),   S(-18,  24),   S( -1,  22),   S(-19,  23),   S(-26,   8),   S(-55,   7),   S(-75,  35),
            S(-36,  29),   S(-35,  54),   S( 26,  28),   S(  6,  41),   S(-29,  43),   S(-64,  32),   S(-68,  34),   S(-65,  45),
            S(-27,  38),   S(-13,  10),   S(-25, -38),   S(  7, -26),   S( -4,  -7),   S(-49,  30),   S(-11,  30),   S(-30,  56),
            S( 59,   8),   S( -7, -34),   S( 28, -93),   S(  6, -71),   S( 50, -41),   S( 22,  21),   S(-16,  69),   S(-39, 117),
            S( 48,  32),   S( 18, -13),   S(-29, -66),   S(-18, -60),   S(-30, -58),   S( 47,  40),   S( 63, 136),   S( 40, 149),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  42),   S(-55,  19),   S(-15,   4),   S( 15,   3),   S( 10,  25),   S( 21,  11),   S( 14,   6),   S(  3,  27),
            S(-76,  25),   S(-58,  16),   S(-46,   9),   S( 17,  12),   S(-11,  27),   S( -8,  12),   S(-16,  12),   S( -9,  13),
            S(-64,  36),   S(-76,  43),   S(-50,  30),   S(-40,  43),   S(  0,  41),   S( 11,  19),   S(  0,  22),   S(-16,  19),
            S(-90,  89),   S(-59,  58),   S(-27,  31),   S(-15,  15),   S( -6, -33),   S(-14, -29),   S(-25,   6),   S( 28,   0),
            S(-13, 103),   S(-45,  72),   S( 30,  10),   S( -6, -33),   S(  6, -72),   S(-38, -66),   S( -8, -32),   S( 82,  -5),
            S( 77,  77),   S( 69,  90),   S( 47,  24),   S( 39, -79),   S( -5, -103),  S(-39, -53),   S( -7, -47),   S( 77,   1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-52,   4),   S(-38, -14),   S(  0, -23),   S(-61,  46),   S( 24,   5),   S( 70, -20),   S( 58, -26),   S( 70, -10),
            S(-59,   4),   S(-62,   5),   S(-34, -19),   S(-35,   4),   S(  2,  -1),   S( 46, -29),   S( 28, -13),   S( 52, -16),
            S(-60,  26),   S(-80,  40),   S(-42,   7),   S(-45,   3),   S( -3,  -2),   S( 20, -15),   S( 53, -13),   S( 52, -17),
            S(-57,  62),   S(-91,  80),   S(-55,  59),   S(-34,  35),   S(-15,  -2),   S( 39, -59),   S( 20, -71),   S( 24, -107),
            S( 15,  62),   S(-63, 135),   S( -2, 117),   S( -8,  86),   S( 12,  20),   S( 21, -82),   S(-44, -132),  S(-17, -97),
            S(129,  84),   S( 81, 123),   S( 91, 105),   S( 60,  94),   S( 33,   3),   S(  4, -103),  S(-28, -91),   S( -9, -181),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 27,   5),   S( 11,   8),   S( 51,  -4),   S( -9, -37),   S(-29, -61),   S(-16, -27),   S( 15, -57),   S( 38, -39),
            S( 19, -60),   S( 17, -15),   S(-34, -57),   S(-55, -37),   S(-26, -58),   S( 35, -63),   S(  9, -65),   S( -4, -51),
            S( 32, -96),   S( 11, -56),   S( -1, -66),   S(-39, -54),   S(-27, -30),   S( 12, -42),   S(-42, -20),   S( -1, -28),
            S(  3, -25),   S(-26, -37),   S( 17, -21),   S(-11,  -3),   S(-21,   8),   S(  1,  20),   S( -5,  24),   S(-10,  24),
            S( 24,   6),   S(  1, -32),   S(  8,  43),   S( 34,  91),   S( 53, 119),   S( 30, 118),   S( 12,  96),   S(-31, 105),
            S( 19,  34),   S(  7,  53),   S( 24,  69),   S( 31,  99),   S( 45,  94),   S( 50, 148),   S( 39, 100),   S(-22,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 27,   8),   S( 30,  21),   S( 23,  17),   S(  3,  26),   S( 19,   1),   S( 16, -19),   S( 30, -45),   S(-18, -18),
            S( 56, -57),   S( 16, -49),   S( 13, -55),   S(-13, -42),   S(-25, -27),   S(-44, -30),   S(-48, -32),   S( 17, -43),
            S(-11, -42),   S(-28, -43),   S(-21, -73),   S(-58, -42),   S( -1, -36),   S(-12, -47),   S(-55, -34),   S( 15, -31),
            S(-43,   0),   S(-46, -50),   S( -5, -69),   S(-38, -29),   S( -1, -42),   S( -3, -26),   S( 11,  -8),   S(  4,   8),
            S(  3,  13),   S( -7, -21),   S(-17,   3),   S( 19,  27),   S( 15,  59),   S( 18,  52),   S(  1,  66),   S(  1,  64),
            S(-10,  68),   S( 26,  60),   S( -3,  57),   S( 22,  60),   S( 25, 108),   S( 15,  83),   S( 16,  78),   S( 16,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-26, -50),   S( -4, -47),   S( -2, -20),   S( -3, -13),   S( 34,  16),   S( 72,   8),   S( 20,   3),   S( 10, -18),
            S( -8, -59),   S(-64, -41),   S(-11, -52),   S( 21, -38),   S(  0, -27),   S( -7, -22),   S( 18, -38),   S( 17, -43),
            S(-20, -46),   S(-87, -22),   S(-60, -41),   S(-11, -31),   S(-18, -47),   S(-12, -63),   S(-25, -61),   S( 63, -68),
            S(-35,  -2),   S(-20,  -7),   S(-23, -36),   S(-53, -40),   S(  4, -68),   S(-46, -53),   S(-24, -53),   S( 20, -50),
            S( 10,  16),   S( 30,  15),   S( 16,  11),   S(-20,  -4),   S(  9,  19),   S( 12,  13),   S(-30,   8),   S( 42,  -4),
            S(  7,  25),   S(  0,  48),   S( 25,  53),   S(  6,  59),   S( 23,  81),   S(  1,  42),   S(-14,  21),   S( 25,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-33, -45),   S(  0, -46),   S(-31, -44),   S(  6,  -2),   S(  2, -21),   S( 76,   4),   S( 54, -10),   S( 61, -11),
            S(-35, -60),   S(-50, -61),   S(-31, -73),   S(  4, -64),   S(-19, -32),   S( 22, -50),   S( 33, -45),   S( 46, -71),
            S(-21, -40),   S(-89,  -5),   S(-28, -25),   S( -7, -28),   S(-57, -46),   S( 44, -68),   S( 28, -121),  S( 86, -102),
            S(-51,  22),   S(-72,  31),   S(  2,  24),   S( 20, -11),   S(-27, -15),   S(-20, -48),   S(-34, -54),   S( 41, -96),
            S(-16,  20),   S(-19,  67),   S(-11,  92),   S( 18,  57),   S( 26,  59),   S(-10,   4),   S(  0,   6),   S(  8, -25),
            S( 14,  69),   S( 25,  55),   S( 29,  79),   S( 24,  79),   S( 11,  61),   S( 33,  80),   S( 12,  32),   S( 27,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -103),  S( 28, -51),   S( -2, -29),   S(  1,  -2),   S( -6, -30),   S(-35, -72),   S( 15, -46),   S(  5, -45),
            S( 39, -87),   S( 28, -47),   S(-22, -75),   S(-32, -59),   S(-31, -87),   S(-11, -63),   S(-12, -90),   S(-21, -67),
            S( -8, -61),   S( -9, -80),   S(-23, -97),   S(-25, -85),   S(-10, -56),   S( -6, -48),   S(-38, -59),   S( -9, -77),
            S(-13, -37),   S( -3, -17),   S(-19, -21),   S( -3,  -1),   S( 17,  56),   S(  3,  39),   S(  3,   9),   S( -7,  -4),
            S( 11,  22),   S(  1,  15),   S(  3,  22),   S( 19,  61),   S( 30,  76),   S( 25,  86),   S( 13,  80),   S( 19,  53),
            S( 12,  30),   S(  1,  36),   S( 12,  52),   S( 12,  60),   S( 25, 102),   S( 24,  92),   S(-21, -23),   S(-14,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-17, -61),   S( 25, -82),   S( 20,   5),   S( -2, -12),   S(  5, -22),   S(-28, -39),   S(-10, -74),   S(-15, -69),
            S( 29, -132),  S( 20, -102),  S(  0, -85),   S( 11, -11),   S(-24, -52),   S(  2, -82),   S(  1, -92),   S(  1, -88),
            S( 30, -88),   S(-10, -76),   S( -3, -91),   S(  6, -60),   S(-43, -29),   S( 20, -74),   S( -5, -75),   S( 59, -89),
            S( 16, -27),   S( -1, -35),   S(  1, -31),   S( -3,  24),   S( 12,   6),   S(-16,   6),   S(-14, -17),   S(  9, -23),
            S( -3,  42),   S(  8,  26),   S( -2,   5),   S( 22,  55),   S( 37,  79),   S( 28,  87),   S( 12,  94),   S( -7,  56),
            S( 11, 103),   S( 29,  51),   S(  3,  34),   S( 12,  44),   S( 19,  65),   S(  9,  50),   S( -4,  37),   S(  2,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -117),  S(  3, -70),   S( -5, -42),   S(  3,   3),   S( -6, -14),   S(  0,   0),   S( 21, -72),   S(-10, -40),
            S( 18, -113),  S(-38, -107),  S( -6, -82),   S(-29, -89),   S( -8, -58),   S( 18, -54),   S(  2, -66),   S( 25, -86),
            S( 17, -95),   S(-21, -78),   S(-14, -64),   S(  4, -75),   S(-23, -51),   S(  4, -91),   S(  2, -101),  S( 36, -60),
            S(  5, -33),   S(-22, -42),   S( -6,  -7),   S(-21, -12),   S( 13, -53),   S( -5, -30),   S( 12, -30),   S( 13,  -7),
            S(-14, -15),   S(  6,  39),   S( 11,  50),   S( -8,  15),   S( 19,  69),   S(  3,  16),   S( 17,  45),   S( 23,  65),
            S( -4,  32),   S(  7,  49),   S( 27,  73),   S( 21,  70),   S( 16,  59),   S(  1,  34),   S( 23,  86),   S( 24,  91),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -27),   S(  3, -59),   S(-26, -55),   S(-10, -26),   S(-12, -29),   S(-15, -43),   S( -8, -57),   S(  3, -86),
            S(-24, -66),   S(-21, -99),   S(-17, -107),  S(-10, -37),   S(-19, -25),   S( -7, -32),   S( 12, -56),   S( 11, -107),
            S(-27, -46),   S(-33, -62),   S(-44, -55),   S(  6, -41),   S(-32, -40),   S( -7, -74),   S(  3, -47),   S(  7, -45),
            S(  9, -36),   S(-26, -17),   S( -3,  38),   S(-20,  12),   S( 10,   5),   S(-10, -22),   S( -6, -13),   S( -7,  33),
            S(  6,  47),   S(  1,  51),   S(  0,  69),   S( 11,  59),   S( 24,  79),   S( 12,  63),   S( 17,  57),   S( 10,  23),
            S(-22,   8),   S( -7,   5),   S( 10,  71),   S( 20,  54),   S( 21,  69),   S( 18,  58),   S( 11,  36),   S( 16,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-76, -31),   S(-28, -26),   S(-19,  -5),   S( -8,  24),   S(-17, -24),   S(-27,   1),   S( -7, -28),   S(-77, -41),
            S( 12, -39),   S( -1,  -1),   S(-22, -31),   S( -9,  -8),   S(-10,  -6),   S( -9, -21),   S(-34, -46),   S(-29, -38),
            S(-18, -25),   S( 19, -34),   S( -1,   9),   S( 32,  24),   S(-11,  11),   S(  5,  -3),   S(-31,  21),   S(-26, -35),
            S(  9,  22),   S( 37,  47),   S( 29,  31),   S( 41,  19),   S( 27,  21),   S( 11,  29),   S( 36, -16),   S( -9, -18),
            S( 54,  38),   S( 20,  52),   S( 57,  60),   S( 58,  43),   S( 68,  30),   S( 17,  26),   S( 20,  -5),   S(  6,   0),
            S( 97, -37),   S(-11,  52),   S(140,   2),   S( 73,  40),   S( 52,  43),   S(-36,  64),   S( 37, -10),   S(-21,   3),
            S( 48,  -8),   S( -6, -23),   S( 41,  19),   S( 83,  65),   S( 39,  23),   S(  7,  30),   S(-12,   5),   S(-46,   1),
            S(-112, -124), S( -2,  -1),   S(  5,   2),   S( 17,  21),   S(  1,  27),   S( 18,  11),   S(-32,  -4),   S( -8,  11),

            /* knights: bucket 1 */
            S( 15,  -4),   S(-54,  19),   S(-23,  10),   S(-35,  32),   S(-17,  35),   S(-19, -20),   S(-28,  -5),   S(  5, -21),
            S(-38,  31),   S(-47,  53),   S(-27,  27),   S(-15,  23),   S(-20,  20),   S( -7,  23),   S(-11,  -4),   S(-16, -54),
            S(-36,  25),   S( -1,   0),   S(-20,  18),   S( -7,  51),   S(-13,  37),   S( -8,  10),   S(-41,  29),   S(-13,  21),
            S(-13,  66),   S( 31,  32),   S( -4,  50),   S( -6,  62),   S( -5,  57),   S( -9,  56),   S(  0,  25),   S(-24,  52),
            S( 56,  -3),   S(  3,  25),   S( 35,  62),   S( 16,  52),   S( 42,  49),   S( -2,  68),   S( -7,  49),   S( -2,  60),
            S( 22,  20),   S( 59, -11),   S( 75,  23),   S( 92,  32),   S( 70,  26),   S(-29,  74),   S( 20,  26),   S(  1,  38),
            S( 14,  -4),   S( 31, -11),   S( 28, -15),   S( 23,  46),   S( 10,  34),   S(  3,  19),   S( 15,  66),   S(-31,  41),
            S(-159, -41),  S( 14, -20),   S(-36, -63),   S(-20,   8),   S( -5,   9),   S( 37,  43),   S( 16,  41),   S(-66,  22),

            /* knights: bucket 2 */
            S(-61,   8),   S(-34,  29),   S(-23,   4),   S(-11,  20),   S(-13,  13),   S(-51,   2),   S(-25,   5),   S(-16, -29),
            S(-16,   3),   S( -3,  32),   S(-22,  11),   S(-18,  18),   S(-26,  24),   S(-16,   7),   S(  9,   5),   S(-30,   1),
            S(-32,  47),   S(-22,  21),   S(-19,  18),   S(-17,  55),   S(-19,  44),   S(-19,   9),   S(-23,  13),   S(  0,  -7),
            S( -4,  52),   S( -6,  43),   S(-24,  73),   S(-14,  76),   S(-33,  72),   S(  4,  48),   S( 10,  33),   S( -1,  35),
            S( -8,  62),   S(-20,  70),   S(  6,  67),   S( 16,  63),   S( -7,  71),   S( 16,  70),   S(-12,  65),   S( 17,  13),
            S(-42,  66),   S(-17,  48),   S(-13,  81),   S( 36,  33),   S( 43,  31),   S(123,  -1),   S( 70,   9),   S( 23, -12),
            S( 31,  34),   S(-43,  54),   S( 44,  19),   S( 29,   6),   S( -9,  43),   S( 11,  -9),   S( 28,  19),   S( 21, -11),
            S(-53,  27),   S( 27,  58),   S(-17,  62),   S(-13, -30),   S(-25, -16),   S(-32, -47),   S( 14,  -4),   S(-127, -60),

            /* knights: bucket 3 */
            S(-49,  16),   S( -9, -52),   S(  3, -20),   S(  5, -11),   S(  7, -15),   S( -5, -26),   S(-16, -24),   S(-24, -77),
            S(-14, -32),   S(  3,  -7),   S( 10, -12),   S( -1,  -3),   S( -3,  -1),   S( 20, -16),   S( 25, -40),   S( 23, -58),
            S(-11,  -4),   S(-10,   5),   S(  5,  17),   S(  8,  41),   S( 15,  27),   S(  0,  15),   S( 12,  -2),   S( 21, -35),
            S( 12,   1),   S( 18,  27),   S( 19,  46),   S( 12,  54),   S( 16,  68),   S( 31,  57),   S( 37,  46),   S( 17,  35),
            S(  0,  42),   S( 25,  35),   S( 27,  54),   S( 31,  76),   S( 30,  74),   S( 33,  84),   S(  4,  91),   S( 61,  74),
            S( -8,  27),   S(  8,  43),   S( 13,  58),   S( 24,  69),   S( 60,  71),   S(136,  68),   S( 56,  82),   S( 22,  95),
            S(-22,  37),   S(-15,  48),   S(-14,  59),   S( 31,  55),   S( 49,  58),   S( 94,  41),   S(  9,  -5),   S( 80,  16),
            S(-145,  35),  S(-30,  74),   S(-47,  82),   S( 32,  43),   S( 57,  71),   S(-54,  65),   S(-28, -44),   S(-61, -108),

            /* knights: bucket 4 */
            S(  9,  10),   S(-10, -11),   S(-52,  15),   S(-33, -12),   S(-31,  21),   S(-17, -12),   S( 19, -28),   S(-18, -16),
            S( 20,  35),   S(  8, -24),   S( -4,   7),   S(-11,   5),   S( -3, -12),   S( 16, -43),   S(-10,  11),   S(-46,  -5),
            S( -5, -20),   S( 20,  -4),   S( 49,   5),   S( 62,   6),   S( 12,  20),   S( 36, -30),   S( -9, -25),   S( -8, -34),
            S(-24, -28),   S( 27,   0),   S( 42, -13),   S( 68,   2),   S( 32,  11),   S( -4,  30),   S(-32,  27),   S( -6,   9),
            S(  0, -43),   S( 31,  -9),   S( 61,  15),   S( 33,  41),   S( 50,   6),   S( 17,  21),   S( 29,  -7),   S(-29,  42),
            S( -5, -26),   S( -1,  -1),   S( 38, -22),   S( 56,  21),   S(  3,  20),   S(-20,  39),   S(-18,   4),   S( 20,   3),
            S(-18, -30),   S(-21,  -9),   S(  3,  -5),   S( 23,  19),   S( 26,   9),   S(  0,  11),   S( 12,  36),   S(-34, -12),
            S(  3,  14),   S(-13, -39),   S( -8, -33),   S( 14,   0),   S( 12,  17),   S( -5,  13),   S( -5,  17),   S(-16, -15),

            /* knights: bucket 5 */
            S( 18,  22),   S( 15,  25),   S(-30,  35),   S( -9,  24),   S( -8,  30),   S( 13,  16),   S(-14,  17),   S(  9,  24),
            S( 20,  23),   S( 34,  24),   S(  3,   8),   S(-18,  14),   S( 35,  -9),   S(-25,  15),   S(-10,  41),   S(-45,  16),
            S(-30,  21),   S(-11,   5),   S( 22,  13),   S( 27,  17),   S( 18,  19),   S(-20,  26),   S( -6,  13),   S(-49,  16),
            S( 29,  14),   S( 30, -17),   S( 48,   2),   S( 85, -13),   S( 82,   7),   S( 71,  10),   S( -4,  21),   S( 15,  32),
            S( 39,   1),   S( 34,  -7),   S( 88,  -8),   S(123,  -5),   S( 87, -12),   S( 42,  19),   S(  5,   9),   S( 16,  24),
            S( -2, -25),   S( 36, -28),   S(  3, -20),   S( 12,  19),   S( 25,   3),   S( 46,   1),   S( -8,  12),   S( 25,  31),
            S(  0,   2),   S(-27, -58),   S( -2, -48),   S(-11, -17),   S( -8, -37),   S(  5,   4),   S( -2,  38),   S( 18,  30),
            S(-24, -40),   S(-27, -68),   S(  8, -14),   S(-25, -31),   S(  6,  -6),   S( -1,  27),   S( 19,  32),   S( -3,  16),

            /* knights: bucket 6 */
            S( -6, -11),   S(-43,  23),   S(-18,   5),   S(-36,  37),   S(-36,  32),   S(-11,  34),   S(-11,  42),   S(-35,   5),
            S(  6, -16),   S(-12,  45),   S(-13,   3),   S( 22,  10),   S( 16,  19),   S(-39,  37),   S(-18,  48),   S(-39,  64),
            S( -8,  15),   S( 13,  15),   S( -1,  28),   S( 22,  36),   S( 21,  33),   S(-45,  42),   S( 16,  28),   S(-15,  39),
            S( 11,  46),   S( 51,   6),   S( 33,  29),   S( 68,  14),   S( 82,  -4),   S( 64,  15),   S( 26,  18),   S(-16,  46),
            S( -4,  39),   S( 35,  14),   S( 86,  10),   S(111,   5),   S(100, -12),   S( 61,  29),   S(117, -15),   S( 20,  26),
            S( 12,  16),   S( 23,   7),   S( 52,  19),   S( 39,   8),   S( 48,   2),   S( 37,   3),   S(  9, -10),   S( 23,  -1),
            S(  0,  28),   S( 14,  30),   S( 34,  36),   S( -2,  -6),   S( 26, -14),   S( 19, -38),   S( -8,  -6),   S( 10,  36),
            S( 13,  29),   S(  0,  25),   S( 15,  30),   S(  2,  14),   S(  7, -10),   S( -9,  -7),   S(  8,  20),   S(-24, -39),

            /* knights: bucket 7 */
            S(-34, -44),   S(-20, -43),   S(  2, -17),   S(-38,  19),   S( -3,  -3),   S(-34,   4),   S(-13,  -9),   S(-16,  19),
            S(-34, -54),   S( -8, -27),   S(-37,  -5),   S(-33,  -1),   S( -1,   9),   S(  3,  22),   S( -7,  11),   S(-60,  33),
            S( -3, -40),   S(-37, -21),   S(  5, -13),   S( -3,  23),   S( 41,  17),   S( 30,  11),   S( 13,  16),   S( -8,  28),
            S(-36,  14),   S(  8,  -5),   S( 53, -16),   S( 78,   5),   S(101,  -5),   S( 76,  18),   S( 64,   3),   S( 62,   4),
            S(  3,   4),   S( -1,  10),   S( 20,  19),   S( 75,   1),   S(101,   4),   S(143, -20),   S(184, -13),   S( 27,  -8),
            S(-19,  12),   S( 25,   9),   S( -4,  10),   S( 45,  20),   S( 92,   0),   S( 90,  -6),   S( 16,  -8),   S(  2, -43),
            S(-22,   1),   S( -7,   1),   S( -4,  13),   S( 25,  20),   S( 55,  14),   S( 26,  20),   S(-16, -37),   S(-17, -40),
            S(-31, -41),   S(-10,   6),   S( -3,  19),   S(  3,  14),   S( 12,   5),   S( 16,   7),   S(  3,  -9),   S(  0, -11),

            /* knights: bucket 8 */
            S( -2,   2),   S(  9,  22),   S( 10,  22),   S(-10, -32),   S( -2,  21),   S( -4, -19),   S( 13,  24),   S( -3, -14),
            S( -7, -24),   S( -6, -24),   S( -9, -39),   S(-11,   4),   S( -6,  33),   S(  0,  -6),   S(  0,  -6),   S( -2,  -3),
            S(-11, -41),   S( -8, -25),   S(  1, -46),   S(  2,  10),   S(-11, -18),   S( 12,  10),   S( -2,  -5),   S( -1, -15),
            S(-18, -55),   S( -9, -30),   S(  8,  20),   S( -1,  11),   S(-19, -13),   S(-24, -10),   S(-20, -29),   S(-15, -36),
            S( -7, -25),   S(  5, -18),   S( -1, -18),   S(  1,  -4),   S(-17,   4),   S(-10, -12),   S(  5,   1),   S( -1, -13),
            S( -3,   8),   S( 12,  -1),   S( -2,   7),   S( -5,  -9),   S( -7,   0),   S( -4,  -8),   S( -9,  -5),   S( -7, -20),
            S(  0,  17),   S( -2, -28),   S(-12, -20),   S(  5,  12),   S(  2,   1),   S(  0,  -1),   S( -4,   3),   S( -3, -18),
            S(  0,   1),   S( -4,   6),   S( -5,   1),   S(  2,  -3),   S( -2,   5),   S( -2,  -7),   S( -1,   3),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-20, -65),   S( -6,  -5),   S( -5, -39),   S( -5, -38),   S(-16, -12),   S(-12,   8),   S(  5,  18),   S(  2, -11),
            S( -6,   1),   S(-15, -47),   S(-21, -107),  S(-26, -64),   S(-10, -34),   S(-22, -58),   S(-11,  -4),   S(-12,   0),
            S(-10, -24),   S(-17, -43),   S(-12, -34),   S( -5, -53),   S(-22,  -9),   S( 10,  10),   S(-14,  -6),   S( -4,   2),
            S(-17, -49),   S(-12, -43),   S( -7, -20),   S( -9, -33),   S(-15, -27),   S(  2,   3),   S(-17, -40),   S(  2,   9),
            S(  3,  24),   S( -8, -25),   S( -1, -16),   S( -1, -26),   S( -9, -21),   S( -4,  12),   S( -9, -10),   S( -5,  -1),
            S(-13, -21),   S(-18, -34),   S(-11, -16),   S( -3, -12),   S(  2,  20),   S( -6,   5),   S( -3,  21),   S( -1,   5),
            S(-10, -16),   S( -1,  17),   S(-12,  -4),   S(-23, -18),   S(  1,   3),   S(  1,  22),   S( -8,  14),   S( -7,   0),
            S(  4,   0),   S(  4,   1),   S( -2,   9),   S( -1,   4),   S(-11,  -7),   S( -5,   0),   S(  3,   8),   S( -1,  12),

            /* knights: bucket 10 */
            S( -9, -33),   S( -6,   9),   S(-10, -11),   S(-11,  15),   S(-22, -51),   S(  6, -23),   S( -4,   7),   S( -3,  11),
            S( -4, -19),   S(  8,  -1),   S(-13, -24),   S( -9, -44),   S( -8, -30),   S(-26, -56),   S( -8,  12),   S(  1,  25),
            S( -3,  -6),   S( -6, -10),   S( -7, -14),   S(  7, -45),   S(-26, -38),   S( -5, -16),   S(-11, -33),   S(-11,   6),
            S(-10, -18),   S(-11, -23),   S( -7,  -9),   S( -3, -18),   S(-10, -13),   S( -5,   1),   S( -9, -48),   S( -4,  -5),
            S(-12, -20),   S(-11, -29),   S( -7,   0),   S( -6, -10),   S(  4,   0),   S( -6, -31),   S( -3,  -8),   S(  4,   9),
            S( -2,   9),   S(-12,   1),   S( -9,  13),   S(-12,  23),   S(-14, -13),   S(-18, -12),   S(-14,  -1),   S(-17,  -7),
            S(  2,   7),   S( -3,  -6),   S( -6, -28),   S( 12, -19),   S( -6,   3),   S(-16, -45),   S( -9,   5),   S(-10, -14),
            S( -1,   1),   S( -2,   7),   S( -1,  15),   S( -5,   2),   S( -5,   3),   S( -7, -13),   S(  5,   8),   S(  1,   6),

            /* knights: bucket 11 */
            S( -4, -17),   S(-25, -27),   S( -4,  -6),   S(  4,  17),   S(-39, -37),   S( -3,   9),   S( -7,   3),   S(  8,  29),
            S( -8, -17),   S(-26, -41),   S(-11, -42),   S( 15,  -3),   S(  7,  18),   S( -3, -27),   S(-14, -25),   S( -9, -13),
            S(-13, -42),   S(-18, -21),   S( -3, -10),   S(  1,  -2),   S( -8,  21),   S( 15,  -2),   S( -2, -13),   S( -4,  -5),
            S(-15, -12),   S(  6, -20),   S( -2, -21),   S( 10,   4),   S( 26,   0),   S( -2, -15),   S( 13,  22),   S( -1,  -6),
            S(-16,   0),   S(  3, -37),   S(-17,   3),   S(  3, -12),   S( 33,  13),   S(  5,  20),   S( -9, -66),   S(-10, -13),
            S( -8, -25),   S( -7, -44),   S(  5,  10),   S(  9,   3),   S(  9,  36),   S( -7,  -9),   S( -4, -26),   S( -2,  18),
            S( -1,  -7),   S( -8,  16),   S(-11, -11),   S(  6,  -3),   S( 12,  -4),   S(  3, -17),   S(  0, -18),   S( -4,   0),
            S( -3, -18),   S(  1,   6),   S( -4, -11),   S(  1,  15),   S( -5, -11),   S( -1, -10),   S(  4,  14),   S( -1,  -5),

            /* knights: bucket 12 */
            S(-14, -42),   S( -3, -10),   S( -2, -19),   S(  0,   8),   S( -4,   6),   S( -5, -11),   S( -1,   6),   S( -1,   1),
            S( -3,  -9),   S(  0,   4),   S(  0, -17),   S( -4,   5),   S( -5,  -9),   S(  0,   3),   S(  1,   0),   S(  0,  -8),
            S( -3, -10),   S( -6, -21),   S( -6, -19),   S(-15, -23),   S( -8,  -3),   S( -2,  27),   S( -3,   0),   S( -4,  -9),
            S(  2,   8),   S( -2, -35),   S( -7,  27),   S(  4,  15),   S( -4, -10),   S(  4,  23),   S(  6,  12),   S(  2,   7),
            S(  0,   3),   S( -4,  -6),   S( -4, -19),   S( -4, -10),   S(  1,   7),   S( -2,   6),   S( -6,  -3),   S( -9,  -8),
            S( -5,  -3),   S( -1,  -3),   S( -3, -12),   S( -1,  -8),   S( -2,   1),   S( -7, -18),   S(  7,   7),   S( -1,   8),
            S( -4,  -8),   S( -2,  -1),   S( -9,  -1),   S( -2,  -6),   S(  0,   8),   S( -9,  -8),   S( -5, -19),   S( -4,  -3),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -6),   S(  1,   2),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -6),   S( -4, -13),   S( -3, -17),   S( -2,  -7),   S( -3, -11),   S( -2,   7),   S( -6,  -2),   S(  3,  10),
            S( -2,   7),   S( -2,  -3),   S(  3,  10),   S( -4,  -2),   S( -6, -11),   S( -1,   9),   S(  2,  20),   S( -4,  -6),
            S(  4,  -3),   S(  5,   9),   S(  5,   3),   S( -4, -24),   S(  4,  23),   S( -5,   9),   S(  6,   3),   S( -3,  -3),
            S( -1,  14),   S(  0,   3),   S( -6,  -1),   S(  1,  28),   S(  0,  13),   S( -2,  30),   S(  0,   7),   S( 10,  19),
            S(  1,  21),   S( -2, -15),   S( -4,  13),   S( -7,   9),   S(-16,  -1),   S( -3,  25),   S( -8, -23),   S( -3,  -3),
            S( -4,  -5),   S(  2,   3),   S( -3,   9),   S(  3,  13),   S( -8,   8),   S( -8,   4),   S(  3,  20),   S(  0,   2),
            S(  1,   4),   S(  3,   8),   S( -6,  -4),   S( -4,   1),   S( -2,   6),   S( -4,  -8),   S(  2,   6),   S( -1,   1),
            S(  2,   6),   S(  0,   2),   S( -2,  -3),   S(  2,   4),   S(  0,   1),   S(  1,   2),   S( -1,  -2),   S(  0,   1),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   3),   S(  5,  17),   S( -2,  -1),   S( -6, -24),   S( -2,  17),   S(  2,   1),   S( -1,   3),
            S( -2, -12),   S( -8, -17),   S(  2,  -4),   S( -1,   0),   S(  3,   1),   S(  0,   4),   S( -8,   5),   S(  6,  56),
            S( -1,  -1),   S( -5, -33),   S(  6,  17),   S(-11, -35),   S( -3,  -1),   S(  1,  10),   S( -1,   9),   S(  3,  17),
            S( -1,  -4),   S( -4, -18),   S(-22, -12),   S( -2,  45),   S(  2,  42),   S( -4,  -4),   S(  0,   5),   S(  1,  34),
            S(  6,  15),   S(-17, -35),   S( -9,  -7),   S( -8,   5),   S(  0,  33),   S(-11,   7),   S( -4,  -1),   S(  3,  12),
            S( -1,   2),   S(  5,   5),   S(  3,  -4),   S( -2,  14),   S(  2,  18),   S(  1,  14),   S(  1,   7),   S( -5, -12),
            S(  0,   4),   S( -3,  -2),   S(  3,  16),   S(  6,   4),   S(  3,  11),   S( -4, -10),   S(  2,   6),   S(  3,   5),
            S(  0,  -1),   S(  0,   1),   S( -1,  -2),   S(  2,   4),   S( -1,   1),   S( -1,  -2),   S(  0,   0),   S(  1,   2),

            /* knights: bucket 15 */
            S( -3, -15),   S( -1,   3),   S(  4,  23),   S( -2,   5),   S( -4, -17),   S(-10, -37),   S( -4, -15),   S( -2, -12),
            S(  2,  -2),   S(  4,   6),   S( -6,  -8),   S(  8,  42),   S(  1,  15),   S( -8, -34),   S( -3,  -3),   S(  1,   2),
            S(  0,  -5),   S( -5, -20),   S(  1,  -9),   S(  5,   8),   S(-18, -27),   S( -1,  -5),   S( -2,  -6),   S( -2,  -2),
            S(  0,  -8),   S( -3,   3),   S( -5, -12),   S( -5,   7),   S( -7,   6),   S( -9,  27),   S(  4,   6),   S( -2,   0),
            S( -1,  -2),   S(  9,  22),   S( -4,   7),   S( -6,   6),   S( 18,  35),   S(  0,  17),   S(  6,  -3),   S(  4,  18),
            S(  1,   3),   S( -4,  -9),   S( -1,   2),   S( -9, -16),   S( -6,  -8),   S(  2,  17),   S(  0,   8),   S(  5,  12),
            S( -1,   0),   S( -2,  -7),   S(  4,  16),   S(  3,   3),   S(  3,  13),   S(  5,   8),   S(  1,   7),   S(  3,   8),
            S(  1,   4),   S( -1,  -6),   S(  0,  -1),   S( -1,   0),   S(  2,  10),   S(  1,   1),   S(  0,   0),   S(  1,   2),

            /* bishops: bucket 0 */
            S( 21,  -7),   S( -9,  38),   S(-13,  16),   S(-24,  -8),   S( -3,   0),   S(  1,  10),   S( 64, -42),   S( 17, -16),
            S(-31,  -9),   S( -9, -20),   S(-23,  36),   S(  0,  13),   S(  2,  19),   S( 49,  -8),   S( 29,  22),   S( 41, -17),
            S( 13,  10),   S(  4,  26),   S(  5,  -5),   S(  7,  11),   S( 23,  18),   S( 30,  19),   S( 37,   4),   S( 24,   3),
            S( 16, -30),   S( 37, -38),   S( 15,  17),   S( 32,  19),   S( 66,  35),   S( 31,  47),   S( 17,  20),   S(  7,  28),
            S( 35, -15),   S( 46, -17),   S( 56,   9),   S( 79,  43),   S( 92,  25),   S( 25,  43),   S( 34,  48),   S( -4,  16),
            S( 49,  16),   S( 54,  43),   S( 96,   5),   S( 60,  -2),   S( 24,  43),   S( 14,  37),   S( 42,  33),   S( -6,  14),
            S(-48, -81),   S( 68,  31),   S( 87,  81),   S( 20,   0),   S( 17,  -8),   S( 29,  31),   S(-24,  21),   S(-13,  52),
            S(-22, -40),   S( -5, -10),   S( 13, -27),   S(-16, -17),   S(-13, -17),   S(-18,   7),   S(-17,  19),   S(-30, -35),

            /* bishops: bucket 1 */
            S(-62,  12),   S( -2,  -3),   S(-18,  40),   S( 19,  -7),   S(-16,  22),   S( 10,   4),   S( 36, -13),   S( 22, -36),
            S( -2, -33),   S(-21, -12),   S( -5,  -4),   S(-16,  17),   S( 28,  -7),   S(  3,   3),   S( 45, -37),   S( 11, -13),
            S(-12,   0),   S( 30,  -9),   S(-21,  -7),   S( 18,   7),   S(  3,   2),   S( 26, -29),   S( 15,  -2),   S( 60,  -5),
            S( 25, -18),   S( 51, -14),   S( 27,   4),   S( 23,  15),   S( 40,   3),   S(  8,  14),   S( 49,  -2),   S(  1,  15),
            S( 21, -10),   S( 57, -13),   S( 15,  10),   S( 96, -13),   S( 49,  23),   S( 42,  23),   S(  0,  27),   S( 31,   3),
            S( 61, -45),   S( 47,  10),   S( 58, -24),   S( 66, -10),   S( 77,   4),   S(-36,  13),   S(-21,  54),   S(-27,  18),
            S(  7, -65),   S( -2, -50),   S(-10,   3),   S( 24,  45),   S( 27,  34),   S(-14,  30),   S(-20,   4),   S(-27,  33),
            S(-14, -34),   S(-21,   6),   S( -8, -22),   S(-49,  -2),   S(-25,  17),   S( 16,   2),   S( 26,   2),   S(-53,  -2),

            /* bishops: bucket 2 */
            S( -4, -23),   S( -9,  -9),   S(  5,  17),   S(-19,   8),   S( 14,  13),   S(-16,   8),   S( 19,  -9),   S( -6, -22),
            S( 18, -24),   S(  3, -33),   S( -5,  -7),   S(  6,  14),   S(-11,  13),   S(  7,   6),   S( -2, -31),   S( 12, -53),
            S( 43,  -1),   S( 24,  -2),   S( -5,  -4),   S( -7,  10),   S( -1,  31),   S(-14, -30),   S(  9, -22),   S(-10,  -8),
            S(-17,   6),   S( 43,  17),   S( -6,  20),   S( 29,  32),   S(  3,  17),   S( -2,  23),   S(-13,   2),   S(  9,   9),
            S(  1,  19),   S(-34,  46),   S( 51,  23),   S( 20,  32),   S( 21,  32),   S( 24,  11),   S( 11,  31),   S( 32,  -6),
            S(-29,  36),   S( -2,  40),   S(-33,  -8),   S( 90,   0),   S( 50,  14),   S( 96, -15),   S( 73,  14),   S( 39, -47),
            S(-32,  61),   S(-39,   0),   S( -7,  22),   S(  2,  15),   S(-48,  -7),   S(-42,  16),   S(-36,  -2),   S( -7, -43),
            S(-82, -22),   S(-15,  25),   S( -3,   8),   S(-22,  27),   S(-33, -13),   S(-35,   9),   S( -8, -13),   S(-62, -23),

            /* bishops: bucket 3 */
            S( 33, -22),   S( 39, -22),   S( 24, -24),   S( 14,  -3),   S( 19,  12),   S(  1,  31),   S(-11,  50),   S( -4, -24),
            S( 37,  -2),   S( 23, -32),   S( 19,  -3),   S( 22,   4),   S( 20,  19),   S( 23,   8),   S(  9, -18),   S( 32, -42),
            S( 16,  -5),   S( 37,  33),   S( 19,   6),   S( 18,  26),   S( 18,  31),   S(  9,   0),   S( 26,  -8),   S( 16,   8),
            S( -7,  16),   S( 13,  43),   S( 26,  50),   S( 36,  47),   S( 33,  25),   S( 31,   9),   S( 29,   0),   S( 42, -42),
            S(  9,  32),   S( 19,  54),   S( 10,  56),   S( 58,  47),   S( 49,  45),   S( 49,  23),   S( 28,  18),   S(  4,   8),
            S(  5,  34),   S( 26,  55),   S(  9,  15),   S( 24,  40),   S( 56,  40),   S( 71,  42),   S( 45,  42),   S( 42,  73),
            S(-22,  74),   S( -2,  26),   S( 16,  28),   S(  1,  54),   S( 29,  33),   S( 55,  50),   S(-44,  25),   S( 13, -22),
            S(-40,  11),   S(-25,  51),   S(-48,  39),   S(-33,  49),   S( 12,  11),   S(-65,  32),   S( 14,   4),   S( -2,   3),

            /* bishops: bucket 4 */
            S(-37,   3),   S(-29,   5),   S(-37,  18),   S(-55,  14),   S(-32,  -9),   S(-22,  -5),   S(-13, -21),   S(-40, -38),
            S(-10,   1),   S(-12, -15),   S( 62, -30),   S(-38,  18),   S(-57,  27),   S(-11, -27),   S(-31, -30),   S(-28, -19),
            S(  5,  22),   S(-11, -14),   S(  2,  -4),   S( -5,   6),   S( 12,  -5),   S(-65,   3),   S(-17, -28),   S(-54, -15),
            S( 29,  -1),   S( 51, -13),   S( 33,  14),   S( 14,  29),   S( -6,  27),   S( 30,   2),   S(-43,  10),   S( -8, -20),
            S( 16, -12),   S( -9, -17),   S( 39,  -8),   S( 18,   8),   S( -1,  32),   S( 22,  14),   S(-13,  38),   S(-54,   5),
            S(-52, -82),   S(-46,  -1),   S(-10,   2),   S(  6,   9),   S(-43,  48),   S( 12,   7),   S(-10,  30),   S( -4,  28),
            S( -2,   0),   S(-26,  -4),   S(  1, -18),   S(-29, -11),   S(  1, -19),   S( 37,   4),   S( -6, -11),   S( 18,  34),
            S( -8,  -9),   S( -2, -21),   S(-13, -10),   S(  0, -17),   S(-19,   3),   S(  3,  18),   S(  5,  42),   S(  5,   1),

            /* bishops: bucket 5 */
            S(-47,  -9),   S( 20,  -8),   S(-39,  20),   S(-48,  22),   S(-13,   6),   S(-59,  19),   S(-36,  20),   S(-51, -19),
            S(-15,  -7),   S(-29,  -6),   S( 21,  -1),   S(-20,  20),   S(-59,  34),   S(-31,  26),   S(-39,  -4),   S(  4, -12),
            S(  4,  28),   S(-17,   4),   S( 14, -19),   S( -2,  13),   S(-14,  25),   S(-69,   5),   S(-19,  22),   S(-22,  26),
            S( 15,  10),   S(  5,  18),   S( 67, -11),   S( 38,  18),   S( -8,  30),   S(  5,  26),   S(-62,  39),   S(-22,  23),
            S(  9,  -4),   S( 31,   3),   S(-12,  18),   S(-12,   7),   S( -4,  16),   S( -7,  20),   S(  9,  26),   S(-45,  19),
            S(  2, -10),   S(-37,  22),   S( 13, -18),   S(-14, -12),   S(-15,  14),   S(-16,  -7),   S(-20,  23),   S(-34,  48),
            S(-25, -11),   S( -7, -15),   S(-18,  -1),   S(  3,  25),   S( 16,   6),   S(-12,  30),   S( -3,   8),   S(-20,  32),
            S(-17,  -7),   S(-11, -19),   S( -2, -17),   S(-19,  -1),   S(-25,  30),   S(  7,  37),   S(-20,  26),   S( 10,   4),

            /* bishops: bucket 6 */
            S(-17, -35),   S(-15,   7),   S(-33,  21),   S(-19,  14),   S(-55,  34),   S(-30,  20),   S(-40,  30),   S(-62,  -7),
            S(-40,  14),   S(-33, -26),   S(-60,  43),   S(-45,  33),   S(-49,  32),   S(-49,  23),   S(-44,   9),   S(-37,  10),
            S( -2,   5),   S(-38,  22),   S( -9, -11),   S(-39,  38),   S(-30,  41),   S(-33,  -8),   S( -8,  -5),   S(-18,  23),
            S(-61,  31),   S(-50,  34),   S(-16,  25),   S( 20,  42),   S(  9,  40),   S( 11,  21),   S( 20,   9),   S(-14,  24),
            S(-43,  26),   S(-24,  34),   S(  5,  17),   S( 55,  20),   S(-20,  25),   S(-15,  15),   S( 10,  20),   S(-23,  -1),
            S(-47,  42),   S(-15,  25),   S(-49,   3),   S(-21,  21),   S( 12,  17),   S( -7,  -2),   S( -8,  24),   S(-31,   1),
            S(-17,  36),   S(-76,  32),   S(-30,  20),   S(-22,  28),   S( -9,   8),   S(  4,   8),   S(  9, -11),   S(-29,  13),
            S(-18,   2),   S(-28,  37),   S(-12,  32),   S( 23,  14),   S(-27,  24),   S( 18, -14),   S(-12,  11),   S(-14,  11),

            /* bishops: bucket 7 */
            S(-20, -53),   S(-56, -10),   S(-39, -20),   S(-17, -12),   S(-39,  -3),   S(-37,  -5),   S(-64, -19),   S(-51, -16),
            S( -9, -49),   S(-10, -48),   S( 13, -21),   S(-27, -11),   S(-35,   3),   S(-44,   5),   S(-40, -29),   S(-10, -16),
            S(-47, -22),   S(-27,   4),   S(-15, -23),   S(  7,  -3),   S(  0,   0),   S( -9, -35),   S(-55,   8),   S(-63,  10),
            S(-18, -23),   S(-59,  29),   S(-24,  14),   S(-10,  24),   S( 88,   2),   S( -6,  18),   S( 38, -27),   S(-19,  -3),
            S(-22,   0),   S( 25, -10),   S(-41,  32),   S( 13,   6),   S( 53,  -4),   S( 45,  12),   S(-27,  18),   S(-36, -10),
            S(-69,  33),   S(-30,  50),   S(-13,  -6),   S(-81,  37),   S(-30,  23),   S(  8,  -9),   S(  3,  38),   S(-54, -80),
            S( -7,  -5),   S(-30,   0),   S(-42,  21),   S( -3,   9),   S( -2,   2),   S( 20, -23),   S(  7, -27),   S(  2, -13),
            S(-23, -32),   S( -4,   6),   S(-11,  13),   S( -5,   8),   S(-11,   1),   S(  8, -15),   S( 29, -27),   S( -3,  -8),

            /* bishops: bucket 8 */
            S( 33,  56),   S( -4, -36),   S( -2,  -3),   S(-10,  40),   S(  0,  18),   S( -7, -38),   S(-16, -26),   S(-11, -18),
            S(  0,  -3),   S( 13,  25),   S( 21,   7),   S(  7,  21),   S(  0, -15),   S(  2,   1),   S(-34, -48),   S(-10,   0),
            S( -7,  -6),   S(-14, -13),   S( 22,  26),   S( 11,  15),   S(  7,  17),   S( -3,  -1),   S(-25, -13),   S(-34, -27),
            S( -4, -11),   S( 31,  25),   S( -1,  28),   S( 24,  12),   S(  7,  36),   S( 14,  30),   S(-10,   9),   S(  3, -17),
            S( 15,  17),   S( 50,  57),   S( 20,  -1),   S( -5,  24),   S( 12,  27),   S(-22,  26),   S( -6, -24),   S(  5,  18),
            S( -9,  -7),   S(  3,   8),   S(  7,  21),   S( 24,  17),   S( 12,  34),   S( 27,   3),   S( -7,  59),   S( -2,  32),
            S(  2,  14),   S(-18, -44),   S( 27,   0),   S( 24,   3),   S( 10,   1),   S( 22,  47),   S( 17,  24),   S(-13,  -3),
            S( -7,  -4),   S(  5,   3),   S(  1,  17),   S(  2,  11),   S( 29,   4),   S( 22,  11),   S( 14,  37),   S( 35,  25),

            /* bishops: bucket 9 */
            S(  5,  27),   S(  4,  12),   S( -3,  -3),   S(-31, -28),   S(-21, -10),   S( -9,  -6),   S( -3,  -2),   S( -9,  -7),
            S( -1,  -4),   S(  6, -12),   S(  3,  16),   S(-33,   5),   S(-28,  14),   S(-11,  -8),   S(-38, -14),   S(-17, -29),
            S(-10,   4),   S( 17,   7),   S( -5, -21),   S(  2,  27),   S( 11,  16),   S(-31, -19),   S( -1,  10),   S(-10,  -5),
            S( -1,  25),   S(  0,  -7),   S( 29,   4),   S( 26,   6),   S( -3,  27),   S( -9,  20),   S(  5,  26),   S( -3,  13),
            S( 25,  17),   S( 19,  15),   S( 28,  25),   S( 16, -15),   S( 14,  33),   S( -1,  37),   S(  6,  38),   S(-15, -19),
            S( 17,  22),   S( -5,  30),   S(  9, -14),   S( 14,  20),   S( 41, -37),   S( -6,  13),   S( 16,  34),   S( 11,  27),
            S( 12,  10),   S(-13,  10),   S(  9,  13),   S( 21,  -1),   S( 24,   2),   S( 32,  18),   S( 14,  28),   S( 17,  55),
            S( 10,  35),   S(  1, -24),   S(  3,  22),   S( 12,  17),   S(  9,  41),   S( 18,  -3),   S( 25,  -1),   S( 28,  20),

            /* bishops: bucket 10 */
            S( -2, -33),   S( 11,  11),   S( -4, -20),   S(-26, -20),   S(-67, -16),   S(-33, -58),   S(  7,  -6),   S( -5,  12),
            S( -9,  18),   S( -5, -54),   S( -8, -15),   S(-23, -34),   S(-50,   9),   S(-31, -19),   S(-32, -17),   S(  1,   1),
            S(-10, -33),   S(-19, -14),   S(-20, -28),   S( -6,  30),   S(-16,  14),   S(-14, -31),   S( -7,   5),   S( -6, -18),
            S(-16,  12),   S(-21,   4),   S(-26, -23),   S(  7,   7),   S(-19,  54),   S( 28,  16),   S( 36,  30),   S( -5, -31),
            S( 11,   6),   S(-36,  27),   S( -1,  12),   S(  5,  39),   S( 39,  -5),   S( 25,  42),   S( 22, -11),   S( 16,   9),
            S(  7,   7),   S( 10,  19),   S(-11,  -1),   S( 27,  15),   S( 15, -13),   S(  1,  -5),   S( 11,  11),   S( 25,  13),
            S( 20,  37),   S( -5,   1),   S( 31, -13),   S( 12,  30),   S(  0,  15),   S( -7, -22),   S(  0, -16),   S( 21,  27),
            S( 10,  23),   S( 20,  30),   S( 42,  15),   S(  9,  20),   S( -4,  23),   S(  6,  13),   S( 13,  16),   S(  0, -15),

            /* bishops: bucket 11 */
            S( 11, -17),   S( -7, -14),   S(-10, -10),   S(  0,  -4),   S(-21, -16),   S( -5,  -6),   S(-22, -27),   S(-11,   0),
            S( -6, -12),   S(  3, -21),   S(-10,  11),   S(  1, -12),   S(-16,  11),   S(-43,  -6),   S(-37, -15),   S(  8,   2),
            S(-10, -49),   S( -1, -17),   S(-12, -35),   S(-31,   8),   S( -8,  -6),   S(  6,  21),   S( -1,  -7),   S( -2, -15),
            S(  3,  -1),   S( -1, -30),   S(  8,   0),   S(-31, -15),   S( 12,   4),   S( 18,  51),   S( 43,  17),   S( -8, -26),
            S(-10, -16),   S(-13,  -8),   S(-35,  41),   S(-25,  39),   S(-22,  36),   S( 38,   9),   S( 29,  -9),   S(  8,   5),
            S( -6,   8),   S( -8,  -7),   S( -8,  -7),   S(  1,  25),   S( 24,  21),   S(  8, -25),   S(  3, -12),   S( -2, -17),
            S( -2,  -6),   S( 15,  24),   S( 18,  49),   S( 32,  24),   S( 18,  -6),   S( -6,  -3),   S(-18, -29),   S( -7, -14),
            S( 27,  15),   S(  3,   0),   S( 28,  44),   S( 28, -19),   S( 17,  16),   S(  4,   5),   S( -6, -13),   S(  5,  -5),

            /* bishops: bucket 12 */
            S( -5, -11),   S( -4, -13),   S( -7,  -2),   S(  6,  18),   S(-10, -10),   S( -7,  -4),   S(  0,   0),   S( -1,   1),
            S(  0,  -6),   S(  6,   2),   S( -2,  -3),   S(  1,  14),   S(  0,  10),   S(  9,   8),   S(-14, -21),   S( -2,  -5),
            S(  8,   5),   S( 11,  -3),   S( 20,  15),   S( 20,  17),   S( -1,  13),   S( -7,  -8),   S(  2,   6),   S( -5,  -3),
            S(  9,   2),   S( 17,   5),   S( 20,   7),   S( 17,  41),   S( 11,   7),   S(  5,  23),   S(  3,  13),   S(  3,   7),
            S( 11,   9),   S( 10,  10),   S( -2,  18),   S( 21,   9),   S( 18,  27),   S(  9,  31),   S(  7,  12),   S(  4,  12),
            S(  2,   0),   S( -8,  -9),   S( -5,  13),   S(  2,  -4),   S( 31,  31),   S(  9,   9),   S( -9,  -8),   S( -5, -11),
            S( -3,  -4),   S(  4,  10),   S(  3,  10),   S(  5,  -5),   S( 13,   2),   S( 20,  24),   S( 12,  25),   S( -1,  -3),
            S(  0,   4),   S( -1,  -4),   S(  0,  -4),   S(  0,  -5),   S(  2,   8),   S(  4, -11),   S( 14,   5),   S(  7,   4),

            /* bishops: bucket 13 */
            S( -5, -19),   S( -1,  -4),   S( -5, -15),   S( -6, -11),   S( 16,  14),   S( -8, -13),   S(-16, -21),   S( -2,  -4),
            S( -5,  -2),   S( -8, -12),   S( -1,   3),   S( 15,   0),   S( -6, -15),   S(  3,  12),   S( -1,  -8),   S(  0,  -4),
            S(  8, -11),   S( 30,  18),   S( 10,  -1),   S( 18,  30),   S(  2,  23),   S(  7,  19),   S( -7,   4),   S( -7,  -5),
            S( 24,  28),   S( 46,  17),   S( 22,  29),   S(-16,  11),   S( 17,  69),   S(  3,  13),   S(  9,   7),   S(  2,   9),
            S( 21,  21),   S( 16,  15),   S( 12,   2),   S(  9,  -6),   S( 11,  -4),   S( 11,  23),   S( 13,  17),   S(  2,  10),
            S(  6,   4),   S(  1,   7),   S( -3, -11),   S( 17,  -4),   S(  7,  15),   S( -6, -18),   S(  2,  -4),   S( 11,   0),
            S(  7,   7),   S( -9, -20),   S( -1, -17),   S(  4,   3),   S(  6,  19),   S( 18,  10),   S(  8,  -4),   S(  9,  11),
            S(  1,  -1),   S( -2,  -2),   S(  0,  12),   S(  2,   9),   S(  7,  14),   S(  3, -12),   S( 13,  -4),   S( 11, -11),

            /* bishops: bucket 14 */
            S(-12, -24),   S(  5,  22),   S( 15,  12),   S(  4,  21),   S(-12,  -2),   S( -8,  -7),   S( -5,   2),   S( -8,  12),
            S( -1,   1),   S( -2,  -5),   S(  2,  12),   S( -2,  -9),   S( 12,   3),   S(  2,   8),   S( -6,  17),   S(  3,  27),
            S(  1,  -4),   S( -2, -13),   S( -9, -15),   S( 19,  33),   S( 23,  45),   S( 11,  20),   S(  5,  37),   S(  3,  28),
            S(  4,  32),   S(  8, -12),   S( -3,  -1),   S(  3,  31),   S( 10,  20),   S( 21,   9),   S( 21,  16),   S(  9, -15),
            S( 10,   6),   S(  6,  15),   S( 11,   8),   S( 20,  12),   S( -2,   2),   S(  6,  14),   S( 23,   1),   S( 15,  10),
            S(  2, -12),   S( 23,  37),   S(  3,   8),   S( 15,   7),   S( 10,   1),   S( -6,   2),   S( -2,  18),   S( 16,   1),
            S( 17,  36),   S(  7,  10),   S( 12,  17),   S(  7,  11),   S(  8,   0),   S(  3,  11),   S(  0, -10),   S(  2,   1),
            S( 13,   2),   S( 12,  17),   S(  4,  10),   S(  5,   1),   S( -4,  -3),   S(  1,  -4),   S(  8,  10),   S(  3,   3),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -1),   S( -7, -16),   S( -2,  -2),   S( -7, -22),   S( -4,  -9),   S( -5, -14),   S( -4,  -5),
            S(  8,  13),   S( -4, -10),   S(  6,   4),   S(  4,   5),   S(  8,  -1),   S( -1,  -4),   S( -2, -10),   S( -3,  -6),
            S(  2,  -5),   S(  3,   1),   S(  1,  -6),   S( 13,  15),   S( 13,  29),   S(  8,  26),   S( 16,  21),   S(  4,   4),
            S(  1,  -8),   S( 12,  13),   S( 12,  31),   S(-17,  -4),   S(  3,   8),   S( 17,   6),   S( 14,   3),   S(  9,  17),
            S( -2,  -9),   S( -1,  12),   S( -3,  22),   S( 21,  54),   S( 20,  25),   S( 12,  -1),   S(  9,   2),   S( -3,   1),
            S( -2,  19),   S(  6,  12),   S(  4,  26),   S(  7,  12),   S( 23,  20),   S(  8, -11),   S(  3,   9),   S(  1,  -3),
            S(  5,  -2),   S(  2,  17),   S(  8,  30),   S( 14,  18),   S( 10,  16),   S( -2,   7),   S(  0,  -8),   S(  0,   0),
            S(  3,  -3),   S( 11,  13),   S(  7,  -1),   S(  9,  10),   S(  5,  16),   S(  1,  -2),   S(  4,  10),   S(  4,  -1),

            /* rooks: bucket 0 */
            S(-20,  12),   S(  7, -13),   S(-12,   1),   S(-10,  16),   S(-30,  61),   S(-18,  37),   S(-47,  63),   S(-54,  46),
            S(  1, -21),   S( -6,  17),   S(-31,  22),   S(  0,  30),   S(  0,  44),   S( -3,  26),   S(-14,  13),   S(-22,  46),
            S( 21, -32),   S( 10, -12),   S(-11,  11),   S( -3,  15),   S(-29,  59),   S(-11,  18),   S(-16,  39),   S( -4,  17),
            S(  8, -19),   S( 35,  -4),   S(-32,  31),   S( 18,  17),   S( 15,  48),   S(-14,  45),   S(-20,  53),   S(-15,  32),
            S( 48, -58),   S( 41,  -1),   S( 18,  27),   S( 37,  24),   S( 45,  20),   S( 27,  66),   S( 37,  51),   S( 12,  59),
            S( 48, -28),   S( 55,  17),   S(100, -17),   S(104,  24),   S( 29,  53),   S( 35,  58),   S( 12,  67),   S(-37,  79),
            S( 16,  16),   S( 51,  44),   S( 91,  29),   S( 62,  13),   S( 61,  46),   S( 18,  61),   S( -9,  74),   S(-16,  66),
            S(  0, -23),   S( 27,  22),   S( 20,  20),   S( 41,  -5),   S( 24,  43),   S( 43,  11),   S( 33,  14),   S( 53, -46),

            /* rooks: bucket 1 */
            S(-52,  50),   S(-20,   5),   S(-13,  12),   S(-42,  28),   S(-39,  43),   S(-43,  48),   S(-48,  67),   S(-73,  76),
            S(-39,  36),   S(-22,  -5),   S(-20,  18),   S(-26,  25),   S(-27,  18),   S(-38,  45),   S(-18,  22),   S(-29,  54),
            S(-26,  26),   S(-11,  -4),   S(-10,   1),   S(-24,  16),   S(-26,  20),   S(-46,  37),   S(-56,  64),   S(-23,  61),
            S(-37,  47),   S( -8,  14),   S(-13,  26),   S(-28,  16),   S(-34,  35),   S(-45,  65),   S(-26,  60),   S(-63,  87),
            S(-16,  46),   S( 14,  -6),   S( 31,  10),   S( 27,   2),   S(  4,  24),   S( -4,  79),   S(  9,  62),   S( -5,  83),
            S( 42,  35),   S( 61,  -1),   S( 29,  14),   S(-11,  32),   S(  5,  23),   S(  9,  57),   S( 37,  44),   S( 11,  79),
            S(  9,  67),   S( 25,   5),   S(  3,  32),   S( 13,  13),   S( 44,  12),   S(  3,  50),   S( 27,  61),   S( 36,  78),
            S( 43,  -9),   S(  5,  -5),   S( -7, -10),   S(-24, -12),   S( 18,   6),   S( 17,  15),   S( 35,  27),   S( 52,  32),

            /* rooks: bucket 2 */
            S(-56,  69),   S(-45,  60),   S(-38,  53),   S(-38,  22),   S(-26,  22),   S(-40,  28),   S(-30,  17),   S(-67,  62),
            S(-45,  62),   S(-45,  55),   S(-42,  57),   S(-43,  34),   S(-46,  40),   S(-40,  21),   S(-18,   7),   S(-46,  40),
            S(-38,  65),   S(-26,  55),   S(-35,  43),   S(-33,  38),   S(-30,  26),   S(-31,  24),   S(-15,  10),   S(-11,  33),
            S(-29,  75),   S(-22,  67),   S(-42,  64),   S(-60,  52),   S(-44,  45),   S(-22,  30),   S( -9,  27),   S(-17,  47),
            S( -7,  85),   S(-15,  81),   S(  9,  68),   S(-15,  43),   S(-32,  55),   S( 29,  22),   S(  4,  42),   S( -1,  68),
            S( 21,  86),   S( 22,  73),   S( 33,  64),   S(-21,  53),   S( 36,  19),   S( 30,  50),   S( 99,   2),   S( 47,  66),
            S( 48,  63),   S( -8,  76),   S( 13,  53),   S( 25,  23),   S( -6,   8),   S( 20,  68),   S(-47,  89),   S( 28,  70),
            S( 14,  41),   S( 18,  47),   S( 24,  32),   S(-38,  26),   S(-47,  11),   S( 10,   9),   S(  5,  22),   S(-10,  51),

            /* rooks: bucket 3 */
            S(-15,  73),   S(-10,  69),   S(-12,  92),   S( -9,  83),   S(  1,  46),   S(  3,  39),   S( 19,  12),   S( -8,   5),
            S(  5,  62),   S( -9,  76),   S( -9,  98),   S(  1,  89),   S(  2,  55),   S( 16,  15),   S( 48, -11),   S( 19,   8),
            S( 17,  59),   S( -2,  84),   S( -5,  84),   S( -2,  92),   S( 19,  43),   S(  6,  34),   S( 37,  12),   S( 31,   9),
            S(  7,  89),   S( -2, 109),   S(-12, 110),   S( -1,  97),   S(  0,  69),   S( 16,  50),   S( 36,  30),   S(  6,  28),
            S(  8, 107),   S( -7, 120),   S( 23, 112),   S( 23, 103),   S( 17,  87),   S( 43,  61),   S( 67,  33),   S( 37,  46),
            S(  9, 123),   S( 27, 107),   S( 37, 114),   S( 51,  95),   S(103,  45),   S(123,  29),   S( 82,  39),   S( 38,  43),
            S( 20, 111),   S( 14, 110),   S( 28, 117),   S( 25, 112),   S( 30,  94),   S( 92,  47),   S(101,  93),   S(127,  65),
            S(117, -33),   S( 50,  40),   S( 14,  94),   S( 15,  78),   S( 16,  66),   S( 57,  58),   S( 34,  30),   S( 79,  10),

            /* rooks: bucket 4 */
            S(-28, -24),   S( 10, -16),   S(-26,  -6),   S(-38,  16),   S(-49,  16),   S(-34,  47),   S(-34,   4),   S(-79,  37),
            S(-28, -44),   S(-49,   0),   S(-18, -17),   S(  6, -30),   S( 24, -14),   S( -3,   6),   S(-19,  -1),   S(  4,  15),
            S(-18, -16),   S(-38, -20),   S(-37,  -4),   S( -8, -32),   S(-26,  -5),   S(-39,  19),   S(-17,  18),   S(-57,  21),
            S(-61, -30),   S(  7,   3),   S(  4, -19),   S( 10, -17),   S( 42,   3),   S( -5,  15),   S( -8,  -3),   S(-12,  11),
            S(-21, -35),   S( 23, -37),   S( 21,   3),   S( 46, -13),   S( 64,  -4),   S( 57,  25),   S( 19,  11),   S( 18,  25),
            S(-15, -37),   S(  9,  10),   S(  7,  -3),   S( 19,   9),   S( 30,  23),   S( 13,  13),   S( 33,  17),   S( 33,  37),
            S(-25, -21),   S( 31,  23),   S( 40,  -3),   S( 53, -10),   S( 62,  -4),   S(-10,  12),   S( 14, -12),   S( 24,   6),
            S( 10, -26),   S(  4,  13),   S( 27,  -8),   S( 26,  -8),   S( 54,   5),   S( 13,   4),   S(  5,   9),   S(  7,  14),

            /* rooks: bucket 5 */
            S(-19,  25),   S(-16,   7),   S(  1,  -6),   S( 24,  -5),   S(-13,  21),   S( -6,  29),   S(-25,  53),   S(-23,  35),
            S(  0,  -3),   S(-23, -12),   S( 45, -53),   S( 35, -20),   S(-15,   7),   S(-18,  14),   S(-32,  31),   S( -3,  29),
            S(-30,  25),   S( -5,  -1),   S(  5, -19),   S(  2, -13),   S(-18,   0),   S( 39, -13),   S(-38,  35),   S(-17,  20),
            S(-24,  27),   S( -1,   8),   S( 43, -25),   S( 25,  -4),   S( 33,  -5),   S( -9,  43),   S( 13,  32),   S(  8,  46),
            S( 35,  21),   S( 16,  10),   S(  9,  21),   S(  1,  -2),   S(-13,  23),   S( 68,  10),   S( 29,  35),   S( 51,  35),
            S( -5,  32),   S(-10,  12),   S(  2,   5),   S(-14, -13),   S( 18,  15),   S( 16,  26),   S( 60,  15),   S( 52,  30),
            S( 41,   7),   S( 37,  -5),   S( -7,   1),   S( 32,   6),   S( 53,  -4),   S( 51, -11),   S( 86, -13),   S( 44,  16),
            S( 20,  31),   S(  8,  10),   S( 49,  -6),   S(  2,  15),   S( 41,  18),   S( 23,  31),   S( 35,  39),   S( 60,  39),

            /* rooks: bucket 6 */
            S(-39,  45),   S(-25,  35),   S(-24,  30),   S(-32,  25),   S( -3,   9),   S(  7,  -2),   S( 22,  -7),   S(-31,  21),
            S(-29,  27),   S( 20,   8),   S(  1,  12),   S(  0,   3),   S( 18, -14),   S(-20,  -1),   S(-28,   2),   S( -2,  13),
            S(-41,  37),   S( 10,  17),   S( 11,   7),   S(  4,   5),   S(-11,  10),   S( 43, -15),   S(  3, -14),   S(  0,  -2),
            S(-30,  53),   S( -1,  39),   S( 18,  17),   S( 49,  -4),   S( 25,  -6),   S( 19,   4),   S(  4,   4),   S( 14,  33),
            S( -2,  52),   S( 60,  25),   S( 86,  20),   S( 57,  -1),   S( 18,  -4),   S( 27,  18),   S( 61,  -3),   S( 82,   6),
            S( 83,  10),   S( 84,  -1),   S( 80,   0),   S( 36, -14),   S(  2, -12),   S( 21,  31),   S( 29,  -3),   S( 56,  17),
            S( 57,  13),   S(126, -21),   S(102, -21),   S( 86, -31),   S( 27, -12),   S( 44,  -3),   S( 55,  -8),   S( 75, -17),
            S( 81,  -8),   S( 54,  15),   S(  5,  33),   S( 56,  -3),   S( 55,   3),   S( 28,  25),   S( 76,  10),   S( 56,  22),

            /* rooks: bucket 7 */
            S(-93,  35),   S(-72,  34),   S(-64,  35),   S(-56,  33),   S(-33,  -1),   S(-29, -17),   S(-36,   5),   S(-74, -14),
            S(-76,  33),   S(-25,   8),   S(-45,  20),   S(-54,  30),   S(-26, -13),   S(-20, -11),   S(  3,  -5),   S( -6, -56),
            S(-75,  34),   S(-60,  28),   S(-23,   6),   S(-28,  18),   S(-33,   2),   S(-24,  10),   S( 44, -33),   S(  1, -50),
            S(-66,  35),   S( -9,  15),   S(  2,  12),   S( 65, -22),   S(  9,  -2),   S( 62, -26),   S( 46,  -8),   S( 14, -21),
            S(  7,  25),   S( 33,  20),   S( 62,   9),   S( 88, -13),   S(137, -48),   S(110, -51),   S( 83, -21),   S(-53, -33),
            S( 32,  15),   S( 33,   1),   S( 96,  -7),   S( 88, -23),   S( 75, -13),   S( 35,   8),   S( 21,  32),   S( -3, -28),
            S(  8,  -2),   S( 41, -15),   S( 73, -15),   S(110, -43),   S(116, -44),   S(105, -42),   S( 44,   5),   S( 12, -29),
            S(-23, -14),   S(  9,   5),   S( 41,  -2),   S( 34,   1),   S( 48, -13),   S( 63,  -8),   S( 27,  12),   S( 15, -16),

            /* rooks: bucket 8 */
            S(-19, -81),   S(-17, -40),   S(-10, -14),   S( 17,   6),   S(-24, -28),   S(-20,   2),   S(-11, -29),   S(-20,   7),
            S(-33, -82),   S(-16, -45),   S(-23,   1),   S(-27, -67),   S(-25, -39),   S(-15, -22),   S(-11,  -6),   S(-38, -34),
            S(  0, -10),   S( -4, -13),   S( 11,  -4),   S(-11,  16),   S( -9,  47),   S( 12,  26),   S(  3,  49),   S(-18,   4),
            S( -5, -21),   S( -2,   2),   S( -2,  -3),   S( 14,  23),   S(  2,  41),   S( 31,  41),   S( -1,  21),   S(-10, -11),
            S(-11, -40),   S( 10,  20),   S(  7,  18),   S( 16,  38),   S(  8,  23),   S( -2,   4),   S( 13,  46),   S(  0,  22),
            S(-27,   8),   S(  1,  10),   S(-18,   9),   S( -6, -15),   S(  5,  34),   S(-15,  31),   S(  0,   4),   S(  3,  22),
            S(  1,  34),   S(  1,  23),   S(  3,   6),   S( 20,  13),   S( 16,  12),   S( 11,  30),   S(  6,  24),   S(  3,  43),
            S(-14,  15),   S(  2,  12),   S(-20,  32),   S( 35,  50),   S( -5,  22),   S( 12,  40),   S(  1,  24),   S(  8,  40),

            /* rooks: bucket 9 */
            S(-33, -65),   S(-13, -64),   S(-10, -100),  S(-16, -45),   S(-18, -47),   S(  0, -32),   S( -6, -20),   S( -3, -31),
            S(-62, -48),   S(-34, -70),   S(-29, -64),   S(-43, -49),   S(-40, -52),   S(-27,   5),   S(-22, -53),   S(-30, -31),
            S(-13, -13),   S(-23, -14),   S(  2,  -6),   S( -8, -33),   S( -8, -14),   S(  5,  18),   S(  1,   7),   S(  2,  15),
            S( -6,   4),   S(  4,  -6),   S(  2,   2),   S( -2,   5),   S(-13, -32),   S(  4,   1),   S( -7,  -2),   S(  4, -24),
            S( -4,   1),   S( -9, -13),   S(-10, -46),   S( -9,   1),   S(-21, -17),   S(-12,   3),   S(-11, -14),   S( -7, -10),
            S( -9,   5),   S(-32, -16),   S(-13, -21),   S( -2,  16),   S( -5,  -2),   S( -6,   9),   S( -3,  -1),   S(-12,   8),
            S(  7,  31),   S(  6,   3),   S(  4, -36),   S(  2,  11),   S(  7, -16),   S( 21,   5),   S(  6,  10),   S( -1, -12),
            S(-15,  14),   S(-19,  28),   S( -9,  15),   S( -5,  31),   S(-10,  31),   S(  6,  53),   S(  5,  20),   S( 14,  27),

            /* rooks: bucket 10 */
            S(-20, -27),   S(-54,  -8),   S(-31, -41),   S( -8, -51),   S(-15, -47),   S( -1, -79),   S(  3, -65),   S(-18, -41),
            S(-43, -13),   S(-31, -32),   S(-43, -24),   S(-40, -49),   S(-43, -46),   S(-25, -47),   S(-15, -35),   S(-47, -75),
            S( -9, -13),   S(-24, -15),   S(-30, -17),   S(-40, -43),   S(-11, -17),   S(  1, -15),   S(-12, -29),   S(-16, -15),
            S(-26,  -9),   S(-35, -34),   S( -6, -35),   S( -9,   2),   S(  4,   3),   S(  4,  10),   S(-11, -33),   S(  0, -35),
            S(  7,  -8),   S(  4, -10),   S(-14, -15),   S(-13, -33),   S(  5,  10),   S( -5,   1),   S( -9, -22),   S(-10, -33),
            S( -9,   3),   S( 12,   0),   S( -5, -17),   S( -4, -28),   S(  1,  -9),   S( -9,  -8),   S(-22, -31),   S( -2, -17),
            S( -8,  -9),   S(  9, -27),   S(  0, -20),   S( -3, -13),   S( 12, -19),   S(-10, -10),   S(-15, -30),   S( -5, -14),
            S( -5,  -3),   S(  9,  28),   S(  2,  35),   S(-11,  13),   S( -9,  31),   S(-27,   4),   S(-32,  15),   S(  0,  12),

            /* rooks: bucket 11 */
            S(-60, -15),   S(-37,  -1),   S(-50,  -7),   S(-27,  -5),   S(-47, -14),   S(-25, -17),   S(-18, -36),   S(-40, -64),
            S(-18, -14),   S(-24, -19),   S(-58, -11),   S(-54, -19),   S(-15, -24),   S(-14, -11),   S(-28, -31),   S(-46, -63),
            S(-32,  24),   S(-23,  11),   S( -7,  31),   S(-19,  16),   S(  5, -22),   S( -9,  -3),   S(  7, -19),   S(-12,  13),
            S(-24,  -7),   S(-12, -16),   S(-13,  12),   S(  7,  16),   S( 18,  12),   S(-21, -35),   S(  6,  16),   S( -8, -22),
            S( -8,  -8),   S(  7,  -5),   S(  4,   6),   S(  3,   7),   S( 34,  -8),   S( -1,  -5),   S( 16,  32),   S(-16, -44),
            S(  3, -16),   S(-11,  -5),   S( 15, -10),   S( 19,  -3),   S(-12, -17),   S(  1,   4),   S(  2,  30),   S( -6, -10),
            S( -4,   7),   S(-20, -27),   S( -3,  -1),   S(  1,   3),   S(  9,  -1),   S(  3,   8),   S( -1,  14),   S(-13,  -7),
            S( -6,   4),   S( 17,  33),   S(  3,  26),   S( 20,  24),   S(-10,   6),   S( -3,  25),   S( 11,  13),   S(-21,  24),

            /* rooks: bucket 12 */
            S(-33, -97),   S( -9, -14),   S(-21, -56),   S(-20, -37),   S(-12, -27),   S(  8,  -9),   S(-17, -41),   S(-19, -42),
            S(  2,   1),   S(  1,   4),   S(  8,  19),   S(  3,  12),   S(  7,   7),   S(  9,  -9),   S(  6,   8),   S(-18, -23),
            S( -6, -12),   S(  6,  34),   S( 11,  21),   S( 23,  22),   S(  5,  -7),   S( 15,  24),   S(  6,  34),   S( -3,  27),
            S(  7,  21),   S(  4,   3),   S( 15,  32),   S( 11,  20),   S( 12,   9),   S(  5,   8),   S(  6,  19),   S( -3,   5),
            S( 11,  17),   S( 12,  29),   S(  8,  46),   S(  3,   0),   S(  9,  27),   S( -2, -13),   S(  5,  16),   S(  6,  14),
            S( -3,   0),   S( -5,  -6),   S(  0,  16),   S( -5,   4),   S(  8,  25),   S(  0, -19),   S( 10,  27),   S(  4,  10),
            S(-16, -11),   S(-12,  18),   S(  7,  41),   S(  0,  22),   S( -2,   1),   S( 13,  18),   S(  3,  23),   S(  1,  24),
            S(  3,   5),   S(-11,  29),   S(  5,  31),   S( 13,  22),   S(  3,   6),   S(  1,  20),   S(  3,  11),   S(  3,  14),

            /* rooks: bucket 13 */
            S(-26, -24),   S(-26, -51),   S(-25, -51),   S(-18, -36),   S(-28, -52),   S( -4,  -2),   S(-27, -48),   S(-24, -37),
            S(-15, -11),   S( -8, -19),   S(  1,   6),   S( -2,  -3),   S( 17,  34),   S(  4,  12),   S(  7,   1),   S(-12, -12),
            S(-14,  -3),   S(-14,   6),   S( -5,  -8),   S(  7,   9),   S(  6,  26),   S( 14,  -2),   S( 11,  44),   S(-13, -27),
            S(  8,  16),   S( -2,   4),   S( -3,   8),   S(  5,  17),   S(  9,  21),   S( -1,   8),   S(  5,  14),   S(  2,  21),
            S(  6,  21),   S(  2,  -9),   S( -5, -21),   S(  2,   5),   S( -4,  24),   S(  0,  -2),   S(  5,   6),   S( -1,  -2),
            S(  1,  15),   S( -4,  -4),   S(-10,  -9),   S(-14,  -2),   S(-13, -12),   S(  3,  -2),   S( -7,   8),   S(  2,   4),
            S(  4,  -8),   S(  8,   5),   S( -9, -30),   S(  3,  16),   S( -8,  -4),   S(  7,  11),   S(  2,   4),   S(  0, -13),
            S(  3,  23),   S(-10,  14),   S( -4,   5),   S(  9,  24),   S( -3,  16),   S(  8,  24),   S(  0,  23),   S(  4,   4),

            /* rooks: bucket 14 */
            S( -6, -27),   S(-31, -28),   S(-18, -20),   S(-20, -56),   S(-13, -40),   S( -6, -22),   S(-33, -63),   S(-26, -34),
            S( -7,  25),   S(  3,  26),   S(  5,   9),   S( -1, -19),   S(  0,  -7),   S( -3,  -4),   S( -2,   4),   S( -6,  -5),
            S(  4,  30),   S( -3,  28),   S(  0,   2),   S(  2,   3),   S(  3,   7),   S( -1,  -5),   S(  1,  21),   S(-19, -48),
            S( -4,  14),   S( 16,  22),   S(  6,  17),   S(  9,   4),   S( -9,  -7),   S(  1, -11),   S(  8,  11),   S(-11, -17),
            S(  9,  18),   S( 21,  22),   S( -1,  -3),   S(  1,   7),   S(  2, -12),   S( 18,  31),   S(  0,   2),   S( -3, -16),
            S(  6,  13),   S(  8,  15),   S(  8,  19),   S(  2,   6),   S( -4,   6),   S(-16,   5),   S( -9,  -8),   S( -6,  -5),
            S( -5, -10),   S( 10,  17),   S( -7, -17),   S(-18, -31),   S( -5,   6),   S(  1,  -1),   S(-12, -12),   S( -7,  -7),
            S(  1,   1),   S(  5,   9),   S( -3, -15),   S(  6,  -7),   S(-11, -16),   S(-16, -41),   S(  2,  -5),   S(  2,  31),

            /* rooks: bucket 15 */
            S(-25, -46),   S(-18, -51),   S(-40, -50),   S(-25, -50),   S( -3, -23),   S(-14, -20),   S( -3,  -9),   S(-21, -53),
            S(  6,  28),   S(-11,   0),   S(-11,  -9),   S( -6, -10),   S( -6, -18),   S(  4,  -1),   S(  7,  10),   S(  3,   5),
            S(  5,   8),   S( -7, -14),   S( 11,  22),   S(  7,  -2),   S(  5,  -2),   S( -8, -14),   S(  6,  23),   S(  2,   6),
            S(  3,  11),   S( -1,  -6),   S( 18,  34),   S( -3, -11),   S(  4,  17),   S(  2,   6),   S(  5,  13),   S(  3, -13),
            S(  6,  15),   S(  6,  10),   S(  7,  -7),   S(  3,  13),   S(  6,  14),   S(  3,   1),   S( -2,  25),   S(  4, -11),
            S(  7,  17),   S(  8,   2),   S(  9,   1),   S(  4,   6),   S( -5, -14),   S( -4,  37),   S(  1,  20),   S(  4,   2),
            S(  4,  -2),   S( -2,   7),   S(  9,  21),   S(  5,  12),   S(  2,  15),   S(  5,  15),   S(-13,  11),   S(-10, -29),
            S(  1,  23),   S(  0,  25),   S(  9,  22),   S(  2,  28),   S(  0,   4),   S( -6, -24),   S( -6,  14),   S(-15,  -9),

            /* queens: bucket 0 */
            S( -4,  -7),   S(-24, -48),   S(-32, -54),   S( -2, -95),   S( -7, -50),   S( 10, -58),   S(-55, -28),   S(-15,  -9),
            S(-15, -28),   S( 13, -76),   S(  3, -65),   S( -7, -16),   S(  2, -16),   S( -7, -35),   S(-24, -28),   S(-37,  -9),
            S( -5,  10),   S( -3, -20),   S( 28, -49),   S(-10,   8),   S( -6,  22),   S(  0,   0),   S(-31,   0),   S(-74, -41),
            S(-23,  19),   S( 15, -24),   S( -8,  18),   S(-16,  68),   S( -6,  64),   S(-21,  36),   S(-39,  27),   S(-15, -23),
            S(-24, -20),   S( -1,  64),   S(  1,  32),   S( -2,  40),   S(  6,  65),   S(-17, 104),   S(-53,  70),   S(-40,   7),
            S(-14,   7),   S( 17,  32),   S( 27,  36),   S(-21,  71),   S(-22,  66),   S(-58,  99),   S(-62,  29),   S(-41,   7),
            S(  0,   0),   S(  0,   0),   S( 16,   0),   S(-32,  30),   S(-35,  25),   S(-63,  81),   S(-86,  61),   S(-97,  24),
            S(  0,   0),   S(  0,   0),   S(  4,  -9),   S(-15, -14),   S(-32,  22),   S(-37,   3),   S(-52,  -5),   S(-64, -28),

            /* queens: bucket 1 */
            S( 18,  -1),   S(  9,   3),   S( 16, -48),   S( 31, -87),   S( 38, -43),   S( 15, -25),   S( 15,  -6),   S(  2,  16),
            S(-20,  34),   S( 23,  17),   S( 39, -36),   S( 31,   4),   S( 42,  14),   S(  4,  21),   S(-18,  37),   S(-17,   9),
            S( 46,  -1),   S( 23,   4),   S( 20,  31),   S( 17,  73),   S( -4,  80),   S( 35,  45),   S( -1,  36),   S( 17,  -8),
            S( 37,   7),   S( 15,  42),   S( 19,  47),   S( 40,  70),   S( 20,  82),   S(  8,  58),   S(  8,  39),   S( -7,  57),
            S( 44,  -4),   S( 52,  15),   S( 49,  38),   S( 24,  33),   S( 47,  66),   S( 32,  27),   S( -4,  70),   S(  6,  91),
            S( 62,  -2),   S( 99,  11),   S( 86,  44),   S( 79,  54),   S( 51,  39),   S( 18,  64),   S( 43,  54),   S(  2,  54),
            S( 93, -25),   S( 53, -21),   S(  0,   0),   S(  0,   0),   S(  5,  40),   S( -8,  17),   S( -9,  51),   S(-38,  34),
            S( 71,  -7),   S( 53, -14),   S(  0,   0),   S(  0,   0),   S( 16,  20),   S( 40,  25),   S( 78,  -2),   S(-18,  32),

            /* queens: bucket 2 */
            S( 35, -13),   S( 31,  11),   S( 33,  21),   S( 45, -26),   S( 47, -32),   S( 32, -22),   S(  0, -20),   S( 36,  32),
            S( 25,   3),   S( 10,  49),   S( 38,  24),   S( 44,  35),   S( 55,   7),   S( 22,  25),   S( 25,  19),   S( 17,  47),
            S( 39,  10),   S( 31,  40),   S( 20, 104),   S( 16,  84),   S( 27,  78),   S( 24,  74),   S( 32,  49),   S( 33,  60),
            S(  3,  68),   S( 24,  83),   S( 19,  85),   S( 13, 122),   S( 34,  92),   S( 20,  95),   S( 34,  63),   S( 37,  79),
            S(  5,  86),   S( -7,  78),   S(  6,  92),   S( 32,  76),   S( 26,  95),   S( 93,  39),   S( 72,  54),   S( 64,  55),
            S(-12,  86),   S( -3,  81),   S(  2,  79),   S( 77,  35),   S( 40,  52),   S( 99,  68),   S(113,  38),   S( 46, 101),
            S( -1,  48),   S( -7,  41),   S( -1,  63),   S( 48,  25),   S(  0,   0),   S(  0,   0),   S( 21,  73),   S( 35,  66),
            S(  1,  31),   S( 36,  -6),   S( 47, -13),   S( 27,  35),   S(  0,   0),   S(  0,   0),   S( 45,  35),   S(  9,  57),

            /* queens: bucket 3 */
            S(-43,  32),   S(-30,  40),   S(-23,  40),   S(-14,  49),   S(-27,  35),   S(-15, -16),   S(-16, -38),   S(-40,  21),
            S(-57,  56),   S(-37,  47),   S(-25,  65),   S(-17,  85),   S(-15,  75),   S(-15,  37),   S( 15, -12),   S( 15, -30),
            S(-51,  79),   S(-38,  90),   S(-32, 113),   S(-41, 144),   S(-30, 125),   S(-22,  95),   S(-11,  58),   S(-14,  23),
            S(-42,  83),   S(-59, 138),   S(-51, 161),   S(-36, 171),   S(-41, 164),   S(-18,  97),   S( -5,  79),   S(-17,  65),
            S(-53, 123),   S(-44, 157),   S(-49, 173),   S(-42, 188),   S(-27, 153),   S( -2, 128),   S(-18, 123),   S(-21,  77),
            S(-62, 114),   S(-58, 159),   S(-58, 181),   S(-56, 190),   S(-51, 165),   S(  8,  96),   S(-24, 116),   S(-24, 108),
            S(-97, 124),   S(-96, 146),   S(-78, 182),   S(-70, 157),   S(-72, 157),   S(-16,  75),   S(  0,   0),   S(  0,   0),
            S(-127, 136),  S(-84, 101),   S(-71, 100),   S(-70, 109),   S(-58,  97),   S(-20,  54),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-36,  -4),   S(-46, -37),   S( -8,   0),   S(-10, -18),   S( -7,  -6),   S( -8,  11),   S(-33, -26),   S( 12,  20),
            S( -4, -12),   S( -9,   6),   S( -4,   1),   S(-14, -15),   S(-40,  18),   S(-17,  12),   S(-47, -11),   S( -1, -16),
            S(  5,  17),   S( 21, -31),   S( 14, -18),   S( 18,   7),   S( 44,   9),   S( 15,  20),   S(-19, -18),   S( 34,  24),
            S(-12, -22),   S( 17, -20),   S(  4,   1),   S(-11,  17),   S( 46,  28),   S(  4,  58),   S(-23,   6),   S(-12,  18),
            S(  0,   0),   S(  0,   0),   S( 15,  -9),   S( 56,  34),   S( 26,  56),   S( 33,  52),   S( 12,  16),   S( 14,  22),
            S(  0,   0),   S(  0,   0),   S( 16,  10),   S( 34,  18),   S( 40,  47),   S( 32,  49),   S( 20,  25),   S(  0,   6),
            S( 14,  -5),   S( 18,   8),   S( 62,  37),   S( 59,  37),   S( 56,  14),   S( 20,  28),   S(  5,  23),   S(-12,  22),
            S( 24,  -9),   S(-18, -33),   S( 24,   8),   S( 45,  19),   S( 15,   6),   S( 10,  21),   S(  0,   4),   S( 21,   8),

            /* queens: bucket 5 */
            S( 36,  23),   S( 25,   9),   S( 16,   6),   S(-10,  25),   S( 33,  -7),   S( 39,  46),   S( 11,  -2),   S( 19,   2),
            S( 20,  16),   S( 14,  -1),   S( 14,  -3),   S( 14,  13),   S( 12,  41),   S(-10, -12),   S( 27,  14),   S( 12,   4),
            S( 19,   3),   S( 46,  -4),   S( 23,  -1),   S(  7,  16),   S( 20,   7),   S( 31,  15),   S( 25,  40),   S( 17,  16),
            S(  8, -31),   S( 36,   3),   S( 23, -17),   S( 31,  13),   S( 60,   7),   S( 32,  14),   S( 35,  48),   S(  6,  32),
            S( 39,  -7),   S( 26, -40),   S(  0,   0),   S(  0,   0),   S( 10,  11),   S( 32,  16),   S( 41,  53),   S( 16,  34),
            S( 34,  12),   S( 33,   4),   S(  0,   0),   S(  0,   0),   S( 30,  20),   S( 61,  33),   S( 43,  36),   S( 50,  38),
            S( 69,   3),   S( 71,  11),   S( 51,  39),   S( 25,  25),   S( 50,  20),   S( 93,  45),   S( 65,  57),   S( 51,  30),
            S( 40,  27),   S( 52,  12),   S( 64,  20),   S( 43,  -3),   S( 55,  20),   S( 62,  37),   S( 67,  46),   S( 60,  31),

            /* queens: bucket 6 */
            S( 49,  51),   S(  3,   3),   S( 36,  17),   S( 33,  22),   S( 26,  14),   S( -5,   1),   S(  2,  12),   S(  7,  19),
            S( 27,  17),   S( 29,  30),   S( 61,  41),   S( 55,  28),   S( 40,  24),   S( 18,  14),   S(-12,  27),   S( 24,  32),
            S( -6,  47),   S( 39,  35),   S( 29,  37),   S( 51,  14),   S( 33,  12),   S( 47,  -1),   S( 62,  28),   S( 66,  58),
            S( 26,  36),   S(  9,  28),   S( 50,  13),   S( 92,  18),   S( 41,  -7),   S( 45,  10),   S( 79,   7),   S( 97,  44),
            S( 32,  53),   S( 35,  38),   S( 57,  39),   S( 50,  32),   S(  0,   0),   S(  0,   0),   S( 64,  21),   S(109,  52),
            S( 43,  48),   S( 58,  46),   S( 47,  54),   S( 27,   8),   S(  0,   0),   S(  0,   0),   S( 76,  45),   S(109,  40),
            S( 60,  36),   S( 26,  26),   S( 72,  19),   S( 59,  19),   S( 42,  37),   S( 67,  47),   S(128,  24),   S(136,   8),
            S( 37,  40),   S( 66,  24),   S( 71,  16),   S( 81,  36),   S(101,  13),   S( 95,  12),   S(108,  12),   S( 95,  26),

            /* queens: bucket 7 */
            S( -8,  25),   S( -8,   1),   S(-23,  23),   S( -7,  24),   S( 12,   5),   S(-12,   5),   S( -4,  15),   S(-14,  -9),
            S( -8,  26),   S(-45,  29),   S( -4,  51),   S(-10,  77),   S(-11,  41),   S(  6,  25),   S(  7,   2),   S(-31,  -6),
            S(  5,  25),   S(-12,  35),   S(-14,  88),   S( 38,  47),   S( 47,  29),   S( 27,   8),   S( 48, -26),   S( 48,  -5),
            S(-13,  24),   S( 19,  44),   S( 19,  69),   S( 46,  69),   S( 77,  45),   S( 68,  -3),   S( 78, -34),   S( 41,  -6),
            S( 16,  23),   S( -9,  61),   S( 18, 103),   S( 51,  81),   S( 88,  18),   S( 69,  -4),   S(  0,   0),   S(  0,   0),
            S(  1,  46),   S( -9,  88),   S( 10,  90),   S(  0,  85),   S( 57,  35),   S( 92,  49),   S(  0,   0),   S(  0,   0),
            S(-37,  60),   S(-23,  42),   S( 10,  58),   S( 34,  59),   S( 62,  39),   S( 77,  16),   S( 69,  22),   S( 62,  28),
            S( 35,  20),   S( 45,  33),   S( 52,  56),   S( 49,  23),   S( 51,  38),   S( 30,   2),   S(-12,   3),   S( 65, -11),

            /* queens: bucket 8 */
            S(-18, -37),   S(  0, -23),   S(-16, -44),   S( -4,  -9),   S(-17, -30),   S(  9,  -4),   S( -1, -11),   S(  1,   5),
            S(-21, -32),   S( -6, -15),   S(  2, -15),   S( -5, -11),   S(  8,  -3),   S( -5, -12),   S( -3,   2),   S( -1,   1),
            S(  0,   0),   S(  0,   0),   S( -1, -17),   S(-10, -46),   S(  5,   2),   S(  7,  -7),   S( -7,  -9),   S(  3,   6),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S( -3, -13),   S( -2,   0),   S(  3,  -2),   S( 11,  19),   S(  6,   3),
            S( -3, -12),   S(  7,  10),   S(  7,   1),   S( 12,  -7),   S(  7, -11),   S( 12,  12),   S( 13,  12),   S(-10,  -9),
            S(  1, -14),   S(  4, -17),   S( 15,  15),   S(  2, -20),   S( 12,   8),   S( 27,  35),   S(  8,  -4),   S( -2,  -4),
            S(-17, -37),   S(  1, -12),   S( 13,  11),   S( 26,  38),   S( 12,  11),   S( 17,  40),   S(  4,   6),   S(  5,   1),
            S(  2,   1),   S(  4,  -6),   S( 14,   9),   S(  9,  -2),   S( 18,  19),   S( -3,  -5),   S(  4,  11),   S(-16, -27),

            /* queens: bucket 9 */
            S(  9, -11),   S(-19, -34),   S(-14, -33),   S( 12,  -9),   S( -7, -35),   S( -2, -10),   S( -5,  -9),   S( -2, -14),
            S( -2,  -8),   S(-10, -20),   S(-11, -27),   S(  1, -15),   S(-23, -52),   S(-12, -32),   S(  6,  -3),   S(  1,  -9),
            S(-17, -45),   S(-14, -27),   S(  0,   0),   S(  0,   0),   S(  5,  -9),   S( 10, -10),   S( -5,  -9),   S(  5,  -4),
            S(  2,  -8),   S(-11, -29),   S(  0,   0),   S(  0,   0),   S(  0,  -4),   S( 10,   2),   S( 10,  10),   S( -2,   3),
            S( -8, -26),   S(  1, -13),   S(  0,  -6),   S(-10, -10),   S( -5, -28),   S( 12,  18),   S(  5,  -8),   S(  0, -15),
            S( 11,  11),   S( -1, -27),   S(  5,  -9),   S( -3, -18),   S(  0,  -9),   S(  5,   5),   S( -2, -11),   S( -2, -12),
            S(  9,   6),   S( 10,  -4),   S( -4,  -3),   S(  1,  10),   S( 24,  26),   S( 25,  29),   S(  8,  20),   S(  8, -11),
            S( 17,  -9),   S( 26,  17),   S(  0,  -6),   S( 20,  13),   S( 21,  17),   S(  6,  14),   S(  1, -18),   S( 14,   3),

            /* queens: bucket 10 */
            S( 16,  10),   S( 12,   9),   S( -1, -11),   S( -5, -26),   S(-10, -30),   S( -9, -18),   S( -4, -28),   S( -4, -15),
            S(  6,   3),   S(-13, -21),   S( -6, -24),   S(-18, -52),   S( -4, -10),   S( 11,   0),   S(-11, -28),   S( -6,  -7),
            S( -2,   1),   S(  3,   4),   S( -2,  -4),   S( -7, -18),   S(  0,   0),   S(  0,   0),   S(  2,  -6),   S(-12, -22),
            S( -4,  -9),   S(  3,   4),   S(  4,   3),   S(  8,   2),   S(  0,   0),   S(  0,   0),   S( -6, -15),   S(  0, -16),
            S( 12,  16),   S( 15,   5),   S(  4,  -4),   S( 30,  32),   S(  0,   1),   S( -1,  -1),   S(  1, -11),   S( 11, -26),
            S( -5,  -8),   S(  6,   7),   S( 23,  27),   S( 11,  13),   S( 15,  14),   S( 15,  22),   S( 16,   9),   S( -4, -22),
            S(  9,   6),   S( 19,  29),   S( 18,  27),   S( 22,  20),   S( 11,  18),   S( 25,  12),   S( 15,   9),   S(  6,  -4),
            S(-11, -30),   S(  4,   6),   S( 23,   8),   S( -5,  -1),   S( 14,  14),   S(  2,   1),   S( 14,   9),   S( 10,  -8),

            /* queens: bucket 11 */
            S(-10,  -3),   S( -3,  -1),   S( -7, -10),   S(-18, -19),   S( -6, -15),   S(-21, -34),   S( -8, -33),   S( -9, -16),
            S( -5,   0),   S(  1,   8),   S(-24, -11),   S( -7,   4),   S( 19,  -1),   S(-10, -27),   S(  7,  -2),   S( -6, -12),
            S(  3,   7),   S(  5,   1),   S(-19,  12),   S( -2,   2),   S( -3, -20),   S(-23, -30),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S( -7,  10),   S( -2,  11),   S( -1,   3),   S(  0,  -9),   S( -3,   6),   S(  0,   0),   S(  0,   0),
            S(  2,  12),   S( 15,  15),   S( 17,  24),   S(  4,  22),   S( 42,  46),   S( 17,  27),   S(  8,  -1),   S(-11, -29),
            S(  1,   4),   S(  1,   0),   S(  0,  12),   S( 12,  28),   S( 14,  19),   S(  1,   3),   S(  4, -10),   S(  4, -23),
            S(  3,   4),   S( 10,  12),   S( 16,  24),   S(  2,  13),   S( 19,  57),   S( 16,  13),   S(  5,   1),   S( 10,  -4),
            S(-16, -56),   S( 11,  14),   S( -7,  -7),   S(  6,  39),   S( 16,  32),   S( 12,   0),   S( -6,  -4),   S( 10,   0),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  1,   2),   S(-15, -19),   S( -6,  -6),   S(-12, -20),   S( -1,  -4),   S( -2,  -3),
            S(  0,   0),   S(  0,   0),   S(  5,   2),   S(-10, -18),   S( -8,  -9),   S(-11, -23),   S( -8, -16),   S(  2,   0),
            S( -6,  -9),   S(  4,   6),   S( -6,  -8),   S(-12, -36),   S( 16,  30),   S(  0,  12),   S( -2,  -7),   S(  8,   9),
            S( -9, -18),   S(  5,   3),   S(  7,  13),   S(  3,  12),   S(  1,   2),   S( -2,   9),   S( -2,  -2),   S( -3,  -8),
            S(-17, -29),   S(  3,   9),   S(  6,   3),   S(  6,   4),   S(  6,  27),   S( -6, -20),   S( -8, -17),   S( -1,  -1),
            S(  2,  -6),   S( -4, -11),   S(  0, -13),   S(  5,   9),   S( -5, -10),   S( -9,  -1),   S(-11, -10),   S( -2,  -7),
            S( -8, -11),   S(  4,   7),   S( -6, -11),   S( 13,  11),   S(  0,   0),   S( -9, -15),   S(  1,   0),   S( -7, -25),
            S(  6,  13),   S(  0,  -3),   S(  2,  -3),   S(  0,   3),   S( -5,  -6),   S(-13, -12),   S( -4,  11),   S( -8, -13),

            /* queens: bucket 13 */
            S(-22, -37),   S(-15, -30),   S(  0,   0),   S(  0,   0),   S(-17, -29),   S(-13, -35),   S(  0,  -2),   S( -4, -10),
            S(-16, -46),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(-16, -36),   S(-23, -44),   S(-12, -22),   S( -4,  -6),
            S(-21, -38),   S( -4, -14),   S( -4,  -5),   S( -2, -14),   S(-22, -41),   S(-10, -16),   S( -8,  -7),   S( -1,  -4),
            S( -8, -19),   S(-19, -30),   S(  0,  -8),   S( -6, -19),   S( 10,   4),   S( 17,  31),   S( -4, -15),   S( -8, -11),
            S(  5,  -9),   S(  1, -22),   S( -6, -20),   S( 12,  23),   S( -7, -12),   S( -1, -17),   S( -3,  -6),   S(  2, -11),
            S( -1,  -2),   S(-14, -18),   S(  5,   3),   S( 11,  22),   S(  1, -10),   S( -5,  -6),   S(-12, -22),   S(-10, -23),
            S(  0,   1),   S( -4,  -9),   S( 11,  24),   S( -2,  -2),   S(  3,   2),   S(  8,   1),   S(-13, -24),   S( -7, -10),
            S( -7,  -5),   S( -2,  -7),   S( -6, -13),   S(  1,  -7),   S(  3,  -1),   S( -1,  -2),   S(  0,  -8),   S(-12, -20),

            /* queens: bucket 14 */
            S( -7, -16),   S( -1, -10),   S( -9, -19),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S( -4,  -7),   S( -8, -23),
            S( -7, -23),   S(-26, -47),   S(-11, -25),   S( -4, -13),   S(  0,   0),   S(  0,   0),   S( -9, -24),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -6, -21),   S(-14, -26),   S( -3,  -5),   S(  2,   3),   S(-10, -16),   S(-17, -32),
            S( -9, -11),   S( -2,  -1),   S(  0,  -1),   S(-15, -22),   S( -8, -15),   S(-20, -29),   S( -3, -22),   S(  1,   0),
            S( -6, -12),   S( -5, -12),   S( -4, -15),   S(  6,   8),   S(  5,  20),   S(-10, -26),   S( -9,  -5),   S( -1,  -3),
            S( -6, -13),   S(  3,  -4),   S(-12, -20),   S(-13, -23),   S(  6,  10),   S(  1,   4),   S( -1,  -5),   S(-10, -10),
            S(-10, -16),   S( -2,  -9),   S(  0,   0),   S(  3,   7),   S(  3,   5),   S(  4,   6),   S( -8, -21),   S( -2,  -8),
            S(-11, -17),   S(  5,  -5),   S(-10, -14),   S( -3,  -9),   S(  3,   1),   S( -3,  -2),   S( -4,  -2),   S(  3,  -6),

            /* queens: bucket 15 */
            S(  1,   3),   S( -7, -18),   S(  4,   1),   S(-11, -18),   S(  4,   6),   S(-10, -11),   S(  0,   0),   S(  0,   0),
            S( -4,  -4),   S(  1,   6),   S(-13, -16),   S( -8, -18),   S(  0,  -7),   S(  2,   6),   S(  0,   0),   S(  0,   0),
            S( -1,   0),   S(  1,   0),   S(-12,  -4),   S( -6,  -6),   S(-10, -22),   S(  4,   3),   S( -1,   2),   S( -1,  -4),
            S( -2,  -5),   S(-10, -15),   S( -3,  -4),   S(  1,   6),   S(  9,  27),   S(  6,  27),   S( -3,   4),   S( -4, -14),
            S(  1,   3),   S(  1,   1),   S( -4,  -9),   S( -1,  -3),   S( 11,  51),   S(  4,  21),   S(  4,  12),   S( -6, -16),
            S( -1,  -3),   S( -3,  -2),   S( -4,  -8),   S( -6,  -1),   S( -1,   5),   S( -9,  -8),   S(  2,  12),   S( -8,  -5),
            S( -4, -12),   S(  0,   0),   S( -5,   4),   S(  3,   4),   S( -7,  -9),   S(  1,   6),   S(  5,  12),   S( -5, -10),
            S( -8, -17),   S(-13, -30),   S( -1, -10),   S(  3,   3),   S(-13,  -3),   S( -3,   0),   S(  1,  -1),   S( -3,   5),

            /* kings: bucket 0 */
            S(-10, -22),   S( 28,  -7),   S( 15,  -4),   S(-26,  15),   S(-11,  15),   S( 29, -25),   S(  1,   3),   S(  9, -50),
            S(-18,  31),   S( -1,   0),   S(  0,   3),   S(-43,  24),   S(-39,  41),   S(-13,  20),   S(-13,  35),   S( -3,  27),
            S( 13,   3),   S( 68, -30),   S(  4,  -5),   S(-20,   4),   S(-28,   4),   S(  2,  -6),   S(-28,  16),   S( 30, -29),
            S(-27, -25),   S( 10, -31),   S( 10, -29),   S(-23,   7),   S(-46,  33),   S(-47,  27),   S(-38,  37),   S(-15,  31),
            S(-49, -122),  S( -3, -48),   S(  0, -35),   S( 14, -22),   S(-48,  -6),   S(-30,  10),   S(-20,  11),   S(  0,  -9),
            S(-10, -121),  S(  0,   8),   S( -9, -56),   S(-13,  -7),   S( -2, -13),   S(-24,  19),   S( 17,  23),   S(-19,   8),
            S(  0,   0),   S(  0,   0),   S(  0, -51),   S(  4, -34),   S(-19,  -6),   S(-10, -15),   S(-28,   5),   S(-10,  -4),
            S(  0,   0),   S(  0,   0),   S(-12, -11),   S(  2,  -9),   S(  9,  -2),   S( -6,  12),   S(  7,   4),   S(  9,   0),

            /* kings: bucket 1 */
            S(  5, -26),   S( 30, -20),   S( 15, -15),   S( 28,  -3),   S( -2,  -1),   S( 32, -19),   S(  3,   5),   S( 17, -24),
            S( 10,  -1),   S(  6,   9),   S(  0,  -8),   S(-47,  27),   S(-30,  20),   S(-12,  15),   S( -4,  16),   S(  3,   9),
            S( -9, -15),   S(  0, -13),   S(  6, -17),   S( 13, -18),   S(-33,   0),   S( 16, -17),   S( 23, -11),   S( 38, -12),
            S( -2,  -1),   S(  4, -11),   S(  6,  -5),   S( -3,   6),   S( 12,   9),   S(-10,   2),   S( 31,  -6),   S(-20,  26),
            S(-18, -54),   S(-15, -45),   S( -8, -53),   S(-14, -42),   S( -1, -25),   S( -2, -29),   S(-10,  -4),   S( -5,  -3),
            S(-31,  -1),   S(-101,   3),  S(-32,  27),   S(  3,  21),   S(-41,   4),   S(-24,  14),   S( 15,   3),   S( -7,  -8),
            S(-35, -53),   S(-24,   4),   S(  0,   0),   S(  0,   0),   S(-41,  12),   S(-51,  28),   S( -5,  27),   S( -3, -33),
            S(-30, -113),  S(-12, -14),   S(  0,   0),   S(  0,   0),   S( -8,   8),   S(-13,  15),   S( -3,  20),   S( -4, -47),

            /* kings: bucket 2 */
            S( 12, -56),   S(  7,  -2),   S( 17, -20),   S( 16,  -9),   S( -1,   7),   S( 35, -23),   S( -4,  16),   S( 18, -26),
            S( 35, -36),   S(-13,  30),   S(-14,   8),   S(-18,   9),   S(-25,  14),   S(-13,   5),   S(  5,   0),   S(  2,   0),
            S(-30,  -4),   S(-18, -12),   S( -7, -11),   S(-10, -18),   S( -6,  -5),   S(  6, -19),   S( 30, -18),   S( 27, -17),
            S( 14,  13),   S(-15,  14),   S(  4,   3),   S(-21,  11),   S( 32,  -7),   S(-13, -10),   S( 34, -29),   S( 31,  -9),
            S( -5, -10),   S( 15, -15),   S( 25, -37),   S(  7, -30),   S( 32, -49),   S(-22, -41),   S( 23, -50),   S(  7, -46),
            S(  3,   7),   S(-10,  -7),   S(-38,   1),   S(-36, -13),   S(  3,   0),   S(-11,  26),   S(-82,   9),   S(-18, -19),
            S( -8, -11),   S( -8,  20),   S(-74,  13),   S(-17,   7),   S(  0,   0),   S(  0,   0),   S(-12,  16),   S(-35, -37),
            S( -7, -39),   S(-19, -27),   S(-30, -32),   S( -6,   8),   S(  0,   0),   S(  0,   0),   S(-10, -14),   S(-34, -122),

            /* kings: bucket 3 */
            S( -4, -53),   S( 15,  -7),   S( 28, -24),   S( -4,  -7),   S( -1, -12),   S( 36, -25),   S(  0,  15),   S(  6, -28),
            S(  3,  17),   S(-18,  38),   S(-16,   6),   S(-35,  17),   S(-52,  31),   S(  2,  -1),   S( -5,  17),   S(  3,  13),
            S( 18, -27),   S(  2,  -3),   S( -3,  -8),   S(-32,  -1),   S( -9,   9),   S( 25, -20),   S( 54, -21),   S( 57, -17),
            S(-19,  31),   S(-90,  45),   S(-54,  18),   S(-43,  14),   S(-30,  12),   S(-10, -23),   S(-35,  -4),   S(-33, -15),
            S(-13,   9),   S(-10,  -4),   S(-35, -10),   S(-21, -16),   S( 32, -44),   S( 54, -67),   S( 36, -70),   S( 11, -82),
            S(-11, -13),   S( 21,   6),   S( 21, -10),   S(  1, -25),   S( 47, -33),   S( 59, -51),   S( 72, -22),   S( 54, -117),
            S(-21, -11),   S( 26,  10),   S( 14, -12),   S( 29, -23),   S( 31, -29),   S( 27, -56),   S(  0,   0),   S(  0,   0),
            S( -5, -10),   S(  6,  10),   S( -3,  20),   S( 12, -11),   S( 10, -70),   S( -2,   9),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-57,   7),   S(  8,  34),   S(  7,  22),   S( 13,   1),   S(-10,   7),   S(  7, -11),   S(  4,   7),   S( 20, -36),
            S(-40,  24),   S( 20,  20),   S( -6,  18),   S( -6,   2),   S( 30,  -2),   S( 24,  -4),   S( 50, -15),   S( 14,  -4),
            S( -1,  26),   S( 13, -13),   S( 19,  -4),   S( -7,   2),   S(-19,  10),   S( 22, -22),   S(-38,   8),   S( 17, -13),
            S( -1, -22),   S(-11,   8),   S(  5,  16),   S(  4,   5),   S(-20,  10),   S(-12,  17),   S( 17,   9),   S( 12,   6),
            S(  0,   0),   S(  0,   0),   S( -1,   2),   S(-29,  12),   S(-37,  14),   S(-26, -16),   S(-20,   1),   S( -4,  -1),
            S(  0,   0),   S(  0,   0),   S(  6, -15),   S( -3,  25),   S(-11,  26),   S(-28, -11),   S(  5, -15),   S( -1,  17),
            S( -3, -20),   S( -4,  -6),   S( -4, -23),   S(  0,  21),   S( -6,  23),   S(-28,  -8),   S(-12,  21),   S(  4,  -4),
            S( -5, -23),   S(  2, -14),   S(-10, -20),   S( -8,   3),   S(  6,  11),   S( -7, -11),   S( -6,   0),   S(  4,  11),

            /* kings: bucket 5 */
            S( 31,  -3),   S(-12,  13),   S(-36,  23),   S(-42,  28),   S(-20,  26),   S( -1,  12),   S( 35,  -3),   S( 24,  -8),
            S( -4,   1),   S( 15,  10),   S( 25,  -4),   S( 24,  -6),   S( 16,  -4),   S( 37, -12),   S( 27,   3),   S( 46, -17),
            S(-12,   9),   S( -5,  -8),   S(-13,  -5),   S( -2,  -7),   S(  8,  -2),   S(-38,   0),   S( -4,   2),   S( 16,  -3),
            S( -2, -13),   S( -1,  -7),   S(  9,  -5),   S(  7,  17),   S(  1,  21),   S(  7,   3),   S( 15,   5),   S(  7,   5),
            S( -3, -29),   S(-30, -46),   S(  0,   0),   S(  0,   0),   S( -8,  -4),   S(-21, -14),   S(  4, -14),   S( -9,   4),
            S( -6, -41),   S(-24, -30),   S(  0,   0),   S(  0,   0),   S(-21,  37),   S(-54,  12),   S(-17,  -4),   S( -6,  -5),
            S(-16, -34),   S(-31,  21),   S(  2,   8),   S(  0, -18),   S(-28,  28),   S(-41,  19),   S( -1,   7),   S( 10,  18),
            S(-10, -101),  S( -9,  11),   S(-10, -27),   S( -3, -35),   S(-10, -18),   S( -6,   8),   S( -3, -16),   S(  0,   7),

            /* kings: bucket 6 */
            S( 35, -36),   S( 28, -13),   S( -4,   2),   S(-22,  22),   S(-11,  20),   S(-23,  20),   S( -2,  21),   S(  3,   3),
            S( 45, -28),   S( 10,  17),   S( 13,  -7),   S( 24,  -9),   S( 21,  -5),   S(-10,  12),   S( 12,   1),   S(  2,   2),
            S( 17, -18),   S(-25,   3),   S(-15,  -9),   S( -2,  -7),   S( 13, -11),   S(-43,   5),   S(  9,  -2),   S(-18,  14),
            S( 12,   6),   S( 26,  -4),   S( 18, -13),   S( 24,   5),   S( 60,   0),   S(-28,   4),   S( -6,   7),   S(  3,   0),
            S(  8, -19),   S( 16, -30),   S(-24, -11),   S( -1, -18),   S(  0,   0),   S(  0,   0),   S(-45, -21),   S(-40, -18),
            S(-16,  -1),   S(  4,  -1),   S(-29,  -2),   S(-10, -22),   S(  0,   0),   S(  0,   0),   S(-26, -15),   S(-27, -23),
            S(  0,  -9),   S( -9,   6),   S(-40,  11),   S(-16,  -3),   S(  3,   6),   S( -9, -31),   S(-29, -13),   S( -7, -38),
            S( -1,  -7),   S(  1,  -6),   S( -3,  10),   S(-14, -30),   S( -8, -38),   S( -5, -27),   S( -6,  -3),   S( -1, -60),

            /* kings: bucket 7 */
            S( 29, -34),   S( -8,  -3),   S(-27,  -3),   S(-14,  10),   S(-30,  12),   S(-43,  35),   S(-30,  34),   S(-42,  23),
            S( 11,  -2),   S( 21, -20),   S( -2,  -8),   S(-31,   7),   S(-12,   7),   S(-36,  22),   S(  3,  -4),   S( -4,  14),
            S( 29, -29),   S(-17,  -8),   S(-31,  -2),   S(-31,   0),   S(-44,   8),   S(-30,  13),   S( 14,  -4),   S(-46,  21),
            S(-23,  18),   S(  4,  10),   S( -7,   0),   S( 36,  -6),   S( 34,  -9),   S( 51, -28),   S( 18, -10),   S( 16,  -9),
            S(-16,  15),   S( -4,   1),   S(  1, -24),   S(  6, -17),   S( 14, -25),   S( 10, -22),   S(  0,   0),   S(  0,   0),
            S(-10, -32),   S( -2,  -7),   S( 15, -10),   S( 10,  -5),   S( 24, -10),   S( 17, -12),   S(  0,   0),   S(  0,   0),
            S( 14,  18),   S( -3, -19),   S(  2,   6),   S(-13, -12),   S(  6, -18),   S( -5, -28),   S(  5, -17),   S(-11,  11),
            S(  7,   7),   S( -8,  -8),   S( 11,  18),   S( -3,  -5),   S(  8,  16),   S(-18, -51),   S(  8, -11),   S(-11, -60),

            /* kings: bucket 8 */
            S( 14, 119),   S( -5,  84),   S( 39,  41),   S( -3,  -2),   S(-12,  13),   S(-14,  -5),   S( 32, -16),   S(-16, -20),
            S( 29,  72),   S( 24,  15),   S( 47,  61),   S( 82,  -2),   S( 17,  25),   S(  6,  -7),   S( -5,  10),   S(  2,  26),
            S(  0,   0),   S(  0,   0),   S( 29,  67),   S( 39,   5),   S( 19,   7),   S( -8,  -8),   S( -1,  14),   S(  9, -17),
            S(  0,   0),   S(  0,   0),   S(  3,  76),   S( -8,   1),   S(-17,  37),   S( -6,  18),   S( 14,  11),   S( 10,  14),
            S( -4, -26),   S( -1,  27),   S(  3,  13),   S(-16,  26),   S(-17,  -3),   S(  4, -16),   S(  1,  11),   S(-14, -27),
            S(  5,  14),   S(  0, -15),   S( -3, -14),   S( -7,   1),   S(-13,   2),   S(-11,  -2),   S( -9,  -2),   S(  9,  -8),
            S( -5, -15),   S( -8, -12),   S(  5,   9),   S( -1, -10),   S( -3, -31),   S(-11,   6),   S( -3,   0),   S(  5, -45),
            S( -7,  -9),   S(-10, -26),   S( -2, -11),   S( -6, -22),   S(  6,   7),   S( -5,   2),   S(  0,  -5),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  6,  27),   S(-15,  34),   S(-19,  57),   S( 15,  10),   S(-18,  33),   S(-26,  28),   S( 40,   4),   S( 19,  14),
            S(-19,  35),   S( 35,  24),   S(  4,   0),   S( 48,   2),   S( 57,  18),   S( 24,   6),   S( -6,  28),   S(-16,  13),
            S( -6,  12),   S( 22,  13),   S(  0,   0),   S(  0,   0),   S( 46,  17),   S( -1,   3),   S( 10,  -1),   S(-19,  22),
            S( -1, -31),   S( 12, -22),   S(  0,   0),   S(  0,   0),   S(  6,  35),   S( 14,   0),   S(-11,  10),   S(-15,  29),
            S(  4, -21),   S( 11,  -2),   S(  4,  16),   S(  0,  13),   S(-13,  17),   S(-20,  15),   S( -9,  13),   S(  0, -16),
            S(  6,   3),   S(  1,  -6),   S(  7,  -9),   S(-10, -20),   S(-13,  13),   S(  0,   9),   S(-32,   2),   S(  5,  32),
            S(  2,  -7),   S( -2, -20),   S(  0, -10),   S(  2, -30),   S( 14, -28),   S( 14,  16),   S(-17,  -8),   S(  4,   4),
            S(  7,   5),   S( -2, -22),   S( 10, -24),   S( -4, -21),   S( -1, -18),   S(  3,  10),   S( -7,  12),   S(  8,  -1),

            /* kings: bucket 10 */
            S( 34,  -1),   S(  3,  -8),   S(  5,  10),   S(  6,  24),   S(-13,  20),   S(-92,  50),   S(-31,  46),   S(-88,  84),
            S(  4,  -1),   S( 62,   1),   S( 23,  -5),   S( 32,  10),   S( 57,  13),   S( 47,   4),   S( 11,  27),   S(-87,  50),
            S( 15,   7),   S( 28,   0),   S( 26, -11),   S( 13,  14),   S(  0,   0),   S(  0,   0),   S( -8,  23),   S(-59,  28),
            S( 15,   6),   S( 43, -27),   S( 36, -33),   S( 29,   4),   S(  0,   0),   S(  0,   0),   S( -1,  14),   S(  7,   2),
            S(  3,   6),   S( 26,   6),   S( 30, -21),   S(  9, -30),   S(  4, -18),   S(  7,  25),   S(  9,   8),   S( -9,  16),
            S(  4,  14),   S(  4,  -7),   S( -3,   6),   S( 10,  -7),   S(  7,  -1),   S(-18,  -6),   S(-12,   5),   S(  0,  -7),
            S(  0, -42),   S( -3, -16),   S(  9, -10),   S( 13,   1),   S( 11,   0),   S( -9, -19),   S(  4, -27),   S(  5,   4),
            S(  4,   6),   S( 11, -12),   S( -2, -14),   S(  0,   3),   S(  5, -14),   S(  0, -31),   S( -5,  -8),   S(  9,   3),

            /* kings: bucket 11 */
            S( -6, -20),   S(  9,   8),   S(  6, -10),   S( -5,  15),   S( -7,   6),   S(-67,  58),   S(-74,  81),   S(-126, 151),
            S( -2, -26),   S( 21,   4),   S(-12, -17),   S( 17,  22),   S( 84,   0),   S( 56,  41),   S(  9,  22),   S( 25,  41),
            S(  2, -50),   S( -2,  18),   S(  0, -11),   S( 23,  12),   S( 61,   1),   S( 26,  62),   S(  0,   0),   S(  0,   0),
            S(  1,  20),   S( 19,  12),   S( -5,   4),   S( 10,  15),   S( 27,  -9),   S( 22,  24),   S(  0,   0),   S(  0,   0),
            S(  0,  35),   S(  2,  -5),   S(  8,  -8),   S( 13, -16),   S( 15,   2),   S( -1,  -1),   S(  8,  10),   S(  7,   1),
            S( 11,   9),   S(  0, -14),   S( 15, -12),   S(  0,   5),   S( -5,  -7),   S(  2, -17),   S( -5,  -8),   S(-11,  -4),
            S(  6,  12),   S(  8,  -6),   S( 18,  23),   S(  1, -25),   S( 17, -17),   S(  3,   3),   S(-10, -11),   S( -7, -14),
            S(  5,   8),   S(  5,   0),   S(-12, -22),   S(  4,  -7),   S( -4, -19),   S( -8, -17),   S(  0, -19),   S(  5,  12),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 19,  59),   S(  6,  -6),   S(  0,  -2),   S(  7,  14),   S(  7,  -1),   S(-20,   6),
            S(  0,   0),   S(  0,   0),   S( 47, 110),   S( 28,  13),   S( 21,  44),   S( 12,  -2),   S( 23,  -5),   S(-18,   0),
            S( -1,   9),   S(  3,  13),   S( 23,  73),   S( 39,  20),   S(  8,  -8),   S( 10,   2),   S(  2, -13),   S( -9,  -2),
            S( -2,   9),   S(  9,  32),   S(  0,  17),   S(  4,  -5),   S( -8,   1),   S( -1,  20),   S( -3,  10),   S(  1,   7),
            S(  9,  17),   S(  6,  23),   S( 10,  19),   S( -3,  41),   S( -4,  40),   S(  0,   3),   S( -9,  15),   S(-12, -11),
            S(  5,   6),   S(  9,  16),   S( -2,  -2),   S(-10, -15),   S( -1,   5),   S( -8,  16),   S( -9, -15),   S(  7,  -1),
            S(  3,   8),   S( -7, -13),   S( -1,   6),   S( -1,   1),   S( -5,  -9),   S(  4,   8),   S(  8,  43),   S( -1, -29),
            S( -3,   2),   S(  6,   3),   S( -4,   7),   S(  0,   3),   S( -1,  -5),   S(  3,   9),   S(-11, -22),   S( -3, -22),

            /* kings: bucket 13 */
            S( -1,  53),   S(  7,  32),   S(  0,   0),   S(  0,   0),   S( 43,  17),   S( 13, -12),   S( -2,  -7),   S(-18,  25),
            S(  2,  22),   S( -1,  -1),   S(  0,   0),   S(  0,   0),   S( 46,   6),   S( 27,  -7),   S(-20,   6),   S(-13,   5),
            S( -3,   3),   S( 19,  23),   S(  2,  -7),   S( 14,  40),   S( 51,  13),   S( 22,  -6),   S(  2,   8),   S( 12, -10),
            S(-10,  -6),   S( 15,  -2),   S(  1,  21),   S( -6,  17),   S( -4,  15),   S(  4, -10),   S(  5,  22),   S(-15, -26),
            S(  6,  11),   S( -1,   6),   S(  5,  44),   S( -5,  24),   S( -9,  11),   S(  6,  18),   S(-11,   1),   S(  8,  10),
            S(  3,   0),   S( -5,  17),   S( -2,  17),   S( -5,  -1),   S(-12, -18),   S( -5,   9),   S( -9,  19),   S(  1,   0),
            S(  9,  11),   S( -8, -22),   S(-11, -45),   S(  4,  21),   S(-10, -10),   S(-10,  16),   S(-14, -25),   S(  6,  13),
            S(  1,  -2),   S(  5,  -4),   S(  4,  20),   S(  3,   4),   S(  0,  18),   S(-11, -17),   S( -3,   8),   S(  8,  15),

            /* kings: bucket 14 */
            S( 18,  34),   S(  0,  -6),   S( 11, -41),   S( 16,   0),   S(  0,   0),   S(  0,   0),   S(  7,  71),   S(-42,  40),
            S(-10,  -8),   S( 18,  -8),   S( 46, -34),   S( 41,  12),   S(  0,   0),   S(  0,   0),   S( 13,  32),   S(-44,   6),
            S(  4,   5),   S( 15,  -5),   S( 34, -32),   S( 40,   5),   S( 10,  -3),   S( 14,  36),   S( 27,  56),   S(-28,   3),
            S(  8,  -6),   S(  8, -10),   S( -1, -10),   S( 11,   1),   S(-20,   0),   S( 15,  56),   S(  3,  24),   S(  7,  -3),
            S(  7,  18),   S(  9,  -1),   S( -9,   1),   S(-17,   9),   S(  1,  29),   S(  4,  55),   S(  3,  38),   S(  6,  13),
            S( -5,  -6),   S(  2,   5),   S( -2,  -1),   S(  0,  10),   S( -6, -19),   S( -6,  -2),   S(-15,  -6),   S( -1,   6),
            S(  4,   9),   S(-10, -14),   S( 10,  -7),   S( 16,   5),   S(  3,  -2),   S( -6,  18),   S(-26, -21),   S(  8,  17),
            S(  1,  12),   S(  5,  -9),   S(  9,   2),   S( -4,  -6),   S(  7, -11),   S( -3,  -5),   S(-13, -25),   S(  0, -10),

            /* kings: bucket 15 */
            S( 11,  31),   S(  6,  -1),   S( 11,  -7),   S( -8,   0),   S(-10, -10),   S(  0,  59),   S(  0,   0),   S(  0,   0),
            S( -2, -23),   S(  7, -11),   S( -7, -14),   S( 19,  51),   S( 39,   0),   S( 61, 111),   S(  0,   0),   S(  0,   0),
            S( -9, -23),   S( 17, -10),   S(  7, -17),   S( -4,  14),   S(  9,  -5),   S( 26,  71),   S(  9,  43),   S(-13,  -3),
            S( -1, -12),   S(  4,  15),   S(  3,  15),   S(-13, -28),   S(-12,  -2),   S( 20,  49),   S( 17,  47),   S( -3, -12),
            S( 10,   5),   S( -7,  25),   S(  0,  -4),   S( -5, -35),   S( -3,   8),   S(  2,  35),   S(  4,   8),   S(  3,   3),
            S(  5,  27),   S(-15,  -4),   S(  8,  15),   S(  8,  19),   S(-10, -23),   S( -2,   7),   S(  1,   5),   S(  4,  16),
            S(  8,  12),   S( -3,  24),   S( -2, -11),   S(  4,   7),   S(  9,   6),   S(  9,  16),   S( -5,  -2),   S(  2,   1),
            S( -2,  -7),   S(  4,   1),   S( -2, -11),   S(  4,   4),   S(  4,   4),   S( 10,  14),   S(  0,  -7),   S(  2,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-41,  52),   S(-17, -16),   S(  3,  66),   S( 16, 101),   S( 26, 122),   S( 34, 146),   S( 42, 149),   S( 55, 141),
            S( 69, 119),

            /* bishop mobility */
            S(-33,  27),   S(-11,   0),   S(  6,  47),   S( 15,  90),   S( 24, 114),   S( 29, 134),   S( 32, 145),   S( 36, 148),
            S( 40, 149),   S( 47, 143),   S( 57, 133),   S( 78, 121),   S( 85, 116),   S( 58, 116),

            /* rook mobility */
            S(-104,  11),  S(-31,  13),   S(-15,  92),   S(-13, 120),   S(-13, 151),   S( -8, 163),   S( -1, 173),   S(  6, 175),
            S( 13, 187),   S( 19, 191),   S( 22, 197),   S( 30, 196),   S( 42, 198),   S( 50, 200),   S( 93, 172),

            /* queen mobility */
            S( 81, 162),   S(-24, 332),   S( 25, 212),   S( 39, 114),   S( 49, 122),   S( 49, 184),   S( 51, 223),   S( 53, 257),
            S( 54, 288),   S( 55, 313),   S( 57, 330),   S( 60, 341),   S( 60, 350),   S( 60, 364),   S( 62, 366),   S( 62, 368),
            S( 65, 368),   S( 69, 361),   S( 76, 347),   S( 91, 331),   S(101, 314),   S(144, 272),   S(149, 262),   S(173, 228),
            S(185, 213),   S(182, 193),   S(121, 186),   S(109, 139),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,  13),   S(-20,  47),   S(-31,  44),   S(-39,  61),   S( 10,  11),   S(-10,  12),   S(  0,  56),   S( 25,  27),
            S( 15,  33),   S( -1,  45),   S(-16,  44),   S(-19,  37),   S( -2,  34),   S(-24,  40),   S(-25,  54),   S( 33,  26),
            S( 24,  67),   S( 14,  71),   S(  6,  53),   S( 21,  45),   S( -2,  49),   S(-23,  62),   S(-28,  93),   S( -5,  75),
            S( 32, 106),   S( 41, 119),   S( 20,  80),   S(  8,  61),   S(  5,  64),   S( -2,  87),   S(-45, 123),   S(-75, 149),
            S( 21, 151),   S( 49, 182),   S( 52, 128),   S( 28, 111),   S(-57, 104),   S( 16, 106),   S(-59, 171),   S(-88, 170),
            S( 90, 230),   S( 79, 268),   S(127, 239),   S(125, 250),   S(130, 262),   S(151, 240),   S(131, 251),   S(132, 262),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,   1),   S( -7, -27),   S( -4, -10),   S(  3,  -8),   S( 14,   8),   S(-16, -37),   S(-24,   9),   S( -2, -48),
            S(-17,  17),   S( 23, -20),   S(  1,  27),   S( 11,  24),   S( 32, -11),   S( -5,  16),   S( 25, -18),   S( -5,  -7),
            S(-13,  16),   S( 15,   6),   S(  3,  41),   S( 17,  51),   S( 25,  28),   S( 34,  16),   S( 29,   1),   S(  0,  14),
            S( 15,  35),   S( 14,  52),   S( 37,  94),   S( 13, 101),   S( 67,  68),   S( 68,  57),   S( 19,  60),   S( 22,  23),
            S( 50,  94),   S( 89, 116),   S(102, 140),   S(140, 164),   S(138, 134),   S(137, 148),   S(131, 124),   S( 51,  61),
            S( 72, 195),   S(117, 279),   S(102, 222),   S( 96, 197),   S( 66, 152),   S( 48, 140),   S( 41, 144),   S( 16,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  21),   S( 16,  21),   S( 30,  33),   S( 32,  21),   S( 20,  19),   S( 22,  24),   S(  3,  11),   S( 39,  -3),
            S( -5,  23),   S( 18,  35),   S( 12,  34),   S(  9,  40),   S( 23,  14),   S( 12,  20),   S( 32,  19),   S(  0,  12),
            S(  0,  22),   S( 28,  49),   S( 53,  57),   S( 39,  58),   S( 44,  54),   S( 70,  19),   S( 30,  35),   S( 21,   6),
            S( 57,  72),   S(104,  57),   S(122, 123),   S(147, 129),   S(141, 118),   S( 78, 131),   S( 72,  57),   S( 73,   9),
            S( 47, 125),   S( 92, 142),   S(152, 212),   S(108, 253),   S(135, 263),   S( 84, 240),   S(158, 207),   S(-54, 172),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26,  32),   S( 11,  18),   S( 13,  33),   S(-14,  63),   S( 65,  22),   S( 19,  10),   S( -2,   2),   S( 30,  12),
            S( -1,  14),   S(  5,   8),   S( 16,  17),   S( 12,  30),   S(  6,  19),   S( -1,   8),   S(  4,   6),   S( 27,  -5),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1, -14),   S( -5,  -8),   S(-16, -17),   S(-12, -30),   S( -6, -19),   S(  1,  -8),   S( -4,  -6),   S(-27,   5),
            S(-26, -32),   S(-11, -18),   S(-13, -33),   S( 14, -63),   S(-65, -22),   S(-19, -10),   S(  2,  -2),   S(-30, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -39),   S(-14, -42),   S(-16, -49),   S(-62, -33),   S(-26, -47),   S(-29, -48),   S( -8, -49),   S(-26, -61),
            S(-26, -23),   S(-20, -31),   S(-31, -16),   S( -6, -38),   S(-38, -38),   S(-26, -28),   S(-38, -22),   S(-13, -44),
            S(-19, -20),   S( -8, -36),   S(-25, -14),   S(-31, -26),   S(-20, -45),   S(-21, -24),   S(-10, -24),   S(-41, -31),
            S( -7, -34),   S( 17, -46),   S( 13, -21),   S(  9, -31),   S( 10, -32),   S( 58, -46),   S( 39, -47),   S(-13, -55),
            S( 14, -50),   S( 40, -75),   S( 47, -31),   S( 61, -33),   S( 77, -53),   S( 82, -40),   S(136, -96),   S( 33, -83),
            S( 95, -100),  S(126, -110),  S( 91, -51),   S( 72, -31),   S( 68, -33),   S(121, -47),   S( 97, -46),   S( 46, -87),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-12, -25),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S( 19,  -3),        // attacks to squares 1 from king
            S( 15,   1),        // attacks to squares 2 from king

            /* castling available */
            S( 67, -63),        // king-side castling available
            S( 15,  64),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 41, -89),   S( 45, -78),   S( 39, -88),   S( 30, -74),   S( 23, -63),   S( 14, -57),   S( -3, -45),   S( -6, -40),
            S(  8, -44),   S( 27, -45),   S( 54, -42),   S( 24, -27),   S(100, -49),

            /* orthogonal lines */
            S(-35, -148),  S(-90, -113),  S(-113, -92),  S(-130, -87),  S(-136, -89),  S(-142, -90),  S(-141, -96),  S(-136, -102),
            S(-148, -93),  S(-165, -89),  S(-162, -102), S(-115, -134), S(-86, -145),  S(-41, -153),

            /* pawnless flank */
            S( 39, -35),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 27, 230),

            /* passed pawn can advance */
            S(-11,  34),   S( -3,  61),   S( 15, 103),   S( 85, 169),

            /* blocked passed pawn */
            S(  0,   0),   S( 50, -25),   S( 28,  -4),   S( 23,  33),   S( 24,  62),   S( 18,  34),   S( 64,  64),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 51, -51),   S( 43,  15),   S( 17,  27),   S( 12,  59),   S( 28,  95),   S(130, 127),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-16, -17),   S( -6, -34),   S(  1, -31),   S(-23,  -7),   S(-30,  23),   S(116,  15),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 25, -14),   S( 27, -17),   S(  6,  -6),   S(  4, -41),   S(-14, -115),  S(-41, -205),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 23,  51),   S( 46,  27),   S( 93,  45),   S( 24,  28),   S(167, 116),   S(102, 124),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 13,  54),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-27, 119),

            /* bad bishop pawn */
            S( -6, -15),

            /* rook on open file */
            S( 26,   1),

            /* rook on half-open file */
            S(  3,  39),

            /* pawn shields minor piece */
            S( 13,  11),

            /* bishop on long diagonal */
            S( 25,  50),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 22,  34),   S( 24,   2),   S( 33,  19),   S( 28,  -1),   S( 35, -22),

            /* pawn threats */
            S(  0,   0),   S( 67, 106),   S( 52, 122),   S( 74,  89),   S( 61,  41),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  61),   S( 51,  47),   S( 76,  42),   S( 50,  64),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 25,  52),   S( 29,  44),   S(-16,  42),   S( 65,  67),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 20,  11),   S( 17,  34),   S( 31,  12),   S(  3,  32),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  15),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
