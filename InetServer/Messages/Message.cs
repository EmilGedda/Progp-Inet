using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using InetServer.i18n;

namespace InetServer.Messages
{
    [DataContract]
    public abstract class Message
    {
        private static readonly Dictionary<MessageType, Func<byte[], Message>> Mapping = new Dictionary<MessageType, Func<byte[], Message>>
        {
            { MessageType.Deposit,              x => new Deposit(x) },
            { MessageType.Withdrawal,           x => new Withdrawal(x) },
            { MessageType.LanguagesAvailable,   x => new LanguagesAvailable(x) },
            { MessageType.Language,             x => new Language(x) },
            { MessageType.Login,                x => new Login(x) },
            { MessageType.Status,               x => new Status(x) }
        };

        public static Message Create(MessageType c, byte[] payload)
        {
            return Mapping[c](payload);
        }

        public static MessageType GetType(byte[] payload)
        {
            return (MessageType)Enum.Parse(typeof(MessageType), payload[0].ToString());
        }
        public abstract byte[] Destruct();
    }
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