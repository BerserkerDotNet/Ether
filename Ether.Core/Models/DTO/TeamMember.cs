using System;
using System.Collections.Generic;

namespace Ether.Core.Models.DTO
{
    public class TeamMember: BaseDto
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string TeamName { get; set; }

        public IEnumerable<int> RelatedWorkItemIds { get; set; }
        public DateTime LastFetchDate { get; set; }
    }
}
