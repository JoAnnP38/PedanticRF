using Pedantic.Chess;
using Pedantic.Chess.HCE;

namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]    
    public class EvaluationTests
    {
        public TestContext? TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            Program.InitializeStaticData();
        }


        [TestMethod]
        public void ComputeTest()
        {
            Board board = new Board(Constants.FEN_START_POS);
            HceEval eval = new(Weights.Default);
            short result = eval.Compute(board);
            Assert.AreEqual(0, result);

            board.LoadFen("r3k2r/2pb1ppp/2pp1q2/p7/1nP1B3/1P2P3/P2N1PPP/R2QK2R w KQkq a6 0 14");
            result = eval.Compute(board);
            Assert.AreEqual(0, result);
        }

    }
}
