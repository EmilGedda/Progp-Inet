using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using InetServer;
using InetServer.i18n;
using InetServer.Message;

namespace InetClient
{
    class Program
    {
        private static InetServer.Client client;
        private static BlockingCollection<StatusCode> bc = new BlockingCollection<StatusCode>();
        private static ConcurrentBag<Language> languages = new ConcurrentBag<Language>();

        // ReSharper disable once UseObjectOrCollectionInitializer
        private static void Main(string[] args)
        {
            try {
                Console.Clear();
                Console.WriteLine("Connecting to server...");
                TcpClient t = new TcpClient();
                t.ReceiveBufferSize *= 8;
                t.Connect(IPAddress.Loopback, 420);
                Console.WriteLine("Connection established to server.");
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

                correctInput = false;
                var loginMsg = new Login(cardInt, pinInt);
                client.SendAsync(loginMsg);
                var status = bc.Take();
                if (status == StatusCode.LoginSuccess)
                    correctInput = true;
            }
            Console.WriteLine("DET FUNKADE DESU");
        }

        private static StatusCode OnStatus(Client c, IMessage message)
        {
            var msg = (Status) message;
            if (msg.Code != StatusCode.LoginSuccess)
            {
                bc.Add(StatusCode.Fail);
                return StatusCode.Fail;
            }
            bc.Add(msg.Code);
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
