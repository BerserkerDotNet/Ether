using System;
using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Queries
{
    public class FetchWorkItemsFromProject : IQuery<IEnumerable<WorkItemViewModel>>
    {
        public FetchWorkItemsFromProject(TeamMemberViewModel member, Guid organizationId)
        {
            Member = member;
            OrganizationId = organizationId;
        }

        public TeamMemberViewModel Member { get; private set; }

        public Guid OrganizationId { get; private set; }
    }
}
