using System.Collections.Concurrent;

namespace Pedantic.Chess.DataGen
{
    public class DataGenWriter
    {
        public DataGenWriter(BinaryWriter writer, BlockingCollection<PedanticFormat> dataQ)
        {
            this.dataQ = dataQ;
            this.writer = writer;
        }

        public void Start()
        {
            thread = new Thread(new ThreadStart(WriteData));
            thread.Start();
        }

        public void Stop()
        {
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }
        }

        private void WriteData()
        {
            for (;;)
            {
                try
                {
                    PedanticFormat pdata = dataQ.Take();
                    PedanticFormat.Store(writer, ref pdata);
                }
                catch (InvalidOperationException)
                {
                    // No more data to be written, exit thread
                    break;
                }
            }
        }

        private Thread? thread = null;
        private readonly BlockingCollection<PedanticFormat> dataQ;
        private readonly BinaryWriter writer;
    }
}
