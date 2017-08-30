using Ether.Interfaces;
using Ether.Types.Configuration;
using Ether.Types.Data;
using Ether.Types.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Types.Reporters
{
    public class PullRequestsReporter
    {
        private readonly VSTSClient _client;
        private readonly IRepository _repository;
        private readonly ILogger<PullRequestsReporter> _logger;
        private readonly VSTSConfiguration _configuration;

        public PullRequestsReporter(VSTSClient client, IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<PullRequestsReporter> logger)
        {
            _client = client;
            _repository = repository;
            _logger = logger;
            _configuration = configuration.Value;
        }

        public async Task<PullRequestsReport> ReportAsync(Guid profileId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(_configuration.AccessToken) || string.IsNullOrEmpty(_configuration.InstanceName))
                throw new ArgumentException("Configuration is missing.");

            var profile = await _repository.GetSingleAsync<Profile>(p => p.Id == profileId);
            if (profile == null)
                throw new ArgumentException("Selected profile was not found.");

            _logger.LogWarning("Report requested for {Profile} starting from {StartDate} until {EndDate}", profile.Name, startDate, endDate);

            var repositories = await _repository.GetAsync<VSTSRepository>(r => profile.Repositories.Contains(r.Id));
            var members = await _repository.GetAsync<TeamMember>(m => profile.Members.Contains(m.Id));

            var resultingPrs = new List<PullRequest>();
            foreach (var repo in repositories)
            {
                foreach (var member in members)
                {
                    var PRsUrl = $"https://{_configuration.InstanceName}.visualstudio.com/DefaultCollection/{repo.Project}/_apis/git/repositories/{repo.Name}/pullRequests?api-version=3.0&creatorId={member.Id}&status=Completed&$top=100";
                    var prsResponseText = _client.ExecuteGet(PRsUrl).Result;
                    var prs = JsonConvert.DeserializeObject<PRResponse>(prsResponseText).Value
                        .Where(p => p.CreationDate >= startDate && p.CreationDate <= endDate).ToList();

                    foreach (var pr in prs)
                    {
                        var iterationUrl = $"https://{_configuration.InstanceName}.visualstudio.com/DefaultCollection/{repo.Project}/_apis/git/repositories/{repo.Name}/pullRequests/{pr.PullRequestId}/iterations?api-version=3.0";
                        var iterationResponseText = _client.ExecuteGet(iterationUrl).Result;
                        var iterationsResponse = JsonConvert.DeserializeObject<IterationsResponse>(iterationResponseText);
                        pr.Iterations = iterationsResponse.Count;
                        pr.Author = member.DisplayName;
                        resultingPrs.Add(pr);
                    }
                }
            }

            var groupedResult = resultingPrs.GroupBy(p => p.Author);
            var report = new PullRequestsReport();
            report.Id = Guid.NewGuid();
            report.DateTaken = DateTime.Now;
            report.StartDate = startDate;
            report.EndDate = endDate;
            report.ProfileName = profile.Name;
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

            await _repository.CreateAsync(report);
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
