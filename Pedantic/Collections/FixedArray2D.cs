using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Pedantic.Utilities;

namespace Pedantic.Collections
{
    public unsafe class FixedArray2D<T> : IEnumerable<T> where T : unmanaged
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            internal Enumerator(FixedArray2D<T> array)
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

        public FixedArray2D(int dim1, int dim2, bool fill = false)
        {
            this.dim1 = dim1;
            this.dim2 = dim2;
            length = dim1 * dim2;
            insertIndex = fill ? length : 0;
            byteCount = (nuint)(length * sizeof(T));
            pArray = (T*)NativeMemory.AllocZeroed(byteCount);
        }

        ~FixedArray2D()
        {
            if (pArray != null)
            {
                NativeMemory.Free(pArray);
                pArray = null;
            }
        }

        public void Add(T item)
        {
            Util.Assert(insertIndex < length);
            pArray[insertIndex++] = item;
        }

        public void Clear()
        {
            NativeMemory.Clear(pArray, byteCount);
        }

        public ref T this[int i, int j]
        {
            get
            {
                Debug.Assert(i >= 0 && i < dim1);
                Debug.Assert(j >= 0 && j < dim2);
                return ref pArray[i * dim2 + j];
            }
        }

        public int GetDimension(int dim)
        {
            return dim switch
            {
                0 => dim1,
                1 => dim2,
                _ => throw new InvalidOperationException("Array2D only supports two dimensions [0-1].")
            };
        }

        public int Count => insertIndex;

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
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
        private readonly int dim1, dim2;
        private readonly nuint byteCount;
        private int insertIndex;
        private T* pArray;
    }
}
