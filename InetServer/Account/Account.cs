using System;

namespace InetServer.Account
{
    /// <summary>
    /// An account used to store savings and related info, also serializable to XML-format
    /// <seealso cref="AccountSerializer"/>
    /// </summary>
    [Serializable]
    public class Account
    {
        public int Cardnumber { get; set; }
        public short Pin { get; set; } 
        public int Savings { get; set; } 

        /// <summary>
        /// Creates a new Account
        /// </summary>
        /// <param name="cn">The cardnumber of the new account</param>
        /// <param name="pin">The pin code of the new account</param>
        /// <param name="savings">The initial savings of the account, defaults to zero</param>
        public Account(int cn, short pin, int savings = 0)
        {
            Cardnumber = cn;
            Pin = pin;
            Savings = savings;
        }

        /// <summary>
        /// Empty public constructor to enable easy serialization
        /// </summary>
        public Account()
        {
            
        }
    }
}
