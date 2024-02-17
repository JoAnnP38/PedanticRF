
namespace Pedantic.Chess
{
    public struct SearchItem
    {
        public Move Move;
        public bool IsCheckingMove;
        public MovePair Killers;
        public short Eval;

        public SearchItem()
        {
            Move = Move.NullMove;
            IsCheckingMove = false;
            Killers = new();
            Eval = 0;
        }
    }
}
