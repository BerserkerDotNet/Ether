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
        private readonly IVstsClientFactory _clientFactory;
        private readonly IMediator _mediator;
        private readonly ILogger<WorkItemsSyncJob> _logger;

        public WorkItemsSyncJob(IVstsClientFactory clientFactory, IMediator mediator, ILogger<WorkItemsSyncJob> logger)
        {
            _clientFactory = clientFactory;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Execute()
        {
            var allTeamMembers = await _mediator.RequestCollection<GetAllTeamMembers, TeamMemberViewModel>();

            // TODO: Parallel?
            foreach (var teamMember in allTeamMembers)
            {
                _logger.LogInformation("Fetching workitems for '{teamMember}'.", teamMember.Email);
                var workItems = await _mediator.RequestCollection<FetchWorkItemsFromProject, WorkItemViewModel>(new FetchWorkItemsFromProject(teamMember));
                _logger.LogInformation("Found {workitemsCount} workitems for '{teamMember}'.", workItems.Count(), teamMember.Email);
                await _mediator.Execute(new SaveWorkItemsForUser(workItems, teamMember));
            }
        }
    }
}
