// <copyright file="Square.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public readonly struct Square : IEquatable<Square>
    {
        private readonly byte bits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square()
        {
            bits = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square(Color color, Piece piece)
        {
            if (color == Color.None || piece == Piece.None)
            {
                bits = 0;
            }
            bits = (byte)(0x80 | (int)color | ((int)piece << 1));
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bits == 0;
        }

        public Color Color
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsEmpty ? Color.None : (Color)(bits & 0x01);
        }
        
        public Piece Piece 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsEmpty ? Piece.None : (Piece)((bits >> 1) & 0x07);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToFenString()
        {
            return IsEmpty ? string.Empty : Notation.ToFenPiece(Color, Piece);
        }

        public bool Equals(Square other)
        {
            return bits == other.bits;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            Square? sq = obj as Square?;
            if (sq == null)
            {
                return false;
            }
            return Equals(sq.Value);
        }

        public override int GetHashCode()
        {
            return bits.GetHashCode();
        }

        public static bool operator ==(Square lhs, Square rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Square lhs, Square rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static readonly Square Empty = new();
    }
}
