using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InetServer;
using InetServer.i18n;
using InetServer.Message;

namespace InetClient
{
    class Program
    {
        private static InetServer.Client client;
        private static ConcurrentBag<Language> languages = new ConcurrentBag<Language>();

        // ReSharper disable once UseObjectOrCollectionInitializer
        private static void Main(string[] args)
        {
            try {
                Console.Clear();
                Console.Write("Connecting to server...");
                TcpClient t = new TcpClient();
                t.ReceiveBufferSize *= 8;
                t.Connect(IPAddress.Loopback, 420);
                client = new Client(t, false)
                {
                    Acc = new Account()
                };
                client.Request += new MessageTranslator(new Dictionary<MessageType, MessageTranslator.CommandEventHandler>
                {
                    {MessageType.Language, OnLang},
                    {MessageType.LanguagesAvailable, OnLangsAvailable},
                    {MessageType.Status, (client1, comm) => StatusCode.Success}
                    
                }).OnRequest;
                client.Send(new LanguagesAvailable());
                client.ListenAsync();
                Console.WriteLine("Finished reading langs");
                Console.ReadKey(true);
                Console.Clear();
                Console.Write("Enter cardnumber: ");
                string cn = Console.ReadLine();
                Console.Write("Enter PIN: ");
                string pin = Console.ReadLine();
            } catch (SocketException se) {
                Console.WriteLine($"\n[EXCEPTION] {se.Message}");
            }
            
            Console.ReadKey();
        }

        private static StatusCode OnLangsAvailable(Client c, IMessage message)
        {
            languages = new ConcurrentBag<Language>();
            return StatusCode.Success;
        }

        private static StatusCode OnLang(Client c, IMessage message)
        {
            var lang = (Language) message;
            languages.Add(lang);
            return StatusCode.Success;
        }
    }
}
