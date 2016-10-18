using AzureScaleLeetTreats.Data;
using AzureScaleLeetTreats.Data.Shoppers;
using AzureScaleLeetTreats.ShardUtilities;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.Schema;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseConfigurator
{
    class Program
    {
        private static void RegisterShardingSchema(ShardMapManager shardMapManager, string shardMapName)
        {
            var schemaInfo = new SchemaInfo();
            schemaInfo.Add(new ReferenceTableInfo("Products")); // this table contains rows that should exist in ALL shards
            schemaInfo.Add(new ShardedTableInfo("Shoppers", "ShopperID")); // shoppers will be partitioned to different shards based on their primary key
            schemaInfo.Add(new ShardedTableInfo("Orders", "ShopperID")); // shoppers' orders will also be partitioned to different shards based on whose order it is

            shardMapManager.GetSchemaInfoCollection().Add(shardMapName, schemaInfo);
        }

        private static void InitializeShard(string shardDatabaseName)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<StoreDataContext, AzureScaleLeetTreats.Data.Migrations.MigrationConfiguration>(true));

            string connectionString = Configuration.GetConnectionString(Configuration.ShardMapManagerServerName, shardDatabaseName);
            using (var db = new StoreDataContext(connectionString))
            {
                var products = db.Products.ToArray();
            }
        }

        private static void CreateAndInitializeShard(ShardMapManager shardMapManager, int shardIndex)
        {
            string shardDatabaseName = Configuration.GetShardDatabaseName(shardIndex);

            // Create database
            if (!SqlDatabaseUtils.DatabaseExists(Configuration.ShardMapManagerServerName, shardDatabaseName))
                SqlDatabaseUtils.CreateDatabase(Configuration.ShardMapManagerServerName, shardDatabaseName);

            // Run EF migrations
            InitializeShard(shardDatabaseName);

            ShardMap shardMap = shardMapManager.GetShardMap(Configuration.ShardMapName);

            // Register shard with SMM
            Shard shard;
            ShardLocation shardLocation = new ShardLocation(Configuration.ShardMapManagerServerName, shardDatabaseName);
            if (!shardMap.TryGetShard(shardLocation, out shard))
                shard = shardMap.CreateShard(shardLocation);
        }

        private static void CreateAndInitializeAuthDatabase()
        {
            if (!SqlDatabaseUtils.DatabaseExists(Configuration.ShardMapManagerServerName, Configuration.AuthDatabaseName))
                SqlDatabaseUtils.CreateDatabase(Configuration.ShardMapManagerServerName, Configuration.AuthDatabaseName);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ShopperDataContext, AzureScaleLeetTreats.Data.Shoppers.Migrations.MigrationConfiguration>(true));

            using (var db = new ShopperDataContext(Configuration.GetAuthConnectionString()))
            {
                db.Shoppers.ToArray();
            }
        }

        static private ShardMapManager CreateAndConfigureShardMapManager()
        {
            // Get or create the Shard Map Manager database
            bool smmDbExists = SqlDatabaseUtils.DatabaseExists(Configuration.ShardMapManagerServerName, Configuration.ShardMapManagerDatabaseName);
            if (!smmDbExists)
                SqlDatabaseUtils.CreateDatabase(Configuration.ShardMapManagerServerName, Configuration.ShardMapManagerDatabaseName);

            // Get or create the Shard Map Manager from the SMM database
            ShardMapManager shardMapManager;
            bool smmExists = ShardMapManagerFactory.TryGetSqlShardMapManager(Configuration.GetShardMapManagerConnectionString(), ShardMapManagerLoadPolicy.Eager, out shardMapManager);
            if (!smmExists)
                shardMapManager = ShardMapManagerFactory.CreateSqlShardMapManager(Configuration.GetShardMapManagerConnectionString());

            // We will be using a ListShardMap to implement a round-robin sharding scheme
            ListShardMap<int> shardMap;
            if (!shardMapManager.TryGetListShardMap(Configuration.ShardMapName, out shardMap))
                shardMap = shardMapManager.CreateListShardMap<int>(Configuration.ShardMapName);

            // Register which database tables and columns are used to partition the data between shards
            // Registering the schema allows you to use the SplitMerge tool to automatically move sharded data between shards
            if (shardMapManager.GetSchemaInfoCollection().Count() == 0)
                RegisterShardingSchema(shardMapManager, Configuration.ShardMapName);

            return shardMapManager;
        }

        static void Main(string[] args)
        {
            ShardMapManager shardMapManager = CreateAndConfigureShardMapManager();

            CreateAndInitializeAuthDatabase();

            for (int x = 0; x < Configuration.ShardCount; x++)
                CreateAndInitializeShard(shardMapManager, x);
        }
    }
}
