using Ether.Core.Interfaces;
using Ether.Core.Configuration;
using Ether.Core.Models.DTO.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Core.Models.VSTS;
using System.Diagnostics;
using Ether.Core.Types;

namespace Ether.Core.Reporters
{
    public class PullRequestsReporter : ReporterBase
    {
        private readonly IVstsClientRepository _vstsRepository;
        private readonly IProgressReporter _progressReporter;
        private static readonly Guid _reporterId = Guid.Parse("e6f4ff5a-a71d-4706-8d34-9227c55c5644");
        private static readonly object _locker = new object();

        public PullRequestsReporter(IVstsClientRepository vstsRepository, IRepository repository, IProgressReporter progressReporter, IOptions<VSTSConfiguration> configuration, ILogger<PullRequestsReporter> logger)
            :base(repository, configuration, logger)
        {
            _vstsRepository = vstsRepository;
            _progressReporter = progressReporter;
        }

        public override string Name => "Pull Requests report";

        public override Guid Id => _reporterId;

        public override Type ReportType => typeof(PullRequestsReport);

        protected override Task<ReportResult> ReportInternal()
        {
            _logger.LogInformation($"Staring to query pull requests");
            var sw = Stopwatch.StartNew();
            var resultingPrs = new List<PullRequest>();
            _progressReporter.Report("Starting to collect data");

            Parallel.ForEach(Input.Repositories, repositoryInfo =>
            {
                var project = Input.GetProjectFor(repositoryInfo);
                FetchPullrequestsForRepository(resultingPrs, repositoryInfo.Name, project.Name);
            });

            var report = GetReport(resultingPrs);
            sw.Stop();
            _logger.LogInformation("Finished generating pull request report '{ReportName}'. Time {OperationTime}", report.ReportName, sw.Elapsed);
            return Task.FromResult<ReportResult>(report);
        }

        private void FetchPullrequestsForRepository(List<PullRequest> resultingPrs, string repositoryName, string projectName)
        {
            var sw = Stopwatch.StartNew();
            var totalTime = TimeSpan.FromMilliseconds(0);
            Parallel.ForEach(Input.Members, member => 
            {
                _progressReporter.Report($"Looking for {member.DisplayName} pull requests in {projectName}/{repositoryName}");
                var pullRequestsQuery = PullRequestQuery.New(Input.Query.StartDate)
                    .WithFilter(IsPullRequestMatch)
                    .WithParameter("creatorId", member.Id.ToString())
                    .WithParameter("status", "all");
                var pullRequests = _vstsRepository.GetPullRequests(projectName, repositoryName, pullRequestsQuery)
                    .GetAwaiter()
                    .GetResult();
                _logger.LogInformation("Fetched pull requests for '{Member}'. Time: {OperationTime}", member.Email, sw.Elapsed);
                totalTime += sw.Elapsed;
                sw.Restart();

                _progressReporter.Report($"Found {pullRequests.Count()} pull requests from {member.DisplayName} in {projectName}/{repositoryName}.", GetProgressStep());
                lock (_locker)
                {
                    resultingPrs.AddRange(pullRequests);
                }
            });
            sw.Stop();
            _logger.LogInformation("Finished fetching pull requests for '{Repository}'. Time: {OperationTime}", repositoryName, totalTime);
        }

        private bool IsPullRequestMatch(PullRequest pullRequest)
        {
            return IsActivePullRequest(pullRequest) ||
                (pullRequest.ClosedDate.HasValue && pullRequest.ClosedDate >= Input.Query.StartDate && pullRequest.ClosedDate <= Input.ActualEndDate);
        }
        private PullRequestsReport GetReport(List<PullRequest> resultingPrs)
        {
            var groupedResult = resultingPrs.GroupBy(p => p.CreatedBy);
            var report = new PullRequestsReport();
            report.IndividualReports = new List<PullRequestsReport.IndividualPRReport>(groupedResult.Count());
            foreach (var personResult in groupedResult)
            {
                _progressReporter.Report($"Aggregating data for {personResult.Key.DisplayName}", GetProgressStep());
                var individualReport = new PullRequestsReport.IndividualPRReport();
                individualReport.TeamMember = GetUserDisplayName(personResult.Key);
                individualReport.Completed = personResult.Count(IsCompletedPullRequest);
                individualReport.Abandoned = personResult.Count(IsAbandonedPullRequest);
                individualReport.Active = personResult.Count(IsActivePullRequest);
                individualReport.Created = personResult.Count(IsPullRequestCreatedInPeriod);
                individualReport.TotalIterations = personResult.AsParallel().Aggregate(0, (x, p) => x += p.IterationsCount);
                individualReport.TotalComments = personResult.AsParallel().Aggregate(0, (x, p) => x += p.CommentsCount);
                individualReport.AverageIterations = (double)individualReport.TotalIterations / (double)individualReport.TotalPullRequestsCount;
                individualReport.AverageComments = (double)individualReport.TotalComments / (double)individualReport.TotalPullRequestsCount;
                individualReport.CodeQuality = ((double)individualReport.TotalPullRequestsCount / individualReport.TotalIterations) * 100;
                var averagePullRequestLifetime = personResult.Where(IsCompletedPullRequest)
                    .Sum(r => (r.ClosedDate.Value - r.CreationDate).TotalSeconds) / individualReport.Completed;
                averagePullRequestLifetime = double.IsNaN(averagePullRequestLifetime) ? 0 : averagePullRequestLifetime;
                individualReport.AveragePRLifespan = TimeSpan.FromSeconds(averagePullRequestLifetime);
                report.IndividualReports.Add(individualReport);
            }

            return report;
        }

        private bool IsCompletedPullRequest(PullRequest pullRequest)
        {
            return string.Equals(pullRequest.Status, "completed", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAbandonedPullRequest(PullRequest pullRequest)
        {
            return string.Equals(pullRequest.Status, "abandoned", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsActivePullRequest(PullRequest pullRequest)
        {
            return string.Equals(pullRequest.Status, "active", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPullRequestCreatedInPeriod(PullRequest pullRequest)
        {
            return pullRequest.CreationDate >= Input.Query.StartDate && pullRequest.CreationDate <= Input.ActualEndDate;
        }

        private string GetUserDisplayName(VSTSUser user)
        {
            var memeber = Input.Members.SingleOrDefault(m => string.Equals(m.Email, user.UniqueName, StringComparison.OrdinalIgnoreCase));
            if (memeber == null)
                return user.DisplayName;

            return memeber.DisplayName;
        }

        private float GetProgressStep()
        {
            var totalSteps = (Input.Repositories.Count() * Input.Members.Count()) + Input.Members.Count();
            return 100.0F / totalSteps;
        }
    }
}
