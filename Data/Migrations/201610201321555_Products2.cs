namespace AzureScaleLeetTreats.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Products2 : DbMigration
    {
        public override void Up()
        {
            string sql;
            sql = Util.GetEmbeddedResourceText("AzureScaleLeetTreats.Data.Migrations.Resources._201610201321555_Products2.01_UP.sql");
            Sql(sql);
        }

        public override void Down()
        {
            string sql;
            sql = Util.GetEmbeddedResourceText("AzureScaleLeetTreats.Data.Migrations.Resources._201610201321555_Products2.01_DN.sql");
            Sql(sql);
        }
    }
}
