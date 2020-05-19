using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Vsts.Queries
{
    public class GetOrganizationById : IQuery<OrganizationViewModel>
    {
        public GetOrganizationById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
