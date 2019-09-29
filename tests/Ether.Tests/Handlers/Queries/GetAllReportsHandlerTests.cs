using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto.Reports;
using Ether.Core.Types.Handlers.Queries;
using Ether.Core.Types.Queries;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    public class GetAllReportsHandlerTests : BaseHandlerTest
    {
        private GetAllReportsHandler _handler;

        [Test]
        public async Task ShouldSortByDateTaken()
        {
            var unOrderedReports = Builder<ReportResult>.CreateListOfSize(10)
                .All()
                .With((r, idx) => r.DateTaken = DateTime.Now.AddDays(-15).AddDays(idx))
                .Build();
            SetupMultiple(unOrderedReports);

            var result = await _handler.Handle(new GetAllReports());

            result.Should().BeInDescendingOrder(p => p.DateTaken);
        }

        protected override void Initialize()
        {
            _handler = new GetAllReportsHandler(RepositoryMock.Object, Mapper);
        }
    }
}
