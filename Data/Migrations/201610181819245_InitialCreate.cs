namespace AzureScaleLeetTreats.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        ShopperID = c.Int(nullable: false),
                        ProductID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OrderID)
                .ForeignKey("dbo.Products", t => t.ProductID, cascadeDelete: true)
                .ForeignKey("dbo.Shoppers", t => t.ShopperID, cascadeDelete: true)
                .Index(t => t.ShopperID)
                .Index(t => t.ProductID);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ImageUrl = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ProductID);
            
            CreateTable(
                "dbo.Shoppers",
                c => new
                    {
                        ShopperID = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 450),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ShopperID)
                .Index(t => t.UserName, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "ShopperID", "dbo.Shoppers");
            DropForeignKey("dbo.Orders", "ProductID", "dbo.Products");
            DropIndex("dbo.Shoppers", new[] { "UserName" });
            DropIndex("dbo.Orders", new[] { "ProductID" });
            DropIndex("dbo.Orders", new[] { "ShopperID" });
            DropTable("dbo.Shoppers");
            DropTable("dbo.Products");
            DropTable("dbo.Orders");
        }
    }
}
