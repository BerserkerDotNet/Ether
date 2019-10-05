using System.Collections.Generic;
using Ether.Contracts.Attributes;
using Ether.ViewModels.Types;

namespace Ether.Contracts.Dto.Reports
{
    [DbName(nameof(ReportResult))]
    public class ReOpenedWorkItemsReport : ReportResult
    {
        public ReOpenedWorkItemsReport()
        {
            Details = new List<ReOpenedWorkItemDetail>();
        }

        public List<ReOpenedWorkItemDetail> Details { get; set; }

        public Dictionary<string, int> ResolvedWorkItemsLookup { get; set; }

        public Dictionary<string, string> MembersLookup { get; set; }
    }
}
