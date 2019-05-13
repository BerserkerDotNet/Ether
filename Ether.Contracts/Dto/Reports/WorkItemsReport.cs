using System.Collections.Generic;
using Ether.Contracts.Attributes;
using Ether.ViewModels.Types;

namespace Ether.Contracts.Dto.Reports
{
    [DbName(nameof(ReportResult))]
    public class WorkItemsReport : ReportResult
    {
        public static WorkItemsReport Empty => new WorkItemsReport
        {
            ActiveWorkItems = new List<WorkItemDetail>(),
            ResolvedWorkItems = new List<WorkItemDetail>(),
            WorkItemsInReview = new List<WorkItemDetail>()
        };

        public List<WorkItemDetail> ActiveWorkItems { get; set; }

        public List<WorkItemDetail> ResolvedWorkItems { get; set; }

        public List<WorkItemDetail> WorkItemsInReview { get; set; }
    }
}
