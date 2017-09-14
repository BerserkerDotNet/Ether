using Ether.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Ether.Core.Reporters.WorkItemsReporter;

namespace Ether.Core.Models.DTO.Reports
{
    [DbName(nameof(ReportResult))]
    public class WorkItemsReport : ReportResult
    {
        public int TotalResolved => IndividualReports.Sum(r => r.ResolvedCount);
        public int TotalInvestigated => IndividualReports.Sum(r => r.InvestigatedCount);
        public IList<IndividualWorkItemsReport> IndividualReports { get; set; }

        public class IndividualWorkItemsReport
        {
            public int ResolvedCount => WorkItems.Count(w => w.Reason != WorkItemResolution.ReasonInvestigated);
            public int InvestigatedCount => WorkItems.Count(w => w.Reason == WorkItemResolution.ReasonInvestigated);

            public IEnumerable<WorkItemResolution> WorkItems { get; set; }
            public string TeamMember { get; set; }
        }
    }
}
