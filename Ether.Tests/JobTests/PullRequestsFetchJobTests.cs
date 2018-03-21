using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using FluentAssertions;
using Ether.Jobs;
using Ether.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Ether.Core.Models.DTO;
using System.Linq.Expressions;
using System.Linq;
using Ether.Core.Types;
using Ether.Core.Models.VSTS;
using System.Threading.Tasks;

namespace Ether.Tests.JobTests
{
    [TestFixture]
    public class PullRequestsFetchJobTests
    {
        private PullRequestsFetchJob _job;
        private Mock<IRepository> _repository;
        private Mock<IVstsClientRepository> _vstsClient;

        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<IRepository>(MockBehavior.Strict);
            _vstsClient = new Mock<IVstsClientRepository>(MockBehavior.Strict);
            _job = new PullRequestsFetchJob(_repository.Object, _vstsClient.Object, Mock.Of<ILogger<PullRequestsFetchJob>>());
        }

        [Test]
        public void ShouldExitIfJobIsDisabled()
        {
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(true);
            _job.Execute();

            _repository.VerifyAll();
        }

        [Test]
        public void ShouldFetchPullRequestsForEveryTeamMember()
        {
            var teamMembers = new[] { new TeamMember { Id = Guid.NewGuid() }, new TeamMember { Id = Guid.NewGuid() } };
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1",  Project = project.Id } };
            var profile = new Profile { Id = Guid.NewGuid(), Members = teamMembers.Select(m => m.Id), Repositories = repositories.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            _repository.Setup(r => r.GetAll<TeamMember>()).Returns(teamMembers);
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositories);
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(Enumerable.Empty<PullRequest>()));

            _job.Execute();

