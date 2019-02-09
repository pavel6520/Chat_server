namespace Chat_server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Users", "Salt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Salt", c => c.String());
        }
    }
}
