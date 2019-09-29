using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetProfileByIdHandler : IQueryHandler<GetProfileById, ProfileViewModel>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetProfileByIdHandler(IRepository repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public async Task<ProfileViewModel> Handle(GetProfileById input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Id == Guid.Empty)
            {
                return null;
            }

            var result = await _repository.GetSingleAsync<VstsProfile>(input.Id);
            return _mapper.Map<ProfileViewModel>(result);
        }
    }
}
