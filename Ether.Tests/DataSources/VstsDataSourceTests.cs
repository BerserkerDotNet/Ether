using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Tests.Handlers;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Dto;
using Ether.Vsts.Types;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.DataSources
{
    public class VstsDataSourceTests : BaseHandlerTest
    {
        private VstsDataSource _dataSource;

        [Test]
        public async Task ShouldReturnProfileById()
        {
            var expectedProfile = Builder<VstsProfile>.CreateNew().Build();
            SetupSingle(expectedProfile, p => p == expectedProfile.Id);

            var result = await _dataSource.GetProfile(expectedProfile.Id);

            result.Id.Should().Be(expectedProfile.Id);
        }

        [Test]
        public async Task ShouldReturnTeamMemberById()
        {
            var expectedMember = Builder<TeamMember>.CreateNew().Build();
            SetupSingle(expectedMember, p => p == expectedMember.Id);

            var result = await _dataSource.GetTeamMember(expectedMember.Id);

            result.Id.Should().Be(expectedMember.Id);
        }

        [Test]
        public async Task ShouldReturnFilteredPullRequests()
        {
            var pullRequests = Builder<PullRequest>.CreateListOfSize(10)
                .All()
                .With(p => p.State = PullRequestState.Active)
                .TheFirst(2)
                .With(p => p.State = PullRequestState.Completed)
                .TheLast(2)
                .With(p => p.State = PullRequestState.Abandoned)
                .Build();
            SetupMultiple(pullRequests);

            var result = await _dataSource.GetPullRequests(p => p.State == ViewModels.Types.PullRequestState.Completed || p.State == ViewModels.Types.PullRequestState.Abandoned);

            result.Should().HaveCount(4);
            result.Should().OnlyContain(p => p.State == ViewModels.Types.PullRequestState.Completed || p.State == ViewModels.Types.PullRequestState.Abandoned);
        }

        [Test]
        public async Task ShouldReturnCorrectETAData()
        {
            var workitem = Builder<WorkItemViewModel>.CreateNew().Build();
            workitem.Fields = new Dictionary<string, string>();
            workitem.Fields.Add(Constants.OriginalEstimateField, "4");
            workitem.Fields.Add(Constants.RemainingWorkField, "8");
            workitem.Fields.Add(Constants.CompletedWorkField, "9");
            var eta = await _dataSource.GetETAValues(workitem);

            eta.Should().NotBeNull();
            eta.OriginalEstimate.Should().Be(4);
            eta.RemainingWork.Should().Be(8);
            eta.CompletedWork.Should().Be(9);
        }

        [Test]
        public async Task ShouldReturnCorrectETAIfNoETAData()
        {
            var workitem = Builder<WorkItemViewModel>.CreateNew().Build();
            workitem.Fields = new Dictionary<string, string>();
            var eta = await _dataSource.GetETAValues(workitem);

            eta.Should().NotBeNull();
            eta.OriginalEstimate.Should().Be(0);
            eta.RemainingWork.Should().Be(0);
            eta.CompletedWork.Should().Be(0);
        }

        [Test]
        public async Task ShouldReturnEmptyListOfWorkItemsIfMemberDoesNotExists()
        {
            var teamMemberId = Guid.NewGuid();
            SetupGetFieldValue<TeamMember, int[]>(teamMemberId, null);

            var workItems = await _dataSource.GetWorkItemsFor(teamMemberId);

            workItems.Should().NotBeNull();
            workItems.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldReturnEmptyListOfWorkItemsIfMemberDoesNotHaveRelatedWorkItems()
        {
            var teamMemberId = Guid.NewGuid();
            SetupGetFieldValue<TeamMember, int[]>(teamMemberId, new int[0]);

            var workItems = await _dataSource.GetWorkItemsFor(teamMemberId);

            workItems.Should().NotBeNull();
            workItems.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldReturnOnlyRelatedWorkItemsForMember()
        {
            var teamMemberId = Guid.NewGuid();
            var relatedWorkItems = new[] { 1, 3, 5, 7 };
            var allWorkItems = Builder<WorkItem>.CreateListOfSize(10).Build();

            SetupGetFieldValue<TeamMember, int[]>(teamMemberId, relatedWorkItems);
            SetupMultipleWithPredicate(allWorkItems);

            var workItems = await _dataSource.GetWorkItemsFor(teamMemberId);

            workItems.Should().NotBeEmpty();
            workItems.Should().HaveCount(relatedWorkItems.Length);
            workItems.Should().OnlyContain(w => relatedWorkItems.Contains(w.WorkItemId));
        }

        [Test]
        public async Task ShouldSkipNonExistantWorkItems()
        {
            var teamMemberId = Guid.NewGuid();
            var relatedWorkItems = new[] { 1, 3, 5, 7, 15, 25 };
            var allWorkItems = Builder<WorkItem>.CreateListOfSize(10).Build();

            SetupGetFieldValue<TeamMember, int[]>(teamMemberId, relatedWorkItems);
            SetupMultipleWithPredicate(allWorkItems);

            var workItems = await _dataSource.GetWorkItemsFor(teamMemberId);

            workItems.Should().NotBeEmpty();
            workItems.Should().HaveCount(relatedWorkItems.Length - 2);
            workItems.Should().OnlyContain(w => relatedWorkItems.Contains(w.WorkItemId));
            workItems.Should().NotContain(w => w.WorkItemId == 15 || w.WorkItemId == 25);
        }

        protected override void Initialize()
        {
            _dataSource = new VstsDataSource(RepositoryMock.Object, Mapper);
        }

        private void SetupGetFieldValue<TType, TProjection>(Guid id, TProjection value)
            where TType : BaseDto
        {
            RepositoryMock.Setup(r => r.GetFieldValueAsync(id, It.IsAny<Expression<Func<TType, TProjection>>>()))
                .ReturnsAsync(value)
                .Verifiable();
        }
    }
}
