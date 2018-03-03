using Ether.Core.Configuration;
using Ether.Core.Data;
using Ether.Core.Interfaces;
using Ether.Core.Models.VSTS.Response;
using Ether.Core.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Collections.Generic;
using Ether.Core.Models.VSTS;

namespace Ether.Tests.VstsClientTests
{
    [TestFixture]
    public class VstsClientRepositoryTests
    {
        Mock<IVSTSClient> _clientMock;
        VstsClientRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var configMock = new Mock<IOptions<VSTSConfiguration>>();
            _clientMock = new Mock<IVSTSClient>();
            _repository = new VstsClientRepository(_clientMock.Object, configMock.Object, Mock.Of<ILogger<VstsClientRepository>>());

            configMock.SetupGet(c => c.Value).Returns(new VSTSConfiguration { InstanceName = "Foo" });
        }

        [Test]
        public async Task ShouldNotThrowIfErrorRetrievingPullRequests()
        {
            _clientMock.Setup(c => c.ExecuteGet<PullRequestsResponse>(It.IsAny<string>()))
                .Returns(Task.FromResult((PullRequestsResponse)null));

            var result = await _repository.GetPullRequests("Foo", "Bar", PullRequestQuery.New(DateTime.UtcNow));

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldNotThrowIfErrorRetrievingIterations()
        {
            _clientMock.Setup(c => c.ExecuteGet<ValueBasedResponse<PullRequestIteration>>(It.IsAny<string>()))
                .Returns(Task.FromResult((ValueBasedResponse<PullRequestIteration>)null));

            var result = await _repository.GetIterations("Foo", "Bar", 0);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldNotThrowIfErrorRetrievingComments()
        {
            _clientMock.Setup(c => c.ExecuteGet<ValueBasedResponse<PullRequestThread>>(It.IsAny<string>()))
                .Returns(Task.FromResult((ValueBasedResponse<PullRequestThread>)null));

            var result = await _repository.GetThreads("Foo", "Bar", 0);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
