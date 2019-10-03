using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Commands
{
    public abstract class GenerateWorkItemsReportHandlerBase<TCommand> : GenerateReportHandlerBase<TCommand>
        where TCommand : GenerateReportCommand
    {
        public GenerateWorkItemsReportHandlerBase(
            IIndex<string, IDataSource> dataSources,
            IRepository repository,
            ILogger logger)
            : base(dataSources, repository, logger)
        {
        }

        protected async Task<List<WorkItemViewModel>> GetAllWorkItems(IDataSource dataSource, IEnumerable<Guid> members)
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

        protected async Task<List<TeamMemberViewModel>> GetAllTeamMembers(IDataSource dataSource, IEnumerable<Guid> members)
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
