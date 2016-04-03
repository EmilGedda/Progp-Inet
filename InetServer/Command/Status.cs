﻿using System;

namespace InetServer.Command
{
    class Status : ICommand
    {
        private readonly int cardnumber;
        private readonly int savings;
        private readonly StatusCode code;

        public Status(Account acc, StatusCode code)
        {
            cardnumber = acc.Cardnumber;
            savings = acc.Savings;
            this.code = code;
        }
        public Status(byte[] payload)
        {
            code = (StatusCode)Enum.Parse(typeof(StatusCode), payload[1].ToString());
            cardnumber = BitConverter.ToInt32(payload, 2);
            savings = BitConverter.ToInt32(payload, 6);
        }
        public override byte[] Destruct()
        {
            var payload = new byte[10];
            payload[0] = (byte) Message.Status;
            payload[1] = (byte) code;

            var cn = BitConverter.GetBytes(cardnumber);
            for (var i = 2; i < cn.Length; i++)
                payload[i] = cn[i];

            var s = BitConverter.GetBytes(savings);
            for (var i = 6; i < s.Length; i++)
                payload[i] = s[i];

            return payload;
        }
        public enum StatusCode : byte
        {
            Success,        // Generic success
            Fail,           // Unknown failure
            InvalidPin,     // Login
            InvalidCode,    // Withdrawal failure
            NotLoggedIn,    // Not logged in
        }
    }
}
