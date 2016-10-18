using AzureScaleLeetTreats.Data.Shoppers.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureScaleLeetTreats.Data.Shoppers
{
    public class ShopperDataContext : DbContext
    {
        public ShopperDataContext() : base("LeetTreats_auth")
        {

        }

        public ShopperDataContext(string connectionString) : base(connectionString)
        {

        }

        public virtual DbSet<Shopper> Shoppers { get; set; }
    }
}
