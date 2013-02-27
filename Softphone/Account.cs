using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Softphone
{
    public class Account
    {
        public string user;
        public string password;

        public Account(string us, string pw)
        {
            this.user = us;
            this.password = pw;
        }

        public string md5Result(byte[] challenge)
        {
            byte[] pwbytes = InformationElement.utf8.GetBytes(password);
            byte[] dgsrc = new byte[challenge.Length + pwbytes.Length];
            Array.Copy(challenge, dgsrc, challenge.Length);
            Array.Copy(pwbytes, 0, dgsrc, challenge.Length, pwbytes.Length);
            return MD5Core.GetHashString(dgsrc);
        }
        public bool hasPassword { get { return password != null; } }
    }
}
