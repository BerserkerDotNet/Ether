using System;
using System.Collections.Generic;
using System.Linq;
using Ether.Contracts.Types;
using Ether.Tests.TestData;
using Ether.ViewModels;
using Ether.Vsts;
using Ether.Vsts.Types.Classifiers;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Classifiers
{
    [TestFixture]
    public class ResolvedWorkItemsClassifierTest
    {
        private ResolvedWorkItemsClassifier _classifier;

        [SetUp]
        public void SetUp()
        {
            _classifier = new ResolvedWorkItemsClassifier();
        }

        [Test]
        public void ShouldInheritFromBaseClassifier()
        {
            var baseType = typeof(BaseWorkItemsClassifier);
            var concreteClassifier = typeof(ResolvedWorkItemsClassifier);
            baseType.IsAssignableFrom(concreteClassifier).Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfNoMatchingHistoryItems()
        {
            var workItem = new WorkItemViewModel { Fields = new Dictionary<string, string>() };
            workItem.Fields.Add(Constants.WorkItemTypeField, Constants.WorkItemTypeBug);
            workItem.Updates = UpdateBuilder.Create()
                .Activated()
                .Then().New()
                .Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfResolvedNotByTheTeam()
        {
            var workItem = new WorkItemViewModel { Fields = new Dictionary<string, string>() };
            workItem.Fields.Add(Constants.WorkItemTypeField, Constants.WorkItemTypeBug);
            workItem.Updates = UpdateBuilder.Create()
                .Resolved(new TeamMemberViewModel { DisplayName = "Not a member", Email = "not@team.com" })
                .Because("Reasons")
                .Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem, Team = ResolvedWorkItemsDataProvider.GetFakeTeam() });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfMovedToResolvedFromClosed()
        {
            var workItem = new WorkItemViewModel { Fields = new Dictionary<string, string>() };
            workItem.Fields.Add(Constants.WorkItemTypeField, Constants.WorkItemTypeBug);
            workItem.Updates = UpdateBuilder.Create().Resolved(ResolvedWorkItemsDataProvider.GetFakeTeam().ElementAt(0), from: "Closed")
                .Because("Reasons")
                .Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem, Team = ResolvedWorkItemsDataProvider.GetFakeTeam() });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfStateIsNotSet()
        {
            var workItem = new WorkItemViewModel { Fields = new Dictionary<string, string>() };
            workItem.Fields.Add(Constants.WorkItemTypeField, Constants.WorkItemTypeBug);
            workItem.Updates = UpdateBuilder.Create().Update().Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem, Team = ResolvedWorkItemsDataProvider.GetFakeTeam() });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfResolvedbyIsNotSet()
        {
            var workItem = new WorkItemViewModel { Fields = new Dictionary<string, string>() };
            workItem.Fields.Add(Constants.WorkItemTypeField, Constants.WorkItemTypeBug);
            workItem.Updates = UpdateBuilder.Create().Resolved()
                .With(UpdateBuilder.ResolvedByField, new WorkItemFieldUpdate())
                .Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem, Team = ResolvedWorkItemsDataProvider.GetFakeTeam() });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test]
        [TestCaseSource(typeof(ResolvedWorkItemsDataProvider), nameof(ResolvedWorkItemsDataProvider.GetTestCasesForResolved))]
        public void ShouldReturnResolvedResolution(WorkItemResolutionRequest request, string expectedReason, DateTime expectedResolutionDate, TeamMemberViewModel expectedTeamMember)
        {
            var result = _classifier.Classify(request);
            result.Should().NotBeNull();
            result.Resolution.Should().Be("Resolved");
            result.Reason.Should().Be(expectedReason);
            result.ResolutionDate.Should().BeCloseTo(expectedResolutionDate, precision: 1000);
            result.MemberEmail.Should().Be(expectedTeamMember.Email);
            result.MemberName.Should().Be(expectedTeamMember.DisplayName);
        }
    }
}
