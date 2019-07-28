using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ether.Api.Jobs
{
    public class JobsHostedService : IHostedService, IDisposable, IJobRunner
    {
        private readonly IServiceProvider _services;
        private readonly IEnumerable<JobConfiguration> _jobs;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        public JobsHostedService(IServiceProvider services, IEnumerable<JobConfiguration> jobs, ILogger<JobsHostedService> logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _services = services;
            _jobs = jobs;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Jobs Background Service is starting.");

            if (!_jobs.Any())
            {
                _logger.LogWarning("No jobs found to process.");
                return Task.CompletedTask;
            }

            foreach (var job in _jobs)
            {
                Task.Factory.StartNew(j => RunJobRecurrent((JobConfiguration)j, _cancellationTokenSource.Token), job, TaskCreationOptions.LongRunning);
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Jobs Background Service is stopping.");
            using (var scope = _services.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                await mediator.Execute(new AbortActiveJobs());
                _cancellationTokenSource.Cancel();
            }
        }

        public Task RunJob<T>(Dictionary<string, object> parameters)
            where T : IJob
        {
            return Task.Factory.StartNew(async () =>
            {
                using (var scope = _services.CreateScope())
                {
                    var jobType = typeof(T);
                    await RunJob(scope, jobType, parameters);
                }
            });
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private async Task RunJobRecurrent(JobConfiguration configuration, CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                var jobName = configuration.JobType.Name;
                while (!cancellationToken.IsCancellationRequested)
                {
                    await RunJob(scope, configuration.JobType, new Dictionary<string, object>(0));
                    _logger.LogInformation($"Finished executing '{jobName}'. Next execution in {configuration.Period}");
                    await Task.Delay(configuration.Period, cancellationToken);
                }
            }
        }

        private async Task RunJob(IServiceScope scope, Type jobType, Dictionary<string, object> parameters)
        {
            var job = (IJob)scope.ServiceProvider.GetService(jobType);
            var jobName = jobType.Name;
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            var jobId = Guid.NewGuid();
            var startTime = DateTime.UtcNow;
            try
            {
                await mediator.Execute(ReportJobState.GetRunning(jobId, jobName, startTime));
                _logger.LogInformation($"Executing '{jobName}'");
                var details = await job.Execute(parameters);
                await mediator.Execute(ReportJobState.GetSuccessful(jobId, jobName, startTime, DateTime.UtcNow, details));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while executing '{jobName}'");
                await mediator.Execute(ReportJobState.GetFailed(jobId, jobName, ex.Message, startTime, DateTime.UtcNow));
            }
        }
    }
}
