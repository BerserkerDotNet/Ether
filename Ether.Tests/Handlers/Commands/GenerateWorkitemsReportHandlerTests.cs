using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Handlers.Commands;
using Ether.ViewModels;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class GenerateWorkitemsReportHandlerTests
        : BaseReportHandlerTests<GenerateWorkitemsReportHandler, GenerateWorkItemsReport>
    {
        private Mock<IWorkItemClassificationContext> _classificationContextMock;

        [Test]
        public void ThrowsExceptionIfIncorrectOrNonExistingDatasourceType([Values(null, "", "Bla")] string dataSourceType)
        {
            RepositoryMock.Setup(r => r.GetFieldValueAsync(It.IsAny<Expression<Func<Profile, bool>>>(), It.IsAny<Expression<Func<Profile, string>>>()))
                .ReturnsAsync(dataSourceType)
                .Verifiable();

            IDataSource ds;
            DataSourceProviderMock.Setup(p => p.TryGetValue(dataSourceType, out ds)).Returns(false);
            Handler.Awaiting(h => h.Handle(new GenerateWorkItemsReport()))
                .Should().Throw<ArgumentException>();

            RepositoryMock.Verify();
        }

        [Test]
        public async Task ReturnsEmptyReportIfNoMembers()
        {
            var profile = Builder<ProfileViewModel>.CreateNew().Build();
            SetupGetProfile(profile);

            var command = new GenerateWorkItemsReport { Profile = profile.Id, Start = DateTime.UtcNow, End = DateTime.UtcNow };
            await InvokeAndVerify<WorkItemsReport>(command, (report, reportId) =>
            {
                report.Resolutions.Should().BeEmpty();
            });
        }

        [Test]
        public async Task ReturnsEmptyReportIfNoWorkitems()
        {
            var jim = Builder<TeamMemberViewModel>.CreateNew()
                .Build();
            var jess = Builder<TeamMemberViewModel>.CreateNew()
                .Build();

            var profile = Builder<ProfileViewModel>.CreateNew()
                .With(p => p.Members, new[] { jim.Id, jess.Id })
                .Build();
            SetupGetProfile(profile);
            SetupGetTeamMember(new[] { jim, jess });
            SetupGetWorkitems(jim.Id, Enumerable.Empty<WorkItemViewModel>());
            SetupGetWorkitems(jess.Id, Enumerable.Empty<WorkItemViewModel>());

            var command = new GenerateWorkItemsReport { Profile = profile.Id, Start = DateTime.UtcNow, End = DateTime.UtcNow };
            await InvokeAndVerify<WorkItemsReport>(command, (report, reportId) =>
            {
                report.Resolutions.Should().BeEmpty();
            });
        }

        [Test]
        public void ReturnsCorrectReportIfOneMember()
        {
        }

        [Test]
        public void ReturnsCorrectReportForMultipleMemebers()
        {
        }

        protected override GenerateWorkitemsReportHandler InitializeHandler()
        {
            _classificationContextMock = new Mock<IWorkItemClassificationContext>(MockBehavior.Strict);
            return new GenerateWorkitemsReportHandler(
                DataSourceProviderMock.Object,
                _classificationContextMock.Object,
                RepositoryMock.Object,
                GetLoggerMock());
        }
    }
}
