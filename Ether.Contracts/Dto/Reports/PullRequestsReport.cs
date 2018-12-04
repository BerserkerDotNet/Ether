using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Attributes;

namespace Ether.Contracts.Dto.Reports
{
    [DbName(nameof(ReportResult))]
    public class PullRequestsReport : ReportResult
    {
        public PullRequestsReport(int individualReportsCount)
        {
            IndividualReports = new List<IndividualPRReport>(individualReportsCount);
        }

        public List<IndividualPRReport> IndividualReports { get; set; }

        public void AddReport(IndividualPRReport report)
        {
            IndividualReports.Add(report);
        }

        public void AddEmpty(string memberName)
        {
            IndividualReports.Add(IndividualPRReport.GetEmptyFor(memberName));
        }

        public class IndividualPRReport
        {
            public string TeamMember { get; set; }

            public int Completed { get; set; }

            public int Created { get; set; }

            public int Active { get; set; }

            public int Abandoned { get; set; }

            public int TotalIterations { get; set; }

            public int TotalComments { get; set; }

            public TimeSpan AveragePRLifespan { get; set; }

            public bool IsEmpty => Completed == 0 && Created == 0 && Active == 0 && Abandoned == 0 && TotalIterations == 0 && TotalComments == 0 && AveragePRLifespan == TimeSpan.Zero;

            public static IndividualPRReport GetEmptyFor(string userDisplayName)
            {
                return new IndividualPRReport { TeamMember = userDisplayName };
            }
        }
    }
}
