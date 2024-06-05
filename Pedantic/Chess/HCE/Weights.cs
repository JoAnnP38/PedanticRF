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
        public const int MAX_WEIGHTS = 12813;
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
        public const int KING_ATTACK_1 = 12681;     // attacks 1 square from king
        public const int KING_ATTACK_2 = 12690;     // attacks 2 square from king
        public const int CAN_CASTLE_KS = 12699;     // can castle king-side
        public const int CAN_CASTLE_QS = 12700;     // can castle queen-side
        public const int KS_DIAG_MOBILITY = 12701;  // open diagonal line attacks against king
        public const int KS_ORTH_MOBILITY = 12714;  // open orthogonal line attacks against king
        public const int PAWNLESS_FLANK = 12728;    // king is on pawnless flank
        public const int KING_OUTSIDE_PP_SQUARE = 12729;    // king cannot stop promotion
        public const int PP_CAN_ADVANCE = 12730;    // passed pawn can safely advance
        public const int BLOCKED_PASSED_PAWN = 12734;       // blocked passed pawn
        public const int ROOK_BEHIND_PASSER = 12774;// rook behine passed pawn
        public const int BISHOP_PAIR = 12775;       // bonus for having both bishops
        public const int BAD_BISHOP_PAWN = 12776;   // bad bishop pawn
        public const int ROOK_ON_OPEN_FILE = 12777; // rook on open file
        public const int ROOK_ON_HALF_OPEN_FILE = 12778;    // rook on half-open file
        public const int PAWN_SHIELDS_MINOR = 12779;// pawn shields minor piece
        public const int BISHOP_LONG_DIAG = 12780;  // bishop on long diagonal
        public const int MINOR_OUTPOST = 12781;     // minor piece outpost (knight or bishop)
        public const int PAWN_PUSH_THREAT = 12782;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12788;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12794;      // minor piece threat
        public const int ROOK_THREAT = 12800;       // rook threat
        public const int CHECK_THREAT = 12806;      // check threat against enemy king
        public const int TEMPO = 12812;             // tempo bonus for side moving

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

        public Score MinorOutpost
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => weights[MINOR_OUTPOST];
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

        // Solution sample size: 16000000, generated on Tue, 04 Jun 2024 22:11:45 GMT
        // Solution K: 0.003850, error: 0.081965, accuracy: 0.5151
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 75, 224),   S(386, 668),   S(411, 663),   S(547, 1091),  S(1395, 1806), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(106, -124),  S(150, -93),   S( 45, -43),   S(-24,  28),   S(-23,  14),   S(-23,   3),   S(-51,   5),   S(-27, -15),
            S(125, -130),  S(106, -109),  S( 10, -64),   S(-10, -53),   S(-20, -15),   S(-20, -27),   S(-37, -22),   S(-20, -42),
            S(113, -106),  S( 62, -64),   S( 13, -66),   S( 12, -71),   S( -9, -62),   S(  3, -57),   S(-11, -51),   S(  6, -53),
            S( 71, -41),   S( 49, -58),   S( 27, -62),   S( 17, -82),   S(-16, -42),   S(-11, -54),   S(-17, -42),   S( -3, -24),
            S( 75,  37),   S( 31,  -9),   S( 38, -27),   S( 51, -71),   S( 20, -42),   S(-10, -38),   S(-26,  -6),   S(-32,  54),
            S( 65,  58),   S( 49,  78),   S(  5,  10),   S( 18, -16),   S(-41,   0),   S(  7,   5),   S( -6,  23),   S( 13,  51),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 35, -30),   S( 35, -38),   S( 54, -23),   S(  4,  20),   S( -4,  -9),   S(  6, -11),   S(-42,   4),   S(-26,  21),
            S( 38, -45),   S( 25, -47),   S( 13, -48),   S( -2, -43),   S(-13, -22),   S( -7, -29),   S(-35, -13),   S(-33, -10),
            S( 32, -42),   S( 10, -31),   S( 18, -56),   S( 12, -58),   S(-21, -28),   S( 13, -49),   S( -9, -33),   S(  3, -24),
            S( 46, -22),   S( 20, -51),   S( 25, -57),   S(  6, -50),   S(-13, -22),   S( 16, -47),   S(-21, -26),   S( -5,   4),
            S( 30,  45),   S(-34,   3),   S( -5, -36),   S( 10, -48),   S( 35, -35),   S(-10,  -7),   S(-28,  22),   S(-25,  73),
            S( 57,  58),   S( 15,   2),   S(-46, -20),   S(-22,  23),   S(-20,  -7),   S(-57,  26),   S(-47,  31),   S(-41,  90),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9,  -1),   S(-17,   2),   S( -4,  -1),   S(  3,   5),   S( 19, -15),   S( 34, -17),   S(  9, -43),   S(  2, -18),
            S(  0, -27),   S(-26, -15),   S(-18, -34),   S(-14, -34),   S(  8, -34),   S( 11, -33),   S( -4, -39),   S(-13, -29),
            S( -8, -25),   S(-22, -28),   S( -8, -55),   S( -1, -55),   S( -3, -33),   S( 25, -47),   S(  5, -41),   S( 13, -31),
            S(-11,  -8),   S( -8, -49),   S(-12, -53),   S( -2, -57),   S( 11, -48),   S(  8, -32),   S(  7, -26),   S(  7,  -8),
            S( -2,  37),   S(-42,  -6),   S(-38, -41),   S(-45, -32),   S( 13,  -8),   S( -8,   3),   S(-23,  22),   S(-20,  77),
            S(-48,  81),   S(-90,  58),   S(-92,  -4),   S(-70, -17),   S(-40,   6),   S(-18,  19),   S( -5,  -4),   S(-16,  77),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -17),   S(-24,  -3),   S(-21,  -3),   S( 18, -49),   S(  0,  -2),   S( 53, -24),   S( 92, -72),   S( 73, -86),
            S( -5, -44),   S(-26, -31),   S(-18, -44),   S(-15, -28),   S( -6, -29),   S( 19, -43),   S( 61, -76),   S( 67, -80),
            S( -5, -48),   S( -7, -59),   S( -3, -68),   S(  1, -70),   S(  2, -58),   S( 26, -60),   S( 42, -70),   S( 83, -77),
            S( -2, -32),   S(  4, -75),   S(  2, -79),   S(  6, -75),   S( 25, -78),   S( 28, -69),   S( 35, -56),   S( 75, -36),
            S( 25,   9),   S( -8, -34),   S( 12, -75),   S( 14, -68),   S( 89, -68),   S( 72, -41),   S( 56,   5),   S( 55,  61),
            S(-30, 104),   S(-22,  13),   S( -3, -48),   S( -6, -65),   S( 64, -75),   S( 61, -21),   S( 64,   1),   S( 71,  53),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  20),   S(-10, -14),   S(-34,  11),   S( -9,  23),   S( -8, -20),   S(-43,  25),   S(-43,  -1),   S(-40,   5),
            S(-13,   3),   S( 38, -23),   S( 16, -36),   S( 15, -29),   S(-10, -20),   S(-48, -18),   S(  0, -42),   S(  8, -29),
            S( 38, -20),   S( 30, -14),   S(-25,   7),   S( -4, -31),   S(-44, -28),   S(-23, -33),   S(-29, -41),   S( 24, -38),
            S( 14,  25),   S(-12,  32),   S( 30,   1),   S( -3,  -1),   S( 12, -37),   S(-41, -24),   S(  7, -44),   S( 57, -32),
            S(-21,  90),   S(-23,  86),   S(-21,  26),   S(-19,   2),   S(  3,  18),   S(-19,   3),   S(-30, -33),   S( 41,  20),
            S( 66,  76),   S( 52, 101),   S(  8,  36),   S( 18,  21),   S( 13, -14),   S(  2, -11),   S(  7,   1),   S(-14,  15),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-95,  55),   S(-75,  43),   S( -9,  11),   S( -7,  13),   S(-15,  29),   S(-29,  19),   S(-44,  14),   S(-22,  28),
            S(-54,  17),   S(-64,  19),   S( 18, -15),   S( 18,   1),   S(  8,  -9),   S(-22, -17),   S(-29,  -8),   S(-24,  10),
            S(-52,  35),   S(-66,  30),   S( 35, -26),   S( -9, -22),   S( 16, -15),   S(-25, -20),   S(-17,  -9),   S( 14,  -8),
            S(-53,  54),   S(-55,  33),   S( -2,   1),   S( 23,   5),   S(-20,   6),   S(-49,  -4),   S( -1, -12),   S( 14,  14),
            S( 33,  59),   S( 30,  35),   S( 27,  40),   S( 28,  19),   S(-12,  32),   S( 63,  -9),   S( 12,   9),   S( 52,  27),
            S( 62,  44),   S( 57,  17),   S( 39,  -6),   S( 37,  -4),   S( 44, -14),   S( 21,  -5),   S( 11,   8),   S(  7,  55),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-46,  29),   S(-35,  19),   S(-32,  16),   S(-26,  15),   S( 29, -21),   S(-32,  10),   S(-63,   6),   S(-55,  19),
            S(-32,   0),   S(-12, -20),   S(-22, -31),   S( -7,  -8),   S( 27, -17),   S( 11, -23),   S(-42,  -7),   S(-68,   8),
            S(-17,  -6),   S(-22,  -8),   S(-22, -22),   S(-41,  -4),   S( -6,  -8),   S( 47, -38),   S(-17, -15),   S(-17,   5),
            S(-29,  18),   S(-74,   9),   S(  6, -32),   S(-16,  -9),   S( 12,  -2),   S( 38, -18),   S( 18, -10),   S( 44,   2),
            S( 11,  24),   S(-46,  13),   S( 15, -30),   S( -5, -13),   S( 43,  23),   S( 70,  18),   S( 33,   8),   S( 70,  27),
            S( 63,  27),   S( 24,   0),   S(  6, -37),   S( 10, -38),   S( 21,  -2),   S( 24,   3),   S( 42, -11),   S( 45,  34),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-41, -22),   S(-43, -15),   S(-24,  -5),   S(-48,  13),   S(-11, -23),   S( 25, -22),   S( -8, -48),   S(-39, -27),
            S(-34, -42),   S(-34, -44),   S(-41, -38),   S(-21, -44),   S(-12, -37),   S( 43, -55),   S( 45, -60),   S( -8, -36),
            S(-37, -44),   S(-55, -39),   S(-43, -47),   S(-22, -43),   S(-21, -26),   S( 25, -38),   S( 31, -56),   S( 49, -47),
            S(-15, -44),   S(-48, -52),   S(-78, -44),   S(-51, -24),   S(-11, -26),   S( 19, -21),   S( 24, -19),   S( 78, -30),
            S( 11, -34),   S(  9, -60),   S(-20, -52),   S(  1, -65),   S( 18,  -4),   S( 27,  -2),   S( 60,  41),   S(102,  32),
            S(-10,   4),   S(-26, -32),   S(  5, -50),   S( -4, -52),   S( -2, -16),   S( 25, -23),   S( 46,  38),   S( 88,  57),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-55,  71),   S(-36,  59),   S( 16,  25),   S(-10,  -2),   S( 13,   9),   S(  3,   7),   S(-37,   5),   S(-42,  26),
            S(-60,  61),   S(-59,  54),   S(-33,  41),   S(-16,  11),   S(-15,  -8),   S(-38, -14),   S(-52,  -7),   S(  1,  -5),
            S(-60,  97),   S(-13, 100),   S(-12,  61),   S(-32,  34),   S( 12, -13),   S(-102,  -4),  S(-74, -17),   S(-42,  -4),
            S(-29, 139),   S(  3, 150),   S(  8, 108),   S( 12,  48),   S(-34,  17),   S(-34, -20),   S(-30,  -5),   S(-52,  10),
            S(-12, 168),   S( 41, 156),   S( 26, 162),   S( 55, 100),   S( 19,  11),   S(  1,   3),   S(-18, -15),   S( -6,  19),
            S( 54, 191),   S( 70, 209),   S( 86, 201),   S( 48,  73),   S(  7,  35),   S(-11,   5),   S(-10, -24),   S(  2,  13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-93,  77),   S(-61,  51),   S( 14,   9),   S( 12,  30),   S( 12,   8),   S(-35,  14),   S(-73,  18),   S(-72,  32),
            S(-54,  38),   S(-56,  33),   S(-43,  28),   S(  6,  43),   S(-50,   1),   S(-25, -11),   S(-74,  -3),   S(-28,  11),
            S(-88,  69),   S(-114,  98),  S(-50,  77),   S(-108,  89),  S(-67,  54),   S(-93,  10),   S(-52, -17),   S(-46,   7),
            S(-67, 109),   S(-36, 117),   S(  2, 121),   S( 41, 125),   S(-30,  59),   S(-45,  14),   S(  8,   3),   S(-49,  24),
            S( 18, 124),   S( 23, 144),   S( 23, 156),   S( 46, 172),   S( 20, 130),   S( -4,  35),   S( -2,   1),   S( -2,   4),
            S( 22,  73),   S( 22, 123),   S( 64, 138),   S( 70, 179),   S( 29, 109),   S( -8,  -6),   S(-13,  -6),   S(-18, -17),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-86,  15),   S(-56,   0),   S( -5,  -2),   S(  3,  20),   S( -9,   3),   S(-45,  25),   S(-100,  27),  S(-53,  33),
            S(-97,   8),   S(-80,   7),   S( -9, -19),   S(-22, -10),   S(-20,  21),   S(-38,  22),   S(-121,  33),  S(-80,  18),
            S(-26, -10),   S(-87,  16),   S(-31,   1),   S(-81,  67),   S(-84,  83),   S(-26,  42),   S(-119,  48),  S(-84,  43),
            S(-101,  35),  S(-79,  29),   S(-10,  10),   S(-43,  79),   S( 17,  97),   S(-57,  81),   S(-32,  50),   S(  3,  27),
            S(-25,  47),   S(-35,  21),   S(  8,  50),   S( 27, 124),   S(102, 110),   S( 53,  65),   S(-13,  89),   S( 33,  45),
            S(  0,  16),   S(-21,  -2),   S( 20,  18),   S( 49, 116),   S( 11, 129),   S( 26,  56),   S( -6,  72),   S( 23,  98),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-68,  -3),   S(-72,  19),   S( 44, -16),   S( -2,  18),   S(  2,  36),   S(-75,  53),   S(-46,  35),   S(-60,  42),
            S(-67, -20),   S(-82, -19),   S(-34, -37),   S(-50,  16),   S(-36,  11),   S(-32,  27),   S(-94,  61),   S(-94,  45),
            S(-40, -31),   S(-62, -32),   S(-59,  -3),   S(-35,   9),   S(-57,  36),   S(-21,  59),   S(-91,  85),   S(-45,  67),
            S(-56,   8),   S(-92, -12),   S(-29, -28),   S(-56,  19),   S(  6,  43),   S(-14,  75),   S( 12, 113),   S( 79,  73),
            S(-20,  25),   S(-45,  -6),   S( -7,  -1),   S( -8,  24),   S( 59,  95),   S( -6, 126),   S( 97, 119),   S( 92, 103),
            S(-32,  46),   S(-19,   5),   S(  9, -19),   S(  2,   2),   S( 20,  69),   S( 31, 152),   S( 65, 181),   S( 35, 173),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13,  12),   S(-16,  10),   S(-18,   1),   S(  2,   5),   S( -3,  -9),   S( -9,  16),   S(-14, -19),   S(-17,  -4),
            S(-39, -26),   S( -7,  18),   S(  9,  20),   S( -1,   3),   S( -1,  33),   S( -8, -11),   S(-37, -32),   S(-28, -42),
            S(-17,  38),   S(-39,  95),   S( 18,  65),   S( 17,  39),   S(-17,   1),   S(-49, -15),   S(-48, -49),   S(-47, -58),
            S(-45,  91),   S(-47, 122),   S( 39, 116),   S( 23,  97),   S(-21, -29),   S(-41, -34),   S(-11, -15),   S(-60, -50),
            S( 33,  95),   S( 39, 211),   S( 47, 151),   S( 18,  56),   S( -1,  16),   S( -3, -20),   S( -1,   3),   S(-20, -44),
            S( 45, 108),   S( 55, 217),   S(117, 222),   S( 46, 100),   S( -6,   6),   S( -9,  -7),   S(-11, -32),   S(-22, -39),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -17),   S(-19,  14),   S( -5,   8),   S( -2,   4),   S( -9, -10),   S(-29,   4),   S(-35, -41),   S(-22,  -6),
            S(-40,  -9),   S(-57,  46),   S(-25,  33),   S( 19,  21),   S(-46,  24),   S(-15, -14),   S(-82, -24),   S(-61,  10),
            S(-58,  48),   S(-52,  50),   S(-38,  77),   S(-13,  94),   S(  0,  34),   S(-43, -30),   S(-68, -28),   S(-79, -25),
            S(-76,  92),   S( -9, 120),   S( -5, 139),   S(  6, 123),   S( -2,  65),   S(-46,  26),   S(-20, -11),   S(-41, -38),
            S(  2,  98),   S( 55, 169),   S( 67, 196),   S( 47, 248),   S( 22, 150),   S(-10,  16),   S( -4, -63),   S(-25, -34),
            S( 41,  71),   S( 74, 171),   S( 84, 193),   S( 84, 251),   S( 39, 108),   S(  3,  11),   S(  0,   2),   S( -5,  -1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -54),   S(-35, -20),   S( -8, -27),   S( -2,  -1),   S( -5,  -1),   S(-31,   9),   S(-34,  -2),   S( -2,  47),
            S(-53,  13),   S(-57,   9),   S(-55, -29),   S( -2,  10),   S(-40,  62),   S(-17,  17),   S(-43,  18),   S(-56,  13),
            S(-62, -22),   S(-62,   6),   S(-38, -19),   S(-23,  42),   S(-21,  70),   S(-55,  36),   S(-37,   4),   S(-66,  44),
            S(-51,  15),   S(-24,  54),   S(-28,  30),   S(  9,  98),   S( -3, 134),   S(-29,  84),   S(-38,  43),   S(-36,  60),
            S(-20, -21),   S( 11,  18),   S( 13,  78),   S( 35, 135),   S( 47, 215),   S( 44, 171),   S( 10,  82),   S( 27,  42),
            S( -2,  24),   S( 19,  38),   S( 30, 114),   S( 35, 139),   S( 64, 213),   S( 57, 115),   S( 29,  95),   S( 21,  30),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-42, -30),   S(-30, -20),   S(-11, -32),   S(  0,  -3),   S( 17,  22),   S(  2,  47),   S(-11, -25),   S(  9,  20),
            S(-43, -32),   S(-35, -15),   S(-14, -42),   S( 22,  -7),   S(-15,   0),   S(  6,  45),   S(  3,  29),   S( -1,  -3),
            S(-20, -72),   S(-34, -58),   S(-21, -51),   S(  1,  -9),   S( 10,  33),   S(-17,  58),   S( -4,  69),   S(-21,  63),
            S(-27, -22),   S(-45, -30),   S(-32,   2),   S( 10,  20),   S(-12,  52),   S(  5,  93),   S(-27, 138),   S( -7,  55),
            S(-27, -41),   S(-31, -30),   S(-13,  16),   S(  0,   2),   S( 35, 115),   S( 67, 164),   S( 59, 220),   S( 74,  69),
            S( -8,   6),   S( -3,  10),   S(  2,   9),   S(  7,  24),   S( 26,  81),   S( 84, 191),   S( 31, 179),   S( 42,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-30,   6),   S(  3,  13),   S(-43,  17),   S(-19,  -5),   S(-27,  -3),   S(  4, -29),   S(-42, -46),   S(-34, -16),
            S(-35,  56),   S( 24, -39),   S(-40,  17),   S( 11, -17),   S(-10, -15),   S(-22, -14),   S(-27, -23),   S(-70, -22),
            S(  5,  67),   S( -1,  -6),   S(  7,  -6),   S(-23,  37),   S( 11,  12),   S(-32,   4),   S(-11, -26),   S(-40, -47),
            S( 19, -24),   S( 41,   5),   S( 12,  20),   S( 28,  23),   S(  3,   2),   S( -3,   1),   S(-12, -16),   S( -3,  -7),
            S( 19, -31),   S( 34,   9),   S( 12,   8),   S( 65, -10),   S( 41,  -8),   S( 26,  18),   S( 19, -17),   S(-60, -15),
            S( 18, -14),   S( 11,   7),   S( 25,  13),   S( 53, -15),   S( 32, -44),   S( 16,  10),   S(  8, -25),   S( -5, -13),
            S( 14, -29),   S( 15, -39),   S( 17, -24),   S( 33, -29),   S( 20, -18),   S(-10, -26),   S( -9, -43),   S(-22, -33),
            S(-69, -55),   S( -7,   1),   S( -5, -17),   S(  0, -44),   S(-21, -22),   S( 20,   8),   S( -7,   3),   S( 15,  -3),

            /* knights: bucket 1 */
            S(-43,  24),   S(-53,  89),   S( 28,  40),   S(-24,  68),   S(-17,  52),   S(-21,  30),   S(-32,  53),   S(-18, -12),
            S( 35,  24),   S( -3,  37),   S( -4,  29),   S( -5,  45),   S( -8,  31),   S(-14,  18),   S( 19, -14),   S(-25,  12),
            S(-30,  28),   S( 15,  13),   S(  2,  15),   S( 15,  31),   S(  6,  34),   S(-27,  32),   S(-18,   7),   S(-30,  22),
            S(  1,  35),   S( 53,  22),   S( 19,  39),   S( 25,  18),   S(  7,  27),   S( -5,  24),   S( 16,   8),   S( 14,  12),
            S(  2,  46),   S( 17,  23),   S( 27,  24),   S( 37,  23),   S( 34,  24),   S( 29,  18),   S( 23,  13),   S( 18,  15),
            S(  8,  16),   S( 17,   9),   S( 14,  31),   S( 43,  14),   S( 10,  22),   S( 32,  31),   S( 25,   4),   S( 14, -10),
            S( 34,   4),   S( 26,  16),   S(-20, -18),   S( 14,  34),   S( 28,  -4),   S( 24,  -3),   S(-32,  11),   S(-10, -21),
            S(-100, -66),  S(-22, -13),   S( -4,  16),   S(  2,  27),   S(-13,   3),   S(-25, -21),   S( -4,  -5),   S(-38, -39),

            /* knights: bucket 2 */
            S(-56,   6),   S(  1,  21),   S(-32,  56),   S(-23,  59),   S(-32,  60),   S(-36,  75),   S(-17,  30),   S(-19,  12),
            S(-14, -17),   S(-17,  12),   S(-12,  20),   S(-12,  37),   S( -5,  26),   S(-14,  55),   S(-33,  57),   S(-39,  66),
            S(-17,  24),   S( -5,  12),   S(-10,  32),   S( 14,  25),   S(  2,  37),   S(  6,  14),   S( -8,  44),   S(-25,  29),
            S( -9,  35),   S(-26,  38),   S(  4,  34),   S(  1,  44),   S( -3,  40),   S( -9,  31),   S(  3,  35),   S( -4,  38),
            S( 16,  21),   S(-16,  31),   S( -6,  43),   S(-18,  50),   S(  0,  45),   S( -9,  39),   S(  2,  30),   S( -1,  20),
            S(-25,  32),   S(  2,  30),   S(-26,  51),   S(-19,  49),   S(-27,  45),   S( -2,  27),   S(-30,   9),   S( 15,   1),
            S(-22,  24),   S(-32,  17),   S(-33,  21),   S(-38,  38),   S(-15,  18),   S(  2,  24),   S(-51,  36),   S(-35,  11),
            S(-144,  18),  S( -3,  -1),   S(-80,  33),   S(-28,  12),   S( -4,  11),   S(-59,   1),   S( -3,   0),   S(-177, -55),

            /* knights: bucket 3 */
            S(-48, -12),   S( 16, -30),   S(-20,  -4),   S( 10,  -2),   S( 13,  -8),   S(-11,  11),   S( 25, -16),   S( -4, -25),
            S(-10,  -1),   S(-22,  -9),   S(-15,  -6),   S( 10,  11),   S( 23,  -2),   S(  1,  -7),   S( -1,  -9),   S(-16,  57),
            S(  3, -32),   S(  6,  -2),   S(  5,   2),   S( 18,  10),   S( 21,  25),   S( 26,   7),   S( 17,   3),   S( 14,  32),
            S(  2,  -3),   S( 12,   8),   S( 17,  28),   S( 22,  25),   S( 31,  31),   S( 26,  24),   S( 31,  14),   S( 26,  10),
            S( 28,   2),   S(  7,  14),   S( 36,   6),   S( 31,  36),   S( 29,  35),   S( 36,  41),   S( 42,  32),   S( 21,  11),
            S(  4,   7),   S( 31, -15),   S( 46,  -2),   S( 59,   3),   S( 70, -16),   S( 73,  -8),   S( 14,   6),   S( 13,  38),
            S( 28,  -7),   S( 15,   6),   S( 43, -20),   S( 51,  -7),   S( 66, -31),   S( 60, -34),   S( 63, -66),   S( 46, -22),
            S(-104,   8),  S(-24,   7),   S(-28,   3),   S(  2,  17),   S( 31,  -7),   S( -8, -12),   S(-11, -22),   S(-72, -48),

            /* knights: bucket 4 */
            S( 13,  17),   S(-46,   7),   S( 14,  27),   S( -1,  -5),   S(-19, -10),   S(-29, -22),   S( -8, -51),   S(-30, -45),
            S( 33,  23),   S(-21,  38),   S( 17, -22),   S( 14,  -5),   S( 22, -14),   S( -4, -40),   S( 12,  -2),   S(  1, -47),
            S(-10,  29),   S(  9,  39),   S( 10,  10),   S( 21,  16),   S( -4,   4),   S(-44,  18),   S(-47, -29),   S(-31, -55),
            S( -2,  65),   S( 33, -22),   S( 44,  26),   S( 22,  19),   S( 13,  13),   S( 92, -19),   S( 27, -31),   S( -1, -20),
            S( 56,  29),   S(-20,  43),   S( 40,  46),   S( 40,  19),   S( 39,  34),   S(-15,  26),   S( -6, -30),   S( -9,  -9),
            S(  7,  16),   S(-34,  -4),   S( 77,  14),   S(  6,   8),   S( 11,  20),   S( 21,  21),   S( 13,  29),   S(-11, -21),
            S( -6,   7),   S(-16,   8),   S( 13,   1),   S(  5,  37),   S(  9,  11),   S(  6, -15),   S(  4,  -8),   S(-14,  -3),
            S(-10,  -7),   S( -2,  -6),   S( 10,  10),   S(  1,   4),   S( -6,  -8),   S(  9,  21),   S( -2,   5),   S( -3, -19),

            /* knights: bucket 5 */
            S( 12,  -2),   S(-37,  46),   S( 31,  36),   S( 23,  49),   S( 37,  26),   S( 14,   2),   S(  3,  18),   S(-21, -21),
            S( 12,   0),   S( 32,  47),   S( 22,  25),   S( -7,  44),   S( 36,  38),   S(  5,  37),   S( 23,  27),   S(-14, -26),
            S(  5,  27),   S( -9,  41),   S( 61,  24),   S( 37,  47),   S(-16,  54),   S( -2,  31),   S(-18,  19),   S(  8,  -3),
            S( 31,  45),   S( 10,  50),   S( 31,  44),   S( -6,  63),   S( 12,  52),   S(  3,  43),   S( 24,  41),   S( 10,  35),
            S( 19,  51),   S( 26,  36),   S( 40,  52),   S( 52,  47),   S( 73,  48),   S( 23,  44),   S( 41,  33),   S( 35,  31),
            S(  5,  30),   S( -6,  50),   S( 17,  30),   S(  9,  56),   S( 34,  45),   S( 14,  52),   S( 18,  15),   S( -5,  31),
            S( 20,  56),   S( -5,  65),   S( 32,  48),   S( 15,  66),   S(  6,  55),   S(  9,  48),   S( 23,  67),   S(  4,   1),
            S( -4,   7),   S(  0,  14),   S(  9,  41),   S( -3,   7),   S( 10,  42),   S(  2,  35),   S(  8,  39),   S(-17, -17),

            /* knights: bucket 6 */
            S(  2, -41),   S(-16,  -4),   S( 32,  29),   S(-22,  41),   S(-24,  49),   S( 20,  40),   S( -8,  35),   S(-12,  24),
            S( -4, -30),   S( 53,   0),   S( 18,  13),   S(-29,  41),   S(-57,  71),   S( 29,  52),   S( 21,  50),   S( -1,   8),
            S(-21, -18),   S(  1,   3),   S( -7,  28),   S( 20,  37),   S(-21,  65),   S(-36,  61),   S( 13,  48),   S(  1,  44),
            S( 33,   4),   S( 32,  12),   S( 38,  31),   S( 70,  30),   S( 16,  54),   S(  6,  56),   S(  9,  62),   S(-24,  69),
            S(  4,  34),   S( 63,  -7),   S( 50,  36),   S( 62,  35),   S( 73,  40),   S( 73,  38),   S( 14,  56),   S( 19,  51),
            S( 23,  26),   S( 10,  15),   S( 63,  23),   S( 43,  45),   S( 51,  49),   S( 28,  31),   S( 16,  40),   S( 36,  38),
            S(-22,  21),   S(  3,  35),   S(-24,  38),   S( 30,  33),   S(  3,  61),   S( 22,  43),   S( 21,  71),   S( -7,  29),
            S(-40,   0),   S( 16,  41),   S( 28,  38),   S( 11,  40),   S( 22,  35),   S( 10,  59),   S( 20,  57),   S( 11,  23),

            /* knights: bucket 7 */
            S(-34, -56),   S(-189, -43),  S(-68, -44),   S(-56, -15),   S(-38,  -8),   S(-29, -16),   S( -9,   3),   S(-17,   5),
            S(-49, -76),   S(-36, -46),   S(-35, -30),   S(-47,   6),   S(-43,  12),   S(  6, -11),   S(-14,  47),   S(  5,  28),
            S(-79, -65),   S(-55, -35),   S(-49,   2),   S( 20, -15),   S(-18,  11),   S(  2,  13),   S(-13,  58),   S( 47,  54),
            S(-61, -24),   S(  9, -24),   S(-13,  11),   S( 27,  -2),   S( 38,  -1),   S(  7,  17),   S(  9,  14),   S(-22,  34),
            S(-60, -23),   S(-22, -29),   S( 46, -22),   S( 72, -15),   S( 97,  -4),   S( 55,  23),   S( 83,  -1),   S( 71,  19),
            S( -6, -40),   S( 16, -39),   S(-23,   0),   S( 25,   1),   S( 58,  12),   S( 68,   6),   S( 48, -19),   S( -7,   8),
            S(-33, -33),   S(-64, -19),   S(  5, -13),   S( 32,  18),   S( 34,  22),   S( 39,   1),   S(-20,  23),   S(  1,   5),
            S(-37, -28),   S( -9,  -9),   S(-27, -14),   S(  9,  13),   S( 10,   4),   S( 22,  18),   S( -4, -11),   S( -4,  -8),

            /* knights: bucket 8 */
            S( -1,  -8),   S( -8,  -9),   S( -4,  -4),   S( -9, -30),   S(-10, -40),   S(-10, -52),   S( -1,  -1),   S( -5, -23),
            S(  2,   0),   S( -6, -11),   S( -7, -29),   S(-17, -43),   S(-27, -26),   S(-17, -70),   S(-13, -58),   S(-16, -36),
            S(  4,  17),   S(-20, -18),   S( 24,   8),   S(  5,   0),   S(  3, -30),   S(-14, -11),   S(-12, -36),   S( -8, -42),
            S(-18,  -1),   S(  0,  -2),   S( -3,  15),   S(  4,  34),   S(  8,  -2),   S(  6,   7),   S(-15, -53),   S( -3, -17),
            S( 26,  53),   S( 10,   9),   S( 13,  35),   S( 34,  18),   S( 10,  30),   S( -5,  -5),   S(  4, -21),   S( -7,  -9),
            S( 13,  36),   S(  8,   5),   S( 27,  24),   S( 30,  15),   S(  2,   0),   S( -1,  -7),   S( -7, -29),   S( -6,  -9),
            S(  3,  12),   S(  1,   3),   S(  6,  11),   S( 11,  10),   S(  7,   9),   S(  5,  21),   S(  2,  12),   S( -1,   2),
            S(  1,   0),   S( 11,  32),   S(  5,  15),   S( -2,   0),   S(  3,  11),   S( -5, -20),   S(  3,   5),   S( -3,  -4),

            /* knights: bucket 9 */
            S(-10, -30),   S(-20, -36),   S(-18, -47),   S( -3, -14),   S(-22, -53),   S(-15, -39),   S( -3, -14),   S( -4, -28),
            S(-12, -39),   S(-12,  -2),   S(-10, -50),   S(-11,  -7),   S( -3, -13),   S( -7, -33),   S( -5,  -2),   S(-15, -42),
            S(  5,   6),   S( -9, -14),   S(  6, -14),   S(  5,   6),   S(  4,  20),   S(-30,   1),   S(-11, -10),   S( -8, -17),
            S(-14,  -3),   S( -6,  -8),   S(  4,  32),   S( 16,  35),   S( 27,  26),   S(  9,  23),   S(-12, -36),   S( -3,  -2),
            S(  0,  21),   S( 20,   8),   S( 17,  42),   S( -1,  45),   S(  7,  18),   S( 11,  -5),   S(  1, -30),   S(  5,   9),
            S(  1,   0),   S(  8,  33),   S( 14,  35),   S(-11,  21),   S( 34,  39),   S( 15,  10),   S(  7,  12),   S( -7, -24),
            S(  1,   0),   S( -1,  20),   S( 19,  38),   S( 11,   5),   S( 14,  42),   S( -1, -16),   S(  4,  17),   S( -2,  -1),
            S(  1,   0),   S(  4,   8),   S( 12,  27),   S( 15,  30),   S(  9,  10),   S(  0,   4),   S(  3,   4),   S(  0,  -4),

            /* knights: bucket 10 */
            S(-18, -50),   S(-16, -54),   S(-12, -26),   S(-17, -21),   S(-12, -12),   S(-14, -43),   S( -3,  15),   S(  4,  20),
            S( -6, -24),   S( -6, -14),   S(  1, -17),   S(-18, -33),   S(-22, -36),   S( -8, -40),   S( -8,  -8),   S( -5, -13),
            S(-16, -51),   S(-17, -61),   S( -6, -10),   S(-13, -13),   S( 15,   4),   S(-11,  -1),   S( -5,   4),   S( -7,   5),
            S( -8, -19),   S( -6, -45),   S(  3, -33),   S( 17,  16),   S(  7,  41),   S( 15,  26),   S(  6,  18),   S( 11,  42),
            S( -7, -46),   S(-13, -31),   S( 15,  11),   S( 21,  34),   S( 16,  54),   S( -2,  30),   S( 18,  13),   S( 23,  51),
            S(-11, -40),   S( -5, -22),   S( -5,  -9),   S( 12,  45),   S( 33,  65),   S( 30,  43),   S( 27,  58),   S( 17,  52),
            S( -1,  -3),   S(-10, -32),   S(  1,  -7),   S( 27,  27),   S( 19,  30),   S(  9,  33),   S(  1,  -3),   S(  9,  25),
            S( -3, -17),   S(  3,  11),   S( -7, -19),   S(  4,  -4),   S( 12,  37),   S(  5,  25),   S(  2,  12),   S( -1,  -3),

            /* knights: bucket 11 */
            S( -1,  -1),   S(-19, -29),   S( -7, -43),   S( -9, -25),   S(-20, -49),   S(-12, -18),   S( -6,  -5),   S( -4,  -7),
            S( -7,  -9),   S(-12, -20),   S(-14, -76),   S(-27, -23),   S( -8,  -1),   S(-29, -37),   S(-16, -30),   S( -8, -10),
            S(-14, -53),   S(-22, -61),   S(-23, -32),   S(  1,   5),   S(-14,   7),   S(-16,  20),   S( 10,  -3),   S(  1,  15),
            S(-13, -31),   S( -8, -31),   S(-26,  -6),   S( 26,  31),   S( 15,  20),   S( 15,  10),   S( 12,  26),   S( 15,  29),
            S( -3, -24),   S(-20, -58),   S(  6, -19),   S(  0,   8),   S( 13,  22),   S( 30,  54),   S(  5,  -3),   S( 24,  66),
            S( -7, -11),   S( -6, -26),   S(  0,  -5),   S( 38,  35),   S( 17,  22),   S( 49,  49),   S( 21,  24),   S( 13,  27),
            S(  9,  27),   S( -2,  -6),   S(  8, -10),   S( 13, -15),   S( 21,  32),   S(  1,   7),   S( 16,  39),   S( 20,  53),
            S( -4,   0),   S( -2, -18),   S(  8,  11),   S(  1,   5),   S(  2,  12),   S(  3,   3),   S(  4,   5),   S(  2,  11),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   2),   S( -2, -20),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   5),   S( -2, -14),
            S(  0,   0),   S(  1,   2),   S(  2,   6),   S( -3, -11),   S( -1,   7),   S( -4, -20),   S( -2, -11),   S(  1,   8),
            S( -5, -14),   S(  5,   5),   S( -5, -11),   S( -5, -21),   S(  0,   4),   S( -5, -17),   S(  2,  -4),   S( -7, -30),
            S( -7, -13),   S( -1,   1),   S( -8, -22),   S(  5,  15),   S( -5,  -3),   S(  1,   6),   S( -2,  -7),   S( -1, -10),
            S(  9,  17),   S(  4,   3),   S( -6, -12),   S(  0,   3),   S( -5, -26),   S(  0,   2),   S( -1, -13),   S( -1,   1),
            S(  1,  -9),   S( -4, -24),   S(  2,   1),   S( -2,  -5),   S(  4,  10),   S( -5, -18),   S( -1,  -7),   S(  0,   2),
            S(  2,   6),   S( -8, -11),   S( -1,  10),   S(  1,  -8),   S( -5,  -7),   S( -5, -21),   S( -2,  -1),   S(  0,  -1),
            S(  2,   3),   S(  2,  13),   S( -2,  -3),   S(  2,  -2),   S( -2,  -4),   S( -2,  -9),   S( -3,  -9),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -1,  -6),   S( -2,  -2),   S( -8, -13),   S( -1,   0),   S( -3, -11),   S(  1,  -2),
            S( -2,  -7),   S(  1,   6),   S( -2, -23),   S(-10, -21),   S( -6, -30),   S( -4, -25),   S(  0,   1),   S(  1,  -1),
            S( -4, -10),   S( -8, -31),   S(  7,  18),   S(  0,   0),   S(-12, -37),   S( -9, -23),   S( -2, -13),   S( -6, -28),
            S( -8, -15),   S(  5,  13),   S(  1,   2),   S(-11, -25),   S( -2,  -7),   S(  5,  12),   S(  0, -13),   S( -5, -11),
            S(  3,  10),   S( -2,  -2),   S(  2,  -8),   S( 11,  22),   S(  4, -11),   S( -4,  -8),   S(  2, -14),   S(  1,   1),
            S( -3,  -9),   S( 14,  14),   S(  6,  23),   S(-14,  11),   S(  4,   5),   S(-10, -34),   S(  4,   5),   S( -4,   2),
            S(  1,   6),   S(  2,   5),   S(  9,  12),   S(  8,  12),   S( 14,  23),   S( -4, -20),   S( -2,  -1),   S( -5,  -3),
            S( -1,   1),   S( -1,  -6),   S( -1,   1),   S(  2,  -9),   S( -1,   0),   S(  3,  -1),   S(  0,  -2),   S( -1,   0),

            /* knights: bucket 14 */
            S( -3, -24),   S( -5, -25),   S( -2,  -2),   S( -3,   3),   S( -8, -24),   S( -2, -14),   S( -1,  -5),   S(  0,   2),
            S(  0,  -2),   S( -3, -10),   S(-15, -60),   S( -8, -36),   S( -1,  -9),   S(  1,   5),   S(  1,  -3),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-10, -53),   S(  1,   3),   S( -4, -20),   S( -4,  -9),   S(  0,  -1),   S(  1,   9),
            S(  1,   5),   S( -6, -32),   S(-15, -40),   S(-12, -36),   S( -2, -21),   S(  1,  -2),   S( -3, -16),   S( -7, -11),
            S( -2,  -4),   S( -2, -17),   S(  1,  23),   S( -7, -31),   S( -9,  -7),   S(  2,  22),   S(  3,   5),   S( -3,  -7),
            S( -4,  -8),   S(  3,  -3),   S( -9, -30),   S(  4,   2),   S( 14,  24),   S(  3,   8),   S( -3,  -1),   S(  0,  -4),
            S(  0,  -3),   S( -2, -10),   S(  7,  -3),   S(  0,  -7),   S( -7,  -9),   S( -2,  -8),   S( -5,  -1),   S(  1,   7),
            S( -1,  -2),   S(  2,   4),   S( -1, -10),   S(  7,  -1),   S(  5,  18),   S(  1,   3),   S( -2,  -8),   S( -1,  -5),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -1, -14),   S( -1, -13),   S( -7, -13),   S( -3,  -1),   S( -2,  -5),   S(  1,   0),   S(  0,  14),
            S( -2,  -6),   S(  0,  -2),   S( -4, -18),   S( -6, -26),   S( -2,  -4),   S( -1,  -8),   S(  0,   0),   S( -1,  -3),
            S( -6, -16),   S( -7, -16),   S( -3, -11),   S(-14, -38),   S( -5, -24),   S( -1,  -3),   S( -1,  -1),   S( -2,   0),
            S( -6, -17),   S( -6, -32),   S( -7, -20),   S(  0,  -8),   S(  0, -18),   S(  7,  23),   S(  5,  10),   S( -4,  -2),
            S(  0,  -2),   S( -2,  -5),   S( -1, -16),   S( -8, -13),   S(  3,  18),   S(  3,  10),   S( -6,  -7),   S( -2,   2),
            S( -3,  -4),   S( -2,  -5),   S( -2, -21),   S( -3,   7),   S( -5, -13),   S( -6,  12),   S( -3,   4),   S(  2,   8),
            S( -3, -13),   S( -1,  -6),   S( -1,  -8),   S( -4,  -8),   S(-10, -13),   S( -4,  16),   S( -2,  -7),   S(  3,  13),
            S(  0,  -3),   S(  0,  -1),   S( -4,  -9),   S( -2,  -9),   S( -2,  -5),   S( -9,  -6),   S(  7,  17),   S( -2,   1),

            /* bishops: bucket 0 */
            S( 15,  19),   S( 23, -10),   S( 42,  20),   S(  8,  25),   S( -3,   0),   S( 18,  -2),   S( 25, -37),   S(  1, -35),
            S( 46, -43),   S( 78,   5),   S( 36,  12),   S( 17,   7),   S(-14,  38),   S(  0, -17),   S(-37,   1),   S( 10, -47),
            S( 26,  42),   S( 51,  10),   S( 29,   7),   S( 13,  58),   S( 20,  15),   S(-31,  27),   S(  9, -20),   S( 11, -39),
            S( 18,  11),   S( 64, -13),   S( 38,   8),   S( 38,  29),   S(  4,  28),   S( 28,   0),   S( -7, -11),   S(  0,   0),
            S( 17,   4),   S( 31,  21),   S(  5,  38),   S( 56,  12),   S( 60,  -1),   S( 17,  -7),   S( 20, -21),   S(-34,  -3),
            S(-37,  63),   S(  0,  18),   S( 57, -21),   S( 86, -23),   S( 36,  31),   S( -7,  -1),   S(  2,  10),   S(  1,  11),
            S(-13,  14),   S( 10,   1),   S( 42,  -7),   S(  1,  37),   S(-30,  -2),   S( 23,  28),   S(  2, -11),   S(-14,  -8),
            S(-34, -42),   S( 11,   4),   S(  3,   7),   S(  5, -11),   S( 19,  27),   S( 34,  11),   S( -1,  45),   S(-22,   2),

            /* bishops: bucket 1 */
            S( 36,   9),   S( -7,  30),   S(  8,  39),   S( 15,  30),   S( -7,  32),   S(  2,  35),   S( -6,   5),   S(-47,  -4),
            S(  9, -13),   S( 34, -11),   S( 51,  11),   S( 26,  33),   S( -9,  20),   S(  7,   3),   S(-33,  -2),   S( 14, -17),
            S( 39,  -3),   S( 22,   6),   S( 39,  -7),   S( 21,  26),   S( 20,  29),   S(-20,   6),   S( 29,  -1),   S(  3, -28),
            S( 40,   3),   S( 17,  15),   S( 13,  12),   S( 37,  16),   S(  4,  21),   S( 22,   3),   S( -7,   4),   S( 15, -11),
            S( 36,  28),   S(  9,  19),   S( 21,  22),   S(  1,  28),   S( 26,   8),   S(  0,  14),   S( 28, -20),   S( -8,  10),
            S(  0,  22),   S( 32,  33),   S( 29,   3),   S( 55,  -5),   S( 17,  17),   S( 34, -17),   S(  1,  27),   S( 48, -15),
            S( -9,  46),   S(-27,  22),   S( 16,  31),   S( 35,  27),   S( 41,  28),   S(-19,  25),   S( 36, -18),   S(-19,  39),
            S( 12,   3),   S( 11,   7),   S(  4,  14),   S(-20,  24),   S( 20,  18),   S( -8,   7),   S( 12,   6),   S( -3,  11),

            /* bishops: bucket 2 */
            S( 17, -15),   S(  7,  20),   S( -2,  23),   S(-29,  54),   S( -9,  38),   S(-28,  33),   S(-18,  -4),   S(-46,  18),
            S(-23,  24),   S(  5, -14),   S( 20,  15),   S( -4,  32),   S( -3,  39),   S( 16,  12),   S( -9, -18),   S(  4, -33),
            S( -6,   6),   S( -2,  14),   S(  5,  10),   S( -2,  47),   S(  8,  37),   S( -1,  20),   S( 17,  16),   S(-17,  -2),
            S(  1,  11),   S(-11,  11),   S(-12,  34),   S(  4,  32),   S( -6,  38),   S(  8,  18),   S(  4,  14),   S(  9,   1),
            S(  9,   5),   S(-18,  31),   S( -7,  23),   S(-31,  43),   S( -9,  32),   S( -9,  43),   S(  3,  18),   S(-28,  32),
            S(  6,  26),   S( -3,  11),   S(-27,  28),   S(-15,  25),   S( 11,  14),   S( -8,   8),   S( -4,  53),   S(  1,  25),
            S(  1,  21),   S(-22,  12),   S(-26,  55),   S( 20,   4),   S( -3,   5),   S(-18,  11),   S(-69,  14),   S(-36,  36),
            S(-54,  31),   S(-35,  46),   S(-26,  28),   S(-38,  27),   S(-48,  38),   S(-33,  19),   S(  4,  15),   S(-71,   8),

            /* bishops: bucket 3 */
            S( -1,   4),   S( 36,  -7),   S( 31,  20),   S( 17,  22),   S( 21,  15),   S( 42,  -2),   S( 46, -20),   S( 41, -61),
            S( 10,   5),   S(  7,   0),   S( 30,  -2),   S(  9,  36),   S( 22,  12),   S( 22,  28),   S( 44,  -2),   S( 38,  -3),
            S( 21,   9),   S( 14,  19),   S( 12,  18),   S( 26,  25),   S( 25,  56),   S( 25,  11),   S( 42,  27),   S( 48, -10),
            S( 31,  -5),   S( 25,   5),   S( 18,  29),   S( 24,  41),   S( 29,  36),   S( 32,  25),   S( 27,  17),   S( 27,  -4),
            S( 19,   3),   S( 25,  11),   S( 44,  10),   S( 29,  41),   S( 25,  43),   S( 36,  26),   S( 18,  29),   S( 25,  32),
            S( 30,   4),   S( 36,  18),   S( 24,  11),   S( 42,  12),   S( 26,  20),   S( 52,   3),   S( 51,  13),   S(  7,  68),
            S( 19,  10),   S( -7,  17),   S( 41,  25),   S( 22,  19),   S( 12,  18),   S( 16,   5),   S( -3,  29),   S( 20,  34),
            S(-33,  59),   S( -2,  34),   S( 57,  11),   S( 22,  17),   S(-17,  36),   S( -2,  36),   S( 24,   2),   S( 55, -25),

            /* bishops: bucket 4 */
            S(-23, -26),   S(-21,   8),   S(-31,  -1),   S(-20,  20),   S(-21,  31),   S(-44,  30),   S(  1,  -8),   S(-12, -11),
            S( -7,   9),   S(  4,   7),   S( -6,  35),   S(-26,  20),   S(-18,  -2),   S( 38,  -1),   S(-22,  -7),   S( 14,  -1),
            S(-13,  -1),   S(-35,  39),   S( 14, -14),   S(-24,  19),   S(  2,  31),   S( 30, -19),   S(-24,  -4),   S(-53,  -1),
            S(-36,  27),   S( -6,  36),   S( 49,  30),   S( 27,  32),   S( 13,  17),   S( 49, -12),   S( 47,  -9),   S(-14, -37),
            S(  3,  18),   S( -2,  45),   S(-23,  53),   S( 13,  39),   S( 29,   6),   S( 32, -22),   S(-10, -26),   S( 17,  -9),
            S( -7,  35),   S( 17,  15),   S(-18,  25),   S( 14,  12),   S( 39,   9),   S(  5, -14),   S( 16, -37),   S(  3,  -6),
            S(-15,   9),   S( 31,  18),   S( 18,  22),   S( 25,  18),   S( 10,  -3),   S(  3,  19),   S(  1,   5),   S(  7, -26),
            S( 12, -16),   S( -9, -36),   S(  2,  -2),   S( -3,   2),   S(  6, -11),   S(  0,   9),   S( -1,  -7),   S( -5,   1),

            /* bishops: bucket 5 */
            S(-16, -12),   S(-11,  36),   S(-31,  30),   S( -9,  31),   S(-29,  33),   S(  7,  14),   S( -6,  18),   S(-22,  14),
            S(-26,  38),   S(-13,   7),   S(-23,  57),   S(  8,  28),   S(-22,  36),   S(-26,  29),   S(-32, -11),   S(-10,   0),
            S(  0,  18),   S(  1,  41),   S( 24,  15),   S(-19,  54),   S(  5,  39),   S(-26,   0),   S(-22,  36),   S(-17,   8),
            S( 29,  15),   S( 21,  30),   S(-18,  60),   S( 27,  31),   S( 30,  35),   S( 14,  27),   S( 17,  -7),   S( 12,  28),
            S( 27,  45),   S( 31,  13),   S( 46,  30),   S( 72,  32),   S( 43,  18),   S( 39,  16),   S( 35,   9),   S( -5,   5),
            S( 21,  40),   S( 22,  44),   S( 31,  21),   S( 19,  35),   S( -5,  35),   S( 15, -18),   S(-22,  44),   S(  4,  33),
            S(  2,  39),   S(-26,  15),   S( 16,  43),   S(  8,  55),   S( 30,  31),   S( 35,  40),   S(  0,  21),   S(  0,  31),
            S( -1,  -7),   S( 16,  37),   S( 16,  14),   S(  7,  38),   S(  4,  58),   S( 15,  26),   S( 30,  56),   S( -7,   0),

            /* bishops: bucket 6 */
            S(-11,  15),   S(  5,  28),   S(-19,  29),   S(-24,  33),   S(-24,  24),   S(-29,  30),   S(-18,  53),   S(-19,   9),
            S( 23,   8),   S(  6, -11),   S(-15,  32),   S(  5,  29),   S(-24,  44),   S(-11,  26),   S(-94,  29),   S( 17,  24),
            S( 22,   2),   S( 17,   9),   S( 36,  -1),   S( 27,  30),   S( 42,  27),   S( 20,  11),   S( 10,  32),   S(-37,  20),
            S( -9,  39),   S( 15,  13),   S( 31,  20),   S( 25,  36),   S( 43,  32),   S( 34,  26),   S( 30,  32),   S(-16,   1),
            S( -3,  21),   S( 53,   2),   S( 21,  23),   S( 48,  21),   S( 92,  24),   S( 53,  25),   S( 31,  27),   S(-29,  46),
            S( 13,  11),   S(-45,  45),   S(  9,  17),   S(  7,  41),   S( 31,  29),   S( 22,  26),   S( -2,  46),   S(-13,  47),
            S(-21,  31),   S(-26,  27),   S(  6,  41),   S( -8,  35),   S( 47,  24),   S( 26,  32),   S( -8,  36),   S( -2,  36),
            S(  7,  46),   S( 14,  35),   S( 12,  41),   S(  3,  47),   S(-14,  41),   S( 35,  19),   S( 13,  25),   S( 13,  11),

            /* bishops: bucket 7 */
            S(-15, -37),   S( -4,   6),   S(-30, -25),   S(-48,  13),   S(-27, -10),   S(-74,  20),   S(-69, -27),   S(-64,   7),
            S(-31, -28),   S(-54, -39),   S(-16,  -3),   S(  2, -12),   S(-26,   3),   S(-39,  15),   S(-47, -11),   S(-31,   7),
            S(-31, -19),   S(  7, -15),   S( 28, -38),   S( 25,   0),   S(-31,  23),   S(-20, -12),   S(-37,  48),   S(-30,  27),
            S(-36,  16),   S( 51, -38),   S( 74, -26),   S( 54,  -2),   S( 77,  -4),   S(  5,  20),   S( 20,  32),   S(-14,  29),
            S( 29, -49),   S( -7, -26),   S( 58, -38),   S( 95, -31),   S( 61,  20),   S( 64,  13),   S(-14,  41),   S( 20,  10),
            S(-22, -13),   S(-25,  -2),   S( 31, -47),   S( 16,  -4),   S( 41, -10),   S( 43,   1),   S( 47,  12),   S( 21,   2),
            S(  3, -15),   S(-38,  -6),   S( 13,  -2),   S( 14,  -6),   S( 17, -19),   S( 33,  -5),   S( 12,   1),   S( 13,  15),
            S(-13,  -4),   S( -9,  15),   S(-30,  10),   S(  7,  -4),   S( 11,  -2),   S( 19,  -3),   S( 25,  10),   S(  7,  12),

            /* bishops: bucket 8 */
            S(-10,  -9),   S(-12, -32),   S(-40,  -5),   S( -2, -26),   S( -5,  20),   S(-22,  -2),   S(  7,  23),   S( -4,  -8),
            S( -6,  -3),   S(-30, -46),   S(-12, -22),   S(-14,  -3),   S( 12,  -8),   S(-17, -27),   S(-18, -53),   S( -4,  -6),
            S(  2,   2),   S( -8,  12),   S(-23,   9),   S( -7,  20),   S( -3,  15),   S( -6, -38),   S(  7, -40),   S(-31, -36),
            S(  5,  33),   S( -8,  45),   S(  6,  41),   S( -6,  13),   S( 14,  18),   S( -4,   6),   S(  1, -19),   S( -4, -16),
            S( 15,  37),   S( 12,  66),   S(-13,  33),   S( 41,  42),   S( -1,  17),   S( 14,   5),   S(  7, -33),   S(-10, -15),
            S( -2,   6),   S( 12,  36),   S(  8,  20),   S(-16,  17),   S( 25,   9),   S(-10, -17),   S(-15, -16),   S(-17, -19),
            S( -4,   5),   S( 11,  26),   S( 10,  24),   S(  1,   3),   S(  5,  13),   S( -1,  23),   S(-12, -13),   S( -9, -27),
            S( -8, -12),   S(  1, -26),   S(  0,  -6),   S( -1, -13),   S(-19,  -9),   S( -5,  -4),   S(  0,  14),   S( -7,   7),

            /* bishops: bucket 9 */
            S(-24, -31),   S( -5,   3),   S(-17,   3),   S( -8, -22),   S(-31, -27),   S(-18, -37),   S(-16, -11),   S(  7,  -5),
            S(-16, -17),   S(-36, -30),   S( -8,  -8),   S(-11,  15),   S(-44,  28),   S(-17, -16),   S(-14, -18),   S( -4,  -5),
            S(  9,  -2),   S( 20,  12),   S(-24, -17),   S(-13,  25),   S(  7,  15),   S( -6, -23),   S(-12, -22),   S( -3,  26),
            S(-15,   8),   S( 14,  16),   S(-12,  28),   S(  6,  23),   S( 19,  23),   S(  7,   0),   S(  2,  -5),   S(-16, -23),
            S( -1,  20),   S( 21,  23),   S(  5,  42),   S( 10,  51),   S(-18,  14),   S(  0,  29),   S( -8,  31),   S( -6,  -5),
            S(-12,   2),   S( 18,  47),   S(  1,  19),   S( 21,  24),   S( 10,  37),   S( -9,  -6),   S(-20,  -1),   S(-12, -10),
            S(  3,  13),   S( 20,  11),   S(  6,  11),   S(  3,  49),   S( 20,  41),   S(  8,   6),   S( -7, -14),   S( -5,  -1),
            S( -4, -27),   S( -7,  22),   S( -3,  19),   S(-18, -11),   S(-13,  -2),   S(  6,  27),   S(  1,   4),   S(-13, -18),

            /* bishops: bucket 10 */
            S(-23, -13),   S(  5, -26),   S(-32, -26),   S(-15, -22),   S(-20, -10),   S(-21, -20),   S(-11, -20),   S(-18, -27),
            S(  6, -16),   S(-27, -37),   S( -3,  -9),   S(-37,   4),   S(-34,   7),   S(-18,  22),   S(-28, -55),   S(-12, -17),
            S( 11,  -9),   S(  5,  -7),   S(-34, -46),   S(  7,   9),   S(-34,  31),   S(-37,  15),   S(-21,  28),   S(  5,  17),
            S(-10, -20),   S(  3,   7),   S(  8,  -7),   S( 19,   3),   S(  9,  27),   S(-13,  58),   S(  4,  30),   S( 15,  26),
            S(-17,   0),   S(  0,  -2),   S( -9,  15),   S( 27,  28),   S( -2,  64),   S( 20,  51),   S(  8,  38),   S(  2, -16),
            S(  4, -25),   S(-28,  -4),   S(-25, -14),   S(-12,  28),   S( 23,  41),   S( 33,  26),   S(  7,  50),   S(  1,   9),
            S(-21,  -8),   S(-10, -45),   S( -8,  -8),   S( 23,  15),   S( -4,  -2),   S( 18,  39),   S( 17,  36),   S( 12,  13),
            S( -5, -28),   S( -9,   4),   S(  5,  18),   S( -9,   5),   S( -8,  18),   S( -8,  -6),   S( 11,   1),   S(  5,  22),

            /* bishops: bucket 11 */
            S(-19,   3),   S(-31, -11),   S(-46, -44),   S(-21, -29),   S(-18,  -9),   S(-63, -44),   S( -9, -12),   S(-22, -22),
            S(-11, -16),   S(  0, -37),   S( -6,  -3),   S(-23, -32),   S(-44,  -8),   S(-28, -28),   S(-24, -42),   S(-22, -34),
            S( -9, -46),   S(  5, -44),   S(-27, -21),   S(  1,  -6),   S(  0,  -2),   S(-34,  12),   S( -8,  28),   S( -1,  20),
            S(-16, -37),   S(-15, -37),   S(  4, -14),   S( -1,  -8),   S( 10,  18),   S( -3,  59),   S(  6,  51),   S( 16,  26),
            S( -8, -24),   S(-19, -46),   S(-19,  17),   S( 46,  -3),   S( 28,  35),   S( -6,  60),   S( 16,  56),   S( 13,  25),
            S(-17, -50),   S(-33,  -4),   S(-14, -42),   S(  4,  11),   S(  2,  30),   S( 14,  29),   S( 24,  38),   S( -5,  -2),
            S( -7,  -7),   S(-18, -42),   S(-19,   3),   S( -5, -14),   S( 11,   1),   S( 36,  19),   S( -7,   7),   S( 14,  28),
            S(-18, -14),   S(-20,  -1),   S( -6,  13),   S( 11,   4),   S( 12,   3),   S(-17, -23),   S(  4,   8),   S( -1, -18),

            /* bishops: bucket 12 */
            S(  0,   3),   S( -7, -13),   S(-12, -29),   S( -6, -26),   S( -9, -19),   S(-11, -20),   S( -1,  11),   S( -5,  -1),
            S( -7,  -7),   S(-13, -32),   S( -7, -13),   S( -6, -11),   S(-13, -22),   S( -2,  14),   S( -3,  -2),   S( -1,  -8),
            S( -1,  -4),   S(-15,  -1),   S(-12, -18),   S( -8,  -4),   S( -4,   9),   S( -5, -14),   S( -9, -42),   S( -4,  -5),
            S( -1,   4),   S(  5,   0),   S(-17, -29),   S( -4,  11),   S(  1,   5),   S(  5,  23),   S( -5,  -7),   S( -6,  -3),
            S( -1,  -3),   S(  2,  17),   S( -5,  20),   S(-10,   0),   S( -4,  -5),   S( -6,   2),   S(  4,   5),   S( -7,  -1),
            S(-13, -14),   S(  3,  59),   S(-29,   2),   S(-10,  -5),   S(  6, -15),   S( -5,   1),   S(  0,   4),   S( -1,  -5),
            S( -2,  -4),   S( -5,  14),   S(  5,  20),   S( -7,   6),   S( -1,   9),   S(  8,  17),   S( -7, -16),   S( -2,   5),
            S( -2,  -3),   S(  0,  -5),   S( -5,   2),   S(  6,   8),   S(  2,  10),   S(  0,   3),   S(-10,  -1),   S(  1,  -3),

            /* bishops: bucket 13 */
            S( -9, -42),   S(-13, -28),   S(-13, -17),   S(-15, -18),   S(-15, -19),   S( -8,   1),   S( -2,  -5),   S( -7,  -9),
            S( -4,  -6),   S(-10, -12),   S(-13, -28),   S(-18,  -9),   S(-12,   7),   S( -8,   1),   S( -1, -11),   S(  3,  -2),
            S( -9, -10),   S( -5,  -5),   S( -7,  11),   S(-21,  -2),   S(-12, -22),   S( -3, -11),   S( -2, -28),   S(  5,  21),
            S( -2,   3),   S(-12,  -3),   S(-14,   4),   S(-27,   9),   S( -2,  15),   S(  2,  -7),   S( -2,   3),   S( -7,  -6),
            S( -3,   9),   S(-17,   4),   S(-18,  -5),   S( 16,  -2),   S(-10,  -2),   S( -6,   6),   S(-11, -20),   S( -2,  -8),
            S( -3,  -6),   S( -9,   2),   S(-20, -13),   S( 10,  15),   S(  3,  10),   S( -4,  -8),   S(  6,  18),   S( -3,  -6),
            S( -6,  -8),   S( -9,  -2),   S(  7,  28),   S( -6,   9),   S( -7,   1),   S(  2,   1),   S(-15, -25),   S(  0,   7),
            S( -8, -16),   S( -1,   7),   S( -1,  -3),   S(  4,   1),   S(  0,   5),   S( -8,  -6),   S(  1,  10),   S( -3, -14),

            /* bishops: bucket 14 */
            S( -7, -16),   S(-12, -16),   S(-18, -28),   S(-17, -43),   S(-14, -36),   S( -5, -26),   S(-10, -15),   S(-10, -16),
            S(-10, -26),   S( -1, -21),   S( -7, -13),   S(-26, -42),   S(-10, -11),   S(-17, -10),   S(-14, -22),   S(  1, -13),
            S( -8, -12),   S( -8, -30),   S(-21, -29),   S(-13, -18),   S(-25,  -4),   S(-22, -31),   S( -6,   2),   S( -3,  -3),
            S( -8, -22),   S( -9,  -8),   S(-11,  -5),   S(-24,  18),   S( -2,   6),   S(-24,   9),   S(-19, -15),   S( -5, -11),
            S( -9,  -6),   S( -8,  25),   S( -8, -20),   S( -7, -21),   S(-15,   7),   S( -8,  -7),   S(  4,  20),   S(  2,  -5),
            S( -1,   4),   S( -9,   7),   S(-22, -12),   S( -8, -16),   S(  5,   8),   S(-10,  18),   S( -2,  33),   S( -7, -21),
            S( -6, -22),   S( -1,   0),   S( -6,   2),   S(  4,  20),   S( -9,  -1),   S( -1,   3),   S( -3, -12),   S( -4,  -7),
            S( -7,  -8),   S( -4,  -8),   S( -3,  -6),   S( -2,   7),   S(-10, -18),   S(  1,  10),   S(  6, -13),   S(  0,   4),

            /* bishops: bucket 15 */
            S(  4,   8),   S(  6,   6),   S(-19, -28),   S(  0, -10),   S( -9, -14),   S(-12, -23),   S( -6, -13),   S( -2,  -9),
            S(  3,   5),   S( -1,  -7),   S(  3,  -1),   S( -8, -11),   S(-14, -20),   S( -6,  -7),   S( -8, -17),   S( -1,   0),
            S( -7, -13),   S(  0,  -1),   S(-12,  -9),   S(-10,  -6),   S(-19, -18),   S(-17, -20),   S( -7, -10),   S(  2,  17),
            S( -4,  -7),   S(-17, -17),   S(  5, -13),   S(-24, -31),   S( -5,   5),   S( -9, -14),   S(  3,  15),   S( -1,  -9),
            S( -1,  -9),   S(-12, -18),   S(-14, -10),   S(-20, -47),   S( -3, -25),   S(-16,  18),   S(  3,  19),   S(-10, -16),
            S( -9, -32),   S(-12, -12),   S(-19, -35),   S(-21, -12),   S( -5,  -3),   S(-10, -27),   S(  7,  38),   S(  1,  11),
            S( -3,   2),   S( -1, -16),   S( -1, -13),   S( -4,   5),   S(-11, -15),   S(  0,  12),   S(-11,   2),   S(  4,   5),
            S( -3,  -2),   S( -1,   2),   S( -4,   1),   S( -6,  -3),   S( -8,  -4),   S(-16, -19),   S( -9, -23),   S(  1,   1),

            /* rooks: bucket 0 */
            S(-22,   7),   S( -8,  -1),   S(-17, -11),   S(-10,  -6),   S(-14,  11),   S( -9,  -9),   S(-15,  21),   S( -2,  16),
            S( 11, -60),   S( 26, -15),   S(  6,  -3),   S(  0,   4),   S( 16,   1),   S( -2,  -3),   S(-32,  22),   S(-44,  33),
            S(  1, -24),   S( 13,  30),   S( 19,  10),   S(  7,  14),   S(-17,  43),   S( -4,  11),   S(-31,  19),   S(-40,  15),
            S( 26, -23),   S( 59,   1),   S( 39,  30),   S( 37,   7),   S( 11,  10),   S( -5,  16),   S(-15,  22),   S(-40,  35),
            S( 57, -23),   S( 85, -14),   S( 60,   0),   S( 34,  -7),   S( 43,   7),   S( 21,  10),   S(-12,  39),   S(-23,  35),
            S( 62, -42),   S(100, -33),   S( 46,   7),   S( 11,  22),   S( 40,  11),   S(-45,  35),   S( 25,  22),   S(-43,  44),
            S( 38, -10),   S( 65,  -3),   S( 19,   8),   S(  3,  29),   S(-14,  31),   S( -8,  16),   S(-20,  35),   S(-18,  27),
            S( 28,  17),   S(  9,  46),   S( 13,  26),   S( -7,  39),   S(  1,  20),   S(  5,   1),   S( -3,  28),   S(  6,  23),

            /* rooks: bucket 1 */
            S(-75,  34),   S(-53,   4),   S(-57,  -7),   S(-44, -14),   S(-31, -21),   S(-29, -21),   S(-34,  -9),   S(-36,  18),
            S(-29,   5),   S(-51,  16),   S(-14, -13),   S(-22, -30),   S(-27, -10),   S(-38, -11),   S(-40, -16),   S(-56,  16),
            S(  4,   9),   S(-22,  33),   S(-17,  15),   S(-41,  25),   S(-46,  33),   S( -6,   3),   S(-25,   9),   S(-47,  23),
            S(-52,  54),   S(-38,  33),   S(  3,  18),   S(-17,  22),   S(-30,  33),   S(-46,  44),   S(-38,  41),   S(-34,  16),
            S( 46,  16),   S( 23,  36),   S( 21,   3),   S(-41,  42),   S(-27,  42),   S( 11,  23),   S( -4,  20),   S(-43,  25),
            S( 41,  12),   S(  5,  30),   S(  2,  26),   S(-38,  31),   S(  4,  14),   S(-33,  44),   S(-19,  27),   S(-48,  34),
            S(-16,  30),   S(  2,  29),   S( 16,  28),   S(-51,  51),   S(-29,  34),   S( -1,  34),   S(-41,  30),   S(-58,  34),
            S( 30,  27),   S( 30,  35),   S( -5,  29),   S(-48,  54),   S( -6,  17),   S( 22,  13),   S(-15,  35),   S( -3,  12),

            /* rooks: bucket 2 */
            S(-64,  36),   S(-46,  19),   S(-47,  14),   S(-57,  14),   S(-60,  10),   S(-44,   2),   S(-31, -23),   S(-48,  28),
            S(-72,  46),   S(-58,  37),   S(-43,  27),   S(-52,  12),   S(-41,  -1),   S(-51,   1),   S(-59,  19),   S(-58,  19),
            S(-71,  65),   S(-56,  53),   S(-53,  55),   S(-33,  15),   S(-43,  29),   S(-20,  24),   S(-15,  17),   S(-32,  25),
            S(-72,  65),   S(-57,  67),   S(-42,  63),   S(-40,  51),   S(-33,  37),   S(  1,  34),   S(-38,  54),   S(-20,  35),
            S(-25,  52),   S(-47,  67),   S(-47,  59),   S(-25,  44),   S( 16,  30),   S( 15,  32),   S(-28,  52),   S(-41,  48),
            S(-37,  45),   S(-32,  48),   S(-21,  33),   S( -9,  25),   S( 13,  31),   S( 43,  18),   S( 21,  20),   S(-19,  30),
            S(-53,  42),   S(-68,  71),   S(-35,  56),   S(-17,  52),   S(  4,  32),   S( 17,  23),   S(-52,  60),   S(-37,  47),
            S(-28,  64),   S( -7,  45),   S(-71,  66),   S(-35,  47),   S(-56,  60),   S(-33,  62),   S(-52,  73),   S(-22,  44),

            /* rooks: bucket 3 */
            S( -5,  74),   S( -7,  66),   S( -4,  59),   S(  3,  49),   S(  0,  44),   S(-19,  67),   S(-10,  75),   S( -4,  38),
            S(-31,  88),   S(-12,  70),   S(  3,  64),   S(  9,  58),   S( 18,  49),   S( 13,  56),   S( 41,   2),   S( 20, -37),
            S(-34,  84),   S(-15,  85),   S( -1,  78),   S( 12,  61),   S(  8,  80),   S( 25,  71),   S( 31,  72),   S(  4,  53),
            S(-27,  91),   S(-18,  84),   S( 17,  73),   S( 22,  69),   S( 17,  74),   S( -6, 111),   S( 57,  63),   S( 18,  70),
            S(-16,  99),   S( 22,  79),   S( 14,  70),   S( 35,  70),   S( 37,  69),   S( 42,  68),   S( 85,  53),   S( 52,  46),
            S(-14,  90),   S( 10,  76),   S(  7,  72),   S( 14,  71),   S( 22,  55),   S( 42,  52),   S( 79,  35),   S( 88,  15),
            S(-37, 100),   S(-20, 100),   S(-11,  95),   S( 19,  81),   S( 10,  77),   S( 19,  74),   S( 48,  68),   S( 92,  31),
            S(-73, 147),   S( -7, 102),   S(  6,  78),   S( 36,  66),   S( 42,  58),   S( 43,  69),   S(103,  52),   S( 84,  50),

            /* rooks: bucket 4 */
            S(-87,  25),   S( -6,  -3),   S(-42,   4),   S(-27,  18),   S(-28, -18),   S(  9, -48),   S( -2, -20),   S( -7, -36),
            S(-32,  -2),   S(-40,   5),   S(-42,  14),   S(-40,  25),   S( -4,  -8),   S(-17, -20),   S(  6, -32),   S(-15, -23),
            S(  0,   8),   S(-27, -18),   S(-16,  11),   S( -8, -10),   S(  1,  -3),   S(  1,  -5),   S( 36, -14),   S(-37,   0),
            S(-30, -12),   S(  3,   5),   S(-27,  18),   S( 28,   1),   S( 20,   6),   S( 16,   1),   S( 19,  11),   S( -4,  14),
            S(-20, -11),   S( -6,  30),   S(-14,  23),   S( 68,   9),   S( 20,  24),   S( -1,  19),   S( 39,  29),   S( 31,   1),
            S( 19,   8),   S( 23,  11),   S( 47,  16),   S( 37,  13),   S( 34,  17),   S(  4,  35),   S(  7,  28),   S( 21,  31),
            S( -2,  -7),   S( 30,  28),   S( 26,  29),   S( 36,  21),   S( 53,  12),   S( 13,   2),   S( 32,  18),   S( 27,  22),
            S( 33, -58),   S( 35,  44),   S( 14,  27),   S( 12,  18),   S( 14,   5),   S( 11,  25),   S( 14,   5),   S( 16,  17),

            /* rooks: bucket 5 */
            S(-34,  30),   S(-49,  50),   S(-61,  47),   S(-52,  32),   S(-42,  23),   S(-36,  40),   S( -4,  26),   S(-31,  42),
            S(-27,  31),   S(-31,  30),   S(-78,  66),   S(-51,  38),   S(-39,  24),   S(-14,  17),   S( 11,  16),   S(-20,  20),
            S( 12,  40),   S(-39,  57),   S(-53,  57),   S(-59,  57),   S(-33,  31),   S( -4,  31),   S( -1,  42),   S(  0,  41),
            S(-23,  70),   S( -5,  47),   S(-26,  66),   S(-15,  44),   S(-19,  58),   S(  8,  61),   S( -2,  55),   S(  8,  36),
            S( 15,  60),   S(  1,  66),   S( 35,  47),   S( 28,  60),   S( 30,  56),   S( 13,  76),   S( 62,  61),   S( 28,  41),
            S( 62,  54),   S( 27,  65),   S( 50,  52),   S( 22,  70),   S( 53,  49),   S( 49,  58),   S( 53,  47),   S( 47,  40),
            S( 46,  40),   S( 21,  64),   S( 42,  53),   S( 59,  42),   S( 32,  51),   S( 43,  56),   S( 66,  48),   S( 67,  43),
            S( 92,  28),   S( 68,  32),   S( 31,  55),   S( 17,  37),   S( 46,  46),   S( 48,  48),   S( 44,  41),   S( 25,  46),

            /* rooks: bucket 6 */
            S(-41,  22),   S(-41,  35),   S(-26,  28),   S(-40,  27),   S(-58,  33),   S(-74,  60),   S(-48,  54),   S(-35,  52),
            S(-20,  28),   S(-15,  29),   S(-14,  29),   S(-45,  27),   S(-52,  47),   S(-65,  61),   S(-63,  58),   S( 25,  13),
            S(-16,  52),   S(-11,  35),   S(  3,  36),   S(-38,  41),   S( -6,  30),   S(-38,  62),   S(-29,  74),   S( 18,  36),
            S(-26,  67),   S( 33,  44),   S(  0,  59),   S(  6,  42),   S(  0,  46),   S( -2,  57),   S(-42,  64),   S(-13,  57),
            S(  9,  70),   S( 43,  58),   S( 55,  47),   S( 28,  47),   S( 13,  65),   S( 35,  52),   S( 35,  49),   S( 13,  58),
            S( 16,  61),   S( 59,  52),   S( 82,  33),   S( 35,  36),   S( 25,  48),   S( 42,  57),   S( 50,  49),   S( 64,  48),
            S( 39,  59),   S( 76,  41),   S( 80,  34),   S( 87,  21),   S( 91,  27),   S( 45,  53),   S( 49,  51),   S( 48,  47),
            S( 58,  70),   S( 27,  65),   S( 33,  51),   S( 37,  43),   S( 58,  49),   S( 52,  64),   S( 55,  61),   S( 21,  65),

            /* rooks: bucket 7 */
            S(-63, -17),   S(-39, -12),   S(-35, -20),   S(-26, -10),   S(-24,  -6),   S(-60,  34),   S(-45,  18),   S( -8, -17),
            S(-55,  15),   S(-31,   1),   S(-37,   2),   S( -6, -13),   S(-20,  11),   S(-11,  13),   S(-17,  -2),   S(-56,   9),
            S(-74,  46),   S(-29,  13),   S( -2,   5),   S(  1, -12),   S( -5,   5),   S(-27,  -4),   S(-26, -11),   S(  7,   6),
            S(-55,  39),   S(  1,  18),   S( 10,  13),   S( 20,  12),   S( 28,   1),   S( 25,   9),   S( 32,   2),   S(-11,   8),
            S(-22,  39),   S( 15,  11),   S( 52, -12),   S( 56,  -5),   S( 64,   2),   S( 89,   7),   S( 72,   4),   S( 39, -10),
            S(-14,  31),   S( 14,  14),   S( 80, -24),   S( 98, -22),   S( 68,  -4),   S( 63,  20),   S( 64,  19),   S( 20,   3),
            S(-14,  35),   S( 14,  19),   S( 43,   4),   S( 61,   3),   S( 84,  -5),   S( 83,  -5),   S( 34,  26),   S(  7,   9),
            S(  5,  60),   S(-26,  43),   S( 32,   4),   S( 78, -23),   S( 17,   8),   S( 10,  17),   S( 40,   7),   S( 52,  -6),

            /* rooks: bucket 8 */
            S(-42, -48),   S( -9, -11),   S(  4,   4),   S(  3, -15),   S( -9, -42),   S( -8, -55),   S(-15, -26),   S( -3, -19),
            S( -3, -19),   S( -4,  -6),   S(  1, -13),   S( 11, -13),   S( -5, -25),   S( -9, -21),   S( -5, -43),   S(-16, -64),
            S(  7,  15),   S(  9, -15),   S(  6,   6),   S( 13,  10),   S(-13, -32),   S( -1, -31),   S( 14,  23),   S(  0,  -1),
            S( -7, -19),   S( -4,  25),   S( -3,   7),   S( 21,   4),   S(  6,  15),   S( -3, -11),   S( 10, -16),   S(  2,   2),
            S( -8, -11),   S(  0,  18),   S( -2,  29),   S( 17,  10),   S(  4,   7),   S( 22,   8),   S( 11,  -9),   S( 13, -32),
            S(  7,  26),   S( -7,   7),   S( 32,  42),   S( 28,  -8),   S(  0,  -5),   S(  6, -13),   S(  3,   0),   S( 13,  43),
            S(  0, -12),   S( 11, -16),   S( 24,   2),   S( 18, -19),   S( 28,   7),   S( 21, -20),   S( 19, -14),   S( 16,  -6),
            S(  1, -147),  S(  8, -15),   S( 21,   8),   S( -1,  -8),   S(  2,   2),   S(  3, -12),   S(  7,  -7),   S( 22,  -1),

            /* rooks: bucket 9 */
            S(-40, -16),   S( -6, -24),   S(-18, -29),   S(-32,  -2),   S(-16,   3),   S( -2,  -4),   S( 16, -45),   S(-33, -31),
            S( 35, -21),   S(  5, -19),   S(-12, -19),   S(-16,  -4),   S(-14, -11),   S( 21,   6),   S(  5, -28),   S( -9, -29),
            S( 13, -19),   S( 20,  -6),   S(  4,   5),   S( -6,  -1),   S( -7, -21),   S( 30,  -4),   S( 15,  20),   S(  0,   0),
            S(  5,   9),   S( 10,   6),   S( 13,  20),   S( -2,   6),   S(  6,  21),   S( 25,  -2),   S( 16,  37),   S( 14,   3),
            S( 14,   5),   S(  4,  10),   S(  4,  27),   S( 14,  24),   S( 35,  32),   S( 26,  31),   S( 12,   0),   S( 14,  -7),
            S( 14,  42),   S( -7,  10),   S( 10,   2),   S(-14,   2),   S( 12,   7),   S( 29,   8),   S(  8,  35),   S( 15,  13),
            S( 63,  18),   S( 60,   5),   S( 28,  26),   S( 53,  11),   S( 30,  -8),   S( 30,   6),   S( 38,   1),   S( 43,  25),
            S( 67, -70),   S( 39, -30),   S( 19,  25),   S( 31,  26),   S( 11,  35),   S( 27,  16),   S( 22,  11),   S( 32,   9),

            /* rooks: bucket 10 */
            S(-49, -80),   S(-14, -50),   S(-39, -29),   S(-29,  -5),   S(-31,  -5),   S(-27, -12),   S( 14, -17),   S(-28, -21),
            S(  2, -19),   S( 15, -28),   S(  3, -26),   S(  1, -15),   S(  7, -18),   S( -5,  -4),   S( 35,   5),   S( 12,  -3),
            S(-10, -19),   S( -8, -22),   S(  5, -17),   S( 24,  -4),   S(-15,  18),   S(  0,  -7),   S( 27,  26),   S( 10,  -6),
            S(  7,   0),   S(  8, -13),   S(  3,  -6),   S(  7,  16),   S( 26,  -4),   S(  2,  -5),   S( 24,  26),   S(  0,  -8),
            S(  9,  12),   S( 33,   7),   S( 15,   9),   S( 18, -20),   S( -5,  -3),   S( 13,  10),   S( 31,  30),   S(  8,  27),
            S( 37,  30),   S( 30,  42),   S( 21,  11),   S( 18,   9),   S(  0,  -8),   S( 15,  10),   S( 32,  20),   S(  7,  37),
            S( 71,  13),   S( 80,   1),   S( 75,  -4),   S( 68, -16),   S( 53, -17),   S( 35,  12),   S( 23,   7),   S( 28,   7),
            S( 60,  15),   S(  8,  -3),   S( 40,  -1),   S( 21,   7),   S( 33,  -2),   S( 28,  11),   S( 14,   3),   S( 20, -12),

            /* rooks: bucket 11 */
            S(-34, -48),   S(-25, -27),   S(-16, -28),   S(-25, -55),   S(  3, -22),   S( -4,   2),   S(-23, -32),   S(-51, -18),
            S(-12, -29),   S( -5, -43),   S(  1, -28),   S(  3, -28),   S( -2, -23),   S(-16, -17),   S( -2, -32),   S(-20,   1),
            S(  5, -31),   S( 17, -13),   S( 23, -14),   S( 13, -20),   S( 14, -10),   S( -9,  11),   S(-21, -24),   S(-12, -53),
            S(  1,  27),   S(  0, -10),   S(  1,  13),   S( 15,   7),   S(  4,  -2),   S( 12,  31),   S( 28, -10),   S(  2, -25),
            S( 12,  11),   S( 21, -10),   S( 31,   2),   S( 21,  -7),   S( 25,  -4),   S( 29,  -6),   S( 10,  11),   S( -2, -12),
            S( 26,  33),   S( 44,   9),   S( 26,  -9),   S( 49,  21),   S( 51,  20),   S( 39,  10),   S( -3,   5),   S( 16,  25),
            S( 61,  35),   S( 59,   3),   S( 69, -12),   S( 75, -14),   S( 45,  -9),   S( 48,  12),   S( 32,  33),   S( 52,  -4),
            S( 45,  33),   S( 14,  27),   S( 22,   7),   S( 11,  -7),   S( -7,  -2),   S( 17,  18),   S( 14,   9),   S( 32,   5),

            /* rooks: bucket 12 */
            S( -3, -10),   S( -8, -31),   S(-12, -53),   S( -4, -10),   S(  1,  -3),   S( -4, -34),   S(-22, -65),   S(-24, -54),
            S(  7,   4),   S( -6, -23),   S(-12, -19),   S( -7, -19),   S( -9,  -5),   S( -7, -15),   S(  1,  -1),   S(-11, -33),
            S(  4,  -1),   S( -6, -18),   S( -8, -25),   S(-13,  -7),   S( -4, -21),   S(  6,  -7),   S( -7, -10),   S(  5,  -9),
            S( -7,  -8),   S(  0, -10),   S(  2,  13),   S(  9, -10),   S(  1,  -8),   S( -9, -37),   S( -7, -12),   S( -4, -38),
            S( -3, -11),   S( -2, -18),   S( 12,   4),   S(  8,   7),   S( -8, -34),   S(  7, -17),   S( -5,  -7),   S(  1, -16),
            S( -4,  -9),   S( -5,  -7),   S( 20,  34),   S(  8,  -5),   S( -4,  -5),   S( -7, -20),   S(  1, -25),   S(  5,   9),
            S( -5,  -6),   S(  1, -28),   S(  1, -40),   S( 10,  -1),   S(  7,  -2),   S( -6, -38),   S( -3,  -8),   S(  9, -17),
            S( -5, -43),   S(  7,  23),   S(  3, -20),   S(  1,   1),   S( -4, -24),   S(-12, -49),   S(-14, -30),   S(  7,  -5),

            /* rooks: bucket 13 */
            S(-12, -42),   S( -6, -26),   S( -3, -19),   S(  2,  10),   S(  6,  -4),   S(-11, -37),   S(  2, -22),   S(-18, -32),
            S( -2, -32),   S( -3, -15),   S(-11,  -8),   S( -6,  -1),   S(-10, -20),   S( -1, -11),   S(  5,   2),   S( -5, -23),
            S( -5, -29),   S( -6, -28),   S( -5, -36),   S( -2, -23),   S( 11,  13),   S(  2,  -5),   S(  1, -20),   S(  1, -33),
            S( -6, -51),   S(  3,  -3),   S( -8, -42),   S( -4,  -9),   S( 14,  14),   S( -6, -34),   S( -2, -27),   S(  3, -16),
            S( 11, -21),   S(  9, -18),   S( 17,  26),   S( -5,  -7),   S( -9, -27),   S(  4, -13),   S( -6, -38),   S( 10,  -8),
            S( -7, -39),   S( 10, -27),   S( -8, -12),   S( 13,  -7),   S(  6, -11),   S( 10,  18),   S(  8,  -1),   S(  5,   9),
            S(  5,  -8),   S(  9,  15),   S(  8,   8),   S(  2, -17),   S( 10, -27),   S( 20,   6),   S(  3, -12),   S(  3, -17),
            S(-15, -121),  S(-18, -71),   S(  5,   5),   S(  1,  -1),   S( -5,  14),   S( -4, -31),   S(-10, -26),   S(  5,   1),

            /* rooks: bucket 14 */
            S( -6, -34),   S(-15, -49),   S( -2,  -7),   S( -1, -34),   S(  4, -23),   S( -9, -23),   S( 10,  -8),   S( -6, -22),
            S(-22, -45),   S(-13, -55),   S( -9,   5),   S(-13, -40),   S(-11, -16),   S(  1, -31),   S(  7,  24),   S(  6, -13),
            S( -2, -23),   S( -8, -19),   S( -3, -18),   S( -6, -13),   S(-13, -24),   S( -7, -22),   S(  7,  22),   S( -1, -27),
            S( 12,   5),   S( -7, -32),   S( -3, -19),   S( -5,   7),   S(  3, -11),   S(  4, -13),   S( -5, -35),   S( -3, -22),
            S(  2, -11),   S(  3, -25),   S( -7, -27),   S( -9, -22),   S( -6, -16),   S( -5, -18),   S(  2,   8),   S(  8,   2),
            S(  2, -14),   S(  1, -24),   S(  1, -16),   S(  2, -17),   S(-11, -18),   S( -9,   7),   S(  5,  13),   S(  1,  -5),
            S( 18,  -1),   S(  0, -37),   S(  4, -22),   S(  2, -29),   S(  5, -44),   S(  6,   1),   S(  8,  10),   S(  9,   6),
            S( -2, -22),   S(  3, -15),   S( -9, -28),   S( 10,  12),   S(-10, -19),   S(  3,   8),   S(  4,  14),   S( -1, -16),

            /* rooks: bucket 15 */
            S( -2, -56),   S(-13, -43),   S( -1, -27),   S( -6, -28),   S(  1, -16),   S( -4,  -9),   S(-17, -54),   S( -9, -16),
            S(-14, -21),   S(-13, -28),   S(  2,  -1),   S( -7, -24),   S(-10, -28),   S(  6, -27),   S(-12, -42),   S(  6,   4),
            S( -8, -23),   S(-10, -22),   S( -3, -23),   S(  2,   0),   S( 10, -27),   S( -4, -10),   S( -3,   5),   S( -4, -14),
            S(  2, -31),   S( -3, -25),   S(-11, -17),   S( -5, -17),   S(-11, -19),   S(  3, -18),   S(  0, -18),   S( -9,  -2),
            S(  0, -10),   S( -5, -11),   S( 11,  -5),   S(  0, -11),   S(  1,   0),   S(  2,  -1),   S( -1,   8),   S(  0,  17),
            S(  7,  18),   S(  2,   1),   S(  0, -14),   S(  0, -10),   S( -6,  -9),   S(  1,  17),   S(  5,  -8),   S( -8, -14),
            S( 10,  20),   S( 11,  -4),   S(  8, -32),   S( -4, -32),   S(  1, -20),   S( 11,  34),   S(  1,  -1),   S(  0,  11),
            S(  1, -18),   S( -7, -18),   S(  2,  -6),   S(  1, -11),   S( -7, -14),   S( -1, -25),   S(  1, -16),   S(  1,  -5),

            /* queens: bucket 0 */
            S(-22, -13),   S(-19, -55),   S( 50, -88),   S( 59, -57),   S( 32, -36),   S( 20,  -2),   S( 55,   7),   S( 21,  19),
            S(-11, -13),   S( 34, -61),   S( 41, -17),   S( 23,  10),   S( 26,  32),   S( 26,  22),   S(  8,  64),   S( 36,  21),
            S( 29,   4),   S( 44,  16),   S( 23,  27),   S( 20,  36),   S( 19,  19),   S( 11,  17),   S(  9,  29),   S( 36,  32),
            S( 22,  19),   S( 26,  45),   S(  6,  48),   S(  5,  49),   S(  6,  58),   S( 12,  35),   S( 15,  28),   S( 18,  29),
            S( 41,  49),   S( 30,  42),   S( 19,  41),   S( 18,  57),   S( -8,  31),   S( -7,  13),   S( 32,  22),   S( 45,  -4),
            S( 26,  59),   S( 24,  54),   S( 10,  39),   S( 16,  15),   S( 41, -10),   S(  0,  35),   S( 22,  20),   S( 23, -23),
            S( 44,  49),   S( 50,  42),   S( 28,  37),   S( 46,  25),   S( 20,   6),   S(-12,  -9),   S( 28,  24),   S( 28,   9),
            S( 43,  29),   S( 20,  36),   S( 37,  17),   S( 33,  35),   S( 42,  29),   S(-16,   1),   S( 47,  26),   S( 43,  26),

            /* queens: bucket 1 */
            S(  1, -18),   S(-74, -25),   S(-51, -29),   S(-13, -69),   S(-10, -25),   S(-18, -46),   S( 16, -30),   S( 12,  26),
            S(-15, -26),   S( -9, -44),   S( 10, -47),   S( -2,   0),   S( -6,   2),   S(  6,  -1),   S( 22, -40),   S(  0,  21),
            S(-26,  43),   S(  2,  -4),   S(  6,  11),   S( -4,   6),   S( -4,  33),   S(-14,  32),   S( 15,  12),   S( 19,  22),
            S( 10, -19),   S(-10,  31),   S(-15,  34),   S(  3,  46),   S( -9,  51),   S(  1,  31),   S(  1,  -1),   S( 18,  19),
            S( 15,  10),   S(  7,  26),   S( -1,  62),   S(-25,  65),   S(-17,  53),   S(  0,  15),   S( -8,  17),   S(  3,  35),
            S( 10,  25),   S( 13,  53),   S( 13,  61),   S(-40,  59),   S(-20,  48),   S(-35,  47),   S( 19,  26),   S( 17,  39),
            S(  2,  37),   S(-14,  71),   S(-21,  34),   S(-25,  69),   S(-30,  48),   S( 10,  29),   S( -8,  38),   S(-27,  45),
            S( -5,   7),   S(  6,  18),   S( 14,  26),   S( -9,  13),   S( -3,  15),   S(  6,  15),   S( 11,  25),   S( -7,  30),

            /* queens: bucket 2 */
            S( 10,  18),   S( 15, -35),   S(  9, -19),   S( -2, -15),   S(-16,  -2),   S(-23, -18),   S(-24, -23),   S( 16,   8),
            S( 20,  13),   S( 13,  39),   S( 18, -11),   S( 20, -20),   S( 16, -28),   S( 16, -46),   S( 12,  -5),   S( 34, -26),
            S( 20,  11),   S( 18,  15),   S(  3,  46),   S(  9,  37),   S(  3,  58),   S( 16,  49),   S( 12,  22),   S( 29,  18),
            S(  7,  28),   S(  1,  56),   S( -3,  45),   S(  3,  56),   S(-20,  81),   S( -2,  84),   S( 13,  19),   S(  4,  72),
            S( 17,   7),   S( -7,  59),   S( -8,  59),   S(-30,  96),   S(-35, 108),   S(-14,  78),   S( -6, 103),   S( -5, 106),
            S( 12,  23),   S(  0,  44),   S(-34,  84),   S( -6,  52),   S(-28,  90),   S(-14,  99),   S( -7,  98),   S( 10,  73),
            S(-22,  54),   S(-37,  80),   S(-16,  65),   S(  5,  63),   S(-19,  74),   S( 24,  43),   S(-18,  46),   S(-16,  80),
            S(-67,  76),   S(  0,  39),   S( 29,  39),   S( 28,  35),   S(  1,  65),   S( 17,  33),   S( 12,  27),   S(-12,  40),

            /* queens: bucket 3 */
            S( 84,  88),   S( 58,  91),   S( 49,  99),   S( 44,  81),   S( 68,  32),   S( 46,  21),   S( 21,  20),   S( 42,  54),
            S( 68, 113),   S( 61, 110),   S( 46, 115),   S( 50,  90),   S( 50,  80),   S( 64,  47),   S( 67,   8),   S( 41,  43),
            S( 65,  86),   S( 55, 106),   S( 55,  84),   S( 55,  78),   S( 51,  91),   S( 56,  99),   S( 64, 101),   S( 69,  71),
            S( 49, 122),   S( 61,  85),   S( 47,  96),   S( 38,  98),   S( 40,  96),   S( 38, 130),   S( 60, 101),   S( 54, 131),
            S( 65,  89),   S( 59, 105),   S( 54,  88),   S( 37,  97),   S( 31, 116),   S( 28, 124),   S( 41, 163),   S( 56, 152),
            S( 49, 119),   S( 56,  98),   S( 49,  94),   S( 25, 115),   S( 29, 134),   S( 67, 105),   S( 63, 136),   S( 37, 180),
            S( 60, 113),   S( 59, 101),   S( 68,  84),   S( 56,  95),   S( 29, 112),   S( 55, 111),   S( 87, 125),   S(154,  69),
            S( 73,  89),   S( 96,  77),   S( 68,  88),   S( 70,  85),   S( 29, 110),   S( 99,  57),   S(127,  59),   S(134,  58),

            /* queens: bucket 4 */
            S(-12, -23),   S(-17, -18),   S(-25,  -9),   S( -5,  -8),   S( 13, -14),   S( 36,   1),   S(-31,  -9),   S(-24,  -2),
            S(-32, -20),   S(-30,  -6),   S( 13,  -7),   S(-39,  23),   S(  5,  -5),   S(  1, -12),   S( -5,  -9),   S(-31, -14),
            S(  1,   0),   S( 11,  -1),   S( -2,  28),   S( -2,  32),   S( 25,  17),   S(  6,  -6),   S(  9, -18),   S(-24, -23),
            S(-16,   2),   S( -6,  14),   S(  2,  37),   S( -7,  29),   S( 14,  35),   S( 22,  20),   S(  3, -13),   S( -1,  -5),
            S( -9,  -3),   S( 15,  10),   S( 14,  27),   S( 27,  41),   S( 22,  28),   S( 21,   2),   S(-18, -14),   S( -7, -28),
            S(  2,  12),   S( 34,  12),   S( 24,  54),   S( 23,  45),   S( 12,   9),   S(  4,   5),   S(-14, -12),   S(-10,  -5),
            S(-13, -21),   S( -6,  16),   S(  2,  24),   S( 30,  34),   S(  9,  12),   S(-12,  -2),   S(-19, -40),   S(-19, -26),
            S( -4, -18),   S( -3,  -5),   S( 28,  36),   S(  4,  20),   S(-18, -17),   S( -7, -10),   S(-20, -33),   S( -8, -18),

            /* queens: bucket 5 */
            S(-36, -14),   S(-24, -30),   S(-28, -28),   S(-41, -28),   S(-51, -28),   S( 10, -14),   S( -5,  -4),   S(  1,  -6),
            S(-27,  -4),   S(-40, -15),   S(-66, -23),   S(-64,  -5),   S(-13,  -5),   S(-41, -16),   S(-44, -15),   S(-47, -14),
            S(-34,   3),   S(-61, -14),   S(-65,   1),   S(-37,  27),   S( 16,  51),   S( -6,  24),   S( -1,   1),   S( 16,  23),
            S(-52, -12),   S(-50,  -6),   S( -1,  37),   S( -4,  50),   S( 13,  26),   S( -2,  14),   S(  2,  -5),   S( -2,  17),
            S(-31,  -4),   S(-25,  17),   S(-11,  46),   S( -4,  44),   S( 28,  47),   S(  1,  19),   S(  2,  12),   S(-24, -25),
            S(-14,  17),   S( 10,  37),   S(-12,  42),   S(  2,  44),   S( 39,  50),   S(  6,  15),   S(  4,   4),   S( -9,  -8),
            S( -5,   9),   S( -7,  14),   S(  8,  61),   S( -1,  34),   S(  1,  38),   S( 25,  37),   S( 13,  11),   S(-16, -13),
            S( 10,  26),   S( 13,  13),   S(  4,  19),   S( 13,  50),   S( 17,  30),   S(  6,  24),   S(  1, -23),   S(-17, -13),

            /* queens: bucket 6 */
            S(-21,   9),   S(-46, -20),   S(-63, -26),   S(-74, -59),   S(-87, -50),   S(-69, -44),   S(-48, -42),   S(-26,   2),
            S(-59, -10),   S(-38,   1),   S(-48,  12),   S(-60,  10),   S(-75,  15),   S(-82,  -1),   S(-84, -19),   S(  7,  18),
            S(-37,  13),   S(-13,  12),   S(-48,  39),   S(-95,  86),   S(-41,  49),   S(-40,   0),   S(-47, -14),   S(  1,   5),
            S(-34,  14),   S(-21,  13),   S(-24,  64),   S(-45,  67),   S(  6,  45),   S( 14,  50),   S(-12,  34),   S( 10,  -7),
            S(-47,  23),   S(  1,  40),   S(-23,  55),   S( 11,  30),   S( 30,  54),   S( 61,  36),   S( 23,  32),   S( -4,  20),
            S(-18,  45),   S( -7,  21),   S( 27,  22),   S( 21,  48),   S(  8,  52),   S( 61,  68),   S( -7,  -6),   S(-12,  14),
            S( -4,   8),   S(  7,   6),   S( -6,  45),   S( -8,  37),   S( 30,  52),   S( 22,  64),   S( -8,  25),   S(-35,   1),
            S(  1,   8),   S( 20,  14),   S( 14,  34),   S( -1,  27),   S( 30,  41),   S( 21,  30),   S(  0,  18),   S(  7,  13),

            /* queens: bucket 7 */
            S( -1,  -6),   S(-28,  16),   S(-45,  24),   S(-27,  12),   S(-25,  -8),   S(-29, -25),   S(-29,  -7),   S(-18, -12),
            S(-26,  -8),   S(-40,   5),   S(-18,   6),   S(-16,  36),   S(-26,  32),   S(-40,  38),   S(-41,  22),   S(-37, -15),
            S(-28, -20),   S(-38,  31),   S(-14,  34),   S( -6,  29),   S( 10,  19),   S(  2,  26),   S(-15,  13),   S(-23,  -3),
            S(-53,   1),   S( 14,   4),   S(-11,  24),   S( -1,  40),   S( 33,  20),   S( 30,  26),   S(  8,  38),   S( -4,  19),
            S(-23,  22),   S(-47,  28),   S( 13,  20),   S( 52,  -8),   S( 60, -10),   S( 81, -15),   S( 33,  12),   S( 36,  -7),
            S(-12,  14),   S(-12,  10),   S(  8,  -1),   S( 16,  -9),   S( 36,  36),   S( 76,  21),   S( 64,   2),   S( 37,  10),
            S( 12, -17),   S(  6,  10),   S(  4,  -5),   S(  5,  14),   S( 35,  18),   S( 51,  37),   S( 51,  19),   S( 47,  23),
            S( 15,   5),   S( 19,   4),   S( 20,  10),   S( 17,  17),   S( 37,  26),   S( 20,  20),   S( 13,   5),   S( 34,  43),

            /* queens: bucket 8 */
            S( -5, -10),   S( -1,   4),   S(-13,  -4),   S( -8,  -6),   S( -4,   1),   S( -2, -16),   S(-19, -24),   S( -3,   5),
            S( -6,   1),   S(-12, -15),   S( -4,   5),   S(-11,  -1),   S( -3,  -2),   S(-16, -19),   S(-18, -38),   S( -4,  -8),
            S( -2,  -1),   S( -5,   2),   S( -7,   2),   S( -4,  -9),   S( -2,   5),   S(-10, -10),   S(-11, -25),   S(-14, -26),
            S( -4,   2),   S(  9,  18),   S( 11,  19),   S(  5,  10),   S( -3,   1),   S( -5,   0),   S(  0,  -1),   S( -6, -20),
            S( 15,  27),   S(  2,  26),   S( 10,  13),   S(  9,  16),   S( 12,  33),   S(  5,   1),   S( -7,  -7),   S(-10, -18),
            S(  7,  18),   S( 10,  20),   S(-20,  13),   S( 14,  34),   S( -9, -15),   S( -5, -11),   S(  4,   3),   S(  4,  13),
            S( -7, -13),   S(-18, -26),   S( 21,  33),   S( 13,  15),   S(  2,  17),   S(  2,  18),   S( -3,  -7),   S( -6, -15),
            S(-15, -29),   S( 14,  10),   S(-17, -48),   S(-10,  -5),   S(-11, -28),   S( -2,  -5),   S( -3, -17),   S( -5,  -6),

            /* queens: bucket 9 */
            S(  6,   8),   S(-12, -26),   S(  2,  -1),   S(-28, -30),   S(-22, -37),   S(-16, -29),   S(-13, -22),   S(-12, -17),
            S( -2,  -4),   S( -8,  -6),   S(-17, -22),   S( -2,   1),   S(-15,  -7),   S(-15, -19),   S(  2,  -2),   S( -3,  -6),
            S(  5,   6),   S(  4,   9),   S( -7,  22),   S( -3,  -5),   S( -5,   8),   S(  3,   0),   S(  5,   4),   S(  5,   2),
            S( -5, -10),   S( -5,   5),   S( 14,  40),   S(  8,  22),   S( 19,  31),   S(  4,  11),   S( -7, -15),   S(  2,  -8),
            S(  5,   9),   S(  8,  30),   S( 11,  31),   S( 17,  50),   S( 21,  33),   S( 11,  20),   S( -3,   7),   S(-10, -10),
            S(-18, -19),   S(-17,  -5),   S(  5,  20),   S( 15,  34),   S( -4,   2),   S( -1,  10),   S( -8,  -5),   S( -5,  -6),
            S( -5, -16),   S(-10, -25),   S(-10,  21),   S( 10,  27),   S( 16,  22),   S(  6,  -6),   S(  6,  -3),   S(-10, -25),
            S(  0,   0),   S( -3, -23),   S( 12,  -2),   S(  1,  15),   S( 13,   2),   S( -2,   0),   S( 12,   3),   S(  3, -15),

            /* queens: bucket 10 */
            S(  3,   0),   S( -2,   4),   S(-10, -17),   S(-20, -23),   S(-11, -14),   S( -6,  -5),   S(  3, -10),   S( -4,  -8),
            S( -7, -11),   S( -8, -15),   S(-12, -23),   S( -7, -10),   S( -5,  -7),   S(-18, -13),   S(  1,  -8),   S(-16, -17),
            S(  0, -11),   S( -8, -13),   S( -6,  -7),   S( -1,   2),   S( -6,   2),   S( -6,   4),   S(  2,   1),   S(  3,   7),
            S(  0,  -2),   S(  3,  -2),   S( -1,  -5),   S(  1,  31),   S( 16,  25),   S( -5,   5),   S( -2,  -6),   S(-13, -19),
            S( -4,  -6),   S(  7,  -4),   S( -4,   4),   S( 21,  47),   S(  1,  -4),   S( 17,  30),   S( 12,  13),   S(  1,   4),
            S( -3,  -4),   S(-19, -32),   S( -4,  -2),   S(  1,  12),   S(  5,  16),   S(  5,  21),   S( 11,   7),   S( -5, -11),
            S( -4,  -3),   S(-16, -27),   S(  8,  22),   S( -6,  -7),   S(  7,   7),   S(  3,   8),   S( -3,  -8),   S( -8,  -6),
            S(  7,   1),   S( -1, -17),   S(  7,  -2),   S(  7,  -5),   S( 17,  15),   S(  5,   5),   S( 15,  16),   S(  2,  -9),

            /* queens: bucket 11 */
            S(-10, -14),   S( -7, -19),   S(-21, -19),   S( -9, -27),   S(-12, -18),   S( -9, -11),   S( -5,  -5),   S(-12, -23),
            S(-16, -32),   S( -8,  -7),   S(-39, -34),   S( -9,  -9),   S(-13, -10),   S(-10,  -6),   S( -5,  -9),   S( -6,  -3),
            S(-16, -20),   S(-14, -33),   S(  5, -19),   S( -8, -15),   S( -9, -15),   S( -4,   4),   S(  6,  19),   S(-11,  -7),
            S(-15, -27),   S(-24, -24),   S( -6, -24),   S( 16,  26),   S( 12,   1),   S(-11,  -5),   S( 24,  25),   S( -2,   1),
            S(-13, -12),   S( -4, -16),   S(-20, -24),   S( 24,  24),   S( 15,  14),   S( 27,  49),   S( 20,  40),   S(  2,  11),
            S(-13, -29),   S(  3,   4),   S(-16, -17),   S( 15,  11),   S( 24,   5),   S( 43,  34),   S(  8,  -1),   S( -8,  -7),
            S( -7,  -3),   S(-13, -22),   S(  9,  16),   S(-12,  -4),   S(  5,   6),   S( 22,  23),   S( 36,  37),   S( -3, -18),
            S(-10, -21),   S( -9, -23),   S( -6, -20),   S(  4, -14),   S(  2,  10),   S( -3,  -9),   S( 17,   7),   S( -2, -33),

            /* queens: bucket 12 */
            S(  6,   0),   S( -1,  -1),   S(  2,   1),   S( -8,  -5),   S(-10, -12),   S( -1,  -3),   S(  0,  -2),   S( -4,  -9),
            S( -3,  -2),   S( -8, -14),   S( -9, -12),   S( -5, -10),   S( -2,  -2),   S( -6,  -2),   S( -1,  -9),   S( -5,  -9),
            S( -2,  -5),   S( -6, -10),   S( 12,  13),   S( -4,  -4),   S( -2,  -5),   S( -8, -13),   S(-12, -24),   S( -8,  -7),
            S(  2,   6),   S( -1,   2),   S(  3,   5),   S(  0,   7),   S(  8,  15),   S(  0,  -3),   S(  0,  -4),   S( -4, -10),
            S(  1,  -4),   S( 10,  12),   S( 31,  56),   S(  1,  16),   S( -5,   7),   S(  1,   7),   S(-13, -29),   S( -2, -14),
            S(  8,  17),   S( 13,  24),   S( 33,  42),   S( -3,   7),   S(  0,   5),   S(  2,   2),   S(  5,   5),   S( -4, -15),
            S(  3,   1),   S(  2,   6),   S( 16,  15),   S( 11,   8),   S(  5,   9),   S( -4,   4),   S(  9,   6),   S( -4,  -4),
            S( -5, -29),   S( -9, -26),   S(-12, -20),   S(-10, -28),   S( 10,  -7),   S(  1,  -1),   S(  1,  -6),   S( -7, -12),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -4, -14),   S(  1,  -4),   S( -2,  -7),   S( -3, -10),   S( -2,  -1),   S( -7,  -9),   S( -6,  -8),
            S(  4,  10),   S(  5,  14),   S(  5,  11),   S( -3,  -2),   S( -6,  -6),   S(  2,  11),   S(  1,   5),   S(-10, -19),
            S( -2,  -7),   S(  1,   0),   S(  3,  17),   S(  4,  12),   S( -1,   0),   S( -6,  -8),   S( -4, -11),   S(-12, -16),
            S( -3,  -4),   S(  2,   3),   S( 13,  13),   S( 20,  28),   S( 15,  32),   S( -3,  -7),   S( -5, -13),   S( -5,  -6),
            S( -3,  -3),   S(  6,  17),   S( 15,  41),   S( 12,  37),   S( 23,  44),   S(  0,  -7),   S( -4,  -6),   S( -7, -14),
            S(  0,   0),   S( 12,  31),   S( 37,  73),   S( 18,  40),   S(  1,  16),   S(  1,   8),   S(  6,  15),   S( -5, -14),
            S( -1,   0),   S( 18,  30),   S(  8,  26),   S( 13,  26),   S( -2,   9),   S(  1,  -8),   S( -1,  -9),   S(  5,   7),
            S(-12, -18),   S(  3,  -5),   S( -2,  -8),   S( -9, -12),   S(  6,   0),   S(  4,   7),   S( -7,  -7),   S( -6, -12),

            /* queens: bucket 14 */
            S( -1,  -1),   S(  0,   2),   S( -2,  -7),   S( -9,  -8),   S(  4,   7),   S( -2,  -4),   S( -1,  -8),   S( -4, -10),
            S( -5,  -7),   S(  5,  16),   S( -1,  -2),   S(  0,  -6),   S( -8, -11),   S( -6, -14),   S( -5,  -3),   S( -2,  -6),
            S( -1,  -2),   S( -9, -12),   S( -5, -10),   S(  1,   0),   S(  2,   0),   S(  1,  -4),   S(  3,   6),   S( -6, -14),
            S( -7,  -8),   S(  8,  10),   S( -4,  -3),   S( 23,  41),   S( 15,  15),   S( -1,   6),   S( 11,  24),   S(  1,  -3),
            S(  4,  14),   S(  4,   1),   S(-12,  -7),   S( 16,  27),   S( 13,  33),   S( 17,  24),   S(  9,  18),   S( -4, -10),
            S( -2,  -5),   S(  5,  15),   S( 14,  24),   S( 12,  20),   S( 17,  40),   S( 14,  44),   S(  7,  16),   S( -3,  -9),
            S(  3,   7),   S(  8,  10),   S( 16,  36),   S( 19,  33),   S( 15,  32),   S( 13,  26),   S( 16,  28),   S(  1,   5),
            S( -4,  -1),   S( -1,   0),   S(-10, -14),   S( 12,  18),   S(  0,   3),   S(  2,  -1),   S(  1,   5),   S(-11, -17),

            /* queens: bucket 15 */
            S( -1,  -2),   S(  0,  -5),   S( -5,  -8),   S( -3, -10),   S( -5, -10),   S( -5, -12),   S(-11, -24),   S(  0,  -7),
            S( -1,  -5),   S( -4,  -9),   S( -5, -11),   S( -4, -11),   S(  1,   7),   S( -3,  -8),   S( 11,  14),   S(  3,   1),
            S(  0,  -8),   S( -3, -11),   S( -1,  -2),   S( -4, -11),   S( -4, -11),   S(  6,  17),   S( -1,  -4),   S(  0,  -8),
            S( -5,  -8),   S(  4,   5),   S( -3,  -2),   S(  4,   2),   S(  1,  10),   S(  0,   6),   S(  5,   5),   S(  3,   4),
            S( -2,  -7),   S( -2,  -5),   S( -8, -12),   S( -4,  -4),   S(  5,  12),   S(  7,   6),   S( -4,  -7),   S(  0,  -7),
            S( -3,  -7),   S( -3,  -7),   S( -1,   2),   S(  0,   0),   S( -2,  -5),   S( 19,  29),   S(  3,  -1),   S( -1,  -9),
            S( -6, -13),   S(  4,  -5),   S(  5,   7),   S(  7,   7),   S(  6,   8),   S( 21,  37),   S( 11,  19),   S(  4,   5),
            S(  1,  -4),   S( -5,  -5),   S( -2,  -4),   S( 10,  12),   S(  7,   4),   S(  2,  -3),   S( -3,  -8),   S( -7, -22),

            /* kings: bucket 0 */
            S( 51,  13),   S( 38,  60),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 32,  45),   S(112,  67),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 1 */
            S(  0,   0),   S(  0,   0),   S( 39,  25),   S(-11,  42),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 87,  48),   S( 69,  59),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-12,  43),   S( -6,  33),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 60,  63),   S( 63,  47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10,  60),   S(-35,  28),
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
            S(-40,  -6),   S( 41,  15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 43, -11),   S( 29,   4),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 34,  30),   S( 25,  26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 52,  16),   S( 15,  13),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 39,  45),   S( 21,  41),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 75,  29),   S( 12,  -8),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 61,  59),   S( -7,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61, -119),  S( 19, -62),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-51, -103),  S(-92, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 11, -55),   S(-30, -47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-42, -33),   S(-51, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-13, -38),   S(-14, -40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-47, -21),   S(-89,   3),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10, -43),   S(-48, -105),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-79, -10),   S(-18, -88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -107),  S(-77, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -223),  S(-16, -96),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-64, -60),   S( 23, -65),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-57, -75),   S(-25, -99),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-14, -54),   S(-109, -19),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18, -114),  S(-65, -68),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-123,   0),  S(-31, -112),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-50, -70),   S( -4, -225),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -4, -22),   S(-23,  15),   S( 13,  -1),   S( -9,  28),   S( 22,   1),   S( 48,   6),   S( 54,  -9),   S( 52,   2),
            S( -7, -28),   S(-27,   2),   S(  2, -11),   S(  2, -12),   S( 19,   2),   S(  5,  12),   S( 29,  -1),   S( 24,  26),
            S(  6, -29),   S( -2, -23),   S( 30, -35),   S( 12, -17),   S( 19,  -8),   S(  8,  29),   S( -5,  48),   S( 29,  24),
            S(  9, -17),   S( 25,   0),   S( 50, -29),   S( 34,  -5),   S( 17,  45),   S(-12,  85),   S( 11,  87),   S( 57,  65),
            S( 90, -51),   S(115, -16),   S( 85, -21),   S( 45,  17),   S( 49, 135),   S(  8, 135),   S( 19, 154),   S( 69, 133),
            S(-220, -72),  S(-130, -135), S( 12, -169),  S( 35,  43),   S( 88, 197),   S( 74, 187),   S(113, 168),   S(102, 147),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37,  20),   S(-39,  26),   S(-15,  12),   S(-37,  58),   S(-10,   4),   S( 20,   9),   S( 21,   0),   S( 18,  27),
            S(-48,  16),   S(-45,  19),   S(-28,   9),   S(-17,   8),   S(  4,   6),   S(-10,   9),   S( -4,   2),   S(-13,  23),
            S(-46,  22),   S(-21,  18),   S(-26,   4),   S(  5,  -9),   S( -2,  18),   S(-25,  19),   S(-33,  31),   S(-15,  30),
            S(-38,  43),   S(  5,  25),   S(-24,  24),   S(  6,  26),   S(  0,  28),   S(-35,  45),   S(  1,  38),   S( 29,  56),
            S(  4,  36),   S( 52,   3),   S( 86, -24),   S( 78, -20),   S( 36,  30),   S(  8,  35),   S(-25,  79),   S( 44,  90),
            S( 41,  43),   S(-38, -20),   S(-15, -101),  S(-19, -98),   S(-38, -68),   S( -4,  47),   S( 48, 185),   S( 70, 214),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38,  41),   S(-24,  23),   S(-16,  15),   S(  2,   6),   S(-22,  34),   S( -7,  13),   S(  9,  -9),   S( -3,  22),
            S(-46,  31),   S(-35,  27),   S(-27,   9),   S(-24,  18),   S(-20,  17),   S(-29,   7),   S(-12, -10),   S(-34,  13),
            S(-43,  48),   S(-36,  49),   S(-12,  15),   S(-14,  17),   S(-19,  19),   S(-27,   3),   S(-32,   7),   S(-32,  12),
            S(-29,  88),   S(-37,  72),   S(-16,  42),   S( -1,  34),   S(-10,  32),   S(-28,  17),   S(  1,  18),   S( 21,  13),
            S(-24, 132),   S(-42, 117),   S(  0,  23),   S( 23, -21),   S( 90,  -8),   S( 85,  -6),   S( 67, -15),   S( 47,   5),
            S( -7, 247),   S( 40, 176),   S( 14,  72),   S( 25, -89),   S(-24, -170),  S(-88, -133),  S(-30, -64),   S( 10,  22),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  8,  15),   S(  5,  14),   S( 13,  12),   S(  4,  33),   S(  0,  50),   S( 36,  21),   S( 26,  -3),   S(  9, -10),
            S(  2,  17),   S(  3,  24),   S(  2,   9),   S(  1,   9),   S( 13,  16),   S( 19,  -1),   S( 11, -10),   S(-17,  -3),
            S(  4,  36),   S( -6,  56),   S( 10,  20),   S(  9,   0),   S( 26, -11),   S( 12, -12),   S(  0, -21),   S(-17,  -9),
            S(  4,  92),   S(-13, 103),   S( 13,  65),   S( 21,  30),   S( 25,   2),   S( 30, -24),   S( 15,   4),   S( 32, -17),
            S(  3, 157),   S( -7, 166),   S(-19, 167),   S( -3, 111),   S( 37,  53),   S( 85, -12),   S(101, -32),   S( 96, -36),
            S(104, 127),   S( 50, 240),   S( 30, 253),   S( 12, 207),   S(-23,  95),   S( 27, -175),  S(-78, -239),  S(-160, -177),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 78,  -3),   S( 28,   2),   S(  9, -13),   S(-11,  -9),   S(  5, -13),   S( 10, -12),   S(  6, -11),   S(-54,  44),
            S( 38,  -3),   S(-14,  24),   S( -7,  -1),   S(-20,  -5),   S(-26, -21),   S(-12, -17),   S(-27, -21),   S(-42,   5),
            S( 56, -11),   S( 70, -20),   S( 15, -12),   S(-40,   1),   S(-72,  10),   S( -7,   2),   S(-69,  22),   S(-62,  33),
            S(-90, -71),   S(-23, -93),   S( 68, -59),   S(-34,   5),   S(-23,  15),   S(-49,  60),   S(-21,  51),   S(-43,  75),
            S(-40, -72),   S(-67, -115),  S(-12, -93),   S( 53,   6),   S( 77,  87),   S(  7,  98),   S( 24,  73),   S(  9,  98),
            S(  2, -63),   S(-17, -78),   S( -1, -67),   S(  2,  47),   S( 59,  85),   S( 69, 149),   S( 46, 155),   S( 62, 125),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-53,  45),   S(-38,  42),   S( -1,  21),   S( 58,   5),   S( 68,  -2),   S( 13,   3),   S(-17,  10),   S(-53,  49),
            S(-84,  42),   S(-55,  44),   S(-43,  28),   S(-28,  27),   S(-40,  27),   S(-38,  11),   S(-60,   8),   S(-73,  36),
            S(-52,  33),   S(-63,  61),   S(-14,  38),   S(-30,  50),   S(-53,  50),   S(-80,  36),   S(-70,  34),   S(-61,  45),
            S(-43,  42),   S(-33,  15),   S(-49, -35),   S(-10, -25),   S(-11,  -6),   S(-60,  32),   S( -5,  27),   S(-23,  55),
            S( 47,  10),   S(-24, -30),   S( 11, -91),   S( -9, -72),   S( 40, -41),   S( 21,  20),   S(-15,  67),   S(-30, 114),
            S( 47,  32),   S( 17, -13),   S(-33, -68),   S(-23, -62),   S(-32, -59),   S( 47,  39),   S( 67, 135),   S( 42, 149),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-79,  40),   S(-48,  18),   S(-13,   3),   S( 22,   2),   S( 14,  24),   S( 21,  10),   S( 17,   6),   S(  6,  26),
            S(-70,  24),   S(-56,  15),   S(-53,  10),   S( -1,  14),   S(-35,  31),   S(-34,  18),   S(-39,  17),   S(-17,  15),
            S(-57,  34),   S(-75,  42),   S(-63,  34),   S(-62,  49),   S(-28,  48),   S(-27,  27),   S(-27,  29),   S(-30,  23),
            S(-82,  86),   S(-56,  56),   S(-38,  33),   S(-21,  15),   S(-22, -32),   S(-32, -27),   S(-38,   9),   S( 17,   2),
            S( -6, 101),   S(-42,  70),   S( 23,  11),   S(-23, -29),   S(-14, -71),   S(-55, -66),   S(-24, -31),   S( 72,  -4),
            S( 82,  77),   S( 71,  89),   S( 45,  24),   S( 35, -80),   S( -9, -104),  S(-43, -54),   S( -8, -47),   S( 76,   1),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-47,   3),   S(-32, -15),   S(  3, -23),   S(-59,  46),   S( 25,   5),   S( 71, -20),   S( 58, -26),   S( 74, -11),
            S(-53,   3),   S(-55,   3),   S(-30, -20),   S(-38,   3),   S( -8,   0),   S( 28, -26),   S(  1,  -8),   S( 43, -14),
            S(-54,  25),   S(-72,  37),   S(-36,   5),   S(-44,   2),   S(-13,   1),   S(  3, -11),   S( 17,  -3),   S( 40, -12),
            S(-51,  61),   S(-82,  78),   S(-47,  57),   S(-33,  33),   S(-18,  -1),   S( 37, -59),   S(  0, -68),   S( 21, -106),
            S( 20,  61),   S(-53, 133),   S(  6, 115),   S( -6,  85),   S(  8,  21),   S(  7, -80),   S(-52, -133),  S(-30, -96),
            S(132,  84),   S( 85, 124),   S( 96, 105),   S( 64,  93),   S( 33,   3),   S(  1, -104),  S(-30, -93),   S(-12, -183),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 19,   8),   S(-10,  12),   S( 44,  -3),   S(-10, -37),   S(-29, -61),   S(-16, -27),   S( 14, -55),   S( 36, -38),
            S(  9, -57),   S( -5, -12),   S(-40, -57),   S(-59, -37),   S(-24, -60),   S( 40, -65),   S( 10, -64),   S( -1, -51),
            S( 18, -93),   S(  5, -58),   S(-14, -64),   S(-41, -55),   S(-27, -31),   S( 17, -43),   S(-37, -21),   S(  1, -28),
            S(  2, -26),   S(-29, -39),   S( 14, -23),   S(-13,  -5),   S(-16,   6),   S(  5,  18),   S(  0,  23),   S( -3,  22),
            S( 27,   6),   S(  2, -32),   S(  9,  43),   S( 35,  92),   S( 54, 119),   S( 32, 118),   S( 15,  94),   S(-28, 104),
            S( 19,  33),   S(  7,  52),   S( 24,  69),   S( 31, 100),   S( 46,  95),   S( 50, 147),   S( 39, 100),   S(-20,  94),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 16,  12),   S( 12,  25),   S(  8,  17),   S(  1,  25),   S( 15,   1),   S(  2, -15),   S( 26, -44),   S(-20, -16),
            S( 43, -54),   S( -4, -45),   S( -6, -54),   S(-30, -45),   S(-36, -25),   S(-52, -28),   S(-53, -31),   S( 19, -43),
            S(-20, -40),   S(-45, -41),   S(-39, -72),   S(-74, -41),   S(-14, -34),   S(-18, -46),   S(-55, -34),   S( 18, -31),
            S(-46,   0),   S(-50, -52),   S(-12, -70),   S(-45, -31),   S( -1, -44),   S(  0, -28),   S( 18, -11),   S(  6,   7),
            S(  6,  12),   S( -4, -22),   S(-16,   4),   S( 20,  27),   S( 18,  59),   S( 20,  51),   S(  5,  64),   S(  3,  63),
            S( -8,  67),   S( 27,  61),   S( -2,  58),   S( 22,  62),   S( 26, 108),   S( 17,  83),   S( 16,  78),   S( 17,  81),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-29, -49),   S( -8, -45),   S( -8, -19),   S( -5, -14),   S( 28,  15),   S( 44,  13),   S(  3,   7),   S( -5, -14),
            S( -8, -58),   S(-67, -40),   S(-19, -50),   S( 12, -38),   S(-26, -25),   S(-30, -19),   S( -3, -33),   S(  7, -40),
            S(-16, -46),   S(-91, -22),   S(-68, -40),   S(-28, -29),   S(-34, -46),   S(-30, -61),   S(-41, -58),   S( 56, -66),
            S(-34,  -1),   S(-19,  -8),   S(-26, -35),   S(-58, -42),   S( -4, -69),   S(-50, -55),   S(-21, -56),   S( 20, -51),
            S( 12,  16),   S( 32,  15),   S( 20,  11),   S(-19,  -3),   S( 12,  19),   S( 13,  13),   S(-26,   5),   S( 44,  -5),
            S(  8,  24),   S(  2,  48),   S( 26,  54),   S(  7,  60),   S( 24,  80),   S(  1,  45),   S(-13,  19),   S( 26,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-35, -44),   S(  2, -45),   S(-30, -43),   S(  5,  -4),   S(  1, -23),   S( 62,   8),   S( 30,  -5),   S( 51,  -7),
            S(-33, -60),   S(-47, -61),   S(-29, -74),   S(  6, -64),   S(-29, -31),   S( 12, -48),   S(  5, -39),   S( 38, -67),
            S(-16, -40),   S(-86,  -5),   S(-25, -25),   S( -8, -29),   S(-60, -46),   S( 31, -66),   S( 21, -121),  S( 71, -98),
            S(-48,  21),   S(-68,  31),   S(  6,  23),   S( 20, -12),   S(-29, -17),   S(-22, -49),   S(-36, -56),   S( 38, -96),
            S(-13,  19),   S(-15,  66),   S( -9,  92),   S( 20,  58),   S( 28,  58),   S( -6,   3),   S(  1,   5),   S(  9, -25),
            S( 16,  69),   S( 27,  56),   S( 31,  79),   S( 26,  80),   S( 12,  62),   S( 34,  79),   S( 12,  32),   S( 27,  36),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11, -101),  S( 28, -51),   S( -4, -29),   S(  0,  -2),   S( -6, -30),   S(-33, -72),   S( 17, -46),   S(  6, -45),
            S( 37, -87),   S( 26, -49),   S(-24, -77),   S(-32, -62),   S(-31, -89),   S( -8, -64),   S( -9, -90),   S(-18, -67),
            S( -7, -61),   S( -9, -81),   S(-22, -96),   S(-24, -86),   S( -8, -56),   S( -3, -47),   S(-36, -59),   S( -5, -77),
            S(-12, -36),   S( -3, -15),   S(-19, -22),   S( -2,  -1),   S( 18,  57),   S(  5,  39),   S(  6,  11),   S( -5,  -4),
            S( 12,  23),   S(  1,  15),   S(  3,  22),   S( 20,  62),   S( 30,  76),   S( 27,  88),   S( 13,  79),   S( 20,  52),
            S( 13,  30),   S(  2,  35),   S( 13,  52),   S( 12,  61),   S( 25, 102),   S( 24,  92),   S(-20, -22),   S(-13,   3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-19, -61),   S( 21, -81),   S( 19,   4),   S( -2, -12),   S(  4, -22),   S(-33, -38),   S(-11, -74),   S(-14, -68),
            S( 26, -132),  S( 17, -104),  S( -3, -88),   S(  8, -14),   S(-26, -55),   S(  2, -83),   S(  0, -92),   S(  4, -87),
            S( 31, -87),   S( -8, -77),   S( -3, -92),   S(  6, -61),   S(-42, -29),   S( 23, -75),   S( -1, -75),   S( 61, -88),
            S( 18, -25),   S(  2, -35),   S(  2, -30),   S( -3,  24),   S( 15,   6),   S(-13,   7),   S(-12, -16),   S(  9, -21),
            S( -3,  42),   S( 10,  26),   S( -2,   5),   S( 22,  55),   S( 37,  79),   S( 28,  87),   S( 13,  93),   S( -6,  55),
            S( 12, 103),   S( 30,  51),   S(  4,  35),   S( 12,  44),   S( 20,  65),   S( 10,  51),   S( -4,  39),   S(  3,  66),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -117),  S(  3, -70),   S( -7, -43),   S(  2,   2),   S( -6, -14),   S( -3,   0),   S( 17, -70),   S(-13, -38),
            S( 18, -113),  S(-36, -106),  S( -7, -83),   S(-29, -90),   S(-12, -59),   S( 15, -56),   S(  2, -67),   S( 22, -86),
            S( 18, -94),   S(-20, -78),   S(-13, -65),   S(  5, -75),   S(-22, -51),   S(  4, -92),   S(  4, -102),  S( 39, -60),
            S(  5, -32),   S(-21, -41),   S( -4,  -6),   S(-20, -11),   S( 14, -54),   S( -3, -28),   S( 13, -30),   S( 14,  -5),
            S(-14, -15),   S(  6,  43),   S( 11,  52),   S( -8,  15),   S( 20,  69),   S(  4,  15),   S( 18,  45),   S( 22,  66),
            S( -5,  33),   S(  8,  49),   S( 27,  73),   S( 21,  72),   S( 16,  60),   S(  2,  35),   S( 24,  85),   S( 23,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -27),   S(  4, -59),   S(-25, -55),   S(-10, -26),   S(-12, -29),   S(-18, -41),   S( -8, -56),   S(  3, -83),
            S(-22, -65),   S(-18, -98),   S(-15, -106),  S( -9, -37),   S(-21, -28),   S( -7, -35),   S(  8, -57),   S( 11, -107),
            S(-24, -46),   S(-33, -61),   S(-42, -55),   S(  8, -40),   S(-30, -39),   S( -6, -75),   S(  4, -48),   S(  6, -43),
            S( 10, -35),   S(-25, -16),   S( -2,  39),   S(-18,  12),   S( 11,   6),   S( -8, -22),   S( -5, -14),   S( -5,  33),
            S(  7,  47),   S(  2,  51),   S(  1,  69),   S( 12,  61),   S( 25,  79),   S( 12,  63),   S( 17,  56),   S( 10,  21),
            S(-22,   7),   S( -7,   4),   S( 10,  71),   S( 20,  54),   S( 21,  69),   S( 19,  58),   S( 11,  36),   S( 16,  24),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-75, -31),   S(-26, -27),   S(-17,  -5),   S( -8,  24),   S(-16, -24),   S(-25,   0),   S( -5, -29),   S(-75, -41),
            S( 14, -37),   S(  1,   0),   S(-21, -31),   S( -8, -10),   S( -8,  -6),   S( -7, -23),   S(-33, -47),   S(-28, -36),
            S(-18, -21),   S( 21, -33),   S(  1,   8),   S( 35,  23),   S( -8,  10),   S(  8,  -5),   S(-29,  21),   S(-24, -35),
            S( 10,  23),   S( 32,  49),   S( 29,  30),   S( 38,  16),   S( 27,  18),   S( 11,  21),   S( 37, -20),   S( -9, -26),
            S( 57,  41),   S( 21,  53),   S( 61,  59),   S( 58,  40),   S( 64,  30),   S( 16,  20),   S( 18, -10),   S(  6,  -3),
            S( 91, -40),   S( -4,  44),   S(132,  -4),   S( 70,  41),   S( 53,  46),   S(-38,  64),   S( 35, -12),   S(-21,   4),
            S( 48,  -7),   S( -2, -20),   S( 41,  23),   S( 83,  70),   S( 40,  27),   S( 10,  32),   S( -9,   5),   S(-45,   1),
            S(-110, -122), S( -1,   0),   S(  6,   4),   S( 19,  23),   S(  4,  32),   S( 19,  12),   S(-32,  -4),   S( -9,  11),

            /* knights: bucket 1 */
            S( 18,  -4),   S(-52,  19),   S(-21,   9),   S(-32,  31),   S(-14,  34),   S(-17, -20),   S(-25,  -7),   S(  7, -21),
            S(-37,  31),   S(-45,  54),   S(-25,  25),   S(-14,  21),   S(-18,  19),   S( -3,  22),   S(-10,  -4),   S(-15, -53),
            S(-35,  26),   S(  0,   1),   S(-18,  17),   S( -5,  50),   S(-10,  36),   S( -6,   8),   S(-38,  29),   S(-12,  20),
            S(-13,  66),   S( 28,  33),   S( -5,  50),   S( -7,  61),   S( -6,  55),   S( -5,  51),   S(  1,  21),   S(-23,  46),
            S( 61,  -4),   S( 12,  17),   S( 45,  57),   S( 19,  49),   S( 40,  45),   S( -2,  63),   S( -5,  46),   S( -3,  58),
            S( 23,  23),   S( 52, -17),   S( 71,  17),   S( 84,  24),   S( 70,  21),   S(-27,  74),   S( 17,  28),   S(  1,  39),
            S( 13,   1),   S( 32,  -5),   S( 30, -10),   S( 26,  50),   S( 10,  36),   S( -1,  24),   S( 16,  72),   S(-30,  42),
            S(-152, -36),  S( 16, -15),   S(-36, -61),   S(-19,  11),   S( -4,  11),   S( 39,  48),   S( 17,  44),   S(-67,  23),

            /* knights: bucket 2 */
            S(-60,   8),   S(-33,  30),   S(-22,   3),   S(-10,  19),   S(-12,  12),   S(-49,   3),   S(-23,   5),   S(-15, -28),
            S(-15,   5),   S( -1,  32),   S(-20,   9),   S(-16,  17),   S(-24,  24),   S(-14,   6),   S( 11,   5),   S(-29,   2),
            S(-31,  47),   S(-21,  22),   S(-18,  17),   S(-15,  54),   S(-16,  43),   S(-18,  10),   S(-21,  15),   S(  1,  -5),
            S( -3,  47),   S( -3,  37),   S(-22,  71),   S(-11,  71),   S(-30,  71),   S(  7,  44),   S( 12,  31),   S( -1,  35),
            S( -5,  57),   S(-17,  66),   S(  4,  64),   S( 19,  57),   S(  3,  64),   S( 18,  68),   S( -1,  55),   S( 20,  12),
            S(-41,  66),   S(-19,  49),   S(-11,  83),   S( 38,  26),   S( 38,  26),   S(113,  -5),   S( 65,   2),   S( 29, -12),
            S( 31,  36),   S(-42,  60),   S( 43,  25),   S( 27,  11),   S( -8,  49),   S( 13,  -4),   S( 23,  25),   S( 19,  -4),
            S(-54,  28),   S( 29,  62),   S(-14,  69),   S(-12, -24),   S(-23,  -9),   S(-33, -46),   S( 17,  -2),   S(-121, -56),

            /* knights: bucket 3 */
            S(-47,  17),   S( -8, -52),   S(  4, -20),   S(  6, -10),   S(  8, -16),   S( -4, -25),   S(-14, -25),   S(-23, -74),
            S(-14, -32),   S(  4,  -7),   S( 11, -13),   S( -1,  -2),   S( -2,  -2),   S( 22, -19),   S( 27, -40),   S( 24, -58),
            S(-11,  -3),   S(-10,   5),   S(  6,  16),   S( 11,  40),   S( 16,  27),   S(  2,  16),   S( 15,  -1),   S( 22, -33),
            S( 12,  -4),   S( 17,  21),   S( 19,  41),   S( 13,  47),   S( 16,  68),   S( 31,  54),   S( 37,  45),   S( 17,  34),
            S( -1,  39),   S( 24,  29),   S( 26,  49),   S( 29,  76),   S( 29,  74),   S( 39,  82),   S(  8,  90),   S( 65,  77),
            S( -8,  27),   S(  6,  40),   S( 14,  56),   S( 25,  71),   S( 59,  74),   S(134,  61),   S( 62,  74),   S( 22,  91),
            S(-22,  37),   S(-13,  46),   S( -9,  61),   S( 31,  62),   S( 47,  65),   S( 95,  46),   S( 14,   1),   S( 83,  20),
            S(-147,  35),  S(-30,  74),   S(-45,  84),   S( 35,  50),   S( 58,  79),   S(-50,  71),   S(-28, -43),   S(-62, -106),

            /* knights: bucket 4 */
            S( 10,  12),   S( -8,  -8),   S(-47,  17),   S(-28, -10),   S(-28,  23),   S(-12, -10),   S( 20, -27),   S(-18, -16),
            S( 21,  37),   S(  8, -20),   S( -3,  12),   S( -5,   6),   S(  0, -10),   S( 19, -42),   S( -7,  12),   S(-45,  -4),
            S( -4, -16),   S( 14,  -1),   S( 52,   7),   S( 61,   7),   S( 15,  20),   S( 43, -30),   S( -7, -24),   S(-10, -33),
            S(-33, -36),   S( 23,  -4),   S( 34, -20),   S( 64,  -3),   S( 28,   9),   S( -7,  23),   S(-30,  23),   S( -5,   7),
            S(-13, -50),   S( 17, -16),   S( 44,   9),   S( 27,  42),   S( 46,   5),   S( 13,  15),   S( 29, -13),   S(-29,  39),
            S( -6, -27),   S( -5,  -5),   S( 33, -24),   S( 53,  22),   S(  2,  20),   S(-19,  36),   S(-20,  -1),   S( 20,   1),
            S(-18, -30),   S(-23,  -8),   S(  3,  -3),   S( 23,  20),   S( 26,  12),   S(  0,  11),   S( 14,  35),   S(-34, -13),
            S(  3,  14),   S(-12, -36),   S( -7, -31),   S( 14,   0),   S( 13,  18),   S( -4,  14),   S( -5,  17),   S(-16, -16),

            /* knights: bucket 5 */
            S( 20,  22),   S( 20,  27),   S(-24,  35),   S( -4,  24),   S(  0,  30),   S( 17,  16),   S(-11,  18),   S( 10,  22),
            S( 23,  28),   S( 38,  25),   S(  7,  10),   S(-12,  17),   S( 44,  -8),   S(-20,  15),   S( -3,  42),   S(-44,  17),
            S(-29,  24),   S( -7,   5),   S( 22,  16),   S( 24,  21),   S( 21,  19),   S(-16,  25),   S( -4,  16),   S(-45,  16),
            S( 25,  15),   S( 18, -20),   S( 33,  -2),   S( 64, -15),   S( 73,   1),   S( 65,   6),   S( -5,  18),   S( 20,  29),
            S( 35,   5),   S( 12,  -9),   S( 71, -14),   S(107, -12),   S( 70, -19),   S( 34,  18),   S(  1,   6),   S( 17,  23),
            S( -4, -20),   S( 29, -28),   S( -4, -23),   S(  2,  11),   S( 19,   0),   S( 41,   4),   S(-15,  15),   S( 27,  31),
            S(  0,   4),   S(-28, -58),   S( -6, -47),   S(-11, -13),   S( -8, -37),   S(  4,   4),   S( -1,  40),   S( 20,  31),
            S(-22, -38),   S(-27, -67),   S(  8, -10),   S(-24, -27),   S(  6,  -4),   S( -1,  28),   S( 20,  35),   S( -2,  17),

            /* knights: bucket 6 */
            S( -4, -10),   S(-35,  22),   S(-14,   5),   S(-29,  36),   S(-29,  31),   S( -5,  33),   S( -7,  42),   S(-33,   7),
            S( 10, -17),   S( -6,  45),   S( -6,   2),   S( 31,   9),   S( 20,  21),   S(-38,  40),   S(-14,  49),   S(-36,  66),
            S( -1,  14),   S( 20,  15),   S(  6,  26),   S( 25,  36),   S( 18,  36),   S(-50,  46),   S( 19,  30),   S(-11,  41),
            S( 12,  43),   S( 47,   5),   S( 26,  27),   S( 57,  10),   S( 65,  -6),   S( 45,  11),   S( 13,  12),   S(-24,  46),
            S( -4,  37),   S( 23,  15),   S( 74,  10),   S( 91,   2),   S( 80, -17),   S( 40,  21),   S( 91, -20),   S( 17,  27),
            S( 14,  16),   S( 14,  10),   S( 39,  23),   S( 25,  11),   S( 35,  -2),   S( 27,  -4),   S( -4, -11),   S( 17,   3),
            S(  2,  28),   S( 17,  33),   S( 33,  37),   S( -3,  -8),   S( 23, -11),   S( 17, -36),   S( -9,  -6),   S( 10,  39),
            S( 12,  29),   S(  1,  26),   S( 15,  32),   S(  0,  15),   S(  5,  -7),   S( -9,  -2),   S(  8,  22),   S(-24, -37),

            /* knights: bucket 7 */
            S(-33, -43),   S(-14, -42),   S(  5, -17),   S(-34,  19),   S(  0,  -3),   S(-32,   4),   S(-12,  -6),   S(-14,  20),
            S(-32, -53),   S( -2, -28),   S(-31,  -7),   S(-27,  -1),   S(  5,   8),   S(  2,  26),   S( -4,  14),   S(-57,  37),
            S(  5, -40),   S(-30, -23),   S( 13, -17),   S( -1,  24),   S( 41,  18),   S( 30,  13),   S(  4,  23),   S( -5,  32),
            S(-33,  10),   S(  9, -10),   S( 54, -22),   S( 72,   4),   S( 95,  -9),   S( 66,  13),   S( 53,  -1),   S( 55,  -1),
            S(  2,   3),   S( -1,   4),   S( 15,  15),   S( 62,   3),   S( 90,   6),   S(116, -23),   S(174, -22),   S( 14, -18),
            S(-17,  11),   S( 23,   6),   S( -9,   9),   S( 34,  23),   S( 83,   4),   S( 81,  -8),   S( 11, -14),   S( -6, -46),
            S(-20,   1),   S( -4,   0),   S(  0,  13),   S( 25,  22),   S( 56,  17),   S( 24,  22),   S(-16, -35),   S(-18, -39),
            S(-31, -41),   S( -9,   6),   S( -3,  19),   S(  5,  17),   S( 11,   8),   S( 18,  10),   S(  4,  -5),   S(  0, -10),

            /* knights: bucket 8 */
            S( -2,   3),   S( 10,  25),   S( 11,  26),   S( -9, -29),   S( -1,  23),   S( -3, -18),   S( 13,  24),   S( -3, -14),
            S( -6, -23),   S( -5, -22),   S( -8, -36),   S(-11,   7),   S( -5,  36),   S(  1,  -5),   S(  0,  -6),   S( -2,  -4),
            S(-11, -39),   S( -8, -22),   S(  0, -43),   S(  3,  14),   S(-11, -16),   S( 12,  10),   S( -2,  -7),   S( -1, -15),
            S(-18, -55),   S(-10, -30),   S(  4,  20),   S( -3,  12),   S(-19, -13),   S(-26, -16),   S(-20, -33),   S(-15, -36),
            S( -6, -23),   S(  2, -21),   S( -1, -16),   S(  1,  -3),   S(-18,   1),   S(-11, -17),   S(  3,  -5),   S( -1, -14),
            S( -2,  11),   S( 13,   2),   S( -1,   8),   S( -5,  -8),   S( -6,  -2),   S( -4, -12),   S( -9,  -7),   S( -7, -21),
            S(  1,  18),   S( -1, -25),   S(-12, -18),   S(  5,  13),   S(  3,   0),   S(  0,  -2),   S( -3,   1),   S( -3, -18),
            S(  0,   1),   S( -4,   6),   S( -5,   1),   S(  2,  -4),   S( -1,   5),   S( -2,  -8),   S(  0,   4),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-20, -64),   S( -5,  -1),   S( -4, -33),   S( -3, -32),   S(-15,  -7),   S(-12,  10),   S(  6,  19),   S(  1, -10),
            S( -6,   2),   S(-15, -46),   S(-20, -102),  S(-27, -59),   S(-11, -33),   S(-22, -57),   S(-11,  -2),   S(-12,   0),
            S( -9, -20),   S(-17, -39),   S(-14, -29),   S( -5, -46),   S(-23,  -5),   S( 10,  13),   S(-13,  -2),   S( -3,   3),
            S(-16, -45),   S(-12, -44),   S(-11, -24),   S(-13, -39),   S(-19, -30),   S(  0,   1),   S(-17, -40),   S(  2,   6),
            S(  3,  27),   S(-10, -24),   S( -5, -22),   S( -5, -32),   S(-12, -25),   S( -5,   9),   S( -9, -13),   S( -4,  -1),
            S(-12, -19),   S(-18, -33),   S(-11, -16),   S( -5, -11),   S(  1,  20),   S( -5,   4),   S( -3,  19),   S( -1,   6),
            S(-10, -15),   S( -1,  20),   S(-11,  -2),   S(-21, -17),   S(  2,   4),   S(  2,  23),   S( -9,  14),   S( -6,   0),
            S(  4,   0),   S(  4,   1),   S( -1,  10),   S(  0,   5),   S(-10,  -8),   S( -5,  -2),   S(  3,   7),   S( -1,  11),

            /* knights: bucket 10 */
            S( -9, -33),   S( -6,  10),   S(-10,  -8),   S(-11,  16),   S(-21, -45),   S(  7, -17),   S( -2,  11),   S( -3,  12),
            S( -4, -19),   S(  9,   1),   S(-13, -24),   S(-10, -45),   S( -8, -26),   S(-25, -51),   S( -8,  13),   S(  2,  26),
            S( -3,  -6),   S( -4,  -8),   S( -7, -10),   S(  5, -43),   S(-26, -34),   S( -6, -11),   S(-12, -30),   S( -9,  10),
            S(-10, -19),   S(-11, -21),   S( -9, -11),   S( -7, -18),   S(-13, -17),   S( -8,  -4),   S(-11, -49),   S( -4,  -3),
            S(-13, -22),   S(-12, -28),   S( -8,  -1),   S( -7, -12),   S( -1,  -5),   S(-10, -37),   S( -5, -13),   S(  5,   9),
            S( -2,   8),   S(-12,   0),   S(-11,  10),   S(-14,  21),   S(-14, -12),   S(-19, -12),   S(-13,  -3),   S(-17,  -6),
            S(  3,   8),   S( -3,  -4),   S( -6, -27),   S( 14, -19),   S( -5,   5),   S(-15, -41),   S( -8,   7),   S(-10, -13),
            S( -1,   1),   S( -2,   7),   S( -1,  14),   S( -4,   3),   S( -4,   3),   S( -7, -13),   S(  5,   8),   S(  1,   5),

            /* knights: bucket 11 */
            S( -3, -16),   S(-25, -27),   S( -3,  -5),   S(  5,  21),   S(-38, -33),   S(  0,  12),   S( -7,   7),   S(  8,  31),
            S( -8, -18),   S(-26, -40),   S(-10, -42),   S( 17,   0),   S(  6,  22),   S( -3, -25),   S(-14, -22),   S( -8, -11),
            S(-13, -42),   S(-18, -22),   S( -1, -10),   S(  0,   1),   S( -9,  25),   S( 14,   1),   S( -1, -10),   S( -4,  -1),
            S(-16, -14),   S(  6, -21),   S( -3, -24),   S(  6,   5),   S( 24,   2),   S( -7, -16),   S( 11,  17),   S( -3,  -8),
            S(-15,   0),   S(  1, -41),   S(-19,  -2),   S(  1, -13),   S( 32,  11),   S(  4,  21),   S(-13, -70),   S(-10, -12),
            S( -9, -26),   S( -7, -47),   S(  4,   6),   S(  8,   1),   S(  7,  34),   S( -7,  -6),   S( -3, -21),   S( -2,  20),
            S( -1,  -7),   S( -8,  16),   S(-10, -12),   S(  7,  -4),   S( 13,  -3),   S(  4, -14),   S(  0, -15),   S( -4,   2),
            S( -3, -18),   S(  2,   6),   S( -3, -12),   S(  1,  15),   S( -4, -11),   S( -1,  -9),   S(  4,  15),   S( -1,  -5),

            /* knights: bucket 12 */
            S(-14, -41),   S( -3,  -9),   S( -1, -17),   S(  0,   9),   S( -3,   7),   S( -5, -11),   S( -1,   6),   S( -1,   1),
            S( -3,  -8),   S(  0,   3),   S(  0, -10),   S( -3,   9),   S( -4,  -7),   S(  1,   4),   S(  2,   0),   S(  0,  -8),
            S( -2, -10),   S( -6, -20),   S( -6, -18),   S(-14, -21),   S( -8,  -2),   S( -2,  26),   S( -4,   0),   S( -4,  -9),
            S(  2,  10),   S( -1, -32),   S( -7,  27),   S(  2,  17),   S( -4, -11),   S(  3,  22),   S(  5,  12),   S(  2,   7),
            S(  0,   3),   S( -3,  -4),   S( -4, -19),   S( -4,  -9),   S(  1,   5),   S( -3,   5),   S( -6,  -4),   S( -8,  -8),
            S( -4,  -3),   S( -1,  -2),   S( -3, -14),   S( -1,  -7),   S( -3,  -2),   S( -7, -20),   S(  7,   7),   S( -1,   8),
            S( -4,  -9),   S( -2,  -1),   S( -9,  -2),   S( -3,  -7),   S(  0,   8),   S( -9,  -8),   S( -5, -19),   S( -4,  -3),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -7),   S(  1,   2),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -6),   S( -4, -12),   S( -3, -16),   S( -2,  -6),   S( -3, -10),   S( -2,   7),   S( -5,  -1),   S(  3,  10),
            S( -2,   9),   S( -2,  -1),   S(  3,  11),   S( -3,   1),   S( -6,  -7),   S(  0,  12),   S(  2,  22),   S( -4,  -6),
            S(  4,  -2),   S(  5,  10),   S(  5,   4),   S( -4, -21),   S(  4,  24),   S( -5,  11),   S(  6,   4),   S( -3,  -3),
            S( -1,  15),   S(  0,   5),   S( -5,  -1),   S(  1,  28),   S(  0,  11),   S( -3,  25),   S(  0,   6),   S( 10,  19),
            S(  1,  22),   S( -1, -15),   S( -3,  15),   S( -7,  10),   S(-15,   1),   S( -4,  24),   S( -8, -23),   S( -3,  -2),
            S( -3,  -5),   S(  2,   2),   S( -4,   8),   S(  2,  11),   S( -8,   8),   S( -8,   3),   S(  2,  19),   S(  0,   2),
            S(  1,   4),   S(  3,   8),   S( -6,  -5),   S( -4,  -1),   S( -2,   6),   S( -4,  -8),   S(  2,   6),   S( -1,   1),
            S(  2,   6),   S(  0,   2),   S( -2,  -3),   S(  2,   3),   S( -1,   0),   S(  1,   2),   S(  0,  -2),   S(  0,   1),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   3),   S(  5,  18),   S( -2,   0),   S( -6, -23),   S( -1,  19),   S(  2,   2),   S( -1,   3),
            S( -2, -11),   S( -8, -16),   S(  1,  -3),   S( -1,   3),   S(  3,   2),   S(  0,   5),   S( -7,   6),   S(  6,  57),
            S( -1,  -1),   S( -4, -34),   S(  6,  18),   S(-11, -36),   S( -3,   2),   S(  2,  13),   S( -1,  10),   S(  3,  18),
            S( -1,  -4),   S( -4, -18),   S(-22, -11),   S( -2,  45),   S(  2,  42),   S( -4,  -1),   S(  0,   6),   S(  1,  35),
            S(  6,  15),   S(-17, -35),   S( -9,  -7),   S( -8,   4),   S(  0,  35),   S(-10,   6),   S( -4,   0),   S(  3,  12),
            S( -1,   2),   S(  5,   5),   S(  3,  -5),   S( -3,  13),   S(  2,  18),   S(  1,  14),   S(  1,   8),   S( -5, -11),
            S(  0,   4),   S( -3,  -3),   S(  3,  16),   S(  5,   5),   S(  3,  10),   S( -5, -12),   S(  2,   7),   S(  3,   5),
            S(  0,  -1),   S(  0,   1),   S( -1,  -2),   S(  2,   4),   S( -1,   1),   S(  0,  -2),   S(  0,  -1),   S(  1,   2),

            /* knights: bucket 15 */
            S( -3, -14),   S( -1,   3),   S(  4,  23),   S( -2,   5),   S( -4, -15),   S(-10, -35),   S( -4, -14),   S( -2, -12),
            S(  2,  -2),   S(  5,   7),   S( -6,  -8),   S(  9,  46),   S(  2,  20),   S( -8, -32),   S( -3,  -3),   S(  1,   3),
            S(  0,  -5),   S( -5, -20),   S(  2,  -9),   S(  6,  11),   S(-18, -26),   S(  0,  -3),   S( -2,  -5),   S( -1,  -1),
            S(  0,  -8),   S( -3,   3),   S( -5, -17),   S( -5,   7),   S( -8,   6),   S(-10,  27),   S(  5,   8),   S( -1,   0),
            S( -1,  -2),   S(  9,  19),   S( -4,   6),   S( -6,   6),   S( 18,  35),   S(  0,  18),   S(  7,   0),   S(  4,  19),
            S(  1,   2),   S( -4, -10),   S( -2,   0),   S( -9, -18),   S( -6,  -9),   S(  2,  18),   S(  0,   8),   S(  5,  11),
            S( -1,   1),   S( -2,  -7),   S(  4,  15),   S(  3,   3),   S(  4,  14),   S(  5,   7),   S(  1,   7),   S(  3,   8),
            S(  1,   4),   S( -1,  -6),   S(  0,  -1),   S( -1,   0),   S(  2,  10),   S(  1,   2),   S(  0,   0),   S(  1,   2),

            /* bishops: bucket 0 */
            S( 22,  -7),   S( -9,  37),   S(-12,  16),   S(-22,  -8),   S( -2,   2),   S(  2,  13),   S( 65, -39),   S( 18, -14),
            S(-31, -11),   S( -8, -20),   S(-23,  34),   S(  1,  13),   S(  4,  20),   S( 50,  -6),   S( 30,  24),   S( 42, -15),
            S( 13,  12),   S(  5,  25),   S(  5,  -7),   S(  7,  10),   S( 25,  21),   S( 32,  21),   S( 39,   7),   S( 25,   7),
            S( 19, -27),   S( 37, -41),   S( 15,  14),   S( 32,  13),   S( 65,  33),   S( 31,  44),   S( 19,  15),   S(  7,  28),
            S( 34, -10),   S( 48, -22),   S( 58,   5),   S( 77,  41),   S( 89,  21),   S( 26,  40),   S( 34,  41),   S( -7,  15),
            S( 52,  17),   S( 56,  38),   S( 97,   4),   S( 58,  -4),   S( 23,  42),   S( 11,  34),   S( 39,  28),   S( -7,  13),
            S(-45, -78),   S( 70,  34),   S( 87,  83),   S( 20,   1),   S( 19,  -7),   S( 29,  29),   S(-25,  18),   S(-15,  52),
            S(-21, -39),   S( -4,  -8),   S( 14, -25),   S(-15, -14),   S(-10, -14),   S(-18,   8),   S(-17,  17),   S(-31, -35),

            /* bishops: bucket 1 */
            S(-60,  13),   S( -1,  -4),   S(-16,  39),   S( 21,  -8),   S(-14,  22),   S( 11,   5),   S( 40, -12),   S( 23, -33),
            S( -1, -30),   S(-21, -12),   S( -5,  -5),   S(-15,  15),   S( 29,  -7),   S(  5,   4),   S( 45, -35),   S( 13,  -9),
            S(-10,   3),   S( 31,  -7),   S(-21,  -8),   S( 19,   6),   S(  6,   1),   S( 28, -27),   S( 18,   2),   S( 61,  -2),
            S( 23, -13),   S( 48, -13),   S( 27,   3),   S( 24,  10),   S( 41,  -1),   S( 11,  13),   S( 50,  -2),   S(  1,  17),
            S( 22,  -6),   S( 57, -14),   S( 15,   8),   S( 95, -19),   S( 51,  17),   S( 42,  20),   S( -1,  24),   S( 31,   4),
            S( 64, -43),   S( 48,   9),   S( 54, -25),   S( 60, -12),   S( 74,   4),   S(-37,  11),   S(-21,  52),   S(-27,  18),
            S( 13, -62),   S( -6, -46),   S(-10,   9),   S( 24,  49),   S( 27,  36),   S(-13,  33),   S(-21,   7),   S(-28,  32),
            S(-12, -30),   S(-20,  11),   S( -6, -18),   S(-47,   1),   S(-25,  22),   S( 18,   5),   S( 28,   5),   S(-51,   1),

            /* bishops: bucket 2 */
            S( -3, -20),   S( -7,  -7),   S(  6,  18),   S(-17,   6),   S( 16,  12),   S(-15,   8),   S( 21, -11),   S( -4, -21),
            S( 21, -21),   S(  4, -30),   S( -4,  -6),   S(  6,  14),   S(-10,  11),   S(  7,   5),   S( -1, -30),   S( 17, -53),
            S( 45,   3),   S( 28,   1),   S( -4,  -2),   S( -7,  11),   S( -1,  31),   S(-12, -31),   S( 14, -21),   S( -8,  -4),
            S(-17,   8),   S( 46,  15),   S( -2,  18),   S( 30,  27),   S(  5,  13),   S(  3,  18),   S(-11,   1),   S( 10,  12),
            S(  5,  18),   S(-35,  43),   S( 54,  21),   S( 24,  27),   S( 25,  25),   S( 24,   6),   S( 12,  29),   S( 35,  -5),
            S(-28,  35),   S(  0,  37),   S(-35,  -8),   S( 94,  -3),   S( 49,  12),   S( 89, -16),   S( 72,  13),   S( 44, -45),
            S(-32,  59),   S(-38,   0),   S( -4,  24),   S(  0,  20),   S(-51,  -2),   S(-48,  23),   S(-36,   3),   S( -4, -39),
            S(-83, -21),   S(-11,  27),   S( -2,  12),   S(-19,  31),   S(-30,  -7),   S(-33,  13),   S(-11,  -7),   S(-61, -17),

            /* bishops: bucket 3 */
            S( 34, -19),   S( 40, -18),   S( 25, -22),   S( 16,  -2),   S( 20,  11),   S(  2,  30),   S(-11,  51),   S( -1, -23),
            S( 37,   2),   S( 23, -27),   S( 21,   0),   S( 23,   5),   S( 20,  17),   S( 23,   8),   S( 10, -19),   S( 30, -39),
            S( 16,   1),   S( 38,  37),   S( 20,   9),   S( 19,  29),   S( 20,  31),   S( 11,  -2),   S( 26,  -7),   S( 17,  10),
            S( -7,  16),   S( 14,  41),   S( 25,  50),   S( 38,  45),   S( 38,  21),   S( 30,   5),   S( 29,  -3),   S( 43, -37),
            S(  7,  30),   S( 18,  49),   S( 11,  56),   S( 56,  45),   S( 48,  45),   S( 53,  22),   S( 32,  15),   S(  5,  13),
            S(  4,  33),   S( 23,  52),   S(  9,  11),   S( 24,  39),   S( 55,  40),   S( 76,  43),   S( 49,  38),   S( 47,  73),
            S(-25,  75),   S( -2,  23),   S( 15,  26),   S(  2,  56),   S( 29,  38),   S( 57,  53),   S(-39,  29),   S( 21, -22),
            S(-41,   9),   S(-25,  49),   S(-46,  40),   S(-29,  52),   S( 13,  17),   S(-60,  37),   S( 15,   7),   S( -2,   6),

            /* bishops: bucket 4 */
            S(-35,   5),   S(-26,   7),   S(-32,  19),   S(-50,  17),   S(-31,  -4),   S(-23,   0),   S(-11, -17),   S(-41, -36),
            S( -8,   4),   S( -9, -14),   S( 62, -27),   S(-36,  22),   S(-57,  29),   S(-10, -24),   S(-31, -28),   S(-26, -19),
            S(  7,  25),   S( -7, -12),   S(  6,  -3),   S( -3,   7),   S( 13,  -4),   S(-65,   6),   S(-16, -27),   S(-52, -14),
            S( 31,   0),   S( 48, -18),   S( 33,   9),   S( 11,  24),   S( -9,  23),   S( 31,  -3),   S(-43,   7),   S( -7, -21),
            S( 15, -11),   S(-18, -24),   S( 31, -14),   S( 12,   2),   S( -4,  27),   S( 18,   8),   S(-16,  29),   S(-54,   2),
            S(-54, -85),   S(-55, -15),   S(-18,  -4),   S(  2,   8),   S(-42,  47),   S( 10,   1),   S(-13,  23),   S( -5,  27),
            S( -2,   0),   S(-25,   0),   S(  2, -16),   S(-28,  -7),   S(  2, -16),   S( 39,   4),   S( -5, -12),   S( 18,  33),
            S( -8,  -7),   S( -1, -20),   S(-11,  -7),   S(  2, -15),   S(-18,   7),   S(  5,  22),   S(  7,  44),   S(  6,   1),

            /* bishops: bucket 5 */
            S(-46,  -6),   S( 23,  -6),   S(-28,  20),   S(-41,  22),   S( -7,   8),   S(-56,  22),   S(-35,  24),   S(-50, -17),
            S(-12,  -5),   S(-24,  -5),   S( 25,   0),   S(-17,  24),   S(-54,  35),   S(-32,  29),   S(-38,  -1),   S(  5,  -9),
            S(  5,  31),   S(-16,   7),   S( 19, -18),   S(  5,  14),   S(-10,  26),   S(-68,   7),   S(-20,  25),   S(-23,  29),
            S( 11,  13),   S( -4,  18),   S( 58, -14),   S( 28,  13),   S(-13,  25),   S(  1,  22),   S(-68,  38),   S(-21,  23),
            S(  8,   1),   S( 21,  -1),   S(-27,  11),   S(-28,   0),   S(-12,   8),   S(-18,  18),   S(  7,  23),   S(-41,  18),
            S(  2,  -4),   S(-50,  22),   S(  1, -25),   S(-30, -23),   S(-29,  14),   S(-21,  -7),   S(-22,  24),   S(-32,  48),
            S(-24,  -6),   S( -9, -11),   S(-18,   3),   S(  4,  28),   S( 15,   9),   S(-14,  34),   S( -2,  10),   S(-18,  34),
            S(-16,  -3),   S(-10, -15),   S(  0, -12),   S(-17,   4),   S(-24,  34),   S(  9,  41),   S(-18,  30),   S( 12,   8),

            /* bishops: bucket 6 */
            S(-16, -32),   S(-15,  10),   S(-28,  23),   S(-11,  14),   S(-47,  33),   S(-23,  19),   S(-39,  32),   S(-62,  -3),
            S(-39,  16),   S(-31, -23),   S(-56,  44),   S(-38,  33),   S(-43,  33),   S(-44,  25),   S(-41,  11),   S(-36,  12),
            S(  1,   7),   S(-38,  24),   S( -2, -12),   S(-34,  37),   S(-26,  43),   S(-31,  -6),   S(-10,  -1),   S(-17,  25),
            S(-61,  31),   S(-61,  35),   S(-23,  23),   S( 17,  36),   S(  2,  36),   S(  2,  18),   S( 16,   7),   S(-18,  26),
            S(-39,  24),   S(-30,  31),   S( -7,  17),   S( 45,  15),   S(-36,  18),   S(-31,   9),   S( -2,  17),   S(-26,   4),
            S(-44,  40),   S(-21,  28),   S(-57,   4),   S(-39,  21),   S( -6,   9),   S(-21, -11),   S(-21,  23),   S(-35,   7),
            S(-14,  37),   S(-72,  33),   S(-30,  24),   S(-20,  30),   S( -9,  11),   S(  2,  11),   S(  9,  -8),   S(-30,  18),
            S(-16,   5),   S(-27,  41),   S( -9,  36),   S( 23,  19),   S(-26,  28),   S( 17,  -9),   S(-12,  14),   S(-13,  15),

            /* bishops: bucket 7 */
            S(-19, -49),   S(-55,  -7),   S(-37, -17),   S(-15,  -8),   S(-36,  -2),   S(-32,  -7),   S(-62, -20),   S(-50, -14),
            S( -6, -48),   S( -8, -46),   S( 14, -19),   S(-27,  -9),   S(-34,   5),   S(-39,   4),   S(-36, -29),   S(-10, -14),
            S(-43, -23),   S(-25,   6),   S(-13, -23),   S(  9,  -4),   S(  4,   0),   S( -5, -35),   S(-52,  11),   S(-63,  14),
            S(-17, -26),   S(-63,  25),   S(-28,  10),   S(-16,  21),   S( 84,  -3),   S( -8,  12),   S( 33, -30),   S(-21,  -4),
            S(-20,  -3),   S( 22, -18),   S(-46,  26),   S(  5,   2),   S( 41,  -6),   S( 37,   6),   S(-47,   9),   S(-38, -12),
            S(-68,  30),   S(-33,  44),   S(-15, -11),   S(-87,  37),   S(-39,  24),   S( -6, -12),   S(-10,  24),   S(-61, -85),
            S( -5,  -6),   S(-25,  -3),   S(-39,  22),   S( -2,  12),   S( -1,   6),   S( 19, -20),   S(  5, -24),   S(  2, -11),
            S(-21, -32),   S( -1,   8),   S( -8,  17),   S( -2,  13),   S(-11,   6),   S( 11, -13),   S( 30, -25),   S( -3,  -6),

            /* bishops: bucket 8 */
            S( 33,  59),   S( -2, -32),   S( -1,   1),   S( -8,  44),   S(  3,  23),   S( -6, -34),   S(-15, -25),   S(-11, -17),
            S(  0,  -1),   S( 14,  28),   S( 22,  10),   S(  9,  26),   S(  2, -10),   S(  4,   3),   S(-34, -49),   S( -9,   0),
            S( -6,  -6),   S(-13, -10),   S( 22,  30),   S( 12,  19),   S(  8,  19),   S( -2,  -2),   S(-25, -13),   S(-34, -27),
            S( -6, -13),   S( 27,  20),   S( -9,  22),   S( 20,   7),   S(  4,  34),   S( 12,  24),   S(-12,   4),   S(  3, -20),
            S( 14,  17),   S( 43,  50),   S( 16,  -5),   S( -9,  17),   S(  9,  22),   S(-24,  19),   S( -8, -30),   S(  5,  17),
            S( -8,  -6),   S(  0,   4),   S(  6,  20),   S( 24,  17),   S( 13,  34),   S( 26,   1),   S( -7,  56),   S( -2,  32),
            S(  3,  14),   S(-17, -39),   S( 29,   2),   S( 26,   5),   S( 11,   3),   S( 25,  50),   S( 18,  26),   S(-12,  -3),
            S( -6,  -4),   S(  5,   3),   S(  1,  16),   S(  4,  12),   S( 30,   6),   S( 24,  13),   S( 16,  41),   S( 35,  27),

            /* bishops: bucket 9 */
            S(  6,  30),   S(  5,  17),   S(  0,   2),   S(-29, -21),   S(-21,  -4),   S( -8,  -2),   S( -2,   1),   S( -9,  -6),
            S(  0,   0),   S(  6, -10),   S(  4,  18),   S(-35,   8),   S(-28,  17),   S(-11,  -4),   S(-37, -12),   S(-16, -29),
            S( -7,   7),   S( 18,   9),   S( -5, -16),   S(  2,  34),   S( 10,  20),   S(-30, -18),   S(  0,  12),   S( -9,  -6),
            S( -2,  28),   S( -8, -13),   S( 21,  -4),   S( 16,  -5),   S(-10,  19),   S(-13,  13),   S(  3,  23),   S( -3,  11),
            S( 25,  19),   S( 14,  10),   S( 21,  16),   S(  6, -25),   S(  9,  22),   S( -4,  29),   S(  4,  31),   S(-14, -20),
            S( 18,  25),   S( -7,  27),   S(  7, -17),   S( 11,  18),   S( 40, -41),   S( -7,  11),   S( 15,  33),   S( 13,  29),
            S( 13,  12),   S(-11,  13),   S( 11,  16),   S( 24,   1),   S( 26,   3),   S( 34,  20),   S( 15,  31),   S( 18,  58),
            S( 12,  38),   S(  2, -23),   S(  4,  21),   S( 13,  15),   S( 11,  40),   S( 20,  -1),   S( 27,   2),   S( 28,  23),

            /* bishops: bucket 10 */
            S( -2, -33),   S( 13,  13),   S( -3, -16),   S(-25, -15),   S(-66,  -9),   S(-29, -51),   S(  8,  -2),   S( -4,  13),
            S( -9,  18),   S( -5, -54),   S( -6, -12),   S(-23, -31),   S(-52,  12),   S(-32, -17),   S(-30, -13),   S(  2,   4),
            S(-10, -34),   S(-17, -14),   S(-20, -28),   S( -5,  31),   S(-17,  20),   S(-13, -26),   S( -7,   7),   S( -6, -15),
            S(-16,   9),   S(-23,   2),   S(-30, -28),   S( -1,   0),   S(-29,  42),   S( 20,   6),   S( 30,  24),   S( -7, -29),
            S( 12,   5),   S(-38,  22),   S( -5,   7),   S(  2,  31),   S( 29, -17),   S( 16,  34),   S( 17, -20),   S( 15,  11),
            S(  9,   7),   S(  8,  17),   S(-12,  -2),   S( 24,  13),   S( 13, -16),   S( -2,  -9),   S(  9,   9),   S( 25,  16),
            S( 20,  39),   S( -3,   3),   S( 32, -12),   S( 15,  30),   S(  3,  18),   S( -3, -21),   S(  2, -13),   S( 22,  29),
            S( 11,  26),   S( 21,  33),   S( 45,  17),   S( 10,  20),   S( -2,  21),   S(  7,  12),   S( 13,  17),   S(  1, -13),

            /* bishops: bucket 11 */
            S( 10, -18),   S( -6, -13),   S( -8,  -5),   S(  3,   1),   S(-18, -12),   S( -3,   1),   S(-22, -25),   S(-11,   3),
            S( -5, -12),   S(  2, -19),   S( -9,  12),   S(  2, -11),   S(-14,  16),   S(-41,  -2),   S(-35, -13),   S( 10,   5),
            S(-10, -50),   S(  0, -18),   S( -9, -37),   S(-30,   9),   S( -8,  -3),   S(  7,  25),   S( -1,  -2),   S( -2, -14),
            S(  2,  -4),   S( -3, -37),   S(  6,  -7),   S(-32, -20),   S(  7,  -2),   S( 10,  45),   S( 36,  10),   S( -9, -27),
            S(-11, -16),   S(-14, -16),   S(-36,  33),   S(-29,  32),   S(-26,  30),   S( 34,   6),   S( 22, -18),   S(  8,   6),
            S( -6,   7),   S( -9, -10),   S( -9, -10),   S(  1,  24),   S( 23,  20),   S(  7, -28),   S(  1, -16),   S( -2, -15),
            S( -2,  -5),   S( 16,  26),   S( 21,  51),   S( 33,  26),   S( 20,  -6),   S( -5,  -2),   S(-17, -27),   S( -7, -13),
            S( 28,  18),   S(  6,   3),   S( 30,  46),   S( 30, -16),   S( 18,  18),   S(  5,   5),   S( -6, -13),   S(  5,  -5),

            /* bishops: bucket 12 */
            S( -5, -11),   S( -4, -13),   S( -5,   0),   S(  8,  21),   S( -9,  -9),   S( -7,  -3),   S(  0,   5),   S( -1,   1),
            S(  0,  -5),   S(  6,   3),   S(  0,  -2),   S(  1,  15),   S(  0,  10),   S(  9,   7),   S(-14, -22),   S( -1,  -5),
            S(  8,   5),   S( 11,  -2),   S( 21,  15),   S( 20,  17),   S(  0,  13),   S( -7,  -9),   S(  2,   6),   S( -5,  -3),
            S( 10,   4),   S( 14,   3),   S( 19,   5),   S( 16,  39),   S( 10,   4),   S(  4,  20),   S(  3,  12),   S(  3,   7),
            S( 11,   9),   S( 10,  10),   S( -3,  16),   S( 21,   7),   S( 18,  24),   S(  8,  28),   S(  7,  10),   S(  4,  11),
            S(  2,   1),   S( -9, -10),   S( -6,  12),   S(  2,  -4),   S( 31,  31),   S(  9,  10),   S( -8,  -7),   S( -4, -10),
            S( -3,  -4),   S(  4,   9),   S(  3,  10),   S(  5,  -6),   S( 13,   2),   S( 21,  25),   S( 12,  25),   S( -1,  -2),
            S(  0,   4),   S( -1,  -4),   S(  0,  -4),   S(  0,  -6),   S(  2,   8),   S(  4, -10),   S( 15,   6),   S(  7,   5),

            /* bishops: bucket 13 */
            S( -5, -18),   S( -1,  -3),   S( -4, -13),   S( -5,  -9),   S( 17,  16),   S( -7, -11),   S(-15, -20),   S( -3,  -4),
            S( -5,  -2),   S( -8, -12),   S(  0,   4),   S( 17,   2),   S( -4, -12),   S(  3,  12),   S( -1,  -6),   S(  0,  -3),
            S(  8, -10),   S( 30,  18),   S( 10,   0),   S( 19,  31),   S(  2,  23),   S(  8,  20),   S( -8,   4),   S( -6,  -4),
            S( 25,  30),   S( 46,  16),   S( 20,  27),   S(-20,   8),   S( 15,  66),   S(  1,  11),   S(  8,   7),   S(  2,   9),
            S( 22,  23),   S( 16,  17),   S( 12,   1),   S(  7,  -9),   S(  9,  -7),   S( 10,  20),   S( 12,  15),   S(  3,  11),
            S(  7,   6),   S(  1,   7),   S( -4, -13),   S( 17,  -5),   S(  7,  14),   S( -5, -20),   S(  2,  -2),   S( 12,   1),
            S(  7,   8),   S( -9, -21),   S( -2, -18),   S(  4,   1),   S(  6,  18),   S( 18,  10),   S(  9,  -2),   S( 10,  13),
            S(  1,  -2),   S( -2,  -3),   S( -1,  11),   S(  2,   8),   S(  7,  13),   S(  3, -11),   S( 13,  -4),   S( 11, -10),

            /* bishops: bucket 14 */
            S(-12, -24),   S(  5,  21),   S( 15,  13),   S(  5,  22),   S(-11,  -1),   S( -7,  -5),   S( -5,   4),   S( -8,  13),
            S( -1,   1),   S( -1,  -5),   S(  2,  13),   S( -1,  -8),   S( 13,   5),   S(  3,  10),   S( -5,  20),   S(  4,  29),
            S(  1,  -4),   S( -2, -13),   S( -8, -14),   S( 19,  32),   S( 23,  46),   S( 11,  22),   S(  6,  39),   S(  3,  30),
            S(  5,  33),   S(  9, -12),   S( -4,  -1),   S(  1,  27),   S(  7,  15),   S( 19,   7),   S( 20,  15),   S(  9, -16),
            S( 10,   7),   S(  7,  17),   S( 10,   8),   S( 19,  10),   S( -4,  -1),   S(  5,  13),   S( 23,   0),   S( 16,  12),
            S(  2, -10),   S( 23,  37),   S(  3,   8),   S( 14,   5),   S( 10,  -1),   S( -7,   0),   S( -1,  19),   S( 17,   3),
            S( 17,  37),   S(  8,  10),   S( 13,  17),   S(  7,  10),   S(  7,  -3),   S(  3,  10),   S(  0, -10),   S(  2,   1),
            S( 14,   4),   S( 13,  18),   S(  4,   9),   S(  5,   1),   S( -4,  -5),   S(  1,  -5),   S(  8,  10),   S(  3,   3),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -1),   S( -7, -16),   S( -2,  -1),   S( -6, -19),   S( -4,  -7),   S( -5, -13),   S( -3,  -5),
            S(  8,  12),   S( -4, -11),   S(  6,   4),   S(  5,   6),   S(  8,   0),   S( -1,  -2),   S( -1, -10),   S( -3,  -6),
            S(  3,  -6),   S(  3,   1),   S(  0,  -7),   S( 13,  16),   S( 13,  30),   S(  8,  26),   S( 17,  22),   S(  4,   5),
            S(  1,  -9),   S( 12,  12),   S( 11,  30),   S(-18,  -6),   S(  2,   8),   S( 16,   5),   S( 13,   2),   S(  9,  18),
            S( -2,  -9),   S( -1,  11),   S( -4,  21),   S( 21,  52),   S( 20,  24),   S( 12,  -1),   S(  7,   1),   S( -3,   2),
            S( -2,  19),   S(  6,  12),   S(  4,  25),   S(  7,  12),   S( 23,  20),   S(  7, -13),   S(  3,   8),   S(  1,  -3),
            S(  6,  -1),   S(  3,  18),   S(  8,  31),   S( 15,  18),   S( 10,  15),   S( -2,   7),   S(  0,  -8),   S(  0,   0),
            S(  3,  -3),   S( 11,  14),   S(  8,   0),   S(  9,  10),   S(  5,  16),   S(  1,  -1),   S(  4,  10),   S(  4,  -1),

            /* rooks: bucket 0 */
            S(-21,  11),   S(  7, -13),   S(-14,   1),   S(-10,  15),   S(-29,  58),   S(-18,  34),   S(-48,  60),   S(-54,  43),
            S(  1, -20),   S( -6,  16),   S(-32,  23),   S(  1,  29),   S(  1,  40),   S( -3,  22),   S(-14,   9),   S(-21,  41),
            S( 23, -31),   S( 12, -12),   S(-11,  13),   S( -2,  15),   S(-26,  53),   S(-10,  13),   S(-15,  34),   S( -1,  12),
            S( 11, -17),   S( 36,  -1),   S(-34,  34),   S( 20,  16),   S( 19,  40),   S(-11,  38),   S(-19,  46),   S(-14,  27),
            S( 52, -55),   S( 41,   4),   S( 16,  33),   S( 40,  26),   S( 47,  17),   S( 31,  62),   S( 39,  48),   S( 13,  56),
            S( 44, -28),   S( 53,  19),   S( 89, -12),   S(101,  29),   S( 33,  53),   S( 35,  58),   S( 14,  67),   S(-36,  79),
            S( 11,  13),   S( 53,  44),   S( 89,  33),   S( 67,  16),   S( 69,  48),   S( 26,  62),   S( -5,  76),   S(-11,  68),
            S( -7, -26),   S( 26,  22),   S( 20,  20),   S( 42,  -2),   S( 26,  46),   S( 44,  13),   S( 35,  16),   S( 55, -44),

            /* rooks: bucket 1 */
            S(-51,  46),   S(-18,   2),   S(-11,  11),   S(-40,  26),   S(-37,  41),   S(-42,  45),   S(-46,  62),   S(-72,  70),
            S(-37,  32),   S(-22,  -6),   S(-19,  17),   S(-24,  23),   S(-24,  15),   S(-36,  41),   S(-15,  16),   S(-24,  47),
            S(-21,  21),   S( -8,  -5),   S( -9,   2),   S(-23,  16),   S(-25,  19),   S(-43,  32),   S(-53,  58),   S(-18,  54),
            S(-31,  41),   S( -6,  14),   S(-10,  26),   S(-26,  16),   S(-31,  34),   S(-41,  60),   S(-22,  51),   S(-59,  79),
            S(-14,  46),   S( 14,  -2),   S( 28,  17),   S( 26,   7),   S(  4,  29),   S( -2,  79),   S( 12,  59),   S( -3,  80),
            S( 39,  39),   S( 48,   7),   S( 22,  17),   S(-19,  34),   S(  1,  26),   S(  8,  61),   S( 38,  45),   S( 13,  79),
            S(  8,  71),   S( 18,  10),   S( -1,  33),   S(  8,  13),   S( 42,  17),   S(  5,  54),   S( 29,  64),   S( 38,  81),
            S( 39,  -5),   S( -4,   0),   S(-14, -12),   S(-30, -16),   S( 13,   9),   S( 14,  19),   S( 34,  29),   S( 51,  36),

            /* rooks: bucket 2 */
            S(-55,  64),   S(-43,  54),   S(-35,  48),   S(-36,  19),   S(-25,  20),   S(-39,  27),   S(-30,  15),   S(-67,  60),
            S(-42,  55),   S(-41,  47),   S(-38,  51),   S(-43,  33),   S(-45,  38),   S(-39,  19),   S(-19,   6),   S(-45,  38),
            S(-33,  56),   S(-21,  47),   S(-31,  38),   S(-32,  37),   S(-31,  26),   S(-28,  23),   S(-14,   9),   S(-10,  30),
            S(-23,  66),   S(-15,  57),   S(-36,  58),   S(-58,  51),   S(-42,  45),   S(-16,  28),   S( -7,  27),   S(-14,  44),
            S( -4,  81),   S(-13,  78),   S( 11,  66),   S(-17,  48),   S(-35,  60),   S( 30,  26),   S(  4,  46),   S(  0,  68),
            S( 22,  86),   S( 23,  74),   S( 32,  68),   S(-32,  61),   S( 22,  25),   S( 24,  52),   S( 82,  11),   S( 40,  71),
            S( 50,  66),   S( -3,  78),   S( 14,  57),   S( 19,  32),   S(-18,  12),   S( 14,  66),   S(-44,  89),   S( 32,  73),
            S( 11,  45),   S( 14,  51),   S( 15,  39),   S(-55,  36),   S(-67,  10),   S(  2,   8),   S( -6,  28),   S(-15,  56),

            /* rooks: bucket 3 */
            S(-15,  69),   S(-10,  66),   S(-11,  88),   S( -9,  79),   S(  1,  46),   S(  2,  38),   S( 20,  11),   S( -8,   4),
            S(  5,  58),   S( -8,  71),   S( -8,  93),   S(  3,  85),   S(  3,  53),   S( 16,  14),   S( 50, -11),   S( 22,   7),
            S( 19,  53),   S(  0,  79),   S( -3,  79),   S(  2,  86),   S( 21,  42),   S(  7,  35),   S( 41,  12),   S( 34,   9),
            S( 10,  84),   S(  0, 104),   S(-10, 104),   S(  3,  90),   S(  2,  69),   S( 17,  53),   S( 38,  33),   S( 10,  30),
            S( 10, 104),   S( -6, 118),   S( 26, 110),   S( 26, 100),   S( 20,  88),   S( 43,  67),   S( 70,  38),   S( 43,  50),
            S(  9, 124),   S( 27, 108),   S( 38, 115),   S( 53,  96),   S(100,  51),   S(114,  36),   S( 79,  45),   S( 36,  45),
            S( 23, 116),   S( 18, 115),   S( 34, 121),   S( 33, 116),   S( 35, 100),   S( 91,  53),   S(100,  97),   S(113,  63),
            S(117, -27),   S( 52,  44),   S( 18,  98),   S( 21,  81),   S( 17,  72),   S( 57,  63),   S( 30,  33),   S( 55,  11),

            /* rooks: bucket 4 */
            S(-27, -25),   S( 10, -14),   S(-34,  -3),   S(-36,  17),   S(-44,  12),   S(-29,  43),   S(-28,  -1),   S(-74,  33),
            S(-31, -43),   S(-51,   1),   S(-23, -15),   S(  6, -27),   S( 27, -20),   S(  1,   1),   S(-16,  -6),   S(  7,  11),
            S(-20, -13),   S(-40, -17),   S(-40,   0),   S( -6, -28),   S(-23,  -9),   S(-37,  16),   S(-14,  15),   S(-53,  17),
            S(-68, -26),   S(  7,   7),   S( -3, -13),   S(  7, -11),   S( 42,   3),   S( -3,  15),   S( -6,  -4),   S(-11,  10),
            S(-31, -38),   S( 20, -39),   S( 13,   3),   S( 47,  -9),   S( 64,  -4),   S( 57,  25),   S( 20,  10),   S( 22,  23),
            S(-17, -38),   S(  9,  11),   S(  6,  -1),   S( 18,  13),   S( 30,  23),   S( 12,  13),   S( 33,  16),   S( 36,  35),
            S(-30, -19),   S( 29,  27),   S( 35,   1),   S( 51,  -7),   S( 61,  -3),   S(-10,  12),   S( 15, -12),   S( 26,   7),
            S(  7, -22),   S(  2,  17),   S( 24,  -4),   S( 29,  -5),   S( 55,   2),   S( 14,   1),   S(  8,   6),   S(  9,  12),

            /* rooks: bucket 5 */
            S( -3,  19),   S(-10,   6),   S(  3,  -6),   S( 29,  -8),   S(-13,  22),   S(  6,  26),   S(-14,  48),   S(-11,  29),
            S( 12,  -8),   S(-22, -12),   S( 42, -52),   S( 34, -21),   S(-14,   7),   S(-15,  14),   S(-28,  28),   S(  5,  23),
            S(-23,  25),   S( -5,   3),   S( -2, -15),   S( -3,  -9),   S(-22,   5),   S( 43, -10),   S(-32,  33),   S(-14,  19),
            S(-26,  32),   S(-13,  19),   S( 29, -17),   S( 16,   2),   S( 22,   4),   S( -8,  46),   S( 14,  32),   S(  9,  47),
            S( 30,  24),   S(  1,  18),   S( -4,  22),   S( -9,  -6),   S(-25,  27),   S( 62,  15),   S( 30,  33),   S( 53,  34),
            S( -9,  35),   S(-13,  19),   S(  0,   5),   S(-16, -14),   S( 15,  18),   S( 13,  31),   S( 62,  14),   S( 51,  29),
            S( 34,  13),   S( 25,   5),   S(-16,   7),   S( 26,  12),   S( 45,   5),   S( 44,  -5),   S( 82, -10),   S( 43,  18),
            S( 30,  31),   S( 12,  16),   S( 44,  -1),   S(  0,  18),   S( 41,  23),   S( 31,  33),   S( 43,  37),   S( 69,  37),

            /* rooks: bucket 6 */
            S(-25,  37),   S(-10,  26),   S(-13,  25),   S(-30,  24),   S( -6,   9),   S( 12,  -4),   S( 24,  -8),   S(-19,  17),
            S(-16,  17),   S( 31,  -1),   S( 14,   7),   S( -2,   5),   S( 14, -13),   S(-17,  -3),   S(-28,   2),   S(  4,  11),
            S(-33,  32),   S( 21,  11),   S( 22,   5),   S( -2,  11),   S(-17,  12),   S( 40, -12),   S( -3,  -8),   S(  6,  -1),
            S(-28,  53),   S(  3,  39),   S( 18,  21),   S( 32,   7),   S(  7,   3),   S(  6,  11),   S( -9,  13),   S( 11,  37),
            S( -1,  52),   S( 61,  25),   S( 82,  24),   S( 37,   9),   S(  2,  -2),   S( 13,  19),   S( 40,   3),   S( 71,  11),
            S( 86,   8),   S( 88,  -4),   S( 77,   3),   S( 31,  -9),   S( -2, -17),   S( 19,  33),   S( 26,   2),   S( 53,  19),
            S( 52,  15),   S(120, -18),   S( 93, -16),   S( 69, -21),   S( 12,  -8),   S( 38,   5),   S( 43,   2),   S( 64,  -9),
            S( 92, -10),   S( 65,  12),   S( 15,  34),   S( 57,   3),   S( 45,  10),   S( 24,  31),   S( 75,  16),   S( 70,  23),

            /* rooks: bucket 7 */
            S(-84,  27),   S(-64,  27),   S(-55,  27),   S(-46,  25),   S(-29,  -1),   S(-31, -17),   S(-37,   6),   S(-75, -16),
            S(-67,  24),   S(-17,  -1),   S(-34,  10),   S(-44,  21),   S(-22, -14),   S(-25, -10),   S(  1,  -3),   S(-11, -55),
            S(-67,  28),   S(-52,  22),   S(-14,   0),   S(-18,  11),   S(-32,   4),   S(-29,  14),   S( 41, -29),   S( -4, -46),
            S(-62,  34),   S( -5,  13),   S(  5,  10),   S( 70, -24),   S(  3,   4),   S( 48, -18),   S( 38,  -1),   S(  2, -15),
            S( 11,  23),   S( 38,  18),   S( 69,   6),   S( 91, -15),   S(132, -43),   S( 91, -45),   S( 74, -17),   S(-71, -33),
            S( 37,  12),   S( 39,  -3),   S(100,  -9),   S( 90, -26),   S( 72, -10),   S( 32,  11),   S( 19,  32),   S( -8, -31),
            S( 10,  -2),   S( 41, -15),   S( 74, -16),   S(110, -44),   S(110, -39),   S( 95, -36),   S( 38,   9),   S(  0, -26),
            S(-21, -15),   S( 13,   3),   S( 41,  -2),   S( 34,   0),   S( 49,  -9),   S( 54,  -2),   S( 21,  18),   S(  7,  -8),

            /* rooks: bucket 8 */
            S(-23, -78),   S(-19, -36),   S(-12, -11),   S( 18,   7),   S(-25, -30),   S(-19,  -1),   S(-10, -34),   S(-21,   3),
            S(-34, -78),   S(-15, -43),   S(-24,   5),   S(-27, -62),   S(-26, -38),   S(-16, -21),   S(-10,  -6),   S(-39, -34),
            S( -2, -12),   S( -4, -12),   S(  9,  -1),   S(-13,  19),   S( -9,  48),   S( 12,  27),   S(  2,  48),   S(-17,   4),
            S( -6, -21),   S( -2,   2),   S( -1,   1),   S( 15,  27),   S(  1,  40),   S( 31,  42),   S( -1,  19),   S(-11, -12),
            S(-11, -37),   S( 11,  25),   S(  7,  23),   S( 17,  41),   S(  8,  23),   S( -3,   4),   S( 13,  44),   S(  1,  21),
            S(-27,  12),   S(  2,  14),   S(-16,  12),   S( -4, -14),   S(  5,  32),   S(-15,  29),   S(  0,   0),   S(  3,  18),
            S(  1,  35),   S(  2,  25),   S(  4,   7),   S( 21,  13),   S( 14,   4),   S( 10,  22),   S(  6,  16),   S(  2,  35),
            S(-15,  16),   S(  2,  13),   S(-20,  33),   S( 36,  48),   S( -6,  14),   S( 11,  33),   S(  1,  16),   S(  7,  31),

            /* rooks: bucket 9 */
            S(-27, -65),   S(-14, -60),   S(-14, -97),   S(-20, -43),   S(-21, -43),   S(  2, -29),   S( -6, -22),   S( -4, -33),
            S(-60, -42),   S(-34, -63),   S(-31, -61),   S(-45, -44),   S(-43, -45),   S(-26,  11),   S(-23, -51),   S(-31, -29),
            S(-15, -10),   S(-27, -10),   S( -1,  -8),   S(-10, -33),   S(-13, -12),   S(  2,  21),   S( -2,   7),   S(  1,  13),
            S( -6,   7),   S(  3,  -1),   S(  1,   2),   S( -2,   6),   S(-14, -28),   S(  2,   2),   S( -8,  -2),   S(  5, -26),
            S( -4,   4),   S( -9,  -6),   S(-11, -44),   S( -9,   6),   S(-24, -10),   S(-12,   7),   S(-12, -15),   S( -7, -11),
            S( -5,   7),   S(-30, -11),   S(-14, -18),   S( -2,  18),   S( -4,   3),   S( -3,  11),   S( -3,   0),   S(-11,   8),
            S(  9,  25),   S(  6,   3),   S(  3, -38),   S(  1,   8),   S(  7, -17),   S( 23,   0),   S(  6,   2),   S( -2, -21),
            S(-12,   8),   S(-19,  27),   S( -8,  12),   S( -4,  28),   S(-10,  29),   S(  7,  49),   S(  5,   9),   S( 14,  16),

            /* rooks: bucket 10 */
            S(-19, -30),   S(-52, -11),   S(-31, -38),   S(-13, -48),   S(-20, -46),   S( -4, -78),   S(  2, -63),   S(-12, -40),
            S(-45,  -9),   S(-31, -29),   S(-41, -19),   S(-46, -42),   S(-47, -43),   S(-28, -42),   S(-16, -28),   S(-46, -70),
            S(-12, -13),   S(-27, -16),   S(-34, -15),   S(-44, -42),   S(-16, -18),   S( -2, -15),   S(-16, -26),   S(-18, -12),
            S(-25, -11),   S(-35, -35),   S( -6, -34),   S( -9,   6),   S(  4,   4),   S(  3,  12),   S(-11, -28),   S( -1, -33),
            S(  7,  -9),   S(  4, -11),   S(-15, -11),   S(-14, -28),   S(  4,  14),   S( -5,   6),   S(-10, -17),   S(-10, -31),
            S( -7,   3),   S( 14,   0),   S( -1, -13),   S( -2, -24),   S(  0,  -7),   S( -8,  -5),   S(-22, -26),   S(  3, -14),
            S( -7, -19),   S( 10, -36),   S(  0, -23),   S( -4, -15),   S( 11, -23),   S( -8, -13),   S(-14, -30),   S( -2, -19),
            S( -3, -14),   S( 10,  16),   S(  4,  29),   S(-10,  11),   S(-10,  29),   S(-26,   2),   S(-29,  13),   S(  2,   8),

            /* rooks: bucket 11 */
            S(-60, -17),   S(-37,  -3),   S(-48, -12),   S(-27,  -9),   S(-46, -11),   S(-29, -13),   S(-20, -32),   S(-42, -62),
            S(-19, -12),   S(-24, -19),   S(-58,  -9),   S(-55, -18),   S(-16, -19),   S(-17,  -5),   S(-28, -27),   S(-47, -59),
            S(-34,  25),   S(-23,  12),   S( -8,  31),   S(-22,  16),   S(  2, -19),   S(-12,  -1),   S(  5, -18),   S(-15,  12),
            S(-24,  -7),   S(-14, -16),   S(-11,  10),   S(  8,  15),   S( 18,  15),   S(-21, -30),   S(  6,  17),   S( -8, -21),
            S( -7,  -9),   S(  8,  -5),   S(  4,   6),   S(  4,   6),   S( 36,  -6),   S( -2,   0),   S( 17,  37),   S(-16, -41),
            S(  5, -19),   S(-10,  -8),   S( 15, -13),   S( 20,  -5),   S(-10, -15),   S(  0,   8),   S(  3,  33),   S( -5,  -6),
            S( -4,  -1),   S(-19, -36),   S( -2,  -9),   S(  0,  -5),   S( 10,  -3),   S(  3,   8),   S(  0,  14),   S(-13,  -6),
            S( -5,  -6),   S( 17,  24),   S(  4,  18),   S( 19,  16),   S( -9,   4),   S( -3,  24),   S( 14,  15),   S(-19,  24),

            /* rooks: bucket 12 */
            S(-34, -97),   S( -9, -14),   S(-22, -56),   S(-20, -35),   S(-12, -24),   S(  8,  -9),   S(-16, -38),   S(-19, -41),
            S(  2,   1),   S(  1,   4),   S(  8,  20),   S(  3,  13),   S(  7,   8),   S(  9,  -9),   S(  6,   9),   S(-18, -26),
            S( -6, -11),   S(  7,  35),   S( 11,  22),   S( 25,  25),   S(  6,  -5),   S( 16,  26),   S(  7,  34),   S( -3,  27),
            S(  8,  22),   S(  5,   5),   S( 16,  33),   S( 12,  22),   S( 12,   8),   S(  6,   8),   S(  6,  19),   S( -3,   5),
            S( 12,  18),   S( 13,  31),   S(  9,  47),   S(  2,  -1),   S(  8,  24),   S( -2, -16),   S(  5,  13),   S(  5,  11),
            S( -2,   0),   S( -5,  -5),   S(  0,  17),   S( -5,   3),   S(  7,  22),   S( -1, -23),   S(  9,  23),   S(  4,   6),
            S(-16, -11),   S(-11,  19),   S(  8,  39),   S( -1,  19),   S( -3,  -3),   S( 12,  13),   S(  2,  20),   S(  0,  21),
            S(  4,   5),   S(-10,  30),   S(  5,  31),   S( 13,  21),   S(  2,   5),   S(  1,  17),   S(  2,   8),   S(  2,  11),

            /* rooks: bucket 13 */
            S(-26, -20),   S(-28, -49),   S(-26, -51),   S(-19, -37),   S(-30, -50),   S( -4,  -1),   S(-27, -47),   S(-24, -36),
            S(-15,  -9),   S( -8, -18),   S(  2,   7),   S( -2,  -2),   S( 17,  37),   S(  4,  14),   S(  8,   3),   S(-11, -12),
            S(-13,   2),   S(-13,   8),   S( -4,  -7),   S(  6,  10),   S(  6,  28),   S( 13,  -1),   S( 11,  44),   S(-12, -27),
            S(  9,  17),   S( -1,   9),   S( -3,   9),   S(  6,  18),   S( 10,  23),   S(  0,   9),   S(  5,  15),   S(  2,  22),
            S(  6,  19),   S(  3,  -9),   S( -5, -22),   S(  2,   6),   S( -3,  23),   S(  0,  -4),   S(  5,   5),   S( -1,  -5),
            S(  0,  12),   S( -3,  -5),   S(-10, -10),   S(-14,  -3),   S(-13, -13),   S(  2,  -5),   S( -8,   3),   S(  1,  -1),
            S(  3, -12),   S(  9,   4),   S(-10, -31),   S(  3,  15),   S( -8,  -4),   S(  7,   8),   S(  1,   1),   S(  0, -16),
            S(  2,  17),   S(-11,  13),   S( -5,   4),   S(  9,  24),   S( -3,  14),   S(  8,  22),   S(  0,  20),   S(  3,  -1),

            /* rooks: bucket 14 */
            S( -6, -25),   S(-31, -28),   S(-19, -16),   S(-20, -55),   S(-14, -40),   S( -8, -22),   S(-33, -60),   S(-26, -32),
            S( -7,  27),   S(  4,  29),   S(  5,  12),   S(  0, -17),   S(  0,  -7),   S( -3,  -3),   S( -2,   6),   S( -5,  -2),
            S(  5,  32),   S( -2,  31),   S(  0,   4),   S(  2,   6),   S(  3,   8),   S( -1,  -4),   S(  2,  25),   S(-19, -45),
            S( -4,  14),   S( 16,  22),   S(  6,  20),   S( 10,   5),   S( -9,  -6),   S(  1,  -9),   S(  8,  14),   S(-11, -16),
            S(  8,  14),   S( 19,  17),   S( -2,  -5),   S(  1,   6),   S(  3, -12),   S( 18,  30),   S( -1,   2),   S( -3, -17),
            S(  5,   7),   S(  6,   9),   S(  7,  16),   S(  2,   5),   S( -3,   6),   S(-15,   4),   S( -9,  -9),   S( -6,  -8),
            S( -7, -14),   S(  8,  12),   S( -8, -21),   S(-17, -34),   S( -5,   6),   S(  1,  -1),   S(-12, -14),   S( -8,  -8),
            S( -1,  -7),   S(  3,   3),   S( -4, -17),   S(  6,  -8),   S(-11, -16),   S(-16, -42),   S(  3,  -5),   S(  1,  26),

            /* rooks: bucket 15 */
            S(-25, -44),   S(-17, -48),   S(-39, -48),   S(-25, -50),   S( -3, -22),   S(-14, -19),   S( -2,  -7),   S(-21, -53),
            S(  6,  29),   S(-11,   1),   S(-11,  -6),   S( -6,  -8),   S( -6, -17),   S(  4,   0),   S(  7,  10),   S(  3,   5),
            S(  5,  10),   S( -7, -13),   S( 12,  24),   S(  7,   0),   S(  6,   0),   S( -7, -11),   S(  6,  25),   S(  2,   6),
            S(  2,  10),   S( -1,  -6),   S( 18,  34),   S( -4, -12),   S(  4,  18),   S(  2,   6),   S(  6,  16),   S(  3, -11),
            S(  5,  12),   S(  5,   6),   S(  7, -10),   S(  2,  10),   S(  6,  13),   S(  4,   3),   S( -1,  28),   S(  4, -10),
            S(  6,  13),   S(  7,  -3),   S(  8,  -4),   S(  3,   2),   S( -5, -16),   S( -4,  37),   S(  2,  23),   S(  4,   2),
            S(  4,  -5),   S( -3,   3),   S(  8,  18),   S(  3,   9),   S(  2,  13),   S(  6,  15),   S(-12,  12),   S( -9, -27),
            S(  0,  18),   S( -1,  21),   S(  8,  18),   S(  1,  24),   S( -1,   3),   S( -6, -24),   S( -4,  15),   S(-15,  -9),

            /* queens: bucket 0 */
            S( -2,  -7),   S(-22, -47),   S(-32, -54),   S( -1, -98),   S( -7, -52),   S( 12, -59),   S(-51, -27),   S(-13,  -9),
            S(-16, -29),   S( 14, -76),   S(  4, -69),   S( -8, -19),   S(  3, -17),   S( -5, -34),   S(-22, -27),   S(-33,  -9),
            S(  0,   6),   S( -2, -22),   S( 29, -51),   S( -9,   8),   S( -3,  25),   S(  0,   2),   S(-29,  -1),   S(-73, -40),
            S(-20,  21),   S( 17, -21),   S( -9,  21),   S( -6,  69),   S( -5,  65),   S(-19,  39),   S(-37,  29),   S(-15, -24),
            S(-23, -21),   S(  4,  64),   S(  2,  33),   S(  1,  47),   S(  6,  71),   S(-16, 109),   S(-53,  70),   S(-41,   4),
            S(-17,   5),   S( 16,  33),   S( 13,  38),   S(-18,  72),   S(-16,  68),   S(-54,  98),   S(-60,  27),   S(-40,   7),
            S(  0,   0),   S(  0,   0),   S( 17,   2),   S(-31,  33),   S(-29,  29),   S(-58,  85),   S(-82,  64),   S(-94,  26),
            S(  0,   0),   S(  0,   0),   S(  2, -10),   S(-18, -13),   S(-31,  25),   S(-32,  10),   S(-47,  -2),   S(-58, -24),

            /* queens: bucket 1 */
            S( 22,  -1),   S( 11,   1),   S( 16, -45),   S( 30, -85),   S( 39, -42),   S( 16, -24),   S( 17,  -5),   S(  5,  16),
            S(-18,  34),   S( 25,  18),   S( 39, -34),   S( 29,   5),   S( 43,  15),   S(  6,  21),   S(-15,  40),   S(-14,  10),
            S( 49,  -2),   S( 28,   2),   S( 21,  32),   S( 18,  73),   S( -2,  81),   S( 35,  47),   S(  1,  41),   S( 19,  -8),
            S( 43,   3),   S( 18,  42),   S( 21,  48),   S( 42,  69),   S( 21,  83),   S(  9,  62),   S( 11,  41),   S( -6,  60),
            S( 47,   1),   S( 54,  13),   S( 51,  34),   S( 22,  31),   S( 47,  66),   S( 32,  32),   S( -6,  77),   S(  8,  90),
            S( 63,  -2),   S( 95,  11),   S( 83,  43),   S( 71,  53),   S( 43,  39),   S( 19,  65),   S( 47,  55),   S(  4,  56),
            S(101, -26),   S( 49, -21),   S(  0,   0),   S(  0,   0),   S( -1,  39),   S(-11,  20),   S(-10,  57),   S(-36,  38),
            S( 69, -13),   S( 42, -21),   S(  0,   0),   S(  0,   0),   S( 10,  16),   S( 31,  21),   S( 78,   0),   S(-16,  36),

            /* queens: bucket 2 */
            S( 38, -12),   S( 31,  12),   S( 34,  21),   S( 45, -25),   S( 47, -29),   S( 33, -21),   S(  2, -19),   S( 37,  31),
            S( 27,   4),   S( 12,  51),   S( 38,  28),   S( 42,  37),   S( 54,   9),   S( 22,  28),   S( 26,  20),   S( 19,  47),
            S( 42,  13),   S( 31,  44),   S( 23, 104),   S( 17,  84),   S( 26,  81),   S( 26,  75),   S( 36,  48),   S( 32,  64),
            S(  3,  74),   S( 26,  84),   S( 26,  83),   S( 15, 123),   S( 34,  93),   S( 25,  93),   S( 38,  63),   S( 37,  84),
            S( 10,  84),   S( -9,  85),   S(  6,  97),   S( 37,  73),   S( 26,  91),   S( 93,  38),   S( 71,  55),   S( 66,  59),
            S(-10,  86),   S(  0,  81),   S(  1,  80),   S( 69,  34),   S( 30,  53),   S( 96,  69),   S(100,  43),   S( 51,  99),
            S(  1,  50),   S( -1,  40),   S( -4,  67),   S( 42,  28),   S(  0,   0),   S(  0,   0),   S( 25,  69),   S( 37,  67),
            S(  1,  34),   S( 33,  -2),   S( 38, -11),   S( 16,  35),   S(  0,   0),   S(  0,   0),   S( 38,  29),   S(  6,  50),

            /* queens: bucket 3 */
            S(-42,  32),   S(-28,  38),   S(-21,  37),   S(-13,  44),   S(-26,  32),   S(-13, -17),   S(-12, -40),   S(-36,  20),
            S(-56,  56),   S(-36,  48),   S(-22,  66),   S(-15,  84),   S(-14,  73),   S(-14,  36),   S( 18, -14),   S( 16, -27),
            S(-49,  80),   S(-37,  90),   S(-29, 114),   S(-38, 145),   S(-27, 125),   S(-20,  94),   S( -5,  55),   S( -8,  21),
            S(-41,  82),   S(-57, 141),   S(-49, 163),   S(-33, 175),   S(-34, 166),   S(-18, 102),   S( -1,  81),   S(-12,  64),
            S(-54, 119),   S(-45, 156),   S(-46, 178),   S(-41, 195),   S(-20, 159),   S(  1, 133),   S(-13, 124),   S(-16,  74),
            S(-62, 113),   S(-57, 159),   S(-55, 183),   S(-49, 194),   S(-48, 169),   S( -2, 105),   S(-26, 124),   S(-26, 107),
            S(-95, 130),   S(-92, 151),   S(-72, 186),   S(-60, 160),   S(-67, 162),   S(-15,  80),   S(  0,   0),   S(  0,   0),
            S(-124, 142),  S(-76, 105),   S(-63, 104),   S(-65, 112),   S(-61, 102),   S(-24,  56),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-30,  -2),   S(-44, -35),   S( -7,   0),   S( -7, -17),   S( -5,  -5),   S( -7,  12),   S(-32, -24),   S( 13,  21),
            S( -2, -11),   S( -9,   5),   S( -4,   0),   S(-11, -12),   S(-38,  18),   S(-17,  12),   S(-44, -10),   S( -1, -16),
            S(  5,  17),   S( 20, -30),   S( 12, -17),   S( 15,   9),   S( 44,  10),   S( 18,  21),   S(-13, -17),   S( 33,  24),
            S(-17, -24),   S( 14, -21),   S(  0,  -1),   S( -9,  18),   S( 43,  31),   S(  7,  58),   S(-23,   6),   S(-13,  18),
            S(  0,   0),   S(  0,   0),   S( 13,  -9),   S( 50,  34),   S( 24,  56),   S( 34,  52),   S( 14,  16),   S( 16,  22),
            S(  0,   0),   S(  0,   0),   S( 15,  10),   S( 29,  17),   S( 42,  48),   S( 33,  49),   S( 23,  26),   S(  2,   6),
            S( 12,  -6),   S( 18,   8),   S( 59,  36),   S( 54,  35),   S( 55,  15),   S( 23,  31),   S(  8,  25),   S( -8,  23),
            S( 16, -12),   S(-20, -33),   S( 21,   7),   S( 46,  23),   S( 16,   9),   S(  9,  24),   S(  2,   3),   S( 21,   8),

            /* queens: bucket 5 */
            S( 38,  25),   S( 28,  12),   S( 18,   8),   S(  1,  27),   S( 37,  -5),   S( 41,  49),   S( 11,  -1),   S( 20,   2),
            S( 23,  18),   S( 17,   1),   S( 15,  -1),   S( 13,  14),   S( 14,  43),   S( -6,  -9),   S( 29,  15),   S( 10,   4),
            S( 21,   5),   S( 43,  -2),   S( 22,  -1),   S(  3,  16),   S( 18,   8),   S( 32,  18),   S( 28,  42),   S( 21,  17),
            S(  7, -30),   S( 32,   1),   S( 17, -20),   S( 25,  10),   S( 55,   3),   S( 31,  15),   S( 36,  50),   S(  7,  32),
            S( 37,  -8),   S( 20, -42),   S(  0,   0),   S(  0,   0),   S(  2,   7),   S( 26,  14),   S( 42,  55),   S( 16,  36),
            S( 32,  13),   S( 32,   4),   S(  0,   0),   S(  0,   0),   S( 28,  19),   S( 58,  33),   S( 45,  38),   S( 53,  41),
            S( 60,   1),   S( 63,   8),   S( 50,  40),   S( 25,  26),   S( 42,  15),   S( 83,  44),   S( 61,  59),   S( 52,  33),
            S( 40,  29),   S( 48,  11),   S( 58,  17),   S( 39,  -5),   S( 54,  21),   S( 64,  41),   S( 69,  51),   S( 60,  32),

            /* queens: bucket 6 */
            S( 52,  52),   S(  6,   5),   S( 41,  19),   S( 36,  25),   S( 28,  15),   S( -1,   3),   S(  4,  12),   S(  6,  18),
            S( 31,  19),   S( 35,  30),   S( 72,  41),   S( 59,  28),   S( 42,  24),   S( 19,  17),   S(-13,  29),   S( 26,  33),
            S( -2,  49),   S( 44,  38),   S( 33,  39),   S( 52,  15),   S( 27,  11),   S( 45,   0),   S( 57,  27),   S( 69,  61),
            S( 29,  36),   S(  9,  30),   S( 48,  15),   S( 83,  14),   S( 30, -11),   S( 35,   6),   S( 71,   3),   S( 94,  45),
            S( 33,  55),   S( 34,  38),   S( 51,  37),   S( 42,  29),   S(  0,   0),   S(  0,   0),   S( 57,  18),   S( 97,  49),
            S( 44,  50),   S( 58,  47),   S( 46,  54),   S( 24,   5),   S(  0,   0),   S(  0,   0),   S( 67,  42),   S(102,  42),
            S( 60,  35),   S( 19,  29),   S( 61,  15),   S( 50,  15),   S( 41,  37),   S( 66,  48),   S(119,  22),   S(125,   6),
            S( 40,  42),   S( 67,  27),   S( 72,  20),   S( 77,  37),   S( 93,   8),   S( 92,  12),   S(102,  13),   S( 97,  30),

            /* queens: bucket 7 */
            S( -5,  26),   S( -4,   2),   S(-19,  23),   S(  0,  24),   S( 16,   3),   S( -9,   5),   S(  0,  15),   S(-14,  -9),
            S( -3,  25),   S(-39,  28),   S(  4,  47),   S( -6,  75),   S( -6,  42),   S(  6,  26),   S(  9,   3),   S(-28,  -5),
            S(  8,  24),   S( -8,  34),   S( -9,  88),   S( 43,  45),   S( 46,  32),   S( 20,  13),   S( 45, -24),   S( 45,  -6),
            S( -9,  21),   S( 22,  43),   S( 21,  70),   S( 46,  73),   S( 77,  47),   S( 56,  -2),   S( 69, -32),   S( 30,  -7),
            S( 20,  24),   S( -6,  58),   S( 22, 102),   S( 51,  82),   S( 79,  21),   S( 61,  -5),   S(  0,   0),   S(  0,   0),
            S(  6,  46),   S( -6,  89),   S( 15,  88),   S( -1,  87),   S( 53,  37),   S( 85,  49),   S(  0,   0),   S(  0,   0),
            S(-33,  62),   S(-18,  42),   S( 13,  59),   S( 34,  61),   S( 51,  41),   S( 67,  19),   S( 66,  22),   S( 56,  25),
            S( 34,  19),   S( 44,  33),   S( 49,  59),   S( 46,  27),   S( 49,  45),   S( 22,   5),   S(-19,   7),   S( 53, -10),

            /* queens: bucket 8 */
            S(-19, -37),   S(  1, -23),   S(-16, -43),   S( -3,  -8),   S(-17, -30),   S(  9,  -3),   S( -1, -12),   S(  1,   4),
            S(-21, -34),   S( -6, -16),   S(  2, -14),   S( -6, -10),   S(  9,  -2),   S( -4, -11),   S( -3,   3),   S( -1,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -16),   S(-11, -46),   S(  7,   3),   S(  7,  -7),   S( -7,  -8),   S(  3,   5),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S( -4, -13),   S( -2,   1),   S(  4,  -1),   S( 12,  19),   S(  7,   3),
            S( -3, -12),   S(  7,  11),   S(  7,   1),   S( 11,  -7),   S(  8, -10),   S( 14,  12),   S( 13,  12),   S(-10,  -9),
            S(  1, -13),   S(  4, -16),   S( 14,  15),   S(  3, -19),   S( 13,   8),   S( 28,  35),   S(  8,  -5),   S( -2,  -5),
            S(-16, -36),   S(  2, -10),   S( 14,  11),   S( 25,  39),   S( 12,  12),   S( 19,  41),   S(  4,   7),   S(  5,   0),
            S(  3,   1),   S(  5,  -5),   S( 14,  10),   S(  9,  -2),   S( 18,  19),   S( -2,  -4),   S(  4,  10),   S(-15, -27),

            /* queens: bucket 9 */
            S(  9, -10),   S(-19, -33),   S(-16, -35),   S( 11, -10),   S( -8, -36),   S( -2,  -9),   S( -4,  -9),   S( -1, -14),
            S( -2,  -8),   S(-13, -22),   S(-12, -28),   S(  1, -16),   S(-25, -53),   S(-13, -31),   S(  6,  -2),   S(  1, -10),
            S(-17, -45),   S(-14, -27),   S(  0,   0),   S(  0,   0),   S(  4,  -9),   S(  8, -10),   S( -5,  -9),   S(  5,  -4),
            S(  1,  -8),   S(-11, -31),   S(  0,   0),   S(  0,   0),   S( -1,  -5),   S(  9,   0),   S( 10,  10),   S( -2,   3),
            S( -8, -25),   S(  0, -14),   S( -1,  -6),   S(-11, -10),   S( -6, -28),   S( 11,  17),   S(  6,  -7),   S(  0, -15),
            S( 11,  11),   S( -2, -27),   S(  3, -12),   S( -3, -18),   S( -1, -11),   S(  6,   6),   S( -1, -10),   S( -2, -13),
            S(  9,   7),   S(  9,  -3),   S( -5,  -2),   S(  1,   9),   S( 24,  27),   S( 25,  29),   S(  8,  20),   S(  7, -11),
            S( 17, -10),   S( 26,  18),   S( -1,  -7),   S( 21,  14),   S( 21,  18),   S(  7,  14),   S(  1, -18),   S( 15,   3),

            /* queens: bucket 10 */
            S( 16,  10),   S( 13,   9),   S( -1,  -9),   S( -6, -26),   S(-10, -30),   S(-11, -20),   S( -4, -27),   S( -5, -14),
            S(  6,   4),   S(-13, -20),   S( -8, -25),   S(-19, -53),   S( -5, -11),   S(  9,  -2),   S(-12, -27),   S( -6,  -7),
            S( -2,   1),   S(  2,   3),   S( -4,  -5),   S( -9, -19),   S(  0,   0),   S(  0,   0),   S(  2,  -5),   S(-13, -23),
            S( -4, -10),   S(  3,   4),   S(  3,   2),   S(  8,   1),   S(  0,   0),   S(  0,   0),   S( -6, -15),   S( -1, -16),
            S( 12,  16),   S( 15,   6),   S(  3,  -4),   S( 27,  30),   S( -1,   1),   S( -2,  -1),   S(  0, -11),   S( 11, -25),
            S( -5,  -8),   S(  6,   7),   S( 24,  29),   S( 10,  12),   S( 14,  15),   S( 15,  22),   S( 16,   9),   S( -2, -22),
            S(  8,   7),   S( 18,  26),   S( 19,  26),   S( 23,  20),   S( 11,  17),   S( 24,  12),   S( 15,  11),   S(  6,  -5),
            S(-11, -31),   S(  4,   6),   S( 22,   7),   S( -5,   0),   S( 14,  14),   S(  3,   3),   S( 15,  10),   S( 10,  -7),

            /* queens: bucket 11 */
            S(-10,  -4),   S( -3,  -1),   S( -8, -10),   S(-18, -18),   S( -5, -13),   S(-24, -34),   S( -8, -32),   S(-10, -16),
            S( -5,  -1),   S(  1,   8),   S(-22, -10),   S( -7,   4),   S( 20,  -2),   S(-12, -27),   S(  7,  -5),   S( -6, -12),
            S(  4,   7),   S(  5,   1),   S(-20,  13),   S( -2,   2),   S( -5, -21),   S(-24, -31),   S(  0,   0),   S(  0,   0),
            S(  0,  -1),   S( -7,   9),   S( -2,  11),   S(  0,   3),   S(  0,  -7),   S( -3,   6),   S(  0,   0),   S(  0,   0),
            S(  2,  12),   S( 16,  15),   S( 17,  25),   S(  5,  23),   S( 41,  47),   S( 16,  27),   S(  9,   0),   S(-11, -28),
            S(  1,   4),   S(  2,   0),   S(  0,  14),   S( 12,  28),   S( 14,  20),   S(  1,   5),   S(  4, -10),   S(  3, -24),
            S(  3,   4),   S( 10,  12),   S( 16,  24),   S(  2,  11),   S( 21,  59),   S( 16,  13),   S(  7,   3),   S( 10,  -3),
            S(-16, -57),   S( 11,  14),   S( -5,  -6),   S(  7,  39),   S( 17,  33),   S( 11,   2),   S( -6,  -2),   S( 12,   1),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  1,   2),   S(-16, -20),   S( -6,  -5),   S(-12, -20),   S( -1,  -3),   S( -2,  -3),
            S(  0,   0),   S(  0,   0),   S(  5,   3),   S(-10, -17),   S( -7,  -8),   S(-11, -23),   S( -8, -16),   S(  2,   0),
            S( -6,  -9),   S(  5,   7),   S( -6,  -8),   S(-12, -35),   S( 16,  30),   S( -1,  11),   S( -1,  -7),   S(  8,   9),
            S( -8, -17),   S(  6,   4),   S(  6,  13),   S(  3,  13),   S(  1,   3),   S( -1,   9),   S( -3,  -3),   S( -3,  -9),
            S(-17, -28),   S(  3,   9),   S(  6,   3),   S(  6,   5),   S(  6,  27),   S( -5, -20),   S( -8, -17),   S( -2,  -2),
            S(  2,  -6),   S( -4, -12),   S(  0, -13),   S(  5,   9),   S( -5, -10),   S( -9,  -1),   S(-11, -10),   S( -2,  -7),
            S( -8, -11),   S(  4,   7),   S( -6, -11),   S( 13,  10),   S(  0,   0),   S( -9, -15),   S(  1,   1),   S( -7, -25),
            S(  7,  12),   S(  0,  -3),   S(  2,  -5),   S(  0,   2),   S( -6,  -7),   S(-13, -13),   S( -4,  12),   S( -8, -13),

            /* queens: bucket 13 */
            S(-23, -35),   S(-16, -31),   S(  0,   0),   S(  0,   0),   S(-17, -28),   S(-13, -34),   S( -1,  -2),   S( -4, -10),
            S(-16, -45),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(-16, -35),   S(-22, -43),   S(-12, -22),   S( -4,  -6),
            S(-22, -38),   S( -5, -14),   S( -4,  -4),   S( -2, -14),   S(-22, -41),   S(-11, -16),   S( -8,  -7),   S( -1,  -4),
            S( -8, -18),   S(-19, -29),   S(  0,  -7),   S( -7, -19),   S(  9,   5),   S( 18,  32),   S( -4, -15),   S( -8, -11),
            S(  6,  -8),   S(  3, -20),   S( -7, -21),   S( 12,  23),   S( -7, -10),   S( -1, -17),   S( -3,  -5),   S(  2, -11),
            S( -1,  -2),   S(-14, -18),   S(  5,   2),   S( 11,  22),   S(  0, -11),   S( -5,  -7),   S(-13, -24),   S(-10, -22),
            S(  0,   0),   S( -3,  -9),   S( 11,  24),   S( -2,  -2),   S(  4,   2),   S(  8,   0),   S(-13, -25),   S( -7, -11),
            S( -7,  -6),   S( -2,  -7),   S( -5, -13),   S(  1,  -7),   S(  3,  -2),   S( -1,  -3),   S(  0,  -8),   S(-12, -20),

            /* queens: bucket 14 */
            S( -7, -15),   S( -1, -10),   S(-10, -20),   S( -2,  -6),   S(  0,   0),   S(  0,   0),   S( -4,  -7),   S( -9, -23),
            S( -7, -24),   S(-26, -47),   S(-11, -22),   S( -4, -13),   S(  0,   0),   S(  0,   0),   S( -8, -23),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -7, -21),   S(-15, -26),   S( -3,  -4),   S(  2,   3),   S(-11, -17),   S(-17, -33),
            S( -9, -12),   S( -2,  -1),   S(  0,   0),   S(-15, -21),   S( -9, -16),   S(-20, -29),   S( -2, -21),   S(  1,   2),
            S( -6, -13),   S( -5, -13),   S( -4, -15),   S(  6,   9),   S(  5,  18),   S(-10, -25),   S( -9,  -3),   S( -1,  -4),
            S( -5, -12),   S(  3,  -4),   S(-12, -21),   S(-12, -22),   S(  6,  10),   S(  2,   5),   S( -1,  -5),   S(-10, -11),
            S(-10, -16),   S( -2,  -9),   S(  0,   0),   S(  3,   6),   S(  3,   5),   S(  4,   5),   S( -8, -21),   S( -3,  -9),
            S(-10, -17),   S(  5,  -5),   S(-10, -14),   S( -3,  -8),   S(  3,   1),   S( -3,  -3),   S( -4,  -2),   S(  2,  -8),

            /* queens: bucket 15 */
            S(  1,   3),   S( -6, -18),   S(  5,   2),   S(-11, -17),   S(  4,   7),   S(-10, -11),   S(  0,   0),   S(  0,   0),
            S( -5,  -5),   S(  1,   6),   S(-13, -17),   S( -8, -17),   S(  0,  -6),   S(  2,   7),   S(  0,   0),   S(  0,   0),
            S( -1,   0),   S(  1,  -1),   S(-12,  -4),   S( -6,  -5),   S(-10, -22),   S(  4,   4),   S( -1,   2),   S( -1,  -4),
            S( -2,  -5),   S(-10, -15),   S( -3,  -6),   S(  2,   8),   S( 10,  29),   S(  6,  27),   S( -3,   5),   S( -4, -15),
            S(  1,   2),   S(  1,   0),   S( -4,  -9),   S(  0,  -1),   S( 11,  51),   S(  4,  21),   S(  4,  12),   S( -5, -15),
            S( -1,  -3),   S( -3,  -3),   S( -3,  -8),   S( -6,  -2),   S( -1,   3),   S( -9,  -8),   S(  2,  11),   S( -7,  -5),
            S( -5, -11),   S(  0,  -1),   S( -5,   4),   S(  3,   3),   S( -7, -11),   S(  1,   6),   S(  5,  10),   S( -5, -10),
            S( -8, -18),   S(-13, -30),   S( -1, -10),   S(  2,   2),   S(-13,  -3),   S( -3,  -1),   S(  1,   0),   S( -3,   4),

            /* kings: bucket 0 */
            S( -9, -20),   S( 30,  -9),   S( 16,  -3),   S(-25,  15),   S(-11,  14),   S( 30, -25),   S(  1,   3),   S(  9, -48),
            S(-17,  30),   S( -5,   1),   S( -1,   2),   S(-43,  24),   S(-39,  41),   S( -9,  18),   S(-13,  34),   S(  1,  24),
            S( 10,   4),   S( 68, -31),   S(  4,  -5),   S(-25,   2),   S(-21,   3),   S(  4,  -9),   S(-23,  13),   S( 34, -29),
            S(-27, -25),   S( 12, -30),   S( 17, -29),   S(-22,   8),   S(-38,  31),   S(-45,  26),   S(-33,  35),   S(-12,  30),
            S(-51, -120),  S( -3, -46),   S( -1, -34),   S( 14, -23),   S(-46,  -6),   S(-31,   9),   S(-19,  10),   S( -2,  -9),
            S(-10, -119),  S(  0,   8),   S( -8, -54),   S(-13,  -8),   S(  0, -14),   S(-24,  17),   S( 18,  22),   S(-20,   7),
            S(  0,   0),   S(  0,   0),   S(  0, -50),   S(  4, -33),   S(-18,  -6),   S(-10, -15),   S(-29,   4),   S(-10,  -4),
            S(  0,   0),   S(  0,   0),   S(-12, -11),   S(  1, -10),   S(  8,  -2),   S( -6,  12),   S(  7,   3),   S(  9,   0),

            /* kings: bucket 1 */
            S(  7, -26),   S( 30, -20),   S( 17, -15),   S( 32,  -4),   S( -2,  -1),   S( 34, -21),   S(  3,   5),   S( 16, -22),
            S( 11,  -2),   S(  1,  10),   S( -3,  -8),   S(-50,  28),   S(-31,  20),   S(-11,  14),   S( -7,  16),   S(  5,   7),
            S(-12, -14),   S( -4, -13),   S(  2, -17),   S(  7, -20),   S(-36,   2),   S( 13, -18),   S( 24, -12),   S( 35, -11),
            S( -4,   0),   S(  7, -12),   S( 14,  -7),   S(  2,   4),   S( 22,   6),   S( -2,  -1),   S( 38,  -8),   S(-20,  27),
            S(-21, -54),   S(-13, -45),   S( -5, -54),   S(-11, -42),   S(  0, -25),   S( -1, -29),   S( -8,  -4),   S( -7,  -3),
            S(-33,   0),   S(-102,   4),  S(-33,  27),   S(  2,  23),   S(-42,   5),   S(-25,  14),   S( 15,   3),   S( -8,  -9),
            S(-36, -51),   S(-24,   5),   S(  0,   0),   S(  0,   0),   S(-40,  13),   S(-52,  27),   S( -5,  28),   S( -3, -33),
            S(-30, -111),  S(-13, -16),   S(  0,   0),   S(  0,   0),   S( -9,   8),   S(-14,  15),   S( -3,  20),   S( -5, -47),

            /* kings: bucket 2 */
            S( 10, -54),   S(  6,  -1),   S( 18, -18),   S( 17,  -9),   S(  1,   6),   S( 38, -24),   S( -4,  17),   S( 17, -24),
            S( 35, -36),   S(-15,  30),   S(-16,   9),   S(-21,   9),   S(-27,  15),   S(-15,   6),   S(  2,   0),   S(  2,   0),
            S(-34,  -3),   S(-17, -13),   S(-12, -11),   S(-17, -19),   S( -8,  -3),   S(  0, -18),   S( 28, -18),   S( 23, -15),
            S( 13,  13),   S( -7,  12),   S( 13,   0),   S(-12,   9),   S( 41,  -9),   S( -8, -11),   S( 39, -29),   S( 31,  -9),
            S( -6,  -9),   S( 17, -15),   S( 26, -37),   S(  9, -29),   S( 33, -49),   S(-20, -42),   S( 24, -50),   S(  1, -43),
            S(  1,   6),   S(-10,  -6),   S(-42,   2),   S(-38, -12),   S(  2,   0),   S(-11,  26),   S(-83,  11),   S(-21, -18),
            S( -8, -10),   S( -9,  21),   S(-75,  13),   S(-17,  10),   S(  0,   0),   S(  0,   0),   S(-10,  17),   S(-37, -36),
            S( -7, -38),   S(-20, -27),   S(-32, -31),   S( -7,   9),   S(  0,   0),   S(  0,   0),   S(-10, -13),   S(-34, -121),

            /* kings: bucket 3 */
            S( -4, -52),   S( 14,  -5),   S( 27, -21),   S( -4,  -6),   S(  1, -13),   S( 37, -26),   S(  1,  15),   S(  8, -28),
            S(  4,  17),   S(-19,  39),   S(-15,   5),   S(-33,  15),   S(-52,  30),   S(  1,  -1),   S( -8,  18),   S(  4,  11),
            S( 18, -26),   S(  4,  -5),   S( -2,  -9),   S(-32,  -4),   S( -9,   8),   S( 20, -20),   S( 52, -22),   S( 54, -16),
            S(-17,  31),   S(-83,  42),   S(-49,  16),   S(-33,  11),   S(-17,   7),   S(  1, -26),   S(-23,  -7),   S(-26, -16),
            S(-15,   9),   S( -9,  -5),   S(-35, -11),   S(-15, -16),   S( 34, -45),   S( 58, -67),   S( 42, -70),   S(  7, -80),
            S(-14, -13),   S( 22,   5),   S( 20, -11),   S(  2, -24),   S( 45, -33),   S( 58, -49),   S( 73, -21),   S( 51, -115),
            S(-23, -11),   S( 26,  10),   S( 16, -13),   S( 31, -23),   S( 32, -29),   S( 28, -54),   S(  0,   0),   S(  0,   0),
            S( -5, -10),   S(  5,  10),   S( -5,  20),   S( 11, -10),   S(  8, -69),   S( -2,  11),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-61,   6),   S(  4,  36),   S(  2,  23),   S(  6,   2),   S(-16,   8),   S(  1,  -9),   S(  1,   8),   S( 13, -32),
            S(-39,  22),   S( 30,  17),   S(  5,  15),   S(  0,   0),   S( 33,  -3),   S( 27,  -5),   S( 50, -17),   S( 13,  -3),
            S( -3,  26),   S( 15, -14),   S( 20,  -5),   S( -7,   0),   S(-18,  11),   S( 21, -23),   S(-35,   7),   S( 18, -12),
            S( -3, -21),   S(-10,   9),   S(  5,  16),   S(  7,   4),   S(-16,   8),   S( -9,  16),   S( 17,   9),   S( 11,   6),
            S(  0,   0),   S(  0,   0),   S( -1,   4),   S(-29,  12),   S(-35,  14),   S(-25, -16),   S(-20,   1),   S( -4,  -1),
            S(  0,   0),   S(  0,   0),   S(  6, -13),   S( -3,  24),   S(-11,  27),   S(-27, -13),   S(  7, -16),   S( -2,  16),
            S( -2, -20),   S( -4,  -6),   S( -4, -22),   S(  0,  21),   S( -6,  23),   S(-28,  -9),   S(-12,  19),   S(  3,  -3),
            S( -5, -23),   S(  2, -14),   S(-10, -19),   S( -8,   3),   S(  6,  11),   S( -7, -10),   S( -6,   0),   S(  4,  11),

            /* kings: bucket 5 */
            S( 27,  -2),   S(-18,  15),   S(-42,  26),   S(-50,  31),   S(-26,  28),   S(-10,  14),   S( 28,   1),   S( 13,  -2),
            S( -3,   1),   S( 19,   9),   S( 33,  -6),   S( 30,  -8),   S( 23,  -6),   S( 40, -13),   S( 30,   1),   S( 43, -15),
            S(-16,  10),   S( -5,  -7),   S(-15,  -4),   S( -3,  -8),   S(  9,   0),   S(-38,   0),   S( -7,   3),   S( 11,   0),
            S( -4, -12),   S( -1,  -6),   S(  8,  -5),   S( 10,  16),   S(  4,  20),   S( 10,   2),   S( 15,   5),   S(  6,   6),
            S( -4, -28),   S(-29, -45),   S(  0,   0),   S(  0,   0),   S( -8,  -3),   S(-19, -14),   S(  7, -14),   S(-10,   5),
            S( -7, -39),   S(-25, -28),   S(  0,   0),   S(  0,   0),   S(-22,  38),   S(-54,  12),   S(-15,  -3),   S( -7,  -3),
            S(-16, -31),   S(-31,  21),   S(  2,   9),   S(  0, -17),   S(-27,  29),   S(-40,  19),   S(  0,   9),   S( 11,  19),
            S(-10, -100),  S( -9,  13),   S(-10, -24),   S( -2, -33),   S(-10, -17),   S( -6,   9),   S( -3, -15),   S(  0,   7),

            /* kings: bucket 6 */
            S( 30, -35),   S( 22, -11),   S(-10,   4),   S(-28,  25),   S(-18,  22),   S(-31,  23),   S(-10,  25),   S(-10,   9),
            S( 39, -25),   S(  8,  18),   S( 11,  -6),   S( 27,  -9),   S( 27,  -7),   S( -5,  10),   S( 17,  -2),   S(  1,   2),
            S( 13, -17),   S(-27,   3),   S(-20,  -8),   S( -5,  -8),   S( 12,  -9),   S(-45,   5),   S(  9,  -2),   S(-24,  16),
            S( 11,   5),   S( 24,  -3),   S( 13, -12),   S( 28,   5),   S( 63,  -1),   S(-29,   5),   S( -6,   7),   S(  0,   1),
            S(  6, -19),   S( 19, -29),   S(-24, -11),   S(  0, -17),   S(  0,   0),   S(  0,   0),   S(-45, -21),   S(-40, -18),
            S(-17,   0),   S(  4,   0),   S(-30,  -1),   S(-11, -20),   S(  0,   0),   S(  0,   0),   S(-27, -14),   S(-30, -21),
            S(  0,  -9),   S( -9,   7),   S(-39,  11),   S(-16,  -2),   S(  4,   6),   S( -9, -31),   S(-28, -12),   S( -8, -37),
            S( -1,  -6),   S(  1,  -5),   S( -3,  11),   S(-14, -30),   S( -8, -35),   S( -5, -25),   S( -6,  -2),   S( -1, -58),

            /* kings: bucket 7 */
            S( 23, -31),   S(-14,   0),   S(-34,  -1),   S(-18,  11),   S(-36,  14),   S(-52,  37),   S(-37,  37),   S(-53,  29),
            S(  7,   0),   S( 20, -20),   S( -3,  -8),   S(-25,   5),   S( -5,   4),   S(-27,  19),   S( 13,  -9),   S( -2,  11),
            S( 27, -28),   S(-19,  -8),   S(-32,  -3),   S(-31,  -3),   S(-42,   8),   S(-28,  11),   S( 19,  -6),   S(-51,  22),
            S(-24,  18),   S(  5,   9),   S( -4,  -1),   S( 42,  -8),   S( 34, -10),   S( 55, -30),   S( 21, -11),   S( 13,  -8),
            S(-18,  16),   S( -3,   1),   S(  2, -25),   S(  9, -17),   S( 17, -26),   S( 11, -21),   S(  0,   0),   S(  0,   0),
            S(-10, -32),   S( -1,  -8),   S( 15, -11),   S( 12,  -6),   S( 21,  -9),   S( 17, -10),   S(  0,   0),   S(  0,   0),
            S( 13,  18),   S( -4, -18),   S(  1,   4),   S(-11, -12),   S(  7, -19),   S( -5, -26),   S(  5, -17),   S(-11,  11),
            S(  7,   8),   S( -8,  -8),   S( 10,  17),   S( -3,  -3),   S(  7,  15),   S(-18, -50),   S(  8, -10),   S(-11, -59),

            /* kings: bucket 8 */
            S( 13, 118),   S( -3,  81),   S( 39,  40),   S( -3,  -2),   S(-13,  13),   S(-15,  -5),   S( 30, -14),   S(-16, -18),
            S( 28,  70),   S( 26,  13),   S( 49,  60),   S( 84,  -3),   S( 18,  23),   S(  6,  -7),   S( -3,   9),   S(  3,  26),
            S(  0,   0),   S(  0,   0),   S( 29,  66),   S( 40,   1),   S( 20,   7),   S( -9,  -9),   S( -2,  14),   S(  9, -16),
            S(  0,   0),   S(  0,   0),   S(  4,  75),   S( -9,   0),   S(-19,  36),   S( -6,  17),   S( 13,  10),   S( 11,  14),
            S( -4, -26),   S( -1,  27),   S(  3,  13),   S(-16,  25),   S(-17,  -3),   S(  4, -16),   S(  2,  11),   S(-15, -27),
            S(  5,  15),   S( -1, -15),   S( -3, -14),   S( -7,   2),   S(-13,   1),   S(-10,  -3),   S( -8,  -2),   S(  9,  -8),
            S( -5, -14),   S( -9, -12),   S(  5,   9),   S( -1, -11),   S( -3, -32),   S(-11,   6),   S( -3,   0),   S(  5, -46),
            S( -6,  -9),   S(-10, -26),   S( -3, -11),   S( -6, -22),   S(  6,   7),   S( -6,   3),   S(  0,  -4),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  4,  30),   S(-14,  33),   S(-19,  56),   S( 16,   9),   S(-18,  33),   S(-28,  28),   S( 36,   5),   S( 18,  16),
            S(-19,  34),   S( 36,  23),   S(  2,   0),   S( 47,   1),   S( 60,  17),   S( 26,   5),   S( -7,  27),   S(-18,  14),
            S( -5,  12),   S( 21,  12),   S(  0,   0),   S(  0,   0),   S( 48,  17),   S(  0,   2),   S( 10,  -1),   S(-19,  22),
            S( -1, -31),   S( 12, -23),   S(  0,   0),   S(  0,   0),   S(  5,  34),   S( 12,  -2),   S(-13,  10),   S(-16,  30),
            S(  4, -19),   S( 11,  -2),   S(  4,  17),   S( -1,  13),   S(-14,  17),   S(-21,  15),   S(-10,  13),   S(  0, -15),
            S(  6,   4),   S(  2,  -8),   S(  8,  -9),   S(-10, -21),   S(-13,  12),   S( -1,   9),   S(-32,   1),   S(  5,  31),
            S(  2,  -6),   S( -3, -20),   S( -1,  -9),   S(  2, -30),   S( 14, -30),   S( 13,  16),   S(-16,  -9),   S(  4,   4),
            S(  7,   6),   S( -2, -23),   S( 10, -24),   S( -5, -20),   S( -1, -17),   S(  2,  10),   S( -7,  12),   S(  8,  -2),

            /* kings: bucket 10 */
            S( 34,  -1),   S(  2,  -7),   S(  5,   8),   S(  7,  24),   S(-14,  20),   S(-92,  49),   S(-33,  47),   S(-91,  86),
            S(  4,   0),   S( 62,   1),   S( 24,  -5),   S( 32,  10),   S( 57,  12),   S( 47,   3),   S( 16,  24),   S(-90,  49),
            S( 15,   7),   S( 27,   1),   S( 26, -12),   S( 13,  11),   S(  0,   0),   S(  0,   0),   S( -7,  21),   S(-60,  28),
            S( 14,   5),   S( 42, -26),   S( 35, -33),   S( 29,   3),   S(  0,   0),   S(  0,   0),   S( -2,  14),   S(  7,   2),
            S(  3,   6),   S( 27,   6),   S( 31, -20),   S( 10, -29),   S(  4, -17),   S(  6,  25),   S(  9,   8),   S( -8,  15),
            S(  3,  15),   S(  3,  -5),   S( -2,   5),   S( 10,  -7),   S(  7,  -1),   S(-17,  -7),   S(-12,   6),   S(  0,  -8),
            S(  0, -42),   S( -3, -15),   S(  9, -10),   S( 13,   0),   S( 10,   0),   S( -9, -19),   S(  5, -27),   S(  5,   6),
            S(  4,   5),   S( 11, -12),   S( -2, -15),   S(  0,   4),   S(  6, -15),   S(  0, -30),   S( -5,  -6),   S(  9,   3),

            /* kings: bucket 11 */
            S( -5, -18),   S(  9,   7),   S(  6, -10),   S( -6,  15),   S( -8,   6),   S(-69,  57),   S(-72,  80),   S(-132, 153),
            S( -2, -25),   S( 21,   4),   S(-12, -16),   S( 19,  22),   S( 89,  -1),   S( 63,  39),   S( 17,  18),   S( 23,  39),
            S(  4, -49),   S( -3,  18),   S(  1, -11),   S( 23,   9),   S( 66,   0),   S( 28,  61),   S(  0,   0),   S(  0,   0),
            S(  1,  21),   S( 18,  12),   S( -7,   3),   S(  9,  15),   S( 25, -10),   S( 22,  23),   S(  0,   0),   S(  0,   0),
            S( -1,  35),   S(  1,  -3),   S(  8,  -7),   S( 14, -15),   S( 16,   0),   S(  0,   0),   S(  8,  10),   S(  7,   1),
            S( 11,  10),   S(  0, -15),   S( 15, -12),   S( -1,   4),   S( -6,  -7),   S(  2, -16),   S( -5,  -8),   S(-10,  -3),
            S(  7,  13),   S(  7,  -6),   S( 17,  22),   S(  0, -26),   S( 15, -16),   S(  3,   3),   S(-10, -11),   S( -7, -12),
            S(  4,   8),   S(  5,  -1),   S(-12, -22),   S(  5,  -6),   S( -4, -20),   S( -8, -18),   S(  0, -20),   S(  5,  13),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 19,  57),   S(  7,  -6),   S(  1,  -2),   S(  8,  14),   S(  8,  -1),   S(-19,   7),
            S(  0,   0),   S(  0,   0),   S( 47, 109),   S( 29,  13),   S( 22,  43),   S( 13,  -3),   S( 25,  -6),   S(-17,   0),
            S( -1,   9),   S(  3,  14),   S( 23,  71),   S( 39,  19),   S(  8,  -7),   S( 11,   1),   S(  2, -13),   S( -8,  -1),
            S( -2,   8),   S(  9,  32),   S( -1,  15),   S(  4,  -7),   S( -8,   0),   S( -2,  19),   S( -4,   9),   S(  1,   7),
            S(  9,  17),   S(  6,  23),   S( 11,  20),   S( -3,  41),   S( -4,  41),   S(  0,   2),   S( -9,  14),   S(-12, -11),
            S(  6,   6),   S(  9,  15),   S( -2,  -2),   S(-10, -16),   S( -1,   5),   S( -8,  17),   S( -9, -16),   S(  7,  -1),
            S(  3,   8),   S( -7, -13),   S( -2,   6),   S( -1,   0),   S( -5,  -9),   S(  4,   8),   S(  8,  43),   S( -1, -29),
            S( -3,   2),   S(  6,   3),   S( -4,   7),   S(  0,   3),   S( -1,  -5),   S(  4,   6),   S(-11, -22),   S( -3, -22),

            /* kings: bucket 13 */
            S( -1,  53),   S(  7,  34),   S(  0,   0),   S(  0,   0),   S( 44,  17),   S( 14, -13),   S( -4,  -7),   S(-18,  26),
            S(  1,  21),   S( -1,  -3),   S(  0,   0),   S(  0,   0),   S( 47,   4),   S( 28,  -9),   S(-18,   4),   S(-14,   7),
            S( -3,   3),   S( 20,  22),   S(  2,  -7),   S( 14,  38),   S( 52,  13),   S( 22,  -6),   S(  2,   7),   S( 13,  -9),
            S(-10,  -6),   S( 15,  -2),   S(  1,  20),   S( -7,  16),   S( -5,  14),   S(  3, -11),   S(  4,  22),   S(-15, -27),
            S(  7,  11),   S(  0,   6),   S(  6,  42),   S( -5,  25),   S( -8,  10),   S(  5,  18),   S(-10,   2),   S(  8,  10),
            S(  3,   0),   S( -5,  17),   S( -2,  16),   S( -5,  -1),   S(-12, -16),   S( -5,   8),   S( -8,  19),   S(  1,   0),
            S(  9,  11),   S( -8, -21),   S(-11, -44),   S(  4,  20),   S(-11, -10),   S(-10,  15),   S(-14, -24),   S(  6,  13),
            S(  1,  -1),   S(  5,  -4),   S(  4,  20),   S(  3,   4),   S(  0,  17),   S(-10, -17),   S( -3,   8),   S(  8,  15),

            /* kings: bucket 14 */
            S( 18,  34),   S(  0,  -6),   S( 11, -41),   S( 16,   0),   S(  0,   0),   S(  0,   0),   S(  6,  71),   S(-43,  40),
            S(-10,  -7),   S( 19,  -8),   S( 46, -34),   S( 41,  12),   S(  0,   0),   S(  0,   0),   S( 13,  30),   S(-42,   4),
            S(  5,   3),   S( 15,  -5),   S( 35, -34),   S( 40,   2),   S( 11,  -2),   S( 14,  34),   S( 27,  55),   S(-27,   2),
            S(  8,  -4),   S(  8,  -9),   S( -2, -11),   S( 10,   1),   S(-22,  -1),   S( 14,  54),   S(  3,  24),   S(  7,  -2),
            S(  7,  19),   S( 10,  -2),   S( -8,   1),   S(-17,  11),   S(  1,  29),   S(  4,  56),   S(  2,  39),   S(  6,  13),
            S( -5,  -7),   S(  2,   6),   S( -2,  -1),   S(  0,  11),   S( -6, -20),   S( -6,  -3),   S(-14,  -8),   S( -1,   6),
            S(  4,   6),   S(-10, -14),   S( 11,  -6),   S( 16,   4),   S(  3,  -1),   S( -6,  17),   S(-27, -21),   S(  8,  18),
            S(  1,  14),   S(  5,  -6),   S(  9,   2),   S( -5,  -6),   S(  7, -10),   S( -3,  -6),   S(-13, -24),   S(  0, -10),

            /* kings: bucket 15 */
            S( 12,  32),   S(  6,  -2),   S( 11,  -7),   S( -7,   0),   S(-11, -10),   S(  0,  58),   S(  0,   0),   S(  0,   0),
            S( -2, -22),   S(  7, -11),   S( -7, -14),   S( 20,  51),   S( 40,  -1),   S( 62, 108),   S(  0,   0),   S(  0,   0),
            S( -9, -22),   S( 17, -10),   S(  7, -17),   S( -4,  11),   S( 11,  -6),   S( 27,  68),   S(  9,  42),   S(-13,  -3),
            S( -1, -11),   S(  3,  15),   S(  2,  14),   S(-13, -28),   S(-14,  -2),   S( 20,  47),   S( 16,  47),   S( -3, -11),
            S( 10,   5),   S( -8,  25),   S(  1,  -4),   S( -5, -34),   S( -3,   7),   S(  2,  34),   S(  4,   5),   S(  3,   3),
            S(  5,  27),   S(-14,  -4),   S(  9,  14),   S(  9,  19),   S(-10, -24),   S( -2,   6),   S(  0,   7),   S(  4,  16),
            S(  8,  12),   S( -4,  23),   S( -2, -10),   S(  3,   6),   S(  9,   7),   S(  9,  16),   S( -5,  -2),   S(  2,   1),
            S( -2,  -7),   S(  4,   1),   S( -2, -11),   S(  4,   3),   S(  4,   4),   S( 10,  12),   S(  0,  -7),   S(  3,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-42,  49),   S(-17, -18),   S(  2,  64),   S( 15,  99),   S( 25, 121),   S( 33, 145),   S( 41, 149),   S( 54, 144),
            S( 68, 123),

            /* bishop mobility */
            S(-36,  22),   S(-13,  -4),   S(  4,  43),   S( 14,  86),   S( 24, 110),   S( 29, 130),   S( 33, 142),   S( 38, 146),
            S( 42, 148),   S( 51, 146),   S( 63, 137),   S( 83, 131),   S( 90, 132),   S( 59, 135),

            /* rook mobility */
            S(-110,   4),  S(-31,   5),   S(-15,  85),   S(-14, 114),   S(-13, 145),   S( -8, 156),   S( -1, 167),   S(  6, 168),
            S( 13, 180),   S( 19, 184),   S( 21, 191),   S( 30, 190),   S( 41, 193),   S( 47, 195),   S( 89, 168),

            /* queen mobility */
            S( 89, 162),   S(-25, 328),   S( 25, 210),   S( 39, 119),   S( 49, 132),   S( 49, 191),   S( 51, 230),   S( 54, 264),
            S( 55, 294),   S( 57, 319),   S( 59, 335),   S( 62, 346),   S( 63, 354),   S( 64, 367),   S( 66, 369),   S( 66, 372),
            S( 69, 372),   S( 73, 365),   S( 80, 351),   S( 95, 336),   S(105, 319),   S(148, 277),   S(151, 269),   S(175, 234),
            S(187, 220),   S(181, 201),   S(123, 194),   S(109, 146),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9,  14),   S(-19,  47),   S(-30,  44),   S(-38,  60),   S(  9,  12),   S(-11,  13),   S( -1,  57),   S( 25,  28),
            S( 15,  34),   S(  0,  46),   S(-16,  45),   S(-19,  38),   S(  0,  34),   S(-25,  41),   S(-25,  54),   S( 33,  27),
            S( 24,  68),   S( 15,  72),   S(  7,  53),   S( 22,  45),   S( -1,  50),   S(-24,  63),   S(-28,  94),   S( -5,  76),
            S( 33, 105),   S( 41, 119),   S( 20,  80),   S(  8,  62),   S(  4,  64),   S(-11,  90),   S(-54, 126),   S(-77, 150),
            S( 25, 151),   S( 50, 183),   S( 52, 130),   S( 26, 113),   S(-58, 106),   S( 12, 109),   S(-60, 174),   S(-84, 171),
            S( 95, 228),   S( 82, 268),   S(129, 239),   S(125, 251),   S(130, 263),   S(152, 241),   S(133, 251),   S(138, 261),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  5,   1),   S( -7, -28),   S( -4, -10),   S(  3,  -8),   S( 13,   8),   S(-15, -37),   S(-23,   8),   S( -1, -48),
            S(-18,  19),   S( 24, -23),   S( -1,  28),   S( 13,  20),   S( 33, -12),   S( -7,  17),   S( 26, -20),   S( -6,  -5),
            S(-12,  16),   S( 15,   5),   S(  3,  41),   S( 16,  50),   S( 25,  27),   S( 33,  17),   S( 30,  -1),   S( -2,  15),
            S( 16,  35),   S( 15,  52),   S( 37,  94),   S( 14, 100),   S( 66,  69),   S( 69,  57),   S( 22,  59),   S( 20,  23),
            S( 50,  93),   S( 89, 115),   S(102, 141),   S(139, 164),   S(137, 133),   S(138, 148),   S(131, 123),   S( 50,  59),
            S( 73, 196),   S(118, 279),   S(102, 222),   S( 97, 198),   S( 67, 153),   S( 48, 140),   S( 41, 144),   S( 16,  80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 11,  22),   S( 16,  21),   S( 29,  34),   S( 32,  21),   S( 20,  19),   S( 23,  25),   S(  5,  11),   S( 38,  -2),
            S( -5,  24),   S( 17,  37),   S( 13,  36),   S( 10,  43),   S( 25,  17),   S( 14,  21),   S( 32,  20),   S(  2,  12),
            S(  0,  22),   S( 28,  51),   S( 53,  59),   S( 39,  60),   S( 44,  55),   S( 71,  20),   S( 32,  35),   S( 18,   7),
            S( 53,  73),   S(104,  57),   S(121, 124),   S(144, 130),   S(135, 120),   S( 72, 132),   S( 67,  58),   S( 63,  12),
            S( 47, 125),   S( 91, 141),   S(155, 211),   S(108, 252),   S(134, 261),   S( 82, 239),   S(149, 205),   S(-55, 170),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 25,  33),   S( 11,  18),   S( 13,  33),   S(-13,  62),   S( 66,  20),   S( 21,   8),   S( -5,   1),   S( 28,  12),
            S( -1,  15),   S(  5,   8),   S( 16,  17),   S( 12,  30),   S(  7,  19),   S(  2,   7),   S(  9,   5),   S( 27,  -4),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1, -15),   S( -5,  -8),   S(-16, -17),   S(-12, -30),   S( -7, -19),   S( -2,  -7),   S( -9,  -5),   S(-27,   4),
            S(-25, -33),   S(-11, -18),   S(-13, -33),   S( 13, -62),   S(-66, -20),   S(-21,  -8),   S(  5,  -1),   S(-28, -12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-27, -39),   S(-14, -42),   S(-16, -49),   S(-62, -33),   S(-26, -46),   S(-29, -47),   S( -8, -49),   S(-25, -61),
            S(-26, -23),   S(-21, -30),   S(-32, -15),   S( -6, -38),   S(-39, -38),   S(-28, -27),   S(-37, -22),   S(-14, -44),
            S(-19, -20),   S( -9, -35),   S(-25, -14),   S(-31, -25),   S(-20, -44),   S(-21, -24),   S(-12, -23),   S(-42, -32),
            S( -7, -34),   S( 17, -46),   S( 12, -20),   S(  8, -31),   S(  7, -31),   S( 57, -45),   S( 33, -45),   S(-15, -55),
            S( 14, -50),   S( 40, -75),   S( 46, -30),   S( 60, -33),   S( 74, -51),   S( 79, -39),   S(131, -94),   S( 31, -82),
            S( 98, -99),   S(127, -110),  S( 92, -50),   S( 71, -31),   S( 66, -32),   S(119, -46),   S( 98, -46),   S( 44, -86),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-12, -24),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(-44,  18),        // attacks to squares 1 from king
            S(-43,   5),   S(-15,   0),   S( 25,  -8),   S( 75, -22),   S(129, -33),   S(141, -16),   S(191, -33),   S(238, -21),

            S(-38,  12),        // attacks to squares 2 from king
            S(-38,   8),   S(-27,   9),   S( -6,   3),   S( 16,   0),   S( 39,  -5),   S( 62, -11),   S( 90, -20),   S(146, -29),

            /* castling available */
            S( 69, -60),        // king-side castling available
            S( 15,  66),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 42, -94),   S( 46, -82),   S( 40, -92),   S( 32, -78),   S( 25, -67),   S( 17, -61),   S(  0, -48),   S( -2, -44),
            S( 12, -48),   S( 31, -49),   S( 58, -48),   S( 23, -31),   S(100, -54),

            /* orthogonal lines */
            S(-44, -145),  S(-95, -110),  S(-117, -89),  S(-133, -84),  S(-140, -86),  S(-146, -87),  S(-145, -93),  S(-140, -99),
            S(-153, -89),  S(-170, -85),  S(-168, -98),  S(-119, -130), S(-88, -142),  S(-40, -152),

            /* pawnless flank */
            S( 44, -36),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 26, 226),

            /* passed pawn can advance */
            S(-10,  34),   S( -2,  60),   S( 19, 101),   S( 90, 168),

            /* blocked passed pawn */
            S(  0,   0),   S( 56, -17),   S( 32,  10),   S( 29,  46),   S( 26,  62),   S( 19,  34),   S( 70,  63),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 50, -44),   S( 45,  27),   S( 20,  37),   S( 12,  59),   S( 27,  95),   S(133, 126),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-15, -16),   S( -7, -33),   S(  1, -32),   S(-23,  -8),   S(-30,  22),   S(118,  12),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 25, -12),   S( 27, -14),   S(  5,  -4),   S(  3, -40),   S(-14, -118),  S(-38, -210),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 26,  48),   S( 59,  22),   S( 99,  43),   S( 34,  23),   S(176, 113),   S(105, 119),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 13,  54),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-29, 121),

            /* bad bishop pawn */
            S( -6, -16),

            /* rook on open file */
            S( 27,   1),

            /* rook on half-open file */
            S(  3,  39),

            /* pawn shields minor piece */
            S( 13,  12),

            /* bishop on long diagonal */
            S( 25,  50),

            /* minor outpost */
            S(  6,  34),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 22,  34),   S( 23,   1),   S( 32,  19),   S( 27,  -4),   S( 36, -21),

            /* pawn threats */
            S(  0,   0),   S( 65,  97),   S( 52, 116),   S( 74,  89),   S( 61,  45),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  65),   S( 52,  49),   S( 77,  45),   S( 51,  68),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 25,  46),   S( 30,  40),   S(-18,  45),   S( 68,  64),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 22,  11),   S( 20,  34),   S( 30,  14),   S(  5,  32),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
