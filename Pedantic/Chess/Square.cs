namespace Pedantic.Chess
{
    public readonly struct Square
    {
        private readonly byte bits;

        public Square()
        {
            bits = 0;
        }

        public Square(Color color, Piece piece)
        {
            if (color == Color.None || piece == Piece.None)
            {
                bits = 0;
            }
            bits = (byte)(0x80 | (int)color | ((int)piece << 1));
        }

        public bool IsEmpty => bits == 0;
        public Color Color => IsEmpty ? Color.None : (Color)(bits & 0x01);
        public Piece Piece => IsEmpty ? Piece.None : (Piece)((bits >> 1) & 0x07);

        public string ToFenString()
        {
            return IsEmpty ? string.Empty : Notation.ToFenPiece(Color, Piece);
        }

        public static readonly Square Empty = new();
    }
}
