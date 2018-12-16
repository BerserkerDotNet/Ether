using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public class GetAllReportsHandler : GetAllHandler<ReportResult, ReportViewModel, GetAllReports>
    {
        public GetAllReportsHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }

        protected override IEnumerable<ReportViewModel> PostProcessData(IEnumerable<ReportViewModel> data)
        {
            return data.OrderByDescending(o => o.DateTaken);
        }
    }
}
