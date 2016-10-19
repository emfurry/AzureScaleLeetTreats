using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureScaleLeetTreats.Data
{
    public class ElasticDbConfiguration : DbConfiguration
    {
        public ElasticDbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new DefaultExecutionStrategy());

            if (ConfigurationManager.ConnectionStrings["Development"] != null)
            {
                SetContextFactory<StoreDataContext>(() =>
                {
                    string connStr = ConfigurationManager.ConnectionStrings["Development"].ConnectionString;
                    return new StoreDataContext(connStr);
                });
            }
        }
    }
}
