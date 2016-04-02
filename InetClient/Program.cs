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
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            Console.WriteLine("Starting client...");
            client.Connect(IPAddress.Loopback, 420);
            Console.WriteLine("Connected to host");
            NetworkStream stream = client.GetStream();
            Console.ReadKey();
            Console.WriteLine("Sending");
            stream.Write(new byte[] {200, 1, 2, 3}, 0, 4);
            stream.Flush();
            Console.ReadKey();
            client.Close();
        }
    }
}
