using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Exceptions;
using Ether.Vsts.Interfaces;
using VSTS.Net.Interfaces;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveTeamMemberHandler : SaveHandler<VstsTeamMemberViewModel, TeamMember, SaveTeamMember>
    {
        private readonly IVstsClientFactory _clientFactory;

        public SaveTeamMemberHandler(IRepository repository, IVstsClientFactory clientFactory, IMapper mapper)
            : base(repository, mapper)
        {
            _clientFactory = clientFactory;
        }

        protected override async Task<VstsTeamMemberViewModel> GetData(SaveTeamMember command)
        {
            var client = await _clientFactory.GetIdentityClient();
            var member = command.TeamMember;
            var identities = await client.GetIdentitiesAsync(member.Email, onlyActive: true);
            if (!identities.Any())
            {
                throw new IdentityNotFoundException($"No identities found for email: {member.Email}");
            }

            member.Id = identities.First().Id;

            return command.TeamMember;
        }

        protected override void ValidateCommand(SaveTeamMember command)
        {
            if (command.TeamMember == null)
            {
                throw new ArgumentNullException(nameof(command.TeamMember));
            }
        }
    }
}
