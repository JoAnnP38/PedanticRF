using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public static partial class Notation
    {
        public const string REGEX_FEN = @"^\s*([rnbqkpRNBQKP1-8]+/){7}[rnbqkpRNBQKP1-8]+\s[bw]\s(-|K?Q?k?q?)\s(-|[a-h][36])\s\d+\s\d+\s*$";
        public const string REGEX_MOVE = @"^[a-h][1-8][a-h][1-8](n|b|r|q)?$";
        public const string REGEX_INDEX = @"^-|[a-h][1-8]$";
        public const string FEN_EMPTY = @"8/8/8/8/8/8/8/8 w - - 0 0";
        public const string FEN_START_POS = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string FEN_KIWI_PETE = @"r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidFen(string fen)
        {
            return fenRegex.IsMatch(fen);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidFen(ReadOnlySpan<char> fen)
        {
            return fenRegex.IsMatch(fen);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFenPiece(Color color, Piece piece)
        {
            return piece.ToChar(color).ToString();
        }

        public static (Color color, Piece piece) ParseFenPiece(char ch)
        {
            Color color = char.IsLower(ch) ? Color.Black : Color.White;
            Piece piece = char.ToUpper(ch) switch
            {
                'P' => Piece.Pawn,
                'N' => Piece.Knight,
                'B' => Piece.Bishop,
                'R' => Piece.Rook,
                'Q' => Piece.Queen,
                'K' => Piece.King,
                _ => throw new ArgumentException($"Character '{ch}' is not a valid FEN piece.")
            };
            return (color, piece);
        }

        public static Color ParseFenColor(string s)
        {
            Util.Assert(!string.IsNullOrEmpty(s));

            return s switch
            {
                "w" => Color.White,
                "b" => Color.Black,
                _ => throw new ArgumentException($"'{s}' is not a valid FEN color.")
            };
        }

        public static Color ParseFenColor(ReadOnlySpan<char> colorToMove)
        {
            Util.Assert(colorToMove.Length > 0);

            return colorToMove[0] switch
            {
                'w' => Color.White,
                'b' => Color.Black,
                _ => throw new ArgumentException($"'{colorToMove[0]}' is not a valid FEN color.")
            };
        }

        public static CastlingRights ParseFenCastlingRights(string s)
        {
            Util.Assert(!string.IsNullOrEmpty(s));

            CastlingRights castling = CastlingRights.None;
            for (int n = 0; n < s.Length; n++)
            {
                CastlingRights cr = s[n] switch
                {
                    'K' => CastlingRights.WhiteKingSide,
                    'Q' => CastlingRights.WhiteQueenSide,
                    'k' => CastlingRights.BlackKingSide,
                    'q' => CastlingRights.BlackQueenSide,
                    '-' => CastlingRights.None,
                    _ => throw new ArgumentException($"Illegal character found in FEN castling availability '{s[n]}'.")
                };
                castling |= cr;
            }

            return castling;
        }

        public static CastlingRights ParseFenCastlingRights(ReadOnlySpan<char> castleRights)
        {
            Util.Assert(castleRights.Length > 0);

            CastlingRights castling = CastlingRights.None;
            for (int n = 0; n < castleRights.Length; n++)
            {
                CastlingRights cr = castleRights[n] switch
                {
                    'K' => CastlingRights.WhiteKingSide,
                    'Q' => CastlingRights.WhiteQueenSide,
                    'k' => CastlingRights.BlackKingSide,
                    'q' => CastlingRights.BlackQueenSide,
                    '-' => CastlingRights.None,
                    _ => throw new ArgumentException($"Illegal character found in FEN castling availability '{castleRights[n]}'.")
                };
                castling |= cr;
            }

            return castling;
        }

        public static SquareIndex ParseFenEnPassant(string s)
        {
            Util.Assert(s.Length > 0);

            if (s[0] == '-')
            {
                return SquareIndex.None;
            }
            return ParseSquareIndex(s);
        }

        public static SquareIndex ParseFenEnPassant(ReadOnlySpan<char> ep)
        {
            Util.Assert(ep.Length > 0);

            if (ep[0] == '-')
            {
                return SquareIndex.None;
            }
            return ParseSquareIndex(ep);
        }

        public static SquareIndex ParseSquareIndex(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return SquareIndex.None;
            }

            if (!indexRegex.IsMatch(s))
            {
                return SquareIndex.None;
            }

            return ChessMath.ToSquareIndex((File)(s[0] - 'a'), (Rank)(s[1] - '1'));
        }

        public static SquareIndex ParseSquareIndex(ReadOnlySpan<char> index)
        {
            if (index.Length < 2)
            {
                return SquareIndex.None;
            }

            if (!indexRegex.IsMatch(index))
            {
                return SquareIndex.None;
            }

            return ChessMath.ToSquareIndex((File)(index[0] - 'a'), (Rank)(index[1] - '1'));
        }

        private static readonly Regex fenRegex = FenRegex();
        private static readonly Regex indexRegex = IndexRegex();

        [GeneratedRegex(REGEX_FEN, RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex FenRegex();
        [GeneratedRegex(REGEX_INDEX, RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex IndexRegex();
    }
}
