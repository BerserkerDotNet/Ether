using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Commands
{
    public class SavePullRequests : ICommand
    {
        public SavePullRequests(IEnumerable<VstsPullRequestViewModel> pullRequests)
        {
            PullRequests = pullRequests;
        }

        public IEnumerable<VstsPullRequestViewModel> PullRequests { get; private set; }
    }
}
