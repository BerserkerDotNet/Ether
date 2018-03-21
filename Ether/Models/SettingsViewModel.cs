using System;

namespace Ether.Models
{
    public class SettingsViewModel
    {
        public Guid TeamProfile { get; set; }

        public bool DisableWorkitemsJob { get; set; }
        public TimeSpan? KeepLastWorkItems { get; set; }

        public bool DisablePullRequestsJob { get; set; }
        public TimeSpan? KeepLastPullRequests { get; set; }

        public TimeSpan? KeepLastReports { get; set; }
    }
}
