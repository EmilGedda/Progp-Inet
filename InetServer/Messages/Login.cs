using System;

namespace InetServer.Messages
{
    /// <summary>
    /// A Login message, the client will only send these, and the server will only recieve these.
    /// </summary>
    public class Login : Message
    {
        /// <summary>
        /// Constructs a Login message
        /// </summary>
        /// <param name="card">The cardnumber entered by the user</param>
        /// <param name="pin">The pin entered by the user</param>
        public Login(int card, short pin)
        {
            Cardnumber = card;
            Pin = pin;
        }

        /// <summary>
        /// Constructs a Login message from a serialized message
        /// </summary>
        /// <param name="payload">The serialized version of the Login message</param>
        public Login(byte[] payload)
        {
            Cardnumber = BitConverter.ToInt32(payload, 1);
            Pin = BitConverter.ToInt16(payload, 5);
        }

        public int Cardnumber { get; }
        public short Pin { get; }

        /// <summary>
        /// <see cref="Message.Destruct"/>
        /// </summary>
        /// <returns><see cref="Message.Destruct"/></returns>
        public override byte[] Destruct()
        {
            var payload = new byte[10];
            payload[0] = (byte) MessageType.Login;

            var cn = BitConverter.GetBytes(Cardnumber);
            Buffer.BlockCopy(cn, 0, payload, 1, cn.Length);
            var pin = BitConverter.GetBytes(Pin);
            Buffer.BlockCopy(pin, 0, payload, 5, pin.Length);

            return payload;
        }
    }
}
