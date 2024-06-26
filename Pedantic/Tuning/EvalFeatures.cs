﻿// <copyright file="EvalFeatures.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Tuning
{
    using System.Runtime.CompilerServices;
    using Pedantic.Chess;
    using Pedantic.Chess.HCE;
    using Pedantic.Collections;
    using Pedantic.Utilities;
    using static Pedantic.Chess.HCE.Weights;

    public unsafe sealed class EvalFeatures
    {
        private Color sideToMove;
        private short phase;
        private (sbyte Scale, sbyte Divisor) drawRatio;
        private readonly SparseArray<short> coefficients = new();

        public EvalFeatures(Board bd, Weights wts)
        {
            ByColor<HceEval.EvalInfo> evalInfo = new();
            HceEval.InitializeEvalInfo(bd, ref evalInfo);

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
                Bitboard blockedPawns = pawns & (color == Color.White ? bd.All >> 8 : bd.All << 8);
                Bitboard fileMask;

                foreach (SquareIndex from in bd.Units(color))
                {
                    Bitboard sqMask = new Bitboard(from);
                    SquareIndex normalFrom = from.Normalize(color);
                    Piece piece = bd.PieceBoard(from).Piece;
                    phase += piece.PhaseValue();

                    IncrementPieceCount(color, coefficients, piece);
                    IncrementPieceSquare(color, coefficients, piece, kb, normalFrom);
                    if (piece != Piece.Pawn && piece != Piece.King)
                    {
                        Bitboard occupied = bd.All;
                        if (piece == Piece.Bishop)
                        {
                            occupied ^= bd.DiagonalSliders(color);
                        }
                        else if (piece == Piece.Rook)
                        {
                            occupied ^= bd.OrthogonalSliders(color);
                        }

                        Bitboard pieceAttacks = Board.GetPieceMoves(piece, from, occupied);
                        evalInfo[c].AttackByTwo |= evalInfo[c].AttackBy[AttackBy.All] & pieceAttacks;
                        evalInfo[c].AttackBy[(int)piece] |= pieceAttacks;
                        evalInfo[c].AttackBy[AttackBy.All] |= pieceAttacks;
                        if (pieceAttacks != 0 && evalInfo[c].AttackCount < HceEval.MAX_ATTACK_LEN)
                        {
                            evalInfo[c].Attacks[evalInfo[c].AttackCount++] = pieceAttacks;
                        }
                        bool _ = bd.IsPinned(from, out Bitboard pinMask);
                        int mobility = (pieceAttacks & pinMask & evalInfo[c].MobilityArea).PopCount;
                        IncrementPieceMobility(color, coefficients, piece, mobility);
                    }


                    if (piece == Piece.Pawn)
                    {
                        Ray ray = Board.Vectors[(int)from];
                        Bitboard friendMask = color == Color.White ? ray.North : ray.South;
                        bool canBeBackward = true;

                        if ((otherPawns & HceEval.PassedPawnMasks[c, (int)from]) == 0 && (pawns & friendMask) == 0)
                        {
                            IncrementPassedPawn(color, coefficients, normalFrom);
                            evalInfo[c].PassedPawns |= sqMask;
                            canBeBackward = false;
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
                            canBeBackward = false;
                        }

                        if (canBeBackward && (pawns & HceEval.BackwardPawnMasks[c, (int)from]) == 0)
                        {
                            IncrementBackwardPawn(color, coefficients);
                        }
                    }
                    else if (piece == Piece.Rook)
                    {
                        fileMask = new Bitboard(from.File());
                        if ((allPawns & fileMask) == 0)
                        {
                            IncrementRookOnOpenFile(color, coefficients);
                        }

                        if ((pawns & fileMask) == 0 && (otherPawns & fileMask) != 0)
                        {
                            IncrementRookOnHalfOpenFile(color, coefficients);
                        }

                        if ((blockedPawns & fileMask) != 0)
                        {
                            IncrementRookOnBlockedFile(color, coefficients);
                        }
                    }
                }

                fileMask = Bitboard.BbFileA;
                for (File f = File.FileA; f <= File.FileH; f++, fileMask <<= 1)
                {
                    int count = (pawns & fileMask).PopCount;
                    if (count > 1)
                    {
                        IncrementDoubledPawn(color, coefficients, --count);
                    }
                }

                Bitboard knights = bd.Pieces(color, Piece.Knight);
                Bitboard bishops = bd.Pieces(color, Piece.Bishop);
                Bitboard shieldPawns = color == Color.White ? (pawns >> 8) : (pawns << 8);
                Bitboard minorPieces = knights | bishops;
                IncrementPawnShieldsMinor(color, coefficients, (shieldPawns & minorPieces).PopCount);

                Bitboard outposts = HceEval.Outposts[color] & evalInfo[c].AttackBy[AttackBy.Pawn];
                outposts = outposts.AndNot(evalInfo[o].AttackBy[AttackBy.Pawn] | evalInfo[o].AttackBy[AttackBy.PawnPush]);
                IncrementMinorOutpost(color, coefficients, Piece.Knight, (knights & outposts).PopCount);
                IncrementMinorOutpost(color, coefficients, Piece.Bishop, (bishops & outposts).PopCount);

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

                if ((pawns & HceEval.FlankMask[(int)KI.File()]).PopCount == 0)
                {
                    IncrementPawnlessFlank(color, coefficients);
                }

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

                foreach (SquareIndex sq in bishops)
                {
                    if ((Board.GetBishopMoves(sq, bd.Pawns) & (Bitboard)HceEval.CENTER_MASK).PopCount == 2)
                    {
                        IncrementBishopLongDiagonal(color, coefficients);
                    }
                }

                Bitboard targets = bd.Units(other).AndNot(otherPawns);
                Bitboard pushAttacks = evalInfo[c].AttackBy[AttackBy.PawnPush];

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

                Bitboard toSquares = (~bd.Units(color)).AndNot(evalInfo[o].AttackBy[AttackBy.Pawn]);
                SquareIndex enemyKI = evalInfo[o].KI;

                // check threats from knights
                Bitboard checkThreats = Board.KnightMoves(enemyKI) & toSquares;
                foreach (SquareIndex from in bd.Pieces(color, Piece.Knight))
                {
                    int count = (Board.KnightMoves(from) & checkThreats).PopCount;
                    IncrementCheckThreat(color, coefficients, Piece.Knight, count);
                }

                // check threats from bishops
                checkThreats = Board.GetBishopMoves(enemyKI, bd.All) & toSquares;
                foreach (SquareIndex from in bd.Pieces(color, Piece.Bishop))
                {
                    int count = (Board.GetBishopMoves(from, bd.All) & checkThreats).PopCount;
                    IncrementCheckThreat(color, coefficients, Piece.Bishop, count);
                }

                // check threats from rooks
                checkThreats = Board.GetRookMoves(enemyKI, bd.All) & toSquares;
                foreach (SquareIndex from in bd.Pieces(color, Piece.Rook))
                {
                    int count = (Board.GetRookMoves(from, bd.All) & checkThreats).PopCount;
                    IncrementCheckThreat(color, coefficients, Piece.Rook, count);
                }

                // check threats from queens
                checkThreats = Board.GetQueenMoves(enemyKI, bd.All) & toSquares;
                foreach (SquareIndex from in bd.Pieces(color, Piece.Queen))
                {
                    int count = (Board.GetQueenMoves(from, bd.All) & checkThreats).PopCount;
                    IncrementCheckThreat(color, coefficients, Piece.Queen, count);
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


                Bitboard defended = evalInfo[o].AttackBy[AttackBy.Pawn] | (evalInfo[o].AttackByTwo & ~evalInfo[c].AttackByTwo);
                SquareIndex enemyKI = evalInfo[o].KI;
                Bitboard attacks = evalInfo[c].AttackBy[AttackBy.PawnLeft].AndNot(defended);
                int count1 = (attacks & (Bitboard)HceEval.KingProximity[0, (int)enemyKI]).PopCount;
                int count2 = (attacks & (Bitboard)HceEval.KingProximity[1, (int)enemyKI]).PopCount;

                attacks = evalInfo[c].AttackBy[AttackBy.PawnRight].AndNot(defended);
                count1 += (attacks & (Bitboard)HceEval.KingProximity[0, (int)enemyKI]).PopCount;
                count2 += (attacks & (Bitboard)HceEval.KingProximity[1, (int)enemyKI]).PopCount;

                for (int n = 0; n < evalInfo[c].AttackCount; n++)
                {
                    attacks = evalInfo[c].Attacks[n].AndNot(defended);
                    count1 += (attacks & (Bitboard)HceEval.KingProximity[0, (int)enemyKI]).PopCount;
                    count2 += (attacks & (Bitboard)HceEval.KingProximity[1, (int)enemyKI]).PopCount;
                }

                IncrementKingAttack1(color, coefficients, count1);
                IncrementKingAttack2(color, coefficients, count2);

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
                    if ((Board.GetAttack(ppIndex, behind, bd.All) & bd.Pieces(color, Piece.Rook)) != 0)
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
            Score score = ComputeImpl(wts);
            drawRatio = HceEval.CalcDrawRatio(bd, in evalInfo, score);
        }

        public IDictionary<int, short> Coefficients
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => coefficients;
        }

        public (sbyte Scale, sbyte Divisor) DrawRatio
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => drawRatio;
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

        public short Compute(Weights weights, int start = PIECE_VALUES, int end = MAX_WEIGHTS, bool egScaling = true)
        {
            try
            {
                Score score = ComputeImpl(weights, start, end);
                short normScore = egScaling ? 
                    score.NormalizeScore(phase, drawRatio.Scale, drawRatio.Divisor) :
                    score.NormalizeScore(phase);

                return sideToMove == Color.White ? normScore : (short)-normScore;
            }
            catch (Exception ex)
            {
                Util.TraceError(ex.ToString());
                throw new Exception("EvalFeatures.Compute error occurred.", ex);
            }
        }

        private Score ComputeImpl(Weights weights, int start = PIECE_VALUES, int end = MAX_WEIGHTS)
        {
            Score computeScore = Score.Zero;
            IEnumerable<KeyValuePair<int, short>> coeffs = coefficients.Where(kvp => kvp.Key >= start && kvp.Key < end);
            foreach (var coeff in coeffs)
            {
                computeScore += coeff.Value * weights[coeff.Key];
            }
            return computeScore;        
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

        private static void IncrementBackwardPawn(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(BACKWARD_PAWN))
            {
                v[BACKWARD_PAWN] += Increment(color);
            }
            else
            {
                v.Add(BACKWARD_PAWN, Increment(color));
            }
        }

        private static void IncrementDoubledPawn(Color color, SparseArray<short> v, int count)
        {
            if (count <= 0)
            {
                return;
            }
            if (v.ContainsKey(DOUBLED_PAWN))
            {
                v[DOUBLED_PAWN] += (short)(count * Increment(color));
            }
            else
            {
                v.Add(DOUBLED_PAWN, (short)(count * Increment(color)));
            }
        }

        private static void IncrementKingAttack1(Color color, SparseArray<short> v, int count)
        {
            count = Math.Clamp(count, 0, 8);
            int index = KING_ATTACK_1 + count;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
            }
        }

        private static void IncrementKingAttack2(Color color, SparseArray<short> v, int count)
        {
            count = Math.Clamp(count, 0, 8);
            int index = KING_ATTACK_2 + count;
            if (v.ContainsKey(index))
            {
                v[index] += Increment(color);
            }
            else
            {
                v.Add(index, Increment(color));
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

        private static void IncrementPawnlessFlank(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(PAWNLESS_FLANK))
            {
                v[PAWNLESS_FLANK] += Increment(color);
            }
            else
            {
                v.Add(PAWNLESS_FLANK, Increment(color));
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

        private static void IncrementRookOnBlockedFile(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(ROOK_ON_BLOCKED_FILE))
            {
                v[ROOK_ON_BLOCKED_FILE] += Increment(color);
            }
            else
            {
                v.Add(ROOK_ON_BLOCKED_FILE, Increment(color));
            }
        }

        private static void IncrementPawnShieldsMinor(Color color, SparseArray<short> v, int count)
        {
            if (count <= 0)
            {
                return;
            }

            if (v.ContainsKey(PAWN_SHIELDS_MINOR))
            {
                v[PAWN_SHIELDS_MINOR] += (short)(count * Increment(color));
            }
            else
            {
                v.Add(PAWN_SHIELDS_MINOR, (short)(count * Increment(color)));
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

        private static void IncrementCheckThreat(Color color, SparseArray<short> v, Piece checker, int count)
        {
            if (count <= 0)
            {
                return;
            }

            int index = CHECK_THREAT + (int)checker;

            if (v.ContainsKey(index))
            {
                v[index] += (short)(count * Increment(color));
            }
            else
            {
                v.Add(index, (short)(count * Increment(color)));
            }
        }

        private static void IncrementBishopLongDiagonal(Color color, SparseArray<short> v)
        {
            if (v.ContainsKey(BISHOP_LONG_DIAG))
            {
                v[BISHOP_LONG_DIAG] += Increment(color);
            }
            else
            {
                v.Add(BISHOP_LONG_DIAG, Increment(color));
            }
        }

        private static void IncrementMinorOutpost(Color color, SparseArray<short> v, Piece piece, int count)
        {
            if (piece < Piece.Knight || piece > Piece.Bishop || count <= 0)
            {
                return;
            }

            int index = MINOR_OUTPOST + (piece - Piece.Knight);
            if (v.ContainsKey(index))
            {
                v[index] += (short)(count * Increment(color));
            }
            else
            {
                v.Add(index, (short)(count * Increment(color)));
            }
        }

        private static void SetTempoBonus(Color color, SparseArray<short> v)
        {
            v[TEMPO] = Increment(color);
        }
    }
}
