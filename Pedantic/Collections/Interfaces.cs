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
}
