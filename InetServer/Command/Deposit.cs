using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Command
{
    [Serializable]
    public class Deposit : ICommand
    {
        public int Owner { get; set; }
        public int Amount { get; set; }

        public CmdType Cmd => CmdType.Deposit;
        public byte[] destruct()
        {
            var payload = new byte[9];
            var owner = BitConverter.GetBytes(Owner);
            var amount = BitConverter.GetBytes(Amount);
            payload[0] = (byte)Cmd;
            for (int i = 1; i < owner.Length; i++)
                payload[i] = owner[i];
            for (int i = 5; i < amount.Length; i++)
                payload[i] = amount[i];

            return payload;
        }

        public void construct(byte[] payload)
        {
            Owner = BitConverter.ToInt32(payload, 0);
            Amount = BitConverter.ToInt32(payload, 4);
        }

        public void Execute(List<Account> accounts)
        {
            foreach (var a in accounts.Where(a => a.Cardnumber == Owner))
            {
                a.Savings += Amount;
                return;
            }
        }
    }
}
