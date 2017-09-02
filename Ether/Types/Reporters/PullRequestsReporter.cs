using Ether.Interfaces;
using Ether.Types.Configuration;
using Ether.Types.Data;
using Ether.Types.DTO;
using Ether.Types.DTO.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Types.Reporters
{
    public class PullRequestsReporter : ReporterBase
    {
        private readonly VSTSClient _client;

        public PullRequestsReporter(VSTSClient client, IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<PullRequestsReporter> logger)
            :base(repository, configuration, logger)
        {
            _client = client;
        }

        public override string Name => $"Pull requests report";

        protected async override Task<ReportResult> ReportInternal(ReportInput input)
        {
            var resultingPrs = new List<PullRequest>();
            foreach (var repo in input.Repositories)
            {
                foreach (var member in input.Members)
                {
                    var PRsUrl = $"https://{_configuration.InstanceName}.visualstudio.com/DefaultCollection/{repo.Project}/_apis/git/repositories/{repo.Name}/pullRequests?api-version=3.0&creatorId={member.Id}&status=Completed&$top=100";
                    var prsResponseText = await _client.ExecuteGet(PRsUrl);
                    var prs = JsonConvert.DeserializeObject<PRResponse>(prsResponseText).Value
                        .Where(p => p.CreationDate >= input.Query.StartDate && p.CreationDate <= input.Query.EndDate).ToList();

                    foreach (var pr in prs)
                    {
                        var iterationUrl = $"https://{_configuration.InstanceName}.visualstudio.com/DefaultCollection/{repo.Project}/_apis/git/repositories/{repo.Name}/pullRequests/{pr.PullRequestId}/iterations?api-version=3.0";
                        var iterationResponseText = await _client.ExecuteGet(iterationUrl);
                        var iterationsResponse = JsonConvert.DeserializeObject<IterationsResponse>(iterationResponseText);
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
