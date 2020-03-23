using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Dto;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class FetchWorkItemsOtherThanBugsAndTasksHandler : IQueryHandler<FetchWorkItemsOtherThanBugsAndTasks, IEnumerable<int>>
    {
        private readonly IVstsClientFactory _clientFactory;
        private readonly IRepository _repository;

        public FetchWorkItemsOtherThanBugsAndTasksHandler(IVstsClientFactory clientFactory, IRepository repository)
        {
            _clientFactory = clientFactory;
            _repository = repository;
        }

        public async Task<IEnumerable<int>> Handle(FetchWorkItemsOtherThanBugsAndTasks query)
        {
            var inProgressWorkItems = await _repository.GetByFilteredArrayAsync<WorkItem>("Fields.v", new[] { "New", "Active" });

            // TODO: Find better way to query this
            var ids = inProgressWorkItems.Select(workItem => Convert.ToInt32(workItem.Fields["System.Id"])).ToArray();

            if (!ids.Any())
            {
                return Enumerable.Empty<int>();
            }

            var client = await _clientFactory.GetClient();
            var workItems = await client.GetWorkItemsAsync(ids);
            var result = new List<int>();

            foreach (var workItem in workItems)
            {
                var workItemType = workItem.Fields?.Where(field => field.Key == "System.WorkItemType");

                if (!workItemType.Any())
                {
                    return Enumerable.Empty<int>();
                }

                if (workItemType.First().Value != "Task" && workItemType.First().Value != "Bug")
                {
                    result.Add(workItem.Id);
                }
            }

            return result;
        }
    }
}
