using Pedantic.Chess;
using Pedantic.Collections;
using Pedantic.Utilities;
using System.Runtime.CompilerServices;

namespace Pedantic.Chess.HCE
{
    public class HceEval : IInitialize
    {
        public struct EvalInfo
        {
            public Bitboard Pawns;
            public Bitboard DefendedPawns;
            public Bitboard PieceAttacks;
            public Bitboard PawnAttacks;
            public Bitboard MobilityArea;
            public Bitboard PassedPawns;
            public short Material;
            public SquareIndex KI;
            public KingBuckets KB;
            public GamePhase GamePhase;
        }
        
        static HceEval()
        {
            wts = Engine.Weights;
            signs[Color.White] = (s) => s;
            signs[Color.Black] = (s) => -s;
        }

        public HceEval()
        { 
        }

        public HceEval(Weights weights)
        { 
            Weights = weights;
        }

        public static Weights Weights
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return wts;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                wts = value;
            }
        }

        public short Compute(Board board)
        {
            Span<EvalInfo> evalInfo = stackalloc EvalInfo[2];

            InitializeEvalInfo(board, evalInfo);
            short score = ComputeNormal(board, evalInfo);
            //Color winningColor = score >= 0 ? Color.White : Color.Black;
            //score = (short)((score * ScaleFactor(winningColor, board, evalInfo)) / 4);
            score = board.SideToMove == Color.White ? score : (short)-score;
            return score;
        }

        public short ComputeNormal(Board board, Span<EvalInfo> evalInfo)
        {
            Score score = Score.Zero;
            for (Color color = Color.White; color <= Color.Black; color++)
            {
                int c = (int)color;
                int o = (int)color.Flip();
                Func<Score, Score> sign = signs[color];
                score += sign(EvalMaterialAndPst(board, evalInfo, color));

                // insert alpha/beta margin check here

                score += sign(EvalPawns(board, evalInfo, color));
                score += sign(EvalPieces(board, evalInfo, color));

                if (color == board.SideToMove)
                {
                    score += signs[color](wts.TempoBonus);
                }
            }

            return score.NormalizeScore(board.Phase);
        }

        private Score EvalMaterialAndPst(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            // material remains up to date via incremental updates
            Score score = board.Material[color];
            int c = (int)color;

            foreach (SquareIndex from in board.Units(color))
            {
                SquareIndex normalFrom = from.Normalize(color);
                Piece piece = board.PieceBoard(from).Piece;
                score += wts.FriendlyPieceSquareValue(piece, evalInfo[c].KB, normalFrom);
                score += wts.EnemyPieceSquareValue(piece, evalInfo[c].KB, normalFrom);
            }

            return score;
        }

        private static Score EvalPawns(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;
            Score score = Score.Zero;
            Bitboard pawns = evalInfo[c].Pawns;

            if (pawns == 0)
            {
                return score;
            }

            Bitboard otherPawns = evalInfo[o].Pawns;
            Bitboard pawnRams = (color == Color.White ? otherPawns >> 8 : otherPawns << 8);


            foreach (SquareIndex from in pawns)
            {
                Ray ray = Board.Vectors[(int)from];
                Bitboard friendMask = color == Color.White ? ray.North : ray.South;
                SquareIndex normalFrom = from.Normalize(color);
                Bitboard sqMask = new Bitboard(from);

                if ((otherPawns & PassedPawnMasks[c, (int)from]) == 0 && (pawns & friendMask) == 0)
                {
                    score += wts.PassedPawn(normalFrom);
                    evalInfo[c].PassedPawns |= sqMask;
                }

                if ((pawns & AdjacentPawnMasks[(int)from]) != 0)
                {
                    score += wts.PhalanxPawn(normalFrom);
                }

                if ((evalInfo[c].PawnAttacks & sqMask) != 0)
                {
                    score += wts.ChainedPawn(normalFrom);
                }

                if ((pawnRams & sqMask) != 0)
                {
                    score += wts.PawnRam(normalFrom);
                }
            }

            return score;
        }

        private static Score EvalPieces(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            Score score = Score.Zero;
            Bitboard exclude = board.Pawns | board.Kings;
            foreach (SquareIndex from in board.Units(color).AndNot(exclude))
            {
                Piece piece = board.PieceBoard(from).Piece;
                Bitboard pieceAttacks = Board.GetPieceMoves(piece, from, board.All);
                evalInfo[c].PieceAttacks |= pieceAttacks;
                int mobility = (pieceAttacks & evalInfo[c].MobilityArea).PopCount;
                score += wts.PieceMobility(piece, mobility);
            }

            return score;
        }

        public static bool SufficientMatingMaterial(Board board, Span<EvalInfo> evalInfo, Color side)
        {
            int numKnights = board.Pieces(side, Piece.Knight).PopCount;
            int numBishops = board.Pieces(side, Piece.Bishop).PopCount;
            bool case1 = (board.Pieces(side, Piece.Rook) | board.Pieces(side, Piece.Queen)) != 0;
            bool case2 = (numKnights >= 1 && numBishops >= 1) || numBishops >= 2 || numKnights >= 3;
            return case1 || case2;
        }

        public int ScaleFactor(Color winningColor, Board board, Span<EvalInfo> evalInfo)
        {
            if (IsOcbEndgame(board, out int pcCount))
            {
                return pcCount == 1 ? 2 : 3;
            }

            return 4;
        }

        public bool IsOcbEndgame(Board board, out int pcCount)
        {
            pcCount = 0;
            int whitePieceCount = board.Units(Color.White).AndNot(board.Pawns).PopCount - 1;
            int blackPieceCount = board.Units(Color.Black).AndNot(board.Pawns).PopCount - 1;
            Bitboard whiteBishops = board.Pieces(Color.White, Piece.Bishop);
            Bitboard blackBishops = board.Pieces(Color.Black, Piece.Bishop);

            if (whitePieceCount > 3 || blackPieceCount > 3 || whitePieceCount != blackPieceCount ||
                whiteBishops.PopCount != 1 || blackBishops.PopCount != 1)
            {
                return false;
            }

            pcCount = whitePieceCount;
            bool isWhiteDark = ((SquareIndex)whiteBishops.TzCount).IsDark();
            bool isBlackDark = ((SquareIndex)blackBishops.TzCount).IsDark();
            return isWhiteDark != isBlackDark;
        }

        public static void InitializeEvalInfo(Board board, Span<EvalInfo> evalInfo)
        {
            evalInfo.Clear();
            for (Color color = Color.White; color <= Color.Black; color++)
            {
                int c = (int)color;
                Bitboard pawns = board.Pieces(color, Piece.Pawn);
                evalInfo[c].Pawns = pawns;
                evalInfo[c].Material = board.Material[color].NormalizeScore(board.Phase);
                evalInfo[c].KI = board.KingIndex[color];
                evalInfo[c].KB = new KingBuckets(color, board.KingIndex[color], board.KingIndex[color.Flip()]);
                if (color == Color.White)
                {
                    evalInfo[c].PawnAttacks = (pawns.AndNot(Bitboard.BbFileA) << 7) 
                                            | (pawns.AndNot(Bitboard.BbFileH) << 9);
                }
                else
                {
                    evalInfo[c].PawnAttacks = (pawns.AndNot(Bitboard.BbFileH) >> 7)
                                            | (pawns.AndNot(Bitboard.BbFileA) >> 9);
                }
            }

            /* two pass init */
            for (Color color = Color.White; color <= Color.Black; color++)
            {
                Color other = color.Flip();
                int c = (int)color;
                int o = (int)other;
                evalInfo[c].MobilityArea = ~(board.Units(color) | evalInfo[o].PawnAttacks);
            }
        }

        public static void Initialize() {}

        public readonly Bitboard BB_KS_MASK = new Bitboard(0xf0f0f0f0f0f0f0f0ul);
        public readonly Bitboard BB_QS_MASK = new Bitboard(0x0f0f0f0f0f0f0f0ful);

        private GamePhase gamePhase;
        private static ByColor<Func<Score, Score>> signs = new();
        private static Weights wts;


        public static readonly FixedArray2D<ulong> PassedPawnMasks = new (MAX_COLORS, MAX_SQUARES)
        {
            #region PassedPawnMasks data

            // white passed pawn masks
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0003030303030000ul, 0x0007070707070000ul, 0x000E0E0E0E0E0000ul, 0x001C1C1C1C1C0000ul,
            0x0038383838380000ul, 0x0070707070700000ul, 0x00E0E0E0E0E00000ul, 0x00C0C0C0C0C00000ul,
            0x0003030303000000ul, 0x0007070707000000ul, 0x000E0E0E0E000000ul, 0x001C1C1C1C000000ul,
            0x0038383838000000ul, 0x0070707070000000ul, 0x00E0E0E0E0000000ul, 0x00C0C0C0C0000000ul,
            0x0003030300000000ul, 0x0007070700000000ul, 0x000E0E0E00000000ul, 0x001C1C1C00000000ul,
            0x0038383800000000ul, 0x0070707000000000ul, 0x00E0E0E000000000ul, 0x00C0C0C000000000ul,
            0x0003030000000000ul, 0x0007070000000000ul, 0x000E0E0000000000ul, 0x001C1C0000000000ul,
            0x0038380000000000ul, 0x0070700000000000ul, 0x00E0E00000000000ul, 0x00C0C00000000000ul,
            0x0003000000000000ul, 0x0007000000000000ul, 0x000E000000000000ul, 0x001C000000000000ul,
            0x0038000000000000ul, 0x0070000000000000ul, 0x00E0000000000000ul, 0x00C0000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            // black passed pawn masks
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000300ul, 0x0000000000000700ul, 0x0000000000000E00ul, 0x0000000000001C00ul,
            0x0000000000003800ul, 0x0000000000007000ul, 0x000000000000E000ul, 0x000000000000C000ul,
            0x0000000000030300ul, 0x0000000000070700ul, 0x00000000000E0E00ul, 0x00000000001C1C00ul,
            0x0000000000383800ul, 0x0000000000707000ul, 0x0000000000E0E000ul, 0x0000000000C0C000ul,
            0x0000000003030300ul, 0x0000000007070700ul, 0x000000000E0E0E00ul, 0x000000001C1C1C00ul,
            0x0000000038383800ul, 0x0000000070707000ul, 0x00000000E0E0E000ul, 0x00000000C0C0C000ul,
            0x0000000303030300ul, 0x0000000707070700ul, 0x0000000E0E0E0E00ul, 0x0000001C1C1C1C00ul,
            0x0000003838383800ul, 0x0000007070707000ul, 0x000000E0E0E0E000ul, 0x000000C0C0C0C000ul,
            0x0000030303030300ul, 0x0000070707070700ul, 0x00000E0E0E0E0E00ul, 0x00001C1C1C1C1C00ul,
            0x0000383838383800ul, 0x0000707070707000ul, 0x0000E0E0E0E0E000ul, 0x0000C0C0C0C0C000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul

            #endregion PassedPawnMasks data
        };

        public static readonly FixedArray<ulong> AdjacentPawnMasks = new (MAX_SQUARES)
        {
            #region AdjacentPawnMasks data

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000200ul, 0x0000000000000500ul, 0x0000000000000A00ul, 0x0000000000001400ul,
            0x0000000000002800ul, 0x0000000000005000ul, 0x000000000000A000ul, 0x0000000000004000ul,
            0x0000000000020000ul, 0x0000000000050000ul, 0x00000000000A0000ul, 0x0000000000140000ul,
            0x0000000000280000ul, 0x0000000000500000ul, 0x0000000000A00000ul, 0x0000000000400000ul,
            0x0000000002000000ul, 0x0000000005000000ul, 0x000000000A000000ul, 0x0000000014000000ul,
            0x0000000028000000ul, 0x0000000050000000ul, 0x00000000A0000000ul, 0x0000000040000000ul,
            0x0000000200000000ul, 0x0000000500000000ul, 0x0000000A00000000ul, 0x0000001400000000ul,
            0x0000002800000000ul, 0x0000005000000000ul, 0x000000A000000000ul, 0x0000004000000000ul,
            0x0000020000000000ul, 0x0000050000000000ul, 0x00000A0000000000ul, 0x0000140000000000ul,
            0x0000280000000000ul, 0x0000500000000000ul, 0x0000A00000000000ul, 0x0000400000000000ul,
            0x0002000000000000ul, 0x0005000000000000ul, 0x000A000000000000ul, 0x0014000000000000ul,
            0x0028000000000000ul, 0x0050000000000000ul, 0x00A0000000000000ul, 0x0040000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul

            #endregion AdjacentPawnMasks data
        };

    }
}
