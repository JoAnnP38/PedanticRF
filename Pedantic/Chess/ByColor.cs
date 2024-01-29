using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static Pedantic.Chess.Constants;

namespace Pedantic.Chess
{
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
