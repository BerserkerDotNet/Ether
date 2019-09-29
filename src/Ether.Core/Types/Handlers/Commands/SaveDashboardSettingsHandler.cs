using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;

namespace Ether.Core.Types.Handlers.Commands
{
    public class SaveDashboardSettingsHandler : ICommandHandler<SaveDashboardSettings>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public SaveDashboardSettingsHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Handle(SaveDashboardSettings command)
        {
            var dto = _mapper.Map<DashboardSettings>(command.Settings);
            await _repository.CreateOrUpdateAsync(dto);
        }
    }
}
