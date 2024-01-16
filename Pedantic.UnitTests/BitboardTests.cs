namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class BitboardTests
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void CtorTests()
        {
            Bitboard bb = new();
            Assert.AreEqual(0ul, bb);

            bb = new(3ul);
            Assert.AreEqual(3ul, bb);

            bb = new(SquareIndex.A1);
            Assert.AreEqual(0x01ul, bb);

            bb = new(File.FileA);
            Assert.AreEqual(Bitboard.FILE_A_MASK, bb);

            bb = new(Rank.Rank1);
            Assert.AreEqual(Bitboard.RANK_1_MASK, bb);
        }

        [TestMethod]
        public void CountTests()
        {
            Bitboard bb = new(SquareIndex.D1);
            Assert.AreEqual(3, bb.TzCount);
            Assert.AreEqual(60, bb.LzCount);
            Assert.AreEqual(1, bb.PopCount);
        }

        [TestMethod]
        public void ResetLsbTest()
        {
            Bitboard bb = new(3ul);
            bb = bb.ResetLsb();
            Assert.AreEqual(2ul, bb);
        }

        [TestMethod]
        public void AndNotTest()
        {
            Bitboard bb = new(3ul);
            bb = bb.AndNot(bb);
            Assert.AreEqual(0ul, bb);
        }

        [TestMethod]
        public void EqualityTests()
        {
            Bitboard bb1 = new(3ul);
            Bitboard bb2 = new(SquareIndex.D1);
            Assert.IsFalse(bb1.Equals(bb2));

            object obj = bb1;
            Assert.IsFalse(obj.Equals(bb2));

            Assert.IsFalse(bb1 == bb2);
            Assert.IsTrue(bb1 != bb2);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            ulong value = 0x0489000b2ul;
            Bitboard bb = new(value);

            Assert.AreEqual(value.GetHashCode(), bb.GetHashCode());
        }

        [TestMethod]
        public void ToStringTest()
        {
            Bitboard bb = new(0x0489000b2ul);
            string bbString = bb.ToString();
            Assert.IsTrue(!string.IsNullOrEmpty(bbString));

            TestContext?.WriteLine(bbString);
            Assert.AreEqual(144, bbString.Length);
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            Bitboard bb = new Bitboard(SquareIndex.A3) | new Bitboard(SquareIndex.D5);
            SquareIndex[] expected = [ SquareIndex.A3, SquareIndex.D5 ];

            int n = 0;
            foreach (SquareIndex sq in bb)
            {
                Assert.AreEqual(expected[n++], sq);
            }
        }

        [TestMethod]
        public void OperatorTests()
        {
            // conversion operators
            Bitboard bb = new(0x0489000b2ul);
            ulong value = bb;
            Assert.AreEqual(0x0489000b2ul, value);

            bb = (Bitboard)0x0489000b2ul;
            Assert.AreEqual(0x0489000b2ul, bb);

            // bitwise operators
            bb = new(SquareIndex.D1);
            bb = bb << 8;
            Assert.AreEqual(new Bitboard(SquareIndex.D2), bb);

            bb = bb >> 1;
            Assert.AreEqual(new Bitboard(SquareIndex.C2), bb);

            Bitboard bb1 = new(0x01ul);
            Bitboard bb2 = new(0x02ul);

            bb = bb1 & bb2;
            Assert.AreEqual(0ul, bb);

            bb = bb1 | bb2;
            Assert.AreEqual(3ul, bb);

            bb = bb1 ^ bb2;
            Assert.AreEqual(3ul, bb);

            bb = bb ^ bb1;
            Assert.AreEqual(bb2, bb);

            bb = ~bb2;
            Assert.AreEqual(0xFFFFFFFFFFFFFFFDul, bb);
        }
    }
}