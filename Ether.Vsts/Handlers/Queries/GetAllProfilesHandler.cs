using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetAllProfilesHandler : IQueryHandler<GetAllProfiles, IEnumerable<VstsProfileViewModel>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetAllProfilesHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VstsProfileViewModel>> Handle(GetAllProfiles input)
        {
            var result = await _repository.GetAsync<VstsProfile>(p => p.Type == Constants.VstsProfileType);
            return _mapper.Map<IEnumerable<VstsProfileViewModel>>(result);
        }
    }
}
