using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Attributes;
using Ether.Contracts.Types;

namespace Ether.Contracts.Dto.Reports
{
    [DbName(nameof(ReportResult))]
    public class WorkItemsReport : ReportResult
    {
        public static WorkItemsReport Empty => new WorkItemsReport { Resolutions = Enumerable.Empty<WorkItemResolution>() };

        public IEnumerable<WorkItemResolution> Resolutions { get; set; }
    }
}
