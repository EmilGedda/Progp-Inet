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
            Console.WriteLine($" [WATCHER] Language {changed.Name} has been " + (affected == null ? "added" : "updated"));
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
            using (var xs = new MemoryStream())
            using (var xw = new XmlTextWriter(xs, Encoding.UTF8))
            {
                Xs.WriteObject(xw, lang);
                return xs.ToArray();
            }
        }

        public static Language FromByteArray(byte[] p, int offset)
        {
            using (var ms = new MemoryStream(p, offset, p.Length - offset))
            using (var xr = new XmlTextReader(ms))
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
    }
}
