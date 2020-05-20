using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetTeamMemberRelatedOrganizationsHandler : IQueryHandler<GetTeamMemberRelatedOrganizations, IEnumerable<OrganizationViewModel>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetTeamMemberRelatedOrganizationsHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrganizationViewModel>> Handle(GetTeamMemberRelatedOrganizations input)
        {
            var profiles = await _repository.GetAsync<VstsProfile>(p => p.Members.Contains(input.Id));

            var repositoriesToFetch = profiles
                .SelectMany(profile => profile.Repositories)
                .Distinct()
                .ToArray();
            var repositories = await _repository.GetAsync<Repository>(repository => repositoriesToFetch.Contains(repository.Id));

            var projectsToFetch = repositories
                .Select(repository => repository.Project)
                .Distinct()
                .ToArray();
            var projects = await _repository.GetAsync<Project>(project => projectsToFetch.Contains(project.Id));

            var organizationsToFetch = projects
                .Select(project => project.Organization)
                .Distinct()
                .ToArray();
            var organizations = await _repository.GetAsync<VstsOrganization>(organization => organizationsToFetch.Contains(organization.Id));

            return _mapper.Map<IEnumerable<OrganizationViewModel>>(organizations);
        }
    }
}
