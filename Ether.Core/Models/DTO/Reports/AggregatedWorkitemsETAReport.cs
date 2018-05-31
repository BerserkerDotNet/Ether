using Ether.Core.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Core.Models.DTO.Reports
{
    [DbName(nameof(ReportResult))]
    public class AggregatedWorkitemsETAReport : ReportResult
    {
        public IList<IndividualETAReport> IndividualReports { get; set; }

        public class IndividualETAReport
        {
            public string TeamMember { get; set; }
            public int TotalResolved { get; set; }
            public int WithoutETA { get; set; }
            public float TotalEstimated { get; set; }
            public float TotalCompleted { get; set; }
            public float ActualCompleted { get; set; }
            public float SuccessRatio => TotalEstimated / ActualCompleted;

            public static IndividualETAReport GetEmptyFor(string teamMember) => new AggregatedWorkitemsETAReport.IndividualETAReport { TeamMember = teamMember };
        }

        public static AggregatedWorkitemsETAReport Empty => new AggregatedWorkitemsETAReport { IndividualReports = Enumerable.Empty<IndividualETAReport>().ToList() };
    }
}
