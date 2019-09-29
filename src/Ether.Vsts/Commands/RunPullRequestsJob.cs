using System;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Vsts.Commands
{
    public class RunPullRequestsJob : ICommand
    {
        public Guid[] Members { get; set; }

        public bool IsReset { get; set; }
    }
}
