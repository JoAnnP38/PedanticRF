using Pedantic.Chess;
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
        { }

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
                score += signs[color](EvalMaterialAndPst(board, evalInfo, color));
                score += signs[color](EvalPieces(board, evalInfo, color));

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

        private static Score EvalPieces(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            Score score = Score.Zero;
            for (Piece piece = Piece.Knight; piece <= Piece.Queen; piece++)
            {
                // calculate mobility
                foreach (SquareIndex from in board.Pieces(color, piece))
                {
                    Bitboard pieceAttacks = Board.GetPieceMoves(piece, from, board.All);
                    evalInfo[c].PieceAttacks |= pieceAttacks;
                    int mobility = (pieceAttacks & evalInfo[c].MobilityArea).PopCount;
                    score += wts.PieceMobility(piece, mobility);
                }
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
    }
}
