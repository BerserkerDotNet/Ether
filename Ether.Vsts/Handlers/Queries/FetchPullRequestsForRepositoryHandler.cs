using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using Microsoft.Extensions.Logging;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Request;

namespace Ether.Vsts.Handlers.Queries
{
    public class FetchPullRequestsForRepositoryHandler : IQueryHandler<FetchPullRequestsForRepository, IEnumerable<VstsPullRequestViewModel>>
    {
        private readonly IVstsClientFactory _clientFactory;
        private readonly ILogger<FetchPullRequestsForRepositoryHandler> _logger;

        public FetchPullRequestsForRepositoryHandler(IVstsClientFactory clientFactory, ILogger<FetchPullRequestsForRepositoryHandler> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<VstsPullRequestViewModel>> Handle(FetchPullRequestsForRepository query)
        {
            if (query.Repository == null)
            {
                throw new ArgumentNullException(nameof(query.Repository));
            }

            var info = query.Repository;
            if (string.IsNullOrEmpty(info.Name) || info.Members == null || !info.Members.Any() || info.Project == null || string.IsNullOrEmpty(info.Project.Name))
            {
                return Enumerable.Empty<VstsPullRequestViewModel>();
            }

            var token = info.Project.Identity != null && !string.IsNullOrEmpty(info.Project.Identity.Token) ? info.Project.Identity.Token : null;
            var client = await _clientFactory.GetPullRequestsClient(token);
            var result = new List<VstsPullRequestViewModel>();
            foreach (var member in info.Members)
            {
                var prs = await client.GetPullRequestsAsync(info.Project.Name, info.Name, new PullRequestQuery
                {
                    CreatorId = member.Id,
                    Status = "all"
                });

                foreach (var pr in prs)
                {
                    var iterations = await client.GetPullRequestIterationsAsync(info.Project.Name, info.Name, pr.PullRequestId);
                    var threads = await client.GetPullRequestThreadsAsync(info.Project.Name, info.Name, pr.PullRequestId);

                    result.Add(new VstsPullRequestViewModel
                    {
                        PullRequestId = pr.PullRequestId,
                        Author = pr.CreatedBy.UniqueName,
                        Created = pr.CreationDate,
                        Title = pr.Title,
                        State = (VstsPullRequestState)Enum.Parse(typeof(VstsPullRequestState), pr.Status, true),
                        Completed = pr.ClosedDate,
                        Repository = pr.Repository.Id,
                        Iterations = iterations.Count(),
                        Comments = threads.Sum(t => t.Comments.Count())
                    });
                }
            }

            return result;
        }
    }
}
