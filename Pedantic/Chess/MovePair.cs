using System.Runtime.CompilerServices;

namespace Pedantic.Chess
{
    public struct MovePair
    {
        private Move move1;
        private Move move2;

        public MovePair()
        {
            move1 = Move.NullMove;
            move2 = Move.NullMove;
        }

        public Move Move1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => move1;
        }

        public Move Move2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => move2;
        }

        public void Add(Move move)
        {
            if (move == move2)
            {
                (move1, move2) = (move2, move1);
            }
            else
            {
                move2 = move1;
                move1 = move;
            }
        }
    }
}
