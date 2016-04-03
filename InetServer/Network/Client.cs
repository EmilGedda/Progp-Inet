using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InetServer
{
    internal class Client : IDisposable
    {
        public delegate void RequestEventHandler(Client c, byte[] payload);
        
        public Client(TcpClient client)
        {
            Tcp = client;
            ListenAsync().ContinueWith(task =>
            {
                Dispose();
            });
        }

        public Account Acc { get; private set; }
        public bool LoggedIn => Acc != null;
        public TcpClient Tcp { get; }
        public event RequestEventHandler Request;
        public void Login(Account acc) => Acc = acc;

        public Task ListenAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    var buffer = new byte[20]; //TODO: Listen to end
                    var cnt = await Tcp.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    Console.WriteLine("Reading from Client: " + (Tcp.Client.RemoteEndPoint as IPEndPoint));
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
            Console.WriteLine("[INFO] Client disconnected: " + (Tcp.Client.RemoteEndPoint as IPEndPoint));
            Tcp.Dispose();
        }
    }
}