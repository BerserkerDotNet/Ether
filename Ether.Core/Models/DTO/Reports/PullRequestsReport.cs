using Ether.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Core.Models.DTO.Reports
{
    [DbName(nameof(ReportResult))]
    public class PullRequestsReport : ReportResult
    {
        public int TotalPRs => IndividualReports.Sum(r => r.TotalPRs);
        public double AverageIterations => IndividualReports.Count == 0 ? 0 : IndividualReports.Sum(r => r.AverageIterations) / IndividualReports.Count;
        public double AverageComments => IndividualReports.Count == 0 ? 0 : IndividualReports.Sum(r => r.AverageComments) / IndividualReports.Count;
        public double CodeQuality => IndividualReports.Count == 0 ? 0 : IndividualReports.Sum(r => r.CodeQuality) / IndividualReports.Count;
        public TimeSpan AveragePRLifespan => IndividualReports.Count == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(IndividualReports.Sum(r => r.AveragePRLifespan.TotalSeconds) / IndividualReports.Count);

        public IList<IndividualPRReport> IndividualReports { get; set; }

        public class IndividualPRReport
        {
            public int TotalPRs { get; set; }
            public int TotalIterations { get; set; }
            public int TotalComments { get; set; }
            public double CodeQuality { get; set; }
            public double AverageIterations { get; set; }
            public double AverageComments { get; set; }
            public TimeSpan AveragePRLifespan { get; set; }
            public string TeamMember { get; set; }
        }
    }
}
