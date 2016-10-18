using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.Data.Shoppers;
using AzureScaleLeetTreats.ShardUtilities;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            ShardMapManager shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(Configuration.GetShardMapManagerConnectionString(), ShardMapManagerLoadPolicy.Eager);
            ListShardMap<int> shardMap = shardMapManager.GetListShardMap<int>(Configuration.ShardMapName);
            int shardCount = shardMap.GetShards().Count();


            using (var transaction = new TransactionScope())
            {
                var shopper = new AzureScaleLeetTreats.Data.Shoppers.Model.Shopper()
                {
                    UserName = "emfurry2",
                    CreateDate = DateTime.UtcNow
                };
                using (var db = new ShopperDataContext(Configuration.GetAuthConnectionString()))
                {
                    db.Shoppers.Add(shopper);
                    db.SaveChanges();
                }

                int shardIndex = (shopper.ShopperID - 1) % shardCount;
                var shard = shardMap.GetShard(new ShardLocation(Configuration.ShardMapManagerServerName, Configuration.GetShardDatabaseName(shardIndex)));

                shardMap.CreatePointMapping(shopper.ShopperID, shard);

                var conn = shardMap.OpenConnectionForKey(shopper.ShopperID, Configuration.GetShardGenericConnectionString(), ConnectionOptions.Validate);
                using (var db = new StoreDataContext(conn))
                {
                    var shardShopper = new AzureScaleLeetTreats.Data.Model.Shopper
                    {
                        ShopperID = shopper.ShopperID,
                        UserName = shopper.UserName,
                        CreateDate = shopper.CreateDate
                    };
                    db.Shoppers.Add(shardShopper);
                    db.SaveChanges();
                }

                transaction.Complete();
            }
        }
    }
}
