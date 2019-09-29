using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.ViewModels.Types;
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
            Constants.WorkItemStateField,
            "System.Reason",
            "System.AssignedTo",
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
        private readonly IMapper _mapper;

        public FetchWorkItemsFromProjectHandler(IVstsClientFactory clientFactory, ILogger<FetchWorkItemsFromProjectHandler> logger, IMapper mapper)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkItemViewModel>> Handle(FetchWorkItemsFromProject query)
        {
            CheckIfArgumentNull(query.Member, nameof(query.Member));

            var client = await _clientFactory.GetClient();

            var lastFetchDate = query.Member.LastWorkitemsFetchDate.HasValue ? query.Member.LastWorkitemsFetchDate.Value : DateTime.UtcNow.AddYears(-10);
            var wiQuery = string.Format(WorkItemsQueryTemplate, query.Member.Email, lastFetchDate.ToString("MM/dd/yyyy"));
            var queryResult = await client.ExecuteFlatQueryAsync(wiQuery);
            var ids = queryResult.WorkItems.Select(w => w.Id).ToArray();
            if (!ids.Any())
            {
                return Enumerable.Empty<WorkItemViewModel>();
            }

            var workItems = await client.GetWorkItemsAsync(ids);
            var result = new List<WorkItemViewModel>(workItems.Count());
            foreach (var workItem in workItems)
            {
                var updates = await client.GetWorkItemUpdatesAsync(workItem.Id);
                var viewModel = new WorkItemViewModel
                {
                    WorkItemId = workItem.Id,
                    Fields = workItem.Fields?.Where(f => _workItemFields.Contains(f.Key)).ToDictionary(k => k.Key, v => v.Value),
                    Relations = workItem.Relations?.Select(r => new WorkItemRelation
                    {
                        Attributes = r.Attributes,
                        RelationType = r.RelationType,
                        Url = r.Url
                    }),
                    Updates = updates.Select(u => _mapper.Map<WorkItemUpdateViewModel>(u))
                };
                result.Add(viewModel);
            }

            return result;
        }
    }
}
