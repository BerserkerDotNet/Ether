using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ether.Contracts.Types.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Ether.Api.HealthChecks
{
    public class ADHealthCheck : IHealthCheck
    {
        private readonly IOptions<ADConfiguration> _adConfig;

        public ADHealthCheck(IOptions<ADConfiguration> adConfig)
        {
            _adConfig = adConfig;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var config = _adConfig.Value;
            if (config is null || string.IsNullOrEmpty(config.Name) || string.IsNullOrEmpty(config.Type))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("AD config is invalid", data: new Dictionary<string, object> { { "CurrentValue", config } }));
            }

            return Task.FromResult(HealthCheckResult.Healthy("AD config looks good.", data: new Dictionary<string, object> { { "CurrentValue", config } }));
        }
    }
}
