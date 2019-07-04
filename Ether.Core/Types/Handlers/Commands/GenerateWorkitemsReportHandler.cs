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
    public class GenerateWorkitemsReportHandler : GenerateReportHandlerBase<GenerateWorkItemsReport>
    {
        private readonly IWorkItemClassificationContext _workItemClassificationContext;

        public GenerateWorkitemsReportHandler(
            IIndex<string, IDataSource> dataSources,
            IWorkItemClassificationContext workItemClassificationContext,
            IRepository repository,
            ILogger<GenerateWorkitemsReportHandler> logger)
            : base(dataSources, repository, logger)
        {
            _workItemClassificationContext = workItemClassificationContext;
        }

        protected override async Task<ReportResult> GenerateAsync(GenerateWorkItemsReport command, IDataSource dataSource, ProfileViewModel profile)
        {
            var report = await GenerateReport(dataSource, profile, command);
            return report;
        }

        protected override (string type, string name) GetReportInfo()
        {
            return (Constants.WorkitemsReportType, Constants.WorkitemsReporterName);
        }

        private async Task<WorkItemsReport> GenerateReport(IDataSource dataSource, ProfileViewModel profile, GenerateWorkItemsReport command)
        {
            if (profile.Members == null || !profile.Members.Any())
            {
                Logger.LogWarning("Profile '{ProfileName}({Profile})' does not have any members.", profile.Name, profile.Id);
                return WorkItemsReport.Empty;
            }

            var workItems = await GetAllWorkItems(dataSource, profile.Members);
            if (!workItems.Any())
            {
                Logger.LogWarning("No work items found for members in '{ProfileName}({Profile})'", profile.Name, profile.Id);
                return WorkItemsReport.Empty;
            }

            var team = await GetAllTeamMembers(dataSource, profile.Members);
            var scope = new ClassificationScope(team, command.Start, command.End);

            var report = WorkItemsReport.Empty;
            foreach (var workItem in workItems)
            {
                var isInCodeReview = await dataSource.IsInCodeReview(workItem);
                if (isInCodeReview && dataSource.IsAssignedToTeamMember(workItem, team))
                {
                    report.WorkItemsInReview.Add(dataSource.CreateWorkItemDetail(workItem, team));
                }
                else if (dataSource.IsActive(workItem) && dataSource.IsAssignedToTeamMember(workItem, team))
                {
                    report.ActiveWorkItems.Add(dataSource.CreateWorkItemDetail(workItem, team));
                }
                else
                {
                    var resolutions = _workItemClassificationContext.Classify(workItem, scope);
                    if (dataSource.IsResolved(resolutions))
                    {
                        report.ResolvedWorkItems.Add(dataSource.CreateWorkItemDetail(workItem, team));
                    }
                }
            }

            return report;
        }

        private async Task<List<WorkItemViewModel>> GetAllWorkItems(IDataSource dataSource, IEnumerable<Guid> members)
        {
            var allWorkItems = new List<WorkItemViewModel>();

            // TODO: Parallel
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

            // TODO: Parallel
            foreach (var member in members)
            {
                var teamMember = await dataSource.GetTeamMember(member);
                allMembers.Add(teamMember);
            }

            return allMembers;
        }
    }
}
