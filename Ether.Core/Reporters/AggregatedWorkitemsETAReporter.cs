using Ether.Core.Configuration;
using Ether.Core.Constants;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Core.Types;
using Ether.Core.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Core.Reporters
{
    public class AggregatedWorkitemsETAReporter : ReporterBase
    {
        private readonly IWorkItemClassificationContext _classificationContext;
        private readonly IProgressReporter _progressReporter;

        public AggregatedWorkitemsETAReporter(IRepository repository, 
            IWorkItemClassificationContext classificationContext,
            IOptions<VSTSConfiguration> configuration,
            IProgressReporter progressReporter,
            ILogger<AggregatedWorkitemsETAReporter> logger)
            : base(repository, configuration, logger)
        {
            _classificationContext = classificationContext;
            _progressReporter = progressReporter;
        }

        public override string Name => "Aggregated workitems ETA report";

        public override Guid Id => Guid.Parse("eda51f15-af36-4d75-a76a-cd74dfa13298");

        public override Type ReportType => typeof(AggregatedWorkitemsETAReport);

        protected override async Task<ReportResult> ReportInternal()
        {
            if (!Input.Members.Any() || !Input.Repositories.Any())
                return AggregatedWorkitemsETAReport.Empty;

            var settings = await _repository.GetSingleAsync<Settings>(_ => true);
            var etaFields = settings?.WorkItemsSettings?.ETAFields;
            if (etaFields == null || !etaFields.Any())
                throw new MissingETASettingsException();

            await _progressReporter.Report("Fetching workitems...");
            var workItemIds = Input.Members.SelectMany(m => m.RelatedWorkItemIds);
            var workitems = await _repository.GetAsync<VSTSWorkItem>(w => workItemIds.Contains(w.WorkItemId));
            if (!workitems.Any())
                return AggregatedWorkitemsETAReport.Empty;

            await _progressReporter.Report("Looking for work item resolutions...");
            var scope = new ClassificationScope(Input.Members, Input.Query.StartDate, Input.ActualEndDate);
            var resolutions = workitems.SelectMany(w => _classificationContext.Classify(w, scope))
                .Where(r => r.Resolution == WorkItemStates.Resolved || (r.WorkItemType == WorkItemTypes.Task && r.Resolution == WorkItemStates.Closed))
                .GroupBy(r => r.MemberEmail)
                .ToDictionary(k => k.Key, v => v.AsEnumerable());

            var result = new AggregatedWorkitemsETAReport();
            result.IndividualReports = new List<AggregatedWorkitemsETAReport.IndividualETAReport>(Input.Members.Count());
            foreach (var member in Input.Members)
            {
                await _progressReporter.Report($"Calculating metrics for {member.DisplayName}", GetProgressStep());
                var individualReport = GetIndividualReport(member);
                result.IndividualReports.Add(individualReport);
            }

            return result;

            // Local methods
            AggregatedWorkitemsETAReport.IndividualETAReport GetIndividualReport(TeamMember member)
            {
                if (!resolutions.ContainsKey(member.Email))
                    return AggregatedWorkitemsETAReport.IndividualETAReport.GetEmptyFor(member);

                var individualReport = new AggregatedWorkitemsETAReport.IndividualETAReport
                {
                    MemberEmail = member.Email,
                    MemberName = member.DisplayName
                };
                PopulateMetrics(member.Email, individualReport);

                return individualReport;
            }

            void PopulateMetrics(string email, AggregatedWorkitemsETAReport.IndividualETAReport report)
            {
                var resolved = resolutions[email];
                report.TotalResolved = resolved.Count();
                report.TotalResolvedBugs = resolved.Count(w => string.Equals(w.WorkItemType, "Bug", StringComparison.OrdinalIgnoreCase));
                report.TotalResolvedTasks = resolved.Count(w => string.Equals(w.WorkItemType, "Task", StringComparison.OrdinalIgnoreCase));
                report.Details = new List<AggregatedWorkitemsETAReport.IndividualReportDetail>(report.TotalResolved);
                foreach (var item in resolved)
                {
                    var workitem = workitems.Single(w => w.WorkItemId == item.WorkItemId);
                    var timeSpent = GetActiveDuration(workitem);
                    var originalEstimate = GetEtaValue(workitem, ETAFieldType.OriginalEstimate);
                    var completedWork = GetEtaValue(workitem, ETAFieldType.CompletedWork);
                    var remainingWork = GetEtaValue(workitem, ETAFieldType.RemainingWork);
                    if (IsETAEmpty(workitem))
                    {
                        report.WithoutETA++;
                        report.CompletedWithoutEstimates += timeSpent;
                    }
                    else
                    {
                        var estimatedByDev = completedWork + remainingWork;
                        if (estimatedByDev == 0)
                            estimatedByDev = originalEstimate;

                        if (originalEstimate != 0)
                            report.WithOriginalEstimate++;

                        report.OriginalEstimated += originalEstimate;
                        report.EstimatedToComplete += estimatedByDev;

                        report.CompletedWithEstimates += timeSpent;
                    }

                    report.Details.Add(new AggregatedWorkitemsETAReport.IndividualReportDetail
                    {
                        WorkItemId = item.WorkItemId,
                        WorkItemTitle = item.WorkItemTitle,
                        WorkItemType = item.WorkItemType,
                        OriginalEstimate = originalEstimate,
                        EstimatedToComplete = remainingWork + completedWork,
                        TimeSpent = timeSpent,
                    });
                }
            }

            bool IsETAEmpty(VSTSWorkItem wi) =>
                IsNullOrEmpty(wi, FieldNameFor(wi.WorkItemType, ETAFieldType.OriginalEstimate)) &&
                IsNullOrEmpty(wi, FieldNameFor(wi.WorkItemType, ETAFieldType.CompletedWork)) &&
                IsNullOrEmpty(wi, FieldNameFor(wi.WorkItemType, ETAFieldType.RemainingWork));

            bool IsNullOrEmpty(VSTSWorkItem wi, string fieldName) => !wi.Fields.ContainsKey(fieldName) || string.IsNullOrEmpty(wi.Fields[fieldName]);

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
        }

        private float GetActiveDuration(VSTSWorkItem workItem)
        {
            if (workItem.Updates == null || !workItem.Updates.Any())
                return 0.0f;

            var activeTime = 0.0F;
            var isActive = false;
            DateTime? lastActivated = null;
            foreach (var update in workItem.Updates)
            {
                var isActivation = !update.State.IsEmpty && update.State.NewValue == WorkItemStates.Active;
                var isOnHold = !update.State.IsEmpty && update.State.NewValue == WorkItemStates.New;
                var isResolved = !update.State.IsEmpty && (update.State.NewValue == WorkItemStates.Resolved || update.State.NewValue == WorkItemStates.Closed);
                var isCodeReview = !update.Tags.IsEmpty && ContainsCodeReviewTag(update.Tags.NewValue);
                var isBlocked = !update.Tags.IsEmpty && ContainsBlockedTag(update.Tags.NewValue);
                var isUnBlocked = !update.Tags.IsEmpty && ContainsBlockedTag(update.Tags.OldValue) && !ContainsBlockedTag(update.Tags.NewValue);

                if (isActivation || isUnBlocked)
                {
                    lastActivated = update.ChangedDate;
                    isActive = true;
                }
                else if (isActive && (isOnHold || isBlocked))
                {
                    isActive = false;
                    if (lastActivated != null)
                        activeTime += CountBusinessDaysBetween(lastActivated.Value,  update.ChangedDate);
                }
                else if (isActive && (isResolved || isCodeReview))
                {
                    if (lastActivated != null)
                        activeTime += CountBusinessDaysBetween(lastActivated.Value, update.ChangedDate);
                    break;
                }
            }

            return activeTime;
        }

        private bool ContainsCodeReviewTag(string tags)
        {
            return tags.Split(';')
                .Select(t => t.Replace(" ", "").ToLower())
                .Contains("codereview");
        }

        private bool ContainsBlockedTag(string tags)
        {
            if (string.IsNullOrEmpty(tags))
                return false;

            return tags.Split(';')
                .Select(t => t.Replace(" ", "").ToLower())
                .Contains("blocked");
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

        private float GetProgressStep()
        {
            return 100.0F / Input.Members.Count();
        }
    }
}
