namespace InetServer.Messages
{
    /// <summary>
    ///     A withdrawal message, inherits from Deposit and Message.
    ///     The last byte is used for validation, all other bytes are
    ///     as described in Deposit.
    /// </summary>
    public class Withdrawal : Deposit
    {
        /// <summary>
        ///     A withdrawal-transaction
        /// </summary>
        /// <param name="amount">The amount to withdraw?</param>
        /// <param name="valid">Whether the transaction should be considered valid or not</param>
        public Withdrawal(int amount, bool valid) : base(amount)
        {
            Valid = valid;
        }

        /// <summary>
        ///     Reconstruct a Withdrawal transaction from a byte-array
        /// </summary>
        /// <param name="payload">The given message to reconstruct from</param>
        public Withdrawal(byte[] payload) : base(payload)
        {
            Valid = payload[9]%2 == 1;
        }

        public bool Valid { get; }

        /// <summary>
        ///     Serialize a Withdrawal-transaction into a 10-byte array
        /// </summary>
        /// <returns>The binary serialized version of the transaction</returns>
        public override byte[] Destruct()
        {
            var payload = destruct(MessageType.Withdrawal);
            payload[9] = (byte) (Valid ? 1 : 0);
            return payload;
        }
    }
}
