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
        private readonly IVstsClientFactory _clientFactory;
        private readonly IMediator _mediator;
        private readonly ILogger<PullRequestsSyncJob> _logger;

        public PullRequestsSyncJob(IVstsClientFactory clientFactory, IMediator mediator, ILogger<PullRequestsSyncJob> logger)
        {
            _clientFactory = clientFactory;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Execute()
        {
            var membersAndRepositories = await _mediator.RequestCollection<GetMembersAndRepositoriesOfAllProfiles, RepositoryInfo>();
            _logger.LogInformation("Found {Count} repositories to scan.", membersAndRepositories.Count());

            // TODO: Parallel?
            foreach (var repository in membersAndRepositories)
            {
                _logger.LogInformation("Fetching pull requests for {Repository}", repository.Name);
                var pullRequests = await _mediator.RequestCollection<FetchPullRequestsForRepository, VstsPullRequestViewModel>(new FetchPullRequestsForRepository(repository));
                await _mediator.Execute(new SavePullRequests(pullRequests));
                _logger.LogInformation("Found {Count} pull requests for {Repository}", pullRequests.Count(), repository.Name);
            }
        }
    }
}
