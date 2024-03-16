
using System.Runtime.InteropServices;

namespace Pedantic.UnitTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public unsafe class MemTests
    {
        [TestMethod]
        public void MemClearTest()
        {
            short[][] test = Mem.Allocate2D<short>(1024, 1024);
            for (int n = 0; n < 1024; n++)
            {
                for (int m = 0; m < 1024; m++)
                {
                    test[n][m] = (short)Random.Shared.Next(short.MinValue, short.MaxValue);
                }
            }

            Mem.Clear(test);

            for (int n = 0; n < 1024; n++)
            {
                for (int m = 0; m < 1024; m++)
                {
                    if (test[n][m] != 0)
                    {
                        Assert.Fail();
                    }
                }
            }
            
        }

        [TestMethod]
        public void TtMemClearTest()
        {
            long* pTable = null;
            nuint byteCount = 1024 * 1024 * 128;
            pTable = (long*)NativeMemory.AlignedRealloc(pTable, byteCount, 64);

            for (int n = 0; n < (int)byteCount / 8; n++)
            {
                pTable[n] = Random.Shared.NextInt64(long.MinValue, long.MaxValue);
            }

            NativeMemory.Clear(pTable, byteCount);

            for (int n = 0; n < (int)byteCount / 8; n++)
            {
                if (pTable[n] != 0)
                {
                    Assert.Fail();
                }
            }
        }
    }
}
