using System.Collections.Generic;
using Ether.ViewModels;

namespace Ether.Types.State
{
    public class TeamMembersState
    {
        public TeamMembersState(IEnumerable<TeamMemberViewModel> members)
        {
            Members = members;
        }

        public IEnumerable<TeamMemberViewModel> Members { get; private set; }
    }
}
