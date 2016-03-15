using System.Collections.Generic;
using InetServer.Command;

namespace InetServer
{

    public interface ICommand
    {
        CmdType Cmd { get; }
        byte[] destruct();
        void construct(byte[] payload);

        void Execute(List<Account> accounts);
    }
}