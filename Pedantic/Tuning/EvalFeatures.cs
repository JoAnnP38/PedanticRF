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
            Span<HceEval.EvalInfo> evalInfo = stackalloc HceEval.EvalInfo[2];
            HceEval.InitializeEvalInfo(bd, evalInfo);

            sideToMove = bd.SideToMove;
            phase = 0;

            for (Color color = Color.White; color <= Color.Black; color++)
            {
                Color other = color.Flip();
                int c = (int)color;
                int o = (int)other;

                KingBuckets kb = new (color, bd.KingIndex[color], bd.KingIndex[color.Flip()]);
                Bitboard pawns = evalInfo[c].Pawns;
                Bitboard otherPawns = evalInfo[o].Pawns;

                foreach (SquareIndex from in bd.Units(color))
                {
                    SquareIndex normalFrom = from.Normalize(color);
                    Piece piece = bd.PieceBoard(from).Piece;
                    phase += piece.PhaseValue();

                    IncrementPieceCount(color, coefficients, piece);
                    SetPieceSquare(color, coefficients, piece, kb, normalFrom);
                    Bitboard pieceAttacks = Board.GetPieceMoves(piece, from, bd.All);
                    evalInfo[c].PieceAttacks |= pieceAttacks;
                    int mobility = (pieceAttacks & evalInfo[c].MobilityArea).PopCount;
                    IncrementPieceMobility(color, coefficients, piece, mobility);

                    if (piece == Piece.Pawn)
                    {
                        Ray ray = Board.Vectors[(int)from];
                        Bitboard friendMask = color == Color.White ? ray.North : ray.South;
                        
                        if ((otherPawns & HceEval.PassedPawnMasks[c, (int)from]) == 0 && (pawns & friendMask) == 0)
                        {
                            IncrementPassedPawn(color, coefficients, normalFrom);
                            evalInfo[c].PassedPawns |= new Bitboard(from);
                        }
                    }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short Increment(Color color)
        {
            return (short)(1 - ((int)color << 1));
        }

        private static void IncrementPieceCount(Color color, SparseArray<short> v, Piece piece)
        {
            if (piece == Piece.King)
            {
                return;
            }

            int index = PIECE_VALUES + (int)piece;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementPieceMobility(Color color, SparseArray<short> v, Piece piece, int mobility)
        {
            if (piece == Piece.Pawn || piece == Piece.King)
            {
                return;
            }

            int index = mobility + piece switch
            {
                Piece.Knight => KNIGHT_MOBILITY,
                Piece.Bishop => BISHOP_MOBILITY,
                Piece.Rook => ROOK_MOBILITY,
                Piece.Queen => QUEEN_MOBILITY,
                _ => throw new InvalidOperationException($"Invalid piece: {piece}")
            };

            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementPassedPawn(Color color, SparseArray<short> v, SquareIndex sq)
        {
            int index = PASSED_PAWN + (int)sq;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void SetPieceSquare(Color color, SparseArray<short> v, Piece piece, KingBuckets kb, SquareIndex square)
        {
            int index = FRIENDLY_KB_PST + 
                ((int)piece * MAX_KING_BUCKETS + kb.Friendly) * MAX_SQUARES + (int)square;
            v[index] = Increment(color);

            index = ENEMY_KB_PST +
                ((int)piece * MAX_KING_BUCKETS + kb.Enemy) * MAX_SQUARES + (int)square;
            v[index] = Increment(color);
        }

        private static void SetTempoBonus(Color color, SparseArray<short> v)
        {
            v[TEMPO] = Increment(color);
        }
    }
}
