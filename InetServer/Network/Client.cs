using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using InetServer.Messages;

#pragma warning disable 4014

namespace InetServer.Network
{
    /// <summary>
    ///     The wrapper class around TcpClient. Both the server and the client uses the same code for communicating with
    ///     eachother.
    ///     This class should be disposed correctly whenever the owner/server exits.
    /// </summary>
    public class Client : IDisposable
    {
        public delegate void RequestEventHandler(Client c, byte[] payload);

        private readonly string source;

        /// <summary>
        ///     The default constructor for the Client
        /// </summary>
        /// <param name="client">The TcpClient which manages the connection to the client</param>
        /// <param name="isserver">Whether the owner is a server or not</param>
        public Client(TcpClient client, bool isserver = true)
        {
            OnServer = isserver;
            Tcp = client;
            Tcp.NoDelay = true;
            var ipEndPoint = Tcp.Client.RemoteEndPoint as IPEndPoint;
            source = ipEndPoint?.ToString() ?? "unkown source";
        }

        public Account.Account Acc { get; set; }
        public bool OnServer { get; }
        public bool LoggedIn => Acc != null;
        public TcpClient Tcp { get; }
        public bool Disposed { get; private set; }

        /// <summary>
        ///     Disposal of the client and the connection.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Logger.Info($"Client disconnected {source}");
            Tcp.Close();
        }

        /// <summary>
        ///     Synchronized Listen, not really useful since it will prevent everything else.
        /// </summary>
        public void Listen()
        {
            ListenAsync().Wait();
        }

        /// <summary>
        ///     Starts a forever-running task which listens to incoming messages and handles them asynchronously.
        /// </summary>
        /// <returns>The task object</returns>
        public Task ListenAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    if (Disposed) return;
                    var msg = new byte[1];
                    var buffer = new byte[10];
                    var cnt = await Tcp.GetStream().ReadAsync(msg, 0, 1);
                    if (msg[0] == 5) // LanguagesAvailable == 5 -> Which is a variable length message
                    {
                        var intbuf = new byte[4];
                        // Read the next word in the packet which specifies the message length.
                        await Tcp.GetStream().ReadAsync(intbuf, 0, 4);
                        var len = BitConverter.ToInt32(intbuf, 0);
                        Array.Resize(ref buffer, len + 1 + 4);
                        buffer[0] = msg[0];
                        Buffer.BlockCopy(intbuf, 0, buffer, 1, 4);
                        // Read the rest of the message, now that we know the length.
                        await Tcp.GetStream().ReadAsync(buffer, 5, len);
                    }
                    else
                    {
                        cnt = await Tcp.GetStream().ReadAsync(buffer, 1, buffer.Length - 1);
                        buffer[0] = msg[0];
                    }
                    if (OnServer)
                        Logger.Info($"Recieved message {Message.GetType(buffer)} from "
                                    + (!OnServer ? "Server" : "Client")
                                    + ": " + source);

                    if (cnt < 1) break; // Client sent disconnect: RST packet most likely
                    Task.Run(() => Request?.Invoke(this, buffer));
                    // "Fire-and-forget", notify the message translator of a new message async.
                }
            });
        }

        public void Login(Account.Account acc) => Acc = acc;

        public event RequestEventHandler Request;

        /// <summary>
        ///     Send a message synchronously to the client.
        ///     TODO: deprecate this.
        /// </summary>
        /// <param name="cmd">The message to be sent</param>
        public void Send(Message cmd)
        {
            var payload = cmd.Destruct();
            Tcp.GetStream().Write(payload, 0, payload.Length);
        }

        /// <summary>
        ///     Send a message asynchronously to the client.
        /// </summary>
        /// <param name="cmd"></param>
        public async void SendAsync(Message cmd)
        {
            var payload = cmd.Destruct();
            if (OnServer)
                Logger.Info($"Sent message {Message.GetType(payload)} to "
                            + (!OnServer ? "Server" : "Client")
                            + ": " + source);
            await Tcp.GetStream().WriteAsync(payload, 0, payload.Length);
        }

        /// <summary>
        ///     Start the infinite listening-task of the client.
        /// </summary>
        public void StartListening()
        {
            ListenAsync().ContinueWith(task => Dispose());
        }
    }
}
