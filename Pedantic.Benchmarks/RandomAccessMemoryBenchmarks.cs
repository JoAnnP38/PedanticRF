using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Pedantic.Benchmarks
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public unsafe class RandomAccessMemoryBenchmarks
    {
        public const int MB_SIZE = 1024 * 1024;
        public const int ITERATIONS = 1000000;
        public const int CACHE_MB = 256;
        public const int MEM_ALIGN = 64;

        public struct Item
        {
            public ulong slot;
            public ulong data;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            capacity = (CACHE_MB * MB_SIZE) / sizeof(Item);

            for (int n = 0; n < ITERATIONS; n++)
            {
                data[n].slot = (ulong)Random.Shared.NextInt64(capacity);
                data[n].data = (ulong)Random.Shared.NextInt64();
            }

            array = new Item[capacity];
            fixedArray = GC.AllocateArray<Item>(capacity, true);
            pNativeArray = (Item*)NativeMemory.Alloc((nuint)capacity, (nuint)sizeof(Item));
            pAlignedArray = (Item*)NativeMemory.AlignedAlloc((nuint)(capacity * sizeof(Item)), MEM_ALIGN);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            NativeMemory.Free(pNativeArray);
            NativeMemory.AlignedFree(pAlignedArray);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Array Write")]
        public void ArrayWrite()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                array[slot] = data[n]; // write
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void PinnedArrayWrite()
        {
            fixed (Item* pinned = &array[0])
            {
                for (int n = 0; n < ITERATIONS; n++)
                {
                    int slot = (int)data[n].slot;
                    pinned[slot] = data[n]; // write
                }
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void FixedArrayWrite()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                fixedArray[slot] = data[n]; // write
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void PinnedFixedArrayWrite()
        {
            fixed (Item* pinned = &fixedArray[0])
            {
                for (int n = 0; n < ITERATIONS; n++)
                {
                    int slot = (int)data[n].slot;
                    pinned[slot] = data[n]; // write
                }
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void NativeArrayWrite()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                pNativeArray[slot] = data[n]; // write
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void AlignedArrayWrite()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                pAlignedArray[slot] = data[n]; // write
            }
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Array Read")]
        public ulong ArrayRead()
        {
            ulong sum = 0;
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                Item item = array[slot]; // read
                sum += item.data;
            }
            return sum;
        }

        [Benchmark, BenchmarkCategory("Array Read")]
        public ulong PinnedArrayRead()
        {
            ulong sum = 0;

            fixed (Item* pinned = &array[0])
            {
                for (int n = 0; n < ITERATIONS; n++)
                {
                    int slot = (int)data[n].slot;
                    Item item = pinned[slot]; // read
                    sum += item.data;
                }
            }
            return sum;
        }

        [Benchmark, BenchmarkCategory("Array Read")]
        public ulong FixedArrayRead()
        {
            ulong sum = 0;
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                Item item = fixedArray[slot]; // read
                sum += item.data;
            }
            return sum;
        }

        [Benchmark, BenchmarkCategory("Array Read")]
        public ulong PinnedFixedArrayRead()
        {
            ulong sum = 0;

            fixed (Item* pinned = &fixedArray[0])
            {
                for (int n = 0; n < ITERATIONS; n++)
                {
                    int slot = (int)data[n].slot;
                    Item item = pinned[slot]; // read
                    sum += item.data;
                }
            }
            return sum;
        }

        [Benchmark, BenchmarkCategory("Array Read")]
        public ulong NativeArrayRead()
        {
            ulong sum = 0;
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                Item item = pNativeArray[slot]; // read
                sum += item.data;
            }
            return sum;
        }


        [Benchmark, BenchmarkCategory("Array Read")]
        public ulong AlignedArrayRead()
        {
            ulong sum = 0;
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)data[n].slot;
                Item item = pAlignedArray[slot]; // read
                sum += item.data;
            }
            return sum;
        }

        Item[] array = Array.Empty<Item>();
        Item[] fixedArray = Array.Empty<Item>();
        Item* pNativeArray = null;
        Item* pAlignedArray = null;

        Item[] data = new Item[ITERATIONS];
        int capacity;
    }
}
