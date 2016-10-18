using AzureScaleLeetTreats.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureScaleLeetTreats.Data
{
    public class StoreDataContext : DbContext
    {
        public StoreDataContext() : base("LeetTreats")
        {

        }

        public StoreDataContext(string connectionString) : base(connectionString)
        {

        }

        public virtual DbSet<Shopper> Shoppers { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
    }
}
