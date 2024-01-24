using System.Diagnostics.CodeAnalysis;

namespace Pedantic.Chess
{
    public readonly struct Score : IEquatable<Score>, IComparable<Score>
    {
        private readonly int score;

        public Score(short mgScore, short egScore)
        {
            score = (int)(((uint)egScore << 16) + mgScore);
        }

        private Score(int scoreValue)
        {
            score = scoreValue;
        }

        public short MgScore => (short)score;
        public short EgScore => (short)((uint)(score + 0x8000) >> 16);

        public short NormalizeScore(int phase)
        {
            return (short)((MgScore * phase + EgScore * (Constants.MAX_PHASE - phase)) / Constants.MAX_PHASE);
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

        public static implicit operator int(Score s) => s.score;
        public static explicit operator Score(int score) => new(score);

        public static bool operator ==(Score lhs, Score rhs) => lhs.score == rhs.score;
        public static bool operator !=(Score lhs, Score rhs) => lhs.score != rhs.score;
        public static Score operator +(Score lhs, Score rhs) => (Score)(lhs.score + rhs.score);
        public static Score operator -(Score lhs, Score rhs) => (Score)(lhs.score - rhs.score);
        public static Score operator -(Score s) => s * -1;
        public static Score operator *(int lhs, Score rhs) => (Score)(lhs * rhs.score);
        public static Score operator *(Score lhs, int rhs) => (Score)(lhs.score * rhs);

        public readonly static Score Zero = new Score(0);
    }
}
