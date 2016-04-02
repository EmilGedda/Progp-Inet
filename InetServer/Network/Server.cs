using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using InetServer.Command;

namespace InetServer
{
    internal class Server
    {
        private readonly ConcurrentBag<Client> clients = new ConcurrentBag<Client>();

        // Create a listener on localhost:420
        private readonly TcpListener listener = TcpListener.Create(420);

        // ReSharper disable once UseObjectOrCollectionInitializer
        public async void Listen()
        {
            while (true)
            {
                var tcp = await listener.AcceptTcpClientAsync();
                var c = new Client(tcp);
                c.Request += NewTranslator().OnRequest;
                clients.Add(c);
            }
        }

        private CommandTranslator NewTranslator()
        {
            return new CommandTranslator(new Dictionary<CmdType, CommandTranslator.CommandEventHandler>
            {
                {CmdType.Deposit, OnDeposit},
                {CmdType.Withdrawal, OnWithdrawal}
            });
        }

        public void SendCommand(Client c, ICommand cmd)
        {

        }

        public void OnDeposit(Client c, ICommand cmd)
        {
            var d = (Deposit) cmd;
            c.Acc.Savings += d.Amount;
        }
        public void OnWithdrawal(Client c, ICommand cmd)
        {
            var d = (Withdrawal)cmd;
            if(d.Valid) c.Acc.Savings -= d.Amount;
        }
    }
}