using System;
using System.Collections.Generic;
using System.Linq;

namespace InetServer.Command
{
    public class Deposit : ICommand
    {

        public int Owner { get; }
        public int Amount { get; set; }

        protected override CmdType Cmd => CmdType.Deposit;

        public Deposit(byte[] payload) : base(payload)
        {
            Owner = BitConverter.ToInt32(payload, 1);
            Amount = BitConverter.ToInt32(payload, 5);
        }

        public override byte[] Destruct()
        {
            var payload = new byte[9];
            var owner = BitConverter.GetBytes(Owner);
            var amount = BitConverter.GetBytes(Amount);
            payload[0] = (byte) Cmd;
            for (var i = 1; i < owner.Length; i++)
                payload[i] = owner[i];
            for (var i = 5; i < amount.Length; i++)
                payload[i] = amount[i];

            return payload;
        }

        public override void Execute(List<Account> accounts)
        {
            foreach (var a in accounts.Where(a => a.Cardnumber == Owner))
            {
                a.Savings += Amount;
                return;
            }
        }
    }
}