using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IEnumerable<ReporterDescriptor> _reportDescriptors;
        private readonly IMapper _mapper;

        public GetReportByIdHandler(IRepository repository, IEnumerable<ReporterDescriptor> reportDescriptors, IMapper mapper)
        {
            _repository = repository;
            _reportDescriptors = reportDescriptors;
            _mapper = mapper;
        }

        public async Task<ReportViewModel> Handle(GetReportById query)
        {
            if (query.Id == Guid.Empty)
            {
                return null;
            }

            var reportType = await _repository.GetFieldValueAsync<ReportResult, string>(r => r.Id == query.Id, r => r.ReportType);
            var descriptor = _reportDescriptors.FirstOrDefault(d => string.Equals(d.UniqueName, reportType, StringComparison.OrdinalIgnoreCase));
            if (descriptor == null)
            {
                return null;
            }

            var report = await _repository.GetSingleAsync(query.Id, descriptor.DtoType);
            return _mapper.Map(report, descriptor.DtoType, descriptor.ViewModelType) as ReportViewModel;
        }
    }
}
