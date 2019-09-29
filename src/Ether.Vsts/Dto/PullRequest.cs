using System;
using Ether.Contracts.Dto;
using Ether.Vsts.Types;

namespace Ether.Vsts.Dto
{
    public class PullRequest : BaseDto
    {
        public int PullRequestId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public Guid AuthorId { get; set; }

        public PullRequestState State { get; set; }

        public int Comments { get; set; }

        public int Iterations { get; set; }

        public Guid Repository { get; set; }

        public DateTime Created { get; set; }

        public DateTime Completed { get; set; }

        public DateTime LastSync { get; set; }
    }
}