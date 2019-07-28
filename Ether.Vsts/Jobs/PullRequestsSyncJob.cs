using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Ether.Vsts.Commands;
using Ether.Vsts.Queries;
using Ether.Vsts.Types;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Jobs
{
    public class PullRequestsSyncJob : IJob
    {
        public const string MembersParameterName = "Members";

        private readonly IMediator _mediator;
        private readonly ILogger<PullRequestsSyncJob> _logger;

        public PullRequestsSyncJob(IMediator mediator, ILogger<PullRequestsSyncJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<JobDetails> Execute(IReadOnlyDictionary<string, object> parameters)
        {
            var details = new PullRequestJobDetails();
            var includeMembers = new Guid[0];
            if (parameters.ContainsKey(MembersParameterName))
            {
                includeMembers = (Guid[])parameters[MembersParameterName];
                if (includeMembers == null || !includeMembers.Any())
                {
                    _logger.LogWarning("Attempt to run PullRequestsSyncJob with empty list of members");
                }
            }

            var membersAndRepositories = await _mediator.RequestCollection<GetMembersAndRepositoriesOfAllProfiles, RepositoryInfo>(new GetMembersAndRepositoriesOfAllProfiles(includeMembers));
            _logger.LogInformation("Found {Count} repositories to scan.", membersAndRepositories.Count());

            // TODO: Parallel?
            var allPullRequests = new List<PullRequestViewModel>();
            var allPullRequestsDetails = new List<PullRequestJobDetails.PullRequestDetail>();
            var allPullRequestsErrors = new List<PullRequestJobDetails.ErrorDetail>();
            var allPullRequestsTimeLogs = new List<PullRequestJobDetails.TimeEntry>();
            foreach (var repository in membersAndRepositories)
            {
                var startTime = DateTime.UtcNow;
                try
                {
                    _logger.LogInformation("Fetching pull requests for {Repository}", repository.Name);
                    var result = await _mediator.Request<FetchPullRequestsForRepository, FetchPullRequestsResult>(new FetchPullRequestsForRepository(repository));
                    allPullRequestsDetails.AddRange(result.Details);
                    allPullRequests.AddRange(result.PullRequests);
                    allPullRequestsErrors.AddRange(result.Errors);
                    allPullRequestsTimeLogs.AddRange(result.TimeLogs);
                    _logger.LogInformation("Found {Count} pull requests for {Repository}", result.PullRequests.Count(), repository.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching pull requests for {Repository}", repository.Name);
                    allPullRequestsErrors.Add(new PullRequestJobDetails.ErrorDetail { Repository = repository.Name, Error = ex.Message });
                }

                allPullRequestsTimeLogs.Add(new PullRequestJobDetails.RepositoryTiemEntry { Repository = repository.Name, Start = startTime, End = DateTime.UtcNow });
            }

            await _mediator.Execute(new SavePullRequests(allPullRequests));
            await _mediator.Execute(new UpdateLastPullRequestFetchDate(allPullRequests));
            details.PullRequests = allPullRequestsDetails;
            details.Errors = allPullRequestsErrors;
            details.TimeLogs = allPullRequestsTimeLogs;

            return details;
        }
    }
}
