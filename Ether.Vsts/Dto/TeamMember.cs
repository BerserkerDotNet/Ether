using System;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    public class TeamMember : BaseDto
    {
        public string Email { get; set; }

        public string DisplayName { get; set; }

        public int[] RelatedWorkItems { get; set; }

        public DateTime? LastWorkitemsFetchDate { get; set; }

        public DateTime? LastPullRequestsFetchDate { get; set; }
    }
}
