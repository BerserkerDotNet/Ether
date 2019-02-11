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
                if (IsInCodeReview(workItem) && IsAssignedToTeamMember(workItem))
                {
                    report.WorkItemsInReview.Add(CreateWorkItemDetail(workItem));
                }
                else if (workItem.State == WorkItemStates.Active && IsAssignedToTeamMember(workItem))
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
                var timeSpent = GetActiveDuration(item);
                var originalEstimate = GetEtaValue(item, ETAFieldType.OriginalEstimate);
                var completedWork = GetEtaValue(item, ETAFieldType.CompletedWork);
                var remainingWork = GetEtaValue(item, ETAFieldType.RemainingWork);
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

            string FieldNameFor(string workItemType, ETAFieldType fieldType) => etaFields.First(f => f.WorkitemType == workItemType && f.FieldType == fieldType).FieldName;

            float GetEtaValue(VSTSWorkItem wi, ETAFieldType etaType)
            {
                var fieldName = FieldNameFor(wi.WorkItemType, etaType);
                if (!wi.Fields.ContainsKey(fieldName))
                    return 0;

                var value = wi.Fields[fieldName];
                if (string.IsNullOrEmpty(value))
                    return 0;

                return float.Parse(value);
            }

            return report;
        }

        private static bool IsInCodeReview(VSTSWorkItem item)
        {
            return item.State == WorkItemStates.Active 
                   && (WorkItemTags.ContainsTag(item.Updates.LastOrDefault(u => !u.Tags.IsEmpty)?.Tags.NewValue, WorkItemTags.CodeReview)
                       || item.Updates.Any(u => u.Relations?.Added != null && u.Relations.Added.Any(i => !string.IsNullOrWhiteSpace(i.Name) && i.Name.Equals("Pull Request", StringComparison.InvariantCultureIgnoreCase))));
        }

        private bool IsAssignedToTeamMember(VSTSWorkItem item)
        {
            var assignedTo = item.Updates.LastOrDefault(u => !u.AssignedTo.IsEmpty)?.AssignedTo.NewValue;
            return Input.Members.Any(m => !string.IsNullOrWhiteSpace(assignedTo) && assignedTo.Contains(m.Email));
        }

        private float GetActiveDuration(VSTSWorkItem workItem)
        {
            if (workItem.Updates == null || !workItem.Updates.Any())
                return 0.0f;

            var activeTime = 0.0F;
            var isActive = false;
            var assignedToTeam = false;
            DateTime? lastActivated = null;
            foreach (var update in workItem.Updates)
            {
                var isActivation = !update.State.IsEmpty && update.State.NewValue == WorkItemStates.Active;
                var isOnHold = !update.State.IsEmpty && update.State.NewValue == WorkItemStates.New;
                var isResolved = !update.State.IsEmpty && (update.State.NewValue == WorkItemStates.Resolved || update.State.NewValue == WorkItemStates.Closed);
                var isCodeReview = !update.Tags.IsEmpty && WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.CodeReview) || update.Relations?.Added != null 
                                   && update.Relations.Added.Any(i => !string.IsNullOrWhiteSpace(i.Name) && i.Name.Equals("Pull Request", StringComparison.InvariantCultureIgnoreCase));
                var isBlocked = !update.Tags.IsEmpty &&
                    (WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.Blocked) || WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.OnHold));
                var isUnBlocked = !isBlocked && !update.Tags.IsEmpty &&
                    (WorkItemTags.ContainsTag(update.Tags.OldValue, WorkItemTags.Blocked) && !WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.Blocked) ||
                     WorkItemTags.ContainsTag(update.Tags.OldValue, WorkItemTags.OnHold) && !WorkItemTags.ContainsTag(update.Tags.NewValue, WorkItemTags.OnHold));

                if (!assignedToTeam && !string.IsNullOrWhiteSpace(update.AssignedTo.NewValue))
                {
                    assignedToTeam = Input.Members.Any(m => update.AssignedTo.NewValue.Contains(m.Email));
                    if(isActive) lastActivated = update.ChangedDate;
                }

                if (isActive && (isOnHold || isBlocked))
                {
                    isActive = false;
                    if (lastActivated != null && assignedToTeam)
                        activeTime += CountBusinessDaysBetween(lastActivated.Value, update.ChangedDate);
                }
                else if ((isActivation && !isBlocked) || isUnBlocked)
                {
                    lastActivated = update.ChangedDate;
                    isActive = true;
                }

                else if (isActive && (isResolved || isCodeReview))
                {
                    if (lastActivated != null && assignedToTeam)
                        activeTime += CountBusinessDaysBetween(lastActivated.Value, update.ChangedDate);
                    break;
                }
            }

            return activeTime;
        }

        public static float CountBusinessDaysBetween(DateTime firstDay, DateTime lastDay, params DateTime[] holidays)
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
                throw new ArgumentException("Incorrect last day " + lastDay);

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days;
            if (businessDays == 0)
                return 1;

            int fullWeekCount = businessDays / 7;
            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = (int)firstDay.DayOfWeek;
                int lastDayOfWeek = (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                        businessDays -= 2;
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                        businessDays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                    businessDays -= 1;
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;

            // subtract the number of bank holidays during the time interval
            foreach (DateTime holiday in holidays)
            {
                DateTime bh = holiday.Date;
                if (firstDay <= bh && bh <= lastDay)
                    --businessDays;
            }

            return businessDays;
        }
    }
}