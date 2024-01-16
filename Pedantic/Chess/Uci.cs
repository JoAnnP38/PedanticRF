using System.Text;
using Pedantic.Utilities;

namespace Pedantic.Chess
{
    public class Uci
    {
        static Uci()
        {
            defaultUci = new Uci(true, false);
        }

        public Uci(bool enable = true, bool debug = false)
        {
            this.enable = enable;
            this.debug = debug;
        }

        public void Log(string message)
        {
            if (!enable)
            {
                return;
            }
            Console.Out.WriteLineAsync($"info string {message}");
        }

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
