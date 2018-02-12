using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Core.Configuration;
using Ether.Core.Data;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Jobs;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ether.Api
{
    [Produces("application/json")]
    [Route("api/Query")]
    public class QueryController : Controller
    {
        private readonly IRepository _repository;
        private readonly IVSTSClient _client;
        private readonly IOptions<VSTSConfiguration> _config;
        private readonly ILogger<QueryController> _logger;

        public QueryController(IRepository repository, IVSTSClient client, IOptions<VSTSConfiguration> config, ILogger<QueryController> logger)
        {
            _repository = repository;
            _client = client;
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        [Route(nameof(GetDifference), Name = "GetDiff")]
        public async Task<IActionResult> GetDifference([FromQuery]QueryDiffViewModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var query = await _repository.GetSingleAsync<Query>(request.Id);
            if (query == null)
                return NotFound();

            var project = await _repository.GetSingleAsync<VSTSProject>(query.Project);
            var diff = await GetQueryHistory(query.QueryId, project.Name, request.From, request.To);
            return new ObjectResult(diff);
        }

        private async Task<QueryDiff> GetQueryHistory(Guid queryId, string project, DateTime from, DateTime to)
        {
            var queryUrl = VSTSApiUrl.Create(_config.Value.InstanceName)
                .ForQueries(project, queryId.ToString())
                .WithQueryParameter("$expand", "wiql")
                .Build();
            var wiqlUrl = VSTSApiUrl.Create(_config.Value.InstanceName)
                .ForWIQL(project)
                .Build();

            var query = await _client.ExecuteGet<VSTSQuery>(queryUrl);
            var days = (int)to.Date.Subtract(from.Date).TotalDays;
            var diff = new QueryDiff() { States = new List<QueryState>(days) };
            for (int i = 0; i < days; i++)
            {
                var date = from.Date.AddDays(i);
                var modifiedQuery = $"{query.Wiql} ASOF '{date.ToString("yyyy-MM-dd")}'";
                var result = await _client.ExecutePost<WorkItemsQueryResponse>(wiqlUrl, new ItemsQuery(modifiedQuery));
                var previous = diff.States.LastOrDefault();
                var current = new QueryState
                {
                    Date = date,
                    ItemsCount = result.Count
                };

                if (previous != null)
                {
                    current.Trend = current.ItemsCount - previous.ItemsCount;
                }
                diff.States.Add(current);
            }

            return diff;
        }
    }

    public class VSTSQuery
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Wiql { get; set; }
    }

    public class QueryState
    {
        public int ItemsCount { get; set; }
        public int Trend { get; set; }
        public DateTime Date { get; set; }
    }

    public class QueryDiff
    {
        public Guid QueryId { get; set; }
        public IList<QueryState> States { get; set; }
        public int TotalDiff => States == null || !States.Any() ? 0 : States.Last().ItemsCount - States.First().ItemsCount;
    }
}