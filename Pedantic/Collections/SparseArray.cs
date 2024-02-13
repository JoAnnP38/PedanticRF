namespace Pedantic.Collections
{
    public class SparseArray<T> : SortedList<int, T> where T : unmanaged
    {
        public SparseArray() { }
        public SparseArray(IDictionary<int, T> other) : base(other) { }
        public SparseArray(int capacity) : base(capacity) { }
    }
}
