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

        private static void Main(string[] args)
        {
            try {
                Console.Clear();
                Console.Write("Connecting to server...");
                client.Connect(IPAddress.Loopback, 420);
                Console.Clear();
                Console.Write("Enter cardnumber: ");
                string cn = Console.ReadLine();
                Console.Write("Enter PIN: ");
                string pin = Console.ReadLine();
                currentMenu.Print();
            } catch (SocketException se) {
                Console.WriteLine($"\n [EXCEPTION] {se.Message}");
            }
            client.Close();
            Console.ReadKey();
        }
    }
}
