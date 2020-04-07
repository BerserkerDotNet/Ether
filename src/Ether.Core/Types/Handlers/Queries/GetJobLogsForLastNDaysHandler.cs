using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Extensions;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetJobLogsForLastNDaysHandler : IQueryHandler<GetJobLogsForLastNDays, IEnumerable<JobLogViewModel>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetJobLogsForLastNDaysHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<JobLogViewModel>> Handle(GetJobLogsForLastNDays query)
        {
            var jobs = await _repository.GetAsync<JobLog>(l => l.StartTime > DateTime.UtcNow.AddDays(-query.Days));
            return _mapper.MapCollection<JobLogViewModel>(jobs);
        }
    }
}
