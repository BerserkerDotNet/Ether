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
    public class RunPullRequestsJobCommandHandler : ICommandHandler<RunPullRequestsJob>
    {
        private readonly IJobRunner _jobRunner;
        private readonly IRepository _repository;

        public RunPullRequestsJobCommandHandler(IJobRunner jobRunner, IRepository repository)
        {
            _jobRunner = jobRunner;
            _repository = repository;
        }

        public async Task Handle(RunPullRequestsJob command)
        {
            if (command.IsReset)
            {
                await _repository.UpdateFieldValue<TeamMember, DateTime?>(m => command.Members.Contains(m.Id), m => m.LastPullRequestsFetchDate, null);
            }

            var parameters = new Dictionary<string, object> { { PullRequestsSyncJob.MembersParameterName, command.Members } };
            await _jobRunner.RunJob<PullRequestsSyncJob>(parameters);
        }
    }
}