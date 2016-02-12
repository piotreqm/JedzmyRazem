namespace InzWebApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FriendListRename : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.FriendLists", newName: "FriendListItems");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.FriendListItems", newName: "FriendLists");
        }
    }
}
