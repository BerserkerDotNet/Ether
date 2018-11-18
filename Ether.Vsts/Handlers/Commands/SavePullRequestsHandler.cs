using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Contracts.Types;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SavePullRequestsHandler : ICommandHandler<SavePullRequests>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public SavePullRequestsHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Handle(SavePullRequests command)
        {
            if (command.PullRequests == null)
            {
                throw new ArgumentNullException(nameof(command.PullRequests));
            }

            if (!command.PullRequests.Any())
            {
                return;
            }

            foreach (var pullRequest in command.PullRequests)
            {
                var pr = _mapper.Map<PullRequest>(pullRequest);
                await _repository.CreateOrUpdateIfAsync(p => p.PullRequestId == pr.PullRequestId, pr);
            }

            return;
        }
    }
}
