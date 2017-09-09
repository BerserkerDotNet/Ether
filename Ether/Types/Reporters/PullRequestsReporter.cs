using Ether.Interfaces;
using Ether.Types.Configuration;
using Ether.Types.Data;
using Ether.Types.DTO.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Types.Reporters
{
    public class PullRequestsReporter : ReporterBase
    {
        private readonly VSTSClient _client;
        private static readonly Guid _reporterId = Guid.Parse("e6f4ff5a-a71d-4706-8d34-9227c55c5644");

        public PullRequestsReporter(VSTSClient client, IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<PullRequestsReporter> logger)
            :base(repository, configuration, logger)
        {
            _client = client;
        }

        public override string Name => $"Completed Pull Requestes report";

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
                    var PRsUrl = $"https://{_configuration.InstanceName}.visualstudio.com/DefaultCollection/{project.Name}/_apis/git/repositories/{repo.Name}/pullRequests?api-version=3.0&creatorId={member.Id}&status=Completed&$top=100";
                    var prsResponse = await _client.ExecuteGet<PRResponse>(PRsUrl);
                    var prs = prsResponse.Value
                        .Where(p => p.CreationDate >= Input.Query.StartDate && p.CreationDate <= Input.Query.EndDate).ToList();

                    foreach (var pr in prs)
                    {
                        var iterationUrl = $"https://{_configuration.InstanceName}.visualstudio.com/DefaultCollection/{project.Name}/_apis/git/repositories/{repo.Name}/pullRequests/{pr.PullRequestId}/iterations?api-version=3.0";
                        var iterationsResponse = await _client.ExecuteGet<IterationsResponse>(iterationUrl);
                        pr.Iterations = iterationsResponse.Count;
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
                individualReport.TotalIterations = personResult.Aggregate(0, (x, p) => x += p.Iterations);
                individualReport.AverageIterations = (double)individualReport.TotalIterations / (double)individualReport.TotalPRs;
                individualReport.CodeQuality = ((double)individualReport.TotalPRs / individualReport.TotalIterations) * 100;
                report.IndividualReports.Add(individualReport);
            }


            return report;
        }


        private class PRResponse
        {
            public PullRequest[] Value { get; set; }
        }

        private class PullRequest
        {
            public int PullRequestId { get; set; }
            public string Author { get; set; }
            public DateTime CreationDate { get; set; }
            public string Title { get; set; }

            public int Iterations { get; set; }
        }

        private class IterationsResponse
        {
            public int Count { get; set; }
        }
    }
}
