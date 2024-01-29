using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Pedantic.Benchmarks
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public unsafe class RandomAccessMemoryBenchmarks
    {
        public const int MB_SIZE = 1024 * 1024;
        public const int ITERATIONS = 10000;
        public const int CACHE_MB = 256;
        public const int MEM_ALIGN = 64;

        public struct Item
        {
            public ulong key;
            public ulong data;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                data[n].key = (ulong)Random.Shared.NextInt64(long.MinValue, long.MaxValue);
                data[n].data = (ulong)Random.Shared.NextInt64();
            }

            capacity = (CACHE_MB * MB_SIZE) / sizeof(Item);
            mask = (ulong)(capacity - 1);
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
                int slot = (int)(data[n].key & mask);
                array[slot] = data[n];
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void PinnedArrayWrite()
        {
            fixed (Item* pinned = &array[0])
            {
                for (int n = 0; n < ITERATIONS; n++)
                {
                    int slot = (int)(data[n].key & mask);
                    pinned[slot] = data[n];
                }
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void FixedArrayWrite()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)(data[n].key & mask);
                fixedArray[slot] = data[n];
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void PinnedFixedArrayWrite()
        {
            fixed (Item* pinned = &fixedArray[0])
            {
                for (int n = 0; n < ITERATIONS; n++)
                {
                    int slot = (int)(data[n].key & mask);
                    pinned[slot] = data[n];
                }
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void NativeArrayWrite()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)(data[n].key & mask);
                pNativeArray[slot] = data[n];
            }
        }

        [Benchmark, BenchmarkCategory("Array Write")]
        public void AlignedArrayWrite()
        {
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)(data[n].key & mask);
                pAlignedArray[slot] = data[n];
            }
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Array Read")]
        public ulong ArrayRead()
        {
            ulong sum = 0;
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)(data[n].key & mask);
                sum += array[slot].data & 0x0ff;
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
                    int slot = (int)(data[n].key & mask);
                    sum += pinned[slot].data & 0x0ff;
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
                int slot = (int)(data[n].key & mask);
                sum += fixedArray[slot].data & 0x0ff;
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
                    int slot = (int)(data[n].key & mask);
                    sum += pinned[slot].data & 0x0ff;
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
                int slot = (int)(data[n].key & mask);
                sum += pNativeArray[slot].data & 0x0ff;
            }
            return sum;
        }


        [Benchmark, BenchmarkCategory("Array Read")]
        public ulong AlignedArrayRead()
        {
            ulong sum = 0;
            for (int n = 0; n < ITERATIONS; n++)
            {
                int slot = (int)(data[n].key & mask);
                sum += pAlignedArray[slot].data & 0x0ff;
            }
            return sum;
        }

        Item[] array = Array.Empty<Item>();
        Item[] fixedArray = Array.Empty<Item>();
        Item* pNativeArray = null;
        Item* pAlignedArray = null;

        Item[] data = new Item[ITERATIONS];
        int capacity;
        ulong mask;
    }
}
