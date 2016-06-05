using System;

namespace InetServer.Message
{
    public class Status : IMessage
    {
        private readonly int cardnumber;
        public StatusCode Code { get; }
        private readonly int savings;

        public Status(Account acc, StatusCode code)
        {
            cardnumber = acc?.Cardnumber ?? 0;
            savings = acc?.Savings ?? 0;
            Code = code;
        }

        public Status(byte[] payload)
        {
            Code = (StatusCode) Enum.Parse(typeof (StatusCode), payload[1].ToString());
            cardnumber = BitConverter.ToInt32(payload, 2);
            savings = BitConverter.ToInt32(payload, 6);
        }

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