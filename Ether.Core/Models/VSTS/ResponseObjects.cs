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

    public class PRResponse
    {
        public PullRequest[] Value { get; set; }
    }

    public class ValueBasedResponse<T>
    {
        public T[] Value { get; set; }
    }

    public class CountBasedResponse
    {
        public int Count { get; set; }
    }

    public class PullRequestReviewer : VSTSUser
    {
        public int Vote { get; set; }
    }

    public class VSTSUser : IEquatable<VSTSUser>
    {
        public bool IsContainer { get; set; }
        public string DisplayName { get; set; }
        public string UniqueName { get; set; }

        public bool Equals(VSTSUser other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(UniqueName, other.UniqueName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as VSTSUser);
        }

        public override int GetHashCode()
        {
            return UniqueName.GetHashCode();
        }
    }

    public class PullRequestThread
    {
        public IEnumerable<Comment> Comments { get; set; }

        public class Comment
        {
            public string CommentType { get; set; }
            public VSTSUser Author { get; set; }
        }
    }
}
