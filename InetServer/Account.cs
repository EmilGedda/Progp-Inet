using System;

namespace InetServer
{
    public class Account
    {
        public int Cardnumber { get; private set; }
        public short Pin { get; private set; } 
        public int Savings { get; set; } 

        public Account(int cn, short pin, int savings = 0)
        {
            Cardnumber = cn;
            Pin = pin;
            Savings = savings;
        }
    }
}
