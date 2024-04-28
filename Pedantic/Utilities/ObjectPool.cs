// <copyright file="ObjectPool.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Utilities
{
    using Pedantic.Collections;

    public class ObjectPool<T> where T : class, new()
    {
        private readonly Bag<T> objects;
        private readonly Func<T> create;
        private readonly Action<T> reset;

        public ObjectPool(Func<T> create, Action<T> reset, int capacity, int preallocate = 0)
        {
            objects = new Bag<T>(capacity);
            this.create = create;
            this.reset = reset;

            for (int i = 0; i < preallocate; ++i)
            {
                Return(create());
            }
        }

        public ObjectPool(int capacity, int preallocate = 0)
            : this(() => new(), (o) => { }, capacity, preallocate)
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
            reset(item);
            objects.Add(item);
        }
    }
}
