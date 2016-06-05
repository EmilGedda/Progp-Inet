using System;

namespace InetServer
{
    /// <summary>
    ///     A singleton Logger responsible for all logging and printing
    /// </summary>
    public class Logger
    {
        // These two below is the main singleton stuff, creating only one instance for the duration of the program
        private static Logger instance;
        private readonly DateTime start = DateTime.Now;

        /// <summary>
        ///     Private constructor to employ the singleton design pattern
        /// </summary>
        private Logger()
        {
        }

        private static Logger Instance => instance ?? (instance = new Logger());
        public static void Critical(string str) => Instance.log(str, Status.Critical);
        public static void Error(string str) => Instance.log(str, Status.Error);

        public static void Info(string str) => Instance.log(str, Status.Info);
        public static void Warning(string str) => Instance.log(str, Status.Warning);
        public static void Watcher(string str) => Instance.log(str, Status.Watcher);

        /// <summary>
        ///     Log the given string and status-level. Provides a timestamp aswell, the timestamp is the time since the server
        ///     start.
        /// </summary>
        /// <param name="str">The string to print</param>
        /// <param name="status">The status level the string has.</param>
        private void log(string str, Status status)
        {
            var span = DateTime.Now - start;
            Console.WriteLine($"[{span.TotalSeconds:0.00}] {status.ToString().ToUpper()}: {str}");
        }

        /// <summary>
        ///     The different status-levels provided by the class.
        ///     This is not exposed outwards, but only as methods such as "Info", "Error" et cetera
        /// </summary>
        protected enum Status
        {
            Info,
            Error,
            Warning,
            Critical,
            Watcher
        }
    }
}
