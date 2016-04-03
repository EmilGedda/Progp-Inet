using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InetClient
{
    public abstract class Menu
    {
        protected abstract string Title { get; }

        protected abstract List<MenuEntry> MenuList { get; }

        public void Print(string banner = null)
        {
            Console.Clear();
            Console.WriteLine();
            string t = $" {Title}";
            Console.WriteLine(t);
            Console.WriteLine(new string('-', t.Length));
            if(banner != null) Console.WriteLine(" " + banner);
            for (int i = 0; i < MenuList.Count; i++)
                Console.WriteLine($" [{i+1}] {MenuList[i].Label}");
            
            Console.WriteLine();
            Console.Write(" Enter option: ");
            Console.ReadKey(true);
        }

        protected class MenuEntry
        {
            public string Label;
            private Action action;

            public MenuEntry(string label, Action action)
            {
                Label = label;
                this.action = action;
            }
        }
    }
}
