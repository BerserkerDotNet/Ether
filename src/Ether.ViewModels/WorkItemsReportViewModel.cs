using System.Collections.Generic;
using System.Linq;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class WorkItemsReportViewModel : ReportViewModel
    {
        public WorkItemsReportViewModel()
        {
        }

        public IEnumerable<WorkItemDetail> ActiveWorkItems { get; set; }

        public IEnumerable<WorkItemDetail> ResolvedWorkItems { get; set; }

        public IEnumerable<WorkItemDetail> WorkItemsInReview { get; set; }

        public int GetTotalBugs(IEnumerable<WorkItemDetail> details) => details.Count(w => string.Equals(w.WorkItemType, "bug", System.StringComparison.OrdinalIgnoreCase));

        public int GetTotalTasks(IEnumerable<WorkItemDetail> details) => details.Count(w => string.Equals(w.WorkItemType, "task", System.StringComparison.OrdinalIgnoreCase));

        public float GetTotalEstimated(IEnumerable<WorkItemDetail> details) => details.Sum(i => i.EstimatedToComplete);

        public float GetTotalTimeSpent(IEnumerable<WorkItemDetail> details) => details.Sum(i => i.TimeSpent);
    }
}
