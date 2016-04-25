using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Message
{
    public class LanguagesAvailable : IMessage
    {

        // Empty placeholder, since the server sends all languages at once upon request.
        public override byte[] Destruct()
        {
            return new[] {(byte) MessageType.LangsAvailable};
        }
    }
}
