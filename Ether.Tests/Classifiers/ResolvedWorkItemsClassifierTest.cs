using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using Ether.Core.Reporters.Classifiers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;

namespace Ether.Tests.Classifiers
{
    [TestFixture]
    public class ResolvedWorkItemsClassifierTest
    {
        ResolvedWorkItemsClassifier _classifier;

        [SetUp]
        public void SetUp()
        {
            _classifier = new ResolvedWorkItemsClassifier();
        }

        [Test]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            _classifier.Invoking(c => c.Classify(null))
                .ShouldThrow<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("Foo")]
        [TestCase("fasfasf")]
        [TestCase("231")]
        public void ShouldReturnNoneIfUnexpectedOrEmptyType(string type)
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest
            {
                WorkItemType = type
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfEmptyHistoryItems()
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest
            {
                WorkItemType = "Bug",
                WorkItemUpdates = Enumerable.Empty<WorkItemUpdate>()
                
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfNoMatchingHistoryItems()
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest
            {
                WorkItemType = "Bug",
                WorkItemUpdates = new[] 
                {
                    GetUpdate("Active"),
                    GetUpdate("New")
                }
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test, TestCaseSource(nameof(GetTestCasesForResolved))]
        public void ShouldReturnResolvedResolution(WorkItemResolutionRequest request, string expectedReason, DateTime expectedResolutionDate, string expectedTeamMember)
        {
            var result = _classifier.Classify(request);
            result.Should().NotBeNull();
            result.Resolution.Should().Be("Resolved");
            result.Reason.Should().Be(expectedReason);
            result.ResolutionDate.Should().BeCloseTo(expectedResolutionDate);
            result.TeamMember.Should().Be(expectedTeamMember);
        }

        private static IEnumerable GetTestCasesForResolved()
        {
            const string CannotReproduce = "Cannot Reproduce";
            var team = Enumerable.Range(1, 3).Select(i => new TeamMember { TeamName = $"Member {i}", Email = $"member{i}@foo.com" });

            var simpleCannotReproduce = new TestCaseData(new WorkItemResolutionRequest
            {
                WorkItemId = 0,
                WorkItemType = "Bug",
                Team = team,
                WorkItemUpdates = new[] { new WorkItemUpdate
                {
                    Fields = new System.Collections.Generic.Dictionary<string, WorkItemUpdate.UpdateValue> {
                        { "System.State", new WorkItemUpdate.UpdateValue {NewValue="Resolved", OldValue = "New" } },
                        { "Microsoft.VSTS.Common.ResolvedBy", new WorkItemUpdate.UpdateValue { NewValue = $"{team.ElementAt(0).DisplayName} <{team.ElementAt(0).Email}>", OldValue = "" } },
                        { "System.Reason", new WorkItemUpdate.UpdateValue {NewValue = CannotReproduce, OldValue = "" } }
                    },
                    RevisedDate = DateTime.UtcNow.AddDays(-4) } }
            }, CannotReproduce, DateTime.UtcNow.AddDays(-4), $"{team.ElementAt(0).DisplayName} <{team.ElementAt(0).Email}>")
            .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnSimpleCannotReproduce");


            return new [] { simpleCannotReproduce };
        }

        private static WorkItemUpdate GetUpdate(string newState, string oldState = "New", string reason = "None", TeamMember resolvedBy = null)
        {
            resolvedBy = resolvedBy ?? new TeamMember { DisplayName = "Foo", Email = "Foo@bar.com" };
            var teamMemberString = $"{resolvedBy.DisplayName} <{resolvedBy.Email}>";
            return new WorkItemUpdate
            {
                Fields = new System.Collections.Generic.Dictionary<string, WorkItemUpdate.UpdateValue> {
                        { "System.State", new WorkItemUpdate.UpdateValue {NewValue = newState, OldValue = oldState } },
                        { "Microsoft.VSTS.Common.ResolvedBy", new WorkItemUpdate.UpdateValue { NewValue = teamMemberString, OldValue = "" } },
                        { "System.Reason", new WorkItemUpdate.UpdateValue {NewValue = reason, OldValue = "" } }
                    },
                RevisedDate = DateTime.UtcNow.AddDays(-4)
            };
        }
    }
}
