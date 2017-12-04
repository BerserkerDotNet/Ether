using System.Collections.Generic;

namespace Ether.Core.Models.VSTS
{
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
