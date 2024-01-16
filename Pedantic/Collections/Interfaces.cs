namespace Pedantic.Collections
{
    public interface IStack<T> : ICollection<T>
    {
        public ref T Peek();
        public ref T Pop();
        public void Push(ref T item);
        public bool TryPeek(out T item);
        public bool TryPop(out T item);
    }
}
