namespace Pedantic.Chess.HCE
{
    public class HceEval : EvalUpdates
    {
        public HceEval()
        {
            weights = Weights.Default;
            signs[Color.White] = (s) => s;
            signs[Color.Black] = (s) => -s;
        }

        public HceEval(Weights weights)
        {
            this.weights = weights;
            signs[Color.White] = (s) => s;
            signs[Color.Black] = (s) => -s;
        }

        public short Compute(Board board)
        {
            Score score = Score.Zero;
            for (Color color = Color.White; color <= Color.Black; color++)
            {
                KingBuckets kb = new (color, board.KingIndex[color], board.KingIndex[color.Flip()]);
                score += signs[color](EvalMaterialAndPst(color, board, kb));
            }
            return signs[board.SideToMove](score).NormalizeScore(phase);
        }

        private Score EvalMaterialAndPst(Color color, Board board, KingBuckets kb)
        {
            // material remains up to date via incremental updates
            Score score = Score.Zero;

            foreach (SquareIndex from in board.Units(color))
            {
                Piece piece = board.PieceBoard(from).Piece;
                score += weights.PieceValue(piece);
                score += weights.FriendlyPieceSquareValue(piece, kb, from);
                score += weights.EnemyPieceSquareValue(piece, kb, from);
            }

            return score;
        }

        public override void Update(Board board)
        {
            phase = 0;
            foreach (SquareIndex from in board.All)
            {
                phase += board.PieceBoard(from).Piece.PhaseValue();
            }
        }

        public override void Update(Move move)
        {
            if (move.IsPromote)
            {
                phase += (short)(move.Promote.PhaseValue() - Piece.Pawn.PhaseValue());
                phase = Math.Clamp(phase, (short)0, (short)64);
            }

            if (move.IsCapture)
            {
                phase -= move.Capture.PhaseValue();
                phase = Math.Clamp(phase, (short)0, (short)64);
            }
        }

        public override void SaveState(ref Board.BoardState state)
        {
            state.Phase = (byte)phase;
        }

        public override void RestoreState(ref Board.BoardState state)
        {
            phase = state.Phase;
        }

        private Weights weights;
        private short phase;
        private ByColor<Func<Score, Score>> signs = new();
    }
}
