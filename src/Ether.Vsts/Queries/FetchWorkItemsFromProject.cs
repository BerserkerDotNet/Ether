using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Queries
{
    public class FetchWorkItemsFromProject : IQuery<IEnumerable<WorkItemViewModel>>
    {
        public FetchWorkItemsFromProject(TeamMemberViewModel member)
        {
            Member = member;
        }

        public TeamMemberViewModel Member { get; private set; }
    }
}
