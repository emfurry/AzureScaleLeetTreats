using Microsoft.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Management.Sql;
using Microsoft.WindowsAzure.Management.Sql.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerticalScaleSqlDatabase
{
    class Program
    {
        private static async Task<AuthenticationResult> GetAccessTokenAsync(string domainName, string clientId, string redirectUri)
        {
            AuthenticationContext authContext = new AuthenticationContext("https://login.microsoftonline.com/" + domainName);
            var parameters = new PlatformParameters(PromptBehavior.Always);
            string resource = "https://management.core.windows.net/";
            return await authContext.AcquireTokenAsync(resource, clientId, new Uri(redirectUri), parameters);
        }

        private static async Task<DatabaseUpdateResponse> SetAzureSqlPerformanceLevel(string subscriptionId, string token, string serverName, string databaseName, string edition, string performanceLevel)
        {
            SqlManagementClient sqlClient = new SqlManagementClient(new TokenCloudCredentials(subscriptionId, token));

            var dbInfo = await sqlClient.Databases.GetAsync(serverName, databaseName);

            var serviceLevels = await sqlClient.ServiceObjectives.ListAsync(serverName);
            var serviceLevel = serviceLevels.Where(sl => sl.Name == performanceLevel).Single();

            var databaseParameters = new DatabaseUpdateParameters
            {
                Name = dbInfo.Database.Name,
                Edition = edition,
                ServiceObjectiveId = serviceLevel.Id
            };

            return await sqlClient.Databases.UpdateAsync(serverName, databaseName, databaseParameters);
        }

        [DebuggerStepThrough]
        private static string GetConfig(string configName)
        {
            return ConfigurationManager.AppSettings[configName];
        }

        static void Main(string[] args)
        {
            string token = null;

            if (string.IsNullOrEmpty(token))
            {
                AuthenticationResult result = GetAccessTokenAsync(GetConfig("DomainName"), GetConfig("ClientId"), GetConfig("RedirectUri")).Result;
                token = result.AccessToken;
            }

            DatabaseUpdateResponse updateResponse = SetAzureSqlPerformanceLevel(GetConfig("SubscriptionId"), token, GetConfig("ServerName"), GetConfig("DatabaseName"), GetConfig("Edition"), GetConfig("PerformanceLevel")).Result;
        }
    }
}
