using System;

namespace InetServer.Account
{
    [Serializable]
    public class Account
    {
        public int Cardnumber { get; set; }
        public short Pin { get; set; } 
        public int Savings { get; set; } 

        public Account(int cn, short pin, int savings = 0)
        {
            Cardnumber = cn;
            Pin = pin;
            Savings = savings;
        }

        public Account()
        {
            
        }
    }
}
