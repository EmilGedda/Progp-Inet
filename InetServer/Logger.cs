using System;
// ReSharper disable InconsistentNaming

namespace InetServer
{
    public class Logger
    {
        private readonly DateTime start = DateTime.Now;
        private static Logger instance;
        private static Logger Instance => instance ?? (instance = new Logger());

        private Logger() { }

        private void log(string str, Status status)
        {
            TimeSpan span = DateTime.Now - start;
            Console.WriteLine($"[{span.TotalSeconds:0.00}] {status}: {str}");
        }

        public static void Info(string str) => Instance.log(str, Status.INFO);
        public static void Error(string str) => Instance.log(str, Status.ERROR);
        public static void Critical(string str) => Instance.log(str, Status.CRITICAL);
        public static void Watcher(string str) => Instance.log(str, Status.WATCHER);
        public static void Warning(string str) => Instance.log(str, Status.WARNING);


        protected enum Status
        {

            INFO,
            ERROR,
            WARNING,
            CRITICAL,
            WATCHER
        }
    }
}