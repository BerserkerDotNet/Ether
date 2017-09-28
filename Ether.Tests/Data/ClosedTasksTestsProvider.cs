using Ether.Core.Constants;
using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using Ether.Tests.Classifiers;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ether.Tests.Data
{
    public static class ClosedTasksTestsProvider
    {
        public static IEnumerable GetTestCasesForClosedTasks()
        {
            return new TestCaseData[] { SimpleClosed(), SimpleFullCycleClosed(), MultipleClosedUpdates(), MultipleClosedUpdates() };
        }

        public static IEnumerable GetTestCasesForNotClosedTasks()
        {
            return new[] { SimpleClosedFromResolved(), ClosedNotByTeamMember(), SimpleClosedThruResolved() };
        }

        public static IEnumerable<TeamMember> FakeTeam => Enumerable.Range(1, 3)
            .Select(i => new TeamMember { TeamName = $"Member {i}", Email = $"member{i}@foo.com" })
            .ToList();

        #region Resolvable
        private static TestCaseData SimpleClosed()
        {
            var teamMember = FakeTeam.ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-2);
            var updates = UpdateBuilder.Create()
                        .ClosedFromActive(by: teamMember)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, revisedDate, teamMember)
                .SetName($"{nameof(ClosedTasksWorkItemsClassifierTests.ShouldReturnClosedResolution)}On{nameof(SimpleClosed)}");
        }

        private static TestCaseData SimpleFullCycleClosed()
        {
            var teamMember = FakeTeam.ElementAt(0);
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
            var teamMember = FakeTeam.ElementAt(0);
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
            var teamMember = FakeTeam.ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-2);
            var updates = UpdateBuilder.Create()
                        .New()
                        .Then().Activated()
                        .Then().ClosedFromActive(by: FakeTeam.ElementAt(1))
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

        #endregion

        #region NotResolvable
        private static TestCaseData SimpleClosedFromResolved()
        {
            var teamMember = FakeTeam.ElementAt(0);
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
            var teamMember = FakeTeam.ElementAt(0);
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
            var teamMember = FakeTeam.ElementAt(0);
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

        private static WorkItemResolutionRequest GetRequest(IEnumerable<WorkItemUpdate> updates, string type = WorkItemTypes.Task)
        {
            var workItem = new VSTSWorkItem { Fields = new Dictionary<string, string>(), WorkItemId = 0 };
            workItem.Fields.Add(VSTSFieldNames.WorkItemType, type);
            workItem.Updates = updates;

            return new WorkItemResolutionRequest
            {
                WorkItem = workItem,
                Team = FakeTeam
            };
        }
    }
}
