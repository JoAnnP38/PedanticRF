using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public unsafe class SearchStack : IDisposable
    {
        public const int OFFSET = 4;
        public const nuint MEM_ALIGN = 64;
        private SearchItem* searchStack;
        private nuint byteCount;

        public SearchStack()
        {
            byteCount = (nuint)((MAX_PLY + OFFSET) * sizeof(SearchItem));
            searchStack = (SearchItem*)NativeMemory.AlignedAlloc(byteCount, MEM_ALIGN);
            for (SearchItem* p = searchStack; p < searchStack + (MAX_PLY + OFFSET); p++)
            {
                *p = new SearchItem();
            }
        }

        ~SearchStack()
        {
            if (searchStack != null)
            {
                NativeMemory.AlignedFree(searchStack);
                searchStack = null;
            }
        }

        public ref SearchItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Util.Assert(index >= -4 && index < MAX_PLY);
                return ref *(searchStack + (index + OFFSET));
            }
        }

        public void Initialize(Board board)
        {
            SearchItem* p = searchStack + 2;
            p->Move = board.PrevLastMove;
            p++;
            p->Move = board.LastMove;
            p->IsCheckingMove = board.IsChecked();
        }

        public void Clear()
        {
            NativeMemory.Clear(searchStack, byteCount);
        }

        public void Dispose()
        {
            if (searchStack != null)
            {
                NativeMemory.AlignedFree(searchStack);
                searchStack = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
