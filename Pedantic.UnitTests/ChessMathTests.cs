namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class ChessMathTests
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void IsValidTests()
        {
            Assert.IsTrue(ChessMath.IsValidCoord(3));
            Assert.IsFalse(ChessMath.IsValidCoord(8));
            Assert.IsTrue(ChessMath.IsValidSquare(32));
            Assert.IsFalse(ChessMath.IsValidSquare(-1));
        }

        [TestMethod]
        public void ToSquareIndexTests()
        {
            SquareIndex sq = ChessMath.ToSquareIndex(File.FileF, Rank.Rank3);
            Assert.AreEqual(SquareIndex.F3, sq);

            sq = ChessMath.ToSquareIndex((int)File.FileF, (int)Rank.Rank3);
            Assert.AreEqual(SquareIndex.F3, sq);
        }

        [TestMethod]
        public void DistanceTests()
        {
            SquareIndex sq1 = SquareIndex.E2;
            SquareIndex sq2 = SquareIndex.H3;

            Assert.AreEqual(3, ChessMath.Distance(sq1, sq2));
            Assert.AreEqual(3, ChessMath.Distance(sq1.File(), sq1.Rank(), sq2.File(), sq2.Rank()));
            Assert.AreEqual(3, ChessMath.Distance((int)sq1.File(), (int)sq1.Rank(), (int)sq2.File(), (int)sq2.Rank()));

            Assert.AreEqual(4, ChessMath.ManhattanDistance(sq1, sq2));
            Assert.AreEqual(4, ChessMath.ManhattanDistance(sq1.File(), sq1.Rank(), sq2.File(), sq2.Rank()));
            Assert.AreEqual(4, ChessMath.ManhattanDistance((int)sq1.File(), (int)sq1.Rank(), (int)sq2.File(), (int)sq2.Rank()));

            Assert.AreEqual(3, ChessMath.CenterDistance(SquareIndex.H3));
        }

        [TestMethod]
        public void GetDirectionTests()
        {
            SquareIndex sq1 = SquareIndex.E4;
            SquareIndex sq2 = SquareIndex.E6;

            Assert.IsTrue(ChessMath.GetDirection(sq1, sq2, out Direction dir));
            Assert.AreEqual(Direction.North, dir);

            sq2 = SquareIndex.G2;
            Assert.IsTrue(ChessMath.GetDirection(sq1, sq2, out dir));
            Assert.AreEqual(Direction.SouthEast, dir);

            sq2 = SquareIndex.F6;
            Assert.IsFalse(ChessMath.GetDirection(sq1, sq2, out _));
        }
    }
}
