using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InetServer.Message;
#pragma warning disable 4014

namespace InetServer
{
    public class Client : IDisposable
    {
        public delegate void RequestEventHandler(Client c, byte[] payload);
        

        public Client(TcpClient client, bool isserver = true)
        {
            OnServer = isserver;
            Tcp = client;
            Tcp.NoDelay = true;
            var ipEndPoint = Tcp.Client.RemoteEndPoint as IPEndPoint;
            source = ipEndPoint?.ToString() ?? "unkown source";
        }

        public void StartListening()
        {
            ListenAsync().ContinueWith(task => Dispose());
        }

        public Account Acc { get; set; }
        public bool OnServer { get; } //Debug shit
        public bool LoggedIn => Acc != null;
        public TcpClient Tcp { get; }
        public bool Disposed { get; private set; }
        public event RequestEventHandler Request;
        public void Login(Account acc) => Acc = acc;
        private readonly string source;

        public Task ListenAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    if (Disposed) return;
                    var msg = new byte[1];
                    var buffer = new byte[10];
                    int cnt = await Tcp.GetStream().ReadAsync(msg, 0, 1);
                    if (msg[0] == 5)
                    {
                        var intbuf = new byte[4];
                        await Tcp.GetStream().ReadAsync(intbuf, 0, 4);
                        var len = BitConverter.ToInt32(intbuf, 0);
                        Array.Resize(ref buffer, len+1+4);
                        buffer[0] = msg[0];
                        Buffer.BlockCopy(intbuf, 0, buffer, 1, 4);
                        await Tcp.GetStream().ReadAsync(buffer, 5, len);
                    }
                    else
                    {
                        cnt = await Tcp.GetStream().ReadAsync(buffer, 1, buffer.Length-1);
                        buffer[0] = msg[0];
                    }
                    if(OnServer)
                        Logger.Info($"Recieved message {IMessage.GetType(buffer)} from " + (!OnServer ? "Server" : "Client") + ": " + source);

                    if (cnt < 1) break; // Client sent disconnect: RST packet most likely
                    if (buffer[0] < 127) 
                        Task.Run(() => Request?.Invoke(this, buffer));
                }
            });
        }

        public void Listen()
        {
            ListenAsync().Wait();
        }
        
        public async void SendAsync(IMessage cmd)
        {
            var payload = cmd.Destruct();
            if (OnServer)
                Logger.Info($"Sent message {IMessage.GetType(payload)} to " + (!OnServer ? "Server" : "Client") + ": " + source);
            await Tcp.GetStream().WriteAsync(payload, 0, payload.Length);
        }

        public void Send(IMessage cmd)
        {
            var payload = cmd.Destruct();
            Tcp.GetStream().Write(payload, 0, payload.Length);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Logger.Info($"Client disconnected {source}");
            Tcp.Close();
        }
    }
}