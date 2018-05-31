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

        public AggregatedWorkitemsETAReporter(IRepository repository, IWorkItemClassificationContext classificationContext, IOptions<VSTSConfiguration> configuration, ILogger<AggregatedWorkitemsETAReporter> logger)
            : base(repository, configuration, logger)
        {
            _classificationContext = classificationContext;
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
            if (etaFields != null && !etaFields.Any())
                throw new MissingETASettingsException();

            var workItemIds = Input.Members.SelectMany(m => m.RelatedWorkItemIds);
            var workitems = await _repository.GetAsync<VSTSWorkItem>(w => workItemIds.Contains(w.WorkItemId));
            if (!workitems.Any())
                return AggregatedWorkitemsETAReport.Empty;

            var scope = new ClassificationScope(Input.Members, Input.Query.StartDate, Input.ActualEndDate);
            var resolutions = workitems.SelectMany(w => _classificationContext.Classify(w, scope))
                .Where(r => r.Resolution == WorkItemStates.Resolved)
                .GroupBy(r => r.MemberEmail)
                .ToDictionary(k => k.Key, v => v.AsEnumerable());

            var result = new AggregatedWorkitemsETAReport();
            result.IndividualReports = new List<AggregatedWorkitemsETAReport.IndividualETAReport>(Input.Members.Count());
            foreach (var member in Input.Members)
            {
                var individualReport = GetIndividualReport(member);
                result.IndividualReports.Add(individualReport);
            }

            return result;


            // Local methods
            AggregatedWorkitemsETAReport.IndividualETAReport GetIndividualReport(TeamMember member)
            {
                if (!resolutions.ContainsKey(member.Email))
                    return AggregatedWorkitemsETAReport.IndividualETAReport.GetEmptyFor(member.Email);

                var individualReport = new AggregatedWorkitemsETAReport.IndividualETAReport
                {
                    TeamMember = member.Email,
                };
                PopulateMetrics(member.Email, individualReport);

                return individualReport;
            }

            void PopulateMetrics(string email, AggregatedWorkitemsETAReport.IndividualETAReport report)
            {
                var resolved = resolutions[email];
                report.TotalResolved = resolved.Count();
                foreach (var item in resolved)
                {
                    var workitem = workitems.Single(w => w.WorkItemId == item.WorkItemId);
                    if (IsETAEmpty(workitem))
                    {
                        report.WithoutETA++;
                    }
                    else
                    {
                        var originalEstimate = GetEtaValue(workitem, ETAFieldType.OriginalEstimate);
                        var completedWork = GetEtaValue(workitem, ETAFieldType.CompletedWork);
                        var remainingWork = GetEtaValue(workitem, ETAFieldType.RemainingWork);

                        var estimatedByDev = completedWork + remainingWork;
                        if (estimatedByDev == 0)
                            estimatedByDev = originalEstimate;

                        report.TotalEstimated = estimatedByDev;
                    }
                }
            }

            bool IsETAEmpty(VSTSWorkItem wi) =>
                !wi.Fields.ContainsKey(FieldNameFor(wi.WorkItemType, ETAFieldType.OriginalEstimate)) &&
                !wi.Fields.ContainsKey(FieldNameFor(wi.WorkItemType, ETAFieldType.CompletedWork)) &&
                !wi.Fields.ContainsKey(FieldNameFor(wi.WorkItemType, ETAFieldType.RemainingWork));

            string FieldNameFor(string workItemType, ETAFieldType fieldType) => etaFields.First(f => f.WorkitemType == workItemType && f.FieldType == fieldType).FieldName;

            float GetEtaValue(VSTSWorkItem wi, ETAFieldType etaType)
            {
                var value = wi.Fields[FieldNameFor(wi.WorkItemType, etaType)];
                if (string.IsNullOrEmpty(value))
                    return 0;

                return float.Parse(value);
            }
        }
    }
}
