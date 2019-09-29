using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetIdentityById : IQuery<IdentityViewModel>
    {
        public Guid Id { get; set; }
    }
}
