namespace Pedantic.Chess
{
    public static class Conversions
    {
        public static bool TryParse(string s, out SquareIndex index)
        {
            s = s.Trim().ToLower();
            if (s.Length >= 2 && s[0] >= 'a' && s[0] <= 'h' && s[1] >= '1' && s[1] <= '8')
            {
                index = ChessMath.ToSquareIndex(s[0] - 'a', s[1] - '1');
                return true;
            }

            index = SquareIndex.None;
            return false;
        }

        public static Piece ParsePiece(char c)
        {
            return char.ToLower(c) switch
            {
                'p' => Piece.Pawn,
                'n' => Piece.Knight,
                'b' => Piece.Bishop,
                'r' => Piece.Rook,
                'q' => Piece.Queen,
                'k' => Piece.King,
                _ => Piece.None
            };
        }
    }
}
