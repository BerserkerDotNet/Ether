using System;
using System.Collections.Generic;

namespace Ether.ViewModels.Types
{
    public class PullRequestJobDetails : JobDetails
    {
        public IEnumerable<ErrorDetail> Errors { get; set; }

        public IEnumerable<PullRequestDetail> PullRequests { get; set; }

        public IEnumerable<TimeEntry> TimeLogs { get; set; }

        public class ErrorDetail
        {
            public string Repository { get; set; }

            public string Member { get; set; }

            public string Error { get; set; }
        }

        public class PullRequestDetail
        {
            public string Repository { get; set; }

            public string Member { get; set; }

            public int PullRequestId { get; set; }

            public string PullRequestTitle { get; set; }

            public string PullRequestState { get; set; }
        }

        public class TimeEntry
        {
            public DateTime Start { get; set; }

            public DateTime End { get; set; }
        }

        public class MemberTimeEntry : TimeEntry
        {
            public string Repository { get; set; }

            public string Member { get; set; }
        }

        public class RepositoryTiemEntry : TimeEntry
        {
            public string Repository { get; set; }
        }
    }
}
