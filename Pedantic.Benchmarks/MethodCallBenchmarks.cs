using BenchmarkDotNet.Attributes;

namespace Pedantic.Benchmarks
{
    public unsafe class MethodCallBenchmarks
    {
        public interface IMethod
        {
            public int Method();
        }

        public class Base
        {
            public virtual int Method()
            {
                return Random.Shared.Next(1, 7);
            }
        }

        public class Derived : Base
        {
            public override int Method()
            {
                return Random.Shared.Next(1, 7);
            }
        }

        public class MethodInterface : IMethod
        {
            public int Method()
            {
                return Random.Shared.Next(1, 7);
            }
        }

        public delegate int MethodDelegate();

        public delegate*<int> funcPtrMethod;

        public Base baseMethod = new Derived();
        public IMethod interfaceMethod = new MethodInterface();
        public MethodDelegate method;

        public int Method()
        {
            return Random.Shared.Next(1, 7);
        }

        public static int StaticMethod()
        {
            return Random.Shared.Next(1, 7);
        }

        public MethodCallBenchmarks()
        {
            method = new(Method);
            funcPtrMethod = &StaticMethod;
        }

        [Benchmark]
        public int MethodCallVirtual()
        {
            int x = 0;
            for (int n = 0; n < 1000; n++)
            {
                x += baseMethod.Method();
            }
            return x;
        }

        [Benchmark]
        public int MethodCallInterface()
        {
            int x = 0;
            for (int n = 0; n < 1000; n++)
            {
                x += interfaceMethod.Method();
            }
            return x;
        }

        [Benchmark]
        public int MethodCallDelegate()
        {
            int x = 0;
            for (int n = 0; n < 1000; n++)
            {
                x +=  method();
            }
            return x;
        }

        [Benchmark]
        public int MethodCallFuncPtr()
        {
            int x = 0;
            for (int n = 0; n < 1000; n++)
            {
                x += funcPtrMethod();
            }
            return x;
        }

        [Benchmark]
        public int MethodCallDirect()
        {
            int x = 0;
            for (int n = 0; n < 1000; n++)
            {
                x += Method();
            }
            return x;
        }
    }
}
