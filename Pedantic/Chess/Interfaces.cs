namespace Pedantic.Chess
{
    public interface IHistory
    {
        public short this[Color stm, Piece piece, SquareIndex to] { get; }
        public short this[Move move] { get; }
    }

    public interface IInitialize
    {
        public static void Initialize() { }
    }

    public interface IEvaluate
    {
        public void AddPiece(Color color, Piece Piece, SquareIndex sq);
        public void RemovePiece(Color color, Piece Piece, SquareIndex sq);
        public short Compute(short alpha, short beta);
    }

    // Implemented by HceEval & NnueEval classes and used by the Board/Search classes for position updates
    public interface IEfficientlyUpdateable
    {
        // called when a new position is setup (initial update)
        // called after position is setup with UCI position command
        public void Update(Board board);

        // called after a legal move is made (incremental update)
        // called after move is confirmed to be legal to avoid unnecessary updates.
        public void Update(Move move);

        // called to allow eval to save any state required for Copy-Make
        public void SaveState(ref Board.BoardState state);

        // called to restore the state saved in SaveState()
        public void RestoreState(ref Board.BoardState state);
    }
}
