using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Command
{
    internal class CommandTranslator
    {
        public delegate Status.StatusCode CommandEventHandler(Client c, ICommand comm);
        private readonly Dictionary<CmdType, CommandEventHandler> handlers; 
        public CommandTranslator(Dictionary<CmdType, CommandEventHandler> eventsHandlers)
        {
            handlers = eventsHandlers;
        }
        public void OnRequest(Client client, byte[] payload)
        {
            var ctype = ICommand.GetType(payload);
            var cmd = ICommand.Create(ctype, payload);
            Status.StatusCode? code = null;
            if(ctype == CmdType.Login || client.LoggedIn)
                code = handlers[ctype]?.Invoke(client, cmd);
            
            client.SendAsync(
                new Status(client.Acc, code ?? Status.StatusCode.Fail)
            );
        }
    }
}
