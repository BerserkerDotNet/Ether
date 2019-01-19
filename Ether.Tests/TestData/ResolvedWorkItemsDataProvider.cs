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
    public static class ResolvedWorkItemsDataProvider
    {
        public static IEnumerable GetTestCasesForResolved()
        {
            return new[]
            {
                GetSimpleCannotReproduce(),
                GetResolvedMultipleTimes(),
                GetVerifiedTask(),
                TestCorrectChangedDate(),
                CorrectResolvedByIfNeeded()
            };
        }

        public static IEnumerable<TeamMemberViewModel> GetFakeTeam()
        {
            return Builder<TeamMemberViewModel>.CreateListOfSize(3)
                    .All()
                    .With((m, i) => m.Email = $"member{i}@foo.com")
                    .With((m, i) => m.DisplayName = $"Member {i}")
                    .Build();
        }

        private static TestCaseData GetSimpleCannotReproduce()
        {
            const string CannotReproduce = "Cannot Reproduce";
            var teamMember = GetFakeTeam().ElementAt(0);
            var revisedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .Resolved(teamMember).With(Constants.WorkItemAssignedToField, "foo", null)
                        .Because(CannotReproduce)
                        .On(revisedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, CannotReproduce, revisedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnSimpleCannotReproduce");
        }

        private static TestCaseData TestCorrectChangedDate()
        {
            const string CannotReproduce = "Cannot Reproduce";
            var teamMember = GetFakeTeam().ElementAt(0);
            var changedDate = DateTime.UtcNow.AddDays(-4);
            var updates = UpdateBuilder.Create()
                        .Resolved(teamMember)
                        .Because(CannotReproduce)
                        .On(changedDate)
                        .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, CannotReproduce, changedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "WithCorrectChangedDate");
        }

        private static TestCaseData GetResolvedMultipleTimes()
        {
            const string CannotReproduce = "Cannot Reproduce";
            const string Reasons = "Reasons";
            var teamMember = GetFakeTeam().ElementAt(0);
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
            var request = GetRequest(updates, Constants.WorkItemTypeTask);
            return new TestCaseData(request, Reasons, expectedRevisedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnMultipleResolutions");
        }

        private static TestCaseData GetVerifiedTask()
        {
            const string Reasons = "Reasons";
            var teamMember = GetFakeTeam().ElementAt(0);
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
            var request = GetRequest(updates, Constants.WorkItemTypeTask);
            return new TestCaseData(request, Reasons, expectedRevisedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnVerifiedTask");
        }

        private static TestCaseData CorrectResolvedByIfNeeded()
        {
            const string Reasons = "Reasons";
            var teamMember = GetFakeTeam().ElementAt(0);
            var resolver = GetFakeTeam().ElementAt(1);
            var assignedTo = $"{teamMember.DisplayName} <{teamMember.Email}>";
            var expectedRevisedDate = DateTime.UtcNow.AddDays(-3);
            var updates = UpdateBuilder.Create()
                .New()
                .With(Constants.WorkItemAssignedToField, assignedTo, string.Empty)
                .Then().Activated()
                .Then().Resolved(by: resolver).Because(Reasons)
                .With(Constants.WorkItemAssignedToField, string.Empty, assignedTo)
                .On(expectedRevisedDate)
                .Then().Closed(by: teamMember).On(DateTime.UtcNow)
                .Build();
            var request = GetRequest(updates);
            return new TestCaseData(request, Reasons, expectedRevisedDate, teamMember)
                .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "AndCorrectResolvedByIfNeeded");
        }

        private static WorkItemResolutionRequest GetRequest(IEnumerable<WorkItemUpdateViewModel> updates, string type = Constants.WorkItemTypeBug)
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
