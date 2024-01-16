namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class NotationTests
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void IsValidFenTest()
        {
            Assert.IsTrue(Notation.IsValidFen(Notation.FEN_START_POS));
            Assert.IsTrue(Notation.IsValidFen(Notation.FEN_KIWI_PETE));
            Assert.IsTrue(Notation.IsValidFen(Notation.FEN_EMPTY));
            Assert.IsFalse(Notation.IsValidFen("INVALID"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseFenPieceTest()
        {
            var piece = Notation.ParseFenPiece('n');
            Assert.AreEqual(Color.Black, piece.color);
            Assert.AreEqual(Piece.Knight, piece.piece);

            // should throw exception
            Notation.ParseFenPiece('y');
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void ParseFenColorTest()
        {
            string s = "b";
            Assert.AreEqual(Color.Black, Notation.ParseFenColor(s));

            // should throw exception
            Notation.ParseFenColor(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), AllowDerivedTypes = true)]
        public void ParseFenColorFromSpanTest()
        {
            ReadOnlySpan<char> span = "w".AsSpan();
            Assert.AreEqual(Color.White, Notation.ParseFenColor(span));

            // should throw exception
            Notation.ParseFenColor(string.Empty.AsSpan());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void ParseFenCastlingRightsTest()
        {
            CastlingRights cr = CastlingRights.WhiteQueenSide | CastlingRights.BlackKingSide | CastlingRights.BlackQueenSide;
            Assert.AreEqual(cr, Notation.ParseFenCastlingRights("Qkq"));
            Assert.AreEqual(cr, Notation.ParseFenCastlingRights("Qkq".AsSpan()));

            // should throw exception
            Notation.ParseFenCastlingRights("x");
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), AllowDerivedTypes = true)]
        public void ParseFenEnPassantTest()
        {
            Assert.AreEqual(SquareIndex.None, Notation.ParseFenEnPassant("-"));
            Assert.AreEqual(SquareIndex.None, Notation.ParseFenEnPassant("-".AsSpan()));
            Assert.AreEqual(SquareIndex.C4, Notation.ParseFenEnPassant("c4"));
            Assert.AreEqual(SquareIndex.C4, Notation.ParseFenEnPassant("c4".AsSpan()));

            // should throw exception
            Notation.ParseFenEnPassant(string.Empty);
        }
    }
}
