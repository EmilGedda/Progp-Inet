using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Message
{
    public class Withdrawal : Deposit
    {
        public bool Valid { get; }

        public Withdrawal(int amount, bool valid) : base(amount)
        {
            Valid = valid;
        }

        public Withdrawal(byte[] payload) : base(payload)
        {
            Valid = payload[9] % 2 == 1;
        }

        public override byte[] Destruct()
        {
            byte[] payload = destruct(MessageType.Withdrawal);
            payload[9] = (byte)(Valid ? 1 : 0);
            return payload;
        }
    }
}
