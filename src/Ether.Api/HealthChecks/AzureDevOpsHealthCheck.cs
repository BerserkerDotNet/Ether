using System;
using System.Threading;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Ether.Vsts.Queries;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VSTS.Net;
using VSTS.Net.Models.Request;
using VSTS.Net.Types;

namespace Ether.Api.HealthChecks
{
    public class AzureDevOpsHealthCheck : IHealthCheck
    {
        private readonly IMediator _mediator;

        public AzureDevOpsHealthCheck(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var config = await _mediator.Request<GetVstsDataSourceConfiguration, VstsDataSourceViewModel>();
            if (config == null || !config.DefaultToken.HasValue)
            {
                return HealthCheckResult.Unhealthy("ADO default identity is not set.");
            }

            var identity = await _mediator.Request<GetIdentityById, IdentityViewModel>(new GetIdentityById { Id = config.DefaultToken.Value });
            if (identity == null || string.IsNullOrEmpty(identity.Token))
            {
                return HealthCheckResult.Unhealthy("Token is empty.");
            }

            if (identity.ExpirationDate < DateTime.UtcNow.AddDays(30))
            {
                return HealthCheckResult.Degraded("Default token will expire soon.");
            }

            if (identity.ExpirationDate < DateTime.UtcNow)
            {
                return HealthCheckResult.Unhealthy("Default token has expired.");
            }

            try
            {
                var client = VstsClient.Get(new OnlineUrlBuilderFactory(config.InstanceName), identity.Token);
                await client.ExecuteQueryAsync(WorkItemsQuery.Get(@"SELECT [System.Id] FROM WorkItems WHERE (ID < 1)"));
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Exception while fetching from ADO", ex);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
