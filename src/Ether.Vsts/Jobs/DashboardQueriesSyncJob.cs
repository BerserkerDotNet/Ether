using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels.Types;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Jobs
{
    public class DashboardQueriesSyncJob : IJob
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DashboardQueriesSyncJob> _logger;

        public DashboardQueriesSyncJob(IMediator mediator, ILogger<DashboardQueriesSyncJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task<JobDetails> Execute(IReadOnlyDictionary<string, object> parameters)
        {
            throw new KeyNotFoundException();
        }
    }
}
