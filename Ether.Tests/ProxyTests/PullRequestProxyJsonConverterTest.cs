using Castle.DynamicProxy;
using Ether.Core.Interfaces;
using Ether.Core.Models.VSTS;
using Ether.Core.Proxy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace Ether.Tests.ProxyTests
{
    [TestFixture]
    public class PullRequestProxyJsonConverterTest
    {
        PullRequestProxyJsonConverter _converter;
        ProxyGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new ProxyGenerator();
            _converter = new PullRequestProxyJsonConverter(_generator, Mock.Of<IVstsClientRepository>(), Mock.Of<ILogger<PullRequestsInterceptor>>());
        }

        [TestCase(typeof(PullRequest), true)]
        [TestCase(typeof(VSTSUser), false)]
        [TestCase(typeof(PullRequestProxyJsonConverter), false)]
        public void ShouldOnlyConvertPullRequestClass(Type type, bool expectedResult)
        {
            var result = _converter.CanConvert(type);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void ShouldBeSkippedWhenAttemptToWrite()
        {
            var pullRequest = new PullRequest
            {
                PullRequestId = 123456,
                CreatedBy = new VSTSUser { UniqueName = "John Foo" },
                Status = "Active"
            };

            _converter.Invoking(c => JsonConvert.SerializeObject(pullRequest, c)).Should()
                .NotThrow<NotSupportedException>();
            _converter.CanWrite.Should().BeFalse();
        }

        [Test]
        public void ShouldCreateProxyWithFieldsPopulated()
        {
            var pullRequest = new PullRequest
            {
                PullRequestId = 123456,
                CreatedBy = new VSTSUser { UniqueName = "John Foo" },
                Status = "Active"
            };

            var json = JsonConvert.SerializeObject(pullRequest);
            var proxy = JsonConvert.DeserializeObject<PullRequest>(json, _converter);

            proxy.Should().NotBeNull();
            proxy.GetType().FullName.Should().Be("Castle.Proxies.PullRequestProxy");
            proxy.PullRequestId.Should().Be(pullRequest.PullRequestId);
            proxy.CreatedBy.Should().Be(pullRequest.CreatedBy);
            proxy.Status.Should().Be(pullRequest.Status);
        }
    }
}
