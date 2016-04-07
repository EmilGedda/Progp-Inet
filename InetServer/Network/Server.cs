using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using InetServer.i18n;
using InetServer.Message;
namespace InetServer
{
    internal class Server : IDisposable
    {
        private readonly string motd = "Welcome to bank X Y Z";
        private readonly List<Client> clients = new List<Client>();
        private readonly List<Account> accounts;
        private readonly List<Language> langs;
        // Create a listener on localhost:420
        private readonly TcpListener listener = TcpListener.Create(420);

        public Server()
        {
            accounts = AccountSerializer.LoadAccounts();
            langs = new LanguageSerializer().LoadAccounts();
        }

        // ReSharper disable once UseObjectOrCollectionInitializer
        // ReSharper disable once FunctionNeverReturns
        public async void Listen()
        {
            listener.Start();
            Console.WriteLine("[INFO] Server started on " + (listener.LocalEndpoint as IPEndPoint));
            while (true)
            {
                var tcp = await listener.AcceptTcpClientAsync();
                var c = new Client(tcp);
                Console.WriteLine("[INFO] New connection from " + (tcp.Client.RemoteEndPoint as IPEndPoint));
                c.Request += new MessageTranslator(new Dictionary<MessageType, MessageTranslator.CommandEventHandler>
                {
                    {MessageType.Deposit, OnDeposit},
                    {MessageType.Withdrawal, OnWithdrawal}
                }).OnRequest;

                clients.Add(c);
            }
        }

        public StatusCode OnDeposit(Client c, IMessage d)
        {
            c.Acc.Savings += ((Deposit) d).Amount;
            return StatusCode.Success;
        }

        public StatusCode OnWithdrawal(Client client, IMessage cmd)
        {
            var w = (Withdrawal) cmd;
            if (!w.Valid) return StatusCode.InvalidCode;
            client.Acc.Savings -= w.Amount;
            return StatusCode.Success;
        }

        public void Dispose()
        {
            Console.WriteLine("[INFO] Shutting down...");
            AccountSerializer.SaveAccounts(accounts);
            foreach (var c in clients.Where(c => !c.Disposed)) c.Dispose();
            Console.WriteLine("[INFO] Server exited.");
        }
    }
}