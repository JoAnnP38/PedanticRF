using System.Runtime.CompilerServices;

namespace Pedantic.Chess
{
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
