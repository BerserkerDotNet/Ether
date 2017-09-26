using Ether.Core.Configuration;
using Ether.Core.Data;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Jobs
{
    public class WorkItemsFetchJob : IJob
    {
        private const string WorkItemsQuery = @"SELECT [System.Id] FROM WorkItems 
                            WHERE [System.WorkItemType] IN ('Bug', 'Task') AND [System.AssignedTo] Ever '{0}' AND System.ChangedDate >= '{1}'";

        private readonly IRepository _repository;
        private readonly IVSTSClient _client;
        private readonly ILogger<WorkItemsFetchJob> _logger;
        private readonly IOptions<VSTSConfiguration> _config;

        public WorkItemsFetchJob(IRepository repository, IVSTSClient client, IOptions<VSTSConfiguration> config, ILogger<WorkItemsFetchJob> logger)
        {
            _repository = repository;
            _client = client;
            _logger = logger;
            _config = config;
        }

        public void Execute()
        {
            if (_config.Value ==null || !_config.Value.IsValid)
            {
                _logger.LogError("Failed to start fetching workitems. Configuration is invalid!");
                return;
            }

            try
            {
                var members = GetMembers();
                var projects = _repository.Get<VSTSProject>(p => !p.DoesNotHaveWorkItems);
                foreach (var member in members)
                {
                    RunFor(member, projects).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch workitems.");
            }
        }

        private async Task RunFor(TeamMember member, IEnumerable<VSTSProject> projects)
        {
            try
            {
                _logger.LogInformation("Starting to fetch workitems for '{0}'", member.Email);
                await FetchWorkItemsFor(member, projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch workitems for '{0}'", member.Email);
            }
        }

        private async Task FetchWorkItemsFor(TeamMember member, IEnumerable<VSTSProject> projects)
        {
            var changedWorkItems = await FetchRelatedWorkItems(member, projects);
            _logger.LogInformation("Fetched related workitem ids, total count is {0}", member.RelatedWorkItemIds.Count());

            var workItems = await GetWorkItems(changedWorkItems);
            _logger.LogInformation("Fetched changed workitems, total count is {0}", workItems.Count());
            await FetchUpdatesAndSave(workItems);

            member.LastFetchDate = DateTime.UtcNow;
            await _repository.CreateOrUpdateAsync(member);
            _logger.LogInformation("Workitems fetch for '{0}' completed", member.Email);
        }

        private async Task<IEnumerable<int>> FetchRelatedWorkItems(TeamMember member, IEnumerable<VSTSProject> projects)
        {
            var date = member.LastFetchDate == DateTime.MinValue ? DateTime.UtcNow.AddYears(-10) : member.LastFetchDate;
            var queryText = string.Format(WorkItemsQuery, member.Email, date.ToString("MM/dd/yyyy"));
            var query = new ItemsQuery(queryText);

            var ids = new List<int>(1000);
            foreach (var project in projects)
            {
                _logger.LogInformation("Processing workitems for project '{0}'", project.Name);
                var wiqlEndPoint = VSTSApiUrl.Create(_config.Value.InstanceName)
                    .ForWIQL(project.Name)
                    .Build();
                var response = await _client.ExecutePost<WorkItemsQueryResponse>(wiqlEndPoint, query);

                ids.AddRange(response.WorkItems.Select(r => r.Id));
            }
            if (member.RelatedWorkItemIds != null)
            {
                member.RelatedWorkItemIds = member.RelatedWorkItemIds.Union(ids).Distinct();
            }
            else
            {
                member.RelatedWorkItemIds = ids;
            }
            return ids;
        }

        private async Task<IEnumerable<VSTSWorkItem>> GetWorkItems(IEnumerable<int> changedWorkItems)
        {
            var count = (decimal)changedWorkItems.Count();
            const int maxIdsPerRequest = 200;
            const string fieldsString = "System.Id,System.WorkItemType,System.Title,System.AreaPath,System.ChangedDate,System.State,System.Reason," +
                "System.CreatedDate,Microsoft.VSTS.Common.ResolvedDate,Microsoft.VSTS.Common.ClosedDate,Microsoft.VSTS.Common.StateChangeDate";
            var iterations = Math.Ceiling(count / maxIdsPerRequest);
            var wis = new List<VSTSWorkItem>((int)count);
            _logger.LogInformation("Starting workitems fetch. Total count: {0}, iterations needed: {1}", count, iterations);
            for (int i = 0; i < iterations; i++)
            {
                _logger.LogInformation("Starting iteration {0}", i);
                var idsToQuery = string.Join(',', changedWorkItems.Skip(i * maxIdsPerRequest).Take(maxIdsPerRequest));
                var wiQuery = VSTSApiUrl.Create(_config.Value.InstanceName)
                    .ForWorkItemsBatch(idsToQuery)
                    .WithQueryParameter("fields", fieldsString)
                    .Build();

                var workItemsResponse = await _client.ExecuteGet<ValueResponse<VSTSWorkItem>>(wiQuery);
                wis.AddRange(workItemsResponse.Value);
            }

            return wis;
        }

        private async Task FetchUpdatesAndSave(IEnumerable<VSTSWorkItem> workItems)
        {
            _logger.LogInformation("Starting to fetch updates");
            foreach (var wi in workItems)
            {
                var url = VSTSApiUrl.Create(_config.Value.InstanceName)
                    .ForWorkItems(wi.WorkItemId)
                    .WithSection("updates")
                    .Build();

                var updates = await _client.ExecuteGet<ValueResponse<WorkItemUpdate>>(url);
                wi.Updates = updates.Value;
                await _repository.CreateOrUpdateAsync(wi, i => i.WorkItemId == wi.WorkItemId);
            }

            _logger.LogInformation("Finished fetching updates");
        }

        private IEnumerable<TeamMember> GetMembers()
        {
            if (SpecificUser != null)
                return new[] { SpecificUser };

            return _repository.GetAll<TeamMember>();
        }

        public TeamMember SpecificUser { get; set; }
    }

    public struct ItemsQuery
    {
        public ItemsQuery(string query)
        {
            Query = query;
        }

        public string Query { get; set; }
    }

    public class WorkItemsQueryResponse
    {
        public WorkItemLink[] WorkItems { get; set; }

        public struct WorkItemLink
        {
            public int Id { get; set; }

            public string Url { get; set; }
        }
    }

    public class ValueResponse<T> where T : class, new()
    {
        public T[] Value { get; set; }
    }
}
