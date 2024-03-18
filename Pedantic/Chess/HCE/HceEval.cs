﻿using Pedantic.Collections;
using Pedantic.Tablebase;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pedantic.Chess.HCE
{
    public sealed class HceEval : IInitialize
    {
        public const int MAX_ATTACK_LEN = 8;
        public const ulong DARK_SQUARES_MASK = 0xAA55AA55AA55AA55ul;
        public const ulong LITE_SQUARES_MASK = 0x55AA55AA55AA55AAul;


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

        [InlineArray(MAX_PIECES + 1)]
        public struct AttackByArray
        {
            public const int CAPACITY = MAX_PIECES + 1;
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
            Span<EvalInfo> evalInfo = stackalloc EvalInfo[2];
            InitializeEvalInfo(board, evalInfo);
            short score = ComputeNormal(board, evalInfo, alpha, beta, ref isLazy);
            score = AdjustDraws(board, evalInfo, score);
            score = (short)(ColorToSign(board.SideToMove) * score);

            if (!isLazy)
            {
                cache.SaveEval(board.Hash, score, board.SideToMove);
            }

            return score;
        }

        public short ComputeNormal(Board board, Span<EvalInfo> evalInfo, int alpha, int beta, ref bool isLazy)
        {
            Score score = EvalMaterialAndPst(board, evalInfo, Color.White);
            score -= EvalMaterialAndPst(board, evalInfo, Color.Black);

            int normalScore = score.NormalizeScore(board.Phase);
            int evalScore = ColorToSign(board.SideToMove) * normalScore;
            if (evalScore < alpha - UciOptions.LzyEvalMargin || evalScore > beta + UciOptions.LzyEvalMargin)
            { 
                isLazy = true;
                return (short)normalScore;
            }

            score += ProbePawnCache(board, evalInfo);
            score += EvalMobility(board, evalInfo, Color.White);
            score -= EvalMobility(board, evalInfo, Color.Black);
            score += EvalKingSafety(board, evalInfo, Color.White);
            score -= EvalKingSafety(board, evalInfo, Color.Black);
            score += EvalPieces(board, evalInfo, Color.White);
            score -= EvalPieces(board, evalInfo, Color.Black);
            score += EvalPassedPawns(board, evalInfo, Color.White);
            score -= EvalPassedPawns(board, evalInfo, Color.Black);
            score += EvalThreats(board, evalInfo, Color.White);
            score -= EvalThreats(board, evalInfo, Color.Black);
            score += ColorToSign(board.SideToMove) * wts.TempoBonus;

            return score.NormalizeScore(board.Phase);
        }

        public Score ProbePawnCache(Board board, Span<EvalInfo> evalInfo)
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
                pawnScore = EvalPawns(board, evalInfo, Color.White) - EvalPawns(board, evalInfo, Color.Black);
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

        public Score EvalMaterialAndPst(Board board, Span<EvalInfo> evalInfo, Color color)
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

        public static Score EvalPawns(Board board, Span<EvalInfo> evalInfo, Color color)
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

                if ((otherPawns & PassedPawnMasks[c, (int)from]) == 0 && (pawns & friendMask) == 0)
                {
                    score += wts.PassedPawn(normalFrom);
                    evalInfo[c].PassedPawns |= sqMask;
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
                }
            }

            return score;
        }

        public static Score EvalMobility(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            Score score = Score.Zero;
            Bitboard exclude = board.Pawns | board.Kings;
            foreach (SquareIndex from in board.Units(color).AndNot(exclude))
            {
                Piece piece = board.PieceBoard(from).Piece;
                Bitboard pieceAttacks = Board.GetPieceMoves(piece, from, board.All);
                evalInfo[c].AttackBy[(int)piece] |= pieceAttacks;
                evalInfo[c].AttackBy[AttackBy.All] |= pieceAttacks;
                if (evalInfo[c].AttackCount < MAX_ATTACK_LEN)
                {
                    evalInfo[c].Attacks[evalInfo[c].AttackCount++] = pieceAttacks;
                }
                int mobility = (pieceAttacks & evalInfo[c].MobilityArea).PopCount;
                score += wts.PieceMobility(piece, mobility);
            }

            return score;
        }

        public static Score EvalKingSafety(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            Score score = Score.Zero;
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            // give a bonus to each attack on a square within 1 or 2 squares from the enemy king
            SquareIndex enemyKI = evalInfo[o].KI;
            for (int n = 0; n < evalInfo[c].AttackCount; n++)
            {
                Bitboard attacks = evalInfo[c].Attacks[n].AndNot(evalInfo[o].AttackBy[AttackBy.Pawn]);
                score += (attacks & (Bitboard)KingProximity[0, (int)enemyKI]).PopCount * wts.KingAttack(0);
                score += (attacks & (Bitboard)KingProximity[1, (int)enemyKI]).PopCount * wts.KingAttack(1);
            }

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
                Bitboard attacks = Board.GetBishopMoves(KI, board.All);
                int mobility = (attacks & evalInfo[o].MobilityArea).PopCount;
                score += wts.KsDiagonalMobility(mobility);
            }

            if (board.OrthogonalSliders(other) != 0)
            {
                Bitboard attacks = Board.GetRookMoves(KI, board.All);
                int mobility = (attacks & evalInfo[o].MobilityArea).PopCount;
                score += wts.KsOrthogonalMobility(mobility);
            }

            return score;
        }

        public static Score EvalPieces(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            Score score = Score.Zero;
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

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

            Bitboard pawns = evalInfo[c].Pawns;
            Bitboard otherPawns = evalInfo[o].Pawns;
            Bitboard allPawns = pawns | otherPawns;

            foreach (SquareIndex sq in board.Pieces(color, Piece.Rook))
            {
                Bitboard fileMask = new Bitboard(sq.File());
                if ((allPawns & fileMask) == 0)
                {
                    score += wts.RookOnOpenFile;
                }

                if ((pawns & fileMask) == 0 && (otherPawns & fileMask) != 0)
                {
                    score += wts.RookOnHalfOpenFile;
                }
            }

            return score;
        }

        public static Score EvalPassedPawns(Board board, Span<EvalInfo> evalInfo, Color color)
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
                if ((GetAttack(board, ppIndex, behind) & board.Pieces(color, Piece.Rook)) != 0)
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

        public static Score EvalThreats(Board board, Span<EvalInfo> evalInfo, Color color)
        {
            Score score = Score.Zero;
            Color other = color.Flip();
            int c = (int)color;
            int o = (int)other;

            Bitboard pawns = evalInfo[c].Pawns;
            Bitboard otherPawns = evalInfo[o].Pawns;
            Bitboard targets = board.Units(other).AndNot(otherPawns);
            Bitboard pushAttacks;

            if (targets == 0)
            {
                return score;
            }

            if (color == Color.White)
            {
                Bitboard pawnPushes = (pawns << 8).AndNot(board.All);
                pushAttacks = (pawnPushes.AndNot(Bitboard.BbFileA) << 7) |
                              (pawnPushes.AndNot(Bitboard.BbFileH) << 9);
            }
            else
            {
                Bitboard pawnPushes = (pawns >> 8).AndNot(board.All);
                pushAttacks = (pawnPushes.AndNot(Bitboard.BbFileH) >> 7) |
                              (pawnPushes.AndNot(Bitboard.BbFileA) >> 9);
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

            return score;
        }

        public static short AdjustDraws(Board board, Span<EvalInfo> evalInfo, short score)
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

        public static (sbyte Scale, sbyte Divisor) CalcDrawRatio(Board board, Span<EvalInfo> evalInfo, short score)
        {
            if (board.HalfMoveClock > 84)
            {
                return ((sbyte)Math.Max(100 - board.HalfMoveClock, 0), 16);
            }
            else if ((score > 0 && !evalInfo[0].CanWin) || (score < 0 && !evalInfo[1].CanWin))
            {
                return (1, 8);
            }
            else if (IsOcbEndgame(board, out int pcCount))
            {
                return ((sbyte)(pcCount == 1 ? 2 : 3), 4);
            }

            return (1, 1);
        }

        public static bool SufficientMatingMaterial(Board board, Span<EvalInfo> evalInfo, Color side)
        {
            int numKnights = board.Pieces(side, Piece.Knight).PopCount;
            int numBishops = board.Pieces(side, Piece.Bishop).PopCount;
            bool case1 = (board.Pieces(side, Piece.Rook) | board.Pieces(side, Piece.Queen)) != 0;
            bool case2 = (numKnights >= 1 && numBishops >= 1) || numBishops >= 2 || numKnights >= 3;
            return case1 || case2;
        }

        public static (bool WhiteCanWin, bool BlackCanWin) CanWin(Board board, Span<EvalInfo> evalInfo)
        {
            bool whiteCanWin = false, blackCanWin = false;
            int winMargin = wts.PieceValue(Piece.Pawn).NormalizeScore(board.Phase) * 4;

            if (evalInfo[0].Pawns != 0)
            {
                whiteCanWin = true;
            }
            else if (evalInfo[0].Material - evalInfo[1].Material >= winMargin)
            {
                whiteCanWin = SufficientMatingMaterial(board, evalInfo, Color.White);
            }

            if (evalInfo[1].Pawns != 0)
            {
                blackCanWin = true;
            }
            else if (evalInfo[1].Material - evalInfo[0].Material >= winMargin)
            {
                blackCanWin = SufficientMatingMaterial(board, evalInfo, Color.Black);
            }

            return (whiteCanWin, blackCanWin);
        }

        public int ScaleFactor(Color winningColor, Board board, Span<EvalInfo> evalInfo)
        {
            if (IsOcbEndgame(board, out int pcCount))
            {
                return pcCount == 1 ? 2 : 3;
            }

            return 4;
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

        public static void InitializeEvalInfo(Board board, Span<EvalInfo> evalInfo)
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
                Bitboard attacks = Board.KingMoves(evalInfo[c].KI);
                evalInfo[c].AttackBy[AttackBy.King] = attacks;
                evalInfo[c].AttackBy[AttackBy.All] = attacks;

                if (color == Color.White)
                {
                    attacks = (pawns.AndNot(Bitboard.BbFileA) << 7) 
                            | (pawns.AndNot(Bitboard.BbFileH) << 9);

                    evalInfo[c].CanCastle = (byte)(board.Castling & CastlingRights.WhiteRights);
                }
                else
                {
                    attacks = (pawns.AndNot(Bitboard.BbFileH) >> 7)
                            | (pawns.AndNot(Bitboard.BbFileA) >> 9);

                    evalInfo[c].CanCastle = (byte)((int)(board.Castling & CastlingRights.BlackRights) >> 2);
                }

                evalInfo[c].AttackBy[AttackBy.Pawn] = attacks;
                evalInfo[c].AttackBy[AttackBy.All] |= attacks;
            }

            /* two pass init */
            for (Color color = Color.White; color <= Color.Black; color++)
            {
                Color other = color.Flip();
                int c = (int)color;
                int o = (int)other;
                evalInfo[c].MobilityArea = ~(board.Units(color) | evalInfo[o].AttackBy[AttackBy.Pawn]);
            }

            var canWin = CanWin(board, evalInfo);
            evalInfo[0].CanWin = canWin.WhiteCanWin;
            evalInfo[1].CanWin = canWin.BlackCanWin;
        }

        public static Bitboard GetAttack(Board board, SquareIndex from, Direction dir)
        {
            Bitboard blockers = board.All;
            Bitboard bbRay = Board.Vectors[(int)from][dir];
            return dir switch
            {
                Direction.North     => bbRay.AndNot(Board.Vectors[(bbRay & blockers).TzCount].North),
                Direction.NorthEast => bbRay.AndNot(Board.Vectors[(bbRay & blockers).TzCount].NorthEast),
                Direction.East      => bbRay.AndNot(Board.Vectors[(bbRay & blockers).TzCount].East),
                Direction.SouthEast => bbRay.AndNot(Board.RevVectors[(bbRay & blockers).LzCount].SouthEast),
                Direction.South     => bbRay.AndNot(Board.RevVectors[(bbRay & blockers).LzCount].South),
                Direction.SouthWest => bbRay.AndNot(Board.RevVectors[(bbRay & blockers).LzCount].SouthWest),
                Direction.West      => bbRay.AndNot(Board.RevVectors[(bbRay & blockers).LzCount].West),
                Direction.NorthWest => bbRay.AndNot(Board.Vectors[(bbRay & blockers).TzCount].NorthWest),
                _ => Bitboard.None
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rank PromoteRank(Color color)
        {
            return color == Color.White ? Rank.Rank8 : Rank.Rank1;
        }

        public static void Initialize() {}

        public readonly Bitboard BB_KS_MASK = new Bitboard(0xf0f0f0f0f0f0f0f0ul);
        public readonly Bitboard BB_QS_MASK = new Bitboard(0x0f0f0f0f0f0f0f0ful);

        private readonly EvalCache cache;
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
    }
}
