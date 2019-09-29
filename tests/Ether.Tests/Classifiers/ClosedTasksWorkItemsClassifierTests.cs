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
    public class ClosedTasksWorkItemsClassifierTests
    {
        private ClosedTasksWorkItemsClassifier _classifier;

        [SetUp]
        public void SetUp()
        {
            _classifier = new ClosedTasksWorkItemsClassifier();
        }

        [Test]
        public void ShouldInheritFromBaseClassifier()
        {
            var baseType = typeof(BaseWorkItemsClassifier);
            var concreteClassifier = typeof(ClosedTasksWorkItemsClassifier);
            baseType.IsAssignableFrom(concreteClassifier).Should().BeTrue();
        }

        [Test]
        public void ShouldReturnNoneIfNoMatchingHistoryItems()
        {
            var workItem = new WorkItemViewModel { Fields = new Dictionary<string, string>() };
            workItem.Fields.Add(Constants.WorkItemTypeField, Constants.WorkItemTypeTask);
            workItem.Updates = UpdateBuilder.Create()
                .New()
                .Then().Activated()
                .Then().Resolved()
                .Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.Should().OnlyContain(r => r.IsNone);
        }

        [Test]
        [TestCaseSource(typeof(ClosedTasksTestsProvider), nameof(ClosedTasksTestsProvider.GetTestCasesForNotClosedTasks))]
        public void ShouldReturnNoneResolution(WorkItemResolutionRequest request)
        {
            var result = _classifier.Classify(request);
            result.Should().NotBeNull();
            result.Should().OnlyContain(r => r.IsNone);
        }

        [Test]
        [TestCaseSource(typeof(ClosedTasksTestsProvider), nameof(ClosedTasksTestsProvider.GetTestCasesForClosedTasks))]
        public void ShouldReturnClosedResolution(WorkItemResolutionRequest request, DateTime expectedResolutionDate, TeamMemberViewModel expectedTeamMember)
        {
            var result = _classifier.Classify(request);
            result.Should().NotBeNull();
            result.Resolution.Should().Be("Closed");
            result.Reason.Should().Be("Fixed");
            result.ResolutionDate.Should().BeCloseTo(expectedResolutionDate, 1000);
            result.MemberEmail.Should().Be(expectedTeamMember.Email);
            result.MemberName.Should().Be(expectedTeamMember.DisplayName);
        }
    }
}
