using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using Ether.Core.Types;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Jobs
{
    public class PullRequestsFetchJob : IJob
    {
        private readonly IRepository _repository;
        private readonly IVstsClientRepository _vstsClient;
        private readonly ILogger<PullRequestsFetchJob> _logger;

        public PullRequestsFetchJob(IRepository repository, IVstsClientRepository vstsClient, ILogger<PullRequestsFetchJob> logger)
        {
            _repository = repository;
            _vstsClient = vstsClient;
            _logger = logger;
        }

        public void Execute()
        {
            try
            {
                FetchPullrequests();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error fetching pull requests");
            }
        }

        public TeamMember SpecificUser { get; set; }

        private void FetchPullrequests()
        {
            var isDisabled = _repository.GetFieldValue<Settings, bool>(_ => true, s => s.PullRequestsSettings.DisablePullRequestsJob);
            if (isDisabled)
            {
                _logger.LogWarning("Pull requests job is disabled in settings.");
                return;
            }

            var teamMembers = GetTeamMembers();
            var profiles = _repository.GetAll<Profile>();
            var repositories = _repository.GetAll<VSTSRepository>();
            var projects = _repository.GetAll<VSTSProject>();
            foreach (var teamMember in teamMembers)
            {
                var memberRepositoriesAndProjects = profiles.Where(p => p.Members.Contains(teamMember.Id))
                    .SelectMany(p => p.Repositories)
                    .Select(r =>
                    {
                        var repository = repositories.Single(ar => ar.Id == r);
                        var project = projects.Single(p => p.Id == repository.Project);
                        return (repository, project);
                    });

                var retrievedPullRequests = new List<PullRequest>();
                foreach (var repositoryAndProject in memberRepositoriesAndProjects)
                {
                    var query = PullRequestQuery.New(teamMember.LastPullRequestsFetchDate)
                        .WithFilter(IsPullRequestMatch)
                        .WithParameter("creatorId", teamMember.Id.ToString())
                        .WithParameter("status", "all");

                    var pullRequests = _vstsClient.GetPullRequests(repositoryAndProject.project.Name, repositoryAndProject.repository.Name, query)
                        .GetAwaiter()
                        .GetResult();
                    retrievedPullRequests.AddRange(pullRequests);
                }

                if (teamMember.PullRequests == null)
                    teamMember.PullRequests = Enumerable.Empty<PullRequest>();

                var newPullrequests = retrievedPullRequests.Except(teamMember.PullRequests);
                _logger.LogInformation("Found {newPullRequestsNumber} pullrequests for '{TeamMember}'", newPullrequests.Count(), teamMember.Email);
                teamMember.PullRequests = teamMember.PullRequests.Union(newPullrequests);
                teamMember.LastPullRequestsFetchDate = DateTime.UtcNow;

                _repository.CreateOrUpdateAsync(teamMember)
                    .GetAwaiter()
                    .GetResult();
            }
        }

        private bool IsPullRequestMatch(PullRequest pr)
        {
            return !"active".Equals(pr.Status, StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<TeamMember> GetTeamMembers()
        {
            if (SpecificUser != null)
            {
                return new[] { SpecificUser };
            }
            else
            {
                return _repository.GetAll<TeamMember>();
            }
        }
    }
}
