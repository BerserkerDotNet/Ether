using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto.Reports;
using Ether.Core.Types;
using Ether.Core.Types.Handlers.Queries;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetReportByIdHandlerTests : BaseHandlerTest
    {
        private GetReportByIdHandler _handler;

        [Test]
        public async Task ShouldReturnNullIfDefaultGuid()
        {
            var result = await _handler.Handle(new GetReportById(Guid.Empty));

            result.Should().BeNull();
        }

        [Test]
        public async Task ShouldReturnNullIfUnsupportedReportType()
        {
            RepositoryMock.Setup(r => r.GetFieldValueAsync(It.IsAny<Expression<Func<ReportResult, bool>>>(), It.IsAny<Expression<Func<ReportResult, string>>>()))
                .ReturnsAsync("FakeReportType")
                .Verifiable();

            var result = await _handler.Handle(new GetReportById(Guid.NewGuid()));

            result.Should().BeNull();
            RepositoryMock.Verify();
        }

        [Test]
        public async Task ShouldReturnCorrectReportType()
        {
            const int expectedNumberOfReports = 5;
            var id = Guid.NewGuid();
            var individualReports = Builder<PullRequestsReport.IndividualPRReport>
                .CreateListOfSize(expectedNumberOfReports).Build();
            var report = new PullRequestsReport(expectedNumberOfReports);
            report.IndividualReports.AddRange(individualReports);

            RepositoryMock.Setup(r => r.GetFieldValueAsync(It.IsAny<Expression<Func<ReportResult, bool>>>(), It.IsAny<Expression<Func<ReportResult, string>>>()))
               .ReturnsAsync(Constants.PullRequestsReportType)
               .Verifiable();
            RepositoryMock.Setup(r => r.GetSingleAsync<PullRequestsReport>(id))
                .ReturnsAsync(report)
                .Verifiable();

            var result = await _handler.Handle(new GetReportById(id));

            result.Should().NotBeNull();
            result.Should().BeOfType<PullRequestReportViewModel>();
            result.As<PullRequestReportViewModel>().IndividualReports.Should().HaveCount(expectedNumberOfReports);
            RepositoryMock.Verify();
        }

        protected override void Initialize()
        {
            _handler = new GetReportByIdHandler(RepositoryMock.Object, Mapper);
        }
    }
}
