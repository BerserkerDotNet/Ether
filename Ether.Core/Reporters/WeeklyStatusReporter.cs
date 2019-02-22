using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Core.Configuration;
using Ether.Core.Constants;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Core.Types;
using Ether.Core.Types.Exceptions;
using Ether.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ether.Core.Reporters
{
    public class WeeklyStatusReporter : ReporterBase
    {
        private IWorkItemClassificationContext _classificationContext;
        private IProgressReporter _progressReporter;

        public WeeklyStatusReporter(IRepository repository,
            IWorkItemClassificationContext classificationContext,
            IOptions<VSTSConfiguration> configuration,
            IProgressReporter progressReporter,
            ILogger<WeeklyStatusReporter> logger) : base(repository, configuration, logger)
        {
            _classificationContext = classificationContext;
            _progressReporter = progressReporter;
        }

        public override string Name => "Weekly Status report";
        public override Guid Id => Guid.Parse("59510DEB-33C5-4C58-BF7B-518191935AD6");
        public override Type ReportType => typeof(WeeklyStatusReport);

        protected override async Task<ReportResult> ReportInternal()
        {
            if (!Input.Members.Any() || !Input.Repositories.Any())
                return WeeklyStatusReport.Empty;

            var settings = await _repository.GetSingleAsync<Settings>(_ => true);
            var etaFields = settings?.WorkItemsSettings?.ETAFields;
            if (etaFields == null || !etaFields.Any())
                throw new MissingETASettingsException();

            await _progressReporter.Report("Fetching workitems...");
            var workItemIds = Input.Members.SelectMany(m => m.RelatedWorkItemIds);
            var workitems = (await _repository.GetAsync<VSTSWorkItem>(w => workItemIds.Contains(w.WorkItemId))).Where(w => w.WorkItemType != null && (w.WorkItemType == WorkItemTypes.Bug || w.WorkItemType == WorkItemTypes.Task));
            if (!workitems.Any())
                return WeeklyStatusReport.Empty;

            await _progressReporter.Report("Looking for work item resolutions...");
            var scope = new ClassificationScope(Input.Members, Input.Query.StartDate, Input.ActualEndDate);

            var report = WeeklyStatusReport.Empty;

            foreach (var workItem in workitems)
            {
                if (workItem.IsInCodeReview() && workItem.IsAssignedToTeamMember(Input.Members))
                {
                    report.WorkItemsInReview.Add(CreateWorkItemDetail(workItem));
                }
                else if (workItem.State == WorkItemStates.Active && workItem.IsAssignedToTeamMember(Input.Members))
                {
                    report.ActiveWorkItems.Add(CreateWorkItemDetail(workItem));
                }
                else
                {
                    var resolutions = _classificationContext.Classify(workItem, scope);
                    if (resolutions.Any(r => r.Resolution == WorkItemStates.Resolved || (r.WorkItemType == WorkItemTypes.Task && r.Resolution == WorkItemStates.Closed)))
                    {
                        report.ResolvedWorkItems.Add(CreateWorkItemDetail(workItem)); ;
                    }
                }
            }

            WeeklyStatusReport.WorkItemDetail CreateWorkItemDetail(VSTSWorkItem item)
            {
                var timeSpent = item.GetActiveDuration(Input.Members);
                var originalEstimate = item.GetEtaValue(ETAFieldType.OriginalEstimate, settings);
                var completedWork = item.GetEtaValue(ETAFieldType.CompletedWork, settings);
                var remainingWork = item.GetEtaValue(ETAFieldType.RemainingWork, settings);
                if (remainingWork < 1)
                {
                    remainingWork = originalEstimate;
                }

                return new WeeklyStatusReport.WorkItemDetail
                {
                    WorkItemId = item.WorkItemId,
                    WorkItemTitle = item.Title,
                    WorkItemType = item.WorkItemType,
                    WorkItemProject = string.IsNullOrWhiteSpace(item.AreaPath) ? null : item.AreaPath.Split('\\')[0],
                    Tags = item.Updates.LastOrDefault(u => !string.IsNullOrWhiteSpace(u.Tags.NewValue))?.Tags?.NewValue,
                    OriginalEstimate = originalEstimate,
                    EstimatedToComplete = remainingWork + completedWork,
                    TimeSpent = timeSpent
                };
            }

            return report;
        }
    }
}