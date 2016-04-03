using System;
using System.Collections.Generic;
using InetServer.Command;

namespace InetServer
{

    public abstract class ICommand
    {
        private static readonly Dictionary<Message, Func<byte[], ICommand>> mapping = new Dictionary<Message, Func<byte[], ICommand>>
        {
            { Message.Deposit, x => new Deposit(x)},
            { Message.Withdrawal, x => new Withdrawal(x)}
        };

        public static ICommand Create(Message c, byte[] payload)
        {
            return mapping[c](payload);
        }

        public static Message GetType(byte[] payload)
        {
            return (Message)Enum.Parse(typeof (Message), payload[0].ToString());
        }
        public abstract byte[] Destruct();
    }
}