using System.Collections.Generic;

namespace Ether.ViewModels
{
    public class WorkitemInformationViewModel
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string State { get; set; }

        public string Type { get; set; }

        public int Priority { get; set; }

        public UserReference AssignedTo { get; set; }

        public float Estimated { get; set; }

        public float Spent { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsOnHold { get; set; }

        public IEnumerable<WorkitemPullRequest> PullRequests { get; set; }
    }
}
