using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetIdentityByIdHandler : IQueryHandler<GetIdentityById, IdentityViewModel>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetIdentityByIdHandler(IRepository repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public async Task<IdentityViewModel> Handle(GetIdentityById input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Id == Guid.Empty)
            {
                return null;
            }

            var result = await _repository.GetSingleAsync<Identity>(input.Id);
            return _mapper.Map<IdentityViewModel>(result);
        }
    }
}
