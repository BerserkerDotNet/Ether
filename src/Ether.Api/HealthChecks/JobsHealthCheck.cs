using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ether.Api.Jobs;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ether.Api.HealthChecks
{
    public class JobsHealthCheck : IHealthCheck
    {
        private readonly IMediator _mediator;
        private readonly IEnumerable<JobConfiguration> _jobs;

        public JobsHealthCheck(IMediator mediator, IEnumerable<JobConfiguration> jobs)
        {
            _mediator = mediator;
            _jobs = jobs;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            const int days = 7;
            var logs = await _mediator.RequestCollection<GetJobLogsForLastNDays, JobLogViewModel>(new GetJobLogsForLastNDays(days));
            var failedCount = logs.Count(l => l.Result == JobExecutionResult.Failed);
            if (failedCount > 0)
            {
                return HealthCheckResult.Degraded($"There are {failedCount} job failures in the past {days} days");
            }

            foreach (var jobConfig in _jobs)
            {
                var jobsCount = logs.Count(l => l.JobType == jobConfig.JobType.Name);
                if (jobsCount == 0)
                {
                    return HealthCheckResult.Degraded($"{jobConfig.JobType.Name} has not run in the past 7 days");
                }
            }

            return HealthCheckResult.Healthy("Jobs are OK.");
        }
    }
}
