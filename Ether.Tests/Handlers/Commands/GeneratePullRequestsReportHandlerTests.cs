using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Core.Types;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Handlers.Commands;
using Ether.Tests.Extensions;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Dates;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class GeneratePullRequestsReportHandlerTests : BaseReportHandlerTests<GeneratePullRequestsReportHandler, GeneratePullRequestsReport>
    {
        protected override string ReportType => Core.Types.Constants.PullRequestsReportType;

        protected override string ReportName => Core.Types.Constants.PullRequestsReportName;

        [Test]
        public void ShouldThrowExceptionIfNotSupportedDataSourceType([Values(null, "", "Bla")]string dataSourceType)
        {
            RepositoryMock.Setup(r => r.GetFieldValueAsync(It.IsAny<Expression<Func<Profile, bool>>>(), It.IsAny<Expression<Func<Profile, string>>>()))
                .ReturnsAsync(dataSourceType)
                .Verifiable();

            IDataSource ds;
            DataSourceProviderMock.Setup(p => p.TryGetValue(dataSourceType, out ds)).Returns(false);
            Handler.Awaiting(h => h.Handle(new GeneratePullRequestsReport()))
                .Should().Throw<ArgumentException>();

            RepositoryMock.Verify();
        }

        [Test]
        public void ShouldThrowExceptionIfProfileNotFound()
        {
            SetupGetProfile(null);

            Handler.Awaiting(h => h.Handle(Command))
                .Should().Throw<ArgumentException>();

            DataSourceMock.Verify();
        }

        [Test]
        public async Task ShouldReturnEmptyReportForEachMemberIfNoPullRequests()
        {
            const int expectedTeamMembersCount = 6;
            var members = Builder<TeamMemberViewModel>.CreateListOfSize(expectedTeamMembersCount)
                .Build();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members = members.Select(m => m.Id).ToArray())
                .With(p => p.Repositories = new Guid[0])
                .Build();
            SetupGetProfile(profile);
            SetupGetTeamMember(members);
            SetupGetPullRequests(Enumerable.Empty<PullRequestViewModel>());

            await InvokeAndVerify<PullRequestsReport>(Command, (report, reportId) =>
            {
                report.Should().NotBeNull();
                report.Id.Should().Be(reportId);
                report.IndividualReports.Should().HaveCount(expectedTeamMembersCount);
                report.IndividualReports.Should().OnlyContain(r => r.IsEmpty);
            });

            DataSourceMock.VerifyAll();
            RepositoryMock.Verify();
        }

        [Test]
        public async Task ShouldReturnEmptyReportIfNoMembers()
        {
            var members = Enumerable.Empty<TeamMemberViewModel>();
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members = new Guid[0])
                .With(p => p.Repositories = new Guid[0])
                .Build();
            SetupGetProfile(profile);
            SetupGetTeamMember(members);
            SetupGetPullRequests(Enumerable.Empty<PullRequestViewModel>());

            await InvokeAndVerify<PullRequestsReport>(Command, (report, reportId) =>
            {
                report.Should().NotBeNull();
                report.Id.Should().Be(reportId);
                report.IndividualReports.Should().BeEmpty();
            });

            RepositoryMock.Verify();
        }

        [Test]
        public async Task ShouldCorrectlyCalculateMetricsForEachMember()
        {
            const int expectedTeamMembersCount = 2;
            var start = DateTime.Now.AddDays(-7);
            var end = DateTime.Now;
            var members = Builder<TeamMemberViewModel>.CreateListOfSize(4)
                .Build();
            var memberIds = members.Take(2).Select(m => m.Id).ToArray();
            var additionalMemberIds = members.TakeLast(2).Select(m => m.Id).ToArray();
            var repositories = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members = memberIds)
                .With(p => p.Repositories = repositories)
                .Build();

            var pullRequests = GetPullRequests(start, end, memberIds, repositories)
                .Union(GetPullRequests(start, end, memberIds, new[] { Guid.NewGuid(), Guid.NewGuid() }))
                .Union(GetPullRequests(start, end, additionalMemberIds, repositories))
                .ToArray();

            SetupGetProfile(profile);
            SetupGetTeamMember(members);
            SetupGetPullRequests(pullRequests);

            var command = new GeneratePullRequestsReport { Profile = Guid.NewGuid(), Start = start, End = end };
            await InvokeAndVerify<PullRequestsReport>(command, (report, reportId) =>
            {
                report.Should().NotBeNull();
                report.IndividualReports.Should().HaveCount(expectedTeamMembersCount);
                var firstMemberReport = report.IndividualReports.First();
                var secondMemberReport = report.IndividualReports.Last();

                firstMemberReport.Created.Should().Be(7);
                firstMemberReport.Active.Should().Be(4);
                firstMemberReport.Completed.Should().Be(3);
                firstMemberReport.Abandoned.Should().Be(2);
                firstMemberReport.TotalIterations.Should().Be(9);
                firstMemberReport.TotalComments.Should().Be(18);

                secondMemberReport.Created.Should().Be(7);
                secondMemberReport.Active.Should().Be(5);
                secondMemberReport.Completed.Should().Be(6);
                secondMemberReport.Abandoned.Should().Be(1);
                secondMemberReport.TotalIterations.Should().Be(12);
                secondMemberReport.TotalComments.Should().Be(24);
            });

            DataSourceMock.VerifyAll();
            RepositoryMock.Verify();
        }

        [Test]
        public async Task ShouldHandlePullRequestCompletedInFuture()
        {
            var start = DateTime.Now.AddDays(-11);
            var end = DateTime.Now;
            var member = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var memberIds = new[] { member.Id };
            var repositories = new[] { Guid.NewGuid() };
            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members = new[] { member.Id })
                .With(p => p.Repositories = repositories)
                .Build();

            var pullRequest = GetCompletedPullRequest(member.Id, repositories[0], end.AddHours(5), end.AddHours(5), createdInThePast: true);

            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { member });
            SetupGetPullRequests(new[] { pullRequest });

            var command = new GeneratePullRequestsReport { Profile = Guid.NewGuid(), Start = start, End = end };
            await InvokeAndVerify<PullRequestsReport>(command, (report, reportId) =>
            {
                report.Should().NotBeNull();
                var firstMemberReport = report.IndividualReports.First();

                firstMemberReport.Created.Should().Be(1);
                firstMemberReport.Active.Should().Be(0);
                firstMemberReport.Completed.Should().Be(0);
                firstMemberReport.Abandoned.Should().Be(0);
                firstMemberReport.TotalIterations.Should().Be(1);
                firstMemberReport.TotalComments.Should().Be(2);
                firstMemberReport.AveragePRLifespan.Should().Be(TimeSpan.Zero);
            });

            DataSourceMock.VerifyAll();
            RepositoryMock.Verify();
        }

        protected override GeneratePullRequestsReportHandler InitializeHandler()
        {
            return new GeneratePullRequestsReportHandler(DataSourceProviderMock.Object, RepositoryMock.Object, Mock.Of<ILogger<GeneratePullRequestsReportHandler>>());
        }

        private void SetupGetPullRequests(IEnumerable<PullRequestViewModel> allPullRequests)
        {
            DataSourceMock.Setup(d => d.GetPullRequests(It.IsAny<Expression<Func<PullRequestViewModel, bool>>>()))
                .Returns<Expression<Func<PullRequestViewModel, bool>>>(e => Task.FromResult(allPullRequests.Where(e.Compile())));
        }

        /*private async Task InvokeAndVerify<PullRequestsReport>(GeneratePullRequestsReport command, Action<PullRequestsReport, Guid> verify)
        {
            PullRequestsReport report = null;
            RepositoryMock.Setup(r => r.CreateAsync<ReportResult>(It.IsAny<PullRequestsReport>()))
                .Callback<ReportResult>(r => report = (PullRequestsReport)r)
                .ReturnsAsync(true);

            var reportId = await Handler.Handle(command);

            verify(report, reportId);
        }*/

        private IEnumerable<PullRequestViewModel> GetPullRequests(DateTime start, DateTime end, Guid[] members, Guid[] repositories)
        {
            // 3 Active PRs in range for 1st member
            yield return GetActivePullRequest(members[0], repositories[0], start, end);
            yield return GetActivePullRequest(members[0], repositories[1], start, end);
            yield return GetActivePullRequest(members[0], repositories[0], start, end);

            // 2 Completed PRs in range for 1st member
            yield return GetCompletedPullRequest(members[0], repositories[0], start, end);
            yield return GetCompletedPullRequest(members[0], repositories[1], start, end);

            // 2 Abandoned PRs in range for 1st member
            yield return GetCompletedPullRequest(members[0], repositories[0], start, end, state: PullRequestState.Abandoned);
            yield return GetCompletedPullRequest(members[0], repositories[1], start, end, state: PullRequestState.Abandoned);

            // 1 Completed PR created out of range for 1st member
            yield return GetCompletedPullRequest(members[0], repositories[0], start, end, createdInThePast: true);

            // 1 Active PR created out of range for 1st member
            yield return GetActivePullRequest(members[0], repositories[1], start, end, createdInThePast: true);

            // 2nd member

            // 2 Active PRs in range for 2nd member
            yield return GetActivePullRequest(members[1], repositories[0], start, end);
            yield return GetActivePullRequest(members[1], repositories[1], start, end);

            // 4 Completed PRs in range for 2nd member
            yield return GetCompletedPullRequest(members[1], repositories[0], start, end);
            yield return GetCompletedPullRequest(members[1], repositories[1], start, end);
            yield return GetCompletedPullRequest(members[1], repositories[0], start, end);
            yield return GetCompletedPullRequest(members[1], repositories[1], start, end);

            // 1 Abandoned PRs in range for 2nd member
            yield return GetCompletedPullRequest(members[1], repositories[0], start, end, state: PullRequestState.Abandoned);

            // 2 Completed PR created out of range for 1st member
            yield return GetCompletedPullRequest(members[1], repositories[0], start, end, createdInThePast: true);
            yield return GetCompletedPullRequest(members[1], repositories[1], start, end, createdInThePast: true);

            // 3 Active PR created out of range for 1st member
            yield return GetActivePullRequest(members[1], repositories[1], start, end, createdInThePast: true);
            yield return GetActivePullRequest(members[1], repositories[0], start, end, createdInThePast: true);
            yield return GetActivePullRequest(members[1], repositories[1], start, end, createdInThePast: true);
        }

        private PullRequestViewModel GetActivePullRequest(Guid author, Guid repository, DateTime start, DateTime end, bool createdInThePast = false)
        {
            var pr = GetPullRequest(author, repository, start, end, createdInThePast);
            pr.State = PullRequestState.Active;
            return pr;
        }

        private PullRequestViewModel GetCompletedPullRequest(Guid author, Guid repository, DateTime start, DateTime end, bool createdInThePast = false, PullRequestState state = PullRequestState.Completed)
        {
            var pr = GetPullRequest(author, repository, start, end, createdInThePast);
            pr.State = state;
            pr.Completed = Generator.Next(start, end);
            return pr;
        }

        private PullRequestViewModel GetPullRequest(Guid author, Guid repository, DateTime start, DateTime end, bool createdInThePast = false)
        {
            return new PullRequestViewModel
            {
                Iterations = 1,
                Comments = 2,
                State = PullRequestState.NotSet,
                Completed = null,
                AuthorId = author,
                Repository = repository,
                Created = !createdInThePast ? Generator.Next(start, end) : start.AddDays(-Generator.Next(1, 10))
            };
        }
    }
}