            _repository.VerifyAll();
            _vstsClient.Verify(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()), Times.Exactly(2));
            _repository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(2));
        }

        [Test]
        public void ShouldFetchPullRequestsForEveryProfileAndRepository()
        {
            var teamMembers = new[] { new TeamMember { Id = Guid.NewGuid() }};
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositoriesForProfile1 = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1", Project = project.Id }, new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 2", Project = project.Id } };
            var repositoriesForProfile2 = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 3", Project = project.Id } };
            var profile1 = new Profile { Id = Guid.NewGuid(), Members = teamMembers.Select(m => m.Id), Repositories = repositoriesForProfile1.Select(r => r.Id) };
            var profile2 = new Profile { Id = Guid.NewGuid(), Members = teamMembers.Select(m => m.Id), Repositories = repositoriesForProfile2.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            _repository.Setup(r => r.GetAll<TeamMember>()).Returns(teamMembers);
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile1, profile2 });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositoriesForProfile1.Union(repositoriesForProfile2));
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(project.Name, It.Is<string>(rn => repositoriesForProfile1.Union(repositoriesForProfile2).Any(rp => rp.Name == rn)), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(Enumerable.Empty<PullRequest>()));

            _job.Execute();

            _repository.VerifyAll();
            _vstsClient.Verify(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()), Times.Exactly(3));
            _repository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(1));
        }

        [Test]
        public void ShouldFetchAllPullRequestsIfLastFetchDateIsNull()
        {
            var teamMembers = new[] { new TeamMember { Id = Guid.NewGuid() }, new TeamMember { Id = Guid.NewGuid() } };
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1", Project = project.Id } };
            var profile = new Profile { Id = Guid.NewGuid(), Members = teamMembers.Select(m => m.Id), Repositories = repositories.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            _repository.Setup(r => r.GetAll<TeamMember>()).Returns(teamMembers);
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositories);
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.Is<PullRequestQuery>(q => q.FromDate == DateTime.MinValue)))
                .Returns(Task.FromResult(Enumerable.Empty<PullRequest>()));

            _job.Execute();

            teamMembers[0].LastPullRequestsFetchDate.Should().BeCloseTo(DateTime.UtcNow);
            teamMembers[1].LastPullRequestsFetchDate.Should().BeCloseTo(DateTime.UtcNow);
            _repository.VerifyAll();
            _vstsClient.Verify(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()), Times.Exactly(2));
            _repository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(2));
        }

        [Test]
        public void ShouldFetchPullrequestsFromLastFetchDate()
        {
            var lastFetchDate = DateTime.UtcNow.AddDays(-2);
            var teamMembers = new[] { new TeamMember { Id = Guid.NewGuid(), LastPullRequestsFetchDate = lastFetchDate } };
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1", Project = project.Id } };
            var profile = new Profile { Id = Guid.NewGuid(), Members = teamMembers.Select(m => m.Id), Repositories = repositories.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            _repository.Setup(r => r.GetAll<TeamMember>()).Returns(teamMembers);
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositories);
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.Is<PullRequestQuery>(q => q.FromDate == lastFetchDate)))
                .Returns(Task.FromResult(Enumerable.Empty<PullRequest>()));

            _job.Execute();

            _repository.VerifyAll();
            _vstsClient.Verify(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()), Times.Exactly(1));
            _repository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(1));

        }

        [Test]
        public void ShouldNotFetchActivePullRequests()
        {
            var activePullRequest = new PullRequest { Status = "Active" };
            var completedPullRequest = new PullRequest { Status = "Completed" };
            var teamMembers = new[] { new TeamMember { Id = Guid.NewGuid() }};
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1", Project = project.Id } };
            var profile = new Profile { Id = Guid.NewGuid(), Members = teamMembers.Select(m => m.Id), Repositories = repositories.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            _repository.Setup(r => r.GetAll<TeamMember>()).Returns(teamMembers);
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositories);
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.Is<PullRequestQuery>(q => !q.Filter(activePullRequest) && q.Filter(completedPullRequest))))
                .Returns(Task.FromResult(Enumerable.Empty<PullRequest>()));

            _job.Execute();

            _repository.VerifyAll();
            _vstsClient.Verify(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()), Times.Exactly(1));
            _repository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(1));
        }

        [Test]
        public void ShouldNotAddDuplicates()
        {
            var pullRequests = new[] { new PullRequest { PullRequestId = 1 }, new PullRequest { PullRequestId = 2 }, new PullRequest { PullRequestId = 3 }, new PullRequest { PullRequestId = 4 } };
            var teamMember = new TeamMember { Id = Guid.NewGuid(), PullRequests = pullRequests.Skip(1).Take(2) };
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1", Project = project.Id } };
            var profile = new Profile { Id = Guid.NewGuid(), Members = new[] { teamMember.Id}, Repositories = repositories.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            _repository.Setup(r => r.GetAll<TeamMember>()).Returns(new[] { teamMember });
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositories);
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(pullRequests.AsEnumerable()));

            _job.Execute();

            teamMember.PullRequests.Should().HaveCount(4);
            teamMember.PullRequests.Should().BeEquivalentTo(pullRequests);
            _repository.VerifyAll();
            _vstsClient.Verify(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()), Times.Exactly(1));
            _repository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(1));
        }

        [Test]
        public void ShouldNotFailIfExceptionIsThrownForOneMemeber()
        {
            var teamMember = new TeamMember { Id = Guid.NewGuid() };
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1", Project = project.Id } };
            var profile = new Profile { Id = Guid.NewGuid(), Members = new[] { teamMember.Id }, Repositories = repositories.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            _repository.Setup(r => r.GetAll<TeamMember>()).Returns(new[] { teamMember });
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositories);
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Throws<Exception>();

            _job.Invoking(j => j.Execute()).Should().NotThrow();
        }

        [Test]
        public void ShouldOnlyFetchForSpecificUserIfSpecified()
        {
            var teamMembers = new[] { new TeamMember { Id = Guid.NewGuid() }, new TeamMember { Id = Guid.NewGuid() } };
            var project = new VSTSProject { Id = Guid.NewGuid(), Name = "Project 1" };
            var repositories = new[] { new VSTSRepository { Id = Guid.NewGuid(), Name = "Repository 1", Project = project.Id } };
            var profile = new Profile { Id = Guid.NewGuid(), Members = teamMembers.Select(m => m.Id), Repositories = repositories.Select(r => r.Id) };
            _repository.Setup(r => r.GetFieldValue(It.IsAny<Expression<Func<Settings, bool>>>(), It.IsAny<Expression<Func<Settings, bool>>>()))
                .Returns(false);
            
            _repository.Setup(r => r.GetAll<Profile>()).Returns(new[] { profile });
            _repository.Setup(r => r.GetAll<VSTSRepository>()).Returns(repositories);
            _repository.Setup(r => r.GetAll<VSTSProject>()).Returns(new[] { project });
            _repository.Setup(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()))
                .Returns(Task.FromResult(true));
            _vstsClient.Setup(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.Is<PullRequestQuery>(q => Guid.Parse(q.Parameters["creatorId"]) == teamMembers[1].Id)))
                .Returns(Task.FromResult(Enumerable.Empty<PullRequest>()));

            _job.SpecificUser = teamMembers[1];
            _job.Execute();

            _repository.VerifyAll();
            _repository.Verify(r => r.GetAll<TeamMember>(), Times.Never());
            _vstsClient.Verify(r => r.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()), Times.Exactly(1));
            _repository.Verify(r => r.CreateOrUpdateAsync(It.IsAny<TeamMember>()), Times.Exactly(1));
        }
    }
}
