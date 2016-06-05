using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using InetServer.i18n;

namespace InetServer.Message
{
    /// <summary>
    ///     The message base class which all messages inherits from
    /// </summary>
    [DataContract]
    public abstract class Message
    {
        // A dictionary mapping MessageType to Messages, and their function to convert a byte[] to a message
        private static readonly Dictionary<MessageType, Func<byte[], Message>> Mapping = new Dictionary
            <MessageType, Func<byte[], Message>>
        {
            {MessageType.Deposit, x => new Deposit(x)},
            {MessageType.Withdrawal, x => new Withdrawal(x)},
            {MessageType.LanguagesAvailable, x => new LanguagesAvailable(x)},
            {MessageType.Language, x => new Language(x)},
            {MessageType.Login, x => new Login(x)},
            {MessageType.Status, x => new Status(x)}
        };

        /// <summary>
        ///     Creates a message object given a messagetype and a byte array
        /// </summary>
        /// <param name="c">The MessageType of the message</param>
        /// <param name="payload">The full byte array as sent by the client</param>
        /// <returns>A Message object corresponding to the object the client sent</returns>
        public static Message Create(MessageType c, byte[] payload)
        {
            return Mapping[c](payload);
        }

        /// <summary>
        ///     Destruct the objects into a byte-array before sending it to another client/server.
        ///     A custom serializer is used instead of the builtin BinaryFormatter in order to comply with the 10 byte constraint.
        /// </summary>
        /// <returns>The binary serialization of the object.</returns>
        public abstract byte[] Destruct();

        /// <summary>
        ///     Get the MessageType based of a raw byte[], by parsing its first byte.
        /// </summary>
        /// <param name="payload">The byte array sent by the client</param>
        /// <returns>The MessageType of the sent message</returns>
        public static MessageType GetType(byte[] payload)
        {
            return (MessageType) Enum.Parse(typeof(MessageType), payload[0].ToString());
        }
    }

    /// <summary>
    ///     All different MessageTypes (and messages) possible
    /// </summary>
    public enum MessageType : byte
    {
        Login,
        Status,
        Motd,
        Deposit,
        Withdrawal,
        Language,
        LanguagesAvailable
    }
}
