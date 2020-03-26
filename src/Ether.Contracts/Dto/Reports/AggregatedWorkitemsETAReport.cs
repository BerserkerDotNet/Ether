using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Attributes;
using Ether.ViewModels;

namespace Ether.Contracts.Dto.Reports
{
    [DbName(nameof(ReportResult))]
    public class AggregatedWorkitemsETAReport : ReportResult
    {
        public AggregatedWorkitemsETAReport(int individualReportsCount)
        {
            IndividualReports = new List<IndividualETAReport>(individualReportsCount);
        }

        public static AggregatedWorkitemsETAReport Empty => new AggregatedWorkitemsETAReport(0);

        public List<IndividualETAReport> IndividualReports { get; set; }

        public int Workdays { get; set; }

        public class IndividualETAReport
        {
            public string MemberEmail { get; set; }

            public string MemberName { get; set; }

            public int TotalResolved { get; set; }

            public int TotalResolvedBugs { get; set; }

            public int TotalResolvedTasks { get; set; }

            public int WithoutETA { get; set; }

            public int WithOriginalEstimate { get; set; }

            public float OriginalEstimated { get; set; }

            public float EstimatedToComplete { get; set; }

            public float CompletedWithEstimates { get; set; }

            public float CompletedWithoutEstimates { get; set; }

            public List<IndividualReportDetail> Details { get; set; }

            public static IndividualETAReport GetEmptyFor(TeamMemberViewModel teamMember) =>
                new IndividualETAReport { MemberEmail = teamMember.Email, MemberName = teamMember.DisplayName, Details = new List<IndividualReportDetail>(0) };
        }

        public class IndividualReportDetail
        {
            public int WorkItemId { get; set; }

            public string WorkItemTitle { get; set; }

            public string WorkItemType { get; set; }

            public float OriginalEstimate { get; set; }

            public float EstimatedToComplete { get; set; }

            public float TimeSpent { get; set; }
        }
    }
}
