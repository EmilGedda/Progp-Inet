using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Message
{
    public class Login : IMessage
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
            Buffer.BlockCopy(cn, 0, payload, 1, cn.Length);
            var pin = BitConverter.GetBytes(this.Pin);
            Buffer.BlockCopy(pin, 0, payload, 5, pin.Length);

            return payload;
        }
    }
}
