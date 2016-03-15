using System;
using System.Collections.Generic;
using InetServer.Command;

namespace InetServer
{

    public abstract class ICommand
    {
        private Dictionary<CmdType, Func<byte[], ICommand>> mapping = new Dictionary<CmdType, Func<byte[], ICommand>>
        {
            { CmdType.Deposit, x => new Deposit(x)} 
        };

        public abstract CmdType Cmd { get; }
        public abstract byte[] Destruct();

        protected ICommand(byte[] payload)
        {

        }
    }
}