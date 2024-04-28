// <copyright file="ByColor.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [InlineArray(MAX_COLORS)]
    public struct ByColor<T> where T : notnull
    {
        public const int LENGTH = MAX_COLORS;
        private T _element0;

        public T this[Color color]
        {
            get => this[(int)color];
            set => this[(int)color] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return MemoryMarshal.CreateSpan(ref _element0, LENGTH);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            AsSpan().Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value)
        {
            this[0] = value;
            this[1] = value;
        }
    }
}
