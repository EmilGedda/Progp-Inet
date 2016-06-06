using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using InetServer;
using InetServer.Account;
using InetServer.i18n;
using InetServer.Messages;
using InetServer.Network;

namespace InetClient
{
    internal class Program
    {
        private const string DefaultLang = "en-US";
        private static Client client;
        private static readonly BlockingCollection<Status> bc = new BlockingCollection<Status>();
        private static ConcurrentDictionary<string, Language> languages = new ConcurrentDictionary<string, Language>();
        private static Language currentLang;
        private static string motd = "Empty Motd";

        /// <summary>
        ///     Method to handle a deposit by the user
        /// </summary>
        /// <param name="lastStatus">The Status of the last withdrawal or deposit</param>
        /// <returns>The new Status after the deposit</returns>
        private static Status Deposit(Status lastStatus)
        {
            var correctInput = false;
            while (!correctInput)
            {
                Console.Clear();
                Console.WriteLine($"Account: {lastStatus.Cardnumber}");
                Console.WriteLine($"Savings: {lastStatus.Savings}$");
                Console.WriteLine();
                Console.WriteLine(currentLang[Language.Label.Deposit]);
                Console.WriteLine(new string('-', 10));
                int amount;
                var valid = true;
                GetCodeAmount(out amount, ref valid);
                var deposit = new Deposit(amount);
                client.SendAsync(deposit);
                lastStatus = bc.Take();
                if (lastStatus.Code == StatusCode.DepositSuccess || amount == 0)
                    correctInput = true;
                else
                {
                    Console.WriteLine(currentLang[Language.Label.TransFail]);
                    Console.ReadKey(true);
                }
                Console.Clear();
            }
            return lastStatus;
        }

        /// <summary>
        ///     Exit the Client and dispose of the connection.
        /// </summary>
        private static void ExitClient()
        {
            client.Dispose();
            Environment.Exit(0);
        }

        /// <summary>
        ///     Helper function used by Withdraw and Deposit, verifies codes and amounts entered.
        /// </summary>
        /// <param name="amount">How much the user entered</param>
        /// <param name="valid">Whether the withdrawal should be considered valid</param>
        private static void GetCodeAmount(out int amount, ref bool valid)
        {
            var correctInput = false;
            amount = 0;
            while (!correctInput && !valid)
            {
                Console.Write(currentLang[Language.Label.EnterCode] + ": ");
                var codeStr = Console.ReadLine();
                var code = 0;
                correctInput = int.TryParse(codeStr, out code);
                if (code < 100 && code > 0 && code%2 == 1)
                    valid = true;
            }
            correctInput = false;
            while (!correctInput)
            {
                Console.Write(currentLang[Language.Label.EnterAmount] + ": ");
                var amountStr = Console.ReadLine();
                correctInput = int.TryParse(amountStr, out amount);
                if (amount < 0)
                    correctInput = false;
                if (!correctInput)
                    Console.WriteLine(currentLang[Language.Label.InvalidInput]);
            }
        }

        /// <summary>
        ///     Menu for handling the selection of languages
        /// </summary>
        private static void LanguageMenu()
        {
            Console.Clear();
            Console.WriteLine(currentLang[Language.Label.Language] + ":\n");
            var langArray = languages.Values.ToArray();
            for (var i = 0; i < languages.Count; i++)
            {
                var row = i + 1;
                Console.WriteLine(row + ". " + langArray[i].Name);
            }
            Console.WriteLine();

            var correctInput = false;
            var indexInt = 0;
            while (!correctInput)
            {
                var index = Console.ReadLine();
                correctInput = int.TryParse(index, out indexInt);
                indexInt--;
                if (!(0 <= indexInt && indexInt < langArray.Length))
                    correctInput = false;
                if (!correctInput)
                    Console.WriteLine(currentLang[Language.Label.InvalidMenuItem]);
            }
            currentLang = langArray[indexInt];
            Console.Clear();
        }

