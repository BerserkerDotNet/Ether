using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ether.Tests.Handlers.Commands
{
    public abstract class BaseReportHandlerTests<THandler, TCommand> : BaseHandlerTest
        where THandler : ICommandHandler<TCommand, Guid>
        where TCommand : GenerateReportCommand, new()
    {
        protected const string DataSourceType = "FooSource";

        protected Mock<IIndex<string, IDataSource>> DataSourceProviderMock { get; set; }

        protected Mock<IDataSource> DataSourceMock { get; set; }

        protected RandomGenerator Generator { get; } = new RandomGenerator();

        protected THandler Handler { get; private set; }

        protected TCommand Command => new TCommand { Profile = Guid.NewGuid(), Start = DateTime.UtcNow, End = DateTime.UtcNow };

        protected abstract string ReportType { get; }

        protected abstract string ReportName { get; }

        protected abstract THandler InitializeHandler();

        protected sealed override void Initialize()
        {
            DataSourceMock = new Mock<IDataSource>(MockBehavior.Strict);
            var ds = DataSourceMock.Object;
            DataSourceProviderMock = new Mock<IIndex<string, IDataSource>>(MockBehavior.Strict);
            DataSourceProviderMock.Setup(p => p.TryGetValue(DataSourceType, out ds)).Returns(true);
            RepositoryMock.Setup(r => r.GetFieldValueAsync(It.IsAny<Expression<Func<Profile, bool>>>(), It.IsAny<Expression<Func<Profile, string>>>()))
                .ReturnsAsync(DataSourceType)
                .Verifiable();

            Handler = InitializeHandler();
        }

        protected void SetupGetProfile(ProfileViewModel profile)
        {
            DataSourceMock.Setup(d => d.GetProfile(It.IsAny<Guid>()))
                .ReturnsAsync(profile)
                .Verifiable();
        }

        protected void SetupGetTeamMember(IEnumerable<TeamMemberViewModel> members)
        {
            DataSourceMock.Setup(d => d.GetTeamMember(It.IsAny<Guid>()))
                .Returns<Guid>(id => Task.FromResult(members.Single(m => m.Id == id)))
                .Verifiable();
        }

        protected void SetupGetWorkitems(Guid memberId, IEnumerable<WorkItemViewModel> workitems)
        {
            DataSourceMock.Setup(d => d.GetWorkItemsFor(memberId))
                .ReturnsAsync(workitems)
                .Verifiable();
        }

        protected async Task InvokeAndVerify<TReport>(TCommand command, Action<TReport, Guid> verify)
            where TReport : ReportResult
        {
            TReport report = null;
            RepositoryMock.Setup(r => r.CreateAsync<ReportResult>(It.IsAny<TReport>()))
                .Callback<ReportResult>(r => report = (TReport)r)
                .ReturnsAsync(true);

            var reportId = await Handler.Handle(command);
            VerifyDefaultFields(report);
            verify(report, reportId);
        }

        protected void VerifyDefaultFields(ReportResult report)
        {
            report.Id.Should().NotBeEmpty();
            report.DateTaken.Should().BeCloseTo(DateTime.UtcNow);
            report.StartDate.Should().NotBe(default(DateTime));
            report.EndDate.Should().NotBe(default(DateTime));
            report.ProfileId.Should().NotBeEmpty();
            report.ProfileName.Should().NotBeNullOrEmpty();
            report.ReportType.Should().Be(ReportType);
            report.ReportName.Should().Be(ReportName);
        }

        protected ILogger<THandler> GetLoggerMock()
        {
            return Mock.Of<ILogger<THandler>>();
        }
    }
}
