using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using InetServer.i18n;

namespace InetServer.Messages
{
    /// <summary>
    ///     The abstract base class for a Message which can be sent and recieved.
    /// </summary>
    [DataContract]
    public abstract class Message
    {
        /// <summary>
        ///     The corresponding constructs from a byte-array to a class inheriting from Message.
        ///     TODO: Make the classes themselves responsible for adding themselves to this dictionary, to increase modularization.
        /// </summary>
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
        ///     Creates a Message from a MessageType and a payload.
        /// </summary>
        /// <param name="c">The MessageType of the Message</param>
        /// <param name="payload">The serialzied version of the Message</param>
        /// <returns>The Message object from the byte[]</returns>
        public static Message Create(MessageType c, byte[] payload)
        {
            return Mapping[c](payload);
        }

        /// <summary>
        ///     Destruct a Message into a byte array, i.e. serialize it.
        /// </summary>
        /// <returns>The byte array representatino of the Message</returns>
        public abstract byte[] Destruct();

        /// <summary>
        ///     Get the MessageType from a serialized Message
        /// </summary>
        /// <param name="payload">The serialized version of Message</param>
        /// <returns>The MessageType of the serialized message</returns>
        public static MessageType GetType(byte[] payload)
        {
            return (MessageType) Enum.Parse(typeof(MessageType), payload[0].ToString());
        }
    }

    /// <summary>
    ///     All different MessageTypes.
    ///     Soft cap is at 256 different Messages, although a Message may also be defined recursively to escape this contraint.
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
