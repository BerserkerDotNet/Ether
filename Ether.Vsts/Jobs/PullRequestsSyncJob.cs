using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using Ether.Vsts.Types;

namespace Ether.Vsts.Jobs
{
    public class PullRequestsSyncJob : IJob
    {
        private readonly IVstsClientFactory _clientFactory;
        private readonly IMediator _mediator;

        public PullRequestsSyncJob(IVstsClientFactory clientFactory, IMediator mediator)
        {
            _clientFactory = clientFactory;
            _mediator = mediator;
        }

        public async Task Execute()
        {
            var membersAndRepositories = await _mediator.RequestCollection<GetMembersAndRepositoriesOfAllProfiles, RepositoryInfo>();

            // TODO: Parallel?
            foreach (var repository in membersAndRepositories)
            {
                var pullRequests = await _mediator.RequestCollection<FetchPullRequestsForRepository, VstsPullRequestViewModel>(new FetchPullRequestsForRepository(repository));
                await _mediator.Execute(new SavePullRequests(pullRequests));
            }
        }
    }
}
