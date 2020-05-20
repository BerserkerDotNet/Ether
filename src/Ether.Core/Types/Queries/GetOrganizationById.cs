using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetOrganizationById : IQuery<OrganizationViewModel>
    {
        public Guid Id { get; set; }
    }
}
