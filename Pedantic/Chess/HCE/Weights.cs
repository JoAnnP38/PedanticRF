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
        public const int MAX_WEIGHTS = 12815;
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
        public const int PAWN_PUSH_THREAT = 12784;  // pushed pawn threatening opponent
        public const int PAWN_THREAT = 12790;       // pawns threating opponent piece
        public const int MINOR_THREAT = 12796;      // minor piece threat
        public const int ROOK_THREAT = 12802;       // rook threat
        public const int CHECK_THREAT = 12808;      // check threat against enemy king
        public const int TEMPO = 12814;             // tempo bonus for side moving

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

        // Solution sample size: 16000000, generated on Fri, 07 Jun 2024 11:22:58 GMT
        // Solution K: 0.003850, error: 0.081925, accuracy: 0.5152
        private static readonly Score[] defaultWeights =
        [
            /* piece values */
            S( 80, 232),   S(388, 668),   S(413, 662),   S(548, 1090),  S(1398, 1804), S(  0,   0),

            /* friendly king relative piece square values */
            #region friendly king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(103, -125),  S(154, -85),   S( 44, -41),   S(-26,  31),   S(-24,  15),   S(-23,   4),   S(-47,  12),   S(-31, -18),
            S(123, -130),  S(110, -103),  S( 11, -61),   S(-13, -51),   S(-22, -14),   S(-20, -25),   S(-34, -17),   S(-24, -45),
            S(111, -108),  S( 67, -59),   S( 13, -63),   S( 10, -73),   S(-11, -63),   S(  3, -57),   S( -7, -47),   S(  3, -58),
            S( 68, -41),   S( 53, -52),   S( 26, -59),   S( 15, -83),   S(-18, -44),   S( -9, -56),   S(-13, -39),   S( -6, -29),
            S( 76,  36),   S( 37,  -6),   S( 39, -26),   S( 51, -72),   S( 19, -43),   S(-10, -40),   S(-22,  -4),   S(-34,  52),
            S( 63,  57),   S( 53,  80),   S(  5,  10),   S( 17, -17),   S(-41,   0),   S(  6,   3),   S( -5,  23),   S( 12,  48),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 32, -31),   S( 38, -30),   S( 56, -21),   S(  4,  23),   S( -4,  -6),   S(  6,  -9),   S(-38,  11),   S(-30,  19),
            S( 35, -46),   S( 29, -41),   S( 17, -47),   S( -4, -41),   S(-14, -21),   S( -6, -27),   S(-31,  -8),   S(-36, -13),
            S( 30, -44),   S( 14, -26),   S( 19, -56),   S( 12, -60),   S(-22, -28),   S( 14, -49),   S( -4, -29),   S(  1, -29),
            S( 42, -23),   S( 23, -46),   S( 27, -56),   S(  5, -50),   S(-14, -22),   S( 19, -46),   S(-17, -22),   S( -7,   0),
            S( 29,  44),   S(-29,   5),   S( -2, -35),   S( 10, -48),   S( 35, -36),   S( -7,  -7),   S(-24,  24),   S(-26,  71),
            S( 56,  57),   S( 18,   3),   S(-43, -19),   S(-22,  24),   S(-21,  -7),   S(-55,  26),   S(-46,  31),   S(-42,  88),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-11,  -4),   S(-14,   9),   S( -4,   1),   S(  2,   6),   S( 19, -13),   S( 35, -15),   S( 12, -35),   S( -2, -20),
            S( -1, -30),   S(-22, -11),   S(-17, -34),   S(-15, -33),   S(  8, -34),   S( 14, -32),   S(  0, -33),   S(-16, -31),
            S( -9, -28),   S(-18, -25),   S( -8, -56),   S( -1, -58),   S( -3, -34),   S( 26, -47),   S(  8, -37),   S( 11, -36),
            S(-12, -10),   S( -5, -45),   S(-11, -55),   S( -1, -58),   S( 11, -49),   S( 12, -32),   S( 10, -22),   S(  5, -12),
            S( -1,  34),   S(-38,  -5),   S(-37, -42),   S(-44, -32),   S( 15,  -9),   S( -4,   4),   S(-19,  25),   S(-21,  75),
            S(-47,  78),   S(-87,  58),   S(-91,  -5),   S(-69, -18),   S(-39,   6),   S(-15,  21),   S( -3,  -2),   S(-18,  76),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -9, -19),   S(-22,   3),   S(-21,  -3),   S( 16, -47),   S(  0,  -1),   S( 53, -23),   S( 96, -63),   S( 70, -88),
            S( -7, -46),   S(-23, -26),   S(-18, -44),   S(-17, -27),   S( -6, -30),   S( 20, -40),   S( 65, -70),   S( 64, -82),
            S( -7, -52),   S( -4, -55),   S( -4, -69),   S(  0, -72),   S(  1, -61),   S( 26, -58),   S( 46, -65),   S( 82, -81),
            S( -5, -35),   S(  6, -72),   S(  2, -81),   S(  6, -77),   S( 25, -82),   S( 30, -66),   S( 40, -50),   S( 73, -39),
            S( 25,   6),   S( -6, -33),   S( 12, -76),   S( 15, -69),   S( 88, -69),   S( 72, -39),   S( 62,   9),   S( 55,  60),
            S(-31, 101),   S(-20,  12),   S( -3, -51),   S( -6, -66),   S( 63, -77),   S( 61, -20),   S( 68,   6),   S( 73,  50),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-88,  19),   S( -7,  -7),   S(-31,  15),   S( -8,  25),   S( -8, -19),   S(-42,  26),   S(-41,   7),   S(-44,   3),
            S(-15,   2),   S( 40, -17),   S( 18, -34),   S( 15, -27),   S(-10, -20),   S(-48, -17),   S(  4, -37),   S(  5, -32),
            S( 37, -23),   S( 33, -10),   S(-23,   7),   S( -7, -31),   S(-45, -30),   S(-22, -34),   S(-25, -37),   S( 21, -43),
            S( 12,  23),   S(-10,  37),   S( 31,   2),   S( -5,   0),   S( 12, -38),   S(-42, -24),   S(  9, -39),   S( 52, -35),
            S(-21,  89),   S(-20,  87),   S(-19,  27),   S(-19,   2),   S(  2,  17),   S(-19,   2),   S(-30, -31),   S( 38,  18),
            S( 65,  75),   S( 53, 102),   S(  8,  37),   S( 18,  21),   S( 13, -16),   S(  1, -12),   S(  6,   0),   S(-15,  12),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-97,  54),   S(-72,  50),   S( -6,  12),   S( -7,  17),   S(-12,  31),   S(-28,  21),   S(-41,  21),   S(-27,  26),
            S(-56,  15),   S(-61,  24),   S( 18, -13),   S( 14,   4),   S( 10,  -8),   S(-21, -15),   S(-26,  -3),   S(-28,   7),
            S(-54,  32),   S(-62,  34),   S( 35, -27),   S(-10, -23),   S( 16, -16),   S(-23, -21),   S(-14,  -5),   S( 12, -13),
            S(-56,  52),   S(-50,  37),   S( -2,   0),   S( 22,   4),   S(-19,   5),   S(-47,  -2),   S(  2,  -7),   S( 10,  11),
            S( 32,  58),   S( 32,  37),   S( 29,  40),   S( 28,  19),   S(-12,  32),   S( 65,  -9),   S( 14,  11),   S( 49,  26),
            S( 61,  43),   S( 57,  18),   S( 40,  -6),   S( 38,  -4),   S( 45, -15),   S( 21,  -6),   S( 11,   8),   S(  6,  52),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-49,  28),   S(-32,  24),   S(-31,  18),   S(-25,  17),   S( 30, -19),   S(-30,  11),   S(-61,  14),   S(-58,  17),
            S(-34,  -2),   S(-10, -15),   S(-22, -30),   S( -7,  -7),   S( 29, -17),   S( 12, -22),   S(-40,  -1),   S(-72,   6),
            S(-19,  -9),   S(-18,  -5),   S(-22, -23),   S(-41,  -6),   S( -7,  -9),   S( 48, -40),   S(-14, -11),   S(-20,   1),
            S(-32,  15),   S(-72,  13),   S(  7, -32),   S(-15, -10),   S( 11,  -3),   S( 40, -18),   S( 21,  -6),   S( 40,   0),
            S( 10,  22),   S(-43,  14),   S( 15, -30),   S( -4, -13),   S( 43,  23),   S( 71,  18),   S( 37,  11),   S( 69,  26),
            S( 62,  25),   S( 25,   0),   S(  6, -38),   S( 10, -39),   S( 20,  -1),   S( 25,   4),   S( 43, -11),   S( 45,  32),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-44, -23),   S(-42,  -9),   S(-24,  -4),   S(-48,  13),   S(-11, -19),   S( 26, -20),   S( -4, -42),   S(-40, -30),
            S(-38, -43),   S(-33, -39),   S(-41, -38),   S(-23, -43),   S(-11, -36),   S( 44, -53),   S( 48, -55),   S(-10, -39),
            S(-41, -47),   S(-53, -35),   S(-44, -48),   S(-23, -45),   S(-22, -26),   S( 25, -38),   S( 34, -52),   S( 48, -52),
            S(-19, -46),   S(-46, -48),   S(-78, -46),   S(-50, -26),   S(-13, -27),   S( 20, -19),   S( 27, -15),   S( 77, -33),
            S(  8, -36),   S( 10, -59),   S(-20, -54),   S(  0, -66),   S( 17,  -4),   S( 28,   1),   S( 65,  43),   S(101,  30),
            S(-10,   1),   S(-26, -33),   S(  4, -52),   S( -5, -53),   S( -3, -17),   S( 26, -21),   S( 48,  40),   S( 87,  56),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  71),   S(-34,  66),   S( 16,  27),   S(-10,  -1),   S( 13,  10),   S(  5,   7),   S(-34,  12),   S(-44,  24),
            S(-61,  60),   S(-58,  61),   S(-33,  42),   S(-15,  13),   S(-16,  -9),   S(-36, -13),   S(-50,  -1),   S( -2,  -8),
            S(-62,  95),   S(-13, 105),   S(-13,  62),   S(-33,  33),   S( 11, -15),   S(-102,  -6),  S(-71, -12),   S(-45,  -8),
            S(-30, 138),   S(  5, 155),   S(  9, 107),   S( 10,  48),   S(-35,  15),   S(-33, -21),   S(-29,   0),   S(-55,   7),
            S(-12, 168),   S( 43, 159),   S( 27, 162),   S( 55, 100),   S( 18,  11),   S(  1,   3),   S(-18, -14),   S( -8,  17),
            S( 53, 191),   S( 70, 210),   S( 87, 201),   S( 48,  74),   S(  6,  36),   S(-11,   5),   S(-10, -25),   S(  1,  10),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-95,  76),   S(-60,  57),   S( 14,  11),   S( 12,  31),   S( 13,  11),   S(-35,  16),   S(-69,  26),   S(-75,  30),
            S(-55,  37),   S(-53,  39),   S(-44,  29),   S(  6,  46),   S(-49,   1),   S(-25,  -9),   S(-70,   3),   S(-29,   8),
            S(-88,  66),   S(-112, 102),  S(-52,  77),   S(-108,  87),  S(-67,  53),   S(-93,   9),   S(-49, -13),   S(-48,   2),
            S(-70, 107),   S(-35, 121),   S(  3, 120),   S( 41, 124),   S(-31,  58),   S(-43,  14),   S( 10,   6),   S(-51,  21),
            S( 17, 123),   S( 24, 146),   S( 23, 156),   S( 45, 173),   S( 20, 130),   S( -6,  35),   S( -2,   3),   S( -4,   2),
            S( 21,  72),   S( 21, 125),   S( 64, 138),   S( 70, 180),   S( 29, 108),   S( -8,  -8),   S(-14,  -8),   S(-20, -19),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-88,  14),   S(-54,   6),   S( -5,   1),   S(  4,  21),   S( -9,   5),   S(-46,  26),   S(-97,  33),   S(-55,  31),
            S(-98,   6),   S(-78,  12),   S( -9, -17),   S(-22,  -9),   S(-19,  22),   S(-39,  24),   S(-118,  38),  S(-82,  16),
            S(-28, -13),   S(-83,  19),   S(-31,   1),   S(-82,  66),   S(-83,  82),   S(-25,  41),   S(-117,  53),  S(-88,  39),
            S(-101,  32),  S(-77,  33),   S(-10,   9),   S(-42,  78),   S( 16,  95),   S(-56,  80),   S(-29,  54),   S(  0,  25),
            S(-26,  45),   S(-34,  22),   S(  7,  50),   S( 26, 125),   S(103, 110),   S( 55,  65),   S(-12,  92),   S( 32,  44),
            S( -2,  14),   S(-21,  -3),   S( 20,  17),   S( 49, 116),   S( 11, 129),   S( 29,  55),   S( -6,  72),   S( 23,  96),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-71,  -5),   S(-70,  25),   S( 43, -14),   S( -2,  18),   S(  3,  39),   S(-74,  55),   S(-43,  42),   S(-61,  40),
            S(-68, -23),   S(-81, -13),   S(-33, -37),   S(-50,  16),   S(-35,  13),   S(-32,  29),   S(-93,  67),   S(-96,  44),
            S(-41, -34),   S(-60, -29),   S(-60,  -5),   S(-36,   7),   S(-58,  36),   S(-21,  59),   S(-88,  89),   S(-48,  63),
            S(-57,   6),   S(-89,  -8),   S(-29, -29),   S(-56,  18),   S(  4,  43),   S(-13,  76),   S( 15, 116),   S( 76,  70),
            S(-21,  22),   S(-44,  -5),   S( -8,  -2),   S( -8,  23),   S( 59,  94),   S( -4, 125),   S( 99, 123),   S( 91, 102),
            S(-33,  43),   S(-19,   5),   S(  6, -19),   S(  2,   1),   S( 20,  69),   S( 32, 151),   S( 65, 183),   S( 34, 173),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13,  13),   S(-15,  16),   S(-17,   3),   S(  2,   5),   S( -3, -10),   S( -9,  16),   S(-11, -13),   S(-18,  -7),
            S(-39, -26),   S( -6,  23),   S(  9,  20),   S( -1,   4),   S( -2,  32),   S( -8, -11),   S(-35, -26),   S(-30, -44),
            S(-18,  36),   S(-37,  99),   S( 18,  63),   S( 17,  38),   S(-18,  -1),   S(-49, -18),   S(-47, -44),   S(-49, -63),
            S(-46,  89),   S(-45, 127),   S( 38, 116),   S( 22,  96),   S(-21, -30),   S(-42, -35),   S(-10, -12),   S(-62, -53),
            S( 31,  94),   S( 38, 215),   S( 47, 150),   S( 18,  56),   S( -2,  15),   S( -3, -21),   S( -1,   2),   S(-21, -46),
            S( 45, 108),   S( 55, 219),   S(117, 222),   S( 46,  98),   S( -7,   4),   S(-10,  -8),   S(-11, -33),   S(-23, -40),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -19),   S(-16,  19),   S( -4,  11),   S( -2,   4),   S( -8,  -9),   S(-28,   6),   S(-32, -35),   S(-23,  -9),
            S(-41, -11),   S(-55,  50),   S(-24,  34),   S( 20,  23),   S(-45,  25),   S(-14, -14),   S(-80, -18),   S(-62,   7),
            S(-59,  46),   S(-49,  54),   S(-39,  76),   S(-13,  94),   S( -1,  32),   S(-44, -32),   S(-65, -24),   S(-80, -29),
            S(-77,  90),   S( -8, 124),   S( -4, 138),   S(  5, 123),   S( -3,  62),   S(-46,  25),   S(-18,  -8),   S(-43, -41),
            S(  0,  97),   S( 55, 170),   S( 66, 195),   S( 47, 248),   S( 21, 150),   S(-11,  15),   S( -4, -62),   S(-26, -36),
            S( 41,  70),   S( 73, 171),   S( 83, 192),   S( 84, 251),   S( 38, 106),   S(  3,  10),   S( -1,   0),   S( -6,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-28, -57),   S(-32, -15),   S( -8, -25),   S( -2,  -1),   S( -5,  -1),   S(-30,  11),   S(-31,   4),   S( -3,  44),
            S(-53,  10),   S(-55,  13),   S(-54, -28),   S( -2,  11),   S(-39,  63),   S(-17,  18),   S(-41,  23),   S(-58,  11),
            S(-64, -26),   S(-60,   9),   S(-38, -19),   S(-24,  41),   S(-21,  70),   S(-54,  35),   S(-35,   8),   S(-68,  40),
            S(-53,  13),   S(-23,  56),   S(-28,  29),   S(  9,  96),   S( -4, 132),   S(-30,  85),   S(-37,  47),   S(-36,  56),
            S(-22, -24),   S( 11,  19),   S( 14,  77),   S( 35, 134),   S( 46, 214),   S( 44, 170),   S( 11,  83),   S( 26,  40),
            S( -3,  22),   S( 18,  37),   S( 30, 113),   S( 35, 138),   S( 64, 213),   S( 57, 114),   S( 29,  95),   S( 21,  29),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -31),   S(-29, -15),   S(-10, -30),   S(  0,  -3),   S( 17,  22),   S(  2,  48),   S( -9, -18),   S(  8,  20),
            S(-44, -34),   S(-32, -11),   S(-14, -41),   S( 22,  -7),   S(-14,   1),   S(  7,  46),   S(  5,  35),   S( -1,  -4),
            S(-20, -75),   S(-33, -56),   S(-20, -51),   S(  0, -10),   S( 10,  33),   S(-17,  56),   S( -2,  75),   S(-23,  60),
            S(-28, -24),   S(-44, -27),   S(-32,   1),   S(  9,  19),   S(-13,  51),   S(  5,  93),   S(-25, 142),   S( -9,  52),
            S(-27, -42),   S(-32, -31),   S(-13,  15),   S(  0,   2),   S( 35, 115),   S( 67, 165),   S( 59, 223),   S( 73,  68),
            S( -8,   5),   S( -4,   9),   S(  2,   9),   S(  7,  23),   S( 26,  80),   S( 84, 190),   S( 32, 182),   S( 43,  37),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-29,   6),   S(  3,  13),   S(-43,  17),   S(-20,  -5),   S(-28,  -4),   S(  4, -29),   S(-42, -45),   S(-34, -16),
            S(-35,  55),   S( 24, -39),   S(-40,  16),   S( 11, -17),   S(-10, -15),   S(-22, -14),   S(-28, -23),   S(-69, -22),
            S(  5,  65),   S( -2,  -7),   S(  6,  -7),   S(-23,  37),   S( 11,  11),   S(-33,   4),   S(-12, -25),   S(-40, -47),
            S( 19, -24),   S( 42,   6),   S( 12,  20),   S( 28,  23),   S(  3,   3),   S( -3,   1),   S(-12, -16),   S( -3,  -6),
            S( 18, -31),   S( 35,   8),   S( 13,   7),   S( 64, -10),   S( 41,  -9),   S( 27,  18),   S( 20, -17),   S(-61, -15),
            S( 18, -15),   S( 10,   7),   S( 23,  14),   S( 53, -16),   S( 32, -44),   S( 17,  10),   S(  9, -25),   S( -5, -12),
            S( 14, -30),   S( 15, -39),   S( 16, -25),   S( 33, -29),   S( 20, -18),   S(-10, -26),   S( -9, -42),   S(-21, -33),
            S(-69, -56),   S( -7,   0),   S( -5, -16),   S(  0, -44),   S(-20, -22),   S( 20,   9),   S( -6,   3),   S( 15,  -3),

            /* knights: bucket 1 */
            S(-43,  24),   S(-53,  90),   S( 28,  40),   S(-25,  68),   S(-17,  52),   S(-21,  29),   S(-32,  53),   S(-17, -12),
            S( 35,  24),   S( -4,  38),   S( -4,  29),   S( -6,  45),   S( -8,  31),   S(-13,  18),   S( 19, -15),   S(-25,  13),
            S(-31,  28),   S( 14,  13),   S(  2,  14),   S( 15,  31),   S(  6,  34),   S(-27,  31),   S(-18,   8),   S(-30,  22),
            S(  1,  35),   S( 54,  23),   S( 19,  39),   S( 25,  19),   S(  7,  27),   S( -4,  25),   S( 16,   8),   S( 14,  12),
            S(  1,  46),   S( 17,  22),   S( 27,  24),   S( 37,  24),   S( 35,  24),   S( 29,  19),   S( 24,  12),   S( 18,  16),
            S(  8,  17),   S( 17,   9),   S( 14,  31),   S( 42,  14),   S( 10,  22),   S( 33,  31),   S( 24,   5),   S( 15, -10),
            S( 35,   4),   S( 27,  17),   S(-21, -17),   S( 14,  33),   S( 27,  -3),   S( 24,  -3),   S(-33,  12),   S(-11, -21),
            S(-100, -67),  S(-22, -12),   S( -4,  17),   S(  3,  28),   S(-13,   3),   S(-26, -21),   S( -5,  -5),   S(-38, -39),

            /* knights: bucket 2 */
            S(-56,   6),   S(  1,  21),   S(-32,  56),   S(-23,  58),   S(-33,  60),   S(-36,  75),   S(-17,  31),   S(-19,  13),
            S(-14, -17),   S(-18,  12),   S(-12,  20),   S(-11,  37),   S( -5,  26),   S(-14,  55),   S(-33,  57),   S(-39,  66),
            S(-17,  23),   S( -5,  12),   S(-10,  32),   S( 14,  24),   S(  2,  36),   S(  6,  14),   S( -8,  43),   S(-25,  28),
            S( -9,  35),   S(-26,  38),   S(  5,  35),   S(  1,  45),   S( -2,  40),   S( -8,  34),   S(  3,  36),   S( -4,  38),
            S( 16,  22),   S(-16,  31),   S( -5,  43),   S(-18,  51),   S(  0,  45),   S( -8,  40),   S(  2,  30),   S( -1,  19),
            S(-24,  32),   S(  1,  30),   S(-26,  51),   S(-20,  49),   S(-27,  45),   S( -3,  27),   S(-30,   9),   S( 15,   1),
            S(-21,  24),   S(-31,  17),   S(-33,  21),   S(-37,  37),   S(-15,  18),   S(  2,  24),   S(-50,  36),   S(-34,  11),
            S(-145,  19),  S( -3,   0),   S(-80,  33),   S(-28,  13),   S( -3,  11),   S(-59,   2),   S( -3,   0),   S(-177, -56),

            /* knights: bucket 3 */
            S(-47, -12),   S( 16, -29),   S(-20,  -4),   S( 10,  -3),   S( 13,  -8),   S(-11,  11),   S( 25, -16),   S( -4, -25),
            S(-10,  -1),   S(-22,  -9),   S(-15,  -7),   S( 10,  11),   S( 23,  -2),   S(  1,  -7),   S(  0,  -9),   S(-16,  56),
            S(  3, -33),   S(  6,  -2),   S(  5,   1),   S( 18,  10),   S( 21,  24),   S( 26,   5),   S( 17,   2),   S( 14,  31),
            S(  2,  -3),   S( 12,   8),   S( 18,  28),   S( 22,  25),   S( 32,  30),   S( 27,  26),   S( 31,  15),   S( 26,  11),
            S( 27,   2),   S(  7,  14),   S( 37,   6),   S( 31,  37),   S( 28,  36),   S( 36,  41),   S( 42,  32),   S( 20,  10),
            S(  4,   7),   S( 31, -14),   S( 46,  -2),   S( 59,   3),   S( 70, -17),   S( 73,  -9),   S( 14,   6),   S( 13,  38),
            S( 28,  -6),   S( 16,   6),   S( 44, -21),   S( 51,  -8),   S( 66, -31),   S( 60, -35),   S( 63, -66),   S( 47, -23),
            S(-104,   9),  S(-24,   6),   S(-28,   3),   S(  3,  16),   S( 32,  -7),   S( -8, -11),   S(-11, -22),   S(-73, -47),

            /* knights: bucket 4 */
            S( 13,  17),   S(-46,   6),   S( 14,  27),   S( -2,  -5),   S(-19, -11),   S(-29, -23),   S( -8, -52),   S(-30, -45),
            S( 32,  23),   S(-21,  37),   S( 16, -21),   S( 15,  -5),   S( 21, -14),   S( -4, -40),   S( 11,  -3),   S(  1, -47),
            S(-10,  29),   S(  9,  39),   S( 10,  10),   S( 20,  16),   S( -4,   3),   S(-44,  18),   S(-47, -28),   S(-32, -56),
            S( -3,  64),   S( 31, -23),   S( 45,  25),   S( 23,  18),   S( 12,  13),   S( 92, -17),   S( 27, -31),   S( -1, -19),
            S( 56,  28),   S(-20,  43),   S( 40,  46),   S( 41,  18),   S( 40,  34),   S(-15,  26),   S( -5, -29),   S(-10,  -9),
            S(  7,  16),   S(-34,  -4),   S( 77,  15),   S(  6,   7),   S( 11,  19),   S( 20,  19),   S( 13,  29),   S(-11, -22),
            S( -6,   7),   S(-16,   8),   S( 13,   0),   S(  4,  37),   S(  9,  11),   S(  6, -15),   S(  5,  -6),   S(-14,  -2),
            S(-11,  -8),   S( -2,  -6),   S( 10,  11),   S(  1,   5),   S( -6,  -8),   S(  9,  21),   S( -2,   5),   S( -3, -19),

            /* knights: bucket 5 */
            S( 11,  -2),   S(-37,  46),   S( 30,  36),   S( 23,  48),   S( 37,  25),   S( 14,   2),   S(  2,  18),   S(-21, -21),
            S( 12,  -1),   S( 32,  46),   S( 22,  25),   S( -7,  44),   S( 36,  38),   S(  5,  37),   S( 23,  27),   S(-14, -26),
            S(  5,  25),   S(-10,  41),   S( 61,  23),   S( 37,  47),   S(-17,  54),   S( -2,  30),   S(-17,  19),   S(  7,  -4),
            S( 29,  46),   S(  9,  49),   S( 31,  44),   S( -6,  62),   S( 11,  51),   S(  4,  45),   S( 24,  41),   S( 10,  34),
            S( 20,  50),   S( 26,  36),   S( 41,  52),   S( 52,  46),   S( 74,  48),   S( 24,  44),   S( 40,  34),   S( 35,  30),
            S(  4,  30),   S( -7,  50),   S( 17,  30),   S(  8,  55),   S( 32,  46),   S( 13,  52),   S( 17,  15),   S( -6,  31),
            S( 20,  56),   S( -5,  66),   S( 31,  48),   S( 15,  65),   S(  7,  55),   S(  9,  48),   S( 23,  68),   S(  4,   1),
            S( -4,   7),   S(  0,  13),   S(  9,  41),   S( -3,   6),   S( 10,  42),   S(  2,  35),   S(  8,  38),   S(-17, -17),

            /* knights: bucket 6 */
            S(  2, -41),   S(-18,  -4),   S( 32,  29),   S(-23,  41),   S(-24,  49),   S( 20,  40),   S( -8,  34),   S(-12,  24),
            S( -4, -30),   S( 52,   1),   S( 18,  13),   S(-30,  41),   S(-58,  71),   S( 29,  52),   S( 21,  50),   S(  0,   8),
            S(-22, -18),   S(  0,   3),   S( -7,  28),   S( 19,  37),   S(-22,  65),   S(-37,  61),   S( 13,  48),   S(  1,  43),
            S( 32,   5),   S( 32,  12),   S( 40,  31),   S( 70,  29),   S( 15,  53),   S(  8,  55),   S(  7,  62),   S(-23,  69),
            S(  4,  34),   S( 63,  -7),   S( 51,  36),   S( 62,  35),   S( 74,  39),   S( 74,  39),   S( 16,  57),   S( 18,  51),
            S( 24,  26),   S( 10,  15),   S( 64,  22),   S( 42,  45),   S( 51,  48),   S( 27,  31),   S( 15,  40),   S( 36,  37),
            S(-22,  21),   S(  3,  35),   S(-24,  38),   S( 30,  33),   S(  2,  61),   S( 22,  43),   S( 21,  71),   S( -7,  28),
            S(-41,  -1),   S( 16,  40),   S( 28,  38),   S( 10,  40),   S( 22,  35),   S( 11,  59),   S( 20,  57),   S( 11,  24),

            /* knights: bucket 7 */
            S(-34, -55),   S(-188, -44),  S(-69, -44),   S(-57, -13),   S(-38,  -8),   S(-30, -17),   S( -8,   3),   S(-17,   5),
            S(-49, -76),   S(-37, -46),   S(-35, -30),   S(-47,   5),   S(-44,  11),   S(  6, -11),   S(-14,  47),   S(  5,  28),
            S(-79, -65),   S(-55, -35),   S(-49,   1),   S( 19, -15),   S(-18,  11),   S(  1,  13),   S(-14,  58),   S( 46,  54),
            S(-60, -24),   S(  8, -24),   S(-12,  11),   S( 27,  -2),   S( 39,  -1),   S(  6,  16),   S(  8,  14),   S(-22,  32),
            S(-59, -23),   S(-22, -29),   S( 48, -22),   S( 73, -15),   S( 98,  -5),   S( 57,  22),   S( 84,   0),   S( 71,  18),
            S( -7, -40),   S( 17, -40),   S(-24,   0),   S( 24,   2),   S( 57,  12),   S( 67,   7),   S( 47, -19),   S( -6,   7),
            S(-33, -33),   S(-64, -20),   S(  5, -13),   S( 32,  18),   S( 34,  22),   S( 39,   1),   S(-20,  23),   S(  1,   4),
            S(-37, -29),   S( -9, -10),   S(-26, -14),   S(  8,  13),   S( 10,   4),   S( 21,  18),   S( -3, -10),   S( -4,  -7),

            /* knights: bucket 8 */
            S( -1,  -8),   S( -8,  -8),   S( -3,  -4),   S( -9, -30),   S(-10, -39),   S(-10, -51),   S( -2,  -1),   S( -5, -23),
            S(  2,   1),   S( -6, -11),   S( -7, -29),   S(-18, -43),   S(-28, -27),   S(-17, -70),   S(-13, -58),   S(-16, -36),
            S(  4,  18),   S(-20, -18),   S( 25,   9),   S(  4,   0),   S(  3, -30),   S(-15, -10),   S(-12, -36),   S( -8, -41),
            S(-18,  -1),   S( -1,  -2),   S( -3,  15),   S(  4,  33),   S(  8,  -2),   S(  6,   7),   S(-14, -52),   S( -3, -17),
            S( 26,  52),   S( 10,   8),   S( 12,  36),   S( 33,  18),   S( 10,  30),   S( -5,  -6),   S(  4, -21),   S( -7,  -9),
            S( 13,  36),   S(  8,   4),   S( 27,  23),   S( 30,  15),   S(  2,  -1),   S( -1,  -7),   S( -7, -28),   S( -6,  -9),
            S(  2,  11),   S(  1,   3),   S(  6,  11),   S( 10,  11),   S(  7,   8),   S(  5,  21),   S(  2,  12),   S( -1,   2),
            S(  1,   0),   S( 11,  32),   S(  5,  15),   S( -2,   0),   S(  2,  11),   S( -5, -20),   S(  3,   5),   S( -3,  -4),

            /* knights: bucket 9 */
            S(-10, -31),   S(-20, -36),   S(-18, -47),   S( -3, -14),   S(-22, -53),   S(-14, -38),   S( -3, -13),   S( -4, -27),
            S(-12, -38),   S(-12,  -1),   S(-10, -50),   S(-11,  -7),   S( -3, -13),   S( -7, -33),   S( -5,  -2),   S(-15, -42),
            S(  5,   6),   S( -9, -14),   S(  6, -14),   S(  4,   5),   S(  4,  20),   S(-30,   0),   S(-11,  -9),   S( -8, -18),
            S(-14,  -3),   S( -6,  -8),   S(  4,  32),   S( 15,  34),   S( 28,  26),   S(  9,  23),   S(-12, -35),   S( -3,  -1),
            S(  0,  21),   S( 20,   8),   S( 18,  41),   S( -2,  46),   S(  6,  18),   S( 13,  -6),   S(  1, -29),   S(  5,   8),
            S(  1,   0),   S(  6,  32),   S( 14,  35),   S(-11,  22),   S( 33,  39),   S( 14,  10),   S(  7,  11),   S( -7, -24),
            S(  1,   0),   S( -1,  21),   S( 18,  38),   S( 11,   5),   S( 13,  42),   S( -2, -15),   S(  4,  17),   S( -2,  -1),
            S(  1,   0),   S(  3,   8),   S( 12,  27),   S( 15,  30),   S(  9,  10),   S(  0,   4),   S(  3,   4),   S(  0,  -4),

            /* knights: bucket 10 */
            S(-18, -50),   S(-16, -54),   S(-13, -26),   S(-17, -21),   S(-12, -11),   S(-13, -43),   S( -3,  16),   S(  4,  20),
            S( -6, -24),   S( -7, -14),   S(  1, -17),   S(-18, -34),   S(-22, -36),   S( -8, -40),   S( -8,  -8),   S( -5, -13),
            S(-16, -50),   S(-17, -61),   S( -6, -11),   S(-13, -13),   S( 14,   5),   S(-11,  -1),   S( -6,   3),   S( -8,   5),
            S( -8, -19),   S( -6, -45),   S(  4, -33),   S( 16,  16),   S(  7,  41),   S( 16,  25),   S(  5,  17),   S( 11,  43),
            S( -6, -46),   S(-13, -31),   S( 15,  12),   S( 21,  34),   S( 16,  54),   S( -1,  28),   S( 18,  13),   S( 21,  51),
            S(-10, -40),   S( -5, -21),   S( -4,  -9),   S( 12,  45),   S( 32,  65),   S( 30,  43),   S( 27,  58),   S( 16,  52),
            S(  0,  -2),   S(-10, -32),   S(  2,  -7),   S( 27,  27),   S( 19,  30),   S(  9,  33),   S(  1,  -3),   S(  9,  24),
            S( -3, -17),   S(  3,  11),   S( -7, -18),   S(  4,  -4),   S( 12,  39),   S(  5,  25),   S(  2,  12),   S( -1,  -3),

            /* knights: bucket 11 */
            S( -1,  -1),   S(-18, -28),   S( -7, -43),   S( -9, -25),   S(-20, -49),   S(-12, -17),   S( -6,  -5),   S( -4,  -6),
            S( -7,  -8),   S(-12, -21),   S(-14, -76),   S(-27, -23),   S( -7,   0),   S(-29, -37),   S(-16, -30),   S( -8, -10),
            S(-14, -53),   S(-21, -61),   S(-23, -32),   S(  1,   6),   S(-14,   8),   S(-16,  20),   S(  8,  -5),   S(  0,  15),
            S(-13, -30),   S( -8, -31),   S(-26,  -4),   S( 26,  31),   S( 15,  21),   S( 14,  10),   S( 12,  25),   S( 16,  29),
            S( -3, -24),   S(-19, -58),   S(  6, -19),   S(  0,   8),   S( 13,  22),   S( 29,  54),   S(  5,  -3),   S( 24,  66),
            S( -7, -11),   S( -6, -25),   S(  1,  -4),   S( 38,  34),   S( 17,  22),   S( 49,  49),   S( 22,  23),   S( 13,  26),
            S(  9,  27),   S( -2,  -6),   S(  8,  -9),   S( 12, -15),   S( 21,  32),   S(  1,   8),   S( 16,  38),   S( 20,  54),
            S( -3,   1),   S( -2, -18),   S(  8,  12),   S(  1,   4),   S(  2,  12),   S(  2,   4),   S(  3,   5),   S(  2,  11),

            /* knights: bucket 12 */
            S( -3,  -8),   S( -1,   3),   S( -2, -20),   S( -2,   2),   S( -4, -11),   S( -3,  -4),   S(  3,   5),   S( -2, -14),
            S(  0,   0),   S(  1,   2),   S(  2,   5),   S( -3, -11),   S( -1,   5),   S( -4, -20),   S( -2, -11),   S(  1,   9),
            S( -5, -14),   S(  5,   4),   S( -6, -12),   S( -6, -21),   S(  0,   3),   S( -5, -17),   S(  2,  -4),   S( -7, -30),
            S( -7, -13),   S( -1,   1),   S( -8, -23),   S(  5,  14),   S( -5,  -3),   S(  0,   6),   S( -2,  -7),   S( -1, -10),
            S(  9,  16),   S(  5,   5),   S( -6, -12),   S(  0,   3),   S( -5, -26),   S(  0,   3),   S( -1, -12),   S( -1,   1),
            S(  1,  -9),   S( -5, -23),   S(  1,   0),   S( -2,  -6),   S(  4,  10),   S( -5, -17),   S( -1,  -8),   S(  0,   3),
            S(  2,   6),   S( -8, -11),   S( -1,  10),   S(  1,  -9),   S( -5,  -7),   S( -5, -21),   S( -2,  -1),   S(  0,  -1),
            S(  2,   3),   S(  1,  13),   S( -2,  -3),   S(  2,  -2),   S( -2,  -4),   S( -2,  -9),   S( -3,  -9),   S( -1,   1),

            /* knights: bucket 13 */
            S( -4, -11),   S( -1,   1),   S( -2,  -6),   S( -2,  -1),   S( -8, -13),   S( -1,   1),   S( -3, -12),   S(  1,  -1),
            S( -2,  -7),   S(  1,   5),   S( -2, -23),   S(-10, -21),   S( -6, -30),   S( -4, -25),   S(  0,   1),   S(  1,  -1),
            S( -4, -10),   S( -8, -31),   S(  7,  18),   S(  0,  -1),   S(-12, -38),   S( -9, -23),   S( -2, -14),   S( -6, -28),
            S( -8, -15),   S(  5,  12),   S(  1,   2),   S(-11, -25),   S( -2,  -7),   S(  5,  12),   S(  0, -13),   S( -5, -10),
            S(  3,  11),   S( -2,  -3),   S(  2,  -8),   S( 11,  22),   S(  4, -13),   S( -3,  -8),   S(  2, -13),   S(  1,   1),
            S( -3,  -9),   S( 14,  14),   S(  7,  23),   S(-14,  11),   S(  5,   5),   S(-10, -33),   S(  4,   5),   S( -4,   2),
            S(  1,   7),   S(  2,   5),   S(  9,  12),   S(  8,  12),   S( 14,  23),   S( -4, -21),   S( -2,  -2),   S( -5,  -3),
            S( -1,   1),   S( -1,  -6),   S( -1,   1),   S(  1,  -8),   S( -1,  -1),   S(  3,  -1),   S(  0,  -1),   S( -1,   0),

            /* knights: bucket 14 */
            S( -3, -24),   S( -5, -25),   S( -2,  -2),   S( -3,   3),   S( -8, -24),   S( -2, -14),   S( -1,  -5),   S(  0,   1),
            S(  0,  -2),   S( -3, -10),   S(-15, -60),   S( -8, -36),   S( -1,  -9),   S(  1,   6),   S(  0,  -4),   S( -1,   3),
            S( -6, -12),   S( -4, -25),   S(-10, -53),   S(  1,   2),   S( -4, -20),   S( -4,  -9),   S(  0,  -1),   S(  1,   8),
            S(  0,   5),   S( -6, -32),   S(-15, -39),   S(-12, -36),   S( -2, -21),   S(  2,  -2),   S( -3, -16),   S( -7, -11),
            S( -2,  -4),   S( -2, -17),   S(  1,  23),   S( -7, -30),   S( -9,  -7),   S(  2,  22),   S(  2,   4),   S( -3,  -6),
            S( -4,  -8),   S(  3,  -3),   S( -9, -30),   S(  5,   2),   S( 14,  25),   S(  3,   8),   S( -3,  -1),   S(  0,  -4),
            S(  0,  -3),   S( -2, -10),   S(  7,  -3),   S(  0,  -7),   S( -7,  -9),   S( -2,  -8),   S( -5,  -2),   S(  1,   7),
            S( -1,  -2),   S(  2,   4),   S( -1, -10),   S(  7,  -1),   S(  5,  19),   S(  1,   3),   S( -2,  -8),   S( -1,  -5),

            /* knights: bucket 15 */
            S( -2,  -8),   S( -1, -14),   S( -1, -12),   S( -7, -13),   S( -2,  -1),   S( -2,  -5),   S(  1,   0),   S(  0,  14),
            S( -2,  -6),   S(  0,  -2),   S( -4, -18),   S( -6, -26),   S( -2,  -3),   S( -1,  -8),   S(  0,   0),   S( -1,  -3),
            S( -6, -16),   S( -7, -16),   S( -3, -11),   S(-14, -38),   S( -5, -24),   S( -2,  -3),   S( -2,  -1),   S( -2,   0),
            S( -7, -17),   S( -6, -32),   S( -7, -20),   S(  0,  -8),   S(  0, -18),   S(  7,  23),   S(  5,  10),   S( -3,  -1),
            S(  0,  -2),   S( -2,  -5),   S( -2, -17),   S( -8, -13),   S(  3,  18),   S(  3,  10),   S( -6,  -7),   S( -2,   2),
            S( -3,  -4),   S( -2,  -5),   S( -2, -21),   S( -3,   7),   S( -5, -13),   S( -6,  13),   S( -4,   3),   S(  2,   7),
            S( -3, -13),   S( -1,  -6),   S( -1,  -8),   S( -4,  -7),   S(-10, -14),   S( -4,  16),   S( -2,  -8),   S(  3,  13),
            S(  0,  -3),   S(  0,  -1),   S( -3,  -9),   S( -2,  -9),   S( -2,  -5),   S( -9,  -6),   S(  7,  18),   S( -3,   1),

            /* bishops: bucket 0 */
            S( 15,  18),   S( 24, -10),   S( 43,  20),   S(  9,  24),   S( -2,  -1),   S( 18,  -2),   S( 27, -37),   S(  2, -35),
            S( 48, -43),   S( 79,   4),   S( 37,  12),   S( 18,   6),   S(-13,  37),   S(  2, -18),   S(-36,   1),   S( 11, -48),
            S( 27,  41),   S( 51,  10),   S( 29,   6),   S( 13,  56),   S( 20,  15),   S(-31,  27),   S( 10, -22),   S( 11, -39),
            S( 17,  10),   S( 64, -12),   S( 37,   9),   S( 38,  30),   S(  2,  30),   S( 28,   1),   S(-10, -10),   S(  0,  -1),
            S( 17,   4),   S( 29,  21),   S(  5,  36),   S( 54,  12),   S( 59,   0),   S( 16,  -5),   S( 20, -20),   S(-35,  -2),
            S(-37,  63),   S(  1,  17),   S( 54, -20),   S( 83, -22),   S( 33,  32),   S( -9,  -1),   S(  0,  11),   S(  1,  10),
            S(-13,  14),   S( 10,   1),   S( 43,  -7),   S(  2,  37),   S(-29,  -3),   S( 23,  28),   S(  3, -11),   S(-14,  -8),
            S(-34, -43),   S( 11,   4),   S(  4,   8),   S(  5, -11),   S( 19,  27),   S( 34,  12),   S(  0,  44),   S(-22,   1),

            /* bishops: bucket 1 */
            S( 37,   9),   S( -5,  30),   S(  9,  39),   S( 17,  30),   S( -6,  31),   S(  2,  35),   S( -6,   5),   S(-45,  -5),
            S( 11, -14),   S( 35, -12),   S( 52,  10),   S( 27,  33),   S( -8,  19),   S(  9,   3),   S(-32,  -3),   S( 15, -16),
            S( 40,  -3),   S( 22,   6),   S( 40,  -7),   S( 22,  26),   S( 21,  29),   S(-19,   6),   S( 29,  -2),   S(  4, -28),
            S( 41,   3),   S( 17,  16),   S( 13,  13),   S( 36,  18),   S(  3,  22),   S( 21,   4),   S( -9,   6),   S( 15, -12),
            S( 36,  29),   S(  8,  20),   S( 22,  22),   S(  0,  29),   S( 24,   9),   S( -2,  16),   S( 28, -19),   S( -8,  10),
            S(  1,  22),   S( 33,  33),   S( 28,   4),   S( 53,  -5),   S( 16,  18),   S( 33, -16),   S(  0,  27),   S( 48, -16),
            S( -9,  46),   S(-25,  22),   S( 17,  30),   S( 36,  27),   S( 42,  28),   S(-19,  25),   S( 38, -18),   S(-18,  39),
            S( 14,   4),   S( 11,   7),   S(  6,  14),   S(-19,  23),   S( 22,  18),   S( -7,   6),   S( 14,   6),   S( -2,  12),

            /* bishops: bucket 2 */
            S( 18, -15),   S(  9,  19),   S( -1,  22),   S(-27,  53),   S( -8,  38),   S(-27,  33),   S(-16,  -4),   S(-44,  18),
            S(-22,  23),   S(  6, -15),   S( 21,  14),   S( -3,  31),   S( -2,  38),   S( 17,  11),   S( -8, -19),   S(  4, -33),
            S( -5,   5),   S( -1,  13),   S(  6,   9),   S( -2,  46),   S(  8,  36),   S(  0,  19),   S( 18,  15),   S(-16,  -2),
            S(  2,  11),   S(-12,  12),   S(-12,  34),   S(  2,  34),   S( -8,  39),   S(  7,  20),   S(  4,  15),   S(  9,   1),
            S(  9,   5),   S(-18,  31),   S( -8,  25),   S(-32,  43),   S(-10,  34),   S(-11,  44),   S(  3,  19),   S(-27,  32),
            S(  7,  26),   S( -4,  12),   S(-27,  28),   S(-17,  25),   S( 10,  15),   S( -9,   9),   S( -5,  54),   S(  1,  25),
            S(  1,  21),   S(-22,  12),   S(-26,  55),   S( 21,   3),   S( -3,   5),   S(-18,  11),   S(-67,  14),   S(-36,  37),
            S(-53,  31),   S(-35,  45),   S(-25,  28),   S(-36,  26),   S(-49,  39),   S(-32,  18),   S(  5,  16),   S(-70,   7),

            /* bishops: bucket 3 */
            S(  0,   4),   S( 36,  -7),   S( 31,  20),   S( 17,  21),   S( 21,  14),   S( 43,  -3),   S( 47, -21),   S( 42, -61),
            S( 11,   5),   S(  8,  -1),   S( 31,  -2),   S(  9,  35),   S( 22,  12),   S( 23,  26),   S( 45,  -2),   S( 39,  -3),
            S( 21,   9),   S( 15,  19),   S( 12,  17),   S( 26,  25),   S( 25,  55),   S( 25,  10),   S( 42,  26),   S( 48, -11),
            S( 31,  -6),   S( 23,   7),   S( 17,  29),   S( 23,  42),   S( 29,  36),   S( 31,  27),   S( 26,  18),   S( 26,  -5),
            S( 20,   2),   S( 23,  12),   S( 42,  11),   S( 26,  42),   S( 23,  44),   S( 35,  26),   S( 17,  29),   S( 25,  31),
            S( 30,   4),   S( 34,  20),   S( 22,  11),   S( 40,  13),   S( 24,  20),   S( 50,   4),   S( 51,  13),   S(  7,  68),
            S( 19,   9),   S( -6,  16),   S( 42,  24),   S( 22,  19),   S( 12,  18),   S( 16,   5),   S( -2,  29),   S( 21,  34),
            S(-33,  59),   S(  0,  33),   S( 58,  10),   S( 23,  17),   S(-16,  35),   S( -2,  35),   S( 24,   3),   S( 56, -26),

            /* bishops: bucket 4 */
            S(-22, -26),   S(-20,   9),   S(-30,  -1),   S(-20,  20),   S(-21,  30),   S(-43,  30),   S(  1,  -8),   S(-12, -11),
            S( -6,   9),   S(  4,   8),   S( -6,  36),   S(-26,  20),   S(-16,  -2),   S( 39,  -2),   S(-21,  -7),   S( 14,  -2),
            S(-12,  -2),   S(-34,  39),   S( 14, -13),   S(-23,  19),   S(  4,  30),   S( 31, -20),   S(-24,  -5),   S(-53,   0),
            S(-36,  26),   S( -6,  36),   S( 49,  30),   S( 25,  33),   S( 10,  19),   S( 50,  -9),   S( 47,  -9),   S(-13, -36),
            S(  2,  18),   S( -3,  44),   S(-23,  53),   S( 13,  39),   S( 30,   6),   S( 32, -20),   S(-12, -24),   S( 16, -10),
            S( -7,  35),   S( 16,  14),   S(-19,  25),   S( 13,  12),   S( 39,   8),   S(  6, -16),   S( 16, -37),   S(  4,  -6),
            S(-15,  10),   S( 31,  18),   S( 18,  21),   S( 25,  19),   S( 12,  -4),   S(  4,  18),   S(  1,   5),   S(  7, -26),
            S( 11, -17),   S( -9, -36),   S(  2,  -2),   S( -3,   1),   S(  7, -11),   S(  1,  10),   S( -1,  -8),   S( -5,   1),

            /* bishops: bucket 5 */
            S(-15, -13),   S(-10,  36),   S(-31,  31),   S( -9,  31),   S(-30,  33),   S(  7,  13),   S( -6,  18),   S(-23,  13),
            S(-25,  38),   S(-12,   6),   S(-24,  57),   S(  7,  28),   S(-20,  35),   S(-25,  29),   S(-32, -12),   S( -8,  -1),
            S(  2,  17),   S(  1,  40),   S( 25,  14),   S(-18,  54),   S(  5,  38),   S(-25,   0),   S(-23,  35),   S(-17,   8),
            S( 29,  15),   S( 22,  30),   S(-16,  60),   S( 27,  31),   S( 27,  36),   S( 14,  29),   S( 17,  -6),   S( 12,  27),
            S( 25,  45),   S( 30,  14),   S( 46,  31),   S( 69,  34),   S( 41,  20),   S( 37,  18),   S( 35,  11),   S( -4,   4),
            S( 22,  39),   S( 21,  46),   S( 30,  21),   S( 18,  36),   S( -5,  35),   S( 14, -18),   S(-23,  44),   S(  4,  33),
            S(  2,  39),   S(-26,  15),   S( 16,  43),   S(  9,  55),   S( 30,  31),   S( 36,  41),   S(  0,  20),   S( -1,  30),
            S( -1,  -7),   S( 16,  37),   S( 17,  14),   S(  7,  38),   S(  4,  58),   S( 15,  26),   S( 30,  56),   S( -7,  -1),

            /* bishops: bucket 6 */
            S(-11,  15),   S(  6,  28),   S(-18,  28),   S(-24,  33),   S(-24,  24),   S(-28,  30),   S(-16,  52),   S(-18,   8),
            S( 22,   8),   S(  7, -12),   S(-15,  32),   S(  5,  29),   S(-23,  44),   S(-10,  26),   S(-93,  29),   S( 17,  24),
            S( 23,   1),   S( 17,   9),   S( 36,  -2),   S( 28,  29),   S( 42,  26),   S( 21,  10),   S( 11,  31),   S(-36,  20),
            S( -8,  39),   S( 16,  13),   S( 29,  21),   S( 23,  36),   S( 42,  32),   S( 34,  28),   S( 29,  33),   S(-16,   0),
            S( -2,  20),   S( 53,   3),   S( 21,  24),   S( 44,  22),   S( 89,  25),   S( 52,  26),   S( 31,  28),   S(-29,  46),
            S( 12,  11),   S(-46,  46),   S(  8,  18),   S(  6,  41),   S( 29,  30),   S( 20,  27),   S( -4,  46),   S(-12,  46),
            S(-21,  31),   S(-26,  26),   S(  7,  41),   S( -7,  35),   S( 48,  24),   S( 25,  32),   S( -7,  35),   S( -2,  35),
            S(  6,  45),   S( 15,  35),   S( 13,  41),   S(  4,  47),   S(-13,  40),   S( 35,  19),   S( 13,  25),   S( 12,   9),

            /* bishops: bucket 7 */
            S(-15, -37),   S( -4,   6),   S(-30, -26),   S(-47,  12),   S(-26, -10),   S(-73,  19),   S(-68, -27),   S(-64,   7),
            S(-30, -28),   S(-55, -39),   S(-15,  -4),   S(  3, -13),   S(-25,   2),   S(-37,  14),   S(-45, -12),   S(-31,   7),
            S(-29, -20),   S(  8, -16),   S( 28, -38),   S( 26,   0),   S(-30,  22),   S(-19, -12),   S(-36,  47),   S(-29,  27),
            S(-35,  15),   S( 52, -37),   S( 74, -25),   S( 51,   0),   S( 75,  -3),   S(  3,  21),   S( 21,  32),   S(-15,  28),
            S( 28, -49),   S( -9, -25),   S( 58, -38),   S( 93, -30),   S( 59,  21),   S( 61,  15),   S(-15,  42),   S( 20,   9),
            S(-23, -13),   S(-26,  -2),   S( 29, -46),   S( 15,  -4),   S( 38,  -8),   S( 41,   1),   S( 42,  13),   S( 21,   1),
            S(  4, -16),   S(-38,  -7),   S( 13,  -3),   S( 14,  -6),   S( 19, -19),   S( 34,  -5),   S( 11,   1),   S( 13,  15),
            S(-13,  -5),   S( -7,  14),   S(-29,  11),   S(  7,  -4),   S( 12,  -2),   S( 20,  -4),   S( 26,  10),   S(  7,  12),

            /* bishops: bucket 8 */
            S(-10,  -9),   S(-12, -33),   S(-39,  -4),   S( -3, -26),   S( -6,  20),   S(-22,  -1),   S(  7,  23),   S( -4,  -8),
            S( -5,  -2),   S(-31, -45),   S(-12, -21),   S(-14,  -4),   S( 12,  -9),   S(-17, -26),   S(-17, -53),   S( -4,  -7),
            S(  2,   2),   S( -7,  12),   S(-23,   8),   S( -7,  20),   S( -2,  15),   S( -6, -39),   S(  7, -40),   S(-31, -35),
            S(  6,  33),   S( -6,  44),   S(  5,  42),   S( -5,  13),   S( 13,  18),   S( -4,   8),   S(  2, -19),   S( -4, -16),
            S( 15,  37),   S( 12,  67),   S(-13,  33),   S( 42,  41),   S( -1,  18),   S( 14,   5),   S(  7, -32),   S( -9, -15),
            S( -3,   5),   S( 12,  36),   S(  8,  19),   S(-15,  17),   S( 27,   7),   S(-11, -17),   S(-16, -16),   S(-18, -20),
            S( -4,   5),   S( 11,  25),   S( 10,  23),   S(  2,   2),   S(  6,  13),   S( -1,  23),   S(-12, -14),   S( -9, -27),
            S( -8, -12),   S(  1, -26),   S(  0,  -7),   S(  0, -13),   S(-19,  -9),   S( -5,  -4),   S(  1,  14),   S( -7,   8),

            /* bishops: bucket 9 */
            S(-25, -32),   S( -6,   2),   S(-17,   4),   S( -8, -22),   S(-31, -27),   S(-17, -36),   S(-17, -10),   S(  8,  -4),
            S(-16, -17),   S(-36, -31),   S( -8,  -9),   S(-11,  16),   S(-44,  28),   S(-17, -15),   S(-14, -19),   S( -4,  -5),
            S(  9,  -1),   S( 20,  12),   S(-25, -17),   S(-13,  25),   S(  6,  15),   S( -7, -23),   S(-12, -21),   S( -3,  26),
            S(-15,   9),   S( 14,  17),   S(-12,  29),   S(  5,  23),   S( 18,  24),   S(  8,   3),   S(  3,  -5),   S(-16, -22),
            S( -1,  20),   S( 21,  24),   S(  5,  42),   S( 10,  52),   S(-19,  16),   S( -1,  29),   S( -9,  33),   S( -6,  -5),
            S(-12,   2),   S( 17,  47),   S(  1,  19),   S( 21,  24),   S( 11,  35),   S( -8,  -6),   S(-21,   0),   S(-12, -11),
            S(  4,  14),   S( 21,  11),   S(  7,  11),   S(  3,  47),   S( 20,  42),   S(  7,   6),   S( -7, -14),   S( -4,  -2),
            S( -4, -27),   S( -7,  22),   S( -4,  19),   S(-18, -11),   S(-14,  -2),   S(  6,  27),   S(  2,   3),   S(-13, -18),

            /* bishops: bucket 10 */
            S(-23, -13),   S(  4, -26),   S(-32, -26),   S(-15, -22),   S(-20,  -9),   S(-21, -20),   S(-11, -21),   S(-18, -28),
            S(  6, -16),   S(-27, -37),   S( -2,  -9),   S(-37,   4),   S(-34,   8),   S(-18,  21),   S(-28, -55),   S(-11, -17),
            S( 11, -10),   S(  5,  -7),   S(-34, -46),   S(  6,  10),   S(-33,  31),   S(-37,  15),   S(-20,  29),   S(  5,  16),
            S(-10, -19),   S(  4,   7),   S(  8,  -6),   S( 17,   4),   S(  9,  28),   S(-11,  57),   S(  4,  31),   S( 15,  26),
            S(-17,   1),   S(  0,  -1),   S( -8,  16),   S( 25,  29),   S( -2,  62),   S( 20,  50),   S(  7,  39),   S(  2, -16),
            S(  2, -24),   S(-27,  -3),   S(-25, -14),   S(-13,  30),   S( 23,  42),   S( 34,  25),   S(  8,  49),   S(  1,   7),
            S(-21,  -8),   S(-10, -45),   S( -8,  -8),   S( 24,  14),   S( -3,  -3),   S( 18,  40),   S( 17,  36),   S( 12,  13),
            S( -6, -28),   S(-10,   4),   S(  5,  18),   S( -9,   5),   S( -8,  18),   S( -8,  -6),   S( 11,   1),   S(  4,  22),

            /* bishops: bucket 11 */
            S(-19,   3),   S(-31, -11),   S(-46, -44),   S(-20, -29),   S(-17, -10),   S(-62, -46),   S( -9, -12),   S(-22, -22),
            S(-11, -16),   S( -1, -37),   S( -6,  -2),   S(-24, -33),   S(-44,  -7),   S(-28, -28),   S(-24, -43),   S(-22, -34),
            S( -8, -46),   S(  4, -45),   S(-27, -22),   S(  2,  -6),   S( -1,  -2),   S(-33,  12),   S( -8,  26),   S( -2,  20),
            S(-16, -38),   S(-15, -38),   S(  5, -12),   S(  0,  -9),   S( 10,  19),   S( -2,  58),   S(  7,  49),   S( 16,  27),
            S( -9, -24),   S(-19, -45),   S(-20,  18),   S( 46,  -1),   S( 26,  37),   S( -4,  58),   S( 15,  56),   S( 13,  25),
            S(-17, -50),   S(-33,  -4),   S(-14, -41),   S(  3,  11),   S(  2,  31),   S( 13,  29),   S( 24,  39),   S( -5,  -2),
            S( -7,  -6),   S(-18, -40),   S(-19,   4),   S( -5, -14),   S( 11,   0),   S( 36,  20),   S( -7,   6),   S( 14,  29),
            S(-18, -14),   S(-20,  -2),   S( -6,  13),   S( 10,   5),   S( 12,   2),   S(-17, -22),   S(  5,   7),   S( -1, -18),

            /* bishops: bucket 12 */
            S(  0,   3),   S( -8, -13),   S(-12, -29),   S( -6, -26),   S( -9, -18),   S(-11, -20),   S( -1,  10),   S( -5,   0),
            S( -7,  -7),   S(-13, -32),   S( -7, -13),   S( -6, -11),   S(-13, -21),   S( -2,  13),   S( -3,  -2),   S( -1,  -8),
            S( -1,  -3),   S(-15,  -1),   S(-12, -18),   S( -8,  -3),   S( -5,   9),   S( -5, -13),   S(-10, -42),   S( -3,  -5),
            S( -1,   4),   S(  4,   0),   S(-17, -29),   S( -4,  12),   S(  1,   6),   S(  5,  23),   S( -5,  -7),   S( -6,  -3),
            S( -1,  -3),   S(  2,  17),   S( -5,  20),   S( -9,   1),   S( -4,  -5),   S( -6,   2),   S(  3,   4),   S( -7,  -1),
            S(-13, -14),   S(  3,  59),   S(-28,   2),   S(-10,  -5),   S(  6, -15),   S( -5,   1),   S( -1,   5),   S( -1,  -5),
            S( -2,  -5),   S( -5,  14),   S(  5,  19),   S( -7,   6),   S( -1,   9),   S(  8,  17),   S( -7, -16),   S( -1,   5),
            S( -2,  -3),   S(  0,  -5),   S( -5,   2),   S(  6,   8),   S(  2,  10),   S(  0,   3),   S(-10,  -1),   S(  0,  -3),

            /* bishops: bucket 13 */
            S( -8, -42),   S(-13, -29),   S(-13, -17),   S(-15, -18),   S(-15, -19),   S( -8,   1),   S( -2,  -5),   S( -7,  -9),
            S( -4,  -6),   S(-11, -12),   S(-12, -28),   S(-18,  -9),   S(-13,   8),   S( -8,   2),   S( -1, -11),   S(  3,  -2),
            S( -9, -10),   S( -5,  -5),   S( -7,  10),   S(-21,  -2),   S(-12, -21),   S( -3, -11),   S( -3, -28),   S(  5,  21),
            S( -2,   4),   S(-13,  -4),   S(-14,   4),   S(-28,   8),   S( -1,  16),   S(  3,  -6),   S( -2,   3),   S( -7,  -6),
            S( -3,   9),   S(-17,   4),   S(-17,  -3),   S( 16,  -2),   S(-10,  -1),   S( -6,   6),   S(-12, -20),   S( -2,  -8),
            S( -3,  -6),   S( -9,   0),   S(-21, -12),   S( 11,  15),   S(  2,  10),   S( -4,  -7),   S(  6,  18),   S( -3,  -6),
            S( -6,  -9),   S( -8,  -2),   S(  7,  29),   S( -6,   8),   S( -7,   1),   S(  2,   0),   S(-15, -25),   S(  0,   7),
            S( -8, -16),   S( -1,   7),   S( -2,  -3),   S(  5,   1),   S(  0,   5),   S( -8,  -7),   S(  1,  10),   S( -2, -14),

            /* bishops: bucket 14 */
            S( -7, -16),   S(-12, -16),   S(-18, -28),   S(-17, -43),   S(-14, -36),   S( -5, -26),   S(-10, -14),   S(-10, -16),
            S(-10, -27),   S( -1, -21),   S( -7, -12),   S(-26, -42),   S(-10, -11),   S(-17, -10),   S(-14, -22),   S(  1, -13),
            S( -9, -12),   S( -8, -30),   S(-21, -28),   S(-14, -18),   S(-25,  -3),   S(-22, -31),   S( -6,   2),   S( -3,  -2),
            S( -7, -21),   S( -9,  -6),   S(-11,  -5),   S(-24,  18),   S( -2,   7),   S(-23,  10),   S(-19, -15),   S( -5, -11),
            S( -9,  -5),   S( -9,  25),   S( -7, -19),   S( -6, -22),   S(-13,   7),   S( -7,  -6),   S(  4,  20),   S(  2,  -6),
            S( -1,   4),   S(-10,   8),   S(-22, -13),   S( -8, -16),   S(  5,   6),   S(-10,  19),   S( -3,  32),   S( -7, -21),
            S( -6, -22),   S( -1,  -1),   S( -6,   2),   S(  5,  20),   S( -9,   0),   S( -1,   3),   S( -3, -12),   S( -4,  -6),
            S( -7,  -8),   S( -4,  -8),   S( -3,  -6),   S( -3,   6),   S(-10, -18),   S(  1,  10),   S(  6, -13),   S(  0,   4),

            /* bishops: bucket 15 */
            S(  4,   8),   S(  6,   4),   S(-19, -28),   S(  0, -11),   S( -9, -13),   S(-12, -23),   S( -6, -13),   S( -2, -10),
            S(  3,   6),   S( -1,  -7),   S(  3,  -2),   S( -8, -11),   S(-14, -21),   S( -6,  -7),   S( -8, -17),   S( -1,   0),
            S( -7, -13),   S(  0,  -1),   S(-12,  -9),   S(-10,  -6),   S(-19, -18),   S(-16, -20),   S( -7,  -9),   S(  2,  17),
            S( -4,  -7),   S(-17, -17),   S(  6, -13),   S(-23, -30),   S( -5,   5),   S( -8, -13),   S(  3,  15),   S( -1,  -8),
            S( -1,  -9),   S(-12, -18),   S(-14, -10),   S(-21, -47),   S( -3, -24),   S(-15,  19),   S(  2,  19),   S(-10, -16),
            S( -9, -32),   S(-12, -12),   S(-18, -34),   S(-21, -12),   S( -5,  -2),   S(-11, -28),   S(  7,  38),   S(  1,  11),
            S( -3,   2),   S( -1, -16),   S( -2, -13),   S( -4,   4),   S(-10, -18),   S(  0,  12),   S(-11,   1),   S(  4,   6),
            S( -3,  -2),   S( -1,   2),   S( -4,   1),   S( -6,  -3),   S( -8,  -5),   S(-16, -20),   S( -9, -23),   S(  1,   1),

            /* rooks: bucket 0 */
            S(-22,   7),   S( -7,  -2),   S(-17, -11),   S(-10,  -7),   S(-14,  10),   S( -8, -10),   S(-14,  21),   S( -1,  16),
            S( 11, -60),   S( 27, -16),   S(  7,  -3),   S(  0,   3),   S( 16,   0),   S( -2,  -4),   S(-33,  21),   S(-44,  33),
            S(  1, -24),   S( 14,  29),   S( 20,  10),   S(  7,  13),   S(-16,  42),   S( -4,  11),   S(-30,  18),   S(-40,  15),
            S( 26, -22),   S( 59,   0),   S( 39,  30),   S( 36,   7),   S( 11,  10),   S( -5,  15),   S(-15,  21),   S(-40,  35),
            S( 57, -24),   S( 86, -15),   S( 62,  -1),   S( 33,  -7),   S( 43,   7),   S( 22,  10),   S(-13,  38),   S(-24,  36),
            S( 61, -41),   S(100, -33),   S( 47,   8),   S( 11,  22),   S( 40,  10),   S(-45,  35),   S( 24,  22),   S(-43,  44),
            S( 38, -11),   S( 64,  -3),   S( 19,   8),   S(  3,  29),   S(-15,  31),   S( -8,  16),   S(-21,  35),   S(-17,  26),
            S( 28,  17),   S(  9,  46),   S( 14,  26),   S( -8,  39),   S(  0,  20),   S(  5,   1),   S( -3,  28),   S(  7,  23),

            /* rooks: bucket 1 */
            S(-74,  34),   S(-52,   4),   S(-56,  -7),   S(-44, -14),   S(-31, -21),   S(-28, -22),   S(-34, -10),   S(-36,  19),
            S(-28,   5),   S(-51,  16),   S(-13, -13),   S(-22, -30),   S(-27, -11),   S(-37, -12),   S(-40, -16),   S(-56,  16),
            S(  4,   9),   S(-23,  33),   S(-17,  14),   S(-41,  25),   S(-46,  32),   S( -6,   4),   S(-25,   9),   S(-47,  23),
            S(-53,  54),   S(-38,  33),   S(  4,  18),   S(-18,  21),   S(-31,  34),   S(-45,  44),   S(-38,  41),   S(-34,  16),
            S( 47,  16),   S( 22,  37),   S( 23,   3),   S(-42,  42),   S(-27,  42),   S( 12,  22),   S( -5,  20),   S(-43,  26),
            S( 41,  12),   S(  5,  30),   S(  2,  26),   S(-39,  31),   S(  3,  14),   S(-32,  44),   S(-19,  28),   S(-50,  35),
            S(-17,  31),   S(  2,  30),   S( 15,  29),   S(-51,  51),   S(-29,  35),   S( -1,  34),   S(-40,  30),   S(-59,  36),
            S( 30,  28),   S( 32,  35),   S( -4,  29),   S(-48,  54),   S( -6,  17),   S( 23,  13),   S(-14,  35),   S( -2,  12),

            /* rooks: bucket 2 */
            S(-63,  36),   S(-46,  19),   S(-46,  13),   S(-57,  13),   S(-59,  10),   S(-43,   2),   S(-31, -23),   S(-48,  28),
            S(-72,  45),   S(-58,  37),   S(-43,  27),   S(-52,  11),   S(-40,  -2),   S(-50,   1),   S(-60,  19),   S(-57,  20),
            S(-71,  65),   S(-57,  54),   S(-53,  55),   S(-32,  14),   S(-43,  28),   S(-19,  24),   S(-15,  16),   S(-33,  25),
            S(-72,  65),   S(-57,  67),   S(-41,  63),   S(-40,  51),   S(-32,  36),   S(  3,  34),   S(-38,  54),   S(-20,  35),
            S(-24,  52),   S(-47,  67),   S(-46,  59),   S(-25,  43),   S( 17,  30),   S( 16,  31),   S(-29,  53),   S(-41,  48),
            S(-36,  46),   S(-32,  48),   S(-21,  33),   S( -8,  25),   S( 12,  31),   S( 43,  18),   S( 21,  22),   S(-19,  30),
            S(-53,  43),   S(-67,  71),   S(-35,  56),   S(-17,  52),   S(  4,  32),   S( 16,  24),   S(-51,  60),   S(-38,  48),
            S(-28,  63),   S( -6,  45),   S(-70,  66),   S(-35,  47),   S(-56,  60),   S(-34,  63),   S(-52,  74),   S(-23,  46),

            /* rooks: bucket 3 */
            S( -5,  74),   S( -6,  66),   S( -4,  59),   S(  3,  49),   S(  0,  44),   S(-18,  67),   S( -9,  74),   S( -4,  38),
            S(-30,  88),   S(-12,  69),   S(  3,  63),   S(  9,  58),   S( 18,  49),   S( 14,  55),   S( 41,   1),   S( 21, -37),
            S(-33,  83),   S(-15,  85),   S( -1,  77),   S( 12,  61),   S(  8,  79),   S( 26,  71),   S( 32,  70),   S(  4,  52),
            S(-27,  91),   S(-18,  84),   S( 17,  73),   S( 22,  69),   S( 17,  73),   S( -5, 111),   S( 58,  63),   S( 19,  69),
            S(-15,  98),   S( 22,  78),   S( 14,  70),   S( 35,  69),   S( 37,  68),   S( 43,  67),   S( 86,  53),   S( 53,  45),
            S(-14,  91),   S( 10,  76),   S(  7,  72),   S( 14,  71),   S( 22,  55),   S( 43,  52),   S( 80,  36),   S( 88,  15),
            S(-37, 100),   S(-19, 100),   S(-11,  94),   S( 19,  81),   S( 10,  76),   S( 20,  74),   S( 49,  68),   S( 92,  31),
            S(-72, 147),   S( -6, 101),   S(  6,  78),   S( 37,  66),   S( 42,  58),   S( 44,  69),   S(104,  52),   S( 85,  50),

            /* rooks: bucket 4 */
            S(-87,  26),   S( -8,  -2),   S(-41,   4),   S(-27,  18),   S(-28, -18),   S(  9, -48),   S( -2, -20),   S( -7, -35),
            S(-32,  -2),   S(-41,   5),   S(-41,  14),   S(-40,  24),   S( -5,  -8),   S(-16, -21),   S(  5, -33),   S(-14, -24),
            S(  0,   9),   S(-27, -19),   S(-16,  10),   S( -9, -11),   S(  2,  -4),   S(  1,  -5),   S( 35, -14),   S(-37,   0),
            S(-31, -13),   S(  3,   5),   S(-27,  18),   S( 28,   1),   S( 20,   6),   S( 16,   1),   S( 18,  10),   S( -5,  14),
            S(-19, -11),   S( -6,  30),   S(-13,  23),   S( 67,   8),   S( 21,  23),   S( -1,  18),   S( 38,  29),   S( 31,   1),
            S( 20,   8),   S( 23,  11),   S( 49,  15),   S( 37,  13),   S( 34,  16),   S(  4,  35),   S(  8,  27),   S( 21,  31),
            S( -2,  -7),   S( 30,  28),   S( 26,  29),   S( 36,  22),   S( 53,  11),   S( 12,   1),   S( 32,  18),   S( 27,  22),
            S( 33, -57),   S( 35,  44),   S( 14,  26),   S( 11,  18),   S( 15,   5),   S( 11,  25),   S( 13,   5),   S( 16,  18),

            /* rooks: bucket 5 */
            S(-34,  30),   S(-49,  50),   S(-60,  47),   S(-53,  33),   S(-42,  23),   S(-36,  40),   S( -4,  26),   S(-32,  42),
            S(-27,  31),   S(-31,  29),   S(-78,  66),   S(-51,  38),   S(-40,  25),   S(-13,  17),   S(  9,  16),   S(-21,  20),
            S( 12,  40),   S(-39,  56),   S(-53,  56),   S(-60,  57),   S(-34,  31),   S( -5,  32),   S( -3,  42),   S( -1,  41),
            S(-23,  69),   S( -5,  47),   S(-26,  66),   S(-15,  43),   S(-19,  58),   S(  8,  61),   S( -3,  55),   S(  8,  36),
            S( 16,  59),   S(  0,  66),   S( 36,  46),   S( 28,  59),   S( 29,  56),   S( 14,  75),   S( 62,  61),   S( 28,  42),
            S( 62,  54),   S( 26,  65),   S( 50,  52),   S( 22,  70),   S( 54,  50),   S( 48,  58),   S( 53,  47),   S( 46,  41),
            S( 46,  40),   S( 22,  64),   S( 42,  53),   S( 58,  43),   S( 33,  51),   S( 42,  56),   S( 65,  49),   S( 69,  43),
            S( 93,  28),   S( 69,  32),   S( 32,  55),   S( 17,  37),   S( 46,  47),   S( 47,  48),   S( 43,  42),   S( 24,  47),

            /* rooks: bucket 6 */
            S(-40,  21),   S(-41,  35),   S(-26,  27),   S(-39,  26),   S(-58,  33),   S(-74,  60),   S(-48,  54),   S(-34,  52),
            S(-21,  27),   S(-15,  28),   S(-14,  29),   S(-45,  28),   S(-53,  47),   S(-64,  60),   S(-63,  57),   S( 25,  13),
            S(-17,  52),   S(-12,  35),   S(  3,  35),   S(-38,  41),   S( -7,  30),   S(-39,  62),   S(-30,  74),   S( 17,  37),
            S(-26,  67),   S( 33,  43),   S(  1,  58),   S(  6,  42),   S( -1,  46),   S( -2,  56),   S(-43,  64),   S(-12,  57),
            S( 10,  70),   S( 43,  57),   S( 56,  46),   S( 29,  46),   S( 13,  64),   S( 36,  52),   S( 35,  49),   S( 14,  57),
            S( 15,  61),   S( 59,  52),   S( 82,  33),   S( 35,  36),   S( 24,  48),   S( 42,  57),   S( 49,  49),   S( 65,  47),
            S( 39,  59),   S( 75,  42),   S( 79,  34),   S( 88,  20),   S( 92,  27),   S( 45,  53),   S( 49,  51),   S( 49,  47),
            S( 60,  70),   S( 26,  66),   S( 31,  52),   S( 37,  43),   S( 58,  49),   S( 51,  64),   S( 55,  62),   S( 22,  65),

            /* rooks: bucket 7 */
            S(-62, -16),   S(-39, -12),   S(-36, -20),   S(-26, -10),   S(-24,  -6),   S(-59,  34),   S(-46,  19),   S( -8, -17),
            S(-55,  15),   S(-32,   1),   S(-37,   1),   S( -6, -13),   S(-20,  11),   S(-10,  13),   S(-16,  -2),   S(-55,   8),
            S(-74,  45),   S(-30,  14),   S( -2,   4),   S(  0, -12),   S( -5,   5),   S(-27,  -4),   S(-26, -12),   S(  8,   6),
            S(-55,  39),   S(  1,  18),   S(  9,  13),   S( 19,  12),   S( 28,   0),   S( 26,   9),   S( 33,   1),   S(-11,   7),
            S(-23,  39),   S( 15,  11),   S( 51, -12),   S( 56,  -6),   S( 64,   1),   S( 90,   7),   S( 71,   5),   S( 39, -11),
            S(-14,  31),   S( 14,  14),   S( 79, -24),   S( 98, -23),   S( 68,  -4),   S( 64,  20),   S( 63,  20),   S( 20,   3),
            S(-14,  34),   S( 15,  19),   S( 42,   5),   S( 60,   3),   S( 86,  -6),   S( 83,  -5),   S( 35,  26),   S(  8,   8),
            S(  5,  60),   S(-26,  44),   S( 31,   4),   S( 77, -22),   S( 18,   8),   S( 10,  18),   S( 40,   7),   S( 52,  -6),

            /* rooks: bucket 8 */
            S(-41, -47),   S( -9, -11),   S(  4,   3),   S(  4, -14),   S( -9, -42),   S( -9, -55),   S(-15, -25),   S( -3, -19),
            S( -2, -19),   S( -4,  -6),   S(  1, -12),   S( 11, -14),   S( -5, -25),   S( -9, -21),   S( -5, -43),   S(-16, -65),
            S(  6,  15),   S(  9, -15),   S(  6,   6),   S( 12,  10),   S(-13, -32),   S( -1, -32),   S( 14,  22),   S(  0,  -2),
            S( -7, -19),   S( -3,  25),   S( -2,   7),   S( 22,   2),   S(  5,  15),   S( -4, -12),   S( 10, -16),   S(  2,   1),
            S( -8, -11),   S(  1,  17),   S( -1,  29),   S( 16,   9),   S(  4,   7),   S( 22,   8),   S( 11,  -9),   S( 13, -32),
            S(  7,  26),   S( -7,   7),   S( 32,  41),   S( 28,  -8),   S( -1,  -5),   S(  6, -14),   S(  3,   0),   S( 12,  42),
            S(  0, -12),   S( 10, -16),   S( 23,   3),   S( 18, -18),   S( 28,   7),   S( 20, -19),   S( 18, -15),   S( 16,  -7),
            S(  2, -146),  S(  8, -15),   S( 21,   8),   S( -1,  -8),   S(  2,   2),   S(  3, -12),   S(  7,  -8),   S( 22,  -2),

            /* rooks: bucket 9 */
            S(-39, -16),   S( -5, -24),   S(-18, -29),   S(-32,  -2),   S(-17,   5),   S( -2,  -3),   S( 16, -45),   S(-33, -31),
            S( 36, -21),   S(  4, -18),   S(-12, -19),   S(-16,  -4),   S(-14, -11),   S( 21,   7),   S(  6, -28),   S( -9, -29),
            S( 13, -18),   S( 20,  -6),   S(  4,   5),   S( -6,  -1),   S( -7, -20),   S( 31,  -6),   S( 15,  19),   S( -1,   0),
            S(  4,   9),   S(  9,   5),   S( 13,  20),   S( -2,   6),   S(  5,  20),   S( 24,  -3),   S( 16,  37),   S( 13,   2),
            S( 14,   5),   S(  4,  10),   S(  3,  28),   S( 14,  24),   S( 35,  32),   S( 25,  30),   S( 12,  -1),   S( 14,  -7),
            S( 14,  42),   S( -8,  10),   S( 10,   1),   S(-14,   2),   S( 11,   8),   S( 29,   7),   S(  7,  35),   S( 15,  13),
            S( 64,  17),   S( 59,   5),   S( 28,  26),   S( 53,  11),   S( 30,  -8),   S( 29,   7),   S( 38,   0),   S( 43,  25),
            S( 66, -70),   S( 39, -30),   S( 18,  25),   S( 31,  27),   S( 11,  36),   S( 26,  16),   S( 22,  12),   S( 31,   9),

            /* rooks: bucket 10 */
            S(-49, -80),   S(-13, -51),   S(-38, -29),   S(-29,  -5),   S(-30,  -5),   S(-27, -12),   S( 14, -17),   S(-28, -21),
            S(  1, -19),   S( 14, -27),   S(  3, -26),   S(  1, -15),   S(  6, -18),   S( -5,  -4),   S( 35,   5),   S( 12,  -2),
            S(-10, -19),   S( -9, -23),   S(  5, -17),   S( 25,  -4),   S(-16,  18),   S(  0,  -8),   S( 27,  26),   S( 10,  -6),
            S(  6,   0),   S(  8, -14),   S(  3,  -6),   S(  7,  16),   S( 26,  -3),   S(  2,  -5),   S( 25,  26),   S(  1,  -8),
            S(  9,  12),   S( 32,   7),   S( 15,   9),   S( 18, -20),   S( -5,  -3),   S( 13,  10),   S( 30,  30),   S(  8,  27),
            S( 36,  30),   S( 30,  41),   S( 21,  10),   S( 17,   9),   S( -1,  -8),   S( 15,   9),   S( 32,  20),   S(  8,  36),
            S( 71,  12),   S( 81,   1),   S( 76,  -4),   S( 67, -16),   S( 52, -16),   S( 34,  13),   S( 23,   7),   S( 28,   6),
            S( 60,  15),   S(  8,  -3),   S( 38,  -1),   S( 21,   7),   S( 34,  -3),   S( 28,  10),   S( 13,   2),   S( 20, -11),

            /* rooks: bucket 11 */
            S(-34, -49),   S(-25, -27),   S(-15, -28),   S(-25, -55),   S(  4, -22),   S( -5,   2),   S(-23, -32),   S(-51, -18),
            S(-12, -29),   S( -5, -43),   S(  0, -28),   S(  2, -27),   S( -2, -23),   S(-16, -17),   S( -2, -32),   S(-20,   1),
            S(  5, -31),   S( 16, -14),   S( 23, -15),   S( 13, -19),   S( 13, -10),   S(-10,  10),   S(-21, -24),   S(-11, -54),
            S(  1,  27),   S( -1, -11),   S(  0,  13),   S( 15,   8),   S(  4,  -3),   S( 12,  31),   S( 28, -11),   S(  2, -25),
            S( 12,  11),   S( 21, -10),   S( 30,   1),   S( 21,  -7),   S( 26,  -4),   S( 29,  -6),   S( 10,  11),   S( -1, -12),
            S( 26,  34),   S( 44,   8),   S( 27,  -9),   S( 50,  20),   S( 51,  22),   S( 39,  11),   S( -3,   5),   S( 15,  25),
            S( 61,  34),   S( 59,   3),   S( 68, -12),   S( 75, -15),   S( 45,  -9),   S( 47,  12),   S( 32,  34),   S( 52,  -4),
            S( 45,  32),   S( 14,  27),   S( 22,   7),   S( 11,  -7),   S( -7,  -2),   S( 17,  18),   S( 14,   9),   S( 32,   6),

            /* rooks: bucket 12 */
            S( -2, -10),   S( -8, -31),   S(-12, -53),   S( -3,  -9),   S(  1,  -3),   S( -4, -34),   S(-22, -66),   S(-24, -55),
            S(  7,   4),   S( -6, -23),   S(-12, -19),   S( -7, -18),   S( -9,  -5),   S( -8, -15),   S(  1,  -2),   S(-11, -33),
            S(  3,  -1),   S( -6, -19),   S( -8, -24),   S(-12,  -7),   S( -5, -21),   S(  6,  -8),   S( -7, -10),   S(  5,  -9),
            S( -7,  -8),   S(  0, -10),   S(  2,  12),   S(  9, -10),   S(  1,  -8),   S( -9, -37),   S( -7, -12),   S( -4, -38),
            S( -3, -11),   S( -2, -19),   S( 12,   4),   S(  9,   8),   S( -8, -35),   S(  7, -17),   S( -5,  -7),   S(  1, -17),
            S( -4,  -9),   S( -5,  -9),   S( 20,  33),   S(  8,  -5),   S( -4,  -5),   S( -7, -20),   S(  0, -24),   S(  5,   9),
            S( -5,  -6),   S(  0, -27),   S(  1, -40),   S( 11,   0),   S(  7,  -2),   S( -5, -38),   S( -3,  -8),   S(  9, -18),
            S( -5, -43),   S(  7,  23),   S(  3, -21),   S(  1,   2),   S( -4, -23),   S(-12, -49),   S(-15, -30),   S(  7,  -6),

            /* rooks: bucket 13 */
            S(-12, -42),   S( -6, -26),   S( -3, -18),   S(  2,  11),   S(  7,  -4),   S(-11, -37),   S(  2, -22),   S(-18, -32),
            S( -2, -31),   S( -3, -15),   S(-11,  -8),   S( -6,  -1),   S(-10, -20),   S( -1, -11),   S(  6,   2),   S( -5, -22),
            S( -5, -29),   S( -7, -28),   S( -4, -35),   S( -2, -22),   S( 11,  13),   S(  2,  -4),   S(  1, -20),   S(  1, -34),
            S( -6, -51),   S(  3,  -3),   S( -8, -42),   S( -4,  -9),   S( 14,  14),   S( -6, -34),   S( -2, -27),   S(  3, -16),
            S( 11, -21),   S(  9, -18),   S( 17,  26),   S( -5,  -8),   S( -9, -27),   S(  4, -13),   S( -6, -38),   S(  9,  -9),
            S( -7, -40),   S( 10, -27),   S(-10, -12),   S( 13,  -7),   S(  5, -12),   S( 10,  17),   S(  8,  -2),   S(  5,   8),
            S(  5,  -8),   S(  9,  15),   S(  8,   7),   S(  1, -18),   S( 10, -27),   S( 20,   6),   S(  3, -12),   S(  3, -17),
            S(-15, -120),  S(-18, -70),   S(  5,   6),   S(  1,  -1),   S( -4,  14),   S( -4, -31),   S(-10, -27),   S(  4,   1),

            /* rooks: bucket 14 */
            S( -6, -34),   S(-16, -48),   S( -2,  -8),   S( -1, -34),   S(  3, -23),   S(-10, -24),   S( 10,  -7),   S( -6, -22),
            S(-22, -45),   S(-13, -55),   S( -8,   5),   S(-12, -39),   S(-10, -16),   S(  1, -31),   S(  7,  24),   S(  6, -11),
            S( -3, -23),   S( -9, -20),   S( -3, -18),   S( -6, -13),   S(-13, -25),   S( -8, -22),   S(  6,  21),   S( -1, -27),
            S( 11,   5),   S( -8, -32),   S( -3, -19),   S( -5,   8),   S(  3, -11),   S(  4, -13),   S( -5, -34),   S( -3, -22),
            S(  1, -11),   S(  3, -26),   S( -7, -28),   S( -9, -22),   S( -6, -16),   S( -5, -18),   S(  2,   8),   S(  8,   2),
            S(  2, -14),   S(  0, -24),   S(  1, -15),   S(  2, -17),   S(-12, -18),   S( -9,   7),   S(  5,  12),   S(  0,  -4),
            S( 18,  -1),   S(  0, -37),   S(  3, -23),   S(  2, -30),   S(  5, -43),   S(  6,   2),   S(  8,  10),   S(  8,   6),
            S( -3, -22),   S(  3, -16),   S( -9, -29),   S( 10,  12),   S(-11, -20),   S(  2,   8),   S(  3,  15),   S( -2, -16),

            /* rooks: bucket 15 */
            S( -2, -55),   S(-13, -43),   S( -1, -27),   S( -6, -28),   S(  1, -16),   S( -3,  -9),   S(-17, -54),   S(-10, -15),
            S(-14, -21),   S(-13, -29),   S(  2,  -1),   S( -7, -24),   S(-10, -28),   S(  6, -28),   S(-12, -42),   S(  6,   4),
            S( -8, -24),   S(-10, -23),   S( -3, -24),   S(  2,   0),   S( 10, -27),   S( -4, -10),   S( -3,   6),   S( -4, -14),
            S(  3, -31),   S( -3, -27),   S(-11, -17),   S( -5, -17),   S(-10, -18),   S(  3, -18),   S(  0, -18),   S( -9,  -2),
            S(  0, -11),   S( -4, -11),   S( 11,  -8),   S(  0, -11),   S(  1,   0),   S(  3,   0),   S( -1,   8),   S(  0,  17),
            S(  7,  18),   S(  1,   1),   S(  0, -14),   S(  0, -10),   S( -6,  -9),   S(  1,  16),   S(  4,  -8),   S( -8, -14),
            S( 10,  19),   S( 11,  -5),   S(  8, -32),   S( -4, -32),   S(  0, -20),   S( 11,  34),   S(  2,  -2),   S(  0,  11),
            S(  0, -18),   S( -7, -18),   S(  2,  -6),   S(  1, -11),   S( -7, -14),   S( -1, -25),   S(  1, -16),   S(  1,  -5),

            /* queens: bucket 0 */
            S(-21, -13),   S(-20, -55),   S( 50, -87),   S( 59, -56),   S( 32, -35),   S( 19,  -1),   S( 55,   8),   S( 22,  19),
            S(-10, -14),   S( 34, -62),   S( 41, -16),   S( 23,  10),   S( 26,  32),   S( 26,  21),   S(  8,  64),   S( 36,  22),
            S( 29,   3),   S( 44,  15),   S( 23,  27),   S( 20,  36),   S( 19,  19),   S( 12,  17),   S(  9,  29),   S( 36,  32),
            S( 21,  19),   S( 27,  45),   S(  6,  48),   S(  5,  50),   S(  7,  58),   S( 13,  35),   S( 14,  28),   S( 18,  30),
            S( 41,  50),   S( 31,  42),   S( 20,  41),   S( 17,  58),   S( -8,  31),   S( -7,  12),   S( 32,  23),   S( 44,  -5),
            S( 26,  59),   S( 24,  54),   S( 10,  39),   S( 17,  15),   S( 41,  -9),   S(  1,  36),   S( 23,  21),   S( 23, -23),
            S( 45,  49),   S( 50,  42),   S( 29,  37),   S( 46,  25),   S( 19,   6),   S(-12,  -9),   S( 27,  23),   S( 29,  10),
            S( 42,  29),   S( 20,  36),   S( 38,  16),   S( 34,  34),   S( 42,  29),   S(-16,   1),   S( 47,  27),   S( 43,  25),

            /* queens: bucket 1 */
            S(  1, -17),   S(-74, -24),   S(-51, -29),   S(-13, -69),   S(-10, -25),   S(-18, -46),   S( 15, -30),   S( 12,  26),
            S(-15, -26),   S(-10, -43),   S( 10, -46),   S( -3,  -1),   S( -6,   1),   S(  6,  -1),   S( 21, -40),   S(  0,  21),
            S(-26,  43),   S(  3,  -3),   S(  5,  12),   S( -4,   5),   S( -4,  33),   S(-14,  31),   S( 15,  12),   S( 19,  21),
            S(  9, -18),   S(-11,  32),   S(-14,  34),   S(  2,  46),   S( -9,  51),   S(  1,  30),   S(  0,  -1),   S( 17,  19),
            S( 15,  12),   S(  7,  27),   S( -1,  62),   S(-25,  66),   S(-17,  53),   S(  0,  15),   S( -8,  17),   S(  3,  36),
            S( 10,  26),   S( 14,  53),   S( 14,  61),   S(-41,  59),   S(-21,  48),   S(-35,  47),   S( 19,  27),   S( 16,  39),
            S(  2,  38),   S(-14,  71),   S(-21,  34),   S(-25,  70),   S(-30,  48),   S( 10,  29),   S( -9,  40),   S(-27,  44),
            S( -5,   8),   S(  6,  17),   S( 14,  26),   S(-10,  13),   S( -3,  15),   S(  6,  16),   S( 12,  26),   S( -7,  31),

            /* queens: bucket 2 */
            S( 10,  18),   S( 14, -33),   S(  9, -19),   S( -2, -15),   S(-17,  -3),   S(-22, -18),   S(-25, -22),   S( 16,   9),
            S( 20,  13),   S( 12,  40),   S( 18, -11),   S( 20, -21),   S( 16, -29),   S( 16, -46),   S( 11,  -4),   S( 34, -26),
            S( 20,  11),   S( 18,  15),   S(  3,  47),   S(  9,  36),   S(  4,  57),   S( 16,  49),   S( 12,  22),   S( 29,  18),
            S(  7,  29),   S(  1,  56),   S( -2,  45),   S(  3,  56),   S(-21,  81),   S( -2,  84),   S( 12,  19),   S(  4,  71),
            S( 17,   8),   S( -7,  60),   S( -8,  59),   S(-31,  97),   S(-35, 109),   S(-14,  79),   S( -6, 103),   S( -5, 106),
            S( 13,  23),   S(  0,  45),   S(-33,  84),   S( -7,  53),   S(-28,  91),   S(-14,  99),   S( -7,  98),   S( 10,  73),
            S(-23,  55),   S(-37,  81),   S(-16,  66),   S(  6,  64),   S(-20,  75),   S( 24,  44),   S(-16,  46),   S(-16,  81),
            S(-68,  78),   S(  0,  39),   S( 30,  40),   S( 28,  35),   S(  1,  65),   S( 16,  35),   S( 12,  28),   S(-12,  40),

            /* queens: bucket 3 */
            S( 84,  89),   S( 59,  92),   S( 50,  99),   S( 45,  81),   S( 68,  32),   S( 47,  20),   S( 21,  19),   S( 43,  55),
            S( 68, 114),   S( 61, 110),   S( 46, 114),   S( 50,  89),   S( 50,  80),   S( 64,  46),   S( 67,   7),   S( 42,  42),
            S( 65,  86),   S( 55, 106),   S( 55,  83),   S( 55,  78),   S( 52,  91),   S( 56,  99),   S( 65, 101),   S( 70,  70),
            S( 49, 122),   S( 61,  85),   S( 48,  95),   S( 39,  98),   S( 40,  96),   S( 38, 130),   S( 60, 100),   S( 55, 129),
            S( 65,  90),   S( 59, 105),   S( 54,  88),   S( 37,  97),   S( 32, 116),   S( 28, 124),   S( 41, 162),   S( 56, 151),
            S( 49, 119),   S( 56,  98),   S( 49,  94),   S( 25, 116),   S( 29, 134),   S( 67, 105),   S( 64, 136),   S( 38, 180),
            S( 60, 114),   S( 59, 102),   S( 68,  85),   S( 57,  96),   S( 29, 112),   S( 55, 112),   S( 87, 126),   S(154,  69),
            S( 74,  89),   S( 96,  78),   S( 68,  89),   S( 70,  85),   S( 30, 110),   S( 99,  57),   S(128,  59),   S(134,  58),

            /* queens: bucket 4 */
            S(-13, -23),   S(-17, -18),   S(-25,  -9),   S( -4,  -7),   S( 13, -13),   S( 36,   1),   S(-30,  -8),   S(-24,  -2),
            S(-32, -20),   S(-30,  -6),   S( 13,  -7),   S(-40,  24),   S(  5,  -5),   S(  1, -12),   S( -5,  -9),   S(-31, -14),
            S(  1,   0),   S( 11,  -1),   S( -2,  28),   S( -2,  33),   S( 25,  16),   S(  6,  -6),   S(  9, -18),   S(-24, -23),
            S(-16,   2),   S( -6,  14),   S(  3,  36),   S( -6,  29),   S( 15,  34),   S( 22,  20),   S(  3, -13),   S( -2,  -5),
            S( -9,  -3),   S( 15,  11),   S( 15,  27),   S( 27,  42),   S( 22,  28),   S( 21,   3),   S(-17, -13),   S( -7, -28),
            S(  2,  12),   S( 35,  12),   S( 25,  54),   S( 23,  45),   S( 12,   9),   S(  4,   6),   S(-15, -12),   S( -9,  -5),
            S(-12, -20),   S( -6,  17),   S(  2,  25),   S( 30,  34),   S(  9,  11),   S(-12,  -2),   S(-19, -40),   S(-19, -26),
            S( -4, -17),   S( -3,  -5),   S( 28,  37),   S(  4,  20),   S(-18, -17),   S( -6, -10),   S(-20, -33),   S( -7, -17),

            /* queens: bucket 5 */
            S(-35, -13),   S(-23, -29),   S(-27, -27),   S(-42, -27),   S(-52, -28),   S( 10, -14),   S( -5,  -4),   S(  1,  -5),
            S(-27,  -4),   S(-40, -15),   S(-65, -22),   S(-64,  -4),   S(-14,  -5),   S(-41, -15),   S(-43, -15),   S(-47, -12),
            S(-34,   3),   S(-62, -14),   S(-65,   1),   S(-37,  27),   S( 15,  50),   S( -5,  25),   S( -1,   0),   S( 16,  23),
            S(-51, -12),   S(-50,  -6),   S( -1,  36),   S( -4,  50),   S( 13,  25),   S( -2,  14),   S(  2,  -5),   S( -3,  17),
            S(-30,  -4),   S(-25,  17),   S(-11,  46),   S( -5,  44),   S( 28,  48),   S(  1,  19),   S(  2,  13),   S(-25, -26),
            S(-14,  17),   S( 10,  36),   S(-12,  42),   S(  2,  44),   S( 40,  50),   S(  5,  14),   S(  4,   4),   S( -9,  -8),
            S( -5,   9),   S( -8,  14),   S(  8,  60),   S( -1,  34),   S(  1,  39),   S( 25,  37),   S( 13,  11),   S(-17, -13),
            S( 10,  26),   S( 13,  13),   S(  3,  20),   S( 12,  50),   S( 17,  32),   S(  5,  23),   S(  1, -22),   S(-17, -13),

            /* queens: bucket 6 */
            S(-21,   9),   S(-46, -21),   S(-62, -25),   S(-73, -59),   S(-87, -50),   S(-70, -43),   S(-48, -42),   S(-25,   2),
            S(-59, -10),   S(-37,   1),   S(-48,  12),   S(-61,   9),   S(-76,  15),   S(-83,  -1),   S(-84, -19),   S(  7,  18),
            S(-37,  13),   S(-13,  13),   S(-48,  39),   S(-95,  85),   S(-42,  49),   S(-40,   0),   S(-48, -14),   S(  1,   5),
            S(-34,  14),   S(-21,  13),   S(-23,  64),   S(-45,  66),   S(  6,  44),   S( 15,  49),   S(-11,  35),   S( 10,  -7),
            S(-47,  24),   S(  0,  40),   S(-24,  54),   S( 11,  30),   S( 30,  54),   S( 62,  36),   S( 23,  32),   S( -4,  20),
            S(-18,  45),   S( -7,  21),   S( 27,  23),   S( 22,  48),   S(  9,  52),   S( 61,  68),   S( -8,  -7),   S(-12,  13),
            S( -4,   8),   S(  7,   6),   S( -6,  45),   S( -8,  37),   S( 30,  53),   S( 21,  64),   S( -7,  24),   S(-35,   1),
            S(  1,   8),   S( 20,  14),   S( 14,  34),   S(  0,  26),   S( 31,  41),   S( 21,  30),   S(  0,  19),   S(  7,  12),

            /* queens: bucket 7 */
            S( -1,  -5),   S(-28,  15),   S(-44,  25),   S(-27,  12),   S(-25,  -9),   S(-29, -24),   S(-29,  -7),   S(-18, -12),
            S(-26,  -8),   S(-40,   4),   S(-17,   5),   S(-17,  36),   S(-26,  32),   S(-40,  37),   S(-41,  22),   S(-37, -14),
            S(-28, -19),   S(-38,  31),   S(-13,  33),   S( -5,  28),   S( 10,  18),   S(  2,  25),   S(-15,  12),   S(-24,  -4),
            S(-53,   0),   S( 14,   4),   S(-12,  24),   S(  0,  38),   S( 34,  19),   S( 31,  25),   S(  8,  37),   S( -4,  18),
            S(-22,  21),   S(-47,  28),   S( 13,  19),   S( 53,  -8),   S( 61, -11),   S( 81, -16),   S( 34,  11),   S( 34,  -7),
            S(-12,  14),   S(-12,  10),   S(  9,  -1),   S( 16,  -9),   S( 36,  36),   S( 76,  21),   S( 64,   2),   S( 36,  10),
            S( 12, -17),   S(  7,  11),   S(  4,  -5),   S(  5,  14),   S( 35,  18),   S( 50,  37),   S( 51,  19),   S( 47,  24),
            S( 16,   4),   S( 18,   4),   S( 19,   9),   S( 17,  16),   S( 37,  26),   S( 20,  20),   S( 13,   5),   S( 35,  43),

            /* queens: bucket 8 */
            S( -5, -10),   S( -1,   4),   S(-13,  -4),   S( -8,  -6),   S( -4,   1),   S( -2, -15),   S(-19, -23),   S( -3,   5),
            S( -6,   0),   S(-11, -15),   S( -3,   5),   S(-11,  -1),   S( -3,  -2),   S(-16, -19),   S(-18, -38),   S( -3,  -8),
            S( -2,  -1),   S( -5,   2),   S( -7,   2),   S( -4,  -9),   S( -2,   6),   S( -9, -10),   S(-11, -25),   S(-14, -26),
            S( -4,   1),   S(  9,  17),   S( 11,  19),   S(  5,  11),   S( -2,   1),   S( -5,   0),   S(  0,  -1),   S( -6, -20),
            S( 15,  27),   S(  2,  26),   S( 10,  13),   S(  9,  15),   S( 12,  33),   S(  5,   1),   S( -7,  -7),   S(-10, -19),
            S(  7,  18),   S( 10,  20),   S(-20,  13),   S( 14,  33),   S( -9, -15),   S( -5, -11),   S(  4,   3),   S(  4,  13),
            S( -7, -13),   S(-18, -27),   S( 21,  33),   S( 13,  15),   S(  2,  17),   S(  2,  18),   S( -3,  -7),   S( -6, -13),
            S(-15, -29),   S( 14,  10),   S(-16, -48),   S(-10,  -6),   S(-12, -29),   S( -2,  -5),   S( -3, -17),   S( -4,  -6),

            /* queens: bucket 9 */
            S(  6,   8),   S(-12, -26),   S(  2,  -1),   S(-28, -32),   S(-22, -37),   S(-16, -29),   S(-12, -21),   S(-12, -17),
            S( -2,  -6),   S( -8,  -6),   S(-17, -22),   S( -2,   1),   S(-15,  -7),   S(-15, -19),   S(  2,  -1),   S( -3,  -6),
            S(  5,   6),   S(  4,   8),   S( -6,  22),   S( -3,  -5),   S( -5,   8),   S(  3,   0),   S(  5,   4),   S(  5,   2),
            S( -4,  -9),   S( -5,   6),   S( 14,  40),   S(  9,  23),   S( 19,  32),   S(  4,  11),   S( -7, -15),   S(  2,  -7),
            S(  5,   9),   S(  8,  30),   S( 11,  31),   S( 17,  50),   S( 21,  34),   S( 11,  20),   S( -3,   6),   S(-10, -11),
            S(-18, -19),   S(-17,  -5),   S(  5,  20),   S( 15,  35),   S( -4,   2),   S( -1,  10),   S( -8,  -5),   S( -5,  -5),
            S( -5, -16),   S(-10, -26),   S(-10,  21),   S( 10,  28),   S( 16,  22),   S(  6,  -6),   S(  7,  -3),   S(-11, -25),
            S(  0,   0),   S( -3, -23),   S( 11,  -3),   S(  1,  15),   S( 13,   2),   S( -2,   0),   S( 12,   3),   S(  3, -14),

            /* queens: bucket 10 */
            S(  3,   0),   S( -2,   4),   S(-10, -18),   S(-21, -23),   S(-11, -13),   S( -5,  -5),   S(  3, -10),   S( -4,  -8),
            S( -7, -11),   S( -8, -15),   S(-12, -23),   S( -7, -12),   S( -5,  -7),   S(-18, -13),   S(  1,  -8),   S(-16, -17),
            S(  0, -11),   S( -8, -13),   S( -6,  -7),   S( -1,   2),   S( -6,   2),   S( -6,   5),   S(  2,   2),   S(  3,   6),
            S(  0,  -2),   S(  4,  -3),   S( -1,  -5),   S(  1,  30),   S( 16,  25),   S( -5,   5),   S( -2,  -6),   S(-13, -19),
            S( -4,  -7),   S(  7,  -5),   S( -4,   4),   S( 21,  47),   S(  0,  -4),   S( 17,  29),   S( 12,  13),   S(  1,   5),
            S( -3,  -4),   S(-19, -32),   S( -4,  -2),   S(  1,  12),   S(  5,  16),   S(  5,  21),   S( 11,   6),   S( -4, -11),
            S( -4,  -4),   S(-16, -27),   S(  8,  22),   S( -6,  -8),   S(  6,   6),   S(  3,   6),   S( -3,  -8),   S( -8,  -6),
            S(  7,   1),   S( -1, -17),   S(  7,  -2),   S(  8,  -5),   S( 17,  14),   S(  5,   6),   S( 15,  14),   S(  2,  -8),

            /* queens: bucket 11 */
            S(-10, -14),   S( -6, -19),   S(-21, -19),   S(-10, -27),   S(-12, -18),   S( -9, -10),   S( -5,  -5),   S(-12, -22),
            S(-16, -32),   S( -8,  -7),   S(-39, -34),   S(-10,  -9),   S(-13, -10),   S(-10,  -6),   S( -5,  -9),   S( -6,  -3),
            S(-17, -21),   S(-14, -33),   S(  4, -20),   S( -7, -15),   S( -9, -15),   S( -4,   4),   S(  6,  19),   S(-11,  -7),
            S(-14, -26),   S(-24, -24),   S( -6, -24),   S( 16,  26),   S( 12,   1),   S(-11,  -5),   S( 23,  24),   S( -2,   1),
            S(-13, -12),   S( -4, -15),   S(-20, -24),   S( 24,  24),   S( 15,  14),   S( 27,  49),   S( 20,  39),   S(  2,  10),
            S(-13, -29),   S(  3,   3),   S(-16, -17),   S( 15,  11),   S( 24,   5),   S( 43,  33),   S(  8,  -2),   S( -8,  -7),
            S( -7,  -3),   S(-13, -21),   S(  9,  16),   S(-12,  -4),   S(  5,   5),   S( 22,  23),   S( 36,  37),   S( -3, -17),
            S(-10, -21),   S( -9, -23),   S( -6, -20),   S(  4, -14),   S(  2,  10),   S( -3,  -8),   S( 17,   7),   S( -2, -33),

            /* queens: bucket 12 */
            S(  6,   0),   S( -1,  -1),   S(  2,   1),   S( -8,  -5),   S(-10, -12),   S( -1,  -3),   S(  0,  -2),   S( -4,  -9),
            S( -3,  -2),   S( -8, -14),   S( -9, -12),   S( -5, -10),   S( -2,  -2),   S( -6,  -2),   S( -1,  -9),   S( -5,  -9),
            S( -2,  -5),   S( -6, -10),   S( 12,  14),   S( -4,  -4),   S( -2,  -5),   S( -8, -13),   S(-12, -24),   S( -8,  -7),
            S(  2,   6),   S( -1,   2),   S(  4,   6),   S(  0,   7),   S(  8,  15),   S(  1,  -3),   S(  0,  -4),   S( -4, -10),
            S(  1,  -4),   S( 10,  12),   S( 32,  56),   S(  1,  16),   S( -5,   7),   S(  1,   7),   S(-12, -29),   S( -2, -14),
            S(  7,  17),   S( 13,  24),   S( 33,  42),   S( -3,   7),   S(  0,   5),   S(  2,   2),   S(  5,   5),   S( -4, -15),
            S(  3,   1),   S(  2,   5),   S( 16,  15),   S( 11,   8),   S(  4,   9),   S( -4,   4),   S(  9,   6),   S( -4,  -4),
            S( -5, -29),   S(-10, -26),   S(-11, -20),   S(-10, -27),   S( 10,  -7),   S(  1,  -1),   S(  1,  -6),   S( -7, -12),

            /* queens: bucket 13 */
            S( -1,  -4),   S( -4, -14),   S(  1,  -4),   S( -2,  -7),   S( -3, -10),   S( -2,  -1),   S( -7,  -9),   S( -6,  -8),
            S(  4,  10),   S(  5,  14),   S(  4,  11),   S( -3,  -2),   S( -6,  -6),   S(  2,  11),   S(  1,   6),   S(-11, -19),
            S( -2,  -7),   S(  1,   0),   S(  4,  17),   S(  4,  12),   S( -1,   0),   S( -5,  -8),   S( -4, -11),   S(-12, -16),
            S( -3,  -4),   S(  2,   2),   S( 13,  13),   S( 20,  29),   S( 15,  33),   S( -3,  -7),   S( -5, -13),   S( -5,  -6),
            S( -3,  -5),   S(  6,  17),   S( 15,  40),   S( 12,  37),   S( 23,  43),   S(  0,  -7),   S( -4,  -6),   S( -7, -14),
            S(  0,   0),   S( 12,  31),   S( 37,  73),   S( 18,  40),   S(  0,  16),   S(  1,   7),   S(  6,  15),   S( -5, -14),
            S( -1,   0),   S( 18,  30),   S(  8,  26),   S( 13,  25),   S( -2,   8),   S(  0,  -8),   S( -1,  -9),   S(  5,   7),
            S(-12, -18),   S(  3,  -5),   S( -2,  -8),   S( -9, -12),   S(  6,   1),   S(  4,   7),   S( -7,  -6),   S( -6, -13),

            /* queens: bucket 14 */
            S( -1,  -1),   S(  0,   2),   S( -2,  -7),   S( -9,  -8),   S(  4,   7),   S( -2,  -4),   S( -1,  -8),   S( -4, -10),
            S( -5,  -7),   S(  6,  16),   S( -1,  -2),   S(  0,  -6),   S( -8, -10),   S( -6, -14),   S( -5,  -3),   S( -2,  -6),
            S( -1,  -2),   S( -9, -11),   S( -5,  -9),   S(  1,   0),   S(  2,   0),   S(  1,  -4),   S(  3,   6),   S( -6, -14),
            S( -7,  -8),   S(  8,  10),   S( -4,  -3),   S( 23,  42),   S( 15,  15),   S( -1,   6),   S( 11,  24),   S(  1,  -3),
            S(  4,  14),   S(  4,   1),   S(-12,  -7),   S( 16,  26),   S( 13,  33),   S( 17,  24),   S(  9,  18),   S( -4,  -9),
            S( -2,  -5),   S(  5,  15),   S( 14,  24),   S( 12,  20),   S( 17,  41),   S( 13,  43),   S(  7,  15),   S( -3,  -9),
            S(  3,   7),   S(  8,   9),   S( 16,  36),   S( 19,  32),   S( 15,  33),   S( 13,  26),   S( 16,  28),   S(  1,   5),
            S( -4,  -1),   S( -1,  -1),   S(-10, -14),   S( 12,  19),   S(  0,   2),   S(  2,  -1),   S(  1,   4),   S(-11, -17),

            /* queens: bucket 15 */
            S( -1,  -2),   S(  0,  -5),   S( -5,  -8),   S( -3, -10),   S( -5, -10),   S( -5, -12),   S(-11, -24),   S(  0,  -7),
            S( -1,  -5),   S( -4,  -9),   S( -5, -12),   S( -4, -11),   S(  1,   7),   S( -3,  -7),   S( 11,  14),   S(  3,   1),
            S(  0,  -8),   S( -3, -11),   S(  0,  -1),   S( -4, -11),   S( -4, -11),   S(  6,  17),   S( -1,  -4),   S(  0,  -9),
            S( -5,  -8),   S(  4,   5),   S( -3,  -2),   S(  3,   2),   S(  1,  10),   S(  0,   6),   S(  5,   5),   S(  3,   3),
            S( -2,  -7),   S( -2,  -5),   S( -8, -12),   S( -4,  -4),   S(  5,  11),   S(  8,   6),   S( -4,  -7),   S(  0,  -8),
            S( -3,  -6),   S( -2,  -6),   S( -1,   2),   S(  0,   0),   S( -2,  -6),   S( 19,  29),   S(  3,  -2),   S(  0,  -9),
            S( -6, -13),   S(  3,  -5),   S(  5,   7),   S(  7,   7),   S(  6,   8),   S( 21,  37),   S( 10,  19),   S(  4,   5),
            S(  1,  -4),   S( -5,  -5),   S( -2,  -4),   S( 10,  12),   S(  7,   4),   S(  2,  -3),   S( -3,  -8),   S( -7, -22),

            /* kings: bucket 0 */
            S( 51,  13),   S( 38,  60),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 31,  46),   S(113,  67),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
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
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 61,  63),   S( 63,  47),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-10,  60),   S(-35,  28),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 20,  91),   S(-45,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  9, -53),   S( 83, -26),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-38,  -6),   S( 41,  15),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 42, -10),   S( 29,   4),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S( 33,  31),   S( 25,  27),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 52,  17),   S( 14,  14),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 39,  45),   S( 22,  41),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 75,  28),   S( 12,  -7),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 61,  58),   S( -7,  26),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-61, -119),  S( 19, -63),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50, -104),  S(-93, -20),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  8, -54),   S(-32, -47),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-43, -34),   S(-54, -21),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-14, -39),   S(-14, -40),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-48, -22),   S(-90,   3),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-11, -44),   S(-48, -106),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-80, -12),   S(-19, -89),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-43, -107),  S(-78, -25),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-45, -224),  S(-16, -96),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-66, -59),   S( 22, -66),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(-56, -76),   S(-26, -100),  S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-14, -54),   S(-111, -18),  S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S( 18, -114),  S(-66, -68),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-124,   0),  S(-31, -114),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(-50, -71),   S( -4, -226),

            #endregion

            /* enemy king relative piece square values */
            #region enemy king relative piece square values

            /* pawns: bucket 0 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -22),   S(-20,  23),   S( 12,   2),   S(-11,  32),   S( 23,   6),   S( 47,   9),   S( 57,  -2),   S( 49,   1),
            S( -9, -28),   S(-25,   9),   S(  2, -10),   S(  0, -10),   S( 19,   4),   S(  7,  14),   S( 32,   4),   S( 22,  24),
            S(  4, -31),   S(  2, -18),   S( 29, -35),   S( 11, -18),   S( 19,  -9),   S(  9,  29),   S( -1,  52),   S( 26,  20),
            S(  7, -19),   S( 27,   5),   S( 50, -29),   S( 34,  -6),   S( 17,  43),   S(-10,  87),   S( 14,  91),   S( 54,  63),
            S( 89, -53),   S(117, -15),   S( 84, -22),   S( 46,  17),   S( 49, 134),   S( 10, 136),   S( 22, 154),   S( 67, 131),
            S(-220, -74),  S(-127, -134), S( 11, -170),  S( 35,  44),   S( 87, 197),   S( 74, 187),   S(113, 168),   S( 99, 146),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 1 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  19),   S(-36,  33),   S(-15,  15),   S(-37,  61),   S(-11,   8),   S( 20,  12),   S( 25,   7),   S( 15,  25),
            S(-51,  15),   S(-43,  25),   S(-27,  11),   S(-18,  10),   S(  4,   9),   S( -9,  12),   S(  0,   8),   S(-15,  20),
            S(-48,  19),   S(-17,  22),   S(-25,   4),   S(  5, -11),   S( -3,  18),   S(-24,  20),   S(-28,  35),   S(-17,  25),
            S(-41,  41),   S(  8,  30),   S(-23,  25),   S(  7,  25),   S(  0,  27),   S(-33,  47),   S(  5,  42),   S( 27,  53),
            S(  3,  34),   S( 55,   4),   S( 87, -25),   S( 79, -20),   S( 36,  29),   S( 10,  36),   S(-21,  81),   S( 42,  88),
            S( 39,  42),   S(-37, -19),   S(-15, -103),  S(-20, -99),   S(-39, -68),   S( -3,  47),   S( 49, 185),   S( 68, 212),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 2 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-40,  39),   S(-20,  29),   S(-16,  17),   S(  1,   9),   S(-22,  37),   S( -6,  15),   S( 13,  -2),   S( -6,  19),
            S(-47,  29),   S(-31,  32),   S(-26,  10),   S(-25,  20),   S(-20,  18),   S(-27,   9),   S( -8,  -4),   S(-36,  10),
            S(-44,  44),   S(-32,  54),   S(-12,  15),   S(-15,  15),   S(-19,  17),   S(-26,   3),   S(-27,  11),   S(-35,   7),
            S(-30,  85),   S(-33,  76),   S(-13,  41),   S( -1,  33),   S(-10,  31),   S(-26,  18),   S(  5,  22),   S( 18,   9),
            S(-23, 130),   S(-37, 118),   S(  2,  23),   S( 24, -21),   S( 88,  -9),   S( 85,  -5),   S( 71, -13),   S( 45,   3),
            S( -8, 245),   S( 42, 176),   S( 15,  71),   S( 26, -91),   S(-25, -170),  S(-89, -132),  S(-25, -65),   S(  9,  20),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 3 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  13),   S(  8,  20),   S( 13,  15),   S(  3,  35),   S(  0,  52),   S( 37,  22),   S( 31,   5),   S(  6, -13),
            S(  1,  15),   S(  6,  29),   S(  3,  10),   S( -1,  11),   S( 13,  16),   S( 20,   1),   S( 15,  -4),   S(-20,  -5),
            S(  2,  32),   S( -3,  60),   S(  9,  19),   S(  8,  -1),   S( 26, -13),   S( 14, -13),   S(  6, -17),   S(-20, -13),
            S(  2,  89),   S( -9, 106),   S( 14,  64),   S( 20,  28),   S( 25,  -1),   S( 32, -24),   S( 19,   8),   S( 29, -20),
            S(  3, 155),   S( -3, 166),   S(-19, 166),   S( -3, 111),   S( 37,  52),   S( 85, -12),   S(104, -29),   S( 94, -37),
            S(103, 125),   S( 51, 240),   S( 31, 251),   S( 11, 207),   S(-22,  93),   S( 29, -175),  S(-75, -235),  S(-158, -178),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 4 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 75,  -4),   S( 32,   9),   S( 11, -12),   S(-10,  -8),   S(  6,  -9),   S( 10,  -9),   S(  8,  -2),   S(-56,  41),
            S( 36,  -5),   S(-11,  28),   S( -6,  -1),   S(-22,  -3),   S(-25, -20),   S(-12, -14),   S(-25, -15),   S(-43,   2),
            S( 54, -15),   S( 73, -16),   S( 15, -14),   S(-41,   0),   S(-72,  10),   S( -6,   3),   S(-65,  26),   S(-64,  28),
            S(-94, -73),   S(-19, -89),   S( 68, -60),   S(-33,   4),   S(-24,  15),   S(-47,  62),   S(-18,  54),   S(-47,  73),
            S(-42, -75),   S(-66, -114),  S(-12, -95),   S( 54,   6),   S( 76,  87),   S(  9,  99),   S( 25,  76),   S(  7,  97),
            S(  0, -64),   S(-18, -78),   S( -1, -68),   S(  2,  47),   S( 58,  85),   S( 70, 148),   S( 45, 154),   S( 60, 124),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 5 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-56,  45),   S(-36,  49),   S(  1,  22),   S( 58,   7),   S( 68,   0),   S( 14,   5),   S(-12,  18),   S(-57,  47),
            S(-88,  40),   S(-52,  49),   S(-42,  28),   S(-30,  28),   S(-40,  27),   S(-37,  13),   S(-57,  14),   S(-75,  33),
            S(-55,  30),   S(-60,  65),   S(-14,  36),   S(-31,  47),   S(-56,  49),   S(-77,  37),   S(-67,  39),   S(-65,  41),
            S(-47,  40),   S(-31,  19),   S(-47, -37),   S(-12, -27),   S(-15,  -6),   S(-58,  34),   S( -2,  32),   S(-27,  52),
            S( 45,   9),   S(-21, -28),   S( 12, -92),   S( -8, -72),   S( 39, -41),   S( 24,  21),   S(-13,  70),   S(-33, 112),
            S( 45,  31),   S( 19, -13),   S(-33, -69),   S(-23, -63),   S(-32, -58),   S( 47,  39),   S( 67, 134),   S( 40, 147),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 6 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-83,  39),   S(-45,  24),   S(-12,   6),   S( 20,   4),   S( 14,  26),   S( 23,  11),   S( 20,  13),   S(  1,  24),
            S(-73,  22),   S(-53,  20),   S(-53,  12),   S( -3,  16),   S(-36,  31),   S(-31,  18),   S(-37,  23),   S(-22,  13),
            S(-59,  31),   S(-70,  45),   S(-62,  33),   S(-63,  47),   S(-30,  46),   S(-24,  26),   S(-24,  33),   S(-35,  19),
            S(-85,  84),   S(-53,  60),   S(-38,  34),   S(-21,  14),   S(-25, -33),   S(-30, -26),   S(-36,  13),   S( 12,   0),
            S( -7, 100),   S(-40,  72),   S( 23,  11),   S(-24, -29),   S(-14, -72),   S(-54, -66),   S(-21, -28),   S( 70,  -6),
            S( 80,  76),   S( 72,  89),   S( 45,  23),   S( 34, -80),   S( -8, -105),  S(-40, -56),   S( -8, -46),   S( 76,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 7 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-50,   1),   S(-30,  -9),   S(  4, -21),   S(-59,  49),   S( 26,   7),   S( 71, -19),   S( 62, -19),   S( 70, -13),
            S(-56,   1),   S(-53,   8),   S(-30, -19),   S(-39,   4),   S( -9,   0),   S( 28, -25),   S(  5,  -3),   S( 41, -17),
            S(-57,  21),   S(-69,  41),   S(-37,   5),   S(-45,   0),   S(-15,   1),   S(  2, -12),   S( 22,   0),   S( 37, -17),
            S(-54,  60),   S(-81,  82),   S(-48,  58),   S(-33,  32),   S(-17,  -3),   S( 37, -58),   S(  3, -63),   S( 18, -108),
            S( 17,  60),   S(-50, 134),   S(  6, 115),   S( -6,  86),   S(  7,  21),   S(  6, -79),   S(-49, -130),  S(-31, -98),
            S(130,  82),   S( 84, 124),   S( 96, 104),   S( 63,  93),   S( 33,   3),   S(  1, -105),  S(-30, -93),   S(-12, -185),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 8 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 17,   7),   S( -6,  15),   S( 46,  -3),   S(-10, -37),   S(-28, -59),   S(-16, -25),   S( 17, -48),   S( 34, -40),
            S(  9, -59),   S(  0,  -9),   S(-40, -57),   S(-58, -35),   S(-24, -59),   S( 41, -62),   S( 13, -59),   S( -2, -53),
            S( 18, -97),   S(  6, -55),   S(-15, -65),   S(-41, -56),   S(-28, -31),   S( 17, -43),   S(-34, -16),   S( -1, -32),
            S(  1, -29),   S(-27, -36),   S( 13, -23),   S(-14,  -5),   S(-18,   5),   S(  7,  20),   S(  3,  28),   S( -6,  19),
            S( 25,   4),   S(  2, -31),   S(  8,  43),   S( 35,  91),   S( 54, 119),   S( 33, 118),   S( 16,  96),   S(-30, 102),
            S( 18,  32),   S(  7,  52),   S( 24,  68),   S( 32,  99),   S( 46,  94),   S( 51, 147),   S( 39, 100),   S(-21,  92),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 9 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 15,  10),   S( 16,  31),   S( 10,  15),   S(  1,  24),   S( 14,   2),   S(  1, -14),   S( 30, -37),   S(-22, -18),
            S( 41, -55),   S( -2, -40),   S( -8, -55),   S(-30, -45),   S(-36, -25),   S(-52, -26),   S(-50, -25),   S( 18, -45),
            S(-21, -44),   S(-44, -36),   S(-39, -74),   S(-76, -43),   S(-14, -36),   S(-19, -46),   S(-51, -29),   S( 15, -35),
            S(-47,  -2),   S(-48, -48),   S(-12, -72),   S(-45, -33),   S( -4, -46),   S(  0, -26),   S( 19,  -5),   S(  3,   5),
            S(  5,  11),   S( -3, -20),   S(-15,   3),   S( 20,  27),   S( 18,  59),   S( 20,  51),   S(  6,  66),   S(  3,  61),
            S( -7,  65),   S( 28,  61),   S( -1,  56),   S( 23,  61),   S( 26, 108),   S( 16,  84),   S( 17,  78),   S( 16,  79),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 10 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-30, -50),   S( -6, -39),   S( -9, -16),   S( -5, -14),   S( 28,  14),   S( 47,  11),   S(  7,  14),   S( -6, -17),
            S(-10, -59),   S(-66, -34),   S(-19, -49),   S( 12, -36),   S(-28, -26),   S(-27, -20),   S( -1, -27),   S(  5, -42),
            S(-18, -49),   S(-89, -17),   S(-68, -40),   S(-30, -29),   S(-36, -48),   S(-31, -63),   S(-38, -53),   S( 53, -70),
            S(-37,  -3),   S(-17,  -3),   S(-28, -34),   S(-58, -42),   S( -6, -71),   S(-49, -56),   S(-18, -52),   S( 17, -53),
            S( 12,  15),   S( 32,  17),   S( 19,  11),   S(-18,  -4),   S( 12,  17),   S( 13,  13),   S(-24,   8),   S( 43,  -6),
            S(  9,  23),   S(  3,  48),   S( 25,  54),   S(  7,  60),   S( 24,  81),   S(  2,  43),   S(-13,  19),   S( 26,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 11 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-37, -46),   S(  4, -39),   S(-29, -40),   S(  6,  -2),   S(  2, -21),   S( 61,   9),   S( 34,   0),   S( 49, -11),
            S(-35, -62),   S(-45, -56),   S(-28, -72),   S(  7, -63),   S(-29, -29),   S( 13, -47),   S(  8, -35),   S( 35, -71),
            S(-18, -43),   S(-84,  -1),   S(-25, -25),   S( -9, -30),   S(-61, -47),   S( 30, -67),   S( 23, -118),  S( 68, -103),
            S(-49,  18),   S(-67,  36),   S(  5,  24),   S( 19, -12),   S(-29, -18),   S(-21, -50),   S(-33, -54),   S( 36, -101),
            S(-13,  17),   S(-14,  67),   S( -9,  92),   S( 21,  59),   S( 28,  57),   S( -7,   2),   S(  2,   6),   S(  8, -28),
            S( 16,  67),   S( 26,  55),   S( 32,  78),   S( 26,  80),   S( 12,  60),   S( 34,  81),   S( 12,  32),   S( 26,  35),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-12, -103),  S( 29, -49),   S( -3, -28),   S(  0,  -2),   S( -5, -29),   S(-32, -70),   S( 20, -38),   S(  6, -47),
            S( 36, -89),   S( 28, -46),   S(-23, -78),   S(-32, -62),   S(-31, -88),   S( -7, -62),   S( -6, -84),   S(-19, -69),
            S( -8, -65),   S( -7, -78),   S(-22, -96),   S(-25, -88),   S( -8, -57),   S( -4, -48),   S(-34, -53),   S( -7, -80),
            S(-13, -38),   S( -2, -14),   S(-20, -23),   S( -2,  -2),   S( 18,  55),   S(  6,  41),   S(  8,  15),   S( -6,  -7),
            S( 11,  22),   S(  1,  16),   S(  3,  22),   S( 19,  61),   S( 30,  77),   S( 28,  87),   S( 14,  81),   S( 19,  52),
            S( 12,  29),   S(  2,  35),   S( 13,  52),   S( 12,  60),   S( 25, 102),   S( 24,  91),   S(-20, -22),   S(-14,   2),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 13 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-20, -62),   S( 22, -76),   S( 19,   5),   S( -2, -12),   S(  5, -21),   S(-33, -37),   S( -9, -67),   S(-15, -69),
            S( 24, -133),  S( 19, -101),  S( -3, -87),   S(  8, -15),   S(-26, -55),   S(  2, -81),   S(  2, -87),   S(  3, -89),
            S( 30, -89),   S( -7, -74),   S( -4, -92),   S(  6, -61),   S(-43, -29),   S( 23, -75),   S(  0, -70),   S( 59, -92),
            S( 16, -28),   S(  3, -31),   S(  2, -30),   S( -4,  21),   S( 14,   6),   S(-13,   7),   S(-10, -11),   S(  8, -24),
            S( -4,  40),   S(  9,  26),   S( -2,   4),   S( 22,  54),   S( 37,  78),   S( 28,  87),   S( 13,  94),   S( -7,  55),
            S( 12, 102),   S( 30,  51),   S(  5,  35),   S( 12,  44),   S( 20,  65),   S( 10,  51),   S( -4,  39),   S(  3,  65),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 14 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-13, -119),  S(  5, -65),   S( -6, -40),   S(  2,   2),   S( -6, -14),   S( -4,   0),   S( 18, -62),   S(-14, -41),
            S( 18, -114),  S(-34, -102),  S( -7, -82),   S(-29, -89),   S(-11, -60),   S( 15, -56),   S(  5, -62),   S( 20, -88),
            S( 17, -96),   S(-18, -73),   S(-14, -65),   S(  4, -77),   S(-23, -52),   S(  4, -95),   S(  6, -98),   S( 37, -64),
            S(  4, -34),   S(-18, -37),   S( -5,  -6),   S(-19, -12),   S( 13, -56),   S( -3, -27),   S( 14, -26),   S( 13,  -8),
            S(-14, -16),   S(  6,  45),   S( 12,  52),   S( -8,  15),   S( 19,  69),   S(  3,  14),   S( 18,  47),   S( 22,  63),
            S( -5,  31),   S(  8,  48),   S( 27,  73),   S( 21,  72),   S( 16,  57),   S(  2,  34),   S( 24,  85),   S( 23,  91),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawns: bucket 15 */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( -7, -28),   S(  7, -53),   S(-24, -52),   S(-10, -26),   S(-12, -29),   S(-17, -40),   S( -7, -53),   S(  2, -86),
            S(-23, -67),   S(-17, -93),   S(-14, -104),  S( -9, -37),   S(-20, -25),   S( -6, -36),   S(  9, -54),   S(  8, -110),
            S(-26, -49),   S(-30, -58),   S(-43, -53),   S(  8, -43),   S(-31, -40),   S( -6, -75),   S(  5, -46),   S(  5, -48),
            S(  9, -37),   S(-25, -12),   S( -2,  40),   S(-20,  10),   S( 11,   6),   S( -8, -22),   S( -5, -13),   S( -7,  30),
            S(  6,  46),   S(  3,  53),   S(  1,  69),   S( 12,  60),   S( 25,  80),   S( 12,  63),   S( 17,  56),   S( 10,  20),
            S(-22,   6),   S( -7,   6),   S( 10,  71),   S( 20,  54),   S( 21,  69),   S( 20,  57),   S( 11,  35),   S( 16,  23),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* knights: bucket 0 */
            S(-75, -30),   S(-26, -26),   S(-18,  -6),   S( -9,  25),   S(-16, -23),   S(-24,   0),   S( -5, -28),   S(-75, -41),
            S( 14, -36),   S(  1,   0),   S(-21, -31),   S( -8, -10),   S( -8,  -6),   S( -7, -23),   S(-33, -47),   S(-28, -36),
            S(-19, -21),   S( 21, -33),   S(  1,   7),   S( 34,  22),   S( -9,  10),   S(  8,  -6),   S(-29,  21),   S(-25, -36),
            S( 10,  22),   S( 32,  50),   S( 29,  31),   S( 38,  16),   S( 28,  16),   S( 11,  21),   S( 36, -20),   S(-10, -26),
            S( 57,  39),   S( 21,  52),   S( 63,  59),   S( 58,  40),   S( 65,  29),   S( 16,  20),   S( 18, -11),   S(  6,  -4),
            S( 90, -41),   S( -4,  44),   S(131,  -4),   S( 70,  41),   S( 53,  46),   S(-39,  64),   S( 35, -12),   S(-22,   3),
            S( 48,  -8),   S( -3, -21),   S( 41,  23),   S( 83,  70),   S( 39,  27),   S( 11,  32),   S( -9,   6),   S(-44,   1),
            S(-111, -123), S( -2,   0),   S(  7,   4),   S( 19,  23),   S(  4,  32),   S( 19,  13),   S(-33,  -5),   S( -9,  10),

            /* knights: bucket 1 */
            S( 17,  -4),   S(-52,  19),   S(-20,   9),   S(-32,  31),   S(-14,  33),   S(-16, -21),   S(-25,  -6),   S(  7, -20),
            S(-36,  31),   S(-45,  54),   S(-25,  25),   S(-13,  21),   S(-17,  18),   S( -3,  22),   S(-10,  -4),   S(-15, -53),
            S(-35,  26),   S(  0,   0),   S(-18,  17),   S( -5,  49),   S(-10,  35),   S( -5,   7),   S(-38,  28),   S(-12,  20),
            S(-13,  66),   S( 29,  33),   S( -4,  51),   S( -7,  61),   S( -5,  55),   S( -5,  51),   S(  1,  21),   S(-23,  47),
            S( 60,  -4),   S( 13,  17),   S( 46,  57),   S( 19,  50),   S( 41,  44),   S( -2,  63),   S( -5,  46),   S( -3,  58),
            S( 23,  23),   S( 52, -16),   S( 71,  17),   S( 83,  24),   S( 69,  22),   S(-28,  74),   S( 17,  28),   S(  2,  39),
            S( 14,   1),   S( 32,  -5),   S( 29, -11),   S( 25,  50),   S(  9,  36),   S( -2,  24),   S( 16,  73),   S(-29,  43),
            S(-154, -35),  S( 15, -16),   S(-36, -61),   S(-19,  10),   S( -4,  12),   S( 39,  49),   S( 17,  45),   S(-68,  22),

            /* knights: bucket 2 */
            S(-59,   9),   S(-33,  31),   S(-21,   3),   S(-10,  19),   S(-11,  12),   S(-49,   3),   S(-23,   6),   S(-14, -28),
            S(-15,   6),   S( -1,  32),   S(-20,   9),   S(-16,  17),   S(-24,  23),   S(-14,   6),   S( 11,   5),   S(-28,   1),
            S(-31,  47),   S(-21,  22),   S(-17,  17),   S(-14,  54),   S(-16,  43),   S(-17,  10),   S(-21,  14),   S(  2,  -7),
            S( -3,  48),   S( -3,  37),   S(-22,  72),   S(-11,  71),   S(-30,  71),   S(  7,  46),   S( 13,  31),   S( -1,  35),
            S( -5,  58),   S(-16,  66),   S(  5,  65),   S( 19,  57),   S(  4,  65),   S( 19,  68),   S( -1,  56),   S( 20,  12),
            S(-40,  65),   S(-18,  49),   S(-11,  83),   S( 38,  26),   S( 38,  26),   S(114,  -5),   S( 65,   1),   S( 31, -13),
            S( 32,  35),   S(-41,  60),   S( 43,  26),   S( 26,  12),   S( -8,  50),   S( 13,  -4),   S( 24,  25),   S( 20,  -4),
            S(-54,  28),   S( 28,  62),   S(-14,  70),   S(-11, -23),   S(-23,  -8),   S(-34, -46),   S( 17,  -3),   S(-121, -56),

            /* knights: bucket 3 */
            S(-47,  17),   S( -8, -52),   S(  4, -20),   S(  6, -10),   S(  8, -16),   S( -4, -24),   S(-14, -25),   S(-23, -74),
            S(-14, -32),   S(  4,  -7),   S( 11, -13),   S( -1,  -3),   S( -2,  -2),   S( 23, -19),   S( 27, -40),   S( 24, -58),
            S(-10,  -4),   S(-10,   5),   S(  6,  16),   S( 11,  40),   S( 16,  26),   S(  2,  15),   S( 15,  -2),   S( 23, -34),
            S( 12,  -4),   S( 17,  21),   S( 20,  40),   S( 14,  47),   S( 16,  67),   S( 31,  55),   S( 37,  46),   S( 18,  34),
            S(  0,  39),   S( 24,  29),   S( 26,  49),   S( 29,  75),   S( 29,  74),   S( 42,  81),   S(  8,  91),   S( 65,  77),
            S( -8,  27),   S(  6,  41),   S( 14,  57),   S( 24,  71),   S( 58,  73),   S(134,  61),   S( 62,  74),   S( 22,  91),
            S(-22,  38),   S(-12,  47),   S(-10,  61),   S( 32,  62),   S( 48,  64),   S( 95,  46),   S( 15,   2),   S( 83,  19),
            S(-148,  36),  S(-30,  75),   S(-44,  84),   S( 35,  50),   S( 59,  79),   S(-51,  71),   S(-28, -42),   S(-62, -107),

            /* knights: bucket 4 */
            S(  9,  11),   S( -8,  -8),   S(-48,  17),   S(-29, -10),   S(-29,  23),   S(-13, -10),   S( 20, -27),   S(-17, -16),
            S( 21,  36),   S(  7, -21),   S( -4,  11),   S( -6,   6),   S( -1, -10),   S( 18, -42),   S( -7,  12),   S(-45,  -5),
            S( -5, -17),   S( 14,  -2),   S( 50,   5),   S( 61,   6),   S( 14,  19),   S( 43, -31),   S( -7, -25),   S(-10, -33),
            S(-33, -35),   S( 23,  -3),   S( 34, -20),   S( 63,  -2),   S( 28,   9),   S( -8,  23),   S(-30,  23),   S( -5,   8),
            S(-13, -50),   S( 16, -16),   S( 44,   8),   S( 27,  42),   S( 45,   4),   S( 12,  16),   S( 30, -12),   S(-30,  39),
            S( -5, -26),   S( -6,  -5),   S( 32, -26),   S( 53,  22),   S(  3,  20),   S(-20,  36),   S(-20,  -1),   S( 20,   1),
            S(-18, -30),   S(-23,  -9),   S(  3,  -3),   S( 23,  20),   S( 27,  12),   S(  0,  12),   S( 14,  35),   S(-34, -13),
            S(  3,  13),   S(-12, -36),   S( -7, -30),   S( 14,   1),   S( 13,  18),   S( -4,  14),   S( -5,  17),   S(-17, -16),

            /* knights: bucket 5 */
            S( 20,  22),   S( 20,  27),   S(-25,  35),   S( -5,  25),   S(  0,  30),   S( 18,  16),   S(-11,  18),   S( 10,  22),
            S( 23,  28),   S( 38,  25),   S(  7,  10),   S(-12,  17),   S( 43,  -8),   S(-20,  15),   S( -3,  42),   S(-44,  17),
            S(-29,  24),   S( -8,   5),   S( 22,  15),   S( 22,  21),   S( 20,  18),   S(-16,  25),   S( -5,  15),   S(-45,  17),
            S( 25,  14),   S( 18, -20),   S( 34,  -1),   S( 67, -16),   S( 72,   1),   S( 65,   6),   S( -6,  18),   S( 20,  30),
            S( 36,   5),   S( 11,  -8),   S( 71, -14),   S(106, -12),   S( 70, -18),   S( 34,  18),   S(  1,   6),   S( 16,  23),
            S( -5, -20),   S( 29, -28),   S( -3, -23),   S(  1,  11),   S( 18,   0),   S( 41,   3),   S(-16,  15),   S( 25,  31),
            S(  0,   4),   S(-29, -58),   S( -6, -47),   S(-11, -14),   S( -9, -37),   S(  4,   5),   S( -2,  41),   S( 20,  32),
            S(-22, -38),   S(-27, -66),   S(  8,  -9),   S(-24, -27),   S(  6,  -5),   S(  0,  29),   S( 20,  36),   S( -2,  18),

            /* knights: bucket 6 */
            S( -5, -10),   S(-36,  23),   S(-14,   5),   S(-30,  37),   S(-29,  31),   S( -5,  34),   S( -7,  42),   S(-33,   7),
            S(  9, -16),   S( -7,  45),   S( -7,   2),   S( 31,   9),   S( 20,  21),   S(-39,  40),   S(-13,  49),   S(-36,  66),
            S( -1,  14),   S( 20,  15),   S(  5,  26),   S( 23,  36),   S( 16,  36),   S(-51,  45),   S( 19,  30),   S(-12,  40),
            S( 11,  44),   S( 47,   5),   S( 28,  27),   S( 58,  10),   S( 67,  -6),   S( 49,  12),   S( 12,  13),   S(-23,  45),
            S( -5,  37),   S( 23,  15),   S( 76,  11),   S( 91,   2),   S( 79, -16),   S( 41,  22),   S( 92, -21),   S( 17,  27),
            S( 15,  16),   S( 15,  10),   S( 39,  23),   S( 24,  11),   S( 34,  -2),   S( 27,  -5),   S( -4, -11),   S( 17,   3),
            S(  3,  28),   S( 18,  33),   S( 33,  37),   S( -3,  -7),   S( 23, -10),   S( 16, -36),   S( -9,  -7),   S( 10,  39),
            S( 13,  29),   S(  1,  26),   S( 16,  32),   S(  0,  15),   S(  6,  -6),   S( -9,  -3),   S(  8,  22),   S(-25, -37),

            /* knights: bucket 7 */
            S(-33, -43),   S(-15, -43),   S(  4, -16),   S(-35,  20),   S( -1,  -3),   S(-32,   5),   S(-13,  -6),   S(-15,  20),
            S(-32, -53),   S( -2, -29),   S(-31,  -7),   S(-27,  -2),   S(  5,   8),   S(  1,  26),   S( -5,  13),   S(-57,  36),
            S(  6, -41),   S(-29, -24),   S( 12, -17),   S(  0,  23),   S( 41,  18),   S( 30,  12),   S(  4,  22),   S( -6,  31),
            S(-33,  11),   S(  9, -10),   S( 54, -22),   S( 72,   4),   S( 96,  -9),   S( 68,  12),   S( 53,   0),   S( 55,  -2),
            S(  2,   3),   S( -1,   5),   S( 18,  14),   S( 63,   2),   S( 90,   6),   S(116, -24),   S(175, -22),   S( 14, -18),
            S(-17,  10),   S( 22,   6),   S( -9,   9),   S( 34,  23),   S( 83,   4),   S( 80,  -9),   S( 10, -15),   S( -6, -47),
            S(-20,   1),   S( -3,   1),   S(  0,  12),   S( 24,  23),   S( 56,  18),   S( 24,  22),   S(-17, -35),   S(-18, -39),
            S(-31, -41),   S( -9,   6),   S( -4,  19),   S(  4,  17),   S( 11,   8),   S( 17,  10),   S(  4,  -5),   S(  0, -10),

            /* knights: bucket 8 */
            S( -2,   4),   S(  9,  25),   S( 11,  26),   S( -9, -29),   S( -1,  24),   S( -3, -18),   S( 13,  24),   S( -3, -14),
            S( -6, -22),   S( -5, -22),   S( -8, -36),   S(-12,   7),   S( -5,  35),   S(  1,  -5),   S(  0,  -6),   S( -3,  -4),
            S(-11, -38),   S( -8, -23),   S(  0, -42),   S(  3,  14),   S(-12, -17),   S( 11,  10),   S( -1,  -6),   S( -1, -15),
            S(-19, -55),   S(-10, -31),   S(  4,  19),   S( -3,  12),   S(-19, -14),   S(-27, -16),   S(-20, -33),   S(-15, -36),
            S( -7, -24),   S(  3, -20),   S( -1, -16),   S(  0,  -4),   S(-18,   0),   S(-12, -17),   S(  3,  -4),   S( -1, -13),
            S( -2,  11),   S( 12,   2),   S( -1,   9),   S( -5,  -8),   S( -6,   0),   S( -4, -11),   S( -9,  -7),   S( -7, -21),
            S(  1,  18),   S( -1, -25),   S(-12, -17),   S(  5,  14),   S(  3,   1),   S(  1,  -2),   S( -3,   2),   S( -3, -19),
            S(  0,   1),   S( -4,   5),   S( -5,   1),   S(  3,  -4),   S( -1,   5),   S( -2,  -8),   S(  0,   4),   S( -3,  -5),

            /* knights: bucket 9 */
            S(-20, -63),   S( -5,  -1),   S( -5, -33),   S( -4, -34),   S(-15,  -8),   S(-12,  10),   S(  6,  19),   S(  1, -10),
            S( -6,   2),   S(-15, -47),   S(-20, -103),  S(-28, -58),   S(-11, -33),   S(-22, -57),   S(-10,  -2),   S(-12,   0),
            S( -9, -20),   S(-16, -38),   S(-14, -29),   S( -5, -47),   S(-24,  -5),   S(  9,  13),   S(-14,  -3),   S( -3,   2),
            S(-17, -45),   S(-13, -45),   S(-11, -24),   S(-14, -40),   S(-19, -30),   S(  0,   2),   S(-18, -41),   S(  2,   6),
            S(  4,  27),   S(-10, -26),   S( -6, -20),   S( -5, -32),   S(-12, -26),   S( -6,   9),   S( -9, -13),   S( -4,  -1),
            S(-13, -19),   S(-19, -33),   S(-11, -16),   S( -4, -11),   S(  1,  20),   S( -6,   3),   S( -4,  19),   S( -1,   6),
            S(-10, -15),   S( -1,  20),   S(-11,  -2),   S(-22, -16),   S(  1,   3),   S(  2,  22),   S( -8,  14),   S( -6,   0),
            S(  4,   0),   S(  4,   1),   S( -1,  10),   S(  0,   5),   S(-10,  -8),   S( -5,  -1),   S(  3,   7),   S( -1,  11),

            /* knights: bucket 10 */
            S( -9, -33),   S( -6,   9),   S(-10,  -8),   S(-11,  16),   S(-21, -46),   S(  7, -18),   S( -3,  11),   S( -3,  11),
            S( -4, -18),   S(  9,   2),   S(-14, -24),   S(-10, -46),   S( -9, -26),   S(-25, -51),   S( -9,  13),   S(  1,  26),
            S( -3,  -6),   S( -4,  -7),   S( -7, -10),   S(  4, -43),   S(-26, -35),   S( -6, -13),   S(-14, -29),   S(-10,  10),
            S(-10, -19),   S(-12, -22),   S( -9, -11),   S( -8, -18),   S(-13, -18),   S( -9,  -3),   S(-11, -49),   S( -4,  -2),
            S(-12, -22),   S(-13, -27),   S( -9,  -1),   S( -7, -13),   S( -2,  -6),   S(-10, -38),   S( -5, -14),   S(  4,  10),
            S( -3,   7),   S(-13,   0),   S(-11,  10),   S(-13,  20),   S(-14, -13),   S(-19, -12),   S(-14,  -1),   S(-17,  -6),
            S(  3,   8),   S( -2,  -4),   S( -6, -28),   S( 13, -20),   S( -5,   6),   S(-16, -42),   S( -8,   7),   S(-10, -13),
            S( -1,   1),   S( -2,   7),   S( -1,  14),   S( -4,   3),   S( -5,   2),   S( -7, -13),   S(  5,   8),   S(  1,   6),

            /* knights: bucket 11 */
            S( -3, -16),   S(-25, -26),   S( -4,  -6),   S(  5,  21),   S(-38, -33),   S( -1,  11),   S( -7,   6),   S(  8,  31),
            S( -8, -18),   S(-26, -41),   S(-10, -42),   S( 16,   1),   S(  6,  21),   S( -3, -25),   S(-14, -22),   S( -8, -11),
            S(-13, -42),   S(-18, -22),   S( -1, -10),   S( -1,   1),   S(-10,  25),   S( 13,   1),   S( -2, -11),   S( -4,  -2),
            S(-17, -14),   S(  5, -21),   S( -4, -24),   S(  5,   5),   S( 24,   2),   S( -7, -16),   S( 11,  17),   S( -3,  -9),
            S(-15,   0),   S(  1, -41),   S(-19,  -2),   S(  0, -13),   S( 31,  11),   S(  4,  21),   S(-13, -69),   S(-10, -12),
            S( -9, -26),   S( -7, -47),   S(  3,   6),   S(  8,   0),   S(  8,  35),   S( -7,  -7),   S( -3, -21),   S( -2,  21),
            S( -1,  -8),   S( -8,  15),   S(-11, -13),   S(  6,  -4),   S( 13,  -3),   S(  4, -15),   S(  0, -15),   S( -4,   2),
            S( -3, -17),   S(  1,   5),   S( -3, -12),   S(  1,  14),   S( -4, -10),   S( -1, -10),   S(  5,  15),   S( -1,  -5),

            /* knights: bucket 12 */
            S(-14, -42),   S( -3,  -9),   S( -1, -17),   S(  0,   9),   S( -3,   8),   S( -5, -11),   S( -1,   6),   S( -1,   1),
            S( -3,  -8),   S(  0,   2),   S(  0, -11),   S( -3,   9),   S( -4,  -7),   S(  1,   4),   S(  2,   0),   S(  0,  -8),
            S( -2, -10),   S( -6, -20),   S( -6, -18),   S(-14, -21),   S( -8,  -2),   S( -2,  26),   S( -4,   0),   S( -4,  -9),
            S(  2,  10),   S( -1, -32),   S( -7,  27),   S(  2,  17),   S( -4, -11),   S(  2,  22),   S(  5,  12),   S(  2,   8),
            S(  0,   4),   S( -3,  -4),   S( -4, -19),   S( -4,  -9),   S(  1,   5),   S( -3,   5),   S( -6,  -4),   S( -8,  -8),
            S( -4,  -3),   S( -1,  -3),   S( -3, -13),   S( -1,  -8),   S( -3,  -2),   S( -7, -20),   S(  7,   8),   S( -1,   7),
            S( -4,  -9),   S( -2,  -1),   S( -9,  -2),   S( -3,  -8),   S(  0,   9),   S( -8,  -7),   S( -5, -19),   S( -3,  -4),
            S(  0,   0),   S(  2,   5),   S(  1,   3),   S( -1,  -7),   S(  1,   2),   S(  2,   3),   S( -1,  -4),   S(  0,   0),

            /* knights: bucket 13 */
            S( -1,  -6),   S( -4, -12),   S( -3, -16),   S( -2,  -6),   S( -3, -10),   S( -2,   7),   S( -5,   0),   S(  3,  10),
            S( -2,   9),   S( -2,  -1),   S(  3,  11),   S( -3,   1),   S( -5,  -7),   S( -1,  12),   S(  2,  22),   S( -4,  -6),
            S(  4,  -2),   S(  5,  10),   S(  5,   5),   S( -4, -21),   S(  4,  24),   S( -5,  10),   S(  6,   4),   S( -3,  -3),
            S( -1,  14),   S(  0,   4),   S( -6,  -1),   S(  1,  28),   S(  0,  11),   S( -3,  26),   S(  0,   6),   S( 10,  19),
            S(  1,  22),   S( -2, -14),   S( -3,  15),   S( -7,   9),   S(-15,   1),   S( -4,  23),   S( -8, -24),   S( -3,  -3),
            S( -3,  -4),   S(  2,   2),   S( -3,   7),   S(  2,  10),   S( -8,   7),   S( -8,   3),   S(  2,  19),   S(  0,   2),
            S(  1,   4),   S(  3,   7),   S( -6,  -5),   S( -5,  -1),   S( -2,   6),   S( -4,  -8),   S(  2,   6),   S( -1,   1),
            S(  2,   6),   S(  0,   2),   S( -2,  -3),   S(  2,   4),   S(  0,   1),   S(  1,   2),   S(  0,  -2),   S(  0,   1),

            /* knights: bucket 14 */
            S(  1,  -2),   S( -1,   3),   S(  5,  18),   S( -2,   0),   S( -6, -23),   S( -1,  19),   S(  2,   2),   S( -1,   3),
            S( -2, -11),   S( -8, -15),   S(  2,  -3),   S( -1,   2),   S(  3,   2),   S(  0,   5),   S( -7,   6),   S(  6,  57),
            S( -1,  -1),   S( -4, -34),   S(  7,  18),   S(-11, -36),   S( -4,   2),   S(  1,  12),   S( -1,  10),   S(  3,  18),
            S( -1,  -4),   S( -4, -18),   S(-22, -11),   S( -2,  44),   S(  2,  42),   S( -4,  -2),   S(  0,   5),   S(  1,  36),
            S(  6,  15),   S(-17, -35),   S( -9,  -8),   S( -8,   5),   S( -1,  35),   S(-10,   4),   S( -3,   0),   S(  3,  12),
            S( -1,   2),   S(  5,   4),   S(  3,  -5),   S( -3,  13),   S(  2,  17),   S(  1,  14),   S(  1,   8),   S( -5, -11),
            S(  0,   4),   S( -2,  -2),   S(  3,  15),   S(  6,   4),   S(  3,  11),   S( -5, -12),   S(  2,   6),   S(  3,   5),
            S(  0,  -1),   S(  0,   1),   S( -1,  -2),   S(  2,   4),   S( -1,   1),   S( -1,  -2),   S(  0,  -1),   S(  1,   2),

            /* knights: bucket 15 */
            S( -3, -14),   S( -1,   3),   S(  4,  23),   S( -2,   5),   S( -3, -15),   S(-10, -35),   S( -4, -14),   S( -2, -12),
            S(  2,  -1),   S(  4,   6),   S( -6,  -8),   S(  9,  45),   S(  1,  19),   S( -8, -33),   S( -3,  -3),   S(  1,   3),
            S(  0,  -5),   S( -5, -21),   S(  2,  -8),   S(  6,  11),   S(-18, -26),   S(  0,  -4),   S( -2,  -5),   S( -1,  -1),
            S(  0,  -9),   S( -3,   3),   S( -6, -17),   S( -6,   7),   S( -8,   7),   S(-10,  26),   S(  4,   7),   S( -1,   0),
            S( -1,  -3),   S(  8,  20),   S( -5,   4),   S( -6,   5),   S( 17,  35),   S(  0,  17),   S(  6,  -1),   S(  4,  19),
            S(  1,   2),   S( -4, -11),   S( -2,   0),   S( -9, -18),   S( -6,  -9),   S(  2,  18),   S(  0,   8),   S(  5,  11),
            S( -1,   0),   S( -2,  -7),   S(  4,  15),   S(  3,   3),   S(  4,  13),   S(  5,   7),   S(  1,   7),   S(  3,   8),
            S(  1,   4),   S( -1,  -6),   S(  0,  -1),   S( -1,   0),   S(  2,  10),   S(  1,   1),   S(  0,   0),   S(  1,   2),

            /* bishops: bucket 0 */
            S( 22,  -7),   S( -9,  38),   S(-12,  16),   S(-21,  -8),   S( -1,   1),   S(  3,  13),   S( 66, -40),   S( 18, -15),
            S(-30, -11),   S( -8, -21),   S(-23,  35),   S(  1,  13),   S(  4,  19),   S( 51,  -7),   S( 31,  23),   S( 41, -15),
            S( 14,  10),   S(  6,  24),   S(  6,  -8),   S(  7,  10),   S( 25,  20),   S( 33,  21),   S( 39,   6),   S( 26,   7),
            S( 19, -28),   S( 35, -38),   S( 15,  15),   S( 28,  15),   S( 63,  34),   S( 29,  45),   S( 17,  16),   S(  5,  28),
            S( 33, -10),   S( 47, -21),   S( 58,   5),   S( 76,  41),   S( 86,  22),   S( 24,  42),   S( 31,  43),   S( -7,  14),
            S( 52,  17),   S( 54,  38),   S( 95,   5),   S( 55,  -3),   S( 21,  44),   S(  9,  34),   S( 36,  29),   S( -7,  12),
            S(-45, -78),   S( 70,  35),   S( 88,  82),   S( 20,   2),   S( 21,  -7),   S( 30,  29),   S(-24,  18),   S(-16,  53),
            S(-21, -39),   S( -5,  -7),   S( 14, -25),   S(-14, -15),   S(-10, -15),   S(-17,   8),   S(-16,  17),   S(-31, -36),

            /* bishops: bucket 1 */
            S(-60,  14),   S(  0,  -3),   S(-16,  39),   S( 22,  -7),   S(-13,  22),   S( 12,   6),   S( 41, -12),   S( 24, -33),
            S(  1, -31),   S(-20, -12),   S( -4,  -4),   S(-14,  16),   S( 29,  -6),   S(  6,   4),   S( 46, -35),   S( 13, -10),
            S( -9,   3),   S( 32,  -7),   S(-20,  -9),   S( 19,   7),   S(  6,   1),   S( 29, -27),   S( 18,   1),   S( 61,  -2),
            S( 24, -14),   S( 47, -12),   S( 27,   4),   S( 22,  12),   S( 40,   0),   S( 10,  14),   S( 50,  -2),   S(  0,  18),
            S( 21,  -5),   S( 56, -14),   S( 15,   9),   S( 94, -18),   S( 48,  19),   S( 41,  22),   S( -2,  25),   S( 32,   4),
            S( 64, -42),   S( 49,   8),   S( 53, -24),   S( 58, -10),   S( 73,   4),   S(-39,  12),   S(-22,  53),   S(-27,  19),
            S( 14, -62),   S( -6, -46),   S(-10,   9),   S( 25,  48),   S( 29,  35),   S(-12,  33),   S(-20,   7),   S(-27,  32),
            S(-11, -29),   S(-20,  12),   S( -6, -18),   S(-46,   2),   S(-24,  20),   S( 19,   6),   S( 28,   5),   S(-51,   0),

            /* bishops: bucket 2 */
            S( -1, -21),   S( -7,  -7),   S(  7,  17),   S(-16,   6),   S( 17,  12),   S(-14,   7),   S( 21, -11),   S( -2, -21),
            S( 22, -22),   S(  4, -30),   S( -3,  -6),   S(  7,  14),   S( -9,  11),   S(  8,   5),   S(  0, -31),   S( 18, -53),
            S( 46,   2),   S( 29,   0),   S( -4,  -2),   S( -7,  10),   S(  0,  31),   S(-11, -32),   S( 15, -22),   S( -7,  -5),
            S(-17,   7),   S( 46,  16),   S( -1,  18),   S( 29,  29),   S(  6,  13),   S(  3,  19),   S(-12,   1),   S( 10,  11),
            S(  4,  18),   S(-35,  44),   S( 54,  22),   S( 21,  28),   S( 23,  26),   S( 23,   7),   S( 11,  30),   S( 35,  -5),
            S(-27,  35),   S(  0,  37),   S(-36,  -8),   S( 93,  -2),   S( 47,  13),   S( 86, -14),   S( 71,  14),   S( 45, -46),
            S(-31,  60),   S(-37,   0),   S( -3,  24),   S(  0,  20),   S(-52,  -2),   S(-49,  22),   S(-34,   3),   S( -3, -40),
            S(-83, -22),   S(-10,  27),   S(  0,  11),   S(-20,  31),   S(-29,  -7),   S(-33,  13),   S(-10,  -7),   S(-60, -17),

            /* bishops: bucket 3 */
            S( 35, -19),   S( 41, -19),   S( 26, -23),   S( 16,  -3),   S( 20,  11),   S(  2,  30),   S(-11,  51),   S(  0, -23),
            S( 38,   2),   S( 24, -27),   S( 22,  -1),   S( 24,   4),   S( 21,  17),   S( 23,   8),   S( 11, -20),   S( 30, -39),
            S( 16,   1),   S( 38,  37),   S( 20,   9),   S( 19,  28),   S( 20,  31),   S( 12,  -3),   S( 26,  -8),   S( 17,   9),
            S( -7,  16),   S( 12,  43),   S( 25,  50),   S( 36,  46),   S( 36,  21),   S( 29,   7),   S( 28,  -1),   S( 43, -38),
            S(  6,  30),   S( 16,  50),   S(  9,  57),   S( 52,  46),   S( 46,  46),   S( 52,  22),   S( 30,  15),   S(  4,  12),
            S(  4,  33),   S( 19,  54),   S(  5,  12),   S( 21,  41),   S( 52,  41),   S( 74,  44),   S( 47,  39),   S( 47,  72),
            S(-24,  75),   S( -1,  23),   S( 15,  26),   S(  3,  55),   S( 29,  38),   S( 56,  53),   S(-39,  29),   S( 21, -22),
            S(-41,   8),   S(-24,  49),   S(-46,  40),   S(-29,  52),   S( 14,  17),   S(-59,  37),   S( 16,   6),   S( -2,   6),

            /* bishops: bucket 4 */
            S(-35,   4),   S(-25,   7),   S(-32,  20),   S(-50,  17),   S(-31,  -5),   S(-23,  -1),   S(-11, -18),   S(-41, -36),
            S( -8,   5),   S( -8, -15),   S( 62, -27),   S(-36,  22),   S(-56,  29),   S( -9, -24),   S(-30, -28),   S(-26, -20),
            S(  8,  24),   S( -7, -13),   S(  7,  -5),   S( -1,   6),   S( 12,  -3),   S(-63,   5),   S(-15, -27),   S(-51, -14),
            S( 31,   1),   S( 48, -18),   S( 32,   9),   S( 11,  24),   S(-11,  25),   S( 30,  -2),   S(-44,   7),   S( -7, -21),
            S( 15, -12),   S(-20, -23),   S( 31, -14),   S( 11,   3),   S( -5,  28),   S( 18,   9),   S(-17,  30),   S(-53,   3),
            S(-54, -85),   S(-56, -15),   S(-20,  -4),   S(  3,   6),   S(-42,  47),   S( 10,   2),   S(-14,  24),   S( -6,  27),
            S( -2,   0),   S(-25,   0),   S(  3, -17),   S(-27,  -8),   S(  3, -17),   S( 41,   4),   S( -5, -12),   S( 19,  33),
            S( -8,  -7),   S( -2, -20),   S(-10,  -6),   S(  2, -15),   S(-17,   7),   S(  5,  21),   S(  7,  43),   S(  6,   1),

            /* bishops: bucket 5 */
            S(-45,  -7),   S( 23,  -6),   S(-28,  20),   S(-40,  22),   S( -7,   7),   S(-54,  22),   S(-34,  23),   S(-48, -17),
            S(-12,  -5),   S(-25,  -5),   S( 26,   0),   S(-17,  23),   S(-53,  35),   S(-30,  28),   S(-37,  -1),   S(  6,  -9),
            S(  5,  30),   S(-15,   7),   S( 19, -19),   S(  5,  14),   S( -9,  25),   S(-66,   7),   S(-20,  25),   S(-22,  29),
            S( 12,  12),   S( -3,  17),   S( 58, -13),   S( 27,  15),   S(-13,  27),   S(  2,  23),   S(-66,  38),   S(-20,  23),
            S(  9,   0),   S( 21,   0),   S(-29,  12),   S(-30,   1),   S(-13,   9),   S(-17,  19),   S(  8,  23),   S(-42,  19),
            S(  1,  -3),   S(-50,  22),   S(  0, -25),   S(-32, -23),   S(-29,  13),   S(-22,  -7),   S(-23,  25),   S(-31,  48),
            S(-24,  -6),   S( -9, -11),   S(-18,   2),   S(  4,  29),   S( 18,   8),   S(-12,  34),   S(  0,  10),   S(-17,  34),
            S(-16,  -3),   S(-10, -15),   S(  1, -11),   S(-17,   5),   S(-23,  34),   S(  9,  41),   S(-17,  30),   S( 13,   8),

            /* bishops: bucket 6 */
            S(-17, -32),   S(-15,   9),   S(-27,  22),   S(-11,  14),   S(-48,  33),   S(-22,  19),   S(-38,  32),   S(-61,  -3),
            S(-38,  15),   S(-30, -23),   S(-55,  43),   S(-37,  32),   S(-42,  33),   S(-41,  24),   S(-39,  10),   S(-36,  12),
            S(  2,   7),   S(-39,  24),   S( -3, -11),   S(-34,  36),   S(-26,  42),   S(-31,  -7),   S(-10,  -2),   S(-17,  25),
            S(-60,  31),   S(-59,  35),   S(-23,  24),   S( 16,  37),   S( -2,  37),   S(  2,  20),   S( 14,   9),   S(-18,  25),
            S(-40,  24),   S(-30,  32),   S( -6,  18),   S( 42,  17),   S(-40,  19),   S(-34,  11),   S( -3,  17),   S(-25,   3),
            S(-43,  40),   S(-22,  28),   S(-58,   5),   S(-41,  22),   S( -8,   9),   S(-24, -11),   S(-23,  23),   S(-34,   6),
            S(-14,  37),   S(-71,  33),   S(-29,  24),   S(-19,  30),   S( -8,  11),   S(  2,  11),   S( 10,  -8),   S(-30,  18),
            S(-17,   5),   S(-26,  40),   S( -8,  35),   S( 24,  19),   S(-26,  28),   S( 18,  -8),   S(-11,  14),   S(-13,  14),

            /* bishops: bucket 7 */
            S(-19, -49),   S(-53,  -8),   S(-36, -17),   S(-13,  -9),   S(-34,  -3),   S(-31,  -7),   S(-61, -20),   S(-48, -15),
            S( -5, -47),   S( -8, -46),   S( 15, -20),   S(-25, -10),   S(-33,   4),   S(-38,   4),   S(-35, -30),   S( -9, -15),
            S(-42, -23),   S(-24,   5),   S(-12, -23),   S(  8,  -4),   S(  6,  -2),   S( -3, -37),   S(-52,   9),   S(-63,  14),
            S(-17, -26),   S(-63,  25),   S(-29,  10),   S(-18,  22),   S( 82,  -3),   S( -9,  12),   S( 31, -29),   S(-22,  -4),
            S(-21,  -4),   S( 20, -17),   S(-46,  27),   S(  2,   3),   S( 39,  -6),   S( 33,   8),   S(-48,  11),   S(-38, -13),
            S(-68,  29),   S(-35,  44),   S(-16, -11),   S(-88,  37),   S(-39,  23),   S( -9, -12),   S(-13,  24),   S(-62, -86),
            S( -4,  -6),   S(-24,  -4),   S(-40,  22),   S(  0,  11),   S(  1,   5),   S( 20, -20),   S(  5, -24),   S(  2, -11),
            S(-22, -32),   S( -1,   7),   S( -8,  17),   S( -1,  12),   S(-11,   6),   S( 11, -13),   S( 30, -25),   S( -2,  -7),

            /* bishops: bucket 8 */
            S( 34,  58),   S( -2, -34),   S( -2,   0),   S( -8,  44),   S(  3,  23),   S( -6, -33),   S(-15, -25),   S(-11, -17),
            S(  1,  -1),   S( 14,  28),   S( 22,  11),   S(  8,  25),   S(  1, -11),   S(  4,   2),   S(-34, -47),   S( -9,   0),
            S( -7,  -6),   S(-13, -10),   S( 21,  30),   S( 12,  19),   S(  7,  19),   S( -2,  -2),   S(-25, -14),   S(-34, -28),
            S( -4, -13),   S( 27,  20),   S( -7,  23),   S( 18,   8),   S(  4,  34),   S( 12,  24),   S(-12,   4),   S(  3, -20),
            S( 15,  18),   S( 41,  51),   S( 16,  -5),   S( -9,  19),   S(  9,  22),   S(-23,  18),   S( -8, -31),   S(  5,  17),
            S( -8,  -6),   S(  0,   5),   S(  5,  21),   S( 23,  17),   S( 12,  35),   S( 26,   1),   S( -7,  55),   S( -2,  32),
            S(  3,  15),   S(-17, -40),   S( 29,   2),   S( 26,   5),   S( 12,   2),   S( 24,  51),   S( 19,  26),   S(-12,  -3),
            S( -6,  -4),   S(  5,   3),   S(  2,  16),   S(  4,  11),   S( 30,   6),   S( 24,  13),   S( 16,  41),   S( 36,  27),

            /* bishops: bucket 9 */
            S(  6,  29),   S(  7,  15),   S( -1,   1),   S(-29, -20),   S(-21,  -5),   S( -7,  -2),   S( -2,   1),   S( -9,  -6),
            S(  1,   0),   S(  6, -11),   S(  4,  18),   S(-34,   9),   S(-28,  17),   S(-10,  -5),   S(-36, -12),   S(-16, -29),
            S( -6,   7),   S( 18,   9),   S( -5, -17),   S(  2,  33),   S(  9,  19),   S(-31, -19),   S(  0,  12),   S(-10,  -6),
            S( -2,  28),   S( -7, -11),   S( 22,   0),   S( 16,  -4),   S( -9,  20),   S(-11,  15),   S(  3,  24),   S( -3,  10),
            S( 25,  18),   S( 15,  11),   S( 21,  17),   S(  9, -26),   S( 11,  22),   S( -4,  30),   S(  5,  31),   S(-14, -20),
            S( 17,  25),   S( -7,  27),   S(  7, -17),   S( 13,  19),   S( 41, -41),   S( -6,  11),   S( 15,  33),   S( 13,  29),
            S( 13,  12),   S(-10,  13),   S( 12,  15),   S( 24,   2),   S( 27,   2),   S( 34,  20),   S( 16,  30),   S( 18,  58),
            S( 12,  37),   S(  3, -23),   S(  4,  21),   S( 13,  15),   S( 11,  41),   S( 21,  -1),   S( 26,   1),   S( 28,  22),

            /* bishops: bucket 10 */
            S( -2, -33),   S( 12,  13),   S( -4, -15),   S(-24, -16),   S(-66, -10),   S(-29, -52),   S(  8,  -2),   S( -4,  13),
            S( -9,  17),   S( -5, -53),   S( -5, -12),   S(-22, -31),   S(-52,  12),   S(-31, -17),   S(-29, -13),   S(  2,   4),
            S( -9, -34),   S(-17, -13),   S(-20, -28),   S( -6,  31),   S(-17,  20),   S(-14, -27),   S( -6,   7),   S( -5, -15),
            S(-16,  10),   S(-24,   2),   S(-28, -27),   S(  0,   0),   S(-30,  44),   S( 22,   8),   S( 31,  26),   S( -6, -30),
            S( 12,   5),   S(-38,  22),   S( -5,   8),   S(  2,  32),   S( 29, -16),   S( 17,  35),   S( 16, -18),   S( 15,  10),
            S(  9,   8),   S(  9,  16),   S(-12,  -2),   S( 25,  13),   S( 15, -16),   S( -2,  -9),   S(  8,   9),   S( 25,  16),
            S( 21,  39),   S( -2,   3),   S( 33, -12),   S( 15,  30),   S(  2,  18),   S( -4, -21),   S(  1, -15),   S( 22,  29),
            S( 12,  26),   S( 21,  31),   S( 47,  16),   S( 10,  20),   S( -2,  21),   S(  7,  12),   S( 14,  17),   S(  0, -13),

            /* bishops: bucket 11 */
            S( 10, -18),   S( -6, -12),   S( -7,  -5),   S(  2,  -1),   S(-18, -13),   S( -3,  -1),   S(-21, -24),   S(-11,   2),
            S( -5, -12),   S(  3, -19),   S( -9,  12),   S(  1, -11),   S(-14,  15),   S(-40,  -4),   S(-35, -12),   S( 10,   5),
            S(-10, -49),   S(  0, -18),   S( -9, -37),   S(-31,   8),   S( -6,  -2),   S(  5,  26),   S( -1,  -3),   S( -2, -15),
            S(  2,  -5),   S( -2, -37),   S(  7,  -7),   S(-32, -20),   S(  6,   0),   S( 12,  45),   S( 36,  11),   S( -9, -27),
            S(-12, -17),   S(-14, -17),   S(-36,  33),   S(-29,  32),   S(-26,  31),   S( 34,   6),   S( 22, -17),   S(  8,   6),
            S( -6,   6),   S( -9, -11),   S( -8, -10),   S(  3,  24),   S( 23,  20),   S(  7, -29),   S(  1, -14),   S( -2, -16),
            S( -2,  -6),   S( 17,  26),   S( 22,  50),   S( 33,  26),   S( 20,  -6),   S( -5,  -2),   S(-18, -27),   S( -7, -12),
            S( 28,  18),   S(  6,   4),   S( 30,  47),   S( 30, -17),   S( 18,  18),   S(  4,   4),   S( -6, -13),   S(  5,  -5),

            /* bishops: bucket 12 */
            S( -5, -11),   S( -4, -13),   S( -5,   0),   S(  7,  20),   S( -9,  -9),   S( -7,  -3),   S(  0,   5),   S( -1,   1),
            S(  0,  -5),   S(  6,   3),   S(  0,  -2),   S(  1,  15),   S(  0,  10),   S(  9,   7),   S(-14, -22),   S( -1,  -5),
            S(  8,   5),   S( 11,  -3),   S( 21,  16),   S( 21,  18),   S(  0,  13),   S( -7,  -9),   S(  2,   6),   S( -5,  -3),
            S( 10,   3),   S( 14,   2),   S( 19,   6),   S( 16,  39),   S( 10,   5),   S(  4,  20),   S(  2,  12),   S(  3,   7),
            S( 11,   9),   S( 10,  10),   S( -3,  16),   S( 21,   7),   S( 19,  25),   S(  8,  28),   S(  7,  10),   S(  4,  12),
            S(  2,   1),   S( -8, -10),   S( -6,  13),   S(  2,  -4),   S( 31,  31),   S(  9,   9),   S( -8,  -7),   S( -4, -10),
            S( -3,  -3),   S(  4,   9),   S(  3,  10),   S(  5,  -6),   S( 13,   1),   S( 21,  25),   S( 12,  25),   S( -1,  -2),
            S(  0,   4),   S( -1,  -4),   S(  0,  -4),   S(  0,  -5),   S(  2,   8),   S(  4, -10),   S( 15,   6),   S(  7,   5),

            /* bishops: bucket 13 */
            S( -5, -17),   S( -1,  -3),   S( -4, -13),   S( -5,  -9),   S( 17,  16),   S( -6, -10),   S(-16, -20),   S( -3,  -4),
            S( -5,  -2),   S( -8, -12),   S(  0,   4),   S( 17,   2),   S( -5, -12),   S(  3,  12),   S( -1,  -6),   S(  0,  -3),
            S(  8, -10),   S( 31,  19),   S( 10,   1),   S( 19,  31),   S(  2,  24),   S(  8,  20),   S( -8,   5),   S( -6,  -4),
            S( 25,  30),   S( 45,  16),   S( 20,  27),   S(-20,   8),   S( 15,  67),   S(  2,  12),   S(  8,   6),   S(  1,   9),
            S( 22,  23),   S( 16,  17),   S( 13,   0),   S(  7,  -9),   S(  9,  -6),   S( 11,  20),   S( 12,  16),   S(  3,  11),
            S(  7,   6),   S(  1,   7),   S( -3, -13),   S( 17,  -5),   S(  8,  13),   S( -5, -19),   S(  2,  -2),   S( 12,   1),
            S(  7,   8),   S( -9, -21),   S( -1, -18),   S(  4,   1),   S(  6,  18),   S( 18,  11),   S(  9,  -2),   S( 10,  13),
            S(  1,  -1),   S( -1,  -2),   S( -1,  11),   S(  4,   8),   S(  7,  13),   S(  3, -12),   S( 13,  -4),   S( 10, -10),

            /* bishops: bucket 14 */
            S(-12, -24),   S(  5,  21),   S( 14,  12),   S(  5,  22),   S(-12,  -1),   S( -7,  -5),   S( -4,   3),   S( -8,  13),
            S( -1,   1),   S( -1,  -5),   S(  2,  13),   S( -2,  -8),   S( 13,   5),   S(  3,  10),   S( -5,  19),   S(  4,  29),
            S(  1,  -4),   S( -2, -13),   S( -8, -14),   S( 19,  33),   S( 22,  46),   S( 11,  21),   S(  6,  39),   S(  3,  30),
            S(  5,  32),   S(  8, -12),   S( -4,  -1),   S(  2,  27),   S(  7,  15),   S( 19,   9),   S( 21,  16),   S(  9, -16),
            S( 10,   7),   S(  7,  16),   S( 11,   7),   S( 19,  10),   S( -3,  -1),   S(  5,  13),   S( 23,   0),   S( 16,  11),
            S(  2, -10),   S( 22,  38),   S(  3,   7),   S( 15,   4),   S( 10,  -1),   S( -7,   0),   S( -2,  19),   S( 17,   3),
            S( 17,  37),   S(  8,  10),   S( 13,  17),   S(  7,  10),   S(  8,  -3),   S(  3,  10),   S(  0, -11),   S(  2,   0),
            S( 14,   4),   S( 13,  18),   S(  4,   9),   S(  5,   1),   S( -4,  -4),   S(  1,  -5),   S(  7,  10),   S(  3,   3),

            /* bishops: bucket 15 */
            S( -3,  -6),   S( -1,  -2),   S( -8, -16),   S( -2,  -1),   S( -6, -20),   S( -4,  -8),   S( -5, -13),   S( -3,  -5),
            S(  9,  13),   S( -4, -11),   S(  5,   3),   S(  5,   5),   S(  7,  -1),   S( -1,  -2),   S( -1, -10),   S( -3,  -6),
            S(  3,  -6),   S(  3,   1),   S(  0,  -7),   S( 13,  16),   S( 13,  31),   S(  8,  26),   S( 16,  22),   S(  4,   5),
            S(  1,  -9),   S( 12,  12),   S( 11,  29),   S(-18,  -5),   S(  2,   8),   S( 16,   5),   S( 12,   1),   S(  9,  19),
            S( -1,  -8),   S( -1,  11),   S( -3,  21),   S( 20,  54),   S( 20,  24),   S( 12,  -1),   S(  7,   1),   S( -3,   1),
            S( -2,  19),   S(  6,  11),   S(  4,  25),   S(  7,  13),   S( 24,  20),   S(  7, -13),   S(  2,   8),   S(  1,  -2),
            S(  5,  -2),   S(  2,  18),   S(  9,  31),   S( 15,  18),   S( 10,  15),   S( -2,   7),   S(  0,  -8),   S(  0,   0),
            S(  3,  -3),   S( 11,  14),   S(  8,   0),   S(  9,  10),   S(  5,  16),   S(  1,  -2),   S(  4,  10),   S(  4,  -1),

            /* rooks: bucket 0 */
            S(-21,  11),   S(  7, -13),   S(-14,   0),   S(-10,  15),   S(-29,  58),   S(-18,  35),   S(-48,  61),   S(-54,  44),
            S(  1, -20),   S( -5,  16),   S(-32,  23),   S(  1,  28),   S(  1,  40),   S( -1,  21),   S(-14,   9),   S(-20,  41),
            S( 23, -31),   S( 12, -12),   S(-11,  12),   S( -2,  15),   S(-26,  53),   S( -9,  12),   S(-15,  35),   S( -1,  11),
            S( 11, -17),   S( 36,  -1),   S(-35,  34),   S( 20,  16),   S( 19,  40),   S(-10,  38),   S(-19,  46),   S(-13,  27),
            S( 52, -55),   S( 41,   4),   S( 15,  33),   S( 40,  25),   S( 48,  18),   S( 32,  62),   S( 38,  49),   S( 13,  56),
            S( 43, -29),   S( 53,  19),   S( 89, -12),   S(101,  28),   S( 34,  52),   S( 36,  58),   S( 13,  67),   S(-36,  80),
            S( 13,  12),   S( 52,  45),   S( 88,  32),   S( 67,  16),   S( 70,  48),   S( 25,  63),   S( -5,  77),   S(-11,  68),
            S( -7, -26),   S( 26,  22),   S( 19,  19),   S( 42,  -1),   S( 27,  46),   S( 45,  13),   S( 35,  17),   S( 54, -43),

            /* rooks: bucket 1 */
            S(-51,  47),   S(-18,   3),   S(-11,  11),   S(-40,  26),   S(-37,  42),   S(-42,  45),   S(-46,  62),   S(-71,  71),
            S(-37,  32),   S(-23,  -6),   S(-20,  18),   S(-24,  24),   S(-24,  16),   S(-36,  41),   S(-16,  16),   S(-24,  47),
            S(-21,  21),   S( -9,  -5),   S( -9,   3),   S(-23,  17),   S(-25,  19),   S(-43,  33),   S(-54,  59),   S(-17,  53),
            S(-31,  42),   S( -6,  13),   S( -9,  26),   S(-26,  16),   S(-31,  35),   S(-41,  61),   S(-22,  51),   S(-58,  80),
            S(-13,  47),   S( 13,  -2),   S( 29,  17),   S( 26,   7),   S(  4,  29),   S( -2,  79),   S( 12,  59),   S( -3,  81),
            S( 38,  39),   S( 47,   7),   S( 23,  17),   S(-19,  34),   S(  0,  27),   S(  7,  62),   S( 38,  45),   S( 13,  79),
            S(  9,  71),   S( 18,  11),   S(  0,  32),   S(  8,  14),   S( 42,  17),   S(  4,  55),   S( 28,  65),   S( 38,  81),
            S( 42,  -5),   S( -4,   0),   S(-15, -12),   S(-30, -16),   S( 13,  10),   S( 14,  19),   S( 35,  29),   S( 50,  37),

            /* rooks: bucket 2 */
            S(-54,  64),   S(-42,  54),   S(-34,  48),   S(-36,  19),   S(-25,  20),   S(-38,  27),   S(-29,  16),   S(-66,  61),
            S(-41,  55),   S(-40,  47),   S(-37,  51),   S(-43,  33),   S(-45,  38),   S(-38,  19),   S(-19,   5),   S(-45,  38),
            S(-33,  56),   S(-21,  47),   S(-30,  38),   S(-32,  37),   S(-31,  26),   S(-28,  23),   S(-14,   9),   S( -9,  30),
            S(-23,  66),   S(-15,  57),   S(-35,  58),   S(-58,  52),   S(-42,  45),   S(-15,  28),   S( -6,  27),   S(-14,  44),
            S( -4,  82),   S(-13,  78),   S( 12,  66),   S(-17,  48),   S(-35,  59),   S( 31,  26),   S(  4,  47),   S(  0,  69),
            S( 22,  87),   S( 23,  74),   S( 32,  68),   S(-32,  61),   S( 19,  26),   S( 25,  52),   S( 81,  12),   S( 40,  71),
            S( 51,  66),   S( -3,  78),   S( 14,  58),   S( 19,  32),   S(-17,  12),   S( 15,  67),   S(-44,  89),   S( 32,  73),
            S( 10,  46),   S( 13,  51),   S( 15,  39),   S(-55,  37),   S(-67,  10),   S(  2,   8),   S( -6,  28),   S(-14,  57),

            /* rooks: bucket 3 */
            S(-15,  69),   S( -9,  65),   S(-10,  88),   S( -8,  79),   S(  1,  46),   S(  3,  38),   S( 20,  11),   S( -8,   5),
            S(  6,  57),   S( -8,  71),   S( -7,  93),   S(  3,  85),   S(  3,  53),   S( 16,  14),   S( 50, -12),   S( 22,   7),
            S( 19,  53),   S(  0,  79),   S( -2,  79),   S(  2,  86),   S( 21,  41),   S(  6,  35),   S( 40,  12),   S( 34,   9),
            S( 10,  84),   S( -1, 104),   S( -9, 104),   S(  2,  91),   S(  3,  69),   S( 17,  53),   S( 38,  33),   S( 10,  30),
            S(  9, 105),   S( -7, 118),   S( 26, 109),   S( 26, 100),   S( 20,  87),   S( 43,  66),   S( 69,  37),   S( 42,  50),
            S(  9, 125),   S( 27, 108),   S( 39, 115),   S( 53,  96),   S(101,  50),   S(114,  35),   S( 79,  45),   S( 35,  44),
            S( 23, 117),   S( 18, 115),   S( 34, 122),   S( 34, 115),   S( 36,  99),   S( 90,  53),   S( 99,  97),   S(113,  62),
            S(117, -26),   S( 51,  45),   S( 19,  98),   S( 21,  81),   S( 18,  71),   S( 56,  63),   S( 30,  33),   S( 57,  11),

            /* rooks: bucket 4 */
            S(-26, -25),   S( 10, -14),   S(-33,  -4),   S(-37,  17),   S(-43,  11),   S(-29,  43),   S(-29,   0),   S(-75,  34),
            S(-30, -44),   S(-51,   1),   S(-23, -16),   S(  7, -28),   S( 28, -20),   S(  1,   2),   S(-16,  -7),   S(  7,  11),
            S(-19, -15),   S(-39, -18),   S(-40,  -1),   S( -6, -29),   S(-23,  -8),   S(-37,  17),   S(-16,  15),   S(-54,  17),
            S(-69, -27),   S(  7,   7),   S( -2, -14),   S(  7, -12),   S( 41,   3),   S( -2,  14),   S( -7,  -4),   S(-12,  10),
            S(-30, -38),   S( 21, -39),   S( 13,   2),   S( 46, -10),   S( 65,  -5),   S( 57,  25),   S( 19,  10),   S( 22,  23),
            S(-17, -38),   S(  9,  11),   S(  6,  -1),   S( 18,  13),   S( 30,  23),   S( 12,  13),   S( 33,  16),   S( 37,  35),
            S(-30, -19),   S( 29,  26),   S( 34,   0),   S( 50,  -6),   S( 61,  -3),   S( -9,  12),   S( 15, -13),   S( 26,   7),
            S(  6, -22),   S(  3,  16),   S( 24,  -5),   S( 29,  -5),   S( 55,   2),   S( 15,   1),   S(  9,   6),   S(  8,  13),

            /* rooks: bucket 5 */
            S( -3,  20),   S(-10,   6),   S(  3,  -6),   S( 28,  -7),   S(-13,  22),   S(  5,  28),   S(-15,  48),   S(-10,  29),
            S( 12,  -8),   S(-23, -11),   S( 42, -52),   S( 34, -20),   S(-16,   8),   S(-14,  14),   S(-28,  28),   S(  4,  24),
            S(-22,  25),   S( -6,   3),   S( -1, -15),   S( -4,  -9),   S(-22,   4),   S( 44, -11),   S(-33,  34),   S(-15,  18),
            S(-26,  32),   S(-13,  18),   S( 31, -18),   S( 16,   2),   S( 22,   3),   S( -7,  46),   S( 15,  32),   S(  9,  46),
            S( 31,  24),   S(  2,  17),   S( -4,  21),   S( -7,  -6),   S(-26,  27),   S( 62,  14),   S( 30,  32),   S( 52,  34),
            S( -9,  35),   S(-13,  19),   S(  1,   5),   S(-16, -14),   S( 14,  18),   S( 13,  31),   S( 61,  15),   S( 51,  30),
            S( 34,  13),   S( 24,   5),   S(-15,   7),   S( 26,  12),   S( 44,   4),   S( 44,  -5),   S( 82, -10),   S( 43,  18),
            S( 30,  32),   S( 12,  15),   S( 43,  -1),   S( -1,  19),   S( 41,  23),   S( 31,  33),   S( 42,  37),   S( 69,  38),

            /* rooks: bucket 6 */
            S(-24,  37),   S( -9,  26),   S(-12,  25),   S(-30,  24),   S( -6,  10),   S( 12,  -3),   S( 23,  -7),   S(-19,  18),
            S(-16,  17),   S( 31,   0),   S( 14,   6),   S( -3,   5),   S( 14, -14),   S(-16,  -4),   S(-29,   2),   S(  5,  11),
            S(-34,  32),   S( 21,  11),   S( 22,   5),   S( -2,  11),   S(-16,  11),   S( 39, -12),   S( -3,  -8),   S(  7,  -2),
            S(-28,  54),   S(  3,  39),   S( 18,  21),   S( 32,   7),   S(  7,   2),   S(  8,  10),   S( -7,  12),   S( 12,  37),
            S( -1,  52),   S( 61,  25),   S( 84,  23),   S( 37,   9),   S(  2,  -2),   S( 14,  18),   S( 40,   3),   S( 72,  11),
            S( 86,   8),   S( 87,  -3),   S( 77,   3),   S( 30,  -9),   S( -2, -16),   S( 19,  32),   S( 26,   2),   S( 53,  19),
            S( 53,  15),   S(120, -18),   S( 94, -16),   S( 69, -21),   S( 12,  -8),   S( 38,   5),   S( 43,   3),   S( 65,  -9),
            S( 92,  -9),   S( 66,  12),   S( 15,  35),   S( 57,   3),   S( 44,  10),   S( 25,  31),   S( 75,  16),   S( 71,  23),

            /* rooks: bucket 7 */
            S(-84,  28),   S(-64,  27),   S(-55,  28),   S(-46,  25),   S(-28,  -2),   S(-30, -18),   S(-36,   6),   S(-75, -16),
            S(-67,  25),   S(-16,  -1),   S(-34,  10),   S(-43,  21),   S(-22, -14),   S(-24, -11),   S(  1,  -4),   S(-11, -56),
            S(-67,  28),   S(-52,  22),   S(-15,   0),   S(-17,  10),   S(-33,   3),   S(-28,  12),   S( 40, -30),   S( -6, -46),
            S(-63,  33),   S( -5,  13),   S(  6,  10),   S( 70, -25),   S(  3,   3),   S( 48, -18),   S( 37,  -1),   S(  0, -14),
            S( 10,  23),   S( 39,  18),   S( 70,   5),   S( 92, -16),   S(132, -44),   S( 91, -47),   S( 73, -18),   S(-72, -34),
            S( 37,  12),   S( 40,  -3),   S(101,  -9),   S( 90, -26),   S( 71, -10),   S( 31,  11),   S( 19,  33),   S( -9, -32),
            S( 11,  -3),   S( 41, -15),   S( 74, -16),   S(110, -44),   S(109, -39),   S( 93, -35),   S( 38,  10),   S( -1, -25),
            S(-21, -14),   S( 14,   2),   S( 41,  -2),   S( 35,   0),   S( 49, -10),   S( 54,  -3),   S( 21,  18),   S(  7,  -9),

            /* rooks: bucket 8 */
            S(-23, -79),   S(-18, -38),   S(-13, -11),   S( 18,   7),   S(-24, -31),   S(-19,  -1),   S(-10, -34),   S(-21,   3),
            S(-35, -77),   S(-16, -45),   S(-25,   5),   S(-27, -62),   S(-26, -37),   S(-16, -20),   S(-10,  -6),   S(-39, -33),
            S( -1, -14),   S( -5, -13),   S(  9,  -2),   S(-12,  19),   S( -9,  48),   S( 12,  27),   S(  2,  47),   S(-18,   4),
            S( -6, -21),   S( -2,   2),   S( -1,   2),   S( 14,  27),   S(  1,  40),   S( 30,  42),   S( -1,  19),   S(-10, -12),
            S(-12, -38),   S( 11,  25),   S(  7,  23),   S( 16,  42),   S(  8,  24),   S( -3,   4),   S( 12,  44),   S(  1,  21),
            S(-27,  11),   S(  2,  14),   S(-16,  12),   S( -4, -14),   S(  5,  32),   S(-15,  28),   S(  0,   0),   S(  3,  18),
            S(  0,  35),   S(  2,  24),   S(  2,   7),   S( 22,  12),   S( 14,   4),   S( 10,  22),   S(  6,  16),   S(  2,  35),
            S(-15,  15),   S(  1,  13),   S(-20,  33),   S( 37,  48),   S( -6,  15),   S( 11,  34),   S(  1,  17),   S(  7,  32),

            /* rooks: bucket 9 */
            S(-28, -64),   S(-14, -61),   S(-14, -98),   S(-21, -43),   S(-21, -43),   S(  3, -30),   S( -6, -23),   S( -4, -33),
            S(-60, -42),   S(-34, -63),   S(-31, -61),   S(-44, -44),   S(-42, -46),   S(-26,  10),   S(-23, -52),   S(-31, -29),
            S(-16, -10),   S(-28, -11),   S( -2,  -9),   S(-11, -34),   S(-14, -12),   S(  3,  21),   S( -1,   6),   S(  0,  13),
            S( -6,   7),   S(  3,  -1),   S(  1,   2),   S( -2,   7),   S(-14, -29),   S(  2,   3),   S( -9,  -3),   S(  5, -27),
            S( -4,   4),   S( -9,  -6),   S(-11, -44),   S( -9,   6),   S(-24, -11),   S(-12,   6),   S(-12, -15),   S( -7, -11),
            S( -4,   6),   S(-31, -11),   S(-15, -19),   S( -2,  18),   S( -4,   2),   S( -3,  10),   S( -3,   0),   S(-11,   8),
            S(  9,  25),   S(  7,   2),   S(  3, -38),   S(  1,   8),   S(  7, -17),   S( 22,   1),   S(  7,   2),   S( -2, -21),
            S(-12,   8),   S(-19,  27),   S( -9,  11),   S( -5,  29),   S(-11,  29),   S(  7,  49),   S(  5,   9),   S( 14,  16),

            /* rooks: bucket 10 */
            S(-18, -30),   S(-53, -11),   S(-32, -38),   S(-13, -48),   S(-20, -46),   S( -5, -78),   S(  1, -63),   S(-13, -40),
            S(-45, -10),   S(-31, -30),   S(-42, -20),   S(-47, -43),   S(-47, -43),   S(-29, -43),   S(-18, -27),   S(-46, -70),
            S(-12, -13),   S(-28, -16),   S(-34, -15),   S(-44, -42),   S(-15, -19),   S( -2, -16),   S(-16, -27),   S(-19, -13),
            S(-26, -10),   S(-35, -36),   S( -7, -34),   S( -9,   5),   S(  4,   4),   S(  3,  11),   S(-11, -29),   S( -1, -34),
            S(  8,  -9),   S(  5, -12),   S(-15, -12),   S(-15, -29),   S(  3,  14),   S( -6,   6),   S(-11, -18),   S(-10, -32),
            S( -8,   3),   S( 14,  -1),   S( -1, -13),   S( -2, -25),   S( -2,  -6),   S( -9,  -6),   S(-22, -26),   S(  3, -14),
            S( -7, -19),   S(  9, -35),   S(  0, -24),   S( -4, -15),   S( 11, -22),   S( -8, -13),   S(-15, -31),   S( -2, -19),
            S( -3, -12),   S( 10,  18),   S(  3,  30),   S(-10,  11),   S(-10,  29),   S(-27,   2),   S(-29,  13),   S(  2,   8),

            /* rooks: bucket 11 */
            S(-60, -17),   S(-38,  -3),   S(-48, -12),   S(-26, -10),   S(-46, -12),   S(-29, -14),   S(-20, -34),   S(-42, -63),
            S(-18, -11),   S(-25, -18),   S(-58, -10),   S(-56, -18),   S(-16, -19),   S(-17,  -6),   S(-28, -27),   S(-47, -60),
            S(-35,  25),   S(-23,  12),   S( -8,  30),   S(-21,  15),   S(  1, -20),   S(-13,  -2),   S(  5, -19),   S(-15,  12),
            S(-24,  -8),   S(-14, -17),   S(-11,  10),   S(  8,  15),   S( 18,  14),   S(-21, -32),   S(  6,  17),   S( -8, -21),
            S( -7,  -9),   S(  8,  -5),   S(  5,   6),   S(  3,   6),   S( 35,  -7),   S( -2,   0),   S( 16,  38),   S(-16, -41),
            S(  4, -19),   S( -9,  -8),   S( 15, -13),   S( 20,  -5),   S(-10, -15),   S(  0,   7),   S(  2,  34),   S( -6,  -7),
            S( -4,  -1),   S(-20, -36),   S( -2,  -8),   S(  0,  -5),   S( 10,  -4),   S(  2,   8),   S(  1,  14),   S(-13,  -7),
            S( -5,  -5),   S( 17,  24),   S(  4,  17),   S( 19,  16),   S( -8,   4),   S( -4,  24),   S( 12,  15),   S(-19,  23),

            /* rooks: bucket 12 */
            S(-35, -98),   S( -9, -13),   S(-21, -56),   S(-19, -37),   S(-12, -24),   S(  8,  -9),   S(-16, -38),   S(-19, -40),
            S(  2,   1),   S(  1,   4),   S(  8,  20),   S(  3,  13),   S(  7,   8),   S(  9,  -8),   S(  6,   9),   S(-18, -26),
            S( -6, -12),   S(  6,  35),   S( 11,  22),   S( 24,  25),   S(  6,  -5),   S( 15,  27),   S(  7,  32),   S( -3,  27),
            S(  7,  22),   S(  4,   5),   S( 15,  33),   S( 12,  21),   S( 12,   8),   S(  6,   8),   S(  6,  19),   S( -3,   5),
            S( 11,  18),   S( 13,  31),   S(  9,  47),   S(  2,  -1),   S(  8,  24),   S( -3, -16),   S(  5,  13),   S(  5,   9),
            S( -3,   0),   S( -5,  -5),   S(  0,  17),   S( -5,   3),   S(  7,  22),   S( -1, -22),   S(  9,  24),   S(  4,   7),
            S(-16, -10),   S(-11,  19),   S(  8,  39),   S( -1,  19),   S( -3,  -3),   S( 12,  14),   S(  3,  20),   S(  1,  20),
            S(  4,   1),   S(-10,  31),   S(  5,  31),   S( 13,  21),   S(  2,   5),   S(  1,  17),   S(  2,   8),   S(  2,  12),

            /* rooks: bucket 13 */
            S(-26, -20),   S(-28, -50),   S(-26, -51),   S(-19, -36),   S(-30, -50),   S( -5,  -2),   S(-27, -46),   S(-24, -36),
            S(-14, -10),   S( -8, -18),   S(  2,   7),   S( -2,  -3),   S( 17,  37),   S(  4,  14),   S(  8,   3),   S(-12, -12),
            S(-13,   2),   S(-13,   9),   S( -5,  -8),   S(  6,  10),   S(  6,  28),   S( 13,  -2),   S( 11,  44),   S(-12, -28),
            S(  9,  17),   S( -2,   9),   S( -3,   9),   S(  6,  18),   S(  9,  23),   S(  0,   8),   S(  5,  14),   S(  2,  21),
            S(  6,  19),   S(  3,  -9),   S( -5, -22),   S(  2,   6),   S( -3,  23),   S(  0,  -4),   S(  5,   5),   S( -1,  -5),
            S(  0,  11),   S( -4,  -5),   S(-10, -10),   S(-14,  -3),   S(-12, -13),   S(  2,  -5),   S( -8,   4),   S(  1,  -1),
            S(  3, -12),   S(  9,   4),   S(-10, -31),   S(  2,  16),   S( -8,  -4),   S(  6,   8),   S(  1,   1),   S( -1, -16),
            S(  2,  18),   S(-11,  12),   S( -5,   3),   S(  9,  23),   S( -3,  14),   S(  7,  22),   S(  0,  20),   S(  3,  -1),

            /* rooks: bucket 14 */
            S( -6, -25),   S(-32, -26),   S(-19, -16),   S(-21, -55),   S(-14, -40),   S( -8, -23),   S(-33, -61),   S(-26, -32),
            S( -7,  27),   S(  4,  28),   S(  6,  12),   S( -1, -17),   S(  0,  -7),   S( -3,  -3),   S( -2,   5),   S( -5,  -2),
            S(  4,  31),   S( -2,  30),   S(  0,   5),   S(  1,   5),   S(  3,   8),   S(  0,  -4),   S(  2,  24),   S(-19, -44),
            S( -4,  14),   S( 16,  22),   S(  6,  21),   S( 11,   5),   S( -9,  -5),   S(  1,  -9),   S(  8,  14),   S(-12, -16),
            S(  8,  14),   S( 19,  17),   S( -2,  -6),   S(  1,   6),   S(  3, -13),   S( 18,  29),   S( -1,   2),   S( -3, -18),
            S(  5,   8),   S(  6,   9),   S(  7,  16),   S(  2,   5),   S( -3,   6),   S(-15,   3),   S( -9,  -8),   S( -6,  -8),
            S( -7, -14),   S(  8,  12),   S( -8, -21),   S(-18, -33),   S( -5,   6),   S(  1,  -1),   S(-12, -14),   S( -8,  -8),
            S( -1,  -6),   S(  3,   4),   S( -4, -18),   S(  6,  -7),   S(-10, -16),   S(-16, -43),   S(  3,  -5),   S(  1,  27),

            /* rooks: bucket 15 */
            S(-25, -45),   S(-17, -48),   S(-39, -48),   S(-25, -50),   S( -3, -22),   S(-14, -19),   S( -3,  -7),   S(-21, -53),
            S(  6,  30),   S(-11,   1),   S(-12,  -7),   S( -6,  -8),   S( -6, -17),   S(  4,   0),   S(  7,  11),   S(  3,   5),
            S(  5,  10),   S( -6, -12),   S( 12,  24),   S(  7,   1),   S(  6,   0),   S( -7, -12),   S(  6,  25),   S(  2,   6),
            S(  2,  10),   S( -1,  -6),   S( 18,  34),   S( -4, -12),   S(  4,  18),   S(  2,   6),   S(  6,  16),   S(  3, -11),
            S(  6,  12),   S(  5,   7),   S(  7,  -9),   S(  2,   9),   S(  6,  13),   S(  4,   3),   S( -2,  27),   S(  4, -10),
            S(  6,  13),   S(  7,  -3),   S(  8,  -3),   S(  3,   2),   S( -5, -14),   S( -3,  37),   S(  2,  23),   S(  4,   3),
            S(  4,  -5),   S( -3,   3),   S(  8,  17),   S(  3,   9),   S(  3,  14),   S(  5,  16),   S(-13,  12),   S( -9, -28),
            S(  0,  19),   S( -1,  21),   S(  8,  19),   S(  1,  24),   S( -1,   3),   S( -6, -24),   S( -4,  15),   S(-15, -10),

            /* queens: bucket 0 */
            S( -2,  -7),   S(-22, -46),   S(-32, -54),   S(  0, -98),   S( -6, -53),   S( 13, -59),   S(-52, -27),   S(-13, -10),
            S(-15, -29),   S( 15, -76),   S(  5, -69),   S( -8, -18),   S(  4, -18),   S( -5, -35),   S(-22, -26),   S(-34, -10),
            S(  0,   6),   S( -1, -22),   S( 29, -50),   S( -9,   8),   S( -3,  24),   S(  1,   0),   S(-29,  -1),   S(-73, -40),
            S(-19,  21),   S( 17, -22),   S( -8,  21),   S( -6,  68),   S( -4,  64),   S(-19,  38),   S(-37,  29),   S(-15, -24),
            S(-23, -20),   S(  3,  64),   S(  2,  33),   S(  0,  49),   S(  6,  71),   S(-15, 108),   S(-54,  71),   S(-41,   4),
            S(-17,   5),   S( 16,  33),   S( 13,  37),   S(-18,  71),   S(-16,  68),   S(-54,  98),   S(-60,  27),   S(-41,   7),
            S(  0,   0),   S(  0,   0),   S( 17,   2),   S(-31,  33),   S(-29,  29),   S(-58,  85),   S(-82,  65),   S(-95,  27),
            S(  0,   0),   S(  0,   0),   S(  2, -10),   S(-18, -13),   S(-30,  25),   S(-31,  10),   S(-46,  -4),   S(-58, -23),

            /* queens: bucket 1 */
            S( 22,   0),   S( 12,   0),   S( 17, -44),   S( 31, -85),   S( 40, -42),   S( 17, -25),   S( 17,  -4),   S(  4,  17),
            S(-18,  35),   S( 26,  18),   S( 40, -34),   S( 30,   5),   S( 43,  14),   S(  7,  20),   S(-14,  40),   S(-14,   9),
            S( 50,  -2),   S( 29,   2),   S( 22,  32),   S( 18,  74),   S( -1,  80),   S( 36,  47),   S(  1,  41),   S( 19,  -8),
            S( 44,   3),   S( 19,  42),   S( 22,  48),   S( 43,  67),   S( 22,  83),   S( 10,  60),   S( 11,  40),   S( -5,  59),
            S( 47,   2),   S( 55,  13),   S( 51,  34),   S( 22,  30),   S( 47,  66),   S( 32,  32),   S( -6,  76),   S(  8,  90),
            S( 63,  -2),   S( 95,  11),   S( 85,  43),   S( 70,  53),   S( 43,  39),   S( 19,  64),   S( 46,  54),   S(  4,  56),
            S(102, -26),   S( 50, -21),   S(  0,   0),   S(  0,   0),   S( -1,  39),   S(-11,  20),   S(-10,  56),   S(-36,  38),
            S( 70, -13),   S( 42, -21),   S(  0,   0),   S(  0,   0),   S( 10,  16),   S( 31,  21),   S( 78,   0),   S(-15,  36),

            /* queens: bucket 2 */
            S( 38, -12),   S( 32,  12),   S( 34,  20),   S( 45, -25),   S( 47, -30),   S( 34, -20),   S(  3, -19),   S( 38,  31),
            S( 27,   5),   S( 12,  52),   S( 39,  27),   S( 43,  36),   S( 54,   8),   S( 22,  28),   S( 27,  20),   S( 19,  49),
            S( 42,  13),   S( 31,  44),   S( 23, 104),   S( 17,  84),   S( 27,  80),   S( 26,  75),   S( 37,  47),   S( 32,  64),
            S(  3,  74),   S( 26,  85),   S( 27,  81),   S( 15, 123),   S( 35,  93),   S( 26,  93),   S( 38,  63),   S( 37,  83),
            S( 10,  84),   S( -8,  84),   S(  6,  97),   S( 38,  73),   S( 26,  91),   S( 93,  38),   S( 71,  53),   S( 67,  58),
            S( -9,  85),   S(  0,  81),   S(  2,  80),   S( 69,  34),   S( 29,  53),   S( 97,  69),   S(101,  42),   S( 51,  98),
            S(  1,  50),   S( -1,  40),   S( -4,  67),   S( 42,  28),   S(  0,   0),   S(  0,   0),   S( 27,  68),   S( 38,  67),
            S(  1,  35),   S( 34,  -2),   S( 38, -10),   S( 16,  35),   S(  0,   0),   S(  0,   0),   S( 38,  30),   S(  7,  50),

            /* queens: bucket 3 */
            S(-42,  32),   S(-27,  38),   S(-21,  37),   S(-13,  44),   S(-26,  32),   S(-13, -16),   S(-12, -38),   S(-36,  21),
            S(-56,  56),   S(-36,  49),   S(-22,  66),   S(-15,  84),   S(-14,  73),   S(-14,  36),   S( 17, -13),   S( 16, -27),
            S(-49,  80),   S(-37,  89),   S(-29, 114),   S(-38, 145),   S(-27, 124),   S(-20,  94),   S( -6,  56),   S( -9,  22),
            S(-41,  82),   S(-57, 142),   S(-48, 162),   S(-33, 175),   S(-34, 166),   S(-18, 101),   S( -1,  81),   S(-12,  65),
            S(-55, 120),   S(-44, 157),   S(-46, 178),   S(-41, 195),   S(-20, 159),   S(  1, 132),   S(-13, 124),   S(-15,  74),
            S(-62, 113),   S(-57, 159),   S(-55, 182),   S(-50, 195),   S(-48, 169),   S( -2, 105),   S(-26, 124),   S(-26, 106),
            S(-95, 130),   S(-92, 152),   S(-72, 186),   S(-60, 160),   S(-67, 162),   S(-15,  80),   S(  0,   0),   S(  0,   0),
            S(-124, 143),  S(-77, 105),   S(-64, 105),   S(-64, 111),   S(-61, 102),   S(-24,  57),   S(  0,   0),   S(  0,   0),

            /* queens: bucket 4 */
            S(-30,  -2),   S(-43, -34),   S( -7,   1),   S( -7, -17),   S( -6,  -6),   S( -6,  12),   S(-32, -24),   S( 14,  21),
            S( -1, -11),   S( -8,   6),   S( -3,   1),   S(-10, -13),   S(-39,  18),   S(-17,  12),   S(-44, -10),   S( -2, -17),
            S(  6,  17),   S( 20, -30),   S( 13, -17),   S( 15,   8),   S( 44,  10),   S( 19,  20),   S(-14, -17),   S( 34,  24),
            S(-17, -24),   S( 14, -21),   S(  0,  -1),   S(-10,  18),   S( 43,  31),   S(  8,  58),   S(-23,   6),   S(-14,  17),
            S(  0,   0),   S(  0,   0),   S( 13,  -9),   S( 50,  34),   S( 24,  56),   S( 33,  52),   S( 13,  16),   S( 16,  22),
            S(  0,   0),   S(  0,   0),   S( 16,  10),   S( 29,  17),   S( 42,  48),   S( 33,  49),   S( 22,  26),   S(  2,   6),
            S( 11,  -6),   S( 18,   8),   S( 60,  37),   S( 53,  36),   S( 54,  16),   S( 23,  31),   S(  7,  25),   S( -9,  24),
            S( 16, -12),   S(-20, -33),   S( 21,   6),   S( 46,  22),   S( 16,   8),   S(  9,  24),   S(  2,   5),   S( 21,   8),

            /* queens: bucket 5 */
            S( 38,  25),   S( 28,  12),   S( 20,   9),   S(  1,  27),   S( 37,  -5),   S( 42,  48),   S( 10,  -1),   S( 20,   3),
            S( 23,  18),   S( 17,   1),   S( 15,  -1),   S( 14,  15),   S( 13,  43),   S( -7,  -8),   S( 29,  15),   S( 11,   4),
            S( 20,   5),   S( 43,  -2),   S( 22,  -1),   S(  4,  16),   S( 18,   8),   S( 32,  18),   S( 28,  42),   S( 21,  17),
            S(  6, -32),   S( 31,   1),   S( 18, -20),   S( 25,  10),   S( 55,   3),   S( 32,  15),   S( 35,  49),   S(  6,  32),
            S( 36,  -9),   S( 20, -42),   S(  0,   0),   S(  0,   0),   S(  1,   6),   S( 26,  14),   S( 42,  56),   S( 16,  36),
            S( 32,  13),   S( 31,   3),   S(  0,   0),   S(  0,   0),   S( 27,  18),   S( 58,  32),   S( 46,  38),   S( 53,  41),
            S( 60,   1),   S( 62,   7),   S( 50,  40),   S( 25,  26),   S( 41,  15),   S( 82,  43),   S( 61,  58),   S( 52,  33),
            S( 40,  29),   S( 48,  11),   S( 58,  17),   S( 38,  -6),   S( 53,  21),   S( 64,  40),   S( 69,  51),   S( 61,  33),

            /* queens: bucket 6 */
            S( 52,  52),   S(  5,   8),   S( 41,  19),   S( 36,  24),   S( 28,  15),   S(  0,   3),   S(  4,  12),   S(  7,  19),
            S( 32,  21),   S( 35,  30),   S( 72,  41),   S( 59,  28),   S( 42,  23),   S( 19,  17),   S(-13,  29),   S( 25,  33),
            S( -2,  48),   S( 44,  38),   S( 32,  39),   S( 52,  14),   S( 27,  11),   S( 43,   1),   S( 58,  27),   S( 69,  61),
            S( 28,  37),   S(  9,  30),   S( 49,  14),   S( 84,  13),   S( 30, -11),   S( 35,   6),   S( 71,   3),   S( 95,  45),
            S( 33,  55),   S( 34,  38),   S( 51,  36),   S( 41,  28),   S(  0,   0),   S(  0,   0),   S( 58,  18),   S( 97,  49),
            S( 44,  49),   S( 58,  46),   S( 47,  55),   S( 24,   5),   S(  0,   0),   S(  0,   0),   S( 67,  41),   S(103,  42),
            S( 59,  36),   S( 19,  28),   S( 61,  15),   S( 50,  15),   S( 41,  37),   S( 66,  48),   S(118,  22),   S(125,   8),
            S( 41,  42),   S( 67,  27),   S( 72,  20),   S( 77,  37),   S( 94,   8),   S( 91,  11),   S(102,  13),   S( 97,  31),

            /* queens: bucket 7 */
            S( -6,  26),   S( -5,   2),   S(-20,  24),   S( -1,  25),   S( 15,   4),   S(-10,   6),   S(  0,  15),   S(-14,  -9),
            S( -3,  26),   S(-40,  28),   S(  4,  48),   S( -7,  75),   S( -7,  42),   S(  6,  25),   S(  8,   3),   S(-27,  -4),
            S(  7,  24),   S( -9,  35),   S(-10,  88),   S( 43,  44),   S( 46,  32),   S( 21,  12),   S( 45, -24),   S( 46,  -6),
            S(-10,  21),   S( 22,  43),   S( 21,  70),   S( 46,  73),   S( 77,  46),   S( 58,  -3),   S( 69, -32),   S( 30,  -7),
            S( 20,  23),   S( -6,  58),   S( 22, 102),   S( 51,  82),   S( 80,  20),   S( 60,  -5),   S(  0,   0),   S(  0,   0),
            S(  5,  47),   S( -6,  89),   S( 16,  88),   S( -1,  87),   S( 53,  37),   S( 85,  48),   S(  0,   0),   S(  0,   0),
            S(-33,  62),   S(-19,  43),   S( 14,  59),   S( 34,  61),   S( 50,  41),   S( 67,  18),   S( 66,  22),   S( 57,  25),
            S( 35,  20),   S( 43,  33),   S( 50,  59),   S( 47,  27),   S( 49,  45),   S( 21,   6),   S(-18,   6),   S( 52,  -9),

            /* queens: bucket 8 */
            S(-19, -37),   S(  1, -22),   S(-16, -42),   S( -3,  -7),   S(-16, -30),   S(  9,  -3),   S( -1, -12),   S(  1,   4),
            S(-21, -36),   S( -6, -15),   S(  2, -14),   S( -6, -11),   S(  9,  -2),   S( -4, -10),   S( -3,   2),   S( -1,   0),
            S(  0,   0),   S(  0,   0),   S( -1, -16),   S(-11, -46),   S(  7,   3),   S(  6,  -7),   S( -7,  -9),   S(  3,   5),
            S(  0,   0),   S(  0,   0),   S(  1,   0),   S( -4, -13),   S( -2,   1),   S(  4,  -2),   S( 12,  19),   S(  7,   3),
            S( -3, -12),   S(  8,  12),   S(  7,  -1),   S( 11,  -6),   S(  8, -10),   S( 14,  12),   S( 13,  11),   S(-10, -10),
            S(  1, -13),   S(  4, -16),   S( 14,  15),   S(  3, -19),   S( 13,   8),   S( 28,  35),   S(  8,  -5),   S( -2,  -5),
            S(-16, -36),   S(  2, -10),   S( 14,  11),   S( 25,  39),   S( 12,  12),   S( 18,  42),   S(  4,   7),   S(  5,   0),
            S(  2,   1),   S(  5,  -5),   S( 15,  10),   S(  9,  -2),   S( 18,  19),   S( -2,  -4),   S(  4,  10),   S(-16, -28),

            /* queens: bucket 9 */
            S(  9,  -9),   S(-19, -33),   S(-16, -35),   S( 11, -10),   S( -8, -36),   S( -2,  -8),   S( -4,  -9),   S( -1, -14),
            S( -2,  -7),   S(-13, -22),   S(-11, -27),   S(  0, -16),   S(-25, -53),   S(-13, -31),   S(  7,  -2),   S(  1,  -9),
            S(-17, -45),   S(-14, -26),   S(  0,   0),   S(  0,   0),   S(  3,  -9),   S(  8, -10),   S( -6,  -9),   S(  5,  -4),
            S(  1,  -8),   S(-12, -32),   S(  0,   0),   S(  0,   0),   S( -1,  -5),   S(  9,   0),   S( 10,   9),   S( -3,   2),
            S( -8, -25),   S(  0, -14),   S( -1,  -7),   S(-11, -10),   S( -6, -28),   S( 11,  17),   S(  6,  -7),   S(  0, -15),
            S( 11,  11),   S( -2, -27),   S(  3, -11),   S( -3, -18),   S( -1, -11),   S(  6,   7),   S( -1, -10),   S( -2, -13),
            S(  9,   7),   S(  9,  -4),   S( -5,  -2),   S(  1,   9),   S( 24,  28),   S( 25,  29),   S(  8,  21),   S(  7, -12),
            S( 17, -10),   S( 26,  18),   S( -1,  -7),   S( 20,  14),   S( 21,  18),   S(  7,  14),   S(  1, -18),   S( 14,   2),

            /* queens: bucket 10 */
            S( 16,  10),   S( 13,   9),   S( -1,  -9),   S( -6, -26),   S(-10, -30),   S(-11, -20),   S( -5, -27),   S( -5, -15),
            S(  6,   4),   S(-13, -20),   S( -8, -25),   S(-19, -52),   S( -5, -11),   S(  9,  -1),   S(-13, -27),   S( -6,  -6),
            S( -2,   1),   S(  2,   3),   S( -3,  -5),   S( -8, -19),   S(  0,   0),   S(  0,   0),   S(  1,  -5),   S(-13, -23),
            S( -4, -10),   S(  3,   4),   S(  3,   3),   S(  8,   1),   S(  0,   0),   S(  0,   0),   S( -6, -15),   S( -1, -17),
            S( 12,  16),   S( 15,   6),   S(  3,  -4),   S( 27,  31),   S( -1,   1),   S( -2,  -1),   S(  1, -11),   S( 10, -25),
            S( -5,  -8),   S(  6,   7),   S( 24,  29),   S( 10,  12),   S( 14,  14),   S( 14,  22),   S( 16,  10),   S( -3, -22),
            S(  8,   8),   S( 18,  26),   S( 19,  26),   S( 23,  20),   S( 11,  18),   S( 24,  12),   S( 15,  10),   S(  6,  -5),
            S(-12, -31),   S(  4,   6),   S( 22,   7),   S( -5,   0),   S( 13,  14),   S(  3,   3),   S( 15,  10),   S( 10,  -7),

            /* queens: bucket 11 */
            S(-10,  -3),   S( -3,  -1),   S( -8, -10),   S(-18, -18),   S( -5, -13),   S(-23, -33),   S( -8, -32),   S(-10, -16),
            S( -5,  -1),   S(  1,   8),   S(-22, -10),   S( -7,   4),   S( 20,  -1),   S(-11, -28),   S(  6,  -5),   S( -6, -13),
            S(  3,   7),   S(  5,   0),   S(-20,  12),   S( -2,   2),   S( -5, -21),   S(-25, -31),   S(  0,   0),   S(  0,   0),
            S( -1,   1),   S( -7,   9),   S( -2,  11),   S(  0,   3),   S(  0,  -7),   S( -3,   6),   S(  0,   0),   S(  0,   0),
            S(  2,  12),   S( 16,  15),   S( 17,  24),   S(  4,  22),   S( 41,  47),   S( 16,  27),   S(  9,   0),   S(-11, -29),
            S(  2,   4),   S(  1,   0),   S(  0,  14),   S( 13,  29),   S( 15,  20),   S(  1,   5),   S(  4,  -9),   S(  3, -24),
            S(  4,   4),   S( 10,  12),   S( 16,  25),   S(  2,  12),   S( 20,  59),   S( 16,  13),   S(  7,   5),   S( 10,  -3),
            S(-16, -57),   S( 11,  14),   S( -5,  -5),   S(  7,  39),   S( 18,  33),   S( 11,   1),   S( -6,  -1),   S( 11,   1),

            /* queens: bucket 12 */
            S(  0,   0),   S(  0,   0),   S(  1,   2),   S(-17, -19),   S( -7,  -5),   S(-12, -19),   S( -1,  -3),   S( -2,  -3),
            S(  0,   0),   S(  0,   0),   S(  5,   3),   S( -9, -17),   S( -8,  -8),   S(-11, -23),   S( -8, -16),   S(  2,   0),
            S( -6,  -9),   S(  5,   7),   S( -6,  -7),   S(-11, -34),   S( 16,  30),   S( -1,  12),   S( -1,  -7),   S(  8,   9),
            S( -9, -18),   S(  6,   3),   S(  6,  13),   S(  4,  13),   S(  2,   3),   S( -1,   9),   S( -3,  -3),   S( -3,  -9),
            S(-17, -28),   S(  3,   9),   S(  7,   3),   S(  6,   5),   S(  6,  27),   S( -5, -20),   S( -8, -17),   S( -2,  -2),
            S(  2,  -5),   S( -4, -11),   S(  0, -13),   S(  6,   9),   S( -5, -10),   S( -9,  -1),   S(-11, -10),   S( -2,  -7),
            S( -8, -10),   S(  4,   7),   S( -6, -10),   S( 13,  10),   S(  0,   0),   S( -9, -14),   S(  1,   1),   S( -7, -25),
            S(  7,  12),   S(  0,  -3),   S(  2,  -6),   S(  0,   2),   S( -6,  -7),   S(-13, -13),   S( -4,  11),   S( -8, -13),

            /* queens: bucket 13 */
            S(-23, -35),   S(-15, -29),   S(  0,   0),   S(  0,   0),   S(-17, -28),   S(-13, -34),   S( -1,  -2),   S( -4,  -9),
            S(-16, -45),   S( -7, -16),   S(  0,   0),   S(  0,   0),   S(-16, -35),   S(-22, -43),   S(-12, -21),   S( -4,  -6),
            S(-22, -38),   S( -5, -14),   S( -4,  -4),   S( -2, -14),   S(-22, -41),   S(-11, -16),   S( -8,  -7),   S( -1,  -5),
            S( -8, -18),   S(-19, -29),   S(  0,  -7),   S( -7, -19),   S( 10,   5),   S( 18,  32),   S( -4, -15),   S( -8, -11),
            S(  6,  -7),   S(  3, -20),   S( -7, -20),   S( 12,  23),   S( -7, -10),   S( -1, -16),   S( -2,  -5),   S(  2, -11),
            S(  0,  -1),   S(-13, -18),   S(  5,   3),   S( 11,  22),   S(  0, -11),   S( -5,  -7),   S(-12, -23),   S( -9, -22),
            S(  0,   0),   S( -3,  -9),   S( 11,  24),   S( -2,  -2),   S(  3,   2),   S(  8,   0),   S(-13, -25),   S( -7, -11),
            S( -7,  -6),   S( -2,  -7),   S( -5, -13),   S(  1,  -6),   S(  4,  -2),   S( -1,  -3),   S(  0,  -8),   S(-12, -20),

            /* queens: bucket 14 */
            S( -6, -15),   S( -1, -10),   S(-10, -19),   S( -2,  -5),   S(  0,   0),   S(  0,   0),   S( -4,  -7),   S( -9, -23),
            S( -7, -24),   S(-26, -47),   S(-11, -23),   S( -3, -14),   S(  0,   0),   S(  0,   0),   S( -8, -22),   S(-10, -31),
            S( -9, -18),   S( -6, -23),   S( -7, -21),   S(-14, -25),   S( -3,  -4),   S(  1,   3),   S(-11, -17),   S(-17, -33),
            S( -9, -12),   S( -2,  -1),   S(  0,   0),   S(-15, -20),   S( -9, -16),   S(-20, -29),   S( -2, -21),   S(  1,   1),
            S( -6, -12),   S( -5, -12),   S( -4, -15),   S(  6,  10),   S(  5,  19),   S(-10, -25),   S( -9,  -3),   S( -1,  -4),
            S( -5, -12),   S(  3,  -4),   S(-12, -20),   S(-12, -22),   S(  6,  10),   S(  2,   5),   S( -1,  -5),   S( -9, -11),
            S(-10, -16),   S( -2,  -9),   S(  0,   0),   S(  3,   6),   S(  3,   4),   S(  4,   5),   S( -8, -21),   S( -3,  -9),
            S(-10, -17),   S(  4,  -5),   S(-10, -15),   S( -3,  -8),   S(  3,   1),   S( -3,  -3),   S( -4,  -3),   S(  2,  -8),

            /* queens: bucket 15 */
            S(  1,   3),   S( -6, -18),   S(  5,   2),   S(-11, -17),   S(  4,   7),   S(-10, -11),   S(  0,   0),   S(  0,   0),
            S( -4,  -4),   S(  1,   6),   S(-13, -17),   S( -8, -17),   S(  0,  -6),   S(  2,   7),   S(  0,   0),   S(  0,   0),
            S( -1,  -1),   S(  0,  -1),   S(-12,  -4),   S( -6,  -5),   S(-10, -22),   S(  4,   4),   S( -1,   2),   S( -1,  -4),
            S( -2,  -5),   S(-10, -15),   S( -3,  -6),   S(  2,   8),   S( 10,  29),   S(  6,  27),   S( -3,   5),   S( -4, -15),
            S(  1,   2),   S(  1,   0),   S( -3,  -9),   S( -1,  -1),   S( 11,  51),   S(  4,  21),   S(  4,  12),   S( -5, -15),
            S( -1,  -3),   S( -3,  -2),   S( -3,  -8),   S( -6,  -2),   S( -1,   4),   S( -9,  -8),   S(  2,  12),   S( -7,  -5),
            S( -5, -11),   S(  0,   0),   S( -4,   4),   S(  4,   4),   S( -7,  -9),   S(  1,   6),   S(  5,  10),   S( -5, -10),
            S( -8, -18),   S(-13, -30),   S( -1, -10),   S(  2,   2),   S(-13,  -3),   S( -3,   0),   S(  1,   0),   S( -3,   4),

            /* kings: bucket 0 */
            S( -9, -20),   S( 29,  -9),   S( 16,  -3),   S(-26,  15),   S(-11,  14),   S( 30, -25),   S(  2,   3),   S(  9, -49),
            S(-16,  30),   S( -4,   1),   S( -1,   2),   S(-42,  23),   S(-39,  41),   S(-10,  19),   S(-13,  34),   S(  1,  24),
            S(  9,   5),   S( 69, -31),   S(  4,  -5),   S(-24,   1),   S(-21,   3),   S(  4,  -9),   S(-23,  13),   S( 33, -29),
            S(-27, -25),   S( 13, -29),   S( 17, -29),   S(-22,   8),   S(-38,  31),   S(-45,  26),   S(-33,  34),   S(-14,  31),
            S(-51, -121),  S( -3, -46),   S( -1, -34),   S( 14, -23),   S(-47,  -6),   S(-32,   9),   S(-19,  10),   S( -1,  -9),
            S(-10, -120),  S(  0,   8),   S( -9, -54),   S(-14,  -8),   S( -1, -13),   S(-24,  17),   S( 18,  22),   S(-20,   7),
            S(  0,   0),   S(  0,   0),   S(  0, -49),   S(  4, -33),   S(-18,  -5),   S(-11, -16),   S(-29,   4),   S(-10,  -4),
            S(  0,   0),   S(  0,   0),   S(-12, -11),   S(  1, -11),   S(  8,  -3),   S( -6,  11),   S(  7,   3),   S(  9,   0),

            /* kings: bucket 1 */
            S(  6, -25),   S( 30, -21),   S( 17, -15),   S( 31,  -4),   S( -2,  -1),   S( 34, -21),   S(  3,   5),   S( 16, -23),
            S( 10,  -2),   S(  3,  10),   S( -3,  -8),   S(-49,  27),   S(-31,  20),   S(-11,  14),   S( -6,  16),   S(  6,   7),
            S(-12, -15),   S( -4, -13),   S(  2, -17),   S(  7, -20),   S(-37,   2),   S( 14, -18),   S( 24, -12),   S( 34, -11),
            S( -4,   0),   S(  8, -12),   S( 14,  -7),   S(  2,   4),   S( 23,   6),   S( -3,   0),   S( 38,  -8),   S(-20,  27),
            S(-20, -54),   S(-15, -45),   S( -5, -54),   S(-13, -42),   S( -1, -25),   S( -3, -29),   S( -9,  -4),   S( -7,  -3),
            S(-33,   0),   S(-102,   5),  S(-34,  27),   S(  2,  22),   S(-44,   5),   S(-25,  13),   S( 15,   4),   S( -9,  -9),
            S(-37, -51),   S(-24,   4),   S(  0,   0),   S(  0,   0),   S(-40,  13),   S(-52,  26),   S( -5,  28),   S( -3, -33),
            S(-30, -111),  S(-12, -15),   S(  0,   0),   S(  0,   0),   S( -9,   8),   S(-14,  15),   S( -4,  19),   S( -5, -47),

            /* kings: bucket 2 */
            S( 10, -54),   S(  6,  -1),   S( 18, -18),   S( 17,  -9),   S(  1,   6),   S( 37, -24),   S( -4,  17),   S( 17, -25),
            S( 35, -36),   S(-16,  31),   S(-17,   8),   S(-21,   9),   S(-27,  15),   S(-15,   6),   S(  2,   0),   S(  1,   0),
            S(-35,  -3),   S(-18, -13),   S(-12, -11),   S(-18, -18),   S( -9,  -3),   S(  0, -18),   S( 27, -17),   S( 23, -15),
            S( 14,  13),   S( -6,  12),   S( 14,   0),   S(-14,   9),   S( 42,  -9),   S( -9, -11),   S( 39, -29),   S( 31,  -9),
            S( -6, -10),   S( 16, -15),   S( 26, -38),   S(  9, -29),   S( 33, -49),   S(-22, -41),   S( 22, -49),   S(  1, -44),
            S(  1,   6),   S(-11,  -6),   S(-42,   2),   S(-39, -12),   S(  1,   1),   S(-11,  26),   S(-82,  10),   S(-21, -18),
            S( -8, -10),   S( -9,  21),   S(-75,  13),   S(-17,  10),   S(  0,   0),   S(  0,   0),   S(-10,  18),   S(-38, -36),
            S( -8, -39),   S(-20, -26),   S(-32, -32),   S( -7,  10),   S(  0,   0),   S(  0,   0),   S(-10, -12),   S(-34, -121),

            /* kings: bucket 3 */
            S( -4, -52),   S( 14,  -6),   S( 27, -21),   S( -5,  -5),   S(  1, -13),   S( 37, -26),   S(  1,  15),   S(  8, -28),
            S(  4,  17),   S(-20,  39),   S(-15,   5),   S(-33,  16),   S(-52,  30),   S(  0,  -1),   S( -8,  18),   S(  4,  11),
            S( 17, -26),   S(  5,  -5),   S( -1, -10),   S(-32,  -4),   S(-10,   8),   S( 20, -20),   S( 52, -22),   S( 54, -16),
            S(-17,  31),   S(-82,  41),   S(-49,  16),   S(-34,  12),   S(-17,   7),   S(  1, -26),   S(-22,  -7),   S(-26, -16),
            S(-14,   9),   S( -8,  -5),   S(-35, -11),   S(-15, -16),   S( 34, -45),   S( 58, -68),   S( 43, -71),   S(  6, -80),
            S(-14, -13),   S( 23,   4),   S( 21, -12),   S(  1, -24),   S( 44, -32),   S( 60, -50),   S( 72, -22),   S( 50, -115),
            S(-23, -11),   S( 26,  11),   S( 16, -14),   S( 31, -23),   S( 32, -29),   S( 28, -54),   S(  0,   0),   S(  0,   0),
            S( -5, -11),   S(  5,   9),   S( -4,  19),   S( 11, -10),   S(  7, -69),   S( -2,  10),   S(  0,   0),   S(  0,   0),

            /* kings: bucket 4 */
            S(-62,   7),   S(  3,  36),   S(  2,  23),   S(  6,   2),   S(-15,   7),   S(  1,  -8),   S(  2,   8),   S( 12, -31),
            S(-38,  22),   S( 30,  18),   S(  5,  15),   S(  0,   1),   S( 33,  -3),   S( 26,  -5),   S( 50, -16),   S( 12,  -3),
            S( -2,  26),   S( 14, -13),   S( 19,  -5),   S( -6,   0),   S(-18,  11),   S( 22, -24),   S(-35,   7),   S( 18, -12),
            S( -2, -22),   S(-10,   9),   S(  5,  15),   S(  8,   4),   S(-15,   8),   S(-10,  16),   S( 17,   9),   S( 11,   6),
            S(  0,   0),   S(  0,   0),   S( -2,   3),   S(-29,  12),   S(-35,  14),   S(-26, -15),   S(-20,   1),   S( -4,  -1),
            S(  0,   0),   S(  0,   0),   S(  6, -12),   S( -4,  24),   S(-11,  27),   S(-27, -13),   S(  7, -16),   S( -2,  16),
            S( -2, -20),   S( -4,  -7),   S( -4, -23),   S(  0,  21),   S( -6,  23),   S(-28,  -8),   S(-12,  20),   S(  3,  -4),
            S( -5, -23),   S(  2, -14),   S( -9, -19),   S( -8,   3),   S(  6,  11),   S( -7, -11),   S( -6,   0),   S(  5,  11),

            /* kings: bucket 5 */
            S( 27,  -2),   S(-18,  16),   S(-44,  27),   S(-50,  31),   S(-25,  28),   S(-10,  14),   S( 29,   1),   S( 14,  -2),
            S( -3,   1),   S( 20,  10),   S( 33,  -6),   S( 31,  -8),   S( 24,  -6),   S( 40, -13),   S( 30,   1),   S( 44, -16),
            S(-16,  10),   S( -5,  -7),   S(-15,  -4),   S( -3,  -9),   S(  9,  -1),   S(-39,   0),   S( -7,   3),   S( 12,  -1),
            S( -3, -12),   S( -1,  -5),   S(  8,  -5),   S( 11,  16),   S(  4,  21),   S( 10,   2),   S( 15,   6),   S(  7,   5),
            S( -4, -29),   S(-29, -45),   S(  0,   0),   S(  0,   0),   S( -7,  -3),   S(-19, -13),   S(  7, -14),   S(-10,   4),
            S( -6, -39),   S(-24, -28),   S(  0,   0),   S(  0,   0),   S(-22,  38),   S(-55,  12),   S(-15,  -4),   S( -7,  -4),
            S(-15, -31),   S(-32,  22),   S(  3,   8),   S(  0, -16),   S(-28,  30),   S(-40,  19),   S( -1,   9),   S( 11,  19),
            S(-10, -100),  S( -8,  13),   S(-10, -25),   S( -2, -33),   S(-10, -17),   S( -6,   9),   S( -3, -16),   S(  0,   7),

            /* kings: bucket 6 */
            S( 30, -34),   S( 22, -11),   S(-10,   4),   S(-27,  24),   S(-18,  22),   S(-31,  23),   S(-10,  25),   S(-10,   9),
            S( 39, -25),   S(  8,  17),   S( 10,  -6),   S( 27,  -9),   S( 28,  -7),   S( -5,  10),   S( 17,  -2),   S(  1,   2),
            S( 14, -18),   S(-27,   3),   S(-19,  -8),   S( -6,  -8),   S( 13, -10),   S(-45,   5),   S(  9,  -2),   S(-24,  16),
            S( 10,   6),   S( 24,  -3),   S( 14, -12),   S( 28,   5),   S( 63,  -1),   S(-29,   5),   S( -8,   8),   S(  1,   0),
            S(  6, -19),   S( 19, -30),   S(-24, -11),   S(  1, -17),   S(  0,   0),   S(  0,   0),   S(-46, -21),   S(-41, -19),
            S(-17,   1),   S(  5,  -1),   S(-31,  -1),   S(-12, -20),   S(  0,   0),   S(  0,   0),   S(-26, -15),   S(-29, -21),
            S( -1,  -9),   S( -9,   7),   S(-40,  11),   S(-16,  -2),   S(  4,   6),   S( -9, -31),   S(-28, -12),   S( -8, -38),
            S( -1,  -6),   S(  1,  -5),   S( -3,  11),   S(-14, -29),   S( -8, -35),   S( -5, -25),   S( -6,  -1),   S( -1, -58),

            /* kings: bucket 7 */
            S( 24, -31),   S(-13,  -1),   S(-33,  -1),   S(-18,  11),   S(-34,  13),   S(-52,  38),   S(-37,  37),   S(-53,  29),
            S(  7,   1),   S( 19, -20),   S( -4,  -8),   S(-23,   5),   S( -5,   4),   S(-27,  19),   S( 14,  -9),   S( -2,  11),
            S( 28, -28),   S(-18,  -8),   S(-33,  -2),   S(-31,  -3),   S(-41,   8),   S(-29,  11),   S( 18,  -5),   S(-51,  22),
            S(-24,  18),   S(  4,  10),   S( -5,   0),   S( 42,  -8),   S( 36, -10),   S( 55, -29),   S( 22, -11),   S( 13,  -8),
            S(-17,  16),   S( -3,   1),   S(  1, -24),   S( 11, -17),   S( 17, -26),   S( 11, -21),   S(  0,   0),   S(  0,   0),
            S(-11, -32),   S( -1,  -7),   S( 15, -11),   S( 12,  -6),   S( 21,  -9),   S( 16,  -9),   S(  0,   0),   S(  0,   0),
            S( 13,  18),   S( -4, -18),   S( -1,   6),   S(-12, -12),   S(  6, -19),   S( -5, -26),   S(  5, -17),   S(-11,  11),
            S(  7,   8),   S( -9,  -8),   S( 10,  17),   S( -3,  -4),   S(  7,  14),   S(-19, -50),   S(  8, -10),   S(-11, -59),

            /* kings: bucket 8 */
            S( 13, 118),   S( -3,  82),   S( 40,  40),   S( -2,  -2),   S(-12,  13),   S(-14,  -4),   S( 30, -14),   S(-17, -17),
            S( 28,  70),   S( 27,  14),   S( 50,  60),   S( 83,  -3),   S( 18,  23),   S(  6,  -6),   S( -4,   9),   S(  3,  26),
            S(  0,   0),   S(  0,   0),   S( 29,  66),   S( 39,   2),   S( 20,   7),   S(-10,  -9),   S( -1,  14),   S(  9, -16),
            S(  0,   0),   S(  0,   0),   S(  3,  75),   S( -9,   0),   S(-19,  36),   S( -5,  17),   S( 14,  10),   S( 10,  15),
            S( -4, -26),   S(  0,  26),   S(  3,  14),   S(-17,  26),   S(-17,  -3),   S(  4, -17),   S(  2,  11),   S(-15, -27),
            S(  5,  15),   S( -2, -15),   S( -3, -15),   S( -8,   2),   S(-13,   1),   S(-11,  -3),   S( -8,  -2),   S(  9,  -8),
            S( -5, -14),   S( -9, -12),   S(  5,  10),   S( -1, -11),   S( -3, -32),   S(-11,   7),   S( -3,   1),   S(  6, -46),
            S( -6,  -9),   S(-10, -26),   S( -2, -11),   S( -6, -22),   S(  6,   7),   S( -5,   3),   S(  0,  -5),   S(  5,  -3),

            /* kings: bucket 9 */
            S(  5,  29),   S(-13,  33),   S(-18,  56),   S( 17,   9),   S(-19,  33),   S(-28,  28),   S( 37,   5),   S( 18,  16),
            S(-19,  34),   S( 36,  23),   S(  3,   0),   S( 47,   1),   S( 61,  16),   S( 27,   5),   S( -7,  27),   S(-18,  13),
            S( -4,  11),   S( 22,  13),   S(  0,   0),   S(  0,   0),   S( 48,  18),   S(  0,   2),   S( 11,  -2),   S(-20,  22),
            S(  0, -31),   S( 12, -22),   S(  0,   0),   S(  0,   0),   S(  5,  33),   S( 14,  -2),   S(-13,  10),   S(-17,  30),
            S(  5, -20),   S( 11,  -2),   S(  3,  18),   S( -1,  13),   S(-14,  17),   S(-20,  15),   S(-10,  13),   S(  1, -16),
            S(  6,   3),   S(  2,  -8),   S(  7,  -9),   S( -9, -22),   S(-13,  11),   S( -2,   9),   S(-32,   1),   S(  5,  31),
            S(  2,  -7),   S( -3, -20),   S(  0,  -9),   S(  2, -30),   S( 14, -30),   S( 13,  17),   S(-16,  -9),   S(  4,   3),
            S(  7,   6),   S( -2, -23),   S( 10, -24),   S( -5, -21),   S( -1, -18),   S(  2,   9),   S( -7,  10),   S(  8,  -1),

            /* kings: bucket 10 */
            S( 35,  -1),   S(  3,  -8),   S(  5,   9),   S(  8,  24),   S(-13,  20),   S(-93,  49),   S(-33,  48),   S(-92,  87),
            S(  3,   1),   S( 62,   0),   S( 26,  -5),   S( 32,  10),   S( 59,  12),   S( 48,   3),   S( 17,  24),   S(-90,  49),
            S( 15,   7),   S( 28,   1),   S( 26, -12),   S( 14,  10),   S(  0,   0),   S(  0,   0),   S( -6,  22),   S(-59,  27),
            S( 14,   6),   S( 41, -26),   S( 34, -32),   S( 30,   3),   S(  0,   0),   S(  0,   0),   S( -3,  14),   S(  5,   2),
            S(  2,   6),   S( 28,   6),   S( 30, -19),   S(  9, -29),   S(  4, -17),   S(  7,  24),   S(  9,   8),   S(-10,  15),
            S(  3,  15),   S(  3,  -5),   S( -1,   4),   S( 10,  -7),   S(  7,  -1),   S(-17,  -6),   S(-12,   6),   S(  0,  -7),
            S(  0, -41),   S( -3, -16),   S(  9, -10),   S( 13,   0),   S( 10,   0),   S(-10, -18),   S(  4, -27),   S(  5,   6),
            S(  4,   4),   S( 11, -12),   S( -2, -14),   S(  0,   4),   S(  6, -14),   S( -1, -30),   S( -5,  -6),   S(  9,   3),

            /* kings: bucket 11 */
            S( -5, -18),   S(  9,   8),   S(  6,  -9),   S( -6,  14),   S( -7,   6),   S(-69,  57),   S(-73,  81),   S(-131, 153),
            S( -2, -25),   S( 21,   4),   S(-11, -16),   S( 20,  22),   S( 89,  -1),   S( 64,  40),   S( 18,  18),   S( 23,  40),
            S(  4, -49),   S( -3,  19),   S(  1, -11),   S( 23,   9),   S( 67,   1),   S( 27,  61),   S(  0,   0),   S(  0,   0),
            S(  2,  21),   S( 18,  11),   S( -8,   4),   S(  9,  15),   S( 23,  -9),   S( 22,  23),   S(  0,   0),   S(  0,   0),
            S(  0,  35),   S(  1,  -3),   S(  8,  -7),   S( 13, -14),   S( 16,   0),   S(  0,   1),   S(  9,  10),   S(  7,   0),
            S( 11,  10),   S(  0, -15),   S( 15, -11),   S(  0,   4),   S( -6,  -7),   S(  3, -17),   S( -5,  -8),   S(-11,  -3),
            S(  7,  13),   S(  7,  -6),   S( 17,  23),   S(  0, -26),   S( 15, -16),   S(  2,   3),   S(-10, -11),   S( -7, -12),
            S(  4,   9),   S(  4,  -1),   S(-12, -22),   S(  5,  -6),   S( -4, -20),   S( -8, -17),   S(  0, -20),   S(  6,  12),

            /* kings: bucket 12 */
            S(  0,   0),   S(  0,   0),   S( 20,  58),   S(  7,  -6),   S(  1,  -2),   S(  8,  14),   S(  8,  -1),   S(-19,   8),
            S(  0,   0),   S(  0,   0),   S( 48, 108),   S( 28,  14),   S( 22,  43),   S( 13,  -3),   S( 25,  -6),   S(-17,  -1),
            S( -1,  10),   S(  3,  13),   S( 23,  71),   S( 39,  18),   S(  8,  -7),   S( 11,   1),   S(  2, -13),   S( -8,  -1),
            S( -2,   9),   S(  9,  31),   S(  0,  15),   S(  3,  -7),   S( -9,   0),   S( -1,  18),   S( -3,   9),   S(  1,   7),
            S(  9,  17),   S(  6,  23),   S( 11,  20),   S( -3,  41),   S( -3,  40),   S(  0,   2),   S( -9,  14),   S(-12, -12),
            S(  6,   6),   S(  9,  15),   S( -2,  -2),   S(-10, -16),   S( -1,   5),   S( -8,  17),   S( -9, -16),   S(  7,  -1),
            S(  3,   8),   S( -7, -13),   S( -2,   6),   S( -1,   0),   S( -6,  -9),   S(  4,   8),   S(  8,  44),   S(  0, -29),
            S( -3,   2),   S(  6,   3),   S( -4,   6),   S(  0,   3),   S( -1,  -4),   S(  3,   6),   S(-11, -22),   S( -3, -22),

            /* kings: bucket 13 */
            S( -1,  53),   S(  7,  35),   S(  0,   0),   S(  0,   0),   S( 44,  18),   S( 14, -12),   S( -5,  -7),   S(-18,  26),
            S(  1,  20),   S(  0,  -4),   S(  0,   0),   S(  0,   0),   S( 47,   3),   S( 29,  -9),   S(-17,   4),   S(-14,   7),
            S( -2,   3),   S( 19,  22),   S(  2,  -7),   S( 14,  37),   S( 52,  13),   S( 22,  -6),   S(  3,   6),   S( 14,  -8),
            S(-10,  -6),   S( 14,  -2),   S(  0,  19),   S( -7,  16),   S( -5,  14),   S(  3, -12),   S(  4,  21),   S(-15, -27),
            S(  6,  11),   S(  0,   6),   S(  4,  42),   S( -5,  24),   S( -8,  10),   S(  6,  18),   S(-10,   1),   S(  8,  10),
            S(  4,  -1),   S( -5,  17),   S( -2,  17),   S( -5,  -1),   S(-12, -16),   S( -5,   9),   S( -8,  19),   S(  1,   1),
            S(  9,  11),   S( -8, -21),   S(-11, -44),   S(  4,  20),   S(-11, -10),   S(-10,  15),   S(-14, -24),   S(  6,  13),
            S(  1,  -1),   S(  5,  -4),   S(  4,  20),   S(  3,   4),   S(  0,  17),   S(-10, -17),   S( -3,   8),   S(  8,  15),

            /* kings: bucket 14 */
            S( 18,  34),   S(  0,  -6),   S( 11, -41),   S( 16,   0),   S(  0,   0),   S(  0,   0),   S(  6,  71),   S(-43,  40),
            S( -9,  -7),   S( 19,  -8),   S( 46, -34),   S( 41,  12),   S(  0,   0),   S(  0,   0),   S( 14,  30),   S(-42,   4),
            S(  5,   5),   S( 15,  -5),   S( 35, -34),   S( 40,   3),   S( 11,  -2),   S( 14,  33),   S( 27,  56),   S(-26,   3),
            S(  9,  -5),   S(  8, -10),   S( -2, -11),   S( 10,   1),   S(-22,  -1),   S( 14,  54),   S(  3,  24),   S(  7,  -2),
            S(  7,  19),   S(  9,  -2),   S( -8,   2),   S(-18,  11),   S(  0,  30),   S(  5,  54),   S(  2,  38),   S(  6,  13),
            S( -5,  -7),   S(  2,   6),   S( -2,  -2),   S(  0,  11),   S( -5, -21),   S( -5,  -3),   S(-14,  -8),   S(  0,   6),
            S(  4,   6),   S(-10, -14),   S( 11,  -6),   S( 16,   4),   S(  3,  -2),   S( -6,  17),   S(-27, -22),   S(  8,  17),
            S(  1,  14),   S(  5,  -6),   S(  9,   2),   S( -5,  -6),   S(  7, -10),   S( -3,  -5),   S(-13, -24),   S(  0, -10),

            /* kings: bucket 15 */
            S( 12,  32),   S(  7,  -2),   S( 11,  -7),   S( -7,   0),   S(-10, -10),   S(  1,  57),   S(  0,   0),   S(  0,   0),
            S( -2, -22),   S(  7, -11),   S( -7, -14),   S( 20,  51),   S( 40,  -1),   S( 62, 108),   S(  0,   0),   S(  0,   0),
            S( -8, -22),   S( 16, -10),   S(  7, -17),   S( -4,  12),   S( 10,  -5),   S( 26,  69),   S(  9,  42),   S(-13,  -2),
            S( -1, -11),   S(  3,  14),   S(  3,  14),   S(-13, -29),   S(-13,  -2),   S( 20,  47),   S( 16,  47),   S( -2, -11),
            S( 10,   5),   S( -9,  25),   S(  0,  -3),   S( -5, -35),   S( -3,   7),   S(  2,  33),   S(  4,   5),   S(  4,   4),
            S(  5,  27),   S(-14,  -4),   S(  8,  16),   S(  9,  19),   S( -9, -24),   S( -2,   7),   S(  0,   7),   S(  4,  16),
            S(  8,  12),   S( -4,  23),   S( -2, -11),   S(  4,   6),   S(  9,   7),   S(  9,  15),   S( -5,  -2),   S(  2,   1),
            S( -2,  -7),   S(  4,   1),   S( -2, -11),   S(  4,   3),   S(  4,   4),   S( 10,  14),   S(  0,  -7),   S(  3,   8),

            #endregion

            /* piece mobility */
            #region piece mobility

            /* knight mobility */
            S(-42,  48),   S(-18, -19),   S(  1,  63),   S( 15,  98),   S( 25, 120),   S( 33, 144),   S( 42, 148),   S( 54, 142),
            S( 68, 122),

            /* bishop mobility */
            S(-36,  22),   S(-13,  -3),   S(  4,  45),   S( 14,  86),   S( 24, 110),   S( 29, 130),   S( 33, 142),   S( 38, 146),
            S( 43, 148),   S( 52, 145),   S( 64, 136),   S( 85, 129),   S( 93, 129),   S( 63, 132),

            /* rook mobility */
            S(-110,   4),  S(-31,   5),   S(-15,  85),   S(-13, 114),   S(-12, 144),   S( -7, 156),   S( -1, 166),   S(  7, 168),
            S( 13, 180),   S( 19, 184),   S( 22, 191),   S( 30, 190),   S( 40, 192),   S( 47, 196),   S( 85, 168),

            /* queen mobility */
            S( 89, 163),   S(-24, 327),   S( 26, 209),   S( 40, 118),   S( 49, 131),   S( 50, 191),   S( 52, 230),   S( 54, 264),
            S( 56, 294),   S( 57, 319),   S( 60, 335),   S( 63, 346),   S( 64, 354),   S( 65, 367),   S( 66, 369),   S( 67, 371),
            S( 70, 371),   S( 73, 365),   S( 81, 352),   S( 95, 336),   S(104, 319),   S(147, 277),   S(150, 269),   S(174, 234),
            S(187, 220),   S(180, 201),   S(123, 193),   S(109, 145),

            #endregion

            /* pawn structure */
            #region pawn structure

            /* passed pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1,   2),   S(-24,  32),   S(-33,  29),   S(-43,  49),   S(  4,  -1),   S(-13,  -1),   S( -4,  43),   S( 14,  16),
            S(  7,  22),   S( -5,  32),   S(-20,  33),   S(-23,  28),   S( -6,  24),   S(-29,  28),   S(-29,  42),   S( 22,  17),
            S( 18,  57),   S( 10,  58),   S(  6,  43),   S( 21,  38),   S( -2,  42),   S(-25,  52),   S(-32,  82),   S(-13,  65),
            S( 28,  93),   S( 39, 105),   S( 20,  71),   S(  8,  55),   S(  6,  59),   S(-11,  79),   S(-56, 114),   S(-82, 138),
            S( 19, 142),   S( 49, 179),   S( 52, 124),   S( 26, 110),   S(-56, 102),   S( 13, 102),   S(-61, 170),   S(-88, 160),
            S( 91, 222),   S( 86, 268),   S(129, 234),   S(124, 248),   S(129, 259),   S(155, 237),   S(137, 252),   S(131, 252),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* adjacent/phalanx pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  6,  -6),   S( -6, -26),   S( -4, -19),   S(  4,  -9),   S( 13,   4),   S(-15, -44),   S(-22,   8),   S(  0, -54),
            S(-19,  16),   S( 24, -23),   S( -2,  27),   S( 15,  21),   S( 32, -11),   S( -6,  16),   S( 26, -23),   S( -5,  -6),
            S(-11,  19),   S( 14,   5),   S(  4,  42),   S( 16,  55),   S( 25,  30),   S( 33,  17),   S( 30,   0),   S( -2,  18),
            S( 16,  38),   S( 16,  50),   S( 34,  96),   S( 13, 103),   S( 65,  70),   S( 67,  57),   S( 24,  57),   S( 18,  27),
            S( 51,  94),   S( 88, 114),   S(101, 139),   S(140, 162),   S(137, 132),   S(137, 145),   S(131, 121),   S( 50,  60),
            S( 72, 195),   S(117, 278),   S(102, 221),   S( 97, 198),   S( 67, 152),   S( 48, 139),   S( 40, 142),   S( 16,  79),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* chained pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 10,  17),   S( 16,  18),   S( 28,  29),   S( 32,  17),   S( 19,  17),   S( 21,  17),   S(  5,   7),   S( 38,  -7),
            S( -5,  24),   S( 16,  36),   S( 12,  35),   S(  9,  44),   S( 24,  16),   S( 14,  21),   S( 31,  18),   S(  1,  14),
            S(  0,  25),   S( 28,  53),   S( 51,  61),   S( 37,  64),   S( 43,  57),   S( 69,  21),   S( 31,  38),   S( 18,  10),
            S( 54,  73),   S(104,  56),   S(119, 123),   S(143, 130),   S(132, 121),   S( 70, 131),   S( 68,  57),   S( 65,  13),
            S( 45, 122),   S( 90, 138),   S(153, 208),   S(108, 250),   S(133, 258),   S( 80, 236),   S(148, 201),   S(-56, 167),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* pawn ram */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S( 26,  33),   S( 12,  12),   S( 13,  29),   S(-12,  60),   S( 65,  17),   S( 20,   6),   S( -3,  -3),   S( 29,  13),
            S( -1,  15),   S(  6,   8),   S( 17,  16),   S( 12,  30),   S(  7,  17),   S(  3,   7),   S( 10,   5),   S( 27,  -3),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  1, -15),   S( -6,  -8),   S(-17, -16),   S(-12, -30),   S( -7, -17),   S( -3,  -7),   S(-10,  -5),   S(-27,   3),
            S(-26, -33),   S(-12, -12),   S(-13, -29),   S( 12, -60),   S(-65, -17),   S(-20,  -6),   S(  3,   3),   S(-29, -13),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* isolated pawn */
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),
            S(-16, -29),   S(-12, -43),   S(-12, -43),   S(-55, -32),   S(-21, -42),   S(-27, -41),   S( -7, -51),   S(-10, -50),
            S(-16, -13),   S(-20, -30),   S(-31, -10),   S( -2, -37),   S(-34, -33),   S(-27, -21),   S(-35, -22),   S(  0, -32),
            S( -9,  -7),   S( -8, -34),   S(-23,  -6),   S(-27, -21),   S(-15, -37),   S(-20, -14),   S(-10, -21),   S(-27, -17),
            S(  4, -23),   S( 17, -46),   S( 13, -14),   S( 10, -28),   S( 10, -26),   S( 56, -38),   S( 34, -44),   S(  0, -42),
            S( 23, -43),   S( 39, -78),   S( 48, -28),   S( 63, -35),   S( 78, -51),   S( 80, -36),   S(131, -98),   S( 45, -73),
            S(105, -95),   S(121, -116),  S( 94, -50),   S( 75, -33),   S( 70, -32),   S(121, -45),   S( 94, -53),   S( 56, -80),
            S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),   S(  0,   0),

            /* backward pawn */
            S(-11, -25),

            /* doubled pawn */
            S(-12, -33),

            #endregion

            /* king safety */
            #region king safety

            /* squares attacked near enemy king */
            S(-44,  18),        // attacks to squares 1 from king
            S(-43,   5),   S(-15,   1),   S( 25,  -8),   S( 75, -22),   S(128, -33),   S(142, -16),   S(191, -32),   S(238, -20),

            S(-38,  11),        // attacks to squares 2 from king
            S(-38,   8),   S(-27,   9),   S( -6,   3),   S( 15,   0),   S( 39,  -4),   S( 62, -11),   S( 90, -20),   S(146, -28),

            /* castling available */
            S( 69, -61),        // king-side castling available
            S( 15,  67),        // queen-side castling available

            /* king mobility penalties (open line attacks) */
            /* diagonal lines */
            S( 41, -93),   S( 45, -82),   S( 39, -92),   S( 31, -77),   S( 24, -67),   S( 16, -61),   S( -1, -48),   S( -3, -44),
            S( 11, -47),   S( 29, -49),   S( 55, -47),   S( 21, -31),   S( 92, -52),

            /* orthogonal lines */
            S(-42, -144),  S(-94, -109),  S(-116, -88),  S(-132, -82),  S(-139, -85),  S(-145, -86),  S(-144, -92),  S(-139, -98),
            S(-152, -88),  S(-168, -84),  S(-166, -97),  S(-118, -130), S(-86, -142),  S(-34, -152),

            /* pawnless flank */
            S( 48, -33),

            #endregion

            /* passed pawns */
            #region passed pawns

            /* enemy king outside passed pawn square */
            S( 26, 225),

            /* passed pawn can advance */
            S(-11,  34),   S( -3,  60),   S( 18, 101),   S( 91, 167),

            /* blocked passed pawn */
            S(  0,   0),   S( 57, -16),   S( 32,   9),   S( 29,  45),   S( 28,  63),   S( 20,  35),   S( 71,  63),   S(  0,   0),       // blocked by Knight
            S(  0,   0),   S( 53, -43),   S( 48,  26),   S( 23,  36),   S( 13,  59),   S( 29,  95),   S(134, 125),   S(  0,   0),       // blocked by Bishop
            S(  0,   0),   S(-14, -16),   S( -7, -33),   S(  0, -33),   S(-23,  -8),   S(-30,  20),   S(117,  10),   S(  0,   0),       // blocked by Rook
            S(  0,   0),   S( 26, -11),   S( 27, -13),   S(  5,  -2),   S(  3, -37),   S(-14, -115),  S(-37, -209),  S(  0,   0),       // blocked by Queen
            S(  0,   0),   S( 25,  50),   S( 59,  22),   S( 96,  44),   S( 32,  23),   S(174, 113),   S(104, 117),   S(  0,   0),       // blocked by King

            /* rook behind passed pawn */
            S( 12,  53),

            #endregion

            /* piece evaluations */
            #region piece evaluations

            /* bishop pair */
            S(-30, 122),

            /* bad bishop pawn */
            S( -6, -15),

            /* rook on open file */
            S( 27,   2),

            /* rook on half-open file */
            S(  5,  43),

            /* pawn shields minor piece */
            S( 12,  11),

            /* bishop on long diagonal */
            S( 24,  50),

            /* minor outpost */
            S(  6,  34),   S( 18,  29),

            #endregion

            /* threats */
            #region threats

            /* pushed pawn threats */
            S(  0,   0),   S( 20,  23),   S( 20,  -4),   S( 32,  18),   S( 27,  -4),   S( 37, -21),

            /* pawn threats */
            S(  0,   0),   S( 65,  97),   S( 51, 116),   S( 73,  88),   S( 61,  44),   S(  0,   0),

            /* minor piece threats */
            S(  0,   0),   S( 32,  64),   S( 52,  49),   S( 78,  45),   S( 51,  68),   S(  0,   0),

            /* rook threats */
            S(  0,   0),   S( 25,  46),   S( 30,  41),   S(-18,  44),   S( 67,  64),   S(  0,   0),

            /* check threats */
            S(  0,   0),   S( 22,  11),   S( 20,  34),   S( 30,  14),   S(  5,  32),   S(  0,   0),

            #endregion

            /* tempo bonus for side to move */
            S( 17,  16),
        ];

        private static readonly Weights defWts = new Weights(defaultWeights);
    }
}
