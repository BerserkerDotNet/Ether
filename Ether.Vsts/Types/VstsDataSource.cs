using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Types
{
    public class VstsDataSource : IDataSource
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public VstsDataSource(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProfileViewModel> GetProfile(Guid id)
        {
            var profile = await _repository.GetSingleAsync<VstsProfile>(id);
            return _mapper.Map<ProfileViewModel>(profile);
        }

        public async Task<TeamMemberViewModel> GetTeamMember(Guid id)
        {
            var member = await _repository.GetSingleAsync<TeamMember>(id);
            return _mapper.Map<TeamMemberViewModel>(member);
        }

        public async Task<IEnumerable<PullRequestViewModel>> GetPullRequests(Expression<Func<PullRequestViewModel, bool>> predicate)
        {
            var pullRequests = await _repository.GetAllAsync<PullRequest>();
            var result = pullRequests
                .Select(p => _mapper.Map<PullRequestViewModel>(p))
                .Where(predicate.Compile())
                .ToArray();

            return result;
        }
    }
}