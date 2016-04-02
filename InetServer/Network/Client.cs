using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using InetServer.Command;

namespace InetServer
{
    class Client
    {
        public delegate void RequestEventHandler(Client c, byte[] payload);
        public event RequestEventHandler Request; 
        public Account Acc { get; private set; }
        public bool LoggedIn => Acc != null;
        public void Login(Account acc) => Acc = acc;
        private TcpClient Tcp { get; }

        public Client(TcpClient client)
        {
            Tcp = client;
            var t = Listen();
            if (t.IsFaulted)
                t.Wait(3000);
        }

        public Task Listen()
        {
            return Task.Run(async () =>
            {
                using (var stream = Tcp.GetStream())
                {
                    var buffer = new byte[4096]; //TODO: Listen to end
                    Console.WriteLine("Reading from Client: " + Acc.Cardnumber);
                    var cnt = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (cnt > 0) Request?.Invoke(this, buffer);
                }
            });
        }
    }
}
