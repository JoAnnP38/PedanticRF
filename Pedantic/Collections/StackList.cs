// <copyright file="StackList.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Collections
{
    using System.Runtime.CompilerServices;
    using Pedantic.Utilities;

    public ref struct StackList<T> where T : unmanaged
    {
        private int count;
        private Span<T> list;

        public StackList(Span<T> buffer)
        {
            count = 0;
            list = buffer;
            list.Clear();
        }

        public void Add(T item)
        {
            if (count + 1 < list.Length)
            {
                list[count++] = item;
            }
        }

        public void Clear()
        {
            count = 0;
            list.Clear();
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Util.Assert(index >= 0 && index < count);
                return list[index];
            }
        }
    }
}
