using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using Microsoft.Extensions.Logging;
using static Ether.Contracts.Types.NullUtil;

namespace Ether.Vsts.Handlers.Queries
{
    public class FetchWorkItemsFromProjectHandler : IQueryHandler<FetchWorkItemsFromProject, IEnumerable<WorkItemViewModel>>
    {
        private const string WorkItemsQueryTemplate = @"SELECT [System.Id] FROM WorkItems 
                            WHERE [System.WorkItemType] IN ('Bug', 'Task') AND [System.AssignedTo] Ever '{0}' AND System.ChangedDate >= '{1}'";

        // TODO: Make configurable?
        private readonly string[] _workItemFields = new[]
        {
            "System.Id",
            "System.WorkItemType",
            "System.Title",
            "System.AreaPath",
            "System.ChangedDate",
            "System.Tags",
            "System.State",
            "System.Reason",
            "System.CreatedDate",
            "Microsoft.VSTS.Common.ResolvedDate",
            "Microsoft.VSTS.Common.ClosedDate",
            "Microsoft.VSTS.Common.StateChangeDate",
            "Microsoft.VSTS.Scheduling.OriginalEstimate",
            "Microsoft.VSTS.Scheduling.CompletedWork",
            "Microsoft.VSTS.Scheduling.RemainingWork"
        };

        private readonly IVstsClientFactory _clientFactory;
        private readonly ILogger<FetchWorkItemsFromProjectHandler> _logger;

        public FetchWorkItemsFromProjectHandler(IVstsClientFactory clientFactory, ILogger<FetchWorkItemsFromProjectHandler> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkItemViewModel>> Handle(FetchWorkItemsFromProject query)
        {
            CheckIfArgumentNull(query.Member, nameof(query.Member));

            var client = await _clientFactory.GetClient();

            var wiQuery = string.Format(WorkItemsQueryTemplate, query.Member.Email, DateTime.UtcNow.AddYears(-10).ToString("MM/dd/yyyy"));
            var queryResult = await client.ExecuteFlatQueryAsync(wiQuery);
            var ids = queryResult.WorkItems.Select(w => w.Id).ToArray();
            var workItems = await client.GetWorkItemsAsync(ids, fields: _workItemFields);

            var result = new List<WorkItemViewModel>(workItems.Count());
            foreach (var workItem in workItems)
            {
                var updates = await client.GetWorkItemUpdatesAsync(workItem.Id);
                var viewModel = new WorkItemViewModel
                {
                    WorkItem = workItem,
                    Updates = updates
                };
                result.Add(viewModel);
            }

            return result;
        }
    }
}
