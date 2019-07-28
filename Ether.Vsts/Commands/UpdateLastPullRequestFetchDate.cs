using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Commands
{
    public class UpdateLastPullRequestFetchDate : ICommand
    {
        public UpdateLastPullRequestFetchDate(IEnumerable<PullRequestViewModel> pullRequests)
        {
            PullRequests = pullRequests;
        }

        public IEnumerable<PullRequestViewModel> PullRequests { get; private set; }
    }
}
