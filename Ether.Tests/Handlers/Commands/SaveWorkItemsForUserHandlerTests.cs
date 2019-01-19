using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;
using Ether.Vsts.Handlers.Commands;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class SaveWorkItemsForUserHandlerTests : BaseHandlerTest
    {
        private SaveWorkItemsForUserHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfMemberIsNull()
        {
            _handler.Awaiting(h => h.Handle(new SaveWorkItemsForUser(Enumerable.Empty<WorkItemViewModel>(), null)))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ShouldThrowExceptionIfWorkitemsIsNull()
        {
            var member = Builder<TeamMemberViewModel>.CreateNew().Build();
            _handler.Awaiting(h => h.Handle(new SaveWorkItemsForUser(null, member)))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldNotDoAnythingIfNoWorkitems()
        {
            var member = Builder<TeamMemberViewModel>.CreateNew().Build();
            await _handler.Handle(new SaveWorkItemsForUser(Enumerable.Empty<WorkItemViewModel>(), member));

            RepositoryMock.Verify(r => r.CreateOrUpdateAsync(It.IsAny<WorkItem>()), Times.Never);
            RepositoryMock.Verify(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, object>>>(), It.IsAny<int[]>()), Times.Never);
            RepositoryMock.Verify(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, object>>>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task ShouldUpdateWorkitems()
        {
            var workitems = Builder<WorkItemViewModel>.CreateListOfSize(10)
                .All()
                .With((w, idx) => w.WorkItemId = idx)
                .Build();
            var member = Builder<TeamMemberViewModel>.CreateNew().Build();
            var memberDto = Builder<TeamMember>.CreateNew()
                .With(m => m.RelatedWorkItems, Enumerable.Range(20, 5).ToArray())
                .Build();

            var combinedIds = workitems.Select(w => w.WorkItemId).Union(memberDto.RelatedWorkItems).ToArray();

            SetupSingle(memberDto);
            SetupCreateOrUpdateIf<WorkItem>(_ => true);
            RepositoryMock.Setup(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, int[]>>>(), It.Is<int[]>(v => CheckIfRelatedWorkitemsAreCorrect(v, combinedIds))))
                .Returns(Task.CompletedTask)
                .Verifiable();
            RepositoryMock.Setup(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, DateTime?>>>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _handler.Handle(new SaveWorkItemsForUser(workitems, member));

            RepositoryMock.Verify(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, int[]>>>(), It.IsAny<int[]>()), Times.Once());
            RepositoryMock.Verify(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, DateTime?>>>(), It.IsAny<DateTime>()), Times.Once());
            RepositoryMock.Verify(r => r.CreateOrUpdateIfAsync(It.IsAny<Expression<Func<WorkItem, bool>>>(), It.IsAny<WorkItem>()), Times.Exactly(workitems.Count));
        }

        [Test]
        public async Task ShouldKeepUpdatingWorkitemsIfOneUpdateFails()
        {
            var workitems = Builder<WorkItemViewModel>.CreateListOfSize(8)
                .All()
                .With((w, idx) => w.WorkItemId = idx)
                .Build();
            var member = Builder<TeamMemberViewModel>.CreateNew().Build();
            var memberDto = Builder<TeamMember>.CreateNew()
                .With(m => m.RelatedWorkItems, Enumerable.Range(20, 3).ToArray())
                .Build();

            var combinedIds = workitems.Select(w => w.WorkItemId).Union(memberDto.RelatedWorkItems).ToArray();

            SetupSingle(memberDto);
            SetupCreateOrUpdateIfManual<WorkItem>(_ => true, i => i.WorkItemId == 2)
                .Throws<Exception>();
            SetupCreateOrUpdateIfManual<WorkItem>(_ => true, i => i.WorkItemId != 2)
                .ReturnsAsync(true);
            RepositoryMock.Setup(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, int[]>>>(), It.Is<int[]>(v => CheckIfRelatedWorkitemsAreCorrect(v, combinedIds))))
                .Returns(Task.CompletedTask)
                .Verifiable();
            RepositoryMock.Setup(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, DateTime?>>>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _handler.Handle(new SaveWorkItemsForUser(workitems, member));

            RepositoryMock.Verify(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, int[]>>>(), It.IsAny<int[]>()), Times.Once());
            RepositoryMock.Verify(r => r.UpdateFieldValue(It.IsAny<TeamMember>(), It.IsAny<Expression<Func<TeamMember, DateTime?>>>(), It.IsAny<DateTime>()), Times.Once());
            RepositoryMock.Verify(r => r.CreateOrUpdateIfAsync(It.IsAny<Expression<Func<WorkItem, bool>>>(), It.IsAny<WorkItem>()), Times.Exactly(workitems.Count));
        }

        protected override void Initialize()
        {
            _handler = new SaveWorkItemsForUserHandler(RepositoryMock.Object, Mapper, Mock.Of<ILogger<SaveWorkItemsForUserHandler>>());
        }

        private bool CheckIfRelatedWorkitemsAreCorrect(int[] actual, int[] expected)
        {
            return expected.All(x => actual.Contains(x));
        }
    }
}
