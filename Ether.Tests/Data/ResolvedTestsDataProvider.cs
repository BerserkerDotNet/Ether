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
    public static class ResolvedWorkItemsDataProvider
    {
        public static IEnumerable GetTestCasesForResolved()
        {
            return new[] { GetSimpleCannotReproduce(), GetResolvedMultipleTimes(), GetVerifiedTask() };
        }

        public static IEnumerable<TeamMember> FakeTeam => Enumerable.Range(1, 3)
            .Select(i => new TeamMember { TeamName = $"Member {i}", Email = $"member{i}@foo.com" })
            .ToList();

        private static TestCaseData GetSimpleCannotReproduce()
        {
            const string CannotReproduce = "Cannot Reproduce";
            var teamMember = FakeTeam.ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .Resolved(teamMember)
                        .Because(CannotReproduce)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, CannotReproduce, revisedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnSimpleCannotReproduce");
        }

        private static TestCaseData GetResolvedMultipleTimes()
        {
            const string CannotReproduce = "Cannot Reproduce";
            const string Reasons = "Reasons";
            var teamMember = FakeTeam.ElementAt(0);
            var expectedTeamMember = $"{teamMember.DisplayName} <{teamMember.Email}>";
            var expectedRevisedDate = DateTime.UtcNow.AddDays(-3);
            var updates = UpdateBuilder.Create()
                .New()
                .Then().Resolved()
                .Because(CannotReproduce)
                .On(DateTime.UtcNow.AddDays(-4))
                .Then().Activated(from: "Resolved")
                .On(DateTime.UtcNow.AddDays(-4))
                .Then().Resolved(by: teamMember)
                .Because(Reasons)
                .On(expectedRevisedDate)
                .Then().New(from: "Resolved")
                .On(expectedRevisedDate.AddHours(8))
                .Then().Resolved()
                .Because("Bad Reasons")
                .On(DateTime.UtcNow.AddDays(-2))
                .Build();
            var request = GetRequest(updates, WorkItemTypes.Task);
            return new TestCaseData(request, Reasons, expectedRevisedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnMultipleResolutions");
        }

        private static TestCaseData GetVerifiedTask()
        {
            const string Reasons = "Reasons";
            var teamMember = FakeTeam.ElementAt(0);
            var expectedTeamMember = $"{teamMember.DisplayName} <{teamMember.Email}>";
            var expectedRevisedDate = DateTime.UtcNow.AddDays(-3);
            var updates = UpdateBuilder.Create()
                .New()
                .Then().Activated()
                .Then().Resolved(by: teamMember)
                .Because(Reasons)
                .On(expectedRevisedDate)
                .Then().Closed(by: teamMember)
                .On(DateTime.UtcNow)
                .Build();
            var request = GetRequest(updates, WorkItemTypes.Task);
            return new TestCaseData(request, Reasons, expectedRevisedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnVerifiedTask");
        }

        private static WorkItemResolutionRequest GetRequest(IEnumerable<WorkItemUpdate> updates, string type = WorkItemTypes.Bug)
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
