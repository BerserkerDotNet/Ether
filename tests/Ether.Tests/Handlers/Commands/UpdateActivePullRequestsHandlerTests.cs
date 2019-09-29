using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using Ether.Vsts.Types;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class UpdateActivePullRequestsHandlerTests : BaseHandlerTest
    {
        private UpdateActivePullRequestsHandler _handler;

        [Test]
        public void ShouldNotThrowExceptionIfUpdateFails()
        {
            var activePullRequests = GeneratePullRequests(10);
            SetupMultiple(VerifyPredicateForActivePullRequests, activePullRequests);
            SetupCreateOrUpdate<PullRequest>();
            SetupPullRequestsFetch(activePullRequests.Skip(2).Take(8), activePullRequests.Take(2));
            SetupIterations();
            SetupThreads();

            _handler.Awaiting(h => h.Handle(new Vsts.Commands.UpdateActivePullRequests()))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldUpdateExistsingPullRequestsAndNotCreateDuplicates()
        {
            var activePullRequests = GeneratePullRequests(5);
            SetupMultiple(VerifyPredicateForActivePullRequests, activePullRequests);
            SetupCreateOrUpdate<PullRequest>();
            SetupPullRequestsFetch(activePullRequests, Enumerable.Empty<PullRequest>());
            SetupIterations();
            SetupThreads();

            await _handler.Handle(new Vsts.Commands.UpdateActivePullRequests());

            RepositoryMock.Verify(r => r.GetAsync<PullRequest>(p => p.State == PullRequestState.Active), Times.Once());
            VstsClientFactory.Verify();
            PullRequestsClient.Verify();
        }

        [Test]
        public async Task ShouldCorrectlyPoulateAllPullRequestInfo()
        {
            var expecetedPullRequest = new PullRequest
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow.AddDays(-20),
                Completed = DateTime.UtcNow.AddDays(-5),
                Author = "Author 1",
                AuthorId = Guid.NewGuid(),
                State = Vsts.Types.PullRequestState.Completed,
                Iterations = 4,
                Comments = 8,
                PullRequestId = 1246587,
                Title = "Test PR",
                Repository = Guid.NewGuid(),
                LastSync = DateTime.UtcNow
            };

            var activePullRequest = Builder<PullRequest>
                .CreateNew()
                .With(p => p.Id, expecetedPullRequest.Id)
                .With(p => p.PullRequestId, expecetedPullRequest.PullRequestId)
                .With(p => p.LastSync, expecetedPullRequest.LastSync)
                .With(p => p.Repository, expecetedPullRequest.Repository)
                .With(p => p.State, Vsts.Types.PullRequestState.Active)
                .Build();

            SetupMultiple(VerifyPredicateForActivePullRequests, new[] { activePullRequest });
            SetupCreateOrUpdate<PullRequest>();
            SetupIterations(expecetedPullRequest.Iterations);
            SetupThreads(expecetedPullRequest.Comments);

            PullRequestsClient.Setup(c => c.GetPullRequestAsync(expecetedPullRequest.PullRequestId, default))
                .ReturnsAsync(new VSTS.Net.Models.PullRequests.PullRequest
                {
                    PullRequestId = expecetedPullRequest.PullRequestId,
                    ClosedDate = expecetedPullRequest.Completed,
                    CreationDate = expecetedPullRequest.Created,
                    CreatedBy = new VSTS.Net.Models.Identity.IdentityReference { Id = expecetedPullRequest.AuthorId, UniqueName = expecetedPullRequest.Author },
                    Title = expecetedPullRequest.Title,
                    Status = "Completed",
                    Repository = new VSTS.Net.Models.Common.Repository { Id = Guid.NewGuid(), Project = new VSTS.Net.Models.Common.Project { } },
                });

            await _handler.Handle(new Vsts.Commands.UpdateActivePullRequests());

            RepositoryMock.Verify(r => r.CreateOrUpdateAsync(It.Is<PullRequest>(p => VerifyPullRequest(p, expecetedPullRequest))), Times.Once());
        }

        protected override void Initialize()
        {
            _handler = new UpdateActivePullRequestsHandler(VstsClientFactory.Object, RepositoryMock.Object, GetLoggerMock<UpdateActivePullRequestsHandler>());

            SetupSingle(Builder<Repository>.CreateNew().Build());
            SetupSingle(Builder<Project>.CreateNew().Build());
            SetupSingle(Builder<Identity>.CreateNew().Build());
        }

        private bool VerifyPullRequest(PullRequest actual, PullRequest expected)
        {
            actual.Author.Should().Be(expected.Author);
            actual.AuthorId.Should().Be(expected.AuthorId);
            actual.Comments.Should().Be(expected.Comments);
            actual.Completed.Should().Be(expected.Completed);
            actual.Created.Should().Be(expected.Created);
            actual.Id.Should().Be(expected.Id);
            actual.Iterations.Should().Be(expected.Iterations);
            actual.LastSync.Should().Be(expected.LastSync);
            actual.PullRequestId.Should().Be(expected.PullRequestId);
            actual.Repository.Should().Be(expected.Repository);
            actual.State.Should().Be(expected.State);
            actual.Title.Should().Be(expected.Title);
            return true;
        }

        private IEnumerable<PullRequest> GeneratePullRequests(int count)
        {
            return Builder<PullRequest>
                    .CreateListOfSize(count)
                    .All()
                    .With(p => p.State, Vsts.Types.PullRequestState.Active)
                    .Build();
        }

        private void SetupPullRequestsFetch(IEnumerable<PullRequest> prsToSucceed, IEnumerable<PullRequest> prsToFail)
        {
            PullRequestsClient.Setup(c => c.GetPullRequestAsync(It.Is<int>(id => prsToSucceed.Any(p => p.PullRequestId == id)), default))
                .Returns<int, CancellationToken>((id, _) =>
                {
                    var pr = prsToSucceed.First(p => p.PullRequestId == id);
                    return Task.FromResult(new VSTS.Net.Models.PullRequests.PullRequest
                    {
                        PullRequestId = id,
                        ClosedDate = DateTime.UtcNow,
                        CreationDate = DateTime.UtcNow,
                        CreatedBy = new VSTS.Net.Models.Identity.IdentityReference { Id = Guid.NewGuid(), UniqueName = "Fooo" },
                        Title = "Test PR",
                        Status = "Completed",
                        Repository = new VSTS.Net.Models.Common.Repository { Id = Guid.NewGuid(), Project = new VSTS.Net.Models.Common.Project { } },
                    });
                })
                .Verifiable();

            if (prsToFail.Any())
            {
                PullRequestsClient.Setup(c => c.GetPullRequestAsync(It.Is<int>(id => prsToFail.Any(p => p.PullRequestId == id)), default))
                    .ThrowsAsync(new Exception("Failed to fetch PR"))
                    .Verifiable();
            }
        }

        private void SetupIterations(int count = 1)
        {
            PullRequestsClient.Setup(c => c.GetPullRequestIterationsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), default))
                .ReturnsAsync(Builder<VSTS.Net.Models.PullRequests.PullRequestIteration>.CreateListOfSize(count).Build());
        }

        private void SetupThreads(int count = 0)
        {
            var comments = Builder<VSTS.Net.Models.PullRequests.PullRequestComment>.CreateListOfSize(1)
                .All()
                .With(c => c.CommentType, "text")
                .Build();
            var threads = count == 0 ? Enumerable.Empty<VSTS.Net.Models.PullRequests.PullRequestThread>() : Builder<VSTS.Net.Models.PullRequests.PullRequestThread>.CreateListOfSize(count)
                .All()
                .With(t => t.Comments, comments)
                .Build();

            PullRequestsClient.Setup(c => c.GetPullRequestThreadsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), default))
                .ReturnsAsync(threads);
        }

        private bool VerifyPredicateForActivePullRequests(Expression e)
        {
            var lExp = (LambdaExpression)e;
            var right = ((BinaryExpression)lExp.Body).Right;
            return Equals(((ConstantExpression)right).Value, (int)Vsts.Types.PullRequestState.Active);
        }
    }
}
