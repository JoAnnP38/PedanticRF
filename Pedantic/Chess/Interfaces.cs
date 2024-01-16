using System.Collections;

namespace Pedantic.Chess
{
    public interface IHistory
    {
        public short this[Color stm, Piece piece, SquareIndex to] { get; }
        public short this[Move move] { get; }
    }

    public interface IInitialize
    {
        public static void Initialize() { }
    }
}
