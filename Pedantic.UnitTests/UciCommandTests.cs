using Pedantic;
using System.Text;

namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class UciCommandTests
    {
        public TestContext? TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            Program.InitializeStaticData();
        }

        [TestMethod]
        public void DebugTest()
        {
            Assert.IsFalse(Engine.Debug);
            Program.ParseUciCommand("debug on");
            Assert.IsTrue(Engine.Debug);
            Program.ParseUciCommand("debug off");
            Assert.IsFalse(Engine.Debug);
        }

        [TestMethod]
        public void IsReadyTest()
        {
            using StringWriter sw = new();
            Console.SetOut(sw);
            Program.ParseUciCommand("isready");
            string expected = $"readyok{Environment.NewLine}";
            Assert.AreEqual(expected, sw.ToString());
        }

        [TestMethod]
        public void PositionTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Assert.AreEqual(Constants.FEN_START_POS, Engine.Board.ToFenString());
            Program.ParseUciCommand("position startpos moves e2e4 e7e5");
            Assert.AreEqual(Engine.Board.PieceBoard(SquareIndex.E4).Color, Color.White);
            Assert.AreEqual(Engine.Board.PieceBoard(SquareIndex.E4).Piece, Piece.Pawn);
            Assert.AreEqual(Engine.Board.PieceBoard(SquareIndex.E5).Color, Color.Black);
            Assert.AreEqual(Engine.Board.PieceBoard(SquareIndex.E5).Piece, Piece.Pawn);
        }

        [TestMethod]
        public void SetOptionTest()
        {
            Assert.AreEqual(2, UciOptions.PromotionDepth);
            Program.ParseUciCommand("setoption name UCI_T_QS_PromotionDepth value 3");
            Assert.AreEqual(3, UciOptions.PromotionDepth);
        }

        [TestMethod]
        public void GoFixedDepthTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position startpos");
            Program.ParseUciCommand("go depth 8");
            Program.ParseUciCommand("wait");
        }

        [TestMethod]
        public void GoFixedNodesTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position startpos");
            Program.ParseUciCommand("go nodes 1000000");
            Program.ParseUciCommand("wait");
        }

        [TestMethod]
        public void GoFixedTimeTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position startpos");
            Program.ParseUciCommand("go movetime 5000");
            Program.ParseUciCommand("wait");
        }

        [TestMethod]
        public void GoTimeNoIncrementTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position startpos");
            Program.ParseUciCommand("go wtime 15000 btime 15000");
            Program.ParseUciCommand("wait");
        }

        [TestMethod]
        public void GoTimeWithIncrementTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position startpos");
            Program.ParseUciCommand("go wtime 15000 winc 100 btime 15000 binc 100");
            Program.ParseUciCommand("wait");
        }

        [TestMethod]
        public void BlindToCheckmateTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position fen 1r1r2k1/3p1ppp/1N1bpB2/1P6/2P1bq2/P7/2B1QPPP/R4RK1 w - - 0 20");
            Program.ParseUciCommand("go depth 4");
            Program.ParseUciCommand("wait");
        }

        [TestMethod]
        public void IndexOutOfRangeExceptionTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position fen 4r2k/P5p1/3p1p1p/3P4/8/6K1/6B1/5R2 b - - 0 49");
            Program.ParseUciCommand("go depth 16");
            Program.ParseUciCommand("wait");
        }

        [TestMethod]
        public void HungMoveGenerationTest()
        {
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand("position fen r3k2r/2pb1ppp/2pp1q2/p7/1nP1B3/1P2P3/P2N1PPP/R2QK2R w KQkq a6 0 14");
            Program.ParseUciCommand("go depth 12");
            Program.ParseUciCommand("wait");
        }


        [TestMethod]
        public void InconsistentBenchTest()
        {
            Program.ParseUciCommand("uci");

            for (int n = 0; n < 3; n++)
            {
                Program.ParseUciCommand("bench depth 12");
                Program.ParseUciCommand("wait");
            }
        }
    }
}
