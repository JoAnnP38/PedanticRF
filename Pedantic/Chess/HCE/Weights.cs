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

        // Solution sample size: 16000000, generated on Mon, 13 May 2024 02:05:18 GMT
        // Solution K: 0.003850, error: 0.082149, accuracy: 0.5145
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 79, 223),   S(386, 668),   S(408, 664),   S(545, 1077),  S(1384, 1804), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(106, -126),  S(150, -95),   S( 44, -45),   S(-24,  29),   S(-26,  13),   S(-23,   2),   S(-51,   6),   S(-29, -14),
            S(127, -131),  S(109, -110),  S( 13, -65),   S( -9, -54),   S(-18, -15),   S(-21, -25),   S(-37, -21),   S(-22, -40),
            S(114, -107),  S( 64, -65),   S( 14, -65),   S( 12, -68),   S( -9, -60),   S(  2, -54),   S(-13, -49),   S(  3, -51),
            S( 74, -43),   S( 53, -60),   S( 29, -62),   S( 16, -80),   S(-16, -41),   S(-16, -52),   S(-21, -39),   S( -8, -23),
            S( 79,  34),   S( 35, -10),   S( 42, -29),   S( 52, -71),   S( 21, -42),   S( -9, -37),   S(-25,  -3),   S(-32,  55),
            S( 66,  56),   S( 50,  76),   S(  6,   8),   S( 17, -17),   S(-44,  -1),   S(  2,   6),   S( -4,  22),   S( 13,  50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 34, -30),   S( 35, -38),   S( 55, -24),   S(  3,  23),   S( -3,  -9),   S(  6, -11),   S(-41,   4),   S(-28,  23),
            S( 38, -45),   S( 27, -45),   S( 17, -48),   S(  3, -43),   S( -9, -22),   S( -6, -28),   S(-34, -12),   S(-35,  -9),
            S( 31, -41),   S( 11, -30),   S( 16, -55),   S( 12, -55),   S(-21, -25),   S( 12, -48),   S(-11, -31),   S(  0, -22),
            S( 45, -23),   S( 20, -52),   S( 27, -57),   S(  6, -50),   S(-12, -21),   S( 13, -45),   S(-25, -23),   S( -9,   5),
            S( 28,  46),   S(-32,   2),   S( -2, -36),   S( 13, -49),   S( 38, -36),   S( -7,  -6),   S(-27,  24),   S(-24,  73),
            S( 55,  58),   S( 16,   2),   S(-46, -19),   S(-22,  24),   S(-21,  -7),   S(-61,  27),   S(-46,  31),   S(-39,  89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  -1),   S(-19,   2),   S( -5,  -1),   S(  1,   6),   S( 17, -13),   S( 36, -18),   S( 12, -44),   S(  3, -18),
            S(  0, -27),   S(-25, -14),   S(-18, -33),   S(-12, -33),   S( 11, -33),   S( 13, -33),   S( -1, -40),   S(-13, -28),
            S( -8, -24),   S(-22, -28),   S( -9, -53),   S(  0, -53),   S( -3, -31),   S( 24, -45),   S(  4, -41),   S( 12, -31),
            S(-12,  -8),   S( -9, -48),   S(-13, -52),   S( -3, -55),   S(  8, -46),   S(  5, -31),   S(  2, -24),   S(  5,  -8),
            S( -3,  37),   S(-43,  -5),   S(-39, -41),   S(-43, -33),   S( 14,  -8),   S(-10,   4),   S(-20,  22),   S(-18,  77),
            S(-54,  81),   S(-92,  58),   S(-95,  -2),   S(-69, -17),   S(-40,   6),   S(-21,  21),   S( -7,  -4),   S(-18,  75),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -18),   S(-25,  -3),   S(-22,  -4),   S( 14, -48),   S( -2,  -3),   S( 53, -25),   S( 92, -72),   S( 73, -86),
            S( -5, -43),   S(-25, -29),   S(-18, -43),   S(-14, -28),   S( -5, -29),   S( 20, -42),   S( 63, -77),   S( 65, -79),
            S( -4, -48),   S( -7, -58),   S( -4, -65),   S(  1, -67),   S(  0, -56),   S( 25, -58),   S( 40, -68),   S( 82, -77),
            S( -2, -33),   S(  3, -74),   S(  2, -77),   S(  4, -73),   S( 22, -75),   S( 23, -66),   S( 33, -53),   S( 72, -35),
            S( 26,   9),   S( -8, -34),   S( 11, -74),   S( 15, -69),   S( 88, -67),   S( 74, -42),   S( 59,   5),   S( 58,  61),
            S(-31, 104),   S(-23,  13),   S( -5, -47),   S( -7, -67),   S( 65, -76),   S( 63, -22),   S( 63,   1),   S( 68,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-91,  22),   S( -9, -13),   S(-32,  11),   S( -9,  24),   S( -9, -19),   S(-48,  27),   S(-48,   2),   S(-45,   8),
            S(-16,   5),   S( 44, -22),   S( 31, -40),   S( 15, -26),   S( -6, -20),   S(-51, -16),   S(  0, -41),   S(  4, -26),
            S( 42, -20),   S( 44, -18),   S(-14,   5),   S(  4, -30),   S(-36, -29),   S(-21, -32),   S(-27, -40),   S( 18, -36),
            S( 14,  24),   S(-11,  32),   S( 38,  -2),   S(  2,  -2),   S( 13, -37),   S(-39, -24),   S(  3, -41),   S( 50, -30),
            S(-16,  88),   S(-21,  85),   S(-15,  24),   S(-18,   3),   S( -1,  18),   S(-21,   4),   S(-33, -31),   S( 36,  22),
            S( 67,  76),   S( 53, 100),   S(  9,  36),   S( 18,  19),   S( 11, -16),   S(  1, -11),   S(  6,   0),   S(-14,  14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-96,  56),   S(-75,  44),   S( -6,  11),   S( -6,  15),   S(-19,  31),   S(-28,  19),   S(-50,  16),   S(-27,  31),
            S(-50,  17),   S(-58,  20),   S( 35, -17),   S( 22,   2),   S( 20, -10),   S(-13, -18),   S(-29,  -7),   S(-30,  13),
            S(-49,  35),   S(-59,  29),   S( 54, -30),   S( 13, -26),   S( 30, -16),   S(-19, -20),   S(-16,  -8),   S( 11,  -7),
            S(-57,  54),   S(-55,  33),   S(  6,  -2),   S( 26,   4),   S(-15,   4),   S(-51,  -3),   S( -2, -11),   S(  7,  15),
            S( 27,  60),   S( 31,  35),   S( 30,  39),   S( 28,  21),   S(-12,  32),   S( 60,  -8),   S(  7,  11),   S( 44,  30),
            S( 60,  45),   S( 57,  17),   S( 40,  -6),   S( 39,  -3),   S( 45, -14),   S( 20,  -4),   S(  8,   8),   S(  4,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  32),   S(-42,  21),   S(-30,  16),   S(-27,  16),   S( 34, -22),   S(-27,   9),   S(-64,   8),   S(-57,  21),
            S(-42,   4),   S(-15, -17),   S(-12, -32),   S(  2,  -9),   S( 37, -19),   S( 29, -26),   S(-36,  -7),   S(-64,   9),
            S(-25,  -2),   S(-24,  -6),   S(-17, -22),   S(-31,  -5),   S( 18, -13),   S( 67, -43),   S( -7, -17),   S(-16,   6),
            S(-36,  19),   S(-79,  11),   S(  4, -30),   S(-13,  -9),   S( 17,  -3),   S( 38, -18),   S( 18, -10),   S( 38,   3),
            S(  5,  25),   S(-54,  16),   S( 11, -29),   S( -2, -13),   S( 50,  22),   S( 73,  17),   S( 35,   8),   S( 67,  28),
            S( 57,  28),   S( 20,   1),   S(  3, -36),   S(  8, -38),   S( 21,  -1),   S( 24,   2),   S( 40, -10),   S( 40,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -20),   S(-48, -13),   S(-28,  -4),   S(-49,  12),   S(-12, -22),   S( 27, -23),   S( -1, -49),   S(-40, -25),
            S(-38, -40),   S(-37, -41),   S(-44, -36),   S(-19, -44),   S( -5, -37),   S( 54, -59),   S( 55, -62),   S( -4, -36),
            S(-41, -41),   S(-58, -36),   S(-44, -45),   S(-18, -42),   S( -8, -28),   S( 38, -41),   S( 50, -61),   S( 56, -48),
            S(-18, -43),   S(-50, -50),   S(-78, -43),   S(-49, -24),   S( -3, -29),   S( 26, -24),   S( 31, -21),   S( 78, -31),
            S(  7, -33),   S(  6, -59),   S(-22, -51),   S( -1, -64),   S( 22,  -5),   S( 36,  -4),   S( 70,  39),   S(106,  31),
            S(-15,   4),   S(-30, -32),   S(  3, -51),   S( -4, -53),   S( -2, -16),   S( 29, -23),   S( 49,  37),   S( 89,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-60,  73),   S(-42,  61),   S( 14,  25),   S(-10,  -1),   S( 12,   8),   S( -2,   8),   S(-42,   6),   S(-46,  29),
            S(-63,  64),   S(-62,  57),   S(-33,  43),   S(-17,  12),   S(-11,  -8),   S(-39, -12),   S(-53,  -6),   S(  0,  -3),
            S(-64, 100),   S( -9, 101),   S(-10,  63),   S(-28,  35),   S( 10, -11),   S(-100,  -3),  S(-72, -16),   S(-43,  -2),
            S(-31, 140),   S( 11, 149),   S( 16, 106),   S( 12,  50),   S(-32,  15),   S(-31, -20),   S(-28,  -4),   S(-52,  10),
            S(-12, 169),   S( 44, 155),   S( 29, 161),   S( 56, 100),   S( 20,  11),   S(  1,   3),   S(-18, -15),   S( -7,  20),
            S( 53, 192),   S( 70, 210),   S( 86, 200),   S( 49,  74),   S(  7,  34),   S(-11,   5),   S(-10, -24),   S(  2,  14),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-102,  79),  S(-73,  53),   S(  6,  10),   S( 11,  29),   S(  8,   8),   S(-46,  16),   S(-80,  21),   S(-79,  35),
            S(-63,  41),   S(-60,  37),   S(-47,  32),   S(  3,  46),   S(-52,   4),   S(-29,  -9),   S(-78,  -1),   S(-34,  13),
            S(-95,  71),   S(-121, 101),  S(-49,  80),   S(-106,  91),  S(-64,  55),   S(-89,  10),   S(-51, -17),   S(-49,   8),
            S(-75, 109),   S(-39, 118),   S(  7, 121),   S( 46, 125),   S(-26,  58),   S(-42,  14),   S(  6,   3),   S(-50,  24),
            S( 13, 124),   S( 22, 144),   S( 23, 156),   S( 46, 172),   S( 21, 129),   S( -6,  35),   S( -2,   1),   S( -2,   3),
            S( 22,  72),   S( 20, 124),   S( 64, 139),   S( 71, 180),   S( 29, 107),   S( -8,  -6),   S(-14,  -8),   S(-20, -17),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-94,  17),   S(-67,   2),   S(-10,  -1),   S(  2,  20),   S(-12,   1),   S(-63,  29),   S(-110,  30),  S(-62,  36),
            S(-102,  10),  S(-86,   9),   S(-17, -15),   S(-25,  -8),   S(-21,  24),   S(-39,  25),   S(-126,  36),  S(-86,  21),
            S(-30,  -9),   S(-90,  17),   S(-31,   3),   S(-89,  71),   S(-80,  85),   S(-14,  41),   S(-122,  50),  S(-90,  46),
            S(-103,  34),  S(-80,  29),   S(-12,  10),   S(-39,  80),   S( 22,  96),   S(-47,  79),   S(-33,  51),   S(  0,  28),
            S(-28,  47),   S(-35,  21),   S(  7,  50),   S( 27, 124),   S(104, 109),   S( 52,  65),   S( -9,  88),   S( 28,  45),
            S( -2,  17),   S(-21,  -2),   S( 18,  19),   S( 48, 117),   S( 11, 130),   S( 27,  56),   S( -8,  74),   S( 23,  97),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-74,  -2),   S(-78,  20),   S( 38, -16),   S( -2,  18),   S(  0,  36),   S(-87,  56),   S(-54,  38),   S(-68,  46),
            S(-71, -19),   S(-83, -18),   S(-34, -36),   S(-51,  18),   S(-37,  13),   S(-28,  28),   S(-98,  63),   S(-99,  49),
            S(-39, -31),   S(-62, -31),   S(-56,  -3),   S(-34,  11),   S(-53,  37),   S(-14,  58),   S(-79,  85),   S(-46,  69),
            S(-55,   7),   S(-91, -12),   S(-29, -27),   S(-54,  18),   S(  7,  43),   S( -4,  73),   S( 23, 111),   S( 74,  74),
            S(-22,  25),   S(-47,  -6),   S( -7,  -2),   S( -9,  24),   S( 60,  95),   S( -7, 126),   S(102, 119),   S( 90, 103),
            S(-33,  47),   S(-20,   4),   S( 10, -18),   S(  2,   3),   S( 21,  71),   S( 33, 152),   S( 66, 180),   S( 33, 175),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-15,  10),   S(-19,   8),   S(-19,   0),   S(  2,   5),   S( -4, -11),   S(-10,  14),   S(-15, -20),   S(-18,  -3),
            S(-39, -25),   S( -7,  19),   S(  8,  20),   S( -1,   3),   S(  0,  33),   S( -7, -11),   S(-36, -32),   S(-28, -41),
            S(-18,  38),   S(-37,  95),   S( 20,  65),   S( 18,  39),   S(-15,   3),   S(-47, -14),   S(-45, -48),   S(-44, -58),
            S(-41,  90),   S(-46, 122),   S( 39, 114),   S( 23,  96),   S(-19, -29),   S(-41, -35),   S( -9, -14),   S(-60, -50),
            S( 34,  96),   S( 42, 211),   S( 49, 152),   S( 19,  57),   S(  0,  16),   S( -2, -21),   S( -1,   3),   S(-20, -46),
            S( 47, 110),   S( 56, 218),   S(118, 222),   S( 47,  98),   S( -6,   5),   S( -9,  -7),   S(-11, -33),   S(-22, -38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -18),   S(-21,  12),   S( -7,  10),   S( -2,   4),   S( -9, -10),   S(-31,   4),   S(-37, -42),   S(-24,  -5),
            S(-40, -10),   S(-58,  48),   S(-24,  34),   S( 19,  22),   S(-45,  25),   S(-15, -14),   S(-82, -24),   S(-62,  10),
            S(-60,  48),   S(-52,  49),   S(-38,  77),   S(-11,  95),   S(  1,  33),   S(-41, -30),   S(-65, -28),   S(-79, -25),
            S(-77,  93),   S( -8, 120),   S( -4, 139),   S(  7, 123),   S(  0,  65),   S(-44,  25),   S(-18, -12),   S(-39, -40),
            S(  1,  99),   S( 54, 171),   S( 67, 196),   S( 49, 248),   S( 23, 150),   S(-11,  15),   S( -4, -63),   S(-25, -37),
            S( 40,  72),   S( 74, 173),   S( 84, 193),   S( 84, 252),   S( 39, 110),   S(  3,  10),   S(  0,   3),   S( -5,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -55),   S(-38, -24),   S( -9, -28),   S( -3,  -1),   S( -5,  -2),   S(-32,   8),   S(-36,  -2),   S( -5,  47),
            S(-53,  13),   S(-56,   9),   S(-55, -29),   S(  0,  11),   S(-39,  64),   S(-18,  17),   S(-42,  19),   S(-55,  13),
            S(-62, -23),   S(-61,   8),   S(-36, -19),   S(-22,  41),   S(-20,  71),   S(-53,  36),   S(-36,   5),   S(-65,  44),
            S(-51,  14),   S(-24,  54),   S(-26,  30),   S(  9,  98),   S( -4, 133),   S(-27,  84),   S(-37,  41),   S(-36,  60),
            S(-21, -21),   S( 10,  18),   S( 14,  79),   S( 36, 135),   S( 47, 215),   S( 44, 170),   S( 10,  86),   S( 27,  41),
            S( -2,  23),   S( 19,  37),   S( 30, 115),   S( 36, 138),   S( 65, 213),   S( 58, 115),   S( 29,  97),   S( 20,  33),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -32),   S(-32, -21),   S(-11, -32),   S(  0,  -3),   S( 17,  22),   S(  0,  47),   S(-11, -24),   S(  9,  22),
            S(-42, -31),   S(-32, -13),   S(-14, -41),   S( 24,  -6),   S(-14,   1),   S(  7,  46),   S(  4,  29),   S(  0,  -2),
            S(-17, -73),   S(-33, -59),   S(-20, -51),   S(  2,  -8),   S( 12,  33),   S(-15,  58),   S( -3,  70),   S(-23,  64),
            S(-26, -23),   S(-43, -30),   S(-31,   2),   S( 12,  20),   S(-10,  52),   S(  6,  92),   S(-26, 139),   S( -5,  55),
            S(-27, -42),   S(-31, -31),   S(-13,  16),   S(  1,   0),   S( 37, 117),   S( 65, 165),   S( 58, 222),   S( 78,  69),
            S( -8,   7),   S( -3,  10),   S(  2,   9),   S(  7,  25),   S( 27,  82),   S( 84, 193),   S( 32, 182),   S( 43,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-35,   4),   S( -3,  12),   S(-47,  14),   S(-22,  -7),   S(-32,  -4),   S(  1, -30),   S(-44, -47),   S(-34, -17),
            S(-37,  55),   S( 18, -39),   S(-42,  15),   S(  8, -20),   S(-12, -18),   S(-23, -16),   S(-31, -24),   S(-73, -22),
            S(  6,  64),   S( -3,  -9),   S(  6,  -8),   S(-23,  34),   S( 10,  10),   S(-34,   2),   S(-11, -29),   S(-41, -49),
            S( 19, -15),   S( 44,  11),   S( 13,  28),   S( 27,  30),   S(  6,   7),   S( -2,   6),   S( -8, -13),   S( -2,  -4),
            S( 18, -28),   S( 38,  14),   S( 16,  11),   S( 66,  -4),   S( 44,  -5),   S( 30,  23),   S( 24, -12),   S(-58, -12),
            S( 20, -14),   S( 12,  10),   S( 33,  14),   S( 56, -13),   S( 37, -43),   S( 20,  15),   S( 11, -22),   S( -3, -12),
            S( 13, -30),   S( 15, -39),   S( 19, -25),   S( 36, -31),   S( 23, -18),   S( -6, -27),   S(-11, -43),   S(-22, -33),
            S(-70, -56),   S( -6,   1),   S( -5, -18),   S(  2, -45),   S(-19, -23),   S( 21,   9),   S( -7,   2),   S( 15,  -3),

            /* knights: bucket 1 */
            S(-44,  23),   S(-54,  87),   S( 18,  41),   S(-28,  67),   S(-20,  50),   S(-23,  28),   S(-34,  52),   S(-19, -14),
            S( 34,  21),   S( -7,  36),   S( -5,  28),   S( -7,  45),   S(-10,  29),   S(-16,  16),   S( 14, -15),   S(-30,  11),
            S(-31,  26),   S( 13,  11),   S( -1,  14),   S( 16,  30),   S(  6,  32),   S(-29,  30),   S(-18,   5),   S(-31,  20),
            S(  1,  39),   S( 56,  26),   S( 19,  45),   S( 25,  30),   S(  9,  33),   S( -2,  32),   S( 16,  13),   S( 14,  15),
            S(  2,  48),   S( 20,  27),   S( 30,  29),   S( 39,  29),   S( 38,  30),   S( 32,  24),   S( 28,  16),   S( 17,  17),
            S(  9,  15),   S( 20,  11),   S( 20,  33),   S( 46,  16),   S( 14,  22),   S( 37,  34),   S( 27,   7),   S( 17, -11),
            S( 34,   2),   S( 26,  15),   S(-16, -19),   S( 15,  31),   S( 30,  -6),   S( 29,  -6),   S(-32,   9),   S(-10, -23),
            S(-98, -67),   S(-23, -14),   S( -5,  14),   S(  3,  26),   S(-13,   1),   S(-24, -21),   S( -4,  -6),   S(-39, -38),

            /* knights: bucket 2 */
            S(-60,   4),   S( -2,  20),   S(-35,  54),   S(-26,  57),   S(-38,  62),   S(-38,  71),   S(-18,  29),   S(-25,  11),
            S(-17, -20),   S(-22,  11),   S(-14,  18),   S(-14,  36),   S( -6,  25),   S(-15,  53),   S(-38,  56),   S(-41,  64),
            S(-19,  22),   S( -6,   9),   S(-12,  31),   S( 15,  23),   S(  3,  35),   S(  4,  14),   S( -7,  41),   S(-25,  26),
            S( -9,  39),   S(-22,  39),   S(  5,  41),   S(  4,  52),   S( -1,  48),   S( -4,  40),   S(  5,  41),   S( -3,  43),
            S( 18,  22),   S(-15,  35),   S( -4,  47),   S(-17,  56),   S(  2,  51),   S( -6,  45),   S(  5,  35),   S( -2,  22),
            S(-24,  32),   S(  2,  32),   S(-26,  54),   S(-15,  48),   S(-25,  46),   S(  6,  27),   S(-28,  11),   S( 16,   1),
            S(-21,  23),   S(-32,  15),   S(-33,  18),   S(-36,  34),   S(-11,  14),   S(  3,  21),   S(-50,  35),   S(-35,  10),
            S(-143,  17),  S( -5,  -2),   S(-80,  31),   S(-29,  10),   S( -2,   9),   S(-58,   0),   S( -2,  -1),   S(-178, -54),

            /* knights: bucket 3 */
            S(-49, -12),   S( 13, -30),   S(-23,  -7),   S(  7,  -4),   S( 10, -11),   S(-14,   9),   S( 22, -19),   S( -9, -30),
            S(-14,  -3),   S(-25, -10),   S(-17,  -9),   S(  8,   8),   S( 22,  -5),   S(  0, -10),   S( -2, -12),   S(-18,  55),
            S(  0, -33),   S(  4,  -4),   S(  4,  -1),   S( 18,   7),   S( 22,  20),   S( 24,   4),   S( 17,   1),   S( 12,  29),
            S(  1,   2),   S( 13,  11),   S( 19,  34),   S( 24,  33),   S( 33,  33),   S( 28,  35),   S( 32,  23),   S( 27,  19),
            S( 29,   3),   S(  9,  19),   S( 38,  11),   S( 32,  41),   S( 29,  40),   S( 37,  46),   S( 44,  40),   S( 21,  12),
            S(  5,   7),   S( 33, -12),   S( 50,   0),   S( 62,   4),   S( 73, -15),   S( 78,  -9),   S( 16,   8),   S( 13,  39),
            S( 27,  -7),   S( 16,   5),   S( 46, -22),   S( 52,  -9),   S( 68, -33),   S( 65, -38),   S( 64, -67),   S( 49, -23),
            S(-106,   9),  S(-25,   6),   S(-27,   1),   S(  7,  14),   S( 37, -12),   S( -4, -15),   S( -9, -24),   S(-68, -48),

            /* knights: bucket 4 */
            S( 13,  17),   S(-52,   3),   S( 13,  25),   S( -4,  -6),   S(-22, -13),   S(-33, -25),   S(-10, -53),   S(-30, -45),
            S( 32,  21),   S(-24,  35),   S( 14, -24),   S(  9,  -7),   S( 15, -17),   S( -7, -43),   S( 10,  -3),   S(  0, -48),
            S(-10,  27),   S(  9,  37),   S(  9,  10),   S( 13,  15),   S( -4,   1),   S(-46,  17),   S(-48, -31),   S(-33, -57),
            S( -1,  65),   S( 35, -20),   S( 48,  25),   S( 27,  24),   S( 19,  16),   S( 97, -13),   S( 27, -28),   S(  0, -19),
            S( 62,  30),   S(-11,  47),   S( 51,  48),   S( 48,  23),   S( 42,  39),   S(-11,  30),   S( -2, -24),   S(-12,  -9),
            S(  9,  18),   S(-25,   2),   S( 85,  19),   S( 10,  11),   S( 11,  21),   S( 23,  22),   S( 11,  28),   S(-12, -22),
            S( -7,   6),   S(-16,   7),   S( 12,   0),   S(  4,  36),   S(  8,   8),   S(  6, -16),   S(  4,  -9),   S(-16,  -4),
            S(-12,  -8),   S( -1,  -6),   S(  9,   9),   S(  0,   2),   S( -7, -10),   S(  9,  19),   S( -2,   4),   S( -3, -20),

            /* knights: bucket 5 */
            S(  9,  -3),   S(-43,  47),   S( 28,  36),   S( 17,  49),   S( 32,  26),   S(  9,   0),   S( -1,  17),   S(-22, -22),
            S( 11,   0),   S( 29,  47),   S( 13,  26),   S(-13,  44),   S( 28,  39),   S( -2,  37),   S( 19,  27),   S(-16, -28),
            S(  0,  26),   S(-13,  40),   S( 57,  24),   S( 42,  44),   S(-18,  53),   S( -5,  30),   S(-20,  18),   S(  4,  -5),
            S( 32,  47),   S( 11,  49),   S( 38,  45),   S( 10,  60),   S( 22,  50),   S( 13,  47),   S( 26,  46),   S( 12,  37),
            S( 21,  53),   S( 35,  37),   S( 53,  54),   S( 69,  46),   S( 84,  49),   S( 31,  47),   S( 43,  39),   S( 37,  32),
            S(  5,  31),   S(  2,  52),   S( 26,  33),   S( 19,  59),   S( 41,  46),   S( 15,  57),   S( 21,  17),   S( -7,  31),
            S( 18,  54),   S( -7,  63),   S( 29,  46),   S( 16,  63),   S(  6,  52),   S(  7,  45),   S( 22,  66),   S(  2,  -1),
            S( -5,   5),   S( -1,  12),   S(  7,  39),   S( -4,   4),   S(  9,  40),   S(  1,  31),   S(  8,  38),   S(-18, -19),

            /* knights: bucket 6 */
            S(  0, -43),   S(-21,  -3),   S( 26,  29),   S(-29,  42),   S(-34,  51),   S(  8,  42),   S(-14,  34),   S(-15,  25),
            S( -7, -31),   S( 49,   0),   S( 11,  13),   S(-38,  43),   S(-62,  71),   S( 25,  50),   S( 12,  50),   S( -5,   8),
            S(-31, -18),   S( -3,   3),   S(-11,  28),   S( 19,  35),   S(-21,  63),   S(-40,  60),   S(  8,  47),   S( -3,  43),
            S( 33,   8),   S( 36,  14),   S( 48,  34),   S( 79,  29),   S( 30,  50),   S( 16,  57),   S( 15,  61),   S(-24,  72),
            S(  2,  37),   S( 68,  -5),   S( 57,  39),   S( 77,  34),   S( 91,  39),   S( 89,  38),   S( 20,  61),   S( 20,  55),
            S( 22,  26),   S( 12,  16),   S( 69,  23),   S( 54,  45),   S( 61,  51),   S( 35,  37),   S( 24,  42),   S( 39,  40),
            S(-24,  21),   S( -3,  34),   S(-30,  36),   S( 29,  31),   S( -1,  57),   S( 21,  41),   S( 17,  69),   S( -9,  28),
            S(-43,  -2),   S( 14,  39),   S( 27,  36),   S(  9,  36),   S( 21,  32),   S(  9,  57),   S( 19,  57),   S( 10,  22),

            /* knights: bucket 7 */
            S(-35, -57),   S(-193, -46),  S(-72, -45),   S(-61, -15),   S(-42,  -9),   S(-37, -16),   S(-15,   1),   S(-17,   5),
            S(-53, -78),   S(-40, -48),   S(-40, -31),   S(-53,   6),   S(-48,  11),   S(  0, -11),   S(-19,  48),   S(  0,  28),
            S(-85, -67),   S(-61, -34),   S(-55,   3),   S( 20, -17),   S(-20,   9),   S(  5,  10),   S(-13,  57),   S( 43,  53),
            S(-63, -22),   S( 15, -23),   S( -8,  15),   S( 33,   3),   S( 46,   4),   S( 15,  18),   S( 19,  15),   S(-22,  34),
            S(-60, -22),   S(-21, -25),   S( 47, -17),   S( 81, -12),   S(108,  -1),   S( 71,  24),   S( 97,   3),   S( 79,  20),
            S( -9, -40),   S( 15, -38),   S(-20,   1),   S( 34,   2),   S( 69,  12),   S( 79,  10),   S( 59, -15),   S( -2,  10),
            S(-35, -33),   S(-67, -21),   S(  3, -15),   S( 31,  16),   S( 36,  20),   S( 41,   0),   S(-20,  21),   S(  1,   3),
            S(-39, -31),   S(-10, -10),   S(-28, -15),   S(  7,  11),   S( 10,   3),   S( 21,  16),   S( -5, -11),   S( -4,  -8),

            /* knights: bucket 8 */
            S( -1,  -8),   S( -9, -10),   S( -3,  -4),   S( -9, -31),   S(-10, -41),   S(-10, -52),   S( -2,  -1),   S( -6, -24),
            S(  2,   0),   S( -6, -12),   S( -7, -30),   S(-19, -45),   S(-30, -28),   S(-17, -70),   S(-13, -59),   S(-17, -37),
            S(  4,  16),   S(-23, -21),   S( 21,   6),   S(  5,  -1),   S(  2, -31),   S(-16, -13),   S(-13, -38),   S( -8, -42),
            S(-18,  -3),   S(  1,  -4),   S(  0,  14),   S(  5,  33),   S(  8,  -1),   S(  7,  10),   S(-13, -50),   S( -3, -18),
            S( 26,  53),   S( 11,   9),   S( 17,  37),   S( 36,  19),   S( 12,  33),   S( -4,   0),   S(  5, -19),   S( -7,  -8),
            S( 13,  36),   S( 11,   7),   S( 30,  24),   S( 33,  17),   S(  4,   1),   S( -1,  -7),   S( -7, -28),   S( -6, -10),
            S(  2,  12),   S(  0,   2),   S(  6,   9),   S( 10,  10),   S(  6,   7),   S(  5,  20),   S(  2,  11),   S( -1,   1),
            S(  1,   1),   S( 11,  32),   S(  5,  15),   S( -2,  -1),   S(  3,  10),   S( -5, -20),   S(  3,   4),   S( -4,  -5),

            /* knights: bucket 9 */
            S(-10, -30),   S(-20, -37),   S(-19, -48),   S( -4, -15),   S(-23, -55),   S(-15, -40),   S( -3, -15),   S( -4, -28),
            S(-12, -39),   S(-12,  -1),   S(-11, -51),   S(-14,  -7),   S( -5, -14),   S( -7, -34),   S( -6,  -4),   S(-15, -42),
            S(  5,   5),   S(-11, -14),   S(  2, -15),   S(  2,   4),   S(  3,  18),   S(-33,   0),   S(-13, -12),   S( -8, -18),
            S(-15,  -3),   S( -6,  -8),   S(  5,  30),   S( 19,  32),   S( 29,  26),   S(  9,  25),   S(-12, -35),   S( -2,   0),
            S(  0,  22),   S( 19,   7),   S( 20,  41),   S(  4,  43),   S( 12,  19),   S( 13,   0),   S(  3, -27),   S(  4,   9),
            S(  1,   1),   S(  7,  32),   S( 17,  34),   S( -5,  21),   S( 36,  37),   S( 17,  14),   S(  7,  15),   S( -7, -24),
            S(  1,   0),   S( -2,  20),   S( 18,  36),   S( 12,   3),   S( 13,  40),   S( -2, -18),   S(  3,  16),   S( -3,  -1),
            S(  1,  -1),   S(  3,   7),   S( 11,  26),   S( 14,  29),   S(  8,   8),   S(  0,   4),   S(  2,   3),   S( -1,  -5),

            /* knights: bucket 10 */
            S(-18, -50),   S(-17, -55),   S(-13, -28),   S(-18, -21),   S(-13, -12),   S(-15, -44),   S( -3,  16),   S(  4,  20),
            S( -7, -26),   S( -7, -14),   S( -1, -17),   S(-20, -34),   S(-24, -35),   S( -8, -41),   S( -9,  -8),   S( -5, -13),
            S(-17, -52),   S(-18, -62),   S(-10, -11),   S(-16, -14),   S( 13,   2),   S(-14,  -2),   S( -6,   3),   S( -7,   5),
            S( -8, -18),   S( -6, -44),   S(  3, -32),   S( 19,  14),   S( 12,  40),   S( 18,  25),   S(  6,  16),   S( 10,  42),
            S( -8, -46),   S(-13, -28),   S( 16,  14),   S( 24,  32),   S( 23,  53),   S(  3,  29),   S( 21,  14),   S( 22,  51),
            S(-11, -41),   S( -5, -21),   S( -3,  -7),   S( 15,  42),   S( 38,  63),   S( 33,  43),   S( 28,  58),   S( 17,  53),
            S( -1,  -3),   S(-10, -33),   S(  1,  -8),   S( 27,  26),   S( 17,  28),   S( 10,  32),   S(  0,  -3),   S(  8,  23),
            S( -3, -17),   S(  3,   9),   S( -8, -19),   S(  3,  -3),   S( 11,  36),   S(  5,  25),   S(  2,  12),   S( -1,  -3),

            /* knights: bucket 11 */
            S( -1,  -1),   S(-20, -29),   S( -8, -45),   S(-10, -27),   S(-21, -50),   S(-13, -19),   S( -6,  -5),   S( -4,  -6),
            S( -8, -11),   S(-13, -21),   S(-16, -78),   S(-29, -25),   S( -9,  -3),   S(-29, -38),   S(-17, -30),   S( -8, -10),
            S(-15, -54),   S(-24, -61),   S(-27, -33),   S(  0,   3),   S(-14,   6),   S(-18,  18),   S(  9,  -5),   S( -1,  14),
            S(-13, -30),   S( -7, -28),   S(-25,  -2),   S( 26,  33),   S( 16,  21),   S( 18,  11),   S( 13,  25),   S( 14,  27),
            S( -3, -24),   S(-18, -56),   S(  7, -16),   S(  2,   9),   S( 16,  24),   S( 36,  57),   S(  7,  -1),   S( 25,  63),
            S( -8, -11),   S( -6, -27),   S(  1,  -3),   S( 40,  37),   S( 18,  25),   S( 50,  48),   S( 22,  24),   S( 14,  27),
            S(  8,  26),   S( -2,  -6),   S(  7, -12),   S( 12, -17),   S( 20,  30),   S( -1,   5),   S( 15,  37),   S( 19,  52),
            S( -4,   0),   S( -2, -18),   S(  8,  11),   S(  1,   5),   S(  1,  11),   S(  2,   3),   S(  3,   4),   S(  1,  11),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   2),   S( -2, -20),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   4),   S( -2, -14),
            S(  0,   0),   S(  1,   2),   S(  2,   6),   S( -3, -12),   S( -2,   6),   S( -4, -21),   S( -2, -12),   S(  1,   9),
            S( -5, -14),   S(  4,   4),   S( -5, -11),   S( -6, -22),   S(  0,   3),   S( -5, -17),   S(  1,  -5),   S( -7, -31),
            S( -7, -13),   S( -1,   1),   S( -8, -22),   S(  5,  15),   S( -6,  -5),   S(  0,   6),   S( -1,  -6),   S( -1,  -8),
            S(  9,  16),   S(  4,   2),   S( -6, -12),   S(  0,   4),   S( -5, -25),   S(  0,   4),   S( -1, -13),   S( -1,   1),
            S(  1,  -9),   S( -3, -22),   S(  2,   1),   S( -1,  -6),   S(  5,  11),   S( -5, -17),   S( -1,  -7),   S(  0,   3),
            S(  2,   7),   S( -9, -12),   S(  0,   9),   S(  2,  -9),   S( -5,  -8),   S( -5, -21),   S( -2,  -2),   S(  0,  -2),
            S(  2,   3),   S(  1,  13),   S( -2,  -4),   S(  2,  -2),   S( -2,  -4),   S( -2, -10),   S( -3,  -9),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -2,  -5),   S( -2,  -2),   S( -8, -13),   S( -1,   0),   S( -3, -12),   S(  1,  -2),
            S( -2,  -7),   S(  1,   5),   S( -3, -24),   S(-11, -22),   S( -6, -31),   S( -4, -25),   S(  0,   1),   S(  0,  -1),
            S( -4, -10),   S( -9, -29),   S(  7,  17),   S(  0,   0),   S(-12, -38),   S(-10, -24),   S( -2, -11),   S( -6, -28),
            S( -9, -15),   S(  5,  13),   S(  1,   2),   S(-10, -26),   S( -2,  -7),   S(  6,  12),   S(  0, -14),   S( -5, -10),
            S(  3,  10),   S( -1,  -2),   S(  3,  -7),   S( 11,  22),   S(  5, -10),   S( -3,  -7),   S(  2, -13),   S(  1,   0),
            S( -3,  -9),   S( 14,  14),   S(  7,  24),   S(-12,  13),   S(  6,   5),   S( -9, -33),   S(  4,   6),   S( -4,   2),
            S(  1,   6),   S(  2,   4),   S( 10,  11),   S(  8,  10),   S( 14,  22),   S( -5, -21),   S( -3,  -2),   S( -5,  -3),
            S( -1,   1),   S( -1,  -6),   S( -1,   1),   S(  1,  -9),   S( -1,  -1),   S(  3,  -1),   S(  0,  -2),   S( -2,   0),

            /* knights: bucket 14 */
            S( -3, -23),   S( -5, -25),   S( -2,  -3),   S( -3,   3),   S( -8, -24),   S( -2, -15),   S( -1,  -5),   S(  0,   2),
            S(  0,  -2),   S( -3,  -7),   S(-15, -60),   S( -8, -36),   S( -2, -10),   S(  1,   6),   S(  1,  -3),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-11, -53),   S(  1,   2),   S( -4, -19),   S( -4,  -9),   S(  0,  -1),   S(  2,   9),
            S(  0,   5),   S( -6, -33),   S(-15, -40),   S(-11, -36),   S( -2, -20),   S(  3,   2),   S( -3, -16),   S( -7, -11),
            S( -2,  -4),   S( -2, -15),   S(  1,  23),   S( -7, -31),   S( -9,  -7),   S(  3,  23),   S(  3,   5),   S( -4,  -6),
            S( -4,  -8),   S(  3,  -2),   S( -9, -30),   S(  5,   2),   S( 16,  26),   S(  5,   9),   S( -3,   1),   S(  0,  -3),
            S(  0,  -3),   S( -2, -11),   S(  7,  -4),   S(  0,  -8),   S( -6, -10),   S( -3,  -9),   S( -6,  -4),   S(  1,   7),
            S( -1,  -2),   S(  2,   4),   S( -2, -11),   S(  7,  -1),   S(  5,  17),   S(  1,   2),   S( -2,  -8),   S( -1,  -5),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -2, -13),   S( -1, -13),   S( -7, -14),   S( -2,  -1),   S( -1,  -5),   S(  1,   0),   S(  0,  14),
            S( -2,  -6),   S(  0,  -2),   S( -4, -18),   S( -6, -26),   S( -2,  -5),   S( -1,  -8),   S(  0,   0),   S( -1,  -3),
            S( -6, -16),   S( -7, -16),   S( -3, -12),   S(-14, -39),   S( -5, -24),   S( -1,  -3),   S( -1,  -1),   S( -2,   1),
            S( -6, -17),   S( -6, -32),   S( -6, -19),   S(  0,  -8),   S(  1, -16),   S(  7,  24),   S(  5,  10),   S( -4,  -1),
            S(  0,  -2),   S( -2,  -5),   S( -1, -15),   S( -7, -10),   S(  4,  19),   S(  4,  11),   S( -6,  -8),   S( -2,   2),
            S( -3,  -4),   S( -2,  -4),   S( -2, -20),   S( -3,   8),   S( -5, -14),   S( -6,  11),   S( -3,   4),   S(  2,   7),
            S( -3, -12),   S( -2,  -6),   S( -1,  -9),   S( -4,  -8),   S(-10, -13),   S( -4,  15),   S( -2,  -8),   S(  3,  12),
            S(  0,  -3),   S(  0,  -1),   S( -4,  -9),   S( -2,  -9),   S( -2,  -5),   S(-10,  -5),   S(  6,  17),   S( -2,   1),

            /* bishops: bucket 0 */
            S( 16,  16),   S( 19, -13),   S( 39,  17),   S(  7,  22),   S( -2,  -5),   S( 17,  -6),   S( 25, -41),   S(  0, -36),
            S( 45, -43),   S( 79,   3),   S( 37,   8),   S( 17,   3),   S(-14,  35),   S(  0, -21),   S(-38,  -3),   S(  8, -50),
            S( 24,  42),   S( 51,   8),   S( 30,   2),   S( 12,  54),   S( 20,  12),   S(-30,  24),   S( 10, -23),   S( 10, -41),
            S( 19,  11),   S( 71,  -8),   S( 39,  15),   S( 39,  35),   S(  7,  34),   S( 29,   3),   S( -3,  -9),   S(  0,   0),
            S( 17,   2),   S( 31,  28),   S(  8,  43),   S( 60,  17),   S( 67,   1),   S( 19,  -1),   S( 24, -15),   S(-34,  -2),
            S(-40,  62),   S( -1,  22),   S( 65, -20),   S( 89, -20),   S( 43,  33),   S( -2,   2),   S(  3,  14),   S(  1,  12),
            S(-12,  11),   S(  9,  -1),   S( 44,  -7),   S(  1,  35),   S(-30,  -3),   S( 27,  27),   S(  4, -12),   S(-14,  -9),
            S(-34, -44),   S( 11,   2),   S(  3,   7),   S(  6, -13),   S( 20,  25),   S( 34,  10),   S( -1,  44),   S(-22,   1),

            /* bishops: bucket 1 */
            S( 35,   8),   S( -6,  29),   S(  7,  37),   S( 13,  29),   S( -5,  28),   S(  0,  33),   S( -7,   2),   S(-47,  -7),
            S( 10, -16),   S( 34, -13),   S( 52,   9),   S( 27,  31),   S(-10,  18),   S(  9,  -1),   S(-33,  -5),   S( 13, -19),
            S( 37,  -4),   S( 20,   5),   S( 38,  -8),   S( 20,  25),   S( 21,  26),   S(-20,   4),   S( 30,  -5),   S(  3, -31),
            S( 39,   4),   S( 22,  18),   S( 12,  17),   S( 38,  26),   S(  8,  26),   S( 23,   7),   S( -4,   8),   S( 15, -11),
            S( 38,  26),   S(  9,  25),   S( 21,  27),   S(  2,  35),   S( 31,  13),   S(  3,  18),   S( 29, -15),   S( -9,   9),
            S( -3,  21),   S( 33,  36),   S( 35,   3),   S( 57,  -4),   S( 19,  19),   S( 38, -15),   S(  4,  30),   S( 49, -18),
            S(-10,  44),   S(-27,  19),   S( 19,  28),   S( 35,  24),   S( 41,  26),   S(-17,  22),   S( 38, -20),   S(-18,  36),
            S( 13,   1),   S(  9,   4),   S(  2,  11),   S(-23,  20),   S( 20,  15),   S( -8,   4),   S( 12,   4),   S( -5,  10),

            /* bishops: bucket 2 */
            S( 17, -19),   S(  7,  16),   S( -3,  20),   S(-28,  51),   S(-12,  37),   S(-28,  29),   S(-17,  -8),   S(-47,  16),
            S(-23,  21),   S(  4, -17),   S( 21,  10),   S( -4,  28),   S( -2,  36),   S( 17,   9),   S(-10, -20),   S(  3, -35),
            S( -7,   2),   S( -2,  12),   S(  4,   8),   S( -3,  44),   S(  8,  34),   S(  1,  16),   S( 18,  13),   S(-16,  -5),
            S(  0,  11),   S( -9,  15),   S(-11,  37),   S(  9,  38),   S( -1,  44),   S(  9,  25),   S(  9,  17),   S(  8,   0),
            S(  8,   4),   S(-17,  36),   S( -5,  27),   S(-28,  49),   S( -8,  40),   S( -6,  48),   S(  3,  23),   S(-28,  30),
            S(  6,  25),   S( -2,  15),   S(-23,  29),   S(-14,  26),   S( 13,  14),   S( -4,   9),   S(  0,  55),   S( -1,  23),
            S( -2,  19),   S(-21,  10),   S(-26,  53),   S( 19,   2),   S( -2,   2),   S(-17,   9),   S(-69,  12),   S(-35,  33),
            S(-54,  29),   S(-37,  43),   S(-27,  27),   S(-43,  25),   S(-48,  35),   S(-34,  16),   S(  7,  12),   S(-70,   5),

            /* bishops: bucket 3 */
            S( -2,   1),   S( 35,  -9),   S( 29,  19),   S( 17,  18),   S( 21,  10),   S( 41,  -6),   S( 41, -24),   S( 42, -68),
            S(  9,   5),   S(  5,  -2),   S( 30,  -5),   S(  9,  32),   S( 21,   8),   S( 21,  24),   S( 44,  -5),   S( 37,  -6),
            S( 20,   7),   S( 13,  18),   S( 11,  16),   S( 25,  21),   S( 24,  51),   S( 25,   7),   S( 42,  23),   S( 46, -12),
            S( 29,  -5),   S( 27,   9),   S( 18,  34),   S( 26,  47),   S( 32,  38),   S( 33,  33),   S( 29,  24),   S( 26,  -3),
            S( 18,   2),   S( 25,  17),   S( 46,  15),   S( 32,  46),   S( 26,  49),   S( 37,  30),   S( 18,  35),   S( 24,  32),
            S( 29,   3),   S( 37,  22),   S( 28,  12),   S( 44,  15),   S( 27,  22),   S( 56,   3),   S( 50,  15),   S(  5,  67),
            S( 17,   8),   S( -5,  14),   S( 42,  23),   S( 24,  16),   S( 15,  15),   S( 21,   2),   S(  0,  26),   S( 17,  34),
            S(-35,  56),   S( -2,  32),   S( 56,   9),   S( 26,  14),   S(-11,  31),   S(  2,  32),   S( 32,  -2),   S( 65, -31),

            /* bishops: bucket 4 */
            S(-24, -27),   S(-24,   7),   S(-36,  -3),   S(-26,  19),   S(-22,  27),   S(-47,  27),   S( -1, -11),   S(-14, -12),
            S(-10,   7),   S(  7,   5),   S(-12,  34),   S(-30,  18),   S(-20,  -5),   S( 37,  -4),   S(-29,  -9),   S( 11,  -4),
            S(-13,  -1),   S(-34,  36),   S( 14, -15),   S(-26,  17),   S(  1,  29),   S( 24, -21),   S(-26,  -7),   S(-55,  -3),
            S(-34,  25),   S(  0,  35),   S( 49,  29),   S( 32,  38),   S( 14,  23),   S( 54,  -7),   S( 49,  -6),   S(-12, -36),
            S(  7,  17),   S(  8,  47),   S(-16,  55),   S( 23,  43),   S( 37,  12),   S( 34, -16),   S( -9, -18),   S( 12, -11),
            S( -6,  35),   S( 25,  18),   S(-10,  30),   S( 21,  15),   S( 42,  10),   S(  9, -10),   S( 19, -34),   S(  2,  -8),
            S(-17,   7),   S( 29,  15),   S( 15,  20),   S( 25,  17),   S( 10,  -4),   S(  1,  17),   S(  0,   5),   S(  6, -26),
            S( 11, -18),   S(-11, -38),   S(  1,  -6),   S( -5,  -2),   S(  6, -13),   S(  0,   8),   S( -1,  -9),   S( -5,   1),

            /* bishops: bucket 5 */
            S(-19, -16),   S(-17,  36),   S(-41,  32),   S(-24,  33),   S(-42,  35),   S( -5,  16),   S( -6,  17),   S(-26,  12),
            S(-28,  35),   S(-17,   6),   S(-35,  58),   S( -1,  29),   S(-33,  37),   S(-31,  28),   S(-37, -13),   S(-12,  -3),
            S(  1,  14),   S(  2,  38),   S( 20,  15),   S(-23,  53),   S( -1,  38),   S(-34,   1),   S(-27,  35),   S(-23,   5),
            S( 31,  13),   S( 26,  30),   S(-13,  60),   S( 33,  32),   S( 29,  39),   S( 18,  33),   S( 19,  -4),   S( 10,  25),
            S( 26,  45),   S( 36,  15),   S( 56,  32),   S( 84,  35),   S( 49,  23),   S( 43,  21),   S( 38,  15),   S( -9,   4),
            S( 19,  40),   S( 31,  46),   S( 36,  24),   S( 33,  36),   S( -1,  38),   S( 19, -16),   S(-23,  47),   S( -3,  32),
            S(  0,  38),   S(-31,  14),   S( 12,  41),   S(  5,  52),   S( 29,  29),   S( 32,  38),   S( -3,  18),   S( -3,  29),
            S( -3, -11),   S( 14,  33),   S( 14,  12),   S(  6,  35),   S(  2,  55),   S( 13,  23),   S( 28,  53),   S( -9,  -3),

            /* bishops: bucket 6 */
            S(-15,  13),   S( -2,  28),   S(-30,  32),   S(-40,  36),   S(-37,  25),   S(-41,  33),   S(-23,  52),   S(-21,   6),
            S( 19,   7),   S(  0, -12),   S(-23,  32),   S( -7,  31),   S(-33,  45),   S(-20,  27),   S(-105,  31),  S( 13,  24),
            S( 17,  -1),   S(  8,  10),   S( 27,   0),   S( 19,  31),   S( 37,  26),   S( 11,  12),   S(  6,  33),   S(-42,  20),
            S(-12,  41),   S( 21,  15),   S( 36,  23),   S( 30,  37),   S( 40,  36),   S( 36,  30),   S( 31,  33),   S(-14,   0),
            S(-11,  21),   S( 56,   7),   S( 26,  28),   S( 51,  26),   S(100,  29),   S( 62,  28),   S( 36,  32),   S(-31,  47),
            S(  6,  11),   S(-40,  48),   S( 10,  20),   S( 18,  41),   S( 41,  30),   S( 30,  29),   S(  5,  49),   S(-14,  46),
            S(-26,  31),   S(-31,  25),   S(  2,  40),   S(-10,  33),   S( 45,  21),   S( 23,  30),   S(-10,  33),   S( -5,  33),
            S(  3,  44),   S( 12,  33),   S(  8,  39),   S(  1,  45),   S(-17,  37),   S( 31,  16),   S( 11,  21),   S( 12,   6),

            /* bishops: bucket 7 */
            S(-17, -40),   S( -6,   3),   S(-33, -28),   S(-52,  11),   S(-33, -11),   S(-77,  19),   S(-74, -31),   S(-66,   5),
            S(-33, -29),   S(-58, -41),   S(-21,  -4),   S( -1, -14),   S(-31,   2),   S(-47,  15),   S(-51, -11),   S(-32,   5),
            S(-37, -21),   S(  3, -15),   S( 21, -37),   S( 19,   0),   S(-33,  21),   S(-21, -13),   S(-36,  47),   S(-28,  25),
            S(-40,  16),   S( 57, -35),   S( 77, -21),   S( 59,   5),   S( 84,   3),   S(  3,  24),   S( 23,  32),   S(-14,  28),
            S( 22, -49),   S( -6, -20),   S( 66, -33),   S(106, -26),   S( 71,  26),   S( 73,  16),   S(  0,  42),   S( 25,   8),
            S(-28, -13),   S(-24,   4),   S( 34, -44),   S( 25,  -3),   S( 53, -10),   S( 55,   5),   S( 58,  17),   S( 24,   1),
            S( -3, -16),   S(-40,  -7),   S(  8,  -3),   S( 13,  -7),   S( 15, -21),   S( 37,  -7),   S( 11,  -1),   S( 13,  12),
            S(-14,  -5),   S(-11,  13),   S(-32,   9),   S(  5,  -6),   S( 11,  -4),   S( 19,  -5),   S( 25,   8),   S(  6,   7),

            /* bishops: bucket 8 */
            S(-10,  -9),   S(-12, -33),   S(-44,  -6),   S( -4, -26),   S( -6,  20),   S(-25,  -4),   S(  5,  21),   S( -5,  -9),
            S( -7,  -3),   S(-34, -49),   S(-13, -23),   S(-16,  -4),   S(  9,  -9),   S(-18, -27),   S(-19, -54),   S( -5,  -8),
            S(  2,   2),   S(-10,  11),   S(-24,   9),   S( -9,  19),   S( -7,  13),   S( -8, -38),   S(  6, -42),   S(-31, -37),
            S(  6,  32),   S( -4,  46),   S(  8,  41),   S( -2,  18),   S( 18,  23),   S( -1,  10),   S(  5, -15),   S( -5, -17),
            S( 15,  37),   S( 14,  69),   S( -8,  35),   S( 49,  46),   S(  4,  25),   S( 18,  11),   S(  8, -27),   S(-10, -15),
            S( -2,   6),   S( 14,  38),   S( 10,  21),   S(-13,  21),   S( 29,  13),   S( -8, -14),   S(-14, -14),   S(-18, -21),
            S( -4,   4),   S( 10,  25),   S(  9,  23),   S(  1,   2),   S(  5,  11),   S( -1,  21),   S(-13, -15),   S(-10, -28),
            S( -8, -12),   S(  1, -28),   S(  0,  -7),   S( -1, -15),   S(-19, -10),   S( -6,  -6),   S(  0,  13),   S( -8,   7),

            /* bishops: bucket 9 */
            S(-25, -33),   S( -7,   1),   S(-20,   2),   S(-10, -23),   S(-35, -28),   S(-19, -37),   S(-18, -12),   S(  7,  -5),
            S(-16, -18),   S(-38, -32),   S( -9,  -8),   S(-15,  15),   S(-48,  29),   S(-18, -16),   S(-18, -20),   S( -5,  -6),
            S(  7,  -2),   S( 17,  11),   S(-26, -17),   S(-15,  26),   S(  1,  15),   S(-10, -23),   S(-14, -24),   S( -5,  25),
            S(-15,   7),   S( 16,  18),   S( -8,  28),   S( 10,  24),   S( 18,  27),   S( 11,   5),   S(  6,  -1),   S(-15, -23),
            S( -2,  19),   S( 24,  27),   S(  9,  43),   S( 13,  55),   S(-13,  19),   S(  5,  34),   S( -4,  38),   S( -7,  -5),
            S(-13,   3),   S( 20,  51),   S(  2,  20),   S( 24,  22),   S( 13,  36),   S( -6,  -1),   S(-17,   4),   S(-12, -12),
            S(  3,  12),   S( 17,  10),   S(  4,   9),   S(  3,  46),   S( 20,  38),   S(  7,   5),   S( -8, -15),   S( -5,  -3),
            S( -4, -27),   S( -8,  19),   S( -5,  17),   S(-19, -12),   S(-14,  -4),   S(  6,  26),   S(  0,   3),   S(-13, -19),

            /* bishops: bucket 10 */
            S(-24, -14),   S(  4, -26),   S(-34, -27),   S(-18, -24),   S(-23, -11),   S(-24, -21),   S(-12, -21),   S(-18, -27),
            S(  5, -18),   S(-29, -39),   S( -6,  -9),   S(-41,   5),   S(-38,   6),   S(-20,  22),   S(-30, -56),   S(-12, -17),
            S(  9, -12),   S(  3,  -9),   S(-38, -47),   S(  2,  10),   S(-37,  33),   S(-39,  15),   S(-22,  27),   S(  4,  16),
            S(-10, -20),   S(  6,  11),   S( 13,  -3),   S( 18,   8),   S( 13,  31),   S( -9,  60),   S(  5,  31),   S( 15,  27),
            S(-18,  -1),   S(  3,   4),   S( -3,  19),   S( 35,  31),   S(  5,  66),   S( 24,  52),   S( 12,  42),   S(  1, -17),
            S(  3, -26),   S(-23,   1),   S(-23, -11),   S(-11,  29),   S( 27,  39),   S( 36,  26),   S( 10,  54),   S(  1,  10),
            S(-21,  -9),   S(-11, -46),   S( -9,  -9),   S( 21,  14),   S( -4,  -4),   S( 18,  38),   S( 15,  35),   S( 12,  13),
            S( -7, -29),   S(-10,   3),   S(  5,  17),   S(-11,   3),   S( -8,  15),   S(-10,  -8),   S(  9,  -1),   S(  5,  22),

            /* bishops: bucket 11 */
            S(-20,   1),   S(-32, -14),   S(-50, -47),   S(-23, -30),   S(-21, -11),   S(-66, -46),   S(-10, -12),   S(-23, -23),
            S(-12, -18),   S( -2, -40),   S( -8,  -6),   S(-26, -36),   S(-45,  -9),   S(-29, -28),   S(-26, -44),   S(-23, -34),
            S(-10, -47),   S(  2, -45),   S(-28, -23),   S(  0,  -7),   S( -3,  -4),   S(-36,  11),   S( -9,  25),   S( -2,  19),
            S(-16, -38),   S(-10, -34),   S(  7,  -7),   S(  5,  -4),   S( 15,  25),   S(  1,  62),   S(  9,  51),   S( 18,  26),
            S( -9, -23),   S(-16, -41),   S(-14,  23),   S( 50,   6),   S( 37,  43),   S(  1,  63),   S( 21,  58),   S( 14,  25),
            S(-19, -50),   S(-31,  -1),   S(-12, -36),   S( 10,  16),   S(  6,  33),   S( 19,  30),   S( 28,  38),   S( -4,  -2),
            S( -8,  -7),   S(-19, -43),   S(-19,   2),   S( -5, -16),   S( 10,  -1),   S( 35,  17),   S( -8,   4),   S( 13,  27),
            S(-19, -15),   S(-21,  -3),   S( -6,  12),   S( 10,   3),   S( 11,   1),   S(-17, -25),   S(  4,   7),   S( -2, -21),

            /* bishops: bucket 12 */
            S(  0,   3),   S( -7, -13),   S(-12, -29),   S( -7, -26),   S( -9, -22),   S(-11, -21),   S( -1,  11),   S( -5,  -1),
            S( -7,  -7),   S(-13, -32),   S( -7, -13),   S( -6, -12),   S(-14, -23),   S( -2,  14),   S( -3,  -3),   S( -1,  -9),
            S( -1,  -4),   S(-15,  -2),   S(-13, -19),   S( -8,  -4),   S( -5,   8),   S( -5, -13),   S(-10, -43),   S( -4,  -6),
            S( -2,   3),   S(  5,   1),   S(-17, -29),   S( -2,  12),   S(  2,   7),   S(  6,  25),   S( -4,  -6),   S( -6,  -3),
            S( -1,  -5),   S(  4,  18),   S( -3,  21),   S( -9,   1),   S( -2,  -3),   S( -4,   5),   S(  5,   7),   S( -7,  -1),
            S(-13, -14),   S(  6,  62),   S(-26,   2),   S( -9,  -3),   S(  7, -16),   S( -4,   3),   S(  0,   5),   S( -1,  -5),
            S( -2,  -5),   S( -5,  12),   S(  5,  17),   S( -7,   6),   S( -1,   9),   S(  8,  17),   S( -7, -17),   S( -1,   5),
            S( -2,  -3),   S(  0,  -5),   S( -5,   0),   S(  6,   7),   S(  1,   9),   S(  0,   3),   S(-10,  -1),   S(  1,  -3),

            /* bishops: bucket 13 */
            S( -8, -43),   S(-13, -29),   S(-13, -17),   S(-15, -18),   S(-16, -20),   S( -8,   0),   S( -2,  -5),   S( -8,  -9),
            S( -4,  -7),   S(-11, -12),   S(-13, -29),   S(-19,  -9),   S(-13,   6),   S( -8,   0),   S( -1, -11),   S(  2,  -2),
            S( -9, -11),   S( -6,  -7),   S( -8,   9),   S(-21,   1),   S(-13, -22),   S( -3, -11),   S( -3, -29),   S(  4,  20),
            S( -2,   3),   S(-12,  -3),   S(-13,   4),   S(-24,  11),   S(  2,  19),   S(  4,  -5),   S(  0,   6),   S( -6,  -6),
            S( -3,  10),   S(-16,   6),   S(-15,  -1),   S( 20,   2),   S( -7,   3),   S( -5,   8),   S( -9, -15),   S( -2,  -8),
            S( -3,  -6),   S( -8,   3),   S(-18, -12),   S( 12,  17),   S(  3,   9),   S( -2,  -5),   S(  7,  20),   S( -3,  -6),
            S( -6,  -8),   S( -9,  -3),   S(  7,  27),   S( -7,   9),   S( -7,   0),   S(  2,   1),   S(-15, -26),   S(  0,   7),
            S( -8, -17),   S( -2,   7),   S( -2,  -4),   S(  4,   0),   S( -1,   6),   S( -8,  -6),   S(  1,   9),   S( -3, -14),

            /* bishops: bucket 14 */
            S( -8, -17),   S(-12, -16),   S(-18, -28),   S(-18, -45),   S(-14, -36),   S( -6, -27),   S(-10, -14),   S(-10, -16),
            S(-10, -27),   S( -2, -22),   S( -7, -14),   S(-27, -42),   S(-10, -11),   S(-18, -11),   S(-15, -22),   S(  1, -13),
            S( -9, -12),   S( -9, -31),   S(-22, -30),   S(-13, -18),   S(-26,  -3),   S(-22, -30),   S( -6,   1),   S( -3,  -3),
            S( -8, -23),   S( -8,  -5),   S(-10,  -4),   S(-22,  21),   S(  2,   9),   S(-21,  14),   S(-19, -15),   S( -5, -11),
            S( -9,  -5),   S( -7,  26),   S( -6, -18),   S( -4, -19),   S(-12,  12),   S( -5,  -4),   S(  7,  23),   S(  2,  -5),
            S( -1,   4),   S( -8,   9),   S(-21, -11),   S( -8, -16),   S(  8,  11),   S( -8,  19),   S( -1,  34),   S( -8, -21),
            S( -6, -22),   S( -1,  -1),   S( -7,   2),   S(  4,  18),   S( -9,  -2),   S( -1,   2),   S( -2, -12),   S( -4,  -7),
            S( -7,  -8),   S( -4,  -8),   S( -3,  -7),   S( -2,   6),   S(-10, -18),   S(  1,   8),   S(  6, -12),   S(  1,   3),

            /* bishops: bucket 15 */
            S(  4,   8),   S(  5,   5),   S(-19, -29),   S( -1, -11),   S(-10, -15),   S(-12, -23),   S( -6, -12),   S( -2, -10),
            S(  3,   5),   S( -1,  -8),   S(  3,  -1),   S( -9, -11),   S(-14, -20),   S( -6,  -7),   S( -8, -17),   S( -1,   0),
            S( -7, -14),   S(  0,  -1),   S(-12, -10),   S(-10,  -7),   S(-19, -18),   S(-16, -22),   S( -7, -10),   S(  2,  17),
            S( -4,  -7),   S(-16, -17),   S(  8, -11),   S(-22, -27),   S( -4,   7),   S( -8, -13),   S(  4,  16),   S( -1,  -9),
            S( -1,  -8),   S(-12, -17),   S(-13,  -8),   S(-18, -44),   S( -1, -22),   S(-14,  21),   S(  5,  20),   S(-10, -17),
            S( -9, -32),   S(-11, -11),   S(-18, -33),   S(-20, -11),   S( -4,  -3),   S( -8, -26),   S(  9,  38),   S(  1,  11),
            S( -3,   1),   S( -1, -17),   S( -2, -14),   S( -4,   4),   S(-11, -15),   S( -1,   9),   S(-11,   0),   S(  3,   5),
            S( -4,  -2),   S( -1,   1),   S( -4,   0),   S( -6,  -3),   S( -8,  -5),   S(-17, -20),   S( -8, -24),   S(  1,   0),

            /* rooks: bucket 0 */
            S(-23,   9),   S(-12,   1),   S(-17, -12),   S( -9,  -7),   S(-14,  11),   S( -9,  -7),   S(-15,  23),   S( -3,  19),
            S( 14, -60),   S( 28, -13),   S(  7,  -4),   S(  0,   4),   S( 14,   0),   S(  0,  -6),   S(-31,  22),   S(-43,  33),
            S(  4, -23),   S( 16,  29),   S( 24,   9),   S(  9,  13),   S(-15,  42),   S( -1,   9),   S(-29,  19),   S(-38,  16),
            S( 27, -21),   S( 62,   0),   S( 43,  27),   S( 39,   5),   S( 13,   9),   S( -3,  14),   S(-15,  23),   S(-36,  34),
            S( 61, -21),   S( 90, -16),   S( 66,  -2),   S( 36,  -8),   S( 45,   7),   S( 23,  11),   S( -7,  38),   S(-19,  35),
            S( 67, -42),   S(104, -34),   S( 52,   5),   S( 13,  21),   S( 41,   9),   S(-41,  33),   S( 31,  20),   S(-40,  42),
            S( 44,  -9),   S( 70,  -3),   S( 25,   7),   S(  8,  28),   S( -8,  30),   S( -4,  15),   S(-15,  35),   S(-15,  28),
            S( 33,  19),   S( 14,  47),   S( 19,  27),   S( -3,  38),   S(  6,  19),   S(  9,   1),   S( -3,  30),   S(  4,  26),

            /* rooks: bucket 1 */
            S(-73,  35),   S(-53,   6),   S(-56,  -4),   S(-43, -14),   S(-30, -21),   S(-28, -21),   S(-34,  -8),   S(-38,  21),
            S(-35,   7),   S(-52,  16),   S(-13,  -9),   S(-20, -29),   S(-25, -11),   S(-39, -11),   S(-40, -16),   S(-56,  17),
            S(  4,  11),   S(-21,  33),   S(-12,  14),   S(-38,  26),   S(-43,  32),   S( -3,   1),   S(-22,   9),   S(-43,  22),
            S(-50,  54),   S(-35,  33),   S(  5,  19),   S(-15,  21),   S(-27,  31),   S(-43,  42),   S(-35,  39),   S(-31,  15),
            S( 47,  17),   S( 30,  35),   S( 25,   4),   S(-38,  41),   S(-23,  41),   S( 15,  21),   S( -2,  20),   S(-40,  25),
            S( 41,  11),   S(  6,  29),   S(  5,  26),   S(-37,  30),   S(  8,  12),   S(-31,  43),   S(-11,  24),   S(-46,  33),
            S(-18,  31),   S(  7,  27),   S( 21,  27),   S(-48,  50),   S(-25,  33),   S(  3,  32),   S(-37,  28),   S(-55,  34),
            S( 26,  30),   S( 30,  36),   S( -2,  29),   S(-45,  54),   S(  1,  15),   S( 21,  14),   S(-16,  35),   S( -5,  14),

            /* rooks: bucket 2 */
            S(-67,  38),   S(-48,  20),   S(-47,  14),   S(-57,  14),   S(-60,  12),   S(-48,   6),   S(-32, -22),   S(-48,  28),
            S(-75,  47),   S(-62,  38),   S(-45,  28),   S(-51,  11),   S(-39,   0),   S(-51,   4),   S(-61,  19),   S(-58,  16),
            S(-71,  65),   S(-56,  53),   S(-53,  55),   S(-29,  13),   S(-38,  28),   S(-21,  25),   S(-15,  17),   S(-32,  26),
            S(-72,  66),   S(-57,  67),   S(-41,  62),   S(-36,  49),   S(-27,  34),   S(  3,  33),   S(-36,  53),   S(-19,  34),
            S(-25,  52),   S(-50,  68),   S(-46,  59),   S(-21,  42),   S( 24,  27),   S( 20,  29),   S(-25,  51),   S(-38,  47),
            S(-40,  46),   S(-34,  47),   S(-20,  31),   S( -7,  23),   S( 18,  28),   S( 46,  17),   S( 24,  18),   S(-19,  29),
            S(-55,  42),   S(-70,  70),   S(-35,  54),   S(-13,  49),   S( 11,  28),   S( 22,  21),   S(-54,  60),   S(-34,  46),
            S(-36,  67),   S(-12,  47),   S(-70,  65),   S(-27,  42),   S(-47,  57),   S(-30,  62),   S(-51,  73),   S(-18,  44),

            /* rooks: bucket 3 */
            S( -7,  74),   S( -7,  66),   S( -4,  59),   S(  4,  46),   S(  1,  44),   S(-19,  67),   S(-10,  75),   S( -7,  42),
            S(-31,  87),   S(-11,  68),   S(  4,  61),   S( 10,  56),   S( 20,  48),   S( 14,  55),   S( 43,   5),   S( 23, -37),
            S(-32,  83),   S(-13,  83),   S(  1,  76),   S( 16,  58),   S( 10,  78),   S( 27,  70),   S( 33,  71),   S(  6,  52),
            S(-25,  91),   S(-17,  83),   S( 19,  72),   S( 24,  66),   S( 21,  71),   S( -2, 108),   S( 60,  62),   S( 19,  70),
            S(-14,  99),   S( 23,  79),   S( 15,  69),   S( 37,  68),   S( 39,  67),   S( 48,  66),   S( 91,  51),   S( 57,  45),
            S(-15,  91),   S( 11,  75),   S(  9,  70),   S( 16,  68),   S( 26,  52),   S( 47,  48),   S( 85,  32),   S( 91,  15),
            S(-36, 100),   S(-18,  99),   S( -8,  92),   S( 25,  77),   S( 17,  72),   S( 29,  70),   S( 59,  64),   S(107,  28),
            S(-75, 150),   S( -8, 102),   S(  9,  77),   S( 41,  63),   S( 51,  54),   S( 56,  64),   S(116,  49),   S(101,  46),

            /* rooks: bucket 4 */
            S(-86,  25),   S(-12,  -4),   S(-45,   5),   S(-29,  17),   S(-33, -18),   S(  5, -50),   S( -6, -21),   S(-12, -35),
            S(-33,   2),   S(-43,   7),   S(-42,  15),   S(-37,  24),   S( -9,  -8),   S(-16, -21),   S(  3, -33),   S(-20, -21),
            S(  1,  13),   S(-27, -15),   S(-10,  10),   S( -9,  -9),   S( -2,  -3),   S( -3,  -4),   S( 34, -14),   S(-41,   2),
            S(-29, -10),   S(  4,   5),   S(-21,  16),   S( 28,   0),   S( 17,   6),   S( 16,   0),   S( 15,  11),   S( -7,  14),
            S(-15, -10),   S( -5,  31),   S( -9,  22),   S( 72,   7),   S( 21,  23),   S(  0,  18),   S( 37,  29),   S( 31,   2),
            S( 24,  10),   S( 23,  12),   S( 53,  14),   S( 41,  12),   S( 33,  18),   S(  4,  35),   S(  7,  27),   S( 23,  31),
            S(  3,  -5),   S( 35,  28),   S( 30,  28),   S( 38,  21),   S( 53,  10),   S( 12,   1),   S( 32,  18),   S( 27,  22),
            S( 35, -55),   S( 37,  44),   S( 15,  28),   S( 11,  17),   S( 16,   4),   S(  9,  24),   S( 12,   6),   S( 15,  17),

            /* rooks: bucket 5 */
            S(-49,  34),   S(-58,  53),   S(-67,  51),   S(-61,  37),   S(-47,  24),   S(-46,  41),   S(-13,  26),   S(-43,  45),
            S(-38,  36),   S(-35,  31),   S(-80,  69),   S(-55,  41),   S(-37,  24),   S(-17,  16),   S(  5,  17),   S(-32,  23),
            S( -4,  47),   S(-47,  61),   S(-53,  62),   S(-61,  61),   S(-33,  31),   S( -7,  32),   S( -9,  44),   S(-12,  45),
            S(-32,  74),   S( -3,  47),   S(-23,  66),   S(-10,  43),   S(-15,  56),   S(  6,  61),   S( -4,  55),   S(  4,  37),
            S( 11,  63),   S(  4,  65),   S( 40,  46),   S( 31,  59),   S( 37,  53),   S( 15,  75),   S( 63,  61),   S( 26,  42),
            S( 57,  56),   S( 31,  64),   S( 54,  53),   S( 25,  70),   S( 55,  49),   S( 51,  57),   S( 48,  48),   S( 40,  42),
            S( 40,  41),   S( 21,  63),   S( 42,  54),   S( 62,  41),   S( 34,  49),   S( 41,  56),   S( 61,  48),   S( 63,  44),
            S( 85,  31),   S( 66,  32),   S( 34,  55),   S( 19,  36),   S( 48,  45),   S( 47,  47),   S( 42,  41),   S( 19,  48),

            /* rooks: bucket 6 */
            S(-58,  26),   S(-59,  41),   S(-40,  31),   S(-43,  27),   S(-67,  38),   S(-86,  66),   S(-56,  56),   S(-47,  54),
            S(-39,  34),   S(-28,  33),   S(-24,  32),   S(-43,  26),   S(-52,  48),   S(-73,  65),   S(-64,  58),   S( 16,  15),
            S(-36,  60),   S(-25,  40),   S( -7,  40),   S(-43,  44),   S( -1,  31),   S(-36,  65),   S(-30,  76),   S(  6,  41),
            S(-38,  72),   S( 24,  47),   S( -6,  61),   S(  8,  40),   S( 10,  42),   S(  3,  55),   S(-39,  62),   S(-18,  59),
            S(  0,  73),   S( 36,  60),   S( 55,  46),   S( 35,  43),   S( 23,  61),   S( 40,  50),   S( 42,  47),   S( 12,  58),
            S(  3,  64),   S( 53,  52),   S( 78,  33),   S( 42,  32),   S( 31,  46),   S( 46,  57),   S( 53,  48),   S( 58,  49),
            S( 29,  60),   S( 66,  43),   S( 73,  35),   S( 88,  19),   S( 96,  25),   S( 47,  52),   S( 49,  51),   S( 46,  47),
            S( 51,  72),   S( 25,  64),   S( 32,  49),   S( 42,  40),   S( 64,  46),   S( 52,  63),   S( 56,  60),   S( 18,  66),

            /* rooks: bucket 7 */
            S(-75, -12),   S(-51,  -9),   S(-46, -17),   S(-35,  -9),   S(-26,  -6),   S(-61,  35),   S(-48,  18),   S( -9, -15),
            S(-68,  20),   S(-42,   5),   S(-46,   4),   S(-15, -10),   S(-20,  11),   S( -8,  11),   S(-18,  -1),   S(-55,  11),
            S(-84,  50),   S(-40,  18),   S(-12,   8),   S( -3, -11),   S( -5,   5),   S(-18,  -6),   S(-17, -10),   S( 12,   9),
            S(-64,  42),   S( -9,  22),   S(  5,  14),   S( 18,  11),   S( 31,  -1),   S( 35,   4),   S( 38,   0),   S( -9,   9),
            S(-25,  41),   S( 10,  13),   S( 49, -12),   S( 58,  -8),   S( 75,  -3),   S(105,   0),   S( 81,   2),   S( 49, -13),
            S(-20,  33),   S( 11,  14),   S( 78, -24),   S( 97, -24),   S( 75,  -8),   S( 74,  16),   S( 71,  18),   S( 27,   3),
            S(-16,  35),   S( 17,  18),   S( 45,   3),   S( 65,   0),   S( 93,  -8),   S( 92,  -8),   S( 40,  26),   S( 16,   9),
            S(  5,  60),   S(-27,  43),   S( 31,   3),   S( 78, -24),   S( 25,   5),   S( 17,  16),   S( 44,   6),   S( 62,  -7),

            /* rooks: bucket 8 */
            S(-47, -45),   S(-12, -11),   S(  2,   4),   S( -4, -15),   S(-14, -42),   S(-12, -56),   S(-16, -25),   S( -7, -18),
            S( -3, -17),   S( -5,  -5),   S(  1, -12),   S(  8, -14),   S( -7, -27),   S(-10, -22),   S( -6, -43),   S(-16, -64),
            S(  7,  17),   S(  9, -16),   S(  4,   5),   S( 11,   9),   S(-13, -32),   S( -3, -33),   S( 12,  20),   S( -1,  -1),
            S( -7, -18),   S( -3,  25),   S( -4,   6),   S( 21,   4),   S(  6,  13),   S( -4, -12),   S(  8, -17),   S(  2,   2),
            S( -6,  -8),   S(  2,  17),   S(  0,  28),   S( 16,   9),   S(  3,   6),   S( 22,   6),   S( 10,  -9),   S( 12, -33),
            S(  8,  29),   S( -5,   7),   S( 34,  41),   S( 28,  -7),   S(  1,  -5),   S(  7, -13),   S(  4,  -1),   S( 13,  42),
            S(  5, -10),   S( 14, -16),   S( 26,   3),   S( 21, -20),   S( 31,   8),   S( 22, -20),   S( 20, -13),   S( 17,  -6),
            S(  4, -147),  S( 10, -13),   S( 22,   7),   S( -2,  -9),   S(  2,   0),   S(  3, -12),   S(  6,  -8),   S( 21,  -1),

            /* rooks: bucket 9 */
            S(-48, -16),   S(-12, -21),   S(-23, -27),   S(-38,  -3),   S(-20,   3),   S( -9,  -4),   S( 10, -43),   S(-41, -32),
            S( 29, -19),   S(  2, -16),   S(-15, -17),   S(-18,  -3),   S(-17, -11),   S( 18,   6),   S(  3, -28),   S(-12, -29),
            S( 10, -17),   S( 19,  -5),   S(  4,   6),   S( -8,   0),   S( -7, -21),   S( 27,  -6),   S( 14,  20),   S( -3,   0),
            S(  3,  10),   S(  9,   5),   S( 14,  21),   S( -1,   5),   S(  8,  18),   S( 25,  -3),   S( 16,  37),   S( 13,   2),
            S( 11,   9),   S(  4,  12),   S(  6,  30),   S( 14,  24),   S( 35,  30),   S( 26,  30),   S( 11,   2),   S( 14,  -6),
            S( 16,  44),   S( -7,  11),   S( 12,   3),   S(-12,   2),   S( 15,   5),   S( 31,   8),   S(  8,  35),   S( 15,  14),
            S( 64,  19),   S( 62,   6),   S( 33,  28),   S( 54,  12),   S( 33,  -8),   S( 31,   6),   S( 38,   1),   S( 45,  26),
            S( 66, -69),   S( 40, -29),   S( 20,  26),   S( 32,  26),   S( 12,  36),   S( 26,  16),   S( 22,  11),   S( 31,   9),

            /* rooks: bucket 10 */
            S(-59, -78),   S(-19, -49),   S(-48, -26),   S(-35,  -4),   S(-37,  -3),   S(-30, -12),   S(  8, -15),   S(-36, -19),
            S( -3, -18),   S( 10, -27),   S( -2, -26),   S( -4, -15),   S(  2, -18),   S( -8,  -3),   S( 34,   4),   S(  8,  -2),
            S(-14, -17),   S(-12, -21),   S(  2, -16),   S( 20,  -4),   S(-16,  19),   S( -2,  -6),   S( 25,  26),   S(  7,  -7),
            S(  5,   1),   S(  7, -12),   S(  1,  -5),   S(  7,  15),   S( 30,  -4),   S(  4,  -5),   S( 26,  25),   S(  0,  -8),
            S(  7,  14),   S( 31,   9),   S( 11,  10),   S( 19, -20),   S( -2,  -4),   S( 15,   9),   S( 30,  32),   S(  8,  28),
            S( 38,  30),   S( 30,  42),   S( 22,  10),   S( 21,   8),   S(  4,  -9),   S( 18,  10),   S( 35,  21),   S(  8,  38),
            S( 73,  13),   S( 81,   1),   S( 77,  -5),   S( 72, -18),   S( 56, -17),   S( 38,  13),   S( 27,   6),   S( 32,   7),
            S( 59,  15),   S(  7,  -3),   S( 39,  -2),   S( 22,   6),   S( 36,  -3),   S( 29,  12),   S( 14,   3),   S( 19, -11),

            /* rooks: bucket 11 */
            S(-42, -45),   S(-30, -26),   S(-21, -29),   S(-30, -56),   S( -1, -22),   S( -6,   4),   S(-27, -30),   S(-55, -16),
            S(-16, -29),   S( -7, -43),   S( -2, -29),   S( -1, -29),   S( -4, -24),   S(-16, -18),   S( -3, -32),   S(-21,   1),
            S(  3, -30),   S( 16, -14),   S( 21, -15),   S( 13, -22),   S( 13, -10),   S( -7,   9),   S(-23, -25),   S(-11, -51),
            S(  0,  27),   S( -2, -10),   S( -2,  11),   S( 16,   6),   S(  6,  -4),   S( 15,  29),   S( 28,  -9),   S(  2, -24),
            S( 11,  11),   S( 21, -10),   S( 30,   1),   S( 22,  -8),   S( 27,  -5),   S( 34, -10),   S( 11,   8),   S(  0, -10),
            S( 27,  33),   S( 46,   7),   S( 28, -10),   S( 51,  20),   S( 53,  18),   S( 44,   8),   S( -1,   3),   S( 18,  26),
            S( 64,  36),   S( 61,   3),   S( 71, -13),   S( 77, -15),   S( 48,  -9),   S( 53,  13),   S( 36,  34),   S( 57,  -2),
            S( 45,  32),   S( 14,  27),   S( 23,   5),   S( 11,  -7),   S( -7,  -3),   S( 19,  18),   S( 14,   9),   S( 34,   8),

            /* rooks: bucket 12 */
            S( -4, -10),   S( -9, -30),   S(-13, -52),   S( -4, -10),   S(  0,  -4),   S( -4, -34),   S(-21, -63),   S(-24, -54),
            S(  7,   5),   S( -6, -23),   S(-12, -19),   S( -7, -19),   S(-11,  -8),   S( -8, -15),   S(  1,  -2),   S(-10, -32),
            S(  3,   0),   S( -6, -19),   S( -8, -26),   S(-13,  -9),   S( -5, -23),   S(  6,  -7),   S( -7,  -9),   S(  5,  -8),
            S( -7,  -8),   S( -1, -12),   S(  2,  11),   S(  8, -12),   S(  1,  -8),   S(-10, -38),   S( -7, -12),   S( -4, -38),
            S( -2, -10),   S( -2, -20),   S( 12,   3),   S(  9,   6),   S( -8, -36),   S(  7, -18),   S( -5,  -8),   S(  1, -16),
            S( -2,  -7),   S( -3, -10),   S( 21,  32),   S(  8,  -6),   S( -4,  -6),   S( -6, -21),   S(  1, -26),   S(  5,   9),
            S( -4,  -4),   S(  3, -28),   S(  4, -41),   S( 12,   0),   S(  8,  -2),   S( -5, -39),   S( -2,  -8),   S( 10, -16),
            S( -5, -42),   S(  8,  22),   S(  3, -21),   S(  1,   1),   S( -3, -24),   S(-11, -49),   S(-14, -29),   S(  8,  -4),

            /* rooks: bucket 13 */
            S(-15, -41),   S( -6, -25),   S( -5, -19),   S(  0,  10),   S(  5,  -4),   S(-13, -38),   S(  1, -23),   S(-19, -32),
            S( -3, -32),   S( -3, -14),   S(-12,  -8),   S( -8,  -3),   S(-11, -19),   S( -2, -13),   S(  4,   0),   S( -5, -22),
            S( -6, -30),   S( -7, -27),   S( -5, -35),   S( -3, -24),   S(  9,  12),   S(  1,  -6),   S(  1, -21),   S(  1, -33),
            S( -7, -52),   S(  2,  -6),   S( -9, -42),   S( -5,  -9),   S( 13,  12),   S( -8, -37),   S( -2, -27),   S(  2, -17),
            S( 11, -20),   S(  8, -19),   S( 17,  24),   S( -5,  -9),   S( -9, -28),   S(  4, -14),   S( -6, -39),   S(  9,  -8),
            S( -7, -39),   S( 11, -26),   S( -7, -12),   S( 15,  -7),   S(  6, -11),   S( 11,  16),   S(  8,  -3),   S(  5,   8),
            S(  5,  -4),   S(  9,  19),   S( 10,   9),   S(  3, -16),   S( 11, -27),   S( 21,   7),   S(  3, -13),   S(  4, -17),
            S(-14, -121),  S(-17, -69),   S(  6,   6),   S(  1,   0),   S( -4,  14),   S( -3, -30),   S(-11, -27),   S(  5,   0),

            /* rooks: bucket 14 */
            S( -8, -31),   S(-16, -48),   S( -3,  -8),   S( -3, -34),   S(  2, -25),   S(-10, -23),   S( 10,  -8),   S( -7, -23),
            S(-22, -45),   S(-14, -55),   S(-10,   3),   S(-14, -39),   S(-11, -16),   S(  1, -32),   S(  6,  24),   S(  5, -10),
            S( -3, -24),   S( -9, -20),   S( -4, -18),   S( -6, -13),   S(-13, -25),   S( -8, -22),   S(  7,  22),   S( -2, -28),
            S( 11,   5),   S( -8, -33),   S( -4, -19),   S( -5,   7),   S(  3, -13),   S(  5, -13),   S( -4, -34),   S( -3, -22),
            S(  1, -14),   S(  2, -25),   S( -7, -28),   S( -9, -24),   S( -6, -17),   S( -4, -18),   S(  3,   7),   S(  8,   4),
            S(  3, -14),   S(  0, -24),   S(  1, -17),   S(  2, -19),   S(-10, -19),   S( -8,   5),   S(  6,  10),   S(  0,  -7),
            S( 19,  -1),   S(  0, -36),   S(  2, -21),   S(  2, -29),   S(  6, -44),   S(  7,   1),   S(  9,  10),   S(  9,   8),
            S( -2, -22),   S(  4, -16),   S( -9, -28),   S( 10,  11),   S(-10, -19),   S(  3,   8),   S(  5,  17),   S( -1, -14),

            /* rooks: bucket 15 */
            S( -2, -54),   S(-13, -42),   S( -2, -28),   S( -8, -29),   S(  0, -17),   S( -4,  -9),   S(-17, -53),   S( -9, -14),
            S(-15, -21),   S(-13, -27),   S(  2,  -1),   S( -7, -24),   S(-11, -30),   S(  6, -28),   S(-11, -42),   S(  7,   5),
            S( -9, -24),   S(-10, -23),   S( -3, -24),   S(  2,   0),   S(  9, -28),   S( -4,  -9),   S( -3,   4),   S( -4, -14),
            S(  2, -31),   S( -4, -26),   S(-11, -18),   S( -5, -20),   S(-11, -20),   S(  3, -19),   S(  0, -19),   S( -9,  -2),
            S(  0, -11),   S( -5, -12),   S( 10,  -7),   S( -1, -11),   S(  1,  -2),   S(  2,  -2),   S( -1,   5),   S(  1,  17),
            S(  7,  18),   S(  2,   0),   S(  1, -14),   S(  0, -11),   S( -6,  -9),   S(  2,  13),   S(  7,  -9),   S( -7, -13),
            S( 11,  20),   S( 12,  -6),   S(  9, -32),   S( -3, -34),   S(  1, -20),   S( 13,  35),   S(  3,  -4),   S(  0,  12),
            S(  1, -17),   S( -6, -18),   S(  3,  -6),   S(  1, -12),   S( -6, -15),   S(  0, -26),   S(  1, -18),   S(  2,  -3),

            /* queens: bucket 0 */
            S(-21, -12),   S(-21, -56),   S( 48, -88),   S( 55, -57),   S( 31, -37),   S( 17,  -2),   S( 54,   7),   S( 20,  19),
            S(-11, -13),   S( 34, -61),   S( 40, -14),   S( 22,   6),   S( 23,  29),   S( 24,  19),   S(  9,  62),   S( 37,  22),
            S( 27,   6),   S( 41,  19),   S( 24,  29),   S( 20,  35),   S( 19,  16),   S( 12,  15),   S( 10,  29),   S( 35,  33),
            S( 19,  23),   S( 26,  46),   S(  8,  47),   S(  9,  45),   S(  8,  56),   S( 14,  32),   S( 17,  26),   S( 19,  29),
            S( 41,  50),   S( 29,  43),   S( 19,  40),   S( 22,  53),   S( -5,  27),   S( -6,  12),   S( 31,  21),   S( 45,  -4),
            S( 26,  61),   S( 24,  53),   S( 13,  37),   S( 19,  14),   S( 44,  -9),   S(  5,  36),   S( 27,  20),   S( 23, -20),
            S( 47,  50),   S( 52,  43),   S( 32,  37),   S( 50,  26),   S( 20,   8),   S( -7,  -9),   S( 30,  25),   S( 30,  12),
            S( 46,  29),   S( 23,  37),   S( 43,  18),   S( 34,  36),   S( 46,  31),   S(-13,   3),   S( 50,  28),   S( 46,  28),

            /* queens: bucket 1 */
            S( -3, -18),   S(-75, -25),   S(-53, -29),   S(-17, -67),   S(-11, -26),   S(-19, -46),   S( 13, -31),   S(  9,  26),
            S(-17, -28),   S(-10, -44),   S(  9, -46),   S( -4,   5),   S( -9,   3),   S(  5,  -3),   S( 20, -39),   S(  1,  20),
            S(-29,  46),   S(  1,  -2),   S(  6,  15),   S( -4,  10),   S( -5,  35),   S(-15,  31),   S( 16,   9),   S( 19,  20),
            S(  9, -18),   S(-11,  31),   S(-14,  36),   S(  8,  45),   S( -7,  51),   S(  1,  28),   S(  2,  -2),   S( 18,  17),
            S( 14,   7),   S(  6,  24),   S( -3,  62),   S(-20,  60),   S(-15,  51),   S(  0,  14),   S( -9,  16),   S(  2,  34),
            S(  9,  26),   S( 14,  51),   S( 15,  57),   S(-37,  54),   S(-18,  46),   S(-36,  45),   S( 26,  23),   S( 18,  40),
            S(  2,  34),   S(-12,  68),   S(-21,  34),   S(-26,  69),   S(-26,  47),   S( 13,  27),   S(-10,  38),   S(-26,  45),
            S( -5,   6),   S(  5,  16),   S( 14,  26),   S(-10,  11),   S( -3,  13),   S(  5,  12),   S(  8,  24),   S( -9,  28),

            /* queens: bucket 2 */
            S(  7,  16),   S( 14, -38),   S(  8, -23),   S( -3, -16),   S(-24,   5),   S(-25, -17),   S(-27, -24),   S( 14,   9),
            S( 15,  10),   S( 11,  34),   S( 18, -12),   S( 17, -19),   S( 14, -25),   S( 16, -46),   S( 12,  -8),   S( 34, -27),
            S( 17,   9),   S( 17,  10),   S(  4,  45),   S(  7,  38),   S(  4,  59),   S( 15,  49),   S( 12,  22),   S( 30,  15),
            S(  6,  22),   S( -2,  53),   S( -2,  42),   S(  3,  57),   S(-20,  82),   S( -3,  84),   S( 13,  21),   S(  5,  67),
            S( 15,   5),   S( -8,  55),   S( -9,  53),   S(-32,  95),   S(-38, 108),   S(-15,  77),   S(-10, 104),   S( -5, 104),
            S( 11,  22),   S( -1,  43),   S(-31,  77),   S(-10,  53),   S(-32,  91),   S(-15,  96),   S( -3,  93),   S( 10,  72),
            S(-22,  52),   S(-37,  77),   S(-17,  60),   S(  5,  60),   S(-22,  74),   S( 24,  41),   S(-23,  46),   S(-13,  77),
            S(-68,  76),   S(  2,  35),   S( 28,  36),   S( 30,  32),   S(  2,  64),   S( 19,  31),   S( 13,  25),   S(-15,  39),

            /* queens: bucket 3 */
            S( 83,  89),   S( 58,  91),   S( 49,  99),   S( 43,  79),   S( 68,  29),   S( 47,  20),   S( 20,  20),   S( 42,  54),
            S( 67, 113),   S( 60, 108),   S( 46, 112),   S( 48,  88),   S( 49,  78),   S( 64,  47),   S( 68,  10),   S( 40,  46),
            S( 63,  87),   S( 54, 104),   S( 57,  79),   S( 55,  75),   S( 52,  89),   S( 56,  98),   S( 64, 101),   S( 66,  75),
            S( 48, 122),   S( 61,  82),   S( 48,  93),   S( 41,  94),   S( 42,  93),   S( 41, 129),   S( 59, 103),   S( 52, 137),
            S( 65,  90),   S( 58, 103),   S( 54,  85),   S( 39,  95),   S( 35, 113),   S( 28, 125),   S( 39, 165),   S( 54, 155),
            S( 49, 120),   S( 58,  97),   S( 51,  93),   S( 26, 115),   S( 32, 131),   S( 74, 100),   S( 65, 135),   S( 35, 187),
            S( 61, 114),   S( 61, 101),   S( 72,  81),   S( 61,  92),   S( 34, 109),   S( 60, 110),   S( 92, 125),   S(158,  69),
            S( 77,  89),   S(101,  76),   S( 77,  84),   S( 81,  79),   S( 40, 106),   S(111,  51),   S(136,  57),   S(143,  56),

            /* queens: bucket 4 */
            S(-12, -23),   S(-19, -20),   S(-25,  -8),   S( -9,  -8),   S( 10, -15),   S( 33,   0),   S(-35, -10),   S(-25,  -2),
            S(-29, -18),   S(-28,  -5),   S( 14,  -7),   S(-40,  23),   S(  2,  -7),   S( -1, -14),   S(-10, -11),   S(-37, -16),
            S(  3,   2),   S( 12,  -1),   S(  4,  29),   S( -1,  31),   S( 23,  15),   S(  5,  -9),   S(  8, -20),   S(-26, -24),
            S(-15,   3),   S( -5,  15),   S(  4,  37),   S( -2,  32),   S( 15,  34),   S( 19,  18),   S(  0, -15),   S( -4,  -7),
            S( -6,   0),   S( 17,  13),   S( 18,  29),   S( 30,  42),   S( 23,  28),   S( 20,   0),   S(-22, -18),   S( -9, -29),
            S(  3,  12),   S( 36,  14),   S( 27,  53),   S( 23,  44),   S( 10,   8),   S(  1,   3),   S(-16, -14),   S(-13,  -8),
            S(-11, -19),   S( -5,  18),   S(  4,  27),   S( 32,  34),   S( 10,  11),   S(-13,  -4),   S(-22, -42),   S(-20, -26),
            S( -4, -18),   S( -2,  -3),   S( 30,  37),   S(  5,  19),   S(-17, -17),   S( -7, -10),   S(-20, -35),   S(-10, -19),

            /* queens: bucket 5 */
            S(-38, -15),   S(-26, -30),   S(-32, -29),   S(-44, -28),   S(-57, -30),   S(  7, -16),   S( -9,  -6),   S( -5,  -7),
            S(-28,  -4),   S(-40, -13),   S(-68, -20),   S(-66,  -3),   S(-14,  -3),   S(-41, -16),   S(-48, -17),   S(-50, -16),
            S(-35,   2),   S(-59, -13),   S(-64,   6),   S(-31,  33),   S( 16,  52),   S(-12,  21),   S( -5,  -1),   S( 10,  20),
            S(-53, -10),   S(-51,  -3),   S(  1,  38),   S( -3,  53),   S( 13,  29),   S( -3,  14),   S( -3,  -7),   S( -9,  14),
            S(-33,  -6),   S(-22,  20),   S(-10,  50),   S( -5,  46),   S( 28,  48),   S(  0,  17),   S( -1,   8),   S(-30, -29),
            S(-19,  15),   S(  7,  37),   S(-12,  42),   S(  1,  45),   S( 40,  49),   S(  2,  12),   S(  0,   1),   S(-11, -10),
            S( -9,   5),   S(-10,  13),   S(  4,  59),   S( -3,  33),   S(  1,  38),   S( 21,  32),   S( 10,   8),   S(-22, -17),
            S(  7,  25),   S( 11,  11),   S(  2,  18),   S( 11,  48),   S( 15,  28),   S(  4,  21),   S( -2, -25),   S(-19, -16),

            /* queens: bucket 6 */
            S(-31,   5),   S(-52, -25),   S(-68, -29),   S(-85, -59),   S(-92, -51),   S(-72, -46),   S(-54, -44),   S(-28,   2),
            S(-63, -13),   S(-47,  -1),   S(-53,  13),   S(-65,  13),   S(-78,  16),   S(-89,  -1),   S(-85, -19),   S(  7,  18),
            S(-45,  10),   S(-22,  10),   S(-56,  40),   S(-99,  87),   S(-39,  52),   S(-35,   4),   S(-47, -14),   S( -1,   4),
            S(-42,  11),   S(-25,   9),   S(-28,  64),   S(-48,  69),   S(  5,  46),   S( 15,  52),   S(-11,  35),   S( 11,  -8),
            S(-55,  21),   S( -7,  37),   S(-29,  53),   S(  9,  30),   S( 31,  54),   S( 59,  37),   S( 26,  31),   S( -5,  19),
            S(-26,  40),   S(-13,  18),   S( 20,  21),   S( 20,  45),   S(  8,  52),   S( 62,  66),   S( -6,  -7),   S(-17,   9),
            S(-10,   5),   S(  0,   0),   S(-14,  40),   S(-12,  34),   S( 27,  50),   S( 17,  59),   S(-10,  21),   S(-39,  -3),
            S( -4,   5),   S( 16,  10),   S( 10,  29),   S( -4,  22),   S( 30,  39),   S( 18,  26),   S( -3,  14),   S(  2,   7),

            /* queens: bucket 7 */
            S( -9, -13),   S(-36,  13),   S(-53,  22),   S(-37,  12),   S(-32, -10),   S(-36, -24),   S(-33,  -7),   S(-17, -11),
            S(-34,  -9),   S(-52,   5),   S(-26,   7),   S(-24,  36),   S(-30,  31),   S(-45,  38),   S(-45,  23),   S(-35, -13),
            S(-37, -22),   S(-51,  29),   S(-20,  31),   S(-11,  27),   S(  9,  18),   S(  2,  26),   S( -9,  15),   S(-17,   0),
            S(-62,   1),   S(  5,   2),   S(-20,  24),   S( -5,  38),   S( 35,  18),   S( 35,  22),   S( 16,  33),   S( -2,  18),
            S(-30,  20),   S(-53,  25),   S(  8,  17),   S( 49,  -9),   S( 63, -13),   S( 87, -18),   S( 38,  10),   S( 40,  -8),
            S(-17,  11),   S(-19,   9),   S(  5,  -2),   S( 14, -10),   S( 36,  35),   S( 81,  20),   S( 65,   3),   S( 41,  10),
            S(  7, -18),   S(  1,  11),   S(  0,  -6),   S(  4,  14),   S( 37,  17),   S( 54,  39),   S( 53,  18),   S( 49,  25),
            S( 14,   4),   S( 18,   4),   S( 20,   8),   S( 18,  16),   S( 40,  25),   S( 23,  19),   S( 14,   6),   S( 38,  44),

            /* queens: bucket 8 */
            S( -5, -10),   S( -1,   4),   S(-13,  -4),   S( -8,  -6),   S( -4,   0),   S( -1, -16),   S(-19, -24),   S( -3,   5),
            S( -7,   0),   S(-11, -15),   S( -3,   5),   S(-13,  -3),   S( -5,  -4),   S(-17, -20),   S(-19, -39),   S( -4,  -8),
            S( -2,  -1),   S( -6,   2),   S( -7,   2),   S( -5, -10),   S( -5,   3),   S(-12, -12),   S(-11, -26),   S(-15, -27),
            S( -3,   3),   S( 10,  20),   S( 12,  20),   S(  5,  11),   S( -3,  -1),   S( -6,  -1),   S( -1,  -3),   S( -7, -21),
            S( 16,  29),   S(  4,  29),   S( 12,  15),   S( 12,  19),   S( 12,  31),   S(  4,   0),   S( -8,  -9),   S(-11, -17),
            S(  8,  20),   S( 13,  23),   S(-17,  17),   S( 15,  35),   S( -7, -14),   S( -5, -11),   S(  4,   2),   S(  3,  13),
            S( -6, -12),   S(-16, -25),   S( 22,  36),   S( 15,  16),   S(  3,  18),   S(  3,  18),   S( -3,  -7),   S( -7, -16),
            S(-13, -27),   S( 13,  10),   S(-15, -47),   S( -8,  -4),   S(-11, -29),   S( -1,  -6),   S( -2, -16),   S( -5,  -7),

            /* queens: bucket 9 */
            S(  5,   6),   S(-13, -27),   S(  2,  -2),   S(-29, -31),   S(-23, -38),   S(-18, -31),   S(-13, -21),   S(-13, -18),
            S( -3,  -5),   S( -9,  -7),   S(-18, -23),   S( -4,   0),   S(-17,  -9),   S(-16, -20),   S(  2,  -2),   S( -4,  -7),
            S(  4,   6),   S(  3,   8),   S( -8,  20),   S( -4,  -5),   S( -6,   7),   S(  0,  -3),   S(  3,   2),   S(  3,   0),
            S( -5,  -9),   S( -5,   5),   S( 14,  42),   S(  8,  23),   S( 19,  33),   S(  4,  11),   S( -9, -16),   S(  0, -10),
            S(  6,  10),   S(  9,  32),   S( 13,  34),   S( 18,  52),   S( 21,  35),   S( 10,  19),   S( -3,   5),   S(-11, -12),
            S(-18, -19),   S(-16,  -3),   S(  6,  23),   S( 16,  37),   S( -4,   3),   S( -1,  10),   S( -8,  -6),   S( -5,  -7),
            S( -6, -17),   S(-10, -24),   S( -8,  24),   S( 12,  31),   S( 17,  21),   S(  7,  -5),   S(  6,  -4),   S(-12, -25),
            S( -1,  -2),   S( -4, -23),   S( 12,  -1),   S(  1,  16),   S( 14,   2),   S( -3,  -1),   S( 10,   2),   S(  2, -15),

            /* queens: bucket 10 */
            S(  3,  -1),   S( -3,   1),   S(-11, -18),   S(-22, -25),   S(-12, -14),   S( -6,  -6),   S(  2, -11),   S( -5, -10),
            S( -8, -11),   S( -8, -15),   S(-15, -25),   S( -9, -11),   S( -5,  -6),   S(-18, -13),   S(  1,  -8),   S(-17, -18),
            S( -1, -13),   S(-10, -14),   S( -9,  -8),   S( -3,   1),   S( -7,   1),   S( -8,   4),   S(  1,   1),   S(  3,   7),
            S(  0,  -2),   S(  2,  -3),   S( -3,  -6),   S(  0,  31),   S( 15,  26),   S( -6,   5),   S( -3,  -6),   S(-13, -19),
            S( -6,  -7),   S(  5,  -6),   S( -4,   5),   S( 21,  48),   S(  1,  -1),   S( 18,  31),   S( 12,  13),   S(  0,   6),
            S( -3,  -5),   S(-20, -32),   S( -4,   0),   S(  2,  14),   S(  6,  17),   S(  5,  22),   S( 12,   8),   S( -5, -11),
            S( -6,  -7),   S(-18, -28),   S(  8,  22),   S( -6,  -7),   S(  7,   6),   S(  4,  10),   S( -3,  -8),   S( -9,  -6),
            S(  5,  -1),   S( -3, -18),   S(  6,  -3),   S(  7,  -5),   S( 17,  15),   S(  5,   7),   S( 15,  14),   S(  1,  -9),

            /* queens: bucket 11 */
            S(-10, -14),   S( -7, -19),   S(-21, -20),   S(-11, -28),   S(-12, -19),   S( -9, -11),   S( -5,  -6),   S(-13, -23),
            S(-16, -32),   S( -9,  -7),   S(-41, -35),   S(-10,  -9),   S(-12, -10),   S(-10,  -6),   S( -5,  -9),   S( -6,  -3),
            S(-17, -22),   S(-16, -35),   S(  3, -21),   S(-10, -18),   S( -8, -14),   S( -2,   5),   S(  8,  20),   S(-12,  -8),
            S(-17, -28),   S(-25, -25),   S( -7, -26),   S( 14,  25),   S( 10,   0),   S(-11,  -5),   S( 24,  25),   S( -2,   1),
            S(-14, -13),   S( -5, -16),   S(-22, -25),   S( 25,  20),   S( 15,  14),   S( 28,  52),   S( 22,  41),   S(  2,  12),
            S(-14, -30),   S(  3,   3),   S(-16, -17),   S( 16,  12),   S( 25,   6),   S( 46,  38),   S( 10,  -1),   S( -8,  -6),
            S( -8,  -3),   S(-14, -23),   S(  9,  17),   S(-13,  -4),   S(  6,   7),   S( 24,  26),   S( 37,  39),   S( -2, -16),
            S(-12, -22),   S( -9, -24),   S( -7, -21),   S(  5, -13),   S(  2,  10),   S( -2,  -7),   S( 19,   7),   S( -1, -31),

            /* queens: bucket 12 */
            S(  6,   0),   S(  0,  -1),   S(  2,   1),   S( -8,  -5),   S(-10, -12),   S( -1,  -3),   S(  0,  -2),   S( -4, -10),
            S( -3,  -2),   S( -8, -14),   S( -9, -11),   S( -5, -11),   S( -3,  -2),   S( -6,  -2),   S( -1,  -9),   S( -5,  -9),
            S( -2,  -5),   S( -6, -10),   S( 11,  13),   S( -5,  -5),   S( -2,  -6),   S( -8, -13),   S(-12, -24),   S( -8,  -7),
            S(  2,   7),   S( -1,   3),   S(  4,   6),   S(  0,   7),   S(  7,  14),   S(  0,  -4),   S(  0,  -4),   S( -4, -11),
            S(  1,  -3),   S( 11,  13),   S( 32,  57),   S(  1,  15),   S( -5,   7),   S(  0,   6),   S(-13, -30),   S( -2, -14),
            S(  8,  18),   S( 14,  25),   S( 34,  44),   S( -1,   8),   S(  0,   5),   S(  2,   2),   S(  5,   5),   S( -5, -15),
            S(  3,   2),   S(  3,   7),   S( 18,  17),   S( 11,   9),   S(  5,  10),   S( -4,   2),   S(  9,   6),   S( -4,  -4),
            S( -4, -28),   S( -9, -26),   S(-11, -21),   S( -8, -26),   S( 11,  -6),   S(  1,  -1),   S(  2,  -5),   S( -6, -11),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -5, -14),   S(  0,  -5),   S( -3,  -7),   S( -3, -10),   S( -2,  -2),   S( -7, -10),   S( -6,  -8),
            S(  4,  10),   S(  5,  14),   S(  4,  10),   S( -4,  -3),   S( -6,  -6),   S(  2,  10),   S(  1,   5),   S(-11, -19),
            S( -3,  -7),   S(  0,   0),   S(  3,  16),   S(  2,  11),   S( -3,  -2),   S( -6,  -9),   S( -5, -12),   S(-12, -16),
            S( -3,  -4),   S(  2,   3),   S( 12,  12),   S( 18,  27),   S( 15,  32),   S( -4,  -8),   S( -5, -14),   S( -5,  -6),
            S( -3,  -1),   S(  6,  19),   S( 16,  41),   S( 12,  38),   S( 23,  43),   S(  0,  -9),   S( -4,  -7),   S( -7, -14),
            S(  0,   0),   S( 13,  33),   S( 39,  75),   S( 20,  42),   S(  1,  17),   S(  1,   8),   S(  6,  15),   S( -5, -14),
            S( -1,   0),   S( 19,  32),   S( 10,  29),   S( 14,  27),   S( -1,  11),   S(  1,  -8),   S( -1,  -9),   S(  5,   9),
            S(-11, -17),   S(  5,  -2),   S( -2,  -7),   S( -9, -11),   S(  7,   3),   S(  5,   8),   S( -8,  -7),   S( -6, -13),

            /* queens: bucket 14 */
            S( -1,  -2),   S(  0,   1),   S( -1,  -7),   S( -9,  -9),   S(  4,   7),   S( -2,  -4),   S( -2,  -9),   S( -5, -10),
            S( -5,  -7),   S(  6,  16),   S( -2,  -3),   S( -1,  -6),   S( -9, -11),   S( -7, -14),   S( -5,  -4),   S( -3,  -7),
            S( -2,  -2),   S(-10, -12),   S( -6, -13),   S(  0,  -1),   S(  1,   0),   S(  1,  -4),   S(  3,   6),   S( -6, -14),
            S( -8,  -9),   S(  8,  10),   S( -6,  -3),   S( 22,  40),   S( 14,  14),   S( -1,   6),   S( 11,  24),   S(  1,  -4),
            S(  4,  13),   S(  4,   1),   S(-13,  -8),   S( 16,  27),   S( 14,  34),   S( 17,  25),   S(  9,  19),   S( -4,  -9),
            S( -2,  -5),   S(  5,  15),   S( 14,  25),   S( 13,  22),   S( 18,  43),   S( 14,  47),   S(  7,  16),   S( -3,  -8),
            S(  3,   7),   S(  8,  10),   S( 16,  37),   S( 20,  34),   S( 15,  34),   S( 14,  27),   S( 16,  28),   S(  1,   5),
            S( -4,  -1),   S(  0,   1),   S( -9, -14),   S( 12,  19),   S(  1,   4),   S(  3,   0),   S(  2,   6),   S(-10, -17),

            /* queens: bucket 15 */
            S( -1,  -2),   S(  1,  -5),   S( -5,  -8),   S( -2, -10),   S( -6, -10),   S( -6, -12),   S(-11, -24),   S(  1,  -6),
            S( -1,  -3),   S( -4,  -8),   S( -5, -13),   S( -4, -11),   S(  1,   9),   S( -3,  -7),   S( 11,  13),   S(  2,   1),
            S(  0,  -9),   S( -3, -11),   S(  0,  -1),   S( -4, -11),   S( -4, -11),   S(  6,  17),   S( -1,  -4),   S(  0,  -8),
            S( -5,  -8),   S(  4,   4),   S( -4,  -3),   S(  4,   2),   S(  2,  10),   S(  0,   6),   S(  6,   6),   S(  4,   3),
            S( -2,  -7),   S( -2,  -5),   S( -8, -13),   S( -4,  -5),   S(  5,  11),   S(  8,   6),   S( -3,  -6),   S(  0,  -7),
            S( -3,  -6),   S( -2,  -6),   S( -1,   3),   S(  1,   1),   S( -1,  -5),   S( 20,  30),   S(  4,   0),   S(  0,  -9),
            S( -6, -13),   S(  4,  -6),   S(  6,   8),   S(  7,   7),   S(  6,   8),   S( 22,  39),   S( 11,  20),   S(  4,   5),
            S(  1,  -4),   S( -5,  -5),   S( -2,  -4),   S( 10,  13),   S(  8,   3),   S(  4,  -2),   S( -2,  -7),   S( -6, -21),

            /* kings: bucket 0 */
            S( 66,   3),   S( 48,  52),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 38,  39),   S(105,  64),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 46,  20),   S( -5,  38),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 69,  52),   S( 48,  63),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -9,  40),   S(  0,  28),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 38,  67),   S( 47,  50),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( -1,  50),   S(-22,  18),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 15,  84),   S(-36,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 23, -59),   S( 74, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-32, -10),   S( 15,  19),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 38, -12),   S( 18,   2),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  6,  35),   S( -9,  32),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 43,  20),   S( 15,  13),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,  53),   S( -7,  47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 64,  29),   S( 24, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 31,  64),   S( -4,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46, -125),  S(  9, -62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38, -109),  S(-98, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  4, -52),   S(-39, -43),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-40, -33),   S(-50, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-21, -36),   S(-19, -38),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-43, -22),   S(-89,   3),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-22, -42),   S(-31, -112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-86, -11),   S( -4, -95),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -112),  S(-74, -29),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -229),  S(-10, -101),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-67, -59),   S( 20, -66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-51, -77),   S(-19, -102),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-19, -54),   S(-111, -19),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 25, -117),  S(-59, -71),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-125,  -2),  S(-25, -118),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-46, -74),   S(  1, -229),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -22),   S(-26,  14),   S( 10,  -2),   S(-11,  28),   S( 20,   2),   S( 43,   8),   S( 48,  -8),   S( 47,   3),
            S(-12, -27),   S(-31,   3),   S( -1, -11),   S(  0, -12),   S( 16,   4),   S(  1,  14),   S( 26,  -1),   S( 21,  26),
            S(  1, -27),   S( -4, -20),   S( 28, -32),   S(  9, -15),   S( 17,  -8),   S(  5,  28),   S( -8,  48),   S( 26,  24),
            S(  7, -17),   S( 28,   3),   S( 50, -27),   S( 35,  -5),   S( 13,  47),   S(-18,  85),   S(  4,  86),   S( 51,  66),
            S( 90, -54),   S(124, -19),   S( 87, -23),   S( 45,  16),   S( 43, 138),   S( -6, 139),   S( 12, 156),   S( 63, 134),
            S(-216, -73),  S(-117, -135), S( 15, -168),  S( 36,  43),   S( 82, 197),   S( 65, 188),   S(107, 167),   S( 95, 148),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  20),   S(-43,  26),   S(-18,  11),   S(-39,  58),   S(-13,   2),   S( 16,   9),   S( 15,   1),   S( 14,  27),
            S(-53,  17),   S(-50,  20),   S(-31,  10),   S(-19,   8),   S(  2,   8),   S(-14,  10),   S( -7,   3),   S(-16,  23),
            S(-49,  24),   S(-23,  21),   S(-27,   7),   S(  5,  -6),   S( -2,  20),   S(-25,  20),   S(-35,  32),   S(-18,  30),
            S(-38,  44),   S(  7,  25),   S(-20,  26),   S( 11,  26),   S(  0,  29),   S(-35,  45),   S( -4,  40),   S( 23,  57),
            S(  6,  36),   S( 62,  -1),   S( 94, -24),   S( 86, -20),   S( 35,  30),   S(  4,  36),   S(-27,  80),   S( 37,  93),
            S( 48,  43),   S(-30, -21),   S( -5, -101),  S( -9, -96),   S(-37, -67),   S( -6,  48),   S( 46, 185),   S( 63, 215),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42,  41),   S(-29,  23),   S(-19,  14),   S(  0,   5),   S(-25,  33),   S(-12,  13),   S(  3,  -9),   S( -7,  22),
            S(-51,  32),   S(-40,  29),   S(-30,   9),   S(-27,  19),   S(-23,  19),   S(-33,   9),   S(-16,  -9),   S(-38,  14),
            S(-47,  50),   S(-41,  52),   S(-15,  17),   S(-17,  20),   S(-20,  22),   S(-27,   6),   S(-32,   9),   S(-34,  13),
            S(-35,  89),   S(-40,  73),   S(-15,  43),   S(  0,  36),   S( -8,  35),   S(-24,  18),   S(  3,  19),   S( 18,  14),
            S(-30, 133),   S(-44, 118),   S(  0,  23),   S( 26, -24),   S( 97,  -9),   S( 91,  -6),   S( 75, -16),   S( 51,   5),
            S(-13, 248),   S( 35, 176),   S( 14,  71),   S( 30, -92),   S(-12, -170),  S(-76, -131),  S(-32, -63),   S( 18,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,  15),   S(  0,  15),   S(  9,  12),   S(  2,  33),   S( -4,  50),   S( 31,  22),   S( 21,  -2),   S(  7, -11),
            S( -2,  18),   S( -2,  26),   S( -1,  10),   S( -2,  11),   S( 10,  18),   S( 15,   1),   S(  7, -10),   S(-20,  -3),
            S(  0,  37),   S(-11,  57),   S(  6,  19),   S(  5,   2),   S( 24, -10),   S( 12, -11),   S( -1, -19),   S(-18,  -9),
            S( -1,  92),   S(-18, 103),   S(  8,  65),   S( 17,  31),   S( 22,   2),   S( 30, -23),   S( 19,   5),   S( 30, -17),
            S( -1, 157),   S(-14, 167),   S(-25, 167),   S( -9, 113),   S( 35,  52),   S( 85, -15),   S(112, -34),   S( 96, -39),
            S(100, 128),   S( 41, 241),   S( 22, 254),   S(  4, 209),   S(-27,  95),   S( 27, -175),  S(-63, -240),  S(-155, -178),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 70,   0),   S( 26,   5),   S(  1, -10),   S(-13,  -9),   S(  4, -13),   S(  8, -11),   S(  1, -10),   S(-56,  43),
            S( 42,  -4),   S( 10,  19),   S(  6,  -3),   S(-11,  -6),   S(-19, -22),   S(-13, -17),   S(-31, -20),   S(-43,   4),
            S( 63, -14),   S(108, -29),   S( 30, -16),   S(-29,  -2),   S(-72,  10),   S(-13,   3),   S(-76,  23),   S(-66,  33),
            S(-79, -74),   S( -9, -93),   S( 73, -60),   S(-28,   4),   S(-25,  17),   S(-62,  65),   S(-40,  56),   S(-54,  78),
            S(-26, -75),   S(-62, -113),  S( -6, -91),   S( 56,   6),   S( 74,  88),   S( -5, 101),   S( 16,  76),   S( -3, 102),
            S(  5, -61),   S(-15, -77),   S(  1, -65),   S(  2,  47),   S( 56,  87),   S( 64, 150),   S( 43, 155),   S( 55, 125),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  46),   S(-42,  43),   S( -2,  22),   S( 58,   7),   S( 65,  -2),   S( 14,   2),   S(-20,  10),   S(-54,  48),
            S(-74,  39),   S(-38,  40),   S(-18,  24),   S( -1,  22),   S(-17,  23),   S(-25,   7),   S(-54,   6),   S(-74,  34),
            S(-36,  28),   S(-36,  54),   S( 25,  28),   S(  6,  41),   S(-29,  43),   S(-63,  32),   S(-68,  34),   S(-65,  45),
            S(-27,  38),   S(-13,   9),   S(-22, -39),   S(  9, -27),   S( -7,  -6),   S(-49,  30),   S(-11,  30),   S(-31,  57),
            S( 60,   7),   S( -5, -34),   S( 31, -94),   S(  7, -72),   S( 51, -41),   S( 21,  22),   S(-17,  69),   S(-40, 117),
            S( 50,  30),   S( 18, -14),   S(-27, -66),   S(-17, -60),   S(-30, -58),   S( 47,  40),   S( 63, 136),   S( 40, 148),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-87,  42),   S(-56,  19),   S(-14,   3),   S( 13,   4),   S( 11,  25),   S( 21,  10),   S( 14,   7),   S(  2,  27),
            S(-77,  25),   S(-59,  17),   S(-45,   9),   S( 18,  11),   S(-10,  27),   S( -8,  12),   S(-17,  13),   S(-10,  14),
            S(-65,  36),   S(-77,  43),   S(-50,  30),   S(-40,  43),   S( -1,  41),   S( 11,  19),   S( -1,  23),   S(-16,  19),
            S(-93,  90),   S(-61,  58),   S(-27,  31),   S(-17,  15),   S( -5, -33),   S(-11, -29),   S(-25,   6),   S( 27,   0),
            S(-15, 104),   S(-48,  72),   S( 28,  11),   S( -5, -33),   S(  7, -72),   S(-37, -67),   S( -6, -32),   S( 81,  -4),
            S( 75,  78),   S( 68,  90),   S( 47,  23),   S( 41, -80),   S( -4, -103),  S(-38, -53),   S( -7, -47),   S( 77,   1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-53,   4),   S(-39, -13),   S(  0, -23),   S(-61,  47),   S( 24,   5),   S( 70, -20),   S( 59, -26),   S( 69, -10),
            S(-59,   4),   S(-63,   5),   S(-34, -19),   S(-35,   4),   S(  2,  -2),   S( 45, -28),   S( 28, -13),   S( 50, -16),
            S(-60,  26),   S(-81,  40),   S(-42,   7),   S(-46,   3),   S( -2,  -2),   S( 18, -15),   S( 53, -13),   S( 51, -16),
            S(-58,  63),   S(-92,  80),   S(-56,  59),   S(-35,  35),   S(-15,  -1),   S( 38, -59),   S( 22, -71),   S( 27, -107),
            S( 14,  63),   S(-64, 136),   S( -4, 118),   S( -9,  86),   S( 11,  21),   S( 23, -82),   S(-44, -131),  S(-12, -99),
            S(127,  85),   S( 80, 123),   S( 89, 105),   S( 56,  94),   S( 32,   3),   S(  4, -103),  S(-27, -91),   S( -7, -182),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 28,   5),   S( 13,   8),   S( 51,  -4),   S( -9, -37),   S(-29, -61),   S(-14, -28),   S( 16, -57),   S( 39, -39),
            S( 21, -60),   S( 18, -15),   S(-35, -57),   S(-55, -37),   S(-25, -59),   S( 36, -64),   S( 10, -65),   S( -3, -51),
            S( 34, -96),   S( 11, -56),   S( -1, -66),   S(-39, -54),   S(-27, -30),   S( 12, -43),   S(-42, -20),   S( -1, -28),
            S(  3, -25),   S(-26, -37),   S( 18, -21),   S(-11,  -3),   S(-22,   9),   S(  0,  20),   S( -7,  24),   S(-11,  24),
            S( 24,   6),   S(  1, -32),   S(  8,  44),   S( 33,  91),   S( 53, 119),   S( 29, 119),   S( 12,  96),   S(-31, 105),
            S( 19,  34),   S(  7,  54),   S( 24,  69),   S( 31,  97),   S( 45,  94),   S( 50, 148),   S( 39, 100),   S(-23,  93),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 30,   7),   S( 31,  21),   S( 25,  16),   S(  3,  27),   S( 19,   1),   S( 19, -20),   S( 31, -46),   S(-16, -18),
            S( 57, -57),   S( 19, -50),   S( 15, -56),   S(-11, -42),   S(-23, -27),   S(-42, -30),   S(-46, -33),   S( 18, -43),
            S( -9, -43),   S(-27, -43),   S(-19, -74),   S(-58, -42),   S(  0, -36),   S(-11, -48),   S(-55, -34),   S( 15, -31),
            S(-42,  -1),   S(-45, -51),   S( -3, -69),   S(-38, -29),   S(  0, -42),   S( -2, -26),   S( 12,  -8),   S(  3,   8),
            S(  3,  12),   S( -6, -21),   S(-16,   3),   S( 19,  28),   S( 16,  59),   S( 18,  52),   S(  1,  67),   S(  1,  64),
            S(-10,  67),   S( 26,  60),   S( -2,  58),   S( 22,  60),   S( 25, 108),   S( 15,  83),   S( 16,  79),   S( 15,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-24, -51),   S( -1, -48),   S(  0, -20),   S( -2, -13),   S( 36,  16),   S( 76,   7),   S( 21,   3),   S( 12, -19),
            S( -7, -60),   S(-62, -41),   S( -9, -52),   S( 22, -39),   S(  3, -28),   S( -6, -22),   S( 21, -38),   S( 17, -43),
            S(-19, -46),   S(-87, -23),   S(-59, -42),   S( -8, -32),   S(-16, -47),   S(-12, -63),   S(-24, -62),   S( 64, -68),
            S(-35,  -2),   S(-20,  -8),   S(-22, -36),   S(-52, -40),   S(  5, -68),   S(-45, -53),   S(-24, -53),   S( 20, -51),
            S( 10,  16),   S( 30,  15),   S( 16,  11),   S(-19,  -4),   S( 10,  19),   S( 13,  12),   S(-30,   8),   S( 42,  -4),
            S(  6,  25),   S(  1,  48),   S( 24,  53),   S(  6,  59),   S( 23,  81),   S(  1,  43),   S(-14,  22),   S( 25,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-31, -47),   S(  1, -47),   S(-30, -44),   S(  6,  -2),   S(  2, -22),   S( 76,   4),   S( 55, -10),   S( 60, -10),
            S(-35, -61),   S(-49, -61),   S(-30, -74),   S(  5, -64),   S(-19, -33),   S( 21, -50),   S( 33, -45),   S( 48, -71),
            S(-21, -40),   S(-89,  -5),   S(-27, -26),   S( -7, -28),   S(-57, -46),   S( 42, -68),   S( 28, -121),  S( 87, -102),
            S(-52,  22),   S(-73,  31),   S(  2,  24),   S( 20, -12),   S(-27, -15),   S(-20, -48),   S(-34, -54),   S( 41, -95),
            S(-17,  20),   S(-20,  67),   S(-11,  92),   S( 18,  57),   S( 26,  59),   S(-10,   4),   S( -1,   6),   S(  8, -25),
            S( 13,  69),   S( 25,  56),   S( 29,  79),   S( 24,  79),   S( 11,  62),   S( 33,  81),   S( 12,  32),   S( 27,  38),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-10, -103),  S( 28, -50),   S( -2, -29),   S(  1,  -2),   S( -6, -30),   S(-35, -72),   S( 15, -47),   S(  5, -45),
            S( 39, -86),   S( 29, -47),   S(-22, -75),   S(-31, -59),   S(-31, -87),   S(-11, -64),   S(-13, -90),   S(-21, -68),
            S( -7, -61),   S( -9, -80),   S(-24, -97),   S(-25, -85),   S(-10, -56),   S( -7, -48),   S(-39, -60),   S( -9, -77),
            S(-12, -37),   S( -3, -17),   S(-19, -21),   S( -3,  -1),   S( 17,  55),   S(  3,  39),   S(  3,   9),   S( -8,  -4),
            S( 11,  22),   S(  1,  15),   S(  2,  22),   S( 19,  62),   S( 30,  76),   S( 24,  86),   S( 13,  80),   S( 18,  53),
            S( 12,  30),   S(  1,  36),   S( 12,  52),   S( 12,  60),   S( 25, 102),   S( 23,  92),   S(-21, -23),   S(-14,   4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -61),   S( 27, -82),   S( 20,   6),   S( -2, -12),   S(  5, -22),   S(-27, -39),   S( -9, -75),   S(-14, -70),
            S( 29, -132),  S( 20, -102),  S(  0, -85),   S( 12, -10),   S(-24, -52),   S(  2, -82),   S(  2, -93),   S(  2, -89),
            S( 31, -88),   S( -9, -77),   S( -3, -91),   S(  7, -60),   S(-43, -29),   S( 20, -75),   S( -6, -75),   S( 58, -89),
            S( 16, -27),   S( -1, -35),   S(  2, -30),   S( -3,  25),   S( 13,   4),   S(-17,   7),   S(-14, -17),   S(  8, -23),
            S( -3,  42),   S(  8,  26),   S( -2,   6),   S( 22,  56),   S( 38,  80),   S( 28,  88),   S( 12,  95),   S( -7,  56),
            S( 11, 103),   S( 29,  51),   S(  3,  34),   S( 13,  45),   S( 19,  65),   S(  9,  50),   S( -4,  37),   S(  1,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -118),  S(  4, -71),   S( -4, -42),   S(  3,   3),   S( -6, -14),   S(  0,   0),   S( 23, -72),   S( -9, -40),
            S( 18, -114),  S(-37, -107),  S( -6, -82),   S(-29, -89),   S( -8, -58),   S( 18, -54),   S(  2, -67),   S( 26, -87),
            S( 17, -95),   S(-21, -79),   S(-15, -65),   S(  4, -75),   S(-23, -51),   S(  4, -91),   S(  2, -101),  S( 36, -60),
            S(  5, -33),   S(-22, -42),   S( -6,  -7),   S(-21, -11),   S( 13, -53),   S( -5, -29),   S( 12, -30),   S( 13,  -7),
            S(-14, -15),   S(  5,  39),   S( 11,  50),   S( -8,  15),   S( 19,  69),   S(  3,  16),   S( 17,  45),   S( 23,  66),
            S( -4,  32),   S(  7,  49),   S( 26,  72),   S( 21,  70),   S( 16,  59),   S(  1,  34),   S( 23,  86),   S( 23,  91),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -8, -27),   S(  2, -59),   S(-26, -56),   S(-10, -26),   S(-12, -29),   S(-15, -43),   S( -8, -56),   S(  4, -85),
            S(-24, -66),   S(-21, -99),   S(-17, -106),  S(-10, -37),   S(-19, -25),   S( -7, -32),   S( 11, -56),   S( 11, -107),
            S(-27, -46),   S(-34, -63),   S(-44, -55),   S(  6, -41),   S(-32, -40),   S( -8, -74),   S(  3, -47),   S(  7, -44),
            S(  9, -36),   S(-27, -17),   S( -3,  37),   S(-20,  12),   S( 10,   5),   S(-10, -22),   S( -7, -12),   S( -7,  34),
            S(  5,  47),   S(  1,  50),   S(  0,  69),   S( 11,  59),   S( 24,  79),   S( 11,  63),   S( 17,  57),   S( 10,  23),
            S(-22,   8),   S( -7,   4),   S( 10,  71),   S( 20,  54),   S( 21,  70),   S( 18,  58),   S( 10,  36),   S( 16,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-76, -31),   S(-27, -27),   S(-19,  -5),   S(-10,  24),   S(-17, -25),   S(-27,  -1),   S( -7, -28),   S(-78, -42),
            S( 11, -39),   S( -2,  -2),   S(-24, -33),   S(-10, -10),   S(-12,  -7),   S(-10, -23),   S(-35, -48),   S(-29, -39),
            S(-20, -26),   S( 17, -34),   S( -3,   8),   S( 29,  24),   S(-14,  10),   S(  4,  -4),   S(-34,  20),   S(-26, -36),
            S(  9,  23),   S( 39,  48),   S( 27,  31),   S( 38,  20),   S( 26,  19),   S( 10,  26),   S( 34, -17),   S(-10, -19),
            S( 60,  38),   S( 27,  55),   S( 58,  61),   S( 61,  43),   S( 66,  31),   S( 13,  23),   S( 15,  -7),   S(  2,   0),
            S(100, -35),   S( -5,  53),   S(144,   4),   S( 74,  42),   S( 52,  43),   S(-41,  62),   S( 33, -12),   S(-24,   2),
            S( 50,  -6),   S( -5, -23),   S( 46,  21),   S( 85,  66),   S( 39,  24),   S(  3,  29),   S(-16,   4),   S(-50,   0),
            S(-112, -123), S( -2,  -1),   S(  7,   3),   S( 18,  22),   S(  1,  28),   S( 17,  11),   S(-32,  -4),   S( -9,  10),

            /* knights: bucket 1 */
            S( 15,  -5),   S(-55,  18),   S(-24,   9),   S(-36,  32),   S(-18,  34),   S(-21, -21),   S(-29,  -6),   S(  3, -21),
            S(-40,  30),   S(-49,  53),   S(-30,  26),   S(-17,  21),   S(-23,  19),   S( -9,  22),   S(-14,  -5),   S(-17, -53),
            S(-37,  24),   S( -4,  -1),   S(-22,  17),   S(-10,  50),   S(-15,  35),   S( -9,   8),   S(-44,  28),   S(-14,  20),
            S(-15,  67),   S( 31,  32),   S( -5,  52),   S( -8,  63),   S( -7,  57),   S(-10,  55),   S( -3,  23),   S(-25,  51),
            S( 64,  -3),   S(  8,  25),   S( 44,  62),   S( 19,  54),   S( 40,  50),   S( -4,  69),   S(-10,  48),   S( -6,  59),
            S( 29,  21),   S( 63,  -9),   S( 81,  25),   S( 95,  34),   S( 71,  28),   S(-34,  77),   S( 17,  27),   S( -2,  37),
            S( 17,  -2),   S( 36,  -9),   S( 30, -14),   S( 25,  47),   S( 13,  35),   S(  3,  21),   S( 13,  68),   S(-33,  40),
            S(-154, -38),  S( 15, -18),   S(-34, -61),   S(-18,   9),   S( -4,  10),   S( 38,  44),   S( 16,  41),   S(-69,  21),

            /* knights: bucket 2 */
            S(-62,   8),   S(-34,  28),   S(-25,   3),   S(-13,  20),   S(-15,  13),   S(-53,   2),   S(-26,   3),   S(-17, -29),
            S(-17,   2),   S( -5,  32),   S(-24,   9),   S(-19,  16),   S(-28,  22),   S(-19,   6),   S(  6,   5),   S(-32,   0),
            S(-34,  46),   S(-24,  19),   S(-21,  16),   S(-20,  54),   S(-22,  44),   S(-20,   8),   S(-26,  12),   S( -2,  -8),
            S( -6,  50),   S( -7,  41),   S(-25,  74),   S(-16,  76),   S(-33,  73),   S(  3,  50),   S( 11,  32),   S( -3,  36),
            S(-11,  61),   S(-19,  68),   S(  5,  68),   S( 15,  64),   S(  0,  71),   S( 20,  72),   S( -7,  64),   S( 21,  15),
            S(-46,  66),   S(-22,  50),   S(-13,  83),   S( 41,  33),   S( 45,  33),   S(128,   0),   S( 71,  12),   S( 31, -11),
            S( 28,  34),   S(-41,  55),   S( 47,  20),   S( 32,   7),   S( -3,  42),   S( 14,  -8),   S( 32,  21),   S( 24,  -8),
            S(-57,  27),   S( 27,  59),   S(-15,  63),   S(-11, -28),   S(-21, -14),   S(-30, -45),   S( 16,  -2),   S(-123, -57),

            /* knights: bucket 3 */
            S(-52,  16),   S( -9, -53),   S(  2, -21),   S(  4, -11),   S(  6, -15),   S( -6, -26),   S(-16, -27),   S(-24, -78),
            S(-15, -33),   S(  2,  -8),   S(  8, -13),   S( -3,  -3),   S( -5,  -3),   S( 18, -19),   S( 23, -40),   S( 22, -60),
            S(-13,  -6),   S(-12,   3),   S(  3,  15),   S(  6,  40),   S( 11,  27),   S( -1,  15),   S(  9,  -2),   S( 19, -36),
            S( 10,   0),   S( 15,  26),   S( 17,  44),   S( 10,  53),   S( 14,  68),   S( 28,  57),   S( 38,  48),   S( 16,  36),
            S( -4,  41),   S( 21,  32),   S( 24,  52),   S( 27,  78),   S( 28,  76),   S( 35,  84),   S( 10,  95),   S( 64,  75),
            S(-12,  26),   S(  5,  40),   S(  9,  57),   S( 20,  71),   S( 60,  72),   S(140,  70),   S( 60,  84),   S( 23,  98),
            S(-25,  36),   S(-20,  47),   S(-20,  60),   S( 28,  57),   S( 44,  61),   S(103,  40),   S(  9,  -6),   S( 84,  18),
            S(-146,  34),  S(-32,  73),   S(-50,  81),   S( 33,  43),   S( 59,  71),   S(-49,  66),   S(-26, -43),   S(-58, -107),

            /* knights: bucket 4 */
            S(  8,  10),   S(-11, -11),   S(-53,  14),   S(-34, -13),   S(-33,  21),   S(-18, -13),   S( 19, -27),   S(-19, -17),
            S( 21,  36),   S( 10, -23),   S( -3,   6),   S(-11,   5),   S( -4, -13),   S( 15, -44),   S(-11,  10),   S(-46,  -5),
            S( -3, -19),   S( 28,  -3),   S( 52,   5),   S( 65,   5),   S( 12,  19),   S( 33, -31),   S(-11, -26),   S( -9, -34),
            S(-19, -25),   S( 29,   2),   S( 46, -12),   S( 71,   2),   S( 33,  10),   S( -6,  28),   S(-35,  25),   S( -9,   8),
            S(  5, -39),   S( 34,  -7),   S( 67,  17),   S( 35,  41),   S( 51,   6),   S( 13,  19),   S( 25,  -9),   S(-31,  41),
            S( -5, -25),   S(  1,   0),   S( 41, -20),   S( 60,  22),   S(  5,  20),   S(-23,  37),   S(-21,   2),   S( 19,   2),
            S(-17, -30),   S(-20,  -7),   S(  4,  -4),   S( 23,  20),   S( 25,   8),   S( -3,   9),   S( 10,  35),   S(-35, -12),
            S(  4,  14),   S(-12, -37),   S( -8, -33),   S( 15,   0),   S( 12,  17),   S( -5,  13),   S( -5,  17),   S(-17, -15),

            /* knights: bucket 5 */
            S( 18,  22),   S( 13,  25),   S(-33,  35),   S(-11,  24),   S(-11,  30),   S( 10,  16),   S(-16,  17),   S(  9,  24),
            S( 18,  24),   S( 32,  25),   S(  3,   8),   S(-18,  15),   S( 34,  -9),   S(-26,  15),   S(-12,  41),   S(-46,  16),
            S(-28,  23),   S(-10,   5),   S( 29,  12),   S( 35,  16),   S( 20,  19),   S(-18,  26),   S( -7,  13),   S(-50,  16),
            S( 32,  15),   S( 33, -16),   S( 53,   3),   S( 90, -14),   S( 85,   7),   S( 74,  10),   S( -5,  21),   S( 12,  31),
            S( 42,   3),   S( 41,  -6),   S( 92,  -8),   S(130,  -5),   S( 92, -12),   S( 43,  19),   S(  4,   8),   S( 14,  23),
            S(  1, -23),   S( 40, -26),   S(  7, -19),   S( 14,  20),   S( 28,   4),   S( 49,   2),   S( -6,  12),   S( 24,  30),
            S(  1,   4),   S(-26, -55),   S(  2, -46),   S( -9, -16),   S( -6, -36),   S(  6,   4),   S( -3,  38),   S( 17,  30),
            S(-23, -39),   S(-26, -66),   S(  9, -12),   S(-23, -30),   S(  7,  -4),   S(  0,  28),   S( 19,  32),   S( -3,  16),

            /* knights: bucket 6 */
            S( -7, -11),   S(-47,  24),   S(-20,   6),   S(-38,  37),   S(-40,  33),   S(-15,  35),   S(-14,  42),   S(-35,   6),
            S(  4, -16),   S(-14,  46),   S(-16,   3),   S( 19,  10),   S( 17,  18),   S(-38,  37),   S(-20,  49),   S(-41,  65),
            S(-11,  15),   S( 10,  15),   S( -1,  28),   S( 25,  36),   S( 30,  31),   S(-33,  40),   S( 19,  29),   S(-12,  40),
            S(  7,  47),   S( 51,   6),   S( 38,  28),   S( 69,  14),   S( 89,  -5),   S( 71,  14),   S( 30,  18),   S(-11,  48),
            S( -8,  39),   S( 36,  13),   S( 88,  10),   S(117,   4),   S(107, -12),   S( 69,  28),   S(127, -16),   S( 22,  27),
            S(  9,  16),   S( 24,   7),   S( 58,  19),   S( 45,   8),   S( 54,   2),   S( 41,   3),   S( 16,  -9),   S( 28,   1),
            S( -1,  27),   S( 13,  31),   S( 34,  36),   S( -1,  -5),   S( 29, -13),   S( 23, -35),   S( -7,  -4),   S( 10,  38),
            S( 13,  29),   S(  0,  25),   S( 16,  31),   S(  4,  16),   S(  8,  -9),   S( -7,  -6),   S(  9,  22),   S(-23, -38),

            /* knights: bucket 7 */
            S(-35, -44),   S(-23, -43),   S( -1, -17),   S(-39,  19),   S( -6,  -3),   S(-36,   4),   S(-15,  -8),   S(-17,  19),
            S(-35, -54),   S(-10, -27),   S(-39,  -6),   S(-35,  -1),   S( -3,  10),   S(  4,  21),   S( -5,  12),   S(-61,  35),
            S( -6, -40),   S(-41, -22),   S(  3, -15),   S( -3,  22),   S( 45,  16),   S( 34,  11),   S( 24,  16),   S( -5,  30),
            S(-41,  14),   S(  4,  -5),   S( 50, -17),   S( 81,   3),   S(106,  -7),   S( 83,  17),   S( 72,   4),   S( 65,   5),
            S( -1,   4),   S( -7,  10),   S( 16,  18),   S( 76,   0),   S(103,   4),   S(153, -21),   S(189, -12),   S( 37,  -7),
            S(-21,  11),   S( 21,   7),   S( -7,   9),   S( 49,  19),   S( 98,   0),   S( 97,  -5),   S( 19,  -8),   S(  6, -41),
            S(-24,  -1),   S(-10,   0),   S( -7,  12),   S( 25,  19),   S( 54,  14),   S( 28,  21),   S(-14, -35),   S(-16, -39),
            S(-32, -41),   S(-10,   5),   S( -5,  18),   S(  2,  14),   S( 13,   7),   S( 17,   9),   S(  4,  -8),   S(  1, -11),

            /* knights: bucket 8 */
            S( -2,   2),   S( 10,  25),   S( 11,  23),   S( -9, -31),   S( -2,  22),   S( -4, -20),   S( 13,  24),   S( -3, -14),
            S( -7, -24),   S( -5, -23),   S( -8, -37),   S( -9,   6),   S( -6,  34),   S( -1,  -6),   S( -1,  -7),   S( -2,  -3),
            S(-11, -40),   S( -8, -25),   S(  3, -44),   S(  4,  11),   S( -9, -18),   S( 12,   9),   S( -3,  -5),   S( -1, -15),
            S(-17, -54),   S( -9, -30),   S(  9,  21),   S(  1,  12),   S(-17, -13),   S(-25, -12),   S(-21, -30),   S(-16, -36),
            S( -6, -24),   S(  5, -17),   S(  0, -17),   S(  1,  -4),   S(-17,   3),   S(-12, -13),   S(  4,   0),   S( -2, -13),
            S( -3,   9),   S( 13,  -1),   S( -2,   7),   S( -5,  -9),   S( -7,  -1),   S( -5, -10),   S(-10,  -6),   S( -7, -20),
            S(  0,  17),   S( -1, -27),   S(-12, -20),   S(  5,  11),   S(  2,   1),   S( -1,  -2),   S( -4,   3),   S( -3, -18),
            S(  0,   1),   S( -4,   6),   S( -6,   1),   S(  2,  -4),   S( -2,   5),   S( -2,  -7),   S( -1,   3),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-20, -64),   S( -6,  -4),   S( -4, -37),   S( -4, -37),   S(-16, -10),   S(-12,   9),   S(  5,  18),   S(  2, -11),
            S( -5,   3),   S(-15, -45),   S(-19, -105),  S(-24, -63),   S( -9, -32),   S(-21, -57),   S(-11,  -3),   S(-12,   0),
            S( -9, -23),   S(-16, -42),   S(-10, -33),   S( -3, -53),   S(-20,  -8),   S( 12,  10),   S(-13,  -6),   S( -4,   2),
            S(-17, -48),   S(-10, -42),   S( -6, -20),   S( -7, -32),   S(-13, -27),   S(  4,   3),   S(-17, -40),   S(  2,   9),
            S(  3,  25),   S( -8, -25),   S(  0, -15),   S(  1, -26),   S( -9, -21),   S( -3,  12),   S(-10, -11),   S( -6,  -1),
            S(-13, -19),   S(-18, -33),   S(-10, -16),   S( -2, -12),   S(  2,  20),   S( -7,   4),   S( -3,  20),   S( -1,   5),
            S(-10, -15),   S( -1,  17),   S(-12,  -4),   S(-23, -18),   S(  1,   3),   S(  1,  21),   S( -9,  14),   S( -7,   0),
            S(  4,   0),   S(  4,   1),   S( -2,   8),   S( -1,   4),   S(-11,  -8),   S( -5,  -1),   S(  2,   8),   S( -1,  12),

            /* knights: bucket 10 */
            S( -9, -33),   S( -6,   9),   S(-10, -10),   S(-10,  17),   S(-20, -50),   S(  8, -21),   S( -3,   9),   S( -3,  11),
            S( -4, -18),   S(  8,   0),   S(-12, -24),   S( -8, -42),   S( -5, -28),   S(-24, -54),   S( -7,  14),   S(  2,  26),
            S( -3,  -5),   S( -5,  -9),   S( -6, -13),   S(  8, -44),   S(-24, -37),   S( -2, -15),   S( -9, -31),   S(-10,   8),
            S(-10, -18),   S(-11, -23),   S( -5,  -9),   S( -1, -18),   S( -8, -13),   S( -3,   0),   S( -8, -47),   S( -3,  -4),
            S(-13, -20),   S(-12, -29),   S( -7,   0),   S( -5, -10),   S(  6,   0),   S( -5, -31),   S( -3,  -8),   S(  4,  10),
            S( -2,   9),   S(-12,   0),   S( -9,  12),   S(-12,  23),   S(-13, -13),   S(-17, -12),   S(-13,  -3),   S(-18,  -7),
            S(  2,   7),   S( -4,  -6),   S( -6, -29),   S( 12, -20),   S( -6,   3),   S(-16, -45),   S( -9,   6),   S(-10, -14),
            S( -1,   1),   S( -2,   7),   S( -2,  14),   S( -5,   1),   S( -5,   2),   S( -7, -13),   S(  5,   9),   S(  1,   6),

            /* knights: bucket 11 */
            S( -4, -16),   S(-26, -27),   S( -4,  -6),   S(  5,  17),   S(-38, -35),   S( -1,  11),   S( -6,   5),   S(  8,  30),
            S( -7, -17),   S(-26, -40),   S(-11, -43),   S( 17,  -3),   S( 10,  20),   S( -2, -25),   S(-13, -23),   S( -9, -12),
            S(-14, -42),   S(-19, -22),   S( -4, -11),   S(  1,  -2),   S( -6,  22),   S( 18,   0),   S(  0, -12),   S( -3,  -3),
            S(-15, -12),   S(  5, -21),   S( -3, -22),   S( 12,   4),   S( 29,   1),   S(  1, -13),   S( 13,  22),   S(  0,  -5),
            S(-16,   0),   S(  2, -38),   S(-19,   1),   S(  2, -13),   S( 33,  12),   S(  6,  20),   S( -8, -66),   S(-10, -13),
            S( -9, -25),   S( -7, -45),   S(  4,   9),   S(  9,   1),   S(  9,  36),   S( -7,  -9),   S( -4, -26),   S( -2,  19),
            S( -1,  -7),   S( -9,  16),   S(-11, -12),   S(  5,  -5),   S( 12,  -5),   S(  3, -18),   S(  0, -17),   S( -4,   0),
            S( -3, -18),   S(  1,   6),   S( -4, -11),   S(  1,  15),   S( -5, -12),   S( -1, -10),   S(  4,  14),   S( -1,  -5),

            /* knights: bucket 12 */
            S(-14, -41),   S( -3, -10),   S( -1, -19),   S(  0,   8),   S( -4,   6),   S( -5, -12),   S( -1,   6),   S( -1,   1),
            S( -3,  -9),   S(  0,   2),   S(  0, -13),   S( -3,   6),   S( -4,  -8),   S(  0,   3),   S(  1,   0),   S( -1,  -8),
            S( -2, -10),   S( -6, -21),   S( -6, -16),   S(-15, -23),   S( -8,  -3),   S( -2,  26),   S( -4,   0),   S( -4,  -9),
            S(  2,   9),   S( -1, -35),   S( -7,  28),   S(  4,  17),   S( -4, -11),   S(  3,  22),   S(  5,  12),   S(  2,   7),
            S(  0,   3),   S( -3,  -5),   S( -4, -20),   S( -4,  -9),   S(  0,   6),   S( -3,   5),   S( -6,  -4),   S( -9,  -9),
            S( -5,  -4),   S( -1,  -3),   S( -3, -13),   S( -2,  -9),   S( -3,   0),   S( -7, -19),   S(  7,   7),   S( -1,   8),
            S( -4,  -8),   S( -2,  -2),   S( -9,  -1),   S( -2,  -6),   S(  0,   8),   S( -9,  -8),   S( -5, -19),   S( -3,  -3),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -6),   S(  1,   2),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -6),   S( -4, -12),   S( -3, -17),   S( -2,  -7),   S( -3, -10),   S( -2,   7),   S( -6,  -5),   S(  3,  10),
            S( -2,   8),   S( -2,  -2),   S(  3,  10),   S( -4,  -2),   S( -6, -10),   S(  0,  10),   S(  2,  20),   S( -4,  -6),
            S(  4,  -2),   S(  5,   9),   S(  5,   3),   S( -4, -23),   S(  5,  24),   S( -5,  10),   S(  7,   4),   S( -3,  -3),
            S(  0,  15),   S(  0,   4),   S( -5,  -1),   S(  2,  29),   S(  1,  13),   S( -1,  30),   S(  0,   7),   S( 10,  19),
            S(  1,  21),   S( -2, -15),   S( -3,  13),   S( -6,   9),   S(-16,  -1),   S( -3,  24),   S( -8, -23),   S( -3,  -3),
            S( -4,  -5),   S(  2,   2),   S( -3,   8),   S(  3,  13),   S( -8,   7),   S( -8,   3),   S(  2,  19),   S(  0,   2),
            S(  1,   4),   S(  3,   8),   S( -6,  -4),   S( -5,   0),   S( -2,   6),   S( -4,  -8),   S(  2,   6),   S( -1,   1),
            S(  2,   6),   S(  0,   2),   S( -2,  -3),   S(  2,   4),   S( -1,   1),   S(  0,   2),   S( -1,  -2),   S(  0,   1),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   3),   S(  5,  18),   S( -2,   0),   S( -6, -24),   S( -2,  17),   S(  2,   2),   S(  0,   3),
            S( -2, -12),   S( -8, -16),   S(  2,  -4),   S(  0,   1),   S(  3,   1),   S(  0,   4),   S( -7,   5),   S(  6,  57),
            S( -1,  -1),   S( -5, -33),   S(  7,  18),   S(-10, -33),   S( -3,   0),   S(  2,  10),   S( -1,   9),   S(  3,  17),
            S( -1,  -4),   S( -3, -18),   S(-21, -12),   S( -1,  43),   S(  3,  42),   S( -4,  -4),   S(  0,   6),   S(  1,  35),
            S(  6,  15),   S(-17, -36),   S( -9,  -8),   S( -8,   4),   S(  0,  33),   S(-10,   6),   S( -3,  -1),   S(  3,  12),
            S( -1,   2),   S(  4,   5),   S(  3,  -4),   S( -3,  13),   S(  2,  18),   S(  1,  13),   S(  1,   7),   S( -5, -12),
            S(  0,   4),   S( -3,  -3),   S(  3,  16),   S(  6,   4),   S(  3,  11),   S( -5, -11),   S(  2,   6),   S(  3,   5),
            S(  0,  -1),   S(  0,   1),   S( -1,  -2),   S(  2,   3),   S( -1,   0),   S( -1,  -2),   S(  0,   0),   S(  1,   2),

            /* knights: bucket 15 */
            S( -3, -14),   S( -1,   4),   S(  4,  23),   S( -2,   5),   S( -4, -16),   S(-10, -36),   S( -4, -15),   S( -2, -12),
            S(  2,  -2),   S(  4,   6),   S( -6,  -8),   S(  9,  43),   S(  1,  16),   S( -8, -34),   S( -3,  -3),   S(  1,   2),
            S(  0,  -5),   S( -5, -20),   S(  1, -10),   S(  5,   8),   S(-17, -27),   S(  0,  -5),   S( -2,  -6),   S( -2,  -2),
            S(  0,  -8),   S( -3,   3),   S( -5, -13),   S( -5,   6),   S( -7,   7),   S( -9,  27),   S(  4,   6),   S( -2,   0),
            S( -1,  -2),   S(  9,  21),   S( -4,   7),   S( -6,   5),   S( 19,  35),   S(  0,  17),   S(  6,  -3),   S(  4,  18),
            S(  1,   3),   S( -4, -10),   S( -1,   2),   S( -9, -17),   S( -6,  -9),   S(  2,  16),   S(  0,   8),   S(  5,  12),
            S( -1,   0),   S( -2,  -7),   S(  4,  16),   S(  3,   3),   S(  3,  13),   S(  5,   7),   S(  1,   7),   S(  3,   8),
            S(  1,   4),   S( -1,  -6),   S(  0,  -1),   S( -1,   0),   S(  2,  10),   S(  1,   1),   S(  0,   0),   S(  1,   2),

            /* bishops: bucket 0 */
            S( 19,  -8),   S(-10,  37),   S(-14,  15),   S(-25,  -9),   S( -4,  -2),   S(  1,  10),   S( 64, -41),   S( 17, -16),
            S(-33, -10),   S(-10, -23),   S(-25,  33),   S( -2,  12),   S(  0,  19),   S( 48,  -8),   S( 29,  23),   S( 40, -15),
            S( 11,   9),   S(  1,  24),   S(  0,  -6),   S(  5,  11),   S( 22,  20),   S( 30,  21),   S( 36,   5),   S( 24,   2),
            S( 15, -30),   S( 35, -39),   S( 13,  16),   S( 34,  18),   S( 67,  37),   S( 31,  48),   S( 16,  20),   S(  5,  27),
            S( 36, -13),   S( 46, -16),   S( 57,   9),   S( 82,  44),   S( 90,  25),   S( 22,  44),   S( 32,  46),   S( -7,  15),
            S( 53,  18),   S( 59,  44),   S(104,   6),   S( 58,  -1),   S( 20,  43),   S(  9,  36),   S( 38,  32),   S( -9,  12),
            S(-45, -79),   S( 78,  34),   S( 87,  82),   S( 19,   0),   S( 13, -10),   S( 23,  30),   S(-30,  19),   S(-16,  51),
            S(-21, -40),   S( -4, -10),   S( 15, -25),   S(-15, -17),   S(-14, -18),   S(-21,   5),   S(-20,  18),   S(-33, -37),

            /* bishops: bucket 1 */
            S(-65,  11),   S( -5,  -3),   S(-20,  40),   S( 17,  -7),   S(-19,  22),   S(  9,   3),   S( 35, -14),   S( 22, -37),
            S( -2, -34),   S(-23, -14),   S( -8,  -5),   S(-19,  17),   S( 25,  -7),   S(  2,   3),   S( 43, -36),   S( 12, -13),
            S(-13,   1),   S( 28, -10),   S(-25,  -8),   S( 16,   6),   S(  1,   1),   S( 24, -28),   S( 15,   0),   S( 60,  -5),
            S( 24, -16),   S( 50, -13),   S( 25,   3),   S( 22,  13),   S( 38,   3),   S( 10,  16),   S( 49,   0),   S(  0,  17),
            S( 24,  -9),   S( 55, -10),   S( 18,   9),   S( 97, -13),   S( 53,  23),   S( 42,  25),   S( -2,  28),   S( 29,   3),
            S( 64, -45),   S( 52,  10),   S( 63, -24),   S( 73, -10),   S( 78,   5),   S(-39,  14),   S(-26,  55),   S(-31,  17),
            S( 13, -67),   S(  1, -50),   S( -3,   4),   S( 27,  47),   S( 28,  34),   S(-15,  29),   S(-24,   2),   S(-31,  32),
            S(-12, -33),   S(-16,   7),   S( -6, -21),   S(-48,  -1),   S(-23,  17),   S( 16,   2),   S( 25,   1),   S(-57,  -5),

            /* bishops: bucket 2 */
            S( -4, -24),   S(-11,  -9),   S(  4,  16),   S(-22,   7),   S( 11,  13),   S(-18,   7),   S( 17, -11),   S( -9, -23),
            S( 18, -24),   S(  1, -32),   S( -8,  -7),   S(  4,  13),   S(-13,  11),   S(  3,   5),   S( -4, -32),   S( 12, -54),
            S( 43,   1),   S( 25,  -1),   S( -7,  -4),   S( -9,   9),   S( -4,  30),   S(-17, -32),   S(  8, -23),   S(-12,  -6),
            S(-19,   8),   S( 45,  18),   S( -5,  21),   S( 26,  32),   S( -1,  17),   S( -3,  22),   S(-15,   3),   S(  8,  12),
            S(  0,  18),   S(-36,  48),   S( 51,  25),   S( 22,  32),   S( 22,  32),   S( 24,  11),   S(  9,  35),   S( 36,  -5),
            S(-35,  37),   S( -5,  40),   S(-35,  -7),   S( 90,   1),   S( 55,  15),   S( 98, -13),   S( 78,  15),   S( 40, -45),
            S(-37,  61),   S(-43,  -1),   S( -7,  21),   S(  4,  15),   S(-43,  -7),   S(-32,  17),   S(-38,   0),   S( -2, -43),
            S(-85, -23),   S(-16,  24),   S( -1,   8),   S(-18,  27),   S(-29, -12),   S(-30,  10),   S(  0, -13),   S(-59, -23),

            /* bishops: bucket 3 */
            S( 32, -21),   S( 38, -21),   S( 24, -24),   S( 12,  -3),   S( 18,  11),   S(  0,  29),   S(-12,  48),   S( -4, -27),
            S( 35,  -1),   S( 22, -30),   S( 17,  -2),   S( 20,   4),   S( 17,  17),   S( 21,   7),   S(  7, -20),   S( 30, -43),
            S( 14,  -5),   S( 35,  34),   S( 18,   7),   S( 16,  28),   S( 16,  30),   S(  7,  -2),   S( 22,  -9),   S( 13,   7),
            S( -9,  15),   S( 11,  43),   S( 23,  52),   S( 36,  49),   S( 33,  24),   S( 28,   8),   S( 26,  -1),   S( 40, -42),
            S(  6,  31),   S( 17,  52),   S(  6,  57),   S( 55,  49),   S( 49,  47),   S( 50,  24),   S( 28,  19),   S(  3,  11),
            S(  2,  33),   S( 22,  54),   S(  3,  14),   S( 18,  41),   S( 52,  42),   S( 81,  41),   S( 49,  43),   S( 44,  73),
            S(-26,  74),   S( -7,  25),   S( 10,  27),   S( -4,  53),   S( 24,  34),   S( 54,  51),   S(-26,  26),   S( 20, -21),
            S(-43,  10),   S(-29,  50),   S(-53,  39),   S(-36,  48),   S( 14,  11),   S(-60,  32),   S( 20,   6),   S( 14,   8),

            /* bishops: bucket 4 */
            S(-38,   3),   S(-31,   4),   S(-40,  17),   S(-56,  15),   S(-30,  -8),   S(-21,  -4),   S(-13, -20),   S(-40, -38),
            S(-11,   1),   S(-13, -17),   S( 61, -30),   S(-37,  19),   S(-56,  27),   S(-11, -26),   S(-30, -30),   S(-30, -20),
            S(  7,  23),   S(-11, -14),   S(  1,  -3),   S( -4,   7),   S( 14,  -5),   S(-65,   3),   S(-18, -29),   S(-55, -16),
            S( 30,   0),   S( 54, -12),   S( 35,  14),   S( 16,  30),   S( -6,  27),   S( 28,   2),   S(-44,   9),   S( -9, -21),
            S( 17, -11),   S( -5, -15),   S( 43,  -7),   S( 22,   7),   S( -2,  31),   S( 19,  12),   S(-18,  37),   S(-56,   4),
            S(-51, -80),   S(-42,   1),   S( -6,   4),   S(  9,   8),   S(-44,  46),   S(  8,   5),   S(-13,  29),   S( -7,  27),
            S( -1,   0),   S(-25,  -3),   S(  2, -17),   S(-27, -10),   S(  2, -20),   S( 35,   3),   S( -8, -13),   S( 17,  33),
            S( -7,  -8),   S( -1, -21),   S(-13,  -9),   S(  2, -16),   S(-18,   4),   S(  4,  19),   S(  3,  41),   S(  5,   0),

            /* bishops: bucket 5 */
            S(-47,  -9),   S( 17,  -7),   S(-42,  20),   S(-53,  23),   S(-15,   6),   S(-60,  20),   S(-35,  21),   S(-50, -19),
            S(-13,  -6),   S(-30,  -6),   S( 17,  -1),   S(-22,  20),   S(-62,  34),   S(-29,  26),   S(-38,  -3),   S(  4, -11),
            S(  6,  29),   S(-16,   5),   S( 15, -20),   S( -2,  13),   S(-12,  26),   S(-67,   5),   S(-16,  22),   S(-21,  27),
            S( 19,  11),   S( 10,  17),   S( 71, -12),   S( 44,  17),   S( -5,  30),   S(  9,  26),   S(-59,  39),   S(-23,  23),
            S( 13,  -4),   S( 36,   3),   S( -4,  17),   S( -2,   5),   S(  1,  16),   S( -4,  19),   S(  8,  25),   S(-49,  19),
            S(  3, -10),   S(-33,  23),   S( 20, -19),   S( -9, -11),   S( -8,  13),   S(-15,  -8),   S(-22,  22),   S(-38,  48),
            S(-23, -10),   S( -4, -15),   S(-15,   0),   S(  6,  26),   S( 19,   7),   S( -8,  30),   S( -5,   8),   S(-22,  32),
            S(-15,  -6),   S(-10, -18),   S(  0, -16),   S(-18,  -1),   S(-23,  31),   S(  9,  38),   S(-19,  27),   S( 10,   4),

            /* bishops: bucket 6 */
            S(-14, -35),   S(-14,   8),   S(-35,  22),   S(-22,  15),   S(-59,  35),   S(-34,  21),   S(-42,  30),   S(-62,  -7),
            S(-39,  15),   S(-32, -25),   S(-61,  44),   S(-47,  34),   S(-52,  32),   S(-52,  23),   S(-46,   9),   S(-36,  11),
            S( -1,   6),   S(-35,  22),   S( -8, -11),   S(-38,  38),   S(-27,  40),   S(-31,  -9),   S( -4,  -5),   S(-17,  25),
            S(-63,  31),   S(-48,  34),   S(-12,  25),   S( 24,  41),   S( 14,  39),   S( 20,  20),   S( 25,   9),   S(-12,  25),
            S(-47,  26),   S(-27,  34),   S(  9,  16),   S( 61,  19),   S(-10,  23),   S( -7,  14),   S( 14,  20),   S(-18,  -1),
            S(-53,  43),   S(-17,  25),   S(-48,   2),   S(-13,  20),   S( 19,  17),   S( -2,  -2),   S( -1,  24),   S(-28,   1),
            S(-19,  36),   S(-79,  32),   S(-27,  20),   S(-20,  29),   S( -5,   9),   S(  8,   9),   S( 12, -10),   S(-26,  14),
            S(-18,   2),   S(-26,  38),   S(-10,  33),   S( 27,  14),   S(-23,  24),   S( 21, -13),   S( -9,  12),   S(-12,  12),

            /* bishops: bucket 7 */
            S(-18, -54),   S(-55,  -9),   S(-37, -20),   S(-16, -11),   S(-41,  -3),   S(-40,  -4),   S(-66, -19),   S(-53, -17),
            S(-10, -49),   S(-10, -48),   S( 13, -21),   S(-28, -10),   S(-36,   4),   S(-45,   5),   S(-42, -30),   S(-12, -16),
            S(-48, -23),   S(-28,   3),   S(-17, -23),   S(  8,  -3),   S(  0,   1),   S( -9, -35),   S(-55,   8),   S(-62,  10),
            S(-22, -24),   S(-61,  28),   S(-26,  13),   S(-11,  25),   S( 91,   2),   S( -2,  18),   S( 41, -27),   S(-14,  -2),
            S(-26,   0),   S( 20, -11),   S(-45,  31),   S( 14,   4),   S( 58,  -5),   S( 51,  12),   S(-16,  18),   S(-32,  -8),
            S(-72,  32),   S(-36,  51),   S(-19,  -7),   S(-80,  35),   S(-27,  21),   S( 17,  -8),   S(  8,  40),   S(-50, -77),
            S(-11,  -6),   S(-35,  -1),   S(-46,  21),   S( -4,   8),   S(  2,   2),   S( 24, -22),   S(  9, -26),   S(  2, -12),
            S(-24, -33),   S( -6,   6),   S(-11,  13),   S( -3,   9),   S( -8,   2),   S( 11, -14),   S( 31, -27),   S( -1,  -7),

            /* bishops: bucket 8 */
            S( 33,  56),   S( -4, -36),   S( -1,  -2),   S( -8,  41),   S(  1,  19),   S( -6, -37),   S(-16, -26),   S(-11, -18),
            S(  1,  -2),   S( 14,  25),   S( 22,   8),   S(  8,  22),   S(  2, -14),   S(  2,   0),   S(-34, -49),   S(-10,   0),
            S( -6,  -6),   S(-13, -13),   S( 24,  27),   S( 12,  16),   S(  8,  16),   S( -5,  -2),   S(-25, -14),   S(-34, -28),
            S( -4, -11),   S( 32,  25),   S(  1,  29),   S( 26,  11),   S(  6,  34),   S( 11,  29),   S(-11,   8),   S(  2, -18),
            S( 15,  17),   S( 51,  58),   S( 20,  -1),   S( -5,  24),   S( 11,  26),   S(-24,  25),   S( -7, -25),   S(  4,  18),
            S( -8,  -7),   S(  3,   8),   S(  8,  22),   S( 25,  16),   S( 13,  34),   S( 26,   2),   S( -8,  58),   S( -3,  31),
            S(  2,  14),   S(-18, -45),   S( 27,  -1),   S( 25,   3),   S( 10,   1),   S( 22,  47),   S( 16,  23),   S(-13,  -4),
            S( -7,  -4),   S(  4,   2),   S(  1,  16),   S(  2,  10),   S( 29,   5),   S( 22,  11),   S( 14,  38),   S( 35,  25),

            /* bishops: bucket 9 */
            S(  6,  28),   S(  6,  13),   S( -1,  -3),   S(-30, -27),   S(-19,  -9),   S( -7,  -4),   S( -3,  -1),   S( -9,  -7),
            S(  0,  -3),   S(  7, -11),   S(  5,  16),   S(-30,   5),   S(-26,  15),   S( -9,  -8),   S(-37, -15),   S(-16, -29),
            S( -9,   4),   S( 18,   9),   S( -3, -21),   S(  5,  28),   S( 14,  17),   S(-28, -19),   S( -1,   9),   S(-10,  -6),
            S( -1,  24),   S(  2,  -7),   S( 31,   4),   S( 29,   6),   S(  0,  27),   S( -9,  19),   S(  5,  24),   S( -4,  13),
            S( 26,  18),   S( 20,  16),   S( 30,  25),   S( 18, -15),   S( 14,  33),   S(  0,  37),   S(  6,  37),   S(-16, -20),
            S( 18,  23),   S( -4,  31),   S(  9, -15),   S( 14,  19),   S( 42, -37),   S( -7,  14),   S( 17,  35),   S( 11,  27),
            S( 12,  10),   S(-12,   9),   S(  8,  12),   S( 20,  -1),   S( 24,   2),   S( 33,  19),   S( 15,  28),   S( 18,  56),
            S( 10,  36),   S(  0, -24),   S(  2,  21),   S( 11,  17),   S(  8,  41),   S( 18,  -3),   S( 26,  -1),   S( 28,  21),

            /* bishops: bucket 10 */
            S( -2, -33),   S( 11,  11),   S( -2, -18),   S(-23, -20),   S(-66, -16),   S(-32, -58),   S(  8,  -5),   S( -4,  13),
            S(-10,  17),   S( -5, -54),   S( -6, -15),   S(-21, -33),   S(-47,   9),   S(-29, -18),   S(-30, -16),   S(  1,   1),
            S(-11, -34),   S(-20, -16),   S(-18, -29),   S( -3,  31),   S(-12,  13),   S(-10, -31),   S( -5,   5),   S( -6, -18),
            S(-18,  11),   S(-22,   3),   S(-26, -24),   S( 10,   7),   S(-17,  55),   S( 31,  16),   S( 38,  31),   S( -5, -31),
            S( 10,   5),   S(-35,  26),   S(  0,  12),   S(  6,  39),   S( 40,  -5),   S( 26,  41),   S( 24, -13),   S( 17,   9),
            S(  6,   7),   S( 11,  20),   S( -9,  -2),   S( 28,  15),   S( 15, -13),   S(  1,  -5),   S( 12,  11),   S( 26,  14),
            S( 20,  38),   S( -4,   1),   S( 31, -13),   S( 12,  30),   S( -1,  14),   S( -7, -23),   S(  0, -15),   S( 21,  28),
            S( 10,  24),   S( 21,  31),   S( 43,  16),   S(  7,  20),   S( -5,  23),   S(  5,  13),   S( 12,  16),   S(  0, -15),

            /* bishops: bucket 11 */
            S( 11, -17),   S( -8, -14),   S( -9,  -9),   S(  1,  -3),   S(-20, -15),   S( -4,  -5),   S(-22, -28),   S(-11,   0),
            S( -7, -12),   S(  2, -22),   S(-11,  10),   S(  3, -11),   S(-15,  12),   S(-42,  -5),   S(-36, -15),   S(  9,   2),
            S(-11, -49),   S( -3, -18),   S(-14, -37),   S(-31,   7),   S( -5,  -5),   S(  8,  22),   S(  1,  -5),   S( -2, -15),
            S(  3,  -1),   S( -3, -31),   S(  5,  -2),   S(-32, -17),   S( 14,   4),   S( 21,  52),   S( 44,  18),   S( -7, -25),
            S(-11, -16),   S(-14,  -9),   S(-37,  39),   S(-26,  38),   S(-22,  35),   S( 39,   9),   S( 31,  -8),   S(  8,   5),
            S( -6,   7),   S( -9,  -8),   S( -9,  -8),   S(  2,  24),   S( 24,  21),   S(  8, -25),   S(  3, -12),   S( -2, -16),
            S( -3,  -7),   S( 14,  23),   S( 18,  49),   S( 33,  24),   S( 19,  -6),   S( -7,  -4),   S(-19, -29),   S( -7, -14),
            S( 28,  15),   S(  4,   1),   S( 28,  45),   S( 29, -19),   S( 17,  16),   S(  4,   5),   S( -7, -13),   S(  5,  -5),

            /* bishops: bucket 12 */
            S( -5, -11),   S( -4, -12),   S( -6,  -1),   S(  7,  19),   S(-10, -10),   S( -8,  -4),   S( -1,   4),   S( -1,   1),
            S(  0,  -5),   S(  6,   3),   S( -1,  -3),   S(  1,  14),   S(  0,  10),   S(  9,   7),   S(-14, -21),   S( -2,  -5),
            S(  8,   5),   S( 12,  -3),   S( 20,  15),   S( 21,  16),   S( -1,  12),   S( -7,  -9),   S(  2,   6),   S( -5,  -3),
            S( 10,   3),   S( 17,   6),   S( 20,   7),   S( 17,  41),   S( 11,   8),   S(  5,  22),   S(  3,  13),   S(  3,   7),
            S( 11,   9),   S( 10,   9),   S( -2,  18),   S( 21,   9),   S( 19,  26),   S(  9,  31),   S(  7,  11),   S(  3,  11),
            S(  2,   0),   S( -8,  -9),   S( -6,  13),   S(  1,  -4),   S( 31,  31),   S(  9,   9),   S( -9,  -8),   S( -5, -11),
            S( -3,  -4),   S(  4,  10),   S(  3,  10),   S(  5,  -6),   S( 12,   2),   S( 20,  24),   S( 12,  25),   S( -1,  -3),
            S(  0,   5),   S( -1,  -4),   S(  0,  -4),   S(  0,  -5),   S(  2,   8),   S(  3, -11),   S( 14,   6),   S(  7,   4),

            /* bishops: bucket 13 */
            S( -5, -18),   S( -1,  -3),   S( -5, -15),   S( -6, -11),   S( 16,  15),   S( -8, -13),   S(-16, -21),   S( -3,  -4),
            S( -5,  -2),   S( -8, -12),   S(  0,   3),   S( 16,   1),   S( -6, -15),   S(  3,  12),   S( -2,  -9),   S(  0,  -4),
            S(  8, -11),   S( 31,  18),   S( 11,   0),   S( 19,  30),   S(  3,  24),   S(  8,  19),   S( -7,   4),   S( -7,  -5),
            S( 25,  29),   S( 47,  17),   S( 23,  29),   S(-15,  11),   S( 18,  70),   S(  3,  14),   S(  9,   7),   S(  2,   9),
            S( 21,  21),   S( 17,  14),   S( 12,   1),   S(  9,  -7),   S( 11,  -4),   S( 12,  23),   S( 13,  17),   S(  3,  10),
            S(  6,   4),   S(  1,   7),   S( -4, -12),   S( 17,  -4),   S(  6,  14),   S( -6, -19),   S(  3,  -4),   S( 12,   0),
            S(  7,   7),   S(-10, -20),   S( -2, -18),   S(  3,   3),   S(  6,  18),   S( 17,  10),   S(  8,  -4),   S(  9,  12),
            S(  1,  -1),   S( -2,  -2),   S(  0,  12),   S(  2,   9),   S(  7,  14),   S(  3, -13),   S( 12,  -4),   S( 11, -11),

            /* bishops: bucket 14 */
            S(-13, -24),   S(  5,  22),   S( 16,  13),   S(  5,  21),   S(-12,  -2),   S( -8,  -7),   S( -5,   2),   S( -8,  13),
            S( -1,   1),   S( -2,  -6),   S(  2,  12),   S( -2,  -7),   S( 12,   3),   S(  3,   8),   S( -5,  17),   S(  4,  29),
            S(  1,  -4),   S( -2, -13),   S( -8, -15),   S( 20,  33),   S( 24,  46),   S( 12,  20),   S(  5,  37),   S(  3,  28),
            S(  4,  32),   S(  9, -12),   S( -2,  -1),   S(  3,  31),   S( 11,  20),   S( 22,   9),   S( 22,  16),   S(  9, -17),
            S( 10,   6),   S(  6,  15),   S( 12,   8),   S( 20,  12),   S( -3,   5),   S(  5,  14),   S( 23,   1),   S( 15,  10),
            S(  2, -11),   S( 23,  38),   S(  3,   8),   S( 15,   7),   S(  9,   0),   S( -7,   1),   S( -2,  17),   S( 16,   1),
            S( 17,  36),   S(  7,  10),   S( 12,  17),   S(  7,  11),   S(  7,  -1),   S(  3,  11),   S(  0, -10),   S(  2,   1),
            S( 13,   2),   S( 12,  17),   S(  3,   9),   S(  5,   1),   S( -4,  -4),   S(  1,  -4),   S(  7,  10),   S(  3,   3),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -1),   S( -7, -16),   S( -2,  -2),   S( -7, -22),   S( -4,  -8),   S( -5, -14),   S( -4,  -5),
            S(  8,  12),   S( -4, -10),   S(  6,   4),   S(  4,   5),   S(  8,  -1),   S( -1,  -3),   S( -2, -10),   S( -3,  -6),
            S(  2,  -5),   S(  2,   1),   S(  0,  -7),   S( 13,  16),   S( 13,  30),   S(  8,  26),   S( 17,  22),   S(  4,   4),
            S(  1,  -8),   S( 12,  13),   S( 12,  31),   S(-17,  -3),   S(  3,  10),   S( 17,   6),   S( 14,   3),   S(  9,  17),
            S( -2,  -9),   S( -1,  12),   S( -3,  22),   S( 21,  54),   S( 20,  25),   S( 12,  -1),   S(  9,   2),   S( -3,   1),
            S( -2,  19),   S(  6,  12),   S(  4,  26),   S(  7,  12),   S( 23,  20),   S(  7, -12),   S(  3,   9),   S(  1,  -3),
            S(  5,  -2),   S(  2,  17),   S(  8,  30),   S( 13,  18),   S( 10,  15),   S( -2,   7),   S( -1,  -8),   S(  0,   0),
            S(  3,  -3),   S( 11,  13),   S(  7,  -1),   S(  8,  10),   S(  4,  16),   S(  1,  -1),   S(  4,  10),   S(  4,  -1),

            /* rooks: bucket 0 */
            S(-20,  13),   S(  7, -12),   S(-11,   2),   S(-12,  15),   S(-32,  58),   S(-20,  34),   S(-51,  61),   S(-56,  44),
            S(  1, -19),   S( -4,  17),   S(-29,  22),   S( -2,  29),   S( -4,  42),   S( -7,  24),   S(-18,  11),   S(-25,  43),
            S( 23, -30),   S( 12, -13),   S(-10,  12),   S( -5,  14),   S(-33,  57),   S(-15,  15),   S(-20,  38),   S( -7,  16),
            S(  9, -17),   S( 37,  -3),   S(-32,  33),   S( 15,  16),   S( 10,  46),   S(-18,  44),   S(-24,  51),   S(-18,  31),
            S( 53, -56),   S( 41,   1),   S( 20,  28),   S( 35,  22),   S( 40,  18),   S( 22,  65),   S( 32,  50),   S(  7,  57),
            S( 57, -25),   S( 64,  17),   S(108, -17),   S(105,  24),   S( 25,  55),   S( 29,  59),   S(  8,  68),   S(-42,  80),
            S( 24,  21),   S( 54,  46),   S( 98,  30),   S( 64,  13),   S( 60,  47),   S( 15,  63),   S(-11,  75),   S(-20,  68),
            S(  7, -18),   S( 29,  24),   S( 26,  23),   S( 46,  -4),   S( 25,  46),   S( 45,  13),   S( 34,  16),   S( 52, -43),

            /* rooks: bucket 1 */
            S(-54,  48),   S(-21,   5),   S(-14,  12),   S(-44,  29),   S(-42,  43),   S(-46,  46),   S(-53,  65),   S(-76,  73),
            S(-41,  34),   S(-22,  -5),   S(-21,  19),   S(-26,  26),   S(-30,  19),   S(-43,  43),   S(-23,  20),   S(-32,  52),
            S(-29,  24),   S( -9,  -5),   S(-11,   2),   S(-25,  18),   S(-29,  21),   S(-51,  36),   S(-61,  62),   S(-27,  59),
            S(-40,  45),   S( -6,  13),   S(-14,  28),   S(-28,  17),   S(-37,  36),   S(-49,  64),   S(-32,  58),   S(-68,  86),
            S(-15,  43),   S( 17,  -6),   S( 31,  13),   S( 28,   3),   S(  3,  24),   S( -9,  78),   S(  5,  60),   S(-11,  82),
            S( 48,  35),   S( 73,  -2),   S( 37,  16),   S( -2,  33),   S(  9,  24),   S(  8,  59),   S( 35,  45),   S(  7,  80),
            S( 14,  68),   S( 33,   6),   S(  8,  35),   S( 19,  17),   S( 48,  14),   S(  2,  53),   S( 25,  64),   S( 31,  81),
            S( 50,  -9),   S( 13,  -3),   S(  0,  -6),   S(-18,  -6),   S( 24,   8),   S( 20,  17),   S( 36,  29),   S( 53,  34),

            /* rooks: bucket 2 */
            S(-60,  67),   S(-50,  58),   S(-42,  51),   S(-40,  22),   S(-28,  23),   S(-41,  27),   S(-32,  17),   S(-68,  58),
            S(-49,  61),   S(-50,  54),   S(-46,  56),   S(-44,  35),   S(-46,  40),   S(-42,  21),   S(-19,   8),   S(-49,  38),
            S(-43,  63),   S(-32,  54),   S(-40,  42),   S(-35,  39),   S(-31,  27),   S(-32,  24),   S(-15,  10),   S(-14,  30),
            S(-34,  74),   S(-29,  66),   S(-47,  63),   S(-62,  53),   S(-45,  48),   S(-23,  31),   S( -9,  28),   S(-21,  45),
            S(-13,  84),   S(-20,  80),   S(  6,  66),   S(-14,  43),   S(-31,  57),   S( 29,  23),   S(  5,  43),   S( -3,  65),
            S( 18,  87),   S( 19,  74),   S( 34,  64),   S(-14,  53),   S( 49,  19),   S( 37,  50),   S(111,   1),   S( 54,  64),
            S( 46,  66),   S( -9,  78),   S( 16,  54),   S( 31,  23),   S(  5,   9),   S( 25,  72),   S(-42,  92),   S( 32,  71),
            S( 17,  42),   S( 23,  48),   S( 32,  32),   S(-24,  25),   S(-30,  14),   S( 16,  14),   S( 15,  24),   S( -3,  52),

            /* rooks: bucket 3 */
            S(-17,  71),   S(-13,  67),   S(-15,  89),   S(-13,  80),   S( -2,  45),   S(  2,  40),   S( 19,  12),   S( -8,   5),
            S(  2,  60),   S(-13,  75),   S(-13,  96),   S( -3,  88),   S( -1,  54),   S( 15,  17),   S( 48, -10),   S( 19,  10),
            S( 14,  57),   S( -6,  82),   S( -9,  82),   S( -6,  90),   S( 16,  42),   S(  6,  35),   S( 37,  13),   S( 31,  10),
            S(  3,  87),   S( -7, 107),   S(-17, 108),   S( -7,  95),   S( -4,  68),   S( 15,  52),   S( 36,  31),   S(  7,  31),
            S(  4, 105),   S(-12, 118),   S( 19, 110),   S( 18, 101),   S( 14,  85),   S( 43,  62),   S( 67,  34),   S( 40,  48),
            S(  5, 124),   S( 22, 108),   S( 32, 115),   S( 46,  97),   S(103,  45),   S(131,  28),   S( 89,  39),   S( 47,  44),
            S( 17, 113),   S( 11, 111),   S( 26, 118),   S( 23, 113),   S( 31,  95),   S( 99,  46),   S(106,  94),   S(141,  69),
            S(115, -29),   S( 50,  43),   S( 13,  97),   S( 15,  82),   S( 21,  67),   S( 68,  58),   S( 39,  33),   S( 97,  14),

            /* rooks: bucket 4 */
            S(-24, -23),   S( 13, -16),   S(-22,  -6),   S(-40,  14),   S(-54,  14),   S(-39,  45),   S(-39,   2),   S(-83,  36),
            S(-24, -42),   S(-47,   1),   S(-14, -17),   S(  4, -31),   S( 18, -16),   S( -8,   4),   S(-23,  -2),   S(  1,  13),
            S(-13, -14),   S(-36, -19),   S(-33,  -3),   S( -9, -34),   S(-30,  -7),   S(-41,  17),   S(-19,  16),   S(-61,  20),
            S(-54, -28),   S( 10,   5),   S(  9, -18),   S( 12, -18),   S( 41,   3),   S( -7,  15),   S(-11,  -3),   S(-15,  11),
            S(-14, -30),   S( 27, -35),   S( 27,   6),   S( 50, -13),   S( 64,  -3),   S( 54,  27),   S( 17,  11),   S( 15,  26),
            S(-13, -34),   S( 10,  11),   S(  8,  -1),   S( 19,  10),   S( 28,  23),   S( 11,  13),   S( 30,  17),   S( 30,  37),
            S(-19, -18),   S( 33,  26),   S( 45,  -2),   S( 56, -10),   S( 62,  -3),   S(-10,  13),   S( 13, -12),   S( 22,   7),
            S( 16, -26),   S(  6,  15),   S( 30,  -8),   S( 25, -11),   S( 50,   1),   S( 11,   1),   S(  3,   7),   S(  3,  11),

            /* rooks: bucket 5 */
            S(-24,  24),   S(-16,   6),   S(  2,  -5),   S( 24,  -5),   S(-13,  21),   S(-12,  28),   S(-33,  52),   S(-31,  34),
            S( -4,  -5),   S(-22, -13),   S( 47, -53),   S( 36, -20),   S(-13,   6),   S(-21,  12),   S(-38,  30),   S( -9,  28),
            S(-33,  23),   S( -1,  -2),   S( 10, -20),   S(  4, -13),   S(-13,  -1),   S( 38, -16),   S(-43,  33),   S(-22,  19),
            S(-24,  27),   S(  6,   7),   S( 53, -25),   S( 32,  -4),   S( 40,  -6),   S( -7,  41),   S( 11,  32),   S(  6,  46),
            S( 39,  21),   S( 25,   9),   S( 14,  24),   S(  6,   1),   S( -6,  23),   S( 71,  10),   S( 29,  35),   S( 49,  36),
            S( -4,  33),   S( -6,  13),   S(  6,   8),   S(-12, -11),   S( 20,  16),   S( 17,  27),   S( 57,  16),   S( 48,  31),
            S( 48,   5),   S( 49,  -8),   S(  0,   2),   S( 37,   7),   S( 60,  -5),   S( 58, -12),   S( 88, -13),   S( 45,  15),
            S( 16,  28),   S( 12,   8),   S( 53,  -7),   S(  5,  15),   S( 44,  17),   S( 21,  27),   S( 31,  36),   S( 56,  37),

            /* rooks: bucket 6 */
            S(-46,  45),   S(-34,  35),   S(-30,  28),   S(-33,  25),   S( -2,   9),   S(  5,  -1),   S( 22,  -8),   S(-36,  20),
            S(-37,  27),   S( 11,   8),   S( -3,  11),   S(  2,   2),   S( 22, -14),   S(-20,  -1),   S(-26,   2),   S( -6,  11),
            S(-49,  37),   S(  1,  17),   S(  7,   6),   S(  8,   4),   S( -7,  10),   S( 46, -15),   S(  8, -15),   S( -2,  -4),
            S(-34,  54),   S( -4,  39),   S( 19,  16),   S( 58,  -6),   S( 36,  -7),   S( 26,   4),   S( 16,   2),   S( 16,  32),
            S( -6,  54),   S( 58,  27),   S( 89,  20),   S( 67,  -2),   S( 27,  -2),   S( 34,  20),   S( 71,  -3),   S( 89,   6),
            S( 76,  13),   S( 79,   0),   S( 82,   0),   S( 41, -13),   S(  6,  -9),   S( 23,  34),   S( 33,  -2),   S( 58,  17),
            S( 59,  13),   S(130, -21),   S(109, -22),   S( 99, -33),   S( 38, -12),   S( 52,  -4),   S( 68, -10),   S( 84, -19),
            S( 77, -10),   S( 49,  13),   S(  3,  29),   S( 62,  -6),   S( 61,   3),   S( 30,  25),   S( 82,   8),   S( 54,  19),

            /* rooks: bucket 7 */
            S(-98,  34),   S(-79,  34),   S(-70,  34),   S(-62,  32),   S(-35,  -3),   S(-28, -16),   S(-34,   5),   S(-69, -13),
            S(-82,  33),   S(-31,   8),   S(-52,  19),   S(-60,  29),   S(-28, -14),   S(-15, -12),   S(  7,  -5),   S(  0, -56),
            S(-81,  34),   S(-65,  28),   S(-29,   5),   S(-35,  18),   S(-34,   0),   S(-20,  10),   S( 48, -33),   S(  7, -48),
            S(-71,  36),   S(-13,  16),   S( -2,  12),   S( 63, -22),   S( 13,  -4),   S( 71, -27),   S( 52,  -6),   S( 26, -21),
            S(  1,  27),   S( 28,  22),   S( 58,  10),   S( 86, -12),   S(142, -49),   S(123, -52),   S( 90, -20),   S(-41, -31),
            S( 26,  16),   S( 26,   2),   S( 90,  -6),   S( 83, -22),   S( 76, -13),   S( 40,   9),   S( 23,  34),   S(  0, -25),
            S(  7,  -1),   S( 41, -14),   S( 74, -15),   S(112, -44),   S(123, -45),   S(116, -43),   S( 50,   6),   S( 23, -29),
            S(-25, -16),   S(  6,   3),   S( 37,  -4),   S( 29,  -1),   S( 47, -17),   S( 69,  -9),   S( 31,  13),   S( 24, -17),

            /* rooks: bucket 8 */
            S(-15, -79),   S(-15, -38),   S( -6, -13),   S( 16,   5),   S(-26, -30),   S(-22,   0),   S(-12, -31),   S(-22,   6),
            S(-30, -79),   S(-14, -43),   S(-20,   3),   S(-25, -67),   S(-26, -39),   S(-15, -22),   S(-11,  -6),   S(-38, -33),
            S(  2,  -8),   S( -2, -11),   S( 14,  -3),   S(-10,  18),   S( -8,  48),   S( 12,  28),   S(  4,  50),   S(-17,   5),
            S( -5, -20),   S( -1,   2),   S( -1,  -2),   S( 15,  25),   S(  2,  41),   S( 31,  41),   S( -1,  21),   S(-11, -11),
            S( -9, -38),   S( 10,  21),   S(  8,  20),   S( 17,  38),   S(  8,  23),   S( -3,   4),   S( 13,  46),   S(  0,  22),
            S(-24,   9),   S(  2,  12),   S(-17,   9),   S( -7, -17),   S(  4,  32),   S(-17,  29),   S( -2,   1),   S(  1,  21),
            S(  1,  35),   S(  1,  24),   S(  3,   6),   S( 18,  10),   S( 13,   8),   S(  9,  27),   S(  4,  22),   S(  1,  41),
            S(-13,  15),   S(  1,  13),   S(-20,  32),   S( 33,  48),   S( -7,  20),   S( 10,  39),   S(  0,  23),   S(  7,  40),

            /* rooks: bucket 9 */
            S(-33, -68),   S(-11, -64),   S( -7, -99),   S(-12, -44),   S(-16, -48),   S(  0, -35),   S( -7, -22),   S( -5, -32),
            S(-60, -48),   S(-30, -69),   S(-26, -62),   S(-40, -47),   S(-35, -52),   S(-25,   4),   S(-22, -53),   S(-30, -31),
            S(-10, -12),   S(-20, -11),   S(  5,  -3),   S( -5, -30),   S( -4, -12),   S(  8,  20),   S(  1,   8),   S(  3,  16),
            S( -5,   5),   S(  6,  -4),   S(  3,   3),   S( -1,   6),   S(-11, -30),   S(  6,   2),   S( -7,  -2),   S(  4, -23),
            S( -2,   0),   S( -6, -12),   S( -8, -45),   S( -8,   2),   S(-18, -17),   S(-10,   3),   S(-11, -14),   S( -8,  -9),
            S(-10,   1),   S(-30, -18),   S(-11, -21),   S(  0,  16),   S( -3,  -3),   S( -7,   5),   S( -6,  -2),   S(-13,   6),
            S(  3,  28),   S(  6,   2),   S(  3, -36),   S(  2,  11),   S(  6, -16),   S( 18,   1),   S(  2,   8),   S( -4, -15),
            S(-19,  12),   S(-20,  28),   S( -9,  15),   S( -7,  32),   S(-11,  31),   S(  3,  51),   S(  1,  17),   S( 10,  26),

            /* rooks: bucket 10 */
            S(-23, -28),   S(-56, -11),   S(-32, -43),   S( -3, -52),   S(-12, -47),   S(  2, -79),   S(  5, -65),   S(-19, -44),
            S(-43, -13),   S(-30, -33),   S(-41, -25),   S(-35, -49),   S(-38, -44),   S(-21, -45),   S(-11, -34),   S(-46, -74),
            S( -7, -12),   S(-23, -14),   S(-27, -16),   S(-35, -41),   S( -7, -14),   S(  4, -13),   S( -8, -26),   S(-14, -14),
            S(-27,  -9),   S(-36, -34),   S( -5, -35),   S( -7,   3),   S(  6,   4),   S(  6,  12),   S(-10, -32),   S(  1, -34),
            S(  7,  -8),   S(  4, -10),   S(-13, -15),   S(-10, -33),   S(  7,  11),   S( -2,   1),   S( -5, -23),   S( -8, -33),
            S(-10,   1),   S( 10,  -3),   S( -6, -20),   S( -2, -29),   S(  3, -10),   S( -7,  -8),   S(-20, -31),   S( -3, -20),
            S(-11, -11),   S(  5, -30),   S( -3, -23),   S( -4, -15),   S( 12, -19),   S(-12, -10),   S(-15, -31),   S( -9, -17),
            S(-10,  -3),   S(  5,  26),   S( -2,  32),   S(-13,  13),   S(-10,  32),   S(-28,   5),   S(-33,  14),   S( -4,  10),

            /* rooks: bucket 11 */
            S(-60, -17),   S(-39,  -2),   S(-52,  -9),   S(-30,  -7),   S(-47, -17),   S(-21, -16),   S(-16, -34),   S(-35, -62),
            S(-18, -14),   S(-24, -19),   S(-57, -11),   S(-53, -19),   S(-13, -25),   S( -9, -10),   S(-25, -29),   S(-43, -59),
            S(-32,  25),   S(-23,  13),   S( -7,  32),   S(-19,  18),   S(  9, -20),   S( -4,  -1),   S(  9, -17),   S( -9,  16),
            S(-25,  -7),   S(-12, -15),   S(-13,  12),   S(  7,  16),   S( 20,  13),   S(-19, -32),   S(  7,  16),   S( -7, -20),
            S( -9,  -8),   S(  6,  -5),   S(  3,   6),   S(  3,   7),   S( 35,  -9),   S(  1,  -4),   S( 17,  34),   S(-14, -42),
            S(  1, -18),   S(-13,  -7),   S( 13, -13),   S( 17,  -5),   S(-13, -20),   S(  3,   4),   S(  4,  30),   S( -4,  -9),
            S( -8,   6),   S(-24, -29),   S( -6,  -3),   S( -3,   0),   S(  6,  -4),   S(  3,   7),   S( -1,  14),   S(-14,  -6),
            S( -9,   4),   S( 14,  32),   S(  0,  25),   S( 17,  23),   S(-13,   3),   S( -4,  24),   S( 11,  14),   S(-21,  24),

            /* rooks: bucket 12 */
            S(-31, -94),   S( -9, -13),   S(-18, -54),   S(-19, -35),   S(-11, -25),   S(  9,  -7),   S(-16, -40),   S(-19, -41),
            S(  3,   2),   S(  1,   4),   S(  8,  20),   S(  4,  13),   S(  8,   8),   S( 10,  -9),   S(  6,   9),   S(-18, -25),
            S( -4, -10),   S(  6,  35),   S( 12,  24),   S( 24,  23),   S(  6,  -5),   S( 15,  25),   S(  6,  34),   S( -3,  27),
            S(  8,  22),   S(  5,   4),   S( 15,  33),   S( 11,  20),   S( 12,   7),   S(  5,   8),   S(  5,  19),   S( -3,   5),
            S( 11,  18),   S( 12,  29),   S(  8,  47),   S(  2,  -1),   S(  8,  25),   S( -2, -14),   S(  5,  15),   S(  5,  13),
            S( -3,   0),   S( -5,  -6),   S(  0,  17),   S( -5,   3),   S(  7,  24),   S( -1, -20),   S( 10,  26),   S(  4,   9),
            S(-16, -11),   S(-12,  18),   S(  7,  41),   S( -1,  21),   S( -3,   0),   S( 12,  17),   S(  2,  22),   S(  0,  23),
            S(  3,   5),   S(-11,  29),   S(  5,  31),   S( 13,  21),   S(  2,   6),   S(  1,  20),   S(  3,  11),   S(  2,  14),

            /* rooks: bucket 13 */
            S(-23, -21),   S(-24, -48),   S(-23, -49),   S(-16, -34),   S(-25, -49),   S( -2,  -1),   S(-25, -47),   S(-23, -35),
            S(-14,  -9),   S( -7, -18),   S(  2,   6),   S( -2,  -2),   S( 18,  36),   S(  5,  12),   S(  8,   2),   S(-11, -12),
            S(-13,  -2),   S(-12,   6),   S( -4,  -7),   S(  8,  11),   S(  8,  27),   S( 16,  -2),   S( 12,  44),   S(-12, -27),
            S(  8,  15),   S( -1,   5),   S( -2,   9),   S(  6,  18),   S( 10,  21),   S( -1,   7),   S(  5,  14),   S(  1,  21),
            S(  5,  20),   S(  3,  -9),   S( -5, -21),   S(  3,   5),   S( -4,  23),   S(  0,  -3),   S(  5,   5),   S( -1,  -3),
            S(  0,  14),   S( -4,  -4),   S(-10,  -9),   S(-14,  -2),   S(-12, -12),   S(  3,  -3),   S( -8,   7),   S(  1,   3),
            S(  3, -10),   S(  8,   4),   S(-10, -30),   S(  3,  15),   S( -8,  -4),   S(  6,  10),   S(  1,   3),   S(  0, -14),
            S(  1,  23),   S(-11,  13),   S( -4,   5),   S(  9,  24),   S( -3,  15),   S(  7,  24),   S(  0,  23),   S(  4,   4),

            /* rooks: bucket 14 */
            S( -4, -25),   S(-30, -27),   S(-17, -14),   S(-17, -54),   S(-11, -38),   S( -4, -20),   S(-31, -61),   S(-25, -32),
            S( -7,  26),   S(  4,  27),   S(  6,  10),   S( -1, -19),   S(  0,  -7),   S( -3,  -3),   S( -2,   5),   S( -5,  -4),
            S(  5,  31),   S( -2,  29),   S(  1,   3),   S(  3,   4),   S(  4,   8),   S(  1,  -4),   S(  2,  23),   S(-18, -47),
            S( -4,  13),   S( 15,  21),   S(  6,  16),   S( 10,   4),   S( -8,  -6),   S(  1, -10),   S(  9,  11),   S(-12, -18),
            S(  8,  17),   S( 20,  21),   S( -2,  -5),   S(  1,   7),   S(  2, -12),   S( 18,  31),   S(  0,   2),   S( -3, -17),
            S(  5,  12),   S(  7,  14),   S(  7,  18),   S(  2,   7),   S( -4,   6),   S(-15,   5),   S( -9,  -8),   S( -6,  -6),
            S( -6, -11),   S(  8,  16),   S( -8, -18),   S(-18, -33),   S( -5,   5),   S(  0,  -1),   S(-12, -13),   S( -8,  -8),
            S(  0,   1),   S(  4,   8),   S( -4, -14),   S(  6,  -7),   S(-11, -16),   S(-16, -41),   S(  2,  -5),   S(  1,  31),

            /* rooks: bucket 15 */
            S(-23, -44),   S(-17, -49),   S(-39, -48),   S(-24, -50),   S( -2, -22),   S(-13, -18),   S( -2,  -8),   S(-19, -52),
            S(  6,  29),   S(-11,   0),   S(-11,  -8),   S( -6,  -9),   S( -6, -17),   S(  4,   0),   S(  7,  10),   S(  3,   5),
            S(  5,   8),   S( -7, -13),   S( 11,  22),   S(  7,  -2),   S(  6,  -1),   S( -7, -14),   S(  6,  24),   S(  3,   7),
            S(  2,  10),   S( -2,  -6),   S( 17,  34),   S( -3, -12),   S(  4,  17),   S(  2,   6),   S(  6,  14),   S(  3, -12),
            S(  6,  15),   S(  5,  10),   S(  6,  -8),   S(  2,  12),   S(  6,  13),   S(  3,   0),   S( -3,  27),   S(  5, -11),
            S(  7,  16),   S(  7,   1),   S(  8,   0),   S(  4,   5),   S( -6, -14),   S( -4,  37),   S(  1,  21),   S(  5,   2),
            S(  4,  -2),   S( -3,   6),   S(  8,  20),   S(  4,  11),   S(  1,  16),   S(  4,  16),   S(-13,  11),   S(-10, -30),
            S(  0,  24),   S( -1,  25),   S(  8,  22),   S(  1,  28),   S( -1,   3),   S( -6, -24),   S( -6,  14),   S(-16,  -9),

            /* queens: bucket 0 */
            S( -5,  -7),   S(-24, -48),   S(-33, -55),   S( -1, -96),   S( -8, -53),   S(  9, -60),   S(-55, -28),   S(-16, -10),
            S(-14, -27),   S( 14, -75),   S(  3, -62),   S( -7, -17),   S(  3, -18),   S( -7, -34),   S(-24, -27),   S(-38,  -9),
            S( -2,  11),   S( -3, -20),   S( 29, -50),   S(-10,   6),   S( -6,  22),   S( -1,   1),   S(-32,  -1),   S(-76, -42),
            S(-22,  27),   S( 17, -25),   S( -7,  19),   S(-14,  66),   S( -6,  63),   S(-24,  36),   S(-43,  26),   S(-18, -26),
            S(-18, -17),   S(  4,  65),   S(  6,  33),   S(  0,  40),   S(  2,  64),   S(-22, 103),   S(-58,  68),   S(-42,   3),
            S(-11,  12),   S( 23,  36),   S( 39,  38),   S(-24,  72),   S(-28,  66),   S(-65,  98),   S(-66,  26),   S(-44,   4),
            S(  0,   0),   S(  0,   0),   S( 18,   3),   S(-31,  31),   S(-37,  25),   S(-66,  80),   S(-89,  60),   S(-100,  22),
            S(  0,   0),   S(  0,   0),   S(  9,  -5),   S(-10, -11),   S(-30,  23),   S(-37,   4),   S(-53,  -5),   S(-65, -27),

            /* queens: bucket 1 */
            S( 15,  -4),   S(  7,   2),   S( 14, -49),   S( 30, -88),   S( 37, -44),   S( 12, -27),   S( 14,  -7),   S(  0,  15),
            S(-22,  33),   S( 23,  17),   S( 37, -35),   S( 29,   4),   S( 42,  13),   S(  3,  20),   S(-20,  35),   S(-19,   8),
            S( 47,  -2),   S( 24,   4),   S( 17,  32),   S( 16,  72),   S( -6,  79),   S( 33,  44),   S( -4,  35),   S( 15, -10),
            S( 37,   5),   S( 18,  41),   S( 18,  48),   S( 41,  68),   S( 18,  82),   S(  6,  57),   S(  5,  37),   S(-11,  54),
            S( 44,  -3),   S( 52,  17),   S( 53,  40),   S( 27,  33),   S( 49,  68),   S( 30,  25),   S( -9,  69),   S(  2,  87),
            S( 60,  -1),   S(103,  13),   S( 86,  46),   S( 83,  58),   S( 53,  41),   S( 14,  64),   S( 38,  52),   S( -1,  50),
            S( 98, -24),   S( 56, -20),   S(  0,   0),   S(  0,   0),   S(  6,  42),   S(-11,  20),   S(-11,  49),   S(-43,  32),
            S( 79,  -5),   S( 57, -10),   S(  0,   0),   S(  0,   0),   S( 19,  24),   S( 43,  27),   S( 77,  -1),   S(-20,  34),

            /* queens: bucket 2 */
            S( 33, -15),   S( 29,   7),   S( 32,  16),   S( 45, -30),   S( 45, -33),   S( 29, -24),   S( -1, -21),   S( 35,  29),
            S( 23,   1),   S(  8,  47),   S( 37,  20),   S( 44,  33),   S( 53,   7),   S( 20,  24),   S( 24,  19),   S( 16,  46),
            S( 36,  10),   S( 29,  38),   S( 19, 101),   S( 15,  83),   S( 24,  79),   S( 23,  71),   S( 32,  48),   S( 30,  61),
            S(  1,  63),   S( 20,  83),   S( 19,  82),   S( 12, 123),   S( 31,  94),   S( 22,  92),   S( 34,  65),   S( 34,  82),
            S(  2,  82),   S(-12,  77),   S(  2,  93),   S( 33,  77),   S( 29,  97),   S( 92,  44),   S( 72,  60),   S( 65,  55),
            S(-18,  84),   S( -7,  79),   S( -2,  79),   S( 78,  39),   S( 44,  57),   S( 97,  73),   S(118,  41),   S( 43, 105),
            S( -6,  48),   S(-12,  42),   S( -6,  67),   S( 49,  27),   S(  0,   0),   S(  0,   0),   S( 18,  79),   S( 40,  66),
            S( -1,  33),   S( 35,  -5),   S( 48, -11),   S( 29,  38),   S(  0,   0),   S(  0,   0),   S( 49,  38),   S( 16,  61),

            /* queens: bucket 3 */
            S(-43,  32),   S(-29,  38),   S(-23,  38),   S(-13,  47),   S(-27,  33),   S(-15, -16),   S(-16, -38),   S(-40,  21),
            S(-57,  54),   S(-37,  48),   S(-25,  66),   S(-16,  84),   S(-15,  75),   S(-15,  39),   S( 15, -11),   S( 17, -29),
            S(-51,  77),   S(-39,  89),   S(-32, 113),   S(-41, 144),   S(-30, 124),   S(-22,  97),   S( -9,  57),   S(-11,  23),
            S(-43,  78),   S(-61, 137),   S(-53, 160),   S(-37, 172),   S(-40, 161),   S(-18,  99),   S( -4,  78),   S(-13,  67),
            S(-56, 119),   S(-48, 154),   S(-53, 174),   S(-46, 188),   S(-25, 153),   S(  0, 131),   S(-12, 123),   S(-17,  80),
            S(-64, 111),   S(-62, 158),   S(-65, 181),   S(-61, 192),   S(-55, 169),   S( 18,  98),   S(-18, 120),   S(-22, 116),
            S(-99, 123),   S(-98, 144),   S(-82, 182),   S(-74, 159),   S(-75, 163),   S(-17,  84),   S(  0,   0),   S(  0,   0),
            S(-129, 140),  S(-84, 104),   S(-71, 103),   S(-68, 113),   S(-52, 101),   S(-13,  58),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-36,  -4),   S(-47, -37),   S( -9,   0),   S(-12, -18),   S( -9,  -7),   S( -9,  11),   S(-34, -26),   S( 11,  20),
            S( -2, -11),   S( -7,   6),   S( -2,   0),   S(-15, -14),   S(-41,  18),   S(-18,  12),   S(-48, -11),   S( -1, -16),
            S(  9,  20),   S( 23, -30),   S( 17, -16),   S( 19,   8),   S( 43,   8),   S( 14,  20),   S(-22, -19),   S( 31,  23),
            S( -9, -19),   S( 20, -18),   S(  9,   4),   S(-10,  17),   S( 46,  27),   S(  3,  57),   S(-25,   5),   S(-14,  18),
            S(  0,   0),   S(  0,   0),   S( 17,  -7),   S( 58,  35),   S( 24,  55),   S( 31,  50),   S(  9,  15),   S( 11,  21),
            S(  0,   0),   S(  0,   0),   S( 18,  11),   S( 35,  19),   S( 40,  47),   S( 29,  47),   S( 18,  24),   S( -2,   5),
            S( 15,  -3),   S( 19,   9),   S( 65,  39),   S( 62,  39),   S( 56,  14),   S( 18,  29),   S(  3,  23),   S(-14,  21),
            S( 28,  -6),   S(-18, -33),   S( 26,   9),   S( 44,  18),   S( 14,   5),   S(  7,  21),   S( -2,   2),   S( 19,   6),

            /* queens: bucket 5 */
            S( 35,  21),   S( 24,   8),   S( 13,   5),   S(-13,  25),   S( 32,  -7),   S( 38,  46),   S(  9,  -3),   S( 17,   1),
            S( 19,  15),   S( 13,  -2),   S( 16,  -3),   S( 12,  15),   S( 12,  41),   S(-12, -13),   S( 26,  13),   S( 10,   4),
            S( 19,   2),   S( 49,  -3),   S( 24,   0),   S( 10,  17),   S( 22,   7),   S( 33,  15),   S( 25,  40),   S( 13,  14),
            S(  9, -31),   S( 38,   6),   S( 26, -14),   S( 35,  16),   S( 64,   9),   S( 33,  13),   S( 35,  48),   S(  3,  30),
            S( 41,  -6),   S( 28, -40),   S(  0,   0),   S(  0,   0),   S( 12,  12),   S( 32,  15),   S( 38,  51),   S( 15,  33),
            S( 36,  14),   S( 35,   6),   S(  0,   0),   S(  0,   0),   S( 30,  20),   S( 62,  33),   S( 42,  34),   S( 47,  35),
            S( 73,   5),   S( 75,  14),   S( 51,  40),   S( 25,  26),   S( 53,  22),   S( 94,  44),   S( 65,  57),   S( 49,  30),
            S( 40,  27),   S( 53,  12),   S( 65,  20),   S( 44,  -2),   S( 55,  19),   S( 61,  36),   S( 67,  44),   S( 57,  29),

            /* queens: bucket 6 */
            S( 46,  50),   S( -1,   1),   S( 33,  15),   S( 32,  21),   S( 24,  14),   S( -7,   0),   S( -1,  11),   S(  6,  18),
            S( 25,  17),   S( 26,  29),   S( 58,  41),   S( 56,  28),   S( 41,  24),   S( 18,  13),   S(-11,  26),   S( 25,  32),
            S(-10,  45),   S( 37,  34),   S( 30,  36),   S( 53,  14),   S( 38,  13),   S( 50,  -2),   S( 67,  28),   S( 66,  58),
            S( 23,  35),   S(  7,  27),   S( 51,  12),   S( 96,  20),   S( 47,  -6),   S( 49,  11),   S( 86,  11),   S(100,  45),
            S( 29,  51),   S( 34,  36),   S( 57,  40),   S( 53,  34),   S(  0,   0),   S(  0,   0),   S( 66,  23),   S(113,  54),
            S( 40,  46),   S( 56,  45),   S( 48,  54),   S( 28,   9),   S(  0,   0),   S(  0,   0),   S( 79,  47),   S(113,  44),
            S( 59,  37),   S( 26,  26),   S( 76,  20),   S( 63,  22),   S( 42,  38),   S( 69,  48),   S(132,  28),   S(140,  12),
            S( 35,  37),   S( 66,  22),   S( 71,  14),   S( 81,  35),   S(104,  15),   S( 98,  13),   S(111,  14),   S( 97,  26),

            /* queens: bucket 7 */
            S(-11,  24),   S(-12,   1),   S(-26,  23),   S(-10,  26),   S( 10,   3),   S(-14,   5),   S( -5,  15),   S(-14,  -8),
            S(-11,  24),   S(-49,  28),   S( -8,  52),   S(-12,  77),   S(-11,  41),   S( 10,  24),   S(  8,   2),   S(-29,  -3),
            S(  2,  23),   S(-17,  35),   S(-18,  89),   S( 36,  47),   S( 50,  28),   S( 35,   6),   S( 55, -26),   S( 53,  -2),
            S(-18,  23),   S( 15,  43),   S( 16,  69),   S( 45,  68),   S( 78,  45),   S( 76,  -2),   S( 85, -32),   S( 49,  -3),
            S( 12,  24),   S(-14,  61),   S( 16, 102),   S( 51,  80),   S( 91,  18),   S( 75,  -1),   S(  0,   0),   S(  0,   0),
            S( -3,  46),   S(-13,  88),   S(  8,  88),   S(  0,  84),   S( 61,  35),   S( 98,  53),   S(  0,   0),   S(  0,   0),
            S(-39,  61),   S(-24,  43),   S(  9,  59),   S( 35,  60),   S( 68,  40),   S( 84,  19),   S( 70,  24),   S( 65,  31),
            S( 32,  16),   S( 44,  30),   S( 50,  54),   S( 50,  21),   S( 52,  37),   S( 32,   3),   S(-10,   5),   S( 72,  -9),

            /* queens: bucket 8 */
            S(-17, -36),   S(  0, -23),   S(-16, -43),   S( -3,  -8),   S(-17, -31),   S(  8,  -4),   S( -1, -12),   S(  1,   5),
            S(-20, -31),   S( -5, -14),   S(  3, -14),   S( -4,  -9),   S(  8,  -4),   S( -5, -11),   S( -3,   3),   S( -1,   1),
            S(  0,   0),   S(  0,   0),   S(  0, -16),   S( -9, -45),   S(  5,   2),   S(  7,  -7),   S( -8,  -9),   S(  3,   6),
            S(  0,   0),   S(  0,   0),   S(  1,   1),   S( -2, -12),   S( -3,   0),   S(  3,  -2),   S( 11,  19),   S(  6,   3),
            S( -2, -10),   S(  7,  11),   S(  8,   1),   S( 12,  -7),   S(  6, -12),   S( 11,  11),   S( 13,  12),   S(-10,  -9),
            S(  2, -13),   S(  4, -17),   S( 15,  15),   S(  2, -21),   S( 11,   6),   S( 26,  33),   S(  7,  -5),   S( -2,  -4),
            S(-17, -38),   S( -1, -13),   S( 12,   9),   S( 25,  38),   S( 11,  10),   S( 16,  38),   S(  4,   5),   S(  5,   1),
            S(  1,   1),   S(  3,  -7),   S( 13,   8),   S(  8,  -4),   S( 17,  18),   S( -4,  -6),   S(  3,  10),   S(-17, -28),

            /* queens: bucket 9 */
            S(  9, -11),   S(-18, -33),   S(-13, -32),   S( 13,  -8),   S( -6, -35),   S( -2, -10),   S( -5,  -9),   S( -2, -14),
            S(  0,  -6),   S( -9, -19),   S(-10, -26),   S(  2, -15),   S(-22, -50),   S(-12, -31),   S(  6,  -3),   S(  1,  -9),
            S(-16, -44),   S(-13, -27),   S(  0,   0),   S(  0,   0),   S(  5,  -8),   S( 10,  -9),   S( -4,  -9),   S(  5,  -4),
            S(  2,  -7),   S(-11, -29),   S(  0,   0),   S(  0,   0),   S(  1,  -3),   S( 10,   2),   S( 10,  10),   S( -2,   3),
            S( -7, -25),   S(  2, -12),   S(  0,  -6),   S(-10,  -9),   S( -5, -28),   S( 12,  17),   S(  5,  -9),   S(  0, -16),
            S( 10,  10),   S( -1, -27),   S(  5, -10),   S( -3, -19),   S( -1, -10),   S(  4,   3),   S( -3, -12),   S( -3, -13),
            S(  8,   5),   S(  8,  -6),   S( -5,  -3),   S(  0,   8),   S( 22,  24),   S( 24,  27),   S(  6,  19),   S(  7, -12),
            S( 15, -12),   S( 25,  16),   S( -1,  -7),   S( 19,  12),   S( 20,  16),   S(  5,  12),   S(  0, -19),   S( 13,   2),

            /* queens: bucket 10 */
            S( 15,   9),   S( 12,   8),   S(  0, -11),   S( -4, -27),   S(-10, -30),   S( -9, -17),   S( -4, -27),   S( -5, -15),
            S(  6,   3),   S(-13, -21),   S( -6, -23),   S(-17, -51),   S( -3,  -9),   S( 11,   0),   S(-10, -26),   S( -5,  -6),
            S( -2,   1),   S(  2,   3),   S( -1,  -3),   S( -7, -17),   S(  0,   0),   S(  0,   0),   S(  3,  -6),   S(-11, -21),
            S( -4, -10),   S(  3,   3),   S(  3,   2),   S(  9,   2),   S(  0,   0),   S(  0,   0),   S( -5, -14),   S(  0, -16),
            S( 11,  15),   S( 15,   4),   S(  3,  -4),   S( 31,  33),   S(  0,   2),   S( -1,  -1),   S(  3, -10),   S( 11, -25),
            S( -6,  -9),   S(  6,   6),   S( 22,  26),   S( 11,  12),   S( 15,  14),   S( 15,  24),   S( 17,  10),   S( -4, -23),
            S(  8,   5),   S( 18,  28),   S( 18,  26),   S( 20,  16),   S( 10,  16),   S( 24,  12),   S( 14,   8),   S(  5,  -5),
            S(-12, -31),   S(  3,   5),   S( 21,   5),   S( -6,  -2),   S( 14,  14),   S(  1,   1),   S( 13,   8),   S(  8, -10),

            /* queens: bucket 11 */
            S(-11,  -4),   S( -4,  -1),   S( -8, -10),   S(-18, -19),   S( -5, -15),   S(-20, -34),   S( -7, -32),   S( -8, -15),
            S( -5,   0),   S(  1,   8),   S(-23, -11),   S( -7,   4),   S( 21,   1),   S( -9, -26),   S(  8,  -1),   S( -5, -12),
            S(  3,   7),   S(  5,   1),   S(-19,  12),   S( -2,   2),   S( -2, -19),   S(-21, -29),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S( -8,  10),   S( -3,  11),   S( -2,   2),   S(  1,  -7),   S( -2,   7),   S(  0,   0),   S(  0,   0),
            S(  1,  12),   S( 14,  15),   S( 16,  23),   S(  3,  21),   S( 42,  46),   S( 18,  28),   S(  8,  -1),   S(-10, -28),
            S(  0,   4),   S(  1,  -1),   S( -1,  11),   S( 11,  27),   S( 14,  19),   S(  1,   4),   S(  5, -10),   S(  6, -21),
            S(  2,   3),   S(  9,  11),   S( 15,  23),   S(  1,  12),   S( 18,  56),   S( 15,  12),   S(  4,   0),   S( 10,  -4),
            S(-18, -58),   S(  9,  13),   S( -8,  -8),   S(  4,  37),   S( 14,  30),   S( 11,   0),   S( -7,  -4),   S( 11,  -2),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  2,   2),   S(-14, -18),   S( -6,  -5),   S(-12, -20),   S( -1,  -3),   S( -2,  -3),
            S(  0,   0),   S(  0,   0),   S(  6,   3),   S( -9, -17),   S( -8,  -9),   S(-12, -24),   S( -8, -16),   S(  2,   0),
            S( -6,  -9),   S(  5,   7),   S( -5,  -7),   S(-11, -35),   S( 15,  30),   S( -1,  11),   S( -2,  -7),   S(  8,   9),
            S( -8, -18),   S(  5,   3),   S(  7,  14),   S(  2,  11),   S(  0,   1),   S( -2,   9),   S( -3,  -2),   S( -3,  -8),
            S(-17, -29),   S(  3,   9),   S(  5,   2),   S(  5,   4),   S(  5,  27),   S( -6, -21),   S( -8, -17),   S( -2,  -1),
            S(  2,  -6),   S( -4, -12),   S(  0, -13),   S(  4,   8),   S( -6, -10),   S(-10,  -2),   S(-11, -10),   S( -2,  -7),
            S( -8, -11),   S(  4,   7),   S( -6, -11),   S( 13,  10),   S( -1,  -1),   S( -9, -15),   S(  0,   0),   S( -7, -23),
            S(  6,  12),   S(  0,  -3),   S(  2,  -6),   S(  0,   3),   S( -6,  -7),   S(-14, -13),   S( -5,  11),   S( -9, -13),

            /* queens: bucket 13 */
            S(-21, -35),   S(-14, -29),   S(  0,   0),   S(  0,   0),   S(-16, -27),   S(-12, -34),   S(  0,  -2),   S( -4, -10),
            S(-16, -46),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(-16, -36),   S(-23, -45),   S(-12, -22),   S( -4,  -6),
            S(-21, -37),   S( -4, -13),   S( -4,  -5),   S( -2, -14),   S(-21, -40),   S(-10, -16),   S( -8,  -7),   S( -1,  -4),
            S( -8, -19),   S(-18, -29),   S(  1,  -8),   S( -6, -18),   S( 11,   4),   S( 17,  31),   S( -4, -16),   S( -8, -11),
            S(  5, -10),   S(  1, -23),   S( -7, -20),   S( 11,  23),   S( -8, -13),   S( -2, -18),   S( -3,  -7),   S(  2, -11),
            S( -2,  -3),   S(-14, -19),   S(  4,   2),   S( 10,  21),   S(  0, -11),   S( -6,  -8),   S(-13, -23),   S(-10, -23),
            S(  0,   0),   S( -4, -10),   S( 11,  24),   S( -2,  -2),   S(  3,   2),   S(  8,   0),   S(-14, -25),   S( -7, -11),
            S( -8,  -6),   S( -2,  -7),   S( -6, -15),   S(  1,  -7),   S(  3,  -2),   S( -2,  -3),   S( -1,  -8),   S(-13, -20),

            /* queens: bucket 14 */
            S( -6, -15),   S(  0,  -9),   S( -8, -18),   S( -1,  -5),   S(  0,   0),   S(  0,   0),   S( -3,  -7),   S( -7, -22),
            S( -8, -24),   S(-27, -48),   S(-12, -25),   S( -4, -13),   S(  0,   0),   S(  0,   0),   S( -9, -23),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -6, -21),   S(-13, -25),   S( -3,  -5),   S(  2,   3),   S(-10, -16),   S(-17, -32),
            S( -9, -12),   S( -2,  -2),   S(  0,  -1),   S(-16, -21),   S( -7, -15),   S(-19, -29),   S( -3, -22),   S(  0,   0),
            S( -6, -12),   S( -5, -13),   S( -4, -16),   S(  5,   7),   S(  5,  18),   S(-10, -26),   S( -9,  -5),   S( -2,  -3),
            S( -6, -13),   S(  2,  -4),   S(-13, -21),   S(-13, -23),   S(  6,   9),   S(  1,   4),   S( -2,  -6),   S(-10, -11),
            S(-10, -16),   S( -2,  -9),   S(  0,  -1),   S(  3,   6),   S(  2,   4),   S(  3,   5),   S( -9, -21),   S( -3,  -9),
            S(-12, -18),   S(  4,  -5),   S(-10, -14),   S( -3,  -9),   S(  3,   1),   S( -3,  -2),   S( -4,  -2),   S(  2,  -7),

            /* queens: bucket 15 */
            S(  1,   3),   S( -7, -18),   S(  4,   1),   S(-11, -17),   S(  4,   6),   S(-10, -10),   S(  0,   0),   S(  0,   0),
            S( -5,  -4),   S(  1,   6),   S(-13, -17),   S( -8, -18),   S(  0,  -7),   S(  2,   7),   S(  0,   0),   S(  0,   0),
            S( -1,  -1),   S(  1,  -1),   S(-12,  -4),   S( -6,  -6),   S( -9, -22),   S(  4,   4),   S( -1,   2),   S( -1,  -4),
            S( -2,  -5),   S(-11, -15),   S( -4,  -5),   S(  0,   6),   S(  9,  26),   S(  7,  28),   S( -3,   4),   S( -4, -15),
            S(  1,   3),   S(  1,   0),   S( -4, -10),   S( -1,  -3),   S( 11,  50),   S(  4,  20),   S(  3,  11),   S( -6, -16),
            S( -1,  -3),   S( -3,  -3),   S( -4,  -8),   S( -6,  -2),   S( -2,   6),   S( -9,  -8),   S(  1,  12),   S( -7,  -5),
            S( -5, -12),   S(  0,   0),   S( -5,   4),   S(  3,   3),   S( -7,  -9),   S(  1,   6),   S(  5,  12),   S( -5, -10),
            S( -8, -18),   S(-13, -31),   S( -2, -10),   S(  2,   3),   S(-13,  -3),   S( -3,  -1),   S(  1,  -1),   S( -3,   5),

            /* kings: bucket 0 */
            S(-10, -21),   S( 29,  -8),   S( 15,  -4),   S(-27,  14),   S( -8,  13),   S( 30, -25),   S(  3,   2),   S( 11, -49),
            S(-18,  31),   S( -2,  -1),   S( -1,   4),   S(-45,  25),   S(-42,  41),   S(-16,  21),   S(-13,  34),   S( -4,  27),
            S( 13,   3),   S( 66, -29),   S(  3,  -4),   S(-22,   4),   S(-29,   4),   S( -1,  -4),   S(-29,  16),   S( 29, -28),
            S(-26, -26),   S(  8, -31),   S(  8, -29),   S(-25,   8),   S(-48,  33),   S(-49,  28),   S(-40,  38),   S(-14,  31),
            S(-47, -123),  S( -3, -48),   S(  0, -36),   S( 13, -22),   S(-47,  -6),   S(-30,  10),   S(-21,  12),   S(  1,  -9),
            S(-10, -122),  S(  0,   8),   S( -9, -57),   S(-13,  -7),   S( -2, -13),   S(-25,  20),   S( 15,  23),   S(-19,   8),
            S(  0,   0),   S(  0,   0),   S( -1, -51),   S(  4, -35),   S(-20,  -6),   S(-11, -15),   S(-28,   5),   S(-10,  -3),
            S(  0,   0),   S(  0,   0),   S(-13, -11),   S(  2,  -9),   S(  9,  -1),   S( -5,  13),   S(  8,   4),   S( 10,   0),

            /* kings: bucket 1 */
            S(  7, -26),   S( 32, -21),   S( 15, -16),   S( 29,  -4),   S(  0,  -2),   S( 33, -19),   S(  6,   4),   S( 20, -24),
            S( 10,  -1),   S(  6,   9),   S( -1,  -7),   S(-47,  27),   S(-31,  21),   S(-14,  15),   S( -4,  16),   S(  3,  10),
            S( -8, -16),   S(  0, -13),   S(  5, -16),   S( 13, -18),   S(-32,   0),   S( 15, -17),   S( 23, -11),   S( 39, -12),
            S( -2,  -1),   S(  3, -11),   S(  3,  -5),   S( -4,   5),   S(  9,   9),   S(-11,   2),   S( 30,  -6),   S(-20,  26),
            S(-17, -55),   S(-15, -46),   S( -8, -53),   S(-14, -43),   S( -2, -25),   S( -2, -29),   S(-11,  -3),   S( -5,  -3),
            S(-30,  -1),   S(-102,   3),  S(-31,  27),   S(  3,  20),   S(-41,   4),   S(-25,  14),   S( 15,   3),   S( -6,  -8),
            S(-34, -53),   S(-24,   5),   S(  0,   0),   S(  0,   0),   S(-41,  12),   S(-50,  27),   S( -5,  26),   S( -2, -33),
            S(-29, -113),  S(-12, -16),   S(  0,   0),   S(  0,   0),   S( -8,   8),   S(-13,  15),   S( -2,  20),   S( -4, -47),

            /* kings: bucket 2 */
            S( 13, -56),   S(  9,  -2),   S( 16, -20),   S( 17,  -9),   S(  1,   6),   S( 35, -23),   S( -2,  16),   S( 20, -26),
            S( 34, -36),   S(-15,  30),   S(-15,   8),   S(-18,   8),   S(-25,  15),   S(-13,   5),   S(  5,   0),   S(  2,   0),
            S(-29,  -4),   S(-18, -13),   S( -7, -11),   S( -9, -18),   S( -7,  -5),   S(  6, -19),   S( 30, -18),   S( 27, -17),
            S( 14,  13),   S(-19,  15),   S(  1,   3),   S(-24,  12),   S( 28,  -6),   S(-15,  -9),   S( 32, -28),   S( 31, -10),
            S( -4, -10),   S( 14, -15),   S( 24, -37),   S(  7, -30),   S( 31, -49),   S(-21, -42),   S( 24, -50),   S(  9, -46),
            S(  3,   8),   S(-11,  -6),   S(-38,   1),   S(-37, -13),   S(  3,  -1),   S(-11,  25),   S(-82,   9),   S(-15, -19),
            S( -7, -11),   S( -9,  21),   S(-74,  13),   S(-17,   7),   S(  0,   0),   S(  0,   0),   S(-12,  16),   S(-34, -37),
            S( -7, -39),   S(-19, -27),   S(-30, -32),   S( -6,   8),   S(  0,   0),   S(  0,   0),   S(-10, -14),   S(-33, -123),

            /* kings: bucket 3 */
            S( -5, -53),   S( 16,  -8),   S( 27, -24),   S( -5,  -7),   S( -1, -13),   S( 34, -25),   S(  1,  14),   S(  6, -27),
            S(  1,  18),   S(-19,  37),   S(-18,   5),   S(-38,  17),   S(-54,  31),   S( -2,   0),   S( -6,  17),   S(  2,  13),
            S( 17, -26),   S(  1,  -4),   S( -3,  -8),   S(-34,  -1),   S(-12,   9),   S( 22, -20),   S( 51, -21),   S( 55, -17),
            S(-19,  31),   S(-92,  46),   S(-56,  19),   S(-48,  15),   S(-35,  13),   S(-14, -22),   S(-38,  -3),   S(-35, -15),
            S(-12,   9),   S(-11,  -4),   S(-36, -10),   S(-23, -16),   S( 30, -43),   S( 52, -67),   S( 33, -70),   S( 13, -83),
            S(-11, -13),   S( 18,   6),   S( 18, -10),   S( -1, -24),   S( 45, -32),   S( 58, -51),   S( 71, -22),   S( 53, -118),
            S(-20, -10),   S( 24,  10),   S( 12, -12),   S( 28, -23),   S( 29, -29),   S( 26, -56),   S(  0,   0),   S(  0,   0),
            S( -4, -10),   S(  7,  10),   S( -2,  20),   S( 13, -11),   S( 10, -70),   S( -2,   9),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-55,   7),   S(  9,  34),   S(  9,  21),   S( 13,   1),   S( -9,   7),   S(  9, -11),   S(  5,   7),   S( 22, -36),
            S(-37,  23),   S( 20,  20),   S( -8,  18),   S( -7,   2),   S( 29,  -2),   S( 22,  -4),   S( 51, -15),   S( 14,  -3),
            S(  0,  26),   S( 13, -13),   S( 17,  -4),   S( -8,   3),   S(-22,  11),   S( 22, -22),   S(-38,   8),   S( 17, -13),
            S( -1, -21),   S(-12,   8),   S(  4,  16),   S(  5,   5),   S(-20,  10),   S(-13,  17),   S( 16,   9),   S( 12,   6),
            S(  0,   0),   S(  0,   0),   S( -1,   2),   S(-29,  13),   S(-37,  14),   S(-26, -16),   S(-20,   1),   S( -3,  -1),
            S(  0,   0),   S(  0,   0),   S(  6, -15),   S( -3,  25),   S(-12,  26),   S(-28, -11),   S(  5, -15),   S( -1,  17),
            S( -2, -20),   S( -4,  -6),   S( -4, -23),   S(  1,  20),   S( -6,  24),   S(-28,  -8),   S(-12,  21),   S(  4,  -5),
            S( -5, -23),   S(  3, -14),   S(-10, -20),   S( -7,   3),   S(  6,  11),   S( -7, -11),   S( -6,   1),   S(  5,  12),

            /* kings: bucket 5 */
            S( 32,  -3),   S( -9,  13),   S(-34,  23),   S(-41,  28),   S(-18,  26),   S(  2,  11),   S( 38,  -4),   S( 30,  -9),
            S( -2,   0),   S( 15,  10),   S( 26,  -4),   S( 24,  -6),   S( 16,  -4),   S( 38, -12),   S( 28,   3),   S( 47, -17),
            S(-10,   9),   S( -5,  -8),   S(-13,  -5),   S( -1,  -7),   S(  9,  -2),   S(-38,   0),   S( -4,   2),   S( 19,  -3),
            S( -2, -13),   S( -1,  -7),   S(  9,  -6),   S(  5,  17),   S(  0,  21),   S(  7,   3),   S( 15,   5),   S(  9,   4),
            S( -3, -29),   S(-31, -46),   S(  0,   0),   S(  0,   0),   S( -7,  -4),   S(-21, -14),   S(  4, -14),   S( -7,   4),
            S( -6, -41),   S(-25, -30),   S(  0,   0),   S(  0,   0),   S(-21,  37),   S(-54,  11),   S(-17,  -4),   S( -5,  -5),
            S(-16, -33),   S(-30,  19),   S(  2,   9),   S( -1, -18),   S(-28,  28),   S(-41,  19),   S(  0,   7),   S( 10,  18),
            S(-10, -101),  S( -8,  11),   S(-10, -27),   S( -3, -35),   S(-10, -18),   S( -5,   7),   S( -3, -17),   S(  0,   7),

            /* kings: bucket 6 */
            S( 38, -37),   S( 31, -14),   S( -1,   1),   S(-20,  22),   S( -9,  20),   S(-20,  19),   S(  1,  21),   S(  9,   1),
            S( 48, -28),   S( 10,  17),   S( 14,  -7),   S( 25,  -9),   S( 21,  -4),   S( -9,  11),   S( 13,   1),   S(  6,   1),
            S( 19, -19),   S(-25,   3),   S(-14,  -9),   S( -1,  -7),   S( 13, -11),   S(-41,   5),   S( 10,  -2),   S(-15,  13),
            S( 13,   5),   S( 26,  -4),   S( 16, -13),   S( 22,   5),   S( 57,   0),   S(-27,   4),   S( -5,   6),   S(  5,  -1),
            S(  9, -19),   S( 16, -30),   S(-24, -11),   S( -2, -17),   S(  0,   0),   S(  0,   0),   S(-44, -21),   S(-39, -18),
            S(-15,  -1),   S(  4,  -1),   S(-29,  -1),   S( -9, -22),   S(  0,   0),   S(  0,   0),   S(-25, -16),   S(-27, -23),
            S(  0,  -9),   S( -9,   7),   S(-40,  11),   S(-16,  -3),   S(  3,   6),   S( -9, -31),   S(-29, -13),   S( -7, -39),
            S( -1,  -7),   S(  1,  -7),   S( -3,  10),   S(-14, -30),   S( -8, -38),   S( -4, -26),   S( -6,  -4),   S(  0, -60),

            /* kings: bucket 7 */
            S( 30, -34),   S( -7,  -3),   S(-25,  -4),   S(-14,  10),   S(-28,  12),   S(-41,  35),   S(-27,  33),   S(-37,  22),
            S( 11,  -2),   S( 22, -20),   S( -2,  -8),   S(-33,   7),   S(-13,   7),   S(-38,  22),   S(  2,  -3),   S( -2,  13),
            S( 29, -30),   S(-17,  -8),   S(-32,  -2),   S(-33,   1),   S(-46,   8),   S(-31,  13),   S( 12,  -3),   S(-43,  20),
            S(-22,  18),   S(  4,  10),   S( -9,   1),   S( 36,  -6),   S( 32,  -9),   S( 49, -28),   S( 16, -10),   S( 17, -10),
            S(-15,  15),   S( -4,   1),   S(  1, -24),   S(  6, -17),   S( 12, -24),   S(  9, -22),   S(  0,   0),   S(  0,   0),
            S( -9, -32),   S( -1,  -7),   S( 15, -10),   S( 10,  -6),   S( 23,  -9),   S( 17, -12),   S(  0,   0),   S(  0,   0),
            S( 14,  19),   S( -2, -19),   S(  1,   6),   S(-14, -12),   S(  6, -19),   S( -6, -28),   S(  5, -16),   S(-11,  11),
            S(  7,   8),   S( -8,  -8),   S( 11,  20),   S( -3,  -5),   S(  9,  16),   S(-18, -51),   S(  8, -11),   S(-11, -60),

            /* kings: bucket 8 */
            S( 15, 119),   S( -5,  85),   S( 39,  41),   S( -3,  -2),   S(-13,  13),   S(-14,  -5),   S( 33, -16),   S(-16, -20),
            S( 29,  72),   S( 23,  15),   S( 46,  62),   S( 82,  -3),   S( 18,  24),   S(  6,  -7),   S( -5,  10),   S(  2,  26),
            S(  0,   0),   S(  0,   0),   S( 28,  67),   S( 38,   5),   S( 18,   7),   S( -9,  -8),   S( -3,  14),   S(  8, -17),
            S(  0,   0),   S(  0,   0),   S(  4,  76),   S( -7,   1),   S(-17,  37),   S( -7,  18),   S( 14,  11),   S( 10,  14),
            S( -4, -26),   S( -1,  27),   S(  3,  13),   S(-15,  25),   S(-17,  -3),   S(  4, -16),   S(  0,  11),   S(-14, -27),
            S(  5,  14),   S(  0, -15),   S( -3, -13),   S( -7,   1),   S(-14,   2),   S(-11,  -2),   S( -9,  -2),   S(  9,  -8),
            S( -5, -14),   S( -8, -12),   S(  6,   9),   S( -1, -10),   S( -3, -32),   S(-11,   6),   S( -3,   0),   S(  5, -45),
            S( -6,  -9),   S(-10, -26),   S( -2, -11),   S( -6, -22),   S(  7,   7),   S( -5,   2),   S(  1,  -4),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  7,  28),   S(-15,  34),   S(-19,  58),   S( 15,  10),   S(-18,  34),   S(-25,  28),   S( 42,   4),   S( 21,  13),
            S(-18,  34),   S( 35,  24),   S(  4,   1),   S( 48,   2),   S( 58,  18),   S( 24,   6),   S( -4,  28),   S(-16,  13),
            S( -6,  12),   S( 22,  13),   S(  0,   0),   S(  0,   0),   S( 45,  17),   S( -1,   3),   S( 10,  -1),   S(-19,  21),
            S( -1, -31),   S( 12, -22),   S(  0,   0),   S(  0,   0),   S(  7,  35),   S( 15,   0),   S(-10,  10),   S(-14,  29),
            S(  4, -21),   S( 11,  -2),   S(  4,  16),   S(  0,  13),   S(-13,  17),   S(-20,  15),   S( -9,  13),   S(  1, -16),
            S(  6,   3),   S(  1,  -6),   S(  7,  -9),   S(-10, -20),   S(-13,  13),   S(  0,   8),   S(-32,   2),   S(  5,  32),
            S(  2,  -7),   S( -2, -20),   S(  0,  -9),   S(  2, -30),   S( 14, -27),   S( 14,  16),   S(-17,  -9),   S(  4,   4),
            S(  7,   5),   S( -1, -23),   S( 10, -24),   S( -4, -20),   S( -1, -18),   S(  3,   8),   S( -6,  12),   S(  9,  -1),

            /* kings: bucket 10 */
            S( 34,  -2),   S(  3,  -8),   S(  6,   9),   S(  6,  24),   S(-13,  21),   S(-91,  50),   S(-29,  46),   S(-84,  83),
            S(  5,  -1),   S( 62,   0),   S( 23,  -5),   S( 33,  10),   S( 56,  13),   S( 48,   4),   S( 12,  28),   S(-85,  49),
            S( 16,   6),   S( 28,   0),   S( 26, -11),   S( 12,  14),   S(  0,   0),   S(  0,   0),   S( -7,  23),   S(-58,  28),
            S( 15,   6),   S( 43, -27),   S( 36, -33),   S( 29,   4),   S(  0,   0),   S(  0,   0),   S( -1,  14),   S(  7,   2),
            S(  4,   6),   S( 27,   6),   S( 30, -21),   S(  9, -30),   S(  4, -18),   S(  7,  25),   S(  9,   8),   S( -9,  16),
            S(  4,  14),   S(  4,  -7),   S( -3,   6),   S( 10,  -7),   S(  7,  -1),   S(-17,  -6),   S(-11,   5),   S(  0,  -8),
            S(  1, -42),   S( -3, -16),   S(  9, -10),   S( 14,   2),   S( 11,   1),   S( -9, -20),   S(  5, -27),   S(  5,   4),
            S(  4,   6),   S( 11, -12),   S( -2, -14),   S(  0,   2),   S(  5, -14),   S(  0, -30),   S( -5,  -7),   S(  9,   3),

            /* kings: bucket 11 */
            S( -6, -20),   S( 10,   8),   S(  7, -10),   S( -6,  15),   S( -8,   7),   S(-68,  59),   S(-74,  82),   S(-123, 151),
            S( -2, -27),   S( 22,   4),   S(-11, -17),   S( 16,  22),   S( 83,   0),   S( 54,  42),   S(  8,  23),   S( 26,  40),
            S(  2, -51),   S( -3,  18),   S( -1, -10),   S( 20,  12),   S( 59,   1),   S( 25,  63),   S(  0,   0),   S(  0,   0),
            S(  1,  21),   S( 20,  12),   S( -5,   3),   S( 10,  15),   S( 28,  -9),   S( 22,  24),   S(  0,   0),   S(  0,   0),
            S(  0,  35),   S(  2,  -5),   S(  8,  -7),   S( 13, -16),   S( 15,   2),   S( -1,  -1),   S(  8,  11),   S(  7,   1),
            S( 12,   9),   S(  0, -14),   S( 15, -12),   S(  0,   5),   S( -5,  -7),   S(  2, -16),   S( -4,  -8),   S(-11,  -4),
            S(  7,  12),   S(  8,  -6),   S( 18,  22),   S(  1, -25),   S( 17, -17),   S(  3,   3),   S(-10, -12),   S( -7, -13),
            S(  5,   8),   S(  5,   0),   S(-12, -22),   S(  5,  -7),   S( -4, -19),   S( -7, -17),   S(  0, -19),   S(  5,  12),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 19,  59),   S(  6,  -6),   S(  1,  -2),   S(  7,  13),   S(  8,  -2),   S(-19,   6),
            S(  0,   0),   S(  0,   0),   S( 46, 111),   S( 28,  14),   S( 21,  44),   S( 13,  -3),   S( 22,  -5),   S(-19,   0),
            S( -1,   9),   S(  3,  13),   S( 23,  73),   S( 38,  20),   S(  7,  -8),   S( 10,   2),   S(  1, -12),   S(-10,  -3),
            S( -2,   9),   S(  9,  32),   S( -1,  18),   S(  4,  -4),   S( -7,   2),   S( -1,  19),   S( -3,  10),   S(  1,   7),
            S(  9,  17),   S(  6,  23),   S( 10,  20),   S( -3,  41),   S( -4,  40),   S( -1,   2),   S( -9,  14),   S(-12, -12),
            S(  5,   5),   S(  8,  16),   S( -2,  -2),   S(-10, -15),   S( -1,   6),   S( -8,  16),   S( -9, -15),   S(  6,  -2),
            S(  3,   8),   S( -7, -13),   S( -1,   6),   S( -1,   1),   S( -6,  -9),   S(  3,  10),   S(  8,  43),   S( -1, -29),
            S( -3,   2),   S(  6,   3),   S( -4,   7),   S(  0,   5),   S( -1,  -5),   S(  4,   9),   S(-11, -22),   S( -3, -22),

            /* kings: bucket 13 */
            S( -1,  53),   S(  7,  32),   S(  0,   0),   S(  0,   0),   S( 43,  17),   S( 13, -12),   S( -1,  -7),   S(-17,  25),
            S(  3,  22),   S( -2,  -1),   S(  0,   0),   S(  0,   0),   S( 46,   6),   S( 27,  -8),   S(-20,   6),   S(-14,   5),
            S( -3,   4),   S( 19,  22),   S(  2,  -7),   S( 14,  40),   S( 51,  13),   S( 22,  -6),   S(  2,   7),   S( 12, -11),
            S(-10,  -6),   S( 15,  -2),   S(  1,  21),   S( -6,  17),   S( -4,  15),   S(  4, -10),   S(  5,  21),   S(-15, -26),
            S(  6,  11),   S( -1,   6),   S(  5,  42),   S( -5,  24),   S( -9,  11),   S(  5,  18),   S(-11,   1),   S(  8,  10),
            S(  3,   0),   S( -5,  17),   S( -2,  17),   S( -5,   0),   S(-12, -18),   S( -6,   9),   S( -9,  20),   S(  1,   0),
            S(  9,  11),   S( -8, -23),   S(-11, -45),   S(  3,  21),   S(-11, -10),   S(-10,  16),   S(-14, -24),   S(  6,  13),
            S(  1,  -2),   S(  6,  -4),   S(  4,  19),   S(  3,   5),   S(  0,  18),   S(-10, -17),   S( -3,   8),   S(  8,  15),

            /* kings: bucket 14 */
            S( 19,  33),   S(  0,  -6),   S( 11, -41),   S( 15,   1),   S(  0,   0),   S(  0,   0),   S(  7,  72),   S(-40,  39),
            S(-10,  -9),   S( 18,  -9),   S( 46, -34),   S( 40,  13),   S(  0,   0),   S(  0,   0),   S( 13,  33),   S(-44,   6),
            S(  3,   4),   S( 15,  -5),   S( 33, -31),   S( 40,   5),   S( 10,  -3),   S( 14,  35),   S( 27,  56),   S(-28,   4),
            S(  8,  -6),   S(  8,  -9),   S( -1, -10),   S( 12,   0),   S(-19,   0),   S( 15,  56),   S(  3,  23),   S(  7,  -3),
            S(  7,  18),   S(  9,   1),   S( -9,   1),   S(-18,  11),   S(  1,  28),   S(  4,  56),   S(  2,  38),   S(  5,  13),
            S( -5,  -6),   S(  1,   5),   S( -3,  -2),   S( -1,  10),   S( -6, -21),   S( -6,  -2),   S(-15,  -6),   S( -1,   6),
            S(  4,   7),   S(-10, -14),   S( 10,  -7),   S( 15,   5),   S(  3,  -2),   S( -6,  18),   S(-26, -21),   S(  8,  17),
            S(  1,  12),   S(  5,  -9),   S(  9,   2),   S( -4,  -6),   S(  7, -10),   S( -3,  -5),   S(-13, -25),   S(  0, -10),

            /* kings: bucket 15 */
            S( 11,  31),   S(  6,  -1),   S( 11,  -7),   S( -8,  -1),   S(-11, -10),   S( -1,  59),   S(  0,   0),   S(  0,   0),
            S( -3, -24),   S(  7, -12),   S( -7, -14),   S( 18,  53),   S( 38,   0),   S( 61, 111),   S(  0,   0),   S(  0,   0),
            S(-10, -23),   S( 17,  -9),   S(  7, -17),   S( -4,  14),   S(  8,  -5),   S( 26,  72),   S(  9,  43),   S(-13,  -3),
            S( -1, -11),   S(  3,  15),   S(  3,  15),   S(-12, -28),   S(-12,  -1),   S( 20,  49),   S( 17,  48),   S( -3, -12),
            S( 10,   5),   S( -8,  25),   S(  0,  -4),   S( -6, -35),   S( -3,   7),   S(  1,  35),   S(  4,   6),   S(  3,   3),
            S(  5,  27),   S(-15,  -4),   S(  8,  15),   S(  8,  19),   S(-10, -23),   S( -2,   7),   S(  1,   6),   S(  4,  16),
            S(  8,  12),   S( -4,  24),   S( -2, -11),   S(  4,   7),   S(  8,   6),   S(  8,  16),   S( -5,  -2),   S(  2,   1),
            S( -2,  -7),   S(  4,   1),   S( -2, -11),   S(  4,   4),   S(  4,   4),   S( 10,  13),   S(  0,  -7),   S(  2,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-41,  58),   S(-17, -16),   S(  3,  67),   S( 16, 102),   S( 26, 124),   S( 34, 149),   S( 43, 152),   S( 56, 145),
            S( 71, 122),

            /* bishop mobility */
            S(-34,  31),   S(-11,  -1),   S(  5,  47),   S( 14,  90),   S( 24, 115),   S( 29, 135),   S( 32, 147),   S( 37, 150),
            S( 41, 151),   S( 50, 145),   S( 60, 134),   S( 83, 122),   S( 95, 116),   S( 72, 115),

            /* rook mobility */
            S(-107,  17),  S(-32,  12),   S(-15,  91),   S(-14, 120),   S(-13, 151),   S( -8, 163),   S( -2, 175),   S(  6, 177),
            S( 12, 189),   S( 19, 193),   S( 23, 200),   S( 32, 199),   S( 45, 201),   S( 54, 203),   S( 98, 175),

            /* queen mobility */
            S( 79, 161),   S(-26, 335),   S( 22, 213),   S( 37, 114),   S( 47, 120),   S( 47, 181),   S( 49, 220),   S( 51, 255),
            S( 52, 287),   S( 53, 312),   S( 56, 329),   S( 58, 342),   S( 59, 351),   S( 59, 365),   S( 60, 367),   S( 61, 369),
            S( 64, 369),   S( 68, 363),   S( 76, 350),   S( 91, 334),   S(101, 317),   S(144, 275),   S(150, 265),   S(174, 232),
            S(186, 216),   S(184, 197),   S(123, 189),   S(111, 142),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  13),   S(-20,  47),   S(-31,  44),   S(-38,  60),   S( 10,  11),   S(-10,  12),   S( -1,  56),   S( 25,  27),
            S( 15,  33),   S( -1,  45),   S(-17,  45),   S(-18,  36),   S( -1,  33),   S(-24,  40),   S(-25,  54),   S( 33,  26),
            S( 24,  68),   S( 13,  72),   S(  6,  54),   S( 21,  45),   S( -1,  49),   S(-23,  62),   S(-29,  94),   S( -4,  75),
            S( 32, 107),   S( 41, 120),   S( 20,  81),   S(  7,  61),   S(  5,  64),   S(  0,  87),   S(-44, 123),   S(-74, 150),
            S( 20, 152),   S( 48, 183),   S( 52, 129),   S( 27, 111),   S(-57, 104),   S( 17, 106),   S(-60, 171),   S(-88, 170),
            S( 88, 231),   S( 78, 268),   S(126, 240),   S(125, 250),   S(130, 262),   S(150, 241),   S(129, 251),   S(131, 263),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,   1),   S( -7, -27),   S( -4, -11),   S(  3,  -8),   S( 14,   9),   S(-15, -37),   S(-24,   9),   S( -1, -49),
            S(-19,  18),   S( 24, -21),   S(  0,  28),   S( 11,  25),   S( 32, -10),   S( -5,  17),   S( 27, -19),   S( -5,  -6),
            S(-12,  16),   S( 15,   6),   S(  4,  41),   S( 16,  51),   S( 25,  28),   S( 34,  16),   S( 29,   1),   S( -1,  14),
            S( 15,  35),   S( 15,  52),   S( 37,  93),   S( 14, 101),   S( 67,  68),   S( 67,  57),   S( 19,  60),   S( 21,  23),
            S( 50,  95),   S( 88, 117),   S(102, 140),   S(140, 165),   S(137, 134),   S(136, 149),   S(130, 125),   S( 50,  61),
            S( 72, 195),   S(117, 279),   S(102, 222),   S( 96, 197),   S( 66, 152),   S( 48, 140),   S( 41, 144),   S( 16,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  21),   S( 16,  21),   S( 30,  34),   S( 31,  21),   S( 19,  19),   S( 23,  24),   S(  4,  11),   S( 39,  -3),
            S( -5,  23),   S( 17,  35),   S( 12,  34),   S(  8,  40),   S( 23,  14),   S( 12,  20),   S( 32,  19),   S(  0,  12),
            S( -1,  22),   S( 28,  50),   S( 53,  57),   S( 39,  58),   S( 44,  54),   S( 71,  19),   S( 29,  35),   S( 20,   6),
            S( 57,  72),   S(104,  57),   S(121, 124),   S(146, 129),   S(138, 120),   S( 77, 132),   S( 71,  57),   S( 71,  10),
            S( 47, 125),   S( 92, 142),   S(151, 213),   S(107, 253),   S(133, 263),   S( 83, 240),   S(158, 207),   S(-56, 172),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26,  33),   S( 11,  18),   S( 13,  33),   S(-14,  63),   S( 65,  22),   S( 20,  10),   S( -1,   1),   S( 30,  12),
            S( -1,  14),   S(  5,   8),   S( 16,  17),   S( 12,  30),   S(  6,  19),   S( -1,   8),   S(  4,   6),   S( 26,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1, -14),   S( -5,  -8),   S(-16, -17),   S(-12, -30),   S( -6, -19),   S(  1,  -8),   S( -4,  -6),   S(-26,   4),
            S(-26, -33),   S(-11, -18),   S(-13, -33),   S( 14, -63),   S(-65, -22),   S(-20, -10),   S(  1,  -1),   S(-30, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -38),   S(-14, -42),   S(-16, -49),   S(-61, -33),   S(-25, -47),   S(-29, -48),   S( -7, -49),   S(-24, -62),
            S(-26, -23),   S(-20, -31),   S(-32, -16),   S( -5, -38),   S(-37, -39),   S(-26, -28),   S(-37, -22),   S(-13, -44),
            S(-20, -20),   S( -8, -36),   S(-24, -14),   S(-31, -26),   S(-20, -44),   S(-20, -24),   S( -9, -24),   S(-41, -32),
            S( -7, -34),   S( 17, -46),   S( 13, -20),   S(  9, -31),   S( 10, -32),   S( 58, -46),   S( 40, -47),   S(-12, -56),
            S( 13, -50),   S( 41, -75),   S( 47, -30),   S( 60, -33),   S( 76, -52),   S( 81, -39),   S(136, -96),   S( 32, -83),
            S( 95, -99),   S(127, -110),  S( 91, -50),   S( 71, -31),   S( 67, -33),   S(121, -47),   S( 98, -47),   S( 45, -87),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-12, -25),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(  4,  -1),        // attacks to squares 1 from king
            S( 10,   2),        // attacks to squares 2 from king

            /* castling available */
            S( 66, -62),        // king-side castling available
            S( 15,  65),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 37, -88),   S( 42, -77),   S( 37, -87),   S( 29, -73),   S( 23, -63),   S( 15, -57),   S(  0, -45),   S( -3, -40),
            S( 11, -44),   S( 28, -44),   S( 54, -41),   S( 23, -25),   S( 99, -48),

            /* orthogonal lines */
            S(-34, -149),  S(-92, -113),  S(-114, -92),  S(-130, -87),  S(-136, -90),  S(-142, -91),  S(-141, -97),  S(-135, -103),
            S(-147, -93),  S(-162, -90),  S(-159, -103), S(-110, -135), S(-82, -146),  S(-39, -153),

            /* pawnless flank */
            S( 39, -35),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 27, 230),

            /* passed pawn can advance */
            S(-12,  34),   S( -4,  61),   S( 14, 103),   S( 82, 170),

            /* blocked passed pawn */
            S(  0,   0),   S( 51, -25),   S( 29,  -4),   S( 24,  33),   S( 24,  62),   S( 18,  35),   S( 61,  65),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 49, -51),   S( 43,  15),   S( 17,  27),   S( 11,  59),   S( 27,  95),   S(127, 127),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-16, -17),   S( -7, -33),   S(  1, -31),   S(-24,  -6),   S(-32,  25),   S(112,  17),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 25, -12),   S( 27, -17),   S(  5,  -5),   S(  4, -40),   S(-15, -115),  S(-42, -203),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 22,  51),   S( 45,  27),   S( 92,  45),   S( 25,  28),   S(160, 118),   S(102, 125),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 14,  54),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-27, 119),

            /* bad bishop pawn */
            S( -6, -15),

            /* rook on open file */
            S( 27,   0),

            /* rook on half-open file */
            S(  4,  39),

            /* pawn shields minor piece */
            S( 13,  11),

            /* bishop on long diagonal */
            S( 26,  51),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 22,  34),   S( 23,   2),   S( 33,  19),   S( 27,  -3),   S( 33, -21),

            /* pawn threats */
            S(  0,   0),   S( 66, 106),   S( 52, 123),   S( 73,  89),   S( 60,  42),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  61),   S( 51,  47),   S( 76,  42),   S( 49,  65),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 26,  52),   S( 29,  43),   S(-15,  41),   S( 66,  68),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 21,  10),   S( 20,  34),   S( 36,  12),   S(  8,  31),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  15),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
