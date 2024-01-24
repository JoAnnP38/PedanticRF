using System.Runtime.CompilerServices;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public static class ColorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Flip(this Color other)
        {
            return other == Color.None ? Color.None : (Color)((int)other ^ 1);
        }

        public static string ToFenString(this Color color)
        {
            Util.Assert(color != Color.None);
            return fenColor[(int)color];
        }

        private static string[] fenColor = [ "w", "b" ];
    }

    public static class PieceExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDiagonalSlider(this Piece piece)
        {
            return piece == Piece.Bishop || piece == Piece.Queen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOrthogonalSlider(this Piece piece)
        {
            return piece == Piece.Rook || piece == Piece.Queen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSlider(this Piece piece)
        {
            return piece.IsDiagonalSlider() || piece.IsOrthogonalSlider();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Value(this Piece piece)
        {
            return pieceValue[(int)piece + 1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short PhaseValue(this Piece piece)
        {
            return phaseValue[(int)piece + 1];
        }

        public static char ToChar(this Piece piece, Color color = Color.None)
        {
            char pcChar = pieceChar[(int)piece + 1];
            if (color == Color.Black)
            {
                pcChar = char.ToLower(pcChar);
            }
            return pcChar;
        }

        public static char ToUciChar(this Piece piece)
        {
            return char.ToLower(pieceChar[(int)piece + 1]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(this Piece piece)
        {
            return pieceString[(int)piece + 1];
        }

        private static short[] pieceValue = [ 0, 100, 300, 300, 500, 900, 9900 ];
        private static sbyte[] phaseValue = [ 0, 1, 2, 2, 4, 8, 0 ];
        private static char[] pieceChar = [ ' ', 'P', 'N', 'B', 'R', 'Q', 'K' ];
        private static string[] pieceString = [ "None", "Pawn", "Knight", "Bishop", "Rook", "Queen", "King" ];
    }

    public static class CastlingRightsExtensions
    {
        public static string ToFenString(this CastlingRights castling)
        {
            return crString[(int)castling];
        }

        private static string[] crString =
        [
            "-", "K", "Q", "KQ", "k", "Kk", "Qk", "KQk", "q", "Kq", "Qq", "KQq", "kq", "Kkq", "Qkq", "KQkq"
        ];
    }

    public static class DirectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOrthogonal(this Direction direction)
        {
            return ((int)direction & 0x01) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDiagonal(this Direction direction)
        {
            return ((int)direction & 0x01) == 0x01;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction Reverse(this Direction direction)
        {
            return reverse[(int)direction + 1];
        }

        private static Direction[] reverse = 
        [ 
            Direction.None,
            Direction.South,
            Direction.SouthWest,
            Direction.West,
            Direction.NorthWest,
            Direction.North,
            Direction.NorthEast,
            Direction.East,
            Direction.SouthEast
        ];
    }

    public static class RankExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToUciChar(this Rank rank)
        {
            return (char)('1' + (int)rank);
        }
    }

    public static class FileExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToUciChar(this File file)
        {
            return (char)('a' + (int)file);
        }
    }

    public static class SquareIndexExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File File(this SquareIndex sq)
        {
            return (File)((int)sq & 0x07);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank Rank(this SquareIndex sq)
        {
            return (Rank)((int)sq / 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Bucket(this SquareIndex sq)
        {
            return sq == SquareIndex.None ? (sbyte)-1 : (sbyte)((sbyte)sq.Rank() * 2 + (sbyte)sq.File());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (File File, Rank Rank) ToCoords(this SquareIndex sq)
        {
            return (sq.File(), sq.Rank());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(this SquareIndex sq)
        {
            return algebraicIndices[(int)sq + 1];
        }

        public static string ToFenString(this SquareIndex sq)
        {
            return sq == SquareIndex.None ? "-" : ToString(sq);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDark(this SquareIndex sq)
        {
            var coords = sq.ToCoords();
            return (((int)coords.File + (int)coords.Rank) & 0x01) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLight(this SquareIndex sq)
        {
            return !sq.IsDark();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SquareIndex Normalize(this SquareIndex sq, Color side)
        {
            return side == Color.White ? sq : (SquareIndex)((int)sq ^ 56);
        }

        private static readonly string[] algebraicIndices =
        {
            string.Empty,
            "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
            "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
            "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8"
        };
    }
}