using System;
using System.Collections.Generic;
using InetServer.Command;

namespace InetServer
{

    public abstract class ICommand
    {
        private static readonly Dictionary<CmdType, Func<byte[], ICommand>> mapping = new Dictionary<CmdType, Func<byte[], ICommand>>
        {
            { CmdType.Deposit, x => new Deposit(x)},
            { CmdType.Withdrawal, x => new Withdrawal(x)}
        };

        public static ICommand Create(CmdType c, byte[] payload)
        {
            return mapping[c](payload);
        }

        public static CmdType GetType(byte[] payload)
        {
            return (CmdType)Enum.Parse(typeof (CmdType), payload[0].ToString());
        }
        public abstract byte[] Destruct();
    }
}