using Ether.Core.Attributes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ether.Core.Models.DTO.Reports
{
    [DbName(nameof(ReportResult))]
    public class WorkItemsReport : ReportResult
    {
        public int TotalResolved => Resolutions.Count(r => r.Resolution == "Resolved");
        public int TotalInvestigated => Resolutions.Count(r => r.Resolution == "Investigated");
        public IList<WorkItemResolution> Resolutions { get; set; }
    }
}
