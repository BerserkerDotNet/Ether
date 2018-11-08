using System;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class DeleteProfileHandler : DeleteHandler<VstsProfile, DeleteProfile>
    {
        public DeleteProfileHandler(IRepository repository)
            : base(repository)
        {
        }

        protected override Guid GetId(DeleteProfile command) => command.Id;
    }
}
