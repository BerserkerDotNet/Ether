using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SavePullRequestsHandlerTests : BaseHandlerTest
    {
        private SavePullRequestsHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfPullRequestsAreNull()
        {
            _handler.Awaiting(h => h.Handle(new SavePullRequests(null)))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldNotDoAnythingIfPullRequestsEmpty()
        {
            await _handler.Handle(new SavePullRequests(Enumerable.Empty<PullRequestViewModel>()));

            RepositoryMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldSavePullRequests()
        {
            const int expectedCallsToDB = 5;
            var pullRequests = Builder<PullRequestViewModel>
                .CreateListOfSize(expectedCallsToDB)
                .Build();
            RepositoryMock.Setup(r => r.UpdateFieldValue<TeamMember, DateTime?>(It.IsAny<Expression<Func<TeamMember, bool>>>(), m => m.LastPullRequestsFetchDate, It.IsAny<DateTime?>()))
                .Returns(Task.CompletedTask);
            SetupCreateOrUpdateIf<PullRequest>(e => ValidateExpression(e), p => pullRequests.Any(pr => pr.PullRequestId == p.PullRequestId));

            await _handler.Handle(new SavePullRequests(pullRequests));

            RepositoryMock.Verify(r => r.CreateOrUpdateIfAsync(It.IsAny<Expression<Func<PullRequest, bool>>>(), It.IsAny<PullRequest>()), Times.Exactly(expectedCallsToDB));
        }

        protected override void Initialize()
        {
            _handler = new SavePullRequestsHandler(RepositoryMock.Object, Mapper, GetLoggerMock<SavePullRequestsHandler>());
        }

        private bool ValidateExpression(Expression e)
        {
            var lambdaExp = e as LambdaExpression;
            var binaryExp = lambdaExp.Body as BinaryExpression;
            var left = binaryExp.Left as MemberExpression;
            var right = binaryExp.Right as MemberExpression;

            return binaryExp.NodeType == ExpressionType.Equal &&
                left.Member.Name == nameof(PullRequest.PullRequestId) &&
                right.Member.Name == nameof(PullRequest.PullRequestId);
        }
    }
}
