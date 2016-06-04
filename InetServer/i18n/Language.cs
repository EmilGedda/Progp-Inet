using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using InetServer.Message;

namespace InetServer.i18n
{
    [DataContract]
    public class Language : IMessage
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public Dictionary<Label, string> Mapping { get; set; }
        public string Get(Label lbl) => Mapping[lbl];

        public Language()
        {
            
        }

        public Language(byte[] p)
        {
            int len = BitConverter.ToInt32(p, 1);
            var l = LanguageSerializer.FromByteArray(p, 5, len);
            Name = l.Name;
            Code = l.Code;
            Mapping = l.Mapping;
        }
        public enum Label
        {
            Login,
            Exit,
            EnterCardnumber,
            EnterPin,
            Withdraw,
            Deposit,
            Transfer,
            EnterCode
        }

        public override byte[] Destruct()
        {
            byte[] l = LanguageSerializer.ToByteArray(this);
            byte[] p = new byte[l.Length + 5];
            p[0] = (byte) MessageType.Language;
            var len = BitConverter.GetBytes(l.Length);
            Buffer.BlockCopy(len, 0, p, 1, len.Length);
            Buffer.BlockCopy(l, 0, p, 5, l.Length);
            return p;
        }
    }
}
