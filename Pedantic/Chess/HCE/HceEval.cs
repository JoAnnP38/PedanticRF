// <copyright file="HceEval.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Chess.HCE
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Pedantic.Collections;

    public sealed class HceEval : IInitialize
    {
        public const int MAX_ATTACK_LEN = 8;
        public const ulong DARK_SQUARES_MASK = 0xAA55AA55AA55AA55ul;
        public const ulong LITE_SQUARES_MASK = 0x55AA55AA55AA55AAul;
        public const ulong CENTER_MASK = 0x0000001818000000ul;
        public readonly static Bitboard WHITE_OUTPOST_RANKS = new Bitboard(Rank.Rank4, Rank.Rank5, Rank.Rank6);
        public readonly static Bitboard BLACK_OUTPOST_RANKS = new Bitboard(Rank.Rank5, Rank.Rank4, Rank.Rank3);
        public readonly static ByColor<Bitboard> Outposts;


        [InlineArray(MAX_ATTACK_LEN)]
        public struct AttackArray
        {
            public const int CAPACITY = MAX_ATTACK_LEN;
            private Bitboard _element0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<Bitboard> AsSpan()
            {
                return MemoryMarshal.CreateSpan(ref _element0, CAPACITY);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                AsSpan().Clear();
            }
        }

        [InlineArray(MAX_PIECES + 4)]
        public struct AttackByArray
        {
            public const int CAPACITY = MAX_PIECES + 4;
            private Bitboard _element0;

            public Bitboard this[AttackBy attackBy]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this[(int)attackBy];

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => this[(int)attackBy] = value;
            }
        }

        public struct EvalInfo
        {
            public Bitboard Pawns;
            public Bitboard MobilityArea;
            public Bitboard PassedPawns;
            public Bitboard AttackByTwo;
            public short Material;
            public SquareIndex KI;
            public KingBuckets KB;
            public bool CanWin;
            public byte CanCastle;
            public byte AttackCount;
            public AttackArray Attacks;
            public AttackByArray AttackBy;
        }
        
        static HceEval()
        {
            wts = Engine.Weights;

            FlankMask[0] = new Bitboard(File.FileA, File.FileB, File.FileC);
            FlankMask[1] = FlankMask[0] | new Bitboard(File.FileD);
            FlankMask[2] = FlankMask[1];
            FlankMask[3] = new Bitboard(File.FileC, File.FileD, File.FileE, File.FileF);
            FlankMask[4] = FlankMask[3];
            FlankMask[5] = new Bitboard(File.FileE, File.FileF, File.FileG, File.FileH);
            FlankMask[6] = FlankMask[5];
            FlankMask[7] = new Bitboard(File.FileF, File.FileG, File.FileH);
            Outposts[Color.White] = WHITE_OUTPOST_RANKS;
            Outposts[Color.Black] = BLACK_OUTPOST_RANKS;
        }

        public HceEval(EvalCache evalCache)
        { 
            cache = evalCache;
        }

        public HceEval(EvalCache evalCache, Weights weights) : this(evalCache)
        { 
            Weights = weights;
        }

        public static Weights Weights
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return wts;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                wts = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ColorToSign(Color color)
        {
            return ((int)color * -2) + 1;
        }

        public short Compute(Board board, int alpha = -INFINITE_WINDOW, int beta = INFINITE_WINDOW)
        {
            if (cache.ProbeEvalCache(board.Hash, board.SideToMove, out EvalCache.EvalCacheItem item))
            {
                return item.EvalScore;
            }

            bool isLazy = false;
            InitializeEvalInfo(board, ref evalInfo);
            short score = ComputeNormal(board, alpha, beta, ref isLazy);

            if (UciOptions.RandomSearch)
            {
                score += (short)rand.Next(-8, 9);
            }

            score = (short)(ColorToSign(board.SideToMove) * score);

            if (!isLazy)
            {
                cache.SaveEval(board.Hash, score, board.SideToMove);
            }

            return score;
        }

        public short ComputeNormal(Board board, int alpha, int beta, ref bool isLazy)
        {
            Score score = EvalMaterialAndPst(board, Color.White);
            score -= EvalMaterialAndPst(board, Color.Black);

            int normalScore = score.NormalizeScore(board.Phase);
            int evalScore = ColorToSign(board.SideToMove) * normalScore;
            if (evalScore < alpha - UciOptions.LzyEvalMargin || evalScore > beta + UciOptions.LzyEvalMargin)
            { 
                isLazy = true;
                return (short)normalScore;
            }

            score += ProbePawnCache(board);
            score += EvalMobility(board, Color.White);
            score -= EvalMobility(board, Color.Black);
            score += EvalKingSafety(board, Color.White);
            score -= EvalKingSafety(board, Color.Black);
            score += EvalPieces(board, Color.White);
            score -= EvalPieces(board, Color.Black);
            score += EvalPassedPawns(board, Color.White);
            score -= EvalPassedPawns(board, Color.Black);
            score += EvalThreats(board, Color.White);
            score -= EvalThreats(board, Color.Black);
            score += ColorToSign(board.SideToMove) * wts.TempoBonus;

            var egScale = CalcDrawRatio(board, in evalInfo, score);

            return score.NormalizeScore(board.Phase, egScale.Scale, egScale.Divisor);
        }

        public Score ProbePawnCache(Board board)
        {
            Score pawnScore;
            if (cache.ProbePawnCache(board.PawnHash, out EvalCache.PawnCacheItem item))
            {
                evalInfo[0].PassedPawns = item.PassedPawns & board.Units(Color.White);
                evalInfo[1].PassedPawns = item.PassedPawns & board.Units(Color.Black);
                pawnScore = item.Eval;
            }
            else
            {
                pawnScore = EvalPawns(board, Color.White) - EvalPawns(board, Color.Black);
                Bitboard passedPawns = evalInfo[0].PassedPawns | evalInfo[1].PassedPawns;
                cache.SavePawnEval(board.PawnHash, passedPawns, pawnScore);
            }

            return pawnScore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prefetch(Board board)
        {
            cache.PrefetchEvalCache(board.Hash, board.PawnHash);
        }

        public Score EvalMaterialAndPst(Board board, Color color)
        {
            // material remains up to date via incremental updates
            Score score = board.Material[color];
            int c = (int)color;

            foreach (SquareIndex from in board.Units(color))
            {
                SquareIndex normalFrom = from.Normalize(color);
                Piece piece = board.PieceBoard(from).Piece;
                score += wts.FriendlyPieceSquareValue(piece, evalInfo[c].KB, normalFrom);
                score += wts.EnemyPieceSquareValue(piece, evalInfo[c].KB, normalFrom);
            }

            return score;
        }

        public Score EvalPawns(Board board, Color color)
        {
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;
            Score score = Score.Zero;
            Bitboard pawns = evalInfo[c].Pawns;

            if (pawns == 0)
            {
                return score;
            }

            Bitboard otherPawns = evalInfo[o].Pawns;
            Bitboard pawnRams = (color == Color.White ? otherPawns >> 8 : otherPawns << 8);


            foreach (SquareIndex from in pawns)
            {
                Ray ray = Board.Vectors[(int)from];
                Bitboard friendMask = color == Color.White ? ray.North : ray.South;
                SquareIndex normalFrom = from.Normalize(color);
                Bitboard sqMask = new Bitboard(from);
                bool canBeBackward = true;

                if ((otherPawns & PassedPawnMasks[c, (int)from]) == 0 && (pawns & friendMask) == 0)
                {
                    score += wts.PassedPawn(normalFrom);
                    evalInfo[c].PassedPawns |= sqMask;
                    canBeBackward = false;
                }

                if ((pawns & AdjacentPawnMasks[(int)from]) != 0)
                {
                    score += wts.PhalanxPawn(normalFrom);
                }

                if ((evalInfo[c].AttackBy[AttackBy.Pawn] & sqMask) != 0)
                {
                    score += wts.ChainedPawn(normalFrom);
                }

                if ((pawnRams & sqMask) != 0)
                {
                    score += wts.PawnRam(normalFrom);
                }

                if ((pawns & IsolatedPawnMasks[(int)from]) == 0)
                {
                    score += wts.IsolatedPawn(normalFrom);
                    canBeBackward = false;
                }

                if (canBeBackward && (pawns & BackwardPawnMasks[c, (int)from]) == 0)
                {
                    score += wts.BackwardPawn;
                }
            }

            return score;
        }

        public Score EvalMobility(Board board, Color color)
        {
            Color other = color.Flip();
            int c = (int)color;
            Score score = Score.Zero;

            foreach (SquareIndex from in board.Pieces(color, Piece.Knight))
            {
                Bitboard pieceAttacks = Board.KnightMoves(from);
                evalInfo[c].AttackByTwo |= evalInfo[c].AttackBy[AttackBy.All] & pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.Knight] |= pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.All] |= pieceAttacks;
                if (evalInfo[c].AttackCount < MAX_ATTACK_LEN)
                {
                    evalInfo[c].Attacks[evalInfo[c].AttackCount++] = pieceAttacks;
                }
                board.IsPinned(from, out Bitboard pinMask);
                int mobility = (pieceAttacks & pinMask & evalInfo[c].MobilityArea).PopCount;
                score += wts.KnightMobility(mobility);
            }

            Bitboard occupied = board.All ^ board.DiagonalSliders(color);
            foreach (SquareIndex from in board.Pieces(color, Piece.Bishop))
            {
                Bitboard pieceAttacks = Board.GetBishopMoves(from, occupied);
                evalInfo[c].AttackByTwo |= evalInfo[c].AttackBy[AttackBy.All] & pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.Bishop] |= pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.All] |= pieceAttacks;
                if (evalInfo[c].AttackCount < MAX_ATTACK_LEN)
                {
                    evalInfo[c].Attacks[evalInfo[c].AttackCount++] = pieceAttacks;
                }
                board.IsPinned(from, out Bitboard pinMask);
                int mobility = (pieceAttacks & pinMask & evalInfo[c].MobilityArea).PopCount;
                score += wts.BishopMobility(mobility);
            }

            occupied = board.All ^ board.OrthogonalSliders(color);
            foreach (SquareIndex from in board.Pieces(color, Piece.Rook))
            {
                Bitboard pieceAttacks = Board.GetRookMoves(from, occupied);
                evalInfo[c].AttackByTwo |= evalInfo[c].AttackBy[AttackBy.All] & pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.Rook] |= pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.All] |= pieceAttacks;
                if (evalInfo[c].AttackCount < MAX_ATTACK_LEN)
                {
                    evalInfo[c].Attacks[evalInfo[c].AttackCount++] = pieceAttacks;
                }
                board.IsPinned(from, out Bitboard pinMask);
                int mobility = (pieceAttacks & pinMask & evalInfo[c].MobilityArea).PopCount;
                score += wts.RookMobility(mobility);
            }

            foreach (SquareIndex from in board.Pieces(color, Piece.Queen))
            {
                Bitboard pieceAttacks = Board.GetQueenMoves(from, board.All);
                evalInfo[c].AttackByTwo |= evalInfo[c].AttackBy[AttackBy.All] & pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.Queen] |= pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.All] |= pieceAttacks;
                if (evalInfo[c].AttackCount < MAX_ATTACK_LEN)
                {
                    evalInfo[c].Attacks[evalInfo[c].AttackCount++] = pieceAttacks;
                }
                board.IsPinned(from, out Bitboard pinMask);
                int mobility = (pieceAttacks & pinMask & evalInfo[c].MobilityArea).PopCount;
                score += wts.QueenMobility(mobility);
            }

            return score;
        }

        public Score EvalKingSafety(Board board, Color color)
        {
            Score score = Score.Zero;
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            // give a bonus to each attack on a square within 1 or 2 squares from the enemy king
            Bitboard defended = evalInfo[o].AttackBy[AttackBy.Pawn] | (evalInfo[o].AttackByTwo & ~evalInfo[c].AttackByTwo);
            SquareIndex enemyKI = evalInfo[o].KI;
            Bitboard attacks = evalInfo[c].AttackBy[AttackBy.PawnLeft].AndNot(defended);
            int count1 = (attacks & (Bitboard)KingProximity[0, (int)enemyKI]).PopCount;
            int count2 = (attacks & (Bitboard)KingProximity[1, (int)enemyKI]).PopCount;

            attacks = evalInfo[c].AttackBy[AttackBy.PawnRight].AndNot(defended);
            count1 += (attacks & (Bitboard)KingProximity[0, (int)enemyKI]).PopCount;
            count2 += (attacks & (Bitboard)KingProximity[1, (int)enemyKI]).PopCount;

            for (int n = 0; n < evalInfo[c].AttackCount; n++)
            {
                attacks = evalInfo[c].Attacks[n].AndNot(defended);
                count1 += (attacks & (Bitboard)KingProximity[0, (int)enemyKI]).PopCount;
                count2 += (attacks & (Bitboard)KingProximity[1, (int)enemyKI]).PopCount;
            }

            score += wts.KingAttack1(count1);
            score += wts.KingAttack2(count2);

            // give a bonus for castling rights to force engine to castle and not forfeit them for free
            if ((evalInfo[c].CanCastle & 0x01) != 0)
            {
                score += wts.CanCastleKS;
            }

            if ((evalInfo[c].CanCastle & 0x02) != 0)
            {
                score += wts.CanCastleQS;
            }

            // give a penalty for each square that a line piece (i.e. B, R or Q) would have an 
            // attack on our king
            SquareIndex KI = evalInfo[c].KI;
            if (board.DiagonalSliders(other) != 0)
            {
                attacks = Board.GetBishopMoves(KI, board.All);
                int mobility = (attacks & evalInfo[o].MobilityArea).PopCount;
                score += wts.KsDiagonalMobility(mobility);
            }

            if (board.OrthogonalSliders(other) != 0)
            {
                attacks = Board.GetRookMoves(KI, board.All);
                int mobility = (attacks & evalInfo[o].MobilityArea).PopCount;
                score += wts.KsOrthogonalMobility(mobility);
            }

            // penalize king for being on pawnless flank
            Bitboard pawns = evalInfo[c].Pawns;
            if ((pawns & FlankMask[(int)KI.File()]).PopCount == 0)
            {
                score += wts.PawnlessFlank;
            }

            return score;
        }

        public Score EvalPieces(Board board, Color color)
        {
            Score score = Score.Zero;
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            Bitboard knights = board.Pieces(color, Piece.Knight);
            Bitboard bishops = board.Pieces(color, Piece.Bishop);
            int bishopCount = bishops.PopCount;
            if (bishopCount >= 2)
            {
                score += wts.BishopPair;
            }
            else if (bishopCount == 1)
            {
                SquareIndex sq = (SquareIndex)bishops.TzCount;
                Bitboard badPawnMask = sq.IsDark() ? (Bitboard)DARK_SQUARES_MASK : (Bitboard)LITE_SQUARES_MASK;
                int badPawnCount = (evalInfo[c].Pawns & badPawnMask).PopCount;
                score += badPawnCount * wts.BadBishopPawn;
            }

            foreach (SquareIndex from in bishops)
            {
                if ((Board.GetBishopMoves(from, board.Pawns) & (Bitboard)CENTER_MASK).PopCount == 2)
                {
                    score += wts.BishopLongDiagonal;
                }
            }

            Bitboard pawns = evalInfo[c].Pawns;
            Bitboard otherPawns = evalInfo[o].Pawns;

            foreach (SquareIndex sq in board.Pieces(color, Piece.Rook))
            {
                Bitboard fileMask = new Bitboard(sq.File());
                if ((board.Pawns & fileMask) == 0)
                {
                    score += wts.RookOnOpenFile;
                }

                if ((pawns & fileMask) == 0 && (otherPawns & fileMask) != 0)
                {
                    score += wts.RookOnHalfOpenFile;
                }
            }

            Bitboard minorPieces = knights | bishops;
            Bitboard shieldPawns = color == Color.White ? (pawns >> 8) : (pawns << 8); 
            score += (shieldPawns & minorPieces).PopCount * wts.PawnShieldsMinor;

            Bitboard outposts = Outposts[color] & evalInfo[c].AttackBy[AttackBy.Pawn];
            outposts = outposts.AndNot(evalInfo[o].AttackBy[AttackBy.Pawn] | evalInfo[o].AttackBy[AttackBy.PawnPush]);
            score += (knights & outposts).PopCount * wts.MinorOutpost(Piece.Knight);
            score += (bishops & outposts).PopCount * wts.MinorOutpost(Piece.Bishop);

            return score;
        }

        public Score EvalPassedPawns(Board board, Color color)
        {
            Score score = Score.Zero;
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

                if (board.PieceCount(other) == 1)
                {
                    SquareIndex promoteSq = ChessMath.ToSquareIndex(ppIndex.File(), PromoteRank(color));
                    if (ChessMath.Distance(ppIndex, promoteSq) < 
                        ChessMath.Distance(evalInfo[o].KI, promoteSq) - (other == board.SideToMove ? 1 : 0))
                    {
                        score += wts.KingOutsidePasserSquare;
                    }
                }

                Bitboard advanceMask = new Bitboard(Board.PawnPlus(color, ppIndex));
                Bitboard attacksMask = evalInfo[o].AttackBy[AttackBy.All];

                if ((advanceMask & board.All) == 0 && (advanceMask & attacksMask) == 0)
                {
                    score += wts.PassedPawnCanAdvance(normalRank);
                }

                Direction behind = color == Color.White ? Direction.South : Direction.North;
                if ((Board.GetAttack(ppIndex, behind, board.All) & board.Pieces(color, Piece.Rook)) != 0)
                {
                    score += wts.RookBehindPassedPawn;
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

            Bitboard blockers = board.Units(color) & ePassedPawns;
            foreach (SquareIndex sq in blockers)
            {
                Piece blocker = board.PieceBoard(sq).Piece;
                Rank normalRank = sq.Normalize(other).Rank();
                score += wts.BlockedPassedPawn(blocker, normalRank - 1);
            }

            return score;
        }

        public Score EvalThreats(Board board, Color color)
        {
            Score score = Score.Zero;
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            Bitboard pawns = evalInfo[c].Pawns;
            Bitboard otherPawns = evalInfo[o].Pawns;
            Bitboard targets = board.Units(other).AndNot(otherPawns);
            Bitboard pushAttacks = evalInfo[c].AttackBy[AttackBy.PawnPush];

            if (targets == 0)
            {
                return score;
            }

            foreach (SquareIndex sq in pushAttacks & targets)
            {
                Piece threatenedPiece = board.PieceBoard(sq).Piece;
                score += wts.PushedPawnThreat(threatenedPiece);
            }

            foreach (SquareIndex sq in evalInfo[c].AttackBy[AttackBy.Pawn] & targets)
            {
                Piece threatenedPiece = board.PieceBoard(sq).Piece;
                score += wts.PawnThreat(threatenedPiece);
            }

            targets &= ~board.Kings;
            Bitboard attacks = evalInfo[c].AttackBy[AttackBy.Knight] | evalInfo[c].AttackBy[AttackBy.Bishop];
            foreach (SquareIndex sq in attacks & targets)
            {
                Piece threatenedPiece = board.PieceBoard(sq).Piece;
                score += wts.MinorThreat(threatenedPiece);
            }

            targets &= ~evalInfo[o].AttackBy[AttackBy.Pawn];
            attacks = evalInfo[c].AttackBy[AttackBy.Rook];
            foreach (SquareIndex sq in attacks & targets)
            {
                Piece threatenedPiece = board.PieceBoard(sq).Piece;
                score += wts.RookThreat(threatenedPiece);
            }

            // check threats
            SquareIndex enemyKI = evalInfo[o].KI;
            Bitboard toSquares = (~board.Units(color)).AndNot(evalInfo[o].AttackBy[AttackBy.Pawn]);

            // check threats from knights
            Bitboard checkThreats = Board.KnightMoves(enemyKI) & toSquares;
            foreach (SquareIndex from in board.Pieces(color, Piece.Knight))
            {
                score += (Board.KnightMoves(from) & checkThreats).PopCount * wts.CheckThreat(Piece.Knight);
            }

            // check threats from bishops
            checkThreats = Board.GetBishopMoves(enemyKI, board.All) & toSquares;
            foreach (SquareIndex from in board.Pieces(color, Piece.Bishop))
            {
                score += (Board.GetBishopMoves(from, board.All) & checkThreats).PopCount * wts.CheckThreat(Piece.Bishop);
            }

            // check threats from rook
            checkThreats = Board.GetRookMoves(enemyKI, board.All) & toSquares;
            foreach (SquareIndex from in board.Pieces(color, Piece.Rook))
            {
                score += (Board.GetRookMoves(from, board.All) & checkThreats).PopCount * wts.CheckThreat(Piece.Rook);
            }

            // check threats from queen
            checkThreats = Board.GetQueenMoves(enemyKI, board.All) & toSquares;
            foreach (SquareIndex from in board.Pieces(color, Piece.Queen))
            {
                score += (Board.GetQueenMoves(from, board.All) & checkThreats).PopCount * wts.CheckThreat(Piece.Queen);
            }

            return score;
        }

        public short AdjustDraws(Board board, short score)
        {
            if (board.HalfMoveClock > 84)
            {
                // scale down eval score as game approaches 50mr draw
                int movesLeft = Math.Max(100 - board.HalfMoveClock, 0);
                score = (short)(score * movesLeft / 16);
            }
            else if ((score > 0 && !evalInfo[0].CanWin) || (score < 0 && !evalInfo[1].CanWin))
            {
                // if "winning" side cannot win scale down score
                score /= 8;
            }
            else if (IsOcbEndgame(board, out int pcCount))
            {
                // if playing OCB endgame, scale down score
                score = (short)((score * (pcCount == 1 ? 2 : 3)) / 4);
            }
            return score;
        }

        public static (sbyte Scale, sbyte Divisor) CalcDrawRatio(Board board, in ByColor<EvalInfo> evalInfo, Score score)
        {
            short normScore = score.NormalizeScore(board.Phase);
            if (board.HalfMoveClock > 84)
            {
                return ((sbyte)Math.Max(100 - board.HalfMoveClock, 0), 16);
            }
            else if ((normScore > 0 && !evalInfo[0].CanWin) || (normScore < 0 && !evalInfo[1].CanWin))
            {
                return (1, 8);
            }
            else if (IsOcbEndgame(board, out int pcCount))
            {
                return ((sbyte)(pcCount == 1 ? 2 : 3), 4);
            }

            return (1, 1);
        }

        public static bool SufficientMatingMaterial(Board board, Color side)
        {
            int numKnights = board.Pieces(side, Piece.Knight).PopCount;
            int numBishops = board.Pieces(side, Piece.Bishop).PopCount;
            bool case1 = (board.Pieces(side, Piece.Rook) | board.Pieces(side, Piece.Queen)) != 0;
            bool case2 = (numKnights >= 1 && numBishops >= 1) || numBishops >= 2 || numKnights >= 3;
            return case1 || case2;
        }

        public static (bool WhiteCanWin, bool BlackCanWin) CanWin(Board board, in ByColor<EvalInfo> evalInfo)
        {
            bool whiteCanWin = false, blackCanWin = false;
            int winMargin = wts.PieceValue(Piece.Pawn).NormalizeScore(board.Phase) * 4;

            if (evalInfo[0].Pawns != 0)
            {
                whiteCanWin = true;
            }
            else if (evalInfo[0].Material - evalInfo[1].Material >= winMargin)
            {
                whiteCanWin = SufficientMatingMaterial(board, Color.White);
            }

            if (evalInfo[1].Pawns != 0)
            {
                blackCanWin = true;
            }
            else if (evalInfo[1].Material - evalInfo[0].Material >= winMargin)
            {
                blackCanWin = SufficientMatingMaterial(board, Color.Black);
            }

            return (whiteCanWin, blackCanWin);
        }

        public static bool IsOcbEndgame(Board board, out int pcCount)
        {
            pcCount = 0;
            int whitePieceCount = board.Units(Color.White).AndNot(board.Pawns).PopCount - 1;
            int blackPieceCount = board.Units(Color.Black).AndNot(board.Pawns).PopCount - 1;
            Bitboard whiteBishops = board.Pieces(Color.White, Piece.Bishop);
            Bitboard blackBishops = board.Pieces(Color.Black, Piece.Bishop);

            if (whitePieceCount > 3 || blackPieceCount > 3 || whitePieceCount != blackPieceCount ||
                whiteBishops.PopCount != 1 || blackBishops.PopCount != 1)
            {
                return false;
            }

            pcCount = whitePieceCount;
            bool isWhiteDark = ((SquareIndex)whiteBishops.TzCount).IsDark();
            bool isBlackDark = ((SquareIndex)blackBishops.TzCount).IsDark();
            return isWhiteDark != isBlackDark;
        }

        public static void InitializeEvalInfo(Board board, ref ByColor<EvalInfo> evalInfo)
        {
            evalInfo.Clear();
            for (Color color = Color.White; color <= Color.Black; color++)
            {
                int c = (int)color;
                Bitboard pawns = board.Pieces(color, Piece.Pawn);
                evalInfo[c].Pawns = pawns;
                evalInfo[c].Material = board.Material[color].NormalizeScore(board.Phase);
                evalInfo[c].KI = board.KingIndex[color];
                evalInfo[c].KB = new KingBuckets(color, board.KingIndex[color], board.KingIndex[color.Flip()]);
                Bitboard kingAttacks = Board.KingMoves(evalInfo[c].KI);
                evalInfo[c].AttackBy[AttackBy.King] = kingAttacks;
                evalInfo[c].AttackBy[AttackBy.All] = kingAttacks;
                Bitboard dblPawnAttack;
                Bitboard pawnAttacks, pawnLeft, pawnRight, pushAttacks;
                if (color == Color.White)
                {
                    Bitboard pawnPush = (pawns << 8).AndNot(board.All);
                    pushAttacks = (pawnPush.AndNot(Bitboard.BbFileA) << 7) |
                                  (pawnPush.AndNot(Bitboard.BbFileH) << 9);
                    pawnLeft = pawns.AndNot(Bitboard.BbFileA) << 7;
                    pawnRight = pawns.AndNot(Bitboard.BbFileH) << 9;
                    pawnAttacks = pawnLeft | pawnRight;
                    dblPawnAttack = pawnLeft & pawnRight;
                    evalInfo[c].CanCastle = (byte)(board.Castling & CastlingRights.WhiteRights);
                }
                else
                {
                    Bitboard pawnPush = (pawns >> 8).AndNot(board.All);
                    pushAttacks = (pawnPush.AndNot(Bitboard.BbFileA) >> 9) |
                                  (pawnPush.AndNot(Bitboard.BbFileH) >> 7);
                    pawnLeft = pawns.AndNot(Bitboard.BbFileA) >> 9;
                    pawnRight = pawns.AndNot(Bitboard.BbFileH) >> 7;
                    pawnAttacks = pawnLeft | pawnRight;
                    dblPawnAttack = pawnLeft & pawnRight;
                    evalInfo[c].CanCastle = (byte)((int)(board.Castling & CastlingRights.BlackRights) >> 2);
                }

                evalInfo[c].AttackByTwo = dblPawnAttack | (pawnAttacks & kingAttacks);
                evalInfo[c].AttackBy[AttackBy.Pawn] = pawnAttacks;
                evalInfo[c].AttackBy[AttackBy.PawnLeft] = pawnLeft;
                evalInfo[c].AttackBy[AttackBy.PawnRight] = pawnRight;
                evalInfo[c].AttackBy[AttackBy.PawnPush] = pushAttacks;
                evalInfo[c].AttackBy[AttackBy.All] |= pawnAttacks;
            }

            /* two pass init */
            for (Color color = Color.White; color <= Color.Black; color++)
            {
                Color other = color.Flip();
                int c = (int)color;
                int o = (int)other;
                Bitboard blockers, lowRanks;
                if (color == Color.White)
                {
                    lowRanks = new Bitboard(Rank.Rank2) | new Bitboard(Rank.Rank3);
                    blockers = board.All >> 8;
                }
                else
                {
                    lowRanks = new Bitboard(Rank.Rank6) | new Bitboard(Rank.Rank7);
                    blockers = board.All << 8;
                }
                Bitboard bb = board.Pieces(color, Piece.Pawn) & (blockers | lowRanks);
                Bitboard royalty = board.Pieces(color, Piece.Queen) | board.Pieces(color, Piece.King);
                evalInfo[c].MobilityArea = ~(bb | royalty | evalInfo[o].AttackBy[AttackBy.Pawn]);
            }

            var canWin = CanWin(board, in evalInfo);
            evalInfo[0].CanWin = canWin.WhiteCanWin;
            evalInfo[1].CanWin = canWin.BlackCanWin;
        }

        public void InitializeEvalInfo(Board board)
        {
            InitializeEvalInfo(board, ref evalInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank PromoteRank(Color color)
        {
            return color == Color.White ? Rank.Rank8 : Rank.Rank1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard WeakEnemies(Board board, Color enemyColor)
        {
            return (board.Units(enemyColor) & evalInfo[enemyColor.Flip()].AttackBy[AttackBy.All])
                .AndNot(evalInfo[enemyColor].AttackBy[AttackBy.Pawn]);
        }

        public Bitboard Hanging(Board board, Color enemyColor)
        {
            Color color = enemyColor.Flip();
            Bitboard hanging = Bitboard.None;

            foreach (SquareIndex sq in WeakEnemies(board, enemyColor))
            {
                Bitboard sqMask = new (sq);
                if ((sqMask & board.Pieces(enemyColor, Piece.Pawn)) != 0 || board.AttackCount(sq, color) > 1)
                {
                    hanging |= sqMask;
                }
            }

            return hanging;
        }

        public static void Initialize() {}

        public readonly Bitboard BB_KS_MASK = new Bitboard(0xf0f0f0f0f0f0f0f0ul);
        public readonly Bitboard BB_QS_MASK = new Bitboard(0x0f0f0f0f0f0f0f0ful);

        private ByColor<EvalInfo> evalInfo;
        private readonly EvalCache cache;
        private readonly Random rand = new Random();
        private static Weights wts;


        public static readonly FixedArray2D<ulong> PassedPawnMasks = new (MAX_COLORS, MAX_SQUARES)
        {
            #region PassedPawnMasks data

            // white passed pawn masks
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0003030303030000ul, 0x0007070707070000ul, 0x000E0E0E0E0E0000ul, 0x001C1C1C1C1C0000ul,
            0x0038383838380000ul, 0x0070707070700000ul, 0x00E0E0E0E0E00000ul, 0x00C0C0C0C0C00000ul,
            0x0003030303000000ul, 0x0007070707000000ul, 0x000E0E0E0E000000ul, 0x001C1C1C1C000000ul,
            0x0038383838000000ul, 0x0070707070000000ul, 0x00E0E0E0E0000000ul, 0x00C0C0C0C0000000ul,
            0x0003030300000000ul, 0x0007070700000000ul, 0x000E0E0E00000000ul, 0x001C1C1C00000000ul,
            0x0038383800000000ul, 0x0070707000000000ul, 0x00E0E0E000000000ul, 0x00C0C0C000000000ul,
            0x0003030000000000ul, 0x0007070000000000ul, 0x000E0E0000000000ul, 0x001C1C0000000000ul,
            0x0038380000000000ul, 0x0070700000000000ul, 0x00E0E00000000000ul, 0x00C0C00000000000ul,
            0x0003000000000000ul, 0x0007000000000000ul, 0x000E000000000000ul, 0x001C000000000000ul,
            0x0038000000000000ul, 0x0070000000000000ul, 0x00E0000000000000ul, 0x00C0000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            // black passed pawn masks
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000300ul, 0x0000000000000700ul, 0x0000000000000E00ul, 0x0000000000001C00ul,
            0x0000000000003800ul, 0x0000000000007000ul, 0x000000000000E000ul, 0x000000000000C000ul,
            0x0000000000030300ul, 0x0000000000070700ul, 0x00000000000E0E00ul, 0x00000000001C1C00ul,
            0x0000000000383800ul, 0x0000000000707000ul, 0x0000000000E0E000ul, 0x0000000000C0C000ul,
            0x0000000003030300ul, 0x0000000007070700ul, 0x000000000E0E0E00ul, 0x000000001C1C1C00ul,
            0x0000000038383800ul, 0x0000000070707000ul, 0x00000000E0E0E000ul, 0x00000000C0C0C000ul,
            0x0000000303030300ul, 0x0000000707070700ul, 0x0000000E0E0E0E00ul, 0x0000001C1C1C1C00ul,
            0x0000003838383800ul, 0x0000007070707000ul, 0x000000E0E0E0E000ul, 0x000000C0C0C0C000ul,
            0x0000030303030300ul, 0x0000070707070700ul, 0x00000E0E0E0E0E00ul, 0x00001C1C1C1C1C00ul,
            0x0000383838383800ul, 0x0000707070707000ul, 0x0000E0E0E0E0E000ul, 0x0000C0C0C0C0C000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul

            #endregion PassedPawnMasks data
        };

        public static readonly FixedArray<ulong> IsolatedPawnMasks = new (MAX_SQUARES)
        {
            #region IsolatedPawnMasks data
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0002020202020200ul, 0x0005050505050500ul, 0x000A0A0A0A0A0A00ul, 0x0014141414141400ul,
            0x0028282828282800ul, 0x0050505050505000ul, 0x00A0A0A0A0A0A000ul, 0x0040404040404000ul,
            0x0002020202020200ul, 0x0005050505050500ul, 0x000A0A0A0A0A0A00ul, 0x0014141414141400ul,
            0x0028282828282800ul, 0x0050505050505000ul, 0x00A0A0A0A0A0A000ul, 0x0040404040404000ul,
            0x0002020202020200ul, 0x0005050505050500ul, 0x000A0A0A0A0A0A00ul, 0x0014141414141400ul,
            0x0028282828282800ul, 0x0050505050505000ul, 0x00A0A0A0A0A0A000ul, 0x0040404040404000ul,
            0x0002020202020200ul, 0x0005050505050500ul, 0x000A0A0A0A0A0A00ul, 0x0014141414141400ul,
            0x0028282828282800ul, 0x0050505050505000ul, 0x00A0A0A0A0A0A000ul, 0x0040404040404000ul,
            0x0002020202020200ul, 0x0005050505050500ul, 0x000A0A0A0A0A0A00ul, 0x0014141414141400ul,
            0x0028282828282800ul, 0x0050505050505000ul, 0x00A0A0A0A0A0A000ul, 0x0040404040404000ul,
            0x0002020202020200ul, 0x0005050505050500ul, 0x000A0A0A0A0A0A00ul, 0x0014141414141400ul,
            0x0028282828282800ul, 0x0050505050505000ul, 0x00A0A0A0A0A0A000ul, 0x0040404040404000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul
            #endregion IsolatedPawnMasks data
        };

        public static readonly FixedArray<ulong> AdjacentPawnMasks = new (MAX_SQUARES)
        {
            #region AdjacentPawnMasks data

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000200ul, 0x0000000000000500ul, 0x0000000000000A00ul, 0x0000000000001400ul,
            0x0000000000002800ul, 0x0000000000005000ul, 0x000000000000A000ul, 0x0000000000004000ul,
            0x0000000000020000ul, 0x0000000000050000ul, 0x00000000000A0000ul, 0x0000000000140000ul,
            0x0000000000280000ul, 0x0000000000500000ul, 0x0000000000A00000ul, 0x0000000000400000ul,
            0x0000000002000000ul, 0x0000000005000000ul, 0x000000000A000000ul, 0x0000000014000000ul,
            0x0000000028000000ul, 0x0000000050000000ul, 0x00000000A0000000ul, 0x0000000040000000ul,
            0x0000000200000000ul, 0x0000000500000000ul, 0x0000000A00000000ul, 0x0000001400000000ul,
            0x0000002800000000ul, 0x0000005000000000ul, 0x000000A000000000ul, 0x0000004000000000ul,
            0x0000020000000000ul, 0x0000050000000000ul, 0x00000A0000000000ul, 0x0000140000000000ul,
            0x0000280000000000ul, 0x0000500000000000ul, 0x0000A00000000000ul, 0x0000400000000000ul,
            0x0002000000000000ul, 0x0005000000000000ul, 0x000A000000000000ul, 0x0014000000000000ul,
            0x0028000000000000ul, 0x0050000000000000ul, 0x00A0000000000000ul, 0x0040000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul

            #endregion AdjacentPawnMasks data
        };

        public static readonly FixedArray2D<ulong> BackwardPawnMasks = new (MAX_COLORS, MAX_SQUARES)
        {
            #region BackwardPawnMasks data

            0x0000000000000002ul, 0x0000000000000005ul, 0x000000000000000Aul, 0x0000000000000014ul,
            0x0000000000000028ul, 0x0000000000000050ul, 0x00000000000000A0ul, 0x0000000000000040ul,
            0x0000000000000202ul, 0x0000000000000505ul, 0x0000000000000A0Aul, 0x0000000000001414ul,
            0x0000000000002828ul, 0x0000000000005050ul, 0x000000000000A0A0ul, 0x0000000000004040ul,
            0x0000000000020202ul, 0x0000000000050505ul, 0x00000000000A0A0Aul, 0x0000000000141414ul,
            0x0000000000282828ul, 0x0000000000505050ul, 0x0000000000A0A0A0ul, 0x0000000000404040ul,
            0x0000000002020202ul, 0x0000000005050505ul, 0x000000000A0A0A0Aul, 0x0000000014141414ul,
            0x0000000028282828ul, 0x0000000050505050ul, 0x00000000A0A0A0A0ul, 0x0000000040404040ul,
            0x0000000202020202ul, 0x0000000505050505ul, 0x0000000A0A0A0A0Aul, 0x0000001414141414ul,
            0x0000002828282828ul, 0x0000005050505050ul, 0x000000A0A0A0A0A0ul, 0x0000004040404040ul,
            0x0000020202020202ul, 0x0000050505050505ul, 0x00000A0A0A0A0A0Aul, 0x0000141414141414ul,
            0x0000282828282828ul, 0x0000505050505050ul, 0x0000A0A0A0A0A0A0ul, 0x0000404040404040ul,
            0x0002020202020202ul, 0x0005050505050505ul, 0x000A0A0A0A0A0A0Aul, 0x0014141414141414ul,
            0x0028282828282828ul, 0x0050505050505050ul, 0x00A0A0A0A0A0A0A0ul, 0x0040404040404040ul,
            0x0202020202020202ul, 0x0505050505050505ul, 0x0A0A0A0A0A0A0A0Aul, 0x1414141414141414ul,
            0x2828282828282828ul, 0x5050505050505050ul, 0xA0A0A0A0A0A0A0A0ul, 0x4040404040404040ul,

            0x0202020202020202ul, 0x0505050505050505ul, 0x0A0A0A0A0A0A0A0Aul, 0x1414141414141414ul,
            0x2828282828282828ul, 0x5050505050505050ul, 0xA0A0A0A0A0A0A0A0ul, 0x4040404040404040ul,
            0x0202020202020200ul, 0x0505050505050500ul, 0x0A0A0A0A0A0A0A00ul, 0x1414141414141400ul,
            0x2828282828282800ul, 0x5050505050505000ul, 0xA0A0A0A0A0A0A000ul, 0x4040404040404000ul,
            0x0202020202020000ul, 0x0505050505050000ul, 0x0A0A0A0A0A0A0000ul, 0x1414141414140000ul,
            0x2828282828280000ul, 0x5050505050500000ul, 0xA0A0A0A0A0A00000ul, 0x4040404040400000ul,
            0x0202020202000000ul, 0x0505050505000000ul, 0x0A0A0A0A0A000000ul, 0x1414141414000000ul,
            0x2828282828000000ul, 0x5050505050000000ul, 0xA0A0A0A0A0000000ul, 0x4040404040000000ul,
            0x0202020200000000ul, 0x0505050500000000ul, 0x0A0A0A0A00000000ul, 0x1414141400000000ul,
            0x2828282800000000ul, 0x5050505000000000ul, 0xA0A0A0A000000000ul, 0x4040404000000000ul,
            0x0202020000000000ul, 0x0505050000000000ul, 0x0A0A0A0000000000ul, 0x1414140000000000ul,
            0x2828280000000000ul, 0x5050500000000000ul, 0xA0A0A00000000000ul, 0x4040400000000000ul,
            0x0202000000000000ul, 0x0505000000000000ul, 0x0A0A000000000000ul, 0x1414000000000000ul,
            0x2828000000000000ul, 0x5050000000000000ul, 0xA0A0000000000000ul, 0x4040000000000000ul,
            0x0200000000000000ul, 0x0500000000000000ul, 0x0A00000000000000ul, 0x1400000000000000ul,
            0x2800000000000000ul, 0x5000000000000000ul, 0xA000000000000000ul, 0x4000000000000000ul,

            #endregion BackwardPawnMasks data
        };

        public static readonly FixedArray2D<ulong> KingProximity = new (2, MAX_SQUARES)
        {
            #region KingProximity data

            // masks for D0
            0x0000000000000302ul, 0x0000000000000705ul, 0x0000000000000E0Aul, 0x0000000000001C14ul,
            0x0000000000003828ul, 0x0000000000007050ul, 0x000000000000E0A0ul, 0x000000000000C040ul,
            0x0000000000030203ul, 0x0000000000070507ul, 0x00000000000E0A0Eul, 0x00000000001C141Cul,
            0x0000000000382838ul, 0x0000000000705070ul, 0x0000000000E0A0E0ul, 0x0000000000C040C0ul,
            0x0000000003020300ul, 0x0000000007050700ul, 0x000000000E0A0E00ul, 0x000000001C141C00ul,
            0x0000000038283800ul, 0x0000000070507000ul, 0x00000000E0A0E000ul, 0x00000000C040C000ul,
            0x0000000302030000ul, 0x0000000705070000ul, 0x0000000E0A0E0000ul, 0x0000001C141C0000ul,
            0x0000003828380000ul, 0x0000007050700000ul, 0x000000E0A0E00000ul, 0x000000C040C00000ul,
            0x0000030203000000ul, 0x0000070507000000ul, 0x00000E0A0E000000ul, 0x00001C141C000000ul,
            0x0000382838000000ul, 0x0000705070000000ul, 0x0000E0A0E0000000ul, 0x0000C040C0000000ul,
            0x0003020300000000ul, 0x0007050700000000ul, 0x000E0A0E00000000ul, 0x001C141C00000000ul,
            0x0038283800000000ul, 0x0070507000000000ul, 0x00E0A0E000000000ul, 0x00C040C000000000ul,
            0x0302030000000000ul, 0x0705070000000000ul, 0x0E0A0E0000000000ul, 0x1C141C0000000000ul,
            0x3828380000000000ul, 0x7050700000000000ul, 0xE0A0E00000000000ul, 0xC040C00000000000ul,
            0x0203000000000000ul, 0x0507000000000000ul, 0x0A0E000000000000ul, 0x141C000000000000ul,
            0x2838000000000000ul, 0x5070000000000000ul, 0xA0E0000000000000ul, 0x40C0000000000000ul,

            // masks for D1
            0x0000000000070404ul, 0x00000000000F0808ul, 0x00000000001F1111ul, 0x00000000003E2222ul,
            0x00000000007C4444ul, 0x0000000000F88888ul, 0x0000000000F01010ul, 0x0000000000E02020ul,
            0x0000000007040404ul, 0x000000000F080808ul, 0x000000001F111111ul, 0x000000003E222222ul,
            0x000000007C444444ul, 0x00000000F8888888ul, 0x00000000F0101010ul, 0x00000000E0202020ul,
            0x0000000704040407ul, 0x0000000F0808080Ful, 0x0000001F1111111Ful, 0x0000003E2222223Eul,
            0x0000007C4444447Cul, 0x000000F8888888F8ul, 0x000000F0101010F0ul, 0x000000E0202020E0ul,
            0x0000070404040700ul, 0x00000F0808080F00ul, 0x00001F1111111F00ul, 0x00003E2222223E00ul,
            0x00007C4444447C00ul, 0x0000F8888888F800ul, 0x0000F0101010F000ul, 0x0000E0202020E000ul,
            0x0007040404070000ul, 0x000F0808080F0000ul, 0x001F1111111F0000ul, 0x003E2222223E0000ul,
            0x007C4444447C0000ul, 0x00F8888888F80000ul, 0x00F0101010F00000ul, 0x00E0202020E00000ul,
            0x0704040407000000ul, 0x0F0808080F000000ul, 0x1F1111111F000000ul, 0x3E2222223E000000ul,
            0x7C4444447C000000ul, 0xF8888888F8000000ul, 0xF0101010F0000000ul, 0xE0202020E0000000ul,
            0x0404040700000000ul, 0x0808080F00000000ul, 0x1111111F00000000ul, 0x2222223E00000000ul,
            0x4444447C00000000ul, 0x888888F800000000ul, 0x101010F000000000ul, 0x202020E000000000ul,
            0x0404070000000000ul, 0x08080F0000000000ul, 0x11111F0000000000ul, 0x22223E0000000000ul,
            0x44447C0000000000ul, 0x8888F80000000000ul, 0x1010F00000000000ul, 0x2020E00000000000ul,

            #endregion KingProximity data
        };

        public static readonly ulong[,,] PawnShieldMask = new ulong[MAX_COLORS, 2, MAX_SQUARES]
        {
            #region PawnShieldMask data

            // White masks
            {
                // Masks for D0
                {
                    0x0000000000000302ul, 0x0000000000000705ul, 0x0000000000000E0Aul, 0x0000000000001C14ul,
                    0x0000000000003828ul, 0x0000000000007050ul, 0x000000000000E0A0ul, 0x000000000000C040ul,
                    0x0000000000030200ul, 0x0000000000070500ul, 0x00000000000E0A00ul, 0x00000000001C1400ul,
                    0x0000000000382800ul, 0x0000000000705000ul, 0x0000000000E0A000ul, 0x0000000000C04000ul,
                    0x0000000003020000ul, 0x0000000007050000ul, 0x000000000E0A0000ul, 0x000000001C140000ul,
                    0x0000000038280000ul, 0x0000000070500000ul, 0x00000000E0A00000ul, 0x00000000C0400000ul,
                    0x0000000302000000ul, 0x0000000705000000ul, 0x0000000E0A000000ul, 0x0000001C14000000ul,
                    0x0000003828000000ul, 0x0000007050000000ul, 0x000000E0A0000000ul, 0x000000C040000000ul,
                    0x0000030200000000ul, 0x0000070500000000ul, 0x00000E0A00000000ul, 0x00001C1400000000ul,
                    0x0000382800000000ul, 0x0000705000000000ul, 0x0000E0A000000000ul, 0x0000C04000000000ul,
                    0x0003020000000000ul, 0x0007050000000000ul, 0x000E0A0000000000ul, 0x001C140000000000ul,
                    0x0038280000000000ul, 0x0070500000000000ul, 0x00E0A00000000000ul, 0x00C0400000000000ul,
                    0x0302000000000000ul, 0x0705000000000000ul, 0x0E0A000000000000ul, 0x1C14000000000000ul,
                    0x3828000000000000ul, 0x7050000000000000ul, 0xE0A0000000000000ul, 0xC040000000000000ul,
                    0x0200000000000000ul, 0x0500000000000000ul, 0x0A00000000000000ul, 0x1400000000000000ul,
                    0x2800000000000000ul, 0x5000000000000000ul, 0xA000000000000000ul, 0x4000000000000000ul,
                },
                // Masks for D1
                {
                    0x0000000000070404ul, 0x00000000000F0808ul, 0x00000000001F1111ul, 0x00000000003E2222ul,
                    0x00000000007C4444ul, 0x0000000000F88888ul, 0x0000000000F01010ul, 0x0000000000E02020ul,
                    0x0000000007040400ul, 0x000000000F080800ul, 0x000000001F111100ul, 0x000000003E222200ul,
                    0x000000007C444400ul, 0x00000000F8888800ul, 0x00000000F0101000ul, 0x00000000E0202000ul,
                    0x0000000704040000ul, 0x0000000F08080000ul, 0x0000001F11110000ul, 0x0000003E22220000ul,
                    0x0000007C44440000ul, 0x000000F888880000ul, 0x000000F010100000ul, 0x000000E020200000ul,
                    0x0000070404000000ul, 0x00000F0808000000ul, 0x00001F1111000000ul, 0x00003E2222000000ul,
                    0x00007C4444000000ul, 0x0000F88888000000ul, 0x0000F01010000000ul, 0x0000E02020000000ul,
                    0x0007040400000000ul, 0x000F080800000000ul, 0x001F111100000000ul, 0x003E222200000000ul,
                    0x007C444400000000ul, 0x00F8888800000000ul, 0x00F0101000000000ul, 0x00E0202000000000ul,
                    0x0704040000000000ul, 0x0F08080000000000ul, 0x1F11110000000000ul, 0x3E22220000000000ul,
                    0x7C44440000000000ul, 0xF888880000000000ul, 0xF010100000000000ul, 0xE020200000000000ul,
                    0x0404000000000000ul, 0x0808000000000000ul, 0x1111000000000000ul, 0x2222000000000000ul,
                    0x4444000000000000ul, 0x8888000000000000ul, 0x1010000000000000ul, 0x2020000000000000ul,
                    0x0400000000000000ul, 0x0800000000000000ul, 0x1100000000000000ul, 0x2200000000000000ul,
                    0x4400000000000000ul, 0x8800000000000000ul, 0x1000000000000000ul, 0x2000000000000000ul,
                },
            },
            // Black masks
            {
                // Masks for D0
                {
                    0x0000000000000002ul, 0x0000000000000005ul, 0x000000000000000Aul, 0x0000000000000014ul,
                    0x0000000000000028ul, 0x0000000000000050ul, 0x00000000000000A0ul, 0x0000000000000040ul,
                    0x0000000000000203ul, 0x0000000000000507ul, 0x0000000000000A0Eul, 0x000000000000141Cul,
                    0x0000000000002838ul, 0x0000000000005070ul, 0x000000000000A0E0ul, 0x00000000000040C0ul,
                    0x0000000000020300ul, 0x0000000000050700ul, 0x00000000000A0E00ul, 0x0000000000141C00ul,
                    0x0000000000283800ul, 0x0000000000507000ul, 0x0000000000A0E000ul, 0x000000000040C000ul,
                    0x0000000002030000ul, 0x0000000005070000ul, 0x000000000A0E0000ul, 0x00000000141C0000ul,
                    0x0000000028380000ul, 0x0000000050700000ul, 0x00000000A0E00000ul, 0x0000000040C00000ul,
                    0x0000000203000000ul, 0x0000000507000000ul, 0x0000000A0E000000ul, 0x000000141C000000ul,
                    0x0000002838000000ul, 0x0000005070000000ul, 0x000000A0E0000000ul, 0x00000040C0000000ul,
                    0x0000020300000000ul, 0x0000050700000000ul, 0x00000A0E00000000ul, 0x0000141C00000000ul,
                    0x0000283800000000ul, 0x0000507000000000ul, 0x0000A0E000000000ul, 0x000040C000000000ul,
                    0x0002030000000000ul, 0x0005070000000000ul, 0x000A0E0000000000ul, 0x00141C0000000000ul,
                    0x0028380000000000ul, 0x0050700000000000ul, 0x00A0E00000000000ul, 0x0040C00000000000ul,
                    0x0203000000000000ul, 0x0507000000000000ul, 0x0A0E000000000000ul, 0x141C000000000000ul,
                    0x2838000000000000ul, 0x5070000000000000ul, 0xA0E0000000000000ul, 0x40C0000000000000ul,
                },
                // Masks for D1
                {
                    0x0000000000000004ul, 0x0000000000000008ul, 0x0000000000000011ul, 0x0000000000000022ul,
                    0x0000000000000044ul, 0x0000000000000088ul, 0x0000000000000010ul, 0x0000000000000020ul,
                    0x0000000000000404ul, 0x0000000000000808ul, 0x0000000000001111ul, 0x0000000000002222ul,
                    0x0000000000004444ul, 0x0000000000008888ul, 0x0000000000001010ul, 0x0000000000002020ul,
                    0x0000000000040407ul, 0x000000000008080Ful, 0x000000000011111Ful, 0x000000000022223Eul,
                    0x000000000044447Cul, 0x00000000008888F8ul, 0x00000000001010F0ul, 0x00000000002020E0ul,
                    0x0000000004040700ul, 0x0000000008080F00ul, 0x0000000011111F00ul, 0x0000000022223E00ul,
                    0x0000000044447C00ul, 0x000000008888F800ul, 0x000000001010F000ul, 0x000000002020E000ul,
                    0x0000000404070000ul, 0x00000008080F0000ul, 0x00000011111F0000ul, 0x00000022223E0000ul,
                    0x00000044447C0000ul, 0x0000008888F80000ul, 0x0000001010F00000ul, 0x0000002020E00000ul,
                    0x0000040407000000ul, 0x000008080F000000ul, 0x000011111F000000ul, 0x000022223E000000ul,
                    0x000044447C000000ul, 0x00008888F8000000ul, 0x00001010F0000000ul, 0x00002020E0000000ul,
                    0x0004040700000000ul, 0x0008080F00000000ul, 0x0011111F00000000ul, 0x0022223E00000000ul,
                    0x0044447C00000000ul, 0x008888F800000000ul, 0x001010F000000000ul, 0x002020E000000000ul,
                    0x0404070000000000ul, 0x08080F0000000000ul, 0x11111F0000000000ul, 0x22223E0000000000ul,
                    0x44447C0000000000ul, 0x8888F80000000000ul, 0x1010F00000000000ul, 0x2020E00000000000ul,
                },
            }

            #endregion
        };

        public static readonly Bitboard[] FlankMask = new Bitboard[MAX_COORDS];
    }
}
