using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Messages
{
    /// <summary>
    ///     The Serializer responsible for serializing a Motd object
    /// </summary>
    public class MotdSerializer
    {
        private const string Filename = "motd.txt";
        private const string DefaultMotd = "Default MOTD";
        private string motd;

        /// <summary>
        ///     Load Motd and start a FileSystemWatcher to watch for changes in Filename
        /// </summary>
        public MotdSerializer()
        {
            LoadMotd();
            EnableWatcher();
        }

        /// <summary>
        ///     Load a motd from disk. If no file was found, a default Motd is supplied and saved.
        /// </summary>
        /// <returns>The current set Motd, if no earlier Motd was found a default one is used</returns>
        public string LoadMotd()
        {
            if (!File.Exists(Filename))
            {
                Logger.Info("No message of the day found, creating default...");
                File.WriteAllText(Filename, DefaultMotd);
                Logger.Info("Default message of the day created at " + Filename);
            }

            var fi = new FileInfo(Filename);
            if (fi.Length > 80)
                Logger.Warning("Message of the day longer than 80 characters, will be truncated");
            var buffer = new byte[80];
            using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
                fs.Read(buffer, 0, 80);
            motd = Encoding.UTF8.GetString(buffer);
            Logger.Info("Message of the day successfully loaded");
            return motd;
        }

        /// <summary>
        ///     Enables a watcher to watch for changes on the motd-file
        /// </summary>
        private void EnableWatcher()
        {
            var watcher = new FileSystemWatcher
            {
                Path = ".",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
                Filter = "motd.txt"
            };

            watcher.Changed += OnWatcherChanged;
        }

        /// <summary>
        ///     Try to lock the given file. Tries for 30 attempts, and fails if no lock was given.
        ///     This allows our watcher to wait a little for the previous locking process to exit cleanly.
        /// </summary>
        /// <param name="path">The file to lock</param>
        /// <returns></returns>
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
        ///     Triggers whenever the motd-file changes
        /// </summary>
        /// <param name="sender">The FileSystemWatcher which triggered this event</param>
        /// <param name="args">The Event arguments specific to this event</param>
        private async void OnWatcherChanged(object sender, FileSystemEventArgs args)
        {
            var fsw = (FileSystemWatcher) sender;
            fsw.EnableRaisingEvents = false;
            if (!await GetIdleFile(args.FullPath)) return;
            LoadMotd();
            Logger.Watcher("Message of the day updated");
            fsw.EnableRaisingEvents = true;
        }
    }
}
