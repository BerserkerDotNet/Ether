using Ether.Core.Configuration;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Core.Types;
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
        private static readonly Guid _reporterId = Guid.Parse("c196c2e9-ac21-48bb-ab03-afd7437900db");
        private static readonly object _locker = new object();
        private readonly IVstsClientRepository _vstsRepository;

        public ListOfReviewersReporter(IVstsClientRepository vstsRepository, IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<ListOfReviewersReporter> logger) 
            : base(repository, configuration, logger)
        {
            _vstsRepository = vstsRepository;
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
                allReviewers.AddRange(GetAllReviewers(pullrequests));
                allComments.AddRange(GetComments(pullrequests));
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

        private async Task<IEnumerable<PullRequest>> GetPullRequests(VSTSRepository repositoryInfo, VSTSProject project)
        {
            var query = PullRequestQuery.New(Input.Query.StartDate)
                .WithFilter(p => p.CreationDate >= Input.Query.StartDate && p.CreationDate <= Input.Query.EndDate)
                .WithParameter("status", "all");
            return await _vstsRepository.GetPullRequests(project.Name, repositoryInfo.Name, query);
        }

        private IEnumerable<PullRequestReviewer> GetAllReviewers(IEnumerable<PullRequest> pullrequests)
        {
            var sw = Stopwatch.StartNew();
            var reviewers = pullrequests.AsParallel().SelectMany(p => p.Reviewers)
                                .Where(r => !r.IsContainer && r.Vote != 0)
                                .Distinct();
            sw.Stop();
            _logger.LogInformation($"Found {reviewers.Count()} reviewers in {sw.Elapsed}");

            return reviewers;
        }

        private IEnumerable<PullRequestThread.Comment> GetComments(IEnumerable<PullRequest> pullrequests)
        {
            var sw = Stopwatch.StartNew();
            var comments = new List<PullRequestThread.Comment>();
            Parallel.ForEach(pullrequests, p => 
            {
                // Threads are lazy loaded, so need to pre-fetch first and then add to collection
                var threads = p.Threads.ToList();
                lock (_locker)
                {
                    comments.AddRange(threads.SelectMany(t => t.Comments).Where(c => c.CommentType != "system"));
                }
            });
            sw.Stop();
            _logger.LogInformation($"Found {comments.Count()} comments in {sw.Elapsed}");
            return comments;
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
