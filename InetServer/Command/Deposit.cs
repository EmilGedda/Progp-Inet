using System;
using System.Collections.Generic;
using System.Linq;

namespace InetServer.Command
{
    public class Deposit : ICommand
    {

        public int Owner { get; }
        public int Amount { get; }

        public Deposit(int owner, int amount)
        {
            Owner = owner;
            Amount = amount;

        }
        public Deposit(byte[] payload)
        {
            Owner = BitConverter.ToInt32(payload, 1);
            Amount = BitConverter.ToInt32(payload, 5);
        }

        protected byte[] destruct(CmdType t)
        {
            var p = new byte[10];
            var owner = BitConverter.GetBytes(Owner);
            var amount = BitConverter.GetBytes(Amount);
            p[0] = (byte)t;
            for (var i = 1; i < owner.Length; i++)
                p[i] = owner[i];
            for (var i = 5; i < amount.Length; i++)
                p[i] = amount[i];

            return p;
        }
        public override byte[] Destruct()
        {

            return destruct(CmdType.Deposit);
        }
    }
}