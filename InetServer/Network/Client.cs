using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InetServer
{
    class Client
    {
        public Action<byte[]> Request { get; }
        private Account account { get; }
        private TcpClient Tcp { get; }

        public Client(TcpClient client, Action<byte[]> request)
        {
            Request = request;
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
                    Console.WriteLine("Reading from Client: " + account.Cardnumber);
                    var cnt = await stream.ReadAsync(buffer, 0, buffer.Length);
                    Request(buffer); //TODO: CommandTranslator service, translates byte[] to ICommand and executes lambdas over it.
                }
            });
        }
    }

    class CommandEvent : EventArgs
    {
        public Client client { get; set; }
        public ICommand Command { get; set; }
    }
}
