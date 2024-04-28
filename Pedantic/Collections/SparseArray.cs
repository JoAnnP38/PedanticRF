// <copyright file="SparseArray.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Collections
{
    public class SparseArray<T> : SortedList<int, T> where T : unmanaged
    {
        public SparseArray() { }

        public SparseArray(IDictionary<int, T> other) : base(other) { }

        public SparseArray(int capacity) : base(capacity) { }
    }
}
