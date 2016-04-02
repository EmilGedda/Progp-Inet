using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
            listener.Start();
            Console.WriteLine("[INFO] Server started on " + (listener.LocalEndpoint as IPEndPoint));
            while (true)
            {
                var tcp = await listener.AcceptTcpClientAsync();
                var c = new Client(tcp);
                Console.WriteLine("[INFO] New connection from " + (tcp.Client.RemoteEndPoint as IPEndPoint));
                c.Request += new CommandTranslator(new Dictionary<CmdType, CommandTranslator.CommandEventHandler>
                {
                    {CmdType.Deposit, OnDeposit},
                    {CmdType.Withdrawal, OnWithdrawal}
                }).OnRequest;

                clients.Add(c);
            }
        }

        public void SendCommand(Client c, ICommand cmd)
        {
        }


        public void OnDeposit(Client c, ICommand d) => c.Acc.Savings += ((Deposit) d).Amount;

        public void OnWithdrawal(Client client, ICommand cmd)
        {
            var w = (Withdrawal) cmd;
            if (w.Valid)
                client.Acc.Savings -= w.Amount;
        }
    }
}