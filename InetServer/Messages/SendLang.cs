using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InetServer.i18n;

namespace InetServer.Message
{
    class SendLang : IMessage
    {
        public Language.Label Key { get; }
        public string Value { get; }
        public SendLang(Language.Label key, string val)
        {
            Key = key;
            Value = val;
        }
        public override byte[] Destruct()
        {
            var str = Encoding.UTF8.GetBytes(Value);
            var p = new byte[str.Length + 2];
            p[0] = (byte)MessageType.Lang;
            p[1] = (byte)Key;
            Buffer.BlockCopy(str, 0, p, 0, str.Length);
            return p;
        }
    }
}
