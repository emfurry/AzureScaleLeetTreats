namespace AzureScaleLeetTreats.Data.Shoppers.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public class MigrationConfiguration : DbMigrationsConfiguration<ShopperDataContext>
    {
        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "ShopperDataContext";
        }
    }
}
