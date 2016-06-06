using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Schema;
using InetServer;
using InetServer.Account;
using InetServer.i18n;
using InetServer.Messages;
using InetServer.Network;

namespace InetClient
{
    class Program
    {
        private static Client client;
        private static BlockingCollection<Status> bc = new BlockingCollection<Status>();
        private static ConcurrentDictionary<string, Language> languages = new ConcurrentDictionary<string, Language>();
        private static Language currentLang;
        private static string motd = "Empty Motd";
        private const string DefaultLang = "en-US";

        // ReSharper disable once UseObjectOrCollectionInitializer
        private static void Main(string[] args)
        {
            try {
                Console.Clear();
                Logger.Info("Connecting to server...");
                TcpClient t = new TcpClient();
                t.ReceiveBufferSize *= 8;
                t.Connect(IPAddress.Loopback, 420);
                Logger.Info("Connection established to server.");
                client = new Client(t, false)
                {
                    Acc = new Account()
                };
                client.Request += new MessageTranslator(new Dictionary<MessageType, MessageTranslator.CommandEventHandler>
                {
                    {MessageType.Language, OnLang},
                    {MessageType.LanguagesAvailable, OnLangsAvailable},
                    {MessageType.Status, OnStatus},
                    {MessageType.Motd, OnMotd}
                    
                }).OnRequest;
                client.ListenAsync();
                client.Send(new LanguagesAvailable());
                client.Send(new Motd());
                var status = LogIn();
                SetLanguage(DefaultLang);
                Menu(status);
            }
            catch (SocketException se) {
                Logger.Error("Unable to connect to server");
            }
            
            Console.ReadKey();
        }

        private static void SetLanguage(string code) => currentLang = languages[code];

        private static void PrintMenu()
        {
            Console.WriteLine("1. " + currentLang[Language.Label.Withdraw]);
            Console.WriteLine("2. " + currentLang[Language.Label.Deposit]);
            Console.WriteLine("3. " + currentLang[Language.Label.Language]);
            Console.WriteLine("4. " + currentLang[Language.Label.Exit]);
        }

        private static void Menu(Status lastStatus)
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine($"Account: {lastStatus.Cardnumber}");
                Console.WriteLine($"Savings: {lastStatus.Savings}$");
                Console.WriteLine();
                Console.WriteLine(motd);
                PrintMenu();
                Console.WriteLine();

                Console.Write("Enter menu option: ");
                var key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case '1':
                        Withdraw(lastStatus);
                        break;
                    case '2':
                        Deposit(lastStatus);
                        break;
                    case '3':
                        LanguageMenu();
                        break;
                    case '4':
                        Console.WriteLine();
                        ExitClient();
                        break;
                    default:
                        Console.WriteLine(currentLang[Language.Label.InvalidMenuItem]);
                        break;
                }
            }
        }

        private static void Withdraw(Status lastStatus)
        {
            Console.Clear();
            Console.WriteLine($"Account: {lastStatus.Cardnumber}");
            Console.WriteLine($"Savings: {lastStatus.Savings}$");
            Console.WriteLine();
            Console.WriteLine(currentLang[Language.Label.Withdraw]);
            Console.WriteLine(new string('-', 10));
            int amount;
            bool valid = false;
            GetCodeAmount(out amount, ref valid);
            var withdrawal = new Withdrawal(amount, valid);
            client.SendAsync(withdrawal);
            Console.Clear();
        }

        private static void Deposit(Status lastStatus)
        {
            Console.Clear();
            Console.WriteLine($"Account: {lastStatus.Cardnumber}");
            Console.WriteLine($"Savings: {lastStatus.Savings}$");
            Console.WriteLine();
            Console.WriteLine(currentLang[Language.Label.Deposit]);
            Console.WriteLine(new string('-', 10));
            int amount;
            bool valid = true;
            GetCodeAmount(out amount, ref valid);
            var deposit = new Deposit(amount);
            client.SendAsync(deposit);
            Console.Clear();
        }
 
        private static void GetCodeAmount(out int amount, ref bool valid)
        {
            bool correctInput = false;
            amount = 0;
            while (!correctInput && !valid)
            {
                Console.Write(currentLang[Language.Label.EnterCode] + ": ");
                string codeStr = Console.ReadLine();
                int code = 0;
                correctInput = Int32.TryParse(codeStr, out code);
                if (code < 100 && code > 0 && code % 2 == 1)
                    valid = true;
            }
            correctInput = false;
            while (!correctInput)
            {
                Console.Write(currentLang[Language.Label.EnterAmount] + ": ");
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
            Console.Clear();
        }

        private static void ExitClient()
        {
            client.Dispose();
            Environment.Exit(0);
        }

        private static Status LogIn()
        {
            short pinInt = 0;
            int cardInt = 0;
            bool correctInput = false;
            Status status = null;
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
                status = bc.Take();
                if (status.Code == StatusCode.LoginSuccess)
                    correctInput = true;
                else
                    Console.WriteLine("Invalid account information, please try again.");
            }
            return status;
        }

        private static StatusCode OnStatus(Client c, Message message)
        {
            var msg = (Status) message;
            if (msg.Code == StatusCode.LoginSuccess | msg.Code == StatusCode.LoginFail)
                bc.Add(msg);
            return StatusCode.Acknowledge;
        }

        private static StatusCode OnMotd(Client c, Message message)
        {
            motd = ((Motd) message).Message;
            return StatusCode.Success;
        }

        private static StatusCode OnLangsAvailable(Client c, Message message)
        {
            languages = new ConcurrentDictionary<string, Language>();
            return StatusCode.Success;
        }

        private static StatusCode OnLang(Client c, Message message)
        {
            var lang = (Language) message;
            languages[lang.Code] = lang;
            return StatusCode.Success;
        }
    }
}
