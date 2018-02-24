using Castle.DynamicProxy;
using Ether.Core.Interfaces;
using Ether.Core.Models.VSTS;
using Ether.Core.Proxy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Tests.ProxyTests
{
    [TestFixture]
    public class PullRequestInterceptorTest
    {
        private const string ExpectedOriginalResult = "Test";

        Mock<IVstsClientRepository> _repositoryMock;
        ProxyGenerator _generator;
        PullRequestsInterceptor _interceptor;

        [SetUp]
        public void SetUp()
        {
            _generator = new ProxyGenerator();
            _repositoryMock = new Mock<IVstsClientRepository>();
            _interceptor = new PullRequestsInterceptor(_repositoryMock.Object, Mock.Of<ILogger<PullRequestsInterceptor>>());
        }

        [Test]
        public void ShouldNotInterceptIfNotPullRequestType()
        {
            var testPoco = new DummyForTest();
            var proxy = _generator.CreateClassProxyWithTarget(testPoco, _interceptor);

            proxy.Comment.Should().Be(ExpectedOriginalResult);
        }

        [Test]
        public void ShouldInterceptIterationsAndCacheFetchedData()
        {
            const string projectName = "Foo";
            const string repositoryName = "Bla";
            const int pullRequestId = 123456;

            IEnumerable<PullRequestIteration> expectedIterations = new[] { new PullRequestIteration(), new PullRequestIteration() };

            var pullRequest = new PullRequest
            {
                PullRequestId = pullRequestId,
                Repository = new RepositoryInfo { Name = repositoryName, Project = new ProjectInfo { Name = projectName } }
            };
            _repositoryMock.Setup(r => r.GetIterations(projectName, repositoryName, pullRequestId))
                .Returns(Task.FromResult(expectedIterations));

            var proxy = _generator.CreateClassProxyWithTarget(pullRequest, _interceptor);

            proxy.Iterations.Should().BeEquivalentTo(expectedIterations);
            proxy.IterationsCount.Should().Be(expectedIterations.Count());

            _repositoryMock.Verify(r => r.GetIterations(projectName, repositoryName, pullRequestId), Times.Once());
        }

        [Test]
        public void ShouldInterceptThreadsAndCacheFetchedData()
        {
            const string projectName = "Foo";
            const string repositoryName = "Bla";
            const int pullRequestId = 123457;

            IEnumerable<PullRequestThread> expectedThreads = new[] { new PullRequestThread(), new PullRequestThread() };

            var pullRequest = new PullRequest
            {
                PullRequestId = pullRequestId,
                Repository = new RepositoryInfo { Name = repositoryName, Project = new ProjectInfo { Name = projectName } }
            };
            _repositoryMock.Setup(r => r.GetThreads(projectName, repositoryName, pullRequestId))
                .Returns(Task.FromResult(expectedThreads));

            var proxy = _generator.CreateClassProxyWithTarget(pullRequest, _interceptor);

            proxy.Threads.Should().BeEquivalentTo(expectedThreads);
            proxy.CommentsCount.Should().Be(expectedThreads.Count());
            _repositoryMock.Verify(r => r.GetThreads(projectName, repositoryName, pullRequestId), Times.Once());
        }

        public class DummyForTest
        {
            public virtual string Comment => ExpectedOriginalResult;
        }
    }
}
