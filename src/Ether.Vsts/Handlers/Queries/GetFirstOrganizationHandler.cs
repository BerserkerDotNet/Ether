using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetFirstOrganizationHandler : IQueryHandler<GetFirstOrganization, OrganizationViewModel>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetFirstOrganizationHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            this._mapper = mapper;
        }

        public async Task<OrganizationViewModel> Handle(GetFirstOrganization input)
        {
            var result = await _repository.GetAllAsync<VstsOrganization>();

            return _mapper.Map<OrganizationViewModel>(result.First());
        }
    }
}
