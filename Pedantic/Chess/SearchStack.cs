using System.Runtime.CompilerServices;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public unsafe class SearchStack
    {
        public const int OFFSET = 4;
        private SearchItem[] searchStack;

        public SearchStack()
        {
            searchStack = new SearchItem[MAX_PLY + OFFSET];
            Array.Fill(searchStack, new SearchItem());
        }

        public ref SearchItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Util.Assert(index >= -4 && index < MAX_PLY);
                return ref searchStack[index + OFFSET];
            }
        }

        public void Initialize(Board board, History history)
        {
            Clear();
            searchStack[0].Continuation = history.NullMoveContinuation;
            searchStack[1].Continuation = history.NullMoveContinuation;
            searchStack[2].Move = board.PrevLastMove;
            searchStack[2].Continuation = history.GetContinuation(board.PrevLastMove);
            searchStack[3].Move = board.LastMove;
            searchStack[3].Continuation = history.GetContinuation(board.LastMove);
            searchStack[3].IsCheckingMove = board.IsChecked();
        }

        private void Clear()
        {
            Array.Fill(searchStack, new SearchItem());
        }
    }
}
