namespace InzWebApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _new : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Travelers", "DateOfBirth", c => c.DateTime(nullable: false));
            DropColumn("dbo.Travelers", "Age");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Travelers", "Age", c => c.Int(nullable: false));
            DropColumn("dbo.Travelers", "DateOfBirth");
        }
    }
}
