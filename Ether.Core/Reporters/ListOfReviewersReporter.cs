using Ether.Core.Configuration;
using Ether.Core.Data;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Core.Reporters
{
    public class ListOfReviewersReporter : ReporterBase
    {
        private static readonly Guid _reporterId = Guid.Parse("c196c2e9-ac21-48bb-ab03-afd7437900db");
        private readonly IVSTSClient _client;

        public ListOfReviewersReporter(IVSTSClient client, IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<IReporter> logger) 
            : base(repository, configuration, logger)
        {
            _client = client;
        }

        public override string Name => "List of reviewers report";

        public override Guid Id => _reporterId;

        public override Type ReportType => typeof(ListOfReviewersReport);

        protected override async Task<ReportResult> ReportInternal()
        {
            const string apiVersion = "3.0-preview";
            var resultingReviewers = new List<VSTSUser>();
            var resultingPrs = new List<PullRequest>();
            var resultingComments = new List<PullRequestThread.Comment>();
            foreach (var repo in Input.Repositories)
            {
                var project = Input.GetProjectFor(repo);
                var prsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                    .ForPullRequests(project.Name, repo.Name)
                    .WithQueryParameter("status", "all")
                    .Build(apiVersion);
                var prsResponse = await _client.ExecuteGet<PRResponse>(prsUrl);
                var prs = prsResponse.Value
                    .Where(p => p.CreationDate >= Input.Query.StartDate && p.CreationDate <= Input.Query.EndDate)
                    .ToList();           

                resultingReviewers.AddRange(prs.SelectMany(p => p.Reviewers)
                    .Where(r => !r.IsContainer && r.Vote != 0 && !resultingReviewers.Contains(r))
                    .Distinct());

                foreach (var pr in prs)
                {
                    var commentsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                        .ForPullRequests(project.Name, repo.Name)
                        .WithSection(pr.PullRequestId.ToString())
                        .WithSection("threads")
                        .Build(apiVersion);
                    var commentsResponse = await _client.ExecuteGet<ValueBasedResponse<PullRequestThread>>(commentsUrl);
                    resultingComments.AddRange(commentsResponse.Value
                        .SelectMany(t => t.Comments)
                        .Where(c => c.CommentType != "system"));
                }

                resultingPrs.AddRange(prs);
            }

            resultingReviewers.AddRange(resultingComments.Select(c => c.Author).Distinct().Where(a => !resultingReviewers.Contains(a)));

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
                    NumberOfPRsVoted = resultingPrs.Count(p => p.Reviewers.Contains(reviewer)),
                    NumberOfComments = resultingComments.Count(c => c.Author.UniqueName == reviewer.UniqueName)
                };
                result.IndividualReports.Add(individualReport);
            }
            
            return result;
        }
    }
}
