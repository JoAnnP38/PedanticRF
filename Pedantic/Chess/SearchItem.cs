
namespace Pedantic.Chess
{
    public struct SearchItem
    {
        public Move Move;
        public bool IsCheckingMove;
        public MovePair Killers;

        public SearchItem()
        {
            Move = Move.NullMove;
            IsCheckingMove = false;
            Killers = new();
        }
    }
}
