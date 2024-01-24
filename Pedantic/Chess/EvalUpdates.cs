
namespace Pedantic.Chess
{
    public class EvalUpdates
    {
        public virtual void RestoreState(ref Board.BoardState state) { }
        public virtual void SaveState(ref Board.BoardState state) { }
        public virtual void Update(Board board) { }
        public virtual void Update(Move move) { }
    }
}
