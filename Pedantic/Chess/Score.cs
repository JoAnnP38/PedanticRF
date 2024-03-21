using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Pedantic.Chess
{
    public readonly struct Score : IEquatable<Score>, IComparable<Score>
    {
        private readonly int score;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score(short mgScore, short egScore)
        {
            score = (int)(((uint)egScore << 16) + mgScore);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Score(int scoreValue)
        {
            score = scoreValue;
        }

        public short MgScore
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (short)score;
        }

        public short EgScore 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (short)((uint)(score + 0x8000) >> 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short NormalizeScore(int phase)
        {
            return (short)((MgScore * phase + EgScore * (MAX_PHASE - phase)) / MAX_PHASE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short NormalizeScore(int phase, int scale, int divisor)
        {
            int egScaled = EgScore * scale / divisor;
            return (short)((MgScore * phase + egScaled * (MAX_PHASE - phase)) / MAX_PHASE);
        }

        public bool Equals(Score other)
        {
            return score == other.score;
        }

        public int CompareTo(Score other)
        {
            return score - other.score;
        }

        public override string ToString()
        {
            return $"({MgScore}, {EgScore})";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            Score? score = obj as Score?;
            if (score == null)
            {
                return false;
            }
            else
            {
                return Equals(score);
            }
        }

        public override int GetHashCode()
        {
            return score.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Score s) => s.score;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Score(int score) => new(score);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Score lhs, Score rhs) => lhs.score == rhs.score;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Score lhs, Score rhs) => lhs.score != rhs.score;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Score operator +(Score lhs, Score rhs) => (Score)(lhs.score + rhs.score);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Score operator -(Score lhs, Score rhs) => (Score)(lhs.score - rhs.score);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Score operator -(Score s) => s * -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Score operator *(int lhs, Score rhs) => (Score)(lhs * rhs.score);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Score operator *(Score lhs, int rhs) => (Score)(lhs.score * rhs);

        public readonly static Score Zero = new Score(0);
    }
}
