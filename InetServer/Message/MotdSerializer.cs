using System;
using System.ComponentModel;
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
                Console.WriteLine("[INFO] No Message of the Day found, creating default...");
                File.WriteAllText(Filename, DefaultMotd);
                Console.WriteLine("[INFO] Default Message of the Day created at " + Filename);
            }

            var fi = new FileInfo(Filename);
            if (fi.Length > 80)
                Console.WriteLine("[WARNING] Message of the Day longer than 80 characters, will be cut!");

            byte[] buffer = new byte[80];
            using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
                fs.Read(buffer, 0, 80);
            motd = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("[INFO] Message of the Day successfully loaded.");
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
            if (!await GetIdleFile(args.FullPath)) return;
            LoadMotd();
            Console.WriteLine("[INFO] Message of the Day updated.");
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
