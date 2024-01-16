
namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class MoveTests
    {
        public TestContext? TestContest { get; set; }

        [TestMethod]
        public void CtorAccessorTests()
        {
            Move move = new Move(Color.White, Piece.Pawn, SquareIndex.E2, SquareIndex.E4, MoveType.DblPawnMove);
            Assert.AreEqual(Color.White, move.Stm);
            Assert.AreEqual(Piece.Pawn, move.Piece);
            Assert.AreEqual(SquareIndex.E2, move.From);
            Assert.AreEqual(SquareIndex.E4, move.To);
            Assert.AreEqual(MoveType.DblPawnMove, move.Type);
            Assert.AreEqual(Piece.None, move.Capture);
            Assert.AreEqual(Piece.None, move.Promote);
            Assert.IsFalse(move.IsCapture);
            Assert.IsFalse(move.IsPromote);
            Assert.IsTrue(move.IsPawnMove);
            Assert.IsFalse(move.IsNoisy);
            Assert.IsTrue(move.IsQuiet);
        }

        [TestMethod]
        public void EqualityTest()
        {
            Move move1 = new Move(Color.Black, Piece.Knight, SquareIndex.C3, SquareIndex.B5, capture: Piece.Bishop);
            Move move2 = new Move(Color.Black, Piece.Knight, SquareIndex.C3, SquareIndex.E2);

            Assert.IsFalse(move1.Equals(move2));
            Assert.IsFalse(move1 == move2);
            Assert.IsTrue(move1 != move2);
            Assert.IsFalse(((object)move1).Equals(move2));

            move2 = new Move(Color.Black, Piece.Knight, (int)SquareIndex.C3, (int)SquareIndex.B5, capture: Piece.Bishop);

            Assert.IsTrue(move1.Equals(move2));
            Assert.IsFalse(move1.Equals(Move.NullMove));
            Assert.IsTrue(move1 == move2);
            Assert.IsFalse(move1 != move2);
        }

    }
}
