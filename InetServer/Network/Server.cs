using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InetServer
{
    class Server
    {
        ConcurrentBag<Client> clients = new ConcurrentBag<Client>();
        // Create a listener on localhost:420
        TcpListener listener = TcpListener.Create(420);
        public async void Listen()
        {
            while (true)
            {
                var tcp = await listener.AcceptTcpClientAsync();
                clients.Add(new Client(tcp));
            }
        }

        public void ProcessCommand()
        {
            
        }
        public void SendCommand(ICommand cmd)
        {
            
        }
    }
}
