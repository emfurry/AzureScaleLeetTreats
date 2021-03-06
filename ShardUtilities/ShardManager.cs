﻿using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.Data.Model;
using Microsoft.Azure.SqlDatabase.ElasticScale.Query;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AzureScaleLeetTreats.ShardUtilities
{
    public static class ShardManager
    {
        private static ShardMapManager _shardMapManager;
        public static ShardMapManager ShardMapManager
        {
            get
            {
                if (_shardMapManager == null)
                    _shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(Configuration.GetShardMapManagerConnectionString(), ShardMapManagerLoadPolicy.Eager);

                return _shardMapManager;
            }
        }

        public static ListShardMap<int> ShardMap
        {
            get
            {
                return ShardMapManager.GetListShardMap<int>(Configuration.ShardMapName);
            }
        }

        public static IEnumerable<T> MultiShardQuery<T>(string sql, Func<IDataReader, T> projection, IDataParameter[] parameters = null)
        {
            IEnumerable<Shard> shards = ShardMap.GetShards();

            using (var conn = new MultiShardConnection(shards, Configuration.GetShardGenericConnectionString()))
            {
                using (MultiShardCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecutionOptions = MultiShardExecutionOptions.IncludeShardNameColumn;
                    cmd.ExecutionPolicy = MultiShardExecutionPolicy.CompleteResults;
                    cmd.CommandTimeout = 30;
                    using (MultiShardDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return projection(reader);
                        }
                    }
                }
            }
        }

        private static void MapShopper(Shopper shopper, Data.Shoppers.Model.Shopper authDbShopper)
        {
            shopper.ShopperID = authDbShopper.ShopperID;
            shopper.UserName = authDbShopper.UserName;
            shopper.CreateDate = authDbShopper.CreateDate;
        }

        private static Shopper CreateShopperInAuthenticationDatabase(Shopper shopper)
        {
            Data.Shoppers.Model.Shopper authDbShopper;
            using (var db = new Data.Shoppers.ShopperDataContext(Configuration.GetAuthConnectionString()))
            {
                authDbShopper = new Data.Shoppers.Model.Shopper
                {
                    UserName = shopper.UserName,
                    CreateDate = shopper.CreateDate
                };
                db.Shoppers.Add(authDbShopper);
                db.SaveChanges();
            }
            MapShopper(shopper, authDbShopper);
            return shopper;
        }

        private static void CreateShardMappingForShopper(int shopperId, ListShardMap<int> shardMap)
        {
            int normalUserShardCount = shardMap.GetShards().Count() - 1;

            int shardIndex = (shopperId - 1) % normalUserShardCount;
            var shard = shardMap.GetShard(new ShardLocation(Configuration.ShardMapManagerServerName, Configuration.GetShardDatabaseName(shardIndex)));

            shardMap.CreatePointMapping(shopperId, shard);
        }

        private static void CreateShopperInShard(Shopper shopper, ListShardMap<int> shardMap)
        {
            using (var db = CreateStoreDataContext(shopper.ShopperID))
            {
                db.Shoppers.Add(shopper);
                db.SaveChanges();
            }
        }

        public static Shopper GetShopperByUserName(string userName)
        {
            Data.Shoppers.Model.Shopper authDbShopper;
            using (var db = new Data.Shoppers.ShopperDataContext(Configuration.GetAuthConnectionString()))
            {
                authDbShopper = db.Shoppers.Where(s => s.UserName == userName).SingleOrDefault();
            }
            if (authDbShopper == null) return null;

            var shopper = new Shopper();
            MapShopper(shopper, authDbShopper);
            return shopper;
        }

        public static Shopper CreateShopper(Shopper shopper)
        {
            ListShardMap<int> shardMap = ShardMapManager.GetListShardMap<int>(Configuration.ShardMapName);

            using (var transaction = new TransactionScope())
            {
                shopper = CreateShopperInAuthenticationDatabase(shopper);

                CreateShardMappingForShopper(shopper.ShopperID, shardMap);

                CreateShopperInShard(shopper, shardMap);

                transaction.Complete();
            }

            return shopper;
        }

        public static StoreDataContext CreateStoreDataContext(int shopperId)
        {
            var conn = ShardMap.OpenConnectionForKey(shopperId, Configuration.GetShardGenericConnectionString(), ConnectionOptions.Validate);
            var context = new StoreDataContext(conn);
            return context;
        }
    }
}
