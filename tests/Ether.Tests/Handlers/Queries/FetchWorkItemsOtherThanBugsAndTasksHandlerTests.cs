using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.Vsts.Handlers.Queries;
using Ether.Vsts.Interfaces;
using Ether.Vsts.Queries;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.WorkItems;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class FetchWorkItemsOtherThanBugsAndTasksHandlerTests : BaseHandlerTest
    {
        private FetchWorkItemsOtherThanBugsAndTasksHandler _handler;
        private Mock<IVstsClientFactory> _clientFactoryMock;
        private Mock<IVstsClient> _clientMock;

        [Test]
        public async Task ShouldFetchWorkItemsAndWorkItemTypes()
        {
            var workitems = Builder<WorkItem>.CreateListOfSize(5)
                .TheFirst(3)
                .With(w => w.Fields, new Dictionary<string, string> { { "System.WorkItemType", "Foo" } })
                .TheRest()
                .With(w => w.Fields, new Dictionary<string, string>())
                .Build();
            var organization = Builder<OrganizationViewModel>.CreateNew()
                .Build();
            var references = workitems.Select(w => new WorkItemReference(w.Id));
            var ids = workitems.Select(w => w.Id).ToArray();

            _clientMock.Setup(c => c.ExecuteFlatQueryAsync(It.IsAny<string>(), default(CancellationToken)))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    WorkItems = references
                })
                .Verifiable();
            _clientMock.Setup(c => c.GetWorkItemsAsync(ids, null, It.IsAny<string[]>(), default(CancellationToken)))
                .ReturnsAsync(workitems)
                .Verifiable();

            RepositoryMock.Setup(r => r.GetByFilteredArrayAsync<Ether.Vsts.Dto.WorkItem>("Fields.v", It.IsAny<string[]>()))
                .ReturnsAsync(workitems.Select(w => new Ether.Vsts.Dto.WorkItem
                {
                    WorkItemId = w.Id
                }));

            var result = await _handler.Handle(new FetchWorkItemsOtherThanBugsAndTasks(organization.Id));

            result.Should().HaveCountLessOrEqualTo(workitems.Count);
            result.Should().OnlyContain(i => workitems.Any(w => w.Id == i));

            _clientFactoryMock.Verify();
            _clientMock.Verify();
        }

        protected override void Initialize()
        {
            _clientFactoryMock = new Mock<IVstsClientFactory>(MockBehavior.Strict);
            _clientMock = new Mock<IVstsClient>(MockBehavior.Strict);
            _handler = new FetchWorkItemsOtherThanBugsAndTasksHandler(_clientFactoryMock.Object, RepositoryMock.Object);

            _clientFactoryMock.Setup(c => c.GetClient(default, null))
                .ReturnsAsync(_clientMock.Object)
                .Verifiable();
        }
    }
}
