using System.Collections;

using Pedantic.Utilities;

namespace Pedantic.Collections
{
    public class ValueList<T> : IValueList<T> where T : struct, IComparable<T>, IEquatable<T>
    {
        private T[] array;
        private int capacity;
        private int count;

        public ValueList()
        {
            count = 0;
            capacity = 16;
            array = new T[capacity];
        }

        public ValueList(int capacity)
        {
            Util.Assert(capacity > 0);
            count = 0;
            this.capacity = capacity;
            array = new T[capacity];
        }

        public ref T this[int index]
        {
            get
            {
                Util.Assert(index >= 0 && index < count);
                return ref array[index];
            }
        }

        public int Count
        {
            get => count;
        }

        public int Capacity
        {
            get => capacity;
        }

        public bool IsFull
        {
            get => count == capacity;
        }

        public void Add(ref T item)
        {
            if (count >= capacity)
            {
                Grow();
            }
            array[count++] = item;
        }

        public void Clear()
        {
            count = 0;
        }

        public bool Contains(T item)
        {
            return Array.Exists(array, (i) => i.CompareTo(item) == 0);
        }

        public void CopyTo(T[] other, int otherIndex)
        {
            Array.Copy(array, 0, other, otherIndex, count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int n = 0; n < count; n++)
            {
                yield return array[n];
            }

            yield break;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(array, item, 0, count);
        }

        public void Insert(int index, ref T item)
        {
            Util.Assert(index >= 0 && index < count);

            if (count >= capacity)
            {
                Grow();
            }

            Array.Copy(array, index, array, index + 1, count - index);
            array[index] = item;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            Util.Assert(index >= 0 && index < count);

            Array.Copy(array, index + 1, array, index, --count - index);
        }

        public void Sort()
        {
            Array.Sort(array, 0, count);
        }

        public Span<T> AsSpan()
        {
            return new(array, 0, count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Grow()
        {
            int newCapacity = capacity * 2;
            T[] newArray = new T[newCapacity];
            Array.Copy(array, newArray, count);
            array = newArray;
            capacity = newCapacity;
        }
    }
}
