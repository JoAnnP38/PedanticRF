using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Pedantic.Utilities;

using static Pedantic.Chess.ChessMath;

namespace Pedantic.Chess
{
    public readonly struct Move : IEquatable<Move>
    {
        private readonly uint move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move(Color stm, Piece piece, SquareIndex from, SquareIndex to, MoveType type = MoveType.Normal, Piece capture = Piece.None, Piece promote = Piece.None)
        { 
            move = Pack(stm, piece, from, to, type, capture, promote);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move(Color stm, Piece piece, int from, int to, MoveType type = MoveType.Normal, Piece capture = Piece.None, Piece promote = Piece.None)
        { 
            Util.Assert(IsValidSquare(from));
            Util.Assert(IsValidSquare(to));
            move = Pack(stm, piece, (SquareIndex)from, (SquareIndex)to, type, capture, promote);
        }           

        public readonly Color Stm
        {
            get
            {
                int c = BitOps.BitFieldExtract(move, 0, 2);
                return c == 0x03 ? Color.None : (Color)c;
            }
        }

        public readonly Piece Piece
        {
            get
            {
                int pc = BitOps.BitFieldExtract(move, 2, 3);
                return pc == 0x07 ? Piece.None : (Piece)pc;
            }
        }

        public readonly SquareIndex From
        {
            get
            {
                int from = BitOps.BitFieldExtract(move, 5, 7);
                return from == 0x07f ? SquareIndex.None : (SquareIndex)from;
            }
        }

        public readonly SquareIndex To
        {
            get
            {
                int to = BitOps.BitFieldExtract(move, 12, 7);
                return to == 0x07f ? SquareIndex.None : (SquareIndex)to;
            }
        }

        public readonly MoveType Type
        {
            get
            {
                return (MoveType)BitOps.BitFieldExtract(move, 19, 4);
            }
        }

        public readonly Piece Capture
        {
            get
            {
                int pc = BitOps.BitFieldExtract(move, 23, 3);
                return pc == 0x07 ? Piece.None : (Piece)pc;
            }
        }

        public readonly Piece Promote
        {
            get
            {
                int pc = BitOps.BitFieldExtract(move, 26, 3);
                return pc == 0x07 ? Piece.None : (Piece)pc;
            }
        }

        public readonly bool IsCapture
        {
            get
            {
                return Capture != Piece.None;
            }
        }

        public readonly bool IsPromote
        {
            get
            {
                return Promote != Piece.None;
            }
        }

        public readonly bool IsPawnMove
        {
            get
            {
                return Piece == Piece.Pawn;
            }
        }

        public readonly bool IsNoisy
        {
            get
            {
                return IsCapture || IsPromote;
            }
        }

        public readonly bool IsQuiet
        {
            get
            {
                return !IsNoisy;
            }
        }

        public readonly bool Equals(Move other)
        {
            return move == other.move;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null || obj is not Move)
            {
                return false;
            }
            return Equals((Move)obj);
        }

        public override string ToString()
        {
            string promote = IsPromote ? char.ToLower(Promote.ToChar()).ToString() : string.Empty;
            string fromString = From.ToString().ToLower();
            string toString = To.ToString().ToLower();
            return $"{fromString}{toString}{promote}";
        }

        public readonly string ToLongString()
        {
            return $"(Stm: {Stm}, Piece: {Piece}, From: {From}, To: {To}, Type: {Type}, Capture: {Capture}, Promote: {Promote})";
        }

        public override int GetHashCode()
        {
            return move.GetHashCode();
        }

        public static bool TryParse(Board board, string s, out Move move)
        {
            move = NullMove;

            try
            {
                if (s.Length < 4)
                {
                    throw new ArgumentException("Parameter too short to represent a valid move.", nameof(s));
                }

                if (!Conversions.TryParse(s[..2], out SquareIndex from))
                {
                    throw new ArgumentException("Invalid from square in move.", nameof(s));
                }

                if (!Conversions.TryParse(s[2..4], out SquareIndex to))
                {
                    throw new ArgumentException("Invalid to square in move.", nameof(s));
                }

                Piece promote = s.Length > 4 ? Conversions.ParsePiece(s[4]) : Piece.None;

                MoveList moveList = new();
                board.GenerateMoves(moveList);

                for (int n = 0; n < moveList.Count; ++n)
                {
                    Move mv = moveList[n];
                    string mvString = mv.ToString();
                    if (from == mv.From && to == mv.To && promote == mv.Promote)
                    {
                        bool legal = board.MakeMove(mv);
                        if (legal)
                        {
                            board.UnmakeMove();
                            move = mv;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.TraceError(ex.ToString());
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator==(Move lhs, Move rhs)
        {
            return lhs.Equals(rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator!=(Move lhs, Move rhs)
        {
            return !lhs.Equals(rhs);
        }

        public readonly void Unpack(out Color stm, out Piece piece, out SquareIndex from, out SquareIndex to, out MoveType type, 
            out Piece capture, out Piece promote)
        {
            uint data = move;
            uint val = data & 0x03;
            stm = val == 0x03 ? Color.None : (Color)val;
            data >>= 2;
            val = data & 0x07;
            piece = val == 0x07 ? Piece.None : (Piece)val;
            data >>= 3;
            val = data & 0x07f;
            from = val == 0x07f ? SquareIndex.None : (SquareIndex)val;
            data >>= 7;
            val = data & 0x07f;
            to = val == 0x07f ? SquareIndex.None : (SquareIndex)val;
            data >>= 7;
            type = (MoveType)(data & 0x0f);
            data >>= 4;
            val = data & 0x07;
            capture = val == 0x07 ? Piece.None : (Piece)val;
            data >>= 3;
            val = data & 0x07;
            promote = val == 0x07 ? Piece.None : (Piece)val;
        }

        private static uint Pack(Color stm, Piece piece, SquareIndex from, SquareIndex to, MoveType type, Piece capture, Piece promote)
        {
            uint move = ((uint)stm & 0x03) 
                      | (((uint)piece & 0x07) << 2)
                      | (((uint)from & 0x07f) << 5)
                      | (((uint)to & 0x07f) << 12)
                      | (((uint)type & 0x0f) << 19)
                      | (((uint)capture & 0x07) << 23)
                      | (((uint)promote & 0x07) << 26)
                      ;

            return move;
        }

        public static readonly Move NullMove = new(Color.None, Piece.None, SquareIndex.None, SquareIndex.None, MoveType.Null);
    }
}
