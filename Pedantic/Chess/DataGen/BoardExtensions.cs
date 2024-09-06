namespace Pedantic.Chess.DataGen
{
    using Pedantic.Chess;
    using Pedantic.Utilities;

    public static class BoardExtensions
    {
        public static PedanticFormat ToBinary(this Board board, ushort maxPly, short eval, Result result)
        {
            ushort ply = (ushort)((board.FullMoveCounter - 1) * 2 + (board.SideToMove == Color.White ? 1 : 2));
            PedanticFormat pdata = default;
            pdata.Hash = board.Hash;
            pdata.Occupancy = board.All;
            pdata.Eval = eval;
            pdata.MoveCounter = board.FullMoveCounter;
            pdata.HalfMoveClock = board.HalfMoveClock;
            pdata.MaxPly = maxPly;
            pdata.Ply = ply;
            pdata.Castling = board.Castling;
            pdata.EpFile = board.EnPassantValidated == SquareIndex.None ? File.None : board.EnPassantValidated.File();
            pdata.Wdl = result;
            pdata.Stm = board.SideToMove;

            int index = 0;
            foreach (SquareIndex sq in board.All)
            {
                var pc = board.PieceBoard(sq);
                pdata.SetPiece(index++, pc.Color, pc.Piece);
            }

            return pdata;
        }

        public static Board LoadBinary(this Board board, ref PedanticFormat pdata)
        {
            board.Clear();
            int index = 0;
            foreach (SquareIndex sq in pdata.Occupancy)
            {
                var pc = pdata.GetPiece(index++);
                board.AddPiece(pc.Color, pc.Piece, sq);
            }

            board.SetSideToMove(pdata.Stm, true);
            board.SetCastling(pdata.Castling, true);
            board.SetEnPassant(pdata.EpFile, true);
            board.SetMoveCounters(pdata.HalfMoveClock, pdata.MoveCounter);
            return board;
        }

        public static ulong CalculateHash(this Board board)
        {
            ulong hash = 0;

            foreach (SquareIndex sq in board.All)
            {
                Square square = board.PieceBoard(sq);
                hash = ZobristHash.HashPiece(hash, square.Color, square.Piece, sq);
            }

            hash = ZobristHash.HashCastling(hash, board.Castling);

            if (board.EnPassantValidated != SquareIndex.None)
            {
                hash = ZobristHash.HashEnPassant(hash, board.EnPassantValidated);
            }

            hash = ZobristHash.HashActiveColor(hash, board.SideToMove);

            return hash;
        }
    }
}
