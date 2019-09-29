using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Handlers.Commands
{
    public class SavePullRequestsHandler : ICommandHandler<SavePullRequests>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SavePullRequestsHandler> _logger;

        public SavePullRequestsHandler(IRepository repository, IMapper mapper, ILogger<SavePullRequestsHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
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

            _logger.LogInformation("Saving pull requests. Totasl to save: {Count}", command.PullRequests.Count());
            foreach (var pullRequest in command.PullRequests)
            {
                var pr = _mapper.Map<PullRequest>(pullRequest);
                await _repository.CreateOrUpdateIfAsync(p => p.PullRequestId == pr.PullRequestId, pr);
            }

            _logger.LogInformation("Finished saving memeber pull request statistics.");
        }
    }
}
