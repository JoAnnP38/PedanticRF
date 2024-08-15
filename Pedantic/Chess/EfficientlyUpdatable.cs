namespace Pedantic.Chess
{
    public class EfficientlyUpdatable : IEfficientlyUpdatable
    {
        public virtual void AddPiece(Color color, Piece piece, SquareIndex sq)
        { }

        public virtual void Clear()
        { }

        public virtual void Copy(IEfficientlyUpdatable other)
        { }

        public virtual void PopState()
        { }

        public virtual void PushState()
        { }

        public virtual void RemovePiece(Color color, Piece piece, SquareIndex sq)
        { }

        public virtual void RestoreState()
        { }

        public virtual void Update(Board board)
        { }

        public virtual void UpdatePiece(Color color, Piece piece, SquareIndex from, SquareIndex to)
        { }
    }
}
