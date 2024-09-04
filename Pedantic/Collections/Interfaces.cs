// <copyright file="Interfaces.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

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

    public interface IValueList<T> : IEnumerable<T>
    {
        public ref T this[int index] { get; }

        public int Count { get; }

        public void Add(ref T item);

        public void Clear();

        public bool Contains(T item);

        public void CopyTo(T[] array, int arrayIndex);

        public int IndexOf(T item);

        public void Insert(int index, ref T item);

        public bool Remove(T item);

        public void RemoveAt(int index);

        public void Sort();
    }
}
