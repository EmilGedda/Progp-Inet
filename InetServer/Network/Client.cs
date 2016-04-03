using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InetServer
{
    internal class Client : IDisposable
    {
        public delegate void RequestEventHandler(Client c, byte[] payload);
        
        public Client(TcpClient client)
        {
            Tcp = client;
            var ipEndPoint = Tcp.Client.RemoteEndPoint as IPEndPoint;
            source = ipEndPoint?.ToString() ?? "unkown source";
            ListenAsync().ContinueWith(task => Dispose());
        }

        public Account Acc { get; private set; }
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
                    var buffer = new byte[20]; //TODO: Listen to end
                    int cnt = await Tcp.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    Console.WriteLine("Reading from Client: " + source);
                    if (cnt < 1) break; // Client sent disconnect: RST packet most likely
                    if (buffer[0] < 127) Request?.Invoke(this, buffer);
                }
            });
        }

        public async void SendAsync(ICommand cmd)
        {
            var payload = cmd.Destruct();
            await Tcp.GetStream().WriteAsync(payload, 0, payload.Length);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Console.WriteLine("[INFO] Client disconnected: " + source);
            Tcp.Close();
        }
    }
}