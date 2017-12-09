using Ether.Core.Interfaces;
using Ether.Core.Configuration;
using Ether.Core.Data;
using Ether.Core.Models.DTO.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Core.Models.VSTS;
using Ether.Core.Models.VSTS.Response;
using System.Diagnostics;
using Ether.Core.Models.DTO;

namespace Ether.Core.Reporters
{
    public class PullRequestsReporter : ReporterBase
    {
        private readonly IVSTSClient _client;
        private static readonly Guid _reporterId = Guid.Parse("e6f4ff5a-a71d-4706-8d34-9227c55c5644");
        private static readonly object _locker = new object();

        public PullRequestsReporter(IVSTSClient client, IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<PullRequestsReporter> logger)
            :base(repository, configuration, logger)
        {
            _client = client;
        }

        public override string Name => $"Completed Pull Requests report";

        public override Guid Id => _reporterId;

        public override Type ReportType => typeof(PullRequestsReport);

        protected async override Task<ReportResult> ReportInternal()
        {
            _logger.LogInformation($"Staring to query pull requests");
            var sw = Stopwatch.StartNew();
            var resultingPrs = new List<PullRequest>();
            Parallel.ForEach(Input.Repositories, repo =>
            {
                var project = Input.GetProjectFor(repo);
                FetchPullrequestsForRepository(resultingPrs, repo, project);
            });

            var report = GetReport(resultingPrs);
            sw.Stop();
            _logger.LogInformation("Finished generating pull request report '{ReportName}'. Time {OperationTime}", report.ReportName, sw.Elapsed);
            return report;
        }

        private void FetchPullrequestsForRepository(List<PullRequest> resultingPrs, VSTSRepository repository, VSTSProject project)
        {
            var sw = Stopwatch.StartNew();
            var totalTime = TimeSpan.FromMilliseconds(0);
            Parallel.ForEach(Input.Members, member => 
            {
                var prsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                    .ForPullRequests(project.Name, repository.Name)
                    .WithQueryParameter("creatorId", member.Id.ToString())
                    .WithQueryParameter("status", "Completed")
                    .Build();
                var prsResponse = _client.ExecuteGet<PRResponse>(prsUrl)
                    .GetAwaiter()
                    .GetResult();
                var pullrequests = prsResponse.Value
                    .Where(p => p.ClosedDate >= Input.Query.StartDate && p.ClosedDate <= Input.ActualEndDate)
                    .ToList();
                _logger.LogInformation("Fetched pull requests for '{Member}'. Time: {OperationTime}", member.Email, sw.Elapsed);
                totalTime += sw.Elapsed;
                sw.Restart();

                FetchPullrequestsInformation(resultingPrs, pullrequests, repository, project, member);
            });
            sw.Stop();
            _logger.LogInformation("Finished fetching pull requests for '{Repository}'. Time: {OperationTime}", repository.Name, totalTime);
        }

        private void FetchPullrequestsInformation(List<PullRequest> resultingPrs, List<PullRequest> pullrequests, VSTSRepository repository, VSTSProject project, TeamMember member)
        {
            var sw = Stopwatch.StartNew();
            Parallel.ForEach(pullrequests, pr => 
            {
                var iterationUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                    .ForPullRequests(project.Name, repository.Name)
                    .WithSection(pr.PullRequestId.ToString())
                    .WithSection("iterations")
                    .Build();
                var commentsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                    .ForPullRequests(project.Name, repository.Name)
                    .WithSection(pr.PullRequestId.ToString())
                    .WithSection("threads")
                    .Build();
                var iterationsResponse = _client.ExecuteGet<CountBasedResponse>(iterationUrl)
                    .GetAwaiter()
                    .GetResult();
                var commentsResponse = _client.ExecuteGet<CountBasedResponse>(commentsUrl)
                    .GetAwaiter()
                    .GetResult();
                pr.IterationsCount = iterationsResponse.Count;
                pr.CommentsCount = commentsResponse.Count;
                pr.Author = member.DisplayName;
                lock (_locker)
                {
                    resultingPrs.Add(pr);
                }
            });
            sw.Stop();
            _logger.LogInformation("Finished fetching additional information for pull requests in '{Repository}'. Time: {OperationTime}",repository.Name, sw.Elapsed);
        }

        private PullRequestsReport GetReport(List<PullRequest> resultingPrs)
        {
            var groupedResult = resultingPrs.GroupBy(p => p.Author);
            var report = new PullRequestsReport();
            report.IndividualReports = new List<PullRequestsReport.IndividualPRReport>(groupedResult.Count());
            foreach (var personResult in groupedResult)
            {
                var individualReport = new PullRequestsReport.IndividualPRReport();
                individualReport.TeamMember = personResult.Key;
                individualReport.TotalPRs = personResult.Count();
                individualReport.TotalIterations = personResult.Aggregate(0, (x, p) => x += p.IterationsCount);
                individualReport.TotalComments = personResult.Aggregate(0, (x, p) => x += p.CommentsCount);
                individualReport.AverageIterations = (double)individualReport.TotalIterations / (double)individualReport.TotalPRs;
                individualReport.AverageComments = (double)individualReport.TotalComments / (double)individualReport.TotalPRs;
                individualReport.CodeQuality = ((double)individualReport.TotalPRs / individualReport.TotalIterations) * 100;
                individualReport.AveragePRLifespan = TimeSpan.FromSeconds(personResult.Sum(r => (r.ClosedDate.Value - r.CreationDate).TotalSeconds) / individualReport.TotalPRs);
                report.IndividualReports.Add(individualReport);
            }

            return report;
        }
    }
}
