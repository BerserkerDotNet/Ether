using System;
using System.Linq.Expressions;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetAllJobLogsHandler : GetAllPagedHandler<JobLog, JobLogViewModel, GetAllJobLogs>
    {
        public GetAllJobLogsHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override Expression<Func<JobLog, object>> GetSorting()
        {
            return l => l.StartTime;
        }
    }
}
