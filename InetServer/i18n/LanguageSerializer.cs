using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InetServer.i18n
{
    /// <summary>
    ///     A singleton class responsible for loading and storing Languages to disk.
    ///     This also updates the languages whenever a edit on disk occurs, through a FileSystemWatcher.
    /// </summary>
    public class LanguageSerializer
    {
        private const string Dir = "langs";
        private static readonly DataContractSerializer Xs = new DataContractSerializer(typeof(Language));

        private static LanguageSerializer instance;
        private List<Language> langs;

        /// <summary>
        ///     Private constructor to enforce singleton pattern
        /// </summary>
        private LanguageSerializer()
        {
        }

        // Singleton instance
        public static LanguageSerializer Instance => instance ?? (instance = new LanguageSerializer());

        /// <summary>
        ///     Deserialize a byte array to a Language
        /// </summary>
        /// <param name="p">The byte array which contains the Langauge</param>
        /// <param name="offset">Where the Language start in the byte array</param>
        /// <param name="count">How long the Language is in the byte array</param>
        /// <returns>The deserialized Language</returns>
        public static Language FromByteArray(byte[] p, int offset, int count)
        {
            var str = Encoding.UTF8.GetString(p, offset, count);
            using (var sr = new StringReader(str))
            using (var xr = XmlReader.Create(sr))
                return (Language) Xs.ReadObject(xr);
        }

        /// <summary>
        ///     Load all Languages stored on disk.
        ///     Enables a watcher to update the languages live whenever a change on disk occurs.
        ///     TODO: Rename
        /// </summary>
        /// <returns>The List of all Languages loaded</returns>
        public List<Language> LoadAccounts()
        {
            if (langs != null) return langs;

            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
            EnableWatcher();
            return langs = Directory.GetFiles(Dir).Select(LoadSingle).ToList();
        }

        /// <summary>
        ///     Load a single language from disk.
        ///     This does not enable a watcher on the file.
        /// </summary>
        /// <param name="file">The file to load a Language from</param>
        /// <returns>The language loaded</returns>
        public static Language LoadSingle(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                return (Language) Xs.ReadObject(fs);
        }

        /// <summary>
        ///     Save a list of languages to xml-files on disk.
        ///     TODO: Rename
        /// </summary>
        /// <param name="langs">The list of languages to save</param>
        public static void SaveAccounts(List<Language> langs)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);

            foreach (var a in langs)
            {
                using (
                    var fs = new FileStream(Dir + "/" + a.Code + ".xml", FileMode.Create, FileAccess.ReadWrite,
                                            FileShare.Read))
                using (var xw = new XmlTextWriter(fs, Encoding.UTF8))
                {
                    xw.Formatting = Formatting.Indented;
                    Xs.WriteObject(xw, a);
                }
            }
        }

        /// <summary>
        ///     Serialize a Language to a byte array
        /// </summary>
        /// <param name="lang">The language to serialize</param>
        /// <returns>The serialized byte array representation of a Language</returns>
        public static byte[] ToByteArray(Language lang)
        {
            var sb = new StringBuilder();
            using (var xw = XmlWriter.Create(sb))
                Xs.WriteObject(xw, lang);
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <summary>
        ///     Enable a watcher to watch on disk changes to update loaded Languages
        /// </summary>
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


        /// <summary>
        ///     Try to lock the given file. Tries for 30 attempts, and fails if no lock was given.
        ///     This allows our watcher to wait a little for the previous locking process to exit cleanly.
        /// </summary>
        /// <param name="path">The file to lock</param>
        /// <returns>The task containing the bool whether we could get an exclusive lock or not</returns>
        private static async Task<bool> GetIdleFile(string path)
        {
            var attemptsMade = 0;

            while (attemptsMade <= 30)
            {
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                        return true;
                }
                catch
                {
                    attemptsMade++;
                    await Task.Delay(50);
                }
            }

            return false;
        }

        /// <summary>
        ///     The event-based method which triggers whenever a Language-file changes
        /// </summary>
        /// <param name="sender">The FileSystemWatcher which triggered the event</param>
        /// <param name="args">The EventArgs which contains information about the event</param>
        private async void OnWatcherChanged(object sender, FileSystemEventArgs args)
        {
            var fsw = (FileSystemWatcher) sender;
            fsw.EnableRaisingEvents = false;
            if (!await GetIdleFile(args.FullPath)) return;
            var affected = langs.FirstOrDefault(l => l.Code == Path.GetFileNameWithoutExtension(args.Name));

            var changed = LoadSingle(args.FullPath);
            if (affected == null) langs.Add(changed);
            else
            {
                affected.Mapping = changed.Mapping;
                affected.Name = changed.Name;
            }
            Logger.Watcher($"Language {changed.Name} has been " + (affected == null ? "added" : "updated"));
            fsw.EnableRaisingEvents = true;
        }
    }
}
