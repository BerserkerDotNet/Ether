using System.Collections.Generic;
using System.Linq;

namespace Ether.Types.DTO.Reports
{
    public class PullRequestsReport : ReportResult
    {
        public int TotalPRs => IndividualReports.Sum(r => r.TotalPRs);
        public double AverageIterations => IndividualReports.Sum(r => r.AverageIterations) / IndividualReports.Count;
        public double CodeQuality => IndividualReports.Sum(r => r.CodeQuality) / IndividualReports.Count;
        public IList<IndividualPRReport> IndividualReports { get; set; }

        public class IndividualPRReport
        {
            public int TotalPRs { get; set; }
            public int TotalIterations { get; set; }
            public double CodeQuality { get; set; }
            public double AverageIterations { get; set; }
            public string TeamMember { get; set; }
        }
    }
}
