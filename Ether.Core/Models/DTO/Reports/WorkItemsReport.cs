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

        [DebuggerDisplay("{Type} - {Id} {Reason}")]
        public class WorkItemResolution
        {
            public const string ReasonInvestigated = "Investigated";

            public int Id { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
            public string Resolution { get; set; }
            public string Reason { get; set; }
            public DateTime ResolutionDate { get; set; }
            public string TeamMember { get; set; }


            public static WorkItemResolution GetResolved(int id, string title, string itemType, WorkItemUpdate update)
            {
                return GetResolution(id, title, itemType, update.Reason.NewValue, update);
            }

            public static WorkItemResolution GetInvestigated(int id, string title, string itemType, WorkItemUpdate update)
            {
                return GetResolution(id, title, itemType, ReasonInvestigated, update);
            }

            private static WorkItemResolution GetResolution(int id, string title, string itemType, string reason, WorkItemUpdate update)
            {
                var resolvedBy = string.IsNullOrEmpty(update.AssignedTo.OldValue) ? update.ResolvedBy.NewValue : update.AssignedTo.OldValue;
                var resolution = new WorkItemResolution();
                resolution.Id = id;
                resolution.Title = title;
                resolution.Type = itemType;
                resolution.Resolution = "Resolved";
                resolution.Reason = reason;
                resolution.ResolutionDate = update.RevisedDate;
                resolution.TeamMember = resolvedBy ?? "N/A";
                return resolution;
            }
        }
    }
}
