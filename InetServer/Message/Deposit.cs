using System;
using System.Collections.Generic;
using System.Linq;

namespace InetServer.Message
{
    public class Deposit : IMessage
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

        protected byte[] destruct(MessageType t)
        {
            var payload = new byte[10];
            payload[0] = (byte)t;
            var amount = BitConverter.GetBytes(Amount);
            for (var i = 1; i < amount.Length; i++)
                payload[i] = amount[i];

            return payload;
        }
        public override byte[] Destruct()
        {
            return destruct(MessageType.Deposit);
        }
    }
}