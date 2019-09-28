using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class TeamMembersState
    {
        [JsonConstructor]
        public TeamMembersState(IEnumerable<TeamMemberViewModel> members)
        {
            Members = members;
        }

        public IEnumerable<TeamMemberViewModel> Members { get; private set; }
    }
}
