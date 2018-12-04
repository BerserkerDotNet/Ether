using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetReportByIdHandler : IQueryHandler<GetReportById, ReportViewModel>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetReportByIdHandler(IRepository repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public async Task<ReportViewModel> Handle(GetReportById query)
        {
            if (query.Id == Guid.Empty)
            {
                return null;
            }

            var reportType = await _repository.GetFieldValueAsync<ReportResult, string>(r => r.Id == query.Id, r => r.ReportType);
            switch (reportType)
            {
                case Constants.PullRequestsReportType:
                    var report = await _repository.GetSingleAsync<PullRequestsReport>(query.Id);
                    return _mapper.Map<PullRequestReportViewModel>(report);
            }

            return null;
        }

        private Type GetTypeFor(string reportType)
        {
            if (string.Equals(reportType, Constants.PullRequestsReportType))
            {
                return typeof(PullRequestsReport);
            }

            throw new ArgumentOutOfRangeException($"Report type '{reportType}' is not supported");
        }
    }
}
