using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Commands
{
    public class SaveWorkItemsForUser : ICommand
    {
        public SaveWorkItemsForUser(IEnumerable<WorkItemViewModel> workitems, TeamMemberViewModel member)
        {
            Workitems = workitems;
            Member = member;
        }

        public IEnumerable<WorkItemViewModel> Workitems { get; }

        public TeamMemberViewModel Member { get; }
    }
}
