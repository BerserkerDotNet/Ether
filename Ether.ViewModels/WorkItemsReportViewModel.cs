using System.Collections.Generic;
using System.Linq;

namespace Ether.ViewModels
{
    public class WorkItemsReportViewModel : ReportViewModel
    {
        public WorkItemsReportViewModel()
        {
            Resolutions = Enumerable.Empty<WorkItemResolutionViewModel>();
        }

        public int TotalResolved => Resolutions.Count(r => r.Resolution == "Resolved" || r.Resolution == "Closed");

        public int TotalInvestigated => Resolutions.Count(r => r.Resolution == "Investigated");

        public int TotalTasks => Resolutions.Count(w => w.WorkItemType == "Task");

        public int TotalBugs => Resolutions.Count(w => w.WorkItemType == "Bug");

        public IEnumerable<WorkItemResolutionViewModel> Resolutions { get; set; }
    }
}
