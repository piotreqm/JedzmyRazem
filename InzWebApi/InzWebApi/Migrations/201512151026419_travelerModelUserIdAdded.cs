namespace InzWebApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class travelerModelUserIdAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Travelers", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Travelers", "UserId");
        }
    }
}
