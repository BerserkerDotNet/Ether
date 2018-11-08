using System;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class DeleteProjectHandler : DeleteHandler<Project, DeleteProject>
    {
        public DeleteProjectHandler(IRepository repository)
            : base(repository)
        {
        }

        protected override Guid GetId(DeleteProject command) => command.Id;
    }
}
