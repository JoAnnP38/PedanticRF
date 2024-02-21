namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("Puzzles")]
    public class MateInPuzzles
    {
        public TestContext? TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            Program.InitializeStaticData();
        }

       
        [TestMethod]
        [DataRow("r2qk2r/pb4pp/1n2Pb2/2B2Q2/p1p5/2P5/2B2PPP/RN2R1K1 w - - 1 0", 6)]
        public void MateIn(string fen, int depth)
        {
            // f5h5 g7g6 c2g6 h7g6
            Program.ParseUciCommand("uci");
            Program.ParseUciCommand("isready");
            Program.ParseUciCommand("ucinewgame");
            Program.ParseUciCommand($"position fen {fen}");
            Program.ParseUciCommand($"go depth {depth}");
            Program.ParseUciCommand("wait");
        }
    }
}
