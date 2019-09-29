using System;

namespace Ether.ViewModels
{
    public class TeamMemberViewModel : ViewModelWithId
    {
        public string Email { get; set; }

        public string DisplayName { get; set; }

        public int WorkItemsCount { get; set; }

        public DateTime? LastWorkitemsFetchDate { get; set; }

        public DateTime? LastPullRequestsFetchDate { get; set; }
    }
}
