using System;
using System.Collections.Generic;
using System.Linq;

namespace InetServer.Command
{
    public class Deposit : ICommand
    {
     
        public int Amount { get; }

        public Deposit(int amount)
        {
            Amount = amount;
        }
        public Deposit(byte[] payload)
        {
            Amount = BitConverter.ToInt32(payload, 1);
        }

        protected byte[] destruct(CmdType t)
        {
            var p = new byte[10];
            p[0] = (byte)t;
            var amount = BitConverter.GetBytes(Amount);
            for (var i = 1; i < amount.Length; i++)
                p[i] = amount[i];

            return p;
        }
        public override byte[] Destruct()
        {
            return destruct(CmdType.Deposit);
        }
    }
}