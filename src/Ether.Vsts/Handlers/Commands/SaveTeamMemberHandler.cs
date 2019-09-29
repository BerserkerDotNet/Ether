using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Exceptions;
using Ether.Vsts.Interfaces;
using static Ether.Contracts.Types.NullUtil;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveTeamMemberHandler : ICommandHandler<SaveTeamMember>
    {
        private readonly IRepository _repository;
        private readonly IVstsClientFactory _clientFactory;
        private readonly IMapper _mapper;

        public SaveTeamMemberHandler(IRepository repository, IVstsClientFactory clientFactory, IMapper mapper)
        {
            _repository = repository;
            _clientFactory = clientFactory;
            _mapper = mapper;
        }

        public async Task Handle(SaveTeamMember command)
        {
            CheckIfArgumentNull(command.TeamMember, nameof(command.TeamMember));

            var client = await _clientFactory.GetIdentityClient();
            var member = command.TeamMember;
            var identities = await client.GetIdentitiesAsync(member.Email, onlyActive: true);
            if (!identities.Any())
            {
                throw new IdentityNotFoundException($"No identities found for email: {member.Email}");
            }

            member.Id = identities.First().Id;

            var relatedWorkitems = await _repository.GetFieldValueAsync<TeamMember, int[]>(t => t.Id == member.Id, t => t.RelatedWorkItems);
            var dto = _mapper.Map<TeamMember>(member);
            dto.RelatedWorkItems = relatedWorkitems;
            await _repository.CreateOrUpdateAsync(dto);
        }
    }
}
