using System.Collections.Generic;
using Ether.Contracts.Attributes;
using Ether.Contracts.Types;

namespace Ether.Contracts.Dto.Reports
{
    [DbName(nameof(ReportResult))]
    public class WorkItemsReport : ReportResult
    {
        public IEnumerable<WorkItemResolution> Resolutions { get; set; }
    }
}
