using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using InetServer.Messages;

namespace InetServer.i18n
{
    /// <summary>
    ///     A Language which may be used in menus but also to sending back and forth between a client and server
    /// </summary>
    [DataContract]
    public class Language : Message
    {
        /// <summary>
        ///     All labels needed by the Client. A label corresponds to a string in a given Language
        /// </summary>
        public enum Label
        {
            Login,
            Exit,
            EnterCardnumber,
            EnterPin,
            Withdraw,
            Deposit,
            Transfer,
            EnterCode,
            Language,
            InvalidInput,
            InvalidMenuItem,
            EnterAmount
        }

        /// <summary>
        ///     Empty public constructor is needed by the serializer
        /// </summary>
        public Language()
        {
        }

        /// <summary>
        ///     Construct a Language from a byte array
        /// </summary>
        /// <param name="p">The byte array containing the Language</param>
        public Language(byte[] p)
        {
            var len = BitConverter.ToInt32(p, 1);
            var l = LanguageSerializer.FromByteArray(p, 5, len);
            Name = l.Name;
            Code = l.Code;
            Mapping = l.Mapping;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Code { get; set; }

        // A dictionary containing all labels for a given language.
        [DataMember]
        public Dictionary<Label, string> Mapping { get; set; }

        /// <summary>
        ///     Destruct a Language into a byte array in preparation for sending to a Client
        /// </summary>
        /// <returns>The binary serialized version of the Language</returns>
        public override byte[] Destruct()
        {
            var l = LanguageSerializer.ToByteArray(this);
            var p = new byte[l.Length + 5];
            p[0] = (byte) MessageType.Language;
            var len = BitConverter.GetBytes(l.Length);
            Buffer.BlockCopy(len, 0, p, 1, len.Length);
            Buffer.BlockCopy(l, 0, p, 5, l.Length);
            return p;
        }

        public string this[Label lbl] => Mapping[lbl];
    }
}
