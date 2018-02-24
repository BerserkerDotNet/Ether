using System;
using System.Collections.Generic;

namespace Ether.Core.Models.VSTS
{
    public class PullRequestThread
    {
        public int Id { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public bool IsDeleted { get; set; }

        public class Comment
        {
            public int Id { get; set; }
            public int ParentCommentId { get; set; }
            public DateTime PublishedDate { get; set; }
            public DateTime LastUpdatedDate { get; set; }
            public string CommentType { get; set; }
            public VSTSUser Author { get; set; }
            public string Content { get; set; }
        }
    }
}
