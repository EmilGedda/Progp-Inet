﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetServer
{
    internal class Program
    {
        private static void Main()
        {
            var s = new Server();
            s.Listen();
            //s.Poll();
            Console.ReadKey();
        }
    }
}
