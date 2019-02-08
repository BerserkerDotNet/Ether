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
    public class GenerateWorkitemsReportHandler : ICommandHandler<GenerateWorkItemsReport, Guid>
    {
        private readonly IIndex<string, IDataSource> _dataSources;
        private readonly IWorkItemClassificationContext _workItemClassificationContext;
        private readonly IRepository _repository;
        private readonly ILogger<GenerateWorkitemsReportHandler> _logger;

        public GenerateWorkitemsReportHandler(
            IIndex<string, IDataSource> dataSources,
            IWorkItemClassificationContext workItemClassificationContext,
            IRepository repository,
            ILogger<GenerateWorkitemsReportHandler> logger)
        {
            _dataSources = dataSources;
            _workItemClassificationContext = workItemClassificationContext;
            _repository = repository;
            _logger = logger;
        }

        public async Task<Guid> Handle(GenerateWorkItemsReport command)
        {
            var dataSourceType = await _repository.GetFieldValueAsync<Profile, string>(p => p.Id == command.Profile, p => p.Type);
            if (!_dataSources.TryGetValue(dataSourceType, out var dataSource))
            {
                throw new ArgumentException($"Data source of type {dataSourceType} is not supported.");
            }

            var profile = await dataSource.GetProfile(command.Profile);
            if (profile == null)
            {
                throw new ArgumentException("Requested profile is not found.");
            }

            var workItems = await GetAllWorkItems(dataSource, profile.Members);
            if (!workItems.Any())
            {
                var empty = AggregatedWorkitemsETAReport.Empty;
            }

            var team = await GetAllTeamMembers(dataSource, profile.Members);
            var scope = new ClassificationScope(team, command.Start, command.End);
            var resolutions = workItems.SelectMany(w => _workItemClassificationContext.Classify(w, scope));

            var report = new WorkItemsReport();
            report.Id = Guid.NewGuid();
            report.DateTaken = DateTime.UtcNow;
            report.StartDate = command.Start;
            report.EndDate = command.End;
            report.ProfileName = profile.Name;
            report.ProfileId = profile.Id;
            report.ReportType = Constants.WorkitemsReportType;
            report.ReportName = Constants.WorkitemsReporterName;
            report.Resolutions = resolutions;

            await _repository.CreateAsync(report);

            return report.Id;
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
