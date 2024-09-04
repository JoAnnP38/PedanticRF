namespace Pedantic.Chess.DataGen
{
    using Pedantic.Chess;
    using Pedantic.Utilities;

    public static class BoardExtensions
    {
        public static PedanticFormat ToBinary(this Board board, ushort ply, ushort maxPly, short eval, Result result)
        {
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
    }
}
