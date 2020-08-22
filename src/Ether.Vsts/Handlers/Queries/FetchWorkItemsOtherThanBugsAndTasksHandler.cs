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
        private const string WorkItemsQueryTemplate = @"SELECT [System.Id] FROM WorkItems WHERE ([System.WorkItemType] != 'Bug' AND [System.WorkItemType]!='Task') AND [System.Id] IN ({0})";

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
            if (!idsFromDataBase.Any())
            {
                return Enumerable.Empty<int>();
            }

            var client = await _clientFactory.GetClient(query.OrganizationId);

            var wiQuery = string.Format(WorkItemsQueryTemplate, string.Join(',', idsFromDataBase));
            var queryResult = await client.ExecuteFlatQueryAsync(wiQuery);

            var idsFromAdoThatIsNotTaskOrBug = queryResult.WorkItems.Select(w => w.Id).ToArray();
            return idsFromAdoThatIsNotTaskOrBug;
        }
    }
}
