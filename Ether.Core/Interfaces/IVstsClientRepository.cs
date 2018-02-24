using Ether.Core.Models.VSTS;
using Ether.Core.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ether.Core.Interfaces
{
    public interface IVstsClientRepository
    {
        Task<IEnumerable<PullRequest>> GetPullRequests(string project, string repository, PullRequestQuery query);
        Task<IEnumerable<PullRequestIteration>> GetIterations(string project, string repository, int pullRequestId);
        Task<IEnumerable<PullRequestThread>> GetThreads(string project, string repository, int pullRequestId);
    }
}
