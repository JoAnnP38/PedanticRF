namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class MoveListTests
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void CtorTests()
        {
            MoveList list = new();
            Assert.AreEqual(0, list.Count);
            Assert.Fail("Update test using real History implementation.");
        }

        [TestMethod]
        public void CountTest()
        {
            MoveList list = new();
            list.AddQuiet(Color.White, Piece.Pawn, SquareIndex.E2, SquareIndex.E4, MoveType.DblPawnMove);
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void IndexerTest()
        {
            MoveList list = new();
            list.AddQuiet(Color.White, Piece.Pawn, SquareIndex.E2, SquareIndex.E4, MoveType.DblPawnMove);
            Move move = list[0];
            Assert.AreEqual(Color.White, move.Stm);
            Assert.AreEqual(Piece.Pawn, move.Piece);
            Assert.AreEqual(SquareIndex.E2, move.From);
            Assert.AreEqual(SquareIndex.E4, move.To);
            Assert.AreEqual(MoveType.DblPawnMove, move.Type);
            Assert.AreEqual(Piece.None, move.Capture);
            Assert.AreEqual(Piece.None, move.Promote);
        }

        [TestMethod]
        public void GetScoreSortTest()
        {
            MoveList list = new();
            list.AddQuiet(Color.White, Piece.Pawn, SquareIndex.E2, SquareIndex.E4, MoveType.DblPawnMove);
            list.AddPromote(Color.White, SquareIndex.E7, SquareIndex.E8, Piece.Queen);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.E4, SquareIndex.D5, MoveType.Capture, Piece.Pawn);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.D7, SquareIndex.E8, MoveType.PromoteCapture, Piece.Rook, Piece.Queen);

            Move moveQuiet = list[0];
            Move movePromote = list[1];
            Move moveCapture = list[2];
            Move movePromoteCapture = list[3];

            Assert.AreEqual(0, list.GetScore(0));
            Assert.AreEqual(MoveList.PromoteScore(Piece.Queen), list.GetScore(1));
            Assert.AreEqual(MoveList.CaptureScore(Piece.Pawn, Piece.Pawn), list.GetScore(2));
            Assert.AreEqual(MoveList.CaptureScore(Piece.Rook, Piece.Pawn, Piece.Queen), list.GetScore(3));

            Assert.AreEqual(movePromoteCapture, list.Sort(0));
            Assert.AreEqual(moveCapture, list.Sort(1));
            Assert.AreEqual(movePromote, list.Sort(2));
            Assert.AreEqual(moveQuiet, list.Sort(3));

            Assert.Fail("Update test using real History implementation.");
        }

        [TestMethod]
        public void ClearTest()
        {
            MoveList list = new();
            list.AddQuiet(Color.White, Piece.Pawn, SquareIndex.E2, SquareIndex.E4, MoveType.DblPawnMove);
            list.AddPromote(Color.White, SquareIndex.E7, SquareIndex.E8, Piece.Queen);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.E4, SquareIndex.D5, MoveType.Capture, Piece.Pawn);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.D7, SquareIndex.E8, MoveType.PromoteCapture, Piece.Rook, Piece.Queen);

            Assert.AreEqual(4, list.Count);

            list.Clear();
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void RemoveTest()
        {
            MoveList list = new();
            list.AddQuiet(Color.White, Piece.Pawn, SquareIndex.E2, SquareIndex.E4, MoveType.DblPawnMove);
            list.AddPromote(Color.White, SquareIndex.E7, SquareIndex.E8, Piece.Queen);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.E4, SquareIndex.D5, MoveType.Capture, Piece.Pawn);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.D7, SquareIndex.E8, MoveType.PromoteCapture, Piece.Rook, Piece.Queen);

            Move move = list[3];

            Assert.IsTrue(list.Remove(move));
            Assert.AreEqual(3, list.Count);
            Assert.IsFalse(list.Remove(move));
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            MoveList list = new();
            list.AddQuiet(Color.White, Piece.Pawn, SquareIndex.E2, SquareIndex.E4, MoveType.DblPawnMove);
            list.AddPromote(Color.White, SquareIndex.E7, SquareIndex.E8, Piece.Queen);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.E4, SquareIndex.D5, MoveType.Capture, Piece.Pawn);
            list.AddCapture(Color.White, Piece.Pawn, SquareIndex.D7, SquareIndex.E8, MoveType.PromoteCapture, Piece.Rook, Piece.Queen);

            int index = 0;
            foreach (Move move in list)
            {
                Assert.AreEqual(list[index++], move);
            }
        }
    }
}
