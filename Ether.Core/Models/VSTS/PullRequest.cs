using Ether.Core.Models.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ether.Core.Models.VSTS
{
    [DebuggerDisplay("{Status} - {PullRequestId} {Title}")]
    public class PullRequest : BaseDto
    {
        public int PullRequestId { get; set; }
        public VSTSUser CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public IEnumerable<PullRequestReviewer> Reviewers { get; set; }
        public virtual IEnumerable<PullRequestIteration> Iterations { get; set; }
        public virtual IEnumerable<PullRequestThread> Threads { get; set; }

        public RepositoryInfo Repository { get; set; }

        public int IterationsCount => Iterations == null ? 0 : Iterations.Count();
        public int CommentsCount => Threads == null ? 0 : Threads.Count();

        public override bool Equals(object obj)
        {
            return Equals(obj as PullRequest);
        }

        public override bool Equals(BaseDto other)
        {
            if (other == null || !(other is PullRequest))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return PullRequestId == ((PullRequest)other).PullRequestId;
        }

        public override int GetHashCode()
        {
            return PullRequestId.GetHashCode();
        }
    }

    public class RepositoryInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ProjectInfo Project { get; set; }
    }

    public class ProjectInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
