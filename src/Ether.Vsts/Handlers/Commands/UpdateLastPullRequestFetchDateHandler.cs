using System;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Handlers.Commands
{
    public class UpdateLastPullRequestFetchDateHandler : ICommandHandler<UpdateLastPullRequestFetchDate>
    {
        private readonly IRepository _repository;
        private readonly ILogger<UpdateLastPullRequestFetchDateHandler> _logger;

        public UpdateLastPullRequestFetchDateHandler(IRepository repository, ILogger<UpdateLastPullRequestFetchDateHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(UpdateLastPullRequestFetchDate command)
        {
            var lastFetchDate = DateTime.UtcNow;
            var members = command.PullRequests.GroupBy(p => p.AuthorId).ToArray();
            foreach (var member in members)
            {
                try
                {
                    _logger.LogInformation("Updating last pull request fetch date for '{MemberName}' to '{LastFetchDate}'", member.First().Author, lastFetchDate);
                    await _repository.UpdateFieldValue<TeamMember, DateTime?>(m => m.Id == member.Key, m => m.LastPullRequestsFetchDate, lastFetchDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating last pull request fetch date for '{MemberName}'", member.First().Author);
                }
            }
        }
    }
}
