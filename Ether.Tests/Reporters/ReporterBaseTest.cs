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
using Ether.Core.Models.DTO;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;

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
            Common.SetupConfiguration(_configurationMock, token: null, instance: null);
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
            Common.SetupConfiguration(_configurationMock, token, instance);

            var dummy = new DummyReporter(Mock.Of<IRepository>(), _configurationMock.Object, loggerMock.Object);
            dummy.Awaiting(async d => await d.ReportAsync(new ReportQuery()))
                .Should()
                .Throw<ArgumentException>("Configuration is missing.");

            loggerMock.VerifyAll();
        }

        [Test]
        public void ShouldThrowIfProfileDoesNotExist()
        {
            var loggerMock = GetLoggerMock<IReporter>();
            Common.SetupConfiguration(_configurationMock);
            var dummy = new DummyReporter(Mock.Of<IRepository>(), _configurationMock.Object, loggerMock.Object);
            dummy.Awaiting(async d => await d.ReportAsync(new ReportQuery()))
                .Should().
                Throw<ArgumentException>("Selected profile was not found.");

            loggerMock.VerifyAll();
        }

        [Test]
        public async Task ShouldInitializeInputObject()
        {
            var loggerMock = GetLoggerMock<IReporter>();
            Common.SetupConfiguration(_configurationMock);
            var repositoryMock = new Mock<IRepository>();
            var data = Common.SetupDataForBaseReporter(repositoryMock);

            var dummy = new DummyReporter(repositoryMock.Object, _configurationMock.Object, loggerMock.Object);
            var query = new ReportQuery
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(2),
                ProfileId = data.profile.Id
            };
            await dummy.ReportAsync(query);

            dummy.VerifyInputObject(query, data.profile, data.members.Take(2), data.repositories, data.projects[0]);
            repositoryMock.Verify(r => r.GetSingleAsync(It.Is<Expression<Func<Profile, bool>>>(e => VerifySelectProfileExpression(e))), Times.Once());
            repositoryMock.Verify();
        }

        [Test]
        public async Task ShouldPopulateStandardReportFieldsAndSave()
        {
            var loggerMock = GetLoggerMock<IReporter>();
            Common.SetupConfiguration(_configurationMock);
            var repositoryMock = new Mock<IRepository>();
            var data = Common.SetupDataForBaseReporter(repositoryMock);
            var dummy = new DummyReporter(repositoryMock.Object, _configurationMock.Object, loggerMock.Object);
            var query = new ReportQuery
            {
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(2),
                ProfileId = data.profile.Id
            };
            var report = await dummy.ReportAsync(query);

            report.Id.Should().NotBeEmpty();
            report.DateTaken.Should().BeCloseTo(DateTime.UtcNow);
            report.StartDate.Should().Be(query.StartDate);
            report.EndDate.Should().Be(query.EndDate);
            report.ProfileName.Should().Be(data.profile.Name);
            report.ReporterId.Should().Be(dummy.Id);
            report.ReportName.Should().Be(dummy.Name);
            repositoryMock.Verify(r => r.CreateAsync(It.IsAny<ReportResult>()), Times.Once());
        }

        private bool VerifySelectProfileExpression(Expression<Func<Profile, bool>> e)
        {
            var body = e.Body as BinaryExpression;
            if (body == null)
                return false;

            var memberLeft = body.Left as MemberExpression;
            var memberRight = body.Right as MemberExpression;
            if (memberLeft == null || memberRight == null)
                return false;

            return memberLeft.Member.Name == nameof(Profile.Id) && memberRight.Member.Name == nameof(ReportQuery.ProfileId);
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

        #region Dummy reporter
        private class DummyReporter : ReporterBase
        {
            public DummyReporter(IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<IReporter> logger) :
                base(repository, configuration, logger)
            {
            }

            public override string Name => "Dummy";

            public override Guid Id => Guid.Parse("{1bf664e6-0c75-4694-9be3-d67cbb3d6415}");

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

            internal void VerifyInputObject(ReportQuery query, Profile profile, IEnumerable<TeamMember> members, VSTSRepository[] repositories, VSTSProject project)
            {
                Input.Should().NotBeNull();
                Input.Query.Should().Be(query);
                Input.Profile.Should().Be(profile);
                Input.Repositories.Should().BeEquivalentTo(repositories);
                Input.Members.Should().BeEquivalentTo(members);
                Input.Projects.Should().HaveCount(1);
                Input.Projects.Should().HaveElementAt(0, project);
                Input.ActualEndDate.Should().BeCloseTo(DateTime.UtcNow.Date.AddDays(1).AddMilliseconds(-1));
            }
        } 
        #endregion
    }
}
