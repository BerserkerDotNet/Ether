using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto.Reports;
using Ether.Core.Types;
using Ether.Core.Types.Commands;
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
        private ReporterDescriptor[] _descriptors = new[]
        {
            new ReporterDescriptor(Constants.PullRequestsReportType, typeof(GeneratePullRequestsReport), typeof(PullRequestsReport), typeof(PullRequestReportViewModel), "PR report"),
            new ReporterDescriptor(Constants.ETAReportType, typeof(GenerateAggregatedWorkitemsETAReport), typeof(AggregatedWorkitemsETAReport), typeof(AggregatedWorkitemsETAReportViewModel), "ETA report")
        };

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
            var individualReports = Builder<AggregatedWorkitemsETAReport.IndividualETAReport>
                .CreateListOfSize(expectedNumberOfReports).Build();
            var report = new AggregatedWorkitemsETAReport(expectedNumberOfReports);
            report.IndividualReports.AddRange(individualReports);

            RepositoryMock.Setup(r => r.GetFieldValueAsync(It.IsAny<Expression<Func<ReportResult, bool>>>(), It.IsAny<Expression<Func<ReportResult, string>>>()))
               .ReturnsAsync(Constants.ETAReportType.ToLower())
               .Verifiable();
            RepositoryMock.Setup(r => r.GetSingleAsync(id, It.Is<Type>(t => t == typeof(AggregatedWorkitemsETAReport))))
                .ReturnsAsync(report)
                .Verifiable();

            var result = await _handler.Handle(new GetReportById(id));

            result.Should().NotBeNull();
            result.Should().BeOfType<AggregatedWorkitemsETAReportViewModel>();
            result.As<AggregatedWorkitemsETAReportViewModel>().IndividualReports.Should().HaveCount(expectedNumberOfReports);
            RepositoryMock.Verify();
        }

        protected override void Initialize()
        {
            _handler = new GetReportByIdHandler(RepositoryMock.Object, _descriptors,  Mapper);
        }
    }
}
