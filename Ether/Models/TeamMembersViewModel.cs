using Ether.Core.Models.DTO;
using System.Collections.Generic;

namespace Ether.Models
{
    public class TeamMembersViewModel
    {
        public IEnumerable<string> Teams { get; set; }

        public IEnumerable<string> AllTeams { get; set; }

        public IEnumerable<TeamMember> TeamMembers { get; set; }
    }
}
