using System;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public class GetProfileById : IQuery<ProfileViewModel>
    {
        public GetProfileById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
