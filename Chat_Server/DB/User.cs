using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chat_server.DB
{
    class User
    {
        public int  Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public DateTime RegDate { get; set; }
    }
}
