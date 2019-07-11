using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetDashboardSettingsHandler : IQueryHandler<GetDashboardSettings, DashboardSettingsViewModel>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetDashboardSettingsHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<DashboardSettingsViewModel> Handle(GetDashboardSettings query)
        {
            var settings = await _repository.GetSingleAsync<DashboardSettings>(query.Id);
            return _mapper.Map<DashboardSettingsViewModel>(settings);
        }
    }
}
