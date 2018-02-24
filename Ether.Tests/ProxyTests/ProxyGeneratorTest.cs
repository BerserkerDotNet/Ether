using Castle.DynamicProxy;
using Ether.Core.Models.VSTS;
using Ether.Core.Proxy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Ether.Tests.ProxyTests
{
    [TestFixture]
    public class ProxyHookTest
    {
        private IEnumerable<Type> _supportedTypes = new[] { typeof(PullRequest) };

        [TestCase(typeof(PullRequest), true)]
        [TestCase(typeof(VSTSUser), false)]
        [TestCase(typeof(ProxyHookTest), false)]
        public void GeneratorShouldOnlyInterceptSupportedTypes(Type type, bool expectedResult)
        {
            var hook = new ProxyHook(_supportedTypes);
            var result = hook.ShouldInterceptMethod(type, null);

            result.Should().Be(expectedResult);
        }
    }

    [TestFixture]
    public class PullRequestInterceptorTest
    {
        private const string ExpectedOriginalResult = "Test";

        [Test]
        public void ShouldNotInterceptIfNotPullRequestType()
        {
            var testPoco = new DummyForTest();
            var generator = new ProxyGenerator();
            var proxy = generator.CreateClassProxyWithTarget(testPoco, new PullRequestsInterceptor());

            proxy.Comment.Should().Be(ExpectedOriginalResult);

        }

        public class DummyForTest
        {
            public virtual string Comment => ExpectedOriginalResult;
        }
    }
}
