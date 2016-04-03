using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Command
{
    class Login : ICommand
    {
        private int cardnumber;
        private short pin;

        public Login(int card, short pin)
        {
            cardnumber = card;
            this.pin = pin;
        }

        public Login(byte[] payload)
        {
            cardnumber = BitConverter.ToInt32(payload, 1);
            pin = BitConverter.ToInt16(payload, 5);
        }
        public override byte[] Destruct()
        {
            var payload = new byte[10];
            payload[0] = (byte)CmdType.Login;

            var cn = BitConverter.GetBytes(cardnumber);
            for (var i = 1; i < cn.Length; i++)
                payload[i] = cn[i];
            var pin = BitConverter.GetBytes(this.pin);
            for (var i = 5; i < pin.Length; i++)
                payload[i] = pin[i];

            return payload;
        }
    }
}
