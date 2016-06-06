using System.Collections.Generic;
using InetServer.Network;

namespace InetServer.Messages
{
    /// <summary>
    ///     The MessageTranslator handles the communication between the lowlevel TCP-communication and the high level client
    ///     This separates incomming messages into their corresponding MessageType counterpart and invokes the bound
    ///     CommandEventHandler.
    ///     Rationale: Separates the "non-managed" layer and the managed layer, so the higher level stuff never have to deal
    ///     with serializing, deserializing, and verification of the incoming data.
    /// </summary>
    public class MessageTranslator
    {
        public delegate StatusCode CommandEventHandler(Client c, Message comm);

        private readonly Dictionary<MessageType, CommandEventHandler> handlers;

        /// <summary>
        ///     Construct a MessageTranslator with the given bindings.
        /// </summary>
        /// <param name="eventsHandlers">The bindings supplied to the translator</param>
        public MessageTranslator(Dictionary<MessageType, CommandEventHandler> eventsHandlers)
        {
            handlers = eventsHandlers;
        }

        /// <summary>
        ///     Whenever a Client recieves a new Request this parses and invokes the corret EventHandler.
        ///     <seealso cref="Client.ListenAsync" />
        /// </summary>
        /// <param name="client">The Client which sent the message</param>
        /// <param name="payload">The message the Client sent</param>
        public void OnRequest(Client client, byte[] payload)
        {
            var ctype = Message.GetType(payload);
            var cmd = Message.Create(ctype, payload);
            StatusCode? code = null;

            if (ctype == MessageType.Login || client.LoggedIn || ctype == MessageType.Motd ||
                ctype == MessageType.LanguagesAvailable || ctype == MessageType.Status)
                code = handlers[ctype]?.Invoke(client, cmd);
            else
                Logger.Warning($"Unauthorized {ctype} by {client}!");

            if (code != StatusCode.Acknowledge)
                client.Send(new Status(client.Acc, code ?? StatusCode.Fail));
        }
    }
}
