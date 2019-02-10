using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;

namespace Ether.Tests.Handlers.Commands
{
    public abstract class BaseReportHandlerTests<T, TCommand> : BaseHandlerTest
        where T : ICommandHandler<TCommand, Guid>
        where TCommand : ICommand<Guid>
    {
        protected const string DataSourceType = "FooSource";

        protected Mock<IIndex<string, IDataSource>> DataSourceProviderMock { get; set; }

        protected Mock<IDataSource> DataSourceMock { get; set; }

        protected RandomGenerator Generator { get; } = new RandomGenerator();

        protected T Handler { get; private set; }

        protected abstract T InitializeHandler();

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

        protected async Task InvokeAndVerify<TReport>(TCommand command, Action<TReport, Guid> verify)
            where TReport : ReportResult
        {
            TReport report = null;
            RepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TReport>()))
                .Callback<TReport>(r => report = r)
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
            report.ReportType.Should().NotBeNullOrEmpty();
            report.ReportName.Should().NotBeNullOrEmpty();
        }
    }
}
