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
        private static ConcurrentDictionary<string, Language> languages = new ConcurrentDictionary<string, Language>();
        private static Language currentLang;
        private const string DefaultLang = "en-US";

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
                client.Send(new LanguagesAvailable());
                LogIn();
                SetLanguage(DefaultLang);
                PrintMenu();
            }
            catch (SocketException se) {
                Console.WriteLine($"\n[EXCEPTION] {se.Message}");
            }
            
            Console.ReadKey();
        }

        private static void SetLanguage(string code) => currentLang = languages[code];

        private static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("1. " + currentLang[Language.Label.Withdraw]);
            Console.WriteLine("2. " + currentLang[Language.Label.Deposit]);
            Console.WriteLine("3. " + currentLang[Language.Label.Exit]);
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
                client.Send(loginMsg);
                var status = bc.Take();
                if (status == StatusCode.LoginSuccess)
                    correctInput = true;
            }
            Console.WriteLine("Insert menu here desu.");
        }

        private static StatusCode OnStatus(Client c, IMessage message)
        {
            var msg = (Status) message;
            if (msg.Code == StatusCode.LoginSuccess | msg.Code == StatusCode.LoginFail)
                bc.Add(msg.Code);
            return StatusCode.Acknowledge;
        }

        private static StatusCode OnLangsAvailable(Client c, IMessage message)
        {
            languages = new ConcurrentDictionary<string, Language>();
            return StatusCode.Success;
        }

        private static StatusCode OnLang(Client c, IMessage message)
        {
            var lang = (Language) message;
            languages[lang.Code] = lang;
            return StatusCode.Success;
        }
    }
}
