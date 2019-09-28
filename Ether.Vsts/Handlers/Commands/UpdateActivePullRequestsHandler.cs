using System;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Handlers.Commands
{
    public class UpdateActivePullRequestsHandler : ICommandHandler<UpdateActivePullRequests>
    {
        private const string UserCommentType = "text";

        private readonly IVstsClientFactory _clientFactory;
        private readonly IRepository _repository;
        private readonly ILogger<UpdateActivePullRequestsHandler> _logger;

        public UpdateActivePullRequestsHandler(IVstsClientFactory clientFactory, IRepository repository, ILogger<UpdateActivePullRequestsHandler> logger)
        {
            _clientFactory = clientFactory;
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(UpdateActivePullRequests command)
        {
            _logger.LogInformation("Fetching active pull requests");
            var activePullRequests = await _repository.GetAsync<PullRequest>(p => p.State == Types.PullRequestState.Active);
            _logger.LogInformation("Found {NumberOfActivePrs} active pull requests", activePullRequests.Count());
            if (!activePullRequests.Any())
            {
                _logger.LogInformation("No active pull requests found.");
                return;
            }

            foreach (var pullRequest in activePullRequests)
            {
                try
                {
                    _logger.LogInformation("Updating pull request {PullRequestId}", pullRequest.PullRequestId);
                    await UpdatePullRequest(pullRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating pull request {PullRequestId}", pullRequest.PullRequestId);
                }
            }
        }

        private async Task UpdatePullRequest(PullRequest pullRequest)
        {
            // TODO: Need to either get info about the repository or iterate over different identities on 401

            /*var vstsRepository = await _repository.GetSingleAsync<Repository>(pullRequest.Repository);
            var vstsProject = await _repository.GetSingleAsync<Project>(vstsRepository.Project);
            var identity = vstsProject.Identity.HasValue ? (await _repository.GetSingleAsync<Identity>(vstsProject.Identity.Value)) : null;
            var token = identity is null ? null : identity.Token;*/
            var client = await _clientFactory.GetPullRequestsClient();

            var prInfo = await client.GetPullRequestAsync(pullRequest.PullRequestId);
            var iterations = await client.GetPullRequestIterationsAsync(prInfo.Repository.Project.Name, prInfo.Repository.Name, prInfo.PullRequestId);
            var threads = await client.GetPullRequestThreadsAsync(prInfo.Repository.Project.Name, prInfo.Repository.Name, prInfo.PullRequestId);

            var updatedPr = new PullRequest
            {
                Id = pullRequest.Id,
                LastSync = pullRequest.LastSync,
                PullRequestId = prInfo.PullRequestId,
                Author = prInfo.CreatedBy.UniqueName,
                AuthorId = prInfo.CreatedBy.Id,
                Created = prInfo.CreationDate,
                Title = prInfo.Title,
                State = (Types.PullRequestState)Enum.Parse(typeof(Types.PullRequestState), prInfo.Status, true),
                Completed = prInfo.ClosedDate.HasValue ? prInfo.ClosedDate.Value : DateTime.MinValue,
                Repository = prInfo.Repository.Id,
                Iterations = iterations.Count(),
                Comments = threads.Sum(t => t.Comments.Count(c => !c.IsDeleted && string.Equals(c.CommentType, UserCommentType, StringComparison.OrdinalIgnoreCase)))
            };

            await _repository.CreateOrUpdateAsync(updatedPr);
        }
    }
}
