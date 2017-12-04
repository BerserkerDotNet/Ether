using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ether.Core.Models.VSTS
{
    [DebuggerDisplay("{Status} - {PullRequestId} {Title}")]
    public class PullRequest
    {
        public int PullRequestId { get; set; }
        public string Author { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public IEnumerable<PullRequestReviewer> Reviewers { get; set; }

        public int IterationsCount { get; set; }
        public int CommentsCount { get; set; }
    }
}
