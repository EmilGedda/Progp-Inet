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
            Logger.Info($"Client disconnected {this}");
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
                    switch (msg[0])
                    {
                        case 5: //Language message
                            var intbuf = new byte[4];
                            // Read the next word in the packet which specifies the message length.
                            await Tcp.GetStream().ReadAsync(intbuf, 0, 4);
                            var len = BitConverter.ToInt32(intbuf, 0);
                            Array.Resize(ref buffer, len + 1 + 4);
                            buffer[0] = msg[0];
                            Buffer.BlockCopy(intbuf, 0, buffer, 1, 4);
                            // Read the rest of the message, now that we know the length.
                            await Tcp.GetStream().ReadAsync(buffer, 5, len);
                            break;
                        case 2: // motd message
                            Array.Resize(ref buffer, 81);
                            await Tcp.GetStream().ReadAsync(buffer, 1, 80);
                            buffer[0] = 2; //motd
                            break;
                        default: //anything else
                            cnt = await Tcp.GetStream().ReadAsync(buffer, 1, buffer.Length - 1);
                            buffer[0] = msg[0];
                            break;
                    }
                    if (OnServer)
                        Logger.Info($"Recieved message {Message.GetType(buffer)} from "
                                    + (!OnServer ? "Server" : "Client")
                                    + ": " + this);

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
                            + ": " + this);
            await Tcp.GetStream().WriteAsync(payload, 0, payload.Length);
        }

        /// <summary>
        /// Enables string representation of the client.
        /// </summary>
        /// <returns>The destination of this client as address:port</returns>
        public override string ToString()
        {
            return source;
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
