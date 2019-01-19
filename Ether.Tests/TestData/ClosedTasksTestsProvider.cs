using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Types;
using Ether.Tests.Classifiers;
using Ether.ViewModels;
using Ether.Vsts;
using FizzWare.NBuilder;
using NUnit.Framework;

namespace Ether.Tests.TestData
{
    public static class ClosedTasksTestsProvider
    {
        public static IEnumerable GetTestCasesForClosedTasks()
        {
            return new TestCaseData[]
            {
                SimpleClosed(),
                SimpleFullCycleClosed(),
                MultipleClosedUpdates(),
                MultipleClosedUpdates(),
                WithCorrectDate(),
                CorrectClosedByIfNeeded()
            };
        }

        public static IEnumerable GetTestCasesForNotClosedTasks()
        {
            return new[] { SimpleClosedFromResolved(), ClosedNotByTeamMember(), SimpleClosedThruResolved() };
        }

        public static IEnumerable<TeamMemberViewModel> GetFakeTeam()
        {
            return Builder<TeamMemberViewModel>.CreateListOfSize(3)
                .All()
                .With((m, i) => m.Email = $"member{i}@foo.com")
                .With((m, i) => m.DisplayName = $"Member {i}")
                .Build();
        }

        #region Resolvable
        private static TestCaseData SimpleClosed()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-2);
            var updates = UpdateBuilder.Create()
                        .ClosedFromActive(by: teamMember).With(Constants.WorkItemAssignedToField, "foo", null)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, revisedDate, teamMember)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnClosedResolution)}On{nameof(SimpleClosed)}");
        }

        private static TestCaseData WithCorrectDate()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var changedDate = DateTime.UtcNow.AddDays(-2);
            var updates = UpdateBuilder.Create()
                        .ClosedFromActive(by: teamMember)
                        .On(changedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, changedDate, teamMember)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnClosedResolution)}On{nameof(SimpleClosed)}");
        }

        private static TestCaseData SimpleFullCycleClosed()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .New()
                        .Then().Activated()
                        .Then().ClosedFromActive(by: teamMember)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, revisedDate, teamMember)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnClosedResolution)}On{nameof(SimpleFullCycleClosed)}");
        }

        private static TestCaseData MultipleClosedUpdates()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-2);
            var updates = UpdateBuilder.Create()
                        .New()
                        .Then().Activated()
                        .Then().ClosedFromActive(by: teamMember)
                        .On(DateTime.UtcNow.AddDays(-4))
                        .Then().Activated()
                        .Then().ClosedFromActive()
                        .Then().New()
                        .Then().ClosedFromActive(by: teamMember)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, revisedDate, teamMember)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnClosedResolution)}On{nameof(MultipleClosedUpdates)}");
        }

        private static TestCaseData ClosedByDifferentTeamMembers()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-2);
            var updates = UpdateBuilder.Create()
                        .New()
                        .Then().Activated()
                        .Then().ClosedFromActive(by: GetFakeTeam().ElementAt(1))
                        .On(DateTime.UtcNow.AddDays(-4))
                        .Then().Activated()
                        .Then().ClosedFromActive()
                        .Then().New()
                        .Then().ClosedFromActive(by: teamMember)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, revisedDate, teamMember)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnClosedResolution)}On{nameof(ClosedByDifferentTeamMembers)}");
        }

        private static TestCaseData CorrectClosedByIfNeeded()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var resolver = GetFakeTeam().ElementAt(1);
            var assignedTo = $"{teamMember.DisplayName} <{teamMember.Email}>";
            var revisedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .New()
                        .Then().Activated().With(Constants.WorkItemAssignedToField, assignedTo, string.Empty)
                        .Then().ClosedFromActive(by: resolver).With(Constants.WorkItemAssignedToField, string.Empty, assignedTo)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, revisedDate, teamMember)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnClosedResolution)}On{nameof(CorrectClosedByIfNeeded)}");
        }

        #endregion

        #region NotResolvable
        private static TestCaseData SimpleClosedFromResolved()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .New()
                        .Then().Activated()
                        .Then().Resolved(by: teamMember)
                        .Then().Closed(by: teamMember)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnNoneResolution)}On{nameof(SimpleClosedFromResolved)}");
        }

        private static TestCaseData SimpleClosedThruResolved()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .New()
                        .Then().Resolved(by: teamMember)
                        .Then().Activated()
                        .Then().ClosedFromActive(by: teamMember)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnNoneResolution)}On{nameof(SimpleClosedThruResolved)}");
        }

        private static TestCaseData ClosedNotByTeamMember()
        {
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .Then().ClosedFromActive()
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnNoneResolution)}On{nameof(ClosedNotByTeamMember)}");
        }

        #endregion

        private static WorkItemResolutionRequest GetRequest(IEnumerable<WorkItemUpdateViewModel> updates, string type = Constants.WorkItemTypeTask)
        {
            var workItem = new WorkItemViewModel { Fields = new Dictionary<string, string>(), WorkItemId = 0 };
            workItem.Fields.Add(Constants.WorkItemTypeField, type);
            workItem.Updates = updates;

            return new WorkItemResolutionRequest
            {
                WorkItem = workItem,
                Team = GetFakeTeam()
            };
        }
    }
}
