using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Jobs
{
    public class WorkItemsSyncJob : IJob
    {
        public const string MembersParameterName = "Members";

        private readonly IMediator _mediator;
        private readonly ILogger<WorkItemsSyncJob> _logger;

        public WorkItemsSyncJob(IVstsClientFactory clientFactory, IMediator mediator, ILogger<WorkItemsSyncJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Execute(IReadOnlyDictionary<string, object> parameters)
        {
            var allTeamMembers = await _mediator.RequestCollection<GetAllTeamMembers, TeamMemberViewModel>();
            if (parameters.ContainsKey(MembersParameterName))
            {
                var members = (Guid[])parameters[MembersParameterName];
                if (members == null || !members.Any())
                {
                    _logger.LogWarning("Attempt to run a job for specific members, but with empty memebrs list.");
                    return;
                }

                _logger.LogInformation("Filtering members based on parameters");
                allTeamMembers = allTeamMembers.Where(m => members.Contains(m.Id)).ToList();
            }

            // TODO: Parallel?
            foreach (var teamMember in allTeamMembers)
            {
                await ExecuteForMemeber(teamMember);
            }
        }

        private async Task ExecuteForMemeber(TeamMemberViewModel teamMember)
        {
            _logger.LogInformation("Fetching workitems for '{teamMember}'.", teamMember.Email);
            var workItems = await _mediator.RequestCollection<FetchWorkItemsFromProject, WorkItemViewModel>(new FetchWorkItemsFromProject(teamMember));
            _logger.LogInformation("Found {workitemsCount} workitems for '{teamMember}'.", workItems.Count(), teamMember.Email);
            await _mediator.Execute(new SaveWorkItemsForUser(workItems, teamMember));
        }
    }
}
