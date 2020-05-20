using System;
using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Vsts.Queries
{
    public class FetchWorkItemsOtherThanBugsAndTasks : IQuery<IEnumerable<int>>
    {
        public FetchWorkItemsOtherThanBugsAndTasks(Guid organizationId)
        {
            OrganizationId = organizationId;
        }

        public Guid OrganizationId { get; private set; }
    }
}
