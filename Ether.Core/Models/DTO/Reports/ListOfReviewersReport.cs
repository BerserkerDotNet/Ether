using Ether.Core.Attributes;
using System.Collections.Generic;

namespace Ether.Core.Models.DTO.Reports
{
    [DbName(nameof(ReportResult))]
    public class ListOfReviewersReport : ReportResult
    {
        public int NumberOfPullRequests { get; set; }

        public int NumberOfReviewers => IndividualReports.Count;

        public IList<IndividualReviewerReport> IndividualReports { get; set; }

        public class IndividualReviewerReport
        {
            public string DisplayName { get; set; }
            public string UniqueName { get; set; }
            public int NumberOfPRsVoted { get; set; }
            public int NumberOfComments { get; set; }
        }
    }
}
