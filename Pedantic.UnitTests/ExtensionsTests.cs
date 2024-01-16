namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class ExtensionsTests
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        public void ColorFlipTest()
        {
            Color color = Color.White;
            Assert.AreEqual(Color.Black, color.Flip());

            color = Color.None;
            Assert.AreEqual(Color.None, color.Flip());
        }

        [TestMethod]
        public void ColorToFenStringTest()
        {
            Color color = Color.Black;
            Assert.AreEqual("b", color.ToFenString());

            bool fail = true;
            color = Color.None;
            try
            {
                string s = color.ToFenString();
            }
            catch
            {
                fail = false;
            }
            Assert.IsFalse(fail);
        }

        [TestMethod]
        public void PieceIsDiagonalSliderTest()
        {
            Piece piece = Piece.Bishop;
            Assert.IsTrue(piece.IsDiagonalSlider());

            piece = Piece.Knight;
            Assert.IsFalse(piece.IsDiagonalSlider());
        }

        [TestMethod]
        public void PieceIsOrthogonalSliderTest()
        {
            Piece piece = Piece.Rook;
            Assert.IsTrue(piece.IsOrthogonalSlider());

            piece = Piece.King;
            Assert.IsFalse(piece.IsOrthogonalSlider());
        }

        [TestMethod]
        public void PieceIsSliderTest()
        {
            Piece piece = Piece.Queen;
            Assert.IsTrue(piece.IsSlider());

            piece = Piece.None;
            Assert.IsFalse(piece.IsSlider());
        }

        [TestMethod]
        public void PieceValueTest()
        {
            Piece piece = Piece.Knight;
            Assert.AreEqual(300, piece.Value());

            piece = Piece.None;
            Assert.AreEqual(0, piece.Value());
        }

        [TestMethod]
        public void PiecePhaseValueTest()
        {
            Piece piece = Piece.Pawn;
            Assert.AreEqual(1, piece.PhaseValue());

            piece = Piece.King;
            Assert.AreEqual(0, piece.PhaseValue());
        }

        [TestMethod]
        public void PieceToCharTest()
        {
            Piece piece = Piece.Knight;
            Assert.AreEqual('N', piece.ToChar());
            Assert.AreEqual('n', piece.ToChar(Color.Black));
        }

        [TestMethod]
        public void PieceToStringTest()
        {
            Piece piece = Piece.Queen;
            Assert.AreEqual("Queen", piece.ToString());

            piece = Piece.None;
            Assert.AreEqual("None", piece.ToString());
        }

        [TestMethod]
        public void CastlingRightsToFenStringTest()
        {
            CastlingRights castling = CastlingRights.None;
            Assert.AreEqual("-", castling.ToFenString());

            castling = CastlingRights.WhiteQueenSide | CastlingRights.BlackKingSide;
            Assert.AreEqual("Qk", castling.ToFenString());
        }

        [TestMethod]
        public void DirectionIsOrthogonalTest()
        {
            Direction dir = Direction.East;
            Assert.IsTrue(dir.IsOrthogonal());
            Assert.IsFalse(dir.IsDiagonal());
        }

        [TestMethod]
        public void SquareIndexFileRankTests()
        {
            SquareIndex sq = SquareIndex.F5;
            Assert.AreEqual(File.FileF, sq.File());
            Assert.AreEqual(Rank.Rank5, sq.Rank());

            var coords = sq.ToCoords();
            Assert.AreEqual(File.FileF, coords.File);
            Assert.AreEqual(Rank.Rank5, coords.Rank);
        }

        [TestMethod]
        public void SquareIndexToStringTest()
        {
            SquareIndex sq = SquareIndex.B7;
            Assert.AreEqual("b7", SquareIndexExtensions.ToString(sq));

            sq = SquareIndex.None;
            Assert.AreEqual(string.Empty, SquareIndexExtensions.ToString(sq));
        }

        [TestMethod]
        public void SquareIndexToFenStringTest()
        {
            SquareIndex sq = SquareIndex.B7;
            Assert.AreEqual("b7", sq.ToFenString());

            sq = SquareIndex.None;
            Assert.AreEqual("-", sq.ToFenString());
        }

        [TestMethod]
        public void SquareIndexIsLightDarkTests()
        {
            SquareIndex sq = SquareIndex.B2;
            Assert.IsFalse(sq.IsLight());
            Assert.IsTrue(sq.IsDark());

            sq = SquareIndex.E4;
            Assert.IsFalse(sq.IsDark());
            Assert.IsTrue(sq.IsLight());
        }

        [TestMethod]
        public void SquareIndexNormalizeTest()
        {
            SquareIndex sq = SquareIndex.D2;
            Assert.AreEqual(SquareIndex.D2, sq.Normalize(Color.White));
            Assert.AreEqual(SquareIndex.D7, sq.Normalize(Color.Black));
        }
    }
}
