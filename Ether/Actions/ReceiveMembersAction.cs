using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveMembersAction : IAction
    {
        public IEnumerable<TeamMemberViewModel> Members { get; set; }
    }
}
