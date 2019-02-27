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
                    var job = (IJob)scope.ServiceProvider.GetService(jobType);
                    var jobName = jobType.Name;
                    var mediator = scope.ServiceProvider.GetService<IMediator>();
                    var sw = new Stopwatch();
                    var jobId = Guid.NewGuid();
                    sw.Restart();
                    try
                    {
                        await mediator.Execute(ReportJobState.GetRunning(jobId, jobName));
                        _logger.LogInformation($"Executing '{jobName}'");
                        await job.Execute(parameters);
                        await mediator.Execute(ReportJobState.GetSuccessful(jobId, jobName, sw.Elapsed));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error while executing '{jobName}'");
                        await mediator.Execute(ReportJobState.GetFailed(jobId, jobName, ex.Message, sw.Elapsed));
                    }

                    sw.Stop();
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
                var job = (IJob)scope.ServiceProvider.GetService(configuration.JobType);
                var jobName = configuration.JobType.Name;
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                var sw = new Stopwatch();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var jobId = Guid.NewGuid();
                    sw.Restart();
                    try
                    {
                        await mediator.Execute(ReportJobState.GetRunning(jobId, jobName));
                        _logger.LogInformation($"Executing '{jobName}'");
                        await job.Execute(new Dictionary<string, object>(0));
                        await mediator.Execute(ReportJobState.GetSuccessful(jobId, jobName, sw.Elapsed));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error while executing '{jobName}'");
                        await mediator.Execute(ReportJobState.GetFailed(jobId, jobName, ex.Message, sw.Elapsed));
                    }

                    sw.Stop();
                    _logger.LogInformation($"Finished executing '{jobName}' in {sw.Elapsed}. Next execution in {configuration.Period}");
                    await Task.Delay(configuration.Period, cancellationToken);
                }
            }
        }
    }
}
