using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;
using Ether.Core.Types.Commands;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Commands
{
    public class AbortActiveJobsHandler : ICommandHandler<AbortActiveJobs>
    {
        private readonly IRepository _repository;
        private readonly ILogger<AbortActiveJobsHandler> _logger;

        public AbortActiveJobsHandler(IRepository repository, ILogger<AbortActiveJobsHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(AbortActiveJobs command)
        {
            try
            {
                await _repository.UpdateFieldValue<JobLog, JobExecutionState>(j => j.Result == JobExecutionState.InProgress, j => j.Result, JobExecutionState.Aborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aborting running jobs");
            }
        }
    }
}
