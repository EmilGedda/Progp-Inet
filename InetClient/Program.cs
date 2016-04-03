using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InetClient
{
    class Program
    {
        private static Menu currentMenu = new LoginMenu();
        private static TcpClient client = new TcpClient();
        static void Main(string[] args)
        {
            try {
                Console.WriteLine("Starting client...");
                client.Connect(IPAddress.Loopback, 420);
                Console.Clear();
                currentMenu.Print();
            } catch (SocketException se) {
                Console.WriteLine($"[EXCEPTION] {se.Message}");
            }
            client.Close();
            Console.ReadKey();
        }
    }
}
