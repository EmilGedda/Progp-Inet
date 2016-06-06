using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace InetServer.Account
{
    /// <summary>
    ///     A singleton class responsible for loading and storing Languages to disk.
    ///     This also updates the languages whenever a edit on disk occurs, through a FileSystemWatcher.
    /// </summary>
    internal static class AccountSerializer
    {
        private const string Dir = "accounts";
        private static readonly XmlSerializer Xs = new XmlSerializer(typeof(Account));

        /// <summary>
        ///     Load all accounts stored on disk
        /// </summary>
        /// <returns>The list of successfully loaded accounts</returns>
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

        /// <summary>
        ///     Save all accounts to disk
        /// </summary>
        /// <param name="accounts">The list of accounts to be saved</param>
        public static void SaveAccounts(List<Account> accounts)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);

            foreach (var a in accounts)
                using (var fs = new FileStream(Dir + "/" + a.Cardnumber + ".xml", FileMode.Create, FileAccess.ReadWrite)
                    )
                    Xs.Serialize(fs, a);
        }
    }
}
