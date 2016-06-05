using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using InetServer.i18n;
using InetServer.Message;

namespace InetServer.Network
{
    /// <summary>
    ///     A server which handles incoming connections from ATM clients.
    /// </summary>
    internal class Server : IDisposable
    {
        private readonly List<Account> accounts;
        private readonly List<Client> clients = new List<Client>();
        private readonly List<Language> langs;

        // Create a listener on localhost:420
        private readonly TcpListener listener = TcpListener.Create(420);
        private readonly Motd motd = new Motd();

        /// <summary>
        ///     Loads the IO-saved accounts and languages
        /// </summary>
        public Server()
        {
            accounts = AccountSerializer.LoadAccounts();
            langs = LanguageSerializer.Instance.LoadAccounts();
        }

        /// <summary>
        ///     Disposes the current server and disconnects all the clients.
        /// </summary>
        public void Dispose()
        {
            Logger.Info("Shutting down...");
            AccountSerializer.SaveAccounts(accounts);
            foreach (var c in clients.Where(c => !c.Disposed)) c.Dispose();
            Logger.Info("Server exited.");
        }

        // ReSharper disable once UseObjectOrCollectionInitializer
        // ReSharper disable once FunctionNeverReturns
        /// <summary>
        ///     The main loop for the Server, this loop accepts asynchronously clients and initializes them
        ///     The client.Request event fires whenever a Server recieves a new message from a client.
        /// </summary>
        public async void Listen()
        {
            listener.Start();
            Logger.Info($"Server started on {listener.LocalEndpoint as IPEndPoint}");
            while (true)
            {
                var tcp = await listener.AcceptTcpClientAsync();
                var c = new Client(tcp);
                Logger.Info($"New connection from {tcp.Client.RemoteEndPoint as IPEndPoint}");

                // Subscribe a MessageTranslator to the Clients OnRequest events, mapping MessageTypes to functions which handles the request.
                c.Request += new MessageTranslator(new Dictionary<MessageType, MessageTranslator.CommandEventHandler>
                {
                    {MessageType.Deposit, OnDeposit},
                    {MessageType.Withdrawal, OnWithdrawal},
                    {MessageType.Motd, OnMotd},
                    {MessageType.Status, OnStatus},
                    {MessageType.Login, OnLogin},
                    {MessageType.LanguagesAvailable, OnLangsAvail}
                }).OnRequest;

                c.StartListening();

                clients.Add(c);
            }
        }

        /// <summary>
        ///     Whenever a Deposit message was recieved by the server
        /// </summary>
        /// <param name="c">The client which sent the message</param>
        /// <param name="d">The Deposit object as an IMessage</param>
        /// <returns>If the amount is positive and does not trigger a overflow a Success is returned, otherwise a Fail</returns>
        public StatusCode OnDeposit(Client c, IMessage d)
        {
            var amt = ((Deposit) d).Amount;
            if (amt < 0)
                return StatusCode.Fail;

            try
            {
                c.Acc.Savings = checked(c.Acc.Savings + amt); //Check for overflow
            }
            catch (OverflowException)
            {
                Logger.Error($"Overflow occured on deposit by client: {c.Acc}");
                return StatusCode.Fail;
            }

            return StatusCode.Success;
        }

        /// <summary>
        ///     When a client requests all languages, this triggers and replies with all stored languages,
        ///     the server first sends the count of all languages as a LanguageAvailable message, however this
        ///     is not used by the client (yet).
        /// </summary>
        /// <param name="client">The client which sent the message</param>
        /// <param name="cmd">The LanguagesAvailable as an IMessage</param>
        /// <returns>Success</returns>
        public StatusCode OnLangsAvail(Client client, IMessage cmd)
        {
            client.SendAsync(new LanguagesAvailable((byte) langs.Count));

            foreach (var l in langs)
                client.SendAsync(l);

            return StatusCode.Success;
        }

        /// <summary>
        ///     Whenever a Login message was recieved by the server.
        ///     This does only trigger whenever a client tries to login.
        /// </summary>
        /// <param name="client">The client which sent the message</param>
        /// <param name="cmd">The Login object as an IMessage</param>
        /// <returns>Fail authorization failed, otherwise LoginSuccess</returns>
        public StatusCode OnLogin(Client client, IMessage cmd)
        {
            var l = (Login) cmd;
            var acc = accounts.FirstOrDefault(a => a.Cardnumber == l.Cardnumber && a.Pin == l.Pin);

            if (acc == null)
                return StatusCode.Fail;

            client.Acc = acc;
            return StatusCode.LoginSuccess;
        }

        /// <summary>
        ///     Whenever a Motd message was recieved by the server.
        ///     The server responds by sending the current Motd back to the client.
        /// </summary>
        /// <param name="client">The client which sent the message</param>
        /// <param name="cmd">The Motd message as an IMessage, this should not be used.</param>
        /// <returns>Success</returns>
        public StatusCode OnMotd(Client client, IMessage cmd)
        {
            client.SendAsync(motd);
            return StatusCode.Success;
        }

        /// <summary>
        ///     Whenever a Status message was recieved by the server
        ///     The server answers silently by acknowledging it.
        ///     TODO: Could be extended for some serious error handling.
        /// </summary>
        /// <param name="client">The client which sent the message</param>
        /// <param name="cmd">The Status object as an IMessage</param>
        /// <returns>StatusCode.Acknowledge</returns>
        public StatusCode OnStatus(Client client, IMessage cmd) => StatusCode.Acknowledge;

        /// <summary>
        ///     Whenever a Withdrawal message was recieved by the server
        /// </summary>
        /// <param name="client">The client which sent the message</param>
        /// <param name="cmd">The Withdrawal object as an IMessage</param>
        /// <returns>Success if the withdrawal was successful and permitted, otherwise Fail</returns>
        public StatusCode OnWithdrawal(Client client, IMessage cmd)
        {
            var w = (Withdrawal) cmd;

            if (!w.Valid || client.Acc.Savings < w.Amount || w.Amount < 1)
                return StatusCode.Fail;

            client.Acc.Savings -= w.Amount;
            return StatusCode.Success;
        }
    }
}
