using System.Globalization;

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

        public static bool TryParse(ReadOnlySpan<char> sp, out SquareIndex index)
        {
            sp = sp.Trim();
            Span<char> lower = sp.Length < 32 ? stackalloc char[sp.Length] : new char[sp.Length];
            sp.ToLower(lower, CultureInfo.CurrentCulture);
            if (lower.Length >= 2 && lower[0] >= 'a' && lower[0] <= 'h' && lower[1] >= '1' && lower[1] <= '8')
            {
                index = ChessMath.ToSquareIndex(lower[0] - 'a', lower[1] - '1');
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
