using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Queries
{
    // TODO: This is an almost complete copy of GenerateWorkitemsReportHandler
    public class GetActiveWorkitemsForProfileHandler : IQueryHandler<GetActiveWorkitemsForProfile, ActiveWorkitemsViewModel>
    {
        private readonly IIndex<string, IDataSource> _dataSources;
        private readonly IRepository _repository;
        private readonly ILogger<GetActiveWorkitemsForProfileHandler> _logger;

        public GetActiveWorkitemsForProfileHandler(IIndex<string, IDataSource> dataSources, IRepository repository, ILogger<GetActiveWorkitemsForProfileHandler> logger)
        {
            _dataSources = dataSources;
            _repository = repository;
            _logger = logger;
        }

        public async Task<ActiveWorkitemsViewModel> Handle(GetActiveWorkitemsForProfile query)
        {
            var dataSourceType = await _repository.GetFieldValueAsync<Profile, string>(p => p.Id == query.ProfileId, p => p.Type);
            if (!_dataSources.TryGetValue(dataSourceType, out var dataSource))
            {
                throw new ArgumentException($"Data source of type {dataSourceType} is not supported.");
            }

            var profile = await dataSource.GetProfile(query.ProfileId);
            if (profile == null)
            {
                throw new ArgumentException("Requested profile is not found.");
            }

            var result = new ActiveWorkitemsViewModel();

            if (profile.Members == null || !profile.Members.Any())
            {
                _logger.LogWarning("Profile '{ProfileName}({Profile})' does not have any members.", profile.Name, profile.Id);
                return result;
            }

            var workItems = await GetAllWorkItems(dataSource, profile.Members);
            if (!workItems.Any())
            {
                _logger.LogWarning("No work items found for members in '{ProfileName}({Profile})'", profile.Name, profile.Id);
                return result;
            }

            var team = await GetAllTeamMembers(dataSource, profile.Members);
            var workItemsInfo = new List<WorkitemInformationViewModel>(workItems.Count);
            foreach (var workItem in workItems)
            {
                var info = await dataSource.GetWorkItemInfo(workItem, team);
                if (info != null)
                {
                    workItemsInfo.Add(info);
                }
            }

            result.Workitems = workItemsInfo;

            return result;
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
