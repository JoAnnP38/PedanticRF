using Pedantic.Chess;
using Pedantic.Chess.HCE;
using Pedantic.Collections;
using Pedantic.Utilities;
using System.Runtime.CompilerServices;
using static Pedantic.Chess.HCE.Weights;
using static System.Formats.Asn1.AsnWriter;

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

                SquareIndex KI = evalInfo[c].KI;
                KingBuckets kb = new (color, bd.KingIndex[color], bd.KingIndex[color.Flip()]);
                Bitboard pawns = evalInfo[c].Pawns;
                Bitboard otherPawns = evalInfo[o].Pawns;
                Bitboard allPawns = pawns | otherPawns;
                Bitboard pawnRams = (color == Color.White ? otherPawns >> 8 : otherPawns << 8);

                foreach (SquareIndex from in bd.Units(color))
                {
                    Bitboard sqMask = new Bitboard(from);
                    SquareIndex normalFrom = from.Normalize(color);
                    Piece piece = bd.PieceBoard(from).Piece;
                    phase += piece.PhaseValue();

                    IncrementPieceCount(color, coefficients, piece);
                    IncrementPieceSquare(color, coefficients, piece, kb, normalFrom);
                    Bitboard pieceAttacks = Board.GetPieceMoves(piece, from, bd.All);
                    if (piece != Piece.Pawn && piece != Piece.King)
                    {
                        evalInfo[c].AttackBy[(int)piece] |= pieceAttacks;
                        evalInfo[c].AttackBy[AttackBy.All] |= pieceAttacks;
                        if (pieceAttacks != 0 && evalInfo[c].AttackCount < HceEval.MAX_ATTACK_LEN)
                        {
                            evalInfo[c].Attacks[evalInfo[c].AttackCount++] = pieceAttacks;
                        }
                    }
                    int mobility = (pieceAttacks & evalInfo[c].MobilityArea).PopCount;
                    IncrementPieceMobility(color, coefficients, piece, mobility);

                    if (piece == Piece.Pawn)
                    {
                        Ray ray = Board.Vectors[(int)from];
                        Bitboard friendMask = color == Color.White ? ray.North : ray.South;
                        
                        if ((otherPawns & HceEval.PassedPawnMasks[c, (int)from]) == 0 && (pawns & friendMask) == 0)
                        {
                            IncrementPassedPawn(color, coefficients, normalFrom);
                            evalInfo[c].PassedPawns |= sqMask;
                        }

                        if ((pawns & HceEval.AdjacentPawnMasks[(int)from]) != 0)
                        {
                            IncrementPhalanxPawn(color, coefficients, normalFrom);
                        }

                        if ((evalInfo[c].AttackBy[AttackBy.Pawn] & sqMask) != 0)
                        {
                            IncrementChainedPawn(color, coefficients, normalFrom);
                        }

                        if ((pawnRams & sqMask) != 0)
                        {
                            IncrementPawnRam(color, coefficients, normalFrom);
                        }

                        if ((pawns & HceEval.IsolatedPawnMasks[(int)from]) == 0)
                        {
                            IncrementIsolatedPawn(color, coefficients, normalFrom);
                        }
                    }
                    else if (piece == Piece.Rook)
                    {
                        Bitboard fileMask = new Bitboard(from.File());
                        if ((allPawns & fileMask) == 0)
                        {
                            IncrementRookOnOpenFile(color, coefficients);
                        }

                        if ((pawns & fileMask) == 0 && (otherPawns & fileMask) != 0)
                        {
                            IncrementRookOnHalfOpenFile(color, coefficients);
                        }
                    }
                }

                SquareIndex enemyKI = evalInfo[o].KI;
                for (int n = 0; n < evalInfo[c].AttackCount; n++)
                {
                    Bitboard attacks = evalInfo[c].Attacks[n].AndNot(evalInfo[o].AttackBy[AttackBy.Pawn]);
                    int count = (attacks & (Bitboard)HceEval.KingProximity[0, (int)enemyKI]).PopCount;
                    IncrementKingAttack(color, coefficients, 0, count);
                    count = (attacks & (Bitboard)HceEval.KingProximity[1, (int)enemyKI]).PopCount;
                    IncrementKingAttack(color, coefficients, 1, count);
                }

                if ((evalInfo[c].CanCastle & 0x01) != 0)
                {
                    IncrementCanCastleKS(color, coefficients);
                }

                if ((evalInfo[c].CanCastle & 0x02) != 0)
                {
                    IncrementCanCastleQS(color, coefficients);
                }

                if (bd.DiagonalSliders(other) != 0)
                {
                    Bitboard attacks = Board.GetBishopMoves(KI, bd.All);
                    int mobility = (attacks & evalInfo[o].MobilityArea).PopCount;
                    IncrementKsDiagonalMobility(color, coefficients, mobility);
                }

                if (bd.OrthogonalSliders(other) != 0)
                {
                    Bitboard attacks = Board.GetRookMoves(KI, bd.All);
                    int mobility = (attacks & evalInfo[o].MobilityArea).PopCount;
                    IncrementKsOrthogonalMobility(color, coefficients, mobility);
                }

                Bitboard bishops = bd.Pieces(color, Piece.Bishop);
                int bishopCount = bishops.PopCount;
                if (bishopCount >= 2)
                {
                    IncrementBishopPair(color, coefficients);
                }
                else if (bishopCount == 1)
                {
                    SquareIndex sq = (SquareIndex)bishops.TzCount;
                    Bitboard badPawnMask = sq.IsDark() ? 
                        (Bitboard)HceEval.DARK_SQUARES_MASK : (Bitboard)HceEval.LITE_SQUARES_MASK;
                    int badPawnCount = (evalInfo[c].Pawns & badPawnMask).PopCount;
                    IncrementBadBishopPawn(color, coefficients, badPawnCount);
                }

                Bitboard targets = bd.Units(other).AndNot(otherPawns);
                Bitboard pushAttacks;
                
                if (color == Color.White)
                {
                    Bitboard pawnPushes = (pawns << 8).AndNot(bd.All);
                    pushAttacks = (pawnPushes.AndNot(Bitboard.BbFileA) << 7) |
                                  (pawnPushes.AndNot(Bitboard.BbFileH) << 9);
                }
                else
                {
                    Bitboard pawnPushes = (pawns >> 8).AndNot(bd.All);
                    pushAttacks = (pawnPushes.AndNot(Bitboard.BbFileH) >> 7) |
                                  (pawnPushes.AndNot(Bitboard.BbFileA) >> 9);
                }

                foreach (SquareIndex sq in pushAttacks & targets)
                {
                    Piece threatenedPiece = bd.PieceBoard(sq).Piece;
                    IncrementPushedPawnThreat(color, coefficients, threatenedPiece);
                }

                foreach (SquareIndex sq in evalInfo[c].AttackBy[AttackBy.Pawn] & targets)
                {
                    Piece threatenedPiece = bd.PieceBoard(sq).Piece;
                    IncrementPawnThreat(color, coefficients, threatenedPiece);
                }

                targets &= ~bd.Kings;
                Bitboard minorAttacks = evalInfo[c].AttackBy[AttackBy.Knight] | evalInfo[c].AttackBy[AttackBy.Bishop];

                foreach (SquareIndex sq in minorAttacks & targets)
                {
                    Piece threatenedPiece = bd.PieceBoard(sq).Piece;
                    IncrementMinorThreat(color, coefficients, threatenedPiece);
                }

                targets &= ~evalInfo[o].AttackBy[AttackBy.Pawn];
                Bitboard rookAttacks = evalInfo[c].AttackBy[AttackBy.Rook];

                foreach (SquareIndex sq in rookAttacks & targets)
                {
                    Piece threatenedPiece = bd.PieceBoard(sq).Piece;
                    IncrementRookThreat(color, coefficients, threatenedPiece);
                }

                if (color == bd.SideToMove)
                {
                    SetTempoBonus(color, coefficients);
                }
            }

            for (Color color = Color.White; color <= Color.Black; color++)
            {
                Color other = color.Flip();
                int c = (int)color;
                int o = (int)other;


                foreach (SquareIndex ppIndex in evalInfo[c].PassedPawns)
                {
                    Rank normalRank = ppIndex.Normalize(color).Rank();
                    if (normalRank < Rank.Rank4)
                    {
                        continue;
                    }

                    if (bd.PieceCount(other) == 1)
                    {
                        SquareIndex promoteSq = ChessMath.ToSquareIndex(ppIndex.File(), HceEval.PromoteRank(color));
                        if (ChessMath.Distance(ppIndex, promoteSq) <
                            ChessMath.Distance(evalInfo[o].KI, promoteSq) - (other == bd.SideToMove ? 1 : 0))
                        {
                            IncrementKingOutsidePasserSquare(color, coefficients);
                        }
                    }

                    Bitboard advanceMask = new Bitboard(Board.PawnPlus(color, ppIndex));
                    Bitboard attacksMask = evalInfo[o].AttackBy[AttackBy.All];

                    if ((advanceMask & bd.All) == 0 && (advanceMask & attacksMask) == 0)
                    {
                        IncrementPassedPawnCanAdvance(color, coefficients, normalRank);
                    }

                    Direction behind = color == Color.White ? Direction.South : Direction.North;
                    if ((HceEval.GetAttack(bd, ppIndex, behind) & bd.Pieces(color, Piece.Rook)) != 0)
                    {
                        IncrementRookBehindPassedPawn(color, coefficients);
                    }
                }

                Bitboard ePassedPawns = evalInfo[o].PassedPawns;
                if (other == Color.White)
                {
                    ePassedPawns <<= 8;
                }
                else
                {
                    ePassedPawns >>= 8;
                }

                Bitboard blockers = bd.Units(color) & ePassedPawns;
                foreach (SquareIndex sq in blockers)
                {
                    Piece blocker = bd.PieceBoard(sq).Piece;
                    Rank normalRank = sq.Normalize(other).Rank();
                    IncrementBlockedPassedPawn(color, coefficients, blocker, normalRank - 1);
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

        private static void IncrementPhalanxPawn(Color color, SparseArray<short> v, SquareIndex sq)
        {
            int index = PHALANX_PAWN + (int)sq;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementChainedPawn(Color color, SparseArray<short> v, SquareIndex sq)
        {
            int index = CHAINED_PAWN + (int)sq;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementPawnRam(Color color, SparseArray<short> v, SquareIndex sq)
        {
            int index = PAWN_RAM + (int)sq;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementIsolatedPawn(Color color, SparseArray<short> v, SquareIndex sq)
        {
            int index = ISOLATED_PAWN + (int)sq;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementKingAttack(Color color, SparseArray<short> v, int dist, int count)
        {
            int index = KING_ATTACK + dist;
            if (v.ContainsKey(index))
            {
                v[index] += (short)(count * Increment(color));
            }
            else
            {
                v[index] = (short)(count * Increment(color));
            }
        }

        private static void IncrementCanCastleKS(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(CAN_CASTLE_KS))
            {
                v[CAN_CASTLE_KS] += Increment(color);
            }
            else
            {
                v.Add(CAN_CASTLE_KS, Increment(color));
            }
        }

        private static void IncrementCanCastleQS(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(CAN_CASTLE_QS))
            {
                v[CAN_CASTLE_QS] += Increment(color);
            }
            else
            {
                v.Add(CAN_CASTLE_QS, Increment(color));
            }
        }
        
        private static void IncrementKsDiagonalMobility(Color color, SparseArray<short> v, int count)
        {
            if (count <= 0)
            {
                return;
            }

            int index = KS_DIAG_MOBILITY + count - 1;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementKsOrthogonalMobility(Color color, SparseArray<short> v, int count)
        {
            if (count <= 0)
            {
                return;
            }

            int index = KS_ORTH_MOBILITY + count - 1;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementKingOutsidePasserSquare(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(KING_OUTSIDE_PP_SQUARE))
            {
                v[KING_OUTSIDE_PP_SQUARE] += Increment(color);
            }
            else
            {
                v.Add(KING_OUTSIDE_PP_SQUARE, Increment(color));
            }
        }

        private static void IncrementPassedPawnCanAdvance(Color color, SparseArray<short> v, Rank rank)
        {
            int index = PP_CAN_ADVANCE + (rank - Rank.Rank4);
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementBlockedPassedPawn(Color color, SparseArray<short> v, Piece blocker, Rank rank)
        {
            int index = BLOCKED_PASSED_PAWN + ((int)blocker - 1) * MAX_COORDS + (int)rank;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementRookBehindPassedPawn(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(ROOK_BEHIND_PASSER))
            {
                v[ROOK_BEHIND_PASSER] += Increment(color);
            }
            else
            {
                v.Add(ROOK_BEHIND_PASSER, Increment(color));
            }
        }

        private static void IncrementBishopPair(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(BISHOP_PAIR))
            {
                v[BISHOP_PAIR] += Increment(color);
            }
            else
            {
                v.Add(BISHOP_PAIR, Increment(color));
            }
        }

        private static void IncrementBadBishopPawn(Color color, SparseArray<short> v, int count)
        {
            if (count <= 0)
            {
                return;
            }

            if (v.ContainsKey(BAD_BISHOP_PAWN))
            {
                v[BAD_BISHOP_PAWN] += (short)(count * Increment(color));
            }
            else
            {
                v.Add(BAD_BISHOP_PAWN, (short)(count * Increment(color)));
            }
        }

        private static void IncrementRookOnOpenFile(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(ROOK_ON_OPEN_FILE))
            {
                v[ROOK_ON_OPEN_FILE] += Increment(color);
            }
            else
            {
                v.Add(ROOK_ON_OPEN_FILE, Increment(color));
            }
        }

        private static void IncrementRookOnHalfOpenFile(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(ROOK_ON_HALF_OPEN_FILE))
            {
                v[ROOK_ON_HALF_OPEN_FILE] += Increment(color);
            }
            else
            {
                v.Add(ROOK_ON_HALF_OPEN_FILE, Increment(color));
            }
        }

        private static void IncrementPieceSquare(Color color, SparseArray<short> v, Piece piece, KingBuckets kb, SquareIndex square)
        {
            int index = FRIENDLY_KB_PST + 
                ((int)piece * MAX_KING_BUCKETS + kb.Friendly) * MAX_SQUARES + (int)square;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }

            index = ENEMY_KB_PST +
                ((int)piece * MAX_KING_BUCKETS + kb.Enemy) * MAX_SQUARES + (int)square;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementPushedPawnThreat(Color color, SparseArray<short> v, Piece threatenedPiece)
        {
            int index = PAWN_PUSH_THREAT + (int)threatenedPiece;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementPawnThreat(Color color, SparseArray<short> v, Piece threatenedPiece)
        {
            int index = PAWN_THREAT + (int)threatenedPiece;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementMinorThreat(Color color, SparseArray<short> v, Piece threatenedPiece)
        {
            int index = MINOR_THREAT + (int)threatenedPiece;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementRookThreat(Color color, SparseArray<short> v, Piece threatenedPiece)
        {
            int index = ROOK_THREAT + (int)threatenedPiece;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void SetTempoBonus(Color color, SparseArray<short> v)
        {
            v[TEMPO] = Increment(color);
        }
    }
}
