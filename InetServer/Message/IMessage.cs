using System;
using System.Collections.Generic;

namespace InetServer.Message
{
    [Serializable]
    public abstract class IMessage
    {
        private static readonly Dictionary<MessageType, Func<byte[], IMessage>> mapping = new Dictionary<MessageType, Func<byte[], IMessage>>
        {
            { MessageType.Deposit, x => new Deposit(x)},
            { MessageType.Withdrawal, x => new Withdrawal(x)}
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
        Lang,
        LangsAvailable
    }
}