using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetAllIdentitiesHandler : IQueryHandler<GetAllIdentities, IEnumerable<IdentityViewModel>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetAllIdentitiesHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<IdentityViewModel>> Handle(GetAllIdentities input)
        {
            var identities = await _repository.GetAllAsync<Identity>();
            return _mapper.Map<IEnumerable<IdentityViewModel>>(identities);
        }
    }
}
