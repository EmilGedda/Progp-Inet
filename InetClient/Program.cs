using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using InetServer.i18n;
using InetServer.Message;

namespace InetClient
{
    class Program
    {
        private static TcpClient client = new TcpClient();
        private static List<Language> languages = new List<Language>();

        private static void Main(string[] args)
        {
            try {
                Console.Clear();
                Console.Write("Connecting to server...");
                client.Connect(IPAddress.Loopback, 420);
                Send(new LanguagesAvailable(5));
                Console.Clear();
                Console.Write("Enter cardnumber: ");
                string cn = Console.ReadLine();
                Console.Write("Enter PIN: ");
                string pin = Console.ReadLine();
            } catch (SocketException se) {
                Console.WriteLine($"\n [EXCEPTION] {se.Message}");
            }

            client.Close();
            Console.ReadKey();
        }
        private static void Send(IMessage cmd)
        {
            var payload = cmd.Destruct();
            client.GetStream().Write(payload, 0, payload.Length);
        }
    }
}
