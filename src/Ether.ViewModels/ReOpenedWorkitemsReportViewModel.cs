using System.Collections.Generic;
using System.Linq;
using Ether.ViewModels.Types;

namespace Ether.ViewModels
{
    public class ReOpenedWorkItemsReportViewModel : ReportViewModel
    {
        public ReOpenedWorkItemsReportViewModel()
        {
            Details = Enumerable.Empty<ReOpenedWorkItemDetail>();
        }

        public IEnumerable<ReOpenedWorkItemDetail> Details { get; set; }
    }
}
