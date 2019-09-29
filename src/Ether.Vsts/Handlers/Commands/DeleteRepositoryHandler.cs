using System;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class DeleteRepositoryHandler : DeleteHandler<Repository, DeleteRepository>
    {
        public DeleteRepositoryHandler(IRepository repository)
            : base(repository)
        {
        }

        protected override Guid GetId(DeleteRepository command) => command.Id;
    }
}
