using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.Vsts.Handlers.Queries;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using Ether.Vsts.Types;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Common;
using VSTS.Net.Models.Identity;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.Request;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class FetchPullRequestsForRepositoryHandlerTests : BaseHandlerTest
    {
        private FetchPullRequestsForRepositoryHandler _handler;
        private Mock<IVstsClientFactory> _clientFactoryMock;
        private Mock<IVstsPullRequestsClient> _clientMock;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        [Test]
        public void ShouldThrowExceptionIfRepositoryInfoIsNull()
        {
            _handler.Awaiting(h => h.Handle(new FetchPullRequestsForRepository(null)))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(nameof(EmptyCollectionTestCases))]
        public async Task ShouldReturnEmptyCollectionIf(RepositoryInfo repository)
        {
            var result = await _handler.Handle(new FetchPullRequestsForRepository(repository));

            result.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldRequestPullRequestsForEveryMember()
        {
            const int expectedMembersCount = 5;
            var members = Builder<TeamMemberViewModel>.CreateListOfSize(expectedMembersCount)
                .Build();
            var pullRequests = members.ToDictionary(k => k.Id, v =>
            {
                var index = members.IndexOf(v);
                return Builder<PullRequest>.CreateListOfSize(index + 1)
                .All()
                .With(p => p.Status, "active")
                .With(p => p.CreatedBy, Builder<IdentityReference>.CreateNew().Build())
                .With(p => p.Repository, Builder<Repository>.CreateNew().Build())
                .Build()
                .AsEnumerable();
            });
            var info = Builder<RepositoryInfo>.CreateNew()
                .With(i => i.Members = members)
                .With(i => i.Project, Builder<ProjectInfo>.CreateNew().Build())
                .Build();

            SetupClient();
            SetupPullRequests(info.Project.Name, info.Name, q => VerifyPullRequestQuery(q, members), q => pullRequests[q.CreatorId.Value]);
            SetupPullRequestIterationsAndComments(info.Project.Name, info.Name, 5, 10);

            var result = await _handler.Handle(new FetchPullRequestsForRepository(info));

            result.Should().HaveCount(15);

            _clientFactoryMock.VerifyAll();
            _clientMock.VerifyAll();
        }

        [Test]
        public async Task ShouldOnlyCountNotDeletedUserComments()
        {
            var members = Builder<TeamMemberViewModel>.CreateListOfSize(1).Build();
            var pullRequests = Builder<PullRequest>.CreateListOfSize(1)
                 .All()
                .With(p => p.Status, "active")
                .With(p => p.CreatedBy, Builder<IdentityReference>.CreateNew().Build())
                .With(p => p.Repository, Builder<Repository>.CreateNew().Build())
                .Build();

            var info = Builder<RepositoryInfo>.CreateNew()
                .With(i => i.Members = members)
                .With(i => i.Project, Builder<ProjectInfo>.CreateNew().Build())
                .Build();

            var threads = Builder<PullRequestThread>.CreateListOfSize(7)
                .TheFirst(1)
                .With(t => t.Comments, new[] { new PullRequestComment { CommentType = "system" } })
                .TheNext(1)
                .With(t => t.Comments, new[] { new PullRequestComment { CommentType = "text" } })
                .TheNext(1)
                .With(t => t.Comments, new[] { new PullRequestComment { CommentType = "text", IsDeleted = true } })
                .TheNext(1)
                .With(t => t.Comments, new[] { new PullRequestComment { CommentType = "text" } })
                .TheNext(1)
                .With(t => t.Comments, new[] { new PullRequestComment { CommentType = "system", IsDeleted = true } })
                .TheNext(1)
                .With(t => t.Comments, new[] { new PullRequestComment { CommentType = "system" } })
                .TheNext(1)
                .With(t => t.Comments, new[] { new PullRequestComment { CommentType = "bla" } })
                .Build();

            SetupClient();
            _clientMock.Setup(c => c.GetPullRequestsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>(), default(CancellationToken)))
                .ReturnsAsync(pullRequests)
                .Verifiable();
            _clientMock.Setup(c => c.GetPullRequestIterationsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), default(CancellationToken)))
                .ReturnsAsync(Enumerable.Empty<PullRequestIteration>())
                .Verifiable();
            _clientMock.Setup(c => c.GetPullRequestThreadsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), default(CancellationToken)))
                .ReturnsAsync(threads)
                .Verifiable();

            var result = await _handler.Handle(new FetchPullRequestsForRepository(info));

            // Only two comments are not deleted and of type 'text'
            result.Should().HaveCount(1);
            result.First().Comments.Should().Be(2);
        }

        [Test]
        public async Task ShouldUseProjectIdentity()
        {
            const int expectedMembersCount = 5;
            var members = Builder<TeamMemberViewModel>.CreateListOfSize(expectedMembersCount)
                .Build();
            var project = Builder<ProjectInfo>.CreateNew()
                .With(p => p.Identity, Builder<IdentityViewModel>.CreateNew().Build())
                .Build();
            var info = Builder<RepositoryInfo>.CreateNew()
                .With(i => i.Members = members)
                .With(i => i.Project, project)
                .Build();

            SetupClient(project.Identity.Token);
            SetupPullRequests(info.Project.Name, info.Name);
            SetupPullRequestIterationsAndComments(info.Project.Name, info.Name);

            var result = await _handler.Handle(new FetchPullRequestsForRepository(info));

            result.Should().HaveCount(0);

            _clientFactoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _clientFactoryMock = new Mock<IVstsClientFactory>(MockBehavior.Strict);
            _clientMock = new Mock<IVstsPullRequestsClient>(MockBehavior.Strict);
            _handler = new FetchPullRequestsForRepositoryHandler(_clientFactoryMock.Object, Mock.Of<ILogger<FetchPullRequestsForRepositoryHandler>>());
        }

        private static IEnumerable EmptyCollectionTestCases()
        {
            yield return new TestCaseData(new RepositoryInfo { Name = null })
                .SetName($"ShouldReturnEmptyCollectionIfRepositoryNameIsNull");
            yield return new TestCaseData(new RepositoryInfo { Name = string.Empty })
                .SetName($"ShouldReturnEmptyCollectionIfRepositoryNameIsEmpty");
            yield return new TestCaseData(new RepositoryInfo { Name = "Foo", Members = null })
                .SetName($"ShouldReturnEmptyCollectionIfMembersCollectionIsNull");
            yield return new TestCaseData(new RepositoryInfo { Name = "Foo", Members = Enumerable.Empty<TeamMemberViewModel>() })
                .SetName($"ShouldReturnEmptyCollectionIfMembersCollectionIsEmpty");
            yield return new TestCaseData(new RepositoryInfo { Name = "Foo", Members = new[] { new TeamMemberViewModel() }, Project = null })
                .SetName($"ShouldReturnEmptyCollectionIfProjectInfoIsNull");
            yield return new TestCaseData(new RepositoryInfo { Name = "Foo", Members = new[] { new TeamMemberViewModel() }, Project = new ProjectInfo { Name = null } })
                .SetName($"ShouldReturnEmptyCollectionIfProjectNameIsNull");
            yield return new TestCaseData(new RepositoryInfo { Name = "Foo", Members = new[] { new TeamMemberViewModel() }, Project = new ProjectInfo { Name = string.Empty } })
                .SetName($"ShouldReturnEmptyCollectionIfProjectNameIsEmpty");
        }

        private bool VerifyPullRequestQuery(PullRequestQuery query, IList<TeamMemberViewModel> members)
        {
            return members.Any(m => m.Id == query.CreatorId) && string.Equals(query.Status, "all");
        }

        private void SetupClient(string token = null)
        {
            _clientFactoryMock.Setup(c => c.GetPullRequestsClient(token))
                .ReturnsAsync(_clientMock.Object)
                .Verifiable();
        }

        private void SetupPullRequests(string project, string repository, Func<PullRequestQuery, bool> queryPredicate = null, Func<PullRequestQuery, IEnumerable<PullRequest>> pullRequests = null)
        {
            if (queryPredicate == null)
            {
                queryPredicate = _ => true;
            }

            if (pullRequests == null)
            {
                pullRequests = _ => Enumerable.Empty<PullRequest>();
            }

            _clientMock.Setup(c => c.GetPullRequestsAsync(project, repository, It.Is<PullRequestQuery>(q => queryPredicate(q)), default(CancellationToken)))
                .Returns<string, string, PullRequestQuery, CancellationToken>((p, r, q, c) => Task.FromResult(pullRequests(q)))
                .Verifiable();
        }

        private void SetupPullRequestIterationsAndComments(string project, string repository, int iterationsCount = 0, int commentsCount = 0)
        {
            var iterations = iterationsCount == 0 ? Enumerable.Empty<PullRequestIteration>() : Builder<PullRequestIteration>.CreateListOfSize(iterationsCount).Build();
            _clientMock.Setup(c => c.GetPullRequestIterationsAsync(project, repository, It.IsAny<int>(), default(CancellationToken)))
                .ReturnsAsync(iterations)
                .Verifiable();

            var commentsPerThread = commentsCount / 2;
            var extraComments = commentsCount % 2;

            var threads = commentsCount == 0 ? Enumerable.Empty<PullRequestThread>() : Builder<PullRequestThread>.CreateListOfSize(2)
                .TheFirst(1)
                .With(t => t.Comments, Builder<PullRequestComment>.CreateListOfSize(commentsPerThread).Build())
                .TheNext(1)
                .With(t => t.Comments, Builder<PullRequestComment>.CreateListOfSize(commentsPerThread + extraComments).Build())
                .Build();

            _clientMock.Setup(c => c.GetPullRequestThreadsAsync(project, repository, It.IsAny<int>(), default(CancellationToken)))
                .ReturnsAsync(threads)
                .Verifiable();
        }
    }
}
