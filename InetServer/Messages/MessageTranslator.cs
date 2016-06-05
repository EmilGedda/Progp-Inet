using System.Collections.Generic;
using InetServer.Network;

namespace InetServer.Messages
{
    public class MessageTranslator
    {
        public delegate StatusCode CommandEventHandler(Client c, Message comm);
        private readonly Dictionary<MessageType, CommandEventHandler> handlers;

        public MessageTranslator(Dictionary<MessageType, CommandEventHandler> eventsHandlers)
        {
            handlers = eventsHandlers;
        }

        public void OnRequest(Client client, byte[] payload)
        {
            var ctype = Message.GetType(payload);
            var cmd = Message.Create(ctype, payload);
            StatusCode? code = null;

            if (ctype == MessageType.Login || client.LoggedIn)
                code = handlers[ctype]?.Invoke(client, cmd);

            if(code != StatusCode.Acknowledge)
                client.Send(new Status(client.Acc, code ?? StatusCode.Fail));
            
        }
    }
}
