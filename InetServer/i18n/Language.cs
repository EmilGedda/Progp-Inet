using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.i18n
{
    [DataContract]
    public class Language
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public Dictionary<Label, string> Mapping { get; set; }
        public string Get(Label lbl) => Mapping[lbl];

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
    }
}
