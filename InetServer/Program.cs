using System;
using System.Collections.Generic;
using System.Linq;
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
                s.Listen();
                Console.ReadKey();
            }
        }
    }
}
