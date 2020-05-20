using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Ether.Vsts.Commands;
using Ether.Vsts.Queries;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Jobs
{
    public class WorkItemsSyncJob : IJob
    {
        public const string MembersParameterName = "Members";

        private readonly IMediator _mediator;
        private readonly ILogger<WorkItemsSyncJob> _logger;

        public WorkItemsSyncJob(IMediator mediator, ILogger<WorkItemsSyncJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<JobDetails> Execute(IReadOnlyDictionary<string, object> parameters)
        {
            var allTeamMembers = await _mediator.RequestCollection<GetAllTeamMembers, TeamMemberViewModel>();
            if (parameters.ContainsKey(MembersParameterName))
            {
                var members = (Guid[])parameters[MembersParameterName];
                if (members == null || !members.Any())
                {
                    _logger.LogWarning("Attempt to run a job for specific members, but with empty memebrs list.");
                    return null;
                }

                _logger.LogInformation("Filtering members based on parameters");
                allTeamMembers = allTeamMembers.Where(m => members.Contains(m.Id)).ToList();
            }

            // TODO: Parallel?
            foreach (var teamMember in allTeamMembers)
            {
                await ExecuteForMemeber(teamMember);
            }

            return null;
        }

        private async Task ExecuteForMemeber(TeamMemberViewModel teamMember)
        {
            var relatedOrganizations = await _mediator.RequestCollection<GetTeamMemberRelatedOrganizations, OrganizationViewModel>(new GetTeamMemberRelatedOrganizations(teamMember.Id));

            foreach (var relatedOrganization in relatedOrganizations)
            {
                _logger.LogInformation("Fetching workitems for '{teamMember}'.", teamMember.Email);
                var workItems = await _mediator.RequestCollection<FetchWorkItemsFromProject, WorkItemViewModel>(new FetchWorkItemsFromProject(teamMember, relatedOrganization.Id));
                _logger.LogInformation("Found {workitemsCount} workitems for '{teamMember}'.", workItems.Count(), teamMember.Email);
                await _mediator.Execute(new SaveWorkItemsForUser(workItems, teamMember));
                _logger.LogInformation("Fetching workitems other than Bugs and Tasks.");
                var workItemsToDeleteIds = await _mediator.RequestCollection<FetchWorkItemsOtherThanBugsAndTasks, int>(new FetchWorkItemsOtherThanBugsAndTasks(relatedOrganization.Id));
                _logger.LogInformation("Deleting workitems other than Bugs and Tasks.");
                await _mediator.Execute(new DeleteWorkItems(workItemsToDeleteIds));
            }
        }
    }
}
