using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using InetServer.i18n;

namespace InetServer.Message
{
    [DataContract]
    public abstract class IMessage
    {
        private static readonly Dictionary<MessageType, Func<byte[], IMessage>> mapping = new Dictionary<MessageType, Func<byte[], IMessage>>
        {
            { MessageType.Deposit, x => new Deposit(x)},
            { MessageType.Withdrawal, x => new Withdrawal(x)},
            { MessageType.LanguagesAvailable, x => new LanguagesAvailable(x)},
            { MessageType.Language, x => new Language(x)},

        };

        public static IMessage Create(MessageType c, byte[] payload)
        {
            return mapping[c](payload);
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