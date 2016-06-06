using System;
using System.Text;

namespace InetServer.Messages
{
    /// <summary>
    ///     Represents the Motd as a message
    /// </summary>
    public class Motd : Message
    {

        /// <summary>
        ///     Construct a Motd object from a given string, defaults to the empty string.
        /// </summary>
        /// <param name="msg">The Motd of chosing</param>
        public Motd(string msg = "")
        {
            Message = msg;
        }

        /// <summary>
        ///     Construct a Motd from a serialized version of this object
        /// </summary>
        /// <param name="p">The byte[] representing a Motd object</param>
        public Motd(byte[] p)
        {
            Message = Encoding.UTF8.GetString(p, 1, 80);
        }

        public string Message { get; }

        /// <summary>
        ///     Serialize this object to a byte[].
        ///     Used in communion with <see cref="Motd(byte[])" /> to serialize and deserialize Motd object.
        /// </summary>
        /// <returns>The byte[] corresponding to this object</returns>
        public override byte[] Destruct()
        {
            var m = Encoding.UTF8.GetBytes(Message);
            var b = new byte[81];
            b[0] = (byte) MessageType.Motd;
            Buffer.BlockCopy(m, 0, b, 1, m.Length > 80 ? 80 : m.Length);
            return b;
        }
    }
}
