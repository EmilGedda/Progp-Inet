using System;

namespace InetServer.Messages
{
    /// <summary>
    ///     Represents a statuscode as a message.
    ///     A Status will always be a response of a Message, capturing its success or failure or anything between.
    /// </summary>
    public class Status : Message
    {
        // The cardnumber of the sender, not really used at the moment
        private readonly int cardnumber;

        // The savings of the sender, after the transaction
        private readonly int savings;

        /// <summary>
        ///     Constructs a Status object
        /// </summary>
        /// <param name="acc">The issuer of the initial request</param>
        /// <param name="code">The response status of the initial request</param>
        public Status(Account.Account acc, StatusCode code)
        {
            cardnumber = acc?.Cardnumber ?? 0;
            savings = acc?.Savings ?? 0;
            Code = code;
        }

        /// <summary>
        ///     Construct a Status object from a serialized version sent by the client
        /// </summary>
        /// <param name="payload">The byte array which represent the Status object</param>
        public Status(byte[] payload)
        {
            Code = (StatusCode) Enum.Parse(typeof(StatusCode), payload[1].ToString());
            cardnumber = BitConverter.ToInt32(payload, 2);
            savings = BitConverter.ToInt32(payload, 6);
        }

        public StatusCode Code { get; }

        /// <summary>
        ///     Serialize a Status object to a byte[10].
        /// </summary>
        /// <returns>The serialized version of the object</returns>
        public override byte[] Destruct()
        {
            var payload = new byte[10];
            payload[0] = (byte) MessageType.Status;
            payload[1] = (byte) Code;

            var cn = BitConverter.GetBytes(cardnumber);
            for (var i = 2; i < cn.Length; i++)
                payload[i] = cn[i];

            var s = BitConverter.GetBytes(savings);
            for (var i = 6; i < s.Length; i++)
                payload[i] = s[i];

            return payload;
        }
    }

    /// <summary>
    ///     Represents all different StatusCodes possible
    /// </summary>
    public enum StatusCode : byte
    {
        Success, // Generic success
        Fail, // Unknown failure
        InvalidPin, // Login
        InvalidCode, // Withdrawal failure
        LoginSuccess, // Login success
        LoginFail, // Login failure
        NotLoggedIn, // Not logged in
        Acknowledge // No more response
    }
}
