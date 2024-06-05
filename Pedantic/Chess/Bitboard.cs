// <copyright file="Bitboard.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Pedantic.Utilities;

    public readonly struct Bitboard : IEquatable<Bitboard>, IEnumerable<SquareIndex>
    {
        #region Nested Types

        public struct Enumerator : IEnumerator<SquareIndex>, IEnumerator, IDisposable
        {
            private readonly ulong bb;
            private ulong bbEnumerating;
            private SquareIndex current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Bitboard bitboard)
            {
                bb = bitboard.bb;
                bbEnumerating = bitboard.bb;
                current = SquareIndex.None;
            }

            public SquareIndex Current => current;

            object IEnumerator.Current => Current;

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if (bbEnumerating != 0)
                {
                    current = (SquareIndex)BitOps.TzCount(bbEnumerating);
                    bbEnumerating = BitOps.ResetLsb(bbEnumerating);
                    return true;
                }
                current = SquareIndex.None;
                return false;
            }

            public void Reset()
            {
                bbEnumerating = bb;
                current = SquareIndex.None;
            }
        }

        #endregion

        public const ulong FILE_A_MASK = 0x0101010101010101ul;
        public const ulong RANK_1_MASK = 0x00000000000000fful;

        private readonly ulong bb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard(ulong bitboard)
        {
            bb = bitboard;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard(SquareIndex sq, params SquareIndex[] squares)
        {
            bb = 1ul << (int)sq;
            foreach (SquareIndex sqIdx in squares)
            {
                bb |= 1ul << (int)sqIdx;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard(File file)
        {
            bb = FILE_A_MASK << (int)file;
        }

        public Bitboard(File file, params File[] files) : this(file)
        {
            for (int i = 0; i < files.Length; i++)
            {
                bb |= FILE_A_MASK << (int)files[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard(Rank rank)
        {
            bb = RANK_1_MASK << ((int)rank * 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard(Rank rank, params Rank[] ranks) : this(rank)
        {
            for (int i = 0; i < ranks.Length; i++)
            {
                bb |= RANK_1_MASK << ((int)ranks[i] * 8);
            }
        }

        public int TzCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BitOps.TzCount(bb);
        }

        public int LzCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BitOps.LzCount(bb);
        }

        public int PopCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BitOps.PopCount(bb);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard ResetLsb() => (Bitboard)BitOps.ResetLsb(bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard AndNot(Bitboard bitboard) => (Bitboard)BitOps.AndNot(bb, bitboard.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard AndNot(ulong bits) => (Bitboard)BitOps.AndNot(bb, bits);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TestBit(SquareIndex sq)
        {
            return ((1ul << (int)sq) & bb) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard SetBit(SquareIndex sq)
        {
            return (Bitboard)BitOps.SetBit(bb, (int)sq);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard ResetBit(SquareIndex sq)
        {
            return (Bitboard)BitOps.ResetBit(bb, (int)sq);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bitboard other)
        {
            return bb == other.bb;
        }

        public int this[SquareIndex sq]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BitOps.GetBit(bb, (int)sq);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null || obj is not Bitboard)
            {
                return false;
            }
            return Equals((Bitboard)obj);
        }

        public override int GetHashCode()
        {
            return bb.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            for (Rank rank = Rank.Rank8; rank >= Rank.Rank1; rank--)
            {
                for (File file = File.FileA; file <= File.FileH; file++)
                {
                    SquareIndex sq = ChessMath.ToSquareIndex(file, rank);
                    sb.Append(BitOps.GetBit(bb, (int)sq));
                    sb.Append(' ');
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<SquareIndex> IEnumerable<SquareIndex>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong (Bitboard bitboard) => bitboard.bb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Bitboard (ulong bb) => new Bitboard(bb);

        // bitwise operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator<< (Bitboard bitboard, int shift) => (Bitboard)(bitboard.bb << shift);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator>> (Bitboard bitboard, int shift) => (Bitboard)(bitboard.bb >> shift);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator& (Bitboard lhs, Bitboard rhs) => (Bitboard)(lhs.bb & rhs.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator| (Bitboard lhs, Bitboard rhs) => (Bitboard)(lhs.bb | rhs.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator^ (Bitboard lhs, Bitboard rhs) => (Bitboard)(lhs.bb ^ rhs.bb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard operator~ (Bitboard bitboard) => (Bitboard)~bitboard.bb;

        // logical operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator== (Bitboard lhs, Bitboard rhs) => lhs.Equals(rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator!= (Bitboard lhs, Bitboard rhs) => !lhs.Equals(rhs);

        public static readonly Bitboard All = new Bitboard(BB_ALL);
        public static readonly Bitboard None = new Bitboard(BB_NONE);
        public static readonly Bitboard BbFileA = new Bitboard(File.FileA);
        public static readonly Bitboard BbFileH = new Bitboard(File.FileH);
    }
}
