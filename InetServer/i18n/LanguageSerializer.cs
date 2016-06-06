using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace InetServer.i18n
{
    public class LanguageSerializer
    {

        private const string Dir = "langs";
        private static readonly DataContractSerializer Xs = new DataContractSerializer(typeof(Language));
        private List<Language> langs;

        private static LanguageSerializer instance;

        private LanguageSerializer()
        {
            DefaultLangGen();
        }

        public static LanguageSerializer Instance => instance ?? (instance = new LanguageSerializer());

        public List<Language> LoadAccounts()
        {
            if(langs != null) return langs;
            
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
            EnableWatcher();
            return langs = Directory.GetFiles(Dir).Select(LoadSingle).ToList();
        }

        public static Language LoadSingle(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                return (Language)Xs.ReadObject(fs);
        }

        private void EnableWatcher()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var watcher = new FileSystemWatcher
            {
                Path = Dir,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
                Filter = "*.xml"
            };

            watcher.Changed += OnWatcherChanged;
        }

        private async void OnWatcherChanged(object sender, FileSystemEventArgs args)
        {
            var fsw = (FileSystemWatcher) sender;
            fsw.EnableRaisingEvents = false;
            if (!await GetIdleFile(args.FullPath)) return;
            var affected = langs.FirstOrDefault(l => l.Code == Path.GetFileNameWithoutExtension(args.Name));

            var changed = LoadSingle(args.FullPath);
            if (affected == null) langs.Add(changed);
            else {
                affected.Mapping = changed.Mapping;
                affected.Name = changed.Name;
            }
            Logger.Watcher($"Language {changed.Name} has been " + (affected == null ? "added" : "updated"));
            fsw.EnableRaisingEvents = true;
        }

        public static void SaveAccounts(List<Language> langs)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);

            foreach (var a in langs)
            {
                using (var fs = new FileStream(Dir + "/" + a.Code + ".xml", FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                using (var xw = new XmlTextWriter(fs, Encoding.UTF8))
                {
                    xw.Formatting = Formatting.Indented;
                    Xs.WriteObject(xw, a);
                }
            }
        }

        public static byte[] ToByteArray(Language lang)
        {
            StringBuilder sb = new StringBuilder();
            using (var xw = XmlWriter.Create(sb))
                Xs.WriteObject(xw, lang);
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public static Language FromByteArray(byte[] p, int offset, int count)
        {
            var str = Encoding.UTF8.GetString(p, offset, count);
            using (var sr = new StringReader(str))
            using (var xr = XmlReader.Create(sr))
                return (Language)Xs.ReadObject(xr);
        }

        private static async Task<bool> GetIdleFile(string path)
        {
            var attemptsMade = 0;

            while (attemptsMade <= 30)
            {
                try {
                    using (File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                        return true;
                } catch {
                    attemptsMade++;
                    await Task.Delay(50);
                }
            }

            return false;
        }

        private static void DefaultLangGen()
        {
            var defaultLangs = new List<Language>
            {
                new Language
                {
                    Name = "Svenska",
                    Code = "sv-SE",
                    Mapping = new Dictionary<Language.Label, string>
                    {
                        { Language.Label.Login,             "Logga in" },
                        { Language.Label.Exit,              "Avsluta" },
                        { Language.Label.EnterCardnumber,   "Skriv in kortnummer" },
                        { Language.Label.EnterPin,          "Skriv in PIN-kod" },
                        { Language.Label.Withdraw,          "Uttag" },
                        { Language.Label.Deposit,           "Insättning" },
                        { Language.Label.Transfer,          "Överföring" },
                        { Language.Label.EnterCode,         "Skriv in kod" },
                        { Language.Label.Language,          "Språk" },
                        { Language.Label.InvalidInput,      "Felaktig inmatning, vänligen försök igen" },
                        { Language.Label.InvalidMenuItem,   "Felaktigt menyval, vänligen försök igen" },
                        { Language.Label.EnterAmount,       "Skriv in summa" }
                    }
                },
                new Language
                {
                    Name = "English (US)",
                    Code = "en-US",
                    Mapping = new Dictionary<Language.Label, string>
                    {
                        { Language.Label.Login,             "Login" },
                        { Language.Label.Exit,              "Exit" },
                        { Language.Label.EnterCardnumber,   "Enter card number" },
                        { Language.Label.EnterPin,          "Enter PIN" },
                        { Language.Label.Withdraw,          "Withdraw" },
                        { Language.Label.Deposit,           "Deposit" },
                        { Language.Label.Transfer,          "Transfer" },
                        { Language.Label.EnterCode,         "Enter code" },
                        { Language.Label.Language,          "Language" },
                        { Language.Label.InvalidInput,      "Invalid input, please try again" },
                        { Language.Label.InvalidMenuItem,   "Invalid menu item chosen, please try again" },
                        { Language.Label.EnterAmount,       "Enter amount" }
                    }
                },
                new Language
                {
                    Name = "日本語",
                    Code = "ja-JP",
                    Mapping = new Dictionary<Language.Label, string>
                    {
                        { Language.Label.Login,             "ログイン" },
                        { Language.Label.Exit,              "エグジット" },
                        { Language.Label.EnterCardnumber,   "カード番号を入力してください" },
                        { Language.Label.EnterPin,          "PIN番号を入力してください" },
                        { Language.Label.Withdraw,          "撤退" },
                        { Language.Label.Deposit,           "保証金" },
                        { Language.Label.Transfer,          "移転" },
                        { Language.Label.EnterCode,         "コードを入力ししてださい" },
                        { Language.Label.Language,          "言語" },
                        { Language.Label.InvalidInput,      "無効入力, 再度お試しください" },
                        { Language.Label.InvalidMenuItem,   "無効な選択、再度お試しください" },
                        { Language.Label.EnterAmount,       "金額を入力してください"}
                    }
                },
                new Language
                {
                    Name = "Português (Brasil)",
                    Code = "pt-BR",
                    Mapping = new Dictionary<Language.Label, string>
                    {
                        { Language.Label.Login,             "Login" },
                        { Language.Label.Exit,              "Sair" },
                        { Language.Label.EnterCardnumber,   "Digite o número do cartão" },
                        { Language.Label.EnterPin,          "Digite o número PIN" },
                        { Language.Label.Withdraw,          "Retirar" },
                        { Language.Label.Deposit,           "Depositar" },
                        { Language.Label.Transfer,          "Transferir" },
                        { Language.Label.EnterCode,         "Digite o código" },
                        { Language.Label.Language,          "Língua" },
                        { Language.Label.InvalidInput,      "Entrada inválida, por favor, tente novamente" },
                        { Language.Label.InvalidMenuItem,   "Seleção inválida, por favor, tente novamente" },
                        { Language.Label.EnterAmount,       "Digite a quantidade" }
                    }
                }
            };

            SaveAccounts(defaultLangs);
        }
    }
}
