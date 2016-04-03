using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InetServer
{
    internal class Program
    {
        private static void Main()
        {
            using (var s = new Server())
            {
                Console.CancelKeyPress += (sender, args) => s?.Dispose();
                s.Listen();
                Console.ReadKey(true);
            }
            Console.ReadKey(true);
        }
    }
}
