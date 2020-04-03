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
        private const string WorkItemsQueryTemplate = @"SELECT [System.Id] FROM WorkItems 
                            WHERE [System.Id] IN ({0})";

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
            var idsFromDataBase = inProgressWorkItems.Select(workItem => Convert.ToInt32(workItem.WorkItemId)).ToArray();
            var wiQuery = string.Format(WorkItemsQueryTemplate, string.Join(',', idsFromDataBase));

            if (!idsFromDataBase.Any())
            {
                return Enumerable.Empty<int>();
            }

            var client = await _clientFactory.GetClient();
            var queryResult = await client.ExecuteFlatQueryAsync(wiQuery);
            var idsFromAdo = queryResult.WorkItems.Select(w => w.Id).ToArray();

            if (!idsFromAdo.Any())
            {
                return Enumerable.Empty<int>();
            }

            var workItems = await client.GetWorkItemsAsync(idsFromAdo);
            var result = new List<int>();

            foreach (var workItem in workItems)
            {
                var workItemType = workItem.Fields?.Where(field => field.Key == "System.WorkItemType");

                if (!workItemType.Any())
                {
                    continue;
                }

                if (workItemType.First().Value != "Task" && workItemType.First().Value != "Bug")
                {
                    result.Add(workItem.Id);
                }
            }

            result.AddRange(idsFromDataBase.Except(idsFromAdo));

            return result;
        }
    }
}
