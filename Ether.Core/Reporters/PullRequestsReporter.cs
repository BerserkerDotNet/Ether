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

namespace Ether.Core.Reporters
{
    public class PullRequestsReporter : ReporterBase
    {
        private readonly IVSTSClient _client;
        private static readonly Guid _reporterId = Guid.Parse("e6f4ff5a-a71d-4706-8d34-9227c55c5644");

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
            var resultingPrs = new List<PullRequest>();
            foreach (var repo in Input.Repositories)
            {
                var project = Input.GetProjectFor(repo);
                foreach (var member in Input.Members)
                {
                    var prsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                        .ForPullRequests(project.Name, repo.Name)
                        .WithQueryParameter("creatorId", member.Id.ToString())
                        .WithQueryParameter("status", "Completed")
                        .Build();
                    var prsResponse = await _client.ExecuteGet<PRResponse>(prsUrl);
                    var prs = prsResponse.Value
                        .Where(p => p.ClosedDate >= Input.Query.StartDate && p.ClosedDate <= Input.Query.EndDate).ToList();

                    foreach (var pr in prs)
                    {
                        var iterationUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                            .ForPullRequests(project.Name, repo.Name)
                            .WithSection(pr.PullRequestId.ToString())
                            .WithSection("iterations")
                            .Build();
                        var commentsUrl = VSTSApiUrl.Create(_configuration.InstanceName)
                            .ForPullRequests(project.Name, repo.Name)
                            .WithSection(pr.PullRequestId.ToString())
                            .WithSection("threads")
                            .Build();
                        var iterationsResponse = await _client.ExecuteGet<CountBasedResponse>(iterationUrl);
                        var commentsResponse = await _client.ExecuteGet<CountBasedResponse>(commentsUrl);
                        pr.IterationsCount = iterationsResponse.Count;
                        pr.CommentsCount = commentsResponse.Count;
                        pr.Author = member.DisplayName;
                        resultingPrs.Add(pr);
                    }
                }
            }

            return GetReport(resultingPrs);
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