        /// <summary>
        ///     Menu for enabling a user to log in onto the server
        /// </summary>
        /// <returns></returns>
        private static Status LogIn()
        {
            short pinInt = 0;
            var cardInt = 0;
            var correctInput = false;
            Status status = null;
            while (!correctInput)
            {
                while (!correctInput)
                {
                    Console.Write("Please enter your card number:");
                    var cardnumber = Console.ReadLine();
                    correctInput = int.TryParse(cardnumber, out cardInt);
                    if (!correctInput)
                        Console.WriteLine("Invalid card number, please try again.");
                }

                correctInput = false;
                while (!correctInput)
                {
                    Console.Write("Please enter your PIN:");
                    var pin = Console.ReadLine();
                    correctInput = short.TryParse(pin, out pinInt);
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

        /// <summary>
        ///     Program entrypoint.
        /// </summary>
        /// <param name="args">Arguments sent on the CLI</param>
        // ReSharper disable once UseObjectOrCollectionInitializer
        private static void Main(string[] args)
        {
            try
            {
                Console.Clear();
                Logger.Info("Connecting to server...");
                var t = new TcpClient();
                t.ReceiveBufferSize *= 8;
                t.Connect(IPAddress.Loopback, 420);
                Logger.Info("Connection established to server.");
                client = new Client(t, false)
                {
                    Acc = new Account()
                };
                client.Request +=
                    new MessageTranslator(new Dictionary<MessageType, MessageTranslator.CommandEventHandler>
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
            catch (SocketException se)
            {
                Logger.Error("Unable to connect to server");
            }

            Console.ReadKey();
        }

        /// <summary>
        ///     The home menu handler
        /// </summary>
        /// <param name="lastStatus">The status of the last Message sent</param>
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
                        lastStatus = Withdraw(lastStatus);
                        break;
                    case '2':
                        lastStatus = Deposit(lastStatus);
                        break;
                    case '3':
                        LanguageMenu();
                        break;
                    case '4':
                        Console.WriteLine();
                        ExitClient();
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine(currentLang[Language.Label.InvalidMenuItem]);
                        break;
                }
            }
        }

        /// <summary>
        ///     This triggers whenever a Language message is recieved
        /// </summary>
        /// <param name="c">The client (i.e the server) which this message originates from</param>
        /// <param name="message">The Language recieved</param>
        /// <returns>StatusCode.Success always</returns>
        private static StatusCode OnLang(Client c, Message message)
        {
            var lang = (Language) message;
            languages[lang.Code] = lang;
            return StatusCode.Success;
        }

        /// <summary>
        ///     This triggers whenever a LanguageAvailable message is recieved.
        ///     Initializes a new ConcurrentDict for the languages held.
        /// </summary>
        /// <param name="c">The client (i.e the server) which this message originates from</param>
        /// <param name="message">The LanguageAvailable recieved</param>
        /// <returns>StatusCode.Success always</returns>
        private static StatusCode OnLangsAvailable(Client c, Message message)
        {
            languages = new ConcurrentDictionary<string, Language>();
            return StatusCode.Success;
        }

        /// <summary>
        ///     This triggers whenever a Motd message is recieved.
        ///     Updates the store motd.
        /// </summary>
        /// <param name="c">The client (i.e the server) which this message originates from</param>
        /// <param name="message">The Motd recieved</param>
        /// <returns>StatusCode.Success always</returns>
        private static StatusCode OnMotd(Client c, Message message)
        {
            motd = ((Motd) message).Message;
            return StatusCode.Success;
        }

        /// <summary>
        ///     This triggers whenever a Status message is recieved.
        ///     If the status originates from a transaction or log in, it is sent to the BlockingCollection for interthread
        ///     communication purposes.
        /// </summary>
        /// <param name="c">The client (i.e the server) which this message originates from</param>
        /// <param name="message">The Status recieved</param>
        /// <returns>StatusCode.Success always</returns>
        private static StatusCode OnStatus(Client c, Message message)
        {
            var msg = (Status) message;
            if (msg.Code == StatusCode.LoginSuccess || msg.Code == StatusCode.LoginFail ||
                msg.Code == StatusCode.WithdrawSuccess || msg.Code == StatusCode.WithdrawFail ||
                msg.Code == StatusCode.DepositSuccess || msg.Code == StatusCode.DepositFail)
                bc.Add(msg);
            return StatusCode.Acknowledge;
        }

        /// <summary>
        ///     Print the basic menu
        /// </summary>
        private static void PrintMenu()
        {
            Console.WriteLine("1. " + currentLang[Language.Label.Withdraw]);
            Console.WriteLine("2. " + currentLang[Language.Label.Deposit]);
            Console.WriteLine("3. " + currentLang[Language.Label.Language]);
            Console.WriteLine("4. " + currentLang[Language.Label.Exit]);
        }

        /// <summary>
        ///     Set the current selected language
        /// </summary>
        /// <param name="code">The Language code of the language chosen to be the new display planguage</param>
        private static void SetLanguage(string code) => currentLang = languages[code];


        /// <summary>
        ///     Menu function for handling a withdrawal inside the client
        /// </summary>
        /// <param name="lastStatus">The status of the last transaction or login</param>
        /// <returns>The status the server responded with after the new transaction</returns>
        private static Status Withdraw(Status lastStatus)
        {
            var correctInput = false;
            while (!correctInput)
            {
                Console.Clear();
                Console.WriteLine($"Account: {lastStatus.Cardnumber}");
                Console.WriteLine($"Savings: {lastStatus.Savings}$");
                Console.WriteLine();
                Console.WriteLine(currentLang[Language.Label.Withdraw]);
                Console.WriteLine(currentLang[Language.Label.TransFail]);
                Console.WriteLine(new string('-', 10));
                int amount;
                var valid = false;
                GetCodeAmount(out amount, ref valid);
                var withdrawal = new Withdrawal(amount, valid);
                if (amount > 0)
                {
                    client.SendAsync(withdrawal);
                    lastStatus = bc.Take();
                    if (lastStatus.Code == StatusCode.WithdrawSuccess || amount == 0)
                        correctInput = true;
                }
                else
                    correctInput = true;
                Console.Clear();
            }
            return lastStatus;
        }
    }
}
