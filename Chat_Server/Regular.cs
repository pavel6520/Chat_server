using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Chat_server
{
    static class Regular
    {
        //TODO: разрешить "_" "-"
        static Regex Rlogin = new Regex("[^A-Za-z0-9]");
        static Regex Rpass = new Regex("[^A-Za-z0-9]");
        static Regex Remail = new Regex("^([a-z0-9_-]+\\.)*[a-z0-9_-]+@[a-z0-9_-]+(\\.[a-z0-9_-]+)*\\.[a-z]{2,6}$");

        /*public static void SetRegex(string login, string pass, string email)
        {
            Rlogin = new Regex(login);
            Rpass = new Regex(pass);
            Remail = new Regex(email);
        }*/

        public static bool CheckLogin(string login)
        {
            return login.Length >= 4 && login.Length <= 20 && !Rlogin.IsMatch(login);
        }

        public static bool CheckPass(string pass)
        {
            return pass.Length >= 4 && pass.Length <= 30 && !Rpass.IsMatch(pass);
        }

        public static bool CheckEmail(string email)
        {
            return Remail.IsMatch(email);
        }
    }
}
