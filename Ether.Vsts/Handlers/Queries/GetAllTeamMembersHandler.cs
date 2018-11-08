using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetAllTeamMembersHandler : GetAllHandler<TeamMember, VstsTeamMemberViewModel, GetAllTeamMembers>
    {
        public GetAllTeamMembersHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }
    }
}
