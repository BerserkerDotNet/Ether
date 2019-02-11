using System.Collections.Generic;
using System.Linq;
using Ether.Core.Attributes;

namespace Ether.Core.Models.DTO.Reports
{
    [DbName(nameof(ReportResult))]
    public class WeeklyStatusReport : ReportResult
    {
        public IList<WorkItemDetail> ActiveWorkItems { get; set; }
        public IList<WorkItemDetail> ResolvedWorkItems { get; set; }
        public IList<WorkItemDetail> WorkItemsInReview { get; set; }

        public static WeeklyStatusReport Empty => new WeeklyStatusReport
        {
            ActiveWorkItems = new List<WorkItemDetail>(),
            ResolvedWorkItems = new List<WorkItemDetail>(),
            WorkItemsInReview = new List<WorkItemDetail>()
        };

        public class WorkItemDetail
        {
            public int WorkItemId { get; set; }

            public string WorkItemTitle { get; set; }

            public string WorkItemType { get; set; }

            public string WorkItemProject { get; set; }

            public string Tags { get; set; }

            public float OriginalEstimate { get; set; }

            public float EstimatedToComplete { get; set; }

            public float TimeSpent { get; set; }

        }
    }
}