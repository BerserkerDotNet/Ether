using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Commands
{
    public class GenerateAggregatedWorkitemsETAReportHandler : GenerateReportHandlerBase<GenerateAggregatedWorkitemsETAReport>
    {
        private readonly IWorkItemClassificationContext _workItemClassificationContext;

        public GenerateAggregatedWorkitemsETAReportHandler(
            IIndex<string, IDataSource> dataSources,
            IWorkItemClassificationContext workItemClassificationContext,
            IRepository repository,
            ILogger<GenerateAggregatedWorkitemsETAReportHandler> logger)
            : base(dataSources, repository, logger)
        {
            _workItemClassificationContext = workItemClassificationContext;
        }

        protected override async Task<ReportResult> GenerateAsync(GenerateAggregatedWorkitemsETAReport command, IDataSource dataSource, ProfileViewModel profile)
        {
            var report = await GenerateReport(command, dataSource, profile);
            return report;
        }

        protected override (string type, string name) GetReportInfo()
        {
            return (Constants.ETAReportType, Constants.ETAReportName);
        }

        private async Task<AggregatedWorkitemsETAReport> GenerateReport(GenerateAggregatedWorkitemsETAReport command, IDataSource dataSource, ProfileViewModel profile)
        {
            var workItems = await GetAllWorkItems(dataSource, profile.Members);
            if (!workItems.Any())
            {
                return AggregatedWorkitemsETAReport.Empty;
            }

            var team = await GetAllTeamMembers(dataSource, profile.Members);
            var scope = new ClassificationScope(team, command.Start, command.End);
            var resolutions = workItems.SelectMany(w => _workItemClassificationContext.Classify(w, scope))
                .GroupBy(r => r.MemberEmail)
                .ToDictionary(k => k.Key, v => v.AsEnumerable());

            var report = new AggregatedWorkitemsETAReport(team.Count());
            foreach (var member in team)
            {
                var individualReport = await GetIndividualReport(resolutions, workItems, dataSource, member);
                report.IndividualReports.Add(individualReport);
            }

            return report;
        }

        private async Task<AggregatedWorkitemsETAReport.IndividualETAReport> GetIndividualReport(
            Dictionary<string, IEnumerable<WorkItemResolution>> resolutions,
            IEnumerable<WorkItemViewModel> workItems,
            IDataSource dataSource,
            TeamMemberViewModel member)
        {
            if (!resolutions.ContainsKey(member.Email))
            {
                return AggregatedWorkitemsETAReport.IndividualETAReport.GetEmptyFor(member);
            }

            var individualReport = new AggregatedWorkitemsETAReport.IndividualETAReport
            {
                MemberEmail = member.Email,
                MemberName = member.DisplayName
            };

            await PopulateMetrics(resolutions, workItems, dataSource, member.Email, individualReport);

            return individualReport;
        }

        private async Task PopulateMetrics(
            Dictionary<string, IEnumerable<WorkItemResolution>> resolutions,
            IEnumerable<WorkItemViewModel> workItems,
            IDataSource dataSource,
            string email,
            AggregatedWorkitemsETAReport.IndividualETAReport report)
        {
            var resolvedByMember = resolutions[email];
            report.TotalResolved = resolvedByMember.Count();
            report.TotalResolvedBugs = resolvedByMember.Count(w => string.Equals(w.WorkItemType, "Bug", StringComparison.OrdinalIgnoreCase));
            report.TotalResolvedTasks = resolvedByMember.Count(w => string.Equals(w.WorkItemType, "Task", StringComparison.OrdinalIgnoreCase));
            report.Details = new List<AggregatedWorkitemsETAReport.IndividualReportDetail>(report.TotalResolved);
            foreach (var item in resolvedByMember)
            {
                var workitem = workItems.Single(w => w.WorkItemId == item.WorkItemId);
                var timeSpent = dataSource.GetActiveDuration(workitem);
                var eta = await dataSource.GetETAValues(workitem);
                if (eta.IsEmpty)
                {
                    report.WithoutETA++;
                    report.CompletedWithoutEstimates += timeSpent;
                }
                else
                {
                    var estimatedByDev = eta.CompletedWork + eta.RemainingWork;
                    if (estimatedByDev == 0)
                    {
                        estimatedByDev = eta.OriginalEstimate;
                    }

                    if (eta.OriginalEstimate != 0)
                    {
                        report.WithOriginalEstimate++;
                    }

                    report.OriginalEstimated += eta.OriginalEstimate;
                    report.EstimatedToComplete += estimatedByDev;

                    report.CompletedWithEstimates += timeSpent;
                }

                report.Details.Add(new AggregatedWorkitemsETAReport.IndividualReportDetail
                {
                    WorkItemId = item.WorkItemId,
                    WorkItemTitle = item.WorkItemTitle,
                    WorkItemType = item.WorkItemType,
                    OriginalEstimate = eta.OriginalEstimate,
                    EstimatedToComplete = eta.RemainingWork + eta.CompletedWork,
                    TimeSpent = timeSpent,
                });
            }
        }

        private async Task<List<WorkItemViewModel>> GetAllWorkItems(IDataSource dataSource, IEnumerable<Guid> members)
        {
            var allWorkItems = new List<WorkItemViewModel>();
            foreach (var member in members)
            {
                var workItems = await dataSource.GetWorkItemsFor(member);
                allWorkItems.AddRange(workItems);
            }

            return allWorkItems.Distinct().ToList();
        }

        private async Task<List<TeamMemberViewModel>> GetAllTeamMembers(IDataSource dataSource, IEnumerable<Guid> members)
        {
            var allMembers = new List<TeamMemberViewModel>();
            foreach (var member in members)
            {
                var teamMember = await dataSource.GetTeamMember(member);
                allMembers.Add(teamMember);
            }

            return allMembers;
        }
    }
}
