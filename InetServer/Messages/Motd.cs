﻿using System;
using System.Text;

namespace InetServer.Messages
{
    class Motd : Message
    {
        public string Message { get; }

        public Motd()
        {
            MotdSerializer serializer = new MotdSerializer();
            Message = serializer.LoadMotd();
        }

        public Motd(string msg)
        {
            Message = msg;
        }

        public Motd(byte[] p)
        {
            Message = Encoding.UTF8.GetString(p, 1, p.Length-1);
        }

        public override byte[] Destruct()
        {
            var m = Encoding.UTF8.GetBytes(Message);
            var b = new byte[m.Length + 1];
            b[0] = (byte) MessageType.Motd;
            Buffer.BlockCopy(m, 0, b, 1, m.Length);
            return b;
        }
    }
}