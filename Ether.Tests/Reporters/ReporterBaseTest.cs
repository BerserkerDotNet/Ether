using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Internal;
using Ether.Core.Configuration;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Core.Reporters;
using Ether.Core.Models.DTO.Reports;

namespace Ether.Tests.Reporters
{
    [TestFixture]
    public class ReporterBaseTest
    {
        private const int LoggerEventId = 0;
        Mock<IOptions<VSTSConfiguration>> _configurationMock = new Mock<IOptions<VSTSConfiguration>>();

        [Test]
        public void ShouldInitializeCommonServices()
        {
            SetupConfiguration();
            var dummy = new DummyReporter(Mock.Of<IRepository>(), _configurationMock.Object, Mock.Of<ILogger<IReporter>>());
            dummy.ValidateServicesAvailable();
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("", null)]
        [TestCase(null, "")]
        public void ShouldThrowIfBadConfiguration(string token, string instance)
        {
            var loggerMock = GetLoggerMock<IReporter>(LogLevel.Warning);
            SetupConfiguration(token, instance);

            var dummy = new DummyReporter(Mock.Of<IRepository>(), _configurationMock.Object, loggerMock.Object);
            dummy.Awaiting(async d => await d.ReportAsync(new ReportQuery()))
                .ShouldThrow<ArgumentException>("Configuration is missing.");

            loggerMock.VerifyAll();
        }

        [Test]
        public void ShouldThrowIfProfileDoesNotExist()
        {
            var loggerMock = GetLoggerMock<IReporter>();
            SetupConfiguration(token: "Foo", instance: "Bar");
            var dummy = new DummyReporter(Mock.Of<IRepository>(), _configurationMock.Object, loggerMock.Object);
            dummy.Awaiting(async d => await d.ReportAsync(new ReportQuery()))
                .ShouldThrow<ArgumentException>("Selected profile was not found.");

            loggerMock.VerifyAll();
        }

        private Mock<ILogger<T>> GetLoggerMock<T>(LogLevel level = LogLevel.None)
        {
            var loggerMock = new Mock<ILogger<T>>();
            if (level != LogLevel.None)
            {
                loggerMock.Setup(l => l.Log(level, LoggerEventId, It.IsAny<FormattedLogValues>(), null, It.IsAny<Func<object, Exception, string>>()))
                    .Verifiable();
            }

            return loggerMock;
        }

        private void SetupConfiguration(string token = null, string instance = null)
        {
            _configurationMock.SetupGet(c => c.Value)
                .Returns(new VSTSConfiguration { AccessToken = token, InstanceName = instance })
                .Verifiable();
        }

        #region Dummy reporter
        private class DummyReporter : ReporterBase
        {
            public DummyReporter(IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<IReporter> logger) :
                base(repository, configuration, logger)
            {
            }

            public override string Name => "Dummy";

            public override Guid Id => Guid.NewGuid();

            public override Type ReportType => typeof(object);

            protected override Task<ReportResult> ReportInternal()
            {
                return Task.FromResult(new ReportResult());
            }

            public void ValidateServicesAvailable()
            {
                _repository.Should().NotBeNull();
                _configuration.Should().NotBeNull();
                _logger.Should().NotBeNull();
            }
        } 
        #endregion
    }
}
