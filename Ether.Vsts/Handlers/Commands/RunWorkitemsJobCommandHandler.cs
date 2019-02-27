using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Jobs;

namespace Ether.Vsts.Handlers.Commands
{
    public class RunWorkitemsJobCommandHandler : ICommandHandler<RunWorkitemsJob>
    {
        private readonly IJobRunner _jobRunner;
        private readonly IRepository _repository;

        public RunWorkitemsJobCommandHandler(IJobRunner jobRunner, IRepository repository)
        {
            _jobRunner = jobRunner;
            _repository = repository;
        }

        public async Task Handle(RunWorkitemsJob command)
        {
            if (command.IsReset)
            {
                await _repository.UpdateFieldValue<TeamMember, DateTime?>(m => command.Members.Contains(m.Id),  m => m.LastWorkitemsFetchDate, DateTime.UtcNow.AddYears(-10));
                await _repository.UpdateFieldValue<TeamMember, int[]>(m => command.Members.Contains(m.Id), m => m.RelatedWorkItems, new int[0]);
            }

            var parameters = new Dictionary<string, object> { { WorkItemsSyncJob.MembersParameterName, command.Members } };
            await _jobRunner.RunJob<WorkItemsSyncJob>(parameters);
        }
    }
}