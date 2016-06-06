namespace InetServer.Messages
{
    /// <summary>
    ///     A message describing when a Client requests all Messages available.
    ///     The server responds with a LanguagesAvailable containing the count of the forthcoming Languages
    ///     and then proceeds to send said Languages.
    /// </summary>
    public class LanguagesAvailable : Message
    {
        /// <summary>
        ///     Construct a LanguagesAvailable from a byte[] message
        /// </summary>
        /// <param name="payload">The serialized version of a LanguagesAvailable message</param>
        public LanguagesAvailable(byte[] payload)
        {
            Count = payload[1];
        }

        /// <summary>
        ///     Create a LanguagesAvailable object
        /// </summary>
        /// <param name="count">The amount of languages the server is about to send, defaults to zero.</param>
        public LanguagesAvailable(byte count = 0)
        {
            Count = count;
        }

        public byte Count { get; }

        /// <summary>
        ///     <see cref="Message.Destruct" />
        /// </summary>
        /// <returns>
        ///     <see cref="Message.Destruct" />
        /// </returns>
        public override byte[] Destruct()
        {
            var buf = new byte[10];
            buf[0] = (byte) MessageType.LanguagesAvailable;
            buf[1] = Count;
            return buf;
        }
    }
}
