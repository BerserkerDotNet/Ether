using System;
using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Queries
{
    public class GetTeamMemberRelatedOrganizations : IQuery<IEnumerable<OrganizationViewModel>>
    {
        public GetTeamMemberRelatedOrganizations(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
