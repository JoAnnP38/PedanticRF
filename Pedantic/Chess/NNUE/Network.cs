using System.Runtime.CompilerServices;

namespace Pedantic.Chess.NNUE
{
    public class Network : IInitialize
    {
        // smol-net™ 768 -> 128 x 2 -> 1
        public const int INPUT_SIZE = MAX_COLORS * MAX_PIECES * MAX_SQUARES;
        public const int HIDDEN_SIZE = 128; 

        private short[] hiddenWeights = new short[INPUT_SIZE * HIDDEN_SIZE];
        private short[] hiddenBiases = new short[HIDDEN_SIZE];
        private short[] outputWeights = new short[HIDDEN_SIZE * 2];
        private short outputBias;

        private static Network defaultNetwork;

        static Network()
        {
            // default network embedded as a resource
            defaultNetwork = new Network(Resource.NN128HL_20240818);
        }

        public Network(byte[] networkBytes)
        {
            using (MemoryStream ms = new MemoryStream(networkBytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    ReadNetwork(reader);
                }
            }
        }

        public Network(string networkFilePath)
        {
            using (FileStream fs = new FileStream(networkFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    ReadNetwork(reader);
                }
            }
        }

        public ReadOnlySpan<short> HiddenWeights
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<short>(hiddenWeights);
            }
        }

        public ReadOnlySpan<short> HiddenBiases
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<short>(hiddenBiases);
            }
        }

        public ReadOnlySpan<short> OutputWeights
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<short>(outputWeights);
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
            for (int n = 0; n < hiddenWeights.Length; n++)
            {
                hiddenWeights[n] = reader.ReadInt16();
            }

            for (int n = 0; n < hiddenBiases.Length; n++)
            {
                hiddenBiases[n] = reader.ReadInt16();
            }

            for (int n = 0; n < outputWeights.Length; n++)
            {
                outputWeights[n] = reader.ReadInt16();
            }

            outputBias = reader.ReadInt16();
        }
    }
}
