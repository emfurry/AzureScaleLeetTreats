namespace AzureScaleLeetTreats.Data.Shoppers.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Shoppers",
                c => new
                    {
                        ShopperID = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 450),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ShopperID)
                .Index(t => t.UserName, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Shoppers", new[] { "UserName" });
            DropTable("dbo.Shoppers");
        }
    }
}
