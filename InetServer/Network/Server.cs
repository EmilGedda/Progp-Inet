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
        private string motd = "Welcome to bank X Y Z";
        private readonly List<Client> clients = new List<Client>();
        private readonly List<Account> accounts;
        private readonly List<Language> langs;
        // Create a listener on localhost:420
        private readonly TcpListener listener = TcpListener.Create(420);

        public Server()
        {
            accounts = AccountSerializer.LoadAccounts();
            langs = LanguageSerializer.Instance.LoadAccounts();
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
                    {MessageType.Withdrawal, OnWithdrawal},
                    {MessageType.Motd, OnMotd},
                    {MessageType.Status, OnStatus},
                    {MessageType.Login, OnLogin},
                    {MessageType.LangsAvailable, OnLangsAvail}
                }).OnRequest;

                clients.Add(c);
            }
        }

        public StatusCode OnDeposit(Client c, IMessage d)
        {
            int amt = ((Deposit) d).Amount;
            if(amt < 0 ) return StatusCode.Fail;
            try
            {
                c.Acc.Savings = checked(c.Acc.Savings + amt); //Check for overflow
            }
            catch (OverflowException)
            {
                Console.WriteLine("[ERROR] Overflow occured on deposit by client: " + c.Acc);
                return StatusCode.Fail;
            }

            return StatusCode.Success;
        }

        public StatusCode OnWithdrawal(Client client, IMessage cmd)
        {
            var w = (Withdrawal) cmd;
            if (!w.Valid || client.Acc.Savings < w.Amount) return StatusCode.Fail;
            client.Acc.Savings -= w.Amount;
            return StatusCode.Success;
        }

        public StatusCode OnLogin(Client client, IMessage cmd)
        {
            var l = (Login) cmd;
            var acc = accounts.FirstOrDefault(a => a.Cardnumber == l.Cardnumber && a.Pin == l.Pin);
            if(acc == null) return StatusCode.InvalidPin;
            client.Acc = acc;
            return StatusCode.Success;
        }
        public StatusCode OnStatus(Client client, IMessage cmd) => StatusCode.Success;

        public StatusCode OnMotd(Client client, IMessage cmd)
        {
            client.SendAsync(new Motd(motd));
            return StatusCode.Success;
        }

        public StatusCode OnLangsAvail(Client client, IMessage cmd)
        {
            foreach(Language l in langs)
                client.SendAsync(l);

            client.SendAsync(new LanguagesAvailable());
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