using Pedantic.Chess;
using Pedantic.Chess.HCE;
using Pedantic.Collections;
using Pedantic.Utilities;
using System.Runtime.CompilerServices;
using static Pedantic.Chess.HCE.Weights;

namespace Pedantic.Tuning
{
    public unsafe sealed class EvalFeatures
    {
        private Color sideToMove;
        private short phase;
        private readonly SparseArray<short> coefficients = new();

        public EvalFeatures(Board bd)
        {
            sideToMove = bd.SideToMove;
            phase = 0;

            for (Color color = Color.White; color <= Color.Black; color++)
            {
                int c = (int)color;
                KingBuckets kb = new (color, bd.KingIndex[color], bd.KingIndex[color.Flip()]);

                foreach (SquareIndex from in bd.Units(color))
                {
                    SquareIndex normalFrom = from.Normalize(color);
                    Piece piece = bd.PieceBoard(from).Piece;
                    phase += piece.PhaseValue();

                    IncrementPieceCount(color, coefficients, piece);
                    SetPieceSquare(color, coefficients, piece, kb, normalFrom);
                }

                if (color == bd.SideToMove)
                {
                    SetTempoBonus(color, coefficients);
                }
            }
        }

        public IDictionary<int, short> Coefficients
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => coefficients;
        }

        public Color SideToMove
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => sideToMove;
        }

        public short Phase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => phase;
        }

        public short Compute(Weights weights, int start = PIECE_VALUES, int end = MAX_WEIGHTS)
        {
            try
            {
                Score computeScore = Score.Zero;
                IEnumerable<KeyValuePair<int, short>> coeffs = coefficients.Where(kvp => kvp.Key >= start && kvp.Key < end);
                foreach (var coeff in coeffs)
                {
                    computeScore += coeff.Value * weights[coeff.Key];
                }
                short score = computeScore.NormalizeScore(phase);
                return sideToMove == Color.White ? score : (short)-score;
            }
            catch (Exception ex)
            {
                Util.TraceError(ex.ToString());
                throw new Exception("EvalFeatures.Compute error occurred.", ex);
            }
        }

        public short Compute(Weights weights, int[] keys)
        {
            try
            {
                Score computeScore = Score.Zero;
                foreach (int key in keys)
                {
                    computeScore += coefficients[key] * weights[key];
                }
                short score = computeScore.NormalizeScore(phase);
                return sideToMove == Color.White ? score : (short)-score;
            }
            catch (Exception ex)
            {
                Util.TraceError(ex.ToString());
                throw new Exception("EvalFeatures.Compute error occurred.", ex);
            }
        }

        private static void IncrementPieceCount(Color color, SparseArray<short> v, Piece piece)
        {
            if (piece == Piece.King)
            {
                return;
            }

            short inc = (short)(color == Color.White ? 1 : -1);

            int index = PIECE_VALUES + (int)piece;
            if (v.ContainsKey(index))
            {
                v[index] += inc;
            }
            else
            {
                v.Add(index, inc);
            }
        }

        private static void SetPieceSquare(Color color, SparseArray<short> v, Piece piece, KingBuckets kb, SquareIndex square)
        {
            short inc = (short)(color == Color.White ? 1 : -1);
            int index = FRIENDLY_KB_PST + 
                ((int)piece * MAX_KING_BUCKETS + kb.Friendly) * MAX_SQUARES + (int)square;
            v[index] = inc;

            index = ENEMY_KB_PST +
                ((int)piece * MAX_KING_BUCKETS + kb.Enemy) * MAX_SQUARES + (int)square;
            v[index] = inc;
        }

        private static void SetTempoBonus(Color color, SparseArray<short> v)
        {
            short inc = (short)(color == Color.White ? 1 : -1);
            v[TEMPO] = inc;
        }
    }
}
