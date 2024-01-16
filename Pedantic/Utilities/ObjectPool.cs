using Pedantic.Collections;

namespace Pedantic.Utilities
{
    public class ObjectPool<T> where T : class, IPooledObject<T>, new()
    {
        private readonly Bag<T> objects;
        private readonly Func<T> create;

        public ObjectPool(Func<T> create, int capacity, int preallocate = 0)
        {
            objects = new Bag<T>(capacity);
            this.create = create;

            for (int i = 0; i < preallocate; ++i)
            {
                Return(create());
            }
        }

        public ObjectPool(int capacity, int preallocate = 0)
            : this(() => new(), capacity, preallocate)
        { }

        public T Rent()
        {
            if (objects.TryTake(out T? item))
            {
                return item ?? create();
            }

            return create();
        }

        public void Return(T item)
        {
            item.Clear();
            objects.Add(item);
        }
    }
}
