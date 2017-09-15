using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using Ether.Core.Reporters.Classifiers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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
                    UpdateBuilder.GetActivated(revisedOn: DateTime.MinValue),
                    UpdateBuilder.GetNew(revisedOn: DateTime.MinValue)
                }
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfResolvedNotByTheTeam()
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest
            {
                WorkItemType = "Bug",
                Team = ResolvedTestsDataProvider.FakeTeam,
                WorkItemUpdates = new[]
                {
                    UpdateBuilder.Resolved(new TeamMember{DisplayName="Not a member", Email="not@team.com" })
                        .Because("Reasons")
                        .Build()
                }
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfMovedToResolvedFromClosed()
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest
            {
                WorkItemType = "Bug",
                Team = ResolvedTestsDataProvider.FakeTeam,
                WorkItemUpdates = new[]
                {
                    UpdateBuilder.Resolved(ResolvedTestsDataProvider.FakeTeam.ElementAt(0), from: "Closed")
                        .Because("Reasons")
                        .Build()
                }
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfStateIsNotSet()
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest
            {
                WorkItemType = "Bug",
                Team = ResolvedTestsDataProvider.FakeTeam,
                WorkItemUpdates = new[]
                {
                    UpdateBuilder.Update().Build()
                }
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfResolvedbyIsNotSet()
        {
            var result = _classifier.Classify(new WorkItemResolutionRequest
            {
                WorkItemType = "Bug",
                Team = ResolvedTestsDataProvider.FakeTeam,
                WorkItemUpdates = new[]
                {
                    UpdateBuilder.Resolved()
                        .With(UpdateBuilder.ResolvedByField, new WorkItemUpdate.UpdateValue())
                        .Build()
                }
            });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test, TestCaseSource(typeof(ResolvedTestsDataProvider), nameof(ResolvedTestsDataProvider.GetTestCasesForResolved))]
        public void ShouldReturnResolvedResolution(WorkItemResolutionRequest request, string expectedReason, DateTime expectedResolutionDate, string expectedTeamMember)
        {
            var result = _classifier.Classify(request);
            result.Should().NotBeNull();
            result.Resolution.Should().Be("Resolved");
            result.Reason.Should().Be(expectedReason);
            result.ResolutionDate.Should().BeCloseTo(expectedResolutionDate);
            result.TeamMember.Should().Be(expectedTeamMember);
        }

        public static class ResolvedTestsDataProvider
        {
            public static IEnumerable GetTestCasesForResolved()
            {
                return new[] { GetSimpleCannotReproduce(FakeTeam) };
            }

            public static IEnumerable<TeamMember> FakeTeam => Enumerable.Range(1, 3)
                .Select(i => new TeamMember { TeamName = $"Member {i}", Email = $"member{i}@foo.com" })
                .ToList();

            private static TestCaseData GetSimpleCannotReproduce(IEnumerable<TeamMember> team)
            {
                const string CannotReproduce = "Cannot Reproduce";
                var expectedTeamMember = $"{team.ElementAt(0).DisplayName} <{team.ElementAt(0).Email}>";
                var revisedDate = DateTime.UtcNow.AddDays(-4);
                var request = new WorkItemResolutionRequest
                {
                    WorkItemId = 0,
                    WorkItemType = "Bug",
                    Team = team,
                    WorkItemUpdates = new[]
                    {
                        UpdateBuilder.Resolved(team.ElementAt(0))
                            .Because(CannotReproduce)
                            .Build(revisedDate)
                    }
                };
                return new TestCaseData(request, CannotReproduce, revisedDate, expectedTeamMember)
                    .SetName(nameof(ResolvedWorkItemsClassifierTest.ShouldReturnResolvedResolution) + "OnSimpleCannotReproduce");
            }
        }
    }
}
