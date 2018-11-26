using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Types;

namespace Ether.Vsts.Queries
{
    public class FetchPullRequestsForRepository : IQuery<IEnumerable<PullRequestViewModel>>
    {
        public FetchPullRequestsForRepository(RepositoryInfo info)
        {
            Repository = info;
        }

        public RepositoryInfo Repository { get; private set; }
    }
}
