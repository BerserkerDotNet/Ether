using System.Collections.Generic;
using Ether.ViewModels;
using Ether.ViewModels.Types;

namespace Ether.Vsts.Types
{
    public class FetchPullRequestsResult
    {
        public IEnumerable<PullRequestViewModel> PullRequests { get; set; }

        public IEnumerable<PullRequestJobDetails.PullRequestDetail> Details { get; set; }

        public IEnumerable<PullRequestJobDetails.ErrorDetail> Errors { get; set; }

        public IEnumerable<PullRequestJobDetails.TimeEntry> TimeLogs { get; set; }
    }
}
