using System;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveTeamMemberHandler : SaveHandler<TeamMember, SaveTeamMember>
    {
        public SaveTeamMemberHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override object GetData(SaveTeamMember command) => command.TeamMember;

        protected override void ValidateCommand(SaveTeamMember command)
        {
            if (command.TeamMember == null)
            {
                throw new ArgumentNullException(nameof(command.TeamMember));
            }
        }
    }
}
