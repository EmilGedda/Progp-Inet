using System.Collections.Generic;

namespace InetServer.Message
{
    internal class MessageTranslator
    {
        public delegate StatusCode CommandEventHandler(Client c, IMessage comm);
        private readonly Dictionary<MessageType, CommandEventHandler> handlers;

        public MessageTranslator(Dictionary<MessageType, CommandEventHandler> eventsHandlers)
        {
            handlers = eventsHandlers;
        }

        public void OnRequest(Client client, byte[] payload)
        {
            var ctype = IMessage.GetType(payload);
            var cmd = IMessage.Create(ctype, payload);
            StatusCode? code = null;
            if(ctype == MessageType.Login || client.LoggedIn)
                code = handlers[ctype]?.Invoke(client, cmd);
            
            client.SendAsync(
                new Status(client.Acc, code ?? StatusCode.Fail)
            );
        }
    }
}
