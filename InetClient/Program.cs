using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Xml.Schema;
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
                Menu();
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
            Console.WriteLine("3. " + currentLang[Language.Label.Language]);
            Console.WriteLine("4. " + currentLang[Language.Label.Exit]);
        }

        private static void Menu()
        {
            while (true)
            {
                PrintMenu();
                var key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case '1':
                        Withdraw();
                        break;
                    case '2':
                        Deposit();
                        break;
                    case '3':
                        LanguageMenu();
                        break;
                    case '4':
                        ExitClient();
                        break;
                    default:
                        Console.WriteLine(currentLang[Language.Label.InvalidMenuItem]);
                        break;
                }
            }
        }

        private static void Withdraw()
        {
            Console.Clear();
            Console.WriteLine(currentLang[Language.Label.Withdraw] + ":\n");
            int amount;
            bool valid;
            GetCodeAmount(out amount, out valid);
            var withdrawal = new Withdrawal(amount, valid);
            client.SendAsync(withdrawal);
        }

        private static void Deposit()
        {
            Console.Clear();
            Console.WriteLine(currentLang[Language.Label.Deposit] + ":\n");
            int amount;
            bool valid;
            GetCodeAmount(out amount, out valid);
            var deposit = new Deposit(amount);
            client.SendAsync(deposit);
        }

        private static void GetCodeAmount(out int amount, out bool valid)
        {
            bool correctInput = false;
            amount = 0;
            while (!correctInput)
            {
                Console.WriteLine(currentLang[Language.Label.EnterCode]);
                string codeStr = Console.ReadLine();
                int code = 0;
                correctInput = Int32.TryParse(codeStr, out code);
                if (!(code < 100 && code > 0 && code % 2 != 0))
                    correctInput = false;
                if (!correctInput)
                    Console.WriteLine(currentLang[Language.Label.InvalidInput]);
            }

            valid = true;
            correctInput = false;
            while (!correctInput)
            {
                Console.WriteLine(currentLang[Language.Label.EnterAmount]);
                string amountStr = Console.ReadLine();
                correctInput = Int32.TryParse(amountStr, out amount);
                if (amount <= 0)
                    correctInput = false;
                if (!correctInput)
                    Console.WriteLine(currentLang[Language.Label.InvalidInput]);
            }
        }

        private static void LanguageMenu()
        {
            Console.Clear();
            Console.WriteLine(currentLang[Language.Label.Language] + ":\n");
            var langArray = languages.Values.ToArray();
            for (int i = 0; i < languages.Count; i++)
            {
                int row = i+1;
                Console.WriteLine(row + ". " + langArray[i].Name);
            }
            Console.WriteLine();

            bool correctInput = false;
            int indexInt = 0;
            while (!correctInput)
            {
                string index = Console.ReadLine();
                correctInput = Int32.TryParse(index, out indexInt);
                indexInt--;
                if (!(0 <= indexInt && indexInt < langArray.Length))
                    correctInput = false;
                if (!correctInput)
                    Console.WriteLine(currentLang[Language.Label.InvalidMenuItem]);
            }
            currentLang = langArray[indexInt];
        }

        private static void ExitClient()
        {
            throw new NotImplementedException();
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
                    if (!correctInput)
                        Console.WriteLine("Invalid card number, please try again.");
                }

                correctInput = false;
                while (!correctInput)
                {
                    Console.Write("Please enter your PIN:");
                    string pin = Console.ReadLine();
                    correctInput = Int16.TryParse(pin, out pinInt);
                    if (!correctInput)
                        Console.WriteLine("Invalid PIN, please try again.");
                }

                correctInput = false;
                var loginMsg = new Login(cardInt, pinInt);
                client.Send(loginMsg);
                var status = bc.Take();
                if (status == StatusCode.LoginSuccess)
                    correctInput = true;
                else
                    Console.WriteLine("Invalid account information, please try again.");
            }
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
