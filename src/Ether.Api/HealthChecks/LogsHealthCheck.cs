using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ether.Api.HealthChecks
{
    public class LogsHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            const string seqUrl = "http://localhost:5341";

            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(seqUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Degraded("Seq is not responding!");
                }
            }
            catch
            {
                return HealthCheckResult.Degraded("Seq is not responding!");
            }

            return HealthCheckResult.Healthy("Seq is OK.");
        }
    }
}
