using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using Ether.Vsts.Types;
using Microsoft.Extensions.Logging;
using VSTS.Net.Models.Request;

namespace Ether.Vsts.Handlers.Queries
{
    public class FetchPullRequestsForRepositoryHandler : IQueryHandler<FetchPullRequestsForRepository, FetchPullRequestsResult>
    {
        private const string UserCommentType = "text";

        private readonly IVstsClientFactory _clientFactory;
        private readonly IRepository _repository;
        private readonly ILogger<FetchPullRequestsForRepositoryHandler> _logger;

        public FetchPullRequestsForRepositoryHandler(IVstsClientFactory clientFactory, IRepository repository, ILogger<FetchPullRequestsForRepositoryHandler> logger)
        {
            _clientFactory = clientFactory;
            _repository = repository;
            _logger = logger;
        }

        public async Task<FetchPullRequestsResult> Handle(FetchPullRequestsForRepository query)
        {
            if (query.Repository == null)
            {
                throw new ArgumentNullException(nameof(query.Repository));
            }

            var info = query.Repository;
            if (string.IsNullOrEmpty(info.Name) || info.Members == null || !info.Members.Any() || info.Project == null || string.IsNullOrEmpty(info.Project.Name))
            {
                return new FetchPullRequestsResult { PullRequests = Enumerable.Empty<PullRequestViewModel>(), Details = Enumerable.Empty<PullRequestJobDetails.PullRequestDetail>() };
            }

            var token = info.Project.Identity != null && !string.IsNullOrEmpty(info.Project.Identity.Token) ? info.Project.Identity.Token : null;
            var client = await _clientFactory.GetPullRequestsClient(token);
            var pullRequests = new List<PullRequestViewModel>();
            var details = new List<PullRequestJobDetails.PullRequestDetail>();
            var errors = new List<PullRequestJobDetails.ErrorDetail>();
            var timeLogs = new List<PullRequestJobDetails.TimeEntry>();
            foreach (var member in info.Members)
            {
                var startTime = DateTime.UtcNow;
                try
                {
                    var prs = await client.GetPullRequestsAsync(info.Project.Name, info.Name, new PullRequestQuery
                    {
                        CreatorId = member.Id,
                        CreatedAfter = member.LastPullRequestsFetchDate,
                        Status = "all"
                    });

                    foreach (var pr in prs)
                    {
                        details.Add(new PullRequestJobDetails.PullRequestDetail
                        {
                            Member = member.DisplayName,
                            Repository = info.Name,
                            PullRequestId = pr.PullRequestId,
                            PullRequestTitle = pr.Title,
                            PullRequestState = pr.Status
                        });

                        var iterations = await client.GetPullRequestIterationsAsync(info.Project.Name, info.Name, pr.PullRequestId);
                        var threads = await client.GetPullRequestThreadsAsync(info.Project.Name, info.Name, pr.PullRequestId);

                        pullRequests.Add(new PullRequestViewModel
                        {
                            PullRequestId = pr.PullRequestId,
                            Author = pr.CreatedBy.UniqueName,
                            AuthorId = pr.CreatedBy.Id,
                            Created = pr.CreationDate,
                            Title = pr.Title,
                            State = (ViewModels.Types.PullRequestState)Enum.Parse(typeof(ViewModels.Types.PullRequestState), pr.Status, true),
                            Completed = pr.ClosedDate,
                            Repository = info.Id,
                            Iterations = iterations.Count(),
                            Comments = threads.Sum(t => t.Comments.Count(c => !c.IsDeleted && string.Equals(c.CommentType, UserCommentType, StringComparison.OrdinalIgnoreCase)))
                        });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new PullRequestJobDetails.ErrorDetail { Repository = info.Name, Member = member.DisplayName, Error = ex.Message });
                }

                timeLogs.Add(new PullRequestJobDetails.MemberTimeEntry { Member = member.DisplayName, Repository = info.Name, Start = startTime, End = DateTime.UtcNow });
            }

            return new FetchPullRequestsResult { PullRequests = pullRequests, Details = details, Errors = errors, TimeLogs = timeLogs };
        }
    }
}
