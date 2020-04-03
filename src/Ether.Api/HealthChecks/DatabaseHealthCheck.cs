using System;
using System.Threading;
using System.Threading.Tasks;
using Ether.Contracts.Types.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ether.Api.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IOptions<DbConfiguration> _dbConfig;

        public DatabaseHealthCheck(IOptions<DbConfiguration> dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var config = _dbConfig.Value;
            if (config is null)
            {
                return HealthCheckResult.Unhealthy("Database config is null");
            }

            if (string.IsNullOrEmpty(config.ConnectionString))
            {
                return HealthCheckResult.Unhealthy("Connection string is missing.");
            }

            if (string.IsNullOrEmpty(config.DbName))
            {
                return HealthCheckResult.Unhealthy("Database name is missing.");
            }

            try
            {
                var mongoClient = new MongoClient(config.ConnectionString);
                var mongoDb = mongoClient.GetDatabase(config.DbName);

                await (await mongoDb.ListCollectionNamesAsync(cancellationToken: cancellationToken)).FirstAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Could not establish a connection to the DB.", ex);
            }

            return HealthCheckResult.Healthy("DB is OK");
        }
    }
}
