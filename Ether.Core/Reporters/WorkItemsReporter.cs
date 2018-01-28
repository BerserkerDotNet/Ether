using Ether.Core.Interfaces;
using Ether.Core.Configuration;
using Ether.Core.Models.DTO.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Core.Models;
using Ether.Core.Models.VSTS;
using static Ether.Core.Models.DTO.Reports.WorkItemsReport;

namespace Ether.Core.Reporters
{
    public class WorkItemsReporter : ReporterBase
    {
        private static readonly DateTime VSTSMaxDate = new DateTime(9999, 1, 1);
        private static readonly Guid _reporterId = Guid.Parse("54c62ebe-cfef-46d5-b90f-ebb00a1611b7");
       
        private readonly IEnumerable<IWorkItemsClassifier> _classifiers;

        public WorkItemsReporter(IRepository repository, 
            IOptions<VSTSConfiguration> configuration,
            IEnumerable<IWorkItemsClassifier> classifiers, 
            ILogger<WorkItemsReporter> logger) 
            : base(repository, configuration, logger)
        {
            _classifiers = classifiers;
        }

        public override string Name => "Work items report (alpha)";
        public override Guid Id => _reporterId;
        public override Type ReportType => typeof(WorkItemsReport);

        protected override async Task<ReportResult> ReportInternal()
        {
            var workItems = await GetWorkItemsForPeriod(Input.ActualEndDate);
            var resolutions = new List<WorkItemResolution>(workItems.Count() * 2);
            foreach (var workItem in workItems)
            {
                var rs = from c in _classifiers
                         let r = c.Classify(new WorkItemResolutionRequest { WorkItem = workItem, Team = Input.Members })
                         where !r.IsNone && IsInRange(r)
                         select r;

                resolutions.AddRange(rs);
            }

            var result = new WorkItemsReport();
            result.Resolutions = resolutions;
            return result;
        }

        private async Task<IEnumerable<VSTSWorkItem>> GetWorkItemsForPeriod(DateTime endDate)
        {
            var workItemsToFetch = Input.Members
                .Where(m => m.RelatedWorkItemIds != null)
                .SelectMany(m => m.RelatedWorkItemIds);
            var relatedItems =  await _repository.GetAsync<VSTSWorkItem>(w => workItemsToFetch.Contains(w.WorkItemId));

            return relatedItems.Where(w => !w.CreatedDate.HasValue || w.CreatedDate <= endDate);
        }

        private bool IsInRange(WorkItemResolution r)
        {
            return (r.ResolutionDate >= Input.Query.StartDate && r.ResolutionDate <= Input.ActualEndDate)
                || r.ResolutionDate == VSTSMaxDate;
        }
    }
}
