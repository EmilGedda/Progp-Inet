using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetClient
{
    class LoginMenu : Menu
    {
        protected override string Title => "Login menu";

        protected override List<MenuEntry> MenuList { get; } = new List<MenuEntry>
        {
            new MenuEntry("Login", () => Console.WriteLine("hej")),
            new MenuEntry("Exit", () => Environment.Exit(0))
        };
    }
}
