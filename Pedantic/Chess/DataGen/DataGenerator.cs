namespace Pedantic.Chess.DataGen
{
    using System.Collections.Concurrent;

    using Pedantic.Utilities;

    public class DataGenerator : IDisposable
    {
        public DataGenerator(string dataFilePath, int concurrency)
        {
            Util.Assert(!string.IsNullOrWhiteSpace(dataFilePath));
            Util.Assert(concurrency > 0 && concurrency < Environment.ProcessorCount);

            ConcurrentQueue<PedanticFormat> queue = new ConcurrentQueue<PedanticFormat>();
            dataQ = new(queue, concurrency * 512);
            writer = new BinaryWriter(System.IO.File.Open(dataFilePath, FileMode.CreateNew, FileAccess.Write));

            dataWriter = new(writer, dataQ);

            for (int n = 0; n < concurrency; n++)
            {
                DataGenThread thread = new(dataQ, cancelSource);
                threads.Add(thread);
            }
        }

        public void Start()
        {
            dataWriter.Start();

            for (int n = 0; n < threads.Count; n++)
            {
                threads[n].Generate();
            }
        }

        public void Stop()
        {
            cancelSource.Cancel();
            for (int n = 0; n < threads.Count; n++)
            {
                threads[n].Join();
            }
            dataQ.CompleteAdding();
            dataWriter.Stop();
            Dispose();

        }

        public int PositionCount
        {
            get
            {
                int count = 0;
                
                for (int n = 0; n < threads.Count; n++)
                {
                    count += threads[n].PositionCount;
                }

                return count;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    writer.Flush();
                    writer.Dispose();
                    cancelSource.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private readonly CancellationTokenSource cancelSource = new();
        private readonly BinaryWriter writer;
        private readonly DataGenWriter dataWriter;
        private readonly List<DataGenThread> threads = new();
        private readonly BlockingCollection<PedanticFormat> dataQ;
        private bool disposedValue;
    }
}
