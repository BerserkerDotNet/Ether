using Ether.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Core.Models.DTO.Reports
{
    [DbName(nameof(ReportResult))]
    public class PullRequestsReport : ReportResult
    {
        public int TotalCompleted => IndividualReports.Sum(r => r.Completed);
        public int TotalAbandoned => IndividualReports.Sum(r => r.Abandoned);
        public int TotalActive => IndividualReports.Sum(r => r.Active);
        public int TotalCreated => IndividualReports.Sum(r => r.Created);
        public double AverageIterations => IndividualReports.Count == 0 ? 0 : IndividualReports.Sum(r => r.AverageIterations) / IndividualReports.Count;
        public double AverageComments => IndividualReports.Count == 0 ? 0 : IndividualReports.Sum(r => r.AverageComments) / IndividualReports.Count;
        public double CodeQuality => IndividualReports.Count == 0 ? 0 : IndividualReports.Sum(r => r.CodeQuality) / IndividualReports.Count;
        public TimeSpan AveragePRLifespan => IndividualReports.Count == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(IndividualReports.Sum(r => r.AveragePRLifespan.TotalSeconds) / IndividualReports.Count);

        public IList<IndividualPRReport> IndividualReports { get; set; }

        public class IndividualPRReport
        {
            public int Completed { get; set; }
            public int Created { get; set; }
            public int Active { get; set; }
            public int Abandoned { get; set; }
            public int TotalIterations { get; set; }
            public int TotalComments { get; set; }
            public double CodeQuality { get; set; }
            public double AverageIterations { get; set; }
            public double AverageComments { get; set; }
            public TimeSpan AveragePRLifespan { get; set; }
            public string TeamMember { get; set; }

            public int TotalPullRequestsCount => Completed + Active + Abandoned;

            public static IndividualPRReport GetEmptyFor(string userDisplayName)
            {
                return new IndividualPRReport { TeamMember = userDisplayName };
            }
        }
    }
}
