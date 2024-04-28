// <copyright file="Move.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Pedantic.Utilities;
    using static Pedantic.Chess.ChessMath;

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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Move(uint mvValue)
        {
            move = mvValue;
        }

        public readonly Color Stm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int c = BitOps.BitFieldExtract(move, 0, 2);
                return c == 0x03 ? Color.None : (Color)c;
            }
        }

        public readonly Piece Piece
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int pc = BitOps.BitFieldExtract(move, 2, 3);
                return pc == 0x07 ? Piece.None : (Piece)pc;
            }
        }

        public readonly SquareIndex From
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int from = BitOps.BitFieldExtract(move, 5, 7);
                return from == 0x07f ? SquareIndex.None : (SquareIndex)from;
            }
        }

        public readonly SquareIndex To
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int to = BitOps.BitFieldExtract(move, 12, 7);
                return to == 0x07f ? SquareIndex.None : (SquareIndex)to;
            }
        }

        public readonly MoveType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (MoveType)BitOps.BitFieldExtract(move, 19, 4);
            }
        }

        public readonly Piece Capture
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int pc = BitOps.BitFieldExtract(move, 23, 3);
                return pc == 0x07 ? Piece.None : (Piece)pc;
            }
        }

        public readonly Piece Promote
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int pc = BitOps.BitFieldExtract(move, 26, 3);
                return pc == 0x07 ? Piece.None : (Piece)pc;
            }
        }

        public readonly bool IsValid
        {
            get
            {
                return  Stm >= Color.None && Stm <= Color.Black &&
                        Piece >= Piece.None && Piece <= Piece.King &&
                        From >= SquareIndex.None && From <= SquareIndex.H8 &&
                        To >= SquareIndex.None && To <= SquareIndex.H8 &&
                        Type >= MoveType.Normal && Type <= MoveType.Null &&
                        Capture >= Piece.None && Capture <= Piece.Queen &&
                        Promote >= Piece.None && Promote <= Piece.Queen;
            }
        }

        public readonly bool IsCapture
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Capture != Piece.None;
            }
        }

        public readonly bool IsPromote
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Promote != Piece.None;
            }
        }

        public readonly bool IsPawnMove
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Piece == Piece.Pawn;
            }
        }

        public readonly bool IsNoisy
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsCapture || IsPromote;
            }
        }

        public readonly bool IsQuiet
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return !IsNoisy;
            }
        }

        public readonly bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return move == NullMove.move;
            }
        }

        public readonly bool IsPromotionThreat
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsPromote || (IsPawnMove && To.Normalize(Stm).Rank() == Rank.Rank7);
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
            Span<char> chars = stackalloc char[5];
            chars[0] = From.File().ToUciChar();
            chars[1] = From.Rank().ToUciChar();
            chars[2] = To.File().ToUciChar();
            chars[3] = To.Rank().ToUciChar();
            if (IsPromote)
            {
                chars[4] = Promote.ToUciChar();
                return chars.ToString();
            }
            return chars.Slice(0, 4).ToString();
        }

        public readonly string ToLongString()
        {
            return $"(Stm: {Stm}, Piece: {Piece}, From: {From}, To: {To}, Type: {Type}, Capture: {Capture}, Promote: {Promote})";
        }

        public override int GetHashCode()
        {
            return move.GetHashCode();
        }

        public static bool TryParse(Board board, ReadOnlySpan<char> sp, out Move move)
        {
            move = NullMove;

            try
            {
                if (sp.Length < 4)
                {
                    throw new ArgumentException("Parameter too short to represent a valid move.", nameof(sp));
                }

                if (!Conversions.TryParse(sp[..2], out SquareIndex from))
                {
                    throw new ArgumentException("Invalid from square in move.", nameof(sp));
                }

                if (!Conversions.TryParse(sp[2..4], out SquareIndex to))
                {
                    throw new ArgumentException("Invalid to square in move.", nameof(sp));
                }

                Piece promote = sp.Length > 4 ? Conversions.ParsePiece(sp[4]) : Piece.None;

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
        public static bool TryParse(Board board, string s, out Move move)
        {
            return TryParse(board, s.AsSpan(), out move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint (Move move) => move.move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Move (uint mv) => new Move(mv);

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
