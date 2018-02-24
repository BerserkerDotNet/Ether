using Ether.Core.Configuration;
using Ether.Core.Interfaces;
using Ether.Core.Models.VSTS;
using Ether.Core.Models.VSTS.Response;
using Ether.Core.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Core.Data
{
    public class VstsClientRepository : IVstsClientRepository
    {
        private const string ApiVersion = "3.0-preview";
        private readonly IVSTSClient _client;
        private readonly IOptions<VSTSConfiguration> _configuration;
        private readonly ILogger<VstsClientRepository> _logger;

        public VstsClientRepository(IVSTSClient client, IOptions<VSTSConfiguration> configuration, ILogger<VstsClientRepository> logger)
        {
            _client = client;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<PullRequest>> GetPullRequests(string project, string repository, PullRequestQuery query)
        {
            var pullrequests = new List<PullRequest>();
            await FetchPullRequests(repository, project, pullrequests, query);
            return pullrequests;
        }

        public async Task<IEnumerable<PullRequestIteration>> GetIterations(string project, string repository, int pullRequestId)
        {
            var iterationUrl = VSTSApiUrl.Create(_configuration.Value.InstanceName)
                 .ForPullRequests(project, repository)
                 .WithSection(pullRequestId.ToString())
                 .WithSection("iterations")
                 .Build(ApiVersion);
            var iterationsResponse = await _client.ExecuteGet<ValueBasedResponse<PullRequestIteration>>(iterationUrl);

            return iterationsResponse.Value;
        }

        public async Task<IEnumerable<PullRequestThread>> GetThreads(string project, string repository, int pullRequestId)
        {
            var commentsUrl = VSTSApiUrl.Create(_configuration.Value.InstanceName)
                .ForPullRequests(project, repository)
                .WithSection(pullRequestId.ToString())
                .WithSection("threads")
                .Build(ApiVersion);
            var commentsResponse = await _client.ExecuteGet<ValueBasedResponse<PullRequestThread>>(commentsUrl);

            return commentsResponse.Value;
        }

        private async Task FetchPullRequests(string repositoryName, string projectName, List<PullRequest> pullrequests, PullRequestQuery query, int startFrom = 0)
        {
            var sw = Stopwatch.StartNew();
            var pullRequestsUrlBuilder = VSTSApiUrl.Create(_configuration.Value.InstanceName)
                .ForPullRequests(projectName, repositoryName);

            foreach (var parameter in query.Parameters)
            {
                pullRequestsUrlBuilder.WithQueryParameter(parameter.Key, parameter.Value);
            }
            pullRequestsUrlBuilder.WithQueryParameter("$skip", startFrom.ToString());
            var pullRequestsUrl = pullRequestsUrlBuilder.Build(ApiVersion);

            _logger.LogInformation($"Attempting to retrieve pull requests from {pullRequestsUrl}");
            var prsResponse = await _client.ExecuteGet<PullRequestsResponse>(pullRequestsUrl);
            _logger.LogInformation($"Retrieved {prsResponse.Value.Count()} PRs from '{repositoryName}' repository. Start position {startFrom}. Operation took: {sw.Elapsed}");

            sw.Restart();
            var prs = prsResponse.Value
                .Where(query.Filter)
                .ToList();
            sw.Stop();
            _logger.LogInformation($"Filtering PRs by range took {sw.Elapsed}. Count {prs.Count}");
            pullrequests.AddRange(prs);

            if (prs.Any() && prs.Min(p => p.CreationDate) > query.FromDate)
            {
                await FetchPullRequests(repositoryName, projectName, pullrequests, query, startFrom + prs.Count);
            }
        }

    }
}
