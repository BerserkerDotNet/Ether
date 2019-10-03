using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Commands
{
    public class GenerateReOpenedWorkitemsReportHandler : GenerateWorkItemsReportHandlerBase<GenerateReOpenedWorkItemsReport>
    {
        private readonly IWorkItemClassificationContext _workItemClassificationContext;

        public GenerateReOpenedWorkitemsReportHandler(
            IIndex<string, IDataSource> dataSources,
            IWorkItemClassificationContext workItemClassificationContext,
            IRepository repository,
            ILogger<GenerateReOpenedWorkitemsReportHandler> logger)
            : base(dataSources, repository, logger)
        {
            _workItemClassificationContext = workItemClassificationContext;
        }

        protected override async Task<ReportResult> GenerateAsync(GenerateReOpenedWorkItemsReport command, IDataSource dataSource, ProfileViewModel profile)
        {
            // TODO: This part is duplicated between work item reporters, move to base class
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

            var report = new ReOpenedWorkItemsReport();
            foreach (var workItem in workItems)
            {
                try
                {
                    var resolutions = _workItemClassificationContext.Classify(workItem, scope);
                    var details = resolutions.OfType<WorkItemReOpenedEvent>()
                        .Select(e => new ReOpenedWorkItemDetail
                        {
                            WorkItemId = e.WorkItem.Id,
                            WorkItemTitle = e.WorkItem.Title,
                            WorkItemType = e.WorkItem.Type,
                            WorkItemProject = string.Empty,
                            ReOpenedDate = e.Date,
                            AssociatedUser = e.AssociatedUser
                        });
                    report.Details.AddRange(details);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error generating details for workitem {WorkItemId}", workItem.WorkItemId);
                }
            }

            return report;
        }

        protected override (string type, string name) GetReportInfo()
        {
            return (Constants.ReOpenedWorkitemsReportType, Constants.ReOpenedWorkitemsReporterName);
        }
    }
}
