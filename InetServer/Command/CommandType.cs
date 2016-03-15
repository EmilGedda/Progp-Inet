using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer.Command
{
    public enum CmdType : byte
    {
        Login,
        Status,
        Motd,
        Deposit,
        Withdrawal
    }
}