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
            public string MemberEmail { get; set; }
            public string MemberName { get; set; }
            public int TotalResolved { get; set; }
            public int TotalResolvedBugs { get; set; }
            public int TotalResolvedTasks { get; set; }
            public int WithoutETA { get; set; }
            public float OriginalEstimated { get; set; }
            public float EstimatedToComplete { get; set; }
            public float CompletedWithEstimates { get; set; }
            public float CompletedWithoutEstimates { get; set; }

            public float TotalCompleted => CompletedWithEstimates + CompletedWithoutEstimates;
            public float EstimatedToCompletedRatio => EstimatedToComplete / CompletedWithEstimates;

            public static IndividualETAReport GetEmptyFor(TeamMember teamMember) => 
                new IndividualETAReport { MemberEmail = teamMember.Email, MemberName = teamMember.DisplayName };
        }

        public int TotalResolved => IndividualReports.Sum(r => r.TotalResolved);
        public float EstimatedToComplete => IndividualReports.Sum(r => r.EstimatedToComplete);
        public float CompletedWithEstimates => IndividualReports.Sum(r => r.CompletedWithEstimates);
        public float EstimatedToCompletedRatio => EstimatedToComplete / CompletedWithEstimates;

        public static AggregatedWorkitemsETAReport Empty => new AggregatedWorkitemsETAReport { IndividualReports = Enumerable.Empty<IndividualETAReport>().ToList() };
    }
}
