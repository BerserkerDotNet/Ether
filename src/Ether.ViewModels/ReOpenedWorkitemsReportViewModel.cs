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

        public int TotalReopened => Details.Count();

        public IEnumerable<IGrouping<string, ReOpenedWorkItemDetail>> GroupedByMember => Details
            .GroupBy(k => k.AssociatedUser.Email, v => v)
            .OrderBy(k => k.First().AssociatedUser.Title);
    }
}
