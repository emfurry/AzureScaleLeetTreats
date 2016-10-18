// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace AzureScaleLeetTreats.ShardUtilities
{
    /// <summary>
    /// Provides access to app.config settings, and contains advanced configuration settings.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Gets the server name for the Shard Map Manager database, which contains the shard maps.
        /// </summary>
        public static string ShardMapManagerServerName
        {
            get { return ServerName; }
        }

        /// <summary>
        /// Gets the database name for the Shard Map Manager database, which contains the shard maps.
        /// </summary>
        public static string ShardMapManagerDatabaseName
        {
            get
            {
                var sb = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["LeetTreats_smm"].ConnectionString);
                return sb.InitialCatalog;
            }
        }

        public static string AuthDatabaseName
        {
            get
            {
                return "LeetTreats_auth";
            }
        }

        /// <summary>
        /// Gets the name for the Shard Map that contains metadata for all the shards and the mappings to those shards.
        /// </summary>
        public static string ShardMapName
        {
            get { return "ModulusOfShopperID"; }
        }

        /// <summary>
        /// Gets the server name from the App.config file for shards to be created on.
        /// </summary>
        private static string ServerName
        {
            get
            {
                var sb = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["LeetTreats_smm"].ConnectionString);
                return sb.DataSource;
            }
        }

        /// <summary>
        /// Gets the edition to use for Shards and Shard Map Manager Database if the server is an Azure SQL DB server. 
        /// If the server is a regular SQL Server then this is ignored.
        /// </summary>
        public static string DatabaseEdition
        {
            get
            {
                return ConfigurationManager.AppSettings["DatabaseEdition"] ?? "Basic";
            }
        }

        /// <summary>
        /// Returns a connection string that can be used to connect to the specified server and database.
        /// </summary>
        public static string GetConnectionString(string serverName, string database)
        {
            var sb = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["LeetTreats_smm"].ConnectionString);
            sb.DataSource = serverName;
            sb.InitialCatalog = database;
            return sb.ConnectionString;
        }

        public static string GetShardMapManagerConnectionString()
        {
            return GetConnectionString(ShardMapManagerServerName, ShardMapManagerDatabaseName);
        }

        public static string GetShardGenericConnectionString()
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(GetShardMapManagerConnectionString());
            sb.DataSource = "";
            sb.InitialCatalog = "";
            return sb.ConnectionString;
        }

        public static string GetAuthConnectionString()
        {
            return GetConnectionString(ShardMapManagerServerName, AuthDatabaseName);
        }

        public static string GetShardDatabaseName(int shardIndex)
        {
            string prefix = Regex.Match(ShardMapManagerDatabaseName, @"(\w+)_\w+").Groups[1].Value;
            string shardDatabaseName = $"{prefix}_{shardIndex}";
            return shardDatabaseName;
        }

        /// <summary>
        /// Returns a connection string to use for Data-Dependent Routing and Multi-Shard Query,
        /// which does not contain DataSource or InitialCatalog.
        /// </summary>
        public static string GetCredentialsConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["LeetTreats_smm"].ConnectionString;
        }

        public static int ShardCount
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["ShardCount"]);
            }
        }
    }
}
