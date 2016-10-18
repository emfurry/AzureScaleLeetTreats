namespace AzureScaleLeetTreats.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public class MigrationConfiguration : DbMigrationsConfiguration<StoreDataContext>
    {
        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = false;
        }
    }
}
