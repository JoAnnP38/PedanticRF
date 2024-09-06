namespace Pedantic.UnitTests
{
    using Pedantic.Chess.DataGen;

    [TestClass]
    [TestCategory("UnitTests")]
    public class BoardTests
    {
        public TestContext? TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Program.InitializeStaticData();
        }

        [TestMethod]
        public void CtorTest()
        {
            Board board = new();
            Assert.AreEqual(0ul, board.All);
            Assert.AreEqual(0ul, board.Units(Color.White));
            Assert.AreEqual(0ul, board.Units(Color.Black));
            Assert.AreEqual(0ul, board.Pawns);
            Assert.AreEqual(0ul, board.Knights);
            Assert.AreEqual(0ul, board.Bishops);
            Assert.AreEqual(0ul, board.Rooks);
            Assert.AreEqual(0ul, board.Queens);
            Assert.AreEqual(0ul, board.Kings);
            Assert.AreEqual(0ul, board.Hash);
        }

        [TestMethod]
        public void CtorFenTest()
        {
            Board board = new(Notation.FEN_START_POS);
            Assert.AreEqual(32, board.All.PopCount);
            Assert.AreEqual(16, board.Units(Color.White).PopCount);
            Assert.AreEqual(16, board.Units(Color.Black).PopCount);
            Assert.AreEqual(16, board.Pawns.PopCount);
            Assert.AreEqual(4, board.Knights.PopCount);
            Assert.AreEqual(4, board.Bishops.PopCount);
            Assert.AreEqual(4, board.Rooks.PopCount);
            Assert.AreEqual(2, board.Queens.PopCount);
            Assert.AreEqual(2, board.Kings.PopCount);
            Assert.AreEqual(0x463b96181691fc9cul, board.Hash);
        }

        [TestMethod]
        public void CtorCopyTest()
        {
            Board board = new(Notation.FEN_KIWI_PETE);
            Board clone = board.Clone();

            Assert.AreNotSame(board, clone);
            string fen = clone.ToFenString();
            Assert.AreEqual(Notation.FEN_KIWI_PETE, fen);
        }

        [TestMethod]
        public void ClearTest()
        {
            Board board = new(Notation.FEN_START_POS);
            board.Clear();
            Assert.AreEqual(0ul, board.All);
            Assert.AreEqual(0ul, board.Units(Color.White));
            Assert.AreEqual(0ul, board.Units(Color.Black));
            Assert.AreEqual(0ul, board.Pawns);
            Assert.AreEqual(0ul, board.Knights);
            Assert.AreEqual(0ul, board.Bishops);
            Assert.AreEqual(0ul, board.Rooks);
            Assert.AreEqual(0ul, board.Queens);
            Assert.AreEqual(0ul, board.Kings);
            Assert.AreEqual(0ul, board.Hash);
        }

        [TestMethod]
        public void ToStringTest()
        {
            Board board = new(Notation.FEN_START_POS);
            string? boardString = board.ToString();
            Assert.IsNotNull(boardString);
            TestContext?.WriteLine($"ToString() : {boardString}\n");

            boardString = board.ToString("V");
            Assert.IsNotNull(boardString);
            TestContext?.WriteLine($"ToString(\"V\") : \n{boardString}");

            boardString = board.ToString("F");
            Assert.IsNotNull(boardString);
            TestContext?.WriteLine($"ToString(\"F\") : {boardString}");
        }

        [TestMethod]
        public void IsEnPassantValidTest()
        {
            Board board = new("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");
            Assert.IsFalse(board.IsEnPassantValid(Color.Black));
        }

        [TestMethod]
        [DataRow("q3r2k/pn2bQpp/1p3n2/8/2P5/2N3P1/PB3P1P/3R1RK1 w - - 3 23", SquareIndex.B2, 0x050005ul)]
        public void GetBishopAttacksTest(string fen, SquareIndex from, ulong attacks)
        {
            Board board = new(fen);
            Bitboard bishAttacks = Board.GetBishopAttacks(from, board.All);
            Assert.AreEqual((Bitboard)attacks, bishAttacks);
        }

        [TestMethod]
        [DataRow("3rr1k1/p5p1/1p2p2p/8/3nB3/2N2bP1/PB5P/2R1R1K1 w - - 4 30", SquareIndex.C1, 0x0004041bul)]
        public void GetRookAttacksTest(string fen, SquareIndex from, ulong attacks)
        {
            Board board = new(fen);
            Bitboard rookAttacks = Board.GetRookAttacks(from, board.All);
            Assert.AreEqual((Bitboard)attacks, rookAttacks);
        }

        [TestMethod]
        public void GenerateMovesTest()
        {
            Board board = new("1k6/1p6/5r2/2pPP3/1n6/2P5/8/2KR4 w - c6 0 1");
            MoveList list = new();
            board.GenerateMoves(list);
            foreach (Move move in list)
            {
                TestContext?.WriteLine(move.ToLongString());
            }
            Assert.AreEqual(17, list.Count);
        }

        [TestMethod]
        public void GenerateEvasionsTest()
        {
            Board board = new("1k6/2p5/1p2N3/p6q/2Q2B2/P1P1p1P1/2P1K3/3r4 w - - 2 51");
            MoveList list = new();
            board.GenerateEvasions(list);
            foreach (Move move in list)
            {
                TestContext?.WriteLine(move.ToLongString());
            }
            Assert.AreEqual(2, list.Count);
        }


        [TestMethod]
        [DataRow("r1bqk2r/pp1n2pp/2n1pp2/3pP3/1b1P1P2/3B1N2/PP4PP/R1BQK1NR w KQkq - 2 10")]
        [DataRow("r1b3k1/pp2b1pB/4p3/3pn3/3q4/P4P2/1PQB3P/2KR3R b - - 0 18")]
        [DataRow("7k/p1r2b2/5q2/1p1p1p1R/5P2/P7/1P2Q2P/1K4R1 b - - 1 32")]
        [DataRow("qr6/4k1p1/2N1pp1p/2P1p3/1b2P2P/5P2/5BPK/1R3Q2 b - - 0 40")]
        [DataRow("rnb1k1nr/pppq1ppp/4p3/3pP3/3P4/P1b5/1PP2PPP/R1BQKBNR w KQkq - 0 6")]
        [DataRow("4q3/k1p5/1p6/p7/2Q1pN2/P1P1B1P1/2P5/3r2K1 w - - 6 47")]
        [DataRow("1k6/2p2q2/1p2N3/p7/2Q1p3/P1P1B1P1/2P2K2/3r4 w - - 10 49")]
        [DataRow("1k6/2p2q2/1p2N3/p7/2Q2B2/P1P1p1P1/2P2K2/3r4 w - - 0 50")]
        [DataRow("1k6/2p5/1p2N3/p6q/2Q2B2/P1P1p1P1/2P1K3/3r4 w - - 2 51")]
        [DataRow("1k6/2Br4/1pQ1N3/p2q4/8/P1P1K1P1/2P5/8 b - - 0 53")]
        [DataRow("4r3/8/1pk5/pN6/2P1K1P1/P3B3/2P5/8 w - - 1 62")]
        [DataRow("4r3/8/1pk5/p5P1/2PN4/P2KB3/2P5/8 b - - 2 64")]
        [DataRow("3r4/1k2N3/1p3B2/p5P1/2P5/P2K4/2P5/8 w - - 11 69")]
        [DataRow("8/1k2N1P1/1p6/p7/2P5/P1K2r2/2P5/8 w - - 1 72")]
        [DataRow("8/1k2N3/1p6/3Q4/p1PK4/r7/2P5/8 b - - 1 74")]
        [DataRow("8/4N3/1k6/1PQ5/p2K4/8/2P5/r7 b - - 2 78")]
        [DataRow("r4rk1/4ppb1/4qN1p/1p1pPR1P/2pP4/2P2Q2/1P4P1/5RK1 b - - 1 34")]
        [DataRow("5r2/5pk1/7p/1p1pRR1P/2pP4/2P5/1P4P1/r5K1 w - - 3 40")]
        [DataRow("r1bqr1k1/2p1Nppp/p2p1n2/4R1B1/p7/8/PPP2PPP/RN1Q2K1 b - - 0 13")]
        [DataRow("r3q1k1/1bp2p1p/p2prp2/8/p4PQR/2N5/PPP2KPP/R7 b - - 4 20")]
        [DataRow("r3q3/1bp1kp1R/p2pQp2/8/p4P2/2N5/PPP2KPP/R7 b - - 0 22")]
        [DataRow("r3q3/1bp2p1R/p2pkp2/8/p4P2/2N5/PPP2KPP/4R3 b - - 1 23")]
        [DataRow("r3k2R/1bp2p2/p2p1p2/8/p4P2/2N5/PPP2KPP/8 b - - 1 25")]
        [DataRow("8/1b3p2/p2p1k2/2pN1p2/2P2P1P/8/PP3KP1/8 b - - 4 31")]
        [DataRow("8/3b4/3pkp2/2pN1p2/1pP2P1P/P1K3P1/8/8 w - - 0 42")]
        [DataRow("1r1q3r/3b1nk1/p2p4/n1pP1p1p/1pP1pP2/1P2N1PP/PQ1N2B1/4RRK1 b - - 2 24")]
        [DataRow("r1b2rk1/p1p3pp/2p5/3pPp2/7q/2Q1BP2/PPP3PP/R3K2R w KQ - 1 14")]
        [DataRow("r5k1/p1p2rpp/2p1b3/q1BpPp2/P2Q4/1P3PP1/2P4P/R3K2R w KQ - 1 19")]
        [DataRow("4r1k1/2pq3p/p1p1bR1p/2BpP3/P4P2/1P6/2PK4/6Q1 b - - 0 31")]
        [DataRow("4rRk1/2pbP2p/p1p5/2Bp1P2/P6p/1P6/2PK4/8 b - - 2 37")]
        [DataRow("4rR2/2pbP1kp/p1p2P2/2Bp4/P6p/1P6/2PK4/8 b - - 0 38")]
        [DataRow("n3rrk1/p7/1p1p3p/P1pP2p1/2P5/2P1RRP1/3Q2PN/1q4K1 w - - 1 30")]
        [DataRow("n4r2/p5k1/3pR2p/p1pP2p1/2P5/2P3P1/4Q1P1/5qK1 w - - 0 35")]
        [DataRow("n7/p5k1/3pR2p/p1pP2p1/2P5/2P3P1/6P1/5rK1 w - - 0 36")]
        [DataRow("8/p3R1k1/1n1p3p/p1pP2p1/2P5/2P3P1/6P1/5K2 b - - 2 37")]
        [DataRow("8/p7/1n1pRk1p/p1pP2p1/2P5/2P3P1/6P1/5K2 b - - 4 38")]
        [DataRow("6R1/8/p7/1n3kp1/2p3P1/2P2K2/6P1/8 b - - 0 50")]
        [DataRow("8/8/p1R2k2/1n4p1/6P1/5K2/6P1/8 b - - 2 53")]
        [DataRow("8/8/p2n4/2R1k1p1/6P1/4K3/6P1/8 b - - 6 55")]
        public void GenerateEvasions2Test(string fen)
        {
            Board board = new Board(fen);
            MoveList list1 = new();
            MoveList list2 = new();
            HashSet<Move> set1 = new();
            HashSet<Move> set2 = new();
            board.PushBoardState();
            board.GenerateMoves(list1);
            foreach (Move move in list1)
            {
                if (board.MakeMoveNs(move))
                {
                    set1.Add(move);
                    board.UnmakeMoveNs();
                }
            }

            board.GenerateEvasions(list2);
            foreach (Move move in list2)
            {
                if (board.MakeMoveNs(move))
                {
                    set2.Add(move);
                    board.UnmakeMoveNs();
                }
            }
            board.PopBoardState();
            Assert.AreEqual(set1.Count, set2.Count);
            Assert.IsTrue(set1.SetEquals(set2));
        }

        [TestMethod]
        [DataRow(Constants.FEN_START_POS, 20)]
        [DataRow("3kn3/4p2r/3p4/5NQP/7P/6P1/8/7K w - - 0 1", 22)]
        public void MovesTest(string fen, int expected)
        {
            Board board = new Board(fen);
            MoveList list = new();
            SearchStack ss = new();
            History history = new(ss);
            int count = 0;
            foreach (var mv in board.Moves(0, history, ss, list, Move.NullMove))
            {
                TestContext?.WriteLine($"{mv.MovePhase}: {mv.Move.ToLongString()}");
                ++count;
            }
            Assert.AreEqual(expected, count);
        }

        [TestMethod]
        [DataRow(Constants.FEN_START_POS, 0)]
        [DataRow("3kn3/4p2r/3p4/5NQP/7P/6P1/8/7K w - - 0 1", 3)]
        public void QMovesTest(string fen, int expected)
        {
            Board board = new Board(fen);
            MoveList list = new();
            SearchStack ss = new();
            History history = new(ss);
            int count = 0;
            foreach (var mv in board.QMoves(0, 0, history, ss, list, Move.NullMove))
            {
                TestContext?.WriteLine($"{mv.MovePhase}: {mv.Move.ToLongString()}");
                ++count;
            }
            Assert.AreEqual(expected, count);
        }

        [TestMethod]
        [DataRow("4k3/1pp1q1pp/2n5/4p2R/3B1b2/2QP1N2/1P2PP2/4K3 w - - 0 1", Color.White, 100)]
        [DataRow("8/1pp1q1pp/2n2k2/4p2R/3B1b2/2QP1N2/1P2PP2/4K3 w - - 0 1", Color.White, -200)]
        public void See0Test(string fen, Color stm, int expected)
        {
            Board bd = new(fen);
            Move move = new (stm, Piece.Bishop, SquareIndex.D4, SquareIndex.E5, MoveType.Capture, Piece.Pawn);

            int see0Eval = bd.See0(move);
            //Assert.AreEqual(expected, seeEval);

            bd.MakeMove(move);
            int see1Eval = bd.See1(move);
            Assert.AreEqual(move.Capture.Value() - see0Eval, see1Eval);
        }

        [TestMethod]
        [DataRow("8/1pp1q1pp/2n2k2/4p2R/3B1b2/2QP1N2/1P2PP2/4K1r1 w - - 0 1", false)]
        [DataRow("8/1pp1q1pp/2n2k2/4p2R/3B4/2QP1N2/1P2PP1b/4K1r1 w - - 0 1", false)]
        [DataRow("8/1pp1q1pp/2n2k2/4p2R/3B4/2QP4/1P1NPP1b/4K1rR w - - 0 1", true)]
        public void See1Test(string fen, bool safe)
        {
            Board bd = new(fen);
            Move move = new (Color.Black, Piece.Rook, SquareIndex.G6, SquareIndex.G1);
            int seeEval = bd.See1(move);
            if (safe)
            {
                Assert.IsTrue(seeEval <= 0);
            }
            else
            {
                Assert.IsTrue(seeEval > 0);
            }
        }

        [TestMethod]
        public void StartPosTest()
        {
            Board board = new Board(Constants.FEN_START_POS);
            Board board1 = new Board();
            board1.StartPos();

            Assert.AreEqual(board.Hash, board1.Hash);
            Assert.AreEqual(board.SideToMove, board1.SideToMove);
            Assert.AreEqual(board.FullMoveCounter, board1.FullMoveCounter);
        }

        [TestMethod]
        [DataRow(0xC2123BCE03ED1E94ul, "r3kb1r/1p1n1ppp/2q5/p1pNp1B1/1nPpP3/3P1NP1/PP3P1P/R2QK1R1 b Qkq - 0 12")]
        [DataRow(0xE6EF29A4704F0D46ul, "2r1k2r/p1p2p2/1p2q3/1P1ppn2/6Pp/B1P2P1R/P1P5/1K1RQ3 b k - 0 21")]
        public void ZobristHashTest(ulong hash, string fen)
        {
            Board board = new(fen);
            Assert.AreEqual(hash, board.Hash);

            PedanticFormat pdata = board.ToBinary(512, 10, Result.Draw);
            board.LoadBinary(ref pdata);

            Assert.AreEqual(hash, board.Hash);
        }

        [TestMethod]
        public void ZobristBugTest()
        {
            Board bd1 = new("r3kb1r/1p1q1ppp/nn6/3pP3/pP1N4/P2Q1P2/4N1PP/R3K2R w KQkq - 0 20");
            Assert.AreEqual(0xBF85F4AF5967FCC2ul, bd1.Hash); // << incorrect which means that board isn't being restored by search
            
            string moves = "f2f3, a7a5, d2d4, g8f6, a2a3, b8a6, c1e3, f6g8, b1d2, g8f6, e3f2, c7c5, e2e4, c5d4, f2d4, d7d5, f1b5, c8d7, b5d7, d8d7, e4e5, f6g8, c2c4, g8h6, g1e2, h6f5, d4b6, e7e6, c4d5, e6d5, d2b3, a5a4, b3d4, f5e3, d1d3, e3c4, b2b4, c4b6";
            string[] moveList = moves.Split(',', StringSplitOptions.TrimEntries);
            Board bd2 = new();
            bd2.StartPos();
            foreach (string move in moveList)
            {
                if (Move.TryParse(bd2, move, out Move mv))
                {
                    bd2.MakeMove(mv);
                }
                else
                {
                    Assert.Fail($"Could not make move {move}");
                    break;
                }
            }
            Assert.AreEqual(bd1.Hash, bd2.Hash);

            PedanticFormat pdata = bd2.ToBinary(512, 0, Result.Draw);
            bd1.LoadBinary(ref pdata);

            Assert.AreEqual(bd2.Hash, bd1.Hash);
        }
    }
}
