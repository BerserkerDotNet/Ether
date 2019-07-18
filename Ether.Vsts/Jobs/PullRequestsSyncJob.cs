using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using Ether.Vsts.Types;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Jobs
{
    public class PullRequestsSyncJob : IJob
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PullRequestsSyncJob> _logger;

        public PullRequestsSyncJob(IMediator mediator, ILogger<PullRequestsSyncJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Execute(IReadOnlyDictionary<string, object> parameters)
        {
            var membersAndRepositories = await _mediator.RequestCollection<GetMembersAndRepositoriesOfAllProfiles, RepositoryInfo>();
            _logger.LogInformation("Found {Count} repositories to scan.", membersAndRepositories.Count());

            // TODO: Parallel?
            foreach (var repository in membersAndRepositories)
            {
                _logger.LogInformation("Fetching pull requests for {Repository}", repository.Name);
                var pullRequests = await _mediator.RequestCollection<FetchPullRequestsForRepository, PullRequestViewModel>(new FetchPullRequestsForRepository(repository));
                await _mediator.Execute(new SavePullRequests(pullRequests));
                _logger.LogInformation("Found {Count} pull requests for {Repository}", pullRequests.Count(), repository.Name);
            }
        }
    }

    public class DashboardQueriesSyncJob : IJob
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DashboardQueriesSyncJob> _logger;

        public DashboardQueriesSyncJob(IMediator mediator, ILogger<DashboardQueriesSyncJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Execute(IReadOnlyDictionary<string, object> parameters)
        {
            throw new KeyNotFoundException();
        }
    }
}
