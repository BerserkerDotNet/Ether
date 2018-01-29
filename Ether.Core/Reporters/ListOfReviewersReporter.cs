using Ether.Core.Configuration;
using Ether.Core.Data;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Core.Models.VSTS.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Core.Reporters
{
    public class ListOfReviewersReporter : ReporterBase
    {
        private const string ApiVersion = "3.0-preview";
        private static readonly Guid _reporterId = Guid.Parse("c196c2e9-ac21-48bb-ab03-afd7437900db");
        private static readonly object _locker = new object();
        private readonly IVSTSClient _client;

        public ListOfReviewersReporter(IVSTSClient client, IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<ListOfReviewersReporter> logger) 
            : base(repository, configuration, logger)
        {
            _client = client;
        }

        public override string Name => "List of reviewers report";

        public override Guid Id => _reporterId;

        public override Type ReportType => typeof(ListOfReviewersReport);

        protected override async Task<ReportResult> ReportInternal()
        {
            var allReviewers = new List<VSTSUser>();
            var allPullrequests = new List<PullRequest>();
            var allComments = new List<PullRequestThread.Comment>();
            var swTotal = Stopwatch.StartNew();
            foreach (var repository in Input.Repositories)
            {
                var project = Input.GetProjectFor(repository);
                var pullrequests = await GetPullRequests(repository, project);
                allPullrequests.AddRange(pullrequests);
                allReviewers.AddRange(GetReviewers(pullrequests));
                GetComments(allComments, repository, project, pullrequests);
            }

            var sw = Stopwatch.StartNew();
            allReviewers.AddRange(allComments.Select(c => c.Author).Distinct());
            allReviewers = allReviewers
                .Distinct()
                .ToList();
            sw.Stop();
            _logger.LogInformation($"Extracting reviewers from comments. Time {sw.Elapsed}");

            var report = CreateReport(allReviewers, allPullrequests, allComments);
            swTotal.Stop();
            _logger.LogInformation($"Total time to generate report is {swTotal.Elapsed}");

            return report;
        }

        private async Task<List<PullRequest>> GetPullRequests(VSTSRepository repo, VSTSProject project)
        {
            var pullrequests = new List<PullRequest>();
            await FetchPullRequests(repo, project, pullrequests);
            return pullrequests;
        }

        private async Task FetchPullRequests(VSTSRepository repo, VSTSProject project, List<PullRequest> pullrequests, int startFrom = 0)
        {
            var sw = Stopwatch.StartNew();
            var prsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                .ForPullRequests(project.Name, repo.Name)
                .WithQueryParameter("status", "all")
                .WithQueryParameter("$skip", startFrom.ToString())
                .Build(ApiVersion);
            var prsResponse = await _client.ExecuteGet<PRResponse>(prsUrl);
            _logger.LogInformation($"Retrieved {prsResponse.Value.Count()} PRs from '{repo.Name}' repository. Start position {startFrom}. Operation took: {sw.Elapsed}");

            sw.Restart();
            var prs = prsResponse.Value
                .Where(p => p.CreationDate >= Input.Query.StartDate && p.CreationDate <= Input.Query.EndDate)
                .ToList();
            sw.Stop();
            _logger.LogInformation($"Filtering PRs took {sw.Elapsed}. Count {prs.Count}");
            pullrequests.AddRange(prs);

            if (prs.Any() && prs.Min(p => p.CreationDate) > Input.Query.StartDate)
            {
                await FetchPullRequests(repo, project, pullrequests, startFrom + prs.Count);
            }
        }

        private IEnumerable<PullRequestReviewer> GetReviewers(List<PullRequest> pullrequests)
        {
            return pullrequests.SelectMany(p => p.Reviewers)
                                .Where(r => !r.IsContainer && r.Vote != 0)
                                .Distinct();
        }

        private void GetComments(List<PullRequestThread.Comment> allComments, VSTSRepository repository, VSTSProject project, List<PullRequest> pullrequests)
        {
            var sw = Stopwatch.StartNew();
            Parallel.ForEach(pullrequests, (pr) =>
            {
                var commentsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                    .ForPullRequests(project.Name, repository.Name)
                    .WithSection(pr.PullRequestId.ToString())
                    .WithSection("threads")
                    .Build(ApiVersion);
                var commentsResponse = _client.ExecuteGet<ValueBasedResponse<PullRequestThread>>(commentsUrl)
                    .GetAwaiter()
                    .GetResult();
                var comments = commentsResponse.Value
                    .SelectMany(t => t.Comments)
                    .Where(c => c.CommentType != "system");
                lock (_locker)
                {
                    allComments.AddRange(comments);
                }
            });
            sw.Stop();
            _logger.LogInformation($"Found {allComments.Count} comments. Repository '{repository.Name}'. Operation took {sw.Elapsed}");
        }

        private ListOfReviewersReport CreateReport(List<VSTSUser> resultingReviewers, List<PullRequest> resultingPrs, List<PullRequestThread.Comment> allComments)
        {
            var sw = Stopwatch.StartNew();
            var result = new ListOfReviewersReport();
            result.IndividualReports = new List<ListOfReviewersReport.IndividualReviewerReport>(resultingReviewers.Count);
            result.NumberOfPullRequests = resultingPrs.Count;
            foreach (var reviewer in resultingReviewers)
            {
                if (!Input.Members.Any(m => m.Email == reviewer.UniqueName))
                    continue;

                var individualReport = new ListOfReviewersReport.IndividualReviewerReport
                {
                    DisplayName = reviewer.DisplayName,
                    UniqueName = reviewer.UniqueName,
                    NumberOfPRsVoted = resultingPrs.Count(p => p.Reviewers.Any(r => r.UniqueName == reviewer.UniqueName && r.Vote != 0)),
                    NumberOfComments = allComments.Count(c => c.Author.UniqueName == reviewer.UniqueName)
                };
                result.IndividualReports.Add(individualReport);
            }
            sw.Stop();
            _logger.LogInformation($"Report created in {sw.Elapsed}");
            return result;
        }

    }
}
