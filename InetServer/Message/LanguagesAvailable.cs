using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Message
{
    public class LanguagesAvailable : IMessage
    {
        public byte Count { get; }

        public LanguagesAvailable(byte[] payload)
        {
            Count = payload[1];
        }

        public LanguagesAvailable(byte count = 0)
        {
            Count = count;
        }

        // Empty placeholder, since the server sends all languages at once upon request.
        public override byte[] Destruct()
        {
            return new[] {(byte) MessageType.LangsAvailable, Count};
        }
    }
}
