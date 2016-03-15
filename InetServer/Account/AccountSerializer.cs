using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace InetServer
{
    internal static class AccountSerializer
    {
        private static readonly XmlSerializer Xs = new XmlSerializer(typeof(Account));
        private const string Dir = "accounts";

        public static List<Account> LoadAccounts()
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);

            var accounts = new List<Account>();

            foreach (var file in Directory.GetFiles(Dir))
                using (var fs = new FileStream(file, FileMode.Open))
                    accounts.Add((Account) Xs.Deserialize(fs));

            return accounts;
        }

        public static void SaveAccounts(List<Account> accounts)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);

            foreach (var a in accounts)
                using (var fs = new FileStream(Dir + "/" + a.Cardnumber + ".xml", FileMode.Create))
                    Xs.Serialize(fs, a);
        }
    }
}
