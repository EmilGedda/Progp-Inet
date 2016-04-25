using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Message
{
    class Login : IMessage
    {
        public int Cardnumber { get; }
        public short Pin { get; }

        public Login(int card, short pin)
        {
            Cardnumber = card;
            Pin = pin;
        }

        public Login(byte[] payload)
        {
            Cardnumber = BitConverter.ToInt32(payload, 1);
            Pin = BitConverter.ToInt16(payload, 5);
        }
        public override byte[] Destruct()
        {
            var payload = new byte[10];
            payload[0] = (byte)MessageType.Login;

            var cn = BitConverter.GetBytes(Cardnumber);
            for (var i = 1; i < cn.Length; i++)
                payload[i] = cn[i];
            var pin = BitConverter.GetBytes(this.Pin);
            for (var i = 5; i < pin.Length; i++)
                payload[i] = pin[i];

            return payload;
        }
    }
}
