using Ether.Core.Constants;
using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.VSTS;
using Ether.Core.Reporters.Classifiers;
using Ether.Tests.Data;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Ether.Tests.Classifiers
{
    [TestFixture]
    public class ClosedTasksWorkItemsClassifierTests
    {
        ClosedTasksWorkItemsClassifier _classifier;

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
            var workItem = new VSTSWorkItem { Fields = new Dictionary<string, string>() };
            workItem.Fields.Add(VSTSFieldNames.WorkItemType, WorkItemTypes.Task);
            workItem.Updates = UpdateBuilder.Create()
                .New()
                .Then().Activated()
                .Then().Resolved()
                .Build();

            var result = _classifier.Classify(new WorkItemResolutionRequest { WorkItem = workItem });

            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test, TestCaseSource(typeof(ClosedTasksTestsProvider), nameof(ClosedTasksTestsProvider.GetTestCasesForNotClosedTasks))]
        public void ShouldReturnNoneResolution(WorkItemResolutionRequest request)
        {
            var result = _classifier.Classify(request);
            result.Should().NotBeNull();
            result.IsNone.Should().BeTrue();
        }

        [Test, TestCaseSource(typeof(ClosedTasksTestsProvider), nameof(ClosedTasksTestsProvider.GetTestCasesForClosedTasks))]
        public void ShouldReturnClosedResolution(WorkItemResolutionRequest request, DateTime expectedResolutionDate, TeamMember expectedTeamMember)
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
