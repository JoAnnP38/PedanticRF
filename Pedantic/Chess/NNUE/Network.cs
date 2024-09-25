using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pedantic.Chess.NNUE
{
    public unsafe class Network : IInitialize, IDisposable
    {
        // smol-net™ (768 -> 384) x 2 -> 1
        public const int INPUT_SIZE = MAX_COLORS * MAX_PIECES * MAX_SQUARES;
        public const int HIDDEN_SIZE = 384;

        private short* featureWeights;  // = new short[INPUT_SIZE * HIDDEN_SIZE];
        private short* featureBiases;   // = new short[HIDDEN_SIZE];
        private short* outputWeights;   // = new short[HIDDEN_SIZE * 2];
        private short outputBias;

        private static Network defaultNetwork;
        private bool disposedValue;

        static Network()
        {
            // default network embedded as a resource
            defaultNetwork = new Network(Resource.NN384HL_20240925);
        }

        protected Network()
        {
            featureWeights = AlignedAlloc<short>(INPUT_SIZE * HIDDEN_SIZE);
            featureBiases = AlignedAlloc<short>(HIDDEN_SIZE);
            outputWeights = AlignedAlloc<short>(HIDDEN_SIZE * 2);
        }

        public Network(byte[] networkBytes) : this()
        {
            using (MemoryStream ms = new MemoryStream(networkBytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    ReadNetwork(reader);
                }
            }
        }

        public Network(string networkFilePath) : this()
        {
            using (FileStream fs = new FileStream(networkFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    ReadNetwork(reader);
                }
            }
        }

        public ReadOnlySpan<short> FeatureWeights
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<short>(featureWeights, INPUT_SIZE * HIDDEN_SIZE);
            }
        }

        public ReadOnlySpan<short> FeatureBiases
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<short>(featureBiases, HIDDEN_SIZE);
            }
        }

        public ReadOnlySpan<short> OutputWeights
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<short>(outputWeights, HIDDEN_SIZE * 2);
            }
        }

        public short OutputBias
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => outputBias;
        }

        public static Network Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => defaultNetwork;
        }

        public static void Initialize() { }

        private void ReadNetwork(BinaryReader reader)
        {
            for (int n = 0; n < FeatureWeights.Length; n++)
            {
                featureWeights[n] = reader.ReadInt16();
            }

            for (int n = 0; n < FeatureBiases.Length; n++)
            {
                featureBiases[n] = reader.ReadInt16();
            }

            for (int n = 0; n < OutputWeights.Length; n++)
            {
                outputWeights[n] = reader.ReadInt16();
            }

            outputBias = reader.ReadInt16();
        }

        public static T* AlignedAlloc<T>(nuint itemCount)
        {
            nuint byteCount = (nuint)sizeof(T) * itemCount;
            void* ptr = NativeMemory.AlignedAlloc(byteCount, 64);

            if (ptr == null)
            {
                throw new OutOfMemoryException();
            }

            NativeMemory.Clear(ptr, byteCount);
            return (T*)ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlignedFree(void* ptr)
        {
            NativeMemory.AlignedFree(ptr);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                AlignedFree(featureWeights);
                AlignedFree(featureBiases);
                AlignedFree(outputWeights);
                featureWeights = null;
                featureBiases = null;
                outputWeights = null;
            }
        }

        ~Network()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
