using System.Collections;
using System.Runtime.InteropServices;
using Pedantic.Utilities;

namespace Pedantic.Collections
{
    public unsafe class FixedArray<T> : IEnumerable<T> where T : unmanaged
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            internal Enumerator(FixedArray<T> array)
            {
                pBegin = array.pArray;
                pEnd = array.pArray + array.length;
                pEnumerating = pEnd;
                current = default;
            }

            public T Current => current;

            object IEnumerator.Current => Current;

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if (pEnumerating != pEnd)
                {
                    current = *pEnumerating++;
                    return true;
                }
                current = default;
                return false;
            }

            public void Reset()
            {
                pEnumerating = pBegin;
                current = default;
            }

            private T* pBegin;
            private T* pEnd;
            private T* pEnumerating;
            private T current;
        }

        public FixedArray(int length, bool fill = false)
        {
            this.length = length;
            insertIndex = fill ? length : 0;
            byteCount = (nuint)(length * sizeof(T));
            pArray = (T*)NativeMemory.AllocZeroed(byteCount);
        }

        ~FixedArray()
        {
            if (pArray != null)
            {
                NativeMemory.Free(pArray);
                pArray = null;
            }
        }

        public ref T this[int index]
        {
            get
            {
                Util.Assert(index >= 0 && index < insertIndex, $"index: {index}, insertIndex: {insertIndex}");
                return ref pArray[index];
            }
        }

        public int Count => insertIndex;
        public int Length => length;

        public void Add(T item)
        {
            Util.Assert(insertIndex < length);
            pArray[insertIndex++] = item;
        }

        public void Clear()
        {
            NativeMemory.Clear(pArray, byteCount);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (pArray != null)
            {
                NativeMemory.Free(pArray);
                pArray = null;
            }
            GC.SuppressFinalize(this);
        }

        private readonly int length;
        private readonly nuint byteCount;
        private int insertIndex;
        private T* pArray;
    }
}
