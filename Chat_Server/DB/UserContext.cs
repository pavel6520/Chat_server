using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Chat_server.DB
{
    class UserContext : DbContext
    {
        public UserContext()
            : base("DbConnection")
        { }

        public DbSet<User> Users { get; set; }
    }
}
