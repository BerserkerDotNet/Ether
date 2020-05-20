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
    public class GetAllOrganizationsHandler : IQueryHandler<GetAllOrganizations, IEnumerable<OrganizationViewModel>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetAllOrganizationsHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrganizationViewModel>> Handle(GetAllOrganizations input)
        {
            var organizations = await _repository.GetAllAsync<Organization>();

            return _mapper.Map<IEnumerable<OrganizationViewModel>>(organizations);
        }
    }
}
