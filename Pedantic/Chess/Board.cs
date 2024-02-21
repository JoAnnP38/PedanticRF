using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Pedantic.Collections;
using Pedantic.Chess.HCE;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public sealed partial class Board : ICloneable, IInitialize
    {
        #region Inner Data Structures

        [InlineArray(MAX_SQUARES)]
        public struct SquareArray
        {
            public const int CAPACITY = MAX_SQUARES;
            private Square _element0;

            public Square this[SquareIndex index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this[(int)index];

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => this[(int)index] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<Square> AsSpan()
            {
                return MemoryMarshal.CreateSpan(ref _element0, CAPACITY);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                AsSpan().Clear();
            }
        }

        [InlineArray(9)]
        public struct BitboardArray
        {
            public const int CAPACITY = 9;
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

        public struct BoardState
        {
            public Color SideToMove;
            public CastlingRights Castling;
            public SquareIndex EnPassant;
            public SquareIndex EnPassantValidated;
            public byte HalfMoveClock;
            public byte Phase;
            public KingBuckets KingBuckets;
            public ushort FullMoveCounter;
            public ulong Hash;
            public BitboardArray Bitboards;
            public SquareArray PieceBoard;
            public ByColor<SquareIndex> KingIndex;
            public ByColor<Score> Material;
            public Move Move;

            public BoardState(Board board)
            {
                SideToMove = board.sideToMove;
                Castling = board.castling;
                EnPassant = board.enPassant;
                EnPassantValidated = board.enPassantValidated;
                HalfMoveClock = board.halfMoveClock;
                FullMoveCounter = board.fullMoveCounter;
                Phase = board.phase;
                Hash = board.hash;
                Bitboards = board.bitboards;
                PieceBoard = board.board;
                KingIndex = board.kingIndex;
                Material = board.material;
            }

            public void Restore(Board board)
            {
                board.sideToMove = SideToMove;
                board.castling = Castling;
                board.enPassant = EnPassant;
                board.enPassantValidated = EnPassantValidated;
                board.halfMoveClock = HalfMoveClock;
                board.fullMoveCounter = FullMoveCounter;
                board.phase = Phase;
                board.hash = Hash;
                board.bitboards = Bitboards;
                board.board = PieceBoard;
                board.kingIndex = KingIndex;
                board.material = Material;
            }
        }

        public abstract class GenMoveHelper
        {
            public GenMoveHelper(Board board)
            {
                this.board = board;
            }

            public Rank PromoteRank { get; protected set; }

            public abstract Bitboard PawnsPromoting(in EvasionInfo info);
            public abstract (Bitboard SingleSquare, Bitboard DoubleSquare) PawnsMoving(in EvasionInfo info);
            public abstract (Bitboard CaptureLeft, Bitboard CaptureRight) PawnsCapturing(in EvasionInfo info);
            public abstract void GenerateCastles(MoveList list);
            public abstract Bitboard EnemyPawnAttacks();

            protected Board board;
        }

        class WhiteGenMoveHelper : GenMoveHelper
        {
            public WhiteGenMoveHelper(Board board) : base(board)
            {
                PromoteRank = Rank.Rank8;
            }

            public override void GenerateCastles(MoveList list)
            {
                if ((board.castling & CastlingRights.WhiteKingSide) != 0 && (whiteKingSideClearMask & board.All) == 0)
                {
                    list.AddQuiet(Color.White, Piece.King, SquareIndex.E1, SquareIndex.G1, MoveType.Castle);
                }

                if ((board.castling & CastlingRights.WhiteQueenSide) != 0 && (whiteQueenSideClearMask & board.All) == 0)
                {
                    list.AddQuiet(Color.White, Piece.King, SquareIndex.E1, SquareIndex.C1, MoveType.Castle);
                }
            }

            public override (Bitboard CaptureLeft, Bitboard CaptureRight) PawnsCapturing(in EvasionInfo info)
            {
                Bitboard pawns = board.Pieces(Color.White, Piece.Pawn);
                Bitboard enemies = board.Units(Color.Black) & info.CaptureMask;
                Bitboard bb1 = pawns & (enemies.AndNot(bbFileH) >> 7);
                Bitboard bb2 = pawns & (enemies.AndNot(bbFileA) >> 9);
                return (bb1, bb2);
            }

            public override (Bitboard SingleSquare, Bitboard DoubleSquare) PawnsMoving(in EvasionInfo info)
            {
                Bitboard pawns = board.Pieces(Color.White, Piece.Pawn);
                Bitboard validMoveTo = info.PushMask.AndNot(board.All);
                Bitboard bb1 = pawns.AndNot(bbRank7) & (validMoveTo >> 8);
                Bitboard bb2 = pawns.AndNot(bbRank7) & (~board.All >> 8);
                bb2 &= bbRank2 & (validMoveTo >> 16);
                return (bb1, bb2);
            }

            public override Bitboard PawnsPromoting(in EvasionInfo info)
            {
                Bitboard pawns = board.Pieces(Color.White, Piece.Pawn);
                Bitboard validMoveTo = info.PushMask.AndNot(board.All);
                return pawns & bbRank7 & (validMoveTo >> 8);
            }

            public override Bitboard EnemyPawnAttacks()
            {
                Bitboard enemyPawns = board.Pieces(board.Opponent, Piece.Pawn);
                return (enemyPawns.AndNot(bbFileH) >> 7) | (enemyPawns.AndNot(bbFileA) >> 9);
            }
        }

        class BlackGenMoveHelper : GenMoveHelper
        {
            public BlackGenMoveHelper(Board board) : base(board)
            {
                PromoteRank = Rank.Rank1;
            }

            public override void GenerateCastles(MoveList list)
            {
                if ((board.castling & CastlingRights.BlackKingSide) != 0 && (blackKingSideClearMask & board.All) == 0)
                { 
                    list.AddQuiet(Color.Black, Piece.King, SquareIndex.E8, SquareIndex.G8, MoveType.Castle);
                }

                if ((board.castling & CastlingRights.BlackQueenSide) != 0 && (blackQueenSideClearMask & board.All) == 0)
                {
                    list.AddQuiet(Color.Black, Piece.King, SquareIndex.E8, SquareIndex.C8, MoveType.Castle);
                }
            }

            public override (Bitboard CaptureLeft, Bitboard CaptureRight) PawnsCapturing(in EvasionInfo info)
            {
                Bitboard pawns = board.Pieces(Color.Black, Piece.Pawn);
                Bitboard enemies = board.Units(Color.White) & info.CaptureMask;
                Bitboard bb1 = pawns & (enemies.AndNot(bbFileH) << 9);
                Bitboard bb2 = pawns & (enemies.AndNot(bbFileA) << 7);
                return (bb1, bb2);
            }

            public override (Bitboard SingleSquare, Bitboard DoubleSquare) PawnsMoving(in EvasionInfo info)
            {
                Bitboard pawns = board.Pieces(Color.Black, Piece.Pawn);
                Bitboard validMoveTo = info.PushMask.AndNot(board.All);
                Bitboard bb1 = pawns.AndNot(bbRank2) & (validMoveTo << 8);
                Bitboard bb2 = pawns.AndNot(bbRank2) & (~board.All << 8);
                bb2 &= bbRank7 & (validMoveTo << 16);
                return (bb1, bb2);
            }

            public override Bitboard PawnsPromoting(in EvasionInfo info)
            {
                Bitboard pawns = board.Pieces(Color.Black, Piece.Pawn);
                Bitboard validMoveTo = info.PushMask.AndNot(board.All);
                return pawns & bbRank2 & (validMoveTo << 8);
            }

            public override Bitboard EnemyPawnAttacks()
            {
                Bitboard enemyPawns = board.Pieces(board.Opponent, Piece.Pawn);
                return (enemyPawns.AndNot(bbFileA) << 7) | (enemyPawns.AndNot(bbFileH) << 9);
            }
        }

        public readonly struct CastlingRookMove
        {
            public readonly SquareIndex KingFrom;
            public readonly SquareIndex KingTo;
            public readonly SquareIndex KingMoveThrough;
            public readonly SquareIndex RookFrom;
            public readonly SquareIndex RookTo;
            public readonly CastlingRights CastlingMask;
            public readonly Bitboard ClearMask;

            public CastlingRookMove(SquareIndex kingFrom, SquareIndex kingTo, SquareIndex kingMoveThrough, 
                SquareIndex rookFrom, SquareIndex rookTo, CastlingRights mask, Bitboard clearMask)
            {
                KingFrom = kingFrom;
                KingTo = kingTo;
                KingMoveThrough = kingMoveThrough;
                RookFrom = rookFrom;
                RookTo = rookTo;
                CastlingMask = mask;
                ClearMask = clearMask;
            }
        }

        public readonly struct EvasionInfo
        {
            // # of pieces checking the king
            public readonly int CheckerCount;

            // squares that are attacked by opponent so king cannot move there
            public readonly Bitboard KingDanger;

            // valid squares to capture when in check
            public readonly Bitboard CaptureMask;

            // valid squares to move to when in check
            public readonly Bitboard PushMask;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EvasionInfo()
            {
                CheckerCount = 0;
                KingDanger = bbNone;
                CaptureMask = bbAll;
                PushMask = bbAll;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EvasionInfo(int checkerCount, Bitboard kingDanger, Bitboard captureMask, Bitboard pushMask)
            {
                CheckerCount = checkerCount;
                KingDanger = kingDanger;
                CaptureMask = captureMask;
                PushMask = pushMask;
            }
        }

        #endregion

        #region Member data

        private SquareArray board;
        private BitboardArray bitboards;
        private Color sideToMove;
        private CastlingRights castling;
        private SquareIndex enPassant;
        private SquareIndex enPassantValidated;
        private byte halfMoveClock;
        private ushort fullMoveCounter;
        private ulong hash;
        private ByColor<SquareIndex> kingIndex;
        private ByColor<GenMoveHelper> helpers;
        private ByColor<Score> material;

        private MoveList moveList = new();
        private ValueStack<BoardState> gameStack = new(MAX_GAME_LENGTH);
        private byte phase;

        #endregion

        #region Constructors

        static Board()
        { 
            Util.Assert(Vectors.Count == 65);
            Util.Assert(RevVectors.Count == 65);

            for (int sq = 0; sq < MAX_SQUARES; ++sq)
            {
                RevVectors[63 - sq] = Vectors[sq];
            }
            InitFancyMagic();
            InitPext();
            IsPextSupported = PextSupported();
        }

        public Board()
        {
            Clear();
            helpers[Color.White] = new WhiteGenMoveHelper(this);
            helpers[Color.Black] = new BlackGenMoveHelper(this);
        }

        public Board(string fen) : this()
        { 
            LoadFen(fen);
        }

        public Board(ReadOnlySpan<char> fen) : this()
        {
            LoadFen(fen);
        }

        private Board(Board other) : this()
        { 
            board = other.board;
            bitboards = other.bitboards;
            sideToMove = other.sideToMove;
            castling = other.castling;
            enPassant = other.enPassant;
            enPassantValidated = other.enPassantValidated;
            halfMoveClock = other.halfMoveClock;
            fullMoveCounter = other.fullMoveCounter;
            hash = other.hash;
            kingIndex = other.kingIndex;
            material = other.material;
            phase = other.phase;
        }

        #endregion

        #region Accessors

        public Bitboard All
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[0];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[0] = value;
        }

        public Bitboard WhitePieces
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Color.White + 1];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Color.White + 1] = value;
        }

        public Bitboard BlackPieces
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Color.Black + 1];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Color.Black + 1] = value;
        }

        public Bitboard Pawns
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Piece.Pawn + 3];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Piece.Pawn + 3] = value;
        }

        public Bitboard Knights
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Piece.Knight + 3];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Piece.Knight + 3] = value;
        }

        public Bitboard Bishops
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Piece.Bishop + 3];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Piece.Bishop + 3] = value;
        }

        public Bitboard Rooks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Piece.Rook + 3];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Piece.Rook + 3] = value;
        }

        public Bitboard Queens
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Piece.Queen + 3];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Piece.Queen + 3] = value;
        }

        public Bitboard Kings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => bitboards[(int)Piece.King + 3];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => bitboards[(int)Piece.King + 3] = value;
        }

        public Color SideToMove
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => sideToMove;
        }

        public Color Opponent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => sideToMove.Flip();
        }

        public CastlingRights Castling
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => castling;
        }

        public SquareIndex EnPassant 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => enPassant;
        }

        public SquareIndex EnPassantValidated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => enPassantValidated;
        }

        public ulong Hash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => hash;
        }

        public byte HalfMoveClock
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => halfMoveClock;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Bitboard Units(Color color)
        {
            Util.Assert(color != Color.None);
            return ref bitboards[(int)color + 1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Bitboard Pieces(Piece piece)
        {
            Util.Assert(piece != Piece.None);
            return ref bitboards[(int)piece + 3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard Pieces(Color color, Piece piece)
        {
            return Units(color) & Pieces(piece);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard DiagonalSliders(Color color)
        {
            return Units(color) & (Bishops | Queens);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard OrthogonalSliders(Color color)
        {
            return Units(color) & (Rooks | Queens);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard MajorPieces(Color color)
        {
            return Units(color) & (Rooks | Queens);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard MinorPieces(Color color)
        {
            return Units(color) & (Knights & Bishops);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square PieceBoard(SquareIndex sq)
        {
            Util.Assert(sq != SquareIndex.None);
            return board[(int)sq];
        }

        public int PieceCount(Color sideToMove)
        {
            return (Units(sideToMove).AndNot(Pieces(sideToMove, Piece.Pawn))).PopCount;
        }

        public ref ByColor<SquareIndex> KingIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref kingIndex;
        }

        public ref ByColor<Score> Material
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref material;
        }

        public byte Phase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => phase;
        }

        public GamePhase GamePhase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return phase switch
                {
                    > 54 => GamePhase.Opening,
                    < 16 => GamePhase.EndGame,
                    _ => GamePhase.MidGame
                };
            }
        }

        public Move LastMove
        {
            get
            {
                if (gameStack.Count > 0)
                {
                    return gameStack.AsSpan()[^1].Move;
                }

                return Move.NullMove;
            }
        }

        public Move PrevLastMove
        {
            get
            {
                if (gameStack.Count > 1)
                {
                    return gameStack.AsSpan()[^2].Move;
                }

                return Move.NullMove;
            }
        }

        #endregion

        #region State management

        public void Clear()
        {
            board.Clear();
            bitboards.Clear();
            halfMoveClock = 0;
            fullMoveCounter = 0;
            sideToMove = Color.None;
            castling = CastlingRights.None;
            enPassant = SquareIndex.None;
            enPassantValidated = SquareIndex.None;
            hash = 0;
            kingIndex.Fill(SquareIndex.None);
            material.Clear();
            phase = 0;
        }

        public void AddPiece(Color color, Piece piece, SquareIndex sq)
        {
            Util.Assert(color != Color.None);
            Util.Assert(piece != Piece.None);
            Util.Assert(sq != SquareIndex.None);
            Util.Assert(board[(int)sq].IsEmpty);

            board[(int)sq] = new Square(color, piece);
            Bitboard pcMask = new Bitboard(sq);
            All |= pcMask;
            Units(color) |= pcMask;
            Pieces(piece) |= pcMask;
            hash = ZobristHash.HashPiece(hash, color, piece, sq);
            if (piece == Piece.King)
            {
                kingIndex[color] = sq;
            }
            material[color] += HceEval.Weights.PieceValue(piece);
            phase = (byte)Math.Min(phase + piece.PhaseValue(), MAX_PHASE);
        }

        public void RemovePiece(Color color, Piece piece, SquareIndex sq)
        {
            Util.Assert(color != Color.None);
            Util.Assert(piece != Piece.None);
            Util.Assert(sq != SquareIndex.None);
            Util.Assert(board[(int)sq].Color == color && board[(int)sq].Piece == piece);

            board[(int)sq] = Square.Empty;
            Bitboard pcMask = ~new Bitboard(sq);
            All &= pcMask;
            Units(color) &= pcMask;
            Pieces(piece) &= pcMask;
            hash = ZobristHash.HashPiece(hash, color, piece, sq);
            material[color] -= HceEval.Weights.PieceValue(piece);
            phase = (byte)Math.Max(phase - piece.PhaseValue(), 0);
        }

        #endregion

        #region Move Generation

        public void GenerateMoves(MoveList list)
        {
            SquareIndex kingIndex = KingIndex[sideToMove];
            EvasionInfo info = new();
            GenerateCaptures(kingIndex, list, in info);
            GeneratePromotions(list, in info);
            GenerateQuiets(kingIndex, list, in info);
        }

        public void GenerateEvasions(MoveList list)
        {
            SquareIndex kingIndex = KingIndex[sideToMove];
            GenMoveHelper helper = helpers[sideToMove];
            EvasionInfo info = GetEvasionInfo(kingIndex, helper);

            GenerateKingCaptures(kingIndex, list, in info);
            GenerateKingQuiets(kingIndex, list, in info);

            if (info.CheckerCount > 1)
            {
                return;
            }

            GeneratePawnCaptures(list, in info, helper);
            GeneratePieceCaptures(list, in info);
            GeneratePromotions(list, in info);
            GeneratePawnQuiets(list, in info, helper);
            GeneratePieceQuiets(list, in info);
        }

        public void GenerateCaptures(SquareIndex kingIndex, MoveList list, in EvasionInfo info)
        {
            GenMoveHelper helper = helpers[sideToMove];
            GeneratePawnCaptures(list, in info, helper);
            GeneratePieceCaptures(list, in info);
            GenerateKingCaptures(kingIndex, list, in info);
        }

        public void GeneratePromotions(MoveList list, in EvasionInfo info)
        {
            GenMoveHelper helper = helpers[sideToMove];
            foreach (SquareIndex from in helper.PawnsPromoting(in info))
            {
                SquareIndex to = PawnPlus(sideToMove, from);
                list.AddPromote(sideToMove, from, to, Piece.Queen);
                list.AddPromote(sideToMove, from, to, Piece.Knight);
                list.AddPromote(sideToMove, from, to, Piece.Rook);
                list.AddPromote(sideToMove, from, to, Piece.Bishop);
            }
        }

        public void GenerateQuiets(SquareIndex kingIndex, MoveList list, in EvasionInfo info)
        {
            GenMoveHelper helper = helpers[sideToMove];
            GeneratePawnQuiets(list, in info, helper);
            GeneratePieceQuiets(list, in info);
            helper.GenerateCastles(list);
            GenerateKingQuiets(kingIndex, list, in info);
        }

        public void GeneratePawnQuiets(MoveList list, in EvasionInfo info, GenMoveHelper helper)
        {
            var pawnsMoving = helper.PawnsMoving(in info);

            foreach (SquareIndex from in pawnsMoving.SingleSquare)
            {
                SquareIndex to = PawnPlus(sideToMove, from);
                list.AddQuiet(sideToMove, Piece.Pawn, from, to, MoveType.PawnMove);
            }

            foreach (SquareIndex from in pawnsMoving.DoubleSquare)
            {
                SquareIndex to = PawnDouble(sideToMove, from);
                list.AddQuiet(sideToMove, Piece.Pawn, from, to, MoveType.DblPawnMove);
            }
        }

        private void GenerateEnPassant(MoveList list, in EvasionInfo info)
        { 
            if (enPassantValidated != SquareIndex.None)
            {
                Bitboard epMask = new(enPassantValidated);
                Bitboard captMask = new((SquareIndex)((int)enPassantValidated + EpOffset(sideToMove)));

                if ((epMask & info.PushMask) != 0 || (captMask & info.CaptureMask) != 0)
                {
                    Bitboard bb = Pieces(sideToMove, Piece.Pawn) & PawnDefends(sideToMove, enPassantValidated);
                    foreach (SquareIndex from in bb)
                    {
                        list.AddCapture(sideToMove, Piece.Pawn, from, enPassantValidated, MoveType.EnPassant, Piece.Pawn);
                    }
                }
            }
        }

        public void GeneratePawnCaptures(MoveList list, in EvasionInfo info, GenMoveHelper helper)
        {
            GenerateEnPassant(list, in info);

            var pawnsCapturing = helper.PawnsCapturing(in info);

            foreach (SquareIndex from in pawnsCapturing.CaptureLeft)
            {
                SquareIndex to = PawnLeft(sideToMove, from);
                Piece capture = PieceBoard(to).Piece;
                if (to.Rank() == helper.PromoteRank)
                {
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Queen);
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Knight);
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Rook);
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Bishop);
                }
                else
                {
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.Capture, capture);
                }
            }

            foreach (SquareIndex from in pawnsCapturing.CaptureRight)
            {
                SquareIndex to = PawnRight(sideToMove, from);
                Piece capture = PieceBoard(to).Piece;
                if (to.Rank() == helper.PromoteRank)
                {
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Queen);
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Knight);
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Rook);
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.PromoteCapture, capture, Piece.Bishop);
                }
                else
                {
                    list.AddCapture(sideToMove, Piece.Pawn, from, to, MoveType.Capture, capture);
                }
            }
        }

        public void GenerateKingQuiets(SquareIndex kingIndex, MoveList list, in EvasionInfo info)
        {
            Bitboard attacks = GetPieceMoves(Piece.King, kingIndex, All).AndNot(All | info.KingDanger);
            foreach (SquareIndex to in attacks)
            {
                list.AddQuiet(sideToMove, Piece.King, kingIndex, to);
            }
        }

        public void GeneratePieceQuiets(MoveList list, in EvasionInfo info)
        {
            for (Piece piece = Piece.Knight; piece <= Piece.Queen; piece++)
            {
                foreach (SquareIndex from in Pieces(sideToMove, piece))
                {
                    Bitboard attacks = GetPieceMoves(piece, from, All).AndNot(All) & info.PushMask;

                    foreach (SquareIndex to in attacks)
                    {
                        list.AddQuiet(sideToMove, piece, from, to);
                    }
                }
            }
        }

        public void GenerateKingCaptures(SquareIndex kingIndex, MoveList list, in EvasionInfo info)
        {
            Bitboard attacks = GetPieceMoves(Piece.King, kingIndex, All) & Units(Opponent);
            attacks = attacks.AndNot(info.KingDanger);
            foreach (SquareIndex to in attacks)
            {
                list.AddCapture(sideToMove, Piece.King, kingIndex, to, MoveType.Capture, PieceBoard(to).Piece);
            }
        }

        public void GeneratePieceCaptures(MoveList list, in EvasionInfo info)
        {
            for (Piece piece = Piece.Knight; piece <= Piece.Queen; piece++)
            {
                foreach (SquareIndex from in Pieces(sideToMove, piece))
                {
                    Bitboard attacks = GetPieceMoves(piece, from, All) & Units(Opponent) & info.CaptureMask;

                    foreach (SquareIndex to in attacks)
                    {
                        list.AddCapture(sideToMove, piece, from, to, MoveType.Capture, PieceBoard(to).Piece);
                    }
                }
            }
        }

        public bool OneLegalMove(MoveList list, out Move move)
        {
            int legalCount = 0;
            move = Move.NullMove;
            GenerateMoves(list);

            for (int n = 0; n < list.Count && legalCount <= 1; n++)
            {
                if (MakeMove(list[n]))
                {
                    if (legalCount == 0)
                    {
                        move = list[n];
                    }
                    UnmakeMove();
                    ++legalCount;
                }
            }

            return legalCount == 1;
        }

        public bool IsLegalMove(Move move)
        {
            bool isLegal = false;
            EvasionInfo info = new();
            GenMoveHelper helper = helpers[sideToMove];
            SquareIndex kingIndex = KingIndex[sideToMove];
            moveList.Clear();

            if (move.Stm == SideToMove)
            { 
                switch ((move.Piece, move.Type))
                {
                    case (Piece.Pawn, MoveType.PawnMove):
                    case (Piece.Pawn, MoveType.DblPawnMove):
                        GeneratePawnQuiets(moveList, in info, helper);
                        break;

                    case (Piece.Pawn, MoveType.EnPassant):
                    case (Piece.Pawn, MoveType.Capture):
                    case (Piece.Pawn, MoveType.PromoteCapture):
                        GeneratePawnCaptures(moveList, in info, helper);
                        break;

                    case (Piece.Pawn, MoveType.Promote):
                        GeneratePromotions(moveList, in info);
                        break;

                    case (Piece.King, MoveType.Castle):
                        helper.GenerateCastles(moveList);
                        break;

                    case (Piece.King, MoveType.Capture):
                        GenerateKingCaptures(kingIndex, moveList, in info);
                        break;

                    case (Piece.King, MoveType.Normal):
                        GenerateKingQuiets(kingIndex, moveList, in info);
                        break;

                    case (>= Piece.Knight and <= Piece.Queen, MoveType.Capture):
                        GeneratePieceCaptures(moveList, in info);
                        break;

                    case (>= Piece.Knight and <= Piece.Queen, MoveType.Normal):
                        GeneratePieceQuiets(moveList, in info);
                        break;

                    default:
                        // no moves
                        break;
                }

                for (int n = 0; n < moveList.Count; n++)
                {
                    if (moveList[n] == move)
                    {
                        if (MakeMove(move))
                        {
                            UnmakeMove();
                            isLegal = true;
                        }
                        break;
                    }
                }
            }

            return isLegal;
        }

        public bool IsPseudoLegal(Move move)
        {
            if (move.Stm == sideToMove)
            {
                if (move.Type == MoveType.EnPassant)
                {
                    return board[move.From].Piece == Piece.Pawn && enPassantValidated == move.To && 
                           board[move.To].IsEmpty && board[(int)move.To + EpOffset(move.Stm)].Piece == Piece.Pawn;
                }
                else if (move.Type == MoveType.Castle)
                {
                    CastlingRookMove rookMove = LookupRookMoves(move.To);
                    return board[move.From].Piece == Piece.King && board[rookMove.RookFrom].Piece == Piece.Rook &&
                           (All & rookMove.ClearMask) == 0ul && (castling & rookMove.CastlingMask) == rookMove.CastlingMask;
                }
                Square captureSquare = Square.Empty;
                if (move.IsCapture)
                {
                    captureSquare = new Square(move.Stm.Flip(), move.Capture);
                }
                return board[move.From].Piece == move.Piece && board[move.From].Color == move.Stm && 
                       board[move.To] == captureSquare;            
            }
            return false;
        }

        public IEnumerable<GenMove> Moves(int ply, SearchStack ss, MoveList list, Move ttMove)
        {
            if (ttMove != Move.NullMove)
            {
                yield return new GenMove(ttMove, MoveGenPhase.HashMove);
            }

            list.Clear();
            SquareIndex kingIndex = KingIndex[sideToMove];
            EvasionInfo info = new();
            GenerateCaptures(kingIndex, list, in info);
            list.Remove(ttMove);

            for (int n = 0; n < list.Count; n++)
            {
                yield return new GenMove(list.Sort(n), MoveGenPhase.GoodCapture);
            }

            list.Clear();
            GeneratePromotions(list, in info);
            list.Remove(ttMove);

            for (int n = 0; n < list.Count; n++)
            {
                yield return new GenMove(list.Sort(n), MoveGenPhase.Promotion);
            }

            list.Clear();
            GenerateQuiets(kingIndex, list, in info);
            list.Remove(ttMove);

            Move killer = ss[ply].Killers.Move1;
            if (list.Remove(killer))
            {
                yield return new GenMove(killer, MoveGenPhase.Killers);
            }

            killer = ss[ply].Killers.Move2;
            if (list.Remove(killer))
            {
                yield return new GenMove(killer, MoveGenPhase.Killers);
            }

            for (int n = 0; n < list.Count; n++)
            {
                yield return new GenMove(list.Sort(n), MoveGenPhase.Quiet);
            }
        }

        public IEnumerable<GenMove> QMoves(int ply, int qsPly, SearchStack ss, MoveList list, Move ttMove)
        {
            if (ttMove != Move.NullMove)
            {
                yield return new GenMove(ttMove, MoveGenPhase.HashMove);
            }

            list.Clear();
            SquareIndex kingIndex = KingIndex[sideToMove];
            EvasionInfo info = new();

            if (qsPly >= UciOptions.RecaptureDepth)
            {
                info = new EvasionInfo(0, Bitboard.None, new Bitboard(ss[ply - 1].Move.To), Bitboard.None);
            }
            GenerateCaptures(kingIndex, list, in info);
            list.Remove(ttMove);

            for (int n = 0; n < list.Count; n++)
            {
                yield return new GenMove(list.Sort(n), MoveGenPhase.GoodCapture);
            }

            if (qsPly < UciOptions.PromotionDepth)
            {
                list.Clear();
                info = new();
                GeneratePromotions(list, in info);
                list.Remove(ttMove);

                for (int n = 0; n < list.Count; n++)
                {
                    yield return new GenMove(list.Sort(n), MoveGenPhase.Promotion);
                }
            }
        }

        public IEnumerable<GenMove> EvasionMoves(int ply, SearchStack ss, MoveList list, Move ttMove)
        {
            if (ttMove != Move.NullMove)
            {
                yield return new GenMove(ttMove, MoveGenPhase.HashMove);
            }

            SquareIndex kingIndex = KingIndex[sideToMove];
            GetEvasionInfo(kingIndex, out EvasionInfo info, out GenMoveHelper helper);
            list.Clear();
            GenerateKingCaptures(kingIndex, list, in info);
            if (info.CheckerCount <= 1)
            {
                GeneratePawnCaptures(list, in info, helper);
                GeneratePieceCaptures(list, in info);
            }

            list.Remove(ttMove);
            for (int n = 0; n < list.Count; n++)
            {
                yield return new GenMove(list.Sort(n), MoveGenPhase.GoodCapture);
            }

            if (info.CheckerCount <= 1)
            {
                list.Clear();
                GeneratePromotions(list, in info);
                list.Remove(ttMove);
                for (int n = 0; n < list.Count; n++)
                {
                    yield return new GenMove(list.Sort(n), MoveGenPhase.Promotion);
                }
            }

            list.Clear();
            GenerateKingQuiets(kingIndex, list, in info);
            if (info.CheckerCount <= 1)
            {
                GeneratePawnQuiets(list, in info, helper);
                GeneratePieceQuiets(list, in info);
            }
            list.Remove(ttMove);

            Move killer = ss[ply].Killers.Move1;
            if (list.Remove(killer))
            {
                yield return new GenMove(killer, MoveGenPhase.Killers);
            }

            killer = ss[ply].Killers.Move2;
            if (list.Remove(killer))
            {
                yield return new GenMove(killer, MoveGenPhase.Killers);
            }

            for (int n = 0; n < list.Count; n++)
            {
                yield return new GenMove(list.Sort(n), MoveGenPhase.Quiet);
            }
        }

        #endregion

        #region Make/Unmake Moves

        public bool MakeMove(Move move)
        {
            PushBoardState();
            bool result = MakeMoveNs(move);
            if (!result)
            {
                PopBoardState();
            }

            return result;
        }

        public bool MakeMoveNs(Move move)
        {
            ulong oldHash = hash;

            if (enPassantValidated != SquareIndex.None)
            {
                hash = ZobristHash.HashEnPassant(hash, enPassantValidated);
            }

            enPassant = enPassantValidated = SquareIndex.None;
            hash = ZobristHash.HashCastling(hash, castling);
            Color opponent = Opponent;

            switch (move.Type)
            {
                case MoveType.Normal:
                    RemovePiece(sideToMove, move.Piece, move.From);
                    AddPiece(sideToMove, move.Piece, move.To);
                    castling &= CastleMask(move.From, move.To);
                    halfMoveClock++;
                    break;

                case MoveType.Capture:
                    RemovePiece(opponent, move.Capture, move.To);
                    RemovePiece(sideToMove, move.Piece, move.From);
                    AddPiece(sideToMove, move.Piece, move.To);
                    castling &= CastleMask(move.From, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveType.Castle:
                    CastlingRookMove rookMove = LookupRookMoves(move.To);
                    if (IsSquareAttackedByColor(move.From, opponent) ||
                        IsSquareAttackedByColor(rookMove.KingMoveThrough, opponent))
                    {
                        hash = oldHash;
                        return false;
                    }

                    RemovePiece(sideToMove, Piece.King, move.From);
                    AddPiece(sideToMove, Piece.King, move.To);
                    RemovePiece(sideToMove, Piece.Rook, rookMove.RookFrom);
                    AddPiece(sideToMove, Piece.Rook, rookMove.RookTo);
                    castling &= CastleMask(move.From, move.To);
                    halfMoveClock++;
                    break;

                case MoveType.EnPassant:
                    RemovePiece(opponent, Piece.Pawn, (SquareIndex)((int)move.To + EpOffset(sideToMove)));
                    RemovePiece(sideToMove, Piece.Pawn, move.From);
                    AddPiece(sideToMove, Piece.Pawn, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveType.PawnMove:
                    RemovePiece(sideToMove, Piece.Pawn, move.From);
                    AddPiece(sideToMove, Piece.Pawn, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveType.DblPawnMove:
                    RemovePiece(sideToMove, Piece.Pawn, move.From);
                    AddPiece(sideToMove, Piece.Pawn, move.To);
                    enPassant = (SquareIndex)((int)move.To + EpOffset(sideToMove));
                    if (IsEnPassantValid(opponent))
                    {
                        enPassantValidated = enPassant;
                        hash = ZobristHash.HashEnPassant(hash, enPassantValidated);
                    }
                    halfMoveClock = 0;
                    break;

                case MoveType.Promote:
                    RemovePiece(sideToMove, Piece.Pawn, move.From);
                    AddPiece(sideToMove, move.Promote, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveType.PromoteCapture:
                    RemovePiece(opponent, move.Capture, move.To);
                    RemovePiece(sideToMove, Piece.Pawn, move.From);
                    AddPiece(sideToMove, move.Promote, move.To);
                    castling &= CastleMask(move.From, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveType.Null:
                    // do nothing
                    break;

                default:
                    Util.Fail("Invalid move type.");
                    break;
            }

            if (IsChecked(opponent))
            {
                UnmakeMoveNs();
                return false;
            }

            gameStack.Peek().Move = move;
            fullMoveCounter += (ushort)sideToMove;
            hash = ZobristHash.HashCastling(hash, castling);
            hash = ZobristHash.HashActiveColor(hash, sideToMove);
            sideToMove = opponent;
            hash = ZobristHash.HashActiveColor(hash, sideToMove);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnmakeMove()
        {
            UnmakeMoveNs();
            gameStack.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnmakeMoveNs()
        {
            gameStack.Peek().Restore(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushBoardState()
        {
            BoardState state = new BoardState(this);
            gameStack.Push(ref state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopBoardState()
        {
            gameStack.Pop();
        }

        #endregion

        #region Attacks & Checks

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChecked()
        {
            return IsChecked(Opponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChecked(Color byColor)
        {
            SquareIndex kingSquare = KingIndex[byColor.Flip()];
            return IsSquareAttackedByColor(kingSquare, byColor);
        }

        public bool IsSquareAttackedByColor(SquareIndex sq, Color color)
        {
            return  ((PawnDefends(color, sq) & Pieces(color, Piece.Pawn)) != 0) ||
                    ((KnightMoves(sq) & Pieces(color, Piece.Knight)) != 0) ||
                    ((KingMoves(sq) & Pieces(color, Piece.King)) != 0) ||
                    ((GetBishopMoves(sq, All) & DiagonalSliders(color)) != 0) ||
                    ((GetRookMoves(sq, All) & OrthogonalSliders(color)) != 0);
        }

        public void GetEvasionInfo(SquareIndex kingIndex, out EvasionInfo info, out GenMoveHelper helper)
        {
            helper = helpers[sideToMove];
            info = GetEvasionInfo(kingIndex, helper);
        }

        private EvasionInfo GetEvasionInfo(SquareIndex kingIndex, GenMoveHelper helper)
        {
            Color opponent = Opponent;
            Piece checkerPiece = Piece.None;
            Bitboard kingMask = new Bitboard(kingIndex);
            Bitboard pawnAttacks = helper.EnemyPawnAttacks();
            Bitboard checkers = (Bitboard)0;
            Bitboard kingDanger = pawnAttacks;

            if ((pawnAttacks & kingMask) != 0)
            {
                checkers |= PawnDefends(opponent, kingIndex) & Pieces(opponent, Piece.Pawn);
                checkerPiece = Piece.Pawn;
            }

            foreach (SquareIndex from in Pieces(opponent, Piece.Knight))
            {
                Bitboard knightAttacks = KnightMoves(from);
                if ((knightAttacks & kingMask) != 0)
                {
                    checkers |= new Bitboard(from);
                    checkerPiece = Piece.Knight;
                }
                kingDanger |= knightAttacks;
            }

            kingDanger |= KingMoves(KingIndex[opponent]);

            // remove king from blockers so line attacks can continue through king square
            Bitboard blockers = All ^ kingMask;

            for (Piece piece = Piece.Bishop; piece <= Piece.Queen; piece++)
            {
                foreach (SquareIndex from in Pieces(opponent, piece))
                {
                    Bitboard lineAttacks = GetPieceMoves(piece, from, blockers);
                    kingDanger |= lineAttacks;
                    if ((lineAttacks & kingMask) != 0)
                    {
                        checkers |= new Bitboard(from);
                        checkerPiece = piece;
                    }
                }
            }

            Bitboard captureMask = bbAll;
            Bitboard pushMask = bbAll;
            int checkerCount = checkers.PopCount;
            if (checkerCount == 1)
            {
                captureMask = checkers;
                pushMask = bbNone;
                if (checkerPiece.IsSlider())
                {
                    pushMask = Between(kingIndex, (SquareIndex)checkers.TzCount);
                }
            }
            else if (checkerCount > 1)
            {
                captureMask = bbNone;
                pushMask = bbNone;
            }

            return new EvasionInfo(checkerCount, kingDanger, captureMask, pushMask);
        }

        public static Bitboard GetPieceMoves(Piece piece, SquareIndex from, Bitboard blockers)
        {
            return piece switch
            {
                Piece.Knight => KnightMoves(from),
                Piece.Bishop => GetBishopMoves(from, blockers),
                Piece.Rook => GetRookMoves(from, blockers),
                Piece.Queen => GetQueenMoves(from, blockers),
                Piece.King => KingMoves(from),
                _ => (Bitboard)0ul
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard GetQueenMoves(SquareIndex from, Bitboard blockers)
        {
            // The usage of IsPextSupported (a static readonly) will be treated as a constant
            // by the JIT. This will allow this function call to be transformed into a 
            // direct call to either GetXXXAttacksPext or GetXXXAttacksFancy with no
            // if statement.
            if (IsPextSupported)
            {
                return GetQueenAttacksPext(from, blockers);
            }
            return GetQueenAttacksFancy(from, blockers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard GetBishopMoves(SquareIndex from, Bitboard blockers)
        {
            if (IsPextSupported)
            {
                return GetBishopAttacksPext(from, blockers);
            }
            return GetBishopAttacksFancy(from, blockers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard GetRookMoves(SquareIndex from, Bitboard blockers)
        {
            if (IsPextSupported)
            {
                return GetRookAttacksPext(from, blockers);
            }
            return GetRookAttacksFancy(from, blockers);
        }

        // traditional diagonal slider move resolution
        public static Bitboard GetBishopAttacks(SquareIndex from, Bitboard blockers)
        {
            ref Ray ray = ref Vectors[(int)from];
            Bitboard bb = ray.NorthEast.AndNot(Vectors[(ray.NorthEast & blockers).TzCount].NorthEast)
                        | ray.NorthWest.AndNot(Vectors[(ray.NorthWest & blockers).TzCount].NorthWest)
                        | ray.SouthEast.AndNot(RevVectors[(ray.SouthEast & blockers).LzCount].SouthEast)
                        | ray.SouthWest.AndNot(RevVectors[(ray.SouthWest & blockers).LzCount].SouthWest)
                        ;

            return bb;
        }

        // traditional orthogonal slider move resolution
        public static Bitboard GetRookAttacks(SquareIndex from, Bitboard blockers)
        {
            ref Ray ray = ref Vectors[(int)from];
            Bitboard bb = ray.North.AndNot(Vectors[(ray.North & blockers).TzCount].North)
                        | ray.East.AndNot(Vectors[(ray.East & blockers).TzCount].East)
                        | ray.South.AndNot(RevVectors[(ray.South & blockers).LzCount].South)
                        | ray.West.AndNot(RevVectors[(ray.West & blockers).LzCount].West)
                        ;

            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard GetQueenAttacks(SquareIndex from, Bitboard blockers)
        {
            return GetBishopAttacks(from, blockers) | GetRookAttacks(from, blockers);
        }

        #endregion

        #region FEN String / ToString

        public bool LoadFen(ReadOnlySpan<char> fen)
        {
            if (!Notation.IsValidFen(fen))
            {
                return false;
            }

            Clear();

            Span<Range> ranges = stackalloc Range[6];
            if (fen.Split(ranges, ' ') == ranges.Length)
            {
                FenParsePieces(fen[ranges[0]]);
                sideToMove = Notation.ParseFenColor(fen[ranges[1]]);
                hash = ZobristHash.HashActiveColor(hash, sideToMove);
                castling = Notation.ParseFenCastlingRights(fen[ranges[2]]);
                hash = ZobristHash.HashCastling(hash, castling);
                enPassantValidated = SquareIndex.None;
                enPassant = Notation.ParseFenEnPassant(fen[ranges[3]]);
                if (IsEnPassantValid(sideToMove))
                {
                    enPassantValidated = enPassant;
                    hash = ZobristHash.HashEnPassant(hash, enPassantValidated);
                }
                if (!byte.TryParse(fen[ranges[4]], out halfMoveClock))
                {
                    halfMoveClock = 0;
                }
                //halfMoveClock = byte.Parse(fen[ranges[4]]);
                fullMoveCounter = ushort.Parse(fen[ranges[5]]);
                return true;
            }
            return false;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LoadFen(string fen)
        {
            return LoadFen(fen.AsSpan());
        }

        private string ToVerboseString()
        {
            ValueStringBuilder sb = new(stackalloc char[256]);
            sb.AppendLine("  A B C D E F G H");
            for (Rank rank = Rank.Rank8; rank >= Rank.Rank1; rank--)
            {
                sb.Append((int)rank + 1);
                sb.Append(' ');
                for (File file = File.FileA; file <= File.FileH; file++)
                {
                    SquareIndex sq = ChessMath.ToSquareIndex(file, rank);
                    sb.Append(board[(int)sq].IsEmpty ? "." : board[(int)sq].ToFenString());
                    sb.Append(' ');
                }

                sb.Append((int)rank + 1);
                sb.AppendLine();
            }
            sb.AppendLine("  A B C D E F G H");
            sb.AppendLine($"Turn: {sideToMove}");
            sb.AppendLine($"Castling: {castling}");
            sb.AppendLine($"EnPassant: {enPassant}");
            sb.AppendLine($"Half Move Clock: {halfMoveClock}");
            sb.AppendLine($"Turn: {fullMoveCounter}");
            sb.AppendLine($"Hash: 0x{hash,8:X}");
            return sb.ToString();
        }

        public string ToFenString()
        {
            StringBuilder sb = new();
            FenFormatPieces(sb);
            sb.Append(' ');
            sb.Append(sideToMove.ToFenString());
            sb.Append(' ');
            sb.Append(castling.ToFenString());
            sb.Append(' ');
            sb.Append(enPassant.ToFenString());
            sb.Append($" {halfMoveClock} {fullMoveCounter}");
            return sb.ToString();
        }

        public string? ToString(string format)
        {
            if (format == "V")
            {
                return ToVerboseString();
            }
            else if (format == "F")
            {
                return ToFenString();
            }
            else
            {
                return ToString();
            }
        }

        void FenFormatPieces(StringBuilder sb)
        {
            for (Rank rank = Rank.Rank8; rank >= Rank.Rank1; rank--)
            {
                int emptyCount = 0;
                for (File file = File.FileA; file <= File.FileH; file++)
                {
                    SquareIndex sq = ChessMath.ToSquareIndex(file, rank);
                    if (PieceBoard(sq).IsEmpty)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            sb.Append(emptyCount);
                            emptyCount = 0;
                        }

                        sb.Append(PieceBoard(sq).ToFenString());
                    }
                }

                if (emptyCount > 0)
                {
                    sb.Append(emptyCount);
                }

                if (rank > Rank.Rank1)
                {
                    sb.Append('/');
                }
            }
        }

        private void FenParsePieces(ReadOnlySpan<char> fenSection)
        {
            Rank rank = Rank.Rank8;
            File file = File.FileA;

            for (int n = 0; n < fenSection.Length; n++)
            {
                char ch = fenSection[n];
                switch (ch)
                {
                    case >= '1' and <= '8':
                        file = (File)((int)file + (ch - '0'));
                        break;

                    case '/':
                        file = File.FileA;
                        rank--;
                        break;

                    case 'p': case 'n': case 'b': case 'r': case 'q': case 'k':
                    case 'P': case 'N': case 'B': case 'R': case 'Q': case 'K':
                        var (color, piece) = Notation.ParseFenPiece(ch);
                        SquareIndex sq = ChessMath.ToSquareIndex(file++, rank);
                        AddPiece(color, piece, sq);
                        break;

                    default:
                        throw new ArgumentException($"Illegal piece encountered in FEN record '{ch}'.");
                }
            }
        }

        #endregion

        #region Positional Status

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnPassantValid(Color color)
        {
            return (enPassant != SquareIndex.None) && (Units(color) & Pawns & PawnDefends(color, enPassant)) != 0;
        }

        public unsafe (bool Repeated, bool OverFiftyMoves) PositionRepeated()
        {
            if (halfMoveClock < 4 || gameStack.Count <= 1)
            {
                return (false, false);
            }

            if (halfMoveClock > 99)
            {
                return (false, true);
            }

            var stackSpan = gameStack.AsSpan();
            int max = Math.Max(stackSpan.Length - halfMoveClock, 0);
            int start = stackSpan.Length - 2;

            fixed (BoardState* pStart = &stackSpan[start], pEnd = &stackSpan[max])
            {
                for (BoardState* p = pStart; p >= pEnd; p -= 2)
                {
                    if (hash == p->Hash)
                    {
                        return (true, false);
                    }
                }
            }

            return (false, false);
        }

        #endregion

        #region Miscellaneous / Interfaces

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Board Clone()
        {
            return new Board(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Static Lookup Tables

        public static void Initialize() {}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CastlingRights CastleMask(SquareIndex from, SquareIndex to)
        {
            return (CastlingRights)(castleMask[(int)from] & castleMask[(int)to]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CastlingRookMove LookupRookMoves(SquareIndex kingTo)
        {
            return kingTo switch
            {
                SquareIndex.C1 => wqRookMove,
                SquareIndex.G1 => wkRookMove,
                SquareIndex.C8 => bqRookMove,
                SquareIndex.G8 => bkRookMove,
                _ => throw new ArgumentException("Invalid king target/to square invalid.", nameof(kingTo)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EpOffset(Color color)
        {
            return 8 * (-1 + ((int)color * 2));
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard PawnDefends(Color color, SquareIndex square)
        {
            return (Bitboard)pawnDefends[(int)color, (int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard PawnCaptures(Color color, SquareIndex square)
        {
            return (Bitboard)pawnCaptures[(int)color, (int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard KnightMoves(SquareIndex square)
        {
            return (Bitboard)knightMoves[(int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard KingMoves(SquareIndex square)
        {
            return (Bitboard)kingMoves[(int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SquareIndex PawnLeft(Color color, SquareIndex square)
        {
            return (SquareIndex)pawnLeft[(int)color, (int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SquareIndex PawnRight(Color color, SquareIndex square)
        {
            return (SquareIndex)pawnRight[(int)color, (int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SquareIndex PawnPlus(Color color, SquareIndex square)
        {
            return (SquareIndex)pawnPlus[(int)color, (int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SquareIndex PawnDouble(Color color, SquareIndex square)
        {
            return (SquareIndex)pawnDouble[(int)color, (int)square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitboard Between(SquareIndex from, SquareIndex to)
        {
            return (Bitboard)between[(int)from, (int)to];
        }

        private readonly static Bitboard whiteKingSideClearMask = new (SquareIndex.F1, SquareIndex.G1);
        private readonly static Bitboard whiteQueenSideClearMask = new (SquareIndex.B1, SquareIndex.C1, SquareIndex.D1);
        private readonly static Bitboard blackKingSideClearMask = new (SquareIndex.F8, SquareIndex.G8);
        private readonly static Bitboard blackQueenSideClearMask = new (SquareIndex.B8, SquareIndex.C8, SquareIndex.D8);
        private readonly static Bitboard bbFileA = new Bitboard(File.FileA);
        private readonly static Bitboard bbFileH = new Bitboard(File.FileH);
        private readonly static Bitboard bbRank2 = new Bitboard(Rank.Rank2);
        private readonly static Bitboard bbRank7 = new Bitboard(Rank.Rank7);
        private readonly static Bitboard bbAll = new Bitboard(BB_ALL);
        private readonly static Bitboard bbNone = new Bitboard(BB_NONE);

        private static readonly FixedArray<byte> castleMask = new (MAX_SQUARES)
        {
            #region castleMask data

            13, 15, 15, 15, 12, 15, 15, 14,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
             7, 15, 15, 15,  3, 15, 15, 11

            #endregion
        };

        private static readonly CastlingRookMove wqRookMove = 
            new(SquareIndex.E1, SquareIndex.C1, SquareIndex.D1, SquareIndex.A1, SquareIndex.D1,
                CastlingRights.WhiteQueenSide, new Bitboard(SquareIndex.B1, SquareIndex.C1, SquareIndex.D1));

        private static readonly CastlingRookMove wkRookMove = 
            new(SquareIndex.E1, SquareIndex.G1, SquareIndex.F1, SquareIndex.H1, SquareIndex.F1,
                CastlingRights.WhiteKingSide, new Bitboard(SquareIndex.F1, SquareIndex.G1));

        private static readonly CastlingRookMove bqRookMove = 
            new(SquareIndex.E8, SquareIndex.C8, SquareIndex.D8, SquareIndex.A8, SquareIndex.D8,
                CastlingRights.BlackQueenSide, new Bitboard(SquareIndex.B8, SquareIndex.C8, SquareIndex.D8));

        private static readonly CastlingRookMove bkRookMove = 
            new(SquareIndex.E8, SquareIndex.G8, SquareIndex.F8, SquareIndex.H8, SquareIndex.F8,
                CastlingRights.BlackKingSide, new Bitboard(SquareIndex.F8, SquareIndex.G8));

        private static readonly FixedArray2D<ulong> between = new (MAX_SQUARES, MAX_SQUARES)
        {
            #region between data

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000002ul, 0x0000000000000006ul,
            0x000000000000000Eul, 0x000000000000001Eul, 0x000000000000003Eul, 0x000000000000007Eul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000100ul, 0x0000000000000000ul, 0x0000000000000200ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000040200ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000001010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000008040200ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000101010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000001008040200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010101010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000201008040200ul, 0x0000000000000000ul,
            0x0001010101010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0040201008040200ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000004ul,
            0x000000000000000Cul, 0x000000000000001Cul, 0x000000000000003Cul, 0x000000000000007Cul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000200ul, 0x0000000000000000ul, 0x0000000000000400ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000080400ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000002020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000010080400ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000202020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000002010080400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020202020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000402010080400ul,
            0x0000000000000000ul, 0x0002020202020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000002ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000008ul, 0x0000000000000018ul, 0x0000000000000038ul, 0x0000000000000078ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000200ul, 0x0000000000000000ul, 0x0000000000000400ul, 0x0000000000000000ul,
            0x0000000000000800ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000100800ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000004040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000020100800ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000404040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000004020100800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040404040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040404040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000006ul, 0x0000000000000004ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000010ul, 0x0000000000000030ul, 0x0000000000000070ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000400ul, 0x0000000000000000ul, 0x0000000000000800ul,
            0x0000000000000000ul, 0x0000000000001000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000020400ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000201000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040201000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000808080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080808080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080808080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x000000000000000Eul, 0x000000000000000Cul, 0x0000000000000008ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000020ul, 0x0000000000000060ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000800ul, 0x0000000000000000ul,
            0x0000000000001000ul, 0x0000000000000000ul, 0x0000000000002000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000040800ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000402000ul,
            0x0000000002040800ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000001010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x000000000000001Eul, 0x000000000000001Cul, 0x0000000000000018ul, 0x0000000000000010ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000040ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000001000ul,
            0x0000000000000000ul, 0x0000000000002000ul, 0x0000000000000000ul, 0x0000000000004000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000081000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000004081000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000204081000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000002020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x000000000000003Eul, 0x000000000000003Cul, 0x0000000000000038ul, 0x0000000000000030ul,
            0x0000000000000020ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000002000ul, 0x0000000000000000ul, 0x0000000000004000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000102000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000404000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008102000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040404000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000408102000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000004040404000ul, 0x0000000000000000ul,
            0x0000020408102000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404040404000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404040404000ul, 0x0000000000000000ul,

            0x000000000000007Eul, 0x000000000000007Cul, 0x0000000000000078ul, 0x0000000000000070ul,
            0x0000000000000060ul, 0x0000000000000040ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000004000ul, 0x0000000000000000ul, 0x0000000000008000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000204000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000808000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000010204000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000080808000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000810204000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000008080808000ul,
            0x0000000000000000ul, 0x0000040810204000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808080808000ul,
            0x0002040810204000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808080808000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000200ul, 0x0000000000000600ul,
            0x0000000000000E00ul, 0x0000000000001E00ul, 0x0000000000003E00ul, 0x0000000000007E00ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000010000ul, 0x0000000000000000ul, 0x0000000000020000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000001010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000004020000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000101010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000804020000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010101010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000100804020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001010101010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0020100804020000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000400ul,
            0x0000000000000C00ul, 0x0000000000001C00ul, 0x0000000000003C00ul, 0x0000000000007C00ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000020000ul, 0x0000000000000000ul, 0x0000000000040000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000002020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000008040000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000202020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000001008040000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020202020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000201008040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002020202020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0040201008040000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000200ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000800ul, 0x0000000000001800ul, 0x0000000000003800ul, 0x0000000000007800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000020000ul, 0x0000000000000000ul, 0x0000000000040000ul, 0x0000000000000000ul,
            0x0000000000080000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000004040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000010080000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000404040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000002010080000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040404040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000402010080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040404040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000600ul, 0x0000000000000400ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000001000ul, 0x0000000000003000ul, 0x0000000000007000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000040000ul, 0x0000000000000000ul, 0x0000000000080000ul,
            0x0000000000000000ul, 0x0000000000100000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000002040000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000020100000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000808080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000004020100000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080808080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080808080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000E00ul, 0x0000000000000C00ul, 0x0000000000000800ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000002000ul, 0x0000000000006000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000080000ul, 0x0000000000000000ul,
            0x0000000000100000ul, 0x0000000000000000ul, 0x0000000000200000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000004080000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040200000ul,
            0x0000000204080000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000001010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000001E00ul, 0x0000000000001C00ul, 0x0000000000001800ul, 0x0000000000001000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000004000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000100000ul,
            0x0000000000000000ul, 0x0000000000200000ul, 0x0000000000000000ul, 0x0000000000400000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008100000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000408100000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000002020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000020408100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000003E00ul, 0x0000000000003C00ul, 0x0000000000003800ul, 0x0000000000003000ul,
            0x0000000000002000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000200000ul, 0x0000000000000000ul, 0x0000000000400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000010200000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000810200000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000004040400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000040810200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404040400000ul, 0x0000000000000000ul,
            0x0002040810200000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404040400000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000007E00ul, 0x0000000000007C00ul, 0x0000000000007800ul, 0x0000000000007000ul,
            0x0000000000006000ul, 0x0000000000004000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000400000ul, 0x0000000000000000ul, 0x0000000000800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000020400000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000080800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000001020400000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000008080800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000081020400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808080800000ul,
            0x0000000000000000ul, 0x0004081020400000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808080800000ul,

            0x0000000000000100ul, 0x0000000000000000ul, 0x0000000000000200ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000020000ul, 0x0000000000060000ul,
            0x00000000000E0000ul, 0x00000000001E0000ul, 0x00000000003E0000ul, 0x00000000007E0000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000001000000ul, 0x0000000000000000ul, 0x0000000002000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000101000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000402000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010101000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000080402000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001010101000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0010080402000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000200ul, 0x0000000000000000ul, 0x0000000000000400ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000040000ul,
            0x00000000000C0000ul, 0x00000000001C0000ul, 0x00000000003C0000ul, 0x00000000007C0000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000002000000ul, 0x0000000000000000ul, 0x0000000004000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000202000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000804000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020202000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000100804000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002020202000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0020100804000000ul, 0x0000000000000000ul,

            0x0000000000000200ul, 0x0000000000000000ul, 0x0000000000000400ul, 0x0000000000000000ul,
            0x0000000000000800ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000020000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000080000ul, 0x0000000000180000ul, 0x0000000000380000ul, 0x0000000000780000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000002000000ul, 0x0000000000000000ul, 0x0000000004000000ul, 0x0000000000000000ul,
            0x0000000008000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000404000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000001008000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040404000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000201008000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040404000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0040201008000000ul,

            0x0000000000000000ul, 0x0000000000000400ul, 0x0000000000000000ul, 0x0000000000000800ul,
            0x0000000000000000ul, 0x0000000000001000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000060000ul, 0x0000000000040000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000100000ul, 0x0000000000300000ul, 0x0000000000700000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000004000000ul, 0x0000000000000000ul, 0x0000000008000000ul,
            0x0000000000000000ul, 0x0000000010000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000204000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000808000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000002010000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080808000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000402010000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080808000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000800ul, 0x0000000000000000ul,
            0x0000000000001000ul, 0x0000000000000000ul, 0x0000000000002000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00000000000E0000ul, 0x00000000000C0000ul, 0x0000000000080000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000200000ul, 0x0000000000600000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008000000ul, 0x0000000000000000ul,
            0x0000000010000000ul, 0x0000000000000000ul, 0x0000000020000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000408000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000001010000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000004020000000ul,
            0x0000020408000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101010000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101010000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000001000ul,
            0x0000000000000000ul, 0x0000000000002000ul, 0x0000000000000000ul, 0x0000000000004000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00000000001E0000ul, 0x00000000001C0000ul, 0x0000000000180000ul, 0x0000000000100000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000400000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000010000000ul,
            0x0000000000000000ul, 0x0000000020000000ul, 0x0000000000000000ul, 0x0000000040000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000810000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000002020000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000040810000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202020000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0002040810000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202020000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000002000ul, 0x0000000000000000ul, 0x0000000000004000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00000000003E0000ul, 0x00000000003C0000ul, 0x0000000000380000ul, 0x0000000000300000ul,
            0x0000000000200000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000020000000ul, 0x0000000000000000ul, 0x0000000040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000001020000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000004040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000081020000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0004081020000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404040000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000004000ul, 0x0000000000000000ul, 0x0000000000008000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00000000007E0000ul, 0x00000000007C0000ul, 0x0000000000780000ul, 0x0000000000700000ul,
            0x0000000000600000ul, 0x0000000000400000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000040000000ul, 0x0000000000000000ul, 0x0000000080000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000002040000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000008080000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000102040000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808080000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0008102040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808080000000ul,

            0x0000000000010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000020400ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000010000ul, 0x0000000000000000ul, 0x0000000000020000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000002000000ul, 0x0000000006000000ul,
            0x000000000E000000ul, 0x000000001E000000ul, 0x000000003E000000ul, 0x000000007E000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000100000000ul, 0x0000000000000000ul, 0x0000000200000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010100000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000040200000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001010100000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0008040200000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000040800ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000020000ul, 0x0000000000000000ul, 0x0000000000040000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000004000000ul,
            0x000000000C000000ul, 0x000000001C000000ul, 0x000000003C000000ul, 0x000000007C000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000200000000ul, 0x0000000000000000ul, 0x0000000400000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020200000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000080400000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002020200000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0010080400000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000081000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000020000ul, 0x0000000000000000ul, 0x0000000000040000ul, 0x0000000000000000ul,
            0x0000000000080000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000002000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000008000000ul, 0x0000000018000000ul, 0x0000000038000000ul, 0x0000000078000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000200000000ul, 0x0000000000000000ul, 0x0000000400000000ul, 0x0000000000000000ul,
            0x0000000800000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040400000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000100800000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040400000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0020100800000000ul, 0x0000000000000000ul,

            0x0000000000040200ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000102000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000040000ul, 0x0000000000000000ul, 0x0000000000080000ul,
            0x0000000000000000ul, 0x0000000000100000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000006000000ul, 0x0000000004000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000010000000ul, 0x0000000030000000ul, 0x0000000070000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000400000000ul, 0x0000000000000000ul, 0x0000000800000000ul,
            0x0000000000000000ul, 0x0000001000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000020400000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080800000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000201000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080800000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0040201000000000ul,

            0x0000000000000000ul, 0x0000000000080400ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000204000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000080000ul, 0x0000000000000000ul,
            0x0000000000100000ul, 0x0000000000000000ul, 0x0000000000200000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x000000000E000000ul, 0x000000000C000000ul, 0x0000000008000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000020000000ul, 0x0000000060000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000800000000ul, 0x0000000000000000ul,
            0x0000001000000000ul, 0x0000000000000000ul, 0x0000002000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000040800000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000402000000000ul,
            0x0002040800000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000100800ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000100000ul,
            0x0000000000000000ul, 0x0000000000200000ul, 0x0000000000000000ul, 0x0000000000400000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x000000001E000000ul, 0x000000001C000000ul, 0x0000000018000000ul, 0x0000000010000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000001000000000ul,
            0x0000000000000000ul, 0x0000002000000000ul, 0x0000000000000000ul, 0x0000004000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000081000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0004081000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000201000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000404000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000200000ul, 0x0000000000000000ul, 0x0000000000400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x000000003E000000ul, 0x000000003C000000ul, 0x0000000038000000ul, 0x0000000030000000ul,
            0x0000000020000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000002000000000ul, 0x0000000000000000ul, 0x0000004000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000102000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0008102000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000402000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000808000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000400000ul, 0x0000000000000000ul, 0x0000000000800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x000000007E000000ul, 0x000000007C000000ul, 0x0000000078000000ul, 0x0000000070000000ul,
            0x0000000060000000ul, 0x0000000040000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000004000000000ul, 0x0000000000000000ul, 0x0000008000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000204000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0010204000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808000000000ul,

            0x0000000001010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000002040800ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000001010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000002040000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000001000000ul, 0x0000000000000000ul, 0x0000000002000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000200000000ul, 0x0000000600000000ul,
            0x0000000E00000000ul, 0x0000001E00000000ul, 0x0000003E00000000ul, 0x0000007E00000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010000000000ul, 0x0000000000000000ul, 0x0000020000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001010000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0004020000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000002020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000004081000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000002020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000004080000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000002000000ul, 0x0000000000000000ul, 0x0000000004000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000400000000ul,
            0x0000000C00000000ul, 0x0000001C00000000ul, 0x0000003C00000000ul, 0x0000007C00000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020000000000ul, 0x0000000000000000ul, 0x0000040000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002020000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0008040000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000004040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008102000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000004040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000008100000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000002000000ul, 0x0000000000000000ul, 0x0000000004000000ul, 0x0000000000000000ul,
            0x0000000008000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000200000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000800000000ul, 0x0000001800000000ul, 0x0000003800000000ul, 0x0000007800000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000020000000000ul, 0x0000000000000000ul, 0x0000040000000000ul, 0x0000000000000000ul,
            0x0000080000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0010080000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000010204000ul,
            0x0000000004020000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000010200000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000004000000ul, 0x0000000000000000ul, 0x0000000008000000ul,
            0x0000000000000000ul, 0x0000000010000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000600000000ul, 0x0000000400000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000001000000000ul, 0x0000003000000000ul, 0x0000007000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000040000000000ul, 0x0000000000000000ul, 0x0000080000000000ul,
            0x0000000000000000ul, 0x0000100000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0002040000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0020100000000000ul, 0x0000000000000000ul,

            0x0000000008040200ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000008040000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000020400000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000008000000ul, 0x0000000000000000ul,
            0x0000000010000000ul, 0x0000000000000000ul, 0x0000000020000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000E00000000ul, 0x0000000C00000000ul, 0x0000000800000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000002000000000ul, 0x0000006000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000080000000000ul, 0x0000000000000000ul,
            0x0000100000000000ul, 0x0000000000000000ul, 0x0000200000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0004080000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010100000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0040200000000000ul,

            0x0000000000000000ul, 0x0000000010080400ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000010080000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000010000000ul,
            0x0000000000000000ul, 0x0000000020000000ul, 0x0000000000000000ul, 0x0000000040000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000001E00000000ul, 0x0000001C00000000ul, 0x0000001800000000ul, 0x0000001000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000004000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000100000000000ul,
            0x0000000000000000ul, 0x0000200000000000ul, 0x0000000000000000ul, 0x0000400000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0008100000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020200000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000020100800ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040404000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000020100000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000020000000ul, 0x0000000000000000ul, 0x0000000040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000003E00000000ul, 0x0000003C00000000ul, 0x0000003800000000ul, 0x0000003000000000ul,
            0x0000002000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000200000000000ul, 0x0000000000000000ul, 0x0000400000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0010200000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040400000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000040201000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000080808000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000040200000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000080800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000040000000ul, 0x0000000000000000ul, 0x0000000080000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000007E00000000ul, 0x0000007C00000000ul, 0x0000007800000000ul, 0x0000007000000000ul,
            0x0000006000000000ul, 0x0000004000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000400000000000ul, 0x0000000000000000ul, 0x0000800000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0020400000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080800000000000ul,

            0x0000000101010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000204081000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000101010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000204080000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000101000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000204000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000100000000ul, 0x0000000000000000ul, 0x0000000200000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000020000000000ul, 0x0000060000000000ul,
            0x00000E0000000000ul, 0x00001E0000000000ul, 0x00003E0000000000ul, 0x00007E0000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001000000000000ul, 0x0000000000000000ul, 0x0002000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000202020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000408102000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000202020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000408100000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000202000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000408000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000200000000ul, 0x0000000000000000ul, 0x0000000400000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000040000000000ul,
            0x00000C0000000000ul, 0x00001C0000000000ul, 0x00003C0000000000ul, 0x00007C0000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002000000000000ul, 0x0000000000000000ul, 0x0004000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000404040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000810204000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000404040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000810200000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000404000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000810000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000200000000ul, 0x0000000000000000ul, 0x0000000400000000ul, 0x0000000000000000ul,
            0x0000000800000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000020000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000080000000000ul, 0x0000180000000000ul, 0x0000380000000000ul, 0x0000780000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0002000000000000ul, 0x0000000000000000ul, 0x0004000000000000ul, 0x0000000000000000ul,
            0x0008000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000808080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000808080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000001020400000ul,
            0x0000000402000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000808000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000001020000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000400000000ul, 0x0000000000000000ul, 0x0000000800000000ul,
            0x0000000000000000ul, 0x0000001000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000060000000000ul, 0x0000040000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000100000000000ul, 0x0000300000000000ul, 0x0000700000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0004000000000000ul, 0x0000000000000000ul, 0x0008000000000000ul,
            0x0000000000000000ul, 0x0010000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000001010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000804020000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000001010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000804000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000001010000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000002040000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000800000000ul, 0x0000000000000000ul,
            0x0000001000000000ul, 0x0000000000000000ul, 0x0000002000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00000E0000000000ul, 0x00000C0000000000ul, 0x0000080000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000200000000000ul, 0x0000600000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0008000000000000ul, 0x0000000000000000ul,
            0x0010000000000000ul, 0x0000000000000000ul, 0x0020000000000000ul, 0x0000000000000000ul,

            0x0000001008040200ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000002020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000001008040000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000002020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000001008000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000002020000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000001000000000ul,
            0x0000000000000000ul, 0x0000002000000000ul, 0x0000000000000000ul, 0x0000004000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00001E0000000000ul, 0x00001C0000000000ul, 0x0000180000000000ul, 0x0000100000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000400000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0010000000000000ul,
            0x0000000000000000ul, 0x0020000000000000ul, 0x0000000000000000ul, 0x0040000000000000ul,

            0x0000000000000000ul, 0x0000002010080400ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000004040404000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000002010080000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000004040400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000002010000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000004040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000002000000000ul, 0x0000000000000000ul, 0x0000004000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00003E0000000000ul, 0x00003C0000000000ul, 0x0000380000000000ul, 0x0000300000000000ul,
            0x0000200000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0020000000000000ul, 0x0000000000000000ul, 0x0040000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000004020100800ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000008080808000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000004020100000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000008080800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000004020000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000008080000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000004000000000ul, 0x0000000000000000ul, 0x0000008000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x00007E0000000000ul, 0x00007C0000000000ul, 0x0000780000000000ul, 0x0000700000000000ul,
            0x0000600000000000ul, 0x0000400000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0040000000000000ul, 0x0000000000000000ul, 0x0080000000000000ul,

            0x0000010101010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000020408102000ul, 0x0000000000000000ul,
            0x0000010101010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020408100000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010101000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000020408000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010100000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000020400000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000010000000000ul, 0x0000000000000000ul, 0x0000020000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0002000000000000ul, 0x0006000000000000ul,
            0x000E000000000000ul, 0x001E000000000000ul, 0x003E000000000000ul, 0x007E000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000020202020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000040810204000ul,
            0x0000000000000000ul, 0x0000020202020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040810200000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020202000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000040810000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020200000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000040800000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000020000000000ul, 0x0000000000000000ul, 0x0000040000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0004000000000000ul,
            0x000C000000000000ul, 0x001C000000000000ul, 0x003C000000000000ul, 0x007C000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040404040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040404040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000081020400000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040404000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000081020000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000040400000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000081000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000020000000000ul, 0x0000000000000000ul, 0x0000040000000000ul, 0x0000000000000000ul,
            0x0000080000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0002000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0008000000000000ul, 0x0018000000000000ul, 0x0038000000000000ul, 0x0078000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080808080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080808080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080808000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000102040000000ul,
            0x0000040200000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000080800000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000102000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000040000000000ul, 0x0000000000000000ul, 0x0000080000000000ul,
            0x0000000000000000ul, 0x0000100000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0006000000000000ul, 0x0004000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0010000000000000ul, 0x0030000000000000ul, 0x0070000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000080402000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101010000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000080400000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000101000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000204000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000080000000000ul, 0x0000000000000000ul,
            0x0000100000000000ul, 0x0000000000000000ul, 0x0000200000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x000E000000000000ul, 0x000C000000000000ul, 0x0008000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0020000000000000ul, 0x0060000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000100804020000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000100804000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202020000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000100800000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000202000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000100000000000ul,
            0x0000000000000000ul, 0x0000200000000000ul, 0x0000000000000000ul, 0x0000400000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x001E000000000000ul, 0x001C000000000000ul, 0x0018000000000000ul, 0x0010000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0040000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000201008040200ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404040404000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000201008040000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404040400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000201008000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000201000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000404000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000200000000000ul, 0x0000000000000000ul, 0x0000400000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x003E000000000000ul, 0x003C000000000000ul, 0x0038000000000000ul, 0x0030000000000000ul,
            0x0020000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0000000000000000ul, 0x0000402010080400ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808080808000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000402010080000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808080800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000402010000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808080000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000402000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000808000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000400000000000ul, 0x0000000000000000ul, 0x0000800000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x007E000000000000ul, 0x007C000000000000ul, 0x0078000000000000ul, 0x0070000000000000ul,
            0x0060000000000000ul, 0x0040000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0001010101010100ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0002040810204000ul,
            0x0001010101010000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0002040810200000ul, 0x0000000000000000ul,
            0x0001010101000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002040810000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001010100000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0002040800000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001010000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0002040000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0001000000000000ul, 0x0000000000000000ul, 0x0002000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0200000000000000ul, 0x0600000000000000ul,
            0x0E00000000000000ul, 0x1E00000000000000ul, 0x3E00000000000000ul, 0x7E00000000000000ul,

            0x0000000000000000ul, 0x0002020202020200ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002020202020000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0004081020400000ul,
            0x0000000000000000ul, 0x0002020202000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004081020000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002020200000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0004081000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002020000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0004080000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0002000000000000ul, 0x0000000000000000ul, 0x0004000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0400000000000000ul,
            0x0C00000000000000ul, 0x1C00000000000000ul, 0x3C00000000000000ul, 0x7C00000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040404040400ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040404040000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040404000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008102040000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040400000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0008102000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0004040000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0008100000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0002000000000000ul, 0x0000000000000000ul, 0x0004000000000000ul, 0x0000000000000000ul,
            0x0008000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0200000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0800000000000000ul, 0x1800000000000000ul, 0x3800000000000000ul, 0x7800000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080808080800ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080808080000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080808000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080800000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0010204000000000ul,
            0x0004020000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0008080000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0010200000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0004000000000000ul, 0x0000000000000000ul, 0x0008000000000000ul,
            0x0000000000000000ul, 0x0010000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0600000000000000ul, 0x0400000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x1000000000000000ul, 0x3000000000000000ul, 0x7000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101010101000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101010100000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101010000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0008040200000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010101000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0008040000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010100000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0020400000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0008000000000000ul, 0x0000000000000000ul,
            0x0010000000000000ul, 0x0000000000000000ul, 0x0020000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0E00000000000000ul, 0x0C00000000000000ul, 0x0800000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x2000000000000000ul, 0x6000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202020202000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202020200000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0010080402000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202020000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0010080400000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020202000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0010080000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020200000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0010000000000000ul,
            0x0000000000000000ul, 0x0020000000000000ul, 0x0000000000000000ul, 0x0040000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x1E00000000000000ul, 0x1C00000000000000ul, 0x1800000000000000ul, 0x1000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x4000000000000000ul,

            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404040404000ul, 0x0000000000000000ul,
            0x0020100804020000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404040400000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0020100804000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404040000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0020100800000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040404000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0020100000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040400000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0020000000000000ul, 0x0000000000000000ul, 0x0040000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x3E00000000000000ul, 0x3C00000000000000ul, 0x3800000000000000ul, 0x3000000000000000ul,
            0x2000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            0x0040201008040200ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808080808000ul,
            0x0000000000000000ul, 0x0040201008040000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808080800000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0040201008000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808080000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0040201000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080808000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0040200000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0080800000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0040000000000000ul, 0x0000000000000000ul, 0x0080000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x7E00000000000000ul, 0x7C00000000000000ul, 0x7800000000000000ul, 0x7000000000000000ul,
            0x6000000000000000ul, 0x4000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul

            #endregion
        };

        private static readonly FixedArray2D<ulong> pawnDefends = new (MAX_COLORS, MAX_SQUARES)
        {
            #region pawnDefends data

            // squares defended by white pawns
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000002ul, 0x0000000000000005ul, 0x000000000000000Aul, 0x0000000000000014ul,
            0x0000000000000028ul, 0x0000000000000050ul, 0x00000000000000A0ul, 0x0000000000000040ul,
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

            // squares defended by black pawns
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
            0x0200000000000000ul, 0x0500000000000000ul, 0x0A00000000000000ul, 0x1400000000000000ul,
            0x2800000000000000ul, 0x5000000000000000ul, 0xA000000000000000ul, 0x4000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul
            #endregion
        };

        private static readonly FixedArray2D<ulong> pawnCaptures = new (MAX_COLORS, MAX_SQUARES)
        {
            #region pawnCaptures data

            // squares attacked by white pawns
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
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
            0x0200000000000000ul, 0x0500000000000000ul, 0x0A00000000000000ul, 0x1400000000000000ul,
            0x2800000000000000ul, 0x5000000000000000ul, 0xA000000000000000ul, 0x4000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,

            // squares attacked by black pawns
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000002ul, 0x0000000000000005ul, 0x000000000000000Aul, 0x0000000000000014ul,
            0x0000000000000028ul, 0x0000000000000050ul, 0x00000000000000A0ul, 0x0000000000000040ul,
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
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
            0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul

            #endregion pawnCaptures data
        };

        private static readonly FixedArray<ulong> knightMoves = new(MAX_SQUARES)
        {
            #region knightMoves data

            0x0000000000020400ul, 0x0000000000050800ul, 0x00000000000A1100ul, 0x0000000000142200ul,
            0x0000000000284400ul, 0x0000000000508800ul, 0x0000000000A01000ul, 0x0000000000402000ul,
            0x0000000002040004ul, 0x0000000005080008ul, 0x000000000A110011ul, 0x0000000014220022ul,
            0x0000000028440044ul, 0x0000000050880088ul, 0x00000000A0100010ul, 0x0000000040200020ul,
            0x0000000204000402ul, 0x0000000508000805ul, 0x0000000A1100110Aul, 0x0000001422002214ul,
            0x0000002844004428ul, 0x0000005088008850ul, 0x000000A0100010A0ul, 0x0000004020002040ul,
            0x0000020400040200ul, 0x0000050800080500ul, 0x00000A1100110A00ul, 0x0000142200221400ul,
            0x0000284400442800ul, 0x0000508800885000ul, 0x0000A0100010A000ul, 0x0000402000204000ul,
            0x0002040004020000ul, 0x0005080008050000ul, 0x000A1100110A0000ul, 0x0014220022140000ul,
            0x0028440044280000ul, 0x0050880088500000ul, 0x00A0100010A00000ul, 0x0040200020400000ul,
            0x0204000402000000ul, 0x0508000805000000ul, 0x0A1100110A000000ul, 0x1422002214000000ul,
            0x2844004428000000ul, 0x5088008850000000ul, 0xA0100010A0000000ul, 0x4020002040000000ul,
            0x0400040200000000ul, 0x0800080500000000ul, 0x1100110A00000000ul, 0x2200221400000000ul,
            0x4400442800000000ul, 0x8800885000000000ul, 0x100010A000000000ul, 0x2000204000000000ul,
            0x0004020000000000ul, 0x0008050000000000ul, 0x00110A0000000000ul, 0x0022140000000000ul,
            0x0044280000000000ul, 0x0088500000000000ul, 0x0010A00000000000ul, 0x0020400000000000ul

            #endregion
        };

        private static readonly FixedArray<ulong> kingMoves = new(MAX_SQUARES)
        {
            #region kingMoves data

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
            0x2838000000000000ul, 0x5070000000000000ul, 0xA0E0000000000000ul, 0x40C0000000000000ul

            #endregion
        };

        public static readonly FixedArray<Ray> Vectors = new(MAX_SQUARES + 1)
        {
            #region vectors data
            new Ray(0x0101010101010100ul, 0x8040201008040200ul, 0x00000000000000FEul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0202020202020200ul, 0x0080402010080400ul, 0x00000000000000FCul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000001ul, 0x0000000000000100ul),
            new Ray(0x0404040404040400ul, 0x0000804020100800ul, 0x00000000000000F8ul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000003ul, 0x0000000000010200ul),
            new Ray(0x0808080808080800ul, 0x0000008040201000ul, 0x00000000000000F0ul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000007ul, 0x0000000001020400ul),
            new Ray(0x1010101010101000ul, 0x0000000080402000ul, 0x00000000000000E0ul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x000000000000000Ful, 0x0000000102040800ul),
            new Ray(0x2020202020202000ul, 0x0000000000804000ul, 0x00000000000000C0ul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x000000000000001Ful, 0x0000010204081000ul),
            new Ray(0x4040404040404000ul, 0x0000000000008000ul, 0x0000000000000080ul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x000000000000003Ful, 0x0001020408102000ul),
            new Ray(0x8080808080808000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x000000000000007Ful, 0x0102040810204000ul),
            new Ray(0x0101010101010000ul, 0x4020100804020000ul, 0x000000000000FE00ul, 0x0000000000000002ul,
                    0x0000000000000001ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0202020202020000ul, 0x8040201008040000ul, 0x000000000000FC00ul, 0x0000000000000004ul,
                    0x0000000000000002ul, 0x0000000000000001ul, 0x0000000000000100ul, 0x0000000000010000ul),
            new Ray(0x0404040404040000ul, 0x0080402010080000ul, 0x000000000000F800ul, 0x0000000000000008ul,
                    0x0000000000000004ul, 0x0000000000000002ul, 0x0000000000000300ul, 0x0000000001020000ul),
            new Ray(0x0808080808080000ul, 0x0000804020100000ul, 0x000000000000F000ul, 0x0000000000000010ul,
                    0x0000000000000008ul, 0x0000000000000004ul, 0x0000000000000700ul, 0x0000000102040000ul),
            new Ray(0x1010101010100000ul, 0x0000008040200000ul, 0x000000000000E000ul, 0x0000000000000020ul,
                    0x0000000000000010ul, 0x0000000000000008ul, 0x0000000000000F00ul, 0x0000010204080000ul),
            new Ray(0x2020202020200000ul, 0x0000000080400000ul, 0x000000000000C000ul, 0x0000000000000040ul,
                    0x0000000000000020ul, 0x0000000000000010ul, 0x0000000000001F00ul, 0x0001020408100000ul),
            new Ray(0x4040404040400000ul, 0x0000000000800000ul, 0x0000000000008000ul, 0x0000000000000080ul,
                    0x0000000000000040ul, 0x0000000000000020ul, 0x0000000000003F00ul, 0x0102040810200000ul),
            new Ray(0x8080808080800000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000000000000080ul, 0x0000000000000040ul, 0x0000000000007F00ul, 0x0204081020400000ul),
            new Ray(0x0101010101000000ul, 0x2010080402000000ul, 0x0000000000FE0000ul, 0x0000000000000204ul,
                    0x0000000000000101ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0202020202000000ul, 0x4020100804000000ul, 0x0000000000FC0000ul, 0x0000000000000408ul,
                    0x0000000000000202ul, 0x0000000000000100ul, 0x0000000000010000ul, 0x0000000001000000ul),
            new Ray(0x0404040404000000ul, 0x8040201008000000ul, 0x0000000000F80000ul, 0x0000000000000810ul,
                    0x0000000000000404ul, 0x0000000000000201ul, 0x0000000000030000ul, 0x0000000102000000ul),
            new Ray(0x0808080808000000ul, 0x0080402010000000ul, 0x0000000000F00000ul, 0x0000000000001020ul,
                    0x0000000000000808ul, 0x0000000000000402ul, 0x0000000000070000ul, 0x0000010204000000ul),
            new Ray(0x1010101010000000ul, 0x0000804020000000ul, 0x0000000000E00000ul, 0x0000000000002040ul,
                    0x0000000000001010ul, 0x0000000000000804ul, 0x00000000000F0000ul, 0x0001020408000000ul),
            new Ray(0x2020202020000000ul, 0x0000008040000000ul, 0x0000000000C00000ul, 0x0000000000004080ul,
                    0x0000000000002020ul, 0x0000000000001008ul, 0x00000000001F0000ul, 0x0102040810000000ul),
            new Ray(0x4040404040000000ul, 0x0000000080000000ul, 0x0000000000800000ul, 0x0000000000008000ul,
                    0x0000000000004040ul, 0x0000000000002010ul, 0x00000000003F0000ul, 0x0204081020000000ul),
            new Ray(0x8080808080000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000000000008080ul, 0x0000000000004020ul, 0x00000000007F0000ul, 0x0408102040000000ul),
            new Ray(0x0101010100000000ul, 0x1008040200000000ul, 0x00000000FE000000ul, 0x0000000000020408ul,
                    0x0000000000010101ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0202020200000000ul, 0x2010080400000000ul, 0x00000000FC000000ul, 0x0000000000040810ul,
                    0x0000000000020202ul, 0x0000000000010000ul, 0x0000000001000000ul, 0x0000000100000000ul),
            new Ray(0x0404040400000000ul, 0x4020100800000000ul, 0x00000000F8000000ul, 0x0000000000081020ul,
                    0x0000000000040404ul, 0x0000000000020100ul, 0x0000000003000000ul, 0x0000010200000000ul),
            new Ray(0x0808080800000000ul, 0x8040201000000000ul, 0x00000000F0000000ul, 0x0000000000102040ul,
                    0x0000000000080808ul, 0x0000000000040201ul, 0x0000000007000000ul, 0x0001020400000000ul),
            new Ray(0x1010101000000000ul, 0x0080402000000000ul, 0x00000000E0000000ul, 0x0000000000204080ul,
                    0x0000000000101010ul, 0x0000000000080402ul, 0x000000000F000000ul, 0x0102040800000000ul),
            new Ray(0x2020202000000000ul, 0x0000804000000000ul, 0x00000000C0000000ul, 0x0000000000408000ul,
                    0x0000000000202020ul, 0x0000000000100804ul, 0x000000001F000000ul, 0x0204081000000000ul),
            new Ray(0x4040404000000000ul, 0x0000008000000000ul, 0x0000000080000000ul, 0x0000000000800000ul,
                    0x0000000000404040ul, 0x0000000000201008ul, 0x000000003F000000ul, 0x0408102000000000ul),
            new Ray(0x8080808000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000000000808080ul, 0x0000000000402010ul, 0x000000007F000000ul, 0x0810204000000000ul),
            new Ray(0x0101010000000000ul, 0x0804020000000000ul, 0x000000FE00000000ul, 0x0000000002040810ul,
                    0x0000000001010101ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0202020000000000ul, 0x1008040000000000ul, 0x000000FC00000000ul, 0x0000000004081020ul,
                    0x0000000002020202ul, 0x0000000001000000ul, 0x0000000100000000ul, 0x0000010000000000ul),
            new Ray(0x0404040000000000ul, 0x2010080000000000ul, 0x000000F800000000ul, 0x0000000008102040ul,
                    0x0000000004040404ul, 0x0000000002010000ul, 0x0000000300000000ul, 0x0001020000000000ul),
            new Ray(0x0808080000000000ul, 0x4020100000000000ul, 0x000000F000000000ul, 0x0000000010204080ul,
                    0x0000000008080808ul, 0x0000000004020100ul, 0x0000000700000000ul, 0x0102040000000000ul),
            new Ray(0x1010100000000000ul, 0x8040200000000000ul, 0x000000E000000000ul, 0x0000000020408000ul,
                    0x0000000010101010ul, 0x0000000008040201ul, 0x0000000F00000000ul, 0x0204080000000000ul),
            new Ray(0x2020200000000000ul, 0x0080400000000000ul, 0x000000C000000000ul, 0x0000000040800000ul,
                    0x0000000020202020ul, 0x0000000010080402ul, 0x0000001F00000000ul, 0x0408100000000000ul),
            new Ray(0x4040400000000000ul, 0x0000800000000000ul, 0x0000008000000000ul, 0x0000000080000000ul,
                    0x0000000040404040ul, 0x0000000020100804ul, 0x0000003F00000000ul, 0x0810200000000000ul),
            new Ray(0x8080800000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000000080808080ul, 0x0000000040201008ul, 0x0000007F00000000ul, 0x1020400000000000ul),
            new Ray(0x0101000000000000ul, 0x0402000000000000ul, 0x0000FE0000000000ul, 0x0000000204081020ul,
                    0x0000000101010101ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0202000000000000ul, 0x0804000000000000ul, 0x0000FC0000000000ul, 0x0000000408102040ul,
                    0x0000000202020202ul, 0x0000000100000000ul, 0x0000010000000000ul, 0x0001000000000000ul),
            new Ray(0x0404000000000000ul, 0x1008000000000000ul, 0x0000F80000000000ul, 0x0000000810204080ul,
                    0x0000000404040404ul, 0x0000000201000000ul, 0x0000030000000000ul, 0x0102000000000000ul),
            new Ray(0x0808000000000000ul, 0x2010000000000000ul, 0x0000F00000000000ul, 0x0000001020408000ul,
                    0x0000000808080808ul, 0x0000000402010000ul, 0x0000070000000000ul, 0x0204000000000000ul),
            new Ray(0x1010000000000000ul, 0x4020000000000000ul, 0x0000E00000000000ul, 0x0000002040800000ul,
                    0x0000001010101010ul, 0x0000000804020100ul, 0x00000F0000000000ul, 0x0408000000000000ul),
            new Ray(0x2020000000000000ul, 0x8040000000000000ul, 0x0000C00000000000ul, 0x0000004080000000ul,
                    0x0000002020202020ul, 0x0000001008040201ul, 0x00001F0000000000ul, 0x0810000000000000ul),
            new Ray(0x4040000000000000ul, 0x0080000000000000ul, 0x0000800000000000ul, 0x0000008000000000ul,
                    0x0000004040404040ul, 0x0000002010080402ul, 0x00003F0000000000ul, 0x1020000000000000ul),
            new Ray(0x8080000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000008080808080ul, 0x0000004020100804ul, 0x00007F0000000000ul, 0x2040000000000000ul),
            new Ray(0x0100000000000000ul, 0x0200000000000000ul, 0x00FE000000000000ul, 0x0000020408102040ul,
                    0x0000010101010101ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0200000000000000ul, 0x0400000000000000ul, 0x00FC000000000000ul, 0x0000040810204080ul,
                    0x0000020202020202ul, 0x0000010000000000ul, 0x0001000000000000ul, 0x0100000000000000ul),
            new Ray(0x0400000000000000ul, 0x0800000000000000ul, 0x00F8000000000000ul, 0x0000081020408000ul,
                    0x0000040404040404ul, 0x0000020100000000ul, 0x0003000000000000ul, 0x0200000000000000ul),
            new Ray(0x0800000000000000ul, 0x1000000000000000ul, 0x00F0000000000000ul, 0x0000102040800000ul,
                    0x0000080808080808ul, 0x0000040201000000ul, 0x0007000000000000ul, 0x0400000000000000ul),
            new Ray(0x1000000000000000ul, 0x2000000000000000ul, 0x00E0000000000000ul, 0x0000204080000000ul,
                    0x0000101010101010ul, 0x0000080402010000ul, 0x000F000000000000ul, 0x0800000000000000ul),
            new Ray(0x2000000000000000ul, 0x4000000000000000ul, 0x00C0000000000000ul, 0x0000408000000000ul,
                    0x0000202020202020ul, 0x0000100804020100ul, 0x001F000000000000ul, 0x1000000000000000ul),
            new Ray(0x4000000000000000ul, 0x8000000000000000ul, 0x0080000000000000ul, 0x0000800000000000ul,
                    0x0000404040404040ul, 0x0000201008040201ul, 0x003F000000000000ul, 0x2000000000000000ul),
            new Ray(0x8000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000808080808080ul, 0x0000402010080402ul, 0x007F000000000000ul, 0x4000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0xFE00000000000000ul, 0x0002040810204080ul,
                    0x0001010101010101ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0xFC00000000000000ul, 0x0004081020408000ul,
                    0x0002020202020202ul, 0x0001000000000000ul, 0x0100000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0xF800000000000000ul, 0x0008102040800000ul,
                    0x0004040404040404ul, 0x0002010000000000ul, 0x0300000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0xF000000000000000ul, 0x0010204080000000ul,
                    0x0008080808080808ul, 0x0004020100000000ul, 0x0700000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0xE000000000000000ul, 0x0020408000000000ul,
                    0x0010101010101010ul, 0x0008040201000000ul, 0x0F00000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0xC000000000000000ul, 0x0040800000000000ul,
                    0x0020202020202020ul, 0x0010080402010000ul, 0x1F00000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0x8000000000000000ul, 0x0080000000000000ul,
                    0x0040404040404040ul, 0x0020100804020100ul, 0x3F00000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0080808080808080ul, 0x0040201008040201ul, 0x7F00000000000000ul, 0x0000000000000000ul),
            new Ray(0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul,
                    0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul)
            #endregion
        };

        public static readonly FixedArray<Ray> RevVectors = new(MAX_SQUARES + 1, true);

        private static readonly FixedArray2D<sbyte> pawnLeft = new(MAX_COLORS, MAX_SQUARES)
        {
            #region pawnLeft data
            -1,  8,  9, 10, 11, 12, 13, 14,
            -1, 16, 17, 18, 19, 20, 21, 22,
            -1, 24, 25, 26, 27, 28, 29, 30,
            -1, 32, 33, 34, 35, 36, 37, 38,
            -1, 40, 41, 42, 43, 44, 45, 46,
            -1, 48, 49, 50, 51, 52, 53, 54,
            -1, 56, 57, 58, 59, 60, 61, 62,
            -1, -1, -1, -1, -1, -1, -1, -1,
            
            -1, -1, -1, -1, -1, -1, -1, -1,
            -1,  0,  1,  2,  3,  4,  5,  6,
            -1,  8,  9, 10, 11, 12, 13, 14,
            -1, 16, 17, 18, 19, 20, 21, 22,
            -1, 24, 25, 26, 27, 28, 29, 30,
            -1, 32, 33, 34, 35, 36, 37, 38,
            -1, 40, 41, 42, 43, 44, 45, 46,
            -1, 48, 49, 50, 51, 52, 53, 54
            #endregion
        };

        private static readonly FixedArray2D<sbyte> pawnRight = new(MAX_COLORS, MAX_SQUARES)
        {
            #region pawnRight data
             9, 10, 11, 12, 13, 14, 15, -1,
            17, 18, 19, 20, 21, 22, 23, -1,
            25, 26, 27, 28, 29, 30, 31, -1,
            33, 34, 35, 36, 37, 38, 39, -1,
            41, 42, 43, 44, 45, 46, 47, -1,
            49, 50, 51, 52, 53, 54, 55, -1,
            57, 58, 59, 60, 61, 62, 63, -1,
            -1, -1, -1, -1, -1, -1, -1, -1,
            
            -1, -1, -1, -1, -1, -1, -1, -1,
             1,  2,  3,  4,  5,  6,  7, -1,
             9, 10, 11, 12, 13, 14, 15, -1,
            17, 18, 19, 20, 21, 22, 23, -1,
            25, 26, 27, 28, 29, 30, 31, -1,
            33, 34, 35, 36, 37, 38, 39, -1,
            41, 42, 43, 44, 45, 46, 47, -1,
            49, 50, 51, 52, 53, 54, 55, -1
            #endregion
        };

        private static readonly FixedArray2D<sbyte> pawnPlus = new(MAX_COLORS, MAX_SQUARES)
        {
            #region pawnPlus data
             8,  9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23,
            24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55,
            56, 57, 58, 59, 60, 61, 62, 63,
            -1, -1, -1, -1, -1, -1, -1, -1,
            
            -1, -1, -1, -1, -1, -1, -1, -1,
             0,  1,  2,  3,  4,  5,  6,  7,
             8,  9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23,
            24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55
            #endregion
        };

        private static readonly FixedArray2D<sbyte> pawnDouble = new(MAX_COLORS, MAX_SQUARES)
        {
            #region pawnDouble data
            16, 17, 18, 19, 20, 21, 22, 23,
            24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55,
            56, 57, 58, 59, 60, 61, 62, 63,
            -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1,
            
            -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1,
             0,  1,  2,  3,  4,  5,  6,  7,
             8,  9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23,
            24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47
            #endregion
        };


        #endregion
    }
}
