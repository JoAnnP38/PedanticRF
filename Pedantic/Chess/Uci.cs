using System.Runtime.CompilerServices;

namespace Pedantic.Chess
{
    public class Uci
    {
        static Uci()
        {
            defaultUci = new Uci(true, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Uci(bool enable = true, bool debug = false)
        {
            this.enable = enable;
            this.debug = debug;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(string message)
        {
            if (!enable)
            {
                return;
            }
            Console.Out.WriteLineAsync($"info string {message}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug(string message)
        {
            if (!enable)
            {
                return;
            }
            if (debug)
            {
                Console.Out.WriteLineAsync($"info string {message}");
            }
        }

        public static Uci Default => defaultUci;

        private readonly bool enable;
        private readonly bool debug;
        private readonly static Uci defaultUci;
    }
}
