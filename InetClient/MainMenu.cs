using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetClient
{
    class MainMenu : Menu
    {
        protected override string Title => "Main menu";

        protected override List<string> Labels => new List<string>
        {
            "Deposit",
            "Withdrawal",
            "Transfer",
            "Change language",
            "Log out"

        };

    }
}
