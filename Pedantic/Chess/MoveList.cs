﻿// <copyright file="MoveList.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess
{
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Pedantic.Utilities;

    public sealed class MoveList : IEnumerable<Move>, IEnumerable, IPooledObject<MoveList>
    {
        #region Nested Types

        public struct ScoredMove : IComparable<ScoredMove>
        {
            public Move Move 
            { 
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get; 

                init; 
            }

            public int Score 
            { 
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get; 

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set; 
            }

            public int CompareTo(ScoredMove other)
            {
                // reverse ordering for sort
                return other.Score - Score;
            }
        }

        public const int CAPACITY = 218;

        [InlineArray(CAPACITY)]
        public struct ScoredMoveArray
        {
            private ScoredMove _element0;

            public ReadOnlySpan<ScoredMove> AsReadOnlySpan(int length = CAPACITY)
            {
                return MemoryMarshal.CreateReadOnlySpan(ref _element0, length);
            }

            public Span<ScoredMove> AsSpan(int length = CAPACITY)
            {
                return MemoryMarshal.CreateSpan(ref _element0, length);
            }
        }

        private class FakeHistory : IHistory
        {
            public short this[Move move] => 0;

            public short this[Color stm, Piece piece, SquareIndex to] => 0;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList()
        {
            history = new FakeHistory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList(IHistory history)
        {
            this.history = history;
        }

        public int Count => insertIndex;

        public IHistory History
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => history;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => history = value;
        }

        public Move this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Util.Assert(index >= 0 && index < insertIndex);
                return array[index].Move;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetScore(int index)
        {

            Util.Assert(index >= 0 && index < insertIndex);
            return array[index].Score;
        }

        public void SetScore(int index, int score)
        {
            Util.Assert(index >= 0 && index < insertIndex);
            array[index].Score = score;
        }

        public void Add(Move move)
        {
            Util.Assert(insertIndex < CAPACITY);
            int score;
            if (move.IsCapture)
            {
                score = CaptureScore(move.Capture, move.Piece, move.Promote);
            }
            else if (move.IsPromote)
            {
                score = PromoteScore(move.Promote);
            }
            else
            {
                score = history[move];
            }

            array[insertIndex++] = new ScoredMove { Move = move, Score = score };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IEnumerable<Move> moves)
        {
            foreach (Move move in moves)
            {
                Add(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddQuiet(Color stm, Piece piece, SquareIndex from, SquareIndex to, MoveType type = MoveType.Normal)
        {
            Util.Assert(insertIndex < CAPACITY);
            Move move = new(stm, piece, from, to, type);
            array[insertIndex++] = new ScoredMove { Move = move, Score = history[stm, piece, to] };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPromote(Color stm, SquareIndex from, SquareIndex to, Piece promote)
        {
            Util.Assert(insertIndex < CAPACITY);
            Move move = new(stm, Piece.Pawn, from, to, MoveType.Promote, promote: promote);
            array[insertIndex++] = new ScoredMove { Move = move, Score = PromoteScore(promote) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCapture(Color stm, Piece piece, SquareIndex from, SquareIndex to, MoveType type, Piece capture, Piece promote = Piece.None)
        {
            Util.Assert(insertIndex < CAPACITY);
            Move move = new(stm, piece, from, to, type, capture, promote);
            array[insertIndex++] = new ScoredMove { Move = move, Score = CaptureScore(capture, piece, promote) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            insertIndex = 0;
        }

        public Move Sort(int n)
        {
            int largest = -1;
            int largestScore = int.MinValue;
            for (int i = n; i < insertIndex; i++)
            {
                if (array[i].Score > largestScore)
                {
                    largest = i;
                    largestScore = array[i].Score;
                }
            }

            if (largest > n)
            {
                (array[n], array[largest]) = (array[largest], array[n]);
            }

            return array[n].Move;
        }

        public void SortAll()
        {
            // use a custom selection sort over the Array.Sort or Span.Sort
            // so that sort is stable and preserves some of the initial move
            // ordering for moves with equal scores
            for (int n = 1; n < insertIndex; n++)
            {
                ScoredMove key = array[n];

                int m = n - 1;
                for (; m >= 0 && array[m].Score < key.Score; m--)
                {
                    array[m + 1] = array[m];
                }
                array[m + 1] = key;
            }

            // remove any illegal moves in the list (their score will be set to int.MinValue)
            int i = insertIndex - 1;
            while (array[i--].Score == int.MinValue)
            {
                --insertIndex;
            }
        }

        public void RemoveIllegals()
        {
            for (int n = insertIndex - 1; n >= 0 && array[n].Score == int.MinValue; n--)
            {
                --insertIndex;
            }
        }

        public bool Remove(Move move)
        {
            for (int n = 0; n < insertIndex; n++)
            {
                if (array[n].Move == move)
                {
                    array[n] = array[--insertIndex];
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<ScoredMove> ScoredMoves
        {
            get
            {
                return array.AsSpan(insertIndex).ToArray();
            }
        }

        public IEnumerator<Move> GetEnumerator()
        {
            for (int n = 0; n < insertIndex; n++)
            {
                yield return array[n].Move;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CaptureScore(Piece captured, Piece attacker, Piece promote = Piece.None)
        {
            return CAPTURE_BONUS + promote.Value() + ((int)captured << 3) + (MAX_PIECES - (int)attacker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PromoteScore(Piece promote)
        {
            return PROMOTE_BONUS + promote.Value();
        }

        private ScoredMoveArray array;
        private int insertIndex;
        private IHistory history;
    }
}
