using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Tests.Extensions;
using Ether.Tests.Handlers;
using Ether.Tests.TestData;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Dto;
using Ether.Vsts.Types;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static Ether.Vsts.Constants;

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
        public void ShouldReturnCorrectETAData()
        {
            var workitem = Builder<WorkItemViewModel>.CreateNew().Build();
            workitem.Fields = new Dictionary<string, string>();
            workitem.Fields.Add(Constants.OriginalEstimateField, "4");
            workitem.Fields.Add(Constants.RemainingWorkField, "8");
            workitem.Fields.Add(Constants.CompletedWorkField, "9");
            var eta = _dataSource.GetETAValues(workitem);

            eta.Should().NotBeNull();
            eta.OriginalEstimate.Should().Be(4);
            eta.RemainingWork.Should().Be(8);
            eta.CompletedWork.Should().Be(9);
        }

        [Test]
        public void ShouldReturnCorrectETAIfNoETAData()
        {
            var workitem = Builder<WorkItemViewModel>.CreateNew().Build();
            workitem.Fields = new Dictionary<string, string>();
            var eta = _dataSource.GetETAValues(workitem);

            eta.Should().NotBeNull();
            eta.OriginalEstimate.Should().Be(0);
            eta.RemainingWork.Should().Be(0);
            eta.CompletedWork.Should().Be(0);
        }

        [Test]
        public void ShouldFallbackToGettingETAFromUpdates()
        {
            var workitem = Builder<WorkItemViewModel>.CreateNew().Build();
            workitem.Fields = new Dictionary<string, string>();
            workitem.Updates = UpdateBuilder.Create()
                .Activated()
                .Then()
                .Closed()
                .With(Constants.RemainingWorkField, "0", "6")
                .Build();
            var eta = _dataSource.GetETAValues(workitem);

            eta.Should().NotBeNull();
            eta.OriginalEstimate.Should().Be(0);
            eta.RemainingWork.Should().Be(6);
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

        #region GetActiveDuration tests

        [Test]
        public void GetActiveDurationShouldReturnDefaultIfUpdatesIsNull()
        {
            var bugData = WorkItemsFactory.CreateBug();

            var result = _dataSource.GetActiveDuration(bugData.WorkItem, Enumerable.Empty<TeamMemberViewModel>());

            result.Should().Be(1.0f);
        }

        [Test]
        public void GetActiveDurationShouldReturnDefaultIfNoUpdates()
        {
            var bugData = WorkItemsFactory.CreateBug().WithNoUpdates();

            var result = _dataSource.GetActiveDuration(bugData.WorkItem, Enumerable.Empty<TeamMemberViewModel>());

            result.Should().Be(1.0f);
        }

        [Test]
        public void GetActiveDurationShouldReturnActiveDurationForResolvedBug()
        {
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var bugData = WorkItemsFactory.CreateBug().WithNormalLifecycle(resolvedBy: john, 5, activatedBy: john, assignedTo: john);

            var result = _dataSource.GetActiveDuration(bugData.WorkItem, new[] { john });

            result.Should().Be(bugData.ExpectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldReturnActiveDurationForClosedTasks()
        {
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory.CreateTask().WithNormalLifecycle(resolvedBy: john, 3, activatedBy: john, assignedTo: john);

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(taskData.ExpectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldNotCountTimeWhileBlocked()
        {
            const int expectedDuration = 7;
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory
                .CreateTask()
                .WithNormalLifecycle(resolvedBy: john, 10, activatedBy: john, assignedTo: john, onAfterActivation: (builder, activationDate) =>
                {
                    builder = builder
                        .Then().AddTag(Constants.BlockedTag).On(activationDate.AddBusinessDays(1))
                        .Then().RemoveTag(Constants.BlockedTag).On(activationDate.AddBusinessDays(2))
                        .Then().AddTag(Constants.BlockedTag).On(activationDate.AddBusinessDays(5))
                        .Then().RemoveTag(Constants.BlockedTag).On(activationDate.AddBusinessDays(7));
                });

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldCorrectlyHandleOnHoldToCodeReviewInOneUpdate()
        {
            const int expectedDuration = 2;
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory
                .CreateTask();

            taskData.WorkItem.Updates = UpdateBuilder.Create()
                .New()
                .With(WorkItemAssignedToField, john.Email, string.Empty)
                .On(new DateTime(2019, 7, 18))
                .Then().Activated().On(new DateTime(2019, 7, 19))
                .Then().AddTag(OnHoldTag).On(new DateTime(2019, 7, 23))
                .Then().RemoveTag(OnHoldTag).AddTag(CodeReviewTag).On(new DateTime(2019, 7, 25))
                .Then().Closed(john, "Active").On(new DateTime(2019, 8, 1))
                .Build();

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldCorrectlyHandleAddedPullRequest()
        {
            const int expectedDuration = 2;
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory
                .CreateTask();

            taskData.WorkItem.Updates = UpdateBuilder.Create()
                .New()
                .With(WorkItemAssignedToField, john.Email, string.Empty)
                .On(new DateTime(2019, 7, 9))
                .Then().Activated().On(new DateTime(2019, 7, 10))
                .Then().WithPullRequest(111111).On(new DateTime(2019, 7, 12))
                .Then().Closed(john, "Active").On(new DateTime(2019, 7, 26))
                .Build();

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldCorrectlyHandleWhenPullRequestAddedBeforeActivation()
        {
            const int expectedDuration = 2;
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory
                .CreateTask();

            taskData.WorkItem.Updates = UpdateBuilder.Create()
                .New()
                .With(WorkItemAssignedToField, john.Email, string.Empty).On(new DateTime(2019, 7, 9))
                .WithPullRequest(2222222).On(new DateTime(2019, 7, 09))
                .Then().Activated().On(new DateTime(2019, 7, 10))
                .Then().Closed(john, "Active").On(new DateTime(2019, 7, 12))
                .Build();

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldCorrectlyHandleWhenPullRequestAddedAfterActivation()
        {
            const int expectedDuration = 2;
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory
                .CreateTask();

            taskData.WorkItem.Updates = UpdateBuilder.Create()
                .New().On(new DateTime(2019, 7, 9))
                .Then().Activated().On(new DateTime(2019, 7, 10))
                .Then().WithPullRequest(2222222).On(new DateTime(2019, 7, 11))
                .Then().With(WorkItemAssignedToField, john.Email, string.Empty).On(new DateTime(2019, 7, 12))
                .Then().Closed(john, "Active").On(new DateTime(2019, 7, 16))
                .Build();

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldNotCountTimeWhileOnHold()
        {
            const int expectedDuration = 7;
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory
                .CreateTask()
                .WithNormalLifecycle(resolvedBy: john, 10, activatedBy: john, assignedTo: john, onAfterActivation: (builder, activationDate) =>
                {
                    builder = builder
                        .Then().AddTag(Constants.OnHoldTag).On(activationDate.AddBusinessDays(1))
                        .Then().RemoveTag(Constants.OnHoldTag).On(activationDate.AddBusinessDays(2))
                        .Then().AddTag(Constants.OnHoldTag).On(activationDate.AddBusinessDays(5))
                        .Then().RemoveTag(Constants.OnHoldTag).On(activationDate.AddBusinessDays(7));
                });

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldReturnDefaultIfItemWasResolvedWithoutActivation()
        {
            const int expectedDuration = 1;
            var john = Builder<TeamMemberViewModel>.CreateNew().Build();
            var taskData = WorkItemsFactory
                .CreateTask()
                .WithResolvedWithoutActivation(5, john);

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldReturnDefaultIfItemWasNotActivateByTeam()
        {
            const int expectedDuration = 1;
            var john = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email, "john@foo.com")
                .Build();
            var merry = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email, "merry@foo.com")
                .Build();
            var taskData = WorkItemsFactory
                .CreateTask()
                .WithNormalLifecycle(resolvedBy: john, 5, activatedBy: merry, assignedTo: merry);

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        public void GetActiveDurationShouldReturnOnlyTimeItrWasActuallyAssignedToTeamMember()
        {
            const int expectedDuration = 3;
            var john = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email, "john@foo.com")
                .Build();
            var merry = Builder<TeamMemberViewModel>.CreateNew()
                .With(m => m.Email, "merry@foo.com")
                .Build();
            var taskData = WorkItemsFactory
                .CreateTask()
                .WithNormalLifecycle(resolvedBy: john, 5, activatedBy: merry, assignedTo: merry, onAfterActivation: (builder, activationDate) =>
                {
                    builder = builder.Then().With(Constants.WorkItemAssignedToField, john.Email, merry.Email).On(activationDate.AddDays(2));
                });

            var result = _dataSource.GetActiveDuration(taskData.WorkItem, new[] { john });

            result.Should().Be(expectedDuration);
        }

        [Test]
        [Ignore("New feature")]
        public void GetActiveDurationShouldReturnActiveDurationForStillActiveItem()
        {
            var bugData = WorkItemsFactory.CreateBug().WithActiveWorkItem(5);

            var result = _dataSource.GetActiveDuration(bugData.WorkItem, Enumerable.Empty<TeamMemberViewModel>());

            result.Should().Be(bugData.ExpectedDuration);
        }

        #endregion

        #region IsInCodeReview tests

        [TestCase(WorkItemStateResolved, "", false)]
        [TestCase(WorkItemStateResolved, "code review", false)]
        [TestCase(WorkItemStateActive, "", false)]
        [TestCase(WorkItemStateActive, "code review", true)]
        [TestCase(WorkItemStateActive, "codereview", true)]
        [TestCase(WorkItemStateActive, "Code Review", true)]
        [TestCase(WorkItemStateActive, "foo;code review;bla;bar", true)]
        public async Task IsInCodeReviewWorkItemStateAndTagTests(string state, string tags, bool expected)
        {
            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = state,
                    [WorkItemTagsField] = tags
                })
                .Build();
            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().Be(expected);
        }

        [Test]
        public async Task IsInCodeReviewReturnFalseIfNoRelations()
        {
            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = WorkItemStateActive,
                    [WorkItemTagsField] = string.Empty
                })
                .With(w => w.Relations, null)
                .Build();

            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().BeFalse();
        }

        [Test]
        public async Task IsInCodeReviewReturnFalseIfEmptyRelations()
        {
            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = WorkItemStateActive,
                    [WorkItemTagsField] = string.Empty
                })
                .With(w => w.Relations, Enumerable.Empty<ViewModels.Types.WorkItemRelation>())
                .Build();

            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().BeFalse();
        }

        [Test]
        public async Task IsInCodeReviewReturnFalseIfNoPullRequestRelations()
        {
            var relations = Builder<ViewModels.Types.WorkItemRelation>.CreateListOfSize(5)
                .All()
                .With(r => r.RelationType, "ArtifactLink")
                .With(r => r.Url, new Uri("vstfs:///Git/Commit/bla%2ffoo%2fsha"))
                .Build();
            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = WorkItemStateActive,
                    [WorkItemTagsField] = string.Empty
                })
                .With(w => w.Relations, relations)
                .Build();

            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().BeFalse();
        }

        [Test]
        public async Task IsInCodeReviewReturnFalseIfPullRequestsNotFetched()
        {
            var relations = Builder<ViewModels.Types.WorkItemRelation>.CreateListOfSize(2)
                .All()
                .With(r => r.RelationType, "ArtifactLink")
                .TheFirst(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f123456789"))
                .TheNext(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f987654321"))
                .Build();

            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = WorkItemStateActive,
                    [WorkItemTagsField] = string.Empty
                })
                .With(w => w.Relations, relations)
                .Build();

            var prs = Builder<PullRequest>.CreateListOfSize(1).Build();
            RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<PullRequest, bool>>>()))
                .ReturnsAsync(prs);

            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().BeFalse();
        }

        [Test]
        public async Task IsInCodeReviewReturnFalseIfPullRequestsCompleted()
        {
            var relations = Builder<ViewModels.Types.WorkItemRelation>.CreateListOfSize(2)
                .All()
                .With(r => r.RelationType, "ArtifactLink")
                .TheFirst(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f123456789"))
                .TheNext(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f987654321"))
                .Build();

            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = WorkItemStateActive,
                    [WorkItemTagsField] = string.Empty
                })
                .With(w => w.Relations, relations)
                .Build();

            var prs = Builder<PullRequest>.CreateListOfSize(2)
                .All()
                .With(p => p.State, PullRequestState.Completed)
                .Build();
            RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<PullRequest, bool>>>()))
                .ReturnsAsync(prs);

            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().BeFalse();
        }

        [Test]
        public async Task IsInCodeReviewReturnTrueIfActiveAndCompletedPullRequests()
        {
            var relations = Builder<ViewModels.Types.WorkItemRelation>.CreateListOfSize(2)
                .All()
                .With(r => r.RelationType, "ArtifactLink")
                .TheFirst(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f123456789"))
                .TheNext(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f987654321"))
                .Build();

            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = WorkItemStateActive,
                    [WorkItemTagsField] = string.Empty
                })
                .With(w => w.Relations, relations)
                .Build();

            var prs = Builder<PullRequest>.CreateListOfSize(2)
                .TheFirst(1)
                .With(p => p.State, PullRequestState.Completed)
                .TheNext(1)
                .With(p => p.State, PullRequestState.Active)
                .Build();
            RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<PullRequest, bool>>>()))
                .ReturnsAsync(prs);

            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().BeTrue();
        }

        [Test]
        public async Task IsInCodeReviewReturnTrueIfActivePullRequests()
        {
            var relations = Builder<ViewModels.Types.WorkItemRelation>.CreateListOfSize(2)
                .All()
                .With(r => r.RelationType, "ArtifactLink")
                .TheFirst(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f123456789"))
                .TheNext(1)
                .With(r => r.Url, new Uri("vstfs:///Git/PullRequestId/bla%2ffoo%2f987654321"))
                .Build();

            var item = Builder<WorkItemViewModel>.CreateNew()
                .With(w => w.Fields, new Dictionary<string, string>
                {
                    [WorkItemStateField] = WorkItemStateActive,
                    [WorkItemTagsField] = string.Empty
                })
                .With(w => w.Relations, relations)
                .Build();

            var prs = Builder<PullRequest>.CreateListOfSize(2)
                .All()
                .With(p => p.State, PullRequestState.Active)
                .Build();
            RepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<PullRequest, bool>>>()))
                .ReturnsAsync(prs);

            var isInCodeReview = await _dataSource.IsInCodeReview(item);

            isInCodeReview.Should().BeTrue();
        }

        #endregion

        #region CreateWorkItemDetail tests

        [Test]
        public void ShouldSetRemainingToOrginal()
        {
            const float expected = 3.45F;

            var vm = Builder<WorkItemViewModel>.CreateNew()
                .With(v => v.Fields, new Dictionary<string, string>())
                .With(v => v.Updates, Enumerable.Empty<WorkItemUpdateViewModel>())
                .With(v => v.Fields[Constants.OriginalEstimateField] = expected.ToString())
                .With(v => v.Fields[Constants.RemainingWorkField] = "0")
                .With(v => v.Fields[Constants.WorkItemTitleField] = "test")
                .With(v => v.Fields[Constants.WorkItemTypeField] = Constants.WorkItemTypeBug)
                .Build();

            var result = _dataSource.CreateWorkItemDetail(vm, Enumerable.Empty<TeamMemberViewModel>());

            result.OriginalEstimate.Should().Be(expected);
            result.EstimatedToComplete.Should().Be(expected);
        }

        [Test]
        public void ShouldNotOverrideRemainingWithOrginalIfRemainingNotNull()
        {
            const float expectedOriginal = 3.45F;
            const float expectedRemaining = 2.0F;

            var vm = Builder<WorkItemViewModel>.CreateNew()
                .With(v => v.Fields, new Dictionary<string, string>())
                .With(v => v.Updates, Enumerable.Empty<WorkItemUpdateViewModel>())
                .With(v => v.Fields[Constants.OriginalEstimateField] = expectedOriginal.ToString())
                .With(v => v.Fields[Constants.RemainingWorkField] = expectedRemaining.ToString())
                .With(v => v.Fields[Constants.WorkItemTitleField] = "test")
                .With(v => v.Fields[Constants.WorkItemTypeField] = Constants.WorkItemTypeBug)
                .Build();

            var result = _dataSource.CreateWorkItemDetail(vm, Enumerable.Empty<TeamMemberViewModel>());

            result.OriginalEstimate.Should().Be(expectedOriginal);
            result.EstimatedToComplete.Should().Be(expectedRemaining);
        }

        [Test]
        public void ShouldNotOverrideRemainingWithOrginalIfCompletedNotNull()
        {
            const float expectedOriginal = 3.45F;
            const float expectedCompleted = 2.0F;

            var vm = Builder<WorkItemViewModel>.CreateNew()
                .With(v => v.Fields, new Dictionary<string, string>())
                .With(v => v.Updates, Enumerable.Empty<WorkItemUpdateViewModel>())
                .With(v => v.Fields[Constants.OriginalEstimateField] = expectedOriginal.ToString())
                .With(v => v.Fields[Constants.CompletedWorkField] = expectedCompleted.ToString())
                .With(v => v.Fields[Constants.WorkItemTitleField] = "test")
                .With(v => v.Fields[Constants.WorkItemTypeField] = Constants.WorkItemTypeBug)
                .Build();

            var result = _dataSource.CreateWorkItemDetail(vm, Enumerable.Empty<TeamMemberViewModel>());

            result.OriginalEstimate.Should().Be(expectedOriginal);
            result.EstimatedToComplete.Should().Be(expectedCompleted);
        }

        [Test]
        public void ShouldCalculateEstimateAsSumOfRemainingAndCompleted()
        {
            const float expectedOriginal = 3.45F;
            const float expectedRemaining = 2.0F;
            const float expectedCompleted = 4.0F;

            var vm = Builder<WorkItemViewModel>.CreateNew()
                .With(v => v.Fields, new Dictionary<string, string>())
                .With(v => v.Updates, Enumerable.Empty<WorkItemUpdateViewModel>())
                .With(v => v.Fields[Constants.OriginalEstimateField] = expectedOriginal.ToString())
                .With(v => v.Fields[Constants.RemainingWorkField] = expectedRemaining.ToString())
                .With(v => v.Fields[Constants.CompletedWorkField] = expectedCompleted.ToString())
                .With(v => v.Fields[Constants.WorkItemTitleField] = "test")
                .With(v => v.Fields[Constants.WorkItemTypeField] = Constants.WorkItemTypeBug)
                .Build();

            var result = _dataSource.CreateWorkItemDetail(vm, Enumerable.Empty<TeamMemberViewModel>());

            result.OriginalEstimate.Should().Be(expectedOriginal);
            result.EstimatedToComplete.Should().Be(expectedRemaining + expectedCompleted);
        }

        [Test]
        public void ShouldSetEstimateAndTimeSpentToDefaultIfAllFieldsAreNull()
        {
            const float defaultValue = 1.0F;

            var vm = Builder<WorkItemViewModel>.CreateNew()
                .With(v => v.Fields, new Dictionary<string, string>())
                .With(v => v.Updates, Enumerable.Empty<WorkItemUpdateViewModel>())
                .With(v => v.Fields[Constants.WorkItemTitleField] = "test")
                .With(v => v.Fields[Constants.WorkItemTypeField] = Constants.WorkItemTypeBug)
                .Build();

            var result = _dataSource.CreateWorkItemDetail(vm, Enumerable.Empty<TeamMemberViewModel>());

            result.OriginalEstimate.Should().Be(0);
            result.EstimatedToComplete.Should().Be(defaultValue);
            result.TimeSpent.Should().Be(defaultValue);
        }

        #endregion

        protected override void Initialize()
        {
            _dataSource = new VstsDataSource(RepositoryMock.Object, Mapper, Mock.Of<ILogger<VstsDataSource>>());
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
