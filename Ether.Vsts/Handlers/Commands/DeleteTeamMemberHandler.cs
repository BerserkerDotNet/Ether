using System;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class DeleteTeamMemberHandler : DeleteHandler<TeamMember, DeleteTeamMember>
    {
        public DeleteTeamMemberHandler(IRepository repository)
            : base(repository)
        {
        }

        protected override Guid GetId(DeleteTeamMember command) => command.Id;
    }
}
