using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
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
        private static ManualResetEvent mre = new ManualResetEvent(false);
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
                    {MessageType.Status, OnStatus}
                    
                }).OnRequest;
                client.ListenAsync();
                Console.ReadKey(true);
                LogIn();
                client.Send(new LanguagesAvailable());
            }
            catch (SocketException se) {
                Console.WriteLine($"\n[EXCEPTION] {se.Message}");
            }
            
            Console.ReadKey();
        }

        private static void LogIn()
        {
            short pinInt = 0;
            int cardInt = 0;
            bool correctInput = false;
            while (!correctInput)
            {
                while (!correctInput)
                {
                    Console.Write("Please enter your card number:");
                    string cardnumber = Console.ReadLine();
                    correctInput = Int32.TryParse(cardnumber, out cardInt);
                }

                correctInput = false;
                while (!correctInput)
                {
                    Console.Write("Please enter your PIN code:");
                    string pin = Console.ReadLine();
                    correctInput = Int16.TryParse(pin, out pinInt);
                }

                var loginMsg = new Login(cardInt, pinInt);
                client.SendAsync(loginMsg);
            }
        }

        private static StatusCode OnStatus(Client c, IMessage message)
        {
            var msg = (Status) message;
            return msg.Code;
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
