using System;
using InetServer.Network;

namespace InetServer
{
    internal class Program
    {
        // ReSharper disable once AccessToDisposedClosure
        /// <summary>
        ///     The entrypoint of the program, spins up a server a server and waits for Ctrl+C to exit.
        /// </summary>
        private static void Main()
        {
            using (var s = new Server())
            {
                Console.CancelKeyPress += (sender, args) => s.Dispose();
                s.Listen();
                Console.ReadKey(true);
            }
            Console.ReadKey(true);
        }
    }
}
