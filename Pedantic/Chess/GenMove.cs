using System.Runtime.CompilerServices;

namespace Pedantic.Chess
{
    public readonly struct GenMove
    {
        public GenMove(Move move, MoveGenPhase phase)
        {
            Move = move;
            MovePhase = phase;
        }

        public Move Move
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get; 

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init;
        }

        public MoveGenPhase MovePhase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init;
        }
    }
}