using System;
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
using Ether.Vsts.Types;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetMembersAndRepositoriesOfAllProfilesHandler : IQueryHandler<GetMembersAndRepositoriesOfAllProfiles, IEnumerable<RepositoryInfo>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetMembersAndRepositoriesOfAllProfilesHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RepositoryInfo>> Handle(GetMembersAndRepositoriesOfAllProfiles query)
        {
            var profiles = await _repository.GetAsync<VstsProfile>(p => p.Type == Constants.VstsType);

            var membersToFetch = profiles
                .SelectMany(p => p.Members)
                .Distinct()
                .ToArray();

            var members = await _repository.GetAsync<TeamMember>(m => membersToFetch.Contains(m.Id));
            if (query.IncludeMembers != null && query.IncludeMembers.Any())
            {
                members = members.Where(m => query.IncludeMembers.Contains(m.Id)).ToArray();
            }

            var repositoriesToFetch = profiles
                .SelectMany(p => p.Repositories)
                .Distinct()
                .ToArray();
            var repositories = await _repository.GetAsync<Repository>(r => repositoriesToFetch.Contains(r.Id));

            var projectsToFetch = repositories
                .Select(r => r.Project)
                .Distinct()
                .ToArray();
            var projects = await _repository.GetAsync<Project>(p => projectsToFetch.Contains(p.Id));

            var organizationsToFetch = projects
                .Select(p => p.Organization)
                .Distinct()
                .ToArray();
            var organizations = await _repository.GetAsync<Contracts.Dto.Organization>(i => organizationsToFetch.Contains(i.Id));

            var identitiesToFetch = projects
                .Select(p => p.Identity)
                .Distinct()
                .ToArray();
            var identities = await _repository.GetAsync<Identity>(i => identitiesToFetch.Contains(i.Id));

            return repositories.Select(r =>
            {
                var info = _mapper.Map<RepositoryInfo>(r);
                info.Members = profiles
                    .Where(p => p.Repositories.Contains(r.Id))
                    .SelectMany(m => m.Members)
                    .Distinct()
                    .Select(i => _mapper.Map<TeamMemberViewModel>(members.SingleOrDefault(m => m.Id == i)))
                    .Where(m => m != null)
                    .ToArray();

                var project = projects.Single(p => p.Id == r.Project);
                var projectInfo = _mapper.Map<ProjectInfo>(project);
                projectInfo.Organization = MapOrganization(organizations, project.Organization);
                projectInfo.Identity = MapIdentity(identities, project.Identity);
                info.Project = projectInfo;

                return info;
            })
            .Where(r => r.Members.Any())
            .ToArray();
        }

        private OrganizationViewModel MapOrganization(IEnumerable<Contracts.Dto.Organization> organizations, Guid? organizationId)
        {
            if (!organizationId.HasValue)
            {
                return null;
            }

            var organization = organizations.Single(p => p.Id == organizationId.Value);
            return _mapper.Map<OrganizationViewModel>(organization);
        }

        private IdentityViewModel MapIdentity(IEnumerable<Identity> identities, Guid? identityId)
        {
            if (!identityId.HasValue)
            {
                return null;
            }

            var identity = identities.Single(p => p.Id == identityId.Value);
            return _mapper.Map<IdentityViewModel>(identity);
        }
    }
}
