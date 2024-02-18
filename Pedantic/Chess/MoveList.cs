using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pedantic.Utilities;
using static Pedantic.Chess.Constants;

namespace Pedantic.Chess
{
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

            public ReadOnlySpan<ScoredMove> AsReadOnlySpan()
            {
                return MemoryMarshal.CreateReadOnlySpan(ref _element0, CAPACITY);
            }

            public Span<ScoredMove> AsSpan()
            {
                return MemoryMarshal.CreateSpan(ref _element0, CAPACITY);
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
            Span<ScoredMove> scoredMoves = array.AsSpan().Slice(0, insertIndex);
            scoredMoves.Sort();
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
