namespace Pedantic.Chess
{
    public enum Color : sbyte { None = -1, White, Black }

    public enum Piece : sbyte { None = -1, Pawn, Knight, Bishop, Rook, Queen, King }

    [Flags]
    public enum CastlingRights : byte
    {
        None,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8,
        WhiteRights = WhiteKingSide | WhiteQueenSide,
        BlackRights = BlackKingSide | BlackQueenSide,
        All = WhiteKingSide | WhiteQueenSide | BlackKingSide | BlackQueenSide
    }

    public enum MoveType : byte
    {
        Normal,
        Capture, 
        Castle, 
        EnPassant, 
        PawnMove, 
        DblPawnMove, 
        Promote, 
        PromoteCapture,
        Null = 15
    }

    public enum Bound : sbyte { None = -1, Exact, Upper, Lower }

    public enum GamePhase : byte { Opening, MidGame, EndGame, EndGameMopup }

    public enum File : sbyte { None = -1, FileA, FileB, FileC, FileD, FileE, FileF, FileG, FileH }

    public enum Rank : sbyte { None = -1, Rank1, Rank2, Rank3, Rank4, Rank5, Rank6, Rank7, Rank8 }

    public enum SquareIndex : sbyte
    {
        None = -1,
        A1, B1, C1, D1, E1, F1, G1, H1,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A8, B8, C8, D8, E8, F8, G8, H8
    }

    public enum Direction : sbyte { None = -1, North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest }

    public enum MoveGenPhase : byte
    {
        HashMove,
        GoodCapture,
        Promotion,
        Killer,
        Counter,
        BadCapture,
        Quiet,
    }
}
