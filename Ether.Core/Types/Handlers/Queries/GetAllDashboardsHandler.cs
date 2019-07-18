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
    public class GetAllDashboardsHandler : IQueryHandler<GetAllDashboards, IEnumerable<DashboardSettingsViewModel>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetAllDashboardsHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DashboardSettingsViewModel>> Handle(GetAllDashboards query)
        {
            var dashboards = await _repository.GetAllAsync<DashboardSettings>();
            return _mapper.Map<IEnumerable<DashboardSettingsViewModel>>(dashboards);
        }
    }
}
