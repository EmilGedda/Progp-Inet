﻿using System;

namespace InetServer.Messages
{
    /// <summary>
    ///     A Deposit message.
    ///     First byte is as always the MessageType, byte[1::4] is an int32 with the amount to deposit.
    /// </summary>
    public class Deposit : Message
    {
        /// <summary>
        ///     Create a Deposit object with a given amount
        /// </summary>
        /// <param name="amount">The amount to deposit</param>
        public Deposit(int amount)
        {
            Amount = amount;
        }

        /// <summary>
        ///     Construct a Deposit object from a serialized byte array
        /// </summary>
        /// <param name="payload">The serialized version of a Deposit object</param>
        public Deposit(byte[] payload)
        {
            Amount = BitConverter.ToInt32(payload, 1);
        }

        public int Amount { get; }

        /// <summary>
        ///     Destructs this object to a byte array
        ///     <see cref="destruct" />
        /// </summary>
        /// <returns>
        ///     <see cref="Message.Destruct" />
        /// </returns>
        public override byte[] Destruct()
        {
            return destruct(MessageType.Deposit);
        }

        /// <summary>
        ///     <seealso cref="Message.Destruct" />
        ///     Takes a MessageType to enable Withdrawal to make use of the same destruct function.
        /// </summary>
        /// <param name="t">The type of Message to serialize</param>
        /// <returns>
        ///     <see cref="Message.Destruct" />
        /// </returns>
        protected byte[] destruct(MessageType t)
        {
            var payload = new byte[10];
            payload[0] = (byte) t;
            var amount = BitConverter.GetBytes(Amount);
            Buffer.BlockCopy(amount, 0, payload, 1, amount.Length);
            return payload;
        }
    }
}
