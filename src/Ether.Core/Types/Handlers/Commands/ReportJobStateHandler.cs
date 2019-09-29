using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;

namespace Ether.Core.Types.Handlers.Commands
{
    public class ReportJobStateHandler : ICommandHandler<ReportJobState>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public ReportJobStateHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Handle(ReportJobState command)
        {
            if (string.IsNullOrEmpty(command.JobType))
            {
                throw new ArgumentNullException(nameof(command.JobType));
            }

            var log = _mapper.Map<JobLog>(command);
            await _repository.CreateOrUpdateAsync(log);
        }
    }
}
