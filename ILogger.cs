using System;
using System.Collections.Generic;
using System.Linq;

namespace NetEnvSwitcher
{
    public interface ILogger
    {
        void WriteLine(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }

    static class LoggerExtensions
    {
        public static void WriteLine(this ILogger me, string format, params object[] args)
        {
            me.WriteLine(String.Format(format, args));
        }
    }
}
