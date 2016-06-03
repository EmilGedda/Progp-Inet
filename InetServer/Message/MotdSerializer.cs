using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Message
{
    public class MotdSerializer
    {
        private const string Filename = "motd.txt";
        private const string DefaultMotd = "Default MOTD";
        private string motd;

        public MotdSerializer()
        {
            LoadMotd();
            EnableWatcher();
        }

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
            byte[] buffer = new byte[80];
            using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
                fs.Read(buffer, 0, 80);
            motd = Encoding.UTF8.GetString(buffer);
            Logger.Info("Message of the day successfully loaded");
            return motd;
        }

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

        private async void OnWatcherChanged(object sender, FileSystemEventArgs args)
        {
            var fsw = (FileSystemWatcher)sender;
            fsw.EnableRaisingEvents = false;
            if (!await GetIdleFile(args.FullPath)) return;
            LoadMotd();
            Logger.Watcher("Message of the day updated");
            fsw.EnableRaisingEvents = true;
        }

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
    }
}
