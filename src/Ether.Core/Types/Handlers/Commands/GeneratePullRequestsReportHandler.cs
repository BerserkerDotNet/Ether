using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Commands
{
    public class GeneratePullRequestsReportHandler : GenerateReportHandlerBase<GeneratePullRequestsReport>
    {
        public GeneratePullRequestsReportHandler(IIndex<string, IDataSource> dataSources, IRepository repository, ILogger<GeneratePullRequestsReportHandler> logger)
            : base(dataSources, repository, logger)
        {
        }

        protected override async Task<ReportResult> GenerateAsync(GeneratePullRequestsReport command, IDataSource dataSource, ProfileViewModel profile)
        {
            var pullRequests = await dataSource.GetPullRequests(p =>
                (IsActivePullRequest(p) || IsCreatedIn(p, command.Start, command.End) || IsCompletedIn(p, command.Start, command.End)) &&
                profile.Repositories.Contains(p.Repository) &&
                profile.Members.Contains(p.AuthorId));
            var report = new PullRequestsReport(profile.Members.Count());

            foreach (var memberId in profile.Members)
            {
                var member = await dataSource.GetTeamMember(memberId);
                var memberPullRequests = pullRequests.Where(p => p.AuthorId == memberId).ToArray();
                if (!memberPullRequests.Any())
                {
                    report.AddEmpty(member.DisplayName);
                    continue;
                }

                var individualReport = new PullRequestsReport.IndividualPRReport();
                individualReport.TeamMember = member.DisplayName;
                individualReport.Created = memberPullRequests.Count(p => IsCreatedIn(p, command.Start, command.End));
                individualReport.Active = memberPullRequests.Count(IsActivePullRequest);
                individualReport.Completed = memberPullRequests.Count(p => IsCompletedPullRequest(p) && IsCompletedIn(p, command.Start, command.End));
                individualReport.Abandoned = memberPullRequests.Count(p => IsAbandonedPullRequest(p) && IsCompletedIn(p, command.Start, command.End));
                individualReport.TotalIterations = memberPullRequests.Sum(p => p.Iterations);
                individualReport.TotalComments = memberPullRequests.Sum(p => p.Comments);
                var averagePullRequestLifetime = memberPullRequests
                    .Where(p => IsCompletedPullRequest(p) && IsCompletedIn(p, command.Start, command.End))
                    .Sum(r => (r.Completed.Value - r.Created).TotalSeconds) / individualReport.Completed;
                averagePullRequestLifetime = double.IsNaN(averagePullRequestLifetime) ? 0 : averagePullRequestLifetime;
                individualReport.AveragePRLifespan = TimeSpan.FromSeconds(averagePullRequestLifetime);

                report.AddReport(individualReport);
            }

            return report;
        }

        protected override (string type, string name) GetReportInfo()
        {
            return (Constants.PullRequestsReportType, Constants.PullRequestsReportName);
        }

        private bool IsActivePullRequest(PullRequestViewModel pullRequest)
        {
            return pullRequest.State == PullRequestState.Active;
        }

        private bool IsCompletedPullRequest(PullRequestViewModel pullRequest)
        {
            return pullRequest.State == PullRequestState.Completed;
        }

        private bool IsAbandonedPullRequest(PullRequestViewModel pullRequest)
        {
            return pullRequest.State == PullRequestState.Abandoned;
        }

        private bool IsCompletedIn(PullRequestViewModel pullRequest, DateTime start, DateTime end)
        {
            if (!pullRequest.Completed.HasValue)
            {
                return false;
            }

            var completed = pullRequest.Completed.Value;
            return completed >= start && completed <= end;
        }

        private bool IsCreatedIn(PullRequestViewModel pullRequest, DateTime start, DateTime end)
        {
            return pullRequest.Created >= start && pullRequest.Created <= end;
        }
    }
}
