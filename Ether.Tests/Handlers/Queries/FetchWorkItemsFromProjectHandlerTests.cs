using System;
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
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.WorkItems;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class FetchWorkItemsFromProjectHandlerTests : BaseHandlerTest
    {
        private const string DummyEmail = "dummy@foo.com";

        private static DateTime?[] _expectedDates = new DateTime?[]
        {
                    null,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddDays(-7)
        };

        private FetchWorkItemsFromProjectHandler _handler;
        private Mock<IVstsClientFactory> _clientFactoryMock;
        private Mock<IVstsClient> _clientMock;

        [Test]
        public void ShouldThrowExceptionIfMemberIsNull()
        {
            _handler.Awaiting(h => h.Handle(new FetchWorkItemsFromProject(null)))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(nameof(_expectedDates))]
        public async Task ShouldFetchWorkitemsAndUpdates(DateTime lastFetchDate)
        {
            var teamMember = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email, DummyEmail)
                .With(m => m.LastWorkitemsFetchDate, lastFetchDate)
                .Build();
            var workitems = Builder<WorkItem>.CreateListOfSize(5)
                .Build();
            var references = workitems.Select(w => new WorkItemReference(w.Id));
            var ids = workitems.Select(w => w.Id).ToArray();
            var updates = Builder<WorkItemUpdate>.CreateListOfSize(2)
                .All()
                .With(u => u.Fields, new System.Collections.Generic.Dictionary<string, VSTS.Net.Models.WorkItems.WorkItemFieldUpdate>())
                .Build();

            _clientMock.Setup(c => c.ExecuteFlatQueryAsync(It.Is<string>(q => ValidateQueryString(q, lastFetchDate)), default(CancellationToken)))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    WorkItems = references
                })
                .Verifiable();
            _clientMock.Setup(c => c.GetWorkItemsAsync(ids, null, It.IsAny<string[]>(), default(CancellationToken)))
                .ReturnsAsync(workitems)
                .Verifiable();
            _clientMock.Setup(c => c.GetWorkItemUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updates)
                .Verifiable();

            var result = await _handler.Handle(new FetchWorkItemsFromProject(teamMember));

            result.Should().HaveCount(workitems.Count);
            result.Should().OnlyContain(vm => workitems.Any(w => w.Id == vm.WorkItemId) && vm.Updates.Count() == 2);

            _clientFactoryMock.Verify();
            _clientMock.Verify();
            _clientMock.Verify(c => c.GetWorkItemUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(workitems.Count));
        }

        [Test]
        public async Task ShouldNotTryToFetchUpdatesIfNoWorkitems()
        {
            var teamMember = Builder<TeamMemberViewModel>.CreateNew()
               .With(m => m.Email, DummyEmail)
               .With(m => m.LastWorkitemsFetchDate, null)
               .Build();

            _clientMock.Setup(c => c.ExecuteFlatQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    WorkItems = Enumerable.Empty<WorkItemReference>()
                })
                .Verifiable();

            var result = await _handler.Handle(new FetchWorkItemsFromProject(teamMember));

            result.Should().BeEmpty();

            _clientFactoryMock.Verify();
            _clientMock.Verify();
            _clientMock.Verify(c => c.GetWorkItemsAsync(It.IsAny<int[]>(), null, It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Never);
            _clientMock.Verify(c => c.GetWorkItemUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ShouldReturnOnlyRequestedFields()
        {
            var fields = new Dictionary<string, string>
            {
                { "System.Id", "1" },
                { "FakeField1", "Fake" },
                { "System.Title", "Workitem" },
                { "FakeField2", "Fake" },
                { "System.AreaPath", "Bugs" }
            };
            var teamMember = Builder<TeamMemberViewModel>.CreateNew()
               .With(m => m.Email, DummyEmail)
               .With(m => m.LastWorkitemsFetchDate, null)
               .Build();

            var workitems = Builder<WorkItem>.CreateListOfSize(5)
                .All()
                .With(w => w.Fields, fields)
                .Build();
            var references = workitems.Select(w => new WorkItemReference(w.Id));
            var ids = workitems.Select(w => w.Id).ToArray();
            _clientMock.Setup(c => c.ExecuteFlatQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    WorkItems = references
                })
                .Verifiable();

            var updates = Builder<WorkItemUpdate>.CreateListOfSize(2)
                .All()
                .With(u => u.Fields, new System.Collections.Generic.Dictionary<string, VSTS.Net.Models.WorkItems.WorkItemFieldUpdate>())
                .Build();

            _clientMock.Setup(c => c.ExecuteFlatQueryAsync(It.Is<string>(q => ValidateQueryString(q, DateTime.Now)), default(CancellationToken)))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    WorkItems = references
                })
                .Verifiable();
            _clientMock.Setup(c => c.GetWorkItemsAsync(ids, null, It.IsAny<string[]>(), default(CancellationToken)))
                .ReturnsAsync(workitems)
                .Verifiable();
            _clientMock.Setup(c => c.GetWorkItemUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updates)
                .Verifiable();

            var result = await _handler.Handle(new FetchWorkItemsFromProject(teamMember));

            result.Should().OnlyContain(w => !w.Fields.ContainsKey("FakeField1") && !w.Fields.ContainsKey("FakeField2"));
        }

        [Test]
        public async Task ShouldReturnRelations()
        {
            var teamMember = Builder<TeamMemberViewModel>.CreateNew()
               .With(m => m.Email, DummyEmail)
               .With(m => m.LastWorkitemsFetchDate, null)
               .Build();

            var relations = Builder<WorkItemRelation>.CreateListOfSize(6)
                .Build();

            var workitems = Builder<WorkItem>.CreateListOfSize(5)
                .All()
                .With(w => w.Relations, relations)
                .Build();
            var references = workitems.Select(w => new WorkItemReference(w.Id));
            var ids = workitems.Select(w => w.Id).ToArray();
            _clientMock.Setup(c => c.ExecuteFlatQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    WorkItems = references
                })
                .Verifiable();

            var updates = Builder<WorkItemUpdate>.CreateListOfSize(2)
                .All()
                .With(u => u.Fields, new System.Collections.Generic.Dictionary<string, VSTS.Net.Models.WorkItems.WorkItemFieldUpdate>())
                .Build();

            _clientMock.Setup(c => c.ExecuteFlatQueryAsync(It.Is<string>(q => ValidateQueryString(q, DateTime.Now)), default(CancellationToken)))
                .ReturnsAsync(new FlatWorkItemsQueryResult
                {
                    WorkItems = references
                })
                .Verifiable();
            _clientMock.Setup(c => c.GetWorkItemsAsync(ids, null, It.IsAny<string[]>(), default(CancellationToken)))
                .ReturnsAsync(workitems)
                .Verifiable();
            _clientMock.Setup(c => c.GetWorkItemUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updates)
                .Verifiable();

            var result = await _handler.Handle(new FetchWorkItemsFromProject(teamMember));

            result.Should().OnlyContain(w => w.Relations != null && w.Relations.Any());
        }

        protected override void Initialize()
        {
            _clientFactoryMock = new Mock<IVstsClientFactory>(MockBehavior.Strict);
            _clientMock = new Mock<IVstsClient>(MockBehavior.Strict);
            _handler = new FetchWorkItemsFromProjectHandler(_clientFactoryMock.Object, Mock.Of<ILogger<FetchWorkItemsFromProjectHandler>>(), Mapper);

            _clientFactoryMock.Setup(c => c.GetClient(null))
                .ReturnsAsync(_clientMock.Object)
                .Verifiable();
        }

        private bool ValidateQueryString(string query, DateTime expectedChangedDateFilter)
        {
            return query.Contains($"'{DummyEmail}'") && query.Contains(expectedChangedDateFilter.ToString("MM/dd/yyyy"));
        }
    }
}